using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace D12.Tests
{
    public sealed class D12TreasureClueTests
    {
        private static D9TreasureQuestSystem NewSystem(out EventLog log)
        {
            log = new EventLog();
            return new D9TreasureQuestSystem(log, D9TreasureQuestCatalog.CreateFirstChart());
        }

        private static void Accept(D9TreasureQuestSystem system)
        {
            Assert.IsTrue(system.HearRumor("player:captain", 1, 1).Accepted);
            Assert.IsTrue(system.AcceptRumor().Accepted);
        }

        private static D9TreasureLandingRequest Landing(string clueId, bool requireVerification,
            bool enforceWindow, int route = 1)
        {
            return new D9TreasureLandingRequest
            {
                ActorRef = "player:captain",
                LocationRef = D9TreasureQuestCatalog.SiteRef,
                ClueId = clueId,
                RouteChoice = route == 1 ? D9TreasureRouteChoice.WaitForTide : D9TreasureRouteChoice.VillageGuide,
                TideWindowOpen = true,
                RequireCrossValidation = requireVerification,
                EnforceClueWindow = enforceWindow,
                VillageGuideAvailable = true,
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            };
        }

        [Test]
        public void MultipleSourcesAreRecordedAndCrossValidatedWithCauseEvents()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);

            var chart = system.RecordCatalogClue("clue:first_chart_salvaged_chart", "player:captain", 2, 2);
            var keeper = system.RecordCatalogClue("clue:first_chart_reef_keeper", "player:captain", 2, 3);
            Assert.IsTrue(chart.Accepted);
            Assert.IsTrue(keeper.Accepted);
            Assert.AreEqual(3, system.Snapshot.Clues.Count);
            Assert.AreEqual(D9TreasureClueSourceKind.SalvagedChart, system.Snapshot.Clues[1].SourceKind);
            Assert.AreEqual(84, system.Snapshot.Clues[1].ReliabilityPercent);
            Assert.AreEqual("chart_fragment:first_chart_cove", system.Snapshot.Clues[1].EvidenceRef);

            var verified = system.CrossValidateClues(new D9TreasureClueVerificationRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:first_chart_tide_mark",
                OtherClueId = "clue:first_chart_salvaged_chart",
            }, 3, 4);

            Assert.IsTrue(verified.Accepted);
            Assert.AreEqual(1, system.Snapshot.CrossValidationCount);
            Assert.IsTrue(system.Snapshot.VerifiedClueIds.Contains("clue:first_chart_tide_mark"));
            Assert.IsTrue(system.Snapshot.VerifiedClueIds.Contains("clue:first_chart_salvaged_chart"));
            Assert.AreEqual("event_d9_treasure_clue", verified.Fact.CauseIds[0]);
            Assert.AreEqual(WorldEventStatus.Normal, verified.Fact.OutcomeStatus);
            Assert.IsTrue(log.TraceCauses(verified.Fact.EventId, verified.Fact.SaveRevision).Count >= 2);
        }

        [Test]
        public void VerificationGateFailsRecoverablyAndDoesNotRequireAnOpaquePuzzle()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            Assert.IsTrue(system.RecordCatalogClue("clue:first_chart_salvaged_chart", "player:captain", 2, 2).Accepted);

            var failed = system.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 3, 3);
            Assert.IsTrue(failed.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.ClueNotVerified, failed.Code);
            Assert.AreEqual(WorldEventStatus.Failed, failed.Fact.OutcomeStatus);
            Assert.AreEqual("route_cross_validate_clue", failed.RecoveryRoute.RouteId);
            Assert.AreEqual("false", failed.Fact.Result.Values["puzzle_gate"]);

            Assert.IsTrue(system.CrossValidateClues(new D9TreasureClueVerificationRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:first_chart_tide_mark",
                OtherClueId = "clue:first_chart_salvaged_chart",
            }, 4, 4).Accepted);
            Assert.IsTrue(system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_cross_validate_clue",
                ClueId = "clue:first_chart_salvaged_chart",
            }, 4, 5).Accepted);

            var resolved = system.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 5, 6);
            Assert.IsTrue(resolved.Accepted);
            Assert.AreEqual(D9TreasureQuestStatus.Discovered, system.Snapshot.Status);
            Assert.AreEqual(D9TreasureRewardType.Item, resolved.RewardRequest.RewardType);
        }

        [Test]
        public void TimeWindowAndEquipmentAreEffectiveAndCanRecover()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            Assert.IsTrue(system.RecordClue(new D9TreasureClueRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:first_chart_window",
                SourceKind = D9TreasureClueSourceKind.DirectObservation,
                SourceRef = "observation:coastline_mark",
                EvidenceRef = "observation:first_chart_window",
                Text = "白石和潮沟在第十至十二世界日可见。",
                ReliabilityPercent = 91,
                RegionRef = "island_saint_marys",
                SiteRef = D9TreasureQuestCatalog.SiteRef,
                ValidFromWorldDay = 10,
                ValidUntilWorldDay = 12,
                RequiredEquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
                RewardType = D9TreasureRewardType.Currency,
            }, 2, 2).Accepted);
            Assert.IsTrue(system.CrossValidateClues(new D9TreasureClueVerificationRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:first_chart_tide_mark",
                OtherClueId = "clue:first_chart_window",
            }, 3, 3).Accepted);

            var closed = system.TryLand(Landing("clue:first_chart_window", true, true), 5, 4);
            Assert.IsTrue(closed.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.ClueWindowClosed, closed.Code);
            Assert.AreEqual(WorldEventStatus.Failed, closed.Fact.OutcomeStatus);
            Assert.AreEqual("route_wait_tide", closed.RecoveryRoute.RouteId);

            Assert.IsTrue(system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_wait_tide",
                TideWindowOpen = true,
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            }, 6, 5).Accepted);

            var resolved = system.TryLand(Landing("clue:first_chart_window", true, true), 10, 6);
            Assert.IsTrue(resolved.Accepted);
            Assert.AreEqual(D9TreasureRewardType.Currency, resolved.RewardRequest.RewardType);
            Assert.AreEqual("pending", resolved.RewardRequest.Status);
        }

        [Test]
        public void SaveRestorePreservesCluesAndRewardCauseWithoutRepeatingDiscovery()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            Assert.IsTrue(system.RecordCatalogClue("clue:first_chart_salvaged_chart", "player:captain", 2, 2).Accepted);
            Assert.IsTrue(system.CrossValidateClues(new D9TreasureClueVerificationRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:first_chart_tide_mark",
                OtherClueId = "clue:first_chart_salvaged_chart",
            }, 3, 3).Accepted);
            Assert.IsTrue(system.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 4, 4).Accepted);

            var envelope = system.CaptureSaveEnvelope();
            var restored = D9TreasureQuestSystem.RestoreFromSave(envelope);
            var countBefore = restored.EventLog.Count;
            var duplicate = restored.TryLand(Landing("clue:first_chart_salvaged_chart", true, false), 5, 5);

            Assert.IsFalse(duplicate.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.AlreadyDiscovered, duplicate.Code);
            Assert.AreEqual(countBefore, restored.EventLog.Count);
            Assert.AreEqual(2, restored.Snapshot.Clues.Count);
            Assert.AreEqual(D9TreasureRewardType.Item, restored.Snapshot.RewardRequest.RewardType);
            CollectionAssert.Contains(restored.Snapshot.RewardRequest.CauseEventIds,
                "event_d12_treasure_clue_clue_first_chart_salvaged_chart");
        }

        [Test]
        public void IllegalClueConfigurationIsRejectedBeforeItReachesTheEventLog()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            var result = system.RecordClue(new D9TreasureClueRequest
            {
                ActorRef = "player:captain",
                ClueId = "clue:invalid_reliability",
                SourceKind = D9TreasureClueSourceKind.TavernRumor,
                SourceRef = "rumor:unknown",
                EvidenceRef = "evidence:unknown",
                ReliabilityPercent = 101,
                RegionRef = "island_saint_marys",
                SiteRef = D9TreasureQuestCatalog.SiteRef,
            }, 2, 2);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual("invalid_clue", result.Code);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(1, system.Snapshot.Clues.Count);
        }
    }
}
