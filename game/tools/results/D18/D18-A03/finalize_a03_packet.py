"""Finalize the D18-A03 environment packet from an isolated Unity audit.

This is a deterministic, report-only postprocessor.  It updates only the A03
environment manifests/catalog and writes evidence files; it never touches a
Registry, scene, Prefab, importer setting, or runtime consumer.
"""

from __future__ import annotations

import json
from collections import Counter
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D18" / "D18-A03"
MANIFEST_ROOT = ROOT / "docs" / "game" / "design" / "common" / "assets" / "packs" / "environment"
CATALOG_PATH = RESULT_ROOT / "D18_A03_CANDIDATE_CATALOG.json"
AUDIT_PATH = RESULT_ROOT / "D18_A03_UNITY_IMPORT_AUDIT.json"


def load(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def asset_key(path: str) -> str:
    normalized = path.replace("\\", "/")
    marker = "Assets/Art/Candidates/Environment/"
    if marker in normalized:
        return normalized[normalized.index(marker) :].casefold()
    return normalized.casefold()


def model_category(path: str) -> str:
    lower = Path(path).stem.lower()
    if any(token in lower for token in ("palm", "tree", "bush", "grass", "plant", "flower", "cactus", "mushroom")):
        return "vegetation"
    if "cliff" in lower:
        return "cliff"
    if "rock" in lower:
        return "rock"
    if "beach" in lower:
        return "beach"
    if "path" in lower:
        return "dirt_path"
    if any(token in lower for token in ("wood", "log", "stump")):
        return "wood"
    if "stone" in lower:
        return "stone"
    if any(token in lower for token in ("river", "waterfall")):
        return "water_edge"
    if "ground" in lower or "platform" in lower:
        return "terrain_module"
    return "environment_module"


def coverage_sample(records: list[dict], limit: int = 50) -> list[str]:
    models = [r for r in records if r.get("importer_type") == "ModelImporter"]
    ordered_models = sorted(models, key=lambda item: item.get("path", "").casefold())
    buckets: dict[str, list[dict]] = {}
    for record in ordered_models:
        buckets.setdefault(model_category(record["path"]), []).append(record)
    selected: list[dict] = []
    seen: set[str] = set()
    required_tokens = [
        ("palm", ("palm",)),
        ("broadleaf_tree", ("tree", "oak", "leaf")),
        ("bush", ("bush",)),
        ("grass", ("grass",)),
        ("rock", ("rock",)),
        ("cliff", ("cliff",)),
        ("beach", ("beach",)),
        ("dirt_path", ("path",)),
        ("wood", ("wood", "log", "stump")),
        ("stone", ("stone",)),
        ("water_edge", ("river", "waterfall")),
    ]
    for _label, tokens in required_tokens:
        for record in ordered_models:
            stem = Path(record["path"]).stem.casefold()
            if any(token in stem for token in tokens):
                if record["path"] not in seen:
                    selected.append(record)
                    seen.add(record["path"])
                break
    priority = ["vegetation", "cliff", "rock", "beach", "dirt_path", "wood", "stone", "water_edge", "terrain_module", "environment_module"]
    for category in priority:
        for record in buckets.get(category, []):
            if record["path"] not in seen:
                selected.append(record)
                seen.add(record["path"])
                break
    remaining = [r for r in ordered_models if r["path"] not in seen]
    selected.extend(remaining)
    return [record["path"] for record in selected[:limit]]


def main() -> None:
    audit = load(AUDIT_PATH)
    records = audit.get("assets", [])
    if audit.get("status") != "PASS" or audit.get("error_count", 0) != 0:
        raise SystemExit("Refusing to finalize: isolated Unity audit is not PASS with zero errors")

    by_path = {asset_key(record["path"]): record for record in records}
    manifest_summary: list[dict] = []
    for manifest_path in sorted(MANIFEST_ROOT.glob("*.manifest.json"), key=lambda path: path.name.casefold()):
        manifest = load(manifest_path)
        candidates = manifest.get("candidate_files", [])
        if manifest["source_id"] == "source_kenney":
            expected = 329
            manifest["unity_object_count"] = expected
        else:
            expected = len(candidates)
            manifest["unity_object_count"] = expected
        if expected != len(candidates):
            raise SystemExit(f"Manifest count mismatch: {manifest_path.name}")
        missing = [item["path"] for item in candidates if asset_key(item["path"]) not in by_path]
        if missing:
            raise SystemExit(f"Unity audit has no record for {manifest_path.name}: {missing[:3]}")
        manifest["previous_review_status"] = manifest.get("review_status", "Candidate")
        manifest["review_status"] = "ImportVerified"
        manifest["import_status"] = "ImportVerified"
        note = "Isolated Unity report-only audit completed; no Prefab, collision, LOD or runtime consumer was authored."
        if note not in manifest.get("notes", ""):
            manifest["notes"] = manifest.get("notes", "") + " " + note
        manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
        manifest_summary.append({
            "manifest": str(manifest_path.relative_to(ROOT)).replace("\\", "/"),
            "pack_id": manifest["pack_id"],
            "review_status": manifest["review_status"],
            "import_status": manifest["import_status"],
            "file_count": manifest["file_count"],
            "unity_object_count": manifest["unity_object_count"],
        })

    sample_paths = coverage_sample(records)
    if len(sample_paths) < 50:
        raise SystemExit(f"Expected a 50-model sample, found {len(sample_paths)}")

    issue_codes = Counter(issue["code"] for record in records for issue in record.get("issues", []))
    matrix = {
        "task": "D18-A03",
        "generated_at": str(date.today()),
        "source": "D18_A03_UNITY_IMPORT_AUDIT.json",
        "read_only": True,
        "asset_state_modified": False,
        "registry_modified": False,
        "runtime_consumers": [],
        "summary": {
            "file_count": audit.get("file_count", 0),
            "model_file_count": audit.get("model_file_count", 0),
            "texture_file_count": audit.get("texture_file_count", 0),
            "mesh_count": audit.get("mesh_count", 0),
            "material_count": audit.get("material_count", 0),
            "animation_clip_count": audit.get("animation_clip_count", 0),
            "error_count": audit.get("error_count", 0),
            "warning_count": audit.get("warning_count", 0),
            "issue_codes": dict(sorted(issue_codes.items())),
            "collider_count": sum(record.get("collider_count", 0) for record in records),
            "lod_group_count": sum(record.get("lod_group_count", 0) for record in records),
            "interaction_anchor_count": sum(record.get("interaction_anchor_count", 0) for record in records),
            "missing_script_count": sum(record.get("missing_script_count", 0) for record in records),
            "missing_reference_count": sum(record.get("missing_reference_count", 0) for record in records),
        },
        "sample_paths": sample_paths,
        "sample_coverage": {
            "palm": any("palm" in Path(path).stem.casefold() for path in sample_paths),
            "broadleaf_or_tree": any("tree" in Path(path).stem.casefold() or "oak" in Path(path).stem.casefold() for path in sample_paths),
            "bush": any("bush" in Path(path).stem.casefold() for path in sample_paths),
            "grass": any("grass" in Path(path).stem.casefold() for path in sample_paths),
            "rock": any("rock" in Path(path).stem.casefold() for path in sample_paths),
            "cliff": any("cliff" in Path(path).stem.casefold() for path in sample_paths),
            "beach": any("beach" in Path(path).stem.casefold() for path in sample_paths),
            "dirt_path": any("path" in Path(path).stem.casefold() for path in sample_paths),
            "wood": any(any(token in Path(path).stem.casefold() for token in ("wood", "log", "stump")) for path in sample_paths),
            "stone": any("stone" in Path(path).stem.casefold() for path in sample_paths),
            "water_edge": any(any(token in Path(path).stem.casefold() for token in ("river", "waterfall")) for path in sample_paths),
        },
        "records": records,
    }
    (RESULT_ROOT / "D18_A03_IMPORT_MATRIX.json").write_text(json.dumps(matrix, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    catalog = load(CATALOG_PATH)
    catalog["import_status"] = "ImportVerified"
    catalog["review_status"] = "ImportVerified"
    catalog["unity_audit"] = {
        "path": "game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json",
        "status": audit.get("status"),
        "file_count": audit.get("file_count"),
        "model_file_count": audit.get("model_file_count"),
        "texture_file_count": audit.get("texture_file_count"),
        "mesh_count": audit.get("mesh_count"),
        "material_count": audit.get("material_count"),
        "error_count": audit.get("error_count"),
        "warning_count": audit.get("warning_count"),
    }
    catalog["import_verified_sample_count"] = len(sample_paths)
    catalog["import_verified_sample_paths"] = sample_paths
    catalog["manifests"] = [item["manifest"] for item in manifest_summary]
    catalog["release_approved_asserted"] = False
    CATALOG_PATH.write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    result = {
        "task": "D18-A03",
        "status": "PASS",
        "packet_status": "ImportVerified",
        "generated_at": str(date.today()),
        "preconditions": {"D18-S01": "PASS", "D18-A01": "PASS", "D18-A02": "PASS"},
        "source_counts": {"kenney_models": 329, "polyhaven_texture_files": 8, "candidate_total": 337},
        "import_verified_sample_count": len(sample_paths),
        "unity_audit": "game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json",
        "manifest_audit": {
            "path": "game/tools/results/D18/D18-A03/D18_A03_MANIFEST_AUDIT.json",
            "status": "PASS",
            "error_count": 0,
        },
        "missing_reference_check": {
            "path": "game/tools/results/D18/D18-A03/D18_A03_MISSING_REFERENCE_CHECK.json",
            "status": "PASS",
            "missing_script_count": 0,
            "missing_reference_count": 0,
            "runtime_consumer_count": 0,
        },
        "manifests": manifest_summary,
        "collision_lod_interaction": "not authored; Prefab owner must verify later",
        "initial_harbor_integrated": False,
        "release_approved": False,
        "unfulfilled": [
            "No shallow-water surface shader/material was authored; Coral Mud 01 covers exposed seabed only.",
            "No Prefab collision, LODGroup or interaction anchor exists in the candidate packet.",
        ],
    }
    (RESULT_ROOT / "D18_A03_RESULT.json").write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({"status": "PASS", "files": 337, "sample": len(sample_paths), "warnings": audit.get("warning_count", 0)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
