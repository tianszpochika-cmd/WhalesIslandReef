using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

namespace D11.Tests
{
    public sealed class D11AcceptanceIntegrationTests
    {
        [Test]
        public void WeatherAndTideChangeFishSchoolProjection()
        {
            var calmEnvironment = new EnvironmentSystem();
            var stormEnvironment = new EnvironmentSystem();
            var weatherLog = new EventLog();
            Assert.IsTrue(stormEnvironment.ApplyStormEvent(4, "nearshore_saint_marys", 1, weatherLog).Succeeded);

            var calmEcology = SeedFishEcology(17);
            var stormEcology = SeedFishEcology(17);
            var calm = calmEnvironment.Snapshot;
            var storm = stormEnvironment.Snapshot;
            var calmDayZero = calmEcology.ReadConditions(calm, 0);
            var calmDayOne = calmEcology.ReadConditions(calm, 1);
            Assert.AreNotEqual(calmDayZero.TidePhase, calmDayOne.TidePhase,
                "潮汐相位必须随世界日确定性变化。");
            Assert.AreNotEqual(calmDayZero.CurrentSpeed, calmDayOne.CurrentSpeed,
                "潮汐必须改变统一海流读取结果。");

            Assert.IsTrue(calmEcology.AdvanceEcology(4, calm, 17, new EventLog(), 2,
                "event_d11_s01_calm_advance").Applied);
            Assert.IsTrue(stormEcology.AdvanceEcology(4, storm, 17, new EventLog(), 2,
                "event_d11_s01_storm_advance").Applied);
            var calmConditions = calmEcology.ReadConditions(calm, 4);
            var stormConditions = stormEcology.ReadConditions(storm, 4);
            Assert.Greater(stormConditions.Turbidity, calmConditions.Turbidity);
            Assert.AreNotEqual(calmConditions.FishRecoveryMultiplier, stormConditions.FishRecoveryMultiplier);
            Assert.AreNotEqual(calmEcology.FindFishSchoolSnapshot("fish_mackerel_school").Recovery,
                stormEcology.FindFishSchoolSnapshot("fish_mackerel_school").Recovery,
                "天气差异必须进入鱼群恢复/迁徙投影。");
        }

        [Test]
        public void FishingChangesFishPressureAndEconomyPriceInputs()
        {
            var log = new EventLog();
            var trade = new D3TradeState(500, 100);
            var economy = new TradeEconomySystem(log);
            var fishing = new D11FishingHarvestSystem(trade, new D4EcologySystem(), economy, log);
            var before = economy.Quote(PortCatalog.StartingPortId, "item_fresh_mackerel", trade.Market,
                D3WeatherType.Calm);

            var harvest = fishing.Harvest(new D11HarvestRequest
            {
                LocationId = "nearshore_saint_marys",
                GearId = "gear_handline_basic",
                Day = 0,
                Seed = 7,
                DepthMeters = 2f,
                TimeOfDayBucket = "day",
                TideBand = "flood",
                SaveRevision = 1,
            });
            Assert.IsTrue(harvest.Succeeded);
            var fishAfter = fishing.EcologySystem.FindFishSchoolSnapshot("fish_mackerel_school");
            Assert.Greater(fishAfter.FishingPressure, 0);
            Assert.Less(fishAfter.Biomass, fishAfter.Capacity);
            Assert.AreEqual(harvest.Quantity,
                D3InventorySystem.CountCargo(trade.Inventory, harvest.ItemId));

            var sale = fishing.Sell(PortCatalog.StartingPortId, harvest.ItemId, harvest.Quantity, 0,
                D3WeatherType.Calm, 2);
            Assert.IsTrue(sale.Succeeded);
            var after = economy.Quote(PortCatalog.StartingPortId, harvest.ItemId, trade.Market,
                D3WeatherType.Calm);
            Assert.Greater(after.PlayerTradeVolume, before.PlayerTradeVolume);
            Assert.AreNotEqual(before.SupplyCoefficient, after.SupplyCoefficient,
                "捕捞后的鱼货出售必须改变 EconomySystem 的市场库存输入。");
            Assert.IsTrue(ContainsPriceReason(sale.PriceReasons, PriceReasonKind.PlayerTradeVolume));
        }

        [Test]
        public void LiveFishHoldRestoreDoesNotDuplicateSettlement()
        {
            var log = new EventLog();
            var trade = new D3TradeState(500, 64);
            var hold = new D11LiveFishHoldSystem(null, trade, log);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory, D11AquacultureIds.FryItemId, 3,
                out string fryError), fryError);
            Assert.IsTrue(hold.StockFry(new D11LiveFishStockRequest
            {
                Quantity = 3,
                WorldDay = 0,
                SaveRevision = 1,
                EventId = "event_d11_s01_live_stock",
            }).Succeeded);
            Assert.IsTrue(hold.Harvest(new D11AquacultureHarvestRequest
            {
                FishSpeciesId = D11AquacultureIds.FishSpeciesId,
                Quantity = 1,
                WorldDay = 1,
                SaveRevision = 2,
                EventId = "event_d11_s01_live_harvest",
            }).Succeeded);

            var savedHold = hold.Snapshot;
            var savedTrade = trade.Inventory.Clone();
            var savedLog = log.CaptureSnapshot();
            int cargoBeforeReplay = D3InventorySystem.CountCargo(savedTrade,
                D11AquacultureIds.FishCargoItemId);
            var restoredTrade = new D3TradeState(trade.Money, trade.Inventory.CargoCapacity);
            restoredTrade.Inventory.CargoWeightCapacity = trade.Inventory.CargoWeightCapacity;
            CopyCargo(savedTrade, restoredTrade.Inventory);
            var restoredLog = EventLog.FromSnapshot(savedLog);
            var restoredHold = new D11LiveFishHoldSystem(savedHold, restoredTrade, restoredLog);

            var duplicate = restoredHold.Harvest(new D11AquacultureHarvestRequest
            {
                FishSpeciesId = D11AquacultureIds.FishSpeciesId,
                Quantity = 1,
                WorldDay = 1,
                SaveRevision = 2,
                EventId = "event_d11_s01_live_harvest",
            });
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(cargoBeforeReplay,
                D3InventorySystem.CountCargo(restoredTrade.Inventory, D11AquacultureIds.FishCargoItemId));
            Assert.AreEqual(savedHold.FishQuantity, restoredHold.Snapshot.FishQuantity);
        }

        [Test]
        public void WildlifeRescueUpdatesCodexFactionAndQuestFact()
        {
            var log = new EventLog();
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var factions = new D4FactionSystem();
            var collection = new D11RareAnimalCollectionSystem(ecology, log, factions);
            var integration = new D11EcologyVoyageIntegrationSystem(collection, factions,
                new RelationSystem(log, new string[0]), log);
            PrepareRescue(collection, D9WeatherSeaSnapshot.CreateDefault());

            var request = RescueRequest(2, "rescue_report:s01");
            var integrated = integration.RescueRareAnimal(request);
            Assert.IsTrue(integrated.Succeeded);
            Assert.IsTrue(integrated.Rescue.BookEntry.Rescued);
            Assert.AreEqual(5, factions.GetReputation("england"));
            Assert.IsNotNull(integrated.FactionFact);
            Assert.IsNotNull(integrated.QuestFact);
            Assert.AreEqual("wildlife_rescue_completed", integrated.QuestFact.Result.ResultCode);
            Assert.IsTrue(integrated.QuestFact.Result.FactIds.Contains("quest:wildlife_rescue_completed"));
            Assert.Contains(integrated.Rescue.EventId, integrated.QuestFact.CauseIds);

            int factCount = log.Count;
            var duplicate = integration.RescueRareAnimal(request);
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(5, factions.GetReputation("england"));
            Assert.AreEqual(factCount, log.Count);
        }

        [Test]
        public void PetFactsEnterCrewAttachmentRelations()
        {
            var log = new EventLog();
            var crew = new List<CrewAgent>
            {
                new CrewAgent("crew_s01", new PersonaDefinition("老水手", "水手"), new InMemoryStore()),
            };
            var trade = new D3TradeState(500, 32);
            var pets = new D11PetSystem(trade, crew, log);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory, D11PetIds.FeedItemId, 2,
                out string feedError), feedError);
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var factions = new D4FactionSystem();
            var collection = new D11RareAnimalCollectionSystem(ecology, log, factions);
            var relations = new RelationSystem(log, new[] { "crew_s01" });
            var integration = new D11EcologyVoyageIntegrationSystem(collection, factions, relations, log);

            var adopted = pets.Adopt(D11PetIds.CatDefinitionId, "harbor:shelter_s01", 1, 1);
            Assert.IsTrue(adopted.Succeeded);
            var applied = integration.ApplyPetCrewRelations(adopted);
            Assert.IsTrue(applied.Succeeded);
            var edge = relations.ReadEdge("crew_s01", pets.Snapshot.PetId, RelationType.Attachment);
            Assert.IsNotNull(edge);
            Assert.Greater(edge.Value, 0f);
            Assert.IsTrue(edge.CauseEventIds.Contains(D11PetSystem.CrewReactionEventId));

            var duplicate = integration.ApplyPetCrewRelations(adopted);
            Assert.IsTrue(duplicate.Succeeded && duplicate.DuplicateCount > 0);
            Assert.AreEqual(edge.Value,
                relations.ReadEdge("crew_s01", pets.Snapshot.PetId, RelationType.Attachment).Value);
        }

        [Test]
        public void VegetationHarvestEntersCargoAndChangesEcology()
        {
            var log = new EventLog();
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("farm", 0, 23);
            var inventory = new D3InventoryState(64, 128);
            var system = new D11VegetationHarvestSystem(inventory, ecology, log);
            D11VegetationHarvestResult settled = null;
            for (int day = 0; day < 32 && settled == null; day++)
            {
                var attempt = system.Harvest(new D11VegetationHarvestRequest
                {
                    PlantId = D11VegetationIds.IndigoPlantId,
                    ClaimedPlantId = D11VegetationIds.IndigoPlantId,
                    ToolId = D11VegetationIds.HarvestingKnifeToolId,
                    RequestedQuantity = 1,
                    BotanySkill = 3,
                    Day = day,
                    Seed = 23,
                    SaveRevision = day + 1,
                    EventId = "event_d11_s01_vegetation_" + day,
                });
                if (attempt.Succeeded) settled = attempt;
            }
            Assert.IsNotNull(settled);
            Assert.Greater(D3InventorySystem.CountCargo(inventory, settled.ItemId), 0);
            Assert.IsNotNull(settled.Fact);
            Assert.AreEqual("EcologySystem", settled.Fact.OwnerSystem);
            Assert.Less(ecology.FindPlantSnapshot(settled.PlantId).Quantity,
                D11EcologyPlantCapacity(settled.PlantId));
        }

        [Test]
        public void AnimalBehaviorIsDeterministicWithoutLanguageModel()
        {
            var first = SeedAnimalEcology(31);
            var second = SeedAnimalEcology(31);
            var weather = D9WeatherSeaSnapshot.CreateDefault();
            Assert.IsTrue(first.AdvanceEcology(5, weather, 31, new EventLog(), 1,
                "event_d11_s01_animal_first").Applied);
            Assert.IsTrue(second.AdvanceEcology(5, weather, 31, new EventLog(), 1,
                "event_d11_s01_animal_second").Applied);
            var a = first.Snapshot.Animals[0];
            var b = second.Snapshot.Animals[0];
            Assert.AreEqual(a.Lifecycle, b.Lifecycle);
            Assert.AreEqual(a.Behavior, b.Behavior);
            Assert.AreEqual(a.Health, b.Health);
            Assert.AreEqual(a.EcologyStress, b.EcologyStress);
        }

        [Test]
        public void EcologyAndAnimalSnapshotsRestoreWithFacts()
        {
            var log = new EventLog();
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var factions = new D4FactionSystem();
            var collection = new D11RareAnimalCollectionSystem(ecology, log, factions);
            var integration = new D11EcologyVoyageIntegrationSystem(collection, factions,
                new RelationSystem(log, new string[0]), log);
            PrepareRescue(collection, D9WeatherSeaSnapshot.CreateDefault());
            var rescue = integration.RescueRareAnimal(RescueRequest(2, "rescue_report:s01_restore"));
            Assert.IsTrue(rescue.Succeeded);

            var saved = new D4RuntimeSnapshot
            {
                Ecology = ecology.Snapshot,
                Factions = factions.Snapshot,
                RareAnimals = collection.Snapshot,
            };
            var restoredLog = EventLog.FromSnapshot(log.CaptureSnapshot());
            var restoredEcology = new D4EcologySystem(saved.Ecology);
            var restoredFactions = new D4FactionSystem(saved.Factions);
            var restoredCollection = new D11RareAnimalCollectionSystem(restoredEcology, restoredLog,
                restoredFactions, saved.RareAnimals);
            var restoredIntegration = new D11EcologyVoyageIntegrationSystem(restoredCollection,
                restoredFactions, new RelationSystem(restoredLog, new string[0]), restoredLog);

            Assert.AreEqual(ecology.Snapshot.EcologyRevision, restoredEcology.Snapshot.EcologyRevision);
            Assert.AreEqual(D11AnimalLifecycleState.Rescued,
                restoredEcology.Snapshot.Animals[0].Lifecycle);
            Assert.IsTrue(restoredCollection.Snapshot.Book[0].Rescued);
            Assert.AreEqual(5, restoredFactions.GetReputation("england"));
            var duplicate = restoredIntegration.RescueRareAnimal(
                RescueRequest(2, "rescue_report:s01_restore"));
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(1, restoredCollection.Snapshot.Book.Count);
            Assert.AreEqual(5, restoredFactions.GetReputation("england"));
        }

        private static D4EcologySystem SeedFishEcology(int seed)
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("nearshore_reef_edge", 0, seed);
            ecology.SeedFishSchool("fish_mackerel_school", "nearshore_saint_marys", 0, seed);
            return ecology;
        }

        private static D4EcologySystem SeedAnimalEcology(int seed)
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, seed);
            var collection = new D11RareAnimalCollectionSystem(ecology, new EventLog(), new D4FactionSystem());
            Assert.IsTrue(collection.Discover(new D11DiscoveryRequest
            {
                SpeciesId = "animal_green_turtle",
                LocationRef = "island_saint_marys",
                WorldDay = 1,
                Season = "wet",
                EvidenceRef = "observation:s01_determinism",
            }).Succeeded);
            return ecology;
        }

        private static void PrepareRescue(D11RareAnimalCollectionSystem collection,
            D9WeatherSeaSnapshot weather)
        {
            Assert.IsTrue(collection.Discover(new D11DiscoveryRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "photo:s01_discovery",
            }).Succeeded);
            Assert.IsTrue(collection.Identify(new D11IdentificationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "field_note:s01_identification",
            }).Succeeded);
            Assert.IsTrue(collection.Observe(new D11ObservationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "photo:s01_observation", Weather = weather,
            }).Succeeded);
        }

        private static D11RescueRequest RescueRequest(int day, string evidence)
        {
            return new D11RescueRequest
            {
                SpeciesId = "animal_green_turtle",
                LocationRef = "island_saint_marys",
                WorldDay = day,
                Season = "wet",
                EvidenceRef = evidence,
                SaveRevision = 4,
                ActionId = "s01_rescue",
            };
        }

        private static int D11EcologyPlantCapacity(string plantId)
        {
            var definition = D11EcologyCatalog.FindPlant(plantId);
            return plantId == D11VegetationIds.IndigoPlantId ? 7 : int.MaxValue;
        }

        private static bool ContainsPriceReason(IReadOnlyList<PriceReason> reasons, PriceReasonKind kind)
        {
            if (reasons == null) return false;
            foreach (var reason in reasons)
                if (reason != null && reason.Kind == kind) return true;
            return false;
        }

        private static void CopyCargo(D3InventoryState source, D3InventoryState target)
        {
            foreach (var pair in source.Cargo) target.Cargo[pair.Key] = pair.Value;
            foreach (var pair in source.PersonalItems) target.PersonalItems[pair.Key] = pair.Value;
        }
    }
}
