// D13-A01 PropertySystem。
// 这里不计算价格、不改金钱、不判定法律。外部 authority 返回已裁决的来源，
// 本系统只在法律与经济批准后写入产权状态，并把每次结果追加到共享 EventLog。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13PropertyAssetSystem
    {
        private readonly Dictionary<string, D13PropertyDefinition> _definitions =
            new Dictionary<string, D13PropertyDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13PropertyState> _states =
            new Dictionary<string, D13PropertyState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13PropertySurveyRecord> _surveys =
            new Dictionary<string, D13PropertySurveyRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13ValuationQuote> _valuations =
            new Dictionary<string, D13ValuationQuote>(StringComparer.Ordinal);
        private readonly EventLog _eventLog;
        private readonly ID13PropertyLegalAuthority _legalAuthority;
        private readonly ID13PropertyEconomyAuthority _economyAuthority;
        private long _revision;

        public D13PropertyAssetSystem(EventLog eventLog = null,
            IEnumerable<D13PropertyDefinition> definitions = null,
            ID13PropertyLegalAuthority legalAuthority = null,
            ID13PropertyEconomyAuthority economyAuthority = null,
            D13PropertyRuntimeSnapshot initial = null)
        {
            _eventLog = eventLog ?? new EventLog();
            _legalAuthority = legalAuthority ?? new UnavailableLegalAuthority();
            _economyAuthority = economyAuthority ?? new UnavailableEconomyAuthority();

            var source = definitions == null ? D13PropertyCatalog.All : definitions.ToList();
            foreach (var definition in source)
            {
                if (definition == null) continue;
                definition.Validate();
                if (_definitions.ContainsKey(definition.Id))
                    throw new InvalidOperationException("D13 property definition duplicated: " + definition.Id);
                _definitions[definition.Id] = definition.Clone();
                _states[definition.Id] = NewInitialState(definition);
            }

            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13PropertyDefinition FindDefinition(string propertyId)
        {
            D13PropertyDefinition definition;
            return _definitions.TryGetValue(propertyId ?? string.Empty, out definition)
                ? definition.Clone() : null;
        }

        public D13PropertyState FindState(string propertyId)
        {
            D13PropertyState state;
            return _states.TryGetValue(propertyId ?? string.Empty, out state) ? state.Clone() : null;
        }

        /// <summary>UI/Agent 只读入口；返回定义、产权投影及来源，不提供写引用。</summary>
        public IReadOnlyList<D13PropertyReadModel> GetReadModel()
        {
            var result = new List<D13PropertyReadModel>();
            foreach (var pair in _definitions)
            {
                _states.TryGetValue(pair.Key, out D13PropertyState state);
                _surveys.TryGetValue(pair.Key, out D13PropertySurveyRecord survey);
                _valuations.TryGetValue(pair.Key, out D13ValuationQuote valuation);
                result.Add(new D13PropertyReadModel
                {
                    Definition = pair.Value.Clone(),
                    State = state == null ? null : state.Clone(),
                    LatestSurvey = survey == null ? null : survey.Clone(),
                    LatestValuation = valuation == null ? null : valuation.Clone(),
                });
            }
            return result;
        }

        public D13PropertyRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13PropertyRuntimeSnapshot { Revision = _revision };
            foreach (var state in _states.Values) result.Properties.Add(state.Clone());
            foreach (var survey in _surveys.Values) result.Surveys.Add(survey.Clone());
            foreach (var quote in _valuations.Values) result.Valuations.Add(quote.Clone());
            foreach (var fact in _eventLog.Facts)
                result.AppliedEventKeys.Add(fact.DedupeKey);
            return result;
        }

        public void Restore(D13PropertyRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            foreach (var state in snapshot.Properties)
                if (!_definitions.ContainsKey(state.PropertyId))
                    throw new InvalidOperationException("D13 property snapshot references unknown definition: " + state.PropertyId);
            foreach (var survey in snapshot.Surveys)
                if (!_definitions.ContainsKey(survey.PropertyId))
                    throw new InvalidOperationException("D13 survey references unknown definition: " + survey.PropertyId);
            foreach (var quote in snapshot.Valuations)
                if (!_definitions.ContainsKey(quote.PropertyId))
                    throw new InvalidOperationException("D13 valuation references unknown definition: " + quote.PropertyId);

            _states.Clear();
            foreach (var definition in _definitions.Values)
                _states[definition.Id] = NewInitialState(definition);
            foreach (var state in snapshot.Properties) _states[state.PropertyId] = state.Clone();
            _surveys.Clear();
            foreach (var survey in snapshot.Surveys) _surveys[survey.PropertyId] = survey.Clone();
            _valuations.Clear();
            foreach (var quote in snapshot.Valuations) _valuations[quote.PropertyId] = quote.Clone();
            _revision = snapshot.Revision;
        }

        /// <summary>产权调查由法律 authority 裁决，PropertySystem 只保存调查来源和结果投影。</summary>
        public D13PropertyResult Investigate(D13PropertySurveyRequest request)
        {
            if (request == null) return InvalidResult("invalid_request", "产权调查请求不存在。", D13PropertyTransactionKind.Purchase);
            D13PropertyDefinition definition;
            D13PropertyState state;
            if (!TryGet(request.PropertyId, out definition, out state))
                return InvalidResult("unknown_property", "未知资产。", D13PropertyTransactionKind.Purchase);
            if (string.IsNullOrWhiteSpace(request.InvestigatorRef))
                return InvalidResult("invalid_investigator", "调查人不能为空。", D13PropertyTransactionKind.Purchase);

            string eventId = EventId(request.OperationId, "survey_" + definition.Id);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, definition, state, D13PropertyTransactionKind.Purchase);

            var decision = _legalAuthority.Investigate(definition.Clone(), state.Clone(), request);
            if (decision == null) decision = new D13LegalDecision { Code = "legal_authority_missing", Message = "法律 authority 未返回决定。" };
            if (!string.IsNullOrWhiteSpace(decision.SourceOwnerSystem) && decision.SourceOwnerSystem != D13PropertyIds.LegalOwner)
            {
                decision = new D13LegalDecision
                {
                    Allowed = false,
                    Code = "invalid_legal_source",
                    Message = "产权调查来源不是法律/势力 authority。",
                    LegalStatus = D13PropertyLegalStatus.Unknown,
                    SourceOwnerSystem = D13PropertyIds.LegalOwner,
                };
            }
            var survey = new D13PropertySurveyRecord
            {
                SurveyId = eventId,
                PropertyId = definition.Id,
                InvestigatorRef = request.InvestigatorRef,
                Approved = decision.Allowed,
                DecisionCode = decision.Code ?? "legal_denied",
                TitleHolderRef = decision.TitleHolderRef ?? string.Empty,
                LegalStatus = decision.LegalStatus,
                SourceAuthority = decision.SourceOwnerSystem ?? D13PropertyIds.LegalOwner,
                SourceEventId = decision.SourceEventId ?? string.Empty,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0, request.SaveRevision),
            };
            if (request.EvidenceRefs != null) survey.EvidenceRefs.AddRange(request.EvidenceRefs.Where(IsNonEmpty));
            if (string.IsNullOrWhiteSpace(survey.SourceEventId)) survey.SourceEventId = "legal:" + eventId;
            survey.Validate();
            _surveys[definition.Id] = survey;
            state.LegalStatus = decision.LegalStatus;
            state.LegalSourceEventId = survey.SourceEventId;

            var status = decision.Allowed ? WorldEventStatus.Normal : WorldEventStatus.Failed;
            var fact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.LegalOwner,
                decision.Allowed ? "property_survey_completed" : "property_survey_denied",
                decision.Message ?? (decision.Allowed ? "产权调查完成。" : "产权调查未通过。"),
                request.InvestigatorRef, new[] { "property:" + definition.Id }, request.CauseEventIds,
                request.SaveRevision, status, status == WorldEventStatus.Failed ?
                    Recovery(D13PropertyIds.PropertySurveyRecoveryRoute, eventId, "补充产权文书或请求法律复核后重试。") : null,
                new Dictionary<string, string>
                {
                    ["property_id"] = definition.Id,
                    ["legal_code"] = survey.DecisionCode,
                    ["legal_source"] = survey.SourceEventId,
                    ["title_holder"] = survey.TitleHolderRef,
                    ["evidence_count"] = survey.EvidenceRefs.Count.ToString(CultureInfo.InvariantCulture),
                });
            return Result(fact, decision.Allowed, definition, state, survey, null, D13PropertyTransactionKind.Purchase,
                0, survey.SourceEventId, null, status == WorldEventStatus.Failed ? D13PropertyIds.PropertySurveyRecoveryRoute : null);
        }

        /// <summary>估值只读取 Economy authority；本系统不使用面积/岛屿系数自行计算价格。</summary>
        public D13PropertyResult RequestValuation(D13PropertyValuationRequest request)
        {
            if (request == null) return InvalidResult("invalid_request", "估值请求不存在。", D13PropertyTransactionKind.Purchase);
            D13PropertyDefinition definition;
            D13PropertyState state;
            if (!TryGet(request.PropertyId, out definition, out state))
                return InvalidResult("unknown_property", "未知资产。", D13PropertyTransactionKind.Purchase);
            if (string.IsNullOrWhiteSpace(request.ApplicantRef))
                return InvalidResult("invalid_applicant", "估值申请人不能为空。", D13PropertyTransactionKind.Purchase);

            string eventId = EventId(request.OperationId, "valuation_" + definition.Id);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null)
            {
                _valuations.TryGetValue(definition.Id, out D13ValuationQuote savedQuote);
                return Duplicate(duplicate, definition, state, D13PropertyTransactionKind.Purchase, savedQuote);
            }

            var decision = _economyAuthority.Quote(definition.Clone(), state.Clone(), request);
            if (decision == null) decision = new D13EconomyValuationDecision { Code = "economy_authority_missing", Message = "经济 authority 未返回估值。" };
            if (!decision.Approved || decision.Quote == null)
            {
                var failedFact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.EconomyOwner,
                    decision.Code ?? "valuation_denied", decision.Message ?? "估值未通过。", request.ApplicantRef,
                    new[] { "property:" + definition.Id }, null, request.SaveRevision, WorldEventStatus.Failed,
                    Recovery(D13PropertyIds.PropertyValuationRecoveryRoute, eventId, "补充市场资料或请求经济系统重新估值。"),
                    new Dictionary<string, string> { ["property_id"] = definition.Id, ["economy_code"] = decision.Code ?? "valuation_denied" });
                return Result(failedFact, false, definition, state, null, null, D13PropertyTransactionKind.Purchase,
                    0, null, null, D13PropertyIds.PropertyValuationRecoveryRoute);
            }

            var quote = decision.Quote.Clone();
            if (!string.IsNullOrWhiteSpace(quote.SourceOwnerSystem) && quote.SourceOwnerSystem != D13PropertyIds.EconomyOwner)
            {
                var invalidSourceFact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.EconomyOwner,
                    "invalid_economy_source", "估值来源不是 EconomySystem。", request.ApplicantRef,
                    new[] { "property:" + definition.Id }, null, request.SaveRevision, WorldEventStatus.Failed,
                    Recovery(D13PropertyIds.PropertyValuationRecoveryRoute, eventId, "请由 EconomySystem 重新提供估值来源。"),
                    new Dictionary<string, string>
                    {
                        ["property_id"] = definition.Id,
                        ["economy_source"] = quote.SourceOwnerSystem,
                    });
                return Result(invalidSourceFact, false, definition, state, null, null, D13PropertyTransactionKind.Purchase,
                    0, null, null, D13PropertyIds.PropertyValuationRecoveryRoute);
            }
            quote.PropertyId = definition.Id;
            quote.SourceOwnerSystem = D13PropertyIds.EconomyOwner;
            quote.WorldDay = Math.Max(0, request.WorldDay);
            quote.SaveRevision = Math.Max(0, request.SaveRevision);
            quote.Validate();
            _valuations[definition.Id] = quote;
            state.LatestValuationAmount = quote.Amount;
            state.LatestValuationSourceEventId = quote.SourceEventId;
            state.LatestValuationDay = quote.WorldDay;
            var fact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.EconomyOwner,
                "property_valuation_completed", "经济系统已返回资产估值。", request.ApplicantRef,
                new[] { "property:" + definition.Id }, null, request.SaveRevision, WorldEventStatus.Normal, null,
                new Dictionary<string, string>
                {
                    ["property_id"] = definition.Id,
                    ["amount"] = quote.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = quote.Currency,
                    ["valuation_basis"] = quote.ValuationBasis,
                    ["economy_source"] = quote.SourceEventId,
                });
            return Result(fact, true, definition, state, null, quote, D13PropertyTransactionKind.Purchase,
                quote.Amount, null, quote.SourceEventId, null);
        }

        public D13PropertyResult Purchase(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Purchase); }
        public D13PropertyResult Lease(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Lease); }
        public D13PropertyResult CoGovern(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.CoGovernance); }
        public D13PropertyResult Mortgage(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Mortgage); }
        public D13PropertyResult Sell(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Sale); }
        public D13PropertyResult Transfer(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Transfer); }
        public D13PropertyResult Inherit(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.Inheritance); }
        public D13PropertyResult OpenDispute(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.OpenDispute); }
        public D13PropertyResult ResolveDispute(D13PropertyTransactionRequest request) { return ExecuteTransaction(request, D13PropertyTransactionKind.ResolveDispute); }

        private D13PropertyResult ExecuteTransaction(D13PropertyTransactionRequest request, D13PropertyTransactionKind kind)
        {
            if (request == null) return InvalidResult("invalid_request", "资产交易请求不存在。", kind);
            request.Kind = kind;
            D13PropertyDefinition definition;
            D13PropertyState state;
            if (!TryGet(request.PropertyId, out definition, out state))
                return InvalidResult("unknown_property", "未知资产。", kind);
            if (string.IsNullOrWhiteSpace(request.ApplicantRef))
                return InvalidResult("invalid_applicant", "交易申请人不能为空。", kind);

            string eventId = EventId(request.OperationId, kind.ToString().ToLowerInvariant() + "_" + definition.Id);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, definition, state, kind);

            if (kind == D13PropertyTransactionKind.OpenDispute || kind == D13PropertyTransactionKind.ResolveDispute)
                return ExecuteDispute(request, kind, definition, state, eventId);

            var survey = LatestSurvey(definition.Id);
            if (survey == null || !survey.Approved)
                return FailedTransaction(eventId, request, definition, state, kind, "survey_required", "先完成通过的产权调查。", D13PropertyIds.PropertySurveyRecoveryRoute);
            if (!ValidShape(request, state, kind, out string shapeCode, out string shapeMessage))
                return FailedTransaction(eventId, request, definition, state, kind, shapeCode, shapeMessage, D13PropertyIds.PropertyTransactionRecoveryRoute);

            var legal = _legalAuthority.Authorize(definition.Clone(), state.Clone(), request, survey.Clone());
            if (legal == null) legal = new D13LegalDecision { Code = "legal_authority_missing", Message = "法律 authority 未返回决定。" };
            if (!string.IsNullOrWhiteSpace(legal.SourceOwnerSystem) && legal.SourceOwnerSystem != D13PropertyIds.LegalOwner)
                return FailedTransaction(eventId, request, definition, state, kind, "invalid_legal_source", "资产操作来源不是法律/势力 authority。", D13PropertyIds.PropertyDisputeRecoveryRoute);
            if (!legal.Allowed)
                return FailedTransaction(eventId, request, definition, state, kind, legal.Code, legal.Message ?? "法律未批准该资产操作。", D13PropertyIds.PropertyDisputeRecoveryRoute, legal.SourceEventId);
            string legalSource = legal.SourceEventId ?? "legal:" + eventId;

            _valuations.TryGetValue(definition.Id, out D13ValuationQuote valuation);
            var economy = _economyAuthority.Approve(definition.Clone(), state.Clone(), request, valuation == null ? null : valuation.Clone());
            if (economy == null) economy = new D13EconomyTransactionDecision { Code = "economy_authority_missing", Message = "经济 authority 未返回交易决定。" };
            if (!string.IsNullOrWhiteSpace(economy.SourceOwnerSystem) && economy.SourceOwnerSystem != D13PropertyIds.EconomyOwner)
                return FailedTransaction(eventId, request, definition, state, kind, "invalid_economy_source", "资产金额来源不是 EconomySystem。", D13PropertyIds.PropertyTransactionRecoveryRoute, legalSource);
            if (!economy.Approved)
                return FailedTransaction(eventId, request, definition, state, kind, economy.Code, economy.Message ?? "经济系统未批准该资产操作。", D13PropertyIds.PropertyTransactionRecoveryRoute, legalSource, economy.SourceEventId);
            if (string.IsNullOrWhiteSpace(economy.SourceEventId))
                return FailedTransaction(eventId, request, definition, state, kind, "economy_source_missing", "经济批准缺少来源事件。", D13PropertyIds.PropertyTransactionRecoveryRoute, legalSource);
            if (economy.Amount < 0)
                return FailedTransaction(eventId, request, definition, state, kind, "economy_amount_invalid", "经济批准金额不能为负数。", D13PropertyIds.PropertyTransactionRecoveryRoute, legalSource, economy.SourceEventId);
            if (kind == D13PropertyTransactionKind.Mortgage && string.IsNullOrWhiteSpace(economy.LedgerRef))
                return FailedTransaction(eventId, request, definition, state, kind, "mortgage_ledger_missing", "抵押批准必须包含 EconomySystem 账本引用。", D13PropertyIds.PropertyTransactionRecoveryRoute, legalSource, economy.SourceEventId);

            ApplyTransaction(state, request, kind, economy);
            state.Revision++;
            state.LastUpdatedDay = Math.Max(0, request.WorldDay);
            state.LastEventId = eventId;
            _revision = Math.Max(_revision + 1, state.Revision);
            var fact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.SystemOwner,
                "property_" + kind.ToString().ToLowerInvariant() + "_completed", "资产权利状态已由 PropertySystem 更新。", request.ApplicantRef,
                new[] { "property:" + definition.Id, "legal:" + legalSource, "economy:" + economy.SourceEventId },
                request.CauseEventIds, request.SaveRevision, WorldEventStatus.Normal, null,
                new Dictionary<string, string>
                {
                    ["property_id"] = definition.Id,
                    ["transaction_kind"] = kind.ToString(),
                    ["owner_ref"] = state.OwnerRef ?? string.Empty,
                    ["lease_holder_ref"] = state.LeaseHolderRef ?? string.Empty,
                    ["legal_source"] = legalSource,
                    ["economy_source"] = economy.SourceEventId,
                    ["ledger_ref"] = economy.LedgerRef ?? string.Empty,
                    ["amount"] = economy.Amount.ToString(CultureInfo.InvariantCulture),
                });
            return Result(fact, true, definition, state, null, valuation, kind, economy.Amount,
                legalSource, economy.SourceEventId, null);
        }

        private D13PropertyResult ExecuteDispute(D13PropertyTransactionRequest request, D13PropertyTransactionKind kind,
            D13PropertyDefinition definition, D13PropertyState state, string eventId)
        {
            if (kind == D13PropertyTransactionKind.OpenDispute && string.IsNullOrWhiteSpace(request.DisputeId))
                return FailedTransaction(eventId, request, definition, state, kind, "dispute_id_required", "争议必须有登记号。", D13PropertyIds.PropertyDisputeRecoveryRoute);
            if (kind == D13PropertyTransactionKind.ResolveDispute && state.DisputeStatus != D13PropertyDisputeStatus.Open)
                return FailedTransaction(eventId, request, definition, state, kind, "no_open_dispute", "没有待解决的产权争议。", D13PropertyIds.PropertyDisputeRecoveryRoute);

            var survey = LatestSurvey(definition.Id);
            var legal = _legalAuthority.Authorize(definition.Clone(), state.Clone(), request, survey == null ? null : survey.Clone());
            if (legal == null) legal = new D13LegalDecision { Code = "legal_authority_missing", Message = "法律 authority 未返回争议决定。" };
            if (!string.IsNullOrWhiteSpace(legal.SourceOwnerSystem) && legal.SourceOwnerSystem != D13PropertyIds.LegalOwner)
                return FailedTransaction(eventId, request, definition, state, kind, "invalid_legal_source", "争议来源不是法律/势力 authority。", D13PropertyIds.PropertyDisputeRecoveryRoute);
            if (!legal.Allowed)
                return FailedTransaction(eventId, request, definition, state, kind, legal.Code, legal.Message ?? "法律未批准争议操作。", D13PropertyIds.PropertyDisputeRecoveryRoute, legal.SourceEventId);
            string legalSource = legal.SourceEventId ?? "legal:" + eventId;

            if (kind == D13PropertyTransactionKind.OpenDispute)
            {
                state.DisputeStatus = legal.DisputeStatus == D13PropertyDisputeStatus.None
                    ? D13PropertyDisputeStatus.Open : legal.DisputeStatus;
                state.DisputeId = request.DisputeId;
                state.LegalStatus = D13PropertyLegalStatus.Disputed;
                state.LegalSourceEventId = legalSource;
                state.Ownership = D13OwnershipKind.Disputed;
            }
            else
            {
                string previousOwner = state.OwnerRef;
                string resolvedOwner = legal.ResolvedOwnerRef;
                if (string.IsNullOrWhiteSpace(resolvedOwner))
                    return FailedTransaction(eventId, request, definition, state, kind, "resolved_owner_missing", "法律争议决定缺少归属对象。", D13PropertyIds.PropertyDisputeRecoveryRoute, legalSource);
                state.OwnerRef = resolvedOwner;
                state.ControllerRef = resolvedOwner;
                state.DisputeStatus = string.Equals(resolvedOwner, previousOwner, StringComparison.Ordinal)
                    ? D13PropertyDisputeStatus.ResolvedForOwner : D13PropertyDisputeStatus.ResolvedForClaimant;
                state.DisputeId = request.DisputeId ?? state.DisputeId;
                state.LegalStatus = legal.LegalStatus == D13PropertyLegalStatus.Unknown
                    ? D13PropertyLegalStatus.Registered : legal.LegalStatus;
                state.LegalSourceEventId = legalSource;
                state.Ownership = state.DisputeStatus == D13PropertyDisputeStatus.ResolvedForClaimant
                    ? D13OwnershipKind.Transferred : D13OwnershipKind.PrivateOwnership;
            }
            state.Revision++;
            state.LastUpdatedDay = Math.Max(0, request.WorldDay);
            state.LastEventId = eventId;
            _revision = Math.Max(_revision + 1, state.Revision);
            var fact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.LegalOwner,
                kind == D13PropertyTransactionKind.OpenDispute ? "property_dispute_opened" : "property_dispute_resolved",
                legal.Message ?? "产权争议法律状态已更新。", request.ApplicantRef,
                new[] { "property:" + definition.Id, "legal:" + legalSource }, request.CauseEventIds,
                request.SaveRevision, WorldEventStatus.Normal, null,
                new Dictionary<string, string>
                {
                    ["property_id"] = definition.Id,
                    ["dispute_id"] = state.DisputeId ?? string.Empty,
                    ["dispute_status"] = state.DisputeStatus.ToString(),
                    ["legal_source"] = legalSource,
                    ["resolved_owner"] = state.OwnerRef ?? string.Empty,
                });
            return Result(fact, true, definition, state, null, null, kind, 0, legalSource, null, null);
        }

        private D13PropertyResult FailedTransaction(string eventId, D13PropertyTransactionRequest request,
            D13PropertyDefinition definition, D13PropertyState state, D13PropertyTransactionKind kind,
            string code, string message, string route, string legalSource = null, string economySource = null)
        {
            var fact = AppendEvent(eventId, request.WorldDay, definition.LocationRef, D13PropertyIds.SystemOwner,
                code ?? "property_transaction_failed", message ?? "资产操作失败。", request.ApplicantRef,
                new[] { "property:" + definition.Id }, request.CauseEventIds, request.SaveRevision, WorldEventStatus.Failed,
                Recovery(route, eventId, message ?? "补充资料后重试。"),
                new Dictionary<string, string>
                {
                    ["property_id"] = definition.Id,
                    ["transaction_kind"] = kind.ToString(),
                    ["failure_code"] = code ?? "property_transaction_failed",
                    ["legal_source"] = legalSource ?? string.Empty,
                    ["economy_source"] = economySource ?? string.Empty,
                });
            return Result(fact, false, definition, state, null, null, kind, 0, legalSource, economySource, route);
        }

        private static void ApplyTransaction(D13PropertyState state, D13PropertyTransactionRequest request,
            D13PropertyTransactionKind kind, D13EconomyTransactionDecision economy)
        {
            switch (kind)
            {
                case D13PropertyTransactionKind.Purchase:
                    state.OwnerRef = request.ApplicantRef;
                    state.ControllerRef = request.ApplicantRef;
                    state.LeaseHolderRef = null;
                    state.LeaseEndDay = 0;
                    state.CoGovernanceRefs.Clear();
                    state.MortgageRef = null;
                    state.Ownership = D13OwnershipKind.PrivateOwnership;
                    state.DisputeStatus = D13PropertyDisputeStatus.None;
                    break;
                case D13PropertyTransactionKind.Lease:
                    state.LeaseHolderRef = request.ApplicantRef;
                    state.LeaseEndDay = request.LeaseEndDay;
                    state.Ownership = D13OwnershipKind.Leasehold;
                    break;
                case D13PropertyTransactionKind.CoGovernance:
                    state.CoGovernanceRefs.Clear();
                    foreach (var member in request.CoGovernanceRefs.Where(IsNonEmpty).Distinct(StringComparer.Ordinal))
                        state.CoGovernanceRefs.Add(member);
                    if (!state.CoGovernanceRefs.Contains(request.ApplicantRef)) state.CoGovernanceRefs.Add(request.ApplicantRef);
                    state.Ownership = D13OwnershipKind.CoGovernance;
                    break;
                case D13PropertyTransactionKind.Mortgage:
                    state.MortgageRef = economy.LedgerRef;
                    state.Ownership = D13OwnershipKind.Mortgaged;
                    break;
                case D13PropertyTransactionKind.Sale:
                    state.OwnerRef = request.CounterpartyRef;
                    state.ControllerRef = request.CounterpartyRef;
                    state.LeaseHolderRef = null;
                    state.LeaseEndDay = 0;
                    state.CoGovernanceRefs.Clear();
                    state.MortgageRef = null;
                    state.Ownership = D13OwnershipKind.PrivateOwnership;
                    break;
                case D13PropertyTransactionKind.Transfer:
                    state.OwnerRef = request.CounterpartyRef;
                    state.ControllerRef = request.CounterpartyRef;
                    state.Ownership = D13OwnershipKind.Transferred;
                    break;
                case D13PropertyTransactionKind.Inheritance:
                    state.OwnerRef = request.CounterpartyRef;
                    state.ControllerRef = request.CounterpartyRef;
                    state.Ownership = D13OwnershipKind.Inherited;
                    break;
            }
        }

        private static bool ValidShape(D13PropertyTransactionRequest request, D13PropertyState state,
            D13PropertyTransactionKind kind, out string code, out string message)
        {
            code = null;
            message = null;
            if (kind == D13PropertyTransactionKind.Lease && request.LeaseEndDay <= request.WorldDay)
            {
                code = "invalid_lease_term";
                message = "租约结束日必须晚于当前世界日。";
                return false;
            }
            if ((kind == D13PropertyTransactionKind.Sale || kind == D13PropertyTransactionKind.Transfer || kind == D13PropertyTransactionKind.Inheritance)
                && string.IsNullOrWhiteSpace(request.CounterpartyRef))
            {
                code = "counterparty_required";
                message = "出售、转让或继承必须指定受让方。";
                return false;
            }
            if (kind == D13PropertyTransactionKind.CoGovernance &&
                (request.CoGovernanceRefs == null || request.CoGovernanceRefs.All(string.IsNullOrWhiteSpace)))
            {
                code = "governance_members_required";
                message = "共同治理必须指定参与方。";
                return false;
            }
            if (kind == D13PropertyTransactionKind.Mortgage && !string.Equals(state.OwnerRef, request.ApplicantRef, StringComparison.Ordinal))
            {
                code = "mortgage_owner_required";
                message = "只有登记所有者可以申请抵押。";
                return false;
            }
            if ((kind == D13PropertyTransactionKind.Sale || kind == D13PropertyTransactionKind.Transfer) &&
                !string.Equals(state.OwnerRef, request.ApplicantRef, StringComparison.Ordinal))
            {
                code = "owner_required";
                message = "只有登记所有者可以出售或转让。";
                return false;
            }
            return true;
        }

        private bool TryGet(string propertyId, out D13PropertyDefinition definition, out D13PropertyState state)
        {
            definition = null;
            state = null;
            if (!_definitions.TryGetValue(propertyId ?? string.Empty, out definition)) return false;
            if (!_states.TryGetValue(propertyId ?? string.Empty, out state))
            {
                definition = null;
                return false;
            }
            return true;
        }

        private D13PropertySurveyRecord LatestSurvey(string propertyId)
        {
            _surveys.TryGetValue(propertyId, out D13PropertySurveyRecord survey);
            return survey;
        }

        private D13PropertyState NewInitialState(D13PropertyDefinition definition)
        {
            return new D13PropertyState
            {
                PropertyId = definition.Id,
                OwnerRef = definition.DefaultOwnerRef,
                ControllerRef = definition.ControllingFactionId,
                LegalStatus = D13PropertyLegalStatus.Unknown,
                DisputeStatus = D13PropertyDisputeStatus.None,
            };
        }

        private FactEvent Existing(string eventId, long saveRevision)
        {
            return _eventLog.Find(eventId, Math.Max(0, saveRevision));
        }

        private FactEvent AppendEvent(string eventId, int worldDay, string locationRef, string ownerSystem,
            string resultCode, string summary, string actorRef, IEnumerable<string> inputFacts,
            IEnumerable<string> causeEventIds, long saveRevision, WorldEventStatus status,
            RecoveryRoute recovery, Dictionary<string, string> values)
        {
            var fact = new WorldEvent(eventId, Math.Max(0, worldDay), locationRef, ownerSystem,
                new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { "property_fact:" + eventId }, Values = values },
                new[] { string.IsNullOrWhiteSpace(actorRef) ? D13PropertyIds.PlayerRef : actorRef },
                inputFacts == null ? new[] { "property:unknown" } : inputFacts.Where(IsNonEmpty),
                FilterCauses(causeEventIds, saveRevision), Math.Max(0, saveRevision), status, recovery);
            return _eventLog.Append(fact).Fact;
        }

        private IEnumerable<string> FilterCauses(IEnumerable<string> causeEventIds, long saveRevision)
        {
            if (causeEventIds == null) return Enumerable.Empty<string>();
            return causeEventIds.Where(IsNonEmpty).Where(id => Existing(id, saveRevision) != null).Distinct(StringComparer.Ordinal);
        }

        private static RecoveryRoute Recovery(string routeId, string eventId, string description)
        {
            return new RecoveryRoute { RouteId = routeId, Action = "review_and_retry", ResumeFromEventId = eventId, Description = description };
        }

        private static string EventId(string operationId, string fallback)
        {
            return "event_d13_property_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId);
        }

        private static D13PropertyResult Result(FactEvent fact, bool succeeded, D13PropertyDefinition definition,
            D13PropertyState state, D13PropertySurveyRecord survey, D13ValuationQuote valuation,
            D13PropertyTransactionKind kind, int amount, string legalSource, string economySource, string route)
        {
            return new D13PropertyResult
            {
                Succeeded = succeeded,
                Duplicate = false,
                Code = fact == null || fact.Result == null ? (succeeded ? "ok" : "failed") : fact.Result.ResultCode,
                Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary,
                EventId = fact == null ? string.Empty : fact.EventId,
                TransactionKind = kind,
                Property = state == null ? null : state.Clone(),
                Survey = survey == null ? null : survey.Clone(),
                Valuation = valuation == null ? null : valuation.Clone(),
                Fact = fact == null ? null : fact.CloneFact(),
                LegalSourceEventId = legalSource,
                EconomySourceEventId = economySource,
                Amount = amount,
                RecoveryRouteId = route,
            };
        }

        private static D13PropertyResult Duplicate(FactEvent fact, D13PropertyDefinition definition,
            D13PropertyState state, D13PropertyTransactionKind kind, D13ValuationQuote valuation = null)
        {
            return new D13PropertyResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate",
                Message = "同一产权操作已结算。",
                EventId = fact.EventId,
                TransactionKind = kind,
                Property = state == null ? null : state.Clone(),
                Valuation = valuation == null ? null : valuation.Clone(),
                Fact = fact.CloneFact(),
            };
        }

        private static D13PropertyResult InvalidResult(string code, string message, D13PropertyTransactionKind kind)
        {
            return new D13PropertyResult { Succeeded = false, Code = code, Message = message, TransactionKind = kind };
        }

        private static bool IsNonEmpty(string value) { return !string.IsNullOrWhiteSpace(value); }

        private sealed class UnavailableLegalAuthority : ID13PropertyLegalAuthority
        {
            public D13LegalDecision Investigate(D13PropertyDefinition definition, D13PropertyState state, D13PropertySurveyRequest request)
            {
                return new D13LegalDecision { Allowed = false, Code = "legal_authority_required", Message = "必须由法律/势力系统提供产权调查决定。" };
            }

            public D13LegalDecision Authorize(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyTransactionRequest request, D13PropertySurveyRecord survey)
            {
                return new D13LegalDecision { Allowed = false, Code = "legal_authority_required", Message = "必须由法律/势力系统提供产权决定。" };
            }
        }

        private sealed class UnavailableEconomyAuthority : ID13PropertyEconomyAuthority
        {
            public D13EconomyValuationDecision Quote(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyValuationRequest request)
            {
                return new D13EconomyValuationDecision { Approved = false, Code = "economy_authority_required", Message = "必须由经济系统提供资产估值。" };
            }

            public D13EconomyTransactionDecision Approve(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyTransactionRequest request, D13ValuationQuote valuation)
            {
                return new D13EconomyTransactionDecision { Approved = false, Code = "economy_authority_required", Message = "必须由经济系统批准资产金额或账本。" };
            }
        }
    }
}
