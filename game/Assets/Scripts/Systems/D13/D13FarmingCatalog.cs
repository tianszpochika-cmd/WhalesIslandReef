// D13-A03 农业代表目录：八种不同标签的作物、两个地块和加工路径。

using System;
using System.Collections.Generic;

using Whalebone.Systems.D3;

namespace Whalebone.Systems.D13
{
    public static class D13FarmingCatalog
    {
        public const string GuavaCropId = "crop_orchard_guava";
        public const string CassavaCropId = "crop_vegetable_cassava";
        public const string SugarcaneCropId = "crop_sugarcane";
        public const string TobaccoCropId = "crop_tobacco";
        public const string CoffeeCropId = "crop_coffee";
        public const string CocoaCropId = "crop_cocoa";
        public const string SpiceCropId = "crop_spice";
        public const string HerbCropId = "crop_herb";

        public const string OrchardPlotId = "plot_saint_marys_orchard";
        public const string CassavaPlotId = "plot_green_cay_cassava_field";

        public const string PestTreatmentItemId = "item_farm_pest_treatment";

        private static readonly IReadOnlyList<D13FarmingCropDefinition> CropList = new List<D13FarmingCropDefinition>
        {
            Crop(GuavaCropId, "番石榴果园", D13FarmingCropKind.Fruit, "item_seed_guava", "item_fruit_guava", "item_guava_preserve", "orchard", "fruit", 12, 4, 3, 8, 15, 65, new[] { "orchard", "fruit", "perennial" }),
            Crop(CassavaCropId, "木薯蔬菜地", D13FarmingCropKind.Vegetable, "item_seed_cassava", "item_vegetable_cassava", "item_cassava_flour", "loam", "vegetable", 5, 5, 2, 10, 12, 55, new[] { "vegetable", "root_crop", "short_cycle" }),
            Crop(SugarcaneCropId, "甘蔗田", D13FarmingCropKind.Sugarcane, "item_seed_sugarcane", "sugar", "rum", "loam", "sugarcane", 10, 5, 4, 13, 18, 45, new[] { "sugarcane", "plantation", "stalk" }),
            Crop(TobaccoCropId, "烟草地", D13FarmingCropKind.Tobacco, "item_seed_tobacco", "tobacco", "item_tobacco_cured", "loam", "tobacco", 9, 4, 3, 12, 22, 35, new[] { "tobacco", "cash_crop", "leaf" }),
            Crop(CoffeeCropId, "咖啡园", D13FarmingCropKind.Coffee, "item_seed_coffee", "coffee", "item_coffee_roasted", "loam", "coffee", 14, 5, 4, 16, 14, 70, new[] { "coffee", "shade_crop", "bean" }),
            Crop(CocoaCropId, "可可园", D13FarmingCropKind.Cocoa, "item_seed_cocoa", "cocoa", "item_cocoa_cured", "loam", "cocoa", 13, 5, 4, 15, 16, 60, new[] { "cocoa", "shade_crop", "pod" }),
            Crop(SpiceCropId, "香料地", D13FarmingCropKind.Spice, "item_seed_spice", "spices", "item_spices_dried", "loam", "spice", 8, 3, 2, 9, 10, 50, new[] { "spice", "drying", "aromatic" }),
            Crop(HerbCropId, "药草圃", D13FarmingCropKind.Herb, "item_seed_herb", "medicine", "item_herb_prepared", "loam", "herb", 6, 3, 2, 7, 9, 60, new[] { "herb", "medicinal", "small_plot" }),
        };

        private static readonly IReadOnlyList<D13FarmingPlotDefinition> PlotList = new List<D13FarmingPlotDefinition>
        {
            new D13FarmingPlotDefinition
            {
                PlotId = OrchardPlotId,
                PropertyId = "property_farmland_green_cay_cassava",
                DisplayName = "圣玛丽果园分地",
                SoilType = "loam",
                WaterSourceId = "water_cistern_saint_marys",
                AreaUnits = 2,
                Tags = new List<string> { "orchard", "fruit", "managed_land" },
            },
            new D13FarmingPlotDefinition
            {
                PlotId = CassavaPlotId,
                PropertyId = "property_farmland_green_cay_cassava",
                DisplayName = "绿礁村木薯田",
                SoilType = "loam",
                WaterSourceId = "water_well_green_cay",
                AreaUnits = 1,
                Tags = new List<string> { "vegetable", "cash_crop", "managed_land" },
            },
        };

        public static IReadOnlyList<D13FarmingCropDefinition> Crops { get { return CropList; } }
        public static IReadOnlyList<D13FarmingPlotDefinition> Plots { get { return PlotList; } }

        public static D13FarmingCropDefinition FindCrop(string cropId)
        {
            if (string.IsNullOrWhiteSpace(cropId)) return null;
            foreach (var item in CropList) if (item != null && item.CropId == cropId) return item;
            return null;
        }

        public static D13FarmingPlotDefinition FindPlot(string plotId)
        {
            if (string.IsNullOrWhiteSpace(plotId)) return null;
            foreach (var item in PlotList) if (item != null && item.PlotId == plotId) return item;
            return null;
        }

        public static D13FarmingCropDefinition FindCropForItem(string itemId)
        {
            foreach (var item in CropList)
                if (item != null && (item.HarvestItemId == itemId || item.ProcessedItemId == itemId)) return item;
            return null;
        }

        public static void RegisterInventoryDefinitions()
        {
            foreach (var crop in CropList)
            {
                D3InventorySystem.RegisterCargoDefinition(crop.SeedItemId, 1, 1);
                D3InventorySystem.RegisterCargoDefinition(crop.HarvestItemId, 1, crop.Kind == D13FarmingCropKind.Sugarcane ? 2 : 1);
                D3InventorySystem.RegisterCargoDefinition(crop.ProcessedItemId, 1, 1);
            }
            D3InventorySystem.RegisterCargoDefinition(PestTreatmentItemId, 1, 1);
        }

        private static D13FarmingCropDefinition Crop(string id, string name, D13FarmingCropKind kind,
            string seed, string harvest, string processed, string soil, string market,
            int growthDays, int baseYield, int labor, int water, int pestRisk, int stormResistance,
            string[] tags)
        {
            return new D13FarmingCropDefinition
            {
                CropId = id,
                DisplayName = name,
                Kind = kind,
                SeedItemId = seed,
                HarvestItemId = harvest,
                ProcessedItemId = processed,
                PestTreatmentItemId = PestTreatmentItemId,
                RequiredSoilType = soil,
                RotationFamily = market,
                MarketRef = "market_santa_marys_" + market,
                GrowthDays = growthDays,
                BaseYield = baseYield,
                LaborRequired = labor,
                WaterPerDay = water,
                PestRiskPercent = pestRisk,
                StormResistancePercent = stormResistance,
                ProcessInputPerBatch = 2,
                ProcessOutputPerBatch = 1,
                ProcessDays = 1,
                AllowedSeasons = new List<D13FarmingSeason> { D13FarmingSeason.Dry, D13FarmingSeason.Wet },
                Tags = new List<string>(tags),
            };
        }
    }
}
