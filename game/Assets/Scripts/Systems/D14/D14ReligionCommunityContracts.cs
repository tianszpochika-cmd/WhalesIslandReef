// D14-A06 宗教、社群礼仪和历史审查契约。
// ReligionCommunitySystem 只写自己的仪式/誓言/审查投影；D10、D12、Building、Community
// authority 仍是关系、社群信任、建筑开放和葬礼结果的唯一写者。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14ReligionCommunityIds
    {
        public const int SchemaVersion = 1;
        public const string OwnerSystem = "ReligionCommunitySystem";
        public const string HistoricalReviewOwner = "HistoricalReviewSystem";
        public const string D10RelationOwner = "RelationSystem";
        public const string D12CommunityOwner = "CommunityTradeSystem";
        public const string BuildingOwner = "BuildingSystem";
        public const string EventPrefix = "event_d14_religion_community_";
        public const string SensitiveRecoveryRoute = "route_d14_sensitive_content_review";
        public const string HistoricalSourceRef = "docs/game/design/common/D14_RELIGION_HISTORY_REVIEW.md";
    }

    public enum D14PerformanceLevel { Full, Contextual, LowIntensity, Omitted }
    public enum D14ReligionContentType { Festival, DietaryRule, BuildingRite, FuneralRite, Oath, Etiquette }
    public enum D14OathStatus { Sworn, Fulfilled, Broken, Released }
    public enum D14OathAction { Swear, Fulfill, Break, Release }
    public enum D14TrustDomain { CrewRelation, Community }

    [Serializable]
    public sealed class D14HistoricalContentDefinition
    {
        public string ContentId;
        public D14ReligionContentType ContentType;
        public string DisplayNameKey;
        public string HistoricalTag = "inferred";
        public string SensitiveLabel;
        public string ContentWarning;
        public D14SensitiveReviewStatus ReviewStatus = D14SensitiveReviewStatus.ApprovedWithWarning;
        public string SourceRef = D14ReligionCommunityIds.HistoricalSourceRef;
        public string SourceNote;
        public bool SkipAllowed = true;
        public List<D14PerformanceLevel> AllowedPerformanceLevels = new List<D14PerformanceLevel>
        { D14PerformanceLevel.Full, D14PerformanceLevel.Contextual, D14PerformanceLevel.LowIntensity };

        public D14HistoricalContentDefinition Clone()
        {
            var result = (D14HistoricalContentDefinition)MemberwiseClone();
            result.AllowedPerformanceLevels = AllowedPerformanceLevels == null ? new List<D14PerformanceLevel>() : new List<D14PerformanceLevel>(AllowedPerformanceLevels);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(DisplayNameKey) || string.IsNullOrWhiteSpace(HistoricalTag) ||
                string.IsNullOrWhiteSpace(SensitiveLabel) || string.IsNullOrWhiteSpace(ContentWarning) || string.IsNullOrWhiteSpace(SourceRef) || string.IsNullOrWhiteSpace(SourceNote) ||
                !Enum.IsDefined(typeof(D14ReligionContentType), ContentType) || !Enum.IsDefined(typeof(D14SensitiveReviewStatus), ReviewStatus) || AllowedPerformanceLevels == null || AllowedPerformanceLevels.Count == 0)
                throw new InvalidOperationException(field + " 缺少历史审查字段。");
            if (ReviewStatus == D14SensitiveReviewStatus.Pending || ReviewStatus == D14SensitiveReviewStatus.Rejected)
                throw new InvalidOperationException(field + " 未完成审查，不得进入运行时。");
            if (ReviewStatus == D14SensitiveReviewStatus.ApprovedWithWarning && !SkipAllowed)
                throw new InvalidOperationException(field + " approved_with_warning 必须允许跳过。");
            foreach (var level in AllowedPerformanceLevels)
                if (!Enum.IsDefined(typeof(D14PerformanceLevel), level) || level == D14PerformanceLevel.Omitted)
                    throw new InvalidOperationException(field + " 表现等级非法。");
        }
    }

    [Serializable]
    public sealed class D14ReligionDefinition
    {
        public string ReligionId;
        public string DisplayNameKey;
        public string CulturalContext;
        public string HistoricalTag = "inferred";
        public D14SensitiveReviewStatus ReviewStatus = D14SensitiveReviewStatus.ApprovedWithWarning;
        public string SourceRef = D14ReligionCommunityIds.HistoricalSourceRef;
        public List<string> FestivalIds = new List<string>();
        public List<string> DietaryRuleIds = new List<string>();
        public List<string> ContentIds = new List<string>();

        public D14ReligionDefinition Clone()
        {
            var result = (D14ReligionDefinition)MemberwiseClone();
            result.FestivalIds = FestivalIds == null ? new List<string>() : new List<string>(FestivalIds);
            result.DietaryRuleIds = DietaryRuleIds == null ? new List<string>() : new List<string>(DietaryRuleIds);
            result.ContentIds = ContentIds == null ? new List<string>() : new List<string>(ContentIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ReligionId) || string.IsNullOrWhiteSpace(DisplayNameKey) || string.IsNullOrWhiteSpace(CulturalContext) || string.IsNullOrWhiteSpace(SourceRef) ||
                !Enum.IsDefined(typeof(D14SensitiveReviewStatus), ReviewStatus) || ReviewStatus == D14SensitiveReviewStatus.Pending || ReviewStatus == D14SensitiveReviewStatus.Rejected)
                throw new InvalidOperationException(field + " 缺少宗教语境或审查状态。");
        }
    }

    [Serializable]
    public sealed class D14FestivalDefinition
    {
        public string FestivalId;
        public string ReligionId;
        public string ContentId;
        public int CycleDays = 30;
        public int StartDay;
        public int DurationDays = 3;
        public string BuildingRiteId;
        public string EtiquetteId;
        public string DietaryRuleId;

        public D14FestivalDefinition Clone() { return (D14FestivalDefinition)MemberwiseClone(); }
        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(FestivalId) || string.IsNullOrWhiteSpace(ReligionId) || string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(BuildingRiteId) || string.IsNullOrWhiteSpace(EtiquetteId) ||
                CycleDays <= 0 || StartDay < 0 || StartDay >= CycleDays || DurationDays <= 0 || DurationDays > CycleDays)
                throw new InvalidOperationException(field + " 节期定义非法。");
        }
    }

    [Serializable]
    public sealed class D14DietaryRuleDefinition
    {
        public string RuleId;
        public string ReligionId;
        public string ContentId;
        public string FoodTag;
        public int ComplianceDelta = 10;

        public D14DietaryRuleDefinition Clone() { return (D14DietaryRuleDefinition)MemberwiseClone(); }
        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RuleId) || string.IsNullOrWhiteSpace(ReligionId) || string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(FoodTag) || ComplianceDelta <= 0 || ComplianceDelta > 100)
                throw new InvalidOperationException(field + " 饮食规则非法。");
        }
    }

    [Serializable]
    public sealed class D14EtiquetteDefinition
    {
        public string EtiquetteId;
        public string CommunityId;
        public string ContentId;
        public string RequiredFestivalId;
        public string RequiredDietaryRuleId;
        public string EtiquetteTag;

        public D14EtiquetteDefinition Clone() { return (D14EtiquetteDefinition)MemberwiseClone(); }
        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(EtiquetteId) || string.IsNullOrWhiteSpace(CommunityId) || string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(EtiquetteTag))
                throw new InvalidOperationException(field + " 社群礼仪定义非法。");
        }
    }

    [Serializable]
    public sealed class D14BuildingRiteDefinition
    {
        public string RiteId;
        public string BuildingId;
        public string ReligionId;
        public string ContentId;
        public int OpenHour;
        public int CloseHour;

        public D14BuildingRiteDefinition Clone() { return (D14BuildingRiteDefinition)MemberwiseClone(); }
        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RiteId) || string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(ReligionId) || string.IsNullOrWhiteSpace(ContentId) || OpenHour < 0 || OpenHour > 23 || CloseHour <= OpenHour || CloseHour > 24)
                throw new InvalidOperationException(field + " 建筑开放规则非法。");
        }
    }

    [Serializable]
    public sealed class D14ReligionPracticeState
    {
        public string ActorRef;
        public string ReligionId;
        public int FestivalParticipationCount;
        public int DietaryComplianceScore;
        public string LastFestivalId;
        public string LastDietaryRuleId;
        public int LastWorldDay;
        public long Revision;
        public string LastEventId;
        public List<string> AppliedEventKeys = new List<string>();

        public D14ReligionPracticeState Clone()
        {
            var result = (D14ReligionPracticeState)MemberwiseClone();
            result.AppliedEventKeys = AppliedEventKeys == null ? new List<string>() : new List<string>(AppliedEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class D14ContentReviewRecord
    {
        public string ActionId;
        public string ContentId;
        public D14SensitiveReviewStatus ReviewStatus;
        public D14PerformanceLevel PerformanceLevel;
        public bool WarningAcknowledged;
        public bool Skipped;
        public string HistoricalEvidenceEventId;
        public int WorldDay;
        public string LastEventId;

        public D14ContentReviewRecord Clone() { return (D14ContentReviewRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14FestivalRecord
    {
        public string ActionId;
        public string ActorRef;
        public string FestivalId;
        public string ReligionId;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14FestivalRecord Clone() { return (D14FestivalRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14DietaryRecord
    {
        public string ActionId;
        public string ActorRef;
        public string RuleId;
        public string FoodTag;
        public string ReligionId;
        public bool Compliant;
        public int ComplianceAfter;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14DietaryRecord Clone() { return (D14DietaryRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14BuildingAccessRecord
    {
        public string ActionId;
        public string ActorRef;
        public string RiteId;
        public string BuildingId;
        public bool Allowed;
        public string BuildingStatus;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string BuildingSourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14BuildingAccessRecord Clone() { return (D14BuildingAccessRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14FuneralRiteRecord
    {
        public string ActionId;
        public string FuneralId;
        public string RiteId;
        public string Status;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string CommunitySourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14FuneralRiteRecord Clone() { return (D14FuneralRiteRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14OathRecord
    {
        public string ActionId;
        public string OathId;
        public string ActorRef;
        public string TargetRef;
        public string ReligionId;
        public string CommunityId;
        public D14OathStatus Status;
        public D14OathAction Action;
        public D14TrustDomain TrustDomain;
        public int ProposedTrustDelta;
        public int AppliedTrustDelta;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string TrustSourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14OathRecord Clone() { return (D14OathRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14CommunityInteractionRecord
    {
        public string ActionId;
        public string ActorRef;
        public string CommunityId;
        public string EtiquetteId;
        public string FestivalId;
        public string DietaryRuleId;
        public bool Allowed;
        public string OutcomeCode;
        public int ProposedTrustDelta;
        public int AppliedTrustDelta;
        public int TrustAfter;
        public D14PerformanceLevel PerformanceLevel;
        public bool Skipped;
        public int WorldDay;
        public string SourceEventId;
        public string CommunitySourceEventId;
        public string HistoricalEvidenceEventId;
        public string LastEventId;
        public D14CommunityInteractionRecord Clone() { return (D14CommunityInteractionRecord)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D14ReligionCommunityRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14ReligionCommunityIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public List<D14ReligionPracticeState> ReligionStates = new List<D14ReligionPracticeState>();
        public List<D14ContentReviewRecord> Reviews = new List<D14ContentReviewRecord>();
        public List<D14FestivalRecord> Festivals = new List<D14FestivalRecord>();
        public List<D14DietaryRecord> DietaryRecords = new List<D14DietaryRecord>();
        public List<D14BuildingAccessRecord> BuildingAccess = new List<D14BuildingAccessRecord>();
        public List<D14FuneralRiteRecord> FuneralRites = new List<D14FuneralRiteRecord>();
        public List<D14OathRecord> Oaths = new List<D14OathRecord>();
        public List<D14CommunityInteractionRecord> CommunityInteractions = new List<D14CommunityInteractionRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14ReligionCommunityRuntimeSnapshot Clone()
        {
            var result = new D14ReligionCommunityRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId, EventLog = EventLog == null ? null : EventLog.Clone() };
            if (ReligionStates != null) foreach (var item in ReligionStates) if (item != null) result.ReligionStates.Add(item.Clone());
            if (Reviews != null) foreach (var item in Reviews) if (item != null) result.Reviews.Add(item.Clone());
            if (Festivals != null) foreach (var item in Festivals) if (item != null) result.Festivals.Add(item.Clone());
            if (DietaryRecords != null) foreach (var item in DietaryRecords) if (item != null) result.DietaryRecords.Add(item.Clone());
            if (BuildingAccess != null) foreach (var item in BuildingAccess) if (item != null) result.BuildingAccess.Add(item.Clone());
            if (FuneralRites != null) foreach (var item in FuneralRites) if (item != null) result.FuneralRites.Add(item.Clone());
            if (Oaths != null) foreach (var item in Oaths) if (item != null) result.Oaths.Add(item.Clone());
            if (CommunityInteractions != null) foreach (var item in CommunityInteractions) if (item != null) result.CommunityInteractions.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0 || ReligionStates == null || Reviews == null || Festivals == null || DietaryRecords == null || BuildingAccess == null || FuneralRites == null || Oaths == null || CommunityInteractions == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14 religion/community snapshot 无效。");
            ValidateIds(ReligionStates, "religion_state", delegate(D14ReligionPracticeState value) { return value.ActorRef + "\u001f" + value.ReligionId; });
            ValidateIds(Reviews, "review", delegate(D14ContentReviewRecord value) { return value.ActionId; });
            ValidateIds(Festivals, "festival", delegate(D14FestivalRecord value) { return value.ActionId; });
            ValidateIds(DietaryRecords, "dietary", delegate(D14DietaryRecord value) { return value.ActionId; });
            ValidateIds(BuildingAccess, "building", delegate(D14BuildingAccessRecord value) { return value.ActionId; });
            ValidateIds(FuneralRites, "funeral", delegate(D14FuneralRiteRecord value) { return value.ActionId; });
            ValidateIds(Oaths, "oath", delegate(D14OathRecord value) { return value.ActionId; });
            ValidateIds(CommunityInteractions, "community_interaction", delegate(D14CommunityInteractionRecord value) { return value.ActionId; });
            foreach (var state in ReligionStates)
            {
                if (state == null || string.IsNullOrWhiteSpace(state.ActorRef) || string.IsNullOrWhiteSpace(state.ReligionId) || state.FestivalParticipationCount < 0 || state.DietaryComplianceScore < 0 || state.DietaryComplianceScore > 100 || state.LastWorldDay < 0 || state.Revision < 0 || state.AppliedEventKeys == null)
                    throw new InvalidOperationException("D14 religion state 无效。");
            }
        }

        private static void ValidateIds<T>(IEnumerable<T> values, string label, Func<T, string> key)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null) throw new InvalidOperationException("D14 " + label + " 不能为 null。");
                var id = key(value);
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) throw new InvalidOperationException("D14 " + label + " 重复：" + id);
            }
        }
    }

    public abstract class D14ReligionCommunityRequest
    {
        public string OperationId;
        public string ActorRef = "player";
        public string SourceEventId;
        public string ContentId;
        public string HistoricalEvidenceEventId;
        public int WorldDay;
        public long SaveRevision;
        public bool WarningAcknowledged;
        public bool SkipSensitiveContent;
        public D14PerformanceLevel PerformanceLevel = D14PerformanceLevel.Contextual;
        public List<string> EvidenceEventIds = new List<string>();
        public List<string> CauseEventIds = new List<string>();

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(ActorRef) || string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(ContentId) || string.IsNullOrWhiteSpace(HistoricalEvidenceEventId) || WorldDay < 0 || SaveRevision < 0 || EvidenceEventIds == null || CauseEventIds == null || !Enum.IsDefined(typeof(D14PerformanceLevel), PerformanceLevel) || PerformanceLevel == D14PerformanceLevel.Omitted)
                throw new InvalidOperationException(field + " 必须包含操作、来源、审查证据、时间和表现等级。");
        }
    }

    public sealed class D14FestivalRequest : D14ReligionCommunityRequest { public string FestivalId; }
    public sealed class D14DietaryRequest : D14ReligionCommunityRequest { public string RuleId; public string FoodTag; }
    public sealed class D14BuildingAccessRequest : D14ReligionCommunityRequest { public string RiteId; public int LocalHour; }
    public sealed class D14FuneralRiteRequest : D14ReligionCommunityRequest { public string RiteId; public string FuneralId; }
    public sealed class D14OathRequest : D14ReligionCommunityRequest
    {
        public string OathId; public string TargetRef; public string ReligionId; public string CommunityId; public D14OathAction Action; public D14TrustDomain TrustDomain;
    }
    public sealed class D14CommunityInteractionRequest : D14ReligionCommunityRequest
    {
        public string CommunityId; public string EtiquetteId; public string FestivalId; public string DietaryRuleId;
    }

    public sealed class D14ExpressionProposal
    {
        public string ProposalId; public string ActorRef; public string ContentId; public string Text; public string Context; public int WorldDay; public D14PerformanceLevel PerformanceLevel;
        public D14ExpressionProposal Clone() { return (D14ExpressionProposal)MemberwiseClone(); }
    }

    public sealed class D14BuildingOpenDecision
    {
        public bool Allowed; public string Code; public string Message; public string BuildingStatus; public string SourceOwnerSystem = D14ReligionCommunityIds.BuildingOwner; public string SourceEventId;
    }
    public sealed class D14FuneralRiteDecision
    {
        public bool Allowed; public string Code; public string Message; public string Status = "Held"; public string SourceOwnerSystem = D14ReligionCommunityIds.D12CommunityOwner; public string SourceEventId;
    }
    public sealed class D14TrustDecision
    {
        public bool Allowed; public string Code; public string Message; public int AppliedTrustDelta; public int TrustAfter; public string SourceOwnerSystem; public string SourceEventId;
    }
    public sealed class D14CommunityInteractionDecision
    {
        public bool Allowed; public string Code; public string Message; public int AppliedTrustDelta; public int TrustAfter; public string SourceOwnerSystem = D14ReligionCommunityIds.D12CommunityOwner; public string SourceEventId;
    }

    public interface ID14BuildingScheduleAuthority { D14BuildingOpenDecision Evaluate(D14BuildingAccessRequest request, FactEvent sourceFact); }
    public interface ID14FuneralRiteAuthority { D14FuneralRiteDecision Hold(D14FuneralRiteRequest request, FactEvent sourceFact); }
    public interface ID14D10TrustAuthority { D14TrustDecision ApplyOath(D14OathRequest request, FactEvent sourceFact, int proposedTrustDelta); }
    public interface ID14D12CommunityAuthority
    {
        D14TrustDecision ApplyOath(D14OathRequest request, FactEvent sourceFact, int proposedTrustDelta);
        D14CommunityInteractionDecision Resolve(D14CommunityInteractionRequest request, FactEvent sourceFact, int proposedTrustDelta);
    }

    public sealed class D14ReligionCommunityResult
    {
        public bool Accepted; public bool Duplicate; public string Code; public string Message; public string EventId; public FactEvent Fact;
        public D14PerformanceLevel PerformanceLevel; public D14ContentReviewRecord Review; public D14FestivalRecord Festival; public D14DietaryRecord Dietary; public D14BuildingAccessRecord Building; public D14FuneralRiteRecord Funeral; public D14OathRecord Oath; public D14CommunityInteractionRecord CommunityInteraction;
        public D14ReligionCommunityRuntimeSnapshot State;
        public static D14ReligionCommunityResult Rejected(string code, string message, D14ReligionCommunityRuntimeSnapshot state) { return new D14ReligionCommunityResult { Accepted = false, Code = code, Message = message, State = state }; }
    }
}
