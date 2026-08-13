import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d15_a03_animal_batch_check import EXPECTED_COUNTS, validate  # noqa: E402


class D15A03AnimalBatchTests(unittest.TestCase):
    def test_batch_has_three_hundred_records_and_all_categories(self):
        result = validate(ROOT / "game" / "Assets" / "Resources" / "ContentRegistry")
        self.assertEqual(300, result["entries"])
        self.assertEqual(EXPECTED_COUNTS, result["category_counts"])
        self.assertEqual(300, result["test_ids"])


if __name__ == "__main__":
    unittest.main()
