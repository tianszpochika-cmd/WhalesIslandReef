// D12CollectionCatalog —— 图鉴代表条目。内容 ID 必须来自 D8 内容注册表或 D12
// 已冻结的地图目录；目录只描述入口和来源，不代表玩家已收集。

using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12CollectionCatalog
    {
        public const string HistoricalFileId = "asset_historical_saint_marys_log";
        public const string SpecialWeaponId = "item_special_weapon_moonwake_cutlass";

        public static IReadOnlyList<D12CollectionEntryDefinition> CreateVerticalSlice()
        {
            return new[]
            {
                Entry("animal_green_turtle", D12CollectionCategory.Animal, "content.animal.animal_green_turtle.name", "CollectionSystem:animal_green_turtle", "achievement_atlas_animal_green_turtle", D12CollectionAcquisitionMethod.Observation, D12CollectionAcquisitionMethod.Salvage),
                Entry("plant_red_mangrove", D12CollectionCategory.Plant, "content.plant.plant_red_mangrove.name", "EcologySystem:plant_red_mangrove", "achievement_atlas_plant_red_mangrove", D12CollectionAcquisitionMethod.Observation, D12CollectionAcquisitionMethod.Harvest),
                Entry("fish_mackerel_school", D12CollectionCategory.Fish, "content.fish.fish_mackerel_school.name", "EcologySystem:fish_mackerel_school", "achievement_atlas_fish_mackerel_school", D12CollectionAcquisitionMethod.Observation, D12CollectionAcquisitionMethod.Catch),
                Entry("ship_moonwake", D12CollectionCategory.Ship, "content.ship.ship_moonwake.name", "ShipSystem:ship_moonwake", "achievement_atlas_ship_moonwake", D12CollectionAcquisitionMethod.Observation, D12CollectionAcquisitionMethod.Purchase, D12CollectionAcquisitionMethod.Transfer),
                Entry("outfit_career_fisher_handline", D12CollectionCategory.Clothing, "content.outfit.outfit_career_fisher_handline.name", "AppearanceSystem:outfit_career_fisher_handline", "achievement_atlas_outfit_career_fisher_handline", D12CollectionAcquisitionMethod.Purchase, D12CollectionAcquisitionMethod.Gift, D12CollectionAcquisitionMethod.Transfer),
                Entry("reef_saint_marys_east", D12CollectionCategory.Map, "content.map.reef_saint_marys_east.name", "ExplorationSystem:reef_saint_marys_east", "achievement_atlas_reef_saint_marys_east", D12CollectionAcquisitionMethod.MapDiscovery),
                AuthorizedEntry(HistoricalFileId, D12CollectionCategory.HistoricalFile, "content.asset.asset_historical_saint_marys_log.name", "ArchiveSystem:saint_marys_log", "achievement_atlas_saint_marys_log", "archive_permit_saint_marys", D12CollectionAcquisitionMethod.AuthorizedArchive),
                Entry("reward_first_chart", D12CollectionCategory.Treasure, "content.reward.reward_first_chart.name", "QuestSystem:reward_first_chart", "achievement_atlas_first_chart", D12CollectionAcquisitionMethod.QuestReward, D12CollectionAcquisitionMethod.Salvage),
                AuthorizedEntry(SpecialWeaponId, D12CollectionCategory.SpecialWeapon, "content.item.item_special_weapon_moonwake_cutlass.name", "ArmorySystem:moonwake_cutlass", "achievement_atlas_moonwake_cutlass", "armory_authorization_moonwake", D12CollectionAcquisitionMethod.AuthorizedArmory),
            };
        }

        private static D12CollectionEntryDefinition Entry(string contentId, D12CollectionCategory category,
            string displayNameKey, string sourceRef, string achievementId,
            params D12CollectionAcquisitionMethod[] methods)
        {
            var result = new D12CollectionEntryDefinition
            {
                ContentId = contentId,
                Category = category,
                DisplayNameKey = displayNameKey,
                SourceRef = sourceRef,
                AchievementId = achievementId,
            };
            result.AllowedAcquisitionMethods.AddRange(methods);
            return result;
        }

        private static D12CollectionEntryDefinition AuthorizedEntry(string contentId, D12CollectionCategory category,
            string displayNameKey, string sourceRef, string achievementId, string authorizationRef,
            params D12CollectionAcquisitionMethod[] methods)
        {
            var result = Entry(contentId, category, displayNameKey, sourceRef, achievementId, methods);
            result.RequiresAuthorization = true;
            result.RequiredAuthorizationRef = authorizationRef;
            return result;
        }
    }
}
