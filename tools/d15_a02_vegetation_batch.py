"""Generate and validate the D15-A02 vegetation content batch.

The batch is deliberately data-only.  It writes immutable content definitions,
asset references and audit metadata; ecology quantities, harvesting rules and
runtime state remain owned by EcologySystem/D11 and D13.
"""

from __future__ import annotations

import argparse
import hashlib
import json
from collections import Counter
from pathlib import Path
from typing import Any, Dict, Iterable, List, Mapping, MutableMapping, Sequence, Tuple


REPOSITORY_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_REGISTRY_ROOT = REPOSITORY_ROOT / "game" / "Assets" / "Resources" / "ContentRegistry"
PLANT_DIR_NAME = "plant"
ASSET_DIR_NAME = "asset"
BATCH_ID = "D15-A02*"
TOTAL_BATCH_COUNT = 1200


VARIANTS: Sequence[Tuple[str, str]] = (
    ("seedling", "幼苗"),
    ("growing", "生长"),
    ("mature", "成熟"),
    ("harvested", "采集后"),
    ("withered", "枯萎"),
    ("storm_damaged", "风暴损伤"),
    ("recovering", "恢复"),
    ("dry_season", "旱季"),
    ("wet_season", "雨季"),
    ("salt_wind", "盐风"),
    ("shade_form", "阴生形态"),
    ("open_field", "开阔地形态"),
)


MATERIALS = {
    "leaf": "game/Assets/Art/Placeholder/Materials/ph_mat_palm_leaf.mat",
    "grass": "game/Assets/Art/Placeholder/Materials/ph_mat_grass.mat",
    "flower": "game/Assets/Art/Placeholder/Materials/ph_mat_flower.mat",
    "trunk": "game/Assets/Art/Placeholder/Materials/ph_mat_palm_trunk.mat",
    "wood": "game/Assets/Art/Placeholder/Materials/ph_mat_dark_wood.mat",
    "sand": "game/Assets/Art/Placeholder/Materials/ph_mat_sand.mat",
    "rock": "game/Assets/Art/Placeholder/Materials/ph_mat_rock.mat",
    "sea": "game/Assets/Art/Placeholder/Materials/ph_mat_sea.mat",
}


PROFILE_DATA: Mapping[str, Mapping[str, Any]] = {
    "tree": {
        "display": "乔木",
        "habitats": ("rainforest", "lowland_rainforest", "limestone_karst", "volcanic_slope"),
        "soil": ("deep_loam", "red_laterite", "shallow_limestone"),
        "bases": ("mahogany", "cedar", "guavawood", "ironwood", "kapok", "wild_fig", "gumbo_limbo", "copperwood"),
        "use": "timber",
        "harvest_parts": ("trunk", "bark", "resin"),
        "outputs": ("timber",),
        "material": "trunk",
        "wind": ("rainforest_canopy", "ridge_gust"),
        "cultivation": "wild_harvest",
        "introduced": "native_or_inferred",
        "fire": "high",
        "animal": "canopy_roost",
    },
    "shrub": {
        "display": "灌木",
        "habitats": ("dry_shrub", "limestone_karst", "lowland_rainforest", "abandoned_field"),
        "soil": ("dry_sandy", "thin_limestone", "disturbed_loam"),
        "bases": ("saltbrush", "thornbush", "coffee_shrub", "wild_hibiscus", "buttonwood", "candlewood", "bitterleaf", "redbush"),
        "use": "fiber_and_fuel",
        "harvest_parts": ("stem", "leaf", "flower"),
        "outputs": ("fiber", "fuelwood"),
        "material": "grass",
        "wind": ("coast_strong", "dry_scrub_gust"),
        "cultivation": "wild_harvest",
        "introduced": "native_or_escaped",
        "fire": "high",
        "animal": "small_animal_cover",
    },
    "herb": {
        "display": "草本",
        "habitats": ("lowland_rainforest", "abandoned_field", "river_wetland", "coast"),
        "soil": ("moist_loam", "disturbed_loam", "sandy_loam"),
        "bases": ("wild_grass", "arrowleaf", "blue_mist", "marsh_ginger", "shore_daisy", "burdock", "plantain", "milkweed"),
        "use": "food_or_observation",
        "harvest_parts": ("leaf", "flower", "seed"),
        "outputs": ("herb",),
        "material": "grass",
        "wind": ("field_open", "wetland_low"),
        "cultivation": "wild_harvest",
        "introduced": "native_or_inferred",
        "fire": "medium",
        "animal": "pollinator_food",
    },
    "fern": {
        "display": "蕨类",
        "habitats": ("rainforest", "cloud_forest", "river_wetland", "cave_mouth"),
        "soil": ("wet_loam", "mossy_rock", "leaf_mould"),
        "bases": ("tree_fern", "bracken", "maidenhair", "birdnest_fern", "royal_fern", "sword_fern", "water_fern", "ribbon_fern"),
        "use": "habitat_cover",
        "harvest_parts": ("frond", "root"),
        "outputs": ("fiber",),
        "material": "grass",
        "wind": ("rainforest_low", "cave_recirculation"),
        "cultivation": "wild_observation",
        "introduced": "native_or_inferred",
        "fire": "low",
        "animal": "invertebrate_cover",
    },
    "moss": {
        "display": "苔藓",
        "habitats": ("cloud_forest", "cave_mouth", "river_wetland", "limestone_karst"),
        "soil": ("mossy_rock", "wet_limestone", "peaty_soil"),
        "bases": ("stone_moss", "velvet_moss", "rain_moss", "cave_moss", "carpet_moss", "beard_moss", "spring_moss", "lichen_moss"),
        "use": "indicator_and_cover",
        "harvest_parts": ("mat", "spore"),
        "outputs": ("dye",),
        "material": "rock",
        "wind": ("cave_still", "cloud_forest_still"),
        "cultivation": "wild_observation",
        "introduced": "native_or_inferred",
        "fire": "low",
        "animal": "microhabitat",
    },
    "epiphyte": {
        "display": "附生",
        "habitats": ("rainforest", "cloud_forest", "mangrove", "lowland_rainforest"),
        "soil": ("canopy_bark", "mossy_rock", "salt_bark"),
        "bases": ("air_plant", "wild_orchid", "bromeliad", "hanging_vine", "staghorn", "climbing_fern", "canopy_moss", "rain_liana"),
        "use": "medicine_or_habitat",
        "harvest_parts": ("leaf", "flower", "aerial_root"),
        "outputs": ("herb", "fiber"),
        "material": "leaf",
        "wind": ("canopy_wind", "rainforest_low"),
        "cultivation": "wild_observation",
        "introduced": "native_or_inferred",
        "fire": "medium",
        "animal": "bird_nectar",
    },
    "coastal": {
        "display": "海滨",
        "habitats": ("coast", "dune", "mangrove", "shallow_water"),
        "soil": ("salt_sand", "shell_sand", "brackish_mud"),
        "bases": ("sea_grape", "beach_morningglory", "dune_palm", "saltgrass", "coastal_agave", "seaside_purslane", "sand_cordgrass", "shore_pandanus"),
        "use": "fiber_or_coastal_cover",
        "harvest_parts": ("leaf", "fruit", "stem"),
        "outputs": ("fiber", "fruit"),
        "material": "sand",
        "wind": ("coast_strong", "mangrove_low"),
        "cultivation": "wild_harvest",
        "introduced": "native_or_inferred",
        "fire": "medium",
        "animal": "shorebird_cover",
    },
    "wetland": {
        "display": "湿地",
        "habitats": ("river_wetland", "mangrove", "shallow_water", "salt_marsh"),
        "soil": ("peaty_soil", "brackish_mud", "floodplain_silt"),
        "bases": ("river_reed", "cattail", "water_lily", "mangrove_seedling", "marsh_grass", "floating_fern", "pondweed", "tidal_sedge"),
        "use": "waterway_habitat",
        "harvest_parts": ("stem", "leaf", "root"),
        "outputs": ("fiber", "fuel"),
        "material": "sea",
        "wind": ("wetland_low", "mangrove_low"),
        "cultivation": "wild_observation",
        "introduced": "native_or_inferred",
        "fire": "low",
        "animal": "fish_nursery",
    },
    "crop": {
        "display": "农作物",
        "habitats": ("farm", "plantation", "abandoned_field", "river_wetland"),
        "soil": ("managed_loam", "sandy_loam", "floodplain_silt"),
        "bases": ("cassava", "sugarcane", "tobacco", "cotton", "cocoa", "coffee", "indigo", "yam"),
        "use": "cultivated_food_or_trade",
        "harvest_parts": ("root", "seed", "leaf", "stalk"),
        "outputs": ("food", "fiber", "dye"),
        "material": "grass",
        "wind": ("field_open", "plantation_open"),
        "cultivation": "cultivated",
        "introduced": "introduced_or_cultivated",
        "fire": "high",
        "animal": "farm_edge_food",
    },
    "fruit_tree": {
        "display": "果树",
        "habitats": ("orchard", "farm", "coast", "lowland_rainforest"),
        "soil": ("managed_loam", "sandy_loam", "deep_loam"),
        "bases": ("guava", "banana", "coconut", "breadfruit", "mango", "papaya", "lime", "soursop"),
        "use": "fruit_and_food",
        "harvest_parts": ("fruit", "leaf", "seed"),
        "outputs": ("fruit", "seed"),
        "material": "leaf",
        "wind": ("orchard_moderate", "coast_strong"),
        "cultivation": "cultivated_or_escaped",
        "introduced": "introduced_or_escaped_cultivar",
        "fire": "medium",
        "animal": "fruit_bird_food",
    },
    "medicinal": {
        "display": "药材",
        "habitats": ("dry_shrub", "rainforest", "cloud_forest", "river_wetland"),
        "soil": ("dry_sandy", "wet_loam", "mossy_rock"),
        "bases": ("aloe", "bush_mint", "fever_leaf", "bitter_bark", "woundwort", "ginger", "arnica", "datura"),
        "use": "medicinal_or_toxic",
        "harvest_parts": ("leaf", "bark", "root", "flower"),
        "outputs": ("medicine", "herb"),
        "material": "flower",
        "wind": ("dry_scrub_gust", "rainforest_low"),
        "cultivation": "wild_harvest_or_cultivated",
        "introduced": "native_or_inferred",
        "fire": "medium",
        "animal": "pollinator_food",
    },
    "spice": {
        "display": "香料",
        "habitats": ("farm", "plantation", "rainforest", "abandoned_field"),
        "soil": ("managed_loam", "deep_loam", "red_laterite"),
        "bases": ("pepper", "allspice", "cinnamon", "clove", "nutmeg", "annatto", "vanilla", "ginger_spice"),
        "use": "spice_and_trade",
        "harvest_parts": ("seed", "bark", "pod", "fruit"),
        "outputs": ("spice", "dye"),
        "material": "leaf",
        "wind": ("plantation_open", "rainforest_low"),
        "cultivation": "cultivated_or_escaped",
        "introduced": "introduced_or_cultivated",
        "fire": "medium",
        "animal": "bird_and_insect_food",
    },
    "variant": {
        "display": "变体",
        "habitats": ("coast", "dry_shrub", "rainforest", "cloud_forest", "farm", "river_wetland", "mangrove"),
        "soil": ("disturbed_loam", "salt_sand", "red_laterite", "wet_loam"),
        "bases": ("palm", "reed", "vine", "grass", "fern", "moss", "agave", "mangrove"),
        "use": "stage_or_weather_variant",
        "harvest_parts": ("leaf", "stem", "seed"),
        "outputs": ("observation",),
        "material": "leaf",
        "wind": ("coast_strong", "rainforest_low", "field_open", "cave_still"),
        "cultivation": "variant_of_existing_definition",
        "introduced": "inferred_variant",
        "fire": "medium",
        "animal": "habitat_signal",
    },
}


CATEGORY_COUNTS: Mapping[str, int] = {
    "tree": 130,
    "shrub": 100,
    "herb": 110,
    "fern": 80,
    "moss": 60,
    "epiphyte": 70,
    "coastal": 100,
    "wetland": 100,
    "crop": 130,
    "fruit_tree": 100,
    "medicinal": 90,
    "spice": 70,
    "variant": 60,
}


PROFILE_IDS: Mapping[str, str] = {
    category: f"asset_vegetation_profile_{category}" for category in PROFILE_DATA
}


SAMPLE_PROFILE_OVERRIDES = {
    "plant_red_mangrove": "wetland",
    "plant_coast_agave_fiber": "coastal",
    "plant_farm_cassava_mature": "crop",
    "plant_dry_aloe_medicinal": "medicinal",
    "plant_dry_datura_toxin_v1": "medicinal",
    "plant_farm_indigo_mature": "crop",
    "plant_orchard_guava_v1": "fruit_tree",
    "plant_rainforest_mahogany_v1": "tree",
    "plant_rainforest_pine_resin_v1": "tree",
    "plant_shallow_seaweed": "wetland",
    "plant_conch_bed": "coastal",
}


# The checked-in canonical samples are used by the D15 review and must keep a
# habitat profile that agrees with the sample's valid_habitats.  Keep this
# correction in the generator so a future batch regeneration cannot restore
# the old wetland profile for the mangrove and nearshore seaweed samples.
CANONICAL_SAMPLE_HABITAT_OVERRIDES: Mapping[str, Tuple[str, str]] = {
    "plant_red_mangrove": ("mangrove", "mangrove_low"),
    "plant_shallow_seaweed": ("nearshore", "nearshore"),
}


def _stable_suffix(value: str) -> str:
    return hashlib.md5(value.encode("utf-8")).hexdigest()[:8]


def _meta_text(content_id: str) -> str:
    guid = hashlib.md5(f"whalebone:{content_id}".encode("utf-8")).hexdigest()
    return (
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "TextScriptImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )


def _source_metadata() -> Dict[str, Any]:
    return {
        "kind": "project-authored",
        "author": "Whalebone team",
        "reference": "docs/game/design/vegetation/02_VEGETATION_CATALOG.md",
    }


def _license_metadata() -> Dict[str, Any]:
    return {
        "status": "internal-authored",
        "holder": "Whalebone project",
        "reference": "docs/game/design/common/D8_CONTENT_ID_REGISTRY.md#sample-licensing",
        "commercial_use": True,
        "attribution_required": False,
        "modification_allowed": True,
    }


def _authorization_metadata() -> Dict[str, Any]:
    return {
        "authorization_id": "auth_internal_d15_vegetation_catalog",
        "rights_holder": "Whalebone project",
        "license_ref": "docs/game/design/common/D8_CONTENT_ID_REGISTRY.md#sample-licensing",
        "source_url_or_path": "docs/game/design/vegetation/02_VEGETATION_CATALOG.md",
        "acquired_at": "2026-08-11",
        "commercial_use": True,
        "attribution_required": False,
        "modification_allowed": True,
        "expiry_or_scope": "perpetual; project authored data and placeholder references",
        "evidence_ref": "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md#4-来源授权和历史审查",
    }


def _asset_profile(category: str) -> Dict[str, Any]:
    profile = PROFILE_DATA[category]
    asset_id = PROFILE_IDS[category]
    material = MATERIALS[str(profile["material"])]
    return {
        "id": asset_id,
        "schema_version": 1,
        "name": f"植被占位资产配置·{profile['display']}",
        "display_name_key": f"content.asset.{asset_id}.name",
        "category": "asset",
        "tags": ["vegetation", "placeholder", "d15_a02", category],
        "description": "可替换的植被模型/材质配置；当前使用 Unity 原生几何体与仓库内占位材质，不宣称最终美术完成。",
        "gameplay_role": "vegetation_visual_profile",
        "valid_regions": [],
        "valid_habitats": list(profile["habitats"]),
        "valid_weather": ["clear", "rain", "storm_watch"],
        "dependencies": [],
        "references": [],
        "asset_refs": [],
        "source_refs": [
            "game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md",
            "game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt",
        ],
        "source_note": "仅复用仓库已有占位资源；不新增外部下载，不把占位配置标记为最终表现。",
        "source": {
            "kind": "project-authored",
            "author": "Whalebone team",
            "reference": "game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md",
        },
        "license": _license_metadata(),
        "historical_tag": "inferred",
        "phase": "D15",
        "status": "production",
        "owner_system": "AssetRegistry",
        "test_id": f"D8-A02-D15-ASSET-{category.upper().replace('_', '-')}",
        "reviewer": "D15-A02*",
        # Asset profiles are inferred project-authored definitions. Keep them
        # explicitly Pending so regenerating the batch cannot erase the D16-A04
        # review structure or accidentally imply historical approval.
        "historical_review": "Pending",
        "review_evidence_refs": [
            "game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md",
            "game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt",
        ],
        "content_warning_ids": [],
        "skip_allowed": False,
        "authorization": _authorization_metadata(),
        "asset_state": "placeholder",
        "model_ref": f"unity-primitive://vegetation/{category}/lod2",
        "model_status": "placeholder_primitive_ready",
        "material_ref": material,
        "material_status": "source_ready",
        "lod_profile": {
            "name": "plant_lod2",
            "minimum_lod": "LOD2",
            "lod0_triangles": 6000,
            "lod1_triangles": 2000,
            "lod2_triangles": 600,
            "far_representation": "cluster_proxy",
        },
        "wind_profile": {
            "id": str(profile["wind"][0]),
            "zone": str(profile["habitats"][0]),
            "frequency_band": "low_to_medium",
            "amplitude_band": "profiled_by_habitat",
            "gust_response": "deterministic_environment_input",
        },
        "test_cases": [
            f"D15-A02-ASSET-LOAD-{category.upper()}",
            f"D15-A02-ASSET-MATERIAL-{category.upper()}",
        ],
    }


def _d15_fields(
    content_id: str,
    category: str,
    ordinal: int,
    stage: str,
    test_id: str,
) -> Dict[str, Any]:
    profile = PROFILE_DATA[category]
    profile_id = PROFILE_IDS[category]
    habitat = str(profile["habitats"][ordinal % len(profile["habitats"])])
    wind_id = str(profile["wind"][ordinal % len(profile["wind"])])
    material = MATERIALS[str(profile["material"])]
    toxic = category == "medicinal" and ordinal % 11 == 0
    warning_ids = ["content_warning_plant_toxicity"] if toxic else []
    sample = {
        "tree": "plant_red_mangrove",
        "shrub": "plant_coast_agave_fiber",
        "herb": "plant_dry_aloe_medicinal",
        "fern": "plant_red_mangrove",
        "moss": "plant_red_mangrove",
        "epiphyte": "plant_red_mangrove",
        "coastal": "plant_coast_agave_fiber",
        "wetland": "plant_red_mangrove",
        "crop": "plant_farm_cassava_mature",
        "fruit_tree": "plant_orchard_guava_v1",
        "medicinal": "plant_dry_aloe_medicinal",
        "spice": "plant_farm_indigo_mature",
        "variant": "plant_red_mangrove",
    }[category]
    return {
        "production_batch": BATCH_ID,
        "representative_sample": sample,
        "production_stage": "cataloged",
        "behavior_profile": f"EcologySystem:plant_resource_profile:{category}",
        "asset_state": "placeholder",
        "quality_level": "Q2",
        "model_ref": f"unity-primitive://vegetation/{category}/lod2",
        "model_status": "placeholder_primitive_ready",
        "material_ref": material,
        "material_status": "source_ready",
        "collision_ref": "placeholder://vegetation/harvest-proximity-collider",
        "collision_status": "shared_interaction_proxy",
        "lod_profile": {
            "name": "plant_lod2",
            "minimum_lod": "LOD2",
            "lod0_triangles": 6000,
            "lod1_triangles": 2000,
            "lod2_triangles": 600,
            "far_representation": "cluster_proxy",
        },
        "wind_profile": {
            "id": wind_id,
            "zone": habitat,
            "frequency_band": "low_to_medium",
            "amplitude_band": "profiled_by_habitat",
            "gust_response": "deterministic_environment_input",
        },
        "habitat_profile": {
            "primary": habitat,
            "soil": str(profile["soil"][ordinal % len(profile["soil"])]),
            "salt_tolerance": "high" if habitat in {"coast", "mangrove", "salt_marsh", "shallow_water"} else "medium",
            "drought_tolerance": "high" if habitat in {"dry_shrub", "dune", "limestone_karst"} else "medium",
            "water_need": "high" if habitat in {"rainforest", "cloud_forest", "river_wetland", "shallow_water"} else "medium",
            "altitude_range_m": [0, 420] if habitat not in {"cloud_forest", "volcanic_slope"} else [180, 900],
        },
        "use_profile": {
            "primary": str(profile["use"]),
            "harvest_parts": list(profile["harvest_parts"]),
            "resource_outputs": list(profile["outputs"]),
            "toxicity": "toxic_or_requires_identification" if toxic else "non_toxic_or_contextual",
            "fire_fuel": str(profile["fire"]),
            "animal_relationship": str(profile["animal"]),
            "human_cultivation": str(profile["cultivation"]),
        },
        "growth_profile": {
            "stage": stage,
            "season_tags": ["dry", "wet"],
            "cycle_days": 18 + (ordinal % 43),
            "mature_window_days": 4 + (ordinal % 8),
            "recovery_profile": "EcologySystem:habitat_recovery_profile",
        },
        "historical_review": "ApprovedWithWarning" if toxic else "Approved",
        "review_evidence_refs": [
            "docs/game/design/vegetation/01_VEGETATION_OVERVIEW.md",
            "docs/game/design/vegetation/02_VEGETATION_CATALOG.md",
            "docs/game/design/common/D11_VEGETATION_HARVEST.md",
        ],
        "content_warning_ids": warning_ids,
        "context_note_key": "content.plant.context.toxicity" if toxic else "content.plant.context.standard",
        "skip_allowed": bool(toxic),
        "presentation_levels": ["Full", "Contextual", "LowIntensity", "Omitted"] if toxic else ["Full"],
        "test_cases": [
            f"D15-A02-LOAD-{ordinal:04d}",
            f"D15-A02-ASSET-{ordinal:04d}",
            f"D15-A02-HARVEST-OWNER-{ordinal:04d}",
            f"D15-A02-SAVE-RESTORE-{ordinal:04d}",
        ],
        "authorization": _authorization_metadata(),
        "asset_profile_id": profile_id,
        "generation_source": "D15-A02 deterministic batch generator",
        "generation_digest": _stable_suffix(f"{content_id}:{category}:{ordinal}:{stage}"),
    }


def _plant_entry(index: int, category: str, ordinal: int) -> Dict[str, Any]:
    profile = PROFILE_DATA[category]
    base_slug = str(profile["bases"][ordinal % len(profile["bases"])])
    base_display = base_slug.replace("_", " ")
    variant_slug, variant_display = VARIANTS[ordinal % len(VARIANTS)]
    habitat = str(profile["habitats"][ordinal % len(profile["habitats"])])
    stage = variant_slug
    content_id = f"plant_{habitat}_{base_slug}_{variant_slug}_v{index:04d}"
    primary_use = str(profile["use"])
    asset_id = PROFILE_IDS[category]
    test_id = f"D8-A02-D15-VEG-{index:04d}"
    toxic = category == "medicinal" and ordinal % 11 == 0
    tags = ["d15_a02", category, habitat, primary_use, stage]
    if toxic:
        tags.append("toxic_requires_warning")
    entry: Dict[str, Any] = {
        "id": content_id,
        "schema_version": 1,
        "name": f"{base_display}·{profile['display']}·{variant_display}·{index:04d}",
        "display_name_key": f"content.plant.{content_id}.name",
        "category": "plant",
        "tags": tags,
        "description": f"{profile['display']}内容条目：{base_display}的{variant_display}形态，适用于{habitat}生境；资源量、成熟度与采集结果由 EcologySystem/D11 owner 裁决。",
        "gameplay_role": primary_use,
        "valid_regions": ["island_saint_marys", "island_driftwood_isle"],
        "valid_habitats": list(profile["habitats"]),
        "valid_weather": ["clear", "rain", "storm_watch"],
        "dependencies": [],
        "references": ["island_saint_marys"],
        "asset_refs": [asset_id],
        "source_refs": [
            "docs/game/design/vegetation/01_VEGETATION_OVERVIEW.md",
            "docs/game/design/vegetation/02_VEGETATION_CATALOG.md",
            "docs/game/design/vegetation/03_VEGETATION_HABITATS.md",
            "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md",
        ],
        "source_note": "D15-A02 项目自有批量目录；形态、阶段和用途为可审计设计推断，不宣称新增外部史实或外部素材。",
        "source": _source_metadata(),
        "license": _license_metadata(),
        "historical_tag": "inferred",
        "phase": "D15",
        "status": "production",
        "owner_system": "EcologySystem",
        "test_id": test_id,
        "reviewer": "D15-A02*",
        "plant_class": category,
        "scientific_or_historical_note": "加勒比 1675 语境下的生态/形态推断；非外部物种鉴定结论。",
        "soil_tags": list(profile["soil"]),
        "salt_tolerance": "high" if habitat in {"coast", "mangrove", "salt_marsh", "shallow_water"} else "medium",
        "drought_tolerance": "high" if habitat in {"dry_shrub", "dune", "limestone_karst"} else "medium",
        "water_need": "high" if habitat in {"rainforest", "cloud_forest", "river_wetland", "shallow_water"} else "medium",
        "altitude_range": [0, 420] if habitat not in {"cloud_forest", "volcanic_slope"} else [180, 900],
        "season_tags": ["dry", "wet"],
        "growth_stage": stage,
        "harvest_parts": list(profile["harvest_parts"]),
        "resource_outputs": list(profile["outputs"]),
        "toxicity": "toxic_or_requires_identification" if toxic else "non_toxic_or_contextual",
        "fire_fuel": str(profile["fire"]),
        "animal_relationships": [str(profile["animal"])],
        "human_cultivation": str(profile["cultivation"]),
        "introduced_status": str(profile["introduced"]),
    }
    entry.update(_d15_fields(content_id, category, ordinal, stage, test_id))
    return entry


def _sample_d15_update(entry: MutableMapping[str, Any], category: str, ordinal: int) -> None:
    content_id = str(entry["id"])
    current_test_id = str(entry.get("test_id") or f"D8-A02-D15-SAMPLE-{_stable_suffix(content_id).upper()}")
    entry.update(_d15_fields(content_id, category, ordinal, str(entry.get("growth_stage") or "mature"), current_test_id))
    canonical_habitat = CANONICAL_SAMPLE_HABITAT_OVERRIDES.get(content_id)
    if canonical_habitat:
        primary_habitat, wind_zone = canonical_habitat
        habitat_profile = dict(entry.get("habitat_profile") or {})
        habitat_profile["primary"] = primary_habitat
        entry["habitat_profile"] = habitat_profile
        wind_profile = dict(entry.get("wind_profile") or {})
        wind_profile["zone"] = wind_zone
        entry["wind_profile"] = wind_profile
    entry["test_cases"] = [
        f"D15-A02-SAMPLE-LOAD-{_stable_suffix(content_id)}",
        f"D15-A02-SAMPLE-OWNER-{_stable_suffix(content_id)}",
        f"D15-A02-SAMPLE-SAVE-RESTORE-{_stable_suffix(content_id)}",
    ]
    entry["asset_refs"] = list(dict.fromkeys([*entry.get("asset_refs", []), PROFILE_IDS[category]]))
    entry["plant_class"] = category
    entry["quality_level"] = "Q2"
    entry["status"] = "production"


def _write_json(path: Path, value: Mapping[str, Any]) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def _all_json_entries(directory: Path) -> Iterable[Tuple[Path, Dict[str, Any]]]:
    for path in sorted(directory.rglob("*.json")):
        yield path, json.loads(path.read_text(encoding="utf-8"))


def _ensure_registry_roots(registry_root: Path, ids: Iterable[str]) -> None:
    manifest_path = registry_root / "registry.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    roots = list(manifest.get("roots", []))
    known = set(roots)
    for content_id in sorted(set(ids)):
        if content_id not in known:
            roots.append(content_id)
            known.add(content_id)
    manifest["roots"] = roots
    _write_json(manifest_path, manifest)


def write_batch(registry_root: Path = DEFAULT_REGISTRY_ROOT, count: int = TOTAL_BATCH_COUNT) -> Dict[str, Any]:
    if count != TOTAL_BATCH_COUNT:
        raise ValueError(f"D15-A02 is frozen at {TOTAL_BATCH_COUNT} generated entries, got {count}")
    plant_dir = registry_root / "entries" / PLANT_DIR_NAME
    asset_dir = registry_root / "entries" / ASSET_DIR_NAME
    plant_dir.mkdir(parents=True, exist_ok=True)
    asset_dir.mkdir(parents=True, exist_ok=True)

    generated_ids: List[str] = []
    next_index = 1
    category_counts: Counter[str] = Counter()
    for category, category_count in CATEGORY_COUNTS.items():
        for ordinal in range(category_count):
            entry = _plant_entry(next_index, category, ordinal)
            entry_path = plant_dir / f"{entry['id']}.json"
            _write_json(entry_path, entry)
            meta_path = entry_path.with_suffix(entry_path.suffix + ".meta")
            if not meta_path.exists():
                meta_path.write_text(_meta_text(str(entry["id"])), encoding="utf-8")
            generated_ids.append(str(entry["id"]))
            category_counts[category] += 1
            next_index += 1

    existing_plant_ids: List[str] = []
    for path, entry in _all_json_entries(plant_dir):
        content_id = str(entry.get("id", path.stem))
        if content_id in generated_ids:
            continue
        category = SAMPLE_PROFILE_OVERRIDES.get(content_id, "variant")
        _sample_d15_update(entry, category, 0)
        _write_json(path, entry)
        existing_plant_ids.append(content_id)

    asset_ids: List[str] = []
    for category in PROFILE_DATA:
        asset_entry = _asset_profile(category)
        asset_path = asset_dir / f"{asset_entry['id']}.json"
        _write_json(asset_path, asset_entry)
        meta_path = asset_path.with_suffix(asset_path.suffix + ".meta")
        if not meta_path.exists():
            meta_path.write_text(_meta_text(str(asset_entry["id"])), encoding="utf-8")
        asset_ids.append(str(asset_entry["id"]))

    all_island_ids = [entry_id for path, entry in _all_json_entries(registry_root / "entries" / "island") for entry_id in [str(entry.get("id", path.stem))]]
    all_plant_ids = [entry_id for path, entry in _all_json_entries(plant_dir) for entry_id in [str(entry.get("id", path.stem))]]
    _ensure_registry_roots(registry_root, [*all_island_ids, *all_plant_ids, *asset_ids])
    return {
        "generated_count": len(generated_ids),
        "sample_count_updated": len(existing_plant_ids),
        "plant_registry_count": len(all_plant_ids),
        "asset_profile_count": len(asset_ids),
        "category_counts": dict(category_counts),
        "generated_ids": generated_ids,
    }


REQUIRED_D15_FIELDS = (
    "production_batch",
    "production_stage",
    "behavior_profile",
    "asset_state",
    "model_ref",
    "model_status",
    "material_ref",
    "material_status",
    "collision_ref",
    "collision_status",
    "lod_profile",
    "wind_profile",
    "habitat_profile",
    "use_profile",
    "growth_profile",
    "historical_review",
    "review_evidence_refs",
    "content_warning_ids",
    "context_note_key",
    "skip_allowed",
    "presentation_levels",
    "test_cases",
    "authorization",
    "plant_class",
)


def validate_batch(registry_root: Path = DEFAULT_REGISTRY_ROOT) -> Dict[str, Any]:
    plant_dir = registry_root / "entries" / PLANT_DIR_NAME
    plant_entries = [entry for _, entry in _all_json_entries(plant_dir)]
    errors: List[str] = []
    ids = [str(entry.get("id", "")) for entry in plant_entries]
    if not 1000 <= len(plant_entries) <= 2000:
        errors.append(f"plant count outside D15 range: {len(plant_entries)}")
    if len(ids) != len(set(ids)):
        errors.append("duplicate plant IDs")
    test_ids = [str(entry.get("test_id", "")) for entry in plant_entries]
    if len(test_ids) != len(set(test_ids)):
        errors.append("duplicate plant test IDs")
    for entry in plant_entries:
        content_id = str(entry.get("id", "<missing>"))
        if not content_id.startswith("plant_"):
            errors.append(f"{content_id}: ID must use plant_ prefix")
        for field in REQUIRED_D15_FIELDS:
            if field not in entry:
                errors.append(f"{content_id}: missing {field}")
        if entry.get("production_batch") != BATCH_ID:
            errors.append(f"{content_id}: wrong production batch")
        if not entry.get("valid_habitats"):
            errors.append(f"{content_id}: missing habitat")
        if not entry.get("use_profile"):
            errors.append(f"{content_id}: missing use profile")
        if not str(entry.get("model_ref", "")).strip() or not str(entry.get("model_status", "")).strip():
            errors.append(f"{content_id}: model status is empty")
        if not str(entry.get("material_ref", "")).strip() or not str(entry.get("material_status", "")).strip():
            errors.append(f"{content_id}: material status is empty")
        if not str(entry.get("lod_profile", {}).get("minimum_lod", "")).strip():
            errors.append(f"{content_id}: LOD profile is empty")
        if not entry.get("source_refs") or not entry.get("source_note"):
            errors.append(f"{content_id}: source evidence is incomplete")
        authorization = entry.get("authorization")
        if not isinstance(authorization, dict) or authorization.get("commercial_use") is not True:
            errors.append(f"{content_id}: authorization is incomplete")
        if not entry.get("test_cases"):
            errors.append(f"{content_id}: test cases are empty")
    if errors:
        raise ValueError("D15-A02 batch validation failed:\n" + "\n".join(errors[:40]))
    return {
        "plant_count": len(plant_entries),
        "category_counts": dict(Counter(str(entry["plant_class"]) for entry in plant_entries)),
        "sample_ids": ["plant_red_mangrove", "plant_coast_agave_fiber", "plant_farm_cassava_mature"],
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate and validate the D15-A02 vegetation batch")
    parser.add_argument("registry_root", nargs="?", type=Path, default=DEFAULT_REGISTRY_ROOT)
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()
    if args.dry_run:
        print(json.dumps({"frozen_count": TOTAL_BATCH_COUNT, "category_counts": CATEGORY_COUNTS}, ensure_ascii=False, indent=2))
        return 0
    result = write_batch(args.registry_root)
    result["validation"] = validate_batch(args.registry_root)
    print(json.dumps({key: value for key, value in result.items() if key != "generated_ids"}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
