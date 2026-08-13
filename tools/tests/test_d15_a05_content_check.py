import unittest
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d15_a05_content_check import check_batch


class D15A05ContentCheckTests(unittest.TestCase):
    def test_batch_counts_and_boundaries(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry"
        errors, categories, sample_count = check_batch(root)
        self.assertEqual([], errors)
        self.assertEqual(12, len(categories))
        self.assertEqual(50, sample_count)


if __name__ == "__main__":
    unittest.main()
