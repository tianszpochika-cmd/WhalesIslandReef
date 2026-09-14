#!/usr/bin/env python3
"""Read-only D28 water, ship and weather presentation audit."""

from __future__ import annotations

import argparse
import json
from pathlib import Path


def audit(root: Path) -> dict:
    water = root / "Prefabs" / "Environment" / "D19" / "Water"
    ship = root / "Prefabs" / "Environment" / "D19" / "Integration" / "R03" / "D19_R03_ShipVisual.prefab"
    code = root / "Scripts" / "Gameplay" / "D28" / "D28WaterWeatherPresentationRuntime.cs"
    water_files = sorted(p for p in water.rglob("*") if p.is_file()) if water.exists() else []
    source_text = code.read_text(encoding="utf-8") if code.exists() else ""
    required = {
        "water_surface_mesh": any("Surface_Mesh" in p.name for p in water_files),
        "shore_foam_mesh": any("ShoreFoam" in p.name for p in water_files),
        "reef_rock_mesh": any("ReefRock" in p.name for p in water_files),
        "ship_visual_source": ship.exists(),
        "calm_rain_storm": all(token in source_text for token in ("Calm", "Rain", "Storm")),
    }
    errors = [name for name, present in required.items() if not present]
    return {
        "audit_id": "D28-P01-P04",
        "status": "PASS" if not errors else "BLOCKED",
        "water_file_count": len(water_files),
        "required": required,
        "errors": errors,
        "read_only": True,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    result = audit(args.root)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{result['status']}: water files={result['water_file_count']}")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
