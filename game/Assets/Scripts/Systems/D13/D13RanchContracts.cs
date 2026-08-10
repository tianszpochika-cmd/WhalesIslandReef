// D13-A04 牧场与基础养殖契约。
// RanchSystem 只写家禽/牲畜实例健康、疾病、周期和待产物；
// Inventory/Cargo、Ecology、Property、Economy 继续由各自 owner 裁决。

using System;
using System.Collections.Generic;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13RanchIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "RanchSystem";
        public const string EcologyOwner = "EcologySystem";
        public const string InventoryOwner = "D3InventorySystem";
        public const string PropertyOwner = "PropertySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string SourceOwner = "AnimalCareSystem";
        public const string PlayerRef = "player";
        public const string Currency = "coin";
        public const string LandRecoveryRoute = "route_ranch_land_review";
        public const string SourceRecoveryRoute = "route_ranch_source_review";
        public const string QuarantineRecoveryRoute = "route_ranch_quarantine_review";
        public const string FeedRecoveryRoute = "route_ranch_feed_review";
        public const string WaterRecoveryRoute = "route_ranch_water_review";
        public const string DiseaseRecoveryRoute = "route_ranch_disease_treatment";
        public const string InventoryRecoveryRoute = "route_ranch_inventory_capacity";
        public const string EconomyRecoveryRoute = "route_ranch_market_review";
        public const string BreedingRecoveryRoute = "route_ranch_breeding_deferred";
    }

    public enum D13RanchAnimalKind
    {
        Poultry,
        Livestock,
    }

    public enum D13RanchProductKind
    {
        Egg,
        Milk,
        Meat,
        Leather,
        Manure,
    }

    public enum D13RanchAnimalLifecycle
    {
        Quarantined,
        Healthy,
        Sick,
        ProductReady,
        Dead,
        Sold,
    }

    public enum D13RanchDisease
    {
        None,
        Respiratory,
        Parasite,
        Infection,
    }

    public enum D13RanchBreedingMode
    {
        Disabled,
        SingleOffspringProposal,
    }

    [Serializable]
    public sealed class D13RanchAnimalDefinition
    {
        public string DefinitionId;
        public string SourceContentId;
        public string DisplayName;
        public D13RanchAnimalKind Kind;
        public string FeedItemId;
        public int FeedPerCycle = 1;
        public int WaterPerCycle = 1;
        public int SpaceRequired = 1;
        public int LaborRequired = 1;
        public string ProductItemId;
        public D13RanchProductKind ProductKind;
        public int ProductPerCycle = 1;
        public int DiseaseRiskPercent = 10;
        public int QuarantineDays = 1;
        public string MarketRef;
        public D13RanchBreedingMode BreedingMode = D13RanchBreedingMode.Disabled;
        public List<string> Tags = new List<string>();

        public D13RanchAnimalDefinition Clone()
        {
            var result = new D13RanchAnimalDefinition
            {
                DefinitionId = DefinitionId,
                SourceContentId = SourceContentId,
                DisplayName = DisplayName,
                Kind = Kind,
                FeedItemId = FeedItemId,
                FeedPerCycle = FeedPerCycle,
                WaterPerCycle = WaterPerCycle,
                SpaceRequired = SpaceRequired,
                LaborRequired = LaborRequired,
                ProductItemId = ProductItemId,
                ProductKind = ProductKind,
                ProductPerCycle = ProductPerCycle,
                DiseaseRiskPercent = DiseaseRiskPercent,
                QuarantineDays = QuarantineDays,
                MarketRef = MarketRef,
                BreedingMode = BreedingMode,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            Require(DefinitionId, "definition_id");
            Require(SourceContentId, "source_content_id");
            Require(FeedItemId, "feed_item_id");
            Require(ProductItemId, "product_item_id");
            Require(MarketRef, "market_ref");
            if (!Enum.IsDefined(typeof(D13RanchAnimalKind), Kind) || !Enum.IsDefined(typeof(D13RanchProductKind), ProductKind) || !Enum.IsDefined(typeof(D13RanchBreedingMode), BreedingMode))
                throw new InvalidOperationException("Invalid ranch definition enum: " + DefinitionId);
            if (FeedPerCycle <= 0 || WaterPerCycle <= 0 || SpaceRequired <= 0 || LaborRequired < 0 || ProductPerCycle <= 0 || DiseaseRiskPercent < 0 || DiseaseRiskPercent > 100 || QuarantineDays < 1)
                throw new InvalidOperationException("Invalid ranch definition values: " + DefinitionId);
            if (Tags == null || Tags.Count == 0) throw new InvalidOperationException("Ranch animal tags are required: " + DefinitionId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Ranch " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13RanchFacilityDefinition
    {
        public string RanchId;
        public string PropertyId;
        public string DisplayName;
        public string WaterSourceId;
        public int SpaceCapacity = 4;
        public int WaterCapacity = 100;
        public List<string> Tags = new List<string>();

        public D13RanchFacilityDefinition Clone()
        {
            var result = new D13RanchFacilityDefinition
            {
                RanchId = RanchId,
                PropertyId = PropertyId,
                DisplayName = DisplayName,
                WaterSourceId = WaterSourceId,
                SpaceCapacity = SpaceCapacity,
                WaterCapacity = WaterCapacity,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            Require(RanchId, "ranch_id");
            Require(PropertyId, "property_id");
            Require(WaterSourceId, "water_source_id");
            if (SpaceCapacity <= 0 || WaterCapacity <= 0) throw new InvalidOperationException("Invalid ranch facility capacity: " + RanchId);
            if (Tags == null) throw new InvalidOperationException("Ranch facility tags are required: " + RanchId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Ranch facility " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13RanchAnimalState
    {
        public string AnimalId;
        public string RanchId;
        public string DefinitionId;
        public string SourceContentId;
        public string SourceRef;
        public string SourceEventId;
        public string InspectionEventId;
        public D13RanchAnimalKind Kind;
        public D13RanchAnimalLifecycle Lifecycle = D13RanchAnimalLifecycle.Quarantined;
        public D13RanchDisease Disease = D13RanchDisease.None;
        public int Health = 100;
        public int QuarantineUntilDay;
        public int CycleStartDay;
        public int LastUpdatedDay;
        public int FeedUnits;
        public int WaterUnits;
        public int LaborUnits;
        public int CycleIndex;
        public int ProductReadyQuantity;
        public string ProductItemId;
        public int DeathDay = -1;
        public long Revision;
        public string LastEventId;
        public string LastDiseaseEventId;

        public D13RanchAnimalState Clone()
        {
            return new D13RanchAnimalState
            {
                AnimalId = AnimalId,
                RanchId = RanchId,
                DefinitionId = DefinitionId,
                SourceContentId = SourceContentId,
                SourceRef = SourceRef,
                SourceEventId = SourceEventId,
                InspectionEventId = InspectionEventId,
                Kind = Kind,
                Lifecycle = Lifecycle,
                Disease = Disease,
                Health = Health,
                QuarantineUntilDay = QuarantineUntilDay,
                CycleStartDay = CycleStartDay,
                LastUpdatedDay = LastUpdatedDay,
                FeedUnits = FeedUnits,
                WaterUnits = WaterUnits,
                LaborUnits = LaborUnits,
                CycleIndex = CycleIndex,
                ProductReadyQuantity = ProductReadyQuantity,
                ProductItemId = ProductItemId,
                DeathDay = DeathDay,
                Revision = Revision,
                LastEventId = LastEventId,
                LastDiseaseEventId = LastDiseaseEventId,
            };
        }

        public void Validate()
        {
            Require(AnimalId, "animal_id");
            Require(RanchId, "ranch_id");
            Require(DefinitionId, "definition_id");
            Require(SourceContentId, "source_content_id");
            Require(SourceRef, "source_ref");
            Require(SourceEventId, "source_event_id");
            Require(InspectionEventId, "inspection_event_id");
            if (!Enum.IsDefined(typeof(D13RanchAnimalKind), Kind) || !Enum.IsDefined(typeof(D13RanchAnimalLifecycle), Lifecycle) || !Enum.IsDefined(typeof(D13RanchDisease), Disease))
                throw new InvalidOperationException("Invalid ranch animal state enum: " + AnimalId);
            if (Health < 0 || Health > 100 || QuarantineUntilDay < 0 || CycleStartDay < 0 || LastUpdatedDay < 0 || FeedUnits < 0 || WaterUnits < 0 || LaborUnits < 0 || CycleIndex < 0 || ProductReadyQuantity < 0 || DeathDay < -1 || Revision < 0)
                throw new InvalidOperationException("Invalid ranch animal state values: " + AnimalId);
            if (Lifecycle == D13RanchAnimalLifecycle.ProductReady)
            {
                if (ProductReadyQuantity <= 0 || string.IsNullOrWhiteSpace(ProductItemId)) throw new InvalidOperationException("Product-ready ranch animal needs a pending product: " + AnimalId);
            }
            else if (ProductReadyQuantity != 0 || !string.IsNullOrWhiteSpace(ProductItemId))
                throw new InvalidOperationException("Non-product-ready ranch animal cannot hold a pending product: " + AnimalId);
            if (Lifecycle == D13RanchAnimalLifecycle.Dead && (DeathDay < 0 || Health != 0)) throw new InvalidOperationException("Dead ranch animal needs death day and zero health: " + AnimalId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Ranch animal " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13RanchWaterState
    {
        public string RanchId;
        public string SourceId;
        public int Available = 100;
        public int Capacity = 100;
        public int LastUpdatedDay;
        public long Revision;
        public string LastEventId;

        public D13RanchWaterState Clone()
        {
            return new D13RanchWaterState
            {
                RanchId = RanchId,
                SourceId = SourceId,
                Available = Available,
                Capacity = Capacity,
                LastUpdatedDay = LastUpdatedDay,
                Revision = Revision,
                LastEventId = LastEventId,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RanchId) || string.IsNullOrWhiteSpace(SourceId) || Available < 0 || Capacity <= 0 || Available > Capacity || LastUpdatedDay < 0 || Revision < 0)
                throw new InvalidOperationException("Invalid ranch water state: " + RanchId);
        }
    }

    [Serializable]
    public sealed class D13RanchWaterRuntimeSnapshot
    {
        public int SchemaVersion = D13RanchIds.SchemaVersion;
        public long Revision;
        public List<D13RanchWaterState> Sources = new List<D13RanchWaterState>();
        public List<string> AppliedEventKeys = new List<string>();

        public D13RanchWaterRuntimeSnapshot Clone()
        {
            var result = new D13RanchWaterRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Sources != null) foreach (var item in Sources) if (item != null) result.Sources.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != D13RanchIds.SchemaVersion || Revision < 0 || Sources == null || AppliedEventKeys == null) throw new InvalidOperationException("D13 ranch water snapshot is invalid.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Sources)
            {
                if (item == null) throw new InvalidOperationException("D13 ranch water snapshot contains null.");
                item.Validate();
                if (!ids.Add(item.RanchId)) throw new InvalidOperationException("D13 ranch water snapshot duplicates ranch: " + item.RanchId);
            }
        }
    }

    [Serializable]
    public sealed class D13RanchSourceDecision
    {
        public bool Allowed;
        public bool InspectionPassed;
        public string Code;
        public string Message;
        public string SourceContentId;
        public string SourceEventId;
        public string InspectionEventId;
        public string SourceOwnerSystem;
        public int QuarantineDays = 1;
    }

    [Serializable]
    public sealed class D13RanchLandDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string PropertySourceEventId;
        public string LegalSourceEventId;
    }

    [Serializable]
    public sealed class D13RanchEconomyDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public long UnitPrice;
        public long TotalPrice;
        public string Currency = D13RanchIds.Currency;
        public string MarketRef;
        public string LedgerRef;
        public string SourceEventId;
        public string SourceOwnerSystem = D13RanchIds.EconomyOwner;
    }

    [Serializable]
    public sealed class D13RanchWaterRequest
    {
        public string RanchId;
        public string SourceId;
        public int Amount;
        public int WorldDay;
        public long SaveRevision;
        public string EventId;
    }

    [Serializable]
    public sealed class D13RanchWaterResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public bool Applied;
        public string Code;
        public string Message;
        public string EventId;
        public int Quantity;
        public D13RanchWaterState State;
        public FactEvent Fact;
    }

    public interface ID13RanchWaterAuthority
    {
        D13RanchWaterState EnsureRanch(D13RanchFacilityDefinition definition, int worldDay);
        D13RanchWaterState FindRanch(string ranchId);
        D13RanchWaterResult ConsumeWater(D13RanchWaterRequest request, EventLog eventLog);
        D13RanchWaterRuntimeSnapshot CaptureRanchSnapshot();
        void RestoreRanchSnapshot(D13RanchWaterRuntimeSnapshot snapshot);
    }

    public interface ID13RanchSourceAuthority
    {
        D13RanchSourceDecision ValidateSource(D13RanchAnimalDefinition definition, string sourceRef, string sourceEventId, string actorRef, int worldDay);
    }

    public interface ID13RanchLandAuthority
    {
        D13RanchLandDecision ValidateFacility(D13RanchFacilityDefinition definition, string actorRef, int worldDay);
    }

    public interface ID13RanchEconomyAuthority
    {
        D13RanchEconomyDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision);
    }

    [Serializable]
    public abstract class D13RanchOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13RanchIds.PlayerRef;
        public string RanchId;
        public string AnimalId;
        public int WorldDay;
        public long SaveRevision;
        public string LocationRef = "island_saint_marys";
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D13RanchAcquireRequest : D13RanchOperationRequest
    {
        public string DefinitionId;
        public string SourceRef;
        public string SourceEventId;
        public string InspectionEventId;
    }

    [Serializable]
    public sealed class D13RanchQuarantineRequest : D13RanchOperationRequest { }

    [Serializable]
    public sealed class D13RanchFeedRequest : D13RanchOperationRequest
    {
        public int Quantity;
    }

    [Serializable]
    public sealed class D13RanchWaterOperationRequest : D13RanchOperationRequest
    {
        public int Quantity;
    }

    [Serializable]
    public sealed class D13RanchLaborRequest : D13RanchOperationRequest
    {
        public int Quantity;
    }

    [Serializable]
    public sealed class D13RanchCycleRequest : D13RanchOperationRequest
    {
        public int Seed;
    }

    [Serializable]
    public sealed class D13RanchCollectProductRequest : D13RanchOperationRequest { }

    [Serializable]
    public sealed class D13RanchDiseaseTreatmentRequest : D13RanchOperationRequest
    {
        public int Quantity = 1;
    }

    [Serializable]
    public sealed class D13RanchSaleRequest : D13RanchOperationRequest
    {
        public string ItemId;
        public int Quantity;
        public string MarketRef = "market_santa_marys_ranch";
    }

    [Serializable]
    public sealed class D13RanchBreedingRequest : D13RanchOperationRequest
    {
        public string MateAnimalId;
    }

    [Serializable]
    public sealed class D13RanchSaleRecord
    {
        public string SaleId;
        public string ItemId;
        public int Quantity;
        public long UnitPrice;
        public long TotalPrice;
        public string Currency = D13RanchIds.Currency;
        public string MarketRef;
        public string LedgerRef;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13RanchSaleRecord Clone()
        {
            return (D13RanchSaleRecord)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class D13RanchRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13RanchIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13RanchAnimalState> Animals = new List<D13RanchAnimalState>();
        public List<D13RanchSaleRecord> Sales = new List<D13RanchSaleRecord>();
        public D13RanchWaterRuntimeSnapshot Water = new D13RanchWaterRuntimeSnapshot();
        public List<string> AppliedEventKeys = new List<string>();

        public D13RanchRuntimeSnapshot Clone()
        {
            var result = new D13RanchRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                Revision = Revision,
                Water = Water == null ? new D13RanchWaterRuntimeSnapshot() : Water.Clone(),
            };
            if (Animals != null) foreach (var item in Animals) if (item != null) result.Animals.Add(item.Clone());
            if (Sales != null) foreach (var item in Sales) if (item != null) result.Sales.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Animals == null || Sales == null || Water == null || AppliedEventKeys == null) throw new InvalidOperationException("D13 ranch snapshot is invalid.");
            Water.Validate();
            var animalIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Animals)
            {
                if (item == null) throw new InvalidOperationException("D13 ranch snapshot contains null animal.");
                item.Validate();
                if (!animalIds.Add(item.AnimalId)) throw new InvalidOperationException("D13 ranch snapshot duplicates animal: " + item.AnimalId);
            }
            var saleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Sales)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.SaleId) || string.IsNullOrWhiteSpace(item.ItemId) || item.Quantity <= 0 || item.UnitPrice < 0 || item.TotalPrice < 0 || item.WorldDay < 0 || item.SaveRevision < 0 || !saleIds.Add(item.SaleId))
                    throw new InvalidOperationException("D13 ranch sale record is invalid.");
            }
        }
    }

    [Serializable]
    public sealed class D13RanchResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string RecoveryRouteId;
        public D13RanchAnimalDefinition Definition;
        public D13RanchAnimalState State;
        public D13RanchWaterState Water;
        public int Quantity;
        public string ProductItemId;
        public long UnitPrice;
        public long TotalPrice;
        public string Currency;
        public string LedgerRef;
        public FactEvent Fact;

        public D13RanchResult Clone()
        {
            return new D13RanchResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                EventId = EventId,
                RecoveryRouteId = RecoveryRouteId,
                Definition = Definition == null ? null : Definition.Clone(),
                State = State == null ? null : State.Clone(),
                Water = Water == null ? null : Water.Clone(),
                Quantity = Quantity,
                ProductItemId = ProductItemId,
                UnitPrice = UnitPrice,
                TotalPrice = TotalPrice,
                Currency = Currency,
                LedgerRef = LedgerRef,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D13RanchRiskNotice
    {
        public string SubjectId;
        public string Risk;
        public string WorstCase;
        public string RecoveryRoute;
    }

    [Serializable]
    public sealed class D13RanchReadModel
    {
        public long Revision;
        public List<D13RanchFacilityDefinition> Facilities = new List<D13RanchFacilityDefinition>();
        public List<D13RanchAnimalDefinition> AnimalDefinitions = new List<D13RanchAnimalDefinition>();
        public List<D13RanchAnimalState> Animals = new List<D13RanchAnimalState>();
        public List<D13RanchWaterState> Water = new List<D13RanchWaterState>();
        public List<D13RanchSaleRecord> Sales = new List<D13RanchSaleRecord>();
        public List<D13RanchRiskNotice> Risks = new List<D13RanchRiskNotice>();
    }
}
