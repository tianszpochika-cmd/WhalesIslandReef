// D10AppearanceCatalog —— 少量可读、可测试的外观代表样例。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D10
{
    public static class D10AppearanceCatalog
    {
        public static readonly IReadOnlyList<D10AppearanceItemDefinition> All = Build();
        private static readonly HashSet<string> ValidDyes = new HashSet<string>(StringComparer.Ordinal)
        {
            "item_dye_indigo",
        };

        public static bool IsValidDye(string dyeId)
        {
            return !string.IsNullOrEmpty(dyeId) && ValidDyes.Contains(dyeId);
        }

        public static D10AppearanceItemDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var item in All)
                if (item.Id == id) return item;
            return null;
        }

        public static D10AppearanceSnapshot CreateStarterSnapshot()
        {
            var result = new D10AppearanceSnapshot();
            foreach (var item in All)
            {
                if (item.StarterOwned)
                {
                    result.Owned[item.Id] = new D10OwnedAppearance
                    {
                        ItemId = item.Id,
                        Durability = item.MaxDurability,
                        PurchaseCount = 1,
                    };
                    result.ActiveLoadout.Set(item.Slot, item.Id);
                }
            }
            return result;
        }

        private static List<D10AppearanceItemDefinition> Build()
        {
            return new List<D10AppearanceItemDefinition>
            {
                Item("outfit_body_sun_bronzed", "asset_ph_crew_male", D10AppearanceSlot.Body,
                    "content.outfit.outfit_body_sun_bronzed.name", 0, 0, false, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "inferred"),
                Item("outfit_hair_short_brown", "asset_ph_crew_male", D10AppearanceSlot.Hair,
                    "content.outfit.outfit_hair_short_brown.name", 0, 0, false, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "inferred"),
                Item("outfit_hat_straw_deck", "asset_ph_crew_male", D10AppearanceSlot.Hat,
                    "content.outfit.outfit_hat_straw_deck.name", 18, 4, true, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "historical"),
                Item("outfit_top_linen_deckhand", "asset_ph_crew_male", D10AppearanceSlot.Top,
                    "content.outfit.outfit_top_linen_deckhand.name", 26, 5, true, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "historical"),
                Item("outfit_bottom_canvas_trousers", "asset_ph_crew_male", D10AppearanceSlot.Bottom,
                    "content.outfit.outfit_bottom_canvas_trousers.name", 22, 4, true, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "historical"),
                Item("outfit_coat_rain_oilskin", "asset_ph_crew_male", D10AppearanceSlot.Coat,
                    "content.outfit.outfit_coat_rain_oilskin.name", 54, 10, true, false,
                    culture: "island_saint_marys", career: "deckhand", weather: "rain", historical: "historical"),
                Item("outfit_shoes_deck_boots", "asset_ph_crew_male", D10AppearanceSlot.Shoes,
                    "content.outfit.outfit_shoes_deck_boots.name", 32, 6, true, true,
                    culture: "island_saint_marys", career: "deckhand", weather: "rain", historical: "historical"),
                Item("outfit_gloves_ropework", "asset_ph_crew_male", D10AppearanceSlot.Gloves,
                    "content.outfit.outfit_gloves_ropework.name", 15, 3, true, false,
                    culture: "island_saint_marys", career: "deckhand", weather: "rain", historical: "historical"),
                Item("outfit_accessory_compass_badge", "asset_ph_crew_male", D10AppearanceSlot.Accessory,
                    "content.outfit.outfit_accessory_compass_badge.name", 40, 8, true, false,
                    culture: "island_saint_marys", career: "navigator", weather: "clear", historical: "inferred"),
                Item("outfit_career_fisher_handline", "asset_ph_crew_male", D10AppearanceSlot.CareerEquipment,
                    "content.outfit.outfit_career_fisher_handline.name", 28, 5, false, false,
                    culture: "island_saint_marys", career: "fisher", weather: "clear", historical: "historical"),
                Item("outfit_injury_bandaged_arm", "asset_ph_crew_male", D10AppearanceSlot.Injury,
                    "content.outfit.outfit_injury_bandaged_arm.name", 0, 0, false, false,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "inferred",
                    injury: true),
                Item("outfit_injury_old_leg", "asset_ph_crew_male", D10AppearanceSlot.Injury,
                    "content.outfit.outfit_injury_old_leg.name", 0, 0, false, false,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "inferred",
                    injury: true),
                Item("outfit_ceremonial_silk_coat", "asset_ph_crew_male", D10AppearanceSlot.Coat,
                    "content.outfit.outfit_ceremonial_silk_coat.name", 120, 20, true, false,
                    culture: "island_saint_marys", career: "navigator", weather: "clear", historical: "inferred"),
                Item("outfit_missing_tailor_placeholder", "asset_ph_crew_male", D10AppearanceSlot.Accessory,
                    "content.outfit.outfit_missing_tailor_placeholder.name", 9, 2, true, false,
                    culture: "island_saint_marys", career: "deckhand", weather: "clear", historical: "fictional"),
            };
        }

        private static D10AppearanceItemDefinition Item(string id, string assetId, D10AppearanceSlot slot,
            string key, int price, int repairCost, bool dyeable, bool starterOwned,
            string culture, string career, string weather, string historical, bool injury = false)
        {
            var item = new D10AppearanceItemDefinition
            {
                Id = id,
                AssetId = assetId,
                ResourceKey = "D10/Appearance/" + assetId,
                Slot = slot,
                DisplayNameKey = key,
                Price = price,
                RepairCost = repairCost,
                MaxDurability = slot == D10AppearanceSlot.Injury ? 1 : 100,
                Dyeable = dyeable,
                StarterOwned = starterOwned,
                IsInjuryOverlay = injury,
                ExclusiveGroup = slot == D10AppearanceSlot.Coat ? "outer_layer" : "",
            };
            if (!string.IsNullOrEmpty(culture)) item.CultureTags.Add(culture);
            if (!string.IsNullOrEmpty(career)) item.CareerTags.Add(career);
            if (!string.IsNullOrEmpty(weather)) item.WeatherTags.Add(weather);
            if (!string.IsNullOrEmpty(historical)) item.HistoricalTags.Add(historical);
            if (id == "outfit_ceremonial_silk_coat")
                item.ExcludesItemIds.Add("outfit_coat_rain_oilskin");
            return item;
        }
    }
}
