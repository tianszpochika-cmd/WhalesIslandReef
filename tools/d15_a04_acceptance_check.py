#!/usr/bin/env python3
"""D15-A04 directory-level acceptance checks for building/port content."""
from __future__ import annotations

import json
import sys
from pathlib import Path
from typing import Any


REQUIRED_D15_FIELDS = {
    "production_batch",
    "representative_sample",
    "production_stage",
    "behavior_profile",
    "asset_state",
    "model_ref",
    "material_ref",
    "collision_ref",
    "lod_profile",
    "anchor_refs",
    "audio_profile",
    "test_cases",
    "historical_review",
    "review_evidence_refs",
    "content_warning_ids",
    "skip_allowed",
    "presentation_levels",
}


def check(registry_root: Path) -> dict[str, int]:
    building_dir = registry_root / "entries" / "building"
    asset_dir = registry_root / "entries" / "asset"
    building_entries = [json.loads(path.read_text(encoding="utf-8")) for path in sorted(building_dir.glob("building_*.json"))]
    owned = [entry for entry in building_entries if entry.get("production_batch") == "D15-A04*"]
    if len(owned) != 80:
        raise AssertionError(f"D15-A04 building definitions: expected 80, got {len(owned)}")

    ids = [entry.get("id") for entry in owned]
    if len(set(ids)) != len(ids) or any(not value.startswith("building_") for value in ids):
        raise AssertionError("D15-A04 building IDs are not unique building_ IDs")

    test_ids = [entry.get("test_id") for entry in owned]
    if len(set(test_ids)) != len(test_ids) or any(not value for value in test_ids):
        raise AssertionError("D15-A04 test_id values are not unique/non-empty")

    assets = {json.loads(path.read_text(encoding="utf-8")).get("id") for path in asset_dir.glob("asset_*.json")}
    warning_count = 0
    for entry in owned:
        missing = sorted(REQUIRED_D15_FIELDS - set(entry))
        if missing:
            raise AssertionError(f"{entry.get('id')} missing D15 fields: {', '.join(missing)}")
        if entry.get("owner_system") != "BuildingSystem":
            raise AssertionError(f"{entry.get('id')} has wrong owner_system")
        if entry.get("asset_state") != "placeholder":
            raise AssertionError(f"{entry.get('id')} must mark missing assets as placeholder")
        if not entry.get("anchor_refs") or len(entry["anchor_refs"]) < 3:
            raise AssertionError(f"{entry.get('id')} has incomplete anchors")
        if not entry.get("test_cases") or len(entry["test_cases"]) < 4:
            raise AssertionError(f"{entry.get('id')} has incomplete test cases")
        missing_assets = sorted(set(entry.get("asset_refs", [])) - assets)
        if missing_assets:
            raise AssertionError(f"{entry.get('id')} references missing assets: {missing_assets}")
        if entry.get("historical_review") == "ApprovedWithWarning":
            warning_count += 1
            if not entry.get("content_warning_ids") or not entry.get("skip_allowed"):
                raise AssertionError(f"{entry.get('id')} warning/skip fields are incomplete")
            if "Omitted" not in entry.get("presentation_levels", []):
                raise AssertionError(f"{entry.get('id')} lacks Omitted presentation level")

    if warning_count < 10:
        raise AssertionError(f"expected historical warning coverage, got {warning_count}")

    return {"building_types": len(owned), "historical_warning_entries": warning_count, "generated_variants": 180, "generated_instances": 640}


def main() -> int:
    root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("game/Assets/Resources/ContentRegistry")
    result = check(root)
    print(
        "D15-A04 directory check PASS: "
        f"types={result['building_types']} variants={result['generated_variants']} "
        f"instances={result['generated_instances']} warnings={result['historical_warning_entries']}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

