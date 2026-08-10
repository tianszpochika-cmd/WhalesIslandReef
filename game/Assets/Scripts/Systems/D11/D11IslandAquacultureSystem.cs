using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    /// <summary>
    /// 岛上鱼塘/近岸网箱的建筑与养殖唯一写者。建筑实例、鱼苗/饲料/劳力、
    /// 水体状态和鱼货结果都通过本系统提交；EcologySystem 只接收并裁决生态压力。
    /// </summary>
    public sealed class D11IslandAquacultureSystem
    {
        private const string Owner = "BuildingAquacultureSystem";
        private readonly D11IslandAquacultureState _state;
        private readonly D3TradeState _trade;
        private readonly D4EcologySystem _ecology;
        private readonly EventLog _eventLog;

        public D11IslandAquacultureSystem(D11IslandAquacultureState initial = null,
            D3TradeState trade = null, D4EcologySystem ecology = null, EventLog eventLog = null)
        {
            D11AquacultureInventoryDefinitions.Ensure();
            _state = initial == null ? new D11IslandAquacultureState() : initial.Clone();
            _trade = trade;
            _ecology = ecology;
            _eventLog = eventLog ?? new EventLog();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D11IslandAquacultureState Snapshot { get { return _state.Clone(); } }

        public D11AquacultureFacilityState FindFacilitySnapshot(string facilityId)
        {
            var facility = FindFacility(facilityId);
            return facility == null ? null : facility.Clone();
        }

        public D11AquacultureResult Build(D11AquacultureBuildRequest request)
        {
            if (request == null) return Reject("invalid_request", "养殖建筑请求不存在。", 0, null, 0);
            var definition = D11AquacultureCatalog.FindFacility(request.Kind);
            string facilityId = string.IsNullOrWhiteSpace(request.FacilityId)
                ? definition == null ? string.Empty : definition.Id : request.FacilityId;
            string eventId = EventId(request.EventId, "event_d11_aquaculture_build_", request.WorldDay, facilityId);
            long revision = Math.Max(0L, request.SaveRevision);
            if (definition == null) return Reject("facility_definition_missing", "未知的鱼塘/网箱建筑。", request.WorldDay, eventId, revision);
            if (string.IsNullOrWhiteSpace(facilityId)) return Reject("facility_id_missing", "养殖建筑缺少实例 ID。", request.WorldDay, eventId, revision);
            if (FindFacility(facilityId) != null)
                return Reject("facility_exists", "该养殖建筑实例已经存在。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(null, _eventLog, eventId, revision))
                return Duplicate(eventId, "养殖建筑事实已经结算，未重复创建建筑。");

            string location = string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef;
            string habitat = string.IsNullOrWhiteSpace(request.HabitatId) ? definition.HabitatId : request.HabitatId;
            var values = new Dictionary<string, string>
            {
                ["facility_id"] = facilityId,
                ["facility_kind"] = request.Kind.ToString(),
                ["location_ref"] = location,
                ["habitat_id"] = habitat,
                ["capacity_volume"] = definition.CapacityVolume.ToString(CultureInfo.InvariantCulture),
            };
            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                location, Owner, new[] { facilityId },
                new[] { "building:" + DefinitionId(request.Kind), "input:land_or_nearshore_right" },
                new string[0], "aquaculture_facility_built", "鱼塘/网箱建筑已登记。", values,
                new[] { facilityId, DefinitionId(request.Kind) }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, "养殖建筑事实已经结算，未重复创建建筑。");

            _state.Facilities.Add(new D11AquacultureFacilityState
            {
                SchemaVersion = D11AquacultureIds.SchemaVersion,
                FacilityId = facilityId,
                Kind = request.Kind,
                LocationRef = location,
                HabitatId = habitat,
                CapacityVolume = definition.CapacityVolume,
                UnitFishVolume = definition.UnitFishVolume,
                UnitFishWeight = definition.UnitFishWeight,
                WaterExchangePerDay = definition.WaterExchangePerDay,
                MaintenanceLaborPerDay = definition.MaintenanceLaborPerDay,
                MaxFish = definition.MaxFish,
                LastWorldDay = Math.Max(0, request.WorldDay),
                LastEventId = eventId,
                AppliedEventKeys = new List<string> { D11AquacultureMath.Key(eventId, revision) },
            });
            return Success(append.Fact, eventId, request.WorldDay, 0, 0, 0, 0, null, false);
        }

        public D11AquacultureResult SupplyInputs(D11AquacultureInputRequest request)
        {
            if (request == null) return Reject("invalid_request", "养殖输入请求不存在。", 0, null, 0);
            var facility = FindFacility(request.FacilityId);
            var fish = D11AquacultureCatalog.FindFish(request.FishSpeciesId);
            string eventId = EventId(request.EventId, "event_d11_aquaculture_inputs_", request.WorldDay, request.FacilityId);
            long revision = Math.Max(0L, request.SaveRevision);
            if (facility == null) return Reject("facility_missing", "未知的鱼塘/网箱实例。", request.WorldDay, eventId, revision);
            if (fish == null) return Reject("fish_species_missing", "未知的养殖鱼种。", request.WorldDay, eventId, revision);
            if (request.FryQuantity < 0 || request.FeedQuantity < 0 || request.LaborUnits < 0 ||
                request.FryQuantity == 0 && request.FeedQuantity == 0 && request.LaborUnits == 0)
                return Reject("invalid_inputs", "鱼苗、饲料或劳力输入必须提供正数。", request.WorldDay, eventId, revision);
            if (_trade == null || _trade.Inventory == null)
                return Reject("inventory_unavailable", "库存系统不可用，不能转移鱼苗或饲料。", request.WorldDay, eventId, revision);
            if (D3InventorySystem.CountCargo(_trade.Inventory, fish.FryItemId) < request.FryQuantity)
                return Reject("not_enough_fry", "货舱中没有足够的鱼苗。", request.WorldDay, eventId, revision);
            if (D3InventorySystem.CountCargo(_trade.Inventory, fish.FeedItemId) < request.FeedQuantity)
                return Reject("not_enough_feed", "货舱中没有足够的饲料。", request.WorldDay, eventId, revision);
            if (facility.FishQuantity + request.FryQuantity > facility.MaxFish ||
                request.FryQuantity * fish.UnitVolume > facility.FreeVolume)
                return Reject("facility_full", "鱼塘/网箱容量或数量已满。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(facility, _eventLog, eventId, revision))
                return Duplicate(eventId, "养殖输入事实已经结算，未重复扣减输入物资。");

            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                facility.LocationRef, Owner, new[] { facility.FacilityId },
                new[] { "input:" + fish.FryItemId, "input:" + fish.FeedItemId, "input:labor" },
                Causes(facility), "aquaculture_inputs_stocked", "鱼苗、饲料和劳力已进入养殖建筑。",
                new Dictionary<string, string>
                {
                    ["facility_id"] = facility.FacilityId,
                    ["fish_species_id"] = fish.SpeciesId,
                    ["fry_quantity"] = request.FryQuantity.ToString(CultureInfo.InvariantCulture),
                    ["feed_quantity"] = request.FeedQuantity.ToString(CultureInfo.InvariantCulture),
                    ["labor_units"] = request.LaborUnits.ToString(CultureInfo.InvariantCulture),
                }, new[] { facility.FacilityId, fish.SpeciesId }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, "养殖输入事实已经结算，未重复扣减输入物资。");

            string fryError = null;
            string feedError = null;
            bool fryRemoved = request.FryQuantity == 0 || D3InventorySystem.TryRemoveCargo(
                _trade.Inventory, fish.FryItemId, request.FryQuantity, out fryError);
            bool feedRemoved = request.FeedQuantity == 0 || D3InventorySystem.TryRemoveCargo(
                _trade.Inventory, fish.FeedItemId, request.FeedQuantity, out feedError);
            if (!fryRemoved || !feedRemoved)
            {
                if (fryRemoved && request.FryQuantity > 0)
                    D3InventorySystem.TryAddCargo(_trade.Inventory, fish.FryItemId, request.FryQuantity, out _);
                if (feedRemoved && request.FeedQuantity > 0)
                    D3InventorySystem.TryAddCargo(_trade.Inventory, fish.FeedItemId, request.FeedQuantity, out _);
                return RejectAfterFact("inventory_conflict", fryError ?? feedError, eventId, append.Fact);
            }
            D11AquacultureMath.AddFish(facility, fish, request.FryQuantity);
            facility.FeedStock = Math.Min(int.MaxValue - facility.FeedStock, facility.FeedStock + request.FeedQuantity);
            facility.LaborStock = Math.Min(int.MaxValue - facility.LaborStock, facility.LaborStock + request.LaborUnits);
            facility.LastWorldDay = Math.Max(facility.LastWorldDay, Math.Max(0, request.WorldDay));
            facility.LastEventId = eventId;
            facility.Crowding = D11AquacultureMath.Crowding(facility);
            D11AquacultureMath.Remember(facility, eventId, revision);
            return Success(append.Fact, eventId, request.WorldDay, request.FryQuantity, 0, 0, 0, null, false);
        }

        public D11AquacultureResult Advance(D11AquacultureAdvanceRequest request)
        {
            if (request == null) return Reject("invalid_request", "养殖维护请求不存在。", 0, null, 0);
            var facility = FindFacility(request.FacilityId);
            string eventId = EventId(request.EventId, "event_d11_aquaculture_advance_", request.WorldDay, request.FacilityId);
            long revision = Math.Max(0L, request.SaveRevision);
            if (facility == null) return Reject("facility_missing", "未知的鱼塘/网箱实例。", request.WorldDay, eventId, revision);
            if (_ecology == null) return Reject("ecology_unavailable", "EcologySystem 不可用，不能裁决生态压力。", request.WorldDay, eventId, revision);
            if (request.WorldDay < facility.LastWorldDay)
                return Reject("world_day_rewind", "养殖建筑世界日不能倒退。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(facility, _eventLog, eventId, revision))
                return Duplicate(eventId, "养殖维护事实已经结算，未重复消耗饲料或劳力。");
            var definition = D11AquacultureCatalog.FindFacility(facility.Kind);
            var projection = D11AquacultureMath.Advance(facility, definition, D11AquacultureCatalog.Mackerel,
                request.WorldDay, request.WaterExchangeUnits, 0, request.Seed, true);
            var ecologyPreview = _ecology.PreviewAquaculturePressure(facility.FacilityId,
                facility.HabitatId, projection.State.FishQuantity, projection.FeedConsumed,
                0, projection.DeadQuantity, request.WorldDay);
            var values = StateValues(projection.State);
            values["days_applied"] = projection.DaysApplied.ToString(CultureInfo.InvariantCulture);
            values["dead_quantity"] = projection.DeadQuantity.ToString(CultureInfo.InvariantCulture);
            values["feed_consumed"] = projection.FeedConsumed.ToString(CultureInfo.InvariantCulture);
            values["labor_consumed"] = projection.LaborConsumed.ToString(CultureInfo.InvariantCulture);
            values["ecology_pressure_points"] = ecologyPreview.Quantity.ToString(CultureInfo.InvariantCulture);
            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                facility.LocationRef, Owner, new[] { facility.FacilityId },
                new[] { "input:feed", "input:labor", "maintenance:water_exchange" }, Causes(facility),
                "aquaculture_maintained", "养殖建筑维护、水质、氧气、拥挤和死亡率已结算。", values,
                new[] { facility.FacilityId, "fact:aquaculture_maintenance" }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, "养殖维护事实已经结算，未重复消耗饲料或劳力。");

            facility.CopyFrom(projection.State);
            facility.LastEventId = eventId;
            D11AquacultureMath.Remember(facility, eventId, revision);
            var ecology = _ecology.ApplyAquaculturePressure(facility.FacilityId, facility.HabitatId,
                projection.State.FishQuantity, projection.FeedConsumed, 0, projection.DeadQuantity,
                request.WorldDay, eventId, revision);
            var result = Success(append.Fact, eventId, request.WorldDay, facility.FishQuantity,
                projection.DeadQuantity, projection.FeedConsumed, projection.LaborConsumed, ecology);
            if (projection.DeadQuantity > 0)
                result.DeathEventId = AppendDeathEvent(facility, request.WorldDay, revision, eventId, projection.DeadQuantity);
            result.EcologyEventId = eventId;
            return result;
        }

        public D11AquacultureResult Harvest(D11AquacultureHarvestRequest request)
        {
            if (request == null) return Reject("invalid_request", "养殖鱼货请求不存在。", 0, null, 0);
            var facility = FindFacility(request.FacilityId);
            var fish = D11AquacultureCatalog.FindFish(request.FishSpeciesId);
            string eventId = EventId(request.EventId, "event_d11_aquaculture_harvest_", request.WorldDay, request.FacilityId);
            long revision = Math.Max(0L, request.SaveRevision);
            if (facility == null) return Reject("facility_missing", "未知的鱼塘/网箱实例。", request.WorldDay, eventId, revision);
            if (fish == null) return Reject("fish_species_missing", "未知的养殖鱼种。", request.WorldDay, eventId, revision);
            if (request.Quantity <= 0 || D11AquacultureMath.OccupiedBySpecies(facility, fish.SpeciesId) < request.Quantity)
                return Reject("not_enough_fish", "养殖建筑中没有足够的鱼可收获。", request.WorldDay, eventId, revision);
            if (_trade == null || _trade.Inventory == null)
                return Reject("inventory_unavailable", "库存系统不可用，不能生成鱼货。", request.WorldDay, eventId, revision);
            if (!D3InventorySystem.CanAddCargo(_trade.Inventory, fish.CargoItemId,
                request.Quantity, out string capacityError))
                return Reject(capacityError, "货舱没有足够空间接收鱼货。", request.WorldDay, eventId, revision);
            if (_ecology == null) return Reject("ecology_unavailable", "EcologySystem 不可用，不能裁决生态压力。", request.WorldDay, eventId, revision);
            if (D11AquacultureMath.IsDuplicate(facility, _eventLog, eventId, revision))
                return Duplicate(eventId, "养殖收获事实已经结算，未重复生成鱼货。");

            var ecologyPreview = _ecology.PreviewAquaculturePressure(facility.FacilityId,
                facility.HabitatId, facility.FishQuantity, 0, request.Quantity, 0, request.WorldDay);
            var append = D11AquacultureMath.Append(_eventLog, eventId, request.WorldDay,
                facility.LocationRef, Owner, new[] { facility.FacilityId },
                new[] { "live_fish:" + fish.SpeciesId }, Causes(facility), "aquaculture_fish_harvested",
                "养殖鱼货已由 InventorySystem 写入货舱。", new Dictionary<string, string>
                {
                    ["facility_id"] = facility.FacilityId,
                    ["fish_species_id"] = fish.SpeciesId,
                    ["output_item_id"] = fish.CargoItemId,
                    ["quantity"] = request.Quantity.ToString(CultureInfo.InvariantCulture),
                    ["weight"] = (request.Quantity * fish.UnitWeight).ToString(CultureInfo.InvariantCulture),
                    ["ecology_pressure_points"] = ecologyPreview.Quantity.ToString(CultureInfo.InvariantCulture),
                }, new[] { facility.FacilityId, fish.CargoItemId }, revision);
            if (append.IsDuplicate) return Duplicate(append.Fact, eventId, "养殖收获事实已经结算，未重复生成鱼货。");

            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, fish.CargoItemId,
                request.Quantity, out string inventoryError))
                return RejectAfterFact("inventory_conflict", inventoryError, eventId, append.Fact);
            int removed = D11AquacultureMath.RemoveFish(facility, fish.SpeciesId, request.Quantity);
            if (removed != request.Quantity)
            {
                D3InventorySystem.TryRemoveCargo(_trade.Inventory, fish.CargoItemId, request.Quantity, out _);
                return RejectAfterFact("facility_conflict", "养殖状态在结算期间发生变化。", eventId, append.Fact);
            }
            facility.Crowding = D11AquacultureMath.Crowding(facility);
            facility.LastWorldDay = Math.Max(facility.LastWorldDay, Math.Max(0, request.WorldDay));
            facility.LastEventId = eventId;
            D11AquacultureMath.Remember(facility, eventId, revision);
            var ecology = _ecology.ApplyAquaculturePressure(facility.FacilityId, facility.HabitatId,
                facility.FishQuantity, 0, request.Quantity, 0, request.WorldDay, eventId, revision);
            var result = Success(append.Fact, eventId, request.WorldDay, request.Quantity,
                0, 0, 0, ecology, true);
            result.OutputQuantity = request.Quantity;
            result.EcologyEventId = eventId;
            return result;
        }

        public D11AquacultureRuntimeSnapshot CaptureRuntime(D11LiveFishHoldState liveFishHold = null)
        {
            return new D11AquacultureRuntimeSnapshot
            {
                LiveFishHold = liveFishHold == null ? new D11LiveFishHoldState() : liveFishHold.Clone(),
                IslandAquaculture = Snapshot,
                EventLog = _eventLog.CaptureSnapshot(),
            };
        }

        private D11AquacultureFacilityState FindFacility(string facilityId)
        {
            if (string.IsNullOrEmpty(facilityId) || _state.Facilities == null) return null;
            foreach (var facility in _state.Facilities)
                if (facility != null && string.Equals(facility.FacilityId, facilityId, StringComparison.Ordinal)) return facility;
            return null;
        }

        private static IEnumerable<string> Causes(D11AquacultureFacilityState facility)
        {
            return facility == null || string.IsNullOrEmpty(facility.LastEventId)
                ? new string[0] : new[] { facility.LastEventId };
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

        private string AppendDeathEvent(D11AquacultureFacilityState facility, int day,
            long revision, string causeEventId, int deadQuantity)
        {
            string deathEventId = "event_d11_aquaculture_death_" + facility.FacilityId + "_" +
                Math.Max(0, day).ToString(CultureInfo.InvariantCulture);
            D11AquacultureMath.Append(_eventLog, deathEventId, day, facility.LocationRef, Owner,
                new[] { facility.FacilityId }, new[] { "fact:aquaculture_maintenance" },
                new[] { causeEventId }, "aquaculture_fish_died", "养殖建筑结算记录了鱼只死亡。",
                new Dictionary<string, string>
                {
                    ["facility_id"] = facility.FacilityId,
                    ["dead_quantity"] = deadQuantity.ToString(CultureInfo.InvariantCulture),
                    ["cause_event_id"] = causeEventId,
                }, new[] { facility.FacilityId, "fact:aquaculture_death" }, revision);
            return deathEventId;
        }

        private D11AquacultureResult Success(FactEvent fact, string eventId, int day,
            int quantity, int dead, int feed, int labor, D11EcologyOperationResult ecology,
            bool outputQuantity = false)
        {
            var facility = FindFacilityByFact(fact);
            return new D11AquacultureResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ok",
                Message = "岛上水产状态已结算。",
                Fact = fact == null ? null : fact.CloneFact(),
                EventId = eventId,
                EcologyEventId = ecology == null ? null : eventId,
                WorldDay = Math.Max(0, day),
                Quantity = quantity,
                DeadQuantity = dead,
                FeedConsumed = feed,
                LaborConsumed = labor,
                OutputQuantity = outputQuantity ? quantity : 0,
                TotalWeight = facility == null ? 0 : facility.TotalWeight,
                UsedVolume = facility == null ? 0 : facility.UsedVolume,
                OxygenAfter = facility == null ? 0f : facility.Oxygen,
                WaterQualityAfter = facility == null ? 0f : facility.WaterQuality,
                CrowdingAfter = facility == null ? 0f : facility.Crowding,
                MaintenanceAfter = facility == null ? 0f : facility.Maintenance,
            };
        }

        private D11AquacultureFacilityState FindFacilityByFact(FactEvent fact)
        {
            if (fact == null || fact.Result == null || fact.Result.Values == null ||
                !fact.Result.Values.TryGetValue("facility_id", out string facilityId)) return null;
            return FindFacility(facilityId);
        }

        private D11AquacultureResult Reject(string code, string message, int day, string eventId, long revision)
        {
            return new D11AquacultureResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                EventId = eventId ?? string.Empty,
                WorldDay = Math.Max(0, day),
            };
        }

        private D11AquacultureResult RejectAfterFact(string code, string message, string eventId, FactEvent fact)
        {
            return new D11AquacultureResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                EventId = eventId,
                WorldDay = fact == null ? 0 : fact.WorldDay,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private static D11AquacultureResult Duplicate(string eventId, string message)
        {
            return new D11AquacultureResult { Succeeded = true, Duplicate = true,
                Code = "duplicate_settlement", Message = message, EventId = eventId };
        }

        private static D11AquacultureResult Duplicate(FactEvent fact, string eventId, string message)
        {
            var result = Duplicate(eventId, message);
            result.Fact = fact == null ? null : fact.CloneFact();
            return result;
        }

        private static string DefinitionId(D11AquacultureFacilityKind kind)
        {
            var definition = D11AquacultureCatalog.FindFacility(kind);
            return definition == null ? "building_unknown" : definition.Id;
        }

        private static string EventId(string requested, string prefix, int day, string facilityId)
        {
            return string.IsNullOrWhiteSpace(requested)
                ? prefix + (facilityId ?? "unknown") + "_" + Math.Max(0, day).ToString(CultureInfo.InvariantCulture)
                : requested;
        }
    }
}
