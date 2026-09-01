using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23MarketScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var request = new D19MarketTradeRequest
            {
                CommandId = "d23.market.buy",
                ActorId = fixture.PlayerId,
                ShipId = fixture.ShipId,
                PortId = fixture.PortId,
                BerthId = fixture.BerthId,
                ServiceId = D19PortLocalizationKeys.ServiceIdMarket,
                CommodityId = fixture.CommodityId,
                Quantity = 1,
                WorldDay = fixture.WorldDay,
                SaveRevision = fixture.SaveRevision + 1,
            };
            int moneyBefore = fixture.TradeState.Money;
            var result = D19MarketPortAdapter.Buy(
                fixture.TradeState, fixture.Economy, request, fixture.CreateClaims());
            int cargoAfter;
            fixture.TradeState.Inventory.Cargo.TryGetValue(fixture.CommodityId, out cargoAfter);
            D23ScenarioAssertions.Check(ref assertions, result.Success,
                "D23 market buy must use the D19 to D3 public adapter.");
            D23ScenarioAssertions.Check(ref assertions, cargoAfter == 1 && fixture.TradeState.Money < moneyBefore,
                "D23 market buy must change the D3-owned cargo and funds together.");

            var poorState = new Whalebone.Systems.D3.D3TradeState(0, 40);
            var poorResult = D19MarketPortAdapter.Buy(
                poorState, new Whalebone.Systems.D3.TradeEconomySystem(), request, fixture.CreateClaims());
            D23ScenarioAssertions.Check(ref assertions, !poorResult.Success && poorState.Money == 0,
                "D23 market insufficient-funds path must fail without money mutation.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
