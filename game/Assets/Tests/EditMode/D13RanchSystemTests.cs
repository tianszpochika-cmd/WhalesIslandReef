// D13-A04 养殖测试：来源/检疫、饲料饮水劳力、疾病恢复、死亡、空间、交易、繁殖边界和存档幂等。

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
    public sealed class D13RanchSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_ranch_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void ChickenSourceQuarantineCycleCollectSaleAndSaveRestoreAreIdempotent()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "牧场测试船长", ShipTypeId = "small_fishing" });
            var log = CreateSources();
            var inventory = NewInventory(100);
            AddCargo(inventory, D13RanchCatalog.FeedItemId, 5);
            var ecology = new D4EcologySystem();
            var ranch = NewRanch(inventory, ecology, log, true, true, true);

            var acquired = ranch.Acquire(new D13RanchAcquireRequest
            {
                OperationId = "acquire_chicken_01", RanchId = D13RanchCatalog.PoultryRanchId, DefinitionId = D13RanchCatalog.ChickenId,
                SourceRef = "chicken_01", SourceEventId = "animal_source_chicken_01", InspectionEventId = "animal_inspection_chicken_01",
                WorldDay = 10, SaveRevision = 10,
            });
            Assert.IsTrue(acquired.Succeeded, acquired.Message);
            Assert.AreEqual(D13RanchAnimalLifecycle.Quarantined, acquired.State.Lifecycle);
            Assert.AreEqual(11, acquired.State.QuarantineUntilDay);
            Assert.AreEqual(100, acquired.State.Health);

            var early = ranch.ReleaseQuarantine(new D13RanchQuarantineRequest
            {
                OperationId = "release_chicken_early", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                WorldDay = 10, SaveRevision = 11,
            });
            Assert.IsFalse(early.Succeeded);
            Assert.AreEqual("quarantine_incomplete", early.Code);

            var released = ranch.ReleaseQuarantine(new D13RanchQuarantineRequest
            {
                OperationId = "release_chicken", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                WorldDay = 11, SaveRevision = 12,
            });
            Assert.IsTrue(released.Succeeded, released.Message);

            var fed = ranch.Feed(new D13RanchFeedRequest
            {
                OperationId = "feed_chicken", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = 1, WorldDay = 12, SaveRevision = 13,
            });
            Assert.IsTrue(fed.Succeeded, fed.Message);
            var watered = ranch.Water(new D13RanchWaterOperationRequest
            {
                OperationId = "water_chicken", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = 2, WorldDay = 12, SaveRevision = 14,
            });
            Assert.IsTrue(watered.Succeeded, watered.Message);
            var labor = ranch.AssignLabor(new D13RanchLaborRequest
            {
                OperationId = "labor_chicken", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = 1, WorldDay = 12, SaveRevision = 15,
            });
            Assert.IsTrue(labor.Succeeded, labor.Message);

            var definition = ranch.FindDefinition(D13RanchCatalog.ChickenId);
            int safeSeed = FindSeed(definition, acquired.State.AnimalId, 13, false);
            var cycle = ranch.AdvanceCycle(new D13RanchCycleRequest
            {
                OperationId = "cycle_chicken_01", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Seed = safeSeed, WorldDay = 13, SaveRevision = 16,
            });
            Assert.IsTrue(cycle.Succeeded, cycle.Message);
            Assert.AreEqual(D13RanchAnimalLifecycle.ProductReady, cycle.State.Lifecycle);
            Assert.AreEqual("item_chicken_egg", cycle.ProductItemId);
            Assert.AreEqual(2, cycle.Quantity);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "item_chicken_egg"));

            var collected = ranch.CollectProduct(new D13RanchCollectProductRequest
            {
                OperationId = "collect_chicken_01", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                WorldDay = 14, SaveRevision = 17,
            });
            Assert.IsTrue(collected.Succeeded, collected.Message);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(inventory, "item_chicken_egg"));

            var sold = ranch.SellProduct(new D13RanchSaleRequest
            {
                OperationId = "sell_chicken_egg_01", ItemId = "item_chicken_egg", Quantity = 1,
                MarketRef = "market_santa_marys_poultry", WorldDay = 15, SaveRevision = 18,
            });
            Assert.IsTrue(sold.Succeeded, sold.Message);
            Assert.AreEqual(14, sold.UnitPrice);
            Assert.AreEqual(14, sold.TotalPrice);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_chicken_egg"));
            Assert.AreEqual(0, ((FixtureEconomy)ranchEconomy).MoneyWritten, "RanchSystem 不能直接写入金钱。");

            var snapshot = ranch.CaptureSnapshot();
            snapshot.Validate();
            SaveSystem.SaveD13RanchRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load(), "牧场快照必须随现有 SaveSystem 读档。");
            var loaded = SaveSystem.LoadD13RanchRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(snapshot.Animals.Count, loaded.Animals.Count);
            Assert.AreEqual(snapshot.Sales.Count, loaded.Sales.Count);
            Assert.AreEqual(snapshot.Water.Sources[0].Available, loaded.Water.Sources[0].Available);

            var restoredLog = new EventLog();
            var restored = NewRanch(inventory, new D4EcologySystem(), restoredLog, true, true, true, loaded);
            int cargoBeforeDuplicate = D3InventorySystem.CountCargo(inventory, "item_chicken_egg");
            var duplicateSale = restored.SellProduct(new D13RanchSaleRequest
            {
                OperationId = "sell_chicken_egg_01", ItemId = "item_chicken_egg", Quantity = 1,
                MarketRef = "market_santa_marys_poultry", WorldDay = 15, SaveRevision = 18,
            });
            Assert.IsTrue(duplicateSale.Duplicate, "读档后重复销售不能再次扣货或写入销售记录。");
            Assert.AreEqual(cargoBeforeDuplicate, D3InventorySystem.CountCargo(inventory, "item_chicken_egg"));
        }

        [Test]
        public void DiseaseTreatmentAndMissingInputsProduceTraceableDeath()
        {
            var log = CreateSources();
            var inventory = NewInventory(100);
            AddCargo(inventory, D13RanchCatalog.FeedItemId, 10);
            AddCargo(inventory, D13RanchCatalog.MedicineItemId, 1);
            var ranch = NewRanch(inventory, new D4EcologySystem(), log, true, true, true);
            var acquired = Acquire(ranch, D13RanchCatalog.GoatId, "goat_01", 20, 30);
            Release(ranch, acquired, 21, 31);
            var definition = ranch.FindDefinition(D13RanchCatalog.GoatId);
            AddCare(ranch, acquired, definition, 22, 32, "goat_care_01");
            int diseaseSeed = FindSeed(definition, acquired.State.AnimalId, 22, true);
            var diseased = ranch.AdvanceCycle(new D13RanchCycleRequest
            {
                OperationId = "cycle_goat_disease", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Seed = diseaseSeed, WorldDay = 22, SaveRevision = 33,
            });
            Assert.IsFalse(diseased.Succeeded);
            Assert.AreEqual("animal_diseased", diseased.Code);
            Assert.AreNotEqual(D13RanchDisease.None, diseased.State.Disease);
            Assert.AreEqual(D13RanchAnimalLifecycle.Sick, diseased.State.Lifecycle);

            var treated = ranch.TreatDisease(new D13RanchDiseaseTreatmentRequest
            {
                OperationId = "treat_goat", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = 1, WorldDay = 23, SaveRevision = 34,
            });
            Assert.IsTrue(treated.Succeeded, treated.Message);
            Assert.AreEqual(D13RanchDisease.None, treated.State.Disease);
            Assert.AreEqual(D13RanchAnimalLifecycle.Healthy, treated.State.Lifecycle);

            AddCare(ranch, acquired, definition, 24, 35, "goat_care_02");
            int safeSeed = FindSeed(definition, acquired.State.AnimalId, 24, false);
            var recovered = ranch.AdvanceCycle(new D13RanchCycleRequest
            {
                OperationId = "cycle_goat_recovered", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Seed = safeSeed, WorldDay = 24, SaveRevision = 36,
            });
            Assert.IsTrue(recovered.Succeeded, recovered.Message);

            var dead = Acquire(ranch, D13RanchCatalog.ChickenId, "chicken_death", 40, 40);
            Release(ranch, dead, 41, 41);
            for (int i = 0; i < 3; i++)
            {
                var failed = ranch.AdvanceCycle(new D13RanchCycleRequest
                {
                    OperationId = "death_missing_input_" + i, AnimalId = dead.State.AnimalId, RanchId = dead.State.RanchId,
                    WorldDay = 42 + i, SaveRevision = 42 + i,
                });
                Assert.IsFalse(failed.Succeeded);
            }
            Assert.AreEqual(D13RanchAnimalLifecycle.Dead, ranch.FindAnimal(dead.State.AnimalId).Lifecycle);
            Assert.AreEqual(0, ranch.FindAnimal(dead.State.AnimalId).Health);
            Assert.IsTrue(ranch.EventLog.Find("event_d13_ranch_death_missing_input_2", 44) != null);
        }

        [Test]
        public void SpaceSourceLandEconomyAndBreedingBoundariesAreDeterministic()
        {
            var log = CreateSources();
            var inventory = NewInventory(100);
            AddCargo(inventory, "item_chicken_egg", 1);
            var ranch = NewRanch(inventory, new D4EcologySystem(), log, true, true, false);
            for (int i = 0; i < 6; i++)
                Assert.IsTrue(Acquire(ranch, D13RanchCatalog.ChickenId, "space_source_" + i, 50 + i, 50 + i).Succeeded);
            var full = ranch.Acquire(new D13RanchAcquireRequest
            {
                OperationId = "space_full", RanchId = D13RanchCatalog.PoultryRanchId, DefinitionId = D13RanchCatalog.ChickenId,
                SourceRef = "space_source_full", SourceEventId = "animal_source_space_source_full", InspectionEventId = "animal_inspection_space_source_full",
                WorldDay = 60, SaveRevision = 60,
            });
            Assert.IsFalse(full.Succeeded);
            Assert.AreEqual("ranch_space_full", full.Code);

            var deniedLand = NewRanch(inventory, new D4EcologySystem(), log, false, true, false);
            var deniedAcquire = deniedLand.Acquire(new D13RanchAcquireRequest
            {
                OperationId = "land_denied", RanchId = D13RanchCatalog.PoultryRanchId, DefinitionId = D13RanchCatalog.ChickenId,
                SourceRef = "source_land_denied", SourceEventId = "animal_source_land_denied", InspectionEventId = "animal_inspection_land_denied",
                WorldDay = 70, SaveRevision = 70,
            });
            Assert.IsFalse(deniedAcquire.Succeeded);
            Assert.AreEqual(6, ranch.GetReadModel().Animals.Count);

            var deniedSale = ranch.SellProduct(new D13RanchSaleRequest
            {
                OperationId = "sale_denied", ItemId = "item_chicken_egg", Quantity = 1, WorldDay = 71, SaveRevision = 71,
            });
            Assert.IsFalse(deniedSale.Succeeded);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_chicken_egg"));

            var animal = ranch.GetReadModel().Animals[0];
            var breeding = ranch.RequestBreeding(new D13RanchBreedingRequest
            {
                OperationId = "breeding_deferred", AnimalId = animal.AnimalId, RanchId = animal.RanchId,
                MateAnimalId = ranch.GetReadModel().Animals[1].AnimalId, WorldDay = 72, SaveRevision = 72,
            });
            Assert.IsFalse(breeding.Succeeded);
            Assert.AreEqual("breeding_deferred", breeding.Code);
            Assert.AreEqual(6, ranch.GetReadModel().Animals.Count);
        }

        private D13RanchSystem NewRanch(D3InventoryState inventory, D4EcologySystem ecology, EventLog log, bool land, bool source, bool economy)
        {
            var fixtureEconomy = new FixtureEconomy(log, economy);
            ranchEconomy = fixtureEconomy;
            return new D13RanchSystem(inventory, ecology, new FixtureSource(source), new FixtureLand(land), fixtureEconomy, log);
        }

        private D13RanchSystem NewRanch(D3InventoryState inventory, D4EcologySystem ecology, EventLog log, bool land, bool source, bool economy, D13RanchRuntimeSnapshot initial)
        {
            var fixtureEconomy = new FixtureEconomy(log, economy);
            ranchEconomy = fixtureEconomy;
            return new D13RanchSystem(inventory, ecology, new FixtureSource(source), new FixtureLand(land), fixtureEconomy, log, initial);
        }

        private FixtureEconomy ranchEconomy;

        private static D3InventoryState NewInventory(int capacity)
        {
            D13RanchCatalog.RegisterInventoryDefinitions();
            return new D3InventoryState(capacity);
        }

        private static void AddCargo(D3InventoryState inventory, string itemId, int quantity)
        {
            string error;
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, itemId, quantity, out error), error);
        }

        private static D13RanchResult Acquire(D13RanchSystem ranch, string definitionId, string sourceRef, int day, long revision)
        {
            return ranch.Acquire(new D13RanchAcquireRequest
            {
                OperationId = "acquire_" + sourceRef, RanchId = definitionId == D13RanchCatalog.ChickenId ? D13RanchCatalog.PoultryRanchId : D13RanchCatalog.SmallholdingId,
                DefinitionId = definitionId, SourceRef = sourceRef, SourceEventId = "animal_source_" + sourceRef, InspectionEventId = "animal_inspection_" + sourceRef,
                WorldDay = day, SaveRevision = revision,
            });
        }

        private static void Release(D13RanchSystem ranch, D13RanchResult acquired, int day, long revision)
        {
            var result = ranch.ReleaseQuarantine(new D13RanchQuarantineRequest
            {
                OperationId = "release_" + acquired.State.AnimalId, AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                WorldDay = day, SaveRevision = revision,
            });
            Assert.IsTrue(result.Succeeded, result.Message);
        }

        private static void AddCare(D13RanchSystem ranch, D13RanchResult acquired, D13RanchAnimalDefinition definition, int day, long revision, string suffix)
        {
            var feed = ranch.Feed(new D13RanchFeedRequest
            {
                OperationId = suffix + "_feed", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = definition.FeedPerCycle, WorldDay = day, SaveRevision = revision,
            });
            Assert.IsTrue(feed.Succeeded, feed.Message);
            var water = ranch.Water(new D13RanchWaterOperationRequest
            {
                OperationId = suffix + "_water", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = definition.WaterPerCycle, WorldDay = day, SaveRevision = revision + 1,
            });
            Assert.IsTrue(water.Succeeded, water.Message);
            var labor = ranch.AssignLabor(new D13RanchLaborRequest
            {
                OperationId = suffix + "_labor", AnimalId = acquired.State.AnimalId, RanchId = acquired.State.RanchId,
                Quantity = definition.LaborRequired, WorldDay = day, SaveRevision = revision + 2,
            });
            Assert.IsTrue(labor.Succeeded, labor.Message);
        }

        private static int FindSeed(D13RanchAnimalDefinition definition, string animalId, int day, bool diseased)
        {
            for (int seed = 0; seed < 10000; seed++)
            {
                int roll = Stable(definition.DefinitionId + ":" + animalId + ":" + day + ":" + seed) % 100;
                if (diseased ? roll < definition.DiseaseRiskPercent : roll >= definition.DiseaseRiskPercent) return seed;
            }
            throw new AssertionException("没有找到满足养殖确定性疾病规则的种子。");
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "property_rights", D13RanchIds.PropertyOwner, 0);
            AddSource(log, "legal_rights", "LegalSystem", 0);
            AddSource(log, "economy_sale", D13RanchIds.EconomyOwner, 0);
            var sourceRefs = new List<string> { "chicken_01", "goat_01", "chicken_death" };
            for (int i = 0; i < 6; i++) sourceRefs.Add("space_source_" + i);
            foreach (var source in sourceRefs)
            {
                AddSource(log, "animal_source_" + source, D13RanchIds.SourceOwner, 0);
                AddSource(log, "animal_inspection_" + source, D13RanchIds.SourceOwner, 0);
            }
            AddSource(log, "animal_source_land_denied", D13RanchIds.SourceOwner, 0);
            AddSource(log, "animal_inspection_land_denied", D13RanchIds.SourceOwner, 0);
            AddSource(log, "animal_source_space_source_full", D13RanchIds.SourceOwner, 0);
            AddSource(log, "animal_inspection_space_source_full", D13RanchIds.SourceOwner, 0);
            return log;
        }

        private static void AddSource(EventLog log, string eventId, string owner, long revision)
        {
            log.Append(new WorldEvent(eventId, 0, "island_saint_marys", owner,
                new EventResult { ResultCode = "fixture_source", Summary = "fixture source", FactIds = new List<string> { "fixture:" + eventId } },
                new[] { D13RanchIds.PlayerRef }, new[] { "fixture" }, new string[0], revision));
        }

        private sealed class FixtureLand : ID13RanchLandAuthority
        {
            private readonly bool _allowed;
            public FixtureLand(bool allowed) { _allowed = allowed; }
            public D13RanchLandDecision ValidateFacility(D13RanchFacilityDefinition definition, string actorRef, int worldDay)
            {
                return _allowed
                    ? new D13RanchLandDecision { Allowed = true, Code = "land_approved", Message = "fixture land approved", PropertySourceEventId = "property_rights", LegalSourceEventId = "legal_rights" }
                    : new D13RanchLandDecision { Allowed = false, Code = "property_dispute", Message = "fixture land denied" };
            }
        }

        private sealed class FixtureSource : ID13RanchSourceAuthority
        {
            private readonly bool _allowed;
            public FixtureSource(bool allowed) { _allowed = allowed; }
            public D13RanchSourceDecision ValidateSource(D13RanchAnimalDefinition definition, string sourceRef, string sourceEventId, string actorRef, int worldDay)
            {
                return new D13RanchSourceDecision
                {
                    Allowed = _allowed, InspectionPassed = _allowed, Code = _allowed ? "source_approved" : "quarantine_failed",
                    Message = _allowed ? "fixture source approved" : "fixture source denied", SourceContentId = definition.SourceContentId,
                    SourceEventId = sourceEventId, InspectionEventId = "animal_inspection_" + sourceRef, SourceOwnerSystem = D13RanchIds.SourceOwner,
                    QuarantineDays = definition.QuarantineDays,
                };
            }
        }

        private sealed class FixtureEconomy : ID13RanchEconomyAuthority
        {
            private readonly EventLog _eventLog;
            private readonly bool _allowed;
            public int MoneyWritten { get; private set; }
            public FixtureEconomy(EventLog eventLog, bool allowed) { _eventLog = eventLog; _allowed = allowed; }
            public D13RanchEconomyDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision)
            {
                if (!_allowed) return new D13RanchEconomyDecision { Allowed = false, Code = "market_closed", Message = "fixture market denied" };
                return new D13RanchEconomyDecision
                {
                    Allowed = true, Code = "sale_approved", Message = "fixture economy approved", UnitPrice = 14, TotalPrice = quantity * 14,
                    Currency = D13RanchIds.Currency, MarketRef = marketRef, LedgerRef = "ledger_ranch_fixture", SourceEventId = "economy_sale", SourceOwnerSystem = D13RanchIds.EconomyOwner,
                };
            }
        }
    }
}
