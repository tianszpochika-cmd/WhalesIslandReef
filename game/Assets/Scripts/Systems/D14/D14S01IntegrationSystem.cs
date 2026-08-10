// D14-S01 同步集成系统。
// 本系统只拥有跨域集成账本；所有领域结果由对应 owner authority 写入。

using System;
using System.Collections.Generic;
using System.Linq;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14S01IntegrationSystem
    {
        private readonly EventLog _eventLog;
        private readonly ID14S01ShipAuthority _ship;
        private readonly ID14S01CrewAuthority _crew;
        private readonly ID14S01CargoAuthority _cargo;
        private readonly ID14S01EconomyAuthority _economy;
        private readonly ID14S01LegalAuthority _legal;
        private readonly ID14S01QuestAuthority _quest;
        private readonly ID14S01RelationAuthority _relation;
        private readonly ID14S01EquipmentAuthority _equipment;
        private readonly ID14S01RewardAuthority _reward;
        private readonly D14S01IntegrationRuntimeSnapshot _state;

        public D14S01IntegrationSystem(EventLog eventLog = null,
            ID14S01ShipAuthority ship = null, ID14S01CrewAuthority crew = null, ID14S01CargoAuthority cargo = null,
            ID14S01EconomyAuthority economy = null, ID14S01LegalAuthority legal = null, ID14S01QuestAuthority quest = null,
            ID14S01RelationAuthority relation = null, ID14S01EquipmentAuthority equipment = null, ID14S01RewardAuthority reward = null,
            D14S01IntegrationRuntimeSnapshot initial = null)
        {
            _eventLog = eventLog ?? (initial == null || initial.EventLog == null ? new EventLog() : EventLog.FromSnapshot(initial.EventLog));
            _ship = ship; _crew = crew; _cargo = cargo; _economy = economy; _legal = legal; _quest = quest;
            _relation = relation; _equipment = equipment; _reward = reward;
            _state = initial == null ? new D14S01IntegrationRuntimeSnapshot() : initial.Clone();
            if (_state.EventLog == null) _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
        }

        public EventLog EventLog { get { return _eventLog; } }
        public D14S01IntegrationRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }
        public IReadOnlyList<D14S01IntegrationLedgerRecord> Ledger { get { return _state.Ledger.Select(value => value.Clone()).ToList().AsReadOnly(); } }

        public D14S01IntegrationResult IntegrateCombat(D14S01CombatIntegrationRequest request)
        {
            D14S01IntegrationResult failure; FactEvent source;
            if (!Prepare(request, D14S01IntegrationStage.Combat, out source, out failure)) return failure;
            var authorityEvents = new List<string>();
            string grantId = null; string legalAction = request.LegalIntent == null ? null : request.LegalIntent.ActionKind;

            var decision = Require(_ship, D14S01IntegrationIds.ShipOwner, "ship_authority_required", delegate { return _ship.ApplyCombat(request, request.CombatResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("ship_consequence_rejected", decision.Message);
            decision = Require(_crew, D14S01IntegrationIds.CrewOwner, "crew_authority_required", delegate { return _crew.ApplyCombat(request, request.CombatResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("crew_consequence_rejected", decision.Message);
            decision = Require(_economy, D14S01IntegrationIds.EconomyOwner, "economy_authority_required", delegate { return _economy.ApplyCombat(request, request.CombatResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("economy_consequence_rejected", decision.Message);
            if (request.LegalIntent != null)
            {
                decision = Require(_legal, D14S01IntegrationIds.LegalOwner, "legal_authority_required", delegate { return _legal.Apply(request.LegalIntent, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
                if (decision != null && !decision.Allowed) return Reject("legal_consequence_rejected", decision.Message);
            }
            decision = OptionalQuest(request.QuestId, D14S01IntegrationStage.Combat, source, request.WorldDay, request.SaveRevision, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("quest_consequence_rejected", decision.Message);
            decision = OptionalRelation(request.RelationTargetRef, D14S01IntegrationStage.Combat, source, request.WorldDay, request.SaveRevision, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("relation_consequence_rejected", decision.Message);
            if (request.Equipment != null)
            {
                decision = Require(_equipment, D14S01IntegrationIds.EquipmentOwner, "equipment_authority_required", delegate { return _equipment.Apply(request.Equipment, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
                if (decision != null && !decision.Allowed) return Reject("equipment_consequence_rejected", decision.Message);
            }
            if (request.Reward != null)
            {
                decision = Require(_reward, D14S01IntegrationIds.RewardOwner, "reward_authority_required", delegate { return _reward.Grant(request.Reward, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
                if (decision != null && !decision.Allowed) return Reject("reward_consequence_rejected", decision.Message);
                grantId = decision == null ? null : decision.GrantId;
            }
            return Commit(request.OperationId, D14S01IntegrationStage.Combat, source, request.WorldDay, request.SaveRevision, authorityEvents, grantId, legalAction, "combat_consequences_applied", "战斗事实已由船体、船员、经济、法律、任务、关系、装备和奖励所属系统接收。");
        }

        public D14S01IntegrationResult IntegrateAftercare(D14S01AftercareIntegrationRequest request)
        {
            D14S01IntegrationResult failure; FactEvent source;
            if (!Prepare(request, D14S01IntegrationStage.Aftercare, out source, out failure)) return failure;
            var authorityEvents = new List<string>();
            string grantId = null; string legalAction = request.LegalIntent == null ? null : request.LegalIntent.ActionKind;

            var decision = Require(_crew, D14S01IntegrationIds.CrewOwner, "crew_authority_required", delegate { return _crew.ApplyAftercare(request, request.AftercareResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("crew_aftercare_rejected", decision.Message);
            decision = Require(_cargo, D14S01IntegrationIds.InventoryOwner, "cargo_authority_required", delegate { return _cargo.ApplyAftercare(request, request.AftercareResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("cargo_consequence_rejected", decision.Message);
            decision = Require(_economy, D14S01IntegrationIds.EconomyOwner, "economy_authority_required", delegate { return _economy.ApplyAftercare(request, request.AftercareResult); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("economy_aftercare_rejected", decision.Message);
            if (request.LegalIntent != null)
            {
                decision = Require(_legal, D14S01IntegrationIds.LegalOwner, "legal_authority_required", delegate { return _legal.Apply(request.LegalIntent, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
                if (decision != null && !decision.Allowed) return Reject("legal_aftercare_rejected", decision.Message);
            }
            decision = OptionalQuest(request.QuestId, D14S01IntegrationStage.Aftercare, source, request.WorldDay, request.SaveRevision, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("quest_aftercare_rejected", decision.Message);
            decision = OptionalRelation(request.RelationTargetRef, D14S01IntegrationStage.Aftercare, source, request.WorldDay, request.SaveRevision, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("relation_aftercare_rejected", decision.Message);
            if (request.Reward != null)
            {
                decision = Require(_reward, D14S01IntegrationIds.RewardOwner, "reward_authority_required", delegate { return _reward.Grant(request.Reward, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
                if (decision != null && !decision.Allowed) return Reject("reward_aftercare_rejected", decision.Message);
                grantId = decision == null ? null : decision.GrantId;
            }
            return Commit(request.OperationId, D14S01IntegrationStage.Aftercare, source, request.WorldDay, request.SaveRevision, authorityEvents, grantId, legalAction, "aftercare_consequences_applied", "战后事实已由船员、货物、经济、法律、任务、关系和奖励所属系统接收。");
        }

        public D14S01IntegrationResult IntegrateEquipment(D14S01EquipmentIntegrationRequest request)
        {
            D14S01IntegrationResult failure; FactEvent source;
            if (!Prepare(request, D14S01IntegrationStage.Equipment, out source, out failure)) return failure;
            var authorityEvents = new List<string>();
            var decision = Require(_equipment, D14S01IntegrationIds.EquipmentOwner, "equipment_authority_required", delegate { return _equipment.Apply(request.Equipment, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("equipment_lifecycle_rejected", decision.Message);
            return Commit(request.OperationId, D14S01IntegrationStage.Equipment, source, request.WorldDay, request.SaveRevision, authorityEvents, null, null, "equipment_lifecycle_applied", "特殊装备操作已交由装备所属系统执行。");
        }

        public D14S01IntegrationResult IntegrateReward(D14S01RewardIntegrationRequest request)
        {
            D14S01IntegrationResult failure; FactEvent source;
            if (!Prepare(request, D14S01IntegrationStage.Reward, out source, out failure)) return failure;
            var authorityEvents = new List<string>();
            var decision = Require(_reward, D14S01IntegrationIds.RewardOwner, "reward_authority_required", delegate { return _reward.Grant(request.Reward, source, request.WorldDay, request.SaveRevision); }, authorityEvents);
            if (decision != null && !decision.Allowed) return Reject("reward_lifecycle_rejected", decision.Message);
            return Commit(request.OperationId, D14S01IntegrationStage.Reward, source, request.WorldDay, request.SaveRevision, authorityEvents, decision == null ? null : decision.GrantId, null, "reward_lifecycle_applied", "奖励请求已由 RewardSystem 处理，集成层未生成奖励值。");
        }

        public D14S01IntegrationRuntimeSnapshot CaptureSnapshot()
        {
            _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
            return _state.Clone();
        }

        private bool Prepare(object request, D14S01IntegrationStage stage, out FactEvent source, out D14S01IntegrationResult failure)
        {
            source = null; failure = null;
            if (request == null) { failure = Reject("request_required", "D14-S01 请求不能为空。"); return false; }
            string operationId; string requestedBy; long revision; int day; object result;
            var combat = request as D14S01CombatIntegrationRequest;
            var aftercare = request as D14S01AftercareIntegrationRequest;
            var equipment = request as D14S01EquipmentIntegrationRequest;
            var reward = request as D14S01RewardIntegrationRequest;
            if (combat != null) { operationId = combat.OperationId; requestedBy = combat.RequestedBySystem; revision = combat.SaveRevision; day = combat.WorldDay; result = combat.CombatResult; source = combat.CombatResult == null ? null : combat.CombatResult.Fact; try { combat.Validate("d14.s01.combat"); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; } }
            else if (aftercare != null) { operationId = aftercare.OperationId; requestedBy = aftercare.RequestedBySystem; revision = aftercare.SaveRevision; day = aftercare.WorldDay; result = aftercare.AftercareResult; source = aftercare.AftercareResult == null ? null : aftercare.AftercareResult.Fact; try { aftercare.Validate("d14.s01.aftercare"); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; } }
            else if (equipment != null) { operationId = equipment.OperationId; requestedBy = equipment.RequestedBySystem; revision = equipment.SaveRevision; day = equipment.WorldDay; result = null; try { equipment.Validate("d14.s01.equipment"); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; } source = _eventLog.Find(equipment.SourceEventId, revision); }
            else { var rewardRequest = reward; if (rewardRequest == null) { failure = Reject("unsupported_request", "未知的 D14-S01 请求类型。"); return false; } operationId = rewardRequest.OperationId; requestedBy = rewardRequest.RequestedBySystem; revision = rewardRequest.SaveRevision; day = rewardRequest.WorldDay; result = null; try { rewardRequest.Validate("d14.s01.reward"); } catch (Exception ex) { failure = Reject("invalid_request", ex.Message); return false; } source = _eventLog.Find(rewardRequest.SourceEventId, revision); }
            if (IsAgent(requestedBy)) { failure = Reject("agent_cannot_integrate", "Agent 只能提出战术或外交意图，不能提交最终跨域后果。"); return false; }
            string key = AppliedKey(operationId, revision);
            if (_state.AppliedEventKeys.Contains(key)) { failure = Duplicate(operationId, stage, source); return false; }
            if (source == null) { failure = Reject("source_fact_missing", "D14-S01 必须消费已存在的来源事实。"); return false; }
            if (result != null)
            {
                var combatResult = result as D14CombatResult;
                if (combatResult != null && (!combatResult.Accepted || combatResult.Fact == null)) { failure = Reject("combat_not_accepted", "战斗结果未被 CombatSystem 接受。"); return false; }
                var aftercareResult = result as D14AftercareResult;
                if (aftercareResult != null && (!aftercareResult.Accepted || aftercareResult.Fact == null)) { failure = Reject("aftercare_not_accepted", "战后结果未被 AftercareSystem 接受。"); return false; }
            }
            return true;
        }

        private D14S01AuthorityDecision OptionalQuest(string questId, D14S01IntegrationStage stage, FactEvent source, int day, long revision, List<string> authorityEvents)
        {
            if (string.IsNullOrWhiteSpace(questId)) return null;
            return Require(_quest, D14S01IntegrationIds.QuestOwner, "quest_authority_required", delegate { return _quest.Observe(questId, stage.ToString(), source, day, revision); }, authorityEvents);
        }

        private D14S01AuthorityDecision OptionalRelation(string target, D14S01IntegrationStage stage, FactEvent source, int day, long revision, List<string> authorityEvents)
        {
            if (string.IsNullOrWhiteSpace(target)) return null;
            return Require(_relation, D14S01IntegrationIds.RelationOwner, "relation_authority_required", delegate { return _relation.Apply(target, stage.ToString(), source, day, revision); }, authorityEvents);
        }

        private D14S01AuthorityDecision Require<T>(T authority, string owner, string missingCode, Func<D14S01AuthorityDecision> call, List<string> authorityEvents) where T : class
        {
            if (authority == null) return new D14S01AuthorityDecision { Allowed = false, Code = missingCode, Message = missingCode };
            var decision = call();
            if (decision == null) return new D14S01AuthorityDecision { Allowed = false, Code = "authority_null", Message = "所属系统没有返回决定。" };
            if (decision.Allowed && decision.SourceOwnerSystem == owner && !string.IsNullOrWhiteSpace(decision.SourceEventId) && FindAnyFact(decision.SourceEventId) != null)
            {
                if (!authorityEvents.Contains(decision.SourceEventId)) authorityEvents.Add(decision.SourceEventId);
                return decision;
            }
            if (decision.Allowed && decision.SourceOwnerSystem != owner) return new D14S01AuthorityDecision { Allowed = false, Code = "authority_owner_mismatch", Message = "所属系统来源 owner 不匹配。" };
            return decision;
        }

        private D14S01IntegrationResult Commit(string operationId, D14S01IntegrationStage stage, FactEvent source, int day, long revision, List<string> authorityEvents, string grantId, string legalAction, string code, string message)
        {
            var eventId = D14S01IntegrationIds.EventPrefix + stage.ToString().ToLowerInvariant() + "_" + Stable(operationId);
            var inputs = new List<string> { source.EventId }; foreach (var id in authorityEvents) if (!inputs.Contains(id)) inputs.Add(id);
            var append = _eventLog.Append(new WorldEvent(eventId, day, "d14_s01", D14S01IntegrationIds.OwnerSystem,
                new EventResult { ResultCode = code, Summary = message, FactIds = new List<string>(authorityEvents), Values = new Dictionary<string, string> { ["stage"] = stage.ToString(), ["authority_count"] = authorityEvents.Count.ToString(), ["reward_grant_id"] = grantId ?? string.Empty, ["legal_action"] = legalAction ?? string.Empty } },
                new[] { "player" }, inputs, inputs, revision));
            var ledger = new D14S01IntegrationLedgerRecord { OperationId = operationId, Stage = stage, SourceEventId = source.EventId, IntegrationEventId = append.Fact.EventId, WorldDay = day, SaveRevision = revision, AuthorityEventIds = new List<string>(authorityEvents), RewardGrantId = grantId, LegalActionKind = legalAction };
            _state.Ledger.Add(ledger); _state.AppliedEventKeys.Add(AppliedKey(operationId, revision)); _state.Revision++; _state.WorldDay = day; _state.LastEventId = append.Fact.EventId;
            return new D14S01IntegrationResult { Accepted = true, Code = code, Message = message, EventId = append.Fact.EventId, Fact = append.Fact.CloneFact(), Ledger = ledger.Clone(), State = CaptureSnapshot() };
        }

        private D14S01IntegrationResult Duplicate(string operationId, D14S01IntegrationStage stage, FactEvent source)
        {
            foreach (var record in _state.Ledger) if (record.OperationId == operationId && record.Stage == stage)
                return new D14S01IntegrationResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "D14-S01 已结算该跨域操作，不重复调用所属系统。", EventId = record.IntegrationEventId, Fact = FindAnyFact(record.IntegrationEventId), Ledger = record.Clone(), State = CaptureSnapshot() };
            return new D14S01IntegrationResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "D14-S01 已结算该操作。", EventId = source == null ? null : source.EventId, Fact = source, State = CaptureSnapshot() };
        }

        private D14S01IntegrationResult Reject(string code, string message) { return D14S01IntegrationResult.Rejected(code, message, CaptureSnapshot()); }
        private FactEvent FindAnyFact(string eventId) { if (string.IsNullOrWhiteSpace(eventId)) return null; foreach (var fact in _eventLog.Facts) if (fact.EventId == eventId) return fact; return null; }
        private static string AppliedKey(string operationId, long revision) { return (operationId ?? string.Empty) + "\u001f" + revision; }
        private static bool IsAgent(string value) { return string.Equals(value, "Agent", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "AgentReply", StringComparison.OrdinalIgnoreCase) || (value != null && value.IndexOf("AgentReply", StringComparison.OrdinalIgnoreCase) >= 0); }
        private static string Stable(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "empty";
            var chars = value.ToCharArray(); for (int i = 0; i < chars.Length; i++) { char c = chars[i]; if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')) chars[i] = '_'; }
            return new string(chars);
        }
    }
}
