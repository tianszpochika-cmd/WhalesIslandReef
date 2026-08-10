// D13-A06 BuildingSystem。
// 建筑状态、建设进度、损伤、开放/地图/占领投影和建设/服务事实的唯一写者。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13BuildingSystem
    {
        private readonly Dictionary<string, D13BuildingDefinition> _definitions = new Dictionary<string, D13BuildingDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13BuildingState> _buildings = new Dictionary<string, D13BuildingState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13ConstructionOrderState> _orders = new Dictionary<string, D13ConstructionOrderState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13BuildingServiceRecord> _services = new Dictionary<string, D13BuildingServiceRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13BuildingFireIncident> _fires = new Dictionary<string, D13BuildingFireIncident>(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly ID13BuildingPropertyAuthority _propertyAuthority;
        private readonly ID13BuildingWorkforceAuthority _workforceAuthority;
        private readonly ID13BuildingInventoryAuthority _inventoryAuthority;
        private readonly ID13BuildingEconomyAuthority _economyAuthority;
        private readonly ID13BuildingOccupationAuthority _occupationAuthority;
        private readonly ID13BuildingShipAuthority _shipAuthority;
        private readonly EventLog _eventLog;
        private long _revision;
        private int _lastWorldDay;
        private string _lastEventId;

        public D13BuildingSystem(
            ID13BuildingPropertyAuthority propertyAuthority = null,
            ID13BuildingWorkforceAuthority workforceAuthority = null,
            ID13BuildingInventoryAuthority inventoryAuthority = null,
            ID13BuildingEconomyAuthority economyAuthority = null,
            ID13BuildingOccupationAuthority occupationAuthority = null,
            ID13BuildingShipAuthority shipAuthority = null,
            EventLog eventLog = null,
            D13BuildingRuntimeSnapshot initial = null,
            IEnumerable<D13BuildingDefinition> definitions = null)
        {
            _propertyAuthority = propertyAuthority ?? new UnavailablePropertyAuthority();
            _workforceAuthority = workforceAuthority ?? new UnavailableWorkforceAuthority();
            _inventoryAuthority = inventoryAuthority ?? new UnavailableInventoryAuthority();
            _economyAuthority = economyAuthority ?? new UnavailableEconomyAuthority();
            _occupationAuthority = occupationAuthority ?? new UnavailableOccupationAuthority();
            _shipAuthority = shipAuthority ?? new UnavailableShipAuthority();
            _eventLog = eventLog ?? new EventLog();
            foreach (var source in definitions ?? D13BuildingCatalog.All)
            {
                if (source == null) continue;
                source.Validate();
                if (_definitions.ContainsKey(source.BuildingId)) throw new InvalidOperationException("Duplicate D13 building: " + source.BuildingId);
                _definitions[source.BuildingId] = source.Clone();
                _buildings[source.BuildingId] = NewState(source);
            }
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13BuildingDefinition FindDefinition(string buildingId)
        {
            D13BuildingDefinition result; return _definitions.TryGetValue(buildingId ?? string.Empty, out result) ? result.Clone() : null;
        }

        public D13BuildingState FindState(string buildingId)
        {
            D13BuildingState result; return _buildings.TryGetValue(buildingId ?? string.Empty, out result) ? result.Clone() : null;
        }

        public IReadOnlyList<D13BuildingDefinition> Definitions
        {
            get { return _definitions.Values.OrderBy(item => item.BuildingId, StringComparer.Ordinal).Select(item => item.Clone()).ToList(); }
        }

        public bool IsOpenAtHour(string buildingId, int hour)
        {
            D13BuildingDefinition definition; D13BuildingState state;
            if (!_definitions.TryGetValue(buildingId ?? string.Empty, out definition) || !_buildings.TryGetValue(buildingId ?? string.Empty, out state)) return false;
            return IsOpen(definition, state, hour);
        }

        public D13BuildingReadModel GetReadModel()
        {
            var result = new D13BuildingReadModel { Revision = _revision };
            foreach (var item in _definitions.Values.OrderBy(item => item.BuildingId, StringComparer.Ordinal)) result.Definitions.Add(item.Clone());
            foreach (var item in _buildings.Values.OrderBy(item => item.BuildingId, StringComparer.Ordinal)) result.Buildings.Add(item.Clone());
            foreach (var item in _orders.Values.OrderBy(item => item.OrderId, StringComparer.Ordinal)) result.Orders.Add(item.Clone());
            foreach (var item in _services.Values.OrderBy(item => item.ServiceRecordId, StringComparer.Ordinal)) result.Services.Add(item.Clone());
            foreach (var item in _fires.Values.OrderBy(item => item.FireId, StringComparer.Ordinal)) result.Fires.Add(item.Clone());
            return result;
        }

        public D13BuildingRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13BuildingRuntimeSnapshot { Revision = _revision, LastWorldDay = _lastWorldDay, LastEventId = _lastEventId };
            foreach (var item in _buildings.Values) result.Buildings.Add(item.Clone());
            foreach (var item in _orders.Values) result.Orders.Add(item.Clone());
            foreach (var item in _services.Values) result.Services.Add(item.Clone());
            foreach (var item in _fires.Values) result.Fires.Add(item.Clone());
            foreach (var key in _appliedEventKeys) result.AppliedEventKeys.Add(key);
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_building_", StringComparison.Ordinal) && !result.AppliedEventKeys.Contains(fact.DedupeKey)) result.AppliedEventKeys.Add(fact.DedupeKey);
            result.Validate(); return result;
        }

        public void Restore(D13BuildingRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var item in snapshot.Buildings)
            {
                D13BuildingDefinition definition;
                if (!_definitions.TryGetValue(item.BuildingId, out definition) || definition.PropertyId != item.PropertyId || definition.IslandId != item.IslandId || definition.Kind != item.Kind) throw new InvalidOperationException("Building snapshot references an incompatible definition: " + item.BuildingId);
            }
            foreach (var item in snapshot.Orders) if (!_definitions.ContainsKey(item.BuildingId)) throw new InvalidOperationException("Construction order references an unknown building: " + item.BuildingId);
            foreach (var item in snapshot.Services) if (!_definitions.ContainsKey(item.BuildingId)) throw new InvalidOperationException("Service record references an unknown building: " + item.BuildingId);
            foreach (var item in snapshot.Fires) if (!_definitions.ContainsKey(item.BuildingId)) throw new InvalidOperationException("Fire record references an unknown building: " + item.BuildingId);
            _buildings.Clear(); foreach (var definition in _definitions.Values) _buildings[definition.BuildingId] = NewState(definition);
            foreach (var item in snapshot.Buildings) _buildings[item.BuildingId] = item.Clone();
            _orders.Clear(); foreach (var item in snapshot.Orders) _orders[item.OrderId] = item.Clone();
            _services.Clear(); foreach (var item in snapshot.Services) _services[item.ServiceRecordId] = item.Clone();
            _fires.Clear(); foreach (var item in snapshot.Fires) _fires[item.FireId] = item.Clone();
            _appliedEventKeys.Clear(); foreach (var item in snapshot.AppliedEventKeys) _appliedEventKeys.Add(item);
            _revision = snapshot.Revision; _lastWorldDay = snapshot.LastWorldDay; _lastEventId = snapshot.LastEventId;
        }

        public D13BuildingResult PlaceConstructionOrder(D13ConstructionOrderRequest request)
        {
            if (request == null) return Invalid("invalid_request", "建设订单请求不存在.", D13BuildingIds.ConstructionRecoveryRoute);
            string eventId = EventId(request.OperationId, "place_order_" + request.BuildingId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building;
            if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "建筑或产权引用不存在.", D13BuildingIds.ConstructionRecoveryRoute);
            if (building.Lifecycle != D13BuildingLifecycle.Planned) return Failure(request, eventId, "construction_not_available", "该建筑已存在建设状态，不能重复下单.", D13BuildingIds.ConstructionRecoveryRoute, building);
            if (request.Workers <= 0 || request.Workers > definition.WorkerSlots) return Failure(request, eventId, "worker_count_invalid", "建设工人数量必须在建筑槽位范围内.", D13BuildingIds.WorkforceRecoveryRoute, building);
            var site = _propertyAuthority.ValidateSite(definition.Clone(), request); if (site == null || !site.Allowed || site.SourceOwnerSystem != D13BuildingIds.PropertyOwner || !SourceExists(site.PropertySourceEventId) || !SourceExists(site.LegalSourceEventId)) return Failure(request, eventId, "site_authority_invalid", "建设必须经过 PropertySystem 和法律来源确认.", D13BuildingIds.ConstructionRecoveryRoute, building);
            var workers = _workforceAuthority.ValidateWorkers(definition.Clone(), request.Workers, request.ActorRef, request.WorldDay); if (workers == null || !workers.Allowed || workers.Workers != request.Workers || workers.SourceOwnerSystem != D13BuildingIds.WorkforceOwner || !SourceExists(workers.SourceEventId)) return Failure(request, eventId, "workforce_authority_invalid", "建设工人必须由岛屿治理/劳务 authority 提供来源事实.", D13BuildingIds.WorkforceRecoveryRoute, building);
            var materials = _inventoryAuthority.ConsumeConstructionMaterials(definition.Clone(), request); if (materials == null || !materials.Allowed || materials.SourceOwnerSystem != D13BuildingIds.InventoryOwner || !SourceExists(materials.SourceEventId)) return Failure(request, eventId, "construction_material_invalid", "建设材料必须由 Inventory/Cargo 消耗并返回来源.", D13BuildingIds.MaterialRecoveryRoute, building);
            var order = new D13ConstructionOrderState { OrderId = eventId, BuildingId = definition.BuildingId, PropertyId = definition.PropertyId, State = D13ConstructionOrderStatus.InProgress, StartDay = Math.Max(0, request.WorldDay), DueDay = Math.Max(0, request.WorldDay) + definition.BuildDaysRequired, WorkerCount = request.Workers, DaysElapsed = 0, PropertySourceEventId = site.PropertySourceEventId, InventorySourceEventId = materials.SourceEventId, WorkforceSourceEventId = workers.SourceEventId, LastUpdatedDay = Math.Max(0, request.WorldDay), Revision = 1, LastEventId = eventId };
            foreach (var item in definition.ConstructionMaterials) order.Materials.Add(item.Clone());
            building.Lifecycle = D13BuildingLifecycle.UnderConstruction; building.DamageState = D13BuildingDamageState.None; building.DamagePoints = 0; building.BuildDaysRequired = definition.BuildDaysRequired; building.BuildDaysElapsed = 0; building.WorkerCount = request.Workers; building.MapState = D13BuildingMapState.Construction; Touch(building, eventId, request.WorldDay); _orders[order.OrderId] = order;
            var fact = AppendEvent(eventId, request, "building_order_placed", "建设订单通过产权、工人和库存 authority，材料已由 Inventory/Cargo 处理.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["property_source_event_id"] = site.PropertySourceEventId, ["inventory_source_event_id"] = materials.SourceEventId, ["workforce_source_event_id"] = workers.SourceEventId, ["due_day"] = order.DueDay.ToString(CultureInfo.InvariantCulture) }); _revision++; return Success(fact, building, order, null, null);
        }

        public D13BuildingResult AdvanceConstruction(D13ConstructionProgressRequest request)
        {
            if (request == null) return Invalid("invalid_request", "建设进度请求不存在.", D13BuildingIds.ConstructionRecoveryRoute);
            string eventId = EventId(request.OperationId, "advance_" + request.BuildingId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "建筑不存在.", D13BuildingIds.ConstructionRecoveryRoute);
            D13ConstructionOrderState order = _orders.Values.FirstOrDefault(item => item != null && item.BuildingId == definition.BuildingId && item.State == D13ConstructionOrderStatus.InProgress); if (order == null || building.Lifecycle != D13BuildingLifecycle.UnderConstruction) return Failure(request, eventId, "construction_order_missing", "没有可推进的建设订单.", D13BuildingIds.ConstructionRecoveryRoute, building);
            if (request.Workers <= 0 || request.Workers > definition.WorkerSlots) return Failure(request, eventId, "worker_count_invalid", "推进工人数量超出建筑槽位.", D13BuildingIds.WorkforceRecoveryRoute, building);
            var workers = _workforceAuthority.ValidateWorkers(definition.Clone(), request.Workers, request.ActorRef, request.WorldDay); if (workers == null || !workers.Allowed || workers.Workers != request.Workers || workers.SourceOwnerSystem != D13BuildingIds.WorkforceOwner || !SourceExists(workers.SourceEventId)) return Failure(request, eventId, "workforce_authority_invalid", "建设推进缺少工人来源事实.", D13BuildingIds.WorkforceRecoveryRoute, building);
            building.WorkerCount = request.Workers; building.BuildDaysElapsed = Math.Min(definition.BuildDaysRequired, building.BuildDaysElapsed + 1); order.WorkerCount = request.Workers; order.DaysElapsed = building.BuildDaysElapsed; order.LastUpdatedDay = Math.Max(order.LastUpdatedDay, request.WorldDay); order.Revision++; order.LastEventId = eventId; Touch(building, eventId, request.WorldDay);
            if (building.BuildDaysElapsed >= definition.BuildDaysRequired) { building.Lifecycle = D13BuildingLifecycle.Operational; building.MapState = D13BuildingMapState.Operational; order.State = D13ConstructionOrderStatus.Completed; }
            var fact = AppendEvent(eventId, request, order.State == D13ConstructionOrderStatus.Completed ? "building_completed" : "building_progressed", order.State == D13ConstructionOrderStatus.Completed ? "建设工期完成，建筑进入可见的运营状态." : "建设订单推进一天，工人和工期仍可追踪.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["days_elapsed"] = building.BuildDaysElapsed.ToString(CultureInfo.InvariantCulture), ["days_required"] = definition.BuildDaysRequired.ToString(CultureInfo.InvariantCulture), ["worker_source_event_id"] = workers.SourceEventId, ["map_state"] = building.MapState.ToString() }); _revision++; return Success(fact, building, order, null, null);
        }

        public D13BuildingResult RunService(D13BuildingServiceRequest request)
        {
            if (request == null) return Invalid("invalid_request", "建筑服务请求不存在.", D13BuildingIds.EconomyRecoveryRoute);
            string eventId = EventId(request.OperationId, "service_" + request.BuildingId + "_" + request.ServiceId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "建筑不存在.", D13BuildingIds.EconomyRecoveryRoute);
            var service = definition.FindService(request.ServiceId); if (service == null) return Failure(request, eventId, "service_unknown", "建筑不提供该服务.", D13BuildingIds.EconomyRecoveryRoute, building);
            bool maintenance = service.ServiceId == D13BuildingCatalog.MaintenanceServiceId; if (!(maintenance ? CanMaintain(building) : CanOperate(definition, building, request.WorldHour))) return Failure(request, eventId, "service_closed", "建筑未开放、受损或被占领，服务不可用.", D13BuildingIds.EconomyRecoveryRoute, building);
            int workersRequested = request.Workers <= 0 ? 1 : request.Workers; if (workersRequested > definition.WorkerSlots) return Failure(request, eventId, "worker_count_invalid", "服务工人数量超出建筑槽位.", D13BuildingIds.WorkforceRecoveryRoute, building);
            var workers = _workforceAuthority.ValidateWorkers(definition.Clone(), workersRequested, request.ActorRef, request.WorldDay); if (workers == null || !workers.Allowed || workers.Workers != workersRequested || workers.SourceOwnerSystem != D13BuildingIds.WorkforceOwner || !SourceExists(workers.SourceEventId)) return Failure(request, eventId, "workforce_authority_invalid", "服务工人必须由劳务 authority 提供来源.", D13BuildingIds.WorkforceRecoveryRoute, building);
            var price = _economyAuthority.QuoteService(definition.Clone(), service.Clone(), request); if (price == null || !price.Allowed || price.ServiceId != service.ServiceId || price.Amount < 0 || string.IsNullOrWhiteSpace(price.Currency) || string.IsNullOrWhiteSpace(price.LedgerRef) || price.SourceOwnerSystem != D13BuildingIds.EconomyOwner || !SourceExists(price.SourceEventId)) return Failure(request, eventId, "service_price_invalid", "服务价格必须由 EconomySystem 返回统一报价和账本来源.", D13BuildingIds.EconomyRecoveryRoute, building);
            D13BuildingShipServiceDecision ship = null;
            if (service.RequiresShip)
            {
                if (string.IsNullOrWhiteSpace(request.ShipId)) return Failure(request, eventId, "ship_required", "该船坞服务需要目标船只.", D13BuildingIds.ShipRecoveryRoute, building);
                var shipRequest = new D13BuildingShipServiceRequest { OperationId = request.OperationId, ActorRef = request.ActorRef, BuildingId = request.BuildingId, IslandId = request.IslandId, PropertyId = request.PropertyId, WorldDay = request.WorldDay, SaveRevision = request.SaveRevision, ShipId = request.ShipId, ServiceId = request.ServiceId, Workers = request.Workers, CauseEventIds = request.CauseEventIds };
                ship = _shipAuthority.ApproveShipService(definition.Clone(), service.Clone(), shipRequest); if (ship == null || !ship.Allowed || ship.ShipId != request.ShipId || ship.SourceOwnerSystem != D13BuildingIds.ShipOwner || !SourceExists(ship.SourceEventId)) return Failure(request, eventId, "ship_service_invalid", "船舶结果必须由 ShipSystem authority 返回来源化批准.", D13BuildingIds.ShipRecoveryRoute, building);
            }
            var inventory = _inventoryAuthority.ApplyService(definition.Clone(), service.Clone(), request); if (inventory == null || !inventory.Allowed || inventory.SourceOwnerSystem != D13BuildingIds.InventoryOwner || !SourceExists(inventory.SourceEventId)) return Failure(request, eventId, "service_inventory_invalid", "服务材料和产出必须由 Inventory/Cargo 处理.", D13BuildingIds.MaterialRecoveryRoute, building);
            if (service.RequiresWarehouse && definition.Kind != D13BuildingKind.Warehouse) return Failure(request, eventId, "warehouse_required", "仓库存取服务只能由仓库提供.", D13BuildingIds.MaterialRecoveryRoute, building);
            if (service.ServiceId == D13BuildingCatalog.MaintenanceServiceId) RecoverDamage(building);
            var record = new D13BuildingServiceRecord { ServiceRecordId = eventId, BuildingId = definition.BuildingId, ServiceId = service.ServiceId, ActorRef = string.IsNullOrWhiteSpace(request.ActorRef) ? D13BuildingIds.PlayerRef : request.ActorRef, PriceAmount = price.Amount, Currency = price.Currency, LedgerRef = price.LedgerRef, EconomySourceEventId = price.SourceEventId, InventorySourceEventId = inventory.SourceEventId, ShipSourceEventId = ship == null ? null : ship.SourceEventId, OutputSummary = inventory.OutputSummary ?? (ship == null ? string.Empty : "hull_points:" + ship.AppliedHullPoints.ToString(CultureInfo.InvariantCulture)), WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0, request.SaveRevision) };
            _services[record.ServiceRecordId] = record; Touch(building, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, service.ServiceId == D13BuildingCatalog.MaintenanceServiceId ? "building_maintained" : "building_service_completed", service.ServiceId == D13BuildingCatalog.MaintenanceServiceId ? "维护服务完成，损伤由 BuildingSystem 按规则恢复." : "建筑服务完成；价格、库存和船舶结果均保留外部 owner 来源.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["service_id"] = service.ServiceId, ["price_amount"] = price.Amount.ToString(CultureInfo.InvariantCulture), ["economy_source_event_id"] = price.SourceEventId, ["inventory_source_event_id"] = inventory.SourceEventId, ["ship_source_event_id"] = ship == null ? string.Empty : ship.SourceEventId, ["map_state"] = building.MapState.ToString() }); _revision++; return Success(fact, building, null, record, null);
        }

        public D13BuildingResult StoreWarehouseItem(D13WarehouseStoreRequest request)
        {
            if (request == null) return Invalid("invalid_request", "仓库存取请求不存在.", D13BuildingIds.MaterialRecoveryRoute);
            string eventId = EventId(request.OperationId, "warehouse_store_" + request.BuildingId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "仓库不存在.", D13BuildingIds.MaterialRecoveryRoute);
            if (definition.Kind != D13BuildingKind.Warehouse || string.IsNullOrWhiteSpace(request.ItemId) || request.Quantity <= 0) return Failure(request, eventId, "warehouse_request_invalid", "只有仓库可以存入正数量物品.", D13BuildingIds.MaterialRecoveryRoute, building);
            if (!CanOperate(definition, building, request.WorldHour)) return Failure(request, eventId, "warehouse_closed", "仓库未开放、受损或被占领.", D13BuildingIds.MaterialRecoveryRoute, building);
            var price = _economyAuthority.QuoteService(definition.Clone(), definition.FindService("warehouse_storage").Clone(), request); if (price == null || !price.Allowed || price.SourceOwnerSystem != D13BuildingIds.EconomyOwner || !SourceExists(price.SourceEventId) || string.IsNullOrWhiteSpace(price.LedgerRef)) return Failure(request, eventId, "service_price_invalid", "仓储服务价格必须来自 EconomySystem.", D13BuildingIds.EconomyRecoveryRoute, building);
            var inventory = _inventoryAuthority.StoreWarehouseItem(definition.Clone(), request); if (inventory == null || !inventory.Allowed || inventory.SourceOwnerSystem != D13BuildingIds.InventoryOwner || !SourceExists(inventory.SourceEventId)) return Failure(request, eventId, "warehouse_inventory_invalid", "仓库物品必须由 Inventory/Cargo 写入.", D13BuildingIds.MaterialRecoveryRoute, building);
            var record = new D13BuildingServiceRecord { ServiceRecordId = eventId, BuildingId = definition.BuildingId, ServiceId = "warehouse_storage", ActorRef = string.IsNullOrWhiteSpace(request.ActorRef) ? D13BuildingIds.PlayerRef : request.ActorRef, PriceAmount = price.Amount, Currency = price.Currency, LedgerRef = price.LedgerRef, EconomySourceEventId = price.SourceEventId, InventorySourceEventId = inventory.SourceEventId, OutputSummary = request.ItemId + ":" + request.Quantity.ToString(CultureInfo.InvariantCulture), WorldDay = Math.Max(0, request.WorldDay), SaveRevision = Math.Max(0, request.SaveRevision) }; _services[record.ServiceRecordId] = record; Touch(building, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "warehouse_item_stored", "仓库物品已由 Inventory/Cargo 写入，仓储价格来自 EconomySystem.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["item_id"] = request.ItemId, ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture), ["economy_source_event_id"] = price.SourceEventId, ["inventory_source_event_id"] = inventory.SourceEventId }); _revision++; return Success(fact, building, null, record, null);
        }

        public D13BuildingResult ApplyFire(D13BuildingFireRequest request)
        {
            if (request == null) return Invalid("invalid_request", "火灾请求不存在.", D13BuildingIds.FireRecoveryRoute);
            string eventId = EventId(request.OperationId, "fire_" + request.BuildingId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "建筑不存在.", D13BuildingIds.FireRecoveryRoute);
            if (request.Severity < 1 || request.Severity > 3 || !SourceExists(request.CauseEventId)) return Failure(request, eventId, "fire_request_invalid", "火灾必须有 1—3 级严重度和原因事实.", D13BuildingIds.FireRecoveryRoute, building);
            var fire = new D13BuildingFireIncident { FireId = eventId, BuildingId = definition.BuildingId, Severity = request.Severity, DamagePoints = request.Severity * 25, Extinguished = false, CauseEventId = request.CauseEventId, WorldDay = Math.Max(0, request.WorldDay), Revision = 1 };
            building.DamagePoints = Clamp(building.DamagePoints + fire.DamagePoints); building.DamageState = D13BuildingDamageState.FireDamaged; building.Lifecycle = D13BuildingLifecycle.Closed; building.MapState = D13BuildingMapState.Fire; Touch(building, eventId, request.WorldDay); _fires[fire.FireId] = fire;
            var fact = AppendEvent(eventId, request, "building_fire_started", "火灾使建筑关闭并在地图上显示火灾状态；损伤只能通过灭火和维护恢复.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["severity"] = request.Severity.ToString(CultureInfo.InvariantCulture), ["cause_event_id"] = request.CauseEventId, ["map_state"] = building.MapState.ToString() }, WorldEventStatus.Normal, D13BuildingIds.FireRecoveryRoute); _revision++; return Success(fact, building, null, null, fire);
        }

        public D13BuildingResult ExtinguishFire(D13BuildingExtinguishFireRequest request)
        {
            if (request == null) return Invalid("invalid_request", "灭火请求不存在.", D13BuildingIds.FireRecoveryRoute);
            string eventId = EventId(request.OperationId, "extinguish_" + request.FireId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, null);
            D13BuildingFireIncident fire; if (!_fires.TryGetValue(request.FireId ?? string.Empty, out fire)) return Failure(request, eventId, "fire_missing", "火灾记录不存在.", D13BuildingIds.FireRecoveryRoute);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndStateById(fire.BuildingId, out definition, out building)) return Failure(request, eventId, "building_unknown", "火灾建筑不存在.", D13BuildingIds.FireRecoveryRoute);
            if (fire.Extinguished) return Failure(request, eventId, "fire_extinguished", "火灾已经扑灭.", D13BuildingIds.FireRecoveryRoute, building);
            var workers = _workforceAuthority.ValidateWorkers(definition.Clone(), Math.Max(1, request.Workers), request.ActorRef, request.WorldDay); if (workers == null || !workers.Allowed || workers.SourceOwnerSystem != D13BuildingIds.WorkforceOwner || !SourceExists(workers.SourceEventId)) return Failure(request, eventId, "workforce_authority_invalid", "灭火工人必须有劳务来源事实.", D13BuildingIds.WorkforceRecoveryRoute, building);
            fire.Extinguished = true; fire.Revision++; building.DamageState = building.DamagePoints >= 50 ? D13BuildingDamageState.Major : D13BuildingDamageState.Minor; building.Lifecycle = D13BuildingLifecycle.Closed; building.MapState = D13BuildingMapState.Damaged; Touch(building, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, "building_fire_extinguished", "火灾已扑灭，但建筑仍保持损伤和关闭状态，不能绕过维护.", new Dictionary<string, string> { ["building_id"] = building.BuildingId, ["fire_id"] = fire.FireId, ["worker_source_event_id"] = workers.SourceEventId, ["map_state"] = building.MapState.ToString() }); _revision++; return Success(fact, building, null, null, fire);
        }

        public D13BuildingResult SetOccupation(D13BuildingOccupationRequest request)
        {
            if (request == null) return Invalid("invalid_request", "占领请求不存在.", D13BuildingIds.OccupationRecoveryRoute);
            string eventId = EventId(request.OperationId, "occupation_" + request.BuildingId); var duplicate = Existing(eventId, request.SaveRevision); if (duplicate != null) return Duplicate(duplicate, request.BuildingId);
            D13BuildingDefinition definition; D13BuildingState building; if (!TryDefinitionAndState(request, out definition, out building)) return Failure(request, eventId, "building_unknown", "建筑不存在.", D13BuildingIds.OccupationRecoveryRoute);
            var decision = _occupationAuthority.AuthorizeOccupation(definition.Clone(), request); if (decision == null || !decision.Allowed || decision.SourceOwnerSystem != D13BuildingIds.OccupationOwner || !Enum.IsDefined(typeof(D13BuildingOccupationState), decision.State) || decision.State != D13BuildingOccupationState.Free && string.IsNullOrWhiteSpace(decision.FactionId) || !SourceExists(decision.SourceEventId)) return Failure(request, eventId, "occupation_authority_invalid", "占领或争议状态必须由法律/势力 authority 返回来源.", D13BuildingIds.OccupationRecoveryRoute, building);
            building.OccupationState = decision.State; building.OccupyingFactionId = decision.State == D13BuildingOccupationState.Free ? null : decision.FactionId; building.OccupationSourceEventId = decision.SourceEventId; if (decision.State != D13BuildingOccupationState.Free) { building.Lifecycle = D13BuildingLifecycle.Closed; building.MapState = D13BuildingMapState.Occupied; } else { if (building.Lifecycle == D13BuildingLifecycle.Closed) building.Lifecycle = D13BuildingLifecycle.Operational; building.MapState = MapFor(definition, building); } Touch(building, eventId, request.WorldDay); var fact = AppendEvent(eventId, request, decision.State == D13BuildingOccupationState.Free ? "building_occupation_released" : "building_occupied", "建筑占领/争议状态由法律 authority 裁决，地图状态同步可见.", new Dictionary<string, string> { ["building_id"] = definition.BuildingId, ["occupation_state"] = decision.State.ToString(), ["faction_id"] = decision.FactionId ?? string.Empty, ["occupation_source_event_id"] = decision.SourceEventId, ["map_state"] = building.MapState.ToString() }); _revision++; return Success(fact, building, null, null, null);
        }

        private void RecoverDamage(D13BuildingState building)
        {
            building.DamagePoints = Math.Max(0, building.DamagePoints - 50);
            if (building.DamagePoints == 0) { building.DamageState = D13BuildingDamageState.None; if (building.OccupationState == D13BuildingOccupationState.Free) { building.Lifecycle = D13BuildingLifecycle.Operational; building.MapState = D13BuildingMapState.Operational; } }
            else { building.DamageState = building.DamagePoints >= 50 ? D13BuildingDamageState.Major : D13BuildingDamageState.Minor; building.Lifecycle = D13BuildingLifecycle.Closed; building.MapState = D13BuildingMapState.Damaged; }
        }

        private bool TryDefinitionAndState(D13BuildingOperationRequest request, out D13BuildingDefinition definition, out D13BuildingState state)
        {
            if (!_definitions.TryGetValue(request.BuildingId ?? string.Empty, out definition) || !_buildings.TryGetValue(request.BuildingId ?? string.Empty, out state) || definition.IslandId != request.IslandId || definition.PropertyId != request.PropertyId) { definition = null; state = null; return false; }
            return true;
        }
        private bool TryDefinitionAndStateById(string buildingId, out D13BuildingDefinition definition, out D13BuildingState state) { definition = null; state = null; if (!_definitions.TryGetValue(buildingId ?? string.Empty, out definition)) return false; return _buildings.TryGetValue(buildingId ?? string.Empty, out state); }
        private D13BuildingState NewState(D13BuildingDefinition definition) { return new D13BuildingState { BuildingId = definition.BuildingId, PropertyId = definition.PropertyId, IslandId = definition.IslandId, Kind = definition.Kind, BuildDaysRequired = definition.BuildDaysRequired, MapState = D13BuildingMapState.Hidden }; }
        private bool IsOpen(D13BuildingDefinition definition, D13BuildingState state, int hour) { return state.Lifecycle == D13BuildingLifecycle.Operational && state.OccupationState == D13BuildingOccupationState.Free && state.DamageState != D13BuildingDamageState.Major && state.DamageState != D13BuildingDamageState.FireDamaged && state.DamageState != D13BuildingDamageState.WarDamaged && hour >= definition.OpenFromHour && hour < definition.OpenToHour; }
        private bool CanOperate(D13BuildingDefinition definition, D13BuildingState state, int worldHour) { return IsOpen(definition, state, Math.Max(0, Math.Min(23, worldHour))); }
        private bool CanMaintain(D13BuildingState state) { return state.OccupationState == D13BuildingOccupationState.Free && state.Lifecycle != D13BuildingLifecycle.Destroyed && state.DamageState != D13BuildingDamageState.None; }
        private static D13BuildingMapState MapFor(D13BuildingDefinition definition, D13BuildingState state) { if (state.OccupationState != D13BuildingOccupationState.Free) return D13BuildingMapState.Occupied; if (state.DamageState == D13BuildingDamageState.FireDamaged) return D13BuildingMapState.Fire; if (state.DamageState != D13BuildingDamageState.None) return D13BuildingMapState.Damaged; if (state.Lifecycle == D13BuildingLifecycle.UnderConstruction) return D13BuildingMapState.Construction; if (state.Lifecycle == D13BuildingLifecycle.Operational) return D13BuildingMapState.Operational; if (state.Lifecycle == D13BuildingLifecycle.Closed) return D13BuildingMapState.Closed; return D13BuildingMapState.Hidden; }
        private static int Clamp(int value) { return Math.Max(0, Math.Min(100, value)); }
        private void Touch(D13BuildingState state, string eventId, int day) { state.Revision++; state.LastEventId = eventId; state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, day)); }
        private string EventId(string operationId, string fallback) { return "event_d13_building_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId); }
        private bool SourceExists(string eventId) { return !string.IsNullOrWhiteSpace(eventId) && _eventLog.Facts.Any(item => item != null && item.EventId == eventId); }
        private FactEvent Existing(string eventId, long saveRevision) { var fact = _eventLog.Find(eventId, Math.Max(0, saveRevision)); if (fact != null) return fact; return _appliedEventKeys.Contains((eventId ?? string.Empty) + "\u001f" + Math.Max(0, saveRevision).ToString(CultureInfo.InvariantCulture)) ? new FactEvent { EventId = eventId, SaveRevision = Math.Max(0, saveRevision), Result = new EventResult { ResultCode = "duplicate", Summary = "建筑操作已在存档中结算." } } : null; }
        private FactEvent AppendEvent(string eventId, D13BuildingOperationRequest request, string resultCode, string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal, string recoveryRoute = null)
        {
            var causes = (request.CauseEventIds ?? new List<string>()).Where(item => SourceExists(item)).Distinct(StringComparer.Ordinal).ToArray();
            var route = string.IsNullOrWhiteSpace(recoveryRoute) ? null : new RecoveryRoute { RouteId = recoveryRoute, Action = "review_and_retry", ResumeFromEventId = eventId, Description = "由建筑、产权、库存、经济或法律 authority 补充事实后重试." };
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request.WorldDay), request.IslandId, D13BuildingIds.SystemOwner, new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { "building_fact:" + eventId }, Values = values ?? new Dictionary<string, string>() }, new[] { string.IsNullOrWhiteSpace(request.ActorRef) ? D13BuildingIds.PlayerRef : request.ActorRef }, new[] { "building:d13_a06" }, causes, Math.Max(0, request.SaveRevision), status, route);
            var fact = _eventLog.Append(worldEvent).Fact; _appliedEventKeys.Add(worldEvent.DedupeKey); _lastEventId = eventId; _lastWorldDay = Math.Max(_lastWorldDay, Math.Max(0, request.WorldDay)); return fact;
        }
        private D13BuildingResult Success(FactEvent fact, D13BuildingState building, D13ConstructionOrderState order, D13BuildingServiceRecord service, D13BuildingFireIncident fire) { return new D13BuildingResult { Succeeded = true, Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode, Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary, EventId = fact == null ? string.Empty : fact.EventId, Building = building == null ? null : building.Clone(), Order = order == null ? null : order.Clone(), Service = service == null ? null : service.Clone(), Fire = fire == null ? null : fire.Clone(), Fact = fact == null ? null : fact.CloneFact() }; }
        private D13BuildingResult Failure(D13BuildingOperationRequest request, string eventId, string code, string message, string route, D13BuildingState building = null) { var fact = AppendEvent(eventId, request, code, message, new Dictionary<string, string> { ["failure_code"] = code, ["recovery_route"] = route }, WorldEventStatus.Failed, route); _revision++; return new D13BuildingResult { Succeeded = false, Code = code, Message = message, EventId = eventId, RecoveryRouteId = route, Building = building == null ? null : building.Clone(), Fact = fact }; }
        private D13BuildingResult Invalid(string code, string message, string route) { return new D13BuildingResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route }; }
        private D13BuildingResult Duplicate(FactEvent fact, string buildingId) { return new D13BuildingResult { Succeeded = true, Duplicate = true, Code = "duplicate", Message = "建筑操作已结算，未重复建设、扣料、服务或改变损伤.", EventId = fact == null ? string.Empty : fact.EventId, Building = FindState(buildingId), Fact = fact == null ? null : fact.CloneFact() }; }

        private sealed class UnavailablePropertyAuthority : ID13BuildingPropertyAuthority { public D13BuildingSiteDecision ValidateSite(D13BuildingDefinition definition, D13BuildingOperationRequest request) { return new D13BuildingSiteDecision { Allowed = false, Code = "property_authority_required" }; } }
        private sealed class UnavailableWorkforceAuthority : ID13BuildingWorkforceAuthority { public D13BuildingWorkerDecision ValidateWorkers(D13BuildingDefinition definition, int workers, string actorRef, int worldDay) { return new D13BuildingWorkerDecision { Allowed = false, Code = "workforce_authority_required" }; } }
        private sealed class UnavailableInventoryAuthority : ID13BuildingInventoryAuthority { public D13BuildingInventoryDecision ConsumeConstructionMaterials(D13BuildingDefinition definition, D13BuildingOperationRequest request) { return new D13BuildingInventoryDecision { Allowed = false, Code = "inventory_authority_required" }; } public D13BuildingInventoryDecision ApplyService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request) { return new D13BuildingInventoryDecision { Allowed = false, Code = "inventory_authority_required" }; } public D13BuildingInventoryDecision StoreWarehouseItem(D13BuildingDefinition definition, D13WarehouseStoreRequest request) { return new D13BuildingInventoryDecision { Allowed = false, Code = "inventory_authority_required" }; } }
        private sealed class UnavailableEconomyAuthority : ID13BuildingEconomyAuthority { public D13BuildingEconomyDecision QuoteService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request) { return new D13BuildingEconomyDecision { Allowed = false, Code = "economy_authority_required" }; } }
        private sealed class UnavailableOccupationAuthority : ID13BuildingOccupationAuthority { public D13BuildingOccupationDecision AuthorizeOccupation(D13BuildingDefinition definition, D13BuildingOccupationRequest request) { return new D13BuildingOccupationDecision { Allowed = false, Code = "occupation_authority_required" }; } }
        private sealed class UnavailableShipAuthority : ID13BuildingShipAuthority { public D13BuildingShipServiceDecision ApproveShipService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingShipServiceRequest request) { return new D13BuildingShipServiceDecision { Allowed = false, Code = "ship_authority_required" }; } }
    }
}
