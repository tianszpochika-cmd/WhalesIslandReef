using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    public sealed class D10RelationModelTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_a01_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void AllNineRelationTypesAreFactBoundAndTraceable()
        {
            var log = new EventLog();
            log.Append(Fact("event_d10_rescue", 1, 1, new string[0], "救援事实已记录。"));
            var fact = log.Append(Fact("event_d10_relation_matrix", 2, 2,
                new[] { "event_d10_rescue" }, "关系变化事实已记录。")).Fact;
            var system = new RelationSystem(log, new[] { "player", "crew_a", "crew_b" });

            var types = (RelationType[])Enum.GetValues(typeof(RelationType));
            foreach (var type in types)
            {
                var source = type == RelationType.Jealousy ? "crew_b" : "player";
                var target = type == RelationType.Jealousy ? "crew_a" : "crew_a";
                var result = system.ApplyFactEvent(fact,
                    new RelationChangeIntent(source, target, type, type == RelationType.Debt ? 12f : 4f, "事实后的关系意图"));
                Assert.IsTrue(result.Succeeded, type.ToString());
                Assert.IsTrue(result.Edge.CauseEventIds.Contains("event_d10_relation_matrix"), type.ToString());
                Assert.IsTrue(result.Edge.CauseEventIds.Contains("event_d10_rescue"), type.ToString());
            }

            Assert.AreEqual(9, system.ReadEdges().Count);
            Assert.AreEqual(1, log.TraceCauses("event_d10_relation_matrix", 2).Count);
        }

        [Test]
        public void DuplicateAfterSnapshotRestoreDoesNotReplay()
        {
            var log = new EventLog();
            var fact = log.Append(Fact("event_d10_trust_change", 3, 3, new string[0], "守约事实已记录。")).Fact;
            var system = new RelationSystem(log, new[] { "player", "crew_a" });
            var intent = new RelationChangeIntent("player", "crew_a", RelationType.Trust, 8f);
            var first = system.ApplyFactEvent(fact, intent);
            Assert.AreEqual(RelationApplyStatus.Applied, first.Status);

            var restored = new RelationSystem(EventLog.FromSnapshot(log.CaptureSnapshot()), new[] { "player", "crew_a" });
            restored.Restore(system.CaptureSnapshot());
            var duplicate = restored.ApplyFactEvent(fact, intent);
            Assert.IsTrue(duplicate.IsDuplicate);
            Assert.AreEqual(first.After, restored.ReadEdge("player", "crew_a", RelationType.Trust).Value, 0.0001f);
        }

        [Test]
        public void UnknownCharactersMissingFactsAndEventVersionAreExplicit()
        {
            var log = new EventLog();
            var system = new RelationSystem(log, new[] { "player", "crew_a" });
            var notLogged = new EventLog().Append(Fact("event_not_logged", 1, 1, new string[0], "未进入日志")).Fact;
            Assert.AreEqual(RelationApplyStatus.MissingFactEvent,
                system.ApplyFactEvent(notLogged, new RelationChangeIntent("player", "crew_a", RelationType.Affinity, 1)).Status);

            var fact = log.Append(Fact("event_d10_unknown_target", 2, 2, new string[0], "目标缺失")).Fact;
            var unknown = system.ApplyFactEvent(fact,
                new RelationChangeIntent("player", "crew_missing", RelationType.Affinity, 1));
            Assert.AreEqual(RelationApplyStatus.UnknownTarget, unknown.Status);

            fact.SchemaVersion = EventSchema.CurrentVersion + 1;
            var mismatch = system.ApplyFactEvent(fact,
                new RelationChangeIntent("player", "crew_a", RelationType.Affinity, 1));
            Assert.AreEqual(RelationApplyStatus.VersionMismatch, mismatch.Status);
            StringAssert.Contains("schema_version", mismatch.Message);
        }

        [Test]
        public void PanelExplainsCauseAndReportsMissingCauseAndUnknownCharacter()
        {
            var log = new EventLog();
            var fact = log.Append(Fact("event_d10_panel_change", 4, 4,
                new[] { "event_d10_missing_cause" }, "救援后信任上升。")).Fact;
            var system = new RelationSystem(log, new[] { "player", "crew_a" });
            Assert.IsTrue(system.ApplyFactEvent(fact,
                new RelationChangeIntent("player", "crew_a", RelationType.Trust, 5f)).Succeeded);

            var panel = D10RelationPanelReadModel.Build(system, log, "player", "crew_a");
            Assert.IsTrue(panel.IsAvailable);
            Assert.AreEqual(1, panel.Rows.Count);
            StringAssert.Contains("event_d10_panel_change", panel.Rows[0].Explanation);
            StringAssert.Contains("event_d10_missing_cause", panel.Rows[0].Explanation);
            Assert.IsFalse(panel.Rows[0].Causes[1].Found);

            var unknown = D10RelationPanelReadModel.Build(system, log, "crew_missing", "crew_a");
            Assert.AreEqual("unknown_character", unknown.Status);
            StringAssert.Contains("未知角色", unknown.Message);
        }

        [Test]
        public void CanonicalRelationGraphSurvivesSaveAndLoad()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "关系测试船长" });
            var log = new EventLog();
            var fact = log.Append(Fact("event_d10_save_relation", 5, 5, new string[0], "关系保存事实。")).Fact;
            var system = new RelationSystem(log, new[] { "player", "crew_a" });
            Assert.IsTrue(system.ApplyFactEvent(fact,
                new RelationChangeIntent("player", "crew_a", RelationType.Attachment, 7f)).Succeeded);

            SaveSystem.SaveD10RelationGraph(system.CaptureSnapshot());
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD10RelationGraph();
            Assert.IsNotNull(restored);
            Assert.AreEqual(7f, restored.Edges[0].Value, 0.0001f);
            CollectionAssert.Contains(restored.Edges[0].CauseEventIds, "event_d10_save_relation");
            CollectionAssert.Contains(restored.Edges[0].AppliedEventKeys, fact.DedupeKey);
        }

        private static WorldEvent Fact(string id, int day, long revision, IEnumerable<string> causes, string summary)
        {
            return new WorldEvent
            {
                EventId = id,
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = day,
                LocationRef = "nearshore_saint_marys",
                ActorIds = new List<string> { "player", "crew_a", "crew_b" },
                InputFacts = new List<string> { "d10_test_input" },
                OwnerSystem = "CrewSystem",
                Result = new EventResult
                {
                    ResultCode = "d10_relation_fact",
                    Summary = summary,
                    FactIds = new List<string> { "player", "crew_a", "crew_b" },
                },
                CauseIds = causes == null ? new List<string>() : new List<string>(causes),
                SaveRevision = revision,
            };
        }
    }
}
