// ============================================================
// D21SeaStateSystem —— 天气输入到海况的确定性裁决（D21-L12）
//
// 本系统只读取 D21WeatherInput 的契约值，返回海况等级、风险、事件概率
// 和可航行性。它不写 D9 EnvironmentSystem、船况、路线、船员或事件日志；
// 视觉波浪、物理浮力和实际驾驶都不参与数值裁决。
// ============================================================

using System;

namespace Whalebone.Systems.D21
{
    /// <summary>海况等级，数值顺序用于确定性阈值比较，不绑定视觉表现。</summary>
    public enum D21SeaStateLevel
    {
        Unknown = 0,
        Calm = 1,
        Slight = 2,
        Moderate = 3,
        Rough = 4,
        VeryRough = 5,
        Storm = 6,
        Extreme = 7
    }

    /// <summary>海况计算的稳定失败码。</summary>
    public static class D21SeaStateFailureCodes
    {
        public const string InputRequired = "d21.sea_state.input_required";
        public const string InvalidInput = "d21.sea_state.invalid_weather_input";
    }

    /// <summary>
    /// 海况只读结果。Input 属性每次返回副本；下游不能通过结果反写天气输入。
    /// </summary>
    [Serializable]
    public sealed class D21SeaStateResult
    {
        private readonly D21WeatherInput _input;

        public int SchemaVersion { get; }
        public bool Success { get; }
        public D21SeaStateLevel Level { get; }
        public int SeverityIndex { get; }
        public float Risk { get; }
        public float EventProbability { get; }
        public bool IsNavigable { get; }
        public string FailureCode { get; }
        public string InputFingerprint { get; }
        public long DeterministicEventSeed { get; }
        public D21WeatherInput Input
        {
            get { return _input == null ? null : _input.Clone(); }
        }

        public float RiskProbability
        {
            get { return Risk; }
        }

        public float EventTriggerProbability
        {
            get { return EventProbability; }
        }

        private D21SeaStateResult(
            bool success,
            D21SeaStateLevel level,
            int severityIndex,
            float risk,
            float eventProbability,
            bool isNavigable,
            string failureCode,
            D21WeatherInput input,
            long deterministicEventSeed)
        {
            SchemaVersion = D21WeatherInputContracts.CurrentSchemaVersion;
            Success = success;
            Level = level;
            SeverityIndex = severityIndex;
            Risk = risk;
            EventProbability = eventProbability;
            IsNavigable = isNavigable;
            FailureCode = failureCode ?? string.Empty;
            _input = input == null ? null : input.Clone();
            InputFingerprint = input == null ? string.Empty : input.Fingerprint();
            DeterministicEventSeed = deterministicEventSeed;
        }

        internal static D21SeaStateResult Accepted(
            D21WeatherInput input,
            D21SeaStateLevel level,
            int severityIndex,
            float risk,
            float eventProbability,
            bool isNavigable,
            long deterministicEventSeed)
        {
            return new D21SeaStateResult(
                true,
                level,
                severityIndex,
                Clamp01(risk),
                Clamp01(eventProbability),
                isNavigable,
                string.Empty,
                input,
                deterministicEventSeed);
        }

        internal static D21SeaStateResult Rejected(D21WeatherInput input, string failureCode)
        {
            return new D21SeaStateResult(
                false,
                D21SeaStateLevel.Unknown,
                0,
                0f,
                0f,
                false,
                failureCode,
                input,
                0L);
        }

        public D21SeaStateResult Clone()
        {
            return new D21SeaStateResult(
                Success,
                Level,
                SeverityIndex,
                Risk,
                EventProbability,
                IsNavigable,
                FailureCode,
                _input,
                DeterministicEventSeed);
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }
    }

    /// <summary>
    /// D21-L12 海况系统。无实例状态，因此同一输入永远得到同一结果。
    /// </summary>
    public sealed class D21SeaStateSystem
    {
        /// <summary>按固定规则计算天气对应海况。</summary>
        public D21SeaStateResult Evaluate(D21WeatherInput input)
        {
            return EvaluateDeterministic(input);
        }

        /// <summary>语义别名，供航程 owner 使用。</summary>
        public D21SeaStateResult Calculate(D21WeatherInput input)
        {
            return EvaluateDeterministic(input);
        }

        /// <summary>语义别名，明确此方法不产生随机事件。</summary>
        public D21SeaStateResult Resolve(D21WeatherInput input)
        {
            return EvaluateDeterministic(input);
        }

        /// <summary>
        /// 纯确定性海况计算入口。先完整校验输入，任何非法值都不部分计算。
        /// </summary>
        public static D21SeaStateResult EvaluateDeterministic(D21WeatherInput input)
        {
            if (input == null)
            {
                return D21SeaStateResult.Rejected(null, D21SeaStateFailureCodes.InputRequired);
            }

            string failureCode;
            if (!D21WeatherInputContracts.TryValidate(input, out failureCode))
            {
                return D21SeaStateResult.Rejected(input, failureCode ?? D21SeaStateFailureCodes.InvalidInput);
            }

            float windRatio = input.WindForce / D21WeatherInputContracts.MaxWindForce;
            float precipitationStress = Clamp01(input.PrecipitationMmPerHour / 50f);
            float visibilityStress = 1f - input.VisibilityFraction;
            float lowPressureStress = Clamp01((1013.25f - input.PressureHpa) / 143.25f);
            float seasonalStress = input.Season == D21WeatherSeason.Wet ? 0.06f :
                (input.Season == D21WeatherSeason.Summer ? 0.025f : 0f);

            // 风力是主导因素，其余输入只能确定性抬高风险，不创建第二天气事实。
            float severityScore = windRatio * 0.70f +
                                  precipitationStress * 0.12f +
                                  visibilityStress * 0.10f +
                                  lowPressureStress * 0.08f +
                                  seasonalStress;
            int severityIndex = (int)Math.Floor(Clamp01(severityScore) * 7f);
            if (input.WindForce >= 10f) severityIndex = Math.Max(severityIndex, 6);
            else if (input.WindForce >= 8f) severityIndex = Math.Max(severityIndex, 5);
            else if (input.WindForce >= 6f) severityIndex = Math.Max(severityIndex, 4);
            else if (input.WindForce >= 4f) severityIndex = Math.Max(severityIndex, 3);
            else if (input.WindForce >= 2f) severityIndex = Math.Max(severityIndex, 2);
            else if (input.WindForce > 1f) severityIndex = Math.Max(severityIndex, 1);
            severityIndex = Math.Max(0, Math.Min(6, severityIndex));

            D21SeaStateLevel level = LevelForIndex(severityIndex);
            float risk = Clamp01(
                0.035f +
                windRatio * 0.56f +
                precipitationStress * 0.12f +
                visibilityStress * 0.18f +
                lowPressureStress * 0.16f +
                (level >= D21SeaStateLevel.Storm ? 0.10f : 0f));

            float eventProbability = Clamp01(
                0.01f +
                risk * 0.36f +
                (level >= D21SeaStateLevel.Rough ? 0.045f : 0f) +
                precipitationStress * 0.025f);

            bool navigable = level < D21SeaStateLevel.Extreme &&
                             input.WindForce <= 10.5f &&
                             input.VisibilityFraction >= 0.12f &&
                             input.PressureHpa >= 900f &&
                             risk < 0.90f;

            long eventSeed = StableSeed(input.Fingerprint());
            return D21SeaStateResult.Accepted(
                input,
                level,
                severityIndex,
                risk,
                eventProbability,
                navigable,
                eventSeed);
        }

        /// <summary>Try 风格入口；拒绝时返回 false，不抛出或修改输入。</summary>
        public static bool TryEvaluate(D21WeatherInput input, out D21SeaStateResult result)
        {
            result = EvaluateDeterministic(input);
            return result != null && result.Success;
        }

        private static D21SeaStateLevel LevelForIndex(int index)
        {
            switch (Math.Max(0, Math.Min(6, index)))
            {
                case 0: return D21SeaStateLevel.Calm;
                case 1: return D21SeaStateLevel.Slight;
                case 2: return D21SeaStateLevel.Moderate;
                case 3: return D21SeaStateLevel.Rough;
                case 4: return D21SeaStateLevel.VeryRough;
                case 5: return D21SeaStateLevel.Storm;
                default: return D21SeaStateLevel.Extreme;
            }
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static long StableSeed(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string text = value ?? string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    hash ^= text[i];
                    hash *= 16777619u;
                }

                return (long)(hash & 0x7fffffff);
            }
        }
    }
}
