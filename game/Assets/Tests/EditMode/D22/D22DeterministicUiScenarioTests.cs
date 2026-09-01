// D22-L28：固定输入重放 UI 逻辑流程。
// 这些测试只使用 D22 DTO、公开 handler 和只读 snapshot，不依赖运行时对象。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D22;

namespace Whalebone.Systems.D22.Tests
{
    public static class D22DeterministicUiScenarioTests
    {
        /// <summary>执行港口、市场、船舶、保存命令及 ViewModel 组合场景。</summary>
        public static int Run()
        {
            int assertions = 0;
            RoutePortMarketShipAndSave(ref assertions);
            MissingOwnerFailsClosed(ref assertions);
            OwnerRejectionIsProjectedWithoutBusinessChange(ref assertions);
            ComposerKeepsStableOrderAndDefensiveCopies(ref assertions);
            return assertions;
        }

        /// <summary>兼容纯逻辑 runner 的入口。</summary>
        public static int RunAll()
        {
            return Run();
        }

        private static void RoutePortMarketShipAndSave(ref int assertions)
        {
            var calls = new Dictionary<string, int>(StringComparer.Ordinal);
            var router = new D22UiCommandRouter();
            RegisterSuccessOwner(router, calls, "D19PortCommandRouter");
            RegisterSuccessOwner(router, calls, "Economy/Market");
            RegisterSuccessOwner(router, calls, "Ship/Voyage");
            RegisterSuccessOwner(router, calls, "SaveSystem");

            D22UiCommandContext context = CreateContext();
            var scenarios = new[]
            {
                new Scenario(D22UiCommandKind.PortAction, "port:scenario", "D19PortCommandRouter"),
                new Scenario(D22UiCommandKind.MarketAction, "market:scenario", "Economy/Market"),
                new Scenario(D22UiCommandKind.ShipAction, "ship:scenario", "Ship/Voyage"),
                new Scenario(D22UiCommandKind.SaveGame, "save:scenario", "SaveSystem"),
            };

            foreach (var scenario in scenarios)
            {
                D22UiCommandRequest request = D22UiCommandRequest.Create(
                    scenario.Kind,
                    "command:" + scenario.TargetId,
                    "correlation:" + scenario.TargetId,
                    context.ActorId,
                    context.ActiveScreenId,
                    context.ActivePanelId,
                    scenario.TargetId,
                    context.WorldDay,
                    context.SaveRevision);
                D22UiCommandRouteResult first = router.RouteDetailed(request, context);
                Check(ref assertions, first.OwnerInvoked, scenario.TargetId + " owner invoked");
                Check(ref assertions,
                    first.CommandResult.Outcome == D22UiCommandOutcome.Succeeded,
                    scenario.TargetId + " succeeded");
                Check(ref assertions,
                    first.CommandResult.BusinessStateChanged,
                    scenario.TargetId + " only reports owner change");
                Check(ref assertions,
                    first.Feedback.Code == D22FeedbackCodes.CommandCompleted,
                    scenario.TargetId + " completed feedback");

                D22UiCommandResult replay = router.Route(request.Clone(), context.Clone());
                Check(ref assertions,
                    replay.Outcome == D22UiCommandOutcome.Duplicate && replay.Duplicate,
                    scenario.TargetId + " replay is duplicate");
                Check(ref assertions,
                    calls[scenario.OwnerSystem] == 1,
                    scenario.TargetId + " owner called once");
            }
            Check(ref assertions, router.ProcessedCommandCount == scenarios.Length,
                "four distinct UI commands have four idempotent records");
        }

        private static void MissingOwnerFailsClosed(ref int assertions)
        {
            D22UiCommandContext context = CreateContext();
            D22UiCommandRequest request = D22UiCommandRequest.Create(
                D22UiCommandKind.ShipAction,
                "command:missing-ship-owner",
                "correlation:missing-ship-owner",
                context.ActorId,
                context.ActiveScreenId,
                context.ActivePanelId,
                "ship:missing-owner",
                context.WorldDay,
                context.SaveRevision);
            D22UiCommandResult result = new D22UiCommandRouter().Route(request, context);

            Check(ref assertions, result.Outcome == D22UiCommandOutcome.Blocked,
                "missing ship owner is blocked");
            Check(ref assertions, !result.Accepted && !result.BusinessStateChanged,
                "missing ship owner cannot report success or business change");
            Check(ref assertions, result.FeedbackCode == D22FeedbackCodes.OwnerUnavailable,
                "missing ship owner uses unavailable feedback");
            Check(ref assertions, result.DisabledReasonCode == D22DisabledReasonCodes.OwnerUnavailable,
                "missing ship owner preserves disabled reason");
        }

        private static void OwnerRejectionIsProjectedWithoutBusinessChange(ref int assertions)
        {
            D22UiCommandContext context = CreateContext();
            var router = new D22UiCommandRouter();
            router.Register("Economy/Market", delegate (D22UiCommandRequest request)
            {
                return D22UiOwnerCommandResult.Rejected(
                    "Economy/Market", "insufficient_funds", "market.insufficient_funds");
            });
            D22UiCommandRequest command = D22UiCommandRequest.Create(
                D22UiCommandKind.MarketAction,
                "command:rejected-market",
                "correlation:rejected-market",
                context.ActorId,
                context.ActiveScreenId,
                context.ActivePanelId,
                "market:rejected",
                context.WorldDay,
                context.SaveRevision);
            D22UiCommandResult result = router.Route(command, context);

            Check(ref assertions, result.Outcome == D22UiCommandOutcome.Rejected,
                "owner rejection remains rejected");
            Check(ref assertions, !result.BusinessStateChanged,
                "owner rejection cannot report a business change");
            Check(ref assertions, result.OwnerResultCode == "insufficient_funds",
                "owner code is retained");
            Check(ref assertions, result.ReasonCode == "market.insufficient_funds",
                "owner reason is retained");
            Check(ref assertions, result.DisabledReasonCode == D22DisabledReasonCodes.OwnerRejected,
                "owner rejection gets stable disabled fallback");
        }

        private static void ComposerKeepsStableOrderAndDefensiveCopies(ref int assertions)
        {
            var portSource = new D22ViewModelSourceSnapshot
            {
                ModuleId = "port",
                OwnerSystem = "D19PortCommandRouter",
                Available = true,
                Fingerprint = "port:fingerprint",
                Fields = new List<D22ViewModelField>
                {
                    new D22ViewModelField { Key = "zeta", Value = "2" },
                    new D22ViewModelField { Key = "alpha", Value = "1" },
                },
            };
            var marketSource = new D22ViewModelSourceSnapshot
            {
                ModuleId = D22ViewModelModuleIds.Market,
                OwnerSystem = "Economy/Market",
                Available = true,
                Fingerprint = "market:fingerprint",
            };
            var input = new D22ViewModelCompositionInput
            {
                PanelState = new D22PanelTransitionSnapshot
                {
                    ActiveScreenId = D22ScreenIds.Dialogue,
                    ActivePanelId = D22PanelIds.Dialogue,
                },
                FocusState = new D22FocusSnapshot
                {
                    ActiveScreenId = D22ScreenIds.Dialogue,
                    ActivePanelId = D22PanelIds.Dialogue,
                    FocusedControlId = D22FocusControlIds.Cancel,
                },
                PauseState = new D22PauseReadModel { IsPaused = false },
                NotificationState = new D22NotificationQueueSnapshot(),
                LoadState = new D22LoadStateReadModel { IsValid = true },
                AccessibilityState = new D22AccessibilityReadModel
                {
                    IsValid = true,
                    Locale = "en-US",
                },
                Sources = new List<ID22ViewModelSourceAdapter>
                {
                    new D22ViewModelSourceRegistration(
                        D22ViewModelModuleIds.Port,
                        delegate { return portSource; }),
                    new D22ViewModelSourceRegistration(
                        D22ViewModelModuleIds.Market,
                        delegate { return marketSource; }),
                },
            };

            D22ViewModel model = D22ViewModelComposer.ComposeSnapshot(input);
            Check(ref assertions, model.Modules.Count == 7,
                "composer emits every required module in a stable registry");
            Check(ref assertions, model.Modules[0].ModuleId == D22ViewModelModuleIds.Market,
                "market is first in stable order");
            Check(ref assertions, model.Modules[1].ModuleId == D22ViewModelModuleIds.Port,
                "port is second in stable order");
            Check(ref assertions,
                model.Modules[1].Fields[0].Key == "alpha" && model.Modules[1].Fields[1].Key == "zeta",
                "adapter scalar fields are sorted deterministically");
            Check(ref assertions,
                model.Modules[2].Status == D22ViewModelModuleStatus.Missing,
                "missing ship adapter is explicit");
            Check(ref assertions, model.IsDegraded,
                "missing upstream adapters degrade the view model");
            string fingerprint = model.CompositionFingerprint;
            portSource.Fields[0].Value = "changed-after-compose";
            Check(ref assertions, model.CompositionFingerprint == fingerprint,
                "composed view model does not retain source field references");

            var mismatchComposer = new D22ViewModelComposer(new[]
            {
                new D22ViewModelSourceRegistration(
                    D22ViewModelModuleIds.Port,
                    delegate
                    {
                        return new D22ViewModelSourceSnapshot
                        {
                            ModuleId = D22ViewModelModuleIds.Port,
                            SchemaVersion = 2,
                            Available = true,
                        };
                    },
                    1),
            });
            D22ViewModelCompositionInput mismatchInput = input.Clone();
            mismatchInput.Sources = new List<ID22ViewModelSourceAdapter>();
            D22ViewModel mismatch = mismatchComposer.Compose(mismatchInput);
            Check(ref assertions,
                mismatch.Modules[1].Status == D22ViewModelModuleStatus.VersionMismatch,
                "version mismatch is explicit and does not fall back to a fake snapshot");
            Check(ref assertions,
                mismatch.ReasonCodes.Contains("d22.viewmodel.version_mismatch:module.port"),
                "version mismatch reason is preserved");
            Check(ref assertions,
                mismatch.LocalizationKeys.Contains("ui.d22.viewmodel.version_mismatch"),
                "version mismatch localization key is preserved");
        }

        private static void RegisterSuccessOwner(
            D22UiCommandRouter router,
            IDictionary<string, int> calls,
            string ownerSystem)
        {
            calls[ownerSystem] = 0;
            router.Register(ownerSystem, delegate (D22UiCommandRequest request)
            {
                calls[ownerSystem] = calls[ownerSystem] + 1;
                return D22UiOwnerCommandResult.Succeeded(
                    ownerSystem, "accepted", true);
            });
        }

        private static D22UiCommandContext CreateContext()
        {
            return new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.Dialogue,
                ActivePanelId = D22PanelIds.Dialogue,
                FocusedControlId = D22FocusControlIds.Cancel,
                ActorId = "actor.d22",
                WorldDay = 10,
                SaveRevision = 4,
                SessionActive = true,
                ContextValid = true,
                PermissionGranted = true,
                CommandEnabled = true,
            };
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private sealed class Scenario
        {
            public readonly D22UiCommandKind Kind;
            public readonly string TargetId;
            public readonly string OwnerSystem;

            public Scenario(D22UiCommandKind kind, string targetId, string ownerSystem)
            {
                Kind = kind;
                TargetId = targetId;
                OwnerSystem = ownerSystem;
            }
        }
    }
}
