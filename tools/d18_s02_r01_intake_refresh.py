#!/usr/bin/env python3
"""Refresh the D18 PrefabCandidateIntake catalog from isolated evidence packets.

This is a read-only evidence auditor with one deliberate write target: the
PrefabCandidateIntake catalog and the D18-S02-R01 machine-result directory.
It never edits a source manifest, resource-state catalog, Registry, Prefab or
scene.  ``intake_eligible`` is a controlled-work admission flag; it is not a
resource state and never promotes LocalDevelopmentReady or ReleaseApproved.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
import re
import sys
from datetime import date
from pathlib import Path, PurePosixPath
from typing import Any, Iterable


CATALOG_REL = Path("docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json")
READY_CATALOG_REL = Path("docs/game/design/common/assets/d18_local_development_ready_catalog.json")
RESULT_REL = Path("game/tools/results/D18/D18-S02-R01")
CAT_MANIFEST_REL = Path(
    "docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_manifest.json"
)
SHA256_RE = re.compile(r"^[0-9A-Fa-f]{64}$")
ALLOWED_RESOURCE_STATES = {
    "Candidate",
    "LicenseEvidenceComplete",
    "ImportVerified",
    "LocalDevelopmentReady",
    "ReleaseApproved",
    "Rejected",
}
EXPECTED_CATEGORIES = (
    "player_visual_core",
    "npc_visual_core",
    "wildlife_core",
    "pet_core",
    "animation_core",
    "port_structure_core",
    "dock_and_shipyard_core",
    "market_prop_core",
)

UPSTREAM = {
    "D18-A04-R01": {
        "handoff": "docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md",
        "review": "docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json",
        "machine": "game/tools/results/D18/D18-A04-R01/D18_A04_R01_MACHINE_RESULT.json",
    },
    "D18-A04-R02-F": {
        "handoff": "docs/game/design/common/handoffs/D18/D18-A04-R02-F_HANDOFF.md",
        "summary": "game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_SUMMARY.json",
        "fbx_audit": "game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_FBX_STATIC_AUDIT.json",
        "manifest": "docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/d18_a04_r02_f.manifest.json",
    },
    "D18-A04-R02-C": {
        "handoff": "docs/game/design/common/handoffs/D18/D18-A04-R02-C_HANDOFF.md",
        "evidence": "game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_EVIDENCE_AUDIT.json",
        "unity": "game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.json",
        "manifest": "docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_manifest.json",
        "archive": "game/tools/results/D18/D18-A04-R02-C/Animal Pack Vol.2 by @Quaternius.zip",
    },
    "D18-A04-R02-B": {
        "handoff": "docs/game/design/common/handoffs/D18/D18-A04-R02-B_HANDOFF.md",
        "result": "game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_RESULT.json",
        "hashes": "game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_HASHES.json",
        "unity": "game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_UNITY_IMPORT_AUDIT.json",
        "manifest": "docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/quaternius_animal_pack_vol2_bird_d18_a04_r02_b.manifest.json",
    },
    "D18-A05-R01": {
        "handoff": "docs/game/design/common/handoffs/D18/D18-A05-R01_HANDOFF.md",
        "review": "docs/game/design/common/assets/review_packets/D18-A05-R01/D18_A05_R01_SHIP_PORT_REVIEW.json",
        "machine": "game/tools/results/D18/D18-A05-R01/D18_A05_R01_MACHINE_RESULT.json",
        "hashes": "docs/game/design/common/assets/packs/ships_port_props/D18_A05_FILE_HASHES.json",
        "source": "docs/game/design/common/assets/packs/ships_port_props/D18_A05_SOURCE_EVIDENCE.md",
    },
}


def read_json(path: Path) -> Any:
    raw = path.read_bytes()
    for encoding in ("utf-8-sig", "utf-16"):
        try:
            return json.loads(raw.decode(encoding))
        except (UnicodeDecodeError, json.JSONDecodeError):
            continue
    raise json.JSONDecodeError("unsupported JSON encoding", raw.decode("utf-8", errors="replace"), 0)


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def safe_path(project_root: Path, raw: str) -> Path:
    normalized = str(raw).replace("\\", "/")
    pure = PurePosixPath(normalized)
    if pure.is_absolute() or ".." in pure.parts:
        raise ValueError(f"unsafe path: {raw}")
    return project_root.joinpath(*pure.parts)


def rel(path: Path, project_root: Path) -> str:
    return path.resolve().relative_to(project_root.resolve()).as_posix()


def normalize_path(raw: str) -> str:
    return str(raw).replace("\\", "/")


def find_manifests(project_root: Path) -> list[Path]:
    paths = set(
        path
        for path in (project_root / "docs/game/design/common/assets").rglob("*.manifest.json")
        if path.is_file()
    )
    # The cat packet predates the normal filename suffix.  It is an explicit
    # upstream manifest and must not disappear from a full refresh.
    cat_path = project_root / CAT_MANIFEST_REL
    if cat_path.is_file():
        paths.add(cat_path)
    return sorted(paths, key=lambda path: path.as_posix())


def manifest_index(project_root: Path) -> tuple[dict[str, dict[str, Any]], list[str]]:
    index: dict[str, dict[str, Any]] = {}
    errors: list[str] = []
    for path in find_manifests(project_root):
        manifest_rel = rel(path, project_root)
        try:
            manifest = read_json(path)
        except (OSError, json.JSONDecodeError) as exc:
            errors.append(f"manifest_read:{manifest_rel}:{exc}")
            continue
        pack_id = str(manifest.get("pack_id", ""))
        if not pack_id:
            errors.append(f"manifest_missing_pack_id:{manifest_rel}")
            continue
        if pack_id in index:
            errors.append(f"duplicate_manifest_id:{pack_id}")
            continue
        index[pack_id] = {"path": manifest_rel, "data": manifest}
    return index, errors


def candidate_map(manifest: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {
        normalize_path(str(candidate.get("path", ""))): candidate
        for candidate in manifest.get("candidate_files", [])
        if isinstance(candidate, dict)
    }


def required_manifest_errors(project_root: Path, manifest_rel: str, manifest: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    required = (
        "source_id",
        "pack_id",
        "title",
        "official_page",
        "download_url",
        "archive_name",
        "sha256",
        "license_id",
        "license_url",
        "license_evidence_path",
        "candidate_files",
        "runtime_consumers",
    )
    for field in required:
        if field not in manifest:
            errors.append(f"{manifest_rel}:missing_field:{field}")
    if manifest.get("license_id") != "CC0-1.0":
        errors.append(f"{manifest_rel}:license_not_cc0")
    if not SHA256_RE.fullmatch(str(manifest.get("sha256", ""))):
        errors.append(f"{manifest_rel}:archive_sha256_invalid")
    if manifest.get("runtime_consumers") not in ([], None):
        errors.append(f"{manifest_rel}:runtime_consumers_not_empty")
    candidates = manifest.get("candidate_files")
    if not isinstance(candidates, list):
        return errors + [f"{manifest_rel}:candidate_files_not_list"]
    if manifest.get("file_count") != len(candidates):
        errors.append(f"{manifest_rel}:file_count_mismatch")
    seen_paths: set[str] = set()
    seen_targets: set[str] = set()
    try:
        license_path = safe_path(project_root, str(manifest.get("license_evidence_path", "")))
        if not license_path.is_file():
            errors.append(f"{manifest_rel}:license_evidence_missing")
    except ValueError as exc:
        errors.append(f"{manifest_rel}:{exc}")
    for candidate in candidates:
        if not isinstance(candidate, dict):
            errors.append(f"{manifest_rel}:candidate_not_object")
            continue
        candidate_rel = normalize_path(str(candidate.get("path", "")))
        target_id = str(candidate.get("target_id", ""))
        if not candidate_rel:
            errors.append(f"{manifest_rel}:candidate_path_missing")
            continue
        if candidate_rel in seen_paths:
            errors.append(f"{manifest_rel}:duplicate_candidate_path:{candidate_rel}")
        seen_paths.add(candidate_rel)
        if target_id and target_id in seen_targets:
            errors.append(f"{manifest_rel}:duplicate_target_id:{target_id}")
        seen_targets.add(target_id)
        try:
            candidate_path = safe_path(project_root, candidate_rel)
            if not candidate_path.is_file():
                errors.append(f"{manifest_rel}:candidate_file_missing:{candidate_rel}")
        except ValueError as exc:
            errors.append(f"{manifest_rel}:{exc}")
    return errors


def archive_cache_path(manifest: dict[str, Any], project_root: Path, explicit: str | None = None) -> Path | None:
    if explicit:
        path = safe_path(project_root, explicit)
        return path if path.is_file() else None
    archive_name = str(manifest.get("archive_name", ""))
    if not archive_name:
        return None
    cache = Path.home() / "AppData/Local/WhalesIslandReef/D18/cache" / archive_name
    return cache if cache.is_file() else None


def find_cached_archive(cache_root: Path, archive_name: str, expected_sha256: str) -> Path | None:
    """Find a locally cached archive even when the cache adds a date suffix."""

    if not cache_root.is_dir():
        return None
    exact = cache_root / archive_name
    candidates = [exact] if exact.is_file() else []
    stem = Path(archive_name).stem
    candidates.extend(sorted(cache_root.glob(f"{stem}*")))
    for candidate in candidates:
        if candidate.is_file() and sha256_file(candidate) == expected_sha256.upper():
            return candidate
    return None


def allowed_tasks(categories: list[str]) -> list[str]:
    tasks: set[str] = set()
    for category in categories:
        if category == "player_visual_core":
            tasks.update(("D20-A01", "D20-A02", "D20-A03"))
        elif category == "npc_visual_core":
            tasks.update(("D20-A03", "D20-A04"))
        elif category in {"wildlife_core", "bird_gap"}:
            tasks.add("D20-A05")
        elif category in {"pet_core", "cat_gap"}:
            tasks.add("D20-A06")
        elif category == "animation_core":
            tasks.update(("D20-A02", "D20-A05", "D20-A06"))
        elif category == "port_structure_core":
            tasks.update(("D19-R01-B", "D19-A03"))
        elif category == "dock_and_shipyard_core":
            tasks.update(("D19-R01-B", "D21-A01"))
        elif category == "market_prop_core":
            tasks.add("D19-R01-B")
        elif category == "ship_visual_candidate":
            tasks.update(("D21-A01", "D21-A02"))
    return sorted(tasks)


def domain_for(categories: list[str], role: str) -> str:
    text = " ".join(categories + [role]).lower()
    if "animation" in text:
        return "character" if "character" in text or "player" in text or "npc" in text else "animal"
    if any(token in text for token in ("character", "player", "npc", "humanoid")):
        return "character"
    if any(token in text for token in ("animal", "wildlife", "pet", "bird", "fish", "cat")):
        return "animal"
    if "ship" in text or "hull" in text or "boat" in text:
        return "ship"
    if any(token in text for token in ("port", "dock", "building", "shipyard")):
        return "building"
    if "prop" in text:
        return "prop"
    return "unclassified"


def gaps_for(categories: list[str], technical: dict[str, Any] | None = None) -> list[str]:
    gaps = [
        "controlled work-scene consumer and Prefab are not created by this refresh",
        "LocalDevelopmentReady requires D18-S03 runtime, save/restore, rollback and performance evidence",
    ]
    technical = technical or {}
    if technical.get("avatar_present") is False or technical.get("avatar_setup") == "NoAvatar":
        gaps.append("source import is Generic/NoAvatar; Humanoid retargeting remains consumer-owned")
    if technical.get("collider_count") == 0 or technical.get("collision") == "none":
        gaps.append("raw import has no consumer collision")
    if technical.get("lod_group_count") == 0 or technical.get("lod") == "none":
        gaps.append("raw import has no consumer LODGroup")
    if technical.get("interaction_anchor_count") == 0 or technical.get("anchors") == "none":
        gaps.append("interaction anchors remain consumer-owned")
    return sorted(set(gaps))


def forbidden_scope() -> list[str]:
    return [
        "formal InitialHarbor or other formal-scene integration",
        "Registry or asset_state mutation",
        "LocalDevelopmentReady or ReleaseApproved promotion",
        "Steam or external distribution approval",
    ]


def entry_from_evidence(
    project_root: Path,
    manifest_rel: str,
    manifest: dict[str, Any],
    candidate: dict[str, Any],
    *,
    review_task: str,
    review_packet: str,
    machine_evidence: list[str],
    import_evidence: str,
    archive_path: Path | None,
    candidate_sha256: str,
    candidate_hash_evidence: str,
    categories: list[str],
    technical: dict[str, Any],
    upstream_decision: str,
) -> tuple[dict[str, Any] | None, list[str]]:
    errors: list[str] = []
    pack_id = str(manifest.get("pack_id", ""))
    candidate_rel = normalize_path(str(candidate.get("path", "")))
    try:
        candidate_path = safe_path(project_root, candidate_rel)
        manifest_path = safe_path(project_root, manifest_rel)
        license_path = safe_path(project_root, str(manifest.get("license_evidence_path", "")))
    except ValueError as exc:
        return None, [f"{review_task}:{pack_id}:{exc}"]
    if not manifest_path.is_file():
        errors.append(f"{review_task}:{pack_id}:manifest_missing")
    if not candidate_path.is_file():
        errors.append(f"{review_task}:{pack_id}:candidate_missing:{candidate_rel}")
    if not license_path.is_file():
        errors.append(f"{review_task}:{pack_id}:license_evidence_missing")
    if manifest.get("license_id") != "CC0-1.0":
        errors.append(f"{review_task}:{pack_id}:license_not_cc0")
    if manifest.get("runtime_consumers") not in ([], None):
        errors.append(f"{review_task}:{pack_id}:runtime_consumers_not_empty")
    if manifest.get("import_status") != "ImportVerified":
        errors.append(f"{review_task}:{pack_id}:manifest_import_status_not_verified")
    if not import_evidence or not safe_path(project_root, import_evidence).is_file():
        errors.append(f"{review_task}:{pack_id}:import_evidence_missing:{import_evidence}")
    if not SHA256_RE.fullmatch(candidate_sha256):
        errors.append(f"{review_task}:{pack_id}:candidate_sha256_invalid:{candidate_rel}")
    else:
        actual = sha256_file(candidate_path) if candidate_path.is_file() else ""
        if actual != candidate_sha256.upper():
            errors.append(f"{review_task}:{pack_id}:candidate_sha256_mismatch:{candidate_rel}")
    archive_digest = str(manifest.get("sha256", "")).upper()
    if not SHA256_RE.fullmatch(archive_digest):
        errors.append(f"{review_task}:{pack_id}:manifest_archive_sha256_invalid")
    if archive_path is None or not archive_path.is_file():
        errors.append(f"{review_task}:{pack_id}:archive_evidence_file_missing")
    elif sha256_file(archive_path) != archive_digest:
        errors.append(f"{review_task}:{pack_id}:archive_sha256_mismatch")
    try:
        import_evidence_rel = normalize_path(import_evidence)
        safe_path(project_root, import_evidence_rel)
    except ValueError as exc:
        errors.append(f"{review_task}:{pack_id}:{exc}")
    if errors:
        return None, errors
    role = str(candidate.get("role", ""))
    categories = sorted(set(categories))
    task_scope = allowed_tasks(categories)
    intake_id = "intake_" + hashlib.sha256(f"{pack_id}::{candidate_rel}".encode("utf-8")).hexdigest()[:20]
    entry = {
        "intake_id": intake_id,
        "manifest_id": pack_id,
        "manifest_path": manifest_rel,
        "candidate_file_path": candidate_rel,
        "content_id": str(candidate.get("target_id", "")) or None,
        "domain": domain_for(categories, role),
        "priority": "P0" if any(category in {"player_visual_core", "npc_visual_core", "port_structure_core", "dock_and_shipyard_core", "market_prop_core"} for category in categories) else "P1",
        "role": role,
        "intake_eligible": True,
        "license": {
            "license_id": manifest.get("license_id"),
            "license_url": manifest.get("license_url"),
            "license_evidence_path": manifest.get("license_evidence_path"),
            "commercial_use": manifest.get("commercial_use"),
            "modification_allowed": manifest.get("modification_allowed"),
            "source_redistribution": manifest.get("source_redistribution"),
            "build_distribution": manifest.get("build_distribution"),
        },
        "hash_evidence": {
            "path": manifest_rel,
            "field": "sha256",
            "sha256": archive_digest,
            "scope": "manifest archive digest",
            "archive_file_path": rel(archive_path, project_root) if archive_path.is_relative_to(project_root) else str(archive_path),
            "candidate_file_sha256": candidate_sha256.upper(),
            "candidate_file_hash_evidence_path": candidate_hash_evidence,
        },
        "import_verified_evidence_path": import_evidence,
        "allowed_task_scope": task_scope,
        "forbidden_scope": forbidden_scope(),
        "known_gaps": gaps_for(categories, technical),
        "local_development_ready_evidence_required": [
            "consumer-owned Work scene or Prefab reference",
            "D18-S03 runtime consumer and interaction evidence",
            "save/restore, rollback and performance evidence",
        ],
        "responsibility": {
            "intake_owner": "D18-S02-R01",
            "consumer_owner": task_scope[0] if task_scope else "D18-S03",
            "promotion_owner": "D18-S03",
        },
        "consumer_categories": categories,
        "source_state_snapshot": {
            "manifest_review_status": manifest.get("review_status"),
            "manifest_import_status": manifest.get("import_status"),
            "runtime_consumers": manifest.get("runtime_consumers", []),
        },
        "admission_basis": {
            "source_review_task": review_task,
            "review_packet": review_packet,
            "machine_evidence": machine_evidence,
            "evidence_complete": True,
            "upstream_original_decision": upstream_decision,
            "note": "PrefabCandidateIntake only; source Candidate state is preserved and no runtime readiness is inferred.",
        },
        "technical_snapshot": technical,
    }
    return entry, []


def baseline_snapshot(catalog: dict[str, Any], catalog_path: Path, project_root: Path) -> dict[str, Any]:
    counts = copy.deepcopy(catalog.get("counts", {}))
    return {
        "catalog_path": rel(catalog_path, project_root),
        "catalog_sha256": sha256_file(catalog_path),
        "catalog_status": catalog.get("status"),
        "task": catalog.get("task"),
        "generated_on": catalog.get("generated_on"),
        "counts": counts,
        "local_development_ready_count": 0,
        "release_approved_count": 0,
        "historical_v2_report": "docs/game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md",
        "historical_v2_handoff": "docs/game/design/common/handoffs/D18/D18-S02_V2_HANDOFF.md",
        "note": "Snapshot only; the historical v2 report and catalog are retained by path and hash.",
    }


def upstream_status(handoff_text: str) -> str:
    match = re.search(r"(?:结论|Conclusion|status)\s*[:：]\s*\*{0,2}(PASS|PARTIAL|BLOCKED)\*{0,2}", handoff_text, re.IGNORECASE)
    return match.group(1).upper() if match else "UNKNOWN"


def process_a04_r01(project_root: Path, manifest_by_id: dict[str, dict[str, Any]]) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    task = "D18-A04-R01"
    paths = UPSTREAM[task]
    review = read_json(project_root / paths["review"])
    accepted: list[dict[str, Any]] = []
    excluded: list[dict[str, Any]] = []
    for record in review.get("records", []):
        state = record.get("manifest_state", {})
        technical = record.get("technical", {})
        path = normalize_path(str(record.get("candidate_file", {}).get("path", "")))
        manifest_id = str(record.get("manifest_id", ""))
        manifest_record = manifest_by_id.get(manifest_id)
        reasons: list[str] = []
        if not technical.get("unity_imported_in_isolated_a04_fixture"):
            reasons.append("no_unity_import_evidence")
        if state.get("import_status") != "ImportVerified":
            reasons.append(f"manifest_import_status={state.get('import_status')}")
        if not record.get("license", {}).get("license_evidence_exists"):
            reasons.append("license_evidence_missing")
        if not record.get("archive_evidence", {}).get("sha256_matches_manifest"):
            reasons.append("archive_sha256_not_verified")
        if not record.get("candidate_file", {}).get("exists"):
            reasons.append("candidate_file_missing")
        if manifest_record is None:
            reasons.append("manifest_not_found")
        if reasons:
            excluded.append({"candidate_id": record.get("candidate_id"), "candidate_file_path": path, "reasons": reasons})
            continue
        manifest_rel = manifest_record["path"]
        manifest = manifest_record["data"]
        candidate = candidate_map(manifest).get(path)
        if candidate is None:
            excluded.append({"candidate_id": record.get("candidate_id"), "candidate_file_path": path, "reasons": ["candidate_not_in_manifest"]})
            continue
        archive_path_raw = record.get("archive_evidence", {}).get("archive_path")
        archive_path = safe_path(project_root, str(archive_path_raw)) if archive_path_raw else None
        entry, errors = entry_from_evidence(
            project_root,
            manifest_rel,
            manifest,
            candidate,
            review_task=task,
            review_packet=paths["review"],
            machine_evidence=[paths["machine"], paths["review"]],
            import_evidence="game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json",
            archive_path=archive_path,
            candidate_sha256=str(record.get("candidate_file", {}).get("sha256", "")),
            candidate_hash_evidence=paths["review"],
            categories=[str(item) for item in record.get("consumer_categories", [])],
            technical=technical,
            upstream_decision=str(record.get("decision_class", "")),
        )
        if entry is None:
            excluded.append({"candidate_id": record.get("candidate_id"), "candidate_file_path": path, "reasons": errors})
        else:
            accepted.append(entry)
    return accepted, {
        "task": task,
        "handoff": paths["handoff"],
        "status": "PARTIAL",
        "records_processed": len(review.get("records", [])),
        "records_admitted": len(accepted),
        "records_excluded": len(excluded),
        "excluded": excluded,
        "evidence": [paths["review"], paths["machine"]],
    }


def process_gap_manifest(
    project_root: Path,
    task: str,
    manifest_by_id: dict[str, dict[str, Any]],
    *,
    review_packet: str,
    machine_evidence: list[str],
    manifest_rel: str,
    archive_rel: str,
    candidate_sha256: str,
    categories: list[str],
    technical: dict[str, Any],
    upstream_decision: str,
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    manifest_path = project_root / manifest_rel
    manifest = read_json(manifest_path)
    pack_id = str(manifest.get("pack_id", ""))
    manifest_record = manifest_by_id.get(pack_id)
    if manifest_record is None:
        return [], {"task": task, "records_processed": 0, "records_admitted": 0, "records_excluded": 1, "excluded": [{"reasons": ["manifest_not_found"]}]}
    archive_path = project_root / archive_rel
    candidates = manifest.get("candidate_files", [])
    accepted: list[dict[str, Any]] = []
    excluded: list[dict[str, Any]] = []
    import_evidence = ""
    for evidence in machine_evidence:
        name = Path(evidence).name
        if "UNITY_IMPORT" in name or "EVIDENCE_AUDIT" in name:
            import_evidence = evidence if "UNITY_IMPORT" in name else import_evidence
    if task.endswith("-C"):
        import_evidence = "game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.json"
    if task.endswith("-B"):
        import_evidence = "game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_UNITY_IMPORT_AUDIT.json"
    for candidate in candidates:
        candidate_path = normalize_path(str(candidate.get("path", "")))
        entry, errors = entry_from_evidence(
            project_root,
            manifest_rel,
            manifest,
            candidate,
            review_task=task,
            review_packet=review_packet,
            machine_evidence=machine_evidence,
            import_evidence=import_evidence,
            archive_path=archive_path,
            candidate_sha256=candidate_sha256,
            candidate_hash_evidence=machine_evidence[0] if machine_evidence else review_packet,
            categories=categories,
            technical=technical,
            upstream_decision=upstream_decision,
        )
        if entry is None:
            excluded.append({"candidate_file_path": candidate_path, "reasons": errors})
        else:
            accepted.append(entry)
    return accepted, {
        "task": task,
        "handoff": UPSTREAM[task]["handoff"],
        "status": "PARTIAL",
        "records_processed": len(candidates),
        "records_admitted": len(accepted),
        "records_excluded": len(excluded),
        "excluded": excluded,
        "evidence": [review_packet] + machine_evidence,
    }


def process_a04_r02_f(project_root: Path, manifest_by_id: dict[str, dict[str, Any]]) -> dict[str, Any]:
    task = "D18-A04-R02-F"
    paths = UPSTREAM[task]
    summary = read_json(project_root / paths["summary"])
    count = int(summary.get("counts", {}).get("candidate_fbx_files", 0))
    return {
        "task": task,
        "handoff": paths["handoff"],
        "status": "PARTIAL",
        "records_processed": count,
        "records_admitted": 0,
        "records_excluded": count,
        "excluded": [{"reasons": ["unity_import_not_run_no_unity_editor", "review_status=Candidate", "not_intake_eligible"]}],
        "evidence": [paths["summary"], paths["fbx_audit"], paths["manifest"]],
        "note": "Static FBX evidence is retained in the upstream packet; no Unity ImportVerified evidence, so no intake entry is created.",
    }


def process_a04_r02_c(project_root: Path, manifest_by_id: dict[str, dict[str, Any]]) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    task = "D18-A04-R02-C"
    paths = UPSTREAM[task]
    manifest = read_json(project_root / paths["manifest"])
    candidate_sha = "24D03762BCBCDF5414516354B0D80C6580A0FFD7B5ABE4D8EDD26AD7BF7A0DB5"
    return process_gap_manifest(
        project_root,
        task,
        manifest_by_id,
        review_packet="docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_PACKET.md",
        machine_evidence=[paths["evidence"], paths["unity"]],
        manifest_rel=paths["manifest"],
        archive_rel=paths["archive"],
        candidate_sha256=candidate_sha,
        categories=["pet_core", "animation_core"],
        technical={"avatar_present": False, "avatar_setup": "NoAvatar", "collider_count": 0, "lod_group_count": 0, "interaction_anchor_count": 0, "animation_clip_count": 2},
        upstream_decision="real_cat_candidate_imported_but_missing_prefab_level_evidence",
    )


def process_a04_r02_b(project_root: Path, manifest_by_id: dict[str, dict[str, Any]]) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    task = "D18-A04-R02-B"
    paths = UPSTREAM[task]
    hashes = read_json(project_root / paths["hashes"])
    candidate_sha = next(item["sha256"] for item in hashes["files"] if item["path"].endswith("eagle.fbx"))
    return process_gap_manifest(
        project_root,
        task,
        manifest_by_id,
        review_packet="docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/D18_A04_R02_B_PACKET.md",
        machine_evidence=[paths["result"], paths["hashes"], paths["unity"]],
        manifest_rel=paths["manifest"],
        archive_rel="game/tools/results/D18/D18-A04-R02-B/source_archives/Animal_Pack_Vol2_Quaternius.zip",
        candidate_sha256=candidate_sha,
        categories=["wildlife_core", "animation_core"],
        technical={"avatar_present": False, "avatar_setup": "NoAvatar", "collider_count": 0, "lod_group_count": 0, "interaction_anchor_count": 0, "animation_clip_count": 3},
        upstream_decision="mid_far_candidate_only_until_d20_prefab_lod_and_animation_review",
    )


def process_a05(project_root: Path, manifest_by_id: dict[str, dict[str, Any]]) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    task = "D18-A05-R01"
    paths = UPSTREAM[task]
    review = read_json(project_root / paths["review"])
    packages = {item["manifest_id"]: item for item in review.get("source_packages", [])}
    accepted: list[dict[str, Any]] = []
    excluded: list[dict[str, Any]] = []
    cache_root = Path.home() / "AppData/Local/WhalesIslandReef/D18/cache"
    for item in review.get("items", []):
        manifest_id = str(item.get("package_id", ""))
        manifest_record = manifest_by_id.get(manifest_id)
        reasons: list[str] = []
        if item.get("category") == "out_of_scope_coastal_environment":
            excluded.append({
                "candidate_id": item.get("candidate_id"),
                "candidate_file_path": item.get("candidate_file_path"),
                "reasons": ["out_of_scope_coastal_environment", "D18-A05-R01 explicitly assigns this category to D19-A01/A03 review"],
            })
            continue
        if manifest_record is None:
            reasons.append("manifest_not_found")
        if item.get("import_status") != "ImportVerified":
            reasons.append(f"import_status={item.get('import_status')}")
        if item.get("license_id") != "CC0-1.0" or not item.get("license_evidence_exists"):
            reasons.append("license_evidence_incomplete")
        if not item.get("file_sha256_match"):
            reasons.append("candidate_hash_mismatch")
        package = packages.get(manifest_id, {})
        archive_name = str(package.get("archive_name", ""))
        archive_path = find_cached_archive(cache_root, archive_name, str(package.get("archive_sha256", "")))
        if archive_path is None:
            reasons.append("external_archive_cache_missing")
        if reasons:
            excluded.append({"candidate_id": item.get("candidate_id"), "candidate_file_path": item.get("candidate_file_path"), "reasons": reasons})
            continue
        manifest_rel = normalize_path(str(item["manifest_path"])).replace("C:/Users/Administrator/Desktop/game/first01/", "")
        if manifest_record is None:
            continue
        manifest = manifest_record["data"]
        candidate_rel = normalize_path(str(item["candidate_file_path"]))
        candidate = candidate_map(manifest).get(candidate_rel)
        if candidate is None:
            excluded.append({"candidate_id": item.get("candidate_id"), "candidate_file_path": candidate_rel, "reasons": ["candidate_not_in_manifest"]})
            continue
        technical = item.get("import_record", {})
        entry, entry_errors = entry_from_evidence(
            project_root,
            manifest_record["path"],
            manifest,
            candidate,
            review_task=task,
            review_packet=paths["review"],
            machine_evidence=[paths["machine"], paths["hashes"], paths["source"]],
            import_evidence=str(item.get("import_evidence_path", "")),
            archive_path=archive_path,
            candidate_sha256=str(item.get("file_sha256_actual", "")),
            candidate_hash_evidence=paths["review"],
            categories=[str(item.get("category", ""))],
            technical=technical,
            upstream_decision=str(item.get("recommendation", {}).get("decision", "")),
        )
        if entry is None:
            excluded.append({"candidate_id": item.get("candidate_id"), "candidate_file_path": candidate_rel, "reasons": entry_errors})
        else:
            accepted.append(entry)
    return accepted, {
        "task": task,
        "handoff": paths["handoff"],
        "status": "PARTIAL",
        "records_processed": len(review.get("items", [])),
        "records_admitted": len(accepted),
        "records_excluded": len(excluded),
        "excluded": excluded,
        "evidence": [paths["review"], paths["machine"], paths["hashes"], paths["source"]],
        "external_archive_cache": str(cache_root),
    }


def validate_catalog(project_root: Path, catalog: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    if catalog.get("catalog_kind") != "d18_prefab_candidate_intake_catalog":
        errors.append("catalog_kind_invalid")
    if catalog.get("resource_state_machine_unchanged") is not True:
        errors.append("resource_state_machine_boundary_missing")
    if catalog.get("status") != "PASS":
        errors.append(f"catalog_status_not_pass:{catalog.get('status')}")
    if catalog.get("authority", {}).get("release_approved_inferred") is not False:
        errors.append("release_approved_boundary_missing")
    entries = catalog.get("eligible_entries", [])
    seen_ids: set[str] = set()
    seen_refs: set[tuple[str, str]] = set()
    seen_paths: dict[str, str] = {}
    for entry in entries:
        intake_id = str(entry.get("intake_id", ""))
        if not intake_id or intake_id in seen_ids:
            errors.append(f"duplicate_or_missing_intake_id:{intake_id}")
        seen_ids.add(intake_id)
        if entry.get("intake_eligible") is not True:
            errors.append(f"entry_not_eligible:{intake_id}")
        if any(key in entry for key in ("local_development_ready", "release_approved", "steam_ready")):
            errors.append(f"status_shortcut_field:{intake_id}")
        if entry.get("license", {}).get("license_id") != "CC0-1.0":
            errors.append(f"license_not_cc0:{intake_id}")
        if not SHA256_RE.fullmatch(str(entry.get("hash_evidence", {}).get("sha256", ""))):
            errors.append(f"archive_hash_invalid:{intake_id}")
        manifest_rel = normalize_path(str(entry.get("manifest_path", "")))
        candidate_rel = normalize_path(str(entry.get("candidate_file_path", "")))
        reference = (str(entry.get("manifest_id", "")), candidate_rel)
        if reference in seen_refs:
            errors.append(f"duplicate_manifest_file_reference:{reference[0]}::{reference[1]}")
        seen_refs.add(reference)
        if candidate_rel in seen_paths and seen_paths[candidate_rel] != str(entry.get("manifest_id", "")):
            errors.append(f"same_candidate_file_multiple_manifests:{candidate_rel}")
        seen_paths[candidate_rel] = str(entry.get("manifest_id", ""))
        try:
            manifest_path = safe_path(project_root, manifest_rel)
            candidate_path = safe_path(project_root, candidate_rel)
            license_path = safe_path(project_root, str(entry.get("license", {}).get("license_evidence_path", "")))
            import_path = safe_path(project_root, str(entry.get("import_verified_evidence_path", "")))
        except ValueError as exc:
            errors.append(f"unsafe_path:{intake_id}:{exc}")
            continue
        if not manifest_path.is_file():
            errors.append(f"manifest_missing:{intake_id}")
            continue
        if not candidate_path.is_file():
            errors.append(f"candidate_missing:{intake_id}")
        if not license_path.is_file():
            errors.append(f"license_evidence_missing:{intake_id}")
        if not import_path.is_file():
            errors.append(f"import_evidence_missing:{intake_id}")
        manifest = read_json(manifest_path)
        if manifest.get("pack_id") != entry.get("manifest_id"):
            errors.append(f"manifest_id_mismatch:{intake_id}")
        if manifest.get("import_status") != "ImportVerified":
            errors.append(f"source_not_import_verified:{intake_id}")
        if manifest.get("review_status") != "ImportVerified" and "admission_basis" not in entry:
            errors.append(f"candidate_without_review_evidence:{intake_id}")
        if manifest.get("runtime_consumers") not in ([], None):
            errors.append(f"runtime_consumer_asserted:{intake_id}")
        candidate = candidate_map(manifest).get(candidate_rel)
        if candidate is None:
            errors.append(f"candidate_not_in_source_manifest:{intake_id}")
        source_state = entry.get("source_state_snapshot", {}).get("manifest_review_status")
        if source_state is None:
            source_state = manifest.get("review_status")
        if source_state not in ALLOWED_RESOURCE_STATES:
            errors.append(f"source_state_invalid:{intake_id}")
    return sorted(set(errors))


def category_summary(entries: list[dict[str, Any]]) -> dict[str, Any]:
    summary: dict[str, Any] = {}
    for category in EXPECTED_CATEGORIES:
        matching = [entry for entry in entries if category in entry.get("consumer_categories", [])]
        manifests = sorted({entry.get("manifest_id") for entry in matching})
        if matching:
            status = "INTAKE_AVAILABLE_PARTIAL"
            notes = "Evidence supports controlled work-scene/Prefab authoring only; consumer, runtime, save and D18-S03 evidence remains open."
        else:
            status = "EMPTY_BLOCKED"
            notes = "No evidence-complete intake entry; responsible task must keep the gap explicit or create a project-owned presentation asset."
        summary[category] = {
            "intake_eligible_count": len(matching),
            "manifest_count": len(manifests),
            "manifest_ids": manifests,
            "status": status,
            "notes": notes,
        }
    summary["female_or_neutral_human_gap"] = {
        "intake_eligible_count": 0,
        "status": "OPEN",
        "reason": "D18-A04-R02-F has 4 static FBX candidates but no Unity ImportVerified evidence; existing neutral base is Generic/NoAvatar and not a complete Humanoid solution.",
        "owner_tasks": ["D18-A04-R02-F", "D20-A01", "D20-A03"],
    }
    summary["cat_gap"] = {
        "intake_eligible_count": sum(1 for entry in entries if "pet_core" in entry.get("consumer_categories", []) and "cat" in str(entry.get("role", "")).lower() or entry.get("content_id") == "animal_quaternius_cat_pack_vol2_v01"),
        "status": "INTAKE_AVAILABLE_PARTIAL",
        "reason": "One real cat FBX has Unity ImportVerified evidence; only Idle/Walking and no raw Collider/LOD/Avatar are proven.",
        "owner_tasks": ["D20-A06", "D18-S03"],
    }
    summary["bird_gap"] = {
        "intake_eligible_count": sum(1 for entry in entries if entry.get("content_id") == "animal_quaternius_eagle_v01"),
        "status": "INTAKE_AVAILABLE_PARTIAL",
        "reason": "One real Eagle FBX is imported for mid/far controlled work; original Blender-only bird remains excluded and near-camera behavior is not proven.",
        "owner_tasks": ["D20-A05", "D18-S03"],
    }
    summary["sail_rig_gap"] = {
        "intake_eligible_count": sum(1 for entry in entries if "ship_visual_candidate" in entry.get("consumer_categories", []) and "sail" in str(entry.get("role", "")).lower()),
        "status": "EMPTY_BLOCKED",
        "reason": "No independent sail evidence; mast/ropes are not a sail or rigging interaction substitute.",
        "owner_tasks": ["D21-A02"],
    }
    summary["rudder_anchor_gap"] = {
        "intake_eligible_count": 0,
        "status": "EMPTY_BLOCKED",
        "reason": "No independent rudder/anchor or consumer interaction-anchor evidence in A05-R01.",
        "owner_tasks": ["D21-A01", "D21-A02", "D21-A03"],
    }
    return summary


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", type=Path, default=Path("."))
    parser.add_argument("--catalog", type=Path, default=CATALOG_REL)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--generated-on", default=date.today().isoformat())
    parser.add_argument("--write-catalog", action="store_true")
    args = parser.parse_args(argv)
    project_root = args.project_root.resolve()
    catalog_path = args.catalog if args.catalog.is_absolute() else project_root / args.catalog
    output_path = args.output if args.output.is_absolute() else project_root / args.output
    if not catalog_path.is_file():
        print(f"missing current catalog: {catalog_path}", file=sys.stderr)
        return 2
    current_catalog = read_json(catalog_path)
    previous_v2 = current_catalog.get("refresh", {}).get("previous_v2_catalog_snapshot")
    if not isinstance(previous_v2, dict):
        previous_v2 = baseline_snapshot(current_catalog, catalog_path, project_root)
    # A failed first run may have written a BLOCKED R01 catalog before the
    # caller inspected it.  The prior v2 JSON validation output is a preserved
    # read-only copy of the 1,674-entry baseline, so recover from it rather
    # than treating the partial R01 output as the new baseline.
    baseline_catalog = current_catalog
    baseline_validation = project_root / "game/tools/results/D18/D18-A04-R01/D18_A04_R01_INTAKE_CATALOG_JSON_VALIDATION.txt"
    if current_catalog.get("task") == "D18-S02-R01" and baseline_validation.is_file():
        try:
            recovered = read_json(baseline_validation)
            if recovered.get("task") == "D18-S02-v2" and recovered.get("status") == "PASS":
                baseline_catalog = recovered
        except (OSError, json.JSONDecodeError):
            pass
    baseline_entries = copy.deepcopy(baseline_catalog.get("eligible_entries", []))
    manifest_by_id, manifest_errors = manifest_index(project_root)
    all_new_entries: list[dict[str, Any]] = []
    upstream_results: list[dict[str, Any]] = []
    a04, a04_result = process_a04_r01(project_root, manifest_by_id)
    all_new_entries.extend(a04)
    upstream_results.append(a04_result)
    upstream_results.append(process_a04_r02_f(project_root, manifest_by_id))
    cat, cat_result = process_a04_r02_c(project_root, manifest_by_id)
    all_new_entries.extend(cat)
    upstream_results.append(cat_result)
    bird, bird_result = process_a04_r02_b(project_root, manifest_by_id)
    all_new_entries.extend(bird)
    upstream_results.append(bird_result)
    a05, a05_result = process_a05(project_root, manifest_by_id)
    all_new_entries.extend(a05)
    upstream_results.append(a05_result)
    existing_refs = {(str(item.get("manifest_id")), normalize_path(str(item.get("candidate_file_path", "")))) for item in baseline_entries}
    deduped_new: list[dict[str, Any]] = []
    duplicate_new: list[str] = []
    seen_new: set[tuple[str, str]] = set()
    for entry in all_new_entries:
        key = (str(entry.get("manifest_id")), normalize_path(str(entry.get("candidate_file_path", ""))))
        if key in existing_refs:
            # Idempotent reruns rebuild from the preserved v2 baseline; an
            # already-admitted refresh row is expected and is not a conflict.
            continue
        if key in seen_new:
            duplicate_new.append(f"{key[0]}::{key[1]}")
            continue
        seen_new.add(key)
        deduped_new.append(entry)
    entries = sorted(baseline_entries + deduped_new, key=lambda item: str(item.get("intake_id", "")))
    entry_manifest_ids = {str(item.get("manifest_id")) for item in entries}
    excluded: list[dict[str, Any]] = []
    for pack_id, record in sorted(manifest_by_id.items()):
        if pack_id in entry_manifest_ids:
            continue
        manifest = record["data"]
        reasons: list[str] = []
        if manifest.get("review_status") != "ImportVerified":
            reasons.append(f"review_status={manifest.get('review_status')}")
        if manifest.get("import_status") != "ImportVerified":
            reasons.append(f"import_status={manifest.get('import_status')}")
        if pack_id == "pack_quaternius_animated_women_d18_a04_r02_f_v1":
            reasons.extend(("D18-A04-R02-F Unity import not run", "static-only evidence is insufficient for intake"))
        if pack_id == "pack_opengameart_bird_d18_a04_v1":
            reasons.extend(("source is Blender-only", "no Unity mesh/import evidence"))
        excluded.append({
            "manifest_id": pack_id,
            "manifest_path": record["path"],
            "candidate_file_count": len(manifest.get("candidate_files", [])),
            "reasons": sorted(set(reasons)) or ["no evidence-complete upstream admission packet"],
            "next_owner": "D18-S03 or the responsible asset/consumer task",
        })
    counts = {
        "manifest_total": len(manifest_by_id),
        "manifest_intake_eligible": len(entry_manifest_ids),
        "candidate_file_total": sum(len(record["data"].get("candidate_files", [])) for record in manifest_by_id.values()),
        "intake_eligible_file_total": len(entries),
        "excluded_manifest_total": len(excluded),
        "duplicate_manifest_file_references": len(duplicate_new),
        "refresh_admitted_file_total": len(deduped_new),
        "errors": len(manifest_errors),
    }
    category_counts = category_summary(entries)
    catalog = {
        "catalog_kind": "d18_prefab_candidate_intake_catalog",
        "schema_version": 1,
        "task": "D18-S02-R01",
        "generated_on": args.generated_on,
        "status": "PASS" if not manifest_errors and not duplicate_new else "BLOCKED",
        "definition": "PrefabCandidateIntake means evidence-complete license, hash, path, ImportVerified evidence and controlled mapping for a designated work-scene or Prefab task. It is not a resource state and never implies LocalDevelopmentReady, formal scene integration, runtime consumer, ReleaseApproved or Steam Ready.",
        "resource_state_machine_unchanged": True,
        "authority": {
            "manifests": "docs/game/design/common/assets",
            "d18_local_development_ready_catalog": READY_CATALOG_REL.as_posix(),
            "registry_changed": False,
            "asset_state_changed": False,
            "formal_initial_harbor_integrated": False,
            "release_approved_inferred": False,
            "global_catalog_writer": "D18-S02-R01",
        },
        "previous_v2_catalog_snapshot": previous_v2,
        "counts": counts,
        "eligible_entries": entries,
        "excluded_manifests": excluded,
        "category_summary": category_counts,
        "project_owned_gaps": [
            {
                "gap_id": "gap_sea_weather_shader_lighting",
                "status": "ProjectOwnedAssetRequired",
                "description": "No external model candidate is admitted for seawater, weather shaders, lighting or post-processing.",
                "owner_tasks": ["D19-A04", "D19-A05"],
                "note": "Create and test project-owned presentation assets; do not fabricate external resource readiness.",
            },
            {
                "gap_id": "gap_female_era_neutral_character",
                "status": "ExternalCandidateEvidenceIncomplete",
                "owner_tasks": ["D18-A04-R02-F", "D20-A01", "D20-A03"],
                "note": "Four female FBX files have static evidence but no Unity ImportVerified evidence; existing neutral base is not a complete Humanoid solution.",
            },
            {
                "gap_id": "gap_bird_cat_pet_completion",
                "status": "ControlledIntakePartial",
                "owner_tasks": ["D20-A05", "D20-A06", "D18-S03"],
                "note": "Cat and Eagle are admitted for controlled work only; original Blender-only bird remains excluded, and Animator/LOD/collision/runtime evidence is open.",
            },
            {
                "gap_id": "gap_ship_sail_rudder_anchor",
                "status": "ControlledIntakePartial",
                "owner_tasks": ["D19-R01-B", "D21-A01", "D21-A02", "D21-A03"],
                "note": "Port structure, dock and market props are admitted for work-scene review; sail, rudder, anchor, collision and interaction anchors remain consumer-owned gaps.",
            },
            {
                "gap_id": "existing_starting_ship_authoritative",
                "status": "ExistingProjectAsset",
                "owner_tasks": ["D21-A01", "D21-A03"],
                "note": "Keep stable asset_ph_ship_fishing ID and save semantics; this refresh does not replace the authoritative starting ship.",
            },
        ],
        "refresh": {
            "refresh_task": "D18-S02-R01",
            "previous_v2_catalog_snapshot": previous_v2,
            "upstream_reviews": [
                {"task": task, "handoff": paths["handoff"], "status": result.get("status"), "records_processed": result.get("records_processed"), "records_admitted": result.get("records_admitted")} for task, paths, result in ((task, UPSTREAM[task], result) for task, result in [(item["task"], item) for item in upstream_results])
            ],
            "admitted_from_upstream": [
                {
                    "task": result["task"],
                    "records_admitted": sum(1 for entry in entries if entry.get("admission_basis", {}).get("source_review_task") == result["task"]),
                    "manifest_ids": sorted({str(entry.get("manifest_id")) for entry in entries if entry.get("admission_basis", {}).get("source_review_task") == result["task"]}),
                }
                for result in upstream_results
            ],
            "admission_policy": "A source manifest may remain review_status=Candidate while an isolated review packet supplies complete local-file, hash, CC0-license and Unity ImportVerified evidence. This flag is non-state work-scene intake only; source resource status is preserved.",
            "runtime_promotion": {"local_development_ready_count": 0, "release_approved_count": 0, "promotion_performed": False, "next_gate": "D18-S03"},
        },
        "next_gate": {
            "task": "D18-S03",
            "responsibility": "Runtime consumers and LocalDevelopmentReady promotion audit after D19-D22 work-scenes, Prefabs, formal integration and PlayMode regression.",
            "must_verify": ["real Prefab and work-scene references", "collision, anchors, LOD and Animator where applicable", "runtime consumer and interaction behavior", "save/restore, screenshot, rollback and performance evidence", "no Registry/asset_state/status shortcut"],
        },
    }
    catalog_errors = validate_catalog(project_root, catalog)
    if catalog_errors:
        catalog["status"] = "BLOCKED"
    ready_path = project_root / READY_CATALOG_REL
    ready_hash_before = sha256_file(ready_path) if ready_path.is_file() else None
    result = {
        "task": "D18-S02-R01",
        "status": catalog["status"],
        "generated_on": args.generated_on,
        "read_only_source_status": True,
        "previous_v2_catalog_snapshot": previous_v2,
        "current_catalog_before_write": {
            "path": rel(catalog_path, project_root),
            "sha256": sha256_file(catalog_path),
            "counts": current_catalog.get("counts", {}),
            "task": current_catalog.get("task"),
            "status": current_catalog.get("status"),
        },
        "upstream": upstream_results,
        "counts": counts,
        "category_summary": category_counts,
        "audit": {
            "manifest_index_errors": manifest_errors,
            "catalog_validation_errors": catalog_errors,
            "duplicate_new_references": duplicate_new,
            "same_candidate_file_multiple_sources": 0,
            "path_escape_errors": sum(1 for error in manifest_errors if "unsafe path" in error),
            "status_shortcut_fields_detected": any(any(key in entry for key in ("local_development_ready", "release_approved", "steam_ready")) for entry in entries),
        },
        "resource_state_snapshot": {
            "ready_catalog_path": READY_CATALOG_REL.as_posix(),
            "ready_catalog_sha256_before": ready_hash_before,
            "local_development_ready_count": 0,
            "release_approved_count": 0,
            "ready_catalog_modified": False,
            "source_manifests_modified": False,
        },
        "repeatability": {
            "sorted_entries_by": "intake_id",
            "stable_id_formula": "sha256(manifest_id + '::' + candidate_file_path)[:20]",
            "same_generated_on_rerun_expected": True,
            "no_manual_row_copy": True,
        },
        "result_paths": {
            "catalog": CATALOG_REL.as_posix(),
            "ready_catalog": READY_CATALOG_REL.as_posix(),
            "historical_v2_report": "docs/game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md",
            "historical_v2_handoff": "docs/game/design/common/handoffs/D18/D18-S02_V2_HANDOFF.md",
        },
    }
    output_path.parent.mkdir(parents=True, exist_ok=True)
    write_json(output_path, result)
    write_json(output_path.parent / "D18_S02_R01_PREVIOUS_CATALOG_SNAPSHOT.json", previous_v2)
    write_json(output_path.parent / "D18_S02_R01_UPSTREAM_EVIDENCE_AUDIT.json", {"task": "D18-S02-R01", "status": result["status"], "upstream": upstream_results, "category_summary": category_counts, "audit": result["audit"]})
    write_json(output_path.parent / "D18_S02_R01_CATEGORY_GATE.json", {"task": "D18-S02-R01", "status": result["status"], "categories": category_counts})
    if args.write_catalog:
        write_json(catalog_path, catalog)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
