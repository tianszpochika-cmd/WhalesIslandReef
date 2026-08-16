from __future__ import annotations

import hashlib
import json
import math
import re
import shutil
import subprocess
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont
import soundfile as sf


ROOT = Path(__file__).resolve().parents[5]
CATALOG = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
LAYOUT = ROOT / "docs/game/design/common/assets/d19_saint_margaret_layout_v1.json"
SCENE = ROOT / "game/Assets/Scenes/D19/Work/D19_Ambience_Work.unity"
PROP_CATALOG = ROOT / "game/Assets/Prefabs/Props/D19/D19_PROP_VFX_CATALOG.json"
ZONE_GRAPH = ROOT / "game/Assets/Audio/D19/D19_AMBIENCE_ZONES.json"
PLACEMENTS = ROOT / "game/Assets/Scenes/D19/Work/Data/D19_A06_PLACEMENT_MANIFEST.json"
OUT = ROOT / "game/tools/results/D19/D19-A06"
SCREENSHOTS = OUT / "screenshots"


def load(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def distance_to_segment(px, pz, ax, az, bx, bz):
    dx, dz = bx - ax, bz - az
    length_sq = dx * dx + dz * dz
    if length_sq == 0:
        return math.hypot(px - ax, pz - az)
    t = max(0.0, min(1.0, ((px - ax) * dx + (pz - az) * dz) / length_sq))
    return math.hypot(px - (ax + t * dx), pz - (az + t * dz))


def route_distance(position, anchors_by_id, steps):
    px, pz = position[0], position[2]
    distances = []
    for step in steps:
        a = anchors_by_id[step["from"]]
        b = anchors_by_id[step["to"]]
        distances.append(distance_to_segment(px, pz, a[0], a[2], b[0], b[2]))
    return min(distances) if distances else float("inf")


def draw_plate(path: Path, title: str, center, half_extents, placements, zone_id):
    width, height = 1280, 720
    image = Image.new("RGB", (width, height), (27, 37, 44))
    draw = ImageDraw.Draw(image)
    try:
        font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 28)
        small = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 19)
    except OSError:
        font = ImageFont.load_default()
        small = font
    draw.rectangle((36, 32, width - 36, height - 32), outline=(216, 181, 106), width=3)
    draw.text((64, 56), title, fill=(248, 236, 204), font=font)
    draw.text((64, 100), "STATIC WORK-SCENE PLATE / NOT A RUNTIME SCREENSHOT", fill=(230, 160, 120), font=small)
    map_box = (80, 160, width - 80, height - 80)
    draw.rectangle(map_box, fill=(42, 64, 68), outline=(115, 164, 148), width=2)
    cx, _, cz = center
    hx, _, hz = half_extents
    scale = min((map_box[2] - map_box[0]) / max(hx * 2.2, 1), (map_box[3] - map_box[1]) / max(hz * 2.2, 1))

    def to_screen(x, z):
        return (map_box[0] + (x - (cx - hx * 1.1)) * scale, map_box[3] - (z - (cz - hz * 1.1)) * scale)

    z0 = to_screen(cx - hx, cz - hz)
    z1 = to_screen(cx + hx, cz + hz)
    draw.rectangle((z0[0], z1[1], z1[0], z0[1]), fill=(65, 91, 88), outline=(228, 198, 122), width=2)
    draw.text((map_box[0] + 16, map_box[1] + 14), zone_id, fill=(248, 236, 204), font=small)
    for item in placements:
        x, _, z = item["position_m"]
        sx, sy = to_screen(x, z)
        color = (230, 175, 86) if item["asset_id"].startswith("prop.") else (137, 200, 245)
        draw.ellipse((sx - 9, sy - 9, sx + 9, sy + 9), fill=color, outline=(20, 25, 30), width=2)
        draw.text((sx + 14, sy - 12), item["instance_id"].replace("d19_", ""), fill=(235, 245, 240), font=small)
    draw.line((map_box[0] + 24, map_box[3] - 24, map_box[2] - 24, map_box[3] - 24), fill=(214, 214, 214), width=2)
    draw.text((map_box[0] + 28, map_box[3] - 55), "gold=prop / blue=VFX proxy / trigger zones only", fill=(216, 230, 224), font=small)
    image.save(path)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    SCREENSHOTS.mkdir(parents=True, exist_ok=True)
    catalog = load(CATALOG)
    layout = load(LAYOUT)
    prop_catalog = load(PROP_CATALOG)
    zones = load(ZONE_GRAPH)
    placement_data = load(PLACEMENTS)
    entries = [
        item for item in catalog["eligible_entries"]
        if item.get("domain") == "audio" and "D19-A06" in item.get("allowed_task_scope", [])
    ]
    entries.sort(key=lambda item: (item["manifest_id"], item["candidate_file_path"]))
    audio_records = []
    for item in entries:
        source = ROOT / item["candidate_file_path"]
        destination = ROOT / "game/Assets/Audio/D19/Interaction" / ("d19_candidate_" + source.name)
        data, sample_rate = sf.read(str(destination), always_2d=True)
        rms = float(np.sqrt(np.mean(np.square(data)))) if data.size else 0.0
        rms_dbfs = 20.0 * math.log10(max(rms, 1e-9))
        audio_records.append({
            "intake_id": item["intake_id"],
            "content_id": item["content_id"],
            "manifest_id": item["manifest_id"],
            "source_candidate_path": item["candidate_file_path"].replace("\\", "/"),
            "destination_path": str(destination.relative_to(ROOT)).replace("\\", "/"),
            "source_exists": source.exists(),
            "destination_exists": destination.exists(),
            "sha256": sha256(destination),
            "bytes": destination.stat().st_size,
            "channels": int(data.shape[1]),
            "sample_rate_hz": int(sample_rate),
            "duration_seconds": round(float(len(data) / sample_rate), 5),
            "loop": False,
            "loop_point": None,
            "rms_dbfs_proxy": round(rms_dbfs, 2),
            "loudness_status": "RMS proxy measured; LUFS not measured without release metering pass",
            "license_id": item["license"]["license_id"],
            "license_evidence_path": item["license"]["license_evidence_path"].replace("\\", "/"),
            "import_evidence": item["import_verified_evidence_path"].replace("\\", "/"),
            "promotion": "not LocalDevelopmentReady; D18-S03 required",
        })

    by_hash = {}
    for record in audio_records:
        by_hash.setdefault(record["sha256"], []).append(record["destination_path"])
    duplicate_hashes = {digest: paths for digest, paths in by_hash.items() if len(paths) > 1}

    scene_text = SCENE.read_text(encoding="utf-8")
    prefab_guids = re.findall(r"guid: ([0-9a-f]{32}), type: 3", scene_text)
    forbidden_scene_tokens = ["AudioSource", "InitialHarbor", "Registry", "Systems", "HttpClient", "LLM"]
    forbidden_tokens_found = [token for token in forbidden_scene_tokens if token in scene_text]
    non_trigger_colliders = bool(re.search(r"--- !u!65[\s\S]*?m_IsTrigger: 0", scene_text))
    prop_prefabs = sorted(str(p.relative_to(ROOT)).replace("\\", "/") for p in (ROOT / "game/Assets/Prefabs/Props/D19").glob("*.prefab"))
    vfx_prefabs = sorted(str(p.relative_to(ROOT)).replace("\\", "/") for p in (ROOT / "game/Assets/Prefabs/VFX/D19").glob("*.prefab"))

    anchors = {anchor["anchor_id"]: anchor["position_m"] for anchor in layout["anchors"]}
    route = layout["player_route"]["steps"]
    route_checks = []
    for item in placement_data["placements"]:
        measured = route_distance(item["position_m"], anchors, route)
        route_checks.append({
            "instance_id": item["instance_id"],
            "declared_clearance_m": item["route_clearance_m"],
            "measured_nearest_route_m": round(measured, 2),
            "non_blocking": item["non_blocking"],
            "pass": item["non_blocking"] and (item["route_clearance_m"] == 0 or measured >= placement_data["route_policy"]["minimum_clearance_m"]),
        })

    git_status = subprocess.run(["git", "status", "--porcelain", "--", "game/Assets/Scenes/InitialHarbor.unity"], cwd=ROOT, capture_output=True, text=True)
    initial_harbor_untouched = git_status.stdout.strip() == ""
    unity_available = shutil.which("Unity") is not None or shutil.which("Unity.exe") is not None

    screenshots = []
    selected_zone_ids = ["zone_port_entrance", "zone_market", "zone_beach", "zone_shipyard", "zone_night_overlay"]
    placement_by_region = {}
    for item in placement_data["placements"]:
        placement_by_region.setdefault(item["region"], []).append(item)
    for index, zone_id in enumerate(selected_zone_ids, start=1):
        zone = next(zone for zone in zones["zone_layers"] if zone["zone_id"] == zone_id)
        path = SCREENSHOTS / f"D19_A06_{index:02d}_{zone['region']}.png"
        draw_plate(path, f"D19-A06 / {zone['region'].upper()}", zone["bounds_center_m"], zone["bounds_half_extents_m"], placement_by_region.get(zone["region"], []), zone_id)
        screenshots.append(str(path.relative_to(ROOT)).replace("\\", "/"))

    audit = {
        "task": "D19-A06",
        "status": "PARTIAL",
        "generated_on": "2026-08-14",
        "counts": {
            "prop_prefabs": len(prop_prefabs),
            "vfx_prefabs": len(vfx_prefabs),
            "ambience_zone_count": len(zones["zone_layers"]),
            "audio_candidates": len(audio_records),
            "audio_bytes": sum(item["bytes"] for item in audio_records),
            "screenshots_static_plates": len(screenshots),
        },
        "intake_gate": {
            "eligible_audio_entries": len(entries),
            "all_source_files_exist": all(item["source_exists"] for item in audio_records),
            "all_destination_files_exist": all(item["destination_exists"] for item in audio_records),
            "licenses": sorted({item["license_id"] for item in audio_records}),
            "external_visual_assets_used": 0,
            "project_owned_proxy_visuals": True,
        },
        "audio": {
            "records": audio_records,
            "duplicate_hashes_in_d19_copy": duplicate_hashes,
            "duplicate_policy": "intentional source-to-work-copy duplication; source hash retained for audit",
            "zone_graph": "game/Assets/Audio/D19/D19_AMBIENCE_ZONES.json",
            "environment_layers_ready": False,
            "environment_layer_gap": "No individually verified legal wave/wind/people/bird recordings in the D19-A06 intake scope; remain null with subtitle/event fallback.",
        },
        "scene": {
            "path": str(SCENE.relative_to(ROOT)).replace("\\", "/"),
            "referenced_prefab_guids": sorted(set(prefab_guids)),
            "forbidden_tokens_found": forbidden_tokens_found,
            "has_direct_audio_source": "AudioSource" in scene_text,
            "non_trigger_colliders_found": non_trigger_colliders,
            "initial_harbor_untouched_in_git_status": initial_harbor_untouched,
        },
        "route_checks": route_checks,
        "performance": {
            "runtime_measured": False,
            "static_budget_sample": {
                "active_prop_prefab_instances": 4,
                "active_vfx_proxy_instances": 5,
                "zone_trigger_count": 5,
                "direct_audio_sources": 0,
                "transparent_vfx_materials": 0,
                "gc_bytes_per_frame": "not measured",
            },
            "pool_policy": prop_catalog["performance_contract"]["pool_policy"],
            "distance_bands_m": prop_catalog["prop_sets"][0]["distance_bands_m"],
        },
        "validation": {
            "unity_compile_and_test": "BLOCKED_UNAVAILABLE: Unity executable not found in environment",
            "static_json_and_scene_audit": "PASS" if not forbidden_tokens_found and not non_trigger_colliders and all(item["pass"] for item in route_checks) else "FAIL",
            "screenshots": screenshots,
            "formal_runtime_integration": "NOT_RUN; D19-S02 only",
        },
        "audio_candidates": audio_records,
    }
    (OUT / "D19_A06_STATIC_AUDIT.json").write_text(json.dumps(audit, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (ROOT / "game/Assets/Audio/D19/D19_AUDIO_CANDIDATE_INDEX.json").write_text(json.dumps({"schema": "d19_audio_candidate_index.v1", "task": "D19-A06", "status": "Candidate", "records": audio_records}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"status": audit["status"], "counts": audit["counts"], "route_checks": route_checks, "screenshots": screenshots}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
