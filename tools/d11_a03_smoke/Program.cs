using System;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var trade = new D3TradeState(500, 100);
        var hold = new D11LiveFishHoldSystem(null, trade, new EventLog());
        Check(D3InventorySystem.TryAddCargo(trade.Inventory, D11AquacultureIds.FryItemId, 8, out _), "seed fry");
        Check(hold.StockFry(new D11LiveFishStockRequest { Quantity = 8, WorldDay = 0, SaveRevision = 1,
            EventId = "event_d11_smoke_stock" }).Succeeded, "stock hold");
        Check(hold.UsedVolume == 8 && hold.TotalLiveWeight == 8, "hold weight/volume");
        Check(hold.Maintain(new D11LiveFishMaintenanceRequest { WorldDay = 1, WaterExchangeUnits = 4,
            LaborUnits = 1, Seed = 1, SaveRevision = 2, EventId = "event_d11_smoke_maintain" }).Succeeded, "maintain hold");

        var ecology = new D4EcologySystem();
        ecology.SeedHabitat("nearshore_reef_edge", 0, 2);
        ecology.SeedFishSchool(D11AquacultureIds.FishSchoolId, "nearshore_saint_marys", 0, 2);
        Check(D3InventorySystem.TryAddCargo(trade.Inventory, D11AquacultureIds.FryItemId, 6, out _), "seed pond fry");
        Check(D3InventorySystem.TryAddCargo(trade.Inventory, D11AquacultureIds.FeedItemId, 12, out _), "seed feed");
        var log = new EventLog();
        var farm = new D11IslandAquacultureSystem(null, trade, ecology, log);
        Check(farm.Build(new D11AquacultureBuildRequest { Kind = D11AquacultureFacilityKind.FishPond,
            FacilityId = "pond_smoke", WorldDay = 0, SaveRevision = 3, EventId = "event_d11_smoke_build" }).Succeeded, "build pond");
        Check(farm.SupplyInputs(new D11AquacultureInputRequest { FacilityId = "pond_smoke", FryQuantity = 6,
            FeedQuantity = 12, LaborUnits = 2, WorldDay = 0, SaveRevision = 4, EventId = "event_d11_smoke_inputs" }).Succeeded, "pond inputs");
        var advance = farm.Advance(new D11AquacultureAdvanceRequest { FacilityId = "pond_smoke", WorldDay = 1,
            WaterExchangeUnits = 3, Seed = 4, SaveRevision = 5, EventId = "event_d11_smoke_advance" });
        Check(advance.Succeeded && ecology.Snapshot.Habitat.Pollution > 0f, "pond advance pressure");
        Check(advance.Fact.CauseIds.Contains("event_d11_smoke_inputs"), "cause chain");
        Check(farm.Harvest(new D11AquacultureHarvestRequest { FacilityId = "pond_smoke", Quantity = 1,
            WorldDay = 2, SaveRevision = 6, EventId = "event_d11_smoke_harvest" }).Succeeded, "pond harvest");
        Check(D3InventorySystem.CountCargo(trade.Inventory, D11AquacultureIds.FishCargoItemId) == 1, "fish cargo");
        Console.WriteLine("D11-A03 smoke PASS: live-hold/pond/cargo/pressure/events");
    }
}
