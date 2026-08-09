// ============================================================
// TradeEconomySystem —— D3 价格、市场库存与买卖裁决
//
// 价格公式：基础价 × 供需系数 × 港口专长 × 关税系数 × 事件扰动。
// 钱由本系统修改，货舱数量委托 D3InventorySystem 修改，避免出现第二个库存写者。
// ============================================================

using System;

namespace Whalebone.Systems.D3
{
    public sealed class TradeEconomySystem
    {
        private const int TargetStock = 50;
        private const float SellBaseRate = 0.75f;

        public MarketQuote Quote(string portId, string commodityId, D3MarketState market, float eventCoefficient = 1f)
        {
            var port = PortCatalog.Find(portId);
            var commodity = CommodityCatalog.Find(commodityId);
            if (port == null || commodity == null || market == null) return null;

            int stock = market.Stock.TryGetValue(commodityId, out int current) ? Math.Max(0, current) : 0;
            float supply = SupplyCoefficient(stock);
            float specialty = port.Specialty(commodityId);
            float eventMultiplier = Clamp(eventCoefficient, 0.5f, 1.5f);
            float buyTariff = 1f + port.TariffRate;
            float sellTariff = 1f - port.TariffRate * 0.5f;

            return new MarketQuote
            {
                PortId = port.Id,
                CommodityId = commodity.Id,
                BuyUnitPrice = ToPrice(commodity.BasePrice * supply * specialty * buyTariff * eventMultiplier),
                SellUnitPrice = ToPrice(commodity.BasePrice * supply * specialty * sellTariff * eventMultiplier * SellBaseRate),
                SupplyCoefficient = supply,
                TariffCoefficient = buyTariff,
                EventCoefficient = eventMultiplier,
            };
        }

        public TradeResult Buy(D3TradeState state, string portId, string commodityId, int quantity,
            float eventCoefficient = 1f)
        {
            if (state == null) return TradeResult.Fail("invalid_state", "贸易状态不存在。", 0);
            var quote = Quote(portId, commodityId, state.Market, eventCoefficient);
            if (quote == null) return TradeResult.Fail("invalid_reference", "港口或货物不存在。", state.Money);
            if (quantity <= 0) return TradeResult.Fail("invalid_quantity", "购买数量必须大于零。", state.Money);

            int stock = state.Market.Stock.TryGetValue(commodityId, out int available) ? Math.Max(0, available) : 0;
            if (stock < quantity)
                return TradeResult.Fail("market_empty", "港口库存不足。", state.Money);

            long total = (long)quote.BuyUnitPrice * quantity;
            if (total > state.Money)
                return TradeResult.Fail("not_enough_money", "金币不足。", state.Money);

            if (!D3InventorySystem.TryAddCargo(state.Inventory, commodityId, quantity, out string cargoError))
                return TradeResult.Fail(cargoError, cargoError == "cargo_full" ? "货舱容量不足。" : "无法装入货物。", state.Money);

            state.Money -= (int)total;
            state.Market.Stock[commodityId] = stock - quantity;
            return Success(quantity, quote.BuyUnitPrice, (int)total, state.Money, "购买完成。");
        }

        public TradeResult Sell(D3TradeState state, string portId, string commodityId, int quantity,
            float eventCoefficient = 1f)
        {
            if (state == null) return TradeResult.Fail("invalid_state", "贸易状态不存在。", 0);
            var quote = Quote(portId, commodityId, state.Market, eventCoefficient);
            if (quote == null) return TradeResult.Fail("invalid_reference", "港口或货物不存在。", state.Money);
            if (quantity <= 0) return TradeResult.Fail("invalid_quantity", "出售数量必须大于零。", state.Money);
            if (!D3InventorySystem.TryRemoveCargo(state.Inventory, commodityId, quantity, out string cargoError))
                return TradeResult.Fail(cargoError, "货舱中没有足够的货物。", state.Money);

            int total = checked(quote.SellUnitPrice * quantity);
            state.Money += total;
            state.Market.Stock.TryGetValue(commodityId, out int currentStock);
            state.Market.Stock[commodityId] = Math.Max(0, currentStock) + quantity;
            return Success(quantity, quote.SellUnitPrice, total, state.Money, "出售完成。");
        }

        private static TradeResult Success(int quantity, int unitPrice, int total, int money, string message)
        {
            return new TradeResult
            {
                Succeeded = true,
                Code = "ok",
                Message = message,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = total,
                MoneyAfter = money,
            };
        }

        private static float SupplyCoefficient(int stock)
        {
            return Clamp(1f + (TargetStock - Math.Max(0, stock)) / 100f, 0.5f, 1.8f);
        }

        private static int ToPrice(float raw)
        {
            if (float.IsNaN(raw) || float.IsInfinity(raw)) return 1;
            return Math.Max(1, (int)Math.Round(raw, MidpointRounding.AwayFromZero));
        }

        private static float Clamp(float value, float min, float max)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 1f : Math.Max(min, Math.Min(max, value));
        }
    }
}
