"""Duplicate identity audit for D18 asset records (D18-L31).

The audit detects duplicate identities across four dimensions declared per
record: sha256, source_id, canonical_id and original_filename. Every finding
is classified into one of three reviewable categories; each finding carries
the exact field values that triggered it so a human reviewer can verify it.

* exact_duplicate — byte-identical registrations of the same file (same
  sha256 + same pack + same original filename) under different paths or
  canonical ids, or the same record declared twice inside one input.
* suspected_duplicate — renamed copies inside a pack, the same path claimed
  with different hashes, or the same canonical_id claimed by files with
  different content hashes.
* legitimate_variant — the same name with different content (version,
  resolution or repack variants) or identical content mirrored across packs
  or sources (legal mirrors). These are observations with reasons, never
  duplicates that were fixed.

The tool is strictly register-only: it never deletes, renames or moves
resources, never claims anything was fixed (``fixed_count`` is always 0)
and never grants, revokes or adjudicates any resource state. It is
read-only against manifests and source files; the only file it writes is
the optional ``--output`` result JSON. Findings are registered as
``PENDING_ADJUDICATION`` (exact/suspected) or ``OBSERVATION_ONLY``
(legitimate variants); final adjudication belongs to later gates.
"""

from __future__ import annotations

import argparse
import glob
import hashlib
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
ABS_RE = re.compile(r"^[A-Za-z]:[/\\]")
PATH_ALIASES = ("path", "repository_path", "candidate_file_path")
ID_ALIASES = ("record_id", "id")
SOURCE_ALIASES = ("source_id", "source")
PACK_ALIASES = ("pack_id", "pack", "manifest_id")
CANON_ALIASES = ("canonical_id", "canonical_asset_id", "target_id", "content_id")
NAME_ALIASES = ("original_filename", "original_file", "filename", "file_name")

STATUS_OK = "ok"
STATUS_PENDING = "pending"
STATUS_ERROR = "error"

CATEGORY_EXACT = "exact_duplicate"
CATEGORY_SUSPECTED = "suspected_duplicate"
CATEGORY_LEGITIMATE = "legitimate_variant"
DISPOSITION_ADJUDICATE = "PENDING_ADJUDICATION"
DISPOSITION_OBSERVATION = "OBSERVATION_ONLY"


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _first(record: dict[str, Any], aliases: tuple[str, ...]) -> str:
    for key in aliases:
        if key in record:
            return _text(record[key])
    return ""


def _normalize_path(raw: str) -> str:
    return raw.strip().replace("\\", "/")


def _normalize_key(value: str) -> str:
    return _text(value).casefold()


def _basename(path: str) -> str:
    normalized = path.strip().replace("\\", "/").rstrip("/")
    return normalized.rsplit("/", 1)[-1] if "/" in normalized else normalized


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


def _resolve(project_root: Path, raw: str) -> Path | None:
    """Resolve a repository-relative path; reject absolute/traversal escape."""

    normalized = _normalize_path(raw)
    if normalized.startswith("/") or ABS_RE.match(normalized):
        return None
    resolved = (project_root / normalized).resolve()
    return resolved if _inside(resolved, project_root) else None


def _member(record: dict[str, Any]) -> dict[str, Any]:
    return {
        "record_id": record["record_id"],
        "path": record["path"],
        "sha256": record["sha256"],
        "source_id": record.get("source_id"),
        "pack_id": record.get("pack_id"),
        "canonical_id": record.get("canonical_id"),
        "original_filename": record.get("original_filename"),
        "input": record.get("input"),
    }


def _issue(code: str, severity: str, message: str, record_id: str = "", path: str = "") -> dict[str, Any]:
    out: dict[str, Any] = {"code": code, "severity": severity, "message": message}
    if record_id:
        out["record_id"] = record_id
    if path:
        out["path"] = path
    return out


def _normalize_record(
    record: Any,
    index: int,
    input_label: str,
    project_root: Path,
    allow_pending_hash: bool = False,
) -> dict[str, Any]:
    """Normalize one raw record; status ok / pending / error with a reason."""

    if not isinstance(record, dict):
        return {
            "index": index,
            "record_id": f"record[{index}]",
            "path": "",
            "sha256": None,
            "source_id": None,
            "pack_id": None,
            "canonical_id": None,
            "original_filename": None,
            "input": input_label,
            "status": STATUS_ERROR,
            "code": "invalid_record",
            "reason": "record must be an object",
        }
    record_id = _first(record, ID_ALIASES) or f"record[{index}]"
    raw_path = _first(record, PATH_ALIASES)
    digest = _text(record.get("sha256"))
    source_id = _first(record, SOURCE_ALIASES) or None
    pack_id = _first(record, PACK_ALIASES) or None
    canonical_id = _first(record, CANON_ALIASES) or None
    original_filename = _first(record, NAME_ALIASES) or None

    if not raw_path:
        return {
            "index": index, "record_id": record_id, "path": raw_path, "sha256": None,
            "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
            "original_filename": original_filename, "input": input_label,
            "status": STATUS_ERROR, "code": "invalid_path",
            "reason": "path is missing or empty",
        }
    path = _normalize_path(raw_path)
    resolved = _resolve(project_root, path)
    if resolved is None:
        return {
            "index": index, "record_id": record_id, "path": path, "sha256": None,
            "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
            "original_filename": original_filename, "input": input_label,
            "status": STATUS_ERROR, "code": "path_escape",
            "reason": f"path escapes the project root or is absolute: {raw_path}",
        }

    if allow_pending_hash:
        actual = sha256_file(resolved) if resolved.is_file() else None
        if actual is None:
            return {
                "index": index, "record_id": record_id, "path": path, "sha256": None,
                "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
                "original_filename": original_filename, "input": input_label,
                "status": STATUS_PENDING, "code": "file_missing",
                "reason": "source file is unavailable on disk; hash unknown, excluded from identity grouping",
            }
        digest = actual

    if not digest:
        return {
            "index": index, "record_id": record_id, "path": path, "sha256": None,
            "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
            "original_filename": original_filename, "input": input_label,
            "status": STATUS_ERROR, "code": "missing_sha256",
            "reason": "sha256 is missing; identity cannot be compared",
        }
    if not SHA256_RE.fullmatch(digest):
        return {
            "index": index, "record_id": record_id, "path": path, "sha256": None,
            "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
            "original_filename": original_filename, "input": input_label,
            "status": STATUS_ERROR, "code": "invalid_hash_format",
            "reason": "sha256 must be 64 hexadecimal characters",
        }
    if original_filename is None:
        original_filename = _basename(path)
    return {
        "index": index, "record_id": record_id, "path": path, "sha256": digest.upper(),
        "source_id": source_id, "pack_id": pack_id, "canonical_id": canonical_id,
        "original_filename": original_filename, "input": input_label,
        "status": STATUS_OK, "code": "ok", "reason": "",
    }


def load_records(path: Path) -> tuple[list[Any] | None, str, str]:
    """Load records from a JSON file; return (records, error_code, message).

    Accepts a plain list, {"records": [...]} or the A05 FILE_HASHES shape
    {"packages": [{"package": ..., "files": [{repository_path, sha256,
    original_filename}]}]}.
    """

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
            files: list[dict[str, Any]] = []
            for package in data["packages"]:
                if not isinstance(package, dict):
                    continue
                pack_id = _text(package.get("package"))
                for file_item in package.get("files", []):
                    if not isinstance(file_item, dict):
                        files.append(file_item)
                        continue
                    entry = dict(file_item)
                    entry.setdefault("pack_id", pack_id or None)
                    files.append(entry)
            return files, "", ""
        return None, "input_not_supported", "records JSON must be a list, {\"records\": [...]} or {\"packages\": [...]}"
    if isinstance(data, list):
        return data, "", ""
    return None, "input_not_supported", "records JSON must be a list, {\"records\": [...]} or {\"packages\": [...]}"


def collect_manifests(arg_value: str) -> list[Path]:
    """Resolve a --manifests argument (file, directory or glob) to files."""

    raw = Path(arg_value)
    if raw.is_file():
        return [raw]
    if raw.is_dir():
        return sorted(raw.rglob("*.manifest.json"))
    return [Path(p) for p in sorted(glob.glob(arg_value)) if Path(p).is_file()]


def load_manifest_records(project_root: Path, manifest_path: Path) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    """Build records from a pack manifest; per-file sha256 computed from disk.

    Returns (records, issues). Records carry source_id/pack_id from the
    manifest and canonical_id from each candidate_files target_id. Files that
    do not exist on disk become pending records (sha256 None) and are
    excluded from identity grouping.
    """

    label = str(manifest_path).replace("\\", "/")
    try:
        data = json.loads(manifest_path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        return [], [_issue("manifest_not_found", "error", f"manifest file does not exist: {label}", path=label)]
    except json.JSONDecodeError as exc:
        return [], [_issue("manifest_json_error", "error", f"{label}: line {exc.lineno}: {exc.msg}", path=label)]
    except OSError as exc:
        return [], [_issue("manifest_read_error", "error", str(exc), path=label)]
    if not isinstance(data, dict) or not isinstance(data.get("candidate_files"), list):
        return [], [_issue("invalid_manifest", "error", f"{label}: manifest must be an object with candidate_files list", path=label)]
    pack_id = _text(data.get("pack_id")) or label
    source_id = _text(data.get("source_id")) or None
    records: list[dict[str, Any]] = []
    for index, candidate in enumerate(data["candidate_files"]):
        if not isinstance(candidate, dict):
            continue
        path = _text(candidate.get("path"))
        if not path:
            continue
        entry: dict[str, Any] = {
            "record_id": f"{pack_id}[{index}]",
            "path": path,
            "sha256": None,
            "source_id": source_id,
            "pack_id": pack_id,
            "canonical_id": _text(candidate.get("target_id")) or None,
        }
        resolved = _resolve(project_root, path)
        if resolved is not None and resolved.is_file():
            entry["sha256"] = sha256_file(resolved)
        records.append(
            _normalize_record(
                entry, index, f"manifest:{label}", project_root, allow_pending_hash=True
            )
        )
    return records, []


def _finding_group(
    category: str,
    kind: str,
    reason: str,
    disposition: str,
    members: list[dict[str, Any]],
    sha256: str = "",
    source_id: str = "",
    pack_id: str = "",
    original_filename: str = "",
) -> dict[str, Any]:
    sorted_members = sorted(members, key=lambda m: (m.get("path") or "", m.get("record_id") or ""))
    canonical_ids = sorted({m.get("canonical_id") for m in members if m.get("canonical_id")})
    return {
        "category": category,
        "kind": kind,
        "reason": reason,
        "disposition": disposition,
        "not_fixed": True,
        "registered_only": True,
        "sha256": sha256,
        "source_id": source_id,
        "pack_id": pack_id,
        "original_filename": original_filename,
        "canonical_ids": canonical_ids,
        "members": sorted_members,
    }


def audit_records(
    project_root: Path,
    declared_records: Iterable[Any],
    manifest_paths: Iterable[Path] = (),
    declared_labels: Iterable[str] | None = None,
) -> dict[str, Any]:
    """Run the duplicate identity audit and return a deterministic result."""

    project_root = project_root.resolve()
    issues: list[dict[str, Any]] = []
    declared: list[dict[str, Any]] = []
    manifest_records: list[dict[str, Any]] = []
    declared_items = list(declared_records)
    if declared_labels is None:
        declared_labels = ["records"] * len(declared_items)
    labels = list(declared_labels)
    if len(labels) < len(declared_items):
        labels.extend(["records"] * (len(declared_items) - len(labels)))
    for index, (record, label) in enumerate(zip(declared_items, labels)):
        item = _normalize_record(record, index, str(label), project_root, allow_pending_hash=False)
        declared.append(item)
    for manifest_path in manifest_paths:
        items, manifest_issues = load_manifest_records(project_root, manifest_path)
        manifest_records.extend(items)
        issues.extend(manifest_issues)

    all_records = declared + manifest_records
    if not all_records:
        issues.append(_issue("no_records", "error", "no records were supplied; nothing was audited"))

    for item in all_records:
        if item["status"] == STATUS_ERROR:
            issues.append(
                _issue(item["code"], "error", item["reason"], record_id=item["record_id"], path=item["path"])
            )
        elif item["status"] == STATUS_PENDING:
            issues.append(
                _issue(item["code"], "pending", item["reason"], record_id=item["record_id"], path=item["path"])
            )

    valid = [item for item in all_records if item["status"] == STATUS_OK]
    pending = [item for item in all_records if item["status"] == STATUS_PENDING]
    error_items = [item for item in all_records if item["status"] == STATUS_ERROR]

    merged: list[dict[str, Any]] = []
    co_declared: list[dict[str, Any]] = []
    path_conflicts: list[dict[str, Any]] = []
    duplicate_records: list[dict[str, Any]] = []
    by_path: dict[str, list[dict[str, Any]]] = {}
    for item in valid:
        by_path.setdefault(item["path"], []).append(item)
    for path in sorted(by_path):
        group = by_path[path]
        hashes = {item["sha256"] for item in group}
        if len(hashes) > 1:
            path_conflicts.append(
                _finding_group(
                    CATEGORY_SUSPECTED,
                    "path_hash_conflict",
                    "the same repository path is declared with different sha256 values; one of the registrations conflicts with the others",
                    DISPOSITION_ADJUDICATE,
                    [_member(item) for item in group],
                )
            )
            merged.extend(dict(item) for item in group)
            continue
        digest = group[0]["sha256"]
        by_input: dict[str, list[dict[str, Any]]] = {}
        for item in group:
            by_input.setdefault(item["input"], []).append(item)
        if len(by_input) == 1 and len(group) > 1:
            duplicate_records.append(
                _finding_group(
                    CATEGORY_EXACT,
                    "duplicate_record",
                    "the exact same path and sha256 are declared more than once inside one input; the record itself is duplicated",
                    DISPOSITION_ADJUDICATE,
                    [_member(item) for item in group],
                    sha256=digest,
                    source_id=group[0].get("source_id") or "",
                    pack_id=group[0].get("pack_id") or "",
                    original_filename=group[0].get("original_filename") or "",
                )
            )
            merged.append(dict(group[0]))
            continue
        base = dict(group[0])
        co_inputs: list[str] = []
        for item in group[1:]:
            co_inputs.append(item["input"])
            for field in ("source_id", "pack_id", "canonical_id", "original_filename"):
                if not base.get(field) and item.get(field):
                    base[field] = item[field]
        base["co_declared"] = co_inputs
        base["_merged"] = True
        merged.append(base)
        if co_inputs:
            co_declared.append(
                {
                    "path": path,
                    "sha256": digest,
                    "record_id": base["record_id"],
                    "declarations": sorted([base["input"]] + co_inputs),
                }
            )

    for item in error_items:
        merged.append(item)
    merged.sort(key=lambda item: (item["path"], item["record_id"]))

    exact_duplicates: list[dict[str, Any]] = []
    suspected_duplicates: list[dict[str, Any]] = []
    legitimate_variants: list[dict[str, Any]] = []
    by_hash: dict[str, list[dict[str, Any]]] = {}
    for item in merged:
        if item["status"] == STATUS_OK and item.get("sha256"):
            by_hash.setdefault(item["sha256"], []).append(item)
    for digest in sorted(by_hash):
        group = by_hash[digest]
        if len(group) < 2:
            continue
        fingerprints = {
            (_normalize_key(item.get("source_id") or ""), _normalize_key(item.get("pack_id") or "")) for item in group
        }
        names = {_normalize_key(item.get("original_filename") or "") for item in group}
        if len(fingerprints) == 1:
            same_pack = next(iter(fingerprints))[1]
            if len(names) == 1:
                exact_duplicates.append(
                    _finding_group(
                        CATEGORY_EXACT,
                        "byte_identical_copy",
                        "identical content hash, same pack and same original filename declared under different paths or canonical ids; the same file is registered more than once",
                        DISPOSITION_ADJUDICATE,
                        [_member(item) for item in group],
                        sha256=digest,
                        source_id=group[0].get("source_id") or "",
                        pack_id=group[0].get("pack_id") or "",
                        original_filename=group[0].get("original_filename") or "",
                    )
                )
            else:
                suspected_duplicates.append(
                    _finding_group(
                        CATEGORY_SUSPECTED,
                        "renamed_copy",
                        "identical content hash inside the same pack but different original filenames; possibly a renamed copy, adjudication required",
                        DISPOSITION_ADJUDICATE,
                        [_member(item) for item in group],
                        sha256=digest,
                        source_id=group[0].get("source_id") or "",
                        pack_id=group[0].get("pack_id") or "",
                        original_filename="",
                    )
                )
        else:
            legitimate_variants.append(
                _finding_group(
                    CATEGORY_LEGITIMATE,
                    "legal_mirror",
                    "identical content hash declared by different pack or source registrations; a legal mirror is an observation and is never written as fixed",
                    DISPOSITION_OBSERVATION,
                    [_member(item) for item in group],
                    sha256=digest,
                    source_id="",
                    pack_id="",
                    original_filename="",
                )
            )

    by_canonical: dict[str, list[dict[str, Any]]] = {}
    for item in merged:
        canonical_id = item.get("canonical_id")
        if item["status"] == STATUS_OK and canonical_id:
            by_canonical.setdefault(_normalize_key(canonical_id), []).append(item)
    for key in sorted(by_canonical):
        group = by_canonical[key]
        if len({item["sha256"] for item in group}) > 1:
            suspected_duplicates.append(
                _finding_group(
                    CATEGORY_SUSPECTED,
                    "canonical_identity_collision",
                    f"the same canonical_id {key!r} is claimed by files with different content hashes; one canonical identity points at different files",
                    DISPOSITION_ADJUDICATE,
                    [_member(item) for item in group],
                    sha256="",
                )
            )

    by_named: dict[tuple[str, str, str], list[dict[str, Any]]] = {}
    for item in merged:
        if item["status"] != STATUS_OK or not item.get("sha256"):
            continue
        key = (
            _normalize_key(item.get("source_id") or ""),
            _normalize_key(item.get("pack_id") or ""),
            _normalize_key(item.get("original_filename") or ""),
        )
        by_named.setdefault(key, []).append(item)
    for (source_key, pack_key, name_key), group in sorted(by_named.items()):
        if len({item["sha256"] for item in group}) > 1:
            legitimate_variants.append(
                _finding_group(
                    CATEGORY_LEGITIMATE,
                    "named_variant",
                    "same source, same pack and same original filename but different content hashes; distinct content under one name is a legitimate variant, never a fixed duplicate",
                    DISPOSITION_OBSERVATION,
                    [_member(item) for item in group],
                    sha256="",
                    source_id=source_key,
                    pack_id=pack_key,
                    original_filename=name_key,
                )
            )

    exact_duplicates.sort(key=lambda g: (g["sha256"], g["pack_id"], g["original_filename"]))
    suspected_duplicates.sort(key=lambda g: (g["sha256"], g["pack_id"], g["kind"]))
    legitimate_variants.sort(key=lambda g: (g["kind"], g["sha256"]))
    co_declared.sort(key=lambda entry: entry["path"])
    duplicate_records.sort(key=lambda g: (g["sha256"], g["pack_id"]))
    path_conflicts.sort(key=lambda g: g["members"][0]["path"])

    all_exact = exact_duplicates + duplicate_records
    all_suspected = suspected_duplicates + path_conflicts
    error_count = sum(1 for issue in issues if issue.get("severity") == "error")
    pending_count = sum(1 for issue in issues if issue.get("severity") == "pending")
    if error_count > 0:
        status = "BLOCKED"
    elif all_exact or all_suspected:
        status = "DUPLICATES_REGISTERED"
    else:
        status = "CLEARED"

    code_counts = Counter(issue["code"] for issue in issues)
    output_records: list[dict[str, Any]] = []
    for item in merged:
        out = {
            "record_id": item["record_id"],
            "path": item["path"],
            "sha256": item.get("sha256"),
            "source_id": item.get("source_id"),
            "pack_id": item.get("pack_id"),
            "canonical_id": item.get("canonical_id"),
            "original_filename": item.get("original_filename"),
            "input": item.get("input"),
            "status": item["status"],
            "code": item.get("code", ""),
        }
        if item.get("co_declared"):
            out["co_declared"] = item["co_declared"]
        output_records.append(out)

    missing_files = [
        {
            "record_id": item["record_id"],
            "path": item["path"],
            "source_id": item.get("source_id"),
            "pack_id": item.get("pack_id"),
            "input": item.get("input"),
            "note": item["reason"],
        }
        for item in pending
    ]
    missing_files.sort(key=lambda entry: entry["path"])

    return {
        "tool": "tools/d18_duplicate_identity_audit.py",
        "task_id": "D18-L31",
        "read_only": True,
        "registrations_only": True,
        "fixed_count": 0,
        "files_deleted": 0,
        "files_renamed": 0,
        "files_moved": 0,
        "status": status,
        "record_count": len(all_records),
        "grouped_record_count": len(valid),
        "pending_count": pending_count,
        "error_count": error_count,
        "missing_file_count": len(missing_files),
        "counts": {
            CATEGORY_EXACT: len(all_exact),
            CATEGORY_SUSPECTED: len(all_suspected),
            CATEGORY_LEGITIMATE: len(legitimate_variants),
        },
        "exact_duplicates": all_exact,
        "suspected_duplicates": all_suspected,
        "legitimate_variants": legitimate_variants,
        "co_declared_paths": co_declared,
        "missing_files": missing_files,
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
        "records": output_records,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--records", type=Path, action="append", default=[])
    parser.add_argument("--manifests", action="append", default=[])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    declared: list[Any] = []
    input_issues: list[dict[str, Any]] = []
    for records_path in args.records:
        loaded, code, message = load_records(records_path)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(records_path)})
            continue
        declared.extend(loaded)
    manifest_paths: list[Path] = []
    for arg_value in args.manifests:
        found = collect_manifests(arg_value)
        if not found:
            input_issues.append({"code": "manifests_not_found", "severity": "error", "message": f"no manifests matched: {arg_value}"})
        manifest_paths.extend(found)
    declared_labels = [str(p).replace("\\", "/") for p in args.records]
    result = audit_records(project_root, declared, manifest_paths, declared_labels)
    if input_issues:
        result["input_issues"] = input_issues
        result["error_count"] += len(input_issues)
        result["status"] = "BLOCKED"
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"CLEARED": 0, "DUPLICATES_REGISTERED": 1, "BLOCKED": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
