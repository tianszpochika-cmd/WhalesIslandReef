import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


class D16A05CandidateImportTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        path = ROOT / "tools" / "results" / "d16_a05_candidate_import_current_20260813.json"
        cls.report = json.loads(path.read_text(encoding="utf-8"))

    def test_unity_import_audit_passes_without_mutation(self):
        self.assertEqual(self.report["status"], "PASS")
        self.assertTrue(self.report["read_only"])
        self.assertFalse(self.report["registry_changed"])
        self.assertFalse(self.report["asset_state_changed"])
        self.assertEqual(self.report["errors"], [])

    def test_all_candidates_have_imported_meshes_and_positive_bounds(self):
        records = self.report["records"]
        self.assertEqual(len(records), 7)
        for record in records:
            with self.subTest(path=record["path"]):
                self.assertTrue(record["source_exists"])
                self.assertTrue(record["candidate_scope"])
                self.assertTrue(record["importer_present"])
                self.assertTrue(record["has_primary_mesh"])
                self.assertGreater(record["mesh_count"], 0)
                self.assertGreater(record["max_dimension"], 0.0)
                self.assertEqual(record["errors"], [])


if __name__ == "__main__":
    unittest.main()
