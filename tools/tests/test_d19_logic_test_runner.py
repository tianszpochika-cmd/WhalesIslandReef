#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
test_d19_logic_test_runner.py —— D19 统一测试运行器单元测试（D19-L41）
"""

import sys
import unittest
from tools.d19_logic_test_runner import run_single_suite, run_all_d19_logic_tests

class TestD19LogicTestRunner(unittest.TestCase):

    def test_single_suite_pass(self):
        suite = {
            "id": "TEST-PASS",
            "name": "Successful Python Command",
            "command": [sys.executable, "-c", "import sys; sys.exit(0)"]
        }
        res = run_single_suite(suite)
        self.assertEqual(res["status"], "PASS")
        self.assertEqual(res["exit_code"], 0)

    def test_single_suite_fail_propagation(self):
        suite = {
            "id": "TEST-FAIL",
            "name": "Failing Python Command",
            "command": [sys.executable, "-c", "import sys; sys.exit(42)"]
        }
        res = run_single_suite(suite)
        self.assertEqual(res["status"], "FAIL")
        self.assertEqual(res["exit_code"], 42)

    def test_overall_runner_fails_if_any_child_fails(self):
        custom_suites = [
            {
                "id": "SUB-1",
                "name": "Passing sub-task",
                "command": [sys.executable, "-c", "import sys; sys.exit(0)"]
            },
            {
                "id": "SUB-2",
                "name": "Failing sub-task",
                "command": [sys.executable, "-c", "import sys; sys.exit(1)"]
            }
        ]
        report = run_all_d19_logic_tests(custom_suites)
        self.assertFalse(report["all_passed"], "任一子项失败时总状态必须为 False")
        self.assertEqual(report["failed_suites"], 1)
        self.assertEqual(report["passed_suites"], 1)

    def test_real_d19_unified_run_passes(self):
        report = run_all_d19_logic_tests()
        self.assertTrue(report["all_passed"], f"D19 统一运行器真实执行失败: {report}")
        self.assertEqual(report["failed_suites"], 0)

if __name__ == "__main__":
    unittest.main()
