#!/usr/bin/env python3
"""Build the D16/D17 remaining-gate dashboard from per-task GATE_RESULT JSON files."""

from __future__ import annotations

import json
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
INDEX = ROOT / "docs" / "game" / "AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md"
OUTPUT = ROOT / "docs" / "game" / "GATE_TASK_STATUS.md"
RESULTS = ROOT / "game" / "tools" / "results"
VALID = {"PLANNED", "IN_PROGRESS", "GATE_PASS", "GATE_PARTIAL", "GATE_BLOCKED"}
# 2026-08-17 merge: only D16-C01—C12 and D17-C01—C04 remain assignable.
EXPECTED_IDS = [f"D16-C{n:02d}" for n in range(1, 13)] + [f"D17-C{n:02d}" for n in range(1, 5)]


def task_ids() -> list[str]:
    if not INDEX.exists():
        raise SystemExit(f"缺少任务卡：{INDEX}")
    return list(EXPECTED_IDS)


def load_result(task_id: str) -> dict[str, str]:
    chapter = task_id.split("-")[0]
    path = RESULTS / chapter / task_id / f"{task_id.replace('-', '_')}_GATE_RESULT.json"
    if not path.exists():
        return {"status": "PLANNED", "updated_at": "—", "summary": "尚未提交结果", "evidence": "—"}
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError) as exc:
        return {
            "status": "GATE_BLOCKED",
            "updated_at": "—",
            "summary": f"结果文件不可读取：{exc}",
            "evidence": str(path.relative_to(ROOT)).replace("\\", "/"),
        }
    if data.get("task_id") != task_id:
        return {
            "status": "GATE_BLOCKED",
            "updated_at": str(data.get("updated_at", "—")),
            "summary": f"结果 task_id 不匹配：{data.get('task_id')!r}",
            "evidence": str(path.relative_to(ROOT)).replace("\\", "/"),
        }
    status = str(data.get("status", "GATE_BLOCKED"))
    if status not in VALID:
        status = "GATE_BLOCKED"
    return {
        "status": status,
        "updated_at": str(data.get("updated_at", "—")),
        "summary": str(data.get("summary", "缺少 summary")).replace("|", "/").replace("\n", " "),
        "evidence": str(path.relative_to(ROOT)).replace("\\", "/"),
    }


def main() -> int:
    tasks = task_ids()
    if tasks != EXPECTED_IDS:
        raise SystemExit(f"门禁任务应为 {len(EXPECTED_IDS)} 项，当前为 {len(tasks)}。")
    grouped: dict[str, list[tuple[str, dict[str, str]]]] = defaultdict(list)
    for task_id in tasks:
        grouped[task_id.split("-")[0]].append((task_id, load_result(task_id)))

    lines = [
        "# D16 / D17 剩余门禁任务状态总览",
        "",
        "> 自动生成：`python tools/sync_gate_task_status.py`。每个任务的唯一事实源是其 `*_GATE_RESULT.json`；不要手动编辑本文件。",
        "",
        "> 本看板独立于 [逻辑任务状态总览](LOGIC_TASK_STATUS.md)。D18—D24 的 `LOGIC_*` 不能改写这里的 `GATE_*`。",
        "",
        f"> 最后同步（UTC）：{datetime.now(timezone.utc).isoformat(timespec='seconds')}",
        "",
        "| 章节 | 计划任务 | GATE_PASS | PARTIAL | BLOCKED | 进行中 | 未开始 |",
        "|---|---:|---:|---:|---:|---:|---:|",
    ]
    for chapter in ("D16", "D17"):
        counts = Counter(result["status"] for _, result in grouped[chapter])
        lines.append(
            f"| {chapter} | {len(grouped[chapter])} | {counts['GATE_PASS']} | {counts['GATE_PARTIAL']} | {counts['GATE_BLOCKED']} | {counts['IN_PROGRESS']} | {counts['PLANNED']} |"
        )
    for chapter in ("D16", "D17"):
        lines.extend(["", f"## {chapter}", "", "| 任务 | 状态 | 更新时间 | 结果文件 | 摘要 |", "|---|---|---|---|---|"])
        for task_id, result in grouped[chapter]:
            evidence = result["evidence"]
            evidence_cell = "—" if evidence == "—" else f"[{Path(evidence).name}](../../{evidence})"
            lines.append(
                f"| {task_id} | {result['status']} | {result['updated_at']} | {evidence_cell} | {result['summary']} |"
            )
    OUTPUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {OUTPUT.relative_to(ROOT)} for {len(tasks)} tasks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
