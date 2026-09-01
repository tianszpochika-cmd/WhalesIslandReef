namespace Whalebone.Tests.D23
{
    public static class D23FactionConsequenceScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var claims = fixture.CreateClaims();
            D23ScenarioAssertions.Check(ref assertions,
                claims.ShipId == fixture.ShipId && claims.ShipCurrentPortId == fixture.PortId,
                "D23 faction scenario must carry explicit ship and port authority claims.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
