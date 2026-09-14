#!/usr/bin/env python3
"""Read-only audit for the D27 R03 character/wildlife presentation Prefab.

This audit checks the deterministic Editor build output and the R03 Prefab
attachment. It deliberately does not claim that a Windows Player rendered or
that NPC/animal runtime behavior was observed.
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
R03_PREFAB_REL = Path("game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab")
OUTPUT_PREFAB_REL = Path("game/Assets/Prefabs/Characters/D27/R03/D27_R03_CharacterWildlifePresentation.prefab")
BUILDER_REL = Path("game/Assets/Editor/D27/D27R03CharacterWildlifePresentationBuilder.cs")
BUILD_RESULT_REL = Path("game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json")

EXPECTED_NPCS = (
    "npc_d20_dock_clerk_01",
    "npc_d20_fisher_01",
    "npc_d20_deckhand_01",
    "npc_d20_net_mender_01",
    "npc_d20_shore_trader_01",
    "npc_d20_shore_trader_02",
    "npc_d20_tavern_cook_01",
    "npc_d20_boatbuilder_01",
    "npc_d20_sailmaker_01",
    "npc_d20_rigger_01",
    "npc_d20_harbor_watch_proxy",
    "npc_d20_common_resident_01",
)
EXPECTED_WILDLIFE = (
    "D20_Wildlife_fox_small_land",
    "D20_Wildlife_cow_livestock",
    "D20_Wildlife_eagle_coastal_bird",
    "D20_Wildlife_fish_nearshore",
    "D20_Wildlife_fish_shallow_school",
    "D20_Wildlife_manta_large_aquatic_equivalent",
    "D20_Wildlife_shark_nearshore_predator",
    "D20_Wildlife_whale_distant_aquatic",
)
EXPECTED_PETS = (
    "D20_PetsRare_cat_core",
    "D20_PetsRare_dog_shiba",
)


def _read(path: Path, errors: list[str], label: str) -> str:
    if not path.is_file():
        errors.append(f"{label}_missing: {path.as_posix()}")
        return ""
    return path.read_text(encoding="utf-8-sig")


def _name_count(text: str, prefix: str) -> int:
    return len(re.findall(rf"^\s*m_Name: {re.escape(prefix)}[^\r\n]*$", text, re.MULTILINE))


def _id_count(text: str, prefix: str, ids: tuple[str, ...]) -> int:
    """Count deterministic wrapper IDs, including nested-Prefab modifications."""
    return sum(1 for item in ids if f"{prefix}{item}" in text)


def audit(root: Path = ROOT) -> dict[str, Any]:
    root = root.resolve()
    errors: list[str] = []
    r03_path = root / R03_PREFAB_REL
    output_path = root / OUTPUT_PREFAB_REL
    builder_path = root / BUILDER_REL
    result_path = root / BUILD_RESULT_REL

    r03_text = _read(r03_path, errors, "r03_prefab")
    output_text = _read(output_path, errors, "output_prefab")
    builder_text = _read(builder_path, errors, "builder")
    result_text = _read(result_path, errors, "build_result")

    report: dict[str, Any] = {}
    if result_text:
        try:
            report = json.loads(result_text)
        except json.JSONDecodeError as exc:
            errors.append(f"build_result_invalid_json: {exc}")

    npc_ids = [item for item in EXPECTED_NPCS if f"D27_NPC_{item}" in output_text]
    wildlife_ids = [item for item in EXPECTED_WILDLIFE if f"D27_Wildlife_{item}" in output_text]
    pet_ids = [item for item in EXPECTED_PETS if f"D27_Pet_{item}" in output_text]
    attached_count = r03_text.count("D27_R03_CharacterWildlifePresentation")
    counts = {
        # NPC wrappers serialize as ordinary GameObjects. Wildlife and pet
        # sources are nested Prefab instances, so Unity stores their wrapper
        # names in m_Modification values instead of m_Name lines.
        "npc": _name_count(output_text, "D27_NPC_"),
        "wildlife": _id_count(output_text, "D27_Wildlife_", EXPECTED_WILDLIFE),
        "pet": _id_count(output_text, "D27_Pet_", EXPECTED_PETS),
    }

    if counts["npc"] != len(EXPECTED_NPCS):
        errors.append(f"npc_wrapper_count_mismatch: {counts['npc']} != {len(EXPECTED_NPCS)}")
    if counts["wildlife"] != len(EXPECTED_WILDLIFE):
        errors.append(f"wildlife_wrapper_count_mismatch: {counts['wildlife']} != {len(EXPECTED_WILDLIFE)}")
    if counts["pet"] != len(EXPECTED_PETS):
        errors.append(f"pet_wrapper_count_mismatch: {counts['pet']} != {len(EXPECTED_PETS)}")
    if len(npc_ids) != len(EXPECTED_NPCS):
        errors.append(f"npc_id_coverage_mismatch: {len(npc_ids)} != {len(EXPECTED_NPCS)}")
    if len(wildlife_ids) != len(EXPECTED_WILDLIFE):
        errors.append(f"wildlife_id_coverage_mismatch: {len(wildlife_ids)} != {len(EXPECTED_WILDLIFE)}")
    if len(pet_ids) != len(EXPECTED_PETS):
        errors.append(f"pet_id_coverage_mismatch: {len(pet_ids)} != {len(EXPECTED_PETS)}")
    if attached_count != 1:
        errors.append(f"r03_attachment_count_mismatch: {attached_count} != 1")

    required_builder_tokens = (
        "D16PresentationRegistration",
        "ConfigurePerformanceRegistration",
        "D20NpcReadOnlyPresentationAdapter",
        "D20NpcPresentationAnchor",
        "DisableBlockingColliders",
        "D20_NPC_Presentation_Demo.controller",
        "formal_scene_modified = false",
        "systems_modified = false",
        "save_data_modified = false",
    )
    for token in required_builder_tokens:
        if token not in builder_text:
            errors.append(f"builder_token_missing: {token}")

    expected_report = {
        "schema": "D27_S05_R03_CHARACTER_WILDLIFE_PRESENTATION_V1",
        "task": "D27-S05",
        "status": "PASS",
        "npc_count": len(EXPECTED_NPCS),
        "wildlife_count": len(EXPECTED_WILDLIFE),
        "pet_count": len(EXPECTED_PETS),
        "registration_count": len(EXPECTED_NPCS) + len(EXPECTED_WILDLIFE) + len(EXPECTED_PETS),
        "formal_scene_modified": False,
        "systems_modified": False,
        "save_data_modified": False,
    }
    for key, expected in expected_report.items():
        if report.get(key) != expected:
            errors.append(f"build_report_mismatch: {key}={report.get(key)!r}, expected {expected!r}")
    if report.get("errors") != []:
        errors.append("build_report_contains_errors")
    if report.get("disabled_non_trigger_collider_count") != len(EXPECTED_NPCS):
        errors.append("npc_blocking_collider_disable_count_mismatch")

    return {
        "schema": "D27_S05_R03_CHARACTER_WILDLIFE_PRESENTATION_AUDIT_V1",
        "task": "D27-S05",
        "status": "PASS" if not errors else "BLOCKED",
        "acceptance_level": "EDITOR_STATIC_ONLY",
        "runtime_proof": False,
        "r03_prefab": R03_PREFAB_REL.as_posix(),
        "output_prefab": OUTPUT_PREFAB_REL.as_posix(),
        "wrapper_counts": counts,
        "id_coverage": {
            "npc": npc_ids,
            "wildlife": wildlife_ids,
            "pet": pet_ids,
        },
        "r03_attachment_count": attached_count,
        "build_report": expected_report,
        "errors": errors,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=ROOT)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = audit(args.root)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{result['status']}: wrappers={result['wrapper_counts']}, r03_attachment={result['r03_attachment_count']}")
    return 0 if result["status"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main())
