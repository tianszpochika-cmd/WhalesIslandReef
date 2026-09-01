// D24-L18：逻辑状态/枚举覆盖入口（纯 C#；枚举清单不等于运行时覆盖）。

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Whalebone.Systems.D19;
using Whalebone.Systems.D21;
using Whalebone.Systems.D22;
using Whalebone.Tests.D23;

namespace Whalebone.Tests.D24
{
    public static class D24StateCoverageTests
    {
        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            int failed = 0;
            failed += Check("D24-L18 declared enum inventory is non-empty", DeclaredEnumInventory);
            failed += Check("D24-L18 D19 transition failures are explicit", PortFailureTransitionIsExplicit);
            failed += Check("D24-L18 D21 time boundaries and rejection are explicit", VoyageBoundaryAndRejection);
            failed += Check("D24-L18 D22 command shape and owner route are explicit", UiCommandCoverage);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception error)
            {
                Console.WriteLine("[EX] " + name + " : " + error.Message);
                return 1;
            }
        }

        private static bool DeclaredEnumInventory()
        {
            var assemblies = new[]
            {
                typeof(D19PortVisitState).Assembly,
                typeof(D21DayNightPhase).Assembly,
                typeof(D22UiCommandKind).Assembly,
            }.Distinct();
            int enumCount = 0;
            int valueCount = 0;
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes().Where(item => item.IsEnum &&
                    item.Namespace != null && item.Namespace.StartsWith("Whalebone.Systems.", StringComparison.Ordinal)))
                {
                    Array values = Enum.GetValues(type);
                    if (values.Length == 0) return false;
                    enumCount++;
                    valueCount += values.Length;
                    foreach (object value in values)
                        if (!Enum.IsDefined(type, value)) return false;
                }
            }
            Console.WriteLine("    enum_inventory=" + enumCount + "; enum_values=" + valueCount);
            return enumCount >= 10 && valueCount >= enumCount;
        }

        private static bool PortFailureTransitionIsExplicit()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            var invalid = fixture.PortRouter.Route(fixture.PortCommand("d24.l18.invalid_dock", D19PortCommandKind.Dock));
            var enter = fixture.PortRouter.Route(fixture.PortCommand("d24.l18.enter", D19PortCommandKind.EnterPort));
            var serviceBeforeBerth = fixture.PortRouter.Route(fixture.PortCommand(
                "d24.l18.invalid_service", D19PortCommandKind.StartService, null,
                "service_santa_margarita_market"));
            return !invalid.Success && !string.IsNullOrEmpty(invalid.ReasonCode)
                && enter.Success && enter.StateAfter == D19PortVisitState.PortVisit
                && !serviceBeforeBerth.Success && !string.IsNullOrEmpty(serviceBeforeBerth.ReasonCode);
        }

        private static bool VoyageBoundaryAndRejection()
        {
            return D21VoyageTimeSystem.GetPhase(359) == D21DayNightPhase.Night
                && D21VoyageTimeSystem.GetPhase(360) == D21DayNightPhase.Dawn
                && D21VoyageTimeSystem.GetPhase(419) == D21DayNightPhase.Dawn
                && D21VoyageTimeSystem.GetPhase(420) == D21DayNightPhase.Day
                && D21VoyageTimeSystem.GetPhase(1079) == D21DayNightPhase.Day
                && D21VoyageTimeSystem.GetPhase(1080) == D21DayNightPhase.Dusk
                && D21VoyageTimeSystem.GetPhase(1140) == D21DayNightPhase.Night
                && VoyageTickMismatchIsRejected();
        }

        private static bool VoyageTickMismatchIsRejected()
        {
            var state = D21VoyageTimeState.CreateInitial("d24.voyage", "ship_d24", 10000, 24, 600, 2400);
            var input = new D21VoyageTimeInput { RouteDistanceMilliNm = 10000, BaseSpeedMilliKnots = 12000 };
            var context = D21VoyageTimeSystem.CreateTickContext(24, 600, 1, 2400, 2401, "d24.voyage", "ship_d24");
            var result = D21VoyageTimeSystem.Advance(state, input, context);
            return !result.Accepted && result.Code == "tick_index_mismatch"
                && result.State != null && result.State.Fingerprint == state.Fingerprint;
        }

        private static bool UiCommandCoverage()
        {
            var request = D22UiCommandRequest.Create(
                D22UiCommandKind.OpenPanel, "d24.l18.open", "d24.l18.correlation", "player_d24",
                D22ScreenIds.MainMenu, D22PanelIds.MainMenu, D22PanelIds.MainMenuSettings, 24, 2400);
            var context = new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.MainMenu,
                ActivePanelId = D22PanelIds.MainMenu,
                ActorId = "player_d24",
                WorldDay = 24,
                SaveRevision = 2400,
                SessionActive = true,
                ContextValid = true,
                PermissionGranted = true,
                CommandEnabled = true,
            };
            if (request.ValidateShape() != D22UiCommandValidationCode.None) return false;
            var invalid = request.Clone();
            invalid.TargetId = string.Empty;
            if (invalid.ValidateShape() != D22UiCommandValidationCode.MissingTargetId) return false;
            var router = new D22UiCommandRouter();
            int ownerCalls = 0;
            if (!router.Register(D22UiCommandContracts.OwnerSystem, delegate (D22UiCommandRequest item)
            {
                ownerCalls++;
                return D22UiOwnerCommandResult.Succeeded(D22UiCommandContracts.OwnerSystem, "panel_opened", true);
            })) return false;
            var routed = router.RouteDetailed(request, context);
            var duplicate = router.RouteDetailed(request, context);
            Console.WriteLine("    ui_enum_values=" + Enum.GetValues(typeof(D22UiCommandKind)).Length);
            return routed.OwnerInvoked && routed.CommandResult.Outcome == D22UiCommandOutcome.Succeeded
                && duplicate.CommandResult.Duplicate && !duplicate.OwnerInvoked && ownerCalls == 1;
        }
    }
}
