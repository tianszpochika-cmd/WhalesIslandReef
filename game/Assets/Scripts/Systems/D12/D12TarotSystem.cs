// ============================================================
// D12TarotSystem —— 非超自然的牌面、软线索与求证系统
//
// 本系统是牌面、占卜语境信任、解释偏差和软线索的唯一写者。它不会写天气、
// 价格、奖励、关系或任务结果；错误后果只记录可追溯标签，交给对应领域系统。
// 解释文本可以来自模板或 LLM，但只能进入 ExplanationText，不能进入裁决路径。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public sealed class D12TarotSystem
    {
        private const string DefaultActor = "player";
        private readonly EventLog _eventLog;
        private readonly D12TarotCatalog _catalog;
        private readonly D12TarotRuntimeSnapshot _snapshot;

        public D12TarotSystem(EventLog eventLog, D12TarotRuntimeSnapshot initial = null, D12TarotCatalog catalog = null)
        {
            _eventLog = eventLog ?? new EventLog();
            _catalog = catalog ?? new D12TarotCatalog();
            _snapshot = initial == null ? new D12TarotRuntimeSnapshot() : initial.Clone();
            _snapshot.Validate();
            foreach (var diviner in _catalog.Diviners)
                EnsureTrustState(diviner);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D12TarotCatalog Catalog { get { return _catalog; } }
        public D12TarotRuntimeSnapshot Snapshot { get { return _snapshot.Clone(); } }

        public D12TarotSessionResult StartSession(D12TarotSessionRequest request)
        {
            ValidateSessionRequest(request);
            var diviner = _catalog.FindDiviner(request.DivinerId);
            if (diviner == null) return FailureSession("unknown_diviner", "找不到占卜者：" + request.DivinerId);

            var sessionId = BuildSessionId(request);
            var existing = FindSession(sessionId);
            if (existing != null)
                return SessionFromState(existing, true, "duplicate_session", "该次占卜已记录，不重复抽牌。");

            if (diviner.Sensitivity != D12TarotSensitivity.None && !request.WarningAcknowledged && !request.SkipSensitiveContent)
            {
                return new D12TarotSessionResult
                {
                    WarningRequired = true,
                    Code = "warning_required",
                    Message = diviner.WarningText,
                    SessionId = sessionId,
                };
            }

            var trust = EnsureTrustState(diviner);
            var state = new D12TarotSessionState
            {
                SessionId = sessionId,
                DivinerId = diviner.DivinerId,
                QueryTag = request.QueryTag.Trim(),
                ActorRef = Clean(request.ActorRef, DefaultActor),
                LocationRef = Clean(request.LocationRef, "island_saint_marys"),
                WorldDay = request.WorldDay,
                Seed = request.Seed,
                Sensitivity = diviner.Sensitivity,
                WarningAcknowledged = request.WarningAcknowledged,
                SkippedSensitiveContent = request.SkipSensitiveContent,
                Revision = 1,
            };

            string sourceRef = Clean(request.TrustSourceRef, diviner.TrustSourceRef);
            string evidenceRef = Clean(request.TrustEvidenceRef, "event:d12_tarot_session:" + sessionId);
            if (request.SkipSensitiveContent)
            {
                state.LastEventId = D12TarotCatalog.EventPrefix + "skip_" + sessionId;
                state.AppliedEventKeys.Add(Dedupe(state.LastEventId, request.SaveRevision));
                var skipEvent = AppendFact(state.LastEventId, request.WorldDay, state.LocationRef, state.ActorRef,
                    "sensitive_content_skipped", "玩家跳过敏感宗教内容。", new[] { "warning:" + diviner.WarningId, "source:" + sourceRef, "evidence:" + evidenceRef }, null,
                    request.SaveRevision, new Dictionary<string, string>
                    {
                        { "sensitive", "true" }, { "skipped", "true" }, { "supernatural", "false" },
                        { "source_ref", sourceRef }, { "evidence_ref", evidenceRef }, { "soft_only", "true" },
                    });
                state.LastEventId = skipEvent.EventId;
                trust.SourceRef = sourceRef;
                trust.EvidenceRef = evidenceRef;
                trust.LastEventId = skipEvent.EventId;
                trust.LastWorldDay = request.WorldDay;
                trust.LastSaveRevision = request.SaveRevision;
                AddEventKey(trust.AppliedEventKeys, skipEvent.DedupeKey);
                AddEventKey(_snapshot.AppliedEventKeys, skipEvent.DedupeKey);
                _snapshot.Sessions.Add(state);
                _snapshot.LastEventId = skipEvent.EventId;
                _snapshot.Revision++;
                return SessionFromState(state, false, "sensitive_content_skipped", "已跳过敏感内容，未抽取牌面。");
            }

            DrawCards(state, trust);
            var cardIds = new List<string>();
            foreach (var card in state.Cards) cardIds.Add(card.CardId);
            string eventId = D12TarotCatalog.EventPrefix + "session_" + sessionId;
            var fact = AppendFact(eventId, request.WorldDay, state.LocationRef, state.ActorRef,
                "tarot_session_recorded", "牌面已由系统确定；解释只产生软线索。",
                new[] { "culture:" + diviner.CultureId, "religion:" + diviner.ReligionId, "source:" + sourceRef, "evidence:" + evidenceRef }, null,
                request.SaveRevision, new Dictionary<string, string>
                {
                    { "session_id", sessionId }, { "cards", string.Join(",", cardIds.ToArray()) },
                    { "supernatural", "false" }, { "soft_only", "true" }, { "world_mutation", "false" },
                    { "bias_code", GetBias(trust).BiasCode }, { "source_ref", sourceRef }, { "evidence_ref", evidenceRef },
                });

            state.LastEventId = fact.EventId;
            state.AppliedEventKeys.Add(fact.DedupeKey);
            trust.SourceRef = sourceRef;
            trust.EvidenceRef = evidenceRef;
            trust.LastEventId = fact.EventId;
            trust.LastWorldDay = request.WorldDay;
            trust.LastSaveRevision = request.SaveRevision;
            AddEventKey(trust.AppliedEventKeys, fact.DedupeKey);
            AddEventKey(_snapshot.AppliedEventKeys, fact.DedupeKey);
            _snapshot.Sessions.Add(state);
            _snapshot.LastEventId = fact.EventId;
            _snapshot.Revision++;
            return SessionFromState(state, false, "tarot_session_recorded", "牌面已记录；请把解释当作软线索并自行求证。", fact.EventId);
        }

        public D12TarotSessionResult AttachExplanation(D12TarotExplanationInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.SessionId)) return FailureSession("invalid_explanation", "解释缺少 session_id。");
            var session = FindSession(input.SessionId);
            if (session == null) return FailureSession("unknown_session", "找不到占卜会话：" + input.SessionId);
            if (string.IsNullOrWhiteSpace(input.ExplanationText)) return FailureSession("empty_explanation", "解释文本不能为空。");
            if (input.ExplanationText.Length > 4000) return FailureSession("explanation_too_long", "解释文本超过 4000 字符。");
            var provider = Clean(input.Provider, D12TarotCatalog.ExplanationProviderTemplate);
            if (!string.Equals(provider, D12TarotCatalog.ExplanationProviderTemplate, StringComparison.Ordinal) &&
                !string.Equals(provider, D12TarotCatalog.ExplanationProviderLlm, StringComparison.Ordinal) &&
                !string.Equals(provider, "player", StringComparison.Ordinal))
                return FailureSession("invalid_explanation_provider", "解释来源只能是 template、llm_text_only 或 player。");

            string eventId = D12TarotCatalog.EventPrefix + "explanation_" + input.SessionId + "_" + Clean(input.ActionId, "r" + session.Revision.ToString(CultureInfo.InvariantCulture));
            if (HasEvent(eventId, input.SaveRevision)) return SessionFromState(session, true, "duplicate_explanation", "该解释已记录，不重复写入。", eventId);
            var fact = AppendFact(eventId, session.WorldDay, session.LocationRef, session.ActorRef,
                "explanation_attached", "解释文本已附加；它不会修改牌面或世界状态。", new[] { "session:" + session.SessionId },
                new[] { session.LastEventId }, input.SaveRevision, new Dictionary<string, string>
                {
                    { "provider", provider }, { "affects_state", "false" }, { "llm_text_only", string.Equals(provider, D12TarotCatalog.ExplanationProviderLlm, StringComparison.Ordinal).ToString().ToLowerInvariant() },
                    { "soft_only", "true" },
                });
            session.ExplanationText = input.ExplanationText;
            session.ExplanationProvider = provider;
            session.LastEventId = fact.EventId;
            session.Revision++;
            AddEventKey(session.AppliedEventKeys, fact.DedupeKey);
            AddEventKey(_snapshot.AppliedEventKeys, fact.DedupeKey);
            _snapshot.LastEventId = fact.EventId;
            _snapshot.Revision++;
            return SessionFromState(session, false, "explanation_attached", "解释已保存为软线索文本。", fact.EventId);
        }

        public D12TarotVerificationResult Verify(D12TarotVerificationRequest request)
        {
            ValidateVerificationRequest(request);
            var session = FindSession(request.SessionId);
            if (session == null) return FailureVerification("unknown_session", "找不到占卜会话：" + request.SessionId);
            var clue = FindClue(session, request.ClueId);
            if (clue == null) return FailureVerification("unknown_clue", "找不到软线索：" + request.ClueId);
            if (clue.Status != D12TarotClueStatus.Unverified) return VerificationFromClue(clue, true, "duplicate_verification", "该线索已经求证。");

            D12TarotClueStatus outcome = DetermineVerification(clue, request);
            string verificationId = "verification_" + session.SessionId + "_" + clue.ClueId;
            string eventId = D12TarotCatalog.EventPrefix + "verify_" + verificationId + "_" + Clean(request.ActionId, request.Method.ToString().ToLowerInvariant());
            if (HasEvent(eventId, request.SaveRevision)) return VerificationFromClue(clue, true, "duplicate_verification", "该求证事件已经记录。", eventId);
            var trust = FindTrust(session.DivinerId);
            int before = trust.Trust;
            int delta = outcome == D12TarotClueStatus.Confirmed ? 2 : outcome == D12TarotClueStatus.Contradicted ? -4 : 0;
            string factId = request.FactEventId ?? string.Empty;
            var fact = AppendFact(eventId, request.WorldDay, session.LocationRef, session.ActorRef,
                "tarot_clue_verified", "软线索求证结果已记录；没有写入天气、关系、任务或奖励。",
                new[] { "session:" + session.SessionId, "clue:" + clue.ClueId, "evidence:" + request.EvidenceRef },
                string.IsNullOrWhiteSpace(factId) ? new[] { session.LastEventId } : new[] { session.LastEventId, factId },
                request.SaveRevision, new Dictionary<string, string>
                {
                    { "outcome", outcome.ToString().ToLowerInvariant() }, { "method", request.Method.ToString() },
                    { "source_ref", request.SourceRef }, { "evidence_ref", request.EvidenceRef },
                    { "fact_event_id", factId }, { "trust_before", before.ToString(CultureInfo.InvariantCulture) },
                    { "trust_delta", delta.ToString(CultureInfo.InvariantCulture) }, { "soft_only", "true" },
                });
            clue.Status = outcome;
            clue.Verified = outcome != D12TarotClueStatus.Unresolved;
            clue.EvidenceEventId = factId;
            clue.SourceRef = request.SourceRef;
            clue.EvidenceRef = request.EvidenceRef;
            var verification = new D12TarotVerificationState
            {
                VerificationId = verificationId,
                SessionId = session.SessionId,
                ClueId = clue.ClueId,
                Method = request.Method,
                Outcome = outcome,
                EvidenceRef = request.EvidenceRef,
                SourceRef = request.SourceRef,
                FactEventId = factId,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                LastEventId = fact.EventId,
            };
            trust.Trust = ClampTrust(trust.Trust + delta);
            trust.SourceRef = request.SourceRef;
            trust.EvidenceRef = request.EvidenceRef;
            trust.LastEventId = fact.EventId;
            trust.LastWorldDay = request.WorldDay;
            trust.LastSaveRevision = request.SaveRevision;
            AddEventKey(trust.AppliedEventKeys, fact.DedupeKey);
            AddEventKey(session.AppliedEventKeys, fact.DedupeKey);
            AddEventKey(_snapshot.AppliedEventKeys, fact.DedupeKey);
            _snapshot.Verifications.Add(verification);
            session.LastEventId = fact.EventId;
            session.Revision++;
            _snapshot.LastEventId = fact.EventId;
            _snapshot.Revision++;
            return new D12TarotVerificationResult
            {
                Succeeded = true,
                Code = "tarot_clue_verified",
                Message = "软线索求证结果已记录。",
                VerificationId = verificationId,
                FactEventId = fact.EventId,
                Outcome = outcome,
                Clue = clue.Clone(),
                Trust = trust.Clone(),
            };
        }

        public D12TarotConsequenceResult RecordConsequence(D12TarotConsequenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.ClueId))
                return FailureConsequence("invalid_consequence", "错误后果缺少 session_id 或 clue_id。");
            var session = FindSession(request.SessionId);
            if (session == null) return FailureConsequence("unknown_session", "找不到占卜会话：" + request.SessionId);
            var clue = FindClue(session, request.ClueId);
            if (clue == null) return FailureConsequence("unknown_clue", "找不到软线索：" + request.ClueId);
            var verification = FindVerification(request.VerificationId, session.SessionId, clue.ClueId);
            if (verification == null) return FailureConsequence("verification_required", "记录错误后果前必须先求证：" + clue.ClueId);

            string outcome = verification.Outcome.ToString().ToLowerInvariant();
            string impact = !request.FollowedAdvice ? "advice_declined_no_domain_write" :
                verification.Outcome == D12TarotClueStatus.Contradicted ? "misinterpretation_recorded" :
                verification.Outcome == D12TarotClueStatus.Unresolved ? "uncertainty_cost_recorded" : "verified_guidance_recorded";
            string cost = !request.FollowedAdvice || verification.Outcome == D12TarotClueStatus.Confirmed ? "none" :
                verification.Outcome == D12TarotClueStatus.Contradicted ? "recheck_time" : "evidence_delay";
            string consequenceId = "consequence_" + session.SessionId + "_" + clue.ClueId + "_" + verification.VerificationId;
            string eventId = D12TarotCatalog.EventPrefix + "consequence_" + consequenceId + "_" + Clean(request.ActionId, "record");
            if (HasEvent(eventId, request.SaveRevision)) return ConsequenceFromState(consequenceId, true, "duplicate_consequence", "该错误后果已经记录。", eventId);
            var record = new D12TarotErrorConsequenceRecord
            {
                ConsequenceId = consequenceId,
                SessionId = session.SessionId,
                ClueId = clue.ClueId,
                VerificationId = verification.VerificationId,
                ImpactTag = impact,
                FollowedAdvice = request.FollowedAdvice,
                CostBand = cost,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
                EventId = eventId,
                DownstreamOwner = "caller_domain_system",
            };
            record.CauseEventIds.Add(verification.LastEventId);
            var fact = AppendFact(eventId, request.WorldDay, session.LocationRef, session.ActorRef,
                "tarot_error_consequence_recorded", "占卜解释后果已记录为可追溯标签，实际领域结算由对应系统负责。",
                new[] { "verification:" + verification.VerificationId }, record.CauseEventIds, request.SaveRevision,
                new Dictionary<string, string>
                {
                    { "impact_tag", impact }, { "cost_band", cost }, { "outcome", outcome },
                    { "followed_advice", request.FollowedAdvice.ToString().ToLowerInvariant() },
                    { "downstream_owner", record.DownstreamOwner }, { "world_mutation", "false" },
                });
            record.EventId = fact.EventId;
            _snapshot.Consequences.Add(record);
            AddEventKey(_snapshot.AppliedEventKeys, fact.DedupeKey);
            session.LastEventId = fact.EventId;
            session.Revision++;
            _snapshot.LastEventId = fact.EventId;
            _snapshot.Revision++;
            return new D12TarotConsequenceResult
            {
                Succeeded = true,
                Code = "tarot_error_consequence_recorded",
                Message = "后果标签已记录；未直接改变任何领域数值。",
                ConsequenceId = consequenceId,
                Consequence = record.Clone(),
            };
        }

        public D12TarotTrustResult RecordTrustEvent(D12TarotTrustEventRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DivinerId) || string.IsNullOrWhiteSpace(request.SourceRef) || string.IsNullOrWhiteSpace(request.EvidenceRef))
                return new D12TarotTrustResult { Code = "trust_source_required", Message = "信任事件必须包含来源和证据引用。" };
            if (!Enum.IsDefined(typeof(D12TarotTrustEventKind), request.Kind)) return new D12TarotTrustResult { Code = "invalid_trust_kind", Message = "信任事件类型非法。" };
            var diviner = _catalog.FindDiviner(request.DivinerId);
            if (diviner == null) return new D12TarotTrustResult { Code = "unknown_diviner", Message = "找不到占卜者：" + request.DivinerId };
            var trust = EnsureTrustState(diviner);
            string eventId = D12TarotCatalog.EventPrefix + "trust_" + diviner.DivinerId + "_" + request.Kind.ToString().ToLowerInvariant() + "_" + Clean(request.ActionId, "record");
            if (HasEvent(eventId, request.SaveRevision)) return new D12TarotTrustResult { Duplicate = true, Code = "duplicate_trust_event", Message = "该信任事件已经记录。", FactEventId = eventId, Trust = trust.Clone() };
            int before = trust.Trust;
            int delta = TrustDelta(request.Kind);
            var fact = AppendFact(eventId, request.WorldDay, "island_saint_marys", DefaultActor,
                "divination_trust_changed", "占卜语境信任变化已记录，未写入 D10 关系值。",
                new[] { "diviner:" + diviner.DivinerId, "source:" + request.SourceRef, "evidence:" + request.EvidenceRef }, null,
                request.SaveRevision, new Dictionary<string, string>
                {
                    { "trust_before", before.ToString(CultureInfo.InvariantCulture) },
                    { "trust_delta", delta.ToString(CultureInfo.InvariantCulture) },
                    { "trust_after", ClampTrust(before + delta).ToString(CultureInfo.InvariantCulture) },
                    { "source_ref", request.SourceRef }, { "evidence_ref", request.EvidenceRef },
                    { "relation_write", "false" },
                });
            trust.Trust = ClampTrust(before + delta);
            trust.SourceRef = request.SourceRef;
            trust.EvidenceRef = request.EvidenceRef;
            trust.LastEventId = fact.EventId;
            trust.LastWorldDay = request.WorldDay;
            trust.LastSaveRevision = request.SaveRevision;
            AddEventKey(trust.AppliedEventKeys, fact.DedupeKey);
            AddEventKey(_snapshot.AppliedEventKeys, fact.DedupeKey);
            _snapshot.LastEventId = fact.EventId;
            _snapshot.Revision++;
            return new D12TarotTrustResult { Succeeded = true, Code = "divination_trust_changed", Message = "占卜语境信任已记录。", FactEventId = fact.EventId, Trust = trust.Clone() };
        }

        public D12DivinationTrustState ReadDivinerTrust(string divinerId)
        {
            var state = FindTrust(divinerId);
            return state == null ? null : state.Clone();
        }

        public D12TarotExplanationContext BuildExplanationContext(string sessionId)
        {
            var session = FindSession(sessionId);
            if (session == null) return null;
            var result = new D12TarotExplanationContext { SessionId = session.SessionId };
            foreach (var card in session.Cards) result.Cards.Add(card.Clone());
            foreach (var clue in session.Clues)
            {
                result.SoftClues.Add(clue.Clone());
                if (!result.BiasCodes.Contains(clue.BiasCode)) result.BiasCodes.Add(clue.BiasCode);
            }
            return result;
        }

        private void DrawCards(D12TarotSessionState state, D12DivinationTrustState trust)
        {
            var used = new HashSet<int>();
            var bias = GetBias(trust);
            for (int position = 0; position < 3; position++)
            {
                uint hash = StableHash(state.DivinerId + "|" + state.QueryTag + "|" + state.WorldDay.ToString(CultureInfo.InvariantCulture) + "|" + state.Seed.ToString(CultureInfo.InvariantCulture) + "|" + position.ToString(CultureInfo.InvariantCulture));
                int index = (int)(hash % (uint)_catalog.Cards.Count);
                while (used.Contains(index)) index = (index + 1) % _catalog.Cards.Count;
                used.Add(index);
                var definition = _catalog.Cards[index];
                string tag = definition.MeaningTags[0];
                string signal = definition.MeaningTags[2];
                var orientation = (hash & 1u) == 0u ? D12TarotOrientation.Upright : D12TarotOrientation.Reversed;
                var draw = new D12TarotCardDraw
                {
                    DrawId = state.SessionId + "_draw_" + position.ToString(CultureInfo.InvariantCulture),
                    CardId = definition.CardId,
                    Orientation = orientation,
                    Position = position,
                    MeaningTag = tag,
                    SoftSignal = signal,
                };
                state.Cards.Add(draw);
                state.Clues.Add(new D12SoftClue
                {
                    ClueId = state.SessionId + "_clue_" + position.ToString(CultureInfo.InvariantCulture),
                    SessionId = state.SessionId,
                    Kind = tag,
                    TargetRef = signal,
                    Signal = orientation == D12TarotOrientation.Upright ? definition.UprightHint : definition.ReversedHint,
                    Reliability = Clamp01(0.65f * bias.ReliabilityModifier),
                    BiasCode = bias.BiasCode,
                    SourceRef = definition.SourceRef,
                    EvidenceRef = "session:" + state.SessionId,
                    WorldDay = state.WorldDay,
                    SaveRevision = 0,
                });
            }
        }

        private D12TarotClueStatus DetermineVerification(D12SoftClue clue, D12TarotVerificationRequest request)
        {
            if (request.Method == D12TarotVerificationMethod.EventRecord || request.Method == D12TarotVerificationMethod.WitnessRecord)
                return string.IsNullOrWhiteSpace(request.FactEventId) || _eventLog.Find(request.FactEventId, request.SaveRevision) == null
                    ? D12TarotClueStatus.Unresolved : D12TarotClueStatus.Confirmed;
            if (request.Weather == null) return D12TarotClueStatus.Unresolved;
            if (clue.TargetRef.IndexOf("weather:review_visibility", StringComparison.Ordinal) >= 0)
                return request.Weather.Visibility < 0.75f ? D12TarotClueStatus.Confirmed : D12TarotClueStatus.Contradicted;
            if (clue.TargetRef.IndexOf("navigation:review_route", StringComparison.Ordinal) >= 0)
                return request.Weather.Weather == D9WeatherState.Storm || request.Weather.Visibility < 0.70f
                    ? D12TarotClueStatus.Confirmed : D12TarotClueStatus.Contradicted;
            if (clue.TargetRef.IndexOf("ship:inspect_before_commit", StringComparison.Ordinal) >= 0)
                return request.Weather.HullRisk > 0.20f ? D12TarotClueStatus.Confirmed : D12TarotClueStatus.Contradicted;
            return D12TarotClueStatus.Unresolved;
        }

        private D12TarotBias GetBias(D12DivinationTrustState trust)
        {
            if (trust.Trust < 35) return new D12TarotBias { BiasCode = "cautious_distance", CultureLens = trust.CultureId, ReliabilityModifier = 0.55f, Caution = "信任不足，解释应优先求证。", SourceRef = trust.SourceRef };
            if (trust.Trust > 70) return new D12TarotBias { BiasCode = "trusted_rapport", CultureLens = trust.CultureId, ReliabilityModifier = 0.82f, Caution = "熟悉语境提高可读性，但仍不是预言。", SourceRef = trust.SourceRef };
            return new D12TarotBias { BiasCode = "cultural_lens", CultureLens = trust.CultureId, ReliabilityModifier = 0.70f, Caution = "解释带有文化语境偏差，必须求证。", SourceRef = trust.SourceRef };
        }

        private FactEvent AppendFact(string eventId, int day, string location, string actor, string code, string summary,
            IEnumerable<string> inputFacts, IEnumerable<string> causes, long saveRevision, Dictionary<string, string> values)
        {
            var result = new EventResult { ResultCode = code, Summary = summary };
            result.FactIds.Add("tarot:" + eventId);
            if (values != null) foreach (var pair in values) result.Values[pair.Key] = pair.Value;
            var worldEvent = new WorldEvent(eventId, day, Clean(location, "island_saint_marys"), D12TarotCatalog.SystemOwner,
                result, new[] { Clean(actor, DefaultActor) }, inputFacts, causes, saveRevision);
            return _eventLog.Append(worldEvent).Fact;
        }

        private D12DivinationTrustState EnsureTrustState(D12DivinerDefinition diviner)
        {
            var existing = FindTrust(diviner.DivinerId);
            if (existing != null) return existing;
            var state = new D12DivinationTrustState
            {
                DivinerId = diviner.DivinerId,
                CultureId = diviner.CultureId,
                ReligionId = diviner.ReligionId,
                Trust = diviner.BaseTrust,
                SourceRef = diviner.TrustSourceRef,
                EvidenceRef = diviner.TrustEvidenceRef,
            };
            _snapshot.TrustStates.Add(state);
            return state;
        }

        private D12DivinationTrustState FindTrust(string divinerId)
        {
            foreach (var state in _snapshot.TrustStates)
                if (state != null && string.Equals(state.DivinerId, divinerId, StringComparison.Ordinal)) return state;
            return null;
        }

        private D12TarotSessionState FindSession(string sessionId)
        {
            foreach (var session in _snapshot.Sessions)
                if (session != null && string.Equals(session.SessionId, sessionId, StringComparison.Ordinal)) return session;
            return null;
        }

        private static D12SoftClue FindClue(D12TarotSessionState session, string clueId)
        {
            foreach (var clue in session.Clues)
                if (clue != null && string.Equals(clue.ClueId, clueId, StringComparison.Ordinal)) return clue;
            return null;
        }

        private D12TarotVerificationState FindVerification(string verificationId, string sessionId, string clueId)
        {
            foreach (var item in _snapshot.Verifications)
                if (item != null && (string.IsNullOrWhiteSpace(verificationId) || string.Equals(item.VerificationId, verificationId, StringComparison.Ordinal)) &&
                    string.Equals(item.SessionId, sessionId, StringComparison.Ordinal) && string.Equals(item.ClueId, clueId, StringComparison.Ordinal)) return item;
            return null;
        }

        private bool HasEvent(string eventId, long revision)
        {
            return _eventLog.Find(eventId, revision) != null || _snapshot.AppliedEventKeys.Contains(Dedupe(eventId, revision));
        }

        private static string BuildSessionId(D12TarotSessionRequest request)
        {
            return "session_" + request.DivinerId + "_" + request.WorldDay.ToString(CultureInfo.InvariantCulture) + "_" + StableHash(request.QueryTag + "|" + request.Seed.ToString(CultureInfo.InvariantCulture) + "|" + Clean(request.ActionId, "default")).ToString("x8");
        }

        private static string Dedupe(string eventId, long revision)
        {
            return eventId + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
        }

        private static uint StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < value.Length; i++) { hash ^= value[i]; hash *= 16777619u; }
                return hash;
            }
        }

        private static string Clean(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static float Clamp01(float value) { return value < 0f ? 0f : value > 1f ? 1f : value; }
        private static int ClampTrust(int value) { return value < 0 ? 0 : value > 100 ? 100 : value; }
        private static int TrustDelta(D12TarotTrustEventKind kind)
        {
            switch (kind)
            {
                case D12TarotTrustEventKind.SessionRespect: return 2;
                case D12TarotTrustEventKind.WarningSkipped: return 1;
                case D12TarotTrustEventKind.ClueConfirmed: return 2;
                case D12TarotTrustEventKind.ClueContradicted: return -4;
                case D12TarotTrustEventKind.Mockery: return -5;
                case D12TarotTrustEventKind.Coerced: return -8;
                default: return 0;
            }
        }

        private static void AddEventKey(List<string> list, string key) { if (!list.Contains(key)) list.Add(key); }

        private static void ValidateSessionRequest(D12TarotSessionRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (string.IsNullOrWhiteSpace(request.DivinerId) || string.IsNullOrWhiteSpace(request.QueryTag)) throw new InvalidOperationException("占卜请求缺少 diviner_id 或 query_tag。");
            if (request.WorldDay < 0 || request.SaveRevision < 0) throw new InvalidOperationException("占卜请求 world_day/save_revision 不能为负数。");
        }

        private static void ValidateVerificationRequest(D12TarotVerificationRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.ClueId) || string.IsNullOrWhiteSpace(request.SourceRef) || string.IsNullOrWhiteSpace(request.EvidenceRef))
                throw new InvalidOperationException("求证请求必须包含 session_id、clue_id、source_ref 和 evidence_ref。");
            if (request.WorldDay < 0 || request.SaveRevision < 0) throw new InvalidOperationException("求证请求 world_day/save_revision 不能为负数。");
            if (!Enum.IsDefined(typeof(D12TarotVerificationMethod), request.Method)) throw new InvalidOperationException("求证方式非法。");
        }

        private D12TarotSessionResult SessionFromState(D12TarotSessionState state, bool duplicate, string code, string message, string factEventId = null)
        {
            var result = new D12TarotSessionResult
            {
                Succeeded = true,
                Duplicate = duplicate,
                Skipped = state.SkippedSensitiveContent,
                Code = code,
                Message = message,
                SessionId = state.SessionId,
                FactEventId = factEventId ?? state.LastEventId,
                Trust = ReadDivinerTrust(state.DivinerId),
                State = state.Clone(),
                ExplanationContext = BuildExplanationContext(state.SessionId),
            };
            foreach (var card in state.Cards) result.Cards.Add(card.Clone());
            foreach (var clue in state.Clues) result.SoftClues.Add(clue.Clone());
            return result;
        }

        private static D12TarotSessionResult FailureSession(string code, string message) { return new D12TarotSessionResult { Code = code, Message = message }; }
        private static D12TarotVerificationResult FailureVerification(string code, string message) { return new D12TarotVerificationResult { Code = code, Message = message }; }
        private static D12TarotConsequenceResult FailureConsequence(string code, string message) { return new D12TarotConsequenceResult { Code = code, Message = message }; }
        private static D12TarotVerificationResult VerificationFromClue(D12SoftClue clue, bool duplicate, string code, string message, string factEventId = null)
        {
            return new D12TarotVerificationResult { Succeeded = true, Duplicate = duplicate, Code = code, Message = message, VerificationId = clue.ClueId, FactEventId = factEventId, Outcome = clue.Status, Clue = clue.Clone() };
        }

        private D12TarotConsequenceResult ConsequenceFromState(string consequenceId, bool duplicate, string code, string message, string eventId)
        {
            foreach (var item in _snapshot.Consequences)
                if (item != null && string.Equals(item.ConsequenceId, consequenceId, StringComparison.Ordinal))
                    return new D12TarotConsequenceResult { Succeeded = true, Duplicate = duplicate, Code = code, Message = message, ConsequenceId = consequenceId, Consequence = item.Clone() };
            return new D12TarotConsequenceResult { Succeeded = true, Duplicate = duplicate, Code = code, Message = message, ConsequenceId = consequenceId };
        }
    }
}
