#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L25：D18—D23 纯逻辑回归总入口。

本 runner 只汇总可离线重复执行的证据：D18—D23 结果/交接协议、D24
静态审计、纯 C# 测试、AgentCore 独立 build、D23 26-case 场景和存档
迁移审计。所有子进程都在仓库内运行；D23 场景报告写入临时目录，不
刷新旧章节结果。Unity Player/PlayMode、场景/Prefab/UI/Input、设备、
性能、网络和 LLM 永远不在本门禁内。
"""

from __future__ import annotations

import argparse
import hashlib
import html
import json
import re
import shlex
import subprocess
import sys
import tempfile
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any, Dict, Iterable, List, Mapping, Optional, Sequence


ROOT = Path(__file__).resolve().parents[1]
PROTOCOL_FIELDS = (
    "task_id",
    "status",
    "updated_at",
    "summary",
    "changed_files",
    "verification",
    "runtime_deferred",
    "blockers",
    "handoff",
)
EXPECTED_COUNTS = {
    "D18": 53,
    "D19": 48,
    "D20": 40,
    "D21": 32,
    "D22": 36,
    "D23": 38,
}
AUDIT_SCRIPTS = (
    "d24_accessibility_logic_audit.py",
    "d24_asset_provenance_audit.py",
    "d24_content_catalog_coverage_audit.py",
    "d24_content_id_uniqueness_audit.py",
    "d24_contract_compatibility_audit.py",
    "d24_currency_invariant_audit.py",
    "d24_data_ownership_audit.py",
    "d24_deterministic_seed_audit.py",
    "d24_error_catalog_audit.py",
    "d24_event_integrity_audit.py",
    "d24_historical_debt_audit.py",
    "d24_inventory_invariant_audit.py",
    "d24_localization_key_audit.py",
    "d24_save_migration_audit.py",
    "d24_test_matrix_audit.py",
)
RUNTIME_DEFERRED = [
    "Unity Player/PlayMode、场景、Prefab、Canvas/UI、真实输入、物理、视觉、音频和相机未执行。",
    "真实设备、分辨率布局、性能采样、网络服务、LLM/Agent runtime、真实 SaveSystem 介质和发行验证未执行。",
    "LOGIC_PASS/logic candidate 不等于 playable、RuntimeApproved 或 ReleaseApproved。",
]
D23_MARKER = re.compile(r"(?m)^D23_(CASES|ASSERTIONS|FAILURES)=(-?\d+)\s*$")
D24_MARKER = re.compile(r"(?m)^(D24_ASSERTIONS)=(-?\d+)\s*$")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Release")
    parser.add_argument("--json-out", type=Path, default=None)
    parser.add_argument(
        "--include-d24-l27",
        action="store_true",
        help="also compile/run D24FullLogicRegressionTests.cs; useful for final candidate acceptance",
    )
    parser.add_argument("--self-test", action="store_true")
    return parser.parse_args(argv)


def now_iso() -> str:
    return datetime.now(timezone(timedelta(hours=8))).isoformat(timespec="seconds")


def relative(root: Path, path: Path) -> str:
    try:
        return path.resolve().relative_to(root.resolve()).as_posix()
    except ValueError:
        return str(path.resolve()).replace("\\", "/")


def command_text(command: Sequence[str]) -> str:
    return shlex.join([str(part) for part in command])


def tail(value: Any, limit: int = 4000) -> str:
    if value is None:
        return ""
    return str(value).strip()[-limit:]


def task_result_path(root: Path, task_id: str) -> Path:
    chapter = task_id.split("-", 1)[0]
    return root / "game" / "tools" / "results" / chapter / task_id / (task_id.replace("-", "_") + "_LOGIC_RESULT.json")


def task_handoff_path(root: Path, task_id: str) -> Path:
    chapter = task_id.split("-", 1)[0]
    return root / "docs" / "game" / "design" / "common" / "handoffs" / chapter / (task_id + "_LOGIC_HANDOFF.md")


def load_json(path: Path) -> tuple[Optional[Dict[str, Any]], str]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        return None, str(error)
    if not isinstance(value, dict):
        return None, "top-level JSON value is not an object"
    return value, ""


def sha256_paths(paths: Iterable[Path]) -> str:
    digest = hashlib.sha256()
    for path in sorted(paths, key=lambda item: item.as_posix()):
        digest.update(path.as_posix().encode("utf-8"))
        digest.update(b"\0")
        try:
            digest.update(path.read_bytes())
        except OSError:
            digest.update(b"<missing>")
        digest.update(b"\0")
    return digest.hexdigest()


def protocol_snapshot(root: Path) -> Dict[str, Any]:
    issues: List[str] = []
    compatibility_findings: List[str] = []
    entries: Dict[str, Dict[str, Any]] = {}
    result_paths: List[Path] = []
    handoff_paths: List[Path] = []
    for chapter, count in EXPECTED_COUNTS.items():
        chapter_entry = {"expected": count, "results": 0, "handoffs": 0, "logic_pass": 0}
        for number in range(1, count + 1):
            task_id = "{}-L{:02d}".format(chapter, number)
            result = task_result_path(root, task_id)
            handoff = task_handoff_path(root, task_id)
            result_paths.append(result)
            handoff_paths.append(handoff)
            task_issues: List[str] = []
            payload, error = load_json(result)
            if payload is None:
                task_issues.append("result: " + (error or "missing"))
            else:
                chapter_entry["results"] += 1
                missing_fields = [field for field in PROTOCOL_FIELDS if field not in payload]
                if missing_fields:
                    task_issues.append("result missing protocol fields: " + ",".join(missing_fields))
                extra_fields = [field for field in payload if field not in PROTOCOL_FIELDS]
                if extra_fields:
                    compatibility_findings.append(
                        "{}: legacy/extended result fields accepted: {}".format(task_id, ",".join(extra_fields))
                    )
                if payload.get("task_id") != task_id:
                    task_issues.append("result task_id mismatch")
                if payload.get("status") != "LOGIC_PASS":
                    task_issues.append("result status is {}".format(payload.get("status")))
                else:
                    chapter_entry["logic_pass"] += 1
                expected_handoff = relative(root, handoff)
                handoff_value = payload.get("handoff")
                if isinstance(handoff_value, str):
                    handoff_pointer_text = handoff_value.replace("\\", "/")
                elif isinstance(handoff_value, dict):
                    handoff_pointer_text = json.dumps(handoff_value, ensure_ascii=False)
                else:
                    handoff_pointer_text = ""
                if expected_handoff not in handoff_pointer_text and handoff.name not in handoff_pointer_text:
                    compatibility_findings.append("{}: result uses a legacy handoff pointer shape".format(task_id))
            if handoff.exists():
                chapter_entry["handoffs"] += 1
                handoff_text = handoff.read_text(encoding="utf-8", errors="replace")
                if task_id not in handoff_text:
                    task_issues.append("handoff task_id missing")
                if "LOGIC_PASS" not in handoff_text:
                    compatibility_findings.append("{}: handoff omits an explicit LOGIC_PASS marker".format(task_id))
                if relative(root, result) not in handoff_text.replace("\\", "/"):
                    compatibility_findings.append("{}: handoff omits an explicit result path".format(task_id))
            else:
                task_issues.append("handoff missing")
            if task_issues:
                issues.extend([task_id + ":" + item for item in task_issues])
            entries[task_id] = {
                "status": payload.get("status") if payload else None,
                "result": relative(root, result),
                "handoff": relative(root, handoff),
                "issues": task_issues,
            }
        entries[chapter] = chapter_entry
    return {
        "status": "PASS" if not issues else "FAIL",
        "expected_tasks": sum(EXPECTED_COUNTS.values()),
        "result_count": sum(int(entries[chapter]["results"]) for chapter in EXPECTED_COUNTS),
        "handoff_count": sum(int(entries[chapter]["handoffs"]) for chapter in EXPECTED_COUNTS),
        "logic_pass_count": sum(int(entries[chapter]["logic_pass"]) for chapter in EXPECTED_COUNTS),
        "chapter_counts": {chapter: entries[chapter] for chapter in EXPECTED_COUNTS},
        "issues": issues,
        "compatibility_findings": compatibility_findings,
        "input_digest": sha256_paths(result_paths + handoff_paths),
        "entries": {task: value for task, value in entries.items() if "-L" in task},
    }


def run_process(command: Sequence[str], *, cwd: Path, timeout: int = 300) -> Dict[str, Any]:
    record: Dict[str, Any] = {
        "command": command_text(command),
        "exit_code": -1,
        "status": "ERROR",
    }
    try:
        completed = subprocess.run(
            list(command),
            cwd=str(cwd),
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            timeout=timeout,
            check=False,
        )
    except subprocess.TimeoutExpired as error:
        record["status"] = "TIMEOUT"
        record["error"] = "timeout after {} seconds".format(timeout)
        record["stdout_tail"] = tail(error.stdout)
        record["stderr_tail"] = tail(error.stderr)
        return record
    except OSError as error:
        record["status"] = "ERROR"
        record["error"] = str(error)
        return record
    record["exit_code"] = completed.returncode
    record["status"] = "PASS" if completed.returncode == 0 else "FAIL"
    record["stdout_tail"] = tail(completed.stdout)
    record["stderr_tail"] = tail(completed.stderr)
    return record


def run_json_script(
    root: Path,
    script_name: str,
    output: Path,
    extra: Sequence[str] = (),
    timeout: int = 300,
) -> Dict[str, Any]:
    script = root / "tools" / script_name
    command = [sys.executable, str(script), "--root", str(root), *extra, "--json-out", str(output)]
    process = run_process(command, cwd=root, timeout=timeout)
    payload, error = load_json(output)
    if payload is None:
        try:
            parsed = json.loads(process.get("stdout_tail", ""))
            payload = parsed if isinstance(parsed, dict) else None
        except json.JSONDecodeError:
            payload = None
    pass_value = bool(
        process.get("exit_code") == 0
        and isinstance(payload, dict)
        and (payload.get("passed") is True or payload.get("status") == "PASS")
    )
    result: Dict[str, Any] = {
        "id": script_name,
        "command": process["command"],
        "exit_code": process.get("exit_code", -1),
        "status": "PASS" if pass_value else "FAIL",
        "payload_status": payload.get("status") if payload else None,
        "payload_passed": payload.get("passed") if payload else None,
        "json_valid": payload is not None,
    }
    if payload is not None:
        result["payload_summary"] = summarize_payload(payload)
    if error and payload is None:
        result["parse_error"] = error
    if not pass_value:
        result["stdout_tail"] = process.get("stdout_tail", "")
        result["stderr_tail"] = process.get("stderr_tail", "")
    return result


def summarize_payload(payload: Mapping[str, Any]) -> Dict[str, Any]:
    """保留基线所需的稳定指标，避免把临时路径/耗时复制进总报告。"""
    summary: Dict[str, Any] = {}
    for key in (
        "schema_version",
        "suite_count",
        "passed_suites",
        "failed_suites",
        "case_count",
        "assertion_count",
        "failure_count",
        "result_count",
        "expected_results",
        "present_results",
        "present_handoffs",
        "files_scanned",
        "forbidden_hits",
        "enum_inventory",
        "enum_values",
        "web_consumer_files",
    ):
        if key in payload:
            summary[key] = payload[key]
    if isinstance(payload.get("checks"), list):
        summary["check_count"] = len(payload["checks"])
        summary["check_statuses"] = [item.get("status") for item in payload["checks"] if isinstance(item, dict)]
    if isinstance(payload.get("tasks"), list):
        summary["task_count"] = len(payload["tasks"])
        summary["task_statuses"] = [item.get("status") for item in payload["tasks"] if isinstance(item, dict)]
    return summary


def parse_markers(value: str, pattern: re.Pattern[str]) -> Dict[str, int]:
    return {name: int(number) for name, number in pattern.findall(value or "")}


def xml(value: str) -> str:
    return html.escape(value, quote=True)


def run_d24_l27(root: Path, configuration: str, temp_dir: Path) -> Dict[str, Any]:
    source = root / "game" / "Assets" / "Tests" / "EditMode" / "D24" / "D24FullLogicRegressionTests.cs"
    d23_project = root / ".codex-temp" / "d23-compile" / "D23IntegrationTests.csproj"
    d23_dll = root / ".codex-temp" / "d23-compile" / "bin" / "Debug" / "net8.0" / "D23IntegrationTests.dll"
    d22_dll = root / ".codex-temp" / "d22-compile" / "bin" / "Debug" / "net8.0" / "D22IntegrationCompile.dll"
    systems_dll = root / ".codex-temp" / "d22-compile" / "bin" / "Debug" / "net8.0" / "Whalebone.Systems.dll"
    agent_dll = root / ".codex-temp" / "d22-compile" / "bin" / "Debug" / "net8.0" / "Whalebone.Agent.Runtime.dll"
    if not source.exists():
        return {"id": "D24-L27", "status": "BLOCKED", "exit_code": 1, "error": "D24 test source missing"}
    if not d23_project.exists():
        return {"id": "D24-L27", "status": "BLOCKED", "exit_code": 1, "error": "D23 compile project missing"}
    references = [d23_dll, d22_dll, systems_dll, agent_dll]
    missing_references = [str(path) for path in references if not path.exists()]
    if missing_references:
        return {"id": "D24-L27", "status": "BLOCKED", "exit_code": 1, "error": "compiled D23 reference missing: " + ", ".join(missing_references)}
    project = temp_dir / "D24FullLogicRegression.csproj"
    program = temp_dir / "Program.cs"
    program.write_text(
        "using System;\n"
        "using Whalebone.Tests.D24;\n"
        "public static class Program\n"
        "{\n"
        "    public static int Main()\n"
        "    {\n"
        "        try\n"
        "        {\n"
        "            int assertions = D24FullLogicRegressionTests.RunAll();\n"
        "            Console.WriteLine(\"D24_ASSERTIONS=\" + assertions);\n"
        "            Console.WriteLine(\"D24_FAILURES=0\");\n"
        "            return 0;\n"
        "        }\n"
        "        catch (Exception error)\n"
        "        {\n"
        "            Console.Error.WriteLine(error);\n"
        "            return 1;\n"
        "        }\n"
        "    }\n"
        "}\n",
        encoding="utf-8",
    )
    project.write_text(
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n"
        "  <PropertyGroup>\n"
        "    <TargetFramework>net8.0</TargetFramework>\n"
        "    <LangVersion>latest</LangVersion>\n"
        "    <Nullable>disable</Nullable>\n"
        "    <ImplicitUsings>disable</ImplicitUsings>\n"
        "    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>\n"
        "    <OutputType>Exe</OutputType>\n"
        "  </PropertyGroup>\n"
        "  <ItemGroup>\n"
        + "\n".join(
            "    <Reference Include=\"" + xml(path.stem) + "\"><HintPath>" + xml(str(path.resolve())) + "</HintPath><Private>true</Private></Reference>"
            for path in references
        )
        + "\n"
        "    <Compile Include=\"" + xml(str(source.resolve())) + "\" />\n"
        "    <Compile Include=\"" + xml(str(program.resolve())) + "\" />\n"
        "  </ItemGroup>\n"
        "</Project>\n",
        encoding="utf-8",
    )
    build = run_process(
        ["dotnet", "build", str(project), "--configuration", configuration, "--nologo", "-warnaserror"],
        cwd=root,
        timeout=300,
    )
    run = (
        run_process(
            ["dotnet", "run", "--project", str(project), "--configuration", configuration, "--no-build", "--no-restore", "--nologo"],
            cwd=root,
            timeout=300,
        )
        if build.get("exit_code") == 0
        else {"command": "skipped because build failed", "exit_code": -1, "status": "BLOCKED", "stdout_tail": "", "stderr_tail": ""}
    )
    combined = "\n".join([build.get("stdout_tail", ""), build.get("stderr_tail", ""), run.get("stdout_tail", ""), run.get("stderr_tail", "")])
    markers = parse_markers(combined, D24_MARKER)
    passed = bool(build.get("exit_code") == 0 and run.get("exit_code") == 0 and markers.get("D24_ASSERTIONS", 0) > 0 and "D24_FAILURES=0" in combined)
    return {
        "id": "D24-L27",
        "status": "PASS" if passed else ("BLOCKED" if build.get("status") == "BLOCKED" else "FAIL"),
        "exit_code": 0 if passed else int(run.get("exit_code", build.get("exit_code", 1))),
        "build_exit_code": build.get("exit_code"),
        "run_exit_code": run.get("exit_code"),
        "assertion_count": markers.get("D24_ASSERTIONS", 0),
        "failure_count": 0 if "D24_FAILURES=0" in combined else -1,
        "command": "dotnet build/run temporary D24FullLogicRegression project",
        "build_stdout_tail": build.get("stdout_tail", "")[-2000:],
        "run_stdout_tail": run.get("stdout_tail", "")[-4000:],
        "stderr_tail": "\n".join([build.get("stderr_tail", ""), run.get("stderr_tail", "")])[-4000:],
    }


def run_core(root: Path, configuration: str, include_d24_l27: bool) -> Dict[str, Any]:
    root = root.resolve()
    protocol = protocol_snapshot(root)
    blockers: List[str] = []
    if protocol["status"] != "PASS":
        blockers.append("D18_D23_PROTOCOL_MATRIX_FAILED")

    temp_parent = root / ".codex-temp"
    temp_parent.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(prefix="d24-regression-", dir=str(temp_parent)) as directory:
        temp_dir = Path(directory)
        suites: List[Dict[str, Any]] = []
        syntax = run_process(
            [sys.executable, "-m", "py_compile", "tools/d24_logic_regression_runner.py"],
            cwd=root,
            timeout=60,
        )
        suites.append({
            "id": "D24-PYTHON-SYNTAX",
            "status": syntax.get("status"),
            "exit_code": syntax.get("exit_code"),
            "command": syntax.get("command"),
        })

        audit_results: List[Dict[str, Any]] = []
        for index, script in enumerate(AUDIT_SCRIPTS):
            audit = run_json_script(root, script, temp_dir / ("audit-{:02d}.json".format(index)), timeout=180)
            audit_results.append(audit)
        suites.append({
            "id": "D24-AUDITS",
            "status": "PASS" if all(item["status"] == "PASS" for item in audit_results) else "FAIL",
            "exit_code": 0 if all(item["status"] == "PASS" for item in audit_results) else 1,
            "count": len(audit_results),
            "passed": sum(1 for item in audit_results if item["status"] == "PASS"),
            "children": audit_results,
        })

        csharp = run_json_script(
            root,
            "d24_logic_test_runner.py",
            temp_dir / "d24-csharp.json",
            extra=("--task", "all", "--configuration", configuration),
            timeout=420,
        )
        suites.append({**csharp, "id": "D24-CSHARP-L17-L22"})

        agentcore = run_json_script(
            root,
            "d24_agentcore_build_audit.py",
            temp_dir / "agentcore.json",
            extra=("--configuration", configuration),
            timeout=420,
        )
        suites.append({**agentcore, "id": "D24-AGENTCORE-L23"})

        d23_report = temp_dir / "d23-runner.json"
        d23_process = run_process(
            [sys.executable, str(root / "tools" / "d23_scenario_runner.py"), "--output", str(d23_report)],
            cwd=root,
            timeout=420,
        )
        d23_payload, d23_error = load_json(d23_report)
        d23_pass = bool(
            d23_process.get("exit_code") == 0
            and isinstance(d23_payload, dict)
            and d23_payload.get("pass_eligible") is True
            and d23_payload.get("status") == "PASS"
        )
        d23_suite: Dict[str, Any] = {
            "id": "D23-SCENARIO-26-CASE",
            "command": d23_process.get("command"),
            "exit_code": d23_process.get("exit_code"),
            "status": "PASS" if d23_pass else "FAIL",
            "json_valid": d23_payload is not None,
        }
        if d23_payload is not None:
            d23_suite["summary"] = summarize_payload(d23_payload)
            d23_suite["observed_markers"] = {
                key: d23_payload.get(key)
                for key in ("case_count", "assertion_count", "failure_count", "pass_eligible")
            }
        if d23_error and d23_payload is None:
            d23_suite["parse_error"] = d23_error
        if not d23_pass:
            d23_suite["stdout_tail"] = d23_process.get("stdout_tail", "")
            d23_suite["stderr_tail"] = d23_process.get("stderr_tail", "")
        suites.append(d23_suite)

        l27: Optional[Dict[str, Any]] = None
        if include_d24_l27:
            l27 = run_d24_l27(root, configuration, temp_dir)
            suites.append(l27)

    for suite in suites:
        if suite.get("status") != "PASS":
            blockers.append(str(suite.get("id")) + ":" + str(suite.get("status")))
    report = {
        "schema_version": "d24.logic-regression.v1",
        "runner_name": "D24 Unified Pure Logic Regression Runner",
        "updated_at": now_iso(),
        "status": "PASS" if not blockers else "FAIL",
        "pass_eligible": not blockers,
        "blockers": sorted(set(blockers)),
        "protocol_matrix": {
            key: value for key, value in protocol.items() if key != "entries"
        },
        "suites": suites,
        "suite_count": len(suites),
        "passed_suites": sum(1 for item in suites if item.get("status") == "PASS"),
        "failed_suites": sum(1 for item in suites if item.get("status") != "PASS"),
        "scope": {
            "unity_player_playmode_executed": False,
            "scene_prefab_canvas_input_executed": False,
            "network_llm_executed": False,
            "performance_sampling_executed": False,
        },
        "runtime_deferred": list(RUNTIME_DEFERRED),
    }
    return report


def self_test() -> Dict[str, Any]:
    checks = [
        {"id": "protocol_field_order", "passed": list(PROTOCOL_FIELDS) == [
            "task_id", "status", "updated_at", "summary", "changed_files",
            "verification", "runtime_deferred", "blockers", "handoff",
        ]},
        {"id": "task_path", "passed": task_result_path(ROOT, "D23-L38").name == "D23_L38_LOGIC_RESULT.json"},
        {"id": "marker_parser", "passed": parse_markers("D23_CASES=26\nD23_ASSERTIONS=13925\nD23_FAILURES=0\n", D23_MARKER) == {"CASES": 26, "ASSERTIONS": 13925, "FAILURES": 0}},
    ]
    return {
        "schema_version": "d24.logic-regression.v1",
        "status": "PASS" if all(item["passed"] for item in checks) else "FAIL",
        "passed": all(item["passed"] for item in checks),
        "checks": checks,
        "runtime_deferred": list(RUNTIME_DEFERRED),
    }


def emit(payload: Mapping[str, Any], output: Optional[Path], root: Path) -> int:
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if output is not None:
        output_path = output if output.is_absolute() else root / output
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload.get("status") == "PASS" or payload.get("passed") is True else 1


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    root = args.root.expanduser().resolve()
    payload = self_test() if args.self_test else run_core(root, args.configuration, args.include_d24_l27)
    return emit(payload, args.json_out, root)


if __name__ == "__main__":
    raise SystemExit(main())
