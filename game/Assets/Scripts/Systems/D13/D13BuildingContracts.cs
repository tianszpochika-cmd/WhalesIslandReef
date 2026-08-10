// D13-A06 建筑/工坊/仓库/船坞契约。
// BuildingSystem 只写建筑实例、建设订单、工期、损伤、开放/占领投影和服务事实；
// 产权、Inventory/Cargo、Economy、工人和 ShipSystem 通过带来源的 authority 裁决。

using System;
using System.Collections.Generic;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13BuildingIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "BuildingSystem";
        public const string PropertyOwner = "PropertySystem";
        public const string InventoryOwner = "InventorySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string WorkforceOwner = "IslandGovernanceSystem";
        public const string OccupationOwner = "FactionLegalSystem";
        public const string ShipOwner = "ShipSystem";
        public const string PlayerRef = "player";
        public const string Currency = "coin";
        public const string ConstructionRecoveryRoute = "route_building_site_review";
        public const string MaterialRecoveryRoute = "route_building_material_review";
        public const string WorkforceRecoveryRoute = "route_building_workforce_review";
        public const string EconomyRecoveryRoute = "route_building_service_price_review";
        public const string FireRecoveryRoute = "route_building_fire_response";
        public const string OccupationRecoveryRoute = "route_building_occupation_review";
        public const string ShipRecoveryRoute = "route_shipyard_ship_service_review";
    }

    public enum D13BuildingKind { Dock, Warehouse, Workshop, Shipyard, Well }
    public enum D13BuildingLifecycle { Planned, UnderConstruction, Operational, Closed, Destroyed, Abandoned }
    public enum D13BuildingDamageState { None, Minor, Major, FireDamaged, WarDamaged }
    public enum D13BuildingOccupationState { Free, Occupied, Contested }
    public enum D13BuildingMapState { Hidden, Planned, Construction, Operational, Damaged, Fire, Occupied, Closed }
    public enum D13ConstructionOrderStatus { Pending, InProgress, Completed, Cancelled, Failed }

    [Serializable]
    public sealed class D13BuildingMaterialRequirement
    {
        public string ItemId;
        public int Quantity;

        public D13BuildingMaterialRequirement Clone() { return (D13BuildingMaterialRequirement)MemberwiseClone(); }
        public void Validate(string label)
        {
            if (string.IsNullOrWhiteSpace(ItemId) || Quantity <= 0) throw new InvalidOperationException(label + " material is invalid.");
        }
    }

    [Serializable]
    public sealed class D13BuildingOutputDefinition
    {
        public string ItemId;
        public int Quantity;

        public D13BuildingOutputDefinition Clone() { return (D13BuildingOutputDefinition)MemberwiseClone(); }
        public void Validate(string label)
        {
            if (string.IsNullOrWhiteSpace(ItemId) || Quantity <= 0) throw new InvalidOperationException(label + " output is invalid.");
        }
    }

    [Serializable]
    public sealed class D13BuildingServiceDefinition
    {
        public string ServiceId;
        public string DisplayNameKey;
        public bool RequiresShip;
        public bool RequiresWarehouse;
        public List<D13BuildingMaterialRequirement> Inputs = new List<D13BuildingMaterialRequirement>();
        public List<D13BuildingOutputDefinition> Outputs = new List<D13BuildingOutputDefinition>();
        public List<string> Tags = new List<string>();

        public D13BuildingServiceDefinition Clone()
        {
            var result = new D13BuildingServiceDefinition { ServiceId = ServiceId, DisplayNameKey = DisplayNameKey, RequiresShip = RequiresShip, RequiresWarehouse = RequiresWarehouse };
            if (Inputs != null) foreach (var item in Inputs) if (item != null) result.Inputs.Add(item.Clone());
            if (Outputs != null) foreach (var item in Outputs) if (item != null) result.Outputs.Add(item.Clone());
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServiceId) || string.IsNullOrWhiteSpace(DisplayNameKey) || Inputs == null || Outputs == null || Tags == null || Tags.Count == 0)
                throw new InvalidOperationException("Building service definition is invalid: " + ServiceId);
            foreach (var item in Inputs) item.Validate(ServiceId);
            foreach (var item in Outputs) item.Validate(ServiceId);
        }
    }

    [Serializable]
    public sealed class D13BuildingDefinition
    {
        public int SchemaVersion = D13BuildingIds.SchemaVersion;
        public string BuildingId;
        public string PropertyId;
        public string IslandId;
        public D13BuildingKind Kind;
        public string DisplayNameKey;
        public int BuildDaysRequired = 1;
        public int WorkerSlots = 1;
        public int StorageCapacity;
        public int OpenFromHour;
        public int OpenToHour = 24;
        public List<D13BuildingServiceDefinition> Services = new List<D13BuildingServiceDefinition>();
        public List<D13BuildingMaterialRequirement> ConstructionMaterials = new List<D13BuildingMaterialRequirement>();
        public List<string> Tags = new List<string>();
        public string HistoricalTag = "inferred";
        public string SourceNote;
        public string TestId;

        public D13BuildingDefinition Clone()
        {
            var result = new D13BuildingDefinition
            {
                SchemaVersion = SchemaVersion, BuildingId = BuildingId, PropertyId = PropertyId, IslandId = IslandId, Kind = Kind,
                DisplayNameKey = DisplayNameKey, BuildDaysRequired = BuildDaysRequired, WorkerSlots = WorkerSlots, StorageCapacity = StorageCapacity,
                OpenFromHour = OpenFromHour, OpenToHour = OpenToHour, HistoricalTag = HistoricalTag, SourceNote = SourceNote, TestId = TestId,
            };
            if (Services != null) foreach (var item in Services) if (item != null) result.Services.Add(item.Clone());
            if (ConstructionMaterials != null) foreach (var item in ConstructionMaterials) if (item != null) result.ConstructionMaterials.Add(item.Clone());
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public D13BuildingServiceDefinition FindService(string serviceId)
        {
            if (Services == null) return null;
            foreach (var item in Services) if (item != null && item.ServiceId == serviceId) return item;
            return null;
        }

        public void Validate()
        {
            if (SchemaVersion != D13BuildingIds.SchemaVersion || string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(PropertyId) || string.IsNullOrWhiteSpace(IslandId) || string.IsNullOrWhiteSpace(DisplayNameKey) || BuildDaysRequired <= 0 || WorkerSlots <= 0 || StorageCapacity < 0 || OpenFromHour < 0 || OpenFromHour > 23 || OpenToHour <= OpenFromHour || OpenToHour > 24 || Services == null || ConstructionMaterials == null || Tags == null || Tags.Count == 0 || string.IsNullOrWhiteSpace(SourceNote) || string.IsNullOrWhiteSpace(TestId))
                throw new InvalidOperationException("Building definition is invalid: " + BuildingId);
            if (!Enum.IsDefined(typeof(D13BuildingKind), Kind)) throw new InvalidOperationException("Building kind is invalid: " + BuildingId);
            if (HistoricalTag != "historical" && HistoricalTag != "inferred" && HistoricalTag != "fictional") throw new InvalidOperationException("Building historical tag is invalid: " + BuildingId);
            var serviceIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var service in Services) { service.Validate(); if (!serviceIds.Add(service.ServiceId)) throw new InvalidOperationException("Duplicate building service: " + service.ServiceId); }
            foreach (var item in ConstructionMaterials) item.Validate(BuildingId + ".construction");
        }
    }

    [Serializable]
    public sealed class D13BuildingState
    {
        public string BuildingId;
        public string PropertyId;
        public string IslandId;
        public D13BuildingKind Kind;
        public D13BuildingLifecycle Lifecycle = D13BuildingLifecycle.Planned;
        public D13BuildingDamageState DamageState = D13BuildingDamageState.None;
        public int DamagePoints;
        public int BuildDaysRequired;
        public int BuildDaysElapsed;
        public int WorkerCount;
        public D13BuildingOccupationState OccupationState = D13BuildingOccupationState.Free;
        public string OccupyingFactionId;
        public string OccupationSourceEventId;
        public D13BuildingMapState MapState = D13BuildingMapState.Hidden;
        public int LastUpdatedDay;
        public long Revision;
        public string LastEventId;

        public D13BuildingState Clone() { return (D13BuildingState)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(PropertyId) || string.IsNullOrWhiteSpace(IslandId) || BuildDaysRequired <= 0 || BuildDaysElapsed < 0 || BuildDaysElapsed > BuildDaysRequired || WorkerCount < 0 || DamagePoints < 0 || DamagePoints > 100 || LastUpdatedDay < 0 || Revision < 0) throw new InvalidOperationException("Building state is invalid: " + BuildingId);
            if (!Enum.IsDefined(typeof(D13BuildingKind), Kind) || !Enum.IsDefined(typeof(D13BuildingLifecycle), Lifecycle) || !Enum.IsDefined(typeof(D13BuildingDamageState), DamageState) || !Enum.IsDefined(typeof(D13BuildingOccupationState), OccupationState) || !Enum.IsDefined(typeof(D13BuildingMapState), MapState)) throw new InvalidOperationException("Building state enum is invalid: " + BuildingId);
            if (OccupationState != D13BuildingOccupationState.Free && string.IsNullOrWhiteSpace(OccupyingFactionId)) throw new InvalidOperationException("Occupied building needs a faction: " + BuildingId);
        }
    }

    [Serializable]
    public sealed class D13ConstructionOrderState
    {
        public string OrderId;
        public string BuildingId;
        public string PropertyId;
        public D13ConstructionOrderStatus State = D13ConstructionOrderStatus.InProgress;
        public int StartDay;
        public int DueDay;
        public int WorkerCount;
        public int DaysElapsed;
        public List<D13BuildingMaterialRequirement> Materials = new List<D13BuildingMaterialRequirement>();
        public string PropertySourceEventId;
        public string InventorySourceEventId;
        public string WorkforceSourceEventId;
        public int LastUpdatedDay;
        public long Revision;
        public string LastEventId;

        public D13ConstructionOrderState Clone()
        {
            var result = new D13ConstructionOrderState { OrderId = OrderId, BuildingId = BuildingId, PropertyId = PropertyId, State = State, StartDay = StartDay, DueDay = DueDay, WorkerCount = WorkerCount, DaysElapsed = DaysElapsed, PropertySourceEventId = PropertySourceEventId, InventorySourceEventId = InventorySourceEventId, WorkforceSourceEventId = WorkforceSourceEventId, LastUpdatedDay = LastUpdatedDay, Revision = Revision, LastEventId = LastEventId };
            if (Materials != null) foreach (var item in Materials) if (item != null) result.Materials.Add(item.Clone());
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(OrderId) || string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(PropertyId) || StartDay < 0 || DueDay < StartDay || WorkerCount <= 0 || DaysElapsed < 0 || DaysElapsed > DueDay - StartDay || string.IsNullOrWhiteSpace(PropertySourceEventId) || string.IsNullOrWhiteSpace(InventorySourceEventId) || string.IsNullOrWhiteSpace(WorkforceSourceEventId) || LastUpdatedDay < 0 || Revision < 0 || Materials == null) throw new InvalidOperationException("Construction order is invalid: " + OrderId);
            if (!Enum.IsDefined(typeof(D13ConstructionOrderStatus), State)) throw new InvalidOperationException("Construction order state is invalid: " + OrderId);
            foreach (var item in Materials) item.Validate(OrderId);
        }
    }

    [Serializable]
    public sealed class D13BuildingServiceRecord
    {
        public string ServiceRecordId;
        public string BuildingId;
        public string ServiceId;
        public string ActorRef;
        public int PriceAmount;
        public string Currency = D13BuildingIds.Currency;
        public string LedgerRef;
        public string EconomySourceEventId;
        public string InventorySourceEventId;
        public string ShipSourceEventId;
        public string OutputSummary;
        public int WorldDay;
        public long SaveRevision;

        public D13BuildingServiceRecord Clone() { return (D13BuildingServiceRecord)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServiceRecordId) || string.IsNullOrWhiteSpace(BuildingId) || string.IsNullOrWhiteSpace(ServiceId) || string.IsNullOrWhiteSpace(ActorRef) || PriceAmount < 0 || string.IsNullOrWhiteSpace(Currency) || string.IsNullOrWhiteSpace(LedgerRef) || string.IsNullOrWhiteSpace(EconomySourceEventId) || string.IsNullOrWhiteSpace(InventorySourceEventId) || WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Building service record is invalid: " + ServiceRecordId);
        }
    }

    [Serializable]
    public sealed class D13BuildingFireIncident
    {
        public string FireId;
        public string BuildingId;
        public int Severity;
        public int DamagePoints;
        public bool Extinguished;
        public string CauseEventId;
        public string RecoveryRouteId = D13BuildingIds.FireRecoveryRoute;
        public int WorldDay;
        public long Revision;

        public D13BuildingFireIncident Clone() { return (D13BuildingFireIncident)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FireId) || string.IsNullOrWhiteSpace(BuildingId) || Severity < 1 || Severity > 3 || DamagePoints <= 0 || string.IsNullOrWhiteSpace(CauseEventId) || string.IsNullOrWhiteSpace(RecoveryRouteId) || WorldDay < 0 || Revision < 0 || (Extinguished && string.IsNullOrWhiteSpace(RecoveryRouteId))) throw new InvalidOperationException("Building fire incident is invalid: " + FireId);
        }
    }

    [Serializable]
    public sealed class D13BuildingRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13BuildingIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13BuildingState> Buildings = new List<D13BuildingState>();
        public List<D13ConstructionOrderState> Orders = new List<D13ConstructionOrderState>();
        public List<D13BuildingServiceRecord> Services = new List<D13BuildingServiceRecord>();
        public List<D13BuildingFireIncident> Fires = new List<D13BuildingFireIncident>();
        public List<string> AppliedEventKeys = new List<string>();
        public int LastWorldDay;
        public string LastEventId;

        public D13BuildingRuntimeSnapshot Clone()
        {
            var result = new D13BuildingRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision, LastWorldDay = LastWorldDay, LastEventId = LastEventId };
            if (Buildings != null) foreach (var item in Buildings) if (item != null) result.Buildings.Add(item.Clone());
            if (Orders != null) foreach (var item in Orders) if (item != null) result.Orders.Add(item.Clone());
            if (Services != null) foreach (var item in Services) if (item != null) result.Services.Add(item.Clone());
            if (Fires != null) foreach (var item in Fires) if (item != null) result.Fires.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Buildings == null || Orders == null || Services == null || Fires == null || AppliedEventKeys == null || LastWorldDay < 0) throw new InvalidOperationException("D13 building snapshot is invalid.");
            if (!Unique(Buildings, item => item.BuildingId) || !Unique(Orders, item => item.OrderId) || !Unique(Services, item => item.ServiceRecordId) || !Unique(Fires, item => item.FireId)) throw new InvalidOperationException("D13 building snapshot contains duplicate IDs.");
            foreach (var item in Buildings) item.Validate(); foreach (var item in Orders) item.Validate(); foreach (var item in Services) item.Validate(); foreach (var item in Fires) item.Validate();
        }

        private static bool Unique<T>(IEnumerable<T> values, Func<T, string> key)
        {
            var set = new HashSet<string>(StringComparer.Ordinal); foreach (var item in values) if (item == null || string.IsNullOrWhiteSpace(key(item)) || !set.Add(key(item))) return false; return true;
        }
    }

    public sealed class D13BuildingSiteDecision { public bool Allowed; public string Code; public string Message; public string PropertySourceEventId; public string LegalSourceEventId; public string SourceOwnerSystem = D13BuildingIds.PropertyOwner; }
    public sealed class D13BuildingWorkerDecision { public bool Allowed; public string Code; public string Message; public int Workers; public string SourceEventId; public string SourceOwnerSystem = D13BuildingIds.WorkforceOwner; }
    public sealed class D13BuildingInventoryDecision { public bool Allowed; public string Code; public string Message; public string SourceEventId; public string SourceOwnerSystem = D13BuildingIds.InventoryOwner; public int WarehouseUsed; public int WarehouseCapacity; public string OutputSummary; }
    public sealed class D13BuildingEconomyDecision { public bool Allowed; public string Code; public string Message; public string ServiceId; public int Amount; public string Currency = D13BuildingIds.Currency; public string LedgerRef; public string SourceEventId; public string SourceOwnerSystem = D13BuildingIds.EconomyOwner; }
    public sealed class D13BuildingOccupationDecision { public bool Allowed; public string Code; public string Message; public D13BuildingOccupationState State; public string FactionId; public string SourceEventId; public string SourceOwnerSystem = D13BuildingIds.OccupationOwner; }
    public sealed class D13BuildingShipServiceDecision { public bool Allowed; public string Code; public string Message; public string ShipId; public int AppliedHullPoints; public string SourceEventId; public string SourceOwnerSystem = D13BuildingIds.ShipOwner; }

    public interface ID13BuildingPropertyAuthority { D13BuildingSiteDecision ValidateSite(D13BuildingDefinition definition, D13BuildingOperationRequest request); }
    public interface ID13BuildingWorkforceAuthority { D13BuildingWorkerDecision ValidateWorkers(D13BuildingDefinition definition, int workers, string actorRef, int worldDay); }
    public interface ID13BuildingInventoryAuthority
    {
        D13BuildingInventoryDecision ConsumeConstructionMaterials(D13BuildingDefinition definition, D13BuildingOperationRequest request);
        D13BuildingInventoryDecision ApplyService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request);
        D13BuildingInventoryDecision StoreWarehouseItem(D13BuildingDefinition definition, D13WarehouseStoreRequest request);
    }
    public interface ID13BuildingEconomyAuthority
    {
        D13BuildingEconomyDecision QuoteService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request);
    }
    public interface ID13BuildingOccupationAuthority { D13BuildingOccupationDecision AuthorizeOccupation(D13BuildingDefinition definition, D13BuildingOccupationRequest request); }
    public interface ID13BuildingShipAuthority { D13BuildingShipServiceDecision ApproveShipService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingShipServiceRequest request); }

    [Serializable]
    public class D13BuildingOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13BuildingIds.PlayerRef;
        public string BuildingId;
        public string IslandId;
        public string PropertyId;
        public int WorldDay;
        public int WorldHour = 12;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D13ConstructionOrderRequest : D13BuildingOperationRequest { public int Workers; }
    [Serializable]
    public sealed class D13ConstructionProgressRequest : D13BuildingOperationRequest { public int Workers; }
    [Serializable]
    public class D13BuildingServiceRequest : D13BuildingOperationRequest { public string ServiceId; public string ShipId; public int Workers; }
    [Serializable]
    public sealed class D13BuildingShipServiceRequest : D13BuildingServiceRequest { }
    [Serializable]
    public sealed class D13WarehouseStoreRequest : D13BuildingOperationRequest { public string ItemId; public int Quantity; }
    [Serializable]
    public sealed class D13BuildingMaintenanceRequest : D13BuildingServiceRequest { }
    [Serializable]
    public sealed class D13BuildingFireRequest : D13BuildingOperationRequest { public int Severity = 1; public string CauseEventId; }
    [Serializable]
    public sealed class D13BuildingExtinguishFireRequest : D13BuildingOperationRequest { public string FireId; public int Workers; }
    [Serializable]
    public sealed class D13BuildingOccupationRequest : D13BuildingOperationRequest { public string FactionId; }

    public sealed class D13BuildingResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string RecoveryRouteId;
        public D13BuildingState Building;
        public D13ConstructionOrderState Order;
        public D13BuildingServiceRecord Service;
        public D13BuildingFireIncident Fire;
        public FactEvent Fact;
    }

    public sealed class D13BuildingReadModel
    {
        public List<D13BuildingDefinition> Definitions = new List<D13BuildingDefinition>();
        public List<D13BuildingState> Buildings = new List<D13BuildingState>();
        public List<D13ConstructionOrderState> Orders = new List<D13ConstructionOrderState>();
        public List<D13BuildingServiceRecord> Services = new List<D13BuildingServiceRecord>();
        public List<D13BuildingFireIncident> Fires = new List<D13BuildingFireIncident>();
        public long Revision;
    }
}
