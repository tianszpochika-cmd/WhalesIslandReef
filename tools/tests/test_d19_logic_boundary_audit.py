#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
test_d19_logic_boundary_audit.py —— D19 逻辑边界审计工具单元测试（D19-L40）
"""

import unittest
from tools.d19_logic_boundary_audit import audit_file_content, run_d19_audit

class TestD19LogicBoundaryAudit(unittest.TestCase):

    def test_positive_clean_file_passes(self):
        clean_code = """
        using System;
        using System.Collections.Generic;
        namespace Whalebone.Systems.D19
        {
            public class CleanClass
            {
                public int WorldDay;
                public void DoLogic() { }
            }
        }
        """
        violations = audit_file_content("CleanClass.cs", clean_code)
        self.assertEqual(len(violations), 0, "合规纯 C# 代码不应触发任何违规")

    def test_forbidden_unity_engine_detected(self):
        dirty_code = """
        using UnityEngine;
        public class DirtyUnity : MonoBehaviour
        {
            public GameObject go;
            public Transform tr;
        }
        """
        violations = audit_file_content("DirtyUnity.cs", dirty_code)
        self.assertTrue(any(v["rule_id"] == "D19-AUDIT-001" for v in violations))

    def test_forbidden_network_or_llm_detected(self):
        dirty_code = """
        using System.Net.Http;
        public class DirtyNet
        {
            private HttpClient client = new HttpClient();
            private void CallOllama() { }
        }
        """
        violations = audit_file_content("DirtyNet.cs", dirty_code)
        self.assertTrue(any(v["rule_id"] == "D19-AUDIT-002" for v in violations))

    def test_forbidden_system_clock_or_random_detected(self):
        dirty_code = """
        using System;
        public class DirtyTime
        {
            public void Test()
            {
                var now = DateTime.Now;
                var rand = new Random();
            }
        }
        """
        violations = audit_file_content("DirtyTime.cs", dirty_code)
        self.assertTrue(any(v["rule_id"] == "D19-AUDIT-003" for v in violations))

    def test_forbidden_foreign_state_mutation_detected(self):
        dirty_code = """
        public class DirtyState
        {
            public void Mutate()
            {
                GameState.Instance.PlayerMoney += 100;
            }
        }
        """
        violations = audit_file_content("DirtyState.cs", dirty_code)
        self.assertTrue(any(v["rule_id"] == "D19-AUDIT-004" for v in violations))

    def test_real_d19_codebase_passes_completely(self):
        passed, report = run_d19_audit(".")
        self.assertTrue(passed, f"真实 D19 源码库存在违规: {report.get('violations')}")

if __name__ == "__main__":
    unittest.main()
