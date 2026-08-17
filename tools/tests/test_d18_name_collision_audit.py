import json
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_name_collision_audit import audit_records  # noqa: E402


def record(record_id, **kwargs):
    out = {"record_id": record_id}
    out.update(kwargs)
    return out


def kinds(report):
    return {c["kind"] for c in report["conflicts"]}


class CleanSetTests(unittest.TestCase):
    def test_clean_records_are_cleared(self):
        records = [
            record("rec_1", canonical_id="asset_kenney_blocky_character_a_v01", category="character",
                   resource_keys=["d10:appearance:asset_kenney_blocky_character_a_v01"],
                   display_names=["Blocky Character A"], original_filenames=["blocky_a.fbx"]),
            record("rec_2", canonical_id="npc_d20_deckhand_01", category="character",
                   resource_keys=["npc_d20_deckhand_01"],
                   display_names=["Deckhand"], original_filenames=["deckhand.fbx"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["conflict_count"], 0)

    def test_canonical_id_equal_to_own_resource_key_is_legal(self):
        records = [
            record("rec_1", canonical_id="asset_x_v01", resource_keys=["asset_x_v01"]),
            record("rec_2", canonical_id="asset_y_v01", resource_keys=["asset_y_v01"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")


class CaseSensitivityTests(unittest.TestCase):
    def test_case_only_variants_are_p0_conflicts(self):
        records = [
            record("rec_1", canonical_id="asset_blocky_a_v01"),
            record("rec_2", canonical_id="Asset_Blocky_A_V01"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("case_collision", kinds(report))
        self.assertEqual(report["severity_counts"]["P0"], 1)

    def test_case_variant_of_resource_key_is_p0(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01", resource_keys=["asset_a_v01"]),
            record("rec_2", canonical_id="asset_b_v01", resource_keys=["ASSET_A_V01"]),
        ]
        report = audit_records(records)
        self.assertIn("case_collision", kinds(report))

    def test_same_display_name_different_case_is_p2_not_p0(self):
        records = [
            record("rec_1", display_names=["Harbor Bell"], category="prop"),
            record("rec_2", display_names=["harbor bell"], category="prop"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["severity_counts"]["P2"], 1)


class AliasTests(unittest.TestCase):
    def test_valid_alias_chain_is_cleared(self):
        records = [
            record("rec_1", canonical_id="asset_new_v01"),
            record("rec_2", canonical_id="asset_old_v01", deprecated_alias_of="rec_1"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")

    def test_alias_cycle_is_p0(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01", deprecated_alias_of="rec_2"),
            record("rec_2", canonical_id="asset_b_v01", deprecated_alias_of="rec_1"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("alias_cycle", kinds(report))

    def test_three_node_alias_cycle_is_p0(self):
        records = [
            record("rec_1", deprecated_alias_of="rec_2"),
            record("rec_2", deprecated_alias_of="rec_3"),
            record("rec_3", deprecated_alias_of="rec_1"),
        ]
        report = audit_records(records)
        self.assertIn("alias_cycle", kinds(report))

    def test_alias_self_reference_is_p0(self):
        records = [record("rec_1", canonical_id="asset_a_v01", deprecated_alias_of="rec_1")]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("alias_self_reference", kinds(report))

    def test_alias_target_not_in_set_is_p1(self):
        records = [record("rec_1", canonical_id="asset_a_v01", deprecated_alias_of="rec_missing")]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertIn("alias_target_not_in_set", kinds(report))
        self.assertEqual(report["severity_counts"]["P1"], 1)


class DuplicateAndCrossFieldTests(unittest.TestCase):
    def test_duplicate_canonical_id_is_p0(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01"),
            record("rec_2", canonical_id="asset_a_v01"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("duplicate_canonical_id", kinds(report))

    def test_canonical_id_collides_with_other_resource_key_is_p0(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01"),
            record("rec_2", canonical_id="asset_b_v01", resource_keys=["asset_a_v01"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("cross_field_collision", kinds(report))

    def test_content_id_collides_with_other_canonical_id_is_p0(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01"),
            record("rec_2", canonical_id="asset_b_v01", content_ids=["asset_a_v01"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("cross_field_collision", kinds(report))

    def test_id_like_value_reused_as_other_display_name_is_p1(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01"),
            record("rec_2", canonical_id="asset_b_v01", display_names=["asset_a_v01"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["severity_counts"]["P1"], 1)


class DisplayNameTests(unittest.TestCase):
    def test_same_display_name_across_categories_is_legal(self):
        records = [
            record("rec_1", display_names=["Harbor Bell"], category="prop"),
            record("rec_2", display_names=["Harbor Bell"], category="building"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["conflict_count"], 0)

    def test_same_display_name_same_category_is_p2(self):
        records = [
            record("rec_1", display_names=["Harbor Bell"], category="prop"),
            record("rec_2", display_names=["Harbor Bell"], category="prop"),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertIn("same_display_name_same_category", kinds(report))
        self.assertEqual(report["severity_counts"]["P2"], 1)

    def test_multiple_records_sharing_display_name_is_p2(self):
        records = [
            record("rec_1", display_names=["Bell"], category="prop"),
            record("rec_2", display_names=["Bell"], category="prop"),
            record("rec_3", display_names=["Bell"], category="prop"),
        ]
        report = audit_records(records)
        self.assertEqual(report["severity_counts"]["P2"], 1)


class FilenameTests(unittest.TestCase):
    def test_same_basename_in_different_paths_is_legal(self):
        records = [
            record("rec_1", original_paths=["game/Assets/Art/Candidates/Kenney/PirateKit/Models/rock.fbx"],
                   original_filenames=["rock.fbx"]),
            record("rec_2", original_paths=["game/Assets/Art/Candidates/Quaternius/Nature/rock.fbx"],
                   original_filenames=["rock.fbx"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertEqual(report["conflict_count"], 0)

    def test_same_full_path_in_two_records_is_p0(self):
        records = [
            record("rec_1", original_paths=["game/Assets/Art/Candidates/Kenney/PirateKit/Models/rock.fbx"]),
            record("rec_2", original_paths=["game/Assets/Art/Candidates/Kenney/PirateKit/Models/rock.fbx"]),
        ]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "BLOCKED")
        self.assertIn("same_path_duplicate", kinds(report))

    def test_backslash_path_normalized_to_forward_slashes(self):
        records = [
            record("rec_1", original_paths=["game\\Assets\\Art\\Candidates\\Kenney\\Models\\rock.fbx"]),
            record("rec_2", original_paths=["game/Assets/Art/Candidates/Kenney/Models/rock.fbx"]),
        ]
        report = audit_records(records)
        self.assertIn("same_path_duplicate", kinds(report))

    def test_id_equals_original_filename_is_p1(self):
        records = [record("rec_1", canonical_id="rock.fbx", original_filenames=["rock.fbx"])]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertIn("id_equals_filename", kinds(report))


class HygieneTests(unittest.TestCase):
    def test_id_equals_own_display_name_is_p1(self):
        records = [record("rec_1", canonical_id="asset_a_v01", display_names=["asset_a_v01"])]
        report = audit_records(records)
        self.assertEqual(report["verdict"], "CLEARED")
        self.assertIn("id_equals_display_name", kinds(report))

    def test_key_equals_display_name_is_p1(self):
        records = [record("rec_1", canonical_id="asset_a_v01", resource_keys=["npc_d20_rigger_01"],
                         display_names=["npc_d20_rigger_01"])]
        report = audit_records(records)
        self.assertIn("key_equals_display_name", kinds(report))

    def test_intra_record_id_and_key_equal_is_legal_with_other_findings(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01", resource_keys=["asset_a_v01"],
                   display_names=["asset_a_v01"]),
        ]
        report = audit_records(records)
        self.assertNotIn("cross_field_collision", kinds(report))
        self.assertIn("id_equals_display_name", kinds(report))


class DeterminismAndReadOnlyTests(unittest.TestCase):
    def test_report_is_deterministic(self):
        records = [
            record("rec_1", canonical_id="asset_b_v01", display_names=["Bell"], category="prop"),
            record("rec_2", canonical_id="asset_a_v01", display_names=["Bell"], category="prop"),
            record("rec_3", canonical_id="asset_a_v01", deprecated_alias_of="rec_1"),
        ]
        first = audit_records(records)
        second = audit_records(list(reversed(records)))
        self.assertEqual(first, second)
        self.assertTrue(first["deterministic"])

    def test_report_declares_read_only_and_no_rename(self):
        report = audit_records([record("rec_1", canonical_id="asset_a_v01")])
        self.assertTrue(report["read_only"])
        self.assertTrue(report["registry_unchanged"])
        self.assertTrue(report["no_auto_rename"])

    def test_conflicts_include_fields_pair_severity_and_action(self):
        records = [
            record("rec_1", canonical_id="asset_a_v01"),
            record("rec_2", canonical_id="asset_b_v01", content_ids=["asset_a_v01"]),
        ]
        report = audit_records(records)
        conflict = report["conflicts"][0]
        self.assertEqual(conflict["pair"], ["rec_1", "rec_2"])
        self.assertEqual(sorted(conflict["fields"]), ["canonical_id", "content_id"])
        self.assertEqual(conflict["severity"], "P0")
        self.assertTrue(conflict["action"])

    def test_module_has_no_network_imports(self):
        source = ROOT.joinpath("tools/d18_name_collision_audit.py").read_text(encoding="utf-8")
        for forbidden in ("import urllib", "import requests", "import socket", "import http"):
            self.assertNotIn(forbidden, source)


if __name__ == "__main__":
    unittest.main()
