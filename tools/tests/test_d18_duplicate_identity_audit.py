import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_duplicate_identity_audit import (  # noqa: E402
    audit_records,
    load_records,
    main,
)


def digest(content: bytes) -> str:
    return hashlib.sha256(content).hexdigest()


def record(path, content, **kwargs):
    out = {"path": path, "sha256": digest(content)}
    out.update(kwargs)
    return out


class D18DuplicateIdentityAuditTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.addCleanup(self._tmp.cleanup)

    def write_file(self, rel: str, content: bytes) -> Path:
        target = self.root / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(content)
        return target

    def audit(self, records, **kwargs):
        return audit_records(self.root, records, **kwargs)

    def test_byte_identical_copy_is_exact_duplicate(self):
        records = [
            record("game/a/f1.fbx", b"same", source_id="source_kenney", pack_id="pack_p", canonical_id="id_a", original_filename="f.fbx"),
            record("game/b/f2.fbx", b"same", source_id="source_kenney", pack_id="pack_p", canonical_id="id_b", original_filename="f.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["exact_duplicate"], 1)
        self.assertEqual(result["counts"]["suspected_duplicate"], 0)
        finding = result["exact_duplicates"][0]
        self.assertEqual(finding["kind"], "byte_identical_copy")
        self.assertEqual(finding["disposition"], "PENDING_ADJUDICATION")
        self.assertTrue(finding["not_fixed"])
        self.assertTrue(finding["registered_only"])
        self.assertEqual(finding["sha256"], digest(b"same").upper())
        self.assertEqual(len(finding["members"]), 2)
        self.assertEqual(result["fixed_count"], 0)
        self.assertEqual(result["files_deleted"], 0)

    def test_duplicate_record_in_one_input_is_exact_duplicate(self):
        rec = record("game/a/f.fbx", b"payload", source_id="source_kenney", pack_id="pack_p")
        result = self.audit([dict(rec), dict(rec)])
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["exact_duplicate"], 1)
        self.assertEqual(result["exact_duplicates"][0]["kind"], "duplicate_record")

    def test_renamed_copy_within_pack_is_suspected(self):
        records = [
            record("game/a/f1.fbx", b"same", source_id="source_kenney", pack_id="pack_p", original_filename="one.fbx"),
            record("game/b/f2.fbx", b"same", source_id="source_kenney", pack_id="pack_p", original_filename="two.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["suspected_duplicate"], 1)
        finding = result["suspected_duplicates"][0]
        self.assertEqual(finding["kind"], "renamed_copy")
        self.assertEqual(finding["disposition"], "PENDING_ADJUDICATION")

    def test_canonical_identity_collision_is_suspected(self):
        records = [
            record("game/a/f1.fbx", b"content-one", source_id="source_kenney", pack_id="pack_p", canonical_id="asset_x_v01", original_filename="one.fbx"),
            record("game/b/f2.fbx", b"content-two", source_id="source_kenney", pack_id="pack_p", canonical_id="asset_x_v01", original_filename="two.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["suspected_duplicate"], 1)
        finding = result["suspected_duplicates"][0]
        self.assertEqual(finding["kind"], "canonical_identity_collision")
        self.assertIn("asset_x_v01", finding["reason"])
        self.assertEqual(len(finding["members"]), 2)

    def test_path_hash_conflict_is_suspected(self):
        records = [
            record("game/a/f.fbx", b"content-one", source_id="source_kenney", pack_id="pack_p", original_filename="one.fbx"),
            record("game/a/f.fbx", b"content-two", source_id="source_kenney", pack_id="pack_p", original_filename="two.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["suspected_duplicate"], 1)
        finding = result["suspected_duplicates"][0]
        self.assertEqual(finding["kind"], "path_hash_conflict")
        self.assertEqual(finding["disposition"], "PENDING_ADJUDICATION")

    def test_same_name_different_hash_is_legitimate_variant_never_fixed(self):
        records = [
            record("game/a/f1.fbx", b"variant-1k", source_id="source_kenney", pack_id="pack_p", original_filename="tex.png"),
            record("game/b/f2.fbx", b"variant-2k", source_id="source_kenney", pack_id="pack_p", original_filename="tex.png"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["counts"]["exact_duplicate"], 0)
        self.assertEqual(result["counts"]["suspected_duplicate"], 0)
        self.assertEqual(result["counts"]["legitimate_variant"], 1)
        finding = result["legitimate_variants"][0]
        self.assertEqual(finding["kind"], "named_variant")
        self.assertEqual(finding["disposition"], "OBSERVATION_ONLY")
        self.assertTrue(finding["not_fixed"])
        self.assertEqual(result["fixed_count"], 0)

    def test_same_hash_across_sources_is_legal_mirror_never_fixed(self):
        records = [
            record("game/a/f1.fbx", b"mirror", source_id="source_kenney", pack_id="pack_p", original_filename="m.fbx"),
            record("game/b/f2.fbx", b"mirror", source_id="source_quaternius", pack_id="pack_q", original_filename="m.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["counts"]["exact_duplicate"], 0)
        self.assertEqual(result["counts"]["suspected_duplicate"], 0)
        self.assertEqual(result["counts"]["legitimate_variant"], 1)
        finding = result["legitimate_variants"][0]
        self.assertEqual(finding["kind"], "legal_mirror")
        self.assertEqual(finding["disposition"], "OBSERVATION_ONLY")
        self.assertTrue(finding["not_fixed"])
        self.assertFalse(result["exact_duplicates"])
        self.assertFalse(result["suspected_duplicates"])
        self.assertEqual(result["fixed_count"], 0)

    def test_same_hash_cross_pack_same_source_is_legal_mirror(self):
        records = [
            record("game/a/f1.fbx", b"mirror", source_id="source_kenney", pack_id="pack_p1", original_filename="shared.fbx"),
            record("game/b/f2.fbx", b"mirror", source_id="source_kenney", pack_id="pack_p2", original_filename="shared.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["counts"]["exact_duplicate"], 0)
        self.assertEqual(result["counts"]["legitimate_variant"], 1)
        self.assertEqual(result["legitimate_variants"][0]["kind"], "legal_mirror")

    def test_same_pack_case_insensitive_comparison(self):
        records = [
            record("game/a/f1.fbx", b"same", source_id="SOURCE_Kenney", pack_id="Pack_P", original_filename="F.FBX"),
            record("game/b/f2.fbx", b"same", source_id="source_kenney", pack_id="pack_p", original_filename="f.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["counts"]["exact_duplicate"], 1)
        self.assertEqual(result["exact_duplicates"][0]["kind"], "byte_identical_copy")

    def test_co_declared_paths_are_merged_not_duplicates(self):
        declared_a = [record("game/a/f.fbx", b"same", source_id="source_kenney", canonical_id="id_x", original_filename="f.fbx")]
        declared_b = [{"path": "game/a/f.fbx", "sha256": digest(b"same"), "pack_id": "pack_p"}]
        result = audit_records(self.root, declared_a + declared_b, declared_labels=["file_a.json", "file_b.json"])
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["counts"]["exact_duplicate"], 0)
        self.assertEqual(len(result["co_declared_paths"]), 1)
        merged = [r for r in result["records"] if r["status"] == "ok"]
        self.assertEqual(len(merged), 1)
        self.assertEqual(merged[0]["co_declared"], ["file_b.json"])
        self.assertEqual(merged[0]["canonical_id"], "id_x")
        self.assertEqual(merged[0]["source_id"], "source_kenney")

    def test_cross_input_path_hash_conflict_is_suspected(self):
        declared_a = [{"path": "game/a/f.fbx", "sha256": digest(b"one"), "source_id": "source_kenney"}]
        declared_b = [{"path": "game/a/f.fbx", "sha256": digest(b"two"), "source_id": "source_kenney"}]
        result = audit_records(self.root, declared_a + declared_b, declared_labels=["a.json", "b.json"])
        self.assertEqual(result["status"], "DUPLICATES_REGISTERED")
        self.assertEqual(result["counts"]["suspected_duplicate"], 1)
        self.assertEqual(result["suspected_duplicates"][0]["kind"], "path_hash_conflict")

    def test_manifest_loader_computes_real_hashes_and_reports_missing(self):
        self.write_file("game/Assets/Art/a.fbx", b"mesh")
        manifest = self.root / "manifests" / "pack.json"
        manifest.parent.mkdir(parents=True, exist_ok=True)
        manifest.write_text(
            json.dumps({
                "manifest_kind": "d18_asset_pack_manifest",
                "pack_id": "pack_test",
                "source_id": "source_test",
                "candidate_files": [
                    {"path": "game/Assets/Art/a.fbx", "role": "r", "target_id": "asset_test_a"},
                    {"path": "game/Assets/Art/missing.fbx", "role": "r", "target_id": "asset_test_missing"},
                ],
            }),
            encoding="utf-8",
        )
        result = audit_records(self.root, [], manifest_paths=[manifest])
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["pending_count"], 1)
        self.assertEqual(result["missing_file_count"], 1)
        self.assertEqual(result["missing_files"][0]["path"], "game/Assets/Art/missing.fbx")
        ok_records = [r for r in result["records"] if r["status"] == "ok"]
        self.assertEqual(len(ok_records), 1)
        self.assertEqual(ok_records[0]["sha256"], digest(b"mesh").upper())
        self.assertEqual(ok_records[0]["record_id"], "pack_test[0]")
        self.assertEqual(ok_records[0]["canonical_id"], "asset_test_a")
        self.assertEqual(ok_records[0]["original_filename"], "a.fbx")

    def test_invalid_manifest_is_blocked(self):
        manifest = self.root / "bad.manifest.json"
        manifest.write_text(json.dumps({"pack_id": "pack_bad"}), encoding="utf-8")
        result = audit_records(self.root, [], manifest_paths=[manifest])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_manifest", result["issue_counts"])

    def test_missing_sha256_is_blocked(self):
        result = self.audit([{"path": "game/a/f.fbx"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_sha256", result["issue_counts"])

    def test_invalid_hash_format_is_blocked(self):
        result = self.audit([{"path": "game/a/f.fbx", "sha256": "not-a-hash"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_hash_format", result["issue_counts"])

    def test_invalid_path_is_blocked(self):
        result = self.audit([{"sha256": digest(b"x")}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_path", result["issue_counts"])

    def test_path_escape_is_blocked(self):
        for bad in ("../outside.bin", "C:/Windows/win.ini", "/etc/passwd"):
            result = self.audit([{"path": bad, "sha256": digest(b"x")}])
            self.assertEqual(result["status"], "BLOCKED", bad)
            self.assertIn("path_escape", result["issue_counts"], bad)

    def test_invalid_record_is_blocked(self):
        result = self.audit(["not-an-object"])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_record", result["issue_counts"])

    def test_empty_records_is_blocked(self):
        result = self.audit([])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("no_records", result["issue_counts"])

    def test_load_records_supports_three_shapes(self):
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            list_file = base / "list.json"
            list_file.write_text(json.dumps([{"path": "game/a", "sha256": digest(b"x")}]), encoding="utf-8")
            records, code, _ = load_records(list_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["path"], "game/a")

            dict_file = base / "dict.json"
            dict_file.write_text(json.dumps({"records": [{"path": "game/b"}]}), encoding="utf-8")
            records, code, _ = load_records(dict_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["path"], "game/b")

            packages_file = base / "packages.json"
            packages_file.write_text(
                json.dumps({"packages": [{"package": "PackX", "files": [
                    {"repository_path": "game/c", "sha256": digest(b"y"), "original_filename": "c.fbx"}]}]}),
                encoding="utf-8",
            )
            records, code, _ = load_records(packages_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["pack_id"], "PackX")
            self.assertEqual(records[0]["repository_path"], "game/c")

    def test_load_records_missing_and_broken_json(self):
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            records, code, _ = load_records(base / "missing.json")
            self.assertEqual(code, "records_not_found")
            broken = base / "broken.json"
            broken.write_text("{not json", encoding="utf-8")
            records, code, _ = load_records(broken)
            self.assertEqual(code, "records_json_error")
            self.assertIsNone(records)

    def test_read_only_never_touches_source_files(self):
        source = self.write_file("game/a/f.fbx", b"mesh-data")
        records = [{"path": "game/a/f.fbx", "sha256": digest(b"mesh-data"), "source_id": "source_kenney"}]
        before = source.read_bytes()
        result = self.audit(records)
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(source.read_bytes(), before)
        self.assertTrue(result["read_only"])
        self.assertTrue(result["registrations_only"])
        self.assertEqual(result["files_deleted"], 0)
        self.assertEqual(result["files_renamed"], 0)
        self.assertEqual(result["files_moved"], 0)
        self.assertEqual(result["fixed_count"], 0)

    def test_deterministic_output(self):
        records = [
            record("game/a/f1.fbx", b"same", source_id="source_kenney", pack_id="pack_p", original_filename="one.fbx"),
            record("game/b/f2.fbx", b"same", source_id="source_kenney", pack_id="pack_p", original_filename="two.fbx"),
            record("game/c/f3.fbx", b"unique", source_id="source_kenney", pack_id="pack_p", original_filename="three.fbx"),
        ]
        first = json.dumps(self.audit(records), sort_keys=True)
        second = json.dumps(self.audit(list(records)), sort_keys=True)
        self.assertEqual(first, second)

    def test_main_exit_codes_and_output_file(self):
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            records_file = base / "records.json"
            out_file = base / "out.json"
            records_file.write_text(
                json.dumps([{"path": "game/a/f.fbx", "sha256": digest(b"x"), "source_id": "source_kenney"}]),
                encoding="utf-8",
            )
            code = main(["--project-root", str(self.root), "--records", str(records_file), "--output", str(out_file)])
            self.assertEqual(code, 0)
            written = json.loads(out_file.read_text(encoding="utf-8"))
            self.assertEqual(written["status"], "CLEARED")
            self.assertEqual(written["tool"], "tools/d18_duplicate_identity_audit.py")

            dup_file = base / "dup.json"
            dup_file.write_text(
                json.dumps([
                    {"path": "game/a/f1.fbx", "sha256": digest(b"dup"), "source_id": "source_kenney", "pack_id": "pack_p"},
                    {"path": "game/b/f2.fbx", "sha256": digest(b"dup"), "source_id": "source_kenney", "pack_id": "pack_p"},
                ]),
                encoding="utf-8",
            )
            code = main(["--project-root", str(self.root), "--records", str(dup_file)])
            self.assertEqual(code, 1)

            bad_file = base / "bad.json"
            bad_file.write_text(json.dumps([{"path": "game/a/f.fbx"}]), encoding="utf-8")
            code = main(["--project-root", str(self.root), "--records", str(bad_file)])
            self.assertEqual(code, 2)

    def test_main_manifests_not_found_is_blocked(self):
        with tempfile.TemporaryDirectory() as temp:
            code = main(["--project-root", str(self.root), "--manifests", str(Path(temp) / "nope")])
            self.assertEqual(code, 2)

    def test_no_network_imports(self):
        source = ROOT.joinpath("tools/d18_duplicate_identity_audit.py").read_text(encoding="utf-8")
        for forbidden in ("import urllib", "import requests", "import socket", "import http", "import ftplib"):
            self.assertNotIn(forbidden, source)

    def test_clear_records_with_canonical_ids_have_no_findings(self):
        records = [
            record("game/a/f1.fbx", b"one", source_id="source_kenney", pack_id="pack_p", canonical_id="asset_a_v01", original_filename="a.fbx"),
            record("game/b/f2.fbx", b"two", source_id="source_kenney", pack_id="pack_p", canonical_id="asset_b_v01", original_filename="b.fbx"),
        ]
        result = self.audit(records)
        self.assertEqual(result["status"], "CLEARED")
        self.assertEqual(result["counts"], {"exact_duplicate": 0, "suspected_duplicate": 0, "legitimate_variant": 0})


if __name__ == "__main__":
    unittest.main()
