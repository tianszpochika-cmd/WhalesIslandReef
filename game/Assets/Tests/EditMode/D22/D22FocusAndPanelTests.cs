// D22-L24：焦点、模态、互斥面板、返回和不可恢复错误的纯逻辑测试。

using System;
using Whalebone.Systems.D22;

namespace Whalebone.Systems.D22.Tests
{
    public static class D22FocusAndPanelTests
    {
        public static int RunAll()
        {
            int assertions = 0;
            var focus = new D22FocusStateMachine();
            Check(ref assertions, focus.ActivateScreen(
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                D22FocusControlIds.MainMenuContinue,
                D22FocusControlIds.MainMenuNewGame,
                D22InputModeIntent.Navigation).Accepted,
                "screen activation accepts a known focus context");

            Check(ref assertions, focus.OpenModal(
                D22PanelIds.AccessibilityModal,
                "owner:d22:test").Accepted,
                "first modal open is accepted");
            D22FocusResult duplicateModal = focus.OpenModal(
                D22PanelIds.AccessibilityModal,
                "owner:d22:test");
            Check(ref assertions,
                !duplicateModal.Accepted &&
                duplicateModal.FailureCode == D22FocusFailureCode.ModalAlreadyOpen,
                "duplicate modal open is explicitly rejected");
            Check(ref assertions, focus.CloseTopModal(
                D22CloseReason.UserDismissed).Accepted,
                "top modal closes through the logical stack");
            Check(ref assertions, focus.HandleFocusLost().Accepted,
                "focus lost has an explicit recovery result");
            Check(ref assertions, focus.RecoverFocus().Accepted,
                "focus recovery restores a stable token");

            var panels = new D22PanelTransitionSystem();
            Check(ref assertions, panels.ActivateScreen(
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu).Accepted,
                "panel screen activation is accepted");
            D22PanelTransitionResult opened = panels.Open(
                D22PanelIds.MainMenuSettings,
                D22ScreenIds.MainMenu,
                "owner:d22:test");
            Check(ref assertions, opened.Accepted && opened.Changed,
                "settings panel opens in its parent screen");
            Check(ref assertions, panels.Snapshot().HasOpenPanel(
                D22PanelIds.MainMenuSettings),
                "opened panel is visible in the logical snapshot");
            Check(ref assertions, panels.Back().Accepted,
                "back closes the top closable panel");
            D22PanelTransitionResult terminal = panels.EnterUnrecoverableError(
                "d22.test.terminal");
            Check(ref assertions, terminal.Accepted && terminal.State.IsUnrecoverableError,
                "terminal error clears interaction and is explicit");
            Check(ref assertions, !panels.Open(
                D22PanelIds.MainMenuSettings,
                D22ScreenIds.MainMenu,
                "owner:d22:test").Accepted,
                "terminal error blocks new panel interaction");
            Check(ref assertions, panels.ResetSession().Accepted,
                "new session is the only terminal recovery path");
            return assertions;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
