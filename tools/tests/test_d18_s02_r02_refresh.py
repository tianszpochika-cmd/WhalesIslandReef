import json
import unittest
from pathlib import Path

from tools.d18_s02_r02_refresh import refresh


ROOT = Path(__file__).resolve().parents[2]


class D18S02R02RefreshTests(unittest.TestCase):
    def test_refresh_is_repeatable_and_preserves_runtime_boundaries(self):
        catalog, outputs = refresh(ROOT, "2026-08-15")
        self.assertEqual(outputs["machine"]["status"], "PASS")
        self.assertEqual(catalog["counts"]["manifest_total"], 30)
        self.assertEqual(catalog["counts"]["candidate_file_total"], 1895)
        self.assertEqual(catalog["counts"]["intake_eligible_file_total"], 1865)
        self.assertEqual(catalog["counts"]["duplicate_manifest_file_references"], 0)
        self.assertEqual(len(catalog["eligible_entries"]), 1865)
        self.assertEqual(outputs["gate"]["humanoid_valid_avatar_count"], 1)
        self.assertTrue(outputs["gate"]["player_humanoid_candidate_available"])
        self.assertEqual(outputs["machine"]["local_development_ready_count"], 0)
        self.assertEqual(outputs["machine"]["release_approved_count"], 0)
        self.assertTrue(all("local_development_ready" not in item for item in catalog["eligible_entries"]))
        self.assertTrue(all("release_approved" not in item for item in catalog["eligible_entries"]))

    def test_catalog_and_gate_on_disk_match_r02_result(self):
        catalog = json.loads(
            (ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json").read_text(encoding="utf-8")
        )
        gate = json.loads(
            (ROOT / "game/tools/results/D18/D18-S02-R02/D18_R02_PLAYER_CONSUMPTION_GATE.json").read_text(encoding="utf-8")
        )
        self.assertEqual(catalog["task"], "D18-S02-R02")
        self.assertEqual(catalog["status"], "PASS")
        self.assertEqual(gate["task"], "D18-S02-R02")
        self.assertTrue(gate["player_humanoid_candidate_available"])
        self.assertEqual(gate["state_boundaries"]["local_development_ready_count"], 0)
        self.assertEqual(gate["state_boundaries"]["release_approved_count"], 0)


if __name__ == "__main__":
    unittest.main()
