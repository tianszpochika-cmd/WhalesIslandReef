import json
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_asset_manifest_audit import audit_manifest_files, discover_manifest_files  # noqa: E402


class D18AssetManifestAuditTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.manifest_root = ROOT / "docs/game/design/common/assets/manifests"
        cls.fixture_root = ROOT / "tools/tests/fixtures/d18_asset_manifest_audit"

    def audit_fixture(self, name):
        paths = discover_manifest_files(self.fixture_root / name, [])
        return audit_manifest_files(ROOT, paths)

    def test_real_kenney_candidates_pass_without_runtime_approval(self):
        result = audit_manifest_files(ROOT, discover_manifest_files(self.manifest_root, []))
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["manifest_count"], 2)
        self.assertTrue(result["read_only"])
        self.assertFalse(result["registry_modified"])

    def test_missing_hash_fixture_is_blocked(self):
        result = self.audit_fixture("missing_field")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("invalid_or_missing_sha256", result["issue_counts"])

    def test_path_escape_fixture_is_blocked(self):
        result = self.audit_fixture("path_escape")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("candidate_path_escape", result["issue_counts"])

    def test_broken_license_evidence_fixture_is_blocked(self):
        result = self.audit_fixture("broken_evidence")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("license_evidence_missing", result["issue_counts"])

    def test_illegal_state_fixture_is_blocked_and_not_approved(self):
        result = self.audit_fixture("state_transition")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("illegal_state_transition", result["issue_counts"])
        self.assertIn("missing_human_reviewer", result["issue_counts"])

    def test_unapproved_runtime_reference_is_blocked(self):
        result = self.audit_fixture("runtime_reference")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("unapproved_runtime_reference", result["issue_counts"])

    def test_same_file_multiple_sources_is_blocked(self):
        result = self.audit_fixture("conflict")
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("same_file_multiple_sources", result["issue_counts"])

    def test_template_is_explicitly_incomplete_when_audited(self):
        template = ROOT / "docs/game/design/common/assets/d18_asset_pack_manifest.example.json"
        data = json.loads(template.read_text(encoding="utf-8"))
        self.assertEqual(data["review_status"], "Candidate")
        self.assertIsNone(data["sha256"])
        self.assertIsNone(data["file_count"])


if __name__ == "__main__":
    unittest.main()
