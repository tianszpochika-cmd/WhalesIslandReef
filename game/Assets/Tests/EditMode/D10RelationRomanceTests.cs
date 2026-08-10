using System;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D10;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    public class D10RelationRomanceTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_relation_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string file = _dbPath + suffix;
                if (File.Exists(file))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        [Test]
        public void PlayerCrewStagesRequireFactsAndPrerequisites()
        {
            var log = new EventLog();
            var relations = new D10RelationSystem(log);
            var edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.Gift, 1)).Relation;
            Assert.AreEqual(D10RelationPhase.Acquaintance, edge.Phase);

            edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.Rescue, 2)).Relation;
            Assert.AreEqual(D10RelationPhase.Acquaintance, edge.Phase);

            for (long revision = 3; revision <= 5; revision++)
                edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.PromiseKept, revision)).Relation;
            Assert.AreEqual(D10RelationPhase.Trust, edge.Phase);

            for (long revision = 6; revision <= 10; revision++)
                edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.NightTalk, revision)).Relation;
            Assert.AreEqual(D10RelationPhase.Intimacy, edge.Phase);

            var premature = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.RomanceAccept, 11));
            Assert.IsTrue(premature.Succeeded);
            Assert.AreEqual(D10RomanceConsent.Accepted, premature.Relation.RomanceConsent);
            Assert.AreNotEqual(D10RelationPhase.Commitment, premature.Relation.Phase);

            edge = premature.Relation;
            for (long revision = 12; revision <= 16; revision++)
                edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.PromiseKept, revision)).Relation;
            edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.NightTalk, 17)).Relation;
            edge = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.PromiseKept, 18)).Relation;

            Assert.AreEqual(D10RelationPhase.Commitment, edge.Phase);
            Assert.GreaterOrEqual(edge.Trust, 60);
            Assert.GreaterOrEqual(edge.Attachment, 55);
            Assert.GreaterOrEqual(edge.Commitment, 60);
            Assert.IsTrue(edge.CauseEventIds.Contains("event_d10_relation_gift"));
        }

        [Test]
        public void ConflictCanBecomeStableThenSeparate()
        {
            var relations = new D10RelationSystem(new EventLog());
            var edge = relations.Apply(Action("crew_isaac_reed", "crew_second", D10RelationAction.Rescue, 1)).Relation;
            for (long revision = 2; revision <= 5; revision++)
                edge = relations.Apply(Action("crew_isaac_reed", "crew_second", D10RelationAction.PromiseKept, revision)).Relation;

            edge = relations.Apply(Action("crew_isaac_reed", "crew_second", D10RelationAction.Argument, 6)).Relation;
            Assert.AreEqual(D10RelationPhase.Conflict, edge.Phase);
            Assert.IsTrue(edge.ConflictActive);

            edge = relations.Apply(Action("crew_isaac_reed", "crew_second", D10RelationAction.ResolveConflict, 7)).Relation;
            Assert.AreEqual(D10RelationPhase.Stable, edge.Phase);
            Assert.IsFalse(edge.ConflictActive);

            edge = relations.Apply(Action("crew_isaac_reed", "crew_second", D10RelationAction.Separate, 8)).Relation;
            Assert.AreEqual(D10RelationPhase.Separation, edge.Phase);
            Assert.AreEqual(D10RomanceConsent.Separated, edge.RomanceConsent);
        }

        [Test]
        public void RomanceDeclineBlocksForcedReentry()
        {
            var relations = new D10RelationSystem(new EventLog());
            BuildIntimacy(relations, "player_captain", "crew_isaac_reed", 1);

            var declined = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.RomanceDecline, 20));
            Assert.IsTrue(declined.Applied);
            Assert.AreEqual(D10RomanceConsent.Declined, declined.Relation.RomanceConsent);
            var countAfterDecline = relations.EventLog.Count;

            var forced = relations.Apply(Action("player_captain", "crew_isaac_reed", D10RelationAction.RomanceAccept, 21));
            Assert.IsFalse(forced.Succeeded);
            Assert.AreEqual("romance_route_declined", forced.Code);
            Assert.AreEqual(countAfterDecline, relations.EventLog.Count);
            Assert.AreEqual(D10RomanceConsent.Declined,
                relations.FindRelation("player_captain", "crew_isaac_reed").RomanceConsent);
        }

        [Test]
        public void FavoritismCreatesTraceableCrewCrewJealousy()
        {
            var log = new EventLog();
            var relations = new D10RelationSystem(log);
            var result = relations.Apply(new D10RelationActionRequest
            {
                SourceId = "player_captain",
                TargetId = "crew_favored",
                Action = D10RelationAction.PublicFavoritism,
                WorldDay = 4,
                SaveRevision = 4,
                AffectedSourceIds = { "crew_jealous" },
            });

            Assert.IsTrue(result.Applied);
            Assert.AreEqual(2, result.ChangedRelationIds.Count);
            var jealousy = relations.FindRelation("crew_jealous", "crew_favored");
            Assert.IsNotNull(jealousy);
            Assert.AreEqual(D10RelationPhase.Conflict, jealousy.Phase);
            Assert.Greater(jealousy.Jealousy, 0);
            Assert.IsTrue(log.Facts[0].Result.Values["changed_relation_ids"].Contains(jealousy.Id));
            Assert.AreEqual("CrewSystem", log.Facts[0].OwnerSystem);

            var crewCrew = relations.Apply(Action("crew_jealous", "crew_favored", D10RelationAction.NightTalk, 5));
            Assert.IsTrue(crewCrew.Applied);
            Assert.AreEqual("crew_jealous", crewCrew.Relation.SourceId);
            Assert.AreEqual("crew_favored", crewCrew.Relation.TargetId);
        }

        [Test]
        public void DuplicateAndSaveRestoreDoNotReplayRelationEffects()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "关系测试船长" });
            var log = new EventLog();
            var relations = new D10RelationSystem(log);
            var request = Action("player_captain", "crew_isaac_reed", D10RelationAction.Rescue, 30);
            var first = relations.Apply(request);
            var second = relations.Apply(request.Clone());

            Assert.IsTrue(first.Applied);
            Assert.IsTrue(second.IsDuplicate);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(first.Relation.Trust, relations.Snapshot.Find("player_captain", "crew_isaac_reed").Trust);

            var d3 = new D3RuntimeSnapshot { EventLog = log.CaptureSnapshot(), SaveRevision = 30 };
            SaveSystem.SaveD3Runtime(d3);
            SaveSystem.SaveD10Runtime(relations.Snapshot);

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD10Runtime();
            Assert.IsNotNull(restored);
            Assert.AreEqual(first.Relation.Trust, restored.Find("player_captain", "crew_isaac_reed").Trust);
            Assert.AreEqual("event_d10_rescue", restored.LastEventId);

            var restoredRelations = new D10RelationSystem(SaveSystem.RestoreD3TradeEventLog(), restored);
            var afterRestore = restoredRelations.Apply(request.Clone());
            Assert.IsTrue(afterRestore.IsDuplicate);
            Assert.AreEqual(first.Relation.Trust,
                restoredRelations.FindRelation("player_captain", "crew_isaac_reed").Trust);
        }

        [Test]
        public void UnknownActionAndCorruptDedupeSnapshotAreRejected()
        {
            var log = new EventLog();
            var relations = new D10RelationSystem(log);
            var unknown = relations.Apply(new D10RelationActionRequest
            {
                SourceId = "player_captain",
                TargetId = "crew_isaac_reed",
                Action = (D10RelationAction)999,
                WorldDay = 1,
                SaveRevision = 1,
            });

            Assert.IsFalse(unknown.Succeeded);
            Assert.AreEqual("unknown_action", unknown.Code);
            Assert.AreEqual(0, log.Count);

            var corrupted = new D10RelationRuntimeSnapshot
            {
                AppliedEventKeys = { "event_d10_relation_gift\u001f1", "event_d10_relation_gift\u001f1" },
            };
            Assert.Throws<InvalidOperationException>(() => corrupted.Validate());
        }

        [Test]
        public void CanonicalGraphRefreshUpdatesOnlyStageProjection()
        {
            var canonical = new RelationGraphSnapshot();
            canonical.Edges.Add(new RelationEdge
            {
                SourceId = "player_captain",
                TargetId = "crew_isaac_reed",
                RelationType = RelationType.Affinity,
                Value = 30f,
                LastChangedDay = 5,
                CauseEventIds = { "event_d10_relation_rescue" },
            });
            canonical.Edges.Add(new RelationEdge
            {
                SourceId = "player_captain",
                TargetId = "crew_isaac_reed",
                RelationType = RelationType.Trust,
                Value = 50f,
                LastChangedDay = 5,
                CauseEventIds = { "event_d10_relation_rescue" },
            });
            canonical.Edges.Add(new RelationEdge
            {
                SourceId = "player_captain",
                TargetId = "crew_isaac_reed",
                RelationType = RelationType.Attachment,
                Value = 45f,
                LastChangedDay = 5,
                CauseEventIds = { "event_d10_relation_rescue" },
            });
            canonical.Validate();

            var relations = new D10RelationSystem(new EventLog());
            relations.RefreshFromCanonicalGraph(canonical);
            var projection = relations.FindRelation("player_captain", "crew_isaac_reed");
            Assert.AreEqual(30, projection.Affinity);
            Assert.AreEqual(50, projection.Trust);
            Assert.AreEqual(45, projection.Attachment);
            Assert.AreEqual(D10RelationPhase.Intimacy, projection.Phase);
            Assert.AreEqual(30f, relations.Snapshot.CanonicalGraph.Edges[0].Value);
        }

        private static void BuildIntimacy(D10RelationSystem relations, string sourceId, string targetId, long startRevision)
        {
            relations.Apply(Action(sourceId, targetId, D10RelationAction.Rescue, startRevision));
            long revision = startRevision + 1;
            for (int i = 0; i < 4; i++)
                relations.Apply(Action(sourceId, targetId, D10RelationAction.PromiseKept, revision++));
            for (int i = 0; i < 5; i++)
                relations.Apply(Action(sourceId, targetId, D10RelationAction.NightTalk, revision++));
            Assert.AreEqual(D10RelationPhase.Intimacy, relations.FindRelation(sourceId, targetId).Phase);
        }

        private static D10RelationActionRequest Action(string sourceId, string targetId,
            D10RelationAction action, long saveRevision)
        {
            return new D10RelationActionRequest
            {
                SourceId = sourceId,
                TargetId = targetId,
                Action = action,
                WorldDay = (int)Math.Max(1, saveRevision),
                SaveRevision = saveRevision,
            };
        }
    }
}
