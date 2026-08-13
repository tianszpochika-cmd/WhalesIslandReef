import unittest
import json
from pathlib import Path

from tools.d15_a02_vegetation_batch import (
    CATEGORY_COUNTS,
    PROFILE_DATA,
    TOTAL_BATCH_COUNT,
    _asset_profile,
    _plant_entry,
    validate_batch,
)


class D15A02VegetationBatchTests(unittest.TestCase):
    def test_frozen_category_quota_is_1200(self):
        self.assertEqual(TOTAL_BATCH_COUNT, sum(CATEGORY_COUNTS.values()))
        self.assertEqual(set(CATEGORY_COUNTS), set(PROFILE_DATA))
        self.assertTrue(all(value > 0 for value in CATEGORY_COUNTS.values()))

    def test_generated_entry_has_asset_habitat_use_lod_wind_and_audit_fields(self):
        entry = _plant_entry(1, "tree", 0)
        self.assertTrue(entry["id"].startswith("plant_"))
        self.assertEqual(entry["category"], "plant")
        self.assertTrue(entry["valid_habitats"])
        self.assertTrue(entry["use_profile"]["resource_outputs"])
        self.assertEqual(entry["lod_profile"]["minimum_lod"], "LOD2")
        self.assertTrue(entry["wind_profile"]["id"])
        self.assertEqual(entry["asset_state"], "placeholder")
        self.assertTrue(entry["model_status"])
        self.assertTrue(entry["material_status"])
        self.assertTrue(entry["authorization"]["commercial_use"])
        self.assertTrue(entry["test_cases"])

    def test_asset_profile_is_explicit_placeholder_not_empty_shell(self):
        asset = _asset_profile("coastal")
        self.assertTrue(asset["model_ref"])
        self.assertEqual(asset["model_status"], "placeholder_primitive_ready")
        self.assertTrue(asset["material_ref"])
        self.assertEqual(asset["material_status"], "source_ready")
        self.assertTrue(asset["lod_profile"]["minimum_lod"])

    def test_checked_in_batch_passes_deep_batch_validation(self):
        result = validate_batch(Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry")
        self.assertGreaterEqual(result["plant_count"], 1000)
        self.assertLessEqual(result["plant_count"], 2000)

    def test_canonical_samples_have_matching_habitat_profiles(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries" / "plant"
        expected = {
            "plant_red_mangrove": ("mangrove", "mangrove_low"),
            "plant_shallow_seaweed": ("nearshore", "nearshore"),
        }
        for content_id, (primary, wind_zone) in expected.items():
            data = json.loads((root / f"{content_id}.json").read_text(encoding="utf-8"))
            self.assertIn(primary, data["valid_habitats"])
            self.assertEqual(primary, data["habitat_profile"]["primary"])
            self.assertEqual(wind_zone, data["wind_profile"]["zone"])


if __name__ == "__main__":
    unittest.main()
