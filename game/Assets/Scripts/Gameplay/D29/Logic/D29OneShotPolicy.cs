using System;

namespace Whalebone.Gameplay.D29
{
    /// <summary>
    /// D29 表现层一次性请求防重策略。
    /// 它只判断同一动作是否仍处于短暂提交窗口，不替代业务 owner 的幂等键、
    /// 权限和拒绝裁决；因此可以脱离 Unity 时间和 UI 单独回归。
    /// </summary>
    public static class D29OneShotPolicy
    {
        public struct Decision
        {
            public Decision(bool allowed, bool duplicate, string normalizedActionId)
            {
                Allowed = allowed;
                IsDuplicate = duplicate;
                NormalizedActionId = normalizedActionId;
            }

            public bool Allowed { get; private set; }
            public bool IsDuplicate { get; private set; }
            public string NormalizedActionId { get; private set; }
        }

        /// <summary>
        /// 计算一次表现层提交是否应继续进入 owner。
        /// 同一动作在 guard 窗口内只被拦截重复提交；不同动作和窗口外重试不互相阻塞。
        /// </summary>
        public static Decision Evaluate(string actionId, string lastActionId,
            float now, float lastActionTime, float repeatGuardSeconds)
        {
            var normalized = Normalize(actionId);
            var normalizedLast = Normalize(lastActionId);
            var duplicate = !string.IsNullOrEmpty(normalized) &&
                string.Equals(normalizedLast, normalized, StringComparison.Ordinal) &&
                IsWithinRepeatWindow(now, lastActionTime, repeatGuardSeconds);
            return new Decision(!duplicate, duplicate, normalized);
        }

        private static string Normalize(string actionId)
        {
            return (actionId ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsWithinRepeatWindow(float now, float lastActionTime, float repeatGuardSeconds)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) ||
                float.IsNaN(lastActionTime) || float.IsInfinity(lastActionTime) ||
                float.IsNaN(repeatGuardSeconds) || float.IsInfinity(repeatGuardSeconds) ||
                repeatGuardSeconds <= 0f)
                return false;

            var elapsed = now - lastActionTime;
            // A monotonic time source is expected.  If a test clock or a
            // platform clock moves backwards, fail open for this presentation
            // guard instead of incorrectly reporting a duplicate forever.
            return elapsed >= 0f && elapsed < repeatGuardSeconds;
        }
    }
}
