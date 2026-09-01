using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23BoardingScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.ReachBoarded(fixture, ref assertions);
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PortVisit.State == D19PortVisitState.Boarded,
                "D23 boarding must use the D19 public lifecycle route.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
