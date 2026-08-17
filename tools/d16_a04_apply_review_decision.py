"""Apply one explicit D16-A04 human review decision.

The queue is intentionally read-only, while this tool is the narrow write
path for a reviewer who has supplied real evidence.  It accepts exactly one
record per invocation, requires an existing local evidence file and an
explicit reviewer/basis, and defaults to a dry run.  It never changes source,
license, authorization, asset state, gameplay data, warning policy, or any
other record.
"""

from __future__ import annotations

import argparse
import json
import os
import tempfile
from datetime import date
from pathlib import Path
from typing import Any

from d16_a04_review_audit import ALLOWED_HISTORICAL_TAGS, SENSITIVE_TAGS, load_entries


DECISIONS = {"Approved", "ApprovedWithWarning", "Rejected"}
PRESENTATION_LEVELS = {"Full", "Contextual", "LowIntensity", "Omitted"}


def _as_list(value: Any) -> list[Any]:
    return value if isinstance(value, list) else []


def _resolve_reference(project_root: Path, reference: str) -> Path | None:
    clean = reference.split("#", 1)[0].strip()
    if not clean or clean.startswith(("http://", "https://")):
        return None
    candidate = Path(clean)
    return candidate if candidate.is_absolute() else project_root / candidate


def _unique_strings(values: list[Any]) -> list[str]:
    result: list[str] = []
    seen: set[str] = set()
    for value in values:
        text = str(value).strip()
        if text and text not in seen:
            result.append(text)
            seen.add(text)
    return result


def _find_entry(root: Path, record_id: str) -> tuple[Path, dict[str, Any]] | None:
    matches = [(path, entry) for path, entry in load_entries(root) if str(entry.get("id")) == record_id]
    if len(matches) > 1:
        # If multiple entries match (e.g. ContentRegistry vs Content/D15/A06), prefer the Pending one
        pending_matches = [(path, entry) for path, entry in matches if entry.get("historical_review") == "Pending"]
        if len(pending_matches) == 1:
            return pending_matches[0]
        raise ValueError(f"record id is not unique: {record_id}")
    return matches[0] if matches else None


def _review_date_error(review_date: str) -> str | None:
    try:
        parsed = date.fromisoformat(review_date)
    except (TypeError, ValueError):
        return "review_date_must_be_iso_date"
    if parsed > date.today():
        return "review_date_cannot_be_in_future"
    return None


def validate_decision(
    project_root: Path,
    entry: dict[str, Any],
    *,
    decision: str,
    reviewer: str,
    evidence_refs: list[str],
    decision_basis: str,
    review_date: str,
    warning_ids: list[str] | None = None,
    skip_allowed: bool | None = None,
    presentation_levels: list[str] | None = None,
) -> list[str]:
    """Return blocking validation errors without changing ``entry``."""

    errors: list[str] = []
    if decision not in DECISIONS:
        errors.append(f"unsupported_decision:{decision}")
    if entry.get("historical_review") != "Pending":
        errors.append("entry_is_not_pending")
    if entry.get("historical_tag") not in ALLOWED_HISTORICAL_TAGS:
        errors.append("invalid_or_missing_historical_tag")
    review_date_error = _review_date_error(review_date)
    if review_date_error:
        errors.append(review_date_error)
    if not reviewer.strip():
        errors.append("reviewer_required")
    if len(decision_basis.strip()) < 20:
        errors.append("decision_basis_too_short")

    refs = _unique_strings(evidence_refs)
    if not refs:
        errors.append("evidence_ref_required")
    for reference in refs:
        resolved = _resolve_reference(project_root, reference)
        if resolved is None or not resolved.is_file():
            errors.append(f"evidence_ref_not_found:{reference}")

    existing_warnings = _unique_strings(_as_list(entry.get("content_warning_ids")))
    requested_warnings = _unique_strings(warning_ids or [])
    warnings = _unique_strings(existing_warnings + requested_warnings)
    sensitive = bool({str(tag).lower() for tag in _as_list(entry.get("tags"))} & SENSITIVE_TAGS) or bool(warnings)

    levels = _unique_strings(_as_list(entry.get("presentation_levels")) + (presentation_levels or []))
    invalid_levels = [level for level in levels if level not in PRESENTATION_LEVELS]
    if invalid_levels:
        errors.append("invalid_presentation_level:" + ",".join(invalid_levels))

    if decision == "Approved" and sensitive:
        errors.append("sensitive_entry_requires_approved_with_warning_or_rejected")
    if decision == "ApprovedWithWarning" and not warnings:
        errors.append("warning_id_required_for_approved_with_warning")
    if decision == "ApprovedWithWarning" and skip_allowed is None:
        errors.append("skip_allowed_must_be_explicit_for_warning_review")
    if sensitive and decision == "ApprovedWithWarning" and not levels:
        errors.append("presentation_level_required_for_sensitive_review")
    if decision == "Approved" and requested_warnings:
        errors.append("approved_cannot_add_content_warning")
    if not isinstance(entry.get("skip_allowed"), bool) and skip_allowed is None:
        errors.append("skip_allowed_must_be_present_or_explicit")
    return errors


def _atomic_write(path: Path, payload: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temp_name = tempfile.mkstemp(prefix=f".{path.stem}.", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(payload, handle, ensure_ascii=False, indent=2)
            handle.write("\n")
            handle.flush()
            os.fsync(handle.fileno())
        os.replace(temp_name, path)
    finally:
        if os.path.exists(temp_name):
            os.unlink(temp_name)


def apply_decision(
    root: Path,
    *,
    record_id: str,
    decision: str,
    reviewer: str,
    evidence_refs: list[str],
    decision_basis: str,
    warning_ids: list[str] | None = None,
    skip_allowed: bool | None = None,
    presentation_levels: list[str] | None = None,
    apply: bool = False,
    review_date: str,
) -> dict[str, Any]:
    """Validate and optionally apply one explicit decision."""

    found = _find_entry(root, record_id)
    if found is None:
        return {"status": "INVALID", "record_id": record_id, "errors": ["record_not_found"]}
    path, entry = found
    project_root = root.parent if root.name == "game" else root
    errors = validate_decision(
        project_root,
        entry,
        decision=decision,
        reviewer=reviewer,
        evidence_refs=evidence_refs,
        decision_basis=decision_basis,
        review_date=review_date,
        warning_ids=warning_ids,
        skip_allowed=skip_allowed,
        presentation_levels=presentation_levels,
    )
    result: dict[str, Any] = {
        "task": "D16-A04",
        "status": "INVALID" if errors else ("APPLIED" if apply else "DRY_RUN"),
        "record_id": record_id,
        "path": path.as_posix(),
        "decision": decision,
        "reviewer": reviewer,
        "review_date": review_date,
        "evidence_refs": _unique_strings(evidence_refs),
        "errors": errors,
        "policy": "One explicit evidence-backed decision only; no automatic or batch approval.",
    }
    if errors or not apply:
        return result

    existing_refs = _unique_strings(_as_list(entry.get("review_evidence_refs")))
    entry["historical_review"] = decision
    entry["reviewer"] = reviewer.strip()
    entry["review_evidence_refs"] = _unique_strings(existing_refs + evidence_refs)
    if warning_ids:
        entry["content_warning_ids"] = _unique_strings(_as_list(entry.get("content_warning_ids")) + warning_ids)
    if skip_allowed is not None:
        entry["skip_allowed"] = skip_allowed
    if presentation_levels:
        entry["presentation_levels"] = _unique_strings(_as_list(entry.get("presentation_levels")) + presentation_levels)
    entry["review_decision_basis"] = {
        "decision": decision,
        "basis": decision_basis.strip(),
        "reviewer": reviewer.strip(),
        "reviewed_at": review_date,
        "evidence_refs": entry["review_evidence_refs"],
        "explicit_human_decision": True,
        "automated_approval": False,
    }
    _atomic_write(path, entry)
    return result


def _parse_bool(value: str) -> bool:
    if value == "true":
        return True
    if value == "false":
        return False
    raise argparse.ArgumentTypeError("expected true or false")


def main() -> int:
    parser = argparse.ArgumentParser(description="Apply one explicit D16-A04 review decision")
    parser.add_argument("root", type=Path, help="D16-A04 audit root, normally game")
    parser.add_argument("--id", dest="record_id", required=True)
    parser.add_argument("--decision", required=True, choices=sorted(DECISIONS))
    parser.add_argument("--reviewer", required=True)
    parser.add_argument("--evidence-ref", dest="evidence_refs", action="append", required=True)
    parser.add_argument("--decision-basis", required=True)
    parser.add_argument("--warning-id", dest="warning_ids", action="append", default=[])
    parser.add_argument("--skip-allowed", type=_parse_bool)
    parser.add_argument("--presentation-level", dest="presentation_levels", action="append", default=[])
    parser.add_argument("--review-date", default=date.today().isoformat())
    parser.add_argument("--apply", action="store_true", help="write the one validated decision")
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()

    result = apply_decision(
        args.root,
        record_id=args.record_id,
        decision=args.decision,
        reviewer=args.reviewer,
        evidence_refs=args.evidence_refs,
        decision_basis=args.decision_basis,
        warning_ids=args.warning_ids,
        skip_allowed=args.skip_allowed,
        presentation_levels=args.presentation_levels,
        apply=args.apply,
        review_date=args.review_date,
    )
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0 if result["status"] != "INVALID" else 2


if __name__ == "__main__":
    raise SystemExit(main())
