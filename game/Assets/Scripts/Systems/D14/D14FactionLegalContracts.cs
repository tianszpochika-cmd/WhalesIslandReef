// D14-A05 势力/法律契约。
// FactionLegalSystem 是声望、外交关系、法律状态和港口许可的唯一写者；
// Combat/Inventory/Economy 只提交事实或返回所属系统的来源引用。

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D14
{
    public static class D14FactionLegalIds
    {
        public const int SchemaVersion = 1;
        public const string OwnerSystem = "FactionLegalSystem";
        public const string CombatOwner = "CombatSystem";
        public const string EconomyOwner = "EconomySystem";
        public const string InventoryOwner = "InventorySystem";
        public const string EscortOwner = "EscortSystem";
        public const string DiplomacyOwner = "DiplomaticAuthority";
        public const string QuestOwner = "QuestSystem";
        public const string RewardOwner = "RewardSystem";
        public const string EventPrefix = "event_d14_faction_legal_";
        public const string BlockadeRecoveryRoute = "route_d14_blockade_diplomatic_review";
        public const string WantedRecoveryRoute = "route_d14_wanted_appeal_or_settlement";
        public const string SeizureRecoveryRoute = "route_d14_seizure_legal_release";

        public static string StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')) chars[i] = '_';
            }
            return new string(chars);
        }

        public static string PairKey(string factionA, string factionB)
        {
            if (string.IsNullOrWhiteSpace(factionA) || string.IsNullOrWhiteSpace(factionB) || factionA == factionB) return string.Empty;
            return string.CompareOrdinal(factionA, factionB) < 0 ? factionA + ":" + factionB : factionB + ":" + factionA;
        }
    }

    public enum D14FactionRelationStatus { Neutral, Friendly, Allied, AtWar, Truce, Blockaded }
    public enum D14PrivateerPermitStatus { Active, Expired, Revoked, Suspended }
    public enum D14PortAccessStatus { Open, Restricted, Denied, Blockaded, EscortRequired }
    public enum D14WantedStatus { Open, Cleared, Expired }
    public enum D14LegalCaseStatus { Open, Paid, Released, Resolved, Expired, Denied }
    public enum D14FinancialCaseKind { Fine, Ransom }
    public enum D14BlockadeStatus { Declared, Lifted, Expired }
    public enum D14DiplomaticIntentKind { RequestTruce, RequestPrivateerPermit, RequestEscort, AppealFine, RequestRansom, RequestLiftBlockade }

    [Serializable]
    public sealed class D14FactionLegalDefinition
    {
        public string FactionId;
        public string DisplayName;
        public string Culture;
        public string HistoricalTag = "inferred";
        public string ReviewStatus = "approved";
        public string SourceRef;
        public List<string> HomePortIds = new List<string>();

        public D14FactionLegalDefinition Clone()
        {
            var result = new D14FactionLegalDefinition
            {
                FactionId = FactionId, DisplayName = DisplayName, Culture = Culture,
                HistoricalTag = HistoricalTag, ReviewStatus = ReviewStatus, SourceRef = SourceRef,
            };
            if (HomePortIds != null) result.HomePortIds.AddRange(HomePortIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(FactionId) || string.IsNullOrWhiteSpace(DisplayName) || string.IsNullOrWhiteSpace(Culture) || string.IsNullOrWhiteSpace(SourceRef))
                throw new InvalidOperationException(field + " requires faction id, display name, culture and source reference.");
            if (string.IsNullOrWhiteSpace(HistoricalTag) || string.IsNullOrWhiteSpace(ReviewStatus) || ReviewStatus == "pending" || ReviewStatus == "rejected")
                throw new InvalidOperationException(field + " requires an approved historical review.");
        }
    }

    [Serializable]
    public sealed class D14PortLegalDefinition
    {
        public string PortId;
        public string ControllingFactionId;
        public string HistoricalTag = "inferred";
        public string ReviewStatus = "approved";
        public string SourceRef;

        public D14PortLegalDefinition Clone()
        {
            return new D14PortLegalDefinition
            {
                PortId = PortId, ControllingFactionId = ControllingFactionId,
                HistoricalTag = HistoricalTag, ReviewStatus = ReviewStatus, SourceRef = SourceRef,
            };
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(PortId) || string.IsNullOrWhiteSpace(ControllingFactionId) || string.IsNullOrWhiteSpace(SourceRef) ||
                string.IsNullOrWhiteSpace(HistoricalTag) || string.IsNullOrWhiteSpace(ReviewStatus) || ReviewStatus == "pending" || ReviewStatus == "rejected")
                throw new InvalidOperationException(field + " requires port ownership and reviewed source metadata.");
        }
    }

    [Serializable]
    public sealed class D14FactionRelationState
    {
        public string RelationKey;
        public string FactionA;
        public string FactionB;
        public D14FactionRelationStatus Status = D14FactionRelationStatus.Neutral;
        public int StartedWorldDay;
        public int LastUpdatedWorldDay;
        public string SourceEventId;
        public string EvidenceEventId;
        public string LastEventId;

        public D14FactionRelationState Clone() { return (D14FactionRelationState)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(RelationKey) || string.IsNullOrWhiteSpace(FactionA) || string.IsNullOrWhiteSpace(FactionB) ||
                string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(LastEventId) || !Enum.IsDefined(typeof(D14FactionRelationStatus), Status))
                throw new InvalidOperationException(field + " requires relation ids, status and source events.");
            if (StartedWorldDay < 0 || LastUpdatedWorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14PrivateerPermitState
    {
        public string PermitId;
        public string IssuerFactionId;
        public string HolderRef;
        public string TargetFactionId;
        public D14PrivateerPermitStatus Status = D14PrivateerPermitStatus.Active;
        public int GrantedWorldDay;
        public int ExpiresWorldDay;
        public string SourceEventId;
        public string EvidenceEventId;
        public string LastEventId;

        public D14PrivateerPermitState Clone() { return (D14PrivateerPermitState)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(PermitId) || string.IsNullOrWhiteSpace(IssuerFactionId) || string.IsNullOrWhiteSpace(HolderRef) ||
                string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(EvidenceEventId) || string.IsNullOrWhiteSpace(LastEventId) ||
                !Enum.IsDefined(typeof(D14PrivateerPermitStatus), Status))
                throw new InvalidOperationException(field + " requires permit, issuer, holder, evidence and status.");
            if (GrantedWorldDay < 0 || ExpiresWorldDay < GrantedWorldDay) throw new InvalidOperationException(field + " has invalid permit time.");
        }
    }

    [Serializable]
    public sealed class D14PortLegalState
    {
        public string PortId;
        public string ControllingFactionId;
        public D14PortAccessStatus AccessStatus = D14PortAccessStatus.Open;
        public string Reason;
        public int LastUpdatedWorldDay;
        public string SourceEventId;
        public string EvidenceEventId;
        public string LastEventId;

        public D14PortLegalState Clone() { return (D14PortLegalState)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(PortId) || string.IsNullOrWhiteSpace(ControllingFactionId) || string.IsNullOrWhiteSpace(SourceEventId) ||
                string.IsNullOrWhiteSpace(LastEventId) || !Enum.IsDefined(typeof(D14PortAccessStatus), AccessStatus))
                throw new InvalidOperationException(field + " requires port, controlling faction, status and source events.");
            if (LastUpdatedWorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14WantedCaseState
    {
        public string CaseId;
        public string SubjectRef;
        public string FactionId;
        public int Severity;
        public D14WantedStatus Status = D14WantedStatus.Open;
        public int IssuedWorldDay;
        public int ResolvedWorldDay = -1;
        public string Reason;
        public string SourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14WantedCaseState Clone()
        {
            var result = (D14WantedCaseState)MemberwiseClone();
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(CaseId) || string.IsNullOrWhiteSpace(SubjectRef) || string.IsNullOrWhiteSpace(FactionId) ||
                string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(LastEventId) || !Enum.IsDefined(typeof(D14WantedStatus), Status) || EvidenceEventIds == null)
                throw new InvalidOperationException(field + " requires wanted case, subject, faction, source, evidence and status.");
            if (Severity < 1 || Severity > 5 || IssuedWorldDay < 0 || ResolvedWorldDay < -1) throw new InvalidOperationException(field + " has invalid severity or time.");
        }
    }

    [Serializable]
    public sealed class D14InspectionRecord
    {
        public string InspectionId;
        public string SubjectRef;
        public string PortId;
        public string ControllingFactionId;
        public bool Allowed;
        public bool CargoLegal;
        public List<string> ProhibitedCargoRefs = new List<string>();
        public string WantedCaseId;
        public string SeizureId;
        public string FineCaseId;
        public int WorldDay;
        public string SourceEventId;
        public string CargoManifestEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14InspectionRecord Clone()
        {
            var result = (D14InspectionRecord)MemberwiseClone();
            result.ProhibitedCargoRefs = ProhibitedCargoRefs == null ? new List<string>() : new List<string>(ProhibitedCargoRefs);
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(InspectionId) || string.IsNullOrWhiteSpace(SubjectRef) || string.IsNullOrWhiteSpace(PortId) ||
                string.IsNullOrWhiteSpace(ControllingFactionId) || string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(CargoManifestEventId) ||
                string.IsNullOrWhiteSpace(LastEventId) || ProhibitedCargoRefs == null || EvidenceEventIds == null)
                throw new InvalidOperationException(field + " requires inspection subject, port, manifest, source and evidence.");
            if (WorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14SeizureRecord
    {
        public string SeizureId;
        public string SubjectRef;
        public string FactionId;
        public D14LegalCaseStatus Status = D14LegalCaseStatus.Open;
        public List<string> CargoRefs = new List<string>();
        public string CargoManifestEventId;
        public string InventorySourceEventId;
        public int SeizedWorldDay;
        public int ReleasedWorldDay = -1;
        public string SourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14SeizureRecord Clone()
        {
            var result = (D14SeizureRecord)MemberwiseClone();
            result.CargoRefs = CargoRefs == null ? new List<string>() : new List<string>(CargoRefs);
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(SeizureId) || string.IsNullOrWhiteSpace(SubjectRef) || string.IsNullOrWhiteSpace(FactionId) ||
                string.IsNullOrWhiteSpace(CargoManifestEventId) || string.IsNullOrWhiteSpace(InventorySourceEventId) || string.IsNullOrWhiteSpace(SourceEventId) ||
                string.IsNullOrWhiteSpace(LastEventId) || CargoRefs == null || EvidenceEventIds == null || !Enum.IsDefined(typeof(D14LegalCaseStatus), Status))
                throw new InvalidOperationException(field + " requires seizure, cargo, inventory, source, evidence and status.");
            if (SeizedWorldDay < 0 || ReleasedWorldDay < -1) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14FinancialCaseState
    {
        public string CaseId;
        public D14FinancialCaseKind Kind;
        public string SubjectRef;
        public string FactionId;
        public string PrisonerRef;
        public D14LegalCaseStatus Status = D14LegalCaseStatus.Open;
        public int RequestedAmount;
        public int IssuedWorldDay;
        public int ResolvedWorldDay = -1;
        public string Reason;
        public string SourceEventId;
        public string EconomySourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14FinancialCaseState Clone()
        {
            var result = (D14FinancialCaseState)MemberwiseClone();
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(CaseId) || string.IsNullOrWhiteSpace(SubjectRef) || string.IsNullOrWhiteSpace(FactionId) ||
                string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(EconomySourceEventId) || string.IsNullOrWhiteSpace(LastEventId) ||
                RequestedAmount < 0 || EvidenceEventIds == null || !Enum.IsDefined(typeof(D14FinancialCaseKind), Kind) || !Enum.IsDefined(typeof(D14LegalCaseStatus), Status))
                throw new InvalidOperationException(field + " requires financial case, amount, economy/source evidence and status.");
            if (IssuedWorldDay < 0 || ResolvedWorldDay < -1) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14EscortRecord
    {
        public string ContractId;
        public string EscorterRef;
        public string IssuerFactionId;
        public string RouteId;
        public string ProtectedCargoManifestEventId;
        public bool Success;
        public int ReputationDelta;
        public int WorldDay;
        public string SourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14EscortRecord Clone()
        {
            var result = (D14EscortRecord)MemberwiseClone();
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ContractId) || string.IsNullOrWhiteSpace(EscorterRef) || string.IsNullOrWhiteSpace(IssuerFactionId) ||
                string.IsNullOrWhiteSpace(RouteId) || string.IsNullOrWhiteSpace(ProtectedCargoManifestEventId) || string.IsNullOrWhiteSpace(SourceEventId) ||
                string.IsNullOrWhiteSpace(LastEventId) || EvidenceEventIds == null) throw new InvalidOperationException(field + " requires escort contract and evidence.");
            if (WorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14BlockadeState
    {
        public string BlockadeId;
        public string IssuerFactionId;
        public string TargetFactionId;
        public string TargetPortId;
        public D14BlockadeStatus Status = D14BlockadeStatus.Declared;
        public int StartedWorldDay;
        public int LastUpdatedWorldDay;
        public string Reason;
        public string SourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string LastEventId;

        public D14BlockadeState Clone()
        {
            var result = (D14BlockadeState)MemberwiseClone();
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(BlockadeId) || string.IsNullOrWhiteSpace(IssuerFactionId) ||
                (string.IsNullOrWhiteSpace(TargetFactionId) && string.IsNullOrWhiteSpace(TargetPortId)) || string.IsNullOrWhiteSpace(SourceEventId) ||
                string.IsNullOrWhiteSpace(LastEventId) || EvidenceEventIds == null || !Enum.IsDefined(typeof(D14BlockadeStatus), Status))
                throw new InvalidOperationException(field + " requires blockade target, source, evidence and status.");
            if (StartedWorldDay < 0 || LastUpdatedWorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14DiplomaticIntentRecord
    {
        public string ProposalId;
        public string ActorRef;
        public string RequestedBySystem;
        public string TargetFactionId;
        public D14DiplomaticIntentKind IntentKind;
        public string Summary;
        public string Status = "pending";
        public int WorldDay;
        public string LastEventId;

        public D14DiplomaticIntentRecord Clone() { return (D14DiplomaticIntentRecord)MemberwiseClone(); }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(ProposalId) || string.IsNullOrWhiteSpace(ActorRef) || string.IsNullOrWhiteSpace(RequestedBySystem) ||
                string.IsNullOrWhiteSpace(TargetFactionId) || string.IsNullOrWhiteSpace(Summary) || string.IsNullOrWhiteSpace(Status) || string.IsNullOrWhiteSpace(LastEventId) ||
                !Enum.IsDefined(typeof(D14DiplomaticIntentKind), IntentKind)) throw new InvalidOperationException(field + " requires diplomatic intent fields.");
            if (WorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14FactionLegalAuditRecord
    {
        public string AuditId;
        public string Action;
        public string EventId;
        public string SourceEventId;
        public List<string> EvidenceEventIds = new List<string>();
        public string SubjectRef;
        public string FactionId;
        public int WorldDay;
        public string Reason;
        public string RecoveryRoute;

        public D14FactionLegalAuditRecord Clone()
        {
            var result = (D14FactionLegalAuditRecord)MemberwiseClone();
            result.EvidenceEventIds = EvidenceEventIds == null ? new List<string>() : new List<string>(EvidenceEventIds);
            return result;
        }

        public void Validate(string field)
        {
            if (string.IsNullOrWhiteSpace(AuditId) || string.IsNullOrWhiteSpace(Action) || string.IsNullOrWhiteSpace(EventId) ||
                string.IsNullOrWhiteSpace(SourceEventId) || EvidenceEventIds == null) throw new InvalidOperationException(field + " requires audit and source evidence.");
            if (WorldDay < 0) throw new InvalidOperationException(field + " has invalid time.");
        }
    }

    [Serializable]
    public sealed class D14FactionLegalRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D14FactionLegalIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public int WorldDay;
        public string LastEventId;
        public Dictionary<string, int> Reputation = new Dictionary<string, int>();
        public Dictionary<string, int> WantedLevels = new Dictionary<string, int>();
        public List<D14FactionRelationState> Relations = new List<D14FactionRelationState>();
        public List<D14PrivateerPermitState> Permits = new List<D14PrivateerPermitState>();
        public List<D14PortLegalState> Ports = new List<D14PortLegalState>();
        public List<D14WantedCaseState> WantedCases = new List<D14WantedCaseState>();
        public List<D14InspectionRecord> Inspections = new List<D14InspectionRecord>();
        public List<D14SeizureRecord> Seizures = new List<D14SeizureRecord>();
        public List<D14FinancialCaseState> FinancialCases = new List<D14FinancialCaseState>();
        public List<D14EscortRecord> Escorts = new List<D14EscortRecord>();
        public List<D14BlockadeState> Blockades = new List<D14BlockadeState>();
        public List<D14DiplomaticIntentRecord> DiplomaticIntents = new List<D14DiplomaticIntentRecord>();
        public List<D14FactionLegalAuditRecord> Audits = new List<D14FactionLegalAuditRecord>();
        public List<string> AppliedEventKeys = new List<string>();
        public EventLogSnapshot EventLog;

        public D14FactionLegalRuntimeSnapshot Clone()
        {
            var result = new D14FactionLegalRuntimeSnapshot
            {
                SchemaVersion = SchemaVersion, Revision = Revision, WorldDay = WorldDay, LastEventId = LastEventId,
                EventLog = EventLog == null ? null : EventLog.Clone(),
            };
            if (Reputation != null) foreach (var pair in Reputation) result.Reputation[pair.Key] = pair.Value;
            if (WantedLevels != null) foreach (var pair in WantedLevels) result.WantedLevels[pair.Key] = pair.Value;
            if (Relations != null) foreach (var item in Relations) if (item != null) result.Relations.Add(item.Clone());
            if (Permits != null) foreach (var item in Permits) if (item != null) result.Permits.Add(item.Clone());
            if (Ports != null) foreach (var item in Ports) if (item != null) result.Ports.Add(item.Clone());
            if (WantedCases != null) foreach (var item in WantedCases) if (item != null) result.WantedCases.Add(item.Clone());
            if (Inspections != null) foreach (var item in Inspections) if (item != null) result.Inspections.Add(item.Clone());
            if (Seizures != null) foreach (var item in Seizures) if (item != null) result.Seizures.Add(item.Clone());
            if (FinancialCases != null) foreach (var item in FinancialCases) if (item != null) result.FinancialCases.Add(item.Clone());
            if (Escorts != null) foreach (var item in Escorts) if (item != null) result.Escorts.Add(item.Clone());
            if (Blockades != null) foreach (var item in Blockades) if (item != null) result.Blockades.Add(item.Clone());
            if (DiplomaticIntents != null) foreach (var item in DiplomaticIntents) if (item != null) result.DiplomaticIntents.Add(item.Clone());
            if (Audits != null) foreach (var item in Audits) if (item != null) result.Audits.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || WorldDay < 0 || Reputation == null || WantedLevels == null ||
                Relations == null || Permits == null || Ports == null || WantedCases == null || Inspections == null || Seizures == null ||
                FinancialCases == null || Escorts == null || Blockades == null || DiplomaticIntents == null || Audits == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D14 faction/legal snapshot is invalid.");
            ValidateIds(Relations, "relation", delegate(D14FactionRelationState value) { value.Validate("d14.relation"); return value.RelationKey; });
            ValidateIds(Permits, "permit", delegate(D14PrivateerPermitState value) { value.Validate("d14.permit"); return value.PermitId; });
            ValidateIds(Ports, "port", delegate(D14PortLegalState value) { value.Validate("d14.port"); return value.PortId; });
            ValidateIds(WantedCases, "wanted", delegate(D14WantedCaseState value) { value.Validate("d14.wanted"); return value.CaseId; });
            ValidateIds(Inspections, "inspection", delegate(D14InspectionRecord value) { value.Validate("d14.inspection"); return value.InspectionId; });
            ValidateIds(Seizures, "seizure", delegate(D14SeizureRecord value) { value.Validate("d14.seizure"); return value.SeizureId; });
            ValidateIds(FinancialCases, "financial", delegate(D14FinancialCaseState value) { value.Validate("d14.financial"); return value.CaseId; });
            ValidateIds(Escorts, "escort", delegate(D14EscortRecord value) { value.Validate("d14.escort"); return value.ContractId; });
            ValidateIds(Blockades, "blockade", delegate(D14BlockadeState value) { value.Validate("d14.blockade"); return value.BlockadeId; });
            ValidateIds(DiplomaticIntents, "intent", delegate(D14DiplomaticIntentRecord value) { value.Validate("d14.intent"); return value.ProposalId; });
            ValidateIds(Audits, "audit", delegate(D14FactionLegalAuditRecord value) { value.Validate("d14.audit"); return value.AuditId; });
            foreach (var pair in Reputation) if (pair.Value < -100 || pair.Value > 100) throw new InvalidOperationException("Reputation is outside -100..100: " + pair.Key);
            foreach (var pair in WantedLevels) if (pair.Value < 0 || pair.Value > 5) throw new InvalidOperationException("Wanted level is outside 0..5: " + pair.Key);
        }

        private static void ValidateIds<T>(IEnumerable<T> values, string label, Func<T, string> key)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null) throw new InvalidOperationException("D14 faction/legal " + label + " contains null.");
                var id = key(value);
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) throw new InvalidOperationException("Duplicate D14 faction/legal " + label + ": " + id);
            }
        }
    }

    public abstract class D14FactionLegalOperationRequest
    {
        public string OperationId;
        public string ActorRef = "player";
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;
        public List<string> EvidenceEventIds = new List<string>();
        public List<string> CauseEventIds = new List<string>();

        public void Validate(string field, bool evidenceRequired = true)
        {
            if (string.IsNullOrWhiteSpace(OperationId) || string.IsNullOrWhiteSpace(ActorRef) || string.IsNullOrWhiteSpace(SourceEventId) ||
                WorldDay < 0 || SaveRevision < 0 || EvidenceEventIds == null || CauseEventIds == null || (evidenceRequired && EvidenceEventIds.Count == 0))
                throw new InvalidOperationException(field + " requires operation, actor, source, time, causes and evidence.");
        }
    }

    public sealed class D14ReputationRequest : D14FactionLegalOperationRequest { public string FactionId; public int Delta; public string Reason; }
    public sealed class D14WarRequest : D14FactionLegalOperationRequest { public string FactionA; public string FactionB; public string TreatyRef; }
    public sealed class D14TruceRequest : D14FactionLegalOperationRequest { public string FactionA; public string FactionB; public string TreatyRef; }
    public sealed class D14PrivateerPermitRequest : D14FactionLegalOperationRequest { public string PermitId; public string IssuerFactionId; public string HolderRef; public string TargetFactionId; public int ExpiresWorldDay; }
    public sealed class D14InspectionRequest : D14FactionLegalOperationRequest { public string InspectionId; public string SubjectRef; public string PortId; public string ControllingFactionId; public string CargoManifestEventId; }
    public sealed class D14WantedRequest : D14FactionLegalOperationRequest { public string CaseId; public string SubjectRef; public string FactionId; public int Severity; public string Reason; }
    public sealed class D14WantedClearRequest : D14FactionLegalOperationRequest { public string CaseId; public string SettlementSourceEventId; }
    public sealed class D14EscortRequest : D14FactionLegalOperationRequest { public string ContractId; public string EscorterRef; public string IssuerFactionId; public string RouteId; public string ProtectedCargoManifestEventId; }
    public sealed class D14PortAccessRequest : D14FactionLegalOperationRequest { public string PortId; public string ControllingFactionId; public D14PortAccessStatus AccessStatus; public string Reason; }
    public sealed class D14SeizureRequest : D14FactionLegalOperationRequest { public string SeizureId; public string SubjectRef; public string FactionId; public string CargoManifestEventId; public List<string> CargoRefs = new List<string>(); }
    public sealed class D14FineRequest : D14FactionLegalOperationRequest { public string CaseId; public string SubjectRef; public string FactionId; public string Reason; }
    public sealed class D14RansomRequest : D14FactionLegalOperationRequest { public string CaseId; public string SubjectRef; public string FactionId; public string PrisonerRef; public string Reason; }
    public sealed class D14FinancialSettlementRequest : D14FactionLegalOperationRequest { public string CaseId; public string SettlementSourceEventId; }
    public sealed class D14BlockadeRequest : D14FactionLegalOperationRequest { public string BlockadeId; public string IssuerFactionId; public string TargetFactionId; public string TargetPortId; public D14BlockadeStatus Status; public string Reason; }
    public sealed class D14DiplomaticIntentRequest
    {
        public string ProposalId; public string ActorRef; public string RequestedBySystem; public string TargetFactionId;
        public D14DiplomaticIntentKind IntentKind; public string Summary; public int WorldDay; public long SaveRevision;
    }

    public sealed class D14CargoInspectionDecision
    {
        public bool Allowed; public bool CargoLegal; public string Code; public string Message;
        public string SourceOwnerSystem = D14FactionLegalIds.InventoryOwner; public string SourceEventId; public List<string> ProhibitedCargoRefs = new List<string>();
    }

    public sealed class D14SeizureDecision
    {
        public bool Allowed; public string Code; public string Message;
        public string SourceOwnerSystem = D14FactionLegalIds.InventoryOwner; public string SourceEventId; public List<string> SeizedCargoRefs = new List<string>();
    }

    public sealed class D14EconomicAssessmentDecision
    {
        public bool Allowed; public string Code; public string Message; public int Amount;
        public string SourceOwnerSystem = D14FactionLegalIds.EconomyOwner; public string SourceEventId;
    }

    public sealed class D14SettlementConfirmationDecision
    {
        public bool Allowed; public string Code; public string Message;
        public string SourceOwnerSystem = D14FactionLegalIds.EconomyOwner; public string SourceEventId;
    }

    public interface ID14FactionLegalCargoAuthority
    {
        D14CargoInspectionDecision Inspect(D14InspectionRequest request, FactEvent manifestFact);
    }

    public interface ID14FactionLegalInventoryAuthority
    {
        D14SeizureDecision Seize(D14SeizureRequest request, FactEvent manifestFact, IReadOnlyList<string> prohibitedCargoRefs);
    }

    public interface ID14FactionLegalEconomyAuthority
    {
        D14EconomicAssessmentDecision AssessFine(D14FineRequest request, FactEvent sourceFact);
        D14EconomicAssessmentDecision AssessRansom(D14RansomRequest request, FactEvent sourceFact);
        D14SettlementConfirmationDecision ConfirmSettlement(D14FinancialSettlementRequest request, D14FinancialCaseState financialCase, FactEvent settlementFact);
    }

    public sealed class D14FactionLegalResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D14FactionRelationState Relation;
        public D14PrivateerPermitState Permit;
        public D14PortLegalState Port;
        public D14WantedCaseState WantedCase;
        public D14InspectionRecord Inspection;
        public D14SeizureRecord Seizure;
        public D14FinancialCaseState FinancialCase;
        public D14EscortRecord Escort;
        public D14BlockadeState Blockade;
        public D14DiplomaticIntentRecord DiplomaticIntent;
        public int ReputationBefore;
        public int ReputationAfter;
        public D14FactionLegalRuntimeSnapshot State;

        public static D14FactionLegalResult Rejected(string code, string message, D14FactionLegalRuntimeSnapshot state)
        {
            return new D14FactionLegalResult { Accepted = false, Code = code, Message = message, State = state };
        }
    }
}
