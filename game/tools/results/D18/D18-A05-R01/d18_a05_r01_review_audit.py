"""D18-A05-R01 read-only evidence review.

This audit reads the D18-A05 candidate files, manifests, source/hash packet,
existing Unity report-only import result, D18-S02-v2 intake catalog and the
historical D19 visual QA. It writes only the R01 review packet and machine
result; it never changes candidate files, manifests, catalog, Registry,
scenes, Prefabs or runtime code.
"""

from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[5]
PACK_ROOT = ROOT / "docs/game/design/common/assets/packs/ships_port_props"
CANDIDATE_ROOT = ROOT / "game/Assets/Art/Candidates/ShipsPortProps"
IMPORT_RESULT = ROOT / "game/tools/results/D18/D18-A05/D18_A02_KENNEY_IMPORT_AUDIT.json"
IMPORT_SUMMARY = ROOT / "game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.json"
MANIFEST_AUDIT = ROOT / "game/tools/results/D18/D18-A05-R01/D18_A05_R01_MANIFEST_AUDIT.json"
INTAKE_AUDIT = ROOT / "game/tools/results/D18/D18-A05-R01/D18_A05_R01_INTAKE_AUDIT.json"
INTAKE_CATALOG = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
HASH_PACKET = PACK_ROOT / "D18_A05_FILE_HASHES.json"
VISUAL_QA = ROOT / "game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json"
REVIEW_JSON = ROOT / "docs/game/design/common/assets/review_packets/D18-A05-R01/D18_A05_R01_SHIP_PORT_REVIEW.json"
MACHINE_JSON = ROOT / "game/tools/results/D18/D18-A05-R01/D18_A05_R01_MACHINE_RESULT.json"

MANIFESTS = [
    PACK_ROOT / "kenney_building_kit_ships_port_props.manifest.json",
    PACK_ROOT / "kenney_pirate_kit_ships_port_props.manifest.json",
]


def read_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def norm(path: str) -> str:
    return path.replace("\\", "/")


def package_record(manifest: dict[str, Any], source_evidence: Path) -> dict[str, Any]:
    files = manifest.get("candidate_files", [])
    model_paths = [item["path"] for item in files if str(item["path"]).lower().endswith(".fbx")]
    preview_root = "Thumbnails" if "BuildingKit" in str(files[0]["path"]) else "Previews"
    return {
        "manifest_id": manifest["pack_id"],
        "manifest_path": norm(str(MANIFESTS[[m["pack_id"] for m in [read_json(p) for p in MANIFESTS]].index(manifest["pack_id"])])),
        "official_page": manifest["official_page"],
        "download_url": manifest["download_url"],
        "license_id": manifest["license_id"],
        "license_url": manifest["license_url"],
        "license_evidence_path": manifest["license_evidence_path"],
        "license_evidence_exists": (ROOT / manifest["license_evidence_path"]).is_file(),
        "archive_name": manifest["archive_name"],
        "archive_sha256": manifest["sha256"],
        "archive_sha256_recorded": len(manifest["sha256"]) == 64,
        "candidate_model_count": len(model_paths),
        "manifest_file_count": manifest["file_count"],
        "import_status": manifest["import_status"],
        "review_status": manifest["review_status"],
        "runtime_consumers": manifest.get("runtime_consumers", []),
        "source_evidence_path": norm(str(source_evidence.relative_to(ROOT))),
        "preview_directory_reference": preview_root,
    }


def classify(role: str) -> str:
    if role in {"building_opening_module", "building_shell_module", "building_modular_candidate", "building_drainage_module", "building_roof_module", "building_access_module", "port_building_module"}:
        return "port_structure_core"
    if role in {"port_platform_module", "port_dock_pier_module"}:
        return "dock_and_shipyard_core"
    if role in {"port_storage_prop", "port_flag_sign_prop", "port_work_prop"}:
        return "market_prop_core"
    if role.startswith("ship_") or role in {"boat_tender", "ship_weapon_prop"}:
        return "ship_visual_candidate"
    if role in {"coastal_environment_prop", "vegetation"}:
        return "out_of_scope_coastal_environment"
    return "unclassified_candidate"


def risk_flags(category: str, imported: dict[str, Any]) -> list[str]:
    flags: list[str] = []
    if imported.get("naming_valid") is False:
        flags.append("source_filename_not_snake_case")
    if imported.get("collider_count") == 0:
        flags.append("no_raw_collider")
    if imported.get("interaction_anchor_count") == 0:
        flags.append("no_raw_interaction_anchor")
    if imported.get("lod_group_count") == 0:
        flags.append("no_raw_lod_group")
    if not imported.get("bounds_valid", False):
        flags.append("invalid_bounds")
    if imported.get("material_count") == 0:
        flags.append("missing_material")
    if category == "port_structure_core":
        flags.extend([
            "grid_closure_unproven",
            "foundation_and_ground_fit_unproven",
            "door_window_proportion_unproven",
            "interior_use_unproven",
            "consumer_snap_dimensions_unproven",
        ])
    elif category == "dock_and_shipyard_core":
        flags.extend([
            "dock_deck_height_and_seam_unproven",
            "berth_clearance_and_draft_unproven",
            "bollard_guardrail_collision_unproven",
        ])
    elif category == "market_prop_core":
        flags.extend([
            "grab_place_pivot_unproven",
            "consumer_collision_unproven",
            "batching_instancing_requires_consumer_prefab",
        ])
    elif category == "ship_visual_candidate":
        flags.extend([
            "pivot_and_forward_axis_unproven",
            "deck_height_and_draft_unproven",
            "ship_collision_and_anchors_unproven",
            "ship_lod_unproven",
        ])
        if imported.get("animation_clip_count", 0) == 0:
            flags.append("no_ship_animation_clip_evidence")
    elif category == "out_of_scope_coastal_environment":
        flags.append("not_a_port_or_ship_visual_core_for_this_review")
    return sorted(set(flags))


def recommendation(category: str) -> dict[str, Any]:
    if category == "port_structure_core":
        return {
            "decision": "RECOMMEND_AS_EVIDENCE_ONLY",
            "d19_r01_b": "supports_candidate_selection_after_D18-S02-R01; not_directly_consumable_now",
            "d21": "secondary_port_shell_reference_only",
            "current_direct_runtime_consumption": False,
            "required_next_gate": "D18-S02-R01 must review/refresh intake; then D19-R01-B consumer Prefab preflight",
        }
    if category == "dock_and_shipyard_core":
        return {
            "decision": "RECOMMEND_AS_EVIDENCE_ONLY",
            "d19_r01_b": "supports_dock_shipyard_visual_gap_review_after_intake_gate",
            "d21": "supports_shipyard/dock visual context; not a ship-system asset",
            "current_direct_runtime_consumption": False,
            "required_next_gate": "consumer seam, height, collision, berth and draft preflight",
        }
    if category == "market_prop_core":
        return {
            "decision": "RECOMMEND_AS_EVIDENCE_ONLY",
            "d19_r01_b": "supports market/port prop gap review after intake gate",
            "d21": "only ship-adjacent props (cannon/paddle) belong to D21; market props remain D19",
            "current_direct_runtime_consumption": False,
            "required_next_gate": "consumer pivot, collision, scale and batching preflight",
        }
    if category == "ship_visual_candidate":
        return {
            "decision": "RECOMMEND_FOR_D21_REVIEW_ONLY",
            "d19_r01_b": "distant ship context only; not a formal scene replacement",
            "d21": "primary D21 hull/boat/mast/ship-prop evidence input after intake gate",
            "current_direct_runtime_consumption": False,
            "required_next_gate": "D21-S01 interfaces, D21-A01/A02 consumer Prefab and D18-S03 evidence",
        }
    return {
        "decision": "REJECT_FOR_THIS_CONSUMPTION_PACKET",
        "d19_r01_b": "not a port/shipyard/market gap fix; defer to D19-A01/A03 ownership",
        "d21": "not a D21 ship/port core dependency",
        "current_direct_runtime_consumption": False,
        "required_next_gate": "none in D18-A05-R01",
    }


def visual_gap_packet(visual: dict[str, Any]) -> list[dict[str, Any]]:
    by_area = {item["area"]: item for item in visual["areas"]}
    return [
        {
            "area": "port_general",
            "status": by_area["market_and_main_street"]["status"],
            "evidence": ["game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json", "screenshots/d19_shot_market.png"],
            "missing": ["high-detail facade variation", "dense ground/shoreline layering", "removal of blocky proxy read"],
            "historical_finding": by_area["market_and_main_street"]["findings"],
        },
        {
            "area": "shipyard",
            "status": by_area["dock_shipyard_berth"]["status"],
            "evidence": ["game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json", "screenshots/d19_shot_dock_overview.png"],
            "missing": ["denser shipyard-specific facade/workshop detail", "clear repair/rigging/ship-service visual language", "Player-camera proof"],
            "historical_finding": by_area["dock_shipyard_berth"]["findings"],
        },
        {
            "area": "dock_and_berth",
            "status": by_area["dock_shipyard_berth"]["status"],
            "evidence": ["game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json", "screenshots/d19_shot_dock_overview.png"],
            "missing": ["non-sparse dock edge detail", "ship/berth visual identity", "Player-camera proof"],
            "historical_finding": by_area["dock_shipyard_berth"]["findings"],
        },
        {
            "area": "market",
            "status": by_area["market_and_main_street"]["status"],
            "evidence": ["game/tools/results/D19/D19-S02-R01/D19_S02_R01_VISUAL_QA.json", "screenshots/d19_shot_market.png"],
            "missing": ["market massing detail at reviewed distance", "denser surrounding props/ground variation", "Player-camera proof"],
            "historical_finding": by_area["market_and_main_street"]["findings"],
        },
        {
            "area": "ship_visual",
            "status": "PARTIAL",
            "evidence": ["docs/game/design/common/handoffs/D18/D18-A05_HANDOFF.md", "game/tools/results/D18/D18-A05/D18_A05_IMPORT_AUDIT_SUMMARY.json"],
            "missing": ["formal Hull Prefab", "independent sail/rudder/anchor", "collision/LOD/interaction anchors", "Player-camera ship proof"],
            "historical_finding": ["Existing assets are raw static candidates with no runtime consumer; starting ship remains authoritative."],
        },
    ]


def main() -> int:
    manifests = [read_json(path) for path in MANIFESTS]
    manifest_by_path = {norm(str(path.relative_to(ROOT))): manifest for path, manifest in zip(MANIFESTS, manifests)}
    manifest_by_pack = {manifest["pack_id"]: manifest for manifest in manifests}
    hash_packet = read_json(HASH_PACKET)
    hash_by_path = {
        norm(item["repository_path"]): item
        for package in hash_packet["packages"]
        for item in package["files"]
    }
    all_hash_checked = 0
    all_hash_mismatches: list[str] = []
    all_missing_hash_files: list[str] = []
    for package in hash_packet["packages"]:
        for hash_item in package["files"]:
            hash_path = norm(hash_item["repository_path"])
            source_path = ROOT / hash_path
            if not source_path.is_file():
                all_missing_hash_files.append(hash_path)
                continue
            all_hash_checked += 1
            actual = hashlib.sha256(source_path.read_bytes()).hexdigest().upper()
            if actual != str(hash_item["sha256"]).upper():
                all_hash_mismatches.append(hash_path)
    import_result = read_json(IMPORT_RESULT)
    import_by_path = {"game/" + norm(item["path"]): item for item in import_result["assets"]}
    intake_catalog = read_json(INTAKE_CATALOG)
    intake_for_path = {
        norm(item["candidate_file_path"]): item
        for item in intake_catalog.get("eligible_entries", [])
    }
    visual = read_json(VISUAL_QA)

    items: list[dict[str, Any]] = []
    package_counts: dict[str, int] = {}
    category_counts: dict[str, int] = {}
    hash_checked = 0
    hash_mismatches: list[str] = []
    missing_files: list[str] = []
    import_missing: list[str] = []
    for manifest_path, manifest in zip(MANIFESTS, manifests):
        for candidate in manifest["candidate_files"]:
            candidate_path = norm(candidate["path"])
            package_id = manifest["pack_id"]
            imported = import_by_path.get(candidate_path, {})
            if not imported:
                import_missing.append(candidate_path)
            file_hash = hash_by_path.get(candidate_path, {})
            actual_sha256 = None
            source_path = ROOT / candidate_path
            if source_path.is_file():
                actual_sha256 = hashlib.sha256(source_path.read_bytes()).hexdigest().upper()
                hash_checked += 1
                if actual_sha256 != str(file_hash.get("sha256", "")).upper():
                    hash_mismatches.append(candidate_path)
            else:
                missing_files.append(candidate_path)
            category = classify(candidate["role"])
            package_counts[package_id] = package_counts.get(package_id, 0) + 1
            category_counts[category] = category_counts.get(category, 0) + 1
            imported_subset = {
                key: imported.get(key)
                for key in (
                    "source_exists", "importer_type", "main_asset_type", "mesh_count", "material_count",
                    "texture_count", "renderer_count", "collider_count", "interaction_anchor_count",
                    "lod_group_count", "animation_clip_count", "missing_script_count", "missing_reference_count",
                    "bounds_valid", "max_dimension", "naming_valid", "issues", "suggestions", "settings",
                )
                if key in imported
            }
            items.append({
                "candidate_id": candidate["target_id"],
                "package_id": package_id,
                "manifest_path": norm(str(manifest_path.relative_to(ROOT))),
                "candidate_file_path": candidate_path,
                "source_url": manifest["official_page"],
                "download_url": manifest["download_url"],
                "license_id": manifest["license_id"],
                "license_evidence_path": manifest["license_evidence_path"],
                "license_evidence_exists": (ROOT / manifest["license_evidence_path"]).is_file(),
                "archive_sha256": manifest["sha256"],
                "file_sha256_declared": file_hash.get("sha256"),
                "file_sha256_actual": actual_sha256,
                "file_sha256_match": actual_sha256 is not None and actual_sha256 == str(file_hash.get("sha256", "")).upper(),
                "role": candidate["role"],
                "category": category,
                "import_evidence_path": norm(str(IMPORT_RESULT.relative_to(ROOT))),
                "import_status": manifest["import_status"],
                "import_record": imported_subset,
                "intake_eligible_now": candidate_path in intake_for_path,
                "risk_flags": risk_flags(category, imported),
                "recommendation": recommendation(category),
            })

    package_records = [package_record(manifest, PACK_ROOT / "D18_A05_SOURCE_EVIDENCE.md") for manifest in manifests]
    category_records = []
    category_descriptions = {
        "port_structure_core": "Building Kit shell/opening/roof/access modules plus Pirate Kit port structures; candidate evidence for D19-R01-B, not direct intake.",
        "dock_and_shipyard_core": "Platform/dock modules; possible D19-R01-B dock/shipyard gap evidence, with D19-A02/D19-R01-B consumer checks.",
        "market_prop_core": "Barrel/crate/bottle/chest/flag/paddle/shovel props; market/port dressing candidates, not inventory/interaction facts.",
        "ship_visual_candidate": "Hull, boat, distant silhouette, mast/ropes and ship weapon candidates; primary D21 review input, not drivable/animated content.",
        "sail_rig_gap": "No independent sail FBX; mast/ropes are only partial rig evidence.",
        "rudder_anchor_gap": "No independent rudder or anchor FBX and no raw interaction anchors.",
        "out_of_scope_coastal_environment": "Pirate Kit grass/palm/rock/sand props; defer to D19-A01/A03 rather than use as port/ship core evidence.",
    }
    for category, description in category_descriptions.items():
        category_records.append({
            "category": category,
            "description": description,
            "candidate_item_count": category_counts.get(category, 0),
            "direct_intake_eligible_item_count": sum(1 for item in items if item["category"] == category and item["intake_eligible_now"]),
            "consumer_boundary": recommendation(category),
            "evidence_paths": [
                norm(str(HASH_PACKET.relative_to(ROOT))),
                norm(str(IMPORT_RESULT.relative_to(ROOT))),
                norm(str(MANIFEST_AUDIT.relative_to(ROOT))),
                norm(str(INTAKE_AUDIT.relative_to(ROOT))),
            ],
        })

    review = {
        "task": "D18-A05-R01",
        "review_date": "2026-08-15",
        "status": "PARTIAL",
        "read_only": True,
        "conclusion": "Evidence is complete for 151 Candidate/ImportVerified models, but no ShipsPortProps item is currently intake_eligible; raw assets do not prove consumer Prefab, collision, anchors, LOD or ship function.",
        "prerequisites": {
            "D18-S02-v2": "PASS",
            "D18-A05": "PARTIAL",
            "D19-S01": "PASS",
            "D19-S02-R01": "PARTIAL",
        },
        "scope_protection": {
            "initial_harbor_modified_by_r01": False,
            "ship_system_modified_by_r01": False,
            "formal_prefab_modified_by_r01": False,
            "global_intake_catalog_modified_by_r01": False,
            "registry_save_contracts_server_web_modified_by_r01": False,
            "candidate_sources_modified_by_r01": False,
        },
        "real_review_counts": {
            "manifest_count": len(manifests),
            "model_file_count": len(items),
            "preview_file_count": len([item for package in hash_packet["packages"] for item in package["files"] if item["kind"] == "preview"]),
            "hash_files_checked": all_hash_checked,
            "model_hash_files_checked": hash_checked,
            "hash_mismatch_count": len(all_hash_mismatches),
            "missing_hashed_file_count": len(all_missing_hash_files),
            "missing_candidate_file_count": len(missing_files),
            "import_asset_records": len(import_result["assets"]),
            "import_missing_asset_records": len(import_missing),
            "direct_intake_eligible_items": sum(1 for item in items if item["intake_eligible_now"]),
        },
        "source_packages": package_records,
        "category_counts": category_counts,
        "category_recommendations": category_records,
        "current_white_block_visual_gaps": visual_gap_packet(visual),
        "historical_visual_gate": {
            "source": norm(str(VISUAL_QA.relative_to(ROOT))),
            "status": visual["manual_review"]["overall_visual_gate"],
            "player_view_evidence": visual["player_view_evidence"],
            "blocking_gates": visual["blocking_gates"],
        },
        "audit_evidence": {
            "manifest_audit": read_json(MANIFEST_AUDIT) if MANIFEST_AUDIT.is_file() else {"status": "NOT_FOUND"},
            "intake_audit": read_json(INTAKE_AUDIT) if INTAKE_AUDIT.is_file() else {"status": "NOT_FOUND"},
            "existing_import_summary": read_json(IMPORT_SUMMARY),
            "existing_import_result": {
                "path": norm(str(IMPORT_RESULT.relative_to(ROOT))),
                "status": import_result["status"],
                "audit_mode": import_result["audit_mode"],
                "read_only": import_result["read_only"],
                "model_file_count": import_result["model_file_count"],
                "errors": import_result["error_count"],
                "warnings": import_result["warning_count"],
            },
            "global_intake_relevant_exclusions": [
                item for item in intake_catalog.get("excluded_manifests", [])
                if "ships_port_props" in item.get("manifest_path", "")
            ],
        },
        "known_gaps": [
            "Both Kenney ships/port manifests remain review_status=Candidate and are absent from current intake_eligible entries.",
            "D19-R01-B cannot directly use these external candidates until D18-S02-R01 and the intake gate provide current eligibility evidence.",
            "D21 still lacks independent sail, rudder and anchor evidence; mast/ropes do not close the sail-rig gap.",
            "Raw FBX has 0 colliders, 0 interaction anchors and 0 LODGroups; no formal Hull/Building/Prop Prefab is proven.",
            "D19-S02-R01 still has blocky/sparse port, shipyard, dock and market presentation and no Player-camera proof.",
            "No lamp, fishing gear, explicit sign, bollard, rudder, anchor or sail mesh is present in the two reviewed packages.",
        ],
        "items": items,
    }
    machine = {
        "task": "D18-A05-R01",
        "status": "PARTIAL",
        "read_only": True,
        "review_packet": norm(str(REVIEW_JSON.relative_to(ROOT))),
        "model_file_count": len(items),
        "preview_file_count": review["real_review_counts"]["preview_file_count"],
        "hash_files_checked": all_hash_checked,
        "model_hash_files_checked": hash_checked,
        "hash_mismatch_count": len(all_hash_mismatches),
        "missing_hashed_file_count": len(all_missing_hash_files),
        "missing_candidate_file_count": len(missing_files),
        "import_asset_records": len(import_result["assets"]),
        "import_errors": import_result["error_count"],
        "import_warnings": import_result["warning_count"],
        "manifest_audit_status": read_json(MANIFEST_AUDIT).get("status", "NOT_FOUND") if MANIFEST_AUDIT.is_file() else "NOT_FOUND",
        "intake_audit_status": read_json(INTAKE_AUDIT).get("status", "NOT_FOUND") if INTAKE_AUDIT.is_file() else "NOT_FOUND",
        "ships_port_intake_eligible_now": 0,
        "category_counts": category_counts,
        "hash_mismatches": all_hash_mismatches,
        "missing_hashed_files": all_missing_hash_files,
        "missing_files": missing_files,
        "missing_import_records": import_missing,
        "protected_scope": review["scope_protection"],
        "next_owner": {
            "D18-S02-R01": "review packet and decide intake eligibility; do not infer approval",
            "D19-R01-B": "consume only after current intake gate; use project-owned fallback otherwise",
            "D21-A01/A02": "review ship candidates and close hull/rigging/anchor Prefab evidence",
            "D18-S03": "decide LocalDevelopmentReady only after runtime consumer/save/rollback evidence",
        },
    }
    REVIEW_JSON.parent.mkdir(parents=True, exist_ok=True)
    REVIEW_JSON.write_text(json.dumps(review, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    MACHINE_JSON.write_text(json.dumps(machine, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(machine, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
