// ============================================================
// D11VegetationHarvestContracts —— D11-A07 植被采集契约
//
// 这些 DTO 只描述内容定义、玩家意图和已裁决结果。EcologySystem 负责
// 植被资源量/压力，D3InventorySystem 负责货舱数量；本文件不写运行时状态。
// ============================================================

using System;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public enum D11VegetationResourceKind
    {
        Fruit,
        Vegetable,
        Medicinal,
        Fiber,
        Timber,
        Resin,
        Dye,
        Toxin,
    }

    public enum D11VegetationMaturityStage
    {
        Seedling,
        Growing,
        Mature,
        Overripe,
    }

    public enum D11VegetationQuality
    {
        Poor,
        Standard,
        Good,
    }

    [Serializable]
    public sealed class D11VegetationToolDefinition
    {
        public string Id;
        public string ContentId;
        public string DisplayName;

        public D11VegetationToolDefinition Clone()
        {
            return new D11VegetationToolDefinition
            {
                Id = Id,
                ContentId = ContentId,
                DisplayName = DisplayName,
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationResourceDefinition
    {
        public string Id;
        public string DisplayName;
        public D11VegetationResourceKind Kind;
        public string ItemId;
        public string HabitatId;
        public string RequiredToolId;
        public int MinimumIdentificationSkill = 1;
        public int Capacity = 8;
        public int HarvestLimitPerAction = 2;
        public int BaseYield = 1;
        public int MaturityCycleDays = 6;
        public int MatureStartDay = 2;
        public int MatureDurationDays = 2;
        public bool Toxic;
        public bool ProtectedSpecies;

        public D11VegetationResourceDefinition Clone()
        {
            return new D11VegetationResourceDefinition
            {
                Id = Id,
                DisplayName = DisplayName,
                Kind = Kind,
                ItemId = ItemId,
                HabitatId = HabitatId,
                RequiredToolId = RequiredToolId,
                MinimumIdentificationSkill = MinimumIdentificationSkill,
                Capacity = Capacity,
                HarvestLimitPerAction = HarvestLimitPerAction,
                BaseYield = BaseYield,
                MaturityCycleDays = MaturityCycleDays,
                MatureStartDay = MatureStartDay,
                MatureDurationDays = MatureDurationDays,
                Toxic = Toxic,
                ProtectedSpecies = ProtectedSpecies,
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationProtectionAreaDefinition
    {
        public string Id;
        public string HabitatId;
        public string RuleId;
        public bool BlocksAllHarvest;
        public D11VegetationResourceKind[] BlockedKinds = new D11VegetationResourceKind[0];

        public bool Blocks(D11VegetationResourceDefinition resource)
        {
            if (resource == null || !string.Equals(HabitatId, resource.HabitatId,
                StringComparison.Ordinal)) return false;
            if (BlocksAllHarvest) return true;
            if (BlockedKinds == null) return false;
            foreach (var kind in BlockedKinds)
                if (kind == resource.Kind) return true;
            return false;
        }

        public D11VegetationProtectionAreaDefinition Clone()
        {
            return new D11VegetationProtectionAreaDefinition
            {
                Id = Id,
                HabitatId = HabitatId,
                RuleId = RuleId,
                BlocksAllHarvest = BlocksAllHarvest,
                BlockedKinds = BlockedKinds == null ? new D11VegetationResourceKind[0] :
                    (D11VegetationResourceKind[])BlockedKinds.Clone(),
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationIdentifyRequest
    {
        public string PlantId;
        public int BotanySkill;
        public bool HasFieldGuide;
        public bool HasCrewAdvice;
        public int Day;
        public int Seed;

        public D11VegetationIdentifyRequest Clone()
        {
            return new D11VegetationIdentifyRequest
            {
                PlantId = PlantId,
                BotanySkill = BotanySkill,
                HasFieldGuide = HasFieldGuide,
                HasCrewAdvice = HasCrewAdvice,
                Day = Day,
                Seed = Seed,
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationIdentificationResult
    {
        public bool Succeeded;
        public bool IsCorrect;
        public string Code;
        public string Message;
        public string ActualPlantId;
        public string IdentifiedPlantId;
        public D11VegetationResourceKind ResourceKind;
        public float Confidence;
        public bool Toxic;

        public D11VegetationIdentificationResult Clone()
        {
            return new D11VegetationIdentificationResult
            {
                Succeeded = Succeeded,
                IsCorrect = IsCorrect,
                Code = Code,
                Message = Message,
                ActualPlantId = ActualPlantId,
                IdentifiedPlantId = IdentifiedPlantId,
                ResourceKind = ResourceKind,
                Confidence = Confidence,
                Toxic = Toxic,
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationHarvestRequest
    {
        /// <summary>世界节点 ID；调用方只能提交引用，不能提交最终资源数量。</summary>
        public string PlantId;
        /// <summary>玩家/船员声称识别出的 ID；空值时由识别规则计算。</summary>
        public string ClaimedPlantId;
        public string ToolId;
        public int RequestedQuantity = 1;
        public int BotanySkill;
        public bool HasFieldGuide;
        public bool HasCrewAdvice;
        public bool AllowUnidentifiedHarvest;
        public string ProtectionAreaId;
        public int Day;
        public int Seed;
        public long SaveRevision;
        public string EventId;

        public D11VegetationHarvestRequest Clone()
        {
            return new D11VegetationHarvestRequest
            {
                PlantId = PlantId,
                ClaimedPlantId = ClaimedPlantId,
                ToolId = ToolId,
                RequestedQuantity = RequestedQuantity,
                BotanySkill = BotanySkill,
                HasFieldGuide = HasFieldGuide,
                HasCrewAdvice = HasCrewAdvice,
                AllowUnidentifiedHarvest = AllowUnidentifiedHarvest,
                ProtectionAreaId = ProtectionAreaId,
                Day = Day,
                Seed = Seed,
                SaveRevision = SaveRevision,
                EventId = EventId,
            };
        }
    }

    [Serializable]
    public sealed class D11VegetationHarvestResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string PlantId;
        public string IdentifiedPlantId;
        public string ItemId;
        public D11VegetationResourceKind ResourceKind;
        public D11VegetationMaturityStage MaturityStage;
        public D11VegetationQuality Quality;
        public string ToolId;
        public int ResourceQuantity;
        public int Quantity;
        public int CargoUsed;
        public float IdentificationConfidence;
        public bool IsCorrectIdentification;
        public bool Misidentified;
        public bool Toxic;
        public string ConsequenceId;
        public string ProtectionAreaId;
        public long SaveRevision;
        public string EventId;
        public FactEvent Fact;

        public D11VegetationHarvestResult Clone()
        {
            return new D11VegetationHarvestResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                PlantId = PlantId,
                IdentifiedPlantId = IdentifiedPlantId,
                ItemId = ItemId,
                ResourceKind = ResourceKind,
                MaturityStage = MaturityStage,
                Quality = Quality,
                ToolId = ToolId,
                ResourceQuantity = ResourceQuantity,
                Quantity = Quantity,
                CargoUsed = CargoUsed,
                IdentificationConfidence = IdentificationConfidence,
                IsCorrectIdentification = IsCorrectIdentification,
                Misidentified = Misidentified,
                Toxic = Toxic,
                ConsequenceId = ConsequenceId,
                ProtectionAreaId = ProtectionAreaId,
                SaveRevision = SaveRevision,
                EventId = EventId,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }
}
