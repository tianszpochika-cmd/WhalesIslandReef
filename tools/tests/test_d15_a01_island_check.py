import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d15_a01_island_check import EXPECTED_TEMPLATE_COUNTS, _is_reviewed_slice_override, validate_catalog  # noqa: E402


class D15A01IslandCatalogTests(unittest.TestCase):
    def test_d16_c08_slice_override_is_evidence_bound(self):
        entry = {
            "id": "island_saint_marys",
            "asset_state": "integrated",
            "production_stage": "slice_final",
            "model_ref": "game/Assets/Art/SliceFinal/D16C08/sand.fbx",
            "runtime_prefab_path": "game/Assets/Prefabs/SliceFinal/D16C08/sand.prefab",
            "lod_profile": "island_slice_sand_lod3",
            "historical_review": "ApprovedWithWarning",
            "review_evidence_refs": ["docs/game/design/common/reviews/D16_C08_SLICE_REVIEW.md"],
        }
        self.assertTrue(_is_reviewed_slice_override(entry))
        entry["review_evidence_refs"] = []
        self.assertFalse(_is_reviewed_slice_override(entry))

    def test_catalog_has_fifty_islands_and_eight_templates(self):
        count, sample_count, template_counts = validate_catalog(
            ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
        )
        self.assertEqual(50, count)
        self.assertGreaterEqual(sample_count, 10)
        self.assertEqual(EXPECTED_TEMPLATE_COUNTS, template_counts)


if __name__ == "__main__":
    unittest.main()
