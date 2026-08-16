"""Read-only audit for D18 asset-pack manifests.

The tool checks provenance and admission evidence without approving assets,
editing the Content Registry, importing Unity files, or changing any state.
It deliberately returns ``BLOCKED`` for incomplete evidence so a missing hash
or broken local path cannot be silently treated as a candidate approval.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from collections import Counter
from datetime import date
from pathlib import Path
from typing import Any, Iterable


MANIFEST_KIND = "d18_asset_pack_manifest"
VALID_STATES = {
    "Candidate",
    "LicenseEvidenceComplete",
    "ImportVerified",
    "LocalDevelopmentReady",
    "ReleaseApproved",
    "Rejected",
}
VALID_IMPORT_STATES = {"NotImported", "ImportVerified", "ImportBlocked", "NotApplicable"}
ALLOWED_TRANSITIONS = {
    "Candidate": {"Candidate", "LicenseEvidenceComplete", "Rejected"},
    "LicenseEvidenceComplete": {"LicenseEvidenceComplete", "ImportVerified", "Rejected"},
    "ImportVerified": {"ImportVerified", "LocalDevelopmentReady", "Rejected"},
    "LocalDevelopmentReady": {"LocalDevelopmentReady", "ReleaseApproved", "Rejected"},
    "ReleaseApproved": {"ReleaseApproved", "Rejected"},
    "Rejected": {"Rejected", "Candidate"},
}
REQUIRED_FIELDS = (
    "manifest_kind",
    "schema_version",
    "source_id",
    "pack_id",
    "title",
    "author",
    "official_page",
    "download_url",
    "downloaded_at",
    "archive_name",
    "sha256",
    "license_id",
    "license_url",
    "license_evidence_path",
    "attribution_required",
    "commercial_use",
    "modification_allowed",
    "source_redistribution",
    "build_distribution",
    "category",
    "file_count",
    "unity_object_count",
    "review_status",
    "import_status",
    "candidate_files",
    "runtime_consumers",
    "notes",
)
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
ID_RE = re.compile(r"^pack_[a-z0-9_]+_v[0-9]+$")
SOURCE_RE = re.compile(r"^source_[a-z0-9_]+$")
URL_RE = re.compile(r"^https://[^\s]+$")
DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _issue(
    issues: list[dict[str, Any]],
    code: str,
    message: str,
    *,
    manifest: str = "",
    record_id: str = "",
    severity: str = "error",
) -> None:
    item: dict[str, Any] = {"code": code, "message": message, "severity": severity}
    if manifest:
        item["manifest"] = manifest
    if record_id:
        item["pack_id"] = record_id
    issues.append(item)


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def resolve_local_reference(project_root: Path, value: Any) -> Path | None:
    """Resolve a repository-relative path and reject traversal/absolute escape."""

    raw = _text(value).split("#", 1)[0].strip().replace("\\", "/")
    if not raw or raw.startswith(("http://", "https://", "file:")):
        return None

    candidate = Path(raw)
    if candidate.is_absolute():
        resolved = candidate.resolve()
    elif raw == "Assets" or raw.startswith("Assets/"):
        resolved = (project_root / "game" / candidate).resolve()
    else:
        resolved = (project_root / candidate).resolve()
    return resolved if _inside(resolved, project_root) else None


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


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


def _validate_date(value: Any, field: str, issues: list[dict[str, Any]], manifest: str, pack_id: str) -> None:
    if value is None and field == "downloaded_at":
        _issue(issues, "missing_download_date", "downloaded_at is null; the package has not been downloaded", manifest=manifest, record_id=pack_id)
        return
    if not isinstance(value, str) or not DATE_RE.fullmatch(value):
        _issue(issues, "invalid_date", f"{field} must be an ISO date", manifest=manifest, record_id=pack_id)
        return
    try:
        date.fromisoformat(value)
    except ValueError:
        _issue(issues, "invalid_date", f"{field} is not a calendar date", manifest=manifest, record_id=pack_id)


def _validate_manifest(
    project_root: Path,
    manifest_path: Path,
    manifest: Any,
    issues: list[dict[str, Any]],
) -> tuple[str, str, list[tuple[str, str]]]:
    name = str(manifest_path.relative_to(project_root)) if _inside(manifest_path, project_root) else str(manifest_path)
    if not isinstance(manifest, dict):
        _issue(issues, "manifest_not_object", "manifest root must be an object", manifest=name)
        return "", "", []

    pack_id = _text(manifest.get("pack_id"))
    source_id = _text(manifest.get("source_id"))
    for field in REQUIRED_FIELDS:
        if field not in manifest:
            _issue(issues, "missing_field", f"required field is missing: {field}", manifest=name, record_id=pack_id)

    if manifest.get("manifest_kind") != MANIFEST_KIND:
        _issue(issues, "invalid_manifest_kind", f"manifest_kind must be {MANIFEST_KIND}", manifest=name, record_id=pack_id)
    if manifest.get("schema_version") != 1:
        _issue(issues, "unsupported_schema_version", "schema_version must be 1", manifest=name, record_id=pack_id)
    if not SOURCE_RE.fullmatch(source_id):
        _issue(issues, "invalid_source_id", "source_id must match source_<lower_snake_case>", manifest=name, record_id=pack_id)
    if not ID_RE.fullmatch(pack_id):
        _issue(issues, "invalid_pack_id", "pack_id must match pack_<lower_snake_case>_v<number>", manifest=name, record_id=pack_id)

    for field in ("official_page", "download_url"):
        if not URL_RE.fullmatch(_text(manifest.get(field))):
            _issue(issues, "invalid_url", f"{field} must be an https URL", manifest=name, record_id=pack_id)
    if not isinstance(manifest.get("title"), str) or not _text(manifest.get("title")):
        _issue(issues, "missing_text", "title must be a non-empty string", manifest=name, record_id=pack_id)
    if not isinstance(manifest.get("author"), str) or not _text(manifest.get("author")):
        _issue(issues, "missing_text", "author must be a non-empty string", manifest=name, record_id=pack_id)
    if not isinstance(manifest.get("notes"), str) or not _text(manifest.get("notes")):
        _issue(issues, "missing_text", "notes must be a non-empty string", manifest=name, record_id=pack_id)

    _validate_date(manifest.get("downloaded_at"), "downloaded_at", issues, name, pack_id)
    archive_name = manifest.get("archive_name")
    if not isinstance(archive_name, str) or not _text(archive_name):
        _issue(issues, "missing_archive_name", "archive_name must be present after a download", manifest=name, record_id=pack_id)
    digest = manifest.get("sha256")
    if not isinstance(digest, str) or not SHA256_RE.fullmatch(digest):
        _issue(issues, "invalid_or_missing_sha256", "sha256 must be the real 64-hex archive digest", manifest=name, record_id=pack_id)
    elif archive_name and isinstance(archive_name, str):
        # The archive may intentionally live in an ignored/external cache.  If a
        # local copy is available, verify it, but do not require the cache copy.
        archive_path = project_root / "artifacts" / "external_sources" / archive_name
        if archive_path.is_file() and sha256_file(archive_path) != digest.upper():
            _issue(issues, "sha256_mismatch", f"local archive hash does not match manifest: {archive_name}", manifest=name, record_id=pack_id)

    license_id = _text(manifest.get("license_id")).upper()
    license_url = _text(manifest.get("license_url"))
    if not license_id:
        _issue(issues, "missing_license_id", "license_id is required", manifest=name, record_id=pack_id)
    if not re.fullmatch(r"https?://[^\s]+", license_url):
        _issue(issues, "invalid_license_url", "license_url must be an http(s) URL", manifest=name, record_id=pack_id)

    evidence = manifest.get("license_evidence_path")
    evidence_path = resolve_local_reference(project_root, evidence)
    if not _text(evidence):
        _issue(issues, "missing_license_evidence", "license_evidence_path must point to local evidence", manifest=name, record_id=pack_id)
    elif evidence_path is None:
        _issue(issues, "license_evidence_path_escape", "license evidence path escapes the project or is remote", manifest=name, record_id=pack_id)
    elif not evidence_path.is_file() or evidence_path.stat().st_size == 0:
        _issue(issues, "license_evidence_missing", "license evidence file does not exist or is empty", manifest=name, record_id=pack_id)

    for field in ("attribution_required", "commercial_use", "modification_allowed"):
        if not isinstance(manifest.get(field), bool):
            _issue(issues, "invalid_license_field", f"{field} must be boolean", manifest=name, record_id=pack_id)
    for field in ("source_redistribution", "build_distribution"):
        if manifest.get(field) not in {"allowed", "not_allowed", "pending", "not_applicable"}:
            _issue(issues, "invalid_distribution_field", f"{field} has an unsupported value", manifest=name, record_id=pack_id)

    category = manifest.get("category")
    if not isinstance(category, list) or not category or not all(isinstance(item, str) and _text(item) for item in category):
        _issue(issues, "invalid_category", "category must be a non-empty string array", manifest=name, record_id=pack_id)
    for field in ("file_count", "unity_object_count"):
        value = manifest.get(field)
        if value is not None and (not isinstance(value, int) or isinstance(value, bool) or value < 0):
            _issue(issues, "invalid_count", f"{field} must be a non-negative integer or null", manifest=name, record_id=pack_id)
        if value is None:
            _issue(issues, "missing_count", f"{field} is null; do not claim an unmeasured count", manifest=name, record_id=pack_id)

    review_status = _text(manifest.get("review_status"))
    previous_status = _text(manifest.get("previous_review_status"))
    import_status = _text(manifest.get("import_status"))
    if review_status not in VALID_STATES:
        _issue(issues, "invalid_review_status", "review_status is not one of the six frozen states", manifest=name, record_id=pack_id)
    if import_status not in VALID_IMPORT_STATES:
        _issue(issues, "invalid_import_status", "import_status is not a frozen import state", manifest=name, record_id=pack_id)
    if not previous_status and review_status != "Candidate":
        _issue(issues, "illegal_initial_state", "a new manifest may only start as Candidate", manifest=name, record_id=pack_id)
    elif previous_status and (previous_status not in VALID_STATES or review_status not in ALLOWED_TRANSITIONS.get(previous_status, set())):
        _issue(issues, "illegal_state_transition", f"{previous_status} -> {review_status} is not allowed", manifest=name, record_id=pack_id)
    if review_status in {"ImportVerified", "LocalDevelopmentReady", "ReleaseApproved"} and import_status != "ImportVerified":
        _issue(issues, "state_import_mismatch", "an import-gated review state requires ImportVerified", manifest=name, record_id=pack_id)

    human_reviewer = _text(manifest.get("human_reviewer"))
    release_evidence = resolve_local_reference(project_root, manifest.get("release_evidence_path"))
    if review_status == "ReleaseApproved":
        if not human_reviewer:
            _issue(issues, "missing_human_reviewer", "ReleaseApproved requires a responsible human reviewer", manifest=name, record_id=pack_id)
        _validate_date(manifest.get("human_review_date"), "human_review_date", issues, name, pack_id)
        if release_evidence is None or not release_evidence.is_file():
            _issue(issues, "missing_release_evidence", "ReleaseApproved requires a local human review evidence path", manifest=name, record_id=pack_id)

    # Automatic baseline is intentionally narrow.  A non-CC0 record can only
    # be carried as Candidate, or be explicitly marked by a human ReleaseApproved
    # record that the tool merely validates and never creates.
    if license_id not in {"CC0", "CC0-1.0"} and review_status not in {"Candidate", "Rejected"}:
        _issue(issues, "non_cc0_human_only", "non-CC0 licenses remain blocked unless an existing human ReleaseApproved record is present", manifest=name, record_id=pack_id)
    if license_id in {"CC0", "CC0-1.0"} and review_status not in {"Candidate", "Rejected"}:
        if manifest.get("commercial_use") is not True or manifest.get("modification_allowed") is not True:
            _issue(issues, "cc0_rights_not_complete", "CC0 admission requires explicit commercial_use and modification_allowed true", manifest=name, record_id=pack_id)
        if manifest.get("source_redistribution") != "allowed" or manifest.get("build_distribution") != "allowed":
            _issue(issues, "cc0_distribution_not_complete", "CC0 admission requires source/build distribution fields to be allowed", manifest=name, record_id=pack_id)

    candidate_files = manifest.get("candidate_files")
    file_keys: list[tuple[str, str]] = []
    if not isinstance(candidate_files, list) or not candidate_files:
        _issue(issues, "missing_candidate_files", "candidate_files must contain at least one enumerated file", manifest=name, record_id=pack_id)
    else:
        for index, item in enumerate(candidate_files):
            if not isinstance(item, dict):
                _issue(issues, "invalid_candidate_file", f"candidate_files[{index}] must be an object", manifest=name, record_id=pack_id)
                continue
            path_value = _text(item.get("path"))
            target_id = _text(item.get("target_id"))
            if not path_value or not _text(item.get("role")) or not target_id:
                _issue(issues, "missing_candidate_file_field", f"candidate_files[{index}] requires path, role and target_id", manifest=name, record_id=pack_id)
                continue
            resolved = resolve_local_reference(project_root, path_value)
            if resolved is None:
                _issue(issues, "candidate_path_escape", f"candidate file path is unsafe: {path_value}", manifest=name, record_id=pack_id)
            elif not resolved.is_file():
                _issue(issues, "candidate_file_missing", f"candidate file does not exist: {path_value}", manifest=name, record_id=pack_id)
            normalized = path_value.replace("\\", "/").casefold()
            file_keys.append((normalized, source_id + ":" + pack_id))

    consumers = manifest.get("runtime_consumers")
    if not isinstance(consumers, list) or not all(isinstance(item, str) and _text(item) for item in consumers):
        _issue(issues, "invalid_runtime_consumers", "runtime_consumers must be a string array", manifest=name, record_id=pack_id)
        consumers = []
    if review_status not in {"ReleaseApproved", "Rejected"} and consumers:
        _issue(issues, "unapproved_runtime_reference", "a non-approved manifest must not declare runtime consumers", manifest=name, record_id=pack_id)
    for consumer in consumers:
        consumer_path = resolve_local_reference(project_root, consumer)
        if consumer_path is None:
            _issue(issues, "runtime_consumer_path_escape", f"runtime consumer path is unsafe: {consumer}", manifest=name, record_id=pack_id)
        elif not consumer_path.is_file():
            _issue(issues, "runtime_consumer_missing", f"runtime consumer does not exist: {consumer}", manifest=name, record_id=pack_id)

    return source_id, pack_id, file_keys


def audit_manifest_files(project_root: Path, manifest_paths: Iterable[Path]) -> dict[str, Any]:
    """Audit manifests and return a deterministic JSON-compatible result."""

    project_root = project_root.resolve()
    paths = sorted(Path(item).resolve() for item in manifest_paths)
    issues: list[dict[str, Any]] = []
    seen_pack_ids: dict[str, Path] = {}
    seen_source_ids: dict[str, Path] = {}
    seen_files: dict[str, tuple[str, str, Path]] = {}
    loaded_count = 0

    for path in paths:
        name = str(path.relative_to(project_root)) if _inside(path, project_root) else str(path)
        try:
            manifest = json.loads(path.read_text(encoding="utf-8"))
        except OSError as exc:
            _issue(issues, "manifest_read_error", str(exc), manifest=name)
            continue
        except json.JSONDecodeError as exc:
            _issue(issues, "manifest_json_error", f"line {exc.lineno}: {exc.msg}", manifest=name)
            continue
        if not isinstance(manifest, dict) or manifest.get("manifest_kind") != MANIFEST_KIND:
            # A manifest root may contain result/sidecar JSON.  They are not
            # silently treated as package manifests.
            continue
        loaded_count += 1
        source_id, pack_id, files = _validate_manifest(project_root, path, manifest, issues)
        if pack_id in seen_pack_ids:
            _issue(issues, "duplicate_pack_id", f"pack_id also appears in {seen_pack_ids[pack_id]}", manifest=name, record_id=pack_id)
        elif pack_id:
            seen_pack_ids[pack_id] = path
        if source_id in seen_source_ids:
            # Multiple packages from one source are normal; this map exists to
            # catch malformed empty/unstable source IDs only through duplicates
            # in the result, so do not report a blocker for a normal source.
            pass
        elif source_id:
            seen_source_ids[source_id] = path
        for normalized_path, owner in files:
            previous = seen_files.get(normalized_path)
            if previous and previous[1] != owner:
                _issue(
                    issues,
                    "same_file_multiple_sources",
                    f"candidate path also belongs to {previous[1]} in {previous[2]}",
                    manifest=name,
                    record_id=pack_id,
                )
            elif not previous:
                seen_files[normalized_path] = (normalized_path, owner, path)

    if loaded_count == 0:
        _issue(issues, "no_manifests", "no d18_asset_pack_manifest files were found")
    code_counts = Counter(issue["code"] for issue in issues)
    errors = sum(1 for issue in issues if issue.get("severity") == "error")
    return {
        "task": "D18-A01",
        "tool": "tools/d18_asset_manifest_audit.py",
        "read_only": True,
        "registry_modified": False,
        "status": "PASS" if errors == 0 else "BLOCKED",
        "manifest_count": loaded_count,
        "manifest_files": [str(path.relative_to(project_root)) if _inside(path, project_root) else str(path) for path in paths],
        "error_count": errors,
        "issue_count": len(issues),
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--manifest-root", type=Path, default=None)
    parser.add_argument("--manifest", type=Path, action="append", default=[])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    project_root = args.project_root.resolve()
    root = args.manifest_root.resolve() if args.manifest_root else None
    paths = discover_manifest_files(root, (item.resolve() for item in args.manifest))
    result = audit_manifest_files(project_root, paths)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
