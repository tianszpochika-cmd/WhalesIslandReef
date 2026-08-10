// D13-A03 农业测试：作物标签、季节、生态后果、库存边界、加工销售和存档幂等。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13FarmingSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_farming_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void FullCycle_StormPestHarvestProcessSellAndSaveRestoreIsIdempotent()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "农业测试船长", ShipTypeId = "small_fishing" });
            var eventLog = CreateSources();
            var inventory = new D3InventoryState(500);
            D13FarmingCatalog.RegisterInventoryDefinitions();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "item_seed_cassava", 1, out string seedError), seedError);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, D13FarmingCatalog.PestTreatmentItemId, 1, out string treatmentError), treatmentError);
            var ecology = new D4EcologySystem();
            var economy = new FixtureEconomy(eventLog, true);
            var farming = new D13FarmingSystem(inventory, ecology, new FixtureLand(true), economy, eventLog);

            var planted = farming.Plant(new D13FarmingPlantRequest
            {
                OperationId = "plant_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                CropId = D13FarmingCatalog.CassavaCropId,
                SeedItemId = "item_seed_cassava",
                WorldDay = 65,
                SaveRevision = 10,
                Seed = 17,
            });
            Assert.IsTrue(planted.Succeeded, planted.Message);
            Assert.AreEqual(D13FarmingCropKind.Vegetable, planted.Crop.Kind);
            Assert.IsTrue(planted.Crop.Tags.Contains("vegetable"));
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "item_seed_cassava"));
            Assert.IsNotNull(ecology.FindPlot(D13FarmingCatalog.CassavaPlotId));

            var watered = farming.Water(new D13FarmingWaterRequest
            {
                OperationId = "water_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                WaterUnits = 20,
                WorldDay = 66,
                SaveRevision = 11,
            });
            Assert.IsTrue(watered.Succeeded, watered.Message);

            var labor = farming.AssignLabor(new D13FarmingLaborRequest
            {
                OperationId = "labor_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                LaborUnits = 2,
                WorldDay = 66,
                SaveRevision = 12,
            });
            Assert.IsTrue(labor.Succeeded, labor.Message);

            var advanced = farming.AdvanceDay(new D13FarmingAdvanceRequest
            {
                OperationId = "advance_cassava_storm",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                Days = 3,
                WorldDay = 69,
                SaveRevision = 13,
                Storm = true,
                StormIntensity = 80,
                Rainfall = true,
                Seed = 23,
            });
            Assert.IsTrue(advanced.Succeeded, advanced.Message);
            Assert.Greater(advanced.Plot.GrowthPoints, 0);
            Assert.Greater(advanced.Plot.StormDamage, 0);

            var treated = farming.TreatPests(new D13FarmingPestTreatmentRequest
            {
                OperationId = "treat_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                TreatmentItemId = D13FarmingCatalog.PestTreatmentItemId,
                TreatmentUnits = 1,
                Skill = 4,
                WorldDay = 70,
                SaveRevision = 14,
            });
            Assert.IsTrue(treated.Succeeded, treated.Message);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, D13FarmingCatalog.PestTreatmentItemId));

            var harvested = farming.Harvest(new D13FarmingHarvestRequest
            {
                OperationId = "harvest_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                WorldDay = 70,
                SaveRevision = 15,
            });
            Assert.IsTrue(harvested.Succeeded, harvested.Message);
            Assert.GreaterOrEqual(harvested.Quantity, 2);
            Assert.AreEqual(D13FarmingCropStateKind.Empty, farming.FindPlotState(D13FarmingCatalog.CassavaPlotId).State);
            Assert.Greater(ecology.FindPlot(D13FarmingCatalog.CassavaPlotId).EcologyPressure, 0);

            int harvestQuantity = harvested.Quantity;
            var processed = farming.Process(new D13FarmingProcessRequest
            {
                OperationId = "process_cassava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                InputItemId = "item_vegetable_cassava",
                Quantity = 2,
                WorldDay = 71,
                SaveRevision = 16,
            });
            Assert.IsTrue(processed.Succeeded, processed.Message);
            Assert.AreEqual("item_cassava_flour", processed.OutputItemId);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_cassava_flour"));

            var sold = farming.Sell(new D13FarmingSaleRequest
            {
                OperationId = "sell_cassava_flour",
                ItemId = "item_cassava_flour",
                Quantity = 1,
                MarketRef = "market_green_cay",
                WorldDay = 72,
                SaveRevision = 17,
            });
            Assert.IsTrue(sold.Succeeded, sold.Message);
            Assert.AreEqual(12, sold.UnitPrice);
            Assert.AreEqual(12, sold.TotalPrice);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "item_cassava_flour"));
            Assert.AreEqual(0, economy.MoneyWritten, "农业销售不能直接写入玩家金钱。");

            var rotated = farming.Rotate(new D13FarmingRotationRequest
            {
                OperationId = "rotate_to_guava",
                PlotId = D13FarmingCatalog.CassavaPlotId,
                NextCropId = D13FarmingCatalog.GuavaCropId,
                WorldDay = 73,
                SaveRevision = 18,
            });
            Assert.IsTrue(rotated.Succeeded, rotated.Message);
            Assert.AreEqual(2, farming.FindPlotState(D13FarmingCatalog.CassavaPlotId).RotationIndex);

            var snapshot = farming.CaptureSnapshot();
            snapshot.Validate();
            SaveSystem.SaveD13FarmingRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load(), "农业快照必须能随现有 SaveSystem 读档。");
            var loaded = SaveSystem.LoadD13FarmingRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(snapshot.Sales.Count, loaded.Sales.Count);
            Assert.AreEqual(snapshot.Processes.Count, loaded.Processes.Count);

            var restoredLog = new EventLog();
            var restoredEcology = new D4EcologySystem();
            var restored = new D13FarmingSystem(inventory, restoredEcology, new FixtureLand(true), new FixtureEconomy(restoredLog, true), restoredLog, loaded);
            int cargoBeforeDuplicate = D3InventorySystem.CountCargo(inventory, "item_cassava_flour");
            var duplicateSale = restored.Sell(new D13FarmingSaleRequest
            {
                OperationId = "sell_cassava_flour", ItemId = "item_cassava_flour", Quantity = 1,
                MarketRef = "market_green_cay", WorldDay = 72, SaveRevision = 17,
            });
            Assert.IsTrue(duplicateSale.Duplicate, "读档后重复销售必须由农业快照的幂等键拦截。");
            Assert.AreEqual(cargoBeforeDuplicate, D3InventorySystem.CountCargo(inventory, "item_cassava_flour"));

            var duplicateHarvest = restored.Harvest(new D13FarmingHarvestRequest
            {
                OperationId = "harvest_cassava", PlotId = D13FarmingCatalog.CassavaPlotId, WorldDay = 70, SaveRevision = 15,
            });
            Assert.IsTrue(duplicateHarvest.Duplicate, "读档后重复收获不能再次生成货物。");
        }

        [Test]
        public void CropKindsTagsAndSeasonsAreDistinctAndDeterministic()
        {
            var expected = new Dictionary<string, D13FarmingCropKind>
            {
                [D13FarmingCatalog.GuavaCropId] = D13FarmingCropKind.Fruit,
                [D13FarmingCatalog.CassavaCropId] = D13FarmingCropKind.Vegetable,
                [D13FarmingCatalog.SugarcaneCropId] = D13FarmingCropKind.Sugarcane,
                [D13FarmingCatalog.TobaccoCropId] = D13FarmingCropKind.Tobacco,
                [D13FarmingCatalog.CoffeeCropId] = D13FarmingCropKind.Coffee,
                [D13FarmingCatalog.CocoaCropId] = D13FarmingCropKind.Cocoa,
                [D13FarmingCatalog.SpiceCropId] = D13FarmingCropKind.Spice,
                [D13FarmingCatalog.HerbCropId] = D13FarmingCropKind.Herb,
            };
            foreach (var item in expected)
            {
                var crop = D13FarmingCatalog.FindCrop(item.Key);
                Assert.IsNotNull(crop);
                Assert.AreEqual(item.Value, crop.Kind);
                Assert.IsTrue(crop.Tags.Count >= 2);
                Assert.IsTrue(crop.Tags.Contains(item.Value.ToString().ToLowerInvariant()) || item.Value == D13FarmingCropKind.Fruit);
                Assert.IsTrue(crop.AllowedSeasons.Contains(D13FarmingSeason.Dry));
                Assert.IsTrue(crop.AllowedSeasons.Contains(D13FarmingSeason.Wet));
                Assert.IsFalse(crop.AllowedSeasons.Contains(D13FarmingSeason.Hurricane));
            }
            Assert.AreEqual(D13FarmingSeason.Dry, D13FarmingSystem.SeasonFor(0));
            Assert.AreEqual(D13FarmingSeason.Wet, D13FarmingSystem.SeasonFor(65));
            Assert.AreEqual(D13FarmingSeason.Hurricane, D13FarmingSystem.SeasonFor(105));
        }

        [Test]
        public void DeniedLandAndEconomyLeaveInventoryAndCanonicalStateUnchanged()
        {
            var eventLog = CreateSources();
            var inventory = new D3InventoryState(100);
            D13FarmingCatalog.RegisterInventoryDefinitions();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "item_seed_cassava", 1, out string addError), addError);
            var denied = new D13FarmingSystem(inventory, new D4EcologySystem(), new FixtureLand(false), new FixtureEconomy(eventLog, false), eventLog);
            var plant = denied.Plant(new D13FarmingPlantRequest
            {
                OperationId = "denied_plant", PlotId = D13FarmingCatalog.CassavaPlotId, CropId = D13FarmingCatalog.CassavaCropId,
                SeedItemId = "item_seed_cassava", WorldDay = 65, SaveRevision = 20,
            });
            Assert.IsFalse(plant.Succeeded);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_seed_cassava"));
            Assert.AreEqual(D13FarmingCropStateKind.Empty, denied.FindPlotState(D13FarmingCatalog.CassavaPlotId).State);

            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "item_cassava_flour", 1, out addError), addError);
            var sale = denied.Sell(new D13FarmingSaleRequest
            {
                OperationId = "denied_sale", ItemId = "item_cassava_flour", Quantity = 1, WorldDay = 65, SaveRevision = 21,
            });
            Assert.IsFalse(sale.Succeeded);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_cassava_flour"));
            Assert.AreEqual(0, denied.CaptureSnapshot().Sales.Count);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "property_rights", 1, D13FarmingIds.PropertyOwner);
            AddSource(log, "legal_rights", 2, D13FarmingIds.LegalOwner);
            AddSource(log, "economy_sale", 3, D13FarmingIds.EconomyOwner);
            return log;
        }

        private static void AddSource(EventLog eventLog, string eventId, long saveRevision, string owner)
        {
            eventLog.Append(new WorldEvent(eventId, 0, "island_saint_marys", owner,
                new EventResult
                {
                    ResultCode = "fixture_source",
                    Summary = "fixture source",
                    FactIds = new List<string> { "fixture:" + eventId },
                    Values = new Dictionary<string, string> { ["source_owner_system"] = owner },
                }, new[] { D13FarmingIds.PlayerRef }, new[] { "fixture" }, new string[0], saveRevision));
        }

        private sealed class FixtureLand : ID13FarmingLandAuthority
        {
            private readonly bool _allowed;

            public FixtureLand(bool allowed) { _allowed = allowed; }

            public D13FarmingLandDecision ValidatePlot(D13FarmingPlotDefinition definition, string actorRef, int worldDay)
            {
                return _allowed
                    ? new D13FarmingLandDecision
                    {
                        Allowed = true, Code = "farming_right_approved", Message = "fixture land approved",
                        ControllerRef = D13FarmingIds.PlayerRef, PropertySourceEventId = "property_rights", LegalSourceEventId = "legal_rights",
                    }
                    : new D13FarmingLandDecision { Allowed = false, Code = "farming_right_required", Message = "fixture land denied" };
            }
        }

        private sealed class FixtureEconomy : ID13FarmingEconomyAuthority
        {
            private readonly EventLog _eventLog;
            private readonly bool _allowed;
            public int MoneyWritten { get; private set; }

            public FixtureEconomy(EventLog eventLog, bool allowed)
            {
                _eventLog = eventLog;
                _allowed = allowed;
            }

            public D13FarmingSaleDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision)
            {
                if (!_allowed) return new D13FarmingSaleDecision { Allowed = false, Code = "market_denied", Message = "fixture economy denied" };
                return new D13FarmingSaleDecision
                {
                    Allowed = true, Code = "sale_approved", Message = "fixture economy approved", UnitPrice = 12,
                    TotalPrice = quantity * 12, Currency = D13FarmingIds.Currency, MarketRef = marketRef,
                    LedgerRef = "ledger_farming_fixture", SourceEventId = "economy_sale", SourceOwnerSystem = D13FarmingIds.EconomyOwner,
                };
            }
        }
    }
}
