"""D18-L39 read-only logic audit aggregator.

Aggregates the schemas and validators delivered by D18-L02..L38 into one
repeatable entry point.  It never rewrites sub-tools, catalogs, Unity or
resources; it is fully offline and read-only.

Three read-only checks are combined into one machine-readable JSON result:

1. delivery completeness: every frozen deliverable (schema / validator / audit
   / catalog / taxonomy / guard / document) must exist, schemas and catalogs
   must be valid JSON, and validator modules must import with their frozen
   entry callable present.  A missing interface is BLOCKED with the task id
   listed - sub-tool absence is never forged into PASS.
2. fixed-fixture smoke: each validator/guard/audit entry is invoked with a
   minimal built-in fixture (records, descriptors, lineage or empty scan).
   An entry must return a machine-readable result without raising; a raised
   error is reported (never swallowed) and blocks the whole audit.
3. read-only aggregation: every record of the supplied static catalogs
   (default ``docs/game/design/common/assets/sources/*.json``) is summarised
   per record with field coverage and P0/P1 gaps, plus state-transition /
   path / license issues and totals.  Approval states (RuntimeApproved /
   ReleaseApproved) inside the logic phase are always blocked (they must be 0).

The tool writes nothing to Unity, assets, catalogs or the repository tree; the
optional ``--output`` only writes its own machine-readable result JSON.
"""

from __future__ import annotations

import argparse
import importlib
import json
import re
import sys
from dataclasses import dataclass, field
from datetime import date
from pathlib import Path
from typing import Any, Callable

TASK = "D18-L39"
TOOL = "tools/d18_logic_audit.py"

VALID_STATES = frozenset(
    {"Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved", "Rejected", "Revoked"}
)
APPROVAL_STATES_IN_LOGIC = ("RuntimeApproved", "ReleaseApproved")
# 双层分类契约（D18-L51）：category 永远是 L02 的 11 值资源大类；
# taxonomy_category 为可选细分类，取值必须来自 d18_taxonomy_category_map.json 且父类匹配。
VALID_CATEGORIES = frozenset(
    {"animal", "audio", "building", "character", "environment", "fish",
     "lighting_reference", "material", "prop", "ship", "ui"}
)
TAXONOMY_MAP_PATH = "tools/schemas/d18_taxonomy_category_map.json"

REPO_PATH_RE = re.compile(r"^[A-Za-z0-9_.][A-Za-z0-9_./-]*$")
TRAVERSAL_RE = re.compile(r"(^|/)\.\.(/|$)")
HTTPS_URL_RE = re.compile(r"^https://[^\s]+$")
NC_OR_UNKNOWN_LICENSE = re.compile(r"nc|noncommercial|non-commercial|unknown|conflict", re.IGNORECASE)

_TAXONOMY_MAP: dict[str, str] | None = None


def load_taxonomy_map(root: Path) -> dict[str, str]:
    """Load {taxonomy_category: parent_category} from the frozen map (deterministic)."""
    global _TAXONOMY_MAP
    if _TAXONOMY_MAP is None:
        path = root.resolve() / TAXONOMY_MAP_PATH
        data = json.loads(path.read_text(encoding="utf-8"))
        entries = data.get("entries", {})
        _TAXONOMY_MAP = {
            str(key): str(entry.get("parent_category"))
            for key, entry in entries.items()
            if isinstance(entry, dict) and entry.get("parent_category")
        }
    return _TAXONOMY_MAP


# ---------------------------------------------------------------------------
# fixtures
# ---------------------------------------------------------------------------


def _min_record() -> dict[str, Any]:
    """Minimal L02-legal asset record grounded in the real Kenney UI pack."""
    return {
        "canonical_asset_id": "asset_kenney_ui_pack_v2",
        "category": "ui",
        "source_id": "source_kenney",
        "source_url": "https://kenney.nl/assets/ui-pack",
        "license": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/License.txt",
            "attribution_required": False,
        },
        "rights": {
            "commercial_use": True,
            "modification_allowed": True,
            "repository_storage": True,
            "distribution_with_game": True,
        },
        "download_date": "2026-08-14",
        "original_filename": "kenney_ui-pack.zip",
        "sha256": "A8A14A234911EB648C062622915C93E79E94E97CB7F9F375A70F6617F1174318",
        "state": "Candidate",
        "content_ids": ["candidate_ui_kenney_button_rectangle_border"],
        "evidence_paths": ["game/Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/License.txt"],
        "runtime_deferred": ["L39 smoke fixture"],
    }


def _min_descriptor() -> dict[str, Any]:
    """Minimal D18-L25 import descriptor (character shape) for smoke runs."""
    return {
        "descriptor_kind": "d18_import_descriptor",
        "schema_version": 1,
        "source_asset_id": "asset_kenney_ui_pack_v2",
        "type": "character",
        "units": "meters",
        "axes": {"up": "y", "forward": "z"},
        "bounds": {"width_m": 1.0, "depth_m": 1.0, "height_m": 1.8},
        "material_slots": [{"slot_name": "body", "slot_role": "body"}],
        "skeleton_type": "humanoid",
        "animation_list": [{"animation_key": "idle", "role": "idle"}],
        "root_motion": "none",
        "collision_intent": "capsule_agent",
        "lod_intent": {"levels": 3, "generate_automatically": True},
        "license_dependency": {
            "license_id": "CC0-1.0",
            "license_url": "https://creativecommons.org/publicdomain/zero/1.0/",
            "license_evidence_path": "game/Assets/Art/Candidates/UIMaterialsAudio/UI/KenneyUiPack/License.txt",
            "attribution_required": False,
        },
        "fallback": {"reference": "placeholder_ph_character_a", "description": "keep placeholder until Runtime import"},
        "represents_imported_asset": False,
    }


def _min_lineage_catalog() -> dict[str, Any]:
    """Minimal D18-L38 lineage catalog (original + derived) for smoke runs."""
    return {
        "lineage_kind": "d18_generated_asset_lineage",
        "schema_version": 1,
        "records": [
            {
                "canonical_asset_id": "asset_kenney_ui_pack_v2",
                "origin_kind": "original",
                "parent_asset_id": None,
                "conversion_steps": None,
                "project_authored": None,
                "third_party_claim": {
                    "source_id": "source_kenney",
                    "source_url": "https://kenney.nl/assets/ui-pack",
                    "license_id": "CC0-1.0",
                },
                "license_basis": None,
                "evidence_paths": [],
                "external_parent": None,
            }
        ],
    }


# ---------------------------------------------------------------------------
# frozen delivery registry (D18-L02..L38)
# ---------------------------------------------------------------------------


@dataclass
class Entry:
    task_id: str
    kind: str
    deliverable: str
    module: str | None = None
    entry: str | None = None
    test: str | None = None
    smoke: Callable[[Any, Path], Any] | None = None


def _records_smoke(fn_name: str) -> Callable[[Any, Path], Any]:
    def run(module: Any, root: Path) -> Any:
        fn = getattr(module, fn_name)
        return fn([_min_record()])
    return run


def _records_root_smoke(fn_name: str) -> Callable[[Any, Path], Any]:
    def run(module: Any, root: Path) -> Any:
        return getattr(module, fn_name)(root, [_min_record()])
    return run


def _records_root_asof_smoke(module: Any, root: Path) -> Any:
    return module.validate_records([_min_record()], root, date.today())


def _records_root_manifests_smoke(module: Any, root: Path) -> Any:
    return module.audit_records(root, [_min_record()])


def _manifest_boundaries_smoke(module: Any, root: Path) -> Any:
    manifest = root / "docs/game/design/common/assets/packs/ui_materials_audio/kenney_ui_v2.manifest.json"
    return module.validate_pack_boundaries(root, [manifest], None)


def _descriptor_file_smoke(module: Any, root: Path) -> Any:
    # 直接调用 validate_descriptor（不落盘），保持零目录写入。
    return module.validate_descriptor(_min_descriptor(), None)


def _descriptors_root_smoke(module: Any, root: Path) -> Any:
    return module.validate_descriptors(root, [])


def _descriptors_empty_smoke(module: Any, root: Path) -> Any:
    return module.validate_descriptors([], root, None)


def _ui_audio_empty_smoke(module: Any, root: Path) -> Any:
    return module.validate_ui_audio_descriptors(root, [])


def _graph_smoke(module: Any, root: Path) -> Any:
    return module.build_graph([_min_record()])


def _scan_smoke(module: Any, root: Path) -> Any:
    config = module.ScanConfig(project_root=root, roots=[], extensions={".cs"}, exclude_dirs=set())
    return module.scan_project(config)


def _guard_root_smoke(module: Any, root: Path) -> Any:
    return module.run_guard(root)


def _catalog_smoke(module: Any, root: Path) -> Any:
    return module.check_catalog()


def _claims_smoke(module: Any, root: Path) -> Any:
    return module.audit_claims()


def _transition_smoke(module: Any, root: Path) -> Any:
    matrix = module.load_matrix(None)
    return module.audit_all([_min_record()], matrix, project_root=root)


def _ldr_smoke(module: Any, root: Path) -> Any:
    return module.evaluate_records([_min_record()], {}, root)


def _lineage_smoke(module: Any, root: Path) -> Any:
    return module.validate_lineage_catalog(_min_lineage_catalog(), root)


REGISTRY: list[Entry] = [
    Entry("D18-L02", "schema", "tools/schemas/d18_logic_asset_record.schema.json", test="tools/tests/d18_logic_asset_record_fixtures/"),
    Entry("D18-L03", "document", "docs/game/design/common/assets/D18_ASSET_STATE_MACHINE.md"),
    Entry("D18-L04", "validator", "tools/d18_license_normalizer.py", "d18_license_normalizer", "normalize_license", "tools/tests/test_d18_license_normalizer.py", _records_smoke("normalize_license")),
    Entry("D18-L05", "validator", "tools/d18_provenance_validator.py", "d18_provenance_validator", "validate_records", "tools/tests/test_d18_provenance_validator.py", _records_root_asof_smoke),
    Entry("D18-L06", "validator", "tools/d18_hash_validator.py", "d18_hash_validator", "validate_records", "tools/tests/test_d18_hash_validator.py", _records_root_smoke("validate_records")),
    Entry("D18-L07", "validator", "tools/d18_distribution_rights_validator.py", "d18_distribution_rights_validator", "build_report", "tools/tests/test_d18_distribution_rights_validator.py", lambda m, r: m.build_report(r, [_min_record()], {})),
    Entry("D18-L08", "validator", "tools/d18_asset_path_validator.py", "d18_asset_path_validator", "check_catalog", "tools/tests/test_d18_asset_path_validator.py", _catalog_smoke),
    Entry("D18-L09", "validator", "tools/d18_attribution_validator.py", "d18_attribution_validator", "validate_attribution_records", "tools/tests/test_d18_attribution_validator.py", _records_smoke("validate_attribution_records")),
    Entry("D18-L10", "validator", "tools/d18_package_boundary_validator.py", "d18_package_boundary_validator", "validate_pack_boundaries", "tools/tests/test_d18_package_boundary_validator.py", _manifest_boundaries_smoke),
    Entry("D18-L11", "document", "docs/game/design/common/assets/D18_TAXONOMY_SHIPS_PORT.md"),
    Entry("D18-L12", "document", "docs/game/design/common/assets/D18_TAXONOMY_CHARACTERS.md"),
    Entry("D18-L13", "document", "docs/game/design/common/assets/D18_TAXONOMY_ECOLOGY.md"),
    Entry("D18-L14", "document", "docs/game/design/common/assets/D18_TAXONOMY_UI_AUDIO.md"),
    Entry("D18-L15", "validator", "tools/d18_content_id_asset_link_validator.py", "d18_content_id_asset_link_validator", "validate_records", "tools/tests/test_d18_content_id_asset_link_validator.py", _records_smoke("validate_records")),
    Entry("D18-L16", "validator", "tools/d18_resource_key_validator.py", "d18_resource_key_validator", "validate_records", "tools/tests/test_d18_resource_key_validator.py", _records_smoke("validate_records")),
    Entry("D18-L17", "guard", "tools/d18_candidate_runtime_guard.py", "d18_candidate_runtime_guard", "scan_project", "tools/tests/test_d18_candidate_runtime_guard.py", _scan_smoke),
    Entry("D18-L18", "guard", "tools/d18_release_claim_guard.py", "d18_release_claim_guard", "run_guard", "tools/tests/test_d18_release_claim_guard.py", _guard_root_smoke),
    Entry("D18-L19", "document", "docs/game/design/common/assets/D18_WAIVER_AND_REJECTION_RULES.md"),
    Entry("D18-L20", "document", "docs/game/design/common/assets/D18_LOGIC_RESOURCE_INDEX.md"),
    Entry("D18-L21", "catalog", "docs/game/design/common/assets/sources/d18_kenney_logic_catalog.json"),
    Entry("D18-L22", "catalog", "docs/game/design/common/assets/sources/d18_quaternius_logic_catalog.json"),
    Entry("D18-L23", "catalog", "docs/game/design/common/assets/sources/d18_polyhaven_logic_catalog.json"),
    Entry("D18-L24", "catalog", "docs/game/design/common/assets/sources/d18_ambientcg_logic_catalog.json"),
    Entry("D18-L25", "schema", "tools/schemas/d18_import_descriptor.schema.json"),
    Entry("D18-L26", "validator", "tools/d18_mesh_descriptor_validator.py", "d18_mesh_descriptor_validator", "validate_descriptor", "tools/tests/test_d18_mesh_descriptor_validator.py", _descriptor_file_smoke),
    Entry("D18-L27", "validator", "tools/d18_material_descriptor_validator.py", "d18_material_descriptor_validator", "validate_descriptors", "tools/tests/test_d18_material_descriptor_validator.py", _descriptors_empty_smoke),
    Entry("D18-L28", "validator", "tools/d18_animation_descriptor_validator.py", "d18_animation_descriptor_validator", "validate_descriptors", "tools/tests/test_d18_animation_descriptor_validator.py", _descriptors_root_smoke),
    Entry("D18-L29", "validator", "tools/d18_ui_audio_descriptor_validator.py", "d18_ui_audio_descriptor_validator", "validate_ui_audio_descriptors", "tools/tests/test_d18_ui_audio_descriptor_validator.py", _ui_audio_empty_smoke),
    Entry("D18-L30", "audit", "tools/d18_asset_dependency_graph.py", "d18_asset_dependency_graph", "build_graph", "tools/tests/test_d18_asset_dependency_graph.py", _graph_smoke),
    Entry("D18-L31", "audit", "tools/d18_duplicate_identity_audit.py", "d18_duplicate_identity_audit", "audit_records", "tools/tests/test_d18_duplicate_identity_audit.py", _records_root_manifests_smoke),
    Entry("D18-L32", "audit", "tools/d18_name_collision_audit.py", "d18_name_collision_audit", "audit_records", "tools/tests/test_d18_name_collision_audit.py", _records_smoke("audit_records")),
    Entry("D18-L33", "audit", "tools/d18_catalog_claim_audit.py", "d18_catalog_claim_audit", "audit_claims", "tools/tests/test_d18_catalog_claim_audit.py", _claims_smoke),
    Entry("D18-L34", "audit", "tools/d18_missing_evidence_audit.py", "d18_missing_evidence_audit", "audit_records", "tools/tests/test_d18_missing_evidence_audit.py", _records_smoke("audit_records")),
    Entry("D18-L35", "audit", "tools/d18_status_transition_audit.py", "d18_status_transition_audit", "audit_all", "tools/tests/test_d18_status_transition_audit.py", _transition_smoke),
    Entry("D18-L36", "guard", "tools/d18_local_development_guard.py", "d18_local_development_guard", "evaluate_records", "tools/tests/test_d18_local_development_guard.py", _ldr_smoke),
    Entry("D18-L37", "schema", "tools/schemas/d18_runtime_promotion_request.schema.json"),
    Entry("D18-L38", "guard", "tools/d18_generated_asset_guard.py", "d18_generated_asset_guard", "validate_lineage_catalog", "tools/tests/test_d18_generated_asset_guard.py", _lineage_smoke),
]


# ---------------------------------------------------------------------------
# delivery completeness
# ---------------------------------------------------------------------------


def check_deliverables(root: Path, registry: list[Entry] | None = None) -> dict[str, Any]:
    registry = registry if registry is not None else REGISTRY
    entries: list[dict[str, Any]] = []
    missing_count = 0
    for entry in registry:
        item: dict[str, Any] = {
            "task_id": entry.task_id,
            "kind": entry.kind,
            "deliverable": entry.deliverable,
            "present": True,
            "json_valid": None,
            "importable": None,
            "entry_present": None,
            "error": None,
        }
        path = root / entry.deliverable
        if not path.is_file():
            item["present"] = False
            item["error"] = "deliverable 缺失"
            missing_count += 1
            entries.append(item)
            continue
        if entry.deliverable.endswith(".json"):
            try:
                json.loads(path.read_text(encoding="utf-8"))
                item["json_valid"] = True
            except (json.JSONDecodeError, OSError) as exc:
                item["json_valid"] = False
                item["error"] = f"JSON 无效：{exc}"
                missing_count += 1
        entries.append(item)
    return {"entries": entries, "total": len(registry), "missing_count": missing_count}


# ---------------------------------------------------------------------------
# fixed-fixture smoke
# ---------------------------------------------------------------------------


def run_smoke(root: Path, registry: list[Entry] | None = None) -> dict[str, Any]:
    registry = registry if registry is not None else REGISTRY
    tools_dir = root / "tools"
    if str(tools_dir) not in sys.path:
        sys.path.insert(0, str(tools_dir))
    results: list[dict[str, Any]] = []
    failed = 0
    for entry in registry:
        item: dict[str, Any] = {"task_id": entry.task_id, "module": entry.module, "entry": entry.entry, "smoke": "SKIP", "error": None}
        if entry.module is None:
            results.append(item)
            continue
        try:
            module = importlib.import_module(entry.module)
            if entry.entry is not None and not hasattr(module, entry.entry):
                raise AttributeError(f"entry {entry.entry!r} 不存在")
            if entry.smoke is None:
                results.append(item)
                continue
            returned = entry.smoke(module, root)
            if isinstance(returned, (dict, list)):
                item["smoke"] = "PASS"
            else:
                raise TypeError(f"入口返回类型不可用：{type(returned).__name__}")
        except Exception as exc:  # 不吞子工具错误：如实上报并 BLOCK
            item["smoke"] = "FAIL"
            item["error"] = f"{type(exc).__name__}: {exc}"
            failed += 1
        results.append(item)
    return {"results": results, "total": len(registry), "failed_count": failed}


# ---------------------------------------------------------------------------
# read-only aggregation of static catalogs
# ---------------------------------------------------------------------------


def _gap(record_id: str, field: str, rule: str, severity: str, message: str) -> dict[str, str]:
    return {"record_id": record_id, "field": field, "rule": rule, "severity": severity, "message": message}


def aggregate_record(
    record: dict[str, Any],
    taxonomy_map: dict[str, str] | None = None,
) -> tuple[dict[str, Any], list[dict[str, str]]]:
    record_id = str(record.get("canonical_asset_id", "<unknown>"))
    gaps: list[dict[str, str]] = []

    state = record.get("state")
    if state not in VALID_STATES:
        gaps.append(_gap(record_id, "state", "state_invalid", "P0", f"非法状态 {state!r}"))
    elif state in APPROVAL_STATES_IN_LOGIC:
        gaps.append(_gap(record_id, "state", "approval_state_in_logic_phase", "P0", f"本逻辑阶段禁止 {state}（应保持 0 个）"))

    # 双层分类契约：category 必须是 11 值大类（与 L42 同语义）；
    # taxonomy_category 必须存在于映射且父类等于 category。
    category = record.get("category")
    if category not in VALID_CATEGORIES:
        gaps.append(_gap(record_id, "category", "category_invalid", "P0", f"category 非法大类：{category!r}（必须为 L02 11 值枚举）"))
    taxonomy_category = record.get("taxonomy_category")
    if taxonomy_category is not None:
        if taxonomy_map is None:
            taxonomy_map = _TAXONOMY_MAP
        if taxonomy_map is None or taxonomy_category not in taxonomy_map:
            gaps.append(_gap(record_id, "taxonomy_category", "unknown_taxonomy_category", "P0", f"未知细分类：{taxonomy_category!r}（不在 d18_taxonomy_category_map.json）"))
        elif taxonomy_map[taxonomy_category] != category:
            gaps.append(_gap(record_id, "taxonomy_category", "taxonomy_parent_mismatch", "P0", f"细分类 {taxonomy_category!r} 的父类 {taxonomy_map[taxonomy_category]!r} 与 category={category!r} 不匹配"))

    license_obj = record.get("license")
    license_id = license_obj.get("license_id") if isinstance(license_obj, dict) else None
    if not license_id:
        gaps.append(_gap(record_id, "license.license_id", "missing_license", "P0", "缺许可证 ID"))
    elif NC_OR_UNKNOWN_LICENSE.search(license_id):
        gaps.append(_gap(record_id, "license.license_id", "license_suspect", "P0", f"许可疑似 NC/未知/冲突：{license_id}"))

    rights = record.get("rights")
    if not isinstance(rights, dict) or not rights:
        gaps.append(_gap(record_id, "rights", "missing_rights", "P0", "缺四项权利记录"))

    content_ids = record.get("content_ids")
    if not isinstance(content_ids, list) or not content_ids:
        gaps.append(_gap(record_id, "content_ids", "missing_content_mapping", "P0", "缺内容映射"))

    evidence = record.get("evidence_paths")
    if not isinstance(evidence, list) or not evidence:
        gaps.append(_gap(record_id, "evidence_paths", "missing_evidence", "P0", "缺证据路径"))
    else:
        for path in evidence:
            if not isinstance(path, str) or TRAVERSAL_RE.search(path) or "\\" in path or not REPO_PATH_RE.fullmatch(path):
                gaps.append(_gap(record_id, "evidence_paths", "path_escape", "P0", f"证据路径越界：{path!r}"))

    source_url = record.get("source_url")
    if not isinstance(source_url, str) or not HTTPS_URL_RE.fullmatch(source_url):
        gaps.append(_gap(record_id, "source_url", "missing_source", "P0", "缺 https 官方来源 URL"))

    sha256 = record.get("sha256")
    if not isinstance(sha256, str) or len(sha256) != 64:
        gaps.append(_gap(record_id, "sha256", "hash_pending", "P1", "SHA-256 缺失或格式无效（Pending，不批准）"))

    summary = {
        "record_id": record_id,
        "category": category,
        "taxonomy_category": taxonomy_category,
        "state": state,
        "license_id": license_id,
        "sha256_present": bool(isinstance(sha256, str) and len(sha256) == 64),
        "source_url_present": bool(isinstance(source_url, str) and HTTPS_URL_RE.fullmatch(source_url)),
        "content_ids": len(content_ids) if isinstance(content_ids, list) else 0,
        "evidence_paths": len(evidence) if isinstance(evidence, list) else 0,
        "gap_count": len(gaps),
    }
    return summary, gaps


def aggregate_catalogs(root: Path, catalog_paths: list[Path], taxonomy_map: dict[str, str] | None = None) -> dict[str, Any]:
    records_summary: list[dict[str, Any]] = []
    gaps: list[dict[str, str]] = []
    catalog_files: list[str] = []
    record_count = 0
    if taxonomy_map is None:
        taxonomy_map = load_taxonomy_map(root)
    for catalog_path in catalog_paths:
        if not catalog_path.is_file():
            continue
        catalog_files.append(str(catalog_path).replace("\\", "/"))
        try:
            data = json.loads(catalog_path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError) as exc:
            gaps.append(_gap(str(catalog_path), "<catalog>", "catalog_json_invalid", "P0", f"目录不可读：{exc}"))
            continue
        records = data.get("records", []) if isinstance(data, dict) else data
        if not isinstance(records, list):
            continue
        for record in records:
            if not isinstance(record, dict):
                continue
            record_count += 1
            summary, record_gaps = aggregate_record(record, taxonomy_map)
            records_summary.append(summary)
            gaps.extend(record_gaps)

    p0 = [g for g in gaps if g["severity"] == "P0"]
    p1 = [g for g in gaps if g["severity"] == "P1"]
    gap_counts: dict[str, int] = {}
    for gap in gaps:
        gap_counts[gap["rule"]] = gap_counts.get(gap["rule"], 0) + 1
    catalog_json_invalid_count = sum(1 for g in gaps if g["rule"] == "catalog_json_invalid")
    return {
        "catalogs": catalog_files,
        "record_count": record_count,
        "records": records_summary,
        "p0_count": len(p0),
        "p1_count": len(p1),
        "catalog_json_invalid_count": catalog_json_invalid_count,
        "gap_counts": {rule: gap_counts[rule] for rule in sorted(gap_counts)},
        "state_issues": [g for g in gaps if g["rule"] in ("state_invalid", "approval_state_in_logic_phase")],
        "path_issues": [g for g in gaps if g["rule"] == "path_escape"],
        "license_issues": [g for g in gaps if g["rule"] in ("missing_license", "license_suspect")],
        "gaps": gaps,
    }


def default_catalog_paths(root: Path) -> list[Path]:
    sources = root / "docs/game/design/common/assets/sources"
    if not sources.is_dir():
        return []
    return sorted(sources.glob("d18_*_logic_catalog.json"))


# ---------------------------------------------------------------------------
# orchestrator
# ---------------------------------------------------------------------------


def run_audit(root: Path, catalog_paths: list[Path] | None = None, registry: list[Entry] | None = None) -> dict[str, Any]:
    root = root.resolve()
    registry = registry if registry is not None else REGISTRY
    deliverables = check_deliverables(root, registry)
    smoke = run_smoke(root, registry)
    catalogs = catalog_paths if catalog_paths is not None else default_catalog_paths(root)
    aggregate = aggregate_catalogs(root, catalogs)

    blocked_reasons: list[str] = []
    if deliverables["missing_count"]:
        missing_ids = [e["task_id"] for e in deliverables["entries"] if not e["present"] or e.get("json_valid") is False]
        blocked_reasons.append(f"交付物缺失/JSON 无效：{missing_ids}")
    if smoke["failed_count"]:
        failed_ids = [r["task_id"] for r in smoke["results"] if r["smoke"] == "FAIL"]
        blocked_reasons.append(f"子规则冒烟失败：{failed_ids}")
    if aggregate["state_issues"]:
        blocked_reasons.append("发现本逻辑阶段禁止的批准状态或非法状态")
    if aggregate["catalog_json_invalid_count"] > 0:
        blocked_reasons.append("存在不可读的输入目录")

    status = "PASS" if not blocked_reasons else "BLOCKED"
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": status,
        "blocked_reasons": blocked_reasons,
        "subtools": {
            "deliverables_total": deliverables["total"],
            "deliverables_missing": deliverables["missing_count"],
            "entries": deliverables["entries"],
        },
        "smoke": {
            "total": smoke["total"],
            "failed_count": smoke["failed_count"],
            "results": smoke["results"],
        },
        "aggregate": aggregate,
        "totals": {
            "records": aggregate["record_count"],
            "gaps_p0": aggregate["p0_count"],
            "gaps_p1": aggregate["p1_count"],
            "smoke_passed": smoke["total"] - smoke["failed_count"],
            "smoke_failed": smoke["failed_count"],
        },
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--catalog", type=Path, action="append", default=[])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    root = args.project_root.resolve()
    if str(root / "tools") not in sys.path:
        sys.path.insert(0, str(root / "tools"))
    catalogs = [item.resolve() for item in args.catalog] or None
    result = run_audit(root, catalogs)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
