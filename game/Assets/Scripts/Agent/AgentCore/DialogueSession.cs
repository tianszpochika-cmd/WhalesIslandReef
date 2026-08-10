using System;
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
        private readonly ActionValidationContext _validationContext;
        private readonly bool _allowActionProposals;
        private readonly Func<string, AgentReply> _templateFallback;

        public int MaxRetry = 1;               // JSON 解析失败重试次数

        public DialogueSession(CrewAgent agent, ILLMClient llm, MutinyMonitor monitor)
            : this(agent, llm, monitor, ActionValidationContext.SchemaOnly(), false, null)
        {
        }

        /// <summary>D9 入口：允许经授权快照校验的命令提议，仍不执行任何动作。</summary>
        public DialogueSession(CrewAgent agent, ILLMClient llm, MutinyMonitor monitor,
            ActionValidationContext validationContext, Func<string, AgentReply> templateFallback)
            : this(agent, llm, monitor, validationContext, true, templateFallback)
        {
        }

        private DialogueSession(CrewAgent agent, ILLMClient llm, MutinyMonitor monitor,
            ActionValidationContext validationContext, bool allowActionProposals,
            Func<string, AgentReply> templateFallback)
        {
            _agent = agent;
            _llm = llm;
            _monitor = monitor;
            _validationContext = validationContext ?? ActionValidationContext.SchemaOnly();
            _allowActionProposals = allowActionProposals;
            _templateFallback = templateFallback;
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

                bool accepted = _allowActionProposals
                    ? AgentReplyValidator.TryValidate(raw, _validationContext, out reply, out _)
                    : AgentReplyValidator.TryParseD2(raw, out reply, out _);
                if (accepted) break;
                reply = null;
            }

            // 4) 模板兜底：任何模型都失败时保证可玩
            if (reply == null)
                reply = _templateFallback == null
                    ? TemplateFallback.Reply(_agent.Persona, playerInput)
                    : _templateFallback(playerInput);

            // 5) 只记录对话内容。好感提议由 Crew 关系系统单独裁决并应用一次。
            _agent.Memory.RecordDialogue(playerInput, reply.Speech);
            if (_agent.Memory.ShouldSummarize) _agent.Memory.Summarize(_llm);

            return reply;
        }
    }
}
