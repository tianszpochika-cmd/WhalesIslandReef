// D23ScenarioFixtureFactory —— WhalesIslandReef D23 固定种子跨系统夹具
//
// 这是 D23-L02 的纯逻辑测试载体：只组装各领域已经公开的 owner API，
// 不复制或直接写入港口、货币、库存、关系、船况、生态和存档事实。
// D21-L32 / D22-L36 的真实门禁结果必须先存在；当前快照由 D23 收口任务维护，
// 下游场景仍通过 fail-closed gate，不能把“可构造”误报为“跨系统已通过”。
// ============================================================

using System;
using Whalebone.Systems.D19;
using Whalebone.Systems.D20;
using Whalebone.Systems.D21;
using Whalebone.Systems.D22;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.D23
{
    /// <summary>
    /// D23 上游门禁快照。它是本次夹具的显式测试输入，不读取文件系统或运行时状态。
    /// 当上游交付真实结果后，必须由新的 D23 任务更新此快照并重新运行场景。
    /// </summary>
    public sealed class D23DependencyGateSnapshot
    {
        public const string Pass = "LOGIC_PASS";
        public const string Missing = "MISSING_RESULT";

        public string D18L50 = Pass;
        public string D19L48 = Pass;
        public string D20L40 = Pass;
        public string D21L32 = Pass;
        public string D22L36 = Pass;

        public bool CanRunCrossSystem
        {
            get
            {
                return string.Equals(D18L50, Pass, StringComparison.Ordinal) &&
                    string.Equals(D19L48, Pass, StringComparison.Ordinal) &&
                    string.Equals(D20L40, Pass, StringComparison.Ordinal) &&
                    string.Equals(D21L32, Pass, StringComparison.Ordinal) &&
                    string.Equals(D22L36, Pass, StringComparison.Ordinal);
            }
        }

        public string BlockerCode
        {
            get
            {
                if (CanRunCrossSystem) return string.Empty;
                return "D23_UPSTREAM_GATE_D21_L32_D22_L36";
            }
        }

        public string Summary
        {
            get
            {
                return "D18-L50=" + D18L50 + "; D19-L48=" + D19L48 +
                    "; D20-L40=" + D20L40 + "; D21-L32=" + D21L32 +
                    "; D22-L36=" + D22L36;
            }
        }
    }

    /// <summary>一次 D23 场景共享的确定性领域快照。</summary>
    public sealed class D23ScenarioFixture
    {
        public int Seed { get; internal set; }
        public int WorldDay { get; internal set; }
        public long SaveRevision { get; internal set; }
        public D23DependencyGateSnapshot Dependencies { get; internal set; }

        public string PlayerId { get; internal set; }
        public string ShipId { get; internal set; }
        public string PortId { get; internal set; }
        public string BerthId { get; internal set; }
        public string CommodityId { get; internal set; }

        // 这些引用均为公开 owner；场景代码只能经其公开命令/读取 API 操作。
        public EventLog EventLog { get; internal set; }
        public D3TradeState TradeState { get; internal set; }
        public TradeEconomySystem Economy { get; internal set; }
        public D19PortAnchorCatalog PortCatalog { get; internal set; }
        public D19PortVisitSystem PortVisit { get; internal set; }
        public D19BerthSystem Berths { get; internal set; }
        public D19CommandIdempotency PortIdempotency { get; internal set; }
        public D19PortCommandRouter PortRouter { get; internal set; }
        public D20CharacterIdentity PlayerIdentity { get; internal set; }
        public D20PlayerPresenceSystem PlayerPresence { get; internal set; }
        public D21ShipIdentity ShipIdentity { get; internal set; }
        public D21HullComponentConfiguration HullConfiguration { get; internal set; }
        public D21ShipConditionSystem ShipCondition { get; internal set; }
        public D21WeatherInput Weather { get; internal set; }
        public D21VoyageTimeState VoyageTime { get; internal set; }
        public D19PortSaveProjection PortSave { get; internal set; }
        public D21ShipSaveProjection ShipSave { get; internal set; }
        public D22UiCommandContext UiContext { get; internal set; }

        internal D23ScenarioFixture() { }

        /// <summary>构造请求级权限声明，不推断登录态或外部场景事实。</summary>
        public D19AuthorityClaims CreateClaims()
        {
            return new D19AuthorityClaims(ShipId, true, false, PortId);
        }

        /// <summary>构造 D19 唯一路由器可消费的命令信封。</summary>
        public D19RoutedPortCommand PortCommand(
            string commandId,
            D19PortCommandKind kind,
            string berthId = null,
            string serviceId = null,
            long? saveRevision = null)
        {
            var command = new D19RoutedPortCommand(
                commandId,
                kind,
                PlayerId,
                ShipId,
                PortId,
                berthId,
                serviceId,
                WorldDay,
                saveRevision ?? SaveRevision,
                CreateClaims());
            command.ActorIds = new[] { PlayerId };
            return command;
        }
    }

    /// <summary>D23-L02 的唯一夹具入口。</summary>
    public static class D23ScenarioFixtureFactory
    {
        public const int FixedSeed = 2301;
        public const int FixedWorldDay = 23;
        public const long FixedSaveRevision = 230100;
        public const string FixedPlayerId = "player_d23_001";
        public const string FixedShipId = "ship_d23_sloop_001";
        public const string FixedPortId = "port_santa_margarita";
        public const string FixedBerthId = "berth_santa_margarita_dock_01";
        public const string FixedCommodityId = "sugar";

        public static D23ScenarioFixture Create()
        {
            var fixture = new D23ScenarioFixture
            {
                Seed = FixedSeed,
                WorldDay = FixedWorldDay,
                SaveRevision = FixedSaveRevision,
                Dependencies = new D23DependencyGateSnapshot(),
                PlayerId = FixedPlayerId,
                ShipId = FixedShipId,
                PortId = FixedPortId,
                BerthId = FixedBerthId,
                CommodityId = FixedCommodityId,
                EventLog = new EventLog(),
                TradeState = new D3TradeState(5000, 40),
                PortCatalog = D19PortAnchorCatalog.Instance,
                PortVisit = new D19PortVisitSystem(FixedShipId),
                Berths = new D19BerthSystem(D19PortAnchorCatalog.Instance),
                PortIdempotency = new D19CommandIdempotency(FixedShipId),
                PlayerIdentity = D20CharacterIdentityContracts.CreateDefault(
                    FixedPlayerId,
                    D20CharacterKind.Player,
                    "d18.player.d23",
                    "d23.fixture"),
                PlayerPresence = new D20PlayerPresenceSystem(),
                ShipIdentity = D21ShipIdentityContracts.CreateDefault(
                    FixedShipId,
                    D21ShipTypeIds.Sloop,
                    FixedPlayerId,
                    "D23 Sloop"),
                Weather = D21WeatherInput.CreateDefault(),
                PortSave = D19PortSaveProjection.CreateDefault(FixedShipId),
                ShipSave = D21ShipSaveProjection.CreateDefault(FixedShipId),
                UiContext = new D22UiCommandContext
                {
                    ActiveScreenId = D22ScreenIds.MainMenu,
                    ActivePanelId = D22PanelIds.MainMenu,
                    FocusedControlId = D22FocusControlIds.MainMenuNewGame,
                    ActorId = FixedPlayerId,
                    WorldDay = FixedWorldDay,
                    SaveRevision = FixedSaveRevision,
                    SessionActive = true,
                    ContextValid = true,
                    PermissionGranted = true,
                    CommandEnabled = true,
                },
            };

            fixture.Economy = new TradeEconomySystem(fixture.EventLog);
            CommodityCatalog.SeedMarket(fixture.TradeState.Market);

            fixture.ShipIdentity.HomePortId = FixedPortId;
            fixture.ShipIdentity.BuildPortId = FixedPortId;

            fixture.HullConfiguration = D21HullComponentSystem.CreateStarterConfiguration(
                FixedShipId,
                D21ShipTypeIds.Sloop);
            fixture.ShipCondition = new D21ShipConditionSystem(
                D21ShipConditionSystem.CreateHealthyState(FixedShipId, 100d, 20d));

            fixture.Weather.EventId = "d23.weather.2301";
            fixture.Weather.SourceId = "d9_environment";
            fixture.Weather.LocationRef = FixedPortId;
            fixture.Weather.WorldDay = FixedWorldDay;
            fixture.Weather.SaveRevision = FixedSaveRevision;
            fixture.Weather.Seed = FixedSeed;
            fixture.Weather.WindDirection = D21WindDirection.Calm;
            fixture.Weather.WindForce = 0f;
            fixture.Weather.PrecipitationMmPerHour = 0f;
            fixture.Weather.VisibilityFraction = 1f;
            fixture.Weather.PressureHpa = 1013.25f;

            fixture.VoyageTime = D21VoyageTimeState.CreateInitial(
                "voyage_d23_001",
                FixedShipId,
                120000,
                FixedWorldDay,
                8 * 60,
                FixedSaveRevision);

            ValidateFixture(fixture);
            fixture.PortRouter = new D19PortCommandRouter(
                fixture.PortCatalog,
                fixture.PortVisit,
                fixture.Berths,
                fixture.PortIdempotency);
            return fixture;
        }

        private static void ValidateFixture(D23ScenarioFixture fixture)
        {
            if (fixture == null) throw new InvalidOperationException("D23 fixture is null.");
            D23ScenarioAssertions.Check(
                fixture.Dependencies != null,
                "D23 dependency snapshot is required.");
            D23ScenarioAssertions.Check(
                fixture.PortCatalog.ResolvePort(fixture.PortId).Success,
                "D23 fixture port must resolve through D19 anchor catalog.");
            D23ScenarioAssertions.Check(
                fixture.PortCatalog.ResolveBerth(fixture.BerthId, fixture.PortId).Success,
                "D23 fixture berth must resolve through D19 anchor catalog.");
            D23ScenarioAssertions.Check(
                fixture.PortCatalog.ResolveService(
                    "service_santa_margarita_market", fixture.PortId).Success,
                "D23 fixture market service must resolve through D19 anchor catalog.");
            D23ScenarioAssertions.Check(
                D20CharacterIdentityContracts.IsStableCharacterId(fixture.PlayerId),
                "D23 fixture player ID must be stable.");
            fixture.PlayerIdentity.Validate();
            D23ScenarioAssertions.Check(
                D21ShipIdentityContracts.Validate(fixture.ShipIdentity).Valid,
                "D23 fixture ship identity must validate.");
            D23ScenarioAssertions.Check(
                D21HullComponentSystem.ValidateConfiguration(fixture.HullConfiguration).Valid,
                "D23 fixture hull configuration must validate.");
            fixture.Weather.Validate();
            fixture.VoyageTime.Validate();
            fixture.PortSave.Validate();
            fixture.ShipSave.Validate();
        }
    }

    /// <summary>所有 D23 场景复用的纯逻辑断言/门禁辅助。</summary>
    public static class D23ScenarioAssertions
    {
        public static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        public static void Check(ref int count, bool condition, string message)
        {
            count++;
            Check(condition, message);
        }

        /// <summary>
        /// 只有所有上游 gate 通过时才允许运行 D23 跨系统断言。
        /// 当前返回 false 是预期的阻断证据，不是测试成功信号。
        /// </summary>
        public static bool RequireCrossSystemGate(D23ScenarioFixture fixture, ref int count)
        {
            Check(ref count, fixture != null && fixture.Dependencies != null,
                "D23 dependency snapshot missing.");
            if (fixture.Dependencies.CanRunCrossSystem) return true;
            Check(ref count,
                string.Equals(fixture.Dependencies.BlockerCode,
                    "D23_UPSTREAM_GATE_D21_L32_D22_L36", StringComparison.Ordinal),
                "D23 blocked gate must expose stable blocker code.");
            return false;
        }

        public static D21VoyageTimeResult AdvanceOneTick(D23ScenarioFixture fixture)
        {
            Check(fixture != null, "D23 fixture required for voyage tick.");
            var input = new D21VoyageTimeInput
            {
                RouteDistanceMilliNm = fixture.VoyageTime.RouteDistanceMilliNm,
                BaseSpeedMilliKnots = 12 * D21VoyageTimeSystem.MilliUnitsPerKnot,
                SpeedPermille = 1000,
                CurrentPermille = 1000,
                TickMinutes = D21VoyageTimeSystem.FixedTickMinutes,
            };
            var context = D21VoyageTimeSystem.CreateTickContext(
                fixture.WorldDay,
                fixture.VoyageTime.StartMinuteOfDay,
                fixture.VoyageTime.ElapsedTicks,
                fixture.SaveRevision,
                fixture.Seed,
                fixture.VoyageTime.VoyageId,
                fixture.ShipId);
            return D21VoyageTimeSystem.Advance(fixture.VoyageTime, input, context);
        }

        public static void ReachBerthed(D23ScenarioFixture fixture, ref int count)
        {
            Check(fixture != null, "D23 fixture required for berth setup.");
            var enter = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.enter", D19PortCommandKind.EnterPort));
            Check(ref count, enter.Success && enter.StateAfter == D19PortVisitState.PortVisit,
                "D23 setup enter port.");
            var dock = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.dock", D19PortCommandKind.Dock));
            Check(ref count, dock.Success && dock.StateAfter == D19PortVisitState.Docked,
                "D23 setup dock.");
            var berth = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.berth", D19PortCommandKind.AssignBerth, fixture.BerthId));
            Check(ref count, berth.Success && berth.StateAfter == D19PortVisitState.Berthed,
                "D23 setup assign berth.");
        }

        public static void ReachBoarded(D23ScenarioFixture fixture, ref int count)
        {
            ReachBerthed(fixture, ref count);
            var boarded = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.board", D19PortCommandKind.Board, fixture.BerthId));
            Check(ref count, boarded.Success && boarded.StateAfter == D19PortVisitState.Boarded,
                "D23 setup board.");
        }

        public static void ReachDeparted(D23ScenarioFixture fixture, ref int count)
        {
            ReachBoarded(fixture, ref count);
            var departed = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.depart", D19PortCommandKind.Depart, fixture.BerthId));
            Check(ref count, departed.Success && departed.StateAfter == D19PortVisitState.Departed,
                "D23 setup depart.");
        }
    }
}
