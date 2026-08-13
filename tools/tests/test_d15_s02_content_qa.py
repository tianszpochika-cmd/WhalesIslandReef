import unittest
from pathlib import Path

import sys

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from d15_s02_content_qa import BATCHES, RUNTIME_SAVE_SAMPLE_IDS, _runtime_state_probe, run_qa


class D15S02ContentQATests(unittest.TestCase):
    def test_batch_inventory_and_sample_coverage(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry"
        report = run_qa(root)
        self.assertEqual(set(BATCHES), set(report["counts"]))
        self.assertEqual(24, report["summary"]["sample_count"])
        self.assertEqual(0, len(report["reference_gaps"]["missing_roots"]))
        self.assertEqual(0, len(report["reference_gaps"]["missing_nested_ids"]))
        self.assertEqual({}, report["model_reuse"]["real_duplicate_models"])

    def test_a08_localization_pair_is_complete(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry"
        report = run_qa(root)
        self.assertEqual([], report["localization"]["a08_missing_zh_cn"])
        self.assertEqual([], report["localization"]["a08_missing_en_us"])

    def test_a01_a07_localization_package_is_proven(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry"
        report = run_qa(root)
        self.assertEqual(0, report["localization"]["non_a08_keys_without_package_proof"])

    def test_required_runtime_save_restore_contracts_are_explicit(self):
        root = Path(__file__).resolve().parents[2] / "game" / "Assets" / "Resources" / "ContentRegistry"
        report = run_qa(root)
        runtime = report["runtime_save_restore"]
        self.assertEqual(sorted(RUNTIME_SAVE_SAMPLE_IDS), runtime["required_sample_ids"])
        self.assertEqual(9, runtime["verified_count"])
        self.assertEqual(0, runtime["missing_count"])
        self.assertEqual([], [issue for issue in report["errors"] if issue["code"].startswith("runtime-save-restore")])

    def test_save_restore_test_case_alone_is_not_evidence(self):
        valid, reason = _runtime_state_probe({
            "id": "sample",
            "test_cases": ["D15-S02-SAVE-RESTORE-sample"],
        })
        self.assertFalse(valid)
        self.assertEqual("missing_runtime_state_contract", reason)


if __name__ == "__main__":
    unittest.main()
