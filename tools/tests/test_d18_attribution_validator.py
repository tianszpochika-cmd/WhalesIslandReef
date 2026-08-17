import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_attribution_validator import (  # noqa: E402
    ATTR_ASSET_LINK_MISSING,
    ATTR_AUTHOR_MISSING,
    ATTR_AUTHOR_PLACEHOLDER,
    ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED,
    ATTR_CREDITS_LOCATION_MISSING,
    ATTR_CREDITS_LOCATION_PLACEHOLDER,
    ATTR_INPUT_TYPE_INVALID,
    ATTR_LICENSE_EVIDENCE_PATH_INVALID,
    ATTR_LICENSE_REF_MISSING,
    ATTR_LICENSE_URL_INVALID,
    ATTR_REQUIREMENT_INVALID,
    ATTR_REQUIREMENT_UNKNOWN,
    NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE,
    WARN_BY_LICENSE_NO_ATTRIBUTION,
    WARN_CANONICAL_LICENSE_MISSING,
    localize,
    validate_attribution,
    validate_attribution_records,
)


def base_record(**overrides):
    record = {
        "canonical_asset_id": "asset_pirate_hut_01",
        "author": "Kenney",
        "license": {
            "canonical_license": "CC-BY-4.0",
            "attribution_required": True,
            "license_url": "https://creativecommons.org/licenses/by/4.0/",
        },
        "credits_location": "THIRD_PARTY_NOTICES.md",
    }
    record.update(overrides)
    return record


class D18AttributionValidatorPositiveTests(unittest.TestCase):
    def test_full_record_with_license_url_passes(self):
        result = validate_attribution(base_record())
        self.assertTrue(result["valid"])
        self.assertEqual(result["issues"], [])
        self.assertEqual(result["canonical_license"], "CC-BY-4.0")
        self.assertTrue(result["attribution_required"])

    def test_license_evidence_path_passes(self):
        result = validate_attribution(
            base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True,
                                  "license_evidence_path": "packs/characters_animals/LICENSE.txt"})
        )
        self.assertTrue(result["valid"])
        self.assertEqual(result["issues"], [])

    def test_license_text_path_passes(self):
        result = validate_attribution(
            base_record(license={"canonical_license": "OFL-1.1", "attribution_required": True,
                                  "license_text_path": "packs/ui_materials_audio/OFL.txt"})
        )
        self.assertTrue(result["valid"])
        self.assertEqual(result["issues"], [])

    def test_flat_l04_interface_fields_pass(self):
        record = {
            "canonical_asset_id": "asset_ui_panel_01",
            "author": "Poly Haven",
            "canonical_license": "CC0",
            "attribution_required": False,
        }
        result = validate_attribution(record)
        self.assertTrue(result["valid"])
        self.assertEqual(result["issues"], [])
        self.assertEqual(result["canonical_license"], "CC0")

    def test_cc0_without_attribution_fields_passes(self):
        result = validate_attribution(
            {"canonical_asset_id": "asset_sand_01", "canonical_license": "CC0-1.0",
             "attribution_required": False}
        )
        self.assertTrue(result["valid"])
        self.assertEqual(result["issues"], [])

    def test_rights_nested_attribution_required_passes(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0",
                                      "license_url": "https://example.com/license"})
        record["rights"] = {"attribution_required": True}
        result = validate_attribution(record)
        self.assertTrue(result["valid"])

    def test_content_ids_as_asset_association_passes(self):
        record = base_record()
        del record["canonical_asset_id"]
        record["content_ids"] = ["island.saint_margaret"]
        result = validate_attribution(record)
        self.assertTrue(result["valid"])

    def test_record_is_not_mutated(self):
        record = base_record()
        snapshot = dict(record)
        validate_attribution(record)
        self.assertEqual(record, snapshot)

    def test_commercial_rights_are_never_inferred(self):
        result = validate_attribution(base_record())
        self.assertFalse(result["commercial_rights_inferred"])
        self.assertIn(NOTE_NO_COMMERCIAL_RIGHTS_INFERENCE, [n["code"] for n in result["notes"]])

    def test_aggregate_counts(self):
        summary = validate_attribution_records([base_record(), {}, base_record()])
        self.assertEqual(summary["total"], 3)
        self.assertEqual(summary["valid"], 2)
        self.assertEqual(summary["invalid"], 1)
        self.assertIn(ATTR_REQUIREMENT_UNKNOWN, summary["issue_counts"])

    def test_localize_returns_zh_en_and_code_fallback(self):
        self.assertIn("署名", localize(ATTR_AUTHOR_MISSING, "zh-CN"))
        self.assertIn("author", localize(ATTR_AUTHOR_MISSING, "en"))
        self.assertEqual(localize("NOT_A_REAL_CODE", "zh-CN"), "NOT_A_REAL_CODE")


class D18AttributionValidatorNegativeTests(unittest.TestCase):
    def test_missing_author_fails(self):
        record = base_record()
        del record["author"]
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_AUTHOR_MISSING, [i["code"] for i in result["issues"]])

    def test_placeholder_author_fails(self):
        result = validate_attribution(base_record(author="unknown"))
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_AUTHOR_PLACEHOLDER, [i["code"] for i in result["issues"]])

    def test_missing_license_reference_fails(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True})
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_LICENSE_REF_MISSING, [i["code"] for i in result["issues"]])

    def test_invalid_license_url_fails(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True,
                                      "license_url": "not a url"})
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_LICENSE_URL_INVALID, [i["code"] for i in result["issues"]])

    def test_non_http_license_url_fails(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True,
                                      "license_url": "ftp://example.com/license.txt"})
        self.assertIn(ATTR_LICENSE_URL_INVALID, [i["code"] for i in validate_attribution(record)["issues"]])

    def test_absolute_evidence_path_fails(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True,
                                      "license_evidence_path": "C:\\secrets\\LICENSE.txt"})
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_LICENSE_EVIDENCE_PATH_INVALID, [i["code"] for i in result["issues"]])

    def test_parent_traversal_evidence_path_fails(self):
        record = base_record(license={"canonical_license": "CC-BY-4.0", "attribution_required": True,
                                      "license_evidence_path": "../outside/LICENSE.txt"})
        self.assertIn(ATTR_LICENSE_EVIDENCE_PATH_INVALID,
                      [i["code"] for i in validate_attribution(record)["issues"]])

    def test_missing_credits_location_fails(self):
        record = base_record()
        del record["credits_location"]
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_CREDITS_LOCATION_MISSING, [i["code"] for i in result["issues"]])

    def test_placeholder_credits_location_fails(self):
        result = validate_attribution(base_record(credits_location="TBD"))
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_CREDITS_LOCATION_PLACEHOLDER, [i["code"] for i in result["issues"]])

    def test_missing_asset_link_fails(self):
        record = base_record()
        del record["canonical_asset_id"]
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_ASSET_LINK_MISSING, [i["code"] for i in result["issues"]])

    def test_cc0_marked_attribution_required_fails(self):
        result = validate_attribution(
            {"canonical_asset_id": "asset_rock_01", "canonical_license": "CC0-1.0",
             "attribution_required": True, "author": "Kenney"}
        )
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_CC0_FALSE_ATTRIBUTION_REQUIRED, [i["code"] for i in result["issues"]])

    def test_missing_attribution_requirement_is_conservative_block(self):
        result = validate_attribution({"canonical_asset_id": "asset_unknown_01"})
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_REQUIREMENT_UNKNOWN, [i["code"] for i in result["issues"]])

    def test_non_boolean_attribution_requirement_fails(self):
        record = base_record(attribution_required="yes")
        record["license"] = {"canonical_license": "CC-BY-4.0", "attribution_required": True}
        result = validate_attribution(record)
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_REQUIREMENT_INVALID, [i["code"] for i in result["issues"]])

    def test_non_dict_input_fails(self):
        result = validate_attribution("not-a-record")
        self.assertFalse(result["valid"])
        self.assertIn(ATTR_INPUT_TYPE_INVALID, [i["code"] for i in result["issues"]])

    def test_multiple_negative_issues_reported_together(self):
        result = validate_attribution({"attribution_required": True, "canonical_license": "CC-BY-4.0"})
        codes = [i["code"] for i in result["issues"]]
        self.assertIn(ATTR_AUTHOR_MISSING, codes)
        self.assertIn(ATTR_LICENSE_REF_MISSING, codes)
        self.assertIn(ATTR_CREDITS_LOCATION_MISSING, codes)
        self.assertIn(ATTR_ASSET_LINK_MISSING, codes)
        self.assertFalse(result["valid"])

    def test_cc_by_without_attribution_flag_warns_but_passes(self):
        record = {
            "canonical_asset_id": "asset_hut_02",
            "canonical_license": "CC-BY-4.0",
            "attribution_required": False,
        }
        result = validate_attribution(record)
        self.assertTrue(result["valid"])
        self.assertIn(WARN_BY_LICENSE_NO_ATTRIBUTION, [w["code"] for w in result["warnings"]])

    def test_missing_canonical_license_warns(self):
        record = base_record()
        record["license"] = {"attribution_required": True, "license_url": "https://example.com/license"}
        result = validate_attribution(record)
        self.assertTrue(result["valid"])
        self.assertIn(WARN_CANONICAL_LICENSE_MISSING, [w["code"] for w in result["warnings"]])


if __name__ == "__main__":
    unittest.main()
