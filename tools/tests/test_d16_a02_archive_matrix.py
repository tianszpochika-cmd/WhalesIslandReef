import tempfile
import unittest
from pathlib import Path

import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d16_a02_archive_matrix import build_report, load_manifest


class D16A02ArchiveMatrixTests(unittest.TestCase):
    def test_manifest_has_unique_categories_and_exact_required_tests(self):
        manifest = load_manifest()
        labels = [item["label"] for item in manifest["categories"]]
        required = [name for item in manifest["categories"] for name in item["required_tests"]]
        self.assertEqual(10, len(labels))
        self.assertEqual(len(labels), len(set(labels)))
        self.assertEqual(len(required), len(set(required)), "存档覆盖测试名不能重复绑定")

    def test_current_unity_result_satisfies_exact_matrix(self):
        root = Path(__file__).resolve().parents[2]
        xml_path = root / "tools" / "results" / "d16_current_save_tests_editmode_unity6.xml"
        report = build_report(xml_path)
        self.assertTrue(report["passed"], report)
        self.assertEqual({}, report["missing_tests"])
        self.assertEqual(0, report["failed_or_inconclusive_count"])
        self.assertEqual(31, report["required_test_count"])

    def test_related_but_renamed_case_does_not_satisfy_manifest(self):
        xml = """<test-run><test-case name=\"D8A05_RepeatedMigration_IsNoOp_renamed\" result=\"Passed\" /></test-run>"""
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "renamed.xml"
            path.write_text(xml, encoding="utf-8")
            report = build_report(path)
        self.assertFalse(report["passed"])
        self.assertIn("D8 迁移与重复迁移", report["missing_tests"])


if __name__ == "__main__":
    unittest.main()
