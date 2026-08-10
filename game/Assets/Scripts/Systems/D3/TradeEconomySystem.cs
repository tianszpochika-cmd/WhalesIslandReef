// ============================================================
// TradeEconomySystem —— D9 十商品价格、税费、库存与交易事实裁决
//
// EconomySystem 是价格、需求、税费、市场库存和玩家交易量的唯一写者。
// UI 只能读取 MarketQuote.PriceReasons；货舱数量委托 D3InventorySystem。
// 交易完成后追加 D8 FactEvent，EventLog 不执行任何领域数值写入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D3
{
    public sealed class TradeEconomySystem
    {
        public const string TradeSettledEventId = "event_d9_trade_settled";

        private const int TargetStock = 50;
        private const float SellBaseRate = 0.75f;
        private const float MinimumFactor = 0.5f;
        private const float MaximumFactor = 1.8f;

        private EventLog _eventLog;
        private long _nextTradeRevision;
        private readonly Dictionary<string, D3MarketItemDefinition> _additionalItems =
            new Dictionary<string, D3MarketItemDefinition>(StringComparer.Ordinal);

        public TradeEconomySystem(EventLog eventLog = null)
        {
            _eventLog = eventLog ?? new EventLog();
            _nextTradeRevision = 0;
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long CurrentSaveRevision { get { return _nextTradeRevision; } }

        /// <summary>登记特殊市场货物；不改变十种基础商品目录。</summary>
        public void RegisterMarketItem(D3MarketItemDefinition definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.Id)) return;
            _additionalItems[definition.Id] = definition;
            D3InventorySystem.RegisterCargoDefinition(definition.Id, definition.UnitWeight, definition.CargoVolume);
        }

        /// <summary>经济系统唯一初始化/补齐特殊货物的市场状态。</summary>
        public void EnsureMarketItem(D3MarketState market, string itemId, int initialStock = 50)
        {
            if (market == null || string.IsNullOrEmpty(itemId)) return;
            if (!_additionalItems.ContainsKey(itemId)) return;
            bool hadStock = market.Stock != null && market.Stock.ContainsKey(itemId);
            EnsureMarketMaps(market, itemId);
            if (!hadStock || market.Stock[itemId] < 0)
                market.Stock[itemId] = Math.Max(0, initialStock);
        }

        /// <summary>经济系统唯一的通用扣款入口；港口服务不能直接修改 Money。</summary>
        public bool TrySpend(D3TradeState state, int amount, out string error)
        {
            error = null;
            if (state == null || amount < 0)
            {
                error = "invalid_amount";
                return false;
            }
            if (amount > state.Money)
            {
                error = "not_enough_money";
                return false;
            }
            state.Money -= amount;
            return true;
        }

        public bool CanSpend(D3TradeState state, int amount)
        {
            return state != null && amount >= 0 && amount <= state.Money;
        }

        /// <summary>
        /// D13 生产/消费/销售的市场供给接口。EconomySystem 仍是市场库存、需求和交易量的唯一写者；
        /// 调用方只提交已由所属系统裁决的商品流，不直接修改 D3MarketState。
        /// </summary>
        public bool ApplyExternalMarketFlow(D3MarketState market, string commodityId, int quantity, bool supplyAdded, out string error)
        {
            error = null;
            if (market == null || string.IsNullOrWhiteSpace(commodityId) || quantity <= 0)
            {
                error = "invalid_market_flow";
                return false;
            }
            var commodity = CommodityCatalog.Find(commodityId);
            if (commodity == null && !_additionalItems.ContainsKey(commodityId))
            {
                error = "market_item_missing";
                return false;
            }
            EnsureMarketMaps(market, commodityId);
            int currentStock = Math.Max(0, market.Stock[commodityId]);
            if (!supplyAdded && currentStock < quantity)
            {
                error = "market_empty";
                return false;
            }
            market.Stock[commodityId] = supplyAdded ? checked(currentStock + quantity) : currentStock - quantity;
            market.Demand[commodityId] = Math.Max(0, market.Demand[commodityId] + (supplyAdded ? -quantity : quantity));
            market.PlayerTradeVolume[commodityId] = Math.Max(0, market.PlayerTradeVolume[commodityId] + quantity);
            return true;
        }

        public MarketQuote Quote(string portId, string commodityId, D3MarketState market, float eventCoefficient = 1f)
        {
            var context = ContextFromMarket(market);
            return BuildQuote(portId, commodityId, market, context, eventCoefficient);
        }

        public MarketQuote Quote(string portId, string commodityId, D3MarketState market,
            TradePriceContext context)
        {
            return BuildQuote(portId, commodityId, market, context ?? ContextFromMarket(market), 1f);
        }

        public MarketQuote Quote(string portId, string commodityId, D3MarketState market,
            D3WeatherType weather, string factionPolicyId = "neutral", float factionPolicyCoefficient = 1f)
        {
            return BuildQuote(portId, commodityId, market, new TradePriceContext
            {
                Weather = weather,
                FactionPolicyId = factionPolicyId,
                FactionPolicyCoefficient = factionPolicyCoefficient,
            }, 1f);
        }

        public TradeResult Buy(D3TradeState state, string portId, string commodityId, int quantity,
            float eventCoefficient = 1f)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Buy,
                ContextFromMarket(state == null ? null : state.Market), eventCoefficient, 0L);
        }

        public TradeResult Buy(D3TradeState state, string portId, string commodityId, int quantity,
            TradePriceContext context)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Buy,
                context, 1f, 0L);
        }

        /// <summary>
        /// 带明确修订号的幂等买入入口；重放同一 event_id + save_revision 不会再次改状态。
        /// </summary>
        public TradeResult Buy(D3TradeState state, string portId, string commodityId, int quantity,
            TradePriceContext context, long saveRevision)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Buy,
                context, 1f, saveRevision);
        }

        public TradeResult Sell(D3TradeState state, string portId, string commodityId, int quantity,
            float eventCoefficient = 1f)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Sell,
                ContextFromMarket(state == null ? null : state.Market), eventCoefficient, 0L);
        }

        public TradeResult Sell(D3TradeState state, string portId, string commodityId, int quantity,
            TradePriceContext context)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Sell,
                context, 1f, 0L);
        }

        /// <summary>带明确修订号的幂等卖出入口。</summary>
        public TradeResult Sell(D3TradeState state, string portId, string commodityId, int quantity,
            TradePriceContext context, long saveRevision)
        {
            return Execute(state, portId, commodityId, quantity, D3MarketAction.Sell,
                context, 1f, saveRevision);
        }

        public EventLogSnapshot CaptureEventLogSnapshot()
        {
            return _eventLog.CaptureSnapshot();
        }

        public void RestoreEventLog(EventLogSnapshot snapshot)
        {
            _eventLog.Restore(snapshot ?? new EventLogSnapshot());
            _nextTradeRevision = 0;
            foreach (var fact in _eventLog.Facts)
                if (fact != null) _nextTradeRevision = Math.Max(_nextTradeRevision, fact.SaveRevision);
        }

        private TradeResult Execute(D3TradeState state, string portId, string commodityId, int quantity,
            D3MarketAction action, TradePriceContext context, float eventCoefficient, long explicitRevision)
        {
            if (state == null) return TradeResult.Fail("invalid_state", "贸易状态不存在。", 0);
            var quote = BuildQuote(portId, commodityId, state.Market,
                context ?? ContextFromMarket(state.Market), eventCoefficient);
            if (quote == null)
                return TradeResult.Fail("invalid_reference", "港口或货物不存在。", state.Money);
            if (quantity <= 0)
                return TradeResult.Fail("invalid_quantity", "交易数量必须大于零。", state.Money);

            long revision = explicitRevision > 0 ? explicitRevision : _nextTradeRevision + 1;
            if (revision < 0) return TradeResult.Fail("invalid_revision", "交易修订号无效。", state.Money);
            var duplicate = explicitRevision > 0 ? _eventLog.Find(TradeSettledEventId, revision) : null;
            if (duplicate != null)
                return DuplicateResult(duplicate, state.Money, quote);

            int stock = quote.Stock;
            int totalPrice;
            int taxAmount;
            if (action == D3MarketAction.Buy)
            {
                if (stock < quantity)
                    return FailWithQuote("market_empty", "港口库存不足，当前商品缺货。", state.Money, quote);
                long totalLong = (long)quote.BuyUnitPrice * quantity;
                if (totalLong > int.MaxValue || totalLong > state.Money)
                    return FailWithQuote("not_enough_money", "金币不足。", state.Money, quote);
                if (!D3InventorySystem.CanAddCargo(state.Inventory, commodityId, quantity, out string cargoError))
                    return FailWithQuote(cargoError, cargoError == "weight_full" ? "货物重量超出限额。" :
                        cargoError == "cargo_full" ? "货舱体积不足。" : "无法装入货物。", state.Money, quote);

                totalPrice = (int)totalLong;
                taxAmount = TaxForBuy(quote, quantity);
                if (!D3InventorySystem.TryAddCargo(state.Inventory, commodityId, quantity, out cargoError))
                    return FailWithQuote(cargoError, "无法装入货物。", state.Money, quote);
                state.Money -= totalPrice;
                state.Market.Stock[commodityId] = stock - quantity;
                AdjustDemandAndVolume(state.Market, commodityId, quantity, true);
            }
            else
            {
                if (!D3InventorySystem.TryRemoveCargo(state.Inventory, commodityId, quantity, out string cargoError))
                    return FailWithQuote(cargoError, "货舱中没有足够的货物。", state.Money, quote);
                long totalLong = (long)quote.SellUnitPrice * quantity;
                if (totalLong > int.MaxValue)
                    return FailWithQuote("invalid_quantity", "出售金额超出安全范围。", state.Money, quote);
                totalPrice = (int)totalLong;
                taxAmount = TaxForSell(quote, quantity);
                state.Money += totalPrice;
                state.Market.Stock[commodityId] = stock + quantity;
                AdjustDemandAndVolume(state.Market, commodityId, quantity, false);
            }

            if (explicitRevision <= 0) _nextTradeRevision = revision;
            else _nextTradeRevision = Math.Max(_nextTradeRevision, revision);

            var result = Success(action, commodityId, quantity, action == D3MarketAction.Buy
                ? quote.BuyUnitPrice : quote.SellUnitPrice, totalPrice, taxAmount, state.Money, quote,
                revision);
            result.WeightAfter = D3InventorySystem.UsedWeight(state.Inventory);
            result.VolumeAfter = D3InventorySystem.UsedVolume(state.Inventory);
            var append = _eventLog.Append(CreateTradeEvent(portId, action, result,
                context ?? ContextFromMarket(state.Market)));
            result.FactLogged = append.Succeeded;
            result.DuplicateSettlement = append.IsDuplicate;
            return result;
        }

        private WorldEvent CreateTradeEvent(string portId, D3MarketAction action, TradeResult result,
            TradePriceContext context)
        {
            var values = new Dictionary<string, string>
            {
                ["action"] = action.ToString().ToLowerInvariant(),
                ["commodity_id"] = result.CommodityId ?? string.Empty,
                ["quantity"] = result.Quantity.ToString(CultureInfo.InvariantCulture),
                ["unit_price"] = result.UnitPrice.ToString(CultureInfo.InvariantCulture),
                ["total_price"] = result.TotalPrice.ToString(CultureInfo.InvariantCulture),
                ["tax_amount"] = result.TaxAmount.ToString(CultureInfo.InvariantCulture),
            };
            // commodity_id is supplied separately in the result event facts below; keeping values opaque
            // ensures the EventLog still has no knowledge of Economy-owned fields.
            values["trade_result"] = result.Code;
            return new WorldEvent(
                TradeSettledEventId,
                context == null ? 0 : Math.Max(0, context.WorldDay),
                portId,
                "EconomySystem",
                new EventResult
                {
                    ResultCode = result.Code,
                    Summary = result.Message,
                    FactIds = new List<string> { "trade:" + result.SaveRevision.ToString(CultureInfo.InvariantCulture) },
                    Values = values,
                },
                actorIds: new[] { "player" },
                inputFacts: new[] { "commodity:" + result.CommodityId },
                causeIds: new string[0],
                saveRevision: result.SaveRevision);
        }

        private void AdjustDemandAndVolume(D3MarketState market, string commodityId, int quantity, bool bought)
        {
            EnsureMarketMaps(market, commodityId);
            int demand = market.Demand[commodityId];
            market.Demand[commodityId] = Math.Max(0, demand + (bought ? quantity : -quantity));
            market.PlayerTradeVolume[commodityId] = Math.Max(0, market.PlayerTradeVolume[commodityId] + quantity);
        }

        private MarketQuote BuildQuote(string portId, string commodityId, D3MarketState market,
            TradePriceContext context, float eventCoefficient)
        {
            var port = PortCatalog.Find(portId);
            var commodity = CommodityCatalog.Find(commodityId);
            D3MarketItemDefinition additional = null;
            if (commodity == null && !_additionalItems.TryGetValue(commodityId, out additional)) return null;
            if (port == null || market == null) return null;
            EnsureMarketMaps(market, commodityId);

            int stock = Math.Max(0, market.Stock[commodityId]);
            int demand = Math.Max(0, market.Demand[commodityId]);
            int playerVolume = Math.Max(0, market.PlayerTradeVolume[commodityId]);
            float supply = SupplyCoefficient(stock);
            int basePrice = commodity == null ? additional.BasePrice : commodity.BasePrice;
            int demandTarget = commodity == null ? additional.DemandTarget : commodity.DemandTarget;
            float demandCoefficient = Clamp(1f + (demand - demandTarget) / 200f, 0.7f, 1.4f);
            float weatherCoefficient = WeatherCoefficient(context == null ? D3WeatherType.Calm : context.Weather);
            float eventMultiplier = Clamp(eventCoefficient, 0.5f, 1.5f);
            weatherCoefficient = Clamp(weatherCoefficient * eventMultiplier, MinimumFactor, MaximumFactor);
            float playerCoefficient = Clamp(1f + playerVolume / 1000f, 0.85f, 1.35f);
            float policyCoefficient = Clamp(context == null ? 1f : context.FactionPolicyCoefficient, 0.75f, 1.35f);
            float buyTax = Clamp(port.TariffRate, 0f, 0.5f);
            float sellTax = Clamp(port.TariffRate * 0.5f, 0f, 0.25f);
            float core = basePrice * supply * demandCoefficient * weatherCoefficient *
                playerCoefficient * policyCoefficient;

            var quote = new MarketQuote
            {
                PortId = port.Id,
                CommodityId = commodity == null ? additional.Id : commodity.Id,
                BuyUnitPrice = ToPrice(core * (1f + buyTax)),
                SellUnitPrice = ToPrice(core * (1f - sellTax) * SellBaseRate),
                Stock = stock,
                Demand = demand,
                PlayerTradeVolume = playerVolume,
                SupplyCoefficient = supply,
                DemandCoefficient = demandCoefficient,
                WeatherCoefficient = weatherCoefficient,
                PlayerTradeVolumeCoefficient = playerCoefficient,
                FactionPolicyCoefficient = policyCoefficient,
                TariffCoefficient = 1f + buyTax,
                EventCoefficient = eventMultiplier,
                BuyTaxRate = buyTax,
                SellTaxRate = sellTax,
            };
            quote.PriceReasons.Add(Reason(PriceReasonKind.Inventory, "inventory",
                "库存 " + stock + " 件，库存因子 " + Format(supply), supply));
            quote.PriceReasons.Add(Reason(PriceReasonKind.Demand, "demand",
                "需求 " + demand + "，需求因子 " + Format(demandCoefficient), demandCoefficient));
            quote.PriceReasons.Add(Reason(PriceReasonKind.Weather, "weather",
                "天气 " + (context == null ? D3WeatherType.Calm : context.Weather) + "，天气因子 " + Format(weatherCoefficient), weatherCoefficient));
            quote.PriceReasons.Add(Reason(PriceReasonKind.PlayerTradeVolume, "player_trade_volume",
                "玩家累计交易量 " + playerVolume + " 件，交易量因子 " + Format(playerCoefficient), playerCoefficient));
            quote.PriceReasons.Add(Reason(PriceReasonKind.FactionPolicy, "faction_policy",
                "势力政策 " + (context == null ? "neutral" : context.FactionPolicyId) + "，政策因子 " + Format(policyCoefficient), policyCoefficient));
            quote.PriceReasons.Add(Reason(PriceReasonKind.Tax, "tax",
                "港口税费 买入 " + (buyTax * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "% / 卖出 " +
                (sellTax * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%", 1f + buyTax));
            return quote;
        }

        private static TradePriceContext ContextFromMarket(D3MarketState market)
        {
            return new TradePriceContext
            {
                WorldDay = market == null ? 0 : Math.Max(0, market.WorldDay),
                Weather = market == null ? D3WeatherType.Calm : market.Weather,
                FactionPolicyId = market == null || string.IsNullOrEmpty(market.FactionPolicyId)
                    ? "neutral" : market.FactionPolicyId,
                FactionPolicyCoefficient = market == null ? 1f : market.FactionPolicyCoefficient,
            };
        }

        private void EnsureMarketMaps(D3MarketState market, string commodityId)
        {
            if (market.Stock == null) market.Stock = new Dictionary<string, int>();
            if (market.Demand == null) market.Demand = new Dictionary<string, int>();
            if (market.PlayerTradeVolume == null) market.PlayerTradeVolume = new Dictionary<string, int>();
            if (!market.Stock.ContainsKey(commodityId)) market.Stock[commodityId] = 0;
            var commodity = CommodityCatalog.Find(commodityId);
            D3MarketItemDefinition additional = null;
            int demandTarget = commodity == null && _additionalItems.TryGetValue(commodityId, out additional)
                ? additional.DemandTarget : commodity == null ? 0 : commodity.DemandTarget;
            if (!market.Demand.ContainsKey(commodityId)) market.Demand[commodityId] = demandTarget;
            if (!market.PlayerTradeVolume.ContainsKey(commodityId)) market.PlayerTradeVolume[commodityId] = 0;
        }

        private static TradeResult FailWithQuote(string code, string message, int money, MarketQuote quote)
        {
            var result = TradeResult.Fail(code, message, money);
            CopyReasons(result, quote);
            return result;
        }

        private static TradeResult Success(D3MarketAction action, string commodityId, int quantity,
            int unitPrice, int total, int tax, int money, MarketQuote quote, long revision)
        {
            var result = new TradeResult
            {
                Succeeded = true,
                Code = "ok",
                Message = action == D3MarketAction.Buy ? "购买完成。" : "出售完成。",
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = total,
                MoneyAfter = money,
                TaxAmount = tax,
                WeightAfter = 0,
                VolumeAfter = 0,
                EventId = TradeEconomySystem.TradeSettledEventId,
                CommodityId = commodityId,
                SaveRevision = revision,
            };
            CopyReasons(result, quote);
            return result;
        }

        private static TradeResult DuplicateResult(FactEvent fact, int money, MarketQuote quote)
        {
            var result = TradeResult.Fail("duplicate_settlement", "该交易事实已结算，未重复修改状态。", money);
            result.Succeeded = true;
            result.DuplicateSettlement = true;
            result.FactLogged = true;
            result.EventId = fact.EventId;
            if (fact.Result != null && fact.Result.Values != null &&
                fact.Result.Values.TryGetValue("commodity_id", out string commodityId))
                result.CommodityId = commodityId;
            result.SaveRevision = fact.SaveRevision;
            CopyReasons(result, quote);
            return result;
        }

        private static void CopyReasons(TradeResult result, MarketQuote quote)
        {
            if (result == null || quote == null || quote.PriceReasons == null) return;
            foreach (var reason in quote.PriceReasons)
                if (reason != null) result.PriceReasons.Add(reason.Clone());
        }

        private static int TaxForBuy(MarketQuote quote, int quantity)
        {
            if (quote == null) return 0;
            return Math.Max(0, (int)Math.Round(quote.BuyUnitPrice * quote.BuyTaxRate / (1f + quote.BuyTaxRate), MidpointRounding.AwayFromZero)) * quantity;
        }

        private static int TaxForSell(MarketQuote quote, int quantity)
        {
            if (quote == null) return 0;
            return Math.Max(0, (int)Math.Round(quote.SellUnitPrice * quote.SellTaxRate, MidpointRounding.AwayFromZero)) * quantity;
        }

        private static PriceReason Reason(PriceReasonKind kind, string code, string explanation, float coefficient)
        {
            return new PriceReason { Kind = kind, Code = code, Explanation = explanation, Coefficient = coefficient };
        }

        private static float WeatherCoefficient(D3WeatherType weather)
        {
            switch (weather)
            {
                case D3WeatherType.TradeWind: return 0.98f;
                case D3WeatherType.Rain: return 1.04f;
                case D3WeatherType.Storm: return 1.12f;
                case D3WeatherType.Hurricane: return 1.18f;
                default: return 1f;
            }
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

        private static string Format(float value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static float Clamp(float value, float min, float max)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? 1f : Math.Max(min, Math.Min(max, value));
        }
    }
}
