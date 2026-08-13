import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d16_s01_readiness import evaluate  # noqa: E402


def _external(status="PASS", errors=None, steam=False):
    return {
        "status": "PASS" if status == "PASS" else "BLOCKED",
        "steam_upload_authorized": steam,
        "gates": {
            f"D16-A0{number}": {"status": status, "errors": list(errors or [])}
            for number in range(1, 7)
        },
    }


def _a04(pending=0, rejected=0, errors=0):
    return {
        "pending_review_count": pending,
        "rejected_review_count": rejected,
        "structural_error_count": errors,
    }


def _a05(placeholder=0, invalid=0, missing=0):
    return {
        "asset_state_counts": {"placeholder": placeholder, "integrated": 1},
        "invalid_metadata_count": invalid,
        "declared_path_missing_count": missing,
    }


def _a06(status="PASS"):
    return {"technical_preparation_status": status}


class D16S01ReadinessTests(unittest.TestCase):
    def test_current_style_blockers_are_explicit_and_story_is_not_run(self):
        result = evaluate(
            _external(status="BLOCKED", errors=["missing_evidence_file"]),
            _a04(pending=267),
            _a05(placeholder=2264),
            _a06(),
            captured_at="2026-08-13",
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertFalse(result["story_executed"])
        self.assertIn("D16-A01:status_BLOCKED", result["blocking_reasons"])
        self.assertIn("D16-A04:pending_review_count_267", result["blocking_reasons"])
        self.assertIn("D16-A05:placeholder_count_2264", result["blocking_reasons"])
        self.assertTrue(result["policy"]["read_only"])

    def test_all_six_gates_pass_makes_local_story_ready_without_steam_upload(self):
        result = evaluate(_external(steam=False), _a04(), _a05(), _a06())
        self.assertEqual(result["status"], "READY_TO_START")
        self.assertFalse(result["steam_upload_authorized"])
        self.assertFalse(result["story_executed"])
        self.assertEqual(result["blocking_reasons"], [])

    def test_gate_errors_block_even_when_status_is_pass(self):
        result = evaluate(
            _external(errors=["raw_artifact_missing"]),
            _a04(),
            _a05(),
            _a06(),
        )
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("D16-A01:errors_present", result["blocking_reasons"])

    def test_malformed_local_audits_do_not_look_ready(self):
        result = evaluate(_external(), {"pending_review_count": 0, "rejected_review_count": 0, "structural_error_count": 0}, {}, _a06())
        self.assertEqual(result["status"], "BLOCKED")
        self.assertIn("D16-A05:asset_state_counts_missing", result["blocking_reasons"])


if __name__ == "__main__":
    unittest.main()
