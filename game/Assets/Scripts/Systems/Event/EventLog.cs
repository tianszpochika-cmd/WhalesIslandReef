// ============================================================
// EventLog —— D8 事实日志、因果读取与结算幂等门
//
// Append 是事实进入系统的唯一入口。EventLog 不调用领域系统，也不持有
// 任何金钱、价格、伤害、奖励、天气或关系数值；ClaimSettlement 只返回
// “是否首次声明结算”的门控结果，实际写入必须由对应 owner 完成。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Whalebone.Systems.Events
{
    public sealed class EventLog
    {
        private readonly List<FactEvent> _facts = new List<FactEvent>();
        private readonly Dictionary<string, FactEvent> _byKey =
            new Dictionary<string, FactEvent>(StringComparer.Ordinal);
        private readonly HashSet<string> _settledKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private long _nextSequence;

        public EventLog() { }

        public EventLog(EventLogSnapshot snapshot)
        {
            Restore(snapshot);
        }

        public int Count { get { return _facts.Count; } }
        public long NextSequence { get { return _nextSequence; } }

        /// <summary>返回副本；UI/Quest/Agent 不获得事实日志的写引用。</summary>
        public IReadOnlyList<FactEvent> Facts
        {
            get { return ReadFactsForUi(int.MaxValue); }
        }

        /// <summary>
        /// 先验证并追加 FactEvent，再返回给调用者。重复键只返回已有事实，绝不追加第二条。
        /// </summary>
        public EventAppendResult Append(WorldEvent worldEvent)
        {
            if (worldEvent == null) throw new EventValidationException("WorldEvent cannot be null.");
            worldEvent.Validate(EventSchema.CurrentVersion);

            FactEvent existing;
            if (_byKey.TryGetValue(worldEvent.DedupeKey, out existing))
                return EventAppendResult.Duplicate(existing.CloneFact());

            var fact = new FactEvent(worldEvent, ++_nextSequence);
            _facts.Add(fact);
            _byKey.Add(fact.DedupeKey, fact);
            return EventAppendResult.Appended(fact.CloneFact());
        }

        /// <summary>
        /// 只有中断事件可以通过新的 Recovered 事实恢复；恢复事件必须反向引用中断事件。
        /// </summary>
        public EventAppendResult AppendRecovery(string interruptedEventId, long interruptedSaveRevision,
            WorldEvent recoveryEvent)
        {
            var interrupted = FindInternal(interruptedEventId, interruptedSaveRevision);
            if (interrupted == null)
                throw new EventValidationException("Interrupted event not found: " + interruptedEventId + ".");
            if (interrupted.OutcomeStatus != WorldEventStatus.Interrupted)
                throw new EventValidationException("Event is not interrupted: " + interruptedEventId + ".");
            if (recoveryEvent == null || recoveryEvent.OutcomeStatus != WorldEventStatus.Recovered)
                throw new EventValidationException("Recovery event must have outcome_status=recovered.");
            if (recoveryEvent.CauseIds == null || !recoveryEvent.CauseIds.Contains(interrupted.EventId))
                throw new EventValidationException(
                    "Recovery event must include interrupted event in cause_ids: " + interrupted.EventId + ".");
            return Append(recoveryEvent);
        }

        /// <summary>
        /// 结算幂等门。首次返回 Claimed，读档后再次调用返回 AlreadyClaimed。
        /// 此方法不发放奖励，也不修改任何领域状态。
        /// </summary>
        public EventSettlementResult ClaimSettlement(string eventId, long saveRevision)
        {
            var fact = FindInternal(eventId, saveRevision);
            if (fact == null)
                throw new EventValidationException("Cannot claim an event that is not in the fact log: " + eventId + ".");
            if (_settledKeys.Contains(fact.DedupeKey))
                return EventSettlementResult.AlreadyClaimed(fact.CloneFact());

            _settledKeys.Add(fact.DedupeKey);
            return EventSettlementResult.Claimed(fact.CloneFact());
        }

        public FactEvent Find(string eventId, long saveRevision)
        {
            var fact = FindInternal(eventId, saveRevision);
            return fact == null ? null : fact.CloneFact();
        }

        public IReadOnlyList<FactEvent> ReadForQuest(IEnumerable<string> eventIds)
        {
            var requested = new HashSet<string>(
                eventIds == null ? Enumerable.Empty<string>() : eventIds,
                StringComparer.Ordinal);
            var result = new List<FactEvent>();
            foreach (var fact in _facts)
                if (requested.Contains(fact.EventId)) result.Add(fact.CloneFact());
            return result;
        }

        public IReadOnlyList<FactEvent> ReadFactsForUi(int maxEntries = 20)
        {
            if (maxEntries <= 0) return new List<FactEvent>();
            int count = Math.Min(maxEntries, _facts.Count);
            var result = new List<FactEvent>(count);
            for (int i = _facts.Count - count; i < _facts.Count; i++)
                result.Add(_facts[i].CloneFact());
            return result;
        }

        /// <summary>只拼接已经落盘的事实；Agent 只能读取此类摘要。</summary>
        public string BuildAgentSummary(int maxEntries = 5)
        {
            var facts = ReadFactsForUi(maxEntries);
            if (facts.Count == 0) return string.Empty;

            var parts = new List<string>();
            foreach (var fact in facts)
            {
                string resultCode = fact.Result == null ? "unknown" : fact.Result.ResultCode;
                string summary = fact.Result == null ? string.Empty : fact.Result.Summary;
                string cause = fact.CauseIds == null || fact.CauseIds.Count == 0
                    ? string.Empty : " cause=" + string.Join(",", fact.CauseIds.ToArray());
                parts.Add("[" + fact.EventId + "] day=" + fact.WorldDay +
                    " status=" + fact.OutcomeStatus.ToString().ToLowerInvariant() +
                    " owner=" + fact.OwnerSystem + " result=" + resultCode +
                    (string.IsNullOrEmpty(summary) ? string.Empty : " " + summary) + cause);
            }
            return string.Join(" | ", parts.ToArray());
        }

        /// <summary>返回所有直接及间接因果事实，结果按日志序列排序。</summary>
        public IReadOnlyList<FactEvent> TraceCauses(string eventId, long saveRevision, int maxDepth = 64)
        {
            var root = FindInternal(eventId, saveRevision);
            if (root == null) throw new EventValidationException("Event not found: " + eventId + ".");
            if (maxDepth <= 0) return new List<FactEvent>();

            var found = new Dictionary<string, FactEvent>(StringComparer.Ordinal);
            var queue = new Queue<TraceNode>();
            queue.Enqueue(new TraceNode(root, 0));
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Depth >= maxDepth || node.Event.CauseIds == null) continue;
                foreach (var causeId in node.Event.CauseIds)
                {
                    var cause = FindCause(causeId, saveRevision);
                    if (cause == null || found.ContainsKey(cause.DedupeKey)) continue;
                    found[cause.DedupeKey] = cause.CloneFact();
                    queue.Enqueue(new TraceNode(cause, node.Depth + 1));
                }
            }

            var result = found.Values.ToList();
            result.Sort((a, b) => a.LogSequence.CompareTo(b.LogSequence));
            return result;
        }

        public IReadOnlyList<FactEvent> GetCauseChain(string eventId, long saveRevision, int maxDepth = 64)
        {
            return TraceCauses(eventId, saveRevision, maxDepth);
        }

        public EventLogSnapshot CaptureSnapshot()
        {
            var snapshot = new EventLogSnapshot { NextSequence = _nextSequence };
            foreach (var fact in _facts) snapshot.Facts.Add(fact.CloneFact());
            snapshot.SettledKeys.AddRange(_settledKeys);
            return snapshot;
        }

        public static EventLog FromSnapshot(EventLogSnapshot snapshot)
        {
            return new EventLog(snapshot);
        }

        /// <summary>读档入口；先校验版本和事实，再恢复结算门状态。</summary>
        public void Restore(EventLogSnapshot snapshot)
        {
            if (snapshot == null) throw new EventValidationException("EventLogSnapshot cannot be null.");
            if (snapshot.SchemaVersion != EventSchema.EventLogVersion)
                throw new EventSchemaVersionException(
                    "EventLog schema_version mismatch: expected " + EventSchema.EventLogVersion +
                    ", received " + snapshot.SchemaVersion + ".");

            var restoredFacts = new List<FactEvent>();
            var restoredByKey = new Dictionary<string, FactEvent>(StringComparer.Ordinal);
            var restoredSettledKeys = new HashSet<string>(StringComparer.Ordinal);
            long restoredNextSequence = 0;
            var sequenceSet = new HashSet<long>();

            if (snapshot.Facts != null)
            {
                foreach (var savedFact in snapshot.Facts)
                {
                    if (savedFact == null) throw new EventValidationException("EventLog contains a null fact.");
                    savedFact.Validate(EventSchema.CurrentVersion);
                    if (savedFact.LogSequence <= 0)
                        throw new EventValidationException("Fact log_sequence must be > 0: " + savedFact.EventId + ".");
                    if (!sequenceSet.Add(savedFact.LogSequence))
                        throw new EventValidationException("Duplicate fact log_sequence in event log snapshot: " + savedFact.LogSequence + ".");
                    if (restoredByKey.ContainsKey(savedFact.DedupeKey))
                        throw new EventValidationException("Duplicate fact key in event log snapshot: " + savedFact.DedupeKey + ".");

                    var fact = savedFact.CloneFact();
                    restoredFacts.Add(fact);
                    restoredByKey.Add(fact.DedupeKey, fact);
                    restoredNextSequence = Math.Max(restoredNextSequence, fact.LogSequence);
                }
            }

            if (snapshot.NextSequence < restoredNextSequence)
                throw new EventValidationException("EventLog next sequence is behind its facts.");
            restoredNextSequence = snapshot.NextSequence;

            if (snapshot.SettledKeys != null)
                foreach (var key in snapshot.SettledKeys)
                {
                    if (!restoredByKey.ContainsKey(key))
                        throw new EventValidationException("Settlement claim references missing fact: " + key + ".");
                    restoredSettledKeys.Add(key);
                }

            _facts.Clear();
            _byKey.Clear();
            _settledKeys.Clear();
            _facts.AddRange(restoredFacts);
            foreach (var pair in restoredByKey) _byKey.Add(pair.Key, pair.Value);
            foreach (var key in restoredSettledKeys) _settledKeys.Add(key);
            _nextSequence = restoredNextSequence;
        }

        private FactEvent FindInternal(string eventId, long saveRevision)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            FactEvent fact;
            string key = eventId + "\u001f" + saveRevision.ToString(CultureInfo.InvariantCulture);
            return _byKey.TryGetValue(key, out fact) ? fact : null;
        }

        private FactEvent FindCause(string eventId, long saveRevision)
        {
            var exact = FindInternal(eventId, saveRevision);
            if (exact != null) return exact;
            for (int i = _facts.Count - 1; i >= 0; i--)
                if (string.Equals(_facts[i].EventId, eventId, StringComparison.Ordinal)) return _facts[i];
            return null;
        }

        private sealed class TraceNode
        {
            public readonly FactEvent Event;
            public readonly int Depth;

            public TraceNode(FactEvent @event, int depth)
            {
                Event = @event;
                Depth = depth;
            }
        }
    }
}
