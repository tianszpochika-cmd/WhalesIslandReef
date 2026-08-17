"""Read-only package-boundary validator for D18 asset packs (D18-L10).

Checks the containment and ownership boundaries of ``d18_asset_pack_manifest``
pack files frozen by D18-L01 (``docs/game/design/common/D18_LOGIC_GOVERNANCE_BASELINE.md``
section 6.2):

* every asset record must be traceable to exactly one pack: ``candidate_files``
  is the pack's declared file manifest, and records passed with ``--records``
  (e.g. the intake catalog ``eligible_entries``) must match the manifest they
  claim;
* every declared path must be a safe repository-relative path (no absolute,
  drive, UNC, parent traversal, URL, environment-variable or ignored-cache
  segments) resolving inside the project; path boundaries follow the frozen
  D18-L08 validator (``tools/d18_asset_path_validator.py``);
* candidate files must live under the frozen candidate root
  ``game/Assets/Art/Candidates`` and exist on disk;
* license evidence must be inside the pack: under the candidate root, inside
  the pack's own category subtree, a real non-empty file, not equal to a
  candidate file, not shared with another pack and not pointing at another
  pack's file;
* one file may not be declared by two packs (different sources are
  incompatible; even the same source must map to exactly one pack);
* a pack without a declared file list fails (missing manifest).

Only metadata and file manifests are processed: the tool parses JSON and stats
files for existence/size. It never reads, hashes, opens or imports candidate
binaries, never approves assets, never writes the registry or pack state and
never touches the network.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

from d18_asset_path_validator import normalize_path, validate_path  # D18-L08 frozen path boundaries


MANIFEST_KIND = "d18_asset_pack_manifest"
CANDIDATE_ROOT = "game/Assets/Art/Candidates"
_ROOT_DEPTH = len(CANDIDATE_ROOT.split("/"))
_URI_RE = re.compile(r"^[a-zA-Z][a-zA-Z0-9+.\-]*://")
_FILE_URI = "file:"


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _issue(
    issues: list[dict[str, Any]],
    code: str,
    message: str,
    *,
    manifest: str = "",
    pack_id: str = "",
    record_id: str = "",
    related_pack_ids: Iterable[str] = (),
    severity: str = "error",
) -> None:
    item: dict[str, Any] = {"code": code, "message": message, "severity": severity}
    if manifest:
        item["manifest"] = manifest
    if pack_id:
        item["pack_id"] = pack_id
    if record_id:
        item["record_id"] = record_id
    related = [value for value in related_pack_ids if value and value != pack_id]
    if related:
        item["related_pack_ids"] = sorted(set(related))
    issues.append(item)


def _outside_code(kind: str) -> str:
    return {
        "candidate": "candidate_file_outside_pack",
        "license_evidence": "license_evidence_outside_pack",
        "release_evidence": "release_evidence_outside_pack",
    }[kind]


def _safe_declared_path(
    project_root: Path,
    raw: Any,
    kind: str,
    field: str,
    issues: list[dict[str, Any]],
    manifest: str,
    pack_id: str,
) -> tuple[str, Path] | None:
    """Validate one declared path; returns (normalized, resolved) or None."""
    if not isinstance(raw, str) or not raw.strip():
        _issue(issues, "declared_path_unsafe", f"{field} must be a non-empty repository-relative path", manifest=manifest, pack_id=pack_id)
        return None
    stripped = raw.strip()
    if _URI_RE.match(stripped) or stripped.startswith(_FILE_URI):
        _issue(issues, "declared_path_unsafe", f"{field} is a URL, not an in-pack file: {raw}", manifest=manifest, pack_id=pack_id)
        return None
    path_result = validate_path(stripped, kind="candidate", field=field)
    for path_issue in path_result.issues:
        code = _outside_code(kind) if path_issue.code == "outside_allowed_root" else path_issue.code
        _issue(issues, code, f"{field}: {path_issue.value} — {path_issue.reason}", manifest=manifest, pack_id=pack_id)
    if not path_result.valid:
        return None
    normalized = path_result.normalized
    resolved = (project_root / normalized).resolve()
    if not _inside(resolved, project_root):
        _issue(issues, "declared_path_escape_project", f"{field} resolves outside the project: {raw}", manifest=manifest, pack_id=pack_id)
        return None
    return normalized, resolved


def _category_of(normalized: str) -> str:
    return normalized.split("/")[_ROOT_DEPTH]


def _validate_one_manifest(
    project_root: Path,
    manifest_path: Path,
    manifest: Any,
    issues: list[dict[str, Any]],
    declared: dict[str, list[dict[str, str]]],
    packs: dict[str, dict[str, Any]],
) -> None:
    name = str(manifest_path.relative_to(project_root)).replace("\\", "/") if _inside(manifest_path, project_root) else str(manifest_path)
    if not isinstance(manifest, dict):
        _issue(issues, "manifest_not_object", "manifest root must be an object", manifest=name)
        return

    pack_id = _text(manifest.get("pack_id"))
    source_id = _text(manifest.get("source_id"))
    if pack_id in packs:
        _issue(issues, "duplicate_pack_id", f"pack_id also appears in {packs[pack_id]['path']}", manifest=name, pack_id=pack_id)
        return
    pack_info: dict[str, Any] = {
        "path": name,
        "source_id": source_id,
        "candidate_keys": set(),
        "candidate_categories": set(),
        "evidence_key": None,
    }
    packs[pack_id] = pack_info

    candidate_files = manifest.get("candidate_files")
    candidates: list[Any] = []
    if not isinstance(candidate_files, list) or not candidate_files:
        _issue(issues, "manifest_without_file_list", "candidate_files must contain at least one declared file", manifest=name, pack_id=pack_id)
    else:
        candidates = candidate_files

    seen_within: set[str] = set()
    for index, item in enumerate(candidates):
        if not isinstance(item, dict):
            _issue(issues, "candidate_file_entry_invalid", f"candidate_files[{index}] must be an object with path, role and target_id", manifest=name, pack_id=pack_id)
            continue
        path_value = item.get("path")
        if not _text(path_value) or not _text(item.get("role")) or not _text(item.get("target_id")):
            _issue(issues, "candidate_file_entry_invalid", f"candidate_files[{index}] requires path, role and target_id", manifest=name, pack_id=pack_id)
            continue
        resolved_info = _safe_declared_path(project_root, path_value, "candidate", "candidate_files[].path", issues, name, pack_id)
        if resolved_info is None:
            continue
        normalized, resolved = resolved_info
        key = normalized.casefold()
        if key in seen_within:
            _issue(issues, "duplicate_file_in_manifest", f"file is declared twice in the same manifest: {path_value}", manifest=name, pack_id=pack_id)
        seen_within.add(key)
        if not resolved.is_file():
            _issue(issues, "candidate_file_missing", f"declared candidate file does not exist: {path_value}", manifest=name, pack_id=pack_id)
        pack_info["candidate_keys"].add(key)
        pack_info["candidate_categories"].add(_category_of(normalized))
        declared.setdefault(key, []).append({"kind": "candidate", "source_id": source_id, "pack_id": pack_id, "manifest": name})

    evidence_raw = manifest.get("license_evidence_path")
    if not _text(evidence_raw):
        _issue(issues, "license_evidence_missing", "license_evidence_path must point to an in-pack license evidence file", manifest=name, pack_id=pack_id)
    else:
        resolved_info = _safe_declared_path(project_root, evidence_raw, "license_evidence", "license_evidence_path", issues, name, pack_id)
        if resolved_info is not None:
            normalized, resolved = resolved_info
            evidence_key = normalized.casefold()
            pack_info["evidence_key"] = evidence_key
            if evidence_key in seen_within or evidence_key in pack_info["candidate_keys"]:
                _issue(issues, "evidence_collides_with_candidate", f"license evidence path equals a declared candidate file: {evidence_raw}", manifest=name, pack_id=pack_id)
            if not resolved.is_file():
                _issue(issues, "license_evidence_missing", "license evidence file does not exist", manifest=name, pack_id=pack_id)
            elif resolved.stat().st_size == 0:
                _issue(issues, "license_evidence_missing", "license evidence file is empty", manifest=name, pack_id=pack_id)
            if pack_info["candidate_categories"]:
                evidence_category = _category_of(normalized)
                candidate_categories = pack_info["candidate_categories"]
                if evidence_category.casefold() not in {category.casefold() for category in candidate_categories}:
                    _issue(
                        issues,
                        "evidence_category_mismatch",
                        f"license evidence category {evidence_category!r} differs from the pack's candidate category subtree {sorted(candidate_categories)}",
                        manifest=name,
                        pack_id=pack_id,
                    )
            declared.setdefault(evidence_key, []).append({"kind": "evidence", "source_id": source_id, "pack_id": pack_id, "manifest": name})

    if manifest.get("review_status") == "ReleaseApproved":
        release_raw = manifest.get("release_evidence_path")
        if not _text(release_raw):
            _issue(issues, "release_evidence_missing", "ReleaseApproved requires a release_evidence_path", manifest=name, pack_id=pack_id)
        else:
            resolved_info = _safe_declared_path(project_root, release_raw, "release_evidence", "release_evidence_path", issues, name, pack_id)
            if resolved_info is not None:
                normalized, resolved = resolved_info
                if not resolved.is_file():
                    _issue(issues, "release_evidence_missing", "release evidence file does not exist", manifest=name, pack_id=pack_id)
                key = normalized.casefold()
                if key in seen_within or key in pack_info["candidate_keys"]:
                    _issue(issues, "release_evidence_collides_with_candidate", f"release evidence path equals a declared candidate file: {release_raw}", manifest=name, pack_id=pack_id)
                declared.setdefault(key, []).append({"kind": "release_evidence", "source_id": source_id, "pack_id": pack_id, "manifest": name})


def _duplicate_issue_code(owners: list[dict[str, str]]) -> str:
    kinds = {owner["kind"] for owner in owners}
    if "release_evidence" in kinds:
        return "release_evidence_collides_with_other_pack_file"
    if "evidence" in kinds:
        if "candidate" in kinds:
            return "evidence_collides_with_other_pack_file"
        return "evidence_shared_across_packs"
    if len({owner["source_id"] for owner in owners}) > 1:
        return "file_declared_by_incompatible_sources"
    return "file_declared_by_multiple_packs"


def _emit_cross_pack_issues(declared: dict[str, list[dict[str, str]]], issues: list[dict[str, Any]]) -> None:
    for key in sorted(declared):
        owners = declared[key]
        if len(owners) < 2:
            continue
        last = owners[-1]
        code = _duplicate_issue_code(owners)
        summary = "; ".join(f"{owner['kind']} in {owner['pack_id']} ({owner['manifest']})" for owner in owners)
        _issue(
            issues,
            code,
            f"declared path is owned by more than one pack: {summary}",
            manifest=last["manifest"],
            pack_id=last["pack_id"],
            related_pack_ids=[owner["pack_id"] for owner in owners[:-1]],
        )


def _pack_summary(packs: dict[str, dict[str, Any]], issues: list[dict[str, Any]]) -> dict[str, Any]:
    failed_ids: set[str] = set()
    for issue in issues:
        if issue.get("pack_id"):
            failed_ids.add(issue["pack_id"])
        failed_ids.update(issue.get("related_pack_ids", []))
    ordered = sorted(packs)
    return {
        "total": len(packs),
        "passed": sum(1 for pack_id in ordered if pack_id not in failed_ids),
        "failed": len(failed_ids),
        "failed_pack_ids": sorted(failed_ids),
        "passing_pack_ids": [pack_id for pack_id in ordered if pack_id not in failed_ids],
    }


def _validate_records(
    records: list[Any],
    packs: dict[str, dict[str, Any]],
    issues: list[dict[str, Any]],
) -> dict[str, Any]:
    if not records:
        _issue(issues, "records_empty", "records must be a non-empty list (e.g. eligible_entries of the intake catalog)")
        return {"total": 0, "failed": 0, "passing": 0, "passed": False}
    total = len(records)
    failed_records = 0
    for index, record in enumerate(records):
        if not isinstance(record, dict):
            _issue(issues, "record_entry_invalid", f"records[{index}] must be an object", record_id=str(index))
            failed_records += 1
            continue
        intake_id = _text(record.get("intake_id")) or f"records[{index}]"
        record_failed = False
        manifest_id = _text(record.get("manifest_id"))
        pack = packs.get(manifest_id)
        if not manifest_id or pack is None:
            _issue(issues, "record_manifest_missing", f"record references an unknown or empty manifest_id: {manifest_id!r}", record_id=intake_id)
            record_failed = True
        else:
            manifest_path = record.get("manifest_path")
            if manifest_path and normalize_path(str(manifest_path)).casefold() != pack["path"].casefold():
                _issue(issues, "record_manifest_path_mismatch", f"manifest_path {manifest_path!r} does not match {pack['path']!r}", record_id=intake_id)
                record_failed = True
            file_path = record.get("candidate_file_path")
            if not _text(file_path):
                _issue(issues, "record_file_not_declared_by_pack", "record has no candidate_file_path", record_id=intake_id)
                record_failed = True
            elif normalize_path(str(file_path)).casefold() not in pack["candidate_keys"]:
                _issue(issues, "record_file_not_declared_by_pack", f"record file is not declared by pack {manifest_id}: {file_path}", record_id=intake_id)
                record_failed = True
            license_obj = record.get("license")
            lic_path = license_obj.get("license_evidence_path") if isinstance(license_obj, dict) else None
            if not _text(lic_path):
                _issue(issues, "record_evidence_missing", f"record license evidence is missing for pack {manifest_id}", record_id=intake_id)
                record_failed = True
            elif pack["evidence_key"] is not None and normalize_path(str(lic_path)).casefold() != pack["evidence_key"]:
                _issue(issues, "record_evidence_mismatch", f"record license evidence {lic_path!r} does not match pack {manifest_id} evidence {pack['evidence_key']!r}", record_id=intake_id)
                record_failed = True
        if record_failed:
            failed_records += 1
    return {"total": total, "failed": failed_records, "passing": total - failed_records, "passed": failed_records == 0}


def discover_manifest_files(manifest_root: Path | None, manifests: Iterable[Path]) -> list[Path]:
    paths: set[Path] = set()
    for path in manifests:
        if path.is_file():
            paths.add(path.resolve())
        elif path.is_dir():
            paths.update(item.resolve() for item in path.rglob("*.json"))
    if manifest_root is not None:
        if manifest_root.is_file():
            paths.add(manifest_root.resolve())
        elif manifest_root.is_dir():
            paths.update(item.resolve() for item in manifest_root.rglob("*.json"))
    return sorted(paths)


def validate_pack_boundaries(
    project_root: Path,
    manifest_paths: Iterable[Path],
    records: list[Any] | None = None,
) -> dict[str, Any]:
    """Validate pack boundaries and return a deterministic JSON result."""
    project_root = project_root.resolve()
    paths = sorted(Path(item).resolve() for item in manifest_paths)
    issues: list[dict[str, Any]] = []
    declared: dict[str, list[dict[str, str]]] = {}
    packs: dict[str, dict[str, Any]] = {}
    manifest_files: list[str] = []
    loaded = 0

    for path in paths:
        name = str(path.relative_to(project_root)).replace("\\", "/") if _inside(path, project_root) else str(path)
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except OSError as exc:
            _issue(issues, "manifest_read_error", str(exc), manifest=name)
            continue
        except json.JSONDecodeError as exc:
            _issue(issues, "manifest_json_error", f"line {exc.lineno}: {exc.msg}", manifest=name)
            continue
        if not isinstance(data, dict) or data.get("manifest_kind") != MANIFEST_KIND:
            continue
        loaded += 1
        manifest_files.append(name)
        _validate_one_manifest(project_root, path, data, issues, declared, packs)

    if loaded == 0:
        _issue(issues, "no_manifests", "no d18_asset_pack_manifest files were found")

    _emit_cross_pack_issues(declared, issues)

    record_summary = None
    if records is not None:
        record_summary = _validate_records(records, packs, issues)

    code_counts = Counter(issue["code"] for issue in issues)
    errors = sum(1 for issue in issues if issue.get("severity") == "error")
    result: dict[str, Any] = {
        "task": "D18-L10",
        "tool": "tools/d18_package_boundary_validator.py",
        "read_only": True,
        "registry_modified": False,
        "status": "PASS" if errors == 0 else "BLOCKED",
        "manifest_count": loaded,
        "manifest_files": manifest_files,
        "pack_summary": _pack_summary(packs, issues),
        "error_count": errors,
        "issue_count": len(issues),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
    }
    if record_summary is not None:
        result["record_count"] = record_summary["total"]
        result["record_summary"] = record_summary
    return result


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--manifest-root", type=Path, default=None)
    parser.add_argument("--manifest", type=Path, action="append", default=[])
    parser.add_argument("--records", type=Path, default=None, help="JSON file with a list or eligible_entries/records object")
    parser.add_argument("--output", type=Path, default=None, help="write the machine result JSON to this file")
    return parser


def main(argv: Iterable[str] | None = None) -> int:
    args = _build_parser().parse_args(list(argv) if argv is not None else None)
    project_root = args.project_root.resolve()
    records: list[Any] | None = None
    if args.records is not None:
        try:
            data = json.loads(args.records.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            print(f"[ERROR] records file unreadable: {args.records}: {exc}")
            return 2
        if isinstance(data, dict):
            records = data.get("eligible_entries") or data.get("records") or []
        elif isinstance(data, list):
            records = data
        else:
            print("[ERROR] records must be a JSON list or an object with eligible_entries/records")
            return 2
    paths = discover_manifest_files(args.manifest_root, args.manifest)
    result = validate_pack_boundaries(project_root, paths, records=records)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
