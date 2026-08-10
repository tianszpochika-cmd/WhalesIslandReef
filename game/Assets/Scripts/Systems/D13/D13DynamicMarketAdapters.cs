// D13-A07 对既有 D3 Economy/Inventory 的最小适配器。
// 适配器调用所属系统的公开入口，不在 D13 再造价格、市场或货舱写逻辑。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13D3InventoryAuthority : ID13MarketInventoryAuthority
    {
        private readonly D3InventoryState _inventory;
        private readonly EventLog _eventLog;

        public D13D3InventoryAuthority(D3InventoryState inventory, EventLog eventLog)
        {
            _inventory = inventory;
            _eventLog = eventLog ?? new EventLog();
            D13DynamicMarketCatalog.RegisterInventoryDefinitions();
        }

        public D13MarketInventoryDecision Apply(D13MarketInventoryMutationRequest request)
        {
            if (request == null || _inventory == null || string.IsNullOrWhiteSpace(request.ItemId) || request.Quantity <= 0)
                return new D13MarketInventoryDecision { Allowed = false, Code = "inventory_request_invalid" };
            string error;
            bool applied = request.Add
                ? D3InventorySystem.TryAddCargo(_inventory, request.ItemId, request.Quantity, out error)
                : D3InventorySystem.TryRemoveCargo(_inventory, request.ItemId, request.Quantity, out error);
            if (!applied) return new D13MarketInventoryDecision { Allowed = false, Code = error ?? "inventory_rejected", Message = "D3InventorySystem 拒绝货舱数量变化。" };
            string sourceEventId = "event_d13_inventory_" + Safe(request.OperationId);
            var fact = _eventLog.Append(new WorldEvent(sourceEventId, Math.Max(0, request.WorldDay), string.IsNullOrWhiteSpace(request.HolderId) ? "inventory_d13_a07" : request.HolderId, D13MarketTransportIds.InventoryOwner,
                new EventResult { ResultCode = request.Add ? "inventory_added" : "inventory_removed", Summary = "D3InventorySystem 已写入货舱数量。", FactIds = new List<string> { "inventory:" + request.ItemId }, Values = new Dictionary<string, string> { ["item_id"] = request.ItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture), ["add"] = request.Add.ToString() } },
                new[] { D13MarketTransportIds.PlayerRef }, new[] { "inventory:d13_a07" }, new string[0], Math.Max(0L, request.SaveRevision))).Fact;
            return new D13MarketInventoryDecision { Allowed = true, Code = "inventory_applied", ItemId = request.ItemId, Quantity = request.Quantity, SourceEventId = fact.EventId, SourceOwnerSystem = D13MarketTransportIds.InventoryOwner };
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "anonymous";
            return value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        }
    }

    public sealed class D13TradeEconomyAuthority : ID13MarketEconomyAuthority
    {
        private readonly TradeEconomySystem _economy;
        private readonly D3MarketState _market;
        private readonly EventLog _eventLog;

        public D13TradeEconomyAuthority(TradeEconomySystem economy, D3MarketState market, EventLog eventLog = null)
        {
            _economy = economy ?? throw new ArgumentNullException("economy");
            _market = market ?? throw new ArgumentNullException("market");
            _eventLog = eventLog ?? economy.EventLog;
            foreach (var good in D13DynamicMarketCatalog.Goods) Register(good);
        }

        public D13MarketQuoteDecision Quote(D13MarketGoodDefinition definition, D13MarketQuoteRequest request)
        {
            var quote = _economy.Quote(request.MarketId, definition.ItemId, _market, new TradePriceContext { WorldDay = request.WorldDay, Weather = request.Weather, FactionPolicyId = request.FactionPolicyId, FactionPolicyCoefficient = request.FactionPolicyCoefficient });
            if (quote == null) return new D13MarketQuoteDecision { Allowed = false, Code = "market_quote_missing", Message = "D3 EconomySystem 无法提供该市场报价。" };
            int unitPrice = quote.SellUnitPrice;
            string quoteEventId = "event_d13_economy_quote_" + Safe(request.OperationId);
            string marketEventId = "event_d13_market_quote_" + Safe(request.OperationId);
            AppendSource(quoteEventId, "economy_quote", request, quote, unitPrice);
            AppendSource(marketEventId, "market_quote", request, quote, unitPrice);
            var result = new D13MarketQuoteDecision { Allowed = true, Code = "quote_ready", Message = "报价由 TradeEconomySystem 生成。", MarketId = request.MarketId, ItemId = definition.ItemId, UnitPrice = unitPrice, Currency = D13MarketTransportIds.Currency, SourceEventId = quoteEventId, MarketSourceEventId = marketEventId };
            foreach (var reason in quote.PriceReasons) if (reason != null) result.Reasons.Add(reason.Clone());
            return result;
        }

        public D13MarketFlowDecision ApplyFlow(D13MarketGoodDefinition definition, D13MarketFlowRequest request)
        {
            if (request == null || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.MarketId)) return new D13MarketFlowDecision { Allowed = false, Code = "market_flow_request_invalid", Message = "市场流请求无效。" };
            var quote = _economy.Quote(request.MarketId, definition.ItemId, _market, new TradePriceContext { WorldDay = request.WorldDay, Weather = request.Weather, FactionPolicyId = request.FactionPolicyId, FactionPolicyCoefficient = request.FactionPolicyCoefficient });
            if (quote == null) return new D13MarketFlowDecision { Allowed = false, Code = "market_quote_missing", Message = "EconomySystem 无法提供市场价格。" };
            bool supplyAdded = request.Kind == D13MarketFlowKind.Production || request.Kind == D13MarketFlowKind.Sale;
            if (!_economy.ApplyExternalMarketFlow(_market, definition.ItemId, request.Quantity, supplyAdded, out string error)) return new D13MarketFlowDecision { Allowed = false, Code = error ?? "market_state_rejected", Message = "EconomySystem 拒绝市场库存/需求变化。" };
            int unitPrice = quote.SellUnitPrice;
            long total = request.Kind == D13MarketFlowKind.Sale ? (long)unitPrice * request.Quantity : 0L;
            string sourceEventId = "event_d13_economy_flow_" + Safe(request.OperationId);
            string marketEventId = "event_d13_market_state_" + Safe(request.OperationId);
            AppendSource(sourceEventId, "economy_flow", request, quote, unitPrice);
            AppendSource(marketEventId, "market_state", request, quote, unitPrice);
            return new D13MarketFlowDecision { Allowed = true, Code = "market_flow_applied", Message = "EconomySystem 已写市场库存/需求；D13 只保存来源。", Kind = request.Kind, MarketId = request.MarketId, ItemId = definition.ItemId, Quantity = request.Quantity, UnitPrice = unitPrice, TotalValue = total, Currency = D13MarketTransportIds.Currency, LedgerRef = request.Kind == D13MarketFlowKind.Sale ? "ledger_d13_market_" + Safe(request.OperationId) : null, SourceEventId = sourceEventId, MarketSourceEventId = marketEventId, SourceOwnerSystem = D13MarketTransportIds.EconomyOwner, MarketStockAfter = _market.Stock[definition.ItemId], MarketDemandAfter = _market.Demand[definition.ItemId] };
        }

        private void Register(D13MarketGoodDefinition good)
        {
            if (good == null || CommodityCatalog.Find(good.ItemId) != null) return;
            _economy.RegisterMarketItem(new D3MarketItemDefinition(good.ItemId, good.DisplayName, good.BasePrice, good.UnitWeight, good.UnitVolume, good.DemandTarget));
            _economy.EnsureMarketItem(_market, good.ItemId, good.DemandTarget);
        }

        private void AppendSource(string eventId, string code, D13MarketOperationRequest request, MarketQuote quote, int unitPrice)
        {
            _eventLog.Append(new WorldEvent(eventId, Math.Max(0, request.WorldDay), request is D13MarketQuoteRequest q && !string.IsNullOrWhiteSpace(q.MarketId) ? q.MarketId : "market_d13_a07", D13MarketTransportIds.EconomyOwner,
                new EventResult { ResultCode = code, Summary = "EconomySystem 提供了价格或市场状态来源。", FactIds = new List<string> { "economy:d13_a07:" + eventId }, Values = new Dictionary<string, string> { ["item_id"] = quote.CommodityId, ["unit_price"] = unitPrice.ToString(CultureInfo.InvariantCulture), ["stock"] = quote.Stock.ToString(CultureInfo.InvariantCulture), ["demand"] = quote.Demand.ToString(CultureInfo.InvariantCulture) } },
                new[] { D13MarketTransportIds.PlayerRef }, new[] { "economy:d13_a07" }, new string[0], Math.Max(0L, request.SaveRevision)));
        }

        private static string Safe(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "anonymous";
            return value.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        }
    }
}
