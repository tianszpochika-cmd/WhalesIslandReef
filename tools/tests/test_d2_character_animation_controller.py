import re
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
CONTROLLER = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D2" / "D2CharacterAnimationController.cs"


class D2CharacterAnimationControllerSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = CONTROLLER.read_text(encoding="utf-8")

    def test_animation_layer_cannot_write_physical_root_or_lateral_visual_pose(self):
        self.assertNotRegex(self.source, r"\btransform\.(position|rotation)\s*=")
        self.assertNotIn("transform.SetPositionAndRotation", self.source)
        self.assertNotIn("Vector3.up * pose.Bob", self.source)
        self.assertNotIn("Quaternion.AngleAxis(pose.Lean", self.source)
        self.assertIn("ApplyVisualRootBaseline();", self.source)
        self.assertIn("_visualRoot.localPosition = _visualRootBasePosition;", self.source)
        self.assertIn("_visualRoot.localRotation = _visualRootBaseRotation;", self.source)

    def test_animator_and_procedural_pose_drivers_are_exclusive(self):
        self.assertIn("private enum PoseDriverMode", self.source)
        self.assertIn("if (_poseDriverMode == PoseDriverMode.Animator)", self.source)
        self.assertIn("if (_poseDriverMode != PoseDriverMode.Procedural) return;", self.source)
        self.assertIn("_animator.applyRootMotion = false;", self.source)
        self.assertIn("_animator.updateMode = AnimatorUpdateMode.Normal;", self.source)
        self.assertIn("DisableOtherAnimators(_animator);", self.source)
        self.assertRegex(self.source, r"ProceduralPoseLayerActive\s*=>")
        self.assertRegex(self.source, r"AnimatorPoseLayerActive\s*=>")

    def test_bone_matching_claims_each_transform_and_rejects_ambiguous_or_wrong_roles(self):
        self.assertIn("claimedBones = new HashSet<Transform>()", self.source)
        self.assertIn("claimed.Contains(bone)", self.source)
        self.assertIn("claimed.Add(best)", self.source)
        self.assertIn("bool tiedForBest", self.source)
        self.assertIn("bindingConflict = true", self.source)
        self.assertIn("IsInvalidBoneCandidate", self.source)
        self.assertIn("candidate.GetComponent<Renderer>()", self.source)
        self.assertIn("IsIncompatibleBoneName", self.source)
        self.assertIn("HasOppositeShortSide", self.source)
        self.assertIn('"upperarm", "uparm", "arm"', self.source)
        self.assertIn('"upperleg", "upleg", "thigh", "femur"', self.source)
        self.assertIn('"lowerleg", "calf", "shin", "leg"', self.source)

    def test_action_contract_and_stable_fallbacks_remain_present(self):
        action_block = re.search(
            r"public enum D2CharacterAction\s*\{(?P<body>.*?)\n\s*\}",
            self.source,
            re.DOTALL,
        )
        self.assertIsNotNone(action_block)
        actions = re.findall(r"^\s*(Idle|Walk|Run|Jump|Boarding|Disembarking),?\s*$", action_block.group("body"), re.MULTILINE)
        self.assertEqual(
            actions,
            ["Idle", "Walk", "Run", "Jump", "Boarding", "Disembarking"],
        )
        self.assertIn("_poseDriverMode = PoseDriverMode.Neutral;", self.source)
        self.assertIn("_animator.enabled = false;", self.source)
        self.assertIn("HasCompleteProceduralRig()", self.source)
        self.assertIn("PlayTransition(D2CharacterAction action)", self.source)

    def test_swimming_fallback_uses_a_visible_but_bounded_stroke(self):
        self.assertIn("float stroke = Mathf.Sin(_phase * 1.6f);", self.source)
        self.assertIn("pose.ArmSwing = ProceduralSwimmingUpperArmReachDegrees + stroke * 10f;", self.source)
        self.assertIn("leftLowerArmAngle = ProceduralSwimmingForearmReachDegrees - stroke * 6f;", self.source)
        self.assertIn("rightLowerArmAngle = ProceduralSwimmingForearmReachDegrees + stroke * 6f;", self.source)
        self.assertIn("pose.LeftUpperLeg = 4f - stroke * 3f;", self.source)
        self.assertIn("pose.LeftLowerLeg = 3f + Mathf.Abs(stroke) * 2.5f;", self.source)
        self.assertIn("roll = 0f;", self.source)
        self.assertIn("const float swimBodyDrop = -0.62f;", self.source)
        self.assertIn("public float SwimmingVisualRollDegrees", self.source)

    def test_ship_control_has_a_fallback_only_symmetric_helm_pose(self):
        self.assertIn("public bool ShipSteeringPoseActive", self.source)
        self.assertIn("SetShipControlActive(bool active)", self.source)
        self.assertIn("_shipControlActive", self.source)
        self.assertIn("const float shipSteeringArmAngle = -35f;", self.source)
        self.assertIn("const float shipSteeringForearmAngle = -10f;", self.source)
        self.assertIn("leftArmAngle = shipSteeringArmAngle;", self.source)
        self.assertIn("rightArmAngle = shipSteeringArmAngle;", self.source)
        self.assertIn("leftLowerArmAngle = shipSteeringForearmAngle;", self.source)
        self.assertIn("rightLowerArmAngle = shipSteeringForearmAngle;", self.source)


if __name__ == "__main__":
    unittest.main()
