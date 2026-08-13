import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d15_a01_island_check import EXPECTED_TEMPLATE_COUNTS, validate_catalog  # noqa: E402


class D15A01IslandCatalogTests(unittest.TestCase):
    def test_catalog_has_fifty_islands_and_eight_templates(self):
        count, sample_count, template_counts = validate_catalog(
            ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
        )
        self.assertEqual(50, count)
        self.assertGreaterEqual(sample_count, 10)
        self.assertEqual(EXPECTED_TEMPLATE_COUNTS, template_counts)


if __name__ == "__main__":
    unittest.main()
