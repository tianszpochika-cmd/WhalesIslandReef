using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.Events;

namespace Save.Tests
{
    /// <summary>D8-A03：统一 WorldEvent/FactEvent 的事实、因果、版本和幂等门测试。</summary>
    public class D8EventPackageTests
    {
        [Test]
        public void WorldEvent_ContainsFrozenFields_AndReadersSeeLoggedFact()
        {
            var log = new EventLog();
            var worldEvent = NewEvent("event_fishing_001", WorldEventStatus.Normal, 12, 31);
            var appended = log.Append(worldEvent);

            Assert.IsTrue(appended.Succeeded);
            Assert.IsFalse(appended.IsDuplicate);
            Assert.AreEqual("event_fishing_001", appended.Fact.EventId);
            Assert.AreEqual(1, appended.Fact.SchemaVersion);
            Assert.AreEqual(12, appended.Fact.WorldDay);
            Assert.AreEqual("reef_014", appended.Fact.LocationRef);
            CollectionAssert.AreEqual(new[] { "player_001", "crew_003" }, appended.Fact.ActorIds);
            CollectionAssert.AreEqual(new[] { "fish_school_active", "gear_net_01" }, appended.Fact.InputFacts);
            Assert.AreEqual("EcologySystem", appended.Fact.OwnerSystem);
            Assert.AreEqual("catch_created", appended.Fact.Result.ResultCode);
            Assert.AreEqual(31, appended.Fact.SaveRevision);

            Assert.AreEqual(1, log.ReadForQuest(new[] { "event_fishing_001" }).Count);
            var uiFact = log.ReadFactsForUi()[0];
            uiFact.Result.ResultCode = "ui_mutation_attempt";
            Assert.AreEqual("catch_created", log.Find(worldEvent.EventId, worldEvent.SaveRevision).Result.ResultCode);
            StringAssert.Contains("event_fishing_001", log.BuildAgentSummary());
        }

        [Test]
        public void FailedEvent_RequiresRecoveryRoute()
        {
            var log = new EventLog();
            var failed = NewEvent("event_storm_failed", WorldEventStatus.Failed, 8, 40);
            failed.RecoveryRoute = null;
            Assert.Throws<EventValidationException>(() => log.Append(failed));

            failed.RecoveryRoute = new RecoveryRoute
            {
                RouteId = "route_repair_at_port",
                Action = "repair_and_resume",
                Description = "回港维修后恢复航程。",
            };
            var result = log.Append(failed);
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(WorldEventStatus.Failed, result.Fact.OutcomeStatus);
            Assert.AreEqual("route_repair_at_port", result.Fact.RecoveryRoute.RouteId);
        }

        [Test]
        public void CancelledEvent_IsARecordedOutcomeWithoutRecoveryRoute()
        {
            var log = new EventLog();
            var cancelled = NewEvent("event_cancelled_001", WorldEventStatus.Cancelled, 9, 41);
            var result = log.Append(cancelled);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(WorldEventStatus.Cancelled, result.Fact.OutcomeStatus);
            Assert.IsNull(result.Fact.RecoveryRoute);
        }

        [Test]
        public void InterruptedEvent_CanBeRecovered_AndCauseChainIsTraceable()
        {
            var log = new EventLog();
            var root = NewEvent("event_route_started", WorldEventStatus.Normal, 1, 1);
            var interrupted = NewEvent("event_route_interrupted", WorldEventStatus.Interrupted, 2, 2);
            interrupted.CauseIds.Add(root.EventId);
            interrupted.RecoveryRoute = new RecoveryRoute
            {
                RouteId = "route_resume_after_fog",
                Action = "resume",
                ResumeFromEventId = interrupted.EventId,
            };
            log.Append(root);
            log.Append(interrupted);

            var recovered = WorldEvent.CreateRecovery(
                interrupted,
                "event_route_recovered",
                3,
                3,
                "lighthouse_01",
                "ShipSystem",
                new EventResult { ResultCode = "voyage_resumed", Summary = "航程从灯塔路线恢复。" },
                new[] { "player_001" });
            var recoveryResult = log.AppendRecovery(interrupted.EventId, interrupted.SaveRevision, recovered);

            Assert.IsTrue(recoveryResult.Succeeded);
            Assert.AreEqual(WorldEventStatus.Recovered, recoveryResult.Fact.OutcomeStatus);
            CollectionAssert.Contains(recoveryResult.Fact.CauseIds, interrupted.EventId);

            var chain = log.TraceCauses(recovered.EventId, recovered.SaveRevision);
            Assert.AreEqual(2, chain.Count);
            Assert.AreEqual(root.EventId, chain[0].EventId);
            Assert.AreEqual(interrupted.EventId, chain[1].EventId);
        }

        [Test]
        public void UnsupportedEventVersion_ReturnsExplicitError()
        {
            var log = new EventLog();
            var invalid = NewEvent("event_future_schema", WorldEventStatus.Normal, 1, 1);
            invalid.SchemaVersion = 99;

            var error = Assert.Throws<EventSchemaVersionException>(() => log.Append(invalid));
            StringAssert.Contains("schema_version mismatch", error.Message);
            StringAssert.Contains("expected 1", error.Message);
        }

        [Test]
        public void DuplicateSettlement_IsProtectedAfterSnapshotRestore()
        {
            var log = new EventLog();
            var worldEvent = NewEvent("event_reward_001", WorldEventStatus.Normal, 7, 88);
            var firstAppend = log.Append(worldEvent);
            var firstSettlement = log.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
            Assert.IsTrue(firstSettlement.CanApplyOwnerSettlement);

            // 模拟 SaveSystem 读档：只恢复事件日志快照和已声明结算键。
            var restored = EventLog.FromSnapshot(log.CaptureSnapshot());
            var duplicateAppend = restored.Append(worldEvent);
            var duplicateSettlement = restored.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);

            Assert.IsTrue(duplicateAppend.Succeeded);
            Assert.IsTrue(duplicateAppend.IsDuplicate);
            Assert.AreEqual(firstAppend.Fact.DedupeKey, duplicateAppend.Fact.DedupeKey);
            Assert.IsTrue(duplicateSettlement.IsDuplicate);
            Assert.IsFalse(duplicateSettlement.CanApplyOwnerSettlement);
            Assert.AreEqual(1, restored.Count);
        }

        [Test]
        public void SameEventIdWithDifferentSaveRevision_IsDistinctKey()
        {
            var log = new EventLog();
            var first = NewEvent("event_weather_001", WorldEventStatus.Normal, 4, 10);
            var second = NewEvent("event_weather_001", WorldEventStatus.Normal, 5, 11);
            Assert.IsFalse(log.Append(first).IsDuplicate);
            Assert.IsFalse(log.Append(second).IsDuplicate);
            Assert.AreEqual(2, log.Count);
        }

        private static WorldEvent NewEvent(string id, WorldEventStatus status, int day, long revision)
        {
            var e = new WorldEvent
            {
                EventId = id,
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = day,
                LocationRef = "reef_014",
                ActorIds = new List<string> { "player_001", "crew_003" },
                InputFacts = new List<string> { "fish_school_active", "gear_net_01" },
                OwnerSystem = "EcologySystem",
                Result = new EventResult
                {
                    ResultCode = "catch_created",
                    Summary = "捕捞事实已记录。",
                    FactIds = new List<string> { "catch_042" },
                },
                CauseIds = new List<string>(),
                SaveRevision = revision,
                OutcomeStatus = status,
            };
            if (status == WorldEventStatus.Failed || status == WorldEventStatus.Interrupted)
                e.RecoveryRoute = new RecoveryRoute
                {
                    RouteId = "route_default_recovery",
                    Action = "recover",
                };
            return e;
        }
    }
}
