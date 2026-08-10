// ============================================================
// CrewRelationSystem —— D2 兼容关系入口
//
// D10 起，RelationSystem 才是关系边的唯一运行时裁决入口。这里保留旧签名
// 作为兼容门，但不允许对白/AgentReply 直接修改 Affinity；调用方必须先把
// 事实追加到 EventLog，再调用 RelationSystem.ApplyFactEvent。
// ============================================================

using System;
using Whalebone.Agent;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D2
{
    public static class CrewRelationSystem
    {
        /// <summary>
        /// 旧 D2 入口不再直接写关系。AgentReply 没有事实日志引用，必须由 D10
        /// 关系系统接收已落盘 FactEvent 后才能产生关系变化。
        /// </summary>
        public static bool ApplyAgentProposal(CrewAgent agent, AgentReply reply)
        {
            return false;
        }

        /// <summary>
        /// D9 旧提议入口不再直接写 CrewStats；它只保留兼容签名，避免 Agent
        /// 绕过 EventLog。请使用 D10 RelationSystem.ApplyFactEvent。
        /// </summary>
        public static bool ApplyAgentProposal(CrewAgent agent, CrewAgentProposal proposal)
        {
            return false;
        }

        /// <summary>唯一允许给旧调用方使用的接线方式；实际写入仍在 RelationSystem 内。</summary>
        public static RelationApplyResult ApplyFactEvent(RelationSystem relationSystem,
            FactEvent fact, RelationChangeIntent intent)
        {
            if (relationSystem == null)
                return null;
            return relationSystem.ApplyFactEvent(fact, intent);
        }

        /// <summary>
        /// 将 AgentCore typed proposal 转成关系意图；不携带写入能力，也不执行裁决。
        /// cause_event_ids 由随后传入的已落盘 FactEvent 提供，避免 Agent 伪造因果。
        /// </summary>
        public static RelationChangeIntent ToRelationIntent(CrewAgentProposal proposal)
        {
            if (proposal == null || !proposal.IsRelationProposal) return null;
            RelationType type;
            switch (proposal.RelationKind)
            {
                case RelationKind.Trust: type = RelationType.Trust; break;
                case RelationKind.Respect: type = RelationType.Respect; break;
                case RelationKind.Loyalty: type = RelationType.Loyalty; break;
                case RelationKind.Fear: type = RelationType.Fear; break;
                case RelationKind.Romance: type = RelationType.Romance; break;
                case RelationKind.Affinity:
                case RelationKind.None: type = RelationType.Affinity; break;
                default: return null;
            }
            return new RelationChangeIntent(proposal.AgentId, proposal.TargetRef, type,
                proposal.AffinityDelta, proposal.Basis);
        }
    }
}
