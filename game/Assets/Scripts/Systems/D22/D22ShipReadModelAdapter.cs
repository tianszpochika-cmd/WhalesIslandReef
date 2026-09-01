// ============================================================
// D22ShipReadModelAdapter —— D22-L19 船舶只读 UI 适配器（纯 C#）
//
// D21 是船况、航线、天气、航程时间和补给账本的 owner。本适配器只复制
// D21 已公开的快照/结果；不依赖聚合服务或保存路径，也不在 UI 层推导
// 任何新的船舶事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D22
{
    /// <summary>D21 船舶 owner 的输入边界。</summary>
    [Serializable]
    public sealed class D22ShipOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public bool HasWorldDay;
        public bool HasSaveRevision;
        public int WorldDay;
        public long SaveRevision;
        public D21ShipIdentity Identity;
        public D21ShipConditionState ConditionState;
        public D21ShipConditionResult LastConditionResult;
        public D21NavigationRouteDefinition Route;
        public D21WeatherImpactResult WeatherImpact;
        public D21VoyageTimeState Time;
        public D21VoyageTimeResult LastTimeResult;
        public D21ProvisionLedgerSnapshot ProvisionLedger;
        public D21ProvisionConsumptionResult LastProvisionResult;
        public string VoyageId = string.Empty;
        public string VoyageStatus = string.Empty;
        public string RouteId = string.Empty;
        public string OriginId = string.Empty;
        public string DestinationId = string.Empty;
        public string CurrentWaypointId = string.Empty;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>船况损伤条目只读投影。</summary>
    [Serializable]
    public sealed class D22ShipDamageReadModel
    {
        public string DamageId { get; }
        public string Kind { get; }
        public string ComponentId { get; }
        public string Severity { get; }
        public double Amount { get; }
        public bool Active { get; }
        public string SourceFactId { get; }
        public int WorldDay { get; }
        public int SaveRevision { get; }

        internal D22ShipDamageReadModel(D21ShipDamageEntry source)
        {
            DamageId = source == null ? string.Empty : source.DamageId ?? string.Empty;
            Kind = source == null ? string.Empty : source.Kind.ToString();
            ComponentId = source == null ? string.Empty : source.ComponentId ?? string.Empty;
            Severity = source == null ? string.Empty : source.Severity.ToString();
            Amount = source == null ? 0d : source.Amount;
            Active = source != null && source.Active;
            SourceFactId = source == null ? string.Empty : source.SourceFactId ?? string.Empty;
            WorldDay = source == null ? 0 : source.WorldDay;
            SaveRevision = source == null ? 0 : source.SaveRevision;
        }

        private D22ShipDamageReadModel(
            string damageId,
            string kind,
            string componentId,
            string severity,
            double amount,
            bool active,
            string sourceFactId,
            int worldDay,
            int saveRevision)
        {
            DamageId = damageId ?? string.Empty;
            Kind = kind ?? string.Empty;
            ComponentId = componentId ?? string.Empty;
            Severity = severity ?? string.Empty;
            Amount = amount;
            Active = active;
            SourceFactId = sourceFactId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
        }

        public D22ShipDamageReadModel Clone()
        {
            return new D22ShipDamageReadModel(
                DamageId, Kind, ComponentId, Severity, Amount, Active, SourceFactId, WorldDay, SaveRevision);
        }
    }

    /// <summary>船况只读投影。</summary>
    [Serializable]
    public sealed class D22ShipConditionReadModel
    {
        public bool HasProjection { get; }
        public string ShipId { get; }
        public double MaxDurability { get; }
        public double Durability { get; }
        public double MaxFlooding { get; }
        public double Flooding { get; }
        public double TiltDegrees { get; }
        public string Seaworthiness { get; }
        public int SaveRevision { get; }
        public string LastEventId { get; }
        public IReadOnlyList<D22ShipDamageReadModel> DamageEntries { get; }

        internal D22ShipConditionReadModel(D21ShipConditionState source)
        {
            HasProjection = source != null;
            ShipId = source == null ? string.Empty : source.ShipId ?? string.Empty;
            MaxDurability = source == null ? 0d : source.MaxDurability;
            Durability = source == null ? 0d : source.Durability;
            MaxFlooding = source == null ? 0d : source.MaxFlooding;
            Flooding = source == null ? 0d : source.Flooding;
            TiltDegrees = source == null ? 0d : source.TiltDegrees;
            Seaworthiness = source == null ? string.Empty : source.Seaworthiness.ToString();
            SaveRevision = source == null ? 0 : source.SaveRevision;
            LastEventId = source == null ? string.Empty : source.LastEventId ?? string.Empty;
            var damages = new List<D22ShipDamageReadModel>();
            if (source != null && source.DamageEntries != null)
                foreach (var damage in source.DamageEntries)
                    if (damage != null) damages.Add(new D22ShipDamageReadModel(damage));
            damages.Sort(CompareDamage);
            DamageEntries = new ReadOnlyCollection<D22ShipDamageReadModel>(damages);
        }

        private D22ShipConditionReadModel(
            bool hasProjection,
            string shipId,
            double maxDurability,
            double durability,
            double maxFlooding,
            double flooding,
            double tiltDegrees,
            string seaworthiness,
            int saveRevision,
            string lastEventId,
            IEnumerable<D22ShipDamageReadModel> damageEntries)
        {
            HasProjection = hasProjection;
            ShipId = shipId ?? string.Empty;
            MaxDurability = maxDurability;
            Durability = durability;
            MaxFlooding = maxFlooding;
            Flooding = flooding;
            TiltDegrees = tiltDegrees;
            Seaworthiness = seaworthiness ?? string.Empty;
            SaveRevision = saveRevision;
            LastEventId = lastEventId ?? string.Empty;
            var damages = new List<D22ShipDamageReadModel>();
            if (damageEntries != null)
                foreach (var damage in damageEntries)
                    if (damage != null) damages.Add(damage.Clone());
            damages.Sort(CompareDamage);
            DamageEntries = new ReadOnlyCollection<D22ShipDamageReadModel>(damages);
        }

        public D22ShipConditionReadModel Clone()
        {
            return new D22ShipConditionReadModel(
                HasProjection, ShipId, MaxDurability, Durability, MaxFlooding, Flooding,
                TiltDegrees, Seaworthiness, SaveRevision, LastEventId, DamageEntries);
        }

        private static int CompareDamage(D22ShipDamageReadModel left, D22ShipDamageReadModel right)
        {
            int result = D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.DamageId,
                right == null ? null : right.DamageId);
            if (result != 0) return result;
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.ComponentId,
                right == null ? null : right.ComponentId);
        }
    }

    /// <summary>航线航点只读投影。</summary>
    [Serializable]
    public sealed class D22ShipWaypointReadModel
    {
        public string WaypointId { get; }
        public string LocationRef { get; }
        public int Sequence { get; }
        public double DistanceFromOriginNm { get; }
        public double MinimumDepthMeters { get; }
        public bool DiscoveryRequired { get; }
        public IReadOnlyList<string> HazardZoneIds { get; }

        internal D22ShipWaypointReadModel(D21RouteWaypoint source)
        {
            WaypointId = source == null ? string.Empty : source.WaypointId ?? string.Empty;
            LocationRef = source == null ? string.Empty : source.LocationRef ?? string.Empty;
            Sequence = source == null ? 0 : source.Sequence;
            DistanceFromOriginNm = source == null ? 0d : source.DistanceFromOriginNm;
            MinimumDepthMeters = source == null ? 0d : source.MinimumDepthMeters;
            DiscoveryRequired = source != null && source.DiscoveryRequired;
            HazardZoneIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(
                source == null ? null : source.HazardZoneIds));
        }

        private D22ShipWaypointReadModel(
            string waypointId,
            string locationRef,
            int sequence,
            double distanceFromOriginNm,
            double minimumDepthMeters,
            bool discoveryRequired,
            IEnumerable<string> hazardZoneIds)
        {
            WaypointId = waypointId ?? string.Empty;
            LocationRef = locationRef ?? string.Empty;
            Sequence = sequence;
            DistanceFromOriginNm = distanceFromOriginNm;
            MinimumDepthMeters = minimumDepthMeters;
            DiscoveryRequired = discoveryRequired;
            HazardZoneIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(hazardZoneIds));
        }

        public D22ShipWaypointReadModel Clone()
        {
            return new D22ShipWaypointReadModel(
                WaypointId, LocationRef, Sequence, DistanceFromOriginNm, MinimumDepthMeters,
                DiscoveryRequired, HazardZoneIds);
        }
    }

    /// <summary>航线危险区只读投影。</summary>
    [Serializable]
    public sealed class D22ShipHazardReadModel
    {
        public string HazardZoneId { get; }
        public string Kind { get; }
        public double Severity { get; }
        public bool DiscoveryRequired { get; }
        public string AlternativeRouteId { get; }

        internal D22ShipHazardReadModel(D21HazardZoneDefinition source)
        {
            HazardZoneId = source == null ? string.Empty : source.HazardZoneId ?? string.Empty;
            Kind = source == null ? string.Empty : source.Kind.ToString();
            Severity = source == null ? 0d : source.Severity;
            DiscoveryRequired = source != null && source.DiscoveryRequired;
            AlternativeRouteId = source == null ? string.Empty : source.AlternativeRouteId ?? string.Empty;
        }

        private D22ShipHazardReadModel(string hazardZoneId, string kind, double severity, bool discoveryRequired, string alternativeRouteId)
        {
            HazardZoneId = hazardZoneId ?? string.Empty;
            Kind = kind ?? string.Empty;
            Severity = severity;
            DiscoveryRequired = discoveryRequired;
            AlternativeRouteId = alternativeRouteId ?? string.Empty;
        }

        public D22ShipHazardReadModel Clone()
        {
            return new D22ShipHazardReadModel(HazardZoneId, Kind, Severity, DiscoveryRequired, AlternativeRouteId);
        }
    }

    /// <summary>航线只读投影。</summary>
    [Serializable]
    public sealed class D22ShipRouteReadModel
    {
        public bool HasProjection { get; }
        public string RouteId { get; }
        public string OriginId { get; }
        public string DestinationId { get; }
        public double TotalDistanceNm { get; }
        public double MinimumSafeDepthMeters { get; }
        public string RequiredDiscoveryState { get; }
        public IReadOnlyList<D22ShipWaypointReadModel> Waypoints { get; }
        public IReadOnlyList<D22ShipHazardReadModel> Hazards { get; }

        internal D22ShipRouteReadModel(D21NavigationRouteDefinition source)
        {
            HasProjection = source != null;
            RouteId = source == null ? string.Empty : source.RouteId ?? string.Empty;
            OriginId = source == null ? string.Empty : source.OriginId ?? string.Empty;
            DestinationId = source == null ? string.Empty : source.DestinationId ?? string.Empty;
            TotalDistanceNm = source == null ? 0d : source.TotalDistanceNm;
            MinimumSafeDepthMeters = source == null ? 0d : source.MinimumSafeDepthMeters;
            RequiredDiscoveryState = source == null ? string.Empty : source.RequiredDiscoveryState.ToString();
            var waypoints = new List<D22ShipWaypointReadModel>();
            if (source != null && source.Waypoints != null)
                foreach (var waypoint in source.Waypoints)
                    if (waypoint != null) waypoints.Add(new D22ShipWaypointReadModel(waypoint));
            waypoints.Sort(CompareWaypoints);
            Waypoints = new ReadOnlyCollection<D22ShipWaypointReadModel>(waypoints);
            var hazards = new List<D22ShipHazardReadModel>();
            if (source != null && source.Hazards != null)
                foreach (var hazard in source.Hazards)
                    if (hazard != null) hazards.Add(new D22ShipHazardReadModel(hazard));
            hazards.Sort(CompareHazards);
            Hazards = new ReadOnlyCollection<D22ShipHazardReadModel>(hazards);
        }

        private D22ShipRouteReadModel(
            bool hasProjection,
            string routeId,
            string originId,
            string destinationId,
            double totalDistanceNm,
            double minimumSafeDepthMeters,
            string requiredDiscoveryState,
            IEnumerable<D22ShipWaypointReadModel> waypoints,
            IEnumerable<D22ShipHazardReadModel> hazards)
        {
            HasProjection = hasProjection;
            RouteId = routeId ?? string.Empty;
            OriginId = originId ?? string.Empty;
            DestinationId = destinationId ?? string.Empty;
            TotalDistanceNm = totalDistanceNm;
            MinimumSafeDepthMeters = minimumSafeDepthMeters;
            RequiredDiscoveryState = requiredDiscoveryState ?? string.Empty;
            var copiedWaypoints = new List<D22ShipWaypointReadModel>();
            if (waypoints != null)
                foreach (var waypoint in waypoints)
                    if (waypoint != null) copiedWaypoints.Add(waypoint.Clone());
            copiedWaypoints.Sort(CompareWaypoints);
            Waypoints = new ReadOnlyCollection<D22ShipWaypointReadModel>(copiedWaypoints);
            var copiedHazards = new List<D22ShipHazardReadModel>();
            if (hazards != null)
                foreach (var hazard in hazards)
                    if (hazard != null) copiedHazards.Add(hazard.Clone());
            copiedHazards.Sort(CompareHazards);
            Hazards = new ReadOnlyCollection<D22ShipHazardReadModel>(copiedHazards);
        }

        public D22ShipRouteReadModel Clone()
        {
            return new D22ShipRouteReadModel(
                HasProjection, RouteId, OriginId, DestinationId, TotalDistanceNm,
                MinimumSafeDepthMeters, RequiredDiscoveryState, Waypoints, Hazards);
        }

        private static int CompareWaypoints(D22ShipWaypointReadModel left, D22ShipWaypointReadModel right)
        {
            int result = (left == null ? 0 : left.Sequence).CompareTo(right == null ? 0 : right.Sequence);
            if (result != 0) return result;
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.WaypointId,
                right == null ? null : right.WaypointId);
        }

        private static int CompareHazards(D22ShipHazardReadModel left, D22ShipHazardReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.HazardZoneId,
                right == null ? null : right.HazardZoneId);
        }
    }

    /// <summary>天气影响只读投影。</summary>
    [Serializable]
    public sealed class D22ShipWeatherReadModel
    {
        public bool HasProjection { get; }
        public bool Success { get; }
        public string FailureCode { get; }
        public string SeaState { get; }
        public float HullRisk { get; }
        public float SpeedMultiplier { get; }
        public float ProvisionConsumptionMultiplier { get; }
        public float WaterConsumptionMultiplier { get; }
        public float FoodConsumptionMultiplier { get; }
        public float FuelConsumptionMultiplier { get; }
        public int CrewFatiguePerDay { get; }
        public float CrewMoraleDelta { get; }
        public float CrewHealthRisk { get; }
        public float FishingActivityMultiplier { get; }
        public float FishingCatchMultiplier { get; }
        public bool FishingAllowed { get; }
        public bool PortAvailable { get; }
        public string PortAvailabilityCode { get; }

        internal D22ShipWeatherReadModel(D21WeatherImpactResult source)
        {
            HasProjection = source != null;
            Success = source != null && source.Success;
            FailureCode = source == null ? string.Empty : source.FailureCode ?? string.Empty;
            SeaState = source == null ? string.Empty : source.SeaState.ToString();
            HullRisk = source == null ? 0f : source.HullRisk;
            SpeedMultiplier = source == null ? 0f : source.SpeedMultiplier;
            ProvisionConsumptionMultiplier = source == null ? 0f : source.ProvisionConsumptionMultiplier;
            WaterConsumptionMultiplier = source == null ? 0f : source.WaterConsumptionMultiplier;
            FoodConsumptionMultiplier = source == null ? 0f : source.FoodConsumptionMultiplier;
            FuelConsumptionMultiplier = source == null ? 0f : source.FuelConsumptionMultiplier;
            CrewFatiguePerDay = source == null ? 0 : source.CrewFatiguePerDay;
            CrewMoraleDelta = source == null ? 0f : source.CrewMoraleDelta;
            CrewHealthRisk = source == null ? 0f : source.CrewHealthRisk;
            FishingActivityMultiplier = source == null ? 0f : source.FishingActivityMultiplier;
            FishingCatchMultiplier = source == null ? 0f : source.FishingCatchMultiplier;
            FishingAllowed = source != null && source.FishingAllowed;
            PortAvailable = source != null && source.PortAvailable;
            PortAvailabilityCode = source == null ? string.Empty : source.PortAvailabilityCode ?? string.Empty;
        }

        private D22ShipWeatherReadModel(
            bool hasProjection,
            bool success,
            string failureCode,
            string seaState,
            float hullRisk,
            float speedMultiplier,
            float provisionConsumptionMultiplier,
            float waterConsumptionMultiplier,
            float foodConsumptionMultiplier,
            float fuelConsumptionMultiplier,
            int crewFatiguePerDay,
            float crewMoraleDelta,
            float crewHealthRisk,
            float fishingActivityMultiplier,
            float fishingCatchMultiplier,
            bool fishingAllowed,
            bool portAvailable,
            string portAvailabilityCode)
        {
            HasProjection = hasProjection;
            Success = success;
            FailureCode = failureCode ?? string.Empty;
            SeaState = seaState ?? string.Empty;
            HullRisk = hullRisk;
            SpeedMultiplier = speedMultiplier;
            ProvisionConsumptionMultiplier = provisionConsumptionMultiplier;
            WaterConsumptionMultiplier = waterConsumptionMultiplier;
            FoodConsumptionMultiplier = foodConsumptionMultiplier;
            FuelConsumptionMultiplier = fuelConsumptionMultiplier;
            CrewFatiguePerDay = crewFatiguePerDay;
            CrewMoraleDelta = crewMoraleDelta;
            CrewHealthRisk = crewHealthRisk;
            FishingActivityMultiplier = fishingActivityMultiplier;
            FishingCatchMultiplier = fishingCatchMultiplier;
            FishingAllowed = fishingAllowed;
            PortAvailable = portAvailable;
            PortAvailabilityCode = portAvailabilityCode ?? string.Empty;
        }

        public D22ShipWeatherReadModel Clone()
        {
            return new D22ShipWeatherReadModel(
                HasProjection, Success, FailureCode, SeaState, HullRisk, SpeedMultiplier,
                ProvisionConsumptionMultiplier, WaterConsumptionMultiplier, FoodConsumptionMultiplier,
                FuelConsumptionMultiplier, CrewFatiguePerDay, CrewMoraleDelta, CrewHealthRisk,
                FishingActivityMultiplier, FishingCatchMultiplier, FishingAllowed, PortAvailable,
                PortAvailabilityCode);
        }
    }

    /// <summary>航程补给账本只读投影。</summary>
    [Serializable]
    public sealed class D22ShipProvisionReadModel
    {
        public bool HasProjection { get; }
        public string VoyageId { get; }
        public string ShipId { get; }
        public int LastTickIndex { get; }
        public int FoodRemaining { get; }
        public int WaterRemaining { get; }
        public int TotalFoodConsumed { get; }
        public int TotalWaterConsumed { get; }
        public int TotalFoodLoss { get; }
        public int TotalWaterLoss { get; }
        public int HungerThresholdUnits { get; }
        public int ThirstThresholdUnits { get; }
        public bool IsFailed { get; }
        public string FailureCode { get; }
        public IReadOnlyList<string> AppliedApplicationIds { get; }

        internal D22ShipProvisionReadModel(D21ProvisionLedgerSnapshot source)
        {
            HasProjection = source != null;
            VoyageId = source == null ? string.Empty : source.VoyageId ?? string.Empty;
            ShipId = source == null ? string.Empty : source.ShipId ?? string.Empty;
            LastTickIndex = source == null ? 0 : source.LastTickIndex;
            FoodRemaining = source == null ? 0 : source.FoodRemaining;
            WaterRemaining = source == null ? 0 : source.WaterRemaining;
            TotalFoodConsumed = source == null ? 0 : source.TotalFoodConsumed;
            TotalWaterConsumed = source == null ? 0 : source.TotalWaterConsumed;
            TotalFoodLoss = source == null ? 0 : source.TotalFoodLoss;
            TotalWaterLoss = source == null ? 0 : source.TotalWaterLoss;
            HungerThresholdUnits = source == null ? 0 : source.HungerThresholdUnits;
            ThirstThresholdUnits = source == null ? 0 : source.ThirstThresholdUnits;
            IsFailed = source != null && source.IsFailed;
            FailureCode = source == null ? string.Empty : source.FailureCode ?? string.Empty;
            AppliedApplicationIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(
                source == null ? null : source.AppliedApplicationIds));
        }

        private D22ShipProvisionReadModel(
            bool hasProjection,
            string voyageId,
            string shipId,
            int lastTickIndex,
            int foodRemaining,
            int waterRemaining,
            int totalFoodConsumed,
            int totalWaterConsumed,
            int totalFoodLoss,
            int totalWaterLoss,
            int hungerThresholdUnits,
            int thirstThresholdUnits,
            bool isFailed,
            string failureCode,
            IEnumerable<string> appliedApplicationIds)
        {
            HasProjection = hasProjection;
            VoyageId = voyageId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            LastTickIndex = lastTickIndex;
            FoodRemaining = foodRemaining;
            WaterRemaining = waterRemaining;
            TotalFoodConsumed = totalFoodConsumed;
            TotalWaterConsumed = totalWaterConsumed;
            TotalFoodLoss = totalFoodLoss;
            TotalWaterLoss = totalWaterLoss;
            HungerThresholdUnits = hungerThresholdUnits;
            ThirstThresholdUnits = thirstThresholdUnits;
            IsFailed = isFailed;
            FailureCode = failureCode ?? string.Empty;
            AppliedApplicationIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(appliedApplicationIds));
        }

        public D22ShipProvisionReadModel Clone()
        {
            return new D22ShipProvisionReadModel(
                HasProjection, VoyageId, ShipId, LastTickIndex, FoodRemaining, WaterRemaining,
                TotalFoodConsumed, TotalWaterConsumed, TotalFoodLoss, TotalWaterLoss,
                HungerThresholdUnits, ThirstThresholdUnits, IsFailed, FailureCode, AppliedApplicationIds);
        }
    }

    /// <summary>D21 航程结果的只读反馈摘要。</summary>
    [Serializable]
    public sealed class D22ShipResultReadModel
    {
        public bool HasResult { get; }
        public bool Accepted { get; }
        public bool IsDuplicate { get; }
        public bool Applied { get; }
        public bool IsArrived { get; }
        public bool VoyageFailed { get; }
        public bool IsInterrupted { get; }
        public string Status { get; }
        public string ErrorCode { get; }
        public string SourceErrorCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }
        public D22AdapterOutcomeReadModel Outcome { get; }

        private D22ShipResultReadModel(
            bool hasResult,
            bool accepted,
            bool isDuplicate,
            bool applied,
            bool isArrived,
            bool voyageFailed,
            bool isInterrupted,
            string status,
            string errorCode,
            string sourceErrorCode,
            string localizationKey,
            string sourceLocalizationKey,
            D22AdapterOutcomeReadModel outcome)
        {
            HasResult = hasResult;
            Accepted = accepted;
            IsDuplicate = isDuplicate;
            Applied = applied;
            IsArrived = isArrived;
            VoyageFailed = voyageFailed;
            IsInterrupted = isInterrupted;
            Status = status ?? string.Empty;
            ErrorCode = errorCode ?? string.Empty;
            SourceErrorCode = sourceErrorCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
            Outcome = outcome == null ? null : outcome.Clone();
        }

        internal D22ShipResultReadModel(D21VoyageTimeResult source, D22AdapterOutcomeReadModel outcome)
            : this(
                source != null,
                source != null && source.Accepted,
                source != null && source.IsDuplicate,
                source != null && source.Accepted,
                source != null && source.IsArrived,
                source != null && !source.Accepted,
                false,
                source == null ? string.Empty : (source.Accepted ? "accepted" : "rejected"),
                source == null ? string.Empty : source.Code ?? string.Empty,
                source == null ? string.Empty : source.Code ?? string.Empty,
                D22ReadModelAdapterSupport.Key("ship", source != null && source.Accepted ? "result_succeeded" : "result_failed"),
                string.Empty,
                outcome)
        {
        }

        public D22ShipResultReadModel Clone()
        {
            return new D22ShipResultReadModel(
                HasResult, Accepted, IsDuplicate, Applied, IsArrived, VoyageFailed, IsInterrupted,
                Status, ErrorCode, SourceErrorCode, LocalizationKey, SourceLocalizationKey, Outcome);
        }
    }

    /// <summary>D22-L19 船舶 UI 读模型。</summary>
    [Serializable]
    public sealed class D22ShipReadModel : D22ReadModelAdapterModel
    {
        public string ShipId { get; }
        public string ShipName { get; }
        public string ShipTypeId { get; }
        public string OperationalState { get; }
        public string VoyageId { get; }
        public string VoyageStatus { get; }
        public string RouteId { get; }
        public string OriginId { get; }
        public string DestinationId { get; }
        public string CurrentWaypointId { get; }
        public long VoyageSaveRevision { get; }
        public bool IsArrived { get; }
        public long RouteDistanceMilliNm { get; }
        public long DistanceTravelledMilliNm { get; }
        public long RemainingDistanceMilliNm { get; }
        public D22ShipConditionReadModel Condition { get; }
        public D22ShipRouteReadModel Route { get; }
        public D22ShipWeatherReadModel Weather { get; }
        public D22ShipProvisionReadModel Provisions { get; }
        public D22ShipResultReadModel LastVoyageResult { get; }

        private D22ShipReadModel(
            D22ReadModelAdapterMetadata metadata,
            string shipId,
            string shipName,
            string shipTypeId,
            string operationalState,
            string voyageId,
            string voyageStatus,
            string routeId,
            string originId,
            string destinationId,
            string currentWaypointId,
            long voyageSaveRevision,
            bool isArrived,
            long routeDistanceMilliNm,
            long distanceTravelledMilliNm,
            long remainingDistanceMilliNm,
            D22ShipConditionReadModel condition,
            D22ShipRouteReadModel route,
            D22ShipWeatherReadModel weather,
            D22ShipProvisionReadModel provisions,
            D22ShipResultReadModel lastVoyageResult)
            : base(metadata)
        {
            ShipId = shipId ?? string.Empty;
            ShipName = shipName ?? string.Empty;
            ShipTypeId = shipTypeId ?? string.Empty;
            OperationalState = operationalState ?? string.Empty;
            VoyageId = voyageId ?? string.Empty;
            VoyageStatus = voyageStatus ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            OriginId = originId ?? string.Empty;
            DestinationId = destinationId ?? string.Empty;
            CurrentWaypointId = currentWaypointId ?? string.Empty;
            VoyageSaveRevision = voyageSaveRevision;
            IsArrived = isArrived;
            RouteDistanceMilliNm = routeDistanceMilliNm;
            DistanceTravelledMilliNm = distanceTravelledMilliNm;
            RemainingDistanceMilliNm = remainingDistanceMilliNm;
            Condition = condition == null ? null : condition.Clone();
            Route = route == null ? null : route.Clone();
            Weather = weather == null ? null : weather.Clone();
            Provisions = provisions == null ? null : provisions.Clone();
            LastVoyageResult = lastVoyageResult == null ? null : lastVoyageResult.Clone();
        }

        public D22ShipReadModel Clone()
        {
            return new D22ShipReadModel(
                CopyMetadata(), ShipId, ShipName, ShipTypeId, OperationalState, VoyageId, VoyageStatus,
                RouteId, OriginId, DestinationId, CurrentWaypointId, VoyageSaveRevision, IsArrived,
                RouteDistanceMilliNm, DistanceTravelledMilliNm, RemainingDistanceMilliNm, Condition,
                Route, Weather, Provisions, LastVoyageResult);
        }

        public static D22ShipReadModel Project(D22ShipOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d21.ship");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.ship.snapshot_missing", D22ReadModelAdapterSupport.Key("ship", "blocked"),
                    "d22.ship.snapshot_missing", "D21", "d21.ship", true));
                return Empty(issues, missing);
            }
            if (snapshot.SchemaVersion != D22ShipOwnerSnapshot.CurrentSchemaVersion)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.ship.schema_version_mismatch", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                    "d22.ship.schema_version_mismatch", "D22ShipOwnerSnapshot", "d22.ship", true));
            }
            if (snapshot.SourceSchemaVersion <= 0)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.ship.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                    "d22.ship.source_schema_version_invalid", "D21", "d21.ship", true));
            }

            var identity = snapshot.Identity;
            var condition = snapshot.ConditionState;
            if (condition == null && snapshot.LastConditionResult != null)
                condition = snapshot.LastConditionResult.State;
            var route = snapshot.Route;
            var weather = snapshot.WeatherImpact;
            var time = snapshot.Time;
            if (time == null && snapshot.LastTimeResult != null)
                time = snapshot.LastTimeResult.State;
            var provision = snapshot.ProvisionLedger;
            if (provision == null && snapshot.LastProvisionResult != null)
                provision = snapshot.LastProvisionResult.Ledger;

            string shipId = identity == null ? string.Empty : identity.ShipId ?? string.Empty;
            if (string.IsNullOrEmpty(shipId) && time != null) shipId = time.ShipId ?? string.Empty;
            if (!D22ReadModelAdapterSupport.IsStableId(shipId))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_ship_id", "d22.ship.ship_id_missing", D22ReadModelAdapterSupport.Key("ship", "blocked"),
                    "d22.ship.ship_id_missing", "D21", "d21.ship.identity", true));
            }
            if (identity != null)
            {
                if (identity.SchemaVersion != D21ShipIdentityContracts.CurrentSchemaVersion)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_identity_schema", "d22.ship.identity_schema_invalid", D22ReadModelAdapterSupport.Key("ship", "partial"),
                        "d22.ship.identity_schema_invalid", "D21", "d21.ship.identity", false));
                ValidateId(identity.ShipTypeId, "ship_type_id_invalid", "d21.ship.identity", issues, true);
                ValidateId(identity.OwnerId, "owner_id_invalid", "d21.ship.identity", issues, true);
                ValidateId(identity.HomePortId, "home_port_id_invalid", "d21.ship.identity", issues, true);
            }
            else
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_identity_projection", "d22.ship.identity_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.identity_missing", "D21", "d21.ship.identity", false));
            }

            if (condition == null)
            {
                missing.Add("d21.ship.condition");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_condition_projection", "d22.ship.condition_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.condition_missing", "D21", "d21.ship.condition", false));
            }
            else
            {
                if (condition.SchemaVersion != D21ShipConditionSystem.CurrentSchemaVersion ||
                    condition.MaxDurability < 0d || condition.Durability < 0d || condition.Durability > condition.MaxDurability ||
                    condition.MaxFlooding < 0d || condition.Flooding < 0d || condition.Flooding > condition.MaxFlooding)
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_condition_projection", "d22.ship.condition_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                        "d22.ship.condition_invalid", "D21", "d21.ship.condition", false));
                }
            }

            if (route == null)
            {
                missing.Add("d21.ship.route");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_route_projection", "d22.ship.route_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.route_missing", "D21", "d21.ship.route", false));
            }
            else
            {
                if (route.SchemaVersion != D21NavigationRouteContracts.CurrentSchemaVersion || route.TotalDistanceNm < 0d)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_route_projection", "d22.ship.route_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                        "d22.ship.route_invalid", "D21", "d21.ship.route", false));
                ValidateId(route.RouteId, "route_id_invalid", "d21.ship.route", issues);
                ValidateId(route.OriginId, "origin_id_invalid", "d21.ship.route", issues, true);
                ValidateId(route.DestinationId, "destination_id_invalid", "d21.ship.route", issues, true);
                ValidateRouteItems(route, issues);
            }

            if (weather == null)
            {
                missing.Add("d21.ship.weather");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_weather_projection", "d22.ship.weather_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.weather_missing", "D21", "d21.ship.weather", false));
            }
            else
            {
                if (weather.SchemaVersion != D21WeatherInputContracts.CurrentSchemaVersion)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_weather_projection", "d22.ship.weather_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                        "d22.ship.weather_invalid", "D21", "d21.ship.weather", false));
                D22ReadModelAdapterSupport.ValidateSourceCode(weather.FailureCode, "D21", "d21.ship.weather", issues);
            }

            if (time == null)
            {
                missing.Add("d21.ship.time");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_time_projection", "d22.ship.time_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.time_missing", "D21", "d21.ship.time", false));
            }
            else
            {
                if (time.SchemaVersion != D21VoyageTimeState.CurrentSchemaVersion ||
                    time.RouteDistanceMilliNm < 0 || time.DistanceTravelledMilliNm < 0 ||
                    time.DistanceTravelledMilliNm > time.RouteDistanceMilliNm)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_time_projection", "d22.ship.time_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                        "d22.ship.time_invalid", "D21", "d21.ship.time", false));
            }

            if (provision == null)
            {
                missing.Add("d21.ship.provisions");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_provision_projection", "d22.ship.provisions_missing", D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship.provisions_missing", "D21", "d21.ship.provisions", false));
            }
            else
            {
                if (provision.SchemaVersion != D21ProvisionConsumptionRequest.CurrentSchemaVersion ||
                    provision.FoodRemaining < 0 || provision.WaterRemaining < 0 || provision.LastTickIndex < 0)
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_provision_projection", "d22.ship.provisions_invalid", D22ReadModelAdapterSupport.Key("ship", "invalid"),
                        "d22.ship.provisions_invalid", "D21", "d21.ship.provisions", false));
                D22ReadModelAdapterSupport.ValidateSourceCode(provision.FailureCode, "D21", "d21.ship.provisions", issues);
            }

            long revision = snapshot.HasSaveRevision ? snapshot.SaveRevision :
                (identity == null ? (time == null ? 0 : time.LastSaveRevision) : identity.SaveRevision);
            int worldDay = snapshot.HasWorldDay ? snapshot.WorldDay : (time == null ? 0 : time.StartWorldDay);
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(revision, worldDay, "D21", "d21.ship", issues);

            D22AdapterOutcomeReadModel outcome = null;
            string sourceError = string.Empty;
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (snapshot.LastTimeResult != null)
            {
                sourceError = snapshot.LastTimeResult.Code ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D21", "d21.ship.result", issues);
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D21", "d21.ship.result", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22ShipOwnerSnapshot", "d21.ship.result", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "ship", "D21", true, snapshot.LastTimeResult.Accepted,
                    snapshot.LastTimeResult.IsDuplicate, sourceError, sourceReason, sourceLoc,
                    snapshot.LastTimeResult.Accepted ? string.Empty :
                        (string.IsNullOrEmpty(sourceReason) ? "d22.ship.voyage_rejected" : sourceReason), issues);
            }
            else if (snapshot.LastConditionResult != null && snapshot.LastConditionResult.ReasonCode != D21ShipConditionFailureCode.None)
            {
                sourceError = StableEnumCode(snapshot.LastConditionResult.ReasonCode);
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D21", "d21.ship.condition_result", issues);
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D21", "d21.ship.condition_result", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "ship", "D21", true, snapshot.LastConditionResult.Accepted,
                    snapshot.LastConditionResult.Duplicate, sourceError, sourceReason, sourceLoc,
                    snapshot.LastConditionResult.Accepted ? string.Empty : sourceReason, issues);
            }
            else if (snapshot.LastProvisionResult != null && !snapshot.LastProvisionResult.Success)
            {
                sourceError = snapshot.LastProvisionResult.FailureCode ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D21", "d21.ship.provision_result", issues);
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D21", "d21.ship.provision_result", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "ship", "D21", true, false, snapshot.LastProvisionResult.IsDuplicate,
                    sourceError, sourceReason, sourceLoc, sourceReason, issues);
            }
            else if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22ShipOwnerSnapshot", "d21.ship", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22ShipOwnerSnapshot", "d21.ship", issues);
                outcome = D22ReadModelAdapterSupport.Outcome(
                    "ship", "D22ShipOwnerSnapshot", true, false, false, sourceReason, sourceReason, sourceLoc, sourceReason, issues);
            }

            string voyageId = PreferValue(snapshot.VoyageId, time == null ? string.Empty : time.VoyageId);
            string voyageStatus = snapshot.VoyageStatus ?? string.Empty;
            string routeId = PreferValue(snapshot.RouteId, route == null ? string.Empty : route.RouteId);
            string originId = PreferValue(snapshot.OriginId, route == null ? string.Empty : route.OriginId);
            string destinationId = PreferValue(snapshot.DestinationId, route == null ? string.Empty : route.DestinationId);
            string currentWaypointId = snapshot.CurrentWaypointId ?? string.Empty;
            ValidateId(voyageId, "voyage_id_invalid", "d21.ship.voyage", issues, true);
            ValidateId(routeId, "route_id_invalid", "d21.ship.voyage", issues, true);
            ValidateId(originId, "origin_id_invalid", "d21.ship.voyage", issues, true);
            ValidateId(destinationId, "destination_id_invalid", "d21.ship.voyage", issues, true);
            ValidateId(currentWaypointId, "waypoint_id_invalid", "d21.ship.voyage", issues, true);

            long routeDistance = time == null ? 0 : time.RouteDistanceMilliNm;
            long travelled = time == null ? 0 : time.DistanceTravelledMilliNm;
            long remaining = routeDistance >= travelled ? routeDistance - travelled : 0;
            bool arrived = time != null && time.IsArrived;
            if (snapshot.LastTimeResult != null) arrived = snapshot.LastTimeResult.IsArrived;
            var status = D22ReadModelAdapterSupport.ResolveStatus(
                snapshot.SchemaVersion != D22ShipOwnerSnapshot.CurrentSchemaVersion,
                identity == null && condition == null && route == null && weather == null && time == null && provision == null,
                issues.Count > 0,
                issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "ship", "D21", snapshot.SourceSchemaVersion, revision, worldDay, status,
                sourceError, sourceError, sourceReason,
                string.IsNullOrEmpty(sourceReason) ? string.Empty : sourceReason,
                sourceLoc, MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22ShipReadModel(
                metadata,
                shipId,
                identity == null ? string.Empty : identity.ShipName,
                identity == null ? string.Empty : identity.ShipTypeId,
                identity == null ? string.Empty : identity.OperationalState.ToString(),
                voyageId,
                voyageStatus,
                routeId,
                originId,
                destinationId,
                currentWaypointId,
                revision,
                arrived,
                routeDistance,
                travelled,
                remaining,
                new D22ShipConditionReadModel(condition),
                new D22ShipRouteReadModel(route),
                new D22ShipWeatherReadModel(weather),
                new D22ShipProvisionReadModel(provision),
                snapshot.LastTimeResult == null ? null : new D22ShipResultReadModel(snapshot.LastTimeResult, outcome));
        }

        public static D22ShipReadModel Adapt(D22ShipOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22ShipReadModel FromSnapshot(D22ShipOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22ShipReadModel Empty(List<D22ReadModelAdapterIssue> issues, List<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "ship", "D21", 0, 0, 0, D22ReadModelAdapterStatus.Blocked,
                "d22.ship.source_missing", string.Empty, "d22.ship.source_missing",
                "d22.ship.source_missing", string.Empty, missing, issues);
            return new D22ShipReadModel(
                metadata, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, 0, false, 0, 0, 0,
                new D22ShipConditionReadModel(null), new D22ShipRouteReadModel(null),
                new D22ShipWeatherReadModel(null), new D22ShipProvisionReadModel(null), null);
        }

        private static void ValidateRouteItems(
            D21NavigationRouteDefinition route,
            List<D22ReadModelAdapterIssue> issues)
        {
            var waypointIds = new HashSet<string>(StringComparer.Ordinal);
            if (route.Waypoints != null)
            {
                foreach (var waypoint in route.Waypoints)
                {
                    if (waypoint == null || !D22ReadModelAdapterSupport.IsStableId(waypoint.WaypointId) ||
                        !waypointIds.Add(waypoint.WaypointId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_route_waypoint", "d22.ship.waypoint_invalid", D22ReadModelAdapterSupport.Key("ship", "partial"),
                            "d22.ship.waypoint_invalid", "D21", "d21.ship.route", false));
                    }
                    else
                    {
                        ValidateId(waypoint.LocationRef, "location_ref_invalid", "d21.ship.route", issues, true);
                    }
                }
            }
            var hazardIds = new HashSet<string>(StringComparer.Ordinal);
            if (route.Hazards != null)
            {
                foreach (var hazard in route.Hazards)
                {
                    if (hazard == null || !D22ReadModelAdapterSupport.IsStableId(hazard.HazardZoneId) ||
                        !hazardIds.Add(hazard.HazardZoneId) || hazard.Severity < 0d)
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_route_hazard", "d22.ship.hazard_invalid", D22ReadModelAdapterSupport.Key("ship", "partial"),
                            "d22.ship.hazard_invalid", "D21", "d21.ship.route", false));
                    }
                }
            }
        }

        private static void ValidateId(
            string value,
            string code,
            string moduleId,
            List<D22ReadModelAdapterIssue> issues,
            bool allowEmpty = false)
        {
            if (!D22ReadModelAdapterSupport.IsStableId(value, allowEmpty))
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    code, "d22.ship." + code, D22ReadModelAdapterSupport.Key("ship", "partial"),
                    "d22.ship." + code, "D21", moduleId, false));
            }
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static string PreferValue(string preferred, string fallback)
        {
            return string.IsNullOrEmpty(preferred) ? (fallback ?? string.Empty) : preferred;
        }

        private static string StableEnumCode(Enum value)
        {
            return value == null ? string.Empty : value.ToString().ToLowerInvariant();
        }
    }
}
