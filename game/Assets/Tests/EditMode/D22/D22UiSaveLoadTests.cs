// D22-L30：通知、暂停意图、可恢复面板及命令幂等投影的 Save/Load 边界。
// 这里只验证显式 snapshot 的复制/恢复语义，不执行真实存档系统。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D22;

namespace Whalebone.Systems.D22.Tests
{
    public static class D22UiSaveLoadTests
    {
        /// <summary>执行所有 D22 UI save/load 边界断言。</summary>
        public static int Run()
        {
            int assertions = 0;
            NotificationSnapshotRoundTrip(ref assertions);
            PauseAndRecoverablePanelProjectionRoundTrip(ref assertions);
            RouterIdempotencySnapshotRoundTrip(ref assertions);
            return assertions;
        }

        /// <summary>兼容纯逻辑 runner 的入口。</summary>
        public static int RunAll()
        {
            return Run();
        }

        private static void NotificationSnapshotRoundTrip(ref int assertions)
        {
            var queue = new D22NotificationQueue();
            D22NotificationEvent persistent = new D22NotificationEvent
            {
                EventId = "event:d22:save:persistent",
                DeduplicationKey = "notice:d22:save:persistent",
                CorrelationId = "correlation:d22:save:persistent",
                SourceOwner = "owner:d22:test",
                LocalizationKey = "ui.d22.test.notification.persistent",
                CreatedWorldDay = 10,
                ExpiresWorldDay = 20,
                RequiresAcknowledgement = true,
                Persistence = D22NotificationPersistence.UntilAcknowledged,
            };
            D22NotificationEvent transient = persistent.Clone();
            transient.EventId = "event:d22:save:transient";
            transient.DeduplicationKey = "notice:d22:save:transient";
            transient.Persistence = D22NotificationPersistence.Transient;
            transient.RequiresAcknowledgement = false;

            D22NotificationQueueResult first = queue.Enqueue(persistent, 10);
            D22NotificationQueueResult second = queue.Enqueue(transient, 10);
            Check(ref assertions, first.Accepted && first.Changed,
                "persistent notification is accepted");
            Check(ref assertions, second.Accepted && second.Changed,
                "transient notification is accepted before save");

            D22NotificationQueueSnapshot saved = queue.CreateSaveSnapshot(10);
            Check(ref assertions, saved.Notifications.Count == 1,
                "transient notification is excluded from UI save projection");
            D22NotificationQueue restored = D22NotificationQueue.FromSnapshot(saved, 10);
            Check(ref assertions, restored.GetActive(10).Count == 1,
                "persistent notification survives snapshot restore");
            Check(ref assertions,
                restored.PeekNext(10).EventId == persistent.EventId,
                "notification order survives snapshot restore");

            D22NotificationQueueResult replay = restored.Enqueue(persistent.Clone(), 10);
            Check(ref assertions, replay.Duplicate && !replay.Changed,
                "restored processed-event index prevents duplicate notification");
            D22NotificationQueueResult acknowledged = restored.Acknowledge(
                persistent.NotificationId, 11);
            Check(ref assertions, acknowledged.Accepted && acknowledged.Changed,
                "restored notification remains acknowledgeable");
            Check(ref assertions, restored.GetActive(11).Count == 0,
                "acknowledged notification leaves active projection");
        }

        private static void PauseAndRecoverablePanelProjectionRoundTrip(ref int assertions)
        {
            var panels = new D22PanelTransitionSystem();
            D22PanelTransitionResult activated = panels.ActivateScreen(
                D22ScreenIds.MainMenu, D22PanelIds.MainMenu);
            Check(ref assertions, activated.Accepted, "panel save context is active");
            D22PanelTransitionResult opened = panels.Open(
                D22PanelIds.MainMenuSettings,
                D22ScreenIds.MainMenu,
                "owner:d22:save");
            Check(ref assertions, opened.Accepted, "recoverable settings panel is open");
            D22PanelTransitionSnapshot panelSave = panels.Snapshot();

            var focus = new D22FocusStateMachine();
            D22FocusResult focusActivated = focus.ActivateScreen(
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                D22FocusControlIds.MainMenuSettings,
                D22FocusControlIds.MainMenuNewGame,
                D22InputModeIntent.Navigation);
            Check(ref assertions, focusActivated.Accepted, "focus save context is active");
            D22FocusSnapshot focusSave = focus.Snapshot();

            var pause = new D22PauseIntentSystem();
            D22PauseContext pauseContext = new D22PauseContext
            {
                ContextId = "context:d22:save",
                ActiveScreenId = D22ScreenIds.MainMenu,
                ActivePanelId = D22PanelIds.MainMenuSettings,
                WorldDay = 10,
                SaveRevision = 5,
                SessionActive = true,
                ContextValid = true,
                PauseAllowed = true,
                ResumeAllowed = true,
            };
            D22PauseIntentResult paused = pause.Handle(
                D22PauseIntent.Pause(
                    "correlation:d22:pause",
                    "actor:d22:save",
                    pauseContext.ContextId,
                    pauseContext.WorldDay,
                    pauseContext.SaveRevision),
                pauseContext);
            Check(ref assertions, paused.Accepted && paused.ReadModel.IsPaused,
                "pause intent is represented in the save projection");

            D22NotificationQueueSnapshot emptyNotifications =
                new D22NotificationQueueSnapshot();
            D22ViewModelCompositionInput saveInput = new D22ViewModelCompositionInput
            {
                PanelState = panelSave.Clone(),
                FocusState = focusSave.Clone(),
                PauseState = paused.ReadModel.Clone(),
                NotificationState = emptyNotifications,
                LoadState = new D22LoadStateReadModel { IsValid = true },
                AccessibilityState = new D22AccessibilityReadModel
                {
                    IsValid = true,
                    Locale = "zh-CN",
                },
                Sources = new List<ID22ViewModelSourceAdapter>(),
            };
            D22ViewModel beforeLoad = D22ViewModelComposer.ComposeSnapshot(saveInput);
            D22ViewModel loaded = D22ViewModelComposer.ComposeSnapshot(saveInput.Clone());
            Check(ref assertions, beforeLoad.CompositionFingerprint == loaded.CompositionFingerprint,
                "cloned UI snapshots compose to the same deterministic view model");
            Check(ref assertions,
                loaded.ActivePanelId == D22PanelIds.MainMenuSettings,
                "recoverable panel remains in the loaded projection");
            Check(ref assertions, loaded.IsPaused && loaded.PauseState.IsPaused,
                "pause state remains in the loaded projection");
            Check(ref assertions, loaded.IsDegraded,
                "absent upstream adapters remain explicit after load");

            var busyPause = new D22PauseIntentSystem();
            D22PauseContext savingContext = pauseContext.Clone();
            savingContext.IsSaving = true;
            D22PauseIntentResult rejected = busyPause.Handle(
                D22PauseIntent.Pause(
                    "correlation:d22:pause:invalid",
                    "actor:d22:save",
                    savingContext.ContextId,
                    savingContext.WorldDay,
                    savingContext.SaveRevision),
                savingContext);
            Check(ref assertions, !rejected.Accepted && !rejected.ReadModel.CanPause,
                "saving boundary rejects a new pause intent without changing pause state");
        }

        private static void RouterIdempotencySnapshotRoundTrip(ref int assertions)
        {
            int calls = 0;
            var router = new D22UiCommandRouter();
            router.Register("D19PortCommandRouter", delegate (D22UiCommandRequest request)
            {
                calls++;
                return D22UiOwnerCommandResult.Succeeded(
                    "D19PortCommandRouter", "port.accepted", true);
            });
            D22UiCommandContext context = new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.Dialogue,
                ActivePanelId = D22PanelIds.Dialogue,
                FocusedControlId = D22FocusControlIds.Cancel,
                ActorId = "actor:d22:save",
                WorldDay = 10,
                SaveRevision = 5,
            };
            D22UiCommandRequest command = D22UiCommandRequest.Create(
                D22UiCommandKind.PortAction,
                "command:d22:save:port",
                "correlation:d22:save:port",
                context.ActorId,
                context.ActiveScreenId,
                context.ActivePanelId,
                "port:d22:save",
                context.WorldDay,
                context.SaveRevision);
            D22UiCommandResult first = router.Route(command, context);
            D22UiCommandRouterSnapshot save = router.CreateSaveSnapshot();
            var loadedRouter = new D22UiCommandRouter();
            D22UiCommandRouterRestoreResult restored = loadedRouter.Restore(save.Clone());
            D22UiCommandResult replay = loadedRouter.Route(command.Clone(), context.Clone());

            Check(ref assertions, first.Outcome == D22UiCommandOutcome.Succeeded,
                "router command succeeds before save");
            Check(ref assertions, restored.Accepted && restored.Changed,
                "router idempotency projection restores a non-empty snapshot");
            Check(ref assertions, replay.Outcome == D22UiCommandOutcome.Duplicate,
                "router replay after restore is duplicate");
            Check(ref assertions, calls == 1,
                "router owner is not called during replay after restore");
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
