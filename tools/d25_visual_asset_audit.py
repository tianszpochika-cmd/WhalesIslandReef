"""Read-only visual asset and prefab admission audit for D25-P05.

The audit intentionally stays below the runtime-approval boundary.  It checks
that a visual catalog points at repository files and that the referenced
prefabs contain the components required by their declared role.  It never
changes the Content Registry, asset state, scenes, prefabs, or import settings.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any


CATALOG_KIND = "d25_visual_asset_catalog"
VALID_SOURCE_STATES = {"Candidate", "IntakeEligible", "InternalAuthored"}
VALID_REQUIREMENTS = {"mesh", "material", "collider", "lod", "animator", "anchor"}
ID_RE = re.compile(r"^d25_[a-z0-9_]+$")


def _text(value: Any) -> str:
    return value.strip() if isinstance(value, str) else ""


def _inside(path: Path, root: Path) -> bool:
    try:
        path.resolve().relative_to(root.resolve())
        return True
    except ValueError:
        return False


def resolve_repo_path(project_root: Path, value: Any) -> Path | None:
    """Resolve only repository-relative paths; reject absolute/traversal paths."""

    raw = _text(value).replace("\\", "/")
    if not raw or raw.startswith(("http://", "https://", "file:")):
        return None
    candidate = Path(raw)
    if candidate.is_absolute():
        return None
    resolved = (project_root / candidate).resolve()
    return resolved if _inside(resolved, project_root) else None


def _issue(issues: list[dict[str, Any]], code: str, message: str, *, asset_id: str = "") -> None:
    item: dict[str, Any] = {"code": code, "message": message, "severity": "error"}
    if asset_id:
        item["asset_id"] = asset_id
    issues.append(item)


def _has_component(text: str, component: str) -> bool:
    return re.search(r"(?:^|\n)--- !u!\d+ .*\n(?:.*\n){0,3}" + re.escape(component) + r":", text) is not None or (
        re.search(r"(?:^|\n)" + re.escape(component) + r":", text) is not None
    )


def inspect_prefab(path: Path) -> dict[str, Any]:
    """Return deterministic component evidence from a Unity YAML prefab."""

    try:
        text = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        return {"readable": False, "error": str(exc), "components": {}}

    # These checks are deliberately conservative.  A positive result means
    # the prefab declares the component; a negative result does not infer a
    # missing runtime consumer or release authorization.
    components = {
        "mesh": bool(re.search(r"(?:MeshFilter|SkinnedMeshRenderer):|m_Mesh:\s*\{fileID:\s*(?!0\b)", text)),
        "material": bool(re.search(r"m_Materials:\s*\n(?:\s*-\s*)?\{fileID:\s*(?!0\b)", text)),
        "collider": bool(re.search(r"(?:BoxCollider|CapsuleCollider|MeshCollider|SphereCollider|CharacterController):", text)),
        "lod": bool(re.search(r"(?:LODGroup|m_LODs):", text)),
        "animator": bool(re.search(r"(?:Animator|Animation):", text)),
        "anchor": bool(re.search(r"(?:^|\n)\s*m_Name:\s*anchor_[a-z0-9_]+\s*$", text, re.IGNORECASE | re.MULTILINE)),
    }
    return {
        "readable": True,
        "bytes": len(text.encode("utf-8")),
        "components": components,
    }


def audit_catalog(project_root: Path, catalog_path: Path) -> dict[str, Any]:
    project_root = project_root.resolve()
    catalog_path = catalog_path.resolve()
    issues: list[dict[str, Any]] = []
    entries_out: list[dict[str, Any]] = []

    if not _inside(catalog_path, project_root):
        _issue(issues, "catalog_path_escape", "catalog must be inside the repository")
        return _result(catalog_path, entries_out, issues)
    try:
        catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    except (OSError, UnicodeDecodeError, json.JSONDecodeError) as exc:
        _issue(issues, "catalog_read_error", str(exc))
        return _result(catalog_path, entries_out, issues)

    if not isinstance(catalog, dict):
        _issue(issues, "catalog_not_object", "catalog root must be an object")
        return _result(catalog_path, entries_out, issues)
    if catalog.get("catalog_kind") != CATALOG_KIND:
        _issue(issues, "invalid_catalog_kind", f"catalog_kind must be {CATALOG_KIND}")
    if catalog.get("schema_version") != 1:
        _issue(issues, "unsupported_schema_version", "schema_version must be 1")

    entries = catalog.get("entries")
    if not isinstance(entries, list) or not entries:
        _issue(issues, "missing_entries", "entries must be a non-empty array")
        return _result(catalog_path, entries_out, issues)

    seen_ids: set[str] = set()
    seen_paths: dict[str, str] = {}
    for index, entry in enumerate(entries):
        if not isinstance(entry, dict):
            _issue(issues, "invalid_entry", f"entries[{index}] must be an object")
            continue
        asset_id = _text(entry.get("asset_id"))
        if not ID_RE.fullmatch(asset_id):
            _issue(issues, "invalid_asset_id", "asset_id must match d25_<lower_snake_case>", asset_id=asset_id)
        if asset_id in seen_ids:
            _issue(issues, "duplicate_asset_id", "asset_id appears more than once", asset_id=asset_id)
        seen_ids.add(asset_id)

        source_state = _text(entry.get("source_state"))
        if source_state not in VALID_SOURCE_STATES:
            _issue(issues, "invalid_source_state", "source_state is not an allowed non-release candidate state", asset_id=asset_id)

        prefab_value = _text(entry.get("prefab_path"))
        prefab = resolve_repo_path(project_root, prefab_value)
        if prefab is None:
            _issue(issues, "unsafe_prefab_path", "prefab_path must be a repository-relative path", asset_id=asset_id)
        elif not prefab.is_file():
            _issue(issues, "prefab_missing", f"prefab does not exist: {prefab_value}", asset_id=asset_id)
        else:
            normalized = prefab.as_posix().casefold()
            previous = seen_paths.get(normalized)
            if previous and previous != asset_id:
                _issue(issues, "duplicate_prefab_path", f"prefab is already owned by {previous}", asset_id=asset_id)
            seen_paths[normalized] = asset_id

        evidence_value = _text(entry.get("license_evidence_path"))
        evidence = resolve_repo_path(project_root, evidence_value)
        if evidence is None or not evidence.is_file() or evidence.stat().st_size == 0:
            _issue(issues, "license_evidence_missing", "license_evidence_path must point to a non-empty local file", asset_id=asset_id)

        requirements = entry.get("required_components")
        if not isinstance(requirements, list) or not requirements:
            _issue(issues, "missing_required_components", "required_components must be a non-empty array", asset_id=asset_id)
            requirements = []
        invalid_requirements = [item for item in requirements if item not in VALID_REQUIREMENTS]
        for requirement in invalid_requirements:
            _issue(issues, "invalid_component_requirement", f"unsupported component requirement: {requirement}", asset_id=asset_id)

        prefab_evidence = inspect_prefab(prefab) if prefab and prefab.is_file() else {"readable": False, "components": {}}
        if not prefab_evidence.get("readable", False) and prefab is not None and prefab.is_file():
            _issue(issues, "prefab_unreadable", "prefab could not be read as UTF-8 Unity YAML", asset_id=asset_id)
        components = prefab_evidence.get("components", {})
        for requirement in requirements:
            if requirement in VALID_REQUIREMENTS and not components.get(requirement, False):
                _issue(issues, "required_component_missing", f"prefab does not declare required component: {requirement}", asset_id=asset_id)

        entries_out.append(
            {
                "asset_id": asset_id,
                "prefab_path": prefab_value,
                "source_state": source_state,
                "required_components": requirements,
                "prefab_evidence": prefab_evidence,
            }
        )

    return _result(catalog_path, entries_out, issues)


def _result(catalog_path: Path, entries: list[dict[str, Any]], issues: list[dict[str, Any]]) -> dict[str, Any]:
    counts = Counter(item["code"] for item in issues)
    return {
        "task_id": "D25-P05",
        "tool": "tools/d25_visual_asset_audit.py",
        "read_only": True,
        "registry_modified": False,
        "catalog": str(catalog_path),
        "entry_count": len(entries),
        "error_count": len(issues),
        "issue_counts": dict(sorted(counts.items())),
        "status": "PASS" if not issues else "BLOCKED",
        "entries": entries,
        "issues": issues,
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--catalog", type=Path, default=None)
    parser.add_argument("--output", type=Path, default=None)
    args = parser.parse_args(argv)
    project_root = args.project_root.resolve()
    catalog = (args.catalog or project_root / "docs/game/design/common/assets/d25_visual_asset_catalog.json").resolve()
    result = audit_catalog(project_root, catalog)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
