using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D11.Tests
{
    public sealed class D11AquacultureTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d11_aquaculture_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string file = _dbPath + suffix;
                if (!File.Exists(file)) continue;
                try { File.Delete(file); } catch { }
            }
        }

        [Test]
        public void LiveFishHoldTracksCapacityWeightWaterQualityDeathAndDuplicateAfterLoad()
        {
            var trade = new D3TradeState(500, 40);
            var initial = new D11LiveFishHoldState
            {
                Oxygen = 0f,
                WaterQuality = 0f,
                Maintenance = 0f,
                Fish = new List<D11FishStockState>
                {
                    new D11FishStockState
                    {
                        SpeciesId = D11AquacultureIds.FishSpeciesId,
                        CargoItemId = D11AquacultureIds.FishCargoItemId,
                        Quantity = 12,
                        UnitWeight = 1,
                        UnitVolume = 1,
                    },
                },
            };
            var log = new EventLog();
            var hold = new D11LiveFishHoldSystem(initial, trade, log);
            Assert.AreEqual(12, hold.UsedVolume);
            Assert.AreEqual(12, hold.TotalLiveWeight);

            var maintained = hold.Maintain(new D11LiveFishMaintenanceRequest
            {
                WorldDay = 1,
                WaterExchangeUnits = 0,
                LaborUnits = 0,
                Seed = 17,
                SaveRevision = 1,
                EventId = "event_d11_live_fish_maintenance_test",
            });
            Assert.IsTrue(maintained.Succeeded, maintained.Message);
            Assert.IsTrue(maintained.DeadQuantity > 0, "degraded water/oxygen should produce deterministic mortality");
            Assert.IsNotNull(log.Find("event_d11_live_fish_maintenance_test", 1));
            Assert.IsNotNull(log.Find(maintained.DeathEventId, 1));

            var runtime = hold.CaptureRuntime();
            var restoredLog = EventLog.FromSnapshot(runtime.EventLog);
            var restoredHold = new D11LiveFishHoldSystem(runtime.LiveFishHold, trade, restoredLog);
            int quantityAfterLoad = restoredHold.Snapshot.FishQuantity;
            var duplicate = restoredHold.Maintain(new D11LiveFishMaintenanceRequest
            {
                WorldDay = 1,
                WaterExchangeUnits = 4,
                LaborUnits = 1,
                Seed = 17,
                SaveRevision = 1,
                EventId = "event_d11_live_fish_maintenance_test",
            });
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(quantityAfterLoad, restoredHold.Snapshot.FishQuantity);
        }

        [Test]
        public void IslandPondConsumesFryFeedLaborOutputsCargoAndSendsPressureToEcology()
        {
            var trade = new D3TradeState(500, 100);
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("nearshore_reef_edge", 0, 9);
            ecology.SeedFishSchool(D11AquacultureIds.FishSchoolId, "nearshore_saint_marys", 0, 9);
            D11AquacultureInventoryDefinitionsForTest.AddInputs(trade.Inventory, 8, 16);
            var log = new EventLog();
            var farm = new D11IslandAquacultureSystem(null, trade, ecology, log);

            var built = farm.Build(new D11AquacultureBuildRequest
            {
                Kind = D11AquacultureFacilityKind.FishPond,
                FacilityId = "pond_saint_marys_01",
                LocationRef = "island_saint_marys",
                HabitatId = "nearshore_reef_edge",
                WorldDay = 0,
                SaveRevision = 1,
                EventId = "event_d11_pond_build_test",
            });
            Assert.IsTrue(built.Succeeded, built.Message);
            var supplied = farm.SupplyInputs(new D11AquacultureInputRequest
            {
                FacilityId = "pond_saint_marys_01",
                FryQuantity = 8,
                FeedQuantity = 16,
                LaborUnits = 3,
                WorldDay = 0,
                SaveRevision = 2,
                EventId = "event_d11_pond_inputs_test",
            });
            Assert.IsTrue(supplied.Succeeded, supplied.Message);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(trade.Inventory, D11AquacultureIds.FryItemId));
            Assert.AreEqual(0, D3InventorySystem.CountCargo(trade.Inventory, D11AquacultureIds.FeedItemId));

            var advanced = farm.Advance(new D11AquacultureAdvanceRequest
            {
                FacilityId = "pond_saint_marys_01",
                WorldDay = 1,
                WaterExchangeUnits = 3,
                Seed = 23,
                SaveRevision = 3,
                EventId = "event_d11_pond_advance_test",
            });
            Assert.IsTrue(advanced.Succeeded, advanced.Message);
            Assert.IsNotNull(advanced.Fact);
            Assert.IsTrue(advanced.Fact.CauseIds.Contains("event_d11_pond_inputs_test"));
            Assert.Greater(ecology.Snapshot.Habitat.Pollution, 0f);
            Assert.Greater(ecology.Snapshot.FishSchools[0].FishingPressure, 0);

            var harvested = farm.Harvest(new D11AquacultureHarvestRequest
            {
                FacilityId = "pond_saint_marys_01",
                Quantity = 1,
                WorldDay = 2,
                SaveRevision = 4,
                EventId = "event_d11_pond_harvest_test",
            });
            Assert.IsTrue(harvested.Succeeded, harvested.Message);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(trade.Inventory, D11AquacultureIds.FishCargoItemId));
            Assert.IsTrue(harvested.Fact.CauseIds.Contains("event_d11_pond_advance_test"));
            Assert.Greater(log.TraceCauses("event_d11_pond_harvest_test", 4).Count, 0);

            var snapshot = farm.CaptureRuntime().Clone();
            var restored = new D11IslandAquacultureSystem(snapshot.IslandAquaculture, trade, ecology,
                EventLog.FromSnapshot(snapshot.EventLog));
            var duplicate = restored.Harvest(new D11AquacultureHarvestRequest
            {
                FacilityId = "pond_saint_marys_01",
                Quantity = 1,
                WorldDay = 2,
                SaveRevision = 4,
                EventId = "event_d11_pond_harvest_test",
            });
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(trade.Inventory, D11AquacultureIds.FishCargoItemId));
        }

        [Test]
        public void AquacultureRuntimeSaveLoadKeepsStateAndFactLog()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "水产存档测试" });
            var runtime = new D11AquacultureRuntimeSnapshot
            {
                LiveFishHold = new D11LiveFishHoldState(),
                IslandAquaculture = new D11IslandAquacultureState(),
                EventLog = new EventLog().CaptureSnapshot(),
            };
            SaveSystem.SaveD11AquacultureRuntime(runtime);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD11AquacultureRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(D11AquacultureIds.LiveFishHoldId, loaded.LiveFishHold.FacilityId);
            Assert.AreEqual(D11AquacultureIds.SchemaVersion, loaded.SchemaVersion);
            Assert.IsNotNull(loaded.EventLog);
        }

        [Test]
        public void UnknownFacilityAndMissingEcologyHaveExplicitResults()
        {
            var farm = new D11IslandAquacultureSystem();
            var unknown = farm.Build(new D11AquacultureBuildRequest
            {
                FacilityId = "unknown",
                Kind = (D11AquacultureFacilityKind)99,
                EventId = "event_d11_unknown_facility",
            });
            Assert.IsFalse(unknown.Succeeded);
            Assert.AreEqual("facility_definition_missing", unknown.Code);
        }
    }

    internal static class D11AquacultureInventoryDefinitionsForTest
    {
        public static void AddInputs(D3InventoryState inventory, int fry, int feed)
        {
            D3InventorySystem.RegisterCargoDefinition(D11AquacultureIds.FryItemId, 1, 1);
            D3InventorySystem.RegisterCargoDefinition(D11AquacultureIds.FeedItemId, 1, 1);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, D11AquacultureIds.FryItemId, fry, out string fryError), fryError);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, D11AquacultureIds.FeedItemId, feed, out string feedError), feedError);
        }
    }
}
