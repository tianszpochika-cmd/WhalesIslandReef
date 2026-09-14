import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d15_a04_acceptance_check import _is_reviewed_slice_override, check


class D15A04AcceptanceTests(unittest.TestCase):
    def test_building_batch_fields_and_counts(self) -> None:
        result = check(Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry")
        self.assertEqual(80, result["building_types"])
        self.assertEqual(180, result["generated_variants"])
        self.assertEqual(640, result["generated_instances"])
        self.assertGreaterEqual(result["historical_warning_entries"], 10)

    def test_reviewed_slice_override_is_narrow(self) -> None:
        valid = {
            "asset_state": "integrated",
            "production_stage": "slice_final",
            "model_ref": "game/Assets/Art/SliceFinal/D16C08/dock.fbx",
            "runtime_prefab_path": "game/Assets/Prefabs/SliceFinal/D16C08/dock.prefab",
            "historical_review": "ApprovedWithWarning",
            "review_evidence_refs": ["docs/game/design/common/reviews/D16_C08_SLICE_REVIEW.md"],
        }
        self.assertTrue(_is_reviewed_slice_override(valid))
        valid["runtime_prefab_path"] = ""
        self.assertFalse(_is_reviewed_slice_override(valid))


if __name__ == "__main__":
    unittest.main()
