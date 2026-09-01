#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D21 纯逻辑汇合门禁运行器（D21-L30）。

运行器只执行离线 Python 审计、纯 C# 编译/测试和结果证据完整性检查。
它不会启动 Unity、Player、PlayMode、场景、Prefab、真实输入或运行时性能采样。

默认检查 L01-L29，因为 L30 的结果文件由本运行器的输出组成；当 L30-L32
结果与 handoff 已经先写入后，可使用 ``--check-all`` 对完整 32 项证据做最终
收敛核验。
"""

from __future__ import annotations

import argparse
import json
import re
import shlex
import subprocess
import sys
import time
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any, Dict, Iterable, List, Mapping, Optional, Sequence, Tuple


ROOT = Path(__file__).resolve().parents[1]
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D21"
HANDOFF_ROOT = ROOT / "docs" / "game" / "design" / "common" / "handoffs" / "D21"
PROTOCOL_FIELDS = {
    "task_id",
    "status",
    "updated_at",
    "summary",
    "changed_files",
    "verification",
    "runtime_deferred",
    "blockers",
    "handoff",
}
MARKER_PATTERN = re.compile(r"(?m)^([A-Z][A-Z0-9_]*)=(\d+)\s*$")


def now_iso() -> str:
    """Return a local +08:00 timestamp used by machine evidence."""

    return datetime.now(timezone(timedelta(hours=8))).isoformat(timespec="seconds")


def command_text(command: Sequence[str]) -> str:
    """Render a reproducible command without changing the executed argv."""

    return shlex.join([str(part) for part in command])


def relative_path(path: Path) -> str:
    return path.resolve().relative_to(ROOT).as_posix()


def extract_markers(output: str) -> Dict[str, int]:
    return {name: int(value) for name, value in MARKER_PATTERN.findall(output)}


def run_command(
    command: Sequence[str],
    *,
    suite_id: str,
    name: str,
    timeout: int,
    expected_markers: Optional[Mapping[str, int]] = None,
) -> Dict[str, Any]:
    """Run one suite and retain enough output to audit a failure."""

    started = time.monotonic()
    rendered = command_text(command)
    record: Dict[str, Any] = {
        "id": suite_id,
        "name": name,
        "command": rendered,
        "exit_code": -1,
        "duration_ms": 0,
        "status": "FAIL",
        "stdout_tail": "",
        "stderr_tail": "",
    }
    stdout = ""
    stderr = ""
    try:
        completed = subprocess.run(
            list(command),
            cwd=str(ROOT),
            text=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            timeout=timeout,
            check=False,
        )
        record["exit_code"] = completed.returncode
        stdout = completed.stdout or ""
        stderr = completed.stderr or ""
        record["status"] = "PASS" if completed.returncode == 0 else "FAIL"
    except subprocess.TimeoutExpired as error:
        stdout = error.stdout or ""
        stderr = error.stderr or ""
        record["status"] = "TIMEOUT"
        record["error_message"] = "execution timed out after {} seconds".format(timeout)
    except OSError as error:
        record["status"] = "ERROR"
        record["error_message"] = str(error)
    except Exception as error:  # pragma: no cover - defensive process boundary
        record["status"] = "ERROR"
        record["error_message"] = str(error)

    combined = "{}\n{}".format(stdout, stderr)
    record["stdout_tail"] = stdout.strip()[-6000:]
    record["stderr_tail"] = stderr.strip()[-6000:]
    if expected_markers is not None:
        observed = extract_markers(combined)
        mismatches = {
            key: {"expected": value, "observed": observed.get(key)}
            for key, value in expected_markers.items()
            if observed.get(key) != value
        }
        record["expected_markers"] = dict(expected_markers)
        record["observed_markers"] = {
            key: observed.get(key) for key in expected_markers
        }
        record["marker_status"] = "PASS" if not mismatches else "FAIL"
        if mismatches and record["status"] == "PASS":
            record["status"] = "FAIL"
            record["error_message"] = "output marker mismatch: {}".format(
                json.dumps(mismatches, ensure_ascii=False, sort_keys=True)
            )

    record["duration_ms"] = int((time.monotonic() - started) * 1000)
    return record


def result_path(task_number: int) -> Path:
    task_id = "D21-L{:02d}".format(task_number)
    return RESULT_ROOT / task_id / "D21_L{:02d}_LOGIC_RESULT.json".format(task_number)


def handoff_path(task_number: int) -> Path:
    task_id = "D21-L{:02d}".format(task_number)
    return HANDOFF_ROOT / "{}_LOGIC_HANDOFF.md".format(task_id)


def all_final_artifacts_exist() -> bool:
    return all(
        result_path(number).exists() and handoff_path(number).exists()
        for number in range(30, 33)
    )


def evidence_check(last_task: int) -> Dict[str, Any]:
    """Validate protocol fields, status and handoff presence for D21 results."""

    missing: List[str] = []
    bad_status: List[Dict[str, Any]] = []
    invalid_json: List[str] = []
    invalid_shape: List[Dict[str, Any]] = []
    result_count = 0
    handoff_count = 0

    for number in range(1, last_task + 1):
        task_id = "D21-L{:02d}".format(number)
        result = result_path(number)
        handoff = handoff_path(number)
        if not result.exists():
            missing.append(relative_path(result))
            continue
        result_count += 1
        if not handoff.exists():
            missing.append(relative_path(handoff))
        else:
            handoff_count += 1

        try:
            payload = json.loads(result.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as error:
            invalid_json.append("{}: {}".format(relative_path(result), error))
            continue

        if not isinstance(payload, dict):
            invalid_shape.append(
                {"task_id": task_id, "reason": "result root is not an object"}
            )
            continue

        missing_fields = sorted(PROTOCOL_FIELDS.difference(payload))
        if missing_fields:
            invalid_shape.append(
                {
                    "task_id": task_id,
                    "reason": "missing protocol fields",
                    "fields": missing_fields,
                }
            )
        if payload.get("task_id") != task_id or payload.get("status") != "LOGIC_PASS":
            bad_status.append(
                {
                    "task_id": task_id,
                    "reported_task_id": payload.get("task_id"),
                    "status": payload.get("status"),
                }
            )
        if not isinstance(payload.get("verification"), list) or not payload.get(
            "verification"
        ):
            invalid_shape.append(
                {"task_id": task_id, "reason": "verification must be non-empty list"}
            )
        if not isinstance(payload.get("runtime_deferred"), list):
            invalid_shape.append(
                {"task_id": task_id, "reason": "runtime_deferred must be a list"}
            )
        if not isinstance(payload.get("blockers"), list):
            invalid_shape.append(
                {"task_id": task_id, "reason": "blockers must be a list"}
            )

        declared_handoff = payload.get("handoff")
        if isinstance(declared_handoff, dict):
            declared_handoff = declared_handoff.get("document")
        expected_handoff = relative_path(handoff)
        if declared_handoff != expected_handoff:
            invalid_shape.append(
                {
                    "task_id": task_id,
                    "reason": "handoff pointer mismatch",
                    "expected": expected_handoff,
                    "observed": declared_handoff,
                }
            )

    issues = missing + invalid_json
    return {
        "status": "PASS"
        if not issues and not bad_status and not invalid_shape
        else "FAIL",
        "expected_results": last_task,
        "present_results": result_count,
        "present_handoffs": handoff_count,
        "missing": missing,
        "bad_status": bad_status,
        "invalid_json": invalid_json,
        "invalid_shape": invalid_shape,
    }


def build_suite_definitions() -> List[Dict[str, Any]]:
    python = sys.executable
    return [
        {
            "id": "D21-RUNNER-PYTHON-COMPILE",
            "name": "D21 runner and audit Python syntax",
            "command": [
                python,
                "-m",
                "py_compile",
                "tools/d21_logic_test_runner.py",
                "tools/d21_logic_boundary_audit.py",
                "tools/tests/test_d21_logic_boundary_audit.py",
            ],
            "timeout": 30,
        },
        {
            "id": "D21-AUDIT-SELF-TEST",
            "name": "D21 logic boundary audit self-test",
            "command": [python, "tools/d21_logic_boundary_audit.py", "--self-test"],
            "timeout": 30,
        },
        {
            "id": "D21-AUDIT-UNIT-TESTS",
            "name": "D21 logic boundary audit unit tests",
            "command": [
                python,
                "-m",
                "unittest",
                "tools/tests/test_d21_logic_boundary_audit.py",
            ],
            "timeout": 60,
        },
        {
            "id": "D21-BOUNDARY-AUDIT",
            "name": "D21 logic boundary static audit",
            "command": [python, "tools/d21_logic_boundary_audit.py"],
            "timeout": 30,
        },
        {
            "id": "D21-INTEGRATION-COMPILE",
            "name": "D21 pure C# integration compile",
            "command": [
                "dotnet",
                "build",
                ".codex-temp/d21-compile/D21IntegrationCompile.csproj",
                "--no-restore",
                "--nologo",
                "-warnaserror",
            ],
            "timeout": 180,
        },
        {
            "id": "D21-INTEGRATION-TEST-BUILD",
            "name": "D21 integration test compile",
            "command": [
                "dotnet",
                "build",
                ".codex-temp/d21-tests/D21IntegrationTests.csproj",
                "--no-restore",
                "--nologo",
                "-warnaserror",
            ],
            "timeout": 180,
        },
        {
            "id": "D21-INTEGRATION-TEST-RUN",
            "name": "D21 integration 11-case pure logic run",
            "command": [
                "dotnet",
                "run",
                "--project",
                ".codex-temp/d21-tests/D21IntegrationTests.csproj",
                "--no-build",
                "--no-restore",
            ],
            "timeout": 240,
            "expected_markers": {
                "D21_INTEGRATION_BASE_CASES": 8,
                "D21_CREW_PROJECTION_ASSERTIONS": 16,
                "D21_INTEGRATION_CASES": 11,
                "D21_INTEGRATION_FAILURES": 0,
            },
        },
        {
            "id": "D21-CREW-PROJECTION-BUILD",
            "name": "D20 to D21 crew projection pure C# compile",
            "command": [
                "dotnet",
                "build",
                ".codex-temp/crew-projection-tests/CrewProjectionTests.csproj",
                "--no-restore",
                "--nologo",
                "-warnaserror",
            ],
            "timeout": 180,
        },
        {
            "id": "D21-CREW-PROJECTION-RUN",
            "name": "D20/D21 crew projection 14+16 assertion run",
            "command": [
                "dotnet",
                "run",
                "--project",
                ".codex-temp/crew-projection-tests/CrewProjectionTests.csproj",
                "--no-build",
                "--no-restore",
            ],
            "timeout": 180,
            "expected_markers": {
                "D20_CREW_PROJECTION_ASSERTIONS": 14,
                "D21_CREW_PROJECTION_ASSERTIONS": 16,
                "CREW_PROJECTION_FAILURES": 0,
            },
        },
    ]


def run_all(expected_task_count: int) -> Dict[str, Any]:
    started = time.monotonic()
    suites: List[Dict[str, Any]] = []
    for definition in build_suite_definitions():
        result = run_command(
            definition["command"],
            suite_id=definition["id"],
            name=definition["name"],
            timeout=definition["timeout"],
            expected_markers=definition.get("expected_markers"),
        )
        suites.append(result)

    evidence = evidence_check(expected_task_count)
    failed_suites = [suite for suite in suites if suite["status"] != "PASS"]
    return {
        "runner_name": "D21 Unified Pure Logic Test Runner",
        "timestamp": now_iso(),
        "status": "PASS" if not failed_suites and evidence["status"] == "PASS" else "FAIL",
        "scope": {
            "expected_logic_tasks": "D21-L01..L{:02d}".format(expected_task_count),
            "unity_player_playmode_executed": False,
            "status_sync_executed": False,
        },
        "suite_count": len(suites),
        "passed_suites": len(suites) - len(failed_suites),
        "failed_suites": len(failed_suites),
        "suites": suites,
        "evidence": evidence,
        "runtime_deferred": [
            "Unity Player/PlayMode、场景、Prefab、真实驾驶、输入、物理、UI/动画、设备、性能和发行验证未执行。",
            "D19 港口真实生命周期与 D20/Crew 运行时 owner 接线不由离线 runner 代替。",
            "真实 SaveSystem 介质、崩溃恢复、迁移和云同步仍需 Runtime/发布阶段验证。",
        ],
        "not_run_by_scope": [
            "python tools/sync_logic_task_status.py（按委派要求不修改共享状态台账）",
        ],
        "duration_ms": int((time.monotonic() - started) * 1000),
    }


def resolve_output(path: Path) -> Path:
    return path if path.is_absolute() else ROOT / path


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--output",
        type=Path,
        help="write the machine-readable runner report to this path",
    )
    parser.add_argument(
        "--check-all",
        action="store_true",
        help="require D21-L01..L32 result and handoff evidence",
    )
    parser.add_argument(
        "--expected-task-count",
        type=int,
        choices=(29, 32),
        help="override evidence scope; defaults to 32 when final artifacts exist, otherwise 29",
    )
    args = parser.parse_args(argv)

    if args.check_all and args.expected_task_count == 29:
        parser.error("--check-all conflicts with --expected-task-count 29")
    expected_task_count = args.expected_task_count
    if expected_task_count is None:
        expected_task_count = 32 if args.check_all or all_final_artifacts_exist() else 29

    report = run_all(expected_task_count)
    serialized = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    print(serialized, end="")
    if args.output:
        output = resolve_output(args.output)
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(serialized, encoding="utf-8")
    return 0 if report["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
