// D13-A05 岛屿治理/人口契约。
// IslandGovernanceSystem 只写居民、家庭、就业投影、健康、满意度、治安和治理记录；
// 产权、经济、生态、社群、宗教和代理人 authority 只返回带来源的裁决。

using System;
using System.Collections.Generic;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13GovernanceIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "IslandGovernanceSystem";
        public const string PropertyOwner = "PropertySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string EconomyOwner = "EconomySystem";
        public const string EcologyOwner = "EcologySystem";
        public const string CommunityOwner = "CommunityTradeSystem";
        public const string ReligionOwner = "ReligionSystem";
        public const string AgentOwner = "AgentManagementSystem";
        public const string PlayerRef = "player";
        public const string Currency = "coin";
        public const string LandRecoveryRoute = "route_governance_residence_review";
        public const string ProvisionRecoveryRoute = "route_governance_provision_review";
        public const string EconomyRecoveryRoute = "route_governance_economy_review";
        public const string PolicyRecoveryRoute = "route_governance_policy_review";
        public const string CommunityRecoveryRoute = "route_governance_community_review";
        public const string ReligionRecoveryRoute = "route_governance_religion_review";
        public const string SecurityRecoveryRoute = "route_governance_security_review";
        public const string AgentRecoveryRoute = "route_governance_agent_review";
    }

    public enum D13GovernanceContentReviewStatus
    {
        Approved,
        ApprovedWithWarning,
        PendingReview,
        Blocked,
    }

    public enum D13ResidentRole
    {
        Adult,
        Child,
        Elder,
    }

    public enum D13ResidentLifecycle
    {
        Resident,
        MigratingIn,
        MigratingOut,
        Departed,
        Deceased,
    }

    public enum D13EmploymentStatus
    {
        Unemployed,
        Employed,
        OnLeave,
    }

    public enum D13SecurityStatus
    {
        Stable,
        Tense,
        Unsafe,
        Crisis,
    }

    [Serializable]
    public sealed class D13GovernanceContentTag
    {
        public string ContentId;
        public string HistoricalTag;
        public string SensitiveContentLabel;
        public string ContentWarning;
        public D13GovernanceContentReviewStatus ReviewStatus = D13GovernanceContentReviewStatus.Approved;
        public string SourceNote;
        public List<string> Tags = new List<string>();

        public D13GovernanceContentTag Clone()
        {
            var result = new D13GovernanceContentTag
            {
                ContentId = ContentId,
                HistoricalTag = HistoricalTag,
                SensitiveContentLabel = SensitiveContentLabel,
                ContentWarning = ContentWarning,
                ReviewStatus = ReviewStatus,
                SourceNote = SourceNote,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate(string label)
        {
            Require(ContentId, label + ".content_id");
            Require(HistoricalTag, label + ".historical_tag");
            Require(SourceNote, label + ".source_note");
            if (HistoricalTag != "historical" && HistoricalTag != "inferred" && HistoricalTag != "fictional")
                throw new InvalidOperationException(label + " historical_tag is invalid.");
            if (!Enum.IsDefined(typeof(D13GovernanceContentReviewStatus), ReviewStatus))
                throw new InvalidOperationException(label + " review status is invalid.");
            if (Tags == null || Tags.Count == 0) throw new InvalidOperationException(label + " tags are required.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13IslandGovernanceDefinition
    {
        public int SchemaVersion = D13GovernanceIds.SchemaVersion;
        public string IslandId;
        public string DisplayNameKey;
        public string PropertyId;
        public string WaterServiceRef;
        public string FoodServiceRef;
        public string CommunityAgreementId;
        public string ReligionContentId;
        public string DefaultReligionArrangementId;
        public int HousingCapacity;
        public int EducationCapacity;
        public int SecurityBase;
        public int FoodUnitsPerResident = 2;
        public int WaterUnitsPerResident = 2;
        public int DefaultTaxRateBps = 800;
        public D13GovernanceContentTag Content = new D13GovernanceContentTag();
        public D13GovernanceContentTag ReligionContent = new D13GovernanceContentTag();
        public List<string> Tags = new List<string>();

        public D13IslandGovernanceDefinition Clone()
        {
            var result = new D13IslandGovernanceDefinition
            {
                SchemaVersion = SchemaVersion,
                IslandId = IslandId,
                DisplayNameKey = DisplayNameKey,
                PropertyId = PropertyId,
                WaterServiceRef = WaterServiceRef,
                FoodServiceRef = FoodServiceRef,
                CommunityAgreementId = CommunityAgreementId,
                ReligionContentId = ReligionContentId,
                DefaultReligionArrangementId = DefaultReligionArrangementId,
                HousingCapacity = HousingCapacity,
                EducationCapacity = EducationCapacity,
                SecurityBase = SecurityBase,
                FoodUnitsPerResident = FoodUnitsPerResident,
                WaterUnitsPerResident = WaterUnitsPerResident,
                DefaultTaxRateBps = DefaultTaxRateBps,
                Content = Content == null ? null : Content.Clone(),
                ReligionContent = ReligionContent == null ? null : ReligionContent.Clone(),
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != D13GovernanceIds.SchemaVersion) throw new InvalidOperationException("Governance island schema is invalid: " + IslandId);
            Require(IslandId, "island_id");
            Require(DisplayNameKey, "display_name_key");
            Require(PropertyId, "property_id");
            Require(WaterServiceRef, "water_service_ref");
            Require(FoodServiceRef, "food_service_ref");
            Require(CommunityAgreementId, "community_agreement_id");
            Require(ReligionContentId, "religion_content_id");
            Require(DefaultReligionArrangementId, "religion_arrangement_id");
            if (HousingCapacity <= 0 || EducationCapacity < 0 || SecurityBase < 0 || SecurityBase > 100 || FoodUnitsPerResident <= 0 || WaterUnitsPerResident <= 0 || DefaultTaxRateBps < 0 || DefaultTaxRateBps > 5000)
                throw new InvalidOperationException("Governance island numeric bounds are invalid: " + IslandId);
            if (Tags == null || Tags.Count == 0) throw new InvalidOperationException("Governance island tags are required: " + IslandId);
            if (Content == null || ReligionContent == null) throw new InvalidOperationException("Governance island content tags are required: " + IslandId);
            Content.Validate("island_content");
            ReligionContent.Validate("religion_content");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Governance island " + field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13GovernanceJobDefinition
    {
        public string JobId;
        public string DisplayNameKey;
        public int Slots;
        public int WagePerDay;
        public int RequiredEducation;
        public int SecurityContribution;
        public List<string> Tags = new List<string>();

        public D13GovernanceJobDefinition Clone()
        {
            var result = new D13GovernanceJobDefinition
            {
                JobId = JobId,
                DisplayNameKey = DisplayNameKey,
                Slots = Slots,
                WagePerDay = WagePerDay,
                RequiredEducation = RequiredEducation,
                SecurityContribution = SecurityContribution,
            };
            if (Tags != null) result.Tags.AddRange(Tags);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(JobId) || string.IsNullOrWhiteSpace(DisplayNameKey) || Slots <= 0 || WagePerDay < 0 || RequiredEducation < 0 || SecurityContribution < 0 || Tags == null || Tags.Count == 0)
                throw new InvalidOperationException("Governance job definition is invalid: " + JobId);
        }
    }

    [Serializable]
    public sealed class D13HouseholdState
    {
        public string HouseholdId;
        public string IslandId;
        public string FamilyName;
        public int HousingUnits = 1;
        public List<string> MemberIds = new List<string>();
        public long Revision;
        public int LastUpdatedDay;
        public string LastEventId;

        public D13HouseholdState Clone()
        {
            var result = new D13HouseholdState
            {
                HouseholdId = HouseholdId, IslandId = IslandId, FamilyName = FamilyName, HousingUnits = HousingUnits,
                Revision = Revision, LastUpdatedDay = LastUpdatedDay, LastEventId = LastEventId,
            };
            if (MemberIds != null) result.MemberIds.AddRange(MemberIds);
            return result;
        }

        public void Validate()
        {
            Require(HouseholdId, "household_id"); Require(IslandId, "island_id");
            if (HousingUnits <= 0 || Revision < 0 || LastUpdatedDay < 0 || MemberIds == null) throw new InvalidOperationException("Household state is invalid: " + HouseholdId);
        }
        private static void Require(string value, string field) { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Household " + field + " is required."); }
    }

    [Serializable]
    public sealed class D13ResidentState
    {
        public string ResidentId;
        public string HouseholdId;
        public string IslandId;
        public D13ResidentRole Role = D13ResidentRole.Adult;
        public D13ResidentLifecycle Lifecycle = D13ResidentLifecycle.Resident;
        public int Health = 100;
        public int Satisfaction = 70;
        public int EducationLevel;
        public string OccupationId;
        public D13EmploymentStatus EmploymentStatus = D13EmploymentStatus.Unemployed;
        public int WagePerDay;
        public int WageDue;
        public string ReligionId;
        public string ReligionContentId;
        public int FaithPracticeCount;
        public int LastWorldDay;
        public long Revision;
        public string LastEventId;

        public D13ResidentState Clone()
        {
            return new D13ResidentState
            {
                ResidentId = ResidentId, HouseholdId = HouseholdId, IslandId = IslandId, Role = Role, Lifecycle = Lifecycle,
                Health = Health, Satisfaction = Satisfaction, EducationLevel = EducationLevel, OccupationId = OccupationId,
                EmploymentStatus = EmploymentStatus, WagePerDay = WagePerDay, WageDue = WageDue, ReligionId = ReligionId,
                ReligionContentId = ReligionContentId, FaithPracticeCount = FaithPracticeCount, LastWorldDay = LastWorldDay,
                Revision = Revision, LastEventId = LastEventId,
            };
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ResidentId) || string.IsNullOrWhiteSpace(HouseholdId) || string.IsNullOrWhiteSpace(IslandId)) throw new InvalidOperationException("Resident identity is invalid: " + ResidentId);
            if (!Enum.IsDefined(typeof(D13ResidentRole), Role) || !Enum.IsDefined(typeof(D13ResidentLifecycle), Lifecycle) || !Enum.IsDefined(typeof(D13EmploymentStatus), EmploymentStatus)) throw new InvalidOperationException("Resident enum is invalid: " + ResidentId);
            if (Health < 0 || Health > 100 || Satisfaction < 0 || Satisfaction > 100 || EducationLevel < 0 || WagePerDay < 0 || WageDue < 0 || FaithPracticeCount < 0 || LastWorldDay < 0 || Revision < 0) throw new InvalidOperationException("Resident values are invalid: " + ResidentId);
        }
    }

    [Serializable]
    public sealed class D13GovernancePolicyState
    {
        public int TaxRateBps;
        public int WaterRationingLevel;
        public int FoodRationingLevel;
        public bool EducationOpen = true;
        public string ReligionArrangementId;
        public string SecurityPostureId;
        public string SourceEventId;
        public long Revision;
        public int LastUpdatedDay;

        public D13GovernancePolicyState Clone() { return (D13GovernancePolicyState)MemberwiseClone(); }

        public void Validate()
        {
            if (TaxRateBps < 0 || TaxRateBps > 5000 || WaterRationingLevel < 0 || WaterRationingLevel > 2 || FoodRationingLevel < 0 || FoodRationingLevel > 2 || string.IsNullOrWhiteSpace(ReligionArrangementId) || string.IsNullOrWhiteSpace(SecurityPostureId) || Revision < 0 || LastUpdatedDay < 0) throw new InvalidOperationException("Governance policy state is invalid.");
        }
    }

    [Serializable]
    public sealed class D13GovernanceAgreementState
    {
        public string AgreementId;
        public string CommunityId;
        public string Status;
        public D13GovernanceContentTag Content;
        public string SourceEventId;
        public int LastUpdatedDay;
        public long Revision;

        public D13GovernanceAgreementState Clone() { return new D13GovernanceAgreementState { AgreementId = AgreementId, CommunityId = CommunityId, Status = Status, Content = Content == null ? null : Content.Clone(), SourceEventId = SourceEventId, LastUpdatedDay = LastUpdatedDay, Revision = Revision }; }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AgreementId) || string.IsNullOrWhiteSpace(CommunityId) || string.IsNullOrWhiteSpace(Status) || Content == null || string.IsNullOrWhiteSpace(SourceEventId) || LastUpdatedDay < 0 || Revision < 0) throw new InvalidOperationException("Governance agreement state is invalid.");
            Content.Validate("community_agreement");
        }
    }

    [Serializable]
    public sealed class D13GovernanceProvisionRecord
    {
        public string RecordId;
        public string IslandId;
        public int ResidentCount;
        public int FoodRequired;
        public int FoodGranted;
        public int WaterRequired;
        public int WaterGranted;
        public string FoodSourceEventId;
        public string WaterSourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13GovernanceProvisionRecord Clone() { return (D13GovernanceProvisionRecord)MemberwiseClone(); }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RecordId) || string.IsNullOrWhiteSpace(IslandId) || ResidentCount < 0 || FoodRequired < 0 || FoodGranted < 0 || FoodGranted > FoodRequired || WaterRequired < 0 || WaterGranted < 0 || WaterGranted > WaterRequired || string.IsNullOrWhiteSpace(FoodSourceEventId) || string.IsNullOrWhiteSpace(WaterSourceEventId) || WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Governance provision record is invalid.");
        }
    }

    [Serializable]
    public sealed class D13GovernanceTaxRecord
    {
        public string AssessmentId;
        public string HouseholdId;
        public int TaxBase;
        public int TaxRateBps;
        public int AmountDue;
        public int AmountCollected;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;
        public D13GovernanceTaxRecord Clone() { return (D13GovernanceTaxRecord)MemberwiseClone(); }
        public void Validate() { if (string.IsNullOrWhiteSpace(AssessmentId) || string.IsNullOrWhiteSpace(HouseholdId) || TaxBase < 0 || TaxRateBps < 0 || TaxRateBps > 5000 || AmountDue < 0 || AmountCollected < 0 || AmountCollected > AmountDue || string.IsNullOrWhiteSpace(SourceEventId) || WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Governance tax record is invalid."); }
    }

    [Serializable]
    public sealed class D13GovernanceWageRecord
    {
        public string PaymentId;
        public string ResidentId;
        public int Amount;
        public string Currency = D13GovernanceIds.Currency;
        public string SourceEventId;
        public string LedgerRef;
        public int WorldDay;
        public long SaveRevision;
        public D13GovernanceWageRecord Clone() { return (D13GovernanceWageRecord)MemberwiseClone(); }
        public void Validate() { if (string.IsNullOrWhiteSpace(PaymentId) || string.IsNullOrWhiteSpace(ResidentId) || Amount < 0 || string.IsNullOrWhiteSpace(Currency) || string.IsNullOrWhiteSpace(SourceEventId) || string.IsNullOrWhiteSpace(LedgerRef) || WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Governance wage record is invalid."); }
    }

    [Serializable]
    public sealed class D13GovernanceIncidentState
    {
        public string IncidentId;
        public string IslandId;
        public int Severity;
        public bool Resolved;
        public string ResolutionId;
        public int WorldDay;
        public long Revision;
        public D13GovernanceIncidentState Clone() { return (D13GovernanceIncidentState)MemberwiseClone(); }
        public void Validate() { if (string.IsNullOrWhiteSpace(IncidentId) || string.IsNullOrWhiteSpace(IslandId) || Severity < 1 || Severity > 3 || WorldDay < 0 || Revision < 0 || (Resolved && string.IsNullOrWhiteSpace(ResolutionId))) throw new InvalidOperationException("Governance incident is invalid."); }
    }

    [Serializable]
    public sealed class D13GovernanceAgentAssignment
    {
        public string AssignmentId;
        public string AgentRef;
        public string IslandId;
        public string ResidentId;
        public string ManagementRole;
        public string SourceEventId;
        public int WorldDay;
        public long Revision;
        public D13GovernanceAgentAssignment Clone() { return (D13GovernanceAgentAssignment)MemberwiseClone(); }
        public void Validate() { if (string.IsNullOrWhiteSpace(AssignmentId) || string.IsNullOrWhiteSpace(AgentRef) || string.IsNullOrWhiteSpace(IslandId) || string.IsNullOrWhiteSpace(ManagementRole) || string.IsNullOrWhiteSpace(SourceEventId) || WorldDay < 0 || Revision < 0) throw new InvalidOperationException("Governance agent assignment is invalid."); }
    }

    [Serializable]
    public sealed class D13GovernanceRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13GovernanceIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13HouseholdState> Households = new List<D13HouseholdState>();
        public List<D13ResidentState> Residents = new List<D13ResidentState>();
        public D13GovernancePolicyState Policy = new D13GovernancePolicyState { ReligionArrangementId = "religion_catholic_parish", SecurityPostureId = "watch_regular" };
        public List<D13GovernanceAgreementState> Agreements = new List<D13GovernanceAgreementState>();
        public List<D13GovernanceProvisionRecord> ProvisionRecords = new List<D13GovernanceProvisionRecord>();
        public List<D13GovernanceTaxRecord> Taxes = new List<D13GovernanceTaxRecord>();
        public List<D13GovernanceWageRecord> Wages = new List<D13GovernanceWageRecord>();
        public List<D13GovernanceIncidentState> Incidents = new List<D13GovernanceIncidentState>();
        public List<D13GovernanceAgentAssignment> AgentAssignments = new List<D13GovernanceAgentAssignment>();
        public D13SecurityStatus SecurityStatus = D13SecurityStatus.Stable;
        public int SecurityScore = 70;
        public int LastWorldDay;
        public string LastEventId;
        public List<string> AppliedEventKeys = new List<string>();

        public D13GovernanceRuntimeSnapshot Clone()
        {
            var result = new D13GovernanceRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision, Policy = Policy == null ? null : Policy.Clone(), SecurityStatus = SecurityStatus, SecurityScore = SecurityScore, LastWorldDay = LastWorldDay, LastEventId = LastEventId };
            if (Households != null) foreach (var item in Households) if (item != null) result.Households.Add(item.Clone());
            if (Residents != null) foreach (var item in Residents) if (item != null) result.Residents.Add(item.Clone());
            if (Agreements != null) foreach (var item in Agreements) if (item != null) result.Agreements.Add(item.Clone());
            if (ProvisionRecords != null) foreach (var item in ProvisionRecords) if (item != null) result.ProvisionRecords.Add(item.Clone());
            if (Taxes != null) foreach (var item in Taxes) if (item != null) result.Taxes.Add(item.Clone());
            if (Wages != null) foreach (var item in Wages) if (item != null) result.Wages.Add(item.Clone());
            if (Incidents != null) foreach (var item in Incidents) if (item != null) result.Incidents.Add(item.Clone());
            if (AgentAssignments != null) foreach (var item in AgentAssignments) if (item != null) result.AgentAssignments.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || Revision < 0 || Households == null || Residents == null || Policy == null || Agreements == null || ProvisionRecords == null || Taxes == null || Wages == null || Incidents == null || AgentAssignments == null || AppliedEventKeys == null || SecurityScore < 0 || SecurityScore > 100 || LastWorldDay < 0) throw new InvalidOperationException("D13 governance snapshot is invalid.");
            if (!Enum.IsDefined(typeof(D13SecurityStatus), SecurityStatus)) throw new InvalidOperationException("D13 governance security status is invalid.");
            Policy.Validate();
            ValidateUnique(Households, item => item.HouseholdId, "household");
            ValidateUnique(Residents, item => item.ResidentId, "resident");
            ValidateUnique(ProvisionRecords, item => item.RecordId, "provision");
            ValidateUnique(Taxes, item => item.AssessmentId, "tax");
            ValidateUnique(Wages, item => item.PaymentId, "wage");
            ValidateUnique(Incidents, item => item.IncidentId, "incident");
            ValidateUnique(AgentAssignments, item => item.AssignmentId, "agent_assignment");
            foreach (var item in Households) item.Validate();
            foreach (var item in Residents) item.Validate();
            foreach (var item in Agreements) item.Validate();
            foreach (var item in ProvisionRecords) item.Validate();
            foreach (var item in Taxes) item.Validate();
            foreach (var item in Wages) item.Validate();
            foreach (var item in Incidents) item.Validate();
            foreach (var item in AgentAssignments) item.Validate();
        }

        private static void ValidateUnique<T>(IEnumerable<T> values, Func<T, string> key, string label)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in values) if (item == null || string.IsNullOrWhiteSpace(key(item)) || !ids.Add(key(item))) throw new InvalidOperationException("D13 governance duplicate " + label + ".");
        }
    }

    [Serializable]
    public sealed class D13GovernanceResidenceDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string PropertySourceEventId;
        public string LegalSourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.PropertyOwner;
    }

    [Serializable]
    public sealed class D13GovernanceProvisionDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public int FoodUnits;
        public int WaterUnits;
        public string FoodSourceEventId;
        public string WaterSourceEventId;
        public string FoodSourceOwnerSystem = D13GovernanceIds.EcologyOwner;
        public string WaterSourceOwnerSystem = D13GovernanceIds.EcologyOwner;
    }

    [Serializable]
    public sealed class D13GovernanceWageDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public int Amount;
        public string Currency = D13GovernanceIds.Currency;
        public string LedgerRef;
        public string SourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.EconomyOwner;
    }

    [Serializable]
    public sealed class D13GovernanceTaxDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public int AmountDue;
        public int AmountCollected;
        public string LedgerRef;
        public string SourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.EconomyOwner;
    }

    [Serializable]
    public sealed class D13GovernancePolicyDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public int TaxRateBps;
        public int WaterRationingLevel;
        public int FoodRationingLevel;
        public bool EducationOpen = true;
        public string ReligionArrangementId;
        public string SecurityPostureId;
        public string SourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.PropertyOwner;
    }

    [Serializable]
    public sealed class D13GovernanceAgreementDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string AgreementId;
        public string CommunityId;
        public string Status;
        public D13GovernanceContentTag Content;
        public string SourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.CommunityOwner;
    }

    [Serializable]
    public sealed class D13GovernanceAgentDecision
    {
        public bool Allowed;
        public string Code;
        public string Message;
        public string AgentRef;
        public string ManagementRole;
        public string SourceEventId;
        public string SourceOwnerSystem = D13GovernanceIds.AgentOwner;
    }

    public interface ID13GovernanceResidenceAuthority
    {
        D13GovernanceResidenceDecision ValidateResidence(D13IslandGovernanceDefinition definition, D13MigrationInRequest request, int worldDay);
    }

    public interface ID13GovernanceProvisionAuthority
    {
        D13GovernanceProvisionDecision Provide(D13GovernanceProvisionRequest request);
    }

    public interface ID13GovernanceEconomyAuthority
    {
        D13GovernanceWageDecision ApproveWage(D13ResidentState resident, D13GovernanceJobDefinition job, int worldDay, long saveRevision);
        D13GovernanceTaxDecision AssessTax(string householdId, int taxBase, int taxRateBps, int worldDay, long saveRevision);
    }

    public interface ID13GovernancePolicyAuthority
    {
        D13GovernancePolicyDecision ApprovePolicy(D13IslandGovernanceDefinition definition, D13GovernancePolicyRequest request, D13GovernancePolicyState currentState);
    }

    public interface ID13GovernanceCommunityAuthority
    {
        D13GovernanceAgreementDecision ValidateAgreement(D13IslandGovernanceDefinition definition, D13GovernanceAgreementRequest request, int worldDay);
    }

    public interface ID13GovernanceAgentAuthority
    {
        D13GovernanceAgentDecision ApproveManagement(D13IslandGovernanceDefinition definition, D13GovernanceAgentRequest request, int worldDay);
    }

    [Serializable]
    public abstract class D13GovernanceOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13GovernanceIds.PlayerRef;
        public string IslandId;
        public string ResidentId;
        public string HouseholdId;
        public int WorldDay;
        public long SaveRevision;
        public bool ReviewAcknowledged;
        public List<string> CauseEventIds = new List<string>();
    }

    [Serializable]
    public sealed class D13MigrationInRequest : D13GovernanceOperationRequest
    {
        public D13ResidentRole Role = D13ResidentRole.Adult;
        public string FamilyName;
        public string ReligionId;
        public string ReligionContentId;
        public string SourceEventId;
    }

    [Serializable]
    public sealed class D13MigrationOutRequest : D13GovernanceOperationRequest { }

    [Serializable]
    public sealed class D13JobAssignmentRequest : D13GovernanceOperationRequest
    {
        public string JobId;
    }

    [Serializable]
    public sealed class D13GovernanceProvisionRequest
    {
        public string OperationId;
        public string IslandId;
        public int ResidentCount;
        public int FoodRequired;
        public int WaterRequired;
        public int FoodRationingLevel;
        public int WaterRationingLevel;
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13GovernanceDayRequest : D13GovernanceOperationRequest
    {
        public int Seed;
    }

    [Serializable]
    public sealed class D13GovernancePolicyRequest : D13GovernanceOperationRequest
    {
        public int TaxRateBps;
        public int WaterRationingLevel;
        public int FoodRationingLevel;
        public bool EducationOpen = true;
        public string ReligionArrangementId;
        public string SecurityPostureId;
    }

    [Serializable]
    public sealed class D13GovernanceTaxRequest : D13GovernanceOperationRequest
    {
        public int TaxBase;
    }

    [Serializable]
    public sealed class D13GovernanceWageRequest : D13GovernanceOperationRequest { }

    [Serializable]
    public sealed class D13EducationRequest : D13GovernanceOperationRequest { }

    [Serializable]
    public sealed class D13ReligionPracticeRequest : D13GovernanceOperationRequest
    {
        public string ReligionId;
        public string ReligionContentId;
    }

    [Serializable]
    public sealed class D13GovernanceAgreementRequest : D13GovernanceOperationRequest
    {
        public string AgreementId;
        public string CommunityId;
    }

    [Serializable]
    public sealed class D13GovernanceAgentRequest : D13GovernanceOperationRequest
    {
        public string AgentRef;
        public string ManagementRole;
    }

    [Serializable]
    public sealed class D13SecurityIncidentRequest : D13GovernanceOperationRequest
    {
        public string IncidentId;
        public int Severity = 1;
        public string ResolutionId;
    }

    [Serializable]
    public sealed class D13GovernancePolicyProposal
    {
        public string ProposalId;
        public string IslandId;
        public int TaxRateBps;
        public int WaterRationingLevel;
        public int FoodRationingLevel;
        public bool EducationOpen;
        public string ReligionArrangementId;
        public string SecurityPostureId;
        public bool ProposedByAgent;

        public D13GovernancePolicyProposal Clone() { return (D13GovernancePolicyProposal)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class D13GovernanceResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public string RecoveryRouteId;
        public D13ResidentState Resident;
        public D13HouseholdState Household;
        public D13GovernancePolicyState Policy;
        public D13GovernanceProvisionRecord Provision;
        public D13GovernanceTaxRecord Tax;
        public D13GovernanceWageRecord Wage;
        public D13GovernanceAgreementState Agreement;
        public D13GovernanceAgentAssignment AgentAssignment;
        public D13GovernanceIncidentState Incident;
        public int SecurityScore;
        public D13SecurityStatus SecurityStatus;
        public FactEvent Fact;
    }

    [Serializable]
    public sealed class D13GovernanceReadModel
    {
        public D13IslandGovernanceDefinition Definition;
        public List<D13HouseholdState> Households = new List<D13HouseholdState>();
        public List<D13ResidentState> Residents = new List<D13ResidentState>();
        public D13GovernancePolicyState Policy;
        public List<D13GovernanceAgreementState> Agreements = new List<D13GovernanceAgreementState>();
        public List<D13GovernanceProvisionRecord> ProvisionRecords = new List<D13GovernanceProvisionRecord>();
        public List<D13GovernanceTaxRecord> Taxes = new List<D13GovernanceTaxRecord>();
        public List<D13GovernanceWageRecord> Wages = new List<D13GovernanceWageRecord>();
        public List<D13GovernanceIncidentState> Incidents = new List<D13GovernanceIncidentState>();
        public List<D13GovernanceAgentAssignment> AgentAssignments = new List<D13GovernanceAgentAssignment>();
        public int SecurityScore;
        public D13SecurityStatus SecurityStatus;
        public long Revision;
    }
}
