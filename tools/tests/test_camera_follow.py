import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "CameraFollow.cs"


class CameraFollowRegressionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.text = SOURCE.read_text(encoding="utf-8-sig")

    def test_ship_control_enters_ship_deck_preset(self):
        self.assertIn("if (isShipControlled && viewMode == CameraViewMode.Character)", self.text)
        self.assertIn("SetViewMode(CameraViewMode.ShipDeck)", self.text)

    def test_ship_control_exit_restores_character_preset(self):
        self.assertIn("if (!isShipControlled && viewMode == CameraViewMode.ShipDeck)", self.text)
        self.assertIn("SetViewMode(CameraViewMode.Character)", self.text)

    def test_controlled_ship_colliders_are_ignored_by_camera_cast(self):
        self.assertIn("shipControlled && shipTarget != null", self.text)
        self.assertIn("collider.transform.IsChildOf(shipTarget)", self.text)

    def test_ship_deck_framing_keeps_helm_operator_readable(self):
        self.assertIn("ShipDeckPresentationDistance = 5.5f", self.text)
        self.assertIn("ShipDeckPresentationPitch = 20f", self.text)
        self.assertIn("ShipDeckPresentationTargetOffset", self.text)
        self.assertIn("distance = ShipDeckPresentationDistance", self.text)

    def test_swimming_framing_exposes_body_against_water_surface(self):
        self.assertIn("SwimmingPresentationDistance = 4.6f", self.text)
        self.assertIn("SwimmingPresentationPitch = 20f", self.text)
        self.assertIn("SwimmingPresentationTargetOffset", self.text)
        self.assertIn("distance = SwimmingPresentationDistance", self.text)

    def test_initial_harbor_composition_keeps_side_landmarks_inside_frame(self):
        self.assertIn("InitialHarborCompositionDistance = 18.5f", self.text)
        self.assertIn(
            "distance = Mathf.Clamp(InitialHarborCompositionDistance, minDistance, maxDistance)",
            self.text,
        )
        self.assertIn("near horizontal frustum edges", self.text)
        self.assertIn("legacy roots are", self.text)
        self.assertIn("camera frustum", self.text)

    def test_swimming_camera_tracks_heading_until_manual_look_handoff(self):
        self.assertIn("swimmingPresentationAutoYaw", self.text)
        self.assertIn("bool manualLookInput = HandleInput()", self.text)
        self.assertIn("yaw = characterTarget.eulerAngles.y + 90f", self.text)
        self.assertIn("swimmingPresentationAutoYaw = false", self.text)
        self.assertIn("SwimmingPresentationAutoYaw", self.text)

    def test_camera_fails_closed_for_non_finite_runtime_state(self):
        self.assertIn("private static bool IsFinite(float value)", self.text)
        self.assertIn("if (!IsFinite(target.position) || !IsFinite(targetOffset)", self.text)
        self.assertIn("if (IsFinite(nextPosition)) transform.position = nextPosition", self.text)
        self.assertIn("if (IsFinite(look) && look.sqrMagnitude > 0.01f)", self.text)

    def test_camera_fix_remains_presentation_only(self):
        self.assertNotIn("Whalebone.Systems", self.text)
        self.assertNotIn("HttpClient", self.text)


if __name__ == "__main__":
    unittest.main()
