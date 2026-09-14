import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools/vc_g01_runtime_input_probe.ps1"


class VcG01RuntimeInputProbeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = SCRIPT.read_text(encoding="utf-8-sig")

    def test_uses_real_native_keyboard_and_mouse_events(self):
        self.assertIn("keybd_event", self.source)
        self.assertIn("SendInput", self.source)
        self.assertIn("UnionPadding", self.source)
        self.assertIn("LastPressUsedSendInput", self.source)
        self.assertIn("mouse_event", self.source)
        self.assertIn("SetForegroundWindow", self.source)
        self.assertIn("foregroundThreadId", self.source)
        self.assertIn("AttachThreadInput(currentThreadId, foregroundThreadId", self.source)
        self.assertIn("keybd_event(0x12", self.source)
        self.assertIn("SetProcessDPIAware", self.source)
        self.assertIn("ClickClient", self.source)

    def test_exercises_required_vc_g01_shortcuts(self):
        for marker in (
            "board_or_reject_e",
            "ship_control_w",
            "ship_turn_a",
            "ship_control_proof",
            "disembark_or_repeat_e",
            "move_w",
            "turn_or_strafe_a",
            "turn_or_strafe_d",
            "move_s",
            "reject_e_far_from_ship",
            "warehouse_open_i",
            "market_open_m",
            "tavern_open_t",
            "pause_open_after_warehouse_escape",
            "pause_recover_after_warehouse",
            "pause_open_escape",
            "pause_recover_escape",
        ):
            self.assertIn(marker, self.source)

    def test_keeps_probe_read_only_and_does_not_invoke_gameplay_methods(self):
        self.assertIn("probe remains read-only", self.source)
        self.assertIn("real native Win32 keyboard/mouse events", self.source)
        self.assertNotIn("TryToggleShip", self.source)
        self.assertNotIn("RequestTrade", self.source)
        self.assertNotIn("OpenSocialPage", self.source)

    def test_rejects_concurrent_known_player_and_closes_own_process(self):
        self.assertIn("拒绝已有同一路径 Player 并发实例", self.source)
        self.assertIn("$process.CloseMainWindow()", self.source)
        self.assertIn("Stop-Process -Id $process.Id -Force", self.source)

    def test_persists_machine_summary_under_visual_clean_results(self):
        self.assertIn("VC_G01_RUNTIME_INPUT_20260906.json", self.source)
        self.assertIn('[string]$RouteFlag = "-d19-r03-route-rerun48"', self.source)
        self.assertIn('[string]$EvidenceTag = ""', self.source)
        self.assertIn("[switch]$UseKeyboardMenuNavigation", self.source)
        self.assertIn("Tab = [byte]0x09", self.source)
        self.assertIn("main_menu_new_game_keyboard_submit", self.source)
        self.assertIn("new_game_confirm_mouse_after_keyboard", self.source)
        self.assertIn("$isolatedRouteRun", self.source)
        self.assertIn("VC_G01_RUNTIME_INPUT_{0}_20260906.json", self.source)
        self.assertIn("D19_R03_A01_RUNTIME_ROUTE_PROBE_{0}.json", self.source)
        self.assertIn("VC_G01_PLAYER_RUNTIME_INPUT_{0}_20260906.log", self.source)
        self.assertIn("runtime_input_{0}_20260906", self.source)
        self.assertIn("runtime_route_proof = $routeProbeProof", self.source)
        self.assertIn("business_interaction_proof = $businessInteractionProof", self.source)
        self.assertIn("movement_observation_proof", self.source)
        self.assertIn("far_ship_reject_proof", self.source)
        self.assertIn("pause_recovery_proof", self.source)
        self.assertIn("Get-PageStateMatrix", self.source)
        self.assertIn("interaction_matrix_proof", self.source)
        self.assertIn("duplicate_i_toggle_proof", self.source)
        self.assertIn("duplicate_m_stable_page_proof", self.source)
        self.assertIn("duplicate_t_stable_page_proof", self.source)
        self.assertIn("owner_not_ready_reject_proof", self.source)
        self.assertIn("[switch]$WaterProbe", self.source)
        self.assertIn("move_s_into_water", self.source)
        self.assertIn("move_s_away_from_ship", self.source)
        self.assertIn("swimming_water_route", self.source)
        self.assertIn("probe_swimming_observations", self.source)
        self.assertIn("swimming_observation_proof", self.source)
        self.assertIn("GetWindowThreadProcessId", self.source)
        self.assertIn("$routeProbeTag", self.source)
        self.assertIn("-rerun(\\d+)$", self.source)

    def test_records_whether_x64_send_input_accepted_each_key_event(self):
        self.assertIn("send_input_accepted = $nativeAccepted", self.source)
        self.assertIn("bool downUsedSendInput;", self.source)
        self.assertIn("bool upUsedSendInput;", self.source)

    def test_uses_correct_unity_player_log_location_and_captures_window_states(self):
        self.assertIn("AppData\\LocalLow\\Whalebone Studio\\WhalesIslandReef\\Player.log", self.source)
        self.assertIn("Capture-PlayerWindow", self.source)
        self.assertIn("screenshot_directory", self.source)

    def test_records_far_ship_reject_from_read_only_feedback(self):
        self.assertIn('feedback_code -eq "ship_too_far"', self.source)
        self.assertIn("Capture-PlayerWindow -Name \"reject_e_far_from_ship\"", self.source)

    def test_has_isolated_bootstrap_reject_run_and_evidence_matrix(self):
        for token in (
            "[switch]$CaptureBootstrapRejectWindow",
            "[string]$BootstrapRejectKey = \"ALL\"",
            "-d19-r03-bootstrap-reject-rerun1",
            "-d19-r03-bootstrap-key-",
            "VC_G01_BOOTSTRAP_REJECT_20260906.json",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_20260906.json",
            "bootstrap_warehouse_i",
            "bootstrap_market_m",
            "bootstrap_tavern_t",
            "Get-BootstrapRejectMatrix",
            "player_log_run_marker",
            "player_log_archive_exists",
            "bootstrap_reject_matrix_proof",
            "target_proof",
        ):
            self.assertIn(token, self.source)


if __name__ == "__main__":
    unittest.main()
