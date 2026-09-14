import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs"
CONTROLLER = ROOT / "game/Assets/Scripts/Gameplay/Harbor/HarborPresentationController.cs"
PLAYMODE = ROOT / "game/Assets/Tests/PlayMode/D19R03Q26GameplayDefectPlayModeTests.cs"
PLAYER_BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03PlayerBuilder.cs"
ROUTE_PROBE = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs"


class Q26SpatialCleanupTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.builder = BUILDER.read_text(encoding="utf-8")
        cls.controller = CONTROLLER.read_text(encoding="utf-8")
        cls.playmode = PLAYMODE.read_text(encoding="utf-8")
        cls.player_builder = PLAYER_BUILDER.read_text(encoding="utf-8")
        cls.route_probe = ROUTE_PROBE.read_text(encoding="utf-8")

    def test_d6_legacy_foam_is_renderer_only_suppressed(self):
        self.assertIn('root.name == "D6_HarborFoam"', self.builder)
        self.assertIn("remove only its duplicate renderers", self.builder)
        self.assertIn("root.GetComponentsInChildren<Renderer>(true)", self.builder)
        self.assertIn('r03Presentation = GameObject.Find("D19_R03_HarborPresentation")', self.controller)
        self.assertIn("R03 shoreline owner active; legacy foam bands suppressed", self.controller)

    def test_market_entry_is_moved_out_of_initial_camera(self):
        self.assertIn("root.transform.position = new Vector3(0f, Height(0f, 4.2f) - 0.02f, 4.2f);", self.builder)
        self.assertIn("Q26A08MarketEntryDoesNotOverlapInitialPlayerCamera", self.playmode)
        self.assertIn("Assert.Greater(planarDistance, 2.0f,", self.playmode)

    def test_pirate_tower_uses_r03_authored_terrain_support(self):
        self.assertIn("KenneyPirateTower", self.builder)
        self.assertIn("new Vector3(-12.8f, 0.20f, 11.8f)", self.builder)
        self.assertIn("Q26A09PirateTowerHasAuthoredTerrainSupport", self.playmode)
        self.assertIn("Physics.RaycastAll", self.playmode)
        self.assertIn('D19_R03_HarborPresentation', self.playmode)
        self.assertIn('D19_TerrainRoot', self.playmode)

    def test_player_candidate_and_probe_namespace_are_unique(self):
        self.assertIn("Build LOCAL QUALITY Q26 Pirate Port Grounding Candidate R3", self.player_builder)
        self.assertIn("Q26_PIRATE_PORT_GROUNDING_CANDIDATE_R3.exe", self.player_builder)
        for token in (
            "-d19-r03-route-rerun84",
            "route_rerun84_",
            "D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN84.json",
        ):
            self.assertIn(token, self.route_probe)


if __name__ == "__main__":
    unittest.main()
