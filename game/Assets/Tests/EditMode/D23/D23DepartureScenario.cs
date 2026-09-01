using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23DepartureScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.ReachDeparted(fixture, ref assertions);
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PortVisit.State == D19PortVisitState.Departed,
                "D23 departure must release the port lifecycle into Departed.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
