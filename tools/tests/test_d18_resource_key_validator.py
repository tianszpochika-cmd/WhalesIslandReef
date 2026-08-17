import json
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_resource_key_validator import (  # noqa: E402
    CATEGORY_KEYS,
    PREFIX_CATEGORY_MAP,
    validate_key_format,
    validate_records,
)

# Historical D20 resource keys (D20-A01/A03/A05/A06 machine results,
# read-only data source; historical evidence, not a gate).
REAL_D20_KEYS = [
    "asset_d20_player_hair_black_v01",
    "asset_d20_player_hair_brown_v01",
    "asset_d20_player_humanoid_avatar_female_r01",
    "asset_d20_player_skin_neutral_v01",
    "asset_d20_player_skin_warm_v01",
    "asset_kenney_blocky_character_a_v01",
    "candidate_superhero_female_fullbody_r03p",
    "npc_d20_boatbuilder_01",
    "npc_d20_common_resident_01",
    "npc_d20_deckhand_01",
    "npc_d20_dock_clerk_01",
    "npc_d20_fisher_01",
    "npc_d20_harbor_watch_proxy",
    "npc_d20_net_mender_01",
    "npc_d20_rigger_01",
    "npc_d20_sailmaker_01",
    "npc_d20_shore_trader_01",
    "npc_d20_shore_trader_02",
    "npc_d20_tavern_cook_01",
    "pet_animal_quaternius_cat_pack_vol2_v01_visual",
    "pet_animal_quaternius_shiba_inu_v01_visual",
    "wildlife_animal_quaternius_cow_v01_visual",
    "wildlife_animal_quaternius_eagle_v01_visual",
    "wildlife_animal_quaternius_fox_v01_visual",
    "wildlife_animal_quaternius_manta_ray_v01_visual",
    "wildlife_animal_quaternius_shark_v01_visual",
    "wildlife_animal_quaternius_whale_v01_visual",
    "wildlife_fish_quaternius_fish_01_v01_visual",
    "wildlife_fish_quaternius_fish_02_v01_visual",
]


def record(key, **kwargs):
    out = {"key": key}
    out.update(kwargs)
    return out


def kinds(report):
    return set(report["issue_kind_counts"])


class FormatTests(unittest.TestCase):
    def test_valid_bare_keys_have_no_issues(self):
        for key in ("npc_d20_deckhand_01", "wildlife_animal_quaternius_eagle_v01_visual",
                    "asset_kenney_blocky_character_a_v01", "pet_animal_cat_v01_visual"):
            self.assertEqual(validate_key_format(key), [], msg=key)

    def test_valid_namespaced_key(self):
        issues = validate_key_format("d10:appearance:outfit_hair_short_brown")
        self.assertEqual(issues, [])

    def test_disk_paths_are_rejected(self):
        for key in ("Assets/Art/Candidates/foo.fbx", "C:/foo/bar", "C:\\foo\\bar",
                    "game/Assets/Art/Models/foo", "../outside/key", ".\\key"):
            issues = validate_key_format(key)
            self.assertTrue(any(i["kind"] == "disk_path" for i in issues), msg=key)

    def test_file_extensions_are_rejected(self):
        for key in ("foo.fbx", "foo.png", "foo.json", "foo.prefab", "foo.meta"):
            issues = validate_key_format(key)
            self.assertTrue(any(i["kind"] == "file_extension_or_path" for i in issues), msg=key)

    def test_display_names_are_rejected(self):
        for key in ("Short Brown Hair", "短发", "Hair Brown", "content.animal.animal_green_turtle.name"):
            issues = validate_key_format(key)
            self.assertTrue(
                any(i["kind"] in ("display_name", "display_name_key") for i in issues), msg=key
            )

    def test_empty_keys_are_rejected(self):
        self.assertTrue(any(i["kind"] == "empty_key" for i in validate_key_format("")))
        self.assertTrue(any(i["kind"] == "empty_key" for i in validate_key_format("   ")))
        self.assertTrue(any(i["kind"] == "empty_key" for i in validate_key_format(None)))

    def test_unity_guid_is_rejected(self):
        issues = validate_key_format("a1b2c3d4e5f60718293a4b5c6d7e8f9a")
        self.assertTrue(any(i["kind"] == "unity_guid" for i in issues))

    def test_double_underscore_and_invalid_characters_are_rejected(self):
        for key in ("npc__deckhand_01", "npc_deckhand_01_", "npc-deckhand-01", "npc deckhand"):
            issues = validate_key_format(key)
            self.assertTrue(any(i["kind"] == "invalid_format" or i["kind"] == "display_name" for i in issues), msg=key)

    def test_candidate_prefix_is_rejected_as_active_key(self):
        issues = validate_key_format("candidate_superhero_female_fullbody_r03p")
        self.assertEqual(issues, [])
        report = validate_records([record("candidate_superhero_female_fullbody_r03p", category="character")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("candidate_prefix_active", kinds(report))


class ValidRecordTests(unittest.TestCase):
    def test_allowed_record_set_is_cleared(self):
        records = [
            record("npc_d20_deckhand_01", category="character", source="D20-A03"),
            record("wildlife_animal_quaternius_eagle_v01_visual", category="animal", source="D20-A05"),
            record("asset_kenney_blocky_character_a_v01", category="character", source="D20-A01"),
            record("pet_animal_cat_v01_visual", category="animal", source="D20-A06"),
            record("d10:appearance:outfit_hair_short_brown", category="character",
                   content_id="outfit_hair_short_brown", source="D10"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["issue_kind_counts"], {})
        self.assertTrue(all(r["valid"] for r in report["records"]))

    def test_report_declares_independence_and_read_only(self):
        report = validate_records([record("npc_a", category="character")])
        self.assertTrue(report["key_independent_of_binary"])
        self.assertTrue(report["registry_unchanged"])
        self.assertTrue(report["runtime_references_unchanged"])
        self.assertTrue(report["read_only"])


class RejectionTests(unittest.TestCase):
    def test_duplicate_key_is_blocked(self):
        records = [record("npc_a", category="character"), record("npc_a", category="character")]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("duplicate_key", kinds(report))

    def test_cross_category_collision_is_blocked(self):
        records = [record("npc_a", category="character"), record("npc_a", category="animal")]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("cross_category_collision", kinds(report))

    def test_unknown_category_is_blocked(self):
        report = validate_records([record("npc_a", category="shoes")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("unknown_category", kinds(report))

    def test_prefix_category_mismatch_is_blocked(self):
        report = validate_records([record("pet_animal_cat_v01", category="vegetation")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("category_prefix_mismatch", kinds(report))

    def test_unknown_prefix_is_blocked(self):
        report = validate_records([record("spaceship_x1", category="ship")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("unknown_prefix", kinds(report))

    def test_disk_path_key_is_blocked(self):
        report = validate_records([record("Assets/Art/Candidates/foo.fbx", category="character")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("disk_path", kinds(report))

    def test_display_name_as_key_is_blocked(self):
        report = validate_records([record("npc_deckhand_01", category="character", display_name="npc_deckhand_01")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("display_name_as_key", kinds(report))

    def test_content_id_mismatch_in_namespaced_key_is_blocked(self):
        report = validate_records(
            [record("d10:appearance:outfit_hair_short_brown", category="character", content_id="outfit_hair_other")]
        )
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("content_id_mismatch", kinds(report))


class AliasTests(unittest.TestCase):
    def test_valid_deprecated_alias_is_cleared(self):
        records = [
            record("npc_d20_deckhand_01", category="character"),
            record("npc_deckhand_v1", category="character", deprecated_alias_of="npc_d20_deckhand_01"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "CLEARED")

    def test_alias_target_missing_is_blocked(self):
        report = validate_records([record("npc_a", category="character", deprecated_alias_of="npc_missing")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("deprecated_alias_of_target_missing", kinds(report))

    def test_alias_self_reference_is_blocked(self):
        report = validate_records([record("npc_a", category="character", deprecated_alias_of="npc_a")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("deprecated_alias_of_self_reference", kinds(report))

    def test_alias_cycle_is_blocked(self):
        records = [
            record("npc_a", category="character", deprecated_alias_of="npc_b"),
            record("npc_b", category="character", deprecated_alias_of="npc_a"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("alias_cycle", kinds(report))

    def test_alias_cross_category_is_blocked(self):
        records = [
            record("npc_a", category="character"),
            record("npc_old", category="animal", deprecated_alias_of="npc_a"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("deprecated_alias_of_category_mismatch", kinds(report))

    def test_candidate_prefix_key_allowed_only_as_deprecated_alias(self):
        records = [
            record("asset_d20_player_humanoid_avatar_female_r01", category="character"),
            record("candidate_superhero_female_fullbody_r03p", category="character",
                   deprecated_alias_of="asset_d20_player_humanoid_avatar_female_r01", source="D20-A01 historical"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "CLEARED")


class MigrationTests(unittest.TestCase):
    def test_valid_migration_is_cleared(self):
        records = [
            record("npc_d20_deckhand_01", category="character"),
            record("npc_d20_rigger_01", category="character", migrated_from="npc_rigger_old"),
            record("npc_rigger_old", category="character"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "CLEARED")

    def test_migration_source_missing_is_blocked(self):
        report = validate_records([record("npc_new", category="character", migrated_from="npc_gone")])
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("migrated_from_target_missing", kinds(report))

    def test_migration_cycle_is_blocked(self):
        records = [
            record("npc_a", category="character", migrated_from="npc_b"),
            record("npc_b", category="character", migrated_from="npc_a"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("migration_cycle", kinds(report))

    def test_migration_cross_category_is_blocked(self):
        records = [
            record("npc_a", category="character"),
            record("npc_b", category="ship", migrated_from="npc_a"),
        ]
        report = validate_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("migrated_from_category_mismatch", kinds(report))


class RealDataSmokeTests(unittest.TestCase):
    def test_historical_d20_keys_are_deterministically_classified(self):
        records = []
        for key in REAL_D20_KEYS:
            if key.startswith("npc_"):
                category = "character"
            elif key.startswith("wildlife_"):
                category = "animal"
            elif key.startswith("pet_"):
                category = "animal"
            else:
                category = "character"
            records.append(record(key, category=category, source="D20 historical evidence"))
        report = validate_records(records)
        self.assertEqual(report["record_count"], 29)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertEqual(report["issue_kind_counts"], {"candidate_prefix_active": 1})
        invalid = [r for r in report["records"] if not r["valid"]]
        self.assertEqual(len(invalid), 1)
        self.assertEqual(invalid[0]["key"], "candidate_superhero_female_fullbody_r03p")

    def test_historical_keys_become_cleared_when_candidate_is_marked_deprecated(self):
        records = []
        for key in REAL_D20_KEYS:
            if key.startswith("npc_"):
                category = "character"
            elif key.startswith("wildlife_"):
                category = "animal"
            elif key.startswith("pet_"):
                category = "animal"
            else:
                category = "character"
            out = {"key": key, "category": category}
            if key.startswith("candidate_"):
                out["deprecated_alias_of"] = "asset_d20_player_humanoid_avatar_female_r01"
            records.append(out)
        report = validate_records(records)
        self.assertEqual(report["verdict"], "CLEARED")


class ToolIntegrityTests(unittest.TestCase):
    def test_frozen_enum_consistency(self):
        self.assertIn("animal", CATEGORY_KEYS)
        self.assertIn("ship", CATEGORY_KEYS)
        self.assertIn("ui", CATEGORY_KEYS)
        self.assertIn("asset_*", PREFIX_CATEGORY_MAP)
        self.assertEqual(set(PREFIX_CATEGORY_MAP["asset_*"]), set(CATEGORY_KEYS))

    def test_module_has_no_network_imports(self):
        source = ROOT.joinpath("tools/d18_resource_key_validator.py").read_text(encoding="utf-8")
        for forbidden in ("import urllib", "import requests", "import socket", "import http"):
            self.assertNotIn(forbidden, source)


if __name__ == "__main__":
    unittest.main()
