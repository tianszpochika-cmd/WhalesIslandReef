using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D4
{
    /// <summary>航线事实日志的唯一追加入口；不负责生成或修改其他系统数值。</summary>
    public sealed class D4LogSystem
    {
        private readonly List<RouteLogEntry> _entries;

        public D4LogSystem(List<RouteLogEntry> initial = null)
        {
            _entries = new List<RouteLogEntry>();
            if (initial != null)
                foreach (var entry in initial) if (entry != null) _entries.Add(entry.Clone());
        }

        public IReadOnlyList<RouteLogEntry> Entries => _entries;

        public RouteLogEntry Append(int day, D4LogKind kind, string text, string originId = null, string destinationId = null)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var entry = new RouteLogEntry
            {
                Id = "log_" + (_entries.Count + 1).ToString("D4"),
                Day = Math.Max(0, day),
                Kind = kind,
                OriginId = originId,
                DestinationId = destinationId,
                Text = text,
            };
            _entries.Add(entry);
            if (_entries.Count > 100) _entries.RemoveAt(0);
            return entry.Clone();
        }

        public string BuildSummary(int maxEntries = 3)
        {
            if (_entries.Count == 0) return "航线日志尚无记录。";
            int count = Math.Max(1, Math.Min(maxEntries, _entries.Count));
            var parts = new List<string>();
            for (int i = _entries.Count - count; i < _entries.Count; i++) parts.Add(_entries[i].Text);
            return string.Join("｜", parts.ToArray());
        }

        public List<RouteLogEntry> CloneEntries()
        {
            var result = new List<RouteLogEntry>();
            foreach (var entry in _entries) result.Add(entry.Clone());
            return result;
        }
    }
}
