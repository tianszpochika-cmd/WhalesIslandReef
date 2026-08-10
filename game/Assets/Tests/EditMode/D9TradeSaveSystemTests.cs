using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.Save;

namespace D9.Tests
{
    public class D9TradeSaveSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d9_trade_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void TradeSnapshot_SaveLoad_PreservesMarketDriversAndFactLog()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D9 贸易船长" });
            var trade = new D3TradeState(100000, 100);
            CommodityCatalog.SeedMarket(trade.Market);
            trade.Market.Weather = D3WeatherType.Storm;
            trade.Market.FactionPolicyId = "harbor_embargo";
            trade.Market.FactionPolicyCoefficient = 1.15f;
            var economy = new TradeEconomySystem();
            var transaction = economy.Buy(trade, PortCatalog.StartingPortId, "sugar", 2);
            Assert.IsTrue(transaction.Succeeded);

            var snapshot = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Weather = trade.Market.Weather,
                Money = trade.Money,
                SaveRevision = economy.CurrentSaveRevision,
                FactionPolicyId = trade.Market.FactionPolicyId,
                FactionPolicyCoefficient = trade.Market.FactionPolicyCoefficient,
                Cargo = new Dictionary<string, int>(trade.Inventory.Cargo),
                MarketStock = new Dictionary<string, int>(trade.Market.Stock),
                MarketDemand = new Dictionary<string, int>(trade.Market.Demand),
                PlayerTradeVolume = new Dictionary<string, int>(trade.Market.PlayerTradeVolume),
                EventLog = economy.CaptureEventLogSnapshot(),
            };
            SaveSystem.SaveD3Runtime(snapshot);

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD3Runtime();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(2, loaded.Cargo["sugar"]);
            Assert.AreEqual(snapshot.MarketStock["sugar"], loaded.MarketStock["sugar"]);
            Assert.AreEqual(snapshot.MarketDemand["sugar"], loaded.MarketDemand["sugar"]);
            Assert.AreEqual(snapshot.PlayerTradeVolume["sugar"], loaded.PlayerTradeVolume["sugar"]);
            Assert.AreEqual(D3WeatherType.Storm, loaded.Weather);
            Assert.AreEqual("harbor_embargo", loaded.FactionPolicyId);
            Assert.IsNotNull(loaded.EventLog);
            Assert.AreEqual(1, loaded.EventLog.Facts.Count);
            Assert.AreEqual(TradeEconomySystem.TradeSettledEventId, loaded.EventLog.Facts[0].EventId);

            var restoredEconomy = new TradeEconomySystem();
            restoredEconomy.RestoreEventLog(loaded.EventLog);
            var restoredTrade = new D3TradeState(100000, 100);
            CommodityCatalog.SeedMarket(restoredTrade.Market);
            var replay = restoredEconomy.Buy(restoredTrade, PortCatalog.StartingPortId, "sugar", 2,
                new TradePriceContext { Weather = D3WeatherType.Storm }, transaction.SaveRevision);
            Assert.IsTrue(replay.DuplicateSettlement);
            Assert.AreEqual(100000, restoredTrade.Money);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(restoredTrade.Inventory, "sugar"));
        }
    }
}
