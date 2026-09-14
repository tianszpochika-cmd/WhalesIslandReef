using System;

namespace Whalebone.Gameplay.D30
{
    /// <summary>
    /// D30 视觉阶段事件的纯逻辑裁决器。
    /// 
    /// 这里故意只处理事件归一化、重复事件和阶段投影，不读取 Unity 时间或场景，
    /// 这样 UI/MonoBehaviour 适配层可以在 EditMode 中验证失败与重复请求不会推进视觉阶段。
    /// 业务事实仍由 D2/D3/D4/D9/D10 owner 裁决，本类不修改任何业务状态。
    /// </summary>
    public static class D30VisualStagePolicy
    {
        public const int HarborStage = 0;
        public const int MarketStage = 1;
        public const int TavernStage = 2;
        public const int ShipyardStage = 3;
        public const int ShipStage = 4;
        public const int VoyageStage = 5;
        public const int ReturnStage = 6;

        public enum EventOutcome
        {
            Unknown = 0,
            Failed = 1,
            Advanced = 2,
            Duplicate = 3,
        }

        public struct Decision
        {
            public Decision(int nextStage, EventOutcome outcome, string normalizedEventId)
            {
                NextStage = nextStage;
                Outcome = outcome;
                NormalizedEventId = normalizedEventId;
            }

            public int NextStage { get; private set; }
            public EventOutcome Outcome { get; private set; }
            public string NormalizedEventId { get; private set; }

            public bool IsStageAdvance
            {
                get { return Outcome == EventOutcome.Advanced; }
            }

            public bool IsFailure
            {
                get { return Outcome == EventOutcome.Failed; }
            }

            public bool IsDuplicate
            {
                get { return Outcome == EventOutcome.Duplicate; }
            }
        }

        /// <summary>
        /// 将一次 D29/D30 事件投影为可显示阶段。
        /// 成功事件即使目标阶段与当前阶段相同（例如 preparation_opened）也算一次
        /// 有效阶段事件；失败、未知和短窗口重复事件永远保持当前阶段。
        /// </summary>
        public static Decision Evaluate(int currentStage, string eventId,
            string lastEventId, float now, float lastEventTime, float repeatGuardSeconds)
        {
            var normalized = Normalize(eventId);
            var normalizedLast = Normalize(lastEventId);
            // Only events understood by this projection may enter the repeat
            // window. Unknown diagnostics or future event ids must remain
            // Unknown on every evaluation; otherwise a second unknown id is
            // misleadingly reported as a duplicate interaction.
            var repeatable = IsRepeatableEvent(normalized);
            var duplicate = repeatable &&
                string.Equals(normalizedLast, normalized, StringComparison.Ordinal) &&
                IsWithinRepeatWindow(now, lastEventTime, repeatGuardSeconds);
            if (duplicate)
                return new Decision(currentStage, EventOutcome.Duplicate, normalized);

            switch (normalized)
            {
                case "market_opened":
                    return new Decision(MarketStage, EventOutcome.Advanced, normalized);
                case "tavern_opened":
                    return new Decision(TavernStage, EventOutcome.Advanced, normalized);
                case "shipyard_opened":
                    return new Decision(ShipyardStage, EventOutcome.Advanced, normalized);
                case "preparation_opened":
                    return new Decision(HarborStage, EventOutcome.Advanced, normalized);
                case "boarded":
                    return new Decision(ShipStage, EventOutcome.Advanced, normalized);
                case "departed":
                    return new Decision(VoyageStage, EventOutcome.Advanced, normalized);
                case "returned":
                    return new Decision(ReturnStage, EventOutcome.Advanced, normalized);
                default:
                    if (IsNormalizedFailureEvent(normalized))
                        return new Decision(currentStage, EventOutcome.Failed, normalized);
                    return new Decision(currentStage, EventOutcome.Unknown, normalized);
            }
        }

        /// <summary>
        /// D29/D30 异常恢复使用的失败事件白名单。失败事件只能反馈当前状态，
        /// 不能把视觉路线推进到一个“成功”阶段；公开此判定让完整阶段矩阵
        /// 可以在无 Unity 场景的纯逻辑测试中覆盖。
        /// </summary>
        public static bool IsFailureEvent(string eventId)
        {
            var normalized = (eventId ?? string.Empty).Trim().ToLowerInvariant();
            return IsNormalizedFailureEvent(normalized);
        }

        private static bool IsNormalizedFailureEvent(string normalized)
        {
            switch (normalized)
            {
                case "market_failed":
                case "tavern_failed":
                case "shipyard_failed":
                case "preparation_failed":
                case "board_failed":
                case "save_failed":
                case "weather_failed":
                case "quality_failed":
                    return true;
                default:
                    return false;
            }
        }

        private static string Normalize(string eventId)
        {
            return (eventId ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsWithinRepeatWindow(float now, float lastEventTime, float repeatGuardSeconds)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) ||
                float.IsNaN(lastEventTime) || float.IsInfinity(lastEventTime) ||
                float.IsNaN(repeatGuardSeconds) || float.IsInfinity(repeatGuardSeconds) ||
                repeatGuardSeconds <= 0f)
                return false;

            var elapsed = now - lastEventTime;
            // Treat a backwards clock as outside the short presentation guard;
            // it must not turn a temporary UI protection into a sticky block.
            return elapsed >= 0f && elapsed < repeatGuardSeconds;
        }

        private static bool IsRepeatableEvent(string normalized)
        {
            switch (normalized)
            {
                case "market_opened":
                case "tavern_opened":
                case "shipyard_opened":
                case "preparation_opened":
                case "boarded":
                case "departed":
                case "returned":
                    return true;
                default:
                    return IsNormalizedFailureEvent(normalized);
            }
        }
    }
}
