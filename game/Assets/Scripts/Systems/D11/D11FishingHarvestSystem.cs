// ============================================================
// D11FishingHarvestSystem —— D11-A02 捕捞与浅水采集编排
//
// 本类只编排意图和跨 owner 事务：
//   EcologySystem：鱼群/浅水资源数量、压力、保护区与质量预览；
//   D3InventorySystem：货舱物品增删；TradeEconomySystem：价格、库存和金币；
//   EventLog：只保存已完成事实，不负责裁决。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Game.Config;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public sealed class D11FishingHarvestSystem
    {
        public const string HarvestEventId = "event_d11_fishing_harvest";
        public const string ResourceEventId = "event_d11_shallow_resource_harvest";
        public const string ProcessingEventId = "event_d11_fish_processing";
        public const string ProtectedRejectEventId = "event_d11_harvest_rejected_protected";

        private const string EcologyOwner = "EcologySystem";
        private const string InventoryOwner = "InventorySystem";
        private readonly D3TradeState _trade;
        private readonly D4EcologySystem _ecology;
        private readonly TradeEconomySystem _economy;
        private readonly EventLog _eventLog;
        private long _nextHarvestRevision;
        private long _nextProcessingRevision;
        private D11HarvestResult _lastHarvest;

        public D11FishingHarvestSystem(D3TradeState trade, D4EcologySystem ecology,
            TradeEconomySystem economy = null, EventLog eventLog = null)
        {
            _trade = trade;
            _ecology = ecology;
            _economy = economy ?? new TradeEconomySystem(eventLog);
            _eventLog = eventLog ?? _economy.EventLog ?? new EventLog();
            RegisterMarketDefinitions();
            ReadNextRevisionsFromLog();
            D11FishingHarvestConfiguration.ThrowIfInvalid();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D11HarvestResult LastHarvest { get { return _lastHarvest == null ? null : _lastHarvest.Clone(); } }
        public EcologyState EcologySnapshot { get { return _ecology == null ? null : _ecology.Snapshot; } }
        public D4EcologySystem EcologySystem { get { return _ecology; } }
        public IReadOnlyList<D11FishingGearDefinition> Gears { get { return D11FishingHarvestCatalog.Gears; } }
        public IReadOnlyList<D11FishingLocationDefinition> Locations { get { return D11FishingHarvestCatalog.Locations; } }

        public D11HarvestResult Harvest(D11HarvestRequest request, D9WeatherSeaSnapshot weather = null)
        {
            if (request == null) return Reject("invalid_request", "采集请求不存在。", 0);
            var location = D11FishingHarvestCatalog.FindLocation(request.LocationId);
            var gear = D11FishingHarvestCatalog.FindGear(request.GearId);
            long revision = request.SaveRevision > 0 ? request.SaveRevision : _nextHarvestRevision + 1;
            if (_trade == null || _ecology == null || location == null || gear == null)
                return Reject("invalid_reference", "采集点、渔具或运行状态不存在。", revision);

            var duplicate = _eventLog.Find(HarvestEventId, revision);
            if (duplicate != null) return Duplicate(duplicate, revision);

            if (location.IsProtectedArea && !request.HasConservationPermit)
                return RejectProtected(location, gear, request, revision);
            if (!GearCanUseLocation(gear, location))
                return Reject("gear_incompatible", "当前渔具不能用于这个采集点。", revision);

            float depth = request.DepthMeters > 0f ? request.DepthMeters : location.PreferredDepthMeters;
            if (depth > gear.MaximumDepthMeters)
                return Reject("gear_depth_limit", "当前渔具无法到达请求深度。", revision);
            string time = CanonicalTime(request.TimeOfDayBucket, location.PreferredTimeOfDay);
            string tide = CanonicalTide(request.TideBand, location.PreferredTideBand);
            var safeWeather = weather == null ? D9WeatherSeaSnapshot.CreateDefault() : weather.Clone();

            if (location.IsFishLocation)
                return HarvestFish(request, location, gear, safeWeather, depth, time, tide, revision);
            return HarvestShallowResource(request, location, gear, depth, time, tide, revision);
        }

        public D11ProcessingResult Process(string inputItemId, int quantity,
            D11ProcessingMethod method, long saveRevision = 0)
        {
            long revision = saveRevision > 0 ? saveRevision : _nextProcessingRevision + 1;
            var definition = D11FishingHarvestCatalog.FindProcessing(inputItemId, method);
            if (_trade == null || definition == null)
                return ProcessReject("invalid_processing_reference", "处理定义或贸易状态不存在。", revision);
            var duplicate = _eventLog.Find(ProcessingEventId, revision);
            if (duplicate != null) return ProcessDuplicate(duplicate, revision);
            if (quantity <= 0) return ProcessReject("invalid_quantity", "处理数量必须大于零。", revision);
            if (D3InventorySystem.CountCargo(_trade.Inventory, definition.InputItemId) < quantity)
                return ProcessReject("not_enough_input", "货舱中没有足够的待处理物品。", revision);

            int outputQuantity = definition.OutputFor(quantity);
            if (!D3InventorySystem.CanAddCargo(_trade.Inventory, definition.OutputItemId,
                    outputQuantity, out string capacityError))
                return ProcessReject(capacityError, "处理结果没有足够货舱空间。", revision);
            if (!D3InventorySystem.TryRemoveCargo(_trade.Inventory, definition.InputItemId,
                    quantity, out string removeError))
                return ProcessReject(removeError, "待处理物品无法从货舱取出。", revision);
            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, definition.OutputItemId,
                    outputQuantity, out string addError))
            {
                D3InventorySystem.TryAddCargo(_trade.Inventory, definition.InputItemId, quantity, out _);
                return ProcessReject(addError, "处理结果无法装回货舱。", revision);
            }

            _nextProcessingRevision = Math.Max(_nextProcessingRevision, revision);
            var result = new D11ProcessingResult
            {
                Succeeded = true,
                Code = "processed",
                Message = definition.DisplayName + "已完成。",
                Method = method,
                InputItemId = definition.InputItemId,
                OutputItemId = definition.OutputItemId,
                InputQuantity = quantity,
                OutputQuantity = outputQuantity,
                CargoUsed = D3InventorySystem.UsedCargo(_trade.Inventory),
                EventId = ProcessingEventId,
                SaveRevision = revision,
            };
            var append = _eventLog.Append(BuildProcessingEvent(result));
            result.Fact = append.Fact;
            result.Duplicate = append.IsDuplicate;
            return result;
        }

        /// <summary>交易只委托 EconomySystem；市场反馈通过 TradeResult.PriceReasons 返回。</summary>
        public TradeResult Sell(string portId, string itemId, int quantity, int worldDay = 0,
            D3WeatherType weather = D3WeatherType.Calm, long saveRevision = 0)
        {
            if (_trade == null || _economy == null)
                return TradeResult.Fail("invalid_trade_state", "交易状态不存在。", 0);
            if (D11FishingHarvestCatalog.FindMarketItem(itemId) == null)
                return TradeResult.Fail("invalid_harvest_item", "该物品不是 D11 捕捞/采集市场物品。", _trade.Money);
            return _economy.Sell(_trade, portId, itemId, quantity,
                new TradePriceContext { WorldDay = Math.Max(0, worldDay), Weather = weather }, saveRevision);
        }

        public D11FishingHarvestRuntimeSnapshot CaptureSnapshot()
        {
            return new D11FishingHarvestRuntimeSnapshot
            {
                NextHarvestRevision = _nextHarvestRevision,
                NextProcessingRevision = _nextProcessingRevision,
                LastHarvest = _lastHarvest == null ? null : _lastHarvest.Clone(),
                EventLog = _eventLog.CaptureSnapshot(),
            };
        }

        public void Restore(D11FishingHarvestRuntimeSnapshot snapshot)
        {
            if (snapshot == null) return;
            _nextHarvestRevision = Math.Max(0, snapshot.NextHarvestRevision);
            _nextProcessingRevision = Math.Max(0, snapshot.NextProcessingRevision);
            _lastHarvest = snapshot.LastHarvest == null ? null : snapshot.LastHarvest.Clone();
            if (snapshot.EventLog != null) _eventLog.Restore(snapshot.EventLog);
            ReadNextRevisionsFromLog();
        }

        private D11HarvestResult HarvestFish(D11HarvestRequest request,
            D11FishingLocationDefinition location, D11FishingGearDefinition gear,
            D9WeatherSeaSnapshot weather, float depth, string time, string tide, long revision)
        {
            string itemId = D11FishingHarvestCatalog.FreshItemForSpecies(location.FishSpeciesId);
            if (string.IsNullOrEmpty(itemId)) return Reject("fish_item_missing", "鱼种没有正式鱼获物品。", revision);

            _ecology.SeedFishSchool(location.FishSchoolId, location.FishSpeciesId,
                location.Id, location.PreferredDepthMeters, "nearshore",
                (int)D11FishingHarvestConfiguration.InitialFishSchoolCapacity.DefaultValue,
                request.Day, request.Seed);
            var preview = _ecology.PreviewFishHarvest(location.FishSchoolId, location.Id, itemId,
                gear.CatchCapacity, request.Day, request.Seed, weather, location.FishSpeciesId,
                gear.Id, depth, time, tide, location.IsProtectedArea,
                request.HasConservationPermit, location.ProtectionRuleId);
            if (!preview.Accepted) return Reject(preview.Code, preview.Message, revision, location, gear, preview);
            if (!D3InventorySystem.CanAddCargo(_trade.Inventory, preview.FishItemId, preview.Quantity,
                    out string capacityError))
                return Reject(capacityError, capacityError == "weight_full" ? "鱼获重量超出限额。" : "货舱没有足够空间。", revision);
            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, preview.FishItemId, preview.Quantity,
                    out string inventoryError))
                return Reject(inventoryError, "鱼获无法装入货舱。", revision);
            if (!_ecology.CommitFishCatch(preview))
            {
                D3InventorySystem.TryRemoveCargo(_trade.Inventory, preview.FishItemId, preview.Quantity, out _);
                return Reject("ecology_conflict", "鱼群状态已变化，本次捕捞未结算。", revision);
            }

            _nextHarvestRevision = Math.Max(_nextHarvestRevision, revision);
            var result = BuildHarvestResult(request, location, gear, preview, revision);
            var append = _eventLog.Append(BuildHarvestEvent(result));
            result.Fact = append.Fact;
            result.Duplicate = append.IsDuplicate;
            _lastHarvest = result.Clone();
            return result;
        }

        private D11HarvestResult HarvestShallowResource(D11HarvestRequest request,
            D11FishingLocationDefinition location, D11FishingGearDefinition gear,
            float depth, string time, string tide, long revision)
        {
            _ecology.EnsureD11ShallowResources(request.Day, request.Seed);
            var resource = _ecology.FindPlantSnapshot(location.ResourceId);
            if (resource == null) return Reject("resource_missing", "浅水资源不存在。", revision);
            string resourceKind = D11FishingHarvestCatalog.ResourceKindForResourceId(location.ResourceId);
            if (resourceKind == "plant") resourceKind = resource.ResourceKind ?? "plant";
            string itemId = D11FishingHarvestCatalog.ItemForResourceKind(resourceKind);
            int requested = Math.Min(gear.CatchCapacity, Math.Max(0, resource.Quantity));
            if (requested <= 0) return Reject("resource_depleted", "该浅水资源当前没有可采集数量。", revision);
            if (!D3InventorySystem.CanAddCargo(_trade.Inventory, itemId, requested,
                    out string capacityError))
                return Reject(capacityError, "采集物没有足够货舱空间。", revision);

            var operation = _ecology.HarvestShallowResource(location.ResourceId, requested,
                request.Day, request.Seed, _eventLog, revision, ResourceEventId);
            if (!operation.Succeeded)
                return Reject(operation.Code, operation.Message, revision);
            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, itemId, operation.Quantity,
                    out string inventoryError))
                return Reject(inventoryError, "采集物无法装入货舱。", revision);

            _nextHarvestRevision = Math.Max(_nextHarvestRevision, revision);
            var result = new D11HarvestResult
            {
                Succeeded = true,
                Code = "shallow_resource_harvested",
                Message = resourceKind == "shellfish" ? "浅水贝类已采集并入库。" : "浅水海草已采集并入库。",
                LocationId = location.Id,
                ResourceId = location.ResourceId,
                ResourceKind = resourceKind,
                GearId = gear.Id,
                Method = gear.Method,
                WorldDay = Math.Max(0, request.Day),
                ItemId = itemId,
                Quantity = operation.Quantity,
                Quality = QualityGrade.Standard,
                QualityScore = 50,
                DepthMeters = depth,
                TimeOfDayBucket = time,
                TideBand = tide,
                ProtectionRuleId = location.ProtectionRuleId ?? string.Empty,
                FishingPressureAfter = operation.HarvestPressureAfter,
                EventId = HarvestEventId,
                SaveRevision = revision,
            };
            var append = _eventLog.Append(BuildHarvestEvent(result));
            result.Fact = append.Fact;
            result.Duplicate = append.IsDuplicate;
            _lastHarvest = result.Clone();
            return result;
        }

        private D11HarvestResult RejectProtected(D11FishingLocationDefinition location,
            D11FishingGearDefinition gear, D11HarvestRequest request, long revision)
        {
            string eventId = ProtectedRejectEventId;
            var fact = new WorldEvent(eventId, Math.Max(0, request.Day), location.Id, EcologyOwner,
                new EventResult
                {
                    ResultCode = "protected_area_blocked",
                    Summary = "保护区规则阻止了本次采集请求。",
                    FactIds = new List<string> { location.Id, location.ProtectionRuleId ?? "protected_area" },
                    Values = new Dictionary<string, string>
                    {
                        ["protection_rule_id"] = location.ProtectionRuleId ?? string.Empty,
                        ["gear_id"] = gear.Id,
                        ["has_conservation_permit"] = request.HasConservationPermit.ToString(),
                    },
                }, new[] { gear.Id }, new[] { location.Id }, new[] { location.ProtectionRuleId ?? "protected_area" }, revision);
            var append = _eventLog.Append(fact);
            return new D11HarvestResult
            {
                Succeeded = false,
                Code = "protected_area_no_permit",
                Message = "保护区没有有效采集许可。",
                LocationId = location.Id,
                GearId = gear.Id,
                Method = gear.Method,
                ProtectionRuleId = location.ProtectionRuleId ?? string.Empty,
                EventId = eventId,
                SaveRevision = revision,
                Fact = append.Fact,
            };
        }

        private void RegisterMarketDefinitions()
        {
            foreach (var item in D11FishingHarvestCatalog.MarketItems)
            {
                _economy.RegisterMarketItem(item);
                if (_trade != null) _economy.EnsureMarketItem(_trade.Market, item.Id);
            }
        }

        private void ReadNextRevisionsFromLog()
        {
            if (_eventLog == null || _eventLog.Facts == null) return;
            foreach (var fact in _eventLog.Facts)
            {
                if (fact == null) continue;
                if (fact.EventId == HarvestEventId) _nextHarvestRevision = Math.Max(_nextHarvestRevision, fact.SaveRevision);
                if (fact.EventId == ProcessingEventId) _nextProcessingRevision = Math.Max(_nextProcessingRevision, fact.SaveRevision);
            }
        }

        private static bool GearCanUseLocation(D11FishingGearDefinition gear, D11FishingLocationDefinition location)
        {
            if (location.IsFishLocation)
                return gear.Method == D11HarvestMethod.Handline || gear.Method == D11HarvestMethod.ShoreFishing ||
                    gear.Method == D11HarvestMethod.CastNet || gear.Method == D11HarvestMethod.FishTrap;
            return gear.Method == location.Method;
        }

        private static D11HarvestResult BuildHarvestResult(D11HarvestRequest request,
            D11FishingLocationDefinition location, D11FishingGearDefinition gear,
            FishCatchPreview preview, long revision)
        {
            return new D11HarvestResult
            {
                Succeeded = true,
                Code = "fish_harvested",
                Message = "已按生态预览结算鱼获并入库。",
                LocationId = location.Id,
                FishSchoolId = preview.FishSchoolId,
                FishSpeciesId = preview.FishSpeciesId,
                GearId = gear.Id,
                Method = gear.Method,
                WorldDay = Math.Max(0, request.Day),
                ItemId = preview.FishItemId,
                Quantity = preview.Quantity,
                Quality = preview.Quality,
                QualityScore = preview.QualityScore,
                DepthMeters = preview.DepthMeters,
                TimeOfDayBucket = preview.TimeOfDayBucket,
                TideBand = preview.TideBand,
                DepthMultiplier = preview.DepthMultiplier,
                TimeMultiplier = preview.TimeMultiplier,
                TideMultiplier = preview.TideMultiplier,
                GearMultiplier = preview.GearMultiplier,
                FishingPressureAfter = preview.FishingPressureAfter,
                ProtectionRuleId = preview.ProtectionRuleId ?? string.Empty,
                EventId = HarvestEventId,
                SaveRevision = revision,
            };
        }

        private static WorldEvent BuildHarvestEvent(D11HarvestResult result)
        {
            var factIds = new List<string> { result.LocationId, result.ItemId };
            if (!string.IsNullOrEmpty(result.FishSchoolId)) factIds.Add(result.FishSchoolId);
            if (!string.IsNullOrEmpty(result.ResourceId)) factIds.Add(result.ResourceId);
            return new WorldEvent(
                HarvestEventId, Math.Max(0, result.WorldDay), result.LocationId, EcologyOwner,
                new EventResult
                {
                    ResultCode = result.Code,
                    Summary = result.Message,
                    FactIds = factIds,
                    Values = new Dictionary<string, string>
                    {
                        ["fish_school_id"] = result.FishSchoolId ?? string.Empty,
                        ["fish_species_id"] = result.FishSpeciesId ?? string.Empty,
                        ["resource_id"] = result.ResourceId ?? string.Empty,
                        ["resource_kind"] = result.ResourceKind ?? string.Empty,
                        ["item_id"] = result.ItemId ?? string.Empty,
                        ["quantity"] = result.Quantity.ToString(CultureInfo.InvariantCulture),
                        ["gear_id"] = result.GearId ?? string.Empty,
                        ["method"] = result.Method.ToString(),
                        ["quality"] = result.Quality.ToString(),
                        ["quality_score"] = result.QualityScore.ToString(CultureInfo.InvariantCulture),
                        ["depth_meters"] = result.DepthMeters.ToString("0.###", CultureInfo.InvariantCulture),
                        ["time_of_day"] = result.TimeOfDayBucket ?? string.Empty,
                        ["tide_band"] = result.TideBand ?? string.Empty,
                        ["fishing_pressure_after"] = result.FishingPressureAfter.ToString(CultureInfo.InvariantCulture),
                        ["protection_rule_id"] = result.ProtectionRuleId ?? string.Empty,
                    },
                }, new[] { result.GearId ?? string.Empty }, factIds, new string[0], result.SaveRevision);
        }

        private static WorldEvent BuildProcessingEvent(D11ProcessingResult result)
        {
            return new WorldEvent(
                ProcessingEventId, 0, "cargo", InventoryOwner,
                new EventResult
                {
                    ResultCode = "processed",
                    Summary = result.Message,
                    FactIds = new List<string> { result.InputItemId, result.OutputItemId },
                    Values = new Dictionary<string, string>
                    {
                        ["method"] = result.Method.ToString(),
                        ["input_item_id"] = result.InputItemId,
                        ["input_quantity"] = result.InputQuantity.ToString(CultureInfo.InvariantCulture),
                        ["output_item_id"] = result.OutputItemId,
                        ["output_quantity"] = result.OutputQuantity.ToString(CultureInfo.InvariantCulture),
                    },
                }, new[] { result.InputItemId }, new[] { result.InputItemId, result.OutputItemId },
                new string[0], result.SaveRevision);
        }

        private static D11HarvestResult Duplicate(FactEvent fact, long revision)
        {
            var values = fact == null || fact.Result == null ? null : fact.Result.Values;
            QualityGrade quality = QualityGrade.Unknown;
            if (values != null) Enum.TryParse(Read(values, "quality"), true, out quality);
            return new D11HarvestResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = "该捕捞/采集事实已结算，未重复增加鱼获。",
                ItemId = Read(values, "item_id"),
                FishSchoolId = Read(values, "fish_school_id"),
                ResourceId = Read(values, "resource_id"),
                Quantity = ReadInt(values, "quantity"),
                Quality = quality,
                SaveRevision = revision,
                EventId = HarvestEventId,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private static D11ProcessingResult ProcessDuplicate(FactEvent fact, long revision)
        {
            var values = fact == null || fact.Result == null ? null : fact.Result.Values;
            D11ProcessingMethod method = D11ProcessingMethod.Salt;
            if (values != null) Enum.TryParse(Read(values, "method"), true, out method);
            return new D11ProcessingResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate_processing",
                Message = "该处理事实已结算，未重复消耗原料。",
                Method = method,
                InputItemId = Read(values, "input_item_id"),
                OutputItemId = Read(values, "output_item_id"),
                InputQuantity = ReadInt(values, "input_quantity"),
                OutputQuantity = ReadInt(values, "output_quantity"),
                EventId = ProcessingEventId,
                SaveRevision = revision,
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private static D11HarvestResult Reject(string code, string message, long revision,
            D11FishingLocationDefinition location = null, D11FishingGearDefinition gear = null,
            FishCatchPreview preview = null)
        {
            return new D11HarvestResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                LocationId = location == null ? string.Empty : location.Id,
                GearId = gear == null ? string.Empty : gear.Id,
                ProtectionRuleId = preview == null ? string.Empty : preview.ProtectionRuleId,
                EventId = HarvestEventId,
                SaveRevision = revision,
            };
        }

        private static D11ProcessingResult ProcessReject(string code, string message, long revision)
        {
            return new D11ProcessingResult
            {
                Succeeded = false, Code = code, Message = message,
                SaveRevision = revision, EventId = ProcessingEventId,
            };
        }

        private static string CanonicalTime(string value, string fallback)
        {
            string result = (string.IsNullOrEmpty(value) ? fallback : value).ToLowerInvariant();
            return result == "dawn" || result == "day" || result == "dusk" || result == "night" ? result : "day";
        }

        private static string CanonicalTide(string value, string fallback)
        {
            string result = (string.IsNullOrEmpty(value) ? fallback : value).ToLowerInvariant();
            return result == "flood" || result == "ebb" || result == "mid" ? result : "mid";
        }

        private static string Read(Dictionary<string, string> values, string key)
        {
            return values != null && values.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private static int ReadInt(Dictionary<string, string> values, string key)
        {
            return int.TryParse(Read(values, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }
    }
}
