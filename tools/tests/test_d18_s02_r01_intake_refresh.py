import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path

from tools.d18_s02_r01_intake_refresh import validate_catalog, safe_path


ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"


class D18S02R01IntakeRefreshTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
        cls.entries = cls.catalog["eligible_entries"]

    def test_refresh_catalog_is_pass_and_has_expected_real_counts(self):
        self.assertEqual(self.catalog["task"], "D18-S02-R01")
        self.assertEqual(self.catalog["status"], "PASS")
        self.assertEqual(self.catalog["counts"]["manifest_total"], 28)
        self.assertEqual(self.catalog["counts"]["candidate_file_total"], 1863)
        self.assertEqual(self.catalog["counts"]["intake_eligible_file_total"], 1833)
        self.assertEqual(self.catalog["counts"]["refresh_admitted_file_total"], 159)
        self.assertEqual(validate_catalog(ROOT, self.catalog), [])

    def test_runtime_promotion_is_not_inferred(self):
        authority = self.catalog["authority"]
        self.assertFalse(authority["release_approved_inferred"])
        promotion = self.catalog["refresh"]["runtime_promotion"]
        self.assertFalse(promotion["promotion_performed"])
        self.assertEqual(promotion["local_development_ready_count"], 0)
        self.assertEqual(promotion["release_approved_count"], 0)
        self.assertTrue(all("local_development_ready" not in entry for entry in self.entries))
        self.assertTrue(all("release_approved" not in entry for entry in self.entries))

    def test_upstream_admission_counts_and_exclusions_are_explicit(self):
        admitted = {item["task"]: item["records_admitted"] for item in self.catalog["refresh"]["admitted_from_upstream"]}
        self.assertEqual(admitted, {
            "D18-A04-R01": 24,
            "D18-A04-R02-F": 0,
            "D18-A04-R02-C": 1,
            "D18-A04-R02-B": 1,
            "D18-A05-R01": 133,
        })
        content_ids = {entry.get("content_id") for entry in self.entries}
        self.assertNotIn("animal_quaternius_eagle_blend_only_v01", content_ids)
        self.assertNotIn("asset_quaternius_animated_women_female_alternative_v01", content_ids)
        a05 = next(item for item in self.catalog["refresh"]["upstream_reviews"] if item["task"] == "D18-A05-R01")
        self.assertEqual(a05["records_processed"], 151)

    def test_category_gate_keeps_real_gaps(self):
        categories = self.catalog["category_summary"]
        self.assertEqual(categories["player_visual_core"]["intake_eligible_count"], 1)
        self.assertEqual(categories["npc_visual_core"]["intake_eligible_count"], 4)
        self.assertEqual(categories["wildlife_core"]["intake_eligible_count"], 18)
        self.assertEqual(categories["pet_core"]["intake_eligible_count"], 3)
        self.assertEqual(categories["port_structure_core"]["intake_eligible_count"], 96)
        self.assertEqual(categories["dock_and_shipyard_core"]["intake_eligible_count"], 6)
        self.assertEqual(categories["market_prop_core"]["intake_eligible_count"], 16)
        self.assertEqual(categories["female_or_neutral_human_gap"]["intake_eligible_count"], 0)
        self.assertEqual(categories["sail_rig_gap"]["intake_eligible_count"], 0)
        self.assertEqual(categories["rudder_anchor_gap"]["intake_eligible_count"], 0)

    def test_intake_id_formula_is_stable(self):
        for entry in self.entries[::137]:
            import hashlib

            expected = "intake_" + hashlib.sha256(
                f"{entry['manifest_id']}::{entry['candidate_file_path']}".encode("utf-8")
            ).hexdigest()[:20]
            self.assertEqual(entry["intake_id"], expected)

    def test_tool_replay_without_catalog_write_passes(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            output = Path(temp_dir) / "machine.json"
            command = [
                sys.executable,
                "tools/d18_s02_r01_intake_refresh.py",
                "--project-root",
                ".",
                "--catalog",
                str(CATALOG.relative_to(ROOT)),
                "--output",
                str(output),
                "--generated-on",
                "2026-08-15",
            ]
            completed = subprocess.run(command, cwd=ROOT, capture_output=True, text=True, check=False)
            self.assertEqual(completed.returncode, 0, completed.stdout + completed.stderr)
            result = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(result["counts"]["intake_eligible_file_total"], 1833)
            self.assertEqual(result["audit"]["catalog_validation_errors"], [])

    def test_path_escape_is_rejected(self):
        with self.assertRaises(ValueError):
            safe_path(ROOT, "../outside/file.fbx")


if __name__ == "__main__":
    unittest.main()
