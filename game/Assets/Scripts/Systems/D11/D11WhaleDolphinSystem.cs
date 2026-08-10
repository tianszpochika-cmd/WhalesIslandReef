// ============================================================
// D11WhaleDolphinSystem —— 鲸群/海豚路线确定性裁决
//
// 唯一写者：本系统只写 D11WhaleDolphinRuntimeSnapshot 的路线投影、
// 事实记录和待结算请求。A01 EcologySystem、D9 EnvironmentSystem、
// Faction/Legal、Ship/Crew、Reward、Quest 是各自事实的唯一写者；
// 本系统只能读取它们提供的快照，并把后果作为可追溯 request 交回。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public sealed class D11WhaleDolphinSystem
    {
        private readonly EventLog _eventLog;
        private readonly D11WhaleDolphinRuntimeSnapshot _state;

        public D11WhaleDolphinSystem(EventLog eventLog = null,
            D11WhaleDolphinRuntimeSnapshot initialSnapshot = null)
        {
            _eventLog = eventLog;
            _state = initialSnapshot == null
                ? new D11WhaleDolphinRuntimeSnapshot()
                : initialSnapshot.Clone();
            _state.EnsureDefaults();
            _state.Validate();
        }

        public D11WhaleDolphinRuntimeSnapshot Snapshot
        {
            get { return _state.Clone(); }
        }

        public D11MarineEncounter FindEncounter(string encounterId)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) return null;
            foreach (var encounter in _state.Encounters)
                if (encounter != null && string.Equals(encounter.EncounterId, encounterId, StringComparison.Ordinal))
                    return encounter.Clone();
            return null;
        }

        /// <summary>
        /// 读取 A01 EcologySystem 与 D9 EnvironmentSystem 的快照，确定性推进迁徙。
        /// 位置是路线投影，不是 Unity Transform；海流/天气不会被本系统写回。
        /// </summary>
        public D11WhaleDolphinRouteResult AdvanceMigration(string encounterId, string speciesId,
            D11MarineSpeciesKind speciesKind, string routeRef, int worldDay, int seed,
            D11MarineEcologyReadSnapshot ecology, D11MarineEnvironmentReadSnapshot environment,
            string actorId = "ecology", string locationRef = "open_sea", long saveRevision = 0,
            string eventId = null)
        {
            if (string.IsNullOrWhiteSpace(encounterId))
                return RejectWithoutEvent(D11MarineRouteAction.Migration, "encounter_missing", "鲸豚路线缺少稳定 encounter_id。", worldDay);
            if (speciesKind != D11MarineSpeciesKind.Whale && speciesKind != D11MarineSpeciesKind.Dolphin)
                return RejectWithoutEvent(D11MarineRouteAction.Migration, "species_unknown", "未知物种只保留 Unknown，不进入路线裁决。", worldDay);

            int day = Math.Max(0, worldDay);
            var safeEcology = ecology == null ? new D11MarineEcologyReadSnapshot() : ecology.Clone();
            var safeEnvironment = environment == null ? new D11MarineEnvironmentReadSnapshot() : environment.Clone();
            string id = string.IsNullOrWhiteSpace(eventId)
                ? BuildEventId(encounterId, D11MarineRouteAction.Migration, day)
                : eventId;
            if (IsDuplicate(id, saveRevision))
                return Duplicate(id, day, "迁徙事实已经记录，未重复推进鲸豚路线。", D11MarineRouteAction.Migration, encounterId);
            var encounter = FindEncounterInternal(encounterId);
            if (encounter == null)
            {
                encounter = new D11MarineEncounter
                {
                    EncounterId = encounterId,
                    SpeciesId = string.IsNullOrWhiteSpace(speciesId) ? speciesKind.ToString().ToLowerInvariant() : speciesId,
                    SpeciesKind = speciesKind,
                    RouteRef = string.IsNullOrWhiteSpace(routeRef) ? "open_sea_migration" : routeRef,
                };
                _state.Encounters.Add(encounter);
            }
            else if (encounter.SpeciesKind != speciesKind)
                return RejectWithoutEvent(D11MarineRouteAction.Migration, "encounter_species_mismatch", "稳定 encounter_id 的物种不可被改写。", day);

            int stable = Stable(encounterId + ":" + routeRef + ":" + day + ":" + seed);
            int direction = (stable % 36) * 10;
            float x = ((stable % 2001) - 1000) / 10f;
            float z = (((stable / 2001) % 2001) - 1000) / 10f;
            var record = NewRecord(encounterId, D11MarineRouteAction.Migration, D11RouteStatus.Recorded,
                "migration_recorded", "鲸群/海豚迁徙已按生境、海流和天气快照确定性记录。", id, day);
            record.HistoricalReviewTag = "not_applicable_migration";
            record.SensitiveContentReview = "not_applicable";
            record.CauseEventIds.Add("habitat:" + Safe(safeEcology.HabitatId, "open_sea"));
            var quest = NewQuestFact("fact_d11_migration_" + encounterId + "_" + day,
                id, speciesKind == D11MarineSpeciesKind.Whale ? "whale_migration_recorded" : "dolphin_route_recorded");
            var result = Commit(id, day, saveRevision, actorId, locationRef,
                D11MarineRouteAction.Migration, record, null, null, quest,
                new Dictionary<string, string>
                {
                    ["species_id"] = Safe(speciesId, "unknown"),
                    ["habitat_id"] = Safe(safeEcology.HabitatId, "open_sea"),
                    ["weather_state"] = Safe(safeEnvironment.WeatherStateId, "Calm"),
                    ["current_speed"] = Format(safeEnvironment.CurrentSpeed),
                    ["migration_direction_degrees"] = direction.ToString(CultureInfo.InvariantCulture),
                    ["world_x"] = Format(x),
                    ["world_z"] = Format(z),
                },
                WorldEventStatus.Normal, null,
                e =>
                {
                    e.RouteRef = string.IsNullOrWhiteSpace(routeRef) ? "open_sea_migration" : routeRef;
                    e.HabitatId = Safe(safeEcology.HabitatId, "open_sea");
                    e.WorldX = x;
                    e.WorldZ = z;
                    e.MigrationDirectionDegrees = direction;
                    e.LastMigratedDay = day;
                    e.EcologyStress = Clamp01(safeEcology.EcologyStress);
                    e.ProtectedPopulation = safeEcology.ProtectedPopulation;
                    e.IsStranded = safeEcology.IsStranded;
                    e.LastEventId = id;
                });
            return result;
        }

        /// <summary>执行观察、伴航、救助、捕鲸或放弃追踪的单一事实入口。</summary>
        public D11WhaleDolphinRouteResult Resolve(D11WhaleDolphinActionRequest request)
        {
            if (request == null) return RejectWithoutEvent(D11MarineRouteAction.Unknown, "request_missing", "鲸豚动作请求为空。", 0);
            var safe = request.Clone();
            int day = Math.Max(0, safe.WorldDay);
            var encounter = FindEncounterInternal(safe.EncounterId);
            if (encounter == null)
                return RejectWithoutEvent(safe.Action, "encounter_missing", "目标鲸豚遭遇不存在，不能凭 UI/Agent 生成事实。", day);
            if (safe.Action == D11MarineRouteAction.Unknown || safe.Action == D11MarineRouteAction.Migration)
                return RejectWithoutEvent(safe.Action, "action_unknown", "该路线动作不能在 Resolve 中直接裁决。", day);

            if (safe.Action == D11MarineRouteAction.Observation)
                return ResolveObservation(safe, encounter);
            if (safe.Action == D11MarineRouteAction.Escort)
                return ResolveEscort(safe, encounter);
            if (safe.Action == D11MarineRouteAction.StrandingRescue)
                return ResolveRescue(safe, encounter);
            if (safe.Action == D11MarineRouteAction.AbandonTracking)
                return ResolveAbandon(safe, encounter);
            if (safe.Action == D11MarineRouteAction.Whaling)
                return ResolveWhaling(safe, encounter);
            return RejectWithoutEvent(safe.Action, "action_unknown", "未知路线动作按拒绝处理。", day);
        }

        private D11WhaleDolphinRouteResult ResolveObservation(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter)
        {
            int day = Math.Max(0, request.WorldDay);
            var environment = request.Environment ?? new D11MarineEnvironmentReadSnapshot();
            if (environment.Visibility < 0.20f)
                return RejectAction(request, encounter, "observation_blocked_visibility", "能见度不足，观察未被强行判定为成功。", D11RouteStatus.Rejected);
            string id = ActionEventId(request, D11MarineRouteAction.Observation, day);
            var record = NewRecord(encounter.EncounterId, D11MarineRouteAction.Observation, D11RouteStatus.Completed,
                "observation_recorded", "观测记录已写入，并生成可由 RewardSystem/QuestSystem 消费的事实。", id, day);
            var reward = NewReward("observation_log_" + encounter.EncounterId + "_" + day,
                "observation_log", id, "鲸豚观测记录（非现金奖励）");
            var quest = NewQuestFact("fact_d11_observation_" + encounter.EncounterId + "_" + day,
                id, "marine_observation_recorded");
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                D11MarineRouteAction.Observation, record, reward, null, quest,
                Values("visibility", Format(environment.Visibility), "weather_state", Safe(environment.WeatherStateId, "Calm")),
                WorldEventStatus.Normal, null,
                e => { e.Observed = true; e.LastEventId = id; });
        }

        private D11WhaleDolphinRouteResult ResolveEscort(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter)
        {
            int day = Math.Max(0, request.WorldDay);
            var environment = request.Environment ?? new D11MarineEnvironmentReadSnapshot();
            if (encounter.SpeciesKind != D11MarineSpeciesKind.Dolphin)
                return RejectAction(request, encounter, "escort_target_not_dolphin", "伴航路线只适用于海豚遭遇。", D11RouteStatus.Rejected);
            if (environment.Visibility < 0.35f)
                return RejectAction(request, encounter, "escort_blocked_visibility", "能见度不足，未强行启动海豚伴航。", D11RouteStatus.Rejected);
            string id = ActionEventId(request, D11MarineRouteAction.Escort, day);
            var record = NewRecord(encounter.EncounterId, D11MarineRouteAction.Escort, D11RouteStatus.Completed,
                "dolphin_escort_recorded", "海豚伴航路线已记录，船舶只读取其影响。", id, day);
            var reward = NewReward("dolphin_route_" + encounter.EncounterId + "_" + day,
                "dolphin_route_chart", id, "海豚伴航路线图（非现金奖励）");
            var quest = NewQuestFact("fact_d11_dolphin_escort_" + encounter.EncounterId + "_" + day,
                id, "dolphin_escort_recorded");
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                D11MarineRouteAction.Escort, record, reward, null, quest,
                Values("weather_state", Safe(environment.WeatherStateId, "Calm"), "visibility", Format(environment.Visibility)),
                WorldEventStatus.Normal, null,
                e => { e.Escorted = true; e.LastEventId = id; });
        }

        private D11WhaleDolphinRouteResult ResolveRescue(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter)
        {
            int day = Math.Max(0, request.WorldDay);
            if (encounter.SpeciesKind != D11MarineSpeciesKind.Whale && encounter.SpeciesKind != D11MarineSpeciesKind.Dolphin)
                return RejectAction(request, encounter, "rescue_species_unknown", "未知物种不能进入救助结算。", D11RouteStatus.Rejected);
            if (!encounter.IsStranded)
                return RejectAction(request, encounter, "not_stranded", "目标没有搁浅事实，不能凭动作按钮生成救助结果。", D11RouteStatus.Rejected);
            if (!request.HasRescueKit || request.RescueCrewCount < 1)
                return RejectAction(request, encounter, "rescue_preparation_missing", "救助需要救助工具和至少一名船员。", D11RouteStatus.Rejected);
            string id = ActionEventId(request, D11MarineRouteAction.StrandingRescue, day);
            var record = NewRecord(encounter.EncounterId, D11MarineRouteAction.StrandingRescue, D11RouteStatus.Completed,
                "stranding_rescue_recorded", "搁浅救助已记录，生态与社会后果等待所属系统结算。", id, day);
            record.EcologyStressDelta = -0.20f;
            record.SocialReputationDelta = 5;
            var reward = NewReward("rescue_record_" + encounter.EncounterId + "_" + day,
                "rescue_reputation_record", id, "搁浅救助记录（由势力/奖励系统决定实际表彰）");
            var consequence = new D11ConsequenceRequest
            {
                RequestId = "consequence_rescue_" + encounter.EncounterId + "_" + day,
                OwnerSystem = "EcologySystem",
                SourceEventId = id,
                ConsequenceKind = "stranding_rescue_ecology_recovery",
                EcologyStressDelta = -0.20f,
                SocialReputationDelta = 5,
                LegalConsequenceCode = "protected_species_rescue_recorded",
            };
            var quest = NewQuestFact("fact_d11_rescue_" + encounter.EncounterId + "_" + day,
                id, "stranding_rescue_completed");
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                D11MarineRouteAction.StrandingRescue, record, reward, consequence, quest,
                Values("rescue_crew_count", request.RescueCrewCount.ToString(CultureInfo.InvariantCulture), "ecology_stress_delta", Format(-0.20f)),
                WorldEventStatus.Normal, null,
                e => { e.IsStranded = false; e.Rescued = true; e.LastEventId = id; });
        }

        private D11WhaleDolphinRouteResult ResolveAbandon(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter)
        {
            int day = Math.Max(0, request.WorldDay);
            string id = ActionEventId(request, D11MarineRouteAction.AbandonTracking, day);
            var record = NewRecord(encounter.EncounterId, D11MarineRouteAction.AbandonTracking, D11RouteStatus.Abandoned,
                "tracking_abandoned", "已放弃追踪；替代奖励保留为观测/路线记录，不发放捕鲸收益。", id, day);
            record.RecoveryRouteId = "alternative_observation_reward";
            var reward = NewReward("abandon_alternative_" + encounter.EncounterId + "_" + day,
                "abandon_tracking_alternative", id, "放弃追踪替代奖励：路线图或观察记录");
            var quest = NewQuestFact("fact_d11_abandon_" + encounter.EncounterId + "_" + day,
                id, "whaling_tracking_abandoned_alternative_route");
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                D11MarineRouteAction.AbandonTracking, record, reward, null, quest,
                Values("alternative_reward", "observation_or_route_record", "whaling_profit", "0"),
                WorldEventStatus.Normal, null,
                e => { e.TrackingAbandoned = true; e.LastEventId = id; });
        }

        private D11WhaleDolphinRouteResult ResolveWhaling(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter)
        {
            int day = Math.Max(0, request.WorldDay);
            var policy = request.WhalingPolicy ?? new D11WhalingPolicySnapshot();
            string gate = WhalingGate(encounter, policy);
            if (!string.IsNullOrEmpty(gate))
                return RejectAction(request, encounter, gate, ExplainWhalingGate(gate), D11RouteStatus.Rejected);
            if (encounter.WhalingAttempted || encounter.WhalingResolved)
                return RejectAction(request, encounter, "whaling_already_attempted", "同一鲸豚遭遇不允许重复追捕或重复发放收益。", D11RouteStatus.Rejected);

            string id = ActionEventId(request, D11MarineRouteAction.Whaling, day);
            int riskRoll = Stable(encounter.EncounterId + ":whaling:" + day + ":" + request.Seed) % 100;
            int riskThreshold = 20;
            bool successful = policy.EquipmentReady && policy.ShipConditionPercent >= 50 &&
                policy.CrewReadinessPercent >= 50 && riskRoll >= riskThreshold;
            if (!successful)
            {
                var failed = NewRecord(encounter.EncounterId, D11MarineRouteAction.Whaling, D11RouteStatus.Failed,
                    "whaling_failed", "追捕失败；船员/船体风险已记录，恢复路线为放弃追踪或观察。", id, day);
                failed.RiskRoll = riskRoll;
                failed.RiskThreshold = riskThreshold;
                failed.RecoveryRouteId = "abandon_tracking_or_rescue";
                var consequence = new D11ConsequenceRequest
                {
                    RequestId = "consequence_whaling_failure_" + encounter.EncounterId + "_" + day,
                    OwnerSystem = "ShipSystem",
                    SourceEventId = id,
                    ConsequenceKind = "whaling_failure_ship_crew_risk",
                    ShipRiskDelta = 0.15f,
                    CrewRiskDelta = 1,
                    LegalConsequenceCode = "failed_hunt_review_required",
                };
                return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                    D11MarineRouteAction.Whaling, failed, null, consequence, null,
                    Values("risk_roll", riskRoll.ToString(CultureInfo.InvariantCulture), "risk_threshold", riskThreshold.ToString(CultureInfo.InvariantCulture)),
                    WorldEventStatus.Failed,
                    new RecoveryRoute
                    {
                        RouteId = "abandon_tracking_or_rescue",
                        Action = "abandon_tracking_or_rescue",
                        ResumeFromEventId = id,
                        Description = "不得重复追捕；可改走观察、救助或放弃追踪替代路线。",
                    },
                    e => { e.WhalingAttempted = true; e.LastEventId = id; });
            }

            var record = NewRecord(encounter.EncounterId, D11MarineRouteAction.Whaling, D11RouteStatus.Completed,
                "licensed_whaling_recorded", "捕鲸条件通过且结果已记录；实际收益、生态和声望由所属系统结算。", id, day);
            record.RiskRoll = riskRoll;
            record.RiskThreshold = riskThreshold;
            record.EcologyStressDelta = 0.18f;
            record.SocialReputationDelta = policy.LawPolicy == D11WhalingLawPolicy.SubsistenceOnly ? 0 : -2;
            record.LegalConsequenceCode = "licensed_whaling_logged";
            var reward = NewReward("licensed_whale_processing_" + encounter.EncounterId + "_" + day,
                "licensed_whale_processing", id, "合法捕鲸处理请求（RewardSystem/InventorySystem 复核后结算）");
            var consequenceEcology = new D11ConsequenceRequest
            {
                RequestId = "consequence_whaling_ecology_" + encounter.EncounterId + "_" + day,
                OwnerSystem = "EcologySystem",
                SourceEventId = id,
                ConsequenceKind = "licensed_whaling_ecology_impact",
                EcologyStressDelta = 0.18f,
                SocialReputationDelta = record.SocialReputationDelta,
                LegalConsequenceCode = "licensed_whaling_logged",
            };
            var quest = NewQuestFact("fact_d11_whaling_" + encounter.EncounterId + "_" + day,
                id, "licensed_whaling_completed_reviewable");
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                D11MarineRouteAction.Whaling, record, reward, consequenceEcology, quest,
                Values("license_id", Safe(policy.LicenseId, "unknown"), "law_policy", policy.LawPolicy.ToString(),
                    "ecology_stress_delta", Format(record.EcologyStressDelta),
                    "social_reputation_delta", record.SocialReputationDelta.ToString(CultureInfo.InvariantCulture),
                    "risk_roll", riskRoll.ToString(CultureInfo.InvariantCulture)),
                WorldEventStatus.Normal, null,
                e => { e.WhalingAttempted = true; e.WhalingResolved = true; e.LastEventId = id; });
        }

        private D11WhaleDolphinRouteResult RejectAction(D11WhaleDolphinActionRequest request,
            D11MarineEncounter encounter, string code, string message, D11RouteStatus status)
        {
            int day = Math.Max(0, request.WorldDay);
            string id = ActionEventId(request, request.Action, day);
            var record = NewRecord(encounter.EncounterId, request.Action, status, code, message, id, day);
            record.RecoveryRouteId = request.Action == D11MarineRouteAction.Whaling
                ? "abandon_tracking_or_observation" : "retry_after_preparation";
            var recovery = new RecoveryRoute
            {
                RouteId = record.RecoveryRouteId,
                Action = record.RecoveryRouteId,
                ResumeFromEventId = id,
                Description = request.Action == D11MarineRouteAction.Whaling
                    ? "先观察或放弃追踪；修复许可证、季节、船员、船型和设施条件后才能重新申请。"
                    : "补齐事实或准备条件后再提交路线请求。",
            };
            return Commit(id, day, request.SaveRevision, request.ActorId, request.LocationRef,
                request.Action, record, null, null, null,
                Values("recovery_route", record.RecoveryRouteId), WorldEventStatus.Failed, recovery, null);
        }

        private string WhalingGate(D11MarineEncounter encounter, D11WhalingPolicySnapshot policy)
        {
            if (encounter.SpeciesKind != D11MarineSpeciesKind.Whale) return "target_not_whale";
            if (encounter.IsStranded || encounter.Rescued) return "stranding_route_required";
            if (encounter.ProtectedPopulation || policy.ProtectedPopulation) return "protected_population";
            if (policy.LicenseStatus != D11WhalingLicenseStatus.Valid || string.IsNullOrWhiteSpace(policy.LicenseId)) return "license_required";
            if (policy.LawPolicy != D11WhalingLawPolicy.CommercialAllowed && policy.LawPolicy != D11WhalingLawPolicy.SubsistenceOnly)
                return "law_policy_prohibits";
            if (!string.Equals(policy.RequiredSeasonId, "unknown", StringComparison.Ordinal) &&
                !string.Equals(policy.SeasonId, policy.RequiredSeasonId, StringComparison.Ordinal)) return "wrong_season";
            if (policy.CrewCount < Math.Max(1, policy.RequiredCrewCount)) return "crew_requirement_missing";
            if (string.IsNullOrWhiteSpace(policy.RequiredShipClassId) ||
                !string.Equals(policy.ShipClassId, policy.RequiredShipClassId, StringComparison.Ordinal)) return "ship_requirement_missing";
            if (!policy.HasProcessingFacility) return "processing_facility_missing";
            if (policy.SocialReputation < policy.MinimumSocialReputation) return "social_reputation_missing";
            if (policy.EcologyStress > 0.75f) return "ecology_stress_too_high";
            return "";
        }

        private static string ExplainWhalingGate(string code)
        {
            switch (code)
            {
                case "license_required": return "捕鲸必须有有效许可证，且许可证 ID 不能缺失。";
                case "law_policy_prohibits": return "当前法律/势力政策不允许该捕鲸分支。";
                case "wrong_season": return "当前季节不在许可证或任务允许窗口内。";
                case "crew_requirement_missing": return "船员数量不足，不能把捕鲸当成单人按钮。";
                case "ship_requirement_missing": return "船型不满足捕鲸路线要求。";
                case "processing_facility_missing": return "缺少处理设施，不能生成捕鲸收益。";
                case "social_reputation_missing": return "社会声望不足，当前路线不可用。";
                case "protected_population": return "目标属于保护种群，捕鲸路线不可用。";
                case "ecology_stress_too_high": return "当前生态压力过高，只能走观察、救助或放弃追踪路线。";
                case "stranding_route_required": return "目标存在搁浅/救助事实，应先处理救助路线。";
                default: return "捕鲸条件未满足，未发放收益。";
            }
        }

        private D11WhaleDolphinRouteResult Commit(string eventId, int day, long saveRevision,
            string actorId, string locationRef, D11MarineRouteAction action, D11RouteRecord record,
            D11RewardRequest reward, D11ConsequenceRequest consequence, D11QuestFactRequest quest,
            Dictionary<string, string> values, WorldEventStatus status, RecoveryRoute recovery,
            Action<D11MarineEncounter> mutateEncounter)
        {
            if (IsDuplicate(eventId, saveRevision))
                return Duplicate(eventId, day, "该鲸豚事实已存在，未重复改变路线状态或产生奖励请求。", action, record.EncounterId);
            record.SaveRevision = Math.Max(0L, saveRevision);
            record.RecordId = "record_d11_" + Stable(record.EventId + ":" + record.SaveRevision) + "_" + action.ToString().ToLowerInvariant();
            var evt = new WorldEvent(eventId, day, Safe(locationRef, "open_sea"), D11WhaleDolphinSchema.OwnerSystem,
                new EventResult
                {
                    ResultCode = record.Code,
                    Summary = record.Message,
                    FactIds = new List<string> { record.RecordId, "encounter:" + record.EncounterId },
                    Values = values ?? new Dictionary<string, string>(),
                },
                new[] { Safe(actorId, "system") },
                new[] { "d11_action:" + action.ToString(), "historical_review:" + record.HistoricalReviewTag },
                record.CauseEventIds, Math.Max(0L, saveRevision), status, recovery);
            if (_eventLog != null)
            {
                var append = _eventLog.Append(evt);
                if (append.IsDuplicate)
                    return Duplicate(eventId, day, "事件日志已存在该事实，未重复执行鲸豚路线。", action, record.EncounterId);
            }

            var encounter = FindEncounterInternal(record.EncounterId);
            if (mutateEncounter != null && encounter != null) mutateEncounter(encounter);
            AddUnique(_state.AppliedEventKeys, EventKey(eventId, saveRevision));
            _state.RouteRecords.Add(record.Clone());
            if (reward != null) _state.RewardRequests.Add(reward.Clone());
            if (consequence != null) _state.ConsequenceRequests.Add(consequence.Clone());
            if (quest != null) _state.QuestFacts.Add(quest.Clone());
            _state.WorldDay = Math.Max(_state.WorldDay, day);
            _state.Revision++;
            _state.LastEventId = eventId;
            return new D11WhaleDolphinRouteResult
            {
                Succeeded = status != WorldEventStatus.Failed,
                Applied = true,
                Code = record.Code,
                Message = record.Message,
                EventId = eventId,
                Record = record.Clone(),
                RewardRequests = reward == null ? new List<D11RewardRequest>() : new List<D11RewardRequest> { reward.Clone() },
                ConsequenceRequests = consequence == null ? new List<D11ConsequenceRequest>() : new List<D11ConsequenceRequest> { consequence.Clone() },
                QuestFacts = quest == null ? new List<D11QuestFactRequest>() : new List<D11QuestFactRequest> { quest.Clone() },
            };
        }

        private D11WhaleDolphinRouteResult RejectWithoutEvent(D11MarineRouteAction action,
            string code, string message, int day)
        {
            return new D11WhaleDolphinRouteResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
            };
        }

        private D11WhaleDolphinRouteResult Duplicate(string eventId, int day, string message,
            D11MarineRouteAction action, string encounterId)
        {
            return new D11WhaleDolphinRouteResult
            {
                Succeeded = true,
                Applied = false,
                Duplicate = true,
                Code = "duplicate_d11_event",
                Message = message,
                EventId = eventId,
                Record = NewRecord(encounterId, action, D11RouteStatus.Recorded, "duplicate_d11_event", message, eventId, day),
            };
        }

        private bool IsDuplicate(string eventId, long saveRevision)
        {
            string key = EventKey(eventId, saveRevision);
            if (_state.AppliedEventKeys.Contains(key)) return true;
            return _eventLog != null && _eventLog.Find(eventId, Math.Max(0L, saveRevision)) != null;
        }

        private D11MarineEncounter FindEncounterInternal(string encounterId)
        {
            foreach (var encounter in _state.Encounters)
                if (encounter != null && string.Equals(encounter.EncounterId, encounterId, StringComparison.Ordinal)) return encounter;
            return null;
        }

        private static D11RouteRecord NewRecord(string encounterId, D11MarineRouteAction action,
            D11RouteStatus status, string code, string message, string eventId, int day)
        {
            return new D11RouteRecord
            {
                RecordId = "record_d11_" + Stable(eventId) + "_" + action.ToString().ToLowerInvariant(),
                EncounterId = Safe(encounterId, "unknown"),
                Action = action,
                Status = status,
                Code = code,
                Message = message,
                EventId = eventId,
                WorldDay = Math.Max(0, day),
            };
        }

        private static D11RewardRequest NewReward(string requestId, string kind, string sourceEventId, string description)
        {
            return new D11RewardRequest
            {
                RequestId = requestId,
                RewardKind = kind,
                SourceEventId = sourceEventId,
                Description = description,
            };
        }

        private static D11QuestFactRequest NewQuestFact(string factId, string eventId, string kind)
        {
            return new D11QuestFactRequest { FactId = factId, SourceEventId = eventId, FactKind = kind };
        }

        private static string ActionEventId(D11WhaleDolphinActionRequest request, D11MarineRouteAction action, int day)
        {
            return string.IsNullOrWhiteSpace(request.EventId)
                ? BuildEventId(request.EncounterId, action, day) : request.EventId;
        }

        private static string BuildEventId(string encounterId, D11MarineRouteAction action, int day)
        {
            return "event_d11_" + Safe(encounterId, "unknown") + "_" + action.ToString().ToLowerInvariant() + "_" + day;
        }

        private static string EventKey(string eventId, long saveRevision)
        {
            return Safe(eventId, "") + "\u001f" + Math.Max(0L, saveRevision).ToString(CultureInfo.InvariantCulture);
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (!list.Contains(value)) list.Add(value);
        }

        private static Dictionary<string, string> Values(string k1, string v1, string k2, string v2)
        {
            return new Dictionary<string, string> { [k1] = v1, [k2] = v2 };
        }

        private static Dictionary<string, string> Values(string k1, string v1)
        {
            return new Dictionary<string, string> { [k1] = v1 };
        }

        private static Dictionary<string, string> Values(string k1, string v1, string k2, string v2, string k3, string v3)
        {
            var result = Values(k1, v1, k2, v2); result[k3] = v3; return result;
        }

        private static Dictionary<string, string> Values(string k1, string v1, string k2, string v2,
            string k3, string v3, string k4, string v4, string k5, string v5)
        {
            var result = Values(k1, v1, k2, v2, k3, v3); result[k4] = v4; result[k5] = v5; return result;
        }

        private static string Safe(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static string Format(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
