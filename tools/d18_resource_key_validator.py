#!/usr/bin/env python3
"""D18-L16 ResourceKey validator (read-only).

Validates logical resource keys for uniqueness, stable naming, deprecated
aliases, category matching and migration rules, per the D18 governance
baseline (D18-L01) and the D18-L11/L12/L13/L14 taxonomy dictionaries.

A ResourceKey is a pure logical identifier: it must exist independently of
any binary resource or Unity GUID. This tool never checks, opens or requires
asset files, never edits the Content Registry, and never touches runtime
references.

Rejected kinds: disk paths (absolute/relative/Windows drive/Assets/),
file extensions, display names (spaces/uppercase/non-ASCII/localization
keys), empty keys, duplicate keys, Unity-GUID-style keys, unknown
categories, category/prefix mismatches, cross-category collisions,
candidate-prefix keys used as active keys, and broken/cyclic deprecated
aliases or migrations.

Input records:

    {
      "record_id": optional stable record id for error messages,
      "key": required logical ResourceKey,
      "category": optional L02 category (animal/fish/building/character/
                  environment/material/prop/ship/ui/audio/lighting_reference),
      "content_id": optional content id that the key tail must equal
                    (namespaced keys only),
      "display_name": optional display name; rejected if it equals the key,
      "deprecated_alias_of": optional active key this key aliases,
      "migrated_from": optional older key this key migrated from,
      "source": optional provenance note (e.g. "D20-A03")
    }
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any

BARE_KEY_PATTERN = re.compile(r"^[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")
NAMESPACED_KEY_PATTERN = re.compile(r"^([a-z][a-z0-9_]*):([a-z][a-z0-9_]*):([a-z][a-z0-9_]*)$")
UNITY_GUID_PATTERN = re.compile(r"^[0-9a-f]{32}$")
DRIVE_PATTERN = re.compile(r"^[A-Za-z]:")
HISTORICAL_CANDIDATE_PREFIXES = ("candidate_", "d18a05_")

# Frozen L02 category enum (D18-L02 schema) — resource/domain level.
CATEGORY_KEYS = (
    "animal",
    "audio",
    "building",
    "character",
    "environment",
    "fish",
    "lighting_reference",
    "material",
    "prop",
    "ship",
    "ui",
)

# Prefix -> allowed L02 categories (frozen from D18-L11/L12/L13/L14 taxonomy
# dictionaries and the real intake catalog domains). asset_* is the universal
# resource record prefix and may carry any domain category.
PREFIX_CATEGORY_MAP: dict[str, tuple[str, ...]] = {
    "animal_*": ("animal",),
    "fish_*": ("fish",),
    "plant_*": ("environment",),
    "pet_*": ("animal",),
    "crew_*": ("character",),
    "npc_*": ("character",),
    "outfit_*": ("character",),
    "wildlife_*": ("animal", "fish"),
    "ship_*": ("ship",),
    "boat_*": ("ship",),
    "building_*": ("building",),
    "port_service_*": ("building",),
    "dock_*": ("building",),
    "berth_*": ("building",),
    "prop_*": ("prop",),
    "item_*": ("prop",),
    "island_*": ("environment",),
    "quest_*": ("environment",),
    "ui_*": ("ui",),
    "icon_*": ("ui",),
    "font_*": ("ui",),
    "audio_*": ("audio",),
    "music_*": ("audio",),
    "material_*": ("material",),
    "hdri_*": ("lighting_reference",),
    "lighting_*": ("lighting_reference",),
    "asset_*": CATEGORY_KEYS,
}

NAMESPACED_DOMAINS = ("d10", "d20")


def _prefix_of(key: str) -> str | None:
    if ":" in key:
        return None
    if "_" not in key:
        return None
    return key.split("_", 1)[0] + "_*"


def validate_key_format(key: Any) -> list[dict[str, str]]:
    """Return format issues for one key. Never touches the filesystem."""
    issues: list[dict[str, str]] = []
    if key is None or not isinstance(key, str):
        return [{"kind": "empty_key", "severity": "P0", "detail": "key is null or not a string"}]
    stripped = key.strip()
    if stripped == "":
        return [{"kind": "empty_key", "severity": "P0", "detail": "key is empty or whitespace"}]
    if key != stripped:
        issues.append({"kind": "invalid_format", "severity": "P0", "detail": "key has leading/trailing whitespace"})
    if "/" in key or "\\" in key:
        issues.append({"kind": "disk_path", "severity": "P0", "detail": f"key contains a path separator: {key!r}"})
    if DRIVE_PATTERN.match(key):
        issues.append({"kind": "disk_path", "severity": "P0", "detail": f"key looks like a drive path: {key!r}"})
    if ".." in key:
        issues.append({"kind": "disk_path", "severity": "P0", "detail": "key contains a parent-escape '..'"})
    if "assets/" in key.lower() or "assets\\" in key.lower():
        issues.append({"kind": "disk_path", "severity": "P0", "detail": "key references an Assets/ path"})
    if "." in key:
        if key.startswith("content."):
            issues.append({"kind": "display_name_key", "severity": "P0", "detail": "key is a localization display key"})
        else:
            issues.append({"kind": "file_extension_or_path", "severity": "P0", "detail": f"key contains a dot (file extension or path): {key!r}"})
    if any(not c.isascii() for c in key):
        issues.append({"kind": "display_name", "severity": "P0", "detail": "key contains non-ASCII characters"})
    if any(c.isupper() for c in key):
        issues.append({"kind": "display_name", "severity": "P0", "detail": "key contains uppercase letters (display-name style)"})
    if " " in key:
        issues.append({"kind": "display_name", "severity": "P0", "detail": "key contains spaces (display-name style)"})
    if UNITY_GUID_PATTERN.match(key):
        issues.append({"kind": "unity_guid", "severity": "P0", "detail": "key is a 32-hex Unity GUID, not a logical key"})

    if ":" in key:
        match = NAMESPACED_KEY_PATTERN.match(key)
        if not match:
            issues.append({"kind": "invalid_format", "severity": "P0", "detail": "namespaced key must be domain:category:content_id with 3 lowercase snake_case segments"})
        else:
            domain, namespace, tail = match.groups()
            if domain not in NAMESPACED_DOMAINS:
                issues.append({"kind": "unknown_namespace", "severity": "P1", "detail": f"namespace {domain!r} is not a frozen logical namespace"})
            if BARE_KEY_PATTERN.match(tail) is None:
                issues.append({"kind": "invalid_format", "severity": "P0", "detail": f"namespaced key tail is not a valid content id: {tail!r}"})
    else:
        if BARE_KEY_PATTERN.match(key) is None:
            issues.append({"kind": "invalid_format", "severity": "P0", "detail": "bare key must be lowercase snake_case [a-z][a-z0-9_]* with single underscores"})
    return issues


def _canonical_category(record: dict[str, Any]) -> str | None:
    category = record.get("category")
    if category is None:
        return None
    return str(category).strip()


def _key_or(record: dict[str, Any]) -> str:
    return str(record.get("key") or "")


def validate_records(
    records: list[dict[str, Any]],
    categories: tuple[str, ...] | None = None,
    prefix_map: dict[str, tuple[str, ...]] | None = None,
) -> dict[str, Any]:
    """Validate a list of ResourceKey records and return a full report.

    Pure data in, pure data out: no file writes, no Registry changes, no
    runtime reference changes, no asset existence checks.
    """
    categories = categories or CATEGORY_KEYS
    prefix_map = prefix_map or PREFIX_CATEGORY_MAP

    per_record: list[dict[str, Any]] = []
    global_issues: list[dict[str, str]] = []
    by_key: dict[str, list[int]] = {}

    for index, record in enumerate(records):
        key = _key_or(record)
        issues = validate_key_format(key)
        if key:
            by_key.setdefault(key, []).append(index)
        display_name = record.get("display_name")
        if display_name is not None and str(display_name).strip() == key and key:
            issues.append({"kind": "display_name_as_key", "severity": "P0", "detail": "key equals its own display name"})
        category = _canonical_category(record)
        if category is not None and category not in categories:
            issues.append({"kind": "unknown_category", "severity": "P0", "detail": f"category {category!r} is not in the frozen enum"})
        if ":" not in key:
            is_historical_candidate = any(key.startswith(p) for p in HISTORICAL_CANDIDATE_PREFIXES)
            if is_historical_candidate:
                is_alias = bool(record.get("deprecated_alias_of"))
                if not is_alias:
                    issues.append({"kind": "candidate_prefix_active", "severity": "P1", "detail": f"historical candidate prefix may only appear as a deprecated alias, never as an active key: {key!r}"})
            else:
                prefix = _prefix_of(key)
                if prefix is not None:
                    if prefix not in prefix_map:
                        issues.append({"kind": "unknown_prefix", "severity": "P0", "detail": f"key prefix {prefix!r} is not in the frozen prefix map"})
                    elif category is not None and category not in prefix_map[prefix]:
                        issues.append({"kind": "category_prefix_mismatch", "severity": "P0", "detail": f"key prefix {prefix!r} does not allow category {category!r}"})
        match = NAMESPACED_KEY_PATTERN.match(key) if ":" in key else None
        if match:
            content_id = record.get("content_id")
            if content_id is not None and str(content_id) != match.group(3):
                issues.append({"kind": "content_id_mismatch", "severity": "P0", "detail": "namespaced key tail does not match the declared content_id"})
            prefix = _prefix_of(match.group(3))
            if prefix is not None and prefix in prefix_map and category is not None and category not in prefix_map[prefix]:
                issues.append({"kind": "category_prefix_mismatch", "severity": "P0", "detail": f"namespaced tail prefix {prefix!r} does not allow category {category!r}"})
        per_record.append(
            {
                "record_id": record.get("record_id") or str(index),
                "key": key,
                "category": category,
                "deprecated_alias_of": record.get("deprecated_alias_of"),
                "migrated_from": record.get("migrated_from"),
                "source": record.get("source"),
                "valid": not issues,
                "issues": issues,
            }
        )

    key_set = set(by_key)
    for key, indexes in by_key.items():
        if len(indexes) > 1:
            global_issues.append(
                {"kind": "duplicate_key", "severity": "P0", "detail": f"key {key!r} declared by {len(indexes)} records"}
            )
            cats = {_canonical_category(records[i]) for i in indexes}
            if len(cats) > 1:
                global_issues.append(
                    {"kind": "cross_category_collision", "severity": "P0", "detail": f"key {key!r} appears with categories {sorted(str(c) for c in cats)}"}
                )

    def check_reference(kind: str, record: dict[str, Any], target_field: str) -> None:
        key = _key_or(record)
        target = record.get(target_field)
        if target is None or str(target).strip() == "":
            return
        target = str(target)
        if target not in key_set:
            global_issues.append({"kind": f"{kind}_target_missing", "severity": "P0", "detail": f"{kind} target {target!r} of key {key!r} does not exist in the record set"})
            return
        if target == key:
            global_issues.append({"kind": f"{kind}_self_reference", "severity": "P0", "detail": f"key {key!r} {kind} itself"})
            return
        target_cat = _canonical_category(records[by_key[target][0]])
        source_cat = _canonical_category(record)
        if source_cat is not None and target_cat is not None and source_cat != target_cat:
            global_issues.append({"kind": f"{kind}_category_mismatch", "severity": "P0", "detail": f"{kind} from {key!r} ({source_cat}) to {target!r} ({target_cat}) crosses categories"})
        if not per_record[by_key[target][0]]["valid"]:
            global_issues.append({"kind": f"{kind}_target_invalid", "severity": "P1", "detail": f"{kind} target {target!r} of key {key!r} is itself invalid"})

    for record in records:
        check_reference("deprecated_alias_of", record, "deprecated_alias_of")
        check_reference("migrated_from", record, "migrated_from")

    def detect_cycle(field: str, label: str) -> None:
        graph = {_key_or(r): str(r.get(field)).strip() for r in records if r.get(field)}
        visited: set[str] = set()
        stack: list[str] = []

        def walk(node: str) -> None:
            if node in visited:
                return
            visited.add(node)
            stack.append(node)
            nxt = graph.get(node)
            if nxt in stack:
                chain = " -> ".join(stack[stack.index(nxt):] + [nxt])
                global_issues.append({"kind": f"{label}_cycle", "severity": "P0", "detail": chain})
            elif nxt:
                walk(nxt)
            stack.pop()

        for node in graph:
            walk(node)

    detect_cycle("deprecated_alias_of", "alias")
    detect_cycle("migrated_from", "migration")

    kinds: dict[str, int] = {}
    for result in per_record:
        for issue in result["issues"]:
            kinds[issue["kind"]] = kinds.get(issue["kind"], 0) + 1
    for issue in global_issues:
        kinds[issue["kind"]] = kinds.get(issue["kind"], 0) + 1

    valid = len(per_record) > 0 and all(r["valid"] for r in per_record) and not global_issues
    return {
        "tool": "d18_resource_key_validator",
        "task_id": "D18-L16",
        "read_only": True,
        "registry_unchanged": True,
        "runtime_references_unchanged": True,
        "key_independent_of_binary": True,
        "record_count": len(records),
        "verdict": "CLEARED" if valid else "BLOCKED",
        "issue_kind_counts": kinds,
        "global_issues": global_issues,
        "records": per_record,
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18-L16 ResourceKey validator (read-only)")
    parser.add_argument("--records", required=True, help="JSON file containing a list of ResourceKey records")
    parser.add_argument("--output", default=None, help="write the validation report to this path")
    parser.add_argument("--categories", default=None, help="optional JSON array overriding the category enum")
    parser.add_argument("--prefix-map", default=None, help="optional JSON object overriding the prefix->categories map")
    args = parser.parse_args(argv)

    records = json.loads(Path(args.records).read_text(encoding="utf-8"))
    if not isinstance(records, list):
        raise SystemExit("--records must contain a JSON array of records")
    categories = tuple(json.loads(Path(args.categories).read_text(encoding="utf-8"))) if args.categories else None
    prefix_map = {str(k): tuple(v) for k, v in json.loads(Path(args.prefix_map).read_text(encoding="utf-8")).items()} if args.prefix_map else None

    report = validate_records(records, categories=categories, prefix_map=prefix_map)
    if args.output:
        Path(args.output).write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(
        {
            "record_count": report["record_count"],
            "verdict": report["verdict"],
            "issue_kind_counts": report["issue_kind_counts"],
        },
        ensure_ascii=False,
        indent=2,
    ))
    return 0 if report["verdict"] == "CLEARED" else 1


if __name__ == "__main__":
    raise SystemExit(main())
