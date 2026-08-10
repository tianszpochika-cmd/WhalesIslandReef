// D13-A02 金融计算统一配置。
// 配置只登记边界和默认值；账户、债务、利息和保险状态仍由 FinanceSystem 写入。

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D13
{
    public static class D13FinanceConfiguration
    {
        public const int CurrentSchemaVersion = 1;

        public static readonly NumericParameterDefinition BaseLoanInterestPercent = Numeric(
            "d13_base_loan_interest_percent", ConfigurationUnit.Percent, 12m, 0m, 100m,
            "FinanceSystem", "D13-A02 贷款本金/期限计算", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition CreditRiskSpreadPercent = Numeric(
            "d13_credit_risk_spread_percent", ConfigurationUnit.Percent, 8m, 0m, 100m,
            "FinanceSystem", "D13-A02 信用风险解释", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition LatePenaltyPercent = Numeric(
            "d13_late_penalty_percent", ConfigurationUnit.Percent, 5m, 0m, 100m,
            "FinanceSystem", "D13-A02 到期/违约规则", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition RenewalFeePercent = Numeric(
            "d13_renewal_fee_percent", ConfigurationUnit.Percent, 3m, 0m, 100m,
            "FinanceSystem", "D13-A02 展期费用规则", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition InsurancePremiumPercent = Numeric(
            "d13_insurance_premium_percent", ConfigurationUnit.Percent, 8m, 0m, 100m,
            "FinanceSystem", "D13-A02 保险保费规则", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition InsuranceDeductiblePercent = Numeric(
            "d13_insurance_deductible_percent", ConfigurationUnit.Percent, 15m, 0m, 100m,
            "FinanceSystem", "D13-A02 保险免赔额规则", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition ClaimCoveragePercent = Numeric(
            "d13_claim_coverage_percent", ConfigurationUnit.Percent, 85m, 0m, 100m,
            "FinanceSystem", "D13-A02 保险最高赔付比例", "D13A02_FinanceTests");
        public static readonly NumericParameterDefinition GraceDays = Numeric(
            "d13_debt_grace_days", ConfigurationUnit.WorldDay, 3m, 0m, 30m,
            "FinanceSystem", "D13-A02 到期宽限期", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultLoanTermDays = Numeric(
            "d13_default_loan_term_days", ConfigurationUnit.WorldDay, 30m, 1m, 365m,
            "FinanceSystem", "D13-A02 贷款默认期限", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MaxLoanTermDays = Numeric(
            "d13_max_loan_term_days", ConfigurationUnit.WorldDay, 365m, 1m, 1000m,
            "FinanceSystem", "D13-A02 贷款期限安全上限", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition RenewalTermDays = Numeric(
            "d13_renewal_term_days", ConfigurationUnit.WorldDay, 15m, 1m, 365m,
            "FinanceSystem", "D13-A02 展期默认期限", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MaxRenewals = Numeric(
            "d13_max_renewals", ConfigurationUnit.None, 3m, 0m, 10m,
            "FinanceSystem", "D13-A02 展期次数上限", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition BankruptcyProtectionDays = Numeric(
            "d13_bankruptcy_protection_days", ConfigurationUnit.WorldDay, 7m, 1m, 30m,
            "FinanceSystem", "D13-A02 破产保护期限", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition CollectionFee = Numeric(
            "d13_collection_fee", ConfigurationUnit.SilverCoin, 5m, 0m, 100000m,
            "FinanceSystem", "D13-A02 催收费用", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition InitialCreditScore = Numeric(
            "d13_initial_credit_score", ConfigurationUnit.Percent, 50m, 0m, 100m,
            "FinanceSystem", "D13-A02 信用初始值", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultCreditLimit = Numeric(
            "d13_default_credit_limit", ConfigurationUnit.SilverCoin, 500m, 0m, 1000000000m,
            "FinanceSystem", "D13-A02 默认授信额度", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition TimelyPaymentCreditDelta = Numeric(
            "d13_timely_payment_credit_delta", ConfigurationUnit.Percent, 5m, 0m, 100m,
            "FinanceSystem", "D13-A02 按时还款信用变化", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultCreditDelta = Numeric(
            "d13_default_credit_delta", ConfigurationUnit.Percent, 10m, 0m, 100m,
            "FinanceSystem", "D13-A02 违约信用变化", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition RenewalCreditDelta = Numeric(
            "d13_renewal_credit_delta", ConfigurationUnit.Percent, 1m, 0m, 100m,
            "FinanceSystem", "D13-A02 展期信用变化", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumInsuranceEvidence = Numeric(
            "d13_minimum_insurance_evidence", ConfigurationUnit.None, 2m, 1m, 10m,
            "FinanceSystem", "D13-A02 理赔证据门槛", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition CollateralAdvancePercent = Numeric(
            "d13_collateral_advance_percent", ConfigurationUnit.Percent, 60m, 0m, 100m,
            "FinanceSystem", "D13-A02 抵押授信比例", "D13A02_FinanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition GuaranteeAdvancePercent = Numeric(
            "d13_guarantee_advance_percent", ConfigurationUnit.Percent, 50m, 0m, 100m,
            "FinanceSystem", "D13-A02 担保授信比例", "D13A02_FinanceTests", ConfigurationValueType.Integer);

        public static readonly IReadOnlyList<NumericParameterDefinition> Parameters = new[]
        {
            BaseLoanInterestPercent, CreditRiskSpreadPercent, LatePenaltyPercent, RenewalFeePercent,
            InsurancePremiumPercent, InsuranceDeductiblePercent, ClaimCoveragePercent, GraceDays,
            DefaultLoanTermDays, MaxLoanTermDays, RenewalTermDays, MaxRenewals, BankruptcyProtectionDays,
            CollectionFee, InitialCreditScore, DefaultCreditLimit, TimelyPaymentCreditDelta,
            DefaultCreditDelta, RenewalCreditDelta, MinimumInsuranceEvidence, CollateralAdvancePercent,
            GuaranteeAdvancePercent,
        };

        public static IReadOnlyList<ConfigurationIssue> Validate()
        {
            var issues = new List<ConfigurationIssue>();
            foreach (var parameter in Parameters)
            {
                if (parameter == null)
                {
                    issues.Add(new ConfigurationIssue("d13_finance", "null_parameter", "金融配置项不能为空。"));
                    continue;
                }
                if (parameter.Minimum > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "range_order", "最小值不能大于最大值。"));
                if (parameter.DefaultValue < parameter.Minimum || parameter.DefaultValue > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "default_outside_range", "默认值必须位于登记边界内。"));
                if (string.IsNullOrWhiteSpace(parameter.OwnerSystem) ||
                    string.IsNullOrWhiteSpace(parameter.Source) ||
                    string.IsNullOrWhiteSpace(parameter.TestEntry))
                    issues.Add(new ConfigurationIssue(parameter.Id, "missing_metadata", "金融配置必须登记 owner、source 和 test entry。"));
            }
            if (DefaultLoanTermDays.DefaultValue > MaxLoanTermDays.DefaultValue)
                issues.Add(new ConfigurationIssue(MaxLoanTermDays.Id, "term_order", "默认贷款期限不能超过最大贷款期限。"));
            return issues;
        }

        public static void ThrowIfInvalid()
        {
            var issues = Validate();
            if (issues.Count > 0)
                throw new InvalidOperationException("非法 D13 金融配置：" + string.Join("; ", issues));
        }

        private static NumericParameterDefinition Numeric(string id, ConfigurationUnit unit,
            decimal defaultValue, decimal minimum, decimal maximum, string owner, string source,
            string testEntry, ConfigurationValueType valueType = ConfigurationValueType.Decimal)
        {
            return new NumericParameterDefinition(id, unit, defaultValue, minimum, maximum,
                owner, source, testEntry, valueType);
        }
    }
}
