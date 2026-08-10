// ============================================================
// D12CommunityTradeContracts —— 社群交易、边界与长期声望 DTO
//
// 社群交易不是 D3 市场的皮肤：社群库存、礼仪门槛、知识/语言、边界
// 和长期声望由 CommunityTradeSystem 裁决；玩家货物数量仍由
// D3InventorySystem 唯一写入。Agent 只能产生 intent，不携带最终结果。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public static class D12CommunityTradeIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "CommunityTradeSystem";
        public const string EventPrefix = "event_d12_community_trade_";
        public const string HelpEventPrefix = "event_d12_community_help_";
        public const string GreenCayCommunityId = "community_green_cay";
        public const string FreeCoveCommunityId = "community_free_cove";
        public const string GreenCayTradeLocation = "green_cay_trade_cove";
        public const string GreenCayMangroveLocation = "green_cay_mangrove_reserve";
        public const string FreeCoveTradeLocation = "free_cove_exchange_quay";
    }

    public enum D12CommunityContentReviewStatus
    {
        Approved,
        ApprovedWithWarning,
        PendingReview,
        Blocked,
    }

    public enum D12CommunityBoundaryKind
    {
        LandBoundary,
        ProtectedArea,
    }

    public enum D12CommunityEtiquette
    {
        FormalGreeting,
        GiftBeforeTrade,
        DirectBargain,
        LeaveOffering,
        QuietObservation,
    }

    public enum D12CommunityTradeDisposition
    {
        Completed,
        HelpOffered,
        Misunderstood,
        Refused,
        Cancelled,
    }

    [Serializable]
    public sealed class D12CommunityDefinition
    {
        public int SchemaVersion = D12CommunityTradeIds.SchemaVersion;
        public string Id;
        public string DisplayNameKey;
        public string SourceNote;
        public string HistoricalTag;
        public string Phase = "D12";
        public string OwnerSystem = D12CommunityTradeIds.SystemOwner;
        public string TestId;
        public string SensitiveContentLabel;
        public string ContentWarning;
        public D12CommunityContentReviewStatus ContentReviewStatus;
        public string SocialInstitution;
        public string RequiredLanguageId;
        public int RequiredLanguageLevel;
        public int RequiredKnowledgeLevel;
        public D12CommunityEtiquette RequiredEtiquette;
        public string LandBoundaryId;
        public string ProtectedAreaId;
        public Dictionary<string, int> InitialStock = new Dictionary<string, int>();

        public D12CommunityDefinition Clone()
        {
            var result = new D12CommunityDefinition
            {
                SchemaVersion = SchemaVersion,
                Id = Id,
                DisplayNameKey = DisplayNameKey,
                SourceNote = SourceNote,
                HistoricalTag = HistoricalTag,
                Phase = Phase,
                OwnerSystem = OwnerSystem,
                TestId = TestId,
                SensitiveContentLabel = SensitiveContentLabel,
                ContentWarning = ContentWarning,
                ContentReviewStatus = ContentReviewStatus,
                SocialInstitution = SocialInstitution,
                RequiredLanguageId = RequiredLanguageId,
                RequiredLanguageLevel = RequiredLanguageLevel,
                RequiredKnowledgeLevel = RequiredKnowledgeLevel,
                RequiredEtiquette = RequiredEtiquette,
                LandBoundaryId = LandBoundaryId,
                ProtectedAreaId = ProtectedAreaId,
            };
            if (InitialStock != null)
                foreach (var pair in InitialStock) result.InitialStock[pair.Key] = pair.Value;
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != D12CommunityTradeIds.SchemaVersion)
                throw new InvalidOperationException("社群定义 schema_version 不匹配：" + Id);
            Require(Id, "社群 ID");
            Require(DisplayNameKey, "社群显示名 key");
            Require(SourceNote, "社群来源说明");
            Require(TestId, "社群 test_id");
            if (HistoricalTag != "historical" && HistoricalTag != "inferred" && HistoricalTag != "fictional")
                throw new InvalidOperationException("社群 historical_tag 非法：" + Id);
            if (OwnerSystem != D12CommunityTradeIds.SystemOwner)
                throw new InvalidOperationException("社群 owner_system 非法：" + Id);
            if (string.IsNullOrWhiteSpace(SocialInstitution))
                throw new InvalidOperationException("社群必须声明社会制度/组织形态：" + Id);
            Require(RequiredLanguageId, "社群语言门槛");
            if (RequiredLanguageLevel < 0 || RequiredKnowledgeLevel < 0)
                throw new InvalidOperationException("社群语言/知识门槛不能为负数：" + Id);
            if (!Enum.IsDefined(typeof(D12CommunityEtiquette), RequiredEtiquette))
                throw new InvalidOperationException("社群礼仪规则非法：" + Id);
            if (ContentReviewStatus == D12CommunityContentReviewStatus.PendingReview ||
                ContentReviewStatus == D12CommunityContentReviewStatus.Blocked)
                throw new InvalidOperationException("未完成审查的社群不能进入运行时：" + Id);
            Require(SensitiveContentLabel, "社群敏感内容标签");
            Require(ContentWarning, "社群内容警告");
            Require(LandBoundaryId, "社群土地边界引用");
            if (string.IsNullOrWhiteSpace(ProtectedAreaId))
                throw new InvalidOperationException("社群必须声明保护区引用：" + Id);
            if (InitialStock == null) InitialStock = new Dictionary<string, int>();
            foreach (var pair in InitialStock)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0)
                    throw new InvalidOperationException("社群初始库存非法：" + Id);
            }
        }

        private static void Require(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(label + "不能为空。");
        }
    }

    [Serializable]
    public sealed class D12CommunityBoundaryDefinition
    {
        public int SchemaVersion = D12CommunityTradeIds.SchemaVersion;
        public string Id;
        public string CommunityId;
        public D12CommunityBoundaryKind Kind;
        public string LocationRef;
        public string OwnerSystem = "PropertyIslandSystem";
        public bool RequiresConsent;
        public bool TradeAllowed;
        public string HistoricalTag;
        public string SensitiveContentLabel;
        public D12CommunityContentReviewStatus ContentReviewStatus;

        public D12CommunityBoundaryDefinition Clone()
        {
            return new D12CommunityBoundaryDefinition
            {
                SchemaVersion = SchemaVersion,
                Id = Id,
                CommunityId = CommunityId,
                Kind = Kind,
                LocationRef = LocationRef,
                OwnerSystem = OwnerSystem,
                RequiresConsent = RequiresConsent,
                TradeAllowed = TradeAllowed,
                HistoricalTag = HistoricalTag,
                SensitiveContentLabel = SensitiveContentLabel,
                ContentReviewStatus = ContentReviewStatus,
            };
        }

        public void Validate()
        {
            if (SchemaVersion != D12CommunityTradeIds.SchemaVersion) throw new InvalidOperationException("边界 schema_version 不匹配：" + Id);
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(CommunityId) || string.IsNullOrWhiteSpace(LocationRef))
                throw new InvalidOperationException("社群边界必须有 ID、社群和位置：" + Id);
            if (OwnerSystem != "PropertyIslandSystem") throw new InvalidOperationException("土地/保护区定义必须由 PropertyIslandSystem 所有：" + Id);
            if (!Enum.IsDefined(typeof(D12CommunityBoundaryKind), Kind)) throw new InvalidOperationException("社群边界类型非法：" + Id);
            if (HistoricalTag != "historical" && HistoricalTag != "inferred" && HistoricalTag != "fictional")
                throw new InvalidOperationException("边界 historical_tag 非法：" + Id);
            if (string.IsNullOrWhiteSpace(SensitiveContentLabel)) throw new InvalidOperationException("边界缺少敏感内容标签：" + Id);
            if (ContentReviewStatus == D12CommunityContentReviewStatus.PendingReview || ContentReviewStatus == D12CommunityContentReviewStatus.Blocked)
                throw new InvalidOperationException("未审查边界不能进入运行时：" + Id);
        }
    }

    [Serializable]
    public sealed class D12CommunityInventoryState
    {
        public string CommunityId;
        public Dictionary<string, int> Stock = new Dictionary<string, int>();
        public long Revision;
        public string LastEventId;
        public List<string> AppliedEventKeys = new List<string>();

        public D12CommunityInventoryState Clone()
        {
            var result = new D12CommunityInventoryState
            {
                CommunityId = CommunityId,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (Stock != null) foreach (var pair in Stock) result.Stock[pair.Key] = pair.Value;
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class D12CommunityReputationChange
    {
        public string EventId;
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public string ReasonCode;
        public int ReputationDelta;
        public int TrustDelta;
        public int ReputationAfter;
        public int TrustAfter;
        public string Explanation;

        public D12CommunityReputationChange Clone()
        {
            var result = new D12CommunityReputationChange
            {
                EventId = EventId,
                WorldDay = WorldDay,
                ReasonCode = ReasonCode,
                ReputationDelta = ReputationDelta,
                TrustDelta = TrustDelta,
                ReputationAfter = ReputationAfter,
                TrustAfter = TrustAfter,
                Explanation = Explanation,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12CommunityReputationState
    {
        public string ActorRef;
        public string CommunityId;
        public int LongTermReputation;
        public int Trust;
        public int CulturalKnowledge;
        public int EtiquetteFamiliarity;
        public int MisunderstandingCount;
        public int HelpReceivedCount;
        public int RefusalCount;
        public int SuccessfulTradeCount;
        public long Revision;
        public List<D12CommunityReputationChange> History = new List<D12CommunityReputationChange>();

        public D12CommunityReputationState Clone()
        {
            var result = new D12CommunityReputationState
            {
                ActorRef = ActorRef,
                CommunityId = CommunityId,
                LongTermReputation = LongTermReputation,
                Trust = Trust,
                CulturalKnowledge = CulturalKnowledge,
                EtiquetteFamiliarity = EtiquetteFamiliarity,
                MisunderstandingCount = MisunderstandingCount,
                HelpReceivedCount = HelpReceivedCount,
                RefusalCount = RefusalCount,
                SuccessfulTradeCount = SuccessfulTradeCount,
                Revision = Revision,
            };
            if (History != null)
                foreach (var item in History) if (item != null) result.History.Add(item.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D12CommunityTradeSnapshot
    {
        public int SchemaVersion = D12CommunityTradeIds.SchemaVersion;
        public int LastWorldDay;
        public long Revision;
        public string LastEventId;
        public List<D12CommunityInventoryState> Communities = new List<D12CommunityInventoryState>();
        public List<D12CommunityReputationState> Reputations = new List<D12CommunityReputationState>();
        public EventLogSnapshot EventLog;

        public D12CommunityTradeSnapshot Clone()
        {
            var result = new D12CommunityTradeSnapshot
            {
                SchemaVersion = SchemaVersion,
                LastWorldDay = LastWorldDay,
                Revision = Revision,
                LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Communities != null) foreach (var item in Communities) if (item != null) result.Communities.Add(item.Clone());
            if (Reputations != null) foreach (var item in Reputations) if (item != null) result.Reputations.Add(item.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != D12CommunityTradeIds.SchemaVersion)
                throw new InvalidOperationException("社群交易快照 schema_version 不匹配：" + SchemaVersion);
            if (LastWorldDay < 0 || Revision < 0) throw new InvalidOperationException("社群交易快照时间/修订号非法。");
            var communities = new HashSet<string>(StringComparer.Ordinal);
            if (Communities != null)
                foreach (var item in Communities)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.CommunityId)) throw new InvalidOperationException("社群库存快照缺少 ID。");
                    if (!communities.Add(item.CommunityId)) throw new InvalidOperationException("社群库存重复：" + item.CommunityId);
                    if (item.Revision < 0) throw new InvalidOperationException("社群库存修订号非法：" + item.CommunityId);
                    if (item.Stock == null) item.Stock = new Dictionary<string, int>();
                    foreach (var pair in item.Stock)
                        if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value < 0) throw new InvalidOperationException("社群库存值非法：" + item.CommunityId);
                }
            var reputationKeys = new HashSet<string>(StringComparer.Ordinal);
            if (Reputations != null)
                foreach (var item in Reputations)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.ActorRef) || string.IsNullOrWhiteSpace(item.CommunityId))
                        throw new InvalidOperationException("社群声望快照缺少 actor/community。");
                    string key = Key(item.ActorRef, item.CommunityId);
                    if (!reputationKeys.Add(key)) throw new InvalidOperationException("社群声望重复：" + key);
                    if (item.LongTermReputation < -100 || item.LongTermReputation > 100 || item.Trust < 0 || item.Trust > 100)
                        throw new InvalidOperationException("社群长期声望/信任越界：" + key);
                    if (item.CulturalKnowledge < 0 || item.CulturalKnowledge > 100 || item.EtiquetteFamiliarity < 0 || item.EtiquetteFamiliarity > 100)
                        throw new InvalidOperationException("社群知识/礼仪熟悉度越界：" + key);
                }
        }

        public static string Key(string actorRef, string communityId)
        {
            return (actorRef ?? string.Empty) + "\u001f" + (communityId ?? string.Empty);
        }
    }

    [Serializable]
    public sealed class D12CommunityTradeIntent
    {
        public string RequestId;
        public string CommunityId;
        public string ActorRef = "player";
        public string LocationRef;
        public string GiveItemId;
        public int GiveQuantity;
        public string WantItemId;
        public int WantQuantity;
        public string LanguageId;
        public int LanguageLevel;
        public int KnowledgeLevel;
        public D12CommunityEtiquette Etiquette;
        public string BoundaryConsentRef;
        public string ProposalText;
        public bool ProposedByAgent;
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;

        public D12CommunityTradeIntent Clone()
        {
            var result = new D12CommunityTradeIntent
            {
                RequestId = RequestId,
                CommunityId = CommunityId,
                ActorRef = ActorRef,
                LocationRef = LocationRef,
                GiveItemId = GiveItemId,
                GiveQuantity = GiveQuantity,
                WantItemId = WantItemId,
                WantQuantity = WantQuantity,
                LanguageId = LanguageId,
                LanguageLevel = LanguageLevel,
                KnowledgeLevel = KnowledgeLevel,
                Etiquette = Etiquette,
                BoundaryConsentRef = BoundaryConsentRef,
                ProposalText = ProposalText,
                ProposedByAgent = ProposedByAgent,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12CommunityHelpRequest
    {
        public string RequestId;
        public string CommunityId;
        public string ActorRef = "player";
        public string LocationRef;
        public string LanguageId;
        public int LanguageLevel;
        public int KnowledgeLevel;
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D12CommunityTradeResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public bool FactLogged;
        public string Code;
        public string Message;
        public D12CommunityTradeDisposition Disposition;
        public string CommunityId;
        public string EventId;
        public string GiveItemId;
        public int GiveQuantity;
        public string WantItemId;
        public int WantQuantity;
        public int LongTermReputationAfter;
        public int TrustAfter;
        public int CulturalKnowledgeAfter;
        public int EtiquetteFamiliarityAfter;
        public string RecoveryRouteId;
        public FactEvent Fact;

        public D12CommunityTradeResult Clone()
        {
            return new D12CommunityTradeResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                FactLogged = FactLogged,
                Code = Code,
                Message = Message,
                Disposition = Disposition,
                CommunityId = CommunityId,
                EventId = EventId,
                GiveItemId = GiveItemId,
                GiveQuantity = GiveQuantity,
                WantItemId = WantItemId,
                WantQuantity = WantQuantity,
                LongTermReputationAfter = LongTermReputationAfter,
                TrustAfter = TrustAfter,
                CulturalKnowledgeAfter = CulturalKnowledgeAfter,
                EtiquetteFamiliarityAfter = EtiquetteFamiliarityAfter,
                RecoveryRouteId = RecoveryRouteId,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D12CommunityView
    {
        public string CommunityId;
        public string DisplayNameKey;
        public string SocialInstitution;
        public string HistoricalTag;
        public string SensitiveContentLabel;
        public D12CommunityContentReviewStatus ContentReviewStatus;
        public string LandBoundaryId;
        public string ProtectedAreaId;
        public Dictionary<string, int> Stock = new Dictionary<string, int>();
        public D12CommunityReputationState Reputation;

        public D12CommunityView Clone()
        {
            var result = new D12CommunityView
            {
                CommunityId = CommunityId,
                DisplayNameKey = DisplayNameKey,
                SocialInstitution = SocialInstitution,
                HistoricalTag = HistoricalTag,
                SensitiveContentLabel = SensitiveContentLabel,
                ContentReviewStatus = ContentReviewStatus,
                LandBoundaryId = LandBoundaryId,
                ProtectedAreaId = ProtectedAreaId,
                Reputation = Reputation == null ? null : Reputation.Clone(),
            };
            if (Stock != null) foreach (var pair in Stock) result.Stock[pair.Key] = pair.Value;
            return result;
        }
    }
}
