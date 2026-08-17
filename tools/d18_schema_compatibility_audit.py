"""D18-L45 read-only schema compatibility audit.

Checks that the D18 schemas, catalog compiler, validators, aggregator audit and
result JSONs agree on field names, state enums, versions and error codes, and
detects semantic splits where one tool accepts what another tool rejects.

Slot comparison (each slot lists its sources; all sources must agree):

- ``state_enum``        L02 schema / L39 aggregator / L42 compiler / L18 guard
- ``category_enum``     L02 schema / L42 compiler / L16 resource keys / L25
- ``schema_version``    L25 / L37 schemas / L42 compiler constant
- ``record_required_fields``  L02 schema / L42 compiler
- ``result_status_enum``      sync_logic_task_status / result JSON files
- ``result_required_fields``  LOGIC_TASK_PROTOCOL / result JSON files

Real-data checks:

- every category used by the D18-L21..L24 source catalogs must be inside the
  frozen category enum; any extra value is a semantic split (P0 blocker);
- every delivered ``*_LOGIC_RESULT.json`` must carry the frozen fields, a valid
  status value and a task_id matching its directory;
- cross-tool acceptance: a record that the aggregator reports without gaps but
  the compiler rejects is a semantic split (recorded, never silently fixed).

The tool never rewrites sub-tools; every finding carries an owner task and a
fix direction for later chapters (D24 regression / L50 acceptance).  It is
strictly read-only, offline and deterministic.
"""

from __future__ import annotations

import argparse
import importlib
import json
import re
import sys
from pathlib import Path
from typing import Any

TASK = "D18-L45"
TOOL = "tools/d18_schema_compatibility_audit.py"

VALID_RESULT_STATUS = frozenset(
    {"PLANNED", "IN_PROGRESS", "LOGIC_PASS", "LOGIC_PARTIAL", "LOGIC_BLOCKED", "RUNTIME_DEFERRED"}
)
RESULT_REQUIRED_FIELDS = (
    "task_id", "status", "updated_at", "summary", "changed_files",
    "verification", "runtime_deferred", "blockers", "handoff",
)

L02_SCHEMA = "tools/schemas/d18_logic_asset_record.schema.json"
L25_SCHEMA = "tools/schemas/d18_import_descriptor.schema.json"
L37_SCHEMA = "tools/schemas/d18_runtime_promotion_request.schema.json"
SOURCES_DIR = "docs/game/design/common/assets/sources"
RESULTS_DIR = "game/tools/results/D18"

CATEGORY_SPLIT_OWNERS = (
    "L02（或 L11—L14 分类词典）与 L21—L24（来源目录）必须先定权威 category 映射；"
    "L42 已按枚举如实拒绝（正确），L39 聚合未校验 category（接受）造成交叉语义分裂。"
)


# ---------------------------------------------------------------------------
# slot extraction
# ---------------------------------------------------------------------------


def _load_json(path: Path) -> dict[str, Any]:
    return json.loads(path.read_text(encoding="utf-8"))


def _schema_slot(schema: dict[str, Any], key: str, sub: str | None = None) -> Any:
    value = schema.get("properties", {}).get(key)
    if sub and isinstance(value, dict):
        value = value.get(sub)
    return value


def collect_sources(project_root: Path) -> dict[str, dict[str, Any]]:
    """Collect every frozen semantic value from schemas and modules."""
    root = project_root.resolve()
    l02 = _load_json(root / L02_SCHEMA)
    l25 = _load_json(root / L25_SCHEMA)
    l37 = _load_json(root / L37_SCHEMA)

    sys.path.insert(0, str(root / "tools"))
    l39 = importlib.import_module("d18_logic_audit")
    l42 = importlib.import_module("d18_logic_catalog_compiler")
    l16 = importlib.import_module("d18_resource_key_validator")
    l18 = importlib.import_module("d18_release_claim_guard")
    sync = importlib.import_module("sync_logic_task_status")

    return {
        "state_enum": {
            "expected": None,
            "sources": {
                "L02 schema": sorted(_schema_slot(l02, "state", "enum")),
                "L39 aggregator": sorted(l39.VALID_STATES),
                "L42 compiler": sorted(l42.VALID_STATES),
                "L18 guard": sorted(l18.STATE_NAMES),
            },
        },
        "category_enum": {
            "expected": None,
            "sources": {
                "L02 schema": sorted(_schema_slot(l02, "category", "enum")),
                "L42 compiler": sorted(l42.VALID_CATEGORIES),
                "L16 resource keys": sorted(l16.CATEGORY_KEYS),
                "L25 descriptor": sorted(_schema_slot(l25, "type", "enum")),
            },
        },
        "schema_version": {
            "expected": None,
            "sources": {
                "L25 schema": _schema_slot(l25, "schema_version", "const"),
                "L37 schema": _schema_slot(l37, "schema_version", "const"),
                "L42 compiler": l42.SCHEMA_VERSION,
            },
        },
        "record_required_fields": {
            "expected": None,
            "sources": {
                "L02 schema": sorted(l02["required"]),
                "L42 compiler": sorted(l42.REQUIRED_RECORD_FIELDS),
            },
        },
        "result_status_enum": {
            "expected": None,
            "sources": {
                "sync tool": sorted(sync.VALID),
            },
        },
    }


def check_slots(slots: dict[str, dict[str, Any]]) -> tuple[dict[str, Any], list[dict[str, str]]]:
    """Compare each slot; returns (slots result, split findings)."""
    findings: list[dict[str, str]] = []
    checked: dict[str, Any] = {}
    for slot, item in slots.items():
        sources = item["sources"]
        values = {name: sorted(value) if isinstance(value, (list, set, frozenset)) else value for name, value in sources.items()}
        unique = {json.dumps(value, ensure_ascii=False) for value in values.values()}
        consistent = len(unique) <= 1
        checked[slot] = {
            "consistent": consistent,
            "source_values": values,
        }
        if not consistent:
            findings.append({
                "rule": "slot_inconsistent",
                "severity": "P0",
                "slot": slot,
                "message": f"语义槽 {slot} 不一致：{ {k: v for k, v in values.items()} }",
                "owner_task": "D18-L45 记录；按基线 §6 冻结词典统一（不得批量重写子工具）",
                "fix_direction": "在权威词典中定值后，由 L50 验收或 D24 回归统一",
            })
    return checked, findings


# ---------------------------------------------------------------------------
# real-data checks
# ---------------------------------------------------------------------------


def check_source_categories(project_root: Path, category_enum: set[str]) -> tuple[dict[str, Any], list[dict[str, str]]]:
    root = project_root.resolve()
    used: set[str] = set()
    per_file: dict[str, list[str]] = {}
    for path in sorted((root / SOURCES_DIR).glob("d18_*_logic_catalog.json")):
        data = _load_json(path)
        records = data.get("records", [])
        per_file[path.name] = sorted({str(record.get("category")) for record in records if isinstance(record, dict)})
        used.update(per_file[path.name])
    extra = sorted(used - category_enum)
    findings: list[dict[str, str]] = []
    if extra:
        findings.append({
            "rule": "category_split_real_data",
            "severity": "P0",
            "slot": "category_enum",
            "message": f"来源目录实际使用 {sorted(used)}，其中 {extra} 不在冻结枚举（{sorted(category_enum)}）",
            "owner_task": CATEGORY_SPLIT_OWNERS,
            "fix_direction": "先定权威映射（L02 枚举扩充或 L21—L24 改细分类为枚举值），再统一 L39 校验；本轮只记录不重写",
        })
    return {"categories_used": sorted(used), "extra_categories": extra, "per_file": per_file}, findings


def check_result_jsons(project_root: Path) -> tuple[dict[str, Any], list[dict[str, str]]]:
    root = project_root.resolve()
    findings: list[dict[str, str]] = []
    files = sorted((root / RESULTS_DIR).glob("D18-L*/D18_L*_LOGIC_RESULT.json"))
    missing_fields: list[str] = []
    bad_status: list[str] = []
    bad_task_id: list[str] = []
    unreadable = 0
    for path in files:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            unreadable += 1
            continue
        for field in RESULT_REQUIRED_FIELDS:
            if field not in data:
                missing_fields.append(f"{path.name}:{field}")
        if data.get("status") not in VALID_RESULT_STATUS:
            bad_status.append(f"{path.name}:{data.get('status')!r}")
        expected_id = path.name.replace("_LOGIC_RESULT.json", "").replace("_", "-")
        if data.get("task_id") != expected_id:
            bad_task_id.append(f"{path.name}:{data.get('task_id')!r}")
    if missing_fields:
        findings.append({
            "rule": "result_json_missing_field", "severity": "P0", "slot": "result_required_fields",
            "message": f"{len(missing_fields)} 处缺必需字段：{missing_fields[:5]}", "owner_task": "对应结果任务",
            "fix_direction": "按 LOGIC_TASK_PROTOCOL 固定格式补齐",
        })
    if bad_status:
        findings.append({
            "rule": "result_json_bad_status", "severity": "P0", "slot": "result_status_enum",
            "message": f"非法 status：{bad_status[:5]}", "owner_task": "对应结果任务",
            "fix_direction": "只使用冻结六状态",
        })
    if bad_task_id:
        findings.append({
            "rule": "result_json_task_id_mismatch", "severity": "P0", "slot": "result_task_id",
            "message": f"task_id 与目录不匹配：{bad_task_id[:5]}", "owner_task": "对应结果任务",
            "fix_direction": "task_id 必须等于 Dxx-Lnn",
        })
    return {
        "file_count": len(files),
        "unreadable_count": unreadable,
        "missing_field_count": len(missing_fields),
        "bad_status_count": len(bad_status),
        "bad_task_id_count": len(bad_task_id),
    }, findings


# ---------------------------------------------------------------------------
# cross-tool acceptance split (L39 accepts what L42 rejects)
# ---------------------------------------------------------------------------


def check_cross_tool_acceptance(project_root: Path, category_enum: set[str]) -> list[dict[str, str]]:
    """Detect records accepted without gaps by the aggregator but rejected by
    the compiler (semantic split), without re-running either tool."""
    root = project_root.resolve()
    findings: list[dict[str, str]] = []
    for path in sorted((root / SOURCES_DIR).glob("d18_*_logic_catalog.json")):
        data = _load_json(path)
        for record in data.get("records", []):
            if isinstance(record, dict) and record.get("category") not in category_enum:
                findings.append({
                    "rule": "cross_tool_acceptance_split",
                    "severity": "P0",
                    "slot": "category_enum",
                    "message": f"{record.get('canonical_asset_id')}（category={record.get('category')!r}）："
                               f"L39 聚合无缺口（接受）而 L42 编译器拒绝（rejected）",
                    "owner_task": CATEGORY_SPLIT_OWNERS,
                    "fix_direction": "权威映射确定后统一两工具语义；本轮只记录",
                })
                break  # 每个文件记录一次即可
    return findings


# ---------------------------------------------------------------------------
# orchestrator
# ---------------------------------------------------------------------------


def audit_schema_compatibility(project_root: Path, overrides: dict[str, dict[str, Any]] | None = None) -> dict[str, Any]:
    """Run the full compatibility audit; overrides inject fixture slots for tests."""
    root = project_root.resolve()
    if overrides is not None:
        slots = overrides
    else:
        slots = collect_sources(root)

    checked, findings = check_slots(slots)
    category_enum = set(slots["category_enum"]["sources"]["L02 schema"])
    real_categories, category_findings = check_source_categories(root, category_enum)
    result_summary, result_findings = check_result_jsons(root)
    cross_findings = [] if overrides is not None else check_cross_tool_acceptance(root, category_enum)

    all_findings = findings + category_findings + result_findings + cross_findings
    p0 = [f for f in all_findings if f["severity"] == "P0"]
    return {
        "task": TASK,
        "tool": TOOL,
        "read_only": True,
        "status": "BLOCKED" if p0 else "PASS",
        "slots": checked,
        "real_data": {
            "source_categories": real_categories,
            "result_jsons": result_summary,
        },
        "semantic_splits": all_findings,
        "summary": {
            "slot_count": len(checked),
            "consistent_slots": sum(1 for item in checked.values() if item["consistent"]),
            "p0_findings": len(p0),
            "finding_count": len(all_findings),
        },
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    result = audit_schema_compatibility(args.project_root.resolve())
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0 if result["status"] == "PASS" else 2


if __name__ == "__main__":
    raise SystemExit(main())
