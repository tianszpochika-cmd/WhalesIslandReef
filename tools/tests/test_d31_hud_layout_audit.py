import unittest

from tools.d31_hud_layout_audit import audit


class D31HudLayoutAuditTests(unittest.TestCase):
    def test_reference_layout_is_inside_and_non_overlapping(self):
        result = audit()
        self.assertEqual("PASS", result["status"], result["errors"])
        self.assertEqual("STATIC_LOGIC_ONLY", result["acceptance_level"])
        self.assertFalse(result["runtime_proof"])
        self.assertEqual("1920x1080", result["reference_resolution"])

    def test_contract_checks_all_primary_surfaces_and_button_count(self):
        result = audit()
        self.assertEqual(5, result["surface_count"])
        self.assertEqual(8, result["quick_action_button_count"])
        self.assertEqual([], result["errors"])


if __name__ == "__main__":
    unittest.main()
