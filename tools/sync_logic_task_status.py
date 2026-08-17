#!/usr/bin/env python3
"""Build the human-readable D18-D24 logic task dashboard from per-task result JSON files."""

from __future__ import annotations

import json
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
INDEX = ROOT / "docs" / "game" / "AGENT_DEVELOPMENT_TASKS.md"
OUTPUT = ROOT / "docs" / "game" / "LOGIC_TASK_STATUS.md"
RESULTS = ROOT / "game" / "tools" / "results"
VALID = {"PLANNED", "IN_PROGRESS", "LOGIC_PASS", "LOGIC_PARTIAL", "LOGIC_BLOCKED", "RUNTIME_DEFERRED"}
# D18-L51—L53 are the documented corrective closure for the first D18
# acceptance attempt. They remain ordinary L tasks so the dashboard has one
# complete, auditable task set rather than a hidden side-channel of reruns.
EXPECTED_COUNTS = {"D18": 53, "D19": 48, "D20": 40, "D21": 32, "D22": 36, "D23": 38, "D24": 36}


def task_ids() -> list[str]:
    """Return the frozen task set, without treating abbreviated index ranges as task IDs."""
    if not INDEX.exists():
        raise SystemExit(f"缺少任务索引：{INDEX}")
    return [
        f"{chapter}-L{number:02d}"
        for chapter, count in EXPECTED_COUNTS.items()
        for number in range(1, count + 1)
    ]


def load_result(task_id: str) -> dict[str, str]:
    chapter = task_id.split("-")[0]
    path = RESULTS / chapter / task_id / f"{task_id.replace('-', '_')}_LOGIC_RESULT.json"
    if not path.exists():
        return {"status": "PLANNED", "updated_at": "—", "summary": "尚未提交结果", "evidence": "—"}
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError) as exc:
        return {"status": "LOGIC_BLOCKED", "updated_at": "—", "summary": f"结果文件不可读取：{exc}", "evidence": str(path.relative_to(ROOT)).replace("\\", "/")}
    if data.get("task_id") != task_id:
        return {
            "status": "LOGIC_BLOCKED",
            "updated_at": str(data.get("updated_at", "—")),
            "summary": f"结果 task_id 不匹配：{data.get('task_id')!r}",
            "evidence": str(path.relative_to(ROOT)).replace("\\", "/"),
        }
    status = str(data.get("status", "LOGIC_BLOCKED"))
    if status not in VALID:
        status = "LOGIC_BLOCKED"
    return {
        "status": status,
        "updated_at": str(data.get("updated_at", "—")),
        "summary": str(data.get("summary", "缺少 summary")).replace("|", "/").replace("\n", " "),
        "evidence": str(path.relative_to(ROOT)).replace("\\", "/"),
    }


def main() -> int:
    tasks = task_ids()
    expected_total = sum(EXPECTED_COUNTS.values())
    if len(tasks) != expected_total:
        raise SystemExit(f"任务索引应包含 {expected_total} 个唯一 L 任务，当前为 {len(tasks)}；请先修复索引。")
    grouped: dict[str, list[tuple[str, dict[str, str]]]] = defaultdict(list)
    for task_id in tasks:
        grouped[task_id.split("-")[0]].append((task_id, load_result(task_id)))

    lines = [
        "# D18—D24 逻辑任务状态总览",
        "",
        "> 自动生成：`python tools/sync_logic_task_status.py`。每个任务的唯一事实源是其 `*_LOGIC_RESULT.json`；不要手动编辑本文件。",
        "",
        f"> 最后同步（UTC）：{datetime.now(timezone.utc).isoformat(timespec='seconds')}",
        "",
        "| 章节 | 计划任务 | LOGIC_PASS | PARTIAL | BLOCKED | Runtime 延期 | 进行中 | 未开始 |",
        "|---|---:|---:|---:|---:|---:|---:|---:|",
    ]
    for chapter in sorted(grouped, key=lambda value: int(value[1:])):
        counts = Counter(result["status"] for _, result in grouped[chapter])
        lines.append(
            f"| {chapter} | {len(grouped[chapter])} | {counts['LOGIC_PASS']} | {counts['LOGIC_PARTIAL']} | {counts['LOGIC_BLOCKED']} | {counts['RUNTIME_DEFERRED']} | {counts['IN_PROGRESS']} | {counts['PLANNED']} |"
        )
    for chapter in sorted(grouped, key=lambda value: int(value[1:])):
        lines.extend(["", f"## {chapter}", "", "| 任务 | 状态 | 更新时间 | 结果文件 | 摘要 |", "|---|---|---|---|---|"])
        for task_id, result in grouped[chapter]:
            evidence = result["evidence"]
            evidence_cell = "—" if evidence == "—" else f"[{Path(evidence).name}](../../{evidence})"
            lines.append(f"| {task_id} | {result['status']} | {result['updated_at']} | {evidence_cell} | {result['summary']} |")
    OUTPUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {OUTPUT.relative_to(ROOT)} for {len(tasks)} tasks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
