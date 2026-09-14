import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D29" / "D29InteractionHud.cs"
D9_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D9" / "D9PortShipPreparationBootstrap.cs"
D4_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D4" / "D4RuntimeBootstrap.cs"
D3_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D3" / "D3RuntimeBootstrap.cs"
D2_ROUTER_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D2" / "D2ControlRouter.cs"
D2_SHIP_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D2" / "D2ShipController.cs"
D5_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D5" / "D5RuntimeBootstrap.cs"
COORDINATOR_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "UI" / "RuntimeHudCoordinator.cs"
D16_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "UI" / "D16" / "D16InputRouter.cs"


class D29InteractionHudTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.text = SOURCE.read_text(encoding="utf-8")
        cls.d9 = D9_SOURCE.read_text(encoding="utf-8")
        cls.d4 = D4_SOURCE.read_text(encoding="utf-8")
        cls.d3 = D3_SOURCE.read_text(encoding="utf-8")
        cls.d2_router = D2_ROUTER_SOURCE.read_text(encoding="utf-8")
        cls.d2_ship = D2_SHIP_SOURCE.read_text(encoding="utf-8")
        cls.d5 = D5_SOURCE.read_text(encoding="utf-8")
        cls.coordinator = COORDINATOR_SOURCE.read_text(encoding="utf-8")
        cls.d16 = D16_SOURCE.read_text(encoding="utf-8")

    def test_canvas_is_scalable_and_buttons_have_visible_feedback(self):
        self.assertIn("D29InteractionLayoutPolicy.ReferenceWidth", self.text)
        self.assertIn("D29InteractionLayoutPolicy.ReferenceHeight", self.text)
        self.assertIn("EnsureEventSystem();", self.text)
        self.assertIn("button.targetGraphic = image;", self.text)
        self.assertIn('CreatePanel(_panel.transform, "Info", new Vector2(0f, 30f)', self.text)
        for method in ("OpenMarket", "ToggleInfo", "OpenTavern", "UpgradeShip", "ToggleShip", "CycleWeather", "CycleQuality", "Save"):
            self.assertIn("CreateButton", self.text)
            self.assertIn(method, self.text)
        self.assertIn("Show(", self.text)

    def test_event_system_is_active_navigation_owner_without_primary_hud_capture(self):
        self.assertIn("eventSystem.gameObject.SetActive(true);", self.text)
        self.assertIn("eventSystem.enabled = true;", self.text)
        self.assertIn("EventSystem.current = eventSystem", self.text)
        self.assertIn("eventSystem.sendNavigationEvents = false;", self.text)
        self.assertIn("inputModule.AssignDefaultActions();", self.text)
        self.assertIn("eventSystem.UpdateModules();", self.text)

    def test_primary_quick_actions_disable_ugui_navigation_at_creation(self):
        self.assertIn("button.navigation = new Navigation", self.text)
        self.assertIn("Navigation.Mode.None", self.text)
        self.assertIn("D29InteractionLayoutPolicy.WarehouseObjectName", self.text)

    def test_ui_does_not_write_domain_fields_or_use_network(self):
        forbidden = ("GameState.CurrentPlayer.Money =", "HttpClient", "UnityWebRequest", "AffinityChange =")
        for token in forbidden:
            self.assertNotIn(token, self.text)
        self.assertIn("RequestTrade()", self.text)
        self.assertIn("RequestSave()", self.text)

    def test_failed_market_does_not_open_the_market_canvas(self):
        # D3 owns the actual canvas transition. D29 must wait for the owner
        # result so a rejected request cannot leave a dead/empty market page.
        self.assertIn('var opened = _d3.RequestTrade();', self.text)
        self.assertIn('opened ? "market_opened" : "market_failed"', self.text)
        self.assertNotIn('ShowTemporaryCanvas("D3Canvas")', self.text)

    def test_tavern_and_shipyard_do_not_claim_success_without_owner_result(self):
        self.assertIn("var entered = _d10.HasEnteredTavern && _d10.HasInputFocus;", self.text)
        self.assertIn('entered ? "tavern_opened" : "tavern_failed"', self.text)
        self.assertIn("_d10.LastMessage", self.text)
        self.assertIn("var upgraded = _d4.TryUpgradeHull();", self.text)
        self.assertIn("var shipyardReady = upgraded && _d4.HasVisibleContentPage;", self.text)
        self.assertIn('shipyardReady ? "shipyard_opened" : "shipyard_failed"', self.text)
        self.assertIn("_d4.LastMessage", self.text)

    def test_rejected_shipyard_restores_primary_hud(self):
        self.assertIn("if (!shipyardReady)", self.text)
        self.assertIn("_d4.CloseContentPage();", self.text)

    def test_preparation_reports_actual_page_focus(self):
        self.assertIn("var opened = _d9.HasInputFocus;", self.text)
        self.assertIn("if (!opened)", self.text)
        self.assertIn("_d9.ClosePreparationPage();", self.text)
        self.assertIn('opened ? "preparation_opened" : "preparation_failed"', self.text)

    def test_save_rejection_and_exception_project_failure_without_claiming_success(self):
        self.assertIn('if (!ok) D30VisualLoopCoordinator.Notify("save_failed");', self.text)
        self.assertIn('ReportActionException("save_failed", "保存", exception);', self.text)

    def test_weather_and_quality_report_missing_owner(self):
        self.assertIn("var changed = D28WaterWeatherPresentationRuntime.TryCycleWeather();", self.text)
        self.assertIn("天气表现 owner 尚未就绪，暂不能切换。", self.text)
        self.assertIn("var changed = D31VisualQualityRuntime.TryCycleQuality();", self.text)
        self.assertIn("画质表现 owner 尚未就绪，暂不能切换。", self.text)
        self.assertIn("B/R/C/I/Q 可用", self.text)

    def test_weather_and_quality_exceptions_use_failure_projection(self):
        self.assertIn('ReportActionException("weather_failed", "天气表现", exception);', self.text)
        self.assertIn('ReportActionException("quality_failed", "画质表现", exception);', self.text)

    def test_weather_button_waits_for_started_owner(self):
        weather = (ROOT / "game/Assets/Scripts/Gameplay/D28/D28WaterWeatherPresentationRuntime.cs").read_text(encoding="utf-8")
        self.assertIn("private bool _started;", weather)
        self.assertIn("public bool IsReady => _started;", weather)
        self.assertIn("if (instance == null || !instance.IsReady) return false;", weather)
        self.assertIn("weatherOwner != null && weatherOwner.IsReady", self.text)
        self.assertIn("!weatherReady", self.text)

    def test_primary_buttons_have_runtime_availability_and_repeat_guard(self):
        self.assertIn("RefreshButtonAvailability();", self.text)
        self.assertIn("ResolveOwners();\n            RefreshButtonAvailability();", self.text)
        # UI interactable is deliberately kept true even when an owner is not
        # ready, so the click reaches the explicit owner-feedback branch.
        self.assertIn("button.interactable = true;", self.text)
        self.assertIn("clickable_unready=true; owner_feedback=true", self.text)
        self.assertIn("仍可点击查看原因", self.text)
        self.assertIn('SetAvailability(_marketButton, d3Ready);', self.text)
        self.assertIn('SetAvailability(_preparationButton, d9Ready);', self.text)
        self.assertIn("AllowOneShot", self.text)
        self.assertIn("D29OneShotPolicy.Evaluate", self.text)
        self.assertIn("重复点击已忽略", self.text)
        self.assertNotIn("button.gameObject.name = button.gameObject.name", self.text)

    def test_repeat_guard_is_reset_when_owner_session_has_closed(self):
        # A new market/tavern/ship session must not inherit the previous
        # session's timestamp; PrimaryInputAllowed already blocks duplicates
        # while the owner page is open.
        self.assertIn("ResetClosedOwnerOneShot(actionId);", self.text)
        self.assertIn('case "market":', self.text)
        self.assertIn('case "tavern":', self.text)
        self.assertIn('case "board":', self.text)
        self.assertIn('case "shipyard":', self.text)
        self.assertIn('case "preparation":', self.text)
        self.assertIn("_lastOneShotAction = string.Empty;", self.text)
        self.assertIn("_lastOneShotTime = -1f;", self.text)

    def test_dynamic_availability_refreshes_modal_keyboard_navigation(self):
        # The primary HUD is a non-modal overlay: W/S must remain gameplay
        # movement and cannot navigate its buttons. Only the warehouse modal
        # owns D16 navigation focus.
        self.assertNotIn("private D16ModalFocusScope _focusScope;", self.text)
        self.assertIn("D16ModalFocusScope.Attach(_warehousePanel", self.text)
        self.assertNotIn("_focusScope?.RefreshNavigation();", self.text)
        self.assertIn("public void RefreshNavigation()", self.d16)
        self.assertIn("RebuildNavigation();", self.d16)

    def test_rejected_tavern_restores_primary_hud(self):
        self.assertIn("var entered = _d10.HasEnteredTavern && _d10.HasInputFocus;", self.text)
        self.assertIn("if (!entered)", self.text)
        self.assertIn("_d10.CloseSocialPage();", self.text)

    def test_action_exceptions_restore_hud_and_project_failed_state(self):
        self.assertIn('ReportActionException("board_failed", "登船", exception);', self.text)
        self.assertIn('ReportActionException("market_failed", "市场", exception);', self.text)
        self.assertIn('ReportActionException("tavern_failed", "酒馆", exception, _d10.CloseSocialPage);', self.text)
        self.assertIn('ReportActionException("shipyard_failed", "船坞", exception, _d4.CloseContentPage);', self.text)
        self.assertIn('ReportActionException("preparation_failed", "港口整备", exception, _d9.ClosePreparationPage);', self.text)
        self.assertIn("状态未改变", self.text)
        self.assertIn("recovery failed", self.text)

    def test_owner_close_failure_has_coordinator_hud_fallback(self):
        # A thrown owner cleanup callback must not strand the temporary page over
        # the primary HUD. Keep this source-level assertion close to the recovery
        # contract so the fallback cannot silently disappear during refactors.
        self.assertIn("RuntimeHudCoordinator.RestorePrimaryHud();", self.text)
        self.assertIn("recovery fallback failed", self.text)
        self.assertIn("fallbackException.Message", self.text)

    def test_missing_warehouse_objects_have_visible_feedback(self):
        self.assertIn('仓库面板尚未就绪，无法打开。', self.text)
        self.assertIn('仓库内容区域尚未就绪，无法读取库存。', self.text)
        self.assertIn('Show("仓库面板已关闭。", true);', self.text)

    def test_warehouse_requires_d3_owner_and_tracks_button_availability(self):
        # The read-only warehouse must obey the same D3 readiness boundary
        # for both mouse and keyboard entry; otherwise I can open an empty
        # modal while the owner is still bootstrapping.
        self.assertIn('private Button _warehouseButton;', self.text)
        self.assertIn('_warehouseButton = CreateButton(_panel.transform, "Warehouse", "仓库  [I]", 1, ToggleInfo);', self.text)
        self.assertIn('if (_d3 == null || !_d3.IsReady)', self.text)
        self.assertIn('仓库 owner 尚未就绪，暂不能读取库存。', self.text)
        self.assertIn('SetAvailability(_warehouseButton, d3Ready);', self.text)

    def test_warehouse_is_a_real_read_only_panel_and_preparation_has_recovery(self):
        self.assertIn('BuildWarehousePanel(canvasObject.transform);', self.text)
        self.assertIn('CreateButton(_panel.transform, "Preparation", "港口整备  [P]"', self.text)
        self.assertIn('RefreshWarehouse();', self.text)
        self.assertIn('snapshot.Cargo', self.text)
        self.assertIn('public void OpenPreparationPage()', self.d9)
        self.assertIn('public void ClosePreparationPage()', self.d9)
        self.assertIn('关闭整备页 [P / Esc]', self.d9)

    def test_content_panel_has_a_close_path_and_does_not_leave_debug_text_unframed(self):
        self.assertIn('public void CloseContentPage()', self.d4)
        self.assertIn('RuntimeHudCoordinator.RestorePrimaryHud();', self.d4)
        self.assertIn('CreateButton(panel, "Close", "关闭内容页 [Esc]"', self.d4)
        self.assertIn('new GameObject("D4Summary"', self.d4)
        self.assertIn('new GameObject("D4HintBand"', self.d4)
        self.assertIn('rect.anchorMin = new Vector2(0.5f, 1f)', self.d4)
        self.assertIn('rect.anchorMin = new Vector2(0.5f, 1f)', self.d9)
        self.assertIn('new GameObject("D9Summary"', self.d9)
        self.assertIn('new GameObject("D9HintBand"', self.d9)
        self.assertIn('RuntimeHudCoordinator.ShowTemporaryCanvas("D29WarehousePanel")', self.text)
        self.assertIn('RuntimeHudCoordinator.RestorePrimaryHud();', self.text)

    def test_temporary_pages_hide_route_overlay(self):
        coordinator = self.coordinator
        self.assertIn('"D30VisualLoopCanvas"', coordinator)
        self.assertIn('SetCanvasState("D30VisualLoopCanvas", showPrimary, false);', coordinator)
        self.assertIn('bool showTemporaryWarehouse = _temporaryCanvasName == "D29WarehousePanel";', coordinator)

    def test_temporary_canvases_start_hidden_before_coordinator_opens_them(self):
        # D4/D9/D10 build their temporary page in Awake. A disabled Canvas is
        # required so its catalog/hint cannot flash over the primary HUD before
        # RuntimeHudCoordinator applies the first frame policy.
        self.assertIn("canvas.enabled = false;", self.d4)
        self.assertIn("canvas.enabled = false;", self.d9)
        d10 = (ROOT / "game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs").read_text(encoding="utf-8")
        self.assertIn("canvas.enabled = false;", d10)
        self.assertIn('ShowTemporaryCanvas("D4Canvas")', self.text)
        self.assertIn('ShowTemporaryCanvas("D9PortServicePage")', self.text)
        self.assertIn('ShowTemporaryCanvas("D10TavernSocialPage")', d10)

    def test_runtime_hud_hides_all_same_named_canvas_instances(self):
        # D29 creates a controller host and a child Canvas at runtime. They must
        # not share a name, and the coordinator must apply visibility to every
        # matching Canvas so a stale/duplicate object cannot remain interactive.
        self.assertIn('private const string ControllerName = "D29InteractionHudController";', self.text)
        self.assertIn('new GameObject(ControllerName).AddComponent<D29InteractionHud>();', self.text)
        self.assertIn('var direct = GameObject.Find(objectName);', self.coordinator)
        self.assertIn('foreach (var canvas in canvases)', self.coordinator)
        self.assertIn('ApplyCanvasState(canvas.gameObject, visible, interactive);', self.coordinator)

    def test_primary_shortcuts_yield_to_temporary_pages(self):
        coordinator = (ROOT / "game/Assets/Scripts/Gameplay/UI/RuntimeHudCoordinator.cs").read_text(encoding="utf-8")
        self.assertIn("public static bool PrimaryInputAllowed", coordinator)
        self.assertIn("FindObjectsByType<RuntimeHudCoordinator>", coordinator)
        self.assertIn("!string.IsNullOrEmpty(coordinator._temporaryCanvasName)", coordinator)
        self.assertIn("page.HasInputFocus", coordinator)
        self.assertIn("public bool HasInputFocus => _pageOpen", self.d9)
        start = self.text.index("if (RuntimeHudCoordinator.PrimaryInputAllowed)")
        end = self.text.index("\n        }\n\n        private void ResolveOwners", start)
        shortcut_block = self.text[start:end]
        for key in ("KeyCode.B", "KeyCode.R", "KeyCode.C", "KeyCode.I", "KeyCode.Q"):
            self.assertIn(key, shortcut_block)
        self.assertIn("KeyCode.P", shortcut_block)

    def test_warehouse_close_shortcuts_precede_primary_input_gate(self):
        # Warehouse is a temporary canvas, so the primary gate is correctly
        # false while it is visible. Its I/Esc close actions must therefore be
        # handled before that gate, while the other D29 shortcuts remain inside
        # the gate and cannot leak into the modal.
        start = self.text.index("var warehouseVisible")
        end = self.text.index("if (RuntimeHudCoordinator.PrimaryInputAllowed)", start)
        close_block = self.text[start:end]
        self.assertIn("KeyCode.I", close_block)
        self.assertIn("KeyCode.Escape", close_block)
        self.assertIn("CloseWarehouse();", close_block)
        self.assertIn("return;", close_block)

    def test_temporary_page_summaries_stay_below_harbor_title(self):
        for source_path in (D9_SOURCE, D4_SOURCE):
            source = source_path.read_text(encoding="utf-8")
            self.assertIn("summaryRect.anchoredPosition = new Vector2(28f, -196f)", source)
        self.assertIn("Input.GetKeyDown(KeyCode.Escape)", self.d9)
        self.assertIn("Input.GetKeyDown(KeyCode.P)", self.d9)
        self.assertIn("Time.frameCount != _pageOpenedFrame", self.d9)

    def test_all_primary_gameplay_owners_yield_to_temporary_page(self):
        # D29's guard is only useful if the other legacy keyboard owners also
        # yield. This prevents D3's B trade shortcut (and movement/content
        # shortcuts) from leaking through an open D9/D4 temporary page.
        for source in (self.d3, self.d2_router, self.d2_ship, self.d5):
            self.assertIn("RuntimeHudCoordinator.PrimaryInputAllowed", source)
        # D4 is the temporary page itself; it must use its own visibility gate
        # instead of the primary-HUD gate, otherwise every D4 key is blocked by
        # the coordinator that correctly marks temporary pages as focused.
        self.assertIn("if (!IsContentPageVisible() || Time.timeScale <= 0f", self.d4)
        self.assertIn("private static bool WasD9Input", self.d9)
        self.assertIn("Input.GetKeyDown(fallback)", self.d9)

    def test_d4_player_keyboard_edges_have_compatibility_fallback(self):
        self.assertIn("private static bool WasD4Input", self.d4)
        for key in ("KeyCode.F", "KeyCode.K", "KeyCode.R", "KeyCode.Alpha1", "KeyCode.Alpha2", "KeyCode.Alpha3", "KeyCode.Space", "KeyCode.B", "KeyCode.E", "KeyCode.U", "KeyCode.A", "KeyCode.M", "KeyCode.J", "KeyCode.C"):
            self.assertIn(key, self.d4)
        self.assertIn("Input.GetKeyDown(fallback)", self.d4)


if __name__ == "__main__":
    unittest.main()
