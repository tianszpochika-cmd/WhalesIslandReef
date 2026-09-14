import json
import unittest
from pathlib import Path

from tools.d29_d30_interaction_matrix_audit import audit, load_matrix


class D29D30InteractionMatrixTests(unittest.TestCase):
    def test_matrix_has_unique_button_ids_and_expected_surfaces(self):
        matrix = load_matrix()
        buttons = matrix["buttons"]
        self.assertGreaterEqual(len(buttons), 29)
        ids = [entry["id"] for entry in buttons]
        self.assertEqual(len(ids), len(set(ids)))
        self.assertEqual({"D29", "D29-Warehouse", "D4", "D9"}, {entry["surface"] for entry in buttons})

    def test_every_entry_declares_owner_and_feedback(self):
        matrix = load_matrix()
        for entry in matrix["buttons"] + matrix["keyboard_only_controls"]:
            self.assertTrue(entry.get("owner", entry.get("surface")))
            self.assertTrue(entry["feedback_markers"])

    def test_read_only_audit_passes(self):
        result = audit()
        self.assertEqual("PASS", result["status"], result["errors"])
        self.assertEqual(29, result["button_count"])
        self.assertEqual(5, result["keyboard_only_count"])

    def test_d29_quality_preview_is_in_keyboard_only_matrix(self):
        matrix = load_matrix()
        matches = [
            entry for entry in matrix["keyboard_only_controls"]
            if entry.get("surface") == "D29" and entry.get("input") == "Q"
        ]
        self.assertEqual(1, len(matches))
        self.assertEqual("CycleQuality", matches[0]["callback"])

    def test_matrix_is_valid_json_and_sources_are_relative(self):
        path = Path("docs/game/design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json")
        parsed = json.loads(path.read_text(encoding="utf-8"))
        for entry in parsed["buttons"]:
            self.assertFalse(Path(entry["source"]).is_absolute())


if __name__ == "__main__":
    unittest.main()
