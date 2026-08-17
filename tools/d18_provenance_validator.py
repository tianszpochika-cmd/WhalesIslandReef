"""Read-only provenance validator for D18 source records (D18-L05).

The tool validates provenance fields of a source record without accessing the
network, downloading anything, or importing resources.  It checks:

  * ``source_id``            must be ``source_<lower_snake_case>``
  * ``source_url``           must be an https URL whose host matches the
                             static official-domain reference for the source
  * ``author``/``organization``  at least one non-empty value is required
  * ``download_date``        must be an ISO-8601 calendar date, never in the
                             future relative to ``--today`` (or the real date)
  * ``original_filename``    original package/file name, no path separators
  * ``evidence_paths``       repository-relative file paths that exist and are
                             non-empty, inside the project root, and not
                             inconsistent with the declared source
  * source/evidence consistency  an evidence path that names a *different*
                             known source is a mismatch; a source whose domain
                             is not in the static reference is PENDING

Every missing or invalid P0 field produces an error that names the field, so
the failure is actionable.  A URL being well-formed and reachable is never
treated as license approval: the result always carries
``license_approval=false`` and ``network_accessed=false``.

The static official-domain table below is a format/consistency reference only;
it is not an admission decision and it never approves a package or license.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from datetime import date
from pathlib import Path
from typing import Any, Iterable
from urllib.parse import urlparse


P0_FIELDS = (
    "source_id",
    "source_url",
    "author",
    "download_date",
    "original_filename",
    "evidence_paths",
)

SOURCE_ID_RE = re.compile(r"^source_[a-z0-9_]+$")
DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
DRIVE_RE = re.compile(r"^[A-Za-z]:")
KNOWN_SOURCE_TOKENS = ("kenney", "quaternius", "polyhaven", "ambientcg", "opengameart")
SOURCE_TOKEN_RE = re.compile(r"(?<![a-z0-9])(kenney|quaternius|polyhaven|ambientcg|opengameart)(?=[_/.\-])")

OFFICIAL_SOURCE_DOMAINS: dict[str, tuple[str, ...]] = {
    "kenney": ("kenney.nl",),
    "quaternius": ("quaternius.com",),
    "polyhaven": ("polyhaven.com",),
    "ambientcg": ("ambientcg.com",),
    "opengameart": ("opengameart.org",),
}


class ProvenanceIssue(dict):
    """A JSON-serializable issue that always locates the offending field."""

    def __init__(
        self,
        code: str,
        severity: str,
        field: str,
        message: str,
        *,
        record: str = "",
    ) -> None:
        item: dict[str, Any] = {
            "code": code,
            "severity": severity,
            "field": field,
            "message": message,
        }
        if record:
            item["record"] = record
        super().__init__(sorted(item.items()))


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def source_token(source_id: Any) -> str:
    return _text(source_id).removeprefix("source_").casefold()


def _is_absolute_reference(raw: str) -> bool:
    value = raw.replace("\\", "/")
    return (
        value.startswith("/")
        or bool(DRIVE_RE.match(value))
        or value.startswith("~")
        or value.startswith("//")
    )


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _validate_source_id(value: Any, issues: list[ProvenanceIssue], record: str) -> None:
    if not _text(value):
        issues.append(ProvenanceIssue("missing_source_id", "error", "source_id", "source_id is required; expected source_<lower_snake_case>", record=record))
    elif not SOURCE_ID_RE.fullmatch(_text(value)):
        issues.append(ProvenanceIssue("invalid_source_id", "error", "source_id", "source_id must match source_<lower_snake_case>", record=record))


def _validate_source_url(value: Any, token: str, issues: list[ProvenanceIssue], record: str) -> None:
    raw = _text(value)
    if not raw:
        issues.append(ProvenanceIssue("missing_source_url", "error", "source_url", "source_url is required", record=record))
        return
    if any(char.isspace() for char in raw):
        issues.append(ProvenanceIssue("invalid_source_url", "error", "source_url", "source_url must not contain whitespace", record=record))
        return
    parsed = urlparse(raw)
    if parsed.scheme != "https":
        issues.append(ProvenanceIssue("invalid_source_url", "error", "source_url", "source_url must be an https URL", record=record))
        return
    if parsed.username or parsed.password:
        issues.append(ProvenanceIssue("invalid_source_url", "error", "source_url", "source_url must not embed credentials", record=record))
        return
    host = (parsed.hostname or "").casefold()
    if not host:
        issues.append(ProvenanceIssue("invalid_source_url", "error", "source_url", "source_url has no host", record=record))
        return
    if not token or token not in OFFICIAL_SOURCE_DOMAINS:
        issues.append(ProvenanceIssue("unknown_source_domain", "pending", "source_url", "source has no static official-domain reference; offline consistency cannot be verified", record=record))
        return
    domains = OFFICIAL_SOURCE_DOMAINS[token]
    if not any(host == domain or host.endswith("." + domain) for domain in domains):
        issues.append(ProvenanceIssue("source_url_domain_mismatch", "error", "source_url", f"source_url host {host!r} is not an official domain for {token!r}", record=record))


def _validate_author(value: Any, organization: Any, issues: list[ProvenanceIssue], record: str) -> None:
    if not _text(value) and not _text(organization):
        issues.append(ProvenanceIssue("missing_author", "error", "author", "author or organization is required; at least one must be a non-empty value", record=record))


def _validate_download_date(value: Any, as_of: date, issues: list[ProvenanceIssue], record: str) -> None:
    if value is None:
        issues.append(ProvenanceIssue("missing_download_date", "error", "download_date", "download_date is required; a package without a real download date has no P0 evidence", record=record))
        return
    if not isinstance(value, str) or not DATE_RE.fullmatch(value):
        issues.append(ProvenanceIssue("invalid_download_date", "error", "download_date", "download_date must be an ISO date YYYY-MM-DD", record=record))
        return
    try:
        parsed = date.fromisoformat(value)
    except ValueError:
        issues.append(ProvenanceIssue("invalid_download_date", "error", "download_date", "download_date is not a calendar date", record=record))
        return
    if parsed > as_of:
        issues.append(ProvenanceIssue("future_download_date", "error", "download_date", f"download_date {value} is later than the as-of date {as_of.isoformat()}", record=record))


def _validate_original_filename(value: Any, issues: list[ProvenanceIssue], record: str) -> None:
    raw = _text(value)
    if not raw:
        issues.append(ProvenanceIssue("missing_original_filename", "error", "original_filename", "original_filename is required", record=record))
        return
    if "/" in raw or "\\" in raw:
        issues.append(ProvenanceIssue("invalid_original_filename", "error", "original_filename", "original_filename must be a bare file name without path separators", record=record))
        return
    if _is_absolute_reference(raw) or raw in {".", ".."} or "://" in raw:
        issues.append(ProvenanceIssue("invalid_original_filename", "error", "original_filename", "original_filename must not be an absolute path, URL, or special directory name", record=record))


def _validate_evidence_paths(value: Any, token: str, project_root: Path, issues: list[ProvenanceIssue], record: str) -> None:
    if not isinstance(value, list) or not value:
        issues.append(ProvenanceIssue("missing_evidence_paths", "error", "evidence_paths", "evidence_paths must be a non-empty list of repository-relative file paths", record=record))
        return
    for index, entry in enumerate(value):
        field = f"evidence_paths[{index}]"
        raw = _text(entry)
        if not raw:
            issues.append(ProvenanceIssue("invalid_evidence_path", "error", field, "evidence path must be a non-empty string", record=record))
            continue
        normalized = raw.replace("\\", "/")
        if _is_absolute_reference(raw):
            issues.append(ProvenanceIssue("absolute_evidence_path", "error", field, f"evidence path must be repository-relative, got absolute reference: {raw!r}", record=record))
            continue
        candidate = (project_root / normalized).resolve()
        if not _inside(candidate, project_root):
            issues.append(ProvenanceIssue("evidence_path_escape", "error", field, f"evidence path escapes the project root: {raw!r}", record=record))
            continue
        if not candidate.is_file():
            issues.append(ProvenanceIssue("evidence_file_missing", "error", field, f"evidence file does not exist: {raw!r}", record=record))
            continue
        if candidate.stat().st_size == 0:
            issues.append(ProvenanceIssue("evidence_file_empty", "error", field, f"evidence file is empty: {raw!r}", record=record))
            continue
        lower = normalized.casefold()
        if token and token in KNOWN_SOURCE_TOKENS:
            for other in {match.group(1) for match in SOURCE_TOKEN_RE.finditer(lower)}:
                if other != token:
                    issues.append(ProvenanceIssue("evidence_source_mismatch", "error", field, f"evidence path contains a different known source ({other!r}) than the declared source ({token!r})", record=record))
                    break


def validate_source_record(record: Any, project_root: Path, as_of: date) -> dict[str, Any]:
    """Validate one provenance record and return a JSON-compatible verdict."""
    if not isinstance(record, dict):
        return {
            "record": "",
            "status": "FAIL",
            "issues": [dict(ProvenanceIssue("record_not_object", "error", "record", "record root must be an object"))],
        }
    record_id = _text(record.get("source_id"))
    issues: list[ProvenanceIssue] = []
    _validate_source_id(record.get("source_id"), issues, record_id)
    token = source_token(record.get("source_id"))
    _validate_source_url(record.get("source_url"), token, issues, record_id)
    _validate_author(record.get("author"), record.get("organization"), issues, record_id)
    _validate_download_date(record.get("download_date"), as_of, issues, record_id)
    _validate_original_filename(record.get("original_filename"), issues, record_id)
    _validate_evidence_paths(record.get("evidence_paths"), token, project_root, issues, record_id)
    errors = [issue for issue in issues if issue["severity"] == "error"]
    if errors:
        status = "FAIL"
    elif any(issue["severity"] == "pending" for issue in issues):
        status = "PENDING"
    else:
        status = "PASS"
    return {"record": record_id, "status": status, "issues": [dict(issue) for issue in issues]}


def validate_records(records: Iterable[Any], project_root: Path, as_of: date) -> dict[str, Any]:
    """Validate a list of records plus cross-record consistency."""
    project_root = project_root.resolve()
    per_record = [validate_source_record(record, project_root, as_of) for record in records]
    issues: list[ProvenanceIssue] = []
    seen_sources: dict[str, str] = {}
    seen_evidence: dict[str, str] = {}
    for verdict in per_record:
        record_id = verdict["record"]
        if record_id:
            if record_id in seen_sources:
                issues.append(ProvenanceIssue("duplicate_source_id", "error", "source_id", f"source_id {record_id!r} is also used by {seen_sources[record_id]}", record=record_id))
            else:
                seen_sources[record_id] = "another record in the same input"
        for issue in verdict["issues"]:
            if issue["code"] == "evidence_file_missing":
                key = issue["field"]
                owner = seen_evidence.get(key)
                if owner and owner != record_id:
                    issues.append(ProvenanceIssue("duplicate_evidence_path", "error", key, f"evidence path is claimed by both {owner!r} and {record_id!r}", record=record_id))
                elif not owner:
                    seen_evidence[key] = record_id
    all_issues = [dict(item) for item in issues] + [dict(item) for verdict in per_record for item in verdict["issues"]]
    errors = [item for item in all_issues if item["severity"] == "error"]
    pendings = [item for item in all_issues if item["severity"] == "pending"]
    if errors:
        status = "FAIL"
    elif pendings:
        status = "PENDING"
    else:
        status = "PASS"
    code_counts = Counter(item["code"] for item in all_issues)
    return {
        "task": "D18-L05",
        "tool": "tools/d18_provenance_validator.py",
        "read_only": True,
        "network_accessed": False,
        "license_approval": False,
        "as_of": as_of.isoformat(),
        "status": status,
        "record_count": len(per_record),
        "records": per_record,
        "issue_count": len(all_issues),
        "error_count": len(errors),
        "pending_count": len(pendings),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": all_issues,
    }


def _load_input(path: Path) -> list[Any]:
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, dict) and "records" in data:
        return list(data["records"])
    if isinstance(data, list):
        return data
    raise ValueError("input JSON must be a record list or an object with a 'records' list")


def _parse_date(value: str) -> date:
    if not DATE_RE.fullmatch(value):
        raise SystemExit(f"--today must be an ISO date YYYY-MM-DD, got {value!r}")
    return date.fromisoformat(value)


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True, help="JSON file with a record list or {'records': [...]}")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--today", type=_parse_date, default=date.today(), help="as-of date for the future-date check (deterministic tests)")
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        records = _load_input(args.input)
    except (OSError, json.JSONDecodeError, ValueError) as exc:
        print(f"d18_provenance_validator: cannot read input {args.input}: {exc}", file=sys.stderr)
        return 2
    result = validate_records(records, args.project_root, args.today)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return {"PASS": 0, "PENDING": 1, "FAIL": 2}[result["status"]]


if __name__ == "__main__":
    raise SystemExit(main())
