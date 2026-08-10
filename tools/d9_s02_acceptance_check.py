"""D9-S02 vertical-slice acceptance gate.

This is a read-only integration gate.  It runs the frozen-slice validator,
checks that each D9 agent handed off its promised integration entry, and
audits the production seams that make the end-to-end flow safe to run.  It
does not manufacture domain events or emulate missing domain owners: a green
result must mean that the real systems can execute the slice.
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path
from typing import Dict, Iterable, List


REPOSITORY_ROOT = Path(__file__).resolve().parents[1]
FREEZE_PATH = REPOSITORY_ROOT / "docs" / "game" / "design" / "common" / "D9_VERTICAL_SLICE_FREEZE.md"
HANDOFF_ROOT = REPOSITORY_ROOT / "docs" / "game" / "design" / "common" / "handoffs" / "D9"
SYSTEMS_ROOT = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems"

FROZEN_EVENTS = (
    "event_d9_departure_prepared",
    "event_d9_departure",
    "event_d9_fishing_catch",
    "event_d9_weather_storm",
    "event_d9_crew_report",
    "event_d9_nearshore_patrol",
    "event_d9_nearshore_retreat",
    "event_d9_treasure_clue",
    "event_d9_treasure_resolved",
    "event_d9_returned_to_harbor",
    "event_d9_trade_settled",
    "event_d9_wage_settled",
    "event_d9_ship_repaired",
    "event_d9_save_checkpoint",
)

FLOW_STAGES = (
    ("整备", "D9-A01", "D9PortShipPreparationSystem", "港口、泊位、补给、修船、招募、工资和离港测试入口"),
    ("装货", "D9-A01", "D9PortShipPreparationSystem.Buy", "买货后由整备 owner 反映重量/货舱变化"),
    ("配员", "D9-A01", "D9PortShipPreparationSystem.Recruit", "船员名册与工资由船舶整备/船员 owner 修改"),
    ("出航", "D9-A01", "D3RuntimeBootstrap.RequestDeparture", "出港后 D3 航行快照为 Sailing"),
    ("钓鱼", "D9-A02", "nearshore_saint_marys / fish_mackerel_school / gear_handline_basic", "需要 A02 的 EcologySystem 入口和捕获事实"),
    ("遇雨/风暴", "D9-A05", "event_d9_weather_storm", "需要 EnvironmentSystem 写入统一天气事实"),
    ("船员讨论", "D9-A04", "D2 AgentCore + event_d9_crew_report", "AgentCore 只能产出已校验意图，CrewSystem 必须写事实"),
    ("登陆寻宝", "D9-A06", "quest_first_chart / landmark:saint_marys_chart_cove", "需要 QuestSystem、奖励请求和寻宝事实"),
    ("返回交易", "D9-A03", "event_d9_returned_to_harbor / event_d9_trade_settled", "交易/返港结果须由所属强逻辑系统写入 EventLog"),
    ("发工资", "D9-A01", "event_d9_wage_settled", "工资数值仍由整备/经济 owner 裁决，日志只记录已裁决事实"),
    ("修船", "D9-A01", "event_d9_ship_repaired", "材料、金钱和船体变化须由 ShipyardSystem/整备入口裁决"),
    ("保存", "D8-S02", "EventLog.CaptureSnapshot + SaveSystem", "事实和 settlement claim 必须与实体快照一起落盘"),
    ("读档", "D8-S02", "EventLog.Restore + D9EventFactCursor", "恢复后必须跳过历史事实，不重复 UI/音频/结算"),
)


def _read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


def _result(check_id: str, status: str, owner: str, evidence: str, fix: str = "") -> Dict[str, str]:
    return {
        "id": check_id,
        "status": status,
        "owner": owner,
        "evidence": evidence,
        "fix": fix,
    }


def _all_cs(root: Path = REPOSITORY_ROOT) -> Iterable[Path]:
    return (root / "game" / "Assets" / "Scripts").rglob("*.cs")


def _production_event_producers(event_id: str) -> List[str]:
    """Find actual EventLog.Append sites, excluding tests and presentation docs."""
    matches: List[str] = []
    append_pattern = re.compile(r"\b(?:EventLog|[A-Za-z0-9_]+)\s*\.\s*Append\s*\(")
    for path in SYSTEMS_ROOT.rglob("*.cs"):
        text = _read(path)
        if event_id not in text or not append_pattern.search(text):
            continue
        matches.append(str(path.relative_to(REPOSITORY_ROOT)))
    return matches


def _content_registry_ids() -> set[str]:
    registry_root = REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"
    ids: set[str] = set()
    if not registry_root.is_dir():
        return ids
    for path in registry_root.rglob("*.json"):
        try:
            value = json.loads(_read(path))
        except json.JSONDecodeError:
            continue
        if isinstance(value, dict) and value.get("id"):
            ids.add(value["id"])
    return ids


def _content_logic_refs() -> set[str]:
    """Return logical runtime aliases declared by formal D8 content entries."""
    registry_root = REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries"
    refs: set[str] = set()
    if not registry_root.is_dir():
        return refs
    for path in registry_root.rglob("*.json"):
        try:
            value = json.loads(_read(path))
        except json.JSONDecodeError:
            continue
        if isinstance(value, dict) and value.get("logic_ref"):
            refs.add(value["logic_ref"])
    return refs


def _handoff_path(task_id: str) -> Path:
    return HANDOFF_ROOT / f"{task_id}_HANDOFF.md"


def _redline_errors() -> List[str]:
    patterns = re.compile(r"\b(?:HttpClient|UnityWebRequest|WebRequest|LLMClient|Ollama|DeepSeek)\b|\bSystem\.Net\b|https?://")
    errors: List[str] = []
    for path in SYSTEMS_ROOT.rglob("*.cs"):
        block_comment = False
        for line_number, line in enumerate(_read(path).splitlines(), 1):
            line_without_comments = line
            if "/*" in line_without_comments:
                block_comment = True
            if block_comment:
                if "*/" in line_without_comments:
                    block_comment = False
                continue
            line_without_comments = line_without_comments.split("//", 1)[0]
            if patterns.search(line_without_comments):
                errors.append(f"{path.relative_to(REPOSITORY_ROOT)}:{line_number}")
    return errors


def run_checks() -> Dict[str, object]:
    checks: List[Dict[str, str]] = []

    freeze_process = subprocess.run(
        [sys.executable, str(REPOSITORY_ROOT / "tools" / "d9_vertical_slice_check.py")],
        cwd=REPOSITORY_ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    freeze_output = (freeze_process.stdout + freeze_process.stderr).strip().replace("\n", " | ")
    checks.append(_result(
        "freeze_manifest",
        "PASS" if freeze_process.returncode == 0 else "BLOCKED",
        "D9-S01",
        freeze_output or "d9_vertical_slice_check produced no output",
        "修复 D9-S01 冻结文件后重新运行。" if freeze_process.returncode else "",
    ))

    required_handoffs = [f"D9-A0{i}" for i in range(1, 8)]
    missing_handoffs = [task for task in required_handoffs if not _handoff_path(task).is_file()]
    checks.append(_result(
        "handoff_completeness",
        "PASS" if not missing_handoffs else "BLOCKED",
        ", ".join(missing_handoffs) if missing_handoffs else "D9-A01..D9-A07",
        "全部 A 任务交接报告存在" if not missing_handoffs else "缺失交接报告：" + ", ".join(missing_handoffs),
        "先完成缺失 A 任务并补齐交接报告，再由 D9-S02 合并验收。" if missing_handoffs else "",
    ))

    a01_test = REPOSITORY_ROOT / "game" / "Assets" / "Tests" / "EditMode" / "D9PortShipPreparationTests.cs"
    a01_system = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D9" / "D9PortShipPreparationSystem.cs"
    a01_ok = a01_test.is_file() and a01_system.is_file()
    checks.append(_result(
        "prep_domain_baseline",
        "PASS" if a01_ok else "BLOCKED",
        "D9-A01",
        "D9PortShipPreparationSystem + D9PortShipPreparationTests" if a01_ok else "D9-A01 entry/test missing",
        "恢复 D9-A01 入口和测试。" if not a01_ok else "",
    ))

    commodity_catalog = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "D3" / "CommodityCatalog.cs"
    commodity_count = len(re.findall(r'new CommodityDefinition\("', _read(commodity_catalog))) if commodity_catalog.is_file() else 0
    checks.append(_result(
        "commodity_sample",
        "PASS" if commodity_count == 10 else "BLOCKED",
        "D9-A03",
        f"CommodityCatalog contains {commodity_count} commodity definitions",
        "冻结并注册十种商品后重新运行。" if commodity_count != 10 else "",
    ))

    agent_core_ok = (
        (REPOSITORY_ROOT / "game" / "AgentCore" / "AgentReplyValidator.cs").is_file()
        and (REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "UI" / "Dialogue" / "D2DialogueController.cs").is_file()
    )
    checks.append(_result(
        "agent_dialogue_baseline",
        "PASS" if agent_core_ok else "BLOCKED",
        "D9-A04",
        "AgentReplyValidator + D2DialogueController present" if agent_core_ok else "AgentCore/D2 dialogue entry missing",
        "恢复 AgentCore/D2 对话入口。" if not agent_core_ok else "",
    ))

    ids = _content_registry_ids()
    logic_refs = _content_logic_refs()
    fish_refs_ok = {"fish_mackerel_school", "item_gear_handline_basic"}.issubset(ids) and "gear_handline_basic" in logic_refs
    checks.append(_result(
        "fishing_entry",
        "PASS" if fish_refs_ok and _production_event_producers("event_d9_fishing_catch") else "BLOCKED",
        "D9-A02",
        "content IDs=" + ", ".join(sorted({"fish_mackerel_school", "item_gear_handline_basic"} & ids))
        + "; logic refs=" + ", ".join(sorted({"gear_handline_basic"} & logic_refs))
        + "; producers=" + ", ".join(_production_event_producers("event_d9_fishing_catch")),
        "补齐 gear_handline_basic、近海鱼场 EcologySystem 入口，并由该 owner 写 event_d9_fishing_catch。"
        if not (fish_refs_ok and _production_event_producers("event_d9_fishing_catch")) else "",
    ))

    storm_producers = _production_event_producers("event_d9_weather_storm")
    checks.append(_result(
        "weather_event_entry",
        "PASS" if storm_producers else "BLOCKED",
        "D9-A05",
        "event_d9_weather_storm producers=" + ", ".join(storm_producers),
        "实现 EnvironmentSystem 的天气事件入口并写入统一 EventLog；不要由 HUD 或 D3 表现层补写。" if not storm_producers else "",
    ))

    quest_refs_ok = {"quest_first_chart", "reward_first_chart"}.issubset(ids)
    quest_entry_paths = list((REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry" / "entries" / "quest").glob("quest_first_chart.json"))
    landmark_registered = any("landmark:saint_marys_chart_cove" in _read(path) for path in quest_entry_paths)
    quest_files = list((REPOSITORY_ROOT / "game" / "Assets" / "Scripts").rglob("*QuestSystem*.cs"))
    treasure_producers = _production_event_producers("event_d9_treasure_resolved")
    checks.append(_result(
        "treasure_entry",
        "PASS" if quest_refs_ok and landmark_registered and quest_files and treasure_producers else "BLOCKED",
        "D9-A06",
        f"quest/reward IDs={quest_refs_ok}; logical landmark ref={landmark_registered}; quest files={len(quest_files)}; producers={treasure_producers}",
        "补齐 landmark:saint_marys_chart_cove、QuestSystem/RewardRequest 和两条寻宝事实，再接入 D9。"
        if not (quest_refs_ok and landmark_registered and quest_files and treasure_producers) else "",
    ))

    missing_event_producers = {
        event_id: _production_event_producers(event_id)
        for event_id in FROZEN_EVENTS
        if not _production_event_producers(event_id)
    }
    checks.append(_result(
        "event_owner_chain",
        "PASS" if not missing_event_producers else "BLOCKED",
        "D9-A01..D9-A06 / D8 事件运行时接线",
        "all 14 frozen events have production EventLog.Append sites" if not missing_event_producers
        else "无生产 EventLog.Append 的冻结事件：" + ", ".join(sorted(missing_event_producers)),
        "由冻结 owner 各自写入事实，并由 D8 事件运行时统一持有 EventLog；UI、AgentCore、gateway 不得成为 owner。" if missing_event_producers else "",
    ))

    save_system = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Systems" / "Save" / "SaveSystem.cs"
    save_text = _read(save_system) if save_system.is_file() else ""
    event_save_ok = "EventLogSnapshot" in save_text and "CaptureSnapshot" in save_text and "Restore" in save_text
    checks.append(_result(
        "event_log_save_restore",
        "PASS" if event_save_ok else "BLOCKED",
        "D8-S02 / SaveSystem",
        "SaveSystem persists EventLogSnapshot" if event_save_ok else "SaveSystem has no EventLogSnapshot save/load path",
        "把 EventLog facts、NextSequence 和 settled_keys 纳入同一存档版本，并在读档时 Restore 后再绑定 D9EventLogBridge。"
        if not event_save_ok else "",
    ))

    bridge_file = REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D9" / "D9PresentationReadModel.cs"
    bridge_text = _read(bridge_file) if bridge_file.is_file() else ""
    bridge_bind_count = len(re.findall(r"\b(?:public\s+static\s+)?void\s+Bind\s*\(", bridge_text))
    runtime_bindings = [
        str(path.relative_to(REPOSITORY_ROOT))
        for path in _all_cs()
        if path != bridge_file and "D9EventLogBridge.Bind(" in _read(path)
        and "Tests" not in str(path)
    ]
    checks.append(_result(
        "presentation_event_binding",
        "PASS" if bridge_file.is_file() and runtime_bindings else "BLOCKED",
        "D9-A07 / D8 事件运行时接线",
        f"read-only bridge={bridge_file.is_file()}, bridge declarations={bridge_bind_count}, runtime bindings={runtime_bindings}",
        "在当前/恢复后的统一 EventLog 创建完成后调用 D9EventLogBridge.Bind；不要由 UI new EventLog。"
        if not (bridge_file.is_file() and runtime_bindings) else "",
    ))

    redline = _redline_errors()
    checks.append(_result(
        "systems_redline",
        "PASS" if not redline else "BLOCKED",
        "架构铁律 / 责任模块见路径",
        "Systems contains no network/LLM references" if not redline else "red-line references: " + ", ".join(redline),
        "删除 Systems 中网络/LLM 依赖并将调用移到允许的边界。" if redline else "",
    ))

    agentcore_dependency_errors: List[str] = []
    forbidden_agentcore = re.compile(r"\b(?:UnityEngine|UnityEditor|Newtonsoft|Json\.NET)\b")
    agentcore_root = REPOSITORY_ROOT / "game" / "AgentCore"
    for path in agentcore_root.rglob("*.cs"):
        for line_number, line in enumerate(_read(path).splitlines(), 1):
            if forbidden_agentcore.search(line.split("//", 1)[0]):
                agentcore_dependency_errors.append(f"{path.relative_to(REPOSITORY_ROOT)}:{line_number}")
    checks.append(_result(
        "agentcore_dependency_boundary",
        "PASS" if not agentcore_dependency_errors else "BLOCKED",
        "D8-A04 / AgentCore",
        "AgentCore contains no Unity or third-party dependency" if not agentcore_dependency_errors
        else "forbidden dependencies: " + ", ".join(agentcore_dependency_errors),
        "将 Unity/第三方引用移出 AgentCore；System.Net.Http 等 .NET 基础库不属于本项禁止范围。" if agentcore_dependency_errors else "",
    ))

    presentation_files = [
        REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D9" / "D9PresentationReadModel.cs",
        REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D9" / "D9AudioPresentation.cs",
        REPOSITORY_ROOT / "game" / "Assets" / "Scripts" / "Gameplay" / "D9" / "D9VoyageHudController.cs",
    ]
    presentation_write_errors: List[str] = []
    for path in presentation_files:
        if not path.is_file():
            presentation_write_errors.append(f"missing:{path.relative_to(REPOSITORY_ROOT)}")
            continue
        text = _read(path)
        if re.search(r"\b(?:EventLog\.Append|ClaimSettlement|new\s+EventLog)\s*", text):
            presentation_write_errors.append(str(path.relative_to(REPOSITORY_ROOT)))
    checks.append(_result(
        "presentation_write_boundary",
        "PASS" if not presentation_write_errors else "BLOCKED",
        "D9-A07 / UI、音频、日志",
        "D9 presentation files only read snapshots/facts" if not presentation_write_errors
        else "presentation write references: " + ", ".join(presentation_write_errors),
        "移除表现层对 EventLog/settlement/domain writer 的写入；状态修改必须回到所属 Systems。" if presentation_write_errors else "",
    ))

    flow = []
    check_status = {check["id"]: check["status"] for check in checks}
    baseline_stages = {"整备"}
    for stage, owner, entry, boundary in FLOW_STAGES:
        status = "PASS" if stage in baseline_stages and a01_ok else "BLOCKED"
        if stage in {"装货", "配员", "出航", "返回交易", "发工资", "修船"}:
            status = "PASS" if a01_ok and check_status.get("event_owner_chain") == "PASS" else "BLOCKED"
        elif stage == "钓鱼":
            status = "PASS" if check_status.get("fishing_entry") == "PASS" else "BLOCKED"
        elif stage == "遇雨/风暴":
            status = "PASS" if check_status.get("weather_event_entry") == "PASS" else "BLOCKED"
        elif stage == "船员讨论":
            status = "PASS" if agent_core_ok and check_status.get("event_owner_chain") == "PASS" else "BLOCKED"
        elif stage == "登陆寻宝":
            status = "PASS" if check_status.get("treasure_entry") == "PASS" else "BLOCKED"
        elif stage == "保存":
            status = "PASS" if event_save_ok else "BLOCKED"
        elif stage == "读档":
            status = "PASS" if event_save_ok and runtime_bindings and check_status.get("event_owner_chain") == "PASS" else "BLOCKED"
        flow.append({"stage": stage, "owner": owner, "entry": entry, "status": status, "boundary": boundary})
    checks.append(_result(
        "full_vertical_slice_flow",
        "PASS" if all(step["status"] == "PASS" for step in flow) else "BLOCKED",
        "D9-S02",
        " → ".join(f'{step["stage"]}:{step["status"]}' for step in flow),
        "按 flow 中标明的 owner 修复后，重新执行真实运行时整合测试。" if any(step["status"] != "PASS" for step in flow) else "",
    ))

    blocked = [check for check in checks if check["status"] == "BLOCKED"]
    return {
        "task_id": "D9-S02",
        "status": "PASS" if not blocked else "BLOCKED",
        "checks": checks,
        "flow": flow,
        "blocked_count": len(blocked),
        "frozen_event_count": len(FROZEN_EVENTS),
    }


def main(argv: List[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Run the D9-S02 read-only vertical-slice acceptance gate.")
    parser.add_argument("--json", action="store_true", help="print machine-readable JSON")
    args = parser.parse_args(argv)
    report = run_checks()
    if args.json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
    else:
        for check in report["checks"]:
            print(f'{check["status"]} {check["id"]} [{check["owner"]}] {check["evidence"]}')
            if check["status"] == "BLOCKED":
                print(f'  FIX {check["fix"]}')
        print(f'D9-S02 {report["status"]}: blocked={report["blocked_count"]}, frozen_events={report["frozen_event_count"]}')
    return 0 if report["status"] == "PASS" else 2


if __name__ == "__main__":
    sys.exit(main())
