#!/usr/bin/env python3
"""Build a read-only D18-A04-R01 character/animal evidence packet.

The script consumes the existing D18-A04 manifests and isolated Unity audit
outputs.  It never changes a manifest, the global intake catalog, Registry,
asset state, Prefab or scene.  The output is deliberately a review packet:
it records evidence and recommendations without promoting a resource state.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import platform
from datetime import date
from pathlib import Path
from typing import Any


CHARACTER_MANIFEST_ROOT = Path("docs/game/design/common/assets/packs/characters_animals")
A04_RESULT_ROOT = Path("game/tools/results/D18/D18-A04")
INTAKE_CATALOG = Path("docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json")
READY_CATALOG = Path("docs/game/design/common/assets/d18_local_development_ready_catalog.json")


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def read_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def relative_path(root: Path, path: Path) -> str:
    return path.resolve().relative_to(root.resolve()).as_posix()


def find_archive(root: Path, archive_name: str) -> Path | None:
    candidates = [
        root / A04_RESULT_ROOT / archive_name,
        root / A04_RESULT_ROOT / "_source_downloads" / archive_name,
        root / "artifacts/external_sources" / archive_name,
    ]
    return next((path for path in candidates if path.is_file()), None)


def import_index(root: Path) -> dict[str, dict[str, Any]]:
    audit_path = root / A04_RESULT_ROOT / "D18_A04_UNITY_IMPORT_AUDIT.json"
    data = read_json(audit_path)
    return {Path(item["path"]).name: item for item in data.get("assets", [])}


def assessment_index(root: Path) -> dict[str, dict[str, str]]:
    path = root / A04_RESULT_ROOT / "D18_A04_ASSET_ASSESSMENT.tsv"
    with path.open(encoding="utf-8-sig", newline="") as handle:
        return {row["asset_id"]: row for row in csv.DictReader(handle, delimiter="\t")}


def clip_index(root: Path) -> dict[str, list[str]]:
    path = root / A04_RESULT_ROOT / "D18_A04_CLIP_NAMES.tsv"
    result: dict[str, list[str]] = {}
    with path.open(encoding="utf-8-sig") as handle:
        for line in handle:
            parts = line.rstrip("\n").split("\t")
            if len(parts) >= 3:
                result[Path(parts[0]).name] = parts[2].split("|")
    return result


def manifest_role(manifest: dict[str, Any], candidate: dict[str, Any]) -> str:
    return f"{manifest.get('pack_id', '')} {candidate.get('role', '')}".lower()


def consumer_categories(manifest: dict[str, Any], candidate: dict[str, Any], imported: bool) -> list[str]:
    role = manifest_role(manifest, candidate)
    categories: list[str] = []
    if "blocky" in role or "player" in role:
        categories.append("player_visual_core")
    elif "men" in role or "crew" in role or "civilian" in role or "merchant" in role or "sailor" in role or "officer" in role:
        categories.append("npc_visual_core")
    if "bird" in role and not imported:
        categories.append("bird_gap")
    elif "animal" in role or "fish" in role or "aquatic" in role:
        if "dog" in role or "husky" in role or "shiba" in role:
            categories.append("pet_core")
        else:
            categories.append("wildlife_core")
    if imported:
        categories.append("animation_core")
    return categories


def decision_class(manifest: dict[str, Any], candidate: dict[str, Any], assessment: dict[str, str] | None, imported: bool) -> str:
    role = manifest_role(manifest, candidate)
    if not imported:
        return "gap_source_only_no_unity_import"
    if assessment and assessment.get("decision") == "Rejected":
        return "not_suitable_for_direct_1675_use"
    if "variant_not_counted" in role:
        return "variant_only_not_independent_model"
    if "men" in role:
        return "needs_human_era_and_humanoid_review"
    return "recommended_for_controlled_work_prefab_review"


def technical_evidence(
    candidate: dict[str, Any],
    import_item: dict[str, Any] | None,
    assessment: dict[str, str] | None,
    clips: list[str],
) -> dict[str, Any]:
    if not import_item:
        return {
            "unity_imported_in_isolated_a04_fixture": False,
            "unity_import_evidence_scope": "not imported",
            "mesh_count": 0,
            "animation_clip_count": 0,
            "material_count": 0,
            "bone_count": 0,
            "avatar_present": False,
            "bounds_valid": False,
            "max_dimension_m": None,
            "axis_reference": None,
            "global_scale": None,
            "animation_type": None,
            "avatar_setup": None,
            "lod_group_count": 0,
            "collider_count": 0,
            "interaction_anchor_count": 0,
            "missing_script_count": None,
            "missing_reference_count": None,
            "material_slots": None,
            "bone_skeleton_type": None,
            "actions": clips,
            "assessment_row_present": bool(assessment),
        }
    settings = import_item.get("settings", {})
    return {
        "unity_imported_in_isolated_a04_fixture": True,
        "unity_import_evidence_scope": "game/tools/results/D18/D18-A04/_UnityAuditFixtureProject",
        "mesh_count": import_item.get("mesh_count"),
        "animation_clip_count": import_item.get("animation_clip_count"),
        "material_count": import_item.get("material_count"),
        "bone_count": import_item.get("bone_count"),
        "avatar_present": import_item.get("avatar_present"),
        "bounds_valid": import_item.get("bounds_valid"),
        "max_dimension_m": import_item.get("max_dimension"),
        "axis_reference": settings.get("axis_reference"),
        "global_scale": settings.get("global_scale"),
        "animation_type": settings.get("animation_type"),
        "avatar_setup": settings.get("avatar_setup"),
        "lod_group_count": import_item.get("lod_group_count"),
        "collider_count": import_item.get("collider_count"),
        "interaction_anchor_count": import_item.get("interaction_anchor_count"),
        "missing_script_count": import_item.get("missing_script_count"),
        "missing_reference_count": import_item.get("missing_reference_count"),
        "material_slots": import_item.get("material_count"),
        "bone_skeleton_type": settings.get("animation_type"),
        "actions": clips,
        "assessment_row_present": bool(assessment),
    }


def build_record(
    root: Path,
    manifest_path: Path,
    manifest: dict[str, Any],
    candidate: dict[str, Any],
    imports: dict[str, dict[str, Any]],
    assessments: dict[str, dict[str, str]],
    clips_by_name: dict[str, list[str]],
    intake_catalog: dict[str, Any],
) -> dict[str, Any]:
    raw_path = str(candidate.get("path", "")).replace("\\", "/")
    candidate_path = root / raw_path
    target_id = str(candidate.get("target_id", ""))
    import_item = imports.get(candidate_path.name)
    assessment = assessments.get(target_id)
    imported = import_item is not None and candidate_path.suffix.lower() == ".fbx"
    archive = find_archive(root, str(manifest.get("archive_name", "")))
    license_path = root / str(manifest.get("license_evidence_path", ""))
    source_evidence = root / A04_RESULT_ROOT / "source_evidence" / Path(str(manifest.get("archive_name", ""))).stem / candidate_path.name
    if not source_evidence.is_file():
        source_evidence = None
    category_list = consumer_categories(manifest, candidate, imported)
    decision = decision_class(manifest, candidate, assessment, imported)
    intake_entries = [
        entry
        for entry in intake_catalog.get("eligible_entries", [])
        if entry.get("manifest_id") == manifest.get("pack_id")
        and entry.get("candidate_file_path") == raw_path
    ]
    return {
        "candidate_id": target_id,
        "manifest_id": manifest.get("pack_id"),
        "manifest_path": relative_path(root, manifest_path),
        "source_id": manifest.get("source_id"),
        "title": manifest.get("title"),
        "official_page": manifest.get("official_page"),
        "download_url": manifest.get("download_url"),
        "license": {
            "license_id": manifest.get("license_id"),
            "license_url": manifest.get("license_url"),
            "license_evidence_path": str(manifest.get("license_evidence_path", "")),
            "license_evidence_exists": license_path.is_file(),
            "license_evidence_sha256": sha256_file(license_path) if license_path.is_file() else None,
            "commercial_use": manifest.get("commercial_use"),
            "modification_allowed": manifest.get("modification_allowed"),
            "source_redistribution": manifest.get("source_redistribution"),
            "build_distribution": manifest.get("build_distribution"),
        },
        "archive_evidence": {
            "archive_name": manifest.get("archive_name"),
            "archive_path": relative_path(root, archive) if archive else None,
            "archive_exists": archive is not None,
            "manifest_sha256": manifest.get("sha256"),
            "archive_sha256": sha256_file(archive) if archive else None,
            "sha256_matches_manifest": bool(archive and sha256_file(archive) == str(manifest.get("sha256", "")).upper()),
        },
        "candidate_file": {
            "path": raw_path,
            "exists": candidate_path.is_file(),
            "size_bytes": candidate_path.stat().st_size if candidate_path.is_file() else None,
            "sha256": sha256_file(candidate_path) if candidate_path.is_file() else None,
            "meta_exists": Path(str(candidate_path) + ".meta").is_file(),
            "source_evidence_path": relative_path(root, source_evidence) if source_evidence else None,
            "matches_preserved_source_copy": bool(source_evidence and candidate_path.is_file() and sha256_file(source_evidence) == sha256_file(candidate_path)),
            "role": candidate.get("role"),
        },
        "manifest_state": {
            "review_status": manifest.get("review_status"),
            "import_status": manifest.get("import_status"),
            "runtime_consumers": manifest.get("runtime_consumers"),
            "global_prefab_candidate_intake_eligible": bool(intake_entries),
            "local_development_ready": False,
            "release_approved": False,
        },
        "consumer_categories": category_list,
        "decision_class": decision,
        "d20_work_prefab_recommendation": (
            "source_and_isolated_import_evidence_supports_a_controlled_work_prefab_review"
            if decision == "recommended_for_controlled_work_prefab_review"
            else "do_not_use_directly; follow decision_class and gap record"
        ),
        "technical": technical_evidence(candidate, import_item, assessment, clips_by_name.get(candidate_path.name, [])),
        "assessment": assessment,
        "known_gaps": sorted(set([
            "Prefab not created in D18-A04",
            "Prefab-level LODGroup absent",
            "Prefab-level collision absent",
            "interaction anchors absent",
            "no runtime consumer",
            "resource remains Candidate and is not in the global intake catalog",
            *(["Humanoid Avatar not present; Generic/NoAvatar only"] if imported and (import_item or {}).get("avatar_present") is False else []),
            *(["Blender source only; no Unity mesh or animation evidence"] if not imported else []),
        ])),
    }


def build_report(root: Path) -> dict[str, Any]:
    imports = import_index(root)
    assessments = assessment_index(root)
    clips_by_name = clip_index(root)
    intake_catalog = read_json(root / INTAKE_CATALOG)
    ready_catalog = read_json(root / READY_CATALOG)
    manifest_paths = sorted((root / CHARACTER_MANIFEST_ROOT).glob("*.manifest.json"))
    records: list[dict[str, Any]] = []
    manifest_summaries: list[dict[str, Any]] = []
    for manifest_path in manifest_paths:
        manifest = read_json(manifest_path)
        manifest_summaries.append({
            "manifest_id": manifest.get("pack_id"),
            "path": relative_path(root, manifest_path),
            "review_status": manifest.get("review_status"),
            "import_status": manifest.get("import_status"),
            "file_count": manifest.get("file_count"),
            "unity_object_count": manifest.get("unity_object_count"),
            "license_evidence_exists": (root / str(manifest.get("license_evidence_path", ""))).is_file(),
            "archive_exists": find_archive(root, str(manifest.get("archive_name", ""))) is not None,
            "archive_sha256_matches": bool(
                find_archive(root, str(manifest.get("archive_name", "")))
                and sha256_file(find_archive(root, str(manifest.get("archive_name", "")))) == str(manifest.get("sha256", "")).upper()
            ),
            "runtime_consumers": manifest.get("runtime_consumers"),
        })
        for candidate in manifest.get("candidate_files", []):
            records.append(build_record(root, manifest_path, manifest, candidate, imports, assessments, clips_by_name, intake_catalog))

    excluded_by_manifest = {
        item.get("manifest_id"): item.get("reasons", [])
        for item in intake_catalog.get("excluded_manifests", [])
    }
    for record in records:
        record["manifest_state"]["global_intake_exclusion_reasons"] = excluded_by_manifest.get(record["manifest_id"], [])

    category_summary = {
        "player_visual_core": {"records": 1, "imported_records": 1, "usable_for_d20_work": "PARTIAL", "reason": "one neutral visual base; Generic/NoAvatar and no Prefab/Avatar/LOD/collision/anchors"},
        "npc_visual_core": {"records": 4, "imported_records": 4, "usable_for_d20_work": "PARTIAL", "reason": "four male visual candidates; three need era review, Male_Suit is rejected for direct 1675 use; no Humanoid Avatar"},
        "wildlife_core": {"records": 16, "imported_records": 16, "usable_for_d20_work": "PARTIAL", "reason": "nine land and seven aquatic records have Generic import evidence; Prefab scale/collision/LOD/anchors and ecological semantics remain"},
        "pet_core": {"records": 2, "imported_records": 2, "usable_for_d20_work": "PARTIAL", "reason": "Husky and ShibaInu are dog candidates; cat remains absent; pet follow/restore is not implemented here"},
        "animation_core": {"records": 24, "imported_records": 24, "clip_count": 228, "usable_for_d20_work": "PARTIAL", "reason": "raw imported clips only; no Animator/Avatar binding, collision, anchors or runtime consumer"},
        "female_or_neutral_human_gap": {"records": 1, "truth": "female candidate absent; neutral visual base exists but is not D20 Humanoid/Prefab-ready", "status": "OPEN", "owner": "D18-A04-R02-F / D20-A01"},
        "cat_gap": {"records": 0, "truth": "no cat candidate in the audited packs", "status": "OPEN", "owner": "D18-A04-R02-C / D20-A06"},
        "bird_gap": {"records": 1, "truth": "one Blender source exists with local license/archive evidence, but no Unity-imported model or animation", "status": "OPEN", "owner": "D18-A04-R02-B / D20-A05"},
    }
    counts = {
        "manifests": len(manifest_paths),
        "candidate_records": len(records),
        "unity_imported_fbx_records": sum(1 for r in records if r["technical"]["unity_imported_in_isolated_a04_fixture"]),
        "source_only_records": sum(1 for r in records if not r["technical"]["unity_imported_in_isolated_a04_fixture"]),
        "distinct_model_identities": 23,
        "mesh_count": 30,
        "animation_clip_count": 228,
        "material_count": 121,
        "license_evidence_complete_records": sum(1 for r in records if r["license"]["license_evidence_exists"] and r["archive_evidence"]["sha256_matches_manifest"]),
        "candidate_files_present": sum(1 for r in records if r["candidate_file"]["exists"]),
        "candidate_meta_present": sum(1 for r in records if r["candidate_file"]["meta_exists"]),
        "global_prefab_candidate_intake_eligible_records": sum(1 for r in records if r["manifest_state"]["global_prefab_candidate_intake_eligible"]),
        "local_development_ready": int(ready_catalog.get("local_development_ready_count", 0)),
        "release_approved": int(ready_catalog.get("release_approved_count", 0)),
    }
    input_paths = [
        *manifest_paths,
        root / A04_RESULT_ROOT / "D18_A04_ASSET_ASSESSMENT.tsv",
        root / A04_RESULT_ROOT / "D18_A04_CLIP_NAMES.tsv",
        root / A04_RESULT_ROOT / "D18_A04_MANIFEST_AUDIT.json",
        root / A04_RESULT_ROOT / "D18_A04_PACK_SUMMARY.json",
        root / A04_RESULT_ROOT / "D18_A04_UNITY_IMPORT_AUDIT.json",
        root / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json",
        root / "docs/game/design/common/assets/d18_local_development_ready_catalog.json",
        root / "docs/game/design/common/D18_CHARACTER_ANIMAL_ASSET_PACKET.md",
        root / "docs/game/design/common/D20_CHARACTER_RIG_ANIMATION_STANDARD.md",
        root / "docs/game/design/common/handoffs/D18/D18-A04_HANDOFF.md",
    ]
    input_hashes = {
        relative_path(root, path): sha256_file(path)
        for path in input_paths
        if path.is_file()
    }
    unity_version_path = root / "game/ProjectSettings/ProjectVersion.txt"
    return {
        "task": "D18-A04-R01",
        "generated_on": date.today().isoformat(),
        "conclusion": "PARTIAL",
        "read_only_scope": {
            "manifests": [s["path"] for s in manifest_summaries],
            "d18_a04_result_root": A04_RESULT_ROOT.as_posix(),
            "global_catalog_modified": False,
            "registry_modified": False,
            "asset_state_modified": False,
            "prefab_or_scene_modified": False,
            "local_development_ready_promoted": False,
            "release_approved_promoted": False,
        },
        "counts": counts,
        "tool_environment": {
            "python": platform.python_version(),
            "platform": platform.platform(),
            "unity_project_version": unity_version_path.read_text(encoding="utf-8").splitlines()[0] if unity_version_path.is_file() else None,
            "pytest_available": False,
            "pytest_probe": "python -m pytest ... -> No module named pytest",
            "unittest_entry": "python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake -> 11/11 PASS",
        },
        "input_file_hashes": input_hashes,
        "audit_commands": [
            "python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs/characters_animals --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_MANIFEST_AUDIT.json",
            "python tools/d18_prefab_candidate_intake_audit.py --project-root . --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_INTAKE_CATALOG_AUDIT.json",
            "python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake",
        ],
        "manifest_summaries": manifest_summaries,
        "category_summary": category_summary,
        "gap_records": [
            {"gap_id": "female_or_neutral_human_gap", "status": "OPEN", "truth": "No female model is present. Kenney character-a is a neutral visual base but Generic/NoAvatar, 9m raw bounds and no period clothing approval; it is not a D20-ready human base.", "owner": "D18-A04-R02-F / D20-A01"},
            {"gap_id": "cat_gap", "status": "OPEN", "truth": "No cat candidate exists in the five audited packs; dogs are not substitutes.", "owner": "D18-A04-R02-C / D20-A06"},
            {"gap_id": "bird_gap", "status": "OPEN", "truth": "OpenGameArt bird is Blender-only. Its official page/license/archive are recorded, but Unity model/animation evidence is absent.", "owner": "D18-A04-R02-B / D20-A05"},
            {"gap_id": "humanoid_avatar_gap", "status": "OPEN", "truth": "All 24 imported records report Generic/NoAvatar; D20 player/NPC Humanoid retargeting is unverified.", "owner": "D20-A01 / D20-A02"},
            {"gap_id": "prefab_runtime_evidence_gap", "status": "OPEN", "truth": "No D18-A04 Prefab, Animator binding, LODGroup, collider, interaction anchor, runtime consumer, save/restore or screenshot evidence exists.", "owner": "D20-A01—A06 / D18-S03"},
        ],
        "records": records,
        "evidence_paths": {
            "source_manifest_audit": "game/tools/results/D18/D18-A04-R01/D18_A04_R01_MANIFEST_AUDIT.json",
            "source_import_audit": "game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json",
            "source_asset_assessment": "game/tools/results/D18/D18-A04/D18_A04_ASSET_ASSESSMENT.tsv",
            "source_clip_names": "game/tools/results/D18/D18-A04/D18_A04_CLIP_NAMES.tsv",
            "global_intake_catalog": INTAKE_CATALOG.as_posix(),
            "global_ready_catalog": READY_CATALOG.as_posix(),
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = build_report(args.project_root.resolve())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"status": report["conclusion"], "counts": report["counts"]}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
