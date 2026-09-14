import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / "tools/vc_g01_bootstrap_reject_aggregate.py"


class VcG01BootstrapRejectAggregateTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = SCRIPT.read_text(encoding="utf-8")

    def test_requires_three_independent_targets_and_keeps_formal_gate(self):
        for token in (
            'TARGETS = ("I", "M", "T")',
            "VC_G01_BOOTSTRAP_REJECT_{target}_20260906.json",
            'matrix.get("target_proof")',
            'status": "PASS_WITHIN_SCOPE"',
            '"vc_g01": "IN_PROGRESS"',
            "same-frame probe ",
            "formal_status_unchanged",
        ):
            self.assertIn(token, self.source)


if __name__ == "__main__":
    unittest.main()
