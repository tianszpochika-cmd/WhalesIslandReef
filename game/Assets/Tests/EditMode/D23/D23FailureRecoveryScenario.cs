using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23FailureRecoveryScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var rejected = fixture.PortRouter.Route(
                fixture.PortCommand("d23.failure.depart-before-entry", D19PortCommandKind.Depart));
            D23ScenarioAssertions.Check(ref assertions, !rejected.Success,
                "D23 failure recovery must reject a departure before port entry.");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PortVisit.State == D19PortVisitState.None,
                "D23 rejected departure must not partially mutate port state.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
