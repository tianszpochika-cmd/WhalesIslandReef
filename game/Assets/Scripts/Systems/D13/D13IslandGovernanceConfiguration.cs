// D13-A05 岛屿治理确定性配置；不保存运行时人口状态。

using System;
using System.Collections.Generic;

using Whalebone.Game.Config;

namespace Whalebone.Systems.D13
{
    public static class D13IslandGovernanceConfiguration
    {
        public static readonly NumericParameterDefinition MaximumResidentsPerOperation = Numeric("d13_governance_max_residents_per_operation", ConfigurationUnit.None, 1m, 1m, 20m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumHealth = Numeric("d13_governance_minimum_health", ConfigurationUnit.Percent, 0m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumSatisfaction = Numeric("d13_governance_minimum_satisfaction", ConfigurationUnit.Percent, 0m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition FoodShortageHealthLoss = Numeric("d13_governance_food_shortage_health_loss", ConfigurationUnit.Percent, 10m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition WaterShortageHealthLoss = Numeric("d13_governance_water_shortage_health_loss", ConfigurationUnit.Percent, 12m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition UnemploymentSatisfactionLoss = Numeric("d13_governance_unemployment_satisfaction_loss", ConfigurationUnit.Percent, 4m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition EducationSatisfactionGain = Numeric("d13_governance_education_satisfaction_gain", ConfigurationUnit.Percent, 2m, 0m, 100m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition SecurityIncidentThresholdBps = Numeric("d13_governance_security_incident_threshold_bps", ConfigurationUnit.Percent, 1200m, 0m, 10000m, "IslandGovernanceSystem", "D13-A05", "D13A05_GovernanceTests", ConfigurationValueType.Integer);

        public static IReadOnlyList<NumericParameterDefinition> Parameters
        {
            get { return new[] { MaximumResidentsPerOperation, MinimumHealth, MinimumSatisfaction, FoodShortageHealthLoss, WaterShortageHealthLoss, UnemploymentSatisfactionLoss, EducationSatisfactionGain, SecurityIncidentThresholdBps }; }
        }

        public static void ThrowIfInvalid()
        {
            var issues = new List<ConfigurationIssue>();
            foreach (var parameter in Parameters)
            {
                if (parameter == null || parameter.Minimum > parameter.Maximum || parameter.DefaultValue < parameter.Minimum || parameter.DefaultValue > parameter.Maximum || string.IsNullOrWhiteSpace(parameter.OwnerSystem) || string.IsNullOrWhiteSpace(parameter.Source) || string.IsNullOrWhiteSpace(parameter.TestEntry))
                    issues.Add(new ConfigurationIssue(parameter == null ? "d13_governance" : parameter.Id, "invalid_metadata_or_range", "岛屿治理配置必须有合法范围、owner、source 和 test entry。"));
            }
            if (issues.Count > 0) throw new InvalidOperationException("非法 D13 岛屿治理配置：" + string.Join("; ", issues));
        }

        public static int Int(NumericParameterDefinition parameter) { return (int)parameter.DefaultValue; }

        private static NumericParameterDefinition Numeric(string id, ConfigurationUnit unit, decimal defaultValue, decimal minimum, decimal maximum, string owner, string source, string testEntry, ConfigurationValueType valueType)
        {
            return new NumericParameterDefinition(id, unit, defaultValue, minimum, maximum, owner, source, testEntry, valueType);
        }
    }
}
