using System;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        var log = new EventLog();
        var ecology = new D4EcologySystem();
        ecology.SeedHabitat("farm", 0, 23);
        var inventory = new D3InventoryState(64, 128);
        var harvest = new D11VegetationHarvestSystem(inventory, ecology, log);

        Check(harvest.Identify(new D11VegetationIdentifyRequest
        {
            PlantId = D11VegetationIds.IndigoPlantId,
            BotanySkill = 3,
            Day = 0,
            Seed = 23,
        }).IsCorrect, "identification");

        D11VegetationHarvestResult settled = null;
        for (int day = 0; day < 32 && settled == null; day++)
        {
            var result = harvest.Harvest(new D11VegetationHarvestRequest
            {
                PlantId = D11VegetationIds.IndigoPlantId,
                ClaimedPlantId = D11VegetationIds.IndigoPlantId,
                ToolId = D11VegetationIds.HarvestingKnifeToolId,
                RequestedQuantity = 1,
                BotanySkill = 3,
                Day = day,
                Seed = 23,
                SaveRevision = 7,
                EventId = "event_d11_a07_smoke_indigo_" + day,
            });
            if (result.Succeeded) settled = result;
        }
        Check(settled != null && settled.Quantity > 0, "mature harvest");
        Check(settled.Fact != null && settled.Fact.OwnerSystem == "EcologySystem", "harvest fact owner");
        int itemCount = D3InventorySystem.CountCargo(inventory, settled.ItemId);
        var snapshot = ecology.Snapshot;
        var restoredLog = new EventLog();
        restoredLog.Restore(log.CaptureSnapshot());
        var restoredInventory = inventory.Clone();
        var restored = new D11VegetationHarvestSystem(restoredInventory,
            new D4EcologySystem(snapshot), restoredLog);
        var duplicate = restored.Harvest(new D11VegetationHarvestRequest
        {
            PlantId = settled.PlantId,
            ClaimedPlantId = settled.PlantId,
            ToolId = D11VegetationIds.HarvestingKnifeToolId,
            RequestedQuantity = 1,
            BotanySkill = 3,
            Day = int.Parse(settled.Fact.WorldDay.ToString()),
            Seed = 23,
            SaveRevision = 7,
            EventId = settled.EventId,
        });
        Check(duplicate.Succeeded && duplicate.Duplicate, "duplicate after restore");
        Check(D3InventorySystem.CountCargo(restoredInventory, settled.ItemId) == itemCount,
            "duplicate cargo");

        var toxinEcology = new D4EcologySystem();
        toxinEcology.SeedHabitat("dry_shrub", 0, 31);
        var toxinInventory = new D3InventoryState(64, 128);
        var toxinLog = new EventLog();
        var toxinSystem = new D11VegetationHarvestSystem(toxinInventory, toxinEcology, toxinLog);
        D11VegetationHarvestResult toxin = null;
        for (int day = 0; day < 32 && toxin == null; day++)
        {
            var result = toxinSystem.Harvest(new D11VegetationHarvestRequest
            {
                PlantId = D11VegetationIds.DaturaPlantId,
                ToolId = D11VegetationIds.HerbKnifeToolId,
                RequestedQuantity = 1,
                BotanySkill = 0,
                AllowUnidentifiedHarvest = true,
                Day = day,
                Seed = 31,
                SaveRevision = day + 100,
                EventId = "event_d11_a07_smoke_toxin_" + day,
            });
            if (result.Succeeded) toxin = result;
        }
        Check(toxin != null && toxin.Toxic && toxin.Misidentified &&
            toxin.ConsequenceId == "plant_poisoning", "toxic misidentification");
        Check(toxin.Fact.Result.Values["misidentified"] == "true" &&
            toxin.Fact.Result.Values["consequence_id"] == "plant_poisoning", "toxic trace");

        var reserveEcology = new D4EcologySystem();
        reserveEcology.SeedHabitat("mangrove", 0, 3);
        var reserveInventory = new D3InventoryState(16, 32);
        var reserveSystem = new D11VegetationHarvestSystem(reserveInventory, reserveEcology, new EventLog());
        var reserveBefore = reserveEcology.FindPlantSnapshot(D11VegetationIds.MangrovePlantId);
        var protectedResult = reserveSystem.Harvest(new D11VegetationHarvestRequest
        {
            PlantId = D11VegetationIds.MangrovePlantId,
            ClaimedPlantId = D11VegetationIds.MangrovePlantId,
            ToolId = D11VegetationIds.AxeToolId,
            BotanySkill = 3,
            ProtectionAreaId = D11VegetationIds.MangroveReserveId,
            AllowUnidentifiedHarvest = false,
            Day = 3,
            Seed = 3,
            SaveRevision = 1,
            EventId = "event_d11_a07_smoke_protected",
        });
        Check(!protectedResult.Succeeded && protectedResult.Code == "protected_area",
            "protected area");
        Check(reserveEcology.FindPlantSnapshot(D11VegetationIds.MangrovePlantId).Quantity == reserveBefore.Quantity,
            "protected resource unchanged");
        Console.WriteLine("D11-A07 smoke PASS: identification/maturity/tools/harvest/misidentification/toxin/protection/save-duplicate");
    }
}
