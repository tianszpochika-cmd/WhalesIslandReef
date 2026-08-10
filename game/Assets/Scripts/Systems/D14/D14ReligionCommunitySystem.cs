// D14-A06 宗教/社群/历史审查裁决接入。
// 本类只写自己的仪式、誓言、内容审查和交互投影；关系、社群信任、建筑开放、葬礼
// 结果仍由 D10/D12/Building/Community authority 唯一写入。

using System;
using System.Collections.Generic;
using System.Linq;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14ReligionCommunitySystem
    {
        private readonly EventLog _eventLog;
        private readonly D14ReligionCommunityCatalog _catalog;
        private readonly ID14BuildingScheduleAuthority _building;
        private readonly ID14FuneralRiteAuthority _funeral;
        private readonly ID14D10TrustAuthority _d10Trust;
        private readonly ID14D12CommunityAuthority _community;
        private readonly D14ReligionCommunityRuntimeSnapshot _state;

        public const string OwnerSystem = D14ReligionCommunityIds.OwnerSystem;

        public D14ReligionCommunitySystem(EventLog eventLog = null, D14ReligionCommunityCatalog catalog = null,
            ID14BuildingScheduleAuthority building = null, ID14FuneralRiteAuthority funeral = null,
            ID14D10TrustAuthority d10Trust = null, ID14D12CommunityAuthority community = null,
            D14ReligionCommunityRuntimeSnapshot initialSnapshot = null)
        {
            _catalog = catalog ?? new D14ReligionCommunityCatalog();
            _eventLog = eventLog ?? (initialSnapshot == null || initialSnapshot.EventLog == null ? new EventLog() : EventLog.FromSnapshot(initialSnapshot.EventLog));
            _building = building;
            _funeral = funeral;
            _d10Trust = d10Trust;
            _community = community;
            _state = initialSnapshot == null ? new D14ReligionCommunityRuntimeSnapshot() : initialSnapshot.Clone();
            if (_state.EventLog == null) _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D14ReligionCommunityRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }
        public IReadOnlyList<D14HistoricalContentDefinition> Contents { get { return _catalog.Contents(); } }
        public IReadOnlyList<D14ReligionDefinition> Religions { get { return _catalog.Religions(); } }
        public IReadOnlyList<D14FestivalDefinition> Festivals { get { return _catalog.Festivals(); } }
        public IReadOnlyList<D14DietaryRuleDefinition> DietaryRules { get { return _catalog.DietaryRules(); } }
        public IReadOnlyList<D14EtiquetteDefinition> Etiquettes { get { return _catalog.Etiquettes(); } }
        public IReadOnlyList<D14BuildingRiteDefinition> BuildingRites { get { return _catalog.BuildingRites(); } }

        public D14ReligionPracticeState ReadPractice(string actorRef, string religionId)
        {
            foreach (var state in _state.ReligionStates)
                if (state.ActorRef == actorRef && state.ReligionId == religionId) return state.Clone();
            return new D14ReligionPracticeState { ActorRef = actorRef, ReligionId = religionId };
        }

        public D14FestivalRecord ReadFestival(string actionId) { foreach (var value in _state.Festivals) if (value.ActionId == actionId) return value.Clone(); return null; }
        public D14CommunityInteractionRecord ReadCommunityInteraction(string actionId) { foreach (var value in _state.CommunityInteractions) if (value.ActionId == actionId) return value.Clone(); return null; }

        /// <summary>Agent/UI 只能生成表达草案；此方法不追加事实、不改变宗教、社群、关系或法律状态。</summary>
        public D14ExpressionProposal ProposeExpression(D14ExpressionProposal proposal)
        {
            if (proposal == null || string.IsNullOrWhiteSpace(proposal.ProposalId) || string.IsNullOrWhiteSpace(proposal.ActorRef) || string.IsNullOrWhiteSpace(proposal.ContentId) || string.IsNullOrWhiteSpace(proposal.Text)) return null;
            var content = _catalog.FindContent(proposal.ContentId);
            if (content == null || content.ReviewStatus == D14SensitiveReviewStatus.Pending || content.ReviewStatus == D14SensitiveReviewStatus.Rejected || proposal.PerformanceLevel == D14PerformanceLevel.Omitted || !content.AllowedPerformanceLevels.Contains(proposal.PerformanceLevel)) return null;
            return proposal.Clone();
        }

        public D14ReligionCommunityResult ParticipateFestival(D14FestivalRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "festival", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            var festival = _catalog.FindFestival(request.FestivalId);
            if (festival == null || festival.ContentId != request.ContentId) return Reject("festival_invalid", "节期或内容引用无效。");
            if (!IsFestivalActive(festival, request.WorldDay)) return Reject("festival_not_active", "当前世界日不在该节期的确定性开放窗口内。");
            var fact = AppendFact("festival", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, skipped ? "sensitive_content_skipped" : "festival_participated", skipped ? "节期敏感内容已跳过，只保留审查事实。" : "节期参与已记录，后续建筑和社群礼仪可读取该事实。", new Dictionary<string, string> { ["festival_id"] = festival.FestivalId, ["religion_id"] = festival.ReligionId, ["performance_level"] = level.ToString(), ["skipped"] = skipped.ToString() });
            var record = new D14FestivalRecord { ActionId = request.OperationId, ActorRef = request.ActorRef, FestivalId = festival.FestivalId, ReligionId = festival.ReligionId, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            _state.Festivals.Add(record); AddReview(review, request.OperationId, request, level, skipped, fact.EventId);
            if (!skipped)
            {
                var practice = EnsurePractice(request.ActorRef, festival.ReligionId); practice.FestivalParticipationCount++; practice.LastFestivalId = festival.FestivalId; practice.LastWorldDay = request.WorldDay; practice.LastEventId = fact.EventId; practice.Revision++;
            }
            Touch(fact, request.OperationId, request.WorldDay);
            return Applied("festival_participated", skipped ? "节期内容已跳过。" : "节期已记录。", fact, level, review, festival: record);
        }

        public D14ReligionCommunityResult PracticeDietaryRule(D14DietaryRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "dietary", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            var rule = _catalog.FindDietaryRule(request.RuleId);
            if (rule == null || rule.ContentId != request.ContentId) return Reject("dietary_invalid", "饮食规则或内容引用无效。");
            if (string.IsNullOrWhiteSpace(request.FoodTag)) return Reject("food_tag_required", "饮食实践必须有食物标签来源。");
            bool compliant = string.Equals(rule.FoodTag, request.FoodTag, StringComparison.Ordinal);
            var fact = AppendFact("dietary", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, skipped ? "sensitive_content_skipped" : (compliant ? "dietary_rule_observed" : "dietary_rule_mismatch"), skipped ? "饮食敏感内容已跳过，只保留审查事实。" : (compliant ? "饮食实践符合已审查规则。" : "饮食实践与已审查规则不匹配。"), new Dictionary<string, string> { ["rule_id"] = rule.RuleId, ["food_tag"] = request.FoodTag, ["compliant"] = compliant.ToString(), ["skipped"] = skipped.ToString() });
            var practice = EnsurePractice(request.ActorRef, rule.ReligionId);
            if (!skipped && compliant) { practice.DietaryComplianceScore = Clamp(practice.DietaryComplianceScore + rule.ComplianceDelta, 0, 100); practice.LastDietaryRuleId = rule.RuleId; practice.LastWorldDay = request.WorldDay; practice.LastEventId = fact.EventId; practice.Revision++; }
            var record = new D14DietaryRecord { ActionId = request.OperationId, ActorRef = request.ActorRef, RuleId = rule.RuleId, FoodTag = request.FoodTag, ReligionId = rule.ReligionId, Compliant = compliant && !skipped, ComplianceAfter = practice.DietaryComplianceScore, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            _state.DietaryRecords.Add(record); AddReview(review, request.OperationId, request, level, skipped, fact.EventId); Touch(fact, request.OperationId, request.WorldDay);
            return Applied(compliant ? "dietary_rule_observed" : "dietary_rule_mismatch", skipped ? "饮食内容已跳过。" : (compliant ? "饮食规则已记录。" : "饮食规则不匹配，社群互动可据此暂缓。"), fact, level, review, dietary: record);
        }

        public D14ReligionCommunityResult RequestBuildingAccess(D14BuildingAccessRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "building_access", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            var rite = _catalog.FindBuildingRite(request.RiteId);
            if (rite == null || rite.ContentId != request.ContentId || request.LocalHour < 0 || request.LocalHour >= 24) return Reject("building_rite_invalid", "建筑仪式、内容或时间无效。");
            if (request.LocalHour < rite.OpenHour || request.LocalHour >= rite.CloseHour) return Reject("building_rite_closed", "当前时间不在该建筑仪式的确定性开放窗口内。");
            D14BuildingOpenDecision decision = null;
            if (!skipped)
            {
                if (_building == null) return Reject("building_authority_required", "建筑开放必须由 BuildingSystem 返回决定。");
                decision = _building.Evaluate(request, source.CloneFact());
                if (!ValidDecision(decision, D14ReligionCommunityIds.BuildingOwner)) return Reject("building_authority_invalid", "BuildingSystem 决定缺少来源或越过唯一写者边界。");
            }
            var fact = AppendFact("building_access", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, decision == null ? request.CauseEventIds : Merge(request.CauseEventIds, decision.SourceEventId), skipped ? "sensitive_content_skipped" : (decision.Allowed ? "building_opened_for_rite" : "building_access_refused"), skipped ? "建筑仪式内容已跳过。" : (decision.Allowed ? "建筑开放结果由 BuildingSystem 确认。" : "建筑当前不允许该仪式。"), new Dictionary<string, string> { ["rite_id"] = rite.RiteId, ["building_id"] = rite.BuildingId, ["building_status"] = decision == null ? "skipped" : decision.BuildingStatus, ["allowed"] = decision != null && decision.Allowed ? "true" : "false" });
            var record = new D14BuildingAccessRecord { ActionId = request.OperationId, ActorRef = request.ActorRef, RiteId = rite.RiteId, BuildingId = rite.BuildingId, Allowed = !skipped && decision.Allowed, BuildingStatus = skipped ? "skipped" : decision.BuildingStatus, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, BuildingSourceEventId = decision == null ? null : decision.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            _state.BuildingAccess.Add(record); AddReview(review, request.OperationId, request, level, skipped, fact.EventId); Touch(fact, request.OperationId, request.WorldDay);
            return Applied(record.Allowed ? "building_opened_for_rite" : (skipped ? "sensitive_content_skipped" : "building_access_refused"), record.Allowed ? "建筑已按 BuildingSystem 结果开放。" : (skipped ? "建筑仪式内容已跳过。" : "建筑开放被所属系统拒绝。"), fact, level, review, building: record);
        }

        public D14ReligionCommunityResult RecordFuneralRite(D14FuneralRiteRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "funeral_rite", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            if (string.IsNullOrWhiteSpace(request.FuneralId) || string.IsNullOrWhiteSpace(request.RiteId) || request.ContentId != "content_d14_funeral_remembrance") return Reject("funeral_rite_invalid", "葬礼仪式必须引用案件、仪式和已审查内容。");
            D14FuneralRiteDecision decision = null;
            if (!skipped)
            {
                if (_funeral == null) return Reject("funeral_authority_required", "葬礼结果必须由 CommunitySystem/Aftercare authority 返回。");
                decision = _funeral.Hold(request, source.CloneFact());
                if (!ValidDecision(decision, D14ReligionCommunityIds.D12CommunityOwner)) return Reject("funeral_authority_invalid", "葬礼 authority 决定缺少社群来源。");
            }
            var fact = AppendFact("funeral_rite", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, decision == null ? request.CauseEventIds : Merge(request.CauseEventIds, decision.SourceEventId), skipped ? "sensitive_content_skipped" : "funeral_rite_recorded", skipped ? "葬礼敏感内容已跳过，只保留来源和审查记录。" : "葬礼仪式结果由 CommunitySystem/Aftercare authority 返回。", new Dictionary<string, string> { ["funeral_id"] = request.FuneralId, ["rite_id"] = request.RiteId, ["status"] = skipped ? "Skipped" : decision.Status });
            var record = new D14FuneralRiteRecord { ActionId = request.OperationId, FuneralId = request.FuneralId, RiteId = request.RiteId, Status = skipped ? "Skipped" : decision.Status, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, CommunitySourceEventId = decision == null ? null : decision.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            _state.FuneralRites.Add(record); AddReview(review, request.OperationId, request, level, skipped, fact.EventId); Touch(fact, request.OperationId, request.WorldDay);
            return Applied(skipped ? "sensitive_content_skipped" : "funeral_rite_recorded", skipped ? "葬礼内容已跳过。" : "葬礼仪式已记录。", fact, level, review, funeral: record);
        }

        public D14ReligionCommunityResult ProcessOath(D14OathRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "oath", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            if (string.IsNullOrWhiteSpace(request.OathId) || string.IsNullOrWhiteSpace(request.ActorRef) || string.IsNullOrWhiteSpace(request.TargetRef) || request.ContentId != "content_d14_oath_mutual_aid" || !Enum.IsDefined(typeof(D14OathAction), request.Action) || !Enum.IsDefined(typeof(D14TrustDomain), request.TrustDomain)) return Reject("oath_invalid", "誓言必须有对象、动作、信任域和已审查内容。");
            var existing = FindOath(request.OathId);
            if (existing != null && request.Action == D14OathAction.Swear) return Duplicate("oath_duplicate", "该誓言已经开始。", oath: existing);
            if (request.Action != D14OathAction.Swear && (existing == null || !CanTransition(existing.Status, request.Action))) return Reject("oath_transition_invalid", "誓言状态不能执行该动作。");
            int proposed = ProposedOathDelta(request.Action);
            D14TrustDecision decision = null;
            if (!skipped)
            {
                if (request.TrustDomain == D14TrustDomain.CrewRelation)
                {
                    if (_d10Trust == null) return Reject("relation_authority_required", "船员信任必须由 D10 RelationSystem 裁决。");
                    decision = _d10Trust.ApplyOath(request, source.CloneFact(), proposed);
                    if (!ValidDecision(decision, D14ReligionCommunityIds.D10RelationOwner)) return Reject("relation_authority_invalid", "D10 信任决定缺少来源。");
                }
                else
                {
                    if (_community == null) return Reject("community_authority_required", "社群信任必须由 D12 CommunityTradeSystem 裁决。");
                    decision = _community.ApplyOath(request, source.CloneFact(), proposed);
                    if (!ValidDecision(decision, D14ReligionCommunityIds.D12CommunityOwner)) return Reject("community_authority_invalid", "D12 信任决定缺少来源。");
                }
            }
            var fact = AppendFact("oath", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, request.CauseEventIds, skipped ? "sensitive_content_skipped" : "oath_processed", skipped ? "誓言内容已跳过，没有改变关系或社群信任。" : "誓言事实已记录，信任结果由所属系统裁决。", new Dictionary<string, string> { ["oath_id"] = request.OathId, ["target_ref"] = request.TargetRef, ["action"] = request.Action.ToString(), ["proposed_trust_delta"] = proposed.ToString(), ["applied_trust_delta"] = decision == null ? "0" : decision.AppliedTrustDelta.ToString() }, new[] { request.TargetRef });
            var record = new D14OathRecord { ActionId = request.OperationId, OathId = request.OathId, ActorRef = request.ActorRef, TargetRef = request.TargetRef, ReligionId = request.ReligionId, CommunityId = request.CommunityId, Status = skipped ? D14OathStatus.Released : StatusFor(request.Action), Action = request.Action, TrustDomain = request.TrustDomain, ProposedTrustDelta = proposed, AppliedTrustDelta = decision == null ? 0 : decision.AppliedTrustDelta, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, TrustSourceEventId = decision == null ? null : decision.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            if (existing != null && !skipped) { existing.Status = record.Status; existing.Action = record.Action; existing.AppliedTrustDelta = record.AppliedTrustDelta; existing.TrustSourceEventId = record.TrustSourceEventId; existing.LastEventId = record.LastEventId; }
            else _state.Oaths.Add(record);
            AddReview(review, request.OperationId, request, level, skipped, fact.EventId); Touch(fact, request.OperationId, request.WorldDay);
            return Applied("oath_processed", skipped ? "誓言内容已跳过。" : "誓言已记录，信任由所属系统写入。", fact, level, review, oath: record);
        }

        public D14ReligionCommunityResult ResolveCommunityInteraction(D14CommunityInteractionRequest request)
        {
            D14ReligionCommunityResult failure; FactEvent source; D14ContentReviewRecord review; D14PerformanceLevel level; bool skipped;
            if (!Prepare(request, "community_interaction", out source, out failure) || !ReviewContent(request, out review, out level, out skipped, out failure)) return failure;
            var etiquette = _catalog.FindEtiquette(request.EtiquetteId);
            if (etiquette == null || etiquette.ContentId != request.ContentId || etiquette.CommunityId != request.CommunityId) return Reject("etiquette_invalid", "社群礼仪、社群或内容引用无效。");
            if (!string.IsNullOrWhiteSpace(request.FestivalId) && !HasFestival(request.ActorRef, request.FestivalId, request.WorldDay)) return Reject("festival_context_required", "该礼仪要求先完成已审查节期语境。");
            if (!string.IsNullOrWhiteSpace(request.DietaryRuleId) && !HasDietary(request.ActorRef, request.DietaryRuleId, request.WorldDay)) return Reject("dietary_context_required", "该礼仪要求先有符合规则的饮食实践。");
            int proposed = (string.IsNullOrWhiteSpace(request.FestivalId) ? 0 : 2) + (string.IsNullOrWhiteSpace(request.DietaryRuleId) ? 0 : 1);
            D14CommunityInteractionDecision decision = null;
            if (!skipped)
            {
                if (_community == null) return Reject("community_authority_required", "社群交互必须由 D12 CommunityTradeSystem 裁决。");
                decision = _community.Resolve(request, source.CloneFact(), proposed);
                if (decision == null || string.IsNullOrWhiteSpace(decision.SourceEventId) || decision.SourceOwnerSystem != D14ReligionCommunityIds.D12CommunityOwner || FindAnyFact(decision.SourceEventId) == null) return Reject("community_authority_invalid", "社群交互决定缺少 D12 来源事实。");
            }
            var fact = AppendFact("community_interaction", request.OperationId, request.WorldDay, request.SaveRevision, request.ActorRef, request.SourceEventId, request.EvidenceEventIds, decision == null ? request.CauseEventIds : Merge(request.CauseEventIds, decision.SourceEventId), skipped ? "sensitive_content_skipped" : (decision.Allowed ? "community_interaction_completed" : "community_interaction_refused"), skipped ? "社群礼仪内容已跳过。" : decision.Message, new Dictionary<string, string> { ["community_id"] = request.CommunityId, ["etiquette_id"] = request.EtiquetteId, ["festival_id"] = request.FestivalId ?? "", ["dietary_rule_id"] = request.DietaryRuleId ?? "", ["proposed_trust_delta"] = proposed.ToString(), ["applied_trust_delta"] = decision == null ? "0" : decision.AppliedTrustDelta.ToString() });
            var record = new D14CommunityInteractionRecord { ActionId = request.OperationId, ActorRef = request.ActorRef, CommunityId = request.CommunityId, EtiquetteId = request.EtiquetteId, FestivalId = request.FestivalId, DietaryRuleId = request.DietaryRuleId, Allowed = !skipped && decision.Allowed, OutcomeCode = skipped ? "sensitive_content_skipped" : decision.Code, ProposedTrustDelta = proposed, AppliedTrustDelta = decision == null ? 0 : decision.AppliedTrustDelta, TrustAfter = decision == null ? 0 : decision.TrustAfter, PerformanceLevel = level, Skipped = skipped, WorldDay = request.WorldDay, SourceEventId = request.SourceEventId, CommunitySourceEventId = decision == null ? null : decision.SourceEventId, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, LastEventId = fact.EventId };
            _state.CommunityInteractions.Add(record); AddReview(review, request.OperationId, request, level, skipped, fact.EventId); Touch(fact, request.OperationId, request.WorldDay);
            return Applied(record.OutcomeCode, record.Allowed ? "社群礼仪完成，D12 已裁决信任结果。" : (skipped ? "社群礼仪内容已跳过。" : "社群系统拒绝了该交互。"), fact, level, review, interaction: record);
        }

        public D14ReligionCommunityRuntimeSnapshot CaptureSnapshot()
        {
            _state.EventLog = _eventLog.CaptureSnapshot(); _state.Validate(); return _state.Clone();
        }

        private bool Prepare(D14ReligionCommunityRequest request, string action, out FactEvent source, out D14ReligionCommunityResult failure)
        {
            source = null; failure = null;
            if (request == null) { failure = Reject("request_required", action + " 请求不能为空。"); return false; }
            if (string.Equals(request.ActorRef, "agent", StringComparison.OrdinalIgnoreCase) || string.Equals(request.ActorRef, "AgentReply", StringComparison.OrdinalIgnoreCase)) { failure = Reject("agent_cannot_write", "Agent 只能提交表达草案，不能直接写宗教、社群或信任结果。"); return false; }
            try { request.Validate("d14." + action); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; }
            if (request.EvidenceEventIds.Count == 0) { failure = Reject("evidence_required", "宗教/社群内容必须有来源证据。"); return false; }
            if (_state.AppliedEventKeys.Contains("op:" + request.OperationId)) { failure = Duplicate("operation_duplicate", "同一宗教/社群操作已应用。"); return false; }
            source = _eventLog.Find(request.SourceEventId, request.SaveRevision);
            if (source == null) { failure = Reject("source_fact_missing", "宗教/社群操作来源事实不存在。"); return false; }
            foreach (var evidence in request.EvidenceEventIds) if (FindAnyFact(evidence) == null) { failure = Reject("evidence_fact_missing", "证据事实不存在：" + evidence); return false; }
            return true;
        }

        private bool ReviewContent(D14ReligionCommunityRequest request, out D14ContentReviewRecord review, out D14PerformanceLevel level, out bool skipped, out D14ReligionCommunityResult failure)
        {
            review = null; level = request == null ? D14PerformanceLevel.Contextual : request.PerformanceLevel; skipped = false; failure = null;
            var content = _catalog.FindContent(request == null ? null : request.ContentId);
            if (content == null) { failure = Reject("content_unknown", "内容 ID 未登记，不能生成历史细节。"); return false; }
            var history = FindAnyFact(request.HistoricalEvidenceEventId);
            if (history == null || history.OwnerSystem != D14ReligionCommunityIds.HistoricalReviewOwner) { failure = Reject("historical_evidence_required", "敏感内容必须引用 HistoricalReviewSystem 的审查事实。"); return false; }
            if (content.ReviewStatus == D14SensitiveReviewStatus.Pending || content.ReviewStatus == D14SensitiveReviewStatus.Rejected) { failure = Reject("content_review_blocked", "内容尚未通过历史/敏感审查。"); return false; }
            if (content.ReviewStatus == D14SensitiveReviewStatus.ApprovedWithWarning && !request.WarningAcknowledged && !request.SkipSensitiveContent) { failure = Reject("warning_required", content.ContentWarning); return false; }
            if (request.SkipSensitiveContent)
            {
                if (!content.SkipAllowed) { failure = Reject("skip_not_allowed", "该内容不允许跳过。"); return false; }
                skipped = true; level = D14PerformanceLevel.Omitted;
            }
            else if (!content.AllowedPerformanceLevels.Contains(request.PerformanceLevel)) { failure = Reject("performance_level_invalid", "请求的敏感内容表现等级未获审查允许。"); return false; }
            review = new D14ContentReviewRecord { ActionId = request.OperationId, ContentId = content.ContentId, ReviewStatus = content.ReviewStatus, PerformanceLevel = level, WarningAcknowledged = request.WarningAcknowledged, Skipped = skipped, HistoricalEvidenceEventId = request.HistoricalEvidenceEventId, WorldDay = request.WorldDay };
            return true;
        }

        private void AddReview(D14ContentReviewRecord review, string actionId, D14ReligionCommunityRequest request, D14PerformanceLevel level, bool skipped, string eventId)
        {
            review.ActionId = actionId; review.PerformanceLevel = level; review.Skipped = skipped; review.LastEventId = eventId; _state.Reviews.Add(review);
        }

        private D14ReligionPracticeState EnsurePractice(string actorRef, string religionId)
        {
            foreach (var value in _state.ReligionStates) if (value.ActorRef == actorRef && value.ReligionId == religionId) return value;
            var result = new D14ReligionPracticeState { ActorRef = actorRef, ReligionId = religionId }; _state.ReligionStates.Add(result); return result;
        }

        private D14OathRecord FindOath(string oathId) { foreach (var value in _state.Oaths) if (value.OathId == oathId) return value; return null; }
        private bool HasFestival(string actorRef, string festivalId, int worldDay) { return _state.Festivals.Any(value => value.ActorRef == actorRef && value.FestivalId == festivalId && !value.Skipped && worldDay - value.WorldDay <= 7 && worldDay >= value.WorldDay); }
        private bool HasDietary(string actorRef, string ruleId, int worldDay) { return _state.DietaryRecords.Any(value => value.ActorRef == actorRef && value.RuleId == ruleId && value.Compliant && !value.Skipped && worldDay - value.WorldDay <= 7 && worldDay >= value.WorldDay); }
        private static bool IsFestivalActive(D14FestivalDefinition definition, int worldDay) { var day = worldDay % definition.CycleDays; return day >= definition.StartDay && day < definition.StartDay + definition.DurationDays; }
        private static bool CanTransition(D14OathStatus status, D14OathAction action) { return (status == D14OathStatus.Sworn && (action == D14OathAction.Fulfill || action == D14OathAction.Break || action == D14OathAction.Release)); }
        private static int ProposedOathDelta(D14OathAction action) { switch (action) { case D14OathAction.Swear: return 5; case D14OathAction.Fulfill: return 8; case D14OathAction.Break: return -10; default: return 1; } }
        private static D14OathStatus StatusFor(D14OathAction action) { switch (action) { case D14OathAction.Swear: return D14OathStatus.Sworn; case D14OathAction.Fulfill: return D14OathStatus.Fulfilled; case D14OathAction.Break: return D14OathStatus.Broken; default: return D14OathStatus.Released; } }
        private bool ValidDecision(D14BuildingOpenDecision decision, string owner) { return decision != null && !string.IsNullOrWhiteSpace(decision.SourceEventId) && decision.SourceOwnerSystem == owner && FindAnyFact(decision.SourceEventId) != null; }
        private bool ValidDecision(D14FuneralRiteDecision decision, string owner) { return decision != null && !string.IsNullOrWhiteSpace(decision.SourceEventId) && decision.SourceOwnerSystem == owner && FindAnyFact(decision.SourceEventId) != null; }
        private bool ValidDecision(D14TrustDecision decision, string owner) { return decision != null && decision.Allowed && !string.IsNullOrWhiteSpace(decision.SourceEventId) && decision.SourceOwnerSystem == owner && FindAnyFact(decision.SourceEventId) != null; }
        private static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
        private static List<string> Merge(IEnumerable<string> values, params string[] additions) { var result = new List<string>(); if (values != null) foreach (var value in values) if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value); if (additions != null) foreach (var value in additions) if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value); return result; }
        private FactEvent FindAnyFact(string id) { if (string.IsNullOrWhiteSpace(id)) return null; foreach (var fact in _eventLog.Facts) if (fact.EventId == id) return fact; return null; }

        private FactEvent AppendFact(string action, string operationId, int worldDay, long saveRevision, string actor, string sourceEventId, IEnumerable<string> evidence, IEnumerable<string> causes, string resultCode, string summary, Dictionary<string, string> values, IEnumerable<string> additionalActors = null)
        {
            var input = new List<string> { sourceEventId }; if (evidence != null) foreach (var id in evidence) if (!input.Contains(id)) input.Add(id);
            var causeIds = Merge(causes, sourceEventId); var actors = new List<string> { string.IsNullOrWhiteSpace(actor) ? "player" : actor }; if (additionalActors != null) foreach (var id in additionalActors) if (!string.IsNullOrWhiteSpace(id) && !actors.Contains(id)) actors.Add(id);
            return _eventLog.Append(new WorldEvent(D14ReligionCommunityIds.EventPrefix + D14ReligionCommunityIds.SchemaVersion + "_" + action + "_" + D14Safe(operationId), worldDay, "d14_religion_community", OwnerSystem, new EventResult { ResultCode = resultCode, Summary = summary, FactIds = new List<string> { operationId }, Values = values }, actors, input, causeIds, saveRevision)).Fact;
        }

        private void Touch(FactEvent fact, string operationId, int worldDay) { _state.Revision++; _state.WorldDay = worldDay; _state.LastEventId = fact.EventId; AddUnique(_state.AppliedEventKeys, "op:" + operationId); AddUnique(_state.AppliedEventKeys, fact.DedupeKey); }
        private static void AddUnique(List<string> list, string value) { if (!string.IsNullOrWhiteSpace(value) && !list.Contains(value)) list.Add(value); }
        private static string D14Safe(string value) { if (string.IsNullOrWhiteSpace(value)) return "empty"; var chars = value.ToCharArray(); for (int i = 0; i < chars.Length; i++) { var c = chars[i]; if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')) chars[i] = '_'; } return new string(chars); }

        private D14ReligionCommunityResult Applied(string code, string message, FactEvent fact, D14PerformanceLevel level, D14ContentReviewRecord review, D14FestivalRecord festival = null, D14DietaryRecord dietary = null, D14BuildingAccessRecord building = null, D14FuneralRiteRecord funeral = null, D14OathRecord oath = null, D14CommunityInteractionRecord interaction = null)
        {
            return new D14ReligionCommunityResult { Accepted = true, Code = code, Message = message, EventId = fact.EventId, Fact = fact.CloneFact(), PerformanceLevel = level, Review = review == null ? null : review.Clone(), Festival = festival == null ? null : festival.Clone(), Dietary = dietary == null ? null : dietary.Clone(), Building = building == null ? null : building.Clone(), Funeral = funeral == null ? null : funeral.Clone(), Oath = oath == null ? null : oath.Clone(), CommunityInteraction = interaction == null ? null : interaction.Clone(), State = CaptureSnapshot() };
        }

        private D14ReligionCommunityResult Duplicate(string code, string message, D14OathRecord oath = null) { return new D14ReligionCommunityResult { Accepted = true, Duplicate = true, Code = code, Message = message, Oath = oath == null ? null : oath.Clone(), State = CaptureSnapshot() }; }
        private D14ReligionCommunityResult Reject(string code, string message) { return D14ReligionCommunityResult.Rejected(code, message, CaptureSnapshot()); }
    }
}
