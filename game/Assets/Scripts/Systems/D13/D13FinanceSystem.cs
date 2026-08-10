// D13-A02 FinanceSystem。
// 账户、债务、利息、保险、信用和金融风险投影的唯一写者。
// 不写 PropertyState、D3 货物或 D3 现金兼容镜像；外部 authority 只提供
// Economy/Property/Legal 的来源化输入，真实资产查封由法律/产权 owner 执行。

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Whalebone.Game.Config;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D13
{
    public sealed class D13FinanceSystem
    {
        private readonly Dictionary<string, D13FinanceAccountState> _accounts =
            new Dictionary<string, D13FinanceAccountState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceDebtState> _debts =
            new Dictionary<string, D13FinanceDebtState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceGuaranteeState> _guarantees =
            new Dictionary<string, D13FinanceGuaranteeState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceEvidenceRecord> _evidence =
            new Dictionary<string, D13FinanceEvidenceRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceInsurancePolicyState> _policies =
            new Dictionary<string, D13FinanceInsurancePolicyState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceInsuranceClaimState> _claims =
            new Dictionary<string, D13FinanceInsuranceClaimState>(StringComparer.Ordinal);
        private readonly Dictionary<string, D13FinanceSeizureState> _seizures =
            new Dictionary<string, D13FinanceSeizureState>(StringComparer.Ordinal);
        private readonly EventLog _eventLog;
        private readonly ID13FinanceCollateralAuthority _collateralAuthority;
        private long _revision;

        public D13FinanceSystem(EventLog eventLog = null,
            ID13FinanceCollateralAuthority collateralAuthority = null,
            D13FinanceRuntimeSnapshot initial = null)
        {
            D13FinanceConfiguration.ThrowIfInvalid();
            _eventLog = eventLog ?? new EventLog();
            _collateralAuthority = collateralAuthority ?? new UnavailableCollateralAuthority();
            if (initial != null) Restore(initial);
        }

        public EventLog EventLog { get { return _eventLog; } }
        public long Revision { get { return _revision; } }

        public D13FinanceAccountState FindAccount(string accountId)
        {
            D13FinanceAccountState result;
            return _accounts.TryGetValue(accountId ?? string.Empty, out result) ? result.Clone() : null;
        }

        public D13FinanceDebtState FindDebt(string debtId)
        {
            D13FinanceDebtState result;
            return _debts.TryGetValue(debtId ?? string.Empty, out result) ? result.Clone() : null;
        }

        public D13FinanceInsurancePolicyState FindPolicy(string policyId)
        {
            D13FinanceInsurancePolicyState result;
            return _policies.TryGetValue(policyId ?? string.Empty, out result) ? result.Clone() : null;
        }

        public D13FinanceInsuranceClaimState FindClaim(string claimId)
        {
            D13FinanceInsuranceClaimState result;
            return _claims.TryGetValue(claimId ?? string.Empty, out result) ? result.Clone() : null;
        }

        /// <summary>UI/Agent 只读入口；所有风险、费用和最坏结果都由 FinanceSystem 生成。</summary>
        public D13FinanceReadModel GetReadModel()
        {
            var result = new D13FinanceReadModel { Revision = _revision };
            foreach (var item in _accounts.Values.OrderBy(value => value.AccountId, StringComparer.Ordinal)) result.Accounts.Add(item.Clone());
            foreach (var item in _debts.Values.OrderBy(value => value.DebtId, StringComparer.Ordinal))
            {
                result.Debts.Add(item.Clone());
                result.Risks.Add(new D13FinanceRiskExplanation
                {
                    SubjectId = item.DebtId,
                    SubjectKind = "debt",
                    Principal = item.Principal,
                    Interest = item.InterestAccrued,
                    Fees = item.LatePenalty,
                    CurrentDue = item.TotalDue,
                    Risk = item.RiskExplanation,
                    WorstCaseOutcome = item.WorstCaseOutcome,
                    RecoveryRoute = D13FinanceIds.DebtRecoveryRoute,
                });
            }
            foreach (var item in _guarantees.Values.OrderBy(value => value.GuaranteeId, StringComparer.Ordinal)) result.Guarantees.Add(item.Clone());
            foreach (var item in _evidence.Values.OrderBy(value => value.EvidenceId, StringComparer.Ordinal)) result.Evidence.Add(item.Clone());
            foreach (var item in _policies.Values.OrderBy(value => value.PolicyId, StringComparer.Ordinal))
            {
                result.Policies.Add(item.Clone());
                result.Risks.Add(new D13FinanceRiskExplanation
                {
                    SubjectId = item.PolicyId,
                    SubjectKind = "insurance",
                    Principal = item.CoverageAmount,
                    Interest = 0,
                    Fees = item.PremiumPaid + item.DeductibleAmount,
                    CurrentDue = item.RemainingCoverage,
                    Risk = "保费 " + item.PremiumPaid + "；免赔额 " + item.DeductibleAmount + "；必须提交已登记证据。",
                    WorstCaseOutcome = "不满足证据、承保范围或排除条款时，赔付为 0；已付保费不自动退回。",
                    RecoveryRoute = D13FinanceIds.InsuranceEvidenceRecoveryRoute,
                });
            }
            foreach (var item in _claims.Values.OrderBy(value => value.ClaimId, StringComparer.Ordinal)) result.Claims.Add(item.Clone());
            foreach (var item in _seizures.Values.OrderBy(value => value.SeizureId, StringComparer.Ordinal)) result.Seizures.Add(item.Clone());
            return result;
        }

        public D13FinanceRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13FinanceRuntimeSnapshot { Revision = _revision };
            foreach (var item in _accounts.Values) result.Accounts.Add(item.Clone());
            foreach (var item in _debts.Values) result.Debts.Add(item.Clone());
            foreach (var item in _guarantees.Values) result.Guarantees.Add(item.Clone());
            foreach (var item in _evidence.Values) result.Evidence.Add(item.Clone());
            foreach (var item in _policies.Values) result.Policies.Add(item.Clone());
            foreach (var item in _claims.Values) result.Claims.Add(item.Clone());
            foreach (var item in _seizures.Values) result.Seizures.Add(item.Clone());
            foreach (var fact in _eventLog.Facts)
                if (fact != null && fact.EventId.StartsWith("event_d13_finance_", StringComparison.Ordinal))
                    result.AppliedEventKeys.Add(fact.DedupeKey);
            return result;
        }

        public void Restore(D13FinanceRuntimeSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            _accounts.Clear();
            _debts.Clear();
            _guarantees.Clear();
            _evidence.Clear();
            _policies.Clear();
            _claims.Clear();
            _seizures.Clear();
            foreach (var item in snapshot.Accounts) _accounts[item.AccountId] = item.Clone();
            foreach (var item in snapshot.Debts)
            {
                if (!_accounts.ContainsKey(item.AccountId)) throw new InvalidOperationException("Finance debt references unknown account: " + item.DebtId);
                _debts[item.DebtId] = item.Clone();
            }
            foreach (var item in snapshot.Guarantees)
            {
                if (!_debts.ContainsKey(item.DebtId)) throw new InvalidOperationException("Finance guarantee references unknown debt: " + item.GuaranteeId);
                _guarantees[item.GuaranteeId] = item.Clone();
            }
            foreach (var item in snapshot.Evidence)
            {
                if (!_accounts.ContainsKey(item.OwnerRef)) throw new InvalidOperationException("Finance evidence references unknown owner: " + item.EvidenceId);
                _evidence[item.EvidenceId] = item.Clone();
            }
            foreach (var item in snapshot.Policies)
            {
                if (!_accounts.ContainsKey(item.AccountId)) throw new InvalidOperationException("Insurance policy references unknown account: " + item.PolicyId);
                _policies[item.PolicyId] = item.Clone();
            }
            foreach (var item in snapshot.Claims)
            {
                if (!_policies.ContainsKey(item.PolicyId)) throw new InvalidOperationException("Insurance claim references unknown policy: " + item.ClaimId);
                _claims[item.ClaimId] = item.Clone();
            }
            foreach (var item in snapshot.Seizures)
            {
                if (!_debts.ContainsKey(item.DebtId)) throw new InvalidOperationException("Finance seizure references unknown debt: " + item.SeizureId);
                _seizures[item.SeizureId] = item.Clone();
            }
            _revision = snapshot.Revision;
        }

        public D13FinanceResult OpenAccount(D13FinanceAccountRequest request)
        {
            if (request == null) return Invalid("invalid_request", "金融账户请求不存在。", D13FinanceIds.LoanRecoveryRoute);
            string eventId = EventId(request.OperationId, "open_account_" + request.AccountId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccount(request.AccountId));
            if (string.IsNullOrWhiteSpace(request.AccountId) || string.IsNullOrWhiteSpace(request.OwnerRef))
                return Failure(request, eventId, "account_fields_required", "账户和账户所有者不能为空。", D13FinanceIds.LoanRecoveryRoute);
            if (_accounts.ContainsKey(request.AccountId))
                return Failure(request, eventId, "account_exists", "金融账户已经存在。", D13FinanceIds.LoanRecoveryRoute);
            if (request.InitialCash < 0 || request.CreditLimit < 0)
                return Failure(request, eventId, "account_amount_invalid", "初始现金和授信额度不能为负数。", D13FinanceIds.LoanRecoveryRoute);
            if (!ValidExternalSource(request.SourceOwnerSystem, request.SourceEventId, request.SaveRevision, out string sourceError))
                return Failure(request, eventId, sourceError, "账户初始金额必须引用 EconomySystem 的已记录事实。", D13FinanceIds.LoanRecoveryRoute);

            var account = new D13FinanceAccountState
            {
                AccountId = request.AccountId,
                OwnerRef = request.OwnerRef,
                AccountType = request.AccountType,
                CashBalance = ClampCash(request.InitialCash),
                CreditLimit = ClampMoney(request.CreditLimit),
                CreditUsed = 0,
                CreditScore = ConfigInt(D13FinanceConfiguration.InitialCreditScore),
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            AddCreditReason(account, "按 D13 金融配置建立初始信用档案。");
            var fact = AppendEvent(eventId, request, "finance_account_opened", "金融账户已建立。",
                new Dictionary<string, string>
                {
                    ["account_id"] = account.AccountId,
                    ["owner_ref"] = account.OwnerRef,
                    ["cash_after"] = account.CashBalance.ToString(CultureInfo.InvariantCulture),
                    ["credit_limit"] = account.CreditLimit.ToString(CultureInfo.InvariantCulture),
                    ["source_owner_system"] = request.SourceOwnerSystem,
                    ["source_event_id"] = request.SourceEventId,
                });
            account.Revision = 1;
            account.LastEventId = eventId;
            _accounts[account.AccountId] = account;
            _revision = Math.Max(_revision + 1, account.Revision);
            return Success(fact, account, null, null, null, null, null, null, request.SourceEventId,
                0, 0, 0, account.CashBalance, null, null);
        }

        public D13FinanceResult Deposit(D13FinanceCashRequest request)
        {
            if (request != null) request.Operation = D13FinanceCashOperation.Deposit;
            return ApplyCash(request);
        }

        public D13FinanceResult Withdraw(D13FinanceCashRequest request)
        {
            if (request != null) request.Operation = D13FinanceCashOperation.Withdraw;
            return ApplyCash(request);
        }

        public D13FinanceResult Spend(D13FinanceCashRequest request)
        {
            if (request != null) request.Operation = D13FinanceCashOperation.Spend;
            return ApplyCash(request);
        }

        public D13FinanceResult Receive(D13FinanceCashRequest request)
        {
            if (request != null) request.Operation = D13FinanceCashOperation.Receive;
            return ApplyCash(request);
        }

        private D13FinanceResult ApplyCash(D13FinanceCashRequest request)
        {
            if (request == null) return Invalid("invalid_request", "现金请求不存在。", D13FinanceIds.LoanRecoveryRoute);
            string eventId = EventId(request.OperationId, "cash_" + request.Operation.ToString().ToLowerInvariant());
            var duplicate = Existing(eventId, request.SaveRevision);
            D13FinanceAccountState account;
            if (duplicate != null)
            {
                _accounts.TryGetValue(request.AccountId ?? string.Empty, out account);
                return Duplicate(duplicate, account);
            }
            if (!TryAccount(request.AccountId, out account)) return Failure(request, eventId, "account_not_found", "现金所属账户不存在。", D13FinanceIds.LoanRecoveryRoute);
            if (request.Amount <= 0) return Failure(request, eventId, "cash_amount_invalid", "现金变动金额必须大于零。", D13FinanceIds.LoanRecoveryRoute, account);
            if (!ValidExternalSource(request.SourceOwnerSystem, request.SourceEventId, request.SaveRevision, out string sourceError))
                return Failure(request, eventId, sourceError, "现金变动必须引用 EconomySystem 的已记录事实。", D13FinanceIds.LoanRecoveryRoute, account);
            long cashAfter = account.CashBalance;
            if (request.Operation == D13FinanceCashOperation.Deposit || request.Operation == D13FinanceCashOperation.Receive)
            {
                cashAfter = SafeAdd(cashAfter, request.Amount);
                if (cashAfter > CashMaximum()) return Failure(request, eventId, "cash_limit_exceeded", "现金超过统一配置上限。", D13FinanceIds.LoanRecoveryRoute, account);
            }
            else
            {
                if (request.Amount > account.CashBalance) return Failure(request, eventId, "cash_insufficient", "现金不足，不能完成该支出。", D13FinanceIds.LoanRecoveryRoute, account);
                cashAfter -= request.Amount;
            }
            var fact = AppendEvent(eventId, request, "finance_cash_" + request.Operation.ToString().ToLowerInvariant(),
                request.Description ?? "金融账户现金已结算。", new Dictionary<string, string>
                {
                    ["account_id"] = account.AccountId,
                    ["cash_before"] = account.CashBalance.ToString(CultureInfo.InvariantCulture),
                    ["amount"] = request.Amount.ToString(CultureInfo.InvariantCulture),
                    ["cash_after"] = cashAfter.ToString(CultureInfo.InvariantCulture),
                    ["source_owner_system"] = request.SourceOwnerSystem,
                    ["source_event_id"] = request.SourceEventId,
                });
            account.CashBalance = ClampCash(cashAfter);
            Touch(account, eventId, request.WorldDay);
            return Success(fact, account, null, null, null, null, null, null, request.SourceEventId,
                0, 0, 0, account.CashBalance, null, null);
        }

        public D13FinanceResult RequestLoan(D13FinanceLoanRequest request)
        {
            if (request == null) return Invalid("invalid_request", "贷款请求不存在。", D13FinanceIds.LoanRecoveryRoute);
            if (!Enum.IsDefined(typeof(D13FinanceDebtKind), request.Kind)) request.Kind = D13FinanceDebtKind.Loan;
            return IssueDebt(request, true);
        }

        public D13FinanceResult RequestCredit(D13FinanceCreditRequest request)
        {
            if (request == null) return Invalid("invalid_request", "赊账请求不存在。", D13FinanceIds.LoanRecoveryRoute);
            request.Kind = D13FinanceDebtKind.MerchantCredit;
            return IssueDebt(request, false);
        }

        private D13FinanceResult IssueDebt(D13FinanceLoanRequest request, bool creditCash)
        {
            string eventId = EventId(request.OperationId, "issue_debt_" + request.AccountId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null)
            {
                D13FinanceDebtState duplicateDebt = _debts.Values.FirstOrDefault(item => item.LastEventId == eventId);
                return Duplicate(duplicate, duplicateDebt == null ? null : FindAccount(duplicateDebt.AccountId), duplicateDebt);
            }
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "贷款或赊账所属账户不存在。", D13FinanceIds.LoanRecoveryRoute);
            if (string.IsNullOrWhiteSpace(request.CreditorRef) || string.IsNullOrWhiteSpace(request.DebtorRef))
                return Failure(request, eventId, "debt_parties_required", "债务人和债权人不能为空。", D13FinanceIds.LoanRecoveryRoute, account);
            if (!string.Equals(account.OwnerRef, request.DebtorRef, StringComparison.Ordinal))
                return Failure(request, eventId, "debt_owner_mismatch", "债务人必须是金融账户所有者。", D13FinanceIds.LoanRecoveryRoute, account);
            if (request.Principal <= 0 || request.Principal > NumericConfiguration.Default.MoneyAmount.Maximum)
                return Failure(request, eventId, "principal_invalid", "本金必须在统一金额边界内且大于零。", D13FinanceIds.LoanRecoveryRoute, account);
            int termDays = request.TermDays <= 0 ? ConfigInt(D13FinanceConfiguration.DefaultLoanTermDays) : request.TermDays;
            if (termDays < 1 || termDays > ConfigInt(D13FinanceConfiguration.MaxLoanTermDays))
                return Failure(request, eventId, "term_invalid", "贷款期限超出统一配置边界。", D13FinanceIds.LoanRecoveryRoute, account);
            if (!ValidExternalSource(request.SourceOwnerSystem, request.SourceEventId, request.SaveRevision, out string sourceError))
                return Failure(request, eventId, sourceError, "贷款或赊账必须引用 EconomySystem 的批准事实。", D13FinanceIds.LoanRecoveryRoute, account);
            if (request.Kind == D13FinanceDebtKind.MerchantCredit && creditCash)
                return Failure(request, eventId, "credit_kind_invalid", "赊账不应重复发放现金。", D13FinanceIds.LoanRecoveryRoute, account);

            var collateral = ResolveCollateral(request.Collateral, request.DebtorRef, request.WorldDay,
                request.SaveRevision, out string collateralCode, out string collateralMessage);
            if (collateral == null && !string.IsNullOrEmpty(collateralCode))
                return Failure(request, eventId, collateralCode, collateralMessage, D13FinanceIds.LoanRecoveryRoute, account);
            long collateralCapacity = collateral == null ? 0 : collateral.Sum(item => PercentOf(item.ClaimValue, ConfigInt(D13FinanceConfiguration.CollateralAdvancePercent)));
            long guaranteeCap = request.GuarantorRef == null ? 0 :
                (request.GuaranteeCap > 0 ? request.GuaranteeCap : request.Principal);
            long guaranteeCapacity = PercentOf(guaranteeCap, ConfigInt(D13FinanceConfiguration.GuaranteeAdvancePercent));
            long availableCapacity = Math.Max(0, account.CreditLimit - account.CreditUsed) + collateralCapacity + guaranteeCapacity;
            if (request.Principal > availableCapacity)
                return Failure(request, eventId, "credit_limit_exceeded",
                    "授信不足：可用授信 " + availableCapacity + "，申请本金 " + request.Principal + "。", D13FinanceIds.LoanRecoveryRoute, account);

            string debtId = "debt_" + (string.IsNullOrWhiteSpace(request.OperationId) ? request.AccountId : request.OperationId);
            if (_debts.ContainsKey(debtId)) return Failure(request, eventId, "debt_exists", "同一债务编号已经存在。", D13FinanceIds.LoanRecoveryRoute, account);
            int spread = (int)Math.Ceiling((100m - account.CreditScore) * ConfigInt(D13FinanceConfiguration.CreditRiskSpreadPercent) / 100m);
            int interestRate = ConfigInt(D13FinanceConfiguration.BaseLoanInterestPercent) + spread;
            int dueDay = Math.Max(0, request.WorldDay) + termDays;
            var debt = new D13FinanceDebtState
            {
                DebtId = debtId,
                AccountId = account.AccountId,
                DebtorRef = request.DebtorRef,
                CreditorRef = request.CreditorRef,
                Kind = request.Kind,
                Principal = request.Principal,
                InterestRatePercent = interestRate,
                LatePenaltyPercent = ConfigInt(D13FinanceConfiguration.LatePenaltyPercent),
                DueDay = dueDay,
                GraceUntilDay = dueDay + ConfigInt(D13FinanceConfiguration.GraceDays),
                LastAccrualDay = Math.Max(0, request.WorldDay),
                LedgerRef = string.IsNullOrWhiteSpace(request.LedgerRef) ? "ledger_" + debtId : request.LedgerRef,
                SourceEventId = request.SourceEventId,
                Collateral = collateral ?? new List<D13FinanceCollateralState>(),
                RiskExplanation = "本金 " + request.Principal + "；年化登记利率 " + interestRate + "%；到期日 " + dueDay +
                    "；宽限期至 " + (dueDay + ConfigInt(D13FinanceConfiguration.GraceDays)) + "。",
                WorstCaseOutcome = WorstCaseFor(request.Principal, interestRate, collateral, guaranteeCap),
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            D13FinanceGuaranteeState guarantee = null;
            if (!string.IsNullOrWhiteSpace(request.GuarantorRef))
            {
                if (string.Equals(request.GuarantorRef, request.DebtorRef, StringComparison.Ordinal))
                    return Failure(request, eventId, "guarantor_same_as_debtor", "担保人不能与债务人相同。", D13FinanceIds.LoanRecoveryRoute, account);
                guarantee = new D13FinanceGuaranteeState
                {
                    GuaranteeId = debtId + "_guarantee",
                    DebtId = debtId,
                    GuarantorRef = request.GuarantorRef,
                    PrincipalCap = guaranteeCap,
                    SourceEventId = request.SourceEventId,
                    LastUpdatedDay = Math.Max(0, request.WorldDay),
                };
                debt.GuaranteeIds.Add(guarantee.GuaranteeId);
            }
            var values = new Dictionary<string, string>
            {
                ["account_id"] = account.AccountId,
                ["debt_id"] = debt.DebtId,
                ["debt_kind"] = debt.Kind.ToString(),
                ["principal"] = debt.Principal.ToString(CultureInfo.InvariantCulture),
                ["interest_rate_percent"] = debt.InterestRatePercent.ToString(CultureInfo.InvariantCulture),
                ["due_day"] = debt.DueDay.ToString(CultureInfo.InvariantCulture),
                ["grace_until_day"] = debt.GraceUntilDay.ToString(CultureInfo.InvariantCulture),
                ["cash_credited"] = creditCash ? request.Principal.ToString(CultureInfo.InvariantCulture) : "0",
                ["source_owner_system"] = request.SourceOwnerSystem,
                ["source_event_id"] = request.SourceEventId,
                ["worst_case"] = debt.WorstCaseOutcome,
            };
            if (collateral != null) values["collateral_refs"] = string.Join(",", collateral.Select(item => item.AssetRef).ToArray());
            if (guarantee != null) values["guarantee_id"] = guarantee.GuaranteeId;
            var fact = AppendEvent(eventId, request, request.Kind == D13FinanceDebtKind.MerchantCredit
                ? "finance_credit_issued" : "finance_loan_issued", request.Kind == D13FinanceDebtKind.MerchantCredit
                ? "赊账已建立，未直接扣除现金。" : "贷款已建立并记入金融账户。", values);
            if (creditCash) account.CashBalance = ClampCash(SafeAdd(account.CashBalance, request.Principal));
            account.CreditUsed = SafeAdd(account.CreditUsed, request.Principal);
            account.DebtIds.Add(debt.DebtId);
            Touch(account, eventId, request.WorldDay);
            debt.Revision = 1;
            debt.LastEventId = eventId;
            _debts[debt.DebtId] = debt;
            if (guarantee != null)
            {
                guarantee.Revision = 1;
                _guarantees[guarantee.GuaranteeId] = guarantee;
            }
            _revision = Math.Max(_revision + 1, debt.Revision);
            return Success(fact, account, debt, guarantee, null, null, null, null, request.SourceEventId,
                0, 0, 0, account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
        }

        public D13FinanceResult AddGuarantee(D13FinanceGuaranteeRequest request)
        {
            if (request == null) return Invalid("invalid_request", "担保请求不存在。", D13FinanceIds.LoanRecoveryRoute);
            string eventId = EventId(request.OperationId, "guarantee_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "担保对应债务不存在。", D13FinanceIds.LoanRecoveryRoute);
            if (string.IsNullOrWhiteSpace(request.GuarantorRef) || string.Equals(request.GuarantorRef, debt.DebtorRef, StringComparison.Ordinal))
                return Failure(request, eventId, "guarantor_invalid", "担保人不能为空且不能与债务人相同。", D13FinanceIds.LoanRecoveryRoute);
            if (request.PrincipalCap <= 0 || request.PrincipalCap > debt.TotalDue)
                return Failure(request, eventId, "guarantee_cap_invalid", "担保责任上限必须覆盖正数债务且不能超过当前应付。", D13FinanceIds.LoanRecoveryRoute);
            if (debt.GuaranteeIds.Any(id => _guarantees.ContainsKey(id) && _guarantees[id].GuarantorRef == request.GuarantorRef))
                return Failure(request, eventId, "guarantee_exists", "该担保人已经登记在该债务上。", D13FinanceIds.LoanRecoveryRoute);
            string guaranteeId = "guarantee_" + (string.IsNullOrWhiteSpace(request.OperationId) ? request.DebtId : request.OperationId);
            var guarantee = new D13FinanceGuaranteeState
            {
                GuaranteeId = guaranteeId,
                DebtId = debt.DebtId,
                GuarantorRef = request.GuarantorRef,
                PrincipalCap = request.PrincipalCap,
                SourceEventId = eventId,
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            var fact = AppendEvent(eventId, request, "finance_guarantee_registered", "担保责任已登记，责任上限已公开。",
                new Dictionary<string, string>
                {
                    ["debt_id"] = debt.DebtId,
                    ["guarantee_id"] = guaranteeId,
                    ["guarantor_ref"] = request.GuarantorRef,
                    ["principal_cap"] = request.PrincipalCap.ToString(CultureInfo.InvariantCulture),
                    ["worst_case"] = "债务违约且抵押不足时，担保人承担不超过该责任上限的追偿责任。",
                });
            debt.GuaranteeIds.Add(guaranteeId);
            Touch(debt, eventId, request.WorldDay);
            guarantee.Revision = 1;
            _guarantees[guaranteeId] = guarantee;
            _revision = Math.Max(_revision + 1, debt.Revision);
            return Success(fact, null, debt, guarantee, null, null, null, null, eventId,
                0, 0, 0, 0, "担保责任上限 " + request.PrincipalCap + "。", "担保人承担不超过登记责任上限的追偿责任。");
        }

        public D13FinanceResult AccrueInterest(D13FinanceAccrueRequest request)
        {
            if (request == null) return Invalid("invalid_request", "利息请求不存在。", D13FinanceIds.DebtRecoveryRoute);
            string eventId = EventId(request.OperationId, "interest_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "利息对应债务不存在。", D13FinanceIds.DebtRecoveryRoute);
            if (IsClosed(debt.State)) return Failure(request, eventId, "debt_closed", "已结清或已查封债务不能继续计息。", D13FinanceIds.DebtRecoveryRoute, null, debt);
            if (IsProtected(debt))
            {
                var frozenFact = AppendEvent(eventId, request, "finance_interest_frozen", "破产保护期间利息冻结，债务本金未被免除。",
                    DebtValues(debt, 0, 0));
                return Success(frozenFact, FindAccount(debt.AccountId), debt, null, null, null, null, null, debt.SourceEventId,
                    0, 0, 0, FindCash(debt.AccountId), debt.RiskExplanation, "破产保护结束后仍需按裁决恢复还款或重组。");
            }
            int elapsed = Math.Max(0, request.WorldDay - debt.LastAccrualDay);
            long added = elapsed == 0 ? 0 : PercentCeiling(debt.Principal * (long)debt.InterestRatePercent * elapsed, 100L * 30L);
            var values = DebtValues(debt, added, 0);
            values["elapsed_days"] = elapsed.ToString(CultureInfo.InvariantCulture);
            values["interest_rate_percent"] = debt.InterestRatePercent.ToString(CultureInfo.InvariantCulture);
            var fact = AppendEvent(eventId, request, "finance_interest_accrued", "利息已按统一配置和经过的世界日计入债务。", values);
            debt.InterestAccrued = SafeAdd(debt.InterestAccrued, added);
            debt.LastAccrualDay = Math.Max(debt.LastAccrualDay, request.WorldDay);
            Touch(debt, eventId, request.WorldDay);
            _revision = Math.Max(_revision + 1, debt.Revision);
            return Success(fact, FindAccount(debt.AccountId), debt, null, null, null, null, null, debt.SourceEventId,
                0, 0, added, FindCash(debt.AccountId), debt.RiskExplanation, debt.WorstCaseOutcome);
        }

        public D13FinanceResult EvaluateDue(D13FinanceDueRequest request)
        {
            if (request == null) return Invalid("invalid_request", "到期检查请求不存在。", D13FinanceIds.DebtRecoveryRoute);
            string eventId = EventId(request.OperationId, "due_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, null, FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "到期检查对应债务不存在。", D13FinanceIds.DebtRecoveryRoute);
            if (IsClosed(debt.State)) return Failure(request, eventId, "debt_closed", "已结清或已查封债务不再进行到期检查。", D13FinanceIds.DebtRecoveryRoute, null, debt);
            D13FinanceAccountState account;
            _accounts.TryGetValue(debt.AccountId, out account);
            if (account != null && account.BankruptcyProtectionUntilDay >= request.WorldDay && account.BankruptcyProtectionUntilDay > 0)
            {
                debt.StateBeforeProtection = debt.State;
                debt.State = D13FinanceDebtLifecycleState.BankruptcyProtected;
                debt.CollectionState = D13FinanceCollectionState.BankruptcyProtected;
                var protectedFact = AppendEvent(eventId, request, "finance_due_deferred_by_protection", "破产保护暂缓到期催收；债务仍被保留。", DebtValues(debt, 0, 0));
                Touch(debt, eventId, request.WorldDay);
                _debts[debt.DebtId] = debt;
                return Success(protectedFact, account, debt, null, null, null, null, null, debt.SourceEventId,
                    0, 0, 0, account.CashBalance, debt.RiskExplanation, "保护期结束后若仍未还款，债务恢复为逾期或违约状态。");
            }
            if (request.WorldDay <= debt.DueDay)
            {
                var beforeFact = AppendEvent(eventId, request, "finance_due_not_reached", "债务尚未到期，当前不触发逾期后果。", DebtValues(debt, 0, 0));
                return Success(beforeFact, account, debt, null, null, null, null, null, debt.SourceEventId,
                    0, 0, 0, account == null ? 0 : account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
            }
            bool becameDefault = request.WorldDay > debt.GraceUntilDay && debt.State != D13FinanceDebtLifecycleState.Defaulted;
            if (request.WorldDay <= debt.GraceUntilDay)
            {
                debt.State = D13FinanceDebtLifecycleState.PastDue;
                debt.CollectionState = D13FinanceCollectionState.Negotiation;
            }
            else
            {
                debt.State = D13FinanceDebtLifecycleState.Defaulted;
                debt.CollectionState = D13FinanceCollectionState.Negotiation;
                if (debt.LatePenalty == 0) debt.LatePenalty = PercentCeiling(debt.Principal, debt.LatePenaltyPercent);
                if (account != null && becameDefault)
                {
                    AdjustCredit(account, -ConfigInt(D13FinanceConfiguration.DefaultCreditDelta), "逾期超过宽限期，信用下降并进入违约。", request.WorldDay, eventId);
                }
            }
            string code = debt.State == D13FinanceDebtLifecycleState.Defaulted ? "finance_debt_defaulted" : "finance_debt_past_due";
            var fact = AppendEvent(eventId, request, code,
                debt.State == D13FinanceDebtLifecycleState.Defaulted
                    ? "债务已超过宽限期，进入违约与催收路径。"
                    : "债务已到期但仍在宽限期内，已记录逾期风险。", DebtValues(debt, 0, debt.LatePenalty));
            Touch(debt, eventId, request.WorldDay);
            if (account != null) Touch(account, eventId, request.WorldDay);
            return Success(fact, account, debt, null, null, null, null, null, debt.SourceEventId,
                0, 0, 0, account == null ? 0 : account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
        }

        public D13FinanceResult Repay(D13FinanceRepayRequest request)
        {
            if (request == null) return Invalid("invalid_request", "还款请求不存在。", D13FinanceIds.DebtRecoveryRoute);
            string eventId = EventId(request.OperationId, "repay_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccount(request.AccountId), FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "还款对应债务不存在。", D13FinanceIds.DebtRecoveryRoute);
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account) || account.AccountId != debt.AccountId)
                return Failure(request, eventId, "account_debt_mismatch", "还款账户与债务不匹配。", D13FinanceIds.DebtRecoveryRoute, account, debt);
            if (request.Amount <= 0 || request.Amount > account.CashBalance)
                return Failure(request, eventId, "repayment_cash_invalid", "还款金额必须为正且不超过现金余额。", D13FinanceIds.DebtRecoveryRoute, account, debt);
            if (IsClosed(debt.State)) return Failure(request, eventId, "debt_closed", "该债务已经结清或查封。", D13FinanceIds.DebtRecoveryRoute, account, debt);
            long payment = Math.Min(request.Amount, debt.TotalDue);
            long penaltyPaid = Math.Min(debt.LatePenalty, payment);
            long afterPenalty = payment - penaltyPaid;
            long interestPaid = Math.Min(debt.InterestAccrued, afterPenalty);
            long principalPaid = Math.Min(debt.Principal, afterPenalty - interestPaid);
            var values = DebtValues(debt, 0, 0);
            values["payment"] = payment.ToString(CultureInfo.InvariantCulture);
            values["principal_paid"] = principalPaid.ToString(CultureInfo.InvariantCulture);
            values["interest_paid"] = interestPaid.ToString(CultureInfo.InvariantCulture);
            values["late_penalty_paid"] = penaltyPaid.ToString(CultureInfo.InvariantCulture);
            var fact = AppendEvent(eventId, request, "finance_debt_repaid", "还款已按滞纳金、利息、本金顺序结算。", values);
            account.CashBalance -= payment;
            account.CreditUsed = Math.Max(0, account.CreditUsed - principalPaid);
            debt.LatePenalty -= penaltyPaid;
            debt.InterestAccrued -= interestPaid;
            debt.Principal -= principalPaid;
            debt.LastPaymentDay = Math.Max(0, request.WorldDay);
            if (debt.TotalDue == 0)
            {
                debt.State = D13FinanceDebtLifecycleState.Paid;
                debt.CollectionState = D13FinanceCollectionState.Resolved;
                AdjustCredit(account, ConfigInt(D13FinanceConfiguration.TimelyPaymentCreditDelta), "债务已结清，信用改善。", request.WorldDay, eventId);
            }
            Touch(debt, eventId, request.WorldDay);
            Touch(account, eventId, request.WorldDay);
            return Success(fact, account, debt, null, null, null, null, null, debt.SourceEventId,
                principalPaid, interestPaid, 0, account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
        }

        public D13FinanceResult Renew(D13FinanceRenewRequest request)
        {
            if (request == null) return Invalid("invalid_request", "展期请求不存在。", D13FinanceIds.DebtRecoveryRoute);
            string eventId = EventId(request.OperationId, "renew_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccountByDebt(request.DebtId), FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "展期对应债务不存在。", D13FinanceIds.DebtRecoveryRoute);
            if (debt.State == D13FinanceDebtLifecycleState.Paid || debt.State == D13FinanceDebtLifecycleState.Seized ||
                debt.State == D13FinanceDebtLifecycleState.WrittenOff || debt.State == D13FinanceDebtLifecycleState.BankruptcyProtected)
                return Failure(request, eventId, "renewal_not_allowed", "当前债务状态不能展期。", D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            if (debt.RenegotiationCount >= ConfigInt(D13FinanceConfiguration.MaxRenewals))
                return Failure(request, eventId, "renewal_limit_reached", "展期次数已经达到统一配置上限。", D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            int term = request.TermDays <= 0 ? ConfigInt(D13FinanceConfiguration.RenewalTermDays) : request.TermDays;
            if (term < 1 || term > ConfigInt(D13FinanceConfiguration.MaxLoanTermDays))
                return Failure(request, eventId, "renewal_term_invalid", "展期期限超出统一配置边界。", D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            var addedCollateral = ResolveCollateral(request.NewCollateral, debt.DebtorRef, request.WorldDay,
                request.SaveRevision, out string collateralCode, out string collateralMessage);
            if (addedCollateral == null && !string.IsNullOrEmpty(collateralCode))
                return Failure(request, eventId, collateralCode, collateralMessage, D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            long fee = PercentCeiling(debt.TotalDue, ConfigInt(D13FinanceConfiguration.RenewalFeePercent));
            var values = DebtValues(debt, fee, 0);
            values["renewal_fee_capitalized"] = fee.ToString(CultureInfo.InvariantCulture);
            values["new_due_day"] = (Math.Max(request.WorldDay, debt.DueDay) + term).ToString(CultureInfo.InvariantCulture);
            var fact = AppendEvent(eventId, request, "finance_debt_renewed", "债务已展期；展期费用资本化并公开计入应付金额。", values);
            debt.InterestAccrued = SafeAdd(debt.InterestAccrued, fee);
            debt.DueDay = Math.Max(request.WorldDay, debt.DueDay) + term;
            debt.GraceUntilDay = debt.DueDay + ConfigInt(D13FinanceConfiguration.GraceDays);
            debt.State = D13FinanceDebtLifecycleState.Extended;
            debt.CollectionState = D13FinanceCollectionState.Negotiation;
            debt.RenegotiationCount++;
            debt.LastAccrualDay = Math.Max(debt.LastAccrualDay, request.WorldDay);
            if (addedCollateral != null) debt.Collateral.AddRange(addedCollateral);
            debt.RiskExplanation = "展期费用 " + fee + " 已加入利息；新到期日 " + debt.DueDay + "；剩余展期次数 " +
                Math.Max(0, ConfigInt(D13FinanceConfiguration.MaxRenewals) - debt.RenegotiationCount) + "。";
            D13FinanceAccountState account;
            _accounts.TryGetValue(debt.AccountId, out account);
            if (account != null) AdjustCredit(account, -ConfigInt(D13FinanceConfiguration.RenewalCreditDelta), "展期增加金融风险，信用轻微下降。", request.WorldDay, eventId);
            Touch(debt, eventId, request.WorldDay);
            return Success(fact, account, debt, null, null, null, null, null, debt.SourceEventId,
                0, 0, fee, account == null ? 0 : account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
        }

        public D13FinanceResult QuoteInsurance(D13FinanceInsuranceQuoteRequest request)
        {
            if (request == null) return Invalid("invalid_request", "保险报价请求不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            string eventId = EventId(request.OperationId, "insurance_quote_" + request.AccountId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccount(request.AccountId));
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "保险报价所属账户不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (!ValidInsuranceShape(request, eventId, account, out string code, out string message))
                return Failure(request, eventId, code, message, D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            var quote = BuildInsuranceQuote(request, eventId);
            var fact = AppendEvent(eventId, request, "finance_insurance_quoted", "保险报价已生成；保费、免赔额、排除项和最坏结果已公开。",
                new Dictionary<string, string>
                {
                    ["account_id"] = request.AccountId,
                    ["kind"] = request.Kind.ToString(),
                    ["coverage_amount"] = quote.CoverageAmount.ToString(CultureInfo.InvariantCulture),
                    ["premium"] = quote.Premium.ToString(CultureInfo.InvariantCulture),
                    ["deductible"] = quote.DeductibleAmount.ToString(CultureInfo.InvariantCulture),
                    ["max_payout"] = quote.MaxPayout.ToString(CultureInfo.InvariantCulture),
                    ["worst_case"] = quote.WorstCaseOutcome,
                });
            quote.SourceEventId = fact.EventId;
            var result = Success(fact, account, null, null, null, null, null, quote, fact.EventId,
                0, 0, quote.Premium, account.CashBalance, quote.RiskExplanation, quote.WorstCaseOutcome);
            result.Quote = quote;
            return result;
        }

        public D13FinanceResult BuyInsurance(D13FinanceInsuranceRequest request)
        {
            if (request == null) return Invalid("invalid_request", "保险购买请求不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            string eventId = EventId(request.OperationId, "insurance_buy_" + request.AccountId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null)
            {
                var existingPolicy = _policies.Values.FirstOrDefault(item => item.LastEventId == eventId);
                return Duplicate(duplicate, existingPolicy == null ? null : FindAccount(existingPolicy.AccountId), null, existingPolicy);
            }
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "保险所属账户不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (!ValidInsuranceShape(request, eventId, account, out string code, out string message))
                return Failure(request, eventId, code, message, D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            if (!ValidExternalSource(request.SourceOwnerSystem, request.SourceEventId, request.SaveRevision, out string sourceError))
                return Failure(request, eventId, sourceError, "保险保费必须引用 EconomySystem 的已记录事实。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            var quote = BuildInsuranceQuote(request, eventId);
            if (quote.Premium > account.CashBalance)
                return Failure(request, eventId, "premium_cash_insufficient", "现金不足以支付保费；保险尚未生效。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            string policyId = "policy_" + (string.IsNullOrWhiteSpace(request.OperationId) ? request.AccountId : request.OperationId);
            if (_policies.ContainsKey(policyId)) return Failure(request, eventId, "policy_exists", "保险单已经存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            var policy = new D13FinanceInsurancePolicyState
            {
                PolicyId = policyId,
                AccountId = account.AccountId,
                InsuredRef = request.InsuredRef,
                Kind = request.Kind,
                AssetRef = request.AssetRef,
                CoverageAmount = quote.CoverageAmount,
                PremiumPaid = quote.Premium,
                DeductibleAmount = quote.DeductibleAmount,
                MaxPayout = quote.MaxPayout,
                PaidOutAmount = 0,
                RemainingCoverage = quote.MaxPayout,
                StartDay = quote.StartDay,
                EndDay = quote.EndDay,
                Exclusions = new List<string>(quote.Exclusions),
                RequiredEvidenceKinds = new List<D13FinanceEvidenceKind>(quote.RequiredEvidenceKinds),
                SourceEventId = request.SourceEventId,
                LastUpdatedDay = Math.Max(0, request.WorldDay),
            };
            var fact = AppendEvent(eventId, request, "finance_insurance_purchased", "保险已生效，保费与免赔额已经写入金融状态。",
                new Dictionary<string, string>
                {
                    ["policy_id"] = policyId,
                    ["kind"] = policy.Kind.ToString(),
                    ["premium"] = policy.PremiumPaid.ToString(CultureInfo.InvariantCulture),
                    ["deductible"] = policy.DeductibleAmount.ToString(CultureInfo.InvariantCulture),
                    ["max_payout"] = policy.MaxPayout.ToString(CultureInfo.InvariantCulture),
                    ["source_owner_system"] = request.SourceOwnerSystem,
                    ["source_event_id"] = request.SourceEventId,
                    ["worst_case"] = quote.WorstCaseOutcome,
                });
            account.CashBalance -= policy.PremiumPaid;
            account.PolicyIds.Add(policyId);
            Touch(account, eventId, request.WorldDay);
            policy.Revision = 1;
            policy.LastEventId = eventId;
            _policies[policyId] = policy;
            _revision = Math.Max(_revision + 1, policy.Revision);
            var result = Success(fact, account, null, null, null, null, policy, quote, request.SourceEventId,
                0, 0, policy.PremiumPaid, account.CashBalance, quote.RiskExplanation, quote.WorstCaseOutcome);
            result.Quote = quote;
            return result;
        }

        public D13FinanceResult RegisterEvidence(D13FinanceEvidenceRequest request)
        {
            if (request == null) return Invalid("invalid_request", "保险证据请求不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            string eventId = EventId(request.OperationId, "evidence_" + request.EvidenceId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccount(request.AccountId));
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "证据所属账户不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (string.IsNullOrWhiteSpace(request.EvidenceId) || string.IsNullOrWhiteSpace(request.FactEventId))
                return Failure(request, eventId, "evidence_fields_required", "证据编号和事实事件不能为空。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            if (_evidence.ContainsKey(request.EvidenceId)) return Failure(request, eventId, "evidence_exists", "证据已经登记。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            var sourceFact = FindAnyFact(request.FactEventId, request.FactSaveRevision);
            if (sourceFact == null) return Failure(request, eventId, "evidence_fact_missing", "证据必须引用已经进入 EventLog 的事实。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account);
            var evidence = new D13FinanceEvidenceRecord
            {
                EvidenceId = request.EvidenceId,
                OwnerRef = account.AccountId,
                Kind = request.Kind,
                FactEventId = sourceFact.EventId,
                FactSaveRevision = sourceFact.SaveRevision,
                Description = request.Description ?? string.Empty,
                SourceEventId = sourceFact.EventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0, request.SaveRevision),
            };
            var fact = AppendEvent(eventId, request, "finance_evidence_registered", "保险证据已登记；理赔仍需满足承保范围和排除条款。",
                new Dictionary<string, string>
                {
                    ["account_id"] = account.AccountId,
                    ["evidence_id"] = evidence.EvidenceId,
                    ["evidence_kind"] = evidence.Kind.ToString(),
                    ["fact_event_id"] = evidence.FactEventId,
                    ["fact_save_revision"] = evidence.FactSaveRevision.ToString(CultureInfo.InvariantCulture),
                });
            evidence.SourceEventId = fact.EventId;
            evidence.SaveRevision = Math.Max(0, request.SaveRevision);
            _evidence[evidence.EvidenceId] = evidence;
            _revision++;
            var result = Success(fact, account, null, null, evidence, null, null, null, sourceFact.EventId,
                0, 0, 0, account.CashBalance, "证据已记录，但不保证理赔通过。", "若证据不足或事件属于排除项，理赔可能为 0。");
            result.Evidence = evidence;
            return result;
        }

        public D13FinanceResult SubmitClaim(D13FinanceClaimRequest request)
        {
            if (request == null) return Invalid("invalid_request", "保险理赔请求不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            string eventId = EventId(request.OperationId, "claim_" + request.PolicyId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null)
            {
                var existingClaim = _claims.Values.FirstOrDefault(item => item.SourceEventId == eventId || item.ClaimId == ClaimId(eventId));
                return Duplicate(duplicate, existingClaim == null ? null : FindAccountByPolicy(existingClaim.PolicyId), null,
                    existingClaim == null ? null : FindPolicy(existingClaim.PolicyId), existingClaim);
            }
            if (!_policies.TryGetValue(request.PolicyId ?? string.Empty, out D13FinanceInsurancePolicyState policy))
                return Failure(request, eventId, "policy_not_found", "理赔对应保险单不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (!TryAccount(policy.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "理赔账户不存在。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (policy.State != D13FinancePolicyState.Active || request.WorldDay < policy.StartDay || request.WorldDay > policy.EndDay)
                return Failure(request, eventId, "policy_inactive", "保险不在有效期或剩余赔付已耗尽。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account, null, null, policy);
            if (request.LossAmount <= 0 || string.IsNullOrWhiteSpace(request.IncidentTag))
                return Failure(request, eventId, "claim_shape_invalid", "损失金额和事故标签必须有效。", D13FinanceIds.InsuranceEvidenceRecoveryRoute, account, null, null, policy);
            var incident = FindAnyFact(request.IncidentEventId, request.SaveRevision);
            var claimId = ClaimId(eventId);
            if (incident == null)
                return DeniedClaim(request, eventId, policy, account, claimId, "incident_fact_missing", "理赔事故必须引用已记录事实。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            var evidence = ResolveEvidence(request.EvidenceIds, account.AccountId, out string evidenceCode, out string evidenceMessage);
            bool exclusion = IsExcluded(policy, request.IncidentTag);
            bool covered = CoverageMatches(policy.Kind, request.IncidentTag);
            if (evidence == null || evidence.Count < ConfigInt(D13FinanceConfiguration.MinimumInsuranceEvidence))
                return DeniedClaim(request, eventId, policy, account, claimId,
                    evidenceCode ?? "evidence_insufficient", evidenceMessage ?? "登记证据数量不足，理赔被拒。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (policy.RequiredEvidenceKinds.Any(required => !evidence.Any(item => item.Kind == required)))
                return DeniedClaim(request, eventId, policy, account, claimId, "evidence_kind_missing",
                    "理赔缺少保险单要求的证据类型，赔付为零。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (exclusion)
                return DeniedClaim(request, eventId, policy, account, claimId, "excluded_incident", "事故属于保险排除项，赔付为零。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            if (!covered)
                return DeniedClaim(request, eventId, policy, account, claimId, "incident_not_covered", "事故标签不在本保险承保范围内。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            long afterDeductible = Math.Max(0, request.LossAmount - policy.DeductibleAmount);
            long payout = Math.Min(afterDeductible, policy.RemainingCoverage);
            if (payout <= 0)
                return DeniedClaim(request, eventId, policy, account, claimId, "deductible_exceeds_loss", "损失不超过免赔额或剩余赔付额度为零。", D13FinanceIds.InsuranceEvidenceRecoveryRoute);
            long deniedAmount = request.LossAmount - payout;
            var claim = new D13FinanceInsuranceClaimState
            {
                ClaimId = claimId,
                PolicyId = policy.PolicyId,
                IncidentEventId = incident.EventId,
                IncidentTag = request.IncidentTag,
                LossAmount = request.LossAmount,
                ApprovedPayout = payout,
                DeniedAmount = deniedAmount,
                State = deniedAmount == 0 ? D13FinanceClaimState.Approved : D13FinanceClaimState.PartiallyPaid,
                DecisionCode = deniedAmount == 0 ? "claim_approved" : "claim_partially_paid",
                DecisionExplanation = deniedAmount == 0
                    ? "证据、承保范围和免赔额检查通过。"
                    : "证据和承保范围通过，但赔付受免赔额或保额上限限制。",
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0, request.SaveRevision),
            };
            if (evidence != null) claim.EvidenceIds.AddRange(evidence.Select(item => item.EvidenceId));
            var fact = AppendEvent(eventId, request, "finance_insurance_claim_settled", "保险理赔已结算；赔付与未赔部分均已公开。",
                new Dictionary<string, string>
                {
                    ["policy_id"] = policy.PolicyId,
                    ["claim_id"] = claimId,
                    ["incident_event_id"] = incident.EventId,
                    ["loss_amount"] = request.LossAmount.ToString(CultureInfo.InvariantCulture),
                    ["approved_payout"] = payout.ToString(CultureInfo.InvariantCulture),
                    ["denied_amount"] = deniedAmount.ToString(CultureInfo.InvariantCulture),
                    ["evidence_count"] = claim.EvidenceIds.Count.ToString(CultureInfo.InvariantCulture),
                });
            claim.SourceEventId = fact.EventId;
            account.CashBalance = ClampCash(SafeAdd(account.CashBalance, payout));
            policy.PaidOutAmount = SafeAdd(policy.PaidOutAmount, payout);
            policy.RemainingCoverage = Math.Max(0, policy.MaxPayout - policy.PaidOutAmount);
            if (policy.RemainingCoverage == 0) policy.State = D13FinancePolicyState.Paid;
            policy.ClaimIds.Add(claimId);
            Touch(account, eventId, request.WorldDay);
            Touch(policy, eventId, request.WorldDay);
            _claims[claimId] = claim;
            _revision++;
            var result = Success(fact, account, null, null, null, claim, policy, null, incident.EventId,
                0, 0, 0, account.CashBalance, "赔付 " + payout + "；未赔 " + deniedAmount + "。", "未赔部分不能在本次理赔中自动补足；后续损失仍受剩余保额限制。");
            result.Claim = claim;
            return result;
        }

        public D13FinanceResult Collect(D13FinanceCollectionRequest request)
        {
            if (request == null) return Invalid("invalid_request", "催收请求不存在。", D13FinanceIds.DebtRecoveryRoute);
            string eventId = EventId(request.OperationId, "collect_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccountByDebt(request.DebtId), FindDebt(request.DebtId));
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "催收对应债务不存在。", D13FinanceIds.DebtRecoveryRoute);
            if (!string.Equals(request.CreditorRef, debt.CreditorRef, StringComparison.Ordinal))
                return Failure(request, eventId, "creditor_mismatch", "催收方不是登记债权人。", D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            if (debt.State != D13FinanceDebtLifecycleState.Defaulted && debt.State != D13FinanceDebtLifecycleState.Collection &&
                debt.State != D13FinanceDebtLifecycleState.PastDue)
                return Failure(request, eventId, "collection_not_ready", "债务尚未进入可催收状态。", D13FinanceIds.DebtRecoveryRoute, FindAccount(debt.AccountId), debt);
            D13FinanceAccountState account;
            _accounts.TryGetValue(debt.AccountId, out account);
            if (request.Action == D13FinanceCollectionAction.IssueNotice)
            {
                long fee = debt.CollectionState == D13FinanceCollectionState.NoticeIssued ? 0 : ConfigInt(D13FinanceConfiguration.CollectionFee);
                var fact = AppendEvent(eventId, request, "finance_collection_notice", "催收通知已登记；催收费用会明确计入应付金额。",
                    DebtValues(debt, fee, 0));
                debt.LatePenalty = SafeAdd(debt.LatePenalty, fee);
                debt.State = D13FinanceDebtLifecycleState.Collection;
                debt.CollectionState = D13FinanceCollectionState.NoticeIssued;
                Touch(debt, eventId, request.WorldDay);
                return Success(fact, account, debt, null, null, null, null, null, debt.SourceEventId,
                    0, 0, fee, account == null ? 0 : account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
            }
            if (request.Action == D13FinanceCollectionAction.CollectAvailableCash)
            {
                if (account == null || account.CashBalance <= 0)
                    return Failure(request, eventId, "collection_cash_unavailable", "账户没有可供催收的现金，下一条路径是展期、任务抵债或法律查封。", D13FinanceIds.DebtRecoveryRoute, account, debt);
                long payment = Math.Min(account.CashBalance, debt.TotalDue);
                long penaltyPaid = Math.Min(debt.LatePenalty, payment);
                long interestPaid = Math.Min(debt.InterestAccrued, payment - penaltyPaid);
                long principalPaid = Math.Min(debt.Principal, payment - penaltyPaid - interestPaid);
                var fact = AppendEvent(eventId, request, "finance_collection_cash_settled", "催收已从可用现金中结算。", DebtValues(debt, 0, payment));
                account.CashBalance -= payment;
                account.CreditUsed = Math.Max(0, account.CreditUsed - principalPaid);
                debt.LatePenalty -= penaltyPaid;
                debt.InterestAccrued -= interestPaid;
                debt.Principal -= principalPaid;
                if (debt.TotalDue == 0)
                {
                    debt.State = D13FinanceDebtLifecycleState.Paid;
                    debt.CollectionState = D13FinanceCollectionState.Resolved;
                }
                Touch(account, eventId, request.WorldDay);
                Touch(debt, eventId, request.WorldDay);
                return Success(fact, account, debt, null, null, null, null, null, debt.SourceEventId,
                    principalPaid, interestPaid, penaltyPaid, account.CashBalance, debt.RiskExplanation, debt.WorstCaseOutcome);
            }
            return RequestAssetSeizure(request);
        }

        public D13FinanceResult RequestAssetSeizure(D13FinanceCollectionRequest request)
        {
            if (request == null) return Invalid("invalid_request", "资产查封请求不存在。", D13FinanceIds.LegalSeizureRecoveryRoute);
            string eventId = EventId(request.OperationId, "seizure_" + request.DebtId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null)
            {
                var existingSeizure = _seizures.Values.FirstOrDefault(item => item.SourceEventId == eventId);
                return Duplicate(duplicate, FindAccountByDebt(request.DebtId), FindDebt(request.DebtId), null, null, existingSeizure);
            }
            if (!_debts.TryGetValue(request.DebtId ?? string.Empty, out D13FinanceDebtState debt))
                return Failure(request, eventId, "debt_not_found", "查封对应债务不存在。", D13FinanceIds.LegalSeizureRecoveryRoute);
            if (debt.State != D13FinanceDebtLifecycleState.Defaulted && debt.State != D13FinanceDebtLifecycleState.Collection)
                return Failure(request, eventId, "seizure_not_ready", "只有违约并进入催收的债务才能申请资产查封。", D13FinanceIds.LegalSeizureRecoveryRoute, FindAccount(debt.AccountId), debt);
            D13FinanceCollateralState collateral = string.IsNullOrWhiteSpace(request.AssetRef)
                ? debt.Collateral.FirstOrDefault() : debt.Collateral.FirstOrDefault(item => item.AssetRef == request.AssetRef);
            if (collateral == null)
                return Failure(request, eventId, "collateral_not_found", "债务没有可供法律 authority 审查的抵押物。", D13FinanceIds.LegalSeizureRecoveryRoute, FindAccount(debt.AccountId), debt);
            var decision = _collateralAuthority.AuthorizeSeizure(debt.Clone(), collateral.Clone(), request);
            if (decision == null || !decision.Allowed)
                return Failure(request, eventId, decision == null ? "legal_authority_required" : decision.Code,
                    decision == null ? "必须由法律/势力 authority 审查查封。" : decision.Message,
                    D13FinanceIds.LegalSeizureRecoveryRoute, FindAccount(debt.AccountId), debt,
                    decision == null ? null : decision.LegalSourceEventId);
            if (decision.LegalSourceOwnerSystem != D13FinanceIds.LegalOwner || string.IsNullOrWhiteSpace(decision.LegalSourceEventId))
                return Failure(request, eventId, "invalid_legal_source", "资产查封缺少法律/势力 authority 来源。", D13FinanceIds.LegalSeizureRecoveryRoute, FindAccount(debt.AccountId), debt);
            var legalFact = FindAnyFact(decision.LegalSourceEventId, request.SaveRevision);
            if (legalFact == null)
                return Failure(request, eventId, "legal_source_missing", "查封法律来源事件不在统一事实日志中。", D13FinanceIds.LegalSeizureRecoveryRoute, FindAccount(debt.AccountId), debt);
            var seizure = new D13FinanceSeizureState
            {
                SeizureId = "seizure_" + (string.IsNullOrWhiteSpace(request.OperationId) ? request.DebtId : request.OperationId),
                DebtId = debt.DebtId,
                AssetRef = collateral.AssetRef,
                Status = "approved_pending_property_owner",
                LegalSourceEventId = legalFact.EventId,
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0, request.SaveRevision),
            };
            var fact = AppendEvent(eventId, request, "finance_asset_seizure_requested", "法律 authority 已批准查封金融请求；PropertySystem 仍负责实际产权状态。",
                new Dictionary<string, string>
                {
                    ["debt_id"] = debt.DebtId,
                    ["asset_ref"] = collateral.AssetRef,
                    ["seizure_id"] = seizure.SeizureId,
                    ["legal_source_event_id"] = seizure.LegalSourceEventId,
                    ["property_owner"] = D13FinanceIds.PropertyOwner,
                });
            debt.State = D13FinanceDebtLifecycleState.Seized;
            debt.CollectionState = D13FinanceCollectionState.Seized;
            Touch(debt, eventId, request.WorldDay);
            _seizures[seizure.SeizureId] = seizure;
            _revision++;
            var result = Success(fact, FindAccount(debt.AccountId), debt, null, null, null, null, null, seizure.LegalSourceEventId,
                0, 0, 0, FindCash(debt.AccountId), debt.RiskExplanation, "金融债权已进入查封状态；PropertySystem 尚未被本系统直接改写。");
            result.Seizure = seizure.Clone();
            return result;
        }

        public D13FinanceResult RequestBankruptcyProtection(D13FinanceBankruptcyRequest request)
        {
            if (request == null) return Invalid("invalid_request", "破产保护请求不存在。", D13FinanceIds.BankruptcyRecoveryRoute);
            return ApplyBankruptcyProtection(request, false);
        }

        public D13FinanceResult ReleaseBankruptcyProtection(D13FinanceBankruptcyRequest request)
        {
            if (request == null) return Invalid("invalid_request", "破产保护复核请求不存在。", D13FinanceIds.BankruptcyRecoveryRoute);
            return ApplyBankruptcyProtection(request, true);
        }

        private D13FinanceResult ApplyBankruptcyProtection(D13FinanceBankruptcyRequest request, bool release)
        {
            string eventId = EventId(request.OperationId, release ? "bankruptcy_release_" + request.AccountId : "bankruptcy_" + request.AccountId);
            var duplicate = Existing(eventId, request.SaveRevision);
            if (duplicate != null) return Duplicate(duplicate, FindAccount(request.AccountId));
            if (!TryAccount(request.AccountId, out D13FinanceAccountState account))
                return Failure(request, eventId, "account_not_found", "破产保护所属账户不存在。", D13FinanceIds.BankruptcyRecoveryRoute);
            if (!string.Equals(account.OwnerRef, request.DebtorRef, StringComparison.Ordinal))
                return Failure(request, eventId, "bankruptcy_owner_mismatch", "破产保护申请人必须是账户所有者。", D13FinanceIds.BankruptcyRecoveryRoute, account);
            if (!release)
            {
                if (account.BankruptcyProtectionUntilDay >= request.WorldDay && account.BankruptcyProtectionUntilDay > 0)
                    return Failure(request, eventId, "bankruptcy_already_protected", "账户已经处于破产保护期。", D13FinanceIds.BankruptcyRecoveryRoute, account);
                var debtList = _debts.Values.Where(item => item.AccountId == account.AccountId && !IsClosed(item.State)).ToList();
                if (debtList.Count == 0)
                    return Failure(request, eventId, "no_debt_for_protection", "没有需要保护的未结清债务。", D13FinanceIds.BankruptcyRecoveryRoute, account);
                int until = Math.Max(0, request.WorldDay) + ConfigInt(D13FinanceConfiguration.BankruptcyProtectionDays);
                var fact = AppendEvent(eventId, request, "finance_bankruptcy_protection_started", "破产保护已生效；债务暂缓催收但本金和历史记录不被抹除。",
                    new Dictionary<string, string>
                    {
                        ["account_id"] = account.AccountId,
                        ["protected_until_day"] = until.ToString(CultureInfo.InvariantCulture),
                        ["debt_count"] = debtList.Count.ToString(CultureInfo.InvariantCulture),
                        ["worst_case"] = "保护期结束后债务恢复；资产、信用和法律后果仍可能继续。",
                    });
                account.BankruptcyProtectionUntilDay = until;
                account.BankruptcyProtectionCount++;
                AdjustCredit(account, -ConfigInt(D13FinanceConfiguration.DefaultCreditDelta), "申请破产保护，信用下降但催收暂缓。", request.WorldDay, eventId);
                foreach (var debt in debtList)
                {
                    debt.StateBeforeProtection = debt.State;
                    debt.State = D13FinanceDebtLifecycleState.BankruptcyProtected;
                    debt.CollectionState = D13FinanceCollectionState.BankruptcyProtected;
                    Touch(debt, eventId, request.WorldDay);
                }
                Touch(account, eventId, request.WorldDay);
                return Success(fact, account, null, null, null, null, null, null, eventId,
                    0, 0, 0, account.CashBalance, "保护期至世界日 " + until + "；期间利息冻结。", "保护不是免债：保护期结束后仍可能违约、催收或查封。");
            }
            if (account.BankruptcyProtectionUntilDay <= 0 || request.WorldDay < account.BankruptcyProtectionUntilDay)
                return Failure(request, eventId, "bankruptcy_protection_active", "破产保护尚未到期，不能提前结束。", D13FinanceIds.BankruptcyRecoveryRoute, account);
            var restoreFact = AppendEvent(eventId, request, "finance_bankruptcy_protection_ended", "破产保护已结束，债务依据当前日期恢复为应有状态。",
                new Dictionary<string, string> { ["account_id"] = account.AccountId });
            account.BankruptcyProtectionUntilDay = 0;
            foreach (var debt in _debts.Values.Where(item => item.AccountId == account.AccountId && item.State == D13FinanceDebtLifecycleState.BankruptcyProtected))
            {
                debt.State = request.WorldDay > debt.GraceUntilDay ? D13FinanceDebtLifecycleState.Defaulted
                    : request.WorldDay > debt.DueDay ? D13FinanceDebtLifecycleState.PastDue : debt.StateBeforeProtection;
                debt.CollectionState = debt.State == D13FinanceDebtLifecycleState.Defaulted || debt.State == D13FinanceDebtLifecycleState.PastDue
                    ? D13FinanceCollectionState.NoticeIssued : D13FinanceCollectionState.None;
                Touch(debt, eventId, request.WorldDay);
            }
            Touch(account, eventId, request.WorldDay);
            return Success(restoreFact, account, null, null, null, null, null, null, eventId,
                0, 0, 0, account.CashBalance, "保护结束；请查看每笔债务当前应付金额。", "未恢复的债务将按逾期、违约、催收和查封规则继续处理。");
        }

        private D13FinanceResult DeniedClaim(D13FinanceClaimRequest request, string eventId,
            D13FinanceInsurancePolicyState policy, D13FinanceAccountState account, string claimId,
            string code, string message, string route)
        {
            var claim = new D13FinanceInsuranceClaimState
            {
                ClaimId = claimId,
                PolicyId = policy.PolicyId,
                IncidentEventId = request.IncidentEventId,
                IncidentTag = request.IncidentTag,
                LossAmount = Math.Max(1, request.LossAmount),
                ApprovedPayout = 0,
                DeniedAmount = Math.Max(1, request.LossAmount),
                State = D13FinanceClaimState.Denied,
                DecisionCode = code ?? "claim_denied",
                DecisionExplanation = message ?? "理赔被拒。",
                SourceEventId = eventId,
                WorldDay = Math.Max(0, request.WorldDay),
                SaveRevision = Math.Max(0, request.SaveRevision),
            };
            if (request.EvidenceIds != null) claim.EvidenceIds.AddRange(request.EvidenceIds.Where(IsNonEmpty));
            var fact = AppendEvent(eventId, request, code ?? "finance_insurance_claim_denied", message ?? "保险理赔被拒。",
                new Dictionary<string, string>
                {
                    ["policy_id"] = policy.PolicyId,
                    ["claim_id"] = claimId,
                    ["loss_amount"] = claim.LossAmount.ToString(CultureInfo.InvariantCulture),
                    ["denied_amount"] = claim.DeniedAmount.ToString(CultureInfo.InvariantCulture),
                    ["recovery_route"] = route,
                }, WorldEventStatus.Failed, route);
            claim.SourceEventId = fact.EventId;
            policy.ClaimIds.Add(claimId);
            Touch(policy, eventId, request.WorldDay);
            _claims[claimId] = claim;
            _revision++;
            var result = Success(fact, account, null, null, null, claim, policy, null, request.IncidentEventId,
                0, 0, 0, account.CashBalance, "拒赔原因：" + message, "本次赔付为 0；可按恢复路线补充证据或请求保险复核。");
            result.Succeeded = false;
            result.Code = code;
            result.RecoveryRouteId = route;
            result.Claim = claim;
            return result;
        }

        private List<D13FinanceCollateralState> ResolveCollateral(IEnumerable<D13FinanceCollateralRequest> requests,
            string debtorRef, int worldDay, long saveRevision, out string code, out string message)
        {
            code = null;
            message = null;
            var result = new List<D13FinanceCollateralState>();
            foreach (var request in requests ?? Enumerable.Empty<D13FinanceCollateralRequest>())
            {
                if (request == null || string.IsNullOrWhiteSpace(request.AssetRef))
                {
                    code = "collateral_ref_required";
                    message = "抵押物必须有稳定资产引用。";
                    return null;
                }
                var decision = _collateralAuthority.ValidateCollateral(request, debtorRef, worldDay);
                if (decision == null || !decision.Allowed)
                {
                    code = decision == null ? "collateral_authority_required" : decision.Code;
                    message = decision == null ? "必须由 Property/Economy/Legal authority 提供抵押物裁决。" : decision.Message;
                    return null;
                }
                if (decision.ValuationSourceOwnerSystem != D13FinanceIds.EconomyOwner ||
                    decision.LegalSourceOwnerSystem != D13FinanceIds.LegalOwner ||
                    string.IsNullOrWhiteSpace(decision.ValuationSourceEventId) || string.IsNullOrWhiteSpace(decision.LegalSourceEventId))
                {
                    code = "collateral_source_invalid";
                    message = "抵押物必须同时引用 EconomySystem 估值与法律/势力登记事实。";
                    return null;
                }
                if (FindAnyFact(decision.ValuationSourceEventId, saveRevision) == null || FindAnyFact(decision.LegalSourceEventId, saveRevision) == null)
                {
                    code = "collateral_source_missing";
                    message = "抵押物估值或法律来源不在统一事实日志中。";
                    return null;
                }
                result.Add(new D13FinanceCollateralState
                {
                    AssetRef = request.AssetRef,
                    AssetKind = request.AssetKind ?? "property",
                    ClaimValue = Math.Max(0, decision.ClaimValue),
                    ValuationSourceOwnerSystem = decision.ValuationSourceOwnerSystem,
                    ValuationSourceEventId = decision.ValuationSourceEventId,
                    LegalSourceOwnerSystem = decision.LegalSourceOwnerSystem,
                    LegalSourceEventId = decision.LegalSourceEventId,
                    SourceEventId = decision.LegalSourceEventId,
                });
            }
            return result;
        }

        private List<D13FinanceEvidenceRecord> ResolveEvidence(IEnumerable<string> ids, string accountId,
            out string code, out string message)
        {
            code = null;
            message = null;
            var result = new List<D13FinanceEvidenceRecord>();
            foreach (var id in ids ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(id) || !_evidence.TryGetValue(id, out D13FinanceEvidenceRecord item))
                {
                    code = "evidence_missing";
                    message = "至少一条证据没有在 FinanceSystem 登记。";
                    return null;
                }
                if (item.OwnerRef != accountId)
                {
                    code = "evidence_owner_mismatch";
                    message = "证据不属于该保险账户。";
                    return null;
                }
                result.Add(item);
            }
            return result;
        }

        private D13InsuranceQuote BuildInsuranceQuote(D13FinanceInsuranceQuoteRequest request, string sourceEventId)
        {
            int term = request.TermDays <= 0 ? ConfigInt(D13FinanceConfiguration.DefaultLoanTermDays) : request.TermDays;
            long premium = Math.Max(1, PercentCeiling(request.CoverageAmount * (long)ConfigInt(D13FinanceConfiguration.InsurancePremiumPercent) * term, 100L * 30L));
            long deductible = Math.Max(1, PercentCeiling(request.CoverageAmount, ConfigInt(D13FinanceConfiguration.InsuranceDeductiblePercent)));
            long maxPayout = PercentOf(request.CoverageAmount, ConfigInt(D13FinanceConfiguration.ClaimCoveragePercent));
            var result = new D13InsuranceQuote
            {
                QuoteId = "quote_" + sourceEventId,
                Kind = request.Kind,
                AssetRef = request.AssetRef,
                CoverageAmount = request.CoverageAmount,
                Premium = premium,
                DeductibleAmount = deductible,
                MaxPayout = maxPayout,
                StartDay = Math.Max(0, request.WorldDay),
                EndDay = Math.Max(0, request.WorldDay) + term,
                RiskExplanation = "保费 " + premium + "；免赔额 " + deductible + "；最高赔付 " + maxPayout + "；证据至少 " +
                    ConfigInt(D13FinanceConfiguration.MinimumInsuranceEvidence) + " 项。",
                WorstCaseOutcome = "排除项、证据不足或超过保额时赔付为 0 或低于损失；保费和免赔额不会隐藏。",
                SourceEventId = sourceEventId,
            };
            result.Exclusions.AddRange(new[] { "illegal", "fraud", "intentional", "abandoned" });
            result.RequiredEvidenceKinds.Add(D13FinanceEvidenceKind.Incident);
            result.RequiredEvidenceKinds.Add(request.Kind == D13FinanceInsuranceKind.Hull
                ? D13FinanceEvidenceKind.DamageReport : D13FinanceEvidenceKind.Manifest);
            return result;
        }

        private bool ValidInsuranceShape(D13FinanceInsuranceQuoteRequest request, string eventId,
            D13FinanceAccountState account, out string code, out string message)
        {
            code = null;
            message = null;
            int term = request.TermDays <= 0 ? ConfigInt(D13FinanceConfiguration.DefaultLoanTermDays) : request.TermDays;
            if (!Enum.IsDefined(typeof(D13FinanceInsuranceKind), request.Kind) || request.CoverageAmount <= 0)
            {
                code = "insurance_shape_invalid";
                message = "保险种类和保额必须有效。";
                return false;
            }
            if (term < 1 || term > ConfigInt(D13FinanceConfiguration.MaxLoanTermDays))
            {
                code = "insurance_term_invalid";
                message = "保险期限超出统一配置边界。";
                return false;
            }
            if (request.Kind != D13FinanceInsuranceKind.Storm && string.IsNullOrWhiteSpace(request.AssetRef))
            {
                code = "insured_asset_required";
                message = "非泛化风暴保险必须有被保险资产引用。";
                return false;
            }
            return true;
        }

        private static bool CoverageMatches(D13FinanceInsuranceKind kind, string tag)
        {
            tag = (tag ?? string.Empty).ToLowerInvariant();
            switch (kind)
            {
                case D13FinanceInsuranceKind.Cargo: return tag == "cargo_loss" || tag == "storm" || tag == "piracy" || tag == "delay";
                case D13FinanceInsuranceKind.Hull: return tag == "hull_damage" || tag == "storm" || tag == "piracy";
                case D13FinanceInsuranceKind.Storm: return tag == "storm";
                case D13FinanceInsuranceKind.Pirate: return tag == "piracy";
                case D13FinanceInsuranceKind.Delay: return tag == "delay";
                default: return false;
            }
        }

        private static bool IsExcluded(D13FinanceInsurancePolicyState policy, string tag)
        {
            string normalized = (tag ?? string.Empty).ToLowerInvariant();
            return policy.Exclusions != null && policy.Exclusions.Any(item => string.Equals(item, normalized, StringComparison.OrdinalIgnoreCase));
        }

        private static string WorstCaseFor(long principal, int interestRate, List<D13FinanceCollateralState> collateral, long guaranteeCap)
        {
            string result = "本金 " + principal + " 加利息、滞纳金未清偿时进入催收。";
            if (collateral != null && collateral.Count > 0) result += "抵押物可能由法律 authority 审查查封。";
            if (guaranteeCap > 0) result += "担保人责任上限为 " + guaranteeCap + "。";
            result += "破产保护只暂缓，不免除债务。";
            return result;
        }

        private Dictionary<string, string> DebtValues(D13FinanceDebtState debt, long added, long payment)
        {
            return new Dictionary<string, string>
            {
                ["debt_id"] = debt.DebtId,
                ["principal"] = debt.Principal.ToString(CultureInfo.InvariantCulture),
                ["interest_before"] = debt.InterestAccrued.ToString(CultureInfo.InvariantCulture),
                ["late_penalty"] = debt.LatePenalty.ToString(CultureInfo.InvariantCulture),
                ["interest_added"] = added.ToString(CultureInfo.InvariantCulture),
                ["payment"] = payment.ToString(CultureInfo.InvariantCulture),
                ["total_due"] = debt.TotalDue.ToString(CultureInfo.InvariantCulture),
                ["source_event_id"] = debt.SourceEventId,
                ["risk"] = debt.RiskExplanation,
                ["worst_case"] = debt.WorstCaseOutcome,
            };
        }

        private D13FinanceResult Success(FactEvent fact, D13FinanceAccountState account,
            D13FinanceDebtState debt, D13FinanceGuaranteeState guarantee, D13FinanceEvidenceRecord evidence,
            D13FinanceInsuranceClaimState claim, D13FinanceInsurancePolicyState policy, D13InsuranceQuote quote,
            string sourceEventId, long principalPaid, long interestPaid, long fee, long cashAfter,
            string risk, string worstCase)
        {
            return new D13FinanceResult
            {
                Succeeded = true,
                Duplicate = false,
                Code = fact == null || fact.Result == null ? "ok" : fact.Result.ResultCode,
                Message = fact == null || fact.Result == null ? string.Empty : fact.Result.Summary,
                EventId = fact == null ? string.Empty : fact.EventId,
                Fact = fact == null ? null : fact.CloneFact(),
                Account = account == null ? null : account.Clone(),
                Debt = debt == null ? null : debt.Clone(),
                Guarantee = guarantee == null ? null : guarantee.Clone(),
                Evidence = evidence == null ? null : evidence.Clone(),
                Claim = claim == null ? null : claim.Clone(),
                Policy = policy == null ? null : policy.Clone(),
                Quote = quote == null ? null : quote.Clone(),
                PrincipalPaid = principalPaid,
                InterestPaid = interestPaid,
                Fee = fee,
                InterestAdded = fee == 0 ? 0 : (principalPaid == 0 && interestPaid == 0 ? fee : 0),
                CashAfter = cashAfter,
                RiskExplanation = risk,
                WorstCaseOutcome = worstCase,
            };
        }

        private D13FinanceResult Failure(D13FinanceOperationRequest request, string eventId, string code,
            string message, string route, D13FinanceAccountState account = null, D13FinanceDebtState debt = null,
            string legalSource = null, D13FinanceInsurancePolicyState policy = null)
        {
            var fact = AppendEvent(eventId, request, code ?? "finance_failed", message ?? "金融操作失败。",
                new Dictionary<string, string>
                {
                    ["failure_code"] = code ?? "finance_failed",
                    ["recovery_route"] = route ?? D13FinanceIds.DebtRecoveryRoute,
                    ["account_id"] = account == null ? string.Empty : account.AccountId,
                    ["debt_id"] = debt == null ? string.Empty : debt.DebtId,
                    ["policy_id"] = policy == null ? string.Empty : policy.PolicyId,
                    ["legal_source_event_id"] = legalSource ?? string.Empty,
                }, WorldEventStatus.Failed, route);
            return new D13FinanceResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                EventId = fact == null ? eventId : fact.EventId,
                Fact = fact,
                Account = account == null ? null : account.Clone(),
                Debt = debt == null ? null : debt.Clone(),
                Policy = policy == null ? null : policy.Clone(),
                CashAfter = account == null ? 0 : account.CashBalance,
                RecoveryRouteId = route,
                RiskExplanation = debt == null ? null : debt.RiskExplanation,
                WorstCaseOutcome = debt == null ? null : debt.WorstCaseOutcome,
            };
        }

        private D13FinanceResult Invalid(string code, string message, string route)
        {
            return new D13FinanceResult { Succeeded = false, Code = code, Message = message, RecoveryRouteId = route };
        }

        private D13FinanceResult Duplicate(FactEvent fact, D13FinanceAccountState account,
            D13FinanceDebtState debt = null, D13FinanceInsurancePolicyState policy = null,
            D13FinanceInsuranceClaimState claim = null, D13FinanceSeizureState seizure = null)
        {
            return new D13FinanceResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate",
                Message = "同一金融操作已结算。",
                EventId = fact == null ? string.Empty : fact.EventId,
                Fact = fact == null ? null : fact.CloneFact(),
                Account = account == null ? null : account.Clone(),
                Debt = debt == null ? null : debt.Clone(),
                Policy = policy == null ? null : policy.Clone(),
                Claim = claim == null ? null : claim.Clone(),
                Seizure = seizure == null ? null : seizure.Clone(),
                CashAfter = account == null ? 0 : account.CashBalance,
            };
        }

        private FactEvent AppendEvent(string eventId, D13FinanceOperationRequest request, string resultCode,
            string summary, Dictionary<string, string> values, WorldEventStatus status = WorldEventStatus.Normal,
            string recoveryRoute = null)
        {
            var fact = new WorldEvent(eventId, Math.Max(0, request == null ? 0 : request.WorldDay),
                request == null || string.IsNullOrWhiteSpace(request.LocationRef) ? "santa_margarita" : request.LocationRef,
                D13FinanceIds.SystemOwner,
                new EventResult
                {
                    ResultCode = resultCode ?? "finance_result",
                    Summary = summary ?? string.Empty,
                    FactIds = new List<string> { "finance_fact:" + eventId },
                    Values = values ?? new Dictionary<string, string>(),
                },
                new[] { request == null || string.IsNullOrWhiteSpace(request.ActorRef) ? D13FinanceIds.PlayerRef : request.ActorRef },
                new[] { "finance:canonical" }, FilterCauses(request == null ? null : request.CauseEventIds,
                    request == null ? 0 : request.SaveRevision), request == null ? 0 : Math.Max(0, request.SaveRevision),
                status, status == WorldEventStatus.Failed
                    ? new RecoveryRoute
                    {
                        RouteId = recoveryRoute ?? D13FinanceIds.DebtRecoveryRoute,
                        Action = "review_and_retry",
                        ResumeFromEventId = eventId,
                        Description = summary ?? "补充资料后重试金融操作。",
                    } : null);
            return _eventLog.Append(fact).Fact;
        }

        private bool ValidExternalSource(string ownerSystem, string sourceEventId, long saveRevision, out string error)
        {
            error = null;
            if (ownerSystem != D13FinanceIds.EconomyOwner)
            {
                error = "invalid_economy_source";
                return false;
            }
            if (string.IsNullOrWhiteSpace(sourceEventId) || FindAnyFact(sourceEventId, saveRevision) == null)
            {
                error = "economy_source_missing";
                return false;
            }
            return true;
        }

        private FactEvent Existing(string eventId, long saveRevision)
        {
            return _eventLog.Find(eventId, Math.Max(0, saveRevision));
        }

        private FactEvent FindAnyFact(string eventId, long saveRevision)
        {
            if (string.IsNullOrWhiteSpace(eventId)) return null;
            var exact = _eventLog.Find(eventId, Math.Max(0, saveRevision));
            if (exact != null) return exact;
            return _eventLog.Facts.FirstOrDefault(item => item.EventId == eventId);
        }

        private IEnumerable<string> FilterCauses(IEnumerable<string> ids, long saveRevision)
        {
            return (ids ?? Enumerable.Empty<string>()).Where(IsNonEmpty)
                .Where(id => FindAnyFact(id, saveRevision) != null).Distinct(StringComparer.Ordinal);
        }

        private bool TryAccount(string id, out D13FinanceAccountState account)
        {
            account = null;
            if (string.IsNullOrWhiteSpace(id)) return false;
            return _accounts.TryGetValue(id, out account);
        }

        private D13FinanceAccountState FindAccountByDebt(string debtId)
        {
            D13FinanceDebtState debt;
            return _debts.TryGetValue(debtId ?? string.Empty, out debt) ? FindAccount(debt.AccountId) : null;
        }

        private D13FinanceAccountState FindAccountByPolicy(string policyId)
        {
            D13FinanceInsurancePolicyState policy;
            return _policies.TryGetValue(policyId ?? string.Empty, out policy) ? FindAccount(policy.AccountId) : null;
        }

        private long FindCash(string accountId)
        {
            var account = FindAccount(accountId);
            return account == null ? 0 : account.CashBalance;
        }

        private static void Touch(D13FinanceAccountState account, string eventId, int worldDay)
        {
            account.Revision++;
            account.LastEventId = eventId;
            account.LastUpdatedDay = Math.Max(0, worldDay);
        }

        private static void Touch(D13FinanceDebtState debt, string eventId, int worldDay)
        {
            debt.Revision++;
            debt.LastEventId = eventId;
            debt.LastUpdatedDay = Math.Max(0, worldDay);
        }

        private static void Touch(D13FinanceInsurancePolicyState policy, string eventId, int worldDay)
        {
            policy.Revision++;
            policy.LastEventId = eventId;
            policy.LastUpdatedDay = Math.Max(0, worldDay);
        }

        private static void AddCreditReason(D13FinanceAccountState account, string reason)
        {
            if (!string.IsNullOrWhiteSpace(reason) && !account.CreditReasons.Contains(reason)) account.CreditReasons.Add(reason);
        }

        private static void AdjustCredit(D13FinanceAccountState account, int delta, string reason, int worldDay, string eventId)
        {
            account.CreditScore = Math.Max(0, Math.Min(100, account.CreditScore + delta));
            AddCreditReason(account, reason + "（事件 " + eventId + "，第 " + Math.Max(0, worldDay) + " 日）");
        }

        private static bool IsProtected(D13FinanceDebtState debt)
        {
            return debt.State == D13FinanceDebtLifecycleState.BankruptcyProtected || debt.CollectionState == D13FinanceCollectionState.BankruptcyProtected;
        }

        private static bool IsClosed(D13FinanceDebtLifecycleState state)
        {
            return state == D13FinanceDebtLifecycleState.Paid || state == D13FinanceDebtLifecycleState.Seized || state == D13FinanceDebtLifecycleState.WrittenOff;
        }

        private static long PercentOf(long amount, int percent)
        {
            return Math.Max(0, (long)Math.Floor((decimal)Math.Max(0, amount) * Math.Max(0, percent) / 100m));
        }

        private static long PercentCeiling(long amount, long divisorPercent)
        {
            if (amount <= 0 || divisorPercent <= 0) return 0;
            return (long)Math.Ceiling((decimal)amount / divisorPercent);
        }

        private static long SafeAdd(long left, long right)
        {
            if (right > 0 && left > long.MaxValue - right) return long.MaxValue;
            if (right < 0 && left < long.MinValue - right) return long.MinValue;
            return left + right;
        }

        private static long ClampCash(long value)
        {
            decimal maximum = NumericConfiguration.Default.CashBalance.Maximum;
            return Math.Max(0, Math.Min((long)maximum, value));
        }

        private static long ClampMoney(long value)
        {
            return Math.Max(0, Math.Min((long)NumericConfiguration.Default.MoneyAmount.Maximum, value));
        }

        private static long CashMaximum()
        {
            return (long)NumericConfiguration.Default.CashBalance.Maximum;
        }

        private static int ConfigInt(NumericParameterDefinition parameter)
        {
            return (int)parameter.DefaultValue;
        }

        private static string EventId(string operationId, string fallback)
        {
            return "event_d13_finance_" + (string.IsNullOrWhiteSpace(operationId) ? fallback : operationId);
        }

        private static string ClaimId(string eventId)
        {
            return "claim_" + eventId.Replace("event_d13_finance_", string.Empty);
        }

        private static bool IsNonEmpty(string value) { return !string.IsNullOrWhiteSpace(value); }

        private sealed class UnavailableCollateralAuthority : ID13FinanceCollateralAuthority
        {
            public D13FinanceCollateralDecision ValidateCollateral(D13FinanceCollateralRequest request, string debtorRef, int worldDay)
            {
                return new D13FinanceCollateralDecision
                {
                    Allowed = false,
                    Code = "collateral_authority_required",
                    Message = "必须由 Property/Economy/Legal authority 提供抵押物裁决。",
                };
            }

            public D13FinanceSeizureDecision AuthorizeSeizure(D13FinanceDebtState debt,
                D13FinanceCollateralState collateral, D13FinanceCollectionRequest request)
            {
                return new D13FinanceSeizureDecision
                {
                    Allowed = false,
                    Code = "legal_authority_required",
                    Message = "必须由法律/势力 authority 审查资产查封。",
                };
            }
        }
    }
}
