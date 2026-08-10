// D14-S01 同步集成契约。
// 本文件只描述跨系统请求、owner authority 和集成审计；不拥有船体、船员、货物、金钱、法律、任务、关系、装备或奖励状态。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14S01IntegrationIds
    {
        public const int SchemaVersion = 1;
        public const string OwnerSystem = "D14IntegrationSystem";
        public const string ShipOwner = "ShipSystem";
        public const string CrewOwner = "CrewSystem";
        public const string InventoryOwner = "InventorySystem";
        public const string EconomyOwner = "EconomySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string QuestOwner = "QuestSystem";
        public const string RelationOwner = "RelationSystem";
        public const string EquipmentOwner = "D14SpecialEquipmentSystem";
        public const string RewardOwner = "RewardSystem";
        public const string EventPrefix = "event_d14_s01_integration_";
        public const string SaveKey = "d14S01Integration";
    }

    public enum D14S01IntegrationStage { Combat, Aftercare, Equipment, Reward }

    [Serializable]
    public sealed class D14S01LegalIntent
    {
        public string ActionKind;
        public string SubjectRef;
        public string FactionId;
        public string PortId;
        public string PermitRef;
        public string EvidenceEventId;

        public D14S01LegalIntent Clone() { return (D14S01LegalIntent)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ActionKind) || string.IsNullOrWhiteSpace(SubjectRef) ||
                string.IsNullOrWhiteSpace(FactionId) || string.IsNullOrWhiteSpace(EvidenceEventId))
                throw new InvalidOperationException(field + " requires action, subject, faction and evidence.");
        }
    }

    [Serializable]
    public sealed class D14S01RewardIntent
    {
        public string RequestId;
        public string RewardId;
        public string RecipientRef = "player";
        public string TaskId;

        public D14S01RewardIntent Clone() { return (D14S01RewardIntent)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RequestId) || string.IsNullOrWhiteSpace(RewardId) || string.IsNullOrWhiteSpace(RecipientRef))
                throw new InvalidOperationException(field + " requires request, reward and recipient ids.");
        }
    }

    [Serializable]
    public sealed class D14S01EquipmentLifecycleRequest
    {
        public string OperationId;
        public D14EquipmentOperationKind Operation;
        public string DefinitionId;
        public string InstanceId;
        public string OwnerRef = "player";
        public string ShipId;
        public string SourceEventId;
        public string HistoricalReviewEventId;

        public D14S01EquipmentLifecycleRequest Clone()
        {
            return (D14S01EquipmentLifecycleRequest)MemberwiseClone();
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(OwnerRef) || string.IsNullOrWhiteSpace(SourceEventId) ||
                !Enum.IsDefined(typeof(D14EquipmentOperationKind), Operation))
                throw new InvalidOperationException(field + " requires operation, owner and source event.");
            if (Operation == D14EquipmentOperationKind.Acquisition && string.IsNullOrWhiteSpace(DefinitionId))
                throw new InvalidOperationException(field + " acquisition requires definition id.");
            if (Operation != D14EquipmentOperationKind.Acquisition && string.IsNullOrWhiteSpace(InstanceId))
                throw new InvalidOperationException(field + " non-acquisition operation requires instance id.");
        }
    }

    public sealed class D14S01CombatIntegrationRequest
    {
        public string OperationId;
        public string RequestedBySystem = D14S01IntegrationIds.OwnerSystem;
        public string PlayerRef = "player";
        public string PlayerShipId;
        public D14CombatResult CombatResult;
        public D14S01LegalIntent LegalIntent;
        public D14S01EquipmentLifecycleRequest Equipment;
        public D14S01RewardIntent Reward;
        public string QuestId;
        public string RelationTargetRef;
        public int WorldDay;
        public long SaveRevision;

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(PlayerRef) || CombatResult == null ||
                CombatResult.Fact == null || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException(field + " requires combat fact, actor, time and revision.");
            if (LegalIntent != null) LegalIntent.Validate(field + ".legal");
            if (Equipment != null) Equipment.Validate(field + ".equipment");
            if (Reward != null) Reward.Validate(field + ".reward");
        }
    }

    public sealed class D14S01AftercareIntegrationRequest
    {
        public string OperationId;
        public string RequestedBySystem = D14S01IntegrationIds.OwnerSystem;
        public string PlayerRef = "player";
        public D14AftercareResult AftercareResult;
        public D14S01LegalIntent LegalIntent;
        public D14S01RewardIntent Reward;
        public string QuestId;
        public string RelationTargetRef;
        public int WorldDay;
        public long SaveRevision;

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(PlayerRef) || AftercareResult == null ||
                AftercareResult.Fact == null || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException(field + " requires aftercare fact, actor, time and revision.");
            if (LegalIntent != null) LegalIntent.Validate(field + ".legal");
            if (Reward != null) Reward.Validate(field + ".reward");
        }
    }

    public sealed class D14S01EquipmentIntegrationRequest
    {
        public string OperationId;
        public string RequestedBySystem = D14S01IntegrationIds.OwnerSystem;
        public D14S01EquipmentLifecycleRequest Equipment;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || Equipment == null || string.IsNullOrWhiteSpace(SourceEventId) || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException(field + " requires equipment, source event, time and revision.");
            Equipment.Validate(field + ".equipment");
        }
    }

    public sealed class D14S01RewardIntegrationRequest
    {
        public string OperationId;
        public string RequestedBySystem = D14S01IntegrationIds.OwnerSystem;
        public D14S01RewardIntent Reward;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || Reward == null || string.IsNullOrWhiteSpace(SourceEventId) || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException(field + " requires reward, source event, time and revision.");
            Reward.Validate(field + ".reward");
        }
    }

    public sealed class D14S01AuthorityDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string SourceOwnerSystem;
        public string SourceEventId;
        public string GrantId;
    }

    [Serializable]
    public sealed class D14S01IntegrationLedgerRecord
    {
        public string OperationId;
        public D14S01IntegrationStage Stage;
        public string SourceEventId;
        public string IntegrationEventId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> AuthorityEventIds = new List<string>();
        public string RewardGrantId;
        public string LegalActionKind;

        public D14S01IntegrationLedgerRecord Clone()
        {
            var result = (D14S01IntegrationLedgerRecord)MemberwiseClone();
            result.AuthorityEventIds = AuthorityEventIds == null ? new List<string>() : new List<string>(AuthorityEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(IntegrationEventId) ||
                !Enum.IsDefined(typeof(D14S01IntegrationStage), Stage) || WorldDay < 0 || SaveRevision < 0 || AuthorityEventIds == null)
                throw new InvalidOperationException(field + " requires operation, stage, source and authority events.");
        }
    }

    [Serializable]
    public sealed class D14S01IntegrationRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14S01IntegrationIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public List<D14S01IntegrationLedgerRecord> Ledger = new List<D14S01IntegrationLedgerRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14S01IntegrationRuntimeSnapshot Clone()
        {
            var result = new D14S01IntegrationRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Ledger != null) foreach (var record in Ledger) if (record != null) result.Ledger.Add(record.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0 || Ledger == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14-S01 integration snapshot is invalid.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in Ledger)
            {
                if (record == null) throw new InvalidOperationException("D14-S01 integration ledger contains null.");
                record.Validate("d14.s01.ledger");
                if (!ids.Add(record.OperationId)) throw new InvalidOperationException("Duplicate D14-S01 integration operation: " + record.OperationId);
            }
        }
    }

    public interface ID14S01ShipAuthority
    {
        D14S01AuthorityDecision ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat);
    }

    public interface ID14S01CrewAuthority
    {
        D14S01AuthorityDecision ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat);
        D14S01AuthorityDecision ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare);
    }

    public interface ID14S01CargoAuthority
    {
        D14S01AuthorityDecision ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare);
    }

    public interface ID14S01EconomyAuthority
    {
        D14S01AuthorityDecision ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat);
        D14S01AuthorityDecision ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare);
    }

    public interface ID14S01LegalAuthority
    {
        D14S01AuthorityDecision Apply(D14S01LegalIntent intent, FactEvent sourceFact, int worldDay, long saveRevision);
    }

    public interface ID14S01QuestAuthority
    {
        D14S01AuthorityDecision Observe(string questId, string stage, FactEvent sourceFact, int worldDay, long saveRevision);
    }

    public interface ID14S01RelationAuthority
    {
        D14S01AuthorityDecision Apply(string relationTargetRef, string stage, FactEvent sourceFact, int worldDay, long saveRevision);
    }

    public interface ID14S01EquipmentAuthority
    {
        D14S01AuthorityDecision Apply(D14S01EquipmentLifecycleRequest request, FactEvent sourceFact, int worldDay, long saveRevision);
    }

    public interface ID14S01RewardAuthority
    {
        D14S01AuthorityDecision Grant(D14S01RewardIntent reward, FactEvent sourceFact, int worldDay, long saveRevision);
    }

    public sealed class D14S01IntegrationResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14S01IntegrationLedgerRecord Ledger;
        public D14S01IntegrationRuntimeSnapshot State;

        public static D14S01IntegrationResult Rejected(string code, string message, D14S01IntegrationRuntimeSnapshot state)
        {
            return new D14S01IntegrationResult { Accepted = false, Code = code, Message = message, State = state };
        }
    }
}
