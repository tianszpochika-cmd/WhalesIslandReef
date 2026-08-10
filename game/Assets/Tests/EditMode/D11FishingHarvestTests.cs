using NUnit.Framework;
using Whalebone.Game.Config;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D11;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace D11.Tests
{
    public sealed class D11FishingHarvestTests
    {
        [Test]
        public void ConfigurationHasOwnerSourceBoundsAndTestEntry()
        {
            Assert.IsEmpty(D11FishingHarvestConfiguration.Validate());
            var invalid = new Whalebone.Game.Config.NumericParameterDefinition("invalid_depth", Whalebone.Game.Config.ConfigurationUnit.Meter,
                4m, 5m, 1m, "", "", "");
            Assert.IsNotEmpty(D11FishingHarvestConfiguration.Validate(new[] { invalid }));
        }

        [Test]
        public void FishSpeciesDepthTimeTideAndGearProduceTraceableDifferences()
        {
            var mackerel = CreateSystem(out D3TradeState mackerelTrade);
            var snapper = CreateSystem(out D3TradeState snapperTrade);
            var sardine = CreateSystem(out D3TradeState sardineTrade);

            var mackerelResult = mackerel.Harvest(new D11HarvestRequest
            {
                LocationId = "nearshore_saint_marys", GearId = "gear_handline_basic",
                Day = 0, Seed = 7, DepthMeters = 2f, TimeOfDayBucket = "day", TideBand = "flood",
                SaveRevision = 1,
            });
            var snapperResult = snapper.Harvest(new D11HarvestRequest
            {
                LocationId = "reef_edge_coral_bay", GearId = "gear_shore_rod",
                Day = 1, Seed = 7, DepthMeters = 8f, TimeOfDayBucket = "dawn", TideBand = "mid",
                SaveRevision = 1,
            });
            var sardineResult = sardine.Harvest(new D11HarvestRequest
            {
                LocationId = "sardine_bay", GearId = "gear_cast_net",
                Day = 2, Seed = 7, DepthMeters = 4f, TimeOfDayBucket = "dusk", TideBand = "ebb",
                SaveRevision = 1,
            });

            Assert.IsTrue(mackerelResult.Succeeded, mackerelResult.Message);
            Assert.IsTrue(snapperResult.Succeeded, snapperResult.Message);
            Assert.IsTrue(sardineResult.Succeeded, sardineResult.Message);
            Assert.AreEqual("mackerel", mackerelResult.FishSpeciesId);
            Assert.AreEqual("snapper", snapperResult.FishSpeciesId);
            Assert.AreEqual("sardine", sardineResult.FishSpeciesId);
            Assert.AreNotEqual(mackerelResult.ItemId, snapperResult.ItemId);
            Assert.AreNotEqual(snapperResult.ItemId, sardineResult.ItemId);
            Assert.Greater(snapperResult.TimeMultiplier, 1f);
            Assert.Greater(sardineResult.TimeMultiplier, 1f);
            Assert.AreEqual(mackerelResult.Quantity, mackerelTrade.Inventory.Cargo[mackerelResult.ItemId]);
            Assert.AreEqual(snapperResult.Quantity, snapperTrade.Inventory.Cargo[snapperResult.ItemId]);
            Assert.IsNotNull(sardine.EventLog.Find(D11FishingHarvestSystem.HarvestEventId, 1));
            Assert.AreEqual("EcologySystem", sardine.EventLog.Find(D11FishingHarvestSystem.HarvestEventId, 1).OwnerSystem);
        }

        [Test]
        public void HandShallowGatheringProcessingAndMarketFeedbackUseTheirOwners()
        {
            var system = CreateSystem(out D3TradeState trade);
            var seaweed = system.Harvest(new D11HarvestRequest
            {
                LocationId = "saint_marys_tidepools", GearId = "gear_seaweed_hook",
                Day = 3, Seed = 11, DepthMeters = 0.5f, TimeOfDayBucket = "dawn", TideBand = "flood",
                SaveRevision = 1,
            });
            var shellfish = system.Harvest(new D11HarvestRequest
            {
                LocationId = "saint_marys_shallows", GearId = "gear_shellfish_rake",
                Day = 3, Seed = 12, DepthMeters = 0.5f, TimeOfDayBucket = "dawn", TideBand = "ebb",
                SaveRevision = 2,
            });

            Assert.IsTrue(seaweed.Succeeded, seaweed.Message);
            Assert.IsTrue(shellfish.Succeeded, shellfish.Message);
            Assert.AreEqual("seaweed", seaweed.ResourceKind);
            Assert.AreEqual("shellfish", shellfish.ResourceKind);
            Assert.Greater(D3InventorySystem.CountCargo(trade.Inventory, "item_seaweed"), 0);
            Assert.Greater(D3InventorySystem.CountCargo(trade.Inventory, "item_conch"), 0);

            var dried = system.Process("item_seaweed", 1, D11ProcessingMethod.Dry, 3);
            var cured = system.Process("item_conch", 1, D11ProcessingMethod.CureShellfish, 4);
            Assert.IsTrue(dried.Succeeded, dried.Message);
            Assert.IsTrue(cured.Succeeded, cured.Message);
            Assert.Greater(D3InventorySystem.CountCargo(trade.Inventory, "item_dried_seaweed"), 0);
            Assert.Greater(D3InventorySystem.CountCargo(trade.Inventory, "item_cured_shellfish"), 0);

            var sale = system.Sell(PortCatalog.StartingPortId, "item_dried_seaweed", 1, 3);
            Assert.IsTrue(sale.Succeeded, sale.Message);
            Assert.Greater(sale.UnitPrice, 0);
            Assert.IsNotEmpty(sale.PriceReasons);
        }

        [Test]
        public void ProtectedAreaIsRejectedAndRecorded()
        {
            var system = CreateSystem(out _);
            var result = system.Harvest(new D11HarvestRequest
            {
                LocationId = "protected_turtle_cove", GearId = "gear_seaweed_hook",
                Day = 5, Seed = 1, DepthMeters = 0.5f, SaveRevision = 9,
            });

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("protected_area_no_permit", result.Code);
            var fact = system.EventLog.Find(D11FishingHarvestSystem.ProtectedRejectEventId, 9);
            Assert.IsNotNull(fact);
            Assert.AreEqual("EcologySystem", fact.OwnerSystem);
            Assert.AreEqual("protected_turtle_cove_no_take", fact.Result.Values["protection_rule_id"]);
        }

        [Test]
        public void RestoredEventAndEcologySnapshotDoNotDuplicateCatch()
        {
            var first = CreateSystem(out D3TradeState trade);
            var request = new D11HarvestRequest
            {
                LocationId = "nearshore_saint_marys", GearId = "gear_handline_basic",
                Day = 0, Seed = 13, DepthMeters = 2f, TimeOfDayBucket = "day", TideBand = "flood",
                SaveRevision = 71,
            };
            var settled = first.Harvest(request);
            Assert.IsTrue(settled.Succeeded, settled.Message);
            int before = D3InventorySystem.CountCargo(trade.Inventory, settled.ItemId);
            var savedEcology = firstEcology(first);
            var savedRuntime = first.CaptureSnapshot();

            var resumedTrade = new D3TradeState(trade.Money, trade.Inventory.CargoCapacity);
            resumedTrade.Inventory.Cargo[settled.ItemId] = before;
            var resumed = new D11FishingHarvestSystem(resumedTrade, new D4EcologySystem(savedEcology),
                new TradeEconomySystem(), new EventLog());
            resumed.Restore(savedRuntime);
            var duplicate = resumed.Harvest(request);

            Assert.IsTrue(duplicate.Succeeded);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(before, D3InventorySystem.CountCargo(resumedTrade.Inventory, settled.ItemId));
            Assert.AreEqual(savedEcology.FishSchools[0].Biomass,
                ((D4EcologySystem)resumedEcology(resumed)).Snapshot.FishSchools[0].Biomass);
        }

        private static D11FishingHarvestSystem CreateSystem(out D3TradeState trade)
        {
            trade = new D3TradeState(500, 100);
            var log = new EventLog();
            return new D11FishingHarvestSystem(trade, new D4EcologySystem(), new TradeEconomySystem(log), log);
        }

        private static Whalebone.Systems.D4.EcologyState firstEcology(D11FishingHarvestSystem system)
        {
            // The system intentionally exposes no ecology writer; this helper is replaced by the
            // test fixture's direct owner reference through the overload below.
            return system.EcologySnapshot;
        }

        private static D4EcologySystem resumedEcology(D11FishingHarvestSystem system)
        {
            return system.EcologySystem;
        }
    }
}
