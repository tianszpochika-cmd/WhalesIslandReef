// ============================================================
// D9WeatherImpactReaders —— 船舶/生态读取天气快照的纯适配层
// ============================================================

using System;

namespace Whalebone.Systems.D9
{
    [Serializable]
    public sealed class D9ShipWeatherImpact
    {
        public D9WeatherState Weather;
        public float WindSpeed;
        public float Visibility;
        public float CurrentSpeed;
        public float SailingSpeedMultiplier;
        public int CrewFatiguePerDay;
        public float HullRisk;
        public bool Hazardous;
    }

    /// <summary>船舶侧只读天气快照；不写 EnvironmentSystem 状态。</summary>
    public static class D9ShipWeatherReader
    {
        public static D9ShipWeatherImpact Read(D9WeatherSeaSnapshot weather)
        {
            var safe = weather ?? D9WeatherSeaSnapshot.CreateDefault();
            return new D9ShipWeatherImpact
            {
                Weather = safe.Weather,
                WindSpeed = safe.WindSpeed,
                Visibility = safe.Visibility,
                CurrentSpeed = safe.CurrentSpeed,
                SailingSpeedMultiplier = SpeedMultiplier(safe),
                CrewFatiguePerDay = safe.CrewFatiguePerDay,
                HullRisk = safe.HullRisk,
                Hazardous = safe.HullRisk >= 0.30f || safe.Visibility < 0.40f,
            };
        }

        public static D9ShipWeatherExposure ResolveExposure(D9WeatherSeaSnapshot weather, int worldDays)
        {
            var impact = Read(weather);
            int days = Math.Max(0, worldDays);
            return new D9ShipWeatherExposure
            {
                Weather = impact.Weather,
                WorldDays = days,
                CrewFatigueDelta = impact.CrewFatiguePerDay * days,
                HullRisk = Clamp01(impact.HullRisk * Math.Max(1, days)),
                SpeedMultiplier = impact.SailingSpeedMultiplier,
                Visibility = impact.Visibility,
            };
        }

        private static float SpeedMultiplier(D9WeatherSeaSnapshot weather)
        {
            if (weather == null) return 1f;
            float windPenalty = weather.WindSpeed / 40f;
            float visibilityPenalty = (1f - weather.Visibility) * 0.25f;
            float currentPenalty = weather.CurrentSpeed * 0.10f;
            return Clamp(1f - windPenalty - visibilityPenalty - currentPenalty, 0.35f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return min;
            return Math.Max(min, Math.Min(max, value));
        }

        private static float Clamp01(float value)
        {
            return Math.Max(0f, Math.Min(1f, value));
        }
    }

    [Serializable]
    public sealed class D9ShipWeatherExposure
    {
        public D9WeatherState Weather;
        public int WorldDays;
        public int CrewFatigueDelta;
        public float HullRisk;
        public float SpeedMultiplier;
        public float Visibility;
    }

    [Serializable]
    public sealed class D9FishWeatherImpact
    {
        public D9WeatherState Weather;
        public float Visibility;
        public float CurrentSpeed;
        public float ActivityMultiplier;
        public float CatchMultiplier;
        public bool ObservationBlocked;
    }

    [Serializable]
    public sealed class D9FishWeatherResult
    {
        public D9WeatherState Weather;
        public int BaseQuantity;
        public int Quantity;
        public float ActivityMultiplier;
        public float CatchMultiplier;
        public bool ObservationBlocked;
    }

    /// <summary>生态侧只读天气快照；不写天气、不直接扣库存或生成鱼获事实。</summary>
    public static class D9EcologyWeatherReader
    {
        public static D9FishWeatherImpact ReadFishConditions(D9WeatherSeaSnapshot weather)
        {
            var safe = weather ?? D9WeatherSeaSnapshot.CreateDefault();
            return new D9FishWeatherImpact
            {
                Weather = safe.Weather,
                Visibility = safe.Visibility,
                CurrentSpeed = safe.CurrentSpeed,
                ActivityMultiplier = safe.FishActivityMultiplier,
                CatchMultiplier = safe.FishCatchMultiplier,
                ObservationBlocked = safe.Visibility < 0.30f,
            };
        }

        public static D9FishWeatherResult ResolveCatch(D9WeatherSeaSnapshot weather, int baseQuantity)
        {
            var impact = ReadFishConditions(weather);
            int safeBase = Math.Max(0, baseQuantity);
            int quantity = (int)Math.Floor(safeBase * Math.Max(0f, impact.CatchMultiplier));
            return new D9FishWeatherResult
            {
                Weather = impact.Weather,
                BaseQuantity = safeBase,
                Quantity = Math.Max(0, quantity),
                ActivityMultiplier = impact.ActivityMultiplier,
                CatchMultiplier = impact.CatchMultiplier,
                ObservationBlocked = impact.ObservationBlocked,
            };
        }
    }
}
