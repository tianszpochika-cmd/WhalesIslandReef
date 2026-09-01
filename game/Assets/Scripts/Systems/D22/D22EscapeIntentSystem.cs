// ============================================================
// D22EscapeIntentSystem —— D22-L11 Escape/返回意图解析（纯 C#）
//
// Escape 是 Runtime 归一化后提交的逻辑意图。这里仅依据面板快照和
// 暂停读模型输出确定性关闭/恢复计划，不读取键盘，也不执行实际关闭。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D22
{
    /// <summary>Runtime 可提交的返回类逻辑意图。</summary>
    public enum D22EscapeIntentKind
    {
        None = 0,
        Escape = 1,
        Back = 2,
    }

    /// <summary>Escape 解析后交给 Runtime/owner 的动作。</summary>
    public enum D22EscapeActionKind
    {
        None = 0,
        ClosePanel = 1,
        ResumePause = 2,
        NoClosablePanel = 3,
        BlockedByModal = 4,
        TerminalError = 5,
    }

    /// <summary>关闭/恢复优先级；数值只用于确定性排序，不对应 UI 层级。</summary>
    public enum D22EscapePriority
    {
        None = 0,
        PauseResume = 10,
        NavigationPanel = 20,
        NonBlockingModal = 25,
        BlockingModal = 30,
        ChildModal = 40,
    }

    /// <summary>Escape 解析的稳定失败或无目标原因。</summary>
    public enum D22EscapeFailureCode
    {
        None = 0,
        MissingIntent = 1,
        MissingContext = 2,
        InvalidIntent = 3,
        InvalidPanelContext = 4,
        TerminalErrorActive = 5,
        BlockingModalIntercepted = 6,
        NoClosablePanel = 7,
        InvalidPauseContext = 8,
    }

    /// <summary>返回类逻辑意图；不包含物理按键。</summary>
    [Serializable]
    public sealed class D22EscapeIntent
    {
        public D22EscapeIntentKind Kind;
        public string CorrelationId = string.Empty;
        public string ActorId = string.Empty;

        /// <summary>创建 Escape 意图。</summary>
        public static D22EscapeIntent Escape(string correlationId = "", string actorId = "")
        {
            return new D22EscapeIntent
            {
                Kind = D22EscapeIntentKind.Escape,
                CorrelationId = correlationId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
            };
        }

        /// <summary>创建返回意图。</summary>
        public static D22EscapeIntent Back(string correlationId = "", string actorId = "")
        {
            return new D22EscapeIntent
            {
                Kind = D22EscapeIntentKind.Back,
                CorrelationId = correlationId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D22EscapeIntent Clone()
        {
            return new D22EscapeIntent
            {
                Kind = Kind,
                CorrelationId = CorrelationId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
            };
        }
    }

    /// <summary>Escape 解析使用的逻辑快照；面板和暂停状态均由各自 owner 提供。</summary>
    [Serializable]
    public sealed class D22EscapeContext
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool SessionActive = true;
        public bool ContextValid = true;
        public D22PanelTransitionSnapshot PanelState = new D22PanelTransitionSnapshot();
        public D22PauseReadModel PauseState = new D22PauseReadModel();

        /// <summary>兼容快照命名；仍返回同一字段的防御性副本。</summary>
        public D22PanelTransitionSnapshot PanelSnapshot
        {
            get { return PanelState; }
            set { PanelState = value ?? new D22PanelTransitionSnapshot(); }
        }

        /// <summary>兼容暂停读模型命名。</summary>
        public D22PauseReadModel PauseReadModel
        {
            get { return PauseState; }
            set { PauseState = value ?? new D22PauseReadModel(); }
        }

        /// <summary>返回深拷贝。</summary>
        public D22EscapeContext Clone()
        {
            return new D22EscapeContext
            {
                SchemaVersion = SchemaVersion,
                SessionActive = SessionActive,
                ContextValid = ContextValid,
                PanelState = PanelState == null ? new D22PanelTransitionSnapshot() : PanelState.Clone(),
                PauseState = PauseState == null ? new D22PauseReadModel() : PauseState.Clone(),
            };
        }
    }

    /// <summary>Escape 解析结果；PanelCloseOrder 是只读执行计划，不是已执行关闭。</summary>
    [Serializable]
    public sealed class D22EscapeResolution
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool Accepted;
        public bool Handled;
        public D22EscapeActionKind Action;
        public D22EscapePriority Priority;
        public D22EscapeFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string FeedbackCode = string.Empty;
        public string CorrelationId = string.Empty;
        public string TargetPanelId = string.Empty;
        public D22CloseReason CloseReason = D22CloseReason.None;
        public string CloseReasonId = D22CloseReasonIds.None;
        public D22PauseIntentKind FollowUpPauseIntent = D22PauseIntentKind.None;
        public List<string> PanelCloseOrder = new List<string>();

        /// <summary>兼容选中面板命名。</summary>
        public string SelectedPanelId
        {
            get { return TargetPanelId; }
        }

        /// <summary>兼容后续暂停意图命名。</summary>
        public D22PauseIntentKind NextPauseIntent
        {
            get { return FollowUpPauseIntent; }
        }

        /// <summary>返回深拷贝。</summary>
        public D22EscapeResolution Clone()
        {
            var result = new D22EscapeResolution
            {
                SchemaVersion = SchemaVersion,
                Accepted = Accepted,
                Handled = Handled,
                Action = Action,
                Priority = Priority,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                TargetPanelId = TargetPanelId ?? string.Empty,
                CloseReason = CloseReason,
                CloseReasonId = CloseReasonId ?? D22CloseReasonIds.None,
                FollowUpPauseIntent = FollowUpPauseIntent,
            };
            if (PanelCloseOrder != null) result.PanelCloseOrder.AddRange(PanelCloseOrder);
            return result;
        }
    }

    /// <summary>
    /// D22-L11 Escape/返回意图优先级解析器。它只生成逻辑计划，不读取设备或写面板状态。
    /// </summary>
    public static class D22EscapeIntentSystem
    {
        public const int CurrentSchemaVersion = D22EscapeResolution.CurrentSchemaVersion;
        public const string OwnerSystem = "D22EscapeIntentSystem";

        /// <summary>按子模态、阻塞模态、普通面板、暂停恢复的顺序解析。</summary>
        public static D22EscapeResolution Resolve(D22EscapeIntent intent, D22EscapeContext context)
        {
            if (intent == null)
                return Reject(D22EscapeFailureCode.MissingIntent, "d22.escape.missing_intent", string.Empty, string.Empty);
            if (intent.Kind != D22EscapeIntentKind.Escape && intent.Kind != D22EscapeIntentKind.Back)
                return Reject(D22EscapeFailureCode.InvalidIntent, "d22.escape.invalid_intent", intent.CorrelationId, string.Empty);
            if (context == null)
                return Reject(D22EscapeFailureCode.MissingContext, "d22.escape.missing_context", intent.CorrelationId, string.Empty);
            if (context.SchemaVersion != D22EscapeContext.CurrentSchemaVersion || !context.SessionActive || !context.ContextValid)
                return Reject(D22EscapeFailureCode.InvalidPanelContext, "d22.escape.invalid_context", intent.CorrelationId, string.Empty);
            if (context.PanelState == null)
                return Reject(D22EscapeFailureCode.InvalidPanelContext, "d22.escape.invalid_panel_context", intent.CorrelationId, string.Empty);
            if (context.PanelState.IsUnrecoverableError)
                return Reject(D22EscapeFailureCode.TerminalErrorActive, "d22.escape.terminal_error_active", intent.CorrelationId, string.Empty,
                    D22EscapeActionKind.TerminalError, true);
            if (context.PanelState.OpenPanels == null)
                return Reject(D22EscapeFailureCode.InvalidPanelContext, "d22.escape.invalid_panel_context", intent.CorrelationId, string.Empty);

            var ordered = BuildCloseOrder(context.PanelState.OpenPanels);
            D22TransitionPanelState blocking = FindTopBlockingModal(context.PanelState.OpenPanels);
            if (blocking != null)
            {
                D22PanelDefinition blockingDefinition;
                if (!D22ScreenContracts.TryGetPanelDefinition(blocking.PanelId, out blockingDefinition))
                    return Reject(D22EscapeFailureCode.InvalidPanelContext, "d22.escape.invalid_panel_context", intent.CorrelationId, blocking.PanelId);
                if (!CanCloseByEscape(blockingDefinition))
                {
                    return new D22EscapeResolution
                    {
                        Accepted = false,
                        Handled = true,
                        Action = D22EscapeActionKind.BlockedByModal,
                        Priority = blockingDefinition.ModalKind == D22ModalKind.ChildBlocking
                            ? D22EscapePriority.ChildModal
                            : D22EscapePriority.BlockingModal,
                        FailureCode = D22EscapeFailureCode.BlockingModalIntercepted,
                        ReasonCode = "d22.escape.blocked_by_modal",
                        FeedbackCode = D22FeedbackCodes.PermissionDenied,
                        CorrelationId = intent.CorrelationId ?? string.Empty,
                        TargetPanelId = blocking.PanelId ?? string.Empty,
                        PanelCloseOrder = ordered,
                    };
                }
            }

            foreach (var panelId in ordered)
            {
                D22TransitionPanelState state = FindPanel(context.PanelState.OpenPanels, panelId);
                if (state == null) continue;
                D22PanelDefinition definition;
                if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out definition))
                    continue;
                if (!CanCloseByEscape(definition)) continue;
                D22EscapePriority priority = PriorityFor(definition);
                return new D22EscapeResolution
                {
                    Accepted = true,
                    Handled = true,
                    Action = D22EscapeActionKind.ClosePanel,
                    Priority = priority,
                    FailureCode = D22EscapeFailureCode.None,
                    ReasonCode = "d22.escape.close_panel",
                    FeedbackCode = D22FeedbackCodes.PanelClosed,
                    CorrelationId = intent.CorrelationId ?? string.Empty,
                    TargetPanelId = state.PanelId ?? string.Empty,
                    CloseReason = D22CloseReason.BackNavigation,
                    CloseReasonId = D22CloseReasonIds.BackNavigation,
                    PanelCloseOrder = ordered,
                };
            }

            if (context.PauseState != null && context.PauseState.IsPaused)
            {
                return new D22EscapeResolution
                {
                    Accepted = true,
                    Handled = true,
                    Action = D22EscapeActionKind.ResumePause,
                    Priority = D22EscapePriority.PauseResume,
                    FailureCode = D22EscapeFailureCode.None,
                    ReasonCode = "d22.escape.resume_pause",
                    FeedbackCode = D22FeedbackCodes.CommandAccepted,
                    CorrelationId = intent.CorrelationId ?? string.Empty,
                    FollowUpPauseIntent = D22PauseIntentKind.Resume,
                    PanelCloseOrder = ordered,
                };
            }

            return new D22EscapeResolution
            {
                Accepted = false,
                Handled = true,
                Action = D22EscapeActionKind.NoClosablePanel,
                Priority = D22EscapePriority.None,
                FailureCode = D22EscapeFailureCode.NoClosablePanel,
                ReasonCode = "d22.escape.no_closable_panel",
                FeedbackCode = D22FeedbackCodes.InvalidRequest,
                CorrelationId = intent.CorrelationId ?? string.Empty,
                PanelCloseOrder = ordered,
            };
        }

        /// <summary>Resolve 的语义别名。</summary>
        public static D22EscapeResolution Handle(D22EscapeIntent intent, D22EscapeContext context)
        {
            return Resolve(intent, context);
        }

        /// <summary>返回确定性的候选关闭顺序；输入快照不会被修改。</summary>
        public static IReadOnlyList<string> GetPanelCloseOrder(D22PanelTransitionSnapshot snapshot)
        {
            if (snapshot == null || snapshot.OpenPanels == null)
                return new List<string>().AsReadOnly();
            return BuildCloseOrder(snapshot.OpenPanels).AsReadOnly();
        }

        private static List<string> BuildCloseOrder(IList<D22TransitionPanelState> panels)
        {
            var result = new List<string>();
            if (panels == null) return result;
            AddCategory(panels, result, true, false);
            AddCategory(panels, result, false, false);
            AddCategory(panels, result, false, true);
            return result;
        }

        private static void AddCategory(
            IList<D22TransitionPanelState> panels,
            List<string> result,
            bool blockingModal,
            bool ordinaryPanel)
        {
            for (int index = panels.Count - 1; index >= 0; index--)
            {
                var state = panels[index];
                if (state == null || !state.IsOpen || result.Contains(state.PanelId)) continue;
                D22PanelDefinition definition;
                if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out definition)) continue;
                bool isBlockingModal = definition.SurfaceKind == D22SurfaceKind.Modal &&
                                       definition.ModalKind != D22ModalKind.NonBlocking;
                bool isOrdinaryPanel = definition.SurfaceKind == D22SurfaceKind.Panel;
                if ((blockingModal && isBlockingModal) || (ordinaryPanel && isOrdinaryPanel) ||
                    (!blockingModal && !ordinaryPanel && definition.SurfaceKind == D22SurfaceKind.Modal && !isBlockingModal))
                    result.Add(state.PanelId ?? string.Empty);
            }
        }

        private static D22TransitionPanelState FindTopBlockingModal(IList<D22TransitionPanelState> panels)
        {
            if (panels == null) return null;
            for (int index = panels.Count - 1; index >= 0; index--)
            {
                var state = panels[index];
                if (state == null || !state.IsOpen) continue;
                D22PanelDefinition definition;
                if (D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out definition) &&
                    definition.SurfaceKind == D22SurfaceKind.Modal && definition.ModalKind != D22ModalKind.NonBlocking)
                    return state;
            }
            return null;
        }

        private static D22TransitionPanelState FindPanel(IList<D22TransitionPanelState> panels, string panelId)
        {
            if (panels == null) return null;
            foreach (var state in panels)
                if (state != null && state.IsOpen && string.Equals(state.PanelId, panelId, StringComparison.Ordinal)) return state;
            return null;
        }

        private static bool CanCloseByEscape(D22PanelDefinition definition)
        {
            return definition != null && definition.ClosePolicy == D22ClosePolicy.UserDismissible;
        }

        private static D22EscapePriority PriorityFor(D22PanelDefinition definition)
        {
            if (definition == null) return D22EscapePriority.None;
            if (definition.SurfaceKind == D22SurfaceKind.Modal)
            {
                if (definition.ModalKind == D22ModalKind.ChildBlocking) return D22EscapePriority.ChildModal;
                if (definition.ModalKind == D22ModalKind.NonBlocking) return D22EscapePriority.NonBlockingModal;
                return D22EscapePriority.BlockingModal;
            }
            return D22EscapePriority.NavigationPanel;
        }

        private static D22EscapeResolution Reject(
            D22EscapeFailureCode code,
            string reasonCode,
            string correlationId,
            string targetPanelId,
            D22EscapeActionKind action = D22EscapeActionKind.None,
            bool handled = false)
        {
            return new D22EscapeResolution
            {
                Accepted = false,
                Handled = handled,
                Action = action,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = code == D22EscapeFailureCode.BlockingModalIntercepted
                    ? D22FeedbackCodes.PermissionDenied
                    : D22FeedbackCodes.InvalidRequest,
                CorrelationId = correlationId ?? string.Empty,
                TargetPanelId = targetPanelId ?? string.Empty,
            };
        }
    }
}
