import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
HUD_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/UI/RuntimeHudCoordinator.cs"
D29_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs"
D9_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs"
D10_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs"
D30_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D30/D30VisualLoopCoordinator.cs"
D7_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D7/D7ControlCoordinator.cs"
PAUSE_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/UI/HarborPauseController.cs"


def method_body(source: str, signature: str) -> str:
    start = source.index(signature)
    opening = source.index("{", start)
    depth = 0
    for index in range(opening, len(source)):
        if source[index] == "{":
            depth += 1
        elif source[index] == "}":
            depth -= 1
            if depth == 0:
                return source[opening:index + 1]
    raise AssertionError(f"unclosed C# body for {signature}")


class VcG01LogicBoundaryTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.hud = HUD_SOURCE.read_text(encoding="utf-8")
        cls.d29 = D29_SOURCE.read_text(encoding="utf-8")
        cls.d9 = D9_SOURCE.read_text(encoding="utf-8")
        cls.d10 = D10_SOURCE.read_text(encoding="utf-8")
        cls.d30 = D30_SOURCE.read_text(encoding="utf-8")
        cls.d7 = D7_SOURCE.read_text(encoding="utf-8")
        cls.pause = PAUSE_SOURCE.read_text(encoding="utf-8")

    def test_primary_gate_covers_pause_focus_and_owner_modal_state(self):
        gate = method_body(self.hud, "public static bool PrimaryInputAllowed")
        for marker in (
            "_primaryInputClaimFrame == Time.frameCount",
            "Application.isFocused",
            "Application.isBatchMode",
            "coordinator._paused",
            "coordinator._temporaryCanvasName",
            "pause.IsPaused",
            "dialogue.IsOpen",
            "page.IsTradeOpen",
            "page.HasVisibleContentPage",
            "page.HasInputFocus",
        ):
            self.assertIn(marker, gate)

    def test_d29_shortcuts_require_both_primary_and_gameplay_gates(self):
        update = method_body(self.d29, "private void Update()")
        primary = update.index("if (RuntimeHudCoordinator.PrimaryInputAllowed)")
        gameplay = update.index("if (D7ControlCoordinator.GameplayInputAllowed)", primary)
        shortcut = update.index("WasD29Input(D16InputAction.D29OpenMarket", gameplay)
        self.assertLess(primary, gameplay)
        self.assertLess(gameplay, shortcut)
        self.assertIn("KeyCode.B", update[shortcut:])
        self.assertIn("[DefaultExecutionOrder(-900)]", self.d29)

    def test_market_accepts_requested_m_shortcut_and_keeps_legacy_b_alias(self):
        update = method_body(self.d29, "private void Update()")
        market = "if (WasD29Input(D16InputAction.D29OpenMarket, KeyCode.M, KeyCode.B)) OpenMarket();"
        self.assertIn(market, update)
        self.assertIn('"市场  [M / B]"', self.d29)

    def test_tavern_accepts_requested_t_shortcut_through_d29_owner(self):
        update = method_body(self.d29, "private void Update()")
        self.assertIn("if (WasD29Input(D16InputAction.D29OpenTavern, KeyCode.T)) OpenTavern();", update)
        self.assertIn('"酒馆  [T]"', self.d29)
        self.assertIn("TryClaimPrimaryAction()", method_body(self.d29, "private void OpenTavern()"))

    def test_d29_actions_claim_one_primary_owner_per_frame(self):
        self.assertIn("public static bool TryClaimPrimaryInput()", self.hud)
        self.assertIn("RuntimeHudCoordinator.TryClaimPrimaryInput()", self.d29)
        for signature in (
            "private void ToggleShip()",
            "private void OpenMarket()",
            "private void OpenTavern()",
            "private void UpgradeShip()",
            "private void OpenPreparation()",
            "private void CycleWeather()",
            "private void CycleQuality()",
            "private void Save()",
            "private void ToggleInfo()",
        ):
            self.assertIn("TryClaimPrimaryAction()", method_body(self.d29, signature), signature)

        # Warehouse close is a modal escape path and must remain usable while
        # the central primary gate is intentionally false.
        self.assertNotIn("TryClaimPrimaryAction()", method_body(self.d29, "private void CloseWarehouse()"))

    def test_d9_and_d10_open_shortcuts_yield_to_existing_modal_focus(self):
        d9_update = method_body(self.d9, "private void Update()")
        d10_update = method_body(self.d10, "private void Update()")
        self.assertIn("bool openPressed", d9_update)
        self.assertIn("_pageOpen ||", d9_update)
        self.assertIn("RuntimeHudCoordinator.PrimaryInputAllowed && D7InputAllowed()", d9_update)
        self.assertIn("ClosePreparationPage();", method_body(self.d9, "private void VisitTavern()"))
        self.assertIn("bool openPressed", d10_update)
        self.assertIn("_pageOpen ||", d10_update)
        self.assertIn("RuntimeHudCoordinator.PrimaryInputAllowed && D7ControlCoordinator.GameplayInputAllowed", d10_update)

    def test_escape_compatibility_closes_modals_before_toggling_pause(self):
        modal_escape = method_body(self.d7, "private void HandleModalEscape()")
        pause_update = method_body(self.pause, "private void Update()")
        self.assertIn("Input.GetKeyDown(KeyCode.Escape)", modal_escape)
        self.assertIn("_pause != null && _pause.IsPaused", modal_escape)
        self.assertIn("_d3 != null && _d3.IsTradeOpen", modal_escape)
        self.assertIn("if (!RuntimeHudCoordinator.PrimaryInputAllowed)", modal_escape)
        self.assertIn("EscapeHandledThisFrame", modal_escape)
        self.assertIn("Input.GetKeyDown(KeyCode.Escape)", pause_update)

    def test_d30_remains_a_noninteractive_projection_owner(self):
        self.assertIn("public static void Notify(string eventId)", self.d30)
        self.assertIn("D30VisualStagePolicy.Evaluate", self.d30)
        self.assertIn("raycaster.enabled = false", self.d30)
        self.assertNotIn("Input.GetKey", self.d30)
        self.assertNotIn("InputSystemUIInputModule", self.d30)

    def test_d29_owner_readiness_and_d30_failure_projection_remain_explicit(self):
        for marker in (
            "_d3 == null || !_d3.IsReady",
            "_d4 == null || !_d4.IsReady",
            "_d9 == null || !_d9.IsReady",
            "_d10 == null || !_d10.IsReady",
            "D30VisualLoopCoordinator.Notify(entered ? \"tavern_opened\" : \"tavern_failed\")",
            "D30VisualLoopCoordinator.Notify(shipyardReady ? \"shipyard_opened\" : \"shipyard_failed\")",
            "if (!ok) D30VisualLoopCoordinator.Notify(\"save_failed\");",
        ):
            self.assertIn(marker, self.d29)


if __name__ == "__main__":
    unittest.main()
