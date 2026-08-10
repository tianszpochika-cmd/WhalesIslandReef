// ============================================================
// D11FishingHarvestContracts —— D11-A02 捕捞/浅水采集纯 C# 契约
//
// 请求只携带内容引用和环境意图；EcologySystem 裁决资源量、压力、保护区
// 与鱼获质量，D3InventorySystem 写库存，TradeEconomySystem 写价格/市场反馈。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public enum D11HarvestMethod
    {
        Handline,
        ShoreFishing,
        CastNet,
        FishTrap,
        ShellfishGathering,
        SeaweedGathering,
    }

    public enum D11ProcessingMethod
    {
        Salt,
        Dry,
        RenderOil,
        CureShellfish,
    }

    [Serializable]
    public sealed class D11FishingGearDefinition
    {
        public string Id;
        public string ContentId;
        public string DisplayName;
        public D11HarvestMethod Method;
        public int CatchCapacity;
        public float MaximumDepthMeters;
        public bool WorksInProtectedArea;

        public D11FishingGearDefinition Clone()
        {
            return new D11FishingGearDefinition
            {
                Id = Id,
                ContentId = ContentId,
                DisplayName = DisplayName,
                Method = Method,
                CatchCapacity = CatchCapacity,
                MaximumDepthMeters = MaximumDepthMeters,
                WorksInProtectedArea = WorksInProtectedArea,
            };
        }
    }

    [Serializable]
    public sealed class D11FishingLocationDefinition
    {
        public string Id;
        public string FishSchoolId;
        public string FishSpeciesId;
        public string Habitat;
        public float PreferredDepthMeters;
        public string PreferredTimeOfDay;
        public string PreferredTideBand;
        public bool IsProtectedArea;
        public string ProtectionRuleId;
        public string ResourceId;
        public D11HarvestMethod Method;
        public float WorldX;
        public float WorldZ;

        public bool IsFishLocation { get { return !string.IsNullOrEmpty(FishSchoolId); } }
        public bool IsShallowResource { get { return !string.IsNullOrEmpty(ResourceId); } }

        public D11FishingLocationDefinition Clone()
        {
            return new D11FishingLocationDefinition
            {
                Id = Id,
                FishSchoolId = FishSchoolId,
                FishSpeciesId = FishSpeciesId,
                Habitat = Habitat,
                PreferredDepthMeters = PreferredDepthMeters,
                PreferredTimeOfDay = PreferredTimeOfDay,
                PreferredTideBand = PreferredTideBand,
                IsProtectedArea = IsProtectedArea,
                ProtectionRuleId = ProtectionRuleId,
                ResourceId = ResourceId,
                Method = Method,
                WorldX = WorldX,
                WorldZ = WorldZ,
            };
        }
    }

    [Serializable]
    public sealed class D11HarvestRequest
    {
        public string LocationId;
        public string GearId;
        public int Day;
        public int Seed;
        public float DepthMeters;
        public string TimeOfDayBucket;
        public string TideBand;
        public bool HasConservationPermit;
        public long SaveRevision;

        public D11HarvestRequest Clone()
        {
            return new D11HarvestRequest
            {
                LocationId = LocationId,
                GearId = GearId,
                Day = Day,
                Seed = Seed,
                DepthMeters = DepthMeters,
                TimeOfDayBucket = TimeOfDayBucket,
                TideBand = TideBand,
                HasConservationPermit = HasConservationPermit,
                SaveRevision = SaveRevision,
            };
        }
    }

    [Serializable]
    public sealed class D11HarvestResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string LocationId;
        public string FishSchoolId;
        public string FishSpeciesId;
        public string ResourceId;
        public string ResourceKind;
        public string GearId;
        public D11HarvestMethod Method;
        public int WorldDay;
        public string ItemId;
        public int Quantity;
        public QualityGrade Quality = QualityGrade.Unknown;
        public int QualityScore;
        public float DepthMeters;
        public string TimeOfDayBucket;
        public string TideBand;
        public float DepthMultiplier;
        public float TimeMultiplier;
        public float TideMultiplier;
        public float GearMultiplier;
        public int FishingPressureAfter;
        public string ProtectionRuleId;
        public long SaveRevision;
        public string EventId;
        public FactEvent Fact;

        public D11HarvestResult Clone()
        {
            return new D11HarvestResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                LocationId = LocationId,
                FishSchoolId = FishSchoolId,
                FishSpeciesId = FishSpeciesId,
                ResourceId = ResourceId,
                ResourceKind = ResourceKind,
                GearId = GearId,
                Method = Method,
                WorldDay = WorldDay,
                ItemId = ItemId,
                Quantity = Quantity,
                Quality = Quality,
                QualityScore = QualityScore,
                DepthMeters = DepthMeters,
                TimeOfDayBucket = TimeOfDayBucket,
                TideBand = TideBand,
                DepthMultiplier = DepthMultiplier,
                TimeMultiplier = TimeMultiplier,
                TideMultiplier = TideMultiplier,
                GearMultiplier = GearMultiplier,
                FishingPressureAfter = FishingPressureAfter,
                ProtectionRuleId = ProtectionRuleId,
                SaveRevision = SaveRevision,
                EventId = EventId,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class D11ProcessingDefinition
    {
        public string InputItemId;
        public string OutputItemId;
        public D11ProcessingMethod Method;
        public int OutputNumerator = 1;
        public int OutputDenominator = 1;
        public bool RequiresPort;
        public string DisplayName;

        public int OutputFor(int quantity)
        {
            if (quantity <= 0) return 0;
            long output = (long)quantity * Math.Max(1, OutputNumerator) /
                Math.Max(1, OutputDenominator);
            return Math.Max(1, output > int.MaxValue ? int.MaxValue : (int)output);
        }
    }

    [Serializable]
    public sealed class D11ProcessingResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public D11ProcessingMethod Method;
        public string InputItemId;
        public string OutputItemId;
        public int InputQuantity;
        public int OutputQuantity;
        public int CargoUsed;
        public string EventId;
        public long SaveRevision;
        public FactEvent Fact;
    }

    [Serializable]
    public sealed class D11FishingHarvestRuntimeSnapshot
    {
        public int SchemaVersion = 1;
        public long NextHarvestRevision;
        public long NextProcessingRevision;
        public D11HarvestResult LastHarvest;
        public EventLogSnapshot EventLog;

        public D11FishingHarvestRuntimeSnapshot Clone()
        {
            return new D11FishingHarvestRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextHarvestRevision = NextHarvestRevision,
                NextProcessingRevision = NextProcessingRevision,
                LastHarvest = LastHarvest == null ? null : LastHarvest.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
        }
    }
}
