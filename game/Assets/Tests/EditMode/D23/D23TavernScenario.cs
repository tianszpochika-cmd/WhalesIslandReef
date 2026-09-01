using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23TavernScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var availability = D19PortAvailabilitySystem.Adjudicate(
                fixture.PortId,
                D19PortLocalizationKeys.ServiceIdTavern,
                D19ServiceTimePeriod.Evening,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                true);
            D23ScenarioAssertions.Check(ref assertions,
                availability.Verdict == D19AvailabilityVerdict.Available,
                "D23 tavern scenario must have a deterministic service availability input.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
