// ============================================================
// D10CrewBehaviorSystem —— 船员岗位、轮班与行为状态唯一裁决者
//
// AgentCore 只保存纯 C# 状态载体和规则函数；本类负责修改 CrewAgent 的
// BehaviorState/CrewStats，并把状态转移追加到 D8 EventLog。没有 LLM、网络或 Unity。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Agent;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D10
{
    public enum D10CrewBehaviorEventType
    {
        StateInitialized,
        ShiftAssigned,
        DutyWorked,
        Rested,
        Slacking,
        Refusal,
        Injured,
        HealthUpdated,
        AlcoholConsumed,
        DutyTransferRequested,
        DutyTransferred,
        LeaveRequested,
        LeaveApproved,
        LeaveEnded,
        DepartureRequested,
        Departed,
        MutinyForewarningChanged,
    }

    [Serializable]
    public sealed class D10CrewBehaviorEvent
    {
        public string EventId;
        public string CrewId;
        public int WorldDay;
        public long SaveRevision;
        public D10CrewBehaviorEventType Type;
        public CrewBehaviorStatus PreviousStatus;
        public CrewBehaviorStatus CurrentStatus;
        public CrewDuty Duty;
        public CrewShift Shift;
        public CrewMutinyForewarning Forewarning;
        public string Reason;
        public float EfficiencyPercent;
        public float FatigueBefore;
        public float FatigueAfter;
        public float HealthBefore;
        public float HealthAfter;
        public string AlcoholProfile = string.Empty;
        public float ConflictRiskPercent;
        public List<string> CauseEventIds = new List<string>();

        public D10CrewBehaviorEvent Clone()
        {
            var result = (D10CrewBehaviorEvent)MemberwiseClone();
            result.CauseEventIds = CauseEventIds == null
                ? new List<string>() : new List<string>(CauseEventIds);
            return result;
        }
    }

    [Serializable]
    public sealed class D10CrewBehaviorResult
    {
        public bool Accepted;
        public bool Duplicate;
        public string Code;
        public string Message;
        public D10CrewBehaviorState State;
        public D10CrewBehaviorEvent Event;
        public FactEvent Fact;

        public static D10CrewBehaviorResult Reject(string code, string message, CrewAgent crew)
        {
            return new D10CrewBehaviorResult
            {
                Accepted = false,
                Duplicate = false,
                Code = code,
                Message = message,
                State = crew == null || crew.BehaviorState == null
                    ? null : crew.BehaviorState.Clone(),
            };
        }
    }

    /// <summary>
    /// 确定性船员行为状态机。所有公开方法都是命令入口，Agent/UI 只能提交请求，
    /// 最终岗位、疲劳、健康、状态和事件均在这里裁决。
    /// </summary>
    public sealed class D10CrewBehaviorSystem
    {
        public const string OwnerSystem = "CrewSystem";
        public const string DefaultLocationRef = "ship:ship_moonwake";

        private readonly EventLog _eventLog;

        public D10CrewBehaviorSystem(EventLog eventLog = null)
        {
            _eventLog = eventLog;
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D10CrewBehaviorResult EnsureState(CrewAgent crew, int worldDay = 0,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (crew == null || crew.Stats == null) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.Unassigned)
                return AcceptedNoEvent(crew, "already_initialized", "船员行为状态已经初始化。");

            if (state.CurrentDuty == CrewDuty.None)
                state.CurrentDuty = PrimaryDutyOrDeckhand(crew);
            if (state.AssignedShift == CrewShift.OffDuty)
                state.AssignedShift = CrewShift.DayWatch;
            CrewBehaviorStatus previousStatus = state.Status;
            state.Status = CrewBehaviorStatus.Resting;
            state.LastWorldDay = Math.Max(0, worldDay);
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.StateInitialized,
                "state_initialized", "船员行为状态已初始化。", worldDay, saveRevision,
                causeEventIds, "initialize|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult AssignShift(CrewAgent crew, CrewShift shift,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            if (!Enum.IsDefined(typeof(CrewShift), shift)) return Reject(crew,
                "invalid_shift", "轮班值无效。");
            EnsureStateObject(crew);
            if (crew.BehaviorState.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不能重新排班。");

            CrewBehaviorStatus previousStatus = crew.BehaviorState.Status;
            crew.BehaviorState.AssignedShift = shift;
            if (shift == CrewShift.OffDuty)
                crew.BehaviorState.Status = CrewBehaviorStatus.Resting;
            else if (crew.BehaviorState.Status == CrewBehaviorStatus.Unassigned)
                crew.BehaviorState.Status = CrewBehaviorStatus.Resting;
            crew.BehaviorState.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.ShiftAssigned,
                "shift_assigned", "轮班已由船员系统安排。", worldDay, saveRevision,
                causeEventIds, ActionKey("shift", crew.BehaviorState.AssignedShift, worldDay, saveRevision),
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult RequestDutyTransfer(CrewAgent crew, CrewDuty requestedDuty,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            if (requestedDuty == CrewDuty.None || !Enum.IsDefined(typeof(CrewDuty), requestedDuty))
                return Reject(crew, "invalid_duty", "调岗目标无效。");
            EnsureStateObject(crew);
            if (crew.BehaviorState.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不能申请调岗。");
            if (!IsQualified(crew, requestedDuty))
                return Reject(crew, "duty_not_qualified", "该船员没有该岗位的资格。");

            CrewBehaviorStatus previousStatus = crew.BehaviorState.Status;
            crew.BehaviorState.RequestedDuty = requestedDuty;
            crew.BehaviorState.Status = CrewBehaviorStatus.TransferRequested;
            return Commit(crew, D10CrewBehaviorEventType.DutyTransferRequested,
                "transfer_requested", "船员调岗请求已记录，等待 CrewSystem 批准。", worldDay,
                saveRevision, causeEventIds,
                "transfer_request|" + requestedDuty + "|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult ApproveDutyTransfer(CrewAgent crew,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.TransferRequested)
                return Reject(crew, "transfer_not_requested", "当前没有待批准的调岗请求。");
            if (!IsQualified(crew, state.RequestedDuty))
                return Reject(crew, "duty_not_qualified", "调岗目标不在船员资格内。");
            if (crew.Stats.Health < D10CrewBehaviorConfiguration.TransferHealthMinimum)
                return Reject(crew, "transfer_blocked_health", "健康状态不足，暂不能调岗。");
            if (crew.Stats.Fatigue > D10CrewBehaviorConfiguration.TransferFatigueMaximum)
                return Reject(crew, "transfer_blocked_fatigue", "疲劳过高，暂不能调岗。");

            CrewBehaviorStatus previousStatus = state.Status;
            CrewDuty previous = state.CurrentDuty;
            state.CurrentDuty = state.RequestedDuty;
            state.RequestedDuty = CrewDuty.None;
            state.TransferCount++;
            state.Status = CrewBehaviorStatus.Resting;
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.DutyTransferred,
                "transfer_approved", "调岗已由 CrewSystem 批准。", worldDay, saveRevision,
                causeEventIds, "transfer_approve|" + previous + "|" + state.CurrentDuty +
                "|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult RequestLeave(CrewAgent crew, string reason,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不能申请请假。");
            if (state.Status == CrewBehaviorStatus.OnLeave || state.Status == CrewBehaviorStatus.LeaveRequested)
                return Reject(crew, "leave_already_requested", "请假请求已经存在。");

            CrewBehaviorStatus previousStatus = state.Status;
            state.Status = CrewBehaviorStatus.LeaveRequested;
            return Commit(crew, D10CrewBehaviorEventType.LeaveRequested,
                "leave_requested", string.IsNullOrWhiteSpace(reason)
                    ? "船员已提出请假请求。" : "船员已提出请假请求：" + reason,
                worldDay, saveRevision, causeEventIds,
                "leave_request|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult ApproveLeave(CrewAgent crew, int leaveDays,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.LeaveRequested)
                return Reject(crew, "leave_not_requested", "当前没有待批准的请假请求。");
            int days = Math.Max(1, Math.Min(D10CrewBehaviorConfiguration.MaxLeaveDays, leaveDays));
            CrewBehaviorStatus previousStatus = state.Status;
            state.LeaveDaysRemaining = days;
            state.LeaveHoursElapsed = 0;
            state.Status = CrewBehaviorStatus.OnLeave;
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.LeaveApproved,
                "leave_approved", "请假已批准，船员暂不承担岗位工作。", worldDay,
                saveRevision, causeEventIds,
                "leave_approve|" + days + "|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult ReturnFromLeave(CrewAgent crew, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.OnLeave)
                return Reject(crew, "not_on_leave", "船员当前不在请假状态。");

            CrewBehaviorStatus previousStatus = state.Status;
            state.LeaveDaysRemaining = 0;
            state.LeaveHoursElapsed = 0;
            state.Status = CrewBehaviorStatus.Resting;
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.LeaveEnded,
                "leave_ended", "船员已结束请假并返回待命。", worldDay, saveRevision,
                causeEventIds, "leave_end|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult RequestDeparture(CrewAgent crew, string reason,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "船员已经离队。");
            if (state.Status == CrewBehaviorStatus.DepartureRequested)
                return Reject(crew, "departure_already_requested", "离队请求已经存在。");

            CrewBehaviorStatus previousStatus = state.Status;
            state.Status = CrewBehaviorStatus.DepartureRequested;
            state.DepartureRequestCount++;
            return Commit(crew, D10CrewBehaviorEventType.DepartureRequested,
                "departure_requested", string.IsNullOrWhiteSpace(reason)
                    ? "船员已提出离队请求。" : "船员已提出离队请求：" + reason,
                worldDay, saveRevision, causeEventIds,
                "departure_request|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        public D10CrewBehaviorResult ApproveDeparture(CrewAgent crew, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.DepartureRequested)
                return Reject(crew, "departure_not_requested", "当前没有待批准的离队请求。");

            CrewBehaviorStatus previousStatus = state.Status;
            state.Status = CrewBehaviorStatus.Departed;
            state.CurrentDuty = CrewDuty.None;
            state.RequestedDuty = CrewDuty.None;
            state.AssignedShift = CrewShift.OffDuty;
            state.LeaveDaysRemaining = 0;
            state.LeaveHoursElapsed = 0;
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew, D10CrewBehaviorEventType.Departed,
                "departure_approved", "离队已由 CrewSystem 裁决；船员不再参与船上岗位。",
                worldDay, saveRevision, causeEventIds,
                "departure_approve|" + worldDay + "|" + saveRevision,
                previousStatusOverride: previousStatus);
        }

        /// <summary>按离散小时推进一次轮班；同一命令键重复提交不会重复结算。</summary>
        public D10CrewBehaviorResult Tick(CrewAgent crew, CrewShift currentShift,
            int hours, int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            if (!Enum.IsDefined(typeof(CrewShift), currentShift)) return Reject(crew,
                "invalid_shift", "当前轮班值无效。");
            if (hours <= 0 || hours > 24) return Reject(crew,
                "invalid_hours", "每次行为推进必须是 1 到 24 小时。");
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不能继续推进行为。");

            string commandKey = "tick|" + currentShift + "|" + hours + "|" + worldDay + "|" + saveRevision;
            if (state.LastCommandKey == commandKey)
                return Duplicate(crew, "duplicate_tick", "相同轮班推进已结算，未重复修改状态。");

            float fatigueBefore = crew.Stats.Fatigue;
            float healthBefore = crew.Stats.Health;
            CrewBehaviorStatus previousStatus = state.Status;
            bool activeShift = state.AssignedShift != CrewShift.OffDuty
                && currentShift == state.AssignedShift
                && state.CurrentDuty != CrewDuty.None;
            D10CrewBehaviorEventType eventType;

            if (state.Status == CrewBehaviorStatus.OnLeave)
            {
                crew.Stats.Fatigue = ClampFatigue(crew.Stats.Fatigue
                    - D10CrewBehaviorConfiguration.LeaveFatigueRecoveryPerHour * hours);
                int totalLeaveHours = state.LeaveHoursElapsed + hours;
                int elapsedDays = totalLeaveHours / 24;
                state.LeaveHoursElapsed = totalLeaveHours % 24;
                state.LeaveDaysRemaining = Math.Max(0, state.LeaveDaysRemaining - elapsedDays);
                if (state.LeaveDaysRemaining == 0)
                {
                    state.LeaveHoursElapsed = 0;
                    state.Status = CrewBehaviorStatus.Resting;
                    eventType = D10CrewBehaviorEventType.LeaveEnded;
                }
                else eventType = D10CrewBehaviorEventType.Rested;
            }
            else if (!activeShift || state.Status == CrewBehaviorStatus.Refusing
                || state.Status == CrewBehaviorStatus.Injured)
            {
                crew.Stats.Fatigue = ClampFatigue(crew.Stats.Fatigue
                    - D10CrewBehaviorConfiguration.RestFatigueRecoveryPerHour * hours);
                state.Status = crew.Stats.Health < D10CrewBehaviorConfiguration.InjuredHealthThreshold
                    ? CrewBehaviorStatus.Injured : CrewBehaviorStatus.Resting;
                eventType = state.Status == CrewBehaviorStatus.Injured
                    ? D10CrewBehaviorEventType.Injured : D10CrewBehaviorEventType.Rested;
            }
            else
            {
                crew.Stats.Fatigue = ClampFatigue(crew.Stats.Fatigue
                    + D10CrewBehaviorConfiguration.WorkFatiguePerHour * hours);
                if (crew.Stats.Health < D10CrewBehaviorConfiguration.InjuredHealthThreshold)
                {
                    state.Status = CrewBehaviorStatus.Injured;
                    eventType = D10CrewBehaviorEventType.Injured;
                }
                else if (crew.Stats.Health < D10CrewBehaviorConfiguration.RefusalHealthThreshold
                    || crew.Stats.Fatigue >= D10CrewBehaviorConfiguration.RefusalFatigueThreshold)
                {
                    state.Status = CrewBehaviorStatus.Refusing;
                    state.RefusalCount++;
                    eventType = D10CrewBehaviorEventType.Refusal;
                }
                else if (crew.Stats.Health < D10CrewBehaviorConfiguration.SlackingHealthThreshold
                    || crew.Stats.Fatigue >= D10CrewBehaviorConfiguration.SlackingFatigueThreshold)
                {
                    state.Status = CrewBehaviorStatus.Slacking;
                    state.SlackingCount++;
                    eventType = D10CrewBehaviorEventType.Slacking;
                }
                else
                {
                    state.Status = CrewBehaviorStatus.Working;
                    eventType = D10CrewBehaviorEventType.DutyWorked;
                }
            }

            state.LastWorldDay = Math.Max(0, worldDay);
            state.CurrentEfficiencyPercent = CalculateEfficiency(crew, currentShift);
            var result = Commit(crew, eventType, EventCode(eventType),
                EventMessage(eventType, state.CurrentDuty), worldDay, saveRevision,
                causeEventIds, commandKey, fatigueBefore, crew.Stats.Fatigue,
                 healthBefore, crew.Stats.Health, previousStatusOverride: previousStatus);
            EvaluateForewarningInternal(crew, worldDay, saveRevision, causeEventIds);
            // 前兆事件是同一轮 Tick 的派生事实；保留 Tick 命令键，避免读档/重复调用再次结算疲劳。
            state.LastCommandKey = commandKey;
            return result;
        }

        /// <summary>健康数值只由 CrewSystem 入口修改，行为系统据此重新裁决状态。</summary>
        public D10CrewBehaviorResult ApplyHealthChange(CrewAgent crew, float delta,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不再接受船上健康结算。");
            if (float.IsNaN(delta) || float.IsInfinity(delta))
                return Reject(crew, "invalid_health_delta", "健康变化值无效。");

            CrewBehaviorStatus previousStatus = state.Status;
            float before = crew.Stats.Health;
            crew.Stats.Health = ClampHealth(crew.Stats.Health + delta);
            if (crew.Stats.Health < D10CrewBehaviorConfiguration.InjuredHealthThreshold)
                state.Status = CrewBehaviorStatus.Injured;
            else if (state.Status == CrewBehaviorStatus.Injured)
                state.Status = CrewBehaviorStatus.Resting;
            state.CurrentEfficiencyPercent = 0f;
            return Commit(crew,
                crew.Stats.Health < D10CrewBehaviorConfiguration.InjuredHealthThreshold
                    ? D10CrewBehaviorEventType.Injured : D10CrewBehaviorEventType.HealthUpdated,
                "health_updated", "健康状态已由 CrewSystem 裁决。", worldDay, saveRevision,
                causeEventIds, "health|" + delta.ToString(CultureInfo.InvariantCulture) +
                "|" + worldDay + "|" + saveRevision, 0f, crew.Stats.Fatigue,
                 before, crew.Stats.Health, previousStatusOverride: previousStatus);
        }

        /// <summary>
        /// 酒精后果由 CrewSystem 统一裁决。酒馆只传入已批准的效果档案，
        /// 本入口负责健康/疲劳/行为状态和可追溯事实，不直接改变关系。
        /// </summary>
        public D10CrewBehaviorResult ApplyAlcoholEffect(CrewAgent crew, string alcoholProfile,
            float healthDelta, float fatigueDelta, float conflictRisk,
            int worldDay, long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            var state = crew.BehaviorState;
            if (state.Status == CrewBehaviorStatus.Departed)
                return Reject(crew, "crew_departed", "离队船员不再接受酒精后果结算。");
            if (float.IsNaN(healthDelta) || float.IsInfinity(healthDelta)
                || float.IsNaN(fatigueDelta) || float.IsInfinity(fatigueDelta)
                || float.IsNaN(conflictRisk) || float.IsInfinity(conflictRisk)
                || conflictRisk < 0f || conflictRisk > 1f)
                return Reject(crew, "invalid_alcohol_effect", "酒精后果档案无效。");

            CrewBehaviorStatus previousStatus = state.Status;
            float healthBefore = crew.Stats.Health;
            float fatigueBefore = crew.Stats.Fatigue;
            crew.Stats.Health = ClampHealth(crew.Stats.Health + healthDelta);
            crew.Stats.Fatigue = ClampFatigue(crew.Stats.Fatigue + fatigueDelta);
            if (crew.Stats.Health < D10CrewBehaviorConfiguration.InjuredHealthThreshold)
                state.Status = CrewBehaviorStatus.Injured;
            else if (crew.Stats.Fatigue >= D10CrewBehaviorConfiguration.RefusalFatigueThreshold)
            {
                state.Status = CrewBehaviorStatus.Refusing;
                state.RefusalCount++;
            }
            else if (crew.Stats.Fatigue >= D10CrewBehaviorConfiguration.SlackingFatigueThreshold)
            {
                state.Status = CrewBehaviorStatus.Slacking;
                state.SlackingCount++;
            }
            else if (state.Status == CrewBehaviorStatus.Injured
                || state.Status == CrewBehaviorStatus.Refusing
                || state.Status == CrewBehaviorStatus.Slacking)
                state.Status = CrewBehaviorStatus.Resting;
            state.CurrentEfficiencyPercent = 0f;

            var result = Commit(crew, D10CrewBehaviorEventType.AlcoholConsumed,
                "alcohol_consumed", "饮酒后果已由 CrewSystem 结算。", worldDay,
                saveRevision, causeEventIds,
                "alcohol|" + (alcoholProfile ?? string.Empty) + "|" +
                healthDelta.ToString(CultureInfo.InvariantCulture) + "|" +
                fatigueDelta.ToString(CultureInfo.InvariantCulture) + "|" +
                worldDay + "|" + saveRevision,
                fatigueBefore, crew.Stats.Fatigue, healthBefore, crew.Stats.Health,
                previousStatusOverride: previousStatus,
                alcoholProfile: alcoholProfile,
                conflictRiskPercent: conflictRisk * 100f);
            EvaluateForewarningInternal(crew, worldDay, saveRevision, causeEventIds);
            return result;
        }

        public D10CrewBehaviorResult EvaluateMutinyForewarning(CrewAgent crew, int worldDay,
            long saveRevision = 0, IEnumerable<string> causeEventIds = null)
        {
            if (!HasCrew(crew)) return D10CrewBehaviorResult.Reject(
                "crew_missing", "船员不存在。", crew);
            EnsureStateObject(crew);
            return EvaluateForewarningInternal(crew, worldDay, saveRevision, causeEventIds);
        }

        public float CalculateEfficiency(CrewAgent crew, CrewShift currentShift)
        {
            if (!HasCrew(crew) || crew.BehaviorState == null) return 0f;
            var state = crew.BehaviorState;
            if (state.Status != CrewBehaviorStatus.Working
                && state.Status != CrewBehaviorStatus.Slacking)
                return 0f;
            if (state.AssignedShift == CrewShift.OffDuty || state.AssignedShift != currentShift)
                return 0f;

            float efficiency = D10CrewBehaviorConfiguration.DutyEfficiencyPercent(state.CurrentDuty);
            if (!IsQualified(crew, state.CurrentDuty))
                efficiency = Math.Min(efficiency, D10CrewBehaviorConfiguration.DutyUnqualifiedEfficiencyPercent);
            if (currentShift == CrewShift.NightWatch)
                efficiency *= D10CrewBehaviorConfiguration.NightWatchEfficiencyPercent / 100f;
            efficiency *= D10CrewBehaviorConfiguration.FatigueFactor(crew.Stats.Fatigue) / 100f;
            efficiency *= D10CrewBehaviorConfiguration.HealthFactor(crew.Stats.Health) / 100f;
            if (state.Status == CrewBehaviorStatus.Slacking)
                efficiency *= D10CrewBehaviorConfiguration.SlackingEfficiencyPercent / 100f;
            if (state.Forewarning == CrewMutinyForewarning.Critical)
                efficiency *= D10CrewBehaviorConfiguration.CriticalForewarningEfficiencyPercent / 100f;
            return Math.Max(0f, Math.Min(100f, efficiency));
        }

        private D10CrewBehaviorResult EvaluateForewarningInternal(CrewAgent crew, int worldDay,
            long saveRevision, IEnumerable<string> causeEventIds)
        {
            var state = crew.BehaviorState;
            CrewMutinyForewarning before = state.Forewarning;
            CrewMutinyForewarning after = D10CrewBehaviorConfiguration.ForewarningForRisk(
                D10CrewBehaviorConfiguration.ForewarningRisk(crew.Stats, state));
            if (before == after)
                return AcceptedNoEvent(crew, "forewarning_unchanged", "叛变前兆等级没有变化。");

            state.Forewarning = after;
            state.LastWorldDay = Math.Max(0, worldDay);
            return Commit(crew, D10CrewBehaviorEventType.MutinyForewarningChanged,
                "mutiny_forewarning_changed", "叛变前兆等级已由 CrewSystem 更新。", worldDay,
                saveRevision, causeEventIds,
                "forewarning|" + before + "|" + after + "|" + worldDay + "|" + saveRevision);
        }

        private D10CrewBehaviorResult Commit(CrewAgent crew, D10CrewBehaviorEventType eventType,
            string code, string message, int worldDay, long saveRevision,
            IEnumerable<string> causeEventIds, string commandKey,
            float fatigueBefore = 0f, float fatigueAfter = 0f,
            float healthBefore = 0f, float healthAfter = 0f,
            CrewBehaviorStatus? previousStatusOverride = null,
            string alcoholProfile = "", float conflictRiskPercent = 0f)
        {
            var state = crew.BehaviorState;
            if (state.LastCommandKey == commandKey)
                return Duplicate(crew, "duplicate_transition", "相同船员行为命令已经结算。");

            var causes = CopyIds(causeEventIds);
            CrewBehaviorStatus previousStatus = previousStatusOverride ?? state.Status;
            state.TransitionSequence++;
            state.LastWorldDay = Math.Max(0, worldDay);
            state.LastCommandKey = commandKey ?? string.Empty;
            state.LastEventId = BuildEventId(crew.Id, eventType, state.TransitionSequence,
                worldDay, saveRevision);
            var behaviorEvent = new D10CrewBehaviorEvent
            {
                EventId = state.LastEventId,
                CrewId = crew.Id,
                WorldDay = Math.Max(0, worldDay),
                SaveRevision = Math.Max(0, saveRevision),
                Type = eventType,
                PreviousStatus = previousStatus,
                CurrentStatus = state.Status,
                Duty = state.CurrentDuty,
                Shift = state.AssignedShift,
                Forewarning = state.Forewarning,
                Reason = message,
                EfficiencyPercent = state.CurrentEfficiencyPercent,
                FatigueBefore = fatigueBefore,
                FatigueAfter = fatigueAfter,
                HealthBefore = healthBefore,
                HealthAfter = healthAfter,
                AlcoholProfile = alcoholProfile ?? string.Empty,
                ConflictRiskPercent = conflictRiskPercent,
                CauseEventIds = causes,
            };

            FactEvent fact = null;
            if (_eventLog != null)
            {
                var values = new Dictionary<string, string>
                {
                    ["crew_id"] = crew.Id ?? string.Empty,
                    ["status_before"] = previousStatus.ToString(),
                    ["status_after"] = state.Status.ToString(),
                    ["duty"] = state.CurrentDuty.ToString(),
                    ["shift"] = state.AssignedShift.ToString(),
                    ["efficiency_percent"] = state.CurrentEfficiencyPercent.ToString(CultureInfo.InvariantCulture),
                    ["fatigue"] = crew.Stats.Fatigue.ToString(CultureInfo.InvariantCulture),
                    ["health"] = crew.Stats.Health.ToString(CultureInfo.InvariantCulture),
                    ["forewarning"] = state.Forewarning.ToString(),
                    ["alcohol_profile"] = alcoholProfile ?? string.Empty,
                    ["conflict_risk_percent"] = conflictRiskPercent.ToString(CultureInfo.InvariantCulture),
                };
                var append = _eventLog.Append(new WorldEvent(
                    state.LastEventId,
                    Math.Max(0, worldDay),
                    DefaultLocationRef,
                    OwnerSystem,
                    new EventResult
                    {
                        ResultCode = code,
                        Summary = message,
                        FactIds = new List<string> { "crew:" + crew.Id },
                        Values = values,
                    },
                    new[] { "crew:" + crew.Id },
                    new[] { "crew_behavior" },
                    causes,
                    Math.Max(0, saveRevision)));
                fact = append.Fact;
            }

            return new D10CrewBehaviorResult
            {
                Accepted = true,
                Duplicate = false,
                Code = code,
                Message = message,
                State = state.Clone(),
                Event = behaviorEvent,
                Fact = fact,
            };
        }

        private static D10CrewBehaviorResult AcceptedNoEvent(CrewAgent crew, string code, string message)
        {
            return new D10CrewBehaviorResult
            {
                Accepted = true,
                Duplicate = false,
                Code = code,
                Message = message,
                State = crew == null || crew.BehaviorState == null
                    ? null : crew.BehaviorState.Clone(),
            };
        }

        private static D10CrewBehaviorResult Reject(CrewAgent crew, string code, string message)
        {
            return D10CrewBehaviorResult.Reject(code, message, crew);
        }

        private static D10CrewBehaviorResult Duplicate(CrewAgent crew, string code, string message)
        {
            return new D10CrewBehaviorResult
            {
                Accepted = true,
                Duplicate = true,
                Code = code,
                Message = message,
                State = crew == null || crew.BehaviorState == null
                    ? null : crew.BehaviorState.Clone(),
            };
        }

        private static bool HasCrew(CrewAgent crew)
        {
            return crew != null && crew.Stats != null;
        }

        private static void EnsureStateObject(CrewAgent crew)
        {
            if (crew.BehaviorState == null) crew.BehaviorState = new D10CrewBehaviorState();
            crew.BehaviorState.Normalize();
        }

        private static CrewDuty PrimaryDutyOrDeckhand(CrewAgent crew)
        {
            if (crew.Persona != null && crew.Persona.PrimaryDuty != CrewDuty.None)
                return crew.Persona.PrimaryDuty;
            return CrewDuty.Deckhand;
        }

        private static bool IsQualified(CrewAgent crew, CrewDuty duty)
        {
            if (crew == null || crew.Persona == null || duty == CrewDuty.None) return false;
            if (crew.Persona.QualifiedDuties != null)
                foreach (var qualified in crew.Persona.QualifiedDuties)
                    if (qualified == duty) return true;
            return crew.Persona.PrimaryDuty == duty;
        }

        private static float ClampFatigue(float value)
        {
            return Math.Max(0f, Math.Min(100f, value));
        }

        private static float ClampHealth(float value)
        {
            return Math.Max(0f, Math.Min(100f, value));
        }

        private static string ActionKey(string prefix, CrewShift shift, int day, long revision)
        {
            return prefix + "|" + shift + "|" + day + "|" + revision;
        }

        private static string BuildEventId(string crewId, D10CrewBehaviorEventType type,
            int sequence, int worldDay, long saveRevision)
        {
            return "event_d10_crew_behavior_" + SafeId(crewId) + "_" + type + "_" +
                sequence.ToString(CultureInfo.InvariantCulture) + "_" +
                Math.Max(0, worldDay).ToString(CultureInfo.InvariantCulture) + "_" +
                Math.Max(0, saveRevision).ToString(CultureInfo.InvariantCulture);
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_';
            return new string(chars);
        }

        private static List<string> CopyIds(IEnumerable<string> values)
        {
            var result = new List<string>();
            if (values == null) return result;
            foreach (var value in values)
                if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value)) result.Add(value);
            return result;
        }

        private static string EventCode(D10CrewBehaviorEventType type)
        {
            switch (type)
            {
                case D10CrewBehaviorEventType.DutyWorked: return "duty_worked";
                case D10CrewBehaviorEventType.Rested: return "rested";
                case D10CrewBehaviorEventType.Slacking: return "slacking";
                case D10CrewBehaviorEventType.Refusal: return "refusal";
                case D10CrewBehaviorEventType.Injured: return "injured";
                case D10CrewBehaviorEventType.AlcoholConsumed: return "alcohol_consumed";
                case D10CrewBehaviorEventType.LeaveEnded: return "leave_ended";
                default: return type.ToString().ToLowerInvariant();
            }
        }

        private static string EventMessage(D10CrewBehaviorEventType type, CrewDuty duty)
        {
            switch (type)
            {
                case D10CrewBehaviorEventType.DutyWorked: return "船员按当前岗位完成了本轮值班。";
                case D10CrewBehaviorEventType.Rested: return "船员处于休息轮班，疲劳得到恢复。";
                case D10CrewBehaviorEventType.Slacking: return "疲劳或健康状态导致船员消极怠工。";
                case D10CrewBehaviorEventType.Refusal: return "疲劳或健康状态导致船员拒绝执行当前岗位。";
                case D10CrewBehaviorEventType.Injured: return "健康状态过低，船员暂不能执行岗位。";
                default: return "船员行为状态已更新：" + duty + "。";
            }
        }
    }
}
