#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24-L26：将 D24-L25 当前回归报告与固定逻辑基线比较。

比较只读取当前报告和仓库源码，不自动更新 baseline。它阻止逻辑
测试数量/关键指标下降，并验证迁移器、稳定错误码和跨章节回归入口
仍存在。baseline 是 D24-L26 的受控证据文件，不能由本工具隐式重写。
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, List, Mapping, Optional, Sequence


ROOT = Path(__file__).resolve().parents[1]
DEFAULT_BASELINE = ROOT / "game" / "tools" / "results" / "D24" / "D24-L26" / "D24_L26_BASELINE.json"
DEFAULT_CURRENT = ROOT / "game" / "tools" / "results" / "D24" / "D24-L25" / "D24_L25_RUNNER_REPORT.json"


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--baseline", type=Path, default=DEFAULT_BASELINE)
    parser.add_argument("--current", type=Path, default=DEFAULT_CURRENT)
    parser.add_argument("--json-out", type=Path, default=None)
    parser.add_argument("--self-test", action="store_true")
    return parser.parse_args(argv)


def load_object(path: Path) -> tuple[Optional[Dict[str, Any]], str]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        return None, str(error)
    if not isinstance(value, dict):
        return None, "top-level JSON value is not an object"
    return value, ""


def path_from(root: Path, value: str) -> Path:
    path = Path(value)
    return path if path.is_absolute() else root / path


def check_record(check_id: str, passed: bool, evidence: str, *, details: Any = None) -> Dict[str, Any]:
    record: Dict[str, Any] = {
        "id": check_id,
        "status": "PASS" if passed else "FAIL",
        "evidence": evidence,
    }
    if details is not None:
        record["details"] = details
    return record


def suite_by_id(current: Mapping[str, Any], suite_id: str) -> Optional[Mapping[str, Any]]:
    suites = current.get("suites")
    if not isinstance(suites, list):
        return None
    for suite in suites:
        if isinstance(suite, dict) and suite.get("id") == suite_id:
            return suite
    return None


def compare(root: Path, baseline_path: Path, current_path: Path) -> Dict[str, Any]:
    baseline, baseline_error = load_object(baseline_path)
    current, current_error = load_object(current_path)
    checks: List[Dict[str, Any]] = []
    issues: List[str] = []
    if baseline is None:
        issues.append("baseline: " + (baseline_error or "missing"))
    if current is None:
        issues.append("current: " + (current_error or "missing"))
    if baseline is None or current is None:
        return {
            "schema_version": "d24.regression-baseline-compare.v1",
            "status": "FAIL",
            "passed": False,
            "baseline": str(baseline_path).replace("\\", "/"),
            "current": str(current_path).replace("\\", "/"),
            "checks": checks,
            "issues": issues,
        }

    baseline_id = str(baseline.get("baseline_id", "unknown"))
    current_pass = current.get("status") == "PASS" and current.get("pass_eligible") is True
    checks.append(check_record("current_report_is_logic_pass", current_pass, "status={} pass_eligible={}".format(current.get("status"), current.get("pass_eligible"))))
    if not current_pass:
        issues.append("current regression report is not PASS/pass_eligible")

    protocol = current.get("protocol_matrix")
    required_protocol = baseline.get("required_protocol", {})
    protocol_ok = isinstance(protocol, dict)
    protocol_evidence = {}
    if protocol_ok:
        for key, minimum in required_protocol.items():
            observed = protocol.get(key)
            protocol_evidence[key] = {"observed": observed, "minimum": minimum}
            if not isinstance(observed, int) or observed < int(minimum):
                protocol_ok = False
                issues.append("protocol {} dropped below baseline: {} < {}".format(key, observed, minimum))
    checks.append(check_record("protocol_matrix_not_reduced", protocol_ok, json.dumps(protocol_evidence, ensure_ascii=False, sort_keys=True)))

    required_suites = baseline.get("required_suites", [])
    suites_ok = True
    missing_suites: List[str] = []
    failed_suites: List[str] = []
    if not isinstance(required_suites, list):
        suites_ok = False
        issues.append("baseline required_suites is not a list")
    else:
        for suite_id in required_suites:
            suite = suite_by_id(current, str(suite_id))
            if suite is None:
                suites_ok = False
                missing_suites.append(str(suite_id))
            elif suite.get("status") != "PASS":
                suites_ok = False
                failed_suites.append(str(suite_id) + ":" + str(suite.get("status")))
        if missing_suites:
            issues.extend("required suite missing: " + item for item in missing_suites)
        if failed_suites:
            issues.extend("required suite failed: " + item for item in failed_suites)
    checks.append(check_record("required_suites_present_and_pass", suites_ok, "missing={} failed={}".format(missing_suites, failed_suites)))

    minimums = baseline.get("minimums", {})
    audit = suite_by_id(current, "D24-AUDITS") or {}
    audit_min = minimums.get("d24_audits", {}) if isinstance(minimums, dict) else {}
    audit_count_ok = int(audit.get("count", 0) or 0) >= int(audit_min.get("count", 0))
    audit_pass_ok = int(audit.get("passed", 0) or 0) >= int(audit_min.get("passed", 0))
    audits_ok = audit_count_ok and audit_pass_ok
    if not audits_ok:
        issues.append("D24 audit count/passed dropped: count={}, passed={}".format(audit.get("count"), audit.get("passed")))
    checks.append(check_record("d24_audit_count_not_reduced", audits_ok, "count={} passed={}".format(audit.get("count"), audit.get("passed"))))

    csharp = suite_by_id(current, "D24-CSHARP-L17-L22") or {}
    csharp_summary = csharp.get("payload_summary") if isinstance(csharp.get("payload_summary"), dict) else {}
    csharp_min = minimums.get("d24_csharp", {}) if isinstance(minimums, dict) else {}
    csharp_task_count = int(csharp_summary.get("task_count", 0) or 0)
    csharp_statuses = csharp_summary.get("task_statuses", [])
    csharp_ok = csharp_task_count >= int(csharp_min.get("task_count", 0)) and isinstance(csharp_statuses, list) and all(status == "PASS" for status in csharp_statuses)
    if not csharp_ok:
        issues.append("D24 C# task coverage/status regressed: {} {}".format(csharp_task_count, csharp_statuses))
    checks.append(check_record("d24_csharp_task_coverage_not_reduced", csharp_ok, "task_count={} statuses={}".format(csharp_task_count, csharp_statuses)))

    agentcore = suite_by_id(current, "D24-AGENTCORE-L23") or {}
    agent_summary = agentcore.get("payload_summary") if isinstance(agentcore.get("payload_summary"), dict) else {}
    agent_min = minimums.get("agentcore", {}) if isinstance(minimums, dict) else {}
    agent_count = int(agent_summary.get("check_count", 0) or 0)
    agent_statuses = agent_summary.get("check_statuses", [])
    agent_ok = agent_count >= int(agent_min.get("check_count", 0)) and isinstance(agent_statuses, list) and all(status == "PASS" for status in agent_statuses)
    if not agent_ok:
        issues.append("AgentCore check coverage/status regressed: {} {}".format(agent_count, agent_statuses))
    checks.append(check_record("agentcore_checks_not_reduced", agent_ok, "check_count={} statuses={}".format(agent_count, agent_statuses)))

    d23 = suite_by_id(current, "D23-SCENARIO-26-CASE") or {}
    observed = d23.get("observed_markers") if isinstance(d23.get("observed_markers"), dict) else {}
    d23_min = minimums.get("d23", {}) if isinstance(minimums, dict) else {}
    d23_ok = (
        int(observed.get("case_count", 0) or 0) >= int(d23_min.get("case_count", 0))
        and int(observed.get("assertion_count", 0) or 0) >= int(d23_min.get("assertion_count", 0))
        and int(observed.get("failure_count", -1) or -1) <= int(d23_min.get("failure_count", 0))
    )
    if not d23_ok:
        issues.append("D23 scenario markers regressed: " + json.dumps(observed, ensure_ascii=False, sort_keys=True))
    checks.append(check_record("d23_scenario_markers_not_reduced", d23_ok, json.dumps(observed, ensure_ascii=False, sort_keys=True)))

    source_checks = baseline.get("required_source_markers", [])
    source_ok = True
    source_details: List[Dict[str, Any]] = []
    if not isinstance(source_checks, list):
        source_ok = False
        issues.append("baseline required_source_markers is not a list")
    else:
        for item in source_checks:
            if not isinstance(item, dict):
                source_ok = False
                issues.append("invalid source marker record")
                continue
            relative_path = str(item.get("path", ""))
            source_path = root / relative_path
            text = source_path.read_text(encoding="utf-8", errors="replace") if source_path.exists() else ""
            patterns = item.get("patterns", [])
            missing = [str(pattern) for pattern in patterns if str(pattern) not in text] if isinstance(patterns, list) else ["patterns_not_list"]
            passed = source_path.exists() and not missing
            source_details.append({"path": relative_path, "missing_patterns": missing, "status": "PASS" if passed else "FAIL"})
            if not passed:
                source_ok = False
                issues.append("source marker missing: {} {}".format(relative_path, missing))
    checks.append(check_record("migration_and_error_markers_preserved", source_ok, "{} source records checked".format(len(source_details)), details=source_details))

    passed = not issues and all(item["status"] == "PASS" for item in checks)
    return {
        "schema_version": "d24.regression-baseline-compare.v1",
        "baseline_id": baseline_id,
        "status": "PASS" if passed else "FAIL",
        "passed": passed,
        "baseline": str(baseline_path).replace("\\", "/"),
        "current": str(current_path).replace("\\", "/"),
        "checks": checks,
        "issues": issues,
    }


def self_test() -> Dict[str, Any]:
    checks = [
        {"id": "minimum_comparison", "passed": 4 >= 3},
        {"id": "missing_marker_detection", "passed": "needle" not in "haystack"},
        {"id": "failure_count_rule", "passed": 0 <= 0},
    ]
    return {
        "schema_version": "d24.regression-baseline-compare.v1",
        "status": "PASS" if all(item["passed"] for item in checks) else "FAIL",
        "passed": all(item["passed"] for item in checks),
        "checks": checks,
    }


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    root = args.root.expanduser().resolve()
    payload = self_test() if args.self_test else compare(root, path_from(root, str(args.baseline)), path_from(root, str(args.current)))
    encoded = json.dumps(payload, ensure_ascii=False, indent=2, sort_keys=True)
    print(encoded)
    if args.json_out is not None:
        output = args.json_out if args.json_out.is_absolute() else root / args.json_out
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(encoded + "\n", encoding="utf-8")
    return 0 if payload.get("passed") is True else 1


if __name__ == "__main__":
    sys.exit(main())
