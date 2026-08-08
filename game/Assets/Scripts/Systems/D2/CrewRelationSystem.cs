// ============================================================
// CrewRelationSystem —— D2 船员关系数值裁决
//
// AgentReply 只提供提议；本类是 Affinity 的唯一运行时裁决入口。
// UI、LLM 和 SaveSystem 都不能直接写 CrewStats.Affinity。
// ============================================================

using System;
using Whalebone.Agent;

namespace Whalebone.Systems.D2
{
    public static class CrewRelationSystem
    {
        public static bool ApplyAgentProposal(CrewAgent agent, AgentReply reply)
        {
            if (agent == null || agent.Stats == null || reply == null || !reply.IsValid) return false;
            float before = agent.Stats.Affinity;
            float delta = Math.Max(-10f, Math.Min(10f, reply.AffinityChange));
            agent.Stats.ApplyAffinityChange(delta);
            return Math.Abs(agent.Stats.Affinity - before) > 0.0001f;
        }
    }
}
