import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d15_a04_acceptance_check import check


class D15A04AcceptanceTests(unittest.TestCase):
    def test_building_batch_fields_and_counts(self) -> None:
        result = check(Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry")
        self.assertEqual(80, result["building_types"])
        self.assertEqual(180, result["generated_variants"])
        self.assertEqual(640, result["generated_instances"])
        self.assertGreaterEqual(result["historical_warning_entries"], 10)


if __name__ == "__main__":
    unittest.main()

