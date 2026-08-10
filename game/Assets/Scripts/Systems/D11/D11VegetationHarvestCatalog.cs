// ============================================================
// D11VegetationHarvestCatalog —— D11-A07 代表植被、工具和保护区目录
//
// 目录是不可变定义；数量、压力和库存不在这里持有。新增条目必须同时
// 在 D8 ContentRegistry 登记，运行时 ID 与目录 ID 保持一致。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;

namespace Whalebone.Systems.D11
{
    public static class D11VegetationIds
    {
        public const string GuavaPlantId = "plant_orchard_guava_v1";
        public const string CassavaPlantId = "plant_farm_cassava_mature";
        public const string AloePlantId = "plant_dry_aloe_medicinal";
        public const string AgavePlantId = "plant_coast_agave_fiber";
        public const string MangrovePlantId = "plant_red_mangrove";
        public const string MahoganyPlantId = "plant_rainforest_mahogany_v1";
        public const string PineResinPlantId = "plant_rainforest_pine_resin_v1";
        public const string IndigoPlantId = "plant_farm_indigo_mature";
        public const string DaturaPlantId = "plant_dry_datura_toxin_v1";

        public const string BasketToolId = "tool_vegetation_basket";
        public const string HarvestingKnifeToolId = "tool_vegetation_harvesting_knife";
        public const string HerbKnifeToolId = "tool_vegetation_herb_knife";
        public const string MacheteToolId = "tool_vegetation_machete";
        public const string AxeToolId = "tool_vegetation_axe";
        public const string ResinScraperToolId = "tool_vegetation_resin_scraper";

        public const string MangroveReserveId = "protected_mangrove_saint_marys";
        public const string CommunityGroveReserveId = "protected_community_grove_saint_marys";
        public const string HarvestEventPrefix = "event_d11_vegetation_harvest_";
    }

    public static class D11VegetationHarvestCatalog
    {
        private static readonly IReadOnlyList<D11VegetationToolDefinition> ToolList =
            new List<D11VegetationToolDefinition>
            {
                Tool(D11VegetationIds.BasketToolId, "item_tool_vegetation_basket", "采集篮"),
                Tool(D11VegetationIds.HarvestingKnifeToolId, "item_tool_vegetation_harvesting_knife", "采集小刀"),
                Tool(D11VegetationIds.HerbKnifeToolId, "item_tool_vegetation_herb_knife", "药草小刀"),
                Tool(D11VegetationIds.MacheteToolId, "item_tool_vegetation_machete", "砍刀"),
                Tool(D11VegetationIds.AxeToolId, "item_tool_vegetation_axe", "短柄斧"),
                Tool(D11VegetationIds.ResinScraperToolId, "item_tool_vegetation_resin_scraper", "树脂刮刀与容器"),
            };

        private static readonly IReadOnlyList<D11VegetationResourceDefinition> ResourceList =
            new List<D11VegetationResourceDefinition>
            {
                Resource(D11VegetationIds.GuavaPlantId, "番石榴", D11VegetationResourceKind.Fruit,
                    "item_fruit_guava", "coast", D11VegetationIds.BasketToolId, 1, 8, 3, 2, 6, 2, 2),
                Resource(D11VegetationIds.CassavaPlantId, "木薯", D11VegetationResourceKind.Vegetable,
                    "item_vegetable_cassava", "farm", D11VegetationIds.HarvestingKnifeToolId, 1, 7, 2, 2, 8, 3, 2),
                Resource(D11VegetationIds.AloePlantId, "芦荟药草", D11VegetationResourceKind.Medicinal,
                    "medicine", "dry_shrub", D11VegetationIds.HerbKnifeToolId, 1, 6, 2, 1, 5, 2, 2),
                Resource(D11VegetationIds.AgavePlantId, "龙舌兰纤维", D11VegetationResourceKind.Fiber,
                    "item_fiber_agave", "coast", D11VegetationIds.MacheteToolId, 1, 8, 3, 2, 7, 2, 3),
                Resource(D11VegetationIds.MangrovePlantId, "红树", D11VegetationResourceKind.Timber,
                    "timber", "mangrove", D11VegetationIds.AxeToolId, 2, 8, 3, 1, 9, 4, 2,
                    protectedSpecies: true),
                Resource(D11VegetationIds.MahoganyPlantId, "桃花心木", D11VegetationResourceKind.Timber,
                    "timber", "rainforest", D11VegetationIds.AxeToolId, 2, 5, 1, 1, 10, 5, 3),
                Resource(D11VegetationIds.PineResinPlantId, "松脂", D11VegetationResourceKind.Resin,
                    "item_resin_pine", "rainforest", D11VegetationIds.ResinScraperToolId, 2, 6, 2, 1, 6, 2, 2),
                Resource(D11VegetationIds.IndigoPlantId, "靛蓝植物", D11VegetationResourceKind.Dye,
                    "dyes", "farm", D11VegetationIds.HarvestingKnifeToolId, 1, 7, 2, 1, 6, 2, 2),
                Resource(D11VegetationIds.DaturaPlantId, "曼陀罗毒草", D11VegetationResourceKind.Toxin,
                    "item_toxin_datura", "dry_shrub", D11VegetationIds.HerbKnifeToolId, 3, 5, 1, 1, 7, 3, 2,
                    toxic: true),
            };

        private static readonly IReadOnlyList<D11VegetationProtectionAreaDefinition> ProtectionList =
            new List<D11VegetationProtectionAreaDefinition>
            {
                new D11VegetationProtectionAreaDefinition
                {
                    Id = D11VegetationIds.MangroveReserveId,
                    HabitatId = "mangrove",
                    RuleId = "mangrove_no_harvest",
                    BlocksAllHarvest = true,
                },
                new D11VegetationProtectionAreaDefinition
                {
                    Id = D11VegetationIds.CommunityGroveReserveId,
                    HabitatId = "rainforest",
                    RuleId = "community_grove_timber_limit",
                    BlockedKinds = new[] { D11VegetationResourceKind.Timber },
                },
            };

        public static IReadOnlyList<D11VegetationResourceDefinition> Resources { get { return ResourceList; } }
        public static IReadOnlyList<D11VegetationToolDefinition> Tools { get { return ToolList; } }
        public static IReadOnlyList<D11VegetationProtectionAreaDefinition> ProtectionAreas
        {
            get { return ProtectionList; }
        }

        public static D11VegetationResourceDefinition FindResource(string plantId)
        {
            if (string.IsNullOrEmpty(plantId)) return null;
            foreach (var resource in ResourceList)
                if (resource != null && resource.Id == plantId) return resource;
            return null;
        }

        public static D11VegetationToolDefinition FindTool(string toolId)
        {
            if (string.IsNullOrEmpty(toolId)) return null;
            foreach (var tool in ToolList)
                if (tool != null && tool.Id == toolId) return tool;
            return null;
        }

        public static D11VegetationProtectionAreaDefinition FindProtectionArea(string areaId)
        {
            if (string.IsNullOrEmpty(areaId)) return null;
            foreach (var area in ProtectionList)
                if (area != null && area.Id == areaId) return area;
            return null;
        }

        /// <summary>
        /// 只登记不可变容量元数据；数量仍必须经 D3InventorySystem 写入。
        /// </summary>
        public static void RegisterInventoryDefinitions()
        {
            foreach (var resource in ResourceList)
            {
                if (resource == null || resource.ItemId == "timber" || resource.ItemId == "medicine" ||
                    resource.ItemId == "dyes") continue;
                D3InventorySystem.RegisterCargoDefinition(resource.ItemId, UnitWeight(resource), UnitVolume(resource));
            }
        }

        private static D11VegetationToolDefinition Tool(string id, string contentId, string displayName)
        {
            return new D11VegetationToolDefinition { Id = id, ContentId = contentId, DisplayName = displayName };
        }

        private static D11VegetationResourceDefinition Resource(string id, string displayName,
            D11VegetationResourceKind kind, string itemId, string habitatId, string toolId,
            int identificationSkill, int capacity, int harvestLimit, int baseYield,
            int cycleDays, int matureStart, int matureDuration, bool toxic = false,
            bool protectedSpecies = false)
        {
            return new D11VegetationResourceDefinition
            {
                Id = id,
                DisplayName = displayName,
                Kind = kind,
                ItemId = itemId,
                HabitatId = habitatId,
                RequiredToolId = toolId,
                MinimumIdentificationSkill = identificationSkill,
                Capacity = capacity,
                HarvestLimitPerAction = harvestLimit,
                BaseYield = baseYield,
                MaturityCycleDays = cycleDays,
                MatureStartDay = matureStart,
                MatureDurationDays = matureDuration,
                Toxic = toxic,
                ProtectedSpecies = protectedSpecies,
            };
        }

        private static int UnitWeight(D11VegetationResourceDefinition resource)
        {
            switch (resource.Kind)
            {
                case D11VegetationResourceKind.Timber: return 2;
                case D11VegetationResourceKind.Resin: return 1;
                default: return 1;
            }
        }

        private static int UnitVolume(D11VegetationResourceDefinition resource)
        {
            switch (resource.Kind)
            {
                case D11VegetationResourceKind.Timber: return 2;
                case D11VegetationResourceKind.Fiber: return 2;
                default: return 1;
            }
        }
    }
}
