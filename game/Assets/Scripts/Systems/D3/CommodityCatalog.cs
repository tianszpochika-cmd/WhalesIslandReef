// ============================================================
// CommodityCatalog —— D3 M1 的 10 种货物静态定义
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.D3
{
    public static class CommodityCatalog
    {
        public static readonly IReadOnlyList<CommodityDefinition> All = new List<CommodityDefinition>
        {
            new CommodityDefinition("salt", "盐", 4, 1, 1, 60),
            new CommodityDefinition("sugar", "糖", 6, 1, 1, 70),
            new CommodityDefinition("tobacco", "烟草", 12, 1, 1, 45),
            new CommodityDefinition("rum", "朗姆酒", 18, 1, 1, 40),
            new CommodityDefinition("coffee", "咖啡", 14, 1, 1, 55),
            new CommodityDefinition("cocoa", "可可", 16, 1, 1, 50),
            new CommodityDefinition("spices", "香料", 22, 1, 1, 35),
            new CommodityDefinition("dyes", "染料", 20, 1, 1, 30),
            new CommodityDefinition("timber", "木材", 8, 2, 2, 65),
            new CommodityDefinition("medicine", "药材", 28, 1, 1, 25),
        };

        private static readonly Dictionary<string, CommodityDefinition> ById = BuildIndex();

        public static CommodityDefinition Find(string id)
        {
            if (id == null) return null;
            ById.TryGetValue(id, out CommodityDefinition definition);
            return definition;
        }

        public static void SeedMarket(D3MarketState market, int amount = 50)
        {
            if (market == null) return;
            foreach (var commodity in All)
            {
                market.Stock[commodity.Id] = amount;
                market.Demand[commodity.Id] = commodity.DemandTarget;
                market.PlayerTradeVolume[commodity.Id] = 0;
            }
        }

        private static Dictionary<string, CommodityDefinition> BuildIndex()
        {
            var result = new Dictionary<string, CommodityDefinition>();
            foreach (var item in All) result[item.Id] = item;
            return result;
        }
    }
}
