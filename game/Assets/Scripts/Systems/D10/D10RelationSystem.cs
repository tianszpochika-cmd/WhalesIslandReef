// ============================================================
// RelationSystem —— D10 关系数值唯一写者
//
// 关系变化只能从已进入 EventLog 的 FactEvent 产生。UI、Agent、SaveSystem
// 只能读取/传递 DTO；本类是唯一修改 RelationEdge.Value 的入口。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public sealed class RelationSystem
    {
        private readonly EventLog _eventLog;
        private readonly HashSet<string> _knownCharacterIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, RelationEdge> _edges = new Dictionary<string, RelationEdge>(StringComparer.Ordinal);

        public RelationSystem(EventLog eventLog, IEnumerable<string> knownCharacterIds)
        {
            _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");
            if (knownCharacterIds != null)
                foreach (var id in knownCharacterIds)
                    if (!string.IsNullOrWhiteSpace(id)) _knownCharacterIds.Add(id);
        }

        public RelationSystem(IEnumerable<string> knownCharacterIds, EventLog eventLog)
            : this(eventLog, knownCharacterIds) { }

        public EventLog EventLog { get { return _eventLog; } }

        public bool KnowsCharacter(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && _knownCharacterIds.Contains(id);
        }

        public IReadOnlyList<string> KnownCharacterIds
        {
            get { return new List<string>(_knownCharacterIds).AsReadOnly(); }
        }

        /// <summary>新增角色引用由 Crew/Character 编排层提供，不会创建关系边。</summary>
        public bool RegisterCharacter(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && _knownCharacterIds.Add(id);
        }

        /// <summary>
        /// 唯一关系写入口。传入的 fact 必须已在构造时注入的 EventLog 中存在；
        /// intent 的 delta 仍是提议值，系统会按关系类型边界裁决后再写入。
        /// </summary>
        public RelationApplyResult ApplyFactEvent(FactEvent fact, RelationChangeIntent intent)
        {
            if (fact == null)
                return Failure(RelationApplyStatus.MissingFactEvent, "missing_fact_event", "关系变化缺少事实事件。", null, 0f, 0f, 0f, 0f, null);
            if (fact.SchemaVersion != EventSchema.CurrentVersion)
                return Failure(RelationApplyStatus.VersionMismatch, "event_schema_version_mismatch",
                    "事实事件 schema_version 不匹配，拒绝关系写入。", null, 0f, 0f, 0f, 0f, null);

            var loggedFact = _eventLog.Find(fact.EventId, fact.SaveRevision);
            if (loggedFact == null)
                return Failure(RelationApplyStatus.MissingFactEvent, "missing_fact_event",
                    "事实事件尚未进入 EventLog，拒绝关系写入：" + (fact.EventId ?? string.Empty), null,
                    0f, 0f, 0f, 0f, null);
            if (intent == null || string.IsNullOrWhiteSpace(intent.SourceId) || string.IsNullOrWhiteSpace(intent.TargetId)
                || !Enum.IsDefined(typeof(RelationType), intent.RelationType)
                || float.IsNaN(intent.ProposedDelta) || float.IsInfinity(intent.ProposedDelta))
                return Failure(RelationApplyStatus.InvalidIntent, "invalid_relation_intent", "关系变化意图无效。", null,
                    0f, 0f, 0f, 0f, loggedFact.CauseIds);
            if (!KnowsCharacter(intent.SourceId))
                return Failure(RelationApplyStatus.UnknownSource, "unknown_source_character",
                    "未知关系源角色：" + intent.SourceId, null, 0f, 0f, 0f, 0f, loggedFact.CauseIds);
            if (!KnowsCharacter(intent.TargetId))
                return Failure(RelationApplyStatus.UnknownTarget, "unknown_target_character",
                    "未知关系目标角色：" + intent.TargetId, null, 0f, 0f, 0f, 0f, loggedFact.CauseIds);
            if (!ReferencesCharacter(loggedFact, intent.SourceId) || !ReferencesCharacter(loggedFact, intent.TargetId))
                return Failure(RelationApplyStatus.InvalidFactEvent, "fact_actor_mismatch",
                    "事实事件未引用关系边两端角色，拒绝关系写入。", null, 0f, 0f, 0f, 0f, loggedFact.CauseIds);

            RelationEdge edge;
            if (!_edges.TryGetValue(EdgeKey(intent.SourceId, intent.TargetId, intent.RelationType), out edge))
            {
                edge = new RelationEdge
                {
                    SourceId = intent.SourceId,
                    TargetId = intent.TargetId,
                    RelationType = intent.RelationType,
                };
                _edges.Add(edge.Key, edge);
            }

            string eventKey = loggedFact.DedupeKey;
            if (edge.AppliedEventKeys.Contains(eventKey))
                return RelationApplyResult.Create(RelationApplyStatus.Duplicate, "duplicate_relation_event",
                    "同一事实事件已应用到该关系边，未重复结算。", edge, edge.Value, intent.ProposedDelta, 0f,
                    edge.Value, edge.CauseEventIds);

            float before = edge.Value;
            float proposedDelta = ClampProposal(intent.ProposedDelta);
            float after = RelationValueRules.ClampValue(intent.RelationType, before + proposedDelta);
            float appliedDelta = after - before;
            edge.Value = after;
            edge.Confidence = 1f;
            edge.LastChangedDay = Math.Max(0, loggedFact.WorldDay);
            edge.LastChangeSaveRevision = Math.Max(0L, loggedFact.SaveRevision);
            AddUnique(edge.AppliedEventKeys, eventKey);
            AddUnique(edge.CauseEventIds, loggedFact.EventId);
            if (loggedFact.CauseIds != null)
                foreach (var cause in loggedFact.CauseIds) AddUnique(edge.CauseEventIds, cause);

            var status = Math.Abs(appliedDelta) < 0.0001f
                ? RelationApplyStatus.NoChange : RelationApplyStatus.Applied;
            return RelationApplyResult.Create(status,
                status == RelationApplyStatus.Applied ? "relation_applied" : "relation_no_change",
                status == RelationApplyStatus.Applied ? "关系变化已由事实事件裁决并写入。" : "关系值已在边界处，未发生数值变化。",
                edge, before, proposedDelta, appliedDelta, after, edge.CauseEventIds);
        }

        public RelationEdge ReadEdge(string sourceId, string targetId, RelationType relationType)
        {
            RelationEdge edge;
            return _edges.TryGetValue(EdgeKey(sourceId, targetId, relationType), out edge) ? edge.Clone() : null;
        }

        /// <summary>返回副本；调用方没有关系边写引用。</summary>
        public IReadOnlyList<RelationEdge> ReadEdges()
        {
            var result = new List<RelationEdge>();
            foreach (var edge in _edges.Values) result.Add(edge.Clone());
            return result.AsReadOnly();
        }

        public RelationGraphSnapshot CaptureSnapshot()
        {
            var snapshot = new RelationGraphSnapshot();
            foreach (var edge in _edges.Values) snapshot.Edges.Add(edge.Clone());
            return snapshot;
        }

        /// <summary>读档恢复唯一入口；版本不匹配会给出明确的 RelationSchemaVersionException。</summary>
        public void Restore(RelationGraphSnapshot snapshot)
        {
            if (snapshot == null) throw new RelationValidationException("RelationGraphSnapshot cannot be null.");
            snapshot.Validate();
            var restored = new Dictionary<string, RelationEdge>(StringComparer.Ordinal);
            foreach (var savedEdge in snapshot.Edges)
            {
                if (!KnowsCharacter(savedEdge.SourceId))
                    throw new RelationValidationException("Unknown source character in saved relation: " + savedEdge.SourceId + ".");
                if (!KnowsCharacter(savedEdge.TargetId))
                    throw new RelationValidationException("Unknown target character in saved relation: " + savedEdge.TargetId + ".");
                savedEdge.Value = RelationValueRules.ClampValue(savedEdge.RelationType, savedEdge.Value);
                savedEdge.Confidence = Clamp01(savedEdge.Confidence);
                var clone = savedEdge.Clone();
                if (restored.ContainsKey(clone.Key)) throw new RelationValidationException("Duplicate saved relation: " + clone.Key + ".");
                restored.Add(clone.Key, clone);
            }
            _edges.Clear();
            foreach (var pair in restored) _edges.Add(pair.Key, pair.Value);
        }

        private static float ClampProposal(float value)
        {
            return Math.Max(-D10RelationSchema.MaxProposalDelta,
                Math.Min(D10RelationSchema.MaxProposalDelta, value));
        }

        private static bool ReferencesCharacter(FactEvent fact, string id)
        {
            if (fact.ActorIds != null && fact.ActorIds.Contains(id)) return true;
            return fact.Result != null && fact.Result.FactIds != null && fact.Result.FactIds.Contains(id);
        }

        private static string EdgeKey(string sourceId, string targetId, RelationType relationType)
        {
            return (sourceId ?? string.Empty) + "\u001f" + (targetId ?? string.Empty) + "\u001f" + relationType;
        }

        private static void AddUnique(List<string> values, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !values.Contains(value)) values.Add(value);
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static RelationApplyResult Failure(RelationApplyStatus status, string code, string message,
            RelationEdge edge, float before, float proposedDelta, float appliedDelta, float after,
            IEnumerable<string> causes)
        {
            return RelationApplyResult.Create(status, code, message, edge, before, proposedDelta, appliedDelta, after, causes);
        }
    }

    public static class RelationValueRules
    {
        public static float ClampValue(RelationType type, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            if (type == RelationType.Trust || type == RelationType.Loyalty
                || type == RelationType.Fear || type == RelationType.Respect
                || type == RelationType.Attachment || type == RelationType.Jealousy
                || type == RelationType.Romance)
                return Math.Max(0f, Math.Min(100f, value));
            if (type == RelationType.Debt)
                return Math.Max(0f, Math.Min(1000000f, value));
            return Math.Max(-100f, Math.Min(100f, value));
        }
    }
}
