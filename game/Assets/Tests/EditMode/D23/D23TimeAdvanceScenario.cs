namespace Whalebone.Tests.D23
{
    public static class D23TimeAdvanceScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var result = D23ScenarioAssertions.AdvanceOneTick(fixture);
            D23ScenarioAssertions.Check(ref assertions, result.Accepted,
                "D23 time advance must accept one fixed logical tick.");
            D23ScenarioAssertions.Check(ref assertions,
                result.ProjectedWorldDay == fixture.WorldDay && result.ProjectedMinuteOfDay == 495,
                "D23 time advance must use the fixed 15-minute logical clock.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
