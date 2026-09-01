#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""D24 审计共用层的离线单元测试；不访问网络、不启动 Unity。"""

from __future__ import annotations

import json
import tempfile
import unittest
from pathlib import Path

from tools.d24_logic_audit_common import (
    duplicate_values,
    run_named_audit,
    scan_csharp,
    self_test,
    strip_comments_and_strings,
)


class D24LogicAuditCommonTests(unittest.TestCase):
    def test_masker_preserves_line_numbers_and_masks_literals(self) -> None:
        source = (
            "// DateTime.Now\n"
            "var text = @\"Guid.NewGuid() // text\";\n"
            "/* Task.Delay(1) */\n"
            "var now = DateTime.Now;\n"
        )
        masked = strip_comments_and_strings(source)
        self.assertNotIn("DateTime.Now", masked.splitlines()[0])
        self.assertNotIn("Guid.NewGuid", masked)
        self.assertNotIn("Task.Delay", masked)
        self.assertIn("DateTime.Now", masked.splitlines()[3])

    def test_scanner_uses_override_root_and_ignores_comment_hits(self) -> None:
        with tempfile.TemporaryDirectory(prefix="d24-common-test-") as directory:
            root = Path(directory)
            sample = root / "sample.cs"
            sample.write_text(
                "// Forbidden.Call()\n"
                "var value = Forbidden.Call();\n",
                encoding="utf-8",
            )
            scan = scan_csharp((("forbidden", r"Forbidden\.Call\s*\("),), roots=(root,), root=root)
            self.assertEqual(1, len(scan["hits"]))
            self.assertEqual(2, scan["hits"][0]["line"])
            self.assertEqual("sample.cs", scan["hits"][0]["file"])

    def test_duplicate_values_is_explicit_failure_input(self) -> None:
        with tempfile.TemporaryDirectory(prefix="d24-duplicate-test-") as directory:
            root = Path(directory)
            first = root / "first.json"
            second = root / "second.json"
            first.write_text(json.dumps({"id": "item_same"}), encoding="utf-8")
            second.write_text(json.dumps({"id": "item_same"}), encoding="utf-8")
            records = [(first, json.loads(first.read_text(encoding="utf-8"))), (second, json.loads(second.read_text(encoding="utf-8")))]
            duplicates = duplicate_values(records, ("id",), base_root=root)
            self.assertEqual([{"file": "first.json", "key": "id"}, {"file": "second.json", "key": "id"}], duplicates["id:item_same"])

    def test_named_audit_reports_duplicate_registry_ids(self) -> None:
        with tempfile.TemporaryDirectory(prefix="d24-named-audit-") as directory:
            root = Path(directory)
            entries = root / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"
            entries.mkdir(parents=True)
            for name in ("one.json", "two.json"):
                (entries / name).write_text(json.dumps({"id": "item_duplicate"}), encoding="utf-8")
            payload = run_named_audit("content_id_uniqueness", root)
            self.assertFalse(payload["passed"])
            self.assertEqual("FAIL", payload["checks"][0]["status"])
            self.assertIn("id:item_duplicate", payload["checks"][0]["duplicates"])

    def test_self_test_is_green(self) -> None:
        self.assertTrue(self_test()["passed"])


if __name__ == "__main__":
    unittest.main()
