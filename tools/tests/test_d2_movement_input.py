"""源码级回归：D2 InitialHarbor 移动输入只保留一个消费入口。"""

from pathlib import Path
import math
import unittest


ROOT = Path(__file__).resolve().parents[2]
SOURCE_PATH = ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D2" / "D2RuntimeBootstrap.cs"


def method_body(source: str, signature: str, start_at: int = 0) -> str:
    start = source.index(signature, start_at)
    opening = source.index("{", start)
    depth = 0
    for index in range(opening, len(source)):
        if source[index] == "{":
            depth += 1
        elif source[index] == "}":
            depth -= 1
            if depth == 0:
                return source[opening + 1:index]
    raise AssertionError(f"unclosed method: {signature}")


def resolve_axis(axis, input_enabled, gameplay_allowed, primary_allowed):
    if not (input_enabled and gameplay_allowed and primary_allowed):
        return (0.0, 0.0)
    length = math.hypot(*axis)
    if length <= 1.0:
        return axis
    return (axis[0] / length, axis[1] / length)


class D2MovementInputSourceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = SOURCE_PATH.read_text(encoding="utf-8")
        cls.controller_start = cls.source.index("public sealed class D2CharacterController")
        cls.update = method_body(cls.source, "private void Update()", cls.controller_start)
        cls.selector_start = cls.source.index("public sealed class D2MovementInputSourceSelector")
        cls.read_axis = method_body(cls.source, "public Vector2 ReadAxis()", cls.selector_start)

    def test_update_has_both_permission_guards_before_axis_read(self):
        self.assertIn("D7ControlCoordinator.GameplayInputAllowed", self.update)
        self.assertIn("RuntimeHudCoordinator.PrimaryInputAllowed", self.update)
        self.assertIn("D2MovementInputPolicy.IsMovementInputAllowed", self.update)
        self.assertIn("_movementInputSource.ReadAxis()", self.update)
        self.assertLess(
            self.update.index("D2MovementInputPolicy.IsMovementInputAllowed"),
            self.update.index("_movementInputSource.ReadAxis()"),
        )

    def test_update_does_not_merge_d16_and_legacy_axes(self):
        self.assertNotIn("D16InputRouter.MoveAxis()", self.update)
        self.assertNotIn("ReadLegacyMoveAxis()", self.update)
        self.assertEqual(self.update.count("_movementInputSource.ReadAxis()"), 1)
        self.assertNotIn("legacyInput", self.update)
        self.assertNotIn("legacyInput.sqrMagnitude > input.sqrMagnitude", self.source)

    def test_blocked_branch_clears_horizontal_state_without_rotation(self):
        gate = self.update.index("if (!D2MovementInputPolicy.IsMovementInputAllowed")
        read = self.update.index("_movementInputSource.ReadAxis()")
        blocked_branch = self.update[gate:read]
        self.assertIn("_moveDirection = Vector3.zero", blocked_branch)
        self.assertIn("_planarVelocity = Vector3.zero", blocked_branch)
        self.assertIn("ApplyPhysicsMotion();", blocked_branch)
        self.assertIn("return;", blocked_branch)
        self.assertNotIn("Quaternion.LookRotation", blocked_branch)

    def test_fallback_is_latched_and_never_selected_from_zero_axis(self):
        self.assertIn("if (IsLegacyFallbackActive)", self.read_axis)
        self.assertIn("ActivateLegacyFallback();", self.read_axis)
        self.assertIn("return Vector2.zero;", self.read_axis)
        self.assertIn("LegacyFallbackActivationCount", self.source)
        self.assertNotIn("_d16AxisReader().sqrMagnitude", self.source)
        self.assertNotIn("_legacyAxisReader().sqrMagnitude", self.source)

    def test_permission_model_covers_modal_hud_block_and_focus_recovery(self):
        self.assertEqual(resolve_axis((0.8, 0.6), True, False, True), (0.0, 0.0))
        self.assertEqual(resolve_axis((0.8, 0.6), True, True, False), (0.0, 0.0))
        self.assertEqual(resolve_axis((0.0, 1.0), True, False, True), (0.0, 0.0))
        self.assertEqual(resolve_axis((0.0, 1.0), True, True, True), (0.0, 1.0))


if __name__ == "__main__":
    unittest.main()
