"""Preflight an explicit D16-A05 final-asset replacement packet.

This tool is intentionally read-only.  It validates the package that an
asset owner would hand to the registry owner; it never copies files, edits a
Content Registry entry, approves a review, or changes ``asset_state``.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any

from d16_a05_asset_audit import load_entries


APPROVED_REVIEW_STATES = {"Approved", "ApprovedWithWarning"}
REQUIRED_LICENSE_FIELDS = {
    "status",
    "holder",
    "reference",
    "commercial_use",
    "modification_allowed",
}
REQUIRED_AUTHORIZATION_FIELDS = {
    "authorization_id",
    "rights_holder",
    "license_ref",
    "source_url_or_path",
    "acquired_at",
    "commercial_use",
    "modification_allowed",
    "expiry_or_scope",
    "evidence_ref",
}


def _as_list(value: Any) -> list[Any]:
    return value if isinstance(value, list) else []


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _is_remote(value: str) -> bool:
    lowered = value.lower()
    return lowered.startswith(("http://", "https://", "git://", "git@"))


def _within(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def _resolve_local(project_root: Path, reference: Any) -> Path | None:
    """Resolve a repository-relative reference without allowing traversal."""
    if not isinstance(reference, str) or not reference.strip():
        return None
    clean = reference.split("#", 1)[0].strip().replace("\\", "/")
    if not clean or _is_remote(clean):
        return None
    candidate = Path(clean)
    if candidate.is_absolute():
        resolved = candidate.resolve()
    elif candidate.parts and candidate.parts[0].lower() == "game":
        resolved = (project_root / candidate).resolve()
    elif candidate.parts and candidate.parts[0].lower() == "assets":
        resolved = (project_root / "game" / candidate).resolve()
    else:
        resolved = (project_root / candidate).resolve()
    return resolved if _within(resolved, project_root) else None


def _issue(issues: list[dict[str, str]], code: str, message: str, record_id: str = "") -> None:
    item = {"code": code, "message": message}
    if record_id:
        item["id"] = record_id
    issues.append(item)


def _load_registry(project_root: Path) -> dict[str, tuple[Path, dict[str, Any]]]:
    registry_root = project_root / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"
    if not registry_root.is_dir():
        registry_root = project_root / "Assets" / "Resources" / "ContentRegistry" / "entries"
    return {
        str(entry.get("id")): (path, entry)
        for path, entry in load_entries(registry_root)
        if _text(entry.get("id"))
    }


def _validate_reference_files(
    project_root: Path,
    references: list[Any],
    issues: list[dict[str, str]],
    code: str,
    record_id: str,
    allow_remote: bool = False,
) -> None:
    for reference in references:
        value = _text(reference)
        if not value:
            _issue(issues, code, "reference must be a non-empty string", record_id)
            continue
        if allow_remote and _is_remote(value):
            continue
        resolved = _resolve_local(project_root, value)
        if resolved is None:
            _issue(issues, code, f"reference is not a safe local file path: {value}", record_id)
        elif not resolved.is_file():
            _issue(issues, code, f"referenced file does not exist: {value}", record_id)


def _validate_record(
    project_root: Path,
    registry: dict[str, tuple[Path, dict[str, Any]]],
    record: Any,
    issues: list[dict[str, str]],
    seen_ids: set[str],
) -> dict[str, Any] | None:
    if not isinstance(record, dict):
        _issue(issues, "invalid_replacement_record", "each replacement must be an object")
        return None

    asset_id = _text(record.get("id"))
    if not asset_id:
        _issue(issues, "missing_asset_id", "replacement id is required")
        return None
    if asset_id in seen_ids:
        _issue(issues, "duplicate_asset_id", "asset id appears more than once", asset_id)
        return None
    seen_ids.add(asset_id)

    current = registry.get(asset_id)
    if current is None:
        _issue(issues, "asset_id_not_found", "replacement id is not in Content Registry", asset_id)
        return None
    entry_path, entry = current
    if entry.get("asset_state") != "placeholder":
        _issue(issues, "asset_not_placeholder", "replacement target must still be placeholder", asset_id)

    final_resource = _text(record.get("final_resource_path"))
    final_prefab = _text(record.get("runtime_prefab_path"))
    resolved_files: dict[str, str] = {}
    for field, value in (("final_resource_path", final_resource), ("runtime_prefab_path", final_prefab)):
        resolved = _resolve_local(project_root, value)
        if resolved is None:
            _issue(issues, "unsafe_final_path", f"{field} is not a safe repository path", asset_id)
            continue
        normalized = resolved.as_posix().lower()
        if "/placeholder/" in normalized or "/ph_" in normalized or "placeholder" in resolved.name.lower():
            _issue(issues, "final_path_is_placeholder", f"{field} still points to a placeholder path", asset_id)
        if not resolved.is_file():
            _issue(issues, "final_file_missing", f"{field} does not exist: {value}", asset_id)
        else:
            resolved_files[field] = resolved.as_posix()

    source = record.get("source")
    if not isinstance(source, dict) or not _text(source.get("kind")) or not _text(source.get("reference")):
        _issue(issues, "source_provenance_incomplete", "source.kind and source.reference are required", asset_id)
    elif not _is_remote(_text(source.get("reference"))):
        _validate_reference_files(
            project_root, [source.get("reference")], issues,
            "source_reference_missing", asset_id, allow_remote=True,
        )

    license_record = record.get("license")
    if not isinstance(license_record, dict):
        _issue(issues, "license_record_missing", "license object is required", asset_id)
    else:
        missing_license = sorted(
            field for field in REQUIRED_LICENSE_FIELDS
            if field not in license_record or license_record[field] in (None, "")
        )
        if missing_license:
            _issue(issues, "license_record_incomplete", "missing license fields: " + ", ".join(missing_license), asset_id)
        if license_record.get("commercial_use") is not True:
            _issue(issues, "commercial_use_not_confirmed", "license.commercial_use must be true", asset_id)
        if license_record.get("modification_allowed") is not True:
            _issue(issues, "modification_not_confirmed", "license.modification_allowed must be true", asset_id)
        _validate_reference_files(
            project_root, [license_record.get("reference")], issues,
            "license_reference_missing", asset_id, allow_remote=True,
        )

    authorization = record.get("authorization")
    if not isinstance(authorization, dict):
        _issue(issues, "authorization_record_missing", "authorization object is required", asset_id)
    else:
        missing_auth = sorted(
            field for field in REQUIRED_AUTHORIZATION_FIELDS
            if field not in authorization or authorization[field] in (None, "")
        )
        if missing_auth:
            _issue(issues, "authorization_record_incomplete", "missing authorization fields: " + ", ".join(missing_auth), asset_id)
        for field in ("commercial_use", "modification_allowed"):
            if authorization.get(field) is not True:
                _issue(issues, f"authorization_{field}_not_confirmed", f"authorization.{field} must be true", asset_id)
        _validate_reference_files(
            project_root,
            [authorization.get("license_ref"), authorization.get("evidence_ref")],
            issues,
            "authorization_reference_missing",
            asset_id,
            allow_remote=True,
        )

    review = record.get("review")
    if not isinstance(review, dict) or review.get("decision") not in APPROVED_REVIEW_STATES:
        _issue(issues, "review_not_approved", "review.decision must be Approved or ApprovedWithWarning", asset_id)
    else:
        if not _text(review.get("reviewer")):
            _issue(issues, "reviewer_missing", "review.reviewer is required", asset_id)
        evidence_refs = _as_list(review.get("evidence_refs"))
        if not evidence_refs:
            _issue(issues, "review_evidence_missing", "review.evidence_refs must not be empty", asset_id)
        _validate_reference_files(
            project_root, evidence_refs, issues,
            "review_evidence_reference_missing", asset_id,
        )

    for field in ("lod_profile", "collision_ref"):
        if not _text(record.get(field)):
            _issue(issues, "visual_metadata_incomplete", f"{field} is required", asset_id)
    if not _as_list(record.get("anchor_refs")):
        _issue(issues, "visual_metadata_incomplete", "anchor_refs must not be empty", asset_id)
    if not _as_list(record.get("test_cases")):
        _issue(issues, "visual_tests_missing", "test_cases must not be empty", asset_id)

    return {
        "id": asset_id,
        "registry_entry": entry_path.as_posix(),
        "current_asset_state": entry.get("asset_state"),
        "final_resource_path": final_resource,
        "runtime_prefab_path": final_prefab,
        "resolved_files": resolved_files,
    }


def validate_manifest(project_root: Path, manifest: dict[str, Any]) -> dict[str, Any]:
    """Return a deterministic validation report without changing the project."""
    root = project_root.resolve()
    issues: list[dict[str, str]] = []
    if manifest.get("schema_version") != 1:
        _issue(issues, "unsupported_schema_version", "schema_version must be 1")
    if manifest.get("task") != "D16-A05-REPLACEMENT":
        _issue(issues, "invalid_task", "task must be D16-A05-REPLACEMENT")
    if not _text(manifest.get("operator")):
        _issue(issues, "operator_missing", "operator is required")
    if not _text(manifest.get("captured_at")):
        _issue(issues, "captured_at_missing", "captured_at is required")

    raw_artifacts = _as_list(manifest.get("raw_artifacts"))
    if not raw_artifacts:
        _issue(issues, "raw_artifacts_missing", "raw_artifacts must contain at least one local file")
    _validate_reference_files(root, raw_artifacts, issues, "raw_artifact_missing", "")

    registry = _load_registry(root)
    replacements = _as_list(manifest.get("replacements"))
    if not replacements:
        _issue(issues, "replacements_missing", "replacements must contain at least one record")
    seen_ids: set[str] = set()
    validated: list[dict[str, Any]] = []
    for record in replacements:
        result = _validate_record(root, registry, record, issues, seen_ids)
        if result is not None:
            validated.append(result)

    return {
        "task": "D16-A05-REPLACEMENT-PREFLIGHT",
        "status": "PASS" if not issues else "REJECTED",
        "can_apply": not issues and bool(validated),
        "project_root": root.as_posix(),
        "replacement_count": len(replacements),
        "validated_count": len(validated),
        "issues": issues,
        "validated_replacements": validated,
        "policy": "Read-only preflight; no files, registry entries, review decisions, or asset_state values are changed.",
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate a D16-A05 final-asset replacement packet")
    parser.add_argument("project_root", type=Path)
    parser.add_argument("manifest", type=Path)
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()
    try:
        manifest = json.loads(args.manifest.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as error:
        report = {
            "task": "D16-A05-REPLACEMENT-PREFLIGHT",
            "status": "REJECTED",
            "can_apply": False,
            "issues": [{"code": "manifest_unreadable", "message": str(error)}],
        }
        if args.json_out:
            args.json_out.parent.mkdir(parents=True, exist_ok=True)
            args.json_out.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(json.dumps(report, ensure_ascii=False, indent=2))
        return 2
    if not isinstance(manifest, dict):
        manifest = {}
    report = validate_manifest(args.project_root, manifest)
    output = json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0 if report["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
