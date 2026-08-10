using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    /// <summary>
    /// 船上活鱼舱的唯一写者。它只写 D11LiveFishHoldState；货物搬运仍经
    /// D3InventorySystem，事件先进入 EventLog，再提交舱内状态。
    /// </summary>
    public sealed class D11LiveFishHoldSystem
    {
        private const string Owner = "LiveFishHoldSystem";
        private readonly D11LiveFishHoldState _state;
        private readonly D3TradeState _trade;
        private readonly EventLog _eventLog;

        public D11LiveFishHoldSystem(D11LiveFishHoldState initial = null,
            D3TradeState trade = null, EventLog eventLog = null)
        {
            D11AquacultureInventoryDefinitions.Ensure();
            _state = initial == null ? new D11LiveFishHoldState() : initial.Clone();
            _trade = trade;
            _eventLog = eventLog ?? new EventLog();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D11LiveFishHoldState Snapshot { get { return _state.Clone(); } }
        public int CapacityVolume { get { return _state.CapacityVolume; } }
        public int UsedVolume { get { return _state.UsedVolume; } }
        public int TotalLiveWeight { get { return _state.TotalWeight; } }
        public float Crowding { get { return _state.Crowding; } }

        public D11AquacultureResult StockFry(D11LiveFishStockRequest request)
        {
            if (request == null) return Reject("invalid_request", "活鱼舱入舱请求不存在。", 0, null);
            var fish = D11AquacultureCatalog.FindFish(request.FishSpeciesId);
            string eventId = EventId(request.EventId, "event_d11_live_fish_stock_", request.WorldDay);
            long revision = Math.Max(0L, request.SaveRevision);
            if (fish == null) return Reject("fish_species_missing", "未知的活鱼物种。", request.WorldDay, eventId, revision);
            if (request.Quantity <= 0) return Reject("invalid_quantity", "鱼苗数量必须大于零。", request.WorldDay, eventId, revision);
            if (_trade == null || _trade.Inventory == null)
                return Reject("inventory_unavailable", "库存系统不可用，不能转移鱼苗。", request.WorldDay, eventId, revision);
            if (D3InventorySystem.CountCargo(_trade.Inventory, fish.FryItemId) < request.Quantity)
                return Reject("not_enough_fry", "货舱中没有足够的鱼苗。", request.WorldDay, eventId, revision);
            if (request.Quantity * fish.UnitVolume > _state.FreeVolume ||
                _state.FishQuantity + request.Quantity > _state.MaxFish)
                return Reject("live_hold_full", "活鱼舱容积或数量已满。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(_state, _eventLog, eventId, revision))
                return Duplicate(eventId, revision, "鱼苗入舱事实已经结算，未重复转移。");

            var values = new Dictionary<string, string>
            {
                ["facility_id"] = _state.FacilityId,
                ["fish_species_id"] = fish.SpeciesId,
                ["input_fry_item_id"] = fish.FryItemId,
                ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                ["weight_added"] = (request.Quantity * fish.UnitWeight).ToString(CultureInfo.InvariantCulture),
                ["volume_added"] = (request.Quantity * fish.UnitVolume).ToString(CultureInfo.InvariantCulture),
            };
            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                _state.LocationRef, Owner, new[] { _state.FacilityId },
                new[] { "input:" + fish.FryItemId, "fish:" + fish.SpeciesId },
                Causes(), "live_fish_stocked", "鱼苗已进入活鱼舱。", values,
                new[] { _state.FacilityId, fish.SpeciesId }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, revision, "鱼苗入舱事实已经结算，未重复转移。");

            // 事件已经落日志；随后由 InventorySystem 和活鱼舱 owner 分别提交自己的状态。
            if (!D3InventorySystem.TryRemoveCargo(_trade.Inventory, fish.FryItemId,
                request.Quantity, out string inventoryError))
                return RejectAfterFact("inventory_conflict", inventoryError, eventId, revision, append.Fact);
            D11AquacultureMath.AddFish(_state, fish, request.Quantity);
            _state.Crowding = D11AquacultureMath.Crowding(_state);
            _state.LastWorldDay = Math.Max(_state.LastWorldDay, Math.Max(0, request.WorldDay));
            _state.LastEventId = eventId;
            D11AquacultureMath.Remember(_state, eventId, revision);
            return Success(append.Fact, eventId, request.WorldDay, request.Quantity, 0, 0, 0);
        }

        public D11AquacultureResult Maintain(D11LiveFishMaintenanceRequest request)
        {
            if (request == null) return Reject("invalid_request", "活鱼舱维护请求不存在。", 0, null);
            string eventId = EventId(request.EventId, "event_d11_live_fish_maintenance_", request.WorldDay);
            long revision = Math.Max(0L, request.SaveRevision);
            if (request.WorldDay < _state.LastWorldDay)
                return Reject("world_day_rewind", "活鱼舱世界日不能倒退。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(_state, _eventLog, eventId, revision))
                return Duplicate(eventId, revision, "活鱼舱维护事实已经结算，未重复扣减活鱼。");

            var definition = D11AquacultureCatalog.FindFacility(D11AquacultureFacilityKind.LiveFishHold);
            var projection = D11AquacultureMath.Advance(_state, definition,
                D11AquacultureCatalog.Mackerel, request.WorldDay,
                request.WaterExchangeUnits, request.LaborUnits, request.Seed, false);
            var values = StateValues(projection.State);
            values["days_applied"] = projection.DaysApplied.ToString(CultureInfo.InvariantCulture);
            values["dead_quantity"] = projection.DeadQuantity.ToString(CultureInfo.InvariantCulture);
            values["water_exchange_units"] = Math.Max(0, request.WaterExchangeUnits).ToString(CultureInfo.InvariantCulture);
            values["labor_units"] = Math.Max(0, request.LaborUnits).ToString(CultureInfo.InvariantCulture);
            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                _state.LocationRef, Owner, new[] { _state.FacilityId },
                new[] { "maintenance:water_exchange", "maintenance:labor" }, Causes(),
                "live_fish_hold_maintained", "活鱼舱换水、氧气、水质和维护已结算。", values,
                new[] { _state.FacilityId, "fact:live_fish_hold_maintenance" }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, revision, "活鱼舱维护事实已经结算，未重复扣减活鱼。");

            _state.CopyFrom(projection.State);
            _state.LastEventId = eventId;
            D11AquacultureMath.Remember(_state, eventId, revision);
            var result = Success(append.Fact, eventId, request.WorldDay, _state.FishQuantity,
                projection.DeadQuantity, projection.FeedConsumed, projection.LaborConsumed);
            if (projection.DeadQuantity > 0)
            {
                string deathEventId = "event_d11_live_fish_death_" + Math.Max(0, request.WorldDay).ToString(CultureInfo.InvariantCulture);
                var death = D11AquacultureMath.Append(_eventLog, deathEventId, request.WorldDay,
                    _state.LocationRef, Owner, new[] { _state.FacilityId },
                    new[] { "fact:live_fish_hold_maintenance" }, new[] { eventId },
                    "live_fish_died", "活鱼舱结算记录了运输死亡。", new Dictionary<string, string>
                    {
                        ["facility_id"] = _state.FacilityId,
                        ["dead_quantity"] = projection.DeadQuantity.ToString(CultureInfo.InvariantCulture),
                        ["cause_event_id"] = eventId,
                    }, new[] { _state.FacilityId, "fact:live_fish_death" }, revision);
                result.DeathEventId = deathEventId;
            }
            return result;
        }

        public D11AquacultureResult Harvest(D11AquacultureHarvestRequest request)
        {
            if (request == null) return Reject("invalid_request", "活鱼舱出舱请求不存在。", 0, null);
            string eventId = EventId(request.EventId, "event_d11_live_fish_harvest_", request.WorldDay);
            long revision = Math.Max(0L, request.SaveRevision);
            var fish = D11AquacultureCatalog.FindFish(request.FishSpeciesId);
            if (fish == null) return Reject("fish_species_missing", "未知的活鱼物种。", request.WorldDay, eventId, revision);
            var existing = D11AquacultureMath.FindStock(_state, fish.SpeciesId);
            if (request.Quantity <= 0 || existing == null || existing.Quantity < request.Quantity)
                return Reject("not_enough_live_fish", "活鱼舱中没有足够的活鱼。", request.WorldDay, eventId, revision);
            if (_trade == null || _trade.Inventory == null)
                return Reject("inventory_unavailable", "库存系统不可用，不能生成鱼货。", request.WorldDay, eventId, revision);
            if (!D3InventorySystem.CanAddCargo(_trade.Inventory, fish.CargoItemId,
                request.Quantity, out string capacityError))
                return Reject(capacityError, "货舱没有足够空间接收鱼货。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(_state, _eventLog, eventId, revision))
                return Duplicate(eventId, revision, "活鱼出舱事实已经结算，未重复生成鱼货。");

            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                _state.LocationRef, Owner, new[] { _state.FacilityId },
                new[] { "live_fish:" + fish.SpeciesId }, Causes(), "live_fish_harvested",
                "活鱼已转为鱼货并进入货舱。", new Dictionary<string, string>
                {
                    ["facility_id"] = _state.FacilityId,
                    ["fish_species_id"] = fish.SpeciesId,
                    ["output_item_id"] = fish.CargoItemId,
                    ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                    ["weight"] = (request.Quantity * fish.UnitWeight).ToString(CultureInfo.InvariantCulture),
                }, new[] { _state.FacilityId, fish.CargoItemId }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, revision, "活鱼出舱事实已经结算，未重复生成鱼货。");

            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, fish.CargoItemId,
                request.Quantity, out string inventoryError))
                return RejectAfterFact("inventory_conflict", inventoryError, eventId, revision, append.Fact);
            D11AquacultureMath.RemoveFish(_state, fish.SpeciesId, request.Quantity);
            _state.Crowding = D11AquacultureMath.Crowding(_state);
            _state.LastWorldDay = Math.Max(_state.LastWorldDay, Math.Max(0, request.WorldDay));
            _state.LastEventId = eventId;
            D11AquacultureMath.Remember(_state, eventId, revision);
            return Success(append.Fact, eventId, request.WorldDay, request.Quantity, 0, 0, 0);
        }

        public D11AquacultureRuntimeSnapshot CaptureRuntime(D11IslandAquacultureState islandState = null)
        {
            return new D11AquacultureRuntimeSnapshot
            {
                LiveFishHold = Snapshot,
                IslandAquaculture = islandState == null ? new D11IslandAquacultureState() : islandState.Clone(),
                EventLog = _eventLog.CaptureSnapshot(),
            };
        }

        private IEnumerable<string> Causes()
        {
            return string.IsNullOrEmpty(_state.LastEventId)
                ? new string[0] : new[] { _state.LastEventId };
        }

        private static Dictionary<string, string> StateValues(D11AquacultureFacilityState state)
        {
            return new Dictionary<string, string>
            {
                ["facility_id"] = state.FacilityId ?? string.Empty,
                ["fish_quantity"] = state.FishQuantity.ToString(CultureInfo.InvariantCulture),
                ["weight"] = state.TotalWeight.ToString(CultureInfo.InvariantCulture),
                ["volume"] = state.UsedVolume.ToString(CultureInfo.InvariantCulture),
                ["oxygen_after"] = D11AquacultureMath.Format(state.Oxygen),
                ["water_quality_after"] = D11AquacultureMath.Format(state.WaterQuality),
                ["crowding_after"] = D11AquacultureMath.Format(state.Crowding),
                ["maintenance_after"] = D11AquacultureMath.Format(state.Maintenance),
            };
        }

        private D11AquacultureResult Success(FactEvent fact, string eventId, int day,
            int quantity, int dead, int feed, int labor)
        {
            return new D11AquacultureResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ok",
                Message = "活鱼舱状态已结算。",
                Fact = fact == null ? null : fact.CloneFact(),
                EventId = eventId,
                WorldDay = Math.Max(0, day),
                Quantity = quantity,
                DeadQuantity = dead,
                FeedConsumed = feed,
                LaborConsumed = labor,
                OxygenAfter = _state.Oxygen,
                WaterQualityAfter = _state.WaterQuality,
                CrowdingAfter = _state.Crowding,
                MaintenanceAfter = _state.Maintenance,
                TotalWeight = _state.TotalWeight,
                UsedVolume = _state.UsedVolume,
            };
        }

        private D11AquacultureResult Reject(string code, string message, int day, string eventId, long revision = 0)
        {
            return RejectWithFact(code, message, day, eventId, revision, null);
        }

        private D11AquacultureResult RejectAfterFact(string code, string message, string eventId,
            long revision, FactEvent fact)
        {
            return RejectWithFact(code, message, fact == null ? 0 : fact.WorldDay, eventId, revision, fact);
        }

        private D11AquacultureResult RejectWithFact(string code, string message, int day,
            string eventId, long revision, FactEvent fact)
        {
            return new D11AquacultureResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                EventId = eventId ?? string.Empty,
                WorldDay = Math.Max(0, day),
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private static D11AquacultureResult Duplicate(string eventId, long revision, string message)
        {
            return new D11AquacultureResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = message,
                EventId = eventId,
            };
        }

        private static D11AquacultureResult Duplicate(FactEvent fact, string eventId,
            long revision, string message)
        {
            var result = Duplicate(eventId, revision, message);
            result.Fact = fact == null ? null : fact.CloneFact();
            return result;
        }

        private static string EventId(string requested, string prefix, int day)
        {
            return string.IsNullOrWhiteSpace(requested)
                ? prefix + Math.Max(0, day).ToString(CultureInfo.InvariantCulture) : requested;
        }
    }

    internal static class D11AquacultureInventoryDefinitions
    {
        private static bool _registered;

        public static void Ensure()
        {
            if (_registered) return;
            D3InventorySystem.RegisterCargoDefinition(D11AquacultureIds.FryItemId, 1, 1);
            D3InventorySystem.RegisterCargoDefinition(D11AquacultureIds.FeedItemId, 1, 1);
            D3InventorySystem.RegisterCargoDefinition(D11AquacultureIds.FishCargoItemId, 1, 1);
            _registered = true;
        }
    }
}
