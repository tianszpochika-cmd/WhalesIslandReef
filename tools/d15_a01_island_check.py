"""Validate and smoke-test the D15-A01 island content batch.

This checker validates content definitions and simulates the catalog-level
stream/save/restore contract. It does not create runtime world state and does
not replace IslandSystem, EcologySystem, BuildingSystem, QuestSystem or SaveSystem.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List, Tuple


EXPECTED_TEMPLATE_COUNTS = {
    "volcanic_high_island": 8,
    "limestone_karst_island": 10,
    "coral_sandbar_island": 8,
    "mangrove_tidal_island": 4,
    "alluvial_estuary_island": 2,
    "wet_mountain_rainforest_island": 8,
    "leeward_dry_shrub_island": 5,
    "atoll_reef_ring": 5,
}

REQUIRED_ENTRY_FIELDS = (
    "id",
    "schema_version",
    "name",
    "name_en",
    "display_name_key",
    "category",
    "tags",
    "description",
    "gameplay_role",
    "valid_regions",
    "valid_habitats",
    "valid_weather",
    "dependencies",
    "references",
    "asset_refs",
    "source_refs",
    "source_note",
    "source",
    "license",
    "historical_tag",
    "phase",
    "status",
    "owner_system",
    "test_id",
    "reviewer",
    "production_batch",
    "production_stage",
    "behavior_profile",
    "asset_state",
    "model_ref",
    "material_ref",
    "collision_ref",
    "lod_profile",
    "test_cases",
    "quality_level",
    "historical_review",
    "review_evidence_refs",
    "content_warning_ids",
    "skip_allowed",
    "natural_archive",
)

REQUIRED_ARCHIVE_FIELDS = (
    "template_id",
    "template_label",
    "variant_code",
    "historical_names",
    "formation",
    "geology",
    "windward_face",
    "leeward_face",
    "coast_types",
    "freshwater",
    "mountain",
    "caves",
    "reef_features",
    "habitats",
    "zones",
    "resources",
    "building_entrances",
    "quest_entrances",
    "landing",
    "routes",
    "hazards",
    "social_structure",
    "streaming",
    "save_contract",
    "recovery",
)

ID_PATTERN = re.compile(r"^island_[a-z0-9]+(?:_[a-z0-9]+)*$")
TEST_ID_PATTERN = re.compile(r"^D8-A02-D15-A01-ISLAND-[0-9]{3}$")


class IslandCatalogError(ValueError):
    """A deterministic, human-readable island catalog validation failure."""


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise IslandCatalogError(message)


def load_islands(registry_root: Path) -> List[Dict[str, Any]]:
    entries_dir = registry_root / "entries" / "island"
    _require(entries_dir.is_dir(), f"missing island entries directory: {entries_dir}")
    entries: List[Dict[str, Any]] = []
    for path in sorted(entries_dir.glob("*.json")):
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            raise IslandCatalogError(f"invalid JSON: {path}: {exc}") from exc
        _require(isinstance(data, dict), f"island entry must be an object: {path}")
        data["__path"] = path.as_posix()
        entries.append(data)
    return entries


def _check_source_and_license(entry: Dict[str, Any]) -> None:
    source = entry["source"]
    license_data = entry["license"]
    _require(isinstance(source, dict), f"{entry['id']}: source must be an object")
    _require(all(isinstance(source.get(key), str) and source[key] for key in ("kind", "author", "reference")),
             f"{entry['id']}: source.kind/author/reference are required")
    _require(isinstance(license_data, dict), f"{entry['id']}: license must be an object")
    for key in ("status", "holder", "reference"):
        _require(isinstance(license_data.get(key), str) and license_data[key],
                 f"{entry['id']}: license.{key} is required")
    for key in ("commercial_use", "attribution_required", "modification_allowed"):
        _require(isinstance(license_data.get(key), bool), f"{entry['id']}: license.{key} must be boolean")
    _require(license_data["commercial_use"], f"{entry['id']}: commercial authorization is required")


def _is_reviewed_slice_override(entry: Dict[str, Any]) -> bool:
    """Recognize only the explicitly reviewed D16-C08 island slice override.

    The D15 catalog requires an asset reference (``asset_*``) and the standard
    terrain LOD. D16-C08 intentionally promoted one sand patch to a local
    vertical-slice mesh, so that single reviewed record uses a Unity path and a
    slice-specific LOD. Keep the exception evidence-bound instead of accepting
    arbitrary integrated island records.
    """
    evidence_refs = entry.get("review_evidence_refs")
    return (
        entry.get("id") == "island_saint_marys"
        and entry.get("asset_state") == "integrated"
        and entry.get("production_stage") == "slice_final"
        and isinstance(entry.get("model_ref"), str)
        and entry["model_ref"].startswith("game/Assets/Art/SliceFinal/D16C08/")
        and isinstance(entry.get("runtime_prefab_path"), str)
        and entry["runtime_prefab_path"].startswith("game/Assets/Prefabs/SliceFinal/D16C08/")
        and entry.get("lod_profile") == "island_slice_sand_lod3"
        and entry.get("historical_review") in {"Approved", "ApprovedWithWarning"}
        and isinstance(evidence_refs, list)
        and any("D16_C08_SLICE_REVIEW.md" in ref for ref in evidence_refs if isinstance(ref, str))
    )


def _check_archive(entry: Dict[str, Any]) -> None:
    island_id = entry["id"]
    archive = entry["natural_archive"]
    _require(isinstance(archive, dict), f"{island_id}: natural_archive must be an object")
    for field in REQUIRED_ARCHIVE_FIELDS:
        _require(field in archive, f"{island_id}: missing natural_archive.{field}")
    _require(len(archive["coast_types"]) >= 2, f"{island_id}: needs two coast types")
    _require(len(archive["caves"]) >= 2, f"{island_id}: needs two cave records")
    _require(len(archive["reef_features"]) >= 2, f"{island_id}: needs two reef records")
    _require(len(archive["zones"]) == 3, f"{island_id}: needs coast/inland/upper zones")
    _require(len(archive["resources"]) >= 4, f"{island_id}: needs four resource nodes")
    _require(len(archive["building_entrances"]) >= 2, f"{island_id}: needs two building entrances")
    _require(len(archive["quest_entrances"]) >= 1, f"{island_id}: needs a quest entrance")
    _require(len(archive["routes"]) >= 3, f"{island_id}: needs three recovery-capable routes")
    _require(len(archive["hazards"]) >= 3, f"{island_id}: needs three hazards")

    freshwater = archive["freshwater"]
    _require(isinstance(freshwater, dict), f"{island_id}: freshwater must be an object")
    for field in ("source_kind", "availability", "capacity_band", "dry_season_rule", "water_points", "owner_system"):
        _require(field in freshwater, f"{island_id}: missing freshwater.{field}")
    _require(freshwater["owner_system"] == "EcologySystem", f"{island_id}: freshwater owner must be EcologySystem")

    for zone in archive["zones"]:
        for field in ("region_id", "geology", "habitat", "elevation", "water_sources", "coast_access",
                      "hazard_tags", "plant_tags", "animal_tags", "resource_tags", "building_tags",
                      "faction_tags", "quest_tags", "streaming_cell"):
            _require(field in zone, f"{island_id}: zone missing {field}")
    for resource in archive["resources"]:
        for field in ("node_id", "content_id", "node_type", "habitat", "refresh_rule", "depletion_rule", "recovery_rule"):
            _require(field in resource, f"{island_id}: resource missing {field}")
    for building in archive["building_entrances"]:
        for field in ("building_id", "instance_anchor", "placement_zone", "access_route", "owner_state", "inventory_state", "damage_state"):
            _require(field in building, f"{island_id}: building entrance missing {field}")
    for quest in archive["quest_entrances"]:
        for field in ("quest_id", "entrance_id", "zone_id", "access_route", "recovery_route"):
            _require(field in quest, f"{island_id}: quest entrance missing {field}")

    streaming = archive["streaming"]
    _require(isinstance(streaming, dict), f"{island_id}: streaming must be an object")
    for field in ("cells", "load_policy", "unload_policy", "prefetch_policy", "lod_profile", "stream_owner"):
        _require(field in streaming, f"{island_id}: missing streaming.{field}")
    _require(len(streaming["cells"]) >= 3, f"{island_id}: needs three streaming cells")
    _require(streaming["stream_owner"] == "IslandSystem", f"{island_id}: stream owner must be IslandSystem")

    save_contract = archive["save_contract"]
    _require(isinstance(save_contract, dict), f"{island_id}: save_contract must be an object")
    for field in ("snapshot_key", "stable_id", "preserve_fields", "restore_rule", "save_owner", "state_owner"):
        _require(field in save_contract, f"{island_id}: missing save_contract.{field}")
    _require(save_contract["stable_id"] == island_id, f"{island_id}: save stable_id mismatch")
    _require(save_contract["save_owner"] == "SaveSystem", f"{island_id}: save owner must be SaveSystem")
    _require(save_contract["state_owner"] == "IslandSystem", f"{island_id}: state owner must be IslandSystem")


def _check_entry_shape(entry: Dict[str, Any], all_ids: set[str]) -> None:
    island_id = entry.get("id", "<missing>")
    for field in REQUIRED_ENTRY_FIELDS:
        _require(field in entry, f"{island_id}: missing field {field}")
    _require(ID_PATTERN.fullmatch(island_id) is not None, f"{island_id}: invalid island ID")
    _require(entry["schema_version"] == 1, f"{island_id}: schema_version must be 1")
    _require(entry["category"] == "island", f"{island_id}: category must be island")
    _require(entry["owner_system"] == "IslandSystem", f"{island_id}: owner must be IslandSystem")
    _require(entry["phase"] == "D15", f"{island_id}: phase must be D15")
    _require(entry["status"] in {"production", "verified"}, f"{island_id}: invalid production status")
    _require(entry["production_batch"] == "D15-A01*", f"{island_id}: incorrect production batch")
    # D16-C08 may promote the explicitly reviewed Saint Mary's vertical-slice
    # asset without changing the D15 batch identity. Keep that post-production
    # stage distinct from the ordinary D15 catalog/integrated/verified values.
    _require(entry["production_stage"] in {"cataloged", "integrated", "verified", "slice_final"},
             f"{island_id}: invalid production stage")
    _require(entry["quality_level"] in {"Q2", "Q3"}, f"{island_id}: island batch must be Q2 or Q3")
    _require(entry["asset_state"] in {"placeholder", "source_ready", "integrated", "verified"}, f"{island_id}: invalid asset state")
    _require(entry["asset_state"] == "placeholder" or entry["model_ref"], f"{island_id}: missing model reference")
    slice_override = _is_reviewed_slice_override(entry)
    _require(
        entry["lod_profile"] == "island_terrain_lod3" or slice_override,
        f"{island_id}: wrong LOD profile",
    )
    _require(TEST_ID_PATTERN.fullmatch(entry["test_id"]) is not None, f"{island_id}: invalid test_id")
    _require(len(entry["test_cases"]) >= 3, f"{island_id}: needs load/save/restore test cases")
    _require(entry["historical_review"] in {"Approved", "ApprovedWithWarning"}, f"{island_id}: review not approved")
    _require(
        entry["model_ref"].startswith("asset_") or slice_override,
        f"{island_id}: model_ref must be an asset reference",
    )
    _check_source_and_license(entry)
    for target in entry["references"] + entry["asset_refs"]:
        _require(target in all_ids, f"{island_id}: missing reference {target}")
    _check_archive(entry)


def _stream_save_restore(entry: Dict[str, Any]) -> None:
    archive = entry["natural_archive"]
    loaded = {
        "island_id": entry["id"],
        "cells": list(archive["streaming"]["cells"]),
        "template_id": archive["template_id"],
        "stable_save_key": archive["save_contract"]["snapshot_key"],
        "resource_nodes": [resource["node_id"] for resource in archive["resources"]],
    }
    serialized = json.dumps(loaded, ensure_ascii=False, sort_keys=True)
    restored = json.loads(serialized)
    _require(restored["island_id"] == entry["id"], f"{entry['id']}: restore changed stable ID")
    _require(restored["cells"] == loaded["cells"], f"{entry['id']}: restore changed streaming cells")
    _require(restored["stable_save_key"] == archive["save_contract"]["snapshot_key"], f"{entry['id']}: restore changed save key")
    _require(restored["resource_nodes"] == loaded["resource_nodes"], f"{entry['id']}: restore changed resource nodes")


def validate_catalog(registry_root: Path) -> Tuple[int, int, Dict[str, int]]:
    entries = load_islands(registry_root)
    _require(len(entries) == 50, f"expected 50 island entries, found {len(entries)}")
    ids = [entry.get("id") for entry in entries]
    _require(len(set(ids)) == len(ids), "duplicate island IDs")
    all_registry_ids = {
        path.stem
        for path in (registry_root / "entries").rglob("*.json")
    }
    all_registry_ids.update(ids)
    for entry in entries:
        _check_entry_shape(entry, all_registry_ids)

    counts: Dict[str, int] = {}
    for entry in entries:
        template_id = entry["natural_archive"]["template_id"]
        counts[template_id] = counts.get(template_id, 0) + 1
    _require(counts == EXPECTED_TEMPLATE_COUNTS, f"template counts mismatch: {counts}")

    sample = entries[::5]
    _require(len(sample) >= 10, f"stream/save/restore sample is too small: {len(sample)}")
    for entry in sample:
        _stream_save_restore(entry)
    return len(entries), len(sample), counts


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Validate the D15-A01 island batch")
    parser.add_argument("registry_root", nargs="?", default="game/Assets/Resources/ContentRegistry")
    args = parser.parse_args(list(argv) if argv is not None else None)
    try:
        entry_count, sample_count, counts = validate_catalog(Path(args.registry_root))
    except (IslandCatalogError, OSError) as exc:
        print(f"D15-A01 island catalog FAILED: {exc}")
        return 1
    print(f"D15-A01 island catalog PASS: islands={entry_count}, templates={len(counts)}, stream_save_restore_sample={sample_count}")
    print("template_counts=" + json.dumps(counts, ensure_ascii=False, sort_keys=True))
    return 0


if __name__ == "__main__":
    sys.exit(main())
