"""Table-driven D18 asset-state transition audit (D18-L35).

Read-only auditor for resource-state transitions.  The allowed/forbidden
transition matrix is frozen by D18-L03 section 4.4
(``docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md``) and mirrored
verbatim in :data:`MATRIX`.  ``--matrix-doc`` parses the JSON block from the
frozen document and compares it with the embedded matrix, so the audit stays
table-driven from the single source of truth.

Checks performed per audit record (L03 section 6):

* record_id / canonical_asset_id formats and cross-consistency
* from_state / to_state / event membership in the frozen enums
* operator format (``tool:<name>`` or ``human:<role>``) and role gates:
  RELEASE_APPROVE and RE_REGISTER require a human operator
* occurred_at ISO-8601 with timezone; result_state == to_state
* reason required for REJECT / REVOKE / RE_REGISTER
* prior_record_id required for REVOKE (and RELEASE_APPROVE, referencing the
  RuntimeApproved grant) and must resolve in the chain
* evidence_paths: repository-relative, existing, non-empty files
* transition membership in the allowed matrix (forbidden pairs are rejected)
* logic-phase gate: in ``phase=logic``, matrix rows with
  ``logic_phase=false`` (RUNTIME_APPROVE / RELEASE_APPROVE) are rejected as
  forgeries; in ``phase=runtime`` they may pass the audit but never grant
  anything themselves
* expired or invalid waiver (L19): no approval may be produced; only
  REJECT / REVOKE with a reason may proceed

The chain audit (L03 6.3) reconstructs each asset's history from record_id
sequence numbers: consecutive records must connect, referenced prior records
must exist with the expected state, and record_id must be unique.

The tool is read-only: it never modifies real records, catalogs or state.
``scan_catalog`` only inspects ``state`` values of existing records and
flags logic-phase holds on RuntimeApproved / ReleaseApproved.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from datetime import date, datetime
from pathlib import Path
from typing import Any, Iterable


STATES = (
    "Candidate",
    "IntakeEligible",
    "RuntimeRequested",
    "RuntimeApproved",
    "ReleaseApproved",
    "Rejected",
    "Revoked",
)
EVENTS = (
    "INTAKE_APPROVE",
    "RUNTIME_REQUEST",
    "RUNTIME_APPROVE",
    "RELEASE_APPROVE",
    "REJECT",
    "REVOKE",
    "RE_REGISTER",
)
ANY = "<any>"
ANY_REJECTABLE = "<any_rejectable>"
ANY_REVOCABLE = "<any_revocable>"
ANY_EXPANSIONS = {
    ANY: set(STATES),
    ANY_REJECTABLE: {"Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved"},
    ANY_REVOCABLE: {"Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved"},
}
WILDCARDS = frozenset(ANY_EXPANSIONS)
GRANT_EVENTS = frozenset({"INTAKE_APPROVE", "RUNTIME_REQUEST", "RUNTIME_APPROVE", "RELEASE_APPROVE", "RE_REGISTER"})
REASON_EVENTS = frozenset({"REJECT", "REVOKE", "RE_REGISTER"})
PRIOR_EVENTS = frozenset({"REVOKE", "RELEASE_APPROVE"})

MATRIX: dict[str, Any] = {
    "schema_version": "1.0",
    "document": "docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md",
    "states": list(STATES),
    "events": list(EVENTS),
    "allowed_transitions": [
        {"from": "Candidate", "event": "INTAKE_APPROVE", "to": "IntakeEligible", "writer": "L42_catalog_compiler", "audit_required": True, "logic_phase": True},
        {"from": "IntakeEligible", "event": "RUNTIME_REQUEST", "to": "RuntimeRequested", "writer": "consumer_task", "audit_required": True, "logic_phase": True},
        {"from": "RuntimeRequested", "event": "RUNTIME_APPROVE", "to": "RuntimeApproved", "writer": "runtime_phase_gate", "audit_required": True, "logic_phase": False},
        {"from": "RuntimeApproved", "event": "RELEASE_APPROVE", "to": "ReleaseApproved", "writer": "human_responsible_reviewer", "audit_required": True, "logic_phase": False},
        {"from": ANY_REJECTABLE, "event": "REJECT", "to": "Rejected", "writer": "human_or_validator", "audit_required": True, "logic_phase": True},
        {"from": ANY_REVOCABLE, "event": "REVOKE", "to": "Revoked", "writer": "human_or_validator", "audit_required": True, "logic_phase": True},
        {"from": "Revoked", "event": "RE_REGISTER", "to": "Candidate", "writer": "human_reviewer", "audit_required": True, "logic_phase": True},
    ],
    "forbidden_pairs": [
        {"from": "Candidate", "to": "RuntimeRequested"},
        {"from": "Candidate", "to": "RuntimeApproved"},
        {"from": "Candidate", "to": "ReleaseApproved"},
        {"from": "IntakeEligible", "to": "RuntimeApproved"},
        {"from": "IntakeEligible", "to": "ReleaseApproved"},
        {"from": "RuntimeRequested", "to": "ReleaseApproved"},
        {"from": "ReleaseApproved", "to": "Rejected"},
        {"from": "Rejected", "to": ANY},
        {"from": ANY, "to": "ReleaseApproved"},
    ],
}

RECORD_ID_RE = re.compile(r"^TR-asset_[a-z0-9_]+-\d+$")
ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
OPERATOR_RE = re.compile(r"^(tool|human):[^\s]+$")
ISO_DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
ISO_TS_RE = re.compile(r"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})$")


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


class AuditIssue(dict):
    def __init__(self, code: str, field: str, message: str, *, record: str = "", severity: str = "error") -> None:
        item: dict[str, Any] = {"code": code, "severity": severity, "field": field, "message": message}
        if record:
            item["record"] = record
        super().__init__(sorted(item.items()))


def expand_wildcards(rows: Iterable[dict[str, str]]) -> list[dict[str, str]]:
    expanded: list[dict[str, str]] = []
    for row in rows:
        from_set = ANY_EXPANSIONS.get(row["from"], {row["from"]})
        to_set = ANY_EXPANSIONS.get(row["to"], {row["to"]})
        for frm in sorted(from_set):
            for to in sorted(to_set):
                expanded.append({"from": frm, "to": to, **{k: v for k, v in row.items() if k not in ("from", "to")}})
    return expanded


def load_matrix(matrix_doc: Path | None) -> dict[str, Any]:
    if matrix_doc is None:
        return json.loads(json.dumps(MATRIX))
    text = matrix_doc.read_text(encoding="utf-8")
    block = re.search(r"```json\s*\n(\{.*?\n\})\s*\n```", text, re.DOTALL)
    if not block:
        raise ValueError("matrix document has no fenced JSON block")
    data = json.loads(block.group(1))
    if "allowed_transitions" not in data:
        raise ValueError("matrix JSON block has no allowed_transitions")
    return data


def matrix_consistency(matrix: dict[str, Any]) -> list[AuditIssue]:
    issues: list[AuditIssue] = []
    states = set(matrix.get("states", []))
    events = set(matrix.get("events", []))
    rows = matrix.get("allowed_transitions", [])
    pairs = matrix.get("forbidden_pairs", [])
    for row in rows:
        if row.get("from") not in states | WILDCARDS:
            issues.append(AuditIssue("matrix_unknown_state", "allowed_transitions", f"from {row.get('from')!r} is not a frozen state or wildcard"))
        if row.get("to") not in states | WILDCARDS:
            issues.append(AuditIssue("matrix_unknown_state", "allowed_transitions", f"to {row.get('to')!r} is not a frozen state or wildcard"))
        if row.get("event") not in events:
            issues.append(AuditIssue("matrix_unknown_event", "allowed_transitions", f"event {row.get('event')!r} is not a frozen event"))
    for pair in pairs:
        if pair.get("from") not in states | WILDCARDS or pair.get("to") not in states | WILDCARDS:
            issues.append(AuditIssue("matrix_unknown_state", "forbidden_pairs", f"pair {pair.get('from')!r}->{pair.get('to')!r} contains an unknown state"))
    allowed = {(row["from"], row["to"]) for row in expand_wildcards(rows)}
    forbidden = {(row["from"], row["to"]) for row in expand_wildcards(pairs)}
    overlap = sorted(allowed & forbidden)
    if overlap != [("RuntimeApproved", "ReleaseApproved")]:
        issues.append(AuditIssue("matrix_overlap", "matrix", f"allowed and forbidden pairs overlap outside the sole T4 exception: {overlap}"))
    return issues


def _evidence_check(evidence: Any, project_root: Path, issues: list[AuditIssue], record: str) -> None:
    if not isinstance(evidence, list) or not evidence:
        issues.append(AuditIssue("missing_evidence", "evidence_paths", "evidence_paths must be a non-empty list of repository-relative files", record=record))
        return
    for index, entry in enumerate(evidence):
        raw = _text(entry)
        field = f"evidence_paths[{index}]"
        if not raw:
            issues.append(AuditIssue("invalid_evidence_path", field, "evidence path must be a non-empty string", record=record))
            continue
        normalized = raw.replace("\\", "/")
        if normalized.startswith("/") or re.match(r"^[A-Za-z]:", normalized) or normalized.startswith(("~", "//")):
            issues.append(AuditIssue("evidence_path_escape", field, f"evidence path must be repository-relative: {raw!r}", record=record))
            continue
        candidate = (project_root / normalized).resolve()
        if not _inside(candidate, project_root):
            issues.append(AuditIssue("evidence_path_escape", field, f"evidence path escapes the project root: {raw!r}", record=record))
            continue
        if not candidate.is_file():
            issues.append(AuditIssue("evidence_file_missing", field, f"evidence file does not exist: {raw!r}", record=record))
            continue
        if candidate.stat().st_size == 0:
            issues.append(AuditIssue("evidence_file_empty", field, f"evidence file is empty: {raw!r}", record=record))


def _waiver_check(waiver: Any, event_date: date | None, event: str, issues: list[AuditIssue], record: str) -> None:
    if waiver is None:
        return
    if not isinstance(waiver, dict):
        issues.append(AuditIssue("invalid_waiver", "waiver", "waiver must be an object with waiver_id and expires_at", record=record))
        return
    waiver_id = _text(waiver.get("waiver_id"))
    expires = _text(waiver.get("expires_at"))
    status = _text(waiver.get("status"))
    if not waiver_id:
        issues.append(AuditIssue("invalid_waiver", "waiver.waiver_id", "waiver_id is required when a waiver is attached", record=record))
    if not expires or not ISO_DATE_RE.fullmatch(expires):
        issues.append(AuditIssue("invalid_waiver", "waiver.expires_at", "waiver expires_at is required as an ISO date (L19)", record=record))
        return
    try:
        expires_on = date.fromisoformat(expires)
    except ValueError:
        issues.append(AuditIssue("invalid_waiver", "waiver.expires_at", "waiver expires_at is not a calendar date", record=record))
        return
    if status and status not in {"Active", "Expired"}:
        issues.append(AuditIssue("invalid_waiver", "waiver.status", "waiver status must be Active or Expired", record=record))
        return
    expired = status == "Expired" or (event_date is not None and expires_on < event_date)
    if not expired:
        return
    if event in GRANT_EVENTS:
        issues.append(
            AuditIssue(
                "expired_waiver",
                "waiver.expires_at",
                f"waiver {waiver_id!r} expired on {expires}; no approval may be granted; only REJECT or REVOKE (reason required) may proceed (L03 4.5.7)",
                record=record,
            )
        )
    else:
        issues.append(
            AuditIssue(
                "expired_waiver_note",
                "waiver.expires_at",
                f"waiver {waiver_id!r} expired on {expires}; REJECT/REVOKE with reason is the only allowed follow-up",
                record=record,
                severity="warning",
            )
        )


def audit_record(record: Any, matrix: dict[str, Any], *, phase: str = "logic", project_root: Path, as_of: date | None = None) -> dict[str, Any]:
    """Audit one audit record; never modifies anything."""
    if not isinstance(record, dict):
        return {"record": "", "status": "FAIL", "issues": [dict(AuditIssue("record_not_object", "record", "audit record must be an object"))]}
    issues: list[AuditIssue] = []
    record_id = _text(record.get("record_id"))
    asset_id = _text(record.get("canonical_asset_id"))
    from_state = _text(record.get("from_state"))
    to_state = _text(record.get("to_state"))
    event = _text(record.get("event"))
    operator = _text(record.get("operator"))
    occurred = _text(record.get("occurred_at"))
    reason = record.get("reason")
    prior = _text(record.get("prior_record_id"))
    result_state = _text(record.get("result_state"))

    if not record_id:
        issues.append(AuditIssue("missing_record_id", "record_id", "record_id is required (TR-<canonical_asset_id>-<seq>)", record=record_id))
    elif not RECORD_ID_RE.fullmatch(record_id):
        issues.append(AuditIssue("invalid_record_id", "record_id", "record_id must match TR-asset_<...>-<seq>", record=record_id))
    if not asset_id:
        issues.append(AuditIssue("missing_canonical_asset_id", "canonical_asset_id", "canonical_asset_id is required", record=record_id))
    elif not ASSET_ID_RE.fullmatch(asset_id):
        issues.append(AuditIssue("invalid_canonical_asset_id", "canonical_asset_id", "canonical_asset_id must match asset_<lower_snake_case>", record=record_id))
    if record_id and asset_id and record_id.startswith("TR-") and record_id[3:] != asset_id and not record_id[3:].startswith(asset_id + "-"):
        issues.append(AuditIssue("record_id_asset_mismatch", "record_id", "record_id prefix must equal canonical_asset_id", record=record_id))

    if from_state not in STATES:
        issues.append(AuditIssue("unknown_state", "from_state", f"from_state {from_state!r} is not a frozen state", record=record_id))
    if to_state not in STATES:
        issues.append(AuditIssue("unknown_state", "to_state", f"to_state {to_state!r} is not a frozen state", record=record_id))
    if event not in EVENTS:
        issues.append(AuditIssue("unknown_event", "event", f"event {event!r} is not a frozen event", record=record_id))

    if not occurred:
        issues.append(AuditIssue("missing_occurred_at", "occurred_at", "occurred_at is required (ISO-8601 with timezone)", record=record_id))
    elif not ISO_TS_RE.fullmatch(occurred):
        issues.append(AuditIssue("invalid_occurred_at", "occurred_at", "occurred_at must be ISO-8601 with timezone", record=record_id))
    event_date: date | None = None
    if occurred and ISO_TS_RE.fullmatch(occurred):
        parsed = datetime.fromisoformat(occurred.replace("Z", "+00:00"))
        if parsed.tzinfo is None:
            issues.append(AuditIssue("invalid_occurred_at", "occurred_at", "occurred_at must carry a timezone offset", record=record_id))
        else:
            event_date = parsed.date()

    if not operator:
        issues.append(AuditIssue("missing_operator", "operator", "operator is required (tool:<name> or human:<role>)", record=record_id))
    elif not OPERATOR_RE.fullmatch(operator):
        issues.append(AuditIssue("invalid_operator", "operator", "operator must match tool:<name> or human:<role>", record=record_id))

    if not result_state:
        issues.append(AuditIssue("missing_result_state", "result_state", "result_state is required", record=record_id))
    elif to_state and result_state != to_state:
        issues.append(AuditIssue("result_state_mismatch", "result_state", f"result_state {result_state!r} must equal to_state {to_state!r}", record=record_id))

    if event in REASON_EVENTS and not isinstance(reason, str):
        issues.append(AuditIssue("missing_reason", "reason", f"{event} requires a non-empty reason", record=record_id))
    if event in PRIOR_EVENTS and not prior:
        issues.append(AuditIssue("missing_prior_record_id", "prior_record_id", f"{event} requires prior_record_id", record=record_id))

    _evidence_check(record.get("evidence_paths"), project_root, issues, record_id)
    _waiver_check(record.get("waiver"), event_date, event, issues, record_id)

    allowed = {(row["from"], row["event"], row["to"]): row for row in expand_wildcards(matrix.get("allowed_transitions", []))}
    row = allowed.get((from_state, event, to_state))
    if row is None:
        message = f"{from_state} -> {to_state} is not an allowed transition (event {event!r})"
        issues.append(AuditIssue("transition_not_allowed", "to_state", message, record=record_id))
    elif phase == "logic" and not row.get("logic_phase", True):
        issues.append(
            AuditIssue(
                "logic_phase_forbidden",
                "event",
                f"{event} ({from_state} -> {to_state}) is a runtime-phase-only grant; any occurrence in the logic phase is treated as forgery",
                record=record_id,
            )
        )
    if event == "RELEASE_APPROVE" and operator and not operator.startswith("human:"):
        issues.append(AuditIssue("release_approve_requires_human", "operator", "RELEASE_APPROVE requires a human operator; a tool must never approve a release", record=record_id))
    if event == "RE_REGISTER" and operator and not operator.startswith("human:"):
        issues.append(AuditIssue("re_register_requires_human", "operator", "RE_REGISTER (Revoked -> Candidate) requires a human operator", record=record_id))

    return {
        "record": record_id,
        "canonical_asset_id": asset_id,
        "from_state": from_state,
        "to_state": to_state,
        "event": event,
        "status": "FAIL" if any(issue["severity"] == "error" for issue in issues) else "PASS",
        "issues": [dict(issue) for issue in issues],
    }


def audit_chain(records: Iterable[Any], project_root: Path) -> list[AuditIssue]:
    """Chain integrity checks across audit records (L03 6.3)."""
    issues: list[AuditIssue] = []
    items = [r for r in records if isinstance(r, dict)]
    by_id: dict[str, dict[str, Any]] = {}
    seen: dict[str, int] = {}
    for item in items:
        record_id = _text(item.get("record_id"))
        if not record_id:
            continue
        if record_id in seen:
            issues.append(AuditIssue("duplicate_record_id", "record_id", f"record_id {record_id!r} appears more than once", record=record_id))
        seen[record_id] = 1
        by_id[record_id] = item

    for item in items:
        record_id = _text(item.get("record_id"))
        asset_id = _text(item.get("canonical_asset_id"))
        event = _text(item.get("event"))
        prior = _text(item.get("prior_record_id"))
        if event not in PRIOR_EVENTS or not prior:
            continue
        referenced = by_id.get(prior)
        if referenced is None:
            issues.append(AuditIssue("missing_prior_record", "prior_record_id", f"{event} references prior_record_id {prior!r} which is not in the audited chain", record=record_id))
            continue
        referenced_to = _text(referenced.get("to_state"))
        if event == "REVOKE":
            if referenced_to != _text(item.get("from_state")):
                issues.append(AuditIssue("prior_record_state_mismatch", "prior_record_id", f"REVOKE prior {prior!r} ended at {referenced_to!r}, expected the revoked state {item.get('from_state')!r}", record=record_id))
        elif event == "RELEASE_APPROVE":
            if referenced_to != "RuntimeApproved":
                issues.append(AuditIssue("wrong_prior_record", "prior_record_id", f"RELEASE_APPROVE prior {prior!r} must be the RuntimeApproved grant, ended at {referenced_to!r}", record=record_id))

    groups: dict[str, list[tuple[int, dict[str, Any]]]] = {}
    for item in items:
        record_id = _text(item.get("record_id"))
        match = RECORD_ID_RE.fullmatch(record_id)
        asset_id = _text(item.get("canonical_asset_id"))
        if not match or not asset_id:
            continue
        seq = int(record_id.rsplit("-", 1)[1])
        groups.setdefault(asset_id, []).append((seq, item))
    for asset_id, chain in groups.items():
        chain.sort(key=lambda pair: pair[0])
        seqs = [seq for seq, _ in chain]
        if len(set(seqs)) != len(seqs):
            issues.append(AuditIssue("duplicate_sequence", "record_id", f"asset {asset_id!r} has duplicate record sequence numbers", record=asset_id))
        for (seq, item), (nxt_seq, nxt) in zip(chain, chain[1:]):
            if seq + 1 != nxt_seq:
                issues.append(AuditIssue("chain_gap", "record_id", f"asset {asset_id!r} has a sequence gap between {seq} and {nxt_seq}", record=asset_id))
            if _text(item.get("to_state")) != _text(nxt.get("from_state")):
                issues.append(
                    AuditIssue(
                        "chain_break",
                        "from_state",
                        f"chain break for {asset_id!r}: {item.get('record_id')} ended at {item.get('to_state')!r} but {nxt.get('record_id')} starts at {nxt.get('from_state')!r}",
                        record=_text(nxt.get("record_id")),
                    )
                )
    return issues


def scan_catalog(records: Iterable[Any], *, phase: str = "logic") -> tuple[list[dict[str, Any]], int, int]:
    """Inspect state fields of existing L02-shaped records; read-only."""
    verdicts: list[dict[str, Any]] = []
    holds = 0
    for record in records:
        if not isinstance(record, dict) or "state" not in record:
            continue
        asset_id = _text(record.get("canonical_asset_id"))
        state = _text(record.get("state"))
        issues: list[AuditIssue] = []
        if state not in STATES:
            issues.append(AuditIssue("unknown_state", "state", f"state {state!r} is not a frozen state", record=asset_id))
        if phase == "logic" and state in {"RuntimeApproved", "ReleaseApproved"}:
            holds += 1
            issues.append(AuditIssue("logic_phase_state_hold", "state", f"logic-phase scan: {asset_id!r} holds {state!r}, which may only be granted in the runtime phase by its unique writer", record=asset_id))
        verdicts.append(
            {
                "record": asset_id,
                "state": state,
                "status": "FAIL" if issues else "PASS",
                "issues": [dict(issue) for issue in issues],
            }
        )
    return verdicts, holds, sum(1 for v in verdicts if v["status"] == "FAIL")


def audit_all(
    records: Iterable[Any],
    matrix: dict[str, Any],
    *,
    phase: str = "logic",
    project_root: Path,
    catalog_records: Iterable[Any] = (),
) -> dict[str, Any]:
    per_record = [audit_record(record, matrix, phase=phase, project_root=project_root) for record in records]
    chain = audit_chain([record for record in records if isinstance(record, dict)], project_root)
    catalog, holds, catalog_fails = scan_catalog(catalog_records, phase=phase)
    all_issues = [dict(issue) for issue in chain] + [dict(issue) for verdict in per_record for issue in verdict["issues"]]
    matrix_issues = matrix_consistency(matrix)
    if matrix_issues:
        all_issues.extend(dict(issue) for issue in matrix_issues)
    errors = [issue for issue in all_issues if issue["severity"] == "error"]
    grants_in_logic = sum(1 for verdict in per_record if verdict["event"] in {"RUNTIME_APPROVE", "RELEASE_APPROVE"} and phase == "logic")
    code_counts = Counter(issue["code"] for issue in all_issues)
    return {
        "task": "D18-L35",
        "tool": "tools/d18_status_transition_audit.py",
        "read_only": True,
        "network_accessed": False,
        "phase": phase,
        "status": "PASS" if not errors and not catalog_fails else "FAIL",
        "record_count": len(per_record),
        "records": per_record,
        "chain_issue_count": len(chain),
        "catalog_records_scanned": len(catalog),
        "catalog_state_holds": holds,
        "logic_phase_grants_attempted": grants_in_logic,
        "issue_count": len(all_issues),
        "error_count": len(errors),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": all_issues,
        "catalog_verdicts": catalog,
    }


def _load_list(path: Path) -> list[Any]:
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, dict) and "records" in data:
        return list(data["records"])
    if isinstance(data, list):
        return data
    return []


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--records", type=Path, default=None, help="JSON file with audit records (list or {'records': [...]})")
    parser.add_argument("--catalog", type=Path, action="append", default=[], help="catalog JSON files whose state fields are scanned read-only")
    parser.add_argument("--matrix-doc", type=Path, default=None, help="frozen state-machine document; parsed JSON block is compared with the embedded matrix")
    parser.add_argument("--phase", choices=("logic", "runtime"), default="logic")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        matrix = load_matrix(args.matrix_doc)
    except (OSError, ValueError, json.JSONDecodeError) as exc:
        print(f"d18_status_transition_audit: cannot load matrix: {exc}", file=sys.stderr)
        return 2
    if args.matrix_doc is not None and matrix != MATRIX:
        print("d18_status_transition_audit: embedded matrix differs from the frozen document matrix", file=sys.stderr)
        return 2
    records = _load_list(args.records) if args.records else []
    catalog: list[Any] = []
    for path in args.catalog:
        catalog.extend(_load_list(path))
    result = audit_all(records, matrix, phase=args.phase, project_root=args.project_root.resolve(), catalog_records=catalog)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
