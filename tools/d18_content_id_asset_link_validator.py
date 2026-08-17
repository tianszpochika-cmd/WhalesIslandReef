"""D18-L15 content_id <-> asset record link validator.

Validates, per D18 logic asset record, that every ``content_ids`` entry:
- exists in a supplied content catalog/definition source (D8 ContentRegistry
  entries or intake-style ``eligible_entries``), else ``missing_content_id``;
- starts with a registered content prefix whose registered type matches the
  definition's declared ``category``, else ``unknown_prefix``/``type_mismatch``;
- is not duplicated inside the record (``duplicate_content_id``) and is not
  mapped by more than one record (``duplicate_content_id_across_records``);
- is not deprecated (``deprecated_content_id``, with replacement when known);
- is not a candidate-file leak: values must be lower snake_case business
  content keys, never candidate file paths, display names or candidate-derived
  keys (``path_like_content_id``, ``candidate_key_leak``, ``candidate_path_leak``).

The tool is strictly read-only: it never edits the content catalog, never
creates private IDs, never imports assets and never grants any state
(``approved_count`` is always 0). Records whose existence cannot be verified
because no catalog was supplied are reported as PENDING, never approved.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Iterable

CONTENT_ID_RE = re.compile(r"^[a-z0-9_]+$")
CANDIDATE_PREFIX = "candidate_"
STATUS_OK = "OK"
STATUS_PENDING = "PENDING"
STATUS_ERROR = "ERROR"

# Frozen D8 13-prefix vocabulary (registry.json, 2026-08-10). A --registry
# file can extend/replace it, but records must never rely on private prefixes.
DEFAULT_PREFIX_TYPES = {
    "ship_": "ship",
    "crew_": "crew",
    "island_": "island",
    "building_": "building",
    "item_": "item",
    "animal_": "animal",
    "plant_": "plant",
    "fish_": "fish",
    "quest_": "quest",
    "reward_": "reward",
    "outfit_": "outfit",
    "pet_": "pet",
    "asset_": "asset",
}


def load_json_file(path: Path) -> tuple[Any | None, str, str]:
    """Load a JSON file; return (data, error_code, message)."""

    try:
        return json.loads(path.read_text(encoding="utf-8")), "", ""
    except FileNotFoundError:
        return None, "file_not_found", f"file does not exist: {path}"
    except json.JSONDecodeError as exc:
        return None, "file_json_error", f"line {exc.lineno}: {exc.msg} in {path.name}"
    except OSError as exc:
        return None, "file_read_error", str(exc)


def load_records(path: Path) -> tuple[list[Any] | None, str, str]:
    """Load records from a JSON file; accept a list or ``{"records": [...]}``."""

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if isinstance(data, list):
        return data, "", ""
    if isinstance(data, dict) and isinstance(data.get("records"), list):
        return data["records"], "", ""
    return None, "records_not_supported", "records JSON must be a list or {\"records\": [...]}"


def _entry_id(entry: Any) -> str:
    if isinstance(entry, dict):
        value = entry.get("id")
        if value is None:
            value = entry.get("content_id")
        if isinstance(value, str):
            return value
    return ""


def load_catalog(path: Path) -> tuple[dict[str, dict[str, set[str]]] | None, str, str]:
    """Load a content definition source into {id: {"categories": set, "domains": set}}.

    Supported shapes: a list of definitions, ``{"entries": [...]}`` (D8
    ContentRegistry style, entries carry ``id`` and ``category``) or
    ``{"eligible_entries": [...]}`` (intake catalog style, entries carry
    ``content_id`` and ``domain``).
    """

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if isinstance(data, list):
        entries: list[Any] = data
    elif isinstance(data, dict) and isinstance(data.get("entries"), list):
        entries = data["entries"]
    elif isinstance(data, dict) and isinstance(data.get("eligible_entries"), list):
        entries = data["eligible_entries"]
    else:
        return None, "catalog_not_supported", f"catalog JSON must be a list, {{\"entries\": [...]}} or {{\"eligible_entries\": [...]}}: {path}"
    index: dict[str, dict[str, set[str]]] = {}
    for entry in entries:
        entry_id = _entry_id(entry)
        if not entry_id:
            continue
        record = index.setdefault(entry_id, {"categories": set(), "domains": set()})
        if isinstance(entry, dict):
            if isinstance(entry.get("category"), str):
                record["categories"].add(entry["category"])
            if isinstance(entry.get("domain"), str):
                record["domains"].add(entry["domain"])
    return index, "", ""


def load_prefix_map(path: Path) -> tuple[dict[str, str] | None, str, str]:
    """Load a prefix map from a registry-style JSON.

    Accepted shapes: ``{"prefix_definitions": [{"prefix", "entity_type"}]}``
    (ContentRegistry registry.json) or a flat object ``{prefix: entity_type}``.
    """

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if isinstance(data, dict) and isinstance(data.get("prefix_definitions"), list):
        result: dict[str, str] = {}
        for definition in data["prefix_definitions"]:
            if isinstance(definition, dict) and isinstance(definition.get("prefix"), str) and isinstance(definition.get("entity_type"), str):
                result[definition["prefix"]] = definition["entity_type"]
        if not result:
            return None, "registry_empty", f"registry has no usable prefix_definitions: {path}"
        return result, "", ""
    if isinstance(data, dict) and all(isinstance(k, str) and isinstance(v, str) for k, v in data.items()):
        return data, "", ""
    return None, "registry_not_supported", f"registry JSON must be {{\"prefix_definitions\": [...]}} or {{prefix: type}}: {path}"


def load_deprecated(path: Path) -> tuple[dict[str, str | None] | None, str, str]:
    """Load deprecated content ids; a list becomes {id: None}, an object keeps replacements."""

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if isinstance(data, list):
        result: dict[str, str | None] = {}
        for item in data:
            if isinstance(item, str):
                result[item] = None
        return result, "", ""
    if isinstance(data, dict):
        result = {}
        for key, value in data.items():
            if isinstance(key, str):
                result[key] = value if isinstance(value, str) else None
        return result, "", ""
    return None, "deprecated_not_supported", "deprecated JSON must be a list or an object {id: replacement}"


def load_candidates(path: Path) -> tuple[set[str] | None, str, str]:
    """Load candidate file paths / display names that must never be content ids."""

    data, code, message = load_json_file(path)
    if data is None:
        return None, code, message
    if not isinstance(data, list):
        return None, "candidates_not_supported", "candidates JSON must be a list of strings"
    return {str(item).replace("\\", "/") for item in data if isinstance(item, str)}, "", ""


def _prefix_type(content_id: str, prefix_types: dict[str, str]) -> tuple[str | None, str | None]:
    """Longest registered prefix matching the content id."""

    for prefix, entity_type in sorted(prefix_types.items(), key=lambda item: -len(item[0])):
        if content_id.startswith(prefix):
            return prefix, entity_type
    return None, None


def _validate_content_id(
    content_id: Any,
    *,
    prefix_types: dict[str, str],
    catalog: dict[str, dict[str, set[str]]] | None,
    deprecated: dict[str, str | None],
    candidates: set[str],
) -> dict[str, Any]:
    if not isinstance(content_id, str) or not content_id:
        return {"content_id": content_id, "status": STATUS_ERROR, "code": "invalid_content_id", "reason": "content id must be a non-empty string"}
    if not CONTENT_ID_RE.fullmatch(content_id):
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "path_like_content_id",
            "reason": "content id must be lower snake_case business key; candidate paths, display names and absolute paths are forbidden",
        }
    if content_id.startswith(CANDIDATE_PREFIX):
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "candidate_key_leak",
            "reason": "candidate_-prefixed keys derive from candidate files and must not be used as business content ids (no reverse dependency on candidates)",
        }
    prefix, registered_type = _prefix_type(content_id, prefix_types)
    if prefix is None:
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "unknown_prefix",
            "reason": f"content id does not start with a registered prefix ({sorted(prefix_types)})",
        }
    if content_id in deprecated:
        replacement = deprecated[content_id]
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "deprecated_content_id",
            "replacement": replacement, "reason": f"content id is deprecated; replacement={replacement!r}" if replacement else "content id is deprecated and has no replacement",
        }
    if content_id in candidates:
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "candidate_path_leak",
            "reason": "content id exactly matches a candidate file path or display name",
        }
    if catalog is None:
        return {
            "content_id": content_id, "status": STATUS_PENDING, "code": "catalog_missing",
            "reason": "no content catalog supplied; existence and type cannot be verified",
        }
    definition = catalog.get(content_id)
    if definition is None:
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "missing_content_id",
            "reason": "content id is not present in any supplied catalog/definition source",
        }
    categories = definition.get("categories") or set()
    if categories and registered_type not in categories:
        return {
            "content_id": content_id, "status": STATUS_ERROR, "code": "type_mismatch",
            "reason": f"prefix {prefix} registers type {registered_type!r} but definition declares {sorted(categories)}",
        }
    return {"content_id": content_id, "status": STATUS_OK, "code": "ok", "reason": ""}


def _validate_record(
    record: Any,
    index: int,
    *,
    prefix_types: dict[str, str],
    catalog: dict[str, dict[str, set[str]]] | None,
    deprecated: dict[str, str | None],
    candidates: set[str],
    seen_content_ids: dict[str, str],
) -> dict[str, Any]:
    if not isinstance(record, dict):
        return {
            "index": index, "id": f"record[{index}]", "status": STATUS_ERROR, "code": "invalid_record",
            "reason": "record must be an object", "content_ids": [],
        }
    record_id = record.get("canonical_asset_id")
    if not isinstance(record_id, str) or not record_id:
        record_id = f"record[{index}]"
    raw_ids = record.get("content_ids")
    if raw_ids is None:
        return {
            "index": index, "id": record_id, "status": STATUS_ERROR, "code": "missing_content_ids",
            "reason": "content_ids field is required on every asset record", "content_ids": [],
        }
    if not isinstance(raw_ids, list):
        return {
            "index": index, "id": record_id, "status": STATUS_ERROR, "code": "invalid_content_ids",
            "reason": "content_ids must be an array", "content_ids": [],
        }
    if not raw_ids:
        return {
            "index": index, "id": record_id, "status": STATUS_ERROR, "code": "empty_content_ids",
            "reason": "content_ids must not be empty; a record must map to at least one content id", "content_ids": [],
        }
    duplicates_in_record = {item for item in raw_ids if isinstance(item, str) and raw_ids.count(item) > 1}
    items: list[dict[str, Any]] = []
    for content_id in raw_ids:
        item = _validate_content_id(
            content_id,
            prefix_types=prefix_types,
            catalog=catalog,
            deprecated=deprecated,
            candidates=candidates,
        )
        if isinstance(content_id, str) and content_id in duplicates_in_record and item["code"] in ("ok", "catalog_missing"):
            item = dict(item)
            item["status"] = STATUS_ERROR
            item["code"] = "duplicate_content_id"
            item["reason"] = "content id appears more than once inside the record's content_ids array"
        items.append(item)
    for item in items:
        content_id = item["content_id"]
        if isinstance(content_id, str) and item["status"] == STATUS_OK:
            previous = seen_content_ids.get(content_id)
            if previous is not None:
                item["status"] = STATUS_ERROR
                item["code"] = "duplicate_content_id_across_records"
                item["reason"] = f"content id is already mapped by record {previous!r}; one content id maps to one asset record"
            else:
                seen_content_ids[content_id] = record_id
    statuses = [item["status"] for item in items]
    if STATUS_ERROR in statuses:
        status, code = STATUS_ERROR, "blocked"
    elif STATUS_PENDING in statuses:
        status, code = STATUS_PENDING, "pending"
    else:
        status, code = STATUS_OK, "ok"
    reason = "" if status == STATUS_OK else ("pending: content id evidence incomplete" if status == STATUS_PENDING else "blocked by content id issues; see content_ids for details")
    return {"index": index, "id": record_id, "status": status, "code": code, "reason": reason, "content_ids": items}


def validate_records(
    records: Iterable[Any],
    *,
    catalogs: Iterable[dict[str, dict[str, set[str]]]] | None = None,
    prefix_types: dict[str, str] | None = None,
    deprecated: dict[str, str | None] | None = None,
    candidates: set[str] | None = None,
) -> dict[str, Any]:
    """Validate records and return a deterministic JSON-compatible result."""

    catalog: dict[str, dict[str, set[str]]] | None = None
    for single in catalogs or ():
        if catalog is None:
            catalog = {}
        for content_id, definition in single.items():
            target = catalog.setdefault(content_id, {"categories": set(), "domains": set()})
            target["categories"] |= definition.get("categories", set())
            target["domains"] |= definition.get("domains", set())
    prefix_types = dict(DEFAULT_PREFIX_TYPES if prefix_types is None else prefix_types)
    deprecated = dict(deprecated or {})
    candidates = set(candidates or [])

    seen_content_ids: dict[str, str] = {}
    items = [
        _validate_record(
            record, index,
            prefix_types=prefix_types,
            catalog=catalog,
            deprecated=deprecated,
            candidates=candidates,
            seen_content_ids=seen_content_ids,
        )
        for index, record in enumerate(records)
    ]

    issues: list[dict[str, Any]] = []
    structural_codes = {"invalid_record", "missing_content_ids", "invalid_content_ids", "empty_content_ids"}
    for item in items:
        if item["status"] == STATUS_OK:
            continue
        if item["code"] in structural_codes:
            issues.append({
                "code": item["code"], "severity": "error", "message": item["reason"],
                "record_id": item["id"],
            })
        for content in item["content_ids"]:
            if content["status"] != STATUS_OK:
                entry = {
                    "code": content["code"],
                    "severity": "pending" if content["status"] == STATUS_PENDING else "error",
                    "message": content["reason"],
                    "record_id": item["id"], "content_id": content["content_id"],
                }
                if content.get("replacement"):
                    entry["replacement"] = content["replacement"]
                issues.append(entry)
    if not items:
        issues.append({"code": "no_records", "severity": "error", "message": "no records were supplied; nothing was verified"})

    code_counts: dict[str, int] = {}
    for issue in issues:
        code_counts[issue["code"]] = code_counts.get(issue["code"], 0) + 1
    error_count = sum(1 for issue in issues if issue.get("severity") == "error")
    pending_count = sum(1 for issue in issues if issue.get("severity") == "pending")
    if error_count > 0:
        status = "BLOCKED"
    elif pending_count > 0:
        status = "PENDING"
    else:
        status = "PASS"

    return {
        "task": "D18-L15",
        "tool": "tools/d18_content_id_asset_link_validator.py",
        "read_only": True,
        "approved_count": 0,
        "status": status,
        "record_count": len(items),
        "verified_count": sum(1 for item in items if item["status"] == STATUS_OK),
        "pending_count": pending_count,
        "error_count": error_count,
        "catalog_entry_count": len(catalog) if catalog is not None else 0,
        "issue_counts": dict(sorted(code_counts.items())),
        "issues": issues,
        "records": items,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--records", type=Path, action="append", default=[], help="asset records JSON (repeatable)")
    parser.add_argument("--catalog", type=Path, action="append", default=[], help="content definition source JSON (repeatable)")
    parser.add_argument("--registry", type=Path, default=None, help="optional prefix map JSON (registry.json style)")
    parser.add_argument("--deprecated", type=Path, default=None, help="deprecated content ids JSON (list or {id: replacement})")
    parser.add_argument("--candidates", type=Path, default=None, help="candidate file paths / display names JSON list")
    parser.add_argument("--output", type=Path, default=None, help="write result JSON to this path")
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    records: list[Any] = []
    input_issues: list[dict[str, Any]] = []

    catalogs: list[dict[str, dict[str, set[str]]]] = []
    for catalog_path in args.catalog:
        loaded, code, message = load_catalog(catalog_path)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(catalog_path)})
            continue
        catalogs.append(loaded)
    for records_path in args.records:
        loaded, code, message = load_records(records_path)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(records_path)})
            continue
        records.extend(loaded)

    prefix_types: dict[str, str] | None = None
    if args.registry is not None:
        loaded, code, message = load_prefix_map(args.registry)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(args.registry)})
        else:
            prefix_types = loaded
    deprecated: dict[str, str | None] | None = None
    if args.deprecated is not None:
        loaded, code, message = load_deprecated(args.deprecated)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(args.deprecated)})
        else:
            deprecated = loaded
    candidates: set[str] | None = None
    if args.candidates is not None:
        loaded, code, message = load_candidates(args.candidates)
        if loaded is None:
            input_issues.append({"code": code, "severity": "error", "message": message, "file": str(args.candidates)})
        else:
            candidates = loaded

    result = validate_records(records, catalogs=catalogs, prefix_types=prefix_types, deprecated=deprecated, candidates=candidates)
    if input_issues:
        result["input_issues"] = input_issues
        result["error_count"] += len(input_issues)
        result["status"] = "BLOCKED"
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    exit_codes = {"PASS": 0, "PENDING": 1, "BLOCKED": 2}
    return exit_codes.get(result["status"], 3)


if __name__ == "__main__":
    raise SystemExit(main())
