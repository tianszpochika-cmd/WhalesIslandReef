using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D10;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    /// <summary>
    /// Q19 只读审计：对话只产生提议，岗位/关系仍由各自 owner 裁决，
    /// 共享事实、行为/关系快照和 AgentMemory 三层状态可保存恢复。
    /// </summary>
    public sealed class Q19CrewInteractionAuditTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_q19_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
            SaveSystem.NewGame(new NewGameParams { CaptainName = "Q19 审计船长" });
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
        public void OfflineDiscussion_DutyRelationAndSaveRestore_RestoresAgentMemory()
        {
            var eventLog = new EventLog();
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var isaac = slice.Agent;
            GameState.CrewAgents.Add(isaac);

            var offlineClient = new D9OfflineTemplateClient();
            Assert.IsFalse(offlineClient.Available, "本审计必须明确区分离线模板与在线 LLM。");

            // 对话只写 CrewSystem 的报告事实并产生 typed proposal，不得直接改岗位。
            var discussion = new D9CrewEventSystem(eventLog).Discuss(
                slice, "请安排瞭望岗位，随后报告风浪。", 1, 1901);
            Assert.IsTrue(discussion.Succeeded, discussion.Message);
            Assert.IsNotNull(discussion.Reply);
            Assert.AreEqual("assign_duty", discussion.Reply.ActionIntent.Name);
            Assert.IsNotNull(discussion.Proposals);
            Assert.AreEqual(1, discussion.Proposals.Proposals.Count);
            var dutyProposal = discussion.Proposals.Proposals[0];
            Assert.AreEqual(CrewProposalKind.Duty, dutyProposal.Kind);
            Assert.AreEqual(CrewDuty.Lookout, dutyProposal.ProposedDuty);
            Assert.AreEqual(CrewBehaviorStatus.Unassigned, isaac.BehaviorState.Status,
                "Agent 提议不能直接写入 CrewSystem 行为状态。");

            // 非法岗位必须由 CrewSystem 拒绝，且不能改变当前状态。
            var behavior = new D10CrewBehaviorSystem(eventLog);
            var initialized = behavior.EnsureState(isaac, 1, 1902,
                new[] { discussion.Fact.EventId });
            Assert.IsTrue(initialized.Accepted, initialized.Message);
            var invalidDuty = behavior.RequestDutyTransfer(isaac, CrewDuty.Helm, 1, 1903,
                new[] { discussion.Fact.EventId });
            Assert.IsFalse(invalidDuty.Accepted);
            Assert.AreEqual("duty_not_qualified", invalidDuty.Code);
            Assert.AreEqual(CrewDuty.Deckhand, isaac.BehaviorState.CurrentDuty);

            // 合法岗位必须经过请求和批准两步，状态变化和事实由 CrewSystem 写入。
            var requested = behavior.RequestDutyTransfer(isaac, dutyProposal.ProposedDuty, 1, 1904,
                new[] { discussion.Fact.EventId });
            Assert.IsTrue(requested.Accepted, requested.Message);
            Assert.AreEqual(CrewBehaviorStatus.TransferRequested, isaac.BehaviorState.Status);
            var approved = behavior.ApproveDutyTransfer(isaac, 1, 1905,
                new[] { discussion.Fact.EventId });
            Assert.IsTrue(approved.Accepted, approved.Message);
            Assert.AreEqual(CrewDuty.Lookout, isaac.BehaviorState.CurrentDuty);
            Assert.IsNotNull(eventLog.Find(approved.Event.EventId, 1905));

            // 关系动作仍须由 D10RelationSystem 追加事实并裁决，Agent 不能直接改关系值。
            var relations = new D10RelationSystem(eventLog);
            var relationRequest = new D10RelationActionRequest
            {
                SourceId = "player:captain",
                TargetId = isaac.Id,
                Action = D10RelationAction.Gift,
                WorldDay = 1,
                SaveRevision = 1906,
                ContextRef = "q19_crew_discussion",
                CauseEventIds = new List<string> { discussion.Fact.EventId },
            };
            var relation = relations.Apply(relationRequest);
            Assert.IsTrue(relation.Succeeded, relation.Message);
            Assert.IsNotNull(relation.Fact);
            var duplicateRelation = relations.Apply(relationRequest.Clone());
            Assert.IsTrue(duplicateRelation.IsDuplicate);
            Assert.AreEqual(relation.Relation.Affinity,
                relations.FindRelation("player:captain", isaac.Id).Affinity);

            string memoryBeforeSave = isaac.Memory.BuildMemorySection();
            Assert.IsTrue(memoryBeforeSave.IndexOf(D9CrewEventSystem.CrewReportEventId,
                StringComparison.Ordinal) >= 0);

            // 真实 SQLite 保存：EventLog、行为状态和 D10 关系快照走各自 owner 的入口。
            SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                EventLog = eventLog.CaptureSnapshot(),
            });
            SaveSystem.SaveD10Runtime(relations.Snapshot);

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restoredLog = SaveSystem.RestoreD3TradeEventLog();
            var restoredD10 = SaveSystem.LoadD10Runtime();
            Assert.IsNotNull(restoredLog.Find(D9CrewEventSystem.CrewReportEventId, 1901));
            Assert.IsNotNull(restoredLog.Find(approved.Event.EventId, 1905));
            Assert.IsNotNull(restoredLog.Find(D10RelationIds.EventId(D10RelationAction.Gift), 1906));
            Assert.IsNotNull(restoredD10);
            Assert.AreEqual(relation.Relation.Affinity,
                restoredD10.Find("player:captain", isaac.Id).Affinity);

            CrewAgent restoredIsaac = null;
            foreach (var crew in GameState.CrewAgents)
                if (crew != null && crew.Id == isaac.Id) restoredIsaac = crew;
            Assert.IsNotNull(restoredIsaac);
            Assert.AreEqual(CrewDuty.Lookout, restoredIsaac.BehaviorState.CurrentDuty);
            Assert.AreEqual(CrewBehaviorStatus.Resting, restoredIsaac.BehaviorState.Status);

            // AgentMemory 的短期上下文、摘要和长期事件都由 Save 域恢复；
            // Save 只复制记忆快照，不替 Crew/Relation owner 裁决任何领域状态。
            bool memoryRestored = restoredIsaac.Memory.BuildMemorySection().IndexOf(
                D9CrewEventSystem.CrewReportEventId, StringComparison.Ordinal) >= 0;
            Assert.IsTrue(memoryRestored,
                "读档后 AgentMemory 必须恢复已落盘的 crew report 事实摘要。");

            TestContext.Progress.WriteLine(
                "[Q19] offline_available={0}, discussion={1}, invalid_duty_rejected={2}, " +
                "duty_approved={3}, relation_success={4}, relation_duplicate={5}, " +
                "event_log_restored={6}, behavior_restored={7}, memory_restored={8}",
                offlineClient.Available,
                discussion.Succeeded,
                !invalidDuty.Accepted && invalidDuty.Code == "duty_not_qualified",
                approved.Accepted && isaac.BehaviorState.CurrentDuty == CrewDuty.Lookout,
                relation.Succeeded,
                duplicateRelation.IsDuplicate,
                restoredLog.Find(D9CrewEventSystem.CrewReportEventId, 1901) != null,
                restoredIsaac.BehaviorState.CurrentDuty == CrewDuty.Lookout,
                memoryRestored);
        }
    }
}
