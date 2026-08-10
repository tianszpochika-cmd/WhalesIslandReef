// ============================================================
// D12PokerContracts —— 牌局事实、快照与跨 owner 请求
//
// MiniGameSystem 是牌面、下注、胜负、退出和牌局结算的唯一写者。
// EconomySystem / RewardSystem / RelationSystem / InformationSystem 只消费
// 本文件中的只读请求并由各自系统写入 canonical 状态。UI 和 Agent 不能
// 通过 DTO 直接改钱、债务、奖励、关系或牌面。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public static class D12PokerIds
    {
        public const int CurrentSchemaVersion = 1;
        public const string OwnerSystem = "MiniGameSystem";
        public const string RulesetId = "poker_caribbean_draw_v1";
        public const string EventPrefix = "event_d12_poker_";
        public const string Currency = "gold";
    }

    public enum D12PokerAction
    {
        Unknown,
        Enter,
        PlaceBet,
        AttemptCheat,
        Resolve,
        Exit,
    }

    public enum D12PokerTableStatus
    {
        Unknown,
        InHand,
        Settled,
        Exited,
        Cheated,
    }

    public enum D12PokerOutcome
    {
        Unknown,
        Win,
        Loss,
        Push,
        Fold,
        Cheated,
    }

    public enum D12PokerCheatRisk
    {
        Unknown,
        None,
        Suspected,
        Exposed,
    }

    public enum D12PokerHandCategory
    {
        Unknown,
        HighCard,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
    }

    public enum D12PokerCardSuit
    {
        Unknown,
        Clubs,
        Diamonds,
        Hearts,
        Spades,
    }

    public enum D12PokerCardRank
    {
        Unknown = 0,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14,
    }

    public enum D12PokerRequestStatus
    {
        Unknown,
        Pending,
        Applied,
        Rejected,
    }

    /// <summary>整数金币金额。它只是牌局请求中的数值，不是经济账本。</summary>
    [Serializable]
    public struct D12PokerMoneyAmount
    {
        public long Value;

        public D12PokerMoneyAmount(long value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }

    [Serializable]
    public sealed class D12PokerCard
    {
        public string CardId;
        public D12PokerCardSuit Suit;
        public D12PokerCardRank Rank;

        public D12PokerCard Clone()
        {
            return new D12PokerCard { CardId = CardId, Suit = Suit, Rank = Rank };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CardId)) throw new InvalidOperationException("牌缺少稳定 card_id。");
            if (!Enum.IsDefined(typeof(D12PokerCardSuit), Suit) || Suit == D12PokerCardSuit.Unknown)
                throw new InvalidOperationException("牌的花色非法：" + CardId);
            if (!Enum.IsDefined(typeof(D12PokerCardRank), Rank) || Rank == D12PokerCardRank.Unknown)
                throw new InvalidOperationException("牌的点数非法：" + CardId);
        }
    }

    /// <summary>桌面配置是内容输入；牌局系统读取后负责验证，不在运行中被 UI 改写。</summary>
    [Serializable]
    public sealed class D12PokerTableDefinition
    {
        public string TableId;
        public string RulesetId = D12PokerIds.RulesetId;
        public string LocationRef = "santa_margarita_tavern";
        public string OpponentId = "tavern_house";
        public string RelationTargetId;
        public string InformationPoolId = "tavern_poker_tells";
        public long FixedSeed = 1;
        public long EntryFee = 1;
        public long BetCap = 10;
        public long DebtLimit = 0;
        public int CheatRiskPercent;

        public D12PokerTableDefinition Clone()
        {
            return new D12PokerTableDefinition
            {
                TableId = TableId,
                RulesetId = RulesetId,
                LocationRef = LocationRef,
                OpponentId = OpponentId,
                RelationTargetId = RelationTargetId,
                InformationPoolId = InformationPoolId,
                FixedSeed = FixedSeed,
                EntryFee = EntryFee,
                BetCap = BetCap,
                DebtLimit = DebtLimit,
                CheatRiskPercent = CheatRiskPercent,
            };
        }

        public void Validate()
        {
            Require(TableId, "table_id");
            Require(RulesetId, "ruleset_id");
            Require(LocationRef, "location_ref");
            if (EntryFee < 0) throw new InvalidOperationException("牌局入场费不能为负数：" + TableId);
            if (BetCap < 0) throw new InvalidOperationException("牌局下注上限不能为负数：" + TableId);
            if (DebtLimit < 0) throw new InvalidOperationException("牌局债务上限不能为负数：" + TableId);
            if (CheatRiskPercent < 0 || CheatRiskPercent > 100)
                throw new InvalidOperationException("牌局作弊风险必须在 0..100：" + TableId);
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(name + " 不能为空。");
        }
    }

    /// <summary>
    /// 经济读取快照。它由 EconomySystem 提供；牌局只读取，不能把结果写回其中。
    /// AvailableMoney 是提交动作开始时的可用余额，CurrentDebt 是已存在债务。
    /// </summary>
    [Serializable]
    public sealed class D12PokerEconomyReadSnapshot
    {
        public long AvailableMoney;
        public long CurrentDebt;
        public bool CanBorrow;
        public long DebtLimit;

        public D12PokerEconomyReadSnapshot Clone()
        {
            return new D12PokerEconomyReadSnapshot
            {
                AvailableMoney = AvailableMoney,
                CurrentDebt = CurrentDebt,
                CanBorrow = CanBorrow,
                DebtLimit = DebtLimit,
            };
        }
    }

    /// <summary>UI/Agent 可提交的动作意图；缺省值不会绕过资金和状态校验。</summary>
    [Serializable]
    public sealed class D12PokerActionRequest
    {
        public D12PokerAction Action;
        public string TableId;
        public string PlayerId;
        public int WorldDay;
        public long SaveRevision;
        public long Amount;
        public string LocationRef;
        public D12PokerEconomyReadSnapshot Economy = new D12PokerEconomyReadSnapshot();

        public D12PokerActionRequest Clone()
        {
            return new D12PokerActionRequest
            {
                Action = Action,
                TableId = TableId,
                PlayerId = PlayerId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Amount = Amount,
                LocationRef = LocationRef,
                Economy = Economy == null ? null : Economy.Clone(),
            };
        }
    }

    [Serializable]
    public sealed class D12PokerSessionState
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string TableId;
        public string PlayerId;
        public string GameId;
        public string RulesetId = D12PokerIds.RulesetId;
        public long FixedSeed;
        public long EntryFee;
        public long BetCap;
        public long CurrentBet;
        public long ProposedDebt;
        public D12PokerTableStatus Status = D12PokerTableStatus.Unknown;
        public D12PokerOutcome Outcome = D12PokerOutcome.Unknown;
        public D12PokerCheatRisk CheatRisk = D12PokerCheatRisk.None;
        public bool CheatAttempted;
        public int WorldDay;
        public long SaveRevision;
        public string LastEventId;
        public List<D12PokerCard> PlayerHand = new List<D12PokerCard>();
        public List<D12PokerCard> HouseHand = new List<D12PokerCard>();

        public D12PokerSessionState Clone()
        {
            var result = new D12PokerSessionState
            {
                SchemaVersion = SchemaVersion,
                TableId = TableId,
                PlayerId = PlayerId,
                GameId = GameId,
                RulesetId = RulesetId,
                FixedSeed = FixedSeed,
                EntryFee = EntryFee,
                BetCap = BetCap,
                CurrentBet = CurrentBet,
                ProposedDebt = ProposedDebt,
                Status = Status,
                Outcome = Outcome,
                CheatRisk = CheatRisk,
                CheatAttempted = CheatAttempted,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId,
            };
            CopyCards(result.PlayerHand, PlayerHand);
            CopyCards(result.HouseHand, HouseHand);
            return result;
        }

        private static void CopyCards(List<D12PokerCard> target, List<D12PokerCard> source)
        {
            if (source != null)
                foreach (var card in source)
                    if (card != null) target.Add(card.Clone());
        }
    }

    [Serializable]
    public sealed class D12PokerGameRecord
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RecordId;
        public string GameId;
        public string TableId;
        public string PlayerId;
        public string SourceEventId;
        public long SaveRevision;
        public int WorldDay;
        public long FixedSeed;
        public long EntryFee;
        public long Bet;
        public long Payout;
        public long NetProposedDelta;
        public D12PokerOutcome Outcome = D12PokerOutcome.Unknown;
        public D12PokerHandCategory PlayerHandCategory = D12PokerHandCategory.Unknown;
        public D12PokerHandCategory HouseHandCategory = D12PokerHandCategory.Unknown;
        public D12PokerCheatRisk CheatRisk = D12PokerCheatRisk.None;
        public List<string> PlayerCardIds = new List<string>();
        public List<string> HouseCardIds = new List<string>();
        public bool SettlementClaimed;

        public D12PokerGameRecord Clone()
        {
            var result = new D12PokerGameRecord
            {
                SchemaVersion = SchemaVersion,
                RecordId = RecordId,
                GameId = GameId,
                TableId = TableId,
                PlayerId = PlayerId,
                SourceEventId = SourceEventId,
                SaveRevision = SaveRevision,
                WorldDay = WorldDay,
                FixedSeed = FixedSeed,
                EntryFee = EntryFee,
                Bet = Bet,
                Payout = Payout,
                NetProposedDelta = NetProposedDelta,
                Outcome = Outcome,
                PlayerHandCategory = PlayerHandCategory,
                HouseHandCategory = HouseHandCategory,
                CheatRisk = CheatRisk,
                SettlementClaimed = SettlementClaimed,
            };
            if (PlayerCardIds != null) result.PlayerCardIds.AddRange(PlayerCardIds);
            if (HouseCardIds != null) result.HouseCardIds.AddRange(HouseCardIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12PokerEconomyRequest
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RequestId;
        public string GameId;
        public string SourceEventId;
        public string PlayerId;
        public string Currency = D12PokerIds.Currency;
        public string Kind;
        public long Amount;
        public D12PokerRequestStatus Status = D12PokerRequestStatus.Pending;

        public D12PokerEconomyRequest Clone()
        {
            return new D12PokerEconomyRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                GameId = GameId,
                SourceEventId = SourceEventId,
                PlayerId = PlayerId,
                Currency = Currency,
                Kind = Kind,
                Amount = Amount,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D12PokerDebtRequest
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RequestId;
        public string GameId;
        public string SourceEventId;
        public string PlayerId;
        public long Principal;
        public long CurrentDebt;
        public long DebtLimit;
        public D12PokerRequestStatus Status = D12PokerRequestStatus.Pending;

        public D12PokerDebtRequest Clone()
        {
            return new D12PokerDebtRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                GameId = GameId,
                SourceEventId = SourceEventId,
                PlayerId = PlayerId,
                Principal = Principal,
                CurrentDebt = CurrentDebt,
                DebtLimit = DebtLimit,
                Status = Status,
            };
        }
    }

    /// <summary>赢牌奖励请求只交给 RewardSystem；牌局系统不增加金币。</summary>
    [Serializable]
    public sealed class D12PokerRewardRequest
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RequestId;
        public string RewardId;
        public string GameId;
        public string SourceEventId;
        public string PlayerId;
        public string RewardTag;
        public string Currency = D12PokerIds.Currency;
        public long CurrencyAmount;
        public D12PokerRequestStatus Status = D12PokerRequestStatus.Pending;

        public D12PokerRewardRequest Clone()
        {
            return new D12PokerRewardRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                RewardId = RewardId,
                GameId = GameId,
                SourceEventId = SourceEventId,
                PlayerId = PlayerId,
                RewardTag = RewardTag,
                Currency = Currency,
                CurrencyAmount = CurrencyAmount,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D12PokerRelationImpactRequest
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RequestId;
        public string SourceEventId;
        public string SourceId;
        public string TargetId;
        public string RelationType = "trust";
        public int ProposedDelta;
        public string Reason;
        public D12PokerRequestStatus Status = D12PokerRequestStatus.Pending;

        public D12PokerRelationImpactRequest Clone()
        {
            return new D12PokerRelationImpactRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                SourceEventId = SourceEventId,
                SourceId = SourceId,
                TargetId = TargetId,
                RelationType = RelationType,
                ProposedDelta = ProposedDelta,
                Reason = Reason,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D12PokerInformationRequest
    {
        public int SchemaVersion = D12PokerIds.CurrentSchemaVersion;
        public string RequestId;
        public string SourceEventId;
        public string PlayerId;
        public string InformationPoolId;
        public string InformationCode;
        public float Reliability;
        public D12PokerRequestStatus Status = D12PokerRequestStatus.Pending;

        public D12PokerInformationRequest Clone()
        {
            return new D12PokerInformationRequest
            {
                SchemaVersion = SchemaVersion,
                RequestId = RequestId,
                SourceEventId = SourceEventId,
                PlayerId = PlayerId,
                InformationPoolId = InformationPoolId,
                InformationCode = InformationCode,
                Reliability = Reliability,
                Status = Status,
            };
        }
    }

    [Serializable]
    public sealed class D12PokerRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D12PokerIds.CurrentSchemaVersion;

        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int LastWorldDay;
        public string LastEventId;
        public long NextGameSequence;
        public List<string> AppliedEventKeys = new List<string>();
        public List<D12PokerSessionState> Sessions = new List<D12PokerSessionState>();
        public List<D12PokerGameRecord> Records = new List<D12PokerGameRecord>();
        public List<D12PokerEconomyRequest> EconomyRequests = new List<D12PokerEconomyRequest>();
        public List<D12PokerDebtRequest> DebtRequests = new List<D12PokerDebtRequest>();
        public List<D12PokerRewardRequest> RewardRequests = new List<D12PokerRewardRequest>();
        public List<D12PokerRelationImpactRequest> RelationImpactRequests = new List<D12PokerRelationImpactRequest>();
        public List<D12PokerInformationRequest> InformationRequests = new List<D12PokerInformationRequest>();

        public D12PokerRuntimeSnapshot Clone()
        {
            var result = new D12PokerRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                Revision = Revision,
                LastWorldDay = LastWorldDay,
                LastEventId = LastEventId,
                NextGameSequence = NextGameSequence,
            };
            CopyStrings(result.AppliedEventKeys, AppliedEventKeys);
            Copy(result.Sessions, Sessions);
            Copy(result.Records, Records);
            Copy(result.EconomyRequests, EconomyRequests);
            Copy(result.DebtRequests, DebtRequests);
            Copy(result.RewardRequests, RewardRequests);
            Copy(result.RelationImpactRequests, RelationImpactRequests);
            Copy(result.InformationRequests, InformationRequests);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 Poker schema_version 不匹配：" + SchemaVersion);
            if (Revision < 0 || LastWorldDay < 0 || NextGameSequence < 0)
                throw new InvalidOperationException("D12 Poker revision/day/sequence 不能为负数。");
            ValidateUnique(AppliedEventKeys, "applied_event_key");
            ValidateUniqueSessions(Sessions);
            ValidateUniqueIds(Records, delegate(D12PokerGameRecord value) { return value.RecordId; }, "record_id");
            ValidateUniqueIds(EconomyRequests, delegate(D12PokerEconomyRequest value) { return value.RequestId; }, "economy_request_id");
            ValidateUniqueIds(DebtRequests, delegate(D12PokerDebtRequest value) { return value.RequestId; }, "debt_request_id");
            ValidateUniqueIds(RewardRequests, delegate(D12PokerRewardRequest value) { return value.RequestId; }, "reward_request_id");
            ValidateUniqueIds(RelationImpactRequests, delegate(D12PokerRelationImpactRequest value) { return value.RequestId; }, "relation_request_id");
            ValidateUniqueIds(InformationRequests, delegate(D12PokerInformationRequest value) { return value.RequestId; }, "information_request_id");
            if (Sessions != null)
                foreach (var session in Sessions) ValidateSession(session);
            if (Records != null)
                foreach (var record in Records) ValidateRecord(record);
        }

        private static void ValidateUniqueSessions(List<D12PokerSessionState> values)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (values == null) return;
            foreach (var value in values)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.TableId))
                    throw new InvalidOperationException("D12 Poker session 缺少 table_id。");
                if (!keys.Add(value.TableId)) throw new InvalidOperationException("D12 Poker session 重复：" + value.TableId);
            }
        }

        private static void ValidateSession(D12PokerSessionState value)
        {
            if (value.SchemaVersion != CurrentSchemaVersion) throw new InvalidOperationException("D12 Poker session 版本非法：" + value.GameId);
            Require(value.GameId, "game_id");
            Require(value.PlayerId, "player_id");
            Require(value.RulesetId, "ruleset_id");
            if (value.EntryFee < 0 || value.BetCap < 0 || value.CurrentBet < 0 || value.ProposedDebt < 0)
                throw new InvalidOperationException("D12 Poker session 金额不能为负数：" + value.GameId);
            if (value.CurrentBet > value.BetCap) throw new InvalidOperationException("D12 Poker session 超过下注上限：" + value.GameId);
            if (!Enum.IsDefined(typeof(D12PokerTableStatus), value.Status) ||
                !Enum.IsDefined(typeof(D12PokerOutcome), value.Outcome) ||
                !Enum.IsDefined(typeof(D12PokerCheatRisk), value.CheatRisk))
                throw new InvalidOperationException("D12 Poker session 枚举非法：" + value.GameId);
            ValidateCards(value.PlayerHand, value.GameId + ":player");
            ValidateCards(value.HouseHand, value.GameId + ":house");
        }

        private static void ValidateRecord(D12PokerGameRecord value)
        {
            if (value == null || value.SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D12 Poker record 版本非法。");
            Require(value.RecordId, "record_id");
            Require(value.GameId, "game_id");
            Require(value.SourceEventId, "source_event_id");
            if (value.EntryFee < 0 || value.Bet < 0 || value.Payout < 0)
                throw new InvalidOperationException("D12 Poker record 金额不能为负数：" + value.RecordId);
            if (!Enum.IsDefined(typeof(D12PokerOutcome), value.Outcome) ||
                !Enum.IsDefined(typeof(D12PokerHandCategory), value.PlayerHandCategory) ||
                !Enum.IsDefined(typeof(D12PokerHandCategory), value.HouseHandCategory) ||
                !Enum.IsDefined(typeof(D12PokerCheatRisk), value.CheatRisk))
                throw new InvalidOperationException("D12 Poker record 枚举非法：" + value.RecordId);
        }

        private static void ValidateCards(List<D12PokerCard> values, string scope)
        {
            if (values == null) throw new InvalidOperationException("D12 Poker cards 缺失：" + scope);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var card in values)
            {
                if (card == null) throw new InvalidOperationException("D12 Poker cards 不能为 null：" + scope);
                card.Validate();
                if (!ids.Add(card.CardId)) throw new InvalidOperationException("D12 Poker card 重复：" + card.CardId);
            }
        }

        private static void ValidateUnique(List<string> values, string name)
        {
            if (values == null) return;
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value) || !keys.Add(value)) throw new InvalidOperationException("重复或空的 " + name + "。");
        }

        private static void ValidateUniqueIds<T>(List<T> values, Func<T, string> selector, string name)
        {
            if (values == null) return;
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                var id = value == null ? null : selector(value);
                if (string.IsNullOrWhiteSpace(id) || !keys.Add(id)) throw new InvalidOperationException("重复或空的 " + name + "。");
            }
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("D12 Poker 缺少 " + name + "。");
        }

        private static void CopyStrings(List<string> target, List<string> source)
        {
            if (source != null) target.AddRange(source);
        }

        private static void Copy<T>(List<T> target, List<T> source) where T : class
        {
            if (source == null) return;
            foreach (var value in source)
                if (value != null) target.Add(CloneValue(value));
        }

        private static T CloneValue<T>(T value) where T : class
        {
            if (value is D12PokerSessionState) return (T)(object)((D12PokerSessionState)(object)value).Clone();
            if (value is D12PokerGameRecord) return (T)(object)((D12PokerGameRecord)(object)value).Clone();
            if (value is D12PokerEconomyRequest) return (T)(object)((D12PokerEconomyRequest)(object)value).Clone();
            if (value is D12PokerDebtRequest) return (T)(object)((D12PokerDebtRequest)(object)value).Clone();
            if (value is D12PokerRewardRequest) return (T)(object)((D12PokerRewardRequest)(object)value).Clone();
            if (value is D12PokerRelationImpactRequest) return (T)(object)((D12PokerRelationImpactRequest)(object)value).Clone();
            if (value is D12PokerInformationRequest) return (T)(object)((D12PokerInformationRequest)(object)value).Clone();
            throw new InvalidOperationException("未知 D12 Poker 可复制类型：" + typeof(T).Name);
        }
    }

    [Serializable]
    public sealed class D12PokerActionResult
    {
        public bool Accepted;
        public string Code;
        public string Message;
        public D12PokerTableStatus Status;
        public D12PokerOutcome Outcome;
        public D12PokerCheatRisk CheatRisk;
        public D12PokerSessionState Session;
        public D12PokerGameRecord Record;
        public D12PokerEconomyRequest EconomyRequest;
        public D12PokerDebtRequest DebtRequest;
        public D12PokerRewardRequest RewardRequest;
        public D12PokerRelationImpactRequest RelationRequest;
        public D12PokerInformationRequest InformationRequest;
        public FactEvent Fact;

        public bool IsDuplicate { get { return Code == "duplicate_action" || Code == "duplicate_settlement"; } }

        internal static D12PokerActionResult Rejected(string code, string message)
        {
            return new D12PokerActionResult { Accepted = false, Code = code, Message = message };
        }
    }
}
