// ============================================================
// D2Contracts —— D2 本地运行时契约（纯 C#）
//
// 归属：Whalebone.Systems.D2
// 目的：统一航行、控制权、昼夜和存档快照的最小字段语义。
// 这里不引用 Unity/Crest/InputSystem/LLM/网络；表现层只能传递输入意图。
// ============================================================

using System;
using Whalebone.Game.Config;

namespace Whalebone.Systems.D2
{
    public enum D2ControlMode
    {
        Character,
        Ship,
        Locked
    }

    [Serializable]
    public struct SailingInput
    {
        public float Throttle;
        public float Rudder;
        public bool Brake;

        public SailingInput(float throttle, float rudder, bool brake = false)
        {
            Throttle = ClampUnit(throttle);
            Rudder = ClampUnit(rudder);
            Brake = brake;
        }

        public SailingInput Normalized()
        {
            return new SailingInput(Throttle, Rudder, Brake);
        }

        private static float ClampUnit(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(-1f, Math.Min(1f, value));
        }
    }

    [Serializable]
    public sealed class SailingSnapshot
    {
        public string ShipId;
        public float PositionX;
        public float PositionZ;
        public float Heading;
        public float Speed;
        public float MaxSpeed;
        public D2ControlMode ControlMode;

        public SailingSnapshot Clone()
        {
            return new SailingSnapshot
            {
                ShipId = ShipId,
                PositionX = PositionX,
                PositionZ = PositionZ,
                Heading = Heading,
                Speed = Speed,
                MaxSpeed = MaxSpeed,
                ControlMode = ControlMode,
            };
        }
    }

    [Serializable]
    public struct DayNightSnapshot
    {
        public int DayIndex;
        public float NormalizedTime;
        public bool Paused;

        public DayNightSnapshot(int dayIndex, float normalizedTime, bool paused = false)
        {
            DayIndex = Math.Max(0, dayIndex);
            NormalizedTime = NormalizeTime(normalizedTime);
            Paused = paused;
        }

        public DayNightSnapshot Normalized()
        {
            return new DayNightSnapshot(DayIndex, NormalizedTime, Paused);
        }

        private static float NormalizeTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return (float)NumericConfiguration.Default.TimeOfDay.DefaultValue;
            value %= 1f;
            if (value < 0f) value += 1f;
            return NumericConfiguration.ClampTimeOfDay(value);
        }
    }

    [Serializable]
    public sealed class D2RuntimeSnapshot
    {
        public SailingSnapshot Sailing = new SailingSnapshot();
        public DayNightSnapshot DayNight;

        public D2RuntimeSnapshot Clone()
        {
            return new D2RuntimeSnapshot
            {
                Sailing = Sailing == null ? new SailingSnapshot() : Sailing.Clone(),
                DayNight = DayNight.Normalized(),
            };
        }
    }
}
