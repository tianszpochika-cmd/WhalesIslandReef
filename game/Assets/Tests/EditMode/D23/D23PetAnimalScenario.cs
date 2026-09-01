namespace Whalebone.Tests.D23
{
    public static class D23PetAnimalScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions,
                fixture.PlayerIdentity.CharacterId == fixture.PlayerId,
                "D23 pet/animal scenario must retain the character owner reference.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
