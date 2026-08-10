// D10AppearanceSystem —— 服装组合、裁缝服务和伤病外观的唯一写者。
//
// UI/Agent 只能提交 item ID、染料 ID 或服务请求；本类验证内容、文化/职业/天气
// 标签、库存所有权和价格，并通过 EconomySystem 完成扣款。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public sealed class D10AppearanceSystem
    {
        public const string OwnerSystem = "AppearanceSystem";
        public const string PurchaseEventId = "event_d10_appearance_purchased";
        public const string TryOnEventId = "event_d10_appearance_try_on";
        public const string DyeEventId = "event_d10_appearance_dyed";
        public const string RepairEventId = "event_d10_appearance_repaired";
        public const int DyeServiceCost = 6;

        private readonly TradeEconomySystem _economy;
        private readonly D3TradeState _trade;
        private readonly EventLog _eventLog;
        private readonly Func<string, bool> _resourceExists;
        private readonly string _locationRef;
        private readonly int _worldDay;
        private D10AppearanceSnapshot _snapshot;

        public D10AppearanceSystem(D10AppearanceSnapshot snapshot = null,
            TradeEconomySystem economy = null, D3TradeState trade = null,
            Func<string, bool> resourceExists = null, int worldDay = 0,
            string locationRef = "santa_margarita")
        {
            _economy = economy ?? new TradeEconomySystem();
            _trade = trade;
            _eventLog = _economy.EventLog;
            _resourceExists = resourceExists ?? delegate { return false; };
            _worldDay = Math.Max(0, worldDay);
            _locationRef = string.IsNullOrEmpty(locationRef) ? "santa_margarita" : locationRef;
            _snapshot = Normalize(snapshot);
        }

        public D10AppearanceSnapshot Snapshot { get { return _snapshot.Clone(); } }
        public D10AppearanceContext Context { get { return _snapshot.Context.Clone(); } }

        public void SetContext(D10AppearanceContext context)
        {
            if (context == null) return;
            _snapshot.Context = context.Clone();
        }

        public D10AppearanceItemDefinition Find(string itemId)
        {
            return D10AppearanceCatalog.Find(itemId);
        }

        public D10AppearanceOperationResult TryOn(string itemId)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            var validation = ValidateDefinition(definition, itemId);
            if (validation != null) return validation;
            var contextResult = ValidateContext(definition);
            if (contextResult != null) return contextResult;
            var composition = ValidateComposition(definition, _snapshot.ActiveLoadout);
            if (composition != null) return composition;

            var preview = _snapshot.ActiveLoadout.Clone();
            preview.Set(definition.Slot, definition.Id);
            var result = ResourceResult(true, D10AppearanceResultCode.Success,
                "试穿预览已生成，尚未改变所有权或已装备外观。", definition, preview);
            result.Fact = AppendFact(TryOnEventId, definition.Id, "appearance_try_on", result.Message, 0);
            return result;
        }

        public D10AppearanceOperationResult Purchase(string itemId)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            var validation = ValidateDefinition(definition, itemId);
            if (validation != null) return validation;
            if (_snapshot.Owned.ContainsKey(itemId))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.AlreadyOwned,
                    "该外观已经属于玩家：" + itemId, itemId);
            var contextResult = ValidateContext(definition);
            if (contextResult != null) return contextResult;
            if (_trade == null)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InsufficientFunds,
                    "裁缝服务尚未绑定 EconomySystem，不能结算购买：" + itemId, itemId);

            string error;
            if (!_economy.TrySpend(_trade, Math.Max(0, definition.Price), out error))
                return D10AppearanceOperationResult.Failed(
                    error == "not_enough_money" ? D10AppearanceResultCode.InsufficientFunds : D10AppearanceResultCode.InvalidContext,
                    "购买失败（" + error + "）：" + itemId, itemId);

            _snapshot.Owned[itemId] = new D10OwnedAppearance
            {
                ItemId = itemId,
                Durability = definition.MaxDurability,
                PurchaseCount = 1,
            };
            _snapshot.SaveRevision++;
            var result = ResourceResult(true, D10AppearanceResultCode.Success,
                "裁缝购买完成，所有权已写入 AppearanceSystem。", definition, _snapshot.ActiveLoadout.Clone());
            result.Cost = definition.Price;
            result.Fact = AppendFact(PurchaseEventId, itemId, "appearance_purchased", result.Message, result.Cost);
            return result;
        }

        public D10AppearanceOperationResult Equip(string itemId)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            var validation = ValidateDefinition(definition, itemId);
            if (validation != null) return validation;
            if (definition.Slot == D10AppearanceSlot.Injury || definition.IsInjuryOverlay)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InjuryProtected,
                    "伤病表现只能由伤病状态入口写入，不能被普通换装清除。", itemId);
            D10OwnedAppearance owned;
            if (!_snapshot.Owned.TryGetValue(itemId, out owned))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.NotOwned,
                    "未拥有该外观，先购买或从存档恢复所有权：" + itemId, itemId);
            var contextResult = ValidateContext(definition);
            if (contextResult != null) return contextResult;
            var composition = ValidateComposition(definition, _snapshot.ActiveLoadout);
            if (composition != null) return composition;
            _snapshot.ActiveLoadout.Set(definition.Slot, itemId);
            return ResourceResult(true, D10AppearanceResultCode.Success,
                "外观已装备：" + itemId, definition, _snapshot.ActiveLoadout.Clone());
        }

        public D10AppearanceOperationResult Dye(string itemId, string dyeId, string dyeColor)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            var validation = ValidateDefinition(definition, itemId);
            if (validation != null) return validation;
            if (!definition.Dyeable)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.NotDyeable,
                    "该外观没有登记可染色属性：" + itemId, itemId);
            if (!D10AppearanceCatalog.IsValidDye(dyeId))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InvalidDye,
                    "染料未登记或不使用 item_dye_ 前缀：" + (dyeId ?? ""), itemId);
            D10OwnedAppearance owned;
            if (!_snapshot.Owned.TryGetValue(itemId, out owned))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.NotOwned,
                    "未拥有该外观，不能染色：" + itemId, itemId);
            if (_trade == null)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InsufficientFunds,
                    "裁缝服务尚未绑定 EconomySystem，不能结算染色：" + itemId, itemId);
            string error;
            if (!_economy.TrySpend(_trade, DyeServiceCost, out error))
                return D10AppearanceOperationResult.Failed(
                    error == "not_enough_money" ? D10AppearanceResultCode.InsufficientFunds : D10AppearanceResultCode.InvalidContext,
                    "染色失败（" + error + "）：" + itemId, itemId);
            owned.DyeId = dyeId;
            owned.DyeColor = dyeColor ?? "";
            _snapshot.SaveRevision++;
            var result = ResourceResult(true, D10AppearanceResultCode.Success,
                "染色完成，原始外观历史仍保留。", definition, _snapshot.ActiveLoadout.Clone());
            result.Cost = DyeServiceCost;
            result.Fact = AppendFact(DyeEventId, itemId, "appearance_dyed", result.Message, DyeServiceCost);
            return result;
        }

        public D10AppearanceOperationResult Repair(string itemId)
        {
            var definition = D10AppearanceCatalog.Find(itemId);
            var validation = ValidateDefinition(definition, itemId);
            if (validation != null) return validation;
            D10OwnedAppearance owned;
            if (!_snapshot.Owned.TryGetValue(itemId, out owned))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.NotOwned,
                    "未拥有该外观，不能维修：" + itemId, itemId);
            if (owned.Durability >= definition.MaxDurability)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.AlreadyRepaired,
                    "该外观耐久已经满值：" + itemId, itemId);
            if (_trade == null)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InsufficientFunds,
                    "裁缝服务尚未绑定 EconomySystem，不能结算维修：" + itemId, itemId);

            string error;
            if (!_economy.TrySpend(_trade, Math.Max(0, definition.RepairCost), out error))
                return D10AppearanceOperationResult.Failed(
                    error == "not_enough_money" ? D10AppearanceResultCode.InsufficientFunds : D10AppearanceResultCode.InvalidContext,
                    "维修失败（" + error + "）：" + itemId, itemId);
            owned.Durability = definition.MaxDurability;
            _snapshot.SaveRevision++;
            var result = ResourceResult(true, D10AppearanceResultCode.Success,
                "维修完成，耐久已由 AppearanceSystem 恢复。", definition, _snapshot.ActiveLoadout.Clone());
            result.Cost = definition.RepairCost;
            result.Fact = AppendFact(RepairEventId, itemId, "appearance_repaired", result.Message, result.Cost);
            return result;
        }

        public D10AppearanceOperationResult ApplyInjuryAppearance(IEnumerable<string> injuryIds)
        {
            var next = new List<string>();
            if (injuryIds != null)
                foreach (var injuryId in injuryIds)
                    if (!string.IsNullOrEmpty(injuryId) && !next.Contains(injuryId)) next.Add(injuryId);
            _snapshot.ActiveInjuryIds = next;
            foreach (var injuryId in next)
            {
                if (!_snapshot.InjuryHistoryIds.Contains(injuryId)) _snapshot.InjuryHistoryIds.Add(injuryId);
                var overlay = FindInjuryOverlay(injuryId);
                if (overlay != null) _snapshot.ActiveLoadout.Set(D10AppearanceSlot.Injury, overlay.Id);
            }
            if (next.Count == 0) _snapshot.ActiveLoadout.Set(D10AppearanceSlot.Injury, "");
            return new D10AppearanceOperationResult
            {
                Success = true,
                Code = D10AppearanceResultCode.Success,
                Message = next.Count == 0 ? "当前没有活动伤病；历史表现仍保留。" : "伤病表现已更新；历史表现未清除。",
                PreviewLoadout = _snapshot.ActiveLoadout.Clone(),
            };
        }

        private D10AppearanceSnapshot Normalize(D10AppearanceSnapshot snapshot)
        {
            if (snapshot == null) return D10AppearanceCatalog.CreateStarterSnapshot();
            if (snapshot.Context == null) snapshot.Context = new D10AppearanceContext();
            if (snapshot.ActiveLoadout == null) snapshot.ActiveLoadout = new D10AppearanceLoadout();
            if (snapshot.Owned == null) snapshot.Owned = new Dictionary<string, D10OwnedAppearance>(StringComparer.Ordinal);
            if (snapshot.ActiveInjuryIds == null) snapshot.ActiveInjuryIds = new List<string>();
            if (snapshot.InjuryHistoryIds == null) snapshot.InjuryHistoryIds = new List<string>();
            if (snapshot.ActiveLoadout.Slots.Count == 0)
            {
                var starter = D10AppearanceCatalog.CreateStarterSnapshot();
                snapshot.ActiveLoadout = starter.ActiveLoadout;
                foreach (var pair in starter.Owned)
                    if (!snapshot.Owned.ContainsKey(pair.Key)) snapshot.Owned[pair.Key] = pair.Value;
            }
            return snapshot.Clone();
        }

        private D10AppearanceOperationResult ValidateDefinition(D10AppearanceItemDefinition definition, string itemId)
        {
            if (definition != null) return null;
            return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InvalidItemId,
                "内容目录中不存在外观 ID：" + (itemId ?? ""), itemId);
        }

        private D10AppearanceOperationResult ValidateContext(D10AppearanceItemDefinition definition)
        {
            if (definition.CultureTags.Count > 0 && !definition.CultureTags.Contains(_snapshot.Context.CultureId))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.CultureRestricted,
                    "文化标签不允许当前身份使用：" + definition.Id, definition.Id);
            if (definition.CareerTags.Count > 0 && !definition.CareerTags.Contains(_snapshot.Context.CareerId))
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.CareerRestricted,
                    "职业标签不允许当前身份使用：" + definition.Id, definition.Id);
            if (definition.WeatherTags.Count > 0 && !definition.WeatherTags.Contains(_snapshot.Context.WeatherId) &&
                definition.Slot == D10AppearanceSlot.CareerEquipment)
                return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.WeatherRestricted,
                    "当前天气与职业装备标签不匹配：" + definition.Id, definition.Id);
            return null;
        }

        private D10AppearanceOperationResult ValidateComposition(D10AppearanceItemDefinition definition,
            D10AppearanceLoadout loadout)
        {
            foreach (var pair in loadout.Slots)
            {
                if (pair.Value == definition.Id || string.IsNullOrEmpty(pair.Value)) continue;
                var active = D10AppearanceCatalog.Find(pair.Value);
                if (active == null) continue;
                if (!string.IsNullOrEmpty(definition.ExclusiveGroup) &&
                    definition.ExclusiveGroup == active.ExclusiveGroup && definition.Slot != active.Slot)
                    return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.SlotConflict,
                        "外观层冲突：" + definition.Id + " 与 " + active.Id, definition.Id);
                if (definition.ExcludesItemIds.Contains(active.Id) || active.ExcludesItemIds.Contains(definition.Id))
                    return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.SlotConflict,
                        "内容目录声明的互斥组合：" + definition.Id + " 与 " + active.Id, definition.Id);
            }
            return null;
        }

        private D10AppearanceItemDefinition FindInjuryOverlay(string injuryId)
        {
            if (string.IsNullOrEmpty(injuryId)) return null;
            if (injuryId.IndexOf("leg", StringComparison.OrdinalIgnoreCase) >= 0)
                return D10AppearanceCatalog.Find("outfit_injury_old_leg");
            return D10AppearanceCatalog.Find("outfit_injury_bandaged_arm");
        }

        private D10AppearanceOperationResult ResourceResult(bool success, D10AppearanceResultCode code,
            string message, D10AppearanceItemDefinition definition, D10AppearanceLoadout preview)
        {
            bool ready = _resourceExists(definition.AssetId);
            return new D10AppearanceOperationResult
            {
                Success = success,
                Code = ready ? code : D10AppearanceResultCode.MissingResourcePlaceholder,
                Message = ready ? message : message + " 资源缺失，使用明确的占位表现：" + definition.AssetId,
                ItemId = definition.Id,
                AssetId = definition.AssetId,
                ResourceStatus = ready ? "ready" : "placeholder_missing_resource",
                UsesPlaceholder = !ready,
                PreviewLoadout = preview,
            };
        }

        private FactEvent AppendFact(string eventId, string itemId, string resultCode, string summary, int cost)
        {
            var revision = ++_snapshot.SaveRevision;
            return _eventLog.Append(new WorldEvent(
                eventId, _worldDay, _locationRef, OwnerSystem,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = summary,
                    FactIds = new List<string> { itemId },
                    Values = new Dictionary<string, string>
                    {
                        ["item_id"] = itemId ?? "",
                        ["cost"] = cost.ToString(),
                        ["resource_status"] = _resourceExists(D10AppearanceCatalog.Find(itemId).AssetId)
                            ? "ready" : "placeholder_missing_resource",
                    },
                }, new[] { "player" }, new string[0], new string[0], revision)).Fact;
        }
    }
}
