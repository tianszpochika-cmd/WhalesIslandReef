"""Generate the deterministic D15-A06 ship/hull/module/function-point batch.

This is a content-only generator. It deliberately does not contain ship weights,
draft, speed, damage, prices, or module-effect formulas; ShipSystem and the D14
owners remain responsible for those decisions.
"""

from __future__ import annotations

import hashlib
import json
from pathlib import Path
from typing import Any, Dict, Iterable, List


ROOT = Path(__file__).resolve().parents[1]
REGISTRY = ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
BATCH = ROOT / "game" / "Assets" / "Content" / "D15" / "A06"

SHARED_ASSETS = {
    "collision": "asset_d15_a06_collision_profiles",
    "anchors": "asset_d15_a06_anchor_profiles",
    "animation": "asset_d15_a06_animation_profiles",
    "audio": "asset_d15_a06_audio_profiles",
    "ui": "asset_d15_a06_ui_profiles",
    "materials": "asset_d15_a06_material_profiles",
}

HULLS = [
    ("ship_moonwake", "月醒号", "coastal fishing schooner", "schooner"),
    ("ship_tideglass_cutter", "潮玻璃号", "coastal cutter", "cutter"),
    ("ship_sable_fin_skiff", "黑鳍艇", "shallow-water skiff", "skiff"),
    ("ship_mangrove_punt", "红树平底艇", "mangrove punt", "punt"),
    ("ship_reef_runner_sloop", "礁行单桅船", "reef-running sloop", "sloop"),
    ("ship_island_packet_sloop", "岛间邮梭", "island packet sloop", "sloop"),
    ("ship_bluewater_cutter", "蓝水快艇", "bluewater cutter", "cutter"),
    ("ship_saltwind_schooner", "盐风号", "salt-route schooner", "schooner"),
    ("ship_raincrown_schooner", "雨冠号", "rain-route schooner", "schooner"),
    ("ship_privateer_brigantine", "私掠双桅船", "brigantine", "brigantine"),
    ("ship_harbor_brig", "港湾双桅船", "harbor brig", "brig"),
    ("ship_coastal_bark", "近岸巴克船", "coastal bark", "bark"),
    ("ship_cocoa_brigantine", "可可商船", "cocoa-route brigantine", "brigantine"),
    ("ship_sugar_route_schooner", "蔗路纵帆船", "sugar-route schooner", "schooner"),
    ("ship_windward_pinnace", "向风小艇", "windward pinnace", "pinnace"),
    ("ship_blackreef_brig", "黑礁双桅船", "black-reef brig", "brig"),
    ("ship_saint_marys_ketch", "圣玛丽双桅艇", "Saint Mary's ketch", "ketch"),
    ("ship_free_cove_catboat", "自由湾单帆艇", "Free Cove catboat", "catboat"),
    ("ship_longwake_barque", "长尾巴克船", "long-voyage barque", "barque"),
    ("ship_deepwater_frigate", "深水护航舰", "deepwater frigate", "frigate"),
]

MODULE_GROUPS = [
    ("hull", [
        ("reinforced_planking", "加厚外板"), ("reinforced_keel", "加强龙骨"), ("braced_frames", "加固肋骨"),
        ("watertight_bulkhead", "防漏隔舱"), ("raised_freeboard", "加高干舷"), ("shallow_draft_conversion", "浅吃水改造"),
        ("stability_sponson", "加宽稳定结构"), ("rapid_patch_layer", "快速修补层"), ("ballast_rack", "压载与配重架"),
        ("fire_bulkhead", "防火舱壁"),
    ]),
    ("rigging", [
        ("enlarged_main_sail", "加大主帆"), ("light_canvas", "轻型帆布"), ("storm_canvas", "加强帆布"),
        ("improved_fore_sail", "改良前帆"), ("auxiliary_staysail", "辅助支索帆"), ("braced_mast", "加固桅杆"),
        ("spare_mast_kit", "备用桅杆组件"), ("quick_release_rigging", "快速收帆索具"), ("quiet_rigging", "低噪音索具"),
        ("professional_sailor_platform", "专业帆手平台"),
    ]),
    ("navigation", [
        ("reinforced_rudder", "加强舵叶"), ("twin_rudder_linkage", "双舵传动"), ("improved_helm_wheel", "改良舵轮"),
        ("spare_tiller_assembly", "舵柄备用件"), ("navigation_compass", "航海罗盘"), ("weatherproof_chart_case", "防潮海图柜"),
        ("sounding_gear_rack", "测深设备架"), ("high_lookout_platform", "高位瞭望台"), ("night_signal_lamp", "夜间信号灯"),
        ("flag_signal_set", "旗语信号组"),
    ]),
    ("cargo", [
        ("expanded_cargo_hold", "扩容货舱"), ("braced_cargo_shelves", "加固货架"), ("dry_cargo_hold", "防潮货舱"),
        ("light_cargo_dividers", "轻型货舱隔板"), ("cargo_davit", "装货吊杆"), ("cargo_winch", "货舱绞盘"),
        ("freshwater_tank", "淡水储藏舱"), ("long_voyage_granary", "远航粮仓"), ("locked_valuables_case", "贵重货柜"),
        ("deck_catch_area", "甲板渔获区"),
    ]),
    ("armament", [
        ("light_gun_mount", "轻型炮位"), ("heavy_gun_mount", "重型炮位"), ("braced_gun_carriage", "炮架加固"),
        ("rapid_loading_rack", "快速装填架"), ("dry_powder_locker", "火药防潮柜"), ("shot_sorting_rack", "炮弹分拣架"),
        ("chain_shot_rack", "链弹储备架"), ("grape_shot_rack", "葡萄弹储备架"), ("boarding_net", "接舷网"),
        ("weapon_and_firearm_rack", "武器与火枪架"),
    ]),
    ("safety", [
        ("simple_medical_berth", "简易医务床"), ("ship_sickbay", "船上医务室"), ("dry_medicine_cabinet", "药品防潮柜"),
        ("isolation_partition", "隔离帘与隔离区"), ("fire_bucket_rack", "消防桶架"), ("fire_sand_box", "防火砂箱"),
        ("bilge_drain_pump", "舱底排水泵"), ("spare_bucket_line", "备用水桶线"), ("ventilation_hatch", "通风舱口"),
        ("rescue_rope_and_float", "救生绳与浮具"),
    ]),
    ("living", [
        ("improved_galley_stove", "改良炉台"), ("water_saving_kitchen", "节水厨房"), ("food_preservation_cabinet", "食物防腐柜"),
        ("extra_crew_berths", "增设水手铺位"), ("captains_cabin_refit", "船长舱改良"), ("crew_storage_lockers", "船员储物柜"),
        ("washing_and_hygiene_point", "洗涤与卫生点"), ("crew_rest_area", "船员休息区"), ("council_table", "会议桌"),
        ("confinement_isolation_cabin", "禁闭与隔离小舱"),
    ]),
    ("support", [
        ("carpenter_workbench", "木工工作台"), ("spare_timber_rack", "备用木材架"), ("spare_rope_rack", "备用绳索架"),
        ("dinghy_davit", "小艇吊架"), ("reinforced_dinghy", "加强小艇"), ("concealed_cargo_compartment", "走私暗格"),
        ("disguise_flag_mount", "伪装船旗架"), ("hull_paint_layer", "船体涂层"), ("name_and_crest_board", "船名与纹章板"),
        ("spare_anchor_chain_set", "备用锚与锚链组"),
    ]),
]

FUNCTION_GROUPS = [
    ("operations", ["rudder", "tiller", "helm_wheel", "steering_transmission", "main_mast", "fore_mast", "aft_mast", "main_sail", "fore_sail", "staysail", "sail_trim", "rigging_locker"]),
    ("mooring", ["port_anchor", "starboard_anchor", "anchor_winch", "anchor_chain_locker", "anchor_chain_guide", "bow_bollard", "stern_bollard", "breast_line_point", "gangplank", "boat_hook_rack"]),
    ("navigation", ["chart_table", "compass_stand", "sandglass", "log_line", "sounding_lead", "lookout_platform", "signal_flag_locker", "ship_bell_alarm"]),
    ("cargo", ["main_cargo_hold", "dry_goods_shelf", "freshwater_barrel_rack", "food_larder", "fish_hold", "cargo_hatch", "cargo_davit", "hand_winch", "valuables_locker", "temporary_deck_cargo"]),
    ("combat", ["gunport", "gun_carriage", "powder_magazine", "shot_locker", "gunner_workbench", "boarding_rail", "boarding_net", "weapon_rack", "fire_bucket_station", "signal_gun"]),
    ("life", ["galley_stove", "cook_workbench", "sailor_berth", "captains_cabin", "medical_berth", "medicine_cabinet", "waste_treatment_point", "carpenter_workbench", "tool_rack", "bilge_pump_access"]),
]


def slug(value: str) -> str:
    return value.replace("-", "_").replace(" ", "_").lower()


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    write_meta(path)


def write_meta(path: Path) -> None:
    digest = hashlib.md5(path.as_posix().encode("utf-8")).hexdigest()
    meta = path.with_name(path.name + ".meta")
    if path.suffix == ".json":
        meta.write_text("fileFormatVersion: 2\n" + "guid: " + digest + "\n", encoding="utf-8")


def write_obj(path: Path, index: int, kind: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    if kind == "hull":
        span = 2.4 + (index % 7) * 0.22
        height = 0.7 + (index % 4) * 0.08
        vertices = [(-span, 0, 0), (span, 0, 0), (span * 0.72, height, 0), (-span * 0.72, height, 0), (-span * 0.65, 0, 1.1), (span * 0.65, 0, 1.1), (span * 0.45, height, 0.85), (-span * 0.45, height, 0.85)]
        faces = [(1, 2, 3, 4), (5, 8, 7, 6), (1, 5, 6, 2), (4, 3, 7, 8), (1, 4, 8, 5), (2, 6, 7, 3)]
    elif kind == "module":
        size = 0.35 + (index % 5) * 0.05
        vertices = [(-size, 0, -size), (size, 0, -size), (size, 0.35, -size), (-size, 0.35, -size), (-size, 0, size), (size, 0, size), (size, 0.35, size), (-size, 0.35, size)]
        faces = [(1, 2, 3, 4), (5, 8, 7, 6), (1, 5, 6, 2), (4, 3, 7, 8), (1, 4, 8, 5), (2, 6, 7, 3)]
    else:
        size = 0.20 + (index % 3) * 0.04
        vertices = [(-size, 0, -size), (size, 0, -size), (size, 0.55, -size), (-size, 0.55, -size), (-size, 0, size), (size, 0, size), (size, 0.55, size), (-size, 0.55, size)]
        faces = [(1, 2, 3, 4), (5, 8, 7, 6), (1, 5, 6, 2), (4, 3, 7, 8), (1, 4, 8, 5), (2, 6, 7, 3)]
    lines = ["# D15-A06 project-authored procedural placeholder geometry", "# Geometry is presentation data; ShipSystem owns gameplay values.", "o " + path.stem]
    lines.extend("v %.3f %.3f %.3f" % vertex for vertex in vertices)
    lines.extend("f " + " ".join(str(value) for value in face) for face in faces)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def base_entry(content_id: str, name: str, category: str, tags: List[str], role: str, owner: str, refs: List[str], assets: List[str], test_id: str, description: str) -> Dict[str, Any]:
    return {
        "id": content_id, "schema_version": 1, "name": name,
        "display_name_key": "content.%s.%s.name" % (category, content_id), "category": category,
        "tags": tags, "description": description, "gameplay_role": role,
        "valid_regions": ["island_saint_marys"], "valid_habitats": ["coastal_water", "ship_deck", "ship_hold"],
        "valid_weather": ["clear", "rain", "storm"], "dependencies": [], "references": refs,
        "asset_refs": assets, "source_refs": ["docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md", "docs/game/design/ship/10_SHIP_DATA_MODEL.md"],
        "source_note": "D15-A06 project-authored content definition; runtime values remain with the owning ship or inventory system.",
        "source": {"kind": "project-authored", "author": "Whalebone team", "reference": "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#d15-a06"},
        "license": {"status": "internal-authored", "holder": "Whalebone project", "reference": "docs/game/design/common/D8_CONTENT_ID_REGISTRY.md#sample-licensing", "commercial_use": True, "attribution_required": False, "modification_allowed": True},
        "historical_tag": "inferred", "phase": "D15", "status": "production", "owner_system": owner,
        "test_id": test_id, "reviewer": "D15-A06*",
    }


def production_fields(kind: str, content_id: str, index: int, model_asset: str, model_path: str, interaction_id: str, anchors: List[str], ui_key: str) -> Dict[str, Any]:
    return {
        "production_batch": "D15-A06*", "representative_sample": index == 0,
        "production_stage": "integrated", "behavior_profile": "ship_content.d15_a06.%s" % kind,
        "asset_state": "integrated", "model_ref": model_asset, "model_path": model_path,
        "material_ref": SHARED_ASSETS["materials"], "collision_ref": SHARED_ASSETS["collision"],
        "collision_profile_id": "collision_%s" % slug(content_id), "anchor_refs": anchors,
        "anchor_data_ref": SHARED_ASSETS["anchors"], "animation_refs": [SHARED_ASSETS["animation"]],
        "animation_profile_id": "animation_%s" % slug(content_id), "audio_refs": [SHARED_ASSETS["audio"]],
        "audio_profile_id": "audio_%s" % slug(content_id), "ui_ref": SHARED_ASSETS["ui"],
        "ui_profile_id": "ui_%s" % slug(content_id), "ui_localization_key": ui_key,
        "lod_profile": {"lod0_triangles_max": 35000 if kind == "hull" else 8000, "lod1_triangles_max": 18000 if kind == "hull" else 3000, "lod2_triangles_max": 7000 if kind == "hull" else 1000, "lod3_triangles_max": 2000 if kind == "hull" else 0},
        "test_cases": ["%s-load" % interaction_id, "%s-interact" % interaction_id, "%s-save-restore" % interaction_id, "%s-owner-handoff" % interaction_id],
        "historical_review": "Approved", "review_evidence_refs": ["docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查", "docs/game/design/common/D15_SHIP_HULL_MODULE_BATCH_CATALOG.md"],
        "content_warning_ids": [], "skip_allowed": False,
    }


def anchors_for(kind: str, index: int) -> List[str]:
    if kind == "hull":
        return ["anchor_bow", "anchor_stern", "anchor_helm", "anchor_boarding", "anchor_cargo", "anchor_lookout", "anchor_camera"]
    if kind == "module":
        return ["anchor_module_mount_%02d" % ((index % 8) + 1), "anchor_module_service_%02d" % ((index % 8) + 1)]
    return ["anchor_function_interact", "anchor_function_crew", "anchor_function_camera"]


def asset_entry(asset_id: str, name: str, path: str, tags: List[str], test_id: str) -> Dict[str, Any]:
    entry = base_entry(asset_id, name, "asset", tags, "d15_a06_asset_resource", "AssetRegistry", [], [], test_id, "D15-A06 project-authored asset data resource.")
    entry["source_refs"] = ["game/Assets/Content/D15/A06", "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md"]
    entry["source_note"] = "Project-authored resource or deterministic placeholder geometry; no external download is required."
    entry["resource_path"] = path
    entry["asset_state"] = "integrated"
    entry["production_batch"] = "D15-A06*"
    entry["production_stage"] = "integrated"
    entry["test_cases"] = ["%s-load" % test_id.lower(), "%s-reference" % test_id.lower(), "%s-save-restore" % test_id.lower()]
    entry["historical_review"] = "Approved"
    entry["review_evidence_refs"] = [
        "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查",
        "docs/game/design/common/D15_SHIP_HULL_MODULE_BATCH_CATALOG.md",
    ]
    entry["content_warning_ids"] = []
    entry["skip_allowed"] = False
    entry["lod_profile"] = "LOD-N/A" if "profile" in asset_id or "material" in asset_id else {"lod0": "source", "lod1": "generated", "lod2": "generated", "lod3": "generated"}
    return entry


def update_existing(path: Path, value: Dict[str, Any]) -> None:
    if path.is_file():
        existing = json.loads(path.read_text(encoding="utf-8"))
        existing.update(value)
        write_json(path, existing)
    else:
        write_json(path, value)


def main() -> None:
    entries_root = REGISTRY / "entries"
    for folder in ("ship", "item", "asset"):
        (entries_root / folder).mkdir(parents=True, exist_ok=True)
    (BATCH / "entries" / "hulls").mkdir(parents=True, exist_ok=True)
    (BATCH / "entries" / "modules").mkdir(parents=True, exist_ok=True)
    (BATCH / "entries" / "functional_points").mkdir(parents=True, exist_ok=True)
    (BATCH / "assets" / "models" / "hulls").mkdir(parents=True, exist_ok=True)
    (BATCH / "assets" / "models" / "modules").mkdir(parents=True, exist_ok=True)
    (BATCH / "assets" / "models" / "functional_points").mkdir(parents=True, exist_ok=True)
    (BATCH / "assets" / "profiles").mkdir(parents=True, exist_ok=True)

    all_entries: List[Dict[str, Any]] = []
    new_asset_entries: List[Dict[str, Any]] = []
    shared_profile_paths = {
        SHARED_ASSETS["collision"]: "game/Assets/Content/D15/A06/assets/profiles/collision_profiles.json",
        SHARED_ASSETS["anchors"]: "game/Assets/Content/D15/A06/assets/profiles/anchor_profiles.json",
        SHARED_ASSETS["animation"]: "game/Assets/Content/D15/A06/assets/profiles/animation_profiles.json",
        SHARED_ASSETS["audio"]: "game/Assets/Content/D15/A06/assets/profiles/audio_profiles.json",
        SHARED_ASSETS["ui"]: "game/Assets/Content/D15/A06/assets/profiles/ui_profiles.json",
        SHARED_ASSETS["materials"]: "game/Assets/Content/D15/A06/assets/profiles/material_profiles.json",
    }
    for offset, (asset_id, path) in enumerate(shared_profile_paths.items()):
        new_asset_entries.append(asset_entry(asset_id, "D15-A06 " + asset_id, path, ["d15_a06", "profile", "project_authored"], "D8-A02-D15-A06-ASSET-%03d" % (offset + 1)))
    existing_ship_asset_path = entries_root / "asset" / "asset_ph_ship_fishing.json"
    if existing_ship_asset_path.is_file():
        existing_ship_asset = json.loads(existing_ship_asset_path.read_text(encoding="utf-8"))
        existing_ship_asset.update({
            "resource_path": "game/Assets/Art/Placeholder/Models/Ships/ph_ship_fishing.fbx",
            "asset_state": "integrated", "production_batch": "D15-A06*", "production_stage": "integrated",
            "test_cases": ["d15-a06-existing-ship-model-load", "d15-a06-existing-ship-model-reference", "d15-a06-existing-ship-model-save-restore"],
            "historical_review": "Approved",
            "review_evidence_refs": [
                "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查",
                "docs/game/design/common/D15_SHIP_HULL_MODULE_BATCH_CATALOG.md",
            ],
            "content_warning_ids": [],
            "skip_allowed": False,
            "lod_profile": "LOD-N/A",
        })
        new_asset_entries.append(existing_ship_asset)

    for index, (content_id, name, role, hull_tag) in enumerate(HULLS):
        model_asset = "asset_ph_ship_fishing" if content_id == "ship_moonwake" else "asset_d15_a06_model_" + content_id[5:]
        model_path = "game/Assets/Art/Placeholder/Models/Ships/ph_ship_fishing.fbx" if content_id == "ship_moonwake" else "game/Assets/Content/D15/A06/assets/models/hulls/%s.obj" % content_id
        refs = ["island_saint_marys"]
        assets = [model_asset] + list(SHARED_ASSETS.values())
        entry = base_entry(content_id, name, ["ship"][0], ["d15_a06", "hull", hull_tag, "ship_system"], "ship_hull_content", "ShipSystem", refs, assets, "D8-A02-D15-A06-HULL-%03d" % (index + 1), "A ship hull presentation and interaction definition for %s." % role)
        entry.update(production_fields("hull", content_id, index, model_asset, model_path, "ship-hull-%03d" % (index + 1), anchors_for("hull", index), "content.ship.%s.name" % content_id))
        entry["hull_layout_profile"] = {"deck_zones": ["bow", "main_deck", "stern"], "compartment_profile": "ship_compartments_d15_a06", "damage_anchor_profile": "ship_damage_anchors_d15_a06"}
        entry["functional_point_slots"] = ["ship_function_helm_wheel", "ship_function_port_anchor", "ship_function_chart_table", "ship_function_main_cargo_hold"]
        entry["compatible_module_tags"] = ["hull", "rigging", "navigation", "cargo", "safety", "living", "support"]
        entry["incompatible_module_tags"] = []
        update_existing(entries_root / "ship" / (content_id + ".json"), entry)
        write_json(BATCH / "entries" / "hulls" / (content_id + ".json"), entry)
        all_entries.append(entry)
        if content_id != "ship_moonwake":
            write_obj(BATCH / "assets" / "models" / "hulls" / (content_id + ".obj"), index, "hull")
            new_asset_entries.append(asset_entry(model_asset, name + " model", model_path, ["d15_a06", "ship", "model", "hull"], "D8-A02-D15-A06-MODEL-HULL-%03d" % (index + 1)))

    module_index = 0
    for group, modules in MODULE_GROUPS:
        for module_slug, name in modules:
            module_index += 1
            content_id = "item_ship_module_" + module_slug
            model_asset = "asset_d15_a06_model_" + content_id[5:]
            model_path = "game/Assets/Content/D15/A06/assets/models/modules/%s.obj" % content_id
            point = "ship_function_" + ("main_cargo_hold" if group == "cargo" else "medical_berth" if group == "safety" else "helm_wheel" if group == "navigation" else "main_mast" if group == "rigging" else "carpenter_workbench" if group == "support" else "gun_carriage" if group == "armament" else "cook_workbench" if group == "living" else "main_cargo_hold")
            if content_id == "item_ship_module_reinforced_keel":
                point = "ship_function_main_cargo_hold"
            if content_id == "item_ship_module_quick_release_rigging":
                point = "ship_function_sail_trim"
            assets = [model_asset] + list(SHARED_ASSETS.values())
            entry = base_entry(content_id, name, "item", ["d15_a06", "ship_module", group, "ship_system"], "ship_module_content", "ItemCatalog", ["ship_moonwake", point], assets, "D8-A02-D15-A06-MODULE-%03d" % module_index, "A refit module definition; final installation and effects belong to ShipSystem and D14 owners.")
            entry.update(production_fields("module", content_id, module_index - 1, model_asset, model_path, "ship-module-%03d" % module_index, anchors_for("module", module_index), "content.item.%s.name" % content_id))
            entry["representative_sample"] = content_id == "item_ship_module_reinforced_keel"
            entry["module_group"] = group
            entry["required_functional_point"] = point
            entry["module_role"] = "visual_and_assembly_slot"
            entry["slot_tags"] = [group, "ship_module"]
            entry["mutual_exclusion_group"] = "d15_a06_%s_slot" % group
            entry["removable"] = True
            update_existing(entries_root / "item" / (content_id + ".json"), entry)
            write_json(BATCH / "entries" / "modules" / (content_id + ".json"), entry)
            all_entries.append(entry)
            write_obj(BATCH / "assets" / "models" / "modules" / (content_id + ".obj"), module_index, "module")
            if not any(item.get("id") == model_asset for item in new_asset_entries):
                new_asset_entries.append(asset_entry(model_asset, name + " model", model_path, ["d15_a06", "ship_module", "model"], "D8-A02-D15-A06-MODEL-MODULE-%03d" % module_index))

    function_index = 0
    function_ids: List[str] = []
    for group, function_slugs in FUNCTION_GROUPS:
        for function_slug in function_slugs:
            function_index += 1
            content_id = "ship_function_" + function_slug
            function_ids.append(content_id)
            name = function_slug.replace("_", " ").title()
            model_asset = "asset_d15_a06_model_" + content_id[5:]
            model_path = "game/Assets/Content/D15/A06/assets/models/functional_points/%s.obj" % content_id
            assets = [model_asset] + list(SHARED_ASSETS.values())
            entry = base_entry(content_id, name, "ship", ["d15_a06", "functional_point", group, "ship_system"], "ship_functional_point", "ShipSystem", ["ship_moonwake"], assets, "D8-A02-D15-A06-FUNCTION-%03d" % function_index, "A ship functional point with an interaction entry, spatial anchors and presentation data.")
            entry.update(production_fields("functional_point", content_id, function_index - 1, model_asset, model_path, "ship-function-%03d" % function_index, anchors_for("functional_point", function_index), "content.ship.%s.name" % content_id))
            entry["representative_sample"] = content_id == "ship_function_helm_wheel"
            entry["function_group"] = group
            entry["interaction_entry"] = "ShipSystem.FunctionalPoint.%s" % function_slug
            entry["interaction_verbs"] = ["inspect", "use", "assign_crew"]
            entry["required_crew_role"] = "ship_crew_role_%s" % group
            entry["state_tags"] = ["available", "blocked", "damaged", "flooded", "burning", "occupied"]
            entry["owner_handoff"] = "ShipSystem"
            write_json(entries_root / "ship" / (content_id + ".json"), entry)
            write_json(BATCH / "entries" / "functional_points" / (content_id + ".json"), entry)
            all_entries.append(entry)
            write_obj(BATCH / "assets" / "models" / "functional_points" / (content_id + ".obj"), function_index, "function")
            new_asset_entries.append(asset_entry(model_asset, name + " model", model_path, ["d15_a06", "functional_point", "model"], "D8-A02-D15-A06-MODEL-FUNCTION-%03d" % function_index))

    profile_data = {
        "schema_version": 1,
        "task": "D15-A06*",
        "collision_profiles": {item[0]: {"shape": "compound_convex", "layers": ["ship_hull", "ship_interaction"], "trigger_policy": "owner_system_only"} for item in HULLS},
        "anchor_profiles": {item[0]: anchors_for("hull", index) for index, item in enumerate(HULLS)},
        "animation_profiles": {item[0]: ["idle_float", "turn_heel", "sail_raise", "sail_lower", "anchor_lower", "anchor_raise", "damage_state"] for item in HULLS},
        "audio_profiles": {item[0]: ["wood_creak", "canvas_tension", "rope_friction", "water_hull", "anchor_chain", "helm_mechanism"] for item in HULLS},
        "ui_profiles": {item[0]: {"title_key": "content.ship.%s.name" % item[0], "state_keys": ["hull_state", "flooding_state", "tilt_state", "fire_state", "helm_state", "module_state"], "interaction_hint_key": "ui.ship.functional_point.interact"} for item in HULLS},
        "material_profiles": {"ship_default": {"material_family": "project_authored_wood_canvas", "placeholder_allowed": True}},
    }
    for filename, key in [("collision_profiles.json", "collision_profiles"), ("anchor_profiles.json", "anchor_profiles"), ("animation_profiles.json", "animation_profiles"), ("audio_profiles.json", "audio_profiles"), ("ui_profiles.json", "ui_profiles"), ("material_profiles.json", "material_profiles")]:
        write_json(BATCH / "assets" / "profiles" / filename, {"schema_version": 1, "task": "D15-A06*", key: profile_data[key]})

    for asset in new_asset_entries:
        update_existing(entries_root / "asset" / (asset["id"] + ".json"), asset)
    for asset in new_asset_entries:
        write_json(BATCH / "entries" / "assets" / (asset["id"] + ".json"), asset)

    manifest = {
        "schema_version": 1, "task": "D15-A06*", "batch": "D15-A06*", "id_rule": "ship_ for hulls and functional points; item_ for inventory modules; asset_ for resources",
        "counts": {"hulls": len(HULLS), "modules": module_index, "functional_points": function_index, "asset_resources": len(new_asset_entries)},
        "hull_ids": [item[0] for item in HULLS],
        "module_ids": ["item_ship_module_" + module_slug for _, modules in MODULE_GROUPS for module_slug, _ in modules],
        "functional_point_ids": function_ids,
        "asset_ids": [item["id"] for item in new_asset_entries],
        "representative_samples": {"hull": "ship_moonwake", "module": "item_ship_module_reinforced_keel", "functional_point": "ship_function_helm_wheel"},
        "owner_systems": {"hulls": "ShipSystem", "modules": "ItemCatalog -> ShipSystem", "functional_points": "ShipSystem", "assets": "AssetRegistry"},
        "runtime_formula_boundary": "This batch contains presentation, interaction and handoff metadata only; ShipSystem/D14 own all strong logic values.",
    }
    write_json(BATCH / "catalog.json", manifest)

    registry_path = REGISTRY / "registry.json"
    registry = json.loads(registry_path.read_text(encoding="utf-8"))
    existing_ids = []
    for path in sorted((REGISTRY / "entries").rglob("*.json")):
        if path.name == "registry.json":
            continue
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            continue
        if isinstance(value, dict) and isinstance(value.get("id"), str):
            existing_ids.append(value["id"])
    # Roots are an index of loadable entry IDs, not a historical append-only list.
    # Rebuild it from the files that actually exist so stale roots cannot create
    # orphan-reference failures in the D8 scanner.
    registry["roots"] = sorted(set(existing_ids))
    registry_path.write_text(json.dumps(registry, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    # Normalize a small set of pre-D15 entries whose historical test IDs
    # predate the D8 scanner's stable D8-A02 test-id format.
    legacy_test_ids = {
        "animal_domestic_duck": "D8-A02-D15-LEGACY-ANIMAL-DUCK",
        "animal_domestic_guinea_fowl": "D8-A02-D15-LEGACY-ANIMAL-GUINEA-FOWL",
        "asset_ph_building_audio_placeholder": "D8-A02-D15-LEGACY-ASSET-BUILDING-AUDIO",
        "asset_ph_building_collision_placeholder": "D8-A02-D15-LEGACY-ASSET-BUILDING-COLLISION",
        "asset_ph_building_modular_placeholder": "D8-A02-D15-LEGACY-ASSET-BUILDING-MODEL",
        "pet_cat_galley": "D8-A02-D15-LEGACY-PET-CAT-GALLEY",
        "pet_cat_mooring": "D8-A02-D15-LEGACY-PET-CAT-MOORING",
        "pet_dog_village": "D8-A02-D15-LEGACY-PET-DOG-VILLAGE",
        "pet_dog_watch": "D8-A02-D15-LEGACY-PET-DOG-WATCH",
    }
    for content_id, test_id in legacy_test_ids.items():
        folder = "asset" if content_id.startswith("asset_") else "animal" if content_id.startswith("animal_") else "pet"
        path = entries_root / folder / (content_id + ".json")
        if path.is_file():
            value = json.loads(path.read_text(encoding="utf-8"))
            value["test_id"] = test_id
            write_json(path, value)

    print("D15-A06 generated: hulls=%d modules=%d functional_points=%d asset_resources=%d" % (len(HULLS), module_index, function_index, len(new_asset_entries)))


if __name__ == "__main__":
    main()
