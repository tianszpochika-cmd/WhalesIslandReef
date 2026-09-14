import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


class VisualContinuationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.d28 = (ROOT / "game/Assets/Scripts/Gameplay/D28/D28WaterWeatherPresentationRuntime.cs").read_text(encoding="utf-8")
        cls.d30 = (ROOT / "game/Assets/Scripts/Gameplay/D30/D30VisualLoopCoordinator.cs").read_text(encoding="utf-8")
        cls.d30_policy = (ROOT / "game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs").read_text(encoding="utf-8")
        cls.d31 = (ROOT / "game/Assets/Scripts/Gameplay/D31/D31VisualQualityRuntime.cs").read_text(encoding="utf-8")

    def test_weather_is_presentation_only_and_has_three_modes(self):
        for token in ("Calm", "Rain", "Storm", "RenderSettings.fog", "D28WaterMotion"):
            self.assertIn(token, self.d28)
        self.assertIn("TryCycleWeather", self.d28)
        for forbidden in ("GameState.CurrentPlayer", "SaveSystem.Save", "Hull =", "Money ="):
            self.assertNotIn(forbidden, self.d28)

    def test_weather_layer_is_camera_anchored_and_readable(self):
        for token in (
            "AnchorPrecipitationToCamera",
            "ParticleSystemRenderMode.Stretch",
            "material.renderQueue = 3000",
            "rainRate = 560f",
            "rainRate = 980f",
            "SurfaceWaveMultiplier",
        ):
            self.assertIn(token, self.d28)

    def test_weather_owner_has_an_explicit_startup_boundary(self):
        self.assertIn("private bool _started;", self.d28)
        self.assertIn("public bool IsReady => _started;", self.d28)
        self.assertIn("_started = true;", self.d28)

    def test_loop_projects_owner_state_without_advancing_it(self):
        for token in ("D2ControlRouter", "D3RuntimeBootstrap", "RefreshStage", "Notify", "港口"):
            self.assertIn(token, self.d30)
        self.assertNotIn("RequestDeparture", self.d30)
        self.assertNotIn("SaveProgress", self.d30)

    def test_loop_keeps_failed_transition_in_the_current_stage(self):
        self.assertIn('case "market_failed":', self.d30_policy)
        self.assertIn('case "tavern_failed":', self.d30_policy)
        self.assertIn('case "shipyard_failed":', self.d30_policy)
        self.assertIn('case "preparation_failed":', self.d30_policy)
        self.assertIn('case "board_failed":', self.d30_policy)
        self.assertIn('case "weather_failed":', self.d30_policy)
        self.assertIn('case "quality_failed":', self.d30_policy)
        self.assertIn("public static bool IsFailureEvent", self.d30_policy)
        self.assertIn("请求未通过，状态未改变", self.d30)
        self.assertIn("_transitionText != null", self.d30)

    def test_loop_projects_preparation_success_and_ignores_duplicate_events(self):
        self.assertIn('case "preparation_opened":', self.d30_policy)
        self.assertIn('case "preparation_failed":', self.d30_policy)
        self.assertIn("IsRepeatableEvent", self.d30_policy)
        self.assertIn("RepeatGuardSeconds", self.d30)
        self.assertIn("重复事件已忽略，状态未改变", self.d30)

    def test_loop_preserves港内表现阶段_until_owner_state_overrides(self):
        self.assertIn("var next = _stage;", self.d30)
        self.assertIn("Market/Tavern/Shipyard", self.d30)
        self.assertIn("_stage == VisualStage.Ship || _stage == VisualStage.Voyage || _stage == VisualStage.Return", self.d30)

    def test_route_band_stays_below_top_title_safe_zone(self):
        self.assertIn("panelRect.anchoredPosition = new Vector2(0f, -174f)", self.d30)
        self.assertIn("top-center safe zone", self.d30)

    def test_quality_has_low_medium_high_and_no_business_dependency(self):
        for token in ("Low", "Medium", "High", "QualitySettings.lodBias", "CycleQuality"):
            self.assertIn(token, self.d31)
        self.assertIn("TryCycleQuality", self.d31)
        self.assertNotIn("D21", self.d31)
        self.assertNotIn("GameState", self.d31)


if __name__ == "__main__":
    unittest.main()
