// ============================================================
// D22InventoryReadModelAdapter —— D22-L20 库存只读 UI 适配器（纯 C#）
//
// D3InventorySystem 仍是库存、容量与重量的唯一 owner。本适配器只读取
// D3InventoryState/TradeResult/D19 交易结果，所有条目按稳定键复制排序；
// 它不会调用 TryAdd/TryRemove，也不会把 UI 禁用状态写回库存。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D3;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D22
{
    /// <summary>D3 库存 owner 的输入边界。</summary>
    [Serializable]
    public sealed class D22InventoryOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public int WorldDay;
        public long SaveRevision;
        public D3InventoryState Inventory;
        public D3TradeState TradeState;
        public TradeResult LastTradeResult;
        public D19MarketTradeResult LastPortTradeResult;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>库存条目的只读投影。</summary>
    [Serializable]
    public sealed class D22InventoryItemReadModel
    {
        public string Container { get; }
        public string ItemId { get; }
        public int Quantity { get; }
        public bool IsValidQuantity { get; }
        public string LocalizationKey { get; }
        public string DisabledReasonCode { get; }

        internal D22InventoryItemReadModel(string container, string itemId, int quantity)
        {
            Container = container ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Quantity = quantity;
            IsValidQuantity = quantity >= 0;
            LocalizationKey = D22ReadModelAdapterSupport.Key("inventory", "item");
            DisabledReasonCode = IsValidQuantity ? string.Empty : "d22.inventory.quantity_invalid";
        }

        private D22InventoryItemReadModel(
            string container,
            string itemId,
            int quantity,
            bool isValidQuantity,
            string localizationKey,
            string disabledReasonCode)
        {
            Container = container ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Quantity = quantity;
            IsValidQuantity = isValidQuantity;
            LocalizationKey = localizationKey ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
        }

        public D22InventoryItemReadModel Clone()
        {
            return new D22InventoryItemReadModel(
                Container, ItemId, Quantity, IsValidQuantity, LocalizationKey, DisabledReasonCode);
        }
    }

    /// <summary>D22-L20 库存 UI 读模型。</summary>
    [Serializable]
    public sealed class D22InventoryReadModel : D22ReadModelAdapterModel
    {
        public int CargoCapacity { get; }
        public int CargoWeightCapacity { get; }
        public int UsedCargo { get; }
        public int FreeCargo { get; }
        public int UsedWeight { get; }
        public int FreeWeight { get; }
        public bool HasMoney { get; }
        public int Money { get; }
        public IReadOnlyList<D22InventoryItemReadModel> Items { get; }
        public D22AdapterOutcomeReadModel LastOperation { get; }

        private D22InventoryReadModel(
            D22ReadModelAdapterMetadata metadata,
            int cargoCapacity,
            int cargoWeightCapacity,
            int usedCargo,
            int freeCargo,
            int usedWeight,
            int freeWeight,
            bool hasMoney,
            int money,
            IEnumerable<D22InventoryItemReadModel> items,
            D22AdapterOutcomeReadModel lastOperation)
            : base(metadata)
        {
            CargoCapacity = cargoCapacity;
            CargoWeightCapacity = cargoWeightCapacity;
            UsedCargo = usedCargo;
            FreeCargo = freeCargo;
            UsedWeight = usedWeight;
            FreeWeight = freeWeight;
            HasMoney = hasMoney;
            Money = money;
            var copied = new List<D22InventoryItemReadModel>();
            if (items != null)
                foreach (var item in items)
                    if (item != null) copied.Add(item.Clone());
            copied.Sort(CompareItems);
            Items = new ReadOnlyCollection<D22InventoryItemReadModel>(copied);
            LastOperation = lastOperation == null ? null : lastOperation.Clone();
        }

        public D22InventoryReadModel Clone()
        {
            return new D22InventoryReadModel(
                CopyMetadata(), CargoCapacity, CargoWeightCapacity, UsedCargo, FreeCargo,
                UsedWeight, FreeWeight, HasMoney, Money, Items, LastOperation);
        }

        public static D22InventoryReadModel Project(D22InventoryOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("d3.inventory");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.inventory.snapshot_missing", D22ReadModelAdapterSupport.Key("inventory", "blocked"),
                    "d22.inventory.snapshot_missing", "D3", "d3.inventory", true));
                return Empty(issues, missing);
            }
            if (snapshot.SchemaVersion != D22InventoryOwnerSnapshot.CurrentSchemaVersion)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.inventory.schema_version_mismatch", D22ReadModelAdapterSupport.Key("inventory", "invalid"),
                    "d22.inventory.schema_version_mismatch", "D22InventoryOwnerSnapshot", "d22.inventory", true));
            }
            if (snapshot.SourceSchemaVersion <= 0)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.inventory.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("inventory", "invalid"),
                    "d22.inventory.source_schema_version_invalid", "D3", "d3.inventory", true));
            }
            var inventory = snapshot.Inventory ?? (snapshot.TradeState == null ? null : snapshot.TradeState.Inventory);
            if (inventory == null)
            {
                missing.Add("d3.inventory");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_inventory_module", "d22.inventory.source_missing", D22ReadModelAdapterSupport.Key("inventory", "blocked"),
                    "d22.inventory.source_missing", "D3", "d3.inventory", true));
                return Empty(issues, MergeMissing(missing, snapshot.MissingModuleIds));
            }
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(snapshot.SaveRevision, snapshot.WorldDay, "D3", "d3.inventory", issues);
            if (inventory.CargoCapacity < 0 || inventory.CargoWeightCapacity < 0)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_capacity", "d22.inventory.capacity_invalid", D22ReadModelAdapterSupport.Key("inventory", "invalid"),
                    "d22.inventory.capacity_invalid", "D3", "d3.inventory", false));
            }

            var items = new List<D22InventoryItemReadModel>();
            AddItems("cargo", inventory.Cargo, items, issues);
            AddItems("personal", inventory.PersonalItems, items, issues);
            // Clone() is the owner convenience API, but malformed save input may
            // contain a null dictionary. Build a safe read-only calculation copy
            // without mutating the owner object or allowing a malformed source to
            // throw at the adapter boundary.
            var safeInventory = new D3InventoryState(inventory.CargoCapacity, inventory.CargoWeightCapacity);
            if (inventory.Cargo != null)
                foreach (var entry in inventory.Cargo)
                    safeInventory.Cargo[entry.Key] = entry.Value;
            if (inventory.PersonalItems != null)
                foreach (var entry in inventory.PersonalItems)
                    safeInventory.PersonalItems[entry.Key] = entry.Value;
            int usedCargo = D3InventorySystem.UsedCargo(safeInventory);
            int usedWeight = D3InventorySystem.UsedWeight(safeInventory);
            int freeCargo = D3InventorySystem.FreeCargo(safeInventory);
            int freeWeight = D3InventorySystem.FreeWeight(safeInventory);

            bool hasMoney = snapshot.TradeState != null;
            int money = hasMoney ? snapshot.TradeState.Money : 0;
            if (!hasMoney)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_money_projection", "d22.inventory.money_missing", D22ReadModelAdapterSupport.Key("inventory", "partial"),
                    "d22.inventory.money_missing", "D3", "d3.trade", false));
            }

            D22AdapterOutcomeReadModel operation = null;
            string sourceError = string.Empty;
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (snapshot.LastPortTradeResult != null)
            {
                var result = snapshot.LastPortTradeResult;
                sourceError = !string.IsNullOrEmpty(result.ReasonCode) ? result.ReasonCode : result.OriginalD3Code;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? result.ReasonCode : sourceReason;
                sourceLoc = string.IsNullOrEmpty(sourceLoc) ? result.LocKey : sourceLoc;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D19", "d19.market", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D19", "d19.market", issues);
                operation = D22ReadModelAdapterSupport.Outcome(
                    "inventory", "D19", true, result.Success, result.DuplicateSettlement,
                    sourceError, sourceReason, sourceLoc,
                    result.Success ? string.Empty :
                        (string.IsNullOrEmpty(sourceReason) ? "d22.inventory.operation_failed" : sourceReason), issues);
            }
            else if (snapshot.LastTradeResult != null)
            {
                var result = snapshot.LastTradeResult;
                sourceError = result.Code ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "D3", "d3.trade", issues);
                operation = D22ReadModelAdapterSupport.Outcome(
                    "inventory", "D3", true, result.Succeeded, result.DuplicateSettlement,
                    sourceError, sourceReason, sourceLoc,
                    result.Succeeded ? string.Empty :
                        (string.IsNullOrEmpty(sourceReason) ? "d22.inventory.operation_failed" : sourceReason), issues);
            }
            else if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22InventoryOwnerSnapshot", "d22.inventory", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22InventoryOwnerSnapshot", "d22.inventory", issues);
                operation = D22ReadModelAdapterSupport.Outcome(
                    "inventory", "D22InventoryOwnerSnapshot", true, false, false,
                    sourceReason, sourceReason, sourceLoc, sourceReason, issues);
            }

            var status = D22ReadModelAdapterSupport.ResolveStatus(
                snapshot.SchemaVersion != D22InventoryOwnerSnapshot.CurrentSchemaVersion,
                false,
                issues.Count > 0,
                issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "inventory", "D3", snapshot.SourceSchemaVersion, snapshot.SaveRevision, snapshot.WorldDay, status,
                sourceError, sourceError, sourceReason,
                string.IsNullOrEmpty(sourceReason) ? string.Empty : sourceReason,
                sourceLoc, MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22InventoryReadModel(
                metadata,
                inventory.CargoCapacity,
                inventory.CargoWeightCapacity,
                usedCargo,
                freeCargo,
                usedWeight,
                freeWeight,
                hasMoney,
                money,
                items,
                operation);
        }

        public static D22InventoryReadModel Adapt(D22InventoryOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22InventoryReadModel FromSnapshot(D22InventoryOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22InventoryReadModel Empty(
            List<D22ReadModelAdapterIssue> issues,
            IEnumerable<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "inventory", "D3", 0, 0, 0, D22ReadModelAdapterStatus.Blocked,
                "d22.inventory.source_missing", string.Empty, "d22.inventory.source_missing",
                "d22.inventory.source_missing", string.Empty, missing, issues);
            return new D22InventoryReadModel(
                metadata, 0, 0, 0, 0, 0, 0, false, 0,
                new List<D22InventoryItemReadModel>(), null);
        }

        private static void AddItems(
            string container,
            IDictionary<string, int> source,
            List<D22InventoryItemReadModel> target,
            List<D22ReadModelAdapterIssue> issues)
        {
            if (source == null)
            {
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_inventory_collection", "d22.inventory.collection_missing", D22ReadModelAdapterSupport.Key("inventory", "partial"),
                    "d22.inventory.collection_missing", "D3", "d3.inventory", false));
                return;
            }
            foreach (var pair in source)
            {
                if (!D22ReadModelAdapterSupport.IsStableId(pair.Key))
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_inventory_item_id", "d22.inventory.item_id_invalid", D22ReadModelAdapterSupport.Key("inventory", "partial"),
                        "d22.inventory.item_id_invalid", "D3", "d3.inventory", false));
                    continue;
                }
                target.Add(new D22InventoryItemReadModel(container, pair.Key, pair.Value));
                if (pair.Value < 0)
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_inventory_quantity", "d22.inventory.quantity_invalid", D22ReadModelAdapterSupport.Key("inventory", "invalid"),
                        "d22.inventory.quantity_invalid", "D3", "d3.inventory", false));
                }
            }
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static int CompareItems(D22InventoryItemReadModel left, D22InventoryItemReadModel right)
        {
            int result = D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.Container,
                right == null ? null : right.Container);
            if (result != 0) return result;
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.ItemId,
                right == null ? null : right.ItemId);
        }
    }
}
