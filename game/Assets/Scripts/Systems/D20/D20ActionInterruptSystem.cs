// ============================================================
// D20ActionInterruptSystem —— D20-L06 动作打断与确定性回退（纯 C#）
//
// 本文件只拥有 D20 动作层的活动意图、暂停/恢复投影和回退语义。
// 受击、失能、presence 变化和取消都只改变动作投影；健康、身份、港口、
// 位置以及其它业务事实仍由所属 owner 裁决。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L06 触发动作用回退的稳定原因。</summary>
    public enum D20ActionInterruptReason
    {
        /// <summary>没有原因，不能提交。</summary>
        None = 0,
        /// <summary>逻辑暂停，保留可恢复意图。</summary>
        Pause = 1,
        /// <summary>收到已校验的受击意图。</summary>
        Hit = 2,
        /// <summary>所属 owner 报告角色暂时失能。</summary>
        Incapacitated = 3,
        /// <summary>语义 presence 已由 presence owner 裁决改变。</summary>
        PresenceChanged = 4,
        /// <summary>调用方取消指定命令。</summary>
        CommandCancelled = 5,
        /// <summary>恢复暂停或失能前保留的意图。</summary>
        Resume = 6,
        /// <summary>活动意图已经由其 owner 完成。</summary>
        Completed = 7,
        /// <summary>外部 owner 报告新的可用性语义。</summary>
        AvailabilityChanged = 8,
    }

    /// <summary>动作回退的稳定类别，表现层只读取该类别。</summary>
    public enum D20ActionFallbackKind
    {
        /// <summary>没有回退。</summary>
        None = 0,
        /// <summary>回到无活动动作的安全空闲状态。</summary>
        Idle = 1,
        /// <summary>进入暂停投影，等待显式恢复。</summary>
        Paused = 2,
        /// <summary>进入不可行动投影，等待 owner 恢复。</summary>
        Unavailable = 3,
        /// <summary>presence 变化导致当前动作停止。</summary>
        Presence = 4,
    }

    /// <summary>L06 命令的稳定失败码。</summary>
    public enum D20ActionInterruptFailureCode
    {
        /// <summary>没有失败。</summary>
        None = 0,
        /// <summary>请求载体为空。</summary>
        MissingRequest = 1,
        /// <summary>请求版本不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>缺少稳定命令 ID。</summary>
        MissingCommandId = 3,
        /// <summary>缺少稳定行为者 ID。</summary>
        MissingActorId = 4,
        /// <summary>请求行为者与已绑定动作状态不一致。</summary>
        ActorMismatch = 5,
        /// <summary>原因或原因所需载荷不合法。</summary>
        InvalidReason = 6,
        /// <summary>没有可取消或完成的活动动作。</summary>
        NoActiveAction = 7,
        /// <summary>指定命令不是当前活动动作。</summary>
        CommandNotActive = 8,
        /// <summary>presence owner 没有返回可接受的事实结果。</summary>
        InvalidPresenceResult = 9,
        /// <summary>当前动作投影不允许该迁移。</summary>
        StateConflict = 10,
        /// <summary>相同命令 ID 已处理。</summary>
        DuplicateCommand = 11,
        /// <summary>相同命令 ID 携带了不同请求。</summary>
        DuplicateCommandConflict = 12,
        /// <summary>动作意图契约无效。</summary>
        InvalidIntent = 13,
        /// <summary>状态快照违反不变量。</summary>
        InvariantViolation = 14,
    }

    /// <summary>动作打断请求；只携带稳定逻辑引用，不携带引擎对象。</summary>
    [Serializable]
    public sealed class D20ActionInterruptRequest
    {
        /// <summary>当前请求版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>请求版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>跨重试稳定的请求 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>提出请求的角色稳定 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>请求原因。</summary>
        public D20ActionInterruptReason Reason = D20ActionInterruptReason.None;

        /// <summary>受击时由调用方提供的已校验动作意图。</summary>
        public D20ActionIntent IncomingIntent;

        /// <summary>presence owner 已裁决的结果；D20 不直接改变 presence。</summary>
        public D20PlayerPresenceResult PresenceResult;

        /// <summary>可用性 owner 返回的语义；Unknown 表示本请求不携带该输入。</summary>
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;

        /// <summary>取消/完成时需要匹配的活动命令 ID。</summary>
        public string TargetCommandId = string.Empty;

        /// <summary>恢复时是否尝试恢复保留的意图。</summary>
        public bool ResumeSuspendedAction = true;

        /// <summary>产生请求的世界日。</summary>
        public int WorldDay;

        /// <summary>产生请求时的存档修订。</summary>
        public long SaveRevision;

        /// <summary>上游事实事件引用。</summary>
        public List<string> CauseEventIds = new List<string>();

        /// <summary>返回防御性请求副本。</summary>
        public D20ActionInterruptRequest Clone()
        {
            var result = new D20ActionInterruptRequest
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                Reason = Reason,
                IncomingIntent = IncomingIntent == null ? null : IncomingIntent.Clone(),
                PresenceResult = PresenceResult == null ? null : PresenceResult.Clone(),
                Availability = Availability,
                TargetCommandId = TargetCommandId ?? string.Empty,
                ResumeSuspendedAction = ResumeSuspendedAction,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            return result;
        }

        /// <summary>创建暂停请求。</summary>
        public static D20ActionInterruptRequest Pause(string commandId, string actorId,
            int worldDay = 0, long saveRevision = 0)
        {
            return Create(commandId, actorId, D20ActionInterruptReason.Pause,
                worldDay, saveRevision);
        }

        /// <summary>创建受击请求。</summary>
        public static D20ActionInterruptRequest Hit(D20ActionIntent hitIntent)
        {
            if (hitIntent == null) throw new ArgumentNullException("hitIntent");
            return new D20ActionInterruptRequest
            {
                CommandId = hitIntent.CommandId ?? string.Empty,
                ActorId = hitIntent.ActorId ?? string.Empty,
                Reason = D20ActionInterruptReason.Hit,
                IncomingIntent = hitIntent.Clone(),
                WorldDay = hitIntent.WorldDay,
                SaveRevision = hitIntent.SaveRevision,
                CauseEventIds = hitIntent.CauseEventIds == null
                    ? new List<string>()
                    : new List<string>(hitIntent.CauseEventIds),
            };
        }

        /// <summary>创建由 presence owner 结果驱动的切换请求。</summary>
        public static D20ActionInterruptRequest Presence(D20PlayerPresenceResult presenceResult)
        {
            if (presenceResult == null) throw new ArgumentNullException("presenceResult");
            var presenceEvent = presenceResult.Event;
            return new D20ActionInterruptRequest
            {
                CommandId = presenceEvent == null ? string.Empty : presenceEvent.EventId ?? string.Empty,
                ActorId = presenceEvent == null ? string.Empty : presenceEvent.PlayerId ?? string.Empty,
                Reason = D20ActionInterruptReason.PresenceChanged,
                PresenceResult = presenceResult.Clone(),
                WorldDay = presenceEvent == null ? 0 : presenceEvent.WorldDay,
                SaveRevision = presenceEvent == null ? 0 : presenceEvent.SaveRevision,
                CauseEventIds = presenceEvent == null || presenceEvent.CauseEventIds == null
                    ? new List<string>()
                    : new List<string>(presenceEvent.CauseEventIds),
            };
        }

        /// <summary>创建可用性变化请求。</summary>
        public static D20ActionInterruptRequest AvailabilityChanged(string commandId,
            string actorId, D20CharacterAvailability availability, int worldDay = 0,
            long saveRevision = 0)
        {
            return new D20ActionInterruptRequest
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = availability == D20CharacterAvailability.Incapacitated
                    ? D20ActionInterruptReason.Incapacitated
                    : D20ActionInterruptReason.AvailabilityChanged,
                Availability = availability,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>创建命令取消请求。</summary>
        public static D20ActionInterruptRequest Cancel(string commandId, string actorId,
            string targetCommandId, int worldDay = 0, long saveRevision = 0)
        {
            return new D20ActionInterruptRequest
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = D20ActionInterruptReason.CommandCancelled,
                TargetCommandId = targetCommandId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>创建恢复请求。</summary>
        public static D20ActionInterruptRequest Resume(string commandId, string actorId,
            bool resumeSuspendedAction = true, int worldDay = 0, long saveRevision = 0)
        {
            return new D20ActionInterruptRequest
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = D20ActionInterruptReason.Resume,
                ResumeSuspendedAction = resumeSuspendedAction,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        private static D20ActionInterruptRequest Create(string commandId, string actorId,
            D20ActionInterruptReason reason, int worldDay, long saveRevision)
        {
            return new D20ActionInterruptRequest
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = reason,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }
    }

    /// <summary>用于读档后幂等恢复的最小命令回执。</summary>
    [Serializable]
    public sealed class D20ActionInterruptReceipt
    {
        /// <summary>命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>请求内容指纹。</summary>
        public string RequestFingerprint = string.Empty;

        /// <summary>命令是否被接受。</summary>
        public bool Accepted;

        /// <summary>稳定结果原因。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>结果事件 ID。</summary>
        public string EventId = string.Empty;

        /// <summary>返回副本。</summary>
        public D20ActionInterruptReceipt Clone()
        {
            return new D20ActionInterruptReceipt
            {
                CommandId = CommandId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                Accepted = Accepted,
                ReasonCode = ReasonCode ?? string.Empty,
                EventId = EventId ?? string.Empty,
            };
        }
    }

    /// <summary>L06 动作投影快照；只保存 D20 自有动作语义。</summary>
    [Serializable]
    public sealed class D20ActionInterruptSnapshot
    {
        /// <summary>当前快照版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>快照版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>状态所属角色 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>当前语义 presence；Unknown 仅允许初始化时使用。</summary>
        public D20PlayerPresenceState PresenceState = D20PlayerPresenceState.Unknown;

        /// <summary>所属 owner 提供的当前可用性语义。</summary>
        public D20CharacterAvailability Availability = D20CharacterAvailability.Available;

        /// <summary>逻辑暂停标记。</summary>
        public bool IsPaused;

        /// <summary>当前活动动作；为空表示没有活动动作。</summary>
        public D20ActionIntent ActiveIntent;

        /// <summary>暂停、失能或 presence 回退时保留的动作。</summary>
        public D20ActionIntent SuspendedIntent;

        /// <summary>最近一次收到的受击意图，仅作只读表现输入。</summary>
        public D20ActionIntent LastIncomingIntent;

        /// <summary>当前回退类别。</summary>
        public D20ActionFallbackKind FallbackKind = D20ActionFallbackKind.None;

        /// <summary>当前回退稳定 ID。</summary>
        public string FallbackActionId = string.Empty;

        /// <summary>活动意图当前状态。</summary>
        public D20ActionIntentStatus ActiveStatus = D20ActionIntentStatus.Cancelled;

        /// <summary>最近一次命令 ID。</summary>
        public string LastCommandId = string.Empty;

        /// <summary>最近一次结果事件 ID。</summary>
        public string LastEventId = string.Empty;

        /// <summary>最近一次结果原因键。</summary>
        public string LastReasonCode = string.Empty;

        /// <summary>最近一次结果世界日。</summary>
        public int WorldDay;

        /// <summary>最近一次结果存档修订。</summary>
        public long SaveRevision;

        /// <summary>已处理命令回执。</summary>
        public List<D20ActionInterruptReceipt> CommandReceipts =
            new List<D20ActionInterruptReceipt>();

        /// <summary>返回深拷贝。</summary>
        public D20ActionInterruptSnapshot Clone()
        {
            var result = new D20ActionInterruptSnapshot
            {
                SchemaVersion = SchemaVersion,
                ActorId = ActorId ?? string.Empty,
                PresenceState = PresenceState,
                Availability = Availability,
                IsPaused = IsPaused,
                ActiveIntent = ActiveIntent == null ? null : ActiveIntent.Clone(),
                SuspendedIntent = SuspendedIntent == null ? null : SuspendedIntent.Clone(),
                LastIncomingIntent = LastIncomingIntent == null ? null : LastIncomingIntent.Clone(),
                FallbackKind = FallbackKind,
                FallbackActionId = FallbackActionId ?? string.Empty,
                ActiveStatus = ActiveStatus,
                LastCommandId = LastCommandId ?? string.Empty,
                LastEventId = LastEventId ?? string.Empty,
                LastReasonCode = LastReasonCode ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                CommandReceipts = new List<D20ActionInterruptReceipt>(),
            };
            if (CommandReceipts != null)
                foreach (var receipt in CommandReceipts)
                    if (receipt != null) result.CommandReceipts.Add(receipt.Clone());
            return result;
        }

        /// <summary>校验快照不变量；失败时不允许进入恢复路径。</summary>
        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 action interrupt schema_version mismatch.");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(ActorId))
                throw new InvalidOperationException("D20 action interrupt actor_id is missing or unstable.");
            if (PresenceState != D20PlayerPresenceState.Unknown &&
                !D20PlayerPresenceSystem.IsSupportedState(PresenceState))
                throw new InvalidOperationException("D20 action interrupt presence_state is unknown.");
            if (!Enum.IsDefined(typeof(D20CharacterAvailability), Availability) ||
                Availability == D20CharacterAvailability.Unknown)
                throw new InvalidOperationException("D20 action interrupt availability is unknown.");
            if (ActiveIntent != null)
            {
                D20ActionIntentValidationResult validation = ActiveIntent.Validate();
                if (!validation.Valid)
                    throw new InvalidOperationException("D20 active intent is invalid: " + validation.ReasonCode);
                if (!string.Equals(ActorId, ActiveIntent.ActorId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 active intent actor_id conflicts with snapshot actor_id.");
            }
            if (SuspendedIntent != null)
            {
                D20ActionIntentValidationResult validation = SuspendedIntent.Validate();
                if (!validation.Valid)
                    throw new InvalidOperationException("D20 suspended intent is invalid: " + validation.ReasonCode);
                if (!string.Equals(ActorId, SuspendedIntent.ActorId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 suspended intent actor_id conflicts with snapshot actor_id.");
            }
            if (LastIncomingIntent != null)
            {
                D20ActionIntentValidationResult validation = LastIncomingIntent.Validate();
                if (!validation.Valid)
                    throw new InvalidOperationException("D20 incoming intent is invalid: " + validation.ReasonCode);
                if (!string.Equals(ActorId, LastIncomingIntent.ActorId, StringComparison.Ordinal))
                    throw new InvalidOperationException("D20 incoming intent actor_id conflicts with snapshot actor_id.");
            }
            if (WorldDay < 0 || SaveRevision < 0)
                throw new InvalidOperationException("D20 action interrupt world_day/save_revision must be non-negative.");
            if (IsPaused && ActiveIntent != null)
                throw new InvalidOperationException("D20 paused action cannot retain an active intent.");
            if ((Availability == D20CharacterAvailability.Incapacitated ||
                 Availability == D20CharacterAvailability.Dead ||
                 Availability == D20CharacterAvailability.Departed) &&
                ActiveIntent != null)
                throw new InvalidOperationException("D20 unavailable action cannot retain an active intent.");
            if (CommandReceipts == null)
                throw new InvalidOperationException("D20 action interrupt command_receipts are required.");
        }
    }

    /// <summary>L06 动作打断命令回执。</summary>
    [Serializable]
    public sealed class D20ActionInterruptResult
    {
        /// <summary>命令是否被接受并应用。</summary>
        public bool Accepted;

        /// <summary>是否返回了历史回执。</summary>
        public bool Duplicate;

        /// <summary>状态是否发生变化。</summary>
        public bool Changed;

        /// <summary>命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>用于幂等判断的确定性输入指纹。</summary>
        public string RequestFingerprint = string.Empty;

        /// <summary>行为者 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>处理原因。</summary>
        public D20ActionInterruptReason Reason;

        /// <summary>动作链状态。</summary>
        public D20ActionIntentStatus Status = D20ActionIntentStatus.Rejected;

        /// <summary>稳定失败码；成功时为 None。</summary>
        public D20ActionInterruptFailureCode FailureCode;

        /// <summary>机器可读原因键。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>打断前活动动作。</summary>
        public D20ActionIntent PreviousIntent;

        /// <summary>打断后活动动作。</summary>
        public D20ActionIntent CurrentIntent;

        /// <summary>打断后保留的待恢复动作。</summary>
        public D20ActionIntent SuspendedIntent;

        /// <summary>外部传入的受击意图。</summary>
        public D20ActionIntent IncomingIntent;

        /// <summary>回退类别和稳定 ID。</summary>
        public D20ActionFallbackKind FallbackKind;
        public string FallbackActionId = string.Empty;

        /// <summary>当前 presence、可用性和暂停状态。</summary>
        public D20PlayerPresenceState PresenceState;
        public D20CharacterAvailability Availability;
        public bool IsPaused;

        /// <summary>结果事件 ID。</summary>
        public string EventId = string.Empty;

        /// <summary>结果时的世界日和存档修订。</summary>
        public int WorldDay;
        public long SaveRevision;

        /// <summary>优先级评估副本；不代表业务 owner 已成功。</summary>
        public D20ActionPriorityDecision PriorityDecision;

        /// <summary>结果后的防御性状态快照。</summary>
        public D20ActionInterruptSnapshot State;

        /// <summary>返回深拷贝。</summary>
        public D20ActionInterruptResult Clone()
        {
            return new D20ActionInterruptResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                Changed = Changed,
                CommandId = CommandId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                Reason = Reason,
                Status = Status,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                PreviousIntent = PreviousIntent == null ? null : PreviousIntent.Clone(),
                CurrentIntent = CurrentIntent == null ? null : CurrentIntent.Clone(),
                SuspendedIntent = SuspendedIntent == null ? null : SuspendedIntent.Clone(),
                IncomingIntent = IncomingIntent == null ? null : IncomingIntent.Clone(),
                FallbackKind = FallbackKind,
                FallbackActionId = FallbackActionId ?? string.Empty,
                PresenceState = PresenceState,
                Availability = Availability,
                IsPaused = IsPaused,
                EventId = EventId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                PriorityDecision = PriorityDecision == null ? null : PriorityDecision.Clone(),
                State = State == null ? null : State.Clone(),
            };
        }
    }

    /// <summary>
    /// D20-L06 动作打断唯一写者。它只写自己的动作投影，不写外部 owner 事实。
    /// </summary>
    public sealed class D20ActionInterruptSystem
    {
        /// <summary>动作打断投影的 owner 标识。</summary>
        public const string OwnerSystem = "D20ActionInterruptSystem";

        /// <summary>确定性空闲回退 ID。</summary>
        public const string IdleFallbackActionId = "fallback.idle";

        /// <summary>确定性暂停回退 ID。</summary>
        public const string PausedFallbackActionId = "fallback.paused";

        /// <summary>确定性不可行动回退 ID。</summary>
        public const string UnavailableFallbackActionId = "fallback.unavailable";

        /// <summary>确定性 presence 回退 ID。</summary>
        public const string PresenceFallbackActionId = "fallback.presence";

        private readonly D20ActionInterruptSnapshot _state;
        private readonly Dictionary<string, D20ActionInterruptResult> _receipts =
            new Dictionary<string, D20ActionInterruptResult>(StringComparer.Ordinal);

        /// <summary>创建空动作投影；首次合法请求绑定角色 ID。</summary>
        public D20ActionInterruptSystem()
        {
            _state = new D20ActionInterruptSnapshot();
        }

        /// <summary>从合法快照恢复动作投影和幂等索引。</summary>
        public D20ActionInterruptSystem(D20ActionInterruptSnapshot initialSnapshot)
        {
            if (initialSnapshot == null) throw new ArgumentNullException("initialSnapshot");
            _state = initialSnapshot.Clone();
            _state.Validate();
            RebuildReceiptIndex();
        }

        /// <summary>返回防御性状态快照。</summary>
        public D20ActionInterruptSnapshot Snapshot
        {
            get { return _state.Clone(); }
        }

        /// <summary>登记一个已通过 L05 前置的活动意图。</summary>
        public D20ActionInterruptResult RegisterActiveAction(D20ActionIntent intent,
            D20ActionPriorityContext context = null)
        {
            if (intent == null)
                return Reject(string.Empty, string.Empty, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.MissingRequest, "intent_missing");

            string commandId = intent.CommandId ?? string.Empty;
            string fingerprint = IntentFingerprint(intent);
            D20ActionInterruptResult duplicate = FindDuplicate(commandId, fingerprint);
            if (duplicate != null) return duplicate;

            D20ActionIntentValidationResult validation = intent.Validate();
            if (!validation.Valid)
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.InvalidIntent, validation.ReasonCode, fingerprint);
            if (!BindOrValidateActor(intent.ActorId, out D20ActionInterruptFailureCode actorCode,
                out string actorReason))
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    actorCode, actorReason, fingerprint);
            if (_state.IsPaused)
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.StateConflict, "action_layer_paused", fingerprint,
                    D20ActionIntentStatus.Unavailable);
            if (!IsAvailableForAction(_state.Availability))
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.StateConflict, "action_layer_unavailable", fingerprint,
                    D20ActionIntentStatus.Unavailable);
            if (_state.ActiveIntent != null)
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.StateConflict, "active_action_exists", fingerprint);

            var safeContext = context == null ? new D20ActionPriorityContext() : context.Clone();
            safeContext.PresenceState = _state.PresenceState;
            safeContext.CharacterAvailable = safeContext.CharacterAvailable &&
                IsAvailableForAction(_state.Availability);
            D20ActionPriorityDecision decision = D20ActionPriorityRules.Evaluate(intent, safeContext);
            if (!decision.Allowed)
                return RejectAndRemember(commandId, intent.ActorId, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.StateConflict, decision.ReasonCode, fingerprint,
                    D20ActionIntentStatus.Rejected, decision);

            _state.ActorId = intent.ActorId;
            _state.ActiveIntent = intent.Clone();
            _state.SuspendedIntent = null;
            _state.ActiveStatus = D20ActionIntentStatus.Accepted;
            _state.FallbackKind = D20ActionFallbackKind.None;
            _state.FallbackActionId = string.Empty;
            return Commit(commandId, intent.ActorId, D20ActionInterruptReason.None,
                D20ActionIntentStatus.Accepted, "action_registered", true, null,
                intent, null, null, D20ActionFallbackKind.None, string.Empty,
                decision, fingerprint, intent.WorldDay, intent.SaveRevision);
        }

        /// <summary>应用暂停、受击、失能、presence 变化、恢复或取消请求。</summary>
        public D20ActionInterruptResult Apply(D20ActionInterruptRequest request)
        {
            if (request == null)
                return Reject(string.Empty, string.Empty, D20ActionInterruptReason.None,
                    D20ActionInterruptFailureCode.MissingRequest, "request_missing");

            string commandId = request.CommandId ?? string.Empty;
            string fingerprint = RequestFingerprint(request);
            D20ActionInterruptResult duplicate = FindDuplicate(commandId, fingerprint);
            if (duplicate != null) return duplicate;

            D20ActionInterruptFailureCode validationCode;
            string validationReason;
            if (!ValidateRequest(request, out validationCode, out validationReason))
                return RejectAndRemember(commandId, request.ActorId, request.Reason,
                    validationCode, validationReason, fingerprint);
            if (!BindOrValidateActor(request.ActorId, out validationCode, out validationReason))
                return RejectAndRemember(commandId, request.ActorId, request.Reason,
                    validationCode, validationReason, fingerprint);

            D20ActionIntent previous = _state.ActiveIntent == null
                ? null : _state.ActiveIntent.Clone();
            D20ActionIntent incoming = request.IncomingIntent == null
                ? null : request.IncomingIntent.Clone();
            D20ActionPriorityDecision decision = null;
            D20ActionIntent current = null;
            D20ActionIntent suspended = _state.SuspendedIntent == null
                ? null : _state.SuspendedIntent.Clone();
            D20ActionFallbackKind fallbackKind = D20ActionFallbackKind.None;
            string fallbackId = string.Empty;
            D20ActionIntentStatus status = D20ActionIntentStatus.Accepted;
            string reasonCode;
            bool changed = false;

            switch (request.Reason)
            {
                case D20ActionInterruptReason.Pause:
                    if (!_state.IsPaused)
                    {
                        if (_state.ActiveIntent != null)
                        {
                            _state.SuspendedIntent = _state.ActiveIntent.Clone();
                            _state.ActiveIntent = null;
                            _state.ActiveStatus = D20ActionIntentStatus.Cancelled;
                        }
                        _state.IsPaused = true;
                        _state.FallbackKind = D20ActionFallbackKind.Paused;
                        _state.FallbackActionId = PausedFallbackActionId;
                        changed = true;
                    }
                    fallbackKind = D20ActionFallbackKind.Paused;
                    fallbackId = PausedFallbackActionId;
                    reasonCode = "action_paused";
                    status = D20ActionIntentStatus.Cancelled;
                    break;

                case D20ActionInterruptReason.Resume:
                    if (!_state.IsPaused)
                        return RejectAndRemember(commandId, request.ActorId, request.Reason,
                            D20ActionInterruptFailureCode.StateConflict, "action_layer_not_paused",
                            fingerprint);
                    _state.IsPaused = false;
                    if (request.ResumeSuspendedAction && _state.SuspendedIntent != null &&
                        IsAvailableForAction(_state.Availability) &&
                        IsPresenceCompatible(_state.SuspendedIntent, _state.PresenceState))
                    {
                        _state.ActiveIntent = _state.SuspendedIntent.Clone();
                        _state.SuspendedIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Accepted;
                        current = _state.ActiveIntent.Clone();
                    }
                    else
                    {
                        _state.ActiveIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Cancelled;
                    }
                    _state.FallbackKind = D20ActionFallbackKind.None;
                    _state.FallbackActionId = string.Empty;
                    changed = true;
                    reasonCode = current == null ? "action_resumed_idle" : "action_resumed";
                    status = current == null ? D20ActionIntentStatus.Cancelled :
                        D20ActionIntentStatus.Accepted;
                    break;

                case D20ActionInterruptReason.Hit:
                    if (_state.ActiveIntent != null)
                    {
                        _state.ActiveIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Cancelled;
                        changed = true;
                    }
                    _state.SuspendedIntent = null;
                    _state.LastIncomingIntent = incoming == null ? null : incoming.Clone();
                    _state.FallbackKind = D20ActionFallbackKind.Idle;
                    _state.FallbackActionId = IdleFallbackActionId;
                    fallbackKind = D20ActionFallbackKind.Idle;
                    fallbackId = IdleFallbackActionId;
                    reasonCode = "action_interrupted_by_hit";
                    status = D20ActionIntentStatus.Cancelled;
                    break;

                case D20ActionInterruptReason.Incapacitated:
                    _state.Availability = D20CharacterAvailability.Incapacitated;
                    if (_state.ActiveIntent != null)
                    {
                        _state.SuspendedIntent = _state.ActiveIntent.Clone();
                        _state.ActiveIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Unavailable;
                        changed = true;
                    }
                    _state.IsPaused = false;
                    _state.FallbackKind = D20ActionFallbackKind.Unavailable;
                    _state.FallbackActionId = UnavailableFallbackActionId;
                    fallbackKind = D20ActionFallbackKind.Unavailable;
                    fallbackId = UnavailableFallbackActionId;
                    reasonCode = "action_incapacitated";
                    status = D20ActionIntentStatus.Unavailable;
                    break;

                case D20ActionInterruptReason.AvailabilityChanged:
                    _state.Availability = request.Availability;
                    if (!IsAvailableForAction(request.Availability) &&
                        _state.ActiveIntent != null)
                    {
                        if (request.Availability == D20CharacterAvailability.Dead ||
                            request.Availability == D20CharacterAvailability.Departed)
                            _state.SuspendedIntent = null;
                        else
                            _state.SuspendedIntent = _state.ActiveIntent.Clone();
                        _state.ActiveIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Unavailable;
                        _state.FallbackKind = D20ActionFallbackKind.Unavailable;
                        _state.FallbackActionId = UnavailableFallbackActionId;
                        changed = true;
                        fallbackKind = D20ActionFallbackKind.Unavailable;
                        fallbackId = UnavailableFallbackActionId;
                        status = D20ActionIntentStatus.Unavailable;
                        reasonCode = "action_availability_fallback";
                    }
                    else
                    {
                        changed = true;
                        reasonCode = "action_availability_changed";
                        status = _state.ActiveIntent == null
                            ? D20ActionIntentStatus.Cancelled
                            : D20ActionIntentStatus.Accepted;
                    }
                    break;

                case D20ActionInterruptReason.PresenceChanged:
                    _state.PresenceState = request.PresenceResult.Event.CurrentState;
                    if (_state.ActiveIntent != null &&
                        !IsPresenceCompatible(_state.ActiveIntent, _state.PresenceState))
                    {
                        _state.SuspendedIntent = _state.ActiveIntent.Clone();
                        _state.ActiveIntent = null;
                        _state.ActiveStatus = D20ActionIntentStatus.Cancelled;
                        _state.FallbackKind = D20ActionFallbackKind.Presence;
                        _state.FallbackActionId = PresenceFallbackActionId;
                        fallbackKind = D20ActionFallbackKind.Presence;
                        fallbackId = PresenceFallbackActionId;
                        status = D20ActionIntentStatus.Cancelled;
                        reasonCode = "action_presence_fallback";
                    }
                    else
                    {
                        reasonCode = "action_presence_changed";
                        status = _state.ActiveIntent == null
                            ? D20ActionIntentStatus.Cancelled
                            : D20ActionIntentStatus.Accepted;
                    }
                    changed = true;
                    break;

                case D20ActionInterruptReason.CommandCancelled:
                    if (_state.ActiveIntent == null)
                        return RejectAndRemember(commandId, request.ActorId, request.Reason,
                            D20ActionInterruptFailureCode.NoActiveAction, "no_active_action",
                            fingerprint);
                    if (!string.Equals(_state.ActiveIntent.CommandId, request.TargetCommandId,
                        StringComparison.Ordinal))
                        return RejectAndRemember(commandId, request.ActorId, request.Reason,
                            D20ActionInterruptFailureCode.CommandNotActive, "command_not_active",
                            fingerprint);
                    _state.ActiveIntent = null;
                    _state.ActiveStatus = D20ActionIntentStatus.Cancelled;
                    _state.FallbackKind = D20ActionFallbackKind.Idle;
                    _state.FallbackActionId = IdleFallbackActionId;
                    fallbackKind = D20ActionFallbackKind.Idle;
                    fallbackId = IdleFallbackActionId;
                    reasonCode = "action_cancelled";
                    status = D20ActionIntentStatus.Cancelled;
                    changed = true;
                    break;

                default:
                    return RejectAndRemember(commandId, request.ActorId, request.Reason,
                        D20ActionInterruptFailureCode.InvalidReason, "invalid_interrupt_reason",
                        fingerprint);
            }

            _state.LastIncomingIntent = request.Reason == D20ActionInterruptReason.Hit
                ? incoming : _state.LastIncomingIntent;
            current = _state.ActiveIntent == null ? current : _state.ActiveIntent.Clone();
            suspended = _state.SuspendedIntent == null ? null : _state.SuspendedIntent.Clone();
            return Commit(commandId, request.ActorId, request.Reason, status, reasonCode,
                changed || request.Reason == D20ActionInterruptReason.Pause ||
                request.Reason == D20ActionInterruptReason.Resume,
                previous, current, suspended, incoming, fallbackKind, fallbackId,
                decision, fingerprint, request.WorldDay, request.SaveRevision);
        }

        /// <summary>Apply 的语义别名，供后续命令聚合器使用。</summary>
        public D20ActionInterruptResult Handle(D20ActionInterruptRequest request)
        {
            return Apply(request);
        }

        /// <summary>完成当前活动动作并清除动作槽。</summary>
        public D20ActionInterruptResult Complete(string commandId, string actorId,
            int worldDay = 0, long saveRevision = 0)
        {
            var request = new D20ActionInterruptRequest
            {
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = D20ActionInterruptReason.Completed,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
            string fingerprint = RequestFingerprint(request);
            D20ActionInterruptResult duplicate = FindDuplicate(request.CommandId, fingerprint);
            if (duplicate != null) return duplicate;
            if (!D20CharacterIdentityContracts.IsStableCharacterId(request.ActorId))
                return RejectAndRemember(request.CommandId, request.ActorId, request.Reason,
                    D20ActionInterruptFailureCode.MissingActorId, "missing_actor_id", fingerprint);
            if (!BindOrValidateActor(request.ActorId, out D20ActionInterruptFailureCode code,
                out string reason))
                return RejectAndRemember(request.CommandId, request.ActorId, request.Reason,
                    code, reason, fingerprint);
            if (_state.ActiveIntent == null)
                return RejectAndRemember(request.CommandId, request.ActorId, request.Reason,
                    D20ActionInterruptFailureCode.NoActiveAction, "no_active_action", fingerprint);
            D20ActionIntent previous = _state.ActiveIntent.Clone();
            _state.ActiveIntent = null;
            _state.ActiveStatus = D20ActionIntentStatus.Succeeded;
            _state.FallbackKind = D20ActionFallbackKind.None;
            _state.FallbackActionId = string.Empty;
            return Commit(request.CommandId, request.ActorId, request.Reason,
                D20ActionIntentStatus.Succeeded, "action_completed", true, previous,
                null, _state.SuspendedIntent, null, D20ActionFallbackKind.None, string.Empty,
                null, fingerprint, worldDay, saveRevision);
        }

        /// <summary>捕获可交给 SaveSystem 组合器的动作投影。</summary>
        public D20ActionInterruptSnapshot CaptureSnapshot()
        {
            return Snapshot;
        }

        /// <summary>恢复 D20 自有动作投影，不推断外部事实。</summary>
        public void Restore(D20ActionInterruptSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            snapshot.Validate();
            CopyInto(_state, snapshot);
            RebuildReceiptIndex();
        }

        private D20ActionInterruptResult Commit(string commandId, string actorId,
            D20ActionInterruptReason reason, D20ActionIntentStatus status, string reasonCode,
            bool changed, D20ActionIntent previous, D20ActionIntent current,
            D20ActionIntent suspended, D20ActionIntent incoming,
            D20ActionFallbackKind fallbackKind, string fallbackId,
            D20ActionPriorityDecision decision, string fingerprint,
            int worldDay, long saveRevision)
        {
            _state.LastCommandId = commandId ?? string.Empty;
            _state.LastEventId = EventIdFor(commandId, reason);
            _state.LastReasonCode = reasonCode ?? string.Empty;
            _state.WorldDay = Math.Max(0, worldDay);
            _state.SaveRevision = Math.Max(0L, saveRevision);
            var result = new D20ActionInterruptResult
            {
                Accepted = true,
                Duplicate = false,
                Changed = changed,
                CommandId = commandId ?? string.Empty,
                RequestFingerprint = fingerprint ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = reason,
                Status = status,
                FailureCode = D20ActionInterruptFailureCode.None,
                ReasonCode = reasonCode ?? string.Empty,
                PreviousIntent = previous == null ? null : previous.Clone(),
                CurrentIntent = current == null ? null : current.Clone(),
                SuspendedIntent = suspended == null ? null : suspended.Clone(),
                IncomingIntent = incoming == null ? null : incoming.Clone(),
                FallbackKind = fallbackKind,
                FallbackActionId = fallbackId ?? string.Empty,
                PresenceState = _state.PresenceState,
                Availability = _state.Availability,
                IsPaused = _state.IsPaused,
                EventId = _state.LastEventId,
                WorldDay = _state.WorldDay,
                SaveRevision = _state.SaveRevision,
                PriorityDecision = decision == null ? null : decision.Clone(),
            };
            result.State = Snapshot;
            _receipts[commandId] = result.Clone();
            ReplaceReceipt(_state, result, fingerprint);
            return result;
        }

        private D20ActionInterruptResult Reject(string commandId, string actorId,
            D20ActionInterruptReason reason, D20ActionInterruptFailureCode code,
            string reasonCode)
        {
            return new D20ActionInterruptResult
            {
                Accepted = false,
                Duplicate = false,
                Changed = false,
                CommandId = commandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                Reason = reason,
                Status = code == D20ActionInterruptFailureCode.StateConflict &&
                    reasonCode == "action_layer_unavailable"
                    ? D20ActionIntentStatus.Unavailable
                    : D20ActionIntentStatus.Rejected,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                PresenceState = _state.PresenceState,
                Availability = _state.Availability,
                IsPaused = _state.IsPaused,
                CurrentIntent = _state.ActiveIntent == null ? null : _state.ActiveIntent.Clone(),
                SuspendedIntent = _state.SuspendedIntent == null ? null : _state.SuspendedIntent.Clone(),
                State = Snapshot,
            };
        }

        private D20ActionInterruptResult RejectAndRemember(string commandId, string actorId,
            D20ActionInterruptReason reason, D20ActionInterruptFailureCode code,
            string reasonCode, string fingerprint,
            D20ActionIntentStatus status = D20ActionIntentStatus.Rejected,
            D20ActionPriorityDecision decision = null)
        {
            var result = Reject(commandId, actorId, reason, code, reasonCode);
            result.RequestFingerprint = fingerprint ?? string.Empty;
            result.Status = status;
            result.PriorityDecision = decision == null ? null : decision.Clone();
            if (!string.IsNullOrEmpty(commandId))
            {
                _receipts[commandId] = result.Clone();
                ReplaceReceipt(_state, result, fingerprint);
            }
            return result;
        }

        private D20ActionInterruptResult FindDuplicate(string commandId, string fingerprint)
        {
            if (string.IsNullOrEmpty(commandId)) return null;
            D20ActionInterruptResult previous;
            if (!_receipts.TryGetValue(commandId, out previous)) return null;
            if (string.Equals(fingerprint, FingerprintFor(previous), StringComparison.Ordinal))
            {
                var duplicate = previous.Clone();
                duplicate.Duplicate = true;
                duplicate.FailureCode = D20ActionInterruptFailureCode.DuplicateCommand;
                duplicate.ReasonCode = "duplicate_command";
                return duplicate;
            }
            var conflict = Reject(commandId, previous.ActorId, previous.Reason,
                D20ActionInterruptFailureCode.DuplicateCommandConflict,
                "duplicate_command_conflict");
            conflict.Duplicate = false;
            return conflict;
        }

        private bool ValidateRequest(D20ActionInterruptRequest request,
            out D20ActionInterruptFailureCode code, out string reasonCode)
        {
            code = D20ActionInterruptFailureCode.None;
            reasonCode = string.Empty;
            if (request.SchemaVersion != D20ActionInterruptRequest.CurrentSchemaVersion)
            {
                code = D20ActionInterruptFailureCode.InvalidSchemaVersion;
                reasonCode = "schema_version_mismatch";
                return false;
            }
            if (!D20CharacterIdentityContracts.IsStableCharacterId(request.ActorId))
            {
                code = D20ActionInterruptFailureCode.MissingActorId;
                reasonCode = "missing_actor_id";
                return false;
            }
            if (string.IsNullOrEmpty(request.CommandId) ||
                request.CommandId.Trim() != request.CommandId)
            {
                code = D20ActionInterruptFailureCode.MissingCommandId;
                reasonCode = "missing_command_id";
                return false;
            }
            if (request.WorldDay < 0 || request.SaveRevision < 0)
            {
                code = D20ActionInterruptFailureCode.InvariantViolation;
                reasonCode = "negative_revision";
                return false;
            }
            if (!Enum.IsDefined(typeof(D20ActionInterruptReason), request.Reason) ||
                request.Reason == D20ActionInterruptReason.None)
            {
                code = D20ActionInterruptFailureCode.InvalidReason;
                reasonCode = "invalid_interrupt_reason";
                return false;
            }
            if (request.Reason == D20ActionInterruptReason.Hit)
            {
                if (request.IncomingIntent == null ||
                    request.IncomingIntent.Kind != D20ActionIntentKind.Hit ||
                    !string.Equals(request.IncomingIntent.ActorId, request.ActorId,
                        StringComparison.Ordinal))
                {
                    code = D20ActionInterruptFailureCode.InvalidIntent;
                    reasonCode = "hit_intent_invalid";
                    return false;
                }
                D20ActionIntentValidationResult validation = request.IncomingIntent.Validate();
                if (!validation.Valid)
                {
                    code = D20ActionInterruptFailureCode.InvalidIntent;
                    reasonCode = validation.ReasonCode;
                    return false;
                }
            }
            if (request.Reason == D20ActionInterruptReason.PresenceChanged)
            {
                if (request.PresenceResult == null || !request.PresenceResult.Accepted ||
                    request.PresenceResult.Event == null ||
                    !string.Equals(request.PresenceResult.Event.PlayerId, request.ActorId,
                        StringComparison.Ordinal) ||
                    !D20PlayerPresenceSystem.IsSupportedState(
                        request.PresenceResult.Event.CurrentState))
                {
                    code = D20ActionInterruptFailureCode.InvalidPresenceResult;
                    reasonCode = "presence_result_invalid";
                    return false;
                }
            }
            if (request.Reason == D20ActionInterruptReason.CommandCancelled &&
                string.IsNullOrEmpty(request.TargetCommandId))
            {
                code = D20ActionInterruptFailureCode.CommandNotActive;
                reasonCode = "target_command_missing";
                return false;
            }
            if (request.Reason == D20ActionInterruptReason.Incapacitated ||
                request.Reason == D20ActionInterruptReason.AvailabilityChanged)
            {
                if (!Enum.IsDefined(typeof(D20CharacterAvailability), request.Availability) ||
                    request.Availability == D20CharacterAvailability.Unknown)
                {
                    code = D20ActionInterruptFailureCode.InvalidReason;
                    reasonCode = "availability_invalid";
                    return false;
                }
                if (request.Reason == D20ActionInterruptReason.Incapacitated &&
                    request.Availability != D20CharacterAvailability.Incapacitated)
                {
                    code = D20ActionInterruptFailureCode.InvalidReason;
                    reasonCode = "incapacitated_availability_mismatch";
                    return false;
                }
            }
            return true;
        }

        private bool BindOrValidateActor(string actorId,
            out D20ActionInterruptFailureCode code, out string reasonCode)
        {
            code = D20ActionInterruptFailureCode.None;
            reasonCode = string.Empty;
            if (!D20CharacterIdentityContracts.IsStableCharacterId(actorId))
            {
                code = D20ActionInterruptFailureCode.MissingActorId;
                reasonCode = "missing_actor_id";
                return false;
            }
            if (!string.IsNullOrEmpty(_state.ActorId) &&
                !string.Equals(_state.ActorId, actorId, StringComparison.Ordinal))
            {
                code = D20ActionInterruptFailureCode.ActorMismatch;
                reasonCode = "actor_mismatch";
                return false;
            }
            _state.ActorId = actorId;
            return true;
        }

        private static bool IsAvailableForAction(D20CharacterAvailability availability)
        {
            return availability == D20CharacterAvailability.Available ||
                   availability == D20CharacterAvailability.Busy;
        }

        private static bool IsPresenceCompatible(D20ActionIntent intent,
            D20PlayerPresenceState presence)
        {
            if (intent == null) return false;
            D20ActionPrerequisite prerequisites =
                D20ActionPriorityRules.GetPrerequisites(intent.Kind);
            D20ActionPrerequisite mask = prerequisites &
                (D20ActionPrerequisite.PresencePort |
                 D20ActionPrerequisite.PresenceShip |
                 D20ActionPrerequisite.PresenceVoyage);
            if (mask == D20ActionPrerequisite.None) return true;
            if (presence == D20PlayerPresenceState.Port &&
                (mask & D20ActionPrerequisite.PresencePort) != 0) return true;
            if (presence == D20PlayerPresenceState.Ship &&
                (mask & D20ActionPrerequisite.PresenceShip) != 0) return true;
            if (presence == D20PlayerPresenceState.Voyage &&
                (mask & D20ActionPrerequisite.PresenceVoyage) != 0) return true;
            return false;
        }

        private static string EventIdFor(string commandId, D20ActionInterruptReason reason)
        {
            return "d20_action_interrupt_" + reason.ToString().ToLowerInvariant() + "_" +
                (commandId ?? string.Empty);
        }

        private static string IntentFingerprint(D20ActionIntent intent)
        {
            if (intent == null) return string.Empty;
            var causes = string.Empty;
            if (intent.CauseEventIds != null)
                causes = string.Join(",", intent.CauseEventIds.ToArray());
            return intent.SchemaVersion + "|" + (intent.CommandId ?? string.Empty) + "|" +
                intent.Kind + "|" + (intent.ActionId ?? string.Empty) + "|" +
                (intent.ActorId ?? string.Empty) + "|" + (intent.TargetId ?? string.Empty) + "|" +
                (intent.LocationRef ?? string.Empty) + "|" + (intent.OwnerSystem ?? string.Empty) + "|" +
                intent.WorldDay + "|" + intent.SaveRevision + "|" + causes;
        }

        private static string RequestFingerprint(D20ActionInterruptRequest request)
        {
            if (request == null) return string.Empty;
            string incoming = IntentFingerprint(request.IncomingIntent);
            string presence = string.Empty;
            if (request.PresenceResult != null && request.PresenceResult.Event != null)
            {
                var presenceEvent = request.PresenceResult.Event;
                presence = (presenceEvent.EventId ?? string.Empty) + "|" +
                    presenceEvent.PreviousState + "|" + presenceEvent.CurrentState + "|" +
                    (presenceEvent.PlayerId ?? string.Empty);
            }
            string causes = request.CauseEventIds == null
                ? string.Empty : string.Join(",", request.CauseEventIds.ToArray());
            return request.SchemaVersion + "|" + (request.CommandId ?? string.Empty) + "|" +
                (request.ActorId ?? string.Empty) + "|" + request.Reason + "|" + incoming + "|" +
                presence + "|" + request.Availability + "|" +
                (request.TargetCommandId ?? string.Empty) + "|" + request.ResumeSuspendedAction + "|" +
                request.WorldDay + "|" + request.SaveRevision + "|" + causes;
        }

        private static string FingerprintFor(D20ActionInterruptResult result)
        {
            if (result == null) return string.Empty;
            if (!string.IsNullOrEmpty(result.RequestFingerprint))
                return result.RequestFingerprint;
            if (result.State == null) return string.Empty;
            for (int i = 0; i < result.State.CommandReceipts.Count; i++)
            {
                var receipt = result.State.CommandReceipts[i];
                if (receipt != null && string.Equals(receipt.CommandId, result.CommandId,
                    StringComparison.Ordinal))
                    return receipt.RequestFingerprint ?? string.Empty;
            }
            return string.Empty;
        }

        private static void ReplaceReceipt(D20ActionInterruptSnapshot state,
            D20ActionInterruptResult result, string fingerprint)
        {
            if (state.CommandReceipts == null)
                state.CommandReceipts = new List<D20ActionInterruptReceipt>();
            var next = new D20ActionInterruptReceipt
            {
                CommandId = result.CommandId ?? string.Empty,
                RequestFingerprint = fingerprint ?? string.Empty,
                Accepted = result.Accepted,
                ReasonCode = result.ReasonCode ?? string.Empty,
                EventId = result.EventId ?? string.Empty,
            };
            for (int i = 0; i < state.CommandReceipts.Count; i++)
            {
                if (state.CommandReceipts[i] != null &&
                    string.Equals(state.CommandReceipts[i].CommandId, next.CommandId,
                        StringComparison.Ordinal))
                {
                    state.CommandReceipts[i] = next;
                    return;
                }
            }
            state.CommandReceipts.Add(next);
        }

        private void RebuildReceiptIndex()
        {
            _receipts.Clear();
            if (_state.CommandReceipts == null) return;
            foreach (var receipt in _state.CommandReceipts)
            {
                if (receipt == null || string.IsNullOrEmpty(receipt.CommandId)) continue;
                _receipts[receipt.CommandId] = new D20ActionInterruptResult
                {
                    Accepted = receipt.Accepted,
                    CommandId = receipt.CommandId,
                    RequestFingerprint = receipt.RequestFingerprint ?? string.Empty,
                    ActorId = _state.ActorId,
                    ReasonCode = receipt.ReasonCode ?? string.Empty,
                    EventId = receipt.EventId ?? string.Empty,
                    State = _state.Clone(),
                };
            }
        }

        private static void CopyInto(D20ActionInterruptSnapshot target,
            D20ActionInterruptSnapshot source)
        {
            target.SchemaVersion = source.SchemaVersion;
            target.ActorId = source.ActorId ?? string.Empty;
            target.PresenceState = source.PresenceState;
            target.Availability = source.Availability;
            target.IsPaused = source.IsPaused;
            target.ActiveIntent = source.ActiveIntent == null ? null : source.ActiveIntent.Clone();
            target.SuspendedIntent = source.SuspendedIntent == null ? null : source.SuspendedIntent.Clone();
            target.LastIncomingIntent = source.LastIncomingIntent == null
                ? null : source.LastIncomingIntent.Clone();
            target.FallbackKind = source.FallbackKind;
            target.FallbackActionId = source.FallbackActionId ?? string.Empty;
            target.ActiveStatus = source.ActiveStatus;
            target.LastCommandId = source.LastCommandId ?? string.Empty;
            target.LastEventId = source.LastEventId ?? string.Empty;
            target.LastReasonCode = source.LastReasonCode ?? string.Empty;
            target.WorldDay = source.WorldDay;
            target.SaveRevision = source.SaveRevision;
            target.CommandReceipts = new List<D20ActionInterruptReceipt>();
            if (source.CommandReceipts != null)
                foreach (var receipt in source.CommandReceipts)
                    if (receipt != null) target.CommandReceipts.Add(receipt.Clone());
        }
    }
}
