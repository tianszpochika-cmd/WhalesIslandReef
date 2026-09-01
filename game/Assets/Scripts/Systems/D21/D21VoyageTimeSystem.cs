// ============================================================
// D21VoyageTimeSystem —— D21-L15 固定 Tick 航程时钟
//
// 航程时间是 D21 航程 owner 的逻辑事实。这里使用 milli-nautical-mile /
// milli-knot 定点数和固定 15 分钟 Tick，避免 float 帧时间、系统时钟或
// 场景位置成为抵达条件。调用方提交环境/船况快照计算出的速度；本文件
// 只裁决时间、昼夜和抵达估算，不写入船、天气、货物或船员事实。
// ============================================================

using System;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>固定 Tick 的昼夜阶段；阶段边界是逻辑常量，不是光照表现。</summary>
    public enum D21DayNightPhase
    {
        Night = 0,
        Dawn = 1,
        Day = 2,
        Dusk = 3,
    }

    /// <summary>
    /// D21 所有固定步计算共用的显式逻辑上下文。
    /// WorldDay/MinuteOfDay 由上游世界快照提交，不从系统时钟读取。
    /// </summary>
    [Serializable]
    public sealed class D21TickContext
    {
        public const int CurrentSchemaVersion = 1;
        public const int FixedTickMinutes = 15;
        public const int MinutesPerDay = 24 * 60;

        public int SchemaVersion = CurrentSchemaVersion;
        public int WorldDay;
        public int MinuteOfDay;
        public int TickIndex;
        public int TickMinutes = FixedTickMinutes;
        public long SaveRevision;
        public long VoyageSeed;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;

        public D21TickContext Clone()
        {
            return new D21TickContext
            {
                SchemaVersion = SchemaVersion,
                WorldDay = WorldDay,
                MinuteOfDay = MinuteOfDay,
                TickIndex = TickIndex,
                TickMinutes = TickMinutes,
                SaveRevision = SaveRevision,
                VoyageSeed = VoyageSeed,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 tick schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (WorldDay < 0 || MinuteOfDay < 0 || MinuteOfDay >= MinutesPerDay)
                throw new InvalidOperationException("D21 tick world clock is outside its saved range.");
            if (TickIndex < 0 || TickMinutes != FixedTickMinutes)
                throw new InvalidOperationException("D21 tick must use a non-negative index and the fixed 15-minute step.");
            if (SaveRevision < 0 || VoyageSeed < 0)
                throw new InvalidOperationException("D21 tick save_revision and voyage_seed must be non-negative.");
        }

        public long AbsoluteMinute
        {
            get { return checked((long)WorldDay * MinutesPerDay + MinuteOfDay); }
        }
    }

    /// <summary>固定点航程时间状态；该对象是返回值的一部分，调用方负责提交给 owner。</summary>
    [Serializable]
    public sealed class D21VoyageTimeState
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public long RouteDistanceMilliNm;
        public long DistanceTravelledMilliNm;
        /// <summary>速度乘 TickMinutes 后除以 60 的余数，范围为 0..59。</summary>
        public long DistanceRemainderNumerator;
        public int ElapsedTicks;
        public int StartWorldDay;
        public int StartMinuteOfDay;
        public long LastSaveRevision;
        public bool IsArrived;
        public D21DayNightPhase CurrentPhase = D21DayNightPhase.Night;

        public static D21VoyageTimeState CreateInitial(
            string voyageId,
            string shipId,
            long routeDistanceMilliNm,
            int startWorldDay,
            int startMinuteOfDay,
            long saveRevision = 0)
        {
            var state = new D21VoyageTimeState
            {
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                RouteDistanceMilliNm = routeDistanceMilliNm,
                StartWorldDay = startWorldDay,
                StartMinuteOfDay = startMinuteOfDay,
                LastSaveRevision = saveRevision,
            };
            state.CurrentPhase = D21VoyageTimeSystem.GetPhase(startMinuteOfDay);
            state.IsArrived = routeDistanceMilliNm == 0;
            state.Validate();
            return state;
        }

        public D21VoyageTimeState Clone()
        {
            return new D21VoyageTimeState
            {
                SchemaVersion = SchemaVersion,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteDistanceMilliNm = RouteDistanceMilliNm,
                DistanceTravelledMilliNm = DistanceTravelledMilliNm,
                DistanceRemainderNumerator = DistanceRemainderNumerator,
                ElapsedTicks = ElapsedTicks,
                StartWorldDay = StartWorldDay,
                StartMinuteOfDay = StartMinuteOfDay,
                LastSaveRevision = LastSaveRevision,
                IsArrived = IsArrived,
                CurrentPhase = CurrentPhase,
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 voyage time schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (RouteDistanceMilliNm < 0 || DistanceTravelledMilliNm < 0 ||
                DistanceTravelledMilliNm > RouteDistanceMilliNm)
                throw new InvalidOperationException("D21 voyage distance is outside its saved range.");
            if (DistanceRemainderNumerator < 0 || DistanceRemainderNumerator >= 60)
                throw new InvalidOperationException("D21 voyage distance remainder must be in [0, 60).");
            if (ElapsedTicks < 0 || StartWorldDay < 0 || StartMinuteOfDay < 0 ||
                StartMinuteOfDay >= D21TickContext.MinutesPerDay || LastSaveRevision < 0)
                throw new InvalidOperationException("D21 voyage time counters are outside their saved ranges.");
            if (!Enum.IsDefined(typeof(D21DayNightPhase), CurrentPhase))
                throw new InvalidOperationException("D21 voyage time phase is unknown.");
            if (IsArrived != (DistanceTravelledMilliNm >= RouteDistanceMilliNm))
                throw new InvalidOperationException("D21 voyage arrived flag disagrees with travelled distance.");
        }

        public string Fingerprint
        {
            get
            {
                return "d21time_v" + SchemaVersion.ToString(CultureInfo.InvariantCulture) +
                    "_" + (VoyageId ?? string.Empty) + "_" + ElapsedTicks.ToString(CultureInfo.InvariantCulture) +
                    "_" + DistanceTravelledMilliNm.ToString(CultureInfo.InvariantCulture) +
                    "_" + DistanceRemainderNumerator.ToString(CultureInfo.InvariantCulture) +
                    "_" + (IsArrived ? "arrived" : "sailing");
            }
        }
    }

    /// <summary>航程时间系统消费的只读速度输入；速度由船况/天气 owner 预先裁决。</summary>
    [Serializable]
    public sealed class D21VoyageTimeInput
    {
        public int SchemaVersion = D21VoyageTimeState.CurrentSchemaVersion;
        public long RouteDistanceMilliNm;
        public long BaseSpeedMilliKnots;
        public int SpeedPermille = 1000;
        public int CurrentPermille = 1000;
        public int TickMinutes = D21TickContext.FixedTickMinutes;

        /// <summary>兼容更直观的调用名；canonical 字段仍是 BaseSpeedMilliKnots。</summary>
        public long SpeedMilliKnots
        {
            get { return BaseSpeedMilliKnots; }
            set { BaseSpeedMilliKnots = value; }
        }

        public D21VoyageTimeInput Clone()
        {
            return new D21VoyageTimeInput
            {
                SchemaVersion = SchemaVersion,
                RouteDistanceMilliNm = RouteDistanceMilliNm,
                BaseSpeedMilliKnots = BaseSpeedMilliKnots,
                SpeedPermille = SpeedPermille,
                CurrentPermille = CurrentPermille,
                TickMinutes = TickMinutes,
            };
        }

        public void Validate()
        {
            if (SchemaVersion != D21VoyageTimeState.CurrentSchemaVersion)
                throw new InvalidOperationException("D21 voyage time input schema_version mismatch.");
            if (RouteDistanceMilliNm < 0 || BaseSpeedMilliKnots < 0 ||
                SpeedPermille < 0 || SpeedPermille > 2000 ||
                CurrentPermille < 0 || CurrentPermille > 2000 ||
                TickMinutes != D21TickContext.FixedTickMinutes)
                throw new InvalidOperationException("D21 voyage time input is outside its fixed-point ranges.");
        }

        public long EffectiveSpeedMilliKnots
        {
            get
            {
                decimal result = (decimal)BaseSpeedMilliKnots * SpeedPermille * CurrentPermille;
                result /= 1000000m;
                if (result <= 0m) return 0;
                if (result >= long.MaxValue) return long.MaxValue;
                return (long)decimal.Truncate(result);
            }
        }
    }

    /// <summary>抵达估算；Reachable=false 表示当前速度输入无法抵达，不是自动改写状态。</summary>
    [Serializable]
    public sealed class D21VoyageArrivalEstimate
    {
        public bool Reachable;
        public int RequiredTicks;
        public int EstimatedArrivalTick;
        public int EstimatedWorldDay;
        public int EstimatedMinuteOfDay;
        public D21DayNightPhase EstimatedPhase;
        public string Code = string.Empty;
    }

    /// <summary>固定步推进结果；失败时 State 是原状态的 clone，不含部分提交。</summary>
    [Serializable]
    public sealed class D21VoyageTimeResult
    {
        public bool Accepted;
        public bool IsDuplicate;
        public bool IsArrived;
        public string Code = string.Empty;
        public long DistanceAdvancedMilliNm;
        public long RemainingDistanceMilliNm;
        public int ProjectedWorldDay;
        public int ProjectedMinuteOfDay;
        public D21DayNightPhase ProjectedPhase;
        public D21VoyageArrivalEstimate ArrivalEstimate;
        public D21VoyageTimeState State;
    }

    /// <summary>纯函数固定 Tick 时间裁决器。</summary>
    public static class D21VoyageTimeSystem
    {
        public const int CurrentSchemaVersion = D21VoyageTimeState.CurrentSchemaVersion;
        public const int FixedTickMinutes = D21TickContext.FixedTickMinutes;
        public const int MilliUnitsPerNauticalMile = 1000;
        public const int MilliUnitsPerKnot = 1000;

        public static D21TickContext CreateTickContext(
            int worldDay,
            int minuteOfDay,
            int tickIndex,
            long saveRevision,
            long voyageSeed,
            string voyageId = "",
            string shipId = "")
        {
            var context = new D21TickContext
            {
                WorldDay = worldDay,
                MinuteOfDay = minuteOfDay,
                TickIndex = tickIndex,
                SaveRevision = saveRevision,
                VoyageSeed = voyageSeed,
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
            };
            context.Validate();
            return context;
        }

        public static D21DayNightPhase GetPhase(int minuteOfDay)
        {
            int minute = NormalizeMinute(minuteOfDay);
            if (minute < 6 * 60) return D21DayNightPhase.Night;
            if (minute < 7 * 60) return D21DayNightPhase.Dawn;
            if (minute < 18 * 60) return D21DayNightPhase.Day;
            if (minute < 19 * 60) return D21DayNightPhase.Dusk;
            return D21DayNightPhase.Night;
        }

        public static D21VoyageTimeResult Advance(
            D21VoyageTimeState current,
            D21VoyageTimeInput input,
            D21TickContext context)
        {
            if (current == null) return Rejected(null, "missing_state");
            if (input == null) return Rejected(current, "missing_input");
            if (context == null) return Rejected(current, "missing_tick_context");

            D21VoyageTimeState original = current.Clone();
            try
            {
                original.Validate();
                input.Validate();
                context.Validate();

                if (!string.IsNullOrEmpty(original.VoyageId) && !string.IsNullOrEmpty(context.VoyageId) &&
                    !string.Equals(original.VoyageId, context.VoyageId, StringComparison.Ordinal))
                    return Rejected(original, "voyage_id_mismatch");
                if (!string.IsNullOrEmpty(original.ShipId) && !string.IsNullOrEmpty(context.ShipId) &&
                    !string.Equals(original.ShipId, context.ShipId, StringComparison.Ordinal))
                    return Rejected(original, "ship_id_mismatch");
                if (context.TickIndex != original.ElapsedTicks)
                    return Rejected(original, "tick_index_mismatch");
                if (input.TickMinutes != context.TickMinutes)
                    return Rejected(original, "tick_size_mismatch");
                if (original.RouteDistanceMilliNm != 0 && input.RouteDistanceMilliNm != 0 &&
                    original.RouteDistanceMilliNm != input.RouteDistanceMilliNm)
                    return Rejected(original, "route_distance_mismatch");

                long routeDistance = original.RouteDistanceMilliNm == 0
                    ? input.RouteDistanceMilliNm
                    : original.RouteDistanceMilliNm;
                if (routeDistance < 0) return Rejected(original, "negative_route_distance");

                int expectedWorldDay;
                int expectedMinute;
                CalculateClock(original.StartWorldDay, original.StartMinuteOfDay,
                    original.ElapsedTicks, context.TickMinutes, out expectedWorldDay, out expectedMinute);

                // 只有全新的零时刻状态可以从显式 context 建立起始时钟；之后每步必须严格连续。
                if (original.ElapsedTicks == 0 && original.DistanceTravelledMilliNm == 0 &&
                    original.StartWorldDay == 0 && original.StartMinuteOfDay == 0 &&
                    (context.WorldDay != 0 || context.MinuteOfDay != 0))
                {
                    original.StartWorldDay = context.WorldDay;
                    original.StartMinuteOfDay = context.MinuteOfDay;
                    expectedWorldDay = context.WorldDay;
                    expectedMinute = context.MinuteOfDay;
                }
                if (expectedWorldDay != context.WorldDay || expectedMinute != context.MinuteOfDay)
                    return Rejected(original, "logical_clock_mismatch");

                original.RouteDistanceMilliNm = routeDistance;
                if (routeDistance == 0 || original.IsArrived)
                {
                    original.IsArrived = true;
                    original.CurrentPhase = GetPhase(context.MinuteOfDay);
                    return Accepted(original, "already_arrived", 0, context, input);
                }

                long speed = input.EffectiveSpeedMilliKnots;
                long numerator = checked(speed * (long)context.TickMinutes + original.DistanceRemainderNumerator);
                long advanced = numerator / 60L;
                long remainder = numerator % 60L;
                long remainingBefore = original.RouteDistanceMilliNm - original.DistanceTravelledMilliNm;
                if (advanced >= remainingBefore)
                {
                    advanced = remainingBefore;
                    remainder = 0;
                    original.DistanceTravelledMilliNm = original.RouteDistanceMilliNm;
                    original.IsArrived = true;
                }
                else
                {
                    original.DistanceTravelledMilliNm = checked(original.DistanceTravelledMilliNm + advanced);
                    original.IsArrived = false;
                }
                original.DistanceRemainderNumerator = remainder;
                original.ElapsedTicks = checked(original.ElapsedTicks + 1);
                original.LastSaveRevision = context.SaveRevision;

                CalculateClock(original.StartWorldDay, original.StartMinuteOfDay,
                    original.ElapsedTicks, context.TickMinutes, out int projectedDay, out int projectedMinute);
                original.CurrentPhase = GetPhase(projectedMinute);
                original.Validate();

                var result = Accepted(original, original.IsArrived ? "arrived" : "tick_advanced",
                    advanced, context, input);
                result.ProjectedWorldDay = projectedDay;
                result.ProjectedMinuteOfDay = projectedMinute;
                result.ProjectedPhase = original.CurrentPhase;
                return result;
            }
            catch (OverflowException)
            {
                return Rejected(original, "counter_overflow");
            }
            catch (InvalidOperationException)
            {
                return Rejected(original, "invalid_time_state");
            }
        }

        /// <summary>
        /// 仅用于单元/工具调用的便捷入口：上下文仍从已保存的逻辑时钟构造，
        /// 不读取系统时钟。生产编排应优先传入完整 D21TickContext。
        /// </summary>
        public static D21VoyageTimeResult Advance(D21VoyageTimeState current, D21VoyageTimeInput input)
        {
            if (current == null) return Rejected(null, "missing_state");
            CalculateClock(current.StartWorldDay, current.StartMinuteOfDay,
                current.ElapsedTicks, FixedTickMinutes, out int worldDay, out int minute);
            var context = new D21TickContext
            {
                WorldDay = worldDay,
                MinuteOfDay = minute,
                TickIndex = current.ElapsedTicks,
                SaveRevision = current.LastSaveRevision,
                VoyageSeed = 0,
                VoyageId = current.VoyageId ?? string.Empty,
                ShipId = current.ShipId ?? string.Empty,
            };
            return Advance(current, input, context);
        }

        public static D21VoyageArrivalEstimate EstimateArrival(
            D21VoyageTimeState current,
            D21VoyageTimeInput input,
            D21TickContext context)
        {
            var estimate = new D21VoyageArrivalEstimate { Code = "unavailable" };
            if (current == null || input == null || context == null) return estimate;
            try
            {
                current.Validate();
                input.Validate();
                context.Validate();
                long routeDistance = current.RouteDistanceMilliNm == 0
                    ? input.RouteDistanceMilliNm
                    : current.RouteDistanceMilliNm;
                long remaining = Math.Max(0L, routeDistance - current.DistanceTravelledMilliNm);
                if (remaining == 0 || current.IsArrived)
                {
                    CalculateClock(current.StartWorldDay, current.StartMinuteOfDay,
                        current.ElapsedTicks, context.TickMinutes, out int day, out int minute);
                    return new D21VoyageArrivalEstimate
                    {
                        Reachable = true,
                        RequiredTicks = 0,
                        EstimatedArrivalTick = current.ElapsedTicks,
                        EstimatedWorldDay = day,
                        EstimatedMinuteOfDay = minute,
                        EstimatedPhase = GetPhase(minute),
                        Code = "already_arrived",
                    };
                }

                long speed = input.EffectiveSpeedMilliKnots;
                if (speed <= 0) return estimate;
                long neededNumerator = checked(remaining * 60L - current.DistanceRemainderNumerator);
                if (neededNumerator < 0) neededNumerator = 0;
                long ticks = (neededNumerator + speed - 1L) / speed;
                if (ticks > int.MaxValue - current.ElapsedTicks)
                    return estimate;
                int arrivalTick = checked(current.ElapsedTicks + (int)ticks);
                CalculateClock(current.StartWorldDay, current.StartMinuteOfDay,
                    arrivalTick, context.TickMinutes, out int arrivalDay, out int arrivalMinute);
                return new D21VoyageArrivalEstimate
                {
                    Reachable = true,
                    RequiredTicks = (int)ticks,
                    EstimatedArrivalTick = arrivalTick,
                    EstimatedWorldDay = arrivalDay,
                    EstimatedMinuteOfDay = arrivalMinute,
                    EstimatedPhase = GetPhase(arrivalMinute),
                    Code = "estimated",
                };
            }
            catch (InvalidOperationException)
            {
                return estimate;
            }
            catch (OverflowException)
            {
                return estimate;
            }
        }

        public static long CalculateDistanceForTick(
            long speedMilliKnots,
            int tickMinutes,
            long remainderNumerator,
            out long nextRemainderNumerator)
        {
            if (speedMilliKnots < 0 || tickMinutes != FixedTickMinutes ||
                remainderNumerator < 0 || remainderNumerator >= 60)
                throw new ArgumentOutOfRangeException("D21 fixed-point tick input is invalid.");
            long numerator = checked(speedMilliKnots * (long)tickMinutes + remainderNumerator);
            nextRemainderNumerator = numerator % 60L;
            return numerator / 60L;
        }

        private static D21VoyageTimeResult Accepted(
            D21VoyageTimeState state,
            string code,
            long advanced,
            D21TickContext context,
            D21VoyageTimeInput input)
        {
            CalculateClock(state.StartWorldDay, state.StartMinuteOfDay,
                state.ElapsedTicks, context.TickMinutes, out int day, out int minute);
            long remaining = Math.Max(0L, state.RouteDistanceMilliNm - state.DistanceTravelledMilliNm);
            return new D21VoyageTimeResult
            {
                Accepted = true,
                IsArrived = state.IsArrived,
                Code = code,
                DistanceAdvancedMilliNm = advanced,
                RemainingDistanceMilliNm = remaining,
                ProjectedWorldDay = day,
                ProjectedMinuteOfDay = minute,
                ProjectedPhase = GetPhase(minute),
                ArrivalEstimate = EstimateArrival(state, input, new D21TickContext
                {
                    WorldDay = day,
                    MinuteOfDay = minute,
                    TickIndex = state.ElapsedTicks,
                    SaveRevision = context.SaveRevision,
                    VoyageSeed = context.VoyageSeed,
                    VoyageId = context.VoyageId,
                    ShipId = context.ShipId,
                }),
                State = state.Clone(),
            };
        }

        private static D21VoyageTimeResult Rejected(D21VoyageTimeState current, string code)
        {
            return new D21VoyageTimeResult
            {
                Accepted = false,
                IsArrived = current != null && current.IsArrived,
                Code = code ?? "rejected",
                State = current == null ? null : current.Clone(),
            };
        }

        private static void CalculateClock(
            int startWorldDay,
            int startMinuteOfDay,
            int elapsedTicks,
            int tickMinutes,
            out int worldDay,
            out int minuteOfDay)
        {
            long absolute = checked((long)startWorldDay * D21TickContext.MinutesPerDay + startMinuteOfDay +
                (long)elapsedTicks * tickMinutes);
            worldDay = checked((int)(absolute / D21TickContext.MinutesPerDay));
            minuteOfDay = checked((int)(absolute % D21TickContext.MinutesPerDay));
        }

        private static int NormalizeMinute(int minuteOfDay)
        {
            int minute = minuteOfDay % D21TickContext.MinutesPerDay;
            return minute < 0 ? minute + D21TickContext.MinutesPerDay : minute;
        }
    }
}
