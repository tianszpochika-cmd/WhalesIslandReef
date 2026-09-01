#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D22 pure-logic convergence gate runner.

The runner performs only offline Python audits, source inventory checks, a
pure C# build/run, and machine-readable evidence checks.  It never starts
Unity, a Player, PlayMode, a scene, a Prefab, a UI, an input device, or a
performance sampler.

The convergence artifacts for D22-L33..L36 are written only after the
commands and the D22-L01..L32/D21 upstream evidence checks have completed.
Missing or invalid upstream evidence therefore produces LOGIC_BLOCKED (and a
failed process exit) instead of a false LOGIC_PASS.
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
from typing import Any, Dict, Iterable, List, Mapping, Optional, Sequence


ROOT = Path(__file__).resolve().parents[1]
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D22"
HANDOFF_ROOT = ROOT / "docs" / "game" / "design" / "common" / "handoffs" / "D22"
D21_RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D21"
D21_HANDOFF_ROOT = ROOT / "docs" / "game" / "design" / "common" / "handoffs" / "D21"
D21_ACCEPTANCE = ROOT / "docs" / "game" / "design" / "common" / "D21_LOGIC_ACCEPTANCE.md"
D21_RUNNER_REPORT = D21_RESULT_ROOT / "D21-L30" / "D21_L30_RUNNER_REPORT.json"
D22_RUNTIME_DEFERRED = ROOT / "docs" / "game" / "design" / "common" / "D22_RUNTIME_DEFERRED_REGISTER.md"
D22_STATUS_REVIEW = ROOT / "docs" / "game" / "design" / "common" / "D22_LOGIC_STATUS_REVIEW.md"
D22_ACCEPTANCE = ROOT / "docs" / "game" / "design" / "common" / "D22_LOGIC_ACCEPTANCE.md"
D22_TEMP_PROJECT = ROOT / ".codex-temp" / "d22-logic" / "D22LogicTests.csproj"
D22_TEMP_PROGRAM = ROOT / ".codex-temp" / "d22-logic" / "D22LogicTestsProgram.cs"

D22_TASK_COUNT = 36
D22_UPSTREAM_TASK_COUNT = 32
D21_TASK_COUNT = 32
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
MARKER_PATTERN = re.compile(r"(?m)^([A-Z][A-Z0-9_]*)=(-?\d+)\s*$")

EXPECTED_D22_SOURCE_NAMES = {
    "D22AccessibilityReadModel.cs",
    "D22DisabledReasonResolver.cs",
    "D22EscapeIntentSystem.cs",
    "D22FeedbackCatalog.cs",
    "D22FocusStateMachine.cs",
    "D22InventoryReadModelAdapter.cs",
    "D22LoadStateReadModel.cs",
    "D22LocalizationKeyAudit.cs",
    "D22MarketReadModelAdapter.cs",
    "D22NotificationQueue.cs",
    "D22NpcReadModelAdapter.cs",
    "D22PanelExclusivityRules.cs",
    "D22PanelTransitionSystem.cs",
    "D22PauseIntentSystem.cs",
    "D22PointerModeReadModel.cs",
    "D22PortReadModelAdapter.cs",
    "D22QuestReadModelAdapter.cs",
    "D22SaveReadModelAdapter.cs",
    "D22ScreenContracts.cs",
    "D22ShipReadModelAdapter.cs",
    "D22UiCommandContracts.cs",
    "D22UiCommandRouter.cs",
    "D22UiCommandValidator.cs",
    "D22ViewModelComposer.cs",
}
EXPECTED_D22_TEST_NAMES = {
    "D22CommandFeedbackTests.cs",
    "D22DeterministicUiScenarioTests.cs",
    "D22FocusAndPanelTests.cs",
    "D22ReadModelAdapterTests.cs",
    "D22UiSaveLoadTests.cs",
    "D22UiStateFuzzTests.cs",
}


def now_iso() -> str:
    """Return a local +08:00 timestamp for machine evidence."""

    return datetime.now(timezone(timedelta(hours=8))).isoformat(timespec="seconds")


def relative_path(path: Path) -> str:
    """Render an in-repository path with stable forward slashes."""

    return path.resolve().relative_to(ROOT).as_posix()


def command_text(command: Sequence[str]) -> str:
    """Render the exact argv without changing what is executed."""

    return shlex.join([str(part) for part in command])


def text_value(value: Any) -> str:
    """Normalize subprocess output for both text and byte exceptions."""

    if value is None:
        return ""
    if isinstance(value, bytes):
        return value.decode("utf-8", errors="replace")
    return str(value)


def extract_markers(output: str) -> Dict[str, int]:
    """Extract explicit assertion markers emitted by a child command."""

    return {name: int(value) for name, value in MARKER_PATTERN.findall(output)}


def run_command(
    command: Sequence[str],
    *,
    suite_id: str,
    name: str,
    timeout: int,
    expected_markers: Optional[Mapping[str, int]] = None,
) -> Dict[str, Any]:
    """Run one suite and retain real exit code, output tails and assertions."""

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
        stdout = text_value(completed.stdout)
        stderr = text_value(completed.stderr)
        record["status"] = "PASS" if completed.returncode == 0 else "FAIL"
    except subprocess.TimeoutExpired as error:
        stdout = text_value(error.stdout)
        stderr = text_value(error.stderr)
        record["status"] = "TIMEOUT"
        record["error_message"] = "execution timed out after {} seconds".format(timeout)
    except OSError as error:
        record["status"] = "ERROR"
        record["error_message"] = str(error)
    except Exception as error:  # pragma: no cover - defensive process boundary
        record["status"] = "ERROR"
        record["error_message"] = str(error)

    record["stdout_tail"] = stdout.strip()[-6000:]
    record["stderr_tail"] = stderr.strip()[-6000:]
    combined = "{}\n{}".format(stdout, stderr)
    observed = extract_markers(combined)
    record["observed_markers"] = observed

    if expected_markers is None:
        record["expected_markers"] = {}
        record["marker_status"] = "NOT_APPLICABLE"
        record["assertion_status"] = (
            "COMMAND_PASS" if record["status"] == "PASS" else "COMMAND_FAIL"
        )
    else:
        expected = dict(expected_markers)
        mismatches = {
            key: {"expected": value, "observed": observed.get(key)}
            for key, value in expected.items()
            if observed.get(key) != value
        }
        record["expected_markers"] = expected
        record["marker_status"] = "PASS" if not mismatches else "FAIL"
        record["assertion_status"] = record["marker_status"]
        if mismatches:
            record["marker_mismatches"] = mismatches
            if record["status"] == "PASS":
                record["status"] = "FAIL"
                record["error_message"] = "output marker mismatch: {}".format(
                    json.dumps(mismatches, ensure_ascii=False, sort_keys=True)
                )

    record["assertions"] = {
        "expected_markers": record["expected_markers"],
        "observed_markers": {
            key: observed.get(key) for key in record["expected_markers"]
        },
        "status": record["assertion_status"],
    }
    record["duration_ms"] = int((time.monotonic() - started) * 1000)
    return record


def result_path(chapter: str, task_number: int) -> Path:
    task_id = "{}-L{:02d}".format(chapter, task_number)
    root = RESULT_ROOT if chapter == "D22" else D21_RESULT_ROOT
    return root / task_id / "{}_LOGIC_RESULT.json".format(task_id.replace("-", "_"))


def handoff_path(chapter: str, task_number: int) -> Path:
    task_id = "{}-L{:02d}".format(chapter, task_number)
    root = HANDOFF_ROOT if chapter == "D22" else D21_HANDOFF_ROOT
    return root / "{}_LOGIC_HANDOFF.md".format(task_id)


def normalize_handoff_pointer(value: Any) -> Optional[str]:
    """Accept the protocol's string form and existing dict/document form."""

    if isinstance(value, str):
        return value
    if isinstance(value, dict) and isinstance(value.get("document"), str):
        return value["document"]
    return None


def protocol_issues(
    *,
    chapter: str,
    task_number: int,
    payload: Any,
    result: Path,
    handoff: Path,
) -> List[Dict[str, Any]]:
    """Validate one result and its paired handoff without trusting summaries."""

    task_id = "{}-L{:02d}".format(chapter, task_number)
    issues: List[Dict[str, Any]] = []

    def add(kind: str, details: Any) -> None:
        issues.append(
            {
                "task_id": task_id,
                "kind": kind,
                "details": details,
            }
        )

    if not isinstance(payload, dict):
        add("invalid_shape", "result root is not an object")
        return issues

    missing_fields = sorted(PROTOCOL_FIELDS.difference(payload))
    if missing_fields:
        add("missing_protocol_fields", missing_fields)

    if payload.get("task_id") != task_id:
        add(
            "task_id_mismatch",
            {"expected": task_id, "observed": payload.get("task_id")},
        )
    if payload.get("status") != "LOGIC_PASS":
        add(
            "bad_status",
            {"expected": "LOGIC_PASS", "observed": payload.get("status")},
        )
    for field in ("updated_at", "summary"):
        if not isinstance(payload.get(field), str) or not payload.get(field).strip():
            add("invalid_{}".format(field), "must be a non-empty string")
    for field in ("changed_files", "runtime_deferred", "blockers"):
        if not isinstance(payload.get(field), list):
            add("invalid_{}".format(field), "must be a list")

    verification = payload.get("verification")
    if not isinstance(verification, list) or not verification:
        add("invalid_verification", "must be a non-empty list")
    elif any(
        not isinstance(item, dict)
        or not any(isinstance(item.get(key), str) and item.get(key).strip() for key in ("command", "check"))
        for item in verification
    ):
        add("invalid_verification", "each item needs a command or check")

    expected_handoff = relative_path(handoff)
    declared_handoff = normalize_handoff_pointer(payload.get("handoff"))
    if declared_handoff != expected_handoff:
        add(
            "handoff_pointer_mismatch",
            {"expected": expected_handoff, "observed": declared_handoff},
        )

    if not handoff.exists():
        add("missing_handoff", relative_path(handoff))
    else:
        try:
            handoff_text = handoff.read_text(encoding="utf-8")
        except (OSError, UnicodeError) as error:
            add("handoff_unreadable", str(error))
        else:
            if not handoff_text.strip():
                add("empty_handoff", relative_path(handoff))
            if task_id not in handoff_text:
                add("handoff_task_id_missing", task_id)
            if chapter == "D22" and "LOGIC_PASS" not in handoff_text:
                add("handoff_status_missing", "LOGIC_PASS")
            if chapter == "D22" and relative_path(result) not in handoff_text.replace("\\", "/"):
                add("handoff_result_pointer_missing", relative_path(result))

    return issues


def evidence_check(chapter: str, task_count: int) -> Dict[str, Any]:
    """Check result/handoff presence, JSON protocol and pass status."""

    missing: List[str] = []
    invalid_json: List[Dict[str, Any]] = []
    issues: List[Dict[str, Any]] = []
    present_results = 0
    present_handoffs = 0

    for number in range(1, task_count + 1):
        result = result_path(chapter, number)
        handoff = handoff_path(chapter, number)
        if result.exists():
            present_results += 1
            try:
                payload = json.loads(result.read_text(encoding="utf-8"))
            except (OSError, UnicodeError, json.JSONDecodeError) as error:
                invalid_json.append(
                    {"path": relative_path(result), "error": str(error)}
                )
            else:
                issues.extend(
                    protocol_issues(
                        chapter=chapter,
                        task_number=number,
                        payload=payload,
                        result=result,
                        handoff=handoff,
                    )
                )
        else:
            missing.append(relative_path(result))

        if handoff.exists():
            present_handoffs += 1
        else:
            missing.append(relative_path(handoff))

    return {
        "status": "PASS" if not missing and not invalid_json and not issues else "FAIL",
        "expected_results": task_count,
        "present_results": present_results,
        "present_handoffs": present_handoffs,
        "missing": missing,
        "invalid_json": invalid_json,
        "issues": issues,
    }


def source_inventory_check() -> Dict[str, Any]:
    """Ensure the isolated project includes all D20/D21/D22 pure source sets."""

    groups = {
        "d20_sources": sorted(
            path.name
            for path in (ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D20").glob("*.cs")
        ),
        "d21_sources": sorted(
            path.name
            for path in (ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D21").glob("*.cs")
        ),
        "d22_sources": sorted(
            path.name
            for path in (ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D22").glob("*.cs")
        ),
        "d22_tests": sorted(
            path.name
            for path in (ROOT / "game" / "Assets" / "Tests" / "EditMode" / "D22").glob("*.cs")
        ),
    }
    issues: List[str] = []
    missing_d22_sources = sorted(EXPECTED_D22_SOURCE_NAMES.difference(groups["d22_sources"]))
    missing_d22_tests = sorted(EXPECTED_D22_TEST_NAMES.difference(groups["d22_tests"]))
    if not groups["d20_sources"]:
        issues.append("D20 source directory has no C# files")
    if not groups["d21_sources"]:
        issues.append("D21 source directory has no C# files")
    if missing_d22_sources:
        issues.append("missing D22 sources: " + ", ".join(missing_d22_sources))
    if missing_d22_tests:
        issues.append("missing D22 tests: " + ", ".join(missing_d22_tests))
    if not D22_TEMP_PROJECT.exists():
        issues.append(relative_path(D22_TEMP_PROJECT) + " is missing")
    if not D22_TEMP_PROGRAM.exists():
        issues.append(relative_path(D22_TEMP_PROGRAM) + " is missing")

    project_text = ""
    if D22_TEMP_PROJECT.exists():
        try:
            project_text = D22_TEMP_PROJECT.read_text(encoding="utf-8").replace("\\", "/")
        except (OSError, UnicodeError) as error:
            issues.append("cannot read isolated project: " + str(error))
    for include in (
        "Assets/Scripts/Systems/D20/*.cs",
        "Assets/Scripts/Systems/D21/*.cs",
        "Assets/Scripts/Systems/D22/*.cs",
        "Assets/Tests/EditMode/D22/*.cs",
    ):
        if include not in project_text:
            issues.append("isolated project does not include " + include)
    nunit_path = ROOT / "game" / "Library" / "PackageCache" / "com.unity.ext.nunit@031a54704bff" / "net40" / "unity-custom" / "nunit.framework.dll"
    if not nunit_path.exists():
        issues.append(relative_path(nunit_path) + " is missing")

    return {
        "status": "PASS" if not issues else "FAIL",
        "groups": groups,
        "counts": {key: len(value) for key, value in groups.items()},
        "missing_expected_d22_sources": missing_d22_sources,
        "missing_expected_d22_tests": missing_d22_tests,
        "issues": issues,
    }


def upstream_acceptance_check() -> Dict[str, Any]:
    """Audit the accepted D21 chapter without rerunning its separate runner."""

    evidence = evidence_check("D21", D21_TASK_COUNT)
    issues: List[str] = []
    acceptance_pass = False
    report_pass = False
    report_payload: Any = None

    if not D21_ACCEPTANCE.exists():
        issues.append(relative_path(D21_ACCEPTANCE) + " is missing")
    else:
        try:
            acceptance_text = D21_ACCEPTANCE.read_text(encoding="utf-8")
            acceptance_pass = bool(
                re.search(r"当前结论[^\n]*LOGIC_PASS", acceptance_text)
                or re.search(r"D21-L32[^\n]*LOGIC_PASS", acceptance_text)
            )
            if not acceptance_pass:
                issues.append("D21 acceptance does not state LOGIC_PASS")
        except (OSError, UnicodeError) as error:
            issues.append("cannot read D21 acceptance: " + str(error))

    if not D21_RUNNER_REPORT.exists():
        issues.append(relative_path(D21_RUNNER_REPORT) + " is missing")
    else:
        try:
            report_payload = json.loads(D21_RUNNER_REPORT.read_text(encoding="utf-8"))
        except (OSError, UnicodeError, json.JSONDecodeError) as error:
            issues.append("invalid D21 runner report: " + str(error))
        else:
            report_evidence = report_payload.get("evidence", {}) if isinstance(report_payload, dict) else {}
            suites = report_payload.get("suites", []) if isinstance(report_payload, dict) else []
            scope = report_payload.get("scope", {}) if isinstance(report_payload, dict) else {}
            report_pass = bool(
                isinstance(report_payload, dict)
                and report_payload.get("status") == "PASS"
                and isinstance(report_evidence, dict)
                and report_evidence.get("status") == "PASS"
                and report_evidence.get("expected_results") == D21_TASK_COUNT
                and report_evidence.get("present_results") == D21_TASK_COUNT
                and report_evidence.get("present_handoffs") == D21_TASK_COUNT
                and isinstance(suites, list)
                and suites
                and all(isinstance(suite, dict) and suite.get("status") == "PASS" for suite in suites)
                and scope.get("unity_player_playmode_executed") is False
            )
            if not report_pass:
                issues.append("D21 runner report is not a complete PASS report")

    if evidence["status"] != "PASS":
        issues.append("D21 result/handoff evidence is incomplete or invalid")

    return {
        "status": "PASS" if not issues else "FAIL",
        "acceptance_path": relative_path(D21_ACCEPTANCE),
        "runner_report_path": relative_path(D21_RUNNER_REPORT),
        "acceptance_pass": acceptance_pass,
        "runner_report_pass": report_pass,
        "evidence": evidence,
        "report_summary": {
            "status": report_payload.get("status") if isinstance(report_payload, dict) else None,
            "suite_count": len(report_payload.get("suites", [])) if isinstance(report_payload, dict) and isinstance(report_payload.get("suites"), list) else 0,
        },
        "issues": issues,
    }


def emit_audit(payload: Dict[str, Any], markers: Mapping[str, int]) -> int:
    """Print a JSON audit followed by stable shell-friendly assertion markers."""

    print(json.dumps(payload, ensure_ascii=False, indent=2))
    for name, value in markers.items():
        print("{}={}".format(name, value))
    return 0 if payload.get("status") == "PASS" else 1


def build_suite_definitions() -> List[Dict[str, Any]]:
    python = sys.executable
    return [
        {
            "id": "D22-PYTHON-SYNTAX",
            "name": "D22 runner and boundary audit Python syntax",
            "command": [
                python,
                "-m",
                "py_compile",
                "tools/d22_logic_test_runner.py",
                "tools/d22_logic_boundary_audit.py",
            ],
            "timeout": 30,
        },
        {
            "id": "D22-SOURCE-INVENTORY",
            "name": "D22 isolated pure-source inventory",
            "command": [python, "tools/d22_logic_test_runner.py", "--audit-source-inventory"],
            "timeout": 30,
            "expected_markers": {
                "D22_SOURCE_FILES": len(EXPECTED_D22_SOURCE_NAMES),
                "D22_TEST_FILES": len(EXPECTED_D22_TEST_NAMES),
                "D22_SOURCE_INVENTORY_PASS": 1,
            },
        },
        {
            "id": "D22-BOUNDARY-SELF-TEST",
            "name": "D22 logic boundary audit self-test",
            "command": [python, "tools/d22_logic_boundary_audit.py", "--self-test"],
            "timeout": 30,
        },
        {
            "id": "D22-BOUNDARY-AUDIT",
            "name": "D22 logic boundary static audit",
            "command": [python, "tools/d22_logic_boundary_audit.py", "--json"],
            "timeout": 30,
        },
        {
            "id": "D21-UPSTREAM-AUDIT",
            "name": "D21 acceptance and result/handoff completeness audit",
            "command": [python, "tools/d22_logic_test_runner.py", "--audit-upstream"],
            "timeout": 60,
            "expected_markers": {
                "D21_UPSTREAM_RESULTS": D21_TASK_COUNT,
                "D21_UPSTREAM_HANDOFFS": D21_TASK_COUNT,
                "D21_UPSTREAM_ACCEPTANCE_PASS": 1,
                "D21_UPSTREAM_REPORT_PASS": 1,
                "D21_UPSTREAM_ISSUES": 0,
                "D21_UPSTREAM_PASS": 1,
            },
        },
        {
            "id": "D22-EVIDENCE-AUDIT",
            "name": "D22-L01..L32 result and handoff protocol audit",
            "command": [python, "tools/d22_logic_test_runner.py", "--audit-d22-evidence"],
            "timeout": 60,
            "expected_markers": {
                "D22_EVIDENCE_RESULTS": D22_UPSTREAM_TASK_COUNT,
                "D22_EVIDENCE_HANDOFFS": D22_UPSTREAM_TASK_COUNT,
                "D22_EVIDENCE_ISSUES": 0,
                "D22_EVIDENCE_PASS": 1,
            },
        },
        {
            "id": "D22-CSHARP-BUILD",
            "name": "D22 full pure C# offline build",
            "command": [
                "dotnet",
                "build",
                ".codex-temp/d22-logic/D22LogicTests.csproj",
                "--nologo",
                "-warnaserror",
            ],
            "timeout": 240,
        },
        {
            "id": "D22-CSHARP-TEST-RUN",
            "name": "D22 deterministic pure C# targeted test run",
            "command": [
                "dotnet",
                "run",
                "--project",
                ".codex-temp/d22-logic/D22LogicTests.csproj",
                "--no-build",
                "--no-restore",
            ],
            "timeout": 240,
            "expected_markers": {
                "D22_ADAPTER_TEST_CASES": 10,
                "D22_ADAPTER_TEST_FAILURES": 0,
                "D22_FOCUS_PANEL_ASSERTIONS": 13,
                "D22_COMMAND_FEEDBACK_ASSERTIONS": 8,
                "D22_DETERMINISTIC_ASSERTIONS": 44,
                "D22_FUZZ_ASSERTIONS": 77817,
                "D22_SAVELOAD_ASSERTIONS": 21,
                "D22_TEST_FAILURES": 0,
            },
        },
    ]


def required_document_check() -> Dict[str, Any]:
    paths = [D22_RUNTIME_DEFERRED, D22_STATUS_REVIEW, D22_ACCEPTANCE]
    missing = [relative_path(path) for path in paths if not path.exists()]
    return {
        "status": "PASS" if not missing else "FAIL",
        "required": [relative_path(path) for path in paths],
        "missing": missing,
    }


def run_all() -> Dict[str, Any]:
    started = time.monotonic()
    suites: List[Dict[str, Any]] = []
    for definition in build_suite_definitions():
        suites.append(
            run_command(
                definition["command"],
                suite_id=definition["id"],
                name=definition["name"],
                timeout=definition["timeout"],
                expected_markers=definition.get("expected_markers"),
            )
        )

    source_inventory = source_inventory_check()
    upstream = upstream_acceptance_check()
    d22_evidence = evidence_check("D22", D22_UPSTREAM_TASK_COUNT)
    documents = required_document_check()
    failed_suites = [suite for suite in suites if suite["status"] != "PASS"]
    commands_pass = not failed_suites
    pass_eligible = bool(
        commands_pass
        and source_inventory["status"] == "PASS"
        and upstream["status"] == "PASS"
        and d22_evidence["status"] == "PASS"
        and documents["status"] == "PASS"
    )
    return {
        "runner_name": "D22 Unified Pure Logic Convergence Gate Runner",
        "timestamp": now_iso(),
        "status": "PASS" if pass_eligible else "FAIL",
        "scope": {
            "expected_logic_tasks": "D22-L01..L36",
            "upstream_evidence_tasks": "D22-L01..L32",
            "d21_upstream_tasks": "D21-L01..L32",
            "unity_player_playmode_executed": False,
            "scene_prefab_ui_executed": False,
            "input_device_performance_executed": False,
            "status_sync_executed": False,
            "d21_runner_reexecuted": False,
        },
        "suite_count": len(suites),
        "passed_suites": len(suites) - len(failed_suites),
        "failed_suites": len(failed_suites),
        "suites": suites,
        "convergence_gate": {
            "pass_eligible": pass_eligible,
            "commands_pass": commands_pass,
            "failed_suite_ids": [suite["id"] for suite in failed_suites],
            "source_inventory": source_inventory,
            "d21_upstream": upstream,
            "d22_l01_l32_evidence": d22_evidence,
            "required_documents": documents,
        },
        "runtime_deferred": [
            "Canvas/UGUI/EventSystem、真实键鼠手柄/触摸、pointer/cursor、按钮点击和设备焦点未执行。",
            "场景/Prefab/Transform、Player/PlayMode、真实 owner runtime 接线和真实 SaveSystem 介质未执行。",
            "布局/分辨率/文本/本地化渲染、动画、字幕/通知视觉、音频、性能、目标设备和发行验证未执行。",
        ],
        "not_run_by_scope": [
            "Unity Player/PlayMode、场景、Prefab、Canvas/UI、真实输入和性能采样。",
            "python tools/sync_logic_task_status.py（按委派要求不修改共享状态台账）。",
            "D21 runner 本身（本任务只审计 D21 已接受的 acceptance、结果、handoff 和 runner report）。",
        ],
        "duration_ms": int((time.monotonic() - started) * 1000),
    }


def convergence_status(report: Mapping[str, Any]) -> str:
    gate = report["convergence_gate"]
    critical = (
        gate["source_inventory"]["status"] != "PASS"
        or gate["d21_upstream"]["status"] != "PASS"
        or gate["d22_l01_l32_evidence"]["status"] != "PASS"
        or gate["required_documents"]["status"] != "PASS"
    )
    if critical:
        return "LOGIC_BLOCKED"
    return "LOGIC_PASS" if report["status"] == "PASS" else "LOGIC_PARTIAL"


def gate_blockers(report: Mapping[str, Any]) -> List[Dict[str, Any]]:
    gate = report["convergence_gate"]
    blockers: List[Dict[str, Any]] = []
    if gate["source_inventory"]["status"] != "PASS":
        blockers.append(
            {
                "id": "D22-GATE-SOURCE-INVENTORY",
                "severity": "P0",
                "details": gate["source_inventory"].get("issues", []),
            }
        )
    if gate["d21_upstream"]["status"] != "PASS":
        blockers.append(
            {
                "id": "D22-GATE-D21-UPSTREAM",
                "severity": "P0",
                "details": gate["d21_upstream"].get("issues", []),
            }
        )
    if gate["d22_l01_l32_evidence"]["status"] != "PASS":
        blockers.append(
            {
                "id": "D22-GATE-D22-EVIDENCE",
                "severity": "P0",
                "details": {
                    "missing": gate["d22_l01_l32_evidence"].get("missing", []),
                    "invalid_json": gate["d22_l01_l32_evidence"].get("invalid_json", []),
                    "issues": gate["d22_l01_l32_evidence"].get("issues", []),
                },
            }
        )
    if gate["required_documents"]["status"] != "PASS":
        blockers.append(
            {
                "id": "D22-GATE-DOCUMENTS",
                "severity": "P0",
                "details": gate["required_documents"].get("missing", []),
            }
        )
    for suite in report["suites"]:
        if suite["status"] != "PASS":
            blockers.append(
                {
                    "id": "D22-GATE-SUITE-{}".format(suite["id"]),
                    "severity": "P0",
                    "details": {
                        "command": suite["command"],
                        "exit_code": suite["exit_code"],
                        "status": suite["status"],
                        "error_message": suite.get("error_message", ""),
                        "assertions": suite.get("assertions", {}),
                    },
                }
            )
    return blockers


def verification_records(report: Mapping[str, Any]) -> List[Dict[str, Any]]:
    records: List[Dict[str, Any]] = []
    for suite in report["suites"]:
        status = suite["status"]
        records.append(
            {
                "command": suite["command"],
                "result": "PASS" if status == "PASS" else "{} (exit_code={})".format(status, suite["exit_code"]),
                "exit_code": suite["exit_code"],
                "status": status,
                "assertion_status": suite.get("assertion_status", "NOT_RECORDED"),
                "assertions": suite.get("assertions", {}),
            }
        )
    return records


def artifact_payload(
    *,
    task_id: str,
    status: str,
    report: Mapping[str, Any],
    report_path: Path,
    changed_files: Iterable[str],
    summary: str,
    handoff: Path,
    extra: Optional[Mapping[str, Any]] = None,
) -> Dict[str, Any]:
    blockers = gate_blockers(report)
    payload: Dict[str, Any] = {
        "task_id": task_id,
        "status": status,
        "updated_at": report["timestamp"],
        "summary": summary,
        "changed_files": list(changed_files),
        "verification": verification_records(report),
        "runtime_deferred": list(report["runtime_deferred"]),
        "blockers": blockers,
        "handoff": relative_path(handoff),
        "runner_report": relative_path(report_path),
        "convergence_gate": {
            "pass_eligible": report["convergence_gate"]["pass_eligible"],
            "commands_pass": report["convergence_gate"]["commands_pass"],
            "d21_upstream_status": report["convergence_gate"]["d21_upstream"]["status"],
            "d22_l01_l32_evidence_status": report["convergence_gate"]["d22_l01_l32_evidence"]["status"],
            "source_inventory_status": report["convergence_gate"]["source_inventory"]["status"],
        },
    }
    if extra:
        payload.update(extra)
    return payload


def handoff_text(
    *,
    task_id: str,
    status: str,
    result: Path,
    report_path: Path,
    report: Mapping[str, Any],
    focus: str,
) -> str:
    blockers = gate_blockers(report)
    lines = [
        "# {} Logic Handoff".format(task_id),
        "",
        "| Field | Value |",
        "|---|---|",
        "| Task | `{}` |".format(task_id),
        "| Status | `{}` |".format(status),
        "| Date | `2026-09-01` |",
        "| Result | `{}` |".format(relative_path(result)),
        "| Runner report | `{}` |".format(relative_path(report_path)),
        "",
        "## 汇合结果",
        "",
        "- {}".format(focus),
        "- D21 upstream acceptance/result/handoff audit: `{}`。".format(
            report["convergence_gate"]["d21_upstream"]["status"]
        ),
        "- D22-L01—L32 result/handoff protocol audit: `{}`（{}/{} results, {}/{} handoffs）。".format(
            report["convergence_gate"]["d22_l01_l32_evidence"]["status"],
            report["convergence_gate"]["d22_l01_l32_evidence"]["present_results"],
            report["convergence_gate"]["d22_l01_l32_evidence"]["expected_results"],
            report["convergence_gate"]["d22_l01_l32_evidence"]["present_handoffs"],
            report["convergence_gate"]["d22_l01_l32_evidence"]["expected_results"],
        ),
        "- Offline command suites: `{}`/`{}` passed；runner exit status is `{}`。".format(
            report["passed_suites"], report["suite_count"], report["status"]
        ),
        "- Unity Player/PlayMode、场景、Prefab、Canvas/UI、真实输入、性能和发行验证均未执行。",
    ]
    if blockers:
        lines.extend(["", "## Blockers", ""])
        for blocker in blockers:
            lines.append(
                "- `{}` [{}] {}".format(
                    blocker["id"], blocker["severity"], json.dumps(blocker["details"], ensure_ascii=False)
                )
            )
    else:
        lines.extend(["", "## Blockers", "", "- 无 P0 blocker；Runtime Deferred 仍按登记文档保留。"])
    lines.extend(
        [
            "",
            "## 交接约束",
            "",
            "- D23 只能消费 D22 的公开命令结果、反馈/localization key 和防御性 ViewModel；不能把 UI 状态写回任何业务 owner。",
            "- `Blocked`、`Rejected`、`Pending`、`Duplicate`、`Cancelled` 必须保持原语义，不能显示为业务成功。",
            "- 本 handoff 不授予 RuntimeApproved 或 ReleaseApproved；真实按钮、设备、布局、Player/PlayMode 和性能仍延期。",
        ]
    )
    return "\n".join(lines) + "\n"


def write_convergence_artifacts(report: Dict[str, Any], report_path: Path) -> None:
    """Write L33..L36 results/handoffs from the same immutable run report."""

    status = convergence_status(report)
    common_changed = ["tools/d22_logic_test_runner.py"]
    if D22_TEMP_PROJECT.exists():
        common_changed.append(relative_path(D22_TEMP_PROJECT))
    if D22_TEMP_PROGRAM.exists():
        common_changed.append(relative_path(D22_TEMP_PROGRAM))

    definitions = [
        (
            33,
            "D22-L33",
            common_changed,
            "D22 纯逻辑汇合 runner 已执行离线源清单、D21 上游审计、D22-L01—L32 证据审计、边界审计、纯 C# 编译和定向测试。" if status == "LOGIC_PASS" else "D22 汇合 runner 已执行，但门禁未满足；结果按真实命令或证据缺口保持 {}。".format(status),
            "runner 汇总了真实命令、退出码、断言标记、边界审计和上下游证据完整性。",
            {},
        ),
        (
            34,
            "D22-L34",
            ["docs/game/design/common/D22_RUNTIME_DEFERRED_REGISTER.md"],
            "已登记 D22 UI 逻辑交接后的 Runtime Deferred 范围；Runtime/Release 仍与逻辑门禁分离。" if status == "LOGIC_PASS" else "Runtime Deferred 登记已保留，但汇合门禁未满足，结果保持 {}。".format(status),
            "延期登记覆盖 Canvas/UI、真实输入/焦点、布局/可访问性、Player/PlayMode、设备、性能和发行验证。",
            {"runtime_deferred_register": relative_path(D22_RUNTIME_DEFERRED)},
        ),
        (
            35,
            "D22-L35",
            ["docs/game/design/common/D22_LOGIC_STATUS_REVIEW.md"],
            "已完成 D22 结果、handoff、D21 上游、边界审计和命令证据状态复核。" if status == "LOGIC_PASS" else "已完成状态审查，但汇合门禁未满足，结果保持 {}。".format(status),
            "状态审查以机器 runner report 为权威，不把局部结果或旧 handoff 当作章节通过。",
            {"status_review": relative_path(D22_STATUS_REVIEW)},
        ),
        (
            36,
            "D22-L36",
            ["docs/game/design/common/D22_LOGIC_ACCEPTANCE.md"],
            "D22 纯逻辑汇合验收通过，可向 D23 提供稳定 UI 命令/读模型边界；Runtime/Release 仍未放行。" if status == "LOGIC_PASS" else "D22 纯逻辑汇合验收未满足，按真实门禁结果保持 {}。".format(status),
            "验收结论只在 D21/D22 证据完整且所有离线命令真实通过时升级为 LOGIC_PASS。",
            {"acceptance_document": relative_path(D22_ACCEPTANCE)},
        ),
    ]

    artifact_paths: Dict[str, Dict[str, str]] = {}
    for number, task_id, changed, summary, focus, extra in definitions:
        result = RESULT_ROOT / task_id / "{}_LOGIC_RESULT.json".format(task_id.replace("-", "_"))
        handoff = HANDOFF_ROOT / "{}_LOGIC_HANDOFF.md".format(task_id)
        changed_files = list(changed) + [relative_path(result), relative_path(handoff)]
        payload = artifact_payload(
            task_id=task_id,
            status=status,
            report=report,
            report_path=report_path,
            changed_files=changed_files,
            summary=summary,
            handoff=handoff,
            extra=extra,
        )
        result.parent.mkdir(parents=True, exist_ok=True)
        handoff.parent.mkdir(parents=True, exist_ok=True)
        result.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        handoff.write_text(
            handoff_text(
                task_id=task_id,
                status=status,
                result=result,
                report_path=report_path,
                report=report,
                focus=focus,
            ),
            encoding="utf-8",
        )
        artifact_paths[task_id] = {
            "result": relative_path(result),
            "handoff": relative_path(handoff),
        }
    report["artifacts"] = artifact_paths


def resolve_output(path: Optional[Path]) -> Path:
    if path is None:
        return RESULT_ROOT / "D22-L33" / "D22_L33_RUNNER_REPORT.json"
    return path if path.is_absolute() else ROOT / path


def run_audit_subcommand(mode: str) -> int:
    if mode == "source":
        inventory = source_inventory_check()
        counts = inventory["counts"]
        return emit_audit(
            inventory,
            {
                "D22_SOURCE_FILES": counts.get("d22_sources", 0),
                "D22_TEST_FILES": counts.get("d22_tests", 0),
                "D22_SOURCE_INVENTORY_PASS": 1 if inventory["status"] == "PASS" else 0,
            },
        )
    if mode == "upstream":
        upstream = upstream_acceptance_check()
        evidence = upstream["evidence"]
        return emit_audit(
            upstream,
            {
                "D21_UPSTREAM_RESULTS": evidence["present_results"],
                "D21_UPSTREAM_HANDOFFS": evidence["present_handoffs"],
                "D21_UPSTREAM_ACCEPTANCE_PASS": 1 if upstream["acceptance_pass"] else 0,
                "D21_UPSTREAM_REPORT_PASS": 1 if upstream["runner_report_pass"] else 0,
                "D21_UPSTREAM_ISSUES": len(upstream["issues"]),
                "D21_UPSTREAM_PASS": 1 if upstream["status"] == "PASS" else 0,
            },
        )
    if mode == "d22":
        evidence = evidence_check("D22", D22_UPSTREAM_TASK_COUNT)
        return emit_audit(
            evidence,
            {
                "D22_EVIDENCE_RESULTS": evidence["present_results"],
                "D22_EVIDENCE_HANDOFFS": evidence["present_handoffs"],
                "D22_EVIDENCE_ISSUES": len(evidence["missing"]) + len(evidence["invalid_json"]) + len(evidence["issues"]),
                "D22_EVIDENCE_PASS": 1 if evidence["status"] == "PASS" else 0,
            },
        )
    raise ValueError("unknown audit mode: " + mode)


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, help="write the main machine report to this path")
    parser.add_argument("--audit-source-inventory", action="store_true")
    parser.add_argument("--audit-upstream", action="store_true")
    parser.add_argument("--audit-d22-evidence", action="store_true")
    args = parser.parse_args(argv)

    modes = [
        (args.audit_source_inventory, "source"),
        (args.audit_upstream, "upstream"),
        (args.audit_d22_evidence, "d22"),
    ]
    selected = [mode for enabled, mode in modes if enabled]
    if len(selected) > 1:
        parser.error("audit flags are mutually exclusive")
    if selected:
        return run_audit_subcommand(selected[0])

    report = run_all()
    report_path = resolve_output(args.output)
    report["report_path"] = relative_path(report_path) if report_path.is_relative_to(ROOT) else str(report_path)
    write_convergence_artifacts(report, report_path)
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if report["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
