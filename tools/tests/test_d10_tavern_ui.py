import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
D10_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D10" / "D10TavernSocialBootstrap.cs"
HUD_SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "UI" / "RuntimeHudCoordinator.cs"


class D10TavernUiTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.d10 = D10_SOURCE.read_text(encoding="utf-8")
        cls.hud = HUD_SOURCE.read_text(encoding="utf-8")

    def test_tavern_has_a_single_recoverable_close_path(self):
        self.assertIn("public void CloseSocialPage()", self.d10)
        self.assertIn("RuntimeHudCoordinator.RestorePrimaryHud();", self.d10)
        self.assertIn('CreateButton(_panel, "Close", "关闭酒馆 [Esc]"', self.d10)
        self.assertIn("WasDefaultKeyboardPressed(Key.Escape)", self.d10)
        self.assertIn("!D16SensitiveContentOverlay.IsOpen", self.d10)

    def test_tavern_buttons_use_top_anchored_layout(self):
        self.assertIn("rect.anchorMin = new Vector2(0.5f, 1f)", self.d10)
        self.assertIn("rect.pivot = new Vector2(0.5f, 1f)", self.d10)
        self.assertIn('CreateButton(_panel, "Skip", "跳过敏感内容 [X]", new Vector2(0f, -585f)', self.d10)

    def test_temporary_tavern_hides_primary_quick_hud(self):
        self.assertIn('"D29InteractionHudCanvas"', self.hud)
        self.assertIn('"D10TavernSocialPage"', self.hud)
        self.assertIn('"D25PlayerHudCanvas"', self.hud)
        self.assertIn('showTemporaryD9', self.hud)


if __name__ == "__main__":
    unittest.main()
