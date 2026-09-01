// ============================================================
// D22PanelExclusivityRules —— D22-L04 面板并存/互斥纯规则（纯 C#）
//
// 规则只返回允许、冲突和需要强制关闭的逻辑 ID；它不打开/关闭 Canvas，
// 不修改焦点状态，也不触碰任何业务 owner 的事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>面板并存策略。</summary>
    public enum D22PanelExclusivityMode
    {
        /// <summary>不参加面板组。</summary>
        None = 0,
        /// <summary>可以与其它逻辑面板并存。</summary>
        Independent = 1,
        /// <summary>同一屏幕的导航面板互斥。</summary>
        ExclusiveWithinScreen = 2,
        /// <summary>全局只能有一个同类阻塞 owner。</summary>
        ExclusiveGlobal = 3,
        /// <summary>只读叠加层，可并存但不能接收命令。</summary>
        ReadOnlyOverlay = 4,
    }

    /// <summary>面板规则的确定性失败码。</summary>
    public enum D22PanelRuleFailureCode
    {
        /// <summary>允许打开。</summary>
        None = 0,
        /// <summary>候选请求为空。</summary>
        MissingCandidate = 1,
        /// <summary>候选面板未登记。</summary>
        UnknownPanel = 2,
        /// <summary>候选屏幕未登记。</summary>
        UnknownScreen = 3,
        /// <summary>候选面板与屏幕上下文不一致。</summary>
        ScreenContextMismatch = 4,
        /// <summary>当前打开集合为空引用或包含非法项。</summary>
        InvalidOpenContext = 5,
        /// <summary>同一面板已经打开。</summary>
        PanelAlreadyOpen = 6,
        /// <summary>已有阻塞模态拥有唯一命令焦点。</summary>
        BlockingModalConflict = 7,
        /// <summary>子模态父关系缺失或未获父模态授权。</summary>
        InvalidChildModal = 8,
        /// <summary>面板 owner 缺失。</summary>
        MissingOwner = 9,
        /// <summary>打开集合本身含重复面板。</summary>
        DuplicateOpenPanel = 10,
        /// <summary>候选面板不是可命令目标。</summary>
        CommandTargetUnavailable = 11,
    }

    /// <summary>当前已打开面板的逻辑状态；只含稳定 ID。</summary>
    [Serializable]
    public sealed class D22OpenPanelState
    {
        /// <summary>面板/模态/叠加层稳定 ID。</summary>
        public string PanelId = string.Empty;
        /// <summary>所属屏幕稳定 ID。</summary>
        public string ScreenId = string.Empty;
        /// <summary>面板 owner；普通面板可为空。</summary>
        public string OwnerId = string.Empty;
        /// <summary>父模态或父面板 ID。</summary>
        public string ParentPanelId = string.Empty;
        /// <summary>是否仍处于打开状态。</summary>
        public bool IsOpen = true;

        /// <summary>返回防御性副本。</summary>
        public D22OpenPanelState Clone()
        {
            return new D22OpenPanelState
            {
                PanelId = PanelId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                ParentPanelId = ParentPanelId ?? string.Empty,
                IsOpen = IsOpen,
            };
        }
    }

    /// <summary>面板打开请求；规则不执行请求。</summary>
    [Serializable]
    public sealed class D22PanelOpenRequest
    {
        /// <summary>候选面板稳定 ID。</summary>
        public string PanelId = string.Empty;
        /// <summary>候选屏幕 ID；为空时使用面板登记的父屏幕。</summary>
        public string ScreenId = string.Empty;
        /// <summary>模态 owner；模态请求必填。</summary>
        public string OwnerId = string.Empty;
        /// <summary>子模态显式父面板/模态 ID。</summary>
        public string ParentPanelId = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22PanelOpenRequest Clone()
        {
            return new D22PanelOpenRequest
            {
                PanelId = PanelId ?? string.Empty,
                ScreenId = ScreenId ?? string.Empty,
                OwnerId = OwnerId ?? string.Empty,
                ParentPanelId = ParentPanelId ?? string.Empty,
            };
        }
    }

    /// <summary>面板规则上下文；调用方必须以快照形式提供打开集合。</summary>
    [Serializable]
    public sealed class D22PanelRuleContext
    {
        /// <summary>当前活动屏幕；为空表示未提供屏幕上下文。</summary>
        public string ActiveScreenId = string.Empty;
        /// <summary>当前已打开面板集合。</summary>
        public List<D22OpenPanelState> OpenPanels = new List<D22OpenPanelState>();

        /// <summary>返回深拷贝。</summary>
        public D22PanelRuleContext Clone()
        {
            var result = new D22PanelRuleContext
            {
                ActiveScreenId = ActiveScreenId ?? string.Empty,
            };
            if (OpenPanels != null)
            {
                foreach (var panel in OpenPanels)
                    result.OpenPanels.Add(panel == null ? null : panel.Clone());
            }
            return result;
        }
    }

    /// <summary>面板规则决策；ForceClosePanelIds 是纯计划，不是已执行的关闭。</summary>
    [Serializable]
    public sealed class D22PanelRuleDecision
    {
        /// <summary>候选是否可以打开。</summary>
        public bool Allowed;
        /// <summary>候选已经打开，重复请求可安全视为无变化。</summary>
        public bool NoChange;
        /// <summary>是否需要调用方先按列表强制关闭面板。</summary>
        public bool RequiresForceClose;
        /// <summary>稳定失败码；Allowed=true 时为 None。</summary>
        public D22PanelRuleFailureCode FailureCode;
        /// <summary>机器可读规则原因 key。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>候选面板 ID。</summary>
        public string CandidatePanelId = string.Empty;
        /// <summary>第一个确定性冲突面板 ID。</summary>
        public string ConflictPanelId = string.Empty;
        /// <summary>需要先关闭的面板 ID，按稳定序排序。</summary>
        public List<string> ForceClosePanelIds = new List<string>();

        /// <summary>兼容“PanelsToClose”语义的只读访问。</summary>
        public IReadOnlyList<string> PanelsToClose
        {
            get { return new ReadOnlyCollection<string>(new List<string>(ForceClosePanelIds)); }
        }

        /// <summary>返回深拷贝。</summary>
        public D22PanelRuleDecision Clone()
        {
            var result = new D22PanelRuleDecision
            {
                Allowed = Allowed,
                NoChange = NoChange,
                RequiresForceClose = RequiresForceClose,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                CandidatePanelId = CandidatePanelId ?? string.Empty,
                ConflictPanelId = ConflictPanelId ?? string.Empty,
            };
            if (ForceClosePanelIds != null)
                result.ForceClosePanelIds.AddRange(ForceClosePanelIds);
            return result;
        }
    }

    /// <summary>
    /// D22-L04 面板互斥规则目录。它只评估结构，不改变任何集合。
    /// </summary>
    public static class D22PanelExclusivityRules
    {
        /// <summary>面板规则版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>规则 owner。</summary>
        public const string OwnerSystem = "D22PanelExclusivityRules";

        /// <summary>以空上下文评估候选。</summary>
        public static D22PanelRuleDecision Evaluate(D22PanelOpenRequest candidate)
        {
            return Evaluate(candidate, new D22PanelRuleContext());
        }

        /// <summary>评估候选与已打开面板集合是否可并存。</summary>
        public static D22PanelRuleDecision Evaluate(
            D22PanelOpenRequest candidate,
            IEnumerable<D22OpenPanelState> openPanels)
        {
            var context = new D22PanelRuleContext();
            if (openPanels == null)
            {
                context.OpenPanels = null;
            }
            else
            {
                foreach (var panel in openPanels)
                    context.OpenPanels.Add(panel == null ? null : panel.Clone());
            }
            return Evaluate(candidate, context);
        }

        /// <summary>评估候选与完整逻辑上下文是否可并存。</summary>
        public static D22PanelRuleDecision Evaluate(
            D22PanelOpenRequest candidate,
            D22PanelRuleContext context)
        {
            if (candidate == null)
                return Reject(D22PanelRuleFailureCode.MissingCandidate, "d22.panel.missing_candidate", string.Empty);
            if (string.IsNullOrEmpty(candidate.PanelId))
                return Reject(D22PanelRuleFailureCode.UnknownPanel, "d22.panel.unknown_panel", candidate.PanelId);

            D22PanelDefinition definition;
            if (!D22ScreenContracts.TryGetPanelDefinition(candidate.PanelId, out definition))
                return Reject(D22PanelRuleFailureCode.UnknownPanel, "d22.panel.unknown_panel", candidate.PanelId);

            string effectiveScreenId = string.IsNullOrEmpty(candidate.ScreenId)
                ? definition.ParentScreenId
                : candidate.ScreenId;
            if (!string.IsNullOrEmpty(effectiveScreenId) &&
                !D22ScreenContracts.IsKnownScreenId(effectiveScreenId))
                return Reject(D22PanelRuleFailureCode.UnknownScreen, "d22.panel.unknown_screen", candidate.PanelId);
            if (!string.IsNullOrEmpty(definition.ParentScreenId) &&
                !string.IsNullOrEmpty(candidate.ScreenId) &&
                !string.Equals(definition.ParentScreenId, candidate.ScreenId, StringComparison.Ordinal))
                return Reject(D22PanelRuleFailureCode.ScreenContextMismatch, "d22.panel.screen_context_mismatch", candidate.PanelId);

            if (context == null || context.OpenPanels == null)
                return Reject(D22PanelRuleFailureCode.InvalidOpenContext, "d22.panel.invalid_open_context", candidate.PanelId);
            if (!string.IsNullOrEmpty(context.ActiveScreenId) &&
                !D22ScreenContracts.IsKnownScreenId(context.ActiveScreenId))
                return Reject(D22PanelRuleFailureCode.UnknownScreen, "d22.panel.unknown_screen", candidate.PanelId);
            if (!string.IsNullOrEmpty(context.ActiveScreenId) &&
                !string.IsNullOrEmpty(effectiveScreenId) &&
                !string.Equals(context.ActiveScreenId, effectiveScreenId, StringComparison.Ordinal))
                return Reject(D22PanelRuleFailureCode.ScreenContextMismatch, "d22.panel.screen_context_mismatch", candidate.PanelId);

            var open = new List<D22OpenPanelState>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var state in context.OpenPanels)
            {
                if (state == null || !state.IsOpen || string.IsNullOrEmpty(state.PanelId))
                    continue;
                D22PanelDefinition stateDefinition;
                if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out stateDefinition))
                    return Reject(D22PanelRuleFailureCode.InvalidOpenContext, "d22.panel.invalid_open_context", candidate.PanelId);
                if (!seen.Add(state.PanelId))
                    return Reject(D22PanelRuleFailureCode.DuplicateOpenPanel, "d22.panel.duplicate_open_panel", state.PanelId);
                if (!string.IsNullOrEmpty(state.ScreenId) && !D22ScreenContracts.IsKnownScreenId(state.ScreenId))
                    return Reject(D22PanelRuleFailureCode.InvalidOpenContext, "d22.panel.invalid_open_context", state.PanelId);
                if (!string.IsNullOrEmpty(stateDefinition.ParentScreenId) &&
                    !string.IsNullOrEmpty(state.ScreenId) &&
                    !string.Equals(stateDefinition.ParentScreenId, state.ScreenId, StringComparison.Ordinal))
                    return Reject(D22PanelRuleFailureCode.InvalidOpenContext, "d22.panel.invalid_open_context", state.PanelId);
                if (stateDefinition.SurfaceKind == D22SurfaceKind.Modal &&
                    (string.IsNullOrEmpty(state.OwnerId) || !D22ScreenContracts.IsStableId(state.OwnerId)))
                    return Reject(D22PanelRuleFailureCode.InvalidOpenContext, "d22.panel.invalid_open_context", state.PanelId);
                open.Add(state);
            }

            foreach (var state in open)
            {
                if (string.Equals(state.PanelId, candidate.PanelId, StringComparison.Ordinal))
                {
                    return new D22PanelRuleDecision
                    {
                        Allowed = false,
                        NoChange = true,
                        RequiresForceClose = false,
                        FailureCode = D22PanelRuleFailureCode.PanelAlreadyOpen,
                        ReasonCode = "d22.panel.already_open",
                        CandidatePanelId = candidate.PanelId,
                    };
                }
            }

            if (definition.SurfaceKind == D22SurfaceKind.Overlay)
                return Allow(candidate.PanelId, new List<string>());

            if (definition.SurfaceKind == D22SurfaceKind.Modal)
                return EvaluateModal(candidate, definition, open);

            if (definition.IsNavigationPanel)
            {
                var forceClose = new List<string>();
                foreach (var state in open)
                {
                    D22PanelDefinition existingDefinition;
                    if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out existingDefinition))
                        continue;
                    if (existingDefinition.IsNavigationPanel &&
                        string.Equals(existingDefinition.ParentScreenId, effectiveScreenId, StringComparison.Ordinal))
                        forceClose.Add(state.PanelId);
                }
                forceClose.Sort(StringComparer.Ordinal);
                return Allow(candidate.PanelId, forceClose);
            }

            return Allow(candidate.PanelId, new List<string>());
        }

        /// <summary>返回候选面板的并存策略。</summary>
        public static D22PanelExclusivityMode GetMode(string panelId)
        {
            D22PanelDefinition definition;
            if (!D22ScreenContracts.TryGetPanelDefinition(panelId, out definition))
                return D22PanelExclusivityMode.None;
            if (definition.SurfaceKind == D22SurfaceKind.Overlay)
                return D22PanelExclusivityMode.ReadOnlyOverlay;
            if (definition.SurfaceKind == D22SurfaceKind.Modal)
                return definition.ModalKind == D22ModalKind.NonBlocking
                    ? D22PanelExclusivityMode.Independent
                    : D22PanelExclusivityMode.ExclusiveGlobal;
            if (definition.IsNavigationPanel)
                return D22PanelExclusivityMode.ExclusiveWithinScreen;
            return D22PanelExclusivityMode.Independent;
        }

        /// <summary>在不改变上下文的前提下给出可并存判断。</summary>
        public static bool CanCoexist(string firstPanelId, string secondPanelId)
        {
            if (string.IsNullOrEmpty(firstPanelId) || string.IsNullOrEmpty(secondPanelId))
                return false;
            if (string.Equals(firstPanelId, secondPanelId, StringComparison.Ordinal))
                return false;

            D22PanelDefinition first;
            D22PanelDefinition second;
            if (!D22ScreenContracts.TryGetPanelDefinition(firstPanelId, out first) ||
                !D22ScreenContracts.TryGetPanelDefinition(secondPanelId, out second))
                return false;
            if (first.SurfaceKind == D22SurfaceKind.Overlay || second.SurfaceKind == D22SurfaceKind.Overlay)
                return true;
            if (first.SurfaceKind == D22SurfaceKind.Modal && second.SurfaceKind == D22SurfaceKind.Modal)
            {
                if (first.ModalKind == D22ModalKind.NonBlocking || second.ModalKind == D22ModalKind.NonBlocking)
                    return true;
                return false;
            }
            if (first.IsNavigationPanel && second.IsNavigationPanel &&
                string.Equals(first.ParentScreenId, second.ParentScreenId, StringComparison.Ordinal))
                return false;
            return true;
        }

        private static D22PanelRuleDecision EvaluateModal(
            D22PanelOpenRequest candidate,
            D22PanelDefinition definition,
            List<D22OpenPanelState> open)
        {
            if (string.IsNullOrEmpty(candidate.OwnerId) || !D22ScreenContracts.IsStableId(candidate.OwnerId))
                return Reject(D22PanelRuleFailureCode.MissingOwner, "d22.panel.missing_owner", candidate.PanelId);

            D22OpenPanelState blocking = null;
            foreach (var state in open)
            {
                D22PanelDefinition existing;
                if (!D22ScreenContracts.TryGetPanelDefinition(state.PanelId, out existing))
                    continue;
                if (existing.SurfaceKind == D22SurfaceKind.Modal &&
                    existing.ModalKind != D22ModalKind.NonBlocking)
                {
                    blocking = state;
                    break;
                }
            }

            if (definition.ModalKind == D22ModalKind.Blocking && blocking != null)
                return Reject(D22PanelRuleFailureCode.BlockingModalConflict, "d22.panel.blocking_modal_conflict", candidate.PanelId, blocking.PanelId);

            if (definition.ModalKind == D22ModalKind.ChildBlocking)
            {
                if (blocking == null ||
                    string.IsNullOrEmpty(candidate.ParentPanelId) ||
                    !string.Equals(candidate.ParentPanelId, blocking.PanelId, StringComparison.Ordinal))
                    return Reject(D22PanelRuleFailureCode.InvalidChildModal, "d22.panel.invalid_child_modal", candidate.PanelId);

                D22PanelDefinition parent;
                if (!D22ScreenContracts.TryGetPanelDefinition(blocking.PanelId, out parent) || !parent.AllowsChildModal)
                    return Reject(D22PanelRuleFailureCode.InvalidChildModal, "d22.panel.invalid_child_modal", candidate.PanelId);
            }
            return Allow(candidate.PanelId, new List<string>());
        }

        private static D22PanelRuleDecision Allow(string candidatePanelId, List<string> forceClosePanelIds)
        {
            return new D22PanelRuleDecision
            {
                Allowed = true,
                NoChange = false,
                RequiresForceClose = forceClosePanelIds != null && forceClosePanelIds.Count > 0,
                FailureCode = D22PanelRuleFailureCode.None,
                ReasonCode = forceClosePanelIds != null && forceClosePanelIds.Count > 0
                    ? "d22.panel.replace_navigation_panel"
                    : "d22.panel.open_allowed",
                CandidatePanelId = candidatePanelId ?? string.Empty,
                ForceClosePanelIds = forceClosePanelIds ?? new List<string>(),
            };
        }

        private static D22PanelRuleDecision Reject(
            D22PanelRuleFailureCode code,
            string reasonCode,
            string candidatePanelId,
            string conflictPanelId = "")
        {
            return new D22PanelRuleDecision
            {
                Allowed = false,
                NoChange = false,
                RequiresForceClose = false,
                FailureCode = code,
                ReasonCode = reasonCode ?? string.Empty,
                CandidatePanelId = candidatePanelId ?? string.Empty,
                ConflictPanelId = conflictPanelId ?? string.Empty,
            };
        }
    }
}
