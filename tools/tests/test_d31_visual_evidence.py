import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D31" / "D31VisualEvidenceProbe.cs"


class D31VisualEvidenceProbeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.text = SOURCE.read_text(encoding="utf-8")

    def test_probe_is_opt_in_and_captures_real_backbuffer_files(self):
        for token in (
            "-d31-visual-evidence",
            "ScreenCapture.CaptureScreenshot",
            "File.Exists(path)",
            "player_backbuffer_capture = true",
            "formal_scene_asset_modified = false",
        ):
            self.assertIn(token, self.text)

    def test_probe_has_fourteen_fixed_shots_and_three_weather_variants(self):
        self.assertIn("fixed_requested_count = FixedShots.Length", self.text)
        self.assertIn('new Shot("d31_fixed_main_menu"', self.text)
        self.assertIn('new Shot("d31_fixed_debug_boundary"', self.text)
        self.assertIn('private static readonly string[] WeatherVariants = { "Calm", "Rain", "Storm" }', self.text)
        self.assertIn('SetWeatherPreview(weather)', self.text)
        self.assertIn("weather_captured_count = CountCaptured(\"weather\")", self.text)

    def test_probe_does_not_write_scene_or_domain_state(self):
        for forbidden in (
            "SceneUtility.SaveScene",
            "EditorSceneManager.SaveScene",
            "SaveSystem.Save",
            "GameState.CurrentPlayer",
            "Money =",
            "Hull =",
        ):
            self.assertNotIn(forbidden, self.text)
        self.assertIn("formal_scene_asset_modified = false", self.text)

    def test_probe_temporarily_owns_camera_for_fixed_shots(self):
        for token in (
            "private CameraFollow _cameraFollow;",
            "CaptureAndDisableCameraFollow();",
            "_cameraFollow = _camera.GetComponent<CameraFollow>();",
            "_cameraFollow.enabled = false;",
            "RestoreCameraFollow();",
            "_cameraFollow.enabled = _cameraFollowWasEnabled;",
        ):
            self.assertIn(token, self.text)

    def test_fixed_shots_use_current_r03_harbor_coordinate_frame(self):
        for stale_coordinate in ("82f", "104f", "-86f", "-66f", "-58f", "-88f"):
            self.assertNotIn(stale_coordinate, self.text)
        for current_anchor in ("-15f", "14f", "-19f", "30f"):
            self.assertIn(current_anchor, self.text)


if __name__ == "__main__":
    unittest.main()
