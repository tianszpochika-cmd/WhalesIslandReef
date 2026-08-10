// D14-A04 奖励目录：只登记稀有度、来源路由和内容引用，不登记最终数量、金钱或库存结果。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D14
{
    public sealed class D14RewardCatalog
    {
        private readonly Dictionary<string, D14RewardDefinition> _definitions =
            new Dictionary<string, D14RewardDefinition>(StringComparer.Ordinal);

        public D14RewardCatalog()
        {
            Add(new D14RewardDefinition
            {
                RewardId = "reward_d14_ordinary_supply", DisplayName = "普通补给配额", Rarity = D14RewardRarity.Ordinary,
                Kind = D14RewardKind.Item, SourceKind = D14RewardSourceKind.Exploration, ContentId = "item_chart_compass",
                SourceRef = "docs/game/design/common/D14_REWARD_SYSTEM.md", HistoricalTag = "fictional", ReviewStatus = "approved",
                TestId = "D8-A02-D14-REWARD-001", RecoveryWindowDays = 14,
                AllowedSourceOwnerSystems = new List<string> { "QuestSystem", "ExplorationSystem", "SalvageSystem" },
            });
            Add(new D14RewardDefinition
            {
                RewardId = "reward_d14_professional_rescue", DisplayName = "专业救援工具", Rarity = D14RewardRarity.Professional,
                Kind = D14RewardKind.Item, SourceKind = D14RewardSourceKind.Boarding, ContentId = "item_tool_rescue_grapnel",
                SourceRef = "docs/game/design/common/D14_REWARD_SYSTEM.md", HistoricalTag = "inferred", ReviewStatus = "approved",
                TestId = "D8-A02-D14-REWARD-002", RecoveryWindowDays = 21,
                AllowedSourceOwnerSystems = new List<string> { "CombatSystem", "AftercareSystem", "QuestSystem" },
            });
            Add(new D14RewardDefinition
            {
                RewardId = "reward_d14_rare_indigo_dye", DisplayName = "稀有靛蓝货品", Rarity = D14RewardRarity.Rare,
                Kind = D14RewardKind.Item, SourceKind = D14RewardSourceKind.Collection, ContentId = "item_dye_indigo",
                SourceRef = "docs/game/design/common/D14_REWARD_SYSTEM.md", HistoricalTag = "inferred", ReviewStatus = "approved",
                TestId = "D8-A02-D14-REWARD-003", RecoveryWindowDays = 30,
                AllowedSourceOwnerSystems = new List<string> { "CollectionAtlasSystem", "QuestSystem", "SalvageSystem" },
            });
            Add(new D14RewardDefinition
            {
                RewardId = "reward_d14_unique_moonwake_armory", DisplayName = "月醒号军械许可奖励", Rarity = D14RewardRarity.Unique,
                Kind = D14RewardKind.Item, SourceKind = D14RewardSourceKind.Collection, Unique = true,
                ContentId = "item_special_weapon_moonwake_cutlass", AuthorizationRef = "armory_authorization_moonwake",
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md", HistoricalTag = "fictional", ReviewStatus = "approved_with_warning",
                TestId = "D8-A02-D14-REWARD-004", RecoveryWindowDays = 60,
                AllowedSourceOwnerSystems = new List<string> { "CollectionAtlasSystem", "QuestSystem", "RewardSystem" },
            });
            Add(new D14RewardDefinition
            {
                RewardId = "reward_d14_worldclass_reinforced_keel", DisplayName = "世界级加固龙骨", Rarity = D14RewardRarity.WorldClass,
                Kind = D14RewardKind.ShipModule, SourceKind = D14RewardSourceKind.Achievement, Unique = true,
                ContentId = "item_ship_module_reinforced_keel", AuthorizationRef = "shipwright_module_register",
                SourceRef = "docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md", HistoricalTag = "inferred", ReviewStatus = "approved",
                TestId = "D8-A02-D14-REWARD-005", RecoveryWindowDays = 90,
                AllowedSourceOwnerSystems = new List<string> { "QuestSystem", "AchievementSystem", "RewardSystem" },
            });
        }

        public IReadOnlyList<D14RewardDefinition> All()
        {
            var result = new List<D14RewardDefinition>();
            foreach (var definition in _definitions.Values) result.Add(definition.Clone());
            return result.AsReadOnly();
        }

        public D14RewardDefinition Find(string rewardId)
        {
            D14RewardDefinition definition;
            return rewardId != null && _definitions.TryGetValue(rewardId, out definition) ? definition.Clone() : null;
        }

        private void Add(D14RewardDefinition definition)
        {
            definition.Validate("d14.reward.catalog");
            if (_definitions.ContainsKey(definition.RewardId))
                throw new InvalidOperationException("Duplicate D14 reward definition: " + definition.RewardId);
            _definitions.Add(definition.RewardId, definition.Clone());
        }
    }
}
