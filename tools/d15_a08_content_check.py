"""Validate the D15-A08 quest, audio and localization production batch.

The checker is catalog-only: it never writes QuestSystem, RewardSystem,
RelationshipSystem or EconomySystem state. It enforces the D12 fact/event and
pending-reward-request contract, the D14 review gate, and the D15-A08 quantity
and localization requirements.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Dict, Iterable, List, Mapping, Sequence, Set, Tuple


class D15A08CatalogError(ValueError):
    """Deterministic validation failure for the D15-A08 content batch."""


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise D15A08CatalogError(message)


def _load_json(path: Path) -> Any:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise D15A08CatalogError(f"invalid JSON: {path}: {exc}") from exc


def _entries(registry_root: Path, folder: str, batch: str = "D15-A08*") -> List[Dict[str, Any]]:
    directory = registry_root / "entries" / folder
    _require(directory.is_dir(), f"missing content directory: {directory}")
    values: List[Dict[str, Any]] = []
    for path in sorted(directory.glob("*.json")):
        data = _load_json(path)
        if isinstance(data, dict) and data.get("production_batch") == batch:
            data["__path"] = path
            values.append(data)
    return values


def _text(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _source_license(entry: Mapping[str, Any]) -> None:
    source = entry.get("source")
    _require(isinstance(source, dict), f"{entry.get('id')}: source object is required")
    _require(all(_text(source.get(key)) for key in ("kind", "author", "reference")),
             f"{entry.get('id')}: source.kind/author/reference are required")
    license_data = entry.get("license")
    _require(isinstance(license_data, dict), f"{entry.get('id')}: license object is required")
    _require(all(_text(license_data.get(key)) for key in ("status", "holder", "reference")),
             f"{entry.get('id')}: license status/holder/reference are required")
    _require(license_data.get("commercial_use") is True,
             f"{entry.get('id')}: commercial authorization is required")


def _registry_ids(registry_root: Path) -> Set[str]:
    return {path.stem for path in (registry_root / "entries").rglob("*.json")}


def _review(entry: Mapping[str, Any]) -> None:
    status = entry.get("historical_review")
    _require(status in {"Approved", "ApprovedWithWarning"},
             f"{entry.get('id')}: historical_review must be approved")
    evidence = entry.get("review_evidence_refs")
    _require(isinstance(evidence, list) and evidence and all(_text(value) for value in evidence),
             f"{entry.get('id')}: review evidence is required")
    warnings = entry.get("content_warning_ids")
    _require(isinstance(warnings, list), f"{entry.get('id')}: content_warning_ids must be a list")
    _require(isinstance(entry.get("skip_allowed"), bool),
             f"{entry.get('id')}: skip_allowed must be boolean")
    _require(isinstance(entry.get("presentation_levels"), list) and entry["presentation_levels"],
             f"{entry.get('id')}: presentation_levels are required")
    if status == "ApprovedWithWarning":
        _require(bool(warnings), f"{entry.get('id')}: warning review needs warning IDs")
        _require(entry.get("skip_allowed") is True,
                 f"{entry.get('id')}: warning review must allow skip")
        _require("Omitted" in entry["presentation_levels"],
                 f"{entry.get('id')}: warning review needs Omitted presentation level")


def _base_entry(entry: Mapping[str, Any], registry_ids: Set[str]) -> None:
    entry_id = entry.get("id", "<missing>")
    _require(_text(entry_id), "D15-A08 entry ID is required")
    _require(entry_id.startswith(("quest_", "asset_")), f"{entry_id}: invalid D15-A08 prefix")
    _require(entry.get("schema_version") == 1, f"{entry_id}: schema_version must be 1")
    _require(entry.get("phase") == "D15", f"{entry_id}: phase must be D15")
    _require(entry.get("status") in {"production", "verified"}, f"{entry_id}: not production-ready")
    _require(_text(entry.get("display_name_key")), f"{entry_id}: display_name_key is required")
    _require(isinstance(entry.get("source_refs"), list) and entry["source_refs"],
             f"{entry_id}: source_refs are required")
    _source_license(entry)
    _review(entry)
    for target in entry.get("references", []) + entry.get("asset_refs", []):
        _require(target in registry_ids, f"{entry_id}: missing reference {target}")


def _quest_validation(quests: Sequence[Mapping[str, Any]], registry_ids: Set[str]) -> Set[str]:
    _require(len(quests) == 125, f"expected 125 D15-A08 quests, found {len(quests)}")
    ids = [entry.get("id") for entry in quests]
    _require(len(set(ids)) == len(ids), "duplicate D15-A08 quest IDs")
    test_ids = [entry.get("test_id") for entry in quests]
    _require(len(set(test_ids)) == len(test_ids), "duplicate D15-A08 quest test IDs")
    mainline = [entry for entry in quests if entry.get("quest_track") == "mainline"]
    side = [entry for entry in quests if entry.get("quest_track") == "side"]
    _require(len(mainline) == 5, f"expected 5 mainline chapters, found {len(mainline)}")
    _require(sorted(entry.get("chapter") for entry in mainline) == [1, 2, 3, 4, 5],
             "mainline chapters must be exactly 1 through 5")
    _require(len(side) == 120, f"expected 120 side templates, found {len(side)}")
    _require(sorted(entry.get("template_number") for entry in side) == list(range(1, 121)),
             "side template numbers must be exactly 1 through 120")
    keys: Set[str] = set()
    for entry in quests:
        _base_entry(entry, registry_ids)
        entry_id = entry["id"]
        _require(entry.get("owner_system") == "QuestSystem", f"{entry_id}: QuestSystem must own task state")
        _require(entry.get("production_stage") in {"cataloged", "verified"},
                 f"{entry_id}: invalid production_stage")
        _require(entry.get("quality_level") in {"Q2", "Q3"}, f"{entry_id}: invalid quest quality level")
        objectives = entry.get("objectives")
        _require(isinstance(objectives, list) and len(objectives) >= 2,
                 f"{entry_id}: at least two fact objectives are required")
        for objective in objectives:
            _require(isinstance(objective, dict), f"{entry_id}: objective must be an object")
            for field in ("objective_id", "description_key", "target_owner_system",
                          "accepted_fact_codes", "required_count", "match"):
                _require(field in objective, f"{entry_id}: objective missing {field}")
            _require(_text(objective["description_key"]), f"{entry_id}: objective description key is empty")
            _require(_text(objective["target_owner_system"]),
                     f"{entry_id}: objective target owner is empty")
            _require(isinstance(objective["accepted_fact_codes"], list)
                     and objective["accepted_fact_codes"], f"{entry_id}: accepted facts are required")
            _require(objective["required_count"] >= 1, f"{entry_id}: objective count must be positive")
            keys.add(objective["description_key"])
        for field in ("failure_result_codes", "recovery_result_codes"):
            _require(isinstance(entry.get(field), list) and entry[field],
                     f"{entry_id}: {field} are required")
        alternatives = entry.get("alternative_routes")
        _require(isinstance(alternatives, list) and len(alternatives) >= 2,
                 f"{entry_id}: at least two alternative routes are required")
        for route in alternatives:
            _require(all(_text(route.get(field)) for field in
                         ("route_id", "accepted_event_code", "target_owner_system", "cause")),
                     f"{entry_id}: alternative route needs ID, event, owner and cause")
        prefixes = entry.get("event_prefixes")
        _require(isinstance(prefixes, dict)
                 and all(_text(prefixes.get(field)) for field in
                         ("accepted", "progress", "completed", "failed", "recovered")),
                 f"{entry_id}: event prefixes are incomplete")
        request = entry.get("reward_request")
        _require(isinstance(request, dict), f"{entry_id}: reward request is required")
        _require(request.get("owner_system") == "RewardSystem",
                 f"{entry_id}: RewardSystem must own reward decision")
        _require(request.get("contains_final_values") is False,
                 f"{entry_id}: reward request contains final values")
        _require(request.get("source_fact_required") is True,
                 f"{entry_id}: reward request must require a source fact")
        _require(_text(request.get("request_id_policy"))
                 and entry_id in request["request_id_policy"]
                 and "{save_revision}" in request["request_id_policy"],
                 f"{entry_id}: request ID must be stable across save/load")
        _require(_text(request.get("final_reward_written_by"))
                 and request["final_reward_written_by"] == "RewardSystem",
                 f"{entry_id}: final reward writer is not RewardSystem")
        keys.add(entry["display_name_key"])
    return keys


def _audio_validation(assets: Sequence[Mapping[str, Any]], registry_ids: Set[str]) -> Set[str]:
    audio = [entry for entry in assets if entry.get("audio_kind") in {"music_theme", "audio_layer"}]
    music = [entry for entry in audio if entry.get("audio_kind") == "music_theme"]
    layers = [entry for entry in audio if entry.get("audio_kind") == "audio_layer"]
    _require(len(music) == 30, f"expected 30 music themes, found {len(music)}")
    _require(len(layers) == 8, f"expected 8 audio layers, found {len(layers)}")
    _require(sorted(entry.get("theme_index") for entry in music) == list(range(1, 31)),
             "music theme indices must be exactly 1 through 30")
    required_layers = {"port", "voyage", "combat", "storm", "night", "faction", "poker", "relationship"}
    _require({entry.get("layer_scope") for entry in layers} == required_layers,
             "audio layer coverage is incomplete")
    event_ids: Set[str] = set()
    keys: Set[str] = set()
    for entry in audio:
        _base_entry(entry, registry_ids)
        entry_id = entry["id"]
        _require(entry.get("owner_system") == "AssetRegistry", f"{entry_id}: AssetRegistry must own asset metadata")
        _require(entry.get("asset_state") == "placeholder", f"{entry_id}: placeholder state must be explicit")
        _require(_text(entry.get("placeholder_policy")), f"{entry_id}: placeholder policy is required")
        _require(_text(entry.get("audio_event_id")),
                 f"{entry_id}: unique audio_event_id is required")
        _require(entry["audio_event_id"] not in event_ids, f"duplicate audio event ID {entry['audio_event_id']}")
        event_ids.add(entry["audio_event_id"])
        _require(isinstance(entry.get("trigger_conditions"), list) and entry["trigger_conditions"],
                 f"{entry_id}: deterministic trigger conditions are required")
        _require(_text(entry.get("mixer_bus")), f"{entry_id}: mixer_bus is required")
        _require(isinstance(entry.get("mix_tags"), list) and entry["mix_tags"],
                 f"{entry_id}: mix_tags are required")
        _require(_text(entry.get("subtitle_key")) and _text(entry.get("fallback")),
                 f"{entry_id}: subtitle fallback is required")
        keys.add(entry["display_name_key"])
        keys.add(entry["subtitle_key"])
    return keys


def _load_locale(path: Path) -> Dict[str, str]:
    data = _load_json(path)
    _require(isinstance(data, dict) and isinstance(data.get("keys"), dict),
             f"{path}: locale must contain a keys object")
    _require(_text(data.get("locale")), f"{path}: locale is required")
    values = data["keys"]
    _require(all(_text(key) and _text(value) for key, value in values.items()),
             f"{path}: locale keys and values must be non-empty")
    return values


def _validate_localization_maps(expected: Set[str], zh: Mapping[str, str], en: Mapping[str, str]) -> None:
    _require(set(zh) == expected, f"zh-CN key mismatch: missing={sorted(expected-set(zh))[:3]} orphan={sorted(set(zh)-expected)[:3]}")
    _require(set(en) == expected, f"en-US key mismatch: missing={sorted(expected-set(en))[:3]} orphan={sorted(set(en)-expected)[:3]}")


def validate_catalog(registry_root: Path, localization_root: Path | None = None) -> Dict[str, int]:
    registry_ids = _registry_ids(registry_root)
    quests = _entries(registry_root, "quest")
    assets = _entries(registry_root, "asset")
    quest_keys = _quest_validation(quests, registry_ids)
    audio_keys = _audio_validation(assets, registry_ids)
    expected_keys = quest_keys | audio_keys
    catalogs = [entry for entry in assets if entry.get("id") == "asset_d15_a08_catalog"]
    _require(len(catalogs) == 1, "D15-A08 catalog root is missing or duplicated")
    _base_entry(catalogs[0], registry_ids)
    expected_keys.add(catalogs[0]["display_name_key"])
    loc_root = localization_root or (Path("game/Assets/Resources/Localization/D15_A08"))
    zh = _load_locale(loc_root / "zh-CN.json")
    en = _load_locale(loc_root / "en-US.json")
    _validate_localization_maps(expected_keys, zh, en)
    return {"mainline": 5, "side_templates": 120, "music_themes": 30, "audio_layers": 8, "localization_keys": len(expected_keys)}


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Validate D15-A08 quest/audio/localization content")
    parser.add_argument("registry_root", nargs="?", default="game/Assets/Resources/ContentRegistry")
    parser.add_argument("--localization-root", type=Path, default=None)
    args = parser.parse_args(list(argv) if argv is not None else None)
    try:
        counts = validate_catalog(Path(args.registry_root), args.localization_root)
    except (D15A08CatalogError, OSError) as exc:
        print(f"D15-A08 catalog FAILED: {exc}")
        return 1
    print("D15-A08 catalog PASS: " + json.dumps(counts, ensure_ascii=False, sort_keys=True))
    print("missing/orphan localization keys: none")
    print("RewardSystem final reward writes: none in content assets")
    print("runtime Systems touched: none")
    return 0


if __name__ == "__main__":
    sys.exit(main())
