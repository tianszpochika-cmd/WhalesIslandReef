namespace Whalebone.Tests.D23
{
    public static class D23FishingCollectionScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions,
                fixture.TradeState.Inventory != null && fixture.EventLog != null,
                "D23 fishing/collection scenario requires the D3 inventory and D8 event owners.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
