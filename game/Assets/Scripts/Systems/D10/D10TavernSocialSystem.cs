// ============================================================
// D10TavernSocialSystem —— 酒馆社会场景请求与 owner 编排
//
// 酒馆层只生成请求、目录和警告；Resolve 将请求转交给现有 Economy、Crew、
// Relation、Recruitment owner。这里不直接修改金钱、健康、疲劳或关系字段。
// 牌局只产生 MiniGameSystem 入口意图，不复制牌局规则或结算。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public sealed class D10TavernSocialSystem
    {
        public const string OwnerSystem = "TavernSystem";
        public const string DefaultTavernId = "tavern_santa_margarita";
        public const string DefaultPortId = "saint_marys";
        public const string DefaultLocationRef = "building:tavern_santa_margarita";

        private readonly EventLog _eventLog;

        public D10TavernSocialSystem(EventLog eventLog = null)
        {
            _eventLog = eventLog ?? new EventLog();
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D10TavernSceneSnapshot BuildSceneSnapshot(string portId, int worldDay)
        {
            string safePort = string.IsNullOrWhiteSpace(portId) ? DefaultPortId : portId;
            var snapshot = new D10TavernSceneSnapshot
            {
                TavernId = TavernIdFor(safePort),
                PortId = safePort,
                DisplayName = safePort == DefaultPortId ? "圣玛格丽特码头酒馆" : "港口酒馆",
            };
            snapshot.Rumors.AddRange(BuildRumors(safePort));
            snapshot.SideJobs.AddRange(BuildSideJobs(safePort, Math.Max(0, worldDay)));
            snapshot.PokerTables.Add(new D10PokerEntryIntent
            {
                TableId = "poker_table_low_stakes_01",
                RulesetId = "poker_caribbean_draw_v1",
                StakeBand = "low",
                Message = "只提交入局意图；牌面、下注、胜负和债务由 MiniGameSystem 结算。",
            });
            return snapshot;
        }

        public D10TavernRequestResult EnterTavern(string actorId, string portId,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            return RequestAction(new D10TavernRequest
            {
                Action = D10TavernAction.Enter,
                ActorId = actorId,
                PortId = portId,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                CauseEventIds = CopyIds(causeEventIds),
            });
        }

        public D10TavernRequestResult RequestRecruitment(string actorId, string portId,
            string candidateId, int worldDay, long saveRevision = 0,
            IEnumerable<string> causeEventIds = null)
        {
            var request = BaseRequest(D10TavernAction.Recruit, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.None,
                true, candidateId, string.Empty, string.Empty);
            request.RecruitCandidateId = candidateId;
            return RequestAction(request);
        }

        public D10TavernRequestResult RequestRest(string actorId, string portId,
            string crewId, int hours, int worldDay, long saveRevision = 0,
            IEnumerable<string> causeEventIds = null)
        {
            var request = BaseRequest(D10TavernAction.Rest, actorId, portId, worldDay,
                saveRevision, causeEventIds, D10TavernSensitivity.None, true, crewId,
                string.Empty, string.Empty);
            request.RestHours = hours;
            return RequestAction(request);
        }

        public D10TavernRequestResult RequestMeal(string actorId, string portId,
            string crewId, int worldDay, long saveRevision = 0,
            IEnumerable<string> causeEventIds = null)
        {
            return RequestAction(BaseRequest(D10TavernAction.Meal, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.None,
                true, crewId, string.Empty, string.Empty));
        }

        public D10TavernRequestResult RequestAlcohol(string actorId, string portId,
            string crewId, D10AlcoholStrength strength, bool warningAcknowledged,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            var request = BaseRequest(D10TavernAction.DrinkAlcohol, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.Alcohol,
                warningAcknowledged, crewId, string.Empty, string.Empty);
            request.AlcoholStrength = strength;
            return RequestAction(request);
        }

        public D10TavernRequestResult HearRumor(string actorId, string portId,
            string rumorId, int worldDay, long saveRevision = 0,
            IEnumerable<string> causeEventIds = null)
        {
            var request = BaseRequest(D10TavernAction.HearRumor, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.None,
                true, string.Empty, string.Empty, rumorId);
            request.RumorId = rumorId;
            return RequestAction(request);
        }

        public D10TavernRequestResult RequestPokerEntry(string actorId, string portId,
            string tableId, bool warningAcknowledged, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            var request = BaseRequest(D10TavernAction.PokerEntry, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.Gambling,
                warningAcknowledged, string.Empty, string.Empty, string.Empty);
            request.TargetId = tableId;
            return RequestAction(request);
        }

        public D10TavernRequestResult RequestDance(string actorId, string portId,
            string crewId, bool warningAcknowledged, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            return RequestSocial(D10TavernAction.Dance, actorId, portId, crewId,
                warningAcknowledged, worldDay, saveRevision, causeEventIds);
        }

        public D10TavernRequestResult RequestNightTalk(string actorId, string portId,
            string crewId, bool warningAcknowledged, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            return RequestSocial(D10TavernAction.NightTalk, actorId, portId, crewId,
                warningAcknowledged, worldDay, saveRevision, causeEventIds);
        }

        public D10TavernRequestResult OpenSideJobBoard(string actorId, string portId,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            return RequestAction(BaseRequest(D10TavernAction.SideJobBoard, actorId, portId,
                worldDay, saveRevision, causeEventIds, D10TavernSensitivity.None,
                true, string.Empty, string.Empty, string.Empty));
        }

        /// <summary>选择公告板上的具体副业；敏感副业先走警告/跳过门槛。</summary>
        public D10TavernRequestResult RequestSideJobOffer(string actorId, string portId,
            string offerId, bool warningAcknowledged, int worldDay, long saveRevision = 0,
            IEnumerable<string> causeEventIds = null)
        {
            var offer = FindSideJob(portId, offerId);
            if (offer == null)
                return D10TavernRequestResult.Rejected("side_job_not_found", "该副业不在当前公告板。");
            var request = BaseRequest(D10TavernAction.SideJobBoard, actorId, portId,
                worldDay, saveRevision, causeEventIds,
                offer.RequiresWarning ? D10TavernSensitivity.ExploitativeWork : D10TavernSensitivity.None,
                warningAcknowledged || !offer.RequiresWarning, string.Empty, string.Empty, string.Empty);
            request.SideJobId = offerId;
            return RequestAction(request);
        }

        public D10TavernRequestResult SkipSensitiveContent(D10TavernSensitivity sensitivity)
        {
            return new D10TavernRequestResult
            {
                Accepted = true,
                Status = D10TavernRequestStatus.Skipped,
                Code = "sensitive_skipped",
                Message = "已跳过" + SensitivityLabel(sensitivity) + "内容。",
            };
        }

        /// <summary>
        /// 将已记录的酒馆请求交给领域 owner；本方法不直接写领域字段。
        /// </summary>
        public D10TavernResolution Resolve(D10TavernRequest request, D10TavernOwnerServices owners)
        {
            if (request == null) return RejectResolution("request_required", "酒馆请求不存在。", request);
            if (owners == null) return RejectResolution("owner_services_required", "酒馆所属系统尚未就绪。", request);
            if (request.SchemaVersion != D10TavernConfiguration.CurrentSchemaVersion)
                return RejectResolution("schema_version_mismatch", "酒馆请求版本不匹配。", request);
            if (string.IsNullOrWhiteSpace(request.FactEventId)
                || _eventLog.Find(request.FactEventId, request.SaveRevision) == null)
                return RejectResolution("request_fact_missing", "酒馆请求尚未进入事实日志。", request);

            string resolutionEventId = ResolutionEventId(request);
            var existing = _eventLog.Find(resolutionEventId, request.SaveRevision);
            if (existing != null)
            {
                return new D10TavernResolution
                {
                    Accepted = true,
                    Duplicate = true,
                    Code = "duplicate_tavern_resolution",
                    Message = "酒馆请求已经结算，未重复执行所属系统。",
                    Request = request.Clone(),
                    Fact = existing,
                };
            }

            switch (request.Action)
            {
                case D10TavernAction.Enter:
                case D10TavernAction.HearRumor:
                case D10TavernAction.PokerEntry:
                case D10TavernAction.SideJobBoard:
                    return Finish(request, "tavern_request_recorded", "酒馆请求已记录；没有直接写入领域数值。",
                        null, null, null, request.Action == D10TavernAction.PokerEntry
                            ? PokerIntent(request.TargetId) : null);
                case D10TavernAction.Recruit:
                    return ResolveRecruitment(request, owners);
                case D10TavernAction.Rest:
                    return ResolveRest(request, owners);
                case D10TavernAction.Meal:
                    return ResolveMeal(request, owners);
                case D10TavernAction.DrinkAlcohol:
                    return ResolveAlcohol(request, owners);
                case D10TavernAction.Dance:
                case D10TavernAction.NightTalk:
                    return ResolveSocial(request, owners);
                default:
                    return RejectResolution("unknown_action", "未知酒馆行为。", request);
            }
        }

        public D10TavernRequestResult RequestAction(D10TavernRequest request)
        {
            string validationError = ValidateRequest(request);
            if (validationError != null)
                return D10TavernRequestResult.Rejected(validationError, RequestMessage(validationError));
            if (request.Sensitivity != D10TavernSensitivity.None && !request.WarningAcknowledged)
                return D10TavernRequestResult.WarningRequired(WarningFor(request.Sensitivity));

            request.SchemaVersion = D10TavernConfiguration.CurrentSchemaVersion;
            request.TavernId = TavernIdFor(request.PortId);
            request.RequestId = RequestId(request);
            request.FactEventId = RequestFactId(request);
            var append = _eventLog.Append(new WorldEvent(
                request.FactEventId,
                Math.Max(0, request.WorldDay),
                "building:" + request.TavernId,
                OwnerSystem,
                new EventResult
                {
                    ResultCode = "tavern_request_" + request.Action.ToString().ToLowerInvariant(),
                    Summary = RequestSummary(request),
                    FactIds = new List<string> { "tavern:" + request.TavernId },
                    Values = RequestValues(request),
                },
                Actors(request),
                new[] { "tavern:" + request.TavernId },
                request.CauseEventIds,
                Math.Max(0, request.SaveRevision)));

            var result = new D10TavernRequestResult
            {
                Accepted = true,
                Duplicate = append.IsDuplicate,
                Status = append.IsDuplicate ? D10TavernRequestStatus.Duplicate : D10TavernRequestStatus.Accepted,
                Code = append.IsDuplicate ? "duplicate_tavern_request" : "tavern_request_accepted",
                Message = append.IsDuplicate ? "同一酒馆请求已经记录。" : RequestSummary(request),
                Request = request.Clone(),
                Fact = append.Fact,
            };
            if (request.Action == D10TavernAction.DrinkAlcohol)
                result.AlcoholEffect = D10TavernConfiguration.AlcoholEffect(request.AlcoholStrength);
            if (request.Action == D10TavernAction.HearRumor)
                result.Rumor = FindRumor(request.PortId, request.RumorId);
            if (request.Action == D10TavernAction.PokerEntry)
                result.PokerEntry = PokerIntent(request.TargetId);
            if (request.Action == D10TavernAction.SideJobBoard)
                result.SideJob = FindSideJob(request.PortId, request.SideJobId);
            return result;
        }

        private D10TavernResolution ResolveRecruitment(D10TavernRequest request,
            D10TavernOwnerServices owners)
        {
            if (owners.Preparation == null || owners.TradeState == null || owners.Ship == null
                || owners.Crew == null || owners.RecruitCandidateResolver == null)
                return RejectResolution("recruitment_owner_missing", "招募所属系统尚未就绪。", request);
            var candidate = owners.RecruitCandidateResolver(request.RecruitCandidateId);
            var recruitment = owners.Preparation.Recruit(owners.TradeState, owners.Ship,
                request.PortId, owners.Crew, candidate);
            if (recruitment == null || !recruitment.Accepted)
                return new D10TavernResolution
                {
                    Accepted = false,
                    Code = recruitment == null ? "recruitment_failed" : recruitment.Code,
                    Message = recruitment == null ? "招募系统没有返回结果。" : recruitment.Message,
                    Request = request.Clone(),
                    Recruitment = recruitment,
                };
            return Finish(request, "recruitment_requested", recruitment.Message,
                recruitment, null, null, null);
        }

        private D10TavernResolution ResolveRest(D10TavernRequest request,
            D10TavernOwnerServices owners)
        {
            var crew = FindCrew(owners.Crew, request.TargetId);
            if (owners.CrewBehavior == null || crew == null)
                return RejectResolution("crew_owner_missing", "休息所属 CrewSystem 尚未就绪。", request);
            var initialized = owners.CrewBehavior.EnsureState(crew, request.WorldDay,
                request.SaveRevision, new[] { request.FactEventId });
            if (!initialized.Accepted)
                return RejectResolution(initialized.Code, initialized.Message, request);
            var rest = owners.CrewBehavior.Tick(crew, CrewShift.OffDuty, request.RestHours,
                request.WorldDay, request.SaveRevision, new[] { request.FactEventId });
            if (!rest.Accepted)
                return new D10TavernResolution { Accepted = false, Code = rest.Code,
                    Message = rest.Message, Request = request.Clone(), CrewBehavior = rest };
            return Finish(request, "rest_resolved", "休息请求已由 CrewSystem 结算。",
                null, rest, null, null);
        }

        private D10TavernResolution ResolveMeal(D10TavernRequest request,
            D10TavernOwnerServices owners)
        {
            var crew = FindCrew(owners.Crew, request.TargetId);
            if (owners.Economy == null || owners.TradeState == null || owners.CrewBehavior == null
                || crew == null)
                return RejectResolution("meal_owner_missing", "吃饭所属系统尚未就绪。", request);
            if (!owners.Economy.CanSpend(owners.TradeState, D10TavernConfiguration.MealPriceQuote))
                return RejectResolution("not_enough_money", "金币不足，无法购买餐食。", request);
            var meal = owners.CrewBehavior.ApplyHealthChange(crew,
                D10TavernConfiguration.MealHealthDelta, request.WorldDay,
                request.SaveRevision, new[] { request.FactEventId });
            if (!meal.Accepted)
                return new D10TavernResolution { Accepted = false, Code = meal.Code,
                    Message = meal.Message, Request = request.Clone(), CrewBehavior = meal };
            if (!owners.Economy.TrySpend(owners.TradeState, D10TavernConfiguration.MealPriceQuote,
                out string spendError))
                return RejectResolution(spendError, "餐费结算失败。", request);
            return Finish(request, "meal_resolved", "餐食已由 Economy/CrewSystem 结算。",
                null, meal, null, null);
        }

        private D10TavernResolution ResolveAlcohol(D10TavernRequest request,
            D10TavernOwnerServices owners)
        {
            var crew = FindCrew(owners.Crew, request.TargetId);
            var effect = D10TavernConfiguration.AlcoholEffect(request.AlcoholStrength);
            if (owners.Economy == null || owners.TradeState == null || owners.CrewBehavior == null
                || crew == null)
                return RejectResolution("alcohol_owner_missing", "饮酒所属系统尚未就绪。", request);
            if (!owners.Economy.CanSpend(owners.TradeState, effect.PriceQuote))
                return RejectResolution("not_enough_money", "金币不足，无法购买饮品。", request);
            var alcohol = owners.CrewBehavior.ApplyAlcoholEffect(crew, effect.ProfileId,
                effect.HealthDelta, effect.FatigueDelta, effect.ConflictRisk,
                request.WorldDay, request.SaveRevision, new[] { request.FactEventId });
            if (!alcohol.Accepted)
                return new D10TavernResolution { Accepted = false, Code = alcohol.Code,
                    Message = alcohol.Message, Request = request.Clone(), CrewBehavior = alcohol };
            if (!owners.Economy.TrySpend(owners.TradeState, effect.PriceQuote, out string spendError))
                return RejectResolution(spendError, "饮品费用结算失败。", request);

            D10RelationChangeResult conflict = null;
            bool conflictCondition = effect.ConflictRisk >= 0.25f
                && (crew.Stats.Morale < 50f
                    || crew.BehaviorState.Status == CrewBehaviorStatus.Slacking
                    || crew.BehaviorState.Status == CrewBehaviorStatus.Refusing);
            if (conflictCondition && owners.Relations != null
                && !string.IsNullOrWhiteSpace(owners.RelationTargetId))
            {
                var conflictCauses = new List<string> { request.FactEventId };
                if (alcohol.Fact != null && !string.IsNullOrWhiteSpace(alcohol.Fact.EventId))
                    conflictCauses.Add(alcohol.Fact.EventId);
                conflict = owners.Relations.Apply(new D10RelationActionRequest
                {
                    SourceId = crew.Id,
                    TargetId = owners.RelationTargetId,
                    Action = D10RelationAction.Argument,
                    WorldDay = request.WorldDay,
                    SaveRevision = Math.Max(0, request.SaveRevision + 1),
                    ContextRef = "tavern:alcohol_conflict",
                    CauseEventIds = conflictCauses,
                });
            }
            return Finish(request, conflict == null ? "alcohol_resolved" : "alcohol_conflict_resolved",
                conflict == null ? effect.ConsequenceSummary : "饮酒后果已结算，并触发了可追溯的冲突事实。",
                null, alcohol, conflict, null);
        }

        private D10TavernResolution ResolveSocial(D10TavernRequest request,
            D10TavernOwnerServices owners)
        {
            if (owners.Relations == null)
                return Finish(request, "social_request_recorded", "社交请求已记录，等待关系系统接管。",
                    null, null, null, null);
            var relation = owners.Relations.Apply(new D10RelationActionRequest
            {
                SourceId = request.ActorId,
                TargetId = request.TargetId,
                Action = D10RelationAction.NightTalk,
                WorldDay = request.WorldDay,
                SaveRevision = Math.Max(0, request.SaveRevision + 1),
                ContextRef = request.Action == D10TavernAction.Dance
                    ? "tavern:dance" : "tavern:night_talk",
                CauseEventIds = new List<string> { request.FactEventId },
            });
            if (!relation.Succeeded)
                return new D10TavernResolution { Accepted = false, Code = relation.Code,
                    Message = relation.Message, Request = request.Clone(), Relation = relation };
            return Finish(request, "social_resolved", "社交行为已由关系系统结算。",
                null, null, relation, null);
        }

        private D10TavernResolution Finish(D10TavernRequest request, string code,
            string message, D9PreparationResult recruitment, D10CrewBehaviorResult crew,
            D10RelationChangeResult relation, D10PokerEntryIntent poker)
        {
            var resolutionFact = AppendResolution(request, code, message, recruitment, crew, relation);
            return new D10TavernResolution
            {
                Accepted = true,
                Code = code,
                Message = message,
                Request = request.Clone(),
                Fact = resolutionFact,
                Recruitment = recruitment,
                CrewBehavior = crew,
                Relation = relation,
                PokerEntry = poker,
            };
        }

        private FactEvent AppendResolution(D10TavernRequest request, string code,
            string message, D9PreparationResult recruitment, D10CrewBehaviorResult crew,
            D10RelationChangeResult relation)
        {
            var causes = new List<string> { request.FactEventId };
            if (recruitment != null && recruitment.Fact != null) causes.Add(recruitment.Fact.EventId);
            if (crew != null && crew.Fact != null) causes.Add(crew.Fact.EventId);
            if (relation != null && relation.Fact != null) causes.Add(relation.Fact.EventId);
            return _eventLog.Append(new WorldEvent(
                ResolutionEventId(request), request.WorldDay, DefaultLocationRef, OwnerSystem,
                new EventResult
                {
                    ResultCode = code,
                    Summary = message,
                    FactIds = new List<string> { "tavern:" + request.TavernId },
                    Values = new Dictionary<string, string>
                    {
                        ["action"] = request.Action.ToString(),
                        ["target_id"] = request.TargetId ?? string.Empty,
                        ["recruitment_code"] = recruitment == null ? string.Empty : recruitment.Code,
                        ["crew_code"] = crew == null ? string.Empty : crew.Code,
                        ["relation_code"] = relation == null ? string.Empty : relation.Code,
                    },
                }, Actors(request), new[] { request.FactEventId }, causes,
                Math.Max(0, request.SaveRevision))).Fact;
        }

        private D10TavernRequestResult RequestSocial(D10TavernAction action, string actorId,
            string portId, string crewId, bool warningAcknowledged, int worldDay,
            long saveRevision, IEnumerable<string> causeEventIds)
        {
            var request = BaseRequest(action, actorId, portId, worldDay, saveRevision,
                causeEventIds, D10TavernSensitivity.Intimacy, warningAcknowledged,
                crewId, string.Empty, string.Empty);
            request.TargetId = crewId;
            return RequestAction(request);
        }

        private D10TavernRequest BaseRequest(D10TavernAction action, string actorId,
            string portId, int worldDay, long saveRevision, IEnumerable<string> causes,
            D10TavernSensitivity sensitivity, bool warningAcknowledged, string targetId,
            string relationTargetId, string rumorId)
        {
            return new D10TavernRequest
            {
                Action = action,
                ActorId = actorId,
                TargetId = targetId ?? string.Empty,
                RelationTargetId = relationTargetId ?? string.Empty,
                RumorId = rumorId ?? string.Empty,
                PortId = portId,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Sensitivity = sensitivity,
                WarningAcknowledged = warningAcknowledged,
                CauseEventIds = CopyIds(causes),
            };
        }

        private string ValidateRequest(D10TavernRequest request)
        {
            if (request == null) return "request_required";
            if (string.IsNullOrWhiteSpace(request.ActorId)) return "actor_required";
            if (string.IsNullOrWhiteSpace(request.PortId)) return "port_required";
            if (request.WorldDay < 0 || request.SaveRevision < 0) return "negative_time_or_revision";
            if (!Enum.IsDefined(typeof(D10TavernAction), request.Action)) return "unknown_action";
            if (!Enum.IsDefined(typeof(D10TavernSensitivity), request.Sensitivity))
                return "unknown_sensitivity";
            if (request.CauseEventIds != null)
                foreach (var cause in request.CauseEventIds)
                    if (string.IsNullOrWhiteSpace(cause)) return "invalid_cause_event";
            switch (request.Action)
            {
                case D10TavernAction.Recruit:
                    return string.IsNullOrWhiteSpace(request.TargetId) ? "candidate_required" : null;
                case D10TavernAction.Rest:
                    if (string.IsNullOrWhiteSpace(request.TargetId)) return "crew_required";
                    return request.RestHours < D10TavernConfiguration.RestMinHours
                        || request.RestHours > D10TavernConfiguration.RestMaxHours
                        ? "invalid_rest_hours" : null;
                case D10TavernAction.Meal:
                    return string.IsNullOrWhiteSpace(request.TargetId) ? "crew_required" : null;
                case D10TavernAction.DrinkAlcohol:
                    if (string.IsNullOrWhiteSpace(request.TargetId)) return "crew_required";
                    return Enum.IsDefined(typeof(D10AlcoholStrength), request.AlcoholStrength)
                        ? null : "invalid_alcohol_strength";
                case D10TavernAction.HearRumor:
                    return FindRumor(request.PortId, request.RumorId) == null ? "rumor_not_found" : null;
                case D10TavernAction.PokerEntry:
                    return PokerIntent(request.TargetId) == null ? "poker_table_not_found" : null;
                case D10TavernAction.Dance:
                case D10TavernAction.NightTalk:
                    return string.IsNullOrWhiteSpace(request.TargetId) ? "crew_required" : null;
                default:
                    return null;
            }
        }

        private string RequestSummary(D10TavernRequest request)
        {
            switch (request.Action)
            {
                case D10TavernAction.Recruit: return "酒馆已发起招募请求，等待 Recruitment owner 裁决。";
                case D10TavernAction.Rest: return "酒馆已发起休息请求，等待 CrewSystem 裁决。";
                case D10TavernAction.Meal: return "酒馆已发起用餐请求，等待 Economy/CrewSystem 裁决。";
                case D10TavernAction.DrinkAlcohol: return "酒馆已发起饮酒请求；健康、疲劳和冲突后果由 owner 裁决。";
                case D10TavernAction.HearRumor: return "传闻已记录，来源和可靠度随事实保存。";
                case D10TavernAction.PokerEntry: return "牌局入口请求已记录，牌面与结算交给 MiniGameSystem。";
                case D10TavernAction.Dance: return "舞会请求已记录，关系变化交给 Relation owner。";
                case D10TavernAction.NightTalk: return "夜话请求已记录，关系变化交给 Relation owner。";
                case D10TavernAction.SideJobBoard: return "副业公告板已打开，收入与完成交给 Job/Economy owner。";
                default: return "已进入酒馆社会场景。";
            }
        }

        private Dictionary<string, string> RequestValues(D10TavernRequest request)
        {
            var values = new Dictionary<string, string>
            {
                ["action"] = request.Action.ToString(),
                ["sensitivity"] = request.Sensitivity.ToString(),
                ["target_id"] = request.TargetId ?? string.Empty,
                ["rumor_id"] = request.RumorId ?? string.Empty,
                ["rest_hours"] = request.RestHours.ToString(CultureInfo.InvariantCulture),
            };
            if (request.Action == D10TavernAction.DrinkAlcohol)
            {
                var effect = D10TavernConfiguration.AlcoholEffect(request.AlcoholStrength);
                values["alcohol_profile"] = effect.ProfileId;
                values["health_delta"] = effect.HealthDelta.ToString(CultureInfo.InvariantCulture);
                values["fatigue_delta"] = effect.FatigueDelta.ToString(CultureInfo.InvariantCulture);
                values["conflict_risk"] = effect.ConflictRisk.ToString(CultureInfo.InvariantCulture);
            }
            return values;
        }

        private static D10TavernResolution RejectResolution(string code, string message,
            D10TavernRequest request)
        {
            return new D10TavernResolution
            {
                Accepted = false,
                Code = code ?? string.Empty,
                Message = message ?? string.Empty,
                Request = request == null ? null : request.Clone(),
            };
        }

        private List<D10TavernRumor> BuildRumors(string portId)
        {
            return new List<D10TavernRumor>
            {
                Rumor("rumor_chart_cove_tide", "低潮时海图湾北侧的礁缝会露出旧木桩。",
                    "npc:santa_margarita_dockhand", "码头装卸工", 0.72f,
                    new[] { "chart_cove", "tide", "exploration" }),
                Rumor("rumor_storm_beyond_reef", "外礁以南的雨线比昨日更早压下来。",
                    "npc:santa_margarita_lookout", "瞭望手", 0.56f,
                    new[] { "weather", "route" }),
                Rumor("rumor_missing_cargo", "有人说东侧仓栈少了一箱药材，但没有账本能证实。",
                    "npc:santa_margarita_tavern_keeper", "酒馆老板", 0.34f,
                    new[] { "cargo", "uncertain" }),
            };
        }

        private List<D10SideJobOffer> BuildSideJobs(string portId, int worldDay)
        {
            return new List<D10SideJobOffer>
            {
                new D10SideJobOffer
                {
                    OfferId = "sidejob_coastal_freight_01",
                    IssuerRef = "guild:santa_margarita_merchants",
                    JobType = "coastal_freight",
                    Summary = "把封好的小批货物送到邻近港口。",
                    PrerequisiteTags = new List<string> { "ship_space", "port_access" },
                    RiskTags = new List<string> { "delay", "weather" },
                    RewardTags = new List<string> { "income_quote", "merchant_reputation" },
                    ExpiresWorldDay = worldDay + 3,
                },
                new D10SideJobOffer
                {
                    OfferId = "sidejob_shore_guide_01",
                    IssuerRef = "community:saint_marys_chart_cove",
                    JobType = "shore_guide",
                    Summary = "为登陆队伍提供潮线和浅滩向导服务。",
                    PrerequisiteTags = new List<string> { "chart_knowledge", "crew_available" },
                    RiskTags = new List<string> { "misinformation", "surf_break" },
                    RewardTags = new List<string> { "income_quote", "community_trust" },
                    ExpiresWorldDay = worldDay + 2,
                },
                new D10SideJobOffer
                {
                    OfferId = "sidejob_dock_fight_01",
                    IssuerRef = "npc:santa_margarita_tavern_keeper",
                    JobType = "tavern_security",
                    Summary = "替酒馆处理一场可能升级的码头纠纷。",
                    PrerequisiteTags = new List<string> { "crew_available" },
                    RiskTags = new List<string> { "injury", "legal_attention", "conflict" },
                    RewardTags = new List<string> { "income_quote", "dock_contact" },
                    ExpiresWorldDay = worldDay + 1,
                    RequiresWarning = true,
                    Warning = "该副业可能包含暴力、伤病和法律后果，可跳过。",
                },
            };
        }

        private static D10TavernRumor Rumor(string id, string text, string sourceRef,
            string sourceKind, float reliability, IEnumerable<string> tags)
        {
            var band = reliability >= 0.67f ? D10RumorReliability.High
                : reliability >= 0.40f ? D10RumorReliability.Mixed : D10RumorReliability.Low;
            var result = new D10TavernRumor
            {
                RumorId = id,
                Text = text,
                SourceRef = sourceRef,
                SourceKind = sourceKind,
                Reliability = reliability,
                ReliabilityBand = band,
                ReliabilityLabel = band == D10RumorReliability.High ? "较可靠"
                    : band == D10RumorReliability.Mixed ? "半可信" : "低可信",
            };
            if (tags != null) result.Tags.AddRange(tags);
            return result;
        }

        private D10TavernRumor FindRumor(string portId, string rumorId)
        {
            foreach (var rumor in BuildRumors(portId ?? DefaultPortId))
                if (rumor.RumorId == rumorId) return rumor;
            return null;
        }

        private D10SideJobOffer FindSideJob(string portId, string offerId)
        {
            foreach (var offer in BuildSideJobs(portId ?? DefaultPortId, 0))
                if (offer.OfferId == offerId) return offer;
            return null;
        }

        private static D10PokerEntryIntent PokerIntent(string tableId)
        {
            if (tableId != "poker_table_low_stakes_01") return null;
            return new D10PokerEntryIntent
            {
                TableId = tableId,
                RulesetId = "poker_caribbean_draw_v1",
                StakeBand = "low",
                Message = "MiniGameSystem 接管牌局结算；本入口不产生钱、债务或胜负。",
            };
        }

        private static string WarningFor(D10TavernSensitivity sensitivity)
        {
            switch (sensitivity)
            {
                case D10TavernSensitivity.Alcohol:
                    return "饮酒会影响健康、疲劳和冲突风险；确认后继续，也可以跳过。";
                case D10TavernSensitivity.Gambling:
                    return "牌局可能产生金钱、债务和声誉风险；确认后只进入牌局系统。";
                case D10TavernSensitivity.Intimacy:
                    return "舞会/夜话是非露骨社交内容；可以确认、跳过或退出。";
                case D10TavernSensitivity.ExploitativeWork:
                    return "该副业包含敏感的暴力、剥削或法律风险；可以跳过。";
                default:
                    return string.Empty;
            }
        }

        private static string SensitivityLabel(D10TavernSensitivity sensitivity)
        {
            switch (sensitivity)
            {
                case D10TavernSensitivity.Alcohol: return "饮酒";
                case D10TavernSensitivity.Gambling: return "牌局";
                case D10TavernSensitivity.Intimacy: return "亲密社交";
                case D10TavernSensitivity.ExploitativeWork: return "敏感副业";
                default: return "敏感";
            }
        }

        private static string RequestMessage(string code)
        {
            switch (code)
            {
                case "actor_required": return "酒馆请求缺少发起者。";
                case "port_required": return "酒馆请求缺少港口。";
                case "unknown_sensitivity": return "酒馆敏感内容类型无效。";
                case "candidate_required": return "招募请求缺少候选人。";
                case "crew_required": return "该行为需要指定船员。";
                case "invalid_rest_hours": return "休息时长必须在酒馆允许范围内。";
                case "invalid_alcohol_strength": return "饮酒强度不在酒馆配置目录。";
                case "rumor_not_found": return "该传闻不在当前酒馆目录。";
                case "poker_table_not_found": return "该牌桌入口不存在。";
                default: return "酒馆请求不合法。";
            }
        }

        private static string TavernIdFor(string portId)
        {
            return portId == DefaultPortId ? DefaultTavernId : "tavern_" + SafeId(portId);
        }

        private static string RequestId(D10TavernRequest request)
        {
            return "tavern_request_" + request.Action + "_" + SafeId(request.ActorId) + "_" +
                SafeId(SubjectId(request)) + "_" + request.WorldDay + "_" + request.SaveRevision;
        }

        private static string RequestFactId(D10TavernRequest request)
        {
            return "event_d10_tavern_request_" + SafeId(request.Action.ToString()) + "_" +
                SafeId(request.ActorId) + "_" + SafeId(SubjectId(request)) + "_" +
                request.WorldDay + "_" + request.SaveRevision;
        }

        private static string ResolutionEventId(D10TavernRequest request)
        {
            return "event_d10_tavern_resolution_" + SafeId(request.Action.ToString()) + "_" +
                SafeId(request.ActorId) + "_" + SafeId(SubjectId(request)) + "_" +
                request.WorldDay + "_" + request.SaveRevision;
        }

        private static string SubjectId(D10TavernRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.TargetId)) return request.TargetId;
            if (request.Action == D10TavernAction.HearRumor && !string.IsNullOrWhiteSpace(request.RumorId))
                return request.RumorId;
            if (request.Action == D10TavernAction.SideJobBoard && !string.IsNullOrWhiteSpace(request.SideJobId))
                return request.SideJobId;
            return string.Empty;
        }

        private static List<string> Actors(D10TavernRequest request)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.ActorId)) result.Add(request.ActorId);
            if (!string.IsNullOrWhiteSpace(request.TargetId) && !result.Contains(request.TargetId))
                result.Add(request.TargetId);
            if (result.Count == 0) result.Add("scene:tavern");
            return result;
        }

        private static List<string> CopyIds(IEnumerable<string> values)
        {
            var result = new List<string>();
            if (values == null) return result;
            foreach (var value in values)
                if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value);
            return result;
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "none";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_';
            return new string(chars);
        }

        private static CrewAgent FindCrew(IList<CrewAgent> crew, string id)
        {
            if (crew == null || string.IsNullOrWhiteSpace(id)) return null;
            foreach (var member in crew)
                if (member != null && member.Id == id) return member;
            return null;
        }
    }
}
