// D14-A04 奖励系统契约。
// RewardSystem 只写奖励请求、奖励记录、领取状态与审计；物品、金钱、图鉴等
// 最终领域值仍由各自 canonical owner 写入，奖励记录只保存可追溯来源引用。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14RewardIds
    {
        public const int SchemaVersion = 1;
        public const string OwnerSystem = "RewardSystem";
        public const string InventoryOwner = "InventorySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string CollectionOwner = "CollectionAtlasSystem";
        public const string RecoveryOwner = "RewardSystem";
        public const string EventPrefix = "event_d14_reward_";
        public const string RecoveryRoute = "route_d14_reward_recover_lost";
        public const string AuditRoute = "route_d14_reward_audit";

        public static string StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') || c == '_' || c == '-')) chars[i] = '_';
            }
            return new string(chars);
        }

        public static string GrantId(string rewardId, string recipientRef, string sourceEventId)
        {
            if (string.IsNullOrWhiteSpace(rewardId) || string.IsNullOrWhiteSpace(recipientRef) || string.IsNullOrWhiteSpace(sourceEventId))
                return string.Empty;
            return rewardId + "@" + StableId(recipientRef) + "@" + StableId(sourceEventId);
        }
    }

    public enum D14RewardRarity { Ordinary, Professional, Rare, Unique, WorldClass }
    public enum D14RewardKind { Currency, Item, ShipModule, Collection, Reputation, Information, Mixed }
    public enum D14RewardStatus { Pending, Granted, Claimed, Lost, Recovered, Denied, Expired }
    public enum D14RewardRequestStatus { Pending, Approved, Rejected, Granted, Claimed, Recovered, Denied }
    public enum D14RewardSourceKind { Quest, Salvage, Collection, Combat, Boarding, Exploration, Achievement, Recovery }
    public enum D14RewardLossReason { InventoryLoss, ShipLoss, Seizure, SaveMigration, OwnerCorrection }

    [Serializable]
    public sealed class D14RewardDefinition
    {
        public string RewardId;
        public string DisplayName;
        public D14RewardRarity Rarity;
        public D14RewardKind Kind;
        public D14RewardSourceKind SourceKind;
        public bool Unique;
        public bool RequiresClaim = true;
        public string ContentId;
        public string SourceRef;
        public string HistoricalTag;
        public string ReviewStatus = "approved";
        public string AuthorizationRef;
        public string OwnerSystem = D14RewardIds.OwnerSystem;
        public bool RecoveryAllowed = true;
        public int RecoveryWindowDays = 30;
        public List<string> AllowedSourceOwnerSystems = new List<string>();
        public string TestId;

        public D14RewardDefinition Clone()
        {
            var result = new D14RewardDefinition
            {
                RewardId = RewardId, DisplayName = DisplayName, Rarity = Rarity, Kind = Kind,
                SourceKind = SourceKind, Unique = Unique, RequiresClaim = RequiresClaim, ContentId = ContentId,
                SourceRef = SourceRef, HistoricalTag = HistoricalTag, ReviewStatus = ReviewStatus,
                AuthorizationRef = AuthorizationRef, OwnerSystem = OwnerSystem, RecoveryAllowed = RecoveryAllowed,
                RecoveryWindowDays = RecoveryWindowDays, TestId = TestId,
            };
            if (AllowedSourceOwnerSystems != null) result.AllowedSourceOwnerSystems.AddRange(AllowedSourceOwnerSystems);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RewardId) || !RewardId.StartsWith("reward_", StringComparison.Ordinal))
                throw new InvalidOperationException(field + ".reward_id must use the reward_ content ID prefix.");
            if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(SourceRef) || string.IsNullOrWhiteSpace(TestId))
                throw new InvalidOperationException(field + " requires display_name, source_ref and test_id.");
            if (!Enum.IsDefined(typeof(D14RewardRarity), Rarity) || !Enum.IsDefined(typeof(D14RewardKind), Kind) || !Enum.IsDefined(typeof(D14RewardSourceKind), SourceKind))
                throw new InvalidOperationException(field + " contains an invalid rarity, kind or source kind.");
            if (string.IsNullOrWhiteSpace(ContentId) && (Kind == D14RewardKind.Item || Kind == D14RewardKind.ShipModule || Kind == D14RewardKind.Collection))
                throw new InvalidOperationException(field + " item/module/collection rewards require content_id.");
            if (string.IsNullOrWhiteSpace(OwnerSystem) || OwnerSystem != D14RewardIds.OwnerSystem)
                throw new InvalidOperationException(field + " must be owned by RewardSystem.");
            if (string.IsNullOrWhiteSpace(HistoricalTag) || string.IsNullOrWhiteSpace(ReviewStatus) || ReviewStatus == "pending" || ReviewStatus == "rejected")
                throw new InvalidOperationException(field + " requires a reviewed historical tag and status.");
            if (RecoveryWindowDays < 0 || AllowedSourceOwnerSystems == null || AllowedSourceOwnerSystems.Count == 0)
                throw new InvalidOperationException(field + " requires a non-negative recovery window and source owners.");
            foreach (var owner in AllowedSourceOwnerSystems)
                if (string.IsNullOrWhiteSpace(owner)) throw new InvalidOperationException(field + " contains an empty source owner.");
        }
    }

    [Serializable]
    public sealed class D14RewardRequest
    {
        public string RequestId;
        public string RewardId;
        public string RecipientRef;
        public string RequestedBySystem;
        public string TaskId;
        public string SourceEventId;
        public long SourceSaveRevision;
        public int WorldDay;
        public long SaveRevision;
        public D14RewardRequestStatus Status = D14RewardRequestStatus.Pending;
        public List<string> CauseEventIds = new List<string>();

        public D14RewardRequest Clone()
        {
            var result = new D14RewardRequest
            {
                RequestId = RequestId, RewardId = RewardId, RecipientRef = RecipientRef,
                RequestedBySystem = RequestedBySystem, TaskId = TaskId, SourceEventId = SourceEventId,
                SourceSaveRevision = SourceSaveRevision, WorldDay = WorldDay, SaveRevision = SaveRevision,
                Status = Status,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RequestId) || string.IsNullOrWhiteSpace(RewardId) || string.IsNullOrWhiteSpace(RecipientRef) ||
                string.IsNullOrWhiteSpace(RequestedBySystem) || string.IsNullOrWhiteSpace(SourceEventId))
                throw new InvalidOperationException(field + " requires request, reward, recipient, requester and source ids.");
            if (RequestedBySystem.IndexOf("AgentReply", StringComparison.OrdinalIgnoreCase) >= 0 ||
                string.Equals(RequestedBySystem, "Agent", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(field + " cannot be submitted as an AgentReply final reward.");
            if (!Enum.IsDefined(typeof(D14RewardRequestStatus), Status))
                throw new InvalidOperationException(field + ".status is invalid.");
            if (SourceSaveRevision < 0 || SaveRevision < 0 || WorldDay < 0)
                throw new InvalidOperationException(field + " has an invalid time or save revision.");
            if (CauseEventIds == null) throw new InvalidOperationException(field + ".cause_event_ids must be present.");
        }
    }

    [Serializable]
    public sealed class D14TaskRewardRequest
    {
        public string RequestId;
        public string RewardId;
        public string TaskId;
        public string RecipientRef;
        public string SourceEventId;
        public long SourceSaveRevision;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D14RewardGrantRecord
    {
        public string GrantId;
        public string RequestId;
        public string RewardId;
        public string ContentId;
        public D14RewardRarity Rarity;
        public D14RewardKind Kind;
        public bool Unique;
        public string RecipientRef;
        public D14RewardStatus Status;
        public string SourceEventId;
        public long SourceSaveRevision;
        public string GrantEventId;
        public long GrantSaveRevision;
        public string ClaimEventId;
        public string LossEventId;
        public string RecoveryEventId;
        public string InventorySourceEventId;
        public string EconomySourceEventId;
        public string CollectionSourceEventId;
        public string RecoveryRoute;
        public D14RewardLossReason LossReason;
        public int LostWorldDay = -1;
        public string LastEventId;

        public D14RewardGrantRecord Clone()
        {
            return (D14RewardGrantRecord)MemberwiseClone();
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(GrantId) || string.IsNullOrWhiteSpace(RequestId) || string.IsNullOrWhiteSpace(RewardId) || string.IsNullOrWhiteSpace(RecipientRef) ||
                string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(GrantEventId) || string.IsNullOrWhiteSpace(LastEventId))
                throw new InvalidOperationException(field + " requires grant, request, recipient and source event ids.");
            if (!Enum.IsDefined(typeof(D14RewardRarity), Rarity) || !Enum.IsDefined(typeof(D14RewardKind), Kind) || !Enum.IsDefined(typeof(D14RewardStatus), Status))
                throw new InvalidOperationException(field + " contains an invalid reward state.");
            if (SourceSaveRevision < 0 || GrantSaveRevision < 0 || LostWorldDay < -1)
                throw new InvalidOperationException(field + " has an invalid revision or lost day.");
        }
    }

    [Serializable]
    public sealed class D14RewardAuditRecord
    {
        public string AuditId;
        public string GrantId;
        public string RequestId;
        public string Action;
        public string EventId;
        public long SaveRevision;
        public int WorldDay;
        public string OwnerSystem;
        public string SourceEventId;
        public string Reason;
        public string RecoveryRoute;

        public D14RewardAuditRecord Clone() { return (D14RewardAuditRecord)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(AuditId) || string.IsNullOrWhiteSpace(Action) || string.IsNullOrWhiteSpace(EventId) ||
                string.IsNullOrWhiteSpace(OwnerSystem) || string.IsNullOrWhiteSpace(SourceEventId))
                throw new InvalidOperationException(field + " requires audit action, event, owner and source ids.");
            if (SaveRevision < 0 || WorldDay < 0) throw new InvalidOperationException(field + " has an invalid time or revision.");
        }
    }

    [Serializable]
    public sealed class D14RewardRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14RewardIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public List<D14RewardRequest> Requests = new List<D14RewardRequest>();
        public List<D14RewardGrantRecord> Grants = new List<D14RewardGrantRecord>();
        public List<D14RewardAuditRecord> Audits = new List<D14RewardAuditRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14RewardRuntimeSnapshot Clone()
        {
            var result = new D14RewardRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Requests != null) foreach (var item in Requests) if (item != null) result.Requests.Add(item.Clone());
            if (Grants != null) foreach (var item in Grants) if (item != null) result.Grants.Add(item.Clone());
            if (Audits != null) foreach (var item in Audits) if (item != null) result.Audits.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0 || Requests == null || Grants == null || Audits == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14 reward snapshot is invalid.");
            var requests = new HashSet<string>(StringComparer.Ordinal);
            foreach (var request in Requests)
            {
                if (request == null) throw new InvalidOperationException("D14 reward snapshot contains a null request.");
                request.Validate("d14.reward.request");
                if (!requests.Add(request.RequestId)) throw new InvalidOperationException("Duplicate D14 reward request: " + request.RequestId);
            }
            var grants = new HashSet<string>(StringComparer.Ordinal);
            foreach (var grant in Grants)
            {
                if (grant == null) throw new InvalidOperationException("D14 reward snapshot contains a null grant.");
                grant.Validate("d14.reward.grant");
                if (!grants.Add(grant.GrantId)) throw new InvalidOperationException("Duplicate D14 reward grant: " + grant.GrantId);
            }
            var audits = new HashSet<string>(StringComparer.Ordinal);
            foreach (var audit in Audits)
            {
                if (audit == null) throw new InvalidOperationException("D14 reward snapshot contains a null audit.");
                audit.Validate("d14.reward.audit");
                if (!audits.Add(audit.AuditId)) throw new InvalidOperationException("Duplicate D14 reward audit: " + audit.AuditId);
            }
        }
    }

    public sealed class D14RewardSettlementRequest
    {
        public string GrantId;
        public string RecipientRef;
        public D14RewardDefinition Definition;
        public D14RewardRequest RewardRequest;
        public FactEvent SourceFact;
    }

    public sealed class D14RewardGrantRequest
    {
        public string RequestId;
        public int WorldDay;
        public long SaveRevision;
    }

    public sealed class D14RewardClaimRequest
    {
        public string OperationId;
        public string GrantId;
        public string RecipientRef;
        public int WorldDay;
        public long SaveRevision;
    }

    public sealed class D14RewardSettlementDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string SourceOwnerSystem;
        public string SourceEventId;
        public string InventorySourceEventId;
        public string EconomySourceEventId;
        public string CollectionSourceEventId;
    }

    public sealed class D14RewardLossRequest
    {
        public string OperationId;
        public string GrantId;
        public D14RewardLossReason Reason;
        public string SourceEventId;
        public long SourceSaveRevision;
        public int WorldDay;
        public long SaveRevision;
    }

    public sealed class D14RewardRecoveryRequest
    {
        public string OperationId;
        public string GrantId;
        public string RecoverySourceEventId;
        public long RecoverySourceSaveRevision;
        public string RecipientRef;
        public int WorldDay;
        public long SaveRevision;
    }

    public sealed class D14RewardRecoveryDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string SourceOwnerSystem;
        public string SourceEventId;
        public string InventorySourceEventId;
        public string EconomySourceEventId;
        public string CollectionSourceEventId;
    }

    public interface ID14RewardSettlementAuthority
    {
        D14RewardSettlementDecision Settle(D14RewardSettlementRequest request);
    }

    public interface ID14RewardRecoveryAuthority
    {
        D14RewardRecoveryDecision Recover(D14RewardRecoveryRequest request, D14RewardGrantRecord grant, D14RewardDefinition definition, FactEvent sourceFact);
    }

    public sealed class D14RewardResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14RewardRequest Request;
        public D14RewardGrantRecord Grant;
        public D14RewardRuntimeSnapshot State;

        public static D14RewardResult Rejected(string code, string message, D14RewardRuntimeSnapshot state)
        {
            return new D14RewardResult { Accepted = false, Code = code, Message = message, State = state };
        }
    }
}
