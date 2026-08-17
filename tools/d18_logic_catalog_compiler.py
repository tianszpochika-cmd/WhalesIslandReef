"""D18-L42 read-only logic catalog compiler.

Compiles the static per-source logic catalogs delivered by D18-L21..L24
(``docs/game/design/common/assets/sources/d18_*_logic_catalog.json``) into one
canonical logic catalog:

* deterministic: the same inputs always produce the byte-identical output
  (stable sort by ``canonical_asset_id``, fixed field order, no timestamps);
* schema-gated: only Schema-legal records (D18-L02 mirror) enter the compiled
  ``records``; illegal records are rejected with a stable reason;
* Pending/Rejected preserved: records with ``sha256=null`` (pending hash) and
  records in ``Rejected``/``Revoked`` states stay in the catalog; the audit
  summary reports them, nothing is guessed;
* conflict detection on multi-source merge: the same ``canonical_asset_id``
  declared identically by several sources is deduplicated; conflicting
  declarations are excluded from the catalog and listed in ``conflicts``;
* audit summary: input sources/records, compiled, rejected, merged duplicates,
  conflicts, state distribution and pending-hash count.

The compiler never writes Unity Assets, Prefabs, the runtime Registry,
approval states or download caches.  The only writable destination is its own
``--output`` catalog JSON (or stdout); it never grants RuntimeApproved /
ReleaseApproved.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any

TASK = "D18-L42"
TOOL = "tools/d18_logic_catalog_compiler.py"
CATALOG_KIND = "d18_logic_catalog"
SCHEMA_VERSION = 1

DEFAULT_SOURCES_DIR = "docs/game/design/common/assets/sources"

VALID_STATES = frozenset(
    {"Candidate", "IntakeEligible", "RuntimeRequested", "RuntimeApproved", "ReleaseApproved", "Rejected", "Revoked"}
)
VALID_CATEGORIES = frozenset(
    {"animal", "audio", "building", "character", "environment", "fish", "lighting_reference", "material", "prop", "ship", "ui"}
)
TAXONOMY_MAP_PATH = "tools/schemas/d18_taxonomy_category_map.json"
SNAKE_CASE_TAXONOMY_RE = re.compile(r"^[a-z0-9_]+$")

ASSET_ID_RE = re.compile(r"^asset_[a-z0-9_]+$")
SOURCE_ID_RE = re.compile(r"^source_[a-z0-9_]+$")
SNAKE_CASE_RE = re.compile(r"^[a-z0-9_]+$")
HTTPS_URL_RE = re.compile(r"^https://[^\s]+$")
URI_RE = re.compile(r"^https?://[^\s]+$")
DATE_RE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
SHA256_RE = re.compile(r"^[0-9a-fA-F]{64}$")
REPO_PATH_RE = re.compile(r"^[A-Za-z0-9_.][A-Za-z0-9_./-]*$")
TRAVERSAL_RE = re.compile(r"(^|/)\.\.(/|$)")
ORIGINAL_NAME_RE = re.compile(r"^[^/\\:]+$")

REQUIRED_RECORD_FIELDS = (
    "canonical_asset_id", "category", "source_id", "source_url", "license",
    "rights", "download_date", "original_filename", "sha256", "state",
    "content_ids", "evidence_paths", "runtime_deferred",
)


# ---------------------------------------------------------------------------
# schema gate (self-contained D18-L02 mirror; the compiler's own duty)
# ---------------------------------------------------------------------------

_TAXONOMY_MAP: dict[str, str] | None = None


def load_taxonomy_map(project_root: Path) -> dict[str, str]:
    """Load {taxonomy_category: parent_category} from the frozen map (deterministic)."""
    global _TAXONOMY_MAP
    if _TAXONOMY_MAP is None:
        path = project_root.resolve() / TAXONOMY_MAP_PATH
        data = json.loads(path.read_text(encoding="utf-8"))
        entries = data.get("entries", {})
        _TAXONOMY_MAP = {
            str(key): str(entry.get("parent_category"))
            for key, entry in entries.items()
            if isinstance(entry, dict) and entry.get("parent_category")
        }
    return _TAXONOMY_MAP


def _record_schema_check(record: Any, taxonomy_map: dict[str, str] | None = None) -> tuple[bool, str]:
    """Return (ok, reason); ok=True only for Schema-legal records.

    Two-layer category contract (D18-L51): category must be one of the frozen
    11 values; optional taxonomy_category must exist in the map and its parent
    must equal category. Records without taxonomy_category remain legal.
    """
    if not isinstance(record, dict):
        return False, "record 必须是 JSON 对象"
    for field in REQUIRED_RECORD_FIELDS:
        if field not in record:
            return False, f"缺少必需字段 {field}"
    asset_id = record["canonical_asset_id"]
    if not isinstance(asset_id, str) or not ASSET_ID_RE.fullmatch(asset_id):
        return False, "canonical_asset_id 必须匹配 ^asset_[a-z0-9_]+$"
    if record["category"] not in VALID_CATEGORIES:
        return False, f"category 非法：{record['category']!r}"
    taxonomy_category = record.get("taxonomy_category")
    if taxonomy_category is not None:
        if not isinstance(taxonomy_category, str) or not SNAKE_CASE_TAXONOMY_RE.fullmatch(taxonomy_category):
            return False, f"taxonomy_category 必须是 lower snake_case 或 null：{taxonomy_category!r}"
        if taxonomy_map is None or taxonomy_category not in taxonomy_map:
            return False, f"未知细分类：{taxonomy_category!r}（不在 d18_taxonomy_category_map.json）"
        parent = taxonomy_map[taxonomy_category]
        if parent != record["category"]:
            return False, f"细分类 {taxonomy_category!r} 的父类 {parent!r} 与 category={record['category']!r} 不匹配"
    if not isinstance(record["source_id"], str) or not SOURCE_ID_RE.fullmatch(record["source_id"]):
        return False, "source_id 必须匹配 ^source_[a-z0-9_]+$"
    if not isinstance(record["source_url"], str) or not HTTPS_URL_RE.fullmatch(record["source_url"]):
        return False, "source_url 必须是 https URL"
    license_obj = record["license"]
    if not isinstance(license_obj, dict) or not isinstance(license_obj.get("license_id"), str) or not license_obj["license_id"]:
        return False, "license.license_id 缺失或为空"
    rights = record["rights"]
    if not isinstance(rights, dict):
        return False, "rights 必须是对象"
    for field in ("commercial_use", "modification_allowed", "repository_storage", "distribution_with_game"):
        if not isinstance(rights.get(field), bool):
            return False, f"rights.{field} 必须是布尔值"
    download_date = record["download_date"]
    if download_date is not None and (not isinstance(download_date, str) or not DATE_RE.fullmatch(download_date)):
        return False, "download_date 必须是 ISO 日期或 null"
    original_filename = record["original_filename"]
    if not isinstance(original_filename, str) or not ORIGINAL_NAME_RE.fullmatch(original_filename):
        return False, "original_filename 不能为空且不得含路径分隔符"
    sha256 = record["sha256"]
    if sha256 is not None and (not isinstance(sha256, str) or not SHA256_RE.fullmatch(sha256)):
        return False, "sha256 必须是 64 位十六进制或 null"
    if record["state"] not in VALID_STATES:
        return False, f"state 非法：{record['state']!r}"
    content_ids = record["content_ids"]
    if not isinstance(content_ids, list) or not content_ids:
        return False, "content_ids 必须是非空数组"
    for content_id in content_ids:
        if not isinstance(content_id, str) or not SNAKE_CASE_RE.fullmatch(content_id):
            return False, f"content_id 必须是 lower snake_case：{content_id!r}"
    evidence_paths = record["evidence_paths"]
    if not isinstance(evidence_paths, list) or not evidence_paths:
        return False, "evidence_paths 必须是非空数组"
    for path in evidence_paths:
        if not isinstance(path, str) or not REPO_PATH_RE.fullmatch(path) or TRAVERSAL_RE.search(path) or "\\" in path:
            return False, f"evidence 路径越界或格式非法：{path!r}"
    if not isinstance(record["runtime_deferred"], list):
        return False, "runtime_deferred 必须是数组"
    return True, ""


# ---------------------------------------------------------------------------
# compiler
# ---------------------------------------------------------------------------


def _canonical_json(record: dict[str, Any]) -> str:
    return json.dumps(record, sort_keys=True, ensure_ascii=False)


def _load_source(path: Path) -> tuple[list[dict[str, Any]], str | None]:
    """Load one source catalog; returns (records, error)."""
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError) as exc:
        return [], f"来源目录不可读或非 JSON：{exc}"
    if isinstance(data, dict):
        records = data.get("records", [])
    elif isinstance(data, list):
        records = data
    else:
        return [], "来源目录必须是对象（records）或数组"
    if not isinstance(records, list):
        return [], "records 必须是数组"
    return records, None


def compile_catalogs(source_paths: list[Path], taxonomy_map: dict[str, str] | None = None) -> dict[str, Any]:
    """Compile source catalogs into one canonical logic catalog (deterministic)."""
    if taxonomy_map is None:
        taxonomy_map = load_taxonomy_map(Path(__file__).resolve().parents[1])
    sources: list[str] = []
    source_errors: list[dict[str, Any]] = []
    input_records = 0
    rejected: list[dict[str, Any]] = []
    compiled: dict[str, dict[str, Any]] = {}
    conflict_list: list[dict[str, Any]] = []
    merged_duplicates = 0
    conflict_count = 0

    for path in sorted(source_paths):
        records, error = _load_source(path)
        source_label = str(path).replace("\\", "/")
        sources.append(source_label)
        if error:
            source_errors.append({"source": source_label, "error": error})
            continue
        input_records += len(records)
        for record in records:
            ok, reason = _record_schema_check(record, taxonomy_map)
            if not ok:
                rejected.append({
                    "source": source_label,
                    "record_id": record.get("canonical_asset_id") if isinstance(record, dict) else "<invalid>",
                    "reason": reason,
                })
                continue
            asset_id = record["canonical_asset_id"]
            if asset_id not in compiled:
                compiled[asset_id] = record
                continue
            if _canonical_json(compiled[asset_id]) == _canonical_json(record):
                merged_duplicates += 1
                continue
            # 同一 id 声明不一致：冲突，排除并记录。
            previous = compiled.pop(asset_id)
            conflict_count += 1
            conflict_list.append({
                "record_id": asset_id,
                "sources": sorted({source_label, _source_of(previous)}),
                "fields": sorted(_conflicting_fields(previous, record)),
                "declared_versions": 2,
            })

    sorted_records = [compiled[asset_id] for asset_id in sorted(compiled)]
    rejected.sort(key=lambda item: (item["record_id"], item["source"]))
    conflict_list.sort(key=lambda item: item["record_id"])
    state_distribution = dict(sorted(Counter(record["state"] for record in sorted_records).items()))
    pending_hash_records = sum(1 for record in sorted_records if record["sha256"] is None)

    return {
        "catalog_kind": CATALOG_KIND,
        "schema_version": SCHEMA_VERSION,
        "compiled_by": TASK,
        "deterministic": True,
        "input": {
            "sources": sources,
            "source_count": len(sources),
        },
        "audit_summary": {
            "input_records": input_records,
            "compiled_records": len(sorted_records),
            "rejected_records": len(rejected),
            "merged_duplicates": merged_duplicates,
            "conflicts": conflict_count,
            "source_errors": source_errors,
            "state_distribution": state_distribution,
            "pending_hash_records": pending_hash_records,
        },
        "rejected": rejected,
        "conflicts": conflict_list,
        "records": sorted_records,
    }


def _source_of(record: dict[str, Any]) -> str:
    source_id = record.get("source_id")
    return str(source_id) if isinstance(source_id, str) else "<unknown>"


def _conflicting_fields(first: dict[str, Any], second: dict[str, Any]) -> list[str]:
    fields: list[str] = []
    for key in sorted(set(first) | set(second)):
        if _canonical_json({key: first[key]}) != _canonical_json({key: second[key]}):
            fields.append(key)
    return fields


def default_source_paths(root: Path) -> list[Path]:
    sources = root / DEFAULT_SOURCES_DIR
    if not sources.is_dir():
        return []
    return sorted(sources.glob("d18_*_logic_catalog.json"))


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--source", type=Path, action="append", default=[])
    parser.add_argument("--output", type=Path, default=None)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    root = args.project_root.resolve()
    sources = [item.resolve() for item in args.source] or default_source_paths(root)
    if not sources:
        print("错误：未找到任何来源目录", file=sys.stderr)
        return 2
    result = compile_catalogs(sources, load_taxonomy_map(root))
    # 输入不可读/非法 JSON → 拒绝（exit 2）；数据级缺口只作审计摘要。
    if result["audit_summary"]["source_errors"]:
        for error in result["audit_summary"]["source_errors"]:
            print(f"来源拒绝：{error['source']} → {error['error']}", file=sys.stderr)
        return 2
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
