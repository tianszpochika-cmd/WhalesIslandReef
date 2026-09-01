using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23EventLogScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var first = fixture.PortRouter.Route(
                fixture.PortCommand("d23.event.enter", D19PortCommandKind.EnterPort));
            D23ScenarioAssertions.Check(ref assertions, first.Success && first.EventDraft != null,
                "D23 event scenario must receive a D19 event draft.");
            var append = fixture.EventLog.Append(first.EventDraft);
            var duplicate = fixture.EventLog.Append(first.EventDraft);
            D23ScenarioAssertions.Check(ref assertions, append.Succeeded && duplicate.IsDuplicate,
                "D23 event log must deduplicate the same event draft.");
            var replay = fixture.PortRouter.Route(
                fixture.PortCommand("d23.event.enter", D19PortCommandKind.EnterPort));
            D23ScenarioAssertions.Check(ref assertions, replay.Success && replay.IsDuplicate,
                "D23 command replay must be idempotent.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
