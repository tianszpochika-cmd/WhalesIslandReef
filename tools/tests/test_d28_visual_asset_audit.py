import unittest
from pathlib import Path

from tools.d28_visual_asset_audit import audit


ROOT = Path(__file__).resolve().parents[2]


class D28VisualAssetAuditTests(unittest.TestCase):
    def test_real_water_ship_weather_candidates_pass(self):
        result = audit(ROOT / "game/Assets")
        self.assertEqual("PASS", result["status"])
        self.assertEqual([], result["errors"])
        self.assertGreater(result["water_file_count"], 0)


if __name__ == "__main__":
    unittest.main()
