// ============================================================
// D9FishingContracts —— D9-A02 捕捞闭环的纯 C# 契约
//
// 位置、生物量和捕捞压力由 D4EcologySystem 写入；价格、市场库存和金币
// 由 TradeEconomySystem 写入；货舱/随身物品数量由 D3InventorySystem 写入。
// 本文件只描述输入、结果和可保存快照，不引用 Unity、网络或 LLM。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    public static class D9FishingIds
    {
        public const string FishSchoolId = "fish_mackerel_school";
        public const string FishingSpotId = "nearshore_saint_marys";
        public const string GearId = "gear_handline_basic";
        public const string GearContentId = "item_gear_handline_basic";
        public const string FreshFishItemId = "item_fresh_mackerel";
        public const string SaltedFishItemId = "item_salted_mackerel";
        public const string CookedFishItemId = "item_cooked_mackerel";
        public const string CatchEventId = "event_d9_fishing_catch";
    }

    [Serializable]
    public sealed class FishingGearDefinition
    {
        public string Id;
        public string ContentId;
        public string DisplayName;
        public int CatchCapacity;

        public FishingGearDefinition Clone()
        {
            return new FishingGearDefinition
            {
                Id = Id,
                ContentId = ContentId,
                DisplayName = DisplayName,
                CatchCapacity = CatchCapacity,
            };
        }
    }

    [Serializable]
    public sealed class FishingSpotDefinition
    {
        public string Id;
        public string FishSchoolId;
        public string Habitat;
        public float WorldX;
        public float WorldZ;

        public FishingSpotDefinition Clone()
        {
            return new FishingSpotDefinition
            {
                Id = Id,
                FishSchoolId = FishSchoolId,
                Habitat = Habitat,
                WorldX = WorldX,
                WorldZ = WorldZ,
            };
        }
    }

    [Serializable]
    public sealed class FishingRequest
    {
        public string FishingSpotId;
        public string GearId;
        public int Day;
        public int Seed;
        public long SaveRevision;

        public FishingRequest Clone()
        {
            return new FishingRequest
            {
                FishingSpotId = FishingSpotId,
                GearId = GearId,
                Day = Day,
                Seed = Seed,
                SaveRevision = SaveRevision,
            };
        }
    }

    [Serializable]
    public sealed class FishingResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string FishingSpotId;
        public string FishSchoolId;
        public string GearId;
        public string ItemId;
        public int Quantity;
        public long SaveRevision;
        public string EventId;
        public FactEvent Fact;

        public FishingResult Clone()
        {
            return new FishingResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                FishingSpotId = FishingSpotId,
                FishSchoolId = FishSchoolId,
                GearId = GearId,
                ItemId = ItemId,
                Quantity = Quantity,
                SaveRevision = SaveRevision,
                EventId = EventId,
                Fact = Fact == null ? null : Fact.CloneFact(),
            };
        }
    }

    [Serializable]
    public sealed class FishProcessingResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public string InputItemId;
        public string OutputItemId;
        public int Quantity;
        public int CargoUsed;
    }

    [Serializable]
    public sealed class FishCookingResult
    {
        public bool Succeeded;
        public string Code;
        public string Message;
        public string InputItemId;
        public string OutputItemId;
        public int Quantity;
        public int PersonalCount;
    }

    [Serializable]
    public sealed class D9FishingRuntimeSnapshot
    {
        public int SchemaVersion = 1;
        public long NextCatchRevision;
        public FishingResult LastCatch;
        public EventLogSnapshot EventLog;

        public D9FishingRuntimeSnapshot Clone()
        {
            return new D9FishingRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextCatchRevision = NextCatchRevision,
                LastCatch = LastCatch == null ? null : LastCatch.Clone(),
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
        }
    }
}
