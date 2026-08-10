using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Save;

namespace D9.Tests
{
    public class D9PortShipPreparationTests
    {
        private static ShipState NewShip()
        {
            return ShipState.CreateNew("ship_test", ShipTypeSeed.Find("small_fishing"), "player_test");
        }

        private static D3TradeState NewTrade()
        {
            var trade = new D3TradeState(500, 40);
            CommodityCatalog.SeedMarket(trade.Market);
            return trade;
        }

        private static D3RuntimeSnapshot InPort()
        {
            return new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Money = 500,
            };
        }

        [Test]
        public void BuyCargoChangesPreparationWeight()
        {
            var ship = NewShip();
            var trade = NewTrade();
            var prep = new D9PortShipPreparationSystem();
            Assert.IsTrue(prep.EnterPort(InPort(), ship, PortCatalog.StartingPortId).Accepted);

            var result = prep.Buy(trade, ship, PortCatalog.StartingPortId, "timber", 2);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(4, D3InventorySystem.UsedCargo(trade.Inventory));
            Assert.AreEqual(4, prep.BuildSnapshot(InPort(), trade, ship, null, null).Ship.Weight);
        }

        [Test]
        public void RepairConsumesTimberAndMoney()
        {
            var ship = NewShip();
            ship.Hull = 40;
            var trade = NewTrade();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory, D9PortShipPreparationSystem.RepairMaterialId, 2, out _));
            var prep = new D9PortShipPreparationSystem();
            Assert.IsTrue(prep.EnterPort(InPort(), ship, PortCatalog.StartingPortId).Accepted);

            var result = prep.Repair(trade, ship, PortCatalog.StartingPortId, 10);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(50, ship.Hull);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(trade.Inventory, "timber"));
            Assert.AreEqual(420, trade.Money);
        }

        [Test]
        public void WagesChangeCrewAndRecruitmentFillsShipRoster()
        {
            var ship = NewShip();
            var trade = NewTrade();
            var crew = new List<CrewAgent>();
            var existing = new CrewAgent("crew_existing", new PersonaDefinition("老水手", "水手"), new InMemoryStore());
            crew.Add(existing);
            ship.CrewIds.Add(existing.Id);
            var prep = new D9PortShipPreparationSystem();
            Assert.IsTrue(prep.EnterPort(InPort(), ship, PortCatalog.StartingPortId).Accepted);

            float moraleBefore = existing.Stats.Morale;
            var wages = prep.SettleWages(trade, ship, PortCatalog.StartingPortId, crew);
            var recruit = prep.Recruit(trade, ship, PortCatalog.StartingPortId, crew, D9CrewRecruitmentCatalog.CreateIsaacReed());

            Assert.IsTrue(wages.Accepted);
            Assert.Greater(existing.Stats.Morale, moraleBefore);
            Assert.IsTrue(recruit.Accepted);
            Assert.Contains(D9CrewRecruitmentCatalog.IsaacReedId, ship.CrewIds);
        }

        [Test]
        public void DepartureLeavesPortAndEntersSailing()
        {
            var ship = NewShip();
            ship.CrewIds.Add("crew_existing");
            var trade = NewTrade();
            var runtime = InPort();
            var prep = new D9PortShipPreparationSystem();
            Assert.IsTrue(prep.EnterPort(runtime, ship, PortCatalog.StartingPortId).Accepted);

            var result = prep.Depart(runtime, ship, PortCatalog.GreenCayId, trade.Money);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(D3VoyageStatus.Sailing, runtime.VoyageStatus);
            Assert.IsFalse(prep.BuildSnapshot(runtime, trade, ship, null, null).Ship.Moored);
        }
    }
}
