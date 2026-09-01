// ============================================================
// D22PanelTransitionSystem —— D22-L09 面板生命周期状态转换（纯 C#）
//
// 面板状态是 UI 逻辑事实，不是 Canvas/GameObject 生命周期。这里复用
// D22-L04 的结构规则执行确定性转换，并把运行时对象操作留给适配器。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>面板转换请求的操作种类。</summary>
    public enum D22PanelTransitionOperation
    {
        None = 0,
        Open = 1,
        Close = 2,
        Back = 3,
        Replace = 4,
        UnrecoverableError = 5,
        ResetSession = 6,
    }

    /// <summary>面板转换的确定性结果状态。</summary>
    public enum D22PanelTransitionStatus
    {
        Rejected = 0,
        Applied = 1,
        NoChange = 2,
        Blocked = 3,
        TerminalError = 4,
    }

    /// <summary>面板转换失败或边界原因。</summary>
    public enum D22PanelTransitionFailureCode
    {
        None = 0,
        MissingRequest = 1,
        MissingPanelId = 2,
        UnknownPanelId = 3,
        MissingScreenId = 4,
        UnknownScreenId = 5,
        ScreenContextMismatch = 6,
        PanelAlreadyOpen = 7,
        PanelNotOpen = 8,
        CloseNotAllowed = 9,
        InvalidCloseReason = 10,
        ExclusivityConflict = 11,
        InvalidState = 12,
        UnrecoverableErrorActive = 13,
        MissingErrorCode = 14,
        ChildPanelStillOpen = 15,
        NoClosablePanel = 16,
        UnknownOperation = 17,
    }

    /// <summary>面板转换请求；只包含稳定逻辑 ID 与关闭原因。</summary>
    [Serializable]
    public sealed class D22PanelTransitionRequest
    {
        public D22PanelTransitionOperation Operation;
        public string PanelId = string.Empty;
        public string ScreenId = string.Empty;
        public string OwnerId = string.Empty;
        public string ParentPanelId = string.Empty;
        public D22CloseReason CloseReason = D22CloseReason.None;
        public string ErrorCode = string.Empty;

        /// <summary>创建打开请求。</summary>
        public static D22PanelTransitionRequest Open(
            string panelId,
            string screenId = "",
            string ownerId = "",
            string parentPanelId = "")
        {
            return new D22PanelTransitionRequest
            {
                Operation = D22PanelTransitionOperation.Open,
                PanelId = panelId ?? string.Empty,
                ScreenId = screenId ?? string.Empty,
                OwnerId = ownerId ?? string.Empty,
                ParentPanelId = parentPanelId ?? string.Empty,
            };
        }

        /// <summary>创建关闭请求。</summary>
        public static D22PanelTransitionRequest Close(string panelId, D22CloseReason reason)
        {
            return new D22PanelTransitionRequest
            {
                Operation = D22PanelTransitionOperation.Close,
                PanelId = panelId ?? string.Empty,
                CloseReason = reason,
            };
        }

        /// <summary>创建返回请求。</summary>
        public static D22PanelTransitionRequest Back()
        {
            return new D22PanelTransitionRequest
            {
                Operation = D22PanelTransitionOperation.Back,
                CloseReason = D22CloseReason.BackNavigation,
            };
        }

        /// <summary>创建替换请求。</summary>
        public static D22PanelTransitionRequest Replace(
            string panelId,
            string screenId = "",
            string ownerId = "",
            string parentPanelId = "")
        {
            var request = Open(panelId, screenId, ownerId, parentPanelId);
            request.Operation = D22PanelTransitionOperation.Replace;
            return request;
        }

        /// <summary>创建不可恢复错误请求。</summary>
        public static D22PanelTransitionRequest Error(string errorCode)
        {
            return new D22PanelTransitionRequest
            {
                Operation = D22PanelTransitionOperation.UnrecoverableError,
                ErrorCode = errorCode ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D22PanelTransitionRequest Clone()
        {
            return new D22PanelTransitionRequest
            {
                Operation = Operation,
                PanelId = PanelId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                ParentPanelId = ParentPanelId ?? string.Empty,
                CloseReason = CloseReason,
                ErrorCode = ErrorCode ?? string.Empty,
            };
        }
    }

    /// <summary>已打开面板的逻辑状态；列表顺序就是确定性的打开顺序。</summary>
    [Serializable]
    public sealed class D22TransitionPanelState
    {
        public string PanelId = string.Empty;
        public string ScreenId = string.Empty;
        public string OwnerId = string.Empty;
        public string ParentPanelId = string.Empty;
        public bool IsOpen = true;

        /// <summary>返回防御性副本。</summary>
        public D22TransitionPanelState Clone()
        {
            return new D22TransitionPanelState
            {
                PanelId = PanelId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                ParentPanelId = ParentPanelId ?? string.Empty,
                IsOpen = IsOpen,
            };
        }
    }

    /// <summary>D22 面板转换的可存档逻辑快照。</summary>
    [Serializable]
    public sealed class D22PanelTransitionSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string ActiveScreenId = string.Empty;
        public string ActivePanelId = string.Empty;
        public bool IsUnrecoverableError;
        public string ErrorCode = string.Empty;
        public D22PanelTransitionOperation LastOperation;
        public string LastReasonCode = string.Empty;
        public List<D22TransitionPanelState> OpenPanels = new List<D22TransitionPanelState>();

        /// <summary>返回深拷贝。</summary>
        public D22PanelTransitionSnapshot Clone()
        {
            var result = new D22PanelTransitionSnapshot
            {
                SchemaVersion = SchemaVersion,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                IsUnrecoverableError = IsUnrecoverableError,
                ErrorCode = ErrorCode ?? string.Empty,
                LastOperation = LastOperation,
                LastReasonCode = LastReasonCode ?? string.Empty,
            };
            if (OpenPanels != null)
            {
                foreach (var panel in OpenPanels)
                    result.OpenPanels.Add(panel == null ? null : panel.Clone());
            }
            return result;
        }

        /// <summary>只读判断指定面板是否已经打开。</summary>
        public bool HasOpenPanel(string panelId)
        {
            if (OpenPanels == null || string.IsNullOrEmpty(panelId)) return false;
            foreach (var panel in OpenPanels)
            {
                if (panel != null && panel.IsOpen &&
                    string.Equals(panel.PanelId, panelId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }

    /// <summary>一次面板转换的结果与转换后的防御性快照。</summary>
    [Serializable]
    public sealed class D22PanelTransitionResult
    {
        public int SchemaVersion = D22PanelTransitionSnapshot.CurrentSchemaVersion;
        public D22PanelTransitionOperation Operation;
        public D22PanelTransitionStatus Status;
        public bool Accepted;
        public bool Changed;
        public bool NoChange;
        public D22PanelTransitionFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string FeedbackCode = string.Empty;
        public string TargetPanelId = string.Empty;
        public string CloseReasonId = D22CloseReasonIds.None;
        public List<string> AffectedPanelIds = new List<string>();
        public D22PanelTransitionSnapshot State = new D22PanelTransitionSnapshot();

        /// <summary>返回深拷贝。</summary>
        public D22PanelTransitionResult Clone()
        {
            var result = new D22PanelTransitionResult
            {
                SchemaVersion = SchemaVersion,
                Operation = Operation,
                Status = Status,
                Accepted = Accepted,
                Changed = Changed,
                NoChange = NoChange,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                TargetPanelId = TargetPanelId ?? string.Empty,
                CloseReasonId = CloseReasonId ?? D22CloseReasonIds.None,
                State = State == null ? new D22PanelTransitionSnapshot() : State.Clone(),
            };
            if (AffectedPanelIds != null) result.AffectedPanelIds.AddRange(AffectedPanelIds);
            return result;
        }
    }

    /// <summary>
    /// D22-L09 面板生命周期状态机。它只写逻辑面板状态，不创建或关闭 Runtime 对象。
    /// </summary>
    public sealed class D22PanelTransitionSystem
    {
        public const int CurrentSchemaVersion = D22PanelTransitionSnapshot.CurrentSchemaVersion;
        public const string OwnerSystem = "D22PanelTransitionSystem";

        private readonly List<D22TransitionPanelState> openPanels = new List<D22TransitionPanelState>();
        private string activeScreenId = string.Empty;
        private string activePanelId = string.Empty;
        private bool isUnrecoverableError;
        private string errorCode = string.Empty;
        private D22PanelTransitionOperation lastOperation;
        private string lastReasonCode = string.Empty;

        /// <summary>创建空逻辑上下文；调用方可随后显式 ActivateScreen。</summary>
        public D22PanelTransitionSystem() { }

        /// <summary>创建一个已激活的 screen/panel 上下文。</summary>
        public D22PanelTransitionSystem(string screenId, string panelId)
        {
            ActivateScreen(screenId, panelId);
        }

        /// <summary>返回当前面板生命周期快照。</summary>
        public D22PanelTransitionSnapshot Snapshot()
        {
            var result = new D22PanelTransitionSnapshot
            {
                ActiveScreenId = activeScreenId,
                ActivePanelId = activePanelId,
                IsUnrecoverableError = isUnrecoverableError,
                ErrorCode = errorCode,
                LastOperation = lastOperation,
                LastReasonCode = lastReasonCode,
            };
            foreach (var panel in openPanels) result.OpenPanels.Add(panel.Clone());
            return result;
        }

        /// <summary>当前是否已经进入不可恢复错误终态。</summary>
        public bool IsUnrecoverableError
        {
            get { return isUnrecoverableError; }
        }

        /// <summary>激活一个逻辑 screen，并以显式 panel 建立初始上下文。</summary>
        public D22PanelTransitionResult ActivateScreen(string screenId, string panelId = "")
        {
            D22ScreenDefinition screen;
            if (string.IsNullOrEmpty(screenId))
                return Reject(D22PanelTransitionOperation.Replace, D22PanelTransitionFailureCode.MissingScreenId,
                    "d22.panel.missing_screen_id", panelId);
            if (!D22ScreenContracts.TryGetScreenDefinition(screenId, out screen))
                return Reject(D22PanelTransitionOperation.Replace, D22PanelTransitionFailureCode.UnknownScreenId,
                    "d22.panel.unknown_screen_id", panelId);
            if (isUnrecoverableError)
                return Reject(D22PanelTransitionOperation.Replace, D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                    "d22.panel.unrecoverable_error_active", panelId, true);

            string resolvedPanelId = string.IsNullOrEmpty(panelId) ? screen.DefaultPanelId : panelId;
            D22PanelDefinition panel;
            if (!D22ScreenContracts.TryGetPanelDefinition(resolvedPanelId, out panel) ||
                !string.Equals(panel.ParentScreenId, screenId, StringComparison.Ordinal) ||
                panel.SurfaceKind != D22SurfaceKind.Panel)
                return Reject(D22PanelTransitionOperation.Replace, D22PanelTransitionFailureCode.ScreenContextMismatch,
                    "d22.panel.screen_context_mismatch", resolvedPanelId);

            bool changed = !string.Equals(activeScreenId, screenId, StringComparison.Ordinal) ||
                           !string.Equals(activePanelId, resolvedPanelId, StringComparison.Ordinal) ||
                           openPanels.Count != 0;
            openPanels.Clear();
            activeScreenId = screenId;
            activePanelId = resolvedPanelId;
            openPanels.Add(new D22TransitionPanelState
            {
                PanelId = resolvedPanelId,
                ScreenId = screenId,
                IsOpen = true,
            });
            lastOperation = D22PanelTransitionOperation.Replace;
            lastReasonCode = "d22.panel.screen_activated";
            return Applied(D22PanelTransitionOperation.Replace, changed, resolvedPanelId,
                D22CloseReasonIds.Replaced, "d22.panel.screen_activated", D22FeedbackCodes.PanelOpened,
                new List<string>());
        }

        /// <summary>打开一个 panel/modal/overlay，互斥由 L04 决定。</summary>
        public D22PanelTransitionResult Open(D22PanelTransitionRequest request)
        {
            return OpenCore(request, D22PanelTransitionOperation.Open);
        }

        /// <summary>使用稳定 ID 打开面板的便捷入口。</summary>
        public D22PanelTransitionResult Open(
            string panelId,
            string screenId = "",
            string ownerId = "",
            string parentPanelId = "")
        {
            return Open(D22PanelTransitionRequest.Open(panelId, screenId, ownerId, parentPanelId));
        }

        /// <summary>替换同一互斥组内的面板。</summary>
        public D22PanelTransitionResult Replace(D22PanelTransitionRequest request)
        {
            return OpenCore(request, D22PanelTransitionOperation.Replace);
        }

        /// <summary>使用稳定 ID 替换面板的便捷入口。</summary>
        public D22PanelTransitionResult Replace(
            string panelId,
            string screenId = "",
            string ownerId = "",
            string parentPanelId = "")
        {
            return Replace(D22PanelTransitionRequest.Replace(panelId, screenId, ownerId, parentPanelId));
        }

        /// <summary>显式关闭面板；关闭策略仍由逻辑契约约束。</summary>
        public D22PanelTransitionResult Close(D22PanelTransitionRequest request)
        {
            if (request == null)
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.MissingRequest,
                    "d22.panel.missing_request", string.Empty);
            return ClosePanel(request.PanelId, request.CloseReason);
        }

        /// <summary>使用稳定 ID 关闭面板的便捷入口。</summary>
        public D22PanelTransitionResult Close(string panelId, D22CloseReason reason)
        {
            return ClosePanel(panelId, reason);
        }

        /// <summary>按确定性栈顺序返回并关闭最上层可关闭面板。</summary>
        public D22PanelTransitionResult Back()
        {
            if (isUnrecoverableError)
                return Reject(D22PanelTransitionOperation.Back, D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                    "d22.panel.unrecoverable_error_active", string.Empty, true);

            D22TransitionPanelState candidate = null;
            for (int index = openPanels.Count - 1; index >= 0; index--)
            {
                var state = openPanels[index];
                if (state == null || !state.IsOpen) continue;
                D22PanelDefinition definition;
                if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out definition))
                    continue;
                if (definition.SurfaceKind == D22SurfaceKind.Modal &&
                    definition.ModalKind != D22ModalKind.NonBlocking)
                {
                    if (!CanClose(definition, D22CloseReason.BackNavigation))
                        return Reject(D22PanelTransitionOperation.Back, D22PanelTransitionFailureCode.CloseNotAllowed,
                            "d22.panel.close_not_allowed", state.PanelId);
                    candidate = state;
                    break;
                }
                if (CanClose(definition, D22CloseReason.BackNavigation))
                {
                    candidate = state;
                    break;
                }
            }

            if (candidate == null)
                return Reject(D22PanelTransitionOperation.Back, D22PanelTransitionFailureCode.NoClosablePanel,
                    "d22.panel.no_closable_panel", string.Empty);
            return ClosePanel(candidate.PanelId, D22CloseReason.BackNavigation);
        }

        /// <summary>关闭指定面板并返回转换结果。</summary>
        public D22PanelTransitionResult ClosePanel(string panelId, D22CloseReason reason)
        {
            if (isUnrecoverableError)
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                    "d22.panel.unrecoverable_error_active", panelId, true);
            if (string.IsNullOrEmpty(panelId))
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.MissingPanelId,
                    "d22.panel.missing_panel_id", panelId);
            if (reason == D22CloseReason.None)
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.InvalidCloseReason,
                    "d22.panel.invalid_close_reason", panelId);

            int index = FindOpenPanelIndex(panelId);
            if (index < 0)
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.PanelNotOpen,
                    "d22.panel.panel_not_open", panelId);

            D22PanelDefinition definition;
            if (!D22ScreenContracts.TryGetPanelDefinition(panelId, out definition))
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.UnknownPanelId,
                    "d22.panel.unknown_panel_id", panelId);
            if (!CanClose(definition, reason))
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.CloseNotAllowed,
                    "d22.panel.close_not_allowed", panelId);
            if (HasOpenChild(panelId))
                return Reject(D22PanelTransitionOperation.Close, D22PanelTransitionFailureCode.ChildPanelStillOpen,
                    "d22.panel.child_panel_still_open", panelId);

            openPanels.RemoveAt(index);
            if (string.Equals(activePanelId, panelId, StringComparison.Ordinal))
                activePanelId = FindFallbackPanel(definition.ParentScreenId);
            lastOperation = D22PanelTransitionOperation.Close;
            lastReasonCode = "d22.panel.panel_closed";
            var affected = new List<string> { panelId };
            return Applied(D22PanelTransitionOperation.Close, true, panelId,
                D22ScreenContracts.GetCloseReasonId(reason), "d22.panel.panel_closed",
                D22FeedbackCodes.PanelClosed, affected);
        }

        /// <summary>进入不可恢复错误终态并清除所有可交互面板。</summary>
        public D22PanelTransitionResult EnterUnrecoverableError(string terminalErrorCode)
        {
            if (string.IsNullOrEmpty(terminalErrorCode))
                return Reject(D22PanelTransitionOperation.UnrecoverableError,
                    D22PanelTransitionFailureCode.MissingErrorCode, "d22.panel.missing_error_code", string.Empty);
            if (isUnrecoverableError)
            {
                if (string.Equals(errorCode, terminalErrorCode, StringComparison.Ordinal))
                    return NoChange(D22PanelTransitionOperation.UnrecoverableError,
                        D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                        "d22.panel.unrecoverable_error_already_active", string.Empty);
                return Reject(D22PanelTransitionOperation.UnrecoverableError,
                    D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                    "d22.panel.unrecoverable_error_active", string.Empty, true);
            }

            var affected = new List<string>();
            foreach (var panel in openPanels)
                if (panel != null && panel.IsOpen) affected.Add(panel.PanelId ?? string.Empty);
            openPanels.Clear();
            activePanelId = string.Empty;
            isUnrecoverableError = true;
            errorCode = terminalErrorCode;
            lastOperation = D22PanelTransitionOperation.UnrecoverableError;
            lastReasonCode = "d22.panel.unrecoverable_error";
            return Applied(D22PanelTransitionOperation.UnrecoverableError, true, string.Empty,
                D22CloseReasonIds.Error, "d22.panel.unrecoverable_error", D22FeedbackCodes.OwnerUnavailable,
                affected, D22PanelTransitionStatus.TerminalError);
        }

        /// <summary>
        /// 只有明确的新会话边界才能清除不可恢复错误；普通 Close/Back 不会恢复它。
        /// </summary>
        public D22PanelTransitionResult ResetSession()
        {
            if (!isUnrecoverableError && openPanels.Count == 0 &&
                string.IsNullOrEmpty(activeScreenId) && string.IsNullOrEmpty(activePanelId))
                return NoChange(D22PanelTransitionOperation.ResetSession,
                    D22PanelTransitionFailureCode.InvalidState, "d22.panel.session_already_empty", string.Empty);

            openPanels.Clear();
            activeScreenId = string.Empty;
            activePanelId = string.Empty;
            isUnrecoverableError = false;
            errorCode = string.Empty;
            lastOperation = D22PanelTransitionOperation.ResetSession;
            lastReasonCode = "d22.panel.session_reset";
            return Applied(D22PanelTransitionOperation.ResetSession, true, string.Empty,
                D22CloseReasonIds.SessionEnded, "d22.panel.session_reset", D22FeedbackCodes.PanelClosed,
                new List<string>());
        }

        private D22PanelTransitionResult OpenCore(
            D22PanelTransitionRequest request,
            D22PanelTransitionOperation operation)
        {
            if (request == null)
                return Reject(operation, D22PanelTransitionFailureCode.MissingRequest,
                    "d22.panel.missing_request", string.Empty);
            if (isUnrecoverableError)
                return Reject(operation, D22PanelTransitionFailureCode.UnrecoverableErrorActive,
                    "d22.panel.unrecoverable_error_active", request.PanelId, true);
            if (string.IsNullOrEmpty(request.PanelId))
                return Reject(operation, D22PanelTransitionFailureCode.MissingPanelId,
                    "d22.panel.missing_panel_id", request.PanelId);

            D22PanelDefinition definition;
            if (!D22ScreenContracts.TryGetPanelDefinition(request.PanelId, out definition))
                return Reject(operation, D22PanelTransitionFailureCode.UnknownPanelId,
                    "d22.panel.unknown_panel_id", request.PanelId);

            string effectiveScreenId = string.IsNullOrEmpty(request.ScreenId)
                ? definition.ParentScreenId
                : request.ScreenId;
            if (!string.IsNullOrEmpty(effectiveScreenId) &&
                !D22ScreenContracts.IsKnownScreenId(effectiveScreenId))
                return Reject(operation, D22PanelTransitionFailureCode.UnknownScreenId,
                    "d22.panel.unknown_screen_id", request.PanelId);
            if (!string.IsNullOrEmpty(definition.ParentScreenId) &&
                !string.IsNullOrEmpty(request.ScreenId) &&
                !string.Equals(definition.ParentScreenId, request.ScreenId, StringComparison.Ordinal))
                return Reject(operation, D22PanelTransitionFailureCode.ScreenContextMismatch,
                    "d22.panel.screen_context_mismatch", request.PanelId);
            if (!string.IsNullOrEmpty(activeScreenId) && !string.IsNullOrEmpty(effectiveScreenId) &&
                !string.Equals(activeScreenId, effectiveScreenId, StringComparison.Ordinal))
                return Reject(operation, D22PanelTransitionFailureCode.ScreenContextMismatch,
                    "d22.panel.screen_context_mismatch", request.PanelId);

            var ruleContext = new D22PanelRuleContext
            {
                ActiveScreenId = activeScreenId,
                OpenPanels = new List<D22OpenPanelState>(),
            };
            foreach (var state in openPanels)
            {
                if (state == null || !state.IsOpen) continue;
                ruleContext.OpenPanels.Add(new D22OpenPanelState
                {
                    PanelId = state.PanelId,
                    ScreenId = state.ScreenId,
                    OwnerId = state.OwnerId,
                    ParentPanelId = state.ParentPanelId,
                    IsOpen = true,
                });
            }
            var ruleRequest = new D22PanelOpenRequest
            {
                PanelId = request.PanelId,
                ScreenId = request.ScreenId ?? string.Empty,
                OwnerId = request.OwnerId ?? string.Empty,
                ParentPanelId = request.ParentPanelId ?? string.Empty,
            };
            D22PanelRuleDecision decision = D22PanelExclusivityRules.Evaluate(ruleRequest, ruleContext);
            if (decision.NoChange)
                return NoChange(operation, D22PanelTransitionFailureCode.PanelAlreadyOpen,
                    "d22.panel.already_open", request.PanelId);
            if (!decision.Allowed)
                return Reject(operation, D22PanelTransitionFailureCode.ExclusivityConflict,
                    decision.ReasonCode, request.PanelId);

            var affected = new List<string>();
            if (decision.RequiresForceClose && decision.ForceClosePanelIds != null)
            {
                foreach (var panelId in decision.ForceClosePanelIds)
                {
                    int index = FindOpenPanelIndex(panelId);
                    if (index >= 0)
                    {
                        openPanels.RemoveAt(index);
                        affected.Add(panelId ?? string.Empty);
                    }
                }
            }

            if (string.IsNullOrEmpty(activeScreenId) && !string.IsNullOrEmpty(effectiveScreenId))
                activeScreenId = effectiveScreenId;
            openPanels.Add(new D22TransitionPanelState
            {
                PanelId = request.PanelId,
                ScreenId = effectiveScreenId,
                OwnerId = request.OwnerId ?? string.Empty,
                ParentPanelId = request.ParentPanelId ?? string.Empty,
                IsOpen = true,
            });
            if (definition.SurfaceKind == D22SurfaceKind.Panel && !definition.IsNavigationPanel &&
                string.Equals(definition.ParentScreenId, activeScreenId, StringComparison.Ordinal))
                activePanelId = request.PanelId;
            else if (definition.SurfaceKind == D22SurfaceKind.Panel && definition.IsNavigationPanel)
                activePanelId = request.PanelId;
            lastOperation = operation;
            lastReasonCode = operation == D22PanelTransitionOperation.Replace
                ? "d22.panel.panel_replaced"
                : "d22.panel.panel_opened";
            return Applied(operation, true, request.PanelId,
                operation == D22PanelTransitionOperation.Replace
                    ? D22CloseReasonIds.Replaced
                    : D22CloseReasonIds.None,
                lastReasonCode, D22FeedbackCodes.PanelOpened, affected);
        }

        private int FindOpenPanelIndex(string panelId)
        {
            for (int index = 0; index < openPanels.Count; index++)
            {
                var state = openPanels[index];
                if (state != null && state.IsOpen &&
                    string.Equals(state.PanelId, panelId, StringComparison.Ordinal))
                    return index;
            }
            return -1;
        }

        private bool HasOpenChild(string panelId)
        {
            foreach (var state in openPanels)
            {
                if (state != null && state.IsOpen &&
                    string.Equals(state.ParentPanelId, panelId, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private string FindFallbackPanel(string screenId)
        {
            if (string.IsNullOrEmpty(screenId)) return string.Empty;
            for (int index = openPanels.Count - 1; index >= 0; index--)
            {
                var state = openPanels[index];
                if (state == null || !state.IsOpen ||
                    !string.Equals(state.ScreenId, screenId, StringComparison.Ordinal)) continue;
                D22PanelDefinition definition;
                if (D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out definition) &&
                    definition.SurfaceKind == D22SurfaceKind.Panel && definition.IsNavigationPanel)
                    return state.PanelId;
            }
            return string.Empty;
        }

        private static bool CanClose(D22PanelDefinition definition, D22CloseReason reason)
        {
            if (definition == null || reason == D22CloseReason.None) return false;
            if (reason == D22CloseReason.ContextInvalidated ||
                reason == D22CloseReason.ParentClosed ||
                reason == D22CloseReason.OwnerCompleted ||
                reason == D22CloseReason.OwnerRejected ||
                reason == D22CloseReason.FocusLost ||
                reason == D22CloseReason.SessionEnded ||
                reason == D22CloseReason.Error)
                return true;
            if (reason == D22CloseReason.Replaced)
                return definition.ClosePolicy != D22ClosePolicy.ParentOnly;
            return definition.ClosePolicy == D22ClosePolicy.UserDismissible;
        }

        private D22PanelTransitionResult Applied(
            D22PanelTransitionOperation operation,
            bool changed,
            string targetPanelId,
            string closeReasonId,
            string reasonCode,
            string feedbackCode,
            List<string> affectedPanelIds,
            D22PanelTransitionStatus status = D22PanelTransitionStatus.Applied)
        {
            return new D22PanelTransitionResult
            {
                Operation = operation,
                Status = status,
                Accepted = true,
                Changed = changed,
                NoChange = !changed,
                FailureCode = D22PanelTransitionFailureCode.None,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = feedbackCode ?? string.Empty,
                TargetPanelId = targetPanelId ?? string.Empty,
                CloseReasonId = closeReasonId ?? D22CloseReasonIds.None,
                AffectedPanelIds = affectedPanelIds ?? new List<string>(),
                State = Snapshot(),
            };
        }

        private D22PanelTransitionResult NoChange(
            D22PanelTransitionOperation operation,
            D22PanelTransitionFailureCode code,
            string reasonCode,
            string targetPanelId)
        {
            return new D22PanelTransitionResult
            {
                Operation = operation,
                Status = D22PanelTransitionStatus.NoChange,
                Accepted = true,
                Changed = false,
                NoChange = true,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = D22FeedbackCodes.DuplicateCommand,
                TargetPanelId = targetPanelId ?? string.Empty,
                State = Snapshot(),
            };
        }

        private D22PanelTransitionResult Reject(
            D22PanelTransitionOperation operation,
            D22PanelTransitionFailureCode code,
            string reasonCode,
            string targetPanelId,
            bool terminal = false)
        {
            return new D22PanelTransitionResult
            {
                Operation = operation,
                Status = terminal ? D22PanelTransitionStatus.TerminalError :
                    (code == D22PanelTransitionFailureCode.CloseNotAllowed
                        ? D22PanelTransitionStatus.Blocked
                        : D22PanelTransitionStatus.Rejected),
                Accepted = false,
                Changed = false,
                NoChange = false,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                FeedbackCode = code == D22PanelTransitionFailureCode.CloseNotAllowed
                    ? D22FeedbackCodes.PermissionDenied
                    : D22FeedbackCodes.InvalidRequest,
                TargetPanelId = targetPanelId ?? string.Empty,
                State = Snapshot(),
            };
        }
    }
}
