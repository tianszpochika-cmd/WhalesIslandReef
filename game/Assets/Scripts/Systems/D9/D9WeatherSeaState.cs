// ============================================================
// D9WeatherSeaState —— D9 三天气与海况快照
//
// EnvironmentSystem 是天气状态唯一写者；Ship/Ecology 只接收快照并返回
// 各自领域的影响，不持有 EnvironmentSystem，也不修改天气字段。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    public enum D9WeatherState
    {
        Calm,
        Rain,
        Storm,
    }

    [Serializable]
    public sealed class D9WeatherProfile
    {
        public readonly D9WeatherState State;
        public readonly float WindSpeed;
        public readonly float WindDirectionDegrees;
        public readonly float Visibility;
        public readonly float CurrentSpeed;
        public readonly float CurrentDirectionDegrees;
        public readonly int CrewFatiguePerDay;
        public readonly float HullRisk;
        public readonly float FishActivityMultiplier;
        public readonly float FishCatchMultiplier;

        public D9WeatherProfile(D9WeatherState state, float windSpeed, float windDirectionDegrees,
            float visibility, float currentSpeed, float currentDirectionDegrees,
            int crewFatiguePerDay, float hullRisk, float fishActivityMultiplier,
            float fishCatchMultiplier)
        {
            State = state;
            WindSpeed = Math.Max(0f, windSpeed);
            WindDirectionDegrees = NormalizeDegrees(windDirectionDegrees);
            Visibility = Clamp01(visibility);
            CurrentSpeed = Math.Max(0f, currentSpeed);
            CurrentDirectionDegrees = NormalizeDegrees(currentDirectionDegrees);
            CrewFatiguePerDay = Math.Max(0, crewFatiguePerDay);
            HullRisk = Clamp01(hullRisk);
            FishActivityMultiplier = Math.Max(0f, fishActivityMultiplier);
            FishCatchMultiplier = Math.Max(0f, fishCatchMultiplier);
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static float NormalizeDegrees(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            value %= 360f;
            return value < 0f ? value + 360f : value;
        }
    }

    public static class D9WeatherProfiles
    {
        public static D9WeatherProfile Get(D9WeatherState state)
        {
            switch (state)
            {
                case D9WeatherState.Rain:
                    return new D9WeatherProfile(
                        D9WeatherState.Rain,
                        8f, 110f, 0.65f,
                        0.45f, 110f,
                        3, 0.15f, 0.90f, 0.80f);
                case D9WeatherState.Storm:
                    return new D9WeatherProfile(
                        D9WeatherState.Storm,
                        16f, 135f, 0.25f,
                        0.80f, 135f,
                        6, 0.45f, 0.55f, 0.50f);
                default:
                    return new D9WeatherProfile(
                        D9WeatherState.Calm,
                        4f, 90f, 0.95f,
                        0.20f, 90f,
                        1, 0.05f, 1.00f, 1.00f);
            }
        }

        public static D9WeatherState FromLegacy(D3WeatherType weather)
        {
            switch (weather)
            {
                case D3WeatherType.Rain: return D9WeatherState.Rain;
                case D3WeatherType.Storm:
                case D3WeatherType.Hurricane: return D9WeatherState.Storm;
                default: return D9WeatherState.Calm;
            }
        }

        public static D3WeatherType ToLegacy(D9WeatherState weather)
        {
            switch (weather)
            {
                case D9WeatherState.Rain: return D3WeatherType.Rain;
                case D9WeatherState.Storm: return D3WeatherType.Storm;
                default: return D3WeatherType.Calm;
            }
        }
    }

    [Serializable]
    public sealed class D9WeatherSeaSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int WorldDay;
        public int Revision;
        public D9WeatherState Weather = D9WeatherState.Calm;
        public float WindSpeed;
        public float WindDirectionDegrees;
        public float Visibility;
        public float CurrentSpeed;
        public float CurrentDirectionDegrees;
        public int CrewFatiguePerDay;
        public float HullRisk;
        public float FishActivityMultiplier;
        public float FishCatchMultiplier;
        public string LocationRef = "nearshore_saint_marys";
        public string LastEventId = "";
        public List<string> AppliedEventIds = new List<string>();

        public static D9WeatherSeaSnapshot CreateDefault()
        {
            var result = new D9WeatherSeaSnapshot();
            result.ApplyProfile(D9WeatherProfiles.Get(D9WeatherState.Calm));
            return result;
        }

        public D9WeatherSeaSnapshot Clone()
        {
            var result = new D9WeatherSeaSnapshot
            {
                SchemaVersion = SchemaVersion,
                WorldDay = WorldDay,
                Revision = Revision,
                Weather = Weather,
                WindSpeed = WindSpeed,
                WindDirectionDegrees = WindDirectionDegrees,
                Visibility = Visibility,
                CurrentSpeed = CurrentSpeed,
                CurrentDirectionDegrees = CurrentDirectionDegrees,
                CrewFatiguePerDay = CrewFatiguePerDay,
                HullRisk = HullRisk,
                FishActivityMultiplier = FishActivityMultiplier,
                FishCatchMultiplier = FishCatchMultiplier,
                LocationRef = LocationRef,
                LastEventId = LastEventId,
            };
            if (AppliedEventIds != null) result.AppliedEventIds.AddRange(AppliedEventIds);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D9 weather schema_version mismatch: expected " + CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (!Enum.IsDefined(typeof(D9WeatherState), Weather))
                throw new InvalidOperationException("Unknown D9 weather state: " + Weather + ".");
            if (WorldDay < 0 || Revision < 0)
                throw new InvalidOperationException("D9 weather world_day and revision must be non-negative.");
            if (string.IsNullOrWhiteSpace(LocationRef))
                throw new InvalidOperationException("D9 weather location_ref is required.");
            if (AppliedEventIds == null)
                throw new InvalidOperationException("D9 weather applied event list is required.");
            if (!IsFinite(WindSpeed) || WindSpeed < 0f ||
                !IsFinite(CurrentSpeed) || CurrentSpeed < 0f ||
                !IsFinite(WindDirectionDegrees) || WindDirectionDegrees < 0f || WindDirectionDegrees >= 360f ||
                !IsFinite(CurrentDirectionDegrees) || CurrentDirectionDegrees < 0f || CurrentDirectionDegrees >= 360f ||
                !IsFinite(Visibility) || Visibility < 0f || Visibility > 1f ||
                CrewFatiguePerDay < 0 ||
                !IsFinite(HullRisk) || HullRisk < 0f || HullRisk > 1f ||
                !IsFinite(FishActivityMultiplier) || FishActivityMultiplier < 0f ||
                !IsFinite(FishCatchMultiplier) || FishCatchMultiplier < 0f)
                throw new InvalidOperationException("D9 weather impact values are outside their saved ranges.");
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        internal void ApplyProfile(D9WeatherProfile profile)
        {
            Weather = profile.State;
            WindSpeed = profile.WindSpeed;
            WindDirectionDegrees = profile.WindDirectionDegrees;
            Visibility = profile.Visibility;
            CurrentSpeed = profile.CurrentSpeed;
            CurrentDirectionDegrees = profile.CurrentDirectionDegrees;
            CrewFatiguePerDay = profile.CrewFatiguePerDay;
            HullRisk = profile.HullRisk;
            FishActivityMultiplier = profile.FishActivityMultiplier;
            FishCatchMultiplier = profile.FishCatchMultiplier;
        }

        public D3WeatherType LegacyWeather
        {
            get { return D9WeatherProfiles.ToLegacy(Weather); }
        }
    }

    [Serializable]
    public sealed class D9WeatherChangeResult
    {
        public bool Succeeded;
        public bool Applied;
        public bool IsDuplicate;
        public string Code;
        public string EventId;
        public string Message;
        public D9WeatherSeaSnapshot Snapshot;
        public FactEvent Fact;

        internal static D9WeatherChangeResult Success(D9WeatherSeaSnapshot snapshot, string message, string eventId, FactEvent fact)
        {
            return new D9WeatherChangeResult
            {
                Succeeded = true,
                Applied = true,
                Code = "weather_applied",
                EventId = eventId ?? "",
                Message = message,
                Snapshot = snapshot == null ? null : snapshot.Clone(),
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        internal static D9WeatherChangeResult Duplicate(D9WeatherSeaSnapshot snapshot, string eventId, FactEvent fact)
        {
            return new D9WeatherChangeResult
            {
                Succeeded = true,
                Applied = false,
                IsDuplicate = true,
                Code = "duplicate_weather_event",
                EventId = eventId ?? "",
                Message = "天气事件已经结算，未重复改变海况或影响。",
                Snapshot = snapshot == null ? null : snapshot.Clone(),
                Fact = fact == null ? null : fact.CloneFact(),
            };
        }

        internal static D9WeatherChangeResult Rejected(string code, string message, D9WeatherSeaSnapshot snapshot)
        {
            return new D9WeatherChangeResult
            {
                Succeeded = false,
                Applied = false,
                Code = code,
                Message = message,
                Snapshot = snapshot == null ? null : snapshot.Clone(),
            };
        }
    }
}
