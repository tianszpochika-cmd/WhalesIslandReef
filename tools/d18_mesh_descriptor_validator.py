"""Read-only mesh descriptor validator for D18 logic assets (D18-L26).

Validates D18-L25 ``d18_import_descriptor`` records (static data; the frozen
schema is ``tools/schemas/d18_import_descriptor.schema.json``) with field-level
errors.  It is a self-contained Python mirror of the schema plus the semantic
rules of the D18-L26 task card:

* units / axes / root-motion / bounds / collision intent / LOD intent / category
  match / fallback: every P0 field is checked and illegal values produce
  field-level errors (field name + rule + severity + message);
* category match: when ``--asset-records`` is supplied, ``source_asset_id`` must
  resolve to an L02 record whose ``category`` equals the descriptor ``type``;
* collision intent semantics follow the L25 description: capsule_agent for
  characters/animals/fish, simple_box/convex_mesh for ships and props,
  terrain/simple_box/convex_mesh for environment and buildings, none for
  UI/audio/material/lighting_reference;
* missing lineage of geometry fields (null LOD for geometric types, non-empty
  material slots for non-geometric types, root-motion without clips) is blocked;
* a missing or malformed ``fallback`` is blocked (every import attempt needs an
  honest rollback statement);
* ``represents_imported_asset`` must be false; a true value is invalid by
  construction (the descriptor never claims an asset was imported).

The tool is strictly read-only and offline: it never reads FBX/OBJ/glTF, never
calls Unity, never opens or creates textures/models/binaries, never imports
anything and never grants approval.  Real vertex/collision/LOD/import checks
belong to a future Runtime phase.

Findings format (field-level errors):

.. code-block:: json

    {
      "source_asset_id": "asset_...",
      "field": "bounds.height_m",
      "rule": "bounds_positive",
      "severity": "P0",
      "message": "..."
    }
"""

from __future__ import annotations

import argparse
import json
import math
import re
import sys
from pathlib import Path
from typing import Any

TASK = "D18-L26"
TOOL = "tools/d18_mesh_descriptor_validator.py"
DESCRIPTOR_KIND = "d18_import_descriptor"
SCHEMA_VERSION = 1

GEOMETRIC_TYPES = frozenset({"character", "animal", "fish", "ship", "environment", "prop", "building"})
NON_GEOMETRIC_TYPES = frozenset({"audio", "ui", "material", "lighting_reference"})
ALL_TYPES = GEOMETRIC_TYPES | NON_GEOMETRIC_TYPES

UNITS_ENUM = frozenset({"meters", "centimeters", "inches", "feet", "unknown"})
UP_ENUM = frozenset({"y", "z", "unknown"})
FORWARD_ENUM = frozenset({"z", "x", "-z", "-x", "unknown"})
ROOT_MOTION_ENUM = frozenset({"none", "required", "derived", "unknown"})
COLLISION_INTENT_ENUM = frozenset(
    {"none", "simple_box", "convex_mesh", "capsule_agent", "terrain", "interaction_anchor_only", "unknown"}
)
SKELETON_ENUM = frozenset({"none", "static", "generic", "humanoid", "animal", "fish", "mech"})
SLOT_ROLE_ENUM = frozenset(
    {"body", "outfit", "fabric", "wood", "metal", "glass", "emissive", "water", "stone", "organic", "ui", "misc"}
)
ANIMATION_ROLE_ENUM = frozenset(
    {"idle", "walk", "run", "work", "combat", "sail", "swim", "fly", "emote", "interact", "other"}
)

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
SNAKE_CASE_RE = re.compile(r"^[a-z0-9_]+$")
REPO_PATH_RE = re.compile(r"^[A-Za-z0-9_.][A-Za-z0-9_./-]*$")
TRAVERSAL_RE = re.compile(r"(^|/)\.\.(/|$)")
FALLBACK_REF_RE = re.compile(r"^(asset_[a-z0-9_]+|placeholder_[a-z0-9_]+)$")
HTTPS_URL_RE = re.compile(r"^https://[^\s]+$")

# 类型 → 允许的碰撞意图（L25 语义描述）
COLLISION_INTENT_BY_TYPE: dict[str, frozenset[str]] = {
    "character": frozenset({"capsule_agent", "interaction_anchor_only"}),
    "animal": frozenset({"capsule_agent", "interaction_anchor_only"}),
    "fish": frozenset({"capsule_agent", "interaction_anchor_only"}),
    "ship": frozenset({"simple_box", "convex_mesh", "interaction_anchor_only"}),
    "prop": frozenset({"simple_box", "convex_mesh", "interaction_anchor_only"}),
    "environment": frozenset({"terrain", "simple_box", "convex_mesh", "interaction_anchor_only"}),
    "building": frozenset({"terrain", "simple_box", "convex_mesh", "interaction_anchor_only"}),
    "audio": frozenset({"none"}),
    "ui": frozenset({"none"}),
    "material": frozenset({"none"}),
    "lighting_reference": frozenset({"none"}),
}

# 类型 → 允许的骨架意图（镜像 L25 allOf，缺失即越界）
SKELETON_BY_TYPE: dict[str, frozenset[str]] = {
    "character": frozenset({"humanoid", "generic"}),
    "animal": frozenset({"animal", "generic"}),
    "fish": frozenset({"fish", "generic"}),
    "ship": frozenset({"none", "static"}),
    "environment": frozenset({"none", "static"}),
    "prop": frozenset({"none", "static"}),
    "building": frozenset({"none", "static"}),
    "audio": frozenset({"none"}),
    "ui": frozenset({"none"}),
    "material": frozenset({"none"}),
    "lighting_reference": frozenset({"none"}),
}

# 类型 → 允许的动画角色（语义建议，违规为 P1）
ANIMATION_ROLES_BY_TYPE: dict[str, frozenset[str]] = {
    "character": frozenset({"idle", "walk", "run", "work", "combat", "emote", "interact", "other"}),
    "animal": frozenset({"idle", "walk", "run", "swim", "fly", "interact", "emote", "other"}),
    "fish": frozenset({"idle", "swim", "other"}),
    "ship": frozenset({"idle", "sail", "work", "other"}),
    "environment": frozenset({"other"}),
    "building": frozenset({"other"}),
    "prop": frozenset({"other"}),
}

FALLBACK_PLACEHOLDER_KEYWORDS = ("placeholder", "keep", "保留", "占位", "回退", "fallback", "reject")


def _finding(source_asset_id: str, field: str, rule: str, severity: str, message: str) -> dict[str, str]:
    return {
        "source_asset_id": source_asset_id,
        "field": field,
        "rule": rule,
        "severity": severity,
        "message": message,
    }


def _is_positive_number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(float(value)) and value > 0


# ---------------------------------------------------------------------------
# field-level checks
# ---------------------------------------------------------------------------


def _check_top_level(descriptor: Any, source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    if not isinstance(descriptor, dict):
        return [_finding(source_id, "<descriptor>", "descriptor_shape_invalid", "P0", "描述器必须是 JSON 对象")]
    for field, value in descriptor.items():
        if field not in {
            "descriptor_kind", "schema_version", "source_asset_id", "type", "units", "axes", "bounds",
            "material_slots", "skeleton_type", "animation_list", "root_motion", "collision_intent",
            "lod_intent", "license_dependency", "fallback", "represents_imported_asset",
        }:
            findings.append(_finding(source_id, field, "unknown_top_level_field", "P0", f"未知顶层字段（拒绝 Unity Import Settings 等混入）：{field}"))
    return findings


def _check_scalar_fields(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    required = {
        "descriptor_kind", "schema_version", "source_asset_id", "type", "units", "axes", "bounds",
        "material_slots", "skeleton_type", "animation_list", "root_motion", "collision_intent",
        "lod_intent", "license_dependency", "fallback", "represents_imported_asset",
    }
    for field in sorted(required - set(descriptor)):
        findings.append(_finding(source_id, field, "required_field_missing", "P0", f"缺少必需字段 {field}"))

    if descriptor.get("descriptor_kind") != DESCRIPTOR_KIND:
        findings.append(_finding(source_id, "descriptor_kind", "descriptor_kind", "P0", f"descriptor_kind 必须为 {DESCRIPTOR_KIND}"))
    if descriptor.get("schema_version") != SCHEMA_VERSION:
        findings.append(_finding(source_id, "schema_version", "schema_version", "P0", f"schema_version 必须为 {SCHEMA_VERSION}"))

    source_asset_id = descriptor.get("source_asset_id")
    if not isinstance(source_asset_id, str) or not ASSET_ID_RE.fullmatch(source_asset_id):
        findings.append(_finding(source_id, "source_asset_id", "source_asset_id_format", "P0", "source_asset_id 必须匹配 ^asset_[a-z0-9_]+$"))

    asset_type = descriptor.get("type")
    if asset_type not in ALL_TYPES:
        findings.append(_finding(source_id, "type", "type_enum", "P0", f"type 必须是 {sorted(ALL_TYPES)} 之一，实际为 {asset_type!r}"))

    units = descriptor.get("units")
    if units not in UNITS_ENUM:
        findings.append(_finding(source_id, "units", "units_enum", "P0", f"units 必须是 {sorted(UNITS_ENUM)} 之一，实际为 {units!r}"))
    elif units == "unknown":
        findings.append(_finding(source_id, "units", "units_unknown", "P1", "单位 unknown：Runtime 导入前必须解析，不得静默假设"))

    root_motion = descriptor.get("root_motion")
    if root_motion not in ROOT_MOTION_ENUM:
        findings.append(_finding(source_id, "root_motion", "root_motion_enum", "P0", f"root_motion 必须是 {sorted(ROOT_MOTION_ENUM)} 之一，实际为 {root_motion!r}"))
    elif root_motion == "unknown":
        findings.append(_finding(source_id, "root_motion", "root_motion_unknown", "P1", "root-motion 意图 unknown：Runtime 导入前必须解析"))

    collision_intent = descriptor.get("collision_intent")
    if collision_intent not in COLLISION_INTENT_ENUM:
        findings.append(_finding(source_id, "collision_intent", "collision_intent_enum", "P0", f"collision_intent 必须是 {sorted(COLLISION_INTENT_ENUM)} 之一，实际为 {collision_intent!r}"))
    elif collision_intent == "unknown":
        findings.append(_finding(source_id, "collision_intent", "collision_intent_unknown", "P1", "碰撞意图 unknown：Runtime 导入前必须解析"))

    skeleton_type = descriptor.get("skeleton_type")
    if skeleton_type not in SKELETON_ENUM:
        findings.append(_finding(source_id, "skeleton_type", "skeleton_type_enum", "P0", f"skeleton_type 必须是 {sorted(SKELETON_ENUM)} 之一，实际为 {skeleton_type!r}"))

    if descriptor.get("represents_imported_asset") is not False:
        findings.append(_finding(source_id, "represents_imported_asset", "imported_claim_invalid", "P0", "represents_imported_asset 必须为 false：描述器不得声称资源已导入"))
    return findings


def _check_axes(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    axes = descriptor.get("axes")
    if not isinstance(axes, dict):
        return [_finding(source_id, "axes", "axes_shape", "P0", "axes 必须是对象（up/forward）")]
    for field in set(axes) - {"up", "forward"}:
        findings.append(_finding(source_id, f"axes.{field}", "unknown_axes_field", "P0", f"axes 未知字段：{field}"))
    up = axes.get("up")
    if up not in UP_ENUM:
        findings.append(_finding(source_id, "axes.up", "axes_up_enum", "P0", f"axes.up 必须是 {sorted(UP_ENUM)} 之一，实际为 {up!r}"))
    elif up == "unknown":
        findings.append(_finding(source_id, "axes.up", "axes_unknown", "P1", "轴向上未知：Runtime 导入必须做显式变换，不得静默假设"))
    forward = axes.get("forward")
    if forward not in FORWARD_ENUM:
        findings.append(_finding(source_id, "axes.forward", "axes_forward_enum", "P0", f"axes.forward 必须是 {sorted(FORWARD_ENUM)} 之一，实际为 {forward!r}"))
    elif forward == "unknown":
        findings.append(_finding(source_id, "axes.forward", "axes_unknown", "P1", "轴向 forward 未知：Runtime 导入必须做显式变换"))
    return findings


def _check_bounds(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    bounds = descriptor.get("bounds")
    asset_type = descriptor.get("type")
    if bounds is None:
        if asset_type in GEOMETRIC_TYPES:
            findings.append(_finding(source_id, "bounds", "geometric_bounds_required", "P0", "几何类型必须声明预期 bounds（width_m/depth_m/height_m）"))
        return findings
    if not isinstance(bounds, dict):
        return [_finding(source_id, "bounds", "bounds_shape", "P0", "bounds 必须是对象或 null")]
    for field in set(bounds) - {"width_m", "depth_m", "height_m"}:
        findings.append(_finding(source_id, f"bounds.{field}", "unknown_bounds_field", "P0", f"bounds 未知字段：{field}"))
    values: list[tuple[str, Any]] = []
    for key in ("width_m", "depth_m", "height_m"):
        value = bounds.get(key)
        if not _is_positive_number(value):
            findings.append(_finding(source_id, f"bounds.{key}", "bounds_positive", "P0", f"bounds.{key} 必须是正有限数值，实际为 {value!r}"))
        else:
            values.append((key, float(value)))
    if asset_type in NON_GEOMETRIC_TYPES:
        findings.append(_finding(source_id, "bounds", "non_geometric_geometry", "P0", "非几何类型（audio/ui/material/lighting_reference）bounds 必须为 null"))
    if len(values) == 3:
        magnitudes = [value for _, value in values]
        if max(magnitudes) / min(magnitudes) > 100:
            findings.append(_finding(source_id, "bounds", "bounds_ratio_suspect", "P1", "bounds 维度比例超过 100:1，疑似数量级错误，需人工复核"))
    return findings


def _check_material_slots(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    slots = descriptor.get("material_slots")
    if not isinstance(slots, list):
        return [_finding(source_id, "material_slots", "material_slots_shape", "P0", "material_slots 必须是数组")]
    if descriptor.get("type") in NON_GEOMETRIC_TYPES and slots:
        findings.append(_finding(source_id, "material_slots", "non_geometric_material_slots", "P0", "非几何类型 material_slots 必须为空"))
    for index, slot in enumerate(slots):
        field = f"material_slots[{index}]"
        if not isinstance(slot, dict):
            findings.append(_finding(source_id, field, "slot_shape", "P0", "材质槽必须是对象"))
            continue
        for key in ("slot_name", "slot_role"):
            if key not in slot:
                findings.append(_finding(source_id, f"{field}.{key}", "required_field_missing", "P0", f"缺少必需字段 {key}"))
        slot_name = slot.get("slot_name")
        if not isinstance(slot_name, str) or not SNAKE_CASE_RE.fullmatch(slot_name):
            findings.append(_finding(source_id, f"{field}.slot_name", "slot_name_format", "P0", "slot_name 必须是 lower snake_case"))
        slot_role = slot.get("slot_role")
        if slot_role not in SLOT_ROLE_ENUM:
            findings.append(_finding(source_id, f"{field}.slot_role", "slot_role_enum", "P0", f"slot_role 必须是 {sorted(SLOT_ROLE_ENUM)} 之一，实际为 {slot_role!r}"))
        material_id = slot.get("material_asset_id")
        if material_id is not None and (not isinstance(material_id, str) or not ASSET_ID_RE.fullmatch(material_id)):
            findings.append(_finding(source_id, f"{field}.material_asset_id", "material_asset_id_format", "P0", "material_asset_id 必须匹配 ^asset_[a-z0-9_]+$"))
    return findings


def _check_skeleton(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    asset_type = descriptor.get("type")
    skeleton_type = descriptor.get("skeleton_type")
    allowed = SKELETON_BY_TYPE.get(asset_type) if asset_type in ALL_TYPES else None
    if allowed is not None and skeleton_type not in allowed:
        findings.append(_finding(source_id, "skeleton_type", "skeleton_intent_mismatch", "P0", f"类型 {asset_type!r} 的骨架意图必须是 {sorted(allowed)} 之一，实际为 {skeleton_type!r}"))
    return findings


def _check_animation_list(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    animations = descriptor.get("animation_list")
    if not isinstance(animations, list):
        return [_finding(source_id, "animation_list", "animation_list_shape", "P0", "animation_list 必须是数组")]
    asset_type = descriptor.get("type")
    if asset_type in NON_GEOMETRIC_TYPES and animations:
        findings.append(_finding(source_id, "animation_list", "non_geometric_animation_list", "P0", "非几何类型 animation_list 必须为空"))
    allowed_roles = ANIMATION_ROLES_BY_TYPE.get(asset_type)
    for index, clip in enumerate(animations):
        field = f"animation_list[{index}]"
        if not isinstance(clip, dict):
            findings.append(_finding(source_id, field, "animation_shape", "P0", "动画条目必须是对象"))
            continue
        for key in ("animation_key", "role"):
            if key not in clip:
                findings.append(_finding(source_id, f"{field}.{key}", "required_field_missing", "P0", f"缺少必需字段 {key}"))
        animation_key = clip.get("animation_key")
        if not isinstance(animation_key, str) or not SNAKE_CASE_RE.fullmatch(animation_key):
            findings.append(_finding(source_id, f"{field}.animation_key", "animation_key_format", "P0", "animation_key 必须是 lower snake_case"))
        role = clip.get("role")
        if role not in ANIMATION_ROLE_ENUM:
            findings.append(_finding(source_id, f"{field}.role", "animation_role_enum", "P0", f"role 必须是 {sorted(ANIMATION_ROLE_ENUM)} 之一，实际为 {role!r}"))
        elif allowed_roles is not None and role not in allowed_roles:
            findings.append(_finding(source_id, f"{field}.role", "animation_role_mismatch", "P1", f"类型 {asset_type!r} 的动画角色 {role!r} 不在建议集合 {sorted(allowed_roles)} 内"))
        if "root_motion" in clip and not isinstance(clip.get("root_motion"), bool):
            findings.append(_finding(source_id, f"{field}.root_motion", "animation_root_motion_type", "P0", "动画条目 root_motion 必须是布尔值"))
    return findings


def _check_lod_intent(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    lod = descriptor.get("lod_intent")
    asset_type = descriptor.get("type")
    if lod is None:
        if asset_type in GEOMETRIC_TYPES:
            findings.append(_finding(source_id, "lod_intent", "geometric_missing_lod_intent", "P0", "几何类型必须声明 lod_intent（levels 1-5）"))
        return findings
    if not isinstance(lod, dict):
        return [_finding(source_id, "lod_intent", "lod_intent_shape", "P0", "lod_intent 必须是对象或 null")]
    for field in set(lod) - {"levels", "generate_automatically"}:
        findings.append(_finding(source_id, f"lod_intent.{field}", "unknown_lod_field", "P0", f"lod_intent 未知字段：{field}"))
    levels = lod.get("levels")
    if not isinstance(levels, int) or isinstance(levels, bool) or not 1 <= levels <= 5:
        findings.append(_finding(source_id, "lod_intent.levels", "lod_levels_range", "P0", f"lod_intent.levels 必须是 1-5 的整数，实际为 {levels!r}"))
    generate = lod.get("generate_automatically")
    if not isinstance(generate, bool):
        findings.append(_finding(source_id, "lod_intent.generate_automatically", "lod_generate_type", "P0", "lod_intent.generate_automatically 必须是布尔值"))
    elif levels is not None and isinstance(levels, int) and levels > 1 and not generate:
        findings.append(_finding(source_id, "lod_intent", "lod_manual_generation", "P1", "levels>1 且 generate_automatically=false：需要手工 LOD 计划，Runtime 阶段确认"))
    if asset_type in NON_GEOMETRIC_TYPES:
        findings.append(_finding(source_id, "lod_intent", "non_geometric_lod_intent", "P0", "非几何类型 lod_intent 必须为 null"))
    return findings


def _check_license_dependency(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    dependency = descriptor.get("license_dependency")
    if not isinstance(dependency, dict):
        return [_finding(source_id, "license_dependency", "license_dependency_shape", "P0", "license_dependency 必须是对象")]
    for field in set(dependency) - {"license_id", "license_url", "license_evidence_path", "attribution_required"}:
        findings.append(_finding(source_id, f"license_dependency.{field}", "unknown_license_field", "P0", f"license_dependency 未知字段：{field}"))
    license_id = dependency.get("license_id")
    if not isinstance(license_id, str) or not license_id:
        findings.append(_finding(source_id, "license_dependency.license_id", "license_id_required", "P0", "license_id 缺失或为空"))
    license_url = dependency.get("license_url")
    if license_url is not None and (not isinstance(license_url, str) or not HTTPS_URL_RE.fullmatch(license_url)):
        findings.append(_finding(source_id, "license_dependency.license_url", "license_url_format", "P0", "license_url 必须是 https URL"))
    evidence = dependency.get("license_evidence_path")
    if not isinstance(evidence, str) or not REPO_PATH_RE.fullmatch(evidence) or TRAVERSAL_RE.search(evidence) or "\\" in evidence:
        findings.append(_finding(source_id, "license_dependency.license_evidence_path", "evidence_path_format", "P0", "license_evidence_path 必须是仓库内相对路径（禁绝对路径/遍历/反斜杠）"))
    if "attribution_required" in dependency and not isinstance(dependency.get("attribution_required"), bool):
        findings.append(_finding(source_id, "license_dependency.attribution_required", "attribution_type", "P0", "attribution_required 必须是布尔值"))
    return findings


def _check_fallback(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    fallback = descriptor.get("fallback")
    if not isinstance(fallback, dict):
        return [_finding(source_id, "fallback", "fallback_required", "P0", "必须声明 fallback（reference + description）；缺失回退阻断")]
    for field in set(fallback) - {"reference", "description"}:
        findings.append(_finding(source_id, f"fallback.{field}", "unknown_fallback_field", "P0", f"fallback 未知字段：{field}"))
    reference = fallback.get("reference")
    if not isinstance(reference, str) or not FALLBACK_REF_RE.fullmatch(reference):
        findings.append(_finding(source_id, "fallback.reference", "fallback_reference_format", "P0", "fallback.reference 必须是 asset_<...> 或 placeholder_<...>"))
    description = fallback.get("description")
    if not isinstance(description, str) or not description:
        findings.append(_finding(source_id, "fallback.description", "fallback_description", "P0", "fallback.description 不能为空（必须说明降级方式）"))
    if isinstance(reference, str) and reference.startswith("placeholder_") and isinstance(description, str):
        if not any(keyword in description.lower() for keyword in FALLBACK_PLACEHOLDER_KEYWORDS):
            findings.append(_finding(source_id, "fallback.description", "fallback_placeholder_consistency", "P1", "placeholder 回退引用建议在 description 中说明保留占位/降级方式"))
    return findings


def _check_collision_intent(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    asset_type = descriptor.get("type")
    collision_intent = descriptor.get("collision_intent")
    allowed = COLLISION_INTENT_BY_TYPE.get(asset_type)
    if allowed is not None and collision_intent not in allowed:
        findings.append(_finding(source_id, "collision_intent", "collision_intent_mismatch", "P0", f"类型 {asset_type!r} 的碰撞意图必须是 {sorted(allowed)} 之一，实际为 {collision_intent!r}"))
    return findings


def _check_root_motion_consistency(descriptor: dict[str, Any], source_id: str) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    root_motion = descriptor.get("root_motion")
    animations = descriptor.get("animation_list")
    if root_motion in ("required", "derived") and (not isinstance(animations, list) or not animations):
        findings.append(_finding(source_id, "root_motion", "root_motion_without_clips", "P0", "root_motion=required/derived 但 animation_list 为空：root-motion 意图需要 clip 支撑"))
    return findings


def _check_category_match(
    descriptor: dict[str, Any], source_id: str, asset_records: dict[str, dict[str, Any]] | None
) -> list[dict[str, str]]:
    findings: list[dict[str, str]] = []
    if asset_records is None:
        return findings
    record = asset_records.get(source_id)
    if record is None:
        return [_finding(source_id, "source_asset_id", "source_asset_id_unresolved", "P0", "source_asset_id 在提供的资源记录中不存在")]
    record_category = record.get("category")
    if record_category != descriptor.get("type"):
        findings.append(_finding(source_id, "type", "category_mismatch", "P0", f"描述器 type={descriptor.get('type')!r} 与 L02 记录 category={record_category!r} 不匹配"))
    return findings


# ---------------------------------------------------------------------------
# orchestrator
# ---------------------------------------------------------------------------


def validate_descriptor(
    descriptor: Any,
    asset_records: dict[str, dict[str, Any]] | None = None,
) -> list[dict[str, str]]:
    """Validate one descriptor record; returns field-level findings."""
    source_id = descriptor.get("source_asset_id", "<descriptor>") if isinstance(descriptor, dict) else "<descriptor>"
    findings: list[dict[str, str]] = []
    findings.extend(_check_top_level(descriptor, source_id))
    if not isinstance(descriptor, dict):
        return findings
    findings.extend(_check_scalar_fields(descriptor, source_id))
    findings.extend(_check_axes(descriptor, source_id))
    findings.extend(_check_bounds(descriptor, source_id))
    findings.extend(_check_material_slots(descriptor, source_id))
    findings.extend(_check_skeleton(descriptor, source_id))
    findings.extend(_check_animation_list(descriptor, source_id))
    findings.extend(_check_lod_intent(descriptor, source_id))
    findings.extend(_check_license_dependency(descriptor, source_id))
    findings.extend(_check_fallback(descriptor, source_id))
    findings.extend(_check_collision_intent(descriptor, source_id))
    findings.extend(_check_root_motion_consistency(descriptor, source_id))
    findings.extend(_check_category_match(descriptor, source_id, asset_records))
    return findings


def load_asset_records(path: Path | None) -> dict[str, dict[str, Any]] | None:
    """Load L02 asset records (array or {records: [...]}); None when not supplied."""
    if path is None:
        return None
    data = json.loads(path.read_text(encoding="utf-8"))
    if isinstance(data, dict):
        data = data.get("records", [])
    if not isinstance(data, list):
        raise ValueError("asset-records 必须是数组或包含 records 数组的对象")
    return {str(record.get("canonical_asset_id", "")): record for record in data if isinstance(record, dict)}


def validate_file(
    descriptor_path: Path,
    asset_records: dict[str, dict[str, Any]] | None,
) -> dict[str, Any]:
    """Validate a descriptor JSON file (single object or array); returns the result dict."""
    try:
        data = json.loads(descriptor_path.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError) as exc:
        return {
            "task": TASK, "tool": TOOL, "read_only": True, "status": "BLOCKED",
            "descriptor_count": 0, "p0_count": 1, "p1_count": 0,
            "error_counts": {"descriptor_json_invalid": 1},
            "findings": [_finding("<descriptor>", "<descriptor>", "descriptor_json_invalid", "P0", f"描述器文件不可读或非 JSON：{exc}")],
        }
    descriptors = data if isinstance(data, list) else [data]
    findings: list[dict[str, str]] = []
    for descriptor in descriptors:
        findings.extend(validate_descriptor(descriptor, asset_records))
    p0 = [f for f in findings if f["severity"] == "P0"]
    p1 = [f for f in findings if f["severity"] == "P1"]
    error_counts: dict[str, int] = {}
    for finding in findings:
        error_counts[finding["rule"]] = error_counts.get(finding["rule"], 0) + 1
    findings.sort(key=lambda f: (f["source_asset_id"], f["field"], f["rule"]))
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": "BLOCKED" if p0 else "PASS",
        "descriptor_count": len(descriptors),
        "p0_count": len(p0),
        "p1_count": len(p1),
        "error_counts": {rule: error_counts[rule] for rule in sorted(error_counts)},
        "findings": findings,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--descriptor", type=Path, required=True, help="导入描述器 JSON（单个或数组）")
    parser.add_argument("--asset-records", type=Path, default=None, help="可选：L02 资源记录 JSON（数组或 {records:[...]}），用于 source_asset_id 解析与类别匹配")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        asset_records = load_asset_records(args.asset_records)
    except (json.JSONDecodeError, OSError, ValueError) as exc:
        result = {
            "task": TASK, "tool": TOOL, "read_only": True, "status": "BLOCKED",
            "descriptor_count": 0, "p0_count": 1, "p1_count": 0,
            "error_counts": {"asset_records_invalid": 1},
            "findings": [_finding("<asset-records>", "<asset-records>", "asset_records_invalid", "P0", f"资源记录不可读或格式非法：{exc}")],
        }
    else:
        result = validate_file(args.descriptor, asset_records)
        result["descriptor"] = str(args.descriptor).replace("\\", "/")
        result["asset_records"] = str(args.asset_records).replace("\\", "/") if args.asset_records else None
        result["project_root"] = str(args.project_root.resolve()).replace("\\", "/")
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
