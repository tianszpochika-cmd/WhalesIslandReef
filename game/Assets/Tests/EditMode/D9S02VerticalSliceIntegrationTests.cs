using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    /// <summary>
    /// D9-S02 的跨系统同步验收：所有领域写入都经过对应 owner，
    /// 这里只串联意图入口并验证共享事实日志、状态一致性和幂等。
    /// </summary>
    public class D9S02VerticalSliceIntegrationTests
    {
        private const long DepartureRevision = 10;
        private const long CatchRevision = 11;
        private const long StormRevision = 12;
        private const long CrewRevision = 13;
        private const long TradeRevision = 30;
        private const long WageRevision = 31;
        private const long RepairRevision = 32;
        private const long SaveRevision = 33;

        [Test]
        public void FullVerticalSlice_UsesSingleEventLog_AndDoesNotRepeatSettlement()
        {
            var eventLog = new EventLog();
            var economy = new TradeEconomySystem(eventLog);
            var trade = new D3TradeState(2000, 40);
            CommodityCatalog.SeedMarket(trade.Market, 50);

            var ship = ShipState.CreateNew("ship_d9_s02_test", ShipTypeSeed.Find("small_fishing"), "player:captain");
            var runtime = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                CurrentDay = 0,
                Money = trade.Money,
                Hull = ship.Hull,
                HullMax = ship.HullMax,
            };
            var crew = new List<CrewAgent>();
            var isaac = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var preparation = new D9PortShipPreparationSystem(economy, eventLog);

            Assert.IsTrue(preparation.EnterPort(runtime, ship, PortCatalog.StartingPortId).Accepted);
            Assert.IsTrue(preparation.Buy(trade, ship, PortCatalog.StartingPortId, "salt", 1).Accepted);
            Assert.IsTrue(preparation.Buy(trade, ship, PortCatalog.StartingPortId, "timber", 2).Accepted);
            Assert.IsTrue(preparation.BuySupply(trade, ship, PortCatalog.StartingPortId, "water", 10).Accepted);
            Assert.IsTrue(preparation.BuySupply(trade, ship, PortCatalog.StartingPortId, "food", 10).Accepted);
            Assert.IsTrue(preparation.Recruit(trade, ship, PortCatalog.StartingPortId, crew, isaac.Agent).Accepted);

            var departure = preparation.Depart(runtime, ship, PortCatalog.GreenCayId, trade.Money, 0, DepartureRevision);
            Assert.IsTrue(departure.Accepted);
            Assert.AreEqual(D3VoyageStatus.Sailing, runtime.VoyageStatus);

            var ecology = new D4EcologySystem();
            var fishing = new D9FishingSystem(trade, ecology, economy, eventLog);
            var school = fishing.PrepareNearshoreSchool(0, 17);
            Assert.IsNotNull(school);
            Assert.AreEqual(D9FishingIds.FishSchoolId, school.FishSchoolId);
            Assert.AreEqual(D9FishingIds.FishingSpotId, school.FishingSpotId);

            var catchRequest = new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 0,
                Seed = 17,
                SaveRevision = CatchRevision,
            };
            var caught = fishing.Fish(catchRequest);
            Assert.IsTrue(caught.Succeeded, caught.Message);
            Assert.Greater(caught.Quantity, 0);
            int cargoAfterCatch = D3InventorySystem.CountCargo(trade.Inventory, caught.ItemId);
            var duplicateCatch = fishing.Fish(catchRequest.Clone());
            Assert.IsTrue(duplicateCatch.Succeeded);
            Assert.IsTrue(duplicateCatch.Duplicate);
            Assert.AreEqual(cargoAfterCatch, D3InventorySystem.CountCargo(trade.Inventory, caught.ItemId));

            var environment = new EnvironmentSystem();
            var storm = environment.ApplyStormEvent(1, D9FishingIds.FishingSpotId, StormRevision, eventLog);
            Assert.IsTrue(storm.Succeeded);
            environment.PublishTo(runtime);
            Assert.AreEqual(D9WeatherState.Storm, runtime.WeatherSea.Weather);

            var crewEvents = new D9CrewEventSystem(eventLog);
            var discussion = crewEvents.Discuss(isaac, "风暴后讨论航线和岗位", 1, CrewRevision);
            Assert.IsTrue(discussion.Succeeded, discussion.Message);
            Assert.AreEqual("CrewSystem", discussion.Fact.OwnerSystem);
            Assert.IsNotNull(discussion.Proposals);

            var combat = new D4CombatSystem(null, eventLog);
            combat.BeginEncounter("ph_spanish_patrol", ship.Hull, ship.HullMax, 2, crew.Count,
                60, 60, 4, 8, 34f, 1f);
            var retreat = combat.Escape();
            Assert.IsTrue(retreat.Accepted);
            Assert.AreEqual(D4CombatStatus.Escaped, combat.Snapshot.Status);

            var quest = new D9TreasureQuestSystem(eventLog, D9TreasureQuestCatalog.CreateFirstChart());
            Assert.IsTrue(quest.HearRumor("player:captain", 1, 14).Accepted);
            Assert.IsTrue(quest.AcceptRumor().Accepted);
            Assert.IsTrue(D3InventorySystem.TryAddPersonal(trade.Inventory,
                D9TreasureQuestCatalog.RequiredEquipmentId, 1, out string equipmentError), equipmentError);
            var treasure = quest.TryLand(new D9TreasureLandingRequest
            {
                ActorRef = "player:captain",
                LocationRef = D9TreasureQuestCatalog.SiteRef,
                RouteChoice = D9TreasureRouteChoice.WaitForTide,
                TideWindowOpen = true,
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            }, 1, 15);
            Assert.IsTrue(treasure.Accepted, treasure.Message);
            Assert.AreEqual(D9TreasureQuestStatus.Discovered, quest.Snapshot.Status);
            Assert.AreEqual(D9TreasureQuestCatalog.RewardRequestId, treasure.RewardRequest.RewardId);

            var voyage = new VoyageSettlementSystem(null, eventLog);
            Assert.IsTrue(voyage.TryBegin(PortCatalog.StartingPortId, PortCatalog.GreenCayId, 4f, 91,
                ship.Supplies["water"], ship.Supplies["food"], ship.Hull, ship.HullMax,
                out VoyageState voyageState, out string voyageError), voyageError);
            for (int i = 0; i < 12 && voyageState.Status == D3VoyageStatus.Sailing; i++)
                Assert.IsTrue(voyage.AdvanceDay(voyageState).Succeeded);
            Assert.AreEqual(D3VoyageStatus.Arrived, voyageState.Status);
            var returned = voyage.Settle(voyageState);
            Assert.IsTrue(returned.Succeeded, returned.Message);
            runtime.CurrentLocationId = PortCatalog.GreenCayId;
            runtime.VoyageStatus = D3VoyageStatus.InPort;
            Assert.IsTrue(preparation.EnterPort(runtime, ship, PortCatalog.GreenCayId).Accepted);

            var processed = fishing.ProcessFreshFish(caught.Quantity);
            Assert.IsTrue(processed.Succeeded, processed.Message);
            int moneyBeforeTrade = trade.Money;
            var sale = fishing.SellFish(PortCatalog.GreenCayId, D9FishingIds.SaltedFishItemId,
                caught.Quantity, TradeRevision);
            Assert.IsTrue(sale.Succeeded, sale.Message);
            Assert.Greater(trade.Money, moneyBeforeTrade);
            int moneyAfterTrade = trade.Money;
            var duplicateSale = fishing.SellFish(PortCatalog.GreenCayId, D9FishingIds.SaltedFishItemId,
                caught.Quantity, TradeRevision);
            Assert.IsTrue(duplicateSale.Succeeded);
            Assert.IsTrue(duplicateSale.DuplicateSettlement);
            Assert.AreEqual(moneyAfterTrade, trade.Money);

            int moneyBeforeWages = trade.Money;
            var wages = preparation.SettleWages(trade, ship, PortCatalog.GreenCayId, crew, 1, 2, WageRevision);
            Assert.IsTrue(wages.Accepted, wages.Message);
            int moneyAfterWages = trade.Money;
            var duplicateWages = preparation.SettleWages(trade, ship, PortCatalog.GreenCayId, crew, 1, 2, WageRevision);
            Assert.IsTrue(duplicateWages.Accepted);
            Assert.IsTrue(duplicateWages.Duplicate);
            Assert.AreEqual(moneyAfterWages, trade.Money);
            Assert.Less(moneyAfterWages, moneyBeforeWages);

            ship.Hull = ship.HullMax - 10;
            int hullBeforeRepair = ship.Hull;
            var repair = preparation.Repair(trade, ship, PortCatalog.GreenCayId, 10, 2, RepairRevision);
            Assert.IsTrue(repair.Accepted, repair.Message);
            Assert.AreEqual(ship.HullMax, ship.Hull);
            int moneyAfterRepair = trade.Money;
            var duplicateRepair = preparation.Repair(trade, ship, PortCatalog.GreenCayId, 10, 2, RepairRevision);
            Assert.IsTrue(duplicateRepair.Accepted);
            Assert.IsTrue(duplicateRepair.Duplicate);
            Assert.AreEqual(moneyAfterRepair, trade.Money);
            Assert.AreEqual(ship.HullMax, ship.Hull);
            Assert.Less(hullBeforeRepair, ship.HullMax);

            var checkpoint = SaveSystem.AppendD9SaveCheckpoint(eventLog, 2, PortCatalog.GreenCayId, SaveRevision);
            Assert.IsNotNull(checkpoint);
            var restoredLog = EventLog.FromSnapshot(eventLog.CaptureSnapshot());
            Assert.AreEqual(eventLog.Count, restoredLog.Count);
            AssertAllFrozenEventsPresent(restoredLog);

            var restoredQuest = D9TreasureQuestSystem.RestoreFromSave(quest.CaptureSaveEnvelope());
            int restoredCount = restoredQuest.EventLog.Count;
            var duplicateTreasure = restoredQuest.TryLand(new D9TreasureLandingRequest
            {
                ActorRef = "player:captain",
                LocationRef = D9TreasureQuestCatalog.SiteRef,
                RouteChoice = D9TreasureRouteChoice.WaitForTide,
                TideWindowOpen = true,
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            }, 3, 15);
            Assert.IsFalse(duplicateTreasure.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.AlreadyDiscovered, duplicateTreasure.Code);
            Assert.AreEqual(restoredCount, restoredQuest.EventLog.Count);
        }

        [Test]
        public void D9RuntimeSaveLoad_PreservesQuestSnapshotAndSharedEventLog()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), "wb_d9_s02_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D9-S02 测试船长" });
                var log = new EventLog();
                var quest = new D9TreasureQuestSystem(log);
                Assert.IsTrue(quest.HearRumor("player:captain", 1, 1).Accepted);
                SaveSystem.SaveD9Runtime(new D9RuntimeSnapshot { TreasureQuest = quest.Snapshot });
                SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
                {
                    CurrentLocationId = PortCatalog.StartingPortId,
                    VoyageStatus = D3VoyageStatus.InPort,
                    EventLog = log.CaptureSnapshot(),
                });

                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loadedD9 = SaveSystem.LoadD9Runtime();
                var loadedD3 = SaveSystem.LoadD3Runtime();
                Assert.IsNotNull(loadedD9);
                Assert.AreEqual(D9TreasureQuestStatus.RumorHeard, loadedD9.TreasureQuest.Status);
                Assert.IsNotNull(loadedD3);
                Assert.IsNotNull(loadedD3.EventLog);
                Assert.AreEqual(1, loadedD3.EventLog.Facts.Count);
                Assert.AreEqual("event_d9_treasure_clue", loadedD3.EventLog.Facts[0].EventId);
            }
            finally
            {
                SaveSystem.DbPath = null;
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    string path = dbPath + suffix;
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
        }

        private static void AssertAllFrozenEventsPresent(EventLog log)
        {
            var expected = new[]
            {
                "event_d9_departure_prepared", "event_d9_departure", "event_d9_fishing_catch",
                "event_d9_weather_storm", "event_d9_crew_report", "event_d9_nearshore_patrol",
                "event_d9_nearshore_retreat", "event_d9_treasure_clue", "event_d9_treasure_resolved",
                "event_d9_returned_to_harbor", "event_d9_trade_settled", "event_d9_wage_settled",
                "event_d9_ship_repaired", "event_d9_save_checkpoint",
            };
            var actual = new HashSet<string>();
            var dedupeKeys = new HashSet<string>();
            foreach (var fact in log.Facts)
            {
                actual.Add(fact.EventId);
                Assert.IsTrue(dedupeKeys.Add(fact.DedupeKey), "重复事实键: " + fact.DedupeKey);
            }
            foreach (var eventId in expected)
                Assert.IsTrue(actual.Contains(eventId), "缺少事件: " + eventId);
        }
    }
}
