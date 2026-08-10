// D14-A03 特殊武器/船体模块运行时编排器。
// 该类不直接生成物品、不计算伤害或修理值；每个结果都必须来自带来源事实的
// Reward/Inventory/Ship/Combat/Economy/Legal authority。

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public sealed class D14SpecialEquipmentSystem
    {
        private readonly EventLog _eventLog;
        private readonly D14SpecialEquipmentCatalog _catalog;
        private readonly ID14EquipmentRewardAuthority _reward;
        private readonly ID14EquipmentShipAuthority _ship;
        private readonly ID14EquipmentCombatAuthority _combat;
        private readonly ID14EquipmentTradeAuthority _trade;
        private readonly ID14EquipmentLegalAuthority _legal;
        private readonly D14SpecialEquipmentRuntimeSnapshot _state;
        private readonly HashSet<string> _appliedEventKeys = new HashSet<string>(StringComparer.Ordinal);

        public D14SpecialEquipmentSystem(
            EventLog eventLog = null,
            D14SpecialEquipmentCatalog catalog = null,
            ID14EquipmentRewardAuthority reward = null,
            ID14EquipmentShipAuthority ship = null,
            ID14EquipmentCombatAuthority combat = null,
            ID14EquipmentTradeAuthority trade = null,
            ID14EquipmentLegalAuthority legal = null,
            D14SpecialEquipmentRuntimeSnapshot initial = null)
        {
            if (initial != null && initial.EventLog != null && eventLog == null)
                _eventLog = EventLog.FromSnapshot(initial.EventLog);
            else
                _eventLog = eventLog ?? new EventLog();
            _catalog = catalog ?? new D14SpecialEquipmentCatalog();
            _reward = reward ?? new UnavailableRewardAuthority();
            _ship = ship ?? new UnavailableShipAuthority();
            _combat = combat ?? new UnavailableCombatAuthority();
            _trade = trade ?? new UnavailableTradeAuthority();
            _legal = legal ?? new UnavailableLegalAuthority();
            _state = initial == null ? new D14SpecialEquipmentRuntimeSnapshot() : initial.Clone();
            if (initial != null)
            {
                initial.Validate();
                if (initial.AppliedEventKeys != null) foreach (var key in initial.AppliedEventKeys) _appliedEventKeys.Add(key);
            }
        }

        public D14SpecialEquipmentRuntimeSnapshot Snapshot { get { return CaptureSnapshot(); } }
        public EventLog SharedEventLog { get { return _eventLog; } }
        public IReadOnlyList<FactEvent> EquipmentLog { get { return _eventLog.ReadFactsForUi(int.MaxValue); } }

        public D14EquipmentInstance Find(string instanceId)
        {
            var item = FindMutable(instanceId);
            return item == null ? null : item.Clone();
        }

        public D14SpecialEquipmentDefinition FindDefinition(string definitionId) { return _catalog.Find(definitionId); }

        public D14SpecialEquipmentRuntimeSnapshot CaptureSnapshot()
        {
            _state.AppliedEventKeys.Clear();
            _state.AppliedEventKeys.AddRange(_appliedEventKeys);
            _state.EventLog = _eventLog.CaptureSnapshot();
            _state.Validate();
            return _state.Clone();
        }

        public D14EquipmentResult Acquire(D14EquipmentAcquireRequest request)
        {
            if (!ValidRequest(request)) return Reject("invalid_request", "特殊装备获取请求缺少 operation_id。", null);
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) return duplicate;
            var definition = _catalog.Find(request.DefinitionId);
            if (definition == null) return Reject("definition_missing", "特殊装备目录中没有该 definition_id。", null);
            if (string.IsNullOrWhiteSpace(request.SourceEventId) || !HasOwnedFact(request.SourceEventId, definition.AcquisitionOwnerSystem))
                return Reject("source_missing", "装备获取必须引用所属系统已登记的来源事实。", null);
            if (definition.RequiresAuthorization)
            {
                if (string.IsNullOrWhiteSpace(request.AuthorizationEventId) || !HasOwnedFact(request.AuthorizationEventId, definition.AuthorizationOwnerSystem))
                    return Reject("authorization_missing", "该装备缺少有效授权事实，不能获取。", null);
            }
            if (RequiresHistoricalReview(definition))
            {
                if (definition.ReviewStatus == D14EquipmentReviewStatus.ApprovedWithWarning && !request.HistoricalWarningAcknowledged)
                    return Reject("historical_warning_unacknowledged", "历史武器的内容警告必须确认后才能获取。", null);
                if (string.IsNullOrWhiteSpace(request.HistoricalReviewEventId) || !HasOwnedFact(request.HistoricalReviewEventId, D14SpecialEquipmentIds.HistoricalReviewOwner))
                    return Reject("historical_review_missing", "历史武器必须引用已完成的历史审查事实。", null);
            }
            var expectedInstanceId = D14SpecialEquipmentIds.CreateInstanceId(definition.DefinitionId, request.SourceEventId);
            if (string.IsNullOrWhiteSpace(expectedInstanceId)) return Reject("instance_id_invalid", "装备实例 ID 无法由定义和来源稳定生成。", null);
            if (FindMutable(expectedInstanceId) != null) return Reject("already_acquired", "同一来源已经生成过该唯一装备。", FindMutable(expectedInstanceId));
            var sourceFact = FindFact(request.SourceEventId);
            var authorizationFact = string.IsNullOrWhiteSpace(request.AuthorizationEventId) ? null : FindFact(request.AuthorizationEventId);
            var decision = _reward.Acquire(request, definition, sourceFact, authorizationFact);
            if (!ValidAcquireDecision(decision, request, definition, expectedInstanceId))
                return Reject(decision == null ? "reward_authority_required" : decision.Code, decision == null ? "RewardSystem 未返回装备获取决定。" : decision.Message, null);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, null);

            var result = Commit(request, "equipment_acquired", "特殊装备已由所属奖励/收藏系统授权获取。", "equipment_acquired",
                new Dictionary<string, string>
                {
                    ["instance_id"] = expectedInstanceId, ["definition_id"] = definition.DefinitionId,
                    ["source_event_id"] = decision.SourceEventId, ["authorization_event_id"] = request.AuthorizationEventId ?? "",
                    ["historical_review_event_id"] = request.HistoricalReviewEventId ?? "",
                    ["strength_cap"] = definition.StrengthCap.ToString(CultureInfo.InvariantCulture),
                    ["strength"] = decision.StrengthAfter.ToString(CultureInfo.InvariantCulture),
                    ["durability"] = decision.DurabilityAfter.ToString(CultureInfo.InvariantCulture),
                }, definition.DefinitionId, expectedInstanceId);
            var equipment = new D14EquipmentInstance
            {
                InstanceId = expectedInstanceId, DefinitionId = definition.DefinitionId, OwnerRef = request.OwnerRef,
                Slot = definition.Slot, Status = D14EquipmentStatus.Acquired, LegalStatus = decision.LegalStatus,
                Strength = decision.StrengthAfter, Durability = decision.DurabilityAfter,
                AcquisitionSourceEventId = decision.SourceEventId, AuthorizationEventId = request.AuthorizationEventId,
                HistoricalReviewEventId = request.HistoricalReviewEventId, InventorySourceEventId = decision.InventorySourceEventId,
                LastEventId = result.EventId,
            };
            _state.Equipment.Add(equipment);
            result.Equipment = equipment.Clone();
            result.Operation = AddOperation(request, D14EquipmentOperationKind.Acquisition, equipment, decision, 0, 0, result.EventId);
            result.State = CaptureSnapshot();
            return result;
        }

        public D14EquipmentResult Install(D14EquipmentInstallRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status == D14EquipmentStatus.Sold || equipment.Status == D14EquipmentStatus.Seized || equipment.Status == D14EquipmentStatus.Lost)
                return Reject("equipment_unavailable", "已出售、收缴或遗失的装备不能装配。", equipment);
            if (string.IsNullOrWhiteSpace(request.ShipId) || string.IsNullOrWhiteSpace(request.ShipTypeId) || !definition.AllowsShip(request.ShipTypeId))
                return Reject("ship_compatibility_failed", "该装备与当前船型不兼容。", equipment);
            if (request.RequestedSlot != D14EquipmentSlot.None && request.RequestedSlot != definition.Slot)
                return Reject("slot_mismatch", "请求槽位与目录定义不一致。", equipment);
            var decision = _ship.Install(request, equipment.Clone(), definition);
            if (!ValidShipDecision(decision, D14EquipmentStatus.Installed, definition, equipment, true))
                return Reject(decision == null ? "ship_authority_required" : decision.Code, decision == null ? "ShipSystem 未返回装配决定。" : decision.Message, equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            var result = Commit(request, "equipment_installed", "装备装配结果已由 ShipSystem/CombatSystem 裁决。", "equipment_installed",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = D14EquipmentStatus.Installed; equipment.ShipId = request.ShipId; equipment.Slot = definition.Slot;
            equipment.Strength = decision.StrengthAfter; equipment.Durability = decision.DurabilityAfter; equipment.LastEventId = result.EventId;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Installation, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        public D14EquipmentResult Maintain(D14EquipmentMaintainRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status == D14EquipmentStatus.Sold || equipment.Status == D14EquipmentStatus.Seized || equipment.Status == D14EquipmentStatus.Lost)
                return Reject("equipment_unavailable", "已出售、收缴或遗失的装备不能维修。", equipment);
            if (string.IsNullOrWhiteSpace(request.ShipId) || string.IsNullOrWhiteSpace(request.RepairMaterialEventId) || !HasOwnedFact(request.RepairMaterialEventId, D14SpecialEquipmentIds.InventoryOwner))
                return Reject("maintenance_source_missing", "维修必须引用 Ship/Inventory 的已登记事实。", equipment);
            var decision = _ship.Maintain(request, equipment.Clone(), definition);
            if (!ValidShipDecision(decision, decision == null ? D14EquipmentStatus.Damaged : decision.Status, definition, equipment, false))
                return Reject(decision == null ? "ship_authority_required" : decision.Code, decision == null ? "ShipSystem 未返回维护决定。" : decision.Message, equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            if (string.IsNullOrWhiteSpace(decision.InventorySourceEventId) || !HasOwnedFact(decision.InventorySourceEventId, D14SpecialEquipmentIds.InventoryOwner))
                return Reject("inventory_source_missing", "维护必须引用 InventorySystem 的材料事实。", equipment);
            if (decision.DurabilityAfter < equipment.Durability) return Reject("maintenance_regressed", "维护决定不能降低耐久。", equipment);
            var result = Commit(request, "equipment_maintained", "装备维护结果已由 ShipSystem/ShipyardSystem 裁决。", "equipment_maintained",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = decision.DurabilityAfter >= definition.MaxDurability ? D14EquipmentStatus.Maintained : D14EquipmentStatus.Damaged;
            equipment.Strength = decision.StrengthAfter; equipment.Durability = decision.DurabilityAfter; equipment.LastEventId = result.EventId;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Maintenance, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        public D14EquipmentResult Damage(D14EquipmentDamageRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status == D14EquipmentStatus.Sold || equipment.Status == D14EquipmentStatus.Seized || equipment.Status == D14EquipmentStatus.Lost)
                return Reject("equipment_unavailable", "当前装备状态不能接受战斗损坏。", equipment);
            if (string.IsNullOrWhiteSpace(request.CombatSourceEventId) || !HasOwnedFact(request.CombatSourceEventId, D14SpecialEquipmentIds.CombatOwner))
                return Reject("combat_source_missing", "装备损坏必须来自 CombatSystem 事实。", equipment);
            var decision = _combat.ApplyDamage(request, equipment.Clone(), definition, FindFact(request.CombatSourceEventId));
            if (!ValidOwnerDecision(decision, D14SpecialEquipmentIds.CombatOwner) || !SameInstance(decision, equipment))
                return Reject(decision == null ? "combat_authority_required" : decision.Code, decision == null ? "CombatSystem 未返回装备损坏决定。" : decision.Message, equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            if (decision.DurabilityAfter > equipment.Durability || decision.DurabilityAfter < 0 || decision.StrengthAfter < 0 || decision.StrengthAfter > definition.StrengthCap)
                return Reject("damage_boundary_failed", "CombatSystem 返回的损坏结果越过耐久或强度边界。", equipment);
            var result = Commit(request, "equipment_damaged", "装备损坏结果已由 CombatSystem 裁决。", "equipment_damaged",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = decision.Status == D14EquipmentStatus.Lost
                ? D14EquipmentStatus.Lost
                : (decision.DurabilityAfter < definition.MaxDurability ? D14EquipmentStatus.Damaged : D14EquipmentStatus.Maintained);
            equipment.Strength = decision.StrengthAfter; equipment.Durability = decision.DurabilityAfter; equipment.LastEventId = result.EventId;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Damage, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        public D14EquipmentResult Sell(D14EquipmentSellRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status == D14EquipmentStatus.Sold || equipment.Status == D14EquipmentStatus.Seized || equipment.Status == D14EquipmentStatus.Lost)
                return Reject("equipment_unavailable", "当前装备状态不能出售。", equipment);
            if (string.IsNullOrWhiteSpace(request.SellerRef) || string.IsNullOrWhiteSpace(request.LegalSourceEventId) || !HasOwnedFact(request.LegalSourceEventId, D14SpecialEquipmentIds.LegalOwner))
                return Reject("sale_legal_source_missing", "出售必须有卖方和法律/授权来源事实。", equipment);
            var decision = _trade.Sell(request, equipment.Clone(), definition, FindFact(request.LegalSourceEventId));
            if (!ValidOwnerDecision(decision, D14SpecialEquipmentIds.EconomyOwner) || !SameInstance(decision, equipment) || string.IsNullOrWhiteSpace(decision.InventorySourceEventId) || !HasOwnedFact(decision.InventorySourceEventId, D14SpecialEquipmentIds.InventoryOwner))
                return Reject(decision == null ? "trade_authority_required" : decision.Code, decision == null ? "Economy/Inventory 未返回出售决定。" : decision.Message, equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            if (string.IsNullOrWhiteSpace(decision.EconomySourceEventId) || !HasOwnedFact(decision.EconomySourceEventId, D14SpecialEquipmentIds.EconomyOwner))
                return Reject("economy_source_missing", "出售必须引用 EconomySystem 的价格/账本事实。", equipment);
            if (equipment.Status == D14EquipmentStatus.Installed && (string.IsNullOrWhiteSpace(decision.ShipSourceEventId) || !HasOwnedFact(decision.ShipSourceEventId, D14SpecialEquipmentIds.ShipOwner)))
                return Reject("installed_remove_source_missing", "出售已装配装备前必须有 ShipSystem 的拆卸事实。", equipment);
            var result = Commit(request, "equipment_sold", "装备出售、库存出库和价格由所属系统裁决。", "equipment_sold",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = D14EquipmentStatus.Sold; equipment.ShipId = null; equipment.LastEventId = result.EventId;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Sale, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        public D14EquipmentResult Seize(D14EquipmentSeizeRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status == D14EquipmentStatus.Sold || equipment.Status == D14EquipmentStatus.Seized)
                return Reject("equipment_unavailable", "该装备已经出售或收缴。", equipment);
            if (string.IsNullOrWhiteSpace(request.LegalSourceEventId) || !HasOwnedFact(request.LegalSourceEventId, D14SpecialEquipmentIds.LegalOwner))
                return Reject("legal_source_missing", "收缴必须引用 FactionLegalSystem 事实。", equipment);
            var decision = _legal.Seize(request, equipment.Clone(), definition, FindFact(request.LegalSourceEventId));
            if (!ValidOwnerDecision(decision, D14SpecialEquipmentIds.LegalOwner) || !SameInstance(decision, equipment))
                return Reject(decision == null ? "legal_authority_required" : decision.Code, decision == null ? "FactionLegalSystem 未返回收缴决定。" : decision.Message, equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            if (string.IsNullOrWhiteSpace(decision.LegalSourceEventId) || !HasOwnedFact(decision.LegalSourceEventId, D14SpecialEquipmentIds.LegalOwner))
                return Reject("legal_source_missing", "收缴结果必须引用 FactionLegalSystem 的证据事实。", equipment);
            var result = Commit(request, "equipment_seized", "装备收缴和证据状态已由 FactionLegalSystem 裁决。", "equipment_seized",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = D14EquipmentStatus.Seized;
            equipment.LegalStatus = decision.LegalStatus == D14EquipmentLegalStatus.Legal ? D14EquipmentLegalStatus.Seized : decision.LegalStatus;
            equipment.ShipId = null; equipment.LastEventId = result.EventId;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Seizure, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        public D14EquipmentResult Recover(D14EquipmentRecoverRequest request)
        {
            var equipment = FindMutable(request == null ? null : request.InstanceId);
            var definition = equipment == null ? null : _catalog.Find(equipment.DefinitionId);
            if (!PrepareOperation(request, equipment, definition, out D14EquipmentResult failure)) return failure;
            if (equipment.Status != D14EquipmentStatus.Lost)
                return Reject("recovery_not_allowed", "只有已记录为遗失的装备可以走恢复路线。", equipment);
            if (string.IsNullOrWhiteSpace(request.RecoverySourceEventId) || !HasFact(request.RecoverySourceEventId))
                return Reject("recovery_source_missing", "恢复必须引用已登记的恢复来源事实。", equipment);
            if (definition.RequiresAuthorization && (string.IsNullOrWhiteSpace(request.AuthorizationEventId) || !HasOwnedFact(request.AuthorizationEventId, definition.AuthorizationOwnerSystem)))
                return Reject("authorization_missing", "恢复仍需要有效授权，不能绕过法律或军械登记。", equipment);
            var decision = _reward.Recover(request, equipment.Clone(), definition, FindFact(request.RecoverySourceEventId));
            if (!ValidOwnerDecision(decision, D14SpecialEquipmentIds.RewardOwner) && !ValidOwnerDecision(decision, D14SpecialEquipmentIds.ItemOwner))
                return Reject(decision == null ? "recovery_authority_required" : decision.Code, decision == null ? "Reward/Item authority 未返回恢复决定。" : decision.Message, equipment);
            if (!SameInstance(decision, equipment)) return Reject("instance_id_mismatch", "恢复不能改变原装备唯一 ID。", equipment);
            if (!decision.Allowed) return Reject(decision.Code, decision.Message, equipment);
            if (string.IsNullOrWhiteSpace(decision.InventorySourceEventId) || !HasOwnedFact(decision.InventorySourceEventId, D14SpecialEquipmentIds.InventoryOwner))
                return Reject("inventory_source_missing", "恢复必须引用 InventorySystem 的恢复事实。", equipment);
            if (decision.StrengthAfter < 0 || decision.StrengthAfter > definition.StrengthCap || decision.DurabilityAfter < 0 || decision.DurabilityAfter > definition.MaxDurability)
                return Reject("recovery_boundary_failed", "恢复结果越过目录强度或耐久上限。", equipment);
            var result = Commit(request, "equipment_recovered", "遗失装备已按恢复路线找回，唯一 ID 保持不变。", "equipment_recovered",
                Values(equipment, definition, decision), definition.DefinitionId, equipment.InstanceId);
            int beforeStrength = equipment.Strength, beforeDurability = equipment.Durability;
            equipment.Status = D14EquipmentStatus.Recovered; equipment.OwnerRef = request.OwnerRef; equipment.Strength = decision.StrengthAfter;
            equipment.Durability = decision.DurabilityAfter; equipment.InventorySourceEventId = decision.InventorySourceEventId; equipment.LastEventId = result.EventId;
            equipment.RecoveryRoute = D14SpecialEquipmentIds.RecoveryRoute;
            result.Equipment = equipment.Clone(); result.Operation = AddOperation(request, D14EquipmentOperationKind.Recovery, equipment, decision, beforeStrength, beforeDurability, result.EventId);
            result.State = CaptureSnapshot(); return result;
        }

        private bool PrepareOperation(D14EquipmentRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, out D14EquipmentResult failure)
        {
            failure = null;
            if (!ValidRequest(request)) { failure = Reject("invalid_request", "装备操作请求缺少 operation_id。", equipment); return false; }
            var duplicate = DuplicateIfKnown(request.OperationId, request.SaveRevision);
            if (duplicate != null) { failure = duplicate; return false; }
            if (equipment == null) { failure = Reject("equipment_missing", "找不到装备实例。", null); return false; }
            if (definition == null) { failure = Reject("definition_missing", "找不到装备目录定义。", equipment); return false; }
            if (equipment.DefinitionId != definition.DefinitionId) { failure = Reject("definition_mismatch", "装备实例与目录定义不一致。", equipment); return false; }
            return true;
        }

        private bool ValidAcquireDecision(D14EquipmentAcquireDecision decision, D14EquipmentAcquireRequest request, D14SpecialEquipmentDefinition definition, string expectedInstanceId)
        {
            if (decision == null || !decision.Allowed || decision.InstanceId != expectedInstanceId || decision.SourceOwnerSystem != definition.AcquisitionOwnerSystem || !HasOwnedFact(decision.SourceEventId, definition.AcquisitionOwnerSystem)) return false;
            if (!string.Equals(decision.AuthorizationEventId, request.AuthorizationEventId, StringComparison.Ordinal)) return false;
            if (definition.RequiresAuthorization && !HasOwnedFact(decision.AuthorizationEventId, definition.AuthorizationOwnerSystem)) return false;
            if (string.IsNullOrWhiteSpace(decision.InventorySourceEventId) || !HasOwnedFact(decision.InventorySourceEventId, D14SpecialEquipmentIds.InventoryOwner)) return false;
            if (RequiresHistoricalReview(definition) && (decision.HistoricalReviewSourceEventId != request.HistoricalReviewEventId || !HasOwnedFact(decision.HistoricalReviewSourceEventId, D14SpecialEquipmentIds.HistoricalReviewOwner))) return false;
            if (decision.StrengthAfter < 0 || decision.StrengthAfter > definition.StrengthCap || decision.DurabilityAfter < 0 || decision.DurabilityAfter > definition.MaxDurability) return false;
            if (!Enum.IsDefined(typeof(D14EquipmentLegalStatus), decision.LegalStatus)) return false;
            return true;
        }

        private bool ValidShipDecision(D14EquipmentDecision decision, D14EquipmentStatus status, D14SpecialEquipmentDefinition definition, D14EquipmentInstance equipment, bool installation)
        {
            if (!ValidOwnerDecision(decision, D14SpecialEquipmentIds.ShipOwner) && !ValidOwnerDecision(decision, D14SpecialEquipmentIds.CombatOwner)) return false;
            if (!SameInstance(decision, equipment) || !decision.Allowed || decision.Status != status) return false;
            if (installation && decision.Slot != definition.Slot) return false;
            if (!string.IsNullOrWhiteSpace(decision.ShipSourceEventId) && !HasOwnedFact(decision.ShipSourceEventId, D14SpecialEquipmentIds.ShipOwner)) return false;
            return decision.StrengthAfter >= 0 && decision.StrengthAfter <= definition.StrengthCap && decision.DurabilityAfter >= 0 && decision.DurabilityAfter <= definition.MaxDurability;
        }

        private bool ValidOwnerDecision(D14EquipmentDecision decision, string owner)
        {
            return decision != null && string.Equals(decision.SourceOwnerSystem, owner, StringComparison.Ordinal) && HasOwnedFact(decision.SourceEventId, owner);
        }

        private bool SameInstance(D14EquipmentDecision decision, D14EquipmentInstance equipment)
        {
            return decision != null && equipment != null && string.Equals(decision.InstanceId, equipment.InstanceId, StringComparison.Ordinal);
        }

        private Dictionary<string, string> Values(D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, D14EquipmentDecision decision)
        {
            return new Dictionary<string, string>
            {
                ["instance_id"] = equipment.InstanceId, ["definition_id"] = definition.DefinitionId,
                ["status"] = decision.Status.ToString(), ["strength_before"] = equipment.Strength.ToString(CultureInfo.InvariantCulture),
                ["strength_after"] = decision.StrengthAfter.ToString(CultureInfo.InvariantCulture),
                ["durability_before"] = equipment.Durability.ToString(CultureInfo.InvariantCulture),
                ["durability_after"] = decision.DurabilityAfter.ToString(CultureInfo.InvariantCulture),
                ["source_event_id"] = decision.SourceEventId ?? "", ["inventory_source_event_id"] = decision.InventorySourceEventId ?? "",
                ["ship_source_event_id"] = decision.ShipSourceEventId ?? "", ["economy_source_event_id"] = decision.EconomySourceEventId ?? "",
                ["legal_source_event_id"] = decision.LegalSourceEventId ?? "", ["sale_value"] = decision.SaleValue.ToString(CultureInfo.InvariantCulture),
            };
        }

        private D14EquipmentOperationRecord AddOperation(D14EquipmentRequest request, D14EquipmentOperationKind kind, D14EquipmentInstance equipment, D14EquipmentDecision decision, int beforeStrength, int beforeDurability, string eventId)
        {
            var operation = new D14EquipmentOperationRecord
            {
                OperationId = request.OperationId, Kind = kind, InstanceId = equipment.InstanceId, DefinitionId = equipment.DefinitionId,
                Status = equipment.Status, StrengthBefore = beforeStrength, StrengthAfter = equipment.Strength,
                DurabilityBefore = beforeDurability, DurabilityAfter = equipment.Durability, SaleValue = decision.SaleValue,
                OwnerRef = equipment.OwnerRef, ShipId = equipment.ShipId, SourceEventId = decision.SourceEventId,
                AuthorizationEventId = decision.AuthorizationEventId, InventorySourceEventId = decision.InventorySourceEventId,
                ShipSourceEventId = decision.ShipSourceEventId, EconomySourceEventId = decision.EconomySourceEventId,
                LegalSourceEventId = decision.LegalSourceEventId, LastEventId = eventId,
            };
            _state.Operations.Add(operation);
            return operation.Clone();
        }

        private D14EquipmentResult Commit(D14EquipmentRequest request, string code, string message, string factCode, Dictionary<string, string> values, string definitionId, string instanceId)
        {
            var causes = new List<string>();
            if (request.CauseEventIds != null) foreach (var id in request.CauseEventIds) if (!string.IsNullOrWhiteSpace(id) && !causes.Contains(id)) causes.Add(id);
            AddCause(causes, request.InstanceId == null ? null : FindMutable(request.InstanceId) == null ? null : FindMutable(request.InstanceId).LastEventId);
            var eventId = D14SpecialEquipmentIds.EventPrefix + request.OperationId;
            var key = eventId + "\u001f" + Math.Max(0L, request.SaveRevision).ToString(CultureInfo.InvariantCulture);
            if (_eventLog.Find(eventId, Math.Max(0L, request.SaveRevision)) != null || _appliedEventKeys.Contains(key))
                return new D14EquipmentResult { Accepted = true, Duplicate = true, Code = "duplicate", Message = "装备操作已结算，不重复修改装备状态。", EventId = eventId, Fact = _eventLog.Find(eventId, Math.Max(0L, request.SaveRevision)), Equipment = Find(instanceId), State = CaptureSnapshot() };
            var append = _eventLog.Append(new WorldEvent(
                eventId, Math.Max(0, request.WorldDay), "open_sea", D14SpecialEquipmentIds.SystemOwner,
                new EventResult { ResultCode = code, Summary = message ?? string.Empty, FactIds = new List<string> { "equipment:" + factCode }, Values = values ?? new Dictionary<string, string>() },
                new[] { "player", request.InstanceId ?? instanceId ?? string.Empty }, new[] { "d14:special_equipment" }, causes, Math.Max(0L, request.SaveRevision)));
            _appliedEventKeys.Add(key); _state.LastEventId = eventId; _state.WorldDay = Math.Max(_state.WorldDay, Math.Max(0, request.WorldDay)); _state.Revision = Math.Max(_state.Revision + 1, Math.Max(0L, request.SaveRevision));
            return new D14EquipmentResult { Accepted = true, Code = code, Message = message, EventId = eventId, Fact = append.Fact, State = CaptureSnapshot() };
        }

        private void AddCause(List<string> causes, string id) { if (!string.IsNullOrWhiteSpace(id) && !causes.Contains(id) && HasFact(id)) causes.Add(id); }

        private D14EquipmentResult DuplicateIfKnown(string operationId, long revision)
        {
            if (string.IsNullOrWhiteSpace(operationId)) return null;
            var eventId = D14SpecialEquipmentIds.EventPrefix + operationId;
            var fact = _eventLog.Find(eventId, Math.Max(0L, revision));
            var key = eventId + "\u001f" + Math.Max(0L, revision).ToString(CultureInfo.InvariantCulture);
            return fact == null && !_appliedEventKeys.Contains(key) ? null : new D14EquipmentResult
            {
                Accepted = true, Duplicate = true, Code = "duplicate", Message = "装备操作已结算，不重复获取、维修、出售、收缴或恢复。", EventId = eventId,
                Fact = fact, Equipment = null, State = CaptureSnapshot(),
            };
        }

        private D14EquipmentResult Reject(string code, string message, D14EquipmentInstance equipment)
        {
            return D14EquipmentResult.Rejected(code, message, equipment, CaptureSnapshot());
        }

        private static bool ValidRequest(D14EquipmentRequest request) { return request != null && !string.IsNullOrWhiteSpace(request.OperationId) && request.SaveRevision >= 0; }

        private D14EquipmentInstance FindMutable(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) return null;
            foreach (var item in _state.Equipment) if (item != null && item.InstanceId == instanceId) return item;
            return null;
        }

        private FactEvent FindFact(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            foreach (var fact in _eventLog.Facts) if (fact != null && fact.EventId == eventId) return fact;
            return null;
        }

        private bool HasFact(string eventId) { return FindFact(eventId) != null; }

        private bool HasOwnedFact(string eventId, string owner)
        {
            var fact = FindFact(eventId);
            return fact != null && string.Equals(fact.OwnerSystem, owner, StringComparison.Ordinal);
        }

        private static bool RequiresHistoricalReview(D14SpecialEquipmentDefinition definition)
        {
            return definition != null && (definition.Kind == D14EquipmentKind.HistoricalWeapon || string.Equals(definition.HistoricalTag, "historical", StringComparison.OrdinalIgnoreCase));
        }

        private sealed class UnavailableRewardAuthority : ID14EquipmentRewardAuthority
        {
            public D14EquipmentAcquireDecision Acquire(D14EquipmentAcquireRequest request, D14SpecialEquipmentDefinition definition, FactEvent sourceFact, FactEvent authorizationFact) { return new D14EquipmentAcquireDecision { Code = "reward_authority_required", Message = "RewardSystem/CollectionAtlasSystem authority 未接入。" }; }
            public D14EquipmentDecision Recover(D14EquipmentRecoverRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent sourceFact) { return new D14EquipmentDecision { Code = "recovery_authority_required", Message = "RewardSystem/ItemCatalog recovery authority 未接入。" }; }
        }

        private sealed class UnavailableShipAuthority : ID14EquipmentShipAuthority
        {
            public D14EquipmentDecision Install(D14EquipmentInstallRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition) { return new D14EquipmentDecision { Code = "ship_authority_required", Message = "ShipSystem authority 未接入。" }; }
            public D14EquipmentDecision Maintain(D14EquipmentMaintainRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition) { return new D14EquipmentDecision { Code = "ship_authority_required", Message = "ShipSystem/ShipyardSystem authority 未接入。" }; }
        }

        private sealed class UnavailableCombatAuthority : ID14EquipmentCombatAuthority
        {
            public D14EquipmentDecision ApplyDamage(D14EquipmentDamageRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent combatFact) { return new D14EquipmentDecision { Code = "combat_authority_required", Message = "CombatSystem authority 未接入。" }; }
        }

        private sealed class UnavailableTradeAuthority : ID14EquipmentTradeAuthority
        {
            public D14EquipmentDecision Sell(D14EquipmentSellRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact) { return new D14EquipmentDecision { Code = "trade_authority_required", Message = "Economy/Inventory authority 未接入。" }; }
        }

        private sealed class UnavailableLegalAuthority : ID14EquipmentLegalAuthority
        {
            public D14EquipmentDecision Seize(D14EquipmentSeizeRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact) { return new D14EquipmentDecision { Code = "legal_authority_required", Message = "FactionLegalSystem authority 未接入。" }; }
        }
    }
}
