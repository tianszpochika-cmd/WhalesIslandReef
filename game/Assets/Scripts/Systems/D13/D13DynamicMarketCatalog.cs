// D13-A07 商品与运输路线不可变目录。运行时数量、价格、市场和库存不在目录中。

using System.Collections.Generic;
using Whalebone.Systems.D3;

namespace Whalebone.Systems.D13
{
    public static class D13DynamicMarketCatalog
    {
        private static readonly IReadOnlyList<D13MarketGoodDefinition> GoodList = BuildGoods();
        private static readonly IReadOnlyList<D13MarketRouteDefinition> RouteList = BuildRoutes();

        public static IReadOnlyList<D13MarketGoodDefinition> Goods { get { return GoodList; } }
        public static IReadOnlyList<D13MarketRouteDefinition> Routes { get { return RouteList; } }

        public static D13MarketGoodDefinition FindGood(string itemId)
        {
            foreach (var item in GoodList) if (item != null && (item.ItemId == itemId || item.GoodId == itemId)) return item;
            return null;
        }

        public static D13MarketRouteDefinition FindRoute(string routeId)
        {
            foreach (var item in RouteList) if (item != null && item.RouteId == routeId) return item;
            return null;
        }

        public static void RegisterInventoryDefinitions()
        {
            foreach (var good in GoodList)
                if (good != null) D3InventorySystem.RegisterCargoDefinition(good.ItemId, good.UnitWeight, good.UnitVolume);
        }

        private static IReadOnlyList<D13MarketGoodDefinition> BuildGoods()
        {
            return new List<D13MarketGoodDefinition>
            {
                Good("item_fresh_mackerel", "鲜鲭鱼", D13MarketGoodCategory.FishCatch, 10, 1, 1, 55, 20, D13MarketLegalStatus.Legal, false, null, new[] { "D11FishingHarvestSystem" }, "fish", "fresh", "perishable"),
                Good("item_fruit_guava", "番石榴果实", D13MarketGoodCategory.FarmProduce, 9, 1, 1, 50, 15, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "D11VegetationHarvestSystem" }, "farm", "fruit", "perishable"),
                Good("item_vegetable_cassava", "木薯根", D13MarketGoodCategory.FarmProduce, 7, 1, 1, 55, 10, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "D11VegetationHarvestSystem" }, "farm", "vegetable", "food"),
                Good("item_guava_preserve", "番石榴果脯", D13MarketGoodCategory.ProcessedProduct, 18, 1, 1, 42, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem" }, "processed", "fruit", "food"),
                Good("item_cassava_flour", "木薯粉", D13MarketGoodCategory.ProcessedProduct, 14, 1, 1, 48, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem" }, "processed", "vegetable", "food"),
                Good("item_dye_indigo", "靛蓝染料", D13MarketGoodCategory.ProcessedProduct, 20, 1, 1, 30, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem", "D11VegetationHarvestSystem" }, "processed", "dye", "crafting"),
                Good("item_coffee_roasted", "烘焙咖啡豆", D13MarketGoodCategory.ProcessedProduct, 24, 1, 1, 45, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem" }, "processed", "coffee", "food"),
                Good("item_cocoa_cured", "醇化可可豆", D13MarketGoodCategory.ProcessedProduct, 26, 1, 1, 40, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem" }, "processed", "cocoa", "food"),
                Good("item_tobacco_cured", "醇化烟叶", D13MarketGoodCategory.ProcessedProduct, 22, 1, 1, 35, 2, D13MarketLegalStatus.Licensed, true, "permit_tobacco_trade", new[] { "FarmingSystem", "BuildingSystem" }, "processed", "tobacco", "licensed"),
                Good("item_spices_dried", "干燥香料", D13MarketGoodCategory.ProcessedProduct, 28, 1, 1, 32, 2, D13MarketLegalStatus.Legal, false, null, new[] { "FarmingSystem", "BuildingSystem", "D11VegetationHarvestSystem" }, "processed", "spice", "aromatic"),
                Good("item_herb_prepared", "配制药草", D13MarketGoodCategory.Medicine, 30, 1, 1, 28, 3, D13MarketLegalStatus.Licensed, true, "permit_medicinal_trade", new[] { "FarmingSystem", "BuildingSystem", "D11VegetationHarvestSystem" }, "medicine", "herb", "licensed"),
                Good("timber", "木材", D13MarketGoodCategory.ShipMaterial, 8, 2, 2, 65, 0, D13MarketLegalStatus.Restricted, true, "permit_timber_cutting", new[] { "D11VegetationHarvestSystem", "BuildingSystem" }, "ship_material", "timber", "restricted"),
                Good("item_fiber_agave", "龙舌兰纤维", D13MarketGoodCategory.ShipMaterial, 11, 1, 2, 45, 3, D13MarketLegalStatus.Legal, false, null, new[] { "D11VegetationHarvestSystem", "FarmingSystem", "BuildingSystem" }, "ship_material", "fiber", "crafting"),
                Good("item_resin_pine", "松脂", D13MarketGoodCategory.ShipMaterial, 16, 1, 1, 35, 4, D13MarketLegalStatus.Legal, false, null, new[] { "D11VegetationHarvestSystem", "BuildingSystem" }, "ship_material", "resin", "crafting"),
                Good("outfit_top_linen_deckhand", "亚麻水手上衣", D13MarketGoodCategory.Clothing, 15, 1, 1, 38, 4, D13MarketLegalStatus.Legal, false, null, new[] { "AppearanceSystem", "BuildingSystem" }, "clothing", "linen", "tailor"),
                Good("item_pet_feed", "宠物饲料", D13MarketGoodCategory.PetSupply, 6, 1, 1, 34, 8, D13MarketLegalStatus.Legal, false, null, new[] { "AnimalCareSystem", "ItemCatalog" }, "pet", "feed", "care"),
                Good("item_gear_handline_basic", "基础手线", D13MarketGoodCategory.Tool, 12, 1, 1, 22, 0, D13MarketLegalStatus.Legal, false, null, new[] { "ItemCatalog", "BuildingSystem" }, "tool", "fishing", "gear"),
                Good("item_special_weapon_moonwake_cutlass", "月醒号礼仪短剑", D13MarketGoodCategory.Special, 80, 2, 1, 5, 0, D13MarketLegalStatus.Restricted, true, "permit_armory_moonwake", new[] { "RewardSystem", "CollectionSystem", "ItemCatalog" }, "special", "authorized", "unique"),
                Good("item_toxin_datura", "曼陀罗毒物", D13MarketGoodCategory.Toxin, 5, 1, 1, 4, 0, D13MarketLegalStatus.Illegal, true, "permit_toxin_research", new[] { "D11VegetationHarvestSystem", "EcologySystem" }, "toxin", "dangerous", "restricted"),
                Good("medicine", "药材", D13MarketGoodCategory.Medicine, 28, 1, 1, 25, 3, D13MarketLegalStatus.Licensed, true, "permit_medicinal_trade", new[] { "D11VegetationHarvestSystem", "FarmingSystem", "AnimalCareSystem" }, "medicine", "trade", "licensed"),
            };
        }

        private static IReadOnlyList<D13MarketRouteDefinition> BuildRoutes()
        {
            return new List<D13MarketRouteDefinition>
            {
                new D13MarketRouteDefinition { RouteId = "route_santa_margarita_to_green_cay", OriginLocationId = "santa_margarita", DestinationLocationId = "green_cay", CapacityUnits = 30, BaseLossPercent = 2, Tags = new List<string> { "coastal", "starter" } },
                new D13MarketRouteDefinition { RouteId = "route_green_cay_to_driftwood_isle", OriginLocationId = "green_cay", DestinationLocationId = "driftwood_isle", CapacityUnits = 24, BaseLossPercent = 4, Tags = new List<string> { "open_water", "workshop_supply" } },
                new D13MarketRouteDefinition { RouteId = "route_santa_margarita_to_driftwood_isle", OriginLocationId = "santa_margarita", DestinationLocationId = "driftwood_isle", CapacityUnits = 20, BaseLossPercent = 5, Tags = new List<string> { "open_water", "ship_material" } },
            };
        }

        private static D13MarketGoodDefinition Good(string itemId, string display, D13MarketGoodCategory category,
            int basePrice, int weight, int volume, int demand, int fragility, D13MarketLegalStatus legal,
            bool permit, string permitId, string[] producers, params string[] tags)
        {
            var result = new D13MarketGoodDefinition
            {
                GoodId = "good_" + itemId.Replace("item_", string.Empty).Replace("-", "_"), ItemId = itemId, DisplayName = display,
                Category = category, BasePrice = basePrice, UnitWeight = weight, UnitVolume = volume, DemandTarget = demand,
                FragilityPercent = fragility, LegalStatus = legal, RequiresPermit = permit, PermitId = permitId,
                AllowedProducerOwners = new List<string>(producers), Tags = new List<string>(tags), Transportable = true,
            };
            result.Validate();
            return result;
        }
    }
}
