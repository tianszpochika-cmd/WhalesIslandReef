import unittest

from tools.d31_interaction_logic_audit import audit


class D31InteractionLogicAuditTests(unittest.TestCase):
    def test_source_level_audit_passes(self):
        result = audit()
        self.assertEqual("PASS", result["status"], result["errors"])
        self.assertEqual("STATIC_LOGIC_ONLY", result["acceptance_level"])
        self.assertFalse(result["runtime_proof"])

    def test_matrix_coverage_is_explicit(self):
        result = audit()
        self.assertEqual(29, result["visible_entry_count"])
        self.assertEqual(5, result["keyboard_only_count"])

    def test_d29_quality_preview_keyboard_entry_is_declared(self):
        result = audit()
        self.assertEqual("PASS", result["status"], result["errors"])
        self.assertEqual(5, result["keyboard_only_count"])

    def test_modal_interaction_surfaces_have_focus_scope(self):
        result = audit()
        self.assertEqual(["D10", "D29-Warehouse", "D4", "D9"], result["focus_surfaces"])

    def test_player_probe_is_opt_in_and_keeps_runtime_boundary(self):
        result = audit()
        self.assertEqual("PASS", result["status"], result["errors"])
        self.assertFalse(result["runtime_proof"])

    def test_warehouse_close_shortcuts_precede_primary_gate(self):
        result = audit()
        self.assertTrue(result["warehouse_close_precedence"], result["errors"])


if __name__ == "__main__":
    unittest.main()
