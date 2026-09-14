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

    /// <summary>
    /// Serializable, Unity-independent memory event projection used by the
    /// Save domain. The AgentCore model remains the owner of memory meaning;
    /// the Save domain only encodes and restores this projection.
    /// </summary>
    public sealed class MemoryEventSnapshot
    {
        public DateTime Time;
        public float Importance;
        public string Kind;
        public string Text;

        public MemoryEventSnapshot() { }

        public MemoryEventSnapshot(MemoryEvent source)
        {
            if (source == null) return;
            Time = source.Time;
            Importance = source.Importance;
            Kind = source.Kind ?? string.Empty;
            Text = source.Text ?? string.Empty;
        }
    }

    /// <summary>
    /// Complete persisted AgentMemory state. Lists are intentionally plain
    /// data so the Save domain can map them to its JSON representation without
    /// importing Unity or a third-party serializer into AgentCore.
    /// </summary>
    public sealed class AgentMemorySnapshot
    {
        public List<string> ShortContext = new List<string>();
        public string SessionSummary = string.Empty;
        public int TurnCount;
        public List<MemoryEventSnapshot> Events = new List<MemoryEventSnapshot>();
    }

    // ============================================================
    // 记忆存储接口：骨架用内存实现；Unity 阶段接入 SQLite
    // ============================================================
    public interface IMemoryStore
    {
        void Append(MemoryEvent e);
        List<MemoryEvent> Top(int n);
        List<MemoryEvent> Snapshot();
        void Replace(IEnumerable<MemoryEvent> events);
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

        public List<MemoryEvent> Snapshot() => new List<MemoryEvent>(_events);

        public void Replace(IEnumerable<MemoryEvent> events)
        {
            _events.Clear();
            if (events == null) return;
            foreach (var e in events)
                if (e != null) _events.Add(e);
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

        public AgentMemory(IMemoryStore store) { _store = store ?? new InMemoryStore(); }

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

        /// <summary>
        /// 记录来自已落盘事实日志的摘要。AgentCore 只保存摘要，不拥有 WorldEvent 的写权限，
        /// 也不从摘要反推或修改任何领域数值。
        /// </summary>
        public void RecordFactSummary(string eventId, string summary, float importance)
        {
            if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(summary)) return;
            RecordEvent("fact:" + eventId, summary, importance);
        }

        public bool ShouldSummarize => _turnCount > 0 && _turnCount % SummarizeEvery == 0;

        /// <summary>把三层记忆拼进 system prompt（长记忆只取重要 Top 3）。</summary>
        public string BuildMemorySection()
        {
            var parts = new List<string>();
            if (_shortContext.Count > 0)
                parts.Add("近期对话：" + string.Join(" | ", new List<string>(_shortContext).ToArray()));
            if (_sessionSummary.Length > 0) parts.Add("本轮航程摘要：" + _sessionSummary);
            foreach (var e in _store.Top(3))
                parts.Add(e.Kind + "：「" + e.Text + "」");
            return parts.Count > 0 ? string.Join(" ", parts) : "";
        }

        public IReadOnlyList<string> GetShortTermContext()
        {
            return new List<string>(_shortContext).AsReadOnly();
        }

        /// <summary>Capture all three memory layers for SaveSystem.</summary>
        public AgentMemorySnapshot CaptureSnapshot()
        {
            var snapshot = new AgentMemorySnapshot
            {
                ShortContext = new List<string>(_shortContext),
                SessionSummary = _sessionSummary ?? string.Empty,
                TurnCount = _turnCount,
            };
            foreach (var e in _store.Snapshot())
                if (e != null) snapshot.Events.Add(new MemoryEventSnapshot(e));
            return snapshot;
        }

        /// <summary>
        /// Restore a previously captured snapshot. This restores memory only;
        /// it never writes a domain fact, crew duty, relation, or other system
        /// state.
        /// </summary>
        public void RestoreSnapshot(AgentMemorySnapshot snapshot)
        {
            _shortContext.Clear();
            _sessionSummary = string.Empty;
            _turnCount = 0;
            _store.Replace(null);
            if (snapshot == null) return;

            if (snapshot.ShortContext != null)
            {
                foreach (var entry in snapshot.ShortContext)
                    _shortContext.AddLast(entry ?? string.Empty);
                while (_shortContext.Count > MaxShortContext) _shortContext.RemoveFirst();
            }
            _sessionSummary = snapshot.SessionSummary ?? string.Empty;
            _turnCount = Math.Max(0, snapshot.TurnCount);

            var events = new List<MemoryEvent>();
            if (snapshot.Events != null)
            {
                foreach (var item in snapshot.Events)
                {
                    if (item == null) continue;
                    var restored = new MemoryEvent(item.Kind ?? string.Empty, item.Text ?? string.Empty, item.Importance)
                    {
                        Time = item.Time == default(DateTime) ? DateTime.Now : item.Time,
                    };
                    events.Add(restored);
                }
            }
            _store.Replace(events);
        }

        /// <summary>触发摘要压缩（由 LLM 生成）。骨架里做文本拼接占位。</summary>
        public void Summarize(ILLMClient llm)
        {
            // 正式版：调用 LLM 把短期上下文压缩为 3 条要点，此处先简化
            _sessionSummary = "（第" + _turnCount + "轮对话后的航程记忆）";
        }
    }
}
