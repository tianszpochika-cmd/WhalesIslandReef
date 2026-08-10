// D13-A06 建筑测试：建设订单、材料/工人、工期、服务价格、仓储、火灾、占领、维护、船坞和存档幂等。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13BuildingSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_building_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void ConstructionMaterialsWorkersProductionAndSaveRestoreAreTraceable()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "建筑测试船长", ShipTypeId = "small_fishing" });
            var log = CreateSources();
            var inventory = new FixtureInventory(log, true);
            var system = NewSystem(log, inventory, true, true, true, true);
            var definition = D13BuildingCatalog.Find(D13BuildingCatalog.WorkshopId);
            Assert.AreEqual(D13BuildingKind.Workshop, definition.Kind);

            var order = system.PlaceConstructionOrder(new D13ConstructionOrderRequest
            {
                OperationId = "build_workshop", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                Workers = 2, WorldDay = 1, WorldHour = 9, SaveRevision = 1,
            });
            Assert.IsTrue(order.Succeeded, order.Message);
            Assert.AreEqual(D13BuildingLifecycle.UnderConstruction, order.Building.Lifecycle);
            Assert.AreEqual(1, inventory.ConstructionMaterialCalls);
            Assert.AreEqual(D13BuildingMapState.Construction, order.Building.MapState);

            D13BuildingResult completed = null;
            for (int i = 0; i < definition.BuildDaysRequired; i++)
            {
                completed = system.AdvanceConstruction(new D13ConstructionProgressRequest
                {
                    OperationId = "build_workshop_day_" + i, ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                    Workers = 2, WorldDay = 2 + i, WorldHour = 9, SaveRevision = 2 + i,
                });
                Assert.IsTrue(completed.Succeeded, completed.Message);
            }
            Assert.AreEqual(D13BuildingLifecycle.Operational, completed.Building.Lifecycle);
            Assert.AreEqual(D13BuildingMapState.Operational, completed.Building.MapState);
            Assert.IsTrue(system.IsOpenAtHour(definition.BuildingId, 12));

            var service = system.RunService(new D13BuildingServiceRequest
            {
                OperationId = "workshop_dye", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ServiceId = "workshop_dye", Workers = 1, WorldDay = 8, WorldHour = 12, SaveRevision = 8,
            });
            Assert.IsTrue(service.Succeeded, service.Message);
            Assert.AreEqual(9, service.Service.PriceAmount);
            Assert.AreEqual("economy_service_quote", service.Service.EconomySourceEventId);
            Assert.AreEqual("inventory_service", service.Service.InventorySourceEventId);
            Assert.AreEqual(1, inventory.ProducedQuantity);

            var snapshot = system.CaptureSnapshot();
            snapshot.Validate();
            SaveSystem.SaveD13BuildingRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD13BuildingRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(snapshot.Buildings.Count, loaded.Buildings.Count);
            Assert.AreEqual(snapshot.Services.Count, loaded.Services.Count);
            Assert.AreEqual(snapshot.Orders[0].State, loaded.Orders[0].State);

            var restored = NewSystem(new EventLog(), new FixtureInventory(new EventLog(), true), true, true, true, true, loaded);
            var duplicate = restored.RunService(new D13BuildingServiceRequest
            {
                OperationId = "workshop_dye", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ServiceId = "workshop_dye", Workers = 1, WorldDay = 8, WorldHour = 12, SaveRevision = 8,
            });
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(snapshot.Services.Count, restored.GetReadModel().Services.Count);
        }

        [Test]
        public void WarehouseFireOccupationAndMaintenanceChangeMapVisibleState()
        {
            var log = CreateSources();
            var inventory = new FixtureInventory(log, true);
            var system = NewSystem(log, inventory, true, true, true, true);
            var definition = D13BuildingCatalog.Find(D13BuildingCatalog.WarehouseId);
            BuildToOperational(system, definition, 10);
            Assert.IsTrue(system.IsOpenAtHour(definition.BuildingId, 12));

            var stored = system.StoreWarehouseItem(new D13WarehouseStoreRequest
            {
                OperationId = "warehouse_store_fish", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ItemId = D13BuildingCatalog.WarehouseItem, Quantity = 3, WorldDay = 20, WorldHour = 12, SaveRevision = 20,
            });
            Assert.IsTrue(stored.Succeeded, stored.Message);
            Assert.AreEqual("item_fresh_mackerel:3", stored.Service.OutputSummary);
            Assert.AreEqual(3, inventory.WarehouseStoredQuantity);

            var fire = system.ApplyFire(new D13BuildingFireRequest
            {
                OperationId = "warehouse_fire", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                Severity = 2, CauseEventId = "fire_cause", WorldDay = 21, WorldHour = 12, SaveRevision = 21,
            });
            Assert.IsTrue(fire.Succeeded, fire.Message);
            Assert.AreEqual(D13BuildingMapState.Fire, system.FindState(definition.BuildingId).MapState);
            Assert.IsFalse(system.IsOpenAtHour(definition.BuildingId, 12));

            var extinguish = system.ExtinguishFire(new D13BuildingExtinguishFireRequest
            {
                OperationId = "warehouse_extinguish", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                FireId = fire.Fire.FireId, Workers = 1, WorldDay = 22, WorldHour = 12, SaveRevision = 22,
            });
            Assert.IsTrue(extinguish.Succeeded, extinguish.Message);
            Assert.AreEqual(D13BuildingDamageState.Major, extinguish.Building.DamageState);
            Assert.AreEqual(D13BuildingMapState.Damaged, extinguish.Building.MapState);

            var maintenance = system.RunService(new D13BuildingMaintenanceRequest
            {
                OperationId = "warehouse_maintenance", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ServiceId = D13BuildingCatalog.MaintenanceServiceId, Workers = 1, WorldDay = 23, WorldHour = 12, SaveRevision = 23,
            });
            Assert.IsTrue(maintenance.Succeeded, maintenance.Message);
            Assert.AreEqual(D13BuildingDamageState.None, maintenance.Building.DamageState);
            Assert.AreEqual(D13BuildingMapState.Operational, maintenance.Building.MapState);

            var occupied = system.SetOccupation(new D13BuildingOccupationRequest
            {
                OperationId = "warehouse_occupation", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                FactionId = "faction_privateer", WorldDay = 24, WorldHour = 12, SaveRevision = 24,
            });
            Assert.IsTrue(occupied.Succeeded, occupied.Message);
            Assert.AreEqual(D13BuildingOccupationState.Occupied, occupied.Building.OccupationState);
            Assert.AreEqual(D13BuildingMapState.Occupied, occupied.Building.MapState);
            Assert.IsFalse(system.IsOpenAtHour(definition.BuildingId, 12));
            var deniedStore = system.StoreWarehouseItem(new D13WarehouseStoreRequest
            {
                OperationId = "warehouse_store_occupied", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ItemId = D13BuildingCatalog.WarehouseItem, Quantity = 1, WorldDay = 25, WorldHour = 12, SaveRevision = 25,
            });
            Assert.IsFalse(deniedStore.Succeeded);
            Assert.AreEqual("warehouse_closed", deniedStore.Code);

            var snapshot = system.CaptureSnapshot();
            SaveSystem.NewGame(new NewGameParams { CaptainName = "建筑存档测试", ShipTypeId = "small_fishing" });
            SaveSystem.SaveD13BuildingRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD13BuildingRuntime();
            Assert.AreEqual(D13BuildingMapState.Occupied, loaded.Buildings.Find(item => item.BuildingId == definition.BuildingId).MapState);
            Assert.AreEqual(1, loaded.Fires.Count);
            Assert.IsTrue(loaded.Fires[0].Extinguished);
        }

        [Test]
        public void ShipyardPriceAndShipAuthorityAreSeparateFromBuildingState()
        {
            var log = CreateSources();
            var inventory = new FixtureInventory(log, true);
            var shipAuthority = new FixtureShip(true);
            var system = NewSystem(log, inventory, true, true, true, true, null, shipAuthority);
            var definition = D13BuildingCatalog.Find(D13BuildingCatalog.ShipyardId);
            BuildToOperational(system, definition, 30);
            var service = system.RunService(new D13BuildingShipServiceRequest
            {
                OperationId = "ship_repair_01", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ServiceId = "ship_repair", ShipId = "ship_test", Workers = 2, WorldDay = 40, WorldHour = 10, SaveRevision = 40,
            });
            Assert.IsTrue(service.Succeeded, service.Message);
            Assert.AreEqual("ship_test", shipAuthority.LastShipId);
            Assert.AreEqual(12, shipAuthority.HullPoints);
            Assert.AreEqual(9, service.Service.PriceAmount);
            Assert.AreEqual("ShipSystem", service.Service.ShipSourceEventId == null ? "" : "ShipSystem");
            Assert.AreEqual(D13BuildingLifecycle.Operational, system.FindState(definition.BuildingId).Lifecycle);
            Assert.AreEqual(D13BuildingDamageState.None, system.FindState(definition.BuildingId).DamageState);

            var deniedLog = CreateSources();
            var deniedShip = NewSystem(deniedLog, new FixtureInventory(deniedLog, true), true, true, true, true, null, new FixtureShip(false));
            BuildToOperational(deniedShip, definition, 30);
            var denied = deniedShip.RunService(new D13BuildingShipServiceRequest
            {
                OperationId = "ship_repair_denied", ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId,
                ServiceId = "ship_repair", ShipId = "ship_test", Workers = 2, WorldDay = 40, WorldHour = 10, SaveRevision = 41,
            });
            Assert.IsFalse(denied.Succeeded);
            Assert.AreEqual("ship_service_invalid", denied.Code);
        }

        private static void BuildToOperational(D13BuildingSystem system, D13BuildingDefinition definition, int day)
        {
            var order = system.PlaceConstructionOrder(new D13ConstructionOrderRequest { OperationId = "build_" + definition.BuildingId, ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId, Workers = Math.Min(2, definition.WorkerSlots), WorldDay = day, WorldHour = 9, SaveRevision = day });
            Assert.IsTrue(order.Succeeded, order.Message);
            for (int i = 0; i < definition.BuildDaysRequired; i++)
            {
                var progress = system.AdvanceConstruction(new D13ConstructionProgressRequest { OperationId = "progress_" + definition.BuildingId + "_" + i, ActorRef = D13BuildingIds.PlayerRef, BuildingId = definition.BuildingId, IslandId = definition.IslandId, PropertyId = definition.PropertyId, Workers = Math.Min(2, definition.WorkerSlots), WorldDay = day + i + 1, WorldHour = 9, SaveRevision = day + i + 1 });
                Assert.IsTrue(progress.Succeeded, progress.Message);
            }
        }

        private static D13BuildingSystem NewSystem(EventLog log, FixtureInventory inventory, bool property, bool workers, bool economy, bool occupation, D13BuildingRuntimeSnapshot initial = null, ID13BuildingShipAuthority ship = null)
        {
            return new D13BuildingSystem(new FixtureProperty(property), new FixtureWorkforce(workers), inventory, new FixtureEconomy(economy), new FixtureOccupation(occupation), ship ?? new FixtureShip(true), log, initial);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "property_rights", D13BuildingIds.PropertyOwner);
            AddSource(log, "legal_rights", D13BuildingIds.PropertyOwner);
            AddSource(log, "inventory_materials", D13BuildingIds.InventoryOwner);
            AddSource(log, "inventory_service", D13BuildingIds.InventoryOwner);
            AddSource(log, "inventory_warehouse", D13BuildingIds.InventoryOwner);
            AddSource(log, "economy_service_quote", D13BuildingIds.EconomyOwner);
            AddSource(log, "workforce", D13BuildingIds.WorkforceOwner);
            AddSource(log, "legal_occupation", D13BuildingIds.OccupationOwner);
            AddSource(log, "ship_service", D13BuildingIds.ShipOwner);
            AddSource(log, "fire_cause", "EnvironmentSystem");
            return log;
        }

        private static void AddSource(EventLog log, string eventId, string owner)
        {
            log.Append(new WorldEvent(eventId, 0, "island_saint_marys", owner, new EventResult { ResultCode = "fixture_source", Summary = "fixture source", FactIds = new List<string> { "fixture:" + eventId } }, new[] { D13BuildingIds.PlayerRef }, new[] { "fixture" }, new string[0], 0));
        }

        private sealed class FixtureProperty : ID13BuildingPropertyAuthority
        {
            private readonly bool _allowed; public FixtureProperty(bool allowed) { _allowed = allowed; }
            public D13BuildingSiteDecision ValidateSite(D13BuildingDefinition definition, D13BuildingOperationRequest request) { return _allowed ? new D13BuildingSiteDecision { Allowed = true, Code = "site_approved", PropertySourceEventId = "property_rights", LegalSourceEventId = "legal_rights", SourceOwnerSystem = D13BuildingIds.PropertyOwner } : new D13BuildingSiteDecision { Allowed = false, Code = "property_denied" }; }
        }

        private sealed class FixtureWorkforce : ID13BuildingWorkforceAuthority
        {
            private readonly bool _allowed; public FixtureWorkforce(bool allowed) { _allowed = allowed; }
            public D13BuildingWorkerDecision ValidateWorkers(D13BuildingDefinition definition, int workers, string actorRef, int worldDay) { return new D13BuildingWorkerDecision { Allowed = _allowed && workers > 0 && workers <= definition.WorkerSlots, Code = "workforce_approved", Workers = workers, SourceEventId = "workforce", SourceOwnerSystem = D13BuildingIds.WorkforceOwner }; }
        }

        private sealed class FixtureInventory : ID13BuildingInventoryAuthority
        {
            private readonly EventLog _log; private readonly bool _allowed;
            public int ConstructionMaterialCalls { get; private set; }
            public int ProducedQuantity { get; private set; }
            public int WarehouseStoredQuantity { get; private set; }
            public FixtureInventory(EventLog log, bool allowed) { _log = log; _allowed = allowed; }
            public D13BuildingInventoryDecision ConsumeConstructionMaterials(D13BuildingDefinition definition, D13BuildingOperationRequest request) { ConstructionMaterialCalls++; return new D13BuildingInventoryDecision { Allowed = _allowed, Code = "materials_consumed", SourceEventId = "inventory_materials", SourceOwnerSystem = D13BuildingIds.InventoryOwner }; }
            public D13BuildingInventoryDecision ApplyService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request) { int quantity = 0; foreach (var item in service.Outputs) quantity += item.Quantity; ProducedQuantity += quantity; return new D13BuildingInventoryDecision { Allowed = _allowed, Code = "service_inventory_applied", SourceEventId = "inventory_service", SourceOwnerSystem = D13BuildingIds.InventoryOwner, OutputSummary = quantity == 0 ? "none" : service.Outputs[0].ItemId + ":" + quantity }; }
            public D13BuildingInventoryDecision StoreWarehouseItem(D13BuildingDefinition definition, D13WarehouseStoreRequest request) { WarehouseStoredQuantity += request.Quantity; return new D13BuildingInventoryDecision { Allowed = _allowed, Code = "warehouse_stored", SourceEventId = "inventory_warehouse", SourceOwnerSystem = D13BuildingIds.InventoryOwner, WarehouseUsed = request.Quantity, WarehouseCapacity = definition.StorageCapacity }; }
        }

        private sealed class FixtureEconomy : ID13BuildingEconomyAuthority
        {
            private readonly bool _allowed; public FixtureEconomy(bool allowed) { _allowed = allowed; }
            public D13BuildingEconomyDecision QuoteService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request) { return new D13BuildingEconomyDecision { Allowed = _allowed, Code = "service_price_approved", ServiceId = service.ServiceId, Amount = 9, Currency = D13BuildingIds.Currency, LedgerRef = "ledger_building_fixture", SourceEventId = "economy_service_quote", SourceOwnerSystem = D13BuildingIds.EconomyOwner }; }
        }

        private sealed class FixtureOccupation : ID13BuildingOccupationAuthority
        {
            private readonly bool _allowed; public FixtureOccupation(bool allowed) { _allowed = allowed; }
            public D13BuildingOccupationDecision AuthorizeOccupation(D13BuildingDefinition definition, D13BuildingOccupationRequest request) { bool free = string.IsNullOrWhiteSpace(request.FactionId); return new D13BuildingOccupationDecision { Allowed = _allowed, Code = "occupation_approved", State = free ? D13BuildingOccupationState.Free : D13BuildingOccupationState.Occupied, FactionId = free ? null : request.FactionId, SourceEventId = "legal_occupation", SourceOwnerSystem = D13BuildingIds.OccupationOwner }; }
        }

        private sealed class FixtureShip : ID13BuildingShipAuthority
        {
            private readonly bool _allowed; public string LastShipId { get; private set; } public int HullPoints { get; private set; }
            public FixtureShip(bool allowed) { _allowed = allowed; }
            public D13BuildingShipServiceDecision ApproveShipService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingShipServiceRequest request) { LastShipId = request.ShipId; HullPoints = 12; return new D13BuildingShipServiceDecision { Allowed = _allowed, Code = "ship_service_approved", ShipId = request.ShipId, AppliedHullPoints = 12, SourceEventId = "ship_service", SourceOwnerSystem = D13BuildingIds.ShipOwner }; }
        }
    }
}
