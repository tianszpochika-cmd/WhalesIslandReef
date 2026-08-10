using NUnit.Framework;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

namespace D11.Tests
{
    public sealed class D11RareAnimalCollectionTests
    {
        [Test]
        public void DiscoveryObservationAndEvidenceKeepFirstLocationAndBehavior()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var log = new EventLog();
            var system = new D11RareAnimalCollectionSystem(ecology, log, new D4FactionSystem());
            Assert.IsTrue(system.Discover(Input("photo:discovery", 1)).Succeeded);
            Assert.IsTrue(system.Identify(new D11IdentificationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "field_note:identification",
            }).Succeeded);
            var observed = system.Observe(new D11ObservationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "photo:behavior", Weather = D9WeatherSeaSnapshot.CreateDefault(),
            });
            Assert.IsTrue(observed.Succeeded, observed.Message);
            Assert.IsTrue(observed.Evidence.Verified);
            Assert.AreEqual("island_saint_marys", system.Snapshot.Book[0].FirstLocationRef);
            Assert.AreEqual("wet", system.Snapshot.Book[0].FirstSeason);
            Assert.IsFalse(string.IsNullOrEmpty(system.Snapshot.Book[0].FirstBehavior));
            Assert.IsNotNull(log.Find(observed.EventId, 0));
        }

        [Test]
        public void LegalTransferCollectionAndReleaseAreDistinctRoutes()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var log = new EventLog();
            var system = new D11RareAnimalCollectionSystem(ecology, log, new D4FactionSystem());
            Prepare(system);
            var transfer = system.StartLegalTransfer(new D11LegalTransferRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 3,
                Season = "wet", EvidenceRef = "permit:transfer", CollectorId = "collector_saint_marys_researcher",
                DestinationRef = "island_saint_marys_research_center", DaysInTransit = 1, Seed = 11,
            });
            Assert.IsTrue(transfer.Succeeded, transfer.Message);
            var arrived = system.AdvanceTransport(new D11TransportAdvanceRequest
            {
                TransportId = transfer.TransportId, WorldDay = 4, Weather = D9WeatherSeaSnapshot.CreateDefault(),
            });
            Assert.IsTrue(arrived.Succeeded, arrived.Message);
            Assert.AreEqual(D11TransportStatus.Arrived, system.Snapshot.Transports[0].Status);
            Assert.IsTrue(system.Collect(new D11CollectionRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys_research_center", WorldDay = 4,
                Season = "wet", EvidenceRef = "receipt:collection", CollectorId = "collector_saint_marys_researcher",
                TransportId = transfer.TransportId,
            }).Succeeded);
            Assert.IsTrue(system.Snapshot.Book[0].Collected);

            var releaseEcology = new D4EcologySystem();
            releaseEcology.SeedHabitat("beach", 0, 21);
            var release = new D11RareAnimalCollectionSystem(releaseEcology, new EventLog(), new D4FactionSystem());
            Prepare(release);
            Assert.IsTrue(release.Release(new D11ReleaseRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
                Season = "wet", EvidenceRef = "report:release", ProtectedAreaId = "protected_area_saint_marys_coast",
            }).Succeeded);
            Assert.IsTrue(release.Snapshot.Book[0].Released);
        }

        [Test]
        public void TransportMortalityIsDeterministicAcrossRuntimeSnapshotRestore()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var log = new EventLog();
            var factions = new D4FactionSystem();
            var system = new D11RareAnimalCollectionSystem(ecology, log, factions);
            Prepare(system);
            var transfer = system.StartLegalTransfer(new D11LegalTransferRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 3,
                Season = "wet", EvidenceRef = "permit:restore", CollectorId = "collector_saint_marys_researcher",
                DestinationRef = "island_saint_marys_research_center", DaysInTransit = 2, Seed = 77,
            });
            Assert.IsTrue(transfer.Succeeded);
            var saved = new D4RuntimeSnapshot { Ecology = ecology.Snapshot, RareAnimals = system.Snapshot };
            var restored = new D11RareAnimalCollectionSystem(new D4EcologySystem(saved.Ecology),
                EventLog.FromSnapshot(log.CaptureSnapshot()), factions, saved.RareAnimals);
            var weather = D9WeatherSeaSnapshot.CreateDefault();
            var first = system.AdvanceTransport(new D11TransportAdvanceRequest
            {
                TransportId = transfer.TransportId, WorldDay = 4, Weather = weather,
            });
            var second = restored.AdvanceTransport(new D11TransportAdvanceRequest
            {
                TransportId = transfer.TransportId, WorldDay = 4, Weather = weather,
            });
            Assert.AreEqual(first.Code, second.Code);
            Assert.AreEqual(system.Snapshot.Transports[0].Status, restored.Snapshot.Transports[0].Status);
            Assert.IsTrue(system.AdvanceTransport(new D11TransportAdvanceRequest
            {
                TransportId = transfer.TransportId, WorldDay = 4, Weather = weather,
            }).Duplicate);
        }

        [Test]
        public void StormTransportCanProduceEcologyOwnedMortality()
        {
            var storm = D9WeatherSeaSnapshot.CreateDefault();
            storm.Weather = D9WeatherState.Storm;
            storm.Visibility = 0.1f;
            storm.CurrentSpeed = 1f;
            storm.CurrentDirectionDegrees = 90f;
            bool foundMortality = false;
            for (int seed = 0; seed < 32 && !foundMortality; seed++)
            {
                var ecology = new D4EcologySystem();
                ecology.SeedHabitat("beach", 0, 21);
                var system = new D11RareAnimalCollectionSystem(ecology, new EventLog(), new D4FactionSystem());
                Prepare(system);
                var transfer = system.StartLegalTransfer(new D11LegalTransferRequest
                {
                    SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 3,
                    Season = "wet", EvidenceRef = "permit:storm", CollectorId = "collector_saint_marys_researcher",
                    DestinationRef = "island_saint_marys_research_center", DaysInTransit = 1, Seed = seed,
                });
                var result = system.AdvanceTransport(new D11TransportAdvanceRequest
                {
                    TransportId = transfer.TransportId, WorldDay = 4, Weather = storm,
                });
                if (!result.Mortality) continue;
                foundMortality = true;
                Assert.IsTrue(system.Snapshot.Book[0].Dead);
                Assert.AreEqual(D11TransportStatus.Died, system.Snapshot.Transports[0].Status);
                Assert.AreEqual(D11AnimalLifecycleState.Dead, ecology.Snapshot.Animals[0].Lifecycle);
            }
            Assert.IsTrue(foundMortality, "storm mortality should be reachable with deterministic seeds");
        }

        [Test]
        public void IllegalTradeRecordsLawAndReputationConsequencesWithoutGeneratingAnimal()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("beach", 0, 21);
            var factions = new D4FactionSystem();
            var log = new EventLog();
            var system = new D11RareAnimalCollectionSystem(ecology, log, factions);
            var result = system.IllegalTrade(new D11IllegalTradeRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
                Season = "wet", EvidenceRef = "law_report:illegal_trade", ControllingFactionId = "england",
            });
            Assert.IsTrue(result.Succeeded, result.Message);
            Assert.Greater(result.LegalConsequence.WantedDelta, 0);
            Assert.Less(factions.GetReputation("england"), 0);
            Assert.AreEqual(0, system.Snapshot.Book.Count);
            Assert.IsNotNull(log.Find(result.EventId, 0));
        }

        private static D11DiscoveryRequest Input(string evidence, int day)
        {
            return new D11DiscoveryRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = day,
                Season = "wet", EvidenceRef = evidence,
            };
        }

        private static void Prepare(D11RareAnimalCollectionSystem system)
        {
            Assert.IsTrue(system.Discover(Input("photo:discover", 1)).Succeeded);
            Assert.IsTrue(system.Identify(new D11IdentificationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "note:identify",
            }).Succeeded);
            Assert.IsTrue(system.Observe(new D11ObservationRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 1,
                Season = "wet", EvidenceRef = "photo:observe", Weather = D9WeatherSeaSnapshot.CreateDefault(),
            }).Succeeded);
            Assert.IsTrue(system.Rescue(new D11RescueRequest
            {
                SpeciesId = "animal_green_turtle", LocationRef = "island_saint_marys", WorldDay = 2,
                Season = "wet", EvidenceRef = "report:rescue",
            }).Succeeded);
        }
    }
}
