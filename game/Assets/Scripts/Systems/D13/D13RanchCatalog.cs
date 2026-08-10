// D13-A04 养殖定义目录。定义不可变；动物实例由 RanchSystem 创建并写入。

using System;
using System.Collections.Generic;

using Whalebone.Systems.D3;

namespace Whalebone.Systems.D13
{
    public static class D13RanchCatalog
    {
        public const string ChickenId = "ranch_poultry_chicken";
        public const string GoatId = "ranch_livestock_goat";
        public const string PigId = "ranch_livestock_pig";
        public const string CattleId = "ranch_livestock_cattle";

        public const string PoultryRanchId = "ranch_green_cay_poultry";
        public const string SmallholdingId = "ranch_saint_marys_smallholding";

        public const string FeedItemId = "item_livestock_feed";
        public const string MedicineItemId = "item_livestock_medicine";

        private static readonly IReadOnlyList<D13RanchAnimalDefinition> AnimalList = new List<D13RanchAnimalDefinition>
        {
            Animal(ChickenId, "animal_domestic_chicken", "加勒比家鸡", D13RanchAnimalKind.Poultry, "item_chicken_egg", D13RanchProductKind.Egg, 2, 1, 2, 1, 1, 12, 1, "market_santa_marys_poultry", new[] { "poultry", "layer", "egg" }),
            Animal(GoatId, "animal_domestic_goat", "加勒比山羊", D13RanchAnimalKind.Livestock, "item_goat_milk", D13RanchProductKind.Milk, 2, 2, 4, 2, 2, 18, 1, "market_santa_marys_dairy", new[] { "livestock", "dairy", "goat" }),
            Animal(PigId, "animal_domestic_pig", "加勒比家猪", D13RanchAnimalKind.Livestock, "item_pig_meat", D13RanchProductKind.Meat, 1, 4, 4, 2, 2, 20, 1, "market_santa_marys_meat", new[] { "livestock", "meat", "pig" }),
            Animal(CattleId, "animal_domestic_cattle", "加勒比牛", D13RanchAnimalKind.Livestock, "item_cattle_leather", D13RanchProductKind.Leather, 1, 5, 6, 3, 3, 16, 1, "market_santa_marys_hide", new[] { "livestock", "draft", "leather" }),
        };

        private static readonly IReadOnlyList<D13RanchFacilityDefinition> FacilityList = new List<D13RanchFacilityDefinition>
        {
            new D13RanchFacilityDefinition
            {
                RanchId = PoultryRanchId,
                PropertyId = "property_farmland_green_cay_cassava",
                DisplayName = "绿礁村家禽棚",
                WaterSourceId = "water_well_green_cay",
                SpaceCapacity = 6,
                WaterCapacity = 100,
                Tags = new List<string> { "poultry", "smallholding", "green_cay" },
            },
            new D13RanchFacilityDefinition
            {
                RanchId = SmallholdingId,
                PropertyId = "property_farmland_green_cay_cassava",
                DisplayName = "圣玛丽小牧场",
                WaterSourceId = "water_cistern_saint_marys",
                SpaceCapacity = 8,
                WaterCapacity = 120,
                Tags = new List<string> { "livestock", "smallholding", "saint_marys" },
            },
        };

        public static IReadOnlyList<D13RanchAnimalDefinition> Animals { get { return AnimalList; } }
        public static IReadOnlyList<D13RanchFacilityDefinition> Facilities { get { return FacilityList; } }

        public static D13RanchAnimalDefinition FindAnimal(string definitionId)
        {
            if (string.IsNullOrWhiteSpace(definitionId)) return null;
            foreach (var item in AnimalList) if (item != null && item.DefinitionId == definitionId) return item;
            return null;
        }

        public static D13RanchAnimalDefinition FindByProduct(string itemId)
        {
            foreach (var item in AnimalList) if (item != null && item.ProductItemId == itemId) return item;
            return null;
        }

        public static D13RanchFacilityDefinition FindFacility(string ranchId)
        {
            if (string.IsNullOrWhiteSpace(ranchId)) return null;
            foreach (var item in FacilityList) if (item != null && item.RanchId == ranchId) return item;
            return null;
        }

        public static void RegisterInventoryDefinitions()
        {
            D3InventorySystem.RegisterCargoDefinition(FeedItemId, 1, 1);
            D3InventorySystem.RegisterCargoDefinition(MedicineItemId, 1, 1);
            foreach (var item in AnimalList)
                if (item != null) D3InventorySystem.RegisterCargoDefinition(item.ProductItemId, 1, item.ProductKind == D13RanchProductKind.Leather ? 2 : 1);
        }

        private static D13RanchAnimalDefinition Animal(string id, string sourceContentId, string name,
            D13RanchAnimalKind kind, string productItemId, D13RanchProductKind productKind,
            int productPerCycle, int feedPerCycle, int waterPerCycle, int spaceRequired, int laborRequired,
            int diseaseRisk, int quarantineDays, string marketRef, string[] tags)
        {
            return new D13RanchAnimalDefinition
            {
                DefinitionId = id,
                SourceContentId = sourceContentId,
                DisplayName = name,
                Kind = kind,
                FeedItemId = FeedItemId,
                FeedPerCycle = feedPerCycle,
                WaterPerCycle = waterPerCycle,
                SpaceRequired = spaceRequired,
                LaborRequired = laborRequired,
                ProductItemId = productItemId,
                ProductKind = productKind,
                ProductPerCycle = productPerCycle,
                DiseaseRiskPercent = diseaseRisk,
                QuarantineDays = quarantineDays,
                MarketRef = marketRef,
                BreedingMode = D13RanchBreedingMode.Disabled,
                Tags = new List<string>(tags),
            };
        }
    }
}
