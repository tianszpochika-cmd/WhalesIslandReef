// D12 牌局纵切片目录：只提供一张可测试的低额牌桌，不批量生成玩法内容。

using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12PokerCatalog
    {
        public const string LowStakesTableId = "poker_table_low_stakes_01";

        public static D12PokerTableDefinition CreateLowStakesTable()
        {
            return new D12PokerTableDefinition
            {
                TableId = LowStakesTableId,
                RulesetId = D12PokerIds.RulesetId,
                LocationRef = "santa_margarita_tavern",
                OpponentId = "tavern_house",
                RelationTargetId = "npc_tavern_keeper",
                InformationPoolId = "tavern_poker_tells",
                FixedSeed = 120501,
                EntryFee = 2,
                BetCap = 20,
                DebtLimit = 40,
                CheatRiskPercent = 25,
            };
        }

        public static IReadOnlyList<D12PokerTableDefinition> CreateVerticalSlice()
        {
            return new List<D12PokerTableDefinition> { CreateLowStakesTable() };
        }
    }
}
