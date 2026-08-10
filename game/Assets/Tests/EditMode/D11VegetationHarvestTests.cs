using System;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;
using Whalebone.Systems.Events;

namespace D11.Tests
{
    public sealed class D11VegetationHarvestTests
    {
        [Test]
        public void CatalogCoversEightResourceKindsAndRequiredTools()
        {
            var kinds = new bool[8];
            foreach (var resource in D11VegetationHarvestCatalog.Resources)
            {
                Assert.IsNotNull(D11VegetationHarvestCatalog.FindTool(resource.RequiredToolId));
                Assert.IsNotEmpty(resource.ItemId);
                kinds[(int)resource.Kind] = true;
            }
            for (int i = 0; i < kinds.Length; i++) Assert.IsTrue(kinds[i], "resource kind " + i);
        }

        [Test]
        public void IdentificationIsDeterministicAndWrongToolDoesNotTouchEcology()
        {
            var request = new D11VegetationIdentifyRequest
            {
                PlantId = D11VegetationIds.GuavaPlantId,
                BotanySkill = 0,
                Day = 4,
                Seed = 19,
            };
            var first = new D11VegetationHarvestSystem(new D3InventoryState(20),
                new D4EcologySystem()).Identify(request);
            var second = new D11VegetationHarvestSystem(new D3InventoryState(20),
                new D4EcologySystem()).Identify(request.Clone());
            Assert.AreEqual(first.IdentifiedPlantId, second.IdentifiedPlantId);
            Assert.AreEqual(first.IsCorrect, second.IsCorrect);

            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("coast", 0, 19);
            var inventory = new D3InventoryState(20);
            var before = ecology.FindPlantSnapshot(D11VegetationIds.GuavaPlantId);
            var result = new D11VegetationHarvestSystem(inventory, ecology).Harvest(
                new D11VegetationHarvestRequest
                {
                    PlantId = D11VegetationIds.GuavaPlantId,
                    ClaimedPlantId = D11VegetationIds.GuavaPlantId,
                    ToolId = D11VegetationIds.AxeToolId,
                    BotanySkill = 3,
                    Day = 4,
                    Seed = 19,
                });
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("tool_required", result.Code);
            Assert.AreEqual(before.Quantity,
                ecology.FindPlantSnapshot(D11VegetationIds.GuavaPlantId).Quantity);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));
        }

        [Test]
        public void EachResourceKindHasAWorkingMaturityAndToolPath()
        {
            foreach (var resource in D11VegetationHarvestCatalog.Resources)
            {
                if (resource.Id == D11VegetationIds.MangrovePlantId) continue;
                D11VegetationHarvestResult settled = null;
                for (int day = 0; day < 48 && settled == null; day++)
                {
                    var ecology = new D4EcologySystem();
                    ecology.SeedHabitat(resource.HabitatId, 0, 41);
                    var inventory = new D3InventoryState(64, 128);
                    var system = new D11VegetationHarvestSystem(inventory, ecology, new EventLog());
                    var result = system.Harvest(new D11VegetationHarvestRequest
                    {
                        PlantId = resource.Id,
                        ClaimedPlantId = resource.Id,
                        ToolId = resource.RequiredToolId,
                        RequestedQuantity = 1,
                        BotanySkill = 4,
                        Day = day,
                        Seed = 41,
                        SaveRevision = day + 1,
                        EventId = "event_d11_a07_test_" + resource.Id + "_" + day,
                    });
                    if (result.Succeeded) settled = result;
                    else Assert.That(result.Code, Is.EqualTo("not_mature"),
                        resource.Id + " rejected for unexpected reason: " + result.Code);
                }
                Assert.IsNotNull(settled, "No mature harvest for " + resource.Id);
                Assert.Greater(settled.Quantity, 0);
                Assert.AreEqual(resource.Kind, settled.ResourceKind);
                Assert.IsNotEmpty(settled.ItemId);
            }
        }

        [Test]
        public void MisidentifiedToxinProducesTraceableConsequence()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("dry_shrub", 0, 53);
            var log = new EventLog();
            var inventory = new D3InventoryState(32, 64);
            var system = new D11VegetationHarvestSystem(inventory, ecology, log);
            D11VegetationHarvestResult result = null;
            for (int day = 0; day < 48 && result == null; day++)
            {
                var candidate = system.Harvest(new D11VegetationHarvestRequest
                {
                    PlantId = D11VegetationIds.DaturaPlantId,
                    ToolId = D11VegetationIds.HerbKnifeToolId,
                    RequestedQuantity = 1,
                    BotanySkill = 0,
                    AllowUnidentifiedHarvest = true,
                    Day = day,
                    Seed = 53,
                    SaveRevision = day + 100,
                    EventId = "event_d11_a07_toxin_" + day,
                });
                if (candidate.Succeeded) result = candidate;
                else Assert.That(candidate.Code, Is.EqualTo("not_mature"), candidate.Message);
            }
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Toxic);
            Assert.IsTrue(result.Misidentified);
            Assert.AreEqual("plant_poisoning", result.ConsequenceId);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "item_toxin_datura"));
            Assert.AreEqual("true", result.Fact.Result.Values["misidentified"]);
            Assert.AreEqual("plant_poisoning", result.Fact.Result.Values["consequence_id"]);
        }

        [Test]
        public void ProtectedAreaRejectsHarvestAndLeavesResourceUntouched()
        {
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("mangrove", 0, 7);
            var inventory = new D3InventoryState(16, 32);
            var system = new D11VegetationHarvestSystem(inventory, ecology);
            var before = ecology.FindPlantSnapshot(D11VegetationIds.MangrovePlantId);
            var result = system.Harvest(new D11VegetationHarvestRequest
            {
                PlantId = D11VegetationIds.MangrovePlantId,
                ClaimedPlantId = D11VegetationIds.MangrovePlantId,
                ToolId = D11VegetationIds.AxeToolId,
                BotanySkill = 4,
                ProtectionAreaId = D11VegetationIds.MangroveReserveId,
                Day = 4,
                Seed = 7,
                SaveRevision = 1,
                EventId = "event_d11_a07_protected_test",
            });
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("protected_area", result.Code);
            Assert.AreEqual(before.Quantity,
                ecology.FindPlantSnapshot(D11VegetationIds.MangrovePlantId).Quantity);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, "timber"));
        }

        [Test]
        public void EventAndEcologySnapshotPreventDuplicateCargoAfterRestore()
        {
            var log = new EventLog();
            var ecology = new D4EcologySystem();
            ecology.SeedHabitat("farm", 0, 61);
            var inventory = new D3InventoryState(32, 64);
            var system = new D11VegetationHarvestSystem(inventory, ecology, log);
            D11VegetationHarvestResult settled = null;
            for (int day = 0; day < 48 && settled == null; day++)
            {
                var candidate = system.Harvest(new D11VegetationHarvestRequest
                {
                    PlantId = D11VegetationIds.IndigoPlantId,
                    ClaimedPlantId = D11VegetationIds.IndigoPlantId,
                    ToolId = D11VegetationIds.HarvestingKnifeToolId,
                    BotanySkill = 3,
                    Day = day,
                    Seed = 61,
                    SaveRevision = 9,
                    EventId = "event_d11_a07_restore_test_" + day,
                });
                if (candidate.Succeeded) settled = candidate;
                else Assert.That(candidate.Code, Is.EqualTo("not_mature"), candidate.Message);
            }
            Assert.IsNotNull(settled);
            int cargoBefore = D3InventorySystem.CountCargo(inventory, settled.ItemId);
            var restoredLog = new EventLog();
            restoredLog.Restore(log.CaptureSnapshot());
            var restoredInventory = inventory.Clone();
            var restoredSystem = new D11VegetationHarvestSystem(restoredInventory,
                new D4EcologySystem(ecology.Snapshot), restoredLog);
            var duplicate = restoredSystem.Harvest(new D11VegetationHarvestRequest
            {
                PlantId = settled.PlantId,
                ClaimedPlantId = settled.PlantId,
                ToolId = settled.ToolId,
                BotanySkill = 3,
                Day = settled.Fact.WorldDay,
                Seed = 61,
                SaveRevision = 9,
                EventId = settled.EventId,
            });
            Assert.IsTrue(duplicate.Succeeded);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(cargoBefore, D3InventorySystem.CountCargo(restoredInventory, settled.ItemId));
        }
    }
}
