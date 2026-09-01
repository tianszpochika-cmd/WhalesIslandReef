// D23FullLoopScenarioSupport —— D23-L26—L29 纯逻辑全链路执行器
//
// 只组合已经公开的 D19/D20/D21/D3/D8 owner API：角色进入港口、靠泊、
// 市场/仓库、登船、出航、固定 Tick、抵达、返港和保存投影回读。这里不
// 读取 Unity 场景、不模拟输入、不持有任何跨系统业务写状态。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Whalebone.Systems.D19;
using Whalebone.Systems.D20;
using Whalebone.Systems.D21;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.D23
{
    /// <summary>一次完整 D23 逻辑运行的机器可比结果。</summary>
    public sealed class D23FullLoopReport
    {
        public int Assertions;
        public int TickCount;
        public int D19FactCount;
        public int D21EventCount;
        public int SharedFactCount;
        public int Money;
        public int CargoCount;
        public bool Arrived;
        public bool Returned;
        public string SaveFingerprint = string.Empty;
        public string EventFingerprint = string.Empty;
        public string FinalFingerprint = string.Empty;
    }

    /// <summary>L28/L29 复用的已启动航程上下文；字段均为只读引用。</summary>
    public sealed class D23VoyageContext
    {
        public D23ScenarioFixture Fixture { get; private set; }
        public D21VoyageCommandService Service { get; private set; }
        public string Tag { get; private set; }
        public string VoyageId { get; private set; }
        public string RouteId { get; private set; }
        public long StartRevision { get; private set; }
        public int StartWorldDay { get; private set; }
        public int StartMinuteOfDay { get; private set; }
        public long VoyageSeed { get; private set; }
        public D21NavigationRouteDefinition Route { get; private set; }
        public D21VoyageCommandResult StartResult { get; private set; }

        internal D23VoyageContext(
            D23ScenarioFixture fixture,
            D21VoyageCommandService service,
            string tag,
            string voyageId,
            string routeId,
            long startRevision,
            int startWorldDay,
            int startMinuteOfDay,
            long voyageSeed,
            D21NavigationRouteDefinition route,
            D21VoyageCommandResult startResult)
        {
            Fixture = fixture;
            Service = service;
            Tag = tag;
            VoyageId = voyageId;
            RouteId = routeId;
            StartRevision = startRevision;
            StartWorldDay = startWorldDay;
            StartMinuteOfDay = startMinuteOfDay;
            VoyageSeed = voyageSeed;
            Route = route;
            StartResult = startResult;
        }
    }

    /// <summary>D23 全链路与长跑场景的唯一共享实现。</summary>
    public static class D23FullLoopScenarioSupport
    {
        private const int MainTickCount = 40;
        private const int LongRunTickCount = 50;
        private const int InitialProvisionUnits = 1000;
        private const int MainBaseSpeedMilliKnots = 12000;
        private const int LongRunBaseSpeedMilliKnots = 1000;

        public static D23FullLoopReport Run(string tag)
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            RequireGate(fixture, ref assertions);

            ApplyPresence(fixture, D20PlayerPresenceState.Port,
                D20PlayerPresenceTransitionReason.EnterPort, "full-loop:presence:port", ref assertions);

            RouteAndAppend(fixture, fixture.PortCommand(
                "full-loop:enter", D19PortCommandKind.EnterPort), ref assertions, "enter port");
            RouteAndAppend(fixture, fixture.PortCommand(
                "full-loop:dock", D19PortCommandKind.Dock), ref assertions, "dock");
            RouteAndAppend(fixture, fixture.PortCommand(
                "full-loop:berth", D19PortCommandKind.AssignBerth, fixture.BerthId),
                ref assertions, "assign berth");
            RouteAndAppend(fixture, fixture.PortCommand(
                "full-loop:board", D19PortCommandKind.Board, fixture.BerthId),
                ref assertions, "board");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PortVisit.State == D19PortVisitState.Boarded,
                "D23 full loop must reach Boarded before any service write.");
            ApplyPresence(fixture, D20PlayerPresenceState.Ship,
                D20PlayerPresenceTransitionReason.BoardShip, "full-loop:presence:ship", ref assertions);

            var marketRequest = new D19MarketTradeRequest
            {
                CommandId = "full-loop:market-buy",
                ActorId = fixture.PlayerId,
                ShipId = fixture.ShipId,
                PortId = fixture.PortId,
                BerthId = fixture.BerthId,
                ServiceId = D19PortLocalizationKeys.ServiceIdMarket,
                CommodityId = fixture.CommodityId,
                Quantity = 1,
                WorldDay = fixture.WorldDay,
                SaveRevision = fixture.SaveRevision + 1,
            };
            int moneyBefore = fixture.TradeState.Money;
            D19MarketTradeResult market = D19MarketPortAdapter.Buy(
                fixture.TradeState, fixture.Economy, marketRequest, fixture.CreateClaims());
            D23ScenarioAssertions.Check(ref assertions,
                market != null && market.Success && market.EventDraft != null,
                "D23 full loop market buy must succeed through D19->D3 and emit a draft.");
            AppendEvent(fixture, market.EventDraft, ref assertions, "market buy");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.TradeState.Money < moneyBefore,
                "D23 full loop market buy must be reflected by the D3 money owner.");

            var warehouse = new D19WarehousePortAdapter();
            var warehouseRequest = new D19WarehouseRequest(
                "full-loop:warehouse-store",
                D19WarehouseOperation.Store,
                fixture.PlayerId,
                fixture.ShipId,
                fixture.PortId,
                "service_santa_margarita_warehouse",
                fixture.CommodityId,
                2,
                fixture.ShipId,
                fixture.WorldDay,
                fixture.SaveRevision + 2);
            D19WarehouseResult stored = warehouse.Store(
                warehouseRequest, fixture.TradeState.Inventory, fixture.CreateClaims());
            D19WarehouseResult replay = warehouse.Store(
                warehouseRequest, fixture.TradeState.Inventory, fixture.CreateClaims());
            int cargoAfterStore;
            fixture.TradeState.Inventory.Cargo.TryGetValue(fixture.CommodityId, out cargoAfterStore);
            D23ScenarioAssertions.Check(ref assertions,
                stored != null && stored.Success && stored.AppliedQuantity == 2 &&
                stored.ItemCount == 3 && stored.Event != null,
                "D23 full loop warehouse store must report the owner-applied item count.");
            D23ScenarioAssertions.Check(ref assertions,
                replay != null && replay.Success && replay.AppliedQuantity == 2 &&
                replay.ItemCount == 3 && replay.Event != null &&
                replay.Event.EventId == stored.Event.EventId && cargoAfterStore == 3,
                "D23 full loop duplicate warehouse Store must not perform a second D3 write.");
            AppendEvent(fixture, stored.Event, ref assertions, "warehouse store");
            EventAppendResult warehouseDuplicate = fixture.EventLog.Append(replay.Event);
            D23ScenarioAssertions.Check(ref assertions, warehouseDuplicate.Succeeded &&
                warehouseDuplicate.IsDuplicate && fixture.EventLog.Count >= 2,
                "D23 full loop warehouse replay event must be deduplicated by D8.");
            var warehouseConflictRequest = new D19WarehouseRequest(
                "full-loop:warehouse-store",
                D19WarehouseOperation.Store,
                fixture.PlayerId,
                fixture.ShipId,
                fixture.PortId,
                "service_santa_margarita_warehouse",
                fixture.CommodityId,
                3,
                fixture.ShipId,
                fixture.WorldDay,
                fixture.SaveRevision + 2);
            D19WarehouseResult conflict = warehouse.Store(
                warehouseConflictRequest, fixture.TradeState.Inventory, fixture.CreateClaims());
            D23ScenarioAssertions.Check(ref assertions,
                conflict != null && !conflict.Success && cargoAfterStore == 3,
                "D23 full loop warehouse command conflict must preserve cargo.");

            D23VoyageContext voyage = CreateAndStart(
                "full-loop", 120d, MainBaseSpeedMilliKnots, true, fixture, ref assertions);
            ApplyPresence(fixture, D20PlayerPresenceState.Voyage,
                D20PlayerPresenceTransitionReason.StartVoyage, "full-loop:presence:voyage", ref assertions);
            AppendD21Events(fixture, voyage.StartResult.Resolution, ref assertions, "voyage start");
            D23ScenarioAssertions.Check(ref assertions,
                voyage.StartResult.Accepted && voyage.StartResult.Status == D21VoyageStatus.Sailing &&
                voyage.StartResult.DepartureResult != null &&
                voyage.StartResult.DepartureResult.Success &&
                fixture.PortVisit.State == D19PortVisitState.Departed,
                "D23 full loop must start D21 only after D19 departure is accepted.");
            AppendEvent(fixture,
                voyage.StartResult.DepartureResult.PortResult == null
                    ? null : voyage.StartResult.DepartureResult.PortResult.EventDraft,
                ref assertions, "departure boundary");

            string midSaveFingerprint = string.Empty;
            int worldDay = voyage.StartWorldDay;
            int minuteOfDay = voyage.StartMinuteOfDay;
            D21VoyageCommandResult lastTick = null;
            for (int tickIndex = 0; tickIndex < MainTickCount; tickIndex++)
            {
                long saveRevision = voyage.StartRevision + tickIndex + 1;
                var tick = new D21VoyageCommandRequest
                {
                    Kind = D21VoyageCommandKind.ResolveTick,
                    CommandId = voyage.Tag + ":tick:" + tickIndex.ToString("D3", CultureInfo.InvariantCulture),
                    ActorId = fixture.PlayerId,
                    ShipId = fixture.ShipId,
                    VoyageId = voyage.VoyageId,
                    RouteId = voyage.RouteId,
                    WorldDay = worldDay,
                    MinuteOfDay = minuteOfDay,
                    TickIndex = tickIndex,
                    SaveRevision = saveRevision,
                    VoyageSeed = voyage.VoyageSeed,
                    WeatherInput = CreateWeather(voyage.Tag, worldDay, saveRevision,
                        voyage.VoyageSeed),
                };
                lastTick = voyage.Service.Route(tick);
                D23ScenarioAssertions.Check(ref assertions,
                    lastTick != null && lastTick.Accepted && !lastTick.IsDuplicate &&
                    lastTick.Resolution != null && lastTick.Resolution.Applied,
                    "D23 full loop tick " + tickIndex + " must be accepted exactly once.");
                D23ScenarioAssertions.Check(ref assertions,
                    lastTick.Resolution.ProvisionResult != null &&
                    lastTick.Resolution.ProvisionResult.Ledger.FoodRemaining >= 0 &&
                    lastTick.Resolution.ProvisionResult.Ledger.WaterRemaining >= 0,
                    "D23 full loop tick " + tickIndex + " must preserve non-negative provisions.");
                D23ScenarioAssertions.Check(ref assertions,
                    lastTick.Resolution.SaveProjection != null,
                    "D23 full loop tick " + tickIndex + " must produce a D21 save projection.");
                AppendD21Events(fixture, lastTick.Resolution, ref assertions,
                    "voyage tick " + tickIndex);

                if (tickIndex == 0 || tickIndex == 20)
                {
                    D21ShipSaveProjection roundTrip = RoundTripSave(
                        lastTick.Resolution.SaveProjection, ref assertions,
                        "mid-run save " + tickIndex);
                    if (tickIndex == 20) midSaveFingerprint = roundTrip.ComputeFingerprint();
                }
                int expectedElapsed = tickIndex + 1;
                D23ScenarioAssertions.Check(ref assertions,
                    lastTick.Resolution.State != null &&
                    lastTick.Resolution.State.Time.ElapsedTicks == expectedElapsed,
                    "D23 full loop tick elapsed counter must be monotonic at " + tickIndex + ".");
                if (tickIndex < MainTickCount - 1)
                {
                    D23ScenarioAssertions.Check(ref assertions,
                        lastTick.Status == D21VoyageStatus.Sailing,
                        "D23 full loop must remain Sailing before the final tick.");
                }
                else
                {
                    D23ScenarioAssertions.Check(ref assertions,
                        lastTick.Status == D21VoyageStatus.Arrived &&
                        lastTick.Resolution.IsArrived,
                        "D23 full loop must reach Arrived at the deterministic route end.");
                }
                AdvanceClock(ref worldDay, ref minuteOfDay);
            }

            D23ScenarioAssertions.Check(ref assertions, lastTick != null,
                "D23 full loop must produce a final voyage tick.");
            D21VoyageResolutionState arrivedState = voyage.Service.ResolutionSystem.GetSnapshot(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions,
                arrivedState != null && arrivedState.Status == D21VoyageStatus.Arrived &&
                arrivedState.Time.ElapsedTicks == MainTickCount,
                "D23 full loop D21 owner snapshot must be Arrived and complete.");
            D21ShipSaveProjection arrivedSave = voyage.Service.ResolutionSystem.GetSaveProjection(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions, arrivedSave != null,
                "D23 full loop must expose the arrived D21 save projection.");
            if (!string.IsNullOrEmpty(midSaveFingerprint))
            {
                D23ScenarioAssertions.Check(ref assertions,
                    midSaveFingerprint != arrivedSave.ComputeFingerprint(),
                    "D23 full loop mid-run and final save revisions must be distinguishable.");
            }

            long returnRevision = voyage.StartRevision + MainTickCount + 1;
            var returnRequest = new D21PortReturnRequest(
                voyage.Tag + ":return-boundary",
                fixture.PlayerId,
                fixture.ShipId,
                fixture.PortId,
                worldDay,
                returnRevision,
                fixture.CreateClaims(),
                null,
                voyage.RouteId);
            D21VoyageCommandResult returned = voyage.Service.Route(new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.ReturnToPort,
                CommandId = voyage.Tag + ":return",
                ActorId = fixture.PlayerId,
                ShipId = fixture.ShipId,
                VoyageId = voyage.VoyageId,
                RouteId = voyage.RouteId,
                OriginPortId = voyage.Route.OriginId,
                DestinationPortId = voyage.Route.DestinationId,
                WorldDay = worldDay,
                MinuteOfDay = minuteOfDay,
                TickIndex = MainTickCount,
                SaveRevision = returnRevision,
                VoyageSeed = voyage.VoyageSeed,
                WeatherInput = CreateWeather(voyage.Tag, worldDay, returnRevision,
                    voyage.VoyageSeed),
                ReturnRequest = returnRequest,
            });
            D23ScenarioAssertions.Check(ref assertions,
                returned != null && returned.Accepted && returned.Status == D21VoyageStatus.Returned &&
                returned.ReturnResult != null && returned.ReturnResult.Success &&
                returned.ReturnResult.VoyageEnded && fixture.PortVisit.State == D19PortVisitState.Returned,
                "D23 full loop must complete D21 return only after D19 accepts Return: " +
                (returned == null ? "result=null" :
                    "accepted=" + returned.Accepted + ";status=" + returned.Status +
                    ";code=" + returned.Code + ";return=" +
                    (returned.ReturnResult == null ? "null" :
                        returned.ReturnResult.Success + ":" + returned.ReturnResult.FailureCode +
                        ":" + returned.ReturnResult.PortStateAfter) +
                    ";port=" + fixture.PortVisit.State));
            AppendEvent(fixture,
                returned.ReturnResult == null || returned.ReturnResult.ReturnResult == null
                    ? null : returned.ReturnResult.ReturnResult.EventDraft,
                ref assertions, "return boundary");
            AppendD21Events(fixture, returned.Resolution, ref assertions, "voyage return");
            ApplyPresence(fixture, D20PlayerPresenceState.Port,
                D20PlayerPresenceTransitionReason.ReturnToPort, "full-loop:presence:return", ref assertions);

            D21VoyageResolutionState finalState = voyage.Service.ResolutionSystem.GetSnapshot(voyage.VoyageId);
            D21ShipSaveProjection finalSave = voyage.Service.ResolutionSystem.GetSaveProjection(voyage.VoyageId);
            D21VoyageEventLogSnapshot finalD21Log = voyage.Service.EventRouter.GetLogSnapshot(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions,
                finalState != null && finalState.Status == D21VoyageStatus.Returned,
                "D23 full loop final D21 state must be Returned.");
            D23ScenarioAssertions.Check(ref assertions,
                finalSave != null && finalD21Log != null && finalD21Log.Events.Count > MainTickCount,
                "D23 full loop final D21 save/event projections must both be present.");
            finalD21Log.Validate();
            D21VoyageEventLog restoredD21Log = D21VoyageEventLog.Restore(finalD21Log);
            D23ScenarioAssertions.Check(ref assertions,
                restoredD21Log.Events.Count == finalD21Log.Events.Count,
                "D23 full loop D21 event log must restore without loss.");

            D19PortSaveProjection portSave = D19PortSaveProjectionMapper.BuildProjection(
                fixture.ShipId,
                fixture.PortId,
                fixture.PortVisit.State,
                fixture.PortVisit.BerthId,
                worldDay,
                returnRevision,
                string.Empty,
                0,
                fixture.Berths.GetAllocations(),
                new Dictionary<string, D19CrewLocation>(StringComparer.Ordinal),
                new List<D19PortServiceSaveEntry>(),
                new List<D19PendingCommandSaveEntry>(),
                new List<D19CompensationDraftSaveEntry>(),
                fixture.PortIdempotency.BuildSaveProjection());
            portSave.Validate();
            D19PortMigrationResult migrated = D19PortSaveMigration.Migrate(portSave, fixture.ShipId);
            D23ScenarioAssertions.Check(ref assertions,
                migrated != null && migrated.Succeeded && migrated.MigratedProjection != null,
                "D23 full loop D19 port save projection must migrate/read back successfully.");

            EventLogSnapshot sharedSnapshot = fixture.EventLog.CaptureSnapshot();
            EventLog restoredSharedLog = EventLog.FromSnapshot(sharedSnapshot);
            D23ScenarioAssertions.Check(ref assertions,
                restoredSharedLog.Count == fixture.EventLog.Count &&
                restoredSharedLog.NextSequence == fixture.EventLog.NextSequence,
                "D23 full loop shared D8 event log must restore without fact loss.");
            D23ScenarioAssertions.Check(ref assertions,
                restoredSharedLog.Facts.Count == fixture.EventLog.Facts.Count,
                "D23 full loop restored D8 fact read model must remain complete.");

            int cargoCount;
            fixture.TradeState.Inventory.Cargo.TryGetValue(fixture.CommodityId, out cargoCount);
            string saveFingerprint = finalSave.ComputeFingerprint();
            string eventFingerprint = FingerprintD21Events(finalD21Log.Events);
            string sharedFingerprint = FingerprintFacts(fixture.EventLog.Facts);
            string finalFingerprint = StableDigest(string.Join("|", new[]
            {
                saveFingerprint,
                eventFingerprint,
                sharedFingerprint,
                finalState.Status.ToString(),
                fixture.TradeState.Money.ToString(CultureInfo.InvariantCulture),
                cargoCount.ToString(CultureInfo.InvariantCulture),
                fixture.PlayerPresence.Snapshot.State.ToString(),
            }));

            return new D23FullLoopReport
            {
                Assertions = assertions,
                TickCount = MainTickCount,
                D19FactCount = fixture.EventLog.Count,
                D21EventCount = finalD21Log.Events.Count,
                SharedFactCount = restoredSharedLog.Count,
                Money = fixture.TradeState.Money,
                CargoCount = cargoCount,
                Arrived = arrivedState.Status == D21VoyageStatus.Arrived,
                Returned = finalState.Status == D21VoyageStatus.Returned,
                SaveFingerprint = saveFingerprint,
                EventFingerprint = eventFingerprint,
                FinalFingerprint = finalFingerprint,
            };
        }

        /// <summary>L29：单一长航程连续执行 50 个固定逻辑 Tick。</summary>
        public static int RunLongRun(string tag)
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            RequireGate(fixture, ref assertions);
            D23VoyageContext voyage = CreateAndStart(
                tag, 5000000d, LongRunBaseSpeedMilliKnots, false, fixture, ref assertions);
            D23ScenarioAssertions.Check(ref assertions,
                voyage.StartResult.Accepted && voyage.StartResult.Status == D21VoyageStatus.Sailing,
                "D23 long run must start a non-terminal voyage.");
            AppendD21Events(fixture, voyage.StartResult.Resolution, ref assertions, "long run start");

            int worldDay = voyage.StartWorldDay;
            int minuteOfDay = voyage.StartMinuteOfDay;
            long previousDistance = 0;
            var eventIds = new HashSet<string>(StringComparer.Ordinal);
            for (int tickIndex = 0; tickIndex < LongRunTickCount; tickIndex++)
            {
                long revision = voyage.StartRevision + tickIndex + 1;
                D21VoyageCommandResult result = voyage.Service.Route(new D21VoyageCommandRequest
                {
                    Kind = D21VoyageCommandKind.ResolveTick,
                    CommandId = tag + ":long-tick:" + tickIndex.ToString("D3", CultureInfo.InvariantCulture),
                    ActorId = fixture.PlayerId,
                    ShipId = fixture.ShipId,
                    VoyageId = voyage.VoyageId,
                    RouteId = voyage.RouteId,
                    WorldDay = worldDay,
                    MinuteOfDay = minuteOfDay,
                    TickIndex = tickIndex,
                    SaveRevision = revision,
                    VoyageSeed = voyage.VoyageSeed,
                    WeatherInput = CreateWeather(tag, worldDay, revision, voyage.VoyageSeed),
                });
                D23ScenarioAssertions.Check(ref assertions,
                    result != null && result.Accepted && !result.IsDuplicate &&
                    result.Status == D21VoyageStatus.Sailing && result.Resolution != null,
                    "D23 long run tick " + tickIndex + " must remain accepted and Sailing.");
                D23ScenarioAssertions.Check(ref assertions,
                    result.Resolution.State.Time.DistanceTravelledMilliNm > previousDistance &&
                    result.Resolution.ProvisionResult.Ledger.FoodRemaining >= 0 &&
                    result.Resolution.ProvisionResult.Ledger.WaterRemaining >= 0,
                    "D23 long run tick " + tickIndex + " must advance distance and preserve provisions.");
                previousDistance = result.Resolution.State.Time.DistanceTravelledMilliNm;
                foreach (D21VoyageEvent item in result.Resolution.EventResult.AppendedEvents)
                {
                    D23ScenarioAssertions.Check(ref assertions,
                        eventIds.Add(item.EventId + "\u001f" + item.SaveRevision.ToString(CultureInfo.InvariantCulture)),
                        "D23 long run event key must be unique at tick " + tickIndex + ".");
                }
                AppendD21Events(fixture, result.Resolution, ref assertions,
                    "long run tick " + tickIndex);
                if (tickIndex % 10 == 9)
                {
                    D21ShipSaveProjection projection = result.Resolution.SaveProjection;
                    D23ScenarioAssertions.Check(ref assertions,
                        projection != null && D21ShipSaveProjection.FromJson(projection.ToJson()).ComputeFingerprint() ==
                        projection.ComputeFingerprint(),
                        "D23 long run save round trip must remain stable at tick " + tickIndex + ".");
                }
                AdvanceClock(ref worldDay, ref minuteOfDay);
            }

            D21VoyageResolutionState final = voyage.Service.ResolutionSystem.GetSnapshot(voyage.VoyageId);
            D21VoyageEventLogSnapshot log = voyage.Service.EventRouter.GetLogSnapshot(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions,
                final != null && final.Status == D21VoyageStatus.Sailing &&
                final.Time.ElapsedTicks == LongRunTickCount,
                "D23 long run final state must remain healthy after 50 ticks.");
            D23ScenarioAssertions.Check(ref assertions,
                log != null && log.Events.Count == 1 + 4 * LongRunTickCount,
                "D23 long run event count must equal the owner-produced start plus four events per tick.");
            log.Validate();
            return assertions;
        }

        /// <summary>L28：跨 D8/D19/D21 的拒绝、重放、快照恢复矩阵。</summary>
        public static int RunInterruptionMatrix(string tag)
        {
            int assertions = 0;
            var fixture = D23ScenarioFixtureFactory.Create();
            RequireGate(fixture, ref assertions);

            D19PortVisitState initialState = fixture.PortVisit.State;
            D19RoutedCommandResult rejectedDeparture = fixture.PortRouter.Route(
                fixture.PortCommand(tag + ":invalid-departure", D19PortCommandKind.Depart,
                    fixture.BerthId));
            D23ScenarioAssertions.Check(ref assertions,
                rejectedDeparture != null && !rejectedDeparture.Success &&
                fixture.PortVisit.State == initialState && rejectedDeparture.EventDraft != null,
                "D23 interruption matrix invalid D19 departure must leave lifecycle unchanged.");
            AppendEvent(fixture, rejectedDeparture.EventDraft, ref assertions,
                "interruption invalid departure");

            D23VoyageContext voyage = CreateAndStart(
                tag + ":voyage", 5000000d, LongRunBaseSpeedMilliKnots, false,
                fixture, ref assertions);
            AppendD21Events(fixture, voyage.StartResult.Resolution, ref assertions,
                "interruption start");
            D21VoyageResolutionState beforeInvalidTick =
                voyage.Service.ResolutionSystem.GetSnapshot(voyage.VoyageId);
            string beforeInvalidSave = voyage.Service.ResolutionSystem.GetSaveProjection(
                voyage.VoyageId).ComputeFingerprint();
            D21VoyageCommandResult invalidTick = voyage.Service.Route(new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.ResolveTick,
                CommandId = tag + ":invalid-tick",
                ActorId = fixture.PlayerId,
                ShipId = fixture.ShipId,
                VoyageId = voyage.VoyageId,
                RouteId = voyage.RouteId,
                WorldDay = voyage.StartWorldDay,
                MinuteOfDay = voyage.StartMinuteOfDay,
                TickIndex = 1,
                SaveRevision = voyage.StartRevision + 1,
                VoyageSeed = voyage.VoyageSeed,
                WeatherInput = CreateWeather(tag, voyage.StartWorldDay,
                    voyage.StartRevision + 1, voyage.VoyageSeed),
            });
            D23ScenarioAssertions.Check(ref assertions,
                invalidTick != null && !invalidTick.Accepted &&
                invalidTick.Code == "d21.voyage.tick_conflict",
                "D23 interruption matrix invalid tick must be rejected by D21 owner.");
            D21VoyageResolutionState afterInvalidTick =
                voyage.Service.ResolutionSystem.GetSnapshot(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions,
                afterInvalidTick.Time.ElapsedTicks == beforeInvalidTick.Time.ElapsedTicks &&
                voyage.Service.ResolutionSystem.GetSaveProjection(voyage.VoyageId).ComputeFingerprint() ==
                beforeInvalidSave,
                "D23 interruption matrix invalid tick must not mutate D21 state or save.");

            D21VoyageCommandRequest firstTick = BuildTickRequest(
                voyage, tag + ":tick-0", 0, voyage.StartWorldDay,
                voyage.StartMinuteOfDay, voyage.StartRevision + 1);
            D21VoyageCommandResult first = voyage.Service.Route(firstTick);
            D21VoyageEventLogSnapshot afterFirst = voyage.Service.EventRouter.GetLogSnapshot(voyage.VoyageId);
            D21VoyageCommandResult replay = voyage.Service.Route(firstTick.Clone());
            D21VoyageEventLogSnapshot afterReplay = voyage.Service.EventRouter.GetLogSnapshot(voyage.VoyageId);
            D23ScenarioAssertions.Check(ref assertions,
                first.Accepted && replay.Accepted && replay.IsDuplicate &&
                afterReplay.Events.Count == afterFirst.Events.Count &&
                replay.Resolution.ProvisionResult.Ledger.TotalFoodConsumed ==
                first.Resolution.ProvisionResult.Ledger.TotalFoodConsumed,
                "D23 interruption matrix D21 tick replay must not consume or emit twice.");
            AppendD21Events(fixture, first.Resolution, ref assertions, "interruption first tick");

            EventLogSnapshot eventSnapshot = fixture.EventLog.CaptureSnapshot();
            int beforeDuplicateFactCount = fixture.EventLog.Count;
            FactEvent lastFact = fixture.EventLog.Facts[fixture.EventLog.Facts.Count - 1];
            EventAppendResult duplicateFact = fixture.EventLog.Append(lastFact);
            D23ScenarioAssertions.Check(ref assertions,
                duplicateFact.Succeeded && duplicateFact.IsDuplicate &&
                fixture.EventLog.Count == beforeDuplicateFactCount,
                "D23 interruption matrix duplicate D8 append must not create a second fact.");
            EventLog restored = EventLog.FromSnapshot(eventSnapshot);
            D23ScenarioAssertions.Check(ref assertions,
                restored.Count == beforeDuplicateFactCount &&
                restored.NextSequence == eventSnapshot.NextSequence,
                "D23 interruption matrix D8 snapshot restore must preserve the cursor.");

            D21VoyageEventLogSnapshot d21Snapshot = voyage.Service.EventRouter.GetLogSnapshot(voyage.VoyageId);
            D21VoyageEventLog restoredD21 = D21VoyageEventLog.Restore(d21Snapshot);
            D23ScenarioAssertions.Check(ref assertions,
                restoredD21.Events.Count == d21Snapshot.Events.Count,
                "D23 interruption matrix D21 event snapshot restore must preserve history.");
            return assertions;
        }

        /// <summary>建立一个已通过 D21 Start 的确定性上下文。</summary>
        public static D23VoyageContext CreateAndStart(
            string tag,
            double distanceNm,
            int baseSpeedMilliKnots,
            bool usePortBoundary,
            D23ScenarioFixture fixture,
            ref int assertions)
        {
            D23ScenarioAssertions.Check(ref assertions, fixture != null,
                "D23 voyage context requires a fixture.");
            if (usePortBoundary)
            {
                D19RoutedCommandResult enter = fixture.PortRouter.Route(
                    fixture.PortCommand(tag + ":enter", D19PortCommandKind.EnterPort));
                D19RoutedCommandResult dock = fixture.PortRouter.Route(
                    fixture.PortCommand(tag + ":dock", D19PortCommandKind.Dock));
                D19RoutedCommandResult berth = fixture.PortRouter.Route(
                    fixture.PortCommand(tag + ":berth", D19PortCommandKind.AssignBerth, fixture.BerthId));
                D19RoutedCommandResult board = fixture.PortRouter.Route(
                    fixture.PortCommand(tag + ":board", D19PortCommandKind.Board, fixture.BerthId));
                D23ScenarioAssertions.Check(ref assertions,
                    enter.Success && dock.Success && berth.Success && board.Success &&
                    fixture.PortVisit.State == D19PortVisitState.Boarded,
                    "D23 voyage context must reach Boarded before the D21 departure adapter.");
            }

            string voyageId = "voyage_d23_" + tag;
            string routeId = "route_d23_" + tag;
            long startRevision = fixture.SaveRevision + 1;
            int startMinute = 8 * 60;
            long voyageSeed = fixture.Seed;
            var route = CreateRoute(routeId, fixture.PortId, "port_d23_destination_" + tag, distanceNm);
            var departureAdapter = usePortBoundary
                ? new D21PortDepartureAdapter(fixture.PortRouter) : null;
            var returnAdapter = usePortBoundary
                ? new D21PortReturnAdapter(fixture.PortRouter) : null;
            var service = new D21VoyageCommandService(null, departureAdapter, returnAdapter);
            D21WeatherInput weather = CreateWeather(tag, fixture.WorldDay, startRevision, voyageSeed);
            var request = new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.Start,
                CommandId = tag + ":start",
                ActorId = fixture.PlayerId,
                ShipId = fixture.ShipId,
                VoyageId = voyageId,
                RouteId = routeId,
                OriginPortId = route.OriginId,
                DestinationPortId = route.DestinationId,
                WorldDay = fixture.WorldDay,
                MinuteOfDay = startMinute,
                TickIndex = 0,
                SaveRevision = startRevision,
                VoyageSeed = voyageSeed,
                BaseSpeedMilliKnots = baseSpeedMilliKnots,
                CurrentDraftMeters = 1,
                HazardTolerance = 100,
                KnownDiscoveryState = D21RouteDiscoveryState.Discovered,
                ShipIdentity = fixture.ShipIdentity.Clone(),
                HullConfiguration = fixture.HullConfiguration.Clone(),
                ConditionState = fixture.ShipCondition.Snapshot(),
                CrewDecision = CreateCrewDecision(tag),
                AnchorState = CreateAnchor(fixture.ShipId, startRevision),
                RouteDefinition = route,
                WeatherInput = weather,
                TimeState = D21VoyageTimeState.CreateInitial(
                    voyageId, fixture.ShipId,
                    checked((long)Math.Round(distanceNm * 1000d, MidpointRounding.AwayFromZero)),
                    fixture.WorldDay, startMinute, startRevision),
                CourseState = CreateCourse(voyageId, fixture.ShipId),
                ProvisionRequest = new D21ProvisionConsumptionRequest
                {
                    VoyageId = voyageId,
                    ShipId = fixture.ShipId,
                    ApplicationId = tag + ":start-provision",
                    WorldDay = fixture.WorldDay,
                    SaveRevision = startRevision,
                    TickIndex = 0,
                    CrewCount = 1,
                    FoodUnits = InitialProvisionUnits,
                    WaterUnits = InitialProvisionUnits,
                    FoodUnitsPerCrew = 1,
                    WaterUnitsPerCrew = 1,
                },
            };
            if (usePortBoundary)
            {
                request.DepartureRequest = new D21PortDepartureRequest(
                    tag + ":departure",
                    fixture.PlayerId,
                    fixture.ShipId,
                    fixture.PortId,
                    fixture.WorldDay,
                    startRevision,
                    fixture.CreateClaims(),
                    routeId,
                    route.DestinationId,
                    fixture.BerthId);
            }
            D21VoyageCommandResult started = service.Route(request);
            D23ScenarioAssertions.Check(ref assertions,
                started != null && started.Accepted && started.Resolution != null,
                "D23 voyage context D21 Start must be accepted: " +
                (started == null ? "null" : started.Code));
            return new D23VoyageContext(
                fixture, service, tag, voyageId, routeId, startRevision,
                fixture.WorldDay, startMinute, voyageSeed, route, started);
        }

        private static void RequireGate(D23ScenarioFixture fixture, ref int assertions)
        {
            bool allowed = D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            D23ScenarioAssertions.Check(ref assertions, allowed,
                "D23 full-loop execution is fail-closed when an upstream logic result is absent.");
        }

        private static void RouteAndAppend(
            D23ScenarioFixture fixture,
            D19RoutedPortCommand command,
            ref int assertions,
            string label)
        {
            D19RoutedCommandResult result = fixture.PortRouter.Route(command);
            D23ScenarioAssertions.Check(ref assertions,
                result != null && result.Success && result.EventDraft != null,
                "D23 full loop D19 " + label + " must succeed and emit a draft.");
            AppendEvent(fixture, result == null ? null : result.EventDraft,
                ref assertions, label);
        }

        private static void AppendEvent(
            D23ScenarioFixture fixture,
            WorldEvent draft,
            ref int assertions,
            string label)
        {
            D23ScenarioAssertions.Check(ref assertions, draft != null,
                "D23 " + label + " must expose a valid D8 event draft.");
            if (draft == null) return;
            EventAppendResult appended = fixture.EventLog.Append(draft);
            D23ScenarioAssertions.Check(ref assertions,
                appended != null && appended.Succeeded,
                "D23 " + label + " event draft must append to D8.");
        }

        private static void AppendD21Events(
            D23ScenarioFixture fixture,
            D21VoyageResolutionResult resolution,
            ref int assertions,
            string label)
        {
            D23ScenarioAssertions.Check(ref assertions,
                resolution != null && resolution.EventResult != null &&
                resolution.EventResult.AppendedEvents != null &&
                resolution.EventResult.AppendedEvents.Count > 0,
                "D23 " + label + " must expose newly appended D21 events.");
            if (resolution == null || resolution.EventResult == null ||
                resolution.EventResult.AppendedEvents == null) return;
            foreach (D21VoyageEvent item in resolution.EventResult.AppendedEvents)
            {
                D23ScenarioAssertions.Check(ref assertions, item != null,
                    "D23 " + label + " cannot contain a null D21 event.");
                if (item == null) continue;
                WorldEvent shared = item.ToWorldEvent();
                EventAppendResult appended = fixture.EventLog.Append(shared);
                D23ScenarioAssertions.Check(ref assertions,
                    appended != null && appended.Succeeded,
                    "D23 " + label + " D21 event must bridge into D8.");
            }
        }

        private static void ApplyPresence(
            D23ScenarioFixture fixture,
            D20PlayerPresenceState desired,
            D20PlayerPresenceTransitionReason reason,
            string commandId,
            ref int assertions)
        {
            D20PlayerPresenceResult result = fixture.PlayerPresence.Apply(new D20PlayerPresenceRequest
            {
                CommandId = commandId,
                PlayerId = fixture.PlayerId,
                DesiredState = desired,
                Reason = reason,
                PortId = desired == D20PlayerPresenceState.Port ? fixture.PortId : string.Empty,
                ShipId = desired == D20PlayerPresenceState.Ship ||
                    desired == D20PlayerPresenceState.Voyage ? fixture.ShipId : string.Empty,
                WorldDay = fixture.WorldDay,
                SaveRevision = fixture.SaveRevision,
            });
            D23ScenarioAssertions.Check(ref assertions,
                result != null && result.Accepted && result.State != null &&
                result.State.State == desired,
                "D23 full loop D20 presence transition must reach " + desired + ".");
        }

        private static D21ShipSaveProjection RoundTripSave(
            D21ShipSaveProjection projection,
            ref int assertions,
            string label)
        {
            D23ScenarioAssertions.Check(ref assertions, projection != null,
                "D23 " + label + " requires a D21 save projection.");
            if (projection == null) return null;
            string json = projection.ToJson();
            D21ShipSaveProjection restored = D21ShipSaveProjection.FromJson(json);
            D23ScenarioAssertions.Check(ref assertions,
                restored != null && restored.ComputeFingerprint() == projection.ComputeFingerprint(),
                "D23 " + label + " save projection JSON round trip must be deterministic.");
            return restored;
        }

        private static D21VoyageCommandRequest BuildTickRequest(
            D23VoyageContext voyage,
            string commandId,
            int tickIndex,
            int worldDay,
            int minuteOfDay,
            long saveRevision)
        {
            return new D21VoyageCommandRequest
            {
                Kind = D21VoyageCommandKind.ResolveTick,
                CommandId = commandId,
                ActorId = voyage.Fixture.PlayerId,
                ShipId = voyage.Fixture.ShipId,
                VoyageId = voyage.VoyageId,
                RouteId = voyage.RouteId,
                WorldDay = worldDay,
                MinuteOfDay = minuteOfDay,
                TickIndex = tickIndex,
                SaveRevision = saveRevision,
                VoyageSeed = voyage.VoyageSeed,
                WeatherInput = CreateWeather(voyage.Tag, worldDay, saveRevision,
                    voyage.VoyageSeed),
            };
        }

        private static D21NavigationRouteDefinition CreateRoute(
            string routeId,
            string originId,
            string destinationId,
            double distanceNm)
        {
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
                WaypointId = routeId + ":origin",
                LocationRef = originId,
                Sequence = 0,
                DistanceFromOriginNm = 0,
                MinimumDepthMeters = 3,
                DiscoveryRequired = false,
                HazardZoneIds = new List<string>(),
            });
            route.Waypoints.Add(new D21RouteWaypoint
            {
                WaypointId = routeId + ":destination",
                LocationRef = destinationId,
                Sequence = 1,
                DistanceFromOriginNm = distanceNm,
                MinimumDepthMeters = 3,
                DiscoveryRequired = false,
                HazardZoneIds = new List<string>(),
            });
            return route;
        }

        private static D21CourseState CreateCourse(string voyageId, string shipId)
        {
            D21CourseState course = D21CourseState.CreateInitial(voyageId, shipId);
            course.Mode = D21CourseMode.Course;
            course.HasCourse = true;
            course.IsStopped = false;
            course.HeadingCentidegrees = 0;
            course.SpeedPermille = 1000;
            course.CooldownUntilTick = 0;
            course.Validate();
            return course;
        }

        private static D21AnchorMooringState CreateAnchor(string shipId, long saveRevision)
        {
            return new D21AnchorMooringState
            {
                SchemaVersion = D21AnchorMooringSystem.CurrentSchemaVersion,
                ShipId = shipId,
                AnchorState = D21AnchorState.Unanchored,
                MooringState = D21MooringState.Unmoored,
                ChainDeployedMeters = 0,
                DragRisk = 0,
                SaveRevision = checked((int)saveRevision),
                LastEventId = string.Empty,
            };
        }

        private static D21CrewRequirementDecision CreateCrewDecision(string tag)
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
                    new D21CrewRoleAssignment
                    {
                        Role = D21CrewRole.Navigator,
                        CrewId = "crew:d23:" + tag,
                    },
                }),
            };
        }

        private static D21WeatherInput CreateWeather(
            string tag,
            int worldDay,
            long saveRevision,
            long voyageSeed)
        {
            return new D21WeatherInput
            {
                EventId = "weather:d23:" + tag + ":" + worldDay.ToString(CultureInfo.InvariantCulture) +
                    ":" + saveRevision.ToString(CultureInfo.InvariantCulture),
                SourceId = "d9:d23",
                LocationRef = "location:d23:open-sea",
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Seed = voyageSeed,
                Season = D21WeatherSeason.Dry,
                WindDirection = D21WindDirection.Calm,
                WindForce = 0,
                PrecipitationMmPerHour = 0,
                VisibilityFraction = 1,
                PressureHpa = 1013.25f,
            };
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

        private static string FingerprintD21Events(IReadOnlyList<D21VoyageEvent> events)
        {
            var builder = new StringBuilder();
            if (events != null)
            {
                foreach (D21VoyageEvent item in events)
                {
                    if (item == null) continue;
                    builder.Append(item.EventId).Append('|')
                        .Append(item.EventType).Append('|')
                        .Append(item.TickIndex.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(item.SaveRevision.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(item.LogSequence.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(item.Result == null ? string.Empty : item.Result.ResultCode).Append(';');
                }
            }
            return StableDigest(builder.ToString());
        }

        private static string FingerprintFacts(IReadOnlyList<FactEvent> facts)
        {
            var builder = new StringBuilder();
            if (facts != null)
            {
                foreach (FactEvent item in facts)
                {
                    if (item == null) continue;
                    builder.Append(item.EventId).Append('|')
                        .Append(item.SaveRevision.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(item.LogSequence.ToString(CultureInfo.InvariantCulture)).Append('|')
                        .Append(item.OwnerSystem).Append('|')
                        .Append(item.OutcomeStatus).Append('|')
                        .Append(item.Result == null ? string.Empty : item.Result.ResultCode).Append(';');
                }
            }
            return StableDigest(builder.ToString());
        }

        private static string StableDigest(string value)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                string text = value ?? string.Empty;
                for (int index = 0; index < text.Length; index++)
                {
                    hash ^= text[index];
                    hash *= 1099511628211UL;
                }
                return hash.ToString("X16", CultureInfo.InvariantCulture);
            }
        }
    }
}
