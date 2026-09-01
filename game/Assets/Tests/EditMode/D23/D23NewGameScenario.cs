using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23NewGameScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions, fixture.Seed == D23ScenarioFixtureFactory.FixedSeed,
                "D23 new game seed must be fixed.");
            D23ScenarioAssertions.Check(ref assertions, fixture.WorldDay == D23ScenarioFixtureFactory.FixedWorldDay,
                "D23 new game world day must be fixed.");
            D23ScenarioAssertions.Check(ref assertions, fixture.PortVisit.State == D19PortVisitState.None,
                "D23 new game starts outside the port lifecycle.");
            D23ScenarioAssertions.Check(ref assertions, fixture.ShipSave.ShipId == fixture.ShipId,
                "D23 new game ship save projection must bind the stable ship ID.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
