// ============================================================
// D21NavigationRouteContracts —— D21-L08 路线、航点、危险区与发现状态（纯 C#）
//
// 路线只使用稳定 ID、逻辑位置引用、序列和距离等契约字段。这里不拥有
// 路径搜索、场景坐标、导航网格或表现层对象；路线裁决仍是纯逻辑输入输出。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>路线发现状态。</summary>
    public enum D21RouteDiscoveryState
    {
        /// <summary>没有可靠信息。</summary>
        Unknown = 0,
        /// <summary>只有传闻。</summary>
        Rumored = 1,
        /// <summary>已有海图记录。</summary>
        Charted = 2,
        /// <summary>已被船员或情报事实确认。</summary>
        Discovered = 3,
        /// <summary>当前路线被逻辑阻断。</summary>
        Blocked = 4,
        /// <summary>路线已退役。</summary>
        Retired = 5,
    }

    /// <summary>路线危险区类别。</summary>
    public enum D21HazardZoneKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>礁石区。</summary>
        Reef = 1,
        /// <summary>浅水区。</summary>
        ShallowWater = 2,
        /// <summary>风暴走廊。</summary>
        StormCorridor = 3,
        /// <summary>海盗活动水域。</summary>
        PirateWaters = 4,
        /// <summary>受限制水域。</summary>
        RestrictedWaters = 5,
        /// <summary>野生动物保护水域。</summary>
        WildlifeReserve = 6,
    }

    /// <summary>路线契约校验或评估失败的稳定原因码。</summary>
    public enum D21RouteValidationCode
    {
        /// <summary>没有错误。</summary>
        None = 0,
        /// <summary>请求缺失。</summary>
        InvalidRequest = 1,
        /// <summary>路线定义无效。</summary>
        InvalidRoute = 2,
        /// <summary>航点定义无效。</summary>
        InvalidWaypoint = 3,
        /// <summary>危险区定义无效。</summary>
        InvalidHazard = 4,
        /// <summary>发现状态不足。</summary>
        DiscoveryRequired = 5,
        /// <summary>路线已阻断或退役。</summary>
        RouteUnavailable = 6,
        /// <summary>船只吃水超过安全深度。</summary>
        DraftExceedsDepth = 7,
        /// <summary>危险区超过调用方的容忍度。</summary>
        HazardToleranceExceeded = 8,
        /// <summary>路线 ID 与请求不匹配。</summary>
        RouteMismatch = 9,
        /// <summary>引用不是稳定逻辑 ID。</summary>
        InvalidReference = 10,
        /// <summary>保存版本无效。</summary>
        InvalidSaveRevision = 11,
    }

    /// <summary>路线中的一个逻辑航点。</summary>
    public sealed class D21RouteWaypoint
    {
        /// <summary>创建空航点。</summary>
        public D21RouteWaypoint()
        {
            HazardZoneIds = new List<string>();
        }

        /// <summary>航点稳定 ID。</summary>
        public string WaypointId { get; set; }

        /// <summary>位置的稳定逻辑引用。</summary>
        public string LocationRef { get; set; }

        /// <summary>路线内的连续序号。</summary>
        public int Sequence { get; set; }

        /// <summary>从起点累计距离，单位为海里。</summary>
        public double DistanceFromOriginNm { get; set; }

        /// <summary>经过该航点所需的最小安全水深，单位为米。</summary>
        public double MinimumDepthMeters { get; set; }

        /// <summary>该航点是否需要已发现路线信息。</summary>
        public bool DiscoveryRequired { get; set; }

        /// <summary>该航点关联的危险区 ID。</summary>
        public List<string> HazardZoneIds { get; set; }

        /// <summary>复制航点。</summary>
        public D21RouteWaypoint Clone()
        {
            var clone = new D21RouteWaypoint
            {
                WaypointId = WaypointId,
                LocationRef = LocationRef,
                Sequence = Sequence,
                DistanceFromOriginNm = DistanceFromOriginNm,
                MinimumDepthMeters = MinimumDepthMeters,
                DiscoveryRequired = DiscoveryRequired,
            };
            if (HazardZoneIds != null)
            {
                clone.HazardZoneIds.AddRange(HazardZoneIds);
            }
            return clone;
        }
    }

    /// <summary>一条路线危险区的逻辑定义。</summary>
    public sealed class D21HazardZoneDefinition
    {
        /// <summary>创建空危险区。</summary>
        public D21HazardZoneDefinition()
        {
            Kind = D21HazardZoneKind.Unknown;
        }

        /// <summary>危险区稳定 ID。</summary>
        public string HazardZoneId { get; set; }

        /// <summary>危险区类别。</summary>
        public D21HazardZoneKind Kind { get; set; }

        /// <summary>危险严重度，范围为 0 到 100。</summary>
        public double Severity { get; set; }

        /// <summary>进入该危险区是否要求路线已发现。</summary>
        public bool DiscoveryRequired { get; set; }

        /// <summary>可选替代路线 ID。</summary>
        public string AlternativeRouteId { get; set; }

        /// <summary>复制危险区定义。</summary>
        public D21HazardZoneDefinition Clone()
        {
            return new D21HazardZoneDefinition
            {
                HazardZoneId = HazardZoneId,
                Kind = Kind,
                Severity = Severity,
                DiscoveryRequired = DiscoveryRequired,
                AlternativeRouteId = AlternativeRouteId,
            };
        }
    }

    /// <summary>路线发现事实的只读记录。</summary>
    public sealed class D21RouteDiscoveryRecord
    {
        /// <summary>路线稳定 ID。</summary>
        public string RouteId { get; set; }

        /// <summary>当前发现状态。</summary>
        public D21RouteDiscoveryState DiscoveryState { get; set; }

        /// <summary>发现事实 ID。</summary>
        public string FactId { get; set; }

        /// <summary>发现事实发生的逻辑世界日。</summary>
        public int WorldDay { get; set; }

        /// <summary>记录保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>复制发现记录。</summary>
        public D21RouteDiscoveryRecord Clone()
        {
            return new D21RouteDiscoveryRecord
            {
                RouteId = RouteId,
                DiscoveryState = DiscoveryState,
                FactId = FactId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>路线、航点和危险区的版本化定义。</summary>
    public sealed class D21NavigationRouteDefinition
    {
        /// <summary>创建空路线定义。</summary>
        public D21NavigationRouteDefinition()
        {
            SchemaVersion = D21NavigationRouteContracts.CurrentSchemaVersion;
            RequiredDiscoveryState = D21RouteDiscoveryState.Unknown;
            Waypoints = new List<D21RouteWaypoint>();
            Hazards = new List<D21HazardZoneDefinition>();
        }

        /// <summary>路线契约版本。</summary>
        public int SchemaVersion { get; set; }

        /// <summary>路线稳定 ID。</summary>
        public string RouteId { get; set; }

        /// <summary>起点位置稳定 ID。</summary>
        public string OriginId { get; set; }

        /// <summary>终点位置稳定 ID。</summary>
        public string DestinationId { get; set; }

        /// <summary>路线总距离，单位为海里。</summary>
        public double TotalDistanceNm { get; set; }

        /// <summary>路线全局最小安全水深，单位为米。</summary>
        public double MinimumSafeDepthMeters { get; set; }

        /// <summary>使用路线所需的最低发现状态。</summary>
        public D21RouteDiscoveryState RequiredDiscoveryState { get; set; }

        /// <summary>有序航点。</summary>
        public List<D21RouteWaypoint> Waypoints { get; set; }

        /// <summary>路线危险区定义。</summary>
        public List<D21HazardZoneDefinition> Hazards { get; set; }

        /// <summary>复制路线定义。</summary>
        public D21NavigationRouteDefinition Clone()
        {
            var clone = new D21NavigationRouteDefinition
            {
                SchemaVersion = SchemaVersion,
                RouteId = RouteId,
                OriginId = OriginId,
                DestinationId = DestinationId,
                TotalDistanceNm = TotalDistanceNm,
                MinimumSafeDepthMeters = MinimumSafeDepthMeters,
                RequiredDiscoveryState = RequiredDiscoveryState,
            };
            if (Waypoints != null)
            {
                for (var index = 0; index < Waypoints.Count; index++)
                {
                    if (Waypoints[index] != null)
                    {
                        clone.Waypoints.Add(Waypoints[index].Clone());
                    }
                }
            }
            if (Hazards != null)
            {
                for (var index = 0; index < Hazards.Count; index++)
                {
                    if (Hazards[index] != null)
                    {
                        clone.Hazards.Add(Hazards[index].Clone());
                    }
                }
            }
            return clone;
        }
    }

    /// <summary>路线使用请求。</summary>
    public sealed class D21NavigationRouteRequest
    {
        /// <summary>创建空路线请求。</summary>
        public D21NavigationRouteRequest()
        {
            KnownDiscoveryState = D21RouteDiscoveryState.Unknown;
        }

        /// <summary>幂等请求 ID。</summary>
        public string CommandId { get; set; }

        /// <summary>目标船只稳定 ID。</summary>
        public string ShipId { get; set; }

        /// <summary>目标路线稳定 ID。</summary>
        public string RouteId { get; set; }

        /// <summary>调用方已知的路线发现状态。</summary>
        public D21RouteDiscoveryState KnownDiscoveryState { get; set; }

        /// <summary>船只当前吃水，单位为米。</summary>
        public double CurrentDraftMeters { get; set; }

        /// <summary>调用方可接受的最高危险严重度。</summary>
        public double HazardTolerance { get; set; }

        /// <summary>逻辑世界日。</summary>
        public int WorldDay { get; set; }

        /// <summary>调用方观察到的保存版本。</summary>
        public int SaveRevision { get; set; }

        /// <summary>复制路线请求。</summary>
        public D21NavigationRouteRequest Clone()
        {
            return new D21NavigationRouteRequest
            {
                CommandId = CommandId,
                ShipId = ShipId,
                RouteId = RouteId,
                KnownDiscoveryState = KnownDiscoveryState,
                CurrentDraftMeters = CurrentDraftMeters,
                HazardTolerance = HazardTolerance,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>生成路线请求的确定性指纹。</summary>
        public string Fingerprint()
        {
            return string.Join("|", new[]
            {
                CommandId ?? string.Empty,
                ShipId ?? string.Empty,
                RouteId ?? string.Empty,
                KnownDiscoveryState.ToString(),
                CurrentDraftMeters.ToString("R", CultureInfo.InvariantCulture),
                HazardTolerance.ToString("R", CultureInfo.InvariantCulture),
                WorldDay.ToString(CultureInfo.InvariantCulture),
                SaveRevision.ToString(CultureInfo.InvariantCulture),
            });
        }
    }

    /// <summary>路线使用评估结果。</summary>
    public sealed class D21NavigationRouteDecision
    {
        /// <summary>创建空路线评估结果。</summary>
        public D21NavigationRouteDecision()
        {
            ReasonCode = D21RouteValidationCode.None;
            UndiscoveredHazardIds = new ReadOnlyCollection<string>(new List<string>());
            RestrictedHazardIds = new ReadOnlyCollection<string>(new List<string>());
            RestrictedWaypointIds = new ReadOnlyCollection<string>(new List<string>());
            Errors = new ReadOnlyCollection<string>(new List<string>());
        }

        /// <summary>输入是否有效。</summary>
        public bool Valid { get; set; }

        /// <summary>是否允许使用该路线。</summary>
        public bool Allowed { get; set; }

        /// <summary>请求使用的路线 ID。</summary>
        public string RouteId { get; set; }

        /// <summary>路线要求的发现状态。</summary>
        public D21RouteDiscoveryState RequiredDiscoveryState { get; set; }

        /// <summary>评估时采用的当前发现状态。</summary>
        public D21RouteDiscoveryState CurrentDiscoveryState { get; set; }

        /// <summary>路线总距离。</summary>
        public double TotalDistanceNm { get; set; }

        /// <summary>路线航点数量。</summary>
        public int WaypointCount { get; set; }

        /// <summary>未发现但被路线引用的危险区。</summary>
        public ReadOnlyCollection<string> UndiscoveredHazardIds { get; set; }

        /// <summary>超过危险容忍度的危险区。</summary>
        public ReadOnlyCollection<string> RestrictedHazardIds { get; set; }

        /// <summary>受吃水限制的航点。</summary>
        public ReadOnlyCollection<string> RestrictedWaypointIds { get; set; }

        /// <summary>首个稳定原因码。</summary>
        public D21RouteValidationCode ReasonCode { get; set; }

        /// <summary>稳定诊断原因集合。</summary>
        public ReadOnlyCollection<string> Errors { get; set; }

        /// <summary>复制路线评估结果。</summary>
        public D21NavigationRouteDecision Clone()
        {
            return new D21NavigationRouteDecision
            {
                Valid = Valid,
                Allowed = Allowed,
                RouteId = RouteId,
                RequiredDiscoveryState = RequiredDiscoveryState,
                CurrentDiscoveryState = CurrentDiscoveryState,
                TotalDistanceNm = TotalDistanceNm,
                WaypointCount = WaypointCount,
                UndiscoveredHazardIds = new ReadOnlyCollection<string>(UndiscoveredHazardIds == null ? new List<string>() : new List<string>(UndiscoveredHazardIds)),
                RestrictedHazardIds = new ReadOnlyCollection<string>(RestrictedHazardIds == null ? new List<string>() : new List<string>(RestrictedHazardIds)),
                RestrictedWaypointIds = new ReadOnlyCollection<string>(RestrictedWaypointIds == null ? new List<string>() : new List<string>(RestrictedWaypointIds)),
                ReasonCode = ReasonCode,
                Errors = new ReadOnlyCollection<string>(Errors == null ? new List<string>() : new List<string>(Errors)),
            };
        }
    }

    /// <summary>路线定义、发现状态和危险区规则的确定性契约。</summary>
    public static class D21NavigationRouteContracts
    {
        /// <summary>当前 D21-L08 路线契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>路线契约的 owner 名称。</summary>
        public const string OwnerSystem = "D21NavigationRouteContracts";

        /// <summary>校验路线定义和所有引用。</summary>
        public static D21NavigationRouteValidationResult ValidateRoute(D21NavigationRouteDefinition route)
        {
            var errors = new List<string>();
            if (route == null)
            {
                return new D21NavigationRouteValidationResult(false, D21RouteValidationCode.InvalidRoute, new[] { "route_missing" });
            }
            if (route.SchemaVersion != CurrentSchemaVersion || !D21ShipIdentityContracts.IsStableId(route.RouteId, 128) || !D21ShipIdentityContracts.IsStableId(route.OriginId, 96) || !D21ShipIdentityContracts.IsStableId(route.DestinationId, 96) || string.Equals(route.OriginId, route.DestinationId, StringComparison.Ordinal))
            {
                errors.Add("route_identity_invalid");
            }
            if (!IsFinitePositive(route.TotalDistanceNm) || !IsFiniteNonNegative(route.MinimumSafeDepthMeters) || !IsSupportedDiscoveryState(route.RequiredDiscoveryState))
            {
                errors.Add("route_measure_or_discovery_invalid");
            }
            if (route.Waypoints == null || route.Waypoints.Count < 2)
            {
                errors.Add("route_waypoints_missing");
            }
            if (route.Hazards == null)
            {
                errors.Add("route_hazards_missing");
            }

            var waypointIds = new HashSet<string>(StringComparer.Ordinal);
            var hazardIdsAtWaypoints = new List<string>();
            double previousDistance = 0d;
            if (route.Waypoints != null)
            {
                for (var index = 0; index < route.Waypoints.Count; index++)
                {
                    var waypoint = route.Waypoints[index];
                    if (waypoint == null || !D21ShipIdentityContracts.IsStableId(waypoint.WaypointId, 128) || !D21ShipIdentityContracts.IsStableId(waypoint.LocationRef, 128) || !waypointIds.Add(waypoint.WaypointId) || (index > 0 && (route.Waypoints[index - 1] == null || waypoint.Sequence != route.Waypoints[index - 1].Sequence + 1)) || index == 0 && waypoint.Sequence != 0 && waypoint.Sequence != 1 || !IsFiniteNonNegative(waypoint.DistanceFromOriginNm) || waypoint.DistanceFromOriginNm < previousDistance || waypoint.DistanceFromOriginNm > route.TotalDistanceNm || !IsFiniteNonNegative(waypoint.MinimumDepthMeters) || waypoint.HazardZoneIds == null)
                    {
                        errors.Add("route_waypoint_invalid");
                        continue;
                    }
                    previousDistance = waypoint.DistanceFromOriginNm;
                    for (var hazardIndex = 0; hazardIndex < waypoint.HazardZoneIds.Count; hazardIndex++)
                    {
                        if (!D21ShipIdentityContracts.IsStableId(waypoint.HazardZoneIds[hazardIndex], 128))
                        {
                            errors.Add("route_waypoint_hazard_reference_invalid");
                        }
                        hazardIdsAtWaypoints.Add(waypoint.HazardZoneIds[hazardIndex]);
                    }
                }
            }

            var hazardIds = new HashSet<string>(StringComparer.Ordinal);
            if (route.Hazards != null)
            {
                for (var index = 0; index < route.Hazards.Count; index++)
                {
                    var hazard = route.Hazards[index];
                    if (hazard == null || !D21ShipIdentityContracts.IsStableId(hazard.HazardZoneId, 128) || !hazardIds.Add(hazard.HazardZoneId) || !IsSupportedHazardKind(hazard.Kind) || !IsScore(hazard.Severity) || !string.IsNullOrEmpty(hazard.AlternativeRouteId) && !D21ShipIdentityContracts.IsStableId(hazard.AlternativeRouteId, 128))
                    {
                        errors.Add("route_hazard_invalid");
                    }
                }
            }
            for (var index = 0; index < hazardIdsAtWaypoints.Count; index++)
            {
                if (!hazardIds.Contains(hazardIdsAtWaypoints[index]))
                {
                    errors.Add("route_hazard_reference_missing");
                }
            }
            return errors.Count == 0 ? D21NavigationRouteValidationResult.Success() : new D21NavigationRouteValidationResult(false, D21RouteValidationCode.InvalidRoute, errors);
        }

        /// <summary>评估船只是否可按当前发现、吃水和危险容忍度使用路线。</summary>
        public static D21NavigationRouteDecision EvaluateRoute(D21NavigationRouteDefinition route, D21NavigationRouteRequest request, D21RouteDiscoveryRecord discoveryRecord)
        {
            var decision = new D21NavigationRouteDecision
            {
                RouteId = route == null ? null : route.RouteId,
            };
            var routeValidation = ValidateRoute(route);
            if (!routeValidation.Valid)
            {
                return Failure(decision, routeValidation.Code, "route_definition_invalid");
            }
            if (request == null || !D21ShipIdentityContracts.IsStableId(request.CommandId, 128) || !D21ShipIdentityContracts.IsStableId(request.ShipId, 96) || !D21ShipIdentityContracts.IsStableId(request.RouteId, 128))
            {
                return Failure(decision, D21RouteValidationCode.InvalidRequest, "route_request_invalid");
            }
            if (!string.Equals(route.RouteId, request.RouteId, StringComparison.Ordinal))
            {
                return Failure(decision, D21RouteValidationCode.RouteMismatch, "route_request_mismatch");
            }
            if (!IsSupportedDiscoveryState(request.KnownDiscoveryState) || !IsFiniteNonNegative(request.CurrentDraftMeters) || !IsScore(request.HazardTolerance) || request.WorldDay < 0 || request.SaveRevision < 0)
            {
                return Failure(decision, D21RouteValidationCode.InvalidRequest, "route_request_value_invalid");
            }
            if (discoveryRecord != null && (!string.Equals(discoveryRecord.RouteId, route.RouteId, StringComparison.Ordinal) || !IsSupportedDiscoveryState(discoveryRecord.DiscoveryState) || !D21ShipIdentityContracts.IsStableId(discoveryRecord.FactId, 128) || discoveryRecord.WorldDay < 0 || discoveryRecord.SaveRevision < 0))
            {
                return Failure(decision, D21RouteValidationCode.InvalidReference, "route_discovery_record_invalid");
            }

            var currentDiscovery = request.KnownDiscoveryState;
            if (discoveryRecord != null && DiscoveryRank(discoveryRecord.DiscoveryState) > DiscoveryRank(currentDiscovery))
            {
                currentDiscovery = discoveryRecord.DiscoveryState;
            }
            decision.Valid = true;
            decision.RouteId = route.RouteId;
            decision.RequiredDiscoveryState = route.RequiredDiscoveryState;
            decision.CurrentDiscoveryState = currentDiscovery;
            decision.TotalDistanceNm = route.TotalDistanceNm;
            decision.WaypointCount = route.Waypoints.Count;

            if (currentDiscovery == D21RouteDiscoveryState.Blocked || currentDiscovery == D21RouteDiscoveryState.Retired)
            {
                return Deny(decision, D21RouteValidationCode.RouteUnavailable, "route_discovery_state_unavailable");
            }
            if (DiscoveryRank(currentDiscovery) < DiscoveryRank(route.RequiredDiscoveryState))
            {
                return Deny(decision, D21RouteValidationCode.DiscoveryRequired, "route_discovery_required");
            }

            var restrictedWaypoints = new List<string>();
            for (var index = 0; index < route.Waypoints.Count; index++)
            {
                var waypoint = route.Waypoints[index];
                if (waypoint.DiscoveryRequired && DiscoveryRank(currentDiscovery) < DiscoveryRank(D21RouteDiscoveryState.Discovered))
                {
                    restrictedWaypoints.Add(waypoint.WaypointId);
                }
                var requiredDepth = Math.Max(route.MinimumSafeDepthMeters, waypoint.MinimumDepthMeters);
                if (request.CurrentDraftMeters > requiredDepth)
                {
                    restrictedWaypoints.Add(waypoint.WaypointId);
                }
            }
            decision.RestrictedWaypointIds = new ReadOnlyCollection<string>(Deduplicate(restrictedWaypoints));
            if (decision.RestrictedWaypointIds.Count > 0)
            {
                var hasDiscoveryRestriction = false;
                for (var index = 0; index < route.Waypoints.Count; index++)
                {
                    if (route.Waypoints[index].DiscoveryRequired && DiscoveryRank(currentDiscovery) < DiscoveryRank(D21RouteDiscoveryState.Discovered))
                    {
                        hasDiscoveryRestriction = true;
                        break;
                    }
                }
                return Deny(decision, hasDiscoveryRestriction ? D21RouteValidationCode.DiscoveryRequired : D21RouteValidationCode.DraftExceedsDepth, hasDiscoveryRestriction ? "route_waypoint_discovery_required" : "route_draft_exceeds_depth");
            }

            var undiscoveredHazards = new List<string>();
            var restrictedHazards = new List<string>();
            for (var index = 0; index < route.Hazards.Count; index++)
            {
                var hazard = route.Hazards[index];
                if (hazard.DiscoveryRequired && DiscoveryRank(currentDiscovery) < DiscoveryRank(D21RouteDiscoveryState.Discovered))
                {
                    undiscoveredHazards.Add(hazard.HazardZoneId);
                }
                if (hazard.Severity > request.HazardTolerance)
                {
                    restrictedHazards.Add(hazard.HazardZoneId);
                }
            }
            decision.UndiscoveredHazardIds = new ReadOnlyCollection<string>(Deduplicate(undiscoveredHazards));
            decision.RestrictedHazardIds = new ReadOnlyCollection<string>(Deduplicate(restrictedHazards));
            if (decision.UndiscoveredHazardIds.Count > 0)
            {
                return Deny(decision, D21RouteValidationCode.DiscoveryRequired, "route_hazard_discovery_required");
            }
            if (decision.RestrictedHazardIds.Count > 0)
            {
                return Deny(decision, D21RouteValidationCode.HazardToleranceExceeded, "route_hazard_tolerance_exceeded");
            }
            decision.Allowed = true;
            decision.ReasonCode = D21RouteValidationCode.None;
            decision.Errors = new ReadOnlyCollection<string>(new List<string>());
            return decision;
        }

        private static D21NavigationRouteDecision Failure(D21NavigationRouteDecision decision, D21RouteValidationCode code, string error)
        {
            decision.Valid = false;
            decision.Allowed = false;
            decision.ReasonCode = code;
            decision.Errors = new ReadOnlyCollection<string>(new[] { error });
            return decision;
        }

        private static D21NavigationRouteDecision Deny(D21NavigationRouteDecision decision, D21RouteValidationCode code, string error)
        {
            decision.Allowed = false;
            decision.ReasonCode = code;
            decision.Errors = new ReadOnlyCollection<string>(new[] { error });
            return decision;
        }

        private static int DiscoveryRank(D21RouteDiscoveryState state)
        {
            if (state == D21RouteDiscoveryState.Blocked || state == D21RouteDiscoveryState.Retired)
            {
                return -1;
            }
            return (int)state;
        }

        private static bool IsSupportedDiscoveryState(D21RouteDiscoveryState state)
        {
            return state >= D21RouteDiscoveryState.Unknown && state <= D21RouteDiscoveryState.Retired;
        }

        private static bool IsSupportedHazardKind(D21HazardZoneKind kind)
        {
            return kind >= D21HazardZoneKind.Reef && kind <= D21HazardZoneKind.WildlifeReserve;
        }

        private static bool IsFinitePositive(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0d;
        }

        private static bool IsFiniteNonNegative(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
        }

        private static bool IsScore(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d && value <= 100d;
        }

        private static List<string> Deduplicate(List<string> values)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < values.Count; index++)
            {
                if (seen.Add(values[index]))
                {
                    result.Add(values[index]);
                }
            }
            return result;
        }
    }

    /// <summary>路线定义校验结果。</summary>
    public sealed class D21NavigationRouteValidationResult
    {
        /// <summary>创建路线校验结果。</summary>
        public D21NavigationRouteValidationResult(bool valid, D21RouteValidationCode code, IEnumerable<string> errors)
        {
            Valid = valid;
            Code = code;
            Errors = new ReadOnlyCollection<string>(new List<string>(errors ?? new string[0]));
        }

        /// <summary>是否通过校验。</summary>
        public bool Valid { get; private set; }

        /// <summary>首个稳定原因码。</summary>
        public D21RouteValidationCode Code { get; private set; }

        /// <summary>稳定错误码集合。</summary>
        public ReadOnlyCollection<string> Errors { get; private set; }

        /// <summary>创建成功结果。</summary>
        public static D21NavigationRouteValidationResult Success()
        {
            return new D21NavigationRouteValidationResult(true, D21RouteValidationCode.None, new string[0]);
        }

        /// <summary>复制路线校验结果。</summary>
        public D21NavigationRouteValidationResult Clone()
        {
            return new D21NavigationRouteValidationResult(Valid, Code, Errors);
        }
    }
}
