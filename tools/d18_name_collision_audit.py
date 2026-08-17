#!/usr/bin/env python3
"""D18-L32 name collision audit (read-only).

Detects conflicts among canonical_id, ResourceKey, content_id, display name
and original filename fields across asset records, plus deprecated alias
cycles. Outputs conflict pairs, involved fields, severity and recommended
actions.

Severity model:

- P0 (blocking): duplicate id-like values (canonical_id / content_id /
  resource_key), cross-field id collisions, same full original path declared
  by two records, case-only variants of id-like values, deprecated alias
  cycles.
- P1 (hygiene): id equals its own display name, id equals its own original
  filename (with extension), id-like value reused as another record's display
  name, alias target not present in the record set.
- P2 (informational, non-blocking): same display name repeated within one
  category; same display name across different categories is LEGAL (no
  issue); same basename in different paths is LEGAL (no issue).

The tool never renames anything and never modifies the Content Registry.
The report is deterministic: every list is sorted.

Input records:

    {
      "record_id": required stable id for messages,
      "canonical_id": optional,
      "resource_keys": optional list,
      "content_ids": optional list,
      "display_names": optional list,
      "original_filenames": optional list (basenames),
      "original_paths": optional list (full relative paths),
      "category": optional (used for display-name legality),
      "deprecated_alias_of": optional target record_id or canonical_id
    }
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any

ID_LIKE_FIELDS = ("canonical_id", "resource_keys", "content_ids")


def _norm_list(value: Any) -> list[str]:
    if value is None:
        return []
    if isinstance(value, str):
        return [value]
    return [str(v) for v in value if str(v).strip()]


def _lower(s: str) -> str:
    return s.lower()


def _sort_key(value: Any) -> tuple:
    if isinstance(value, list):
        return tuple(value)
    return (value,)


class NameCollisionAudit:
    """Pure data audit: no file writes, no Registry changes, no renames."""

    def __init__(self, records: list[dict[str, Any]]):
        self.records = records
        self.conflicts: list[dict[str, Any]] = []
        self.per_record: list[dict[str, Any]] = []
        self.id_owner: dict[str, list[tuple[str, int]]] = defaultdict(list)
        self.display_owner: dict[str, list[tuple[str, int]]] = defaultdict(list)
        self.path_owner: dict[str, list[tuple[str, int]]] = defaultdict(list)
        self.case_variants: dict[str, list[str]] = defaultdict(list)
        self.alias_edges: dict[str, str] = {}
        self.record_ids: list[str] = []

    def add(self, kind: str, severity: str, pair: list[str], fields: list[str], value: str, action: str) -> None:
        self.conflicts.append(
            {
                "pair": sorted(pair),
                "fields": sorted(fields),
                "value": value,
                "severity": severity,
                "kind": kind,
                "action": action,
            }
        )

    def index(self) -> None:
        for index, record in enumerate(self.records):
            record_id = str(record.get("record_id") or f"record_{index}")
            self.record_ids.append(record_id)
            canonical = record.get("canonical_id")
            if canonical is not None and str(canonical).strip():
                self.id_owner[str(canonical)].append(("canonical_id", index))
            for field, values in (
                ("resource_key", _norm_list(record.get("resource_keys"))),
                ("content_id", _norm_list(record.get("content_ids"))),
            ):
                for value in values:
                    self.id_owner[value].append((field, index))
            for display in _norm_list(record.get("display_names")):
                self.display_owner[_lower(display)].append((display, index))
            for path in _norm_list(record.get("original_paths")):
                normalized = path.replace("\\", "/")
                self.path_owner[normalized].append((index, path))
            alias = record.get("deprecated_alias_of")
            if alias is not None and str(alias).strip():
                self.alias_edges[record_id] = str(alias)
                self.alias_edges.setdefault(str(alias), "")

    def run(self) -> dict[str, Any]:
        self.index()
        self.check_intra_record()
        self.check_id_like()
        self.check_case_variants()
        self.check_display_names()
        self.check_paths()
        self.check_aliases()
        return self.report()

    def check_intra_record(self) -> None:
        for index, record in enumerate(self.records):
            record_id = self.record_ids[index]
            canonical = record.get("canonical_id")
            if canonical is None or not str(canonical).strip():
                continue
            canonical = str(canonical)
            for display in _norm_list(record.get("display_names")):
                if display == canonical:
                    self.add(
                        "id_equals_display_name", "P1", [record_id],
                        ["canonical_id", "display_name"], canonical,
                        "display_name 应使用本地化 key（content.*），不要复用逻辑 id 字符串",
                    )
            for filename in _norm_list(record.get("original_filenames")):
                if filename == canonical:
                    self.add(
                        "id_equals_filename", "P1", [record_id],
                        ["canonical_id", "original_filename"], canonical,
                        "original_filename 保留包内原名；逻辑 id 独立命名，避免与带扩展名的文件名相同",
                    )
            for key in _norm_list(record.get("resource_keys")):
                for display in _norm_list(record.get("display_names")):
                    if display == key:
                        self.add(
                            "key_equals_display_name", "P1", [record_id],
                            ["resource_key", "display_name"], key,
                            "ResourceKey 不得与显示名相同；display_name 使用本地化 key",
                        )

    def check_id_like(self) -> None:
        for value, owners in sorted(self.id_owner.items()):
            distinct_records = sorted({self.record_ids[i] for _, i in owners})
            if len(distinct_records) < 2:
                continue
            fields = sorted({f for f, _ in owners})
            if len({f for f, _ in owners}) == 1 and fields[0] == "canonical_id":
                kind, severity, action = "duplicate_canonical_id", "P0", "保留一个登记，其余以 deprecated_alias_of 指向保留项（append-only），禁止自动重命名"
            else:
                kind, severity, action = "cross_field_collision", "P0", "为冲突记录提供不同 content_id/ResourceKey；若为同一资源，合并登记并保留弃用别名"
            self.add(kind, severity, distinct_records, fields, value, action)

    def check_case_variants(self) -> None:
        lower_groups: dict[str, list[tuple[str, str, int]]] = defaultdict(list)
        for value, owners in self.id_owner.items():
            lower_groups[_lower(value)].extend((value, field, index) for field, index in owners)
        for key, group in sorted(lower_groups.items()):
            distinct = sorted({value for value, _, _ in group})
            if len(distinct) < 2:
                continue
            distinct_records = sorted({self.record_ids[i] for _, _, i in group})
            if len(distinct_records) < 2:
                continue
            fields = sorted({field for _, field, _ in group})
            self.add(
                "case_collision", "P0", distinct_records, fields, f"{key} (variants: {', '.join(distinct)})",
                "id-like 值必须唯一且小写；大小写变体禁止并存（Registry 为小写 snake_case 口径）",
            )

    def check_display_names(self) -> None:
        for key, group in sorted(self.display_owner.items()):
            if len(group) < 2:
                continue
            categories = {}
            for display, index in group:
                categories.setdefault(str(self.records[index].get("category") or ""), []).append(display)
            record_ids = sorted({self.record_ids[i] for _, i in group})
            if len(categories) > 1:
                continue
            self.add(
                "same_display_name_same_category", "P2", record_ids,
                ["display_name"], key,
                "同类别多条记录使用相同显示名；人工确认是否需要区分（如加语境词）",
            )
        for display, owners in sorted(self.display_owner.items()):
            id_owners = self.id_owner.get(display)
            if not id_owners:
                continue
            id_records = {self.record_ids[i] for _, i in id_owners}
            display_records = {self.record_ids[i] for _, i in owners}
            if display_records <= id_records and display_records:
                continue
            self.add(
                "id_looks_like_display_name", "P1", sorted(display_records | id_records),
                ["display_name"], display,
                "另一条记录的逻辑 id 被复用为显示名；display_name 应使用本地化 key（content.*）",
            )

    def check_paths(self) -> None:
        for path, owners in sorted(self.path_owner.items()):
            if len(owners) < 2:
                continue
            pair = sorted({self.record_ids[i] for i, _ in owners})
            self.add(
                "same_path_duplicate", "P0", pair,
                ["original_path"], path,
                "同一路径被多条记录声明；合并登记或按 L31 重复身份登记，禁止静默删除",
            )

    def check_aliases(self) -> None:
        known = set(self.record_ids)
        for source, target in sorted(self.alias_edges.items()):
            if not target:
                continue
            if source == target:
                self.add("alias_self_reference", "P0", [source], ["deprecated_alias_of"], target, "弃用别名不得指向自己")
            elif target not in known:
                self.add("alias_target_not_in_set", "P1", [source], ["deprecated_alias_of"], target, "补登记目标记录或修正别名目标")
        visited: set[str] = set()
        stack: list[str] = []
        for start in sorted(self.alias_edges):
            if start in visited:
                continue
            visited.add(start)
            stack.append(start)
            node = self.alias_edges.get(start)
            while node in self.alias_edges:
                if node in stack:
                    chain = " -> ".join(stack[stack.index(node):] + [node])
                    self.add("alias_cycle", "P0", [node], ["deprecated_alias_of"], chain, "人工复核弃用链并消除环；记录 append-only")
                    break
                if node in visited:
                    break
                visited.add(node)
                stack.append(node)
                node = self.alias_edges.get(node)
            stack.clear()

    def report(self) -> dict[str, Any]:
        severity_counts = Counter(c["severity"] for c in self.conflicts)
        self.conflicts.sort(key=lambda c: (c["severity"], c["kind"], c["pair"], c["fields"], c["value"]))
        return {
            "tool": "d18_name_collision_audit",
            "task_id": "D18-L32",
            "read_only": True,
            "registry_unchanged": True,
            "no_auto_rename": True,
            "deterministic": True,
            "record_count": len(self.records),
            "verdict": "BLOCKED" if severity_counts.get("P0", 0) else "CLEARED",
            "conflict_count": len(self.conflicts),
            "severity_counts": {
                "P0": severity_counts.get("P0", 0),
                "P1": severity_counts.get("P1", 0),
                "P2": severity_counts.get("P2", 0),
            },
            "conflicts": self.conflicts,
        }


def audit_records(records: list[dict[str, Any]]) -> dict[str, Any]:
    """Audit a list of name records; pure data in, pure data out."""
    return NameCollisionAudit(records).run()


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="D18-L32 name collision audit (read-only)")
    parser.add_argument("--records", required=True, help="JSON file containing a list of name records")
    parser.add_argument("--output", default=None, help="write the audit report to this path")
    args = parser.parse_args(argv)

    records = json.loads(Path(args.records).read_text(encoding="utf-8"))
    if not isinstance(records, list):
        raise SystemExit("--records must contain a JSON array of records")
    report = audit_records(records)
    if args.output:
        Path(args.output).write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(
        {
            "record_count": report["record_count"],
            "verdict": report["verdict"],
            "conflict_count": report["conflict_count"],
            "severity_counts": report["severity_counts"],
        },
        ensure_ascii=False,
        indent=2,
    ))
    return 0 if report["verdict"] == "CLEARED" else 1


if __name__ == "__main__":
    raise SystemExit(main())
