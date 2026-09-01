// D22-L25：命令合法性、禁用原因、owner 反馈和幂等的纯逻辑测试。

using System;
using Whalebone.Systems.D22;

namespace Whalebone.Systems.D22.Tests
{
    public static class D22CommandFeedbackTests
    {
        public static int RunAll()
        {
            int assertions = 0;
            D22UiCommandContext context = CreateContext();
            D22UiCommandRequest valid = D22UiCommandRequest.Create(
                D22UiCommandKind.ShipAction,
                "command:d22:valid",
                "correlation:d22:valid",
                context.ActorId,
                context.ActiveScreenId,
                context.ActivePanelId,
                "ship:d22:test",
                context.WorldDay,
                context.SaveRevision);
            D22UiCommandValidationResult accepted =
                D22UiCommandValidator.Validate(valid, context);
            Check(ref assertions, accepted.Valid &&
                accepted.Code == D22UiCommandValidationCode.None,
                "valid command reaches owner validation");

            D22UiCommandRequest invalid = valid.Clone();
            invalid.CommandId = string.Empty;
            D22UiCommandValidationResult rejected =
                D22UiCommandValidator.Validate(invalid, context);
            Check(ref assertions, !rejected.Valid &&
                rejected.Code == D22UiCommandValidationCode.MissingCommandId,
                "malformed command is rejected before owner invocation");

            context.PermissionGranted = false;
            D22UiCommandValidationResult forbidden =
                D22UiCommandValidator.Validate(valid, context);
            Check(ref assertions, !forbidden.Valid &&
                forbidden.Code == D22UiCommandValidationCode.PermissionDenied,
                "permission failure remains a stable validation result");

            var router = new D22UiCommandRouter();
            int ownerCalls = 0;
            router.Register("Ship/Voyage", delegate (D22UiCommandRequest request)
            {
                ownerCalls++;
                return D22UiOwnerCommandResult.Succeeded(
                    "Ship/Voyage", "ship_action_applied", true);
            });
            context.PermissionGranted = true;
            D22UiCommandRouteResult routed = router.RouteDetailed(valid, context);
            Check(ref assertions, routed.OwnerInvoked &&
                routed.CommandResult.Outcome == D22UiCommandOutcome.Succeeded,
                "owner success is projected as command success");
            Check(ref assertions, routed.Feedback.Code == D22FeedbackCodes.CommandCompleted,
                "owner success maps to the stable completed feedback code");
            D22UiCommandResult replay = router.Route(valid.Clone(), context.Clone());
            Check(ref assertions, replay.Duplicate &&
                replay.Outcome == D22UiCommandOutcome.Duplicate && ownerCalls == 1,
                "same command is idempotent and owner runs once");

            D22UiCommandRequest missingOwner = D22UiCommandRequest.Create(
                D22UiCommandKind.MarketAction,
                "command:d22:missing-owner",
                "correlation:d22:missing-owner",
                context.ActorId,
                context.ActiveScreenId,
                context.ActivePanelId,
                "market:d22:test",
                context.WorldDay,
                context.SaveRevision);
            D22UiCommandResult blocked = new D22UiCommandRouter().Route(
                missingOwner, context);
            Check(ref assertions, blocked.Outcome == D22UiCommandOutcome.Blocked &&
                !blocked.Accepted && !blocked.BusinessStateChanged,
                "missing owner fails closed without business mutation");
            Check(ref assertions, blocked.FeedbackCode == D22FeedbackCodes.OwnerUnavailable,
                "missing owner preserves stable feedback");
            return assertions;
        }

        private static D22UiCommandContext CreateContext()
        {
            return new D22UiCommandContext
            {
                ActiveScreenId = D22ScreenIds.Dialogue,
                ActivePanelId = D22PanelIds.Dialogue,
                FocusedControlId = D22FocusControlIds.Cancel,
                ActorId = "actor:d22:test",
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
    }
}
