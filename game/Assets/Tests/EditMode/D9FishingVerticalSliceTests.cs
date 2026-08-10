// ============================================================
// D9FishingVerticalSliceTests —— D9-A02 捕捞垂直切片
// ============================================================

using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.EditMode
{
    public sealed class D9FishingVerticalSliceTests
    {
        private D3TradeState _trade;
        private D4EcologySystem _ecology;
        private TradeEconomySystem _economy;
        private D9FishingSystem _fishing;

        [SetUp]
        public void SetUp()
        {
            _trade = new D3TradeState(500, 8);
            CommodityCatalog.SeedMarket(_trade.Market);
            _economy = new TradeEconomySystem(new EventLog());
            _ecology = new D4EcologySystem();
            _fishing = new D9FishingSystem(_trade, _ecology, _economy, _economy.EventLog);
            _fishing.PrepareNearshoreSchool(0, 17);
        }

        [Test]
        public void Fishing_UsesEcologyForSchoolAndInventoryForCatch()
        {
            var before = _fishing.FishSchool;
            var result = _fishing.Fish(new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 0,
                Seed = 17,
                SaveRevision = 1,
            });

            Assert.IsTrue(result.Succeeded, result.Message);
            Assert.AreEqual(D9FishingIds.FreshFishItemId, result.ItemId);
            Assert.Greater(result.Quantity, 0);
            Assert.AreEqual(result.Quantity, D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.FreshFishItemId));
            Assert.Less(_fishing.FishSchool.Biomass, before.Biomass);
            Assert.AreEqual("EcologySystem", result.Fact.OwnerSystem);
            Assert.AreEqual(D9FishingIds.CatchEventId, result.Fact.EventId);
        }

        [Test]
        public void Fishing_IsIdempotentForEventIdAndSaveRevision()
        {
            var request = new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 0,
                Seed = 17,
                SaveRevision = 9,
            };
            var first = _fishing.Fish(request);
            int cargoAfterFirst = D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.FreshFishItemId);
            int biomassAfterFirst = _fishing.FishSchool.Biomass;
            var duplicate = _fishing.Fish(request);

            Assert.IsTrue(first.Succeeded);
            Assert.IsTrue(duplicate.Succeeded);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(cargoAfterFirst, D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.FreshFishItemId));
            Assert.AreEqual(biomassAfterFirst, _fishing.FishSchool.Biomass);
        }

        [Test]
        public void FreshFish_CanBeProcessed_SoldOrCookedAtPort()
        {
            var catchResult = _fishing.Fish(new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 0,
                Seed = 22,
                SaveRevision = 2,
            });
            Assert.IsTrue(catchResult.Succeeded);

            var processed = _fishing.ProcessFreshFish(catchResult.Quantity);
            Assert.IsTrue(processed.Succeeded, processed.Message);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.FreshFishItemId));
            Assert.AreEqual(catchResult.Quantity, D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.SaltedFishItemId));

            int moneyBefore = _trade.Money;
            var sold = _fishing.SellFish(PortCatalog.StartingPortId, D9FishingIds.SaltedFishItemId, 1, 12);
            Assert.IsTrue(sold.Succeeded, sold.Message);
            Assert.Greater(_trade.Money, moneyBefore);
            Assert.AreEqual(catchResult.Quantity - 1,
                D3InventorySystem.CountCargo(_trade.Inventory, D9FishingIds.SaltedFishItemId));

            var freshAgain = _fishing.Fish(new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 1,
                Seed = 23,
                SaveRevision = 3,
            });
            Assert.IsTrue(freshAgain.Succeeded);
            var cooked = _fishing.CookFreshFish(PortCatalog.StartingPortId, 1);
            Assert.IsTrue(cooked.Succeeded, cooked.Message);
            Assert.AreEqual(1, D3InventorySystem.CountPersonal(_trade.Inventory, D9FishingIds.CookedFishItemId));
        }

        [Test]
        public void FishingSnapshot_RestoresCatchFactsAndDuplicateGate()
        {
            var request = new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = 0,
                Seed = 17,
                SaveRevision = 4,
            };
            var first = _fishing.Fish(request);
            var saved = _fishing.CaptureSnapshot();
            var restoredTrade = new D3TradeState(_trade.Money, _trade.Inventory.CargoCapacity);
            foreach (var pair in _trade.Inventory.Cargo) restoredTrade.Inventory.Cargo[pair.Key] = pair.Value;
            foreach (var pair in _trade.Inventory.PersonalItems) restoredTrade.Inventory.PersonalItems[pair.Key] = pair.Value;
            foreach (var pair in _trade.Market.Stock) restoredTrade.Market.Stock[pair.Key] = pair.Value;

            var restoredEconomy = new TradeEconomySystem(new EventLog());
            var restored = new D9FishingSystem(restoredTrade,
                new D4EcologySystem(_ecology.Snapshot), restoredEconomy, restoredEconomy.EventLog);
            restored.Restore(saved);
            var duplicate = restored.Fish(request);

            Assert.IsTrue(first.Succeeded);
            Assert.AreEqual(1, restored.EventLog.Count);
            Assert.AreEqual(saved.NextCatchRevision, restored.CaptureSnapshot().NextCatchRevision);
            Assert.IsTrue(duplicate.Succeeded);
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(first.Quantity,
                D3InventorySystem.CountCargo(restoredTrade.Inventory, D9FishingIds.FreshFishItemId));
        }
    }
}
