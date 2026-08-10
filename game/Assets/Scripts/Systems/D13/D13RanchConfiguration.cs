// D13-A04 养殖统一配置；只登记确定性边界，不持有运行时动物状态。

using System;
using System.Collections.Generic;

using Whalebone.Game.Config;

namespace Whalebone.Systems.D13
{
    public static class D13RanchConfiguration
    {
        public static readonly NumericParameterDefinition MaximumLaborPerAction = Numeric(
            "d13_ranch_maximum_labor_per_action", ConfigurationUnit.None, 10m, 1m, 100m,
            "RanchSystem", "D13-A04 劳力操作上限", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumHealthyRelease = Numeric(
            "d13_ranch_minimum_healthy_release", ConfigurationUnit.Percent, 70m, 1m, 100m,
            "RanchSystem", "D13-A04 检疫释放健康门槛", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MissingInputHealthLoss = Numeric(
            "d13_ranch_missing_input_health_loss", ConfigurationUnit.Percent, 15m, 1m, 100m,
            "RanchSystem", "D13-A04 饲料饮水不足后果", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DiseaseHealthLoss = Numeric(
            "d13_ranch_disease_health_loss", ConfigurationUnit.Percent, 20m, 1m, 100m,
            "RanchSystem", "D13-A04 疾病发现后果", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DiseaseTreatmentHealthGain = Numeric(
            "d13_ranch_disease_treatment_health_gain", ConfigurationUnit.Percent, 25m, 1m, 100m,
            "RanchSystem", "D13-A04 检疫治疗恢复", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition ProductHealthBonus = Numeric(
            "d13_ranch_product_health_bonus", ConfigurationUnit.Percent, 5m, 0m, 50m,
            "RanchSystem", "D13-A04 单周期产出健康变化", "D13A04_RanchTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumSalePrice = Numeric(
            "d13_ranch_minimum_sale_price", ConfigurationUnit.SilverCoin, 0m, 0m, 1000000m,
            "EconomySystem", "D13-A04 养殖产品价格下限", "D13A04_RanchTests", ConfigurationValueType.Integer);

        public static IReadOnlyList<NumericParameterDefinition> Parameters
        {
            get
            {
                return new[]
                {
                    MaximumLaborPerAction, MinimumHealthyRelease, MissingInputHealthLoss,
                    DiseaseHealthLoss, DiseaseTreatmentHealthGain, ProductHealthBonus, MinimumSalePrice,
                };
            }
        }

        public static void ThrowIfInvalid()
        {
            var issues = new List<ConfigurationIssue>();
            foreach (var parameter in Parameters)
            {
                if (parameter == null)
                {
                    issues.Add(new ConfigurationIssue("d13_ranch", "null_parameter", "养殖配置项不能为空。"));
                    continue;
                }
                if (parameter.Minimum > parameter.Maximum || parameter.DefaultValue < parameter.Minimum || parameter.DefaultValue > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "invalid_range", "养殖配置默认值必须位于登记边界内。"));
                if (string.IsNullOrWhiteSpace(parameter.OwnerSystem) || string.IsNullOrWhiteSpace(parameter.Source) || string.IsNullOrWhiteSpace(parameter.TestEntry))
                    issues.Add(new ConfigurationIssue(parameter.Id, "missing_metadata", "养殖配置必须登记 owner、source 和 test entry。"));
            }
            if (issues.Count > 0) throw new InvalidOperationException("非法 D13 养殖配置：" + string.Join("; ", issues));
        }

        public static int Int(NumericParameterDefinition parameter)
        {
            return (int)parameter.DefaultValue;
        }

        private static NumericParameterDefinition Numeric(string id, ConfigurationUnit unit,
            decimal defaultValue, decimal minimum, decimal maximum, string owner, string source,
            string testEntry, ConfigurationValueType valueType)
        {
            return new NumericParameterDefinition(id, unit, defaultValue, minimum, maximum,
                owner, source, testEntry, valueType);
        }
    }
}
