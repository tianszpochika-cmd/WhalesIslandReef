// D24-L17：固定输入回放测试（纯 C#；不启动 Unity/Player/PlayMode）。

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Whalebone.Systems.D19;
using Whalebone.Systems.D21;
using Whalebone.Tests.D23;

namespace Whalebone.Tests.D24
{
    public static class D24SeedReplayTests
    {
        public static int Run() { return RunAll(); }

        public static int RunAll()
        {
            int failed = 0;
            failed += Check("D24-L17 fixed input replay is byte-stable", FixedInputReplayIsStable);
            failed += Check("D24-L17 port command replay is deterministic", PortReplayHasExpectedStates);
            failed += Check("D24-L17 duplicate command is idempotent", DuplicateCommandIsStable);
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

        private static bool FixedInputReplayIsStable()
        {
            string first = ReplaySignature();
            for (int index = 0; index < 24; index++)
                if (!string.Equals(first, ReplaySignature(), StringComparison.Ordinal)) return false;
            return first.Length == 64;
        }

        private static bool PortReplayHasExpectedStates()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            var enter = fixture.PortRouter.Route(fixture.PortCommand("d24.l17.enter", D19PortCommandKind.EnterPort));
            var dock = fixture.PortRouter.Route(fixture.PortCommand("d24.l17.dock", D19PortCommandKind.Dock));
            var berth = fixture.PortRouter.Route(fixture.PortCommand("d24.l17.berth", D19PortCommandKind.AssignBerth, fixture.BerthId));
            var board = fixture.PortRouter.Route(fixture.PortCommand("d24.l17.board", D19PortCommandKind.Board, fixture.BerthId));
            var depart = fixture.PortRouter.Route(fixture.PortCommand("d24.l17.depart", D19PortCommandKind.Depart, fixture.BerthId));
            return enter.Success && enter.StateAfter == D19PortVisitState.PortVisit
                && dock.Success && dock.StateAfter == D19PortVisitState.Docked
                && berth.Success && berth.StateAfter == D19PortVisitState.Berthed
                && board.Success && board.StateAfter == D19PortVisitState.Boarded
                && depart.Success && depart.StateAfter == D19PortVisitState.Departed;
        }

        private static bool DuplicateCommandIsStable()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            var command = fixture.PortCommand("d24.l17.duplicate", D19PortCommandKind.EnterPort);
            var first = fixture.PortRouter.Route(command);
            var second = fixture.PortRouter.Route(command);
            return first.Success && !first.IsDuplicate
                && second.Success && second.IsDuplicate
                && second.StateAfter == first.StateAfter;
        }

        private static string ReplaySignature()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            var parts = new List<string> { fixture.Seed.ToString(), fixture.WorldDay.ToString(), fixture.SaveRevision.ToString() };
            var commands = new[]
            {
                fixture.PortRouter.Route(fixture.PortCommand("d24.replay.enter", D19PortCommandKind.EnterPort)),
                fixture.PortRouter.Route(fixture.PortCommand("d24.replay.dock", D19PortCommandKind.Dock)),
                fixture.PortRouter.Route(fixture.PortCommand("d24.replay.berth", D19PortCommandKind.AssignBerth, fixture.BerthId)),
                fixture.PortRouter.Route(fixture.PortCommand("d24.replay.board", D19PortCommandKind.Board, fixture.BerthId)),
                fixture.PortRouter.Route(fixture.PortCommand("d24.replay.depart", D19PortCommandKind.Depart, fixture.BerthId)),
            };
            foreach (var command in commands)
                parts.Add(command.Success + ":" + command.IsDuplicate + ":" + command.Kind + ":" + command.StateAfter + ":" + command.ReasonCode);

            var current = fixture.VoyageTime;
            for (int index = 0; index < 8; index++)
            {
                var input = new D21VoyageTimeInput
                {
                    RouteDistanceMilliNm = current.RouteDistanceMilliNm,
                    BaseSpeedMilliKnots = 12 * D21VoyageTimeSystem.MilliUnitsPerKnot,
                    SpeedPermille = 1000,
                    CurrentPermille = 1000,
                    TickMinutes = D21VoyageTimeSystem.FixedTickMinutes,
                };
                int absoluteMinute = fixture.VoyageTime.StartMinuteOfDay + current.ElapsedTicks * D21TickContext.FixedTickMinutes;
                int worldDay = fixture.VoyageTime.StartWorldDay + absoluteMinute / D21TickContext.MinutesPerDay;
                int minuteOfDay = absoluteMinute % D21TickContext.MinutesPerDay;
                var context = D21VoyageTimeSystem.CreateTickContext(
                    worldDay, minuteOfDay, current.ElapsedTicks, fixture.SaveRevision,
                    fixture.Seed, current.VoyageId, current.ShipId);
                var result = D21VoyageTimeSystem.Advance(current, input, context);
                parts.Add(result.Accepted + ":" + result.Code + ":" + (result.State == null ? "" : result.State.Fingerprint));
                if (result.State == null) break;
                current = result.State;
            }
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("|", parts)));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte value in bytes) builder.Append(value.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
