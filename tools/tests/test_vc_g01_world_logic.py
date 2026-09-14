import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
R03_BRIDGE = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03HarborRuntimePresentation.cs"
COLLISION = ROOT / "game/Assets/Scripts/Gameplay/Harbor/HarborCollisionController.cs"
FIXED_SHOTS = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03FixedShotProbe.cs"
ROUTE_PROBE = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs"


class VcG01WorldLogicTests(unittest.TestCase):
    def setUp(self):
        self.bridge = R03_BRIDGE.read_text(encoding="utf-8")
        self.collision = COLLISION.read_text(encoding="utf-8")
        self.fixed_shots = FIXED_SHOTS.read_text(encoding="utf-8")
        self.route = ROUTE_PROBE.read_text(encoding="utf-8")

    def test_route_boundary_is_resolved_from_the_d6_owner(self):
        self.assertIn('private static GameObject FindOwnedShoreBoundary()', self.bridge)
        self.assertIn('GameObject.Find(CollisionRootName)', self.bridge)
        self.assertIn('collisionRoot.transform.Find(ShoreBoundaryName)', self.bridge)
        self.assertIn('!collider.isTrigger', self.bridge)
        self.assertNotIn('FindChildByName("ShoreBoundary")', self.bridge)

    def test_r03_route_window_retains_d6_perimeter_boundaries_outside_water(self):
        self.assertIn('private static bool HasOwnedR03OuterBoundarySet()', self.bridge)
        self.assertIn('ShoreBoundaryName + ".South"', self.bridge)
        self.assertIn('ShoreBoundaryName + ".West"', self.bridge)
        self.assertIn('ShoreBoundaryName + ".East"', self.bridge)
        self.assertIn('D6-owned R03 perimeter boundaries remain outside water', self.bridge)
        self.assertIn('move .South to z=17', self.bridge)

    def test_hidden_legacy_physical_colliders_are_disabled_without_disabling_triggers(self):
        start = self.bridge.index("private static void DisableConflictingLegacyPhysicalColliders")
        end = self.bridge.index("private static void SetText", start)
        method = self.bridge[start:end]
        self.assertIn('root.name != LegacyPresentationRootName', method)
        self.assertIn('if (collider.isTrigger)', method)
        self.assertIn('collider.enabled = false', method)

    def test_collision_owner_can_upgrade_after_delayed_r03_presentation_load(self):
        self.assertIn('public bool R03SurfaceInstalled { get; private set; }', self.collision)
        self.assertIn('private void Update()', self.collision)
        self.assertIn('R03RetryIntervalSeconds', self.collision)
        self.assertIn('private bool TryUpgradeToR03Presentation()', self.collision)
        self.assertIn('TryInstallR03WalkableSurface(r03)', self.collision)
        self.assertIn('ConfigureR03WalkableSurfaces(r03)', self.collision)
        self.assertIn('SceneManager.GetActiveScene().name != "InitialHarbor"', self.collision)

    def test_fixed_shot_probe_restores_the_previous_camera_follow_state_and_versions_any_attempt(self):
        self.assertIn('private bool _cameraFollowWasEnabled;', self.fixed_shots)
        self.assertIn('private bool _cameraFollowStateCaptured;', self.fixed_shots)
        self.assertIn('_cameraFollowWasEnabled = _cameraFollow.enabled;', self.fixed_shots)
        self.assertIn('_cameraFollow.enabled = _cameraFollowWasEnabled;', self.fixed_shots)
        self.assertIn('const string prefix = "-d19-r03-fixed-shots-rerun";', self.fixed_shots)
        self.assertIn('arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)', self.fixed_shots)
        self.assertIn('int.TryParse(suffix, out var attempt)', self.fixed_shots)
        self.assertNotRegex(self.fixed_shots, r'if \(_cameraFollow != null\)\s*_cameraFollow\.enabled = true;')

    def test_route_probe_does_not_call_compute_penetration_without_a_player_collider(self):
        self.assertIn('var playerCollider = _controller == null ? null : _controller.GetComponent<Collider>();', self.route)
        self.assertIn('player_overlaps_ship_hull = _shipHull != null && playerCollider != null', self.route)
        self.assertIn('Physics.ComputePenetration(', self.route)

    def test_route_probe_landmarks_prefer_active_r03_objects_over_hidden_legacy_namesakes(self):
        self.assertIn('private const string R03PresentationRootName = "D19_R03_HarborPresentation";', self.route)
        self.assertIn('private static bool IsR03LandmarkName(string objectName)', self.route)
        self.assertIn('var presentation = GameObject.Find(R03PresentationRootName);', self.route)
        self.assertIn('GetComponentsInChildren<Transform>(false)', self.route)


if __name__ == "__main__":
    unittest.main()
