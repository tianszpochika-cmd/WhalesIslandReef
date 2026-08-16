"""Read-only D19-A05 configuration audit.

This checks the project-owned lighting catalog and its intake-eligible HDRI
references. It deliberately does not promote resource state or edit Unity
scenes/settings outside D19-A05.
"""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
CATALOG = ROOT / "game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json"
SCENE = ROOT / "game/Assets/Scenes/D19/Work/D19_Lighting_Work.unity"
INTAKE = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
WEATHER = ROOT / "game/Assets/Scripts/Systems/D9/D9WeatherSeaState.cs"
TIME = ROOT / "game/Assets/Scripts/Systems/D2/DayNightSystem.cs"
OUTPUT = ROOT / "game/tools/results/D19/D19-A05/D19_A05_STATIC_AUDIT.json"


def in_window(value: float, start: float, end: float) -> bool:
    if end - start >= 0.999999:
        return True
    return value >= start and value <= end if start <= end else value >= start or value <= end


def choose(catalog: dict, weather: str, time: float, visibility: float) -> str:
    best = None
    for mapping in catalog["mappings"]:
        if mapping["weather_state"] not in ("*", weather):
            continue
        if not in_window(time, mapping["time_start"], mapping["time_end"]):
            continue
        if visibility > mapping["max_visibility"]:
            continue
        if best is None or mapping["priority"] > best["priority"]:
            best = mapping
    return best["profile_id"] if best else "clear_day"


def main() -> int:
    errors: list[str] = []
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    intake = json.loads(INTAKE.read_text(encoding="utf-8"))
    eligible_sky_entries = [
        entry
        for entry in intake["eligible_entries"]
        if entry.get("intake_eligible") is True
        and entry.get("manifest_id") == "pack_polyhaven_hdri_sky_references_v1"
    ]
    eligible = {entry["content_id"] for entry in eligible_sky_entries}
    profiles = catalog.get("profiles", [])
    profile_ids = {profile.get("profile_id") for profile in profiles}
    expected = {"clear_day", "overcast_rain", "twilight_night", "fog", "storm"}
    if (catalog.get("schema_version") != 1 or catalog.get("task") != "D19-A05" or
            catalog.get("owner_system") != "D19-A05" or not catalog.get("external_asset_policy")):
        errors.append("catalog_header_invalid")
    if profile_ids != expected or len(profiles) != 5:
        errors.append("profile_set_must_be_exactly_five")
    for profile in profiles:
        for required in ("owner_system", "test_id", "sky_reference_asset", "low", "medium", "high"):
            if not profile.get(required):
                errors.append(f"missing:{profile.get('profile_id')}:{required}")
        if profile.get("owner_system") != "D19-A05":
            errors.append(f"owner:{profile.get('profile_id')}")
        for required in ("source_note", "test_id", "sky_reference_asset"):
            if not profile.get(required):
                errors.append(f"missing:{profile.get('profile_id')}:{required}")
        if profile.get("sky_reference_asset") not in eligible:
            errors.append(f"sky_reference_not_intake_eligible:{profile.get('profile_id')}:{profile.get('sky_reference_asset')}")
        if profile.get("depth_of_field") or profile.get("motion_blur") or profile.get("flicker_hz", -1) != 0:
            errors.append(f"unsafe_effect:{profile.get('profile_id')}")
        if not -4 <= profile.get("exposure_ev", 99) <= 4:
            errors.append(f"exposure:{profile.get('profile_id')}")
    for index, mapping in enumerate(catalog.get("mappings", [])):
        if not 0.0 <= mapping.get("time_start", -1.0) <= 1.0 or not 0.0 <= mapping.get("time_end", -1.0) <= 1.0:
            errors.append(f"mapping_time:{index}")
    mapping_ids = {mapping.get("profile_id") for mapping in catalog.get("mappings", [])}
    if mapping_ids != expected:
        errors.append("mapping_does_not_cover_all_profiles")
    samples = {
        "clear_day": choose(catalog, "Calm", 0.35, 0.95),
        "overcast_rain": choose(catalog, "Rain", 0.55, 0.65),
        "twilight_night": choose(catalog, "Calm", 0.90, 0.95),
        "fog": choose(catalog, "Rain", 0.30, 0.65),
        "storm": choose(catalog, "Storm", 0.35, 0.25),
    }
    if set(samples.values()) != expected:
        errors.append(f"sample_mapping_incomplete:{samples}")
    if not SCENE.is_file():
        errors.append("work_scene_missing")
    else:
        scene_text = SCENE.read_text(encoding="utf-8")
        if "InitialHarbor" in scene_text:
            errors.append("formal_scene_reference_forbidden")
        if "D19_Lighting_Weather_Adapter" not in scene_text:
            errors.append("adapter_missing_from_work_scene")
    if not WEATHER.is_file() or not TIME.is_file():
        errors.append("read_only_domain_source_missing")

    result = {
        "task": "D19-A05",
        "status": "PASS" if not errors else "BLOCKED",
        "read_only": True,
        "profile_count": len(profiles),
        "profile_ids": sorted(profile_ids),
        "intake_eligible_sky_reference_count": len(eligible_sky_entries),
        "sample_mapping": samples,
        "domain_writer_files_unchanged_by_scope": True,
        "errors": errors,
    }
    OUTPUT.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if not errors else 2


if __name__ == "__main__":
    raise SystemExit(main())
