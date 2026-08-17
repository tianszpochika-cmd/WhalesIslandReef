"""Unit tests for the D18-L27 material/texture descriptor validator.

Covers every rejection rule (R1—R11) plus a valid descriptor; the tool must
be deterministic, offline, read-only and never touch Unity / image pixels.
"""

import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_material_descriptor_validator import validate_descriptors, validate_descriptor

VALID_DESCRIPTOR = {
    "descriptor_kind": "d18_material_descriptor",
    "source_asset_id": "asset_fixture_material_01",
    "category": "material",
    "maps": [
        {"map_name": "albedo", "map_role": "albedo", "channels": "rgb", "color_space_intent": "srgb", "source_path": "docs/fixtures/albedo.png"},
        {"map_name": "normal", "map_role": "normal", "channels": "xyz", "color_space_intent": "linear", "source_path": "docs/fixtures/normal.png"},
        {"map_name": "roughness", "map_role": "roughness", "channels": "r", "color_space_intent": "linear", "source_path": "docs/fixtures/rough.png"},
        {"map_name": "ao", "map_role": "ao", "channels": "r", "color_space_intent": "linear", "source_path": "docs/fixtures/ao.png"},
    ],
    "pbr_set_complete": True,
    "license_dependency": {
        "license_id": "CC0-1.0",
        "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
        "license_evidence_path": "LICENSE_FIXTURE.txt",
        "attribution_required": False,
    },
    "rights": {
        "commercial_use": True,
        "modification_allowed": True,
        "repository_storage": True,
        "distribution_with_game": True,
    },
    "fallback": {"reference": "placeholder_shared_lod2", "description": "keep placeholder material"},
    "runtime_deferred": ["actual image inspection and Unity material import are deferred to Runtime"],
    "represents_imported_asset": False,
}


class MaterialDescriptorValidatorTests(unittest.TestCase):
    def setUp(self) -> None:
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name).resolve()
        (self.root / "LICENSE_FIXTURE.txt").write_text("CC0 1.0", encoding="utf-8")
        self.records = [{"canonical_asset_id": "asset_fixture_material_01"}]

    def tearDown(self) -> None:
        self._tmp.cleanup()

    def _check(self, descriptor, records=None):
        return validate_descriptor(descriptor, self.root, records=records if records is not None else self.records)

    def _codes(self, result):
        return [issue["code"] for issue in result["issues"]]

    def test_valid_material_descriptor_passes(self):
        result = self._check(dict(VALID_DESCRIPTOR))
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["error_count"], 0)

    def test_valid_without_records_still_passes(self):
        result = validate_descriptor(dict(VALID_DESCRIPTOR), self.root, records=None)
        self.assertEqual(result["status"], "PASS")

    # R1 asset_id
    def test_invalid_asset_id_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, source_asset_id="game/Assets/Art/Candidates/foo.png")
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_asset_id", self._codes(result))

    def test_unknown_asset_id_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, source_asset_id="asset_does_not_exist_01")
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("asset_id_not_found", self._codes(result))

    # R2 category
    def test_unknown_category_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, category="rock")
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("unknown_category", self._codes(result))

    # R3 category<->map
    def test_texture_with_pbr_maps_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, category="texture")
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("category_map_conflict", self._codes(result))

    def test_hdri_needs_exactly_one_hdri_map(self):
        descriptor = dict(VALID_DESCRIPTOR, category="hdri", maps=[
            {"map_role": "hdri", "channels": "rgb", "color_space_intent": "linear"},
            {"map_role": "albedo", "channels": "rgb", "color_space_intent": "srgb"},
        ])
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("category_map_conflict", self._codes(result))

    def test_font_with_maps_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, category="font", maps=[])
        descriptor["maps"] = [{"map_role": "misc", "channels": "rgba", "color_space_intent": "srgb"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("category_map_conflict", self._codes(result))

    # R4 map roles
    def test_duplicate_map_role_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [
            {"map_role": "albedo", "channels": "rgb", "color_space_intent": "srgb"},
            {"map_role": "albedo", "channels": "rgb", "color_space_intent": "srgb"},
        ]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("duplicate_map_role", self._codes(result))

    def test_unknown_map_role_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [{"map_role": "bump_tangent", "channels": "rgb", "color_space_intent": "srgb"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("unknown_map_role", self._codes(result))

    # R5 channels
    def test_wrong_channels_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [{"map_role": "albedo", "channels": "r", "color_space_intent": "srgb"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_channels", self._codes(result))

    def test_normal_with_rgb_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [{"map_role": "normal", "channels": "rgb", "color_space_intent": "linear"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_channels", self._codes(result))

    # R6 color space
    def test_missing_color_space_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [{"map_role": "albedo", "channels": "rgb"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_color_space", self._codes(result))

    def test_invalid_color_space_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [{"map_role": "albedo", "channels": "rgb", "color_space_intent": "gamma_2_2_import"}]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_color_space", self._codes(result))

    # R7 PBR completeness (AO missing cannot fake a full set)
    def test_pbr_complete_without_ao_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["maps"] = [
            {"map_role": "albedo", "channels": "rgb", "color_space_intent": "srgb"},
            {"map_role": "normal", "channels": "xyz", "color_space_intent": "linear"},
            {"map_role": "roughness", "channels": "r", "color_space_intent": "linear"},
        ]
        descriptor["pbr_set_complete"] = True
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("pbr_incomplete", self._codes(result))

    # R8 license
    def test_nc_license_rejected(self):
        descriptor = json.loads(json.dumps(VALID_DESCRIPTOR))
        descriptor["license_dependency"]["license_id"] = "CC-BY-NC-4.0"
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_license", self._codes(result))

    def test_unknown_license_rejected(self):
        descriptor = json.loads(json.dumps(VALID_DESCRIPTOR))
        descriptor["license_dependency"]["license_id"] = "some-mystery-license"
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_license", self._codes(result))

    def test_missing_license_evidence_rejected(self):
        descriptor = json.loads(json.dumps(VALID_DESCRIPTOR))
        descriptor["license_dependency"]["license_evidence_path"] = "NOPE_MISSING.txt"
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("license_evidence_missing_file", self._codes(result))

    def test_rights_not_explicit_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["rights"] = {"commercial_use": True, "modification_allowed": True, "repository_storage": True}
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("rights_not_explicit", self._codes(result))

    # R9 fallback
    def test_empty_fallback_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["fallback"] = {"reference": "", "description": ""}
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("empty_fallback", self._codes(result))

    def test_missing_fallback_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        del descriptor["fallback"]
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_fallback", self._codes(result))

    def test_fallback_to_candidate_binary_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["fallback"] = {"reference": "Assets/Art/Candidates/Kenney/foo.png", "description": "use candidate"}
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("fallback_candidate_binary", self._codes(result))

    # R10 runtime_deferred
    def test_missing_runtime_deferred_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR)
        descriptor["runtime_deferred"] = []
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_runtime_deferred", self._codes(result))

    # R11 not imported
    def test_claims_imported_rejected(self):
        descriptor = dict(VALID_DESCRIPTOR, represents_imported_asset=True)
        result = self._check(descriptor)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("claims_imported", self._codes(result))

    # aggregate + read-only + determinism
    def test_batch_aggregate(self):
        valid = json.loads(json.dumps(VALID_DESCRIPTOR))
        invalid = dict(VALID_DESCRIPTOR, category="rock")
        result = validate_descriptors([valid, invalid], self.root, records=self.records)
        self.assertEqual(result["status"], "FAIL")
        self.assertEqual(result["descriptor_count"], 2)
        self.assertEqual(result["error_count"], 1)
        self.assertEqual(result["approved_count"], 0)

    def test_read_only_and_deterministic(self):
        before = (self.root / "LICENSE_FIXTURE.txt").read_bytes()
        first = self._check(dict(VALID_DESCRIPTOR))
        second = self._check(dict(VALID_DESCRIPTOR))
        self.assertEqual(first, second)
        self.assertEqual((self.root / "LICENSE_FIXTURE.txt").read_bytes(), before)

    def test_json_serialisable(self):
        json.dumps(self._check(dict(VALID_DESCRIPTOR)), ensure_ascii=False)


if __name__ == "__main__":
    unittest.main()
