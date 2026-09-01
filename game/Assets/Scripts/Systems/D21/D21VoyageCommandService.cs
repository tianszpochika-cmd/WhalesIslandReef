// ============================================================
// D21VoyageCommandService —— D21-L24 航程命令聚合入口
//
// 该服务只做跨 owner 的引用、权限、前置条件和命令顺序编排。船况、船体、
// 船员、天气、航线、补给和港口的事实仍由各自公开 owner 裁决；本文件不
// 持有任何外部可变集合，也不直接改写 D19/D20/D3/D9 的状态。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Whalebone.Systems.D19;
using Whalebone.Systems.D20;

namespace Whalebone.Systems.D21
{
    /// <summary>D21 唯一航程命令入口支持的命令类别。</summary>
    public enum D21VoyageCommandKind
    {
        None = 0,
        Start = 1,
        ResolveTick = 2,
        ChangeCourse = 3,
        Stop = 4,
        ReturnToPort = 5,
        Recover = 6,
    }

    /// <summary>D21 航程生命周期；只表达逻辑状态，不绑定场景或位姿。</summary>
    public enum D21VoyageStatus
    {
        Idle = 0,
        Sailing = 1,
        Stopped = 2,
        Arrived = 3,
        Failed = 4,
        Interrupted = 5,
        Returning = 6,
        Returned = 7,
    }

    /// <summary>
    /// 一次跨 owner 航程命令的只读输入载体。字段是快照/命令副本，进入
    /// 聚合器后会再次 clone；任何事实写入必须通过公开 owner 方法完成。
    /// </summary>
    [Serializable]
    public sealed class D21VoyageCommandRequest
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D21VoyageCommandKind Kind;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        public string VoyageId = string.Empty;
        public string RouteId = string.Empty;
        public string OriginPortId = string.Empty;
        public string DestinationPortId = string.Empty;
        public string TargetBerthId = string.Empty;
        public int WorldDay;
        public int MinuteOfDay;
        public int TickIndex;
        public long SaveRevision;
        public long VoyageSeed;
        public int BaseSpeedMilliKnots = 1000;
        public double CurrentDraftMeters;
        public double HazardTolerance = 100d;
        public D21RouteDiscoveryState KnownDiscoveryState = D21RouteDiscoveryState.Discovered;

        // 这些都是 owner 提交的只读快照或规则结果，不是命令服务的写状态。
        public D21ShipIdentity ShipIdentity;
        public D21HullComponentConfiguration HullConfiguration;
        public D21ShipConditionState ConditionState;
        public D21CrewRequirementProfile CrewProfile;
        public D21CrewRosterSnapshot CrewRoster;
        /// <summary>D20-L40 发布的船员公共只读投影；优先于旧输入。</summary>
        public D20CrewRosterProjection D20CrewProjection;
        public D21CrewRequirementDecision CrewDecision;
        public D21AnchorMooringState AnchorState;
        public D21NavigationRouteDefinition RouteDefinition;
        public D21RouteDiscoveryRecord DiscoveryRecord;
        public D21WeatherInput WeatherInput;
        public D21VoyageTimeState TimeState;
        public D21CourseState CourseState;
        public D21CourseCommand CourseCommand;
        public D21ProvisionConsumptionRequest ProvisionRequest;
        public D21PortDepartureRequest DepartureRequest;
        public D21PortReturnRequest ReturnRequest;

        /// <summary>兼容调用方使用 CommandType 的命名别名。</summary>
        public D21VoyageCommandKind CommandType
        {
            get { return Kind; }
            set { Kind = value; }
        }

        /// <summary>兼容调用方使用 CrewProjection 的命名别名。</summary>
        public D20CrewRosterProjection CrewProjection
        {
            get { return D20CrewProjection; }
            set { D20CrewProjection = value; }
        }

        /// <summary>返回隔离所有集合和 owner 快照的命令副本。</summary>
        public D21VoyageCommandRequest Clone()
        {
            return new D21VoyageCommandRequest
            {
                SchemaVersion = SchemaVersion,
                Kind = Kind,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                OriginPortId = OriginPortId ?? string.Empty,
                DestinationPortId = DestinationPortId ?? string.Empty,
                TargetBerthId = TargetBerthId ?? string.Empty,
                WorldDay = WorldDay,
                MinuteOfDay = MinuteOfDay,
                TickIndex = TickIndex,
                SaveRevision = SaveRevision,
                VoyageSeed = VoyageSeed,
                BaseSpeedMilliKnots = BaseSpeedMilliKnots,
                CurrentDraftMeters = CurrentDraftMeters,
                HazardTolerance = HazardTolerance,
                KnownDiscoveryState = KnownDiscoveryState,
                ShipIdentity = ShipIdentity == null ? null : ShipIdentity.Clone(),
                HullConfiguration = HullConfiguration == null ? null : HullConfiguration.Clone(),
                ConditionState = ConditionState == null ? null : ConditionState.Clone(),
                CrewProfile = CrewProfile == null ? null : CrewProfile.Clone(),
                CrewRoster = CrewRoster == null ? null : CrewRoster.Clone(),
                D20CrewProjection = D20CrewProjection == null ? null : D20CrewProjection.Clone(),
                CrewDecision = CrewDecision == null ? null : CrewDecision.Clone(),
                AnchorState = AnchorState == null ? null : AnchorState.Clone(),
                RouteDefinition = RouteDefinition == null ? null : RouteDefinition.Clone(),
                DiscoveryRecord = DiscoveryRecord == null ? null : DiscoveryRecord.Clone(),
                WeatherInput = WeatherInput == null ? null : WeatherInput.Clone(),
                TimeState = TimeState == null ? null : TimeState.Clone(),
                CourseState = CourseState == null ? null : CourseState.Clone(),
                CourseCommand = CourseCommand == null ? null : CourseCommand.Clone(),
                ProvisionRequest = ProvisionRequest == null ? null : ProvisionRequest.Clone(),
                DepartureRequest = DepartureRequest == null ? null : DepartureRequest.Clone(),
                ReturnRequest = CloneReturnRequest(ReturnRequest),
            };
        }

        /// <summary>生成不依赖运行时哈希的命令指纹。</summary>
        public string Fingerprint()
        {
            return string.Join("|", new[]
            {
                SchemaVersion.ToString(CultureInfo.InvariantCulture),
                Kind.ToString(), CommandId ?? string.Empty, ActorId ?? string.Empty,
                ShipId ?? string.Empty, VoyageId ?? string.Empty, RouteId ?? string.Empty,
                OriginPortId ?? string.Empty, DestinationPortId ?? string.Empty,
                TargetBerthId ?? string.Empty, WorldDay.ToString(CultureInfo.InvariantCulture),
                MinuteOfDay.ToString(CultureInfo.InvariantCulture), TickIndex.ToString(CultureInfo.InvariantCulture),
                SaveRevision.ToString(CultureInfo.InvariantCulture), VoyageSeed.ToString(CultureInfo.InvariantCulture),
                BaseSpeedMilliKnots.ToString(CultureInfo.InvariantCulture),
                CurrentDraftMeters.ToString("R", CultureInfo.InvariantCulture),
                HazardTolerance.ToString("R", CultureInfo.InvariantCulture),
                KnownDiscoveryState.ToString(),
                ShipIdentity == null ? string.Empty : ShipIdentity.Fingerprint(),
                HullConfiguration == null ? string.Empty : HullConfiguration.ShipId + ":" + HullConfiguration.SaveRevision.ToString(CultureInfo.InvariantCulture),
                ConditionState == null ? string.Empty : ConditionState.ShipId + ":" + ConditionState.SaveRevision.ToString(CultureInfo.InvariantCulture),
                CrewProfile == null ? string.Empty : CrewProfileFingerprint(CrewProfile),
                CrewRoster == null ? string.Empty : CrewRoster.SourceProjectionId + ":" + CrewRoster.SaveRevision.ToString(CultureInfo.InvariantCulture),
                D20CrewProjection == null ? string.Empty : D20CrewProjection.Fingerprint(),
                CrewDecision == null ? string.Empty : CrewDecisionFingerprint(CrewDecision),
                WeatherInput == null ? string.Empty : WeatherInput.Fingerprint(),
                RouteDefinition == null ? string.Empty : RouteDefinition.RouteId,
                CourseCommand == null ? string.Empty : CourseCommand.CommandId,
                ProvisionRequest == null ? string.Empty : ProvisionFingerprint(ProvisionRequest),
                DepartureRequest == null ? string.Empty : DepartureRequest.CommandId,
                ReturnRequest == null ? string.Empty : ReturnRequest.CommandId,
            });
        }

        private static string CrewProfileFingerprint(D21CrewRequirementProfile profile)
        {
            var values = new List<string>
            {
                profile.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                profile.MinimumCrew.ToString(CultureInfo.InvariantCulture),
                profile.StandardCrew.ToString(CultureInfo.InvariantCulture),
                profile.MaximumCrew.ToString(CultureInfo.InvariantCulture),
                profile.MinimumAverageMorale.ToString("R", CultureInfo.InvariantCulture),
                profile.MaximumAverageFatigue.ToString("R", CultureInfo.InvariantCulture),
                profile.MinimumAverageHealth.ToString("R", CultureInfo.InvariantCulture),
            };
            if (profile.RequiredRoles != null)
                foreach (D21CrewRoleRequirement role in profile.RequiredRoles)
                    values.Add(role == null ? string.Empty : role.Role + ":" + role.MinimumCount.ToString(CultureInfo.InvariantCulture));
            return string.Join(",", values);
        }

        private static string CrewDecisionFingerprint(D21CrewRequirementDecision decision)
        {
            var values = new List<string>
            {
                decision.Valid ? "1" : "0",
                decision.Eligibility.ToString(),
                decision.CanDepart ? "1" : "0",
                decision.ActiveCrewCount.ToString(CultureInfo.InvariantCulture),
                decision.AverageMorale.ToString("R", CultureInfo.InvariantCulture),
                decision.AverageFatigue.ToString("R", CultureInfo.InvariantCulture),
                decision.AverageHealth.ToString("R", CultureInfo.InvariantCulture),
                decision.RequiredCrewCount.ToString(CultureInfo.InvariantCulture),
                decision.NavigationEfficiencyMultiplier.ToString("R", CultureInfo.InvariantCulture),
                decision.ReasonCode.ToString(),
            };
            if (decision.MissingRoles != null)
                foreach (D21CrewRole role in decision.MissingRoles) values.Add("missing:" + role);
            if (decision.ReasonCodes != null)
                foreach (string reason in decision.ReasonCodes) values.Add("reason:" + (reason ?? string.Empty));
            if (decision.Assignments != null)
                foreach (D21CrewRoleAssignment assignment in decision.Assignments)
                    values.Add(assignment == null ? string.Empty : "assignment:" + assignment.Role + ":" + assignment.CrewId);
            return string.Join(",", values);
        }

        private static string ProvisionFingerprint(D21ProvisionConsumptionRequest request)
        {
            return string.Join(":", new[]
            {
                request.ApplicationId ?? string.Empty,
                request.WorldDay.ToString(CultureInfo.InvariantCulture),
                request.SaveRevision.ToString(CultureInfo.InvariantCulture),
                request.TickIndex.ToString(CultureInfo.InvariantCulture),
                request.CrewCount.ToString(CultureInfo.InvariantCulture),
                request.FoodUnits.ToString(CultureInfo.InvariantCulture),
                request.WaterUnits.ToString(CultureInfo.InvariantCulture),
                request.FoodUnitsPerCrew.ToString("R", CultureInfo.InvariantCulture),
                request.WaterUnitsPerCrew.ToString("R", CultureInfo.InvariantCulture),
                request.FoodConsumptionMultiplier.ToString("R", CultureInfo.InvariantCulture),
                request.WaterConsumptionMultiplier.ToString("R", CultureInfo.InvariantCulture),
                request.FoodLossUnits.ToString(CultureInfo.InvariantCulture),
                request.WaterLossUnits.ToString(CultureInfo.InvariantCulture),
                request.FoodLossRate.ToString("R", CultureInfo.InvariantCulture),
                request.WaterLossRate.ToString("R", CultureInfo.InvariantCulture),
                request.EnforceStockMatch ? "1" : "0",
                request.WeatherImpact == null ? string.Empty : request.WeatherImpact.InputFingerprint,
            });
        }

        private static D21PortReturnRequest CloneReturnRequest(D21PortReturnRequest request)
        {
            if (request == null) return null;
            var clone = new D21PortReturnRequest(
                request.CommandId, request.ActorId, request.ShipId, request.PortId,
                request.WorldDay, request.SaveRevision, request.Claims,
                request.TargetBerthId, request.RouteId)
            {
                SchemaVersion = request.SchemaVersion,
                ActorIds = request.ActorIds == null
                    ? new List<string>()
                    : new List<string>(request.ActorIds),
                Parameters = request.Parameters == null
                    ? new Dictionary<string, string>(StringComparer.Ordinal)
                    : new Dictionary<string, string>(request.Parameters, StringComparer.Ordinal),
            };
            return clone;
        }
    }

    /// <summary>
    /// 已通过 L24 前置校验、交给 L25 的航程启动计划。该计划只保存快照，
    /// 不把 D19/D20/D3/D9 的内部对象带入 D21 owner。
    /// </summary>
    [Serializable]
    public sealed class D21VoyageStartPlan
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public string OriginId = string.Empty;
        public string DestinationId = string.Empty;
        public int StartWorldDay;
        public int StartMinuteOfDay;
        public long SaveRevision;
        public long VoyageSeed;
        public int BaseSpeedMilliKnots;
        public bool PortBoundaryAccepted;
        public string PortBoundaryReference = string.Empty;
        public D21PortDepartureResult DepartureResult;
        public D21ShipIdentity ShipIdentity;
        public D21HullComponentConfiguration HullConfiguration;
        public D21ShipConditionState ConditionState;
        public D21CrewRequirementDecision CrewDecision;
        public string CrewSourceProjectionId = string.Empty;
        public string CrewSourceOwnerSystem = string.Empty;
        public D21AnchorMooringState AnchorState;
        public D21NavigationRouteDefinition RouteDefinition;
        public D21RouteDiscoveryRecord DiscoveryRecord;
        public D21WeatherInput WeatherInput;
        public D21SeaStateResult SeaState;
        public D21WeatherImpactResult WeatherImpact;
        public D21VoyageTimeState TimeState;
        public D21CourseState CourseState;
        public D21ProvisionConsumptionRequest ProvisionRequest;
        public D21ShipSaveProjection SaveProjection;

        /// <summary>返回可安全交给解析系统的深复制计划。</summary>
        public D21VoyageStartPlan Clone()
        {
            return new D21VoyageStartPlan
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                OriginId = OriginId ?? string.Empty,
                DestinationId = DestinationId ?? string.Empty,
                StartWorldDay = StartWorldDay,
                StartMinuteOfDay = StartMinuteOfDay,
                SaveRevision = SaveRevision,
                VoyageSeed = VoyageSeed,
                BaseSpeedMilliKnots = BaseSpeedMilliKnots,
                PortBoundaryAccepted = PortBoundaryAccepted,
                PortBoundaryReference = PortBoundaryReference ?? string.Empty,
                DepartureResult = DepartureResult,
                ShipIdentity = ShipIdentity == null ? null : ShipIdentity.Clone(),
                HullConfiguration = HullConfiguration == null ? null : HullConfiguration.Clone(),
                ConditionState = ConditionState == null ? null : ConditionState.Clone(),
                CrewDecision = CrewDecision == null ? null : CrewDecision.Clone(),
                CrewSourceProjectionId = CrewSourceProjectionId ?? string.Empty,
                CrewSourceOwnerSystem = CrewSourceOwnerSystem ?? string.Empty,
                AnchorState = AnchorState == null ? null : AnchorState.Clone(),
                RouteDefinition = RouteDefinition == null ? null : RouteDefinition.Clone(),
                DiscoveryRecord = DiscoveryRecord == null ? null : DiscoveryRecord.Clone(),
                WeatherInput = WeatherInput == null ? null : WeatherInput.Clone(),
                SeaState = SeaState == null ? null : SeaState.Clone(),
                WeatherImpact = WeatherImpact == null ? null : WeatherImpact.Clone(),
                TimeState = TimeState == null ? null : TimeState.Clone(),
                CourseState = CourseState == null ? null : CourseState.Clone(),
                ProvisionRequest = ProvisionRequest == null ? null : ProvisionRequest.Clone(),
                SaveProjection = SaveProjection == null ? null : SaveProjection.Clone(),
            };
        }
    }

    /// <summary>D21-L24 命令聚合返回值。</summary>
    [Serializable]
    public sealed class D21VoyageCommandResult
    {
        public bool Accepted;
        public bool IsDuplicate;
        public bool DependencyBlocked;
        public D21VoyageCommandKind Kind;
        public D21VoyageStatus Status;
        public string Code = string.Empty;
        public string CommandId = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public long SaveRevision;
        public D21VoyageStartPlan StartPlan;
        public D21VoyageResolutionResult Resolution;
        public D21PortDepartureResult DepartureResult;
        public D21PortReturnResult ReturnResult;
        public D21CourseCommandResult CourseResult;
        public ReadOnlyCollection<string> ValidationErrors =
            new ReadOnlyCollection<string>(new List<string>());

        /// <summary>返回不暴露内部计划/解析状态的结果副本。</summary>
        public D21VoyageCommandResult Clone()
        {
            return new D21VoyageCommandResult
            {
                Accepted = Accepted,
                IsDuplicate = IsDuplicate,
                DependencyBlocked = DependencyBlocked,
                Kind = Kind,
                Status = Status,
                Code = Code ?? string.Empty,
                CommandId = CommandId ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                SaveRevision = SaveRevision,
                StartPlan = StartPlan == null ? null : StartPlan.Clone(),
                Resolution = Resolution == null ? null : Resolution.Clone(),
                DepartureResult = DepartureResult,
                ReturnResult = ReturnResult,
                CourseResult = CourseResult == null ? null : CloneCourseResult(CourseResult),
                ValidationErrors = new ReadOnlyCollection<string>(
                    ValidationErrors == null ? new List<string>() : new List<string>(ValidationErrors)),
            };
        }

        private static D21CourseCommandResult CloneCourseResult(D21CourseCommandResult result)
        {
            return new D21CourseCommandResult
            {
                Accepted = result.Accepted,
                IsDuplicate = result.IsDuplicate,
                Code = result.Code,
                CommandId = result.CommandId,
                CooldownUntilTick = result.CooldownUntilTick,
                State = result.State == null ? null : result.State.Clone(),
            };
        }
    }

    /// <summary>
    /// D21 唯一航程命令聚合器。它只调用公开规则、快照和适配器入口；所有
    /// 命令结果先经过稳定指纹去重，再交给对应 owner，避免并行入口产生半态。
    /// </summary>
    public sealed class D21VoyageCommandService
    {
        public const string CrewProjectionRequiredCode = "d21.voyage.crew_projection_required";
        public const string CrewProjectionInvalidCode = "d21.voyage.crew_projection_invalid";
        public const string CrewDecisionInvalidCode = "d21.voyage.crew_decision_invalid";
        public const string RequestRequiredCode = "d21.voyage.request_required";
        public const string InvalidRequestCode = "d21.voyage.request_invalid";
        public const string PortAdapterRequiredCode = "d21.voyage.port_adapter_required";
        public const string RouteRejectedCode = "d21.voyage.route_rejected";
        public const string WeatherRejectedCode = "d21.voyage.weather_rejected";
        public const string ConditionRejectedCode = "d21.voyage.condition_rejected";
        public const string HullRejectedCode = "d21.voyage.hull_rejected";
        public const string AnchorRejectedCode = "d21.voyage.anchor_rejected";
        public const string VoyageAlreadyActiveCode = "d21.voyage.already_active";
        public const string CommandIdConflictCode = "d21.voyage.command_id_conflict";

        private readonly D21VoyageResolutionSystem _resolutionSystem;
        private readonly D21PortDepartureAdapter _departureAdapter;
        private readonly D21PortReturnAdapter _returnAdapter;
        private readonly Dictionary<string, string> _commandFingerprints =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, D21VoyageCommandResult> _commandResults =
            new Dictionary<string, D21VoyageCommandResult>(StringComparer.Ordinal);

        /// <summary>使用默认解析系统；港口适配器可在生产组合时注入。</summary>
        public D21VoyageCommandService(
            D21VoyageResolutionSystem resolutionSystem = null,
            D21PortDepartureAdapter departureAdapter = null,
            D21PortReturnAdapter returnAdapter = null)
        {
            _resolutionSystem = resolutionSystem ?? new D21VoyageResolutionSystem();
            _departureAdapter = departureAdapter;
            _returnAdapter = returnAdapter;
        }

        /// <summary>公开解析 owner，供纯逻辑测试和保存恢复边界读取。</summary>
        public D21VoyageResolutionSystem ResolutionSystem
        {
            get { return _resolutionSystem; }
        }

        /// <summary>公开事件路由 owner，供上层读取只读事件/保存投影。</summary>
        public D21VoyageEventRouter EventRouter
        {
            get { return _resolutionSystem.EventRouter; }
        }

        /// <summary>所有 D21 航程命令的唯一入口。</summary>
        public D21VoyageCommandResult Route(D21VoyageCommandRequest request)
        {
            if (request == null)
                return Rejected(null, RequestRequiredCode);

            string commandId = request.CommandId == null ? string.Empty : request.CommandId.Trim();
            string fingerprint = request.Fingerprint();
            D21VoyageCommandResult previous;
            string previousFingerprint;
            if (!string.IsNullOrEmpty(commandId) && _commandFingerprints.TryGetValue(commandId, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal) &&
                    _commandResults.TryGetValue(commandId, out previous))
                {
                    D21VoyageCommandResult replay = previous.Clone();
                    replay.IsDuplicate = true;
                    if (replay.Resolution != null) replay.Resolution.IsDuplicate = true;
                    return replay;
                }

                return Rejected(request, CommandIdConflictCode);
            }

            D21VoyageCommandResult result;
            string validationFailure = ValidateEnvelope(request);
            if (!string.IsNullOrEmpty(validationFailure))
            {
                result = Rejected(request, validationFailure);
                Remember(commandId, fingerprint, result);
                return result.Clone();
            }

            switch (request.Kind)
            {
                case D21VoyageCommandKind.Start:
                    result = Start(request);
                    break;
                case D21VoyageCommandKind.ResolveTick:
                    result = ResolveTick(request);
                    break;
                case D21VoyageCommandKind.ChangeCourse:
                    result = ChangeCourse(request);
                    break;
                case D21VoyageCommandKind.Stop:
                    result = Stop(request);
                    break;
                case D21VoyageCommandKind.ReturnToPort:
                    result = ReturnToPort(request);
                    break;
                case D21VoyageCommandKind.Recover:
                    result = Recover(request);
                    break;
                default:
                    result = Rejected(request, InvalidRequestCode);
                    break;
            }

            Remember(commandId, fingerprint, result);
            return result.Clone();
        }

        /// <summary>语义别名。</summary>
        public D21VoyageCommandResult Execute(D21VoyageCommandRequest request)
        {
            return Route(request);
        }

        /// <summary>语义别名。</summary>
        public D21VoyageCommandResult Command(D21VoyageCommandRequest request)
        {
            return Route(request);
        }

        /// <summary>开始航程的便捷入口；不会修改调用方请求。</summary>
        public D21VoyageCommandResult StartVoyage(D21VoyageCommandRequest request)
        {
            D21VoyageCommandRequest copy = request == null ? null : request.Clone();
            if (copy != null) copy.Kind = D21VoyageCommandKind.Start;
            return Route(copy);
        }

        /// <summary>固定 Tick 的便捷入口；仍经过唯一 Route 入口。</summary>
        public D21VoyageCommandResult ResolveVoyageTick(D21VoyageCommandRequest request)
        {
            D21VoyageCommandRequest copy = request == null ? null : request.Clone();
            if (copy != null) copy.Kind = D21VoyageCommandKind.ResolveTick;
            return Route(copy);
        }

        private D21VoyageCommandResult Start(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult baseResult = BaseResult(request);
            D21VoyageStartPlan plan;
            D21VoyageCommandResult preparationFailure;
            if (!TryBuildStartPlan(request, out plan, out preparationFailure))
                return preparationFailure;

            D21VoyageResolutionResult resolution = _resolutionSystem.Start(plan);
            baseResult.Accepted = resolution != null && resolution.Accepted;
            baseResult.IsDuplicate = resolution != null && resolution.IsDuplicate;
            baseResult.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            baseResult.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            baseResult.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            baseResult.StartPlan = plan;
            baseResult.Resolution = resolution;
            baseResult.DepartureResult = plan == null ? null : plan.DepartureResult;
            return baseResult;
        }

        private D21VoyageCommandResult ResolveTick(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult result = BaseResult(request);
            var tickRequest = new D21VoyageTickRequest
            {
                CommandId = request.CommandId,
                VoyageId = request.VoyageId,
                ShipId = request.ShipId,
                TickContext = D21VoyageTimeSystem.CreateTickContext(
                    request.WorldDay, request.MinuteOfDay, request.TickIndex,
                    request.SaveRevision, request.VoyageSeed,
                    request.VoyageId, request.ShipId),
                WeatherInput = request.WeatherInput == null ? null : request.WeatherInput.Clone(),
            };
            D21VoyageResolutionResult resolution = _resolutionSystem.ResolveTick(tickRequest);
            result.Accepted = resolution != null && resolution.Accepted;
            result.IsDuplicate = resolution != null && resolution.IsDuplicate;
            result.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            result.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            result.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            result.Resolution = resolution;
            return result;
        }

        private D21VoyageCommandResult ChangeCourse(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult result = BaseResult(request);
            D21CourseCommand command = request.CourseCommand == null ? null : request.CourseCommand.Clone();
            if (command == null)
            {
                result.Code = "d21.voyage.course_command_required";
                return result;
            }
            D21VoyageResolutionResult resolution = _resolutionSystem.ChangeCourse(
                request.VoyageId, request.CommandId, command);
            result.Accepted = resolution != null && resolution.Accepted;
            result.IsDuplicate = resolution != null && resolution.IsDuplicate;
            result.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            result.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            result.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            result.Resolution = resolution;
            result.CourseResult = resolution == null ? null : resolution.CourseResult;
            return result;
        }

        private D21VoyageCommandResult Stop(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult result = BaseResult(request);
            D21VoyageTickRequest tickRequest = BuildTickRequest(request);
            D21VoyageResolutionResult resolution = _resolutionSystem.Stop(
                request.VoyageId, request.CommandId, tickRequest.TickContext);
            result.Accepted = resolution != null && resolution.Accepted;
            result.IsDuplicate = resolution != null && resolution.IsDuplicate;
            result.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            result.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            result.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            result.Resolution = resolution;
            result.CourseResult = resolution == null ? null : resolution.CourseResult;
            return result;
        }

        private D21VoyageCommandResult ReturnToPort(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult result = BaseResult(request);
            if (_returnAdapter == null)
            {
                result.Code = PortAdapterRequiredCode;
                return result;
            }
            D21PortReturnRequest returnRequest = request.ReturnRequest;
            if (returnRequest == null)
            {
                result.Code = "d21.voyage.return_request_required";
                return result;
            }
            D21PortReturnResult returned = _returnAdapter.Return(returnRequest);
            result.ReturnResult = returned;
            if (returned == null || !returned.Success)
            {
                result.Code = returned == null ? "d21.voyage.return_result_missing" : returned.FailureCode;
                return result;
            }
            D21VoyageTickRequest tickRequest = BuildTickRequest(request);
            D21VoyageResolutionResult resolution = _resolutionSystem.CompleteReturn(
                request.VoyageId, request.CommandId, tickRequest.TickContext, returned);
            result.Accepted = resolution != null && resolution.Accepted;
            result.IsDuplicate = resolution != null && resolution.IsDuplicate;
            result.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            result.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            result.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            result.Resolution = resolution;
            return result;
        }

        private D21VoyageCommandResult Recover(D21VoyageCommandRequest request)
        {
            D21VoyageCommandResult result = BaseResult(request);
            D21VoyageTickRequest tickRequest = BuildTickRequest(request);
            D21VoyageResolutionResult resolution = _resolutionSystem.Recover(
                request.VoyageId, request.CommandId, tickRequest.TickContext);
            result.Accepted = resolution != null && resolution.Accepted;
            result.IsDuplicate = resolution != null && resolution.IsDuplicate;
            result.Status = resolution == null ? D21VoyageStatus.Failed : resolution.Status;
            result.Code = resolution == null ? "d21.voyage.resolution_missing" : resolution.Code;
            result.SaveRevision = resolution == null ? request.SaveRevision : resolution.SaveRevision;
            result.Resolution = resolution;
            return result;
        }

        private D21VoyageTickRequest BuildTickRequest(D21VoyageCommandRequest request)
        {
            return new D21VoyageTickRequest
            {
                CommandId = request.CommandId,
                VoyageId = request.VoyageId,
                ShipId = request.ShipId,
                TickContext = D21VoyageTimeSystem.CreateTickContext(
                    request.WorldDay, request.MinuteOfDay, request.TickIndex,
                    request.SaveRevision, request.VoyageSeed,
                    request.VoyageId, request.ShipId),
                WeatherInput = request.WeatherInput == null ? null : request.WeatherInput.Clone(),
            };
        }

        private bool TryBuildStartPlan(
            D21VoyageCommandRequest request,
            out D21VoyageStartPlan plan,
            out D21VoyageCommandResult failure)
        {
            plan = null;
            failure = BaseResult(request);

            if (request.ShipIdentity == null || !D21ShipIdentityContracts.Validate(request.ShipIdentity).Valid)
            {
                failure.Code = "d21.voyage.ship_identity_invalid";
                return false;
            }
            if (request.HullConfiguration == null ||
                !D21HullComponentSystem.ValidateConfiguration(request.HullConfiguration).Valid)
            {
                failure.Code = HullRejectedCode;
                return false;
            }
            if (request.ConditionState == null)
            {
                failure.Code = ConditionRejectedCode;
                return false;
            }
            var conditionSystem = new D21ShipConditionSystem(request.ConditionState);
            D21ShipConditionValidationResult conditionValidation = conditionSystem.ValidateState(request.ConditionState);
            if (!conditionValidation.Valid ||
                (request.ConditionState.Seaworthiness != D21ShipSeaworthiness.Navigable &&
                 request.ConditionState.Seaworthiness != D21ShipSeaworthiness.Restricted))
            {
                failure.Code = ConditionRejectedCode;
                return false;
            }
            if (request.AnchorState == null)
            {
                failure.Code = AnchorRejectedCode;
                return false;
            }
            var anchorSystem = new D21AnchorMooringSystem(request.AnchorState);
            string anchorError;
            if (!anchorSystem.ValidateState(request.AnchorState, out anchorError) ||
                request.AnchorState.MooringState == D21MooringState.Moored ||
                request.AnchorState.MooringState == D21MooringState.Approaching ||
                request.AnchorState.MooringState == D21MooringState.Securing ||
                request.AnchorState.AnchorState == D21AnchorState.Holding ||
                request.AnchorState.AnchorState == D21AnchorState.Deploying ||
                request.AnchorState.AnchorState == D21AnchorState.Recovering)
            {
                failure.Code = AnchorRejectedCode;
                return false;
            }

            string crewSourceProjectionId;
            string crewSourceOwnerSystem;
            D21CrewRequirementDecision crewDecision = EvaluateCrew(
                request, out failure, out crewSourceProjectionId,
                out crewSourceOwnerSystem);
            if (crewDecision == null)
                return false;
            if (!crewDecision.Valid || !crewDecision.CanDepart)
            {
                failure.Code = "d21.voyage.crew_not_ready";
                return false;
            }

            if (request.RouteDefinition == null)
            {
                failure.Code = RouteRejectedCode;
                return false;
            }
            D21NavigationRouteRequest routeRequest = new D21NavigationRouteRequest
            {
                CommandId = request.CommandId,
                ShipId = request.ShipId,
                RouteId = request.RouteId,
                KnownDiscoveryState = request.KnownDiscoveryState,
                CurrentDraftMeters = request.CurrentDraftMeters,
                HazardTolerance = request.HazardTolerance,
                WorldDay = request.WorldDay,
                SaveRevision = checked((int)request.SaveRevision),
            };
            D21NavigationRouteDecision routeDecision = D21NavigationRouteContracts.EvaluateRoute(
                request.RouteDefinition, routeRequest, request.DiscoveryRecord);
            if (routeDecision == null || !routeDecision.Valid || !routeDecision.Allowed)
            {
                failure.Code = RouteRejectedCode + ":" +
                    (routeDecision == null ? "missing" : routeDecision.ReasonCode.ToString());
                return false;
            }

            D21WeatherInput weather = request.WeatherInput == null
                ? D21WeatherInput.CreateDefault()
                : request.WeatherInput.Clone();
            if (weather.WorldDay != request.WorldDay || weather.SaveRevision < 0)
            {
                failure.Code = WeatherRejectedCode;
                return false;
            }
            D21SeaStateResult seaState = D21SeaStateSystem.EvaluateDeterministic(weather);
            D21WeatherImpactResult weatherImpact = D21WeatherImpactRules.Evaluate(weather, seaState);
            if (seaState == null || !seaState.Success || weatherImpact == null || !weatherImpact.Success ||
                !seaState.IsNavigable)
            {
                failure.Code = WeatherRejectedCode;
                return false;
            }

            if (request.BaseSpeedMilliKnots <= 0 || request.VoyageSeed < 0 || request.SaveRevision < 0 ||
                request.WorldDay < 0 || request.MinuteOfDay < 0 || request.MinuteOfDay >= D21TickContext.MinutesPerDay)
            {
                failure.Code = InvalidRequestCode;
                return false;
            }
            if (!D21ShipIdentityContracts.IsStableId(request.VoyageId, 128) ||
                !string.Equals(request.ShipId, request.ShipIdentity.ShipId, StringComparison.Ordinal) ||
                !string.Equals(request.RouteId, request.RouteDefinition.RouteId, StringComparison.Ordinal))
            {
                failure.Code = InvalidRequestCode;
                return false;
            }

            long distanceMilliNm;
            try
            {
                double scaledDistance = request.RouteDefinition.TotalDistanceNm * 1000d;
                if (double.IsNaN(scaledDistance) || double.IsInfinity(scaledDistance) ||
                    scaledDistance <= 0d || scaledDistance > long.MaxValue)
                {
                    failure.Code = RouteRejectedCode;
                    return false;
                }
                distanceMilliNm = checked((long)Math.Round(scaledDistance, MidpointRounding.AwayFromZero));
            }
            catch (OverflowException)
            {
                failure.Code = RouteRejectedCode;
                return false;
            }

            D21VoyageTimeState timeState = request.TimeState == null
                ? D21VoyageTimeState.CreateInitial(
                    request.VoyageId, request.ShipId, distanceMilliNm,
                    request.WorldDay, request.MinuteOfDay, request.SaveRevision)
                : request.TimeState.Clone();
            if (timeState.RouteDistanceMilliNm != distanceMilliNm ||
                !string.Equals(timeState.VoyageId, request.VoyageId, StringComparison.Ordinal) ||
                !string.Equals(timeState.ShipId, request.ShipId, StringComparison.Ordinal))
            {
                failure.Code = InvalidRequestCode;
                return false;
            }

            D21CourseState courseState = request.CourseState == null
                ? CreateInitialCourse(request.VoyageId, request.ShipId)
                : request.CourseState.Clone();
            try
            {
                courseState.Validate();
            }
            catch (InvalidOperationException)
            {
                failure.Code = InvalidRequestCode;
                return false;
            }

            D21ProvisionConsumptionRequest provision = request.ProvisionRequest == null
                ? new D21ProvisionConsumptionRequest()
                : request.ProvisionRequest.Clone();
            provision.VoyageId = request.VoyageId;
            provision.ShipId = request.ShipId;
            provision.WorldDay = request.WorldDay;
            provision.SaveRevision = request.SaveRevision;
            provision.CrewCount = crewDecision.ActiveCrewCount;
            if (string.IsNullOrEmpty(provision.ApplicationId)) provision.ApplicationId = "start:" + request.VoyageId;

            D21PortDepartureResult departureResult = null;
            bool portAccepted = false;
            string portReference = "offline:port-boundary-not-supplied";
            if (request.DepartureRequest != null)
            {
                if (_departureAdapter == null)
                {
                    failure.Code = PortAdapterRequiredCode;
                    return false;
                }
                departureResult = _departureAdapter.Depart(request.DepartureRequest);
                if (departureResult == null || !departureResult.Success)
                {
                    failure.DepartureResult = departureResult;
                    failure.Code = departureResult == null
                        ? "d21.voyage.departure_result_missing"
                        : departureResult.FailureCode;
                    return false;
                }
                portAccepted = true;
                portReference = departureResult.CommandId;
            }

            plan = new D21VoyageStartPlan
            {
                CommandId = request.CommandId,
                ActorId = request.ActorId,
                VoyageId = request.VoyageId,
                ShipId = request.ShipId,
                RouteId = request.RouteId,
                OriginId = request.OriginPortId,
                DestinationId = string.IsNullOrEmpty(request.DestinationPortId)
                    ? request.RouteDefinition.DestinationId
                    : request.DestinationPortId,
                StartWorldDay = request.WorldDay,
                StartMinuteOfDay = request.MinuteOfDay,
                SaveRevision = request.SaveRevision,
                VoyageSeed = request.VoyageSeed,
                BaseSpeedMilliKnots = request.BaseSpeedMilliKnots,
                PortBoundaryAccepted = portAccepted,
                PortBoundaryReference = portReference,
                DepartureResult = departureResult,
                ShipIdentity = request.ShipIdentity.Clone(),
                HullConfiguration = request.HullConfiguration.Clone(),
                ConditionState = request.ConditionState.Clone(),
                CrewDecision = crewDecision,
                CrewSourceProjectionId = crewSourceProjectionId,
                CrewSourceOwnerSystem = crewSourceOwnerSystem,
                AnchorState = request.AnchorState.Clone(),
                RouteDefinition = request.RouteDefinition.Clone(),
                DiscoveryRecord = request.DiscoveryRecord == null ? null : request.DiscoveryRecord.Clone(),
                WeatherInput = weather,
                SeaState = seaState.Clone(),
                WeatherImpact = weatherImpact.Clone(),
                TimeState = timeState,
                CourseState = courseState,
                ProvisionRequest = provision,
                SaveProjection = null,
            };
            failure.DepartureResult = departureResult;
            return true;
        }

        private static D21CrewRequirementDecision EvaluateCrew(
            D21VoyageCommandRequest request,
            out D21VoyageCommandResult failure,
            out string sourceProjectionId,
            out string sourceOwnerSystem)
        {
            failure = BaseResult(request);
            sourceProjectionId = string.Empty;
            sourceOwnerSystem = string.Empty;

            D21CrewRosterSnapshot roster = null;
            if (request.D20CrewProjection != null)
            {
                D21CrewProjectionMappingResult mapping = D21CrewProjectionAdapter.Map(
                    request.D20CrewProjection, request.ShipId, request.SaveRevision);
                if (mapping == null || !mapping.Valid || mapping.Roster == null)
                {
                    failure.Code = CrewProjectionInvalidCode;
                    failure.ValidationErrors = mapping == null || mapping.Errors == null
                        ? new ReadOnlyCollection<string>(new[] { "d21.crew.mapping_result_missing" })
                        : mapping.Errors;
                    return null;
                }
                roster = mapping.Roster;
                sourceProjectionId = mapping.SourceProjectionId ?? string.Empty;
                sourceOwnerSystem = mapping.SourceOwnerSystem ?? string.Empty;
            }
            else if (request.CrewRoster != null)
            {
                // 兼容已由上游验证过的 D21 roster，但仍通过 Evaluate 再次校验。
                roster = request.CrewRoster.Clone();
                sourceProjectionId = roster.SourceProjectionId ?? string.Empty;
            }
            else if (request.CrewDecision != null)
            {
                // D21-L27 历史离线调用仍提交已计算结果；只保留严格形状校验，
                // 新的 D20 public projection 路径永远优先并重新执行 D21 规则。
                ReadOnlyCollection<string> decisionErrors;
                if (D21CrewRequirementRules.ValidateDecision(
                    request.CrewDecision, out decisionErrors) !=
                    D21CrewRequirementFailureCode.None)
                {
                    failure.Code = CrewDecisionInvalidCode;
                    failure.ValidationErrors = decisionErrors;
                    return null;
                }
                return request.CrewDecision.Clone();
            }
            else
            {
                failure.DependencyBlocked = true;
                failure.Code = CrewProjectionRequiredCode;
                return null;
            }

            D21CrewRequirementDecision decision = D21CrewRequirementRules.Evaluate(
                request.CrewProfile, roster);
            if (decision == null || !decision.Valid)
            {
                failure.Code = CrewProjectionInvalidCode;
                var errors = new List<string>();
                ReadOnlyCollection<string> profileErrors;
                if (D21CrewRequirementRules.ValidateProfile(
                    request.CrewProfile, out profileErrors) !=
                    D21CrewRequirementFailureCode.None && profileErrors != null)
                {
                    foreach (string error in profileErrors)
                        errors.Add("d21.crew.profile:" + (error ?? string.Empty));
                }
                ReadOnlyCollection<string> rosterErrors;
                if (D21CrewRequirementRules.ValidateRoster(
                    roster, out rosterErrors) !=
                    D21CrewRequirementFailureCode.None && rosterErrors != null)
                {
                    foreach (string error in rosterErrors)
                        errors.Add("d21.crew.roster:" + (error ?? string.Empty));
                }
                failure.ValidationErrors = new ReadOnlyCollection<string>(errors);
                return null;
            }
            return decision;
        }

        private static D21CourseState CreateInitialCourse(string voyageId, string shipId)
        {
            D21CourseState state = D21CourseState.CreateInitial(voyageId, shipId);
            state.Mode = D21CourseMode.Course;
            state.HasCourse = true;
            state.IsStopped = false;
            state.HeadingCentidegrees = 0;
            state.SpeedPermille = 1000;
            state.CooldownUntilTick = 0;
            state.Validate();
            return state;
        }

        private static string ValidateEnvelope(D21VoyageCommandRequest request)
        {
            if (request == null) return RequestRequiredCode;
            if (request.SchemaVersion != D21VoyageCommandRequest.CurrentSchemaVersion ||
                request.Kind == D21VoyageCommandKind.None)
                return InvalidRequestCode;
            if (!D21ShipIdentityContracts.IsStableId(request.CommandId, 128) ||
                !D21ShipIdentityContracts.IsStableId(request.ShipId, 128) ||
                !D21ShipIdentityContracts.IsStableId(request.VoyageId, 128))
                return InvalidRequestCode;
            if (!string.IsNullOrEmpty(request.ActorId) &&
                !D21ShipIdentityContracts.IsStableId(request.ActorId, 160))
                return InvalidRequestCode;
            if (request.WorldDay < 0 || request.MinuteOfDay < 0 ||
                request.MinuteOfDay >= D21TickContext.MinutesPerDay ||
                request.TickIndex < 0 || request.SaveRevision < 0 || request.VoyageSeed < 0)
                return InvalidRequestCode;
            return string.Empty;
        }

        private static D21VoyageCommandResult BaseResult(D21VoyageCommandRequest request)
        {
            return new D21VoyageCommandResult
            {
                Accepted = false,
                IsDuplicate = false,
                DependencyBlocked = false,
                Kind = request == null ? D21VoyageCommandKind.None : request.Kind,
                Status = D21VoyageStatus.Idle,
                Code = string.Empty,
                CommandId = request == null ? string.Empty : request.CommandId ?? string.Empty,
                VoyageId = request == null ? string.Empty : request.VoyageId ?? string.Empty,
                ShipId = request == null ? string.Empty : request.ShipId ?? string.Empty,
                RouteId = request == null ? string.Empty : request.RouteId ?? string.Empty,
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
        }

        private static D21VoyageCommandResult Rejected(
            D21VoyageCommandRequest request,
            string code)
        {
            D21VoyageCommandResult result = BaseResult(request);
            result.Code = code ?? InvalidRequestCode;
            return result;
        }

        private void Remember(
            string commandId,
            string fingerprint,
            D21VoyageCommandResult result)
        {
            if (string.IsNullOrEmpty(commandId) || result == null) return;
            _commandFingerprints[commandId] = fingerprint ?? string.Empty;
            _commandResults[commandId] = result.Clone();
        }
    }
}
