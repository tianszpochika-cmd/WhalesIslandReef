import json
import tempfile
import unittest
from pathlib import Path

from tools.d27_presentation_asset_audit import audit


ROOT = Path(__file__).resolve().parents[2]
PREFABS = ROOT / "game" / "Assets" / "Prefabs"


class D27PresentationAssetAuditTests(unittest.TestCase):
    def test_real_character_and_animal_candidates_pass(self):
        result = audit(PREFABS)
        self.assertEqual("PASS", result["status"])
        self.assertGreaterEqual(result["character_prefab_count"], 10)
        self.assertGreaterEqual(result["animal_prefab_count"], 7)
        self.assertEqual([], result["errors"])

    def test_result_round_trips_as_json(self):
        result = audit(PREFABS)
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "d27.json"
            output.write_text(json.dumps(result), encoding="utf-8")
            self.assertEqual(result, json.loads(output.read_text(encoding="utf-8")))


if __name__ == "__main__":
    unittest.main()
