// ============================================================
// D12DynamicQuestLogContracts —— 动态任务与航海日志契约
//
// QuestSystem 只拥有任务状态、目标投影、失败/恢复/替代路线引用和
// RewardRequest。金钱、库存、关系、图鉴和奖励结果仍由各自 owner 写入。
// 日志只保存已进入 EventLog 的事实引用，不复制事实载荷。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public enum D12DynamicQuestStatus
    {
        Available,
        Active,
        Failed,
        Recovered,
        Completed,
        Abandoned,
    }

    public enum D12DynamicQuestKind
    {
        Mainline,
        Side,
    }

    /// <summary>奖励类别仅用于 RewardSystem 路由，不携带金额、数量或物品结算。</summary>
    public enum D12QuestRewardType
    {
        MapKnowledge,
        Currency,
        Item,
        Reputation,
        Collection,
    }

    [Serializable]
    public sealed class D12QuestFactRef
    {
        public string EventId;
        public long SaveRevision;

        public D12QuestFactRef() { }

        public D12QuestFactRef(string eventId, long saveRevision)
        {
            EventId = eventId;
            SaveRevision = saveRevision;
        }

        public string Key
        {
            get { return (EventId ?? string.Empty) + "\u001f" + SaveRevision.ToString(CultureInfo.InvariantCulture); }
        }

        public D12QuestFactRef Clone()
        {
            return new D12QuestFactRef(EventId, SaveRevision);
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(EventId))
                throw new InvalidOperationException(field + ".event_id is required.");
            if (SaveRevision < 0)
                throw new InvalidOperationException(field + ".save_revision must be >= 0.");
        }
    }

    [Serializable]
    public sealed class D12QuestObjectiveAlternative
    {
        public string RouteId;
        public string TargetOwnerSystem;
        public string MatchValueKey;
        public string MatchValue;
        public List<string> AcceptedResultCodes = new List<string>();

        public D12QuestObjectiveAlternative Clone()
        {
            var result = new D12QuestObjectiveAlternative
            {
                RouteId = RouteId,
                TargetOwnerSystem = TargetOwnerSystem,
                MatchValueKey = MatchValueKey,
                MatchValue = MatchValue,
            };
            if (AcceptedResultCodes != null) result.AcceptedResultCodes.AddRange(AcceptedResultCodes);
            return result;
        }

        public void Validate(string objectiveId)
        {
            Require(RouteId, "RouteId", objectiveId);
            if (AcceptedResultCodes == null || AcceptedResultCodes.Count == 0)
                throw new InvalidOperationException("目标替代路线必须有结果码：" + objectiveId);
            if (!string.IsNullOrWhiteSpace(MatchValueKey) && string.IsNullOrWhiteSpace(MatchValue))
                throw new InvalidOperationException("目标替代路线指定 MatchValueKey 后必须有 MatchValue：" + objectiveId);
            foreach (var code in AcceptedResultCodes) Require(code, "AcceptedResultCode", objectiveId);
        }

        private static void Require(string value, string field, string objectiveId)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("任务目标 " + objectiveId + " 缺少 " + field + "。");
        }
    }

    [Serializable]
    public sealed class D12QuestObjectiveDefinition
    {
        public string ObjectiveId;
        public string Description;
        public string TargetOwnerSystem;
        public List<string> AcceptedResultCodes = new List<string>();
        public string MatchValueKey;
        public string MatchValue;
        public int RequiredCount = 1;
        public List<D12QuestObjectiveAlternative> AlternativeAcceptances =
            new List<D12QuestObjectiveAlternative>();

        public D12QuestObjectiveDefinition Clone()
        {
            var result = new D12QuestObjectiveDefinition
            {
                ObjectiveId = ObjectiveId,
                Description = Description,
                TargetOwnerSystem = TargetOwnerSystem,
                MatchValueKey = MatchValueKey,
                MatchValue = MatchValue,
                RequiredCount = RequiredCount,
            };
            if (AcceptedResultCodes != null) result.AcceptedResultCodes.AddRange(AcceptedResultCodes);
            if (AlternativeAcceptances != null)
                foreach (var alternative in AlternativeAcceptances)
                    if (alternative != null) result.AlternativeAcceptances.Add(alternative.Clone());
            return result;
        }

        public void Validate()
        {
            Require(ObjectiveId, "ObjectiveId");
            Require(Description, "Description");
            if (RequiredCount <= 0) throw new InvalidOperationException("任务目标数量必须大于零：" + ObjectiveId);
            if (AcceptedResultCodes == null || AcceptedResultCodes.Count == 0)
                throw new InvalidOperationException("任务目标必须有统一事件结果码：" + ObjectiveId);
            foreach (var code in AcceptedResultCodes) Require(code, "AcceptedResultCode");
            if (!string.IsNullOrWhiteSpace(MatchValueKey) && string.IsNullOrWhiteSpace(MatchValue))
                throw new InvalidOperationException("任务目标指定 MatchValueKey 后必须有 MatchValue：" + ObjectiveId);
            if (AlternativeAcceptances != null)
                foreach (var alternative in AlternativeAcceptances)
                {
                    if (alternative == null) throw new InvalidOperationException("任务目标替代条件不能为 null：" + ObjectiveId);
                    alternative.Validate(ObjectiveId);
                }
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D12QuestAlternativeRouteDefinition
    {
        public string RouteId;
        public string Action;
        public string Description;
        public string ResumeObjectiveId;

        public D12QuestAlternativeRouteDefinition Clone()
        {
            return new D12QuestAlternativeRouteDefinition
            {
                RouteId = RouteId,
                Action = Action,
                Description = Description,
                ResumeObjectiveId = ResumeObjectiveId,
            };
        }

        public void Validate(string questId)
        {
            Require(RouteId, "RouteId", questId);
            Require(Action, "Action", questId);
            Require(Description, "Description", questId);
            Require(ResumeObjectiveId, "ResumeObjectiveId", questId);
        }

        private static void Require(string value, string field, string questId)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("任务 " + questId + " 的替代路线缺少 " + field + "。");
        }
    }

    [Serializable]
    public sealed class D12DynamicQuestDefinition
    {
        public string QuestId;
        public string DisplayNameKey;
        public string Title;
        public string Description;
        public D12DynamicQuestKind Kind = D12DynamicQuestKind.Side;
        public string RewardId;
        public D12QuestRewardType RewardType = D12QuestRewardType.MapKnowledge;
        public List<string> FailureResultCodes = new List<string>();
        public List<string> RecoveryResultCodes = new List<string>();
        public string DefaultRecoveryRouteId;
        public List<D12QuestObjectiveDefinition> Objectives = new List<D12QuestObjectiveDefinition>();
        public List<D12QuestAlternativeRouteDefinition> AlternativeRoutes =
            new List<D12QuestAlternativeRouteDefinition>();

        public D12DynamicQuestDefinition Clone()
        {
            var result = new D12DynamicQuestDefinition
            {
                QuestId = QuestId,
                DisplayNameKey = DisplayNameKey,
                Title = Title,
                Description = Description,
                Kind = Kind,
                RewardId = RewardId,
                RewardType = RewardType,
                DefaultRecoveryRouteId = DefaultRecoveryRouteId,
            };
            if (FailureResultCodes != null) result.FailureResultCodes.AddRange(FailureResultCodes);
            if (RecoveryResultCodes != null) result.RecoveryResultCodes.AddRange(RecoveryResultCodes);
            if (Objectives != null)
                foreach (var objective in Objectives)
                    if (objective != null) result.Objectives.Add(objective.Clone());
            if (AlternativeRoutes != null)
                foreach (var route in AlternativeRoutes)
                    if (route != null) result.AlternativeRoutes.Add(route.Clone());
            return result;
        }

        public void Validate()
        {
            Require(QuestId, "QuestId");
            Require(DisplayNameKey, "DisplayNameKey");
            Require(Title, "Title");
            Require(Description, "Description");
            Require(RewardId, "RewardId");
            if (!Enum.IsDefined(typeof(D12DynamicQuestKind), Kind))
                throw new InvalidOperationException("任务类型非法：" + QuestId);
            if (!Enum.IsDefined(typeof(D12QuestRewardType), RewardType))
                throw new InvalidOperationException("任务奖励路由非法：" + QuestId);
            if (Objectives == null || Objectives.Count == 0)
                throw new InvalidOperationException("任务必须至少有一个目标：" + QuestId);

            var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var objective in Objectives)
            {
                if (objective == null) throw new InvalidOperationException("任务目标不能为 null：" + QuestId);
                objective.Validate();
                if (!objectiveIds.Add(objective.ObjectiveId))
                    throw new InvalidOperationException("任务目标重复：" + objective.ObjectiveId);
            }

            var routeIds = new HashSet<string>(StringComparer.Ordinal);
            if (AlternativeRoutes != null)
                foreach (var route in AlternativeRoutes)
                {
                    if (route == null) throw new InvalidOperationException("任务替代路线不能为 null：" + QuestId);
                    route.Validate(QuestId);
                    if (!routeIds.Add(route.RouteId)) throw new InvalidOperationException("任务替代路线重复：" + route.RouteId);
                    if (!objectiveIds.Contains(route.ResumeObjectiveId))
                        throw new InvalidOperationException("替代路线引用未知目标：" + route.RouteId);
                }

            if (!string.IsNullOrWhiteSpace(DefaultRecoveryRouteId) && !routeIds.Contains(DefaultRecoveryRouteId))
                throw new InvalidOperationException("默认恢复路线不存在：" + DefaultRecoveryRouteId);
            if (FailureResultCodes != null)
                foreach (var code in FailureResultCodes) Require(code, "FailureResultCode");
            if (RecoveryResultCodes != null)
                foreach (var code in RecoveryResultCodes) Require(code, "RecoveryResultCode");
            if (FailureResultCodes != null && FailureResultCodes.Count > 0 && string.IsNullOrWhiteSpace(DefaultRecoveryRouteId))
                throw new InvalidOperationException("有失败结果码的任务必须提供默认恢复路线：" + QuestId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D12QuestObjectiveState
    {
        public string ObjectiveId;
        public int Progress;
        public List<D12QuestFactRef> AppliedFacts = new List<D12QuestFactRef>();

        public D12QuestObjectiveState Clone()
        {
            var result = new D12QuestObjectiveState { ObjectiveId = ObjectiveId, Progress = Progress };
            if (AppliedFacts != null)
                foreach (var fact in AppliedFacts)
                    if (fact != null) result.AppliedFacts.Add(fact.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D12QuestRewardRequest
    {
        public string RequestId;
        public string RewardId;
        public string QuestId;
        public string SourceEventId;
        public long SourceSaveRevision;
        public D12QuestRewardType RewardType = D12QuestRewardType.MapKnowledge;
        public string OwnerSystem = "RewardSystem";
        public string Status = "pending";
        public List<string> CauseEventIds = new List<string>();

        public D12QuestRewardRequest Clone()
        {
            var result = new D12QuestRewardRequest
            {
                RequestId = RequestId,
                RewardId = RewardId,
                QuestId = QuestId,
                SourceEventId = SourceEventId,
                SourceSaveRevision = SourceSaveRevision,
                RewardType = RewardType,
                OwnerSystem = OwnerSystem,
                Status = Status,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate()
        {
            Require(RequestId, "RequestId");
            Require(RewardId, "RewardId");
            Require(QuestId, "QuestId");
            Require(SourceEventId, "SourceEventId");
            Require(OwnerSystem, "OwnerSystem");
            if (SourceSaveRevision < 0) throw new InvalidOperationException("RewardRequest source revision cannot be negative.");
            if (!Enum.IsDefined(typeof(D12QuestRewardType), RewardType)) throw new InvalidOperationException("RewardRequest type invalid.");
            if (CauseEventIds == null) throw new InvalidOperationException("RewardRequest cause ids are required.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("RewardRequest " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D12DynamicQuestState
    {
        public string QuestId;
        public D12DynamicQuestStatus Status = D12DynamicQuestStatus.Available;
        public int AcceptedWorldDay = -1;
        public int LastUpdatedWorldDay;
        public string LastFailureCode = string.Empty;
        public D12QuestFactRef LastFailureFact;
        public D12QuestFactRef LastRecoveryFact;
        public string RecoveryRouteId;
        public string AlternativeRouteId;
        public D12QuestFactRef CompletedFact;
        public List<D12QuestObjectiveState> Objectives = new List<D12QuestObjectiveState>();
        public List<D12QuestFactRef> FactEvents = new List<D12QuestFactRef>();
        public D12QuestRewardRequest RewardRequest;

        public D12DynamicQuestState Clone()
        {
            var result = new D12DynamicQuestState
            {
                QuestId = QuestId,
                Status = Status,
                AcceptedWorldDay = AcceptedWorldDay,
                LastUpdatedWorldDay = LastUpdatedWorldDay,
                LastFailureCode = LastFailureCode,
                LastFailureFact = LastFailureFact == null ? null : LastFailureFact.Clone(),
                LastRecoveryFact = LastRecoveryFact == null ? null : LastRecoveryFact.Clone(),
                RecoveryRouteId = RecoveryRouteId,
                AlternativeRouteId = AlternativeRouteId,
                CompletedFact = CompletedFact == null ? null : CompletedFact.Clone(),
                RewardRequest = RewardRequest == null ? null : RewardRequest.Clone(),
            };
            if (Objectives != null)
                foreach (var objective in Objectives)
                    if (objective != null) result.Objectives.Add(objective.Clone());
            if (FactEvents != null)
                foreach (var fact in FactEvents)
                    if (fact != null) result.FactEvents.Add(fact.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D12DynamicQuestLogSnapshot
    {
        public const int CurrentSchemaVersion = 1;
        public int SchemaVersion = CurrentSchemaVersion;
        public List<D12DynamicQuestState> Quests = new List<D12DynamicQuestState>();
        public List<D12QuestFactRef> JournalFacts = new List<D12QuestFactRef>();

        public D12DynamicQuestLogSnapshot Clone()
        {
            var result = new D12DynamicQuestLogSnapshot { SchemaVersion = SchemaVersion };
            if (Quests != null)
                foreach (var quest in Quests)
                    if (quest != null) result.Quests.Add(quest.Clone());
            if (JournalFacts != null)
                foreach (var fact in JournalFacts)
                    if (fact != null) result.JournalFacts.Add(fact.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 dynamic quest log schema_version mismatch.");
            var questIds = new HashSet<string>(StringComparer.Ordinal);
            if (Quests != null)
                foreach (var quest in Quests)
                {
                    if (quest == null || string.IsNullOrWhiteSpace(quest.QuestId))
                        throw new InvalidOperationException("D12 dynamic quest snapshot contains an invalid quest.");
                    if (!questIds.Add(quest.QuestId)) throw new InvalidOperationException("D12 dynamic quest duplicated: " + quest.QuestId);
                    if (!Enum.IsDefined(typeof(D12DynamicQuestStatus), quest.Status))
                        throw new InvalidOperationException("D12 dynamic quest status invalid: " + quest.QuestId);
                    if (quest.AcceptedWorldDay < -1 || quest.LastUpdatedWorldDay < 0)
                        throw new InvalidOperationException("D12 dynamic quest day invalid: " + quest.QuestId);
                    ValidateFact(quest.LastFailureFact, "last_failure_fact");
                    ValidateFact(quest.LastRecoveryFact, "last_recovery_fact");
                    ValidateFact(quest.CompletedFact, "completed_fact");
                    if (quest.Objectives == null) throw new InvalidOperationException("D12 dynamic quest objectives missing: " + quest.QuestId);
                    var objectiveIds = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var objective in quest.Objectives)
                    {
                        if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                            throw new InvalidOperationException("D12 dynamic quest objective invalid: " + quest.QuestId);
                        if (!objectiveIds.Add(objective.ObjectiveId)) throw new InvalidOperationException("D12 dynamic quest objective duplicated: " + objective.ObjectiveId);
                        if (objective.Progress < 0) throw new InvalidOperationException("D12 dynamic quest objective progress invalid: " + objective.ObjectiveId);
                        if (objective.AppliedFacts == null) throw new InvalidOperationException("D12 dynamic quest objective facts missing: " + objective.ObjectiveId);
                        foreach (var fact in objective.AppliedFacts) ValidateFact(fact, "objective_fact");
                    }
                    if (quest.FactEvents == null) throw new InvalidOperationException("D12 dynamic quest facts missing: " + quest.QuestId);
                    foreach (var fact in quest.FactEvents) ValidateFact(fact, "quest_fact");
                    if (quest.RewardRequest != null) quest.RewardRequest.Validate();
                }
            if (JournalFacts == null) throw new InvalidOperationException("D12 dynamic journal facts missing.");
            foreach (var fact in JournalFacts) ValidateFact(fact, "journal_fact");
        }

        private static void ValidateFact(D12QuestFactRef fact, string field)
        {
            if (fact != null) fact.Validate(field);
        }
    }

    [Serializable]
    public sealed class D12QuestAcceptRequest
    {
        public string QuestId;
        public string ActorRef;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12QuestFactObservationRequest
    {
        public string QuestId;
        public string FactEventId;
        public long FactSaveRevision;
        public string ActorRef;
    }

    [Serializable]
    public sealed class D12QuestAlternativeRouteRequest
    {
        public string QuestId;
        public string RouteId;
        public string ActorRef;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12QuestJournalEntry
    {
        public string EventId;
        public long SaveRevision;
        public int WorldDay;
        public string LocationRef;
        public string OwnerSystem;
        public WorldEventStatus OutcomeStatus;
        public string ResultCode;
        public string Summary;
        public List<string> CauseEventIds = new List<string>();

        public D12QuestJournalEntry Clone()
        {
            var result = new D12QuestJournalEntry
            {
                EventId = EventId,
                SaveRevision = SaveRevision,
                WorldDay = WorldDay,
                LocationRef = LocationRef,
                OwnerSystem = OwnerSystem,
                OutcomeStatus = OutcomeStatus,
                ResultCode = ResultCode,
                Summary = Summary,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12QuestResult
    {
        public bool Accepted;
        public bool Applied;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string QuestId;
        public D12DynamicQuestStatus Status;
        public FactEvent Fact;
        public D12QuestRewardRequest RewardRequest;
        public D12QuestAlternativeRouteDefinition AlternativeRoute;
        public List<string> MatchedObjectiveIds = new List<string>();

        public D12QuestResult Clone()
        {
            var result = new D12QuestResult
            {
                Accepted = Accepted,
                Applied = Applied,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                QuestId = QuestId,
                Status = Status,
                Fact = Fact == null ? null : Fact.CloneFact(),
                RewardRequest = RewardRequest == null ? null : RewardRequest.Clone(),
                AlternativeRoute = AlternativeRoute == null ? null : AlternativeRoute.Clone(),
            };
            if (MatchedObjectiveIds != null) result.MatchedObjectiveIds.AddRange(MatchedObjectiveIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12QuestAgentSummary
    {
        public string SummaryText;
        public bool FactsOnly = true;
        public bool CanCreateFacts;
        public List<D12QuestFactRef> FactRefs = new List<D12QuestFactRef>();

        public D12QuestAgentSummary Clone()
        {
            var result = new D12QuestAgentSummary
            {
                SummaryText = SummaryText,
                FactsOnly = FactsOnly,
                CanCreateFacts = CanCreateFacts,
            };
            if (FactRefs != null)
                foreach (var fact in FactRefs)
                    if (fact != null) result.FactRefs.Add(fact.Clone());
            return result;
        }
    }
}
