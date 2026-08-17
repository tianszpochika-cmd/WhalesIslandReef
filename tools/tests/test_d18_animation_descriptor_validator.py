"""Tests for tools/d18_animation_descriptor_validator.py (D18-L28)."""

import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_animation_descriptor_validator import validate_descriptors  # noqa: E402


def base_descriptor(**overrides) -> dict:
    descriptor = {
        "descriptor_kind": "d18_animation_descriptor",
        "descriptor_schema_version": 1,
        "canonical_asset_id": "asset_quaternius_male_casual_v01",
        "type": "character",
        "skeleton_type": "humanoid",
        "animation_list": [
            {"animation_key": "idle", "role": "idle", "looping": True},
            {"animation_key": "walk", "role": "walk", "looping": True, "root_motion": True},
            {"animation_key": "interact", "role": "interact", "looping": False},
        ],
        "root_motion": "required",
        "action_intent_map": {"walk": "walk", "interact": "interact"},
        "state_machine_owner": "player",
        "fallback": {"reference": "asset_ph_crew_male", "description": "keep placeholder on import failure"},
        "license_dependency": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/license.txt",
            "attribution_required": False,
        },
        "represents_imported_asset": False,
    }
    descriptor.update(overrides)
    return descriptor


class D18AnimationDescriptorValidatorTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.addCleanup(self._tmp.cleanup)
        lic = self.root / "game/Assets/Art/Candidates/license.txt"
        lic.parent.mkdir(parents=True, exist_ok=True)
        lic.write_text("CC0-1.0", encoding="utf-8")

    def validate(self, descriptor):
        return validate_descriptors(self.root, [descriptor])

    def first(self, result):
        return result["descriptors"][0]

    def codes(self, result):
        return [issue["code"] for issue in self.first(result)["issues"]]

    def test_valid_player_descriptor_passes(self):
        result = self.validate(base_descriptor())
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["approved_count"], 0)
        self.assertEqual(self.first(result)["status"], "PASS")
        self.assertEqual(self.first(result)["missing_required_actions"], [])

    def test_valid_npc_descriptor_passes(self):
        result = self.validate(base_descriptor(state_machine_owner="npc"))
        self.assertEqual(result["status"], "PASS")

    def test_animal_descriptor_passes(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_quaternius_deer_v01",
            type="animal", skeleton_type="animal", state_machine_owner="animal",
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "walk", "role": "walk", "looping": True},
            ],
            root_motion="none",
            action_intent_map={"walk": "walk"},
        ))
        self.assertEqual(result["status"], "PASS")

    def test_fish_descriptor_requires_swim(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_quaternius_shark_v01",
            type="fish", skeleton_type="fish", state_machine_owner="animal",
            animation_list=[{"animation_key": "swim", "role": "swim", "looping": True}],
            root_motion="none",
            action_intent_map={},
        ))
        self.assertEqual(result["status"], "PASS")

    def test_ship_descriptor_requires_sail(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_ship_moonwake_anim",
            type="ship", skeleton_type="static", state_machine_owner="none",
            animation_list=[{"animation_key": "sail_trim", "role": "sail", "looping": False}],
            root_motion="none",
            action_intent_map={},
        ))
        self.assertEqual(result["status"], "PASS")

    def test_missing_required_action_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "interact", "role": "interact", "looping": False},
            ],
            action_intent_map={"interact": "interact"},
        ))
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_required_action", self.codes(result))
        self.assertEqual(self.first(result)["missing_required_actions"], ["walk"])

    def test_fish_missing_swim_fails(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_quaternius_fish_01_v01",
            type="fish", skeleton_type="fish", state_machine_owner="animal",
            animation_list=[{"animation_key": "emote", "role": "emote", "looping": False}],
            root_motion="none", action_intent_map={},
        ))
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_required_action", self.codes(result))
        self.assertEqual(self.first(result)["missing_required_actions"], ["swim"])

    def test_role_type_conflict_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[{"animation_key": "swim", "role": "swim", "looping": True}],
            action_intent_map={"walk": "walk"},
        ))
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("role_type_conflict", self.codes(result))

    def test_fish_walk_role_conflict_fails(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_quaternius_shark_v01",
            type="fish", skeleton_type="fish", state_machine_owner="animal",
            animation_list=[{"animation_key": "walk", "role": "walk", "looping": True}],
            root_motion="none", action_intent_map={"walk": "walk"},
        ))
        self.assertIn("role_type_conflict", self.codes(result))

    def test_skeleton_type_conflict_fails(self):
        result = self.validate(base_descriptor(skeleton_type="animal"))
        self.assertIn("skeleton_type_conflict", self.codes(result))

    def test_static_skeleton_with_clips_fails(self):
        result = self.validate(base_descriptor(skeleton_type="none"))
        self.assertIn("skeleton_animation_conflict", self.codes(result))

    def test_static_hull_ship_can_carry_sail_clip(self):
        result = self.validate(base_descriptor(
            canonical_asset_id="asset_ship_harbor_brig_anim",
            type="ship", skeleton_type="static", state_machine_owner="none",
            animation_list=[{"animation_key": "sail_trim", "role": "sail", "looping": False}],
            root_motion="none", action_intent_map={},
        ))
        self.assertEqual(result["status"], "PASS")

    def test_non_geometric_with_clips_fails(self):
        result = self.validate(base_descriptor(
            type="audio", skeleton_type="none", state_machine_owner="none",
            animation_list=[{"animation_key": "cue", "role": "other"}],
            action_intent_map={}, root_motion="none",
        ))
        self.assertIn("role_type_conflict", self.codes(result))

    def test_unknown_action_intent_fails(self):
        result = self.validate(base_descriptor(action_intent_map={"dance": "walk"}))
        self.assertIn("unknown_action_intent", self.codes(result))

    def test_action_intent_missing_clip_fails(self):
        result = self.validate(base_descriptor(action_intent_map={"walk": "missing_clip"}))
        self.assertIn("action_intent_missing_clip", self.codes(result))

    def test_action_intent_role_conflict_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "swim_clip", "role": "interact", "looping": False},
            ],
            action_intent_map={"walk": "swim_clip", "interact": "swim_clip"},
        ))
        self.assertIn("action_intent_role_conflict", self.codes(result))

    def test_missing_fallback_fails(self):
        descriptor = base_descriptor()
        del descriptor["fallback"]
        result = self.validate(descriptor)
        self.assertIn("missing_fallback", self.codes(result))

    def test_empty_fallback_description_fails(self):
        result = self.validate(base_descriptor(fallback={"reference": "asset_ph_crew_male", "description": ""}))
        self.assertIn("missing_fallback", self.codes(result))

    def test_invalid_fallback_reference_fails(self):
        result = self.validate(base_descriptor(fallback={"reference": "game/Assets/Models/x.fbx", "description": "x"}))
        self.assertIn("invalid_fallback_reference", self.codes(result))

    def test_nc_license_dependency_fails(self):
        result = self.validate(base_descriptor(license_dependency={
            "license_id": "CC-BY-NC-4.0",
            "license_url": "https://creativecommons.org/licenses/by-nc/4.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/license.txt",
            "attribution_required": True,
        }))
        self.assertIn("license_dependency_restricted", self.codes(result))

    def test_unknown_license_dependency_fails(self):
        result = self.validate(base_descriptor(license_dependency={
            "license_id": "Totally-Unknown-License",
            "license_url": "https://example.com/license",
            "license_evidence_path": "game/Assets/Art/Candidates/license.txt",
            "attribution_required": True,
        }))
        self.assertIn("license_dependency_unusable", self.codes(result))

    def test_missing_license_evidence_file_fails(self):
        result = self.validate(base_descriptor(license_dependency={
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/missing_license.txt",
            "attribution_required": False,
        }))
        self.assertIn("license_evidence_missing", self.codes(result))

    def test_missing_state_machine_owner_fails(self):
        descriptor = base_descriptor()
        del descriptor["state_machine_owner"]
        result = self.validate(descriptor)
        self.assertIn("missing_state_machine_owner", self.codes(result))

    def test_state_machine_owner_conflict_fails(self):
        result = self.validate(base_descriptor(state_machine_owner="animal"))
        self.assertIn("state_machine_owner_conflict", self.codes(result))

    def test_looping_conflict_one_shot_looping_true_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "walk", "role": "walk", "looping": True},
                {"animation_key": "interact", "role": "interact", "looping": True},
            ],
            action_intent_map={"walk": "walk", "interact": "interact"},
        ))
        self.assertIn("looping_conflict", self.codes(result))

    def test_looping_conflict_walk_looping_false_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "walk", "role": "walk", "looping": False},
            ],
            action_intent_map={"walk": "walk"},
        ))
        self.assertIn("looping_conflict", self.codes(result))

    def test_root_motion_conflict_with_none_fails(self):
        result = self.validate(base_descriptor(
            root_motion="none",
            animation_list=[{"animation_key": "walk", "role": "walk", "looping": True, "root_motion": True}],
        ))
        self.assertIn("root_motion_conflict", self.codes(result))

    def test_root_motion_unknown_is_pending(self):
        result = self.validate(base_descriptor(root_motion="unknown"))
        self.assertEqual(result["status"], "PENDING")
        self.assertIn("root_motion_unknown", self.codes(result))

    def test_import_claim_forbidden(self):
        result = self.validate(base_descriptor(represents_imported_asset=True))
        self.assertIn("import_claim_forbidden", self.codes(result))

    def test_unity_runtime_token_forbidden(self):
        result = self.validate(base_descriptor(fallback={"reference": "asset_ph_crew_male", "description": "fallback keeps placeholder if Animator fails"}))
        self.assertIn("unity_runtime_token_forbidden", self.codes(result))

    def test_duplicate_clip_key_fails(self):
        result = self.validate(base_descriptor(
            animation_list=[
                {"animation_key": "walk", "role": "walk", "looping": True},
                {"animation_key": "walk", "role": "run", "looping": True},
            ],
            action_intent_map={"walk": "walk"},
        ))
        self.assertIn("duplicate_clip_key", self.codes(result))

    def test_empty_descriptors_is_fail(self):
        result = validate_descriptors(self.root, [])
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("no_descriptors", result["issue_counts"])

    def test_invalid_descriptor_is_fail(self):
        result = validate_descriptors(self.root, ["not-an-object"])
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("descriptor_not_object", result["issue_counts"])

    def test_deterministic_and_read_only(self):
        descriptor = base_descriptor()
        before = json.dumps(descriptor, sort_keys=True)
        first = validate_descriptors(self.root, [descriptor])
        second = validate_descriptors(self.root, [descriptor])
        self.assertEqual(json.dumps(first, sort_keys=True), json.dumps(second, sort_keys=True))
        self.assertEqual(first["approved_count"], 0)
        self.assertEqual(json.dumps(descriptor, sort_keys=True), before)

    def test_multi_descriptor_aggregation(self):
        good = base_descriptor()
        bad = base_descriptor(
            canonical_asset_id="asset_bad",
            animation_list=[
                {"animation_key": "idle", "role": "idle", "looping": True},
                {"animation_key": "interact", "role": "interact", "looping": False},
            ],
            action_intent_map={"interact": "interact"},
        )
        result = validate_descriptors(self.root, [good, bad])
        self.assertEqual(result["status"], "FAIL")
        self.assertEqual(result["pass_count"], 1)
        self.assertEqual(result["fail_count"], 1)


if __name__ == "__main__":
    unittest.main()
