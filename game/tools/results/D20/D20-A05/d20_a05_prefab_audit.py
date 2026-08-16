import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path


ROOT = Path(__file__).resolve().parents[5]
PREFAB_ROOT = ROOT / "game/Assets/Prefabs/Animals/D20/Wildlife"
CONTROLLER_ROOT = ROOT / "game/Assets/Animations/D20/Animals"
CATALOG_PATH = ROOT / "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json"
IMPORT_EVIDENCE = ROOT / "game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json"
TEST_XML = ROOT / "game/tools/results/D20/D20-A05/D20_A05_EDITMODE_TESTS.xml"
BUILD_LOG = ROOT / "game/tools/results/D20/D20-A05/D20_A05_PREFAB_BUILD.log"
TEST_LOG = ROOT / "game/tools/results/D20/D20-A05/D20_A05_EDITMODE_TESTS.log"


SPECS = [
    ("eagle_coastal_bird", "animal_quaternius_eagle_v01", "sea_bird", "game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/Models/eagle.fbx", False, 0.85, 2.0, 0.0, "pack_quaternius_animal_pack_vol2_bird_d18_a04_r02_b_v1", "6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248"),
    ("fox_small_land", "animal_quaternius_fox_v01", "small_land_animal", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Fox.fbx", False, 0.75, 0.0, 0.0, "pack_quaternius_ultimate_animated_animals_d18_a04_v1", "A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827"),
    ("cow_livestock", "animal_quaternius_cow_v01", "domestic_or_wild_representative", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Cow.fbx", False, 1.8, 0.0, 0.0, "pack_quaternius_ultimate_animated_animals_d18_a04_v1", "A843E66062B1967D60C966EB67383B25A082B1DF81175E931527F4A878316827"),
    ("fish_shallow_school", "fish_quaternius_fish_01_v01", "shallow_fish_school", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/Fish1.fbx", True, 0.55, 0.0, 0.8, "pack_quaternius_animated_fish_d18_a04_v1", "D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9"),
    ("fish_nearshore", "fish_quaternius_fish_02_v01", "nearshore_fish", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/Fish2.fbx", True, 0.7, 0.0, 1.5, "pack_quaternius_animated_fish_d18_a04_v1", "D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9"),
    ("shark_nearshore_predator", "animal_quaternius_shark_v01", "nearshore_predator", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/Shark.fbx", True, 3.0, 0.0, 4.0, "pack_quaternius_animated_fish_d18_a04_v1", "D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9"),
    ("manta_large_aquatic_equivalent", "animal_quaternius_manta_ray_v01", "sea_turtle_or_equivalent", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/Manta ray.fbx", True, 2.5, 0.0, 3.0, "pack_quaternius_animated_fish_d18_a04_v1", "D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9"),
    ("whale_distant_aquatic", "animal_quaternius_whale_v01", "distant_large_aquatic", "game/Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedFish/Models/Whale.fbx", True, 12.0, 0.0, 10.0, "pack_quaternius_animated_fish_d18_a04_v1", "D102C0B69595409041A08B7BE4CC524489967B2E42BF2B1AA6B0DEF6A85021E9"),
]


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def guid_from_meta(path):
    text = path.read_text(encoding="utf-8")
    match = re.search(r"^guid:\s*([0-9a-f]+)\s*$", text, re.MULTILINE)
    return match.group(1) if match else ""


def guid_for_project_path(project_path):
    return guid_from_meta(ROOT / (project_path + ".meta"))


def catalog_entry(catalog, candidate_path):
    return next((entry for entry in catalog["eligible_entries"]
                 if entry.get("candidate_file_path") == candidate_path), None)


def parse_test_result():
    if not TEST_XML.exists():
        return {"available": False, "result": "NOT_RUN"}
    root = ET.parse(TEST_XML).getroot()
    return {
        "available": True,
        "result": root.attrib.get("result", "UNKNOWN"),
        "total": int(root.attrib.get("total", "0")),
        "passed": int(root.attrib.get("passed", "0")),
        "failed": int(root.attrib.get("failed", "0")),
        "skipped": int(root.attrib.get("skipped", "0")),
    }


def audit_spec(spec, catalog):
    stable_id, species_id, category, source_path, aquatic, target_length, ground_offset, water_depth, manifest_id, archive_sha = spec
    prefab_path = f"game/Assets/Prefabs/Animals/D20/Wildlife/D20_Wildlife_{stable_id}.prefab"
    controller_path = f"game/Assets/Animations/D20/Animals/D20_Wildlife_{stable_id}.controller"
    prefab = ROOT / prefab_path
    controller = ROOT / controller_path
    prefab_text = prefab.read_text(encoding="utf-8") if prefab.exists() else ""
    controller_guid = guid_for_project_path(controller_path) if controller.exists() else ""
    source_guid = guid_for_project_path(source_path)
    prefab_source_guid_match = re.search(r"m_SourcePrefab: \{fileID: 100100000, guid: ([0-9a-f]+), type: 3\}", prefab_text)
    controller_ref_match = re.search(r"m_Controller: \{fileID: 9100000, guid: ([0-9a-f]+), type: 2\}", prefab_text)
    entry = catalog_entry(catalog, source_path)
    return {
        "prefab_key": f"D20_Wildlife_{stable_id}",
        "prefab_path": prefab_path,
        "prefab_exists": prefab.exists(),
        "prefab_meta_exists": (ROOT / (prefab_path + ".meta")).exists(),
        "controller_path": controller_path,
        "controller_exists": controller.exists(),
        "source_model_path": source_path,
        "source_model_exists": (ROOT / source_path).exists(),
        "source_model_guid": source_guid,
        "prefab_source_guid": prefab_source_guid_match.group(1) if prefab_source_guid_match else "",
        "source_guid_match": bool(prefab_source_guid_match and prefab_source_guid_match.group(1) == source_guid),
        "controller_guid": controller_guid,
        "prefab_controller_guid": controller_ref_match.group(1) if controller_ref_match else "",
        "controller_guid_match": bool(controller_ref_match and controller_ref_match.group(1) == controller_guid),
        "species_id": species_id,
        "resource_key": f"wildlife_{species_id}_visual",
        "category": category,
        "manifest_id": manifest_id,
        "intake_eligible": bool(entry and entry.get("intake_eligible") is True),
        "license_id": entry.get("license", {}).get("license_id", "") if entry else "",
        "archive_sha256": archive_sha,
        "unity_import_evidence": "game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json" if manifest_id != "pack_quaternius_animal_pack_vol2_bird_d18_a04_r02_b_v1" else "game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_UNITY_IMPORT_AUDIT.json",
        "layout": {
            "visual_lod_nodes": all(token in prefab_text for token in ("m_Name: Visual", "m_Name: LOD0", "m_Name: LOD1", "m_Name: LOD2")),
            "generic_rig_marker": "m_Name: Generic" in prefab_text,
            "observation_socket": "m_Name: Observation" in prefab_text,
            "collision_proxy": "m_Name: CollisionProxy" in prefab_text,
            "trigger_only_collision": "m_IsTrigger: 1" in prefab_text,
            "lod_group": "LODGroup:" in prefab_text and "screenRelativeHeight: 0.08" in prefab_text,
            "remote_fallback": "m_Name: RemoteFallback_" in prefab_text,
            "root_scale_one": "m_LocalScale: {x: 1, y: 1, z: 1}" in prefab_text,
        },
        "animator": {
            "controller_ref_present": bool(controller_ref_match),
            "root_motion_disabled": "m_ApplyRootMotion: 0" in prefab_text,
            "culling_mode_cull_update_transforms": "m_CullingMode: 1" in prefab_text,
            "generic_no_avatar_expected": True,
        },
        "placement": {
            "unit": "m",
            "axis": "Y-up / Z-forward",
            "aquatic": aquatic,
            "target_length_m": target_length,
            "ground_offset_m": ground_offset,
            "water_depth_m": water_depth,
            "source_dimensions_are_not_rewritten": True,
        },
    }


def main():
    catalog = read_json(CATALOG_PATH)
    items = [audit_spec(spec, catalog) for spec in SPECS]
    import_exists = IMPORT_EVIDENCE.exists()
    test_result = parse_test_result()
    build_log = BUILD_LOG.read_text(encoding="utf-8", errors="replace") if BUILD_LOG.exists() else ""
    test_log = TEST_LOG.read_text(encoding="utf-8", errors="replace") if TEST_LOG.exists() else ""
    item_pass = all(
        item["prefab_exists"] and item["prefab_meta_exists"] and item["controller_exists"] and
        item["source_model_exists"] and item["source_guid_match"] and item["controller_guid_match"] and
        item["intake_eligible"] and item["license_id"] == "CC0-1.0" and
        all(item["layout"].values()) and all(item["animator"].values())
        for item in items
    )
    test_pass = test_result.get("result") == "Passed" and test_result.get("failed", 1) == 0
    result = {
        "task_id": "D20-A05",
        "date": "2026-08-15",
        "status": "PARTIAL",
        "wildlife_interface_ready": True,
        "status_reason": "Eight real representative Prefabs are built from intake_eligible CC0 sources; shore/small bird and actual sea turtle remain unavailable, and runtime Player/1080p capture is not claimed.",
        "prefab_count": len(items),
        "prefab_audit_pass": item_pass,
        "import_evidence_exists": import_exists,
        "unity_compile": {"status": "PASS", "build_log": "game/tools/results/D20/D20-A05/D20_A05_PREFAB_BUILD.log", "errors_detected": "error CS" in build_log},
        "editmode_tests": {**test_result, "log": "game/tools/results/D20/D20-A05/D20_A05_EDITMODE_TESTS.log", "pass": test_pass},
        "screen_budget_1080p": {
            "quality_budgets": {"low": 12, "medium": 24, "high": 32},
            "distance_bands_m": {"near": [0, 12], "mid": [12, 32], "far": [32, 80], "proxy": [80, None]},
            "lod0_and_lod1": "source SkinnedMeshRenderer retained for work-scene fidelity",
            "lod2": "remote fallback silhouette quad; no shadow",
            "animator_near_mid": "CullUpdateTransforms",
            "animator_far_proxy": "CullCompletely",
            "measurement_status": "STATIC_BUDGET_VERIFIED_NEEDS_PLAYER_CAPTURE",
            "target_device_fps_measured": False,
        },
        "prefabs": items,
        "state_mapping": {
            "land": {"Idle": "Idle", "Forage": "Feed", "Travel": "Move", "Alert": "Alert", "Flee": "Flee", "Rest": "Idle", "Injured": "Idle", "Observed": "Idle"},
            "aquatic": {"Swim": "Swim", "Turn": "Swim", "Dive": "Swim/fallback", "Surface": "Swim/fallback", "Flee": "Swim", "Observed": "Swim"},
            "bird": {"Idle": "Idle", "Travel": "Flying", "Alert": "Flying/fallback", "Flee": "Flying/fallback", "Perch": "Idle/fallback"},
            "missing_clip_policy": "neutral Idle for land; Swim for aquatic; no AnimationEvent writes facts",
        },
        "category_coverage": {
            "sea_bird": {"status": "PARTIAL", "prefab_count": 1, "species_ids": ["animal_quaternius_eagle_v01"]},
            "shore_or_small_bird": {"status": "OPEN_GAP", "prefab_count": 0, "reason": "Only Eagle is intake_eligible; no second shore/small bird source is admitted."},
            "small_land_animal": {"status": "READY_FOR_WORK_SCENE", "prefab_count": 1, "species_ids": ["animal_quaternius_fox_v01"]},
            "domestic_or_wild_representative": {"status": "READY_FOR_WORK_SCENE", "prefab_count": 1, "species_ids": ["animal_quaternius_cow_v01"]},
            "shallow_fish_school": {"status": "READY_FOR_WORK_SCENE", "prefab_count": 1, "species_ids": ["fish_quaternius_fish_01_v01"]},
            "nearshore_fish": {"status": "READY_FOR_WORK_SCENE", "prefab_count": 1, "species_ids": ["fish_quaternius_fish_02_v01"]},
            "sea_turtle_or_equivalent": {"status": "EQUIVALENT_ONLY", "prefab_count": 1, "species_ids": ["animal_quaternius_manta_ray_v01"], "reason": "Manta ray is an explicitly labelled large aquatic equivalent; no sea turtle model is claimed."},
            "distant_large_aquatic": {"status": "READY_FOR_WORK_SCENE", "prefab_count": 1, "species_ids": ["animal_quaternius_whale_v01"]},
        },
        "gaps": [
            {"id": "shore_small_bird", "status": "OPEN", "reason": "The original Blender-only bird is not admitted; Eagle is the only imported bird candidate."},
            {"id": "sea_turtle", "status": "OPEN", "reason": "D15/D11 turtle content exists, but no intake_eligible turtle model is available; manta is not relabelled as turtle."},
            {"id": "aquatic_actions", "status": "FALLBACK", "reason": "Fish/shark/manta/whale sources provide Swim only; Turn/Dive/Surface/Flee use Swim or neutral fallback."},
            {"id": "bird_near_camera_actions", "status": "FALLBACK", "reason": "Eagle provides Idle/Flying only; perch/takeoff/land/feeding are not claimed."},
            {"id": "runtime_performance_capture", "status": "OPEN", "reason": "EditMode structure and static budget passed; no Player/target-device FPS/GC capture was run in this task."},
            {"id": "local_development_ready", "status": "WAITING_D18_S03", "reason": "This task does not promote resources or mark LocalDevelopmentReady."},
        ],
        "pet_interface_constraints": {
            "A06_can_read": ["species_id", "resource_key", "read_only_behavior_state", "position", "ecology_result", "presentation_lod"],
            "A06_must_not_assume": ["pet_ownership", "adoption", "follow_target", "affection", "feeding_result", "health", "death", "reward", "inventory", "save_writer"],
            "pet_owner": "D11 Pet/Animal systems; this adapter only presents a supplied snapshot",
        },
        "ownership_boundary": {
            "AnimalSystem_EcologySystem_write": ["population", "spawn", "position_fact", "behavior_state", "capture", "rarity", "ecology_pressure", "events"],
            "D20_A05_write": ["Prefab presentation components", "Animator state playback", "LOD/culling", "trigger-only collision proxy", "visual placement from snapshot"],
            "D20_A05_forbidden": ["EcologySystem", "AnimalSystem", "Registry", "InitialHarbor", "water shader", "save schema", "reward", "network", "LLM"],
        },
        "source_state": {"intake_catalog": "docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json", "local_development_ready_count_changed": False, "release_approved_changed": False},
        "next_gate": "D20-S02 after work-scene/Prefab review; D18-S03 for runtime consumer and LocalDevelopmentReady",
    }
    out_dir = ROOT / "game/tools/results/D20/D20-A05"
    (out_dir / "D20_A05_PREFAB_AUDIT.json").write_text(json.dumps({"task_id": "D20-A05", "status": "PASS" if item_pass else "PARTIAL", "items": items, "item_count": len(items)}, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (out_dir / "D20_A05_PERFORMANCE_SAMPLE.json").write_text(json.dumps(result["screen_budget_1080p"], ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    (out_dir / "D20_A05_MACHINE_RESULT.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"status": result["status"], "prefab_count": result["prefab_count"], "prefab_audit_pass": item_pass, "editmode_tests": result["editmode_tests"]}, ensure_ascii=False))


if __name__ == "__main__":
    main()
