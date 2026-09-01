// ============================================================
// D21VoyageResolutionSystem —— D21-L25 固定 Tick 航程结算
//
// 航程 owner 只接受显式 D21TickContext。每一步先计算只读天气/海况、船况、
// 船员、风险和时间结果，再提交补给 owner，最后以事件路由器提交事实与
// Save projection。系统不读取帧时间、系统时钟或不可控随机，也不直接写入
// D19/D20/D3/D9 的事实对象。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>固定 Tick 结算请求；天气为空时沿用启动时的已校验输入。</summary>
    [Serializable]
    public sealed class D21VoyageTickRequest
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public D21TickContext TickContext;
        public D21WeatherInput WeatherInput;

        /// <summary>返回防御性请求副本。</summary>
        public D21VoyageTickRequest Clone()
        {
            return new D21VoyageTickRequest
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                TickContext = TickContext == null ? null : TickContext.Clone(),
                WeatherInput = WeatherInput == null ? null : WeatherInput.Clone(),
            };
        }

        /// <summary>生成固定字符串指纹，避免使用运行时 HashCode。</summary>
        public string Fingerprint()
        {
            return string.Join("|", new[]
            {
                SchemaVersion.ToString(CultureInfo.InvariantCulture),
                CommandId ?? string.Empty,
                VoyageId ?? string.Empty,
                ShipId ?? string.Empty,
                TickContext == null ? string.Empty : TickContext.TickIndex.ToString(CultureInfo.InvariantCulture),
                TickContext == null ? string.Empty : TickContext.WorldDay.ToString(CultureInfo.InvariantCulture),
                TickContext == null ? string.Empty : TickContext.MinuteOfDay.ToString(CultureInfo.InvariantCulture),
                TickContext == null ? string.Empty : TickContext.SaveRevision.ToString(CultureInfo.InvariantCulture),
                TickContext == null ? string.Empty : TickContext.VoyageSeed.ToString(CultureInfo.InvariantCulture),
                WeatherInput == null ? string.Empty : WeatherInput.Fingerprint(),
            });
        }

        /// <summary>校验请求的 envelope；时间与 owner 状态匹配在解析阶段完成。</summary>
        public bool TryValidate(out string code)
        {
            code = string.Empty;
            if (SchemaVersion != CurrentSchemaVersion)
            {
                code = "d21.voyage.tick_schema_invalid";
                return false;
            }
            if (!D21ShipIdentityContracts.IsStableId(CommandId, 128) ||
                !D21ShipIdentityContracts.IsStableId(VoyageId, 128) ||
                !D21ShipIdentityContracts.IsStableId(ShipId, 128))
            {
                code = "d21.voyage.tick_reference_invalid";
                return false;
            }
            if (TickContext == null)
            {
                code = "d21.voyage.tick_context_required";
                return false;
            }
            try
            {
                TickContext.Validate();
            }
            catch (InvalidOperationException)
            {
                code = "d21.voyage.tick_context_invalid";
                return false;
            }
            return true;
        }
    }

    /// <summary>D21 航程 owner 的完整可保存逻辑快照。</summary>
    [Serializable]
    public sealed class D21VoyageResolutionState
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public string OriginId = string.Empty;
        public string DestinationId = string.Empty;
        public string CurrentWaypointId = string.Empty;
        public D21VoyageStatus Status = D21VoyageStatus.Idle;
        public long SaveRevision;
        public long VoyageSeed;
        public int BaseSpeedMilliKnots;
        public bool PortBoundaryAccepted;
        public string PortBoundaryReference = string.Empty;
        public D21VoyageTimeState Time;
        public D21CourseState Course;
        public D21ShipIdentity ShipIdentity;
        public D21HullComponentConfiguration HullConfiguration;
        public D21ShipConditionState ConditionState;
        public D21CrewRequirementDecision CrewDecision;
        public D21AnchorMooringState AnchorState;
        public D21NavigationRouteDefinition RouteDefinition;
        public D21RouteDiscoveryRecord DiscoveryRecord;
        public D21WeatherInput WeatherInput;
        public D21WeatherImpactResult WeatherImpact;
        public D21ProvisionConsumptionRequest ProvisionRequest;
        public List<D21VoyageRiskSeed> RiskSeeds = new List<D21VoyageRiskSeed>();
        public D21ShipSaveProjection SaveProjection;
        public string LastEventId = string.Empty;
        public int LastTickIndex;

        /// <summary>返回隔离 owner 快照的深复制。</summary>
        public D21VoyageResolutionState Clone()
        {
            var result = new D21VoyageResolutionState
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                OriginId = OriginId ?? string.Empty,
                DestinationId = DestinationId ?? string.Empty,
                CurrentWaypointId = CurrentWaypointId ?? string.Empty,
                Status = Status,
                SaveRevision = SaveRevision,
                VoyageSeed = VoyageSeed,
                BaseSpeedMilliKnots = BaseSpeedMilliKnots,
                PortBoundaryAccepted = PortBoundaryAccepted,
                PortBoundaryReference = PortBoundaryReference ?? string.Empty,
                Time = Time == null ? null : Time.Clone(),
                Course = Course == null ? null : Course.Clone(),
                ShipIdentity = ShipIdentity == null ? null : ShipIdentity.Clone(),
                HullConfiguration = HullConfiguration == null ? null : HullConfiguration.Clone(),
                ConditionState = ConditionState == null ? null : ConditionState.Clone(),
                CrewDecision = CrewDecision == null ? null : CrewDecision.Clone(),
                AnchorState = AnchorState == null ? null : AnchorState.Clone(),
                RouteDefinition = RouteDefinition == null ? null : RouteDefinition.Clone(),
                DiscoveryRecord = DiscoveryRecord == null ? null : DiscoveryRecord.Clone(),
                WeatherInput = WeatherInput == null ? null : WeatherInput.Clone(),
                WeatherImpact = WeatherImpact == null ? null : WeatherImpact.Clone(),
                ProvisionRequest = ProvisionRequest == null ? null : ProvisionRequest.Clone(),
                SaveProjection = SaveProjection == null ? null : SaveProjection.Clone(),
                LastEventId = LastEventId ?? string.Empty,
                LastTickIndex = LastTickIndex,
            };
            if (RiskSeeds != null)
            {
                foreach (D21VoyageRiskSeed seed in RiskSeeds)
                    if (seed != null) result.RiskSeeds.Add(seed.Clone());
            }
            return result;
        }

        /// <summary>校验解析状态的跨 owner 引用和固定计数器。</summary>
        public bool TryValidate(out string code)
        {
            code = string.Empty;
            if (SchemaVersion != CurrentSchemaVersion ||
                !D21ShipIdentityContracts.IsStableId(VoyageId, 128) ||
                !D21ShipIdentityContracts.IsStableId(ShipId, 128) ||
                !D21ShipIdentityContracts.IsStableId(RouteId, 128) ||
                SaveRevision < 0 || VoyageSeed < 0 || BaseSpeedMilliKnots <= 0 ||
                !Enum.IsDefined(typeof(D21VoyageStatus), Status))
            {
                code = "d21.voyage.state_metadata_invalid";
                return false;
            }
            if (Time == null || Course == null || RouteDefinition == null ||
                WeatherInput == null || WeatherImpact == null || CrewDecision == null)
            {
                code = "d21.voyage.state_projection_missing";
                return false;
            }
            try
            {
                Time.Validate();
                Course.Validate();
                if (!D21NavigationRouteContracts.ValidateRoute(RouteDefinition).Valid)
                {
                    code = "d21.voyage.route_invalid";
                    return false;
                }
                WeatherInput.Validate();
            }
            catch (InvalidOperationException)
            {
                code = "d21.voyage.state_projection_invalid";
                return false;
            }
            if (!WeatherImpact.Success || !CrewDecision.Valid || !CrewDecision.CanDepart)
            {
                code = "d21.voyage.owner_projection_rejected";
                return false;
            }
            if (RiskSeeds == null)
            {
                code = "d21.voyage.risk_seeds_missing";
                return false;
            }
            var riskTypes = new HashSet<D21VoyageRiskType>();
            foreach (D21VoyageRiskSeed seed in RiskSeeds)
            {
                if (seed == null || seed.RiskType == D21VoyageRiskType.None ||
                    seed.Seed < 0 || seed.RollPermille < 0 || seed.RollPermille > 999 ||
                    !riskTypes.Add(seed.RiskType))
                {
                    code = "d21.voyage.risk_seed_invalid";
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>一次固定 Tick 的完整结算结果。</summary>
    [Serializable]
    public sealed class D21VoyageResolutionResult
    {
        public bool Accepted;
        public bool IsDuplicate;
        public bool Applied;
        public bool IsArrived;
        public bool VoyageFailed;
        public bool IsInterrupted;
        public D21VoyageStatus Status;
        public string Code = string.Empty;
        public string CommandId = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public long SaveRevision;
        public D21VoyageTimeResult TimeResult;
        public D21VoyageRiskResult RiskResult;
        public D21ProvisionConsumptionResult ProvisionResult;
        public D21ShipConditionResult ConditionResult;
        public D21CourseCommandResult CourseResult;
        public D21VoyageEventRouteResult EventResult;
        public D21VoyageCompensationDraft Compensation;
        public D21VoyageResolutionState State;
        public D21ShipSaveProjection SaveProjection;

        /// <summary>返回不暴露内部可变快照的结果副本。</summary>
        public D21VoyageResolutionResult Clone()
        {
            return new D21VoyageResolutionResult
            {
                Accepted = Accepted,
                IsDuplicate = IsDuplicate,
                Applied = Applied,
                IsArrived = IsArrived,
                VoyageFailed = VoyageFailed,
                IsInterrupted = IsInterrupted,
                Status = Status,
                Code = Code ?? string.Empty,
                CommandId = CommandId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                SaveRevision = SaveRevision,
                TimeResult = TimeResult == null ? null : CloneTimeResult(TimeResult),
                RiskResult = RiskResult == null ? null : RiskResult.Clone(),
                ProvisionResult = ProvisionResult,
                ConditionResult = ConditionResult == null ? null : ConditionResult.Clone(),
                CourseResult = CourseResult == null ? null : CloneCourseResult(CourseResult),
                EventResult = EventResult == null ? null : EventResult.Clone(),
                Compensation = Compensation == null ? null : Compensation.Clone(),
                State = State == null ? null : State.Clone(),
                SaveProjection = SaveProjection == null ? null : SaveProjection.Clone(),
            };
        }

        private static D21VoyageTimeResult CloneTimeResult(D21VoyageTimeResult value)
        {
            var result = new D21VoyageTimeResult
            {
                Accepted = value.Accepted,
                IsDuplicate = value.IsDuplicate,
                IsArrived = value.IsArrived,
                Code = value.Code,
                DistanceAdvancedMilliNm = value.DistanceAdvancedMilliNm,
                RemainingDistanceMilliNm = value.RemainingDistanceMilliNm,
                ProjectedWorldDay = value.ProjectedWorldDay,
                ProjectedMinuteOfDay = value.ProjectedMinuteOfDay,
                ProjectedPhase = value.ProjectedPhase,
                ArrivalEstimate = value.ArrivalEstimate,
                State = value.State == null ? null : value.State.Clone(),
            };
            return result;
        }

        private static D21CourseCommandResult CloneCourseResult(D21CourseCommandResult value)
        {
            return new D21CourseCommandResult
            {
                Accepted = value.Accepted,
                IsDuplicate = value.IsDuplicate,
                Code = value.Code,
                CommandId = value.CommandId,
                CooldownUntilTick = value.CooldownUntilTick,
                State = value.State == null ? null : value.State.Clone(),
            };
        }
    }

    /// <summary>
    /// D21-L25 航程结算 owner。状态只由本类更新；船况/补给/事件分别通过
    /// 已注入的公开 owner API 提交，便于离线重放和失败路径审计。
    /// </summary>
    public sealed class D21VoyageResolutionSystem
    {
        public const string VoyageStartedCode = "d21.voyage.started";
        public const string TickAdvancedCode = "d21.voyage.tick_advanced";
        public const string ArrivedCode = "d21.voyage.arrived";
        public const string ProvisionRejectedCode = "d21.voyage.provision_rejected";
        public const string WeatherRejectedCode = "d21.voyage.weather_rejected";
        public const string ConditionRejectedCode = "d21.voyage.condition_rejected";
        public const string VoyageInactiveCode = "d21.voyage.inactive";
        public const string TickConflictCode = "d21.voyage.tick_conflict";
        public const string CommandConflictCode = "d21.voyage.command_conflict";
        public const string EventRouteFailedCode = "d21.voyage.event_route_failed";

        private readonly D21ProvisionConsumptionSystem _provisionSystem;
        private readonly D21ShipConditionSystem _conditionSystem;
        private readonly Dictionary<string, D21VoyageResolutionState> _voyages =
            new Dictionary<string, D21VoyageResolutionState>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _commandFingerprints =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, D21VoyageResolutionResult> _commandResults =
            new Dictionary<string, D21VoyageResolutionResult>(StringComparer.Ordinal);

        /// <summary>事件顺序/幂等/Save projection owner。</summary>
        public D21VoyageEventRouter EventRouter { get; }

        /// <summary>默认构造只使用显式请求和本地纯逻辑 owner。</summary>
        public D21VoyageResolutionSystem(
            D21ProvisionConsumptionSystem provisionSystem = null,
            D21ShipConditionSystem conditionSystem = null,
            D21VoyageEventRouter eventRouter = null)
        {
            _provisionSystem = provisionSystem ?? new D21ProvisionConsumptionSystem();
            _conditionSystem = conditionSystem;
            EventRouter = eventRouter ?? new D21VoyageEventRouter();
        }

        /// <summary>方便只注入事件路由器的构造重载。</summary>
        public D21VoyageResolutionSystem(D21VoyageEventRouter eventRouter)
            : this(null, null, eventRouter)
        {
        }

        /// <summary>读取航程 owner 的防御性快照。</summary>
        public D21VoyageResolutionState GetSnapshot(string voyageId)
        {
            if (string.IsNullOrEmpty(voyageId)) return null;
            D21VoyageResolutionState state;
            return _voyages.TryGetValue(voyageId, out state) ? state.Clone() : null;
        }

        /// <summary>读取当前 Save projection 的防御性副本。</summary>
        public D21ShipSaveProjection GetSaveProjection(string voyageId)
        {
            D21ShipSaveProjection routed = EventRouter.GetProjection(voyageId);
            if (routed != null) return routed;
            D21VoyageResolutionState state = GetSnapshot(voyageId);
            return state == null || state.SaveProjection == null ? null : state.SaveProjection.Clone();
        }

        /// <summary>以已通过 D21-L24 聚合的计划启动航程。</summary>
        public D21VoyageResolutionResult Start(D21VoyageStartPlan plan)
        {
            if (plan == null) return Rejected((D21VoyageStartPlan)null, "d21.voyage.start_plan_required");
            string planKey = "start|" + (plan.VoyageId ?? string.Empty);
            string fingerprint = PlanFingerprint(plan);
            D21VoyageResolutionResult previous;
            string previousFingerprint;
            if (_commandFingerprints.TryGetValue(planKey, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal) &&
                    _commandResults.TryGetValue(planKey, out previous))
                {
                    D21VoyageResolutionResult replay = previous.Clone();
                    replay.IsDuplicate = true;
                    return replay;
                }
                return Rejected(plan, CommandConflictCode);
            }
            if (_voyages.ContainsKey(plan.VoyageId ?? string.Empty))
            {
                D21VoyageResolutionResult existing = Rejected(plan, "d21.voyage.already_exists");
                Remember(planKey, fingerprint, existing);
                return existing.Clone();
            }

            string validationCode;
            if (!TryValidatePlan(plan, out validationCode))
            {
                D21VoyageResolutionResult invalid = Rejected(plan, validationCode);
                Remember(planKey, fingerprint, invalid);
                return invalid.Clone();
            }

            D21VoyageResolutionState state = FromPlan(plan);
            D21ShipSaveProjection projection = BuildProjection(state, null, string.Empty);
            D21TickContext startContext = new D21TickContext
            {
                WorldDay = plan.StartWorldDay,
                MinuteOfDay = plan.StartMinuteOfDay,
                TickIndex = 0,
                SaveRevision = plan.SaveRevision,
                VoyageSeed = plan.VoyageSeed,
                VoyageId = plan.VoyageId,
                ShipId = plan.ShipId,
            };
            D21VoyageEvent started = D21VoyageEventContracts.Create(
                D21VoyageEventType.VoyageStarted,
                plan.VoyageId,
                plan.ShipId,
                plan.RouteId,
                startContext,
                VoyageStartedCode,
                new[] { "ship:" + plan.ShipId, "route:" + plan.RouteId },
                null,
                D21VoyageEventOutcome.Normal,
                plan.CommandId,
                string.Empty,
                0,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "origin_id", plan.OriginId ?? string.Empty },
                    { "destination_id", plan.DestinationId ?? string.Empty },
                    { "port_boundary", plan.PortBoundaryAccepted ? "accepted" : "offline_explicit" },
                    { "status", D21VoyageStatus.Sailing.ToString() },
                },
                new[] { "voyage:" + plan.VoyageId });

            D21VoyageEventRouteResult routeResult = EventRouter.Append(started, projection);
            if (routeResult == null || !routeResult.Accepted)
            {
                D21VoyageResolutionResult failedRoute = Rejected(plan,
                    routeResult == null ? "d21.voyage.start_event_missing" : routeResult.Code);
                Remember(planKey, fingerprint, failedRoute);
                return failedRoute.Clone();
            }

            state.LastEventId = routeResult.Event == null ? started.EventId : routeResult.Event.EventId;
            state.SaveProjection = EventRouter.ReconcileProjection(
                routeResult.SaveProjection, state.VoyageId, state.ShipId, state.RouteId,
                state.Status, state.Time.ElapsedTicks, state.SaveRevision, state.Time,
                state.Course, state.RiskSeeds, state.AnchorState, state.WeatherInput,
                null, string.Empty);
            _voyages[state.VoyageId] = state;

            D21VoyageResolutionResult accepted = new D21VoyageResolutionResult
            {
                Accepted = true,
                IsDuplicate = routeResult.IsDuplicate,
                Applied = !routeResult.IsDuplicate,
                IsArrived = false,
                VoyageFailed = false,
                IsInterrupted = false,
                Status = state.Status,
                Code = VoyageStartedCode,
                CommandId = plan.CommandId,
                VoyageId = state.VoyageId,
                ShipId = state.ShipId,
                SaveRevision = state.SaveRevision,
                EventResult = routeResult,
                State = state.Clone(),
                SaveProjection = state.SaveProjection.Clone(),
            };
            Remember(planKey, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>按显式固定 Tick 结算速度、风险、补给和抵达。</summary>
        public D21VoyageResolutionResult ResolveTick(D21VoyageTickRequest request)
        {
            if (request == null) return Rejected((D21VoyageTickRequest)null, "d21.voyage.tick_request_required");
            string validationCode;
            if (!request.TryValidate(out validationCode))
                return Rejected(request, validationCode);

            D21VoyageResolutionState current;
            if (!_voyages.TryGetValue(request.VoyageId, out current))
                return Rejected(request, "d21.voyage.unknown_voyage");

            string commandKey = "tick|" + request.VoyageId + "|" + request.CommandId;
            string fingerprint = request.Fingerprint();
            D21VoyageResolutionResult previous;
            string previousFingerprint;
            if (_commandFingerprints.TryGetValue(commandKey, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal) &&
                    _commandResults.TryGetValue(commandKey, out previous))
                {
                    D21VoyageResolutionResult replay = previous.Clone();
                    replay.IsDuplicate = true;
                    return replay;
                }
                return Rejected(request, TickConflictCode);
            }

            if (current.Status != D21VoyageStatus.Sailing)
            {
                D21VoyageResolutionResult inactive = Rejected(current, request.CommandId, VoyageInactiveCode);
                Remember(commandKey, fingerprint, inactive);
                return inactive.Clone();
            }
            if (!string.Equals(current.ShipId, request.ShipId, StringComparison.Ordinal) ||
                request.TickContext.TickIndex != current.Time.ElapsedTicks ||
                request.TickContext.SaveRevision < current.SaveRevision)
            {
                D21VoyageResolutionResult conflict = Rejected(current, request.CommandId, TickConflictCode);
                Remember(commandKey, fingerprint, conflict);
                return conflict.Clone();
            }

            D21VoyageResolutionState candidate = current.Clone();
            D21WeatherInput weather = request.WeatherInput == null
                ? candidate.WeatherInput.Clone()
                : request.WeatherInput.Clone();
            weather.WorldDay = request.TickContext.WorldDay;
            weather.SaveRevision = request.TickContext.SaveRevision;
            D21SeaStateResult seaState = D21SeaStateSystem.EvaluateDeterministic(weather);
            D21WeatherImpactResult impact = D21WeatherImpactRules.Evaluate(weather, seaState);
            if (seaState == null || !seaState.Success || impact == null || !impact.Success || !seaState.IsNavigable)
            {
                D21VoyageResolutionResult weatherFailure = FailVoyage(
                    current, request, WeatherRejectedCode, null, null, null);
                Remember(commandKey, fingerprint, weatherFailure);
                return weatherFailure.Clone();
            }

            D21ShipConditionState condition = _conditionSystem == null
                ? candidate.ConditionState.Clone()
                : _conditionSystem.Snapshot();
            if (condition == null)
            {
                D21VoyageResolutionResult conditionFailure = FailVoyage(
                    current, request, ConditionRejectedCode, null, null, null);
                Remember(commandKey, fingerprint, conditionFailure);
                return conditionFailure.Clone();
            }
            D21ShipConditionValidationResult conditionValidation =
                new D21ShipConditionSystem(condition).ValidateState(condition);
            if (!conditionValidation.Valid ||
                (condition.Seaworthiness != D21ShipSeaworthiness.Navigable &&
                 condition.Seaworthiness != D21ShipSeaworthiness.Restricted))
            {
                D21VoyageResolutionResult conditionFailure = FailVoyage(
                    current, request, ConditionRejectedCode, null, null, null);
                Remember(commandKey, fingerprint, conditionFailure);
                return conditionFailure.Clone();
            }

            int coursePermille = candidate.Course == null ? 0 : candidate.Course.SpeedPermille;
            if (candidate.Course == null || candidate.Course.IsStopped ||
                candidate.Course.Mode == D21CourseMode.Stopped || coursePermille <= 0)
            {
                D21VoyageResolutionResult stopped = Rejected(current, request.CommandId, "d21.voyage.course_stopped");
                Remember(commandKey, fingerprint, stopped);
                return stopped.Clone();
            }
            int crewPermille = ToPermille(candidate.CrewDecision.NavigationEfficiencyMultiplier);
            int conditionPermille = condition.Seaworthiness == D21ShipSeaworthiness.Restricted ? 700 : 1000;
            int weatherPermille = ToPermille(impact.SpeedMultiplier);
            int combinedPermille = MultiplyPermille(MultiplyPermille(crewPermille, conditionPermille), weatherPermille);
            D21VoyageTimeInput timeInput = new D21VoyageTimeInput
            {
                RouteDistanceMilliNm = candidate.Time.RouteDistanceMilliNm,
                BaseSpeedMilliKnots = candidate.BaseSpeedMilliKnots,
                SpeedPermille = Math.Max(0, Math.Min(2000, coursePermille)),
                CurrentPermille = Math.Max(0, Math.Min(2000, combinedPermille)),
            };
            D21VoyageTimeResult timeResult = D21VoyageTimeSystem.Advance(
                candidate.Time, timeInput, request.TickContext);
            if (timeResult == null || !timeResult.Accepted)
            {
                D21VoyageResolutionResult timeFailure = FailVoyage(
                    current, request, "d21.voyage.time_" +
                    (timeResult == null ? "rejected" : timeResult.Code), null, timeResult, null);
                Remember(commandKey, fingerprint, timeFailure);
                return timeFailure.Clone();
            }

            D21VoyageRiskResult riskResult = EvaluateRisk(candidate, weather, seaState, condition, request.TickContext);
            if (riskResult == null || !riskResult.Accepted)
            {
                D21VoyageResolutionResult riskFailure = FailVoyage(
                    current, request, "d21.voyage.risk_rejected", null, timeResult, riskResult);
                Remember(commandKey, fingerprint, riskFailure);
                return riskFailure.Clone();
            }

            D21ProvisionConsumptionRequest provisionRequest = candidate.ProvisionRequest.Clone();
            D21ProvisionLedgerSnapshot ledgerBefore = _provisionSystem.GetSnapshot(candidate.VoyageId);
            provisionRequest.ApplicationId = "tick:" + candidate.VoyageId + ":" +
                request.TickContext.TickIndex.ToString(CultureInfo.InvariantCulture);
            provisionRequest.WorldDay = request.TickContext.WorldDay;
            provisionRequest.SaveRevision = request.TickContext.SaveRevision;
            provisionRequest.TickIndex = request.TickContext.TickIndex;
            provisionRequest.CrewCount = candidate.CrewDecision.ActiveCrewCount;
            provisionRequest.WeatherImpact = impact.Clone();
            if (ledgerBefore != null)
            {
                provisionRequest.FoodUnits = ledgerBefore.FoodRemaining;
                provisionRequest.WaterUnits = ledgerBefore.WaterRemaining;
                provisionRequest.EnforceStockMatch = true;
            }
            D21ProvisionConsumptionResult provisionResult = _provisionSystem.Consume(provisionRequest);

            var events = new List<D21VoyageEvent>();
            string previousEventId = current.LastEventId;
            events.Add(CreateRiskQualifiedEvent(candidate, request.TickContext, riskResult, previousEventId));
            string riskQualifiedId = events[events.Count - 1].EventId;
            events.Add(CreateRiskResolvedEvent(candidate, request.TickContext, riskResult, riskQualifiedId));
            string riskResolvedId = events[events.Count - 1].EventId;

            if (provisionResult == null || !provisionResult.Applied)
            {
                events.Add(CreateFailureEvent(
                    candidate, request.TickContext, ProvisionRejectedCode + ":" +
                    (provisionResult == null ? "missing" : provisionResult.FailureCode),
                    riskResolvedId, 20));
                D21VoyageEventRouteResult routeFailure = EventRouter.AppendMany(
                    events, candidate.SaveProjection);
                if (routeFailure == null || !routeFailure.Accepted)
                {
                    D21VoyageResolutionResult eventFailure = Rejected(current, request.CommandId, EventRouteFailedCode);
                    Remember(commandKey, fingerprint, eventFailure);
                    return eventFailure.Clone();
                }
                D21VoyageResolutionResult failed = BuildFailedResult(
                    current, request, ProvisionRejectedCode + ":" +
                    (provisionResult == null ? "missing" : provisionResult.FailureCode),
                    provisionResult, timeResult, riskResult, routeFailure,
                    false, provisionResult != null && provisionResult.Applied,
                    weather, impact);
                Remember(commandKey, fingerprint, failed);
                return failed.Clone();
            }

            events.Add(CreateProvisionEvent(candidate, request.TickContext, provisionResult, riskResolvedId, 20));
            string provisionEventId = events[events.Count - 1].EventId;
            events.Add(CreateTickEvent(candidate, request.TickContext, timeResult, provisionEventId, 30));
            string tickEventId = events[events.Count - 1].EventId;
            bool depleted = provisionResult.VoyageFailed;
            if (timeResult.IsArrived && !depleted)
            {
                events.Add(CreateTerminalEvent(
                    candidate, request.TickContext, D21VoyageEventType.Arrived,
                    ArrivedCode, tickEventId, 40, D21VoyageEventOutcome.Normal));
            }
            else if (depleted)
            {
                events.Add(CreateFailureEvent(
                    candidate, request.TickContext, provisionResult.FailureCode,
                    tickEventId, 40));
            }

            if (!EventRouter.CanAppend(events, candidate.SaveProjection))
            {
                D21VoyageResolutionResult eventFailure = Rejected(current, request.CommandId, EventRouteFailedCode);
                Remember(commandKey, fingerprint, eventFailure);
                return eventFailure.Clone();
            }
            D21VoyageEventRouteResult routeResult = EventRouter.AppendMany(events, candidate.SaveProjection);
            if (routeResult == null || !routeResult.Accepted)
            {
                D21VoyageResolutionResult eventFailure = Rejected(current, request.CommandId, EventRouteFailedCode);
                Remember(commandKey, fingerprint, eventFailure);
                return eventFailure.Clone();
            }

            candidate.Time = timeResult.State.Clone();
            candidate.SaveRevision = request.TickContext.SaveRevision;
            candidate.WeatherInput = weather.Clone();
            candidate.WeatherImpact = impact.Clone();
            candidate.ConditionState = condition.Clone();
            candidate.Status = depleted
                ? D21VoyageStatus.Failed
                : (timeResult.IsArrived ? D21VoyageStatus.Arrived : D21VoyageStatus.Sailing);
            candidate.LastTickIndex = request.TickContext.TickIndex;
            candidate.CurrentWaypointId = FindCurrentWaypoint(candidate.RouteDefinition, candidate.Time.DistanceTravelledMilliNm);
            AddRiskSeeds(candidate.RiskSeeds, riskResult);
            D21ProvisionLedgerSnapshot ledger = provisionResult.Ledger;
            D21VoyageEvent lastEvent = routeResult.Event;
            candidate.LastEventId = lastEvent == null ? candidate.LastEventId : lastEvent.EventId;
            candidate.SaveProjection = EventRouter.ReconcileProjection(
                routeResult.SaveProjection, candidate.VoyageId, candidate.ShipId, candidate.RouteId,
                candidate.Status, candidate.Time.ElapsedTicks, candidate.SaveRevision, candidate.Time,
                candidate.Course, candidate.RiskSeeds, candidate.AnchorState, candidate.WeatherInput,
                ledger, candidate.LastEventId);
            _voyages[candidate.VoyageId] = candidate;

            D21VoyageResolutionResult accepted = new D21VoyageResolutionResult
            {
                Accepted = !depleted,
                IsDuplicate = routeResult.IsDuplicate,
                Applied = true,
                IsArrived = timeResult.IsArrived && !depleted,
                VoyageFailed = depleted,
                IsInterrupted = false,
                Status = candidate.Status,
                Code = depleted ? provisionResult.FailureCode :
                    (timeResult.IsArrived ? ArrivedCode : TickAdvancedCode),
                CommandId = request.CommandId,
                VoyageId = candidate.VoyageId,
                ShipId = candidate.ShipId,
                SaveRevision = candidate.SaveRevision,
                TimeResult = timeResult,
                RiskResult = riskResult,
                ProvisionResult = provisionResult,
                EventResult = routeResult,
                State = candidate.Clone(),
                SaveProjection = candidate.SaveProjection.Clone(),
            };
            Remember(commandKey, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>使用 D21CourseCommandSystem 的公开纯函数处理改航命令。</summary>
        public D21VoyageResolutionResult ChangeCourse(
            string voyageId,
            string commandId,
            D21CourseCommand command)
        {
            D21VoyageResolutionState current;
            if (string.IsNullOrEmpty(voyageId) || !_voyages.TryGetValue(voyageId, out current))
                return Rejected((D21VoyageResolutionState)null, commandId, "d21.voyage.unknown_voyage");
            if (command == null || !string.Equals(commandId, command.CommandId, StringComparison.Ordinal))
                return Rejected(current, commandId, "d21.voyage.course_command_invalid");
            string key = "course|" + voyageId + "|" + commandId;
            string fingerprint = command.Fingerprint;
            D21VoyageResolutionResult previous;
            string previousFingerprint;
            if (_commandFingerprints.TryGetValue(key, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal) &&
                    _commandResults.TryGetValue(key, out previous))
                {
                    D21VoyageResolutionResult replay = previous.Clone();
                    replay.IsDuplicate = true;
                    return replay;
                }
                return Rejected(current, commandId, CommandConflictCode);
            }
            var authority = new D21CourseAuthoritySnapshot
            {
                VoyageActive = current.Status == D21VoyageStatus.Sailing,
                IsAnchored = current.AnchorState != null &&
                    (current.AnchorState.AnchorState == D21AnchorState.Holding ||
                     current.AnchorState.AnchorState == D21AnchorState.Deploying ||
                     current.AnchorState.AnchorState == D21AnchorState.Dragging),
                IsBerthed = current.AnchorState != null &&
                    current.AnchorState.MooringState == D21MooringState.Moored,
                IsSeaworthy = current.ConditionState != null &&
                    (current.ConditionState.Seaworthiness == D21ShipSeaworthiness.Navigable ||
                     current.ConditionState.Seaworthiness == D21ShipSeaworthiness.Restricted),
                CrewReady = current.CrewDecision != null && current.CrewDecision.CanDepart,
                CurrentTickIndex = current.Time.ElapsedTicks,
                ExpectedSaveRevision = current.SaveRevision,
            };
            D21CourseCommandResult courseResult = D21CourseCommandSystem.Apply(
                current.Course, command, authority);
            if (courseResult == null || !courseResult.Accepted)
            {
                D21VoyageResolutionResult rejected = Rejected(current, commandId,
                    "d21.voyage.course_" + (courseResult == null ? "rejected" : courseResult.Code));
                rejected.CourseResult = courseResult;
                Remember(key, fingerprint, rejected);
                return rejected.Clone();
            }
            D21VoyageEvent courseEvent = CreateTerminalEvent(
                current, command.TickContext, D21VoyageEventType.CourseChanged,
                "d21.voyage.course_changed", current.LastEventId, 1,
                D21VoyageEventOutcome.Normal);
            D21VoyageEventRouteResult routeResult = EventRouter.Append(courseEvent, current.SaveProjection);
            if (routeResult == null || !routeResult.Accepted)
            {
                D21VoyageResolutionResult failed = Rejected(current, commandId, EventRouteFailedCode);
                failed.CourseResult = courseResult;
                Remember(key, fingerprint, failed);
                return failed.Clone();
            }
            D21VoyageResolutionState next = current.Clone();
            next.Course = courseResult.State.Clone();
            next.SaveRevision = command.TickContext.SaveRevision;
            next.LastEventId = courseEvent.EventId;
            next.SaveProjection = EventRouter.ReconcileProjection(
                routeResult.SaveProjection, next.VoyageId, next.ShipId, next.RouteId,
                next.Status, next.Time.ElapsedTicks, next.SaveRevision, next.Time,
                next.Course, next.RiskSeeds, next.AnchorState, next.WeatherInput, null,
                next.LastEventId);
            _voyages[voyageId] = next;
            var accepted = new D21VoyageResolutionResult
            {
                Accepted = true,
                IsDuplicate = routeResult.IsDuplicate,
                Applied = !routeResult.IsDuplicate,
                Status = next.Status,
                Code = "d21.voyage.course_changed",
                CommandId = commandId,
                VoyageId = next.VoyageId,
                ShipId = next.ShipId,
                SaveRevision = next.SaveRevision,
                CourseResult = courseResult,
                EventResult = routeResult,
                State = next.Clone(),
                SaveProjection = next.SaveProjection.Clone(),
            };
            Remember(key, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>以公开航向 owner 处理停船命令并追加事实事件。</summary>
        public D21VoyageResolutionResult Stop(
            string voyageId,
            string commandId,
            D21TickContext context)
        {
            D21VoyageResolutionState current;
            if (string.IsNullOrEmpty(voyageId) || !_voyages.TryGetValue(voyageId, out current))
                return Rejected((D21VoyageResolutionState)null, commandId, "d21.voyage.unknown_voyage");
            if (context == null) return Rejected(current, commandId, "d21.voyage.stop_context_required");
            var command = new D21CourseCommand
            {
                CommandId = commandId ?? string.Empty,
                Type = D21CourseCommandType.Stop,
                VoyageId = current.VoyageId,
                ShipId = current.ShipId,
                RouteId = current.RouteId,
                TickContext = context.Clone(),
            };
            D21VoyageResolutionResult result = ChangeCourse(voyageId, commandId, command);
            if (result.CourseResult != null && result.CourseResult.Accepted)
            {
                D21VoyageResolutionState next = result.State.Clone();
                next.Status = D21VoyageStatus.Stopped;
                next.SaveProjection = EventRouter.ReconcileProjection(
                    result.SaveProjection, next.VoyageId, next.ShipId, next.RouteId,
                    next.Status, next.Time.ElapsedTicks, next.SaveRevision, next.Time,
                    next.Course, next.RiskSeeds, next.AnchorState, next.WeatherInput, null,
                    next.LastEventId);
                _voyages[voyageId] = next;
                result.Status = next.Status;
                result.State = next.Clone();
                result.SaveProjection = next.SaveProjection.Clone();
            }
            return result;
        }

        /// <summary>返港适配器成功后提交 Returned 事实；港口事实仍由 D19 写入。</summary>
        public D21VoyageResolutionResult CompleteReturn(
            string voyageId,
            string commandId,
            D21TickContext context,
            D21PortReturnResult returnResult)
        {
            D21VoyageResolutionState current;
            if (string.IsNullOrEmpty(voyageId) || !_voyages.TryGetValue(voyageId, out current))
                return Rejected((D21VoyageResolutionState)null, commandId, "d21.voyage.unknown_voyage");
            if (returnResult == null || !returnResult.Success)
                return Rejected(current, commandId, "d21.voyage.return_not_accepted");
            if (current.Status != D21VoyageStatus.Arrived)
                return Rejected(current, commandId, "d21.voyage.not_arrived");
            if (context == null) return Rejected(current, commandId, "d21.voyage.return_context_required");
            string key = "return|" + voyageId + "|" + commandId;
            string fingerprint = commandId + "|" + context.TickIndex.ToString(CultureInfo.InvariantCulture) + "|" +
                context.SaveRevision.ToString(CultureInfo.InvariantCulture);
            D21VoyageResolutionResult previous;
            string previousFingerprint;
            if (_commandFingerprints.TryGetValue(key, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal) &&
                    _commandResults.TryGetValue(key, out previous))
                {
                    D21VoyageResolutionResult replay = previous.Clone();
                    replay.IsDuplicate = true;
                    return replay;
                }
                return Rejected(current, commandId, CommandConflictCode);
            }
            D21VoyageEvent recovered = CreateTerminalEvent(
                current, context, D21VoyageEventType.Recovered,
                "d21.voyage.returned_to_port", current.LastEventId, 50,
                D21VoyageEventOutcome.Recovered);
            recovered.CauseIds.Add("port:return:" + returnResult.CommandId);
            recovered.Validate();
            D21VoyageEventRouteResult routeResult = EventRouter.Append(recovered, current.SaveProjection);
            if (routeResult == null || !routeResult.Accepted)
            {
                D21VoyageResolutionResult failed = Rejected(current, commandId, EventRouteFailedCode);
                Remember(key, fingerprint, failed);
                return failed.Clone();
            }
            D21VoyageResolutionState next = current.Clone();
            next.Status = D21VoyageStatus.Returned;
            next.SaveRevision = context.SaveRevision;
            next.LastEventId = recovered.EventId;
            next.SaveProjection = EventRouter.ReconcileProjection(
                routeResult.SaveProjection, next.VoyageId, next.ShipId, next.RouteId,
                next.Status, next.Time.ElapsedTicks, next.SaveRevision, next.Time,
                next.Course, next.RiskSeeds, next.AnchorState, next.WeatherInput, null,
                next.LastEventId);
            _voyages[voyageId] = next;
            var accepted = new D21VoyageResolutionResult
            {
                Accepted = true,
                IsDuplicate = routeResult.IsDuplicate,
                Applied = !routeResult.IsDuplicate,
                Status = next.Status,
                Code = "d21.voyage.returned_to_port",
                CommandId = commandId,
                VoyageId = next.VoyageId,
                ShipId = next.ShipId,
                SaveRevision = next.SaveRevision,
                EventResult = routeResult,
                State = next.Clone(),
                SaveProjection = next.SaveProjection.Clone(),
            };
            Remember(key, fingerprint, accepted);
            return accepted.Clone();
        }

        /// <summary>中断航程的显式恢复入口，恢复事件带有因果来源。</summary>
        public D21VoyageResolutionResult Recover(
            string voyageId,
            string commandId,
            D21TickContext context)
        {
            D21VoyageResolutionState current;
            if (string.IsNullOrEmpty(voyageId) || !_voyages.TryGetValue(voyageId, out current))
                return Rejected((D21VoyageResolutionState)null, commandId, "d21.voyage.unknown_voyage");
            if (current.Status != D21VoyageStatus.Failed && current.Status != D21VoyageStatus.Interrupted)
                return Rejected(current, commandId, "d21.voyage.not_recoverable");
            if (context == null) return Rejected(current, commandId, "d21.voyage.recovery_context_required");
            D21VoyageEvent recovered = CreateTerminalEvent(
                current, context, D21VoyageEventType.Recovered,
                "d21.voyage.recovered", current.LastEventId, 60,
                D21VoyageEventOutcome.Recovered);
            D21VoyageEventRouteResult routeResult = EventRouter.Append(recovered, current.SaveProjection);
            if (routeResult == null || !routeResult.Accepted)
                return Rejected(current, commandId, EventRouteFailedCode);
            D21VoyageResolutionState next = current.Clone();
            next.Status = D21VoyageStatus.Sailing;
            next.SaveRevision = context.SaveRevision;
            next.LastEventId = recovered.EventId;
            next.SaveProjection = EventRouter.ReconcileProjection(
                routeResult.SaveProjection, next.VoyageId, next.ShipId, next.RouteId,
                next.Status, next.Time.ElapsedTicks, next.SaveRevision, next.Time,
                next.Course, next.RiskSeeds, next.AnchorState, next.WeatherInput, null,
                next.LastEventId);
            _voyages[voyageId] = next;
            return new D21VoyageResolutionResult
            {
                Accepted = true,
                Applied = true,
                Status = next.Status,
                Code = "d21.voyage.recovered",
                CommandId = commandId,
                VoyageId = next.VoyageId,
                ShipId = next.ShipId,
                SaveRevision = next.SaveRevision,
                EventResult = routeResult,
                State = next.Clone(),
                SaveProjection = next.SaveProjection.Clone(),
            };
        }

        private D21VoyageResolutionState FromPlan(D21VoyageStartPlan plan)
        {
            D21VoyageResolutionState state = new D21VoyageResolutionState
            {
                CommandId = plan.CommandId ?? string.Empty,
                VoyageId = plan.VoyageId ?? string.Empty,
                ShipId = plan.ShipId ?? string.Empty,
                RouteId = plan.RouteId ?? string.Empty,
                OriginId = plan.OriginId ?? string.Empty,
                DestinationId = plan.DestinationId ?? string.Empty,
                CurrentWaypointId = plan.RouteDefinition.Waypoints.Count == 0
                    ? string.Empty : plan.RouteDefinition.Waypoints[0].WaypointId,
                Status = D21VoyageStatus.Sailing,
                SaveRevision = plan.SaveRevision,
                VoyageSeed = plan.VoyageSeed,
                BaseSpeedMilliKnots = plan.BaseSpeedMilliKnots,
                PortBoundaryAccepted = plan.PortBoundaryAccepted,
                PortBoundaryReference = plan.PortBoundaryReference ?? string.Empty,
                Time = plan.TimeState.Clone(),
                Course = plan.CourseState.Clone(),
                ShipIdentity = plan.ShipIdentity.Clone(),
                HullConfiguration = plan.HullConfiguration.Clone(),
                ConditionState = plan.ConditionState.Clone(),
                CrewDecision = plan.CrewDecision.Clone(),
                AnchorState = plan.AnchorState.Clone(),
                RouteDefinition = plan.RouteDefinition.Clone(),
                DiscoveryRecord = plan.DiscoveryRecord == null ? null : plan.DiscoveryRecord.Clone(),
                WeatherInput = plan.WeatherInput.Clone(),
                WeatherImpact = plan.WeatherImpact.Clone(),
                ProvisionRequest = plan.ProvisionRequest.Clone(),
                LastTickIndex = 0,
            };
            state.Time.LastSaveRevision = plan.SaveRevision;
            return state;
        }

        private bool TryValidatePlan(D21VoyageStartPlan plan, out string code)
        {
            code = string.Empty;
            if (plan.SchemaVersion != D21VoyageStartPlan.CurrentSchemaVersion ||
                !D21ShipIdentityContracts.IsStableId(plan.CommandId, 128) ||
                !D21ShipIdentityContracts.IsStableId(plan.VoyageId, 128) ||
                !D21ShipIdentityContracts.IsStableId(plan.ShipId, 128) ||
                !D21ShipIdentityContracts.IsStableId(plan.RouteId, 128) ||
                plan.StartWorldDay < 0 || plan.StartMinuteOfDay < 0 ||
                plan.StartMinuteOfDay >= D21TickContext.MinutesPerDay ||
                plan.SaveRevision < 0 || plan.VoyageSeed < 0 || plan.BaseSpeedMilliKnots <= 0)
            {
                code = "d21.voyage.start_plan_invalid";
                return false;
            }
            if (!plan.PortBoundaryAccepted &&
                !string.Equals(plan.PortBoundaryReference, "offline:port-boundary-not-supplied", StringComparison.Ordinal))
            {
                code = "d21.voyage.port_boundary_invalid";
                return false;
            }
            if (plan.ShipIdentity == null || !D21ShipIdentityContracts.Validate(plan.ShipIdentity).Valid ||
                plan.HullConfiguration == null || !D21HullComponentSystem.ValidateConfiguration(plan.HullConfiguration).Valid ||
                plan.ConditionState == null || plan.AnchorState == null || plan.CrewDecision == null ||
                plan.RouteDefinition == null || plan.WeatherInput == null || plan.WeatherImpact == null ||
                plan.TimeState == null || plan.CourseState == null || plan.ProvisionRequest == null)
            {
                code = "d21.voyage.start_owner_projection_missing";
                return false;
            }
            if (!string.Equals(plan.ShipId, plan.ShipIdentity.ShipId, StringComparison.Ordinal) ||
                !string.Equals(plan.RouteId, plan.RouteDefinition.RouteId, StringComparison.Ordinal) ||
                !plan.CrewDecision.Valid || !plan.CrewDecision.CanDepart || !plan.WeatherImpact.Success)
            {
                code = "d21.voyage.start_owner_projection_rejected";
                return false;
            }
            string anchorError;
            if (!new D21AnchorMooringSystem(plan.AnchorState).ValidateState(plan.AnchorState, out anchorError))
            {
                code = "d21.voyage.anchor_projection_invalid";
                return false;
            }
            try
            {
                plan.TimeState.Validate();
                plan.CourseState.Validate();
                plan.WeatherInput.Validate();
            }
            catch (InvalidOperationException)
            {
                code = "d21.voyage.start_projection_invalid";
                return false;
            }
            if (plan.TimeState.RouteDistanceMilliNm <= 0 || plan.TimeState.ElapsedTicks != 0 ||
                plan.TimeState.DistanceTravelledMilliNm != 0 || plan.TimeState.IsArrived)
            {
                code = "d21.voyage.start_time_state_invalid";
                return false;
            }
            return true;
        }

        private D21VoyageRiskResult EvaluateRisk(
            D21VoyageResolutionState state,
            D21WeatherInput weather,
            D21SeaStateResult seaState,
            D21ShipConditionState condition,
            D21TickContext context)
        {
            D21RouteWaypoint waypoint = FindWaypoint(state.RouteDefinition, state.Time.DistanceTravelledMilliNm);
            bool reef = false;
            bool hostile = false;
            int reefRisk = 0;
            int hostileRisk = 0;
            if (waypoint != null && state.RouteDefinition.Hazards != null)
            {
                foreach (string hazardId in waypoint.HazardZoneIds)
                {
                    foreach (D21HazardZoneDefinition hazard in state.RouteDefinition.Hazards)
                    {
                        if (hazard == null || !string.Equals(hazard.HazardZoneId, hazardId, StringComparison.Ordinal)) continue;
                        int risk = ToPermille(hazard.Severity / 100d);
                        if (hazard.Kind == D21HazardZoneKind.Reef || hazard.Kind == D21HazardZoneKind.ShallowWater)
                        {
                            reef = true;
                            reefRisk = Math.Max(reefRisk, risk);
                        }
                        if (hazard.Kind == D21HazardZoneKind.PirateWaters || hazard.Kind == D21HazardZoneKind.RestrictedWaters)
                        {
                            hostile = true;
                            hostileRisk = Math.Max(hostileRisk, risk);
                        }
                    }
                }
            }
            var input = new D21VoyageRiskInput
            {
                VoyageId = state.VoyageId,
                ShipId = state.ShipId,
                RouteId = state.RouteId,
                LocationRef = waypoint == null ? state.RouteId : waypoint.LocationRef,
                VoyageSeed = state.VoyageSeed,
                TickContext = context.Clone(),
                ReefZone = reef,
                StormActive = weather.WindForce >= 8f || seaState.Level >= D21SeaStateLevel.Storm,
                HostileWaters = hostile,
                DiseaseExposure = candidateDiseaseExposure(state),
                ReefRiskPermille = reefRisk,
                StormRiskPermille = ToPermille(seaState.Risk),
                HostileRiskPermille = hostileRisk,
                DiseaseRiskPermille = ToPermille(Math.Max(0d, 1d - state.CrewDecision.AverageHealth / 100d) * 0.25d),
                SeaStateRiskPermille = ToPermille(seaState.Risk),
                VisibilityPermille = ToPermille(weather.VisibilityFraction),
                ShipConditionPermille = condition.Seaworthiness == D21ShipSeaworthiness.Restricted ? 700 : 1000,
                CrewHealthPermille = ToPermille(state.CrewDecision.AverageHealth / 100d),
                CrewExposurePermille = ToPermille(state.CrewDecision.AverageFatigue / 100d),
                HasLookout = HasRole(state.CrewDecision, D21CrewRole.Lookout),
                HasMedicine = HasRole(state.CrewDecision, D21CrewRole.Medic),
                WeatherSnapshotId = weather.EventId ?? string.Empty,
                WeatherSnapshotSchemaVersion = weather.SchemaVersion,
                WeatherSnapshotRevision = Math.Max(0L, weather.SaveRevision),
            };
            input.InputFactIds.Add("weather:" + (weather.EventId ?? string.Empty));
            if (!string.IsNullOrEmpty(condition.LastEventId)) input.InputFactIds.Add("condition:" + condition.LastEventId);
            input.InputFactIds.Add("crew:explicit-read-projection");
            return D21VoyageRiskSystem.Evaluate(input);
        }

        private static bool candidateDiseaseExposure(D21VoyageResolutionState state)
        {
            return state.CrewDecision.AverageHealth < 60d || state.CrewDecision.AverageFatigue > 80d;
        }

        private static bool HasRole(D21CrewRequirementDecision decision, D21CrewRole role)
        {
            if (decision == null || decision.Assignments == null) return false;
            foreach (D21CrewRoleAssignment assignment in decision.Assignments)
                if (assignment != null && assignment.Role == role) return true;
            return false;
        }

        private static void AddRiskSeeds(List<D21VoyageRiskSeed> target, D21VoyageRiskResult result)
        {
            if (target == null || result == null || result.Qualifications == null) return;
            foreach (D21VoyageRiskQualification qualification in result.Qualifications)
            {
                if (qualification == null || qualification.Seed == null) continue;
                bool replaced = false;
                for (int index = 0; index < target.Count; index++)
                {
                    if (target[index] != null && target[index].RiskType == qualification.Seed.RiskType)
                    {
                        target[index] = qualification.Seed.Clone();
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) target.Add(qualification.Seed.Clone());
            }
            target.Sort((left, right) => left.RiskType.CompareTo(right.RiskType));
        }

        private D21VoyageEvent CreateRiskQualifiedEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21VoyageRiskResult result,
            string causeId)
        {
            return CreateEvent(state, context, D21VoyageEventType.RiskQualified,
                "d21.voyage.risk_qualified", causeId, 0,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "qualification_count", result.Qualifications.Count.ToString(CultureInfo.InvariantCulture) },
                    { "any_triggered", result.AnyTriggered ? "true" : "false" },
                }, null, D21VoyageEventOutcome.Normal);
        }

        private D21VoyageEvent CreateRiskResolvedEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21VoyageRiskResult result,
            string causeId)
        {
            return CreateEvent(state, context, D21VoyageEventType.RiskResolved,
                "d21.voyage.risk_resolved", causeId, 10,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "primary_risk", result.PrimaryTriggeredRisk.ToString() },
                    { "any_triggered", result.AnyTriggered ? "true" : "false" },
                }, null, D21VoyageEventOutcome.Normal);
        }

        private D21VoyageEvent CreateProvisionEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21ProvisionConsumptionResult result,
            string causeId,
            int ordinal)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "food_consumed", result.FoodConsumed.ToString(CultureInfo.InvariantCulture) },
                { "water_consumed", result.WaterConsumed.ToString(CultureInfo.InvariantCulture) },
                { "food_after", result.FoodAfter.ToString(CultureInfo.InvariantCulture) },
                { "water_after", result.WaterAfter.ToString(CultureInfo.InvariantCulture) },
                { "result_code", result.FailureCode ?? string.Empty },
            };
            D21VoyageEvent item = CreateEvent(state, context, D21VoyageEventType.ProvisionConsumed,
                "d21.voyage.provision_consumed", causeId, ordinal, values,
                new[] { "provision:" + state.VoyageId + ":" + context.TickIndex.ToString(CultureInfo.InvariantCulture) },
                D21VoyageEventOutcome.Normal);
            item.Result.Succeeded = result.Success;
            item.Result.Applied = result.Applied;
            item.Validate();
            return item;
        }

        private D21VoyageEvent CreateTickEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21VoyageTimeResult result,
            string causeId,
            int ordinal)
        {
            return CreateEvent(state, context, D21VoyageEventType.VoyageTickAdvanced,
                "d21.voyage.tick_advanced", causeId, ordinal,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "distance_advanced_mill_nm", result.DistanceAdvancedMilliNm.ToString(CultureInfo.InvariantCulture) },
                    { "remaining_mill_nm", result.RemainingDistanceMilliNm.ToString(CultureInfo.InvariantCulture) },
                    { "world_day", result.ProjectedWorldDay.ToString(CultureInfo.InvariantCulture) },
                    { "minute_of_day", result.ProjectedMinuteOfDay.ToString(CultureInfo.InvariantCulture) },
                }, null, D21VoyageEventOutcome.Normal);
        }

        private D21VoyageEvent CreateFailureEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            string code,
            string causeId,
            int ordinal)
        {
            return CreateEvent(state, context, D21VoyageEventType.Rejected, code,
                causeId, ordinal, null, null, D21VoyageEventOutcome.Failed);
        }

        private D21VoyageEvent CreateTerminalEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21VoyageEventType type,
            string code,
            string causeId,
            int ordinal,
            D21VoyageEventOutcome outcome)
        {
            return CreateEvent(state, context, type, code, causeId, ordinal, null, null, outcome);
        }

        private D21VoyageEvent CreateEvent(
            D21VoyageResolutionState state,
            D21TickContext context,
            D21VoyageEventType type,
            string code,
            string causeId,
            int ordinal,
            IDictionary<string, string> values,
            IEnumerable<string> resultFactIds,
            D21VoyageEventOutcome outcome)
        {
            var causes = new List<string>();
            if (!string.IsNullOrEmpty(causeId)) causes.Add(causeId);
            return D21VoyageEventContracts.Create(
                type, state.VoyageId, state.ShipId, state.RouteId, context,
                code, new[] { "voyage:" + state.VoyageId }, causes, outcome,
                state.CommandId, outcome == D21VoyageEventOutcome.Failed
                    ? "d21.voyage.recover" : string.Empty,
                ordinal, values, resultFactIds);
        }

        private D21ShipSaveProjection BuildProjection(
            D21VoyageResolutionState state,
            D21ProvisionLedgerSnapshot ledger,
            string lastProvisionEventId)
        {
            D21ShipSaveProjection baseProjection = state.SaveProjection == null
                ? (state == null ? null : D21ShipSaveProjection.CreateDefault(state.ShipId))
                : state.SaveProjection.Clone();
            return EventRouter.ReconcileProjection(
                baseProjection, state.VoyageId, state.ShipId, state.RouteId, state.Status,
                state.Time.ElapsedTicks, state.SaveRevision, state.Time, state.Course,
                state.RiskSeeds, state.AnchorState, state.WeatherInput, ledger,
                string.IsNullOrEmpty(lastProvisionEventId) ? state.LastEventId : lastProvisionEventId);
        }

        private D21VoyageResolutionResult FailVoyage(
            D21VoyageResolutionState current,
            D21VoyageTickRequest request,
            string code,
            D21ProvisionConsumptionResult provision,
            D21VoyageTimeResult time,
            D21VoyageRiskResult risk)
        {
            var events = new List<D21VoyageEvent>
            {
                CreateFailureEvent(current, request.TickContext, code, current.LastEventId, 0),
            };
            D21VoyageEventRouteResult eventResult = EventRouter.AppendMany(events, current.SaveProjection);
            if (eventResult == null || !eventResult.Accepted)
                return Rejected(current, request.CommandId, EventRouteFailedCode);
            D21VoyageResolutionState next = current.Clone();
            next.Status = D21VoyageStatus.Failed;
            next.SaveRevision = request.TickContext.SaveRevision;
            next.LastEventId = events[0].EventId;
            next.SaveProjection = EventRouter.ReconcileProjection(
                eventResult.SaveProjection, next.VoyageId, next.ShipId, next.RouteId,
                next.Status, next.Time.ElapsedTicks, next.SaveRevision, next.Time,
                next.Course, next.RiskSeeds, next.AnchorState, next.WeatherInput,
                null, next.LastEventId);
            _voyages[next.VoyageId] = next;
            return new D21VoyageResolutionResult
            {
                Accepted = false,
                Applied = true,
                VoyageFailed = true,
                Status = next.Status,
                Code = code,
                CommandId = request.CommandId,
                VoyageId = next.VoyageId,
                ShipId = next.ShipId,
                SaveRevision = next.SaveRevision,
                TimeResult = time,
                RiskResult = risk,
                ProvisionResult = provision,
                EventResult = eventResult,
                Compensation = new D21VoyageCompensationDraft
                {
                    CommandId = request.CommandId,
                    CauseEventId = next.LastEventId,
                    RecoveryRouteId = "d21.voyage.recover",
                    ReasonCode = code,
                    Steps = new List<string> { "restore_owner_snapshots", "resume_from_last_event" },
                },
                State = next.Clone(),
                SaveProjection = next.SaveProjection.Clone(),
            };
        }

        private D21VoyageResolutionResult BuildFailedResult(
            D21VoyageResolutionState current,
            D21VoyageTickRequest request,
            string code,
            D21ProvisionConsumptionResult provision,
            D21VoyageTimeResult time,
            D21VoyageRiskResult risk,
            D21VoyageEventRouteResult eventResult,
            bool isArrived,
            bool tickCommitted,
            D21WeatherInput weather,
            D21WeatherImpactResult impact)
        {
            D21VoyageResolutionState next = current.Clone();
            next.Status = D21VoyageStatus.Failed;
            next.SaveRevision = request.TickContext.SaveRevision;
            if (eventResult != null && eventResult.Event != null) next.LastEventId = eventResult.Event.EventId;
            if (tickCommitted && time != null && time.State != null)
            {
                next.Time = time.State.Clone();
                next.LastTickIndex = next.Time.ElapsedTicks;
                next.CurrentWaypointId = FindCurrentWaypoint(
                    next.RouteDefinition, next.Time.DistanceTravelledMilliNm);
                if (weather != null) next.WeatherInput = weather.Clone();
                if (impact != null) next.WeatherImpact = impact.Clone();
                AddRiskSeeds(next.RiskSeeds, risk);
            }
            next.SaveProjection = EventRouter.ReconcileProjection(
                eventResult == null ? current.SaveProjection : eventResult.SaveProjection,
                next.VoyageId, next.ShipId, next.RouteId, next.Status, next.Time.ElapsedTicks,
                next.SaveRevision, next.Time, next.Course, next.RiskSeeds, next.AnchorState,
                next.WeatherInput, provision == null ? null : provision.Ledger, next.LastEventId);
            _voyages[next.VoyageId] = next;
            return new D21VoyageResolutionResult
            {
                Accepted = false,
                Applied = true,
                IsArrived = isArrived,
                VoyageFailed = true,
                Status = next.Status,
                Code = code,
                CommandId = request.CommandId,
                VoyageId = next.VoyageId,
                ShipId = next.ShipId,
                SaveRevision = next.SaveRevision,
                TimeResult = time,
                RiskResult = risk,
                ProvisionResult = provision,
                EventResult = eventResult,
                Compensation = new D21VoyageCompensationDraft
                {
                    CommandId = request.CommandId,
                    CauseEventId = next.LastEventId,
                    RecoveryRouteId = "d21.voyage.recover",
                    ReasonCode = code,
                    Steps = new List<string> { "restore_owner_snapshots", "resume_from_last_event" },
                },
                State = next.Clone(),
                SaveProjection = next.SaveProjection.Clone(),
            };
        }

        private static D21VoyageResolutionResult Rejected(
            D21VoyageStartPlan plan,
            string code)
        {
            return new D21VoyageResolutionResult
            {
                Accepted = false,
                Status = D21VoyageStatus.Idle,
                Code = code ?? "d21.voyage.rejected",
                CommandId = plan == null ? string.Empty : plan.CommandId,
                VoyageId = plan == null ? string.Empty : plan.VoyageId,
                ShipId = plan == null ? string.Empty : plan.ShipId,
                SaveRevision = plan == null ? 0 : plan.SaveRevision,
            };
        }

        private static D21VoyageResolutionResult Rejected(
            D21VoyageTickRequest request,
            string code)
        {
            return new D21VoyageResolutionResult
            {
                Accepted = false,
                Status = D21VoyageStatus.Idle,
                Code = code ?? "d21.voyage.rejected",
                CommandId = request == null ? string.Empty : request.CommandId,
                VoyageId = request == null ? string.Empty : request.VoyageId,
                ShipId = request == null ? string.Empty : request.ShipId,
                SaveRevision = request == null || request.TickContext == null ? 0 : request.TickContext.SaveRevision,
            };
        }

        private static D21VoyageResolutionResult Rejected(
            D21VoyageResolutionState state,
            string commandId,
            string code)
        {
            return new D21VoyageResolutionResult
            {
                Accepted = false,
                Status = state == null ? D21VoyageStatus.Idle : state.Status,
                Code = code ?? "d21.voyage.rejected",
                CommandId = commandId ?? string.Empty,
                VoyageId = state == null ? string.Empty : state.VoyageId,
                ShipId = state == null ? string.Empty : state.ShipId,
                SaveRevision = state == null ? 0 : state.SaveRevision,
                State = state == null ? null : state.Clone(),
                SaveProjection = state == null || state.SaveProjection == null ? null : state.SaveProjection.Clone(),
            };
        }

        private static string PlanFingerprint(D21VoyageStartPlan plan)
        {
            return string.Join("|", new[]
            {
                plan.CommandId ?? string.Empty, plan.VoyageId ?? string.Empty,
                plan.ShipId ?? string.Empty, plan.RouteId ?? string.Empty,
                plan.SaveRevision.ToString(CultureInfo.InvariantCulture),
                plan.VoyageSeed.ToString(CultureInfo.InvariantCulture),
                plan.StartWorldDay.ToString(CultureInfo.InvariantCulture),
                plan.StartMinuteOfDay.ToString(CultureInfo.InvariantCulture),
                plan.TimeState == null ? string.Empty : plan.TimeState.Fingerprint,
                plan.WeatherInput == null ? string.Empty : plan.WeatherInput.Fingerprint(),
                plan.ProvisionRequest == null ? string.Empty : plan.ProvisionRequest.FoodUnits.ToString(CultureInfo.InvariantCulture) + ":" + plan.ProvisionRequest.WaterUnits.ToString(CultureInfo.InvariantCulture),
                plan.PortBoundaryReference ?? string.Empty,
            });
        }

        private void Remember(
            string commandKey,
            string fingerprint,
            D21VoyageResolutionResult result)
        {
            if (string.IsNullOrEmpty(commandKey) || result == null) return;
            _commandFingerprints[commandKey] = fingerprint ?? string.Empty;
            _commandResults[commandKey] = result.Clone();
        }

        private static int ToPermille(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return 0;
            return Math.Max(0, Math.Min(1000, (int)Math.Round(value * 1000d, MidpointRounding.AwayFromZero)));
        }

        private static int MultiplyPermille(int left, int right)
        {
            return Math.Max(0, Math.Min(2000, (int)Math.Round(left * (double)right / 1000d, MidpointRounding.AwayFromZero)));
        }

        private static D21RouteWaypoint FindWaypoint(
            D21NavigationRouteDefinition route,
            long distanceMilliNm)
        {
            if (route == null || route.Waypoints == null || route.Waypoints.Count == 0) return null;
            D21RouteWaypoint selected = route.Waypoints[0];
            double distanceNm = distanceMilliNm / 1000d;
            foreach (D21RouteWaypoint waypoint in route.Waypoints)
            {
                if (waypoint != null && waypoint.DistanceFromOriginNm <= distanceNm)
                    selected = waypoint;
            }
            return selected;
        }

        private static string FindCurrentWaypoint(
            D21NavigationRouteDefinition route,
            long distanceMilliNm)
        {
            D21RouteWaypoint waypoint = FindWaypoint(route, distanceMilliNm);
            return waypoint == null ? string.Empty : waypoint.WaypointId ?? string.Empty;
        }
    }
}
