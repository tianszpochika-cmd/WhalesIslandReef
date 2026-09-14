import json
import tempfile
import unittest
from pathlib import Path

from tools.d27_r03_character_wildlife_audit import audit


ROOT = Path(__file__).resolve().parents[2]


class D27R03CharacterWildlifeAuditTests(unittest.TestCase):
    def test_real_editor_build_is_structurally_complete(self):
        result = audit(ROOT)
        self.assertEqual("PASS", result["status"])
        self.assertEqual({"npc": 12, "wildlife": 8, "pet": 2}, result["wrapper_counts"])
        self.assertEqual(1, result["r03_attachment_count"])
        self.assertEqual([], result["errors"])

    def test_result_round_trips_as_json(self):
        result = audit(ROOT)
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / "d27_r03.json"
            output.write_text(json.dumps(result, ensure_ascii=False), encoding="utf-8")
            self.assertEqual(result, json.loads(output.read_text(encoding="utf-8")))


if __name__ == "__main__":
    unittest.main()
