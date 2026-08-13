"""Build a read-only D16-A04 human review packet.

The pending queue is useful for discovery, but a reviewer also needs one
stable form per record.  This module creates that form without making any
historical, sensitive-content, licensing, or authorization decision.  It
never writes the Content Registry; the explicit single-record decision tool
remains the only write path.
"""

from __future__ import annotations

import argparse
import json
from datetime import date
from pathlib import Path
from typing import Any

from d16_a04_review_queue import build_queue


DECISIONS = ["Approved", "ApprovedWithWarning", "Rejected", "Pending"]
PRESENTATION_LEVELS = ["Full", "Contextual", "LowIntensity", "Omitted"]
EXTERNAL_PREFIXES = ("http://", "https://", "git://", "urn:")


def _reference_inventory(project_root: Path, references: list[str]) -> list[dict[str, Any]]:
    """Describe references without treating a reference as approval evidence."""

    inventory: list[dict[str, Any]] = []
    for reference in references:
        value = str(reference).strip()
        clean = value.split("#", 1)[0].strip()
        if not clean:
            inventory.append({"reference": value, "kind": "empty", "exists": False})
            continue
        if clean.startswith(EXTERNAL_PREFIXES):
            inventory.append({"reference": value, "kind": "external_or_logical", "exists": True})
            continue

        candidate = Path(clean)
        resolved = candidate if candidate.is_absolute() else project_root / candidate
        resolved = resolved.resolve()
        if resolved.is_file():
            kind = "local_file"
            exists = True
        elif resolved.is_dir():
            kind = "local_directory"
            exists = True
        else:
            kind = "missing_local_reference"
            exists = False
        inventory.append(
            {
                "reference": value,
                "kind": kind,
                "exists": exists,
                "resolved_path": resolved.as_posix(),
            }
        )
    return inventory


def _decision_template(record: dict[str, Any]) -> dict[str, Any]:
    """Return blank fields that map to the explicit decision writer."""

    template: dict[str, Any] = {
        "decision": "Pending",
        "reviewer": "",
        "review_date": "",
        "decision_basis": "",
        "additional_evidence_refs": [],
        "notes": "",
    }
    if "rights_holder_and_license" in record["required_decisions"]:
        template["rights_holder_and_license"] = ""
        template["commercial_use_and_modification"] = ""
    if "presentation_level" in record["required_decisions"]:
        template["presentation_levels"] = []
        template["warning_ids"] = []
        template["skip_allowed"] = None
    return template


def build_packet(
    root: Path,
    *,
    batch_size: int = 25,
    captured_at: str | None = None,
) -> dict[str, Any]:
    """Build a deterministic, human-only packet from the current queue."""

    queue = build_queue(root, batch_size=batch_size)
    project_root = root.parent if root.name.lower() == "game" else root
    records: list[dict[str, Any]] = []
    reference_gaps: list[dict[str, str]] = []
    decision_evidence_ready = 0

    for source_record in queue["records"]:
        record = dict(source_record)
        source_inventory = _reference_inventory(project_root, record["source_refs"])
        review_inventory = _reference_inventory(project_root, record["review_evidence_refs"])
        missing = [
            item
            for item in source_inventory + review_inventory
            if item["kind"] == "missing_local_reference"
        ]
        for item in missing:
            reference_gaps.append({"id": record["id"], "reference": item["reference"]})

        file_evidence = [
            item["reference"]
            for item in review_inventory
            if item["kind"] == "local_file"
        ]
        if file_evidence:
            decision_evidence_ready += 1

        record["source_reference_inventory"] = source_inventory
        record["review_evidence_inventory"] = review_inventory
        record["decision_evidence_status"] = (
            "has_existing_local_file_reference"
            if file_evidence
            else "reviewer_must_add_local_file_reference"
        )
        record["decision_template"] = _decision_template(record)
        record["human_decision_required"] = True
        records.append(record)

    captured = captured_at or date.today().isoformat()
    status = "HUMAN_DECISION_REQUIRED"
    if reference_gaps:
        status = "BLOCKED_REFERENCE_GAP"
    elif not records:
        status = "EMPTY"

    return {
        "schema_version": 1,
        "packet_id": f"D16-A04-REVIEW-PACKET-{captured}",
        "task": "D16-A04",
        "status": status,
        "captured_at": captured,
        "root": root.as_posix(),
        "pending_count": len(records),
        "batch_size": queue["batch_size"],
        "batch_count": queue["batch_count"],
        "scope_counts": queue["scope_counts"],
        "batches": queue["batches"],
        "records": records,
        "reference_gap_count": len(reference_gaps),
        "reference_gaps": reference_gaps,
        "decision_evidence_file_ready_count": decision_evidence_ready,
        "decision_options": DECISIONS,
        "presentation_levels": PRESENTATION_LEVELS,
        "approval_policy": (
            "This packet never approves a record. Keep historical_review=Pending "
            "until a responsible human supplies real evidence and uses the "
            "single-record decision writer."
        ),
        "write_path": "tools/d16_a04_apply_review_decision.py",
    }


def _markdown_value(value: Any) -> str:
    if value is None:
        return ""
    if isinstance(value, list):
        return "; ".join(_markdown_value(item) for item in value)
    if isinstance(value, dict):
        return json.dumps(value, ensure_ascii=False, sort_keys=True)
    return str(value).replace("|", "\\|").replace("\n", " ")


def render_markdown(packet: dict[str, Any]) -> str:
    lines = [
        "# D16-A04 Pending 人工复核填写包",
        "",
        "> 本文件只生成逐条填写模板和证据入口，不批准历史事实、敏感内容、授权或资产状态。",
        "",
        f"- 包编号：`{packet['packet_id']}`",
        f"- 状态：**{packet['status']}**",
        f"- Pending 条目：**{packet['pending_count']}**",
        f"- 审查批次：**{packet['batch_count']}**（每批最多 {packet['batch_size']} 条）",
        f"- 引用缺口：**{packet['reference_gap_count']}**",
        f"- 已有本地文件型复核入口：**{packet['decision_evidence_file_ready_count']}** 条",
        "- 所有条目初始决定均为 `Pending`；只有责任审核者提供真实证据后，才能调用单条决定工具。",
        "",
        "## 填写规则",
        "",
        "1. 每条记录单独填写责任审核者、日期、决定依据和新增证据文件。",
        "2. `ApprovedWithWarning` 必须同时填写警告、展示级别和跳过策略；敏感条目不能直接 `Approved`。",
        "3. 目录引用只是上下文入口，不等同于决定证据；写回工具要求新增证据引用最终解析到文件。",
        "4. 不修改 `historical_tag`、`source`、`license`、`authorization`、`asset_state` 或玩法数值。",
        "",
        "## 批次清单",
        "",
        "| 批次 | 范围 | 条目数 | 首条 ID | 末条 ID |",
        "|---|---|---:|---|---|",
    ]
    for batch in packet["batches"]:
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

    lines.extend(["", "## 逐条填写表", ""])
    for record in packet["records"]:
        template = record["decision_template"]
        lines.extend(
            [
                f"### `{record['id']}`",
                "",
                f"- 批次：`{record['batch_id']}`；范围：`{record['scope']}`",
                f"- Registry 文件：`{record['path']}`",
                f"- 当前标签：`{_markdown_value(record['historical_tag'])}`；当前资产状态：`{_markdown_value(record['asset_state'])}`",
                f"- 现有来源入口：`{_markdown_value(record['source_refs'])}`",
                f"- 现有复核入口：`{_markdown_value(record['review_evidence_refs'])}`",
                f"- 决定证据状态：`{record['decision_evidence_status']}`",
                f"- 必须决定：`{_markdown_value(record['required_decisions'])}`",
                "",
                "| 字段 | 待填写值 |",
                "|---|---|",
                f"| decision | `{template['decision']}` |",
                f"| reviewer | `{template['reviewer']}` |",
                f"| review_date | `{template['review_date']}` |",
                f"| decision_basis | `{template['decision_basis']}` |",
                f"| additional_evidence_refs | `{_markdown_value(template['additional_evidence_refs'])}` |",
            ]
        )
        for field in ("rights_holder_and_license", "commercial_use_and_modification", "presentation_levels", "warning_ids", "skip_allowed"):
            if field in template:
                lines.append(f"| {field} | `{_markdown_value(template[field])}` |")
        lines.extend(
            [
                f"| notes | `{template['notes']}` |",
                "",
                "---",
                "",
            ]
        )
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Build a read-only D16-A04 review packet")
    parser.add_argument("root", type=Path)
    parser.add_argument("--json-out", type=Path)
    parser.add_argument("--markdown-out", type=Path)
    parser.add_argument("--batch-size", type=int, default=25)
    parser.add_argument("--captured-at", default=date.today().isoformat())
    args = parser.parse_args()

    packet = build_packet(args.root, batch_size=args.batch_size, captured_at=args.captured_at)
    output = json.dumps(packet, ensure_ascii=False, indent=2) + "\n"
    print(output, end="")
    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        args.json_out.write_text(output, encoding="utf-8")
    if args.markdown_out:
        args.markdown_out.parent.mkdir(parents=True, exist_ok=True)
        args.markdown_out.write_text(render_markdown(packet), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
