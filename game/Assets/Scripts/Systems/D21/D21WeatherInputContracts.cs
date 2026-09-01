// ============================================================
// D21WeatherInputContracts —— D21 天气输入契约（D21-L11）
//
// 本文件定义进入 D21 海况/天气规则的确定性输入，不拥有世界天气事实。
// EnvironmentSystem/D9 仍是天气唯一写者；D21 只消费已校验的输入或向
// D9 提交等价的公开命令。所有数值有固定单位、范围和稳定指纹，避免把
// 系统时钟、Unity 帧时间或不可控随机带入强逻辑。
// ============================================================

using System;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>天气输入使用的季节语义。Unknown 永远不能进入裁决。</summary>
    public enum D21WeatherSeason
    {
        Unknown = 0,
        Dry = 1,
        Wet = 2,
        Spring = 3,
        Summer = 4,
        Autumn = 5,
        Winter = 6
    }

    /// <summary>
    /// 风向语义。Calm 只在风力为零时有效，Variable 表示已裁决但无固定
    /// 罗盘方向的环境输入；Unknown 是未初始化值，不是可玩的风向。
    /// </summary>
    public enum D21WindDirection
    {
        Unknown = 0,
        Calm = 1,
        North = 2,
        NorthEast = 3,
        East = 4,
        SouthEast = 5,
        South = 6,
        SouthWest = 7,
        West = 8,
        NorthWest = 9,
        Variable = 10
    }

    /// <summary>由降水率确定性派生的分档，不替代降水毫米/小时输入。</summary>
    public enum D21PrecipitationBand
    {
        Unknown = 0,
        None = 1,
        Light = 2,
        Moderate = 3,
        Heavy = 4,
        Torrential = 5
    }

    /// <summary>天气输入契约的稳定常量和纯函数。</summary>
    public static class D21WeatherInputContracts
    {
        public const int CurrentSchemaVersion = 1;
        public const string OwnerSystem = "D9.EnvironmentSystem";
        public const int ReferenceMaxLength = 160;
        public const int EventIdMaxLength = 128;
        public const float MaxWindForce = 12f;
        public const float MaxPrecipitationMmPerHour = 500f;
        public const float MinPressureHpa = 870f;
        public const float MaxPressureHpa = 1085f;

        /// <summary>判断季节是否可进入天气裁决。</summary>
        public static bool IsSupportedSeason(D21WeatherSeason season)
        {
            return season >= D21WeatherSeason.Dry && season <= D21WeatherSeason.Winter;
        }

        /// <summary>判断风向是否已被环境 owner 裁决。</summary>
        public static bool IsSupportedWindDirection(D21WindDirection direction)
        {
            return direction >= D21WindDirection.Calm && direction <= D21WindDirection.Variable;
        }

        /// <summary>判断输入是否是有限 IEEE 浮点数。</summary>
        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>把罗盘方向映射为固定角度；无方向输入返回 NaN 以便调用方拒绝。</summary>
        public static float DirectionDegrees(D21WindDirection direction)
        {
            switch (direction)
            {
                case D21WindDirection.Calm: return 0f;
                case D21WindDirection.North: return 0f;
                case D21WindDirection.NorthEast: return 45f;
                case D21WindDirection.East: return 90f;
                case D21WindDirection.SouthEast: return 135f;
                case D21WindDirection.South: return 180f;
                case D21WindDirection.SouthWest: return 225f;
                case D21WindDirection.West: return 270f;
                case D21WindDirection.NorthWest: return 315f;
                case D21WindDirection.Variable: return 360f;
                default: return float.NaN;
            }
        }

        /// <summary>
        /// 将蒲福风力平滑映射为节，不使用随机或浮点环境状态。非有限值返回零，
        /// 但完整输入仍会由 Validate 拒绝。
        /// </summary>
        public static float WindSpeedKnots(float windForce)
        {
            if (!IsFinite(windForce) || windForce <= 0f) return 0f;
            if (windForce >= MaxWindForce) return 63f;

            float lower = (float)Math.Floor(windForce);
            int index = (int)lower;
            float[] beaufortUpperBounds =
            {
                0f, 1f, 3f, 6f, 10f, 16f, 21f, 27f, 34f, 41f, 48f, 56f, 63f
            };
            float fraction = windForce - lower;
            return beaufortUpperBounds[index] +
                   (beaufortUpperBounds[index + 1] - beaufortUpperBounds[index]) * fraction;
        }

        /// <summary>按固定毫米/小时阈值派生降水分档。</summary>
        public static D21PrecipitationBand ClassifyPrecipitation(float millimetersPerHour)
        {
            if (!IsFinite(millimetersPerHour) || millimetersPerHour < 0f ||
                millimetersPerHour > MaxPrecipitationMmPerHour)
                return D21PrecipitationBand.Unknown;
            if (millimetersPerHour <= 0f) return D21PrecipitationBand.None;
            if (millimetersPerHour < 2.5f) return D21PrecipitationBand.Light;
            if (millimetersPerHour < 10f) return D21PrecipitationBand.Moderate;
            if (millimetersPerHour < 50f) return D21PrecipitationBand.Heavy;
            return D21PrecipitationBand.Torrential;
        }

        /// <summary>校验天气输入并返回稳定失败原因，不改变输入对象。</summary>
        public static bool TryValidate(D21WeatherInput input, out string failureCode)
        {
            failureCode = string.Empty;
            if (input == null)
            {
                failureCode = "d21.weather.input_required";
                return false;
            }

            if (input.SchemaVersion != CurrentSchemaVersion)
            {
                failureCode = "d21.weather.schema_version_invalid";
                return false;
            }

            if (!IsSupportedSeason(input.Season))
            {
                failureCode = "d21.weather.season_unknown";
                return false;
            }

            if (!IsSupportedWindDirection(input.WindDirection))
            {
                failureCode = "d21.weather.wind_direction_unknown";
                return false;
            }

            if (!IsFinite(input.WindForce) || input.WindForce < 0f || input.WindForce > MaxWindForce)
            {
                failureCode = "d21.weather.wind_force_out_of_range";
                return false;
            }

            if (input.WindDirection == D21WindDirection.Calm && input.WindForce > 0f)
            {
                failureCode = "d21.weather.calm_direction_with_wind";
                return false;
            }

            if (!IsFinite(input.PrecipitationMmPerHour) || input.PrecipitationMmPerHour < 0f ||
                input.PrecipitationMmPerHour > MaxPrecipitationMmPerHour)
            {
                failureCode = "d21.weather.precipitation_out_of_range";
                return false;
            }

            if (!IsFinite(input.VisibilityFraction) || input.VisibilityFraction < 0f ||
                input.VisibilityFraction > 1f)
            {
                failureCode = "d21.weather.visibility_out_of_range";
                return false;
            }

            if (!IsFinite(input.PressureHpa) || input.PressureHpa < MinPressureHpa ||
                input.PressureHpa > MaxPressureHpa)
            {
                failureCode = "d21.weather.pressure_out_of_range";
                return false;
            }

            if (input.WorldDay < 0)
            {
                failureCode = "d21.weather.world_day_negative";
                return false;
            }

            if (input.SaveRevision < 0)
            {
                failureCode = "d21.weather.save_revision_negative";
                return false;
            }

            if (input.Seed < 0)
            {
                failureCode = "d21.weather.seed_negative";
                return false;
            }

            if (!IsStableReference(input.EventId, EventIdMaxLength))
            {
                failureCode = "d21.weather.event_id_invalid";
                return false;
            }

            if (!IsStableReference(input.SourceId, ReferenceMaxLength))
            {
                failureCode = "d21.weather.source_id_invalid";
                return false;
            }

            if (!IsStableReference(input.LocationRef, ReferenceMaxLength))
            {
                failureCode = "d21.weather.location_ref_invalid";
                return false;
            }

            return true;
        }

        /// <summary>抛出稳定错误，供要求异常式恢复的存档入口使用。</summary>
        public static void Validate(D21WeatherInput input)
        {
            string failureCode;
            if (!TryValidate(input, out failureCode))
                throw new InvalidOperationException(failureCode);
        }

        /// <summary>生成只由契约字段组成的确定性天气指纹。</summary>
        public static string BuildFingerprint(D21WeatherInput input)
        {
            if (input == null) return string.Empty;
            return string.Join("|", new string[]
            {
                input.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                input.EventId ?? string.Empty,
                input.SourceId ?? string.Empty,
                input.LocationRef ?? string.Empty,
                input.WorldDay.ToString(CultureInfo.InvariantCulture),
                input.SaveRevision.ToString(CultureInfo.InvariantCulture),
                input.Seed.ToString(CultureInfo.InvariantCulture),
                input.Season.ToString(),
                input.WindDirection.ToString(),
                input.WindForce.ToString("R", CultureInfo.InvariantCulture),
                input.PrecipitationMmPerHour.ToString("R", CultureInfo.InvariantCulture),
                input.VisibilityFraction.ToString("R", CultureInfo.InvariantCulture),
                input.PressureHpa.ToString("R", CultureInfo.InvariantCulture)
            });
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!letter && !digit && !separator) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// D21 天气输入。字段是序列化载体；进入规则前必须调用 Validate，
    /// 通过 Clone/指纹可以避免下游持有外部可变引用。
    /// </summary>
    [Serializable]
    public sealed class D21WeatherInput
    {
        public int SchemaVersion = D21WeatherInputContracts.CurrentSchemaVersion;
        public string EventId = "d21_weather_input_default";
        public string SourceId = "d9_environment";
        public string LocationRef = "open_sea";
        public int WorldDay;
        public long SaveRevision;
        public long Seed;
        public D21WeatherSeason Season = D21WeatherSeason.Dry;
        public D21WindDirection WindDirection = D21WindDirection.Calm;
        public float WindForce;
        public float PrecipitationMmPerHour;
        public float VisibilityFraction = 1f;
        public float PressureHpa = 1013.25f;

        /// <summary>与常见天气字段命名兼容的只读别名；单位仍为归一化比例。</summary>
        public float Visibility
        {
            get { return VisibilityFraction; }
            set { VisibilityFraction = value; }
        }

        /// <summary>与常见压力字段命名兼容的只读别名，单位为 hPa。</summary>
        public float Pressure
        {
            get { return PressureHpa; }
            set { PressureHpa = value; }
        }

        /// <summary>与常见降水字段命名兼容的只读别名，单位为毫米/小时。</summary>
        public float Precipitation
        {
            get { return PrecipitationMmPerHour; }
            set { PrecipitationMmPerHour = value; }
        }

        /// <summary>降水分档是由毫米/小时值派生的只读结果。</summary>
        public D21PrecipitationBand PrecipitationBand
        {
            get { return D21WeatherInputContracts.ClassifyPrecipitation(PrecipitationMmPerHour); }
        }

        public static D21WeatherInput CreateDefault()
        {
            return new D21WeatherInput();
        }

        public D21WeatherInput Clone()
        {
            return new D21WeatherInput
            {
                SchemaVersion = SchemaVersion,
                EventId = EventId ?? string.Empty,
                SourceId = SourceId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Seed = Seed,
                Season = Season,
                WindDirection = WindDirection,
                WindForce = WindForce,
                PrecipitationMmPerHour = PrecipitationMmPerHour,
                VisibilityFraction = VisibilityFraction,
                PressureHpa = PressureHpa
            };
        }

        public void Validate()
        {
            D21WeatherInputContracts.Validate(this);
        }

        public bool TryValidate(out string failureCode)
        {
            return D21WeatherInputContracts.TryValidate(this, out failureCode);
        }

        public string Fingerprint()
        {
            return D21WeatherInputContracts.BuildFingerprint(this);
        }
    }

    /// <summary>
    /// 向 D9 环境 owner 提交的输入包。它只携带输入和审计字段，不提供
    /// 修改 EnvironmentSystem 快照的方法，因此不会创建第二天气写者。
    /// </summary>
    [Serializable]
    public sealed class D21WeatherInputCommand
    {
        public int SchemaVersion = D21WeatherInputContracts.CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string OwnerSystem = D21WeatherInputContracts.OwnerSystem;
        public D21WeatherInput Input = D21WeatherInput.CreateDefault();

        public D21WeatherInputCommand() { }

        public D21WeatherInputCommand(string commandId, D21WeatherInput input)
        {
            CommandId = commandId ?? string.Empty;
            Input = input == null ? null : input.Clone();
        }

        public void Validate()
        {
            if (SchemaVersion != D21WeatherInputContracts.CurrentSchemaVersion)
                throw new InvalidOperationException("d21.weather.command_schema_version_invalid");
            if (string.IsNullOrWhiteSpace(CommandId))
                throw new InvalidOperationException("d21.weather.command_id_required");
            if (!string.Equals(OwnerSystem, D21WeatherInputContracts.OwnerSystem, StringComparison.Ordinal))
                throw new InvalidOperationException("d21.weather.owner_system_invalid");
            if (Input == null)
                throw new InvalidOperationException("d21.weather.input_required");
            Input.Validate();
        }
    }
}
