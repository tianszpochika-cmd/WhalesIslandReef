"""Close only the explicitly fictional, project-authored D16-A04 subset.

This is intentionally stricter than a generic bulk reviewer.  It refuses
external assets, inferred/historical records, sensitive tags, warning-bearing
records, missing evidence, and anything already reviewed.  Dry-run is the
default; ``--apply`` is required to update registry JSON files.
"""

from __future__ import annotations

import argparse
import json
from datetime import date
from pathlib import Path
from typing import Any

from d16_a04_review_audit import SENSITIVE_TAGS, load_entries


ALLOWED_INTERNAL_LICENSES = {"internal-authored", "internal-authored-plus-cc0"}
DECISION_REVIEWER = "D16-A04-SAFE-FICTIONAL-GATE"


def _list(value: Any) -> list[Any]:
    return value if isinstance(value, list) else []


def _resolve_reference(project_root: Path, reference: Any) -> Path | None:
    if not isinstance(reference, str) or not reference.strip():
        return None
    clean = reference.split("#", 1)[0]
    if not clean:
        return None
    candidate = Path(clean)
    return candidate if candidate.is_absolute() else project_root / candidate


def eligibility_reason(project_root: Path, entry: dict[str, Any]) -> str | None:
    if entry.get("historical_review") != "Pending":
        return "review_state_is_not_pending"
    if entry.get("historical_tag") != "fictional":
        return "historical_tag_is_not_explicitly_fictional"

    source = entry.get("source")
    if not isinstance(source, dict) or source.get("kind") != "project-authored":
        return "source_is_not_project_authored"
    license_data = entry.get("license")
    if not isinstance(license_data, dict) or license_data.get("status") not in ALLOWED_INTERNAL_LICENSES:
        return "license_is_not_internal_authored"

    tags = {str(tag).lower() for tag in _list(entry.get("tags"))}
    if tags & SENSITIVE_TAGS:
        return "sensitive_tag_present"
    if _list(entry.get("content_warning_ids")):
        return "content_warning_present"
    if not isinstance(entry.get("skip_allowed"), bool):
        return "skip_allowed_is_missing"

    refs = _list(entry.get("review_evidence_refs"))
    if not refs:
        return "review_evidence_refs_missing"
    if any(_resolve_reference(project_root, ref) is None for ref in refs):
        return "review_evidence_ref_invalid"
    missing = [
        str(_resolve_reference(project_root, ref))
        for ref in refs
        if _resolve_reference(project_root, ref) is not None
        and not _resolve_reference(project_root, ref).is_file()
    ]
    if missing:
        return "review_evidence_file_missing"
    return None


def apply_gate(root: Path, apply: bool, review_date: str) -> dict[str, Any]:
    project_root = root.parent if root.name == "game" else root
    eligible: list[tuple[Path, dict[str, Any]]] = []
    ineligible: list[dict[str, str]] = []
    for path, entry in load_entries(root):
        if entry.get("historical_review") != "Pending":
            continue
        reason = eligibility_reason(project_root, entry)
        if reason is None:
            eligible.append((path, entry))
        else:
            ineligible.append({"id": str(entry.get("id")), "path": path.as_posix(), "reason": reason})

    changed_ids: list[str] = []
    if apply:
        for path, entry in eligible:
            entry["historical_review"] = "Approved"
            entry["reviewer"] = DECISION_REVIEWER
            entry["review_decision_basis"] = {
                "scope": "fictional_content_only",
                "decision": "Approved",
                "reason": (
                    "Explicitly fictional, project-authored content with internal license, "
                    "existing evidence files, no sensitive tags, and no content warnings."
                ),
                "reviewed_at": review_date,
                "not_a_historical_fact_claim": True,
            }
            path.write_text(json.dumps(entry, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
            changed_ids.append(str(entry["id"]))

    return {
        "task": "D16-A04",
        "status": "APPLIED" if apply else "DRY_RUN",
        "root": root.as_posix(),
        "eligible_count": len(eligible),
        "eligible_ids": [str(entry["id"]) for _, entry in eligible],
        "changed_count": len(changed_ids),
        "changed_ids": changed_ids,
        "ineligible_pending_count": len(ineligible),
        "ineligible_pending": ineligible,
        "reviewer": DECISION_REVIEWER,
        "review_date": review_date,
        "policy": "Only explicit fictional project-authored records are eligible; no historical or external claim is approved.",
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("--apply", action="store_true", help="write the strictly eligible records")
    parser.add_argument("--review-date", default=date.today().isoformat())
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()

    result = apply_gate(args.root, args.apply, args.review_date)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
