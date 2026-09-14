import json
import tempfile
import unittest
from pathlib import Path

from tools.d26_environment_asset_audit import audit


ROOT = Path(__file__).resolve().parents[2]
PREFAB = ROOT / "game" / "Assets" / "Prefabs" / "Environment" / "D19" / "Integration" / "R03" / "D19_R03_HarborPresentation.prefab"


class D26EnvironmentAssetAuditTests(unittest.TestCase):
    def test_real_r03_presentation_contains_all_visual_groups(self):
        result = audit(PREFAB)
        self.assertEqual("PASS", result["status"])
        self.assertGreaterEqual(result["renderer_count"], 120)
        self.assertEqual([], result["errors"])
        self.assertTrue(all(group["count"] > 0 for group in result["groups"].values()))

    def test_result_is_json_serializable_and_read_only(self):
        result = audit(PREFAB)
        encoded = json.dumps(result, ensure_ascii=False)
        self.assertIn('"read_only": true', encoded)
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "result.json"
            output.write_text(encoded, encoding="utf-8")
            self.assertEqual(result, json.loads(output.read_text(encoding="utf-8")))


if __name__ == "__main__":
    unittest.main()
