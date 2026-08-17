"""Read-only catalog claim audit for D18 logic records (D18-L33).

Computes counts from actual static records (intake catalog, local-development
ready catalog, replacement map, Content Registry, source logic catalogs,
manifests, Poly Haven selection, font audit, localization keys, candidate disk
scan) grouped by category and state, then verifies quantity claims frozen in
authoritative documents (D18-L01 baseline section 1.2, D18-L14 taxonomy
section 10, D18-L15 result, D15-S02 snapshot).

Rules (frozen by the task card and D18-L01 baseline):

* Every number is computed from a real record source; the tool never fills a
  missing source with requirement targets or folder counts.
* Claims whose record source does not exist on disk return ``UNKNOWN``.
  Requirement-target claims always return ``UNSUPPORTED``: a design goal is
  never a completion count.
* Duplicate records within a source (duplicate identity keys) are reported as
  count inflation: raw count, unique count, inflated delta and sample keys.
* The tool is strictly read-only: it never edits catalogs, documents,
  registry, .gitignore or resource directories, and never grants approval
  states. It does not treat any target as a completion count.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Iterable


REPO_ROOT = Path(__file__).resolve().parents[1]

INTAKE_CATALOG = "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
LOCAL_DEV_CATALOG = "docs/game/design/common/assets/d18_local_development_ready_catalog.json"
REPLACEMENT_MAP = "docs/game/design/common/assets/replacement/d18_placeholder_replacement_map.json"
REGISTRY_DIR = "game/Assets/Resources/ContentRegistry"
CANDIDATES_DIR = "game/Assets/Art/Candidates"
LOCALIZATION_DIR = "game/Assets/Resources/Localization/D15_A08"
POLYHAVEN_SELECTION = "docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_selected_assets.json"
FONT_AUDIT = "docs/game/design/common/assets/packs/ui_materials_audio/font_license_audit.json"
SOURCE_CATALOGS = {
    "kenney": "docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json",
    "quaternius": "docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json",
    "polyhaven": "docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json",
    "ambientcg": "docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json",
}
UI_MATERIALS_PACKS = "docs/game/design/common/assets/packs/ui_materials_audio"

VALID_STATES = {
    "Candidate",
    "IntakeEligible",
    "RuntimeRequested",
    "RuntimeApproved",
    "ReleaseApproved",
    "Rejected",
    "Revoked",
}


def load_json(rel_path: str) -> tuple[Any | None, str | None]:
    path = REPO_ROOT / rel_path
    if not path.exists():
        return None, f"记录源不存在：{rel_path}"
    try:
        return json.loads(path.read_text(encoding="utf-8")), None
    except (json.JSONDecodeError, OSError) as exc:
        return None, f"记录源不可读：{rel_path}（{exc}）"


def duplicates(records: Iterable[str]) -> list[tuple[str, int]]:
    return [(key, count) for key, count in Counter(records).items() if count > 1]


def _leaf_keys(obj: Any) -> int:
    count = 0
    if isinstance(obj, dict):
        for value in obj.values():
            count += _leaf_keys(value)
    else:
        count = 1
    return count


def _manifest_paths(intake: dict) -> dict[str, str]:
    ids: dict[str, str] = {}
    for entry in intake.get("eligible_entries", []):
        ids[entry.get("manifest_id")] = entry.get("manifest_path")
    for excluded in intake.get("excluded_manifests", []):
        ids[excluded.get("manifest_id")] = excluded.get("manifest_path")
    return ids


CLAIMS: list[dict[str, Any]] = [
    # --- D18-L01 基线 §1.2 声明 ---
    {"claim_id": "intake_manifest_total", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "manifest_total", "expected": 30, "kind": "derived", "basis": "准入目录 eligible+excluded 的 manifest_path 去重数（30 条路径均需真实存在）"},
    {"claim_id": "intake_manifest_intake_eligible", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "manifest_intake_eligible", "expected": 26, "kind": "derived", "basis": "eligible_entries 中 distinct manifest_id 数"},
    {"claim_id": "intake_excluded_manifest_total", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "excluded_manifest_total", "expected": 4, "kind": "derived", "basis": "excluded_manifests 数组长度"},
    {"claim_id": "intake_candidate_file_total", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "candidate_file_total", "expected": 1895, "kind": "derived", "basis": "30 个被声明 manifest 的 candidate_files 长度之和（按 manifest_path 真实读取）"},
    {"claim_id": "intake_eligible_file_total", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "intake_eligible_file_total", "expected": 1865, "kind": "derived", "basis": "eligible_entries 条目数"},
    {"claim_id": "intake_duplicate_manifest_file_references", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "duplicate_manifest_file_references", "expected": 0, "kind": "derived", "basis": "eligible_entries 内 candidate_file_path 重复数（重复即虚增）"},
    {"claim_id": "intake_errors", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "errors", "expected": 0, "kind": "record_value", "basis": "准入目录 counts.errors 字段"},
    {"claim_id": "disk_candidate_files", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "候选磁盘文件（非 .meta）", "expected": 2124, "kind": "disk_count", "basis": "game/Assets/Art/Candidates 递归扫描非 .meta 文件数"},
    {"claim_id": "disk_candidate_top_dirs", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "候选顶层目录", "expected": 5, "kind": "disk_count", "basis": "候选根下顶层目录去重数"},
    # --- 本地就绪目录 ---
    {"claim_id": "local_dev_status", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "status", "expected": "BLOCKED", "kind": "record_value", "basis": "d18_local_development_ready_catalog.json status 字段"},
    {"claim_id": "local_development_ready_count", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "local_development_ready_count", "expected": 0, "kind": "record_value", "basis": "目录字段"},
    {"claim_id": "release_approved_count", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "release_approved_count", "expected": 0, "kind": "record_value", "basis": "目录字段"},
    {"claim_id": "pending_categories_total", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "Pending 类别数", "expected": 10, "kind": "derived", "basis": "pending_categories 数组长度"},
    {"claim_id": "pending_category_environment", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "environment Pending", "expected": 332, "kind": "derived", "basis": "pending_categories 中 category=environment 的 candidate_files"},
    {"claim_id": "pending_category_character", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "character Pending", "expected": 6, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_animal", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "animal Pending", "expected": 13, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_fish", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "fish Pending", "expected": 7, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_ship", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "ship Pending", "expected": 13, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_building", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "building Pending", "expected": 104, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_prop", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "prop Pending", "expected": 37, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_ui", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "ui Pending", "expected": 1224, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_material", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "material Pending", "expected": 91, "kind": "derived", "basis": "同上"},
    {"claim_id": "pending_category_audio", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "audio Pending", "expected": 30, "kind": "derived", "basis": "同上"},
    # --- 占位替换映射（D18-L01 §1.2 / 映射 audit） ---
    {"claim_id": "replacement_input", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "替换输入", "expected": 2264, "kind": "derived", "basis": "records 长度与 audit.input_placeholder_count 双重核对"},
    {"claim_id": "replacement_output", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "替换输出", "expected": 2264, "kind": "record_value", "basis": "audit.output_record_count"},
    {"claim_id": "replacement_mapped", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "mapped", "expected": 1107, "kind": "record_value", "basis": "audit.mapped_count"},
    {"claim_id": "replacement_unmapped", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "unmapped", "expected": 1157, "kind": "record_value", "basis": "audit.unmapped_count"},
    {"claim_id": "replacement_blocked", "document": "D18_LOGIC_GOVERNANCE_BASELINE §1.2", "metric": "blocked", "expected": 0, "kind": "record_value", "basis": "audit.blocked_count"},
    {"claim_id": "replacement_dup_content_id", "document": "本审计口径", "metric": "重复 content_id", "expected": 0, "kind": "derived", "basis": "records 内 content_id 重复检测（重复即虚增）"},
    {"claim_id": "replacement_dup_candidate_path", "document": "本审计口径", "metric": "重复候选路径", "expected": 0, "kind": "derived", "basis": "records 内候选路径字段重复检测"},
    # --- Content Registry（D15-S02 快照 2,812 / 当前磁盘扫描） ---
    {"claim_id": "registry_entries", "document": "D15-S02 验收快照（CONTEXT 2026-08-12）", "metric": "Registry 条目", "expected": 2812, "kind": "derived", "basis": "game/Assets/Resources/ContentRegistry 条目 JSON 文件数（排除 registry 索引文件）"},
    {"claim_id": "registry_dup_content_id", "document": "本审计口径", "metric": "重复 content_id", "expected": 0, "kind": "derived", "basis": "registry 条目 content_id 重复检测"},
    # --- 来源 catalog（L21—L24） ---
    {"claim_id": "source_kenney_records", "document": "D18-L21 目录（self）", "metric": "records", "expected": None, "kind": "source_catalog", "basis": "d18_kenney_logic_catalog.json records 数；状态必须全部命中七状态枚举、无重复 canonical_asset_id"},
    {"claim_id": "source_quaternius_records", "document": "D18-L22 目录（self）", "metric": "records", "expected": None, "kind": "source_catalog", "basis": "d18_quaternius_logic_catalog.json 同上"},
    {"claim_id": "source_polyhaven_records", "document": "D18-L23 目录（self）", "metric": "records", "expected": None, "kind": "source_catalog", "basis": "d18_polyhaven_logic_catalog.json 同上"},
    {"claim_id": "source_ambientcg_records", "document": "D18-L24 目录（self）", "metric": "records", "expected": None, "kind": "source_catalog", "basis": "d18_ambientcg_logic_catalog.json 同上"},
    # --- D18-L14 第 10 节声明 ---
    {"claim_id": "l14_domain_ui", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "domain=ui", "expected": 1224, "kind": "derived", "basis": "eligible_entries 按 domain 计数"},
    {"claim_id": "l14_domain_audio", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "domain=audio", "expected": 30, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_domain_material", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "domain=material", "expected": 87, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_domain_lighting_reference", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "domain=lighting_reference", "expected": 4, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_ui_audio_material_total", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "UI/音频/材质域合计", "expected": 1345, "kind": "derived", "basis": "ui+audio+material+lighting_reference 之和"},
    {"claim_id": "l14_manifest_kenney_ui_v2", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "kenney_ui_v2 文件数", "expected": 868, "kind": "derived", "basis": "manifest candidate_files 长度"},
    {"claim_id": "l14_manifest_kenney_input_prompts_v15", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "kenney_input_prompts_v15 文件数", "expected": 356, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_manifest_kenney_ui_audio_v1", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "kenney_ui_audio_v1 文件数", "expected": 10, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_manifest_kenney_interface_sounds_v1", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "kenney_interface_sounds_v1 文件数", "expected": 10, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_manifest_kenney_impact_sounds_v1", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "kenney_impact_sounds_v1 文件数", "expected": 10, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_manifest_polyhaven_ui_materials_selected_v1", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "polyhaven_ui_materials_selected_v1 文件数", "expected": 79, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_manifest_polyhaven_hdri_sky_references_v1", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "polyhaven_hdri_sky_references_v1 文件数", "expected": 4, "kind": "derived", "basis": "同上"},
    {"claim_id": "l14_polyhaven_materials", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "Poly Haven 材质数", "expected": 20, "kind": "record_value", "basis": "polyhaven_selected_assets.json materials 长度"},
    {"claim_id": "l14_polyhaven_hdris", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "Poly Haven HDRI 数", "expected": 4, "kind": "record_value", "basis": "polyhaven_selected_assets.json hdris 长度"},
    {"claim_id": "l14_font_records", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "字体记录数", "expected": 1, "kind": "derived", "basis": "font_license_audit.json records 长度"},
    {"claim_id": "l14_localization_keys", "document": "D18_TAXONOMY_UI_AUDIO §10", "metric": "本地化 key 数（每 locale）", "expected": 452, "kind": "derived", "basis": "D15_A08/*.json keys 子树叶子数；zh-CN/en-US 两文件应一致"},
    # --- 需求目标（只作对照，永远不是完成数） ---
    {"claim_id": "req_vegetation_entries_target", "document": "REQUIREMENTS §6", "metric": "植被内容条目目标", "expected": "1,000–2,000", "kind": "target", "basis": "内容规模目标是设计目标；无实际记录源时不可当作当前数量"},
    {"claim_id": "req_animal_entries_target", "document": "REQUIREMENTS §6", "metric": "动物条目目标", "expected": 300, "kind": "target", "basis": "同上"},
]


def compute_claim(claim: dict[str, Any]) -> dict[str, Any]:
    result = {
        "claim_id": claim["claim_id"],
        "document": claim["document"],
        "metric": claim["metric"],
        "expected": claim["expected"],
        "status": "UNKNOWN",
        "actual": None,
        "basis": claim["basis"],
        "reason": "",
    }
    if claim["kind"] == "target":
        result["status"] = "UNSUPPORTED"
        result["reason"] = "目标是设计目标，不是当前完成数量；需要实际记录源才能计算，本工具不把目标当完成数。"
        return result
    kind = claim["kind"]
    if kind == "disk_count":
        if claim["claim_id"] == "disk_candidate_files":
            if not (REPO_ROOT / CANDIDATES_DIR).exists():
                result["reason"] = f"记录源不存在：{CANDIDATES_DIR}"
                return result
            files = [p for p in (REPO_ROOT / CANDIDATES_DIR).rglob("*") if p.is_file() and not p.name.endswith(".meta")]
            result["actual"] = len(files)
        else:
            if not (REPO_ROOT / CANDIDATES_DIR).exists():
                result["reason"] = f"记录源不存在：{CANDIDATES_DIR}"
                return result
            top = {
                p.relative_to(REPO_ROOT / CANDIDATES_DIR).parts[0]
                for p in (REPO_ROOT / CANDIDATES_DIR).rglob("*")
                if p.is_file() and not p.name.endswith(".meta")
            }
            result["actual"] = len(top)
    elif claim["claim_id"].startswith("intake_") or claim["claim_id"].startswith("l14_domain") or claim["claim_id"] == "l14_ui_audio_material_total":
        intake, error = load_json(INTAKE_CATALOG)
        if error:
            result["reason"] = error
            return result
        entries = intake.get("eligible_entries", [])
        if claim["claim_id"] == "intake_manifest_total":
            ids = _manifest_paths(intake)
            result["actual"] = len(ids)
            missing = [p for p in ids.values() if not (REPO_ROOT / p).exists()]
            if missing:
                result["reason"] = f"{len(missing)} 条被声明 manifest_path 磁盘不存在：{missing[:3]}"
        elif claim["claim_id"] == "intake_manifest_intake_eligible":
            result["actual"] = len({e.get("manifest_id") for e in entries})
        elif claim["claim_id"] == "intake_excluded_manifest_total":
            result["actual"] = len(intake.get("excluded_manifests", []))
        elif claim["claim_id"] == "intake_candidate_file_total":
            total = 0
            missing_manifest = 0
            for mid, path in _manifest_paths(intake).items():
                manifest, error = load_json(path)
                if error or manifest is None:
                    missing_manifest += 1
                    continue
                total += len(manifest.get("candidate_files") or [])
            result["actual"] = total
            if missing_manifest:
                result["reason"] = f"{missing_manifest} 个被声明 manifest 不可读"
        elif claim["claim_id"] == "intake_eligible_file_total":
            result["actual"] = len(entries)
        elif claim["claim_id"] == "intake_duplicate_manifest_file_references":
            dup = duplicates(e.get("candidate_file_path") for e in entries)
            result["actual"] = len(dup)
            if dup:
                result["reason"] = f"重复 candidate_file_path：{dup[:3]}（raw={len(entries)}, unique={len(entries) - sum(c - 1 for _, c in dup)}）"
        elif claim["claim_id"] == "intake_errors":
            result["actual"] = (intake.get("counts") or {}).get("errors")
        elif claim["claim_id"] == "l14_domain_ui":
            result["actual"] = sum(1 for e in entries if e.get("domain") == "ui")
        elif claim["claim_id"] == "l14_domain_audio":
            result["actual"] = sum(1 for e in entries if e.get("domain") == "audio")
        elif claim["claim_id"] == "l14_domain_material":
            result["actual"] = sum(1 for e in entries if e.get("domain") == "material")
        elif claim["claim_id"] == "l14_domain_lighting_reference":
            result["actual"] = sum(1 for e in entries if e.get("domain") == "lighting_reference")
        else:  # l14_ui_audio_material_total
            domains = {"ui", "audio", "material", "lighting_reference"}
            result["actual"] = sum(1 for e in entries if e.get("domain") in domains)
    elif claim["claim_id"] == "pending_categories_total" or claim["claim_id"].startswith("pending_category_") or claim["claim_id"] in ("local_dev_status", "local_development_ready_count", "release_approved_count"):
        catalog, error = load_json(LOCAL_DEV_CATALOG)
        if error:
            result["reason"] = error
            return result
        if claim["claim_id"] == "local_dev_status":
            result["actual"] = catalog.get("status")
        elif claim["claim_id"] == "local_development_ready_count":
            result["actual"] = catalog.get("local_development_ready_count")
        elif claim["claim_id"] == "release_approved_count":
            result["actual"] = catalog.get("release_approved_count")
        elif claim["claim_id"] == "pending_categories_total":
            result["actual"] = len(catalog.get("pending_categories", []))
        else:
            category = claim["claim_id"].replace("pending_category_", "")
            for item in catalog.get("pending_categories", []):
                if item.get("category") == category:
                    result["actual"] = item.get("candidate_files")
                    break
            else:
                result["reason"] = f"pending_categories 中无 category={category}"
    elif claim["claim_id"].startswith("replacement_"):
        mapping, error = load_json(REPLACEMENT_MAP)
        if error:
            result["reason"] = error
            return result
        audit = mapping.get("audit") or {}
        records = mapping.get("records", [])
        if claim["claim_id"] == "replacement_input":
            result["actual"] = len(records)
            if audit.get("input_placeholder_count") is not None and result["actual"] != audit.get("input_placeholder_count"):
                result["reason"] = f"records 长度 {result['actual']} 与 audit.input_placeholder_count {audit.get('input_placeholder_count')} 不一致"
        elif claim["claim_id"] == "replacement_output":
            result["actual"] = audit.get("output_record_count")
        elif claim["claim_id"] == "replacement_mapped":
            result["actual"] = audit.get("mapped_count")
        elif claim["claim_id"] == "replacement_unmapped":
            result["actual"] = audit.get("unmapped_count")
        elif claim["claim_id"] == "replacement_blocked":
            result["actual"] = audit.get("blocked_count")
        elif claim["claim_id"] == "replacement_dup_content_id":
            dup = duplicates(r.get("content_id") for r in records)
            result["actual"] = len(dup)
            if dup:
                result["reason"] = f"重复 content_id：{dup[:3]}"
        else:  # replacement_dup_candidate_path
            keys = []
            for record in records:
                for field in ("candidate_path", "candidate_file_path", "source_path"):
                    value = (record.get("placeholder_asset") or {}).get(field) or record.get(field)
                    if value:
                        keys.append(value)
            dup = duplicates(keys)
            result["actual"] = len(dup)
            if dup:
                result["reason"] = f"重复候选路径：{dup[:3]}"
    elif claim["claim_id"].startswith("registry_"):
        registry_dir = REPO_ROOT / REGISTRY_DIR
        if not registry_dir.exists():
            result["reason"] = f"记录源不存在：{REGISTRY_DIR}"
            return result
        files = sorted(registry_dir.rglob("*.json"))
        content_ids = []
        for path in files:
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, OSError):
                continue
            if not isinstance(data, dict):
                continue
            identity = data.get("content_id") or data.get("id")
            if identity is None:
                continue
            content_ids.append(identity)
        if claim["claim_id"] == "registry_entries":
            result["actual"] = len(content_ids)
        else:
            dup = duplicates(content_ids)
            result["actual"] = len(dup)
            if dup:
                result["reason"] = f"重复 content_id：{dup[:3]}"
    elif claim["claim_id"].startswith("source_"):
        source = claim["claim_id"].replace("source_", "").replace("_records", "")
        rel_path = SOURCE_CATALOGS.get(source)
        if rel_path is None:
            result["reason"] = f"未知来源：{source}"
            return result
        catalog, error = load_json(rel_path)
        if error:
            result["reason"] = error
            return result
        records = catalog.get("records", [])
        result["actual"] = len(records)
        states = {r.get("state") for r in records}
        invalid_states = sorted(states - VALID_STATES)
        dup = duplicates(r.get("canonical_asset_id") for r in records)
        if invalid_states:
            result["status"] = "DIFF"
            result["reason"] = f"存在非冻结状态：{invalid_states}"
        if dup:
            result["status"] = "DIFF"
            result["reason"] += f"；重复 canonical_asset_id：{dup[:3]}" if result["reason"] else f"重复 canonical_asset_id：{dup[:3]}"
    elif claim["claim_id"].startswith("l14_manifest_"):
        intake, error = load_json(INTAKE_CATALOG)
        if error:
            result["reason"] = error
            return result
        pack_id = {
            "l14_manifest_kenney_ui_v2": "pack_kenney_ui_pack_v2",
            "l14_manifest_kenney_input_prompts_v15": "pack_kenney_input_prompts_v15",
            "l14_manifest_kenney_ui_audio_v1": "pack_kenney_ui_audio_v1",
            "l14_manifest_kenney_interface_sounds_v1": "pack_kenney_interface_sounds_v1",
            "l14_manifest_kenney_impact_sounds_v1": "pack_kenney_impact_sounds_v1",
            "l14_manifest_polyhaven_ui_materials_selected_v1": "pack_polyhaven_ui_materials_selected_v1",
            "l14_manifest_polyhaven_hdri_sky_references_v1": "pack_polyhaven_hdri_sky_references_v1",
        }[claim["claim_id"]]
        manifest, error = load_json(_manifest_paths(intake).get(pack_id, ""))
        if error or manifest is None:
            result["reason"] = error or f"manifest 缺失：{pack_id}"
            return result
        result["actual"] = len(manifest.get("candidate_files") or [])
    elif claim["claim_id"] == "l14_polyhaven_materials":
        data, error = load_json(POLYHAVEN_SELECTION)
        if error:
            result["reason"] = error
            return result
        result["actual"] = len(data.get("materials") or [])
    elif claim["claim_id"] == "l14_polyhaven_hdris":
        data, error = load_json(POLYHAVEN_SELECTION)
        if error:
            result["reason"] = error
            return result
        result["actual"] = len(data.get("hdris") or [])
    elif claim["claim_id"] == "l14_font_records":
        data, error = load_json(FONT_AUDIT)
        if error:
            result["reason"] = error
            return result
        result["actual"] = len(data.get("records") or [])
    else:  # l14_localization_keys
        loc_dir = REPO_ROOT / LOCALIZATION_DIR
        if not loc_dir.exists():
            result["reason"] = f"记录源不存在：{LOCALIZATION_DIR}"
            return result
        counts = []
        for path in sorted(loc_dir.glob("*.json")):
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, OSError):
                continue
            counts.append(_leaf_keys(data.get("keys", {})))
        result["actual"] = counts[0] if counts else None
        if len(set(counts)) > 1:
            result["reason"] = f"各 locale key 数不一致：{counts}"
    if result["status"] == "UNKNOWN" and result["actual"] is not None:
        if result["expected"] is None:
            result["status"] = "REPORTED"
        elif result["actual"] == result["expected"]:
            result["status"] = "MATCH"
        else:
            result["status"] = "DIFF"
            generic = f"实际 {result['actual']} ≠ 声明 {result['expected']}"
            result["reason"] = f"{result['reason']}；{generic}" if result["reason"] else generic
    return result


def duplicate_inflation() -> list[dict[str, Any]]:
    findings: list[dict[str, Any]] = []
    sources = [
        ("intake_catalog", INTAKE_CATALOG, "candidate_file_path", lambda d: [e.get("candidate_file_path") for e in d.get("eligible_entries", [])]),
        ("intake_catalog", INTAKE_CATALOG, "intake_id", lambda d: [e.get("intake_id") for e in d.get("eligible_entries", [])]),
        ("replacement_map", REPLACEMENT_MAP, "content_id", lambda d: [r.get("content_id") for r in d.get("records", [])]),
        ("registry", REGISTRY_DIR, "content_id", lambda d: d),
    ]
    for source, rel_path, key_field, extract in sources:
        if source == "registry":
            registry_dir = REPO_ROOT / rel_path
            if not registry_dir.exists():
                findings.append({"source": source, "key_field": key_field, "raw": None, "unique": None, "inflated": None, "duplicates": [], "reason": f"记录源不存在：{rel_path}"})
                continue
            keys = []
            for path in sorted(registry_dir.rglob("*.json")):
                try:
                    data = json.loads(path.read_text(encoding="utf-8"))
                except (json.JSONDecodeError, OSError):
                    continue
                if isinstance(data, dict):
                    identity = data.get("content_id") or data.get("id")
                    if identity is not None:
                        keys.append(identity)
        else:
            data, error = load_json(rel_path)
            if error:
                findings.append({"source": source, "key_field": key_field, "raw": None, "unique": None, "inflated": None, "duplicates": [], "reason": error})
                continue
            keys = [k for k in extract(data) if k]
        dup = duplicates(keys)
        findings.append(
            {
                "source": source,
                "key_field": key_field,
                "raw": len(keys),
                "unique": len(keys) - sum(count - 1 for _, count in dup),
                "inflated": sum(count - 1 for _, count in dup),
                "duplicates": dup[:5],
                "reason": "",
            }
        )
    for source, rel_path in SOURCE_CATALOGS.items():
        catalog, error = load_json(rel_path)
        if error:
            findings.append({"source": f"source_catalog_{source}", "key_field": "canonical_asset_id", "raw": None, "unique": None, "inflated": None, "duplicates": [], "reason": error})
            continue
        records = catalog.get("records", [])
        dup = duplicates(r.get("canonical_asset_id") for r in records)
        findings.append(
            {
                "source": f"source_catalog_{source}",
                "key_field": "canonical_asset_id",
                "raw": len(records),
                "unique": len(records) - sum(count - 1 for _, count in dup),
                "inflated": sum(count - 1 for _, count in dup),
                "duplicates": dup[:5],
                "reason": "",
            }
        )
    return findings


def per_manifest_entry_observation() -> list[dict[str, Any]]:
    intake, error = load_json(INTAKE_CATALOG)
    if error:
        return [{"status": "UNKNOWN", "reason": error}]
    entries_by_manifest: Counter[str] = Counter(e.get("manifest_id") for e in intake.get("eligible_entries", []))
    excluded = {x.get("manifest_id") for x in intake.get("excluded_manifests", [])}
    observations = []
    for manifest_id, entry_count in sorted(entries_by_manifest.items()):
        if manifest_id in excluded:
            continue
        manifest, error = load_json(_manifest_paths(intake).get(manifest_id, ""))
        if error or manifest is None:
            continue
        declared = len(manifest.get("candidate_files") or [])
        if declared != entry_count:
            observations.append(
                {
                    "manifest_id": manifest_id,
                    "manifest_candidate_files": declared,
                    "eligible_entries": entry_count,
                    "difference": declared - entry_count,
                    "status": "INFO",
                    "note": "manifest 声明文件数多于准入条目数：差额未被准入目录收录（可能为重复/未准入文件），只报告不猜测原因",
                }
            )
    return observations


def audit_claims() -> list[dict[str, Any]]:
    return [compute_claim(claim) for claim in CLAIMS]


def full_report() -> dict[str, Any]:
    claims = audit_claims()
    by_status = Counter(result["status"] for result in claims)
    return {
        "audit_kind": "d18_catalog_claim_audit",
        "task_id": "D18-L33",
        "read_only": True,
        "claims_total": len(claims),
        "by_status": dict(by_status),
        "claims": claims,
        "duplicate_inflation": duplicate_inflation(),
        "observations": per_manifest_entry_observation(),
    }


def _print_report(report: dict[str, Any]) -> None:
    print(f"D18-L33 目录声明审计（只读）：共 {report['claims_total']} 条声明，状态分布 {report['by_status']}")
    for claim in report["claims"]:
        status = claim["status"]
        actual = "—" if claim["actual"] is None else claim["actual"]
        line = f"[{status:>10}] {claim['claim_id']}: 声明={claim['expected']} 实际={actual}（{claim['document']}）"
        print(line)
        if claim["reason"]:
            print(f"           原因: {claim['reason']}")
    print()
    print("重复虚增检测（raw / unique / inflated）：")
    for finding in report["duplicate_inflation"]:
        raw = "—" if finding["raw"] is None else finding["raw"]
        unique = "—" if finding["unique"] is None else finding["unique"]
        inflated = "—" if finding["inflated"] is None else finding["inflated"]
        print(f"  {finding['source']}[{finding['key_field']}]: raw={raw} unique={unique} inflated={inflated}{'  ' + finding['reason'] if finding['reason'] else ''}")
    observations = report.get("observations", [])
    if observations:
        print()
        print("观察项（非声明核对，INFO）：")
        for obs in observations:
            print(f"  {obs.get('manifest_id')}: manifest={obs.get('manifest_candidate_files')} eligible={obs.get('eligible_entries')} 差={obs.get('difference')}")


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18 目录数量声明审计（只读，D18-L33）")
    parser.add_argument("--check", action="store_true", help="执行全量声明审计并输出人类可读报告")
    parser.add_argument("--json", action="store_true", help="同时输出完整 JSON 报告")
    args = parser.parse_args(list(argv) if argv is not None else None)

    report = full_report()
    if args.json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
    _print_report(report)
    has_diff = any(claim["status"] == "DIFF" for claim in report["claims"])
    has_unknown_source = any(
        claim["status"] == "UNKNOWN" and claim["reason"].startswith("记录源") for claim in report["claims"]
    )
    print()
    print(f"结论：DIFF={sum(1 for c in report['claims'] if c['status']=='DIFF')}；UNKNOWN（缺记录源）={sum(1 for c in report['claims'] if c['status']=='UNKNOWN')}；目标对照 UNSUPPORTED={sum(1 for c in report['claims'] if c['status']=='UNSUPPORTED')}")
    if has_diff:
        print("存在声明与实际不一致（DIFF），退出码 1。")
    return 1 if has_diff else (0 if not has_unknown_source else 0)


if __name__ == "__main__":
    raise SystemExit(main())
