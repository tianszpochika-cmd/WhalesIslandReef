// ============================================================
// D9FishingSystem —— D9-A02 捕捞闭环编排
//
// 本类接收玩家/表现层意图，但不自行计算鱼群位置、鱼获数量或价格：
//   - D4EcologySystem 预览并提交鱼群生物量、压力和位置；
//   - D3InventorySystem 写入货舱/随身物品数量；
//   - TradeEconomySystem 写入价格、市场库存和金币；
//   - EventLog 只记录已完成的 EcologySystem 事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    public sealed class D9FishingSystem
    {
        // Production fact owner: EcologySystem; frozen event_id = event_d9_fishing_catch.
        private const string CatchEventId = "event_d9_fishing_catch";
        private readonly D3TradeState _trade;
        private readonly D4EcologySystem _ecology;
        private readonly TradeEconomySystem _economy;
        private readonly EventLog _eventLog;
        private long _nextCatchRevision;
        private FishingResult _lastCatch;

        public D9FishingSystem(D3TradeState trade, D4EcologySystem ecology,
            TradeEconomySystem economy, EventLog eventLog = null)
        {
            _trade = trade;
            _ecology = ecology;
            _economy = economy ?? new TradeEconomySystem(eventLog);
            _eventLog = eventLog ?? _economy.EventLog ?? new EventLog();
            RegisterMarketDefinitions();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public FishingResult LastCatch { get { return _lastCatch == null ? null : _lastCatch.Clone(); } }
        public FishSchoolState FishSchool
        {
            get { return _ecology == null ? null : _ecology.FindFishSchoolSnapshot(D9FishingIds.FishSchoolId); }
        }

        public FishSchoolState PrepareNearshoreSchool(int day, int seed)
        {
            if (_ecology == null) return null;
            return _ecology.SeedFishSchool(D9FishingIds.FishSchoolId,
                D9FishingIds.FishingSpotId, day, seed);
        }

        public bool MoveNearshoreSchool(int day, int seed)
        {
            return _ecology != null && _ecology.MoveFishSchool(D9FishingIds.FishSchoolId, day, seed);
        }

        public FishingResult Fish(FishingRequest request)
        {
            if (request == null) return Reject("invalid_request", "捕捞请求不存在。", 0);
            var spot = D9FishingCatalog.FindSpot(request.FishingSpotId);
            var gear = D9FishingCatalog.FindGear(request.GearId);
            if (_trade == null || _ecology == null || spot == null || gear == null)
                return Reject("invalid_reference", "捕捞点、渔具或运行状态不存在。", request.SaveRevision);

            long revision = request.SaveRevision > 0 ? request.SaveRevision : _nextCatchRevision + 1;
            var duplicate = _eventLog.Find(CatchEventId, revision);
            if (duplicate != null)
                return Duplicate(duplicate, revision);

            _ecology.SeedFishSchool(spot.FishSchoolId, spot.Id, request.Day, request.Seed);
            var preview = _ecology.PreviewFishCatch(spot.FishSchoolId, spot.Id,
                D9FishingIds.FreshFishItemId, gear.CatchCapacity, request.Day, request.Seed);
            if (!preview.Accepted)
                return Reject(preview.Code, preview.Message, revision);

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

            _nextCatchRevision = Math.Max(_nextCatchRevision, revision);
            var result = new FishingResult
            {
                Succeeded = true,
                Code = "ok",
                Message = "在圣玛丽近海捕获 " + preview.Quantity + " 条鲜鲭鱼。",
                FishingSpotId = spot.Id,
                FishSchoolId = preview.FishSchoolId,
                GearId = gear.Id,
                ItemId = preview.FishItemId,
                Quantity = preview.Quantity,
                SaveRevision = revision,
                EventId = CatchEventId,
            };
            var append = _eventLog.Append(BuildCatchEvent(result, request.Day));
            result.Fact = append.Fact;
            result.Duplicate = append.IsDuplicate;
            _lastCatch = result.Clone();
            return result;
        }

        public FishProcessingResult ProcessFreshFish(int quantity)
        {
            if (_trade == null || quantity <= 0)
                return ProcessFail("invalid_quantity", "处理数量必须大于零。");
            if (D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.FreshFishItemId) < quantity)
                return ProcessFail("not_enough_fresh_fish", "货舱中没有足够的鲜鱼。");
            if (!D3InventorySystem.TryRemoveCargo(_trade.Inventory, D9FishingIds.FreshFishItemId, quantity,
                out string removeError))
                return ProcessFail(removeError, "鲜鱼无法从货舱取出。");
            if (!D3InventorySystem.TryAddCargo(_trade.Inventory, D9FishingIds.SaltedFishItemId, quantity,
                out string addError))
            {
                D3InventorySystem.TryAddCargo(_trade.Inventory, D9FishingIds.FreshFishItemId, quantity, out _);
                return ProcessFail(addError, "处理后的鱼无法装回货舱。");
            }
            return new FishProcessingResult
            {
                Succeeded = true,
                Code = "ok",
                Message = "鲜鲭鱼已处理为盐渍鲭鱼。",
                InputItemId = D9FishingIds.FreshFishItemId,
                OutputItemId = D9FishingIds.SaltedFishItemId,
                Quantity = quantity,
                CargoUsed = D3InventorySystem.UsedCargo(_trade.Inventory),
            };
        }

        public FishCookingResult CookFreshFish(string portId, int quantity)
        {
            var port = PortCatalog.Find(portId);
            if (_trade == null || port == null ||
                (port.Type != D3LocationType.Town && port.Type != D3LocationType.Village))
                return CookFail("not_at_port", "只有港口厨房可以烹饪鲜鱼。");
            if (quantity <= 0) return CookFail("invalid_quantity", "烹饪数量必须大于零。");
            if (!D3InventorySystem.TryRemoveCargo(_trade.Inventory, D9FishingIds.FreshFishItemId, quantity,
                out string removeError))
                return CookFail(removeError, "货舱中没有足够的鲜鱼。");
            if (!D3InventorySystem.TryAddPersonal(_trade.Inventory, D9FishingIds.CookedFishItemId, quantity,
                out string addError))
            {
                D3InventorySystem.TryAddCargo(_trade.Inventory, D9FishingIds.FreshFishItemId, quantity, out _);
                return CookFail(addError, "烹饪结果无法放入随身物品。");
            }
            return new FishCookingResult
            {
                Succeeded = true,
                Code = "ok",
                Message = "港口厨房已将鲜鲭鱼烹饪完成。",
                InputItemId = D9FishingIds.FreshFishItemId,
                OutputItemId = D9FishingIds.CookedFishItemId,
                Quantity = quantity,
                PersonalCount = D3InventorySystem.CountPersonal(_trade.Inventory, D9FishingIds.CookedFishItemId),
            };
        }

        public TradeResult SellFish(string portId, string itemId, int quantity, long saveRevision = 0)
        {
            var port = PortCatalog.Find(portId);
            if (port == null || (port.Type != D3LocationType.Town && port.Type != D3LocationType.Village))
                return TradeResult.Fail("not_at_port", "只有港口市场可以出售鱼获。", _trade == null ? 0 : _trade.Money);
            if (itemId != D9FishingIds.FreshFishItemId && itemId != D9FishingIds.SaltedFishItemId)
                return TradeResult.Fail("invalid_fish_item", "只有鲜鲭鱼或盐渍鲭鱼可以通过本入口出售。", _trade.Money);
            return _economy.Sell(_trade, portId, itemId, quantity,
                new TradePriceContext { Weather = _trade.Market.Weather }, saveRevision);
        }

        public D9FishingRuntimeSnapshot CaptureSnapshot()
        {
            return new D9FishingRuntimeSnapshot
            {
                NextCatchRevision = _nextCatchRevision,
                LastCatch = _lastCatch == null ? null : _lastCatch.Clone(),
                EventLog = _eventLog.CaptureSnapshot(),
            };
        }

        public void Restore(D9FishingRuntimeSnapshot snapshot)
        {
            if (snapshot == null) return;
            _nextCatchRevision = Math.Max(0, snapshot.NextCatchRevision);
            _lastCatch = snapshot.LastCatch == null ? null : snapshot.LastCatch.Clone();
            if (snapshot.EventLog != null) _eventLog.Restore(snapshot.EventLog);
        }

        private void RegisterMarketDefinitions()
        {
            _economy.RegisterMarketItem(D9FishingCatalog.FreshMackerel);
            _economy.RegisterMarketItem(D9FishingCatalog.SaltedMackerel);
            if (_trade != null)
            {
                _economy.EnsureMarketItem(_trade.Market, D9FishingIds.FreshFishItemId);
                _economy.EnsureMarketItem(_trade.Market, D9FishingIds.SaltedFishItemId);
            }
        }

        private static WorldEvent BuildCatchEvent(FishingResult result, int day)
        {
            return new WorldEvent(
                CatchEventId,
                Math.Max(0, day),
                result.FishingSpotId,
                "EcologySystem",
                new EventResult
                {
                    ResultCode = "catch_settled",
                    Summary = result.Message,
                    FactIds = new List<string> { result.FishSchoolId, result.ItemId },
                    Values = new Dictionary<string, string>
                    {
                        ["fish_school_id"] = result.FishSchoolId,
                        ["fish_item_id"] = result.ItemId,
                        ["quantity"] = result.Quantity.ToString(CultureInfo.InvariantCulture),
                        ["gear_id"] = result.GearId,
                    },
                },
                actorIds: new[] { result.GearId },
                inputFacts: new[] { result.FishSchoolId, result.FishingSpotId },
                causeIds: new string[0],
                saveRevision: result.SaveRevision);
        }

        private static FishingResult Duplicate(FactEvent fact, long revision)
        {
            return new FishingResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = "该捕捞事实已结算，未重复增加鱼获。",
                ItemId = fact.Result == null || fact.Result.Values == null ? string.Empty :
                    (fact.Result.Values.ContainsKey("fish_item_id") ? fact.Result.Values["fish_item_id"] : string.Empty),
                Quantity = fact.Result == null || fact.Result.Values == null ? 0 : ReadInt(fact.Result.Values, "quantity"),
                SaveRevision = revision,
                EventId = CatchEventId,
                Fact = fact.CloneFact(),
            };
        }

        private static FishingResult Reject(string code, string message, long revision)
        {
            return new FishingResult { Succeeded = false, Code = code, Message = message, SaveRevision = revision };
        }

        private static FishProcessingResult ProcessFail(string code, string message)
        {
            return new FishProcessingResult { Succeeded = false, Code = code, Message = message };
        }

        private static FishCookingResult CookFail(string code, string message)
        {
            return new FishCookingResult { Succeeded = false, Code = code, Message = message };
        }

        private static int ReadInt(Dictionary<string, string> values, string key)
        {
            return values != null && values.TryGetValue(key, out string text) &&
                int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
                ? result : 0;
        }
    }
}
