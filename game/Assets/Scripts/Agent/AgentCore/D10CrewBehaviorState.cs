using System;
using System.Collections.Generic;

namespace Whalebone.Agent
{
    /// <summary>D10 轮班表的 canonical 值；OffDuty 表示当前没有值班窗口。</summary>
    public enum CrewShift
    {
        OffDuty,
        DayWatch,
        NightWatch
    }

    /// <summary>船员行为状态；状态转移由 D10CrewBehaviorSystem 裁决。</summary>
    public enum CrewBehaviorStatus
    {
        Unassigned,
        Resting,
        Working,
        Slacking,
        Refusing,
        Injured,
        LeaveRequested,
        OnLeave,
        TransferRequested,
        DepartureRequested,
        Departed
    }

    /// <summary>叛变前兆等级，不等于叛变结果。</summary>
    public enum CrewMutinyForewarning
    {
        None,
        Tension,
        Warning,
        Critical
    }

    /// <summary>
    /// D10 船员行为存档载体。它只保存状态，不包含任何 Unity、网络或事件日志引用。
    /// </summary>
    public sealed class D10CrewBehaviorState
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public CrewDuty CurrentDuty = CrewDuty.None;
        public CrewDuty RequestedDuty = CrewDuty.None;
        public CrewShift AssignedShift = CrewShift.DayWatch;
        public CrewBehaviorStatus Status = CrewBehaviorStatus.Unassigned;
        public CrewMutinyForewarning Forewarning = CrewMutinyForewarning.None;
        public int LeaveDaysRemaining;
        public int LeaveHoursElapsed;
        public int RefusalCount;
        public int SlackingCount;
        public int TransferCount;
        public int DepartureRequestCount;
        public int TransitionSequence;
        public int LastWorldDay;
        public float CurrentEfficiencyPercent;
        public string LastEventId = string.Empty;
        public string LastCommandKey = string.Empty;

        public D10CrewBehaviorState Clone()
        {
            var result = new D10CrewBehaviorState
            {
                SchemaVersion = SchemaVersion,
                CurrentDuty = CurrentDuty,
                RequestedDuty = RequestedDuty,
                AssignedShift = AssignedShift,
                Status = Status,
                Forewarning = Forewarning,
                LeaveDaysRemaining = LeaveDaysRemaining,
                LeaveHoursElapsed = LeaveHoursElapsed,
                RefusalCount = RefusalCount,
                SlackingCount = SlackingCount,
                TransferCount = TransferCount,
                DepartureRequestCount = DepartureRequestCount,
                TransitionSequence = TransitionSequence,
                LastWorldDay = LastWorldDay,
                CurrentEfficiencyPercent = CurrentEfficiencyPercent,
                LastEventId = LastEventId,
                LastCommandKey = LastCommandKey,
            };
            result.Normalize();
            return result;
        }

        public void Normalize()
        {
            if (!Enum.IsDefined(typeof(CrewDuty), CurrentDuty)) CurrentDuty = CrewDuty.None;
            if (!Enum.IsDefined(typeof(CrewDuty), RequestedDuty)) RequestedDuty = CrewDuty.None;
            if (!Enum.IsDefined(typeof(CrewShift), AssignedShift)) AssignedShift = CrewShift.OffDuty;
            if (!Enum.IsDefined(typeof(CrewBehaviorStatus), Status)) Status = CrewBehaviorStatus.Unassigned;
            if (!Enum.IsDefined(typeof(CrewMutinyForewarning), Forewarning)) Forewarning = CrewMutinyForewarning.None;
            LeaveDaysRemaining = Math.Max(0, LeaveDaysRemaining);
            LeaveHoursElapsed = Math.Max(0, Math.Min(23, LeaveHoursElapsed));
            RefusalCount = Math.Max(0, RefusalCount);
            SlackingCount = Math.Max(0, SlackingCount);
            TransferCount = Math.Max(0, TransferCount);
            DepartureRequestCount = Math.Max(0, DepartureRequestCount);
            TransitionSequence = Math.Max(0, TransitionSequence);
            LastWorldDay = Math.Max(0, LastWorldDay);
            CurrentEfficiencyPercent = Math.Max(0f, Math.Min(100f, CurrentEfficiencyPercent));
            LastEventId = LastEventId ?? string.Empty;
            LastCommandKey = LastCommandKey ?? string.Empty;
        }

        public Dictionary<string, object> ToSnapshot()
        {
            Normalize();
            return new Dictionary<string, object>
            {
                ["schema_version"] = SchemaVersion,
                ["current_duty"] = CurrentDuty.ToString(),
                ["requested_duty"] = RequestedDuty.ToString(),
                ["assigned_shift"] = AssignedShift.ToString(),
                ["status"] = Status.ToString(),
                ["forewarning"] = Forewarning.ToString(),
                ["leave_days_remaining"] = LeaveDaysRemaining,
                ["leave_hours_elapsed"] = LeaveHoursElapsed,
                ["refusal_count"] = RefusalCount,
                ["slacking_count"] = SlackingCount,
                ["transfer_count"] = TransferCount,
                ["departure_request_count"] = DepartureRequestCount,
                ["transition_sequence"] = TransitionSequence,
                ["last_world_day"] = LastWorldDay,
                ["current_efficiency_percent"] = CurrentEfficiencyPercent,
                ["last_event_id"] = LastEventId,
                ["last_command_key"] = LastCommandKey,
            };
        }

        public void Restore(Dictionary<string, object> snapshot)
        {
            if (snapshot == null) return;
            SchemaVersion = ReadInt(snapshot, "schema_version", CurrentSchemaVersion);
            CurrentDuty = ReadEnum(snapshot, "current_duty", CrewDuty.Deckhand);
            RequestedDuty = ReadEnum(snapshot, "requested_duty", CrewDuty.None);
            AssignedShift = ReadEnum(snapshot, "assigned_shift", CrewShift.DayWatch);
            Status = ReadEnum(snapshot, "status", CrewBehaviorStatus.Unassigned);
            Forewarning = ReadEnum(snapshot, "forewarning", CrewMutinyForewarning.None);
            LeaveDaysRemaining = ReadInt(snapshot, "leave_days_remaining", 0);
            LeaveHoursElapsed = ReadInt(snapshot, "leave_hours_elapsed", 0);
            RefusalCount = ReadInt(snapshot, "refusal_count", 0);
            SlackingCount = ReadInt(snapshot, "slacking_count", 0);
            TransferCount = ReadInt(snapshot, "transfer_count", 0);
            DepartureRequestCount = ReadInt(snapshot, "departure_request_count", 0);
            TransitionSequence = ReadInt(snapshot, "transition_sequence", 0);
            LastWorldDay = ReadInt(snapshot, "last_world_day", 0);
            CurrentEfficiencyPercent = ReadFloat(snapshot, "current_efficiency_percent", 0f);
            LastEventId = ReadString(snapshot, "last_event_id", string.Empty);
            LastCommandKey = ReadString(snapshot, "last_command_key", string.Empty);
            Normalize();
        }

        private static string ReadString(Dictionary<string, object> values, string key, string fallback)
        {
            object value;
            return values != null && values.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value) : fallback;
        }

        private static int ReadInt(Dictionary<string, object> values, string key, int fallback)
        {
            object value;
            if (values == null || !values.TryGetValue(key, out value) || value == null) return fallback;
            try { return Convert.ToInt32(value); }
            catch { return fallback; }
        }

        private static float ReadFloat(Dictionary<string, object> values, string key, float fallback)
        {
            object value;
            if (values == null || !values.TryGetValue(key, out value) || value == null) return fallback;
            try { return Convert.ToSingle(value); }
            catch { return fallback; }
        }

        private static T ReadEnum<T>(Dictionary<string, object> values, string key, T fallback)
            where T : struct
        {
            string value = ReadString(values, key, string.Empty);
            T parsed;
            return Enum.TryParse(value, true, out parsed) && Enum.IsDefined(typeof(T), parsed)
                ? parsed : fallback;
        }
    }

    /// <summary>
    /// D10 行为参数的单一纯 C# 入口。它只影响岗位行为状态和效率，不改经济/战斗公式。
    /// </summary>
    public static class D10CrewBehaviorConfiguration
    {
        public static readonly int StateSchemaVersion = D10CrewBehaviorState.CurrentSchemaVersion;
        public static readonly float WorkFatiguePerHour = 3f;
        public static readonly float RestFatigueRecoveryPerHour = 5f;
        public static readonly float LeaveFatigueRecoveryPerHour = 6f;
        public static readonly float RefusalFatigueThreshold = 90f;
        public static readonly float SlackingFatigueThreshold = 72f;
        public static readonly float InjuredHealthThreshold = 15f;
        public static readonly float RefusalHealthThreshold = 25f;
        public static readonly float SlackingHealthThreshold = 55f;
        public static readonly float TransferHealthMinimum = 35f;
        public static readonly float TransferFatigueMaximum = 85f;
        public static readonly int MaxLeaveDays = 7;
        public static readonly float DutyUnqualifiedEfficiencyPercent = 55f;
        public static readonly float NightWatchEfficiencyPercent = 90f;
        public static readonly float SlackingEfficiencyPercent = 60f;
        public static readonly float CriticalForewarningEfficiencyPercent = 50f;
        public static readonly float MutinyLoyaltyRiskStart = 40f;
        public static readonly float MutinyLoyaltyRiskPerPoint = 0.012f;
        public static readonly float MutinyMoraleRiskStart = 40f;
        public static readonly float MutinyMoraleRiskPerPoint = 0.006f;
        public static readonly float MutinyFatigueRiskStart = 70f;
        public static readonly float MutinyFatigueRiskPerPoint = 0.004f;
        public static readonly float MutinyAffinityRiskIncrement = 0.10f;
        public static readonly float RefusalRiskPerCount = 0.04f;
        public static readonly float SlackingRiskPerCount = 0.02f;

        /// <summary>启动/测试入口使用的行为配置合法性检查；不修改任何配置。</summary>
        public static List<string> Validate()
        {
            var errors = new List<string>();
            if (StateSchemaVersion != D10CrewBehaviorState.CurrentSchemaVersion)
                errors.Add("state_schema_version_mismatch");
            if (WorkFatiguePerHour <= 0f || RestFatigueRecoveryPerHour <= 0f
                || LeaveFatigueRecoveryPerHour <= 0f)
                errors.Add("fatigue_rate_must_be_positive");
            if (!(0f < SlackingFatigueThreshold && SlackingFatigueThreshold < RefusalFatigueThreshold
                && RefusalFatigueThreshold <= 100f)) errors.Add("fatigue_threshold_order_invalid");
            if (!(0f <= InjuredHealthThreshold && InjuredHealthThreshold < RefusalHealthThreshold
                && RefusalHealthThreshold < SlackingHealthThreshold && SlackingHealthThreshold <= 100f))
                errors.Add("health_threshold_order_invalid");
            if (TransferHealthMinimum < 0f || TransferHealthMinimum > 100f
                || TransferFatigueMaximum < 0f || TransferFatigueMaximum > 100f)
                errors.Add("transfer_bound_invalid");
            if (MaxLeaveDays <= 0) errors.Add("max_leave_days_invalid");
            if (!PercentInRange(DutyUnqualifiedEfficiencyPercent)
                || !PercentInRange(NightWatchEfficiencyPercent)
                || !PercentInRange(SlackingEfficiencyPercent)
                || !PercentInRange(CriticalForewarningEfficiencyPercent))
                errors.Add("efficiency_percent_invalid");
            if (RefusalRiskPerCount < 0f || SlackingRiskPerCount < 0f
                || RefusalRiskPerCount > 1f || SlackingRiskPerCount > 1f)
                errors.Add("forewarning_risk_increment_invalid");
            if (MutinyLoyaltyRiskStart < 0f || MutinyLoyaltyRiskStart > 100f
                || MutinyMoraleRiskStart < 0f || MutinyMoraleRiskStart > 100f
                || MutinyFatigueRiskStart < 0f || MutinyFatigueRiskStart > 100f
                || MutinyLoyaltyRiskPerPoint < 0f || MutinyMoraleRiskPerPoint < 0f
                || MutinyFatigueRiskPerPoint < 0f || MutinyAffinityRiskIncrement < 0f
                || MutinyAffinityRiskIncrement > 1f)
                errors.Add("mutiny_risk_parameter_invalid");
            foreach (CrewDuty duty in Enum.GetValues(typeof(CrewDuty)))
                if (!PercentInRange(DutyEfficiencyPercent(duty))) errors.Add("duty_efficiency_invalid");
            return errors;
        }

        private static bool PercentInRange(float value)
        {
            return value >= 0f && value <= 100f;
        }

        public static float DutyEfficiencyPercent(CrewDuty duty)
        {
            switch (duty)
            {
                case CrewDuty.Helm: return 100f;
                case CrewDuty.Navigator: return 100f;
                case CrewDuty.Lookout: return 95f;
                case CrewDuty.Gunner: return 100f;
                case CrewDuty.Repair: return 90f;
                case CrewDuty.Cook: return 95f;
                case CrewDuty.Medic: return 100f;
                case CrewDuty.Deckhand: return 85f;
                default: return 0f;
            }
        }

        public static float FatigueFactor(float fatigue)
        {
            if (fatigue < 40f) return 100f;
            if (fatigue < 72f) return 85f;
            if (fatigue < 90f) return 65f;
            return 40f;
        }

        public static float HealthFactor(float health)
        {
            if (health >= 70f) return 100f;
            if (health >= 55f) return 90f;
            if (health >= 25f) return 65f;
            return 30f;
        }

        public static float ForewarningRisk(CrewStats stats, D10CrewBehaviorState state)
        {
            if (stats == null || state == null) return 1f;
            float risk = stats.GetMutinyRisk();
            risk += state.RefusalCount * RefusalRiskPerCount;
            risk += state.SlackingCount * SlackingRiskPerCount;
            return Math.Max(0f, Math.Min(1f, risk));
        }

        public static CrewMutinyForewarning ForewarningForRisk(float risk)
        {
            if (risk >= 0.50f) return CrewMutinyForewarning.Critical;
            if (risk >= 0.30f) return CrewMutinyForewarning.Warning;
            if (risk >= 0.18f) return CrewMutinyForewarning.Tension;
            return CrewMutinyForewarning.None;
        }
    }
}
