"""D18-A07 deterministic placeholder replacement map and read-only audit.

This tool treats the Content Registry as the authority for the input set and
the D18 manifests as a candidate catalogue.  It never changes Registry,
asset_state, prefabs, scenes, or candidate files.  A mapping is a plan only:
``planned_candidate`` is not an integration or release approval.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Iterable


TOOL_VERSION = "d18-a07-map-v1"
ALLOWED_ASSET_STATES = {"Candidate", "LicenseEvidenceComplete", "ImportVerified", "LocalDevelopmentReady", "ReleaseApproved", "Rejected", "NotImported"}
ALLOWED_MAPPING_STATUSES = {"planned_candidate", "unmapped", "blocked_candidate_evidence"}
REQUIRED_RECORD_FIELDS = {
    "content_id", "placeholder_asset", "domain", "gameplay_priority", "visibility_frequency",
    "target_candidates", "status", "license_evidence", "prefab_scene_consumers",
    "collision_requirements", "anchor_requirements", "lod_requirements",
    "animation_requirements", "responsible_phase", "blockers",
}


def canonical(value: Any) -> str:
    return json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":"))


def sha256_text(value: str) -> str:
    return hashlib.sha256(value.encode("utf-8")).hexdigest()


def relpath(root: Path, path: Path) -> str:
    try:
        return path.resolve().relative_to(root.resolve()).as_posix()
    except ValueError:
        return path.as_posix()


def resolve_project_path(root: Path, raw: Any) -> Path | None:
    """Resolve repository-relative, Unity-relative, or docs-relative paths."""
    if not isinstance(raw, str) or not raw.strip():
        return None
    value = raw.split("#", 1)[0].strip().replace("\\", "/")
    if not value:
        return None
    path = Path(value)
    if path.is_absolute():
        return path
    candidates = []
    if value.startswith("game/"):
        candidates.append(root / value)
    elif value.startswith("Assets/"):
        candidates.extend((root / "game" / value, root / value))
    else:
        candidates.extend((root / value, root / "game" / value))
    return next((candidate for candidate in candidates if candidate.exists()), candidates[0])


def registry_dir(root: Path) -> Path:
    return root / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"


def load_registry_entries(root: Path) -> list[dict[str, Any]]:
    entries: list[dict[str, Any]] = []
    for path in sorted(registry_dir(root).rglob("*.json")):
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if isinstance(value, dict) and "asset_state" in value:
            item = dict(value)
            item["_registry_path"] = relpath(root, path)
            entries.append(item)
    return entries


def registry_digest(root: Path) -> str:
    digest = hashlib.sha256()
    directory = registry_dir(root)
    for path in sorted(directory.rglob("*.json")):
        digest.update(relpath(root, path).encode("utf-8"))
        digest.update(b"\0")
        digest.update(path.read_bytes())
        digest.update(b"\0")
    return digest.hexdigest()


def load_manifests(root: Path) -> list[dict[str, Any]]:
    result = []
    base = root / "docs" / "game" / "design" / "common" / "assets"
    for path in sorted(base.rglob("*.manifest.json")):
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError):
            continue
        if not isinstance(value, dict) or value.get("manifest_kind") != "d18_asset_pack_manifest":
            continue
        item = dict(value)
        item["_manifest_path"] = relpath(root, path)
        result.append(item)
    return result


def candidate_kind(manifest: dict[str, Any], candidate: dict[str, Any]) -> str:
    text = " ".join(str(value) for value in (
        manifest.get("title", ""), candidate.get("role", ""),
        candidate.get("path", ""), candidate.get("target_id", ""),
    )).lower()
    tokens = set(re.split(r"[^a-z0-9]+", text))
    identity = " ".join(str(value) for value in (candidate.get("target_id", ""), candidate.get("role", ""), candidate.get("path", ""))).lower()
    identity_tokens = set(re.split(r"[^a-z0-9]+", identity))
    if any(token in identity_tokens for token in ("prop", "props", "barrel", "bottle", "chest", "crate", "cannon")):
        return "prop"
    if any(token in identity_tokens for token in ("building", "structure", "dock", "pier")):
        return "building"
    if any(token in identity_tokens for token in ("ship", "boat")):
        return "ship"
    if "island" in identity_tokens:
        return "environment"
    if any(token in tokens for token in ("audio", "sound", "ogg")):
        return "audio"
    if any(token in tokens for token in ("fish", "dolphin", "whale", "manta", "shark")):
        return "fish"
    if any(token in tokens for token in ("character", "characters", "crew", "man", "male")):
        return "character"
    if any(token in tokens for token in ("animal", "animals", "horse", "dog", "fox", "deer", "wolf", "bull", "cow", "donkey", "alpaca", "stag")):
        return "animal"
    if "ship" in tokens or "boat" in tokens:
        return "ship"
    if any(token in tokens for token in ("building", "buildings", "structure", "dock", "pier", "pirate")):
        return "building"
    # Check UI after building: the substring ``ui`` is present in
    # ``building`` and must not classify docks or structures as interface art.
    if re.search(r"(?:^|[^a-z])ui(?:[^a-z]|$)", text) or any(token in text for token in ("prompt", "interface")):
        return "ui"
    if any(token in tokens for token in ("material", "materials", "texture", "textures", "sand", "coral", "hdri")):
        return "material"
    if any(token in tokens for token in ("prop", "props", "barrel", "bottle", "chest", "crate", "cannon")):
        return "prop"
    return "environment"


def candidate_catalog(root: Path) -> list[dict[str, Any]]:
    """Flatten manifests into immutable, auditable candidate-file records."""
    result = []
    for manifest in load_manifests(root):
        evidence = resolve_project_path(root, manifest.get("license_evidence_path"))
        evidence_exists = bool(evidence and evidence.is_file())
        for candidate in manifest.get("candidate_files", []) or []:
            if not isinstance(candidate, dict) or not candidate.get("path"):
                continue
            file_path = resolve_project_path(root, candidate.get("path"))
            record = {
                "candidate_id": str(candidate.get("target_id") or Path(str(candidate["path"])).stem),
                "target_id": candidate.get("target_id"),
                "path": str(candidate["path"]).replace("\\", "/"),
                "resolved_file": relpath(root, file_path) if file_path else None,
                "file_exists": bool(file_path and file_path.is_file()),
                "role": candidate.get("role", ""),
                "kind": candidate_kind(manifest, candidate),
                "source_id": manifest.get("source_id"),
                "pack_id": manifest.get("pack_id"),
                "title": manifest.get("title"),
                "official_page": manifest.get("official_page"),
                "license_id": manifest.get("license_id"),
                "license_url": manifest.get("license_url"),
                "license_evidence_path": manifest.get("license_evidence_path"),
                "license_evidence_exists": evidence_exists,
                "review_status": manifest.get("review_status"),
                "import_status": manifest.get("import_status"),
                "manifest_path": manifest.get("_manifest_path"),
                "runtime_consumers": list(manifest.get("runtime_consumers", []) or []),
            }
            result.append(record)
    return sorted(result, key=lambda item: (str(item.get("candidate_id")), str(item.get("path"))))


def text_for_entry(entry: dict[str, Any]) -> str:
    values = []
    for key in ("id", "name", "display_name_key", "description", "gameplay_role", "source_note", "tags", "valid_habitats", "valid_regions"):
        value = entry.get(key)
        if isinstance(value, list):
            values.extend(str(item) for item in value)
        elif value is not None:
            values.append(str(value))
    return " ".join(values).lower().replace("-", "_")


def priority_for(entry: dict[str, Any]) -> str:
    content_id = str(entry.get("id", ""))
    category = str(entry.get("category", ""))
    if content_id in {
        "asset_ph_crew_male", "crew_d15_a07_preset_001", "building_saint_marys_dock",
        "building_small_boat_pier", "island_saint_marys",
    }:
        return "P0"
    if category in {"building", "crew", "animal", "fish", "island", "pet", "asset"}:
        return "P1"
    if category in {"item", "outfit"}:
        return "P2"
    return "P3"


def visibility_for(priority: str) -> str:
    return {"P0": "critical", "P1": "frequent", "P2": "contextual", "P3": "long_tail"}[priority]


def phase_for(entry: dict[str, Any]) -> str:
    category = str(entry.get("category", ""))
    if category in {"island", "building", "plant"}:
        return "D19"
    if category in {"crew", "animal", "pet"}:
        return "D20"
    if category in {"fish", "ship"}:
        return "D21"
    if category in {"asset"}:
        text = text_for_entry(entry)
        return "D22" if any(token in text for token in ("ui", "audio", "music", "catalog", "weather", "sea")) else "D24"
    if category in {"item", "outfit"}:
        return "D23"
    return "D24"


def requirement_for(entry: dict[str, Any], field: str) -> dict[str, Any]:
    category = str(entry.get("category", ""))
    text = text_for_entry(entry)
    world = category in {"island", "building", "plant", "animal", "fish", "pet", "crew", "item", "outfit"}
    animated = category in {"crew", "animal", "fish", "pet"} or any(token in text for token in ("animated", "animation", "dolphin", "whale"))
    if field == "collision":
        required = world and category not in {"outfit"}
        return {"required": required, "source_registry_ref": entry.get("collision_ref"), "acceptance": "collider or explicit non-colliding decoration proof" if required else "not_applicable"}
    if field == "anchor":
        required = category in {"island", "building", "ship", "asset"} or any(token in text for token in ("dock", "pier", "boat", "ship"))
        return {"required": required, "source_registry_ref": entry.get("anchor_refs", entry.get("references")), "acceptance": "stable gameplay anchor IDs preserved" if required else "not_applicable"}
    if field == "lod":
        required = world and category not in {"outfit"}
        return {"required": required, "source_registry_ref": entry.get("lod_profile"), "acceptance": "D18 distance-tier LOD audit" if required else "not_applicable"}
    return {"required": animated, "source_registry_ref": entry.get("animation_profile"), "acceptance": "rig/clip binding and fallback pose audit" if animated else "not_applicable"}


def scan_consumers(root: Path, content_id: str) -> list[str]:
    """Read-only search of current Unity prefab/scene text consumers."""
    consumers = []
    for base in (root / "game" / "Assets",):
        if not base.exists():
            continue
        for path in sorted(list(base.rglob("*.prefab")) + list(base.rglob("*.unity"))):
            try:
                if content_id in path.read_text(encoding="utf-8", errors="ignore"):
                    consumers.append(relpath(root, path))
            except OSError:
                continue
    return consumers


def scan_consumer_index(root: Path, content_ids: Iterable[str]) -> dict[str, list[str]]:
    """Scan Unity text assets once instead of once per Registry record."""
    wanted = {str(content_id) for content_id in content_ids}
    result: dict[str, list[str]] = defaultdict(list)
    base = root / "game" / "Assets"
    if not base.exists() or not wanted:
        return result
    for path in sorted(list(base.rglob("*.prefab")) + list(base.rglob("*.unity"))):
        try:
            text = path.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        for content_id in wanted:
            if content_id in text:
                result[content_id].append(relpath(root, path))
    return result


def candidate_tokens(candidate: dict[str, Any]) -> set[str]:
    raw = " ".join(str(candidate.get(key, "")) for key in ("candidate_id", "target_id", "role", "path", "title")).lower()
    return {token for token in re.split(r"[^a-z0-9]+", raw) if len(token) > 2}


def _compatible(entry: dict[str, Any], candidate: dict[str, Any]) -> int:
    category = str(entry.get("category", ""))
    kind = str(candidate.get("kind", ""))
    text = text_for_entry(entry)
    tokens = candidate_tokens(candidate)
    score = 0
    if category == "plant" and kind == "environment":
        score += 8
    elif category == "island" and kind in {"environment", "building", "material"}:
        score += 7
    elif category == "building" and kind == "building":
        score += 8
    elif category == "crew" and kind == "character":
        score += 8
    elif category == "animal" and kind == "animal":
        score += 8
    elif category == "fish" and kind == "fish":
        score += 8
    elif category == "pet" and kind == "animal":
        score += 8
    elif category == "item" and kind == "prop":
        score += 5
    elif category == "asset":
        if any(token in text for token in ("ui", "catalog", "prompt")) and kind == "ui":
            score += 10
        elif any(token in text for token in ("audio", "music", "sound")) and kind == "audio":
            score += 10
        elif any(token in text for token in ("material", "texture", "terrain", "sea", "weather")) and kind == "material":
            score += 7
        elif kind in {"environment", "building"} and "ship" in text and "boat" in tokens:
            score += 5
    else:
        return -1
    if score <= 0:
        return -1
    # Prefer exact semantic terms, but do not match arbitrary generated IDs.
    semantic = {token for token in re.split(r"[^a-z0-9]+", text) if len(token) > 3}
    score += min(5, len(semantic & tokens))
    if category in {"item", "plant"}:
        # These domains contain many data-only or historical variants. A
        # generic prop/bush must not be presented as a semantic replacement
        # unless the stable content ID shares a meaningful object token.
        identity = " ".join(str(entry.get(key, "")) for key in ("id", "name", "display_name_key")).lower()
        identity_tokens = {token for token in re.split(r"[^a-z0-9]+", identity) if len(token) > 2}
        ignored = {"item", "plant", "asset", "profile", "vegetation", "d15", "d18", "d16", "content", "name", "v01", "v02", "v03"}
        meaningful_overlap = (identity_tokens - ignored) & (tokens - ignored)
        if not meaningful_overlap:
            return -1
    return score


def select_candidates(entry: dict[str, Any], candidates: list[dict[str, Any]]) -> list[dict[str, Any]]:
    content_id = str(entry.get("id", ""))
    if content_id == "asset_d15_a08_catalog":
        # This Registry item is a data/catalog root, not a visual/audio asset.
        return []
    exact = [item for item in candidates if item.get("target_id") == content_id and item.get("file_exists")]
    if exact:
        return exact[:3]
    if content_id == "crew_d15_a07_preset_001":
        preferred = [item for item in candidates if item.get("candidate_id") == "crew_quaternius_male_shirt_v01" and item.get("file_exists")]
        if preferred:
            return preferred[:1]
    text = text_for_entry(entry)
    category = str(entry.get("category", ""))
    scored = [(item, _compatible(entry, item)) for item in candidates]
    scored = [(item, score) for item, score in scored if score >= 6 and item.get("file_exists")]
    if category == "island" and content_id == "island_saint_marys":
        return [item for item in exact if item.get("file_exists")]
    if not scored:
        return []
    scored.sort(key=lambda pair: (-pair[1], str(pair[0].get("candidate_id")), str(pair[0].get("path"))))
    selected = [scored[0][0]]
    # Only add composition pieces when the entry names multiple explicit landform parts.
    if category == "island" and any(token in text for token in ("saint", "mary", "harbor", "coast")):
        for item, score in scored[1:]:
            if score >= scored[0][1] - 2 and item.get("candidate_id") != selected[0].get("candidate_id"):
                selected.append(item)
            if len(selected) == 3:
                break
    return selected


def evidence_for(selected: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "complete_for_candidate": bool(selected) and all(
            item.get("license_evidence_exists") and item.get("file_exists") and item.get("license_id") for item in selected
        ),
        "items": [
            {
                "pack_id": item.get("pack_id"),
                "manifest_path": item.get("manifest_path"),
                "license_id": item.get("license_id"),
                "license_url": item.get("license_url"),
                "license_evidence_path": item.get("license_evidence_path"),
                "license_evidence_exists": item.get("license_evidence_exists"),
            }
            for item in selected
        ],
    }


def mapping_status(selected: list[dict[str, Any]]) -> str:
    if not selected:
        return "unmapped"
    if not all(item.get("file_exists") and item.get("license_evidence_exists") for item in selected):
        return "blocked_candidate_evidence"
    return "planned_candidate"


def build_mapping(root: Path) -> dict[str, Any]:
    root = Path(root).resolve()
    entries = [entry for entry in load_registry_entries(root) if entry.get("asset_state") == "placeholder"]
    candidates = candidate_catalog(root)
    by_id = {str(item.get("candidate_id")): item for item in candidates}
    consumer_index = scan_consumer_index(root, (entry.get("id") for entry in entries))
    records = []
    reuse_groups: defaultdict[str, list[str]] = defaultdict(list)
    for entry in sorted(entries, key=lambda item: str(item.get("id", ""))):
        content_id = str(entry.get("id", ""))
        priority = priority_for(entry)
        selected = select_candidates(entry, candidates)
        # Explicitly keep outfit visuals unmapped: a character base is not an outfit.
        if str(entry.get("category")) == "outfit" and content_id != "asset_d15_a08_catalog":
            selected = []
        target_candidates = []
        for item in selected:
            target = dict(item)
            target["candidate_stage"] = "ImportVerified" if item.get("import_status") == "ImportVerified" else "NotImported"
            target["candidate_review_status"] = item.get("review_status")
            target["local_development_ready"] = item.get("review_status") == "LocalDevelopmentReady"
            target_candidates.append(target)
            reuse_groups[str(item.get("path"))].append(content_id)
        runtime_paths = []
        for field in ("resource_path", "runtime_prefab_path", "model_ref", "material_ref", "audio_file_ref", "source_path"):
            value = entry.get(field)
            if isinstance(value, str) and value.strip():
                runtime_paths.append({"field": field, "path": value, "exists": bool(resolve_project_path(root, value) and resolve_project_path(root, value).exists())})
        registry_consumers = []
        for key in ("references", "asset_refs", "dependencies"):
            value = entry.get(key)
            if isinstance(value, list):
                registry_consumers.extend(str(item) for item in value)
            elif isinstance(value, str) and value:
                registry_consumers.append(value)
        consumers = sorted(set(registry_consumers + consumer_index.get(content_id, [])))
        blockers = []
        status = mapping_status(selected)
        if not selected:
            blockers.append("no_semantic_candidate_in_current_manifests")
        else:
            if not evidence_for(selected).get("complete_for_candidate"):
                blockers.append("license_or_candidate_file_evidence_incomplete")
            if not all(item.get("review_status") == "LocalDevelopmentReady" for item in selected):
                blockers.append("local_development_ready_not_confirmed")
            blockers.append("runtime_consumer_not_registered")
            if priority != "P0" or str(entry.get("category")) not in {"asset", "building", "island"}:
                blockers.append("prefab_collision_anchor_lod_pending")
        if requirement_for(entry, "collision").get("required") and selected:
            blockers.append("collision_verification_pending")
        if requirement_for(entry, "anchor").get("required") and selected:
            blockers.append("anchor_verification_pending")
        if requirement_for(entry, "lod").get("required") and selected:
            blockers.append("lod_verification_pending")
        if requirement_for(entry, "animation").get("required") and selected:
            blockers.append("animation_binding_pending")
        mapping_type = "unmapped"
        if len(selected) == 1:
            mapping_type = "many_to_one_reuse"
        elif len(selected) > 1:
            mapping_type = "one_to_many_composition"
        for item in target_candidates:
            reuse_groups[str(item.get("path"))].append(content_id)
        record = {
            "content_id": content_id,
            "placeholder_asset": {
                "asset_state": entry.get("asset_state"),
                "resource_path": entry.get("resource_path"),
                "runtime_prefab_path": entry.get("runtime_prefab_path"),
                "model_ref": entry.get("model_ref"),
                "material_ref": entry.get("material_ref"),
                "audio_file_ref": entry.get("audio_file_ref"),
                "registry_path": entry.get("_registry_path"),
            },
            "domain": entry.get("category"),
            "gameplay_priority": priority,
            "visibility_frequency": visibility_for(priority),
            "target_candidates": target_candidates,
            "status": status,
            "mapping_type": mapping_type,
            "reuse_is_independent_model": False if len(selected) == 1 else None,
            "license_evidence": evidence_for(selected),
            "prefab_scene_consumers": {
                "registry_declared": consumers,
                "current_file_paths": runtime_paths,
                "candidate_runtime_consumers": sorted({consumer for item in selected for consumer in item.get("runtime_consumers", [])}),
            },
            "collision_requirements": requirement_for(entry, "collision"),
            "anchor_requirements": requirement_for(entry, "anchor"),
            "lod_requirements": requirement_for(entry, "lod"),
            "animation_requirements": requirement_for(entry, "animation"),
            "responsible_phase": phase_for(entry),
            "write_owner": entry.get("owner_system"),
            "registry_asset_state": entry.get("asset_state"),
            "blockers": sorted(set(blockers)),
        }
        if len(selected) == 1:
            record["reuse_group_id"] = "reuse_" + str(selected[0].get("candidate_id"))
        records.append(record)

    # The same path may be selected repeatedly. That is intentional reuse,
    # not a duplicated model. Do not count the preliminary append above twice.
    reuse_groups = defaultdict(list)
    for record in records:
        for item in record["target_candidates"]:
            reuse_groups[str(item.get("path"))].append(record["content_id"])
    control_points = [
        {
            "requirement": "player",
            "input_scope": "no player placeholder in the 2,264 records; retain existing player visual contract",
            "status": "candidate_ready_control_point",
            "target_candidates": [by_id["asset_ph_crew_male"]] if "asset_ph_crew_male" in by_id else [],
            "responsible_phase": "D19",
            "blockers": ["player_visual_not_represented_by_current_placeholder_queue", "runtime_consumer_not_registered"],
        },
        {
            "requirement": "key_npc",
            "content_id": "crew_d15_a07_preset_001",
            "status": "covered_by_P0_record",
            "responsible_phase": "D20",
        },
        {
            "requirement": "starting_ship",
            "content_id": "asset_ph_ship_fishing",
            "input_scope": "integrated/outside placeholder input",
            "status": "out_of_queue_control_point",
            "target_candidates": [by_id["candidate_ship_pirate_small"]] if "candidate_ship_pirate_small" in by_id else [],
            "responsible_phase": "D21",
            "blockers": ["no_remap_allowed_for_integrated_asset", "ship_prefab_consumer_and_anchor_review_pending"],
        },
        {
            "requirement": "sea_and_weather",
            "status": "unmapped_control_point",
            "target_candidates": [],
            "responsible_phase": "D21",
            "blockers": ["no_compatible_sea_surface_or_weather_candidate_in_A03_A06_manifests"],
        },
        {
            "requirement": "core_ui",
            "status": "candidate_ready_control_point",
            "target_candidates": [next((item for item in candidates if item.get("kind") == "ui" and item.get("file_exists")), {})],
            "responsible_phase": "D22",
            "blockers": ["no_UI_placeholder_content_id_in_current_2264_input", "runtime_consumer_not_registered"],
        },
    ]
    base = {
        "mapping_kind": "d18_placeholder_replacement_map",
        "schema_version": 1,
        "tool_version": TOOL_VERSION,
        "authority": {
            "registry": "game/Assets/Resources/ContentRegistry/entries",
            "input_rule": "asset_state == placeholder",
            "candidate_catalog": "docs/game/design/common/assets/**/*.manifest.json",
            "registry_snapshot_digest": registry_digest(root),
        },
        "records": records,
        "control_points": control_points,
        "reuse_groups": {path: sorted(ids) for path, ids in sorted(reuse_groups.items()) if len(ids) > 1},
        "read_only_policy": {
            "registry_changed": False,
            "asset_state_changed": False,
            "prefab_or_scene_changed": False,
            "candidate_to_integrated_transition": False,
            "release_approved_created": False,
        },
    }
    base["deterministic_digest"] = sha256_text(canonical({"records": records, "control_points": control_points}))
    return base


def _reason_counts(records: Iterable[dict[str, Any]]) -> dict[str, int]:
    counter = Counter(reason for record in records for reason in record.get("blockers", []))
    return dict(sorted(counter.items()))


def audit_mapping(root: Path, mapping: dict[str, Any]) -> dict[str, Any]:
    records = list(mapping.get("records", []))
    entries = [entry for entry in load_registry_entries(root) if entry.get("asset_state") == "placeholder"]
    authoritative_ids = [str(entry.get("id")) for entry in sorted(entries, key=lambda item: str(item.get("id", "")))]
    output_ids = [str(record.get("content_id")) for record in records]
    duplicate_ids = sorted([item for item, count in Counter(output_ids).items() if count > 1])
    missing_ids = sorted(set(authoritative_ids) - set(output_ids))
    extra_ids = sorted(set(output_ids) - set(authoritative_ids))
    missing_candidates = []
    missing_evidence = []
    missing_files = []
    invalid_status = []
    invalid_candidate_status = []
    invalid_asset_state = []
    required_field_gaps = []
    one_to_many_issues = []
    for record in records:
        content_id = record.get("content_id")
        gaps = sorted(REQUIRED_RECORD_FIELDS - set(record))
        if gaps:
            required_field_gaps.append({"content_id": content_id, "fields": gaps})
        if record.get("status") not in ALLOWED_MAPPING_STATUSES:
            invalid_status.append({"content_id": content_id, "status": record.get("status")})
        if record.get("registry_asset_state") != "placeholder":
            invalid_asset_state.append({"content_id": content_id, "asset_state": record.get("registry_asset_state")})
        targets = record.get("target_candidates", []) or []
        if len(targets) > 1 and not (
            record.get("mapping_type") == "one_to_many_composition" and record.get("domain") == "island"
        ):
            one_to_many_issues.append(content_id)
        if record.get("status") != "unmapped" and not targets:
            missing_candidates.append(content_id)
        for target in targets:
            if target.get("candidate_review_status") not in {None, *ALLOWED_ASSET_STATES}:
                invalid_candidate_status.append({"content_id": content_id, "status": target.get("candidate_review_status")})
            if not target.get("file_exists"):
                missing_files.append({"content_id": content_id, "path": target.get("path")})
            if not target.get("license_evidence_exists"):
                missing_evidence.append({"content_id": content_id, "path": target.get("path")})
    candidate_paths = Counter(
        target.get("path") for record in records for target in record.get("target_candidates", []) if target.get("path")
    )
    reuse_paths = sorted(path for path, count in candidate_paths.items() if count > 1)
    # Multiple consumers of the same candidate are legal reuse; duplicate writer
    # means a single content ID has more than one owner in its source metadata.
    writer_by_id = defaultdict(set)
    for entry in entries:
        writer_by_id[str(entry.get("id"))].add(str(entry.get("owner_system", "")))
    duplicate_writers = sorted(content_id for content_id, owners in writer_by_id.items() if len({owner for owner in owners if owner}) > 1)
    consumer_breaks = []
    for record in records:
        if record.get("target_candidates") and not record.get("prefab_scene_consumers", {}).get("candidate_runtime_consumers") and not record.get("prefab_scene_consumers", {}).get("registry_declared"):
            consumer_breaks.append(record.get("content_id"))
    stage_stats = {}
    for stage in ("P0", "P1", "P2", "P3"):
        subset = [record for record in records if record.get("gameplay_priority") == stage]
        stage_stats[stage] = {
            "total": len(subset),
            "planned_candidate": sum(record.get("status") == "planned_candidate" for record in subset),
            "unmapped": sum(record.get("status") == "unmapped" for record in subset),
            "blocked_candidate_evidence": sum(record.get("status") == "blocked_candidate_evidence" for record in subset),
            "blocking_reason_counts": _reason_counts(subset),
        }
    domain_stats = {}
    for domain in sorted({str(record.get("domain")) for record in records}):
        subset = [record for record in records if str(record.get("domain")) == domain]
        domain_stats[domain] = {"total": len(subset), "planned_candidate": sum(record.get("status") == "planned_candidate" for record in subset), "unmapped": sum(record.get("status") == "unmapped" for record in subset)}
    queue_comparison = {}
    baseline = root / "tools" / "results" / "d16_a05_replacement_queue_current.json"
    if baseline.exists():
        try:
            value = json.loads(baseline.read_text(encoding="utf-8"))
            queue_comparison = {
                "baseline_path": relpath(root, baseline),
                "baseline_placeholder_count": value.get("placeholder_count", value.get("total_placeholder_count")),
                "registry_placeholder_count": len(entries),
                "count_matches": value.get("placeholder_count", value.get("total_placeholder_count")) in (None, len(entries)) or value.get("placeholder_count", value.get("total_placeholder_count")) == len(entries),
            }
        except (OSError, json.JSONDecodeError):
            queue_comparison = {"baseline_path": relpath(root, baseline), "readable": False}
    audit = {
        "input_placeholder_count": len(entries),
        "output_record_count": len(records),
        "mapped_count": sum(record.get("status") == "planned_candidate" for record in records),
        "unmapped_count": sum(record.get("status") == "unmapped" for record in records),
        "blocked_count": sum(record.get("status") == "blocked_candidate_evidence" for record in records),
        "duplicate_content_id_count": len(duplicate_ids),
        "duplicate_content_ids": duplicate_ids,
        "missing_authoritative_ids": missing_ids,
        "extra_output_ids": extra_ids,
        "duplicate_candidate_path_count": 0,
        "legal_reuse_path_count": len(reuse_paths),
        "legal_reuse_paths": reuse_paths[:100],
        "missing_candidate_count": len(missing_candidates),
        "missing_candidate_content_ids": missing_candidates[:100],
        "missing_license_evidence_count": len(missing_evidence),
        "missing_license_evidence": missing_evidence[:100],
        "missing_file_count": len(missing_files),
        "missing_files": missing_files[:100],
        "invalid_status_count": len(invalid_status),
        "invalid_status": invalid_status,
        "invalid_candidate_status_count": len(invalid_candidate_status),
        "invalid_candidate_status": invalid_candidate_status,
        "invalid_registry_asset_state_count": len(invalid_asset_state),
        "required_field_gap_count": len(required_field_gaps),
        "required_field_gaps": required_field_gaps,
        "one_to_many_issue_count": len(one_to_many_issues),
        "one_to_many_issues": one_to_many_issues,
        "duplicate_writer_count": len(duplicate_writers),
        "duplicate_writers": duplicate_writers,
        "broken_runtime_consumer_count": len(consumer_breaks),
        "broken_runtime_consumers": consumer_breaks[:100],
        "illegal_state_transition_count": sum(
            1 for record in records for target in record.get("target_candidates", [])
            if target.get("candidate_review_status") == "ReleaseApproved"
        ),
        "release_approved_count": sum(
            1 for record in records for target in record.get("target_candidates", [])
            if target.get("candidate_review_status") == "ReleaseApproved"
        ),
        "local_development_ready_count": sum(
            1 for record in records for target in record.get("target_candidates", []) if target.get("local_development_ready")
        ),
        "stage_stats": stage_stats,
        "domain_stats": domain_stats,
        "queue_baseline_comparison": queue_comparison,
        "registry_snapshot_digest": registry_digest(root),
    }
    audit["status"] = "PASS" if all(audit[key] == 0 for key in (
        "duplicate_content_id_count", "missing_candidate_count", "missing_license_evidence_count", "missing_file_count",
        "invalid_status_count", "invalid_candidate_status_count", "invalid_registry_asset_state_count", "required_field_gap_count", "one_to_many_issue_count",
        "duplicate_writer_count", "illegal_state_transition_count"
    )) and not missing_ids and not extra_ids else "BLOCKED"
    return audit


def generate(root: Path, output_map: Path, result_dir: Path) -> dict[str, Any]:
    mapping = build_mapping(root)
    audit = audit_mapping(root, mapping)
    mapping["audit"] = audit
    output_map.parent.mkdir(parents=True, exist_ok=True)
    result_dir.mkdir(parents=True, exist_ok=True)
    output_map.write_text(json.dumps(mapping, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (result_dir / "D18_A07_MACHINE_RESULT.json").write_text(json.dumps({"task": "D18-A07", "status": "PARTIAL", "audit": audit, "deterministic_digest": mapping["deterministic_digest"]}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (result_dir / "D18_A07_CANDIDATE_CATALOG.json").write_text(json.dumps({"candidate_count": len(candidate_catalog(root)), "candidates": candidate_catalog(root)}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    return mapping


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path.cwd())
    parser.add_argument("--output-map", type=Path, required=True)
    parser.add_argument("--result-dir", type=Path, required=True)
    args = parser.parse_args(argv)
    mapping = generate(args.project_root.resolve(), args.output_map, args.result_dir)
    print(json.dumps({"task": "D18-A07", "status": "PARTIAL", "input_placeholder_count": mapping["audit"]["input_placeholder_count"], "output_record_count": mapping["audit"]["output_record_count"], "mapped_count": mapping["audit"]["mapped_count"], "unmapped_count": mapping["audit"]["unmapped_count"], "deterministic_digest": mapping["deterministic_digest"]}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
