using NUnit.Framework;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.EditMode
{
    public sealed class D11WhaleDolphinRoutesTests
    {
        [Test]
        public void MigrationIsDeterministicAndRecorded()
        {
            var a = new D11WhaleDolphinSystem(new EventLog());
            var b = new D11WhaleDolphinSystem(new EventLog());
            var ecology = new D11MarineEcologyReadSnapshot { HabitatId = "open_sea", EcologyStress = 0.12f };
            var environment = new D11MarineEnvironmentReadSnapshot { WeatherStateId = "Rain", CurrentSpeed = 0.4f };
            var left = a.AdvanceMigration("encounter_whale_001", "whale_humpback", D11MarineSpeciesKind.Whale,
                "route_westward", 7, 42, ecology, environment, saveRevision: 7);
            var right = b.AdvanceMigration("encounter_whale_001", "whale_humpback", D11MarineSpeciesKind.Whale,
                "route_westward", 7, 42, ecology, environment, saveRevision: 7);

            Assert.IsTrue(left.Succeeded);
            Assert.AreEqual(left.Record.Code, right.Record.Code);
            Assert.AreEqual(a.FindEncounter("encounter_whale_001").WorldX,
                b.FindEncounter("encounter_whale_001").WorldX);
            Assert.AreEqual(1, a.Snapshot.RouteRecords.Count);
            Assert.AreEqual(1, a.Snapshot.QuestFacts.Count);
        }

        [Test]
        public void ObservationEscortRescueAndAlternativeRouteAreAvailable()
        {
            var system = new D11WhaleDolphinSystem(new EventLog());
            system.AdvanceMigration("encounter_dolphin_001", "dolphin_common", D11MarineSpeciesKind.Dolphin,
                "route_coast", 1, 3, new D11MarineEcologyReadSnapshot(), new D11MarineEnvironmentReadSnapshot(), saveRevision: 1);
            var escort = system.Resolve(new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.Escort, EncounterId = "encounter_dolphin_001",
                WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(escort.Succeeded);
            Assert.AreEqual("dolphin_route_chart", escort.RewardRequests[0].RewardKind);

            var whale = new D11WhaleDolphinSystem(new EventLog());
            whale.AdvanceMigration("encounter_whale_stranded", "whale_humpback", D11MarineSpeciesKind.Whale,
                "route_shallow", 2, 3,
                new D11MarineEcologyReadSnapshot { IsStranded = true },
                new D11MarineEnvironmentReadSnapshot(), saveRevision: 2);
            var rescue = whale.Resolve(new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.StrandingRescue, EncounterId = "encounter_whale_stranded",
                WorldDay = 2, SaveRevision = 2, HasRescueKit = true, RescueCrewCount = 2,
            });
            Assert.IsTrue(rescue.Succeeded);
            Assert.IsTrue(whale.FindEncounter("encounter_whale_stranded").Rescued);

            var alternative = whale.Resolve(new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.AbandonTracking, EncounterId = "encounter_whale_stranded",
                WorldDay = 3, SaveRevision = 3,
            });
            Assert.IsTrue(alternative.Succeeded);
            Assert.AreEqual("abandon_tracking_alternative", alternative.RewardRequests[0].RewardKind);
            Assert.AreEqual(0, alternative.Record.SocialReputationDelta);
        }

        [Test]
        public void WhalingRequiresLicenseCrewShipFacilityLawAndReputation()
        {
            var log = new EventLog();
            var system = new D11WhaleDolphinSystem(log);
            system.AdvanceMigration("encounter_whale_legal", "whale_humpback", D11MarineSpeciesKind.Whale,
                "route_open", 12, 4, new D11MarineEcologyReadSnapshot(), new D11MarineEnvironmentReadSnapshot(), saveRevision: 12);
            var missing = system.Resolve(new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.Whaling, EncounterId = "encounter_whale_legal",
                WorldDay = 12, SaveRevision = 12, Seed = 0,
                WhalingPolicy = new D11WhalingPolicySnapshot(),
            });
            Assert.IsFalse(missing.Succeeded);
            Assert.AreEqual("license_required", missing.Code);
            Assert.AreEqual(0, missing.RewardRequests.Count);

            var valid = new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.Whaling, EncounterId = "encounter_whale_legal",
                WorldDay = 12, SaveRevision = 13, Seed = 0,
                WhalingPolicy = new D11WhalingPolicySnapshot
                {
                    LicenseStatus = D11WhalingLicenseStatus.Valid,
                    LicenseId = "license_saint_marys_001",
                    LawPolicy = D11WhalingLawPolicy.CommercialAllowed,
                    SeasonId = "dry",
                    RequiredSeasonId = "dry",
                    CrewCount = 6,
                    RequiredCrewCount = 4,
                    ShipClassId = "whaling_vessel",
                    RequiredShipClassId = "whaling_vessel",
                    HasProcessingFacility = true,
                    EquipmentReady = true,
                    SocialReputation = 30,
                    MinimumSocialReputation = 20,
                },
            };
            var resolved = system.Resolve(valid);
            Assert.IsTrue(resolved.Succeeded);
            Assert.AreEqual("licensed_whale_processing", resolved.RewardRequests[0].RewardKind);
            Assert.AreEqual(0.18f, resolved.Record.EcologyStressDelta);
            Assert.AreEqual(-2, resolved.Record.SocialReputationDelta);
            Assert.IsNotNull(log.Find(resolved.EventId, 13));
        }

        [Test]
        public void DuplicateResolutionDoesNotDuplicateRewardOrWorldState()
        {
            var log = new EventLog();
            var system = new D11WhaleDolphinSystem(log);
            system.AdvanceMigration("encounter_dolphin_duplicate", "dolphin_common", D11MarineSpeciesKind.Dolphin,
                "route_coast", 4, 1, new D11MarineEcologyReadSnapshot(), new D11MarineEnvironmentReadSnapshot(), saveRevision: 4);
            var request = new D11WhaleDolphinActionRequest
            {
                Action = D11MarineRouteAction.Observation, EncounterId = "encounter_dolphin_duplicate",
                WorldDay = 4, SaveRevision = 5,
            };
            var first = system.Resolve(request);
            var second = system.Resolve(request);
            Assert.IsTrue(first.Succeeded);
            Assert.IsTrue(second.Duplicate);
            Assert.AreEqual(1, system.Snapshot.RewardRequests.Count);
            Assert.AreEqual(2, system.Snapshot.RouteRecords.Count);
        }

        [Test]
        public void UnknownEnumAndMissingFieldsUseSafeDefaults()
        {
            var snapshot = new D11WhaleDolphinRuntimeSnapshot
            {
                Encounters = null, RouteRecords = null, RewardRequests = null,
                ConsequenceRequests = null, QuestFacts = null, AppliedEventKeys = null,
            };
            snapshot.EnsureDefaults();
            snapshot.Validate();
            Assert.AreEqual(D11MarineSpeciesKind.Unknown, new D11MarineEncounter().SpeciesKind);
            Assert.AreEqual(D11WhalingLawPolicy.Prohibited, new D11WhalingPolicySnapshot().LawPolicy);
            Assert.AreEqual(0, snapshot.Encounters.Count);
        }
    }
}
