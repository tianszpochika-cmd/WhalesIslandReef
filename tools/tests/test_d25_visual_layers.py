import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT_ROOT = ROOT / "game/Assets/Scripts/Gameplay/D25"


class D25VisualLayerTests(unittest.TestCase):
    def read(self, name):
        return (SCRIPT_ROOT / name).read_text(encoding="utf-8")

    def test_theme_owns_only_rendering_defaults(self):
        text = self.read("D25VisualTheme.cs")
        self.assertIn("ApplyHarborAtmosphere", text)
        self.assertIn("ApplyCameraDefaults", text)
        self.assertIn('skybox.HasProperty("_GroundColor")', text)
        self.assertIn('skybox.SetColor("_GroundColor"', text)
        self.assertIn('skybox.SetColor("_SkyTint"', text)
        self.assertNotIn("HttpClient", text)
        self.assertNotIn("UnityWebRequest", text)
        self.assertNotIn("SaveSystem", text)

    def test_hud_uses_reference_resolution_and_does_not_capture_raycast(self):
        text = self.read("D25PlayerHud.cs")
        self.assertIn("referenceResolution = new Vector2(1920f, 1080f)", text)
        self.assertIn("sortingOrder = 140", text)
        self.assertIn("group.blocksRaycasts = false", text)
        self.assertIn("raycaster.enabled = false", text)
        self.assertIn("ReadLastInteractionFeedback", text)

    def test_runtime_bootstrap_is_presentation_only(self):
        text = self.read("D25VisualRuntimeBootstrap.cs")
        self.assertIn("D25PlayerHud", text)
        self.assertIn("D25HarborAtmosphere", text)
        self.assertIn("button.interactable = false", text)
        self.assertNotIn("SceneManager.LoadScene", text)
        self.assertNotIn("HttpClient", text)


if __name__ == "__main__":
    unittest.main()
