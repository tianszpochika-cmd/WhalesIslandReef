using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;

namespace D12.Tests
{
    public sealed class D12DynamicQuestLogTests
    {
        [Test]
        public void ObjectivesConsumeUnifiedFactsAndOnlyRequestReward()
        {
            var log = new EventLog();
            var system = new D12DynamicQuestLogSystem(log);
            Assert.IsTrue(system.Accept(new D12QuestAcceptRequest
            {
                QuestId = D12DynamicQuestCatalog.ChartRecoveryQuestId,
                ActorRef = "player:captain",
                WorldDay = 1,
                SaveRevision = 1,
            }).Applied);

            var mapFact = Append(log, "event_test_map_discovery", 2, 2, "ExplorationSystem",
                "map_discovery_recorded", new Dictionary<string, string>
                {
                    ["feature_id"] = "wreck_saint_marys_chart_cove",
                });
            var mapResult = system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, mapFact));
            Assert.IsTrue(mapResult.Applied);
            Assert.AreEqual("objective_progressed", mapResult.Code);

            var salvageFact = Append(log, "event_test_salvage_success", 3, 3, "ExplorationSystem",
                "salvage_success", new Dictionary<string, string>
                {
                    ["site_id"] = "wreck_saint_marys_chart_cove",
                });
            var completed = system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, salvageFact));
            Assert.IsTrue(completed.Applied);
            Assert.AreEqual("quest_completed", completed.Code);
            Assert.IsNotNull(completed.RewardRequest);
            Assert.AreEqual("pending", completed.RewardRequest.Status);
            Assert.AreEqual("RewardSystem", completed.RewardRequest.OwnerSystem);
            Assert.IsFalse(completed.RewardRequest.RequestId.Contains("money"));
            Assert.IsTrue(completed.RewardRequest.CauseEventIds.Contains(mapFact.EventId));
            Assert.IsTrue(completed.RewardRequest.CauseEventIds.Contains(salvageFact.EventId));
            Assert.AreEqual(D12DynamicQuestStatus.Completed,
                system.ReadQuest(D12DynamicQuestCatalog.ChartRecoveryQuestId).Status);
            Assert.IsTrue(system.ReadJournal().Count >= 4);
        }

        [Test]
        public void FailureRecoveryAndAlternativeRouteRemainTraceable()
        {
            var log = new EventLog();
            var system = new D12DynamicQuestLogSystem(log);
            Accept(system, D12DynamicQuestCatalog.ChartRecoveryQuestId, 1, 1);

            var mapFact = Append(log, "event_test_alt_map", 2, 2, "ExplorationSystem",
                "map_discovery_recorded", new Dictionary<string, string>
                {
                    ["feature_id"] = "wreck_saint_marys_chart_cove",
                });
            Assert.IsTrue(system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, mapFact)).Applied);

            var failedFact = Append(log, "event_test_salvage_failed", 3, 3, "ExplorationSystem",
                "salvage_risk_failed", new Dictionary<string, string>
                {
                    ["site_id"] = "wreck_saint_marys_chart_cove",
                }, WorldEventStatus.Failed, new RecoveryRoute
                {
                    RouteId = "route_return_to_harbor",
                    Action = "return_to_harbor",
                    ResumeFromEventId = "event_test_salvage_failed",
                    Description = "回港补齐装备后重试。",
                });
            var failed = system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, failedFact));
            Assert.IsTrue(failed.Applied);
            Assert.AreEqual(D12DynamicQuestStatus.Failed, failed.Status);
            Assert.AreEqual(D12DynamicQuestCatalog.ReturnToHarborRouteId, system.ReadQuest(
                D12DynamicQuestCatalog.ChartRecoveryQuestId).RecoveryRouteId);

            var route = system.SelectAlternativeRoute(new D12QuestAlternativeRouteRequest
            {
                QuestId = D12DynamicQuestCatalog.ChartRecoveryQuestId,
                RouteId = D12DynamicQuestCatalog.CommunityChartRouteId,
                ActorRef = "player:captain",
                WorldDay = 4,
                SaveRevision = 4,
            });
            Assert.IsTrue(route.Applied);
            Assert.IsNotNull(route.Fact);
            Assert.IsTrue(route.Fact.CauseIds.Contains(failedFact.EventId));
            Assert.AreEqual(D12DynamicQuestCatalog.CommunityChartRouteId,
                system.ReadQuest(D12DynamicQuestCatalog.ChartRecoveryQuestId).AlternativeRouteId);

            var communityFact = Append(log, "event_test_community_witness", 5, 5, D12CommunityTradeIds.SystemOwner,
                "community_help_offered", new Dictionary<string, string>
                {
                    ["community_id"] = "community_green_cay",
                });
            var completed = system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, communityFact));
            Assert.IsTrue(completed.Applied);
            Assert.AreEqual(D12DynamicQuestStatus.Completed, completed.Status);
            Assert.IsTrue(completed.RewardRequest.CauseEventIds.Contains(route.Fact.EventId));
        }

        [Test]
        public void AgentSummaryAndJournalOnlyExposeEventLogFacts()
        {
            var log = new EventLog();
            var system = new D12DynamicQuestLogSystem(log);
            Accept(system, D12DynamicQuestCatalog.CommunityReciprocityQuestId, 1, 1);
            var summary = system.BuildAgentSummary(10);
            Assert.IsTrue(summary.FactsOnly);
            Assert.IsFalse(summary.CanCreateFacts);
            Assert.IsTrue(summary.SummaryText.Contains("event_d12_quest_accepted_"));
            Assert.IsTrue(summary.FactRefs.Count > 0);

            var missing = system.ObserveFact(new D12QuestFactObservationRequest
            {
                QuestId = D12DynamicQuestCatalog.CommunityReciprocityQuestId,
                FactEventId = "event_not_in_log",
                FactSaveRevision = 2,
                ActorRef = "player:captain",
            });
            Assert.IsFalse(missing.Accepted);
            Assert.AreEqual("fact_missing", missing.Code);
            Assert.AreEqual(1, system.ReadJournal().Count);
        }

        [Test]
        public void SaveRestorePreservesQuestJournalAndDuplicateProtection()
        {
            var log = new EventLog();
            var system = new D12DynamicQuestLogSystem(log);
            Accept(system, D12DynamicQuestCatalog.ChartRecoveryQuestId, 1, 1);
            var mapFact = Append(log, "event_test_restore_map", 2, 2, "ExplorationSystem",
                "map_discovery_recorded", new Dictionary<string, string>
                {
                    ["feature_id"] = "wreck_saint_marys_chart_cove",
                });
            system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, mapFact));
            var salvageFact = Append(log, "event_test_restore_salvage", 3, 3, "ExplorationSystem",
                "salvage_success", new Dictionary<string, string>
                {
                    ["site_id"] = "wreck_saint_marys_chart_cove",
                });
            var completed = system.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, salvageFact));
            Assert.IsTrue(completed.Applied);

            var snapshot = system.Snapshot;
            var runtime = new D12RuntimeSnapshot { DynamicQuestLog = snapshot };
            runtime.Validate();
            var restoredLog = EventLog.FromSnapshot(log.CaptureSnapshot());
            var restored = new D12DynamicQuestLogSystem(restoredLog,
                D12DynamicQuestCatalog.CreateVerticalSlice(), snapshot);
            Assert.AreEqual(D12DynamicQuestStatus.Completed,
                restored.ReadQuest(D12DynamicQuestCatalog.ChartRecoveryQuestId).Status);
            Assert.AreEqual(system.ReadJournal().Count, restored.ReadJournal().Count);

            int factCount = restoredLog.Count;
            var duplicate = restored.ObserveFact(Observe(D12DynamicQuestCatalog.ChartRecoveryQuestId, salvageFact));
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(factCount, restoredLog.Count);
            Assert.AreEqual("pending", restored.ReadQuest(D12DynamicQuestCatalog.ChartRecoveryQuestId).RewardRequest.Status);
        }

        private static void Accept(D12DynamicQuestLogSystem system, string questId, int day, long revision)
        {
            Assert.IsTrue(system.Accept(new D12QuestAcceptRequest
            {
                QuestId = questId,
                ActorRef = "player:captain",
                WorldDay = day,
                SaveRevision = revision,
            }).Applied);
        }

        private static D12QuestFactObservationRequest Observe(string questId, FactEvent fact)
        {
            return new D12QuestFactObservationRequest
            {
                QuestId = questId,
                FactEventId = fact.EventId,
                FactSaveRevision = fact.SaveRevision,
                ActorRef = "player:captain",
            };
        }

        private static FactEvent Append(EventLog log, string eventId, int day, long revision,
            string owner, string resultCode, Dictionary<string, string> values,
            WorldEventStatus status = WorldEventStatus.Normal, RecoveryRoute route = null)
        {
            return log.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = day,
                LocationRef = "test_location",
                OwnerSystem = owner,
                ActorIds = new List<string> { "player:captain" },
                InputFacts = new List<string> { "test_input:" + eventId },
                CauseIds = new List<string>(),
                SaveRevision = revision,
                OutcomeStatus = status,
                RecoveryRoute = route,
                Result = new EventResult
                {
                    ResultCode = resultCode,
                    Summary = resultCode,
                    FactIds = new List<string> { "test:" + eventId },
                    Values = values,
                },
            }).Fact;
        }
    }
}
