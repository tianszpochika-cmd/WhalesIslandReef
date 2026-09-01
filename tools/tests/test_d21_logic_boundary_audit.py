#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D21 逻辑边界审计工具测试（D21-L29）。"""

from pathlib import Path
import unittest

from tools.d21_logic_boundary_audit import audit_file_content, run_audit, run_self_test


class TestD21LogicBoundaryAudit(unittest.TestCase):
    def test_comments_and_string_literals_are_not_scanned(self):
        source = r'''// UnityEngine.Transform HttpClient DateTime.UtcNow new Random()
/* Socket, Physics.Raycast and D20Projection._privateField are documentation only. */
public static class CleanSource
{
    private const string Normal = "UnityEngine.GameObject // HttpClient DateTime.UtcNow";
    private const string Verbatim = @"Socket /* Stopwatch */ D20Projection._privateField";
    public static int Tick(int value) { return value + 1; }
}'''
        self.assertEqual(audit_file_content("CleanSource.cs", source), [])

    def test_executable_forbidden_references_are_detected(self):
        source = """
using UnityEngine;
using System.Net.Http;
public class DirtySource : MonoBehaviour
{
    private HttpClient client = new HttpClient();
    public void Tick()
    {
        var now = DateTime.UtcNow;
        var random = new Random();
        D20CrewProjection._privateField = random;
    }
}
"""
        rule_ids = {item["rule_id"] for item in audit_file_content("DirtySource.cs", source)}
        self.assertTrue({
            "D21-AUDIT-001",
            "D21-AUDIT-002",
            "D21-AUDIT-003",
            "D21-AUDIT-004",
        }.issubset(rule_ids))

    def test_builtin_self_test_and_real_d21_scan_pass(self):
        self.assertTrue(run_self_test()["passed"])
        report = run_audit(Path("."))
        self.assertTrue(report["passed"], report["violations"])


if __name__ == "__main__":
    unittest.main()
