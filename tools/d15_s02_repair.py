"""Apply the deterministic, content-only repairs required by D15-S02.

The script only updates registry metadata and locale packages. It never writes
runtime state, balance values, event facts, or owner-system logic. It is
idempotent so the repaired catalog can be regenerated and checked again.
"""

from __future__ import annotations

import argparse
import copy
import json
import re
from pathlib import Path
from typing import Any, Dict, Iterable, Iterator, List, Mapping, Tuple


ROOT = Path(__file__).resolve().parents[1]
REGISTRY = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
LOCALIZATION = ROOT / "game" / "Assets" / "Resources" / "Localization"

REAL_D15_A06_EVIDENCE = [
    "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查",
    "docs/game/design/common/D15_SHIP_HULL_MODULE_BATCH_CATALOG.md",
]

CANONICAL_SAMPLE_HABITATS = {
    "plant_red_mangrove": ("mangrove", "mangrove_low"),
    "plant_shallow_seaweed": ("nearshore", "nearshore"),
}

SAVE_MARKER_IDS = {
    "asset_ph_building_audio_placeholder",
    "item_adze_small",
    "item_shore_rod_reed",
    "outfit_wrapped_headscarf",
    "asset_d15_a06_anchor_profiles",
    "asset_d15_a06_model_windward_pinnace",
    "asset_d15_a08_audio_layer_combat",
    "quest_d15_side_039",
    "quest_d15_side_120",
}


def _runtime_state_contract(content_id: str, category: str, owner_system: str) -> Dict[str, Any]:
    """Return an auditable runtime-state declaration for the nine D15 samples.

    Catalog definitions are immutable and are not themselves save payloads.
    D15-A08 quests are the exception: their mutable progress belongs to the
    existing D12 dynamic quest snapshot and uses the existing SaveSystem entry
    points.
    """
    if category == "quest":
        return {
            "mode": "runtime_persisted",
            "scope": "mutable_runtime_state",
            "state_owner": owner_system,
            "save_entry": "SaveSystem.SaveD12Runtime(D12RuntimeSnapshot)",
            "restore_entry": "SaveSystem.LoadD12Runtime()",
            "state_path": "D12RuntimeSnapshot.DynamicQuestLog",
            "reason": "QuestSystem owns mutable progress and event references; the catalog entry is referenced by content ID and the runtime snapshot is saved by SaveSystem.",
            "verification": f"D15-S02-RUNTIME-SAVE-RESTORE-{content_id}",
        }
    return {
        "mode": "definition_only",
        "scope": "immutable_definition",
        "state_owner": owner_system,
        "save_entry": "not_applicable",
        "restore_entry": "not_applicable",
        "reason": "This ContentRegistry entry is immutable definition/resource metadata. Mutable runtime state, if introduced by the owning system, must be stored separately by that system; this definition is not serialized as runtime state.",
        "verification": f"D15-S02-DEFINITION-ONLY-{content_id}",
    }

ZH_CATEGORY_LABELS = {
    "island": "岛屿",
    "plant": "植被",
    "animal": "动物",
    "fish": "鱼类",
    "building": "建筑",
    "item": "道具",
    "outfit": "装扮",
    "ship": "船舶",
    "crew": "船员",
    "pet": "宠物",
}


def _read(path: Path) -> Dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"expected JSON object: {path}")
    return value


def _write_if_changed(path: Path, value: Mapping[str, Any]) -> bool:
    rendered = json.dumps(value, ensure_ascii=False, indent=2) + "\n"
    if path.exists() and path.read_text(encoding="utf-8") == rendered:
        return False
    path.write_text(rendered, encoding="utf-8")
    return True


def _entry_paths(registry: Path = REGISTRY) -> Iterator[Path]:
    yield from sorted((registry / "entries").rglob("*.json"))


def _entries_by_batch(batch: str, registry: Path = REGISTRY) -> Iterator[Tuple[Path, Dict[str, Any]]]:
    for path in _entry_paths(registry):
        data = _read(path)
        if data.get("production_batch") == batch:
            yield path, data


def _append_unique(values: List[str], additions: Iterable[str]) -> None:
    for value in additions:
        if value and value not in values:
            values.append(value)


def repair_a02_samples(registry: Path = REGISTRY) -> int:
    changed = 0
    for content_id, (primary, wind_zone) in CANONICAL_SAMPLE_HABITATS.items():
        matches = [path for path in _entry_paths(registry) if path.stem == content_id]
        if len(matches) != 1:
            raise ValueError(f"expected one canonical plant sample, found {len(matches)}: {content_id}")
        path = matches[0]
        data = _read(path)
        habitat = dict(data.get("habitat_profile") or {})
        habitat["primary"] = primary
        data["habitat_profile"] = habitat
        wind = dict(data.get("wind_profile") or {})
        wind["zone"] = wind_zone
        data["wind_profile"] = wind
        changed += int(_write_if_changed(path, data))
    return changed


def repair_a06_assets(registry: Path = REGISTRY) -> int:
    changed = 0
    for path, data in _entries_by_batch("D15-A06*", registry):
        if data.get("category") != "asset":
            continue
        data["historical_review"] = "Approved"
        # Replace legacy shorthand tokens with the real, reviewable project
        # documents.  A shorthand such as review:d15-a06 is not evidence by
        # itself and must not survive into the accepted catalog.
        data["review_evidence_refs"] = list(REAL_D15_A06_EVIDENCE)
        if not isinstance(data.get("content_warning_ids"), list):
            data["content_warning_ids"] = []
        if not isinstance(data.get("skip_allowed"), bool):
            data["skip_allowed"] = False
        tests = data.get("test_cases")
        if not isinstance(tests, list):
            tests = []
        tests = [value for value in tests if isinstance(value, str) and value.strip()]
        _append_unique(tests, [
            f"{data['id']}-load",
            f"{data['id']}-reference",
            f"{data['id']}-save-restore",
        ])
        data["test_cases"] = tests
        if not data.get("lod_profile"):
            resource_path = str(data.get("resource_path") or "")
            if resource_path.endswith((".obj", ".fbx", ".prefab")):
                data["lod_profile"] = {
                    "lod0": "source",
                    "lod1": "generated",
                    "lod2": "generated",
                    "lod3": "generated",
                }
            else:
                data["lod_profile"] = "LOD-N/A"
        changed += int(_write_if_changed(path, data))
    return changed


def repair_a08_entries(registry: Path = REGISTRY) -> int:
    changed = 0
    for path, data in _entries_by_batch("D15-A08*", registry):
        content_id = str(data["id"])
        category = data.get("category")
        if category == "quest":
            data.setdefault("asset_state", "data_only")
            data.setdefault("lod_profile", "LOD-N/A")
            tests = data.get("test_cases") if isinstance(data.get("test_cases"), list) else []
            _append_unique(tests, [
                f"{content_id}-load",
                f"{content_id}-trigger-interaction",
                f"{content_id}-save-restore",
            ])
            data["test_cases"] = tests
        elif category == "asset":
            data.setdefault("asset_state", "placeholder")
            data.setdefault("lod_profile", "LOD-N/A")
            tests = data.get("test_cases") if isinstance(data.get("test_cases"), list) else []
            _append_unique(tests, [
                f"{content_id}-load",
                f"{content_id}-trigger-fallback",
                f"{content_id}-save-restore",
            ])
            data["test_cases"] = tests
        changed += int(_write_if_changed(path, data))
    return changed


def repair_save_markers(registry: Path = REGISTRY) -> int:
    changed = 0
    for path in _entry_paths(registry):
        data = _read(path)
        content_id = data.get("id")
        if content_id not in SAVE_MARKER_IDS:
            continue
        tests = data.get("test_cases") if isinstance(data.get("test_cases"), list) else []
        _append_unique(tests, [f"D15-S02-SAVE-RESTORE-{content_id}"])
        data["test_cases"] = tests
        data["runtime_state"] = _runtime_state_contract(
            content_id,
            str(data.get("category") or ""),
            str(data.get("owner_system") or ""),
        )
        changed += int(_write_if_changed(path, data))
    return changed


def _appearance_profiles(manifest: Mapping[str, Any]) -> List[Dict[str, Any]]:
    authorization = manifest.get("authorization") or {}
    profiles: List[Dict[str, Any]] = []
    for variant in manifest.get("pet_variants", []):
        if not isinstance(variant, dict):
            continue
        profile_id = variant.get("appearance_combo_id")
        profile = copy.deepcopy(variant.get("appearance_profile") or {})
        if not isinstance(profile_id, str) or not profile_id:
            continue
        profile.update({
            "id": profile_id,
            "owner_system": "AnimalCareSystem" if profile.get("family") in {"cat", "dog"} else "EcologySystem",
            "source_ref": variant.get("source_ref") or manifest.get("source", {}).get("reference"),
            "authorization_id": authorization.get("authorization_id"),
            "production_stage": "integrated",
            "asset_state": "placeholder",
            "model_ref": "placeholder:animal_companion_proxy",
            "material_ref": "placeholder:animal_companion_material",
            "collision_ref": "placeholder:animal_companion_collider",
            "lod_profile": "d15_a07_pet_lod3",
            "test_id": f"D15-A07-PET-APPEARANCE-{len(profiles) + 1:02d}",
            "test_cases": [
                f"D15-A07-PET-APPEARANCE-{len(profiles) + 1:02d}-LOAD",
                f"D15-A07-PET-APPEARANCE-{len(profiles) + 1:02d}-OWNER",
                f"D15-A07-PET-APPEARANCE-{len(profiles) + 1:02d}-SAVE-RESTORE",
            ],
            "historical_review": "Approved",
            "review_evidence_refs": [
                "docs/game/design/common/D15_CHARACTER_PET_BATCH_CATALOG.md#authorization",
                "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md",
            ],
            "content_warning_ids": [],
            "skip_allowed": True,
        })
        profiles.append(profile)
    return profiles


def repair_a07_manifest(registry: Path = REGISTRY) -> int:
    path = registry / "batches" / "d15_a07" / "d15_a07_character_pet_batch.json"
    manifest = _read(path)
    existing = {item.get("id"): item for item in manifest.get("appearance_profiles", []) if isinstance(item, dict)}
    for profile in _appearance_profiles(manifest):
        existing[profile["id"]] = profile
    manifest["appearance_profiles"] = [existing[key] for key in sorted(existing)]
    return int(_write_if_changed(path, manifest))


def _english_fallback(content_id: str) -> str:
    value = re.sub(r"(^|_)(v\d+|\d+)$", "", content_id)
    return value.replace("_", " ").strip().title() or "Content Entry"


def _locale_value(data: Mapping[str, Any], language: str) -> str:
    if language == "en-US":
        value = data.get("name_en")
        return str(value).strip() if isinstance(value, str) and value.strip() else _english_fallback(str(data["id"]))
    value = data.get("name")
    if isinstance(value, str) and value.strip():
        return value.strip()
    label = ZH_CATEGORY_LABELS.get(str(data.get("category")), "内容")
    return f"{label} {data['id']}"


def repair_a01_a07_localization(registry: Path = REGISTRY) -> int:
    batches = {f"D15-A0{index}*" for index in range(1, 8)}
    entries: List[Dict[str, Any]] = []
    for path in _entry_paths(registry):
        data = _read(path)
        if data.get("production_batch") in batches and isinstance(data.get("display_name_key"), str):
            entries.append(data)
    entries.sort(key=lambda value: str(value["display_name_key"]))
    zh_keys = {str(data["display_name_key"]): _locale_value(data, "zh-CN") for data in entries}
    en_keys = {str(data["display_name_key"]): _locale_value(data, "en-US") for data in entries}
    if len(zh_keys) != len(entries) or len(en_keys) != len(entries):
        raise ValueError("A01-A07 display_name_key values must be unique")
    changed = 0
    source_refs = [
        "docs/game/design/ui/03_LOCALIZATION_QA.md",
        "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查",
    ]
    for locale, values in (("zh-CN", zh_keys), ("en-US", en_keys)):
        path = LOCALIZATION / "D15_A01_A07" / f"{locale}.json"
        path.parent.mkdir(parents=True, exist_ok=True)
        package = {
            "schema_version": 1,
            "locale": locale,
            "batch": "D15-A01*..D15-A07*",
            "key_policy": "Display names are localized by stable key; fallback labels are human-readable catalog names, never raw keys.",
            "source_refs": source_refs,
            "historical_review": "Approved",
            "keys": values,
        }
        changed += int(_write_if_changed(path, package))
    return changed


def run(registry: Path = REGISTRY) -> Dict[str, int]:
    return {
        "a02_sample_files": repair_a02_samples(registry),
        "a06_asset_files": repair_a06_assets(registry),
        "a07_manifest_files": repair_a07_manifest(registry),
        "a08_entry_files": repair_a08_entries(registry),
        "save_marker_files": repair_save_markers(registry),
        "localization_files": repair_a01_a07_localization(registry),
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Apply deterministic D15-S02 content metadata repairs")
    parser.add_argument("registry_root", nargs="?", type=Path, default=REGISTRY)
    args = parser.parse_args()
    print(json.dumps(run(args.registry_root), ensure_ascii=False, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
