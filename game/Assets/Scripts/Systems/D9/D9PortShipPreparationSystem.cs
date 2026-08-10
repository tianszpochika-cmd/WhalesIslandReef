// ============================================================
// D9PortShipPreparationSystem —— 港口与船舶整备的强逻辑编排
//
// 本系统只接受表现层意图，最终写入委托给既有唯一 owner：
//   - 市场/货物/金币：TradeEconomySystem + D3InventorySystem
//   - 改装：D4ShipyardSystem
//   - 船体、补给、锚泊：本 D9 船舶整备 owner
//   - 船员工资后的士气/忠诚：本 D9 工资结算 owner
// UI 不直接修改这些状态；本文件无 Unity、网络或 LLM 依赖。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Game.Config;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D9
{
    public sealed class D9PortShipPreparationSystem
    {
        public const string DeparturePreparedEventId = "event_d9_departure_prepared";
        public const string DepartureEventId = "event_d9_departure";
        public const string WageSettledEventId = "event_d9_wage_settled";
        public const string ShipRepairedEventId = "event_d9_ship_repaired";
        public const string SantaMargaritaBerthId = "building_saint_marys_dock";
        public const string GreenCayBerthId = "berth_green_cay_01";
        public const string RepairMaterialId = "timber";
        public const int RepairMoneyPerPoint = 8;
        public const int WaterUnitPrice = 2;
        public const int FoodUnitPrice = 3;
        public const int RecruitmentPrice = 60;
        public const int DailyWage = 25;
        public const int TavernPricePerCrew = 4;

        private readonly TradeEconomySystem _economy;
        private readonly EventLog _eventLog;
        private long _nextEventRevision;

        public D9PortShipPreparationSystem(TradeEconomySystem economy = null, EventLog eventLog = null)
        {
            _economy = economy ?? new TradeEconomySystem();
            _eventLog = eventLog ?? _economy.EventLog ?? new EventLog();
            _nextEventRevision = _eventLog.NextSequence;
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D9PreparationResult EnterPort(D3RuntimeSnapshot runtime, ShipState ship, string portId)
        {
            var port = PortCatalog.Find(portId);
            if (runtime == null || ship == null)
                return D9PreparationResult.Fail("berth", "invalid_state", "港口入口缺少航行或船舶状态。", 0);
            if (port == null || port.Type == D3LocationType.DesertedIsland)
                return D9PreparationResult.Fail("berth", "invalid_port", "该地点没有港口整备入口。", 0);
            if (runtime.VoyageStatus == D3VoyageStatus.Sailing)
                return D9PreparationResult.Fail("berth", "still_sailing", "航行中不能进入港口整备。", 0);
            if (runtime.VoyageStatus == D3VoyageStatus.Failed)
                return D9PreparationResult.Fail("berth", "voyage_failed", "失败航程必须先由航行系统恢复。", 0);

            runtime.CurrentLocationId = port.Id;
            runtime.DestinationId = null;
            runtime.VoyageStatus = D3VoyageStatus.InPort;
            runtime.CurrentDay = 0;
            runtime.TotalDays = 0;
            SetBerth(ship, port.Id);
            return D9PreparationResult.Success("berth", "已进入" + port.DisplayName + "并完成靠泊。", 0, 1, 0);
        }

        public D9PreparationResult AssignBerth(ShipState ship, string portId, string berthId, int money = 0)
        {
            if (ship == null) return D9PreparationResult.Fail("berth", "invalid_state", "船舶状态不存在。", money);
            if (!IsKnownBerth(portId, berthId))
                return D9PreparationResult.Fail("berth", "unknown_berth", "该泊位不属于当前垂直切片港口。", money);
            SetBerth(ship, portId, berthId);
            return D9PreparationResult.Success("berth", "已分配泊位。", 0, 1, money);
        }

        public D9PreparationResult Buy(D3TradeState trade, ShipState ship, string portId, string commodityId, int quantity)
        {
            var gate = CheckService(trade, ship, portId, "market");
            if (gate != null) return gate;
            var result = _economy.Buy(trade, portId, commodityId, quantity);
            return FromTradeResult("market", result);
        }

        public D9PreparationResult BuySupply(D3TradeState trade, ShipState ship, string portId, string supplyId, int quantity)
        {
            var gate = CheckService(trade, ship, portId, "supplies");
            if (gate != null) return gate;
            if (supplyId != "water" && supplyId != "food")
                return D9PreparationResult.Fail("supplies", "unknown_supply", "只允许补充水和食物。", trade.Money);
            if (quantity <= 0 || quantity > 1000)
                return D9PreparationResult.Fail("supplies", "invalid_quantity", "补给数量超出整备范围。", trade.Money);

            int unitPrice = supplyId == "water" ? WaterUnitPrice : FoodUnitPrice;
            long totalLong = (long)unitPrice * quantity;
            if (totalLong > int.MaxValue)
                return D9PreparationResult.Fail("supplies", "invalid_quantity", "补给费用超出安全范围。", trade.Money);
            int total = (int)totalLong;
            if (!_economy.TrySpend(trade, total, out string error))
                return D9PreparationResult.Fail("supplies", error, "金币不足，无法购买补给。", trade.Money);

            int current = ship.Supplies.TryGetValue(supplyId, out int amount) ? Math.Max(0, amount) : 0;
            ship.Supplies[supplyId] = current + quantity;
            return D9PreparationResult.Success("supplies", "补给已装船。", total, quantity, trade.Money);
        }

        public D9PreparationResult Repair(D3TradeState trade, ShipState ship, string portId, int repairPoints,
            int worldDay = 0, long saveRevision = 0)
        {
            var gate = CheckService(trade, ship, portId, "repair");
            if (gate != null) return gate;
            if (repairPoints <= 0)
                return D9PreparationResult.Fail("repair", "invalid_quantity", "修船点数必须大于零。", trade.Money);
            long revision = ResolveRevision(saveRevision);
            var existing = _eventLog.Find(ShipRepairedEventId, revision);
            if (existing != null)
                return Duplicate("repair", "修船事实已经结算，未重复消耗材料或金币。", trade.Money, existing);

            int missing = Math.Max(0, ship.HullMax - ship.Hull);
            if (missing == 0)
                return D9PreparationResult.Fail("repair", "already_repaired", "船体已经完整。", trade.Money);
            if (repairPoints > missing)
                return D9PreparationResult.Fail("repair", "quantity_exceeds_damage", "修复点数不能超过当前损伤。", trade.Money);

            int timber = (repairPoints + 9) / 10;
            if (D3InventorySystem.CountCargo(trade.Inventory, RepairMaterialId) < timber)
                return D9PreparationResult.Fail("repair", "missing_material", "修船需要木材。", trade.Money);
            long costLong = (long)repairPoints * RepairMoneyPerPoint;
            if (costLong > int.MaxValue)
                return D9PreparationResult.Fail("repair", "invalid_quantity", "修船费用超出安全范围。", trade.Money);
            int cost = (int)costLong;
            if (!_economy.CanSpend(trade, cost))
                return D9PreparationResult.Fail("repair", "not_enough_money", "金币不足，无法修船。", trade.Money);

            _economy.TrySpend(trade, cost, out _);
            D3InventorySystem.TryRemoveCargo(trade.Inventory, RepairMaterialId, timber, out _);
            ship.Hull = Math.Min(ship.HullMax, ship.Hull + repairPoints);
            var result = D9PreparationResult.Success("repair", "修船完成，木材和金币已结算。", cost, repairPoints, trade.Money);
            result.Fact = AppendFact(ShipRepairedEventId, worldDay, revision, portId, "ShipyardSystem", "ship_repaired",
                result.Message, new Dictionary<string, string>
                {
                    ["repair_points"] = repairPoints.ToString(),
                    ["material_id"] = RepairMaterialId,
                    ["material_quantity"] = timber.ToString(),
                    ["cost"] = cost.ToString(),
                }, new[] { "ship:" + (ship.Id ?? string.Empty), "material:" + RepairMaterialId });
            return result;
        }

        public D9PreparationResult Recruit(D3TradeState trade, ShipState ship, string portId,
            IList<CrewAgent> crew, CrewAgent candidate)
        {
            var gate = CheckService(trade, ship, portId, "recruitment");
            if (gate != null) return gate;
            if (crew == null || candidate == null || string.IsNullOrEmpty(candidate.Id))
                return D9PreparationResult.Fail("recruitment", "invalid_candidate", "招募对象无效。", trade.Money);
            if (ship.CrewIds.Contains(candidate.Id) || ContainsCrew(crew, candidate.Id))
                return D9PreparationResult.Fail("recruitment", "already_recruited", "该船员已经在船上。", trade.Money);
            if (ship.CrewIds.Count >= ship.CrewCapacity)
                return D9PreparationResult.Fail("recruitment", "crew_full", "船员编制已满。", trade.Money);
            if (!_economy.TrySpend(trade, RecruitmentPrice, out string error))
                return D9PreparationResult.Fail("recruitment", error, "金币不足，无法招募。", trade.Money);

            crew.Add(candidate);
            ship.CrewIds.Add(candidate.Id);
            return D9PreparationResult.Success("recruitment", "已招募" + candidate.DisplayName + "。", RecruitmentPrice, 1, trade.Money);
        }

        public D9PreparationResult VisitTavern(D3TradeState trade, ShipState ship, string portId, IList<CrewAgent> crew)
        {
            var gate = CheckService(trade, ship, portId, "tavern");
            if (gate != null) return gate;
            int crewCount = CountCrew(crew);
            int cost = Math.Max(1, crewCount) * TavernPricePerCrew;
            if (!_economy.TrySpend(trade, cost, out string error))
                return D9PreparationResult.Fail("tavern", error, "金币不足，酒馆不赊账。", trade.Money);
            if (crew != null)
            {
                foreach (var member in crew)
                {
                    if (member == null || member.Stats == null) continue;
                    member.Stats.Morale = NumericConfiguration.ClampMorale(member.Stats.Morale + 4f);
                    member.Stats.Fatigue = NumericConfiguration.ClampFatigue(member.Stats.Fatigue - 3f);
                }
            }
            return D9PreparationResult.Success("tavern", "船员在酒馆短暂休整。", cost, crewCount, trade.Money);
        }

        public D9PreparationResult SettleWages(D3TradeState trade, ShipState ship, string portId,
            IList<CrewAgent> crew, int days = 1, int worldDay = 0, long saveRevision = 0)
        {
            var gate = CheckService(trade, ship, portId, "wages");
            if (gate != null) return gate;
            if (days <= 0 || days > 30)
                return D9PreparationResult.Fail("wages", "invalid_days", "工资结算天数超出整备范围。", trade.Money);
            int crewCount = CountCrew(crew);
            long costLong = (long)crewCount * DailyWage * days;
            if (costLong > int.MaxValue)
                return D9PreparationResult.Fail("wages", "invalid_days", "工资费用超出安全范围。", trade.Money);
            int cost = (int)costLong;
            long revision = ResolveRevision(saveRevision);
            var existing = _eventLog.Find(WageSettledEventId, revision);
            if (existing != null)
                return Duplicate("wages", "工资事实已经结算，未重复扣款。", trade.Money, existing);
            if (!_economy.TrySpend(trade, cost, out string error))
            {
                ApplyWageOutcome(crew, false);
                return D9PreparationResult.Fail("wages", error, "工资未能结清，船员士气下降。", trade.Money);
            }
            ApplyWageOutcome(crew, true);
            var result = D9PreparationResult.Success("wages", "工资已结算，船员状态得到确认。", cost, crewCount, trade.Money);
            result.Fact = AppendFact(WageSettledEventId, worldDay, revision, portId, "EconomySystem", "wages_paid",
                result.Message, new Dictionary<string, string>
                {
                    ["crew_count"] = crewCount.ToString(),
                    ["days"] = days.ToString(),
                    ["amount"] = cost.ToString(),
                }, CrewIds(crew));
            return result;
        }

        public D9PreparationResult Upgrade(D3TradeState trade, ShipState ship, D4ShipyardSystem shipyard,
            int reputation, D4UpgradeSlot slot)
        {
            if (trade == null || ship == null || shipyard == null)
                return D9PreparationResult.Fail("shipyard", "invalid_state", "船坞整备状态不完整。", trade == null ? 0 : trade.Money);
            if (!IsMoored(ship))
                return D9PreparationResult.Fail("shipyard", "not_moored", "必须先靠泊才能进入船坞。", trade.Money);
            int before = trade.Money;
            int money = before;
            var result = shipyard.TryUpgrade(slot, ref money, reputation);
            if (!result.Accepted)
                return D9PreparationResult.Fail("shipyard", "shipyard_rejected", result.Message, before);
            trade.Money = money;
            return D9PreparationResult.Success("shipyard", result.Message, result.Cost, result.NewLevel, money);
        }

        /// <summary>整备页离港前的最后一道检查；实际航线由 D3 Voyage owner 开始。</summary>
        public D9PreparationResult PrepareDeparture(ShipState ship, string portId, int money = 0)
        {
            if (ship == null) return D9PreparationResult.Fail("berth", "invalid_state", "船舶状态不存在。", money);
            if (!IsMoored(ship)) return D9PreparationResult.Fail("berth", "not_moored", "离港前必须先完成泊位绑定。", money);
            if (CountCrewIds(ship) == 0) return D9PreparationResult.Fail("berth", "no_crew", "没有船员不能离港。", money);
            if (ReadSupply(ship, "water") <= 0 || ReadSupply(ship, "food") <= 0)
                return D9PreparationResult.Fail("berth", "insufficient_supplies", "水和食物都必须至少有一份。", money);
            if (PortCatalog.Find(portId) == null)
                return D9PreparationResult.Fail("berth", "invalid_port", "离港港口不存在。", money);
            SetMooring(ship, false);
            return D9PreparationResult.Success("berth", "整备完成，可以离港。", 0, 1, money);
        }

        /// <summary>纯系统测试和无 D3 UI 的集成入口；带目的地才允许进入 Sailing。</summary>
        public D9PreparationResult Depart(D3RuntimeSnapshot runtime, ShipState ship, string destinationId, int money = 0,
            int worldDay = 0, long saveRevision = 0)
        {
            if (runtime == null || ship == null)
                return D9PreparationResult.Fail("berth", "invalid_state", "离港状态不完整。", money);
            var destination = PortCatalog.Find(destinationId);
            if (runtime.VoyageStatus != D3VoyageStatus.InPort)
                return D9PreparationResult.Fail("berth", "not_in_port", "只有港内状态可以离港。", money);
            if (destination == null || destination.Id == runtime.CurrentLocationId)
                return D9PreparationResult.Fail("berth", "invalid_destination", "目的地必须是另一个已注册港口。", money);
            var ready = PrepareDeparture(ship, runtime.CurrentLocationId, money);
            if (!ready.Accepted) return ready;
            runtime.DestinationId = destination.Id;
            runtime.VoyageStatus = D3VoyageStatus.Sailing;
            var result = D9PreparationResult.Success("berth", "已离港并进入航行状态。", 0, 1, money);
            AppendDepartureEvents(runtime, ship, worldDay, saveRevision);
            return result;
        }

        /// <summary>
        /// D3 已成功进入 Sailing 后由整备入口调用；这里仅记录 ShipSystem 事实，
        /// 不重复执行航线公式或改变 D3 状态。
        /// </summary>
        public bool RecordDeparture(D3RuntimeSnapshot runtime, ShipState ship, int worldDay = 0,
            long saveRevision = 0)
        {
            if (runtime == null || ship == null || runtime.VoyageStatus != D3VoyageStatus.Sailing)
                return false;
            AppendDepartureEvents(runtime, ship, worldDay, saveRevision);
            return true;
        }

        public D9PortShipPreparationSnapshot BuildSnapshot(D3RuntimeSnapshot runtime, D3TradeState trade,
            ShipState ship, D4ShipyardSystem shipyard, IList<CrewAgent> crew)
        {
            var port = runtime == null ? null : PortCatalog.Find(runtime.CurrentLocationId);
            var result = new D9PortShipPreparationSnapshot
            {
                PortId = port == null ? string.Empty : port.Id,
                PortDisplayName = port == null ? "未知地点" : port.DisplayName,
                InPort = runtime != null && runtime.VoyageStatus == D3VoyageStatus.InPort,
                Money = trade == null ? 0 : trade.Money,
            };
            result.Ship = BuildShipSnapshot(trade, ship, shipyard, crew);
            result.Moored = result.Ship.Moored;
            result.BerthId = result.Ship.BerthId;
            foreach (D9PortService service in Enum.GetValues(typeof(D9PortService)))
                result.Services[service.ToString()] = result.InPort && (service == D9PortService.Berth || result.Moored);
            return result;
        }

        private static D9ShipPreparationSnapshot BuildShipSnapshot(D3TradeState trade, ShipState ship,
            D4ShipyardSystem shipyard, IList<CrewAgent> crew)
        {
            var type = ship == null ? null : ShipTypeSeed.Find(ship.ShipTypeId);
            var result = new D9ShipPreparationSnapshot
            {
                ShipId = ship == null ? string.Empty : ship.Id,
                ShipTypeId = ship == null ? string.Empty : ship.ShipTypeId,
                Weight = trade == null ? 0 : D3InventorySystem.UsedCargo(trade.Inventory),
                CargoCapacity = trade == null ? 0 : trade.Inventory.CargoCapacity,
                Draft = type == null ? 0f : type.Draft,
                CrewCount = crew == null ? (ship == null ? 0 : ship.CrewIds.Count) : CountCrew(crew),
                CrewCapacity = ship == null ? 0 : ship.CrewCapacity,
                Hull = ship == null ? 0 : ship.Hull,
                HullMax = ship == null ? 0 : ship.HullMax,
                Damage = ship == null ? 0 : Math.Max(0, ship.HullMax - ship.Hull),
                Water = ship == null ? 0 : ReadSupply(ship, "water"),
                Food = ship == null ? 0 : ReadSupply(ship, "food"),
                Moored = ship != null && IsMoored(ship),
                BerthId = ship == null ? string.Empty : ReadBerth(ship),
            };
            if (trade != null && trade.Inventory != null)
                foreach (var pair in trade.Inventory.Cargo) result.Cargo[pair.Key] = pair.Value;
            if (shipyard != null)
                foreach (var pair in shipyard.Snapshot.Upgrades) result.Upgrades[pair.Key] = pair.Value;
            return result;
        }

        private D9PreparationResult CheckService(D3TradeState trade, ShipState ship, string portId, string service)
        {
            int money = trade == null ? 0 : trade.Money;
            if (trade == null || ship == null) return D9PreparationResult.Fail(service, "invalid_state", "整备状态不存在。", money);
            var port = PortCatalog.Find(portId);
            if (port == null || port.Type == D3LocationType.DesertedIsland)
                return D9PreparationResult.Fail(service, "invalid_port", "该地点没有此项港口服务。", money);
            if (!IsMoored(ship)) return D9PreparationResult.Fail(service, "not_moored", "必须先靠泊才能使用港口服务。", money);
            return null;
        }

        private static D9PreparationResult FromTradeResult(string service, TradeResult result)
        {
            if (result == null) return D9PreparationResult.Fail(service, "trade_failed", "市场未返回结果。", 0);
            if (!result.Succeeded) return D9PreparationResult.Fail(service, result.Code, result.Message, result.MoneyAfter);
            return D9PreparationResult.Success(service, result.Message, result.TotalPrice, result.Quantity, result.MoneyAfter);
        }

        private void AppendDepartureEvents(D3RuntimeSnapshot runtime, ShipState ship, int worldDay,
            long saveRevision)
        {
            long revision = ResolveRevision(saveRevision);
            string location = string.IsNullOrEmpty(runtime.CurrentLocationId)
                ? PortCatalog.StartingPortId : runtime.CurrentLocationId;
            AppendFact(DeparturePreparedEventId, worldDay, revision, location, "ShipSystem", "departure_prepared",
                "船舶、货物、补给和船员岗位已通过离港前检查。",
                new Dictionary<string, string>
                {
                    ["ship_id"] = ship.Id ?? string.Empty,
                    ["destination_id"] = runtime.DestinationId ?? string.Empty,
                    ["crew_count"] = CountCrewIds(ship).ToString(),
                }, new[] { "ship:" + (ship.Id ?? string.Empty), "port:" + location });
            AppendFact(DepartureEventId, worldDay, revision, location, "ShipSystem", "departed",
                "船舶已离开圣玛格丽特港，航行系统接管航程。",
                new Dictionary<string, string>
                {
                    ["ship_id"] = ship.Id ?? string.Empty,
                    ["origin_id"] = location,
                    ["destination_id"] = runtime.DestinationId ?? string.Empty,
                    ["voyage_status"] = runtime.VoyageStatus.ToString(),
                }, new[] { "ship:" + (ship.Id ?? string.Empty), "voyage:" + location });
        }

        private long ResolveRevision(long requested)
        {
            if (requested > 0)
            {
                _nextEventRevision = Math.Max(_nextEventRevision, requested);
                return requested;
            }
            return ++_nextEventRevision;
        }

        private D9PreparationResult Duplicate(string service, string message, int money, FactEvent fact)
        {
            return new D9PreparationResult
            {
                Accepted = true,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = message,
                Service = service,
                MoneyAfter = Math.Max(0, money),
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        private FactEvent AppendFact(string eventId, int worldDay, long saveRevision, string location,
            string ownerSystem, string resultCode, string summary, Dictionary<string, string> values,
            IEnumerable<string> facts)
        {
            var append = _eventLog.Append(new WorldEvent(
                eventId,
                Math.Max(0, worldDay),
                string.IsNullOrEmpty(location) ? PortCatalog.StartingPortId : location,
                ownerSystem,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string>(facts ?? new string[0]),
                    Values = values ?? new Dictionary<string, string>(),
                },
                new[] { "player" },
                facts,
                new string[0],
                Math.Max(0, saveRevision)));
            return append.Fact;
        }

        private static List<string> CrewIds(IList<CrewAgent> crew)
        {
            var ids = new List<string>();
            if (crew == null) return ids;
            foreach (var member in crew)
                if (member != null && !string.IsNullOrEmpty(member.Id)) ids.Add("crew:" + member.Id);
            return ids;
        }

        private static bool IsKnownBerth(string portId, string berthId)
        {
            return (portId == PortCatalog.StartingPortId && berthId == SantaMargaritaBerthId) ||
                   (portId == PortCatalog.GreenCayId && berthId == GreenCayBerthId);
        }

        private static void SetBerth(ShipState ship, string portId, string berthId = null)
        {
            if (ship.State == null) ship.State = new Dictionary<string, float>();
            if (string.IsNullOrEmpty(berthId))
                berthId = portId == PortCatalog.StartingPortId ? SantaMargaritaBerthId :
                    portId == PortCatalog.GreenCayId ? GreenCayBerthId : string.Empty;
            ship.State["moored"] = 1f;
            ship.State["berth_santa_margarita"] = berthId == SantaMargaritaBerthId ? 1f : 0f;
            ship.State["berth_green_cay"] = berthId == GreenCayBerthId ? 1f : 0f;
        }

        private static void SetMooring(ShipState ship, bool moored)
        {
            if (ship.State == null) ship.State = new Dictionary<string, float>();
            ship.State["moored"] = moored ? 1f : 0f;
            if (!moored)
            {
                ship.State["berth_santa_margarita"] = 0f;
                ship.State["berth_green_cay"] = 0f;
            }
        }

        private static bool IsMoored(ShipState ship)
        {
            return ship != null && ship.State != null && ship.State.TryGetValue("moored", out float value) && value >= 0.5f;
        }

        private static string ReadBerth(ShipState ship)
        {
            if (ship == null || ship.State == null) return string.Empty;
            if (ship.State.TryGetValue("berth_santa_margarita", out float santa) && santa >= 0.5f) return SantaMargaritaBerthId;
            if (ship.State.TryGetValue("berth_green_cay", out float green) && green >= 0.5f) return GreenCayBerthId;
            return string.Empty;
        }

        private static int ReadSupply(ShipState ship, string id)
        {
            return ship == null || ship.Supplies == null || !ship.Supplies.TryGetValue(id, out int value) ? 0 : Math.Max(0, value);
        }

        private static int CountCrew(IList<CrewAgent> crew)
        {
            if (crew == null) return 0;
            int count = 0;
            foreach (var member in crew) if (member != null) count++;
            return count;
        }

        private static int CountCrewIds(ShipState ship)
        {
            if (ship == null || ship.CrewIds == null) return 0;
            int count = 0;
            foreach (var id in ship.CrewIds) if (!string.IsNullOrEmpty(id)) count++;
            return count;
        }

        private static bool ContainsCrew(IList<CrewAgent> crew, string id)
        {
            if (crew == null) return false;
            foreach (var member in crew) if (member != null && member.Id == id) return true;
            return false;
        }

        private static void ApplyWageOutcome(IList<CrewAgent> crew, bool paid)
        {
            if (crew == null) return;
            foreach (var member in crew)
            {
                if (member == null || member.Stats == null) continue;
                member.Stats.Morale = NumericConfiguration.ClampMorale(member.Stats.Morale + (paid ? 3f : -8f));
                member.Stats.Loyalty = NumericConfiguration.ClampLoyalty(member.Stats.Loyalty + (paid ? 2f : -10f));
            }
        }
    }
}
