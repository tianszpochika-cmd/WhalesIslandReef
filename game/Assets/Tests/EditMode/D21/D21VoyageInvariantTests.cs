// D21VoyageInvariantTests —— D21-L28 聚合不变式与拒绝路径
//
// 测试只使用 D21/D19 的公开纯逻辑载荷。每个拒绝都验证没有创建航程、没有
// 追加事件或没有产生第二次补给副作用；真实场景、物理、输入和 Player 不参与。

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21VoyageInvariantTests
    {
        private const string ShipId = "ship_l28_01";
        private const string VoyageId = "voyage:l28:main";
        private const string RouteId = "route:l28:main";

        [Test]
        public void AnchoredShip_CannotStartVoyage_AndProducesNoEvent()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21VoyageCommandRequest request = CreateValidStartRequest();
            request.AnchorState.AnchorState = D21AnchorState.Holding;

            D21VoyageCommandResult result = service.Route(request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Code, Is.EqualTo(D21VoyageCommandService.AnchorRejectedCode));
            Assert.That(service.ResolutionSystem.GetSnapshot(VoyageId), Is.Null);
            Assert.That(service.EventRouter.GetLogSnapshot(VoyageId), Is.Null);
        }

        [Test]
        public void LostShipCondition_CannotStartVoyage_AndPreservesEmptyRouter()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21ShipConditionSystem conditionSystem = new D21ShipConditionSystem(
                D21ShipConditionSystem.CreateHealthyState(ShipId, 100, 50));
            D21ShipConditionResult damage = conditionSystem.ApplyDamage(
                "l28-damage", ShipId, new D21ShipDamageEntry
                {
                    DamageId = "damage:l28:structural",
                    Kind = D21ShipDamageKind.Structural,
                    ComponentId = "hull.main",
                    Severity = D21ShipDamageSeverity.Critical,
                    Amount = 1000,
                    Active = true,
                    SourceFactId = "fact:l28:grounding",
                    SourceKind = D21DamageSourceKind.GroundingFact,
                    WorldDay = 1,
                    SaveRevision = 0,
                }, 0, "event:l28:grounding");
            Assert.That(damage.Accepted, Is.True);

            D21VoyageCommandRequest request = CreateValidStartRequest();
            request.ConditionState = damage.State;
            D21VoyageCommandResult result = service.Route(request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Code, Is.EqualTo(D21VoyageCommandService.ConditionRejectedCode));
            Assert.That(service.ResolutionSystem.GetSnapshot(VoyageId), Is.Null);
            Assert.That(service.EventRouter.GetLogSnapshot(VoyageId), Is.Null);
        }

        [Test]
        public void MissingCrewProjection_IsDependencyBlocked_AndCannotBePresentedAsPass()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21VoyageCommandRequest request = CreateValidStartRequest();
            request.CrewDecision = null;
            request.CrewProfile = null;
            request.CrewRoster = null;

            D21VoyageCommandResult result = service.Route(request);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.DependencyBlocked, Is.True);
            Assert.That(result.Code, Is.EqualTo(D21VoyageCommandService.CrewProjectionRequiredCode));
            Assert.That(service.ResolutionSystem.GetSnapshot(VoyageId), Is.Null);
        }

        [Test]
        public void CommandReplay_IsIdempotent_AndReusingCommandIdWithDifferentPayloadIsRejected()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21VoyageCommandRequest original = CreateValidStartRequest();
            D21VoyageCommandResult first = service.Route(original);
            D21VoyageCommandResult replay = service.Route(original.Clone());
            D21VoyageCommandRequest changed = original.Clone();
            changed.WorldDay = original.WorldDay + 1;
            D21VoyageCommandResult conflict = service.Route(changed);

            Assert.That(first.Accepted, Is.True, first.Code);
            Assert.That(replay.Accepted, Is.True);
            Assert.That(replay.IsDuplicate, Is.True);
            Assert.That(conflict.Accepted, Is.False);
            Assert.That(conflict.Code, Is.EqualTo(D21VoyageCommandService.CommandIdConflictCode));
            Assert.That(service.EventRouter.GetLogSnapshot(VoyageId).Events.Count, Is.EqualTo(1));
        }

        [Test]
        public void EventRouter_RejectsBatchWithDescendingOrderWithoutPartialAppend()
        {
            D21VoyageEventRouter router = new D21VoyageEventRouter();
            D21ShipSaveProjection projection = D21ShipSaveProjection.CreateDefault(ShipId);
            D21TickContext context = D21VoyageTimeSystem.CreateTickContext(
                1, 0, 0, 0, 28, VoyageId, ShipId);
            D21VoyageEvent riskResolved = D21VoyageEventContracts.Create(
                D21VoyageEventType.RiskResolved, VoyageId, ShipId, RouteId, context,
                "d21.test.risk_resolved", new[] { "input:l28" });
            D21VoyageEvent started = D21VoyageEventContracts.Create(
                D21VoyageEventType.VoyageStarted, VoyageId, ShipId, RouteId, context,
                "d21.test.started", new[] { "input:l28" });

            D21VoyageEventRouteResult result = router.AppendMany(
                new[] { riskResolved, started }, projection);

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.Code, Is.EqualTo(D21VoyageEventRouter.FirstEventMustStartCode));
            Assert.That(router.GetLogSnapshot(VoyageId), Is.Null);
        }

        private static D21VoyageCommandRequest CreateValidStartRequest()
        {
            const string originId = "port:l28:origin";
            const string destinationId = "port:l28:destination";
            const double distanceNm = 100000;
            var route = new D21NavigationRouteDefinition
            {
                RouteId = RouteId,
                OriginId = originId,
                DestinationId = destinationId,
                TotalDistanceNm = distanceNm,
                MinimumSafeDepthMeters = 3,
                RequiredDiscoveryState = D21RouteDiscoveryState.Discovered,
            };
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:l28:origin",
                LocationRef = "location:l28:origin",
                Sequence = 0,
                DistanceFromOriginNm = 0,
                MinimumDepthMeters = 3,
                HazardZoneIds = new List<string>(),
            });
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:l28:destination",
                LocationRef = "location:l28:destination",
                Sequence = 1,
                DistanceFromOriginNm = distanceNm,
                MinimumDepthMeters = 3,
                HazardZoneIds = new List<string>(),
            });
            return new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.Start,
                CommandId = "l28-start",
                ActorId = "actor:l28",
                ShipId = ShipId,
                VoyageId = VoyageId,
                RouteId = RouteId,
                OriginPortId = originId,
                DestinationPortId = destinationId,
                WorldDay = 1,
                MinuteOfDay = 0,
                SaveRevision = 0,
                VoyageSeed = 28,
                BaseSpeedMilliKnots = 1000,
                CurrentDraftMeters = 1,
                HazardTolerance = 100,
                KnownDiscoveryState = D21RouteDiscoveryState.Discovered,
                ShipIdentity = D21ShipIdentityContracts.CreateDefault(
                    ShipId, D21ShipTypeIds.Sloop, "owner:l28", "L28 Test Ship"),
                HullConfiguration = D21HullComponentSystem.CreateStarterConfiguration(
                    ShipId, D21ShipTypeIds.Sloop),
                ConditionState = D21ShipConditionSystem.CreateHealthyState(ShipId, 100, 50),
                CrewDecision = CreateCrewDecision(),
                AnchorState = new D21AnchorMooringState
                {
                    SchemaVersion = D21AnchorMooringSystem.CurrentSchemaVersion,
                    ShipId = ShipId,
                    AnchorState = D21AnchorState.Unanchored,
                    MooringState = D21MooringState.Unmoored,
                    ChainDeployedMeters = 0,
                    DragRisk = 0,
                    SaveRevision = 0,
                },
                RouteDefinition = route,
                WeatherInput = new D21WeatherInput
                {
                    EventId = "weather:l28:1",
                    SourceId = "d9:l28",
                    LocationRef = "location:l28:open-sea",
                    WorldDay = 1,
                    SaveRevision = 0,
                    Seed = 28,
                    Season = D21WeatherSeason.Dry,
                    WindDirection = D21WindDirection.Calm,
                    VisibilityFraction = 1,
                    PressureHpa = 1013.25f,
                },
                ProvisionRequest = new D21ProvisionConsumptionRequest
                {
                    VoyageId = VoyageId,
                    ShipId = ShipId,
                    FoodUnits = 1000,
                    WaterUnits = 1000,
                    FoodUnitsPerCrew = 1,
                    WaterUnitsPerCrew = 1,
                },
            };
        }

        private static D21CrewRequirementDecision CreateCrewDecision()
        {
            return new D21CrewRequirementDecision
            {
                Valid = true,
                Eligibility = D21CrewEligibilityState.Eligible,
                CanDepart = true,
                ActiveCrewCount = 1,
                AverageMorale = 90,
                AverageFatigue = 10,
                AverageHealth = 95,
                RequiredCrewCount = 1,
                NavigationEfficiencyMultiplier = 1,
                MissingRoles = new ReadOnlyCollection<D21CrewRole>(new List<D21CrewRole>()),
                ReasonCodes = new ReadOnlyCollection<string>(new List<string>()),
                Assignments = new ReadOnlyCollection<D21CrewRoleAssignment>(new[]
                {
                    new D21CrewRoleAssignment { Role = D21CrewRole.Navigator, CrewId = "crew:l28:01" },
                }),
            };
        }
    }
}
