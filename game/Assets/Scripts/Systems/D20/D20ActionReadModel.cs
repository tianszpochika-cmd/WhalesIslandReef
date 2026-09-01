// ============================================================
// D20ActionReadModel —— D20-L08 动作只读读模型（纯 C#）
//
// 本文件把动作意图、优先级、动作层结果、回退和表现事件组合为不可变
// 读模型。它只复制公开契约值，消费方不能通过读模型反向修改任何事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    /// <summary>动作读模型的确定性状态。</summary>
    public enum D20ActionReadModelState
    {
        /// <summary>尚未绑定角色。</summary>
        Uninitialized = 0,
        /// <summary>没有活动动作。</summary>
        NoAction = 1,
        /// <summary>意图已通过本地校验，等待 owner 结果。</summary>
        Proposed = 2,
        /// <summary>动作处于活动投影。</summary>
        Active = 3,
        /// <summary>动作因暂停保留。</summary>
        Paused = 4,
        /// <summary>动作已完成。</summary>
        Completed = 5,
        /// <summary>动作已取消。</summary>
        Cancelled = 6,
        /// <summary>动作当前不可用。</summary>
        Unavailable = 7,
        /// <summary>动作回退到确定性安全状态。</summary>
        Fallback = 8,
        /// <summary>读模型输入无效，但仍可展示失败原因。</summary>
        Invalid = 9,
    }

    /// <summary>只读动作意图投影。</summary>
    [Serializable]
    public sealed class D20ActionIntentReadModel
    {
        private readonly string _commandId;
        private readonly string _actorId;
        private readonly string _actionId;
        private readonly D20ActionIntentKind _kind;
        private readonly string _targetId;
        private readonly string _locationRef;
        private readonly string _ownerSystem;
        private readonly D20ActionIntentStatus _status;
        private readonly D20ActionPriority _priority;
        private readonly D20ActionExclusiveGroup _exclusiveGroup;
        private readonly string _reasonCode;
        private readonly int _worldDay;
        private readonly long _saveRevision;

        internal D20ActionIntentReadModel(D20ActionIntent intent,
            D20ActionPriorityDecision decision, D20ActionIntentStatus status,
            string reasonCode)
        {
            _commandId = intent == null ? string.Empty : intent.CommandId ?? string.Empty;
            _actorId = intent == null ? string.Empty : intent.ActorId ?? string.Empty;
            _actionId = intent == null ? string.Empty : intent.ActionId ?? string.Empty;
            _kind = intent == null ? D20ActionIntentKind.None : intent.Kind;
            _targetId = intent == null ? string.Empty : intent.TargetId ?? string.Empty;
            _locationRef = intent == null ? string.Empty : intent.LocationRef ?? string.Empty;
            _ownerSystem = intent == null ? string.Empty : intent.OwnerSystem ?? string.Empty;
            _status = status;
            _priority = decision == null ? D20ActionPriority.None : decision.Priority;
            _exclusiveGroup = decision == null
                ? D20ActionExclusiveGroup.None : decision.ExclusiveGroup;
            _reasonCode = reasonCode ?? string.Empty;
            _worldDay = intent == null ? 0 : Math.Max(0, intent.WorldDay);
            _saveRevision = intent == null ? 0 : Math.Max(0L, intent.SaveRevision);
        }

        /// <summary>命令 ID。</summary>
        public string CommandId { get { return _commandId; } }
        /// <summary>行为者 ID。</summary>
        public string ActorId { get { return _actorId; } }
        /// <summary>动作稳定 ID。</summary>
        public string ActionId { get { return _actionId; } }
        /// <summary>动作类型。</summary>
        public D20ActionIntentKind Kind { get { return _kind; } }
        /// <summary>目标 ID。</summary>
        public string TargetId { get { return _targetId; } }
        /// <summary>逻辑位置引用。</summary>
        public string LocationRef { get { return _locationRef; } }
        /// <summary>业务 owner 提示。</summary>
        public string OwnerSystem { get { return _ownerSystem; } }
        /// <summary>动作在结果链中的状态。</summary>
        public D20ActionIntentStatus Status { get { return _status; } }
        /// <summary>优先级。</summary>
        public D20ActionPriority Priority { get { return _priority; } }
        /// <summary>互斥集。</summary>
        public D20ActionExclusiveGroup ExclusiveGroup { get { return _exclusiveGroup; } }
        /// <summary>本地校验或 owner 结果原因。</summary>
        public string ReasonCode { get { return _reasonCode; } }
        /// <summary>世界日。</summary>
        public int WorldDay { get { return _worldDay; } }
        /// <summary>存档修订。</summary>
        public long SaveRevision { get { return _saveRevision; } }

        /// <summary>返回值语义相同的副本。</summary>
        public D20ActionIntentReadModel Clone()
        {
            return new D20ActionIntentReadModel(
                new D20ActionIntent
                {
                    CommandId = _commandId,
                    ActorId = _actorId,
                    ActionId = _actionId,
                    Kind = _kind,
                    TargetId = _targetId,
                    LocationRef = _locationRef,
                    OwnerSystem = _ownerSystem,
                    WorldDay = _worldDay,
                    SaveRevision = _saveRevision,
                },
                new D20ActionPriorityDecision
                {
                    Priority = _priority,
                    ExclusiveGroup = _exclusiveGroup,
                },
                _status,
                _reasonCode);
        }
    }

    /// <summary>只读动作层结果投影。</summary>
    [Serializable]
    public sealed class D20ActionResultReadModel
    {
        private readonly bool _accepted;
        private readonly bool _duplicate;
        private readonly bool _changed;
        private readonly string _commandId;
        private readonly D20ActionInterruptReason _reason;
        private readonly D20ActionIntentStatus _status;
        private readonly D20ActionInterruptFailureCode _failureCode;
        private readonly string _reasonCode;
        private readonly D20ActionFallbackKind _fallbackKind;
        private readonly string _fallbackActionId;
        private readonly string _eventId;
        private readonly bool _isPaused;
        private readonly D20PlayerPresenceState _presenceState;
        private readonly D20CharacterAvailability _availability;
        private readonly int _worldDay;
        private readonly long _saveRevision;

        internal D20ActionResultReadModel(D20ActionInterruptResult result)
        {
            _accepted = result != null && result.Accepted;
            _duplicate = result != null && result.Duplicate;
            _changed = result != null && result.Changed;
            _commandId = result == null ? string.Empty : result.CommandId ?? string.Empty;
            _reason = result == null ? D20ActionInterruptReason.None : result.Reason;
            _status = result == null ? D20ActionIntentStatus.Rejected : result.Status;
            _failureCode = result == null
                ? D20ActionInterruptFailureCode.MissingRequest : result.FailureCode;
            _reasonCode = result == null ? "result_missing" : result.ReasonCode ?? string.Empty;
            _fallbackKind = result == null ? D20ActionFallbackKind.None : result.FallbackKind;
            _fallbackActionId = result == null ? string.Empty : result.FallbackActionId ?? string.Empty;
            _eventId = result == null ? string.Empty : result.EventId ?? string.Empty;
            _isPaused = result != null && result.IsPaused;
            _presenceState = result == null ? D20PlayerPresenceState.Unknown : result.PresenceState;
            _availability = result == null ? D20CharacterAvailability.Unknown : result.Availability;
            _worldDay = result == null ? 0 : Math.Max(0, result.WorldDay);
            _saveRevision = result == null ? 0 : Math.Max(0L, result.SaveRevision);
        }

        /// <summary>命令是否被接受。</summary>
        public bool Accepted { get { return _accepted; } }
        /// <summary>是否为历史重复结果。</summary>
        public bool Duplicate { get { return _duplicate; } }
        /// <summary>动作投影是否变化。</summary>
        public bool Changed { get { return _changed; } }
        /// <summary>命令 ID。</summary>
        public string CommandId { get { return _commandId; } }
        /// <summary>结果原因。</summary>
        public D20ActionInterruptReason Reason { get { return _reason; } }
        /// <summary>动作状态。</summary>
        public D20ActionIntentStatus Status { get { return _status; } }
        /// <summary>失败码。</summary>
        public D20ActionInterruptFailureCode FailureCode { get { return _failureCode; } }
        /// <summary>稳定原因码。</summary>
        public string ReasonCode { get { return _reasonCode; } }
        /// <summary>回退类别。</summary>
        public D20ActionFallbackKind FallbackKind { get { return _fallbackKind; } }
        /// <summary>回退稳定 ID。</summary>
        public string FallbackActionId { get { return _fallbackActionId; } }
        /// <summary>结果事件 ID。</summary>
        public string EventId { get { return _eventId; } }
        /// <summary>是否暂停。</summary>
        public bool IsPaused { get { return _isPaused; } }
        /// <summary>presence。</summary>
        public D20PlayerPresenceState PresenceState { get { return _presenceState; } }
        /// <summary>可用性。</summary>
        public D20CharacterAvailability Availability { get { return _availability; } }
        /// <summary>世界日。</summary>
        public int WorldDay { get { return _worldDay; } }
        /// <summary>存档修订。</summary>
        public long SaveRevision { get { return _saveRevision; } }

        /// <summary>返回值语义相同的副本。</summary>
        public D20ActionResultReadModel Clone()
        {
            var result = new D20ActionInterruptResult
            {
                Accepted = _accepted,
                Duplicate = _duplicate,
                Changed = _changed,
                CommandId = _commandId,
                Reason = _reason,
                Status = _status,
                FailureCode = _failureCode,
                ReasonCode = _reasonCode,
                FallbackKind = _fallbackKind,
                FallbackActionId = _fallbackActionId,
                EventId = _eventId,
                IsPaused = _isPaused,
                PresenceState = _presenceState,
                Availability = _availability,
                WorldDay = _worldDay,
                SaveRevision = _saveRevision,
            };
            return new D20ActionResultReadModel(result);
        }
    }

    /// <summary>动作、结果、回退和表现事件的不可变组合读模型。</summary>
    [Serializable]
    public sealed class D20ActionReadModel
    {
        /// <summary>当前读模型版本。</summary>
        public const int CurrentSchemaVersion = 1;

        private readonly int _schemaVersion;
        private readonly string _actorId;
        private readonly D20ActionReadModelState _state;
        private readonly D20ActionIntentReadModel _intent;
        private readonly D20ActionResultReadModel _result;
        private readonly D20PresentationEvent _presentationEvent;
        private readonly D20ActionIntentKind _actionKind;
        private readonly string _actionId;
        private readonly string _commandId;
        private readonly string _targetId;
        private readonly string _locationRef;
        private readonly string _ownerSystem;
        private readonly D20ActionPriority _priority;
        private readonly D20ActionExclusiveGroup _exclusiveGroup;
        private readonly D20ActionIntentStatus _intentStatus;
        private readonly D20ActionFallbackKind _fallbackKind;
        private readonly string _fallbackActionId;
        private readonly string _reasonCode;
        private readonly string _localizationKey;
        private readonly D20PlayerPresenceState _presenceState;
        private readonly D20CharacterAvailability _availability;
        private readonly bool _isActive;
        private readonly bool _isPaused;
        private readonly bool _isFallback;
        private readonly bool _isAvailable;
        private readonly string _eventId;
        private readonly int _worldDay;
        private readonly long _saveRevision;

        private D20ActionReadModel(int schemaVersion, string actorId,
            D20ActionReadModelState state, D20ActionIntentReadModel intent,
            D20ActionResultReadModel result, D20PresentationEvent presentationEvent,
            D20ActionIntentKind actionKind, string actionId, string commandId,
            string targetId, string locationRef, string ownerSystem,
            D20ActionPriority priority, D20ActionExclusiveGroup exclusiveGroup,
            D20ActionIntentStatus intentStatus, D20ActionFallbackKind fallbackKind,
            string fallbackActionId, string reasonCode, string localizationKey,
            D20PlayerPresenceState presenceState, D20CharacterAvailability availability,
            bool isActive, bool isPaused, bool isFallback, bool isAvailable,
            string eventId, int worldDay, long saveRevision)
        {
            _schemaVersion = schemaVersion;
            _actorId = actorId ?? string.Empty;
            _state = state;
            _intent = intent == null ? null : intent.Clone();
            _result = result == null ? null : result.Clone();
            _presentationEvent = presentationEvent == null ? null : presentationEvent.Clone();
            _actionKind = actionKind;
            _actionId = actionId ?? string.Empty;
            _commandId = commandId ?? string.Empty;
            _targetId = targetId ?? string.Empty;
            _locationRef = locationRef ?? string.Empty;
            _ownerSystem = ownerSystem ?? string.Empty;
            _priority = priority;
            _exclusiveGroup = exclusiveGroup;
            _intentStatus = intentStatus;
            _fallbackKind = fallbackKind;
            _fallbackActionId = fallbackActionId ?? string.Empty;
            _reasonCode = reasonCode ?? string.Empty;
            _localizationKey = localizationKey ?? string.Empty;
            _presenceState = presenceState;
            _availability = availability;
            _isActive = isActive;
            _isPaused = isPaused;
            _isFallback = isFallback;
            _isAvailable = isAvailable;
            _eventId = eventId ?? string.Empty;
            _worldDay = worldDay;
            _saveRevision = saveRevision;
        }

        /// <summary>读模型版本。</summary>
        public int SchemaVersion { get { return _schemaVersion; } }
        /// <summary>行为者 ID。</summary>
        public string ActorId { get { return _actorId; } }
        /// <summary>组合状态。</summary>
        public D20ActionReadModelState State { get { return _state; } }
        /// <summary>意图子模型。</summary>
        public D20ActionIntentReadModel Intent { get { return _intent == null ? null : _intent.Clone(); } }
        /// <summary>结果子模型。</summary>
        public D20ActionResultReadModel Result { get { return _result == null ? null : _result.Clone(); } }
        /// <summary>只读表现事件。</summary>
        public D20PresentationEvent PresentationEvent
        {
            get { return _presentationEvent == null ? null : _presentationEvent.Clone(); }
        }
        /// <summary>动作类型。</summary>
        public D20ActionIntentKind ActionKind { get { return _actionKind; } }
        /// <summary>动作 ID。</summary>
        public string ActionId { get { return _actionId; } }
        /// <summary>命令 ID。</summary>
        public string CommandId { get { return _commandId; } }
        /// <summary>目标 ID。</summary>
        public string TargetId { get { return _targetId; } }
        /// <summary>逻辑位置引用。</summary>
        public string LocationRef { get { return _locationRef; } }
        /// <summary>业务 owner。</summary>
        public string OwnerSystem { get { return _ownerSystem; } }
        /// <summary>动作优先级。</summary>
        public D20ActionPriority Priority { get { return _priority; } }
        /// <summary>动作互斥集。</summary>
        public D20ActionExclusiveGroup ExclusiveGroup { get { return _exclusiveGroup; } }
        /// <summary>动作状态。</summary>
        public D20ActionIntentStatus IntentStatus { get { return _intentStatus; } }
        /// <summary>回退类别。</summary>
        public D20ActionFallbackKind FallbackKind { get { return _fallbackKind; } }
        /// <summary>回退 ID。</summary>
        public string FallbackActionId { get { return _fallbackActionId; } }
        /// <summary>稳定原因码。</summary>
        public string ReasonCode { get { return _reasonCode; } }
        /// <summary>表现端本地化 key。</summary>
        public string LocalizationKey { get { return _localizationKey; } }
        /// <summary>presence。</summary>
        public D20PlayerPresenceState PresenceState { get { return _presenceState; } }
        /// <summary>可用性。</summary>
        public D20CharacterAvailability Availability { get { return _availability; } }
        /// <summary>是否有活动动作。</summary>
        public bool IsActive { get { return _isActive; } }
        /// <summary>是否暂停。</summary>
        public bool IsPaused { get { return _isPaused; } }
        /// <summary>是否回退。</summary>
        public bool IsFallback { get { return _isFallback; } }
        /// <summary>是否可继续接受动作。</summary>
        public bool IsAvailable { get { return _isAvailable; } }
        /// <summary>表现事件 ID。</summary>
        public string EventId { get { return _eventId; } }
        /// <summary>世界日。</summary>
        public int WorldDay { get { return _worldDay; } }
        /// <summary>存档修订。</summary>
        public long SaveRevision { get { return _saveRevision; } }

        /// <summary>从动作意图和 L05 判定建立待处理读模型。</summary>
        public static D20ActionReadModel FromIntent(D20ActionIntent intent,
            D20ActionPriorityContext context = null)
        {
            D20ActionPriorityDecision decision = D20ActionPriorityRules.Evaluate(intent, context);
            string reason = decision.ReasonCode ?? string.Empty;
            var intentModel = new D20ActionIntentReadModel(intent, decision,
                decision.Allowed ? D20ActionIntentStatus.Accepted : D20ActionIntentStatus.Rejected,
                reason);
            D20ActionIntentKind kind = intent == null ? D20ActionIntentKind.None : intent.Kind;
            string actorId = intent == null ? string.Empty : intent.ActorId;
            string actionId = intent == null ? string.Empty : intent.ActionId;
            return new D20ActionReadModel(
                CurrentSchemaVersion,
                actorId,
                decision.Allowed ? D20ActionReadModelState.Proposed : D20ActionReadModelState.Invalid,
                intentModel,
                null,
                null,
                kind,
                actionId,
                intent == null ? string.Empty : intent.CommandId,
                intent == null ? string.Empty : intent.TargetId,
                intent == null ? string.Empty : intent.LocationRef,
                intent == null ? string.Empty : intent.OwnerSystem,
                decision.Priority,
                decision.ExclusiveGroup,
                decision.Allowed ? D20ActionIntentStatus.Accepted : D20ActionIntentStatus.Rejected,
                D20ActionFallbackKind.None,
                string.Empty,
                reason,
                D20PresentationEventContracts.LocalizationKeyFor(
                    D20PresentationEventKind.Feedback),
                context == null ? D20PlayerPresenceState.Unknown : context.PresenceState,
                context == null || context.CharacterAvailable
                    ? D20CharacterAvailability.Available
                    : D20CharacterAvailability.Incapacitated,
                false,
                false,
                false,
                decision.Allowed,
                string.Empty,
                intent == null ? 0 : Math.Max(0, intent.WorldDay),
                intent == null ? 0 : Math.Max(0L, intent.SaveRevision));
        }

        /// <summary>从 L06 动作回执和可选 L07 事件建立结果读模型。</summary>
        public static D20ActionReadModel FromInterruptResult(D20ActionInterruptResult result,
            D20PresentationEvent presentationEvent = null)
        {
            if (result == null)
                return InvalidModel("result_missing", null, null);

            D20ActionIntent intent = result.CurrentIntent ?? result.PreviousIntent ??
                result.IncomingIntent;
            D20ActionPriorityDecision decision = result.PriorityDecision;
            var intentModel = intent == null ? null : new D20ActionIntentReadModel(
                intent, decision, result.Status, result.ReasonCode);
            D20ActionResultReadModel resultModel = new D20ActionResultReadModel(result);
            D20ActionReadModelState state = StateFor(result);
            string actionId = intent == null ? string.Empty : intent.ActionId ?? string.Empty;
            string commandId = result.CommandId ?? string.Empty;
            string targetId = intent == null ? string.Empty : intent.TargetId ?? string.Empty;
            string locationRef = intent == null ? string.Empty : intent.LocationRef ?? string.Empty;
            string owner = intent == null ? string.Empty : intent.OwnerSystem ?? string.Empty;
            D20ActionPriority priority = decision == null ? D20ActionPriority.None : decision.Priority;
            D20ActionExclusiveGroup group = decision == null
                ? D20ActionExclusiveGroup.None : decision.ExclusiveGroup;
            D20ActionIntentKind kind = intent == null
                ? D20ActionIntentKind.None : intent.Kind;
            string locKey = presentationEvent == null
                ? D20PresentationEventContracts.LocalizationKeyFor(
                    result.FallbackKind == D20ActionFallbackKind.None
                        ? D20PresentationEventKind.Feedback
                        : D20PresentationEventKind.ActionFallback)
                : presentationEvent.LocalizationKey;
            return new D20ActionReadModel(
                CurrentSchemaVersion,
                result.ActorId,
                state,
                intentModel,
                resultModel,
                presentationEvent,
                kind,
                actionId,
                commandId,
                targetId,
                locationRef,
                owner,
                priority,
                group,
                result.Status,
                result.FallbackKind,
                result.FallbackActionId,
                result.ReasonCode,
                locKey,
                result.PresenceState,
                result.Availability,
                result.CurrentIntent != null,
                result.IsPaused,
                result.FallbackKind != D20ActionFallbackKind.None,
                result.Status != D20ActionIntentStatus.Unavailable &&
                    result.Availability != D20CharacterAvailability.Incapacitated &&
                    result.Availability != D20CharacterAvailability.Dead &&
                    result.Availability != D20CharacterAvailability.Departed,
                result.EventId,
                Math.Max(0, result.WorldDay),
                Math.Max(0L, result.SaveRevision));
        }

        /// <summary>从 L06 快照建立当前只读状态。</summary>
        public static D20ActionReadModel FromSnapshot(D20ActionInterruptSnapshot snapshot,
            D20PresentationEvent presentationEvent = null)
        {
            if (snapshot == null)
                return InvalidModel("snapshot_missing", null, null);
            D20ActionIntent intent = snapshot.ActiveIntent ?? snapshot.SuspendedIntent;
            D20ActionPriorityDecision decision = intent == null ? null :
                D20ActionPriorityRules.Evaluate(intent, new D20ActionPriorityContext
                {
                    PresenceState = snapshot.PresenceState,
                    CharacterAvailable = snapshot.Availability == D20CharacterAvailability.Available ||
                        snapshot.Availability == D20CharacterAvailability.Busy,
                    OwnerPermission = true,
                    HasTarget = !string.IsNullOrEmpty(intent.TargetId),
                });
            var intentModel = intent == null ? null : new D20ActionIntentReadModel(
                intent, decision, snapshot.ActiveStatus, snapshot.LastReasonCode);
            D20ActionReadModelState state = snapshot.IsPaused
                ? D20ActionReadModelState.Paused
                : snapshot.FallbackKind != D20ActionFallbackKind.None
                    ? D20ActionReadModelState.Fallback
                    : snapshot.ActiveIntent != null
                        ? D20ActionReadModelState.Active
                        : string.IsNullOrEmpty(snapshot.ActorId)
                            ? D20ActionReadModelState.Uninitialized
                            : D20ActionReadModelState.NoAction;
            D20ActionIntentKind kind = intent == null ? D20ActionIntentKind.None : intent.Kind;
            return new D20ActionReadModel(
                CurrentSchemaVersion,
                snapshot.ActorId,
                state,
                intentModel,
                null,
                presentationEvent,
                kind,
                intent == null ? string.Empty : intent.ActionId,
                intent == null ? snapshot.LastCommandId : intent.CommandId,
                intent == null ? string.Empty : intent.TargetId,
                intent == null ? string.Empty : intent.LocationRef,
                intent == null ? string.Empty : intent.OwnerSystem,
                decision == null ? D20ActionPriority.None : decision.Priority,
                decision == null ? D20ActionExclusiveGroup.None : decision.ExclusiveGroup,
                snapshot.ActiveStatus,
                snapshot.FallbackKind,
                snapshot.FallbackActionId,
                snapshot.LastReasonCode,
                presentationEvent == null ? D20PresentationEventContracts.LocalizationKeyFor(
                    snapshot.FallbackKind == D20ActionFallbackKind.None
                        ? D20PresentationEventKind.Feedback
                        : D20PresentationEventKind.ActionFallback) : presentationEvent.LocalizationKey,
                snapshot.PresenceState,
                snapshot.Availability,
                snapshot.ActiveIntent != null,
                snapshot.IsPaused,
                snapshot.FallbackKind != D20ActionFallbackKind.None,
                snapshot.Availability == D20CharacterAvailability.Available ||
                    snapshot.Availability == D20CharacterAvailability.Busy,
                snapshot.LastEventId,
                Math.Max(0, snapshot.WorldDay),
                Math.Max(0L, snapshot.SaveRevision));
        }

        /// <summary>返回防御性副本。</summary>
        public D20ActionReadModel Clone()
        {
            return new D20ActionReadModel(
                _schemaVersion,
                _actorId,
                _state,
                _intent,
                _result,
                _presentationEvent,
                _actionKind,
                _actionId,
                _commandId,
                _targetId,
                _locationRef,
                _ownerSystem,
                _priority,
                _exclusiveGroup,
                _intentStatus,
                _fallbackKind,
                _fallbackActionId,
                _reasonCode,
                _localizationKey,
                _presenceState,
                _availability,
                _isActive,
                _isPaused,
                _isFallback,
                _isAvailable,
                _eventId,
                _worldDay,
                _saveRevision);
        }

        /// <summary>校验读模型边界；校验不会修改任何来源。</summary>
        public void Validate()
        {
            if (_schemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 action read model schema_version mismatch.");
            if (!string.IsNullOrEmpty(_actorId) &&
                !D20CharacterIdentityContracts.IsStableCharacterId(_actorId))
                throw new InvalidOperationException("D20 action read model actor_id is unstable.");
            if (!Enum.IsDefined(typeof(D20ActionReadModelState), _state))
                throw new InvalidOperationException("D20 action read model state is unknown.");
            if (_worldDay < 0 || _saveRevision < 0)
                throw new InvalidOperationException("D20 action read model revision is negative.");
            if (_presentationEvent != null) _presentationEvent.Validate();
            if (_intent != null && _intent.Kind != D20ActionIntentKind.None &&
                !D20ActionIntentContracts.IsSupported(_intent.Kind))
                throw new InvalidOperationException("D20 action read model intent kind is unknown.");
            if (_isFallback && _fallbackKind == D20ActionFallbackKind.None)
                throw new InvalidOperationException("D20 action read model fallback kind is missing.");
        }

        private static D20ActionReadModelState StateFor(D20ActionInterruptResult result)
        {
            if (!result.Accepted && result.Status == D20ActionIntentStatus.Unavailable)
                return D20ActionReadModelState.Unavailable;
            if (result.IsPaused) return D20ActionReadModelState.Paused;
            if (result.FallbackKind != D20ActionFallbackKind.None)
                return D20ActionReadModelState.Fallback;
            if (result.Status == D20ActionIntentStatus.Succeeded)
                return D20ActionReadModelState.Completed;
            if (result.Status == D20ActionIntentStatus.Cancelled)
                return D20ActionReadModelState.Cancelled;
            if (result.Status == D20ActionIntentStatus.Unavailable)
                return D20ActionReadModelState.Unavailable;
            if (result.CurrentIntent != null) return D20ActionReadModelState.Active;
            return result.Accepted ? D20ActionReadModelState.NoAction : D20ActionReadModelState.Invalid;
        }

        private static D20ActionReadModel InvalidModel(string reasonCode,
            D20ActionInterruptResult result, D20PresentationEvent presentationEvent)
        {
            var resultModel = result == null ? null : new D20ActionResultReadModel(result);
            return new D20ActionReadModel(
                CurrentSchemaVersion,
                result == null ? string.Empty : result.ActorId,
                D20ActionReadModelState.Invalid,
                null,
                resultModel,
                presentationEvent,
                D20ActionIntentKind.None,
                string.Empty,
                result == null ? string.Empty : result.CommandId,
                string.Empty,
                string.Empty,
                string.Empty,
                D20ActionPriority.None,
                D20ActionExclusiveGroup.None,
                D20ActionIntentStatus.Rejected,
                D20ActionFallbackKind.None,
                string.Empty,
                reasonCode,
                D20PresentationEventContracts.LocalizationKeyFor(D20PresentationEventKind.Feedback),
                D20PlayerPresenceState.Unknown,
                D20CharacterAvailability.Unknown,
                false,
                false,
                false,
                false,
                result == null ? string.Empty : result.EventId,
                result == null ? 0 : Math.Max(0, result.WorldDay),
                result == null ? 0 : Math.Max(0L, result.SaveRevision));
        }
    }

    /// <summary>便于下游调用的 L08 工厂别名。</summary>
    public static class D20ActionReadModelBuilder
    {
        /// <summary>从动作意图和优先级上下文建立读模型。</summary>
        public static D20ActionReadModel Build(D20ActionIntent intent,
            D20ActionPriorityContext context = null)
        {
            return D20ActionReadModel.FromIntent(intent, context);
        }

        /// <summary>从动作层结果与表现事件建立读模型。</summary>
        public static D20ActionReadModel Build(D20ActionInterruptResult result,
            D20PresentationEvent presentationEvent = null)
        {
            return D20ActionReadModel.FromInterruptResult(result, presentationEvent);
        }

        /// <summary>从动作层快照建立读模型。</summary>
        public static D20ActionReadModel Build(D20ActionInterruptSnapshot snapshot,
            D20PresentationEvent presentationEvent = null)
        {
            return D20ActionReadModel.FromSnapshot(snapshot, presentationEvent);
        }
    }
}
