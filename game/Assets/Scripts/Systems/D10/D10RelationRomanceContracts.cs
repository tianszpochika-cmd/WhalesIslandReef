// ============================================================
// D10RelationRomanceContracts —— D10 关系阶段与恋爱/冲突快照
//
// 兼容 D8 RelationEntity 的 source/target/value/cause 字段；D10 只补充
// 阶段、恋爱同意、冲突状态和承诺字段。所有状态由 D10RelationSystem
// 裁决，Agent/UI 只提交已经过边界校验的行动请求或读取快照。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public enum D10RelationPhase
    {
        Acquaintance,
        Trust,
        Intimacy,
        Commitment,
        Conflict,
        Stable,
        Separation,
    }

    public enum D10RomanceConsent
    {
        Undecided,
        Open,
        Accepted,
        Declined,
        Separated,
    }

    public enum D10RelationVisibility
    {
        Public,
        Private,
        Rumored,
        Hidden,
    }

    public enum D10RelationAction
    {
        Gift,
        PromiseKept,
        Rescue,
        PublicFavoritism,
        Jealousy,
        Argument,
        NightTalk,
        PersonalTask,
        RomanceAccept,
        RomanceDecline,
        ResolveConflict,
        Separate,
    }

    public static class D10RelationIds
    {
        public const string OwnerSystem = "CrewSystem";

        public static string EdgeId(string sourceId, string targetId)
        {
            return "relation:" + (sourceId ?? string.Empty) + "->" + (targetId ?? string.Empty);
        }

        public static string EventId(D10RelationAction action)
        {
            switch (action)
            {
                case D10RelationAction.Gift: return "event_d10_relation_gift";
                case D10RelationAction.PromiseKept: return "event_d10_promise_kept";
                case D10RelationAction.Rescue: return "event_d10_rescue";
                case D10RelationAction.PublicFavoritism: return "event_d10_public_favoritism";
                case D10RelationAction.Jealousy: return "event_d10_jealousy";
                case D10RelationAction.Argument: return "event_d10_argument";
                case D10RelationAction.NightTalk: return "event_d10_night_talk";
                case D10RelationAction.PersonalTask: return "event_d10_personal_task";
                case D10RelationAction.RomanceAccept: return "event_d10_romance_accepted";
                case D10RelationAction.RomanceDecline: return "event_d10_romance_declined";
                case D10RelationAction.ResolveConflict: return "event_d10_conflict_stabilized";
                case D10RelationAction.Separate: return "event_d10_relation_separated";
                default: return "event_d10_relation_unknown";
            }
        }

        public static string DedupeKey(string eventId, long saveRevision)
        {
            return (eventId ?? string.Empty) + "\u001f" + saveRevision;
        }
    }

    [Serializable]
    public sealed class D10RelationActionRequest
    {
        public string SourceId = string.Empty;
        public string TargetId = string.Empty;
        public D10RelationAction Action;
        public int WorldDay;
        public long SaveRevision;
        public string ContextRef = string.Empty;
        public List<string> CauseEventIds = new List<string>();

        /// <summary>
        /// PublicFavoritism 的旁观者列表。每个旁观者会获得一条独立的嫉妒关系边；
        /// 不传入旁观者时，偏袒只影响 source→target 关系。
        /// </summary>
        public List<string> AffectedSourceIds = new List<string>();

        public D10RelationActionRequest Clone()
        {
            var result = new D10RelationActionRequest
            {
                SourceId = SourceId,
                TargetId = TargetId,
                Action = Action,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                ContextRef = ContextRef,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            if (AffectedSourceIds != null) result.AffectedSourceIds.AddRange(AffectedSourceIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D10RelationEdge
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string Id = string.Empty;
        public string SourceId = string.Empty;
        public string TargetId = string.Empty;
        public D10RelationPhase Phase = D10RelationPhase.Acquaintance;
        public D10RomanceConsent RomanceConsent = D10RomanceConsent.Undecided;

        // D8 RelationEntity.value 的有界运行时投影；D10 各维度保持可解释。
        public int Affinity;
        public int Trust;
        public int Attachment;
        public int Respect;
        public int Jealousy;
        public int ConflictScore;
        public int Commitment;

        public bool ConflictActive;
        public bool ConflictHistory;
        public bool LockedByStory;
        public D10RelationVisibility Visibility = D10RelationVisibility.Private;
        public int LastChangedDay;
        public List<string> CauseEventIds = new List<string>();

        public static D10RelationEdge Create(string sourceId, string targetId)
        {
            return new D10RelationEdge
            {
                Id = D10RelationIds.EdgeId(sourceId, targetId),
                SourceId = sourceId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
            };
        }

        public D10RelationEdge Clone()
        {
            var result = new D10RelationEdge
            {
                SchemaVersion = SchemaVersion,
                Id = Id,
                SourceId = SourceId,
                TargetId = TargetId,
                Phase = Phase,
                RomanceConsent = RomanceConsent,
                Affinity = Affinity,
                Trust = Trust,
                Attachment = Attachment,
                Respect = Respect,
                Jealousy = Jealousy,
                ConflictScore = ConflictScore,
                Commitment = Commitment,
                ConflictActive = ConflictActive,
                ConflictHistory = ConflictHistory,
                LockedByStory = LockedByStory,
                Visibility = Visibility,
                LastChangedDay = LastChangedDay,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D10 relation schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (string.IsNullOrWhiteSpace(SourceId) || string.IsNullOrWhiteSpace(TargetId))
                throw new InvalidOperationException("D10 relation source_id and target_id are required.");
            if (string.Equals(SourceId, TargetId, StringComparison.Ordinal))
                throw new InvalidOperationException("D10 relation source_id and target_id must differ.");
            if (!string.Equals(Id, D10RelationIds.EdgeId(SourceId, TargetId), StringComparison.Ordinal))
                throw new InvalidOperationException("D10 relation id does not match source/target.");
            if (!Enum.IsDefined(typeof(D10RelationPhase), Phase) ||
                !Enum.IsDefined(typeof(D10RomanceConsent), RomanceConsent) ||
                !Enum.IsDefined(typeof(D10RelationVisibility), Visibility))
                throw new InvalidOperationException("D10 relation contains an unknown enum value.");
            if (Affinity < -100 || Affinity > 100 || Trust < 0 || Trust > 100 ||
                Attachment < 0 || Attachment > 100 || Respect < 0 || Respect > 100 ||
                Jealousy < 0 || Jealousy > 100 || ConflictScore < 0 || ConflictScore > 100 ||
                Commitment < 0 || Commitment > 100 || LastChangedDay < 0)
                throw new InvalidOperationException("D10 relation values are outside their ranges.");
            if (CauseEventIds == null)
                throw new InvalidOperationException("D10 relation cause_event_ids is required.");
        }
    }

    [Serializable]
    public sealed class D10RelationRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int WorldDay;
        public long Revision;
        public string LastEventId = string.Empty;
        public List<string> AppliedEventKeys = new List<string>();
        public List<D10RelationEdge> Relations = new List<D10RelationEdge>();

        /// <summary>
        /// D10-A01 canonical 多维关系图。阶段/恋爱投影保留在 Relations，
        /// 九类 RelationEdge 统一存入此字段，二者共享同一 D10 存档容器。
        /// </summary>
        public RelationGraphSnapshot CanonicalGraph = new RelationGraphSnapshot();

        public D10RelationEdge Find(string sourceId, string targetId)
        {
            string id = D10RelationIds.EdgeId(sourceId, targetId);
            foreach (var relation in Relations)
                if (relation != null && relation.Id == id) return relation;
            return null;
        }

        public D10RelationRuntimeSnapshot Clone()
        {
            var result = new D10RelationRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                WorldDay = WorldDay,
                Revision = Revision,
                LastEventId = LastEventId,
                CanonicalGraph = CanonicalGraph == null ? null : CanonicalGraph.Clone(),
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            if (Relations != null)
                foreach (var relation in Relations)
                    if (relation != null) result.Relations.Add(relation.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D10 relation runtime schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (WorldDay < 0 || Revision < 0)
                throw new InvalidOperationException("D10 relation runtime world_day and revision must be non-negative.");
            if (AppliedEventKeys == null || Relations == null)
                throw new InvalidOperationException("D10 relation runtime lists are required.");
            if (CanonicalGraph != null) CanonicalGraph.Validate();

            var appliedKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var appliedKey in AppliedEventKeys)
            {
                if (string.IsNullOrWhiteSpace(appliedKey))
                    throw new InvalidOperationException("D10 relation runtime contains an empty applied event key.");
                if (!appliedKeys.Add(appliedKey))
                    throw new InvalidOperationException("Duplicate D10 applied event key: " + appliedKey + ".");
            }

            var relationIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var relation in Relations)
            {
                if (relation == null) throw new InvalidOperationException("D10 relation runtime contains a null relation.");
                relation.Validate();
                if (!relationIds.Add(relation.Id))
                    throw new InvalidOperationException("Duplicate D10 relation edge: " + relation.Id + ".");
            }
        }
    }

    [Serializable]
    public sealed class D10RelationChangeResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool IsDuplicate;
        public string Code = string.Empty;
        public string Message = string.Empty;
        public string EventId = string.Empty;
        public D10RelationEdge Relation;
        public List<string> ChangedRelationIds = new List<string>();
        public FactEvent Fact;
        public D10RelationRuntimeSnapshot Snapshot;

        internal static D10RelationChangeResult Rejected(string code, string message,
            D10RelationRuntimeSnapshot snapshot)
        {
            return new D10RelationChangeResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                Snapshot = snapshot == null ? null : snapshot.Clone(),
            };
        }

        internal static D10RelationChangeResult Duplicate(string eventId, FactEvent fact,
            D10RelationEdge relation, D10RelationRuntimeSnapshot snapshot)
        {
            var result = new D10RelationChangeResult
            {
                Succeeded = true,
                Applied = false,
                IsDuplicate = true,
                Code = "duplicate_settlement",
                Message = "关系事实已经结算，未重复改变关系阶段或数值。",
                EventId = eventId ?? string.Empty,
                Fact = fact == null ? null : fact.CloneFact(),
                Relation = relation == null ? null : relation.Clone(),
                Snapshot = snapshot == null ? null : snapshot.Clone(),
            };
            return result;
        }

        internal static D10RelationChangeResult Success(string eventId, FactEvent fact,
            D10RelationEdge relation, IEnumerable<string> changedRelationIds,
            D10RelationRuntimeSnapshot snapshot)
        {
            var result = new D10RelationChangeResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ok",
                Message = "关系事实已记录，阶段和关系数值已由 CrewSystem 裁决。",
                EventId = eventId ?? string.Empty,
                Fact = fact == null ? null : fact.CloneFact(),
                Relation = relation == null ? null : relation.Clone(),
                Snapshot = snapshot == null ? null : snapshot.Clone(),
            };
            if (changedRelationIds != null) result.ChangedRelationIds.AddRange(changedRelationIds);
            return result;
        }
    }
}
