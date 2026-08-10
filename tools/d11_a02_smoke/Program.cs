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
        Check(D11FishingHarvestConfiguration.Validate().Count == 0, "configuration validation");
        Check(D11FishingHarvestConfiguration.Validate(new[]
        {
            new Whalebone.Game.Config.NumericParameterDefinition("invalid_depth",
                Whalebone.Game.Config.ConfigurationUnit.Meter, 4m, 5m, 1m, "", "", ""),
        }).Count > 0, "invalid configuration detection");

        var mackerel = Create(out D3TradeState mackerelTrade);
        var snapper = Create(out _);
        var sardine = Create(out _);
        var mackerelResult = mackerel.Harvest(new D11HarvestRequest
        {
            LocationId = "nearshore_saint_marys", GearId = "gear_handline_basic",
            Day = 0, Seed = 7, DepthMeters = 2f, TimeOfDayBucket = "day", TideBand = "flood", SaveRevision = 1,
        });
        var snapperResult = snapper.Harvest(new D11HarvestRequest
        {
            LocationId = "reef_edge_coral_bay", GearId = "gear_shore_rod",
            Day = 1, Seed = 7, DepthMeters = 8f, TimeOfDayBucket = "dawn", TideBand = "mid", SaveRevision = 1,
        });
        var sardineResult = sardine.Harvest(new D11HarvestRequest
        {
            LocationId = "sardine_bay", GearId = "gear_cast_net",
            Day = 2, Seed = 7, DepthMeters = 4f, TimeOfDayBucket = "dusk", TideBand = "ebb", SaveRevision = 1,
        });
        Check(mackerelResult.Succeeded && snapperResult.Succeeded && sardineResult.Succeeded, "fish harvest");
        Check(mackerelResult.FishSpeciesId == "mackerel" && snapperResult.FishSpeciesId == "snapper" && sardineResult.FishSpeciesId == "sardine", "fish species");
        Check(snapperResult.TimeMultiplier > 1f && sardineResult.TimeMultiplier > 1f, "time suitability");
        Check(D3InventorySystem.CountCargo(mackerelTrade.Inventory, mackerelResult.ItemId) == mackerelResult.Quantity, "inventory owner");
        Check(mackerel.EventLog.Find(D11FishingHarvestSystem.HarvestEventId, 1).OwnerSystem == "EcologySystem", "ecology fact owner");

        var shallow = Create(out D3TradeState shallowTrade);
        var seaweed = shallow.Harvest(new D11HarvestRequest
        {
            LocationId = "saint_marys_tidepools", GearId = "gear_seaweed_hook",
            Day = 3, Seed = 11, DepthMeters = 0.5f, TimeOfDayBucket = "dawn", TideBand = "flood", SaveRevision = 1,
        });
        var shellfish = shallow.Harvest(new D11HarvestRequest
        {
            LocationId = "saint_marys_shallows", GearId = "gear_shellfish_rake",
            Day = 3, Seed = 12, DepthMeters = 0.5f, TimeOfDayBucket = "dawn", TideBand = "ebb", SaveRevision = 2,
        });
        Check(seaweed.Succeeded && shellfish.Succeeded, "shallow resources");
        Check(shallow.Process("item_seaweed", 1, D11ProcessingMethod.Dry, 3).Succeeded, "dry seaweed");
        Check(shallow.Process("item_conch", 1, D11ProcessingMethod.CureShellfish, 4).Succeeded, "cure shellfish");
        Check(D3InventorySystem.CountCargo(shallowTrade.Inventory, "item_dried_seaweed") > 0, "dried seaweed inventory");
        Check(D3InventorySystem.CountCargo(shallowTrade.Inventory, "item_cured_shellfish") > 0, "cured shellfish inventory");
        var sale = shallow.Sell(PortCatalog.StartingPortId, "item_dried_seaweed", 1, 3);
        Check(sale.Succeeded && sale.UnitPrice > 0 && sale.PriceReasons.Count > 0, "market feedback");

        var blocked = shallow.Harvest(new D11HarvestRequest
        {
            LocationId = "protected_turtle_cove", GearId = "gear_seaweed_hook",
            Day = 5, Seed = 1, DepthMeters = 0.5f, SaveRevision = 9,
        });
        Check(!blocked.Succeeded && blocked.Code == "protected_area_no_permit", "protected area rule");
        Check(shallow.EventLog.Find(D11FishingHarvestSystem.ProtectedRejectEventId, 9) != null, "protected event trace");

        var savedEcology = mackerel.EcologySnapshot;
        var savedRuntime = mackerel.CaptureSnapshot();
        var resumedTrade = new D3TradeState(mackerelTrade.Money, mackerelTrade.Inventory.CargoCapacity);
        resumedTrade.Inventory.Cargo[mackerelResult.ItemId] = mackerelResult.Quantity;
        var resumed = new D11FishingHarvestSystem(resumedTrade, new D4EcologySystem(savedEcology), new TradeEconomySystem(), new EventLog());
        resumed.Restore(savedRuntime);
        var duplicate = resumed.Harvest(new D11HarvestRequest
        {
            LocationId = "nearshore_saint_marys", GearId = "gear_handline_basic",
            Day = 0, Seed = 7, DepthMeters = 2f, TimeOfDayBucket = "day", TideBand = "flood", SaveRevision = 1,
        });
        Check(duplicate.Succeeded && duplicate.Duplicate, "restored duplicate settlement");
        Check(D3InventorySystem.CountCargo(resumedTrade.Inventory, mackerelResult.ItemId) == mackerelResult.Quantity, "no duplicate inventory");

        Console.WriteLine("D11-A02 smoke PASS: species/depth/time/tide/gear/shallow/processing/market/protection/idempotency");
    }

    private static D11FishingHarvestSystem Create(out D3TradeState trade)
    {
        trade = new D3TradeState(500, 100);
        var log = new EventLog();
        return new D11FishingHarvestSystem(trade, new D4EcologySystem(), new TradeEconomySystem(log), log);
    }
}
