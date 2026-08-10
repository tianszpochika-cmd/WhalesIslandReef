// D10AppearanceContracts —— 角色外观、服装与装备的领域契约。
//
// 外观快照只保存可验证的内容 ID 与状态，不保存 Unity 对象引用。
// 资产缺失由表现层报告；占有权、耐久、染色和伤病历史仍由 AppearanceSystem 写入。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public enum D10AppearanceSlot
    {
        Body,
        Hair,
        Hat,
        Top,
        Bottom,
        Coat,
        Shoes,
        Gloves,
        Accessory,
        CareerEquipment,
        Injury,
    }

    [Serializable]
    public sealed class D10AppearanceContext
    {
        public string CultureId = "island_saint_marys";
        public string CareerId = "deckhand";
        public string WeatherId = "clear";
        public string LegalIdentityId = "player";

        public D10AppearanceContext Clone()
        {
            return new D10AppearanceContext
            {
                CultureId = CultureId ?? "island_saint_marys",
                CareerId = CareerId ?? "deckhand",
                WeatherId = WeatherId ?? "clear",
                LegalIdentityId = LegalIdentityId ?? "player",
            };
        }
    }

    [Serializable]
    public sealed class D10AppearanceItemDefinition
    {
        public string Id;
        public string AssetId;
        public string ResourceKey;
        public D10AppearanceSlot Slot;
        public string DisplayNameKey;
        public int Price;
        public int RepairCost;
        public int MaxDurability = 100;
        public bool Dyeable;
        public bool StarterOwned;
        public bool IsInjuryOverlay;
        public List<string> CultureTags = new List<string>();
        public List<string> CareerTags = new List<string>();
        public List<string> WeatherTags = new List<string>();
        public List<string> HistoricalTags = new List<string>();
        public string ExclusiveGroup;
        public List<string> ExcludesItemIds = new List<string>();

        public D10AppearanceItemDefinition Clone()
        {
            var result = new D10AppearanceItemDefinition
            {
                Id = Id,
                AssetId = AssetId,
                ResourceKey = ResourceKey,
                Slot = Slot,
                DisplayNameKey = DisplayNameKey,
                Price = Price,
                RepairCost = RepairCost,
                MaxDurability = MaxDurability,
                Dyeable = Dyeable,
                StarterOwned = StarterOwned,
                IsInjuryOverlay = IsInjuryOverlay,
                ExclusiveGroup = ExclusiveGroup,
                CultureTags = new List<string>(),
                CareerTags = new List<string>(),
                WeatherTags = new List<string>(),
                HistoricalTags = new List<string>(),
                ExcludesItemIds = new List<string>(),
            };
            if (CultureTags != null) result.CultureTags.AddRange(CultureTags);
            if (CareerTags != null) result.CareerTags.AddRange(CareerTags);
            if (WeatherTags != null) result.WeatherTags.AddRange(WeatherTags);
            if (HistoricalTags != null) result.HistoricalTags.AddRange(HistoricalTags);
            if (ExcludesItemIds != null) result.ExcludesItemIds.AddRange(ExcludesItemIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D10OwnedAppearance
    {
        public string ItemId;
        public int Durability = 100;
        public string DyeId = "";
        public string DyeColor = "";
        public int PurchaseCount;

        public D10OwnedAppearance Clone()
        {
            return new D10OwnedAppearance
            {
                ItemId = ItemId,
                Durability = Durability,
                DyeId = DyeId ?? "",
                DyeColor = DyeColor ?? "",
                PurchaseCount = PurchaseCount,
            };
        }
    }

    [Serializable]
    public sealed class D10AppearanceLoadout
    {
        public Dictionary<string, string> Slots = new Dictionary<string, string>(StringComparer.Ordinal);

        public string Get(D10AppearanceSlot slot)
        {
            string itemId;
            return Slots.TryGetValue(slot.ToString(), out itemId) ? itemId : "";
        }

        public void Set(D10AppearanceSlot slot, string itemId)
        {
            Slots[slot.ToString()] = itemId ?? "";
        }

        public D10AppearanceLoadout Clone()
        {
            var result = new D10AppearanceLoadout();
            foreach (var pair in Slots) result.Slots[pair.Key] = pair.Value ?? "";
            return result;
        }
    }

    [Serializable]
    public sealed class D10AppearanceSnapshot
    {
        public int SchemaVersion = 1;
        public D10AppearanceContext Context = new D10AppearanceContext();
        public D10AppearanceLoadout ActiveLoadout = new D10AppearanceLoadout();
        public Dictionary<string, D10OwnedAppearance> Owned =
            new Dictionary<string, D10OwnedAppearance>(StringComparer.Ordinal);
        public List<string> ActiveInjuryIds = new List<string>();
        public List<string> InjuryHistoryIds = new List<string>();
        public long SaveRevision;

        public D10AppearanceSnapshot Clone()
        {
            var result = new D10AppearanceSnapshot
            {
                SchemaVersion = SchemaVersion,
                Context = Context == null ? new D10AppearanceContext() : Context.Clone(),
                ActiveLoadout = ActiveLoadout == null ? new D10AppearanceLoadout() : ActiveLoadout.Clone(),
                Owned = new Dictionary<string, D10OwnedAppearance>(StringComparer.Ordinal),
                ActiveInjuryIds = new List<string>(),
                InjuryHistoryIds = new List<string>(),
                SaveRevision = SaveRevision,
            };
            if (Owned != null)
                foreach (var pair in Owned)
                    result.Owned[pair.Key] = pair.Value == null ? null : pair.Value.Clone();
            if (ActiveInjuryIds != null) result.ActiveInjuryIds.AddRange(ActiveInjuryIds);
            if (InjuryHistoryIds != null) result.InjuryHistoryIds.AddRange(InjuryHistoryIds);
            return result;
        }
    }

    public enum D10AppearanceResultCode
    {
        Success,
        InvalidItemId,
        InvalidSlot,
        NotOwned,
        AlreadyOwned,
        NotDyeable,
        InvalidDye,
        AlreadyRepaired,
        InsufficientFunds,
        CultureRestricted,
        CareerRestricted,
        WeatherRestricted,
        SlotConflict,
        MissingResourcePlaceholder,
        InjuryProtected,
        InvalidContext,
    }

    public sealed class D10AppearanceOperationResult
    {
        public bool Success;
        public D10AppearanceResultCode Code;
        public string Message = "";
        public string ItemId = "";
        public string AssetId = "";
        public string ResourceStatus = "ready";
        public bool UsesPlaceholder;
        public int Cost;
        public D10AppearanceLoadout PreviewLoadout;
        public FactEvent Fact;

        public static D10AppearanceOperationResult Failed(D10AppearanceResultCode code, string message, string itemId = "")
        {
            return new D10AppearanceOperationResult
            {
                Success = false,
                Code = code,
                Message = message,
                ItemId = itemId ?? "",
            };
        }
    }
}
