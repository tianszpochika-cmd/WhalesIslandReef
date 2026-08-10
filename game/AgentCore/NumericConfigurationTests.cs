using System;

namespace Whalebone.Game.Config
{
    /// <summary>D8-A06 纯 C# 配置测试入口；不依赖 Unity、网络或第三方库。</summary>
    public static class NumericConfigurationTests
    {
        public static int RunAll()
        {
            int failed = 0;
            failed += Check("统一配置默认值合法", DefaultsAreValid);
            failed += Check("统一参数元数据完整", ParameterMetadataIsComplete);
            failed += Check("边界读取保持 D7 行为", ClampsUseRegisteredBounds);
            failed += Check("非法数值范围可被发现", InvalidRangeIsRejected);
            failed += Check("非法枚举可被发现", InvalidEnumIsRejected);
            return failed;
        }

        private static int Check(string name, Func<bool> test)
        {
            try
            {
                bool ok = test();
                Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
                return ok ? 0 : 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("[EX] " + name + " : " + e.Message);
                return 1;
            }
        }

        private static bool DefaultsAreValid()
        {
            return NumericConfiguration.Validate(NumericConfiguration.Default).Count == 0
                && NumericConfiguration.Default.StartingMoney.DefaultValue == 500m
                && NumericConfiguration.Default.SecondsPerWorldDay.DefaultValue == 600m
                && NumericConfiguration.Default.CombatDistance.DefaultValue == 35m
                && NumericConfiguration.Default.BoardingDistance.DefaultValue == 28m
                && NumericConfiguration.Default.DefaultWeather == WeatherState.Calm
                && NumericConfiguration.Default.DefaultRarity == Rarity.Common
                && NumericConfiguration.Default.DefaultLegalStatus == LegalStatus.Legal
                && NumericConfiguration.Default.DefaultQuality == QualityGrade.Standard;
        }

        private static bool ParameterMetadataIsComplete()
        {
            if (NumericConfiguration.NumericParameters.Count != 20
                || NumericConfiguration.EnumParameters.Count != 4) return false;
            foreach (var parameter in NumericConfiguration.NumericParameters)
            {
                if (string.IsNullOrEmpty(parameter.Id)
                    || string.IsNullOrEmpty(parameter.OwnerSystem)
                    || string.IsNullOrEmpty(parameter.Source)
                    || string.IsNullOrEmpty(parameter.TestEntry)
                    || parameter.Minimum > parameter.Maximum) return false;
            }
            foreach (var parameter in NumericConfiguration.EnumParameters)
            {
                if (string.IsNullOrEmpty(parameter.Id)
                    || string.IsNullOrEmpty(parameter.DefaultValue)
                    || parameter.AllowedValues.Count == 0
                    || string.IsNullOrEmpty(parameter.OwnerSystem)
                    || string.IsNullOrEmpty(parameter.Source)
                    || string.IsNullOrEmpty(parameter.TestEntry)) return false;
            }
            return true;
        }

        private static bool ClampsUseRegisteredBounds()
        {
            return NumericConfiguration.ClampRelation(150f) == 100f
                && NumericConfiguration.ClampRelationProposal(-20f) == -10f
                && NumericConfiguration.ClampHealth(-1f) == 0f
                && NumericConfiguration.ClampCombatDistance(200f) == 100f
                && NumericConfiguration.ClampBoardingDistance(-1f) == 0f
                && NumericConfiguration.ClampMoney(2000000000L) == 1000000000L
                && NumericConfiguration.ClampCash(-1) == 0
                && NumericConfiguration.ClampItemUnitVolume(0) == 1
                && NumericConfiguration.ClampWantedLevel(120) == 100;
        }

        private static bool InvalidRangeIsRejected()
        {
            var invalid = NumericConfiguration.Default.With(
                relationProposalDelta: new NumericRange(0m, -11m, 10m));
            var issues = NumericConfiguration.Validate(invalid);
            return Contains(issues, "relation_proposal_delta", "outside_schema");
        }

        private static bool InvalidEnumIsRejected()
        {
            var invalid = NumericConfiguration.Default.With(defaultWeather: (WeatherState)999);
            var issues = NumericConfiguration.Validate(invalid);
            return Contains(issues, "weather_state", "unknown_enum");
        }

        private static bool Contains(
            System.Collections.Generic.IReadOnlyList<ConfigurationIssue> issues, string parameterId, string code)
        {
            foreach (var issue in issues)
                if (issue.ParameterId == parameterId && issue.Code == code) return true;
            return false;
        }
    }
}
