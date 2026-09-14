import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ROUTE_PROBE = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs"


class VCG01RuntimeProbeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = ROUTE_PROBE.read_text(encoding="utf-8")

    def test_observes_all_required_vc_g01_shortcuts(self):
        for token in (
            "public bool key_w;",
            "public bool key_a;",
            "public bool key_s;",
            "public bool key_d;",
            "public bool key_e;",
            "public bool key_i;",
            "public bool key_m;",
            "public bool key_t;",
            "public bool key_escape;",
            "public float player_yaw;",
            "public float yaw_delta_from_start;",
        ):
            self.assertIn(token, self.source)
        for token in (
            "Input.GetKey(KeyCode.I)",
            "Input.GetKey(KeyCode.M)",
            "Input.GetKeyDown(KeyCode.I)",
            "Input.GetKeyDown(KeyCode.M)",
            "Input.GetKeyDown(KeyCode.Escape)",
        ):
            self.assertIn(token, self.source)

    def test_classifies_warehouse_market_and_close_edges_for_route_evidence(self):
        for token in (
            'AddEvent("warehouse_input_observed"',
            'AddEvent("market_input_observed"',
            'AddEvent("tavern_input_observed"',
            'AddEvent("close_input_observed"',
            'AddEvent("interaction_input_observed", "E/B/T/V/I/M',
            'AddInputEdgeEvent("warehouse_input_edge_observed"',
            'AddInputEdgeEvent("market_input_edge_observed"',
            'AddInputEdgeEvent("tavern_input_edge_observed"',
            '"warehouse_page_open_observed"',
            '"market_page_open_observed"',
            '"tavern_page_open_observed"',
        ):
            self.assertIn(token, self.source)

    def test_records_input_system_axis_and_movement_permission_diagnostics(self):
        for token in (
            "public float d16_move_x;",
            "public float d16_move_y;",
            "public bool input_system_keyboard_present;",
            "public bool input_system_w_pressed;",
            "public bool input_system_a_pressed;",
            "public bool input_system_s_pressed;",
            "public bool input_system_d_pressed;",
            "public bool application_focused;",
            "public bool gameplay_input_allowed;",
            "public bool primary_input_allowed;",
            "public bool event_system_found;",
            "public bool ui_navigation_events_enabled;",
            "public bool ui_move_action_bound;",
            "public string selected_ui_object;",
            "public bool gameplay_ui_navigation_suppressed;",
            "public string movement_input_source;",
            "public bool legacy_movement_fallback_active;",
            "public bool turning_observed;",
            "public bool d3_owner_ready;",
            "public bool d10_owner_ready;",
            "public bool d29_warehouse_panel_found;",
            "public bool warehouse_visible;",
            "public bool market_visible;",
            "public bool tavern_visible;",
            "D16InputRouter.MoveAxis()",
            "RuntimeHudCoordinator.PrimaryInputAllowed",
            "Quaternion.Angle(_firstRotation, _player.rotation)",
            'AddEvent("turning_observed"',
            'AddEvent("gameplay_ui_navigation_suppressed_observed"',
            'AddEvent("gameplay_ui_navigation_conflict_observed"',
            "EventSystem.current",
            "InputSystemUIInputModule",
            "eventSystem.sendNavigationEvents",
            "uiInputModule.move",
            "eventSystem.currentSelectedGameObject",
        ):
            self.assertIn(token, self.source)

    def test_captures_owner_state_on_real_input_edges_for_bootstrap_rejection(self):
        for token in (
            "public bool runtime_state_captured;",
            "public bool d3_owner_ready;",
            "public bool d10_owner_ready;",
            "public bool warehouse_visible;",
            "public bool market_visible;",
            "public bool tavern_visible;",
            "-d19-r03-bootstrap-reject-rerun1",
            "bootstrap-key-i",
            "BootstrapRejectFileSuffix",
            "Bootstrap owner-not-ready rejection window active",
            "runtime_state_captured = true",
        ):
            self.assertIn(token, self.source)

    def test_probe_remains_read_only_and_does_not_invoke_gameplay_actions(self):
        self.assertIn("Read-only runtime evidence probe", self.source)
        self.assertIn("never invokes gameplay methods or writes game/save state", self.source)
        self.assertNotIn("Invoke", self.source)
        self.assertNotIn("SaveSystem", self.source)

    def test_q29_route_flag_has_unique_probe_and_screenshot_namespace(self):
        for token in (
            "-d19-r03-route-rerun59",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN59.json",
            "route_rerun59_",
            "-d19-r03-route-rerun62",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN62.json",
            "route_rerun62_",
            "-d19-r03-route-rerun85",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN85.json",
            "route_rerun85_",
            "-d19-r03-route-rerun86",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN86.json",
            "route_rerun86_",
            "-d19-r03-route-rerun90",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN90.json",
            "route_rerun90_",
            "-d19-r03-route-rerun91",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN91.json",
            "route_rerun91_",
            "-d19-r03-route-rerun92",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN92.json",
            "route_rerun92_",
            "-d19-r03-route-rerun94",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN94.json",
            "route_rerun94_",
            "-d19-r03-route-rerun95",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN95.json",
            "route_rerun95_",
            "-d19-r03-route-rerun96",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN96.json",
            "route_rerun96_",
            "-d19-r03-route-rerun97",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN97.json",
            "route_rerun97_",
            "-d19-r03-route-rerun100",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN100.json",
            "route_rerun100_",
            "-d19-r03-route-rerun99",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN99.json",
            "route_rerun99_",
            "-d19-r03-route-rerun98",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN98.json",
            "route_rerun98_",
            "-d19-r03-route-rerun93",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN93.json",
            "route_rerun93_",
            "-d19-r03-route-rerun89",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN89.json",
            "route_rerun89_",
            "-d19-r03-route-rerun88",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN88.json",
            "route_rerun88_",
            "-d19-r03-route-rerun87",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN87.json",
            "route_rerun87_",
        ):
            self.assertIn(token, self.source)


if __name__ == "__main__":
    unittest.main()
