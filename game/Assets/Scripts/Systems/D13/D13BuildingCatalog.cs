// D13-A06 建筑代表目录；定义是只读内容，实例状态由 BuildingSystem 写入。

using System.Collections.Generic;

namespace Whalebone.Systems.D13
{
    public static class D13BuildingCatalog
    {
        public const string DockId = "building_saint_marys_dock";
        public const string WarehouseId = "building_santa_marys_north_warehouse";
        public const string WorkshopId = "building_driftwood_ropewalk_workshop";
        public const string ShipyardId = "building_santa_marys_shipyard";
        public const string WellId = "building_green_cay_water_well";
        public const string WellServiceId = "well_water_access";
        public const string ConstructionFiber = "item_fiber_agave";
        public const string ConstructionResin = "item_resin_pine";
        public const string WarehouseItem = "item_fresh_mackerel";
        public const string WorkshopOutput = "item_dye_indigo";
        public const string MaintenanceServiceId = "building_maintenance";

        private static readonly IReadOnlyList<D13BuildingDefinition> Buildings = Create();
        public static IReadOnlyList<D13BuildingDefinition> All { get { return Buildings; } }

        public static D13BuildingDefinition Find(string buildingId)
        {
            foreach (var item in Buildings) if (item != null && item.BuildingId == buildingId) return item.Clone();
            return null;
        }

        private static IReadOnlyList<D13BuildingDefinition> Create()
        {
            return new List<D13BuildingDefinition>
            {
                Definition(DockId, "property_shipyard_santa_marys_dock", "island_saint_marys", D13BuildingKind.Dock, "content.building.building_saint_marys_dock.name", 2, 3, 0, 6, 22, new[] { Service("dock_mooring", "content.building.service.dock_mooring", true, false, new D13BuildingMaterialRequirement[0], new D13BuildingOutputDefinition[0], "dock", "berth"), Maintenance(), }, new[] { Material(ConstructionFiber, 2), Material(ConstructionResin, 1) }, "D13-A06-BUILDING-001"),
                Definition(WarehouseId, "property_warehouse_santa_marys_north_store", "island_saint_marys", D13BuildingKind.Warehouse, "content.building.building_santa_marys_north_warehouse.name", 3, 2, 24, 0, 24, new[] { Service("warehouse_storage", "content.building.service.warehouse_storage", false, true, new D13BuildingMaterialRequirement[0], new D13BuildingOutputDefinition[0], "warehouse", "storage"), Maintenance(), }, new[] { Material(ConstructionFiber, 4), Material(ConstructionResin, 2) }, "D13-A06-BUILDING-002"),
                Definition(WorkshopId, "property_workshop_driftwood_ropewalk", "island_driftwood_isle", D13BuildingKind.Workshop, "content.building.building_driftwood_ropewalk_workshop.name", 4, 3, 0, 8, 20, new[] { Service("workshop_dye", "content.building.service.workshop_dye", false, false, new[] { Material(ConstructionFiber, 1), Material(ConstructionResin, 1) }, new[] { Output(WorkshopOutput, 1) }, "workshop", "production"), Maintenance(), }, new[] { Material(ConstructionFiber, 5), Material(ConstructionResin, 2) }, "D13-A06-BUILDING-003"),
                Definition(ShipyardId, "property_shipyard_santa_marys_dock", "island_saint_marys", D13BuildingKind.Shipyard, "content.building.building_santa_marys_shipyard.name", 5, 4, 0, 6, 22, new[] { Service("ship_repair", "content.building.service.ship_repair", true, false, new[] { Material(ConstructionResin, 1) }, new D13BuildingOutputDefinition[0], "shipyard", "repair"), Maintenance(), }, new[] { Material(ConstructionFiber, 6), Material(ConstructionResin, 4) }, "D13-A06-BUILDING-004"),
                Definition(WellId, "property_farmland_green_cay_cassava", "island_green_cay", D13BuildingKind.Well, "content.building.building_green_cay_water_well.name", 2, 1, 0, 0, 24, new[] { Service(WellServiceId, "content.building.service.well_water_access", false, false, new D13BuildingMaterialRequirement[0], new D13BuildingOutputDefinition[0], "well", "water", "ecology_bridge"), Maintenance(), }, new[] { Material(ConstructionFiber, 2), Material(ConstructionResin, 1) }, "D13-S01-BUILDING-001"),
            };
        }

        private static D13BuildingDefinition Definition(string id, string propertyId, string islandId, D13BuildingKind kind, string display, int days, int workers, int storage, int open, int close, IEnumerable<D13BuildingServiceDefinition> services, IEnumerable<D13BuildingMaterialRequirement> materials, string testId)
        {
            var result = new D13BuildingDefinition { BuildingId = id, PropertyId = propertyId, IslandId = islandId, Kind = kind, DisplayNameKey = display, BuildDaysRequired = days, WorkerSlots = workers, StorageCapacity = storage, OpenFromHour = open, OpenToHour = close, SourceNote = "D13-A06 项目自有建筑代表定义；实例状态、损伤和服务事实由 BuildingSystem 写入。", TestId = testId, Tags = new List<string> { kind.ToString().ToLowerInvariant(), "construction", "historical_context" } };
            result.Services.AddRange(services); result.ConstructionMaterials.AddRange(materials); result.Validate(); return result;
        }

        private static D13BuildingServiceDefinition Service(string id, string display, bool requiresShip, bool requiresWarehouse, IEnumerable<D13BuildingMaterialRequirement> inputs, IEnumerable<D13BuildingOutputDefinition> outputs, params string[] tags)
        {
            return new D13BuildingServiceDefinition { ServiceId = id, DisplayNameKey = display, RequiresShip = requiresShip, RequiresWarehouse = requiresWarehouse, Inputs = new List<D13BuildingMaterialRequirement>(inputs), Outputs = new List<D13BuildingOutputDefinition>(outputs), Tags = new List<string>(tags) };
        }

        private static D13BuildingMaterialRequirement Material(string itemId, int quantity) { return new D13BuildingMaterialRequirement { ItemId = itemId, Quantity = quantity }; }
        private static D13BuildingOutputDefinition Output(string itemId, int quantity) { return new D13BuildingOutputDefinition { ItemId = itemId, Quantity = quantity }; }
        private static D13BuildingServiceDefinition Maintenance() { return Service(MaintenanceServiceId, "content.building.service.maintenance", false, false, new[] { Material(ConstructionResin, 1) }, new D13BuildingOutputDefinition[0], "maintenance", "repair"); }
    }
}
