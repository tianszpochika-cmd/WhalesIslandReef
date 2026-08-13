"""D15-S02 cross-batch content QA.

This checker is deliberately directory-level. It does not write registry data,
change content counts, or execute gameplay systems. Runtime save/restore is
accepted only when the entry declares a structured runtime contract or an
explicit definition-only scope; a test-case string alone is not evidence.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Dict, Iterable, List, Tuple


BATCHES = {
    "D15-A01*": {"entry": 50, "content": 50},
    "D15-A02*": {"entry": 1211, "content": 1211},
    "D15-A03*": {"entry": 300, "content": 300},
    "D15-A04*": {"entry": 83, "content": 80},
    "D15-A05*": {"entry": 500, "content": 500},
    "D15-A06*": {"entry": 326, "content": 160},
    "D15-A07*": {"entry": 66, "content": 66},
    "D15-A08*": {"entry": 164, "content": 125},
}

D8_FIELDS = (
    "id", "schema_version", "name", "display_name_key", "category", "tags",
    "description", "gameplay_role", "valid_regions", "valid_habitats",
    "valid_weather", "dependencies", "references", "asset_refs", "source_refs",
    "source_note", "source", "license", "historical_tag", "phase", "status",
    "owner_system", "test_id", "reviewer",
)

D15_FIELDS = (
    "production_batch", "production_stage", "asset_state", "lod_profile",
    "historical_review", "review_evidence_refs", "content_warning_ids", "skip_allowed",
)

OWNER_BY_PREFIX = {
    "island_": "IslandSystem",
    "plant_": "EcologySystem",
    "animal_": "EcologySystem",
    "fish_": "EcologySystem",
    "building_": "BuildingSystem",
    "item_": "ItemCatalog",
    "outfit_": "AppearanceSystem",
    "ship_": "ShipSystem",
    "crew_": "CrewSystem",
    "pet_": "AnimalCareSystem",
    "quest_": "QuestSystem",
    "reward_": "RewardSystem",
    "asset_": "AssetRegistry",
}

VISUAL_CATEGORIES = {
    "island", "plant", "animal", "fish", "building", "item", "outfit", "ship", "crew", "pet"
}

PLACEHOLDER_MARKERS = ("placeholder", "primitive://", "unity-primitive://", "none:", "asset_ph_")
ID_PREFIXES = tuple(OWNER_BY_PREFIX)
NESTED_REFERENCE_FIELDS = {
    "references", "dependencies", "asset_refs", "model_ref", "material_ref",
    "collision_ref", "required_functional_point", "building_id", "variant_id",
    "ship_id", "island_id", "crew_id", "pet_id", "animal_id", "fish_id",
    "item_id", "outfit_id", "quest_id", "reward_id", "resource_ids",
}
MODERN_MARKERS = re.compile(
    r"\b(plastic|nylon|diesel|motorized|motor|radio|telephone|smartphone|computer|digital|"
    r"concrete|synthetic|torpedo|rocket|machine\s+gun|revolver)\b",
    re.IGNORECASE,
)

# These are the nine D15-S02 samples called out by the production task. They
# require an explicit runtime persistence contract or an explicit declaration
# that the registry entry is immutable definition data rather than mutable
# runtime state.
RUNTIME_SAVE_SAMPLE_IDS = {
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


def _is_text(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def _prefix(content_id: str) -> str | None:
    for prefix in ID_PREFIXES:
        if content_id.startswith(prefix):
            return prefix
    return None


def _sample(items: List[Tuple[Path, Dict[str, Any]]], limit: int = 3) -> List[Tuple[Path, Dict[str, Any]]]:
    if len(items) <= limit:
        return items
    positions = [0, len(items) // 2, len(items) - 1]
    return [items[pos] for pos in positions]


def _value_has_path(value: Any) -> bool:
    return isinstance(value, str) and value.startswith("game/")


def _add_issue(report: Dict[str, Any], severity: str, code: str, message: str, ids: Iterable[str] = ()) -> None:
    target = report["errors"] if severity == "error" else report["warnings"]
    id_list = list(ids)
    target.append({"code": code, "message": message, "ids": id_list[:12], "count": len(id_list)})


def _load_entries(registry_root: Path, report: Dict[str, Any]) -> Tuple[List[Tuple[Path, Dict[str, Any]]], Dict[str, Dict[str, Any]]]:
    entries: List[Tuple[Path, Dict[str, Any]]] = []
    by_id: Dict[str, Dict[str, Any]] = {}
    paths_by_id: Dict[str, Path] = {}
    for path in sorted((registry_root / "entries").rglob("*.json")):
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            _add_issue(report, "error", "invalid-json", f"无法加载 JSON：{path}: {exc}", [path.stem])
            continue
        if not isinstance(data, dict):
            _add_issue(report, "error", "invalid-entry", f"JSON 根不是对象：{path}", [path.stem])
            continue
        content_id = data.get("id", path.stem)
        if not _is_text(content_id):
            _add_issue(report, "error", "missing-id", f"条目缺少稳定 ID：{path}", [path.stem])
            continue
        if content_id in by_id:
            _add_issue(report, "error", "duplicate-id", f"ID 重复：{content_id}", [str(paths_by_id[content_id]), str(path)])
        by_id[content_id] = data
        paths_by_id[content_id] = path
        entries.append((path, data))
    return entries, by_id


def _load_localization(project_root: Path) -> Tuple[set[str], set[str]]:
    localization_root = project_root / "game" / "Assets" / "Resources" / "Localization"
    package_dirs = (localization_root / "D15_A08", localization_root / "D15_A01_A07")
    result = [set(), set()]
    for locale in ("zh-CN.json", "en-US.json"):
        locale_index = 0 if locale == "zh-CN.json" else 1
        for base in package_dirs:
            try:
                value = json.loads((base / locale).read_text(encoding="utf-8"))
                if isinstance(value, dict) and isinstance(value.get("keys"), dict):
                    result[locale_index].update(value["keys"])
                elif isinstance(value, dict):
                    result[locale_index].update(value)
            except (OSError, json.JSONDecodeError):
                continue
    return result[0], result[1]


def _check_nested_references(data: Any, known_ids: set[str], owner_id: str, missing: List[Tuple[str, str]], field_name: str = "") -> None:
    if isinstance(data, dict):
        for key, value in data.items():
            _check_nested_references(value, known_ids, owner_id, missing, str(key))
    elif isinstance(data, list):
        for value in data:
            _check_nested_references(value, known_ids, owner_id, missing, field_name)
    elif isinstance(data, str):
        if field_name not in NESTED_REFERENCE_FIELDS:
            return
        if _prefix(data) and data not in known_ids:
            missing.append((owner_id, data))


def _takeover_probe(data: Dict[str, Any]) -> bool:
    category = data.get("category")
    if category in {"island", "plant", "animal", "fish", "building", "crew", "pet"}:
        return bool(data.get("behavior_profile"))
    if category == "item" or category == "outfit":
        integration = data.get("integration")
        return bool(data.get("function_entry") or data.get("interaction_entry") or data.get("behavior_profile") or (isinstance(integration, dict) and integration.get("canonical_write_owner")))
    if category == "ship":
        return bool(data.get("behavior_profile") and (data.get("functional_point_slots") or data.get("interaction_entry")))
    if category == "quest":
        return bool(data.get("target_systems") and data.get("event_prefixes") and isinstance(data.get("reward_request"), dict))
    if category == "asset":
        return bool(data.get("resource_path") or data.get("audio_file_ref") or data.get("model_ref") or data.get("source_path"))
    return bool(data.get("behavior_profile") or data.get("gameplay_role"))


def _save_probe(data: Dict[str, Any]) -> bool:
    text = json.dumps(data, ensure_ascii=False, sort_keys=True)
    restored = json.loads(text)
    if restored.get("id") != data.get("id"):
        return False
    return bool(
        data.get("save_contract")
        or data.get("save_state_key")
        or data.get("save_key")
        or any("save" in str(value).lower() or "restore" in str(value).lower() for value in data.get("test_cases", []))
    )


def _runtime_state_probe(data: Dict[str, Any]) -> Tuple[bool, str]:
    """Validate the explicit D15-S02 runtime-state declaration.

    ``test_cases`` are intentionally not accepted here. A definition-only
    entry must explain why it is not mutable runtime state. A runtime-persisted
    entry must point to real SaveSystem entry points and a state path.
    """
    contract = data.get("runtime_state")
    if not isinstance(contract, dict):
        return False, "missing_runtime_state_contract"

    mode = contract.get("mode")
    verification = contract.get("verification")
    if not isinstance(verification, str) or not verification.strip():
        return False, "missing_verification_id"

    if mode == "definition_only":
        required = ("scope", "state_owner", "save_entry", "restore_entry", "reason")
        if any(not isinstance(contract.get(field), str) or not contract[field].strip() for field in required):
            return False, "incomplete_definition_only_contract"
        if contract.get("scope") != "immutable_definition":
            return False, "definition_only_scope_must_be_immutable_definition"
        if contract.get("save_entry") != "not_applicable" or contract.get("restore_entry") != "not_applicable":
            return False, "definition_only_must_not_claim_runtime_save_entry"
        return True, "definition_only"

    if mode == "runtime_persisted":
        required = ("scope", "state_owner", "save_entry", "restore_entry", "state_path", "reason")
        if any(not isinstance(contract.get(field), str) or not contract[field].strip() for field in required):
            return False, "incomplete_runtime_persistence_contract"
        if contract.get("scope") != "mutable_runtime_state":
            return False, "runtime_persisted_scope_must_be_mutable_runtime_state"
        if not contract["save_entry"].startswith("SaveSystem.") or not contract["restore_entry"].startswith("SaveSystem."):
            return False, "runtime_persistence_must_use_SaveSystem_entry_points"
        return True, "runtime_persisted"

    return False, "unsupported_runtime_state_mode"


def _check_entry_shape(path: Path, data: Dict[str, Any], report: Dict[str, Any], registry_root: Path) -> None:
    content_id = str(data.get("id", path.stem))
    batch = data.get("production_batch")
    for field in D8_FIELDS:
        if field not in data:
            _add_issue(report, "error", "missing-d8-field", f"{content_id} 缺 D8 字段 {field}", [content_id])
    for field in D15_FIELDS:
        if field not in data:
            _add_issue(report, "error", "missing-d15-field", f"{content_id} 缺 D15 字段 {field}", [content_id])
    prefix = _prefix(content_id)
    if prefix and data.get("owner_system") != OWNER_BY_PREFIX[prefix]:
        _add_issue(report, "error", "owner-mismatch", f"{content_id} owner={data.get('owner_system')}，应为 {OWNER_BY_PREFIX[prefix]}", [content_id])
    if not _is_text(data.get("display_name_key")):
        _add_issue(report, "error", "missing-localization-key", f"{content_id} 缺 display_name_key", [content_id])
    if data.get("historical_review") not in {"Approved", "ApprovedWithWarning"}:
        _add_issue(report, "error", "historical-review-not-approved", f"{content_id} 审查状态为 {data.get('historical_review')}", [content_id])
    if data.get("historical_review") == "ApprovedWithWarning" and not data.get("content_warning_ids"):
        _add_issue(report, "error", "warning-without-warning-id", f"{content_id} 有警告审查但缺 content_warning_ids", [content_id])
    if not _is_text(data.get("test_id")):
        _add_issue(report, "error", "missing-test-id", f"{content_id} 缺 test_id", [content_id])
    category = data.get("category")
    if category not in {"quest", "asset"} and category in VISUAL_CATEGORIES:
        for field in ("model_ref", "material_ref", "collision_ref", "lod_profile"):
            if not data.get(field):
                _add_issue(report, "error", "missing-visual-field", f"{content_id} 缺 {field}", [content_id])
        if not data.get("test_cases"):
            _add_issue(report, "error", "missing-test-cases", f"{content_id} 缺加载/接管/保存测试用例", [content_id])
    if category in {"quest", "asset"} and batch == "D15-A08*" and not data.get("test_cases"):
        _add_issue(report, "error", "missing-test-cases", f"{content_id} 缺 D15 test_cases（现只有 test_id）", [content_id])
    if category == "asset" and batch == "D15-A08*" and not data.get("lod_profile"):
        _add_issue(report, "error", "missing-audio-lod-na", f"{content_id} 是音频资产但缺明确 LOD-N/A", [content_id])
    if category in VISUAL_CATEGORIES and isinstance(data.get("habitat_profile"), dict):
        primary = data["habitat_profile"].get("primary")
        valid = set(data.get("valid_habitats") or [])
        if primary and primary not in valid:
            _add_issue(report, "error", "habitat-mismatch", f"{content_id} habitat_profile.primary={primary} 不在 valid_habitats={sorted(valid)}", [content_id])
    for field in ("model_ref", "material_ref", "collision_ref"):
        value = data.get(field)
        if isinstance(value, str) and value.startswith("game/"):
            path_value = value.split(":", 1)[0].split("#", 1)[0]
            target = registry_root.parents[3] / path_value
            if not target.exists():
                _add_issue(report, "error", "missing-asset-path", f"{content_id}.{field} 路径不存在：{value}", [content_id])
    if category == "asset":
        resource_path = data.get("resource_path") or data.get("source_path")
        if _value_has_path(resource_path):
            path_value = resource_path.split("#", 1)[0]
            if not (registry_root.parents[3] / path_value).exists():
                _add_issue(report, "error", "missing-resource-path", f"{content_id} 资源路径不存在：{resource_path}", [content_id])
    text = json.dumps(data, ensure_ascii=False)
    match = MODERN_MARKERS.search(text)
    if match and data.get("historical_tag") == "historical":
        _add_issue(report, "warning", "historical-review-needed", f"{content_id} 含可能超出 1675 语境的词：{match.group(0)}", [content_id])


def _load_a07_manifest(project_root: Path, report: Dict[str, Any]) -> Dict[str, Any]:
    path = project_root / "game" / "Assets" / "Resources" / "ContentRegistry" / "batches" / "d15_a07" / "d15_a07_character_pet_batch.json"
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        _add_issue(report, "error", "a07-manifest-invalid", f"A07 manifest 无法加载：{exc}")
        return {}


def run_qa(registry_root: Path) -> Dict[str, Any]:
    registry_root = registry_root.resolve()
    project_root = registry_root.parents[3]
    report: Dict[str, Any] = {
        "tool": "D15-S02",
        "registry_root": str(registry_root),
        "errors": [],
        "warnings": [],
        "counts": {},
        "samples": [],
        "model_reuse": {},
        "localization": {},
        "manifest_probes": {},
    }
    entries, by_id = _load_entries(registry_root, report)
    d15: Dict[str, List[Tuple[Path, Dict[str, Any]]]] = defaultdict(list)
    for path, data in entries:
        batch = data.get("production_batch")
        if isinstance(batch, str) and batch in BATCHES:
            d15[batch].append((path, data))
            _check_entry_shape(path, data, report, registry_root)
    roots = set()
    try:
        manifest = json.loads((registry_root / "registry.json").read_text(encoding="utf-8"))
        roots = set(manifest.get("roots", []))
    except (OSError, json.JSONDecodeError) as exc:
        _add_issue(report, "error", "registry-invalid", f"registry.json 无法加载：{exc}")
    for batch, expected in BATCHES.items():
        items = sorted(d15.get(batch, []), key=lambda pair: pair[1].get("id", ""))
        report["counts"][batch] = {
            "registry_entries": len(items),
            "expected_registry_entries": expected["entry"],
            "content_entries": sum(data.get("category") != "asset" for _, data in items),
            "expected_content_entries": expected["content"],
            "by_category": dict(Counter(data.get("category") for _, data in items)),
            "placeholder_entries": sum(data.get("asset_state") == "placeholder" for _, data in items),
            "non_verified_entries": sum(data.get("production_stage") != "verified" or data.get("status") != "verified" for _, data in items),
        }
        if len(items) != expected["entry"]:
            _add_issue(report, "error", "batch-count-mismatch", f"{batch} 注册条目数量 {len(items)} != {expected['entry']}", [batch])
        for _, data in _sample(items):
            content_id = data["id"]
            loaded = json.loads(json.dumps(data, ensure_ascii=False, sort_keys=True))
            load_ok = loaded.get("id") == content_id
            save_ok = _save_probe(data)
            interaction_ok = _takeover_probe(data)
            owner_ok = _is_text(data.get("owner_system"))
            root_ok = content_id in roots
            sample = {
                "batch": batch,
                "id": content_id,
                "category": data.get("category"),
                "load": load_ok,
                "save_roundtrip": save_ok,
                "interaction_or_event_entry": interaction_ok,
                "owner_takeover_metadata": owner_ok,
                "registry_root": root_ok,
                "test_id": data.get("test_id"),
            }
            report["samples"].append(sample)
            if not load_ok or not root_ok:
                _add_issue(report, "error", "sample-load-or-scan-failed", f"抽样加载/目录扫描失败：{content_id}", [content_id])
            if not interaction_ok:
                _add_issue(report, "error", "sample-owner-entry-missing", f"抽样缺行为/交互/事件接管入口：{content_id}", [content_id])
            if not save_ok:
                _add_issue(report, "warning", "sample-save-marker-missing", f"抽样只有目录 JSON round-trip，缺明确 save/restore 字段：{content_id}", [content_id])

    runtime_contracts = {}
    for content_id in sorted(RUNTIME_SAVE_SAMPLE_IDS):
        data = by_id.get(content_id)
        if data is None:
            runtime_contracts[content_id] = {"valid": False, "reason": "missing_registry_entry"}
            _add_issue(report, "error", "runtime-save-restore-entry-missing", f"保存恢复抽样条目不存在：{content_id}", [content_id])
            continue
        valid, reason = _runtime_state_probe(data)
        runtime_contracts[content_id] = {
            "valid": valid,
            "mode": (data.get("runtime_state") or {}).get("mode") if isinstance(data.get("runtime_state"), dict) else None,
            "reason": reason,
        }
        if not valid:
            _add_issue(
                report,
                "error",
                "runtime-save-restore-contract-missing",
                f"{content_id} 只有 JSON/test_cases 证据，缺有效运行时 save/restore 或 definition-only 说明：{reason}",
                [content_id],
            )
    report["runtime_save_restore"] = {
        "required_sample_ids": sorted(RUNTIME_SAVE_SAMPLE_IDS),
        "contracts": runtime_contracts,
        "verified_count": sum(1 for value in runtime_contracts.values() if value.get("valid")),
        "missing_count": sum(1 for value in runtime_contracts.values() if not value.get("valid")),
    }
    missing_roots = [data["id"] for items in d15.values() for _, data in items if data["id"] not in roots]
    if missing_roots:
        _add_issue(report, "error", "orphan-root", f"D15 条目未进入 registry roots，共 {len(missing_roots)} 条", missing_roots)
    a07 = _load_a07_manifest(project_root, report)
    local_reference_ids = set(by_id)
    local_reference_ids.update(
        combo.get("id")
        for combo in a07.get("appearance_combinations", [])
        if isinstance(combo, dict) and _is_text(combo.get("id"))
    )
    local_reference_ids.update(
        profile.get("id")
        for profile in a07.get("appearance_profiles", [])
        if isinstance(profile, dict) and _is_text(profile.get("id"))
    )
    nested_missing: List[Tuple[str, str]] = []
    for items in d15.values():
        for _, data in items:
            _check_nested_references(data, local_reference_ids, str(data.get("id")), nested_missing)
    unique_nested_missing = sorted(set(nested_missing))
    if unique_nested_missing:
        _add_issue(report, "error", "orphan-nested-reference", f"D15 嵌套 ID 引用不存在，共 {len(unique_nested_missing)} 条", [f"{owner}->{target}" for owner, target in unique_nested_missing])
    report["reference_gaps"] = {
        "missing_roots": missing_roots,
        "missing_nested_ids": [list(pair) for pair in unique_nested_missing],
    }

    model_users: Dict[str, List[str]] = defaultdict(list)
    for items in d15.values():
        for _, data in items:
            value = data.get("model_ref")
            if _is_text(value):
                model_users[value].append(data["id"])
    real_duplicates = {key: ids for key, ids in model_users.items() if len(ids) > 1 and not any(marker in key.lower() for marker in PLACEHOLDER_MARKERS)}
    placeholder_reuse = {key: len(ids) for key, ids in model_users.items() if len(ids) > 1 and key not in real_duplicates}
    report["model_reuse"] = {"real_duplicate_models": real_duplicates, "intentional_placeholder_reuse": placeholder_reuse}
    if real_duplicates:
        _add_issue(report, "error", "duplicate-model", f"非占位模型被多个 D15 条目复用，共 {len(real_duplicates)} 个模型", list(real_duplicates))

    zh, en = _load_localization(project_root)
    a08_keys = [data.get("display_name_key") for _, data in d15.get("D15-A08*", [])]
    a08_missing_zh = sorted(key for key in a08_keys if key not in zh)
    a08_missing_en = sorted(key for key in a08_keys if key not in en)
    all_other_keys = sorted({
        data.get("display_name_key")
        for batch, items in d15.items()
        if batch != "D15-A08*"
        for _, data in items
        if _is_text(data.get("display_name_key"))
    })
    non_a08_missing_zh = sorted(key for key in all_other_keys if key not in zh)
    non_a08_missing_en = sorted(key for key in all_other_keys if key not in en)
    non_a08_unproven = sorted(set(non_a08_missing_zh) | set(non_a08_missing_en))
    report["localization"] = {
        "zh_cn_keys": len(zh),
        "en_us_keys": len(en),
        "a08_entries": len(a08_keys),
        "a08_missing_zh_cn": a08_missing_zh,
        "a08_missing_en_us": a08_missing_en,
        "non_a08_keys_without_package_proof": len(non_a08_unproven),
        "non_a08_missing_zh_cn": non_a08_missing_zh,
        "non_a08_missing_en_us": non_a08_missing_en,
        "non_a08_examples": all_other_keys[:12],
    }
    if a08_missing_zh or a08_missing_en:
        _add_issue(report, "error", "missing-localization", f"A08 本地化缺失：zh-CN={len(a08_missing_zh)}、en-US={len(a08_missing_en)}", a08_missing_zh + a08_missing_en)
    if non_a08_unproven:
        _add_issue(report, "warning", "localization-unverified", f"A01-A07 有 {len(non_a08_unproven)} 个 display_name_key 缺少可验证的中英条目", non_a08_unproven)

    npc_instances = a07.get("npc_instances", []) if isinstance(a07, dict) else []
    named_variants = a07.get("pet_variants", []) if isinstance(a07, dict) else []
    manifest_missing = []
    for record in named_variants:
        if isinstance(record, dict) and record.get("id") and record["id"] not in by_id:
            if not any(record["id"] == data.get("id") for data in by_id.values()):
                manifest_missing.append(record["id"])
    npc_sample = _sample([(Path("manifest"), item) for item in npc_instances], 4)
    npc_checks = []
    for _, item in npc_sample:
        npc_checks.append({
            "id": item.get("instance_id"),
            "save_key": bool(item.get("save_key")),
            "spawn_anchor": bool(item.get("spawn_anchor_id")),
            "owner_entry": bool(item.get("behavior_profile")),
        })
    report["manifest_probes"]["A07"] = {
        "npc_instances": len(npc_instances),
        "named_variants": len(named_variants),
        "named_variant_missing_ids": manifest_missing,
        "npc_sample": npc_checks,
    }
    combo_ids = {
        combo.get("id")
        for combo in a07.get("appearance_combinations", [])
        if isinstance(combo, dict) and _is_text(combo.get("id"))
    }
    combo_ids.update(
        profile.get("id")
        for profile in a07.get("appearance_profiles", [])
        if isinstance(profile, dict) and _is_text(profile.get("id"))
    )
    appearance_missing: List[Tuple[str, str]] = []
    for _, data in d15.get("D15-A07*", []):
        combo_id = data.get("appearance_combo_id")
        if _is_text(combo_id) and combo_id not in combo_ids:
            appearance_missing.append((data.get("id"), combo_id))
    for variant in a07.get("pet_variants", []):
        if not isinstance(variant, dict):
            continue
        combo_id = variant.get("appearance_combo_id")
        if _is_text(combo_id) and combo_id not in combo_ids:
            appearance_missing.append((variant.get("id"), combo_id))
    manifest_asset_missing: List[Tuple[str, str]] = []
    for combo in a07.get("appearance_combinations", []):
        if not isinstance(combo, dict):
            continue
        combo_owner = combo.get("id", "<appearance-combo>")
        refs = list(combo.get("asset_refs", []))
        refs.extend(combo.get("clothing_ids", []))
        refs.extend(combo.get("accessory_ids", []))
        if combo.get("career_equipment_id"):
            refs.append(combo["career_equipment_id"])
        for ref in refs:
            if _is_text(ref) and ref not in by_id:
                manifest_asset_missing.append((combo_owner, ref))
    report["manifest_probes"]["A07"]["appearance_missing_ids"] = appearance_missing
    report["manifest_probes"]["A07"]["appearance_nested_missing_ids"] = manifest_asset_missing
    if manifest_missing:
        _add_issue(report, "error", "a07-manifest-orphan", f"A07 named variant 引用不存在，共 {len(manifest_missing)} 条", manifest_missing)
    if appearance_missing:
        _add_issue(report, "error", "a07-appearance-orphan", f"A07 appearance_combo_id 未找到定义，共 {len(appearance_missing)} 条", [f"{owner}->{target}" for owner, target in appearance_missing])
    if manifest_asset_missing:
        _add_issue(report, "error", "a07-appearance-asset-orphan", f"A07 外观组合引用不存在，共 {len(manifest_asset_missing)} 条", [f"{owner}->{target}" for owner, target in manifest_asset_missing])
    if any(not all(item.values()) for item in npc_checks):
        _add_issue(report, "error", "a07-npc-save-entry-missing", "A07 NPC 抽样缺 save_key、spawn_anchor 或 owner 入口")

    report["status"] = "FAIL" if report["errors"] else "PASS_WITH_WARNINGS" if report["warnings"] else "PASS"
    report["summary"] = {
        "error_count": len(report["errors"]),
        "warning_count": len(report["warnings"]),
        "sample_count": len(report["samples"]),
        "batch_count": len(BATCHES),
    }
    return report


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("registry_root", type=Path)
    parser.add_argument("--json-out", type=Path)
    args = parser.parse_args()
    report = run_qa(args.registry_root)
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"D15-S02 QA {report['status']}: batches={report['summary']['batch_count']} samples={report['summary']['sample_count']} errors={report['summary']['error_count']} warnings={report['summary']['warning_count']}")
    print("batch_counts=" + json.dumps(report["counts"], ensure_ascii=False, sort_keys=True))
    print("localization=" + json.dumps(report["localization"], ensure_ascii=False, sort_keys=True))
    print("model_reuse=" + json.dumps(report["model_reuse"], ensure_ascii=False, sort_keys=True))
    for issue in report["errors"][:20]:
        print(f"ERROR {issue['code']}: {issue['message']}")
    for issue in report["warnings"][:20]:
        print(f"WARNING {issue['code']}: {issue['message']}")
    return 1 if report["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
