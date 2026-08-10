// D13-A03 农业统一数值配置；不把配置类变成农业状态写者。

using System;
using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D13
{
    public static class D13FarmingConfiguration
    {
        public const int CurrentSchemaVersion = 1;

        public static readonly NumericParameterDefinition GrowthPointsPerDay = Numeric(
            "d13_farming_growth_points_per_day", ConfigurationUnit.None, 10m, 1m, 100m,
            "FarmingSystem", "D13-A03 作物成熟度规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition WaterGrowthBonus = Numeric(
            "d13_farming_water_growth_bonus", ConfigurationUnit.None, 4m, 0m, 50m,
            "FarmingSystem", "D13-A03 浇水成长规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition LaborGrowthBonus = Numeric(
            "d13_farming_labor_growth_bonus", ConfigurationUnit.None, 3m, 0m, 50m,
            "FarmingSystem", "D13-A03 劳力成长规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition LowMoistureThreshold = Numeric(
            "d13_farming_low_moisture_threshold", ConfigurationUnit.Percent, 30m, 0m, 100m,
            "FarmingSystem", "D13-A03 缺水规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition PestThreshold = Numeric(
            "d13_farming_pest_threshold", ConfigurationUnit.Percent, 35m, 0m, 100m,
            "FarmingSystem", "D13-A03 病虫害规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition HarvestFertilityCost = Numeric(
            "d13_farming_harvest_fertility_cost", ConfigurationUnit.Percent, 5m, 0m, 50m,
            "EcologySystem", "D13-A03 收获肥力后果", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition RotationFertilityBonus = Numeric(
            "d13_farming_rotation_fertility_bonus", ConfigurationUnit.Percent, 8m, 0m, 50m,
            "EcologySystem", "D13-A03 轮作恢复规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultWaterCapacity = Numeric(
            "d13_farming_default_water_capacity", ConfigurationUnit.None, 100m, 1m, 1000m,
            "EcologySystem", "D13-A03 水源容量规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultWaterPerAction = Numeric(
            "d13_farming_default_water_per_action", ConfigurationUnit.None, 20m, 1m, 100m,
            "EcologySystem", "D13-A03 浇水消耗规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition DefaultStormIntensity = Numeric(
            "d13_farming_default_storm_intensity", ConfigurationUnit.Percent, 35m, 1m, 100m,
            "EcologySystem", "D13-A03 风暴后果规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MaximumLaborPerAction = Numeric(
            "d13_farming_maximum_labor_per_action", ConfigurationUnit.None, 10m, 1m, 100m,
            "FarmingSystem", "D13-A03 劳力上限规则", "D13A03_FarmingTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition MinimumSalePrice = Numeric(
            "d13_farming_minimum_sale_price", ConfigurationUnit.SilverCoin, 0m, 0m, 1000000m,
            "EconomySystem", "D13-A03 销售价格下限", "D13A03_FarmingTests", ConfigurationValueType.Integer);

        public static void ThrowIfInvalid()
        {
            var issues = new List<ConfigurationIssue>();
            foreach (var parameter in Parameters)
            {
                if (parameter == null)
                {
                    issues.Add(new ConfigurationIssue("d13_farming", "null_parameter", "农业配置项不能为空。"));
                    continue;
                }
                if (parameter.Minimum > parameter.Maximum || parameter.DefaultValue < parameter.Minimum || parameter.DefaultValue > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "invalid_range", "农业配置默认值必须位于登记边界内。"));
                if (string.IsNullOrWhiteSpace(parameter.OwnerSystem) || string.IsNullOrWhiteSpace(parameter.Source) || string.IsNullOrWhiteSpace(parameter.TestEntry))
                    issues.Add(new ConfigurationIssue(parameter.Id, "missing_metadata", "农业配置必须登记 owner、source 和 test entry。"));
            }
            if (issues.Count > 0) throw new InvalidOperationException("非法 D13 农业配置：" + string.Join("; ", issues));
        }

        public static NumericParameterDefinition[] Parameters
        {
            get
            {
                return new[]
                {
                    GrowthPointsPerDay, WaterGrowthBonus, LaborGrowthBonus,
                    LowMoistureThreshold, PestThreshold, HarvestFertilityCost,
                    RotationFertilityBonus, DefaultWaterCapacity, DefaultWaterPerAction,
                    DefaultStormIntensity, MaximumLaborPerAction, MinimumSalePrice,
                };
            }
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
