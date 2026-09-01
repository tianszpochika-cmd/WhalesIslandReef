// D22-L29：固定种子 UI 状态/命令序列测试。
// 使用自有线性同余步进器，避免不可控随机与墙钟依赖。

using System;
using Whalebone.Systems.D22;

namespace Whalebone.Systems.D22.Tests
{
    public static class D22UiStateFuzzTests
    {
        private const int SequenceCount = 1000;
        private const int StepsPerSequence = 16;

        /// <summary>执行 1,000 个固定种子序列并检查焦点始终可恢复。</summary>
        public static int Run()
        {
            int assertions = 0;
            uint seed = 0xD22F29u;
            var focus = new D22FocusStateMachine();
            var panels = new D22PanelTransitionSystem();
            var router = new D22UiCommandRouter();

            D22FocusResult activated = focus.ActivateScreen(
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                D22FocusControlIds.MainMenuContinue,
                D22FocusControlIds.MainMenuNewGame,
                D22InputModeIntent.Navigation);
            Check(ref assertions, activated.Accepted, "initial focus screen is accepted");
            D22PanelTransitionResult panelActivated = panels.ActivateScreen(
                D22ScreenIds.MainMenu, D22PanelIds.MainMenu);
            Check(ref assertions, panelActivated.Accepted, "initial panel screen is accepted");

            for (int sequence = 0; sequence < SequenceCount; sequence++)
            {
                for (int step = 0; step < StepsPerSequence; step++)
                {
                    int operation = (int)(Next(ref seed) % 10u);
                    ExecuteOperation(operation, sequence, step, focus, panels, router,
                        ref assertions);
                    AssertReachableState(focus, panels, ref assertions,
                        "sequence " + sequence + " step " + step);
                }
            }

            Check(ref assertions, focus.Snapshot().ModalStack != null,
                "fuzz run leaves a readable modal stack");
            Check(ref assertions, panels.Snapshot().OpenPanels != null,
                "fuzz run leaves a readable panel collection");
            return assertions;
        }

        /// <summary>兼容纯逻辑 runner 的入口。</summary>
        public static int RunAll()
        {
            return Run();
        }

        private static void ExecuteOperation(
            int operation,
            int sequence,
            int step,
            D22FocusStateMachine focus,
            D22PanelTransitionSystem panels,
            D22UiCommandRouter router,
            ref int assertions)
        {
            switch (operation)
            {
                case 0:
                    focus.SetFocus(D22FocusControlIds.MainMenuContinue,
                        D22InputModeIntent.Navigation);
                    break;
                case 1:
                    focus.SetFocus(D22FocusControlIds.MainMenuNewGame,
                        D22InputModeIntent.Navigation);
                    break;
                case 2:
                    focus.HandleFocusLost();
                    break;
                case 3:
                    if (focus.Snapshot().ModalStack.Count == 0)
                    {
                        D22FocusResult opened = focus.OpenModal(
                            D22PanelIds.AccessibilityModal,
                            "owner:d22:fuzz");
                        Check(ref assertions, opened.Accepted ||
                            opened.FailureCode == D22FocusFailureCode.ModalAlreadyOpen,
                            "modal operation has an explicit result");
                    }
                    else
                    {
                        D22FocusResult closed = focus.CloseTopModal(D22CloseReason.UserDismissed);
                        Check(ref assertions, closed.Accepted ||
                            closed.FailureCode == D22FocusFailureCode.ModalStackEmpty,
                            "modal close has an explicit result");
                    }
                    break;
                case 4:
                    panels.Open(D22PanelIds.MainMenuSettings,
                        D22ScreenIds.MainMenu, "owner:d22:fuzz");
                    break;
                case 5:
                    panels.Back();
                    break;
                case 6:
                    panels.Close(D22PanelIds.MainMenuSettings, D22CloseReason.BackNavigation);
                    break;
                case 7:
                    focus.ActivateScreen(
                        (sequence + step) % 3 == 0
                            ? D22ScreenIds.MainMenu
                            : (sequence + step) % 3 == 1
                                ? D22ScreenIds.Pause
                                : D22ScreenIds.Dialogue,
                        (sequence + step) % 3 == 0
                            ? D22PanelIds.MainMenu
                            : (sequence + step) % 3 == 1
                                ? D22PanelIds.Pause
                                : D22PanelIds.Dialogue,
                        D22FocusControlIds.Cancel,
                        D22FocusControlIds.Cancel,
                        D22InputModeIntent.Navigation);
                    break;
                case 8:
                    D22UiCommandRequest command = D22UiCommandRequest.Create(
                        D22UiCommandKind.ShipAction,
                        "command:fuzz:" + sequence + ":" + step,
                        "correlation:fuzz:" + sequence + ":" + step,
                        "actor.d22.fuzz",
                        D22ScreenIds.MainMenu,
                        D22PanelIds.MainMenu,
                        "ship:fuzz:" + sequence,
                        10,
                        0);
                    D22UiCommandResult routed = router.Route(command, CreateCommandContext());
                    Check(ref assertions,
                        routed.Outcome == D22UiCommandOutcome.Blocked ||
                        routed.Outcome == D22UiCommandOutcome.Rejected,
                        "missing ship owner never becomes success");
                    break;
                default:
                    focus.RecoverFocus();
                    panels.Replace(D22PanelIds.MainMenu,
                        D22ScreenIds.MainMenu, "owner:d22:fuzz");
                    break;
            }
        }

        private static D22UiCommandContext CreateCommandContext()
        {
            return new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.MainMenu,
                ActivePanelId = D22PanelIds.MainMenu,
                FocusedControlId = D22FocusControlIds.MainMenuContinue,
                ActorId = "actor.d22.fuzz",
                WorldDay = 10,
                SaveRevision = 0,
            };
        }

        private static void AssertReachableState(
            D22FocusStateMachine focus,
            D22PanelTransitionSystem panels,
            ref int assertions,
            string label)
        {
            D22FocusSnapshot focusSnapshot = focus.Snapshot();
            Check(ref assertions,
                string.IsNullOrEmpty(focusSnapshot.ActiveScreenId) ||
                D22ScreenContracts.IsKnownScreenId(focusSnapshot.ActiveScreenId),
                label + " focus screen remains known or empty");
            Check(ref assertions,
                string.IsNullOrEmpty(focusSnapshot.ActivePanelId) ||
                D22ScreenContracts.IsKnownPanelId(focusSnapshot.ActivePanelId),
                label + " focus panel remains known or empty");
            Check(ref assertions, focusSnapshot.ModalStack != null,
                label + " focus modal stack is readable");
            if (focusSnapshot.ModalStack.Count > 0)
            {
                D22ModalFocusEntry top = focusSnapshot.ModalStack[
                    focusSnapshot.ModalStack.Count - 1];
                Check(ref assertions, top != null &&
                    string.Equals(top.ModalId, focusSnapshot.ActiveModalId,
                        StringComparison.Ordinal),
                    label + " top modal owns the active modal token");
                Check(ref assertions, top != null &&
                    D22ScreenContracts.IsStableId(top.FocusedControlId),
                    label + " modal focus has a stable control token");
            }
            Check(ref assertions, panels.Snapshot().OpenPanels != null,
                label + " panel state remains readable");
        }

        private static uint Next(ref uint state)
        {
            unchecked
            {
                state = state * 1664525u + 1013904223u;
            }
            return state;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
