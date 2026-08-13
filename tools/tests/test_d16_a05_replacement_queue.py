import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_a05_replacement_queue import build_queue, render_markdown  # noqa: E402


class D16A05ReplacementQueueTests(unittest.TestCase):
    def test_queue_only_contains_placeholders_and_reports_real_gates(self):
        with tempfile.TemporaryDirectory() as directory:
            project_root = Path(directory)
            root = project_root / "game"
            assets = root / "Assets"
            assets.mkdir(parents=True)
            (assets / "placeholder.fbx").write_bytes(b"placeholder")
            (root / "final.prefab").write_text("final", encoding="utf-8")
            (root / "review.md").write_text("review evidence", encoding="utf-8")
            (root / "placeholder.json").write_text(json.dumps({
                "id": "asset_placeholder_test",
                "asset_state": "placeholder",
                "production_stage": "prototype_placeholder",
                "historical_tag": "inferred",
                "historical_review": "Pending",
                "resource_path": "game/Assets/placeholder.fbx",
                "runtime_prefab_path": "game/final.prefab",
                "source": {"kind": "project-authored"},
                "license": {"status": "internal-authored"},
                "review_evidence_refs": ["review.md"],
                "test_cases": [],
            }), encoding="utf-8")
            (root / "integrated.json").write_text(json.dumps({
                "id": "asset_integrated_test",
                "asset_state": "integrated",
            }), encoding="utf-8")
            queue = build_queue(root, batch_size=1)
            markdown = render_markdown(queue)
        self.assertEqual(queue["status"], "BLOCKED")
        self.assertEqual(queue["placeholder_count"], 1)
        record = queue["records"][0]
        self.assertEqual(record["id"], "asset_placeholder_test")
        self.assertIn("approved_history_or_content_review", record["requirements"])
        self.assertIn("replace_placeholder_source_and_prefab", record["requirements"])
        self.assertIn("visual_load_collision_anchor_and_save_tests", record["requirements"])
        self.assertIn("asset_placeholder_test", markdown)

    def test_queue_batches_are_stable_and_does_not_write_registry(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for entry_id in ("asset_b", "asset_a", "item_c"):
                (root / f"{entry_id}.json").write_text(json.dumps({
                    "id": entry_id,
                    "asset_state": "placeholder",
                    "production_stage": "prototype_placeholder",
                }), encoding="utf-8")
            before = {path.name: path.read_text(encoding="utf-8") for path in root.glob("*.json")}
            queue = build_queue(root, batch_size=1)
            after = {path.name: path.read_text(encoding="utf-8") for path in root.glob("*.json")}
        self.assertEqual(queue["placeholder_count"], 3)
        self.assertEqual(queue["batch_count"], 3)
        self.assertEqual([batch["record_ids"] for batch in queue["batches"]], [["asset_a"], ["asset_b"], ["item_c"]])
        self.assertEqual(before, after)

    def test_vertical_slice_placeholders_are_first_without_approving_them(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for entry_id in (
                "item_other",
                "building_saint_marys_dock",
                "asset_ph_ship_fishing",
                "asset_ph_crew_male",
                "island_saint_marys",
            ):
                (root / f"{entry_id}.json").write_text(json.dumps({
                    "id": entry_id,
                    "asset_state": "placeholder",
                    "production_stage": "prototype_placeholder",
                }), encoding="utf-8")
            queue = build_queue(root, batch_size=50)

        self.assertEqual(queue["status"], "BLOCKED")
        self.assertEqual(queue["priority_counts"], {"general": 1, "vertical_slice": 4})
        self.assertEqual(queue["batches"][0]["queue_scope"], "vertical_slice_first")
        self.assertEqual(queue["batches"][0]["record_ids"], [
            "asset_ph_crew_male",
            "asset_ph_ship_fishing",
            "building_saint_marys_dock",
            "island_saint_marys",
        ])
        self.assertEqual(queue["batches"][1]["record_ids"], ["item_other"])
        self.assertTrue(all(record["asset_state"] == "placeholder" for record in queue["records"]))
        self.assertTrue(all(record["replacement_policy"].startswith("Replace only") for record in queue["records"]))

    def test_missing_provenance_reference_is_reported_without_writing_registry(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            entry_path = root / "asset_reference_gap.json"
            entry_path.write_text(json.dumps({
                "id": "asset_reference_gap",
                "asset_state": "placeholder",
                "production_stage": "prototype_placeholder",
                "source_refs": ["missing-source.md"],
                "review_evidence_refs": ["https://example.invalid/review"],
                "source": {"kind": "project-authored", "reference": "missing-source.md"},
                "license": {"status": "internal-authored", "reference": "missing-license.md"},
            }), encoding="utf-8")
            before = entry_path.read_text(encoding="utf-8")
            queue = build_queue(root)
            after = entry_path.read_text(encoding="utf-8")

        self.assertEqual(before, after)
        record = queue["records"][0]
        self.assertIn("provenance_and_review_reference_files", record["requirements"])
        self.assertEqual(record["evidence"]["missing_local_references"], [
            "missing-license.md",
            "missing-source.md",
        ])

    def test_repository_relative_reference_resolves_from_project_root_and_ids_are_not_paths(self):
        with tempfile.TemporaryDirectory() as directory:
            project_root = Path(directory)
            root = project_root / "game"
            (project_root / "docs").mkdir(parents=True)
            (project_root / "docs" / "source.md").write_text("source\n", encoding="utf-8")
            entry_path = root / "asset_reference_ok.json"
            root.mkdir(parents=True)
            entry_path.write_text(json.dumps({
                "id": "asset_reference_ok",
                "asset_state": "placeholder",
                "production_stage": "prototype_placeholder",
                "source_refs": ["docs/source.md"],
                "review_evidence_refs": ["D15-A07-AUTH-001"],
                "source": {"kind": "project-authored", "reference": "docs/source.md"},
                "license": {"status": "internal-authored"},
            }), encoding="utf-8")
            queue = build_queue(root)

        record = queue["records"][0]
        self.assertNotIn("provenance_and_review_reference_files", record["requirements"])
        self.assertEqual(record["evidence"]["missing_local_references"], [])


if __name__ == "__main__":
    unittest.main()
