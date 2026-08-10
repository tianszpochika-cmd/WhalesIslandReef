using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D3;

namespace D9.Tests
{
    public class D9TradeVerticalSliceTests
    {
        private static D3TradeState NewTrade(int money = 100000, int volumeCapacity = 100)
        {
            var trade = new D3TradeState(money, volumeCapacity);
            CommodityCatalog.SeedMarket(trade.Market);
            return trade;
        }

        [Test]
        public void CommodityCatalog_ContainsFrozenTenWithWeightVolumeAndDemand()
        {
            var ids = new[] { "salt", "sugar", "tobacco", "rum", "coffee", "cocoa", "spices", "dyes", "timber", "medicine" };
            Assert.AreEqual(10, CommodityCatalog.All.Count);
            foreach (var id in ids)
            {
                var commodity = CommodityCatalog.Find(id);
                Assert.IsNotNull(commodity, id);
                Assert.Greater(commodity.BasePrice, 0);
                Assert.Greater(commodity.UnitWeight, 0);
                Assert.Greater(commodity.CargoVolume, 0);
                Assert.Greater(commodity.DemandTarget, 0);
            }
        }

        [Test]
        public void Inventory_ReportsWeightAndVolumeAndRejectsBothLimits()
        {
            var inventory = new D3InventoryState(3, 2);
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "timber", 1, out _));
            Assert.AreEqual(2, D3InventorySystem.UsedWeight(inventory));
            Assert.AreEqual(2, D3InventorySystem.UsedVolume(inventory));
            Assert.IsFalse(D3InventorySystem.TryAddCargo(inventory, "salt", 2, out string volumeError));
            Assert.AreEqual("cargo_full", volumeError);

            Assert.IsFalse(D3InventorySystem.TryAddCargo(inventory, "salt", 1, out string weightError));
            Assert.AreEqual("weight_full", weightError);
            Assert.AreEqual(2, D3InventorySystem.UsedWeight(inventory));
            Assert.AreEqual(2, D3InventorySystem.UsedVolume(inventory));
        }

        [Test]
        public void Quote_ExplainsInventoryDemandWeatherVolumePolicyAndTax()
        {
            var trade = NewTrade();
            trade.Market.Stock["salt"] = 10;
            trade.Market.Demand["salt"] = 80;
            trade.Market.PlayerTradeVolume["salt"] = 100;
            var context = new TradePriceContext
            {
                Weather = D3WeatherType.Storm,
                FactionPolicyId = "harbor_embargo",
                FactionPolicyCoefficient = 1.2f,
            };

            var quote = new TradeEconomySystem().Quote(
                PortCatalog.StartingPortId, "salt", trade.Market, context);

            Assert.IsNotNull(quote);
            Assert.Greater(quote.BuyUnitPrice, quote.SellUnitPrice);
            Assert.IsFalse(quote.IsOutOfStock);
            Assert.AreEqual(6, quote.PriceReasons.Count);
            var codes = new HashSet<string>();
            foreach (var reason in quote.PriceReasons)
            {
                codes.Add(reason.Code);
                Assert.IsNotEmpty(reason.Explanation);
            }
            CollectionAssert.IsSubsetOf(
                new[] { "inventory", "demand", "weather", "player_trade_volume", "faction_policy", "tax" }, codes);
        }

        [Test]
        public void AllTenCommodities_CanBuyAndSellAndAppendFacts()
        {
            var trade = NewTrade();
            var economy = new TradeEconomySystem();

            foreach (var commodity in CommodityCatalog.All)
            {
                var buy = economy.Buy(trade, PortCatalog.StartingPortId, commodity.Id, 1);
                Assert.IsTrue(buy.Succeeded, commodity.Id + " buy failed: " + buy.Code);
                Assert.IsTrue(buy.FactLogged, commodity.Id + " buy was not logged");
                Assert.AreEqual(commodity.Id, buy.CommodityId);

                var sell = economy.Sell(trade, PortCatalog.GreenCayId, commodity.Id, 1);
                Assert.IsTrue(sell.Succeeded, commodity.Id + " sell failed: " + sell.Code);
                Assert.IsTrue(sell.FactLogged, commodity.Id + " sell was not logged");
                Assert.AreEqual(0, D3InventorySystem.CountCargo(trade.Inventory, commodity.Id));
            }

            Assert.AreEqual(20, economy.EventLog.Count);
            Assert.AreEqual(20, economy.CurrentSaveRevision);
            foreach (var fact in economy.EventLog.Facts)
            {
                Assert.AreEqual(TradeEconomySystem.TradeSettledEventId, fact.EventId);
                Assert.AreEqual("EconomySystem", fact.OwnerSystem);
                Assert.IsTrue(fact.Result.Values.ContainsKey("commodity_id"));
            }
        }

        [Test]
        public void OutOfStock_IsVisibleAndBuyDoesNotChangeState()
        {
            var trade = NewTrade();
            var economy = new TradeEconomySystem();
            trade.Market.Stock["medicine"] = 0;
            var quote = economy.Quote(PortCatalog.StartingPortId, "medicine", trade.Market);
            int beforeMoney = trade.Money;

            Assert.IsTrue(quote.IsOutOfStock);
            var result = economy.Buy(trade, PortCatalog.StartingPortId, "medicine", 1);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("market_empty", result.Code);
            Assert.AreEqual(beforeMoney, trade.Money);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(trade.Inventory, "medicine"));
            Assert.AreEqual(0, economy.EventLog.Count);
        }

        [Test]
        public void SaveSnapshotRestore_ProtectsTradeReplay()
        {
            var trade = NewTrade();
            var economy = new TradeEconomySystem();
            var first = economy.Buy(trade, PortCatalog.StartingPortId, "sugar", 2);
            var snapshot = new D3RuntimeSnapshot
            {
                Money = trade.Money,
                SaveRevision = economy.CurrentSaveRevision,
                MarketStock = new Dictionary<string, int>(trade.Market.Stock),
                MarketDemand = new Dictionary<string, int>(trade.Market.Demand),
                PlayerTradeVolume = new Dictionary<string, int>(trade.Market.PlayerTradeVolume),
                EventLog = economy.CaptureEventLogSnapshot(),
            };

            var loaded = snapshot.Clone();
            var restoredEconomy = new TradeEconomySystem();
            restoredEconomy.RestoreEventLog(loaded.EventLog);
            var restoredTrade = NewTrade();
            int moneyBeforeReplay = restoredTrade.Money;
            var replay = restoredEconomy.Buy(restoredTrade, PortCatalog.StartingPortId, "sugar", 2,
                new TradePriceContext(), first.SaveRevision);

            Assert.IsTrue(replay.Succeeded);
            Assert.IsTrue(replay.DuplicateSettlement);
            Assert.AreEqual(moneyBeforeReplay, restoredTrade.Money);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(restoredTrade.Inventory, "sugar"));
            Assert.AreEqual(1, restoredEconomy.EventLog.Count);
            Assert.AreEqual(first.SaveRevision, restoredEconomy.EventLog.Facts[0].SaveRevision);
        }
    }
}
