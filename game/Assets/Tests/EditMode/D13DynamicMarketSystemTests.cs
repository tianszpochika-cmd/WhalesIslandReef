// D13-A07 动态市场验收：商品流、运输损耗、法律规则、库存/市场唯一写者和读档幂等。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D3;
using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13DynamicMarketSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_market_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = _dbPath + suffix;
                if (File.Exists(path)) try { File.Delete(path); } catch { }
            }
        }

        [Test]
        public void ProductionTransportConsumptionSaleAndSaveRestoreAreTraceable()
        {
            var log = CreateSources("production_source", "consumer_source", "weather_source", "ecology_source", "asset_source");
            var fixture = CreateFixture(log, 50, true);
            var inventory = fixture.InventoryState;
            AddCargo(inventory, "item_fruit_guava", 12);
            var system = fixture.System;

            var production = system.Produce(new D13MarketProductionRequest
            {
                OperationId = "produce_guava", ItemId = "item_fruit_guava", RequestedQuantity = 4,
                SourceLocationId = "island_saint_marys", InventoryHolderId = "player_hold", MarketId = PortCatalog.StartingPortId,
                WorldDay = 1, SaveRevision = 1, CauseEventIds = new List<string> { "production_source" },
            });
            Assert.IsTrue(production.Succeeded, production.Message);
            Assert.AreEqual(16, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));
            Assert.AreEqual(D13MarketFlowKind.Production, production.Flow.Kind);
            Assert.IsNotEmpty(production.Flow.EcologySourceEventId);

            var loaded = system.CreateShipment(new D13MarketShipmentRequest
            {
                OperationId = "load_guava", RouteId = "route_santa_margarita_to_green_cay", ItemId = "item_fruit_guava",
                Quantity = 8, InventoryHolderId = "player_hold", ShipId = "ship_test", WorldDay = 2, SaveRevision = 2,
                CauseEventIds = new List<string> { production.EventId },
            });
            Assert.IsTrue(loaded.Succeeded, loaded.Message);
            Assert.AreEqual(8, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));

            var advanced = system.AdvanceShipment(new D13MarketShipmentAdvanceRequest
            {
                OperationId = "move_guava_storm", ShipmentId = loaded.Shipment.ShipmentId, Days = 1,
                Weather = D3WeatherType.Storm, AssetConditionPercent = 50, EcologyPressurePercent = 40,
                WeatherSourceEventId = "weather_source", EcologySourceEventId = "ecology_source", AssetSourceEventId = "asset_source",
                WorldDay = 3, SaveRevision = 3, CauseEventIds = new List<string> { "weather_source", "ecology_source", "asset_source" },
            });
            Assert.IsTrue(advanced.Succeeded, advanced.Message);
            Assert.AreEqual(3, advanced.Shipment.LostQuantity);
            Assert.AreEqual(5, advanced.Shipment.RemainingQuantity);
            Assert.AreEqual(10, advanced.Shipment.LastWeatherRiskPercent);
            Assert.AreEqual(10, advanced.Shipment.LastAssetRiskPercent);
            Assert.AreEqual(4, advanced.Shipment.LastEcologyRiskPercent);

            var arrived = system.ArriveShipment(new D13MarketShipmentArrivalRequest
            {
                OperationId = "arrive_guava", ShipmentId = loaded.Shipment.ShipmentId, InventoryHolderId = "player_hold",
                WorldDay = 4, SaveRevision = 4, CauseEventIds = new List<string> { advanced.EventId },
            });
            Assert.IsTrue(arrived.Succeeded, arrived.Message);
            Assert.AreEqual(D13TransportStatus.Arrived, arrived.Shipment.Status);
            Assert.AreEqual(13, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));

            var quote = system.Quote(new D13MarketQuoteRequest
            {
                OperationId = "quote_guava", MarketId = PortCatalog.GreenCayId, ItemId = "item_fruit_guava",
                WorldDay = 4, SaveRevision = 4,
            });
            Assert.IsTrue(quote.Succeeded, quote.Message);
            Assert.IsTrue(quote.Quote.UnitPrice > 0);
            Assert.IsNotEmpty(quote.Quote.Reasons);

            var sale = system.Sell(new D13MarketSaleRequest
            {
                OperationId = "sell_guava", ItemId = "item_fruit_guava", Quantity = 3, SellerId = "player",
                LocationId = PortCatalog.GreenCayId, InventoryHolderId = "player_hold", MarketId = PortCatalog.GreenCayId,
                WorldDay = 5, SaveRevision = 5, CauseEventIds = new List<string> { arrived.EventId },
            });
            Assert.IsTrue(sale.Succeeded, sale.Message);
            Assert.AreEqual(D13MarketFlowKind.Sale, sale.Flow.Kind);
            Assert.IsTrue(sale.Flow.TotalValue > 0);
            Assert.AreEqual(10, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));

            var consumption = system.Consume(new D13MarketConsumptionRequest
            {
                OperationId = "consume_guava", ItemId = "item_fruit_guava", Quantity = 2, ConsumerId = "island_household",
                LocationId = PortCatalog.GreenCayId, InventoryHolderId = "player_hold", MarketId = PortCatalog.GreenCayId,
                ConsumptionReason = "island_food", WorldDay = 6, SaveRevision = 6, CauseEventIds = new List<string> { "consumer_source" },
            });
            Assert.IsTrue(consumption.Succeeded, consumption.Message);
            Assert.AreEqual(D13MarketFlowKind.Consumption, consumption.Flow.Kind);
            Assert.AreEqual(8, D3InventorySystem.CountCargo(inventory, "item_fruit_guava"));
            Assert.AreEqual(3, system.GetReadModel().Flows.Count);
            Assert.AreEqual(55, fixture.Market.Stock["item_fruit_guava"]);
            Assert.AreEqual(45, fixture.Market.Demand["item_fruit_guava"]);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D13-A07 市场测试船长", ShipTypeId = "small_fishing" });
            var snapshot = system.CaptureSnapshot();
            SaveSystem.SaveD13DynamicMarketRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load());
            var loadedSnapshot = SaveSystem.LoadD13DynamicMarketRuntime();
            Assert.IsNotNull(loadedSnapshot);
            Assert.AreEqual(snapshot.Flows.Count, loadedSnapshot.Flows.Count);
            Assert.AreEqual(snapshot.Shipments.Count, loadedSnapshot.Shipments.Count);

            var duplicateFixture = CreateFixture(log, 50, true, loadedSnapshot);
            var duplicate = duplicateFixture.System.Consume(new D13MarketConsumptionRequest
            {
                OperationId = "consume_guava", ItemId = "item_fruit_guava", Quantity = 2, ConsumerId = "island_household",
                LocationId = PortCatalog.GreenCayId, InventoryHolderId = "player_hold", MarketId = PortCatalog.GreenCayId,
                ConsumptionReason = "island_food", WorldDay = 6, SaveRevision = 6, CauseEventIds = new List<string> { "consumer_source" },
            });
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(duplicateFixture.InventoryState, "item_fruit_guava"));
        }

        [Test]
        public void WeatherAssetAndEcologyProduceDeterministicTransportLoss()
        {
            var log = CreateSources("weather_calm", "weather_hurricane", "ecology_source", "asset_source");
            var fixture = CreateFixture(log, 50, true);
            AddCargo(fixture.InventoryState, "item_fruit_guava", 20);

            var calmLoad = fixture.System.CreateShipment(new D13MarketShipmentRequest
            {
                OperationId = "load_calm", RouteId = "route_santa_margarita_to_green_cay", ItemId = "item_fruit_guava", Quantity = 6,
                InventoryHolderId = "player_hold", ShipId = "ship_test", WorldDay = 1, SaveRevision = 1,
            });
            var hurricaneLoad = fixture.System.CreateShipment(new D13MarketShipmentRequest
            {
                OperationId = "load_hurricane", RouteId = "route_santa_margarita_to_green_cay", ItemId = "item_fruit_guava", Quantity = 6,
                InventoryHolderId = "player_hold", ShipId = "ship_test", WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(calmLoad.Succeeded && hurricaneLoad.Succeeded);

            var calm = fixture.System.AdvanceShipment(new D13MarketShipmentAdvanceRequest
            {
                OperationId = "advance_calm", ShipmentId = calmLoad.Shipment.ShipmentId, Weather = D3WeatherType.Calm,
                AssetConditionPercent = 50, EcologyPressurePercent = 40, WeatherSourceEventId = "weather_calm",
                EcologySourceEventId = "ecology_source", AssetSourceEventId = "asset_source", WorldDay = 2, SaveRevision = 2,
            });
            var hurricane = fixture.System.AdvanceShipment(new D13MarketShipmentAdvanceRequest
            {
                OperationId = "advance_hurricane", ShipmentId = hurricaneLoad.Shipment.ShipmentId, Weather = D3WeatherType.Hurricane,
                AssetConditionPercent = 50, EcologyPressurePercent = 40, WeatherSourceEventId = "weather_hurricane",
                EcologySourceEventId = "ecology_source", AssetSourceEventId = "asset_source", WorldDay = 2, SaveRevision = 2,
            });
            Assert.IsTrue(calm.Succeeded && hurricane.Succeeded);
            Assert.IsTrue(hurricane.Shipment.LostQuantity > calm.Shipment.LostQuantity);
            Assert.AreEqual(22, hurricane.Shipment.LastWeatherRiskPercent);
            Assert.AreEqual(10, hurricane.Shipment.LastAssetRiskPercent);
            Assert.AreEqual(4, hurricane.Shipment.LastEcologyRiskPercent);
        }

        [Test]
        public void RestrictedAndIllegalGoodsRequireLegalDecisionAndPermit()
        {
            var log = CreateSources("production_source");
            var fixture = CreateFixture(log, 100, true);
            AddCargo(fixture.InventoryState, "item_special_weapon_moonwake_cutlass", 1);
            int before = D3InventorySystem.CountCargo(fixture.InventoryState, "item_special_weapon_moonwake_cutlass");

            var denied = fixture.System.CreateShipment(new D13MarketShipmentRequest
            {
                OperationId = "ship_restricted_without_permit", RouteId = "route_santa_margarita_to_green_cay",
                ItemId = "item_special_weapon_moonwake_cutlass", Quantity = 1, InventoryHolderId = "player_hold",
                ShipId = "ship_test", WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsFalse(denied.Succeeded);
            Assert.AreEqual(D13MarketTransportIds.LegalRecoveryRoute, denied.RecoveryRouteId);
            Assert.AreEqual(before, D3InventorySystem.CountCargo(fixture.InventoryState, "item_special_weapon_moonwake_cutlass"));

            var permitted = fixture.System.CreateShipment(new D13MarketShipmentRequest
            {
                OperationId = "ship_restricted_with_permit", RouteId = "route_santa_margarita_to_green_cay",
                ItemId = "item_special_weapon_moonwake_cutlass", Quantity = 1, InventoryHolderId = "player_hold",
                ShipId = "ship_test", PermitId = "permit_armory_moonwake", WorldDay = 2, SaveRevision = 2,
            });
            Assert.IsTrue(permitted.Succeeded, permitted.Message);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(fixture.InventoryState, "item_special_weapon_moonwake_cutlass"));

            var illegal = fixture.System.Produce(new D13MarketProductionRequest
            {
                OperationId = "produce_illegal_toxin", ItemId = "item_toxin_datura", RequestedQuantity = 1,
                SourceLocationId = "island_driftwood_isle", InventoryHolderId = "player_hold", MarketId = PortCatalog.DriftwoodIsleId,
                WorldDay = 3, SaveRevision = 3, CauseEventIds = new List<string> { "production_source" },
            });
            Assert.IsFalse(illegal.Succeeded);
            Assert.IsNull(fixture.System.FindFlow("event_d13_market_produce_illegal_toxin"));
        }

        private static Fixture CreateFixture(EventLog log, int condition, bool allowLegal, D13DynamicMarketRuntimeSnapshot initial = null)
        {
            var inventoryState = new D3InventoryState(100);
            var market = new D3MarketState();
            var economy = new TradeEconomySystem(log);
            var inventory = new D13D3InventoryAuthority(inventoryState, log);
            var economyAuthority = new D13TradeEconomyAuthority(economy, market, log);
            var system = new D13DynamicMarketTransportSystem(
                new FixtureLegalAuthority(allowLegal),
                new FixtureProductionAuthority(),
                new FixtureConsumptionAuthority(),
                inventory,
                economyAuthority,
                new FixtureTransportAuthority(condition),
                log,
                initial);
            return new Fixture { System = system, InventoryState = inventoryState, Market = market };
        }

        private static void AddCargo(D3InventoryState state, string itemId, int quantity)
        {
            string error;
            Assert.IsTrue(D3InventorySystem.TryAddCargo(state, itemId, quantity, out error), error);
        }

        private static EventLog CreateSources(params string[] ids)
        {
            var log = new EventLog();
            foreach (var id in ids)
            {
                log.Append(new WorldEvent(id, 0, "test_source", "FixtureSourceSystem",
                    new EventResult { ResultCode = "source_ready", Summary = "测试来源事实。", FactIds = new List<string> { "source:" + id } },
                    new[] { "player" }, new[] { "test:d13_a07" }));
            }
            log.Append(new WorldEvent("legal_source", 0, "test_source", D13MarketTransportIds.LegalOwner,
                new EventResult { ResultCode = "legal_ready", Summary = "法律来源事实。", FactIds = new List<string> { "source:legal" } },
                new[] { "player" }, new[] { "legal:d13_a07" }));
            return log;
        }

        private sealed class Fixture
        {
            public D13DynamicMarketTransportSystem System;
            public D3InventoryState InventoryState;
            public D3MarketState Market;
        }

        private sealed class FixtureLegalAuthority : ID13MarketLegalAuthority
        {
            private readonly bool _allow;
            public FixtureLegalAuthority(bool allow) { _allow = allow; }

            public D13MarketLegalDecision ValidateGood(D13MarketGoodDefinition definition, string locationId, string permitId, D13MarketOperationRequest request)
            {
                if (!_allow || definition.LegalStatus == D13MarketLegalStatus.Illegal)
                    return new D13MarketLegalDecision { Allowed = false, Code = "legal_denied", Message = "测试法律裁决拒绝。", LegalStatus = definition.LegalStatus, SourceEventId = "legal_source", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
                if (definition.RequiresPermit && permitId != definition.PermitId)
                    return new D13MarketLegalDecision { Allowed = false, Code = "permit_required", Message = "缺少许可。", LegalStatus = definition.LegalStatus, SourceEventId = "legal_source", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
                return new D13MarketLegalDecision { Allowed = true, Code = "legal_allowed", LegalStatus = definition.RequiresPermit ? D13MarketLegalStatus.Licensed : definition.LegalStatus, PermitId = definition.RequiresPermit ? definition.PermitId : null, SourceEventId = "legal_source", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
            }

            public D13MarketLegalDecision ValidateRoute(D13MarketRouteDefinition route, string permitId, D13MarketShipmentRequest request)
            {
                if (!_allow || permitId == null && request.ItemId == "item_special_weapon_moonwake_cutlass")
                    return new D13MarketLegalDecision { Allowed = false, Code = "permit_required", Message = "运输需要法律许可。", LegalStatus = D13MarketLegalStatus.Restricted, SourceEventId = "legal_source", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
                return new D13MarketLegalDecision { Allowed = true, Code = "route_legal", LegalStatus = request.ItemId == "item_special_weapon_moonwake_cutlass" ? D13MarketLegalStatus.Licensed : D13MarketLegalStatus.Legal, PermitId = permitId, SourceEventId = "legal_source", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
            }
        }

        private sealed class FixtureProductionAuthority : ID13MarketProductionAuthority
        {
            public D13MarketProductionDecision ValidateProduction(D13MarketGoodDefinition definition, D13MarketProductionRequest request)
            {
                return new D13MarketProductionDecision { Allowed = true, Code = "production_ready", Quantity = request.RequestedQuantity,
                    SourceEventId = "production_source", SourceOwnerSystem = definition.AllowedProducerOwners[0], EcologySourceEventId = "ecology_source", AssetSourceEventId = "asset_source" };
            }
        }

        private sealed class FixtureConsumptionAuthority : ID13MarketConsumptionAuthority
        {
            public D13MarketConsumptionDecision ValidateConsumption(D13MarketGoodDefinition definition, D13MarketConsumptionRequest request)
            {
                return new D13MarketConsumptionDecision { Allowed = true, Code = "consumption_ready", SourceEventId = "consumer_source", SourceOwnerSystem = "IslandGovernanceSystem" };
            }
        }

        private sealed class FixtureTransportAuthority : ID13TransportAssetAuthority
        {
            private readonly int _condition;
            public FixtureTransportAuthority(int condition) { _condition = condition; }
            public D13TransportAssetDecision ValidateTransportAsset(D13MarketRouteDefinition route, D13MarketShipmentAdvanceRequest request)
            {
                return new D13TransportAssetDecision { Allowed = true, Code = "ship_ready", ConditionPercent = _condition,
                    SourceEventId = request.AssetSourceEventId, SourceOwnerSystem = D13MarketTransportIds.ShipOwner };
            }
        }
    }
}
