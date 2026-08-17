"""Tests for tools/d18_content_id_asset_link_validator.py (D18-L15)."""

import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_content_id_asset_link_validator import (  # noqa: E402
    validate_records,
    load_records,
    load_catalog,
    load_prefix_map,
    load_deprecated,
    load_candidates,
    DEFAULT_PREFIX_TYPES,
)

D8_CATALOG = [
    {"id": "outfit_hair_short_brown", "category": "outfit"},
    {"id": "asset_ph_crew_male", "category": "asset"},
    {"id": "ship_moonwake", "category": "ship"},
    {"id": "crew_isaac_reed", "category": "crew"},
    {"id": "item_dye_indigo", "category": "item"},
]

INTAKE_CATALOG = [
    {"content_id": "crew_quaternius_male_long_sleeve_v01", "domain": "character"},
    {"content_id": "asset_kenney_blocky_character_a_v01", "domain": "character"},
    {"content_id": "candidate_hair_buzzed_r03p", "domain": "character"},
]


def catalog() -> dict:
    return load_catalog(_write_json("catalog.json", D8_CATALOG))[0]


def _write_json(name: str, data) -> Path:
    base = Path(tempfile.gettempdir()) / "d18_l15_fixtures"
    base.mkdir(parents=True, exist_ok=True)
    path = base / name
    path.write_text(json.dumps(data), encoding="utf-8")
    return path


class D18ContentIdLinkValidatorTests(unittest.TestCase):
    def setUp(self):
        self.catalog = catalog()

    def validate(self, records, **kwargs):
        catalogs = kwargs.pop("catalogs", [self.catalog])
        return validate_records(records, catalogs=catalogs, **kwargs)

    def test_valid_record_passes(self):
        result = self.validate([{
            "canonical_asset_id": "asset_a",
            "content_ids": ["outfit_hair_short_brown", "asset_ph_crew_male"],
        }])
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["verified_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertEqual(result["records"][0]["status"], "OK")

    def test_missing_content_ids_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_content_ids", result["issue_counts"])

    def test_empty_content_ids_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": []}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("empty_content_ids", result["issue_counts"])

    def test_non_list_content_ids_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": "outfit_hair_short_brown"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_content_ids", result["issue_counts"])

    def test_non_string_content_id_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": [42]}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_content_id", result["issue_counts"])

    def test_path_like_content_ids_are_blocked(self):
        for bad in ("Assets/Art/Candidates/CharactersAnimals/a.fbx", "Kenney Nature Kit", "C:/Windows/x", "outfit.hair.short"):
            result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": [bad]}])
            self.assertEqual(result["status"], "BLOCKED", bad)
            self.assertIn("path_like_content_id", result["issue_counts"], bad)
            self.assertEqual(result["records"][0]["content_ids"][0]["code"], "path_like_content_id", bad)

    def test_candidate_key_leak_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": ["candidate_hair_buzzed_r03p"]}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("candidate_key_leak", result["issue_counts"])

    def test_candidate_path_leak_via_candidates_list_is_blocked(self):
        candidates = {"asset_leaked_display_name", "game/Assets/Art/Candidates/CharactersAnimals/foo.fbx"}
        result = self.validate(
            [{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown", "asset_leaked_display_name"]}],
            candidates=candidates,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("candidate_path_leak", result["issue_counts"])
        self.assertEqual(result["records"][0]["content_ids"][1]["code"], "candidate_path_leak")

    def test_duplicate_content_id_inside_record_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown", "outfit_hair_short_brown"]}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("duplicate_content_id", result["issue_counts"])
        self.assertEqual(result["records"][0]["content_ids"][1]["code"], "duplicate_content_id")

    def test_duplicate_content_id_across_records_is_blocked(self):
        records = [
            {"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown"]},
            {"canonical_asset_id": "asset_b", "content_ids": ["outfit_hair_short_brown"]},
        ]
        result = self.validate(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("duplicate_content_id_across_records", result["issue_counts"])
        self.assertEqual(result["records"][1]["content_ids"][0]["code"], "duplicate_content_id_across_records")

    def test_missing_content_id_is_blocked(self):
        result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_missing"]}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_content_id", result["issue_counts"])

    def test_unknown_prefix_is_blocked(self):
        for bad in ("d18a05_kenney_kit", "body_peasant_v1", "skin_tan"):
            result = self.validate([{"canonical_asset_id": "asset_a", "content_ids": [bad]}])
            self.assertEqual(result["status"], "BLOCKED", bad)
            self.assertIn("unknown_prefix", result["issue_counts"], bad)

    def test_type_mismatch_is_blocked(self):
        catalog_with_mismatch = load_catalog(_write_json("catalog_mismatch.json", [
            {"id": "outfit_weird_layer", "category": "ship"},
        ]))[0]
        result = validate_records(
            [{"canonical_asset_id": "asset_a", "content_ids": ["outfit_weird_layer"]}],
            catalogs=[catalog_with_mismatch],
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("type_mismatch", result["issue_counts"])
        self.assertEqual(result["records"][0]["content_ids"][0]["code"], "type_mismatch")

    def test_deprecated_content_id_is_blocked_with_replacement(self):
        deprecated = {"outfit_old_hat": "outfit_hat_straw_deck", "outfit_gone": None}
        result = self.validate(
            [{"canonical_asset_id": "asset_a", "content_ids": ["outfit_old_hat", "outfit_gone"]}],
            deprecated=deprecated,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("deprecated_content_id", result["issue_counts"])
        self.assertEqual(result["records"][0]["content_ids"][0]["replacement"], "outfit_hat_straw_deck")
        self.assertIsNone(result["records"][0]["content_ids"][1]["replacement"])

    def test_no_catalog_is_pending_and_never_approved(self):
        result = validate_records([{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown"]}], catalogs=[])
        self.assertEqual(result["status"], "PENDING")
        self.assertEqual(result["pending_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertIn("catalog_missing", result["issue_counts"])
        self.assertEqual(result["records"][0]["content_ids"][0]["status"], "PENDING")

    def test_intake_style_catalog_has_no_false_type_mismatch(self):
        intake = load_catalog(_write_json("catalog_intake.json", INTAKE_CATALOG))[0]
        result = validate_records(
            [{"canonical_asset_id": "asset_a", "content_ids": ["crew_quaternius_male_long_sleeve_v01", "asset_kenney_blocky_character_a_v01"]}],
            catalogs=[intake],
        )
        self.assertEqual(result["status"], "PASS")

    def test_registry_override_changes_prefix_vocabulary(self):
        prefix_map = load_prefix_map(_write_json("registry_custom.json", {"prefix_definitions": [
            {"prefix": "foo_", "entity_type": "foo"},
        ]}))[0]
        result = self.validate(
            [{"canonical_asset_id": "asset_a", "content_ids": ["foo_bar"]}],
            prefix_types=prefix_map,
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_content_id", result["issue_counts"])

    def test_invalid_record_is_blocked(self):
        result = self.validate(["not-an-object"])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_record", result["issue_counts"])

    def test_empty_records_is_blocked(self):
        result = self.validate([])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("no_records", result["issue_counts"])

    def test_validator_is_read_only_against_catalogs(self):
        catalog_path = _write_json("catalog_ro.json", D8_CATALOG)
        before = catalog_path.read_bytes()
        loaded, code, message = load_catalog(catalog_path)
        self.assertEqual(code, "")
        self.assertEqual(loaded["outfit_hair_short_brown"]["categories"], {"outfit"})
        result = validate_records(
            [{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown"]}],
            catalogs=[loaded],
        )
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(catalog_path.read_bytes(), before)

    def test_load_records_supports_list_and_dict(self):
        list_file = _write_json("records_list.json", [{"canonical_asset_id": "asset_a", "content_ids": ["outfit_hair_short_brown"]}])
        records, code, message = load_records(list_file)
        self.assertEqual(code, "")
        self.assertEqual(records[0]["canonical_asset_id"], "asset_a")

        dict_file = _write_json("records_dict.json", {"records": [{"canonical_asset_id": "asset_b"}]})
        records, code, message = load_records(dict_file)
        self.assertEqual(code, "")
        self.assertEqual(records[0]["canonical_asset_id"], "asset_b")

    def test_load_records_rejects_missing_and_broken_files(self):
        records, code, message = load_records(Path(tempfile.gettempdir()) / "d18_l15_fixtures" / "missing_records.json")
        self.assertEqual(code, "file_not_found")
        broken = Path(tempfile.gettempdir()) / "d18_l15_fixtures" / "records_broken_raw.json"
        broken.write_bytes(b"{not json")
        records, code, message = load_records(broken)
        self.assertEqual(code, "file_json_error")
        self.assertIsNone(records)

    def test_load_catalog_rejects_unsupported_shape(self):
        bad = _write_json("catalog_bad.json", {"packages": []})
        index, code, message = load_catalog(bad)
        self.assertEqual(code, "catalog_not_supported")
        self.assertIsNone(index)

    def test_load_deprecated_and_candidates(self):
        deprecated, code, message = load_deprecated(_write_json("deprecated.json", {"old_a": "new_b"}))
        self.assertEqual(code, "")
        self.assertEqual(deprecated["old_a"], "new_b")
        deprecated_list, code, message = load_deprecated(_write_json("deprecated_list.json", ["old_x"]))
        self.assertEqual(code, "")
        self.assertEqual(deprecated_list["old_x"], None)
        candidates, code, message = load_candidates(_write_json("candidates.json", ["a/b.fbx", "Display Name"]))
        self.assertEqual(code, "")
        self.assertEqual(candidates, {"a/b.fbx", "Display Name"})

    def test_default_prefix_vocabulary_is_d8_frozen(self):
        self.assertEqual(set(DEFAULT_PREFIX_TYPES), {
            "ship_", "crew_", "island_", "building_", "item_", "animal_",
            "plant_", "fish_", "quest_", "reward_", "outfit_", "pet_", "asset_",
        })


if __name__ == "__main__":
    unittest.main()
