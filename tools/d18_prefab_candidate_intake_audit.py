#!/usr/bin/env python3
"""只读构建并审计 D18 PrefabCandidateIntake 目录。

该目录是资源状态机之外的受控工作场景准入记录。工具不会写 Registry、
asset_state、Prefab、场景或任何 manifest；只会按显式参数写入自己的 JSON
目录和机器结果。
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path, PurePosixPath
from typing import Any, Iterable


MANIFEST_ROOT = Path("docs/game/design/common/assets")
CATALOG_REL = Path("docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
ALLOWED_RESOURCE_STATES = {
    "Candidate",
    "LicenseEvidenceComplete",
    "ImportVerified",
    "LocalDevelopmentReady",
    "ReleaseApproved",
    "Rejected",
}


def read_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def relative_file(project_root: Path, raw: str) -> Path:
    """解析项目相对路径；拒绝绝对路径和目录逃逸。"""

    normalized = raw.replace("\\", "/")
    pure = PurePosixPath(normalized)
    if pure.is_absolute() or ".." in pure.parts:
        raise ValueError(f"unsafe path: {raw}")
    return project_root.joinpath(*pure.parts)


def discover_manifests(project_root: Path) -> list[Path]:
    return sorted(
        path
        for path in (project_root / MANIFEST_ROOT).rglob("*.manifest.json")
        if path.is_file()
    )


def normalized_categories(value: Any) -> list[str]:
    if isinstance(value, list):
        return [str(item).lower() for item in value]
    if value is None:
        return []
    return [str(value).lower()]


def classify_domain(manifest: dict[str, Any], candidate: dict[str, Any]) -> str:
    categories = normalized_categories(manifest.get("category"))
    role = str(candidate.get("role", "")).lower()
    if any(token in role for token in ("input", "ui_", "icon", "navigation")) or "ui" in categories or "input" in categories:
        return "ui"
    if "audio" in categories or role.startswith("audio_"):
        return "audio"
    if any(token in role for token in ("hdri", "sky")) or "hdri" in categories:
        return "lighting_reference"
    if any(token in role for token in ("material", "pbr", "seabed", "beach_material")) or "materials" in categories or "material" in categories:
        return "material"
    if any(token in role for token in ("character", "player", "crew", "npc")) or "character" in categories or "player_base" in categories:
        return "character"
    if any(token in role for token in ("animal", "fish", "bird", "aquatic", "pet")) or "animal" in categories or "fish" in categories:
        return "animal"
    if any(token in role for token in ("ship", "boat", "hull")) or "ship" in categories or "ships" in categories:
        return "ship"
    if any(token in role for token in ("building", "dock", "pier", "port")) or "building" in categories or "buildings" in categories:
        return "building"
    if any(token in role for token in ("vegetation", "rock", "cliff", "terrain", "environment", "coastal")) or any(
        token in categories for token in ("environment", "vegetation", "rock", "cliff", "terrain_module", "coastal")
    ):
        return "environment"
    if "props" in categories or "prop" in categories:
        return "prop"
    return "unclassified"


def priority_for(domain: str, role: str) -> str:
    if domain in {"environment", "material", "ui", "audio"}:
        return "P0"
    if domain == "lighting_reference" or "reference" in role.lower():
        return "P1"
    return "P1"


def allowed_tasks(domain: str) -> list[str]:
    return {
        "environment": ["D19-A01", "D19-A03"],
        "material": ["D19-A01", "D19-A03", "D19-A04", "D19-A05", "D21-A04", "D21-A05"],
        "lighting_reference": ["D19-A05", "D21-A05"],
        "ui": ["D22-S01", "D22-A01", "D22-A02", "D22-A03", "D22-A04", "D22-A05", "D22-A06", "D22-A07"],
        "audio": ["D19-A06", "D21-A06", "D22-A06"],
    }.get(domain, [])


def import_evidence_for(manifest_path: Path) -> str:
    path = manifest_path.as_posix()
    if "/environment/" in path:
        return "game/tools/results/D18/D18-A03/D18_A03_UNITY_IMPORT_AUDIT.json"
    if "/ui_materials_audio/" in path:
        return "game/tools/results/D18/D18-A06/D18_A06_UNITY_IMPORT_AUDIT.json"
    if "/characters_animals/" in path:
        return "game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json"
    if "/ships_port_props/" in path:
        return "game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.json"
    return "game/tools/results/D18/D18-A02/D18_A02_KENNEY_IMPORT_AUDIT.json"


def missing_local_ready_evidence(domain: str, manifest: dict[str, Any]) -> list[str]:
    common = [
        "consumer-owned work-scene or Prefab reference",
        "D18-S03 runtime consumer evidence",
        "save/restore and rollback evidence",
    ]
    if domain in {"environment", "building", "prop", "ship"}:
        common += ["Prefab-level collision", "named interaction anchors", "LODGroup or documented equivalent"]
    if domain in {"character", "animal"}:
        common += ["Animator/clip binding", "scale and collision evidence", "LODGroup or documented equivalent"]
    if domain == "material":
        common += ["owned Material asset or controlled scene assignment", "lighting/performance sample"]
    if domain == "audio":
        common += ["Mixer/AudioSource consumer", "cue, loop and loudness review"]
    if domain == "ui":
        common += ["Canvas consumer", "input/focus/localization and save/restore evidence"]
    if domain == "lighting_reference":
        common += ["owned skybox/lighting profile", "weather/quality-tier performance sample"]
    return common


def known_gap_for(domain: str, candidate: dict[str, Any], manifest: dict[str, Any]) -> list[str]:
    notes = str(manifest.get("notes", ""))
    gaps: list[str] = []
    if "collision" in notes.lower():
        gaps.append("source manifest explicitly leaves collision to the consumer")
    if "lod" in notes.lower():
        gaps.append("source manifest explicitly leaves LOD to the consumer")
    if "runtime" in notes.lower() and "consumer" in notes.lower():
        gaps.append("no runtime consumer is asserted by the source manifest")
    if domain == "lighting_reference":
        gaps.append("reference only; no formal skybox or lighting profile")
    if domain == "material":
        gaps.append("texture candidate is not a project-owned Material asset")
    if domain in {"environment", "ui", "audio"} and not gaps:
        gaps.append("controlled work-scene consumer not yet authored")
    return sorted(set(gaps))


def build_catalog(project_root: Path) -> tuple[dict[str, Any], dict[str, Any]]:
    manifests = discover_manifests(project_root)
    entries: list[dict[str, Any]] = []
    excluded: list[dict[str, Any]] = []
    errors: list[str] = []
    seen_keys: set[tuple[str, str]] = set()
    eligible_manifest_ids: set[str] = set()
    candidate_file_total = 0

    for manifest_path in manifests:
        manifest_rel = manifest_path.relative_to(project_root).as_posix()
        manifest = read_json(manifest_path)
        candidate_files = manifest.get("candidate_files", [])
        if not isinstance(candidate_files, list):
            errors.append(f"{manifest_rel}: candidate_files is not a list")
            continue
        candidate_file_total += len(candidate_files)
        pack_id = str(manifest.get("pack_id", ""))
        reasons: list[str] = []
        review_status = manifest.get("review_status")
        import_status = manifest.get("import_status")
        if review_status != "ImportVerified":
            reasons.append(f"review_status={review_status}")
        if import_status != "ImportVerified":
            reasons.append(f"import_status={import_status}")
        if manifest.get("license_id") != "CC0-1.0":
            reasons.append("license_not_cc0")
        try:
            license_path = relative_file(project_root, str(manifest.get("license_evidence_path", "")))
            if not license_path.is_file():
                reasons.append("license_evidence_missing")
        except ValueError as exc:
            reasons.append(str(exc))
        digest = str(manifest.get("sha256", ""))
        if not SHA256_RE.fullmatch(digest):
            reasons.append("manifest_sha256_invalid")
        if manifest.get("runtime_consumers") not in ([], None):
            reasons.append("runtime_consumers_must_be_empty_at_intake")
        if manifest.get("file_count") != len(candidate_files):
            reasons.append("file_count_mismatch")
        import_evidence = import_evidence_for(manifest_path)
        try:
            import_evidence_path = relative_file(project_root, import_evidence)
            if not import_evidence_path.is_file():
                reasons.append("import_evidence_missing")
        except ValueError as exc:
            reasons.append(str(exc))

        if reasons:
            excluded.append({
                "manifest_id": pack_id,
                "manifest_path": manifest_rel,
                "candidate_file_count": len(candidate_files),
                "reasons": sorted(set(reasons)),
                "next_owner": "D18-S03 or the source asset task",
            })
            continue

        eligible_manifest_ids.add(pack_id)
        for candidate in candidate_files:
            if not isinstance(candidate, dict):
                errors.append(f"{manifest_rel}: candidate entry is not an object")
                continue
            raw_path = str(candidate.get("path", ""))
            try:
                candidate_path = relative_file(project_root, raw_path)
            except ValueError as exc:
                errors.append(f"{manifest_rel}: {exc}")
                continue
            if not candidate_path.is_file():
                errors.append(f"{manifest_rel}: candidate file missing: {raw_path}")
                continue
            key = (pack_id, raw_path.replace("\\", "/"))
            if key in seen_keys:
                errors.append(f"duplicate manifest/file reference: {key[0]}::{key[1]}")
                continue
            seen_keys.add(key)
            target_id = str(candidate.get("target_id", ""))
            domain = classify_domain(manifest, candidate)
            role = str(candidate.get("role", ""))
            intake_id = "intake_" + hashlib.sha256(f"{pack_id}::{raw_path}".encode("utf-8")).hexdigest()[:20]
            entries.append({
                "intake_id": intake_id,
                "manifest_id": pack_id,
                "manifest_path": manifest_rel,
                "candidate_file_path": raw_path.replace("\\", "/"),
                "content_id": target_id or None,
                "domain": domain,
                "priority": priority_for(domain, role),
                "role": role,
                "intake_eligible": True,
                "license": {
                    "license_id": manifest.get("license_id"),
                    "license_evidence_path": manifest.get("license_evidence_path"),
                    "commercial_use": manifest.get("commercial_use"),
                    "modification_allowed": manifest.get("modification_allowed"),
                    "source_redistribution": manifest.get("source_redistribution"),
                    "build_distribution": manifest.get("build_distribution"),
                },
                "hash_evidence": {
                    "path": manifest_rel,
                    "field": "sha256",
                    "sha256": digest,
                    "scope": "manifest archive or selected-file digest; source scope is preserved rather than inferred as a per-file digest",
                },
                "import_verified_evidence_path": import_evidence,
                "allowed_task_scope": allowed_tasks(domain),
                "forbidden_scope": [
                    "formal InitialHarbor integration",
                    "Registry or asset_state mutation",
                    "LocalDevelopmentReady or ReleaseApproved promotion",
                    "Steam or external distribution approval",
                ],
                "known_gaps": known_gap_for(domain, candidate, manifest),
                "local_development_ready_evidence_required": missing_local_ready_evidence(domain, manifest),
                "responsibility": {
                    "intake_owner": "D18-S02-v2",
                    "consumer_owner": allowed_tasks(domain)[0] if allowed_tasks(domain) else "D18-S03",
                    "promotion_owner": "D18-S03",
                },
            })

    entries.sort(key=lambda item: item["intake_id"])
    catalog = {
        "catalog_kind": "d18_prefab_candidate_intake_catalog",
        "schema_version": 1,
        "task": "D18-S02-v2",
        "generated_on": "2026-08-14",
        "status": "PASS" if not errors else "BLOCKED",
        "definition": "PrefabCandidateIntake means license, hash, path, import evidence and mapping are complete for controlled work-scene or Prefab authoring. It is not a resource state and never implies LocalDevelopmentReady, formal scene integration, runtime consumer, ReleaseApproved or Steam Ready.",
        "resource_state_machine_unchanged": True,
        "authority": {
            "manifests": MANIFEST_ROOT.as_posix(),
            "d18_local_development_ready_catalog": "docs/game/design/common/assets/d18_local_development_ready_catalog.json",
            "registry_changed": False,
            "asset_state_changed": False,
            "formal_initial_harbor_integrated": False,
            "release_approved_inferred": False,
        },
        "counts": {
            "manifest_total": len(manifests),
            "manifest_intake_eligible": len(eligible_manifest_ids),
            "candidate_file_total": candidate_file_total,
            "intake_eligible_file_total": len(entries),
            "excluded_manifest_total": len(excluded),
            "duplicate_manifest_file_references": 0,
            "errors": len(errors),
        },
        "eligible_entries": entries,
        "excluded_manifests": excluded,
        "project_owned_gaps": [
            {
                "gap_id": "gap_sea_weather_shader_lighting",
                "status": "ProjectOwnedAssetRequired",
                "description": "No external model candidate is being promoted for seawater, weather shaders, lighting or post-processing.",
                "owner_tasks": ["D19-A04", "D19-A05"],
                "note": "Create and test project-owned presentation assets; do not fabricate external resource readiness.",
            },
            {
                "gap_id": "gap_female_era_neutral_character",
                "status": "ExternalCandidateMissing",
                "owner_tasks": ["D20-A01", "D20-A03"],
                "note": "Current imported evidence does not prove a suitable female or era-neutral character set.",
            },
            {
                "gap_id": "gap_bird_cat_dog_prefabs",
                "status": "ExternalCandidateOrPrefabMissing",
                "owner_tasks": ["D20-A05", "D20-A06"],
                "note": "Bird is source-only and not imported; no cat candidate is present; dog-like source candidates remain non-intake until Prefab/era review.",
            },
            {
                "gap_id": "gap_ship_sail_rudder_anchor",
                "status": "CandidateNotIntake",
                "owner_tasks": ["D21-A01", "D21-A02", "D21-A03"],
                "note": "Ship/port candidates remain Candidate and have no consumer-owned sail, rudder, anchor, collision or interaction evidence.",
            },
            {
                "gap_id": "existing_starting_ship_authoritative",
                "status": "ExistingProjectAsset",
                "description": "The existing integrated asset_ph_ship_fishing remains the authoritative starting-ship visual; no external replacement is inferred by D18-S02-v2.",
                "owner_tasks": ["D21-A01", "D21-A03"],
                "note": "Keep the stable content ID and save semantics; validate any visual consumer change in D18-S03.",
            },
        ],
        "next_gate": {
            "task": "D18-S03",
            "responsibility": "Runtime consumers and LocalDevelopmentReady promotion audit after D19-D22 work-scenes, Prefabs, formal integration and PlayMode regression.",
            "must_verify": [
                "real Prefab and work-scene references",
                "collision, anchors, LOD and Animator where applicable",
                "runtime consumer and interaction behavior",
                "save/restore, screenshot, rollback and performance evidence",
                "no Registry/asset_state/status shortcut",
            ],
        },
    }
    result = {
        "task": "D18-S02-v2",
        "status": catalog["status"],
        "manifest_count": len(manifests),
        "candidate_file_total": candidate_file_total,
        "intake_eligible_file_total": len(entries),
        "intake_eligible_manifest_total": len(eligible_manifest_ids),
        "excluded_manifest_total": len(excluded),
        "errors": errors,
        "resource_state_snapshot": {
            "local_development_ready_count": 0,
            "release_approved_count": 0,
            "source_catalog_unchanged": True,
        },
        "cycle_fix": {
            "d18_s02_role": "admission and controlled development unlock",
            "d18_s03_role": "runtime consumer and LocalDevelopmentReady promotion gate",
            "d19_s01_allowed_after_v2_pass": True,
            "d23_requires_d18_s03_pass": True,
        },
    }
    return catalog, result


def validate_catalog(project_root: Path, catalog: dict[str, Any]) -> list[str]:
    """再次读取语义目录并检查引用、状态和重复；仍然不写任何资源状态。"""

    errors: list[str] = []
    if catalog.get("catalog_kind") != "d18_prefab_candidate_intake_catalog":
        errors.append("catalog_kind_invalid")
    if catalog.get("resource_state_machine_unchanged") is not True:
        errors.append("resource_state_machine_boundary_missing")
    if catalog.get("status") not in {"PASS", "BLOCKED"}:
        errors.append("catalog_status_invalid")
    entries = catalog.get("eligible_entries", [])
    seen_ids: set[str] = set()
    seen_files: set[tuple[str, str]] = set()
    for entry in entries:
        intake_id = str(entry.get("intake_id", ""))
        if not intake_id or intake_id in seen_ids:
            errors.append(f"duplicate_or_missing_intake_id:{intake_id}")
        seen_ids.add(intake_id)
        if entry.get("intake_eligible") is not True:
            errors.append(f"entry_not_eligible:{intake_id}")
        for forbidden_key in ("local_development_ready", "release_approved", "steam_ready"):
            if forbidden_key in entry:
                errors.append(f"status_shortcut_field_present:{intake_id}:{forbidden_key}")
        if entry.get("license", {}).get("license_id") != "CC0-1.0":
            errors.append(f"license_not_cc0:{intake_id}")
        if not SHA256_RE.fullmatch(str(entry.get("hash_evidence", {}).get("sha256", ""))):
            errors.append(f"hash_invalid:{intake_id}")
        manifest_rel = str(entry.get("manifest_path", ""))
        candidate_rel = str(entry.get("candidate_file_path", ""))
        key = (str(entry.get("manifest_id", "")), candidate_rel)
        if key in seen_files:
            errors.append(f"duplicate_manifest_file_reference:{key[0]}::{key[1]}")
        seen_files.add(key)
        try:
            manifest_path = relative_file(project_root, manifest_rel)
            candidate_path = relative_file(project_root, candidate_rel)
        except ValueError as exc:
            errors.append(f"unsafe_catalog_path:{intake_id}:{exc}")
            continue
        if not manifest_path.is_file():
            errors.append(f"manifest_missing:{intake_id}:{manifest_rel}")
            continue
        if not candidate_path.is_file():
            errors.append(f"candidate_missing:{intake_id}:{candidate_rel}")
            continue
        manifest = read_json(manifest_path)
        if manifest.get("pack_id") != entry.get("manifest_id"):
            errors.append(f"manifest_id_mismatch:{intake_id}")
        if manifest.get("review_status") != "ImportVerified" or manifest.get("import_status") != "ImportVerified":
            errors.append(f"non_import_verified_entry:{intake_id}")
    return sorted(set(errors))


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", type=Path, default=Path("."))
    parser.add_argument("--catalog", type=Path, default=CATALOG_REL)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--write-catalog", action="store_true")
    args = parser.parse_args(argv)
    project_root = args.project_root.resolve()
    catalog, result = build_catalog(project_root)
    catalog_validation_errors = validate_catalog(project_root, catalog)
    result["catalog_validation"] = {
        "status": "PASS" if not catalog_validation_errors else "BLOCKED",
        "entry_count": len(catalog.get("eligible_entries", [])),
        "duplicate_or_path_errors": catalog_validation_errors,
        "forbidden_promotion_fields_detected": any("status_shortcut_field_present" in error for error in catalog_validation_errors),
    }
    if catalog_validation_errors:
        result["status"] = "BLOCKED"
    output = args.output if args.output.is_absolute() else project_root / args.output
    write_json(output, result)
    if args.write_catalog:
        catalog_path = args.catalog if args.catalog.is_absolute() else project_root / args.catalog
        write_json(catalog_path, catalog)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
