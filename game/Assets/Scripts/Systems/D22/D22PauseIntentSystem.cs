// ============================================================
// D22PauseIntentSystem —— D22-L10 暂停/恢复意图与读模型（纯 C#）
//
// Runtime 只提交归一化意图和上下文快照；暂停状态在这里保持确定性，
// 实际游戏时钟、输入设备和表现层暂停由 Runtime/对应 owner 负责。
// ============================================================

using System;

namespace Whalebone.Systems.D22
{
    /// <summary>暂停系统接收的逻辑意图。</summary>
    public enum D22PauseIntentKind
    {
        None = 0,
        Pause = 1,
        PauseRequested = Pause,
        Resume = 2,
        ResumeRequested = Resume,
        Toggle = 3,
        ToggleRequested = Toggle,
    }

    /// <summary>暂停逻辑状态；不等同于游戏时钟的实际缩放值。</summary>
    public enum D22PauseState
    {
        Running = 0,
        Paused = 1,
    }

    /// <summary>暂停/恢复意图的稳定失败或无变化原因。</summary>
    public enum D22PauseFailureCode
    {
        None = 0,
        MissingIntent = 1,
        MissingContext = 2,
        InvalidSchemaVersion = 3,
        InvalidIntent = 4,
        SessionInactive = 5,
        ContextInvalid = 6,
        PauseNotAllowed = 7,
        ResumeNotAllowed = 8,
        UnpausableContext = 9,
        UnrecoverableError = 10,
        AlreadyPaused = 11,
        AlreadyRunning = 12,
        RevisionMismatch = 13,
    }

    /// <summary>由 Runtime 或上层逻辑归一化后提交的暂停意图。</summary>
    [Serializable]
    public sealed class D22PauseIntent
    {
        public D22PauseIntentKind Kind;
        public string CorrelationId = string.Empty;
        public string ActorId = string.Empty;
        public string ContextId = string.Empty;
        public int WorldDay;
        public long SaveRevision;

        /// <summary>创建暂停意图。</summary>
        public static D22PauseIntent Pause(
            string correlationId = "",
            string actorId = "",
            string contextId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return Create(D22PauseIntentKind.Pause, correlationId, actorId, contextId, worldDay, saveRevision);
        }

        /// <summary>创建恢复意图。</summary>
        public static D22PauseIntent Resume(
            string correlationId = "",
            string actorId = "",
            string contextId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return Create(D22PauseIntentKind.Resume, correlationId, actorId, contextId, worldDay, saveRevision);
        }

        /// <summary>创建切换意图。</summary>
        public static D22PauseIntent Toggle(
            string correlationId = "",
            string actorId = "",
            string contextId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return Create(D22PauseIntentKind.Toggle, correlationId, actorId, contextId, worldDay, saveRevision);
        }

        /// <summary>创建任意暂停意图。</summary>
        public static D22PauseIntent Create(
            D22PauseIntentKind kind,
            string correlationId = "",
            string actorId = "",
            string contextId = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return new D22PauseIntent
            {
                Kind = kind,
                CorrelationId = correlationId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                ContextId = contextId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D22PauseIntent Clone()
        {
            return new D22PauseIntent
            {
                Kind = Kind,
                CorrelationId = CorrelationId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                ContextId = ContextId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>暂停前置上下文；所有不可暂停原因都必须由调用方显式提供。</summary>
    [Serializable]
    public sealed class D22PauseContext
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string ContextId = string.Empty;
        public string ActiveScreenId = string.Empty;
        public string ActivePanelId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public bool SessionActive = true;
        public bool ContextValid = true;
        public bool PauseAllowed = true;
        public bool ResumeAllowed = true;
        public bool IsLoading;
        public bool IsSaving;
        public bool IsMigrating;
        public bool IsRecovering;
        public bool IsUnrecoverableError;
        public string UnpausableReasonCode = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22PauseContext Clone()
        {
            return new D22PauseContext
            {
                SchemaVersion = SchemaVersion,
                ContextId = ContextId ?? string.Empty,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                SessionActive = SessionActive,
                ContextValid = ContextValid,
                PauseAllowed = PauseAllowed,
                ResumeAllowed = ResumeAllowed,
                IsLoading = IsLoading,
                IsSaving = IsSaving,
                IsMigrating = IsMigrating,
                IsRecovering = IsRecovering,
                IsUnrecoverableError = IsUnrecoverableError,
                UnpausableReasonCode = UnpausableReasonCode ?? string.Empty,
            };
        }

        /// <summary>上下文是否包含保存/加载/迁移等不可暂停阶段。</summary>
        public bool HasUnpausableOperation
        {
            get { return IsLoading || IsSaving || IsMigrating || IsRecovering; }
        }

        /// <summary>兼容 Runtime 读模型命名；写入仍只改变候选上下文副本。</summary>
        public bool CanPause
        {
            get { return PauseAllowed; }
            set { PauseAllowed = value; }
        }

        /// <summary>兼容 Runtime 读模型命名；写入仍只改变候选上下文副本。</summary>
        public bool CanResume
        {
            get { return ResumeAllowed; }
            set { ResumeAllowed = value; }
        }

        /// <summary>兼容更明确的不可暂停原因命名。</summary>
        public string UnpausableContextReasonCode
        {
            get { return UnpausableReasonCode; }
            set { UnpausableReasonCode = value ?? string.Empty; }
        }
    }

    /// <summary>暂停逻辑状态的只读快照。</summary>
    [Serializable]
    public sealed class D22PauseReadModel
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D22PauseState State = D22PauseState.Running;
        public bool IsPaused;
        public bool CanPause;
        public bool CanResume;
        public D22PauseIntentKind LastIntent;
        public D22PauseFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string FeedbackCode = string.Empty;
        public string CorrelationId = string.Empty;
        public string ContextId = string.Empty;
        public string UnpausableReasonCode = string.Empty;
        public int WorldDay;
        public long SaveRevision;

        /// <summary>返回防御性副本。</summary>
        public D22PauseReadModel Clone()
        {
            return new D22PauseReadModel
            {
                SchemaVersion = SchemaVersion,
                State = State,
                IsPaused = IsPaused,
                CanPause = CanPause,
                CanResume = CanResume,
                LastIntent = LastIntent,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                ContextId = ContextId ?? string.Empty,
                UnpausableReasonCode = UnpausableReasonCode ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>一次暂停意图的结果；ReadModel 是转换后的防御性状态。</summary>
    [Serializable]
    public sealed class D22PauseIntentResult
    {
        public int SchemaVersion = D22PauseReadModel.CurrentSchemaVersion;
        public bool Accepted;
        public bool Changed;
        public bool NoChange;
        public D22PauseState State = D22PauseState.Running;
        public D22PauseIntentKind IntentKind;
        public D22PauseFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string FeedbackCode = string.Empty;
        public string CorrelationId = string.Empty;
        public D22PauseReadModel ReadModel = new D22PauseReadModel();

        /// <summary>返回防御性副本。</summary>
        public D22PauseIntentResult Clone()
        {
            return new D22PauseIntentResult
            {
                SchemaVersion = SchemaVersion,
                Accepted = Accepted,
                Changed = Changed,
                NoChange = NoChange,
                State = State,
                IntentKind = IntentKind,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                ReadModel = ReadModel == null ? new D22PauseReadModel() : ReadModel.Clone(),
            };
        }
    }

    /// <summary>
    /// D22-L10 暂停/恢复意图系统。它只写自己的逻辑状态，不写时间缩放、设备或领域状态。
    /// </summary>
    public sealed class D22PauseIntentSystem
    {
        public const int CurrentSchemaVersion = D22PauseReadModel.CurrentSchemaVersion;
        public const string OwnerSystem = "D22PauseIntentSystem";

        private D22PauseState state;
        private D22PauseIntentKind lastIntent;
        private D22PauseReadModel lastReadModel = new D22PauseReadModel();

        /// <summary>创建运行中状态。</summary>
        public D22PauseIntentSystem() : this(false) { }

        /// <summary>以显式逻辑状态创建系统，不读取任何设备或时钟。</summary>
        public D22PauseIntentSystem(bool initiallyPaused)
        {
            state = initiallyPaused ? D22PauseState.Paused : D22PauseState.Running;
            lastReadModel.State = state;
            lastReadModel.IsPaused = initiallyPaused;
        }

        /// <summary>返回上一次处理后的暂停读模型副本。</summary>
        public D22PauseReadModel Snapshot()
        {
            return lastReadModel == null ? new D22PauseReadModel() : lastReadModel.Clone();
        }

        /// <summary>处理暂停、恢复或切换意图。</summary>
        public D22PauseIntentResult Resolve(D22PauseIntent intent, D22PauseContext context)
        {
            if (intent == null)
                return Reject(null, D22PauseFailureCode.MissingIntent, "d22.pause.missing_intent", D22FeedbackCodes.MissingContext);
            if (context == null)
                return Reject(intent, D22PauseFailureCode.MissingContext, "d22.pause.missing_context", D22FeedbackCodes.MissingContext);
            if (context.SchemaVersion != D22PauseContext.CurrentSchemaVersion)
                return Reject(intent, D22PauseFailureCode.InvalidSchemaVersion, "d22.pause.invalid_schema_version", D22FeedbackCodes.InvalidRequest, context);
            if (!context.SessionActive)
                return Reject(intent, D22PauseFailureCode.SessionInactive, "d22.pause.session_inactive", D22FeedbackCodes.PermissionDenied, context);
            if (!context.ContextValid)
                return Reject(intent, D22PauseFailureCode.ContextInvalid, "d22.pause.context_invalid", D22FeedbackCodes.ContextMismatch, context);
            if (!string.IsNullOrEmpty(context.ContextId) &&
                !string.Equals(context.ContextId, intent.ContextId ?? string.Empty, StringComparison.Ordinal))
                return Reject(intent, D22PauseFailureCode.ContextInvalid, "d22.pause.context_mismatch", D22FeedbackCodes.ContextMismatch, context);
            if (intent.WorldDay != context.WorldDay || intent.SaveRevision != context.SaveRevision)
                return Reject(intent, D22PauseFailureCode.RevisionMismatch, "d22.pause.revision_mismatch", D22FeedbackCodes.RevisionMismatch, context);

            D22PauseIntentKind effectiveIntent = intent.Kind;
            if (effectiveIntent == D22PauseIntentKind.Toggle)
                effectiveIntent = state == D22PauseState.Paused
                    ? D22PauseIntentKind.Resume
                    : D22PauseIntentKind.Pause;
            if (effectiveIntent != D22PauseIntentKind.Pause && effectiveIntent != D22PauseIntentKind.Resume)
                return Reject(intent, D22PauseFailureCode.InvalidIntent, "d22.pause.invalid_intent", D22FeedbackCodes.InvalidRequest, context);

            if (effectiveIntent == D22PauseIntentKind.Pause)
            {
                if (state == D22PauseState.Paused)
                    return NoChange(intent, D22PauseFailureCode.AlreadyPaused, "d22.pause.already_paused", D22FeedbackCodes.CommandAccepted, context);
                if (context.IsUnrecoverableError)
                    return Reject(intent, D22PauseFailureCode.UnrecoverableError, "d22.pause.unrecoverable_error", D22FeedbackCodes.OwnerUnavailable, context);
                if (context.HasUnpausableOperation || !context.PauseAllowed)
                {
                    string reason = string.IsNullOrEmpty(context.UnpausableReasonCode)
                        ? "d22.pause.unpausable_context"
                        : context.UnpausableReasonCode;
                    return Reject(intent, D22PauseFailureCode.UnpausableContext, reason, D22FeedbackCodes.OwnerPending, context);
                }
                state = D22PauseState.Paused;
                return Accept(intent, true, "d22.pause.paused", D22FeedbackCodes.CommandCompleted, context);
            }

            if (state == D22PauseState.Running)
                return NoChange(intent, D22PauseFailureCode.AlreadyRunning, "d22.pause.already_running", D22FeedbackCodes.CommandAccepted, context);
            if (context.IsUnrecoverableError)
                return Reject(intent, D22PauseFailureCode.UnrecoverableError, "d22.pause.unrecoverable_error", D22FeedbackCodes.OwnerUnavailable, context);
            if (!context.ResumeAllowed)
                return Reject(intent, D22PauseFailureCode.ResumeNotAllowed, "d22.pause.resume_not_allowed", D22FeedbackCodes.PermissionDenied, context);

            state = D22PauseState.Running;
            return Accept(intent, true, "d22.pause.resumed", D22FeedbackCodes.CommandCompleted, context);
        }

        /// <summary>Resolve 的语义别名，便于 Runtime adapter 接线。</summary>
        public D22PauseIntentResult Handle(D22PauseIntent intent, D22PauseContext context)
        {
            return Resolve(intent, context);
        }

        /// <summary>仅处理暂停意图。</summary>
        public D22PauseIntentResult Pause(D22PauseContext context, string correlationId = "")
        {
            return Resolve(D22PauseIntent.Pause(correlationId, context == null ? "" : "", context == null ? "" : context.ContextId,
                context == null ? 0 : context.WorldDay, context == null ? 0 : context.SaveRevision), context);
        }

        /// <summary>仅处理恢复意图。</summary>
        public D22PauseIntentResult Resume(D22PauseContext context, string correlationId = "")
        {
            return Resolve(D22PauseIntent.Resume(correlationId, context == null ? "" : "", context == null ? "" : context.ContextId,
                context == null ? 0 : context.WorldDay, context == null ? 0 : context.SaveRevision), context);
        }

        private D22PauseIntentResult Accept(
            D22PauseIntent intent,
            bool changed,
            string reasonCode,
            string feedbackCode,
            D22PauseContext context)
        {
            lastIntent = intent == null ? D22PauseIntentKind.None : intent.Kind;
            lastReadModel = BuildReadModel(intent, context, D22PauseFailureCode.None, reasonCode, feedbackCode);
            return Result(intent, true, changed, false, D22PauseFailureCode.None, reasonCode, feedbackCode, lastReadModel);
        }

        private D22PauseIntentResult NoChange(
            D22PauseIntent intent,
            D22PauseFailureCode failureCode,
            string reasonCode,
            string feedbackCode,
            D22PauseContext context)
        {
            lastIntent = intent == null ? D22PauseIntentKind.None : intent.Kind;
            lastReadModel = BuildReadModel(intent, context, failureCode, reasonCode, feedbackCode);
            return Result(intent, true, false, true, failureCode, reasonCode, feedbackCode, lastReadModel);
        }

        private D22PauseIntentResult Reject(
            D22PauseIntent intent,
            D22PauseFailureCode failureCode,
            string reasonCode,
            string feedbackCode,
            D22PauseContext context = null)
        {
            lastIntent = intent == null ? D22PauseIntentKind.None : intent.Kind;
            lastReadModel = BuildReadModel(intent, context, failureCode, reasonCode, feedbackCode);
            return Result(intent, false, false, false, failureCode, reasonCode, feedbackCode, lastReadModel);
        }

        private D22PauseReadModel BuildReadModel(
            D22PauseIntent intent,
            D22PauseContext context,
            D22PauseFailureCode failureCode,
            string reasonCode,
            string feedbackCode)
        {
            return new D22PauseReadModel
            {
                State = state,
                IsPaused = state == D22PauseState.Paused,
                CanPause = state == D22PauseState.Running && context != null && context.SessionActive &&
                           context.ContextValid && context.PauseAllowed && !context.HasUnpausableOperation &&
                           !context.IsUnrecoverableError,
                CanResume = state == D22PauseState.Paused && context != null && context.SessionActive &&
                            context.ContextValid && context.ResumeAllowed && !context.IsUnrecoverableError,
                LastIntent = intent == null ? lastIntent : intent.Kind,
                FailureCode = failureCode,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = feedbackCode ?? string.Empty,
                CorrelationId = intent == null ? string.Empty : intent.CorrelationId ?? string.Empty,
                ContextId = context == null ? string.Empty : context.ContextId ?? string.Empty,
                UnpausableReasonCode = context == null ? string.Empty : context.UnpausableReasonCode ?? string.Empty,
                WorldDay = context == null ? 0 : context.WorldDay,
                SaveRevision = context == null ? 0 : context.SaveRevision,
            };
        }

        private static D22PauseIntentResult Result(
            D22PauseIntent intent,
            bool accepted,
            bool changed,
            bool noChange,
            D22PauseFailureCode failureCode,
            string reasonCode,
            string feedbackCode,
            D22PauseReadModel readModel)
        {
            return new D22PauseIntentResult
            {
                Accepted = accepted,
                Changed = changed,
                NoChange = noChange,
                State = readModel == null ? D22PauseState.Running : readModel.State,
                IntentKind = intent == null ? D22PauseIntentKind.None : intent.Kind,
                FailureCode = failureCode,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = feedbackCode ?? string.Empty,
                CorrelationId = intent == null ? string.Empty : intent.CorrelationId ?? string.Empty,
                ReadModel = readModel == null ? new D22PauseReadModel() : readModel.Clone(),
            };
        }
    }
}
