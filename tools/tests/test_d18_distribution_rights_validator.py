import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_distribution_rights_validator import (  # noqa: E402
    compare_records,
    discover_manifest_paths,
    normalize_bool_right,
    normalize_distribution_right,
    record_from_intake_entry,
    record_from_manifest,
    scan_intake_catalog,
    scan_manifests,
    validate_record,
)

ALLOWED_RECORD = {
    "record_id": "pack_test_all_allowed_v1",
    "source": "manifest",
    "license_id": "CC0-1.0",
    "evidence_path": "evidence/LICENSE.txt",
    "commercial_use": True,
    "modification_allowed": True,
    "source_redistribution": "allowed",
    "build_distribution": "allowed",
}


class EvidenceFixture(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name)
        evidence_dir = self.root / "evidence"
        evidence_dir.mkdir()
        (evidence_dir / "LICENSE.txt").write_text("CC0-1.0", encoding="utf-8")

    def tearDown(self):
        self.tmp.cleanup()


class AllAllowedTests(EvidenceFixture):
    def test_all_allowed_with_evidence_is_cleared(self):
        result = validate_record(ALLOWED_RECORD, self.root)
        self.assertEqual(result["verdict"], "CLEARED")
        self.assertTrue(result["migration_allowed"])
        self.assertEqual(result["blockers"], [])
        for right in ("commercial_use", "modification_allowed", "source_redistribution", "build_distribution"):
            self.assertEqual(result["rights"][right]["value"], "allowed")
            self.assertTrue(result["rights"][right]["explicit"])
        self.assertTrue(result["evidence"]["present"])

    def test_cleared_record_is_plain_data_and_changes_no_state(self):
        result = validate_record(ALLOWED_RECORD, self.root)
        self.assertNotIn("status", result)
        self.assertNotIn("approval", result)
        self.assertNotIn("RuntimeApproved", result)
        self.assertNotIn("ReleaseApproved", result)


class PersonalOnlyTests(EvidenceFixture):
    def test_personal_only_record_is_blocked_with_denied(self):
        record = dict(ALLOWED_RECORD, record_id="pack_personal_only_v1")
        record.update(
            {
                "commercial_use": False,
                "modification_allowed": False,
                "source_redistribution": "not_allowed",
                "build_distribution": "not_allowed",
            }
        )
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        self.assertFalse(result["migration_allowed"])
        kinds = [b["kind"] for b in result["blockers"]]
        self.assertEqual(kinds, ["denied", "denied", "denied", "denied"])

    def test_personal_use_text_claim_denies_commercial_even_when_field_missing(self):
        record = dict(ALLOWED_RECORD, record_id="pack_personal_claim_v1")
        record.pop("commercial_use")
        record["notes"] = "Free assets for personal use only."
        result = validate_record(record, self.root)
        self.assertEqual(result["rights"]["commercial_use"]["value"], "denied")
        self.assertEqual(result["verdict"], "BLOCKED")
        self.assertIn("personal", result["non_commercial_markers"][0])


class NCLicenseTests(EvidenceFixture):
    def test_nc_license_with_denied_commercial_is_blocked_as_denied(self):
        record = dict(ALLOWED_RECORD, record_id="pack_nc_denied_v1", license_id="CC-BY-NC-4.0")
        record["commercial_use"] = False
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        commercial_blockers = [b for b in result["blockers"] if b["right"] == "commercial_use"]
        self.assertEqual(commercial_blockers[0]["kind"], "denied")

    def test_nc_license_with_allowed_commercial_is_conflict(self):
        record = dict(ALLOWED_RECORD, record_id="pack_nc_conflict_v1", license_id="CC-BY-NC-4.0")
        record["commercial_use"] = True
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        self.assertFalse(result["migration_allowed"])
        commercial_blockers = [b for b in result["blockers"] if b["right"] == "commercial_use"]
        self.assertEqual(commercial_blockers[0]["kind"], "conflict")
        self.assertIn("CC-BY-NC", commercial_blockers[0]["detail"])

    def test_nc_license_with_missing_commercial_field_is_denied_not_guessed(self):
        record = dict(ALLOWED_RECORD, record_id="pack_nc_unknown_v1", license_id="CC-BY-NC-4.0")
        record.pop("commercial_use")
        result = validate_record(record, self.root)
        self.assertEqual(result["rights"]["commercial_use"]["value"], "denied")
        self.assertFalse(result["rights"]["commercial_use"]["explicit"])

    def test_cc0_is_not_misread_as_nc(self):
        result = validate_record(ALLOWED_RECORD, self.root)
        self.assertEqual(result["non_commercial_markers"], [])


class UnknownValueTests(EvidenceFixture):
    def test_missing_field_is_unknown_and_blocks(self):
        record = dict(ALLOWED_RECORD, record_id="pack_missing_field_v1")
        record.pop("modification_allowed")
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        self.assertFalse(result["migration_allowed"])
        blockers = [b for b in result["blockers"] if b["right"] == "modification_allowed"]
        self.assertEqual(blockers[0]["kind"], "unknown")

    def test_string_boolean_is_unknown_not_allowed(self):
        record = dict(ALLOWED_RECORD, record_id="pack_string_bool_v1")
        record["commercial_use"] = "true"
        result = validate_record(record, self.root)
        self.assertEqual(result["rights"]["commercial_use"]["value"], "unknown")
        self.assertEqual(result["verdict"], "BLOCKED")

    def test_pending_and_not_applicable_distribution_values_are_unknown(self):
        for raw in ("pending", "not_applicable", "maybe", 1, None):
            record = dict(ALLOWED_RECORD, record_id=f"pack_dist_{raw}_v1")
            record["source_redistribution"] = raw
            result = validate_record(record, self.root)
            self.assertEqual(result["rights"]["source_redistribution"]["value"], "unknown", msg=str(raw))

    def test_normalizers_keep_three_way_distinction(self):
        self.assertEqual(normalize_bool_right(True), "allowed")
        self.assertEqual(normalize_bool_right(False), "denied")
        self.assertEqual(normalize_bool_right(None), "unknown")
        self.assertEqual(normalize_distribution_right("allowed"), "allowed")
        self.assertEqual(normalize_distribution_right("not_allowed"), "denied")
        self.assertEqual(normalize_distribution_right("pending"), "unknown")


class FreeClaimGuardTests(EvidenceFixture):
    def test_free_claim_never_upgrades_unknown_to_allowed(self):
        record = dict(ALLOWED_RECORD, record_id="pack_free_claim_v1")
        record.pop("source_redistribution")
        record.pop("build_distribution")
        record["free_claims"] = ["100% free", "free download"]
        result = validate_record(record, self.root)
        for right in ("source_redistribution", "build_distribution"):
            self.assertEqual(result["rights"][right]["value"], "unknown")
            blocker = [b for b in result["blockers"] if b["right"] == right][0]
            self.assertEqual(blocker["kind"], "unknown")
            self.assertIn("not permission evidence", blocker["detail"])
        self.assertEqual(result["verdict"], "BLOCKED")

    def test_downloadable_flag_never_upgrades_distribution(self):
        record = dict(ALLOWED_RECORD, record_id="pack_downloadable_v1")
        record.pop("build_distribution")
        record["downloadable"] = True
        result = validate_record(record, self.root)
        self.assertEqual(result["rights"]["build_distribution"]["value"], "unknown")
        self.assertFalse(result["migration_allowed"])

    def test_free_claim_never_rescues_denied(self):
        record = dict(ALLOWED_RECORD, record_id="pack_free_denied_v1")
        record["commercial_use"] = False
        record["free_claims"] = ["free to use commercially"]
        result = validate_record(record, self.root)
        commercial = [b for b in result["blockers"] if b["right"] == "commercial_use"][0]
        self.assertEqual(commercial["kind"], "denied")

    def test_free_marker_in_notes_is_non_evidence(self):
        record = dict(ALLOWED_RECORD, record_id="pack_free_notes_v1")
        record.pop("build_distribution")
        record["notes"] = "Free to download, no license text yet."
        result = validate_record(record, self.root)
        self.assertEqual(result["rights"]["build_distribution"]["value"], "unknown")
        self.assertTrue(result["claims_guarded"])


class ContradictionTests(EvidenceFixture):
    def test_multi_source_contradiction_is_conflict(self):
        manifest = dict(ALLOWED_RECORD, record_id="pack_shared_v1", source="manifest")
        catalog = dict(ALLOWED_RECORD, record_id="pack_shared_v1", source="intake_catalog")
        catalog["commercial_use"] = False
        catalog["source_redistribution"] = "not_allowed"
        conflicts = compare_records(catalog, manifest)
        self.assertEqual(len(conflicts), 2)
        self.assertTrue(all(c["kind"] == "conflict" for c in conflicts))

    def test_contradiction_blocks_migration(self):
        result = validate_record(ALLOWED_RECORD, self.root)
        manifest_record = dict(ALLOWED_RECORD, source="manifest")
        catalog_record = dict(ALLOWED_RECORD, source="intake_catalog")
        catalog_record["build_distribution"] = "not_allowed"
        for conflict in compare_records(catalog_record, manifest_record):
            result["blockers"].append(conflict)
        result["verdict"] = "BLOCKED"
        result["migration_allowed"] = False
        self.assertFalse(result["migration_allowed"])
        self.assertEqual(result["verdict"], "BLOCKED")


class MissingEvidenceTests(EvidenceFixture):
    def test_null_evidence_is_blocked(self):
        record = dict(ALLOWED_RECORD, record_id="pack_no_evidence_v1", evidence_path=None)
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        blockers = [b for b in result["blockers"] if b["kind"] == "missing_evidence"]
        self.assertEqual(len(blockers), 1)

    def test_missing_evidence_file_is_blocked(self):
        record = dict(ALLOWED_RECORD, record_id="pack_missing_file_v1")
        record["evidence_path"] = "evidence/NOT_THERE.txt"
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        self.assertFalse(result["evidence"]["present"])

    def test_absolute_evidence_path_is_blocked(self):
        record = dict(ALLOWED_RECORD, record_id="pack_abs_evidence_v1")
        record["evidence_path"] = str(self.root / "evidence" / "LICENSE.txt")
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")
        blockers = [b for b in result["blockers"] if b["kind"] == "missing_evidence"]
        self.assertEqual(len(blockers), 1)

    def test_parent_escape_evidence_path_is_blocked(self):
        record = dict(ALLOWED_RECORD, record_id="pack_escape_evidence_v1")
        record["evidence_path"] = "../outside/LICENSE.txt"
        result = validate_record(record, self.root)
        self.assertEqual(result["verdict"], "BLOCKED")


class RecordMappingTests(unittest.TestCase):
    def test_record_from_manifest_maps_all_four_rights(self):
        manifest = {
            "pack_id": "pack_kenney_x_v1",
            "license_id": "CC0-1.0",
            "license_url": "https://example.invalid/cc0",
            "license_evidence_path": "game/Assets/Art/Candidates/Kenney/X/License.txt",
            "commercial_use": True,
            "modification_allowed": True,
            "source_redistribution": "allowed",
            "build_distribution": "allowed",
        }
        record = record_from_manifest(manifest)
        self.assertEqual(record["record_id"], "pack_kenney_x_v1")
        self.assertEqual(record["source"], "manifest")
        self.assertTrue(record["commercial_use"])
        self.assertEqual(record["build_distribution"], "allowed")

    def test_record_from_intake_entry_maps_license_object(self):
        entry = {
            "intake_id": "intake_kenney_x_001",
            "manifest_id": "pack_kenney_x_v1",
            "license": {
                "license_id": "CC0-1.0",
                "license_evidence_path": "game/Assets/Art/Candidates/Kenney/X/License.txt",
                "commercial_use": True,
                "modification_allowed": True,
                "source_redistribution": "allowed",
                "build_distribution": "allowed",
            },
        }
        record = record_from_intake_entry(entry)
        self.assertEqual(record["record_id"], "intake_kenney_x_001")
        self.assertEqual(record["source"], "intake_catalog")
        self.assertEqual(record["license_id"], "CC0-1.0")


class RealDataSmokeTests(unittest.TestCase):
    def test_real_manifests_scan_deterministic_and_read_only(self):
        paths = discover_manifest_paths(ROOT)
        self.assertEqual(len(paths), 30)
        results = scan_manifests(ROOT)
        self.assertEqual(len(results), 30)
        for result in results:
            self.assertIn(result["verdict"], ("CLEARED", "BLOCKED"))
            self.assertIn("manifest_path", result)

    def test_real_intake_catalog_scan_matches_baseline_counts(self):
        results, entry_count, manifest_count = scan_intake_catalog(ROOT)
        self.assertEqual(entry_count, 1865)
        self.assertEqual(manifest_count, 30)
        self.assertEqual(len(results), 1865)
        for result in results:
            self.assertIn(result["verdict"], ("CLEARED", "BLOCKED"))
            if result["verdict"] == "CLEARED":
                self.assertTrue(result["migration_allowed"])

    def test_module_has_no_network_imports(self):
        source = Path(__file__).resolve().parents[2].joinpath(
            "tools/d18_distribution_rights_validator.py"
        ).read_text(encoding="utf-8")
        for forbidden in ("import urllib", "import requests", "import socket", "import http", "import ftplib"):
            self.assertNotIn(forbidden, source)


if __name__ == "__main__":
    unittest.main()
