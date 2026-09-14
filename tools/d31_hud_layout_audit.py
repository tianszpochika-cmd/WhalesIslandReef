"""Static audit for the D25/D29 HUD layout contract.

The audit intentionally proves only deterministic source/layout facts. It does
not claim that a Windows Player rendered the layout or that physical OS input
was delivered.
"""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
CONTRACT_PATH = ROOT / "docs/game/design/common/D31_HUD_LAYOUT_CONTRACT.json"
POLICY_PATH = ROOT / "game/Assets/Scripts/Gameplay/D29/Logic/D29InteractionLayoutPolicy.cs"
D29_PATH = ROOT / "game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs"


def _rect(value: dict[str, Any]) -> tuple[int, int, int, int]:
    return (
        int(value["left"]),
        int(value["top"]),
        int(value["width"]),
        int(value["height"]),
    )


def _overlaps(first: tuple[int, int, int, int], second: tuple[int, int, int, int]) -> bool:
    first_left, first_top, first_width, first_height = first
    second_left, second_top, second_width, second_height = second
    return (
        first_left < second_left + second_width
        and first_left + first_width > second_left
        and first_top < second_top + second_height
        and first_top + first_height > second_top
    )


def audit() -> dict[str, Any]:
    errors: list[str] = []
    contract = json.loads(CONTRACT_PATH.read_text(encoding="utf-8"))
    policy = POLICY_PATH.read_text(encoding="utf-8-sig")
    d29 = D29_PATH.read_text(encoding="utf-8-sig")

    viewport = contract.get("reference_resolution", {})
    width = int(viewport.get("width", 0))
    height = int(viewport.get("height", 0))
    if (width, height) != (1920, 1080):
        errors.append(f"unexpected reference resolution: {width}x{height}")
    if viewport.get("coordinate_origin") != "top_left":
        errors.append("contract must use top_left coordinates")

    surfaces = contract.get("surfaces", {})
    rectangles = {name: _rect(value) for name, value in surfaces.items()}
    for name, (left, top, surface_width, surface_height) in rectangles.items():
        if left < 0 or top < 0 or surface_width <= 0 or surface_height <= 0:
            errors.append(f"{name}: invalid rectangle")
        if left + surface_width > width or top + surface_height > height:
            errors.append(f"{name}: outside viewport")

    names = list(rectangles)
    for index, name in enumerate(names):
        for other in names[index + 1 :]:
            if _overlaps(rectangles[name], rectangles[other]):
                errors.append(f"{name} overlaps {other}")

    quick = contract.get("quick_actions", {})
    expected_constants = {
        "ReferenceWidth": 1920,
        "ReferenceHeight": 1080,
        "QuickActionsRightInset": quick.get("right_inset"),
        "QuickActionsTopInset": quick.get("top_inset"),
        "QuickActionsWidth": rectangles.get("D29QuickActions", (0, 0, 0, 0))[2],
        "QuickActionsHeight": rectangles.get("D29QuickActions", (0, 0, 0, 0))[3],
        "QuickActionButtonWidth": quick.get("button_width"),
        "QuickActionButtonHeight": quick.get("button_height"),
        "QuickActionButtonTop": quick.get("first_button_top_in_panel"),
        "QuickActionButtonStep": quick.get("vertical_step"),
        "QuickActionButtonCount": quick.get("button_count"),
        "WarehouseWidth": rectangles.get("D29Warehouse", (0, 0, 0, 0))[2],
        "WarehouseHeight": rectangles.get("D29Warehouse", (0, 0, 0, 0))[3],
    }
    for name, expected in expected_constants.items():
        if expected is None:
            errors.append(f"contract missing constant mapping: {name}")
            continue
        match = re.search(rf"public const int {re.escape(name)}\s*=\s*(-?\d+)\s*;", policy)
        if not match or int(match.group(1)) != int(expected):
            errors.append(f"policy constant mismatch: {name} expected {expected}")

    for token in (
        "D29InteractionLayoutPolicy.QuickActionsRightInset",
        "D29InteractionLayoutPolicy.QuickActionsTopInset",
        "D29InteractionLayoutPolicy.QuickActionsWidth",
        "D29InteractionLayoutPolicy.QuickActionsHeight",
        "D29InteractionLayoutPolicy.WarehouseWidth",
        "D29InteractionLayoutPolicy.WarehouseHeight",
        "D29InteractionLayoutPolicy.QuickActionButtonTop",
        "D29InteractionLayoutPolicy.QuickActionButtonStep",
    ):
        if token not in d29:
            errors.append(f"D29 UI does not consume layout policy: {token}")

    if "WarehouseObjectName" not in policy:
        errors.append("policy missing warehouse object name")
    if "WarehouseSurfaceName" not in policy:
        errors.append("policy missing warehouse surface name")
    if "ValidateReferenceLayout" not in policy:
        errors.append("policy missing deterministic validation")

    return {
        "schema": "D31_HUD_LAYOUT_AUDIT_V1",
        "task_id": "D31-S05",
        "status": "PASS" if not errors else "PARTIAL",
        "acceptance_level": "STATIC_LOGIC_ONLY",
        "runtime_proof": False,
        "reference_resolution": f"{width}x{height}",
        "surface_count": len(rectangles),
        "checked_surfaces": names,
        "quick_action_button_count": quick.get("button_count"),
        "errors": errors,
        "next_runtime_gate": "D29-G06/D30-G06/D31-G06",
    }


if __name__ == "__main__":
    print(json.dumps(audit(), ensure_ascii=False, indent=2))
