using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D11.Tests
{
    public class D11PetSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d11_pet_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
            D11PetCatalog.Find(D11PetIds.CatDefinitionId);
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

        private static D3TradeState TradeWithFeed(int quantity = 4)
        {
            var trade = new D3TradeState(500, 20);
            D3InventorySystem.RegisterCargoDefinition(D11PetIds.FeedItemId, 1, 1);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory, D11PetIds.FeedItemId,
                quantity, out string error), error);
            return trade;
        }

        private static List<CrewAgent> Crew()
        {
            return new List<CrewAgent>
            {
                new CrewAgent("crew_pet_test", new PersonaDefinition("老水手", "水手"), new InMemoryStore()),
            };
        }

        [Test]
        public void AdoptionKeepsSource_AndCrewReactionIsASeparateFact()
        {
            var log = new EventLog();
            var system = new D11PetSystem(TradeWithFeed(), Crew(), log);

            var adopted = system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 2, 10);

            Assert.IsTrue(adopted.Succeeded);
            Assert.IsTrue(adopted.Applied);
            Assert.AreEqual("harbor:shelter_01", system.Snapshot.SourceRef);
            Assert.AreEqual(D11PetLifeState.Adopted, system.Snapshot.LifeState);
            Assert.AreEqual(D11PetLocation.Harbor, system.Snapshot.Location);
            Assert.AreEqual(1, adopted.CrewReactions.Count);
            Assert.AreEqual("adoption_welcome", adopted.CrewReactions[0].ReactionId);
            Assert.AreEqual(D11PetSystem.OwnerSystem, log.Find(D11PetSystem.AdoptionEventId, 10).OwnerSystem);

            var second = system.Adopt(D11PetIds.DogDefinitionId, "harbor:shelter_02", 2, 11);
            Assert.IsFalse(second.Succeeded);
            Assert.AreEqual("pet_already_owned", second.Code);
        }

        [Test]
        public void FeedCleanAndComfortUseInventoryOwner_AndAreIdempotent()
        {
            var trade = TradeWithFeed();
            var system = new D11PetSystem(trade, Crew());
            Assert.IsTrue(system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 1, 1).Succeeded);
            var before = system.Snapshot;

            var fed = system.Feed(2, 1, 2);
            Assert.IsTrue(fed.Applied);
            Assert.Less(system.Snapshot.Hunger, before.Hunger);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(trade.Inventory, D11PetIds.FeedItemId));

            var duplicate = system.Feed(1, 1, 2);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(trade.Inventory, D11PetIds.FeedItemId),
                "重复结算必须退回预扣的饲料。");

            Assert.IsTrue(system.Clean(1, 3).Applied);
            Assert.AreEqual(100, system.Snapshot.Cleanliness);
            Assert.IsTrue(system.Comfort(1, 4).Applied);
            Assert.LessOrEqual(system.Snapshot.Fear, 20);

            Assert.IsTrue(system.MoveTo(D11PetLocation.ShipCabin, 1, 5).Applied);
            Assert.AreEqual(D11PetLocation.ShipCabin, system.Snapshot.Location);
            Assert.IsFalse(system.MoveTo(D11PetLocation.Lost, 1, 6).Succeeded);
        }

        [Test]
        public void QuarantineTreatAndReleaseHaveARecoverablePath()
        {
            var system = new D11PetSystem(TradeWithFeed(), Crew());
            Assert.IsTrue(system.Adopt(D11PetIds.DogDefinitionId, "harbor:shelter_02", 1, 1).Succeeded);
            Assert.IsTrue(system.Quarantine(1, 2).Applied);
            Assert.AreEqual(D11PetLocation.Quarantine, system.Snapshot.Location);
            Assert.IsTrue(system.Treat(1, 3).Applied);
            Assert.IsTrue(system.AdvanceDay(2, 4).Applied);
            Assert.AreEqual(0, system.Snapshot.QuarantineDaysRemaining);

            var released = system.ReleaseFromQuarantine(2, 5);
            Assert.IsTrue(released.Applied);
            Assert.AreEqual(D11PetLifeState.Adopted, system.Snapshot.LifeState);
            Assert.AreEqual(D11PetLocation.Harbor, system.Snapshot.Location);
        }

        [Test]
        public void LostSearchRecoversTheSameInstance_WithoutRespawn()
        {
            var system = new D11PetSystem(TradeWithFeed(), Crew());
            Assert.IsTrue(system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 1, 1).Succeeded);
            string petId = system.Snapshot.PetId;
            Assert.IsTrue(system.MarkLost("storm_noise", 2, 2).Applied);

            D11PetActionResult found = null;
            for (int seed = 0; seed < 100 && found == null; seed++)
            {
                var attempt = system.Search("harbor:fish_market", true, 3, seed, 10 + seed);
                if (attempt.SearchFound) found = attempt;
            }

            Assert.IsNotNull(found, "确定性搜索应在验收种子范围内提供恢复路线。");
            Assert.AreEqual(petId, system.Snapshot.PetId);
            Assert.AreEqual(D11PetLifeState.Adopted, system.Snapshot.LifeState);
            Assert.AreEqual(-1, system.Snapshot.LostSinceDay);
            Assert.AreEqual(WorldEventStatus.Recovered, found.Fact.OutcomeStatus);
            Assert.Greater(system.EventLog.TraceCauses(D11PetSystem.SearchEventId, found.SaveRevision).Count, 0);
        }

        [Test]
        public void CatMouseControlAndDogWarningAreObservations_NotCombatOrWeatherWrites()
        {
            var cat = new D11PetSystem(TradeWithFeed());
            Assert.IsTrue(cat.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 1, 1).Succeeded);
            D11PetActionResult mouse = null;
            for (int seed = 0; seed < 100 && mouse == null; seed++)
            {
                var result = cat.CatchMouse(1, seed, 10 + seed);
                if (result.MouseCaught) mouse = result;
            }
            Assert.IsNotNull(mouse);
            Assert.AreEqual(1, cat.Snapshot.MouseCaughtCount);
            Assert.AreEqual(0, cat.Snapshot.WarningCount);

            var dog = new D11PetSystem(TradeWithFeed());
            Assert.IsTrue(dog.Adopt(D11PetIds.DogDefinitionId, "harbor:shelter_02", 1, 1).Succeeded);
            D11PetActionResult warning = null;
            for (int seed = 0; seed < 100 && warning == null; seed++)
            {
                var result = dog.Warn(1, seed, 10 + seed);
                if (result.WarningRaised) warning = result;
            }
            Assert.IsNotNull(warning);
            Assert.AreEqual("true", warning.Fact.Result.Values["observation_only"]);
            Assert.AreEqual(1, dog.Snapshot.WarningCount);
        }

        [Test]
        public void SaveLoadPreservesPetStateAndFactLog()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "宠物测试船长" });
            var system = new D11PetSystem(TradeWithFeed(), Crew());
            Assert.IsTrue(system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 1, 7).Succeeded);
            Assert.IsTrue(system.MarkLost("test_loss", 2, 8).Succeeded);
            var snapshot = system.CaptureSnapshot();

            SaveSystem.SaveD11PetRuntime(snapshot);
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD11PetRuntime();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(snapshot.Pet.PetId, loaded.Pet.PetId);
            Assert.AreEqual(D11PetLifeState.Lost, loaded.Pet.LifeState);
            Assert.AreEqual(snapshot.Pet.SourceRef, loaded.Pet.SourceRef);
            Assert.AreEqual(snapshot.EventLog.Facts.Count, loaded.EventLog.Facts.Count);
            FactEvent lost = null;
            foreach (var fact in loaded.EventLog.Facts)
                if (fact.EventId == D11PetSystem.LostEventId) lost = fact;
            Assert.IsNotNull(lost);
            Assert.AreEqual(WorldEventStatus.Interrupted, lost.OutcomeStatus);
        }

        [Test]
        public void DeadPetCannotBeRecreated()
        {
            var system = new D11PetSystem(TradeWithFeed());
            Assert.IsTrue(system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_01", 0, 1).Succeeded);
            Assert.IsTrue(system.AdvanceDay(100, 2).Succeeded);
            Assert.AreEqual(D11PetLifeState.Dead, system.Snapshot.LifeState);

            var rejected = system.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_03", 101, 3);
            Assert.IsFalse(rejected.Succeeded);
            Assert.AreEqual("pet_dead_no_respawn", rejected.Code);
        }
    }
}
