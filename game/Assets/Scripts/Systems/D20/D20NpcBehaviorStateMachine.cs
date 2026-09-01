// D20NpcBehaviorStateMachine —— D20-L17 NPC 行为的纯 C# 状态机。
//
// 这里拥有的只是 D20 行为投影和恢复语义。岗位、轮班、疲劳、健康、关系、
// 交易、库存和任务仍由各自 owner 裁决；本状态机只根据已验证的日程输入
// 返回确定性的状态结果，不实现寻路、表现或任何外部副作用。

using System;

namespace Whalebone.Systems.D20
{
    /// <summary>D20 NPC 行为状态集合。</summary>
    public enum D20NpcBehaviorState
    {
        Unknown = 0,
        Idle = 1,
        Work = 2,
        Trade = 3,
        Talk = 4,
        Rest = 5,
        Alarm = 6,
        Refuse = 7,
        Injured = 8,
    }

    /// <summary>状态机接受的确定性请求种类。</summary>
    public enum D20NpcBehaviorRequestKind
    {
        Unknown = 0,
        SynchronizeSchedule = 1,
        StartWork = 2,
        RequestTrade = 3,
        RequestTalk = 4,
        RequestRest = 5,
        RaiseAlarm = 6,
        ReportInjury = 7,
        Recover = 8,
        Refuse = 9,
        Cancel = 10,
    }

    /// <summary>请求载体的稳定校验码。</summary>
    public enum D20NpcBehaviorRequestValidationCode
    {
        None = 0,
        MissingRequest = 1,
        InvalidSchemaVersion = 2,
        MissingCommandId = 3,
        MissingNpcId = 4,
        UnknownRequest = 5,
        MissingSchedule = 6,
        InvalidSchedule = 7,
        NpcIdMismatch = 8,
        InvalidReasonCode = 9,
    }

    /// <summary>行为请求被拒绝或完成后的稳定失败码。</summary>
    public enum D20NpcBehaviorFailureCode
    {
        None = 0,
        InvalidRequest = 1,
        NpcUnavailable = 2,
        NpcTerminal = 3,
        Busy = 4,
        Injured = 5,
        RestRequired = 6,
        DangerPresent = 7,
        WeatherUnsafe = 8,
        OutsideWorkPeriod = 9,
        MissingWorkDuty = 10,
        RoleCapabilityMissing = 11,
        IllegalTransition = 12,
        RecoveryBlocked = 13,
        AlreadyInState = 14,
    }

    /// <summary>状态机为失败状态提供的确定性恢复策略。</summary>
    public enum D20NpcBehaviorRecoveryPolicy
    {
        None = 0,
        RetryNextSchedule = 1,
        WaitForOwner = 2,
        WaitForDangerToClear = 3,
        RestBeforeRetry = 4,
        RetryAfterRefusalCleared = 5,
        ManualRetry = 6,
    }

    /// <summary>行为请求校验结果；校验不会修正请求。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorRequestValidationResult
    {
        /// <summary>请求是否有效。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20NpcBehaviorRequestValidationCode Code;

        /// <summary>机器可读原因。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20NpcBehaviorRequestValidationResult Success()
        {
            return new D20NpcBehaviorRequestValidationResult
            {
                Valid = true,
                Code = D20NpcBehaviorRequestValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20NpcBehaviorRequestValidationResult Failure(
            D20NpcBehaviorRequestValidationCode code,
            string reasonCode)
        {
            return new D20NpcBehaviorRequestValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D20NpcBehaviorRequestValidationResult Clone()
        {
            return new D20NpcBehaviorRequestValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>NPC 行为状态机的可序列化请求。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorRequest
    {
        /// <summary>请求契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>请求契约 owner 标识。</summary>
        public const string OwnerSystem = "D20NpcBehaviorStateMachine";

        /// <summary>请求契约版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>跨重试保持不变的逻辑命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>目标 NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>请求类型。</summary>
        public D20NpcBehaviorRequestKind Kind = D20NpcBehaviorRequestKind.Unknown;

        /// <summary>本次判断的完整日程输入副本。</summary>
        public D20NpcScheduleInput Schedule;

        /// <summary>可选的 owner 拒绝原因引用。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建行为请求；输入会复制，避免状态机持有外部可变引用。</summary>
        public static D20NpcBehaviorRequest Create(
            D20NpcBehaviorRequestKind kind,
            string commandId,
            D20NpcScheduleInput schedule,
            string reasonCode = "")
        {
            return new D20NpcBehaviorRequest
            {
                CommandId = commandId ?? string.Empty,
                NpcId = schedule == null ? string.Empty : schedule.NpcId ?? string.Empty,
                Kind = kind,
                Schedule = schedule == null ? null : schedule.Clone(),
                ReasonCode = reasonCode ?? string.Empty,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D20NpcBehaviorRequest Clone()
        {
            return new D20NpcBehaviorRequest
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                NpcId = NpcId ?? string.Empty,
                Kind = Kind,
                Schedule = Schedule == null ? null : Schedule.Clone(),
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }

        /// <summary>验证请求版本、命令键、NPC ID 和日程输入。</summary>
        public D20NpcBehaviorRequestValidationResult Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.InvalidSchemaVersion,
                    "npc_behavior_schema_version_mismatch");
            if (!D20NpcIdentityContracts.IsStableReference(CommandId, false))
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.MissingCommandId,
                    "npc_behavior_command_id_missing");
            if (!D20NpcIdentityContracts.IsStableNpcId(NpcId))
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.MissingNpcId,
                    "npc_behavior_npc_id_missing");
            if (!IsSupportedRequest(Kind))
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.UnknownRequest,
                    "npc_behavior_request_unknown");
            if (Schedule == null)
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.MissingSchedule,
                    "npc_behavior_schedule_missing");

            D20NpcScheduleInputValidationResult scheduleResult =
                D20NpcScheduleInputContracts.Validate(Schedule);
            if (!scheduleResult.Valid)
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.InvalidSchedule,
                    scheduleResult.ReasonCode);
            if (!string.Equals(NpcId, Schedule.NpcId, StringComparison.Ordinal))
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.NpcIdMismatch,
                    "npc_behavior_npc_id_mismatch");
            if (!D20NpcIdentityContracts.IsStableReference(ReasonCode, true))
                return D20NpcBehaviorRequestValidationResult.Failure(
                    D20NpcBehaviorRequestValidationCode.InvalidReasonCode,
                    "npc_behavior_reason_code_invalid");

            return D20NpcBehaviorRequestValidationResult.Success();
        }

        /// <summary>判断请求枚举是否支持。</summary>
        public static bool IsSupportedRequest(D20NpcBehaviorRequestKind kind)
        {
            return kind >= D20NpcBehaviorRequestKind.SynchronizeSchedule &&
                   kind <= D20NpcBehaviorRequestKind.Cancel;
        }
    }

    /// <summary>D20 自有 NPC 行为投影；不复制外部 owner 的岗位或健康数值。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorSnapshot
    {
        /// <summary>行为投影版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>行为投影版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>只读岗位引用，用于解释当前行为，不拥有岗位分配。</summary>
        public string RoleId = string.Empty;

        /// <summary>当前 D20 行为状态。</summary>
        public D20NpcBehaviorState State = D20NpcBehaviorState.Unknown;

        /// <summary>进入当前状态前的状态。</summary>
        public D20NpcBehaviorState PreviousState = D20NpcBehaviorState.Unknown;

        /// <summary>危险/受伤/拒绝解除后建议恢复的状态。</summary>
        public D20NpcBehaviorState ResumeState = D20NpcBehaviorState.Idle;

        /// <summary>进入当前状态的稳定原因码。</summary>
        public string StateReasonCode = string.Empty;

        /// <summary>进入当前状态的世界日。</summary>
        public int StateSinceWorldDay;

        /// <summary>进入当前状态的世界时段。</summary>
        public D20NpcTimeSlot StateSinceTimeSlot = D20NpcTimeSlot.Unknown;

        /// <summary>当前阻断状态的恢复策略。</summary>
        public D20NpcBehaviorRecoveryPolicy RecoveryPolicy =
            D20NpcBehaviorRecoveryPolicy.None;

        /// <summary>生成该投影时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>返回深拷贝。</summary>
        public D20NpcBehaviorSnapshot Clone()
        {
            return new D20NpcBehaviorSnapshot
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId ?? string.Empty,
                RoleId = RoleId ?? string.Empty,
                State = State,
                PreviousState = PreviousState,
                ResumeState = ResumeState,
                StateReasonCode = StateReasonCode ?? string.Empty,
                StateSinceWorldDay = StateSinceWorldDay,
                StateSinceTimeSlot = StateSinceTimeSlot,
                RecoveryPolicy = RecoveryPolicy,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>验证投影版本、稳定引用、状态和恢复信息。</summary>
        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException(
                    "D20 NPC behavior schema_version mismatch.");
            if (!D20NpcIdentityContracts.IsStableNpcId(NpcId))
                throw new InvalidOperationException(
                    "D20 NPC behavior npc_id is missing or unstable.");
            if (!D20NpcIdentityContracts.IsStableReference(RoleId, true))
                throw new InvalidOperationException(
                    "D20 NPC behavior role_id is unstable.");
            if (!D20NpcBehaviorStateMachine.IsSupportedState(State))
                throw new InvalidOperationException(
                    "D20 NPC behavior state is unknown.");
            if (PreviousState != D20NpcBehaviorState.Unknown &&
                !D20NpcBehaviorStateMachine.IsSupportedState(PreviousState))
                throw new InvalidOperationException(
                    "D20 NPC behavior previous_state is unknown.");
            if (!D20NpcBehaviorStateMachine.IsSupportedState(ResumeState))
                throw new InvalidOperationException(
                    "D20 NPC behavior resume_state is unknown.");
            if (!D20NpcIdentityContracts.IsStableReference(StateReasonCode, true))
                throw new InvalidOperationException(
                    "D20 NPC behavior reason_code is unstable.");
            if (StateSinceWorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException(
                    "D20 NPC behavior revisions must be non-negative.");
            if (!D20NpcScheduleInputContracts.IsSupportedTimeSlot(StateSinceTimeSlot))
                throw new InvalidOperationException(
                    "D20 NPC behavior state_since_time_slot is unknown.");
            if (!Enum.IsDefined(typeof(D20NpcBehaviorRecoveryPolicy), RecoveryPolicy))
                throw new InvalidOperationException(
                    "D20 NPC behavior recovery policy is unknown.");
        }
    }

    /// <summary>状态转换的只读结果。</summary>
    [Serializable]
    public sealed class D20NpcBehaviorTransitionResult
    {
        /// <summary>请求是否被状态机接受。</summary>
        public bool Accepted;

        /// <summary>是否完成了合法状态转换。</summary>
        public bool Succeeded;

        /// <summary>状态是否实际变化。</summary>
        public bool Changed;

        /// <summary>请求命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>转换前状态。</summary>
        public D20NpcBehaviorState FromState;

        /// <summary>转换后状态；失败时是当前状态。</summary>
        public D20NpcBehaviorState ToState;

        /// <summary>稳定失败码；成功时为 None。</summary>
        public D20NpcBehaviorFailureCode FailureCode;

        /// <summary>稳定原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>建议的恢复策略。</summary>
        public D20NpcBehaviorRecoveryPolicy RecoveryPolicy;

        /// <summary>转换发生的世界日。</summary>
        public int WorldDay;

        /// <summary>转换发生的时段。</summary>
        public D20NpcTimeSlot TimeSlot;

        /// <summary>转换对应的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>返回深拷贝。</summary>
        public D20NpcBehaviorTransitionResult Clone()
        {
            return new D20NpcBehaviorTransitionResult
            {
                Accepted = Accepted,
                Succeeded = Succeeded,
                Changed = Changed,
                CommandId = CommandId ?? string.Empty,
                NpcId = NpcId ?? string.Empty,
                FromState = FromState,
                ToState = ToState,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                RecoveryPolicy = RecoveryPolicy,
                WorldDay = WorldDay,
                TimeSlot = TimeSlot,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>
    /// D20-L17 确定性 NPC 行为状态机。所有状态变化都来自已验证的请求，
    /// 且只更新本类的行为投影；它不会调用或修改经济、关系、库存、任务、
    /// 健康、港口或 Agent 核心库 owner。
    /// </summary>
    public sealed class D20NpcBehaviorStateMachine
    {
        /// <summary>行为状态机 owner 标识。</summary>
        public const string OwnerSystem = "D20NpcBehaviorStateMachine";

        /// <summary>默认实例的内部占位 ID，不代表真实 NPC 实体。</summary>
        public const string UnassignedNpcId = "npc_unassigned";

        private readonly D20NpcBehaviorSnapshot _snapshot;

        /// <summary>创建一个尚未绑定 NPC 的 Idle 状态机。</summary>
        public D20NpcBehaviorStateMachine()
        {
            _snapshot = new D20NpcBehaviorSnapshot
            {
                NpcId = UnassignedNpcId,
                State = D20NpcBehaviorState.Idle,
                PreviousState = D20NpcBehaviorState.Unknown,
                ResumeState = D20NpcBehaviorState.Idle,
                StateReasonCode = "npc_behavior_initialized",
                StateSinceWorldDay = 0,
                StateSinceTimeSlot = D20NpcTimeSlot.Dawn,
                RecoveryPolicy = D20NpcBehaviorRecoveryPolicy.None,
            };
        }

        /// <summary>从已验证行为投影恢复状态机。</summary>
        public D20NpcBehaviorStateMachine(D20NpcBehaviorSnapshot initialSnapshot)
        {
            if (initialSnapshot == null)
                throw new ArgumentNullException("initialSnapshot");
            _snapshot = initialSnapshot.Clone();
            _snapshot.Validate();
        }

        /// <summary>当前状态的只读值。</summary>
        public D20NpcBehaviorState CurrentState
        {
            get { return _snapshot.State; }
        }

        /// <summary>状态机是否已经绑定真实 NPC ID。</summary>
        public bool IsAssigned
        {
            get { return !string.Equals(_snapshot.NpcId, UnassignedNpcId, StringComparison.Ordinal); }
        }

        /// <summary>返回行为投影副本。</summary>
        public D20NpcBehaviorSnapshot CaptureSnapshot()
        {
            return _snapshot.Clone();
        }

        /// <summary>只评估请求，不改变内部投影。</summary>
        public D20NpcBehaviorTransitionResult Evaluate(D20NpcBehaviorRequest request)
        {
            D20NpcBehaviorTransitionResult result = CreateBaseResult(request);
            if (request == null)
                return Fail(result, D20NpcBehaviorFailureCode.InvalidRequest,
                    "npc_behavior_request_missing", D20NpcBehaviorRecoveryPolicy.ManualRetry);

            D20NpcBehaviorRequestValidationResult validation = request.Validate();
            if (!validation.Valid)
                return Fail(result, D20NpcBehaviorFailureCode.InvalidRequest,
                    validation.ReasonCode, D20NpcBehaviorRecoveryPolicy.ManualRetry);

            if (IsAssigned && !string.Equals(_snapshot.NpcId, request.NpcId,
                    StringComparison.Ordinal))
                return Fail(result, D20NpcBehaviorFailureCode.InvalidRequest,
                    "npc_behavior_npc_id_mismatch", D20NpcBehaviorRecoveryPolicy.ManualRetry);

            result.NpcId = request.NpcId;
            result.WorldDay = request.Schedule.WorldDay;
            result.TimeSlot = request.Schedule.TimeSlot;
            result.SaveRevision = request.Schedule.SaveRevision;
            result.FromState = _snapshot.State;
            result.ToState = _snapshot.State;

            if (IsTerminal(request.Schedule.Availability))
                return Fail(result, D20NpcBehaviorFailureCode.NpcTerminal,
                    "npc_behavior_npc_terminal", D20NpcBehaviorRecoveryPolicy.WaitForOwner);

            D20NpcBehaviorState targetState;
            D20NpcBehaviorFailureCode failureCode;
            string reasonCode;
            D20NpcBehaviorRecoveryPolicy recoveryPolicy;
            if (!TryResolveTarget(request, out targetState, out failureCode,
                    out reasonCode, out recoveryPolicy))
                return Fail(result, failureCode, reasonCode, recoveryPolicy);

            if (!CanTransition(_snapshot.State, targetState))
                return Fail(result, D20NpcBehaviorFailureCode.IllegalTransition,
                    "npc_behavior_illegal_transition",
                    D20NpcBehaviorRecoveryPolicy.ManualRetry);

            result.ToState = targetState;
            result.Succeeded = true;
            result.Accepted = true;
            result.Changed = !IsAssigned || _snapshot.State != targetState;
            result.FailureCode = result.Changed
                ? D20NpcBehaviorFailureCode.None
                : D20NpcBehaviorFailureCode.AlreadyInState;
            result.ReasonCode = result.Changed
                ? reasonCode
                : "npc_behavior_already_in_state";
            result.RecoveryPolicy = result.Changed
                ? recoveryPolicy
                : _snapshot.RecoveryPolicy;
            return result;
        }

        /// <summary>评估并应用请求；失败时内部投影保持不变。</summary>
        public D20NpcBehaviorTransitionResult Apply(D20NpcBehaviorRequest request)
        {
            D20NpcBehaviorTransitionResult result = Evaluate(request);
            if (!result.Succeeded)
                return result;

            if (!result.Changed)
                return result;

            _snapshot.PreviousState = _snapshot.State;
            _snapshot.State = result.ToState;
            _snapshot.ResumeState = GetResumeState(_snapshot.State, result.ToState);
            _snapshot.StateReasonCode = result.ReasonCode ?? string.Empty;
            _snapshot.RecoveryPolicy = result.RecoveryPolicy;
            _snapshot.NpcId = result.NpcId ?? string.Empty;
            _snapshot.RoleId = request.Schedule.RoleId ?? string.Empty;
            _snapshot.StateSinceWorldDay = result.WorldDay;
            _snapshot.StateSinceTimeSlot = result.TimeSlot;
            _snapshot.SaveRevision = result.SaveRevision;
            return result;
        }

        /// <summary>按日程输入推进一次确定性状态判断。</summary>
        public D20NpcBehaviorTransitionResult Tick(D20NpcScheduleInput schedule)
        {
            if (schedule == null)
                return Apply(null);
            string commandId = BuildCommandId(schedule, D20NpcBehaviorRequestKind.SynchronizeSchedule);
            return Apply(D20NpcBehaviorRequest.Create(
                D20NpcBehaviorRequestKind.SynchronizeSchedule, commandId, schedule));
        }

        /// <summary>请求进入工作状态。</summary>
        public D20NpcBehaviorTransitionResult RequestWork(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.StartWork, schedule));
        }

        /// <summary>请求进入交易状态；交易事实由外部 owner 裁决。</summary>
        public D20NpcBehaviorTransitionResult RequestTrade(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.RequestTrade, schedule));
        }

        /// <summary>请求进入交谈状态；关系和对话事实由外部 owner 裁决。</summary>
        public D20NpcBehaviorTransitionResult RequestTalk(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.RequestTalk, schedule));
        }

        /// <summary>请求进入休息状态。</summary>
        public D20NpcBehaviorTransitionResult RequestRest(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.RequestRest, schedule));
        }

        /// <summary>报告危险并进入警戒状态。</summary>
        public D20NpcBehaviorTransitionResult RaiseAlarm(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.RaiseAlarm, schedule));
        }

        /// <summary>接收健康 owner 的受伤输入。</summary>
        public D20NpcBehaviorTransitionResult ReportInjury(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.ReportInjury, schedule));
        }

        /// <summary>在外部 owner 清除阻断后请求恢复到日程状态。</summary>
        public D20NpcBehaviorTransitionResult Recover(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.Recover, schedule));
        }

        /// <summary>记录拒绝状态；拒绝不等于关系或任务事实。</summary>
        public D20NpcBehaviorTransitionResult Refuse(
            D20NpcScheduleInput schedule,
            string reasonCode = "")
        {
            D20NpcBehaviorRequest request = CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.Refuse, schedule);
            if (request != null) request.ReasonCode = reasonCode ?? string.Empty;
            return Apply(request);
        }

        /// <summary>取消当前可取消的行为并回到安全日程状态。</summary>
        public D20NpcBehaviorTransitionResult Cancel(D20NpcScheduleInput schedule)
        {
            return Apply(CreateConvenienceRequest(
                D20NpcBehaviorRequestKind.Cancel, schedule));
        }

        /// <summary>判断状态枚举是否受当前版本支持。</summary>
        public static bool IsSupportedState(D20NpcBehaviorState state)
        {
            return state >= D20NpcBehaviorState.Idle &&
                   state <= D20NpcBehaviorState.Injured;
        }

        /// <summary>判断两个状态之间是否存在合法业务转移。</summary>
        public static bool CanTransition(
            D20NpcBehaviorState from,
            D20NpcBehaviorState to)
        {
            if (!IsSupportedState(to)) return false;
            if (from == D20NpcBehaviorState.Unknown)
                return to == D20NpcBehaviorState.Idle;
            if (!IsSupportedState(from)) return false;
            if (from == to) return true;

            // 受伤、危险和拒绝是可从任何正常行为进入的保护状态；恢复
            // 由新的 owner 输入重新进入日程，不需要中间的视觉动作。
            if (to == D20NpcBehaviorState.Injured ||
                to == D20NpcBehaviorState.Alarm ||
                to == D20NpcBehaviorState.Refuse)
                return true;

            if ((from == D20NpcBehaviorState.Injured ||
                 from == D20NpcBehaviorState.Alarm ||
                 from == D20NpcBehaviorState.Refuse) &&
                IsNormalState(to))
                return true;

            // 所有正常状态之间的转换只由请求/恢复结果触发，状态机不
            // 假定某一种工作会写入另一域的事实。
            return IsNormalState(from) && IsNormalState(to);
        }

        private static bool IsNormalState(D20NpcBehaviorState state)
        {
            return state == D20NpcBehaviorState.Idle ||
                   state == D20NpcBehaviorState.Work ||
                   state == D20NpcBehaviorState.Trade ||
                   state == D20NpcBehaviorState.Talk ||
                   state == D20NpcBehaviorState.Rest;
        }

        private static bool IsTerminal(D20CharacterAvailability availability)
        {
            return availability == D20CharacterAvailability.Dead ||
                   availability == D20CharacterAvailability.Departed;
        }

        private bool TryResolveTarget(
            D20NpcBehaviorRequest request,
            out D20NpcBehaviorState targetState,
            out D20NpcBehaviorFailureCode failureCode,
            out string reasonCode,
            out D20NpcBehaviorRecoveryPolicy recoveryPolicy)
        {
            targetState = _snapshot.State;
            failureCode = D20NpcBehaviorFailureCode.None;
            reasonCode = string.Empty;
            recoveryPolicy = D20NpcBehaviorRecoveryPolicy.None;
            D20NpcScheduleInput input = request.Schedule;

            if (request.Kind == D20NpcBehaviorRequestKind.SynchronizeSchedule)
                return TryResolveScheduledState(input, out targetState, out failureCode,
                    out reasonCode, out recoveryPolicy);

            if (request.Kind == D20NpcBehaviorRequestKind.ReportInjury)
            {
                if (input.Availability == D20CharacterAvailability.Unavailable ||
                    input.Availability == D20CharacterAvailability.Incapacitated ||
                    input.IsInjured)
                {
                    targetState = D20NpcBehaviorState.Injured;
                    reasonCode = "npc_behavior_injury_reported";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                    return true;
                }

                failureCode = D20NpcBehaviorFailureCode.InvalidRequest;
                reasonCode = "npc_behavior_injury_input_missing";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.ManualRetry;
                return false;
            }

            if (input.IsInjured ||
                input.Availability == D20CharacterAvailability.Incapacitated)
            {
                failureCode = D20NpcBehaviorFailureCode.Injured;
                reasonCode = "npc_behavior_injured";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                return false;
            }

            if (input.Availability == D20CharacterAvailability.Unavailable &&
                request.Kind != D20NpcBehaviorRequestKind.Recover)
            {
                if (request.Kind == D20NpcBehaviorRequestKind.Refuse)
                {
                    targetState = D20NpcBehaviorState.Refuse;
                    reasonCode = "npc_behavior_refused_by_owner";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                    return true;
                }

                failureCode = D20NpcBehaviorFailureCode.NpcUnavailable;
                reasonCode = "npc_behavior_unavailable";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                return false;
            }

            if (request.Kind == D20NpcBehaviorRequestKind.RaiseAlarm)
            {
                targetState = D20NpcBehaviorState.Alarm;
                reasonCode = "npc_behavior_alarm_raised";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForDangerToClear;
                return true;
            }

            if (request.Kind == D20NpcBehaviorRequestKind.Refuse)
            {
                targetState = D20NpcBehaviorState.Refuse;
                reasonCode = string.IsNullOrEmpty(request.ReasonCode)
                    ? "npc_behavior_refused"
                    : request.ReasonCode;
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryAfterRefusalCleared;
                return true;
            }

            if (request.Kind == D20NpcBehaviorRequestKind.Recover)
            {
                if (input.IsRestRequired)
                {
                    failureCode = D20NpcBehaviorFailureCode.RecoveryBlocked;
                    reasonCode = "npc_behavior_recovery_requires_rest";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RestBeforeRetry;
                    return false;
                }
                if (D20NpcScheduleInputContracts.IsAlarmDanger(input.DangerLevel) ||
                    D20NpcScheduleInputContracts.IsSevereWeather(input.Weather))
                {
                    failureCode = D20NpcBehaviorFailureCode.RecoveryBlocked;
                    reasonCode = "npc_behavior_recovery_danger_present";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForDangerToClear;
                    return false;
                }
                return TryResolveScheduledState(input, out targetState, out failureCode,
                    out reasonCode, out recoveryPolicy);
            }

            if (D20NpcScheduleInputContracts.IsAlarmDanger(input.DangerLevel))
            {
                failureCode = D20NpcBehaviorFailureCode.DangerPresent;
                reasonCode = "npc_behavior_danger_present";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForDangerToClear;
                return false;
            }
            if (D20NpcScheduleInputContracts.IsSevereWeather(input.Weather))
            {
                failureCode = D20NpcBehaviorFailureCode.WeatherUnsafe;
                reasonCode = "npc_behavior_weather_unsafe";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForDangerToClear;
                return false;
            }
            if (input.IsRestRequired &&
                request.Kind != D20NpcBehaviorRequestKind.RequestRest)
            {
                failureCode = D20NpcBehaviorFailureCode.RestRequired;
                reasonCode = "npc_behavior_rest_required";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RestBeforeRetry;
                return false;
            }

            switch (request.Kind)
            {
                case D20NpcBehaviorRequestKind.StartWork:
                    if (!input.IsWorkPeriod)
                    {
                        failureCode = D20NpcBehaviorFailureCode.OutsideWorkPeriod;
                        reasonCode = "npc_behavior_outside_work_period";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryNextSchedule;
                        return false;
                    }
                    if (!D20NpcScheduleInputContracts.IsWorkDuty(input.ScheduledDuty))
                    {
                        failureCode = D20NpcBehaviorFailureCode.MissingWorkDuty;
                        reasonCode = "npc_behavior_work_duty_missing";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryNextSchedule;
                        return false;
                    }
                    targetState = StateForDuty(input.ScheduledDuty);
                    reasonCode = "npc_behavior_work_started";
                    return true;

                case D20NpcBehaviorRequestKind.RequestTrade:
                    if (input.IsBusy || input.Availability == D20CharacterAvailability.Busy)
                    {
                        failureCode = D20NpcBehaviorFailureCode.Busy;
                        reasonCode = "npc_behavior_busy";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryNextSchedule;
                        return false;
                    }
                    if (!HasCapability(input.RoleId, D20NpcRoleCapability.Trade))
                    {
                        failureCode = D20NpcBehaviorFailureCode.RoleCapabilityMissing;
                        reasonCode = "npc_behavior_trade_capability_missing";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.ManualRetry;
                        return false;
                    }
                    targetState = D20NpcBehaviorState.Trade;
                    reasonCode = "npc_behavior_trade_requested";
                    return true;

                case D20NpcBehaviorRequestKind.RequestTalk:
                    if (input.IsBusy || input.Availability == D20CharacterAvailability.Busy)
                    {
                        failureCode = D20NpcBehaviorFailureCode.Busy;
                        reasonCode = "npc_behavior_busy";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryNextSchedule;
                        return false;
                    }
                    if (!HasCapability(input.RoleId, D20NpcRoleCapability.Dialogue))
                    {
                        failureCode = D20NpcBehaviorFailureCode.RoleCapabilityMissing;
                        reasonCode = "npc_behavior_dialogue_capability_missing";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.ManualRetry;
                        return false;
                    }
                    targetState = D20NpcBehaviorState.Talk;
                    reasonCode = "npc_behavior_talk_requested";
                    return true;

                case D20NpcBehaviorRequestKind.RequestRest:
                    if (input.IsBusy || input.Availability == D20CharacterAvailability.Busy)
                    {
                        failureCode = D20NpcBehaviorFailureCode.Busy;
                        reasonCode = "npc_behavior_busy";
                        recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RetryNextSchedule;
                        return false;
                    }
                    targetState = D20NpcBehaviorState.Rest;
                    reasonCode = "npc_behavior_rest_requested";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.None;
                    return true;

                case D20NpcBehaviorRequestKind.Cancel:
                    targetState = input.IsRestRequired
                        ? D20NpcBehaviorState.Rest
                        : D20NpcBehaviorState.Idle;
                    reasonCode = "npc_behavior_cancelled";
                    return true;

                default:
                    failureCode = D20NpcBehaviorFailureCode.InvalidRequest;
                    reasonCode = "npc_behavior_request_not_actionable";
                    recoveryPolicy = D20NpcBehaviorRecoveryPolicy.ManualRetry;
                    return false;
            }
        }

        private static bool TryResolveScheduledState(
            D20NpcScheduleInput input,
            out D20NpcBehaviorState targetState,
            out D20NpcBehaviorFailureCode failureCode,
            out string reasonCode,
            out D20NpcBehaviorRecoveryPolicy recoveryPolicy)
        {
            targetState = D20NpcBehaviorState.Idle;
            failureCode = D20NpcBehaviorFailureCode.None;
            reasonCode = "npc_behavior_schedule_idle";
            recoveryPolicy = D20NpcBehaviorRecoveryPolicy.None;

            if (input.IsInjured ||
                input.Availability == D20CharacterAvailability.Incapacitated)
            {
                targetState = D20NpcBehaviorState.Injured;
                reasonCode = "npc_behavior_injured";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                return true;
            }
            if (input.Availability == D20CharacterAvailability.Unavailable)
            {
                targetState = D20NpcBehaviorState.Refuse;
                reasonCode = "npc_behavior_unavailable";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForOwner;
                return true;
            }
            if (D20NpcScheduleInputContracts.IsAlarmDanger(input.DangerLevel))
            {
                targetState = D20NpcBehaviorState.Alarm;
                reasonCode = "npc_behavior_alarm_schedule";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.WaitForDangerToClear;
                return true;
            }
            if (D20NpcScheduleInputContracts.IsSevereWeather(input.Weather) ||
                (input.Weather == D20NpcWeatherKind.Rain && !input.ShelterAvailable))
            {
                targetState = D20NpcBehaviorState.Rest;
                reasonCode = "npc_behavior_weather_rest";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.RestBeforeRetry;
                return true;
            }
            if (input.IsRestRequired ||
                D20NpcScheduleInputContracts.IsRestDuty(input.ScheduledDuty))
            {
                targetState = D20NpcBehaviorState.Rest;
                reasonCode = "npc_behavior_schedule_rest";
                recoveryPolicy = D20NpcBehaviorRecoveryPolicy.None;
                return true;
            }
            if (input.IsWorkPeriod &&
                D20NpcScheduleInputContracts.IsWorkDuty(input.ScheduledDuty))
            {
                targetState = StateForDuty(input.ScheduledDuty);
                reasonCode = targetState == D20NpcBehaviorState.Trade
                    ? "npc_behavior_schedule_trade"
                    : "npc_behavior_schedule_work";
                return true;
            }

            targetState = D20NpcBehaviorState.Idle;
            return true;
        }

        private static D20NpcBehaviorState StateForDuty(D20NpcDutyKind duty)
        {
            return duty == D20NpcDutyKind.Trade ||
                   duty == D20NpcDutyKind.Market ||
                   duty == D20NpcDutyKind.SecretTrade
                ? D20NpcBehaviorState.Trade
                : D20NpcBehaviorState.Work;
        }

        private static bool HasCapability(string roleId, D20NpcRoleCapability capability)
        {
            D20NpcRoleDefinition definition = D20NpcRoleCatalog.Find(roleId);
            return definition != null &&
                   (definition.Capabilities & capability) == capability;
        }

        private static D20NpcBehaviorState GetResumeState(
            D20NpcBehaviorState previous,
            D20NpcBehaviorState target)
        {
            if (target == D20NpcBehaviorState.Alarm ||
                target == D20NpcBehaviorState.Injured ||
                target == D20NpcBehaviorState.Refuse)
                return IsNormalState(previous) ? previous : D20NpcBehaviorState.Idle;
            return IsNormalState(target) ? target : D20NpcBehaviorState.Idle;
        }

        private static D20NpcBehaviorRequest CreateConvenienceRequest(
            D20NpcBehaviorRequestKind kind,
            D20NpcScheduleInput schedule)
        {
            if (schedule == null) return null;
            return D20NpcBehaviorRequest.Create(
                kind,
                BuildCommandId(schedule, kind),
                schedule);
        }

        private static string BuildCommandId(
            D20NpcScheduleInput schedule,
            D20NpcBehaviorRequestKind kind)
        {
            if (schedule == null) return string.Empty;
            return "npc_behavior:" + (schedule.NpcId ?? string.Empty) + ":" +
                   kind + ":" + schedule.WorldDay + ":" +
                   schedule.TimeSlot + ":" + schedule.SaveRevision;
        }

        private D20NpcBehaviorTransitionResult CreateBaseResult(
            D20NpcBehaviorRequest request)
        {
            return new D20NpcBehaviorTransitionResult
            {
                Accepted = false,
                Succeeded = false,
                Changed = false,
                CommandId = request == null ? string.Empty : request.CommandId ?? string.Empty,
                NpcId = request == null ? _snapshot.NpcId : request.NpcId ?? string.Empty,
                FromState = _snapshot.State,
                ToState = _snapshot.State,
                FailureCode = D20NpcBehaviorFailureCode.InvalidRequest,
                ReasonCode = string.Empty,
                RecoveryPolicy = D20NpcBehaviorRecoveryPolicy.ManualRetry,
                WorldDay = request == null || request.Schedule == null
                    ? _snapshot.StateSinceWorldDay : request.Schedule.WorldDay,
                TimeSlot = request == null || request.Schedule == null
                    ? _snapshot.StateSinceTimeSlot : request.Schedule.TimeSlot,
                SaveRevision = request == null || request.Schedule == null
                    ? _snapshot.SaveRevision : request.Schedule.SaveRevision,
            };
        }

        private static D20NpcBehaviorTransitionResult Fail(
            D20NpcBehaviorTransitionResult result,
            D20NpcBehaviorFailureCode failureCode,
            string reasonCode,
            D20NpcBehaviorRecoveryPolicy recoveryPolicy)
        {
            result.Accepted = false;
            result.Succeeded = false;
            result.Changed = false;
            result.FailureCode = failureCode;
            result.ReasonCode = reasonCode ?? string.Empty;
            result.RecoveryPolicy = recoveryPolicy;
            result.ToState = result.FromState;
            return result;
        }
    }
}
