// ============================================================
// D10RelationRomanceSystem —— 关系阶段、恋爱与冲突强逻辑裁决
//
// 本系统是 D10 A03 阶段投影、恋爱同意、承诺和冲突状态的唯一写者。
// A01 canonicalGraph 仍由 RelationSystem 独占写入；本系统先向共享
// EventLog 追加事实，再提交阶段快照；重复 event_id + save_revision 不会重放影响。
// Agent/UI 不能直接写 D10RelationEdge。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public sealed class D10RelationSystem
    {
        private readonly EventLog _eventLog;
        private D10RelationRuntimeSnapshot _snapshot;

        public D10RelationSystem(EventLog eventLog, D10RelationRuntimeSnapshot initial = null)
        {
            _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");
            _snapshot = initial == null ? new D10RelationRuntimeSnapshot() : initial.Clone();
            _snapshot.Validate();
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D10RelationRuntimeSnapshot Snapshot
        {
            get { return _snapshot.Clone(); }
        }

        public D10RelationEdge FindRelation(string sourceId, string targetId)
        {
            var relation = _snapshot.Find(sourceId, targetId);
            return relation == null ? null : relation.Clone();
        }

        public void Restore(D10RelationRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            _snapshot = snapshot.Clone();
        }

        /// <summary>
        /// 读取 A01 canonical 关系图并刷新 A03 阶段投影。此方法只写本系统拥有的
        /// 阶段/表现字段，不修改传入的 RelationEdge，也不裁决 A01 的 Value。
        /// D10-S01 应在 A01 RelationSystem 应用事实后调用本入口。
        /// </summary>
        public void RefreshFromCanonicalGraph(RelationGraphSnapshot canonicalGraph)
        {
            if (canonicalGraph == null) throw new ArgumentNullException("canonicalGraph");
            canonicalGraph.Validate();

            var working = _snapshot.Clone();
            working.CanonicalGraph = canonicalGraph.Clone();
            foreach (var canonical in canonicalGraph.Edges)
            {
                var projection = working.Find(canonical.SourceId, canonical.TargetId);
                bool hasMappedMetric = false;
                if (projection == null)
                {
                    projection = D10RelationEdge.Create(canonical.SourceId, canonical.TargetId);
                }

                switch (canonical.RelationType)
                {
                    case RelationType.Affinity:
                        projection.Affinity = ToBoundedInt(canonical.Value, -100, 100);
                        hasMappedMetric = true;
                        break;
                    case RelationType.Trust:
                        projection.Trust = ToBoundedInt(canonical.Value, 0, 100);
                        hasMappedMetric = true;
                        break;
                    case RelationType.Attachment:
                        projection.Attachment = ToBoundedInt(canonical.Value, 0, 100);
                        hasMappedMetric = true;
                        break;
                    case RelationType.Respect:
                        projection.Respect = ToBoundedInt(canonical.Value, 0, 100);
                        hasMappedMetric = true;
                        break;
                    case RelationType.Jealousy:
                        projection.Jealousy = ToBoundedInt(canonical.Value, 0, 100);
                        hasMappedMetric = true;
                        break;
                }

                if (!hasMappedMetric) continue;
                projection.LastChangedDay = Math.Max(projection.LastChangedDay,
                    Math.Max(0, canonical.LastChangedDay));
                projection.LockedByStory = projection.LockedByStory || canonical.LockedByStory;
                projection.Visibility = ToProjectionVisibility(canonical.Visibility);
                if (canonical.CauseEventIds != null)
                    foreach (var cause in canonical.CauseEventIds) AddCause(projection, cause);
                ClampEdge(projection);
                ResolvePhase(projection);
                Upsert(working, projection);
            }

            working.Validate();
            _snapshot = working;
        }

        /// <summary>
        /// 由 CrewSystem/RelationSystem 统一裁决关系动作。请求不是最终数值，
        /// 只有通过前置条件和事实追加后，关系边才会被写入。
        /// </summary>
        public D10RelationChangeResult Apply(D10RelationActionRequest request)
        {
            string validationError = ValidateRequest(request);
            if (validationError != null)
                return D10RelationChangeResult.Rejected(validationError, RejectionMessage(validationError), _snapshot);

            string eventId = D10RelationIds.EventId(request.Action);
            long saveRevision = request.SaveRevision > 0
                ? request.SaveRevision
                : Math.Max(_snapshot.Revision + 1L, _eventLog.NextSequence + 1L);
            string dedupeKey = D10RelationIds.DedupeKey(eventId, saveRevision);

            if (_snapshot.AppliedEventKeys.Contains(dedupeKey))
                return D10RelationChangeResult.Duplicate(eventId, _eventLog.Find(eventId, saveRevision),
                    _snapshot.Find(request.SourceId, request.TargetId), _snapshot);

            var existingFact = _eventLog.Find(eventId, saveRevision);
            if (existingFact != null)
                return D10RelationChangeResult.Duplicate(eventId, existingFact,
                    _snapshot.Find(request.SourceId, request.TargetId), _snapshot);

            var primary = _snapshot.Find(request.SourceId, request.TargetId);
            string prerequisiteError = ValidatePrerequisites(request, primary);
            if (prerequisiteError != null)
                return D10RelationChangeResult.Rejected(prerequisiteError,
                    RejectionMessage(prerequisiteError), _snapshot);

            var working = new Dictionary<string, D10RelationEdge>(StringComparer.Ordinal);
            var beforePrimary = primary == null
                ? D10RelationEdge.Create(request.SourceId, request.TargetId)
                : primary.Clone();
            var workingPrimary = beforePrimary.Clone();
            ApplyActionEffects(workingPrimary, request.Action);
            workingPrimary.LastChangedDay = Math.Max(0, request.WorldDay);
            ResolvePhase(workingPrimary);
            working[workingPrimary.Id] = workingPrimary;

            if (request.Action == D10RelationAction.PublicFavoritism && request.AffectedSourceIds != null)
            {
                foreach (var sourceId in request.AffectedSourceIds)
                {
                    if (string.IsNullOrWhiteSpace(sourceId) ||
                        string.Equals(sourceId, request.SourceId, StringComparison.Ordinal) ||
                        string.Equals(sourceId, request.TargetId, StringComparison.Ordinal))
                        continue;

                    var oldJealousy = _snapshot.Find(sourceId, request.TargetId);
                    var jealousy = oldJealousy == null
                        ? D10RelationEdge.Create(sourceId, request.TargetId)
                        : oldJealousy.Clone();
                    ApplyJealousyEffects(jealousy);
                    jealousy.LastChangedDay = Math.Max(0, request.WorldDay);
                    ResolvePhase(jealousy);
                    working[jealousy.Id] = jealousy;
                }
            }

            var worldEvent = BuildWorldEvent(request, eventId, saveRevision, beforePrimary,
                workingPrimary, working.Values);
            var append = _eventLog.Append(worldEvent);
            if (append.IsDuplicate)
                return D10RelationChangeResult.Duplicate(eventId, append.Fact, primary, _snapshot);

            foreach (var changed in working.Values)
            {
                AddCause(changed, eventId);
                Upsert(changed);
            }
            _snapshot.WorldDay = Math.Max(_snapshot.WorldDay, Math.Max(0, request.WorldDay));
            _snapshot.Revision++;
            _snapshot.LastEventId = eventId;
            _snapshot.AppliedEventKeys.Add(dedupeKey);

            var changedIds = new List<string>(working.Keys);
            return D10RelationChangeResult.Success(eventId, append.Fact,
                _snapshot.Find(request.SourceId, request.TargetId), changedIds, _snapshot);
        }

        private string ValidateRequest(D10RelationActionRequest request)
        {
            if (request == null) return "request_required";
            if (string.IsNullOrWhiteSpace(request.SourceId) || string.IsNullOrWhiteSpace(request.TargetId))
                return "participants_required";
            if (string.Equals(request.SourceId, request.TargetId, StringComparison.Ordinal))
                return "self_relation_forbidden";
            if (!Enum.IsDefined(typeof(D10RelationAction), request.Action)) return "unknown_action";
            if (request.WorldDay < 0 || request.SaveRevision < 0) return "negative_time_or_revision";
            if (request.CauseEventIds != null)
                foreach (var cause in request.CauseEventIds)
                    if (string.IsNullOrWhiteSpace(cause)) return "invalid_cause_event";
            return null;
        }

        private string ValidatePrerequisites(D10RelationActionRequest request, D10RelationEdge relation)
        {
            switch (request.Action)
            {
                case D10RelationAction.RomanceAccept:
                    if (relation == null) return "romance_relation_required";
                    if (relation.RomanceConsent == D10RomanceConsent.Declined ||
                        relation.RomanceConsent == D10RomanceConsent.Separated)
                        return "romance_route_declined";
                    var reverse = _snapshot.Find(request.TargetId, request.SourceId);
                    if (reverse != null && (reverse.RomanceConsent == D10RomanceConsent.Declined ||
                        reverse.RomanceConsent == D10RomanceConsent.Separated))
                        return "romance_route_declined";
                    if (relation.Phase < D10RelationPhase.Intimacy || relation.Trust < 45 ||
                        relation.Affinity < 20 || relation.Attachment < 30)
                        return "romance_prerequisite";
                    break;
                case D10RelationAction.Separate:
                    if (relation == null) return "relation_required";
                    break;
                case D10RelationAction.ResolveConflict:
                    if (relation == null) return "relation_required";
                    if (!relation.ConflictActive && relation.Phase != D10RelationPhase.Conflict)
                        return "conflict_not_active";
                    break;
            }
            return null;
        }

        private static void ApplyActionEffects(D10RelationEdge edge, D10RelationAction action)
        {
            switch (action)
            {
                case D10RelationAction.Gift:
                    edge.Affinity += 6;
                    edge.Trust += 2;
                    edge.Attachment += 1;
                    break;
                case D10RelationAction.PromiseKept:
                    edge.Affinity += 3;
                    edge.Trust += 10;
                    edge.Commitment += 8;
                    edge.ConflictScore -= 15;
                    break;
                case D10RelationAction.Rescue:
                    edge.Affinity += 5;
                    edge.Trust += 14;
                    edge.Attachment += 8;
                    edge.Respect += 4;
                    edge.ConflictScore -= 10;
                    break;
                case D10RelationAction.PublicFavoritism:
                    edge.Affinity += 4;
                    edge.Trust += 2;
                    edge.Respect += 1;
                    break;
                case D10RelationAction.Jealousy:
                    ApplyJealousyEffects(edge);
                    break;
                case D10RelationAction.Argument:
                    edge.Affinity -= 7;
                    edge.Trust -= 10;
                    edge.Jealousy += 4;
                    edge.ConflictScore += 35;
                    edge.ConflictActive = true;
                    edge.ConflictHistory = true;
                    break;
                case D10RelationAction.NightTalk:
                    edge.Affinity += 4;
                    edge.Trust += 6;
                    edge.Attachment += 8;
                    edge.ConflictScore -= 12;
                    break;
                case D10RelationAction.PersonalTask:
                    edge.Trust += 8;
                    edge.Respect += 6;
                    edge.Attachment += 4;
                    edge.Commitment += 6;
                    edge.ConflictScore -= 5;
                    break;
                case D10RelationAction.RomanceAccept:
                    edge.RomanceConsent = D10RomanceConsent.Accepted;
                    edge.Attachment += 5;
                    edge.Commitment += 20;
                    edge.Trust += 3;
                    break;
                case D10RelationAction.RomanceDecline:
                    bool wasRomantic = edge.RomanceConsent == D10RomanceConsent.Accepted ||
                        edge.Phase == D10RelationPhase.Commitment;
                    edge.RomanceConsent = D10RomanceConsent.Declined;
                    edge.Commitment = 0;
                    if (wasRomantic)
                    {
                        edge.Phase = D10RelationPhase.Separation;
                        edge.Affinity -= 5;
                    }
                    break;
                case D10RelationAction.ResolveConflict:
                    edge.Trust += 5;
                    edge.Respect += 2;
                    edge.ConflictScore -= 40;
                    break;
                case D10RelationAction.Separate:
                    edge.RomanceConsent = D10RomanceConsent.Separated;
                    edge.Commitment = 0;
                    edge.ConflictActive = false;
                    edge.Affinity -= 12;
                    edge.Trust -= 20;
                    edge.Phase = D10RelationPhase.Separation;
                    break;
            }

            ClampEdge(edge);
            if (edge.ConflictScore <= 20) edge.ConflictActive = false;
        }

        private static void ApplyJealousyEffects(D10RelationEdge edge)
        {
            edge.Affinity -= 5;
            edge.Trust -= 7;
            edge.Jealousy += 18;
            edge.ConflictScore += 25;
            edge.ConflictActive = true;
            edge.ConflictHistory = true;
            ClampEdge(edge);
        }

        private static void ResolvePhase(D10RelationEdge edge)
        {
            if (edge.RomanceConsent == D10RomanceConsent.Separated || edge.Phase == D10RelationPhase.Separation)
            {
                edge.Phase = D10RelationPhase.Separation;
                return;
            }
            if (edge.ConflictActive)
            {
                edge.Phase = D10RelationPhase.Conflict;
                return;
            }
            if (edge.ConflictHistory && edge.Trust >= 35 && edge.Affinity >= -10)
            {
                edge.Phase = D10RelationPhase.Stable;
                return;
            }
            if (edge.RomanceConsent == D10RomanceConsent.Accepted && edge.Commitment >= 60 &&
                edge.Trust >= 60 && edge.Attachment >= 55)
            {
                edge.Phase = D10RelationPhase.Commitment;
                return;
            }
            if (edge.Attachment >= 45 && edge.Trust >= 45)
            {
                edge.Phase = D10RelationPhase.Intimacy;
                return;
            }
            if (edge.Trust >= 30)
            {
                edge.Phase = D10RelationPhase.Trust;
                return;
            }
            edge.Phase = D10RelationPhase.Acquaintance;
        }

        private static void ClampEdge(D10RelationEdge edge)
        {
            edge.Affinity = Math.Max(-100, Math.Min(100, edge.Affinity));
            edge.Trust = Math.Max(0, Math.Min(100, edge.Trust));
            edge.Attachment = Math.Max(0, Math.Min(100, edge.Attachment));
            edge.Respect = Math.Max(0, Math.Min(100, edge.Respect));
            edge.Jealousy = Math.Max(0, Math.Min(100, edge.Jealousy));
            edge.ConflictScore = Math.Max(0, Math.Min(100, edge.ConflictScore));
            edge.Commitment = Math.Max(0, Math.Min(100, edge.Commitment));
        }

        private static int ToBoundedInt(float value, int minimum, int maximum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return minimum;
            return Math.Max(minimum, Math.Min(maximum, (int)Math.Round(value)));
        }

        private static D10RelationVisibility ToProjectionVisibility(RelationVisibility visibility)
        {
            switch (visibility)
            {
                case RelationVisibility.Public: return D10RelationVisibility.Public;
                case RelationVisibility.Rumored: return D10RelationVisibility.Rumored;
                case RelationVisibility.Hidden: return D10RelationVisibility.Hidden;
                default: return D10RelationVisibility.Private;
            }
        }

        private static void AddCause(D10RelationEdge relation, string eventId)
        {
            if (!relation.CauseEventIds.Contains(eventId)) relation.CauseEventIds.Add(eventId);
        }

        private void Upsert(D10RelationEdge relation)
        {
            Upsert(_snapshot, relation);
        }

        private static void Upsert(D10RelationRuntimeSnapshot snapshot, D10RelationEdge relation)
        {
            for (int i = 0; i < snapshot.Relations.Count; i++)
            {
                if (snapshot.Relations[i].Id != relation.Id) continue;
                snapshot.Relations[i] = relation.Clone();
                return;
            }
            snapshot.Relations.Add(relation.Clone());
        }

        private static WorldEvent BuildWorldEvent(D10RelationActionRequest request, string eventId,
            long saveRevision, D10RelationEdge before, D10RelationEdge after,
            ICollection<D10RelationEdge> changedRelations)
        {
            var causeIds = new List<string>();
            if (request.CauseEventIds != null)
                foreach (var cause in request.CauseEventIds)
                    if (!causeIds.Contains(cause)) causeIds.Add(cause);

            var factIds = new List<string>();
            var changedIds = new List<string>();
            if (changedRelations != null)
                foreach (var relation in changedRelations)
                {
                    factIds.Add(relation.Id);
                    changedIds.Add(relation.Id);
                }

            return new WorldEvent(
                eventId,
                Math.Max(0, request.WorldDay),
                "nearshore_saint_marys",
                D10RelationIds.OwnerSystem,
                new EventResult
                {
                    ResultCode = "relation_changed",
                    Summary = "关系事实已记录：" + request.Action + "。",
                    FactIds = factIds,
                    Values = new Dictionary<string, string>
                    {
                        ["action"] = request.Action.ToString(),
                        ["source_id"] = request.SourceId,
                        ["target_id"] = request.TargetId,
                        ["relation_id"] = after.Id,
                        ["phase_before"] = before.Phase.ToString(),
                        ["phase_after"] = after.Phase.ToString(),
                        ["romance_consent"] = after.RomanceConsent.ToString(),
                        ["affinity_before"] = before.Affinity.ToString(CultureInfo.InvariantCulture),
                        ["affinity_after"] = after.Affinity.ToString(CultureInfo.InvariantCulture),
                        ["trust_before"] = before.Trust.ToString(CultureInfo.InvariantCulture),
                        ["trust_after"] = after.Trust.ToString(CultureInfo.InvariantCulture),
                        ["changed_relation_ids"] = string.Join("|", changedIds.ToArray()),
                        ["context_ref"] = request.ContextRef ?? string.Empty,
                    },
                },
                BuildActors(request, changedRelations),
                new[] { after.Id, "action:" + request.Action },
                request.CauseEventIds,
                saveRevision);
        }

        private static IEnumerable<string> BuildActors(D10RelationActionRequest request,
            ICollection<D10RelationEdge> changedRelations)
        {
            var result = new List<string> { request.SourceId, request.TargetId };
            if (request.AffectedSourceIds != null)
                foreach (var sourceId in request.AffectedSourceIds)
                    if (!string.IsNullOrWhiteSpace(sourceId) && !result.Contains(sourceId)) result.Add(sourceId);
            return result;
        }

        private static string RejectionMessage(string code)
        {
            switch (code)
            {
                case "romance_prerequisite": return "亲密与信任条件尚未满足，不能直接进入恋爱承诺。";
                case "romance_route_declined": return "该关系路线已经被拒绝或结束，系统不会强制重启恋爱路线。";
                case "conflict_not_active": return "当前没有可供调解的冲突。";
                case "relation_required": return "该动作需要已经存在的关系边。";
                case "romance_relation_required": return "恋爱确认需要已经建立的关系边。";
                case "self_relation_forbidden": return "关系双方必须是不同角色。";
                case "participants_required": return "关系双方 ID 不能为空。";
                case "unknown_action": return "未知关系动作，未改变关系。";
                case "negative_time_or_revision": return "世界日和存档修订号不能为负数。";
                case "invalid_cause_event": return "因果事件 ID 不能为空。";
                default: return "关系动作未通过前置校验，未改变关系。";
            }
        }
    }
}
