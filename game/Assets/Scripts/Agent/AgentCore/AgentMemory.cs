using System;
using System.Collections.Generic;

namespace Whalebone.Agent
{
    // ============================================================
    // 记忆事件（长期记忆的最小单元）
    // ============================================================
    public class MemoryEvent
    {
        public DateTime Time;
        public float Importance;   // 0~1，影响长期保留与检索权重
        public string Kind;        // dialogue / event / decision
        public string Text;

        public MemoryEvent(string kind, string text, float importance)
        {
            Time = DateTime.Now;
            Kind = kind;
            Text = text;
            Importance = importance;
        }
    }

    // ============================================================
    // 记忆存储接口：骨架用内存实现；Unity 阶段接入 SQLite
    // ============================================================
    public interface IMemoryStore
    {
        void Append(MemoryEvent e);
        List<MemoryEvent> Top(int n);
    }

    public class InMemoryStore : IMemoryStore
    {
        private readonly List<MemoryEvent> _events = new List<MemoryEvent>();
        public void Append(MemoryEvent e) => _events.Add(e);
        public List<MemoryEvent> Top(int n)
        {
            if (_events.Count <= n) return new List<MemoryEvent>(_events);
            var sorted = new List<MemoryEvent>(_events);
            sorted.Sort((a, b) => b.Importance.CompareTo(a.Importance));
            return sorted.GetRange(0, n);
        }
    }

    // ============================================================
    // 三层记忆：短期上下文 / 会话摘要 / 长期事件
    // 范式参考：斯坦福 Generative Agents（重要性加权 + 时间衰减）
    // ============================================================
    public class AgentMemory
    {
        public const int MaxShortContext = 8;       // 短期：最近 8 轮
        public const int SummarizeEvery = 10;       // 每 10 轮触发一次摘要

        private readonly LinkedList<string> _shortContext = new LinkedList<string>(); // "玩家: xxx\n船员: xxx"
        private string _sessionSummary = "";                                        // 中期摘要
        private readonly IMemoryStore _store;
        private int _turnCount;

        public AgentMemory(IMemoryStore store) { _store = store; }

        public void RecordDialogue(string player, string agent)
        {
            _shortContext.AddLast("玩家: " + player);
            _shortContext.AddLast("船员: " + agent);
            while (_shortContext.Count > MaxShortContext) _shortContext.RemoveFirst();
            _turnCount++;
        }

        public void RecordEvent(string kind, string text, float importance)
        {
            _store.Append(new MemoryEvent(kind, text, importance));
        }

        public bool ShouldSummarize => _turnCount > 0 && _turnCount % SummarizeEvery == 0;

        /// <summary>把三层记忆拼进 system prompt（长记忆只取重要 Top 3）。</summary>
        public string BuildMemorySection()
        {
            var parts = new List<string>();
            if (_sessionSummary.Length > 0) parts.Add("本轮航程摘要：" + _sessionSummary);
            foreach (var e in _store.Top(3))
                parts.Add(e.Kind + "：「" + e.Text + "」");
            return parts.Count > 0 ? string.Join(" ", parts) : "";
        }

        /// <summary>触发摘要压缩（由 LLM 生成）。骨架里做文本拼接占位。</summary>
        public void Summarize(ILLMClient llm)
        {
            // 正式版：调用 LLM 把短期上下文压缩为 3 条要点，此处先简化
            _sessionSummary = "（第" + _turnCount + "轮对话后的航程记忆）";
        }
    }
}
