using Whalebone.Systems.D21;

namespace Whalebone.Tests.D23
{
    public static class D23PreparationScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions,
                D21HullComponentSystem.ValidateConfiguration(fixture.HullConfiguration).Valid,
                "D23 preparation must read a valid D21 hull configuration.");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.ShipCondition.EvaluateSeaworthiness() == D21ShipSeaworthiness.Navigable,
                "D23 preparation starts from a navigable D21 condition snapshot.");
            D23ScenarioAssertions.Check(ref assertions, fixture.Weather.TryValidate(out _),
                "D23 preparation weather input must validate.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
