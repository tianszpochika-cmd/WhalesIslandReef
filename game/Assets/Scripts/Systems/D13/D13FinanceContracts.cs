// D13-A02 金融、信用与保险契约。
// FinanceSystem 是账户、债务、利息、保险和金融风险投影的唯一写者；
// Property/Legal/Economy authority 只提供各自的来源化输入或裁决。

using System;
using System.Collections.Generic;
using System.Linq;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public static class D13FinanceIds
    {
        public const int SchemaVersion = 1;
        public const string SystemOwner = "FinanceSystem";
        public const string EconomyOwner = "EconomySystem";
        public const string PropertyOwner = "PropertySystem";
        public const string LegalOwner = "FactionLegalSystem";
        public const string PlayerRef = "player";
        public const string Currency = "coin";
        public const string LoanRecoveryRoute = "route_finance_loan_review";
        public const string DebtRecoveryRoute = "route_finance_debt_recovery";
        public const string InsuranceEvidenceRecoveryRoute = "route_finance_insurance_evidence_review";
        public const string LegalSeizureRecoveryRoute = "route_finance_legal_seizure_review";
        public const string BankruptcyRecoveryRoute = "route_finance_bankruptcy_review";
    }

    public enum D13FinanceAccountType
    {
        PlayerCashAndCredit,
        MerchantLedger,
        InstitutionLedger,
    }

    public enum D13FinanceCashOperation
    {
        Deposit,
        Withdraw,
        Spend,
        Receive,
    }

    public enum D13FinanceDebtKind
    {
        MerchantCredit,
        Loan,
        PropertyMortgage,
        TradeAdvance,
        Other,
    }

    public enum D13FinanceDebtLifecycleState
    {
        Active,
        PastDue,
        Extended,
        Defaulted,
        Collection,
        SeizurePending,
        Seized,
        Paid,
        BankruptcyProtected,
        WrittenOff,
    }

    public enum D13FinanceCollectionState
    {
        None,
        NoticeIssued,
        Negotiation,
        SeizureRequested,
        Seized,
        BankruptcyProtected,
        Resolved,
    }

    public enum D13FinanceGuaranteeStateKind
    {
        Active,
        Called,
        Released,
        Defaulted,
    }

    public enum D13FinanceInsuranceKind
    {
        Cargo,
        Hull,
        Storm,
        Pirate,
        Delay,
    }

    public enum D13FinancePolicyState
    {
        Active,
        Paid,
        Expired,
        Cancelled,
    }

    public enum D13FinanceClaimState
    {
        Submitted,
        Approved,
        PartiallyPaid,
        Denied,
    }

    public enum D13FinanceEvidenceKind
    {
        Incident,
        Manifest,
        DamageReport,
        WeatherRecord,
        Witness,
        Receipt,
        LegalPermit,
        RepairEstimate,
    }

    public enum D13FinanceCollectionAction
    {
        IssueNotice,
        CollectAvailableCash,
        RequestAssetSeizure,
    }

    [Serializable]
    public sealed class D13FinanceAccountState
    {
        public string AccountId;
        public string OwnerRef;
        public D13FinanceAccountType AccountType = D13FinanceAccountType.PlayerCashAndCredit;
        public string Currency = D13FinanceIds.Currency;
        public long CashBalance;
        public long CreditLimit;
        public long CreditUsed;
        public int CreditScore;
        public List<string> CreditReasons = new List<string>();
        public List<string> DebtIds = new List<string>();
        public List<string> PolicyIds = new List<string>();
        public int BankruptcyProtectionUntilDay;
        public int BankruptcyProtectionCount;
        public long Revision;
        public int LastUpdatedDay;
        public string LastEventId;

        public D13FinanceAccountState Clone()
        {
            var result = new D13FinanceAccountState
            {
                AccountId = AccountId,
                OwnerRef = OwnerRef,
                AccountType = AccountType,
                Currency = Currency,
                CashBalance = CashBalance,
                CreditLimit = CreditLimit,
                CreditUsed = CreditUsed,
                CreditScore = CreditScore,
                BankruptcyProtectionUntilDay = BankruptcyProtectionUntilDay,
                BankruptcyProtectionCount = BankruptcyProtectionCount,
                Revision = Revision,
                LastUpdatedDay = LastUpdatedDay,
                LastEventId = LastEventId,
            };
            if (CreditReasons != null) result.CreditReasons.AddRange(CreditReasons);
            if (DebtIds != null) result.DebtIds.AddRange(DebtIds);
            if (PolicyIds != null) result.PolicyIds.AddRange(PolicyIds);
            return result;
        }

        public void Validate()
        {
            Require(AccountId, "finance account id");
            Require(OwnerRef, "finance account owner_ref");
            Require(Currency, "finance account currency");
            if (!Enum.IsDefined(typeof(D13FinanceAccountType), AccountType)) throw new InvalidOperationException("Finance account type is invalid.");
            if (CashBalance < 0 || CreditLimit < 0 || CreditUsed < 0 || CreditUsed > CreditLimit)
                throw new InvalidOperationException("Finance account cash or credit is invalid.");
            if (CreditScore < 0 || CreditScore > 100) throw new InvalidOperationException("Finance account credit score is invalid.");
            if (BankruptcyProtectionUntilDay < 0 || BankruptcyProtectionCount < 0 || Revision < 0 || LastUpdatedDay < 0)
                throw new InvalidOperationException("Finance account time or revision is invalid.");
            if (CreditReasons == null || DebtIds == null || PolicyIds == null)
                throw new InvalidOperationException("Finance account collections are required.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceCollateralState
    {
        public string AssetRef;
        public string AssetKind;
        public long ClaimValue;
        public string ValuationSourceOwnerSystem;
        public string ValuationSourceEventId;
        public string LegalSourceOwnerSystem;
        public string LegalSourceEventId;
        public string SourceEventId;

        public D13FinanceCollateralState Clone()
        {
            return new D13FinanceCollateralState
            {
                AssetRef = AssetRef,
                AssetKind = AssetKind,
                ClaimValue = ClaimValue,
                ValuationSourceOwnerSystem = ValuationSourceOwnerSystem,
                ValuationSourceEventId = ValuationSourceEventId,
                LegalSourceOwnerSystem = LegalSourceOwnerSystem,
                LegalSourceEventId = LegalSourceEventId,
                SourceEventId = SourceEventId,
            };
        }

        public void Validate()
        {
            Require(AssetRef, "collateral asset_ref");
            Require(AssetKind, "collateral asset_kind");
            if (ClaimValue < 0) throw new InvalidOperationException("Collateral claim value must be non-negative.");
            Require(ValuationSourceOwnerSystem, "collateral valuation source owner");
            Require(ValuationSourceEventId, "collateral valuation source event");
            Require(LegalSourceOwnerSystem, "collateral legal source owner");
            Require(LegalSourceEventId, "collateral legal source event");
            Require(SourceEventId, "collateral source event");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceDebtState
    {
        public string DebtId;
        public string AccountId;
        public string DebtorRef;
        public string CreditorRef;
        public D13FinanceDebtKind Kind;
        public D13FinanceDebtLifecycleState State = D13FinanceDebtLifecycleState.Active;
        public D13FinanceDebtLifecycleState StateBeforeProtection = D13FinanceDebtLifecycleState.Active;
        public D13FinanceCollectionState CollectionState = D13FinanceCollectionState.None;
        public long Principal;
        public long InterestAccrued;
        public long LatePenalty;
        public int InterestRatePercent;
        public int LatePenaltyPercent;
        public int DueDay;
        public int GraceUntilDay;
        public int LastAccrualDay;
        public int LastPaymentDay;
        public int RenegotiationCount;
        public string LedgerRef;
        public string SourceEventId;
        public List<D13FinanceCollateralState> Collateral = new List<D13FinanceCollateralState>();
        public List<string> GuaranteeIds = new List<string>();
        public string RiskExplanation;
        public string WorstCaseOutcome;
        public long Revision;
        public int LastUpdatedDay;
        public string LastEventId;

        public long TotalDue { get { return Principal + InterestAccrued + LatePenalty; } }

        public D13FinanceDebtState Clone()
        {
            var result = new D13FinanceDebtState
            {
                DebtId = DebtId,
                AccountId = AccountId,
                DebtorRef = DebtorRef,
                CreditorRef = CreditorRef,
                Kind = Kind,
                State = State,
                StateBeforeProtection = StateBeforeProtection,
                CollectionState = CollectionState,
                Principal = Principal,
                InterestAccrued = InterestAccrued,
                LatePenalty = LatePenalty,
                InterestRatePercent = InterestRatePercent,
                LatePenaltyPercent = LatePenaltyPercent,
                DueDay = DueDay,
                GraceUntilDay = GraceUntilDay,
                LastAccrualDay = LastAccrualDay,
                LastPaymentDay = LastPaymentDay,
                RenegotiationCount = RenegotiationCount,
                LedgerRef = LedgerRef,
                SourceEventId = SourceEventId,
                RiskExplanation = RiskExplanation,
                WorstCaseOutcome = WorstCaseOutcome,
                Revision = Revision,
                LastUpdatedDay = LastUpdatedDay,
                LastEventId = LastEventId,
            };
            if (Collateral != null)
                foreach (var item in Collateral) if (item != null) result.Collateral.Add(item.Clone());
            if (GuaranteeIds != null) result.GuaranteeIds.AddRange(GuaranteeIds);
            return result;
        }

        public void Validate()
        {
            Require(DebtId, "debt id");
            Require(AccountId, "debt account_id");
            Require(DebtorRef, "debt debtor_ref");
            Require(CreditorRef, "debt creditor_ref");
            Require(SourceEventId, "debt source_event_id");
            if (!Enum.IsDefined(typeof(D13FinanceDebtKind), Kind) || !Enum.IsDefined(typeof(D13FinanceDebtLifecycleState), State) ||
                !Enum.IsDefined(typeof(D13FinanceDebtLifecycleState), StateBeforeProtection) ||
                !Enum.IsDefined(typeof(D13FinanceCollectionState), CollectionState))
                throw new InvalidOperationException("Finance debt enum is invalid.");
            if (Principal < 0 || InterestAccrued < 0 || LatePenalty < 0 || InterestRatePercent < 0 || LatePenaltyPercent < 0 ||
                DueDay < 0 || GraceUntilDay < 0 || LastAccrualDay < 0 || LastPaymentDay < 0 || RenegotiationCount < 0 ||
                Revision < 0 || LastUpdatedDay < 0)
                throw new InvalidOperationException("Finance debt number is invalid.");
            if (Collateral == null || GuaranteeIds == null) throw new InvalidOperationException("Finance debt collections are required.");
            foreach (var item in Collateral)
            {
                if (item == null) throw new InvalidOperationException("Finance debt contains null collateral.");
                item.Validate();
            }
            if (string.IsNullOrWhiteSpace(RiskExplanation) || string.IsNullOrWhiteSpace(WorstCaseOutcome))
                throw new InvalidOperationException("Finance debt must explain risk and worst case.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceGuaranteeState
    {
        public string GuaranteeId;
        public string DebtId;
        public string GuarantorRef;
        public long PrincipalCap;
        public D13FinanceGuaranteeStateKind State = D13FinanceGuaranteeStateKind.Active;
        public string SourceEventId;
        public long Revision;
        public int LastUpdatedDay;

        public D13FinanceGuaranteeState Clone()
        {
            return new D13FinanceGuaranteeState
            {
                GuaranteeId = GuaranteeId,
                DebtId = DebtId,
                GuarantorRef = GuarantorRef,
                PrincipalCap = PrincipalCap,
                State = State,
                SourceEventId = SourceEventId,
                Revision = Revision,
                LastUpdatedDay = LastUpdatedDay,
            };
        }

        public void Validate()
        {
            Require(GuaranteeId, "guarantee id");
            Require(DebtId, "guarantee debt_id");
            Require(GuarantorRef, "guarantee guarantor_ref");
            Require(SourceEventId, "guarantee source_event_id");
            if (PrincipalCap <= 0 || !Enum.IsDefined(typeof(D13FinanceGuaranteeStateKind), State) || Revision < 0 || LastUpdatedDay < 0)
                throw new InvalidOperationException("Finance guarantee is invalid.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceEvidenceRecord
    {
        public string EvidenceId;
        public string OwnerRef;
        public D13FinanceEvidenceKind Kind;
        public string FactEventId;
        public long FactSaveRevision;
        public string Description;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13FinanceEvidenceRecord Clone()
        {
            return new D13FinanceEvidenceRecord
            {
                EvidenceId = EvidenceId,
                OwnerRef = OwnerRef,
                Kind = Kind,
                FactEventId = FactEventId,
                FactSaveRevision = FactSaveRevision,
                Description = Description,
                SourceEventId = SourceEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }

        public void Validate()
        {
            Require(EvidenceId, "evidence id");
            Require(OwnerRef, "evidence owner_ref");
            Require(FactEventId, "evidence fact_event_id");
            Require(SourceEventId, "evidence source_event_id");
            if (!Enum.IsDefined(typeof(D13FinanceEvidenceKind), Kind) || FactSaveRevision < 0 || WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException("Finance evidence is invalid.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceInsurancePolicyState
    {
        public string PolicyId;
        public string AccountId;
        public string InsuredRef;
        public D13FinanceInsuranceKind Kind;
        public string AssetRef;
        public long CoverageAmount;
        public long PremiumPaid;
        public long DeductibleAmount;
        public long MaxPayout;
        public long PaidOutAmount;
        public long RemainingCoverage;
        public int StartDay;
        public int EndDay;
        public D13FinancePolicyState State = D13FinancePolicyState.Active;
        public List<string> Exclusions = new List<string>();
        public List<D13FinanceEvidenceKind> RequiredEvidenceKinds = new List<D13FinanceEvidenceKind>();
        public List<string> ClaimIds = new List<string>();
        public string SourceEventId;
        public long Revision;
        public int LastUpdatedDay;
        public string LastEventId;

        public D13FinanceInsurancePolicyState Clone()
        {
            var result = new D13FinanceInsurancePolicyState
            {
                PolicyId = PolicyId,
                AccountId = AccountId,
                InsuredRef = InsuredRef,
                Kind = Kind,
                AssetRef = AssetRef,
                CoverageAmount = CoverageAmount,
                PremiumPaid = PremiumPaid,
                DeductibleAmount = DeductibleAmount,
                MaxPayout = MaxPayout,
                PaidOutAmount = PaidOutAmount,
                RemainingCoverage = RemainingCoverage,
                StartDay = StartDay,
                EndDay = EndDay,
                State = State,
                SourceEventId = SourceEventId,
                Revision = Revision,
                LastUpdatedDay = LastUpdatedDay,
                LastEventId = LastEventId,
            };
            if (Exclusions != null) result.Exclusions.AddRange(Exclusions);
            if (RequiredEvidenceKinds != null) result.RequiredEvidenceKinds.AddRange(RequiredEvidenceKinds);
            if (ClaimIds != null) result.ClaimIds.AddRange(ClaimIds);
            return result;
        }

        public void Validate()
        {
            Require(PolicyId, "insurance policy id");
            Require(AccountId, "insurance policy account_id");
            Require(InsuredRef, "insurance policy insured_ref");
            Require(SourceEventId, "insurance policy source_event_id");
            if (!Enum.IsDefined(typeof(D13FinanceInsuranceKind), Kind) || !Enum.IsDefined(typeof(D13FinancePolicyState), State))
                throw new InvalidOperationException("Insurance policy enum is invalid.");
            if (CoverageAmount <= 0 || PremiumPaid < 0 || DeductibleAmount < 0 || MaxPayout < 0 || PaidOutAmount < 0 ||
                RemainingCoverage < 0 || StartDay < 0 || EndDay < StartDay || Revision < 0 || LastUpdatedDay < 0)
                throw new InvalidOperationException("Insurance policy number or term is invalid.");
            if (PaidOutAmount + RemainingCoverage != MaxPayout)
                throw new InvalidOperationException("Insurance policy payout projection is inconsistent.");
            if (Exclusions == null || RequiredEvidenceKinds == null || ClaimIds == null)
                throw new InvalidOperationException("Insurance policy collections are required.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceInsuranceClaimState
    {
        public string ClaimId;
        public string PolicyId;
        public string IncidentEventId;
        public string IncidentTag;
        public long LossAmount;
        public long ApprovedPayout;
        public long DeniedAmount;
        public D13FinanceClaimState State = D13FinanceClaimState.Submitted;
        public string DecisionCode;
        public string DecisionExplanation;
        public List<string> EvidenceIds = new List<string>();
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13FinanceInsuranceClaimState Clone()
        {
            var result = new D13FinanceInsuranceClaimState
            {
                ClaimId = ClaimId,
                PolicyId = PolicyId,
                IncidentEventId = IncidentEventId,
                IncidentTag = IncidentTag,
                LossAmount = LossAmount,
                ApprovedPayout = ApprovedPayout,
                DeniedAmount = DeniedAmount,
                State = State,
                DecisionCode = DecisionCode,
                DecisionExplanation = DecisionExplanation,
                SourceEventId = SourceEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (EvidenceIds != null) result.EvidenceIds.AddRange(EvidenceIds);
            return result;
        }

        public void Validate()
        {
            Require(ClaimId, "insurance claim id");
            Require(PolicyId, "insurance claim policy_id");
            Require(IncidentEventId, "insurance claim incident_event_id");
            Require(IncidentTag, "insurance claim incident_tag");
            Require(DecisionCode, "insurance claim decision_code");
            Require(DecisionExplanation, "insurance claim decision_explanation");
            Require(SourceEventId, "insurance claim source_event_id");
            if (!Enum.IsDefined(typeof(D13FinanceClaimState), State) || LossAmount <= 0 || ApprovedPayout < 0 || DeniedAmount < 0 ||
                ApprovedPayout + DeniedAmount != LossAmount || WorldDay < 0 || SaveRevision < 0 || EvidenceIds == null)
                throw new InvalidOperationException("Insurance claim is invalid.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceSeizureState
    {
        public string SeizureId;
        public string DebtId;
        public string AssetRef;
        public string Status;
        public string LegalSourceEventId;
        public string SourceEventId;
        public int WorldDay;
        public long SaveRevision;

        public D13FinanceSeizureState Clone()
        {
            return new D13FinanceSeizureState
            {
                SeizureId = SeizureId,
                DebtId = DebtId,
                AssetRef = AssetRef,
                Status = Status,
                LegalSourceEventId = LegalSourceEventId,
                SourceEventId = SourceEventId,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }

        public void Validate()
        {
            Require(SeizureId, "seizure id");
            Require(DebtId, "seizure debt_id");
            Require(AssetRef, "seizure asset_ref");
            Require(Status, "seizure status");
            Require(LegalSourceEventId, "seizure legal_source_event_id");
            Require(SourceEventId, "seizure source_event_id");
            if (WorldDay < 0 || SaveRevision < 0) throw new InvalidOperationException("Finance seizure time is invalid.");
        }

        private static void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(field + " is required.");
        }
    }

    [Serializable]
    public sealed class D13FinanceRuntimeSnapshot
    {
        public const int CurrentSchemaVersion = D13FinanceIds.SchemaVersion;
        public int SchemaVersion = CurrentSchemaVersion;
        public long Revision;
        public List<D13FinanceAccountState> Accounts = new List<D13FinanceAccountState>();
        public List<D13FinanceDebtState> Debts = new List<D13FinanceDebtState>();
        public List<D13FinanceGuaranteeState> Guarantees = new List<D13FinanceGuaranteeState>();
        public List<D13FinanceEvidenceRecord> Evidence = new List<D13FinanceEvidenceRecord>();
        public List<D13FinanceInsurancePolicyState> Policies = new List<D13FinanceInsurancePolicyState>();
        public List<D13FinanceInsuranceClaimState> Claims = new List<D13FinanceInsuranceClaimState>();
        public List<D13FinanceSeizureState> Seizures = new List<D13FinanceSeizureState>();
        public List<string> AppliedEventKeys = new List<string>();

        public D13FinanceRuntimeSnapshot Clone()
        {
            var result = new D13FinanceRuntimeSnapshot { SchemaVersion = SchemaVersion, Revision = Revision };
            if (Accounts != null) foreach (var item in Accounts) if (item != null) result.Accounts.Add(item.Clone());
            if (Debts != null) foreach (var item in Debts) if (item != null) result.Debts.Add(item.Clone());
            if (Guarantees != null) foreach (var item in Guarantees) if (item != null) result.Guarantees.Add(item.Clone());
            if (Evidence != null) foreach (var item in Evidence) if (item != null) result.Evidence.Add(item.Clone());
            if (Policies != null) foreach (var item in Policies) if (item != null) result.Policies.Add(item.Clone());
            if (Claims != null) foreach (var item in Claims) if (item != null) result.Claims.Add(item.Clone());
            if (Seizures != null) foreach (var item in Seizures) if (item != null) result.Seizures.Add(item.Clone());
            if (AppliedEventKeys != null) result.AppliedEventKeys.AddRange(AppliedEventKeys);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion) throw new InvalidOperationException("D13 finance schema_version mismatch.");
            if (Revision < 0) throw new InvalidOperationException("D13 finance revision must be non-negative.");
            if (Accounts == null || Debts == null || Guarantees == null || Evidence == null || Policies == null ||
                Claims == null || Seizures == null || AppliedEventKeys == null)
                throw new InvalidOperationException("D13 finance snapshot collections are required.");
            ValidateUnique(Accounts.Select(item => item == null ? null : item.AccountId), "account");
            ValidateUnique(Debts.Select(item => item == null ? null : item.DebtId), "debt");
            ValidateUnique(Guarantees.Select(item => item == null ? null : item.GuaranteeId), "guarantee");
            ValidateUnique(Evidence.Select(item => item == null ? null : item.EvidenceId), "evidence");
            ValidateUnique(Policies.Select(item => item == null ? null : item.PolicyId), "policy");
            ValidateUnique(Claims.Select(item => item == null ? null : item.ClaimId), "claim");
            ValidateUnique(Seizures.Select(item => item == null ? null : item.SeizureId), "seizure");
            foreach (var item in Accounts) { if (item == null) throw new InvalidOperationException("Null finance account."); item.Validate(); }
            foreach (var item in Debts) { if (item == null) throw new InvalidOperationException("Null finance debt."); item.Validate(); }
            foreach (var item in Guarantees) { if (item == null) throw new InvalidOperationException("Null finance guarantee."); item.Validate(); }
            foreach (var item in Evidence) { if (item == null) throw new InvalidOperationException("Null finance evidence."); item.Validate(); }
            foreach (var item in Policies) { if (item == null) throw new InvalidOperationException("Null insurance policy."); item.Validate(); }
            foreach (var item in Claims) { if (item == null) throw new InvalidOperationException("Null insurance claim."); item.Validate(); }
            foreach (var item in Seizures) { if (item == null) throw new InvalidOperationException("Null finance seizure."); item.Validate(); }
        }

        private static void ValidateUnique(IEnumerable<string> values, string kind)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Finance " + kind + " id is required.");
                if (!seen.Add(value)) throw new InvalidOperationException("Duplicate finance " + kind + ": " + value);
            }
        }
    }

    [Serializable]
    public class D13FinanceOperationRequest
    {
        public string OperationId;
        public string ActorRef = D13FinanceIds.PlayerRef;
        public string LocationRef = "santa_margarita";
        public List<string> CauseEventIds = new List<string>();
        public int WorldDay;
        public long SaveRevision;
    }

    [Serializable]
    public sealed class D13FinanceAccountRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public string OwnerRef = D13FinanceIds.PlayerRef;
        public D13FinanceAccountType AccountType = D13FinanceAccountType.PlayerCashAndCredit;
        public long InitialCash;
        public long CreditLimit;
        public string SourceOwnerSystem = D13FinanceIds.EconomyOwner;
        public string SourceEventId;
    }

    [Serializable]
    public sealed class D13FinanceCashRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public long Amount;
        public D13FinanceCashOperation Operation;
        public string SourceOwnerSystem = D13FinanceIds.EconomyOwner;
        public string SourceEventId;
        public string Description;
    }

    [Serializable]
    public class D13FinanceLoanRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public string DebtorRef = D13FinanceIds.PlayerRef;
        public string CreditorRef;
        public D13FinanceDebtKind Kind = D13FinanceDebtKind.Loan;
        public long Principal;
        public int TermDays;
        public string LedgerRef;
        public string LegalInstrumentRef;
        public string GuarantorRef;
        public long GuaranteeCap;
        public List<D13FinanceCollateralRequest> Collateral = new List<D13FinanceCollateralRequest>();
        public string SourceOwnerSystem = D13FinanceIds.EconomyOwner;
        public string SourceEventId;
    }

    [Serializable]
    public sealed class D13FinanceCreditRequest : D13FinanceLoanRequest
    {
        public string MerchantOrderRef;
    }

    [Serializable]
    public sealed class D13FinanceCollateralRequest
    {
        public string AssetRef;
        public string AssetKind = "property";
        public string RequiredLegalInstrumentRef;
    }

    [Serializable]
    public sealed class D13FinanceRepayRequest : D13FinanceOperationRequest
    {
        public string DebtId;
        public string AccountId;
        public long Amount;
    }

    [Serializable]
    public sealed class D13FinanceAccrueRequest : D13FinanceOperationRequest
    {
        public string DebtId;
    }

    [Serializable]
    public sealed class D13FinanceDueRequest : D13FinanceOperationRequest
    {
        public string DebtId;
    }

    [Serializable]
    public sealed class D13FinanceRenewRequest : D13FinanceOperationRequest
    {
        public string DebtId;
        public int TermDays;
        public List<D13FinanceCollateralRequest> NewCollateral = new List<D13FinanceCollateralRequest>();
    }

    [Serializable]
    public sealed class D13FinanceGuaranteeRequest : D13FinanceOperationRequest
    {
        public string DebtId;
        public string GuarantorRef;
        public long PrincipalCap;
        public string LegalInstrumentRef;
    }

    [Serializable]
    public class D13FinanceInsuranceQuoteRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public string InsuredRef = D13FinanceIds.PlayerRef;
        public D13FinanceInsuranceKind Kind;
        public string AssetRef;
        public long CoverageAmount;
        public int TermDays;
    }

    [Serializable]
    public sealed class D13FinanceInsuranceRequest : D13FinanceInsuranceQuoteRequest
    {
        public string SourceOwnerSystem = D13FinanceIds.EconomyOwner;
        public string SourceEventId;
    }

    [Serializable]
    public sealed class D13FinanceEvidenceRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public string EvidenceId;
        public D13FinanceEvidenceKind Kind;
        public string FactEventId;
        public long FactSaveRevision;
        public string Description;
    }

    [Serializable]
    public sealed class D13FinanceClaimRequest : D13FinanceOperationRequest
    {
        public string PolicyId;
        public string IncidentEventId;
        public string IncidentTag;
        public long LossAmount;
        public List<string> EvidenceIds = new List<string>();
    }

    [Serializable]
    public sealed class D13FinanceCollectionRequest : D13FinanceOperationRequest
    {
        public string DebtId;
        public string CreditorRef;
        public D13FinanceCollectionAction Action;
        public string AssetRef;
    }

    [Serializable]
    public sealed class D13FinanceBankruptcyRequest : D13FinanceOperationRequest
    {
        public string AccountId;
        public string DebtorRef = D13FinanceIds.PlayerRef;
    }

    [Serializable]
    public sealed class D13FinanceCollateralDecision
    {
        public bool Allowed;
        public string Code = "collateral_denied";
        public string Message;
        public long ClaimValue;
        public string ValuationSourceOwnerSystem = D13FinanceIds.EconomyOwner;
        public string ValuationSourceEventId;
        public string LegalSourceOwnerSystem = D13FinanceIds.LegalOwner;
        public string LegalSourceEventId;
    }

    [Serializable]
    public sealed class D13FinanceSeizureDecision
    {
        public bool Allowed;
        public string Code = "seizure_denied";
        public string Message;
        public string LegalSourceOwnerSystem = D13FinanceIds.LegalOwner;
        public string LegalSourceEventId;
    }

    public interface ID13FinanceCollateralAuthority
    {
        D13FinanceCollateralDecision ValidateCollateral(D13FinanceCollateralRequest request,
            string debtorRef, int worldDay);

        D13FinanceSeizureDecision AuthorizeSeizure(D13FinanceDebtState debt,
            D13FinanceCollateralState collateral, D13FinanceCollectionRequest request);
    }

    [Serializable]
    public sealed class D13InsuranceQuote
    {
        public string QuoteId;
        public D13FinanceInsuranceKind Kind;
        public string AssetRef;
        public long CoverageAmount;
        public long Premium;
        public long DeductibleAmount;
        public long MaxPayout;
        public int StartDay;
        public int EndDay;
        public List<string> Exclusions = new List<string>();
        public List<D13FinanceEvidenceKind> RequiredEvidenceKinds = new List<D13FinanceEvidenceKind>();
        public string RiskExplanation;
        public string WorstCaseOutcome;
        public string SourceEventId;

        public D13InsuranceQuote Clone()
        {
            var result = new D13InsuranceQuote
            {
                QuoteId = QuoteId,
                Kind = Kind,
                AssetRef = AssetRef,
                CoverageAmount = CoverageAmount,
                Premium = Premium,
                DeductibleAmount = DeductibleAmount,
                MaxPayout = MaxPayout,
                StartDay = StartDay,
                EndDay = EndDay,
                RiskExplanation = RiskExplanation,
                WorstCaseOutcome = WorstCaseOutcome,
                SourceEventId = SourceEventId,
            };
            if (Exclusions != null) result.Exclusions.AddRange(Exclusions);
            if (RequiredEvidenceKinds != null) result.RequiredEvidenceKinds.AddRange(RequiredEvidenceKinds);
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(QuoteId) || string.IsNullOrWhiteSpace(RiskExplanation) ||
                string.IsNullOrWhiteSpace(WorstCaseOutcome) || string.IsNullOrWhiteSpace(SourceEventId))
                throw new InvalidOperationException("Insurance quote explanation and source are required.");
            if (!Enum.IsDefined(typeof(D13FinanceInsuranceKind), Kind) || CoverageAmount <= 0 || Premium < 0 ||
                DeductibleAmount < 0 || MaxPayout < 0 || StartDay < 0 || EndDay < StartDay ||
                Exclusions == null || RequiredEvidenceKinds == null)
                throw new InvalidOperationException("Insurance quote is invalid.");
        }
    }

    [Serializable]
    public sealed class D13FinanceRiskExplanation
    {
        public string SubjectId;
        public string SubjectKind;
        public long Principal;
        public long Interest;
        public long Fees;
        public long CurrentDue;
        public string Risk;
        public string WorstCaseOutcome;
        public string RecoveryRoute;

        public D13FinanceRiskExplanation Clone()
        {
            return new D13FinanceRiskExplanation
            {
                SubjectId = SubjectId,
                SubjectKind = SubjectKind,
                Principal = Principal,
                Interest = Interest,
                Fees = Fees,
                CurrentDue = CurrentDue,
                Risk = Risk,
                WorstCaseOutcome = WorstCaseOutcome,
                RecoveryRoute = RecoveryRoute,
            };
        }
    }

    [Serializable]
    public sealed class D13FinanceReadModel
    {
        public long Revision;
        public List<D13FinanceAccountState> Accounts = new List<D13FinanceAccountState>();
        public List<D13FinanceDebtState> Debts = new List<D13FinanceDebtState>();
        public List<D13FinanceGuaranteeState> Guarantees = new List<D13FinanceGuaranteeState>();
        public List<D13FinanceEvidenceRecord> Evidence = new List<D13FinanceEvidenceRecord>();
        public List<D13FinanceInsurancePolicyState> Policies = new List<D13FinanceInsurancePolicyState>();
        public List<D13FinanceInsuranceClaimState> Claims = new List<D13FinanceInsuranceClaimState>();
        public List<D13FinanceSeizureState> Seizures = new List<D13FinanceSeizureState>();
        public List<D13FinanceRiskExplanation> Risks = new List<D13FinanceRiskExplanation>();

        public D13FinanceReadModel Clone()
        {
            var result = new D13FinanceReadModel { Revision = Revision };
            if (Accounts != null) foreach (var item in Accounts) if (item != null) result.Accounts.Add(item.Clone());
            if (Debts != null) foreach (var item in Debts) if (item != null) result.Debts.Add(item.Clone());
            if (Guarantees != null) foreach (var item in Guarantees) if (item != null) result.Guarantees.Add(item.Clone());
            if (Evidence != null) foreach (var item in Evidence) if (item != null) result.Evidence.Add(item.Clone());
            if (Policies != null) foreach (var item in Policies) if (item != null) result.Policies.Add(item.Clone());
            if (Claims != null) foreach (var item in Claims) if (item != null) result.Claims.Add(item.Clone());
            if (Seizures != null) foreach (var item in Seizures) if (item != null) result.Seizures.Add(item.Clone());
            if (Risks != null) foreach (var item in Risks) if (item != null) result.Risks.Add(item.Clone());
            return result;
        }
    }

    [Serializable]
    public sealed class D13FinanceResult
    {
        public bool Succeeded;
        public bool Duplicate;
        public string Code;
        public string Message;
        public string EventId;
        public FactEvent Fact;
        public D13FinanceAccountState Account;
        public D13FinanceDebtState Debt;
        public D13FinanceGuaranteeState Guarantee;
        public D13FinanceEvidenceRecord Evidence;
        public D13FinanceInsurancePolicyState Policy;
        public D13FinanceInsuranceClaimState Claim;
        public D13FinanceSeizureState Seizure;
        public D13InsuranceQuote Quote;
        public long CashAfter;
        public long PrincipalPaid;
        public long InterestPaid;
        public long Fee;
        public long InterestAdded;
        public string RiskExplanation;
        public string WorstCaseOutcome;
        public string RecoveryRouteId;

        public D13FinanceResult Clone()
        {
            return new D13FinanceResult
            {
                Succeeded = Succeeded,
                Duplicate = Duplicate,
                Code = Code,
                Message = Message,
                EventId = EventId,
                Fact = Fact == null ? null : Fact.CloneFact(),
                Account = Account == null ? null : Account.Clone(),
                Debt = Debt == null ? null : Debt.Clone(),
                Guarantee = Guarantee == null ? null : Guarantee.Clone(),
                Evidence = Evidence == null ? null : Evidence.Clone(),
                Policy = Policy == null ? null : Policy.Clone(),
                Claim = Claim == null ? null : Claim.Clone(),
                Seizure = Seizure == null ? null : Seizure.Clone(),
                Quote = Quote == null ? null : Quote.Clone(),
                CashAfter = CashAfter,
                PrincipalPaid = PrincipalPaid,
                InterestPaid = InterestPaid,
                Fee = Fee,
                InterestAdded = InterestAdded,
                RiskExplanation = RiskExplanation,
                WorstCaseOutcome = WorstCaseOutcome,
                RecoveryRouteId = RecoveryRouteId,
            };
        }
    }
}
