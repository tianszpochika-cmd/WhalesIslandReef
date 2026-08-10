using System;
using System.Collections.Generic;

namespace Whalebone.Agent
{
    /// <summary>D9-A04 纯 C# 纵切片测试；不启动 Unity、网络或模型。</summary>
    public static class D9CrewAgentVerticalSliceTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("D9 Persona 与岗位完整", PersonaAndDutyProfile);
            failed += Check("D9 无模型模板可提出航线建议", OfflineRouteTemplate);
            failed += Check("D9 无模型模板可提出岗位建议", OfflineDutyTemplate);
            failed += Check("D9 事件摘要进入短期记忆", FactSummaryFeedsMemory);
            failed += Check("D9 命令提议不直接修改关系", ProposalsDoNotMutateStats);
            failed += Check("D9 关系裁决强制绑定 cause_event_ids", RelationRequiresCauseEvent);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("[EX] " + name + " : " + e.Message);
                return 1;
            }
        }

        private static bool PersonaAndDutyProfile()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            return slice.Agent.Id == D9CrewAgentVerticalSlice.IsaacReedId
                && slice.Agent.Persona.Name == "艾萨克·里德"
                && slice.Agent.Persona.PrimaryDuty == CrewDuty.Deckhand
                && slice.Agent.Persona.QualifiedDuties.Contains(CrewDuty.Navigator)
                && slice.Agent.Persona.QualifiedDuties.Contains(CrewDuty.Lookout)
                && slice.Agent.Session != null;
        }

        private static bool OfflineRouteTemplate()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var reply = slice.Speak("请给我一个去绿湾的航线建议");
            var proposals = slice.ProjectReply(reply).Proposals;
            return reply.ActionIntent.Name == "set_course"
                && proposals.Count == 1
                && proposals[0].Kind == CrewProposalKind.Route
                && proposals[0].DestinationRef == "port:green_cay"
                && reply.AffinityChange == 0f;
        }

        private static bool OfflineDutyTemplate()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var reply = slice.Speak("风暴前岗位怎么安排？");
            var proposals = slice.ProjectReply(reply).Proposals;
            return reply.ActionIntent.Name == "assign_duty"
                && proposals.Count == 1
                && proposals[0].Kind == CrewProposalKind.Duty
                && proposals[0].ProposedDuty == CrewDuty.Lookout
                && slice.Agent.Persona.PrimaryDuty == CrewDuty.Deckhand;
        }

        private static bool FactSummaryFeedsMemory()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var summary = new CrewFactSummary(
                "event_d9_weather_storm", 2, "nearshore_saint_marys", "EnvironmentSystem",
                "weather_storm", "风暴已由环境系统记录。", new[] { "event_d9_departure" });
            if (!slice.RecordFactSummary(summary)) return false;
            slice.Speak("天气如何？");
            var memory = slice.Agent.Memory.BuildMemorySection();
            return slice.FactSummaries.Count == 1
                && memory.Contains("event_d9_weather_storm")
                && memory.Contains("cause_event_ids=event_d9_departure")
                && memory.Contains("近期对话");
        }

        private static bool ProposalsDoNotMutateStats()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            float before = slice.Agent.Stats.Affinity;
            var proposals = slice.ProjectReply(slice.Speak("风暴前岗位怎么安排？")).Proposals;
            return proposals.Count == 1 && slice.Agent.Stats.Affinity == before;
        }

        private static bool RelationRequiresCauseEvent()
        {
            var slice = D9CrewAgentVerticalSlice.CreateIsaacReed();
            var reply = new AgentReply
            {
                SchemaVersion = AgentReply.CurrentSchemaVersion,
                Speech = "这件事让我更信任船长。",
                DialogueIntent = DialogueIntent.Inform,
                Emotion = Emotion.Happy,
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

            var withoutCause = slice.ProjectReply(reply).Proposals;
            var rejected = slice.EvaluateRelationProposal(withoutCause[0]);
            if (rejected.Status != CrewRelationDecisionStatus.MissingCauseEvent
                || slice.Agent.Stats.Affinity != 0f) return false;

            var summary = new CrewFactSummary(
                "event_d9_weather_storm", 2, "nearshore_saint_marys", "EnvironmentSystem",
                "weather_storm", "风暴事实已记录。");
            if (!slice.RecordFactSummary(summary)) return false;
            var withCause = slice.ProjectReply(reply).Proposals;
            var applied = slice.EvaluateRelationProposal(withCause[0]);
            return applied.Status == CrewRelationDecisionStatus.Applied
                && applied.AppliedDelta == 4f
                && applied.After == 4f
                && slice.Agent.Stats.Affinity == 0f
                && ContainsCause(applied.CauseEventIds, "event_d9_weather_storm");
        }

        private static bool ContainsCause(IReadOnlyList<string> values, string expected)
        {
            if (values == null) return false;
            for (int i = 0; i < values.Count; i++)
                if (values[i] == expected) return true;
            return false;
        }
    }
}
