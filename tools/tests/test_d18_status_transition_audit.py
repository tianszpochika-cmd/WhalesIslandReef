"""Unit tests for the D18-L35 table-driven state-transition audit (read-only)."""

from __future__ import annotations

import hashlib
import json
import sys
import tempfile
import unittest
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_status_transition_audit import (  # noqa: E402
    MATRIX,
    audit_all,
    audit_record,
    expand_wildcards,
    load_matrix,
    matrix_consistency,
)


def make_record(event="INTAKE_APPROVE", from_state="Candidate", to_state="IntakeEligible", **overrides):
    record = {
        "record_id": "TR-asset_test_v1-1",
        "canonical_asset_id": "asset_test_v1",
        "from_state": from_state,
        "to_state": to_state,
        "event": event,
        "operator": "tool:d18_status_transition_audit",
        "occurred_at": "2026-08-16T10:00:00+08:00",
        "evidence_paths": ["evidence/chain.md"],
        "reason": "test reason",
        "prior_record_id": None,
        "result_state": to_state,
    }
    record.update(overrides)
    return record


class AuditFixturesMixin(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.addCleanup(self._tmp.cleanup)
        self.project_root = Path(self._tmp.name)
        evidence = self.project_root / "evidence/chain.md"
        evidence.parent.mkdir(parents=True)
        evidence.write_text("chain evidence", encoding="utf-8")

    def audit(self, record, **kwargs):
        return audit_record(record, MATRIX, phase=kwargs.get("phase", "logic"), project_root=kwargs.get("project_root", self.project_root))


class TestAllowedTransitions(AuditFixturesMixin):
    def test_t1_candidate_intake_approve_passes(self):
        self.assertEqual(self.audit(make_record())["status"], "PASS")

    def test_t2_intake_runtime_request_passes(self):
        record = make_record(event="RUNTIME_REQUEST", from_state="IntakeEligible", to_state="RuntimeRequested")
        self.assertEqual(self.audit(record)["status"], "PASS")

    def test_t3_runtime_request_approve_matrix_valid_and_phase_gated(self):
        record = make_record(event="RUNTIME_APPROVE", from_state="RuntimeRequested", to_state="RuntimeApproved", operator="human:runtime_gate")
        verdict_logic = self.audit(record, phase="logic")
        self.assertEqual(verdict_logic["status"], "FAIL")
        self.assertIn("logic_phase_forbidden", [issue["code"] for issue in verdict_logic["issues"]])
        verdict_runtime = self.audit(record, phase="runtime")
        self.assertEqual(verdict_runtime["status"], "PASS")

    def test_t4_runtime_release_approve_phase_gated_and_human(self):
        record = make_record(event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="human:release_reviewer", prior_record_id="TR-asset_test_v1-3")
        verdict_logic = self.audit(record, phase="logic")
        self.assertEqual(verdict_logic["status"], "FAIL")
        self.assertIn("logic_phase_forbidden", [issue["code"] for issue in verdict_logic["issues"]])
        verdict_runtime = self.audit(record, phase="runtime")
        self.assertEqual(verdict_runtime["status"], "PASS")

    def test_t5_reject_each_rejectable_state_passes(self):
        for state in ("Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved"):
            with self.subTest(state=state):
                record = make_record(event="REJECT", from_state=state, to_state="Rejected")
                self.assertEqual(self.audit(record)["status"], "PASS", state)

    def test_t6_revoke_each_revocable_state_passes(self):
        for state in ("Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved"):
            with self.subTest(state=state):
                record = make_record(event="REVOKE", from_state=state, to_state="Revoked", operator="human:reviewer", prior_record_id="TR-asset_test_v1-0")
                self.assertEqual(self.audit(record)["status"], "PASS", state)

    def test_t7_re_register_human_passes(self):
        record = make_record(event="RE_REGISTER", from_state="Revoked", to_state="Candidate", operator="human:reviewer")
        self.assertEqual(self.audit(record)["status"], "PASS")


class TestForbiddenTransitions(AuditFixturesMixin):
    def test_candidate_skip_runtime_requested_fails(self):
        for event in ("RUNTIME_REQUEST", "RUNTIME_APPROVE", "RELEASE_APPROVE", "INTAKE_APPROVE"):
            with self.subTest(event=event):
                to = {"RUNTIME_REQUEST": "RuntimeRequested", "RUNTIME_APPROVE": "RuntimeApproved", "RELEASE_APPROVE": "ReleaseApproved", "INTAKE_APPROVE": "RuntimeRequested"}[event]
                record = make_record(event=event, from_state="Candidate", to_state=to)
                verdict = self.audit(record, phase="runtime")
                self.assertEqual(verdict["status"], "FAIL", event)
                self.assertIn("transition_not_allowed", [issue["code"] for issue in verdict["issues"]])

    def test_candidate_to_runtime_approved_and_release_fail(self):
        for to_state in ("RuntimeApproved", "ReleaseApproved"):
            with self.subTest(to_state=to_state):
                verdict = self.audit(make_record(event="RUNTIME_APPROVE" if to_state == "RuntimeApproved" else "RELEASE_APPROVE", from_state="Candidate", to_state=to_state, operator="human:reviewer"), phase="runtime")
                self.assertEqual(verdict["status"], "FAIL")

    def test_intake_eligible_skip_fails(self):
        for event, to_state in (("RUNTIME_APPROVE", "RuntimeApproved"), ("RELEASE_APPROVE", "ReleaseApproved")):
            with self.subTest(event=event):
                verdict = self.audit(make_record(event=event, from_state="IntakeEligible", to_state=to_state, operator="human:reviewer"), phase="runtime")
                self.assertEqual(verdict["status"], "FAIL")

    def test_runtime_requested_to_release_approved_fails(self):
        verdict = self.audit(make_record(event="RELEASE_APPROVE", from_state="RuntimeRequested", to_state="ReleaseApproved", operator="human:reviewer"), phase="runtime")
        self.assertEqual(verdict["status"], "FAIL")

    def test_release_approved_to_rejected_fails(self):
        verdict = self.audit(make_record(event="REJECT", from_state="ReleaseApproved", to_state="Rejected"))
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("transition_not_allowed", [issue["code"] for issue in verdict["issues"]])

    def test_rejected_terminal_has_no_out_edges(self):
        for to_state in ("Candidate", "IntakeEligible", "Revoked", "Rejected", "RuntimeRequested"):
            with self.subTest(to_state=to_state):
                event = "RE_REGISTER" if to_state == "Candidate" else "REJECT" if to_state == "Rejected" else "REVOKE"
                record = make_record(event=event, from_state="Rejected", to_state=to_state, operator="human:reviewer", prior_record_id="TR-asset_test_v1-0")
                verdict = self.audit(record, phase="runtime")
                self.assertEqual(verdict["status"], "FAIL", to_state)

    def test_revoked_cannot_skip_to_higher_states(self):
        for event, to_state in (("INTAKE_APPROVE", "IntakeEligible"), ("RUNTIME_APPROVE", "RuntimeApproved"), ("RUNTIME_REQUEST", "RuntimeRequested")):
            with self.subTest(to_state=to_state):
                verdict = self.audit(make_record(event=event, from_state="Revoked", to_state=to_state, operator="human:reviewer"), phase="runtime")
                self.assertEqual(verdict["status"], "FAIL")

    def test_revoked_recovery_requires_re_register_event(self):
        record = make_record(event="INTAKE_APPROVE", from_state="Revoked", to_state="Candidate")
        verdict = self.audit(record, phase="runtime")
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("transition_not_allowed", [issue["code"] for issue in verdict["issues"]])

    def test_re_register_requires_human_operator(self):
        record = make_record(event="RE_REGISTER", from_state="Revoked", to_state="Candidate", operator="tool:d18_status_transition_audit")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("re_register_requires_human", [issue["code"] for issue in verdict["issues"]])


class TestReleaseApprovalForgery(AuditFixturesMixin):
    def test_release_approve_by_tool_fails(self):
        record = make_record(event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="tool:d18_status_transition_audit", prior_record_id="TR-asset_test_v1-3")
        verdict = self.audit(record, phase="runtime")
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("release_approve_requires_human", [issue["code"] for issue in verdict["issues"]])

    def test_release_approve_from_wrong_state_fails(self):
        record = make_record(event="RELEASE_APPROVE", from_state="Candidate", to_state="ReleaseApproved", operator="human:reviewer")
        verdict = self.audit(record, phase="runtime")
        self.assertEqual(verdict["status"], "FAIL")

    def test_runtime_approve_by_tool_in_runtime_phase_is_accepted_as_audit(self):
        record = make_record(event="RUNTIME_APPROVE", from_state="RuntimeRequested", to_state="RuntimeApproved", operator="human:runtime_gate")
        verdict = self.audit(record, phase="runtime")
        self.assertEqual(verdict["status"], "PASS")


class TestAuditCompleteness(AuditFixturesMixin):
    def test_reject_without_reason_fails(self):
        record = make_record(event="REJECT", from_state="Candidate", to_state="Rejected", reason=None)
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_reason", [issue["code"] for issue in verdict["issues"]])

    def test_revoke_without_reason_fails(self):
        record = make_record(event="REVOKE", from_state="IntakeEligible", to_state="Revoked", operator="human:reviewer", prior_record_id="TR-asset_test_v1-0", reason=None)
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_reason", [issue["code"] for issue in verdict["issues"]])

    def test_revoke_without_prior_record_fails(self):
        record = make_record(event="REVOKE", from_state="IntakeEligible", to_state="Revoked", operator="human:reviewer", prior_record_id=None)
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_prior_record_id", [issue["code"] for issue in verdict["issues"]])

    def test_release_approve_without_prior_record_fails(self):
        record = make_record(event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="human:reviewer", prior_record_id=None)
        verdict = self.audit(record, phase="runtime")
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_prior_record_id", [issue["code"] for issue in verdict["issues"]])

    def test_re_register_without_reason_fails(self):
        record = make_record(event="RE_REGISTER", from_state="Revoked", to_state="Candidate", operator="human:reviewer", reason=None)
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_reason", [issue["code"] for issue in verdict["issues"]])

    def test_result_state_mismatch_fails(self):
        record = make_record(result_state="Candidate")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("result_state_mismatch", [issue["code"] for issue in verdict["issues"]])

    def test_missing_evidence_fails(self):
        record = make_record(evidence_paths=[])
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("missing_evidence", [issue["code"] for issue in verdict["issues"]])

    def test_evidence_path_escape_fails(self):
        record = make_record(evidence_paths=["C:/Windows/system32/x.md"])
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("evidence_path_escape", [issue["code"] for issue in verdict["issues"]])

    def test_evidence_file_missing_fails(self):
        record = make_record(evidence_paths=["evidence/missing.md"])
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("evidence_file_missing", [issue["code"] for issue in verdict["issues"]])


class TestWaiver(AuditFixturesMixin):
    def test_expired_waiver_blocks_intake_approve(self):
        record = make_record(waiver={"waiver_id": "wv-1", "expires_at": "2026-08-10", "status": "Active"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("expired_waiver", [issue["code"] for issue in verdict["issues"]])

    def test_expired_waiver_blocks_runtime_request(self):
        record = make_record(event="RUNTIME_REQUEST", from_state="IntakeEligible", to_state="RuntimeRequested", waiver={"waiver_id": "wv-1", "expires_at": "2026-08-10", "status": "Active"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("expired_waiver", [issue["code"] for issue in verdict["issues"]])

    def test_expired_waiver_guides_to_reject(self):
        record = make_record(event="REJECT", from_state="Candidate", to_state="Rejected", waiver={"waiver_id": "wv-1", "expires_at": "2026-08-10", "status": "Expired"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "PASS")
        codes = [issue["code"] for issue in verdict["issues"]]
        self.assertNotIn("expired_waiver", codes)
        self.assertIn("expired_waiver_note", codes)

    def test_expired_waiver_guides_to_revoke(self):
        record = make_record(event="REVOKE", from_state="IntakeEligible", to_state="Revoked", operator="human:reviewer", prior_record_id="TR-asset_test_v1-0", waiver={"waiver_id": "wv-1", "expires_at": "2026-08-10", "status": "Active"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "PASS")

    def test_active_waiver_does_not_block(self):
        record = make_record(waiver={"waiver_id": "wv-2", "expires_at": "2026-08-20", "status": "Active"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "PASS")

    def test_waiver_without_expiry_fails(self):
        record = make_record(waiver={"waiver_id": "wv-3"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("invalid_waiver", [issue["code"] for issue in verdict["issues"]])

    def test_waiver_invalid_status_fails(self):
        record = make_record(waiver={"waiver_id": "wv-3", "expires_at": "2026-08-20", "status": "Suspended"})
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("invalid_waiver", [issue["code"] for issue in verdict["issues"]])


class TestUnknownInputs(AuditFixturesMixin):
    def test_unknown_event_fails(self):
        record = make_record(event="NONSENSE", from_state="Candidate", to_state="IntakeEligible")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("unknown_event", [issue["code"] for issue in verdict["issues"]])

    def test_unknown_state_fails(self):
        record = make_record(from_state="Pending")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("unknown_state", [issue["code"] for issue in verdict["issues"]])

    def test_unknown_to_state_fails(self):
        record = make_record(to_state="Approved")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("unknown_state", [issue["code"] for issue in verdict["issues"]])

    def test_invalid_operator_fails(self):
        record = make_record(operator="reviewer")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("invalid_operator", [issue["code"] for issue in verdict["issues"]])

    def test_invalid_occurred_at_fails(self):
        record = make_record(occurred_at="2026-08-16")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("invalid_occurred_at", [issue["code"] for issue in verdict["issues"]])

    def test_invalid_record_id_fails(self):
        record = make_record(record_id="TR-1")
        verdict = self.audit(record)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("invalid_record_id", [issue["code"] for issue in verdict["issues"]])

    def test_record_not_object_fails(self):
        verdict = audit_record("not-a-record", MATRIX, project_root=self.project_root)
        self.assertEqual(verdict["status"], "FAIL")
        self.assertIn("record_not_object", [issue["code"] for issue in verdict["issues"]])


class TestChainAudit(AuditFixturesMixin):
    def build_chain(self):
        return [
            make_record(record_id="TR-asset_test_v1-1", event="INTAKE_APPROVE", from_state="Candidate", to_state="IntakeEligible"),
            make_record(record_id="TR-asset_test_v1-2", event="RUNTIME_REQUEST", from_state="IntakeEligible", to_state="RuntimeRequested"),
            make_record(record_id="TR-asset_test_v1-3", event="RUNTIME_APPROVE", from_state="RuntimeRequested", to_state="RuntimeApproved", operator="human:runtime_gate"),
        ]

    def test_valid_chain_passes(self):
        result = audit_all(self.build_chain(), MATRIX, phase="runtime", project_root=self.project_root)
        self.assertEqual(result["status"], "PASS")
        self.assertEqual(result["chain_issue_count"], 0)

    def test_chain_break_fails(self):
        chain = self.build_chain()
        chain[1] = dict(chain[1], from_state="Candidate")
        result = audit_all(chain, MATRIX, phase="runtime", project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("chain_break", result["issue_counts"])

    def test_duplicate_record_id_fails(self):
        chain = self.build_chain()
        chain.append(dict(chain[0]))
        result = audit_all(chain, MATRIX, phase="runtime", project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("duplicate_record_id", result["issue_counts"])

    def test_revoke_prior_must_exist(self):
        record = make_record(event="REVOKE", from_state="IntakeEligible", to_state="Revoked", operator="human:reviewer", prior_record_id="TR-asset_test_v1-9")
        result = audit_all([record], MATRIX, project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("missing_prior_record", result["issue_counts"])

    def test_revoke_prior_state_mismatch_fails(self):
        chain = [
            make_record(record_id="TR-asset_test_v1-1", event="INTAKE_APPROVE", from_state="Candidate", to_state="IntakeEligible"),
            make_record(record_id="TR-asset_test_v1-2", event="REVOKE", from_state="Candidate", to_state="Revoked", operator="human:reviewer", prior_record_id="TR-asset_test_v1-1"),
        ]
        result = audit_all(chain, MATRIX, project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("prior_record_state_mismatch", result["issue_counts"])

    def test_release_approve_prior_must_be_runtime_approved_grant(self):
        chain = [
            make_record(record_id="TR-asset_test_v1-1", event="INTAKE_APPROVE", from_state="Candidate", to_state="IntakeEligible"),
            make_record(record_id="TR-asset_test_v1-2", event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="human:reviewer", prior_record_id="TR-asset_test_v1-1"),
        ]
        result = audit_all(chain, MATRIX, phase="runtime", project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertIn("wrong_prior_record", result["issue_counts"])


class TestMatrixAndReadOnly(AuditFixturesMixin):
    def test_embedded_matrix_matches_frozen_document(self):
        doc = ROOT / "docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md"
        self.assertTrue(doc.is_file())
        self.assertEqual(load_matrix(doc), MATRIX)

    def test_matrix_internally_consistent(self):
        issues = matrix_consistency(MATRIX)
        self.assertEqual([i["code"] for i in issues], [])

    def test_reachability_unique_path_via_expansion(self):
        allowed = {(row["from"], row["to"]) for row in expand_wildcards(MATRIX["allowed_transitions"])}
        for skip in (("Candidate", "RuntimeApproved"), ("Candidate", "ReleaseApproved"), ("IntakeEligible", "RuntimeApproved"), ("IntakeEligible", "ReleaseApproved"), ("RuntimeRequested", "ReleaseApproved")):
            self.assertNotIn(skip, allowed)
        self.assertIn(("Candidate", "IntakeEligible"), allowed)
        self.assertIn(("IntakeEligible", "RuntimeRequested"), allowed)
        self.assertIn(("RuntimeRequested", "RuntimeApproved"), allowed)
        self.assertIn(("RuntimeApproved", "ReleaseApproved"), allowed)
        self.assertIn(("Revoked", "Candidate"), allowed)

    def test_logic_phase_grants_are_counted_and_blocked(self):
        records = [
            make_record(event="RUNTIME_APPROVE", from_state="RuntimeRequested", to_state="RuntimeApproved", operator="human:runtime_gate"),
            make_record(event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="human:reviewer", prior_record_id="TR-asset_test_v1-1"),
        ]
        result = audit_all(records, MATRIX, phase="logic", project_root=self.project_root)
        self.assertEqual(result["status"], "FAIL")
        self.assertEqual(result["logic_phase_grants_attempted"], 2)
        self.assertIn("logic_phase_forbidden", result["issue_counts"])

    def test_catalog_scan_flags_phase_holds(self):
        from d18_status_transition_audit import scan_catalog

        records = [
            {"canonical_asset_id": "asset_ok_v1", "state": "Candidate"},
            {"canonical_asset_id": "asset_bad_v1", "state": "RuntimeApproved"},
            {"canonical_asset_id": "asset_bad_v2", "state": "ReleaseApproved"},
            {"canonical_asset_id": "asset_weird_v1", "state": "Approved"},
        ]
        verdicts, holds, fails = scan_catalog(records, phase="logic")
        self.assertEqual(holds, 2)
        self.assertEqual(fails, 3)
        codes = [issue["code"] for v in verdicts for issue in v["issues"]]
        self.assertIn("logic_phase_state_hold", codes)
        self.assertIn("unknown_state", codes)

    def test_real_catalogs_are_read_only(self):
        from d18_status_transition_audit import main

        targets = [
            ROOT / "docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json",
            ROOT / "docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json",
            ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json",
        ]
        for target in targets:
            self.assertTrue(target.is_file(), str(target))
        before = {str(target): hashlib.sha256(target.read_bytes()).hexdigest() for target in targets}
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp) / "out.json"
            code = main(["--catalog", str(targets[0]), "--catalog", str(targets[1]), "--catalog", str(targets[2]), "--project-root", str(ROOT), "--output", str(out)])
            self.assertEqual(code, 0, "real catalogs hold no RuntimeApproved/ReleaseApproved state")
            result = json.loads(out.read_text(encoding="utf-8"))
            self.assertTrue(result["read_only"])
            self.assertGreaterEqual(result["catalog_records_scanned"], 70)
            self.assertEqual(result["catalog_state_holds"], 0)
        after = {str(target): hashlib.sha256(target.read_bytes()).hexdigest() for target in targets}
        self.assertEqual(before, after)

    def test_cli_failure_exit_code_and_output(self):
        from d18_status_transition_audit import main

        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            evidence = root / "evidence/chain.md"
            evidence.parent.mkdir(parents=True)
            evidence.write_text("x", encoding="utf-8")
            records = [make_record(event="RELEASE_APPROVE", from_state="RuntimeApproved", to_state="ReleaseApproved", operator="tool:d18_status_transition_audit")]
            input_path = root / "records.json"
            input_path.write_text(json.dumps({"records": records}), encoding="utf-8")
            out_path = root / "out.json"
            code = main(["--records", str(input_path), "--project-root", str(root), "--output", str(out_path)])
            self.assertEqual(code, 2)
            result = json.loads(out_path.read_text(encoding="utf-8"))
            self.assertIn("release_approve_requires_human", result["issue_counts"])
            self.assertIn("logic_phase_forbidden", result["issue_counts"])

    def test_embedding_mismatch_rejected_by_cli(self):
        from d18_status_transition_audit import main

        with tempfile.TemporaryDirectory() as tmp:
            bogus = Path(tmp) / "bogus.md"
            bogus.write_text("```json\n{\"allowed_transitions\": []}\n```\n", encoding="utf-8")
            code = main(["--matrix-doc", str(bogus)])
            self.assertEqual(code, 2)


if __name__ == "__main__":
    unittest.main()
