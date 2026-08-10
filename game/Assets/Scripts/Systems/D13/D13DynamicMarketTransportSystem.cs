// D13-A07 动态商品流与运输系统。
// 本类写商品流记录、运输状态和运输损耗；价格/市场与库存必须经 authority。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13DynamicMarketTransportSystem
    {
        private readonly Dictionary<string, D13MarketGoodDefinition> _goods = new Dictionary<string, D13MarketGoodDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13MarketRouteDefinition> _routes = new Dictionary<string, D13MarketRouteDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13MarketFlowRecord> _flows = new Dictionary<string, D13MarketFlowRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13TransportShipmentState> _shipments = new Dictionary<string, D13TransportShipmentState>(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly ID13MarketLegalAuthority _legal;
        private readonly ID13MarketProductionAuthority _production;
        private readonly ID13MarketConsumptionAuthority _consumption;
        private readonly ID13MarketInventoryAuthority _inventory;
        private readonly ID13MarketEconomyAuthority _economy;
        private readonly ID13TransportAssetAuthority _transportAsset;
        private readonly EventLog _eventLog;
        private long _revision;

        public D13DynamicMarketTransportSystem(
            ID13MarketLegalAuthority legal = null,
            ID13MarketProductionAuthority production = null,
            ID13MarketConsumptionAuthority consumption = null,
            ID13MarketInventoryAuthority inventory = null,
            ID13MarketEconomyAuthority economy = null,
            ID13TransportAssetAuthority transportAsset = null,
            EventLog eventLog = null,
            D13DynamicMarketRuntimeSnapshot initial = null,
            IEnumerable<D13MarketGoodDefinition> goods = null,
            IEnumerable<D13MarketRouteDefinition> routes = null)
        {
            _legal = legal ?? new UnavailableLegalAuthority();
            _production = production ?? new UnavailableProductionAuthority();
            _consumption = consumption ?? new UnavailableConsumptionAuthority();
            _inventory = inventory ?? new UnavailableInventoryAuthority();
            _economy = economy ?? new UnavailableEconomyAuthority();
            _transportAsset = transportAsset ?? new UnavailableTransportAssetAuthority();
            _eventLog = eventLog ?? new EventLog();
            D13DynamicMarketCatalog.RegisterInventoryDefinitions();
            foreach (var source in goods ?? D13DynamicMarketCatalog.Goods)
            {
                if (source == null) continue;
                source.Validate();
                if (_goods.ContainsKey(source.ItemId)) throw new InvalidOperationException("Duplicate D13 market good: " + source.ItemId);
                _goods[source.ItemId] = source.Clone();
            }
            foreach (var source in routes ?? D13DynamicMarketCatalog.Routes)
            {
                if (source == null) continue;
                source.Validate();
                if (_routes.ContainsKey(source.RouteId)) throw new InvalidOperationException("Duplicate D13 market route: " + source.RouteId);
                _routes[source.RouteId] = source.Clone();
            }
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13MarketGoodDefinition FindGood(string itemId)
        {
            return _goods.TryGetValue(itemId ?? string.Empty, out D13MarketGoodDefinition result) ? result.Clone() : null;
        }

        public D13MarketRouteDefinition FindRoute(string routeId)
        {
            return _routes.TryGetValue(routeId ?? string.Empty, out D13MarketRouteDefinition result) ? result.Clone() : null;
        }

        public D13MarketFlowRecord FindFlow(string flowId)
        {
            return _flows.TryGetValue(flowId ?? string.Empty, out D13MarketFlowRecord result) ? result.Clone() : null;
        }

        public D13TransportShipmentState FindShipment(string shipmentId)
        {
            return _shipments.TryGetValue(shipmentId ?? string.Empty, out D13TransportShipmentState result) ? result.Clone() : null;
        }

        public D13DynamicMarketReadModel GetReadModel()
        {
            var result = new D13DynamicMarketReadModel { Revision = _revision };
            foreach (var item in _goods.Values.OrderBy(item => item.ItemId, StringComparer.Ordinal)) result.Goods.Add(item.Clone());
            foreach (var item in _routes.Values.OrderBy(item => item.RouteId, StringComparer.Ordinal)) result.Routes.Add(item.Clone());
            foreach (var item in _flows.Values.OrderBy(item => item.FlowId, StringComparer.Ordinal)) result.Flows.Add(item.Clone());
            foreach (var item in _shipments.Values.OrderBy(item => item.ShipmentId, StringComparer.Ordinal)) result.Shipments.Add(item.Clone());
            return result;
        }

        public D13DynamicMarketRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13DynamicMarketRuntimeSnapshot { Revision = _revision };
            foreach (var item in _flows.Values) result.Flows.Add(item.Clone());
            foreach (var item in _shipments.Values) result.Shipments.Add(item.Clone());
            foreach (var key in _appliedEventKeys) result.AppliedEventKeys.Add(key);
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_market_", StringComparison.Ordinal) && !result.AppliedEventKeys.Contains(fact.DedupeKey)) result.AppliedEventKeys.Add(fact.DedupeKey);
            result.Validate();
            return result;
        }

        public void Restore(D13DynamicMarketRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var item in snapshot.Flows)
                if (!_goods.ContainsKey(item.ItemId)) throw new InvalidOperationException("Unknown good in D13 market snapshot: " + item.ItemId);
            foreach (var item in snapshot.Shipments)
                if (!_goods.ContainsKey(item.ItemId) || !_routes.ContainsKey(item.RouteId)) throw new InvalidOperationException("Unknown shipment reference in D13 market snapshot: " + item.ShipmentId);
            _flows.Clear(); foreach (var item in snapshot.Flows) _flows[item.FlowId] = item.Clone();
            _shipments.Clear(); foreach (var item in snapshot.Shipments) _shipments[item.ShipmentId] = item.Clone();
            _appliedEventKeys.Clear(); foreach (var key in snapshot.AppliedEventKeys) _appliedEventKeys.Add(key);
            _revision = snapshot.Revision;
        }

        public D13MarketResult Quote(D13MarketQuoteRequest request)
        {
            if (request == null) return Invalid("invalid_request", "市场报价请求不存在。", D13MarketTransportIds.MarketRecoveryRoute);
            var definition = FindGood(request.ItemId);
            if (definition == null || string.IsNullOrWhiteSpace(request.MarketId)) return Invalid("good_or_market_missing", "商品或市场不存在。", D13MarketTransportIds.MarketRecoveryRoute);
            var decision = _economy.Quote(definition, request);
            if (!ValidQuote(decision, definition, request.MarketId)) return Failure(request, EventId(request.OperationId, "quote"), decision == null ? "economy_authority_required" : decision.Code, decision == null ? "必须由 EconomySystem 提供市场报价。" : decision.Message, D13MarketTransportIds.MarketRecoveryRoute);
            return new D13MarketResult { Succeeded = true, Code = "quote_ready", Message = "报价来自 EconomySystem，UI 只能读取价格原因。", Quote = decision, EventId = decision.SourceEventId };
        }

        public D13MarketResult Produce(D13MarketProductionRequest request)
        {
            if (request == null) return Invalid("invalid_request", "生产请求不存在。", D13MarketTransportIds.ProductionRecoveryRoute);
            string eventId = EventId(request.OperationId, "production_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, null);
            if (request.RequestedQuantity <= 0) return Failure(request, eventId, "quantity_invalid", "生产数量必须大于零。", D13MarketTransportIds.ProductionRecoveryRoute);
            D13MarketGoodDefinition definition;
            if (!_goods.TryGetValue(request.ItemId ?? string.Empty, out definition)) return Failure(request, eventId, "good_missing", "商品定义不存在。", D13MarketTransportIds.ProductionRecoveryRoute);
            var legal = _legal.ValidateGood(definition.Clone(), request.SourceLocationId, request.PermitId, request);
            if (!ValidLegal(legal, definition, request.PermitId)) return Failure(request, eventId, legal == null ? "legal_authority_required" : legal.Code, legal == null ? "生产必须经过法律/势力 authority。" : legal.Message, D13MarketTransportIds.LegalRecoveryRoute);
            var source = _production.ValidateProduction(definition.Clone(), request);
            if (source == null || !source.Allowed || source.Quantity <= 0 || source.Quantity > request.RequestedQuantity || !definition.AllowedProducerOwners.Contains(source.SourceOwnerSystem) || !SourceExists(source.SourceEventId) || !OptionalSourceExists(source.EcologySourceEventId, source.AssetSourceEventId))
                return Failure(request, eventId, source == null ? "production_authority_required" : source.Code, source == null ? "生产必须由农场、采集、养殖或建筑 owner 提供来源。" : source.Message, D13MarketTransportIds.ProductionRecoveryRoute);
            var inventory = ApplyInventory(eventId, request.InventoryHolderId, definition.ItemId, source.Quantity, true, request.WorldDay, request.SaveRevision);
            if (!ValidInventory(inventory, definition.ItemId, source.Quantity)) return Failure(request, eventId, inventory == null ? "inventory_authority_required" : inventory.Code, inventory == null ? "生产产物必须由 Inventory/Cargo 写入。" : inventory.Message, D13MarketTransportIds.InventoryRecoveryRoute);
            var market = _economy.ApplyFlow(definition.Clone(), new D13MarketFlowRequest { OperationId = request.OperationId + "_market", Kind = D13MarketFlowKind.Production, ItemId = definition.ItemId, Quantity = source.Quantity, MarketId = request.MarketId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, CauseEventIds = request.CauseEventIds });
            if (!ValidMarketFlow(market, D13MarketFlowKind.Production, definition.ItemId, source.Quantity))
            {
                ApplyInventory(eventId + "_rollback", request.InventoryHolderId, definition.ItemId, source.Quantity, false, request.WorldDay, request.SaveRevision);
                return Failure(request, eventId, market == null ? "economy_authority_required" : market.Code, market == null ? "生产进入市场必须由 EconomySystem 写入市场状态。" : market.Message, D13MarketTransportIds.MarketRecoveryRoute);
            }
            var flow = new D13MarketFlowRecord { FlowId = eventId, Kind = D13MarketFlowKind.Production, ItemId = definition.ItemId, Quantity = source.Quantity, OriginLocationId = request.SourceLocationId, MarketId = request.MarketId, UnitPrice = market.UnitPrice, TotalValue = 0, Currency = market.Currency, ProducerSourceEventId = source.SourceEventId, InventorySourceEventId = inventory.SourceEventId, EconomySourceEventId = market.SourceEventId, MarketSourceEventId = market.MarketSourceEventId, LegalSourceEventId = legal.SourceEventId, EcologySourceEventId = source.EcologySourceEventId, AssetSourceEventId = source.AssetSourceEventId, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0L, request.SaveRevision) };
            flow.Validate();
            _flows[flow.FlowId] = flow;
            var fact = AppendEvent(eventId, request, "market_production_recorded", "生产商品已由来源系统和 Inventory/Cargo 写入，并进入 EconomySystem 市场供给。", new Dictionary<string, string> { ["item_id"] = flow.ItemId, ["quantity"] = flow.Quantity.ToString(CultureInfo.InvariantCulture), ["producer_source_event_id"] = flow.ProducerSourceEventId, ["inventory_source_event_id"] = flow.InventorySourceEventId, ["economy_source_event_id"] = flow.EconomySourceEventId, ["market_source_event_id"] = flow.MarketSourceEventId, ["legal_source_event_id"] = flow.LegalSourceEventId });
            _revision++;
            return Success(fact, flow, null, null);
        }

        public D13MarketResult Consume(D13MarketConsumptionRequest request)
        {
            if (request == null) return Invalid("invalid_request", "消费请求不存在。", D13MarketTransportIds.ConsumptionRecoveryRoute);
            string eventId = EventId(request.OperationId, "consumption_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, null);
            if (request.Quantity <= 0) return Failure(request, eventId, "quantity_invalid", "消费数量必须大于零。", D13MarketTransportIds.ConsumptionRecoveryRoute);
            D13MarketGoodDefinition definition;
            if (!_goods.TryGetValue(request.ItemId ?? string.Empty, out definition)) return Failure(request, eventId, "good_missing", "商品定义不存在。", D13MarketTransportIds.ConsumptionRecoveryRoute);
            var legal = _legal.ValidateGood(definition.Clone(), request.LocationId, request.PermitId, request);
            if (!ValidLegal(legal, definition, request.PermitId)) return Failure(request, eventId, legal == null ? "legal_authority_required" : legal.Code, legal == null ? "消费必须经过法律/势力 authority。" : legal.Message, D13MarketTransportIds.LegalRecoveryRoute);
            var consumer = _consumption.ValidateConsumption(definition.Clone(), request);
            if (consumer == null || !consumer.Allowed || consumer.SourceOwnerSystem != "IslandGovernanceSystem" && consumer.SourceOwnerSystem != "AnimalCareSystem" && consumer.SourceOwnerSystem != "ShipSystem" || !SourceExists(consumer.SourceEventId))
                return Failure(request, eventId, consumer == null ? "consumption_authority_required" : consumer.Code, consumer == null ? "消费结果必须由所属消费系统裁决。" : consumer.Message, D13MarketTransportIds.ConsumptionRecoveryRoute);
            var inventory = ApplyInventory(eventId, request.InventoryHolderId, definition.ItemId, request.Quantity, false, request.WorldDay, request.SaveRevision);
            if (!ValidInventory(inventory, definition.ItemId, request.Quantity)) return Failure(request, eventId, inventory == null ? "inventory_authority_required" : inventory.Code, inventory == null ? "消费物品必须由 Inventory/Cargo 扣除。" : inventory.Message, D13MarketTransportIds.InventoryRecoveryRoute);
            var market = _economy.ApplyFlow(definition.Clone(), new D13MarketFlowRequest { OperationId = request.OperationId + "_market", Kind = D13MarketFlowKind.Consumption, ItemId = definition.ItemId, Quantity = request.Quantity, MarketId = request.MarketId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, CauseEventIds = request.CauseEventIds });
            if (!ValidMarketFlow(market, D13MarketFlowKind.Consumption, definition.ItemId, request.Quantity))
            {
                ApplyInventory(eventId + "_rollback", request.InventoryHolderId, definition.ItemId, request.Quantity, true, request.WorldDay, request.SaveRevision);
                return Failure(request, eventId, market == null ? "economy_authority_required" : market.Code, market == null ? "消费需求必须由 EconomySystem 写入市场状态。" : market.Message, D13MarketTransportIds.MarketRecoveryRoute);
            }
            var flow = new D13MarketFlowRecord { FlowId = eventId, Kind = D13MarketFlowKind.Consumption, ItemId = definition.ItemId, Quantity = request.Quantity, OriginLocationId = request.LocationId, DestinationLocationId = request.ConsumerId, MarketId = request.MarketId, UnitPrice = market.UnitPrice, TotalValue = 0, Currency = market.Currency, ConsumerSourceEventId = consumer.SourceEventId, InventorySourceEventId = inventory.SourceEventId, EconomySourceEventId = market.SourceEventId, MarketSourceEventId = market.MarketSourceEventId, LegalSourceEventId = legal.SourceEventId, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0L, request.SaveRevision) };
            flow.Validate();
            _flows[flow.FlowId] = flow;
            var fact = AppendEvent(eventId, request, "market_consumption_recorded", "消费系统已裁决用途，Inventory/Cargo 已扣除，EconomySystem 已更新需求。", new Dictionary<string, string> { ["item_id"] = flow.ItemId, ["quantity"] = flow.Quantity.ToString(CultureInfo.InvariantCulture), ["consumer_source_event_id"] = flow.ConsumerSourceEventId, ["inventory_source_event_id"] = flow.InventorySourceEventId, ["economy_source_event_id"] = flow.EconomySourceEventId, ["market_source_event_id"] = flow.MarketSourceEventId, ["legal_source_event_id"] = flow.LegalSourceEventId });
            _revision++;
            return Success(fact, flow, null, null);
        }

        public D13MarketResult Sell(D13MarketSaleRequest request)
        {
            if (request == null) return Invalid("invalid_request", "销售请求不存在。", D13MarketTransportIds.MarketRecoveryRoute);
            string eventId = EventId(request.OperationId, "sale_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, null);
            if (request.Quantity <= 0) return Failure(request, eventId, "quantity_invalid", "销售数量必须大于零。", D13MarketTransportIds.MarketRecoveryRoute);
            D13MarketGoodDefinition definition;
            if (!_goods.TryGetValue(request.ItemId ?? string.Empty, out definition)) return Failure(request, eventId, "good_missing", "商品定义不存在。", D13MarketTransportIds.MarketRecoveryRoute);
            var legal = _legal.ValidateGood(definition.Clone(), request.LocationId, request.PermitId, request);
            if (!ValidLegal(legal, definition, request.PermitId)) return Failure(request, eventId, legal == null ? "legal_authority_required" : legal.Code, legal == null ? "销售必须经过法律/势力 authority。" : legal.Message, D13MarketTransportIds.LegalRecoveryRoute);
            var inventory = ApplyInventory(eventId, request.InventoryHolderId, definition.ItemId, request.Quantity, false, request.WorldDay, request.SaveRevision);
            if (!ValidInventory(inventory, definition.ItemId, request.Quantity)) return Failure(request, eventId, inventory == null ? "inventory_authority_required" : inventory.Code, inventory == null ? "销售货物必须由 Inventory/Cargo 扣除。" : inventory.Message, D13MarketTransportIds.InventoryRecoveryRoute);
            var market = _economy.ApplyFlow(definition.Clone(), new D13MarketFlowRequest { OperationId = request.OperationId + "_market", Kind = D13MarketFlowKind.Sale, ItemId = definition.ItemId, Quantity = request.Quantity, MarketId = request.MarketId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, Weather = request.Weather, FactionPolicyId = request.FactionPolicyId, FactionPolicyCoefficient = request.FactionPolicyCoefficient, CauseEventIds = request.CauseEventIds });
            if (!ValidMarketFlow(market, D13MarketFlowKind.Sale, definition.ItemId, request.Quantity) || string.IsNullOrWhiteSpace(market.LedgerRef) || market.TotalValue < 0)
            {
                ApplyInventory(eventId + "_rollback", request.InventoryHolderId, definition.ItemId, request.Quantity, true, request.WorldDay, request.SaveRevision);
                return Failure(request, eventId, market == null ? "economy_authority_required" : market.Code, market == null ? "销售价格和账本必须由 EconomySystem 裁决。" : market.Message, D13MarketTransportIds.MarketRecoveryRoute);
            }
            var flow = new D13MarketFlowRecord { FlowId = eventId, Kind = D13MarketFlowKind.Sale, ItemId = definition.ItemId, Quantity = request.Quantity, OriginLocationId = request.LocationId, DestinationLocationId = request.SellerId, MarketId = request.MarketId, UnitPrice = market.UnitPrice, TotalValue = market.TotalValue, Currency = market.Currency, LedgerRef = market.LedgerRef, InventorySourceEventId = inventory.SourceEventId, EconomySourceEventId = market.SourceEventId, MarketSourceEventId = market.MarketSourceEventId, LegalSourceEventId = legal.SourceEventId, WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0L, request.SaveRevision) };
            flow.Validate();
            _flows[flow.FlowId] = flow;
            var fact = AppendEvent(eventId, request, "market_sale_recorded", "商品已由 Inventory/Cargo 出库，销售价格、市场供给和账本由 EconomySystem 裁决。", new Dictionary<string, string> { ["item_id"] = flow.ItemId, ["quantity"] = flow.Quantity.ToString(CultureInfo.InvariantCulture), ["unit_price"] = flow.UnitPrice.ToString(CultureInfo.InvariantCulture), ["total_value"] = flow.TotalValue.ToString(CultureInfo.InvariantCulture), ["ledger_ref"] = flow.LedgerRef ?? "", ["inventory_source_event_id"] = flow.InventorySourceEventId, ["economy_source_event_id"] = flow.EconomySourceEventId, ["market_source_event_id"] = flow.MarketSourceEventId });
            _revision++;
            return Success(fact, flow, null, null);
        }

        public D13MarketResult CreateShipment(D13MarketShipmentRequest request)
        {
            if (request == null) return Invalid("invalid_request", "运输请求不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            string eventId = EventId(request.OperationId, "shipment_load_" + request.ItemId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindShipmentByEvent(duplicate.EventId));
            D13MarketGoodDefinition good;
            D13MarketRouteDefinition route;
            if (!_goods.TryGetValue(request.ItemId ?? string.Empty, out good) || !_routes.TryGetValue(request.RouteId ?? string.Empty, out route)) return Failure(request, eventId, "good_or_route_missing", "商品或运输路线不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            if (!good.Transportable || request.Quantity <= 0 || request.Quantity > route.CapacityUnits) return Failure(request, eventId, "transport_capacity_invalid", "商品不可运输或数量超过路线容量。", D13MarketTransportIds.TransportRecoveryRoute);
            var legal = _legal.ValidateRoute(route.Clone(), request.PermitId, request);
            if (!ValidLegal(legal, good, request.PermitId)) return Failure(request, eventId, legal == null ? "legal_authority_required" : legal.Code, legal == null ? "运输必须经过法律/势力 route authority。" : legal.Message, D13MarketTransportIds.LegalRecoveryRoute);
            var inventory = ApplyInventory(eventId, request.InventoryHolderId, good.ItemId, request.Quantity, false, request.WorldDay, request.SaveRevision);
            if (!ValidInventory(inventory, good.ItemId, request.Quantity)) return Failure(request, eventId, inventory == null ? "inventory_authority_required" : inventory.Code, inventory == null ? "运输装货必须由 Inventory/Cargo 扣除。" : inventory.Message, D13MarketTransportIds.InventoryRecoveryRoute);
            var shipment = new D13TransportShipmentState { ShipmentId = eventId, RouteId = route.RouteId, ItemId = good.ItemId, OriginLocationId = route.OriginLocationId, DestinationLocationId = route.DestinationLocationId, ShipId = request.ShipId, RequestedQuantity = request.Quantity, RemainingQuantity = request.Quantity, LegalSourceEventId = legal.SourceEventId, InventoryLoadSourceEventId = inventory.SourceEventId, CreatedDay = Math.Max(0, request.WorldDay), LastUpdatedDay = Math.Max(0, request.WorldDay), LastEventId = eventId, Revision = 1 };
            shipment.Validate();
            _shipments[shipment.ShipmentId] = shipment;
            var fact = AppendEvent(eventId, request, "market_shipment_loaded", "运输系统已装货；库存、法律来源和运输路线均可追踪。", new Dictionary<string, string> { ["shipment_id"] = shipment.ShipmentId, ["route_id"] = shipment.RouteId, ["item_id"] = shipment.ItemId, ["quantity"] = shipment.RequestedQuantity.ToString(CultureInfo.InvariantCulture), ["inventory_source_event_id"] = shipment.InventoryLoadSourceEventId, ["legal_source_event_id"] = shipment.LegalSourceEventId });
            _revision++;
            return Success(fact, null, shipment, null);
        }

        public D13MarketResult AdvanceShipment(D13MarketShipmentAdvanceRequest request)
        {
            if (request == null) return Invalid("invalid_request", "运输推进请求不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            string eventId = EventId(request.OperationId, "shipment_advance_" + request.ShipmentId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindShipmentByEvent(duplicate.EventId));
            D13TransportShipmentState shipment;
            D13MarketRouteDefinition route;
            D13MarketGoodDefinition good;
            if (!_shipments.TryGetValue(request.ShipmentId ?? string.Empty, out shipment) || !_routes.TryGetValue(shipment.RouteId ?? string.Empty, out route) || !_goods.TryGetValue(shipment.ItemId ?? string.Empty, out good)) return Failure(request, eventId, "shipment_missing", "运输状态不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            if (shipment.Status != D13TransportStatus.Loaded && shipment.Status != D13TransportStatus.InTransit && shipment.Status != D13TransportStatus.Held) return Failure(request, eventId, "shipment_not_moving", "运输状态不能推进。", D13MarketTransportIds.TransportRecoveryRoute, shipment);
            if (!SourceExists(request.WeatherSourceEventId) || !SourceExists(request.EcologySourceEventId)) return Failure(request, eventId, "transport_factor_source_missing", "天气和生态压力必须引用统一事实。", D13MarketTransportIds.TransportRecoveryRoute, shipment);
            var asset = _transportAsset.ValidateTransportAsset(route.Clone(), request);
            if (asset == null || !asset.Allowed || (asset.SourceOwnerSystem != D13MarketTransportIds.ShipOwner && asset.SourceOwnerSystem != D13MarketTransportIds.PropertyOwner) || !SourceExists(asset.SourceEventId) || asset.ConditionPercent < 0 || asset.ConditionPercent > 100)
                return Failure(request, eventId, asset == null ? "transport_asset_authority_required" : asset.Code, asset == null ? "运输船体/资产状态必须由所属系统提供。" : asset.Message, D13MarketTransportIds.TransportRecoveryRoute, shipment);
            int days = Math.Max(1, request.Days);
            int weatherRisk = WeatherRisk(request.Weather);
            int assetRisk = Math.Max(0, 100 - asset.ConditionPercent) / 5;
            int ecologyRisk = Math.Max(0, Math.Min(100, request.EcologyPressurePercent)) / 10;
            int lossPercent = Math.Min(90, route.BaseLossPercent + weatherRisk + assetRisk + ecologyRisk + good.FragilityPercent / 10);
            int loss = shipment.RemainingQuantity == 0 || lossPercent == 0 ? 0 : Math.Max(1, (int)Math.Ceiling(shipment.RemainingQuantity * (lossPercent / 100f) * days));
            loss = Math.Min(shipment.RemainingQuantity, loss);
            shipment.RemainingQuantity -= loss;
            shipment.LostQuantity += loss;
            shipment.Status = shipment.RemainingQuantity == 0 ? D13TransportStatus.Lost : D13TransportStatus.InTransit;
            shipment.LastWeatherRiskPercent = weatherRisk; shipment.LastAssetRiskPercent = assetRisk; shipment.LastEcologyRiskPercent = ecologyRisk; shipment.AssetSourceEventId = asset.SourceEventId; shipment.LastUpdatedDay = Math.Max(shipment.LastUpdatedDay, request.WorldDay); shipment.LastEventId = eventId; shipment.Revision++;
            var fact = AppendEvent(eventId, request, loss > 0 ? "market_transport_loss_recorded" : "market_transport_advanced", loss > 0 ? "天气、船体状态、生态压力和货物易损度产生了确定性运输损耗。" : "运输推进未产生损耗。", new Dictionary<string, string> { ["shipment_id"] = shipment.ShipmentId, ["weather"] = request.Weather.ToString(), ["weather_risk_percent"] = weatherRisk.ToString(CultureInfo.InvariantCulture), ["asset_risk_percent"] = assetRisk.ToString(CultureInfo.InvariantCulture), ["ecology_risk_percent"] = ecologyRisk.ToString(CultureInfo.InvariantCulture), ["loss_percent"] = lossPercent.ToString(CultureInfo.InvariantCulture), ["loss_quantity"] = loss.ToString(CultureInfo.InvariantCulture), ["remaining_quantity"] = shipment.RemainingQuantity.ToString(CultureInfo.InvariantCulture), ["asset_source_event_id"] = asset.SourceEventId, ["weather_source_event_id"] = request.WeatherSourceEventId, ["ecology_source_event_id"] = request.EcologySourceEventId });
            _revision++;
            return Success(fact, null, shipment, null);
        }

        public D13MarketResult ArriveShipment(D13MarketShipmentArrivalRequest request)
        {
            if (request == null) return Invalid("invalid_request", "运输到达请求不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            string eventId = EventId(request.OperationId, "shipment_arrive_" + request.ShipmentId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindShipmentByEvent(duplicate.EventId));
            D13TransportShipmentState shipment;
            D13MarketGoodDefinition good;
            if (!_shipments.TryGetValue(request.ShipmentId ?? string.Empty, out shipment) || !_goods.TryGetValue(shipment.ItemId ?? string.Empty, out good)) return Failure(request, eventId, "shipment_missing", "运输状态不存在。", D13MarketTransportIds.TransportRecoveryRoute);
            if (shipment.Status == D13TransportStatus.Lost || shipment.Status == D13TransportStatus.Cancelled || shipment.Status == D13TransportStatus.Arrived) return Failure(request, eventId, "shipment_not_arrivable", "运输状态不能到达。", D13MarketTransportIds.TransportRecoveryRoute, shipment);
            if (shipment.RemainingQuantity <= 0) return Failure(request, eventId, "shipment_empty", "运输损耗后没有可到达货物。", D13MarketTransportIds.TransportRecoveryRoute, shipment);
            var inventory = ApplyInventory(eventId, request.InventoryHolderId, good.ItemId, shipment.RemainingQuantity, true, request.WorldDay, request.SaveRevision);
            if (!ValidInventory(inventory, good.ItemId, shipment.RemainingQuantity))
            {
                shipment.Status = D13TransportStatus.Held; shipment.LastUpdatedDay = Math.Max(shipment.LastUpdatedDay, request.WorldDay); shipment.LastEventId = eventId; shipment.Revision++;
                return Failure(request, eventId, inventory == null ? "inventory_authority_required" : inventory.Code, inventory == null ? "到达货物必须由 Inventory/Cargo 接收。" : inventory.Message, D13MarketTransportIds.InventoryRecoveryRoute, shipment);
            }
            int delivered = shipment.RemainingQuantity;
            shipment.RemainingQuantity = 0; shipment.DeliveredQuantity += delivered; shipment.Status = D13TransportStatus.Arrived; shipment.InventoryArrivalSourceEventId = inventory.SourceEventId; shipment.LastUpdatedDay = Math.Max(shipment.LastUpdatedDay, request.WorldDay); shipment.LastEventId = eventId; shipment.Revision++;
            var fact = AppendEvent(eventId, request, "market_shipment_arrived", "运输状态已到达，扣除损耗后的货物由 Inventory/Cargo 写入目的地。", new Dictionary<string, string> { ["shipment_id"] = shipment.ShipmentId, ["item_id"] = shipment.ItemId, ["delivered_quantity"] = delivered.ToString(CultureInfo.InvariantCulture), ["lost_quantity"] = shipment.LostQuantity.ToString(CultureInfo.InvariantCulture), ["inventory_source_event_id"] = inventory.SourceEventId });
            _revision++;
            return Success(fact, null, shipment, null);
        }

        private D13MarketInventoryDecision ApplyInventory(string operationId, string holderId, string itemId, int quantity, bool add, int worldDay, long saveRevision)
        {
            return _inventory.Apply(new D13MarketInventoryMutationRequest { OperationId = operationId + (add ? "_add" : "_remove"), HolderId = holderId, ItemId = itemId, Quantity = quantity, Add = add, WorldDay = worldDay, SaveRevision = saveRevision });
        }

        private bool ValidQuote(D13MarketQuoteDecision decision, D13MarketGoodDefinition definition, string marketId)
        {
            return decision != null && decision.Allowed && decision.SourceOwnerSystem == D13MarketTransportIds.EconomyOwner && decision.ItemId == definition.ItemId && decision.MarketId == marketId && decision.UnitPrice > 0 && !string.IsNullOrWhiteSpace(decision.Currency) && !string.IsNullOrWhiteSpace(decision.SourceEventId) && !string.IsNullOrWhiteSpace(decision.MarketSourceEventId) && SourceExists(decision.SourceEventId) && SourceExists(decision.MarketSourceEventId);
        }

        private bool ValidMarketFlow(D13MarketFlowDecision decision, D13MarketFlowKind kind, string itemId, int quantity)
        {
            return decision != null && decision.Allowed && decision.SourceOwnerSystem == D13MarketTransportIds.EconomyOwner && decision.Kind == kind && decision.ItemId == itemId && decision.Quantity == quantity && decision.UnitPrice >= 0 && decision.TotalValue >= 0 && !string.IsNullOrWhiteSpace(decision.Currency) && !string.IsNullOrWhiteSpace(decision.SourceEventId) && !string.IsNullOrWhiteSpace(decision.MarketSourceEventId) && SourceExists(decision.SourceEventId) && SourceExists(decision.MarketSourceEventId);
        }

        private bool ValidLegal(D13MarketLegalDecision decision, D13MarketGoodDefinition definition, string permitId)
        {
            return decision != null && decision.Allowed && decision.SourceOwnerSystem == D13MarketTransportIds.LegalOwner && decision.LegalStatus != D13MarketLegalStatus.Illegal && (!definition.RequiresPermit || decision.PermitId == definition.PermitId && permitId == definition.PermitId) && SourceExists(decision.SourceEventId);
        }

        private bool ValidInventory(D13MarketInventoryDecision decision, string itemId, int quantity)
        {
            return decision != null && decision.Allowed && decision.SourceOwnerSystem == D13MarketTransportIds.InventoryOwner && decision.ItemId == itemId && decision.Quantity == quantity && SourceExists(decision.SourceEventId);
        }

        private bool OptionalSourceExists(params string[] ids)
        {
            foreach (var id in ids) if (!string.IsNullOrWhiteSpace(id) && !SourceExists(id)) return false;
            return true;
        }

        private D13TransportShipmentState FindShipmentByEvent(string eventId)
        {
            return _shipments.Values.FirstOrDefault(item => item != null && item.LastEventId == eventId)?.Clone();
        }

        private FactEvent Existing(string eventId, long saveRevision)
        {
            var fact = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            if (fact != null) return fact;
            return _appliedEventKeys.Contains((eventId ?? string.Empty) + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture)) ? new FactEvent { EventId = eventId, SaveRevision = Math.Max(0L, saveRevision), Result = new EventResult { ResultCode = "duplicate", Summary = "市场/运输操作已在存档中结算。" } } : null;
        }

        private bool SourceExists(string eventId)
        {
            return !string.IsNullOrWhiteSpace(eventId) && _eventLog.Facts.Any(item => item != null && item.EventId == eventId);
        }

        private string EventId(string operationId, string fallback)
        {
            return "event_d13_market_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId);
        }

        private FactEvent AppendEvent(string eventId, D13MarketOperationRequest request, string resultCode, string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal, string recoveryRoute = null)
        {
            var causes = (request.CauseEventIds ?? new List<string>()).Where(item => SourceExists(item)).Distinct(StringComparer.Ordinal).ToArray();
            var route = string.IsNullOrWhiteSpace(recoveryRoute) ? null : new RecoveryRoute { RouteId = recoveryRoute, Action = "review_and_retry", ResumeFromEventId = eventId, Description = "补齐市场、库存、法律、生态、资产或运输来源后重试。" };
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request.WorldDay), "market_d13_a07", D13MarketTransportIds.SystemOwner, new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { "market_fact:" + eventId }, Values = values ?? new Dictionary<string, string>() }, new[] { string.IsNullOrWhiteSpace(request.ActorRef) ? D13MarketTransportIds.PlayerRef : request.ActorRef }, new[] { "market:d13_a07" }, causes, Math.Max(0L, request.SaveRevision), status, route);
            var fact = _eventLog.Append(worldEvent).Fact;
            _appliedEventKeys.Add(worldEvent.DedupeKey);
            return fact;
        }

        private D13MarketResult Success(FactEvent fact, D13MarketFlowRecord flow, D13TransportShipmentState shipment, D13MarketQuoteDecision quote)
        {
            return new D13MarketResult { Succeeded = true, Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode, Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary, EventId = fact == null ? string.Empty : fact.EventId, Flow = flow == null ? null : flow.Clone(), Shipment = shipment == null ? null : shipment.Clone(), Quote = quote, Fact = fact == null ? null : fact.CloneFact() };
        }

        private D13MarketResult Duplicate(FactEvent fact, D13MarketFlowRecord flow, D13TransportShipmentState shipment)
        {
            return new D13MarketResult { Succeeded = true, Duplicate = true, Code = "duplicate", Message = "市场或运输操作已结算，未重复修改库存、市场或运输损耗。", EventId = fact == null ? string.Empty : fact.EventId, Flow = flow == null ? null : flow.Clone(), Shipment = shipment == null ? null : shipment.Clone(), Fact = fact == null ? null : fact.CloneFact() };
        }

        private D13MarketResult Failure(D13MarketOperationRequest request, string eventId, string code, string message, string route, D13TransportShipmentState shipment = null)
        {
            var fact = AppendEvent(eventId, request, code ?? "market_failed", message ?? "市场/运输操作失败。", new Dictionary<string, string> { ["failure_code"] = code ?? "market_failed", ["recovery_route"] = route }, WorldEventStatus.Failed, route);
            _revision++;
            return new D13MarketResult { Succeeded = false, Code = code, Message = message, EventId = eventId, RecoveryRouteId = route, Shipment = shipment == null ? null : shipment.Clone(), Fact = fact == null ? null : fact.CloneFact() };
        }

        private D13MarketResult Invalid(string code, string message, string route)
        {
            return new D13MarketResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route };
        }

        private static int WeatherRisk(D3WeatherType weather)
        {
            switch (weather)
            {
                case D3WeatherType.Rain: return 2;
                case D3WeatherType.Storm: return 10;
                case D3WeatherType.Hurricane: return 22;
                case D3WeatherType.TradeWind: return 0;
                default: return 0;
            }
        }

        private sealed class UnavailableLegalAuthority : ID13MarketLegalAuthority
        {
            public D13MarketLegalDecision ValidateGood(D13MarketGoodDefinition definition, string locationId, string permitId, D13MarketOperationRequest request) { return new D13MarketLegalDecision { Allowed = false, Code = "legal_authority_required" }; }
            public D13MarketLegalDecision ValidateRoute(D13MarketRouteDefinition route, string permitId, D13MarketShipmentRequest request) { return new D13MarketLegalDecision { Allowed = false, Code = "legal_authority_required" }; }
        }
        private sealed class UnavailableProductionAuthority : ID13MarketProductionAuthority { public D13MarketProductionDecision ValidateProduction(D13MarketGoodDefinition definition, D13MarketProductionRequest request) { return new D13MarketProductionDecision { Allowed = false, Code = "production_authority_required" }; } }
        private sealed class UnavailableConsumptionAuthority : ID13MarketConsumptionAuthority { public D13MarketConsumptionDecision ValidateConsumption(D13MarketGoodDefinition definition, D13MarketConsumptionRequest request) { return new D13MarketConsumptionDecision { Allowed = false, Code = "consumption_authority_required" }; } }
        private sealed class UnavailableInventoryAuthority : ID13MarketInventoryAuthority { public D13MarketInventoryDecision Apply(D13MarketInventoryMutationRequest request) { return new D13MarketInventoryDecision { Allowed = false, Code = "inventory_authority_required" }; } }
        private sealed class UnavailableEconomyAuthority : ID13MarketEconomyAuthority
        {
            public D13MarketQuoteDecision Quote(D13MarketGoodDefinition definition, D13MarketQuoteRequest request) { return new D13MarketQuoteDecision { Allowed = false, Code = "economy_authority_required" }; }
            public D13MarketFlowDecision ApplyFlow(D13MarketGoodDefinition definition, D13MarketFlowRequest request) { return new D13MarketFlowDecision { Allowed = false, Code = "economy_authority_required" }; }
        }
        private sealed class UnavailableTransportAssetAuthority : ID13TransportAssetAuthority { public D13TransportAssetDecision ValidateTransportAsset(D13MarketRouteDefinition route, D13MarketShipmentAdvanceRequest request) { return new D13TransportAssetDecision { Allowed = false, Code = "transport_asset_authority_required" }; } }
    }
}
