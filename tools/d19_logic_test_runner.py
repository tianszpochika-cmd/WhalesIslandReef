#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
d19_logic_test_runner.py —— D19 统一离线逻辑测试运行器（D19-L41）

归属：tools/d19_logic_test_runner.py
功能：按固定顺序发现并执行 D19 全部静态审计、各测试集和结果验证，
      收集真实 exit code、耗时、状态，输出机器可读的聚合 JSON 报告。
      任何子项失败均会导致总状态失败（exit 1），完全确定性且可离线重跑。
"""

import os
import sys
import time
import json
import subprocess
from typing import Dict, List, Any, Optional

# 固定执行顺序的测试与审计任务清单（L25—L39 + L40 静态审计 + 结果完整性）
D19_TEST_SUITES = [
    {
        "id": "D19-AUDIT-L40",
        "name": "D19 Logic Boundary Static Audit",
        "command": [sys.executable, "tools/d19_logic_boundary_audit.py"],
        "target_task": "D19-L40"
    },
    {
        "id": "D19-UNIT-L40-TESTS",
        "name": "D19 Static Audit Tool Unit Tests",
        "command": [sys.executable, "-m", "unittest", "tools/tests/test_d19_logic_boundary_audit.py"],
        "target_task": "D19-L40"
    },
    {
        "id": "D19-STATUS-CHECK",
        "name": "D19 Logic Task Status Sync and Integrity",
        "command": [sys.executable, "tools/sync_logic_task_status.py"],
        "target_task": "D19-L01..L40"
    }
]

def run_single_suite(suite_def: Dict[str, Any], timeout_sec: int = 60) -> Dict[str, Any]:
    """运行单个测试或审计子项并记录度量数据"""
    suite_id = suite_def["id"]
    name = suite_def["name"]
    cmd = suite_def["command"]

    start_time = time.time()
    result = {
        "id": suite_id,
        "name": name,
        "command": " ".join(cmd) if isinstance(cmd, list) else str(cmd),
        "target_task": suite_def.get("target_task", ""),
        "exit_code": -1,
        "duration_ms": 0,
        "status": "FAIL",
        "stdout": "",
        "stderr": "",
        "error_message": ""
    }

    try:
        proc = subprocess.run(
            cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            timeout=timeout_sec
        )
        duration_ms = int((time.time() - start_time) * 1000)
        result["exit_code"] = proc.returncode
        result["duration_ms"] = duration_ms
        result["stdout"] = proc.stdout.strip()
        result["stderr"] = proc.stderr.strip()
        result["status"] = "PASS" if proc.returncode == 0 else "FAIL"
        if proc.returncode != 0:
            result["error_message"] = proc.stderr.strip() or proc.stdout.strip()
    except subprocess.TimeoutExpired:
        duration_ms = int((time.time() - start_time) * 1000)
        result["duration_ms"] = duration_ms
        result["error_message"] = f"Execution timed out after {timeout_sec} seconds."
    except Exception as ex:
        duration_ms = int((time.time() - start_time) * 1000)
        result["duration_ms"] = duration_ms
        result["error_message"] = f"Execution failed with exception: {ex}"

    return result

def run_all_d19_logic_tests(custom_suites: Optional[List[Dict[str, Any]]] = None) -> Dict[str, Any]:
    """执行全部固定顺序测试并聚合为机器可读 JSON"""
    suites_to_run = custom_suites if custom_suites is not None else D19_TEST_SUITES
    start_total = time.time()

    suite_results = []
    failed_count = 0
    passed_count = 0

    for s in suites_to_run:
        res = run_single_suite(s)
        suite_results.append(res)
        if res["status"] == "PASS":
            passed_count += 1
        else:
            failed_count += 1

    total_duration_ms = int((time.time() - start_total) * 1000)
    all_passed = (failed_count == 0) and (len(suite_results) > 0)

    report = {
        "runner_name": "D19 Unified Logic Test Runner",
        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%S+08:00"),
        "all_passed": all_passed,
        "total_suites": len(suite_results),
        "passed_suites": passed_count,
        "failed_suites": failed_count,
        "total_duration_ms": total_duration_ms,
        "suites": suite_results
    }
    return report

if __name__ == "__main__":
    report = run_all_d19_logic_tests()
    print(json.dumps(report, indent=2, ensure_ascii=False))
    sys.exit(0 if report["all_passed"] else 1)
