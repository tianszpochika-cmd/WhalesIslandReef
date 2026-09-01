#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Tests for the D19 fixed logic-result evidence envelope."""

from __future__ import annotations

import json
import tempfile
import unittest
from pathlib import Path

from tools.d19_logic_result_integrity_audit import EXPECTED_TASK_IDS, audit_d19_logic_results


def _record(task_id: str) -> dict:
    return {
        "task_id": task_id,
        "status": "LOGIC_PASS",
        "updated_at": "2026-08-31T12:00:00+08:00",
        "summary": "valid test result",
        "changed_files": ["game/Assets/Scripts/Example.cs"],
        "verification": [{"command": "python -m unittest", "result": "PASS"}],
        "runtime_deferred": ["runtime validation deferred"],
        "blockers": [],
        "handoff": f"docs/game/design/common/handoffs/D19/{task_id}_LOGIC_HANDOFF.md",
    }


class TestD19LogicResultIntegrityAudit(unittest.TestCase):
    def _make_complete_root(self) -> Path:
        temp = Path(tempfile.mkdtemp())
        for task_id in EXPECTED_TASK_IDS:
            result_dir = temp / "game" / "tools" / "results" / "D19" / task_id
            handoff = temp / "docs" / "game" / "design" / "common" / "handoffs" / "D19" / f"{task_id}_LOGIC_HANDOFF.md"
            result_dir.mkdir(parents=True, exist_ok=True)
            handoff.parent.mkdir(parents=True, exist_ok=True)
            handoff.write_text(f"# {task_id}\n", encoding="utf-8")
            result_path = result_dir / f"{task_id.replace('-', '_')}_LOGIC_RESULT.json"
            result_path.write_text(json.dumps(_record(task_id), ensure_ascii=False), encoding="utf-8")
        return temp

    def test_complete_protocol_passes(self):
        root = self._make_complete_root()
        passed, report = audit_d19_logic_results(root)
        self.assertTrue(passed, report)
        self.assertEqual(report["result_files_found"], 48)
        self.assertEqual(report["logic_pass_count"], 48)

    def test_legacy_shape_is_rejected(self):
        root = self._make_complete_root()
        result_path = root / "game" / "tools" / "results" / "D19" / "D19-L26" / "D19_L26_LOGIC_RESULT.json"
        data = json.loads(result_path.read_text(encoding="utf-8"))
        data["verifications"] = data.pop("verification")
        data["files_written"] = data.pop("changed_files")
        data["handoff_document"] = data.pop("handoff")
        result_path.write_text(json.dumps(data, ensure_ascii=False), encoding="utf-8")
        passed, report = audit_d19_logic_results(root)
        self.assertFalse(passed)
        codes = {item["code"] for item in report["errors"] if item["task_id"] == "D19-L26"}
        self.assertIn("missing_required_fields", codes)
        self.assertIn("legacy_fields", codes)

    def test_missing_task_is_rejected(self):
        root = self._make_complete_root()
        result_path = root / "game" / "tools" / "results" / "D19" / "D19-L48" / "D19_L48_LOGIC_RESULT.json"
        result_path.unlink()
        passed, report = audit_d19_logic_results(root)
        self.assertFalse(passed)
        self.assertTrue(any(item["code"] == "missing_result" and item["task_id"] == "D19-L48" for item in report["errors"]))


if __name__ == "__main__":
    unittest.main()
