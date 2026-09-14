#!/usr/bin/env python3
"""Read-only audit for D27 player/NPC/wildlife presentation candidates."""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path


def audit(root: Path) -> dict:
    prefab_paths = sorted(root.rglob("*.prefab"))
    character_paths = [p for p in prefab_paths if "Characters" in p.parts]
    animal_paths = [p for p in prefab_paths if "Animals" in p.parts]
    all_text = "\n".join(p.read_text(encoding="utf-8") for p in prefab_paths)
    renderers = len(re.findall(r"^(?:MeshRenderer|SkinnedMeshRenderer):$", all_text, re.MULTILINE))
    animator_prefabs = sum("Animator:" in p.read_text(encoding="utf-8") for p in prefab_paths)
    required_tokens = {
        "player_variants": ["D18_R03_A01_outfit_male", "D18_R03_A01_outfit_female"],
        "hair_variants": ["D20_R02_Hair_01", "D20_R02_Hair_02", "D20_R02_Hair_03"],
        "pet_variants": ["D20_PetsRare_cat_core", "D20_PetsRare_dog_shiba"],
        "wildlife_variants": ["D20_Wildlife_fish", "D20_Wildlife_eagle", "D20_Wildlife_whale"],
    }
    groups = {}
    errors = []
    for group_id, tokens in required_tokens.items():
        matches = sorted({p.name for p in prefab_paths if any(token in p.name for token in tokens)})
        groups[group_id] = {"count": len(matches), "examples": matches[:12]}
        if not matches:
            errors.append(f"missing presentation candidate group: {group_id}")
    if len(character_paths) < 10:
        errors.append(f"character_prefab_count_below_budget: {len(character_paths)} < 10")
    if len(animal_paths) < 7:
        errors.append(f"animal_prefab_count_below_budget: {len(animal_paths)} < 7")
    if renderers < 20:
        errors.append(f"renderer_count_below_budget: {renderers} < 20")
    return {
        "audit_id": "D27-P01-P04",
        "status": "PASS" if not errors else "BLOCKED",
        "source_root": str(root).replace("\\", "/"),
        "character_prefab_count": len(character_paths),
        "animal_prefab_count": len(animal_paths),
        "renderer_count": renderers,
        "animator_prefab_count": animator_prefabs,
        "groups": groups,
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
    print(f"{result['status']}: {result['character_prefab_count']} character prefabs, {result['animal_prefab_count']} animal prefabs")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
