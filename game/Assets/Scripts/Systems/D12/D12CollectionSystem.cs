// ============================================================
// D12CollectionAtlasSystem —— 统一收藏/图鉴唯一写者
//
// 该系统只写 D12CollectionAtlasState。它不创建动物、植物、鱼获、船、
// 外观、地图或任务的领域结果，也不发放经济奖励；所有输入都必须指向
// EventLog 中已存在的 owner 事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12CollectionAtlasSystem
    {
        public const string OwnerSystem = "CollectionAtlasSystem";
        private readonly EventLog _eventLog;
        private readonly Dictionary<string, D12CollectionEntryDefinition> _definitions =
            new Dictionary<string, D12CollectionEntryDefinition>(StringComparer.Ordinal);
        private D12CollectionAtlasState _state;

        public D12CollectionAtlasSystem(IEnumerable<D12CollectionEntryDefinition> definitions = null,
            EventLog eventLog = null, D12CollectionAtlasState initialState = null)
        {
            _eventLog = eventLog ?? new EventLog();
            var sourceDefinitions = definitions ?? D12CollectionCatalog.CreateVerticalSlice();
            foreach (var definition in sourceDefinitions)
            {
                if (definition == null) throw new InvalidOperationException("D12 图鉴定义不能为 null。");
                definition.Validate();
                if (!_definitions.ContainsKey(definition.ContentId))
                    _definitions.Add(definition.ContentId, definition.Clone());
                else
                    throw new InvalidOperationException("D12 图鉴 content_id 重复：" + definition.ContentId);
            }
            _state = initialState == null ? new D12CollectionAtlasState() : initialState.Clone();
            _state.Validate();
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D12CollectionAtlasState Snapshot
        {
            get { return _state.Clone(); }
        }

        public IReadOnlyList<D12CollectionEntryDefinition> Definitions
        {
            get { return new List<D12CollectionEntryDefinition>(_definitions.Values); }
        }

        public D12CollectionRecord FindRecord(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId)) return null;
            foreach (var record in _state.Records)
                if (record != null && record.ContentId == contentId) return record.Clone();
            return null;
        }

        public bool HasCollected(string contentId)
        {
            var record = FindRecord(contentId);
            return record != null && record.Unlocked;
        }

        /// <summary>
        /// 记录一个由所属领域系统已经裁决的收藏事实。EvidenceRef 只是引用，
        /// 没有 source fact 时即使文本完整也不能通过。
        /// </summary>
        public D12CollectionResult Record(D12CollectionRecordRequest request)
        {
            if (request == null) return Reject("request_missing", "图鉴请求不能为空。");
            D12CollectionEntryDefinition definition;
            if (!_definitions.TryGetValue(request.ContentId ?? string.Empty, out definition))
                return Reject("unknown_content_id", "图鉴 content_id 未注册：" + request.ContentId);
            if (definition.Category != request.Category)
                return Reject("category_mismatch", "图鉴类别与 content_id 不匹配：" + request.ContentId);
            if (string.IsNullOrWhiteSpace(request.FactEventId)) return Reject("fact_event_missing", "图鉴必须引用事实事件。");
            if (request.FactSaveRevision < 0 || request.WorldDay < 0) return Reject("invalid_time", "图鉴事实时间或版本非法。");
            if (string.IsNullOrWhiteSpace(request.EvidenceRef)) return Reject("evidence_ref_missing", "图鉴必须引用已有证据。");
            if (string.IsNullOrWhiteSpace(request.LocationRef)) return Reject("location_ref_missing", "图鉴必须有地点。");
            if (!Enum.IsDefined(typeof(D12CollectionAcquisitionMethod), request.AcquisitionMethod)) return Reject("invalid_acquisition_method", "图鉴获得方式非法。");
            if (!definition.AllowedAcquisitionMethods.Contains(request.AcquisitionMethod)) return Reject("acquisition_not_allowed", "该内容不允许使用此获得方式：" + request.ContentId);

            var sourceFact = _eventLog.Find(request.FactEventId, request.FactSaveRevision);
            if (sourceFact == null) return Reject("fact_event_missing", "来源事实不在共享 EventLog 中：" + request.FactEventId);
            if (sourceFact.WorldDay != request.WorldDay || sourceFact.LocationRef != request.LocationRef)
                return Reject("fact_context_mismatch", "图鉴地点和时间必须与来源事实一致。");

            FactEvent authorizationFact = null;
            if (definition.RequiresAuthorization)
            {
                if (request.AuthorizationRef != definition.RequiredAuthorizationRef)
                    return Reject("authorization_required", "该收藏需要有效授权引用：" + definition.RequiredAuthorizationRef);
                if (string.IsNullOrWhiteSpace(request.AuthorizationFactEventId))
                    return Reject("authorization_fact_missing", "该收藏需要共享 EventLog 中的授权事实。");
                if (request.AuthorizationSaveRevision < 0)
                    return Reject("authorization_revision_invalid", "授权事实版本非法。");
                authorizationFact = _eventLog.Find(request.AuthorizationFactEventId, request.AuthorizationSaveRevision);
                if (authorizationFact == null)
                    return Reject("authorization_fact_missing", "授权事实不在共享 EventLog 中：" + request.AuthorizationFactEventId);
            }

            string evidenceId = "evidence_d12_" + request.ContentId + "_" + StableToken(
                request.FactEventId + "|" + request.FactSaveRevision.ToString(CultureInfo.InvariantCulture) + "|" + request.EvidenceRef);
            var existingEvidence = FindEvidence(evidenceId);
            if (existingEvidence != null)
            {
                var existingRecord = FindRecord(request.ContentId);
                return new D12CollectionResult
                {
                    Accepted = true,
                    Duplicate = true,
                    AchievementUnlocked = false,
                    Code = "duplicate_evidence",
                    Message = "该事实已经进入图鉴，未重复解锁成就。",
                    Record = existingRecord,
                    Evidence = existingEvidence,
                    Achievement = FindAchievement(definition.AchievementId),
                    Fact = _eventLog.Find(existingEvidence.RecordedEventId, existingEvidence.RecordedSaveRevision),
                };
            }

            string recordEventId = "event_d12_collection_record_" + request.ContentId + "_" + StableToken(
                request.FactEventId + "|" + request.FactSaveRevision.ToString(CultureInfo.InvariantCulture));
            var recordEvent = new WorldEvent(
                recordEventId,
                request.WorldDay,
                request.LocationRef,
                OwnerSystem,
                new EventResult
                {
                    ResultCode = "collection_recorded",
                    Summary = "图鉴记录已由来源事实确认：" + request.ContentId,
                    FactIds = new List<string> { "content:" + request.ContentId, "evidence:" + evidenceId },
                    Values = new Dictionary<string, string> { ["acquisition_method"] = request.AcquisitionMethod.ToString() },
                },
                new[] { string.IsNullOrWhiteSpace(request.ActorRef) ? "player" : request.ActorRef },
                new[] { request.FactEventId },
                authorizationFact == null ? new[] { request.FactEventId } : new[] { request.FactEventId, request.AuthorizationFactEventId },
                request.SaveRevisionForEvent());
            var recordFactResult = _eventLog.Append(recordEvent);
            var recordedFact = recordFactResult.Fact;
            if (recordedFact == null) return Reject("record_event_failed", "图鉴记录事实写入失败。");

            var evidence = new D12CollectionEvidenceRecord
            {
                EvidenceId = evidenceId,
                ContentId = request.ContentId,
                Category = request.Category,
                FactEventId = request.FactEventId,
                FactSaveRevision = request.FactSaveRevision,
                SourceRef = definition.SourceRef,
                EvidenceRef = request.EvidenceRef,
                LocationRef = request.LocationRef,
                WorldDay = request.WorldDay,
                AcquisitionMethod = request.AcquisitionMethod,
                AuthorizationRef = request.AuthorizationRef,
                AuthorizationFactEventId = request.AuthorizationFactEventId,
                AuthorizationSaveRevision = request.AuthorizationSaveRevision,
                RecordedEventId = recordedFact.EventId,
                RecordedSaveRevision = recordedFact.SaveRevision,
            };
            evidence.Validate();
            _state.Evidence.Add(evidence);
            var record = FindRecord(request.ContentId);
            if (record == null)
            {
                record = new D12CollectionRecord
                {
                    CollectionId = "collection_" + request.ContentId,
                    ContentId = request.ContentId,
                    Category = request.Category,
                    Unlocked = true,
                    FirstEvidenceId = evidence.EvidenceId,
                    FirstLocationRef = evidence.LocationRef,
                    FirstWorldDay = evidence.WorldDay,
                    FirstAcquisitionMethod = evidence.AcquisitionMethod,
                    AchievementId = definition.AchievementId,
                    AchievementUnlocked = false,
                    Revision = ++_state.Revision,
                };
                record.EvidenceIds.Add(evidence.EvidenceId);
                _state.Records.Add(record);
            }
            else
            {
                record.EvidenceIds.Add(evidence.EvidenceId);
                record.Revision = ++_state.Revision;
                ReplaceRecord(record);
            }

            D12CollectionAchievement achievement = FindAchievement(definition.AchievementId);
            FactEvent achievementFact = null;
            bool achievementUnlocked = false;
            if (achievement == null)
            {
                string achievementEventId = "event_d12_achievement_unlocked_" + request.ContentId;
                var achievementEvent = new WorldEvent(
                    achievementEventId,
                    request.WorldDay,
                    request.LocationRef,
                    OwnerSystem,
                    new EventResult
                    {
                        ResultCode = "collection_achievement_unlocked",
                        Summary = "图鉴成就由收藏记录首次解锁：" + definition.AchievementId,
                        FactIds = new List<string> { "achievement:" + definition.AchievementId, "evidence:" + evidence.EvidenceId },
                    },
                    new[] { string.IsNullOrWhiteSpace(request.ActorRef) ? "player" : request.ActorRef },
                    new[] { recordedFact.EventId },
                    new[] { recordedFact.EventId },
                    request.SaveRevisionForEvent());
                achievementFact = _eventLog.Append(achievementEvent).Fact;
                if (achievementFact == null) return Reject("achievement_event_failed", "图鉴成就事实写入失败。");
                achievement = new D12CollectionAchievement
                {
                    AchievementId = definition.AchievementId,
                    ContentId = request.ContentId,
                    Unlocked = true,
                    EvidenceId = evidence.EvidenceId,
                    EventId = achievementFact.EventId,
                    WorldDay = request.WorldDay,
                    SaveRevision = achievementFact.SaveRevision,
                };
                _state.Achievements.Add(achievement);
                record.AchievementUnlocked = true;
                record.AchievementEventId = achievementFact.EventId;
                ReplaceRecord(record);
                achievementUnlocked = true;
            }

            _state.AppliedEventKeys.Add(recordedFact.DedupeKey);
            if (achievementFact != null) _state.AppliedEventKeys.Add(achievementFact.DedupeKey);
            _state.Validate();
            return new D12CollectionResult
            {
                Accepted = true,
                Duplicate = false,
                AchievementUnlocked = achievementUnlocked,
                Code = achievementUnlocked ? "collection_recorded_and_unlocked" : "collection_recorded",
                Message = achievementUnlocked ? "图鉴已记录并首次解锁成就。" : "图鉴已追加证据，成就保持幂等。",
                Record = record.Clone(),
                Evidence = evidence.Clone(),
                Achievement = achievement.Clone(),
                Fact = recordedFact.CloneFact(),
                AchievementFact = achievementFact == null ? null : achievementFact.CloneFact(),
            };
        }

        public void Restore(D12CollectionAtlasState snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("D12 图鉴快照不能为空。");
            snapshot.Validate();
            _state = snapshot.Clone();
        }

        private static D12CollectionResult Reject(string code, string message)
        {
            return new D12CollectionResult { Accepted = false, Code = code, Message = message };
        }

        private D12CollectionEvidenceRecord FindEvidence(string evidenceId)
        {
            foreach (var item in _state.Evidence)
                if (item != null && item.EvidenceId == evidenceId) return item.Clone();
            return null;
        }

        private D12CollectionAchievement FindAchievement(string achievementId)
        {
            foreach (var item in _state.Achievements)
                if (item != null && item.AchievementId == achievementId) return item.Clone();
            return null;
        }

        private void ReplaceRecord(D12CollectionRecord replacement)
        {
            for (int i = 0; i < _state.Records.Count; i++)
                if (_state.Records[i] != null && _state.Records[i].CollectionId == replacement.CollectionId)
                {
                    _state.Records[i] = replacement;
                    return;
                }
        }

        private static string StableToken(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char ch in value ?? string.Empty)
                {
                    hash ^= ch;
                    hash *= 16777619u;
                }
                return hash.ToString("x8", CultureInfo.InvariantCulture);
            }
        }
    }

    internal static class D12CollectionRequestExtensions
    {
        public static long SaveRevisionForEvent(this D12CollectionRecordRequest request)
        {
            return request == null ? 0L : Math.Max(0L, request.FactSaveRevision);
        }
    }
}
