// ============================================================
// D22ScreenContracts —— D22-L02 屏幕、面板、模态与焦点稳定契约（纯 C#）
//
// 这些 ID 是逻辑契约，不是 Canvas、GameObject、Prefab 或显示文本的别名。
// Runtime 适配器可以把它们映射到具体控件，但不能把表现对象反向提升为事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>D22 逻辑表面种类。</summary>
    public enum D22SurfaceKind
    {
        /// <summary>无效占位。</summary>
        None = 0,
        /// <summary>全屏逻辑屏幕。</summary>
        Screen = 1,
        /// <summary>屏幕内面板。</summary>
        Panel = 2,
        /// <summary>需要 modal 生命周期的面板。</summary>
        Modal = 3,
        /// <summary>只读叠加层，不取得业务命令焦点。</summary>
        Overlay = 4,
    }

    /// <summary>模态的逻辑阻塞等级。</summary>
    public enum D22ModalKind
    {
        /// <summary>不是模态。</summary>
        None = 0,
        /// <summary>可与阻塞模态并存但不接收业务命令。</summary>
        NonBlocking = 1,
        /// <summary>取得唯一顶层命令焦点。</summary>
        Blocking = 2,
        /// <summary>只能作为当前阻塞模态的显式子模态。</summary>
        ChildBlocking = 3,
    }

    /// <summary>逻辑焦点层级；数值只表达确定性优先级，不对应 Unity sorting order。</summary>
    public enum D22FocusLayer
    {
        /// <summary>无焦点层。</summary>
        None = 0,
        /// <summary>背景读模型层。</summary>
        Background = 10,
        /// <summary>屏幕层。</summary>
        Screen = 20,
        /// <summary>普通面板层。</summary>
        Panel = 30,
        /// <summary>模态层。</summary>
        Modal = 40,
        /// <summary>只读叠加层；不应抢占命令焦点。</summary>
        Overlay = 50,
    }

    /// <summary>逻辑关闭原因。每个原因都有稳定 ID，不能用用户可见文案替代。</summary>
    public enum D22CloseReason
    {
        /// <summary>非法或未提供原因。</summary>
        None = 0,
        /// <summary>用户明确关闭。</summary>
        UserDismissed = 1,
        /// <summary>返回上一级。</summary>
        BackNavigation = 2,
        /// <summary>被新的互斥面板替换。</summary>
        Replaced = 3,
        /// <summary>父屏幕或父面板关闭。</summary>
        ParentClosed = 4,
        /// <summary>逻辑上下文已经失效。</summary>
        ContextInvalidated = 5,
        /// <summary>所属 owner 已完成操作。</summary>
        OwnerCompleted = 6,
        /// <summary>所属 owner 拒绝或不可用。</summary>
        OwnerRejected = 7,
        /// <summary>焦点恢复失败后的安全关闭。</summary>
        FocusLost = 8,
        /// <summary>会话结束。</summary>
        SessionEnded = 9,
        /// <summary>不可恢复错误。</summary>
        Error = 10,
    }

    /// <summary>允许 Runtime 解释的关闭策略，不拥有任何业务事实。</summary>
    public enum D22ClosePolicy
    {
        /// <summary>只能由父上下文或系统强制关闭。</summary>
        ParentOnly = 0,
        /// <summary>允许用户返回或显式关闭。</summary>
        UserDismissible = 1,
        /// <summary>关闭前须由外部 owner 确认。</summary>
        OwnerConfirmation = 2,
        /// <summary>出现上下文失效时强制关闭。</summary>
        ContextBound = 3,
    }

    /// <summary>D22 逻辑表面稳定 ID。</summary>
    public static class D22ScreenIds
    {
        /// <summary>空屏幕。</summary>
        public const string None = "";
        /// <summary>主菜单。</summary>
        public const string MainMenu = "screen.main_menu";
        /// <summary>暂停屏幕。</summary>
        public const string Pause = "screen.pause";
        /// <summary>航线 HUD 的逻辑屏幕。</summary>
        public const string RouteHud = "screen.route_hud";
        /// <summary>对话屏幕。</summary>
        public const string Dialogue = "screen.dialogue";
    }

    /// <summary>D22 面板与模态共用的稳定逻辑 ID。</summary>
    public static class D22PanelIds
    {
        /// <summary>空面板。</summary>
        public const string None = "";
        /// <summary>主菜单根面板。</summary>
        public const string MainMenu = "panel.main_menu";
        /// <summary>新游戏面板。</summary>
        public const string MainMenuNewGame = "panel.main_menu.new_game";
        /// <summary>继续游戏面板。</summary>
        public const string MainMenuContinue = "panel.main_menu.continue";
        /// <summary>设置面板。</summary>
        public const string MainMenuSettings = "panel.main_menu.settings";
        /// <summary>关于面板。</summary>
        public const string MainMenuAbout = "panel.main_menu.about";
        /// <summary>暂停根面板。</summary>
        public const string Pause = "panel.pause";
        /// <summary>航线状态面板。</summary>
        public const string RouteHud = "panel.route_hud";
        /// <summary>对话面板。</summary>
        public const string Dialogue = "panel.dialogue";

        /// <summary>无障碍设置模态。</summary>
        public const string AccessibilityModal = "modal.accessibility";
        /// <summary>财务债务模态。</summary>
        public const string FinanceDebtModal = "modal.finance_debt";
        /// <summary>本地诊断模态。</summary>
        public const string LocalDiagnosticsModal = "modal.local_diagnostics";
        /// <summary>敏感内容警告模态。</summary>
        public const string SensitiveWarningModal = "modal.sensitive_warning";
        /// <summary>通用确认模态。</summary>
        public const string ConfirmationModal = "modal.confirmation";
        /// <summary>未保存变更确认模态。</summary>
        public const string UnsavedChangesModal = "modal.unsaved_changes";

        /// <summary>字幕叠加层。</summary>
        public const string SubtitleOverlay = "overlay.subtitle";
        /// <summary>通知叠加层。</summary>
        public const string NotificationOverlay = "overlay.notification";
    }

    /// <summary>焦点控制目标的稳定 ID。Runtime 可扩展同一格式，但不能使用显示文本作 ID。</summary>
    public static class D22FocusControlIds
    {
        /// <summary>无焦点。</summary>
        public const string None = "focus.none";
        /// <summary>主菜单继续入口。</summary>
        public const string MainMenuContinue = "control.main_menu.continue";
        /// <summary>主菜单新游戏入口。</summary>
        public const string MainMenuNewGame = "control.main_menu.new_game";
        /// <summary>主菜单设置入口。</summary>
        public const string MainMenuSettings = "control.main_menu.settings";
        /// <summary>暂停恢复入口。</summary>
        public const string PauseResume = "control.pause.resume";
        /// <summary>暂停返回主菜单入口。</summary>
        public const string PauseMainMenu = "control.pause.main_menu";
        /// <summary>通用确认入口。</summary>
        public const string Confirm = "control.confirm";
        /// <summary>通用取消入口。</summary>
        public const string Cancel = "control.cancel";
    }

    /// <summary>逻辑层 ID 前缀。</summary>
    public static class D22LayerIds
    {
        /// <summary>背景层。</summary>
        public const string Background = "layer.background";
        /// <summary>屏幕层。</summary>
        public const string Screen = "layer.screen";
        /// <summary>面板层。</summary>
        public const string Panel = "layer.panel";
        /// <summary>模态层。</summary>
        public const string Modal = "layer.modal";
        /// <summary>叠加层。</summary>
        public const string Overlay = "layer.overlay";
    }

    /// <summary>稳定关闭原因 ID 的集中目录。</summary>
    public static class D22CloseReasonIds
    {
        /// <summary>缺少原因。</summary>
        public const string None = "d22.close.none";
        /// <summary>用户关闭。</summary>
        public const string UserDismissed = "d22.close.user_dismissed";
        /// <summary>返回导航。</summary>
        public const string BackNavigation = "d22.close.back_navigation";
        /// <summary>被替换。</summary>
        public const string Replaced = "d22.close.replaced";
        /// <summary>父上下文关闭。</summary>
        public const string ParentClosed = "d22.close.parent_closed";
        /// <summary>上下文失效。</summary>
        public const string ContextInvalidated = "d22.close.context_invalidated";
        /// <summary>owner 完成。</summary>
        public const string OwnerCompleted = "d22.close.owner_completed";
        /// <summary>owner 拒绝。</summary>
        public const string OwnerRejected = "d22.close.owner_rejected";
        /// <summary>焦点丢失。</summary>
        public const string FocusLost = "d22.close.focus_lost";
        /// <summary>会话结束。</summary>
        public const string SessionEnded = "d22.close.session_ended";
        /// <summary>不可恢复错误。</summary>
        public const string Error = "d22.close.error";
    }

    /// <summary>一个屏幕的纯逻辑定义。</summary>
    [Serializable]
    public sealed class D22ScreenDefinition
    {
        /// <summary>稳定屏幕 ID。</summary>
        public string Id = string.Empty;
        /// <summary>表面类型。</summary>
        public D22SurfaceKind SurfaceKind = D22SurfaceKind.Screen;
        /// <summary>本地化标题 key；不是显示文本。</summary>
        public string LocalizationKey = string.Empty;
        /// <summary>逻辑焦点层。</summary>
        public D22FocusLayer FocusLayer = D22FocusLayer.Screen;
        /// <summary>默认面板 ID。</summary>
        public string DefaultPanelId = string.Empty;
        /// <summary>默认焦点控制 ID。</summary>
        public string DefaultFocusControlId = D22FocusControlIds.None;
        /// <summary>默认回退焦点控制 ID。</summary>
        public string FallbackFocusControlId = D22FocusControlIds.None;
        /// <summary>是否属于全屏导航互斥组。</summary>
        public bool IsNavigationSurface;
        /// <summary>是否允许逻辑命令以该屏幕为上下文。</summary>
        public bool CanReceiveCommands = true;
        /// <summary>关闭策略。</summary>
        public D22ClosePolicy ClosePolicy = D22ClosePolicy.ContextBound;

        /// <summary>返回防御性副本。</summary>
        public D22ScreenDefinition Clone()
        {
            return new D22ScreenDefinition
            {
                Id = Id ?? string.Empty,
                SurfaceKind = SurfaceKind,
                LocalizationKey = LocalizationKey ?? string.Empty,
                FocusLayer = FocusLayer,
                DefaultPanelId = DefaultPanelId ?? string.Empty,
                DefaultFocusControlId = DefaultFocusControlId ?? D22FocusControlIds.None,
                FallbackFocusControlId = FallbackFocusControlId ?? D22FocusControlIds.None,
                IsNavigationSurface = IsNavigationSurface,
                CanReceiveCommands = CanReceiveCommands,
                ClosePolicy = ClosePolicy,
            };
        }
    }

    /// <summary>一个面板、模态或只读叠加层的纯逻辑定义。</summary>
    [Serializable]
    public sealed class D22PanelDefinition
    {
        /// <summary>稳定面板 ID。</summary>
        public string Id = string.Empty;
        /// <summary>表面类型。</summary>
        public D22SurfaceKind SurfaceKind = D22SurfaceKind.Panel;
        /// <summary>所属屏幕 ID；全局模态可以为空。</summary>
        public string ParentScreenId = string.Empty;
        /// <summary>逻辑父面板 ID；子模态可引用。</summary>
        public string ParentPanelId = string.Empty;
        /// <summary>本地化标题 key；不是显示文本。</summary>
        public string LocalizationKey = string.Empty;
        /// <summary>焦点层级。</summary>
        public D22FocusLayer FocusLayer = D22FocusLayer.Panel;
        /// <summary>模态阻塞等级。</summary>
        public D22ModalKind ModalKind = D22ModalKind.None;
        /// <summary>是否可以成为逻辑命令目标。</summary>
        public bool CanReceiveCommands = true;
        /// <summary>是否属于全屏/导航互斥面板组。</summary>
        public bool IsNavigationPanel;
        /// <summary>是否允许一个显式子阻塞模态。</summary>
        public bool AllowsChildModal;
        /// <summary>默认焦点控制 ID。</summary>
        public string DefaultFocusControlId = D22FocusControlIds.None;
        /// <summary>失焦时的确定性回退焦点控制 ID。</summary>
        public string FallbackFocusControlId = D22FocusControlIds.None;
        /// <summary>关闭策略。</summary>
        public D22ClosePolicy ClosePolicy = D22ClosePolicy.UserDismissible;

        /// <summary>返回防御性副本。</summary>
        public D22PanelDefinition Clone()
        {
            return new D22PanelDefinition
            {
                Id = Id ?? string.Empty,
                SurfaceKind = SurfaceKind,
                ParentScreenId = ParentScreenId ?? string.Empty,
                ParentPanelId = ParentPanelId ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                FocusLayer = FocusLayer,
                ModalKind = ModalKind,
                CanReceiveCommands = CanReceiveCommands,
                IsNavigationPanel = IsNavigationPanel,
                AllowsChildModal = AllowsChildModal,
                DefaultFocusControlId = DefaultFocusControlId ?? D22FocusControlIds.None,
                FallbackFocusControlId = FallbackFocusControlId ?? D22FocusControlIds.None,
                ClosePolicy = ClosePolicy,
            };
        }
    }

    /// <summary>
    /// D22-L02 稳定屏幕/面板/模态/焦点/层级目录。
    /// 目录只返回副本，避免 Runtime 适配器修改契约事实。
    /// </summary>
    public static class D22ScreenContracts
    {
        /// <summary>当前 D22 屏幕契约版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>本契约的逻辑 owner。</summary>
        public const string OwnerSystem = "D22ScreenContracts";

        private static readonly ReadOnlyCollection<D22ScreenDefinition> ScreenDefinitions =
            new ReadOnlyCollection<D22ScreenDefinition>(new List<D22ScreenDefinition>
            {
                new D22ScreenDefinition
                {
                    Id = D22ScreenIds.MainMenu,
                    LocalizationKey = "ui.d22.screen.main_menu",
                    DefaultPanelId = D22PanelIds.MainMenu,
                    DefaultFocusControlId = D22FocusControlIds.MainMenuContinue,
                    FallbackFocusControlId = D22FocusControlIds.MainMenuNewGame,
                    IsNavigationSurface = true,
                },
                new D22ScreenDefinition
                {
                    Id = D22ScreenIds.Pause,
                    LocalizationKey = "ui.d22.screen.pause",
                    DefaultPanelId = D22PanelIds.Pause,
                    DefaultFocusControlId = D22FocusControlIds.PauseResume,
                    FallbackFocusControlId = D22FocusControlIds.PauseResume,
                    IsNavigationSurface = true,
                },
                new D22ScreenDefinition
                {
                    Id = D22ScreenIds.RouteHud,
                    LocalizationKey = "ui.d22.screen.route_hud",
                    DefaultPanelId = D22PanelIds.RouteHud,
                    DefaultFocusControlId = D22FocusControlIds.None,
                    FallbackFocusControlId = D22FocusControlIds.None,
                    IsNavigationSurface = false,
                    CanReceiveCommands = false,
                    FocusLayer = D22FocusLayer.Background,
                },
                new D22ScreenDefinition
                {
                    Id = D22ScreenIds.Dialogue,
                    LocalizationKey = "ui.d22.screen.dialogue",
                    DefaultPanelId = D22PanelIds.Dialogue,
                    DefaultFocusControlId = D22FocusControlIds.Cancel,
                    FallbackFocusControlId = D22FocusControlIds.Cancel,
                    IsNavigationSurface = true,
                },
            });

        private static readonly ReadOnlyCollection<D22PanelDefinition> PanelDefinitions =
            new ReadOnlyCollection<D22PanelDefinition>(new List<D22PanelDefinition>
            {
                NavigationPanel(D22PanelIds.MainMenu, D22ScreenIds.MainMenu, "ui.d22.panel.main_menu", D22FocusControlIds.MainMenuContinue, D22FocusControlIds.MainMenuNewGame),
                NavigationPanel(D22PanelIds.MainMenuNewGame, D22ScreenIds.MainMenu, "ui.d22.panel.main_menu.new_game", D22FocusControlIds.MainMenuNewGame, D22FocusControlIds.Cancel),
                NavigationPanel(D22PanelIds.MainMenuContinue, D22ScreenIds.MainMenu, "ui.d22.panel.main_menu.continue", D22FocusControlIds.MainMenuContinue, D22FocusControlIds.MainMenuNewGame),
                NavigationPanel(D22PanelIds.MainMenuSettings, D22ScreenIds.MainMenu, "ui.d22.panel.main_menu.settings", D22FocusControlIds.MainMenuSettings, D22FocusControlIds.Cancel),
                NavigationPanel(D22PanelIds.MainMenuAbout, D22ScreenIds.MainMenu, "ui.d22.panel.main_menu.about", D22FocusControlIds.Cancel, D22FocusControlIds.Cancel),
                NavigationPanel(D22PanelIds.Pause, D22ScreenIds.Pause, "ui.d22.panel.pause", D22FocusControlIds.PauseResume, D22FocusControlIds.PauseResume),
                new D22PanelDefinition
                {
                    Id = D22PanelIds.RouteHud,
                    SurfaceKind = D22SurfaceKind.Panel,
                    ParentScreenId = D22ScreenIds.RouteHud,
                    LocalizationKey = "ui.d22.panel.route_hud",
                    FocusLayer = D22FocusLayer.Background,
                    CanReceiveCommands = false,
                    DefaultFocusControlId = D22FocusControlIds.None,
                    FallbackFocusControlId = D22FocusControlIds.None,
                    ClosePolicy = D22ClosePolicy.ContextBound,
                },
                NavigationPanel(D22PanelIds.Dialogue, D22ScreenIds.Dialogue, "ui.d22.panel.dialogue", D22FocusControlIds.Cancel, D22FocusControlIds.Cancel),
                Modal(D22PanelIds.AccessibilityModal, "ui.d22.modal.accessibility", D22ModalKind.Blocking, false),
                Modal(D22PanelIds.FinanceDebtModal, "ui.d22.modal.finance_debt", D22ModalKind.NonBlocking, false),
                Modal(D22PanelIds.LocalDiagnosticsModal, "ui.d22.modal.local_diagnostics", D22ModalKind.Blocking, false),
                Modal(D22PanelIds.SensitiveWarningModal, "ui.d22.modal.sensitive_warning", D22ModalKind.Blocking, true),
                Modal(D22PanelIds.ConfirmationModal, "ui.d22.modal.confirmation", D22ModalKind.ChildBlocking, false),
                Modal(D22PanelIds.UnsavedChangesModal, "ui.d22.modal.unsaved_changes", D22ModalKind.ChildBlocking, false),
                new D22PanelDefinition
                {
                    Id = D22PanelIds.SubtitleOverlay,
                    SurfaceKind = D22SurfaceKind.Overlay,
                    LocalizationKey = "ui.d22.overlay.subtitle",
                    FocusLayer = D22FocusLayer.Overlay,
                    ModalKind = D22ModalKind.NonBlocking,
                    CanReceiveCommands = false,
                    DefaultFocusControlId = D22FocusControlIds.None,
                    FallbackFocusControlId = D22FocusControlIds.None,
                    ClosePolicy = D22ClosePolicy.ContextBound,
                },
                new D22PanelDefinition
                {
                    Id = D22PanelIds.NotificationOverlay,
                    SurfaceKind = D22SurfaceKind.Overlay,
                    LocalizationKey = "ui.d22.overlay.notification",
                    FocusLayer = D22FocusLayer.Overlay,
                    ModalKind = D22ModalKind.NonBlocking,
                    CanReceiveCommands = false,
                    DefaultFocusControlId = D22FocusControlIds.None,
                    FallbackFocusControlId = D22FocusControlIds.None,
                    ClosePolicy = D22ClosePolicy.ContextBound,
                },
            });

        /// <summary>返回所有已冻结屏幕定义的防御性副本。</summary>
        public static IReadOnlyList<D22ScreenDefinition> GetScreenDefinitions()
        {
            var result = new List<D22ScreenDefinition>();
            foreach (var definition in ScreenDefinitions)
                result.Add(definition.Clone());
            return new ReadOnlyCollection<D22ScreenDefinition>(result);
        }

        /// <summary>返回所有已冻结面板定义的防御性副本。</summary>
        public static IReadOnlyList<D22PanelDefinition> GetPanelDefinitions()
        {
            var result = new List<D22PanelDefinition>();
            foreach (var definition in PanelDefinitions)
                result.Add(definition.Clone());
            return new ReadOnlyCollection<D22PanelDefinition>(result);
        }

        /// <summary>按稳定 ID 查找屏幕定义。</summary>
        public static bool TryGetScreenDefinition(string id, out D22ScreenDefinition definition)
        {
            foreach (var candidate in ScreenDefinitions)
            {
                if (string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate.Clone();
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>按稳定 ID 查找面板/模态/叠加层定义。</summary>
        public static bool TryGetPanelDefinition(string id, out D22PanelDefinition definition)
        {
            foreach (var candidate in PanelDefinitions)
            {
                if (string.Equals(candidate.Id, id, StringComparison.Ordinal))
                {
                    definition = candidate.Clone();
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>判断是否为已登记屏幕 ID。</summary>
        public static bool IsKnownScreenId(string id)
        {
            D22ScreenDefinition ignored;
            return TryGetScreenDefinition(id, out ignored);
        }

        /// <summary>判断是否为已登记面板、模态或叠加层 ID。</summary>
        public static bool IsKnownPanelId(string id)
        {
            D22PanelDefinition ignored;
            return TryGetPanelDefinition(id, out ignored);
        }

        /// <summary>判断是否为已登记模态 ID。</summary>
        public static bool IsKnownModalId(string id)
        {
            D22PanelDefinition definition;
            return TryGetPanelDefinition(id, out definition) && definition.SurfaceKind == D22SurfaceKind.Modal;
        }

        /// <summary>判断是否为已登记叠加层 ID。</summary>
        public static bool IsKnownOverlayId(string id)
        {
            D22PanelDefinition definition;
            return TryGetPanelDefinition(id, out definition) && definition.SurfaceKind == D22SurfaceKind.Overlay;
        }

        /// <summary>校验不依赖显示文本的稳定 ASCII ID。</summary>
        public static bool IsStableId(string id, string requiredPrefix = null)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 160)
                return false;
            if (!string.IsNullOrEmpty(requiredPrefix) && !id.StartsWith(requiredPrefix, StringComparison.Ordinal))
                return false;
            foreach (var character in id)
            {
                if (!((character >= 'a' && character <= 'z') ||
                      (character >= '0' && character <= '9') ||
                      character == '.' || character == '_' || character == '-' || character == ':'))
                    return false;
            }
            return true;
        }

        /// <summary>将关闭枚举转换为稳定机器 ID。</summary>
        public static string GetCloseReasonId(D22CloseReason reason)
        {
            switch (reason)
            {
                case D22CloseReason.UserDismissed: return D22CloseReasonIds.UserDismissed;
                case D22CloseReason.BackNavigation: return D22CloseReasonIds.BackNavigation;
                case D22CloseReason.Replaced: return D22CloseReasonIds.Replaced;
                case D22CloseReason.ParentClosed: return D22CloseReasonIds.ParentClosed;
                case D22CloseReason.ContextInvalidated: return D22CloseReasonIds.ContextInvalidated;
                case D22CloseReason.OwnerCompleted: return D22CloseReasonIds.OwnerCompleted;
                case D22CloseReason.OwnerRejected: return D22CloseReasonIds.OwnerRejected;
                case D22CloseReason.FocusLost: return D22CloseReasonIds.FocusLost;
                case D22CloseReason.SessionEnded: return D22CloseReasonIds.SessionEnded;
                case D22CloseReason.Error: return D22CloseReasonIds.Error;
                default: return D22CloseReasonIds.None;
            }
        }

        private static D22PanelDefinition NavigationPanel(
            string id,
            string parentScreenId,
            string localizationKey,
            string defaultFocusControlId,
            string fallbackFocusControlId)
        {
            return new D22PanelDefinition
            {
                Id = id,
                SurfaceKind = D22SurfaceKind.Panel,
                ParentScreenId = parentScreenId,
                LocalizationKey = localizationKey,
                FocusLayer = D22FocusLayer.Panel,
                ModalKind = D22ModalKind.None,
                CanReceiveCommands = true,
                IsNavigationPanel = true,
                DefaultFocusControlId = defaultFocusControlId,
                FallbackFocusControlId = fallbackFocusControlId,
                ClosePolicy = D22ClosePolicy.UserDismissible,
            };
        }

        private static D22PanelDefinition Modal(
            string id,
            string localizationKey,
            D22ModalKind modalKind,
            bool allowsChildModal)
        {
            return new D22PanelDefinition
            {
                Id = id,
                SurfaceKind = D22SurfaceKind.Modal,
                LocalizationKey = localizationKey,
                FocusLayer = D22FocusLayer.Modal,
                ModalKind = modalKind,
                CanReceiveCommands = true,
                AllowsChildModal = allowsChildModal,
                DefaultFocusControlId = D22FocusControlIds.Confirm,
                FallbackFocusControlId = D22FocusControlIds.Cancel,
                ClosePolicy = D22ClosePolicy.UserDismissible,
            };
        }
    }

    /// <summary>兼容单数命名的稳定屏幕 ID 入口；值仍由 D22ScreenIds 统一持有。</summary>
    public static class D22ScreenId
    {
        /// <summary>主菜单。</summary>
        public const string MainMenu = D22ScreenIds.MainMenu;
        /// <summary>暂停屏幕。</summary>
        public const string Pause = D22ScreenIds.Pause;
        /// <summary>航线 HUD。</summary>
        public const string RouteHud = D22ScreenIds.RouteHud;
        /// <summary>对话屏幕。</summary>
        public const string Dialogue = D22ScreenIds.Dialogue;
    }
}
