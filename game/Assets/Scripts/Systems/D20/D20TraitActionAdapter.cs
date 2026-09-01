// D20TraitActionAdapter —— D5 特质到动作资格的只读投影。
//
// D5TraitSystem 仍是特质目录、校验和修正值的唯一写者/裁决者。本适配器只
// 读取 D5 的公开定义，把少量与动作直接相关的修正投影为稳定的资格与反馈
// 码；不会复制、修改或重新计算特质数值。

using System;
using System.Collections.Generic;
using Whalebone.Systems.D5;

namespace Whalebone.Systems.D20
{
    public enum D20TraitActionDecision
    {
        Unknown = 0,
        Neutral = 1,
        Eligible = 2,
        Restricted = 3,
    }

    public enum D20TraitActionFailureCode
    {
        None = 0,
        MissingTraitInput = 1,
        InvalidTraitSet = 2,
        UnknownAction = 3,
        TraitRestriction = 4,
    }

    [Serializable]
    public sealed class D20TraitActionEvaluation
    {
        public D20ActionIntentKind ActionKind;
        public bool Eligible;
        public bool TraitDataAvailable;
        public D20TraitActionDecision Decision;
        public D20TraitActionFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string FeedbackKey = string.Empty;
        public List<string> MatchedTraitIds = new List<string>();

        public D20TraitActionEvaluation Clone()
        {
            var result = new D20TraitActionEvaluation
            {
                ActionKind = ActionKind,
                Eligible = Eligible,
                TraitDataAvailable = TraitDataAvailable,
                Decision = Decision,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode,
                FeedbackKey = FeedbackKey,
                MatchedTraitIds = new List<string>(),
            };

            if (MatchedTraitIds != null) result.MatchedTraitIds.AddRange(MatchedTraitIds);
            return result;
        }
    }

    public static class D20TraitActionAdapter
    {
        public const string OwnerSystem = "D20TraitActionAdapter";

        /// <summary>
        /// Evaluates an action against an existing D5 trait set. A missing trait
        /// projection is a safe neutral result; an invalid projection is denied
        /// so callers cannot accidentally treat malformed data as valid.
        /// </summary>
        public static D20TraitActionEvaluation Evaluate(
            D20ActionIntentKind actionKind,
            IEnumerable<string> traitIds)
        {
            var result = new D20TraitActionEvaluation
            {
                ActionKind = actionKind,
                Eligible = true,
                Decision = D20TraitActionDecision.Neutral,
                ReasonCode = "trait_no_action_mapping",
                FeedbackKey = "trait.feedback.neutral",
            };

            if (!D20ActionIntentContracts.IsSupported(actionKind))
            {
                result.Eligible = false;
                result.Decision = D20TraitActionDecision.Unknown;
                result.FailureCode = D20TraitActionFailureCode.UnknownAction;
                result.ReasonCode = "action_unknown";
                result.FeedbackKey = "trait.feedback.unknown_action";
                return result;
            }

            if (traitIds == null)
            {
                result.FailureCode = D20TraitActionFailureCode.MissingTraitInput;
                result.ReasonCode = "trait_input_missing_safe_default";
                result.FeedbackKey = "trait.feedback.unavailable";
                return result;
            }

            var ids = new List<string>();
            foreach (var traitId in traitIds) ids.Add(traitId);

            // Reuse the owner validation. No local normalization or duplicate
            // rules are introduced here.
            if (!D5TraitSystem.IsValidTraitSet(ids, 0, 4))
            {
                result.Eligible = false;
                result.Decision = D20TraitActionDecision.Unknown;
                result.FailureCode = D20TraitActionFailureCode.InvalidTraitSet;
                result.ReasonCode = "trait_input_invalid";
                result.FeedbackKey = "trait.feedback.invalid";
                return result;
            }

            result.TraitDataAvailable = true;
            bool hasMapping = false;
            bool hasPositiveMapping = false;
            bool hasHardRestriction = false;

            foreach (var traitId in ids)
            {
                var definition = D5TraitCatalog.Find(traitId);
                if (definition == null) continue;

                int direction;
                bool hardRestriction;
                if (!TryGetActionBinding(definition.Modifier, actionKind, out direction, out hardRestriction))
                    continue;

                hasMapping = true;
                result.MatchedTraitIds.Add(definition.Id);

                // Read the canonical value through D5TraitSystem rather than
                // maintaining a second modifier table in D20.
                int value = D5TraitSystem.Modifier(definition.Id, definition.Modifier);
                if (value == 0) continue;

                if (direction > 0 && value > 0) hasPositiveMapping = true;
                if (direction > 0 && value < 0 && hardRestriction) hasHardRestriction = true;
            }

            if (hasHardRestriction)
            {
                result.Eligible = false;
                result.Decision = D20TraitActionDecision.Restricted;
                result.FailureCode = D20TraitActionFailureCode.TraitRestriction;
                result.ReasonCode = "trait_action_restricted";
                result.FeedbackKey = "trait.feedback.restricted";
                return result;
            }

            if (hasMapping && hasPositiveMapping)
            {
                result.Decision = D20TraitActionDecision.Eligible;
                result.ReasonCode = "trait_action_qualified";
                result.FeedbackKey = "trait.feedback.qualified";
            }
            else if (hasMapping)
            {
                result.ReasonCode = "trait_action_feedback_only";
                result.FeedbackKey = "trait.feedback.consider_condition";
            }

            return result;
        }

        private static bool TryGetActionBinding(
            string modifier,
            D20ActionIntentKind actionKind,
            out int direction,
            out bool hardRestriction)
        {
            direction = 0;
            hardRestriction = false;
            if (string.IsNullOrEmpty(modifier)) return false;

            switch (modifier)
            {
                case "move_speed":
                    if (actionKind == D20ActionIntentKind.Walk || actionKind == D20ActionIntentKind.Run)
                    {
                        direction = 1;
                        hardRestriction = actionKind == D20ActionIntentKind.Run;
                        return true;
                    }
                    return false;

                case "boarding":
                    if (actionKind == D20ActionIntentKind.Board)
                    {
                        direction = 1;
                        hardRestriction = true;
                        return true;
                    }
                    return false;

                case "sailing":
                case "night_watch":
                    if (actionKind == D20ActionIntentKind.Helm)
                    {
                        direction = 1;
                        return true;
                    }
                    return false;

                case "trade":
                case "profit":
                case "negotiation":
                case "culture":
                case "intimidation":
                case "conflict":
                    return BindTo(actionKind, D20ActionIntentKind.Talk, D20ActionIntentKind.Interact, out direction);

                case "combat":
                case "combat_damage":
                case "morale_combat":
                case "rival_damage":
                case "cannon_reload":
                    return BindTo(actionKind, D20ActionIntentKind.Hit, out direction);

                case "exploration":
                case "sight":
                case "island_safety":
                case "map":
                case "inspection":
                case "medical":
                case "medicine":
                case "disease_resist":
                    return BindTo(actionKind, D20ActionIntentKind.Interact, D20ActionIntentKind.Fish, out direction);

                default:
                    return false;
            }
        }

        private static bool BindTo(
            D20ActionIntentKind actionKind,
            D20ActionIntentKind first,
            D20ActionIntentKind second,
            out int direction)
        {
            if (actionKind == first || actionKind == second)
            {
                direction = 1;
                return true;
            }

            direction = 0;
            return false;
        }

        private static bool BindTo(
            D20ActionIntentKind actionKind,
            D20ActionIntentKind expected,
            out int direction)
        {
            if (actionKind == expected)
            {
                direction = 1;
                return true;
            }

            direction = 0;
            return false;
        }
    }
}
