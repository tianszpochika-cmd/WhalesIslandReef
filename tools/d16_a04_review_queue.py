"""Create a deterministic human-review queue for D16-A04 Pending records.

The queue is an evidence aid, not an approval mechanism.  It never changes a
registry entry and never infers historical truth, licensing, warning level, or
skip permission.  A reviewer must make and record those decisions separately.
"""

from __future__ import annotations

import argparse
import json
from collections import Counter
from pathlib import Path
from typing import Any, Iterable

from d16_a04_review_audit import SENSITIVE_TAGS, load_entries

DEFAULT_BATCH_SIZE = 25


def _as_list(value: Any) -> list[str]:
    if isinstance(value, list):
        return [str(item) for item in value if item is not None]
    if value is None:
        return []
    return [str(value)]


def _review_scope(entry: dict[str, Any]) -> str:
    record_id = str(entry.get("id", ""))
    tags = {str(tag).lower() for tag in _as_list(entry.get("tags"))}
    warnings = _as_list(entry.get("content_warning_ids"))
    if tags & SENSITIVE_TAGS or warnings:
        return "sensitive_content"
    if record_id.startswith("asset_"):
        return "asset_history_and_authorization"
    return "historical_content"


def _required_decisions(entry: dict[str, Any], scope: str) -> list[str]:
    decisions = ["historical_tag_and_evidence"]
    if scope == "asset_history_and_authorization":
        decisions.extend(("rights_holder_and_license", "commercial_use_and_modification"))
    if scope == "sensitive_content":
        decisions.extend(("presentation_level", "warning_and_skip_behavior"))
    return decisions


def build_queue(root: Path, batch_size: int = DEFAULT_BATCH_SIZE) -> dict[str, Any]:
    if batch_size <= 0:
        raise ValueError("batch_size must be greater than zero")

    records: list[dict[str, Any]] = []
    for path, entry in load_entries(root):
        if entry.get("historical_review") != "Pending":
            continue
        scope = _review_scope(entry)
        source_refs = _as_list(entry.get("source_refs"))
        evidence_refs = _as_list(entry.get("review_evidence_refs"))
        records.append(
            {
                "id": str(entry["id"]),
                "path": path.as_posix(),
                "scope": scope,
                "historical_tag": entry.get("historical_tag"),
                "asset_state": entry.get("asset_state"),
                "tags": _as_list(entry.get("tags")),
                "content_warning_ids": _as_list(entry.get("content_warning_ids")),
                "skip_allowed": entry.get("skip_allowed"),
                "source_refs": source_refs,
                "review_evidence_refs": evidence_refs,
                "source": entry.get("source"),
                "license": entry.get("license"),
                "authorization": entry.get("authorization"),
                "presentation_levels": _as_list(entry.get("presentation_levels")),
                "lod_profile": entry.get("lod_profile"),
                "required_decisions": _required_decisions(entry, scope),
                "approval_policy": "human_decision_required",
            }
        )

    records.sort(key=lambda item: (item["scope"], item["id"], item["path"]))

    # Batch within one review scope so a human reviewer never receives a batch
    # that silently mixes historical, sensitive, and asset-rights decisions.
    batches: list[dict[str, Any]] = []
    batch_number = 0
    for scope in sorted({record["scope"] for record in records}):
        scoped_records = [record for record in records if record["scope"] == scope]
        for offset in range(0, len(scoped_records), batch_size):
            batch_number += 1
            batch_records = scoped_records[offset : offset + batch_size]
            batch_id = f"D16-A04-{batch_number:03d}"
            for record in batch_records:
                record["batch_id"] = batch_id
                record["batch_number"] = batch_number
            batches.append(
                {
                    "batch_id": batch_id,
                    "scope": scope,
                    "record_count": len(batch_records),
                    "record_ids": [record["id"] for record in batch_records],
                }
            )

    scope_counts = Counter(record["scope"] for record in records)
    state_counts = Counter(str(record["asset_state"]) for record in records if record["asset_state"] is not None)
    return {
        "task": "D16-A04",
        "status": "BLOCKED" if records else "PASS",
        "root": root.as_posix(),
        "pending_count": len(records),
        "batch_size": batch_size,
        "batch_count": len(batches),
        "batches": batches,
        "scope_counts": dict(sorted(scope_counts.items())),
        "asset_state_counts": dict(sorted(state_counts.items())),
        "records": records,
        "approval_policy": "No record is approved by this tool; every decision requires human evidence.",
    }


def _markdown_value(value: Any) -> str:
    if value is None:
        return ""
    if isinstance(value, list):
        return "; ".join(_markdown_value(item) for item in value)
    if isinstance(value, dict):
        return json.dumps(value, ensure_ascii=False, sort_keys=True)
    return str(value).replace("|", "\\|").replace("\n", " ")


def render_markdown(queue: dict[str, Any]) -> str:
    lines = [
        "# D16-A04 Pending 人工复核队列",
        "",
        "> 本文件由 `tools/d16_a04_review_queue.py` 生成。它只整理证据入口，不批准历史事实、授权、表现等级、警告或跳过行为。",
        "",
        f"- 状态：**{queue['status']}**",
        f"- Pending 条目：**{queue['pending_count']}**",
        f"- 审查批次：**{queue['batch_count']}**（每批最多 {queue['batch_size']} 条，批次不混合审查范围）",
        f"- 范围统计：`{json.dumps(queue['scope_counts'], ensure_ascii=False, sort_keys=True)}`",
        f"- 资产状态统计：`{json.dumps(queue['asset_state_counts'], ensure_ascii=False, sort_keys=True)}`",
        "- 每条记录必须由人工补充决定和证据；没有决定前继续保持 `Pending`。",
        "",
        "## 批次清单",
        "",
        "| 批次 | 范围 | 条目数 | 首条 ID | 末条 ID |",
        "|---|---|---:|---|---|",
    ]
    for batch in queue["batches"]:
        ids = batch["record_ids"]
        lines.append(
            "| "
            + " | ".join(
                _markdown_value(value)
                for value in (
                    batch["batch_id"],
                    batch["scope"],
                    batch["record_count"],
                    ids[0] if ids else "",
                    ids[-1] if ids else "",
                )
            )
            + " |"
        )
    lines.extend(
        [
            "",
            "## 条目清单",
            "",
            "| 批次 | ID | 范围 | 文件 | 标签 | 警告 | 跳过 | 来源/证据 | 权利原始字段 | 必须决定 |",
            "|---|---|---|---|---|---|---|---|---|---|",
        ]
    )
    for record in queue["records"]:
        refs = record["source_refs"] + record["review_evidence_refs"]
        rights = {
            "source": record["source"],
            "license": record["license"],
            "authorization": record["authorization"],
        }
        lines.append(
            "| "
            + " | ".join(
                _markdown_value(value)
                for value in (
                    record["batch_id"],
                    record["id"],
                    record["scope"],
                    record["path"],
                    record["tags"],
                    record["content_warning_ids"],
                    record["skip_allowed"],
                    refs,
                    rights,
                    record["required_decisions"],
                )
            )
            + " |"
        )
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("--json-out", type=Path)
    parser.add_argument("--markdown-out", type=Path)
    parser.add_argument("--batch-size", type=int, default=DEFAULT_BATCH_SIZE)
    args = parser.parse_args()

    queue = build_queue(args.root, batch_size=args.batch_size)
    output = json.dumps(queue, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    if args.markdown_out:
        args.markdown_out.parent.mkdir(parents=True, exist_ok=True)
        args.markdown_out.write_text(render_markdown(queue), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
