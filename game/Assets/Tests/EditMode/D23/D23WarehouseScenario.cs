using Whalebone.Systems.D19;

namespace Whalebone.Tests.D23
{
    public static class D23WarehouseScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var adapter = new D19WarehousePortAdapter();
            var request = new D19WarehouseRequest(
                "d23.warehouse.store",
                D19WarehouseOperation.Store,
                fixture.PlayerId,
                fixture.ShipId,
                fixture.PortId,
                "service_santa_margarita_warehouse",
                fixture.CommodityId,
                2,
                fixture.ShipId,
                fixture.WorldDay,
                fixture.SaveRevision + 1);
            var stored = adapter.Store(request, fixture.TradeState.Inventory, fixture.CreateClaims());
            var replay = adapter.Store(request, fixture.TradeState.Inventory, fixture.CreateClaims());
            int cargoCount;
            fixture.TradeState.Inventory.Cargo.TryGetValue(fixture.CommodityId, out cargoCount);
            D23ScenarioAssertions.Check(ref assertions,
                stored.Success && stored.AppliedQuantity == 2 && stored.ItemCount == 2,
                "D23 warehouse store must use D3 inventory owner API: " +
                    (stored.Failure == null ? "no_failure" : stored.Failure.ReasonId));
            D23ScenarioAssertions.Check(ref assertions,
                replay.Success && replay.AppliedQuantity == 2 && replay.ItemCount == 2 &&
                cargoCount == 2 && replay.Event != null && stored.Event != null &&
                replay.Event.EventId == stored.Event.EventId,
                "D23 warehouse replay must preserve the original result and avoid a second cargo write.");
            D23ScenarioAssertions.Check(ref assertions,
                !adapter.Store(new D19WarehouseRequest(
                    "d23.warehouse.store",
                    D19WarehouseOperation.Store,
                    fixture.PlayerId,
                    fixture.ShipId,
                    fixture.PortId,
                    "service_santa_margarita_warehouse",
                    fixture.CommodityId,
                    3,
                    fixture.ShipId,
                    fixture.WorldDay,
                    fixture.SaveRevision + 1),
                    fixture.TradeState.Inventory,
                    fixture.CreateClaims()).Success && cargoCount == 2,
                "D23 warehouse command conflict must not mutate the D3 cargo owner.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
