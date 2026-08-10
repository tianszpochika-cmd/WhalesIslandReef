// ============================================================
// D11FishingHarvestCatalog —— D11-A02 正式内容与处理映射
//
// 数值是实现入口的登记值，不是 UI 文本，也不在处理系统中重复硬编码。
// 既有 D9 鲭鱼价格沿用 D9 目录；新增商品只提供可交易的接入基线。
// ============================================================

using System.Collections.Generic;
using Whalebone.Systems.D3;

namespace Whalebone.Systems.D11
{
    public static class D11FishingHarvestCatalog
    {
        public static readonly IReadOnlyList<D11FishingGearDefinition> Gears =
            new List<D11FishingGearDefinition>
            {
                new D11FishingGearDefinition
                {
                    Id = "gear_handline_basic", ContentId = "item_gear_handline_basic",
                    DisplayName = "基础手线", Method = D11HarvestMethod.Handline,
                    CatchCapacity = 2, MaximumDepthMeters = 5f,
                },
                new D11FishingGearDefinition
                {
                    Id = "gear_shore_rod", ContentId = "item_gear_shore_rod",
                    DisplayName = "岸钓竿", Method = D11HarvestMethod.ShoreFishing,
                    CatchCapacity = 3, MaximumDepthMeters = 12f,
                },
                new D11FishingGearDefinition
                {
                    Id = "gear_cast_net", ContentId = "item_gear_cast_net",
                    DisplayName = "小型撒网", Method = D11HarvestMethod.CastNet,
                    CatchCapacity = 5, MaximumDepthMeters = 4f,
                },
                new D11FishingGearDefinition
                {
                    Id = "gear_fish_trap", ContentId = "item_gear_fish_trap",
                    DisplayName = "近岸鱼笼", Method = D11HarvestMethod.FishTrap,
                    CatchCapacity = 4, MaximumDepthMeters = 8f,
                },
                new D11FishingGearDefinition
                {
                    Id = "gear_shellfish_rake", ContentId = "item_gear_shellfish_rake",
                    DisplayName = "贝类耙", Method = D11HarvestMethod.ShellfishGathering,
                    CatchCapacity = 3, MaximumDepthMeters = 1f,
                },
                new D11FishingGearDefinition
                {
                    Id = "gear_seaweed_hook", ContentId = "item_gear_seaweed_hook",
                    DisplayName = "海草钩", Method = D11HarvestMethod.SeaweedGathering,
                    CatchCapacity = 4, MaximumDepthMeters = 1f,
                },
            };

        public static readonly IReadOnlyList<D11FishingLocationDefinition> Locations =
            new List<D11FishingLocationDefinition>
            {
                new D11FishingLocationDefinition
                {
                    Id = "nearshore_saint_marys", FishSchoolId = "fish_mackerel_school",
                    FishSpeciesId = "mackerel", Habitat = "nearshore/reef_edge",
                    PreferredDepthMeters = 2f, PreferredTimeOfDay = "day", PreferredTideBand = "flood",
                    Method = D11HarvestMethod.Handline, WorldX = -4f, WorldZ = 8f,
                },
                new D11FishingLocationDefinition
                {
                    Id = "reef_edge_coral_bay", FishSchoolId = "fish_snapper_school",
                    FishSpeciesId = "snapper", Habitat = "reef_edge",
                    PreferredDepthMeters = 8f, PreferredTimeOfDay = "dawn", PreferredTideBand = "mid",
                    Method = D11HarvestMethod.ShoreFishing, WorldX = -7f, WorldZ = 11f,
                },
                new D11FishingLocationDefinition
                {
                    Id = "sardine_bay", FishSchoolId = "fish_sardine_school",
                    FishSpeciesId = "sardine", Habitat = "nearshore",
                    PreferredDepthMeters = 4f, PreferredTimeOfDay = "dusk", PreferredTideBand = "ebb",
                    Method = D11HarvestMethod.CastNet, WorldX = -2f, WorldZ = 6f,
                },
                new D11FishingLocationDefinition
                {
                    Id = "saint_marys_shallows", ResourceId = "plant_conch_bed",
                    Habitat = "coast", PreferredDepthMeters = 0.5f,
                    PreferredTimeOfDay = "dawn", PreferredTideBand = "ebb",
                    Method = D11HarvestMethod.ShellfishGathering, WorldX = -3f, WorldZ = 5f,
                },
                new D11FishingLocationDefinition
                {
                    Id = "saint_marys_tidepools", ResourceId = "plant_shallow_seaweed",
                    Habitat = "coast", PreferredDepthMeters = 0.5f,
                    PreferredTimeOfDay = "dawn", PreferredTideBand = "flood",
                    Method = D11HarvestMethod.SeaweedGathering, WorldX = -3f, WorldZ = 4f,
                },
                new D11FishingLocationDefinition
                {
                    Id = "protected_turtle_cove", ResourceId = "plant_shallow_seaweed",
                    Habitat = "coast", PreferredDepthMeters = 0.5f,
                    PreferredTimeOfDay = "day", PreferredTideBand = "mid",
                    IsProtectedArea = true, ProtectionRuleId = "protected_turtle_cove_no_take",
                    Method = D11HarvestMethod.SeaweedGathering, WorldX = -9f, WorldZ = 14f,
                },
            };

        public static readonly IReadOnlyList<D3MarketItemDefinition> MarketItems =
            new List<D3MarketItemDefinition>
            {
                // D9 price entries are preserved exactly; D11 only re-registers the shared owner.
                new D3MarketItemDefinition("item_fresh_mackerel", "鲜鲭鱼", 11, 1, 1, 42),
                new D3MarketItemDefinition("item_salted_mackerel", "盐渍鲭鱼", 17, 1, 1, 36),
                new D3MarketItemDefinition("item_fresh_snapper", "鲜笛鲷", 15, 1, 1, 38),
                new D3MarketItemDefinition("item_salted_snapper", "盐渍笛鲷", 22, 1, 1, 32),
                new D3MarketItemDefinition("item_dried_snapper", "鱼干笛鲷", 20, 1, 1, 34),
                new D3MarketItemDefinition("item_fresh_sardine", "鲜沙丁鱼", 8, 1, 1, 52),
                new D3MarketItemDefinition("item_salted_sardine", "盐渍沙丁鱼", 12, 1, 1, 46),
                new D3MarketItemDefinition("item_dried_sardine", "沙丁鱼干", 10, 1, 1, 48),
                new D3MarketItemDefinition("item_dried_mackerel", "鲭鱼干", 16, 1, 1, 38),
                new D3MarketItemDefinition("item_fish_oil", "鱼油", 24, 1, 1, 28),
                new D3MarketItemDefinition("item_conch", "海螺", 13, 1, 1, 30),
                new D3MarketItemDefinition("item_cured_shellfish", "腌贝肉", 19, 1, 1, 26),
                new D3MarketItemDefinition("item_seaweed", "浅水海草", 4, 1, 1, 60),
                new D3MarketItemDefinition("item_dried_seaweed", "海草干", 7, 1, 1, 54),
            };

        public static readonly IReadOnlyList<D11ProcessingDefinition> Processing =
            new List<D11ProcessingDefinition>
            {
                new D11ProcessingDefinition { InputItemId = "item_fresh_mackerel", OutputItemId = "item_salted_mackerel", Method = D11ProcessingMethod.Salt, DisplayName = "腌制鲭鱼" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_mackerel", OutputItemId = "item_dried_mackerel", Method = D11ProcessingMethod.Dry, DisplayName = "制作鲭鱼干" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_mackerel", OutputItemId = "item_fish_oil", Method = D11ProcessingMethod.RenderOil, OutputNumerator = 1, OutputDenominator = 2, DisplayName = "熬制鱼油" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_snapper", OutputItemId = "item_salted_snapper", Method = D11ProcessingMethod.Salt, DisplayName = "腌制笛鲷" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_snapper", OutputItemId = "item_dried_snapper", Method = D11ProcessingMethod.Dry, DisplayName = "制作笛鲷干" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_snapper", OutputItemId = "item_fish_oil", Method = D11ProcessingMethod.RenderOil, OutputNumerator = 1, OutputDenominator = 2, DisplayName = "熬制鱼油" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_sardine", OutputItemId = "item_salted_sardine", Method = D11ProcessingMethod.Salt, DisplayName = "腌制沙丁鱼" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_sardine", OutputItemId = "item_dried_sardine", Method = D11ProcessingMethod.Dry, DisplayName = "制作沙丁鱼干" },
                new D11ProcessingDefinition { InputItemId = "item_fresh_sardine", OutputItemId = "item_fish_oil", Method = D11ProcessingMethod.RenderOil, OutputNumerator = 1, OutputDenominator = 2, DisplayName = "熬制鱼油" },
                new D11ProcessingDefinition { InputItemId = "item_conch", OutputItemId = "item_cured_shellfish", Method = D11ProcessingMethod.CureShellfish, DisplayName = "腌制贝肉" },
                new D11ProcessingDefinition { InputItemId = "item_seaweed", OutputItemId = "item_dried_seaweed", Method = D11ProcessingMethod.Dry, DisplayName = "制作海草干" },
            };

        public static D11FishingGearDefinition FindGear(string id)
        {
            foreach (var gear in Gears) if (gear != null && gear.Id == id) return gear;
            return null;
        }

        public static D11FishingLocationDefinition FindLocation(string id)
        {
            foreach (var location in Locations) if (location != null && location.Id == id) return location;
            return null;
        }

        public static D3MarketItemDefinition FindMarketItem(string id)
        {
            foreach (var item in MarketItems) if (item != null && item.Id == id) return item;
            return null;
        }

        public static D11ProcessingDefinition FindProcessing(string inputItemId, D11ProcessingMethod method)
        {
            foreach (var definition in Processing)
                if (definition != null && definition.InputItemId == inputItemId && definition.Method == method)
                    return definition;
            return null;
        }

        public static string FreshItemForSpecies(string fishSpeciesId)
        {
            if (fishSpeciesId == "mackerel") return "item_fresh_mackerel";
            if (fishSpeciesId == "snapper") return "item_fresh_snapper";
            if (fishSpeciesId == "sardine") return "item_fresh_sardine";
            return null;
        }

        public static string ItemForResourceKind(string resourceKind)
        {
            if (resourceKind == "shellfish") return "item_conch";
            if (resourceKind == "seaweed") return "item_seaweed";
            return "item_seaweed";
        }

        public static string ResourceKindForResourceId(string resourceId)
        {
            if (resourceId == "plant_conch_bed") return "shellfish";
            if (resourceId == "plant_shallow_seaweed") return "seaweed";
            return "plant";
        }

        public static D11FishingLocationDefinition FindFishLocationBySchool(string fishSchoolId)
        {
            foreach (var location in Locations)
                if (location != null && location.FishSchoolId == fishSchoolId) return location;
            return null;
        }
    }
}
