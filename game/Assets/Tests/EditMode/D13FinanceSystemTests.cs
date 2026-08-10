// D13-A02 金融测试：覆盖现金、赊账、贷款、利息、展期、保险、拒赔、催收、查封、破产保护和存档幂等。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13FinanceSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_finance_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void LoanInterestRenewalDefaultCollectionSeizureAndBankruptcyAreDeterministic()
        {
            D13FinanceConfiguration.ThrowIfInvalid();
            var eventLog = new EventLog();
            AddSource(eventLog, "economy_open", 1, "EconomySystem", 0);
            AddSource(eventLog, "economy_collateral", 2, "EconomySystem", 0);
            AddSource(eventLog, "legal_collateral", 3, "FactionLegalSystem", 0);
            AddSource(eventLog, "economy_loan", 4, "EconomySystem", 0);
            AddSource(eventLog, "legal_seizure", 5, "FactionLegalSystem", 0);
            var system = new D13FinanceSystem(eventLog, new FixtureCollateralAuthority());

            var accountResult = system.OpenAccount(new D13FinanceAccountRequest
            {
                AccountId = "account_player",
                OwnerRef = D13FinanceIds.PlayerRef,
                InitialCash = 100,
                CreditLimit = 1000,
                SourceEventId = "economy_open",
                OperationId = "open_player_account",
                WorldDay = 0,
                SaveRevision = 10,
            });
            Assert.IsTrue(accountResult.Succeeded, accountResult.Message);

            var loanRequest = new D13FinanceLoanRequest
            {
                AccountId = "account_player",
                DebtorRef = D13FinanceIds.PlayerRef,
                CreditorRef = "merchant_bank_saint_marys",
                Kind = D13FinanceDebtKind.PropertyMortgage,
                Principal = 300,
                TermDays = 30,
                GuarantorRef = "guarantor_maria",
                GuaranteeCap = 120,
                SourceEventId = "economy_loan",
                OperationId = "warehouse_mortgage_loan",
                WorldDay = 1,
                SaveRevision = 11,
                Collateral = new List<D13FinanceCollateralRequest>
                {
                    new D13FinanceCollateralRequest
                    {
                        AssetRef = "property_warehouse_santa_marys_north_store",
                        AssetKind = "warehouse",
                    },
                },
            };
            var loan = system.RequestLoan(loanRequest);
            Assert.IsTrue(loan.Succeeded, loan.Message);
            Assert.AreEqual(D13FinanceDebtKind.PropertyMortgage, loan.Debt.Kind);
            Assert.AreEqual(400, loan.Account.CashBalance);
            Assert.AreEqual("economy_loan", loan.Debt.SourceEventId);
            Assert.IsTrue(loan.RiskExplanation.Contains("利率"));
            Assert.IsTrue(loan.WorstCaseOutcome.Contains("查封"));
            Assert.IsNotNull(loan.Guarantee);

            var interest = system.AccrueInterest(new D13FinanceAccrueRequest
            {
                DebtId = loan.Debt.DebtId,
                OperationId = "warehouse_interest_day10",
                WorldDay = 10,
                SaveRevision = 12,
            });
            Assert.IsTrue(interest.Succeeded, interest.Message);
            Assert.Greater(interest.InterestAdded, 0);
            Assert.Greater(interest.Debt.InterestAccrued, 0);

            var renewal = system.Renew(new D13FinanceRenewRequest
            {
                DebtId = loan.Debt.DebtId,
                OperationId = "warehouse_renewal",
                TermDays = 15,
                WorldDay = 20,
                SaveRevision = 13,
            });
            Assert.IsTrue(renewal.Succeeded, renewal.Message);
            Assert.AreEqual(D13FinanceDebtLifecycleState.Extended, renewal.Debt.State);
            Assert.Greater(renewal.Fee, 0);
            Assert.IsTrue(renewal.RiskExplanation.Contains("展期费用"));

            var credit = system.RequestCredit(new D13FinanceCreditRequest
            {
                AccountId = "account_player",
                DebtorRef = D13FinanceIds.PlayerRef,
                CreditorRef = "merchant_ropewalk",
                Principal = 40,
                SourceEventId = "economy_loan",
                OperationId = "ropewalk_credit",
                WorldDay = 25,
                SaveRevision = 14,
            });
            Assert.IsTrue(credit.Succeeded, credit.Message);
            Assert.AreEqual(D13FinanceDebtKind.MerchantCredit, credit.Debt.Kind);
            Assert.AreEqual(400, credit.Account.CashBalance, "赊账不能直接增加现金。");

            var protection = system.RequestBankruptcyProtection(new D13FinanceBankruptcyRequest
            {
                AccountId = "account_player",
                DebtorRef = D13FinanceIds.PlayerRef,
                OperationId = "bankruptcy_protection",
                WorldDay = 25,
                SaveRevision = 15,
            });
            Assert.IsTrue(protection.Succeeded, protection.Message);
            Assert.Greater(protection.Account.BankruptcyProtectionUntilDay, 25);
            Assert.IsTrue(system.FindDebt(loan.Debt.DebtId).State == D13FinanceDebtLifecycleState.BankruptcyProtected);

            var release = system.ReleaseBankruptcyProtection(new D13FinanceBankruptcyRequest
            {
                AccountId = "account_player",
                DebtorRef = D13FinanceIds.PlayerRef,
                OperationId = "bankruptcy_release",
                WorldDay = protection.Account.BankruptcyProtectionUntilDay,
                SaveRevision = 16,
            });
            Assert.IsTrue(release.Succeeded, release.Message);
            Assert.AreEqual(0, release.Account.BankruptcyProtectionUntilDay);

            var due = system.EvaluateDue(new D13FinanceDueRequest
            {
                DebtId = loan.Debt.DebtId,
                OperationId = "warehouse_default",
                WorldDay = 55,
                SaveRevision = 17,
            });
            Assert.IsTrue(due.Succeeded, due.Message);
            Assert.AreEqual(D13FinanceDebtLifecycleState.Defaulted, due.Debt.State);
            Assert.Greater(due.Debt.LatePenalty, 0);
            Assert.Less(due.Account.CreditScore, accountResult.Account.CreditScore);

            var notice = system.Collect(new D13FinanceCollectionRequest
            {
                DebtId = loan.Debt.DebtId,
                CreditorRef = "merchant_bank_saint_marys",
                Action = D13FinanceCollectionAction.IssueNotice,
                OperationId = "warehouse_collection_notice",
                WorldDay = 56,
                SaveRevision = 18,
            });
            Assert.IsTrue(notice.Succeeded, notice.Message);
            Assert.AreEqual(D13FinanceDebtLifecycleState.Collection, notice.Debt.State);
            Assert.Greater(notice.Fee, 0);

            var seizure = system.RequestAssetSeizure(new D13FinanceCollectionRequest
            {
                DebtId = loan.Debt.DebtId,
                CreditorRef = "merchant_bank_saint_marys",
                AssetRef = "property_warehouse_santa_marys_north_store",
                OperationId = "warehouse_seizure",
                WorldDay = 57,
                SaveRevision = 19,
            });
            Assert.IsTrue(seizure.Succeeded, seizure.Message);
            Assert.AreEqual(D13FinanceDebtLifecycleState.Seized, seizure.Debt.State);
            Assert.AreEqual("approved_pending_property_owner", seizure.Seizure.Status);
            Assert.AreEqual("legal_seizure", seizure.Seizure.LegalSourceEventId);

            var snapshot = system.CaptureSnapshot();
            snapshot.Validate();
            var restoredLog = new EventLog(eventLog.CaptureSnapshot());
            var restored = new D13FinanceSystem(restoredLog, new FixtureCollateralAuthority(), snapshot);
            var duplicate = restored.RequestAssetSeizure(new D13FinanceCollectionRequest
            {
                DebtId = loan.Debt.DebtId,
                CreditorRef = "merchant_bank_saint_marys",
                AssetRef = "property_warehouse_santa_marys_north_store",
                OperationId = "warehouse_seizure",
                WorldDay = 57,
                SaveRevision = 19,
            });
            Assert.IsTrue(duplicate.Duplicate, "读档后重复查封不能重复结算。");
            Assert.AreEqual(eventLog.Count, restoredLog.Count);
        }

        [Test]
        public void InsuranceEvidenceDenialPartialPayoutAndSaveRestoreAreTraceable()
        {
            var eventLog = new EventLog();
            AddSource(eventLog, "economy_open_insurance", 1, "EconomySystem", 0);
            AddSource(eventLog, "economy_premium", 2, "EconomySystem", 0);
            AddSource(eventLog, "event_storm_incident", 3, "EnvironmentSystem", 10);
            var system = new D13FinanceSystem(eventLog, new FixtureCollateralAuthority());
            Assert.IsTrue(system.OpenAccount(new D13FinanceAccountRequest
            {
                AccountId = "insurance_account",
                OwnerRef = D13FinanceIds.PlayerRef,
                InitialCash = 1000,
                CreditLimit = 200,
                SourceEventId = "economy_open_insurance",
                OperationId = "open_insurance_account",
                WorldDay = 0,
                SaveRevision = 10,
            }).Succeeded);

            var quote = system.QuoteInsurance(new D13FinanceInsuranceQuoteRequest
            {
                AccountId = "insurance_account",
                InsuredRef = D13FinanceIds.PlayerRef,
                Kind = D13FinanceInsuranceKind.Cargo,
                AssetRef = "cargo_hold_01",
                CoverageAmount = 500,
                TermDays = 30,
                OperationId = "cargo_quote",
                WorldDay = 10,
                SaveRevision = 11,
            });
            Assert.IsTrue(quote.Succeeded, quote.Message);
            Assert.Greater(quote.Quote.Premium, 0);
            Assert.Greater(quote.Quote.DeductibleAmount, 0);
            Assert.IsTrue(quote.Quote.WorstCaseOutcome.Contains("0"));

            var policy = system.BuyInsurance(new D13FinanceInsuranceRequest
            {
                AccountId = "insurance_account",
                InsuredRef = D13FinanceIds.PlayerRef,
                Kind = D13FinanceInsuranceKind.Cargo,
                AssetRef = "cargo_hold_01",
                CoverageAmount = 500,
                TermDays = 30,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner,
                SourceEventId = "economy_premium",
                OperationId = "cargo_policy",
                WorldDay = 10,
                SaveRevision = 12,
            });
            Assert.IsTrue(policy.Succeeded, policy.Message);

            var evidenceOne = system.RegisterEvidence(new D13FinanceEvidenceRequest
            {
                AccountId = "insurance_account",
                EvidenceId = "evidence_storm_incident",
                Kind = D13FinanceEvidenceKind.Incident,
                FactEventId = "event_storm_incident",
                FactSaveRevision = 10,
                Description = "风暴事实记录",
                OperationId = "register_incident_evidence",
                WorldDay = 10,
                SaveRevision = 13,
            });
            Assert.IsTrue(evidenceOne.Succeeded, evidenceOne.Message);

            var denied = system.SubmitClaim(new D13FinanceClaimRequest
            {
                PolicyId = policy.Policy.PolicyId,
                IncidentEventId = "event_storm_incident",
                IncidentTag = "storm",
                LossAmount = 500,
                EvidenceIds = new List<string> { "evidence_storm_incident" },
                OperationId = "cargo_claim_missing_manifest",
                WorldDay = 11,
                SaveRevision = 14,
            });
            Assert.IsFalse(denied.Succeeded);
            Assert.AreEqual(D13FinanceClaimState.Denied, denied.Claim.State);
            Assert.AreEqual(0, denied.Claim.ApprovedPayout);
            Assert.IsTrue(denied.Fact.OutcomeStatus == WorldEventStatus.Failed);
            Assert.IsNotNull(denied.Fact.RecoveryRoute);

            var evidenceTwo = system.RegisterEvidence(new D13FinanceEvidenceRequest
            {
                AccountId = "insurance_account",
                EvidenceId = "evidence_manifest",
                Kind = D13FinanceEvidenceKind.Manifest,
                FactEventId = "event_storm_incident",
                FactSaveRevision = 10,
                Description = "货物清单",
                OperationId = "register_manifest_evidence",
                WorldDay = 11,
                SaveRevision = 15,
            });
            Assert.IsTrue(evidenceTwo.Succeeded, evidenceTwo.Message);

            var claim = system.SubmitClaim(new D13FinanceClaimRequest
            {
                PolicyId = policy.Policy.PolicyId,
                IncidentEventId = "event_storm_incident",
                IncidentTag = "storm",
                LossAmount = 500,
                EvidenceIds = new List<string> { "evidence_storm_incident", "evidence_manifest" },
                OperationId = "cargo_claim_with_evidence",
                WorldDay = 12,
                SaveRevision = 16,
            });
            Assert.IsTrue(claim.Succeeded, claim.Message);
            Assert.AreEqual(D13FinanceClaimState.PartiallyPaid, claim.Claim.State);
            Assert.Greater(claim.Claim.ApprovedPayout, 0);
            Assert.Greater(claim.Claim.DeniedAmount, 0, "免赔额/保额上限造成的未赔部分必须可见。");
            Assert.IsTrue(claim.Fact.Result.Values.ContainsKey("denied_amount"));

            SaveSystem.NewGame(new NewGameParams { CaptainName = "金融测试船长" });
            SaveSystem.SaveD13FinanceRuntime(system.CaptureSnapshot());
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var saved = SaveSystem.LoadD13FinanceRuntime();
            Assert.IsNotNull(saved);
            Assert.AreEqual(2, saved.Claims.Count);
            Assert.AreEqual(claim.Claim.ApprovedPayout,
                saved.Claims.Single(item => item.ClaimId == claim.Claim.ClaimId).ApprovedPayout);

            var restoredLog = new EventLog(eventLog.CaptureSnapshot());
            var restored = new D13FinanceSystem(restoredLog, new FixtureCollateralAuthority(), saved);
            var duplicate = restored.SubmitClaim(new D13FinanceClaimRequest
            {
                PolicyId = policy.Policy.PolicyId,
                IncidentEventId = "event_storm_incident",
                IncidentTag = "storm",
                LossAmount = 500,
                EvidenceIds = new List<string> { "evidence_storm_incident", "evidence_manifest" },
                OperationId = "cargo_claim_with_evidence",
                WorldDay = 12,
                SaveRevision = 16,
            });
            Assert.IsTrue(duplicate.Duplicate);
            Assert.AreEqual(eventLog.Count, restoredLog.Count);
        }

        [Test]
        public void InvalidSourceAndInsufficientCashLeaveCanonicalStateUnchanged()
        {
            var eventLog = new EventLog();
            var system = new D13FinanceSystem(eventLog, new FixtureCollateralAuthority());
            var denied = system.OpenAccount(new D13FinanceAccountRequest
            {
                AccountId = "bad_account",
                OwnerRef = D13FinanceIds.PlayerRef,
                InitialCash = 500,
                CreditLimit = 100,
                SourceOwnerSystem = "Agent",
                SourceEventId = "not_authority",
                OperationId = "bad_open",
                SaveRevision = 1,
            });
            Assert.IsFalse(denied.Succeeded);
            Assert.IsNull(system.FindAccount("bad_account"));
            Assert.IsNotNull(denied.Fact);
            Assert.AreEqual(WorldEventStatus.Failed, denied.Fact.OutcomeStatus);

            AddSource(eventLog, "economy_valid_open", 2, "EconomySystem", 0);
            var account = system.OpenAccount(new D13FinanceAccountRequest
            {
                AccountId = "cash_account",
                OwnerRef = D13FinanceIds.PlayerRef,
                InitialCash = 50,
                CreditLimit = 0,
                SourceEventId = "economy_valid_open",
                OperationId = "valid_open",
                SaveRevision = 3,
            });
            Assert.IsTrue(account.Succeeded);
            var spend = system.Spend(new D13FinanceCashRequest
            {
                AccountId = "cash_account",
                Amount = 51,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner,
                SourceEventId = "economy_valid_open",
                OperationId = "overspend",
                SaveRevision = 4,
            });
            Assert.IsFalse(spend.Succeeded);
            Assert.AreEqual(50, system.FindAccount("cash_account").CashBalance);
        }

        private static void AddSource(EventLog eventLog, string eventId, long saveRevision, string owner, int worldDay)
        {
            eventLog.Append(new WorldEvent(eventId, worldDay, "santa_margarita", owner,
                new EventResult
                {
                    ResultCode = "fixture_source",
                    Summary = "fixture source",
                    FactIds = new List<string> { "fixture:" + eventId },
                    Values = new Dictionary<string, string> { ["source_owner_system"] = owner },
                },
                new[] { "fixture" }, new[] { "fixture:input" }, new string[0], saveRevision));
        }

        private sealed class FixtureCollateralAuthority : ID13FinanceCollateralAuthority
        {
            public D13FinanceCollateralDecision ValidateCollateral(D13FinanceCollateralRequest request,
                string debtorRef, int worldDay)
            {
                return new D13FinanceCollateralDecision
                {
                    Allowed = true,
                    Code = "collateral_approved",
                    Message = "fixture collateral approved",
                    ClaimValue = 1000,
                    ValuationSourceOwnerSystem = D13FinanceIds.EconomyOwner,
                    ValuationSourceEventId = "economy_collateral",
                    LegalSourceOwnerSystem = D13FinanceIds.LegalOwner,
                    LegalSourceEventId = "legal_collateral",
                };
            }

            public D13FinanceSeizureDecision AuthorizeSeizure(D13FinanceDebtState debt,
                D13FinanceCollateralState collateral, D13FinanceCollectionRequest request)
            {
                return new D13FinanceSeizureDecision
                {
                    Allowed = true,
                    Code = "seizure_approved",
                    Message = "fixture legal authority approved seizure request",
                    LegalSourceOwnerSystem = D13FinanceIds.LegalOwner,
                    LegalSourceEventId = "legal_seizure",
                };
            }
        }
    }
}
