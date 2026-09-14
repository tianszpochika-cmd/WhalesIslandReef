#!/usr/bin/env python3
"""Generate the D25-D31 visual/runtime task dashboard from result JSON files."""

from __future__ import annotations

import json
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "docs" / "game" / "VISUAL_TASK_STATUS_D25_D31.md"
RESULTS = ROOT / "game" / "tools" / "results"
HANDOFFS = ROOT / "docs" / "game" / "design" / "common" / "handoffs"

TASKS: list[tuple[str, str, str, str]] = [
    ("D25-S01", "S", "基线冻结与视觉债务清点", "D25-P02—P05"),
    ("D25-P02", "P", "统一视觉主题、材质和质量档", "D25-S06"),
    ("D25-P03", "P", "相机、天空、灯光和雾", "D25-S06"),
    ("D25-P04", "P", "玩家模式 HUD 清理和信息层级", "D25-S06"),
    ("D25-P05", "P", "资源和 Prefab 视觉准入验证器", "D25-S06"),
    ("D25-S06", "S", "视觉基础正式场景集成", "D25-G07"),
    ("D25-G07", "G", "视觉基础汇合门禁", "D26/D27/D28/D29"),
    ("D26-P01", "P", "自然地形和海岸层次", "D26-S05"),
    ("D26-P02", "P", "港口建筑和功能轮廓", "D26-S05"),
    ("D26-P03", "P", "植被、岩石和风响应", "D26-S05"),
    ("D26-P04", "P", "港口道具和生活痕迹", "D26-S05"),
    ("D26-S05", "S", "环境正式场景集成", "D26-G06"),
    ("D26-G06", "G", "环境视觉汇合门禁", "D27/D28/D29"),
    ("D27-P01", "P", "玩家身体、发型、服装和手持物", "D27-S05"),
    ("D27-P02", "P", "岗位可读 NPC", "D27-S05"),
    ("D27-P03", "P", "动物、宠物和水生生物", "D27-S05"),
    ("D27-P04", "P", "动画、锚点、LOD 和表现回退", "D27-S05"),
    ("D27-S05", "S", "角色和动物运行时集成", "D27-G06"),
    ("D27-G06", "G", "角色与动物视觉门禁", "D28/D29"),
    ("D28-P01", "P", "船体、甲板、桅杆、帆和锚", "D28-S05"),
    ("D28-P02", "P", "海水、泡沫、浅水和礁石", "D28-S05"),
    ("D28-P03", "P", "Calm、Rain、Storm 天气表现", "D28-S05"),
    ("D28-P04", "P", "天空、远景、雾和后处理", "D28-S05"),
    ("D28-S05", "S", "船海天气正式集成", "D28-G06"),
    ("D28-G06", "G", "船海天气汇合门禁", "D29/D30"),
    ("D29-P01", "P", "主 HUD 和状态布局", "D29-S05"),
    ("D29-P02", "P", "市场、仓库、酒馆、船坞等面板", "D29-S05"),
    ("D29-P03", "P", "按钮、焦点和命令桥接", "D29-S05"),
    ("D29-P04", "P", "本地化、缩放和可访问性", "D29-S05"),
    ("D29-S05", "S", "正式 UI Canvas 集成", "D29-G06"),
    ("D29-G06", "G", "UI 交互汇合门禁", "D30"),
    ("D30-P01", "P", "逻辑状态到视觉反馈桥", "D30-S05"),
    ("D30-P02", "P", "相机过渡和流程表现", "D30-S05"),
    ("D30-P03", "P", "保存加载后的视觉恢复", "D30-S05"),
    ("D30-P04", "P", "音效、环境声、粒子和动作反馈", "D30-S05"),
    ("D30-S05", "S", "视觉可玩完整路线", "D30-G06"),
    ("D30-G06", "G", "视觉可玩汇合门禁", "D31"),
    ("D31-P01", "P", "LOD、遮挡、批处理和对象池", "D31-S05"),
    ("D31-P02", "P", "贴图、Shader、阴影和粒子预算", "D31-S05"),
    ("D31-P03", "P", "1080p 三档画质与性能采样", "D31-S05"),
    ("D31-P04", "P", "固定镜头、天气和 UI 回归", "D31-S05"),
    ("D31-S05", "S", "Visual Candidate 正式整合", "D31-G06"),
    ("D31-G06", "G", "视觉与交互候选版总验收", "后续 Runtime/Release 门禁"),
]

VALID = {
    "PLANNED",
    "IN_PROGRESS",
    "PASS",
    "ART_PASS",
    "SCENE_PASS",
    "RUNTIME_PASS",
    "PARTIAL",
    "BLOCKED",
    "VISUAL_CANDIDATE",
}


def result_path(task_id: str) -> Path:
    chapter = task_id.split("-")[0]
    return RESULTS / chapter / task_id / f"{task_id.replace('-', '_')}_RESULT.json"


def load_result(task_id: str) -> dict[str, str]:
    path = result_path(task_id)
    if not path.exists():
        return {"status": "PLANNED", "updated_at": "—", "summary": "尚未提交结果", "evidence": "—"}
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        return {
            "status": "BLOCKED",
            "updated_at": "—",
            "summary": f"结果 JSON 不可读取：{exc}",
            "evidence": path.relative_to(ROOT).as_posix(),
        }
    status = str(data.get("status", "BLOCKED"))
    if data.get("task_id") != task_id or status not in VALID:
        status = "BLOCKED"
    summary = str(data.get("summary", "缺少 summary")).replace("|", "/").replace("\n", " ")
    return {
        "status": status,
        "updated_at": str(data.get("updated_at", "—")),
        "summary": summary,
        "evidence": path.relative_to(ROOT).as_posix(),
    }


def display_status(status: str) -> str:
    return status


def main() -> int:
    if len({task_id for task_id, _, _, _ in TASKS}) != len(TASKS):
        raise SystemExit("视觉任务清单含重复 task_id")

    records = [(task_id, mode, summary, next_task, load_result(task_id)) for task_id, mode, summary, next_task in TASKS]
    lines = [
        "# D25—D31 视觉与交互任务状态",
        "",
        "> 自动生成：`python tools/sync_visual_task_status.py`。每个任务的唯一事实源是其 `*_RESULT.json`。",
        "> 本看板不覆盖 D18—D24 逻辑状态，也不改变 D16/D17 发布门禁。",
        "",
        f"> 最后同步（UTC）：{datetime.now(timezone.utc).isoformat(timespec='seconds')}",
        "",
        "| 章节 | 计划任务 | PASS/ART_PASS | SCENE_PASS | RUNTIME_PASS | PARTIAL | BLOCKED | IN_PROGRESS | PLANNED |",
        "|---|---:|---:|---:|---:|---:|---:|---:|---:|",
    ]
    for chapter in ("D25", "D26", "D27", "D28", "D29", "D30", "D31"):
        chapter_records = [record for record in records if record[0].startswith(chapter + "-")]
        counts = Counter(record[4]["status"] for record in chapter_records)
        pass_count = counts["PASS"] + counts["ART_PASS"]
        lines.append(
            f"| {chapter} | {len(chapter_records)} | {pass_count} | {counts['SCENE_PASS']} | {counts['RUNTIME_PASS']} | {counts['PARTIAL']} | {counts['BLOCKED']} | {counts['IN_PROGRESS']} | {counts['PLANNED']} |"
        )

    lines.extend(["", "## 任务明细", "", "| 任务 | 模式 | 工作内容 | 状态 | 更新时间 | 结果 JSON | 下一个任务 |", "|---|---|---|---|---|---|---|"])
    for task_id, mode, summary, next_task, result in records:
        evidence = result["evidence"]
        evidence_cell = "—" if evidence == "—" else f"[{Path(evidence).name}](../../{evidence})"
        lines.append(f"| {task_id} | {mode} | {summary} | {display_status(result['status'])} | {result['updated_at']} | {evidence_cell} | {next_task} |")

    lines.extend(["", "## 规则", "", "- P 任务只写独立工作目录/Prefab/材质/测试；S 任务是正式场景或正式 Canvas 的唯一写者。", "- G 任务只审计和汇合，不得顺手修改其他章节。", "- `RUNTIME_PASS` 必须来自当前 Player 的真实运行过程；编辑器预览、静态计数和编译成功不能替代它。", "- `VISUAL_CANDIDATE` 不等于 `RuntimeApproved` 或 `ReleaseApproved`。", ""])
    OUTPUT.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {OUTPUT.relative_to(ROOT)} for {len(records)} tasks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
