"""Unit tests for the D18-L26 mesh descriptor validator (read-only, offline).

Positive fixtures (must PASS):
- complete character descriptor with matching L02 asset record
- ship descriptor with sail animation, convex_mesh and LOD intent
- environment terrain descriptor
- fish descriptor with swim animation and capsule_agent
- non-geometric ui / audio descriptors (all geometry fields empty)
- root_motion=required backed by a root-motion clip
- descriptor without --asset-records (no source resolution needed)

Negative fixtures (must be BLOCKED; every P0 field of the task card is covered
with field-level errors): units enum, axes enum, axes unknown field, root-motion
enum, root-motion without clips, bounds positive (zero/negative), geometric
bounds required, collision intent enum, collision intent mismatch, LOD levels
range, geometric LOD required, category mismatch, unresolved source id, missing
fallback, fallback reference format, empty fallback description, imported claim
invalid, unknown top-level field (Unity import settings), skeleton intent
mismatch, non-geometric geometry, non-geometric material slots.

P1 warnings: unknown units/axes/root-motion/collision-intent, suspicious bounds
ratio, animation role mismatch, manual LOD generation, placeholder fallback
consistency.

The tool never reads FBX/OBJ, never calls Unity and never modifies inputs.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_mesh_descriptor_validator import (
    validate_descriptor,
    validate_file,
    load_asset_records,
)


def _descriptor(**overrides: object) -> dict:
    base = {
        "descriptor_kind": "d18_import_descriptor",
        "schema_version": 1,
        "source_asset_id": "asset_kenney_pirate_character_a",
        "type": "character",
        "units": "meters",
        "axes": {"up": "y", "forward": "z"},
        "bounds": {"width_m": 1.0, "depth_m": 1.0, "height_m": 1.8},
        "material_slots": [
            {"slot_name": "body", "slot_role": "body"},
            {"slot_name": "outfit", "slot_role": "outfit"},
        ],
        "skeleton_type": "humanoid",
        "animation_list": [
            {"animation_key": "idle", "role": "idle"},
            {"animation_key": "walk", "role": "walk"},
        ],
        "root_motion": "none",
        "collision_intent": "capsule_agent",
        "lod_intent": {"levels": 3, "generate_automatically": True},
        "license_dependency": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/Kenney/PirateKit/License.txt",
            "attribution_required": False,
        },
        "fallback": {"reference": "placeholder_ph_character_a", "description": "keep placeholder until Runtime import"},
        "represents_imported_asset": False,
    }
    base.update(overrides)
    return base


def _asset_record(category: str) -> dict:
    return {"canonical_asset_id": "asset_kenney_pirate_character_a", "category": category}


def _records_index(*records: dict) -> dict:
    return {str(record.get("canonical_asset_id", "")): record for record in records}


class MeshDescriptorValidatorTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self) -> None:
        self._tmp.cleanup()

    def _p0_rules(self, descriptor: dict) -> set:
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        return {f["rule"] for f in findings if f["severity"] == "P0"}

    # ------------------------------------------------------------------
    # positive fixtures
    # ------------------------------------------------------------------

    def test_complete_character_passes(self):
        descriptor = _descriptor()
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_ship_descriptor_passes(self):
        descriptor = _descriptor(
            source_asset_id="asset_kenney_pirate_ship_medium",
            type="ship",
            skeleton_type="static",
            collision_intent="convex_mesh",
            animation_list=[{"animation_key": "sail", "role": "sail"}],
            material_slots=[{"slot_name": "hull", "slot_role": "wood"}],
            lod_intent={"levels": 2, "generate_automatically": True},
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_pirate_ship_medium", "category": "ship"}))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_environment_terrain_passes(self):
        descriptor = _descriptor(
            source_asset_id="asset_kenney_nature_ground",
            type="environment",
            skeleton_type="none",
            collision_intent="terrain",
            animation_list=[],
            material_slots=[{"slot_name": "ground", "slot_role": "organic"}],
            lod_intent={"levels": 1, "generate_automatically": True},
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_nature_ground", "category": "environment"}))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_fish_descriptor_passes(self):
        descriptor = _descriptor(
            source_asset_id="asset_quaternius_fish_dolphin",
            type="fish",
            skeleton_type="fish",
            collision_intent="capsule_agent",
            animation_list=[{"animation_key": "swim", "role": "swim"}],
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_quaternius_fish_dolphin", "category": "fish"}))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_non_geometric_ui_passes(self):
        descriptor = _descriptor(
            source_asset_id="asset_kenney_ui_pack_v2",
            type="ui",
            units="unknown",
            bounds=None,
            material_slots=[],
            skeleton_type="none",
            animation_list=[],
            root_motion="none",
            collision_intent="none",
            lod_intent=None,
            axes={"up": "y", "forward": "z"},
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_ui_pack_v2", "category": "ui"}))
        p0 = [f for f in findings if f["severity"] == "P0"]
        self.assertEqual(p0, [])
        p1_rules = {f["rule"] for f in findings if f["severity"] == "P1"}
        self.assertIn("units_unknown", p1_rules)

    def test_non_geometric_audio_passes(self):
        descriptor = _descriptor(
            source_asset_id="asset_kenney_ui_audio_v1",
            type="audio",
            units="unknown",
            axes={"up": "unknown", "forward": "unknown"},
            bounds=None,
            material_slots=[],
            skeleton_type="none",
            animation_list=[],
            root_motion="none",
            collision_intent="none",
            lod_intent=None,
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_ui_audio_v1", "category": "audio"}))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_root_motion_required_with_clip_passes(self):
        descriptor = _descriptor(
            root_motion="required",
            animation_list=[
                {"animation_key": "walk", "role": "walk", "root_motion": True},
            ],
        )
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_without_asset_records_no_source_check(self):
        descriptor = _descriptor()
        findings = validate_descriptor(descriptor, None)
        self.assertEqual([f for f in findings if f["severity"] == "P0"], [])

    def test_multi_descriptor_file_passes(self):
        file = self.root / "descriptors.json"
        file.write_text(
            json.dumps([_descriptor(), _descriptor(source_asset_id="asset_second", animation_list=[])]),
            encoding="utf-8",
        )
        records = _records_index(_asset_record("character"))
        records["asset_second"] = {"canonical_asset_id": "asset_second", "category": "character"}
        result = validate_file(file, records)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["descriptor_count"], 2)

    def test_asset_records_loader_accepts_both_shapes(self):
        array = self.root / "array.json"
        array.write_text(json.dumps([_asset_record("character")]), encoding="utf-8")
        self.assertEqual(load_asset_records(array), _records_index(_asset_record("character")))
        wrapped = self.root / "wrapped.json"
        wrapped.write_text(json.dumps({"records": [_asset_record("character")]}), encoding="utf-8")
        self.assertEqual(load_asset_records(wrapped), _records_index(_asset_record("character")))

    # ------------------------------------------------------------------
    # negative fixtures: every P0 field of the task card
    # ------------------------------------------------------------------

    def test_units_illegal_value_blocks(self):
        rules = self._p0_rules(_descriptor(units="yards"))
        self.assertIn("units_enum", rules)
        finding = validate_descriptor(_descriptor(units="yards"), _records_index(_asset_record("character")))[0]
        self.assertEqual(finding["field"], "units")

    def test_axes_illegal_up_blocks(self):
        descriptor = _descriptor()
        descriptor["axes"] = {"up": "x", "forward": "z"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("axes_up_enum", {f["rule"] for f in findings})
        self.assertEqual(findings[0]["field"], "axes.up")

    def test_axes_illegal_forward_blocks(self):
        descriptor = _descriptor()
        descriptor["axes"] = {"up": "y", "forward": "north"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("axes_forward_enum", {f["rule"] for f in findings})

    def test_axes_unknown_field_blocks(self):
        descriptor = _descriptor()
        descriptor["axes"] = {"up": "y", "forward": "z", "left": "x"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("unknown_axes_field", {f["rule"] for f in findings})

    def test_root_motion_illegal_value_blocks(self):
        rules = self._p0_rules(_descriptor(root_motion="baked"))
        self.assertIn("root_motion_enum", rules)

    def test_root_motion_required_without_clips_blocks(self):
        rules = self._p0_rules(_descriptor(root_motion="required", animation_list=[]))
        self.assertIn("root_motion_without_clips", rules)

    def test_bounds_negative_blocks(self):
        rules = self._p0_rules(_descriptor())
        descriptor = _descriptor()
        descriptor["bounds"] = {"width_m": 1.0, "depth_m": -1.0, "height_m": 1.8}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("bounds_positive", {f["rule"] for f in findings})
        self.assertTrue(any(f["field"] == "bounds.depth_m" for f in findings))

    def test_bounds_zero_blocks(self):
        descriptor = _descriptor()
        descriptor["bounds"] = {"width_m": 0.0, "depth_m": 1.0, "height_m": 1.8}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("bounds_positive", {f["rule"] for f in findings})

    def test_geometric_bounds_required_blocks(self):
        rules = self._p0_rules(_descriptor(bounds=None))
        self.assertIn("geometric_bounds_required", rules)

    def test_collision_intent_illegal_value_blocks(self):
        rules = self._p0_rules(_descriptor(collision_intent="water"))
        self.assertIn("collision_intent_enum", rules)

    def test_collision_intent_mismatch_blocks(self):
        rules = self._p0_rules(_descriptor(collision_intent="none"))
        self.assertIn("collision_intent_mismatch", rules)

    def test_lod_levels_out_of_range_blocks(self):
        descriptor = _descriptor()
        descriptor["lod_intent"] = {"levels": 6, "generate_automatically": True}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("lod_levels_range", {f["rule"] for f in findings})

    def test_lod_levels_non_integer_blocks(self):
        descriptor = _descriptor()
        descriptor["lod_intent"] = {"levels": 2.5, "generate_automatically": True}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("lod_levels_range", {f["rule"] for f in findings})

    def test_geometric_missing_lod_intent_blocks(self):
        rules = self._p0_rules(_descriptor(lod_intent=None))
        self.assertIn("geometric_missing_lod_intent", rules)

    def test_category_mismatch_blocks(self):
        descriptor = _descriptor(type="prop", skeleton_type="static", collision_intent="simple_box")
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("category_mismatch", {f["rule"] for f in findings})
        mismatch = next(f for f in findings if f["rule"] == "category_mismatch")
        self.assertEqual(mismatch["field"], "type")

    def test_unresolved_source_asset_id_blocks(self):
        descriptor = _descriptor(source_asset_id="asset_unknown_record")
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("source_asset_id_unresolved", {f["rule"] for f in findings})

    def test_missing_fallback_blocks(self):
        descriptor = _descriptor()
        del descriptor["fallback"]
        rules = self._p0_rules(descriptor)
        self.assertIn("fallback_required", rules)
        self.assertIn("required_field_missing", rules)

    def test_fallback_reference_format_blocks(self):
        rules = self._p0_rules(_descriptor())
        descriptor = _descriptor()
        descriptor["fallback"] = {"reference": "ph_icon", "description": "keep placeholder"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("fallback_reference_format", {f["rule"] for f in findings})

    def test_fallback_empty_description_blocks(self):
        descriptor = _descriptor()
        descriptor["fallback"] = {"reference": "placeholder_ph_a", "description": ""}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("fallback_description", {f["rule"] for f in findings})

    def test_imported_claim_invalid_blocks(self):
        rules = self._p0_rules(_descriptor(represents_imported_asset=True))
        self.assertIn("imported_claim_invalid", rules)

    def test_unknown_top_level_field_blocks(self):
        descriptor = _descriptor()
        descriptor["import_settings"] = {"mesh_compression": "high"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("unknown_top_level_field", {f["rule"] for f in findings})

    def test_skeleton_intent_mismatch_blocks(self):
        rules = self._p0_rules(_descriptor(skeleton_type="none"))
        self.assertIn("skeleton_intent_mismatch", rules)

    def test_non_geometric_bounds_blocks(self):
        descriptor = _descriptor(
            type="ui",
            bounds={"width_m": 1.0, "depth_m": 1.0, "height_m": 1.0},
            skeleton_type="none",
            collision_intent="none",
            root_motion="none",
            lod_intent=None,
            material_slots=[],
            animation_list=[],
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_ui_pack_v2", "category": "ui"}))
        self.assertIn("non_geometric_geometry", {f["rule"] for f in findings})

    def test_non_geometric_material_slots_blocks(self):
        descriptor = _descriptor(
            type="audio",
            bounds=None,
            skeleton_type="none",
            collision_intent="none",
            root_motion="none",
            lod_intent=None,
            animation_list=[],
            material_slots=[{"slot_name": "x", "slot_role": "misc"}],
        )
        findings = validate_descriptor(descriptor, _records_index({"canonical_asset_id": "asset_kenney_ui_audio_v1", "category": "audio"}))
        self.assertIn("non_geometric_material_slots", {f["rule"] for f in findings})

    def test_license_evidence_path_escape_blocks(self):
        descriptor = _descriptor()
        descriptor["license_dependency"]["license_evidence_path"] = "C:/Users/Administrator/secret.txt"
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertIn("evidence_path_format", {f["rule"] for f in findings})

    def test_descriptor_json_invalid_blocks(self):
        file = self.root / "bad.json"
        file.write_text("{not json", encoding="utf-8")
        result = validate_file(file, None)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("descriptor_json_invalid", result["error_counts"])

    # ------------------------------------------------------------------
    # P1 warnings
    # ------------------------------------------------------------------

    def test_unknown_units_is_p1(self):
        descriptor = _descriptor(units="unknown")
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        warning = next(f for f in findings if f["rule"] == "units_unknown")
        self.assertEqual(warning["severity"], "P1")

    def test_unknown_axes_is_p1(self):
        descriptor = _descriptor()
        descriptor["axes"] = {"up": "unknown", "forward": "unknown"}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        self.assertTrue(any(f["rule"] == "axes_unknown" and f["severity"] == "P1" for f in findings))

    def test_root_motion_unknown_is_p1(self):
        descriptor = _descriptor(root_motion="unknown")
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        warning = next(f for f in findings if f["rule"] == "root_motion_unknown")
        self.assertEqual(warning["severity"], "P1")

    def test_suspicious_bounds_ratio_is_p1(self):
        descriptor = _descriptor()
        descriptor["bounds"] = {"width_m": 0.01, "depth_m": 1.0, "height_m": 1.8}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        warning = next(f for f in findings if f["rule"] == "bounds_ratio_suspect")
        self.assertEqual(warning["severity"], "P1")

    def test_animation_role_mismatch_is_p1(self):
        descriptor = _descriptor(animation_list=[{"animation_key": "sail", "role": "sail"}])
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        warning = next(f for f in findings if f["rule"] == "animation_role_mismatch")
        self.assertEqual(warning["severity"], "P1")

    def test_manual_lod_generation_is_p1(self):
        descriptor = _descriptor()
        descriptor["lod_intent"] = {"levels": 3, "generate_automatically": False}
        findings = validate_descriptor(descriptor, _records_index(_asset_record("character")))
        warning = next(f for f in findings if f["rule"] == "lod_manual_generation")
        self.assertEqual(warning["severity"], "P1")

    # ------------------------------------------------------------------
    # contracts
    # ------------------------------------------------------------------

    def test_tool_is_read_only(self):
        file = self.root / "descriptor.json"
        file.write_text(json.dumps(_descriptor()), encoding="utf-8")
        before = file.read_text(encoding="utf-8")
        validate_file(file, None)
        self.assertEqual(file.read_text(encoding="utf-8"), before)

    def test_result_json_serialisable(self):
        result = validate_file(self.root / "x.json", None) if False else None
        # 直接验证 validate_descriptor 输出可序列化
        findings = validate_descriptor(_descriptor(), _records_index(_asset_record("character")))
        json.dumps(findings, ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
