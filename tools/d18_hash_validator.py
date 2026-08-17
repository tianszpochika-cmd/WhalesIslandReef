"""Offline SHA-256 integrity validator for D18 asset records (D18-L06).

The tool validates, per record: hash format (64 hex chars), repository
relative path (no absolute path or traversal escape), source file existence
and the actual SHA-256 of the file on disk. Records whose source file is
unavailable return PENDING (missing evidence) and are never approved; the
tool never synthesises a hash and its result always reports
``approved_count: 0``.

Duplicate hashes shared by different records are reported (``duplicate_hash``
issue plus a ``duplicates`` list); files are never deleted. The tool is
strictly read-only against source files and manifests; the only file it
writes is the optional ``--output`` result JSON.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
PATH_ALIASES = ("path", "candidate_file_path", "repository_path")
ABS_RE = re.compile(r"^[A-Za-z]:[/\\]")
STATUS_OK = "OK"
STATUS_PENDING = "PENDING"
STATUS_ERROR = "ERROR"


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def sha256_file(path: Path) -> str:
    """Compute the real SHA-256 of a file (read-only, streamed)."""

    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def _path_value(record: dict[str, Any]) -> str:
    for key in PATH_ALIASES:
        if key in record:
            return _text(record[key])
    return ""


def _resolve(project_root: Path, raw: str) -> Path | None:
    """Resolve a repository-relative path; reject absolute/traversal escape."""

    normalized = raw.replace("\\", "/")
    if normalized.startswith("/") or normalized.startswith("//") or ABS_RE.match(normalized):
        return None
    resolved = (project_root / normalized).resolve()
    return resolved if _inside(resolved, project_root) else None


def _validate_record(project_root: Path, record: Any, index: int) -> dict[str, Any]:
    if not isinstance(record, dict):
        return {
            "index": index,
            "id": f"record[{index}]",
            "path": "",
            "sha256": None,
            "status": STATUS_ERROR,
            "code": "invalid_record",
            "reason": "record must be an object",
        }
    record_id = _text(record.get("id")) or f"record[{index}]"
    raw_path = _path_value(record)
    digest = _text(record.get("sha256"))

    if not raw_path:
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_ERROR, "code": "invalid_path",
            "reason": "path is missing or empty",
        }
    if not digest:
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_ERROR, "code": "missing_sha256",
            "reason": "sha256 is missing; evidence is incomplete, nothing can be verified",
        }
    if not SHA256_RE.fullmatch(digest):
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_ERROR, "code": "invalid_hash_format",
            "reason": "sha256 must be 64 hexadecimal characters",
        }

    resolved = _resolve(project_root, raw_path)
    if resolved is None:
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_ERROR, "code": "path_escape",
            "reason": f"path escapes the project root or is absolute: {raw_path}",
        }
    if not resolved.is_file():
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_PENDING, "code": "file_missing",
            "reason": "source file is unavailable; pending evidence — never synthesise a hash or approve",
        }

    actual = sha256_file(resolved)
    if actual != digest.upper():
        return {
            "index": index, "id": record_id, "path": raw_path, "sha256": digest,
            "status": STATUS_ERROR, "code": "hash_mismatch",
            "reason": f"content hash {actual} does not match declared {digest.upper()}",
        }
    return {
        "index": index, "id": record_id, "path": raw_path, "sha256": digest,
        "status": STATUS_OK, "code": "ok", "reason": "",
    }


def load_records(path: Path) -> tuple[list[Any] | None, str, str]:
    """Load records from a JSON file; return (records, error_code, message)."""

    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        return None, "records_not_found", f"records file does not exist: {path}"
    except json.JSONDecodeError as exc:
        return None, "records_json_error", f"line {exc.lineno}: {exc.msg}"
    except OSError as exc:
        return None, "records_read_error", str(exc)
    if isinstance(data, dict):
        if isinstance(data.get("records"), list):
            return data["records"], "", ""
        if isinstance(data.get("packages"), list):
            files: list[Any] = []
            for package in data["packages"]:
                if isinstance(package, dict) and isinstance(package.get("files"), list):
                    files.extend(package["files"])
            return files, "", ""
        return None, "input_not_supported", "records JSON must be a list, {\"records\": [...]} or {\"packages\": [...]}"
    if isinstance(data, list):
        return data, "", ""
    return None, "input_not_supported", "records JSON must be a list, {\"records\": [...]} or {\"packages\": [...]}"


def validate_records(project_root: Path, records: Iterable[Any]) -> dict[str, Any]:
    """Validate records and return a deterministic JSON-compatible result."""

    project_root = project_root.resolve()
    items = [result for result in (_validate_record(project_root, record, index) for index, record in enumerate(records))]
    issues: list[dict[str, Any]] = []
    for item in items:
        if item["status"] == STATUS_OK:
            continue
        issues.append(
            {
                "code": item["code"],
                "severity": "pending" if item["code"] == "file_missing" else "error",
                "message": item["reason"],
                "record_id": item["id"],
                "path": item["path"],
            }
        )

    by_hash: dict[str, list[dict[str, Any]]] = {}
    for item in items:
        if item["status"] == STATUS_OK:
            by_hash.setdefault(item["sha256"].upper(), []).append(item)
    duplicates: list[dict[str, Any]] = []
    for digest, group in by_hash.items():
        if len(group) > 1:
            duplicates.append(
                {
                    "sha256": digest,
                    "record_ids": [item["id"] for item in group],
                    "paths": sorted(item["path"] for item in group),
                }
            )
            issues.append(
                {
                    "code": "duplicate_hash",
                    "severity": "error",
                    "message": f"same sha256 declared by {len(group)} records; reported only, no file is deleted",
                    "record_ids": [item["id"] for item in group],
                }
            )

    if not items:
        issues.append({"code": "no_records", "severity": "error", "message": "no records were supplied; nothing was verified"})

    code_counts = Counter(issue["code"] for issue in issues)
    error_count = sum(1 for issue in issues if issue.get("severity") == "error")
    pending_count = sum(1 for issue in issues if issue.get("severity") == "pending")
    if error_count > 0:
        status = "BLOCKED"
    elif pending_count > 0:
        status = "PENDING"
    else:
        status = "PASS"

    return {
        "task": "D18-L06",
        "tool": "tools/d18_hash_validator.py",
        "read_only": True,
        "approved_count": 0,
        "status": status,
        "record_count": len(items),
        "verified_count": sum(1 for item in items if item["status"] == STATUS_OK),
        "pending_count": pending_count,
        "error_count": error_count,
        "duplicate_count": len(duplicates),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
        "duplicates": duplicates,
        "records": items,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--records", type=Path, action="append", default=[])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    records: list[Any] = []
    input_issues: list[dict[str, Any]] = []
    for records_path in args.records:
        loaded, code, message = load_records(records_path)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(records_path)})
            continue
        records.extend(loaded)
    result = validate_records(project_root, records)
    if input_issues:
        result["input_issues"] = input_issues
        result["error_count"] += len(input_issues)
        result["status"] = "BLOCKED"
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"PASS": 0, "PENDING": 1, "BLOCKED": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
