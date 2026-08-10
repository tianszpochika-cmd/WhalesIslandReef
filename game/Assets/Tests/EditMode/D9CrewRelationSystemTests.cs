using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D2;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using System.Collections.Generic;

namespace D9.Tests
{
    public class D9CrewRelationSystemTests
    {
        [Test]
        public void CauseBoundProposal_StillRequiresLoggedFactEvent()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var reply = new AgentReply
            {
                Speech = "这件事让我更信任船长。",
                AffinityChange = 4f,
                RelationProposal = new RelationProposal
                {
                    Kind = RelationKind.Affinity,
                    TargetRef = D9CrewAgentVerticalSlice.RelationTargetRef,
                    Stance = RelationStance.Build,
                    Basis = "事实事件支持的关系提议。"
                },
                Action = "none",
                ActionIntent = ActionIntent.Empty()
            };

            var withoutCause = slice.ProjectReply(reply).Proposals[0];
            Assert.IsFalse(CrewRelationSystem.ApplyAgentProposal(slice.Agent, withoutCause));
            Assert.AreEqual(0f, slice.Agent.Stats.Affinity, 0.0001f);

            Assert.IsTrue(slice.RecordFactSummary(new CrewFactSummary(
                "event_d9_crew_report", 2, "nearshore_saint_marys", "CrewSystem",
                "crew_reported", "艾萨克已提交风暴风险报告。")));
            var withCause = slice.ProjectReply(reply).Proposals[0];
            Assert.IsFalse(CrewRelationSystem.ApplyAgentProposal(slice.Agent, withCause));
            Assert.AreEqual(0f, slice.Agent.Stats.Affinity, 0.0001f);

            var log = new EventLog();
            var fact = log.Append(new WorldEvent
            {
                EventId = "event_d9_crew_report",
                SchemaVersion = EventSchema.CurrentVersion,
                WorldDay = 2,
                LocationRef = "nearshore_saint_marys",
                ActorIds = new List<string> { slice.Agent.Id, "player:captain" },
                InputFacts = new List<string> { "crew_report" },
                OwnerSystem = "CrewSystem",
                Result = new EventResult { ResultCode = "crew_reported", Summary = "艾萨克已提交风暴风险报告。", FactIds = new List<string> { slice.Agent.Id, "player:captain" } },
                CauseIds = new List<string>(),
                SaveRevision = 2,
            }).Fact;
            var relationSystem = new RelationSystem(log, new[] { slice.Agent.Id, "player:captain" });
            var result = relationSystem.ApplyFactEvent(fact, CrewRelationSystem.ToRelationIntent(withCause));
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(4f, result.After, 0.0001f);
        }
    }
}
