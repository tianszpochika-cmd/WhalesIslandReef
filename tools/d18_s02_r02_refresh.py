#!/usr/bin/env python3
"""Refresh the D18-S02-R02 controlled-work intake from R02 evidence.

This tool has one intentional global write target: the PrefabCandidateIntake
catalog. It never changes the six resource states, Registry, Prefab, scene,
runtime consumer, LocalDevelopmentReady or ReleaseApproved.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
from datetime import date
from pathlib import Path
from typing import Any

from tools import d18_s02_r01_intake_refresh as base


CATALOG_REL = Path("docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json")
READY_REL = Path("docs/game/design/common/assets/d18_local_development_ready_catalog.json")
PACKET_REL = Path(
    "docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance/"
    "D18_A04_R03_P_PLAYER_APPEARANCE_PACKET.json"
)
IMPORT_REL = Path("game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json")
MACHINE_REL = Path("game/tools/results/D18/D18-A04-R03-P/D18_A04_R03_P_MACHINE_RESULT.json")
HANDOFF_REL = Path("docs/game/design/common/handoffs/D18/D18-A04-R03-P_HANDOFF.md")
R02F_MACHINE_REL = Path("game/tools/results/D18/D18-A04-R02-F-R01/D18_A04_R02_F_R01_MACHINE_RESULT.json")
R02F_HANDOFF_REL = Path("docs/game/design/common/handoffs/D18/D18-A04-R02-F-R01_HANDOFF.md")


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def make_entries(project_root: Path, catalog: dict[str, Any]) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[str]]:
    packet = base.read_json(project_root / PACKET_REL)
    manifest_by_id, manifest_errors = base.manifest_index(project_root)
    packet_by_id = {str(item.get("content_id")): item for item in packet.get("modules", [])}
    entries: list[dict[str, Any]] = []
    excluded: list[dict[str, Any]] = []
    errors: list[str] = list(manifest_errors)
    for package in packet.get("source_packages", []):
        pack_id = str(package.get("package_id", ""))
        record = manifest_by_id.get(pack_id)
        if record is None:
            errors.append(f"R03-P:manifest_missing:{pack_id}")
            continue
        manifest_rel = str(record["path"])
        manifest = record["data"]
        for module in [item for item in packet.get("modules", []) if item.get("package_id") == pack_id]:
            target_id = str(module.get("content_id", ""))
            candidate = next(
                (item for item in manifest.get("candidate_files", []) if item.get("target_id") == target_id),
                None,
            )
            if candidate is None:
                excluded.append({"content_id": target_id, "reasons": ["candidate_not_in_manifest"]})
                continue
            entry, entry_errors = base.entry_from_evidence(
                project_root,
                manifest_rel,
                manifest,
                candidate,
                review_task="D18-A04-R03-P",
                review_packet=base.rel(project_root / PACKET_REL, project_root),
                machine_evidence=[base.rel(project_root / MACHINE_REL, project_root), base.rel(project_root / HANDOFF_REL, project_root)],
                import_evidence=base.rel(project_root / IMPORT_REL, project_root),
                archive_path=base.safe_path(project_root, str(package.get("archive_path", ""))),
                candidate_sha256=str(module.get("sha256", "")),
                candidate_hash_evidence=base.rel(project_root / PACKET_REL, project_root),
                categories=["player_visual_core"],
                technical={
                    "animation_type": module.get("animation_type"),
                    "avatar_setup": module.get("avatar_setup"),
                    "avatar_present": module.get("avatar_present"),
                    "avatar_valid": module.get("avatar_valid"),
                    "avatar_is_human": module.get("avatar_is_human"),
                    "mesh_count": module.get("mesh_count"),
                    "material_count": module.get("material_count"),
                    "renderer_height_m": module.get("renderer_height_m"),
                    "collider_count": 0,
                    "lod_group_count": 0,
                    "interaction_anchor_count": 0,
                    "animation_clip_count": 0,
                },
                upstream_decision="isolated_source_and_import_evidence_complete; formal consumer evidence remains open",
            )
            if entry is None:
                excluded.append({"content_id": target_id, "candidate_file_path": module.get("candidate_file_path"), "reasons": entry_errors})
                errors.extend(entry_errors)
            else:
                entries.append(entry)
    return entries, excluded, errors


def refresh(project_root: Path, generated_on: str) -> tuple[dict[str, Any], dict[str, Any]]:
    catalog_path = project_root / CATALOG_REL
    current = base.read_json(catalog_path)
    prior_snapshot = current.get("previous_r01_catalog_snapshot")
    baseline_sha = (
        str(prior_snapshot.get("catalog_sha256"))
        if current.get("task") == "D18-S02-R02" and isinstance(prior_snapshot, dict) and prior_snapshot.get("catalog_sha256")
        else base.sha256_file(catalog_path)
    )
    old_entries = copy.deepcopy(current.get("eligible_entries", []))
    new_entries, new_excluded, manifest_errors = make_entries(project_root, current)
    existing_refs = {
        (str(item.get("manifest_id")), base.normalize_path(str(item.get("candidate_file_path", ""))))
        for item in old_entries
    }
    accepted_new: list[dict[str, Any]] = []
    duplicate_refs: list[str] = []
    seen_refs: set[tuple[str, str]] = set()
    for entry in new_entries:
        ref = (str(entry.get("manifest_id")), base.normalize_path(str(entry.get("candidate_file_path", ""))))
        if ref in existing_refs:
            # Idempotent reruns must not report already admitted source rows as
            # a new duplicate. They remain governed by the prior evidence.
            continue
        if ref in seen_refs:
            duplicate_refs.append(f"{ref[0]}::{ref[1]}")
            continue
        seen_refs.add(ref)
        accepted_new.append(entry)
    entries = sorted(old_entries + accepted_new, key=lambda item: str(item.get("intake_id", "")))
    previous_entry_count = int(
        (prior_snapshot or {}).get("counts", {}).get("intake_eligible_file_total", len(old_entries))
    ) if isinstance(prior_snapshot, dict) else len(old_entries)
    r03_admitted_total = sum(
        1 for item in entries if item.get("admission_basis", {}).get("source_review_task") == "D18-A04-R03-P"
    )
    manifest_by_id, index_errors = base.manifest_index(project_root)
    entry_manifest_ids = {str(item.get("manifest_id")) for item in entries}
    excluded = copy.deepcopy(current.get("excluded_manifests", []))
    excluded_ids = {str(item.get("manifest_id")) for item in excluded}
    for item in new_excluded:
        pack_id = str(item.get("manifest_id", ""))
        if pack_id and pack_id not in excluded_ids:
            excluded.append(item)
    for pack_id, record in sorted(manifest_by_id.items()):
        if pack_id in entry_manifest_ids or pack_id in excluded_ids:
            continue
        excluded.append({
            "manifest_id": pack_id,
            "manifest_path": record["path"],
            "candidate_file_count": len(record["data"].get("candidate_files", [])),
            "reasons": ["no evidence-complete controlled intake packet"],
            "next_owner": "D18-S03 or responsible consumer task",
        })
    counts = {
        "manifest_total": len(manifest_by_id),
        "manifest_intake_eligible": len(entry_manifest_ids),
        "candidate_file_total": sum(len(record["data"].get("candidate_files", [])) for record in manifest_by_id.values()),
        "intake_eligible_file_total": len(entries),
        "excluded_manifest_total": len(excluded),
        "duplicate_manifest_file_references": len(duplicate_refs),
        "refresh_admitted_file_total": int(current.get("counts", {}).get("refresh_admitted_file_total", 0)) + len(accepted_new),
        "errors": len(set(index_errors + manifest_errors)),
    }
    category_summary = base.category_summary(entries)
    category_summary["female_or_neutral_human_gap"] = {
        "intake_eligible_count": sum(
            1 for item in entries if item.get("content_id") == "candidate_superhero_female_fullbody_r03p"
        ),
        "status": "INTAKE_AVAILABLE_PARTIAL",
        "reason": "One isolated female source has a real Humanoid + valid Avatar; historical four-file female FBX set remains non-human, and 1675 clothing/scale/Prefab evidence is open.",
        "owner_tasks": ["D20-A01-R01", "D20-A03", "D18-S03"],
    }
    category_summary["player_appearance_module"] = {
        "intake_eligible_count": sum(
            1 for item in entries if item.get("admission_basis", {}).get("source_review_task") == "D18-A04-R03-P"
        ),
        "status": "INTAKE_AVAILABLE_PARTIAL",
        "reason": "R03-P supplies five hair styles, one facial-hair mesh and female/male modular clothing meshes; attachment, era review, collision, LOD and runtime evidence remain open.",
        "owner_tasks": ["D20-A01-R01", "D20-A03", "D18-S03"],
    }
    project_gaps = copy.deepcopy(current.get("project_owned_gaps", []))
    for gap in project_gaps:
        if gap.get("gap_id") == "gap_female_era_neutral_character":
            gap["status"] = "ControlledIntakePartial"
            gap["owner_tasks"] = ["D20-A01-R01", "D20-A03", "D18-S03"]
            gap["note"] = "R03-P proves one female Humanoid + valid Avatar candidate and appearance modules for controlled work, but the D17 scale, historical clothing, bone attachment, collision, LOD and runtime evidence remain open; R02-F four historical FBX remain non-human."
    ready = base.read_json(project_root / READY_REL)
    ready_counts = {
        "local_development_ready_count": int(ready.get("local_development_ready_count", 0)),
        "release_approved_count": int(ready.get("release_approved_count", 0)),
        "eligible_entry_count": int(ready.get("eligible_entry_count", 0)),
        "ready_catalog_modified": False,
    }
    catalog = {
        "catalog_kind": "d18_prefab_candidate_intake_catalog",
        "schema_version": 1,
        "task": "D18-S02-R02",
        "generated_on": generated_on,
        "status": "PASS" if not (index_errors or manifest_errors or duplicate_refs) else "BLOCKED",
        "definition": current.get("definition"),
        "resource_state_machine_unchanged": True,
        "authority": {
            **copy.deepcopy(current.get("authority", {})),
            "registry_changed": False,
            "asset_state_changed": False,
            "formal_initial_harbor_integrated": False,
            "release_approved_inferred": False,
            "global_catalog_writer": "D18-S02-R02",
        },
        "previous_v2_catalog_snapshot": copy.deepcopy(current.get("previous_v2_catalog_snapshot")),
        "previous_r01_catalog_snapshot": copy.deepcopy(prior_snapshot) if isinstance(prior_snapshot, dict) else {
            "catalog_path": CATALOG_REL.as_posix(),
            "catalog_sha256": baseline_sha,
            "task": current.get("task"),
            "status": current.get("status"),
            "counts": copy.deepcopy(current.get("counts", {})),
            "historical_report": "docs/game/design/common/D18_FREE_ASSET_INTAKE_REFRESH_R01.md",
            "historical_handoff": "docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md",
        },
        "counts": counts,
        "eligible_entries": entries,
        "excluded_manifests": sorted(excluded, key=lambda item: str(item.get("manifest_id", ""))),
        "category_summary": category_summary,
        "project_owned_gaps": project_gaps,
        "refresh": {
            "refresh_task": "D18-S02-R02",
            "previous_r01_catalog_snapshot": {
                "catalog_sha256": baseline_sha,
                "counts": copy.deepcopy(current.get("counts", {})),
            },
            "upstream_reviews": [
                {"task": "D18-S02-R01", "status": current.get("status"), "records_processed": len(old_entries), "records_admitted": len(old_entries), "handoff": "docs/game/design/common/handoffs/D18/D18-S02-R01_HANDOFF.md"},
                {"task": "D18-A04-R02-F-R01", "status": "PARTIAL", "records_processed": 4, "records_admitted": 0, "handoff": R02F_HANDOFF_REL.as_posix()},
                {"task": "D18-A04-R03-P", "status": "PARTIAL", "records_processed": len(new_entries), "records_admitted": r03_admitted_total, "handoff": HANDOFF_REL.as_posix()},
            ],
            "admitted_from_upstream": [
                {"task": "D18-S02-R01", "records_admitted": len(old_entries)},
                {"task": "D18-A04-R02-F-R01", "records_admitted": 0},
                {"task": "D18-A04-R03-P", "records_admitted": r03_admitted_total, "manifest_ids": sorted({str(item.get("manifest_id")) for item in entries if item.get("admission_basis", {}).get("source_review_task") == "D18-A04-R03-P"})},
            ],
            "admission_policy": "PrefabCandidateIntake is a controlled work-scene admission flag backed by local file, per-file hash, CC0 evidence and isolated ImportVerified evidence. It is not a resource state and never implies runtime readiness.",
            "runtime_promotion": {"local_development_ready_count": 0, "release_approved_count": 0, "promotion_performed": False, "next_gate": "D18-S03"},
        },
        "next_gate": {
            "task": "D18-S03",
            "responsibility": "Runtime consumers and LocalDevelopmentReady promotion audit after D19-D22 work-scenes, Prefabs, formal integration and PlayMode regression.",
            "must_verify": ["real Prefab and work-scene references", "collision, anchors, LOD and Animator where applicable", "runtime consumer and interaction behavior", "save/restore, screenshot, rollback and performance evidence", "no Registry/asset_state/status shortcut"],
        },
    }
    validation_errors = base.validate_catalog(project_root, catalog)
    if validation_errors:
        catalog["status"] = "BLOCKED"
    gate_candidates = [
        {
            "content_id": item.get("content_id"),
            "manifest_id": item.get("manifest_id"),
            "candidate_file_path": item.get("candidate_file_path"),
            "intake_id": item.get("intake_id"),
            "avatar_is_human": item.get("technical_snapshot", {}).get("avatar_is_human"),
            "avatar_valid": item.get("technical_snapshot", {}).get("avatar_valid"),
            "import_evidence": item.get("import_verified_evidence_path"),
            "allowed_work_tasks": ["D20-A01-R01", "D20-A03"],
            "not_runtime_ready": True,
        }
        for item in entries
        if item.get("content_id") == "candidate_superhero_female_fullbody_r03p"
    ]
    gate = {
        "task": "D18-S02-R02",
        "status": catalog["status"],
        "generated_on": generated_on,
        "player_humanoid_candidate_available": len(gate_candidates) == 1,
        "humanoid_valid_avatar_count": len(gate_candidates),
        "candidates": gate_candidates,
        "d20_a01_r01": {"allowed_to_start": len(gate_candidates) == 1, "condition": "controlled work Prefab only; D20-A01 must produce a real rig_interface_ready result before D20-A02"},
        "d20_a02_r01": {"allowed_to_start": False, "reason": "D20-A01-R01 rig_interface_ready is not yet proven"},
        "female_gap": category_summary["female_or_neutral_human_gap"],
        "excluded_historical_female_candidates": {"count": 4, "reason": "D18-A04-R02-F-R01 real Unity evidence is non-human Avatar; see machine result", "evidence": R02F_MACHINE_REL.as_posix()},
        "state_boundaries": {"local_development_ready_count": 0, "release_approved_count": 0, "formal_scene_consumers": 0, "d18_s03_required": True},
        "source_catalog": CATALOG_REL.as_posix(),
    }
    machine = {
        "task": "D18-S02-R02",
        "status": catalog["status"],
        "generated_on": generated_on,
        "previous_r01_catalog_sha256": baseline_sha,
        "input_manifest_total": len(manifest_by_id),
        "input_candidate_file_total": counts["candidate_file_total"],
        "previous_intake_entries": previous_entry_count,
        "new_intake_entries": len(entries) - previous_entry_count,
        "output_intake_entries": len(entries),
        "duplicate_new_references": duplicate_refs,
        "manifest_index_errors": index_errors,
        "evidence_errors": manifest_errors,
        "catalog_validation_errors": validation_errors,
        "local_development_ready_count": ready_counts["local_development_ready_count"],
        "release_approved_count": ready_counts["release_approved_count"],
        "source_manifests_modified": False,
        "ready_catalog_modified": False,
        "registry_modified": False,
        "asset_state_modified": False,
        "new_source_evidence": [PACKET_REL.as_posix(), IMPORT_REL.as_posix(), MACHINE_REL.as_posix(), HANDOFF_REL.as_posix(), R02F_MACHINE_REL.as_posix(), R02F_HANDOFF_REL.as_posix()],
        "result": {"catalog": CATALOG_REL.as_posix(), "player_consumption_gate": "game/tools/results/D18/D18-S02-R02/D18_R02_PLAYER_CONSUMPTION_GATE.json"},
        "manifest_audit": "game/tools/results/D18/D18-S02-R02/D18_S02_R02_MANIFEST_AUDIT_PACKS.json",
    }
    return catalog, {"machine": machine, "gate": gate}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", type=Path, default=Path("."))
    parser.add_argument("--generated-on", default=date.today().isoformat())
    parser.add_argument("--write", action="store_true")
    args = parser.parse_args()
    project_root = args.project_root.resolve()
    catalog, outputs = refresh(project_root, args.generated_on)
    result_dir = project_root / "game/tools/results/D18/D18-S02-R02"
    result_dir.mkdir(parents=True, exist_ok=True)
    write_json(result_dir / "D18_S02_R02_MACHINE_RESULT.json", outputs["machine"])
    write_json(result_dir / "D18_R02_PLAYER_CONSUMPTION_GATE.json", outputs["gate"])
    if args.write and outputs["machine"]["status"] == "PASS":
        write_json(project_root / CATALOG_REL, catalog)
    print(json.dumps({"status": outputs["machine"]["status"], "machine": outputs["machine"], "gate": outputs["gate"]}, ensure_ascii=False, indent=2))
    return 0 if outputs["machine"]["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
