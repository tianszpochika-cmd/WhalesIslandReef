// ============================================================
// D12TarotContracts —— 牌面、软线索、信任与求证的可保存契约
//
// 牌面由 D12TarotSystem 确定；解释文本是只读表现输入。任何天气、任务、
// 奖励、金钱或关系结果仍由各自系统裁决，本文件不包含这些领域写入口。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D9;

namespace Whalebone.Systems.D12
{
    public enum D12TarotOrientation
    {
        Upright,
        Reversed,
    }

    public enum D12TarotSensitivity
    {
        None,
        ReligiousPractice,
    }

    public enum D12TarotClueStatus
    {
        Unverified,
        Confirmed,
        Contradicted,
        Unresolved,
        Skipped,
    }

    public enum D12TarotVerificationMethod
    {
        WeatherObservation,
        EventRecord,
        WitnessRecord,
    }

    public enum D12TarotTrustEventKind
    {
        SessionRespect,
        WarningSkipped,
        ClueConfirmed,
        ClueContradicted,
        Mockery,
        Coerced,
    }

    [Serializable]
    public sealed class D12TarotCardDefinition
    {
        public string CardId;
        public string DisplayName;
        public string DisplayNameKey;
        public string Arcana;
        public List<string> MeaningTags = new List<string>();
        public string UprightHint;
        public string ReversedHint;
        public string SourceRef;
        public string AuthorizationRef;
        public string LicenseRef;
        public string HistoricalTag;
        public string Phase;
        public string OwnerSystem;
        public string TestId;

        public D12TarotCardDefinition Clone()
        {
            var result = new D12TarotCardDefinition
            {
                CardId = CardId,
                DisplayName = DisplayName,
                DisplayNameKey = DisplayNameKey,
                Arcana = Arcana,
                UprightHint = UprightHint,
                ReversedHint = ReversedHint,
                SourceRef = SourceRef,
                AuthorizationRef = AuthorizationRef,
                LicenseRef = LicenseRef,
                HistoricalTag = HistoricalTag,
                Phase = Phase,
                OwnerSystem = OwnerSystem,
                TestId = TestId,
            };
            if (MeaningTags != null) result.MeaningTags.AddRange(MeaningTags);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(CardId)) throw new InvalidOperationException("牌面缺少 card_id。");
            if (string.IsNullOrWhiteSpace(DisplayNameKey)) throw new InvalidOperationException("牌面缺少本地化 key：" + CardId);
            if (MeaningTags == null || MeaningTags.Count == 0) throw new InvalidOperationException("牌面缺少 meaning_tags：" + CardId);
            if (string.IsNullOrWhiteSpace(SourceRef)) throw new InvalidOperationException("牌面缺少来源：" + CardId);
            if (string.IsNullOrWhiteSpace(AuthorizationRef)) throw new InvalidOperationException("牌面缺少授权引用：" + CardId);
            if (string.IsNullOrWhiteSpace(LicenseRef)) throw new InvalidOperationException("牌面缺少许可：" + CardId);
            if (string.IsNullOrWhiteSpace(HistoricalTag)) throw new InvalidOperationException("牌面缺少历史标签：" + CardId);
            if (string.IsNullOrWhiteSpace(Phase)) throw new InvalidOperationException("牌面缺少阶段：" + CardId);
            if (string.IsNullOrWhiteSpace(OwnerSystem)) throw new InvalidOperationException("牌面缺少所属系统：" + CardId);
            if (string.IsNullOrWhiteSpace(TestId)) throw new InvalidOperationException("牌面缺少 test_id：" + CardId);
        }
    }

    [Serializable]
    public sealed class D12DivinerDefinition
    {
        public string DivinerId;
        public string DisplayName;
        public string CultureId;
        public string ReligionId;
        public string SourceRef;
        public string AuthorizationRef;
        public string LicenseRef;
        public string HistoricalTag;
        public string WarningId;
        public string WarningText;
        public D12TarotSensitivity Sensitivity;
        public int BaseTrust;
        public string TrustSourceRef;
        public string TrustEvidenceRef;
        public string TestId;
        public bool SkipAllowed;

        public D12DivinerDefinition Clone()
        {
            return new D12DivinerDefinition
            {
                DivinerId = DivinerId,
                DisplayName = DisplayName,
                CultureId = CultureId,
                ReligionId = ReligionId,
                SourceRef = SourceRef,
                AuthorizationRef = AuthorizationRef,
                LicenseRef = LicenseRef,
                HistoricalTag = HistoricalTag,
                WarningId = WarningId,
                WarningText = WarningText,
                Sensitivity = Sensitivity,
                BaseTrust = BaseTrust,
                TrustSourceRef = TrustSourceRef,
                TrustEvidenceRef = TrustEvidenceRef,
                TestId = TestId,
                SkipAllowed = SkipAllowed,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DivinerId)) throw new InvalidOperationException("占卜者缺少 diviner_id。");
            if (string.IsNullOrWhiteSpace(CultureId)) throw new InvalidOperationException("占卜者缺少 culture_id：" + DivinerId);
            if (string.IsNullOrWhiteSpace(ReligionId)) throw new InvalidOperationException("占卜者缺少 religion_id：" + DivinerId);
            if (string.IsNullOrWhiteSpace(SourceRef)) throw new InvalidOperationException("占卜者缺少来源：" + DivinerId);
            if (string.IsNullOrWhiteSpace(AuthorizationRef)) throw new InvalidOperationException("占卜者缺少授权引用：" + DivinerId);
            if (string.IsNullOrWhiteSpace(LicenseRef)) throw new InvalidOperationException("占卜者缺少许可：" + DivinerId);
            if (string.IsNullOrWhiteSpace(HistoricalTag)) throw new InvalidOperationException("占卜者缺少历史标签：" + DivinerId);
            if (string.IsNullOrWhiteSpace(TrustSourceRef) || string.IsNullOrWhiteSpace(TrustEvidenceRef))
                throw new InvalidOperationException("占卜者信任基线缺少来源或证据：" + DivinerId);
            if (BaseTrust < 0 || BaseTrust > 100) throw new InvalidOperationException("占卜者基线信任必须在 0..100：" + DivinerId);
            if (!Enum.IsDefined(typeof(D12TarotSensitivity), Sensitivity)) throw new InvalidOperationException("占卜者敏感度非法：" + DivinerId);
            if (Sensitivity != D12TarotSensitivity.None && string.IsNullOrWhiteSpace(WarningId))
                throw new InvalidOperationException("敏感占卜者缺少 warning_id：" + DivinerId);
            if (string.IsNullOrWhiteSpace(TestId)) throw new InvalidOperationException("占卜者缺少 test_id：" + DivinerId);
        }
    }

    [Serializable]
    public sealed class D12TarotCardDraw
    {
        public string DrawId;
        public string CardId;
        public D12TarotOrientation Orientation;
        public int Position;
        public string MeaningTag;
        public string SoftSignal;

        public D12TarotCardDraw Clone()
        {
            return new D12TarotCardDraw
            {
                DrawId = DrawId,
                CardId = CardId,
                Orientation = Orientation,
                Position = Position,
                MeaningTag = MeaningTag,
                SoftSignal = SoftSignal,
            };
        }
    }

    [Serializable]
    public sealed class D12SoftClue
    {
        public string ClueId;
        public string SessionId;
        public string Kind;
        public string TargetRef;
        public string Signal;
        public float Reliability;
        public string BiasCode;
        public D12TarotClueStatus Status = D12TarotClueStatus.Unverified;
        public string EvidenceEventId;
        public string SourceRef;
        public string EvidenceRef;
        public int WorldDay;
        public long SaveRevision;
        public bool Verified;

        public D12SoftClue Clone()
        {
            return new D12SoftClue
            {
                ClueId = ClueId,
                SessionId = SessionId,
                Kind = Kind,
                TargetRef = TargetRef,
                Signal = Signal,
                Reliability = Reliability,
                BiasCode = BiasCode,
                Status = Status,
                EvidenceEventId = EvidenceEventId,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Verified = Verified,
            };
        }
    }

    [Serializable]
    public sealed class D12DivinationTrustState
    {
        public string DivinerId;
        public string CultureId;
        public string ReligionId;
        public int Trust;
        public string SourceRef;
        public string EvidenceRef;
        public string LastEventId;
        public int LastWorldDay;
        public long LastSaveRevision;
        public List<string> AppliedEventKeys = new List<string>();

        public D12DivinationTrustState Clone()
        {
            var result = new D12DivinationTrustState
            {
                DivinerId = DivinerId,
                CultureId = CultureId,
                ReligionId = ReligionId,
                Trust = Trust,
                SourceRef = SourceRef,
                EvidenceRef = EvidenceRef,
                LastEventId = LastEventId,
                LastWorldDay = LastWorldDay,
                LastSaveRevision = LastSaveRevision,
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class D12TarotSessionState
    {
        public string SessionId;
        public string DivinerId;
        public string QueryTag;
        public string ActorRef;
        public string LocationRef;
        public int WorldDay;
        public long Seed;
        public D12TarotSensitivity Sensitivity;
        public bool WarningAcknowledged;
        public bool SkippedSensitiveContent;
        public List<D12TarotCardDraw> Cards = new List<D12TarotCardDraw>();
        public List<D12SoftClue> Clues = new List<D12SoftClue>();
        public string ExplanationText;
        public string ExplanationProvider;
        public string LastEventId;
        public long Revision;
        public List<string> AppliedEventKeys = new List<string>();

        public D12TarotSessionState Clone()
        {
            var result = new D12TarotSessionState
            {
                SessionId = SessionId,
                DivinerId = DivinerId,
                QueryTag = QueryTag,
                ActorRef = ActorRef,
                LocationRef = LocationRef,
                WorldDay = WorldDay,
                Seed = Seed,
                Sensitivity = Sensitivity,
                WarningAcknowledged = WarningAcknowledged,
                SkippedSensitiveContent = SkippedSensitiveContent,
                ExplanationText = ExplanationText,
                ExplanationProvider = ExplanationProvider,
                LastEventId = LastEventId,
                Revision = Revision,
            };
            if (Cards != null) foreach (var card in Cards) if (card != null) result.Cards.Add(card.Clone());
            if (Clues != null) foreach (var clue in Clues) if (clue != null) result.Clues.Add(clue.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }
    }

    [Serializable]
    public sealed class D12TarotVerificationState
    {
        public string VerificationId;
        public string SessionId;
        public string ClueId;
        public D12TarotVerificationMethod Method;
        public D12TarotClueStatus Outcome;
        public string EvidenceRef;
        public string SourceRef;
        public string FactEventId;
        public int WorldDay;
        public long SaveRevision;
        public string LastEventId;

        public D12TarotVerificationState Clone()
        {
            return new D12TarotVerificationState
            {
                VerificationId = VerificationId,
                SessionId = SessionId,
                ClueId = ClueId,
                Method = Method,
                Outcome = Outcome,
                EvidenceRef = EvidenceRef,
                SourceRef = SourceRef,
                FactEventId = FactEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D12TarotErrorConsequenceRecord
    {
        public string ConsequenceId;
        public string SessionId;
        public string ClueId;
        public string VerificationId;
        public string ImpactTag;
        public bool FollowedAdvice;
        public string CostBand;
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
        public string DownstreamOwner = "caller_domain_system";

        public D12TarotErrorConsequenceRecord Clone()
        {
            var result = new D12TarotErrorConsequenceRecord
            {
                ConsequenceId = ConsequenceId,
                SessionId = SessionId,
                ClueId = ClueId,
                VerificationId = VerificationId,
                ImpactTag = ImpactTag,
                FollowedAdvice = FollowedAdvice,
                CostBand = CostBand,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                EventId = EventId,
                DownstreamOwner = DownstreamOwner,
            };
            if (CauseEventIds != null) result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D12TarotRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public string LastEventId;
        public List<string> AppliedEventKeys = new List<string>();
        public List<D12TarotSessionState> Sessions = new List<D12TarotSessionState>();
        public List<D12DivinationTrustState> TrustStates = new List<D12DivinationTrustState>();
        public List<D12TarotVerificationState> Verifications = new List<D12TarotVerificationState>();
        public List<D12TarotErrorConsequenceRecord> Consequences = new List<D12TarotErrorConsequenceRecord>();

        public D12TarotRuntimeSnapshot Clone()
        {
            var result = new D12TarotRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            if (Sessions != null) foreach (var item in Sessions) if (item != null) result.Sessions.Add(item.Clone());
            if (TrustStates != null) foreach (var item in TrustStates) if (item != null) result.TrustStates.Add(item.Clone());
            if (Verifications != null) foreach (var item in Verifications) if (item != null) result.Verifications.Add(item.Clone());
            if (Consequences != null) foreach (var item in Consequences) if (item != null) result.Consequences.Add(item.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion) throw new InvalidOperationException("D12 塔罗快照 schema_version 不匹配：" + SchemaVersion);
            if (Revision < 0) throw new InvalidOperationException("D12 塔罗快照 revision 不能为负数。");
            if (AppliedEventKeys == null || Sessions == null || TrustStates == null || Verifications == null || Consequences == null)
                throw new InvalidOperationException("D12 塔罗快照集合不能为空。");
            ValidateUnique(AppliedEventKeys, "事件去重键");
            ValidateUniqueSessions();
            ValidateUniqueTrust();
            ValidateUniqueVerifications();
            ValidateUniqueConsequences();
        }

        private void ValidateUniqueSessions()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var session in Sessions)
            {
                if (session == null || string.IsNullOrWhiteSpace(session.SessionId)) throw new InvalidOperationException("D12 塔罗会话缺少 session_id。");
                if (!ids.Add(session.SessionId)) throw new InvalidOperationException("D12 塔罗会话重复：" + session.SessionId);
                if (session.WorldDay < 0 || session.Revision < 0) throw new InvalidOperationException("D12 塔罗会话时间或 revision 非法：" + session.SessionId);
                if (session.Cards == null || session.Clues == null) throw new InvalidOperationException("D12 塔罗会话缺少牌面或线索：" + session.SessionId);
                var clueIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (var clue in session.Clues)
                {
                    if (clue == null || string.IsNullOrWhiteSpace(clue.ClueId)) throw new InvalidOperationException("D12 软线索缺少 clue_id：" + session.SessionId);
                    if (!clueIds.Add(clue.ClueId)) throw new InvalidOperationException("D12 软线索重复：" + clue.ClueId);
                    if (clue.Reliability < 0f || clue.Reliability > 1f) throw new InvalidOperationException("D12 软线索可靠度非法：" + clue.ClueId);
                    if (!Enum.IsDefined(typeof(D12TarotClueStatus), clue.Status)) throw new InvalidOperationException("D12 软线索状态非法：" + clue.ClueId);
                }
            }
        }

        private void ValidateUniqueTrust()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var state in TrustStates)
            {
                if (state == null || string.IsNullOrWhiteSpace(state.DivinerId)) throw new InvalidOperationException("D12 占卜信任缺少 diviner_id。");
                if (!ids.Add(state.DivinerId)) throw new InvalidOperationException("D12 占卜信任重复：" + state.DivinerId);
                if (state.Trust < 0 || state.Trust > 100) throw new InvalidOperationException("D12 占卜信任超出范围：" + state.DivinerId);
                if (state.AppliedEventKeys == null) throw new InvalidOperationException("D12 占卜信任缺少事件键：" + state.DivinerId);
            }
        }

        private void ValidateUniqueVerifications()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Verifications)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.VerificationId)) throw new InvalidOperationException("D12 求证缺少 verification_id。");
                if (!ids.Add(item.VerificationId)) throw new InvalidOperationException("D12 求证重复：" + item.VerificationId);
                if (!Enum.IsDefined(typeof(D12TarotVerificationMethod), item.Method) || !Enum.IsDefined(typeof(D12TarotClueStatus), item.Outcome))
                    throw new InvalidOperationException("D12 求证枚举非法：" + item.VerificationId);
            }
        }

        private void ValidateUniqueConsequences()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Consequences)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.ConsequenceId)) throw new InvalidOperationException("D12 后果记录缺少 consequence_id。");
                if (!ids.Add(item.ConsequenceId)) throw new InvalidOperationException("D12 后果记录重复：" + item.ConsequenceId);
            }
        }

        private static void ValidateUnique(List<string> values, string field)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || !set.Add(value)) throw new InvalidOperationException("D12 " + field + "重复或为空：" + value);
            }
        }
    }

    [Serializable]
    public sealed class D12TarotSessionRequest
    {
        public string DivinerId;
        public string QueryTag;
        public string ActorRef = "player";
        public string LocationRef = "island_saint_marys";
        public int WorldDay;
        public long Seed;
        public long SaveRevision;
        public bool WarningAcknowledged;
        public bool SkipSensitiveContent;
        public string TrustSourceRef;
        public string TrustEvidenceRef;
        public string ActionId;
    }

    [Serializable]
    public sealed class D12TarotExplanationInput
    {
        public string SessionId;
        public string ExplanationText;
        public string Provider = "template";
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D12TarotVerificationRequest
    {
        public string SessionId;
        public string ClueId;
        public D12TarotVerificationMethod Method;
        public D9WeatherSeaSnapshot Weather;
        public string FactEventId;
        public string EvidenceRef;
        public string SourceRef;
        public int WorldDay;
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D12TarotConsequenceRequest
    {
        public string SessionId;
        public string ClueId;
        public string VerificationId;
        public bool FollowedAdvice;
        public int WorldDay;
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D12TarotTrustEventRequest
    {
        public string DivinerId;
        public D12TarotTrustEventKind Kind;
        public string SourceRef;
        public string EvidenceRef;
        public int WorldDay;
        public long SaveRevision;
        public string ActionId;
    }

    [Serializable]
    public sealed class D12TarotBias
    {
        public string BiasCode;
        public string CultureLens;
        public float ReliabilityModifier;
        public string Caution;
        public string SourceRef;

        public D12TarotBias Clone()
        {
            return new D12TarotBias
            {
                BiasCode = BiasCode,
                CultureLens = CultureLens,
                ReliabilityModifier = ReliabilityModifier,
                Caution = Caution,
                SourceRef = SourceRef,
            };
        }
    }

    [Serializable]
    public sealed class D12TarotExplanationContext
    {
        public string SessionId;
        public List<D12TarotCardDraw> Cards = new List<D12TarotCardDraw>();
        public List<D12SoftClue> SoftClues = new List<D12SoftClue>();
        public List<string> BiasCodes = new List<string>();
        public bool IsSoftOnly = true;
        public bool NoWorldMutation = true;

        public D12TarotExplanationContext Clone()
        {
            var result = new D12TarotExplanationContext { SessionId = SessionId, IsSoftOnly = IsSoftOnly, NoWorldMutation = NoWorldMutation };
            if (Cards != null) foreach (var card in Cards) if (card != null) result.Cards.Add(card.Clone());
            if (SoftClues != null) foreach (var clue in SoftClues) if (clue != null) result.SoftClues.Add(clue.Clone());
            if (BiasCodes != null) result.BiasCodes.AddRange(BiasCodes);
            return result;
        }
    }

    [Serializable]
    public sealed class D12TarotSessionResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public bool WarningRequired;
        public bool Skipped;
        public string Code;
        public string Message;
        public string SessionId;
        public string FactEventId;
        public List<D12TarotCardDraw> Cards = new List<D12TarotCardDraw>();
        public List<D12SoftClue> SoftClues = new List<D12SoftClue>();
        public D12DivinationTrustState Trust;
        public D12TarotExplanationContext ExplanationContext;
        public D12TarotSessionState State;
    }

    [Serializable]
    public sealed class D12TarotVerificationResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string VerificationId;
        public string FactEventId;
        public D12TarotClueStatus Outcome;
        public D12SoftClue Clue;
        public D12DivinationTrustState Trust;
    }

    [Serializable]
    public sealed class D12TarotConsequenceResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string ConsequenceId;
        public D12TarotErrorConsequenceRecord Consequence;
    }

    [Serializable]
    public sealed class D12TarotTrustResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string FactEventId;
        public D12DivinationTrustState Trust;
    }
}
