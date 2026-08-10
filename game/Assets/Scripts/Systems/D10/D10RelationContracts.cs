// ============================================================
// D10RelationContracts —— D10 多维关系边 DTO
//
// RelationEdge 是关系域的可保存事实投影。它不承载 Agent/LLM 执行入口；
// Agent 只能提交 RelationChangeIntent，最终写入必须经过 RelationSystem。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D10
{
    public static class D10RelationSchema
    {
        public const int CurrentVersion = 1;
        public const int RuntimeVersion = 1;
        public const float MaxProposalDelta = 10f;
    }

    /// <summary>关系边类型；D10-A01 的九类关系均为有向边。</summary>
    public enum RelationType
    {
        Affinity,
        Trust,
        Loyalty,
        Fear,
        Respect,
        Attachment,
        Jealousy,
        Debt,
        Romance,
    }

    public enum RelationVisibility
    {
        Public,
        Private,
        Rumored,
        Hidden,
    }

    public enum RelationApplyStatus
    {
        Applied,
        NoChange,
        Duplicate,
        UnknownSource,
        UnknownTarget,
        MissingFactEvent,
        InvalidFactEvent,
        InvalidIntent,
        VersionMismatch,
    }

    /// <summary>
    /// Agent/行为系统的关系变化意图。ProposedDelta 只是提议值，不能单独写入关系图。
    /// </summary>
    [Serializable]
    public sealed class RelationChangeIntent
    {
        public string SourceId;
        public string TargetId;
        public RelationType RelationType;
        public float ProposedDelta;
        public string Basis;

        public RelationChangeIntent() { }

        public RelationChangeIntent(string sourceId, string targetId, RelationType relationType,
            float proposedDelta, string basis = "")
        {
            SourceId = sourceId;
            TargetId = targetId;
            RelationType = relationType;
            ProposedDelta = proposedDelta;
            Basis = basis ?? string.Empty;
        }

        public RelationChangeIntent Clone()
        {
            return new RelationChangeIntent(SourceId, TargetId, RelationType, ProposedDelta, Basis);
        }
    }

    /// <summary>可存档的关系边；AppliedEventKeys 用于读档后的重复保护。</summary>
    [Serializable]
    public sealed class RelationEdge
    {
        public int SchemaVersion = D10RelationSchema.CurrentVersion;
        public string SourceId;
        public string TargetId;
        public RelationType RelationType;
        public float Value;
        public float Confidence = 1f;
        public List<string> CauseEventIds = new List<string>();
        public int LastChangedDay;
        public long LastChangeSaveRevision;
        public bool LockedByStory;
        public RelationVisibility Visibility = RelationVisibility.Private;

        /// <summary>event_id + save_revision；与 EventLog 的去重键保持一致。</summary>
        public List<string> AppliedEventKeys = new List<string>();

        // Contract-shaped aliases for serializers and boundary checks.
        public int schema_version { get { return SchemaVersion; } set { SchemaVersion = value; } }
        public string source_id { get { return SourceId; } set { SourceId = value; } }
        public string target_id { get { return TargetId; } set { TargetId = value; } }
        public RelationType relation_type { get { return RelationType; } set { RelationType = value; } }
        public float value { get { return Value; } set { Value = value; } }
        public float confidence { get { return Confidence; } set { Confidence = value; } }
        public List<string> cause_event_ids { get { return CauseEventIds; } set { CauseEventIds = value; } }
        public int last_changed_day { get { return LastChangedDay; } set { LastChangedDay = value; } }
        public long last_change_save_revision { get { return LastChangeSaveRevision; } set { LastChangeSaveRevision = value; } }
        public bool locked_by_story { get { return LockedByStory; } set { LockedByStory = value; } }
        public RelationVisibility visibility { get { return Visibility; } set { Visibility = value; } }
        public List<string> applied_event_keys { get { return AppliedEventKeys; } set { AppliedEventKeys = value; } }

        public string Key
        {
            get { return (SourceId ?? string.Empty) + "\u001f" + (TargetId ?? string.Empty) + "\u001f" + RelationType; }
        }

        public RelationEdge Clone()
        {
            var result = new RelationEdge
            {
                SchemaVersion = SchemaVersion,
                SourceId = SourceId,
                TargetId = TargetId,
                RelationType = RelationType,
                Value = Value,
                Confidence = Confidence,
                LastChangedDay = LastChangedDay,
                LastChangeSaveRevision = LastChangeSaveRevision,
                LockedByStory = LockedByStory,
                Visibility = Visibility,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class RelationGraphSnapshot
    {
        public int SchemaVersion = D10RelationSchema.CurrentVersion;
        public List<RelationEdge> Edges = new List<RelationEdge>();

        public RelationGraphSnapshot Clone()
        {
            var result = new RelationGraphSnapshot { SchemaVersion = SchemaVersion };
            if (Edges != null)
                foreach (var edge in Edges)
                    if (edge != null) result.Edges.Add(edge.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != D10RelationSchema.CurrentVersion)
                throw new RelationSchemaVersionException(
                    "RelationGraph schema_version mismatch: expected " + D10RelationSchema.CurrentVersion +
                    ", received " + SchemaVersion + ".");
            if (Edges == null) throw new RelationValidationException("RelationGraph edges must be present.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var edge in Edges)
            {
                if (edge == null) throw new RelationValidationException("RelationGraph contains a null edge.");
                if (edge.SchemaVersion != D10RelationSchema.CurrentVersion)
                    throw new RelationSchemaVersionException(
                        "RelationEdge schema_version mismatch for " + edge.Key + ".");
                if (string.IsNullOrWhiteSpace(edge.SourceId) || string.IsNullOrWhiteSpace(edge.TargetId))
                    throw new RelationValidationException("RelationEdge source_id and target_id are required.");
                if (!Enum.IsDefined(typeof(RelationType), edge.RelationType))
                    throw new RelationValidationException("Unknown relation_type for " + edge.Key + ".");
                if (!keys.Add(edge.Key)) throw new RelationValidationException("Duplicate relation edge: " + edge.Key + ".");
                if (edge.CauseEventIds == null || edge.CauseEventIds.Count == 0)
                    throw new RelationValidationException("RelationEdge cause_event_ids are required: " + edge.Key + ".");
                if (edge.AppliedEventKeys == null)
                    throw new RelationValidationException("RelationEdge applied event keys are required: " + edge.Key + ".");
                if (edge.LastChangedDay < 0 || edge.LastChangeSaveRevision < 0)
                    throw new RelationValidationException("RelationEdge change day/revision cannot be negative: " + edge.Key + ".");
                foreach (var cause in edge.CauseEventIds)
                    if (string.IsNullOrWhiteSpace(cause)) throw new RelationValidationException("RelationEdge cause_event_ids cannot be empty: " + edge.Key + ".");
                foreach (var appliedKey in edge.AppliedEventKeys)
                    if (string.IsNullOrWhiteSpace(appliedKey)) throw new RelationValidationException("RelationEdge applied event key cannot be empty: " + edge.Key + ".");
            }
        }
    }

    [Serializable]
    public sealed class RelationApplyResult
    {
        public RelationApplyStatus Status;
        public string Code;
        public string Message;
        public RelationEdge Edge;
        public float Before;
        public float ProposedDelta;
        public float AppliedDelta;
        public float After;
        public List<string> CauseEventIds = new List<string>();

        public bool Succeeded { get { return Status == RelationApplyStatus.Applied || Status == RelationApplyStatus.NoChange; } }
        public bool IsDuplicate { get { return Status == RelationApplyStatus.Duplicate; } }

        internal static RelationApplyResult Create(RelationApplyStatus status, string code, string message,
            RelationEdge edge, float before, float proposedDelta, float appliedDelta, float after,
            IEnumerable<string> causes)
        {
            var result = new RelationApplyResult
            {
                Status = status,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty,
                Edge = edge == null ? null : edge.Clone(),
                Before = before,
                ProposedDelta = proposedDelta,
                AppliedDelta = appliedDelta,
                After = after,
            };
            if (causes != null)
                foreach (var cause in causes)
                    if (!string.IsNullOrWhiteSpace(cause) && !result.CauseEventIds.Contains(cause)) result.CauseEventIds.Add(cause);
            return result;
        }
    }

    [Serializable]
    public class RelationValidationException : Exception
    {
        public RelationValidationException(string message) : base(message) { }
    }

    [Serializable]
    public sealed class RelationSchemaVersionException : RelationValidationException
    {
        public RelationSchemaVersionException(string message) : base(message) { }
    }
}
