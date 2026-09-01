// ============================================================
// D22FocusStateMachine —— D22-L03 逻辑焦点状态机（纯 C#）
//
// 这里保存的是屏幕/面板/控制目标的逻辑 ID。Runtime 输入适配器只提交
// 已归一化的意图；本文件不读取设备、不调用 EventSystem，也不修改业务事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>输入适配器可以提出的逻辑模式，不代表设备当前状态。</summary>
    public enum D22InputModeIntent
    {
        /// <summary>尚未提供模式。</summary>
        Unknown = 0,
        /// <summary>键鼠语义。</summary>
        KeyboardMouse = 1,
        /// <summary>手柄语义。</summary>
        Gamepad = 2,
        /// <summary>触控语义。</summary>
        Touch = 3,
        /// <summary>自动导航语义。</summary>
        Navigation = 4,
    }

    /// <summary>焦点状态机的稳定失败码。</summary>
    public enum D22FocusFailureCode
    {
        /// <summary>操作成功。</summary>
        None = 0,
        /// <summary>屏幕 ID 缺失。</summary>
        MissingScreenId = 1,
        /// <summary>屏幕 ID 未登记。</summary>
        UnknownScreenId = 2,
        /// <summary>面板 ID 缺失。</summary>
        MissingPanelId = 3,
        /// <summary>面板 ID 未登记。</summary>
        UnknownPanelId = 4,
        /// <summary>面板不属于当前屏幕。</summary>
        PanelContextMismatch = 5,
        /// <summary>当前没有活动屏幕。</summary>
        MissingActiveScreen = 6,
        /// <summary>控制目标 ID 缺失或不稳定。</summary>
        InvalidFocusControlId = 7,
        /// <summary>模态 ID 缺失。</summary>
        MissingModalId = 8,
        /// <summary>目标不是已登记模态。</summary>
        UnknownModalId = 9,
        /// <summary>同一模态已在栈中。</summary>
        ModalAlreadyOpen = 10,
        /// <summary>已有阻塞模态拥有唯一焦点。</summary>
        BlockingModalAlreadyOwned = 11,
        /// <summary>子模态没有合法的父阻塞模态。</summary>
        InvalidChildModal = 12,
        /// <summary>模态 owner 缺失。</summary>
        MissingModalOwner = 13,
        /// <summary>模态栈为空。</summary>
        ModalStackEmpty = 14,
        /// <summary>要关闭的模态不是栈顶。</summary>
        ModalNotTop = 15,
        /// <summary>当前上下文无法接收焦点。</summary>
        FocusContextUnavailable = 16,
        /// <summary>没有可用的确定性回退焦点。</summary>
        NoFallbackFocus = 17,
        /// <summary>输入的模式值未知。</summary>
        InvalidInputMode = 18,
        /// <summary>关闭请求缺少稳定原因。</summary>
        InvalidCloseReason = 19,
    }

    /// <summary>保存一个可恢复的逻辑焦点位置，不包含 Unity 对象或设备状态。</summary>
    [Serializable]
    public sealed class D22FocusToken
    {
        /// <summary>焦点所在屏幕。</summary>
        public string ScreenId = string.Empty;
        /// <summary>焦点所在普通面板。</summary>
        public string PanelId = string.Empty;
        /// <summary>控件稳定 ID。</summary>
        public string ControlId = D22FocusControlIds.None;
        /// <summary>焦点提出的逻辑输入模式。</summary>
        public D22InputModeIntent InputMode = D22InputModeIntent.Unknown;

        /// <summary>返回防御性副本。</summary>
        public D22FocusToken Clone()
        {
            return new D22FocusToken
            {
                ScreenId = ScreenId ?? string.Empty,
                PanelId = PanelId ?? string.Empty,
                ControlId = ControlId ?? D22FocusControlIds.None,
                InputMode = InputMode,
            };
        }

        /// <summary>返回确定性 token 字符串，供读模型或审计关联。</summary>
        public string ToStableString()
        {
            return (ScreenId ?? string.Empty) + "|" +
                   (PanelId ?? string.Empty) + "|" +
                   (ControlId ?? D22FocusControlIds.None) + "|" +
                   InputMode;
        }
    }

    /// <summary>模态栈中的一帧；记录打开前焦点，关闭后按此帧恢复。</summary>
    [Serializable]
    public sealed class D22ModalFocusEntry
    {
        /// <summary>模态稳定 ID。</summary>
        public string ModalId = string.Empty;
        /// <summary>提出/拥有该模态的逻辑 owner ID。</summary>
        public string OwnerId = string.Empty;
        /// <summary>模态阻塞等级。</summary>
        public D22ModalKind ModalKind = D22ModalKind.Blocking;
        /// <summary>显式父模态 ID；非子模态为空。</summary>
        public string ParentModalId = string.Empty;
        /// <summary>该模态当前焦点。</summary>
        public string FocusedControlId = D22FocusControlIds.None;
        /// <summary>失焦时的确定性回退焦点。</summary>
        public string FallbackFocusControlId = D22FocusControlIds.None;
        /// <summary>打开前的逻辑焦点帧。</summary>
        public D22FocusToken ReturnFocus = new D22FocusToken();

        /// <summary>返回深拷贝。</summary>
        public D22ModalFocusEntry Clone()
        {
            return new D22ModalFocusEntry
            {
                ModalId = ModalId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                ModalKind = ModalKind,
                ParentModalId = ParentModalId ?? string.Empty,
                FocusedControlId = FocusedControlId ?? D22FocusControlIds.None,
                FallbackFocusControlId = FallbackFocusControlId ?? D22FocusControlIds.None,
                ReturnFocus = ReturnFocus == null ? new D22FocusToken() : ReturnFocus.Clone(),
            };
        }
    }

    /// <summary>打开模态所需的逻辑输入。</summary>
    [Serializable]
    public sealed class D22ModalFocusRequest
    {
        /// <summary>模态稳定 ID。</summary>
        public string ModalId = string.Empty;
        /// <summary>模态 owner 稳定 ID。</summary>
        public string OwnerId = string.Empty;
        /// <summary>模态打开后的初始焦点；为空时使用契约默认值。</summary>
        public string FocusedControlId = string.Empty;
        /// <summary>模态失焦回退控制；为空时使用契约默认值。</summary>
        public string FallbackFocusControlId = string.Empty;
        /// <summary>子模态显式声明的父模态 ID。</summary>
        public string ParentModalId = string.Empty;
        /// <summary>调用方声明的屏幕上下文；为空表示使用当前屏幕。</summary>
        public string ScreenId = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22ModalFocusRequest Clone()
        {
            return new D22ModalFocusRequest
            {
                ModalId = ModalId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                FocusedControlId = FocusedControlId ?? string.Empty,
                FallbackFocusControlId = FallbackFocusControlId ?? string.Empty,
                ParentModalId = ParentModalId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
            };
        }
    }

    /// <summary>焦点状态机的不可变读模型。</summary>
    [Serializable]
    public sealed class D22FocusSnapshot
    {
        /// <summary>快照版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>逻辑快照版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;
        /// <summary>当前屏幕稳定 ID。</summary>
        public string ActiveScreenId = string.Empty;
        /// <summary>当前普通面板稳定 ID。</summary>
        public string ActivePanelId = string.Empty;
        /// <summary>当前真正取得命令焦点的模态 ID；无则为空。</summary>
        public string ActiveModalId = string.Empty;
        /// <summary>当前焦点控制 ID。</summary>
        public string FocusedControlId = D22FocusControlIds.None;
        /// <summary>最近保存的返回焦点 token。</summary>
        public string ReturnFocusToken = string.Empty;
        /// <summary>逻辑输入模式意图。</summary>
        public D22InputModeIntent InputMode = D22InputModeIntent.Unknown;
        /// <summary>模态栈快照，栈顶位于末尾。</summary>
        public List<D22ModalFocusEntry> ModalStack = new List<D22ModalFocusEntry>();

        /// <summary>返回不共享内部集合的深拷贝。</summary>
        public D22FocusSnapshot Clone()
        {
            var result = new D22FocusSnapshot
            {
                SchemaVersion = SchemaVersion,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                ActiveModalId = ActiveModalId ?? string.Empty,
                FocusedControlId = FocusedControlId ?? D22FocusControlIds.None,
                ReturnFocusToken = ReturnFocusToken ?? string.Empty,
                InputMode = InputMode,
            };
            if (ModalStack != null)
            {
                foreach (var entry in ModalStack)
                    if (entry != null) result.ModalStack.Add(entry.Clone());
            }
            return result;
        }
    }

    /// <summary>焦点操作结果；失败不改变状态，成功总是返回完整快照。</summary>
    [Serializable]
    public sealed class D22FocusResult
    {
        /// <summary>操作是否接受。</summary>
        public bool Accepted;
        /// <summary>操作是否真正改变了焦点状态。</summary>
        public bool Changed;
        /// <summary>稳定失败码；成功为 None。</summary>
        public D22FocusFailureCode FailureCode;
        /// <summary>机器可读原因 key，不承载用户文案。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>本次操作影响的模态 ID。</summary>
        public string ModalId = string.Empty;
        /// <summary>关闭操作的稳定原因 ID。</summary>
        public string CloseReasonId = D22CloseReasonIds.None;
        /// <summary>完整防御性焦点快照。</summary>
        public D22FocusSnapshot State = new D22FocusSnapshot();

        /// <summary>返回深拷贝。</summary>
        public D22FocusResult Clone()
        {
            return new D22FocusResult
            {
                Accepted = Accepted,
                Changed = Changed,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                ModalId = ModalId ?? string.Empty,
                CloseReasonId = CloseReasonId ?? D22CloseReasonIds.None,
                State = State == null ? new D22FocusSnapshot() : State.Clone(),
            };
        }
    }

    /// <summary>
    /// D22-L03 纯逻辑焦点状态机。它是唯一的 D22 焦点状态写者，
    /// 但不拥有任何经济、航行、角色、存档或 Runtime 输入事实。
    /// </summary>
    public sealed class D22FocusStateMachine
    {
        private readonly List<D22ModalFocusEntry> modalStack = new List<D22ModalFocusEntry>();
        private string activeScreenId = string.Empty;
        private string activePanelId = string.Empty;
        private string focusedControlId = D22FocusControlIds.None;
        private string returnFocusToken = string.Empty;
        private D22InputModeIntent inputMode = D22InputModeIntent.Unknown;
        private string screenFallbackFocusControlId = D22FocusControlIds.None;
        private string panelFallbackFocusControlId = D22FocusControlIds.None;

        /// <summary>创建空上下文；必须先通过 ActivateScreen 建立逻辑屏幕。</summary>
        public D22FocusStateMachine() { }

        /// <summary>创建并尝试激活指定的逻辑屏幕；非法参数保持空上下文。</summary>
        public D22FocusStateMachine(
            string initialScreenId,
            string initialPanelId,
            string initialFocusControlId = "",
            string fallbackFocusControlId = "")
        {
            ActivateScreen(initialScreenId, initialPanelId, initialFocusControlId, fallbackFocusControlId);
        }

        /// <summary>返回当前完整防御性快照。</summary>
        public D22FocusSnapshot Snapshot()
        {
            var result = new D22FocusSnapshot
            {
                ActiveScreenId = activeScreenId,
                ActivePanelId = activePanelId,
                ActiveModalId = GetActiveBlockingModalId(),
                FocusedControlId = focusedControlId,
                ReturnFocusToken = returnFocusToken,
                InputMode = inputMode,
            };
            foreach (var entry in modalStack)
                result.ModalStack.Add(entry.Clone());
            return result;
        }

        /// <summary>当前是否存在活动屏幕。</summary>
        public bool HasActiveScreen
        {
            get { return !string.IsNullOrEmpty(activeScreenId); }
        }

        /// <summary>当前命令焦点目标；阻塞模态优先于普通面板。</summary>
        public string CommandFocusTargetId
        {
            get
            {
                var modal = GetActiveBlockingModal();
                return modal == null ? focusedControlId : modal.FocusedControlId;
            }
        }

        /// <summary>切换逻辑屏幕，并清除旧屏幕上不能恢复的模态栈。</summary>
        public D22FocusResult ActivateScreen(
            string screenId,
            string panelId,
            string focusControlId = "",
            string fallbackFocusControlId = "",
            D22InputModeIntent requestedInputMode = D22InputModeIntent.Unknown)
        {
            D22ScreenDefinition screen;
            if (string.IsNullOrEmpty(screenId))
                return Reject(D22FocusFailureCode.MissingScreenId, "d22.focus.missing_screen_id");
            if (!D22ScreenContracts.TryGetScreenDefinition(screenId, out screen))
                return Reject(D22FocusFailureCode.UnknownScreenId, "d22.focus.unknown_screen_id");

            D22PanelDefinition panel;
            if (string.IsNullOrEmpty(panelId))
                return Reject(D22FocusFailureCode.MissingPanelId, "d22.focus.missing_panel_id");
            if (!D22ScreenContracts.TryGetPanelDefinition(panelId, out panel))
                return Reject(D22FocusFailureCode.UnknownPanelId, "d22.focus.unknown_panel_id");
            if (panel.SurfaceKind != D22SurfaceKind.Panel ||
                !string.Equals(panel.ParentScreenId, screenId, StringComparison.Ordinal))
                return Reject(D22FocusFailureCode.PanelContextMismatch, "d22.focus.panel_context_mismatch");
            if (!IsValidInputMode(requestedInputMode))
                return Reject(D22FocusFailureCode.InvalidInputMode, "d22.focus.invalid_input_mode");

            string resolvedFocus = string.IsNullOrEmpty(focusControlId)
                ? panel.DefaultFocusControlId
                : focusControlId;
            string resolvedFallback = string.IsNullOrEmpty(fallbackFocusControlId)
                ? panel.FallbackFocusControlId
                : fallbackFocusControlId;
            if (!IsOptionalFocusId(resolvedFocus) || !IsOptionalFocusId(resolvedFallback))
                return Reject(D22FocusFailureCode.InvalidFocusControlId, "d22.focus.invalid_focus_control_id");

            bool changed = !string.Equals(activeScreenId, screenId, StringComparison.Ordinal) ||
                           !string.Equals(activePanelId, panelId, StringComparison.Ordinal) ||
                           !string.Equals(focusedControlId, resolvedFocus, StringComparison.Ordinal) ||
                           inputMode != requestedInputMode || modalStack.Count != 0;
            modalStack.Clear();
            activeScreenId = screenId;
            activePanelId = panelId;
            focusedControlId = resolvedFocus;
            screenFallbackFocusControlId = string.IsNullOrEmpty(fallbackFocusControlId)
                ? screen.FallbackFocusControlId
                : fallbackFocusControlId;
            panelFallbackFocusControlId = resolvedFallback;
            inputMode = requestedInputMode;
            returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
            return Accept(changed, D22FocusFailureCode.None, string.Empty, string.Empty, D22CloseReasonIds.None);
        }

        /// <summary>切换当前普通面板；模态栈存在时仍保留其恢复帧。</summary>
        public D22FocusResult SetActivePanel(
            string panelId,
            string focusControlId = "",
            string fallbackFocusControlId = "")
        {
            if (!HasActiveScreen)
                return Reject(D22FocusFailureCode.MissingActiveScreen, "d22.focus.missing_active_screen");
            D22PanelDefinition panel;
            if (string.IsNullOrEmpty(panelId))
                return Reject(D22FocusFailureCode.MissingPanelId, "d22.focus.missing_panel_id");
            if (!D22ScreenContracts.TryGetPanelDefinition(panelId, out panel))
                return Reject(D22FocusFailureCode.UnknownPanelId, "d22.focus.unknown_panel_id");
            if (panel.SurfaceKind != D22SurfaceKind.Panel ||
                !string.Equals(panel.ParentScreenId, activeScreenId, StringComparison.Ordinal))
                return Reject(D22FocusFailureCode.PanelContextMismatch, "d22.focus.panel_context_mismatch");

            string resolvedFocus = string.IsNullOrEmpty(focusControlId)
                ? panel.DefaultFocusControlId
                : focusControlId;
            string resolvedFallback = string.IsNullOrEmpty(fallbackFocusControlId)
                ? panel.FallbackFocusControlId
                : fallbackFocusControlId;
            if (!IsOptionalFocusId(resolvedFocus) || !IsOptionalFocusId(resolvedFallback))
                return Reject(D22FocusFailureCode.InvalidFocusControlId, "d22.focus.invalid_focus_control_id");

            bool changed = !string.Equals(activePanelId, panelId, StringComparison.Ordinal) ||
                           !string.Equals(focusedControlId, resolvedFocus, StringComparison.Ordinal);
            activePanelId = panelId;
            focusedControlId = resolvedFocus;
            panelFallbackFocusControlId = resolvedFallback;
            returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
            return Accept(changed, D22FocusFailureCode.None, string.Empty, string.Empty, D22CloseReasonIds.None);
        }

        /// <summary>提交一个逻辑焦点控制目标，不接触设备或 UGUI 选择。</summary>
        public D22FocusResult SetFocus(
            string controlId,
            D22InputModeIntent requestedInputMode = D22InputModeIntent.Unknown)
        {
            if (!HasActiveScreen)
                return Reject(D22FocusFailureCode.MissingActiveScreen, "d22.focus.missing_active_screen");
            if (!IsStableFocusId(controlId))
                return Reject(D22FocusFailureCode.InvalidFocusControlId, "d22.focus.invalid_focus_control_id");
            if (!IsValidInputMode(requestedInputMode))
                return Reject(D22FocusFailureCode.InvalidInputMode, "d22.focus.invalid_input_mode");

            var modal = GetActiveBlockingModal();
            bool changed;
            if (modal != null)
            {
                changed = !string.Equals(modal.FocusedControlId, controlId, StringComparison.Ordinal) ||
                          (requestedInputMode != D22InputModeIntent.Unknown && inputMode != requestedInputMode);
                modal.FocusedControlId = controlId;
            }
            else
            {
                changed = !string.Equals(focusedControlId, controlId, StringComparison.Ordinal) ||
                          (requestedInputMode != D22InputModeIntent.Unknown && inputMode != requestedInputMode);
                focusedControlId = controlId;
                returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
            }
            if (requestedInputMode != D22InputModeIntent.Unknown)
                inputMode = requestedInputMode;
            return Accept(changed, D22FocusFailureCode.None, string.Empty, modal == null ? string.Empty : modal.ModalId, D22CloseReasonIds.None);
        }

        /// <summary>打开一个模态；阻塞模态只能有一个，子模态必须显式声明父关系。</summary>
        public D22FocusResult OpenModal(D22ModalFocusRequest request)
        {
            if (!HasActiveScreen)
                return Reject(D22FocusFailureCode.MissingActiveScreen, "d22.focus.missing_active_screen");
            if (request == null || string.IsNullOrEmpty(request.ModalId))
                return Reject(D22FocusFailureCode.MissingModalId, "d22.focus.missing_modal_id");

            D22PanelDefinition definition;
            if (!D22ScreenContracts.TryGetPanelDefinition(request.ModalId, out definition) ||
                definition.SurfaceKind != D22SurfaceKind.Modal)
                return Reject(D22FocusFailureCode.UnknownModalId, "d22.focus.unknown_modal_id");
            if (!string.IsNullOrEmpty(request.ScreenId) &&
                !string.Equals(request.ScreenId, activeScreenId, StringComparison.Ordinal))
                return Reject(D22FocusFailureCode.PanelContextMismatch, "d22.focus.panel_context_mismatch");
            if (string.IsNullOrEmpty(request.OwnerId) || !D22ScreenContracts.IsStableId(request.OwnerId))
                return Reject(D22FocusFailureCode.MissingModalOwner, "d22.focus.missing_modal_owner");
            if (ContainsModal(request.ModalId))
                return Reject(D22FocusFailureCode.ModalAlreadyOpen, "d22.focus.modal_already_open");

            if (definition.ModalKind == D22ModalKind.Blocking && GetActiveBlockingModal() != null)
                return Reject(D22FocusFailureCode.BlockingModalAlreadyOwned, "d22.focus.blocking_modal_already_owned");

            var currentBlocking = GetActiveBlockingModal();
            if (definition.ModalKind == D22ModalKind.ChildBlocking)
            {
                if (currentBlocking == null ||
                    !D22ScreenContracts.IsStableId(request.ParentModalId) ||
                    !string.Equals(request.ParentModalId, currentBlocking.ModalId, StringComparison.Ordinal))
                    return Reject(D22FocusFailureCode.InvalidChildModal, "d22.focus.invalid_child_modal");

                D22PanelDefinition parentDefinition;
                if (!D22ScreenContracts.TryGetPanelDefinition(currentBlocking.ModalId, out parentDefinition) ||
                    !parentDefinition.AllowsChildModal)
                    return Reject(D22FocusFailureCode.InvalidChildModal, "d22.focus.invalid_child_modal");
            }

            string modalFocus = string.IsNullOrEmpty(request.FocusedControlId)
                ? definition.DefaultFocusControlId
                : request.FocusedControlId;
            string modalFallback = string.IsNullOrEmpty(request.FallbackFocusControlId)
                ? definition.FallbackFocusControlId
                : request.FallbackFocusControlId;
            if (!IsStableFocusId(modalFocus) || !IsOptionalFocusId(modalFallback))
                return Reject(D22FocusFailureCode.InvalidFocusControlId, "d22.focus.invalid_focus_control_id");

            var entry = new D22ModalFocusEntry
            {
                ModalId = request.ModalId,
                OwnerId = request.OwnerId,
                ModalKind = definition.ModalKind,
                ParentModalId = request.ParentModalId ?? string.Empty,
                FocusedControlId = modalFocus,
                FallbackFocusControlId = modalFallback,
                ReturnFocus = new D22FocusToken
                {
                    ScreenId = activeScreenId,
                    PanelId = activePanelId,
                    ControlId = CommandFocusTargetId,
                    InputMode = inputMode,
                },
            };
            modalStack.Add(entry);
            returnFocusToken = entry.ReturnFocus.ToStableString();
            if (definition.ModalKind != D22ModalKind.NonBlocking)
                focusedControlId = modalFocus;
            return Accept(true, D22FocusFailureCode.None, string.Empty, request.ModalId, D22CloseReasonIds.None);
        }

        /// <summary>使用契约默认焦点打开模态的便捷入口。</summary>
        public D22FocusResult OpenModal(string modalId, string ownerId, string parentModalId = "")
        {
            return OpenModal(new D22ModalFocusRequest
            {
                ModalId = modalId ?? string.Empty,
                OwnerId = ownerId ?? string.Empty,
                ParentModalId = parentModalId ?? string.Empty,
            });
        }

        /// <summary>关闭栈顶模态并恢复打开前的逻辑焦点。</summary>
        public D22FocusResult CloseTopModal(D22CloseReason reason)
        {
            if (modalStack.Count == 0)
                return Reject(D22FocusFailureCode.ModalStackEmpty, "d22.focus.modal_stack_empty");
            if (reason == D22CloseReason.None)
                return Reject(D22FocusFailureCode.InvalidCloseReason, "d22.focus.invalid_close_reason");

            int lastIndex = modalStack.Count - 1;
            D22ModalFocusEntry entry = modalStack[lastIndex];
            modalStack.RemoveAt(lastIndex);
            bool restored = false;
            if (entry.ModalKind != D22ModalKind.NonBlocking)
            {
                restored = RestoreFocus(entry.ReturnFocus, entry.FallbackFocusControlId);
                if (!restored)
                    returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
            }
            else
            {
                restored = true;
            }
            string closeReasonId = D22ScreenContracts.GetCloseReasonId(reason);
            return Accept(true, D22FocusFailureCode.None, string.Empty, entry.ModalId, closeReasonId);
        }

        /// <summary>只有栈顶模态可以显式关闭，避免破坏恢复栈顺序。</summary>
        public D22FocusResult CloseModal(string modalId, D22CloseReason reason)
        {
            if (modalStack.Count == 0)
                return Reject(D22FocusFailureCode.ModalStackEmpty, "d22.focus.modal_stack_empty");
            if (string.IsNullOrEmpty(modalId) ||
                !string.Equals(modalStack[modalStack.Count - 1].ModalId, modalId, StringComparison.Ordinal))
                return Reject(D22FocusFailureCode.ModalNotTop, "d22.focus.modal_not_top");
            return CloseTopModal(reason);
        }

        /// <summary>将焦点恢复到模态/面板/屏幕定义的确定性回退目标。</summary>
        public D22FocusResult HandleFocusLost()
        {
            if (!HasActiveScreen)
                return Reject(D22FocusFailureCode.FocusContextUnavailable, "d22.focus.context_unavailable");

            var modal = GetActiveBlockingModal();
            string fallback = modal == null ? panelFallbackFocusControlId : modal.FallbackFocusControlId;
            if (!IsStableFocusId(fallback))
                fallback = screenFallbackFocusControlId;
            if (!IsStableFocusId(fallback))
                return Reject(D22FocusFailureCode.NoFallbackFocus, "d22.focus.no_fallback_focus");

            if (modal != null)
                modal.FocusedControlId = fallback;
            else
            {
                focusedControlId = fallback;
                returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
            }
            return Accept(true, D22FocusFailureCode.None, "d22.focus.focus_recovered", modal == null ? string.Empty : modal.ModalId, D22CloseReasonIds.FocusLost);
        }

        /// <summary>RecoverFocus 是 HandleFocusLost 的语义别名，不读取任何设备状态。</summary>
        public D22FocusResult RecoverFocus()
        {
            return HandleFocusLost();
        }

        /// <summary>取得当前阻塞模态 owner；没有阻塞模态时返回空。</summary>
        public string ActiveModalOwnerId
        {
            get
            {
                var modal = GetActiveBlockingModal();
                return modal == null ? string.Empty : modal.OwnerId;
            }
        }

        private bool ContainsModal(string modalId)
        {
            foreach (var entry in modalStack)
                if (string.Equals(entry.ModalId, modalId, StringComparison.Ordinal)) return true;
            return false;
        }

        private D22ModalFocusEntry GetActiveBlockingModal()
        {
            for (int index = modalStack.Count - 1; index >= 0; index--)
            {
                if (modalStack[index].ModalKind != D22ModalKind.NonBlocking)
                    return modalStack[index];
            }
            return null;
        }

        private string GetActiveBlockingModalId()
        {
            var modal = GetActiveBlockingModal();
            return modal == null ? string.Empty : modal.ModalId;
        }

        private bool RestoreFocus(D22FocusToken token, string fallbackFocusControlId)
        {
            if (token != null &&
                string.Equals(token.ScreenId, activeScreenId, StringComparison.Ordinal) &&
                string.Equals(token.PanelId, activePanelId, StringComparison.Ordinal) &&
                IsStableFocusId(token.ControlId))
            {
                focusedControlId = token.ControlId;
                inputMode = token.InputMode;
                returnFocusToken = token.ToStableString();
                return true;
            }
            if (IsStableFocusId(fallbackFocusControlId))
            {
                focusedControlId = fallbackFocusControlId;
                returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
                return true;
            }
            if (IsStableFocusId(panelFallbackFocusControlId))
            {
                focusedControlId = panelFallbackFocusControlId;
                returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
                return true;
            }
            if (IsStableFocusId(screenFallbackFocusControlId))
            {
                focusedControlId = screenFallbackFocusControlId;
                returnFocusToken = BuildFocusToken(activeScreenId, activePanelId, focusedControlId);
                return true;
            }
            focusedControlId = D22FocusControlIds.None;
            return false;
        }

        private static bool IsValidInputMode(D22InputModeIntent mode)
        {
            return mode >= D22InputModeIntent.Unknown && mode <= D22InputModeIntent.Navigation;
        }

        private static bool IsStableFocusId(string id)
        {
            return !string.IsNullOrEmpty(id) &&
                   !string.Equals(id, D22FocusControlIds.None, StringComparison.Ordinal) &&
                   D22ScreenContracts.IsStableId(id);
        }

        private static bool IsOptionalFocusId(string id)
        {
            return string.IsNullOrEmpty(id) ||
                   string.Equals(id, D22FocusControlIds.None, StringComparison.Ordinal) ||
                   IsStableFocusId(id);
        }

        private static string BuildFocusToken(string screenId, string panelId, string controlId)
        {
            return (screenId ?? string.Empty) + "|" +
                   (panelId ?? string.Empty) + "|" +
                   (controlId ?? D22FocusControlIds.None);
        }

        private D22FocusResult Accept(
            bool changed,
            D22FocusFailureCode code,
            string reasonCode,
            string modalId,
            string closeReasonId)
        {
            return new D22FocusResult
            {
                Accepted = true,
                Changed = changed,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                ModalId = modalId ?? string.Empty,
                CloseReasonId = closeReasonId ?? D22CloseReasonIds.None,
                State = Snapshot(),
            };
        }

        private D22FocusResult Reject(D22FocusFailureCode code, string reasonCode)
        {
            return new D22FocusResult
            {
                Accepted = false,
                Changed = false,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                State = Snapshot(),
            };
        }
    }
}
