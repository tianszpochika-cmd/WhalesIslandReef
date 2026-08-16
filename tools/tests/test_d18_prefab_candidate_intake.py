import json
import tempfile
import unittest
from pathlib import Path

from tools.d18_prefab_candidate_intake_audit import build_catalog, relative_file


class PrefabCandidateIntakeTests(unittest.TestCase):
    def test_relative_file_rejects_escape(self):
        with self.assertRaises(ValueError):
            relative_file(Path("."), "../outside.fbx")

    def test_only_import_verified_manifest_enters_catalog(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            manifest_dir = root / "docs/game/design/common/assets/packs/environment/test"
            candidate = root / "game/Assets/Art/Candidates/Test/test.fbx"
            license_path = root / "game/Assets/Art/Candidates/Test/License.txt"
            import_report = root / "game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json"
            manifest_dir.mkdir(parents=True)
            candidate.parent.mkdir(parents=True)
            import_report.parent.mkdir(parents=True)
            candidate.write_bytes(b"mesh")
            license_path.write_text("CC0-1.0", encoding="utf-8")
            import_report.write_text("{}", encoding="utf-8")
            data = {
                "pack_id": "pack_test",
                "license_id": "CC0-1.0",
                "license_evidence_path": "game/Assets/Art/Candidates/Test/License.txt",
                "sha256": "a" * 64,
                "review_status": "ImportVerified",
                "import_status": "ImportVerified",
                "runtime_consumers": [],
                "file_count": 1,
                "candidate_files": [{"path": "game/Assets/Art/Candidates/Test/test.fbx", "role": "candidate_environment_module", "target_id": "env_test"}],
            }
            (manifest_dir / "test.manifest.json").write_text(json.dumps(data), encoding="utf-8")
            catalog, result = build_catalog(root)
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(catalog["counts"]["intake_eligible_file_total"], 1)
            self.assertTrue(catalog["eligible_entries"][0]["intake_eligible"])

    def test_candidate_and_not_imported_are_excluded(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            manifest_dir = root / "docs/game/design/common/assets"
            manifest_dir.mkdir(parents=True)
            data = {
                "pack_id": "pack_pending",
                "license_id": "CC0-1.0",
                "license_evidence_path": "missing.txt",
                "sha256": "b" * 64,
                "review_status": "Candidate",
                "import_status": "NotImported",
                "runtime_consumers": [],
                "file_count": 0,
                "candidate_files": [],
            }
            (manifest_dir / "pending.manifest.json").write_text(json.dumps(data), encoding="utf-8")
            catalog, result = build_catalog(root)
            self.assertEqual(result["status"], "PASS")
            self.assertEqual(catalog["counts"]["intake_eligible_file_total"], 0)
            self.assertEqual(len(catalog["excluded_manifests"]), 1)


if __name__ == "__main__":
    unittest.main()
