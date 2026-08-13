"""D16-A05 final-asset gate audit.

The audit is deliberately conservative: any placeholder remains a blocking
asset replacement item, and no script changes its state.
"""

import argparse
import json
from collections import Counter
from pathlib import Path


TRACKED_PATH_FIELDS = (
    "resource_path",
    "runtime_prefab_path",
    "model_path",
    "audio_file_ref",
    "source_path",
)


def normalize_project_root(root: Path, assets_root: Path | None = None) -> Path:
    """Find the Unity project root for a command-line audit.

    Registry tooling is often invoked with a nested ``ContentRegistry`` path.
    That used to succeed while silently scanning only that subtree, producing a
    plausible but incomplete asset count.  Keep the pure ``audit`` API usable
    for focused fixtures, but make the CLI locate the nearest ancestor that
    contains both ``Assets`` and ``ProjectSettings``.
    """
    candidate = root.resolve()
    search_start = assets_root.resolve().parent if assets_root is not None else candidate
    candidates = [candidate]
    if search_start not in candidates:
        candidates.insert(0, search_start)
    candidates.extend(parent for parent in search_start.parents if parent not in candidates)
    for project_root in candidates:
        if (project_root / "Assets").is_dir() and (project_root / "ProjectSettings").is_dir():
            return project_root
    return root


def load_entries(root: Path):
    for path in sorted(root.rglob("*.json")):
        if path.name == "registry.json":
            continue
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if isinstance(value, dict) and "asset_state" in value:
            yield path, value


def resolve_declared_path(root: Path, value):
    """Resolve a project-relative path without treating a missing file as valid.

    Registry records use both ``game/Assets/...`` paths (repository-relative) and
    ``Assets/...`` paths (Unity-project-relative). Source references may carry a
    Markdown anchor, which is removed before checking the file on disk.
    """
    if not isinstance(value, str) or not value.strip():
        return None
    raw = value.split("#", 1)[0].strip().replace("\\", "/")
    if not raw:
        return None
    candidate = Path(raw)
    if candidate.is_absolute():
        return candidate
    return next(
        (resolved for resolved in (root.parent / candidate, root / candidate)
         if resolved.exists()),
        root.parent / candidate,
    )


def audit(root: Path, assets_root: Path):
    entries = list(load_entries(root))
    states = Counter(str(entry.get("asset_state")) for _, entry in entries)
    invalid = []
    missing_visual_metadata = []
    declared_path_counts = {
        field: {"present": 0, "exists": 0, "missing": 0}
        for field in TRACKED_PATH_FIELDS
    }
    missing_declared_paths = []
    for path, entry in entries:
        state = entry.get("asset_state")
        if state not in {"data_only", "placeholder", "integrated", "verified", "production"}:
            invalid.append(f"{entry.get('id')}:{state}")
        stage = str(entry.get("production_stage", "")).lower()
        if state == "placeholder" and stage in {"verified", "final", "release"}:
            invalid.append(f"{entry.get('id')}:placeholder_marked_{stage}")
        if state in {"integrated", "verified", "production"}:
            if not entry.get("lod_profile"):
                missing_visual_metadata.append(f"{entry.get('id')}:lod_profile")
            if not entry.get("test_cases"):
                missing_visual_metadata.append(f"{entry.get('id')}:test_cases")
        for field in TRACKED_PATH_FIELDS:
            value = entry.get(field)
            if not isinstance(value, str) or not value.strip():
                continue
            declared_path_counts[field]["present"] += 1
            resolved = resolve_declared_path(root, value)
            if resolved is not None and resolved.exists():
                declared_path_counts[field]["exists"] += 1
            else:
                declared_path_counts[field]["missing"] += 1
                missing_declared_paths.append({
                    "id": entry.get("id"),
                    "entry": str(path),
                    "field": field,
                    "value": value,
                })

    file_counts = Counter()
    for suffix in ("*.fbx", "*.obj", "*.blend", "*.png", "*.jpg", "*.wav", "*.mp3", "*.ogg"):
        file_counts[suffix] = sum(1 for _ in assets_root.rglob(suffix)) if assets_root.exists() else 0

    blockers = []
    if states.get("placeholder", 0):
        blockers.append("placeholder_assets_remain")
    if invalid:
        blockers.append("invalid_asset_state_metadata")
    if missing_visual_metadata:
        blockers.append("integrated_assets_missing_lod_or_tests")
    if missing_declared_paths:
        blockers.append("declared_asset_paths_missing")

    return {
        "task": "D16-A05",
        "status": "IN_PROGRESS",
        "registry_root": str(root),
        "assets_root": str(assets_root),
        "visual_entry_count": sum(1 for _, entry in entries if entry.get("asset_state") != "data_only"),
        "asset_state_counts": dict(sorted(states.items())),
        "invalid_metadata_count": len(invalid),
        "invalid_metadata": invalid[:100],
        "integrated_missing_visual_metadata_count": len(missing_visual_metadata),
        "integrated_missing_visual_metadata": missing_visual_metadata[:100],
        "declared_path_counts": declared_path_counts,
        "declared_path_missing_count": len(missing_declared_paths),
        "declared_path_missing": missing_declared_paths[:100],
        "source_file_counts": dict(sorted(file_counts.items())),
        "blocking_reasons": blockers,
        "note": "占位资源保持占位状态；本审计不把程序生成几何或共享材质升级为最终美术。",
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("assets_root", type=Path)
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()
    normalized_root = normalize_project_root(args.root, args.assets_root)
    result = audit(normalized_root, args.assets_root)
    output = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
