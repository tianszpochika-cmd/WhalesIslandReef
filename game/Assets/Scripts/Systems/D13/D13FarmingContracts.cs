// D13-A03 农业、果园与蔬菜种植契约。
// FarmingSystem 只写作物生长/收获状态；EcologySystem、Inventory/Cargo、
// PropertySystem 和 EconomySystem 通过 authority 保持各自唯一写者边界。

using System;
using System.Collections.Generic;
using System.Linq;

using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13FarmingIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "FarmingSystem";
        public const string EcologyOwner = "EcologySystem";
        public const string InventoryOwner = "D3InventorySystem";
        public const string PropertyOwner = "PropertySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string PlayerRef = "player";
        public const string Currency = "coin";
        public const string LandRecoveryRoute = "route_farming_land_review";
        public const string WaterRecoveryRoute = "route_farming_water_review";
        public const string CropRecoveryRoute = "route_farming_crop_recovery";
        public const string InventoryRecoveryRoute = "route_farming_inventory_capacity";
        public const string EconomyRecoveryRoute = "route_farming_market_review";
        public const string EcologyRecoveryRoute = "route_farming_ecology_review";
    }

    public enum D13FarmingSeason
    {
        Dry,
        Wet,
        Hurricane,
    }

    public enum D13FarmingCropKind
    {
        Fruit,
        Vegetable,
        Sugarcane,
        Tobacco,
        Coffee,
        Cocoa,
        Spice,
        Herb,
    }

    public enum D13FarmingCropStateKind
    {
        Empty,
        Planted,
        Growing,
        Infested,
        StormDamaged,
        Ready,
        Withered,
    }

    public enum D13FarmingEcologyOperation
    {
        Planting,
        Watering,
        PestInfestation,
        PestTreatment,
        Storm,
        Harvest,
        Rotation,
        AdvanceDay,
    }

    [Serializable]
    public sealed class D13FarmingCropDefinition
    {
        public string CropId;
        public string DisplayName;
        public D13FarmingCropKind Kind;
        public string SeedItemId;
        public string HarvestItemId;
        public string ProcessedItemId;
        public string PestTreatmentItemId;
        public string RequiredSoilType = "loam";
        public string RotationFamily;
        public string MarketRef;
        public int GrowthDays = 6;
        public int BaseYield = 2;
        public int LaborRequired = 2;
        public int WaterPerDay = 10;
        public int PestRiskPercent = 12;
        public int StormResistancePercent = 50;
        public int ProcessInputPerBatch = 2;
        public int ProcessOutputPerBatch = 1;
        public int ProcessDays = 1;
        public List<D13FarmingSeason> AllowedSeasons = new List<D13FarmingSeason>();
        public List<string> Tags = new List<string>();

        public D13FarmingCropDefinition Clone()
        {
            var result = new D13FarmingCropDefinition
            {
                CropId = CropId,
                DisplayName = DisplayName,
                Kind = Kind,
                SeedItemId = SeedItemId,
                HarvestItemId = HarvestItemId,
                ProcessedItemId = ProcessedItemId,
                PestTreatmentItemId = PestTreatmentItemId,
                RequiredSoilType = RequiredSoilType,
                RotationFamily = RotationFamily,
                MarketRef = MarketRef,
                GrowthDays = GrowthDays,
                BaseYield = BaseYield,
                LaborRequired = LaborRequired,
                WaterPerDay = WaterPerDay,
                PestRiskPercent = PestRiskPercent,
                StormResistancePercent = StormResistancePercent,
                ProcessInputPerBatch = ProcessInputPerBatch,
                ProcessOutputPerBatch = ProcessOutputPerBatch,
                ProcessDays = ProcessDays,
            };
            if (AllowedSeasons != null) result.AllowedSeasons.AddRange(AllowedSeasons);
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            Require(CropId, "crop_id");
            Require(SeedItemId, "seed_item_id");
            Require(HarvestItemId, "harvest_item_id");
            Require(ProcessedItemId, "processed_item_id");
            Require(PestTreatmentItemId, "pest_treatment_item_id");
            Require(RequiredSoilType, "required_soil_type");
            Require(RotationFamily, "rotation_family");
            Require(MarketRef, "market_ref");
            if (!Enum.IsDefined(typeof(D13FarmingCropKind), Kind)) throw new InvalidOperationException("Invalid farming crop kind: " + CropId);
            if (GrowthDays <= 0 || BaseYield <= 0 || LaborRequired < 0 || WaterPerDay < 0) throw new InvalidOperationException("Invalid crop growth values: " + CropId);
            if (PestRiskPercent < 0 || PestRiskPercent > 100 || StormResistancePercent < 0 || StormResistancePercent > 100) throw new InvalidOperationException("Invalid crop risk values: " + CropId);
            if (ProcessInputPerBatch <= 0 || ProcessOutputPerBatch <= 0 || ProcessDays <= 0) throw new InvalidOperationException("Invalid crop processing values: " + CropId);
            if (AllowedSeasons == null || AllowedSeasons.Count == 0 || Tags == null || Tags.Count == 0) throw new InvalidOperationException("Crop seasons and tags are required: " + CropId);
            foreach (var season in AllowedSeasons)
                if (!Enum.IsDefined(typeof(D13FarmingSeason), season)) throw new InvalidOperationException("Invalid crop season: " + CropId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Farming " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FarmingPlotDefinition
    {
        public string PlotId;
        public string PropertyId;
        public string DisplayName;
        public string SoilType = "loam";
        public string WaterSourceId;
        public int AreaUnits = 1;
        public bool ProtectedArea;
        public List<string> Tags = new List<string>();

        public D13FarmingPlotDefinition Clone()
        {
            var result = new D13FarmingPlotDefinition
            {
                PlotId = PlotId,
                PropertyId = PropertyId,
                DisplayName = DisplayName,
                SoilType = SoilType,
                WaterSourceId = WaterSourceId,
                AreaUnits = AreaUnits,
                ProtectedArea = ProtectedArea,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            Require(PlotId, "plot_id");
            Require(PropertyId, "property_id");
            Require(SoilType, "soil_type");
            Require(WaterSourceId, "water_source_id");
            if (AreaUnits <= 0) throw new InvalidOperationException("Farming plot area must be positive: " + PlotId);
            if (Tags == null) throw new InvalidOperationException("Farming plot tags are required: " + PlotId);
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Farming " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FarmingPlotState
    {
        public string PlotId;
        public string PropertyId;
        public string PlantingId;
        public string CropId;
        public string SeedItemId;
        public D13FarmingCropKind CropKind;
        public D13FarmingCropStateKind State = D13FarmingCropStateKind.Empty;
        public D13FarmingSeason PlantedSeason;
        public int PlantDay;
        public int LastUpdatedDay;
        public int GrowthPoints;
        public int GrowthPercent;
        public int WaterApplied;
        public int LaborApplied;
        public int PestPressure;
        public int StormDamage;
        public int Health = 100;
        public int HarvestCount;
        public int RotationIndex;
        public long Revision;
        public string LastEventId;
        public List<string> RotationHistory = new List<string>();

        public D13FarmingPlotState Clone()
        {
            var result = new D13FarmingPlotState
            {
                PlotId = PlotId,
                PropertyId = PropertyId,
                PlantingId = PlantingId,
                CropId = CropId,
                SeedItemId = SeedItemId,
                CropKind = CropKind,
                State = State,
                PlantedSeason = PlantedSeason,
                PlantDay = PlantDay,
                LastUpdatedDay = LastUpdatedDay,
                GrowthPoints = GrowthPoints,
                GrowthPercent = GrowthPercent,
                WaterApplied = WaterApplied,
                LaborApplied = LaborApplied,
                PestPressure = PestPressure,
                StormDamage = StormDamage,
                Health = Health,
                HarvestCount = HarvestCount,
                RotationIndex = RotationIndex,
                Revision = Revision,
                LastEventId = LastEventId,
            };
            if (RotationHistory != null) result.RotationHistory.AddRange(RotationHistory);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PlotId) || string.IsNullOrWhiteSpace(PropertyId)) throw new InvalidOperationException("Farming plot state ids are required.");
            if (!Enum.IsDefined(typeof(D13FarmingCropKind), CropKind) || !Enum.IsDefined(typeof(D13FarmingCropStateKind), State) || !Enum.IsDefined(typeof(D13FarmingSeason), PlantedSeason)) throw new InvalidOperationException("Farming plot state enum is invalid: " + PlotId);
            if (PlantDay < 0 || LastUpdatedDay < 0 || GrowthPoints < 0 || GrowthPercent < 0 || GrowthPercent > 100 || WaterApplied < 0 || LaborApplied < 0 || PestPressure < 0 || StormDamage < 0 || Health < 0 || Health > 100 || HarvestCount < 0 || RotationIndex < 0 || Revision < 0) throw new InvalidOperationException("Farming plot state value is invalid: " + PlotId);
            if (RotationHistory == null) throw new InvalidOperationException("Farming rotation history is required: " + PlotId);
            if (State == D13FarmingCropStateKind.Empty)
            {
                if (!string.IsNullOrEmpty(CropId) || !string.IsNullOrEmpty(PlantingId)) throw new InvalidOperationException("Empty farming plot cannot hold crop state: " + PlotId);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(CropId) || string.IsNullOrWhiteSpace(PlantingId)) throw new InvalidOperationException("Active farming plot needs crop and planting ids: " + PlotId);
            }
        }
    }

    [Serializable]
    public sealed class D13FarmingEcologyEnvironmentState
    {
        public string PlotId;
        public string SoilType;
        public int Fertility = 70;
        public int Moisture = 50;
        public int WaterAvailable = 100;
        public int WaterCapacity = 100;
        public string WaterSourceId;
        public int Pollution;
        public int EcologyPressure;
        public int PestPressure;
        public int StormDamage;
        public int LastUpdatedDay;
        public long Revision;
        public string LastEventId;

        public D13FarmingEcologyEnvironmentState Clone()
        {
            return new D13FarmingEcologyEnvironmentState
            {
                PlotId = PlotId,
                SoilType = SoilType,
                Fertility = Fertility,
                Moisture = Moisture,
                WaterAvailable = WaterAvailable,
                WaterCapacity = WaterCapacity,
                WaterSourceId = WaterSourceId,
                Pollution = Pollution,
                EcologyPressure = EcologyPressure,
                PestPressure = PestPressure,
                StormDamage = StormDamage,
                LastUpdatedDay = LastUpdatedDay,
                Revision = Revision,
                LastEventId = LastEventId,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(PlotId) || string.IsNullOrWhiteSpace(SoilType) || string.IsNullOrWhiteSpace(WaterSourceId)) throw new InvalidOperationException("Farming ecology ids are required.");
            if (Fertility < 0 || Fertility > 100 || Moisture < 0 || Moisture > 100 || WaterAvailable < 0 || WaterCapacity <= 0 || WaterAvailable > WaterCapacity || Pollution < 0 || Pollution > 100 || EcologyPressure < 0 || PestPressure < 0 || StormDamage < 0 || StormDamage > 100 || LastUpdatedDay < 0 || Revision < 0) throw new InvalidOperationException("Farming ecology values are invalid: " + PlotId);
        }
    }

    [Serializable]
    public sealed class D13FarmingEcologyRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13FarmingIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13FarmingEcologyEnvironmentState> Environments = new List<D13FarmingEcologyEnvironmentState>();
        public List<string> AppliedEventKeys = new List<string>();

        public D13FarmingEcologyRuntimeSnapshot Clone()
        {
            var result = new D13FarmingEcologyRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Environments != null) foreach (var item in Environments) if (item != null) result.Environments.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Environments == null || AppliedEventKeys == null) throw new InvalidOperationException("D13 farming ecology snapshot is invalid.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Environments)
            {
                if (item == null) throw new InvalidOperationException("D13 farming ecology snapshot contains null.");
                item.Validate();
                if (!ids.Add(item.PlotId)) throw new InvalidOperationException("D13 farming ecology duplicated plot: " + item.PlotId);
            }
        }
    }

    [Serializable]
    public sealed class D13FarmingEnvironmentRequest
    {
        public string PlotId;
        public D13FarmingEcologyOperation Operation;
        public string CropId;
        public int Amount;
        public int Days = 1;
        public int StormIntensity;
        public bool Rainfall;
        public int WorldDay;
        public int Seed;
        public string EventId;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13FarmingEnvironmentResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public bool Applied;
        public string Code;
        public string Message;
        public string EventId;
        public int Quantity;
        public D13FarmingEcologyEnvironmentState Environment;
        public FactEvent Fact;
    }

    public interface ID13FarmingEcologyAuthority
    {
        D13FarmingEcologyEnvironmentState EnsurePlot(D13FarmingPlotDefinition definition, int worldDay, int seed);
        D13FarmingEcologyEnvironmentState FindPlot(string plotId);
        D13FarmingEnvironmentResult Apply(D13FarmingEnvironmentRequest request, EventLog eventLog);
        D13FarmingEcologyRuntimeSnapshot CaptureSnapshot();
        void RestoreSnapshot(D13FarmingEcologyRuntimeSnapshot snapshot);
    }

    [Serializable]
    public sealed class D13FarmingLandDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string ControllerRef;
        public string PropertySourceEventId;
        public string LegalSourceEventId;
    }

    public interface ID13FarmingLandAuthority
    {
        D13FarmingLandDecision ValidatePlot(D13FarmingPlotDefinition definition, string actorRef, int worldDay);
    }

    [Serializable]
    public sealed class D13FarmingSaleDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public long UnitPrice;
        public long TotalPrice;
        public string Currency = D13FarmingIds.Currency;
        public string MarketRef;
        public string LedgerRef;
        public string SourceEventId;
        public string SourceOwnerSystem = D13FarmingIds.EconomyOwner;
    }

    public interface ID13FarmingEconomyAuthority
    {
        D13FarmingSaleDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision);
    }

    [Serializable]
    public abstract class D13FarmingOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13FarmingIds.PlayerRef;
        public string PlotId;
        public int WorldDay;
        public long SaveRevision;
        public string LocationRef = "island_saint_marys";
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D13FarmingPlantRequest : D13FarmingOperationRequest
    {
        public string CropId;
        public string SeedItemId;
        public int Seed = 1;
    }

    [Serializable]
    public sealed class D13FarmingWaterRequest : D13FarmingOperationRequest
    {
        public int WaterUnits;
    }

    [Serializable]
    public sealed class D13FarmingLaborRequest : D13FarmingOperationRequest
    {
        public int LaborUnits;
    }

    [Serializable]
    public sealed class D13FarmingPestTreatmentRequest : D13FarmingOperationRequest
    {
        public string TreatmentItemId;
        public int TreatmentUnits = 1;
        public int Skill;
    }

    [Serializable]
    public sealed class D13FarmingAdvanceRequest : D13FarmingOperationRequest
    {
        public int Days = 1;
        public int Seed;
        public string WeatherId = "Clear";
        public bool Storm;
        public int StormIntensity = 35;
        public bool Rainfall;
    }

    [Serializable]
    public sealed class D13FarmingHarvestRequest : D13FarmingOperationRequest
    {
        public int Seed;
    }

    [Serializable]
    public sealed class D13FarmingRotationRequest : D13FarmingOperationRequest
    {
        public string NextCropId;
    }

    [Serializable]
    public sealed class D13FarmingProcessRequest : D13FarmingOperationRequest
    {
        public string InputItemId;
        public int Quantity;
    }

    [Serializable]
    public sealed class D13FarmingSaleRequest : D13FarmingOperationRequest
    {
        public string ItemId;
        public int Quantity;
        public string MarketRef = "market_santa_marys";
    }

    [Serializable]
    public sealed class D13FarmingProcessRecord
    {
        public string ProcessId;
        public string PlotId;
        public string InputItemId;
        public string OutputItemId;
        public int InputQuantity;
        public int OutputQuantity;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13FarmingProcessRecord Clone()
        {
            return (D13FarmingProcessRecord)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class D13FarmingSaleRecord
    {
        public string SaleId;
        public string ItemId;
        public int Quantity;
        public long TotalPrice;
        public long UnitPrice;
        public string Currency = D13FarmingIds.Currency;
        public string MarketRef;
        public string LedgerRef;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13FarmingSaleRecord Clone()
        {
            return (D13FarmingSaleRecord)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class D13FarmingRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13FarmingIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13FarmingPlotState> Plots = new List<D13FarmingPlotState>();
        public List<D13FarmingProcessRecord> Processes = new List<D13FarmingProcessRecord>();
        public List<D13FarmingSaleRecord> Sales = new List<D13FarmingSaleRecord>();
        public D13FarmingEcologyRuntimeSnapshot Ecology = new D13FarmingEcologyRuntimeSnapshot();
        public List<string> AppliedEventKeys = new List<string>();

        public D13FarmingRuntimeSnapshot Clone()
        {
            var result = new D13FarmingRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision, Ecology = Ecology == null ? new D13FarmingEcologyRuntimeSnapshot() : Ecology.Clone() };
            if (Plots != null) foreach (var item in Plots) if (item != null) result.Plots.Add(item.Clone());
            if (Processes != null) foreach (var item in Processes) if (item != null) result.Processes.Add(item.Clone());
            if (Sales != null) foreach (var item in Sales) if (item != null) result.Sales.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Plots == null || Processes == null || Sales == null || Ecology == null || AppliedEventKeys == null) throw new InvalidOperationException("D13 farming snapshot is invalid.");
            Ecology.Validate();
            var plotIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Plots)
            {
                if (item == null) throw new InvalidOperationException("D13 farming snapshot contains null plot.");
                item.Validate();
                if (!plotIds.Add(item.PlotId)) throw new InvalidOperationException("D13 farming duplicated plot: " + item.PlotId);
            }
            var processIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Processes)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.ProcessId) || item.InputQuantity <= 0 || item.OutputQuantity <= 0 || item.WorldDay < 0 || item.SaveRevision < 0 || !processIds.Add(item.ProcessId)) throw new InvalidOperationException("D13 farming process record is invalid.");
            }
            var saleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Sales)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.SaleId) || item.Quantity <= 0 || item.TotalPrice < 0 || item.UnitPrice < 0 || item.WorldDay < 0 || item.SaveRevision < 0 || !saleIds.Add(item.SaleId)) throw new InvalidOperationException("D13 farming sale record is invalid.");
            }
        }
    }

    [Serializable]
    public sealed class D13FarmingResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string RecoveryRouteId;
        public D13FarmingCropDefinition Crop;
        public D13FarmingPlotState Plot;
        public D13FarmingEcologyEnvironmentState Environment;
        public int Quantity;
        public int InputQuantity;
        public string InputItemId;
        public string OutputItemId;
        public long UnitPrice;
        public long TotalPrice;
        public string Currency;
        public string LedgerRef;
        public FactEvent Fact;

        public D13FarmingResult Clone()
        {
            return new D13FarmingResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                EventId = EventId,
                RecoveryRouteId = RecoveryRouteId,
                Crop = Crop == null ? null : Crop.Clone(),
                Plot = Plot == null ? null : Plot.Clone(),
                Environment = Environment == null ? null : Environment.Clone(),
                Quantity = Quantity,
                InputQuantity = InputQuantity,
                InputItemId = InputItemId,
                OutputItemId = OutputItemId,
                UnitPrice = UnitPrice,
                TotalPrice = TotalPrice,
                Currency = Currency,
                LedgerRef = LedgerRef,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D13FarmingReadModel
    {
        public long Revision;
        public List<D13FarmingPlotDefinition> PlotDefinitions = new List<D13FarmingPlotDefinition>();
        public List<D13FarmingCropDefinition> CropDefinitions = new List<D13FarmingCropDefinition>();
        public List<D13FarmingPlotState> Plots = new List<D13FarmingPlotState>();
        public List<D13FarmingEcologyEnvironmentState> Environments = new List<D13FarmingEcologyEnvironmentState>();
        public List<D13FarmingProcessRecord> Processes = new List<D13FarmingProcessRecord>();
        public List<D13FarmingSaleRecord> Sales = new List<D13FarmingSaleRecord>();
        public List<D13FarmingRiskNotice> Risks = new List<D13FarmingRiskNotice>();
    }

    [Serializable]
    public sealed class D13FarmingRiskNotice
    {
        public string SubjectId;
        public string Risk;
        public string WorstCase;
        public string RecoveryRoute;
    }
}
