using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23ReturnScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.ReachDeparted(fixture, ref assertions);
            var returned = fixture.PortRouter.Route(
                fixture.PortCommand("d23.setup.return", D19PortCommandKind.Return));
            D23ScenarioAssertions.Check(ref assertions,
                returned.Success && returned.StateAfter == D19PortVisitState.Returned,
                "D23 return must use the D19 public return transition.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
