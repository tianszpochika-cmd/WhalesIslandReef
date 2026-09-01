// D21-L06/L24 D20 船员公开投影闭环纯逻辑测试。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D20;

namespace Whalebone.Systems.D21.Tests
{
    /// <summary>验证 D20 public projection 到 D21 Start 的真实只读闭环。</summary>
    public static class D21CrewProjectionIntegrationTests
    {
        /// <summary>运行全部断言并返回断言数量。</summary>
        public static int Run()
        {
            int assertions = 0;
            D20CrewOwnerRosterReadSnapshot owner = CreateOwnerSnapshot();
            D20CrewProjectionResult projectionResult =
                D20CrewProjectionAdapter.Project(owner);
            Check(ref assertions, projectionResult.Valid && projectionResult.Projection != null,
                "D20 owner projection must be available for D21 integration");

            D20CrewRosterProjection projection = projectionResult.Projection;
            D21CrewProjectionMappingResult mapping = D21CrewProjectionAdapter.Map(
                projection, projection.ShipId, projection.SaveRevision);
            Check(ref assertions, mapping.Valid && mapping.Roster != null,
                "valid D20 projection must map to a D21 roster");
            Check(ref assertions,
                mapping.Roster.CrewMembers[0].CrewId == "crew:projection:a" &&
                mapping.Roster.CrewMembers[1].CrewId == "crew:projection:z",
                "D21 mapped roster must preserve deterministic crew ordering");
            Check(ref assertions,
                mapping.Roster.SourceProjectionId == projection.SourceProjectionId &&
                mapping.SourceOwnerSystem == projection.SourceOwnerSystem,
                "D21 adapter must retain auditable D20 source references");
            D21CrewRole mappedRole;
            Check(ref assertions,
                D21CrewProjectionAdapter.TryMapRole("Helm", out mappedRole) &&
                mappedRole == D21CrewRole.Helmsman,
                "Helm must map uniquely to Helmsman");
            Check(ref assertions,
                D21CrewProjectionAdapter.TryMapRole("repair", out mappedRole) &&
                mappedRole == D21CrewRole.Carpenter,
                "Repair must map uniquely to Carpenter");
            Check(ref assertions,
                !D21CrewProjectionAdapter.TryMapRole("unknown-duty", out mappedRole) &&
                mappedRole == D21CrewRole.Unknown,
                "unknown role must not be silently accepted");

            D21CrewRequirementProfile profile = CreateProfile();
            D21CrewRequirementDecision decision = D21CrewRequirementRules.Evaluate(
                profile, mapping.Roster);
            Check(ref assertions,
                decision.Valid && decision.CanDepart &&
                decision.Eligibility == D21CrewEligibilityState.Eligible &&
                decision.Assignments.Count == 2,
                "D21 crew rules must evaluate the mapped public roster");
            Check(ref assertions,
                decision.Assignments[0].Role == D21CrewRole.Helmsman &&
                decision.Assignments[1].Role == D21CrewRole.Navigator,
                "D21 role assignment must be deterministic after mapping");

            D21VoyageCommandRequest publicRequest = CreateStartRequest(
                "voyage:projection:public", "command:projection:public", profile);
            publicRequest.D20CrewProjection = projection;
            // 该旧输入故意无效；D20 public projection 必须优先，不能被它绕过。
            publicRequest.CrewRoster = new D21CrewRosterSnapshot
            {
                SchemaVersion = D21CrewRequirementRules.CurrentSchemaVersion,
                ShipId = projection.ShipId,
                SaveRevision = 7,
                SourceProjectionId = "legacy:invalid",
                CrewMembers = null,
            };
            publicRequest.CrewDecision = new D21CrewRequirementDecision
            {
                Valid = false,
                CanDepart = true,
                RequiredCrewCount = 99,
            };
            D21VoyageCommandService publicService = new D21VoyageCommandService();
            D21VoyageCommandResult publicResult = publicService.Route(publicRequest);
            Check(ref assertions, publicResult.Accepted && publicResult.StartPlan != null,
                "D21 Start must consume a valid D20 projection and call crew rules");
            Check(ref assertions,
                publicResult.StartPlan.CrewSourceProjectionId == projection.SourceProjectionId &&
                publicResult.StartPlan.CrewSourceOwnerSystem == projection.SourceOwnerSystem &&
                publicResult.StartPlan.CrewDecision.ActiveCrewCount == 2,
                "Start plan must expose only read-only crew provenance and evaluated result");

            D20CrewRosterProjection unknownRoleProjection = projection.Clone();
            unknownRoleProjection.CrewMembers[0].QualifiedRoleIds.Add("unknown-duty");
            D21VoyageCommandRequest unknownRoleRequest = CreateStartRequest(
                "voyage:projection:unknown", "command:projection:unknown", profile);
            unknownRoleRequest.D20CrewProjection = unknownRoleProjection;
            unknownRoleRequest.CrewDecision = CreateValidDecision();
            D21VoyageCommandResult unknownRoleResult =
                new D21VoyageCommandService().Route(unknownRoleRequest);
            Check(ref assertions,
                !unknownRoleResult.Accepted &&
                unknownRoleResult.Code == D21VoyageCommandService.CrewProjectionInvalidCode &&
                Contains(unknownRoleResult.ValidationErrors, "d21.crew.role_unknown"),
                "unknown D20 role must reject D21 Start even with a legacy decision present");

            D21VoyageCommandRequest missingRequest = CreateStartRequest(
                "voyage:projection:missing", "command:projection:missing", profile);
            missingRequest.D20CrewProjection = null;
            missingRequest.CrewRoster = null;
            missingRequest.CrewDecision = null;
            D21VoyageCommandResult missingResult =
                new D21VoyageCommandService().Route(missingRequest);
            Check(ref assertions,
                !missingResult.Accepted && missingResult.DependencyBlocked &&
                missingResult.Code == D21VoyageCommandService.CrewProjectionRequiredCode,
                "missing crew projection must fail closed as a dependency block");

            D21VoyageCommandRequest directRequest = CreateStartRequest(
                "voyage:projection:direct", "command:projection:direct", profile);
            directRequest.D20CrewProjection = null;
            directRequest.CrewRoster = mapping.Roster.Clone();
            directRequest.CrewDecision = null;
            D21VoyageCommandResult directResult =
                new D21VoyageCommandService().Route(directRequest);
            Check(ref assertions,
                directResult.Accepted && directResult.StartPlan.CrewDecision.ActiveCrewCount == 2,
                "validated direct D21 roster must remain a compatible input path");

            D21VoyageCommandRequest invalidDirectRequest = directRequest.Clone();
            invalidDirectRequest.CommandId = "command:projection:direct-invalid";
            invalidDirectRequest.VoyageId = "voyage:projection:direct-invalid";
            invalidDirectRequest.CrewRoster.CrewMembers.Add(
                invalidDirectRequest.CrewRoster.CrewMembers[0].Clone());
            D21VoyageCommandResult invalidDirectResult =
                new D21VoyageCommandService().Route(invalidDirectRequest);
            Check(ref assertions,
                !invalidDirectResult.Accepted &&
                invalidDirectResult.Code == D21VoyageCommandService.CrewProjectionInvalidCode,
                "invalid direct D21 roster must not bypass projection validation");

            string sourceIdBefore = owner.SourceProjectionId;
            string sourceOwnerBefore = owner.SourceOwnerSystem;
            double moraleBefore = owner.CrewMembers[0].MoraleScore;
            D21CrewProjectionAdapter.Map(projection, owner.ShipId, owner.SaveRevision);
            Check(ref assertions,
                owner.SourceProjectionId == sourceIdBefore &&
                owner.SourceOwnerSystem == sourceOwnerBefore &&
                owner.CrewMembers[0].MoraleScore == moraleBefore,
                "D20/D21 adapters must not write D10/Crew owner data");
            return assertions;
        }

        private static D20CrewOwnerRosterReadSnapshot CreateOwnerSnapshot()
        {
            return new D20CrewOwnerRosterReadSnapshot
            {
                SchemaVersion = D20CrewProjectionContracts.CurrentSchemaVersion,
                ShipId = "ship:projection:01",
                SaveRevision = 7,
                SourceProjectionId = "d10:crew:projection:1",
                SourceOwnerSystem = "D10CrewBehaviorSystem",
                CrewMembers = new List<D20CrewMemberOwnerReadSnapshot>
                {
                    new D20CrewMemberOwnerReadSnapshot
                    {
                        CrewId = "crew:projection:z",
                        QualifiedRoleIds = new List<string> { "Navigator" },
                        IsAvailable = true,
                        IsIncapacitated = false,
                        MoraleScore = 80,
                        FatigueScore = 20,
                        HealthScore = 95,
                    },
                    new D20CrewMemberOwnerReadSnapshot
                    {
                        CrewId = "crew:projection:a",
                        QualifiedRoleIds = new List<string> { "Helm" },
                        IsAvailable = true,
                        IsIncapacitated = false,
                        MoraleScore = 90,
                        FatigueScore = 10,
                        HealthScore = 100,
                    },
                },
            };
        }

        private static D21CrewRequirementProfile CreateProfile()
        {
            return new D21CrewRequirementProfile
            {
                SchemaVersion = D21CrewRequirementRules.CurrentSchemaVersion,
                MinimumCrew = 2,
                StandardCrew = 2,
                MaximumCrew = 3,
                MinimumAverageMorale = 50,
                MaximumAverageFatigue = 70,
                MinimumAverageHealth = 50,
                RequiredRoles = new List<D21CrewRoleRequirement>
                {
                    new D21CrewRoleRequirement { Role = D21CrewRole.Helmsman, MinimumCount = 1 },
                    new D21CrewRoleRequirement { Role = D21CrewRole.Navigator, MinimumCount = 1 },
                },
            };
        }

        private static D21VoyageCommandRequest CreateStartRequest(
            string voyageId, string commandId, D21CrewRequirementProfile profile)
        {
            const string shipId = "ship:projection:01";
            const string routeId = "route:projection:01";
            const string originId = "port:projection:origin";
            const string destinationId = "port:projection:destination";
            const double distanceNm = 100000d;
            var route = new D21NavigationRouteDefinition
            {
                RouteId = routeId,
                OriginId = originId,
                DestinationId = destinationId,
                TotalDistanceNm = distanceNm,
                MinimumSafeDepthMeters = 3,
                RequiredDiscoveryState = D21RouteDiscoveryState.Discovered,
            };
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:projection:origin",
                LocationRef = "location:projection:origin",
                Sequence = 0,
                DistanceFromOriginNm = 0,
                MinimumDepthMeters = 3,
                HazardZoneIds = new List<string>(),
            });
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:projection:destination",
                LocationRef = "location:projection:destination",
                Sequence = 1,
                DistanceFromOriginNm = distanceNm,
                MinimumDepthMeters = 3,
                HazardZoneIds = new List<string>(),
            });
            return new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.Start,
                CommandId = commandId,
                ActorId = "actor:projection",
                ShipId = shipId,
                VoyageId = voyageId,
                RouteId = routeId,
                OriginPortId = originId,
                DestinationPortId = destinationId,
                WorldDay = 7,
                MinuteOfDay = 0,
                TickIndex = 0,
                SaveRevision = 7,
                VoyageSeed = 21,
                BaseSpeedMilliKnots = 1000,
                CurrentDraftMeters = 1,
                HazardTolerance = 100,
                KnownDiscoveryState = D21RouteDiscoveryState.Discovered,
                ShipIdentity = D21ShipIdentityContracts.CreateDefault(
                    shipId, D21ShipTypeIds.Sloop, "owner:projection", "Projection Ship"),
                HullConfiguration = D21HullComponentSystem.CreateStarterConfiguration(
                    shipId, D21ShipTypeIds.Sloop),
                ConditionState = D21ShipConditionSystem.CreateHealthyState(shipId, 100, 50),
                CrewProfile = profile.Clone(),
                AnchorState = new D21AnchorMooringState
                {
                    SchemaVersion = D21AnchorMooringSystem.CurrentSchemaVersion,
                    ShipId = shipId,
                    AnchorState = D21AnchorState.Unanchored,
                    MooringState = D21MooringState.Unmoored,
                    ChainDeployedMeters = 0,
                    DragRisk = 0,
                    SaveRevision = 7,
                },
                RouteDefinition = route,
                WeatherInput = new D21WeatherInput
                {
                    EventId = "weather:projection:7",
                    SourceId = "d9:projection",
                    LocationRef = "location:projection:open-sea",
                    WorldDay = 7,
                    SaveRevision = 7,
                    Seed = 21,
                    Season = D21WeatherSeason.Dry,
                    WindDirection = D21WindDirection.Calm,
                    WindForce = 0,
                    VisibilityFraction = 1,
                    PressureHpa = 1013.25f,
                },
                ProvisionRequest = new D21ProvisionConsumptionRequest
                {
                    VoyageId = voyageId,
                    ShipId = shipId,
                    ApplicationId = "projection:start:" + voyageId,
                    WorldDay = 7,
                    SaveRevision = 7,
                    TickIndex = 0,
                    CrewCount = 2,
                    FoodUnits = 1000,
                    WaterUnits = 1000,
                    FoodUnitsPerCrew = 1,
                    WaterUnitsPerCrew = 1,
                },
            };
        }

        private static D21CrewRequirementDecision CreateValidDecision()
        {
            return new D21CrewRequirementDecision
            {
                Valid = true,
                Eligibility = D21CrewEligibilityState.Eligible,
                CanDepart = true,
                ActiveCrewCount = 2,
                AverageMorale = 85,
                AverageFatigue = 15,
                AverageHealth = 97.5,
                RequiredCrewCount = 2,
                NavigationEfficiencyMultiplier = 1,
                MissingRoles = new System.Collections.ObjectModel.ReadOnlyCollection<D21CrewRole>(
                    new List<D21CrewRole>()),
                ReasonCodes = new System.Collections.ObjectModel.ReadOnlyCollection<string>(
                    new List<string>()),
                Assignments = new System.Collections.ObjectModel.ReadOnlyCollection<D21CrewRoleAssignment>(
                    new[]
                    {
                        new D21CrewRoleAssignment { Role = D21CrewRole.Helmsman, CrewId = "crew:projection:a" },
                        new D21CrewRoleAssignment { Role = D21CrewRole.Navigator, CrewId = "crew:projection:z" },
                    }),
            };
        }

        private static bool Contains(IEnumerable<string> values, string prefix)
        {
            if (values == null) return false;
            foreach (string value in values)
                if (value != null && value.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
