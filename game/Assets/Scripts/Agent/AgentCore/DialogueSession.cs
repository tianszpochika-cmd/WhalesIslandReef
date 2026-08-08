using System.Threading.Tasks;

namespace Whalebone.Agent
{
    // ============================================================
    // 对话会话：玩家输入 → 组装 prompt → LLM → 结构化校验 → 数值裁决 → 写记忆
    // 这是"重交互"的最小闭环，也是 Agent 与强逻辑层之间的唯一通道。
    // ============================================================
    public class DialogueSession
    {
        private readonly CrewAgent _agent;
        private readonly ILLMClient _llm;
        private readonly MutinyMonitor _monitor;

        public int MaxRetry = 1;               // JSON 解析失败重试次数

        public DialogueSession(CrewAgent agent, ILLMClient llm, MutinyMonitor monitor)
        {
            _agent = agent;
            _llm = llm;
            _monitor = monitor;
        }

        /// <summary>
        /// 同步便捷入口（Unity 里请改用异步版 SpeakAsync 配合协程）。
        /// </summary>
        public AgentReply Speak(string playerInput) => SpeakAsync(playerInput).GetAwaiter().GetResult();

        public async Task<AgentReply> SpeakAsync(string playerInput)
        {
            // 1) 数值边界：叛变预警等危险状态会改写约束（锁分支，LLM 看不到不该给的选项）
            string boundaryNote = _monitor.BuildBoundaryNote();
            string memorySection = _agent.Memory.BuildMemorySection();

            // 2) 组装 system prompt
            string system = _agent.Persona.BuildSystemPrompt(_agent.Stats, memorySection);
            if (boundaryNote.Length > 0) system += "\n[注意] " + boundaryNote;

            // 3) 调用 LLM（本地→云端→失败抛异常）
            AgentReply reply = null;
            for (int attempt = 0; attempt <= MaxRetry; attempt++)
            {
                string raw;
                try { raw = await _llm.CompleteAsync(system, playerInput); }
                catch { break; } // 模型不可用，走模板兜底

                reply = AgentReply.FromJson(raw);
                if (reply.IsValid) break;
                reply = null;
            }

            // 4) 模板兜底：任何模型都失败时保证可玩
            if (reply == null) reply = TemplateFallback.Reply(_agent.Persona, playerInput);

            // 5) 数值裁决（强逻辑层）：好感度变化真实生效
            _agent.Stats.ApplyAffinityChange(reply.AffinityChange);

            // 6) 写记忆
            _agent.Memory.RecordDialogue(playerInput, reply.Speech);
            if (reply.AffinityChange != 0f)
                _agent.Memory.RecordEvent("event",
                    "船长与" + _agent.DisplayName + "交谈，好感变化" + reply.AffinityChange.ToString("+#;-#;0"), 0.6f);
            if (_agent.Memory.ShouldSummarize) _agent.Memory.Summarize(_llm);

            return reply;
        }
    }
}
