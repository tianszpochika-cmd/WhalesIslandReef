using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace D9.Tests
{
    public class D9TreasureQuestTests
    {
        private static D9TreasureQuestSystem NewSystem(out EventLog log)
        {
            log = new EventLog();
            return new D9TreasureQuestSystem(log, D9TreasureQuestCatalog.CreateFirstChart());
        }

        private static void Accept(D9TreasureQuestSystem system)
        {
            Assert.IsTrue(system.HearRumor("player:captain", 1, 10).Accepted);
            Assert.IsTrue(system.AcceptRumor().Accepted);
        }

        private static D9TreasureLandingRequest Landing(D9TreasureRouteChoice route)
        {
            return new D9TreasureLandingRequest
            {
                ActorRef = "player:captain",
                LocationRef = D9TreasureQuestCatalog.SiteRef,
                RouteChoice = route,
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
                TideWindowOpen = route == D9TreasureRouteChoice.WaitForTide,
                VillageGuideAvailable = route == D9TreasureRouteChoice.VillageGuide,
            };
        }

        [Test]
        public void RumorReliabilityIsLogged_AndAgentOnlyGetsExpressionProposal()
        {
            EventLog log;
            var system = NewSystem(out log);
            var proposal = system.BuildClueExpressionProposal("crew:isaac_reed", "潮线在北侧。可靠度由系统保留。");

            Assert.AreEqual(D9TreasureQuestStatus.RumorAvailable, system.Snapshot.Status);
            Assert.AreEqual(72, proposal.ReliabilityPercent);
            Assert.AreEqual(0, log.Count);

            var result = system.HearRumor("player:captain", 2, 11);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual("event_d9_treasure_clue", result.Fact.EventId);
            Assert.AreEqual("72", result.Fact.Result.Values["reliability_percent"]);
            Assert.AreEqual("false", result.Fact.Result.Values["puzzle_gate"]);
            Assert.AreEqual(D9TreasureQuestStatus.RumorHeard, system.Snapshot.Status);
        }

        [Test]
        public void MissingEquipmentFailsWithRecovery_ThenRetrySucceedsWithoutGuessing()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);

            var missing = system.TryLand(new D9TreasureLandingRequest
            {
                ActorRef = "player:captain",
                LocationRef = D9TreasureQuestCatalog.SiteRef,
                RouteChoice = D9TreasureRouteChoice.DirectLanding,
                EquipmentIds = new List<string>(),
            }, 3, 12);

            Assert.IsTrue(missing.Accepted);
            Assert.AreEqual("treasure_attempt_failed_equipment", missing.Code);
            Assert.AreEqual(WorldEventStatus.Failed, missing.Fact.OutcomeStatus);
            Assert.AreEqual("route_buy_chart_kit", missing.RecoveryRoute.RouteId);
            CollectionAssert.Contains(missing.MissingEquipment, D9TreasureQuestCatalog.RequiredEquipmentId);

            var recovered = system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_buy_chart_kit",
                EquipmentIds = new List<string> { D9TreasureQuestCatalog.RequiredEquipmentId },
            }, 4, 13);

            Assert.IsTrue(recovered.Accepted);
            Assert.AreEqual(WorldEventStatus.Recovered, recovered.Fact.OutcomeStatus);
            Assert.AreEqual(D9TreasureQuestStatus.Recovered, system.Snapshot.Status);

            var success = system.TryLand(Landing(D9TreasureRouteChoice.WaitForTide), 5, 14);

            Assert.IsTrue(success.Accepted);
            Assert.AreEqual("treasure_discovered", success.Code);
            Assert.AreEqual(D9TreasureQuestStatus.Discovered, system.Snapshot.Status);
            Assert.IsNotNull(success.RewardRequest);
            Assert.AreEqual("reward_first_chart", success.RewardRequest.RewardId);
            Assert.AreEqual(4, log.Count);
        }

        [Test]
        public void DangerousDirectLandingOffersTideAlternative()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);

            var failed = system.TryLand(Landing(D9TreasureRouteChoice.DirectLanding), 3, 20);

            Assert.IsTrue(failed.Accepted);
            Assert.AreEqual("treasure_attempt_failed_danger", failed.Code);
            Assert.AreEqual("route_wait_tide", failed.RecoveryRoute.RouteId);
            Assert.AreEqual("surf_break,unstable_shore", failed.Fact.Result.Values["danger_tags"]);

            var beforeWindow = system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_wait_tide",
                TideWindowOpen = false,
            }, 4, 21);
            Assert.IsFalse(beforeWindow.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.TideWindowClosed, beforeWindow.Code);

            var recovered = system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_wait_tide",
                TideWindowOpen = true,
            }, 5, 22);
            Assert.IsTrue(recovered.Accepted);

            var success = system.TryLand(Landing(D9TreasureRouteChoice.WaitForTide), 6, 23);
            Assert.IsTrue(success.Accepted);
            Assert.AreEqual("treasure_discovered", success.Code);
            Assert.AreEqual(4, log.Count);
        }

        [Test]
        public void SaveRestorePreservesCauseChainAndDoesNotRepeatDiscovery()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            Assert.IsTrue(system.TryLand(Landing(D9TreasureRouteChoice.WaitForTide), 2, 31).Accepted);

            var envelope = system.CaptureSaveEnvelope();
            var restored = D9TreasureQuestSystem.RestoreFromSave(envelope);
            int countBefore = restored.EventLog.Count;
            var duplicate = restored.TryLand(Landing(D9TreasureRouteChoice.DirectLanding), 3, 31);

            Assert.IsFalse(duplicate.Accepted);
            Assert.AreEqual(D9TreasureFailureCodes.AlreadyDiscovered, duplicate.Code);
            Assert.AreEqual(countBefore, restored.EventLog.Count);
            Assert.AreEqual(D9TreasureQuestStatus.Discovered, restored.Snapshot.Status);
            Assert.AreEqual(envelope.Quest.RewardRequest.RewardId, restored.Snapshot.RewardRequest.RewardId);
            Assert.AreEqual(envelope.EventLog.Facts.Count, restored.EventLog.Count);
        }

        [Test]
        public void FailedAndRecoveryFactsRemainTraceable()
        {
            EventLog log;
            var system = NewSystem(out log);
            Accept(system);
            var failed = system.TryLand(Landing(D9TreasureRouteChoice.DirectLanding), 2, 40);
            Assert.IsTrue(failed.Accepted);
            var recovered = system.Recover(new D9TreasureRecoveryRequest
            {
                ActorRef = "player:captain",
                RouteId = "route_wait_tide",
                TideWindowOpen = true,
            }, 3, 41);
            Assert.IsTrue(recovered.Accepted);

            var causes = log.TraceCauses(recovered.Fact.EventId, recovered.Fact.SaveRevision);

            Assert.AreEqual(1, causes.Count);
            Assert.AreEqual("event_d9_treasure_resolved", causes[0].EventId);
            Assert.AreEqual(WorldEventStatus.Failed, causes[0].OutcomeStatus);
        }
    }
}
