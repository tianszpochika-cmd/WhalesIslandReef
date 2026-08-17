import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_hash_validator import load_records, sha256_file, validate_records  # noqa: E402


def digest(content: bytes) -> str:
    return hashlib.sha256(content).hexdigest()


class D18HashValidatorTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)
        self.addCleanup(self._tmp.cleanup)

    def write_file(self, rel: str, content: bytes) -> Path:
        target = self.root / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(content)
        return target

    def validate(self, records):
        return validate_records(self.root, records)

    def test_valid_hash_passes(self):
        self.write_file("game/a.fbx", b"mesh-data")
        result = self.validate([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"mesh-data")}])
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["verified_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertEqual(result["records"][0]["status"], "OK")

    def test_lowercase_hash_accepted(self):
        self.write_file("game/a.fbx", b"mesh-data")
        result = self.validate([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"mesh-data").lower()}])
        self.assertEqual(result["status"], "PASS")

    def test_invalid_hash_format_is_blocked(self):
        self.write_file("game/a.fbx", b"mesh-data")
        result = self.validate([{"id": "a", "path": "game/a.fbx", "sha256": "not-a-sha256"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_hash_format", result["issue_counts"])
        self.assertEqual(result["records"][0]["status"], "ERROR")

    def test_missing_sha256_is_blocked(self):
        self.write_file("game/a.fbx", b"mesh-data")
        result = self.validate([{"id": "a", "path": "game/a.fbx"}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("missing_sha256", result["issue_counts"])

    def test_missing_file_is_pending_and_never_approved(self):
        result = self.validate([{"id": "a", "path": "game/missing.fbx", "sha256": digest(b"any")}])
        self.assertEqual(result["status"], "PENDING")
        self.assertEqual(result["pending_count"], 1)
        self.assertEqual(result["approved_count"], 0)
        self.assertIn("file_missing", result["issue_counts"])
        self.assertEqual(result["records"][0]["status"], "PENDING")

    def test_tampered_content_is_blocked(self):
        self.write_file("game/a.fbx", b"original")
        result = self.validate([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"original")}])
        self.assertEqual(result["status"], "PASS")
        self.write_file("game/a.fbx", b"tampered-content")
        result = self.validate([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"original")}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("hash_mismatch", result["issue_counts"])

    def test_duplicate_hash_reported_without_deletion(self):
        file_a = self.write_file("game/a.fbx", b"same-content")
        file_b = self.write_file("game/b.fbx", b"same-content")
        records = [
            {"id": "a", "path": "game/a.fbx", "sha256": digest(b"same-content")},
            {"id": "b", "path": "game/b.fbx", "sha256": digest(b"same-content")},
        ]
        result = self.validate(records)
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("duplicate_hash", result["issue_counts"])
        self.assertEqual(result["duplicate_count"], 1)
        self.assertEqual(sorted(result["duplicates"][0]["paths"]), ["game/a.fbx", "game/b.fbx"])
        self.assertTrue(file_a.is_file())
        self.assertTrue(file_b.is_file())

    def test_path_escape_is_blocked(self):
        for bad in ("../outside.bin", "/etc/passwd", "C:/Windows/win.ini"):
            result = self.validate([{"id": "x", "path": bad, "sha256": digest(b"x")}])
            self.assertEqual(result["status"], "BLOCKED", bad)
            self.assertIn("path_escape", result["issue_counts"], bad)
            self.assertEqual(result["records"][0]["code"], "path_escape", bad)

    def test_invalid_path_is_blocked(self):
        result = self.validate([{"id": "x", "path": "", "sha256": digest(b"x")}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_path", result["issue_counts"])
        result = self.validate([{"id": "x", "sha256": digest(b"x")}])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_path", result["issue_counts"])

    def test_invalid_record_is_blocked(self):
        result = self.validate(["not-an-object"])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_record", result["issue_counts"])

    def test_empty_records_is_blocked(self):
        result = self.validate([])
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("no_records", result["issue_counts"])

    def test_validator_never_modifies_source_files_or_manifests(self):
        source = self.write_file("game/a.fbx", b"mesh-data")
        manifest = self.write_file("game/records.json", json.dumps([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"mesh-data")}]).encode())
        before = (source.read_bytes(), manifest.read_bytes())
        result = self.validate(json.loads(manifest.read_text(encoding="utf-8")))
        self.assertEqual(result["status"], "PASS")
        self.assertEqual((source.read_bytes(), manifest.read_bytes()), before)

    def test_load_records_supports_list_records_and_packages(self):
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            list_file = base / "list.json"
            list_file.write_text(json.dumps([{"id": "a", "path": "game/a.fbx", "sha256": digest(b"x")}]), encoding="utf-8")
            records, code, message = load_records(list_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["id"], "a")

            dict_file = base / "dict.json"
            dict_file.write_text(json.dumps({"records": [{"id": "b"}]}), encoding="utf-8")
            records, code, message = load_records(dict_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["id"], "b")

            packages_file = base / "packages.json"
            packages_file.write_text(
                json.dumps({"packages": [{"package": "P", "files": [{"repository_path": "game/a.fbx", "sha256": digest(b"x")}]}]}),
                encoding="utf-8",
            )
            records, code, message = load_records(packages_file)
            self.assertEqual(code, "")
            self.assertEqual(records[0]["repository_path"], "game/a.fbx")

    def test_load_records_missing_and_broken_json(self):
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            records, code, message = load_records(base / "missing.json")
            self.assertEqual(code, "records_not_found")
            broken = base / "broken.json"
            broken.write_text("{not json", encoding="utf-8")
            records, code, message = load_records(broken)
            self.assertEqual(code, "records_json_error")
            self.assertIsNone(records)

    def test_sha256_file_matches_manual_digest(self):
        path = self.write_file("game/a.bin", b"payload")
        self.assertEqual(sha256_file(path), digest(b"payload").upper())


if __name__ == "__main__":
    unittest.main()
