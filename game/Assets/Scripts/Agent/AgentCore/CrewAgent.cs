using System.Collections.Generic;

namespace Whalebone.Agent
{
    // ============================================================
    // 船员 Agent：人格 + 数值 + 记忆 的聚合体
    // 由外部注入 DialogueSession 完成对话闭环（依赖倒置，便于测试）
    // ============================================================
    public class CrewAgent
    {
        public string Id;
        public string DisplayName => Persona != null ? Persona.Name : Id;

        public CrewStats Stats;
        public PersonaDefinition Persona;
        public AgentMemory Memory;
        public DialogueSession Session;   // 注入

        public CrewAgent(string id, PersonaDefinition persona, IMemoryStore store)
        {
            Id = id;
            Persona = persona;
            Stats = new CrewStats(id);
            Memory = new AgentMemory(store);
        }

        /// <summary>快照/状态导出（需求文档阶段可扩展为存档 Schema）。</summary>
        public Dictionary<string, object> ToSnapshot()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["morale"] = Stats.Morale,
                ["loyalty"] = Stats.Loyalty,
                ["health"] = Stats.Health,
                ["fatigue"] = Stats.Fatigue,
                ["affinity"] = Stats.Affinity,
                ["mutiny_risk"] = Stats.GetMutinyRisk()
            };
        }
    }
}
