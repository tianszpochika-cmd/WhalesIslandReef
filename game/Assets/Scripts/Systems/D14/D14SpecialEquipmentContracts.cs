// D14-A03 特殊武器与船体模块契约。
// 目录定义不可变；装备获取、Inventory、Ship/Combat、Economy 与 Legal 的最终状态
// 仍由所属 authority 写入，A03 只保存带来源的装备投影和统一事实事件。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14SpecialEquipmentIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "D14SpecialEquipmentSystem";
        public const string ItemOwner = "ItemCatalog";
        public const string InventoryOwner = "InventorySystem";
        public const string RewardOwner = "RewardSystem";
        public const string ShipOwner = "ShipSystem";
        public const string CombatOwner = "CombatSystem";
        public const string EconomyOwner = "EconomySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string HistoricalReviewOwner = "HistoricalReviewSystem";
        public const string EventPrefix = "event_d14_equipment_";
        public const string RecoveryRoute = "route_d14_equipment_recover_from_loss";
        public const string ReviewRoute = "route_d14_historical_review";

        public static string CreateInstanceId(string definitionId, string sourceEventId)
        {
            if (string.IsNullOrWhiteSpace(definitionId) || string.IsNullOrWhiteSpace(sourceEventId)) return string.Empty;
            return definitionId + "@" + StablePart(sourceEventId);
        }

        private static string StablePart(string value)
        {
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-'))
                    chars[i] = '_';
            }
            return new string(chars);
        }
    }

    public enum D14EquipmentKind
    {
        SpecialGunMount,
        Firearm,
        SignalDevice,
        RescueTool,
        HistoricalWeapon,
        HullModule,
    }

    public enum D14EquipmentSlot
    {
        None,
        GunMount,
        DeckArmament,
        Signal,
        Rescue,
        Hull,
        Rigging,
        Utility,
    }

    public enum D14EquipmentStatus
    {
        Acquired,
        Installed,
        Damaged,
        Maintained,
        Sold,
        Seized,
        Lost,
        Recovered,
    }

    public enum D14EquipmentOperationKind
    {
        Acquisition,
        Installation,
        Maintenance,
        Damage,
        Sale,
        Seizure,
        Recovery,
    }

    public enum D14EquipmentReviewStatus
    {
        Pending,
        Approved,
        ApprovedWithWarning,
        Rejected,
    }

    public enum D14EquipmentLegalStatus
    {
        Legal,
        Licensed,
        Restricted,
        Seized,
        Evidence,
    }

    [Serializable]
    public sealed class D14SpecialEquipmentDefinition
    {
        public string DefinitionId;
        public string DisplayName;
        public D14EquipmentKind Kind;
        public D14EquipmentSlot Slot;
        public bool Unique;
        public int StrengthCap = 100;
        public int MaxDurability = 100;
        public int WearPerUse = 1;
        public string SourceRef;
        public string AuthorizationRef;
        public string AcquisitionOwnerSystem = D14SpecialEquipmentIds.RewardOwner;
        public string AuthorizationOwnerSystem = D14SpecialEquipmentIds.LegalOwner;
        public string HistoricalTag;
        public D14EquipmentReviewStatus ReviewStatus = D14EquipmentReviewStatus.Approved;
        public bool RequiresAuthorization;
        public string LicenseRef;
        public string TestId;
        public List<string> AllowedShipTypeIds = new List<string>();
        public List<string> DependencyIds = new List<string>();

        public D14SpecialEquipmentDefinition Clone()
        {
            var result = new D14SpecialEquipmentDefinition
            {
                DefinitionId = DefinitionId, DisplayName = DisplayName, Kind = Kind, Slot = Slot,
                Unique = Unique, StrengthCap = StrengthCap, MaxDurability = MaxDurability, WearPerUse = WearPerUse,
                SourceRef = SourceRef, AuthorizationRef = AuthorizationRef, AcquisitionOwnerSystem = AcquisitionOwnerSystem,
                AuthorizationOwnerSystem = AuthorizationOwnerSystem, HistoricalTag = HistoricalTag, ReviewStatus = ReviewStatus,
                RequiresAuthorization = RequiresAuthorization, LicenseRef = LicenseRef, TestId = TestId,
            };
            if (AllowedShipTypeIds != null) result.AllowedShipTypeIds.AddRange(AllowedShipTypeIds);
            if (DependencyIds != null) result.DependencyIds.AddRange(DependencyIds);
            return result;
        }

        public bool AllowsShip(string shipTypeId)
        {
            if (AllowedShipTypeIds == null || AllowedShipTypeIds.Count == 0) return true;
            return !string.IsNullOrWhiteSpace(shipTypeId) && AllowedShipTypeIds.Contains(shipTypeId);
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(DefinitionId) || !DefinitionId.StartsWith("item_", StringComparison.Ordinal))
                throw new InvalidOperationException(field + ".definition_id must use the item_ content ID prefix.");
            if (string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(SourceRef) || string.IsNullOrWhiteSpace(TestId))
                throw new InvalidOperationException(field + " requires display_name, source_ref and test_id.");
            if (!Enum.IsDefined(typeof(D14EquipmentKind), Kind) || !Enum.IsDefined(typeof(D14EquipmentSlot), Slot))
                throw new InvalidOperationException(field + " contains an invalid kind or slot.");
            if (StrengthCap < 0 || StrengthCap > 100 || MaxDurability <= 0 || MaxDurability > 100 || WearPerUse < 0 || WearPerUse > MaxDurability)
                throw new InvalidOperationException(field + " contains an invalid strength boundary or durability.");
            if (string.IsNullOrWhiteSpace(AcquisitionOwnerSystem) || string.IsNullOrWhiteSpace(AuthorizationOwnerSystem))
                throw new InvalidOperationException(field + " requires acquisition and authorization owners.");
            if (string.IsNullOrWhiteSpace(HistoricalTag))
                throw new InvalidOperationException(field + " requires a historical tag.");
            if (!Enum.IsDefined(typeof(D14EquipmentReviewStatus), ReviewStatus) || ReviewStatus == D14EquipmentReviewStatus.Pending || ReviewStatus == D14EquipmentReviewStatus.Rejected)
                throw new InvalidOperationException(field + " requires an approved historical review status.");
            if (RequiresAuthorization && (string.IsNullOrWhiteSpace(AuthorizationRef) || string.IsNullOrWhiteSpace(LicenseRef)))
                throw new InvalidOperationException(field + " requires authorization_ref and license_ref.");
        }
    }

    [Serializable]
    public sealed class D14EquipmentInstance
    {
        public string InstanceId;
        public string DefinitionId;
        public string OwnerRef;
        public string ShipId;
        public D14EquipmentSlot Slot;
        public D14EquipmentStatus Status;
        public D14EquipmentLegalStatus LegalStatus = D14EquipmentLegalStatus.Legal;
        public int Strength;
        public int Durability;
        public string AcquisitionSourceEventId;
        public string AuthorizationEventId;
        public string HistoricalReviewEventId;
        public string InventorySourceEventId;
        public string LastEventId;
        public string RecoveryRoute;

        public D14EquipmentInstance Clone()
        {
            return new D14EquipmentInstance
            {
                InstanceId = InstanceId, DefinitionId = DefinitionId, OwnerRef = OwnerRef, ShipId = ShipId,
                Slot = Slot, Status = Status, LegalStatus = LegalStatus, Strength = Strength, Durability = Durability,
                AcquisitionSourceEventId = AcquisitionSourceEventId, AuthorizationEventId = AuthorizationEventId,
                HistoricalReviewEventId = HistoricalReviewEventId, InventorySourceEventId = InventorySourceEventId,
                LastEventId = LastEventId, RecoveryRoute = RecoveryRoute,
            };
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(InstanceId) || string.IsNullOrWhiteSpace(DefinitionId) || string.IsNullOrWhiteSpace(OwnerRef))
                throw new InvalidOperationException(field + " requires instance_id, definition_id and owner_ref.");
            if (!Enum.IsDefined(typeof(D14EquipmentSlot), Slot) || !Enum.IsDefined(typeof(D14EquipmentStatus), Status) || !Enum.IsDefined(typeof(D14EquipmentLegalStatus), LegalStatus))
                throw new InvalidOperationException(field + " contains an invalid status.");
            if (Strength < 0 || Strength > 100 || Durability < 0 || Durability > 100)
                throw new InvalidOperationException(field + " strength/durability must be 0..100.");
            if (string.IsNullOrWhiteSpace(AcquisitionSourceEventId) || string.IsNullOrWhiteSpace(InventorySourceEventId) || string.IsNullOrWhiteSpace(LastEventId))
                throw new InvalidOperationException(field + " requires source and last event ids.");
        }
    }

    [Serializable]
    public sealed class D14EquipmentOperationRecord
    {
        public string OperationId;
        public D14EquipmentOperationKind Kind;
        public string InstanceId;
        public string DefinitionId;
        public D14EquipmentStatus Status;
        public int StrengthBefore;
        public int StrengthAfter;
        public int DurabilityBefore;
        public int DurabilityAfter;
        public int SaleValue;
        public string OwnerRef;
        public string ShipId;
        public string SourceEventId;
        public string AuthorizationEventId;
        public string InventorySourceEventId;
        public string ShipSourceEventId;
        public string EconomySourceEventId;
        public string LegalSourceEventId;
        public string LastEventId;

        public D14EquipmentOperationRecord Clone()
        {
            return new D14EquipmentOperationRecord
            {
                OperationId = OperationId, Kind = Kind, InstanceId = InstanceId, DefinitionId = DefinitionId,
                Status = Status, StrengthBefore = StrengthBefore, StrengthAfter = StrengthAfter,
                DurabilityBefore = DurabilityBefore, DurabilityAfter = DurabilityAfter, SaleValue = SaleValue,
                OwnerRef = OwnerRef, ShipId = ShipId, SourceEventId = SourceEventId, AuthorizationEventId = AuthorizationEventId,
                InventorySourceEventId = InventorySourceEventId, ShipSourceEventId = ShipSourceEventId,
                EconomySourceEventId = EconomySourceEventId, LegalSourceEventId = LegalSourceEventId, LastEventId = LastEventId,
            };
        }
    }

    [Serializable]
    public sealed class D14SpecialEquipmentRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14SpecialEquipmentIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public List<D14EquipmentInstance> Equipment = new List<D14EquipmentInstance>();
        public List<D14EquipmentOperationRecord> Operations = new List<D14EquipmentOperationRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14SpecialEquipmentRuntimeSnapshot Clone()
        {
            var result = new D14SpecialEquipmentRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Equipment != null) foreach (var item in Equipment) if (item != null) result.Equipment.Add(item.Clone());
            if (Operations != null) foreach (var item in Operations) if (item != null) result.Operations.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0 || Equipment == null || Operations == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14 special equipment snapshot is invalid.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in Equipment)
            {
                if (item == null) throw new InvalidOperationException("D14 equipment snapshot contains null equipment.");
                item.Validate("d14.equipment");
                if (!ids.Add(item.InstanceId)) throw new InvalidOperationException("Duplicate D14 equipment instance: " + item.InstanceId);
            }
            var operations = new HashSet<string>(StringComparer.Ordinal);
            foreach (var operation in Operations)
            {
                if (operation == null || string.IsNullOrWhiteSpace(operation.OperationId) || !operations.Add(operation.OperationId))
                    throw new InvalidOperationException("D14 equipment operation ids must be unique.");
            }
        }
    }

    public abstract class D14EquipmentRequest
    {
        public string OperationId;
        public string InstanceId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> CauseEventIds = new List<string>();
    }

    public sealed class D14EquipmentAcquireRequest : D14EquipmentRequest
    {
        public string DefinitionId;
        public string SourceEventId;
        public string AuthorizationEventId;
        public string HistoricalReviewEventId;
        public string OwnerRef;
        public string ShipId;
        public bool HistoricalWarningAcknowledged;
    }

    public sealed class D14EquipmentInstallRequest : D14EquipmentRequest
    {
        public string ShipId;
        public string ShipTypeId;
        public D14EquipmentSlot RequestedSlot;
        public int RequestedStrength;
    }

    public sealed class D14EquipmentMaintainRequest : D14EquipmentRequest
    {
        public string ShipId;
        public string RepairMaterialEventId;
        public int RequestedDurability;
    }

    public sealed class D14EquipmentDamageRequest : D14EquipmentRequest
    {
        public string ShipId;
        public string CombatSourceEventId;
        public string DamageContext;
    }

    public sealed class D14EquipmentSellRequest : D14EquipmentRequest
    {
        public string SellerRef;
        public string LegalSourceEventId;
    }

    public sealed class D14EquipmentSeizeRequest : D14EquipmentRequest
    {
        public string LegalSourceEventId;
        public string EvidenceRef;
    }

    public sealed class D14EquipmentRecoverRequest : D14EquipmentRequest
    {
        public string RecoverySourceEventId;
        public string AuthorizationEventId;
        public string OwnerRef;
    }

    public class D14EquipmentDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string SourceOwnerSystem;
        public string SourceEventId;
        public string InventorySourceEventId;
        public string ShipSourceEventId;
        public string EconomySourceEventId;
        public string LegalSourceEventId;
        public string AuthorizationEventId;
        public string InstanceId;
        public D14EquipmentStatus Status;
        public D14EquipmentLegalStatus LegalStatus = D14EquipmentLegalStatus.Legal;
        public D14EquipmentSlot Slot;
        public int StrengthAfter;
        public int DurabilityAfter;
        public int SaleValue;
    }

    public sealed class D14EquipmentAcquireDecision : D14EquipmentDecision
    {
        public string AcquisitionOwnerSystem;
        public string HistoricalReviewSourceEventId;
    }

    public sealed class D14EquipmentResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14EquipmentInstance Equipment;
        public D14EquipmentOperationRecord Operation;
        public D14SpecialEquipmentRuntimeSnapshot State;

        public static D14EquipmentResult Rejected(string code, string message, D14EquipmentInstance equipment, D14SpecialEquipmentRuntimeSnapshot state)
        {
            return new D14EquipmentResult { Accepted = false, Code = code, Message = message, Equipment = equipment == null ? null : equipment.Clone(), State = state };
        }
    }

    public interface ID14EquipmentRewardAuthority
    {
        D14EquipmentAcquireDecision Acquire(D14EquipmentAcquireRequest request, D14SpecialEquipmentDefinition definition, FactEvent sourceFact, FactEvent authorizationFact);
        D14EquipmentDecision Recover(D14EquipmentRecoverRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent sourceFact);
    }

    public interface ID14EquipmentShipAuthority
    {
        D14EquipmentDecision Install(D14EquipmentInstallRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition);
        D14EquipmentDecision Maintain(D14EquipmentMaintainRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition);
    }

    public interface ID14EquipmentCombatAuthority
    {
        D14EquipmentDecision ApplyDamage(D14EquipmentDamageRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent combatFact);
    }

    public interface ID14EquipmentTradeAuthority
    {
        D14EquipmentDecision Sell(D14EquipmentSellRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact);
    }

    public interface ID14EquipmentLegalAuthority
    {
        D14EquipmentDecision Seize(D14EquipmentSeizeRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact);
    }
}
