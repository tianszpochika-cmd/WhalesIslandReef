"""Unit tests for the D18-L05 provenance validator (read-only, offline)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_provenance_validator import validate_records, validate_source_record  # noqa: E402


AS_OF = date(2026, 8, 16)


def valid_record(**overrides):
    record = {
        "source_id": "source_kenney",
        "source_url": "https://kenney.nl/assets/pirate-kit",
        "author": "Kenney",
        "download_date": "2026-08-14",
        "original_filename": "kenney_pirate-kit.zip",
        "evidence_paths": ["evidence/kenney/source_evidence.md"],
    }
    record.update(overrides)
    return record


class ProvenanceFixturesMixin:
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self._tmp.cleanup)
        self.project_root = Path(self._tmp.name)
        evidence = self.project_root / "evidence/kenney/source_evidence.md"
        evidence.parent.mkdir(parents=True)
        evidence.write_text("official page and archive evidence", encoding="utf-8")


class TestValidRecords(ProvenanceFixturesMixin, unittest.TestCase):
    def test_valid_kenney_record_passes(self):
        result = validate_source_record(valid_record(), self.project_root, AS_OF)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["issues"], [])

    def test_organization_may_replace_author(self):
        record = valid_record(author="", organization="Kenney BV")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "PASS")

    def test_download_date_equal_to_as_of_is_not_future(self):
        record = valid_record(download_date=AS_OF.isoformat())
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "PASS")

    def test_unknown_source_with_any_https_url_is_pending_not_approved(self):
        record = valid_record(source_id="source_fancy_store", source_url="https://fancy.example.com/pack")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "PENDING")
        codes = [issue["code"] for issue in result["issues"]]
        self.assertIn("unknown_source_domain", codes)
        self.assertNotIn("source_url_domain_mismatch", codes)

    def test_subdomain_of_official_domain_is_accepted_as_format(self):
        record = valid_record(source_url="https://www.kenney.nl/assets/pirate-kit")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "PASS")


class TestMissingP0Evidence(ProvenanceFixturesMixin, unittest.TestCase):
    def test_each_missing_p0_field_fails_with_located_field(self):
        cases = [
            ("source_id", "missing_source_id"),
            ("source_url", "missing_source_url"),
            ("author", "missing_author"),
            ("download_date", "missing_download_date"),
            ("original_filename", "missing_original_filename"),
            ("evidence_paths", "missing_evidence_paths"),
        ]
        for field, code in cases:
            with self.subTest(field=field):
                record = dict(valid_record())
                record[field] = None if field != "evidence_paths" else []
                result = validate_source_record(record, self.project_root, AS_OF)
                self.assertEqual(result["status"], "FAIL", f"field {field} must fail")
                issues = {issue["code"]: issue for issue in result["issues"]}
                self.assertIn(code, issues)
                self.assertEqual(issues[code]["field"], field)

    def test_blank_author_fails(self):
        result = validate_source_record(valid_record(author="   ", organization=""), self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_author", [issue["code"] for issue in result["issues"]])


class TestInvalidFields(ProvenanceFixturesMixin, unittest.TestCase):
    def test_invalid_source_id_format_fails(self):
        record = valid_record(source_id="kenney")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_source_id", [issue["code"] for issue in result["issues"]])

    def test_non_https_url_fails(self):
        record = valid_record(source_url="http://kenney.nl/assets/pirate-kit")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_source_url", [issue["code"] for issue in result["issues"]])

    def test_url_with_credentials_fails(self):
        record = valid_record(source_url="https://user:pass@kenney.nl/assets/pirate-kit")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_source_url", [issue["code"] for issue in result["issues"]])

    def test_non_official_url_fails(self):
        record = valid_record(source_url="https://example.com/pirate-kit")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("source_url_domain_mismatch", [issue["code"] for issue in result["issues"]])

    def test_future_download_date_fails(self):
        record = valid_record(download_date="2026-08-17")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        issue = [item for item in result["issues"] if item["code"] == "future_download_date"]
        self.assertEqual(len(issue), 1)
        self.assertEqual(issue[0]["field"], "download_date")

    def test_malformed_download_date_fails(self):
        record = valid_record(download_date="2026-13-40")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_download_date", [issue["code"] for issue in result["issues"]])

    def test_original_filename_with_separator_fails(self):
        record = valid_record(original_filename="cache/kenney_pirate-kit.zip")
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("invalid_original_filename", [issue["code"] for issue in result["issues"]])


class TestEvidencePaths(ProvenanceFixturesMixin, unittest.TestCase):
    def test_absolute_windows_path_fails(self):
        record = valid_record(evidence_paths=["C:/Users/Administrator/evidence.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        issue = [item for item in result["issues"] if item["code"] == "absolute_evidence_path"]
        self.assertEqual(len(issue), 1)
        self.assertEqual(issue[0]["field"], "evidence_paths[0]")

    def test_absolute_posix_path_fails(self):
        record = valid_record(evidence_paths=["/etc/evidence.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("absolute_evidence_path", [issue["code"] for issue in result["issues"]])

    def test_traversal_escape_fails(self):
        record = valid_record(evidence_paths=["../outside.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("evidence_path_escape", [issue["code"] for issue in result["issues"]])

    def test_missing_evidence_file_fails(self):
        record = valid_record(evidence_paths=["evidence/kenney/does_not_exist.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("evidence_file_missing", [issue["code"] for issue in result["issues"]])

    def test_empty_evidence_file_fails(self):
        empty = self.project_root / "evidence/kenney/empty.md"
        empty.write_text("", encoding="utf-8")
        record = valid_record(evidence_paths=["evidence/kenney/empty.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("evidence_file_empty", [issue["code"] for issue in result["issues"]])

    def test_evidence_package_mismatch_fails(self):
        other = self.project_root / "evidence/quaternius/source_evidence.md"
        other.parent.mkdir(parents=True)
        other.write_text("quaternius pack evidence", encoding="utf-8")
        record = valid_record(evidence_paths=["evidence/quaternius/source_evidence.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("evidence_source_mismatch", [issue["code"] for issue in result["issues"]])

    def test_evidence_filename_of_other_source_fails(self):
        other = self.project_root / "evidence/quaternius_manifest.json"
        other.write_text("{}", encoding="utf-8")
        record = valid_record(evidence_paths=["evidence/quaternius_manifest.json"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("evidence_source_mismatch", [issue["code"] for issue in result["issues"]])

    def test_evidence_path_without_source_token_is_accepted(self):
        plain = self.project_root / "docs/evidence_notes.md"
        plain.parent.mkdir(parents=True)
        plain.write_text("notes", encoding="utf-8")
        record = valid_record(evidence_paths=["docs/evidence_notes.md"])
        result = validate_source_record(record, self.project_root, AS_OF)
        self.assertEqual(result["status"], "PASS")


class TestBatchAndGuards(unittest.TestCase):
    def test_duplicate_source_id_fails_across_records(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            evidence = root / "evidence/kenney/source_evidence.md"
            evidence.parent.mkdir(parents=True)
            evidence.write_text("x", encoding="utf-8")
            records = [valid_record(), valid_record(source_url="https://kenney.nl/assets/building-kit")]
            result = validate_records(records, root, AS_OF)
            self.assertEqual(result["status"], "FAIL")
            self.assertIn("duplicate_source_id", result["issue_counts"])

    def test_license_approval_and_network_flags_are_always_false(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            evidence = root / "evidence/kenney/source_evidence.md"
            evidence.parent.mkdir(parents=True)
            evidence.write_text("x", encoding="utf-8")
            result = validate_records([valid_record()], root, AS_OF)
            self.assertFalse(result["license_approval"])
            self.assertFalse(result["network_accessed"])
            self.assertTrue(result["read_only"])

    def test_record_not_object_fails(self):
        result = validate_source_record("not-a-dict", Path("."), AS_OF)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("record_not_object", [issue["code"] for issue in result["issues"]])

    def test_cli_round_trip_with_input_file(self):
        from d18_provenance_validator import main

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            evidence = root / "evidence/kenney/source_evidence.md"
            evidence.parent.mkdir(parents=True)
            evidence.write_text("x", encoding="utf-8")
            input_path = root / "records.json"
            input_path.write_text(json.dumps({"records": [valid_record()]}), encoding="utf-8")
            output_path = root / "out.json"
            code = main(["--input", str(input_path), "--project-root", str(root), "--today", AS_OF.isoformat(), "--output", str(output_path)])
            self.assertEqual(code, 0)
            result = json.loads(output_path.read_text(encoding="utf-8"))
            self.assertEqual(result["status"], "PASS")

    def test_cli_failure_exit_code(self):
        from d18_provenance_validator import main

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            input_path = root / "records.json"
            input_path.write_text(json.dumps([valid_record(author="")]), encoding="utf-8")
            code = main(["--input", str(input_path), "--project-root", str(root), "--today", AS_OF.isoformat()])
            self.assertEqual(code, 2)


if __name__ == "__main__":
    unittest.main()
