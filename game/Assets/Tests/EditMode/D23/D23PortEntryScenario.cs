using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23PortEntryScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.ReachBerthed(fixture, ref assertions);
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PortVisit.State == D19PortVisitState.Berthed &&
                fixture.PortVisit.BerthId == fixture.BerthId,
                "D23 port entry must finish at the assigned berth.");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.Berths.ValidateInvariants().Count == 0,
                "D23 port entry must leave berth invariants valid.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
