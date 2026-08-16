"""Build the D18-A03 environment candidate packet from verified local downloads.

This script is intentionally provenance-first: it copies only the FBX format from
the Kenney archive (one model per source basename), keeps the package License.txt,
and emits manifests that never declare runtime consumers or ReleaseApproved.
"""

from __future__ import annotations

import hashlib
import json
import re
import shutil
from datetime import date
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
SOURCE_ROOT = ROOT / "artifacts" / "d18_a03_work" / "kenney_nature-kit"
KENNEY_SOURCE = SOURCE_ROOT / "Models" / "FBX format"
KENNEY_DEST = ROOT / "game" / "Assets" / "Art" / "Candidates" / "Environment" / "KenneyNatureKit" / "Models"
POLYHAVEN_DEST = ROOT / "game" / "Assets" / "Art" / "Candidates" / "Environment" / "PolyHaven" / "Textures"
PACK_ROOT = ROOT / "docs" / "game" / "design" / "common" / "assets" / "packs" / "environment"
RESULT_ROOT = ROOT / "game" / "tools" / "results" / "D18" / "D18-A03"

TODAY = "2026-08-14"
KENNEY_PAGE = "https://kenney.nl/assets/nature-kit"
KENNEY_DOWNLOAD = "https://kenney.nl/media/pages/assets/nature-kit/37ac38a37b-1677698939/kenney_nature-kit.zip"
POLYHAVEN_SAND_PAGE = "https://polyhaven.com/a/sand_01"
POLYHAVEN_CORAL_PAGE = "https://polyhaven.com/a/coral_mud_01"
POLYHAVEN_LICENSE = "https://polyhaven.com/license"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def slug(value: str) -> str:
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1_\2", value)
    value = re.sub(r"[^a-zA-Z0-9]+", "_", value).strip("_").lower()
    return value or "unnamed"


def model_tags(name: str) -> list[str]:
    lower = name.lower()
    tags: list[str] = []
    if any(token in lower for token in ("tree", "palm", "bush", "grass", "plant", "flower", "cactus", "mushroom")):
        tags.append("vegetation")
    if "cliff" in lower:
        tags.append("cliff")
    if "rock" in lower:
        tags.append("rock")
    if "stone" in lower:
        tags.append("stone")
    if any(token in lower for token in ("wood", "log", "stump")):
        tags.append("wood")
    if "path" in lower:
        tags.append("dirt_path")
    if "beach" in lower:
        tags.append("beach")
    if any(token in lower for token in ("river", "waterfall")):
        tags.append("water_edge")
    if "ground" in lower or "platform" in lower:
        tags.append("terrain_module")
    return tags or ["environment_module"]


def primary_role(tags: list[str]) -> str:
    preferred = ("vegetation", "cliff", "rock", "beach", "dirt_path", "wood", "stone", "water_edge", "terrain_module")
    for tag in preferred:
        if tag in tags:
            return f"candidate_{tag}_model"
    return "candidate_environment_module"


def candidate(path: str, role: str, target_id: str) -> dict[str, str]:
    return {"path": path, "role": role, "target_id": target_id}


def manifest_base(
    source_id: str,
    pack_id: str,
    title: str,
    official_page: str,
    download_url: str,
    archive_name: str | None,
    digest: str | None,
    license_id: str,
    license_url: str,
    evidence_path: str,
    category: list[str],
    files: list[dict[str, str]],
    notes: str,
) -> dict:
    return {
        "manifest_kind": "d18_asset_pack_manifest",
        "schema_version": 1,
        "source_id": source_id,
        "pack_id": pack_id,
        "title": title,
        "author": "Kenney / Kenney.nl" if source_id == "source_kenney" else "Poly Haven / credited asset authors",
        "official_page": official_page,
        "download_url": download_url,
        "downloaded_at": TODAY,
        "archive_name": archive_name,
        "sha256": digest,
        "license_id": license_id,
        "license_url": license_url,
        "license_evidence_path": evidence_path,
        "attribution_required": False,
        "commercial_use": True,
        "modification_allowed": True,
        "source_redistribution": "allowed",
        "build_distribution": "allowed",
        "category": category,
        "file_count": len(files),
        "unity_object_count": None,
        "review_status": "Candidate",
        "import_status": "NotImported",
        "candidate_files": files,
        "runtime_consumers": [],
        "notes": notes,
    }


def main() -> None:
    if not KENNEY_SOURCE.is_dir():
        raise SystemExit(f"Missing verified extraction: {KENNEY_SOURCE}")
    archive = ROOT / "artifacts" / "external_sources" / "kenney_nature-kit.zip"
    if not archive.is_file():
        raise SystemExit(f"Missing verified archive: {archive}")
    license_source = SOURCE_ROOT / "License.txt"
    if not license_source.is_file():
        raise SystemExit(f"Missing package license: {license_source}")

    KENNEY_DEST.mkdir(parents=True, exist_ok=True)
    POLYHAVEN_DEST.mkdir(parents=True, exist_ok=True)
    PACK_ROOT.mkdir(parents=True, exist_ok=True)
    RESULT_ROOT.mkdir(parents=True, exist_ok=True)

    model_files = sorted(KENNEY_SOURCE.glob("*.fbx"), key=lambda path: path.name.lower())
    if len(model_files) < 150:
        raise SystemExit(f"Expected at least 150 real FBX candidates, found {len(model_files)}")

    model_candidates: list[dict[str, str]] = []
    records: list[dict] = []
    for source_file in model_files:
        destination = KENNEY_DEST / source_file.name
        if destination.exists() and sha256(destination) != sha256(source_file):
            raise SystemExit(f"Refusing to overwrite changed candidate: {destination}")
        if not destination.exists():
            shutil.copy2(source_file, destination)
        tags = model_tags(source_file.stem)
        target_id = "env_kenney_nature_" + slug(source_file.stem)
        model_candidates.append(candidate(f"game/Assets/Art/Candidates/Environment/KenneyNatureKit/Models/{source_file.name}", primary_role(tags), target_id))
        records.append({
            "source_file": str(source_file.relative_to(SOURCE_ROOT)).replace("\\", "/"),
            "candidate_path": f"game/Assets/Art/Candidates/Environment/KenneyNatureKit/Models/{source_file.name}",
            "target_id": target_id,
            "kind": "model",
            "tags": tags,
            "pivot": "not_authored_in_source; verify in Prefab stage",
            "collision": "not_authored_in_source; verify in Prefab stage",
            "lod": "not_authored_in_source; verify in Prefab stage",
            "scale_reference": "1 Unity unit = 1 metre; source and bounds require Unity audit",
            "axis_reference": "Unity Y-up / Z-forward reference; importer result required",
        })

    license_destination = KENNEY_DEST.parent / "License.txt"
    if license_destination.exists() and sha256(license_destination) != sha256(license_source):
        raise SystemExit(f"Refusing to overwrite changed license: {license_destination}")
    if not license_destination.exists():
        shutil.copy2(license_source, license_destination)

    kenney_manifest = manifest_base(
        "source_kenney",
        "pack_kenney_nature_kit_d18_a03_v1",
        "Nature Kit — D18-A03 environment candidates",
        KENNEY_PAGE,
        KENNEY_DOWNLOAD,
        archive.name,
        sha256(archive),
        "CC0-1.0",
        "https://creativecommons.org/publicdomain/zero/1.0/",
        "game/Assets/Art/Candidates/Environment/KenneyNatureKit/License.txt",
        ["models", "environment", "vegetation", "rock", "cliff", "terrain_module"],
        model_candidates,
        "Only the archive's FBX format is copied; OBJ/DAE/GLB/STL duplicates and preview images are not copied. Original filenames are preserved for provenance. Candidate files are not Registry entries, Prefabs, scene references or runtime consumers. Source models do not author the required collision, interaction anchors or LOD; those remain a later Prefab responsibility.",
    )

    poly_files = [
        ("sand_01_diff_1k.jpg", "beach_material"),
        ("sand_01_nor_gl_1k.jpg", "beach_material_normal"),
        ("sand_01_rough_1k.jpg", "beach_material_roughness"),
        ("sand_01_ao_1k.jpg", "beach_material_ao"),
        ("coral_mud_01_diff_1k.jpg", "seabed_material"),
        ("coral_mud_01_nor_gl_1k.jpg", "seabed_material_normal"),
        ("coral_mud_01_rough_1k.jpg", "seabed_material_roughness"),
        ("coral_mud_01_ao_1k.jpg", "seabed_material_ao"),
    ]
    poly_candidates: list[dict[str, str]] = []
    poly_records: list[dict] = []
    for filename, role in poly_files:
        path = POLYHAVEN_DEST / filename
        if not path.is_file():
            raise SystemExit(f"Missing verified Poly Haven file: {path}")
        asset_id = "material_polyhaven_" + slug(filename.replace("_1k", ""))
        poly_candidates.append(candidate(f"game/Assets/Art/Candidates/Environment/PolyHaven/Textures/{filename}", role, asset_id))
        poly_records.append({
            "candidate_path": f"game/Assets/Art/Candidates/Environment/PolyHaven/Textures/{filename}",
            "target_id": asset_id,
            "kind": "texture",
            "sha256": sha256(path),
            "resolution": "1K source download; Unity importer must verify dimensions",
            "pivot": "not_applicable",
            "collision": "not_applicable",
            "lod": "not_applicable",
        })

    manifests = {"kenney_nature_kit_d18_a03.manifest.json": kenney_manifest}
    for index, (filename, role) in enumerate(poly_files, start=1):
        is_coral = filename.startswith("coral_mud_01")
        asset_page = POLYHAVEN_CORAL_PAGE if is_coral else POLYHAVEN_SAND_PAGE
        asset_id = "material_polyhaven_" + slug(filename.replace("_1k", ""))
        direct_url = (
            "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/coral_mud_01/" + filename
            if is_coral
            else "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/sand_01/" + filename
        )
        tags = ["materials", "seabed", "coastal", "pbr"] if is_coral else ["materials", "beach", "sand", "pbr"]
        map_manifest = manifest_base(
            "source_polyhaven",
            "pack_polyhaven_" + slug(filename.replace("_1k", "")) + "_d18_a03_v1",
            ("Coral Mud 01" if is_coral else "Sand 01") + " — 1K " + role,
            asset_page,
            direct_url,
            filename,
            sha256(POLYHAVEN_DEST / filename),
            "CC0-1.0",
            POLYHAVEN_LICENSE,
            "docs/game/design/common/assets/packs/environment/licenses/polyhaven_license_evidence_2026-08-14.md",
            tags,
            [poly_candidates[index - 1]],
            "One direct 1K JPG map from one official Poly Haven asset page; no site crawl, preview, logo, metadata or 8K archive was copied. This file is a material candidate and not a Unity material asset or scene consumer. Coral Mud 01 describes exposed coastal seabed; water surface shading remains owned by the project water system.",
        )
        manifests["polyhaven_" + slug(filename) + "_d18_a03.manifest.json"] = map_manifest
    for filename, manifest in manifests.items():
        (PACK_ROOT / filename).write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    catalog = {
        "task": "D18-A03",
        "generated_at": TODAY,
        "source_archive_sha256": sha256(archive),
        "source_archive_bytes": archive.stat().st_size,
        "kenney_model_count": len(model_candidates),
        "polyhaven_texture_count": len(poly_candidates),
        "candidate_count_total": len(model_candidates) + len(poly_candidates),
        "models": records,
        "textures": poly_records,
        "sample_selection_rule": "deterministic coverage-first sample of 50 Kenney FBX files; all 329 models remain candidates and are audited by Unity when the A03 batch report runs",
        "manifests": [f"docs/game/design/common/assets/packs/environment/{name}" for name in manifests],
        "runtime_consumers": [],
        "release_approved_asserted": False,
    }
    (RESULT_ROOT / "D18_A03_CANDIDATE_CATALOG.json").write_text(json.dumps(catalog, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({
        "kenney_models": len(model_candidates),
        "polyhaven_textures": len(poly_candidates),
        "candidate_total": catalog["candidate_count_total"],
        "archive_bytes": archive.stat().st_size,
        "archive_sha256": sha256(archive),
        "manifests": list(manifests),
        "license": str(license_destination),
    }, indent=2))


if __name__ == "__main__":
    main()
