// D20CharacterConditionAdapter —— D10 健康/疲劳/行为状态的只读动作投影。
//
// 健康、疲劳和状态仍由 CrewSystem/D10CrewBehaviorSystem 写入与裁决。本文件
// 只消费公开的 CrewStats 和 D10CrewBehaviorState，不保存副本、不钳制数值，
// 并把拒绝原因转换为稳定的 D20 反馈码。

using System;
using Whalebone.Agent;

namespace Whalebone.Systems.D20
{
    public enum D20CharacterConditionLevel
    {
        Unknown = 0,
        Ready = 1,
        Fatigued = 2,
        Refusing = 3,
        Injured = 4,
        Unavailable = 5,
        Incapacitated = 6,
        Dead = 7,
        Departed = 8,
    }

    public enum D20CharacterConditionFailureCode
    {
        None = 0,
        MissingInput = 1,
        InvalidValue = 2,
        UnknownAction = 3,
        Dead = 4,
        Departed = 5,
        Unavailable = 6,
        InjuryRestriction = 7,
        FatigueRestriction = 8,
        RefusalRestriction = 9,
    }

    [Serializable]
    public sealed class D20CharacterConditionEvaluation
    {
        public D20ActionIntentKind ActionKind;
        public bool Eligible;
        public bool ConditionDataAvailable;
        public bool BehaviorDataAvailable;
        public D20CharacterConditionLevel Level;
        public D20CharacterConditionFailureCode FailureCode;
        public float Health;
        public float Fatigue;
        public CrewBehaviorStatus BehaviorStatus;
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;

        public D20CharacterConditionEvaluation Clone()
        {
            return new D20CharacterConditionEvaluation
            {
                ActionKind = ActionKind,
                Eligible = Eligible,
                ConditionDataAvailable = ConditionDataAvailable,
                BehaviorDataAvailable = BehaviorDataAvailable,
                Level = Level,
                FailureCode = FailureCode,
                Health = Health,
                Fatigue = Fatigue,
                BehaviorStatus = BehaviorStatus,
                ReasonCode = ReasonCode,
                FeedbackKey = FeedbackKey,
            };
        }
    }

    public static class D20CharacterConditionAdapter
    {
        public const string OwnerSystem = "D20CharacterConditionAdapter";

        /// <summary>从 D10 的公开数值和行为状态计算 D20 动作资格。</summary>
        public static D20CharacterConditionEvaluation Evaluate(
            CrewStats stats,
            D10CrewBehaviorState behaviorState,
            D20ActionIntentKind actionKind)
        {
            if (stats == null)
            {
                var missing = CreateBase(actionKind);
                missing.Eligible = false;
                missing.FailureCode = D20CharacterConditionFailureCode.MissingInput;
                missing.ReasonCode = "condition_input_missing";
                missing.FeedbackKey = "condition.feedback.unavailable";
                return missing;
            }

            var result = Evaluate(stats.Health, stats.Fatigue,
                behaviorState == null ? CrewBehaviorStatus.Unassigned : behaviorState.Status,
                actionKind);
            result.BehaviorDataAvailable = behaviorState != null;
            if (behaviorState == null && result.FailureCode == D20CharacterConditionFailureCode.None)
            {
                result.ReasonCode = "condition_behavior_missing_safe_default";
                result.FeedbackKey = "condition.feedback.partial";
            }
            return result;
        }

        /// <summary>
        /// 纯值入口供契约测试和其他公开快照消费者使用；数值仍必须来自
        /// D10/CrewSystem 的公开状态，适配器不负责生成或修正它们。
        /// </summary>
        public static D20CharacterConditionEvaluation Evaluate(
            float health,
            float fatigue,
            CrewBehaviorStatus behaviorStatus,
            D20ActionIntentKind actionKind)
        {
            var result = CreateBase(actionKind);
            result.ConditionDataAvailable = true;
            result.BehaviorDataAvailable = true;
            result.Health = health;
            result.Fatigue = fatigue;
            result.BehaviorStatus = behaviorStatus;

            if (!D20ActionIntentContracts.IsSupported(actionKind))
            {
                result.Eligible = false;
                result.Level = D20CharacterConditionLevel.Unknown;
                result.FailureCode = D20CharacterConditionFailureCode.UnknownAction;
                result.ReasonCode = "action_unknown";
                result.FeedbackKey = "condition.feedback.unknown_action";
                return result;
            }

            if (!IsFiniteInRange(health) || !IsFiniteInRange(fatigue)
                || !Enum.IsDefined(typeof(CrewBehaviorStatus), behaviorStatus))
            {
                result.Eligible = false;
                result.Level = D20CharacterConditionLevel.Unknown;
                result.FailureCode = D20CharacterConditionFailureCode.InvalidValue;
                result.ReasonCode = "condition_value_invalid";
                result.FeedbackKey = "condition.feedback.invalid";
                return result;
            }

            if (health <= 0f)
            {
                return Restrict(result, D20CharacterConditionLevel.Dead,
                    D20CharacterConditionFailureCode.Dead, "condition_dead", "condition.feedback.dead");
            }

            if (behaviorStatus == CrewBehaviorStatus.Departed)
            {
                return Restrict(result, D20CharacterConditionLevel.Departed,
                    D20CharacterConditionFailureCode.Departed, "condition_departed", "condition.feedback.departed");
            }

            if (IsUnavailableStatus(behaviorStatus))
            {
                result.Level = D20CharacterConditionLevel.Unavailable;
                if (IsUnavailableAction(actionKind))
                {
                    return Restrict(result, D20CharacterConditionLevel.Unavailable,
                        D20CharacterConditionFailureCode.Unavailable,
                        "condition_unavailable", "condition.feedback.unavailable");
                }

                result.ReasonCode = "condition_unavailable_safe_action";
                result.FeedbackKey = "condition.feedback.safe_action";
                return result;
            }

            // These thresholds intentionally mirror D10CrewBehaviorConfiguration;
            // this adapter reads those canonical values instead of duplicating them.
            if (behaviorStatus == CrewBehaviorStatus.Injured
                || health < D10CrewBehaviorConfiguration.InjuredHealthThreshold)
            {
                result.Level = D20CharacterConditionLevel.Injured;
                if (IsInjuryRestrictedAction(actionKind))
                {
                    return Restrict(result, D20CharacterConditionLevel.Injured,
                        D20CharacterConditionFailureCode.InjuryRestriction,
                        "condition_injured", "condition.feedback.injured");
                }

                result.ReasonCode = "condition_injured_safe_action";
                result.FeedbackKey = "condition.feedback.safe_action";
                return result;
            }

            if (behaviorStatus == CrewBehaviorStatus.Refusing
                || health < D10CrewBehaviorConfiguration.RefusalHealthThreshold
                || fatigue >= D10CrewBehaviorConfiguration.RefusalFatigueThreshold)
            {
                result.Level = D20CharacterConditionLevel.Refusing;
                if (IsRefusalRestrictedAction(actionKind))
                {
                    return Restrict(result, D20CharacterConditionLevel.Refusing,
                        D20CharacterConditionFailureCode.RefusalRestriction,
                        "condition_refusing", "condition.feedback.refusing");
                }

                result.ReasonCode = "condition_refusing_safe_action";
                result.FeedbackKey = "condition.feedback.safe_action";
                return result;
            }

            if (behaviorStatus == CrewBehaviorStatus.Slacking
                || health < D10CrewBehaviorConfiguration.SlackingHealthThreshold
                || fatigue >= D10CrewBehaviorConfiguration.SlackingFatigueThreshold)
            {
                result.Level = D20CharacterConditionLevel.Fatigued;
                if (IsFatigueRestrictedAction(actionKind))
                {
                    return Restrict(result, D20CharacterConditionLevel.Fatigued,
                        D20CharacterConditionFailureCode.FatigueRestriction,
                        "condition_fatigued", "condition.feedback.fatigued");
                }

                result.ReasonCode = "condition_fatigued_safe_action";
                result.FeedbackKey = "condition.feedback.safe_action";
                return result;
            }

            result.Level = D20CharacterConditionLevel.Ready;
            result.ReasonCode = "condition_ready";
            result.FeedbackKey = "condition.feedback.ready";
            return result;
        }

        private static D20CharacterConditionEvaluation CreateBase(D20ActionIntentKind actionKind)
        {
            return new D20CharacterConditionEvaluation
            {
                ActionKind = actionKind,
                Eligible = true,
                Level = D20CharacterConditionLevel.Unknown,
                FailureCode = D20CharacterConditionFailureCode.None,
                BehaviorStatus = CrewBehaviorStatus.Unassigned,
            };
        }

        private static D20CharacterConditionEvaluation Restrict(
            D20CharacterConditionEvaluation result,
            D20CharacterConditionLevel level,
            D20CharacterConditionFailureCode failureCode,
            string reasonCode,
            string feedbackKey)
        {
            result.Eligible = false;
            result.Level = level;
            result.FailureCode = failureCode;
            result.ReasonCode = reasonCode;
            result.FeedbackKey = feedbackKey;
            return result;
        }

        private static bool IsFiniteInRange(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f && value <= 100f;
        }

        private static bool IsUnavailableStatus(CrewBehaviorStatus status)
        {
            return status == CrewBehaviorStatus.LeaveRequested
                || status == CrewBehaviorStatus.OnLeave
                || status == CrewBehaviorStatus.TransferRequested
                || status == CrewBehaviorStatus.DepartureRequested;
        }

        private static bool IsUnavailableAction(D20ActionIntentKind actionKind)
        {
            return actionKind != D20ActionIntentKind.Talk
                && actionKind != D20ActionIntentKind.Interact;
        }

        private static bool IsInjuryRestrictedAction(D20ActionIntentKind actionKind)
        {
            return actionKind == D20ActionIntentKind.Run
                || actionKind == D20ActionIntentKind.Fish
                || actionKind == D20ActionIntentKind.Board
                || actionKind == D20ActionIntentKind.Helm
                || actionKind == D20ActionIntentKind.Hit;
        }

        private static bool IsRefusalRestrictedAction(D20ActionIntentKind actionKind)
        {
            return actionKind == D20ActionIntentKind.Run
                || actionKind == D20ActionIntentKind.Fish
                || actionKind == D20ActionIntentKind.Board
                || actionKind == D20ActionIntentKind.Helm;
        }

        private static bool IsFatigueRestrictedAction(D20ActionIntentKind actionKind)
        {
            return actionKind == D20ActionIntentKind.Run
                || actionKind == D20ActionIntentKind.Fish
                || actionKind == D20ActionIntentKind.Board
                || actionKind == D20ActionIntentKind.Helm;
        }
    }
}
