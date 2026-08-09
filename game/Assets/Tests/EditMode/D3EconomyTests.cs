using NUnit.Framework;
using Whalebone.Systems.D3;

namespace D3.Tests
{
    public class D3EconomyTests
    {
        private static D3TradeState NewState()
        {
            var state = new D3TradeState(500, 40);
            CommodityCatalog.SeedMarket(state.Market);
            return state;
        }

        [Test]
        public void Quote_ContainsFormulaComponentsAndPositivePrices()
        {
            var state = NewState();
            var quote = new TradeEconomySystem().Quote(PortCatalog.StartingPortId, "salt", state.Market, 1.1f);
            Assert.IsNotNull(quote);
            Assert.AreEqual(1f, quote.SupplyCoefficient, 0.0001f);
            Assert.AreEqual(1.1f, quote.EventCoefficient, 0.0001f);
            Assert.Greater(quote.BuyUnitPrice, 0);
            Assert.Greater(quote.SellUnitPrice, 0);
            Assert.Greater(quote.BuyUnitPrice, quote.SellUnitPrice);
        }

        [Test]
        public void Buy_ChangesMoneyCargoAndMarketStock()
        {
            var state = NewState();
            int beforeStock = state.Market.Stock["sugar"];
            var result = new TradeEconomySystem().Buy(state, PortCatalog.StartingPortId, "sugar", 3);
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(3, D3InventorySystem.CountCargo(state.Inventory, "sugar"));
            Assert.AreEqual(beforeStock - 3, state.Market.Stock["sugar"]);
            Assert.AreEqual(500 - result.TotalPrice, state.Money);
        }

        [Test]
        public void Buy_RejectsInsufficientMoneyAndCapacity()
        {
            var economy = new TradeEconomySystem();
            var poor = new D3TradeState(1, 40);
            CommodityCatalog.SeedMarket(poor.Market);
            Assert.IsFalse(economy.Buy(poor, PortCatalog.StartingPortId, "medicine", 1).Succeeded);
            Assert.AreEqual("not_enough_money", economy.Buy(poor, PortCatalog.StartingPortId, "medicine", 1).Code);

            var full = new D3TradeState(10000, 1);
            CommodityCatalog.SeedMarket(full.Market);
            Assert.IsTrue(economy.Buy(full, PortCatalog.StartingPortId, "salt", 1).Succeeded);
            Assert.AreEqual("cargo_full", economy.Buy(full, PortCatalog.StartingPortId, "salt", 1).Code);
        }

        [Test]
        public void Sell_ReturnsMoneyAndRestocksMarket()
        {
            var state = NewState();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(state.Inventory, "tobacco", 2, out _));
            int beforeMoney = state.Money;
            var result = new TradeEconomySystem().Sell(state, PortCatalog.GreenCayId, "tobacco", 2);
            Assert.IsTrue(result.Succeeded);
            Assert.Greater(state.Money, beforeMoney);
            Assert.AreEqual(52, state.Market.Stock["tobacco"]);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(state.Inventory, "tobacco"));
        }

        [Test]
        public void Gathering_RespectsIslandTypeAndVisitLimit()
        {
            var inventory = new D3InventoryState(30);
            var gathered = new System.Collections.Generic.Dictionary<string, int>();
            var gathering = new GatheringSystem();
            Assert.IsTrue(gathering.Gather(inventory, gathered, PortCatalog.DriftwoodIsleId, "timber", 12).Succeeded);
            Assert.IsFalse(gathering.Gather(inventory, gathered, PortCatalog.DriftwoodIsleId, "timber", 1).Succeeded);
            Assert.AreEqual("gathering_limit", gathering.Gather(
                inventory, gathered, PortCatalog.DriftwoodIsleId, "timber", 1).Code);
            Assert.AreEqual("not_gatherable", gathering.Gather(
                inventory, gathered, PortCatalog.StartingPortId, "timber", 1).Code);
        }
    }
}
