import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_external_gate_audit import audit  # noqa: E402


class D16ExternalGateAuditTests(unittest.TestCase):
    def test_missing_external_evidence_stays_blocked_and_never_passes_by_default(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            evidence = root / "external"
            (project / "Assets" / "Resources").mkdir(parents=True)
            (project / "Assets" / "Resources" / "D16ReleaseManifest.json").write_text(
                json.dumps({"steam": {"upload_authorized": False}}), encoding="utf-8"
            )
            result = audit(project, evidence)

        self.assertEqual(result["status"], "BLOCKED")
        self.assertFalse(result["all_external_gates_pass"])
        self.assertEqual(len(result["gates"]), 6)
        self.assertIn("D16-A01", result["blocking_reasons"])
        self.assertIn("steam_upload_not_authorized", result["blocking_reasons"])
        self.assertTrue(result["policy"]["this_tool_never_approves_content"])

    def test_synthetic_or_editor_only_evidence_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            evidence = root / "external"
            evidence.mkdir(parents=True)
            (project / "Assets" / "Resources").mkdir(parents=True)
            (project / "Assets" / "Resources" / "D16ReleaseManifest.json").write_text(
                json.dumps({"steam": {"upload_authorized": True}}), encoding="utf-8"
            )
            payload = {
                "status": "PASS",
                "evidence_kind": "editor_playmode",
                "operator": "test",
                "device_id": "editor",
                "captured_at": "2026-08-12T00:00:00Z",
                "raw_artifacts": ["editor.xml"],
                "synthetic": True,
                "editor_only": True,
                "duration_seconds": 60,
                "fps_min": 30,
                "fps_avg": 60,
                "memory_peak_mb": 100,
                "gc_alloc_bytes": 0,
                "same_screen_count": 10,
                "streaming_load_restore_pass": True,
                "save_restore_pass": True,
            }
            (evidence / "d16_a01_target_device.json").write_text(
                json.dumps(payload), encoding="utf-8"
            )
            result = audit(project, evidence)

        self.assertEqual(result["gates"]["D16-A01"]["status"], "BLOCKED")
        self.assertIn("synthetic_evidence_rejected", result["gates"]["D16-A01"]["errors"])
        self.assertIn("editor_only_evidence_rejected", result["gates"]["D16-A01"]["errors"])

    def test_valid_a01_envelope_is_only_one_gate_and_does_not_close_release(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            evidence = root / "external"
            evidence.mkdir(parents=True)
            (project / "Assets" / "Resources").mkdir(parents=True)
            (project / "Assets" / "Resources" / "D16ReleaseManifest.json").write_text(
                json.dumps({"steam": {"upload_authorized": False}}), encoding="utf-8"
            )
            payload = {
                "status": "PASS",
                "evidence_kind": "physical_target_device",
                "operator": "qa-001",
                "device_id": "target-pc-001",
                "captured_at": "2026-08-12T00:00:00Z",
                "raw_artifacts": ["target.log", "target-performance.json"],
                "duration_seconds": 60,
                "fps_min": 30,
                "fps_avg": 59,
                "memory_peak_mb": 1024,
                "gc_alloc_bytes": 100,
                "same_screen_count": 40,
                "streaming_load_restore_pass": True,
                "save_restore_pass": True,
            }
            (evidence / "target.log").write_text("target-device-log\n", encoding="utf-8")
            (evidence / "target-performance.json").write_text(
                "{\"fps_avg\":59}\n", encoding="utf-8"
            )
            (evidence / "d16_a01_target_device.json").write_text(
                json.dumps(payload), encoding="utf-8"
            )
            result = audit(project, evidence)

        self.assertEqual(result["gates"]["D16-A01"]["status"], "PASS")
        self.assertFalse(result["all_external_gates_pass"])
        self.assertEqual(result["status"], "BLOCKED")

    def test_missing_raw_artifact_is_rejected_even_when_envelope_fields_are_valid(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            evidence = root / "external"
            evidence.mkdir(parents=True)
            (project / "Assets" / "Resources").mkdir(parents=True)
            (project / "Assets" / "Resources" / "D16ReleaseManifest.json").write_text(
                json.dumps({"steam": {"upload_authorized": False}}), encoding="utf-8"
            )
            payload = {
                "status": "PASS",
                "evidence_kind": "physical_target_device",
                "operator": "qa-001",
                "device_id": "target-pc-001",
                "captured_at": "2026-08-12T00:00:00Z",
                "raw_artifacts": ["missing-target.log"],
                "duration_seconds": 60,
                "fps_min": 30,
                "fps_avg": 59,
                "memory_peak_mb": 1024,
                "gc_alloc_bytes": 100,
                "same_screen_count": 40,
                "streaming_load_restore_pass": True,
                "save_restore_pass": True,
            }
            (evidence / "d16_a01_target_device.json").write_text(
                json.dumps(payload), encoding="utf-8"
            )
            result = audit(project, evidence)

        self.assertEqual(result["gates"]["D16-A01"]["status"], "BLOCKED")
        self.assertIn(
            "raw_artifact_missing:missing-target.log",
            result["gates"]["D16-A01"]["errors"],
        )

    def test_relative_raw_artifact_cannot_escape_evidence_root(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            project = root / "game"
            evidence = root / "external"
            evidence.mkdir(parents=True)
            (root / "outside.log").write_text("outside\n", encoding="utf-8")
            (project / "Assets" / "Resources").mkdir(parents=True)
            (project / "Assets" / "Resources" / "D16ReleaseManifest.json").write_text(
                json.dumps({"steam": {"upload_authorized": False}}), encoding="utf-8"
            )
            payload = {
                "status": "PASS",
                "evidence_kind": "physical_target_device",
                "operator": "qa-001",
                "device_id": "target-pc-001",
                "captured_at": "2026-08-12T00:00:00Z",
                "raw_artifacts": ["../outside.log"],
                "duration_seconds": 60,
                "fps_min": 30,
                "fps_avg": 59,
                "memory_peak_mb": 1024,
                "gc_alloc_bytes": 100,
                "same_screen_count": 40,
                "streaming_load_restore_pass": True,
                "save_restore_pass": True,
            }
            (evidence / "d16_a01_target_device.json").write_text(
                json.dumps(payload), encoding="utf-8"
            )
            result = audit(project, evidence)

        self.assertEqual(result["gates"]["D16-A01"]["status"], "BLOCKED")
        self.assertIn(
            "raw_artifact_outside_evidence_root:../outside.log",
            result["gates"]["D16-A01"]["errors"],
        )


if __name__ == "__main__":
    unittest.main()
