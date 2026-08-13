"""D15-A03 animal/fish content batch validator.

This is a content gate only. It validates immutable definitions and the
EcologySystem handoff metadata; it never mutates runtime ecology state.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from pathlib import Path
from typing import Any


BATCH = "D15-A03*"
EXPECTED_COUNTS = {
    "terrestrial": 40,
    "birds": 25,
    "reptiles": 25,
    "insects": 20,
    "freshwater_estuary": 25,
    "nearshore_fish": 50,
    "sharks_rays": 20,
    "sea_turtles": 15,
    "whales_dolphins": 15,
    "crustaceans_softbody": 35,
    "coastal_birds": 30,
}
ALLOWED_BEHAVIOR_LEVELS = {"L0", "L1", "L2", "L3", "L4"}
ALLOWED_LEGAL_STATUS = {"legal", "regulated", "protected", "licensed"}
ALLOWED_REVIEW = {"Approved", "ApprovedWithWarning"}
REQUIRED_FIELDS = (
    "id",
    "schema_version",
    "name",
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
    "representative_sample",
    "production_stage",
    "d15_category",
    "behavior_level",
    "behavior_profile",
    "habitat_profile",
    "refresh_profile",
    "food_chain_profile",
    "danger_profile",
    "legal_profile",
    "performance_budget",
    "asset_state",
    "model_ref",
    "material_ref",
    "collision_ref",
    "lod_profile",
    "test_cases",
    "historical_review",
    "review_evidence_refs",
    "content_warning_ids",
    "context_note_key",
    "skip_allowed",
    "presentation_levels",
    "quality_level",
    "authorization",
)


class BatchError(AssertionError):
    pass


def require(condition: bool, message: str) -> None:
    if not condition:
        raise BatchError(message)


def load_entries(root: Path) -> list[tuple[Path, dict[str, Any]]]:
    entries: list[tuple[Path, dict[str, Any]]] = []
    for directory in ("animal", "fish"):
        for path in sorted((root / "entries" / directory).glob("*.json")):
            value = json.loads(path.read_text(encoding="utf-8"))
            require(isinstance(value, dict), f"{path}: JSON root must be an object")
            entries.append((path, value))
    return entries


def validate(root: Path) -> dict[str, Any]:
    entries = load_entries(root)
    batch = [(path, value) for path, value in entries if value.get("production_batch") == BATCH]
    require(len(batch) == 300, f"expected 300 D15-A03 entries, got {len(batch)}")

    ids: set[str] = set()
    test_ids: set[str] = set()
    category_counts: Counter[str] = Counter()
    for path, value in batch:
        entry_id = value.get("id")
        require(isinstance(entry_id, str) and entry_id, f"{path}: missing id")
        require(entry_id not in ids, f"duplicate id: {entry_id}")
        ids.add(entry_id)
        require(path.stem == entry_id, f"{path}: filename must equal id")
        require(entry_id.startswith(("animal_", "fish_")), f"{entry_id}: invalid D15 prefix")
        expected_category = "animal" if entry_id.startswith("animal_") else "fish"
        require(value.get("category") == expected_category, f"{entry_id}: category/prefix mismatch")
        require(value.get("owner_system") == "EcologySystem", f"{entry_id}: owner must be EcologySystem")
        for field in REQUIRED_FIELDS:
            require(field in value, f"{entry_id}: missing field {field}")

        test_id = value["test_id"]
        require(isinstance(test_id, str) and test_id not in test_ids, f"{entry_id}: duplicate/empty test_id")
        test_ids.add(test_id)
        require(re.fullmatch(r"D8-A02-[A-Z0-9-]+", test_id), f"{entry_id}: test_id is not registered")

        category = value["d15_category"]
        require(category in EXPECTED_COUNTS, f"{entry_id}: unknown d15_category {category}")
        category_counts[category] += 1
        require(value["behavior_level"] in ALLOWED_BEHAVIOR_LEVELS, f"{entry_id}: invalid behavior level")
        require(value["behavior_profile"].startswith("ecology."), f"{entry_id}: behavior must be an EcologySystem profile")
        require(value["habitat_profile"]["primary"] in value["valid_habitats"], f"{entry_id}: habitat profile not in valid_habitats")
        require(value["refresh_profile"]["interval_days"] >= 1, f"{entry_id}: refresh interval must be positive")
        require(value["food_chain_profile"]["role"], f"{entry_id}: food-chain role missing")
        require(value["danger_profile"]["level"], f"{entry_id}: danger level missing")
        legal = value["legal_profile"]
        require(legal["status"] in ALLOWED_LEGAL_STATUS, f"{entry_id}: invalid legal status")
        require(isinstance(legal["permit_required"], bool), f"{entry_id}: permit flag missing")
        budget = value["performance_budget"]
        for key, maximum in (
            ("lod0_triangles", 12000),
            ("lod1_triangles", 5000),
            ("lod2_triangles", 1500),
            ("lod3_triangles", 500),
        ):
            require(isinstance(budget[key], int) and 0 < budget[key] <= maximum, f"{entry_id}: invalid {key}")
        require(0 < budget["active_distance_m"] <= 60, f"{entry_id}: invalid active distance")
        require(0 < budget["update_hz"] <= 10, f"{entry_id}: invalid update rate")
        require(0 < budget["max_active"] <= 64, f"{entry_id}: invalid active-object budget")
        license_data = value["license"]
        authorization = value["authorization"]
        require(license_data.get("commercial_use") is True, f"{entry_id}: license is not commercial")
        require(authorization.get("commercial_use") is True, f"{entry_id}: authorization is not commercial")
        require(authorization.get("rights_holder"), f"{entry_id}: authorization holder missing")
        require(authorization.get("evidence_ref"), f"{entry_id}: authorization evidence missing")
        review = value["historical_review"]
        require(review in ALLOWED_REVIEW, f"{entry_id}: invalid historical review")
        if review == "ApprovedWithWarning":
            require(value["content_warning_ids"], f"{entry_id}: warning missing for warned content")
            require(value["skip_allowed"] is True, f"{entry_id}: warned content must be skippable")
        require(value["asset_state"] == "placeholder", f"{entry_id}: batch must disclose placeholder state")
        require(value["quality_level"] == "Q2", f"{entry_id}: batch quality must be Q2")
        # A negative audit note may legitimately say “no LLM”. Inspect the
        # executable handoff fields instead of rejecting that documentation.
        for field in ("behavior_profile", "model_ref", "material_ref", "collision_ref"):
            field_value = str(value.get(field, "")).lower()
            require("llm" not in field_value and "http" not in field_value and "openai" not in field_value,
                    f"{entry_id}: forbidden runtime dependency in {field}")

    require(category_counts == Counter(EXPECTED_COUNTS), f"category counts mismatch: {dict(category_counts)}")
    for sample in ("animal_green_turtle", "animal_domestic_chicken", "fish_mackerel_school"):
        sample_value = next((value for _, value in batch if value.get("id") == sample), None)
        require(sample_value is not None, f"representative sample missing from batch: {sample}")
        require(sample_value["owner_system"] == "EcologySystem", f"{sample}: EcologySystem takeover missing")
        require(sample_value["test_cases"] == ["D15-A03-LOAD", "D15-A03-ECOLOGY-TAKEOVER", "D15-A03-SAVE-RESTORE"], f"{sample}: test entry incomplete")

    return {
        "entries": len(batch),
        "category_counts": dict(sorted(category_counts.items())),
        "animal_entries": sum(1 for _, value in batch if value["category"] == "animal"),
        "fish_entries": sum(1 for _, value in batch if value["category"] == "fish"),
        "test_ids": len(test_ids),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("registry_root", type=Path)
    args = parser.parse_args()
    result = validate(args.registry_root)
    print(f"D15-A03 animal batch PASS: {result['entries']} entries; "
          f"{result['animal_entries']} animal / {result['fish_entries']} fish; "
          f"{result['test_ids']} unique test IDs")
    for category, count in result["category_counts"].items():
        print(f"  {category}: {count}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
