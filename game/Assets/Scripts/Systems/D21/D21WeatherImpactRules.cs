// ============================================================
// D21WeatherImpactRules —— 天气/海况只读影响规则（D21-L13）
//
// 这里的数值是向船况、补给、船员、生态和港口 owner 提交的影响结果，
// 不是这些事实本身。规则只消费 D21WeatherInput 与 D21SeaStateResult，
// 不直接改任何外部集合或系统快照；所有计算均为固定数学和稳定 clamp。
// ============================================================

using System;

namespace Whalebone.Systems.D21
{
    /// <summary>天气影响计算的稳定失败码。</summary>
    public static class D21WeatherImpactFailureCodes
    {
        public const string InputRequired = "d21.weather_impact.input_required";
        public const string SeaStateRequired = "d21.weather_impact.sea_state_required";
        public const string SeaStateRejected = "d21.weather_impact.sea_state_rejected";
        public const string InputMismatch = "d21.weather_impact.input_mismatch";
    }

    /// <summary>
    /// 天气影响只读投影。所有属性只读，Input/SeaState 每次返回防御性副本。
    /// </summary>
    [Serializable]
    public sealed class D21WeatherImpactResult
    {
        private readonly D21WeatherInput _input;
        private readonly D21SeaStateResult _seaState;

        public int SchemaVersion { get; }
        public bool Success { get; }
        public string FailureCode { get; }
        public D21SeaStateLevel SeaState { get; }
        public float HullRisk { get; }
        public float SpeedMultiplier { get; }
        public float SailingSpeedMultiplier { get { return SpeedMultiplier; } }
        public float ProvisionConsumptionMultiplier { get; }
        public float WaterConsumptionMultiplier { get; }
        public float FoodConsumptionMultiplier { get; }
        public float FuelConsumptionMultiplier { get; }
        public int CrewFatiguePerDay { get; }
        public float CrewMoraleDelta { get; }
        public float CrewHealthRisk { get; }
        public float FishingActivityMultiplier { get; }
        public float FishingCatchMultiplier { get; }
        public bool FishingAllowed { get; }
        public bool PortAvailable { get; }
        public string PortAvailabilityCode { get; }
        public string InputFingerprint { get; }
        public D21WeatherInput Input
        {
            get { return _input == null ? null : _input.Clone(); }
        }
        public D21SeaStateResult SeaStateResult
        {
            get { return _seaState == null ? null : _seaState.Clone(); }
        }

        private D21WeatherImpactResult(
            bool success,
            string failureCode,
            D21WeatherInput input,
            D21SeaStateResult seaState,
            float hullRisk,
            float speedMultiplier,
            float provisionConsumptionMultiplier,
            float waterConsumptionMultiplier,
            float foodConsumptionMultiplier,
            float fuelConsumptionMultiplier,
            int crewFatiguePerDay,
            float crewMoraleDelta,
            float crewHealthRisk,
            float fishingActivityMultiplier,
            float fishingCatchMultiplier,
            bool fishingAllowed,
            bool portAvailable,
            string portAvailabilityCode)
        {
            SchemaVersion = D21WeatherInputContracts.CurrentSchemaVersion;
            Success = success;
            FailureCode = failureCode ?? string.Empty;
            _input = input == null ? null : input.Clone();
            _seaState = seaState == null ? null : seaState.Clone();
            InputFingerprint = input == null ? string.Empty : input.Fingerprint();
            SeaState = seaState == null ? D21SeaStateLevel.Unknown : seaState.Level;
            HullRisk = Clamp01(hullRisk);
            SpeedMultiplier = Clamp(speedMultiplier, 0f, 1f);
            ProvisionConsumptionMultiplier = Math.Max(0f, provisionConsumptionMultiplier);
            WaterConsumptionMultiplier = Math.Max(0f, waterConsumptionMultiplier);
            FoodConsumptionMultiplier = Math.Max(0f, foodConsumptionMultiplier);
            FuelConsumptionMultiplier = Math.Max(0f, fuelConsumptionMultiplier);
            CrewFatiguePerDay = Math.Max(0, crewFatiguePerDay);
            CrewMoraleDelta = Clamp(crewMoraleDelta, -1f, 1f);
            CrewHealthRisk = Clamp01(crewHealthRisk);
            FishingActivityMultiplier = Math.Max(0f, fishingActivityMultiplier);
            FishingCatchMultiplier = Math.Max(0f, fishingCatchMultiplier);
            FishingAllowed = fishingAllowed;
            PortAvailable = portAvailable;
            PortAvailabilityCode = portAvailabilityCode ?? string.Empty;
        }

        internal static D21WeatherImpactResult Accepted(
            D21WeatherInput input,
            D21SeaStateResult seaState,
            float hullRisk,
            float speedMultiplier,
            float provisionConsumptionMultiplier,
            float waterConsumptionMultiplier,
            float foodConsumptionMultiplier,
            float fuelConsumptionMultiplier,
            int crewFatiguePerDay,
            float crewMoraleDelta,
            float crewHealthRisk,
            float fishingActivityMultiplier,
            float fishingCatchMultiplier,
            bool fishingAllowed,
            bool portAvailable,
            string portAvailabilityCode)
        {
            return new D21WeatherImpactResult(
                true,
                string.Empty,
                input,
                seaState,
                hullRisk,
                speedMultiplier,
                provisionConsumptionMultiplier,
                waterConsumptionMultiplier,
                foodConsumptionMultiplier,
                fuelConsumptionMultiplier,
                crewFatiguePerDay,
                crewMoraleDelta,
                crewHealthRisk,
                fishingActivityMultiplier,
                fishingCatchMultiplier,
                fishingAllowed,
                portAvailable,
                portAvailabilityCode);
        }

        internal static D21WeatherImpactResult Rejected(
            D21WeatherInput input,
            D21SeaStateResult seaState,
            string failureCode)
        {
            return new D21WeatherImpactResult(
                false,
                failureCode,
                input,
                seaState,
                0f,
                0f,
                0f,
                0f,
                0f,
                0f,
                0,
                0f,
                0f,
                0f,
                0f,
                false,
                false,
                "d21.weather_impact.rejected");
        }

        public D21WeatherImpactResult Clone()
        {
            return new D21WeatherImpactResult(
                Success,
                FailureCode,
                _input,
                _seaState,
                HullRisk,
                SpeedMultiplier,
                ProvisionConsumptionMultiplier,
                WaterConsumptionMultiplier,
                FoodConsumptionMultiplier,
                FuelConsumptionMultiplier,
                CrewFatiguePerDay,
                CrewMoraleDelta,
                CrewHealthRisk,
                FishingActivityMultiplier,
                FishingCatchMultiplier,
                FishingAllowed,
                PortAvailable,
                PortAvailabilityCode);
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }
    }

    /// <summary>
    /// D21-L13 天气影响规则。静态无状态，调用顺序不影响结果。
    /// </summary>
    public static class D21WeatherImpactRules
    {
        /// <summary>使用已计算海况，避免同一航程中重复或错配环境输入。</summary>
        public static D21WeatherImpactResult Evaluate(
            D21WeatherInput input,
            D21SeaStateResult seaState)
        {
            if (input == null)
            {
                return D21WeatherImpactResult.Rejected(null, seaState, D21WeatherImpactFailureCodes.InputRequired);
            }

            if (seaState == null)
            {
                return D21WeatherImpactResult.Rejected(input, null, D21WeatherImpactFailureCodes.SeaStateRequired);
            }

            if (!seaState.Success)
            {
                return D21WeatherImpactResult.Rejected(input, seaState, D21WeatherImpactFailureCodes.SeaStateRejected);
            }

            string inputFailure;
            if (!D21WeatherInputContracts.TryValidate(input, out inputFailure))
            {
                return D21WeatherImpactResult.Rejected(input, seaState, inputFailure);
            }

            if (!string.Equals(input.Fingerprint(), seaState.InputFingerprint, StringComparison.Ordinal))
            {
                return D21WeatherImpactResult.Rejected(input, seaState, D21WeatherImpactFailureCodes.InputMismatch);
            }

            float windRatio = input.WindForce / D21WeatherInputContracts.MaxWindForce;
            float precipitationRatio = Clamp01(input.PrecipitationMmPerHour / 100f);
            float visibilityPenalty = 1f - input.VisibilityFraction;
            int stateIndex = Math.Max(0, (int)seaState.Level - (int)D21SeaStateLevel.Calm);
            float stateRatio = stateIndex / 6f;

            float speedMultiplier = Clamp(
                1f - windRatio * 0.44f - visibilityPenalty * 0.16f -
                precipitationRatio * 0.08f - stateRatio * 0.16f,
                0.25f,
                1f);

            float waterMultiplier = 1f + windRatio * 0.28f + precipitationRatio * 0.08f + stateRatio * 0.16f;
            float foodMultiplier = 1f + windRatio * 0.20f + precipitationRatio * 0.10f + stateRatio * 0.12f;
            float provisionMultiplier = (waterMultiplier + foodMultiplier) / 2f;
            float fuelMultiplier = Clamp(1f + windRatio * 0.35f + stateRatio * 0.18f, 1f, 2f);

            int crewFatigue = (int)Math.Ceiling(
                0.5f + input.WindForce * 0.42f + input.PrecipitationMmPerHour / 45f +
                visibilityPenalty * 2.5f + stateIndex * 0.45f);
            crewFatigue = Math.Max(0, Math.Min(24, crewFatigue));
            float moraleDelta = -Clamp(
                windRatio * 0.28f + precipitationRatio * 0.12f + visibilityPenalty * 0.12f + stateRatio * 0.20f,
                0f,
                1f);
            float crewHealthRisk = Clamp01(seaState.Risk * 0.40f + precipitationRatio * 0.12f);

            float fishingActivity = Clamp(
                1f - windRatio * 0.40f - stateRatio * 0.18f +
                Math.Min(input.PrecipitationMmPerHour, 20f) / 400f,
                0.25f,
                1.20f);
            float fishingCatch = Clamp(
                fishingActivity * (0.65f + input.VisibilityFraction * 0.35f),
                0.20f,
                1.20f);
            bool fishingAllowed = seaState.IsNavigable &&
                                  seaState.Level <= D21SeaStateLevel.Rough &&
                                  input.VisibilityFraction >= 0.25f;

            bool portAvailable = seaState.IsNavigable &&
                                 seaState.Level <= D21SeaStateLevel.Moderate &&
                                 input.WindForce <= 7f &&
                                 input.VisibilityFraction >= 0.35f &&
                                 input.PressureHpa >= 960f;
            string portCode = portAvailable
                ? "d21.weather.port_available"
                : (seaState.Level >= D21SeaStateLevel.Storm
                    ? "d21.weather.port_closed_sea_state"
                    : "d21.weather.port_unavailable_conditions");

            return D21WeatherImpactResult.Accepted(
                input,
                seaState,
                seaState.Risk,
                speedMultiplier,
                provisionMultiplier,
                waterMultiplier,
                foodMultiplier,
                fuelMultiplier,
                crewFatigue,
                moraleDelta,
                crewHealthRisk,
                fishingActivity,
                fishingCatch,
                fishingAllowed,
                portAvailable,
                portCode);
        }

        /// <summary>便捷入口：先以相同天气输入确定性计算海况，再计算影响。</summary>
        public static D21WeatherImpactResult Evaluate(D21WeatherInput input)
        {
            D21SeaStateResult seaState = D21SeaStateSystem.EvaluateDeterministic(input);
            return Evaluate(input, seaState);
        }

        /// <summary>语义别名。</summary>
        public static D21WeatherImpactResult Calculate(
            D21WeatherInput input,
            D21SeaStateResult seaState)
        {
            return Evaluate(input, seaState);
        }

        /// <summary>Try 风格入口；拒绝结果仍保留可审计失败码。</summary>
        public static bool TryEvaluate(
            D21WeatherInput input,
            D21SeaStateResult seaState,
            out D21WeatherImpactResult result)
        {
            result = Evaluate(input, seaState);
            return result != null && result.Success;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
