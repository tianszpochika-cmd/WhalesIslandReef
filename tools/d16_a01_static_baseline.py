#!/usr/bin/env python3
"""Create a truthful static inventory for the D16 InitialHarbor baseline.

This is not a runtime performance benchmark. It deliberately reports scene-YAML
component counts separately from CPU/GPU/GC measurements, which require Unity
runtime access.
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


OBJECT_MARKER = re.compile(r"^--- !u!(\d+)")
NAME_MARKER = re.compile(r"^  m_Name: (.*)$")

COMPONENT_NAMES = {
    "1": "GameObject",
    "4": "Transform",
    "20": "Camera",
    "23": "MeshRenderer",
    "33": "MeshFilter",
    "54": "Rigidbody",
    "64": "MeshCollider",
    "65": "BoxCollider",
    "108": "Light",
    "136": "CapsuleCollider",
}


def scan(scene_path: Path) -> dict:
    counts: dict[str, int] = {}
    names: list[str] = []
    current_type: str | None = None
    for line in scene_path.read_text(encoding="utf-8", errors="replace").splitlines():
        marker = OBJECT_MARKER.match(line)
        if marker:
            current_type = marker.group(1)
            label = COMPONENT_NAMES.get(current_type, f"UnityType{current_type}")
            counts[label] = counts.get(label, 0) + 1
            continue
        if current_type == "1":
            name_match = NAME_MARKER.match(line)
            if name_match:
                name = name_match.group(1).strip()
                if name:
                    names.append(name)

    return {
        "scene": scene_path.stem,
        "source": str(scene_path),
        "measurement_kind": "static_scene_yaml_inventory",
        "runtime_performance_measured": False,
        "component_counts": counts,
        "named_game_objects": sorted(names),
        "runtime_metrics": {
            "cpu_ms": None,
            "gpu_ms": None,
            "managed_gc_bytes": None,
            "loaded_cell_count": None,
            "active_agent_count": None,
            "active_animal_count": None,
        },
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("scene", type=Path)
    parser.add_argument("--json-out", type=Path, required=True)
    args = parser.parse_args()
    result = scan(args.scene)
    args.json_out.parent.mkdir(parents=True, exist_ok=True)
    args.json_out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(
        "D16-A01 static baseline: "
        f"scene={result['scene']} "
        f"game_objects={result['component_counts'].get('GameObject', 0)} "
        f"renderers={result['component_counts'].get('MeshRenderer', 0)} "
        f"colliders={result['component_counts'].get('MeshCollider', 0) + result['component_counts'].get('CapsuleCollider', 0)}"
    )
    print("Runtime metrics intentionally unset; Unity runtime sampling is required.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
