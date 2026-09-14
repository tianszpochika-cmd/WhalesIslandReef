"""Static interaction-logic audit for the D31 visual interaction slice.

This audit proves only source-level wiring and focus ownership. It deliberately
does not turn a static result into Player/runtime approval.
"""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
MATRIX = ROOT / "docs/game/design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json"
FOCUS_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/UI/D16/D16InputRouter.cs"
D3_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs"
SURFACE_SOURCES = {
    "D29": ROOT / "game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs",
    "D4": ROOT / "game/Assets/Scripts/Gameplay/D4/D4RuntimeBootstrap.cs",
    "D9": ROOT / "game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs",
    "D10": ROOT / "game/Assets/Scripts/Gameplay/D10/D10TavernSocialBootstrap.cs",
}
D30_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D30/D30VisualLoopCoordinator.cs"
D30_POLICY_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs"
D29_POLICY_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D29/Logic/D29OneShotPolicy.cs"
PLAYER_PROBE_SOURCE = ROOT / "game/Assets/Scripts/Gameplay/D31/D31InteractionEvidenceProbe.cs"
FORBIDDEN = ("HttpClient", "UnityWebRequest", "System.Net.Http", "using System.Net")
MODAL_FOCUS_SURFACES = {"D29-Warehouse", "D4", "D9", "D10"}


def audit() -> dict[str, Any]:
    errors: list[str] = []
    matrix = json.loads(MATRIX.read_text(encoding="utf-8"))
    buttons = matrix.get("buttons", [])
    keyboard_only = matrix.get("keyboard_only_controls", [])
    focus_policy = matrix.get("focus_policy", {})

    if len(buttons) != 29:
        errors.append(f"expected 29 visible entries, got {len(buttons)}")
    if len(keyboard_only) != 5:
        errors.append(f"expected 5 keyboard-only entries, got {len(keyboard_only)}")
    quality_entries = [
        entry for entry in keyboard_only
        if entry.get("surface") == "D29" and entry.get("input") == "Q" and
        entry.get("callback") == "CycleQuality"
    ]
    if len(quality_entries) != 1:
        errors.append("D29 quality preview keyboard entry is missing or duplicated")
    expected_surfaces = {"D29", "D29-Warehouse", "D4", "D9"}
    actual_surfaces = {entry.get("surface") for entry in buttons}
    if actual_surfaces != expected_surfaces:
        errors.append(f"unexpected visible surfaces: {sorted(actual_surfaces)}")
    if focus_policy.get("implementation") != "D16ModalFocusScope":
        errors.append("matrix focus policy does not name D16ModalFocusScope")
    if set(focus_policy.get("surfaces", [])) != MODAL_FOCUS_SURFACES:
        errors.append("matrix focus policy surface set is incomplete")
    if focus_policy.get("runtime_evidence_required") is not True:
        errors.append("matrix must keep runtime focus evidence requirement enabled")
    availability_policy = matrix.get("availability_policy", {})
    if availability_policy.get("runtime_evidence_required") is not True:
        errors.append("matrix must keep runtime availability evidence requirement enabled")

    focus_text = FOCUS_SOURCE.read_text(encoding="utf-8-sig")
    for token in ("IsModalVisible", "ClearOwnedSelection", "GetComponentInParent<Canvas>", "CanvasGroup"):
        if token not in focus_text:
            errors.append(f"focus scope missing visibility token: {token}")

    for surface, path in SURFACE_SOURCES.items():
        if not path.is_file():
            errors.append(f"{surface}: source missing")
            continue
        text = path.read_text(encoding="utf-8-sig")
        if surface != "D29" and "D16ModalFocusScope.Attach" not in text:
            errors.append(f"{surface}: no D16 modal focus scope")
        if any(token in text for token in FORBIDDEN):
            errors.append(f"{surface}: network token found")

    d29 = SURFACE_SOURCES["D29"].read_text(encoding="utf-8-sig")
    d3 = D3_SOURCE.read_text(encoding="utf-8-sig") if D3_SOURCE.is_file() else ""
    d4 = SURFACE_SOURCES["D4"].read_text(encoding="utf-8-sig")
    d9 = SURFACE_SOURCES["D9"].read_text(encoding="utf-8-sig")
    d10 = SURFACE_SOURCES["D10"].read_text(encoding="utf-8-sig")
    d30 = D30_SOURCE.read_text(encoding="utf-8-sig") if D30_SOURCE.is_file() else ""
    d30_policy = D30_POLICY_SOURCE.read_text(encoding="utf-8-sig") if D30_POLICY_SOURCE.is_file() else ""
    d29_policy = D29_POLICY_SOURCE.read_text(encoding="utf-8-sig") if D29_POLICY_SOURCE.is_file() else ""
    player_probe = PLAYER_PROBE_SOURCE.read_text(encoding="utf-8-sig") if PLAYER_PROBE_SOURCE.is_file() else ""
    if not D3_SOURCE.is_file():
        errors.append("D3: source missing")
    if not D30_SOURCE.is_file():
        errors.append("D30: source missing")
    if not D30_POLICY_SOURCE.is_file():
        errors.append("D30: pure stage policy source missing")
    if not D29_POLICY_SOURCE.is_file():
        errors.append("D29: pure one-shot policy source missing")
    if not PLAYER_PROBE_SOURCE.is_file():
        errors.append("D31: Player interaction evidence probe source missing")
    if "private D16ModalFocusScope _focusScope;" in d29 or "_focusScope?.RefreshNavigation();" in d29:
        errors.append("D29 primary HUD must not own modal navigation focus")
    if "D16ModalFocusScope.Attach(_warehousePanel" not in d29:
        errors.append("D29 warehouse modal must retain its own focus scope")

    warehouse_marker = "var warehouseVisible = _warehousePanel != null && _warehousePanel.activeSelf;"
    primary_gate_marker = "if (RuntimeHudCoordinator.PrimaryInputAllowed)"
    warehouse_start = d29.find(warehouse_marker)
    primary_gate_start = d29.find(primary_gate_marker)
    if warehouse_start < 0:
        errors.append("D29: warehouse visibility marker missing")
    elif primary_gate_start < 0 or warehouse_start > primary_gate_start:
        errors.append("D29: warehouse close handling must precede the primary input gate")
    else:
        warehouse_gate = d29[warehouse_start:primary_gate_start]
        for token in ("KeyCode.I", "KeyCode.Escape", "CloseWarehouse();", "return;"):
            if token not in warehouse_gate:
                errors.append(f"D29: warehouse close precedence missing {token}")
    required_tokens = {
        "D29 primary input gate": (d29, "RuntimeHudCoordinator.PrimaryInputAllowed"),
        "D4 temporary canvas starts hidden": (d4, "canvas.enabled = false;"),
        "D9 temporary canvas starts hidden": (d9, "canvas.enabled = false;"),
        "D10 temporary canvas starts hidden": (d10, "canvas.enabled = false;"),
        "D16 dynamic navigation refresh API": (focus_text, "public void RefreshNavigation()"),
        "D29 warehouse modal focus scope": (d29, "D16ModalFocusScope.Attach(_warehousePanel"),
        "D29 tavern readiness guard": (d29, "!_d10.IsReady"),
        "D29 visible feedback": (d29, "Show("),
        "D29 market result guard": (d29, "var opened = _d3.RequestTrade();"),
        "D29 warehouse unavailable feedback": (d29, "仓库面板尚未就绪，无法打开。"),
        "D4 page-local input gate": (d4, "IsContentPageVisible()"),
        "D4 gameplay gate": (d4, "D7ControlCoordinator.GameplayInputAllowed"),
        "D4 unavailable feedback": (d4, "EnsureReadyForAction"),
        "D9 explicit focus state": (d9, "HasInputFocus"),
        "D9 same-frame open guard": (d9, "_pageOpenedFrame"),
        "D9 unavailable feedback": (d9, "EnsureReadyForAction"),
        "D10 close recovery": (d10, "CloseSocialPage"),
        "D10 owner entry result": (d10, "HasEnteredTavern"),
        "D10 visible feedback": (d10, "LastMessage"),
        "D10 unavailable feedback": (d10, "EnsureReadyForAction"),
        "D4 owner upgrade result": (d4, "TryUpgradeHull"),
        "D4 visible feedback": (d4, "LastMessage"),
        "D29 weather result": (d29, "TryCycleWeather"),
        "D29 quality result": (d29, "TryCycleQuality"),
        "D29 weather exception projection": (d29, 'ReportActionException("weather_failed"'),
        "D29 quality exception projection": (d29, 'ReportActionException("quality_failed"'),
        "D29 availability refresh": (d29, "RefreshButtonAvailability"),
        "D29 unready buttons remain clickable for owner feedback": (d29, "button.interactable = true;"),
        "D29 unready click feedback is explicitly logged": (d29, "clickable_unready=true; owner_feedback=true"),
        "D29 repeat guard": (d29, "AllowOneShot"),
        "D29 coordinator consumes pure one-shot policy": (d29, "D29OneShotPolicy.Evaluate"),
        "D29 pure one-shot policy": (d29_policy, "public static class D29OneShotPolicy"),
        "D29 pure one-shot duplicate outcome": (d29_policy, "IsDuplicate"),
        "D29 repeat feedback": (d29, "重复点击已忽略"),
        "D29 rejected tavern recovery": (d29, "_d10.CloseSocialPage();"),
        "D29 initial availability refresh": (d29, "ResolveOwners();\n            RefreshButtonAvailability();"),
        "D29 exception recovery helper": (d29, "ReportActionException"),
        "D29 exception recovery feedback": (d29, "状态未改变"),
        "D29 recovery failure guard": (d29, "recovery failed"),
        "D29 board exception projection": (d29, 'ReportActionException("board_failed"'),
        "D29 market exception projection": (d29, 'ReportActionException("market_failed"'),
        "D29 tavern exception projection": (d29, 'ReportActionException("tavern_failed"'),
        "D29 shipyard exception projection": (d29, 'ReportActionException("shipyard_failed"'),
        "D29 preparation exception projection": (d29, 'ReportActionException("preparation_failed"'),
        "D29 save failure projection": (d29, 'ReportActionException("save_failed"'),
        "D29 save owner rejection projection": (d29, 'if (!ok) D30VisualLoopCoordinator.Notify("save_failed");'),
        "D30 preparation success transition": (d30_policy, 'case "preparation_opened":'),
        "D30 duplicate event guard": (d30, "RepeatGuardSeconds"),
        "D30 duplicate event feedback": (d30, "重复事件已忽略"),
        "D30 failed transition feedback": (d30, "请求未通过，状态未改变"),
        "D30 generic failed transitions": (d30_policy, "shipyard_failed"),
        "D30 save failed transition": (d30_policy, "save_failed"),
        "D30 weather failed transition": (d30_policy, "weather_failed"),
        "D30 quality failed transition": (d30_policy, "quality_failed"),
        "D30 transition null guard": (d30, "_transitionText != null"),
        "D30 pure stage policy": (d30_policy, "public static class D30VisualStagePolicy"),
        "D30 coordinator consumes pure stage policy": (d30, "D30VisualStagePolicy.Evaluate"),
        "D30 policy failure outcome": (d30_policy, "EventOutcome.Failed"),
        "D30 policy duplicate outcome": (d30_policy, "EventOutcome.Duplicate"),
        "D30 explicit failure classifier": (d30_policy, "public static bool IsFailureEvent"),
        "D3 commodity navigation feedback": (d3, 'SetTradeResult("已切换商品："'),
        "D4 faction action refresh": (d4, 'AddLog(D4LogKind.Faction, result.Message);\n            _message = result.Message;\n            RefreshHud();'),
        "D4 combat action refresh": (d4, 'else AddLog(D4LogKind.Combat, _message);\n            RefreshHud();'),
        "D9 result refresh": (d9, '_lastMessage = result == null ? "整备没有返回结果。" : result.Message;\n            Refresh();'),
        "D9 tavern action refresh": (d9, '_lastMessage = "D10 酒馆社会页尚未就绪，请按 V 打开。";\n            }\n            Refresh();'),
        "D31 probe opt-in flag": (player_probe, 'private const string Flag = "-d31-interaction-evidence";'),
        "D31 probe UGUI event path": (player_probe, "ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);"),
        "D31 probe physical-input boundary": (player_probe, "public bool physical_os_input = false;"),
        "D31 probe formal-scene boundary": (player_probe, "public bool formal_scene_asset_modified = false;"),
        "D31 probe output prefix": (player_probe, 'D31_S05_INTERACTION_EVIDENCE_PLAYER_'),
        "D31 probe explicit output directory": (player_probe, "ReadOutputDirectory"),
        "D31 probe save non-mutation boundary": (player_probe, "PRESENT_ONLY_NO_MUTATION"),
        "D31 probe expected matrix": (player_probe, "_result.expected_case_count = 48;"),
    }
    for label, (text, token) in required_tokens.items():
        if token not in text:
            errors.append(f"{label}: missing {token}")
    if 'ShowTemporaryCanvas("D3Canvas")' in d29:
        errors.append("D29 market canvas may open before owner result")
    if 'D30VisualLoopCoordinator.Notify("tavern_opened")' in d29:
        errors.append("D29 tavern path may claim success without D10 entry result")
    if 'D30VisualLoopCoordinator.Notify("shipyard_opened")' in d29:
        errors.append("D29 shipyard path may claim success without D4 upgrade result")
    if "button.gameObject.name = button.gameObject.name" in d29:
        errors.append("D29 availability setter contains a no-op self-assignment")

    return {
        "schema": "D31_INTERACTION_LOGIC_AUDIT_V1",
        "task": "D31-S05",
        "status": "PASS" if not errors else "PARTIAL",
        "acceptance_level": "STATIC_LOGIC_ONLY",
        "runtime_proof": False,
        "visible_entry_count": len(buttons),
        "keyboard_only_count": len(keyboard_only),
        "focus_surfaces": sorted(MODAL_FOCUS_SURFACES),
        "warehouse_close_precedence": warehouse_start >= 0 and primary_gate_start > warehouse_start,
        "errors": errors,
        "next_runtime_gate": "D29-G06/D30-G06/D31-G06",
    }


if __name__ == "__main__":
    print(json.dumps(audit(), ensure_ascii=False, indent=2))
