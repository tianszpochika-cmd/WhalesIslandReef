using Whalebone.Systems.D3;

namespace Whalebone.Tests.D23
{
    public static class D23CurrencyInventoryInvariantTests
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            D23ScenarioAssertions.Check(ref assertions, fixture.TradeState.Money >= 0,
                "D23 currency invariant must never start below zero.");
            D23ScenarioAssertions.Check(ref assertions,
                D3InventorySystem.FreeCargo(fixture.TradeState.Inventory) >= 0 &&
                D3InventorySystem.UsedWeight(fixture.TradeState.Inventory) >= 0,
                "D23 inventory invariant must expose non-negative owner readbacks.");
            D23ScenarioAssertions.Check(ref assertions,
                fixture.Berths.ValidateInvariants().Count == 0,
                "D23 currency/inventory fixture must not start with berth corruption.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
