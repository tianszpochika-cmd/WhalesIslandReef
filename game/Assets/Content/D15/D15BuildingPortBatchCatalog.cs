// D15-A04* 建筑/港口批量内容目录。
// Unity import marker: keep this canonical catalog included in the Content runtime assembly.
// 只提供不可变定义、变体、实例绑定和占位资产引用；
// 位置、产权、库存、损伤、开放规则及其他运行时状态仍由对应 canonical owner 写入。

using System;
using System.Collections.Generic;

namespace Whalebone.Content.D15
{
    public static class D15BuildingPortBatchCatalog
    {
        public const int SchemaVersion = 1;
        public const string ProductionBatch = "D15-A04*";
        public const string BuildingOwnerSystem = "BuildingSystem";
        public const string PropertyOwnerSystem = "PropertySystem";
        public const string IslandOwnerSystem = "IslandSystem";
        public const string InventoryOwnerSystem = "Inventory/Cargo";
        public const string AssetOwnerSystem = "AssetRegistry";
        public const int FunctionalBuildingTypeTarget = 80;
        public const int VariantTarget = 180;
        public const int InstanceTarget = 640;
        public const string RepresentativeDockId = "building_saint_marys_dock";
        public const string RepresentativeWellId = "building_green_cay_water_well";

        private static readonly string[] LocationRefs =
        {
            "island_ashfall_bay", "island_bathedral_ridge", "island_blackglass_cape", "island_blackmud_delta",
            "island_blue_hole", "island_bluechain", "island_brightshoal", "island_burrow_cape",
            "island_cactus_bight", "island_cinder_spine", "island_cloudfall", "island_copperkarst",
            "island_cove_caves", "island_deepgreen", "island_driftwood_isle", "island_drydrop",
            "island_drythorn", "island_ember_peak", "island_green_cay", "island_hollowspine",
            "island_horseshoe_atoll", "island_jaguar_heights", "island_lagoon_drift", "island_lava_gate",
            "island_limestone_crown", "island_mangrove_six", "island_moonpool", "island_mossback",
            "island_orchid_slope", "island_parrot_ridge", "island_pearl_sand", "island_raincrown",
            "island_redcrater", "island_reedwater_basin", "island_reefclock", "island_ringwatch",
            "island_riverroot", "island_saint_marys", "island_saltbrush", "island_saltwind_key",
            "island_seaglass_cay", "island_sinkhole_key", "island_sulfur_shelf", "island_sunscorch",
            "island_tidal_reed", "island_turtle_spit", "island_waterfall_isle", "island_whitecap_spit",
            "island_whitecliff", "island_windward_cone"
        };

        private static readonly string[] Cultures =
        {
            "culture_caribbean_local", "culture_spanish_colonial", "culture_english_colonial",
            "culture_french_colonial", "culture_dutch_merchant", "culture_maroon_community",
            "culture_coastal_fisherfolk", "culture_island_council", "culture_free_port"
        };

        private static readonly string[] Factions =
        {
            "faction_local_community", "faction_spanish_crown", "faction_english_crown",
            "faction_french_crown", "faction_dutch_company", "faction_maroons", "faction_free_port"
        };

        private static readonly string[] Materials =
        {
            "material_palm_thatch", "material_hardwood", "material_stone", "material_brick",
            "material_lime_plaster", "material_driftwood", "material_reused_ship_timber",
            "material_coral_masonry"
        };

        private sealed class Seed
        {
            public readonly string Suffix;
            public readonly string Name;
            public readonly string NameEn;
            public readonly string Category;
            public readonly string FunctionProfile;
            public readonly string Habitat;
            public readonly bool Sensitive;

            public Seed(string suffix, string name, string nameEn, string category, string functionProfile, string habitat, bool sensitive)
            {
                Suffix = suffix;
                Name = name;
                NameEn = nameEn;
                Category = category;
                FunctionProfile = functionProfile;
                Habitat = habitat;
                Sensitive = sensitive;
            }
        }

        private static readonly Seed[] Seeds =
        {
        new Seed("saint_marys_dock", "圣玛丽码头", "Saint Mary's Dock", "port", "mooring_and_loading", "harbor", false),
        new Seed("santa_marys_north_warehouse", "圣玛丽北仓库", "Saint Mary's North Warehouse", "trade", "cargo_storage", "warehouse", false),
        new Seed("driftwood_ropewalk_workshop", "漂木岛绳作工坊", "Driftwood Ropewalk Workshop", "shipyard", "rope_and_dye_processing", "workshop", false),
        new Seed("santa_marys_shipyard", "圣玛丽船坞", "Saint Mary's Shipyard", "shipyard", "ship_repair_and_refit", "shipyard", false),
        new Seed("green_cay_water_well", "绿湾公共水井", "Green Cay Public Well", "utility", "freshwater_access", "village", false),
        new Seed("small_boat_pier", "小艇泊位", "Small Boat Pier", "port", "small_craft_landing", "harbor", false),
        new Seed("cargo_wharf", "货运栈桥", "Cargo Wharf", "port", "bulk_loading_and_unloading", "harbor", false),
        new Seed("berth_house", "泊位管理所", "Berth House", "port", "berth_assignment", "harbor", false),
        new Seed("lighthouse", "灯塔", "Lighthouse", "port", "coastal_navigation_signal", "coast", false),
        new Seed("signal_tower", "信号塔", "Signal Tower", "port", "visual_harbor_signal", "coast", false),
        new Seed("harbor_office", "港务所", "Harbor Office", "management", "harbor_clearance", "harbor", false),
        new Seed("customs_house", "海关所", "Customs House", "management", "cargo_inspection_and_duties", "harbor", true),
        new Seed("inspection_station", "检查站", "Inspection Station", "defense", "identity_and_cargo_check", "road", true),
        new Seed("pilot_station", "领航员站", "Pilot Station", "port", "pilotage_and_channel_guidance", "harbor", false),
        new Seed("ferry_house", "渡船所", "Ferry House", "port", "shore_ferry_crossing", "shore", false),
        new Seed("drydock", "干船坞", "Dry Dock", "shipyard", "hull_docking_and_repair", "shipyard", false),
        new Seed("repair_shed", "修船棚", "Repair Shed", "shipyard", "small_vessel_repair", "shipyard", false),
        new Seed("sail_loft", "帆布工坊", "Sail Loft", "shipyard", "sail_cutting_and_repair", "workshop", false),
        new Seed("carpenter_workshop", "木工坊", "Carpenter Workshop", "shipyard", "ship_timber_joinery", "workshop", false),
        new Seed("blacksmith_workshop", "铁匠铺", "Blacksmith Workshop", "production", "metal_tool_forging", "workshop", false),
        new Seed("caulker_workshop", "捻缝工坊", "Caulker Workshop", "shipyard", "hull_caulk_and_seal", "workshop", false),
        new Seed("chandlery", "船具店", "Chandlery", "trade", "ship_supply_service", "harbor", false),
        new Seed("timber_yard", "船材堆场", "Timber Yard", "production", "timber_sorting_and_issue", "yard", false),
        new Seed("sailcloth_storehouse", "帆布库", "Sailcloth Storehouse", "trade", "sailcloth_storage", "warehouse", false),
        new Seed("fish_market", "鱼市", "Fish Market", "trade", "fresh_fish_exchange", "market", false),
        new Seed("general_market", "杂货市场", "General Market", "trade", "mixed_goods_exchange", "market", false),
        new Seed("barter_market", "物物交换集市", "Barter Market", "community", "community_barter_exchange", "market", true),
        new Seed("market_stall", "市场摊位", "Market Stall", "trade", "small_scale_retail", "market", false),
        new Seed("warehouse", "公共仓库", "Public Warehouse", "trade", "general_storage", "warehouse", false),
        new Seed("bonded_warehouse", "保税仓", "Bonded Warehouse", "trade", "legal_hold_storage", "warehouse", true),
        new Seed("smokehouse", "熏制房", "Smokehouse", "production", "smoked_food_processing", "workshop", false),
        new Seed("fish_drying_shed", "鱼干棚", "Fish Drying Shed", "production", "sun_dried_fish_processing", "coast", false),
        new Seed("saltworks", "盐场", "Saltworks", "production", "salt_evaporation_and_storage", "salt_marsh", true),
        new Seed("cooperage", "桶匠铺", "Cooperage", "production", "barrel_making", "workshop", false),
        new Seed("mill", "磨坊", "Mill", "production", "grain_and_root_milling", "farmland", false),
        new Seed("bakery", "面包房", "Bakery", "production", "baked_food_preparation", "market", false),
        new Seed("cookhouse", "公共灶房", "Cookhouse", "life", "communal_meal_preparation", "village", false),
        new Seed("tavern", "酒馆", "Tavern", "life", "food_drink_and_information", "market", false),
        new Seed("inn", "旅店", "Inn", "life", "traveler_lodging", "town", false),
        new Seed("clinic", "诊疗所", "Clinic", "life", "basic_medicine_and_recovery", "village", false),
        new Seed("quarantine_house", "检疫所", "Quarantine House", "life", "arrival_health_isolation", "harbor", true),
        new Seed("apothecary", "药铺", "Apothecary", "trade", "herbal_medicine_service", "market", false),
        new Seed("water_cistern", "蓄水池", "Water Cistern", "utility", "rainwater_storage_access", "village", false),
        new Seed("bathhouse", "公共浴室", "Bathhouse", "life", "washing_and_social_gathering", "village", false),
        new Seed("schoolhouse", "学舍", "Schoolhouse", "social", "literacy_and_navigation_teaching", "village", false),
        new Seed("archive", "档案馆", "Archive", "social", "record_and_deed_access", "town", false),
        new Seed("mapmakers_shop", "制图铺", "Mapmaker's Shop", "trade", "chart_copying_and_sale", "market", false),
        new Seed("post_house", "传递所", "Post House", "management", "message_and_letter_exchange", "road", false),
        new Seed("chapel", "小礼拜堂", "Chapel", "social", "worship_and_ceremony", "village", true),
        new Seed("community_meeting_house", "社群会所", "Community Meeting House", "community", "community_council_and_shelter", "village", true),
        new Seed("shrine", "地方祭坛", "Local Shrine", "social", "devotion_and_offering", "coast", true),
        new Seed("cemetery", "墓园", "Cemetery", "social", "burial_and_remembrance", "village", true),
        new Seed("council_house", "议事屋", "Council House", "management", "local_decision_and_mediation", "village", true),
        new Seed("courthouse", "法庭", "Courthouse", "management", "hearing_and_legal_records", "town", true),
        new Seed("tax_office", "税务所", "Tax Office", "management", "tax_assessment_and_receipts", "town", true),
        new Seed("guardhouse", "卫所", "Guardhouse", "defense", "gate_watch_and_patrol", "road", true),
        new Seed("barracks", "营房", "Barracks", "defense", "garrison_lodging", "town", true),
        new Seed("powder_magazine", "火药库", "Powder Magazine", "defense", "secure_munition_storage", "fort", true),
        new Seed("fort_battery", "炮台", "Fort Battery", "defense", "coastal_defense_position", "coast", true),
        new Seed("watchtower", "瞭望塔", "Watchtower", "defense", "lookout_and_warning", "coast", true),
        new Seed("jail", "拘留所", "Jail", "management", "lawful_detention", "town", true),
        new Seed("bridge_tollhouse", "桥梁关卡", "Bridge Tollhouse", "management", "bridge_access_control", "road", true),
        new Seed("farmstead", "农庄", "Farmstead", "production", "smallholder_food_production", "farmland", false),
        new Seed("estate_farm", "庄园农场", "Estate Farm", "production", "large_estate_cultivation", "farmland", true),
        new Seed("garden_plot", "社区菜园", "Community Garden Plot", "production", "small_plot_food_growing", "farmland", false),
        new Seed("granary", "谷仓", "Granary", "trade", "grain_storage_and_release", "farmland", false),
        new Seed("livestock_pen", "牲畜栏", "Livestock Pen", "production", "livestock_holding_and_care", "farmland", false),
        new Seed("fish_pond", "岛上鱼塘", "Island Fish Pond", "production", "freshwater_fish_rearing", "wetland", false),
        new Seed("nearshore_net_cage", "近岸网箱", "Nearshore Net Cage", "production", "nearshore_fish_rearing", "coast", false),
        new Seed("mangrove_nursery", "红树林育苗场", "Mangrove Nursery", "ecology", "mangrove_restoration", "mangrove", false),
        new Seed("salt_marsh_reserve_station", "盐沼保护站", "Salt Marsh Reserve Station", "ecology", "protected_habitat_monitoring", "salt_marsh", true),
        new Seed("quarry", "采石场", "Quarry", "production", "stone_extraction", "hillside", false),
        new Seed("clay_pit", "黏土坑", "Clay Pit", "production", "clay_extraction", "hillside", false),
        new Seed("brick_kiln", "砖窑", "Brick Kiln", "production", "brick_firing", "workshop", false),
        new Seed("lime_kiln", "石灰窑", "Lime Kiln", "production", "lime_burning", "hillside", false),
        new Seed("pottery_workshop", "陶作坊", "Pottery Workshop", "production", "pottery_shaping_and_firing", "workshop", false),
        new Seed("salvage_shed", "打捞棚", "Salvage Shed", "exploration", "wreck_salvage_sorting", "coast", false),
        new Seed("diving_platform", "潜水平台", "Diving Platform", "exploration", "shallow_dive_access", "coast", false),
        new Seed("cave_waystation", "洞穴歇脚站", "Cave Waystation", "exploration", "cave_shelter_and_supply", "cave", false),
        new Seed("ceremonial_house", "仪式屋", "Ceremonial House", "community", "ceremony_and_community_memory", "village", true),
        };

        public static D15BuildingPortBatchCatalogData Create()
        {
            var definitions = new List<D15BuildingDefinition>();
            foreach (var seed in Seeds) definitions.Add(CreateDefinition(seed));

            var variants = CreateVariants(definitions);
            var instances = CreateInstances(definitions, variants);
            var result = new D15BuildingPortBatchCatalogData(definitions, variants, instances);
            result.Validate();
            return result;
        }

        private static D15BuildingDefinition CreateDefinition(Seed seed)
        {
            var warnings = seed.Sensitive ? new[] { "content_warning_historical_institution" } : new string[0];
            var review = seed.Sensitive ? "ApprovedWithWarning" : "Approved";
            return new D15BuildingDefinition
            {
                Id = "building_" + seed.Suffix,
                SchemaVersion = SchemaVersion,
                Name = seed.Name,
                NameEn = seed.NameEn,
                DisplayNameKey = "content.building.building_" + seed.Suffix + ".name",
                Category = seed.Category,
                FunctionProfile = seed.FunctionProfile,
                FunctionTags = new[] { seed.Category, seed.FunctionProfile, "building_system_owned" },
                ValidRegions = new[] { "caribbean_archipelago" },
                ValidHabitats = new[] { seed.Habitat },
                ValidWeather = new[] { "clear", "rain", "storm_watch" },
                References = new[] { "island_saint_marys" },
                AssetRefs = new[] { "asset_ph_building_modular_placeholder", "asset_ph_building_collision_placeholder", "asset_ph_building_audio_placeholder" },
                SourceRefs = new[] { "docs/game/design/buildings/01_BUILDING_OVERVIEW.md", "docs/game/design/buildings/02_PORT_SHIPYARD.md", "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md" },
                SourceNote = "D15-A04 project-authored functional building definition; runtime state is owned by the canonical systems.",
                HistoricalTag = seed.Sensitive ? "historical" : "inferred",
                Phase = "D15",
                Status = "production",
                OwnerSystem = BuildingOwnerSystem,
                TestId = "D15-A04-BUILDING-" + seed.Suffix,
                Reviewer = "D15-A04",
                ProductionBatch = ProductionBatch,
                RepresentativeSample = seed.Sensitive ? RepresentativeDockId : RepresentativeWellId,
                ProductionStage = "cataloged",
                BehaviorProfile = "building_behavior_profile_" + seed.FunctionProfile,
                AssetState = "placeholder",
                ModelRef = "asset_ph_building_modular_placeholder",
                MaterialRef = "placeholder_building_material_" + seed.Category,
                CollisionRef = "asset_ph_building_collision_placeholder",
                LodProfile = "building_functional_lod3",
                AnchorRefs = new[] { "anchor_building_" + seed.Suffix + "_entry", "anchor_building_" + seed.Suffix + "_service", "anchor_building_" + seed.Suffix + "_state" },
                AudioProfile = "asset_ph_building_audio_placeholder",
                DefaultOpenRuleRef = "open_rule_" + seed.FunctionProfile,
                TestCases = new[] { "D15-A04-LOAD-" + seed.Suffix, "D15-A04-INTERACT-" + seed.Suffix, "D15-A04-SAVE-" + seed.Suffix, "D15-A04-RESTORE-" + seed.Suffix },
                QualityLevel = "Q2",
                HistoricalReview = review,
                ReviewEvidenceRefs = new[] { "docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md" },
                ContentWarningIds = warnings,
                ContextNoteKey = seed.Sensitive ? "content.building.building_" + seed.Suffix + ".context" : string.Empty,
                SkipAllowed = true,
                PresentationLevels = seed.Sensitive ? new[] { "Full", "Contextual", "LowIntensity", "Omitted" } : new[] { "Full", "LowIntensity" }
            };
        }

        private static List<D15BuildingVariantDefinition> CreateVariants(IReadOnlyList<D15BuildingDefinition> definitions)
        {
            var variants = new List<D15BuildingVariantDefinition>();
            foreach (var definition in definitions)
            {
                variants.Add(CreateVariant(definition, "building_variant_" + definition.Id.Substring("building_".Length) + "_base", Cultures[0], Factions[0], Materials[0], "building_functional_lod3"));
            }

            for (var i = 0; i < 100; i++)
            {
                var definition = definitions[i % definitions.Count];
                var culture = Cultures[i % Cultures.Length];
                var faction = Factions[(i * 3) % Factions.Length];
                var material = Materials[(i * 5) % Materials.Length];
                var variantId = "building_variant_" + definition.Id.Substring("building_".Length) + "_" + culture.Substring("culture_".Length) + "_" + material.Substring("material_".Length) + "_" + (i + 1).ToString("D3");
                variants.Add(CreateVariant(definition, variantId, culture, faction, material, "building_variant_lod2"));
            }
            return variants;
        }

        private static D15BuildingVariantDefinition CreateVariant(D15BuildingDefinition definition, string variantId, string culture, string faction, string material, string lod)
        {
            return new D15BuildingVariantDefinition
            {
                VariantId = variantId,
                BuildingId = definition.Id,
                SchemaVersion = SchemaVersion,
                DisplayNameKey = definition.DisplayNameKey + "." + variantId,
                CultureId = culture,
                FactionId = faction,
                MaterialId = material,
                FunctionProfile = definition.FunctionProfile,
                ModelRef = definition.ModelRef,
                MaterialRef = definition.MaterialRef,
                CollisionRef = definition.CollisionRef,
                AnchorRefs = definition.AnchorRefs,
                AudioProfile = definition.AudioProfile,
                AssetState = "placeholder",
                ProductionStage = "cataloged",
                LodProfile = lod,
                OwnerSystem = BuildingOwnerSystem,
                TestId = "D15-A04-VARIANT-" + variantId,
                TestCases = new[] { "D15-A04-VARIANT-LOAD", "D15-A04-VARIANT-INTERACT", "D15-A04-VARIANT-SAVE" },
                HistoricalReview = definition.HistoricalReview,
                ContentWarningIds = definition.ContentWarningIds
            };
        }

        private static List<D15BuildingInstanceDefinition> CreateInstances(IReadOnlyList<D15BuildingDefinition> definitions, IReadOnlyList<D15BuildingVariantDefinition> variants)
        {
            var instances = new List<D15BuildingInstanceDefinition>();
            for (var definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++)
            {
                var definition = definitions[definitionIndex];
                var matchingVariants = new List<D15BuildingVariantDefinition>();
                foreach (var variant in variants) if (variant.BuildingId == definition.Id) matchingVariants.Add(variant);

                for (var slot = 0; slot < 8; slot++)
                {
                    var instanceId = "instance_" + definition.Id.Substring("building_".Length) + "_" + (slot + 1).ToString("D2");
                    var location = LocationRefs[(definitionIndex * 8 + slot) % LocationRefs.Length];
                    var ownerRef = OwnerBindingFor(definition.Category);
                    instances.Add(new D15BuildingInstanceDefinition
                    {
                        InstanceId = instanceId,
                        SchemaVersion = SchemaVersion,
                        BuildingId = definition.Id,
                        VariantId = matchingVariants[slot % matchingVariants.Count].VariantId,
                        CreatedDay = 0,
                        UpdatedDay = 0,
                        LocationRef = location,
                        LocationAnchorRef = "anchor_" + definition.Id + "_instance_" + (slot + 1).ToString("D2"),
                        OwnerRef = ownerRef,
                        OwnerBindingSourceRef = "PropertySystem_or_FactionLegalSystem_runtime_assignment",
                        FunctionProfileId = definition.FunctionProfile,
                        FunctionTags = definition.FunctionTags,
                        InventoryProfileId = "inventory_profile_" + definition.FunctionProfile,
                        InventoryBindingRef = "inventory_binding_" + instanceId,
                        InventoryOwnerSystem = InventoryOwnerSystem,
                        DamageProfileId = "damage_profile_building_standard",
                        DamageStateBindingRef = "damage_binding_" + instanceId,
                        DamageOwnerSystem = BuildingOwnerSystem,
                        OpenRuleId = definition.DefaultOpenRuleRef,
                        OpenRuleBindingRef = "open_rule_binding_" + instanceId,
                        OpenRuleOwnerSystem = BuildingOwnerSystem,
                        InteractionPoints = CreateInteractionPoints(instanceId, definition),
                        AudioProfile = definition.AudioProfile,
                        ModelRef = definition.ModelRef,
                        CollisionRef = definition.CollisionRef,
                        AssetState = "placeholder",
                        LodProfile = definition.LodProfile,
                        SaveStateKey = "location_json.d15_a04_building_instances." + instanceId,
                        OwnerSystem = BuildingOwnerSystem,
                        TestId = "D15-A04-INSTANCE-" + instanceId,
                        TestCases = new[] { "D15-A04-INSTANCE-LOAD", "D15-A04-INSTANCE-INTERACT", "D15-A04-INSTANCE-SAVE", "D15-A04-INSTANCE-RESTORE" },
                        HistoricalReview = definition.HistoricalReview,
                        ContentWarningIds = definition.ContentWarningIds,
                        SkipAllowed = true
                    });
                }
            }
            return instances;
        }

        private static List<D15BuildingInteractionPoint> CreateInteractionPoints(string instanceId, D15BuildingDefinition definition)
        {
            return new List<D15BuildingInteractionPoint>
            {
                new D15BuildingInteractionPoint { InteractionPointId = instanceId + "_entry", AnchorRef = "anchor_" + definition.Id + "_entry", ActionId = "enter", OwnerSystem = BuildingOwnerSystem, TestId = "D15-A04-INTERACTION-ENTER" },
                new D15BuildingInteractionPoint { InteractionPointId = instanceId + "_service", AnchorRef = "anchor_" + definition.Id + "_service", ActionId = "service", OwnerSystem = BuildingOwnerSystem, TestId = "D15-A04-INTERACTION-SERVICE" },
                new D15BuildingInteractionPoint { InteractionPointId = instanceId + "_inspect", AnchorRef = "anchor_" + definition.Id + "_state", ActionId = "inspect", OwnerSystem = BuildingOwnerSystem, TestId = "D15-A04-INTERACTION-INSPECT" }
            };
        }

        private static string OwnerBindingFor(string category)
        {
            if (category == "port" || category == "shipyard") return "owner_harbor_authority";
            if (category == "defense" || category == "management") return "owner_faction_legal_registry";
            if (category == "social" || category == "community" || category == "ecology") return "owner_community_council";
            return "owner_property_registry";
        }
    }

    public sealed class D15BuildingPortBatchCatalogData
    {
        public IReadOnlyList<D15BuildingDefinition> Definitions { get; }
        public IReadOnlyList<D15BuildingVariantDefinition> Variants { get; }
        public IReadOnlyList<D15BuildingInstanceDefinition> Instances { get; }

        public D15BuildingPortBatchCatalogData(IReadOnlyList<D15BuildingDefinition> definitions, IReadOnlyList<D15BuildingVariantDefinition> variants, IReadOnlyList<D15BuildingInstanceDefinition> instances)
        {
            Definitions = definitions;
            Variants = variants;
            Instances = instances;
        }

        public void Validate()
        {
            if (Definitions == null || Definitions.Count != D15BuildingPortBatchCatalog.FunctionalBuildingTypeTarget) throw new InvalidOperationException("D15-A04 requires exactly 80 functional building definitions.");
            if (Variants == null || Variants.Count != D15BuildingPortBatchCatalog.VariantTarget) throw new InvalidOperationException("D15-A04 requires exactly 180 building variants.");
            if (Instances == null || Instances.Count < 600 || Instances.Count > 1200) throw new InvalidOperationException("D15-A04 instance count must be within 600-1200.");

            var buildingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions)
            {
                if (definition == null || !buildingIds.Add(definition.Id)) throw new InvalidOperationException("D15-A04 building ID is missing or duplicated.");
                definition.Validate();
            }

            var variantIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var variant in Variants)
            {
                if (variant == null || !variantIds.Add(variant.VariantId) || !buildingIds.Contains(variant.BuildingId)) throw new InvalidOperationException("D15-A04 variant reference is invalid.");
                variant.Validate();
            }

            var instanceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var instance in Instances)
            {
                if (instance == null || !instanceIds.Add(instance.InstanceId) || !buildingIds.Contains(instance.BuildingId) || !variantIds.Contains(instance.VariantId)) throw new InvalidOperationException("D15-A04 instance ID/reference is invalid.");
                instance.Validate();
            }
        }
    }

    public sealed class D15BuildingDefinition
    {
        public int SchemaVersion;
        public string Id;
        public string Name;
        public string NameEn;
        public string DisplayNameKey;
        public string Category;
        public string FunctionProfile;
        public string[] FunctionTags;
        public string[] ValidRegions;
        public string[] ValidHabitats;
        public string[] ValidWeather;
        public string[] References;
        public string[] AssetRefs;
        public string[] SourceRefs;
        public string SourceNote;
        public string HistoricalTag;
        public string Phase;
        public string Status;
        public string OwnerSystem;
        public string TestId;
        public string Reviewer;
        public string ProductionBatch;
        public string RepresentativeSample;
        public string ProductionStage;
        public string BehaviorProfile;
        public string AssetState;
        public string ModelRef;
        public string MaterialRef;
        public string CollisionRef;
        public string LodProfile;
        public string[] AnchorRefs;
        public string AudioProfile;
        public string DefaultOpenRuleRef;
        public string[] TestCases;
        public string QualityLevel;
        public string HistoricalReview;
        public string[] ReviewEvidenceRefs;
        public string[] ContentWarningIds;
        public string ContextNoteKey;
        public bool SkipAllowed;
        public string[] PresentationLevels;

        public void Validate()
        {
            Require(Id, "building ID");
            if (!Id.StartsWith("building_", StringComparison.Ordinal)) throw new InvalidOperationException("Building ID must use building_ prefix: " + Id);
            if (SchemaVersion != D15BuildingPortBatchCatalog.SchemaVersion || OwnerSystem != D15BuildingPortBatchCatalog.BuildingOwnerSystem) throw new InvalidOperationException("Building schema or owner is invalid: " + Id);
            Require(FunctionProfile, "function profile");
            Require(ProductionBatch, "production batch");
            Require(ProductionStage, "production stage");
            Require(BehaviorProfile, "behavior profile");
            Require(ModelRef, "model ref");
            Require(MaterialRef, "material ref");
            Require(CollisionRef, "collision ref");
            Require(LodProfile, "LOD profile");
            Require(AudioProfile, "audio profile");
            if (AssetRefs == null || AssetRefs.Length == 0 || AnchorRefs == null || AnchorRefs.Length < 3 || TestCases == null || TestCases.Length < 4) throw new InvalidOperationException("Building asset/test fields are incomplete: " + Id);
            if (HistoricalReview == "ApprovedWithWarning" && (ContentWarningIds == null || ContentWarningIds.Length == 0)) throw new InvalidOperationException("Warning building is missing content warning: " + Id);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Building " + field + " is empty.");
        }
    }

    public sealed class D15BuildingVariantDefinition
    {
        public string VariantId;
        public string BuildingId;
        public int SchemaVersion;
        public string DisplayNameKey;
        public string CultureId;
        public string FactionId;
        public string MaterialId;
        public string FunctionProfile;
        public string ModelRef;
        public string MaterialRef;
        public string CollisionRef;
        public string[] AnchorRefs;
        public string AudioProfile;
        public string AssetState;
        public string ProductionStage;
        public string LodProfile;
        public string OwnerSystem;
        public string TestId;
        public string[] TestCases;
        public string HistoricalReview;
        public string[] ContentWarningIds;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(VariantId) || string.IsNullOrWhiteSpace(BuildingId) || SchemaVersion != D15BuildingPortBatchCatalog.SchemaVersion || string.IsNullOrWhiteSpace(CultureId) || string.IsNullOrWhiteSpace(FactionId) || string.IsNullOrWhiteSpace(MaterialId) || string.IsNullOrWhiteSpace(ModelRef) || string.IsNullOrWhiteSpace(CollisionRef) || string.IsNullOrWhiteSpace(AudioProfile) || string.IsNullOrWhiteSpace(LodProfile) || OwnerSystem != D15BuildingPortBatchCatalog.BuildingOwnerSystem || TestCases == null || TestCases.Length < 3) throw new InvalidOperationException("Building variant fields are incomplete: " + VariantId);
        }
    }

    public sealed class D15BuildingInstanceDefinition
    {
        public string InstanceId;
        public int SchemaVersion;
        public string BuildingId;
        public string VariantId;
        public int CreatedDay;
        public int UpdatedDay;
        public string LocationRef;
        public string LocationAnchorRef;
        public string OwnerRef;
        public string OwnerBindingSourceRef;
        public string FunctionProfileId;
        public string[] FunctionTags;
        public string InventoryProfileId;
        public string InventoryBindingRef;
        public string InventoryOwnerSystem;
        public string DamageProfileId;
        public string DamageStateBindingRef;
        public string DamageOwnerSystem;
        public string OpenRuleId;
        public string OpenRuleBindingRef;
        public string OpenRuleOwnerSystem;
        public List<D15BuildingInteractionPoint> InteractionPoints;
        public string AudioProfile;
        public string ModelRef;
        public string CollisionRef;
        public string AssetState;
        public string LodProfile;
        public string SaveStateKey;
        public string OwnerSystem;
        public string TestId;
        public string[] TestCases;
        public string HistoricalReview;
        public string[] ContentWarningIds;
        public bool SkipAllowed;

        public void Validate()
        {
            Require(InstanceId, "instance ID");
            if (!InstanceId.StartsWith("instance_", StringComparison.Ordinal)) throw new InvalidOperationException("Instance ID must use instance_ prefix: " + InstanceId);
            if (SchemaVersion != D15BuildingPortBatchCatalog.SchemaVersion || string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(VariantId) || string.IsNullOrWhiteSpace(LocationRef) || !LocationRef.StartsWith("island_", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(OwnerRef) || string.IsNullOrWhiteSpace(FunctionProfileId) || OwnerSystem != D15BuildingPortBatchCatalog.BuildingOwnerSystem) throw new InvalidOperationException("Instance identity/function fields are incomplete: " + InstanceId);
            if (string.IsNullOrWhiteSpace(InventoryProfileId) || string.IsNullOrWhiteSpace(InventoryBindingRef) || InventoryOwnerSystem != D15BuildingPortBatchCatalog.InventoryOwnerSystem) throw new InvalidOperationException("Instance inventory binding is incomplete: " + InstanceId);
            if (string.IsNullOrWhiteSpace(DamageProfileId) || string.IsNullOrWhiteSpace(DamageStateBindingRef) || DamageOwnerSystem != D15BuildingPortBatchCatalog.BuildingOwnerSystem) throw new InvalidOperationException("Instance damage binding is incomplete: " + InstanceId);
            if (string.IsNullOrWhiteSpace(OpenRuleId) || string.IsNullOrWhiteSpace(OpenRuleBindingRef) || OpenRuleOwnerSystem != D15BuildingPortBatchCatalog.BuildingOwnerSystem) throw new InvalidOperationException("Instance open-rule binding is incomplete: " + InstanceId);
            if (InteractionPoints == null || InteractionPoints.Count < 3 || string.IsNullOrWhiteSpace(AudioProfile) || string.IsNullOrWhiteSpace(ModelRef) || string.IsNullOrWhiteSpace(CollisionRef) || AssetState != "placeholder" || string.IsNullOrWhiteSpace(LodProfile) || string.IsNullOrWhiteSpace(SaveStateKey) || TestCases == null || TestCases.Length < 4) throw new InvalidOperationException("Instance interaction/audio/save/test fields are incomplete: " + InstanceId);
            foreach (var point in InteractionPoints) if (point == null) throw new InvalidOperationException("Instance interaction point is null: " + InstanceId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Instance " + field + " is empty.");
        }
    }

    public sealed class D15BuildingInteractionPoint
    {
        public string InteractionPointId;
        public string AnchorRef;
        public string ActionId;
        public string OwnerSystem;
        public string TestId;
    }
}
