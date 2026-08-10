using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D10;
using Whalebone.Systems.D3;
using Whalebone.Systems.D5;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    /// <summary>
    /// D10-S01 的同步验收。测试只编排既有 owner，不能直接写关系、行为、身份或外观字段。
    /// </summary>
    public sealed class D10S01AcceptanceIntegrationTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d10_s01_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            GameState.Reset();
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
        public void FullAcceptance_WageArrearsRescueFavoritismBehaviorIdentityAndSaveLoad()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D10-S01 验收船长" });

            var playerId = GameState.CurrentPlayer.Id;
            var oldSailor = GameState.CrewAgents[0];
            var isaacSlice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var isaac = isaacSlice.Agent;
            var eventLog = new EventLog();
            var economy = new TradeEconomySystem(eventLog);
            var trade = new D3TradeState(500, 100);
            var ship = GameState.CurrentShip;
            var runtime = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Money = trade.Money,
                Hull = ship.Hull,
                HullMax = ship.HullMax,
            };
            var preparation = new D9PortShipPreparationSystem(economy, eventLog);
            Assert.IsTrue(preparation.EnterPort(runtime, ship, PortCatalog.StartingPortId).Accepted);

            // 1. 工资失败分支必须可见，且只能由 D9 工资 owner 修改士气/忠诚；不把失败伪装成已付款。
            Assert.IsTrue(preparation.Recruit(trade, ship, PortCatalog.StartingPortId,
                GameState.CrewAgents, isaac).Accepted);
            float loyaltyBeforeArrears = isaac.Stats.Loyalty;
            var arrears = preparation.SettleWages(trade, ship, PortCatalog.StartingPortId,
                GameState.CrewAgents, 30, 1, 100);
            Assert.IsFalse(arrears.Accepted);
            Assert.AreEqual("not_enough_money", arrears.Code);
            Assert.Less(isaac.Stats.Loyalty, loyaltyBeforeArrears);
            Assert.IsNull(arrears.Fact, "拖欠工资不能被错误记录成 wages_paid 事实。");
            Assert.IsFalse(HasFact(eventLog, D9PortShipPreparationSystem.WageSettledEventId));

            // 船员报告是现有 CrewSystem 事实；它承接工资失败后的可观察状态，供行为状态机引用。
            var crewReports = new D9CrewEventSystem(eventLog);
            var report = crewReports.Discuss(isaacSlice, "拖欠工资后的船员报告：请给出岗位和航线建议。", 1, 101);
            Assert.IsTrue(report.Succeeded, report.Message);
            Assert.IsNotNull(report.Fact);

            // 2. 行为变化只能由 CrewSystem 状态机裁决；轮班推进达到边界后进入怠工。
            var behavior = new D10CrewBehaviorSystem(eventLog);
            Assert.IsTrue(behavior.EnsureState(isaac, 1, 102,
                new[] { report.Fact.EventId }).Accepted);
            Assert.IsTrue(behavior.AssignShift(isaac, CrewShift.DayWatch, 1, 103,
                new[] { report.Fact.EventId }).Accepted);
            var behaviorTick = behavior.Tick(isaac, CrewShift.DayWatch, 24, 1, 104,
                new[] { report.Fact.EventId });
            Assert.IsTrue(behaviorTick.Accepted, behaviorTick.Message);
            Assert.AreEqual(CrewBehaviorStatus.Slacking, isaac.BehaviorState.Status);
            Assert.AreEqual(D10CrewBehaviorEventType.Slacking, behaviorTick.Event.Type);
            CollectionAssert.Contains(behaviorTick.Event.CauseEventIds, report.Fact.EventId);

            // 3. 救援与公开偏袒走 D10 A03 关系 owner；关系动作仍带有事实因果。
            var relations = new D10RelationSystem(eventLog);
            var rescue = relations.Apply(new D10RelationActionRequest
            {
                SourceId = playerId,
                TargetId = isaac.Id,
                Action = D10RelationAction.Rescue,
                WorldDay = 2,
                SaveRevision = 200,
                ContextRef = "d10_s01_rescue",
                CauseEventIds = { report.Fact.EventId },
            });
            Assert.IsTrue(rescue.Applied, rescue.Message);
            Assert.IsNotNull(rescue.Fact);
            CollectionAssert.Contains(rescue.Fact.CauseIds, report.Fact.EventId);

            var favoritism = relations.Apply(new D10RelationActionRequest
            {
                SourceId = playerId,
                TargetId = isaac.Id,
                Action = D10RelationAction.PublicFavoritism,
                WorldDay = 2,
                SaveRevision = 201,
                ContextRef = "d10_s01_public_favoritism",
                AffectedSourceIds = { oldSailor.Id },
                CauseEventIds = { rescue.Fact.EventId },
            });
            Assert.IsTrue(favoritism.Applied, favoritism.Message);
            Assert.IsNotNull(relations.FindRelation(oldSailor.Id, isaac.Id));

            // 4. A01 canonical relation owner先裁决，再让A03只读刷新阶段投影；面板只读取原因。
            var canonical = new RelationSystem(eventLog, new[] { playerId, isaac.Id, oldSailor.Id });
            Assert.IsTrue(canonical.ApplyFactEvent(rescue.Fact,
                new RelationChangeIntent(playerId, isaac.Id, RelationType.Trust, 14f,
                    "救援事实后的信任提议")).Succeeded);
            Assert.IsTrue(canonical.ApplyFactEvent(favoritism.Fact,
                new RelationChangeIntent(playerId, isaac.Id, RelationType.Affinity, 4f,
                    "公开偏袒事实后的关系提议")).Succeeded);
            relations.RefreshFromCanonicalGraph(canonical.CaptureSnapshot());

            var panel = D10RelationPanelReadModel.Build(canonical, eventLog, playerId, isaac.Id);
            Assert.IsTrue(panel.IsAvailable, panel.Message);
            Assert.AreEqual(2, panel.Rows.Count, "Trust 与 Affinity 是 A01 的两条 canonical 关系边。");
            Assert.IsTrue(HasFoundCause(panel, "event_d10_rescue"));
            Assert.IsTrue(HasFoundCause(panel, "event_d10_public_favoritism"));

            // 5. 身份修改只在港口通过 Identity owner，并委托外观 owner 装备服装。
            var appearance = new D10AppearanceSystem(null, economy, trade, delegate { return true; }, 2,
                PortCatalog.StartingPortId);
            var purchase = appearance.Purchase("outfit_missing_tailor_placeholder");
            Assert.IsTrue(purchase.Success, purchase.Message);
            var identity = new D10PlayerIdentitySystem(GameState.CurrentPlayer.Identity,
                new D10AppearanceOwnerWriter(appearance));
            var beforeInspection = identity.InspectAtPort(true, PortCatalog.StartingPortId);
            Assert.IsTrue(beforeInspection.CanEditTitle);
            Assert.IsTrue(beforeInspection.CanEditAppearance);
            var identityUpdate = identity.ApplyPortUpdate(new D10PortIdentityUpdateRequest
            {
                NewTitle = new D10LocalizedText("title.harbor_witness", "港口见证人", "Harbor Witness"),
                AppearanceLoadoutId = "outfit_missing_tailor_placeholder",
                Document = new D10IdentityDocumentRecord
                {
                    DocumentTypeKey = "document.harbor_inspection",
                    IssuerRef = "port:" + PortCatalog.StartingPortId,
                },
                ChangeReasonKey = "d10_s01_port_inspection",
                WorldDay = 2,
            }, true, PortCatalog.StartingPortId,
            new D10IdentityHistoryPreservationContext
            {
                DebtHistoryRefs = { "debt:d10_s01_wage_arrears" },
                RelationHistoryRefs = { "relation:" + playerId + "->" + isaac.Id },
            });
            Assert.IsTrue(identityUpdate.Succeeded, identityUpdate.ErrorCode);
            Assert.AreEqual("title.harbor_witness", identityUpdate.Identity.Title.Key);
            Assert.AreEqual("outfit_missing_tailor_placeholder",
                appearance.Snapshot.ActiveLoadout.Get(D10AppearanceSlot.Accessory));
            var afterInspection = identity.InspectAtPort(true, PortCatalog.StartingPortId);
            Assert.AreEqual("title.harbor_witness", afterInspection.Identity.Title.Key);
            StringAssert.Contains("outfit_missing_tailor_placeholder", afterInspection.CurrentAppearanceLoadoutId);

            // 6. SaveSystem只复制各 owner 快照；D3 保存同一只 EventLog，关系图和外观分别落在既有入口。
            SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Money = trade.Money,
                Hull = ship.Hull,
                HullMax = ship.HullMax,
                SaveRevision = 300,
                EventLog = eventLog.CaptureSnapshot(),
            });
            SaveSystem.SaveD10Runtime(relations.Snapshot);
            SaveSystem.SaveD10RelationGraph(canonical.CaptureSnapshot());
            SaveSystem.SaveD5Runtime(new D5RuntimeSnapshot { Appearance = appearance.Snapshot });
            SaveSystem.SaveD10Identity(identity.Snapshot);

            var relationBeforeLoad = relations.FindRelation(playerId, isaac.Id);
            var behaviorSequenceBeforeLoad = isaac.BehaviorState.TransitionSequence;
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());

            var restoredD3 = SaveSystem.LoadD3Runtime();
            var restoredLog = SaveSystem.RestoreD3TradeEventLog();
            var restoredD10 = SaveSystem.LoadD10Runtime();
            var restoredGraph = SaveSystem.LoadD10RelationGraph();
            var restoredD5 = SaveSystem.LoadD5Runtime();
            var restoredIdentity = SaveSystem.LoadD10Identity();
            Assert.IsNotNull(restoredD3);
            Assert.IsNotNull(restoredD10);
            Assert.IsNotNull(restoredGraph);
            Assert.IsNotNull(restoredD5);
            Assert.IsNotNull(restoredIdentity);
            Assert.AreEqual(eventLog.Count, restoredLog.Count);
            Assert.IsNotNull(restoredLog.Find("event_d10_rescue", 200));
            Assert.IsNotNull(restoredLog.Find("event_d10_public_favoritism", 201));
            Assert.AreEqual(relationBeforeLoad.Trust,
                restoredD10.Find(playerId, isaac.Id).Trust);
            CollectionAssert.Contains(restoredD10.Find(playerId, isaac.Id).CauseEventIds,
                "event_d10_rescue");
            Assert.AreEqual(relationBeforeLoad.Trust,
                FindCanonicalEdge(restoredGraph, playerId, isaac.Id, RelationType.Trust).Value, 0.0001f);
            Assert.AreEqual("outfit_missing_tailor_placeholder",
                restoredD5.Appearance.ActiveLoadout.Get(D10AppearanceSlot.Accessory));
            Assert.AreEqual("title.harbor_witness", restoredIdentity.Title.Key);

            var restoredCrew = FindCrew(isaac.Id);
            Assert.IsNotNull(restoredCrew);
            Assert.AreEqual(CrewBehaviorStatus.Slacking, restoredCrew.BehaviorState.Status);
            Assert.AreEqual(behaviorSequenceBeforeLoad, restoredCrew.BehaviorState.TransitionSequence);

            var restoredCanonical = new RelationSystem(restoredLog,
                new[] { playerId, isaac.Id, oldSailor.Id });
            restoredCanonical.Restore(restoredGraph);
            var restoredPanel = D10RelationPanelReadModel.Build(restoredCanonical, restoredLog,
                playerId, isaac.Id);
            Assert.IsTrue(restoredPanel.IsAvailable);
            Assert.IsTrue(HasFoundCause(restoredPanel, "event_d10_rescue"));

            var restoredAppearance = new D10AppearanceSystem(restoredD5.Appearance);
            var restoredIdentitySystem = new D10PlayerIdentitySystem(restoredIdentity,
                new D10AppearanceOwnerWriter(restoredAppearance));
            var restoredInspection = restoredIdentitySystem.InspectAtPort(true, PortCatalog.StartingPortId);
            Assert.AreEqual("title.harbor_witness", restoredInspection.Identity.Title.Key);
            StringAssert.Contains("outfit_missing_tailor_placeholder",
                restoredInspection.CurrentAppearanceLoadoutId);
        }

        private static CrewAgent FindCrew(string id)
        {
            foreach (var crew in GameState.CrewAgents)
                if (crew != null && crew.Id == id) return crew;
            return null;
        }

        private static bool HasFact(EventLog log, string eventId)
        {
            foreach (var fact in log.Facts)
                if (fact != null && fact.EventId == eventId) return true;
            return false;
        }

        private static bool HasFoundCause(RelationPanelRow row, string eventId)
        {
            foreach (var cause in row.Causes)
                if (cause.EventId == eventId && cause.Found) return true;
            return false;
        }

        private static bool HasFoundCause(RelationPanelSnapshot panel, string eventId)
        {
            foreach (var row in panel.Rows)
                if (HasFoundCause(row, eventId)) return true;
            return false;
        }

        private static RelationEdge FindCanonicalEdge(RelationGraphSnapshot graph,
            string sourceId, string targetId, RelationType relationType)
        {
            foreach (var edge in graph.Edges)
                if (edge.SourceId == sourceId && edge.TargetId == targetId
                    && edge.RelationType == relationType) return edge;
            return null;
        }
    }
}
