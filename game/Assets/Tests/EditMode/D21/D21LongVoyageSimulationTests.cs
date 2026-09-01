// D21LongVoyageSimulationTests —— D21-L27 固定种子长航程纯逻辑测试
//
// 这里只运行 D21 owner 的公开 C# API。长航程使用显式世界日、TickIndex 和
// save_revision，验证 10,000 个固定步不会产生负补给、重复事件或无法解释
// 的生命周期状态；不启动 Unity 场景、物理、输入或 Player。

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21LongVoyageSimulationTests
    {
        private const string VoyageId = "voyage:l27:10000";
        private const string ShipId = "ship_l27_01";
        private const string RouteId = "route:l27:10000";
        private const long VoyageSeed = 271828L;
        private const int InitialProvisions = 20000;
        private const int LongTickCount = 10000;
        private const int IntegratedPrefixTickCount = 128;
        private const long LongRouteDistanceMilliNm = 50000000000L;

        [Test]
        public void FixedSeed_10000Ticks_NeverCreatesNegativeProvisionOrDeadlock()
        {
            D21VoyageTimeState time = D21VoyageTimeState.CreateInitial(
                VoyageId, ShipId, LongRouteDistanceMilliNm, 10, 0, 0);
            D21CourseState course = D21CourseState.CreateInitial(VoyageId, ShipId);
            course.Mode = D21CourseMode.Course;
            course.HasCourse = true;
            course.HeadingCentidegrees = 0;
            course.SpeedPermille = 1000;
            course.Validate();
            D21AnchorMooringState anchor = new D21AnchorMooringState
            {
                ShipId = ShipId,
                AnchorState = D21AnchorState.Unanchored,
                MooringState = D21MooringState.Unmoored,
                ChainDeployedMeters = 0,
                DragRisk = 0,
                SaveRevision = 0,
                LastEventId = string.Empty,
            };
            D21VoyageEventRouter router = new D21VoyageEventRouter();
            D21WeatherInput initialWeather = CreateWeather(10, 0);
            D21ShipSaveProjection projection = router.ReconcileProjection(
                D21ShipSaveProjection.CreateDefault(ShipId),
                VoyageId,
                ShipId,
                RouteId,
                D21VoyageStatus.Sailing,
                0,
                0,
                time,
                course,
                null,
                anchor,
                initialWeather,
                null,
                string.Empty);
            Assert.That(projection, Is.Not.Null);

            D21TickContext startContext = D21VoyageTimeSystem.CreateTickContext(
                10, 0, 0, 0, VoyageSeed, VoyageId, ShipId);
            D21VoyageEvent started = D21VoyageEventContracts.Create(
                D21VoyageEventType.VoyageStarted,
                VoyageId,
                ShipId,
                RouteId,
                startContext,
                "d21.voyage.started",
                new[] { "ship:" + ShipId, "route:" + RouteId },
                null,
                D21VoyageEventOutcome.Normal,
                "l27-long-start");
            D21VoyageEventRouteResult startRoute = router.Append(started, projection);
            Assert.That(startRoute.Accepted, Is.True, startRoute.Code);
            projection = router.ReconcileProjection(
                startRoute.SaveProjection,
                VoyageId,
                ShipId,
                RouteId,
                D21VoyageStatus.Sailing,
                0,
                0,
                time,
                course,
                null,
                anchor,
                null,
                null,
                started.EventId);
            Assert.That(projection, Is.Not.Null);

            string previousEventId = started.EventId;
            int worldDay = 10;
            int minuteOfDay = 0;
            var eventIds = new HashSet<string>(StringComparer.Ordinal);
            int arrivedCount = 0;
            int foodRemaining = InitialProvisions;
            int waterRemaining = InitialProvisions;

            for (int tick = 0; tick < LongTickCount; tick++)
            {
                D21TickContext context = D21VoyageTimeSystem.CreateTickContext(
                    worldDay, minuteOfDay, tick, tick + 1, VoyageSeed, VoyageId, ShipId);
                D21WeatherInput weather = CreateWeather(worldDay, tick + 1);
                D21SeaStateResult seaState = D21SeaStateSystem.EvaluateDeterministic(weather);
                D21WeatherImpactResult impact = D21WeatherImpactRules.Evaluate(weather, seaState);
                Assert.That(seaState.Success, Is.True, "tick " + tick + " sea state");
                Assert.That(impact.Success, Is.True, "tick " + tick + " weather impact");

                D21VoyageTimeResult timeResult = D21VoyageTimeSystem.Advance(
                    time,
                    new D21VoyageTimeInput
                    {
                        RouteDistanceMilliNm = LongRouteDistanceMilliNm,
                        BaseSpeedMilliKnots = 1000,
                        SpeedPermille = 1000,
                        CurrentPermille = ToPermille(impact.SpeedMultiplier),
                    },
                    context);
                Assert.That(timeResult.Accepted, Is.True, "tick " + tick + " time");
                Assert.That(timeResult.IsArrived, Is.False, "tick " + tick + " arrived");

                D21VoyageRiskResult riskResult = D21VoyageRiskSystem.Evaluate(
                    CreateRiskInput(context, weather, seaState));
                Assert.That(riskResult.Accepted, Is.True, "tick " + tick + " risk");
                Assert.That(riskResult.AnyTriggered, Is.False, "calm tick " + tick + " risk");

                int foodCost = Math.Max(1, ToCeilingUnits(impact.FoodConsumptionMultiplier));
                int waterCost = Math.Max(1, ToCeilingUnits(impact.WaterConsumptionMultiplier));
                foodRemaining -= foodCost;
                waterRemaining -= waterCost;
                Assert.That(foodRemaining, Is.GreaterThanOrEqualTo(0), "tick " + tick + " food");
                Assert.That(waterRemaining, Is.GreaterThanOrEqualTo(0), "tick " + tick + " water");

                D21VoyageEvent latest = D21VoyageEventContracts.Create(
                    D21VoyageEventType.VoyageTickAdvanced,
                    VoyageId,
                    ShipId,
                    RouteId,
                    context,
                    "d21.voyage.tick_advanced",
                    new[] { weather.EventId },
                    new[] { previousEventId },
                    D21VoyageEventOutcome.Normal,
                    "l27-long-tick-" + tick.ToString("D5"),
                    string.Empty,
                    0,
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { "food_remaining", foodRemaining.ToString() },
                        { "water_remaining", waterRemaining.ToString() },
                        { "elapsed_ticks", timeResult.State.ElapsedTicks.ToString() },
                    });
                D21VoyageEventRouteResult eventResult = router.Append(latest, projection);
                Assert.That(eventResult.Accepted, Is.True, "tick " + tick + " event");
                Assert.That(eventIds.Add(eventResult.Event.EventId), Is.True,
                    "tick " + tick + " event repeated");
                if (eventResult.Event.EventType == D21VoyageEventType.Arrived) arrivedCount++;

                projection = router.ReconcileProjection(
                    eventResult.SaveProjection,
                    VoyageId,
                    ShipId,
                    RouteId,
                    D21VoyageStatus.Sailing,
                    timeResult.State.ElapsedTicks,
                    context.SaveRevision,
                    timeResult.State,
                    course,
                    null,
                    anchor,
                    null,
                    null,
                    eventResult.Event.EventId);
                Assert.That(projection, Is.Not.Null, "tick " + tick + " projection");
                time = timeResult.State;
                previousEventId = eventResult.Event.EventId;

                AdvanceClock(ref worldDay, ref minuteOfDay);
            }

            Assert.That(time.ElapsedTicks, Is.EqualTo(LongTickCount));
            Assert.That(time.DistanceTravelledMilliNm, Is.LessThan(time.RouteDistanceMilliNm));
            Assert.That(arrivedCount, Is.EqualTo(0));
            Assert.That(foodRemaining, Is.EqualTo(InitialProvisions - LongTickCount));
            Assert.That(waterRemaining, Is.EqualTo(InitialProvisions - LongTickCount));

            D21VoyageEventLogSnapshot finalLog = router.GetLogSnapshot(VoyageId);
            Assert.That(finalLog.Events.Count, Is.EqualTo(1 + LongTickCount));
            Assert.That(finalLog.Events.Exists(item => item == null), Is.False);
            D21ShipSaveProjection fullProjection = router.GetProjection(VoyageId);
            Assert.That(fullProjection, Is.Not.Null);
            fullProjection.Validate();
            Assert.That(fullProjection.Events.EventLog.Events.Count, Is.EqualTo(1 + LongTickCount));
        }

        [Test]
        public void CommandService_IntegratesResolutionAndProvisionOwners_ForBoundedPrefix()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21VoyageCommandResult started = service.Route(CreateStartRequest());

            Assert.That(started.Accepted, Is.True, started.Code);
            Assert.That(started.Resolution, Is.Not.Null);
            Assert.That(started.Status, Is.EqualTo(D21VoyageStatus.Sailing));

            int worldDay = 10;
            int minuteOfDay = 0;
            for (int tick = 0; tick < IntegratedPrefixTickCount; tick++)
            {
                D21VoyageCommandResult result = service.Route(CreateTickRequest(
                    tick, worldDay, minuteOfDay, tick + 1));
                Assert.That(result.Accepted, Is.True,
                    "tick " + tick + ": " + result.Code);
                Assert.That(result.Status, Is.EqualTo(D21VoyageStatus.Sailing));
                Assert.That(result.Resolution.ProvisionResult.Ledger.FoodRemaining,
                    Is.GreaterThanOrEqualTo(0));
                Assert.That(result.Resolution.ProvisionResult.Ledger.WaterRemaining,
                    Is.GreaterThanOrEqualTo(0));
                AdvanceClock(ref worldDay, ref minuteOfDay);
            }

            D21VoyageResolutionState finalState = service.ResolutionSystem.GetSnapshot(VoyageId);
            Assert.That(finalState, Is.Not.Null);
            Assert.That(finalState.Time.ElapsedTicks, Is.EqualTo(IntegratedPrefixTickCount));
            finalState.SaveProjection.Validate();
            Assert.That(service.EventRouter.GetLogSnapshot(VoyageId).Events.Count,
                Is.EqualTo(1 + 4 * IntegratedPrefixTickCount));
            D21ShipSaveProjection fullProjection = service.ResolutionSystem.GetSaveProjection(VoyageId);
            Assert.That(fullProjection.Events.EventLog.Events.Count,
                Is.EqualTo(1 + 4 * IntegratedPrefixTickCount));
            Assert.That(fullProjection.Provisions.ConsumedFoodUnits,
                Is.EqualTo(IntegratedPrefixTickCount));
            Assert.That(fullProjection.Provisions.ConsumedWaterUnits,
                Is.EqualTo(IntegratedPrefixTickCount));
        }

        [Test]
        public void CommandService_IntegratesResolutionAndProvisionOwners_For10000Ticks()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            D21VoyageCommandResult started = service.Route(CreateStartRequest());

            Assert.That(started.Accepted, Is.True, started.Code);
            Assert.That(started.Status, Is.EqualTo(D21VoyageStatus.Sailing));

            int worldDay = 10;
            int minuteOfDay = 0;
            D21VoyageCommandResult last = null;
            for (int tick = 0; tick < LongTickCount; tick++)
            {
                last = service.Route(CreateTickRequest(
                    tick, worldDay, minuteOfDay, tick + 1));
                Assert.That(last.Accepted, Is.True,
                    "tick " + tick + ": " + last.Code);
                Assert.That(last.IsDuplicate, Is.False, "tick " + tick + " duplicated");
                Assert.That(last.Status, Is.EqualTo(D21VoyageStatus.Sailing),
                    "tick " + tick + " entered a terminal/deadlocked state");
                Assert.That(last.Resolution, Is.Not.Null, "tick " + tick + " resolution");
                Assert.That(last.Resolution.Applied, Is.True, "tick " + tick + " was not applied");
                Assert.That(last.Resolution.VoyageFailed, Is.False, "tick " + tick + " failed");
                Assert.That(last.Resolution.ProvisionResult, Is.Not.Null,
                    "tick " + tick + " provision result");
                Assert.That(last.Resolution.ProvisionResult.Applied, Is.True,
                    "tick " + tick + " provision was not applied");
                Assert.That(last.Resolution.ProvisionResult.Ledger.FoodRemaining,
                    Is.GreaterThanOrEqualTo(0), "tick " + tick + " food");
                Assert.That(last.Resolution.ProvisionResult.Ledger.WaterRemaining,
                    Is.GreaterThanOrEqualTo(0), "tick " + tick + " water");
                AdvanceClock(ref worldDay, ref minuteOfDay);
            }

            Assert.That(last, Is.Not.Null);
            D21VoyageResolutionState finalState = service.ResolutionSystem.GetSnapshot(VoyageId);
            Assert.That(finalState, Is.Not.Null);
            Assert.That(finalState.Status, Is.EqualTo(D21VoyageStatus.Sailing));
            Assert.That(finalState.Time.ElapsedTicks, Is.EqualTo(LongTickCount));
            Assert.That(finalState.LastTickIndex, Is.EqualTo(LongTickCount - 1));
            Assert.That(finalState.SaveProjection, Is.Not.Null);
            finalState.SaveProjection.Validate();

            D21ProvisionLedgerSnapshot finalLedger = last.Resolution.ProvisionResult.Ledger;
            Assert.That(finalLedger.FoodRemaining, Is.EqualTo(InitialProvisions - LongTickCount));
            Assert.That(finalLedger.WaterRemaining, Is.EqualTo(InitialProvisions - LongTickCount));
            IReadOnlyList<string> applicationHistory = finalLedger.AppliedApplicationIds;
            Assert.That(applicationHistory.Count, Is.EqualTo(LongTickCount));
            Assert.That(applicationHistory[0],
                Is.EqualTo("tick:" + VoyageId + ":0"));
            Assert.That(applicationHistory[LongTickCount - 1],
                Is.EqualTo("tick:" + VoyageId + ":" + (LongTickCount - 1)));
            ((List<string>)applicationHistory)[0] = "tampered";
            Assert.That(finalLedger.AppliedApplicationIds[0],
                Is.EqualTo("tick:" + VoyageId + ":0"));

            D21VoyageEventLogSnapshot finalLog = service.EventRouter.GetLogSnapshot(VoyageId);
            Assert.That(finalLog, Is.Not.Null);
            Assert.That(finalLog.Events.Count, Is.EqualTo(1 + 4 * LongTickCount));
            var eventKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (D21VoyageEvent item in finalLog.Events)
            {
                Assert.That(item, Is.Not.Null);
                Assert.That(eventKeys.Add(item.DedupeKey), Is.True,
                    "duplicate event key " + item.DedupeKey);
            }
            finalLog.Validate();
        }

        [Test]
        public void ProvisionHistory_IsOrderedDefensive_AndReplayKeepsLedgerStable()
        {
            D21ProvisionConsumptionSystem owner = new D21ProvisionConsumptionSystem();
            D21ProvisionConsumptionRequest firstRequest = new D21ProvisionConsumptionRequest
            {
                VoyageId = "voyage:l27:history",
                ShipId = "ship_l27_history",
                ApplicationId = "history-app-1",
                WorldDay = 1,
                SaveRevision = 1,
                TickIndex = 0,
                CrewCount = 0,
                FoodUnits = 10,
                WaterUnits = 10,
                FoodUnitsPerCrew = 0,
                WaterUnitsPerCrew = 0,
            };
            D21ProvisionConsumptionResult first = owner.Consume(firstRequest);
            D21ProvisionConsumptionResult replay = owner.Consume(firstRequest.Clone());

            Assert.That(first.Success, Is.True, first.FailureCode);
            Assert.That(first.Applied, Is.True);
            Assert.That(replay.Success, Is.True);
            Assert.That(replay.Applied, Is.False);
            Assert.That(replay.IsDuplicate, Is.True);
            Assert.That(replay.Ledger.FoodRemaining, Is.EqualTo(first.Ledger.FoodRemaining));
            Assert.That(replay.Ledger.AppliedApplicationIds.Count, Is.EqualTo(1));

            D21ProvisionConsumptionRequest conflictRequest = firstRequest.Clone();
            conflictRequest.FoodUnits = 9;
            D21ProvisionConsumptionResult conflict = owner.Consume(conflictRequest);
            Assert.That(conflict.Applied, Is.False);
            Assert.That(conflict.FailureCode,
                Is.EqualTo(D21ProvisionFailureCodes.ApplicationConflict));
            Assert.That(owner.GetSnapshot(firstRequest.VoyageId).FoodRemaining, Is.EqualTo(10));

            D21ProvisionConsumptionRequest secondRequest = firstRequest.Clone();
            secondRequest.ApplicationId = "history-app-2";
            secondRequest.TickIndex = 1;
            D21ProvisionConsumptionResult second = owner.Consume(secondRequest);
            Assert.That(second.Success, Is.True, second.FailureCode);
            IReadOnlyList<string> history = second.Ledger.AppliedApplicationIds;
            Assert.That(history, Is.EqualTo(new[] { "history-app-1", "history-app-2" }));
            ((List<string>)history).Clear();
            Assert.That(second.Ledger.AppliedApplicationIds,
                Is.EqualTo(new[] { "history-app-1", "history-app-2" }));

            D21ProvisionConsumptionSystem shortageOwner = new D21ProvisionConsumptionSystem();
            D21ProvisionConsumptionResult shortage = shortageOwner.Consume(new D21ProvisionConsumptionRequest
            {
                VoyageId = "voyage:l27:shortage",
                ShipId = "ship_l27_shortage",
                ApplicationId = "shortage-app-1",
                WorldDay = 1,
                SaveRevision = 1,
                TickIndex = 0,
                CrewCount = 2,
                FoodUnits = 1,
                WaterUnits = 1,
                FoodUnitsPerCrew = 1,
                WaterUnitsPerCrew = 1,
            });
            Assert.That(shortage.Applied, Is.False);
            Assert.That(shortage.FailureCode,
                Is.EqualTo(D21ProvisionFailureCodes.InsufficientFoodAndWater));
            Assert.That(shortageOwner.GetSnapshot("voyage:l27:shortage"), Is.Null);
        }

        [Test]
        public void ReplayingSameTick_IsIdempotentWithoutSecondProvisionOrEvent()
        {
            D21VoyageCommandService service = new D21VoyageCommandService();
            Assert.That(service.Route(CreateStartRequest()).Accepted, Is.True);

            D21VoyageCommandRequest request = CreateTickRequest(0, 10, 0, 1);
            D21VoyageCommandResult first = service.Route(request);
            D21VoyageEventLogSnapshot afterFirst = service.EventRouter.GetLogSnapshot(VoyageId);
            D21VoyageCommandResult replay = service.Route(request.Clone());
            D21VoyageEventLogSnapshot afterReplay = service.EventRouter.GetLogSnapshot(VoyageId);

            Assert.That(first.Accepted, Is.True, first.Code);
            Assert.That(replay.Accepted, Is.True, replay.Code);
            Assert.That(replay.IsDuplicate, Is.True);
            Assert.That(replay.Resolution.IsDuplicate, Is.True);
            Assert.That(afterReplay.Events.Count, Is.EqualTo(afterFirst.Events.Count));
            Assert.That(replay.Resolution.ProvisionResult.Ledger.TotalFoodConsumed,
                Is.EqualTo(first.Resolution.ProvisionResult.Ledger.TotalFoodConsumed));
            Assert.That(replay.Resolution.ProvisionResult.Ledger.TotalWaterConsumed,
                Is.EqualTo(first.Resolution.ProvisionResult.Ledger.TotalWaterConsumed));
        }

        private static D21VoyageCommandRequest CreateStartRequest()
        {
            D21NavigationRouteDefinition route = CreateRoute();
            D21ShipIdentity identity = D21ShipIdentityContracts.CreateDefault(
                ShipId, D21ShipTypeIds.Sloop, "owner_l27", "L27 Long Run");
            D21VoyageCommandRequest request = new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.Start,
                CommandId = "l27-start",
                ActorId = "actor_l27",
                ShipId = ShipId,
                VoyageId = VoyageId,
                RouteId = RouteId,
                OriginPortId = route.OriginId,
                DestinationPortId = route.DestinationId,
                WorldDay = 10,
                MinuteOfDay = 0,
                TickIndex = 0,
                SaveRevision = 0,
                VoyageSeed = VoyageSeed,
                BaseSpeedMilliKnots = 1000,
                CurrentDraftMeters = 1,
                HazardTolerance = 100,
                KnownDiscoveryState = D21RouteDiscoveryState.Discovered,
                ShipIdentity = identity,
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
                    LastEventId = string.Empty,
                },
                RouteDefinition = route,
                WeatherInput = CreateWeather(10, 0),
                ProvisionRequest = new D21ProvisionConsumptionRequest
                {
                    VoyageId = VoyageId,
                    ShipId = ShipId,
                    ApplicationId = "l27-start-provision",
                    WorldDay = 10,
                    SaveRevision = 0,
                    TickIndex = 0,
                    CrewCount = 1,
                    FoodUnits = InitialProvisions,
                    WaterUnits = InitialProvisions,
                    FoodUnitsPerCrew = 1,
                    WaterUnitsPerCrew = 1,
                },
            };
            return request;
        }

        private static D21VoyageCommandRequest CreateTickRequest(
            int tick,
            int worldDay,
            int minuteOfDay,
            long saveRevision)
        {
            return new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.ResolveTick,
                CommandId = "l27-tick-" + tick.ToString("D5"),
                ActorId = "actor_l27",
                ShipId = ShipId,
                VoyageId = VoyageId,
                RouteId = RouteId,
                WorldDay = worldDay,
                MinuteOfDay = minuteOfDay,
                TickIndex = tick,
                SaveRevision = saveRevision,
                VoyageSeed = VoyageSeed,
                WeatherInput = CreateWeather(worldDay, saveRevision),
            };
        }

        private static D21NavigationRouteDefinition CreateRoute()
        {
            const double distanceNm = 5000000d;
            var route = new D21NavigationRouteDefinition
            {
                RouteId = RouteId,
                OriginId = "port:l27:origin",
                DestinationId = "port:l27:destination",
                TotalDistanceNm = distanceNm,
                MinimumSafeDepthMeters = 3,
                RequiredDiscoveryState = D21RouteDiscoveryState.Discovered,
            };
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:l27:origin",
                LocationRef = "location:l27:origin",
                Sequence = 0,
                DistanceFromOriginNm = 0,
                MinimumDepthMeters = 3,
                DiscoveryRequired = false,
                HazardZoneIds = new List<string>(),
            });
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = "waypoint:l27:destination",
                LocationRef = "location:l27:destination",
                Sequence = 1,
                DistanceFromOriginNm = distanceNm,
                MinimumDepthMeters = 3,
                DiscoveryRequired = false,
                HazardZoneIds = new List<string>(),
            });
            return route;
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
                    new D21CrewRoleAssignment { Role = D21CrewRole.Navigator, CrewId = "crew:l27:01" },
                }),
            };
        }

        private static D21WeatherInput CreateWeather(int worldDay, long saveRevision)
        {
            return new D21WeatherInput
            {
                EventId = "weather:l27:" + worldDay.ToString() + ":" + saveRevision.ToString(),
                SourceId = "d9:l27",
                LocationRef = "location:l27:open-sea",
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Seed = VoyageSeed,
                Season = D21WeatherSeason.Dry,
                WindDirection = D21WindDirection.Calm,
                WindForce = 0,
                PrecipitationMmPerHour = 0,
                VisibilityFraction = 1,
                PressureHpa = 1013.25f,
            };
        }

        private static D21VoyageRiskInput CreateRiskInput(
            D21TickContext context,
            D21WeatherInput weather,
            D21SeaStateResult seaState)
        {
            return new D21VoyageRiskInput
            {
                VoyageId = VoyageId,
                ShipId = ShipId,
                RouteId = RouteId,
                LocationRef = "location:l27:open-sea",
                VoyageSeed = VoyageSeed,
                TickContext = context,
                ReefZone = false,
                StormActive = false,
                HostileWaters = false,
                DiseaseExposure = false,
                ReefRiskPermille = 0,
                StormRiskPermille = 0,
                HostileRiskPermille = 0,
                DiseaseRiskPermille = 0,
                SeaStateRiskPermille = 0,
                VisibilityPermille = 1000,
                ShipConditionPermille = 1000,
                CrewHealthPermille = 1000,
                CrewExposurePermille = 0,
                HasLookout = true,
                HasMedicine = true,
                WeatherSnapshotId = weather.EventId,
                WeatherSnapshotSchemaVersion = weather.SchemaVersion,
                WeatherSnapshotRevision = weather.SaveRevision,
                InputFactIds = new List<string> { weather.EventId, seaState.InputFingerprint },
            };
        }

        private static int ToPermille(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0;
            return Math.Max(0, Math.Min(2000,
                (int)Math.Round(value * 1000f, MidpointRounding.AwayFromZero)));
        }

        private static int ToCeilingUnits(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) return 0;
            return (int)Math.Ceiling(value);
        }

        private static void AdvanceClock(ref int worldDay, ref int minuteOfDay)
        {
            minuteOfDay += D21TickContext.FixedTickMinutes;
            if (minuteOfDay >= D21TickContext.MinutesPerDay)
            {
                minuteOfDay -= D21TickContext.MinutesPerDay;
                worldDay++;
            }
        }
    }
}
