"""Validate the D15-A07 character and pet production batch."""

from __future__ import annotations

import argparse
import json
from pathlib import Path
from typing import Any, Iterable


REQUIRED_CREW_FIELDS = {
    "id",
    "culture_id",
    "career_id",
    "source_ref",
    "authorization_id",
    "appearance_combo_id",
    "behavior_profile",
    "asset_state",
    "model_ref",
    "material_ref",
    "collision_ref",
    "lod_profile",
    "test_id",
    "test_cases",
    "historical_review",
    "review_evidence_refs",
    "content_warning_ids",
    "skip_allowed",
}
REQUIRED_NPC_FIELDS = REQUIRED_CREW_FIELDS | {
    "instance_id",
    "preset_id",
    "location_ref",
    "spawn_anchor_id",
    "position",
    "composition_key",
    "save_key",
}
REQUIRED_PET_FIELDS = REQUIRED_CREW_FIELDS | {"owner_system"}
FORBIDDEN_KEYS = {
    "relation",
    "relation_state",
    "wage",
    "wage_amount",
    "health",
    "health_value",
    "reward",
    "reward_result",
    "combat",
    "combat_result",
    "damage",
    "damage_result",
}


def _walk_keys(value: Any) -> Iterable[str]:
    if isinstance(value, dict):
        for key, child in value.items():
            yield key
            yield from _walk_keys(child)
    elif isinstance(value, list):
        for child in value:
            yield from _walk_keys(child)


def _require(record: dict[str, Any], fields: set[str], label: str, errors: list[str]) -> None:
    missing = sorted(fields - set(record))
    if missing:
        errors.append(f"{label}: missing fields {', '.join(missing)}")


def _validate_common(record: dict[str, Any], label: str, errors: list[str]) -> None:
    if record.get("production_batch") != "D15-A07*":
        errors.append(f"{label}: production_batch must be D15-A07*")
    if record.get("production_stage") not in {"cataloged", "integrated", "verified"}:
        errors.append(f"{label}: invalid production_stage")
    if record.get("quality_level") != "Q2":
        errors.append(f"{label}: batch output must be Q2 until final asset verification")
    if record.get("asset_state") != "placeholder":
        errors.append(f"{label}: placeholder batch must declare asset_state=placeholder")
    if not record.get("source_ref") or not record.get("authorization_id"):
        errors.append(f"{label}: source_ref and authorization_id are required")
    if record.get("historical_review") not in {"Approved", "ApprovedWithWarning"}:
        errors.append(f"{label}: historical_review is not approved")
    if not isinstance(record.get("review_evidence_refs"), list) or not record["review_evidence_refs"]:
        errors.append(f"{label}: review_evidence_refs is required")
    if not isinstance(record.get("content_warning_ids"), list):
        errors.append(f"{label}: content_warning_ids must be an array")
    if not isinstance(record.get("test_cases"), list) or len(record["test_cases"]) < 3:
        errors.append(f"{label}: load/owner/save test cases are required")
    if not isinstance(record.get("skip_allowed"), bool):
        errors.append(f"{label}: skip_allowed must be boolean")
    if not isinstance(record.get("test_id"), str) or not record["test_id"]:
        errors.append(f"{label}: test_id is required")
    for key in _walk_keys(record):
        if key in FORBIDDEN_KEYS:
            errors.append(f"{label}: forbidden runtime field {key}")


def validate_batch(path: Path) -> list[str]:
    errors: list[str] = []
    try:
        batch = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return [f"invalid batch JSON: {exc}"]

    if batch.get("schema_version") != 1:
        errors.append("batch schema_version must be 1")
    if batch.get("production_batch") != "D15-A07*":
        errors.append("batch production_batch must be D15-A07*")
    authorization = batch.get("authorization")
    for key in (
        "authorization_id",
        "rights_holder",
        "license_ref",
        "source_url_or_path",
        "acquired_at",
        "evidence_ref",
    ):
        if not isinstance(authorization, dict) or not authorization.get(key):
            errors.append(f"authorization.{key} is required")
    if not isinstance(authorization, dict) or authorization.get("commercial_use") is not True:
        errors.append("authorization.commercial_use must be true")

    crews = batch.get("crew_presets", [])
    npcs = batch.get("npc_instances", [])
    pets = batch.get("pet_variants", [])
    combos = batch.get("appearance_combinations", [])
    if len(crews) != 60:
        errors.append(f"crew preset count is {len(crews)}, expected 60")
    if not 500 <= len(npcs) <= 1000:
        errors.append(f"NPC instance count is {len(npcs)}, expected 500-1000")
    if len(pets) != 12:
        errors.append(f"named variant count is {len(pets)}, expected 12")
    if len(combos) < 12:
        errors.append("at least 12 appearance combinations are required")

    all_ids: list[str] = []
    test_ids: list[str] = []
    for index, record in enumerate(crews, 1):
        label = f"crew[{index}]"
        if not isinstance(record, dict):
            errors.append(f"{label}: record must be an object")
            continue
        _require(record, REQUIRED_CREW_FIELDS, label, errors)
        _validate_common(record, label, errors)
        if record.get("owner_system") != "CrewSystem":
            errors.append(f"{label}: owner_system must be CrewSystem")
        all_ids.append(str(record.get("id", "")))
        test_ids.append(str(record.get("test_id", "")))

    for index, record in enumerate(npcs, 1):
        label = f"npc[{index}]"
        if not isinstance(record, dict):
            errors.append(f"{label}: record must be an object")
            continue
        _require(record, REQUIRED_NPC_FIELDS, label, errors)
        _validate_common(record, label, errors)
        if record.get("owner_system") not in {None, "CrewSystem"}:
            errors.append(f"{label}: NPC owner must remain CrewSystem")
        if not isinstance(record.get("position"), list) or len(record["position"]) != 3:
            errors.append(f"{label}: position must be a three-value array")
        elif any(not isinstance(value, (int, float)) for value in record["position"]):
            errors.append(f"{label}: position values must be numeric")
        all_ids.append(str(record.get("instance_id", "")))
        test_ids.append(str(record.get("test_id", "")))

    family_counts: dict[str, int] = {}
    for index, record in enumerate(pets, 1):
        label = f"pet[{index}]"
        if not isinstance(record, dict):
            errors.append(f"{label}: record must be an object")
            continue
        _require(record, REQUIRED_PET_FIELDS, label, errors)
        _validate_common(record, label, errors)
        if not isinstance(record.get("source"), dict) or not record["source"].get("reference"):
            errors.append(f"{label}: source metadata is required")
        if not isinstance(record.get("license"), dict) or record["license"].get("commercial_use") is not True:
            errors.append(f"{label}: commercial license metadata is required")
        family = record.get("appearance_profile", {}).get("family")
        family_counts[family] = family_counts.get(family, 0) + 1
        expected_owner = "AnimalCareSystem" if family in {"cat", "dog"} else "EcologySystem"
        if record.get("owner_system") != expected_owner:
            errors.append(f"{label}: owner_system must be {expected_owner}")
        all_ids.append(str(record.get("id", "")))
        test_ids.append(str(record.get("test_id", "")))

    if family_counts != {"cat": 3, "dog": 3, "poultry": 3, "livestock": 3}:
        errors.append(f"variant family counts are {family_counts}, expected 3 each")
    if len(set(all_ids)) != len(all_ids):
        errors.append("content and instance IDs must be unique")
    if len(set(test_ids)) != len(test_ids):
        errors.append("test IDs must be unique")
    if len({record.get("composition_key") for record in npcs}) != len(npcs):
        errors.append("NPC composition_key values must be unique")
    if len({record.get("spawn_anchor_id") for record in npcs}) != len(npcs):
        errors.append("NPC spawn_anchor_id values must be unique")

    for index, combo in enumerate(combos, 1):
        label = f"appearance[{index}]"
        for key in ("id", "culture_id", "career_id", "hair_id", "clothing_ids", "career_equipment_id", "source_ref", "authorization_id", "test_id"):
            if not combo.get(key):
                errors.append(f"{label}: {key} is required")
        if not all(isinstance(value, str) and value.startswith("outfit_") for value in combo.get("clothing_ids", [])):
            errors.append(f"{label}: clothing_ids must use outfit_ content IDs")
        if not str(combo.get("hair_id", "")).startswith("outfit_"):
            errors.append(f"{label}: hair_id must use an outfit_ content ID")

    counts = batch.get("scope", {}).get("variant_breakdown", {})
    if counts != {"cat": 3, "dog": 3, "poultry": 3, "livestock": 3}:
        errors.append("scope.variant_breakdown does not match the frozen target")
    sampling = batch.get("sampling", {})
    if sampling.get("crew", {}).get("sample_count") != 6:
        errors.append("crew sampling minimum must be 6")
    if sampling.get("npc_instances", {}).get("sample_count") != 20:
        errors.append("NPC sampling minimum must be 20")
    if sampling.get("pet_variants", {}).get("sample_count") != 4:
        errors.append("pet sampling must cover all four families")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate the D15-A07 content batch")
    parser.add_argument("batch", type=Path)
    args = parser.parse_args()
    batch_path = args.batch
    if batch_path.is_dir():
        candidates = (
            batch_path / "batches" / "d15_a07" / "d15_a07_character_pet_batch.json",
            batch_path / "d15_a07" / "d15_a07_character_pet_batch.json",
            batch_path / "d15_a07_character_pet_batch.json",
        )
        batch_path = next((candidate for candidate in candidates if candidate.is_file()), candidates[0])
    errors = validate_batch(batch_path)
    if errors:
        for error in errors:
            print(f"ERROR {error}")
        print(f"D15-A07 batch FAILED: {len(errors)} error(s)")
        return 1
    batch = json.loads(batch_path.read_text(encoding="utf-8"))
    print(
        "D15-A07 batch OK: "
        f"{len(batch['crew_presets'])} crew presets, "
        f"{len(batch['npc_instances'])} NPC instances, "
        f"{len(batch['pet_variants'])} named variants"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
