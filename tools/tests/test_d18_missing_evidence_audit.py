import json
import shutil
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_missing_evidence_audit import (  # noqa: E402
    audit_record,
    audit_records,
    load_source_catalogs,
    main,
)


def complete_record(**overrides):
    record = {
        "canonical_asset_id": "asset_kenney_blocky_characters_v1",
        "category": "character",
        "source_id": "source_kenney",
        "source_url": "https://kenney.nl/assets/blocky-characters",
        "license": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/CharactersAnimals/Kenney/License.txt",
            "attribution_required": False,
        },
        "rights": {
            "commercial_use": True,
            "modification_allowed": True,
            "repository_storage": True,
            "distribution_with_game": True,
        },
        "download_date": "2026-08-14",
        "original_filename": "kenney_blocky_characters.zip",
        "sha256": "72CAA887BBA1B301B0BDB82B4FBC2702058C72314F3A1680D2558C6DA6E98338",
        "state": "Candidate",
        "content_ids": ["asset_kenney_blocky_character_a_v01"],
        "evidence_paths": ["game/Assets/Art/Candidates/CharactersAnimals/Kenney/License.txt"],
        "runtime_deferred": ["逐文件完整性待 L06/L31"],
    }
    record.update(overrides)
    return record


def valid_descriptor(**overrides):
    descriptor = {
        "descriptor_kind": "d18_import_descriptor",
        "schema_version": 1,
        "source_asset_id": "asset_kenney_blocky_characters_v1",
        "type": "character",
        "units": "meters",
        "axes": {"up": "y", "forward": "z"},
        "bounds": {"width_m": 1.0, "depth_m": 1.0, "height_m": 2.0},
        "material_slots": [],
        "skeleton_type": "humanoid",
        "animation_list": [{"animation_key": "idle", "role": "idle"}],
        "root_motion": "none",
        "collision_intent": "capsule_agent",
        "lod_intent": {"levels": 2, "generate_automatically": True},
        "license_dependency": {"license_id": "CC0-1.0", "license_evidence_path": "game/Assets/LICENSE.txt"},
        "fallback": {"reference": "placeholder_character", "description": "keep placeholder"},
        "represents_imported_asset": False,
    }
    descriptor.update(overrides)
    return descriptor


class D18MissingEvidenceAuditPositiveTests(unittest.TestCase):
    def test_complete_record_with_descriptor_has_no_gaps(self):
        report = audit_records([complete_record()], descriptors=[valid_descriptor()])
        self.assertEqual(report["records_with_gaps"], 0)
        self.assertEqual(report["gaps_total"], 0)

    def test_complete_record_has_only_descriptor_gap_without_registry(self):
        result = audit_record(complete_record())
        codes = [g["code"] for g in result["gaps"]]
        self.assertEqual(codes, ["GAP_DESCRIPTOR_MISSING"])

    def test_null_sha256_is_p1_pending_not_guessed(self):
        result = audit_record(complete_record(sha256=None))
        self.assertTrue(result["has_gaps"])
        codes = [g["code"] for g in result["gaps"]]
        self.assertIn("GAP_SHA256_PENDING", codes)
        self.assertNotIn("GAP_SHA256_INVALID", codes)
        self.assertEqual([g for g in result["gaps"] if g["code"] == "GAP_SHA256_PENDING"][0]["severity"], "P1")

    def test_missing_descriptor_is_p1(self):
        report = audit_records([complete_record()], descriptors=[])
        gap = [g for r in report["records"] for g in r["gaps"] if g["code"] == "GAP_DESCRIPTOR_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P1")

    def test_runtime_deferred_missing_is_p2(self):
        result = audit_record(complete_record(runtime_deferred=[]))
        gap = [g for g in result["gaps"] if g["code"] == "GAP_RUNTIME_DEFERRED_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P2")

    def test_audit_records_is_deterministic(self):
        records = [complete_record(canonical_asset_id="asset_b"), {}, complete_record()]
        first = audit_records(records)
        second = audit_records(records)
        self.assertEqual(first, second)
        ids = [r["canonical_asset_id"] for r in first["records"]]
        self.assertEqual(ids, sorted(ids))

    def test_input_is_never_mutated(self):
        record = complete_record()
        snapshot = dict(record)
        audit_record(record)
        audit_records([record])
        self.assertEqual(record, snapshot)

    def test_summary_counts(self):
        report = audit_records([complete_record(), complete_record()], descriptors=[valid_descriptor(), valid_descriptor()])
        self.assertEqual(report["total"], 2)
        self.assertEqual(report["records_with_gaps"], 0)
        self.assertEqual(report["gaps_total"], 0)

    def test_load_source_catalogs_reads_json_records(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "d18_fake_catalog.json"
            path.write_text(json.dumps({"records": [complete_record()], "extra": []}), encoding="utf-8")
            records = load_source_catalogs(tmp)
            self.assertEqual(len(records), 1)
            self.assertEqual(records[0]["canonical_asset_id"], "asset_kenney_blocky_characters_v1")
            self.assertIn("audit_source_file", records[0])

    def test_cli_main_prints_machine_readable_json(self):
        with tempfile.TemporaryDirectory() as tmp:
            (Path(tmp) / "d18_fake_catalog.json").write_text(
                json.dumps({"records": [complete_record()]}), encoding="utf-8")
            exit_code = main(["--catalog-dir", tmp])
            self.assertEqual(exit_code, 0)


class D18MissingEvidenceAuditNegativeTests(unittest.TestCase):
    def test_empty_record_reports_p0_schema_gaps(self):
        result = audit_record({})
        self.assertTrue(result["has_gaps"])
        codes = [g["code"] for g in result["gaps"]]
        for expected in ("GAP_ASSET_ID_MISSING", "GAP_CATEGORY_MISSING", "GAP_STATE_MISSING",
                         "GAP_SOURCE_ID_MISSING", "GAP_LICENSE_MISSING", "GAP_RIGHTS_MISSING",
                         "GAP_CONTENT_IDS_MISSING"):
            self.assertIn(expected, codes)
            self.assertEqual([g for g in result["gaps"] if g["code"] == expected][0]["severity"], "P0")

    def test_partial_record_only_reports_missing_pieces(self):
        record = {"canonical_asset_id": "asset_partial_v1", "source_id": "source_kenney"}
        result = audit_record(record)
        codes = [g["code"] for g in result["gaps"]]
        self.assertNotIn("GAP_SOURCE_ID_MISSING", codes)
        self.assertIn("GAP_LICENSE_MISSING", codes)
        self.assertIn("GAP_RIGHTS_MISSING", codes)
        self.assertIn("GAP_CONTENT_IDS_MISSING", codes)

    def test_missing_license_is_p0(self):
        record = complete_record(license={})
        gap = [g for g in audit_record(record)["gaps"] if g["code"] == "GAP_LICENSE_ID_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_unknown_license_with_all_fields_is_not_a_license_gap(self):
        record = complete_record(license={"license_id": "UNKNOWN-LICENSE", "license_url": "https://x.example",
                                          "license_evidence_path": "packs/x/LICENSE.txt",
                                          "attribution_required": False})
        result = audit_record(record)
        self.assertNotIn("license", [g["dimension"] for g in result["gaps"]])
        self.assertEqual([g["code"] for g in result["gaps"]], ["GAP_DESCRIPTOR_MISSING"])

    def test_cc0_with_false_attribution_is_conflict(self):
        license_obj = {"license_id": "CC0-1.0", "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
                       "license_evidence_path": "packs/x/LICENSE.txt", "attribution_required": True}
        gap = [g for g in audit_record(complete_record(license=license_obj))["gaps"]
               if g["code"] == "GAP_CC0_FALSE_ATTRIBUTION"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_permissive_license_with_false_right_is_conflict(self):
        record = complete_record(rights={"commercial_use": False, "modification_allowed": True,
                                         "repository_storage": True, "distribution_with_game": True})
        codes = [g["code"] for g in audit_record(record)["gaps"]]
        self.assertIn("GAP_RIGHT_NOT_GRANTED", codes)
        self.assertIn("GAP_RIGHTS_LICENSE_CONFLICT", codes)

    def test_missing_rights_field_is_p0(self):
        record = complete_record(rights={"commercial_use": True})
        codes = [g["code"] for g in audit_record(record)["gaps"]]
        self.assertIn("GAP_RIGHT_FIELD_MISSING", codes)

    def test_invalid_sha256_format_is_p0(self):
        gap = [g for g in audit_record(complete_record(sha256="not-a-hash"))["gaps"]
               if g["code"] == "GAP_SHA256_INVALID"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_empty_content_ids_is_p0(self):
        gap = [g for g in audit_record(complete_record(content_ids=[]))["gaps"]
               if g["code"] == "GAP_CONTENT_IDS_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_invalid_content_id_element_is_p0(self):
        gap = [g for g in audit_record(complete_record(content_ids=["Not Snake Case"]))["gaps"]
               if g["code"] == "GAP_CONTENT_ID_INVALID"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_absolute_evidence_path_is_p0(self):
        gap = [g for g in audit_record(complete_record(evidence_paths=["C:\\secret\\evidence.txt"]))["gaps"]
               if g["code"] == "GAP_EVIDENCE_PATH_INVALID"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_missing_evidence_paths_is_p1(self):
        gap = [g for g in audit_record(complete_record(evidence_paths=[]))["gaps"]
               if g["code"] == "GAP_EVIDENCE_PATHS_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P1")

    def test_runtime_approved_without_consumers_is_p0(self):
        gap = [g for g in audit_record(complete_record(state="RuntimeApproved"))["gaps"]
               if g["code"] == "GAP_STATE_APPROVED_NO_EVIDENCE"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_unknown_state_is_p0(self):
        gap = [g for g in audit_record(complete_record(state="LocalDevelopmentReady"))["gaps"]
               if g["code"] == "GAP_STATE_INVALID"]
        self.assertEqual(len(gap), 1)

    def test_descriptor_type_mismatch_is_p0_conflict(self):
        report = audit_records([complete_record()], descriptors=[valid_descriptor(type="ship")])
        gap = [g for r in report["records"] for g in r["gaps"] if g["code"] == "GAP_DESCRIPTOR_TYPE_MISMATCH"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P0")

    def test_descriptor_without_fallback_is_p2(self):
        descriptor = valid_descriptor()
        descriptor["fallback"] = {}
        report = audit_records([complete_record()], descriptors=[descriptor])
        gap = [g for r in report["records"] for g in r["gaps"] if g["code"] == "GAP_DESCRIPTOR_FALLBACK_MISSING"]
        self.assertEqual(len(gap), 1)
        self.assertEqual(gap[0]["severity"], "P2")

    def test_non_dict_record_is_p0(self):
        result = audit_record("not-a-record")
        self.assertTrue(result["has_gaps"])
        self.assertEqual(result["gaps"][0]["code"], "GAP_RECORD_NOT_OBJECT")

    def test_non_distributable_next_step_mentions_waiver_boundary(self):
        record = complete_record(rights={"commercial_use": False, "modification_allowed": True,
                                         "repository_storage": True, "distribution_with_game": True})
        gap = [g for g in audit_record(record)["gaps"] if g["code"] == "GAP_RIGHT_NOT_GRANTED"][0]
        self.assertIn("waiver", gap["next_step"])
        self.assertIn("L19", gap["next_step"])

    def test_by_dimension_and_by_code_are_stable(self):
        records = [
            complete_record(),
            {},
            complete_record(license={"license_id": "CC0-1.0"}),
            complete_record(license={"license_id": "CC0-1.0", "license_url": "https://x.example",
                                     "license_evidence_path": "packs/x/LICENSE.txt", "attribution_required": False}),
        ]
        report = audit_records(records, descriptors=[valid_descriptor()])
        self.assertIn("by_dimension", report)
        self.assertIn("by_code", report)
        self.assertIn("source", report["by_dimension"])
        self.assertEqual(report["by_dimension"]["license"], 4)
        self.assertEqual(report["by_dimension"]["content_id"], 1)


if __name__ == "__main__":
    unittest.main()
