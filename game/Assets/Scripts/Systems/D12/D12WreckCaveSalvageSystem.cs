// ============================================================
// D12WreckCaveSalvageSystem —— 沉船、洞穴和打捞唯一写者
//
// 本系统只裁决 D12 探索流程本身：位置是否已测绘、潮汐/装备是否满足、
// 风险与竞争者如何影响本次尝试，以及失败/恢复/奖励请求如何进入事实链。
// 法律、库存、船体、船员、奖励和 UI 不在这里写入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D12
{
    public static class D12SalvageRiskCalculator
    {
        /// <summary>使用稳定输入生成 0..99 的确定性风险骰；不依赖技术时间或 UI 输入。</summary>
        public static int Roll(string siteId, string attemptId, int seed)
        {
            unchecked
            {
                long hash = 1469598103934665603L;
                string input = (siteId ?? string.Empty) + "|" + (attemptId ?? string.Empty) + "|" + seed.ToString(CultureInfo.InvariantCulture);
                foreach (char character in input)
                {
                    hash ^= character;
                    hash *= 1099511628211L;
                }
                if (hash < 0) hash = -hash;
                return (int)(hash % 100L);
            }
        }
    }

    public sealed class D12WreckCaveSalvageSystem
    {
        private readonly List<D12ExplorationSiteDefinition> _orderedDefinitions =
            new List<D12ExplorationSiteDefinition>();
        private readonly Dictionary<string, D12ExplorationSiteDefinition> _definitions =
            new Dictionary<string, D12ExplorationSiteDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D12SalvageSiteState> _states =
            new Dictionary<string, D12SalvageSiteState>(StringComparer.Ordinal);
        private readonly D12MapDiscoverySystem _mapDiscovery;
        private readonly EventLog _eventLog;
        private readonly ID12SalvageLegalAuthority _legalAuthority;
        private int _lastWorldDay;
        private long _revision;
        private string _lastEventId;

        public D12WreckCaveSalvageSystem(IEnumerable<D12ExplorationSiteDefinition> definitions,
            D12MapDiscoverySystem mapDiscovery, EventLog eventLog,
            ID12SalvageLegalAuthority legalAuthority, D12SalvageSnapshot savedSnapshot = null)
        {
            if (definitions == null) throw new ArgumentNullException("definitions");
            _mapDiscovery = mapDiscovery;
            _eventLog = eventLog ?? throw new ArgumentNullException("eventLog");
            _legalAuthority = legalAuthority;

            foreach (var source in definitions)
            {
                if (source == null) throw new InvalidOperationException("沉船/洞穴目录不能包含 null。");
                var definition = source.Clone();
                definition.Validate();
                if (!_definitions.ContainsKey(definition.SiteId))
                {
                    _definitions.Add(definition.SiteId, definition);
                    _orderedDefinitions.Add(definition);
                }
                else throw new InvalidOperationException("沉船/洞穴目录重复：" + definition.SiteId);
            }

            if (savedSnapshot != null)
            {
                savedSnapshot.Validate();
                foreach (var saved in savedSnapshot.Sites)
                {
                    if (!_definitions.ContainsKey(saved.SiteId))
                        throw new InvalidOperationException("存档中的 D12 打捞地点不在当前目录：" + saved.SiteId);
                    _states[saved.SiteId] = saved.Clone();
                }
                _lastWorldDay = savedSnapshot.LastWorldDay;
                _revision = savedSnapshot.Revision;
                _lastEventId = savedSnapshot.LastEventId;
            }

            foreach (var definition in _orderedDefinitions)
                if (!_states.ContainsKey(definition.SiteId))
                    _states.Add(definition.SiteId, new D12SalvageSiteState { SiteId = definition.SiteId });
        }

        public IReadOnlyList<D12ExplorationSiteDefinition> Definitions
        {
            get
            {
                var result = new List<D12ExplorationSiteDefinition>();
                foreach (var definition in _orderedDefinitions) result.Add(definition.Clone());
                return result;
            }
        }

        public D12SalvageSnapshot Snapshot
        {
            get
            {
                var result = new D12SalvageSnapshot
                {
                    LastWorldDay = _lastWorldDay,
                    Revision = _revision,
                    LastEventId = _lastEventId,
                };
                foreach (var definition in _orderedDefinitions) result.Sites.Add(_states[definition.SiteId].Clone());
                return result;
            }
        }

        public EventLog EventLog { get { return _eventLog; } }

        /// <summary>UI 只读取已在 D12-A01 海图中出现的地点与 D12 打捞投影。</summary>
        public D12SalvageReadModel GetReadModel()
        {
            var result = new D12SalvageReadModel();
            var visible = new HashSet<string>(StringComparer.Ordinal);
            if (_mapDiscovery != null)
                foreach (var feature in _mapDiscovery.GetMapReadModel().VisibleFeatures)
                    if (feature != null) visible.Add(feature.FeatureId);

            foreach (var definition in _orderedDefinitions)
            {
                if (!visible.Contains(definition.MapFeatureId)) continue;
                var state = _states[definition.SiteId];
                result.VisibleSites.Add(new D12SalvageSiteView
                {
                    SiteId = definition.SiteId,
                    Type = definition.Type,
                    DisplayName = definition.DisplayName,
                    Status = state.Status,
                    AttemptCount = state.AttemptCount,
                    LastFailureCode = state.LastFailureCode,
                    LastRecoveryRouteId = state.LastRecoveryRouteId,
                    LossCount = state.LossRecords == null ? 0 : state.LossRecords.Count,
                    RewardRequested = state.RewardRequest != null,
                });
            }
            return result;
        }

        /// <summary>开始一次沉船/洞穴尝试；成功只生成 RewardRequest，不生成物品。</summary>
        public D12SalvageResult TrySalvage(D12SalvageRequest request)
        {
            if (request == null) return Reject(null, D12SalvageFailureCode.None, "request_required", "打捞请求不能为空。");
            D12ExplorationSiteDefinition definition;
            if (!_definitions.TryGetValue(request.SiteId ?? string.Empty, out definition))
                return Reject(request.SiteId, D12SalvageFailureCode.None, "site_unknown", "沉船或洞穴不在 D12 目录中。");
            if (string.IsNullOrWhiteSpace(request.ActorRef) || string.IsNullOrWhiteSpace(request.AttemptId))
                return Reject(definition.SiteId, D12SalvageFailureCode.None, "actor_or_attempt_required", "打捞必须有执行者和稳定 attempt_id。");
            if (!IsCharted(definition.MapFeatureId))
                return Reject(definition.SiteId, D12SalvageFailureCode.SiteNotCharted, "site_not_charted", "尚未在海图中确认该沉船/洞穴位置。");

            var state = _states[definition.SiteId];
            if (state.Status == D12SalvageStatus.Salvaged)
                return Reject(definition.SiteId, D12SalvageFailureCode.AlreadySalvaged, "already_salvaged", "该地点的打捞结果已经记录，不能重复生成宝物请求。", state);
            if (state.Status == D12SalvageStatus.Failed)
                return Reject(definition.SiteId, D12SalvageFailureCode.RecoveryRequired, "recovery_required", "上次打捞失败，必须先执行恢复路线。", state);

            var legal = RuleLegal(definition, request);
            if (legal == null || !legal.RulingAvailable)
                return RecordFailure(definition, state, request, D12SalvageFailureCode.LegalRulingRequired,
                    "legal_ruling_required", "未收到法律/势力系统的沉船归属裁决；D12 不自行推断产权。", null, null, 0, 0, legal);
            if (!legal.SalvageAllowed)
                return RecordFailure(definition, state, request, D12SalvageFailureCode.LegalDenied,
                    "legal_denied", "法律/势力系统拒绝当前打捞；可走法律复核或回港路线。", FindRoute(definition, "legal_review"), null, 0, 0, legal);

            var equipment = new HashSet<string>(request.EquipmentIds ?? new List<string>(), StringComparer.Ordinal);
            var missing = MissingEquipment(definition, equipment);
            if (missing.Count > 0)
                return RecordFailure(definition, state, request, D12SalvageFailureCode.EquipmentRequired,
                    "equipment_required", "打捞装备不足：" + string.Join(",", missing.ToArray()) + "。", FindRoute(definition, "prepare_equipment"), null, 0, 0, legal);
            if (!request.TideWindowOpen || !TideMatches(definition.PreferredTideStage, request.TideStage))
                return RecordFailure(definition, state, request, D12SalvageFailureCode.TideWindowClosed,
                    "tide_window_closed", "当前潮汐不在可解释的打捞窗口；失败事实保留，可等待下一窗口。", FindRoute(definition, "wait_for_tide"), null, 0, 0, legal);
            if (request.RopeMeters < definition.MinimumRopeMeters)
                return RecordFailure(definition, state, request, D12SalvageFailureCode.RopeTooShort,
                    "rope_too_short", "绳索长度不足，不能安全到达目标深度/回头路线。", FindRoute(definition, "prepare_equipment"), null, 0, 0, legal);
            if (request.LightHoursAvailable < definition.RequiredLightHours)
                return RecordFailure(definition, state, request, D12SalvageFailureCode.LightInsufficient,
                    "light_insufficient", "照明余量不足，洞穴回头路线不可安全复盘。", FindRoute(definition, "prepare_equipment"), null, 0, 0, legal);

            int riskThreshold = CalculateRiskThreshold(definition, request, equipment);
            int riskRoll = D12SalvageRiskCalculator.Roll(definition.SiteId, request.AttemptId, request.RiskSeed);
            if (riskRoll < riskThreshold)
            {
                D12SalvageFailureCode failureCode;
                D12SalvageLossKind lossKind;
                int severity;
                GetRiskFailure(definition, request, riskRoll, riskThreshold, out failureCode, out lossKind, out severity);
                var route = definition.Type == D12ExplorationSiteType.Cave
                    ? FindRoute(definition, "return_to_entrance")
                    : FindRoute(definition, "return_to_harbor");
                if (route == null) route = FindRoute(definition, "return_to_known_place");
                return RecordFailure(definition, state, request, failureCode,
                    failureCode == D12SalvageFailureCode.CompetitorInterference ? "competitor_interference" : "salvage_risk_failed",
                    definition.Type == D12ExplorationSiteType.Cave
                        ? "洞穴内的塌方/落水风险被触发；回头路线和损失记录已写入事实链。"
                        : "沉船打捞遭遇绳索/进水风险；残骸未被重复结算，可回港恢复。",
                    route, new D12SalvageLossRecord
                    {
                        Kind = lossKind,
                        Severity = severity,
                        Summary = failureCode == D12SalvageFailureCode.CompetitorInterference
                            ? "竞争者介入导致本次打捞中断。"
                            : (definition.Type == D12ExplorationSiteType.Cave ? "洞穴塌方或落水风险造成路线中断。" : "沉船打捞绳索/进水风险造成路线中断。"),
                        RecoveryRouteId = route == null ? string.Empty : route.RouteId,
                    }, riskRoll, riskThreshold, legal);
            }

            return RecordSuccess(definition, state, request, equipment, legal, riskRoll, riskThreshold);
        }

        /// <summary>失败恢复追加 recovered 事实，不覆盖原失败，也不发放奖励。</summary>
        public D12SalvageResult Recover(D12SalvageRecoveryRequest request)
        {
            if (request == null) return Reject(null, D12SalvageFailureCode.RecoveryUnavailable, "request_required", "恢复请求不能为空。");
            D12ExplorationSiteDefinition definition;
            if (!_definitions.TryGetValue(request.SiteId ?? string.Empty, out definition))
                return Reject(request.SiteId, D12SalvageFailureCode.RecoveryUnavailable, "site_unknown", "恢复目标不在 D12 目录中。");
            if (string.IsNullOrWhiteSpace(request.ActorRef) || string.IsNullOrWhiteSpace(request.RecoveryAttemptId))
                return Reject(definition.SiteId, D12SalvageFailureCode.RecoveryUnavailable, "actor_or_recovery_id_required", "恢复必须有执行者和稳定 recovery_attempt_id。");
            var state = _states[definition.SiteId];
            if (state.Status != D12SalvageStatus.Failed)
                return Reject(definition.SiteId, D12SalvageFailureCode.RecoveryUnavailable, "recovery_unavailable", "当前没有等待恢复的失败事实。", state);
            var route = FindRoute(definition, request.RouteId);
            if (route == null)
                return Reject(definition.SiteId, D12SalvageFailureCode.RecoveryUnavailable, "recovery_route_unknown", "恢复路线不存在。", state);
            if (route.RequiredTideStage != D12TideStage.Unknown &&
                (!request.TideWindowOpen || !TideMatches(route.RequiredTideStage, request.TideStage)))
                return Reject(definition.SiteId, D12SalvageFailureCode.TideWindowClosed, "recovery_tide_closed", "恢复路线仍需等待指定潮汐窗口。", state, route);
            if (!string.IsNullOrWhiteSpace(route.RequiredEquipmentId) &&
                !(request.EquipmentIds ?? new List<string>()).Contains(route.RequiredEquipmentId))
                return Reject(definition.SiteId, D12SalvageFailureCode.EquipmentRequired, "recovery_equipment_required", "恢复路线缺少装备：" + route.RequiredEquipmentId + "。", state, route);

            D12SalvageLegalDecision legal = null;
            if (route.Action == "legal_review")
            {
                legal = _legalAuthority == null ? null : _legalAuthority.Rule(new D12SalvageLegalRequest
                {
                    SiteId = definition.SiteId,
                    ActorRef = request.ActorRef,
                    ClaimRef = definition.ClaimRef,
                    EvidenceRef = "recovery:" + request.RecoveryAttemptId,
                    WorldDay = request.WorldDay,
                    SaveRevision = request.SaveRevision,
                });
                if (legal == null || !legal.RulingAvailable || !legal.SalvageAllowed)
                    return Reject(definition.SiteId, D12SalvageFailureCode.LegalDenied, "legal_review_pending", "法律复核尚未允许继续打捞。", state, route);
            }

            string eventId = D12SalvageIds.EventPrefix + "recovered_" + SafeId(request.RecoveryAttemptId);
            var fact = _eventLog.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                LocationRef = string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef,
                OwnerSystem = D12MapDiscoveryIds.SystemOwner,
                ActorIds = new List<string> { request.ActorRef },
                InputFacts = new List<string>
                {
                    "site:" + definition.SiteId,
                    "recovery_route:" + route.RouteId,
                    "failed_event:" + (state.LastFailureEventRef ?? string.Empty),
                },
                CauseIds = new List<string> { EventIdPart(state.LastFailureEventRef) },
                SaveRevision = Math.Max(0L, request.SaveRevision),
                OutcomeStatus = WorldEventStatus.Recovered,
                Result = new EventResult
                {
                    ResultCode = "salvage_recovered",
                    Summary = route.Description,
                    FactIds = new List<string> { "salvage:" + definition.SiteId, "recovery:" + route.RouteId },
                    Values = new Dictionary<string, string>
                    {
                        ["site_id"] = definition.SiteId,
                        ["route_id"] = route.RouteId,
                        ["resume_from_event_ref"] = state.LastFailureEventRef ?? string.Empty,
                        ["legal_ruling_ref"] = legal == null ? string.Empty : legal.RulingRef,
                    },
                },
            }).Fact;

            string factRef = FactRef(fact);
            if (!state.FactEventRefs.Contains(factRef)) state.FactEventRefs.Add(factRef);
            state.Status = D12SalvageStatus.Recovered;
            state.LastFailureCode = D12SalvageFailureCode.None.ToString();
            state.LastRecoveryRouteId = route.RouteId;
            state.LastRecoveryEventRef = factRef;
            _lastWorldDay = Math.Max(_lastWorldDay, Math.Max(0, request.WorldDay));
            _revision = Math.Max(_revision + 1, request.SaveRevision);
            _lastEventId = eventId;
            return new D12SalvageResult
            {
                Accepted = true,
                Recovered = true,
                Code = "salvage_recovered",
                Message = route.Description,
                SiteId = definition.SiteId,
                Status = state.Status,
                RecoveryRoute = route.Clone(),
                Fact = fact,
                LegalDecision = legal == null ? null : legal.Clone(),
            };
        }

        public static int CalculateRiskThreshold(D12ExplorationSiteDefinition definition,
            D12SalvageRequest request, ISet<string> equipment)
        {
            int threshold = definition.BaseRiskPercent;
            if (definition.Type == D12ExplorationSiteType.Cave && !equipment.Contains(D12SalvageIds.CaveMarkerToolId))
                threshold += definition.CollapseRiskPercent;
            if (definition.Type == D12ExplorationSiteType.Wreck && !equipment.Contains(D12SalvageIds.PumpToolId))
                threshold += definition.WaterRiskPercent;
            if (request.Competitor != null && request.Competitor.Present)
            {
                threshold += definition.CompetitorRiskPercent;
                threshold += Math.Max(0, Math.Min(100, request.Competitor.PressurePercent)) / 4;
            }
            if (request.TideStage != definition.PreferredTideStage) threshold += 5;
            if (definition.Type == D12ExplorationSiteType.Cave && request.LightHoursAvailable == definition.RequiredLightHours)
                threshold += 5;
            if (definition.Type == D12ExplorationSiteType.Wreck && equipment.Contains(D12SalvageIds.PumpToolId)) threshold -= 8;
            if (definition.Type == D12ExplorationSiteType.Cave && equipment.Contains(D12SalvageIds.CaveMarkerToolId)) threshold -= 8;
            return Math.Max(0, Math.Min(99, threshold));
        }

        private D12SalvageResult RecordSuccess(D12ExplorationSiteDefinition definition,
            D12SalvageSiteState state, D12SalvageRequest request, ISet<string> equipment,
            D12SalvageLegalDecision legal, int riskRoll, int riskThreshold)
        {
            string eventId = D12SalvageIds.EventPrefix + "attempt_" + SafeId(request.AttemptId);
            var factResult = _eventLog.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                LocationRef = string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef,
                OwnerSystem = D12MapDiscoveryIds.SystemOwner,
                ActorIds = new List<string> { request.ActorRef },
                InputFacts = BuildInputFacts(definition, request, equipment, legal),
                CauseIds = new List<string> { "map:" + definition.MapFeatureId },
                SaveRevision = Math.Max(0L, request.SaveRevision),
                OutcomeStatus = WorldEventStatus.Normal,
                Result = new EventResult
                {
                    ResultCode = "salvage_success",
                    Summary = definition.Type == D12ExplorationSiteType.Cave
                        ? "洞穴路线完成；宝藏只生成可追溯的 RewardRequest。"
                        : "沉船打捞完成；宝藏只生成可追溯的 RewardRequest。",
                    FactIds = new List<string> { "salvage:" + definition.SiteId, "reward_request:" + definition.RewardRequestId },
                    Values = BuildValues(definition, request, legal, riskRoll, riskThreshold, D12SalvageFailureCode.None, null),
                },
            });
            if (factResult.IsDuplicate)
                return Duplicate(definition, state, factResult.Fact, legal);

            var fact = factResult.Fact;
            string factRef = FactRef(fact);
            state.Status = D12SalvageStatus.Salvaged;
            state.AttemptCount++;
            state.LastFailureCode = D12SalvageFailureCode.None.ToString();
            state.LastRecoveryRouteId = null;
            state.LastResolutionEventRef = factRef;
            state.LastCompetitorRef = request.Competitor == null ? null : request.Competitor.CompetitorRef;
            state.LastCompetitorPressurePercent = request.Competitor == null ? 0 : ClampPercent(request.Competitor.PressurePercent);
            if (!state.FactEventRefs.Contains(factRef)) state.FactEventRefs.Add(factRef);
            state.RewardRequest = new D12SalvageRewardRequest
            {
                RequestId = definition.RewardRequestId + "@" + fact.SaveRevision.ToString(CultureInfo.InvariantCulture),
                RewardId = definition.RewardRequestId,
                SiteId = definition.SiteId,
                SourceEventRef = factRef,
            };
            Advance(request.WorldDay, request.SaveRevision, eventId);
            return new D12SalvageResult
            {
                Accepted = true,
                Succeeded = true,
                Code = "salvage_success",
                Message = "打捞完成；RewardRequest 已提交，实际奖励由 RewardSystem 裁决。",
                SiteId = definition.SiteId,
                Status = state.Status,
                RiskRoll = riskRoll,
                RiskThreshold = riskThreshold,
                RewardRequest = state.RewardRequest.Clone(),
                LegalDecision = legal == null ? null : legal.Clone(),
                Fact = fact,
            };
        }

        private D12SalvageResult RecordFailure(D12ExplorationSiteDefinition definition,
            D12SalvageSiteState state, D12SalvageRequest request, D12SalvageFailureCode failureCode,
            string resultCode, string summary, D12SalvageRecoveryOption route,
            D12SalvageLossRecord loss, int riskRoll, int riskThreshold, D12SalvageLegalDecision legal)
        {
            if (route == null) route = _orderedDefinitions.Count == 0 ? null : definition.RecoveryOptions[0].Clone();
            if (route == null) throw new InvalidOperationException("打捞失败必须有恢复路线：" + definition.SiteId);
            string eventId = D12SalvageIds.EventPrefix + "attempt_" + SafeId(request.AttemptId);
            var values = BuildValues(definition, request, legal, riskRoll, riskThreshold, failureCode, route);
            var factResult = _eventLog.Append(new WorldEvent
            {
                EventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                LocationRef = string.IsNullOrWhiteSpace(request.LocationRef) ? definition.LocationRef : request.LocationRef,
                OwnerSystem = D12MapDiscoveryIds.SystemOwner,
                ActorIds = new List<string> { request.ActorRef },
                InputFacts = BuildInputFacts(definition, request, new HashSet<string>(request.EquipmentIds ?? new List<string>(), StringComparer.Ordinal), legal),
                CauseIds = new List<string> { "map:" + definition.MapFeatureId },
                SaveRevision = Math.Max(0L, request.SaveRevision),
                OutcomeStatus = WorldEventStatus.Failed,
                RecoveryRoute = new RecoveryRoute
                {
                    RouteId = route.RouteId,
                    Action = route.Action,
                    ResumeFromEventId = eventId,
                    Description = route.Description,
                },
                Result = new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string> { "salvage:" + definition.SiteId, "failure:" + failureCode.ToString() },
                    Values = values,
                },
            });
            if (factResult.IsDuplicate)
                return Duplicate(definition, state, factResult.Fact, legal);

            var fact = factResult.Fact;
            string factRef = FactRef(fact);
            state.Status = D12SalvageStatus.Failed;
            state.AttemptCount++;
            state.LastFailureCode = failureCode.ToString();
            state.LastRecoveryRouteId = route.RouteId;
            state.LastFailureEventRef = factRef;
            state.LastResolutionEventRef = factRef;
            state.LastCompetitorRef = request.Competitor == null ? null : request.Competitor.CompetitorRef;
            state.LastCompetitorPressurePercent = request.Competitor == null ? 0 : ClampPercent(request.Competitor.PressurePercent);
            if (!state.FactEventRefs.Contains(factRef)) state.FactEventRefs.Add(factRef);
            if (loss != null)
            {
                loss.LossId = "loss:" + definition.SiteId + ":" + SafeId(request.AttemptId);
                loss.EventId = fact.EventId;
                loss.WorldDay = Math.Max(0, request.WorldDay);
                if (!ContainsLoss(state.LossRecords, loss.LossId)) state.LossRecords.Add(loss);
            }
            Advance(request.WorldDay, request.SaveRevision, eventId);
            return new D12SalvageResult
            {
                Accepted = true,
                Succeeded = false,
                Code = resultCode,
                Message = summary,
                SiteId = definition.SiteId,
                Status = state.Status,
                FailureCode = failureCode,
                RiskRoll = riskRoll,
                RiskThreshold = riskThreshold,
                RecoveryRoute = route.Clone(),
                Loss = loss == null ? null : loss.Clone(),
                LegalDecision = legal == null ? null : legal.Clone(),
                Fact = fact,
            };
        }

        private D12SalvageLegalDecision RuleLegal(D12ExplorationSiteDefinition definition, D12SalvageRequest request)
        {
            if (_legalAuthority == null) return null;
            return _legalAuthority.Rule(new D12SalvageLegalRequest
            {
                SiteId = definition.SiteId,
                ActorRef = request.ActorRef,
                ClaimRef = definition.ClaimRef,
                CompetitorClaimRef = request.Competitor == null ? null : request.Competitor.ClaimRef,
                EvidenceRef = request.Competitor == null ? "salvage:" + request.AttemptId : request.Competitor.EvidenceRef,
                WorldDay = request.WorldDay,
                SaveRevision = request.SaveRevision,
            });
        }

        private bool IsCharted(string mapFeatureId)
        {
            if (_mapDiscovery == null) return false;
            foreach (var feature in _mapDiscovery.GetMapReadModel().VisibleFeatures)
                if (feature != null && feature.FeatureId == mapFeatureId) return true;
            return false;
        }

        private static List<string> MissingEquipment(D12ExplorationSiteDefinition definition, ISet<string> equipment)
        {
            var result = new List<string>();
            foreach (var required in definition.RequiredEquipmentIds)
                if (!equipment.Contains(required)) result.Add(required);
            return result;
        }

        private static bool TideMatches(D12TideStage required, D12TideStage actual)
        {
            if (required == D12TideStage.Low) return actual == D12TideStage.Low;
            if (required == D12TideStage.Falling) return actual == D12TideStage.Falling || actual == D12TideStage.Low;
            return actual != D12TideStage.Unknown;
        }

        private static void GetRiskFailure(D12ExplorationSiteDefinition definition, D12SalvageRequest request,
            int riskRoll, int riskThreshold, out D12SalvageFailureCode failureCode,
            out D12SalvageLossKind lossKind, out int severity)
        {
            int competitor = request.Competitor == null || !request.Competitor.Present
                ? 0 : definition.CompetitorRiskPercent + ClampPercent(request.Competitor.PressurePercent) / 4;
            if (competitor > 0 && riskRoll < Math.Min(riskThreshold, competitor + definition.BaseRiskPercent))
            {
                failureCode = D12SalvageFailureCode.CompetitorInterference;
                lossKind = D12SalvageLossKind.CompetitorDispute;
                severity = Math.Min(100, 45 + ClampPercent(request.Competitor.PressurePercent) / 2);
                return;
            }
            if (definition.Type == D12ExplorationSiteType.Cave)
            {
                int collapseBoundary = Math.Min(99, definition.BaseRiskPercent + definition.CollapseRiskPercent);
                if (riskRoll < collapseBoundary)
                {
                    failureCode = D12SalvageFailureCode.CaveCollapse;
                    lossKind = D12SalvageLossKind.CaveCollapse;
                    severity = 70;
                    return;
                }
                failureCode = D12SalvageFailureCode.FallOrWaterInrush;
                lossKind = D12SalvageLossKind.FallRisk;
                severity = 55;
                return;
            }
            failureCode = D12SalvageFailureCode.RiskEncountered;
            lossKind = riskRoll % 2 == 0 ? D12SalvageLossKind.LineSlipped : D12SalvageLossKind.WaterInrush;
            severity = 50;
        }

        private static Dictionary<string, string> BuildValues(D12ExplorationSiteDefinition definition,
            D12SalvageRequest request, D12SalvageLegalDecision legal, int riskRoll, int riskThreshold,
            D12SalvageFailureCode failureCode, D12SalvageRecoveryOption route)
        {
            var values = new Dictionary<string, string>
            {
                ["site_id"] = definition.SiteId,
                ["site_type"] = definition.Type.ToString(),
                ["map_feature_id"] = definition.MapFeatureId,
                ["preferred_tide_stage"] = definition.PreferredTideStage.ToString(),
                ["actual_tide_stage"] = request.TideStage.ToString(),
                ["tide_window_open"] = request.TideWindowOpen.ToString().ToLowerInvariant(),
                ["rope_meters"] = request.RopeMeters.ToString(CultureInfo.InvariantCulture),
                ["light_hours"] = request.LightHoursAvailable.ToString(CultureInfo.InvariantCulture),
                ["risk_roll"] = riskRoll.ToString(CultureInfo.InvariantCulture),
                ["risk_threshold"] = riskThreshold.ToString(CultureInfo.InvariantCulture),
                ["failure_code"] = failureCode.ToString(),
                ["recovery_route_id"] = route == null ? string.Empty : route.RouteId,
                ["competitor_present"] = (request.Competitor != null && request.Competitor.Present).ToString().ToLowerInvariant(),
                ["competitor_ref"] = request.Competitor == null ? string.Empty : request.Competitor.CompetitorRef ?? string.Empty,
                ["competitor_pressure_percent"] = request.Competitor == null ? "0" : ClampPercent(request.Competitor.PressurePercent).ToString(CultureInfo.InvariantCulture),
                ["legal_ruling_ref"] = legal == null ? string.Empty : legal.RulingRef ?? string.Empty,
                ["legal_claim_status"] = legal == null ? "unavailable" : legal.ClaimStatus ?? string.Empty,
                ["legal_claimant_ref"] = legal == null ? string.Empty : legal.ClaimantRef ?? string.Empty,
                ["controlling_faction_id"] = legal == null ? string.Empty : legal.ControllingFactionId ?? string.Empty,
            };
            return values;
        }

        private static List<string> BuildInputFacts(D12ExplorationSiteDefinition definition,
            D12SalvageRequest request, ISet<string> equipment, D12SalvageLegalDecision legal)
        {
            var result = new List<string>
            {
                "site:" + definition.SiteId,
                "map_feature:" + definition.MapFeatureId,
                "tide:" + request.TideStage,
                "legal_ruling:" + (legal == null ? "unavailable" : legal.RulingRef),
            };
            foreach (var item in equipment) result.Add("equipment:" + item);
            if (request.Competitor != null && request.Competitor.Present)
                result.Add("competitor:" + (request.Competitor.CompetitorRef ?? "unknown"));
            return result;
        }

        private D12SalvageResult Duplicate(D12ExplorationSiteDefinition definition,
            D12SalvageSiteState state, FactEvent fact, D12SalvageLegalDecision legal)
        {
            return new D12SalvageResult
            {
                Accepted = true,
                Duplicate = true,
                Succeeded = state.Status == D12SalvageStatus.Salvaged,
                Code = "duplicate_salvage_attempt",
                Message = "该打捞 attempt_id 已有事实记录，未重复生成宝物或损失。",
                SiteId = definition.SiteId,
                Status = state.Status,
                RewardRequest = state.RewardRequest == null ? null : state.RewardRequest.Clone(),
                LegalDecision = legal == null ? null : legal.Clone(),
                Fact = fact,
            };
        }

        private static D12SalvageResult Reject(string siteId, D12SalvageFailureCode code,
            string resultCode, string message, D12SalvageSiteState state = null,
            D12SalvageRecoveryOption route = null)
        {
            return new D12SalvageResult
            {
                Accepted = false,
                Code = resultCode,
                Message = message,
                SiteId = siteId,
                Status = state == null ? D12SalvageStatus.Available : state.Status,
                FailureCode = code,
                RecoveryRoute = route == null ? null : route.Clone(),
                RewardRequest = state == null || state.RewardRequest == null ? null : state.RewardRequest.Clone(),
            };
        }

        private D12SalvageRecoveryOption FindRoute(D12ExplorationSiteDefinition definition, string actionOrId)
        {
            if (definition.RecoveryOptions == null) return null;
            foreach (var route in definition.RecoveryOptions)
                if (route != null && (route.RouteId == actionOrId || route.Action == actionOrId)) return route.Clone();
            return null;
        }

        private void Advance(int worldDay, long saveRevision, string eventId)
        {
            _lastWorldDay = Math.Max(_lastWorldDay, Math.Max(0, worldDay));
            _revision = Math.Max(_revision + 1, saveRevision);
            _lastEventId = eventId;
        }

        private static bool ContainsLoss(List<D12SalvageLossRecord> records, string lossId)
        {
            if (records == null) return false;
            foreach (var record in records)
                if (record != null && record.LossId == lossId) return true;
            return false;
        }

        private static int ClampPercent(int value) { return Math.Max(0, Math.Min(100, value)); }

        private static string FactRef(FactEvent fact)
        {
            return fact.EventId + "@" + fact.SaveRevision.ToString(CultureInfo.InvariantCulture);
        }

        private static string EventIdPart(string factRef)
        {
            if (string.IsNullOrWhiteSpace(factRef)) return string.Empty;
            int index = factRef.LastIndexOf('@');
            return index <= 0 ? factRef : factRef.Substring(0, index);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-') chars[i] = '_';
            return new string(chars);
        }
    }
}
