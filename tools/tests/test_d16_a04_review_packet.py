import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_a04_review_packet import build_packet, render_markdown  # noqa: E402


class D16A04ReviewPacketTests(unittest.TestCase):
    def _write_entry(self, root: Path, payload: dict) -> Path:
        path = root / f"{payload['id']}.json"
        path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
        return path

    def test_packet_is_human_only_and_does_not_mutate_registry(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            game = project / "game"
            game.mkdir()
            evidence = project / "review.md"
            evidence.write_text("A human reviewer must still make the decision.", encoding="utf-8")
            entry = self._write_entry(game, {
                "id": "asset_pending_packet",
                "asset_state": "placeholder",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "content_warning_ids": [],
                "skip_allowed": False,
                "source_refs": ["review.md"],
                "review_evidence_refs": ["review.md"],
                "source": {"kind": "project-authored-placeholder"},
                "license": {"status": "internal-authored"},
            })
            before = entry.read_text(encoding="utf-8")
            packet = build_packet(game, captured_at="2026-08-13")
            after = entry.read_text(encoding="utf-8")

        self.assertEqual(packet["status"], "HUMAN_DECISION_REQUIRED")
        self.assertEqual(packet["pending_count"], 1)
        self.assertEqual(packet["reference_gap_count"], 0)
        self.assertEqual(packet["decision_evidence_file_ready_count"], 1)
        self.assertEqual(packet["records"][0]["decision_template"]["decision"], "Pending")
        self.assertTrue(packet["records"][0]["human_decision_required"])
        self.assertEqual(before, after)

    def test_missing_reference_is_reported_without_becoming_an_approval(self):
        with tempfile.TemporaryDirectory() as directory:
            game = Path(directory) / "game"
            game.mkdir()
            entry = self._write_entry(game, {
                "id": "item_missing_packet_evidence",
                "historical_tag": "historical",
                "historical_review": "Pending",
                "content_warning_ids": [],
                "skip_allowed": False,
                "source_refs": ["missing-source.md"],
                "review_evidence_refs": ["missing-review.md"],
            })
            before = entry.read_text(encoding="utf-8")
            packet = build_packet(game, captured_at="2026-08-13")
            after = entry.read_text(encoding="utf-8")

        self.assertEqual(packet["status"], "BLOCKED_REFERENCE_GAP")
        self.assertEqual(packet["reference_gap_count"], 2)
        self.assertEqual(packet["records"][0]["decision_template"]["decision"], "Pending")
        self.assertEqual(before, after)

    def test_sensitive_packet_exposes_warning_skip_and_presentation_fields(self):
        with tempfile.TemporaryDirectory() as directory:
            project = Path(directory)
            game = project / "game"
            game.mkdir()
            (project / "sensitive.md").write_text("Review evidence remains pending.", encoding="utf-8")
            self._write_entry(game, {
                "id": "quest_sensitive_packet",
                "historical_tag": "historical",
                "historical_review": "Pending",
                "tags": ["gambling"],
                "content_warning_ids": ["gambling"],
                "skip_allowed": True,
                "review_evidence_refs": ["sensitive.md"],
            })
            packet = build_packet(game, captured_at="2026-08-13")

        template = packet["records"][0]["decision_template"]
        self.assertIn("warning_ids", template)
        self.assertIn("skip_allowed", template)
        self.assertIn("presentation_levels", template)
        self.assertIn("ApprovedWithWarning", packet["decision_options"])
        self.assertIn("decision_basis", render_markdown(packet))

    def test_current_project_packet_covers_all_pending_records(self):
        packet = build_packet(ROOT / "game", captured_at="2026-08-13")
        self.assertEqual(packet["pending_count"], 267)
        self.assertEqual(packet["batch_count"], 12)
        self.assertEqual(packet["reference_gap_count"], 0)
        self.assertTrue(all(record["decision_template"]["decision"] == "Pending" for record in packet["records"]))


if __name__ == "__main__":
    unittest.main()
