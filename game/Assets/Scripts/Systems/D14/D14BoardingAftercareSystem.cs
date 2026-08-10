// D14-A02 接舷/俘虏/战后处理编排。
// 本类不直接改 Inventory、Crew、Medical 或 Relation 的 canonical 数值，
// 所有外部结果必须由对应 authority 返回并带有统一 EventLog 来源。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14BoardingAftercareSystem
    {
        private readonly EventLog _eventLog;
        private readonly ID14CombatBoardingAuthority _combat;
        private readonly ID14CasualtyAuthority _casualties;
        private readonly ID14PrisonerAuthority _prisoners;
        private readonly ID14LootAuthority _loot;
        private readonly ID14MedicalAuthority _medical;
        private readonly ID14InterrogationAuthority _interrogation;
        private readonly ID14FuneralAuthority _funeral;
        private readonly ID14RelationAuthority _relations;
        private D14AftercareRuntimeSnapshot _state;
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);

        public D14BoardingAftercareSystem(EventLog eventLog,
            ID14CombatBoardingAuthority combat = null,
            ID14CasualtyAuthority casualties = null,
            ID14PrisonerAuthority prisoners = null,
            ID14LootAuthority loot = null,
            ID14MedicalAuthority medical = null,
            ID14InterrogationAuthority interrogation = null,
            ID14FuneralAuthority funeral = null,
            ID14RelationAuthority relations = null,
            D14AftercareRuntimeSnapshot initial = null)
        {
            if (initial != null && initial.EventLog != null && eventLog == null)
                _eventLog = EventLog.FromSnapshot(initial.EventLog);
            else
                _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");

            _combat = combat ?? new D14CombatBoardingRuleAuthority(_eventLog);
            _casualties = casualties ?? new UnavailableCasualtyAuthority();
            _prisoners = prisoners ?? new UnavailablePrisonerAuthority();
            _loot = loot ?? new UnavailableLootAuthority();
            _medical = medical ?? new UnavailableMedicalAuthority();
            _interrogation = interrogation ?? new UnavailableInterrogationAuthority();
            _funeral = funeral ?? new UnavailableFuneralAuthority();
            _relations = relations ?? new UnavailableRelationAuthority();

            _state = initial == null ? new D14AftercareRuntimeSnapshot() : initial.Clone();
            _state.Validate();
            if (initial != null && initial.AppliedEventKeys != null)
                foreach (var key in initial.AppliedEventKeys) _appliedEventKeys.Add(key);
        }

        public D14AftercareRuntimeSnapshot Snapshot { get { return _state.Clone(); } }
        public EventLog SharedEventLog { get { return _eventLog; } }
        public IReadOnlyList<FactEvent> AftercareLog { get { return _eventLog.ReadFactsForUi(int.MaxValue); } }

        public D14BoardingCase FindCase(string caseId)
        {
            if (string.IsNullOrWhiteSpace(caseId)) return null;
            foreach (var item in _state.Cases)
                if (string.Equals(item.CaseId, caseId, StringComparison.Ordinal)) return item.Clone();
            return null;
        }

        public D14PrisonerRecord FindPrisoner(string prisonerId)
        {
            if (string.IsNullOrWhiteSpace(prisonerId)) return null;
            foreach (var item in _state.Prisoners)
                if (string.Equals(item.PrisonerId, prisonerId, StringComparison.Ordinal)) return item.Clone();
            return null;
        }

        public D14CasualtyRecord FindCasualty(string casualtyId)
        {
            if (string.IsNullOrWhiteSpace(casualtyId)) return null;
            foreach (var item in _state.Casualties)
                if (string.Equals(item.CasualtyId, casualtyId, StringComparison.Ordinal)) return item.Clone();
            return null;
        }

        public D14AftercareRuntimeSnapshot CaptureSnapshot()
        {
            var result = _state.Clone();
            result.AppliedEventKeys.Clear();
            result.AppliedEventKeys.AddRange(_appliedEventKeys);
            result.EventLog = _eventLog.CaptureSnapshot();
            result.Validate();
            return result;
        }

        public D14AftercareResult RequestBoarding(D14BoardingRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "接舷请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            if (request.CombatSnapshot == null || request.PlayerParty == null || request.EnemyParty == null || request.PlayerParty.Count == 0 || request.EnemyParty.Count == 0)
                return Reject("boarding_inputs_missing", "接舷需要战斗快照以及双方人员。", null);
            try
            {
                request.CombatSnapshot.Validate();
                ValidateParty(request.PlayerParty, D14ParticipantSide.Player, "player_party");
                ValidateParty(request.EnemyParty, D14ParticipantSide.Enemy, "enemy_party");
                if (request.Content != null) request.Content.Validate("boarding.content", false);
            }
            catch (Exception ex) { return Reject("invalid_boarding_inputs", ex.Message, null); }

            var decision = _combat.Evaluate(request);
            if (!ValidDecision(decision, D14AftercareIds.CombatOwner))
                return Reject(decision == null ? "combat_authority_required" : decision.Code, decision == null ? "CombatSystem 未返回接舷决定。" : decision.Message, null);
            if (!decision.Allowed)
                return Reject(decision.Code, decision.Message, null);

            var boardingCase = new D14BoardingCase
            {
                CaseId = request.CaseId,
                EncounterId = request.CombatSnapshot.EncounterId,
                PlayerShipId = request.CombatSnapshot.PlayerShipId,
                EnemyShipId = request.CombatSnapshot.EnemyShipId,
                Status = D14BoardingCaseStatus.Open,
                Outcome = D14AftercareOutcome.NonLethalDisengagement,
                Distance = request.CombatSnapshot.Kinematics.Distance,
                PlayerMorale = Clamp01Hundred(decision.PlayerMorale),
                EnemyMorale = Clamp01Hundred(decision.EnemyMorale),
                CombatSourceEventId = decision.SourceEventId,
                Content = request.Content == null ? null : request.Content.Clone(),
            };
            foreach (var participant in request.PlayerParty) boardingCase.PlayerParty.Add(participant.Clone());
            foreach (var participant in request.EnemyParty) boardingCase.EnemyParty.Add(participant.Clone());
            UpsertCase(boardingCase);
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "boarding_opened", "已满足接舷条件，双方人员和武器进入接舷处理。", "boarding_opened",
                new Dictionary<string, string>
                {
                    ["case_id"] = boardingCase.CaseId, ["combat_source_event_id"] = boardingCase.CombatSourceEventId,
                    ["distance"] = F(boardingCase.Distance), ["player_party_count"] = boardingCase.PlayerParty.Count.ToString(CultureInfo.InvariantCulture),
                    ["enemy_party_count"] = boardingCase.EnemyParty.Count.ToString(CultureInfo.InvariantCulture),
                });
            boardingCase.LastEventId = result.EventId;
            UpsertCase(boardingCase);
            result.BoardingCase = boardingCase.Clone();
            return result;
        }

        public D14AftercareResult ProcessSurrender(D14SurrenderAftercareRequest request)
        {
            if (request == null || request.CombatSnapshot == null || request.CombatSnapshot.Status != D14NavalCombatStatus.Surrendered)
                return Reject("surrender_status_required", "只有 CombatSystem 已裁决投降的战斗才能进入投降后处理。", null);
            var opened = RequestBoarding(request);
            if (!opened.Accepted || opened.Duplicate) return opened;
            return ResolveBoarding(new D14BoardingResolutionRequest
            {
                OperationId = request.OperationId + "_resolve_surrender",
                CaseId = request.CaseId,
                RequestedOutcome = D14AftercareOutcome.Surrendered,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision + 1,
                CauseEventIds = new List<string> { opened.EventId },
            });
        }

        public D14AftercareResult ResolveBoarding(D14BoardingResolutionRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "接舷结算请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var boardingCase = FindCaseMutable(request.CaseId);
            if (boardingCase == null) return Reject("boarding_case_missing", "接舷案件不存在。", null);
            if (boardingCase.Status != D14BoardingCaseStatus.Open)
                return Reject("boarding_case_closed", "接舷案件已结算，不能重复裁决。", boardingCase);
            if (!Enum.IsDefined(typeof(D14AftercareOutcome), request.RequestedOutcome))
                return Reject("invalid_outcome", "接舷结果无效。", boardingCase);

            var decision = _combat.Resolve(request, boardingCase.Clone());
            if (!ValidDecision(decision, D14AftercareIds.CombatOwner))
                return Reject(decision == null ? "combat_authority_required" : decision.Code, decision == null ? "CombatSystem 未返回结算决定。" : decision.Message, boardingCase);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, boardingCase);
            var casualtyDecision = _casualties.Resolve(request, boardingCase.Clone(), decision);
            if (!ValidDecision(casualtyDecision, D14AftercareIds.CasualtyOwner) || !casualtyDecision.Allowed)
                return Reject(casualtyDecision == null ? "casualty_authority_required" : casualtyDecision.Code, casualtyDecision == null ? "CrewSystem 未返回伤亡决定。" : casualtyDecision.Message, boardingCase);

            boardingCase.Status = StatusFor(decision.Outcome);
            boardingCase.Outcome = decision.Outcome;
            boardingCase.PlayerMorale = Clamp01Hundred(decision.PlayerMorale);
            boardingCase.EnemyMorale = Clamp01Hundred(decision.EnemyMorale);
            UpsertCase(boardingCase);
            foreach (var casualty in casualtyDecision.Casualties)
            {
                if (casualty == null) continue;
                casualty.CaseId = boardingCase.CaseId;
                casualty.SourceEventId = casualtyDecision.SourceEventId;
                UpsertCasualty(casualty);
            }

            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "boarding_resolved", "接舷结局已由 CombatSystem、CrewSystem 和相关 authority 裁决。", "boarding_resolved",
                new Dictionary<string, string>
                {
                    ["case_id"] = boardingCase.CaseId, ["outcome"] = decision.Outcome.ToString(),
                    ["casualty_count"] = casualtyDecision.Casualties.Count.ToString(CultureInfo.InvariantCulture),
                    ["player_morale"] = boardingCase.PlayerMorale.ToString(CultureInfo.InvariantCulture),
                    ["enemy_morale"] = boardingCase.EnemyMorale.ToString(CultureInfo.InvariantCulture),
                    ["combat_source_event_id"] = decision.SourceEventId, ["casualty_source_event_id"] = casualtyDecision.SourceEventId,
                });
            boardingCase.LastEventId = result.EventId;
            UpsertCase(boardingCase);
            result.BoardingCase = boardingCase.Clone();

            if (decision.Outcome == D14AftercareOutcome.Captured || decision.Outcome == D14AftercareOutcome.Surrendered)
            {
                var prisonerDecision = _prisoners.Register(request, boardingCase.Clone(), result.Fact);
                if (ValidDecision(prisonerDecision, D14AftercareIds.PrisonerOwner) && prisonerDecision.Allowed)
                {
                    foreach (var prisoner in prisonerDecision.Prisoners)
                    {
                        if (prisoner == null) continue;
                        prisoner.CaseId = boardingCase.CaseId;
                        prisoner.SourceEventId = prisonerDecision.SourceEventId;
                        UpsertPrisoner(prisoner);
                    }
                    AppendLinkedEvent(request.OperationId + "_prisoners", request.WorldDay, request.SaveRevision + 1,
                        new List<string> { result.EventId }, "prisoners_registered", "俘虏已由 PrisonerSystem 登记并进入可救援、释放或转移状态。",
                        "prisoners_registered",
                        new Dictionary<string, string> { ["case_id"] = boardingCase.CaseId, ["prisoner_count"] = prisonerDecision.Prisoners.Count.ToString(CultureInfo.InvariantCulture) });
                }
            }

            if (decision.RelationProposals != null)
                foreach (var proposal in decision.RelationProposals)
                {
                    if (proposal == null) continue;
                    var relationDecision = _relations.Apply(proposal, result.Fact);
                    if (relationDecision == null || !relationDecision.Allowed) continue;
                    _state.Relations.Add(new D14RelationConsequenceRecord
                    {
                        RelationId = "relation:" + proposal.SourceId + "->" + proposal.TargetId + ":" + proposal.Metric,
                        SourceId = proposal.SourceId, TargetId = proposal.TargetId, Metric = proposal.Metric,
                        ProposedDelta = proposal.ProposedDelta, AppliedDelta = relationDecision.AppliedDelta,
                        CauseEventId = result.EventId, RelationSourceEventId = relationDecision.SourceEventId, LastEventId = result.EventId,
                    });
                }
            _state.Revision++;
            result.State = CaptureSnapshot();
            return result;
        }

        public D14AftercareResult RequestLoot(D14LootRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "战利品请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var boardingCase = FindCaseMutable(request.CaseId);
            if (boardingCase == null || (boardingCase.Status != D14BoardingCaseStatus.Captured && boardingCase.Status != D14BoardingCaseStatus.Surrendered))
                return Reject("loot_case_invalid", "只有俘获或投降案件才能申请战利品裁决。", boardingCase);
            if (!HasOwnedFact(request.LegalSourceEventId, D14AftercareIds.LegalOwner))
                return Reject("legal_source_missing", "战利品需要已登记的法律/势力来源事实。", boardingCase);
            var decision = _loot.Grant(request, boardingCase.Clone(), FindFact(boardingCase.LastEventId));
            if (!ValidDecision(decision, D14AftercareIds.LootOwner))
                return Reject(decision == null ? "loot_authority_required" : decision.Code, decision == null ? "InventorySystem 未返回战利品决定。" : decision.Message, boardingCase);
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                decision.Allowed ? "loot_granted" : "loot_denied", decision.Message ?? "战利品结果已由 InventorySystem 裁决。", "loot_resolved",
                new Dictionary<string, string>
                {
                    ["case_id"] = boardingCase.CaseId, ["status"] = decision.Status.ToString(),
                    ["legal_source_event_id"] = request.LegalSourceEventId, ["inventory_source_event_id"] = decision.SourceEventId ?? "",
                });
            var loot = new D14LootRecord
            {
                LootId = "loot:" + boardingCase.CaseId, CaseId = boardingCase.CaseId, Status = decision.Status,
                LegalSourceEventId = request.LegalSourceEventId, InventorySourceEventId = decision.SourceEventId, LastEventId = result.EventId,
            };
            if (decision.Items != null) foreach (var item in decision.Items) if (item != null) loot.Items.Add(item.Clone());
            UpsertLoot(loot);
            result.Loot = loot.Clone(); result.State = CaptureSnapshot();
            return result;
        }

        public D14AftercareResult Rescue(D14RescueRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "救援请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var boardingCase = FindCaseMutable(request.CaseId);
            if (boardingCase == null) return Reject("rescue_case_missing", "救援案件不存在。", null);
            var prisoners = new List<D14PrisonerRecord>();
            foreach (var id in request.PrisonerIds ?? new List<string>())
            {
                var prisoner = FindPrisoner(id);
                if (prisoner == null || prisoner.CaseId != boardingCase.CaseId || prisoner.Status != D14PrisonerStatus.Held)
                    return Reject("prisoner_not_held", "救援列表包含不在押或不属于本案的俘虏。", boardingCase);
                prisoners.Add(prisoner);
            }
            if (prisoners.Count == 0) return Reject("rescue_targets_missing", "救援至少需要一名在押俘虏。", boardingCase);
            var decision = _medical.Rescue(request, boardingCase.Clone(), prisoners, FindFact(boardingCase.LastEventId));
            if (!ValidDecision(decision, D14AftercareIds.MedicalOwner))
                return Reject(decision == null ? "medical_authority_required" : decision.Code, decision == null ? "MedicalSystem 未返回救援决定。" : decision.Message, boardingCase);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, boardingCase);
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "prisoners_rescued", "救援已由 MedicalSystem 裁决，俘虏转为获救状态。", "rescue_resolved",
                new Dictionary<string, string> { ["case_id"] = boardingCase.CaseId, ["medical_source_event_id"] = decision.SourceEventId ?? "", ["rescued_count"] = decision.RescuedPrisonerIds.Count.ToString(CultureInfo.InvariantCulture) });
            var rescued = new D14RescueRecord { RescueId = "rescue:" + boardingCase.CaseId + ":" + request.OperationId, CaseId = boardingCase.CaseId, MedicalSourceEventId = decision.SourceEventId, LastEventId = result.EventId };
            foreach (var id in decision.RescuedPrisonerIds)
            {
                var prisoner = FindPrisonerMutable(id);
                if (prisoner == null) continue;
                prisoner.Status = D14PrisonerStatus.Rescued; prisoner.Rescued = true; prisoner.LastEventId = result.EventId;
                rescued.RescuedPrisonerIds.Add(id);
            }
            UpsertRescue(rescued);
            if (rescued.RescuedPrisonerIds.Count == prisoners.Count) boardingCase.Status = D14BoardingCaseStatus.Rescued;
            boardingCase.LastEventId = result.EventId; UpsertCase(boardingCase);
            result.Rescue = rescued.Clone(); result.BoardingCase = boardingCase.Clone(); result.State = CaptureSnapshot();
            return result;
        }

        public D14AftercareResult Treat(D14TreatmentRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "治疗请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var casualty = FindCasualtyMutable(request.CasualtyId);
            if (casualty == null || casualty.CaseId != request.CaseId || casualty.Status != D14CasualtyStatus.Wounded)
                return Reject("casualty_not_treatable", "只有本案的受伤人员可以治疗。", FindCaseMutable(request.CaseId));
            var decision = _medical.Treat(request, casualty.Clone(), FindFact(casualty.LastEventId ?? casualty.SourceEventId));
            if (!ValidDecision(decision, D14AftercareIds.MedicalOwner))
                return Reject(decision == null ? "medical_authority_required" : decision.Code, decision == null ? "MedicalSystem 未返回治疗决定。" : decision.Message, FindCaseMutable(request.CaseId));
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, FindCaseMutable(request.CaseId));
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                "casualty_treated", "治疗结果已由 MedicalSystem 裁决。", "treatment_resolved",
                new Dictionary<string, string> { ["case_id"] = request.CaseId, ["casualty_id"] = casualty.CasualtyId, ["medical_source_event_id"] = decision.SourceEventId ?? "", ["health_after"] = decision.HealthAfter.ToString(CultureInfo.InvariantCulture) });
            var treatment = new D14TreatmentRecord
            {
                TreatmentId = "treatment:" + casualty.CasualtyId, CaseId = request.CaseId, PatientId = casualty.ParticipantId,
                Status = decision.Status, HealthBefore = Math.Max(0, 100 - casualty.HealthLoss), HealthAfter = decision.HealthAfter,
                MedicalSourceEventId = decision.SourceEventId, LastEventId = result.EventId,
            };
            casualty.Status = decision.Status == D14TreatmentStatus.Treated || decision.Status == D14TreatmentStatus.Stabilized ? D14CasualtyStatus.Recovered : casualty.Status;
            casualty.LastEventId = result.EventId; UpsertCasualty(casualty); UpsertTreatment(treatment);
            result.Treatment = treatment.Clone(); result.State = CaptureSnapshot();
            return result;
        }

        public D14AftercareResult Interrogate(D14InterrogationRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "审讯请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var prisoner = FindPrisoner(request.PrisonerId);
            if (prisoner == null || prisoner.CaseId != request.CaseId || prisoner.Status != D14PrisonerStatus.Held)
                return Reject("prisoner_not_held", "只有在押俘虏可以进入审讯流程。", FindCaseMutable(request.CaseId));
            try
            {
                if (request.Content == null) throw new InvalidOperationException("审讯必须提供历史/敏感内容审查标签。");
                request.Content.Validate("interrogation.content", true);
            }
            catch (Exception ex) { return Reject("sensitive_review_required", ex.Message, FindCaseMutable(request.CaseId)); }
            D14InterrogationDecision decision = null;
            if (!request.Content.SkipSensitiveContent)
                decision = _interrogation.Interrogate(request, prisoner, FindFact(prisoner.LastEventId ?? prisoner.SourceEventId));
            if (request.Content.SkipSensitiveContent)
            {
                decision = new D14InterrogationDecision { Allowed = true, Code = "interrogation_skipped", Message = "玩家跳过了敏感审讯内容。", SourceOwnerSystem = D14AftercareIds.LegalOwner, SourceEventId = prisoner.SourceEventId, InformationRef = "skipped" };
            }
            if (!(request.Content.SkipSensitiveContent
                ? ValidDecision(decision, D14AftercareIds.LegalOwner, false)
                : ValidDecision(decision, D14AftercareIds.LegalOwner)))
                return Reject(decision == null ? "legal_review_required" : decision.Code, decision == null ? "法律/历史审查 authority 未返回审讯决定。" : decision.Message, FindCaseMutable(request.CaseId));
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, FindCaseMutable(request.CaseId));
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                decision.Code, decision.Message ?? "审讯结果已记录。", "interrogation_resolved",
                new Dictionary<string, string> { ["case_id"] = request.CaseId, ["prisoner_id"] = prisoner.PrisonerId, ["skipped"] = request.Content.SkipSensitiveContent.ToString(), ["information_ref"] = decision.InformationRef ?? "" });
            var interrogation = new D14InterrogationRecord
            {
                InterrogationId = "interrogation:" + prisoner.PrisonerId, CaseId = request.CaseId, PrisonerId = prisoner.PrisonerId,
                Skipped = request.Content.SkipSensitiveContent, InformationRef = decision.InformationRef, Content = request.Content.Clone(),
                SourceEventId = decision.SourceEventId, LastEventId = result.EventId,
            };
            _state.Interrogations.Add(interrogation); result.Interrogation = interrogation.Clone(); result.State = CaptureSnapshot();
            return result;
        }

        public D14AftercareResult HoldFuneral(D14FuneralRequest request)
        {
            if (!CanRequest(request == null ? null : request.OperationId, request == null ? 0 : request.SaveRevision))
                return Reject("invalid_request", "葬礼请求缺少操作号。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var boardingCase = FindCaseMutable(request.CaseId);
            if (boardingCase == null) return Reject("funeral_case_missing", "葬礼案件不存在。", null);
            var casualties = new List<D14CasualtyRecord>();
            foreach (var id in request.CasualtyIds ?? new List<string>())
            {
                var casualty = FindCasualty(id);
                if (casualty == null || casualty.CaseId != boardingCase.CaseId || casualty.Status != D14CasualtyStatus.Dead)
                    return Reject("deceased_casualty_required", "葬礼只能处理本案已裁决为死亡的人员。", boardingCase);
                casualties.Add(casualty);
            }
            if (casualties.Count == 0) return Reject("funeral_targets_missing", "葬礼至少需要一名死亡人员。", boardingCase);
            try
            {
                if (request.Content == null) throw new InvalidOperationException("葬礼必须提供历史/敏感内容审查标签。");
                request.Content.Validate("funeral.content", true);
            }
            catch (Exception ex) { return Reject("sensitive_review_required", ex.Message, boardingCase); }
            D14FuneralDecision decision;
            if (request.Content.SkipSensitiveContent)
                decision = new D14FuneralDecision { Allowed = true, Code = "funeral_skipped", Message = "玩家跳过了敏感葬礼内容。", Status = D14FuneralStatus.Skipped, SourceOwnerSystem = D14AftercareIds.FuneralOwner, SourceEventId = casualties[0].SourceEventId };
            else
                decision = _funeral.Hold(request, casualties, FindFact(boardingCase.LastEventId));
            if (!(request.Content.SkipSensitiveContent
                ? ValidDecision(decision, D14AftercareIds.FuneralOwner, false)
                : ValidDecision(decision, D14AftercareIds.FuneralOwner)))
                return Reject(decision == null ? "community_authority_required" : decision.Code, decision == null ? "CommunitySystem 未返回葬礼决定。" : decision.Message, boardingCase);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, boardingCase);
            var result = Commit(request.OperationId, request.WorldDay, request.SaveRevision, "open_sea", request.CauseEventIds,
                decision.Code, decision.Message ?? "葬礼结果已记录。", "funeral_resolved",
                new Dictionary<string, string> { ["case_id"] = boardingCase.CaseId, ["casualty_count"] = casualties.Count.ToString(CultureInfo.InvariantCulture), ["skipped"] = request.Content.SkipSensitiveContent.ToString() });
            var funeral = new D14FuneralRecord
            {
                FuneralId = "funeral:" + boardingCase.CaseId + ":" + request.OperationId, CaseId = boardingCase.CaseId, Status = decision.Status,
                Skipped = request.Content.SkipSensitiveContent, Content = request.Content.Clone(), CommunitySourceEventId = decision.SourceEventId, LastEventId = result.EventId,
            };
            foreach (var casualty in casualties) { casualty.BurialRequested = true; casualty.LastEventId = result.EventId; UpsertCasualty(casualty); funeral.CasualtyIds.Add(casualty.CasualtyId); }
            _state.Funerals.Add(funeral); result.Funeral = funeral.Clone(); result.State = CaptureSnapshot();
            return result;
        }

        private D14AftercareResult Commit(string operationId, int worldDay, long saveRevision, string location,
            IEnumerable<string> causes, string code, string message, string factCode, Dictionary<string, string> values)
        {
            return AppendLinkedEvent(operationId, worldDay, saveRevision, causes, code, message, factCode, values);
        }

        private D14AftercareResult AppendLinkedEvent(string operationId, int worldDay, long saveRevision, IEnumerable<string> causes,
            string code, string message, string factCode, Dictionary<string, string> values)
        {
            string eventId = D14AftercareIds.EventPrefix + operationId;
            string key = eventId + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture);
            var existing = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            if (existing != null || _appliedEventKeys.Contains(key))
                return new D14AftercareResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "该战后操作已结算，不重复修改人员、俘虏、战利品或治疗状态。", EventId = eventId, Fact = existing, State = CaptureSnapshot() };
            var causeList = ResolveCauses(causes);
            var actors = BuildActors();
            var append = _eventLog.Append(new WorldEvent(
                eventId, Math.Max(0, worldDay), "open_sea",
                D14AftercareIds.OwnerSystem,
                new EventResult { ResultCode = code, Summary = message ?? string.Empty, FactIds = new List<string> { "aftercare:" + factCode }, Values = values ?? new Dictionary<string, string>() },
                actors, new[] { "d14:boarding_aftercare" }, causeList, Math.Max(0L, saveRevision)));
            _appliedEventKeys.Add(key);
            _state.LastEventId = eventId;
            _state.WorldDay = Math.Max(_state.WorldDay, Math.Max(0, worldDay));
            _state.Revision = Math.Max(_state.Revision, Math.Max(0L, saveRevision));
            return new D14AftercareResult { Accepted = true, Code = code, Message = message, EventId = eventId, Fact = append.Fact, State = CaptureSnapshot() };
        }

        private D14AftercareResult DuplicateIfKnown(string operationId, long revision)
        {
            if (!CanRequest(operationId, revision)) return null;
            string id = D14AftercareIds.EventPrefix + operationId;
            string key = id + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
            if (!_appliedEventKeys.Contains(key) && _eventLog.Find(id, revision) == null) return null;
            return new D14AftercareResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "该战后操作已结算，不重复修改。", EventId = id, Fact = _eventLog.Find(id, revision), State = CaptureSnapshot() };
        }

        private D14AftercareResult Reject(string code, string message, D14BoardingCase boardingCase)
        {
            return new D14AftercareResult { Accepted = false, Code = code, Message = message, BoardingCase = boardingCase == null ? null : boardingCase.Clone(), State = CaptureSnapshot() };
        }

        private bool ValidDecision(object decision, string expectedOwner, bool requireOwnedSource = true)
        {
            if (decision == null) return false;
            string owner = null; string source = null;
            if (decision is D14BoardingDecision board) { owner = board.SourceOwnerSystem; source = board.SourceEventId; }
            else if (decision is D14CasualtyDecision casualty) { owner = casualty.SourceOwnerSystem; source = casualty.SourceEventId; }
            else if (decision is D14PrisonerDecision prisoner) { owner = prisoner.SourceOwnerSystem; source = prisoner.SourceEventId; }
            else if (decision is D14LootDecision loot) { owner = loot.SourceOwnerSystem; source = loot.SourceEventId; }
            else if (decision is D14MedicalDecision medical) { owner = medical.SourceOwnerSystem; source = medical.SourceEventId; }
            else if (decision is D14RescueDecision rescue) { owner = rescue.SourceOwnerSystem; source = rescue.SourceEventId; }
            else if (decision is D14InterrogationDecision interrogation) { owner = interrogation.SourceOwnerSystem; source = interrogation.SourceEventId; }
            else if (decision is D14FuneralDecision funeral) { owner = funeral.SourceOwnerSystem; source = funeral.SourceEventId; }
            else if (decision is D14RelationDecision relation) { owner = relation.SourceOwnerSystem; source = relation.SourceEventId; }
            if (!string.Equals(owner, expectedOwner, StringComparison.Ordinal)) return false;
            return requireOwnedSource ? HasOwnedFact(source, expectedOwner) : HasFact(source);
        }

        private bool HasOwnedFact(string eventId, string owner)
        {
            var fact = FindFact(eventId);
            return fact != null && string.Equals(fact.OwnerSystem, owner, StringComparison.Ordinal);
        }

        private bool HasFact(string eventId) { return FindFact(eventId) != null; }
        private FactEvent FindFact(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            foreach (var fact in _eventLog.Facts) if (string.Equals(fact.EventId, eventId, StringComparison.Ordinal)) return fact;
            return null;
        }

        private List<string> ResolveCauses(IEnumerable<string> causes)
        {
            var result = new List<string>();
            if (causes != null) foreach (var cause in causes) if (!string.IsNullOrWhiteSpace(cause) && !result.Contains(cause)) result.Add(cause);
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(_state.LastEventId)) result.Add(_state.LastEventId);
            return result;
        }

        private List<string> BuildActors()
        {
            var result = new List<string> { "player" };
            foreach (var item in _state.Cases)
            {
                if (item.PlayerParty != null) foreach (var participant in item.PlayerParty) AddUnique(result, participant.ParticipantId);
                if (item.EnemyParty != null) foreach (var participant in item.EnemyParty) AddUnique(result, participant.ParticipantId);
            }
            return result;
        }

        private D14BoardingCase FindCaseMutable(string id)
        {
            foreach (var item in _state.Cases) if (item.CaseId == id) return item;
            return null;
        }
        private D14PrisonerRecord FindPrisonerMutable(string id)
        {
            foreach (var item in _state.Prisoners) if (item.PrisonerId == id) return item;
            return null;
        }
        private D14CasualtyRecord FindCasualtyMutable(string id)
        {
            foreach (var item in _state.Casualties) if (item.CasualtyId == id) return item;
            return null;
        }
        private void UpsertCase(D14BoardingCase value) { ReplaceOrAdd(_state.Cases, value, x => x.CaseId); }
        private void UpsertCasualty(D14CasualtyRecord value) { ReplaceOrAdd(_state.Casualties, value, x => x.CasualtyId); }
        private void UpsertPrisoner(D14PrisonerRecord value) { ReplaceOrAdd(_state.Prisoners, value, x => x.PrisonerId); }
        private void UpsertLoot(D14LootRecord value) { ReplaceOrAdd(_state.Loot, value, x => x.LootId); }
        private void UpsertTreatment(D14TreatmentRecord value) { ReplaceOrAdd(_state.Treatments, value, x => x.TreatmentId); }
        private void UpsertRescue(D14RescueRecord value) { ReplaceOrAdd(_state.Rescues, value, x => x.RescueId); }
        private static void ReplaceOrAdd<T>(List<T> list, T value, Func<T, string> key)
        {
            string id = key(value);
            for (int i = 0; i < list.Count; i++) if (key(list[i]) == id) { list[i] = value; return; }
            list.Add(value);
        }
        private static void ValidateParty(List<D14BoardingParticipant> list, D14ParticipantSide side, string field)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null) throw new InvalidOperationException(field + " contains null.");
                list[i].Validate(field + "[" + i + "]");
                if (list[i].Side != side || !ids.Add(list[i].ParticipantId)) throw new InvalidOperationException(field + " contains wrong side or duplicate participant.");
            }
        }
        private static D14BoardingCaseStatus StatusFor(D14AftercareOutcome outcome)
        {
            switch (outcome)
            {
                case D14AftercareOutcome.Captured: return D14BoardingCaseStatus.Captured;
                case D14AftercareOutcome.Surrendered: return D14BoardingCaseStatus.Surrendered;
                case D14AftercareOutcome.Retreated: return D14BoardingCaseStatus.Retreated;
                case D14AftercareOutcome.Rescued: return D14BoardingCaseStatus.Rescued;
                case D14AftercareOutcome.Released: return D14BoardingCaseStatus.Released;
                default: return D14BoardingCaseStatus.Closed;
            }
        }
        private static bool CanRequest(string operationId, long revision) { return !string.IsNullOrWhiteSpace(operationId) && revision >= 0; }
        private static int Clamp01Hundred(int value) { return Math.Max(0, Math.Min(100, value)); }
        private static string F(float value) { return value.ToString("0.####", CultureInfo.InvariantCulture); }
        private static void AddUnique(List<string> values, string id) { if (!string.IsNullOrWhiteSpace(id) && !values.Contains(id)) values.Add(id); }

        private sealed class D14CombatBoardingRuleAuthority : ID14CombatBoardingAuthority
        {
            private readonly EventLog _log;
            public D14CombatBoardingRuleAuthority(EventLog log) { _log = log; }
            public D14BoardingDecision Evaluate(D14BoardingRequest request)
            {
                var combat = request == null ? null : request.CombatSnapshot;
                string source = combat == null ? null : combat.LastEventId;
                var result = new D14BoardingDecision { SourceEventId = source, PlayerMorale = AverageMorale(request == null ? null : request.PlayerParty), EnemyMorale = AverageMorale(request == null ? null : request.EnemyParty) };
                if (combat == null || source == null || Find(_log, source) == null) { result.Code = "combat_source_missing"; result.Message = "接舷必须引用已记录的 CombatSystem 事实。"; return result; }
                bool surrendered = combat.Status == D14NavalCombatStatus.Surrendered;
                bool active = combat.Status == D14NavalCombatStatus.Engaged || combat.Status == D14NavalCombatStatus.BoardingReady || surrendered;
                bool near = combat.Kinematics.Distance <= 30f || surrendered;
                bool crew = combat.PlayerDamage.Crew > 0 && combat.EnemyDamage.Crew > 0;
                result.Allowed = active && near && crew;
                result.Code = result.Allowed ? "boarding_ready" : "boarding_conditions_not_met";
                result.Message = result.Allowed ? "距离、人员和战斗状态满足接舷条件。" : "需要有效战斗状态、30以内距离和存活人员。";
                return result;
            }
            public D14BoardingDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase)
            {
                var result = new D14BoardingDecision { Allowed = true, SourceEventId = boardingCase.CombatSourceEventId, PlayerMorale = boardingCase.PlayerMorale, EnemyMorale = boardingCase.EnemyMorale, Outcome = request.RequestedOutcome, Message = "接舷结局已按请求类别和人员状态裁决。" };
                if (request.RequestedOutcome == D14AftercareOutcome.Captured)
                {
                    int playerScore = PartyScore(boardingCase.PlayerParty) + StableInt(boardingCase.CaseId + "|capture", 12);
                    int enemyScore = PartyScore(boardingCase.EnemyParty) + 10;
                    if (playerScore < enemyScore) { result.Outcome = D14AftercareOutcome.Retreated; result.Code = "boarding_repulsed"; result.Message = "接舷受阻，船员撤回，未歼灭对手。"; }
                    else { result.Code = "boarding_captured"; result.Message = "接舷成功，敌方失去战斗控制。"; }
                }
                else if (request.RequestedOutcome == D14AftercareOutcome.Retreated)
                {
                    result.Code = "boarding_retreat"; result.Message = "玩家选择撤退，案件以非歼灭结局结束。";
                }
                else if (request.RequestedOutcome == D14AftercareOutcome.Surrendered)
                {
                    result.Code = "boarding_surrendered"; result.Message = "对手已投降，进入俘虏和战后照护流程。";
                }
                else
                {
                    result.Outcome = D14AftercareOutcome.NonLethalDisengagement; result.Code = "boarding_nonlethal_disengagement"; result.Message = "双方脱离接舷距离，未发生歼灭。";
                }
                return result;
            }
            private static FactEvent Find(EventLog log, string id) { foreach (var fact in log.Facts) if (fact.EventId == id) return fact; return null; }
            private static int AverageMorale(List<D14BoardingParticipant> party) { if (party == null || party.Count == 0) return 0; int total = 0; foreach (var item in party) total += item.Morale; return total / party.Count; }
            private static int PartyScore(List<D14BoardingParticipant> party) { if (party == null) return 0; int score = 0; foreach (var item in party) score += item.Morale + WeaponScore(item.Weapon); return score; }
            private static int WeaponScore(D14BoardingWeapon weapon) { return weapon == D14BoardingWeapon.BoardingAxe || weapon == D14BoardingWeapon.Musket ? 12 : weapon == D14BoardingWeapon.Cutlass || weapon == D14BoardingWeapon.Pistol ? 9 : 4; }
            private static int StableInt(string value, int maximum) { unchecked { uint hash = 2166136261u; foreach (var c in value ?? "") { hash ^= c; hash *= 16777619u; } return (int)(hash % (uint)Math.Max(1, maximum)); } }
        }

        private sealed class UnavailableCasualtyAuthority : ID14CasualtyAuthority
        {
            public D14CasualtyDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, D14BoardingDecision decision) { return new D14CasualtyDecision { Code = "casualty_authority_required", Message = "必须由 CrewSystem/伤亡 authority 裁决伤亡。" }; }
        }
        private sealed class UnavailablePrisonerAuthority : ID14PrisonerAuthority
        {
            public D14PrisonerDecision Register(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, FactEvent fact) { return new D14PrisonerDecision { Code = "prisoner_authority_required", Message = "必须由 PrisonerSystem 裁决俘虏登记。" }; }
        }
        private sealed class UnavailableLootAuthority : ID14LootAuthority
        {
            public D14LootDecision Grant(D14LootRequest request, D14BoardingCase boardingCase, FactEvent fact) { return new D14LootDecision { Code = "loot_authority_required", Message = "必须由 InventorySystem/Legal authority 裁决战利品。" }; }
        }
        private sealed class UnavailableMedicalAuthority : ID14MedicalAuthority
        {
            public D14MedicalDecision Treat(D14TreatmentRequest request, D14CasualtyRecord casualty, FactEvent fact) { return new D14MedicalDecision { Code = "medical_authority_required", Message = "必须由 MedicalSystem 裁决治疗。" }; }
            public D14RescueDecision Rescue(D14RescueRequest request, D14BoardingCase boardingCase, IReadOnlyList<D14PrisonerRecord> prisoners, FactEvent fact) { return new D14RescueDecision { Code = "medical_authority_required", Message = "必须由 MedicalSystem 裁决救援。" }; }
        }
        private sealed class UnavailableInterrogationAuthority : ID14InterrogationAuthority
        {
            public D14InterrogationDecision Interrogate(D14InterrogationRequest request, D14PrisonerRecord prisoner, FactEvent fact) { return new D14InterrogationDecision { Code = "legal_authority_required", Message = "必须由法律/历史审查 authority 裁决审讯。" }; }
        }
        private sealed class UnavailableFuneralAuthority : ID14FuneralAuthority
        {
            public D14FuneralDecision Hold(D14FuneralRequest request, IReadOnlyList<D14CasualtyRecord> casualties, FactEvent fact) { return new D14FuneralDecision { Code = "community_authority_required", Message = "必须由 CommunitySystem 裁决葬礼。" }; }
        }
        private sealed class UnavailableRelationAuthority : ID14RelationAuthority
        {
            public D14RelationDecision Apply(D14RelationProposal proposal, FactEvent aftercareFact) { return new D14RelationDecision { Code = "relation_authority_required", Message = "必须由 RelationSystem 应用关系事实。" }; }
        }

        private static bool HasFact(EventLog log, string id) { foreach (var fact in log.Facts) if (fact.EventId == id) return true; return false; }

        /// <summary>将战后事实交给 D10 canonical RelationSystem，不在 A02 写关系数值。</summary>
        public sealed class D14D10RelationAuthority : ID14RelationAuthority
        {
            private readonly RelationSystem _relationSystem;
            public D14D10RelationAuthority(RelationSystem relationSystem) { _relationSystem = relationSystem ?? throw new ArgumentNullException("relationSystem"); }
            public D14RelationDecision Apply(D14RelationProposal proposal, FactEvent aftercareFact)
            {
                if (proposal == null || aftercareFact == null) return new D14RelationDecision { Code = "relation_input_missing", Message = "关系 authority 缺少事实或提议。" };
                RelationType type;
                switch (proposal.Metric)
                {
                    case D14RelationMetric.Trust: type = RelationType.Trust; break;
                    case D14RelationMetric.Loyalty: type = RelationType.Loyalty; break;
                    case D14RelationMetric.Fear: type = RelationType.Fear; break;
                    case D14RelationMetric.Respect: type = RelationType.Respect; break;
                    default: type = RelationType.Affinity; break;
                }
                var applied = _relationSystem.ApplyFactEvent(aftercareFact, new RelationChangeIntent(proposal.SourceId, proposal.TargetId, type, proposal.ProposedDelta, proposal.Basis));
                return new D14RelationDecision
                {
                    Allowed = applied != null && applied.Succeeded,
                    Code = applied == null ? "relation_denied" : applied.Code,
                    Message = applied == null ? "RelationSystem 未返回决定。" : applied.Message,
                    SourceOwnerSystem = D14AftercareIds.RelationOwner,
                    SourceEventId = aftercareFact.EventId,
                    AppliedDelta = applied == null ? 0f : applied.AppliedDelta,
                };
            }
        }
    }
}
