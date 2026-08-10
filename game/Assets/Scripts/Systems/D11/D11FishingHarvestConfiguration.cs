// ============================================================
// D11FishingHarvestConfiguration —— D11-A02 配置唯一入口
//
// 这里登记捕捞实现所需的边界和处理产率；它不改写 D9 既有经济价格，
// 也不直接持有 Ecology/Inventory/Economy 运行时状态。
// ============================================================

using System.Collections.Generic;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D11
{
    public static class D11FishingHarvestConfiguration
    {
        public static readonly NumericParameterDefinition HarvestDepthMeters = Numeric(
            "d11_harvest_depth_meters", ConfigurationUnit.Meter, 2m, 0m, 30m,
            "EcologySystem", "D11-A02 鱼种/深度捕捞入口", "D11A02_FishingHarvestTests");
        public static readonly NumericParameterDefinition GearCapacity = Numeric(
            "d11_gear_capacity", ConfigurationUnit.None, 2m, 1m, 20m,
            "EcologySystem", "D11-A02 渔具定义", "D11A02_FishingHarvestTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition FishQualityScore = Numeric(
            "d11_fish_quality_score", ConfigurationUnit.Percent, 50m, 0m, 100m,
            "EcologySystem", "D11-A02 鱼获品质预览", "D11A02_FishingHarvestTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition ProcessingYieldPercent = Numeric(
            "d11_processing_yield_percent", ConfigurationUnit.Percent, 100m, 1m, 100m,
            "InventorySystem", "D11-A02 腌制/鱼干/鱼油处理定义", "D11A02_FishingHarvestTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition InitialShallowResourceCapacity = Numeric(
            "d11_shallow_resource_capacity", ConfigurationUnit.None, 8m, 1m, 100m,
            "EcologySystem", "D11-A02 贝类/海草资源登记", "D11A02_FishingHarvestTests", ConfigurationValueType.Integer);
        public static readonly NumericParameterDefinition InitialFishSchoolCapacity = Numeric(
            "d11_fish_school_capacity", ConfigurationUnit.None, 12m, 1m, 100m,
            "EcologySystem", "D11-A02 鱼群初始化入口", "D11A02_FishingHarvestTests", ConfigurationValueType.Integer);

        public static readonly IReadOnlyList<NumericParameterDefinition> Parameters = new[]
        {
            HarvestDepthMeters, GearCapacity, FishQualityScore,
            ProcessingYieldPercent, InitialShallowResourceCapacity, InitialFishSchoolCapacity,
        };

        public static IReadOnlyList<ConfigurationIssue> Validate()
        {
            return Validate(Parameters);
        }

        /// <summary>测试/加载器入口：检查传入配置快照，确保非法范围不会进入裁决系统。</summary>
        public static IReadOnlyList<ConfigurationIssue> Validate(
            IReadOnlyList<NumericParameterDefinition> parameters)
        {
            var issues = new List<ConfigurationIssue>();
            foreach (var parameter in parameters ?? new NumericParameterDefinition[0])
            {
                if (parameter == null)
                {
                    issues.Add(new ConfigurationIssue("d11_fishing", "null_parameter", "D11 捕捞配置项不能为空。"));
                    continue;
                }
                if (parameter.Minimum > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "range_order", "最小值不能大于最大值。"));
                if (parameter.DefaultValue < parameter.Minimum || parameter.DefaultValue > parameter.Maximum)
                    issues.Add(new ConfigurationIssue(parameter.Id, "default_outside_range", "默认值必须位于登记边界内。"));
                if (string.IsNullOrEmpty(parameter.OwnerSystem) || string.IsNullOrEmpty(parameter.Source) ||
                    string.IsNullOrEmpty(parameter.TestEntry))
                    issues.Add(new ConfigurationIssue(parameter.Id, "missing_metadata", "配置必须登记 owner、source 和 test entry。"));
            }
            return issues;
        }

        public static void ThrowIfInvalid()
        {
            var issues = Validate();
            if (issues.Count == 0) return;
            throw new System.InvalidOperationException("非法 D11 捕捞配置：" + string.Join("; ", issues));
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
