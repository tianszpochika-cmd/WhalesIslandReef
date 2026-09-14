"""Audit the current R03 presentation material and UV integration.

This is an editor/static audit only. It verifies that the generated R03
prefab and its source builder reference approved local textures, that the
intentional plaster color-only exception stays explicit, and that the
normal-map importer metadata is in the expected color space. It deliberately
does not claim Player or final visual-quality approval.
"""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs"
MATERIAL_ROOT = ROOT / "game/Assets/Art/Materials/D19/R03"
PREFAB = ROOT / "game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab"
EDITOR_AUDIT = ROOT / "game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json"

MATERIALS = (
    "R03_Grass.mat",
    "R03_GrassLight.mat",
    "R03_Earth.mat",
    "R03_Plaster.mat",
    "R03_Sand.mat",
    "R03_SandLight.mat",
    "R03_Stone.mat",
    "R03_StoneLight.mat",
    "R03_Wood.mat",
    "R03_WoodLight.mat",
    "R03_Rope.mat",
    "R03_Roof.mat",
)

# R52 deliberately removed the terrain mud maps from plaster walls after a
# Player review showed that they made the buildings read as rock/terrain proxy
# geometry. R75 restores the approved texture binding for the five terrain
# surfaces that had been cleared by a stale builder branch. Every other R03
# surface remains texture-backed; plaster is the single, explicit authored-
# color exception and must not be silently treated as a missing material.
TEXTURED_MATERIALS = tuple(name for name in MATERIALS if name != "R03_Plaster.mat")
COLOR_ONLY_MATERIALS = ("R03_Plaster.mat",)

# R64 added the perimeter foam strokes and wave-surface presentation mesh;
# R66 added the imported Pirate Kit port candidates. The current R76 baseline
# deliberately excludes the six authored palm groups because the builder keeps
# RenderAuthoredPalms=false until a pivot/ground-contact contract is approved.
# The connected current-scene audit is 294 renderers. Keep that count explicit
# so a changed visual prefab cannot be silently accepted while still refusing to
# treat the missing palms as a visual completion.
EXPECTED_RENDERER_COUNT = 294

NORMAL_META = (
    ROOT / "game/Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_nor_gl_1k.jpg.meta",
    ROOT / "game/Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_normal.jpg.meta",
    ROOT / "game/Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_normal.jpg.meta",
    ROOT / "game/Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_normal.jpg.meta",
    ROOT / "game/Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_normal.jpg.meta",
    ROOT / "game/Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_normal.jpg.meta",
)


def audit() -> dict[str, Any]:
    errors: list[str] = []
    builder = BUILDER.read_text(encoding="utf-8-sig") if BUILDER.is_file() else ""
    required_builder_tokens = (
        "ConfigurePresentationMaterial",
        "PresentationBaseMapPath",
        "PresentationNormalMapPath",
        "LoadNormalMap",
        "ApplyTriplanarPresentationUv",
        "mesh.uv = islandUv",
        "brown_planks_03_albedo.jpg",
        "aerial_grass_rock_albedo.jpg",
        "sand_01_diff_1k.jpg",
    )
    for token in required_builder_tokens:
        if token not in builder:
            errors.append(f"builder missing token: {token}")

    material_hits = 0
    for name in MATERIALS:
        path = MATERIAL_ROOT / name
        if not path.is_file():
            errors.append(f"material missing: {path.relative_to(ROOT)}")
            continue
        text = path.read_text(encoding="utf-8-sig")
        has_texture = "m_Texture: {fileID: 2800000" in text
        if name in TEXTURED_MATERIALS:
            if not has_texture:
                errors.append(f"material has no texture reference: {name}")
            else:
                material_hits += 1
            if "_BaseColor: {r: 1, g: 1, b: 1, a: 1}" not in text:
                errors.append(f"material is not using texture-neutral base color: {name}")
        elif name in COLOR_ONLY_MATERIALS:
            if has_texture:
                errors.append(f"color-only material unexpectedly has texture reference: {name}")
            if "_BaseColor: {r: 0.52, g: 0.43, b: 0.32, a: 1}" not in text:
                errors.append(f"color-only material lost authored plaster color: {name}")
        else:
            errors.append(f"material is not classified: {name}")

    normal_meta_pass = 0
    for path in NORMAL_META:
        if not path.is_file():
            errors.append(f"normal map meta missing: {path.relative_to(ROOT)}")
            continue
        text = path.read_text(encoding="utf-8-sig")
        if "textureType: 1" not in text or "sRGBTexture: 0" not in text:
            errors.append(f"normal map importer metadata is not linear NormalMap: {path.relative_to(ROOT)}")
        else:
            normal_meta_pass += 1

    editor_audit: dict[str, Any] = {}
    if not PREFAB.is_file():
        errors.append(f"prefab missing: {PREFAB.relative_to(ROOT)}")
    if not EDITOR_AUDIT.is_file():
        errors.append(f"editor audit missing: {EDITOR_AUDIT.relative_to(ROOT)}")
    else:
        editor_audit = json.loads(EDITOR_AUDIT.read_text(encoding="utf-8-sig"))
        if editor_audit.get("renderer_count") != EXPECTED_RENDERER_COUNT:
            errors.append(f"unexpected renderer count: {editor_audit.get('renderer_count')}")
        if editor_audit.get("collider_count") != 1:
            errors.append(f"unexpected R03 collider count: {editor_audit.get('collider_count')}")
        if int(editor_audit.get("mesh_vertex_sum", 0)) < 32000:
            errors.append(f"UV-expanded mesh vertex sum is too small: {editor_audit.get('mesh_vertex_sum')}")

    return {
        "schema": "D31_S05_VISUAL_MATERIAL_AUDIT_V2",
        "task": "D31-S05",
        "revision": "R76",
        "status": "PASS" if not errors else "PARTIAL",
        "acceptance_level": "EDITOR_STATIC",
        "runtime_proof": False,
        "material_count": len(MATERIALS),
        "textured_material_count": material_hits,
        "color_only_material_count": len(COLOR_ONLY_MATERIALS),
        "normal_map_meta_count": len(NORMAL_META),
        "normal_map_meta_pass": normal_meta_pass,
        "prefab_exists": PREFAB.is_file(),
        "editor_audit": {
            "renderer_count": editor_audit.get("renderer_count"),
            "collider_count": editor_audit.get("collider_count"),
            "mesh_vertex_sum": editor_audit.get("mesh_vertex_sum"),
        },
        "errors": errors,
        "next_gate": "D31-P04/D31-G06 Player visual recheck",
    }


if __name__ == "__main__":
    print(json.dumps(audit(), ensure_ascii=False, indent=2))
