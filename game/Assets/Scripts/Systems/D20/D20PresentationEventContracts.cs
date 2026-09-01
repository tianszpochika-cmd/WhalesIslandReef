// ============================================================
// D20PresentationEventContracts —— D20-L07 逻辑到表现事件（纯 C#）
//
// 事件是逻辑结果的不可变只读投影。事件消费者可以将其转换为提示、音频
// 或动作表现，但本文件不提供任何事实写入口，也不保存运行时对象。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    /// <summary>Presentation 事件类型。</summary>
    public enum D20PresentationEventKind
    {
        /// <summary>未定义事件。</summary>
        None = 0,
        /// <summary>动作已经进入活动投影。</summary>
        ActionStarted = 1,
        /// <summary>动作由 owner 结果完成。</summary>
        ActionCompleted = 2,
        /// <summary>动作被命令取消。</summary>
        ActionCancelled = 3,
        /// <summary>动作被另一逻辑输入打断。</summary>
        ActionInterrupted = 4,
        /// <summary>动作回退到安全状态。</summary>
        ActionFallback = 5,
        /// <summary>逻辑暂停或恢复。</summary>
        PauseChanged = 6,
        /// <summary>语义 presence 已改变。</summary>
        PresenceChanged = 7,
        /// <summary>可用性语义已改变。</summary>
        AvailabilityChanged = 8,
        /// <summary>逻辑结果需要表现层显示反馈。</summary>
        Feedback = 9,
    }

    /// <summary>表现事件严重度；不代表业务结果成功与否。</summary>
    public enum D20PresentationEventSeverity
    {
        /// <summary>普通信息。</summary>
        Info = 0,
        /// <summary>需要用户注意。</summary>
        Warning = 1,
        /// <summary>角色暂时不能继续动作。</summary>
        Critical = 2,
    }

    /// <summary>
    /// 不可变 Presentation 事件。只有构造和克隆，不提供修改逻辑事实的方法。
    /// </summary>
    [Serializable]
    public sealed class D20PresentationEvent
    {
        private readonly int _schemaVersion;
        private readonly string _eventId;
        private readonly string _sourceOwnerSystem;
        private readonly string _sourceCommandId;
        private readonly string _actorId;
        private readonly D20PresentationEventKind _kind;
        private readonly D20PresentationEventSeverity _severity;
        private readonly D20ActionIntentKind _actionKind;
        private readonly D20ActionIntentStatus _actionStatus;
        private readonly D20PlayerPresenceState _presenceState;
        private readonly D20CharacterAvailability _availability;
        private readonly string _actionId;
        private readonly string _targetId;
        private readonly string _locationRef;
        private readonly string _previousActionId;
        private readonly string _currentActionId;
        private readonly string _fallbackActionId;
        private readonly D20ActionFallbackKind _fallbackKind;
        private readonly string _reasonCode;
        private readonly string _localizationKey;
        private readonly bool _isPaused;
        private readonly int _worldDay;
        private readonly long _saveRevision;
        private readonly ReadOnlyCollection<string> _causeEventIds;

        /// <summary>创建不可变事件；所有集合均被复制。</summary>
        public D20PresentationEvent(
            int schemaVersion,
            string eventId,
            string sourceOwnerSystem,
            string sourceCommandId,
            string actorId,
            D20PresentationEventKind kind,
            D20PresentationEventSeverity severity,
            D20ActionIntentKind actionKind,
            D20ActionIntentStatus actionStatus,
            D20PlayerPresenceState presenceState,
            D20CharacterAvailability availability,
            string actionId,
            string targetId,
            string locationRef,
            string previousActionId,
            string currentActionId,
            string fallbackActionId,
            D20ActionFallbackKind fallbackKind,
            string reasonCode,
            string localizationKey,
            bool isPaused,
            int worldDay,
            long saveRevision,
            IEnumerable<string> causeEventIds)
        {
            _schemaVersion = schemaVersion;
            _eventId = eventId ?? string.Empty;
            _sourceOwnerSystem = sourceOwnerSystem ?? string.Empty;
            _sourceCommandId = sourceCommandId ?? string.Empty;
            _actorId = actorId ?? string.Empty;
            _kind = kind;
            _severity = severity;
            _actionKind = actionKind;
            _actionStatus = actionStatus;
            _presenceState = presenceState;
            _availability = availability;
            _actionId = actionId ?? string.Empty;
            _targetId = targetId ?? string.Empty;
            _locationRef = locationRef ?? string.Empty;
            _previousActionId = previousActionId ?? string.Empty;
            _currentActionId = currentActionId ?? string.Empty;
            _fallbackActionId = fallbackActionId ?? string.Empty;
            _fallbackKind = fallbackKind;
            _reasonCode = reasonCode ?? string.Empty;
            _localizationKey = localizationKey ?? string.Empty;
            _isPaused = isPaused;
            _worldDay = worldDay;
            _saveRevision = saveRevision;
            var causes = new List<string>();
            if (causeEventIds != null)
                foreach (var causeEventId in causeEventIds)
                    if (causeEventId != null) causes.Add(causeEventId);
            _causeEventIds = new ReadOnlyCollection<string>(causes);
        }

        /// <summary>契约版本。</summary>
        public int SchemaVersion { get { return _schemaVersion; } }
        /// <summary>事件稳定 ID。</summary>
        public string EventId { get { return _eventId; } }
        /// <summary>产生逻辑结果的 owner。</summary>
        public string SourceOwnerSystem { get { return _sourceOwnerSystem; } }
        /// <summary>关联命令 ID。</summary>
        public string SourceCommandId { get { return _sourceCommandId; } }
        /// <summary>行为者稳定 ID。</summary>
        public string ActorId { get { return _actorId; } }
        /// <summary>事件类型。</summary>
        public D20PresentationEventKind Kind { get { return _kind; } }
        /// <summary>表现严重度。</summary>
        public D20PresentationEventSeverity Severity { get { return _severity; } }
        /// <summary>关联动作类型。</summary>
        public D20ActionIntentKind ActionKind { get { return _actionKind; } }
        /// <summary>关联动作结果状态。</summary>
        public D20ActionIntentStatus ActionStatus { get { return _actionStatus; } }
        /// <summary>事件发生时的 presence。</summary>
        public D20PlayerPresenceState PresenceState { get { return _presenceState; } }
        /// <summary>事件发生时的可用性。</summary>
        public D20CharacterAvailability Availability { get { return _availability; } }
        /// <summary>动作稳定 ID。</summary>
        public string ActionId { get { return _actionId; } }
        /// <summary>目标稳定 ID。</summary>
        public string TargetId { get { return _targetId; } }
        /// <summary>逻辑位置引用。</summary>
        public string LocationRef { get { return _locationRef; } }
        /// <summary>事件前动作稳定 ID。</summary>
        public string PreviousActionId { get { return _previousActionId; } }
        /// <summary>事件后动作稳定 ID。</summary>
        public string CurrentActionId { get { return _currentActionId; } }
        /// <summary>回退稳定 ID。</summary>
        public string FallbackActionId { get { return _fallbackActionId; } }
        /// <summary>回退类别。</summary>
        public D20ActionFallbackKind FallbackKind { get { return _fallbackKind; } }
        /// <summary>机器可读原因码。</summary>
        public string ReasonCode { get { return _reasonCode; } }
        /// <summary>表现端本地化 key。</summary>
        public string LocalizationKey { get { return _localizationKey; } }
        /// <summary>是否处于逻辑暂停。</summary>
        public bool IsPaused { get { return _isPaused; } }
        /// <summary>世界日。</summary>
        public int WorldDay { get { return _worldDay; } }
        /// <summary>逻辑存档修订。</summary>
        public long SaveRevision { get { return _saveRevision; } }
        /// <summary>因果事件 ID 的不可变视图。</summary>
        public IReadOnlyList<string> CauseEventIds { get { return _causeEventIds; } }
        /// <summary>恒为 true，明确该对象没有事实写入口。</summary>
        public bool IsReadOnly { get { return true; } }

        /// <summary>返回值语义相同的不可变副本。</summary>
        public D20PresentationEvent Clone()
        {
            return new D20PresentationEvent(
                _schemaVersion, _eventId, _sourceOwnerSystem, _sourceCommandId, _actorId,
                _kind, _severity, _actionKind, _actionStatus, _presenceState, _availability,
                _actionId, _targetId, _locationRef, _previousActionId, _currentActionId,
                _fallbackActionId, _fallbackKind, _reasonCode, _localizationKey, _isPaused,
                _worldDay, _saveRevision, _causeEventIds);
        }

        /// <summary>校验事件是否适合交给表现消费者。</summary>
        public void Validate()
        {
            D20PresentationEventContracts.Validate(this);
        }
    }

    /// <summary>D20-L07 表现事件构造和只读契约校验。</summary>
    public static class D20PresentationEventContracts
    {
        /// <summary>当前表现事件契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>表现事件契约 owner 标识。</summary>
        public const string OwnerSystem = "D20PresentationEventContracts";

        /// <summary>从动作层回执生成只读表现事件。</summary>
        public static bool TryCreateFromActionResult(D20ActionInterruptResult result,
            out D20PresentationEvent presentationEvent, out string reasonCode)
        {
            presentationEvent = null;
            reasonCode = string.Empty;
            if (result == null)
            {
                reasonCode = "presentation_result_missing";
                return false;
            }
            if (!D20CharacterIdentityContracts.IsStableCharacterId(result.ActorId))
            {
                reasonCode = "presentation_actor_missing";
                return false;
            }
            if (!IsStableReference(result.CommandId, 160))
            {
                reasonCode = "presentation_command_missing";
                return false;
            }
            if (result.Duplicate)
            {
                reasonCode = "presentation_duplicate_suppressed";
                return false;
            }

            D20PresentationEventKind kind = KindFor(result);
            D20PresentationEventSeverity severity = SeverityFor(result);
            D20ActionIntent action = result.CurrentIntent ?? result.PreviousIntent ??
                result.IncomingIntent;
            string actionId = action == null ? string.Empty : action.ActionId ?? string.Empty;
            D20ActionIntentKind actionKind = action == null
                ? D20ActionIntentKind.None : action.Kind;
            string targetId = action == null ? string.Empty : action.TargetId ?? string.Empty;
            string locationRef = action == null ? string.Empty : action.LocationRef ?? string.Empty;
            string eventId = string.IsNullOrEmpty(result.EventId)
                ? "d20_presentation_" + (result.CommandId ?? string.Empty)
                : "d20_presentation_" + result.EventId;
            presentationEvent = new D20PresentationEvent(
                CurrentSchemaVersion,
                eventId,
                D20ActionInterruptSystem.OwnerSystem,
                result.CommandId,
                result.ActorId,
                kind,
                severity,
                actionKind,
                result.Status,
                result.PresenceState,
                result.Availability,
                actionId,
                targetId,
                locationRef,
                result.PreviousIntent == null ? string.Empty :
                    result.PreviousIntent.ActionId,
                result.CurrentIntent == null ? string.Empty :
                    result.CurrentIntent.ActionId,
                result.FallbackActionId,
                result.FallbackKind,
                result.ReasonCode,
                LocalizationKeyFor(kind),
                result.IsPaused,
                result.WorldDay,
                result.SaveRevision,
                result.IncomingIntent == null ? null : result.IncomingIntent.CauseEventIds);
            try
            {
                presentationEvent.Validate();
                return true;
            }
            catch (InvalidOperationException)
            {
                presentationEvent = null;
                reasonCode = "presentation_event_invalid";
                return false;
            }
        }

        /// <summary>从 presence owner 的已裁决结果生成只读表现事件。</summary>
        public static bool TryCreateFromPresenceResult(D20PlayerPresenceResult result,
            out D20PresentationEvent presentationEvent, out string reasonCode)
        {
            presentationEvent = null;
            reasonCode = string.Empty;
            if (result == null || !result.Accepted || result.Event == null)
            {
                reasonCode = "presence_result_missing";
                return false;
            }
            var source = result.Event;
            if (!D20CharacterIdentityContracts.IsStableCharacterId(source.PlayerId) ||
                !D20PlayerPresenceSystem.IsSupportedState(source.CurrentState) ||
                !IsStableReference(source.EventId, 256) ||
                !IsStableReference(source.CommandId, 160))
            {
                reasonCode = "presence_event_invalid";
                return false;
            }
            presentationEvent = new D20PresentationEvent(
                CurrentSchemaVersion,
                "d20_presentation_" + (source.EventId ?? string.Empty),
                D20PlayerPresenceSystem.OwnerSystem,
                source.CommandId,
                source.PlayerId,
                D20PresentationEventKind.PresenceChanged,
                D20PresentationEventSeverity.Info,
                D20ActionIntentKind.None,
                D20ActionIntentStatus.Succeeded,
                source.CurrentState,
                D20CharacterAvailability.Available,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                D20ActionFallbackKind.None,
                "presence_changed",
                LocalizationKeyFor(D20PresentationEventKind.PresenceChanged),
                false,
                source.WorldDay,
                source.SaveRevision,
                source.CauseEventIds);
            try
            {
                presentationEvent.Validate();
                return true;
            }
            catch (InvalidOperationException)
            {
                presentationEvent = null;
                reasonCode = "presence_event_invalid";
                return false;
            }
        }

        /// <summary>校验只读事件的稳定字段和枚举范围。</summary>
        public static void Validate(D20PresentationEvent presentationEvent)
        {
            if (presentationEvent == null)
                throw new ArgumentNullException("presentationEvent");
            if (presentationEvent.SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D20 presentation event schema_version mismatch.");
            if (!IsStableReference(presentationEvent.EventId, 256))
                throw new InvalidOperationException("D20 presentation event_id is missing or unstable.");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(presentationEvent.ActorId))
                throw new InvalidOperationException("D20 presentation actor_id is missing or unstable.");
            if (string.IsNullOrEmpty(presentationEvent.SourceOwnerSystem) ||
                string.IsNullOrEmpty(presentationEvent.ReasonCode) ||
                string.IsNullOrEmpty(presentationEvent.LocalizationKey))
                throw new InvalidOperationException("D20 presentation source/reason/localization fields are required.");
            if (!Enum.IsDefined(typeof(D20PresentationEventKind), presentationEvent.Kind) ||
                presentationEvent.Kind == D20PresentationEventKind.None)
                throw new InvalidOperationException("D20 presentation event kind is unknown.");
            if (!Enum.IsDefined(typeof(D20PresentationEventSeverity), presentationEvent.Severity))
                throw new InvalidOperationException("D20 presentation event severity is unknown.");
            if (presentationEvent.WorldDay < 0 || presentationEvent.SaveRevision < 0)
                throw new InvalidOperationException("D20 presentation event revision is negative.");
            if (presentationEvent.ActionKind != D20ActionIntentKind.None &&
                !D20ActionIntentContracts.IsSupported(presentationEvent.ActionKind))
                throw new InvalidOperationException("D20 presentation action_kind is unknown.");
            if (presentationEvent.CauseEventIds == null)
                throw new InvalidOperationException("D20 presentation cause_event_ids are required.");
            foreach (var causeEventId in presentationEvent.CauseEventIds)
            {
                if (!IsStableReference(causeEventId, 160))
                    throw new InvalidOperationException("D20 presentation cause_event_id is unstable.");
            }
            if (!presentationEvent.IsReadOnly)
                throw new InvalidOperationException("D20 presentation event must remain read-only.");
        }

        /// <summary>返回稳定的表现本地化 key。</summary>
        public static string LocalizationKeyFor(D20PresentationEventKind kind)
        {
            if (!Enum.IsDefined(typeof(D20PresentationEventKind), kind) ||
                kind == D20PresentationEventKind.None)
                return string.Empty;
            return "d20.presentation." + kind.ToString().ToLowerInvariant();
        }

        private static D20PresentationEventKind KindFor(D20ActionInterruptResult result)
        {
            if (result.Reason == D20ActionInterruptReason.Pause ||
                result.Reason == D20ActionInterruptReason.Resume)
                return D20PresentationEventKind.PauseChanged;
            if (result.Reason == D20ActionInterruptReason.PresenceChanged)
                return result.FallbackKind == D20ActionFallbackKind.None
                    ? D20PresentationEventKind.PresenceChanged
                    : D20PresentationEventKind.ActionFallback;
            if (result.FallbackKind != D20ActionFallbackKind.None)
                return D20PresentationEventKind.ActionFallback;
            if (result.Reason == D20ActionInterruptReason.Hit)
                return D20PresentationEventKind.ActionInterrupted;
            if (result.Reason == D20ActionInterruptReason.CommandCancelled)
                return D20PresentationEventKind.ActionCancelled;
            if (result.Reason == D20ActionInterruptReason.Completed)
                return D20PresentationEventKind.ActionCompleted;
            if (result.Reason == D20ActionInterruptReason.AvailabilityChanged ||
                result.Reason == D20ActionInterruptReason.Incapacitated)
                return D20PresentationEventKind.AvailabilityChanged;
            if (result.Accepted && result.CurrentIntent != null)
                return D20PresentationEventKind.ActionStarted;
            return D20PresentationEventKind.Feedback;
        }

        private static D20PresentationEventSeverity SeverityFor(D20ActionInterruptResult result)
        {
            if (result.Status == D20ActionIntentStatus.Unavailable ||
                result.Reason == D20ActionInterruptReason.Incapacitated)
                return D20PresentationEventSeverity.Critical;
            if (!result.Accepted || result.FallbackKind != D20ActionFallbackKind.None)
                return D20PresentationEventSeverity.Warning;
            return D20PresentationEventSeverity.Info;
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }
    }
}
