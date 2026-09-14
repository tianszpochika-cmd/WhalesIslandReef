#!/usr/bin/env python3
"""Read-only audit for the D26 harbor environment presentation source."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


REQUIRED_GROUPS = {
    "terrain_shore_water": [r"D19_TerrainRoot", r"Water", r"ShoreRock", r"RoadEdge"],
    "readable_buildings": [r"WarehouseVisual", r"Shipyard", r"Market", r"Tavern", r"House"],
    "vegetation": [r"Palm_", r"Leaf_", r"Shrub_", r"Trunk"],
    "port_life_props": [r"Barrel", r"Crate", r"Rope", r"NoticeBoard", r"Sign", r"Plank"],
}


def audit(prefab_path: Path) -> dict:
    text = prefab_path.read_text(encoding="utf-8")
    names = [match.group(1) for match in re.finditer(r"^  m_Name: (.+)$", text, re.MULTILINE)]
    renderers = len(re.findall(r"^(?:MeshRenderer|SkinnedMeshRenderer):$", text, re.MULTILINE))
    materials = len(re.findall(r"^  m_Materials:$", text, re.MULTILINE))

    groups = {}
    errors = []
    for group_id, patterns in REQUIRED_GROUPS.items():
        matched = sorted({name for name in names if any(re.search(pattern, name) for pattern in patterns)})
        groups[group_id] = {"count": len(matched), "examples": matched[:12]}
        if not matched:
            errors.append(f"missing required environment group: {group_id}")

    if renderers < 120:
        errors.append(f"renderer_count_below_visual_budget: {renderers} < 120")
    if materials < 8:
        errors.append(f"material_slot_count_below_visual_budget: {materials} < 8")

    return {
        "audit_id": "D26-P01-P04",
        "status": "PASS" if not errors else "BLOCKED",
        "source": str(prefab_path).replace("\\", "/"),
        "renderer_count": renderers,
        "material_slot_count": materials,
        "groups": groups,
        "errors": errors,
        "read_only": True,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--prefab", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    result = audit(args.prefab)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{result['status']}: {result['renderer_count']} renderers, {result['material_slot_count']} material slots")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
