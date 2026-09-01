// ============================================================
// D20ActionPriorityRules —— D20-L05 动作优先级与互斥规则（纯 C#）
//
// 本文件只消费 D20-L04 的动作意图契约，决定一个意图是否满足角色可用性、
// presence、目标和 owner 权限前置，以及它能否打断当前动作。它不执行动作，
// 不写业务 owner 事实，也不把优先级结果当成动画或结算成功。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    /// <summary>动作优先级，数值越大越有资格打断较低优先级动作。</summary>
    public enum D20ActionPriority
    {
        /// <summary>无效动作。</summary>
        None = 0,
        /// <summary>普通移动。</summary>
        Walk = 10,
        /// <summary>快速移动。</summary>
        Run = 20,
        /// <summary>通用交互。</summary>
        Interact = 30,
        /// <summary>交谈。</summary>
        Talk = 40,
        /// <summary>钓鱼作业。</summary>
        Fish = 50,
        /// <summary>掌舵作业。</summary>
        Helm = 60,
        /// <summary>登船状态过渡。</summary>
        Board = 70,
        /// <summary>受击反馈/事实入口。</summary>
        Hit = 100,
    }

    /// <summary>动作互斥集；同一非空集合内不能并行保持两个动作。</summary>
    public enum D20ActionExclusiveGroup
    {
        /// <summary>不参加互斥。</summary>
        None = 0,
        /// <summary>走与跑互斥。</summary>
        Locomotion = 1,
        /// <summary>交互与交谈互斥。</summary>
        Social = 2,
        /// <summary>钓鱼作业互斥。</summary>
        Fishing = 3,
        /// <summary>掌舵作业互斥。</summary>
        Helm = 4,
        /// <summary>登船过渡互斥。</summary>
        Boarding = 5,
        /// <summary>受击动作互斥。</summary>
        Impact = 6,
    }

    /// <summary>动作启动前必须满足的前置集合。</summary>
    [Flags]
    public enum D20ActionPrerequisite
    {
        /// <summary>没有额外前置。</summary>
        None = 0,
        /// <summary>角色必须处于可行动状态。</summary>
        CharacterAvailable = 1,
        /// <summary>presence 必须在港口；与其它 presence 位按“任一满足”解释。</summary>
        PresencePort = 2,
        /// <summary>presence 必须在船上；与其它 presence 位按“任一满足”解释。</summary>
        PresenceShip = 4,
        /// <summary>presence 必须在航行中；与其它 presence 位按“任一满足”解释。</summary>
        PresenceVoyage = 8,
        /// <summary>意图必须有目标稳定键。</summary>
        Target = 16,
        /// <summary>所属业务 owner 必须明确允许该请求。</summary>
        OwnerPermission = 32,
    }

    /// <summary>动作优先级规则的纯数据定义。</summary>
    [Serializable]
    public sealed class D20ActionPriorityRule
    {
        /// <summary>对应的 L04 动作枚举。</summary>
        public D20ActionIntentKind Kind;

        /// <summary>动作优先级。</summary>
        public D20ActionPriority Priority;

        /// <summary>所属互斥集。</summary>
        public D20ActionExclusiveGroup ExclusiveGroup;

        /// <summary>开始动作所需的前置。</summary>
        public D20ActionPrerequisite Prerequisites;

        /// <summary>该动作能否被更高优先级动作打断。</summary>
        public bool Interruptible;

        /// <summary>返回防御性副本。</summary>
        public D20ActionPriorityRule Clone()
        {
            return new D20ActionPriorityRule
            {
                Kind = Kind,
                Priority = Priority,
                ExclusiveGroup = ExclusiveGroup,
                Prerequisites = Prerequisites,
                Interruptible = Interruptible,
            };
        }
    }

    /// <summary>动作优先级判断所需的只读上下文。</summary>
    [Serializable]
    public sealed class D20ActionPriorityContext
    {
        /// <summary>角色当前 presence。</summary>
        public D20PlayerPresenceState PresenceState = D20PlayerPresenceState.Unknown;

        /// <summary>由健康/行为 owner 提供的可行动判断。</summary>
        public bool CharacterAvailable = true;

        /// <summary>所属业务 owner 是否允许本次请求。</summary>
        public bool OwnerPermission = true;

        /// <summary>调用方提供的目标存在事实；意图自身有目标键时也视为有目标。</summary>
        public bool HasTarget;

        /// <summary>当前占用动作槽的意图；为空表示没有活动动作。</summary>
        public D20ActionIntent ActiveIntent;

        /// <summary>返回不共享意图引用的副本。</summary>
        public D20ActionPriorityContext Clone()
        {
            return new D20ActionPriorityContext
            {
                PresenceState = PresenceState,
                CharacterAvailable = CharacterAvailable,
                OwnerPermission = OwnerPermission,
                HasTarget = HasTarget,
                ActiveIntent = ActiveIntent == null ? null : ActiveIntent.Clone(),
            };
        }
    }

    /// <summary>动作优先级规则的确定性判定结果。</summary>
    [Serializable]
    public sealed class D20ActionPriorityDecision
    {
        /// <summary>是否满足动作自身前置且可以开始。</summary>
        public bool Allowed;

        /// <summary>是否会打断当前动作。</summary>
        public bool CanInterrupt;

        /// <summary>机器可读判定原因键。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>候选动作优先级。</summary>
        public D20ActionPriority Priority;

        /// <summary>候选动作互斥集。</summary>
        public D20ActionExclusiveGroup ExclusiveGroup;

        /// <summary>缺失的前置集合；允许时为 None。</summary>
        public D20ActionPrerequisite MissingPrerequisites;

        /// <summary>当前占用动作槽的动作类型。</summary>
        public D20ActionIntentKind ActiveKind;

        /// <summary>返回深拷贝。</summary>
        public D20ActionPriorityDecision Clone()
        {
            return new D20ActionPriorityDecision
            {
                Allowed = Allowed,
                CanInterrupt = CanInterrupt,
                ReasonCode = ReasonCode ?? string.Empty,
                Priority = Priority,
                ExclusiveGroup = ExclusiveGroup,
                MissingPrerequisites = MissingPrerequisites,
                ActiveKind = ActiveKind,
            };
        }
    }

    /// <summary>
    /// D20-L05 的动作优先级、互斥、前置和可打断单一规则表。
    /// </summary>
    public static class D20ActionPriorityRules
    {
        /// <summary>规则 owner 标识。</summary>
        public const string OwnerSystem = "D20ActionPriorityRules";

        private static readonly ReadOnlyCollection<D20ActionPriorityRule> _rules =
            new ReadOnlyCollection<D20ActionPriorityRule>(new List<D20ActionPriorityRule>
            {
                Rule(D20ActionIntentKind.Walk, D20ActionPriority.Walk,
                    D20ActionExclusiveGroup.Locomotion,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresencePort |
                    D20ActionPrerequisite.PresenceShip, true),
                Rule(D20ActionIntentKind.Run, D20ActionPriority.Run,
                    D20ActionExclusiveGroup.Locomotion,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresencePort |
                    D20ActionPrerequisite.PresenceShip, true),
                Rule(D20ActionIntentKind.Interact, D20ActionPriority.Interact,
                    D20ActionExclusiveGroup.Social,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresencePort |
                    D20ActionPrerequisite.PresenceShip |
                    D20ActionPrerequisite.Target, true),
                Rule(D20ActionIntentKind.Talk, D20ActionPriority.Talk,
                    D20ActionExclusiveGroup.Social,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresencePort |
                    D20ActionPrerequisite.PresenceShip |
                    D20ActionPrerequisite.Target, true),
                Rule(D20ActionIntentKind.Fish, D20ActionPriority.Fish,
                    D20ActionExclusiveGroup.Fishing,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresenceShip |
                    D20ActionPrerequisite.PresenceVoyage, true),
                Rule(D20ActionIntentKind.Helm, D20ActionPriority.Helm,
                    D20ActionExclusiveGroup.Helm,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresenceShip |
                    D20ActionPrerequisite.PresenceVoyage |
                    D20ActionPrerequisite.OwnerPermission, true),
                Rule(D20ActionIntentKind.Board, D20ActionPriority.Board,
                    D20ActionExclusiveGroup.Boarding,
                    D20ActionPrerequisite.CharacterAvailable |
                    D20ActionPrerequisite.PresencePort |
                    D20ActionPrerequisite.Target, true),
                Rule(D20ActionIntentKind.Hit, D20ActionPriority.Hit,
                    D20ActionExclusiveGroup.Impact,
                    D20ActionPrerequisite.None, false),
            });

        /// <summary>返回规则的防御性只读副本集合。</summary>
        public static IReadOnlyList<D20ActionPriorityRule> Rules
        {
            get
            {
                var copy = new List<D20ActionPriorityRule>(_rules.Count);
                for (int i = 0; i < _rules.Count; i++)
                    copy.Add(_rules[i].Clone());
                return new ReadOnlyCollection<D20ActionPriorityRule>(copy);
            }
        }

        /// <summary>按动作读取规则副本。</summary>
        public static bool TryGetRule(D20ActionIntentKind kind,
            out D20ActionPriorityRule rule)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                if (_rules[i].Kind == kind)
                {
                    rule = _rules[i].Clone();
                    return true;
                }
            }
            rule = null;
            return false;
        }

        /// <summary>返回动作优先级；未知动作返回 None。</summary>
        public static D20ActionPriority GetPriority(D20ActionIntentKind kind)
        {
            D20ActionPriorityRule rule;
            return TryGetRule(kind, out rule) ? rule.Priority : D20ActionPriority.None;
        }

        /// <summary>返回动作互斥集；未知动作返回 None。</summary>
        public static D20ActionExclusiveGroup GetExclusiveGroup(D20ActionIntentKind kind)
        {
            D20ActionPriorityRule rule;
            return TryGetRule(kind, out rule)
                ? rule.ExclusiveGroup
                : D20ActionExclusiveGroup.None;
        }

        /// <summary>返回动作前置集合；未知动作返回 None。</summary>
        public static D20ActionPrerequisite GetPrerequisites(D20ActionIntentKind kind)
        {
            D20ActionPriorityRule rule;
            return TryGetRule(kind, out rule)
                ? rule.Prerequisites
                : D20ActionPrerequisite.None;
        }

        /// <summary>判断两个动作是否属于同一互斥关系。</summary>
        public static bool AreMutuallyExclusive(D20ActionIntentKind left,
            D20ActionIntentKind right)
        {
            if (left == D20ActionIntentKind.None || right == D20ActionIntentKind.None)
                return false;
            if (left == D20ActionIntentKind.Hit || right == D20ActionIntentKind.Hit)
                return true;
            D20ActionExclusiveGroup leftGroup = GetExclusiveGroup(left);
            D20ActionExclusiveGroup rightGroup = GetExclusiveGroup(right);
            return leftGroup != D20ActionExclusiveGroup.None && leftGroup == rightGroup;
        }

        /// <summary>判断当前动作是否允许被更高优先级动作打断。</summary>
        public static bool CanInterrupt(D20ActionIntentKind incoming,
            D20ActionIntentKind current)
        {
            if (incoming == D20ActionIntentKind.None || current == D20ActionIntentKind.None)
                return current == D20ActionIntentKind.None && incoming != D20ActionIntentKind.None;
            if (!AreMutuallyExclusive(incoming, current))
                return false;

            D20ActionPriorityRule incomingRule;
            D20ActionPriorityRule currentRule;
            if (!TryGetRule(incoming, out incomingRule) ||
                !TryGetRule(current, out currentRule))
                return false;
            return currentRule.Interruptible && incomingRule.Priority > currentRule.Priority;
        }

        /// <summary>按当前活动意图判断新意图能否打断它。</summary>
        public static bool CanInterrupt(D20ActionIntent incoming,
            D20ActionIntent current)
        {
            if (incoming == null)
                return false;
            return current == null || CanInterrupt(incoming.Kind,
                current == null ? D20ActionIntentKind.None : current.Kind);
        }

        /// <summary>
        /// 评估动作前置和当前动作互斥关系。该方法只返回判定，不执行或写入动作。
        /// </summary>
        public static D20ActionPriorityDecision Evaluate(
            D20ActionIntent intent,
            D20ActionPriorityContext context = null)
        {
            var safeContext = context == null
                ? new D20ActionPriorityContext()
                : context.Clone();
            var decision = new D20ActionPriorityDecision
            {
                Allowed = false,
                CanInterrupt = false,
                ActiveKind = safeContext.ActiveIntent == null
                    ? D20ActionIntentKind.None
                    : safeContext.ActiveIntent.Kind,
            };

            D20ActionIntentValidationResult validation = intent == null
                ? D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingIntent, "intent_missing")
                : intent.Validate();
            if (!validation.Valid)
            {
                decision.ReasonCode = validation.ReasonCode;
                return decision;
            }

            D20ActionPriorityRule rule;
            if (!TryGetRule(intent.Kind, out rule))
            {
                decision.ReasonCode = "priority_rule_missing";
                return decision;
            }
            decision.Priority = rule.Priority;
            decision.ExclusiveGroup = rule.ExclusiveGroup;

            D20ActionPrerequisite missing = MissingPrerequisites(intent, rule, safeContext);
            decision.MissingPrerequisites = missing;
            if (missing != D20ActionPrerequisite.None)
            {
                decision.ReasonCode = "action_prerequisite_missing";
                return decision;
            }

            if (safeContext.ActiveIntent == null ||
                safeContext.ActiveIntent.Kind == D20ActionIntentKind.None)
            {
                decision.Allowed = true;
                decision.ReasonCode = "action_allowed";
                return decision;
            }
            if (safeContext.ActiveIntent.Kind == intent.Kind)
            {
                decision.ReasonCode = "action_already_active";
                return decision;
            }
            if (!AreMutuallyExclusive(intent.Kind, safeContext.ActiveIntent.Kind))
            {
                decision.Allowed = true;
                decision.ReasonCode = "action_parallel_group_allowed";
                return decision;
            }
            if (!CanInterrupt(intent, safeContext.ActiveIntent))
            {
                decision.ReasonCode = "action_not_interruptible";
                return decision;
            }

            decision.Allowed = true;
            decision.CanInterrupt = true;
            decision.ReasonCode = "action_interrupt_allowed";
            return decision;
        }

        /// <summary>校验规则表覆盖所有 L04 动作且优先级唯一递增。</summary>
        public static List<string> ValidateCatalog()
        {
            var errors = new List<string>();
            if (_rules.Count != 8) errors.Add("rule_count_invalid");
            var seen = new HashSet<D20ActionIntentKind>();
            var seenPriorities = new HashSet<D20ActionPriority>();
            D20ActionPriority previousPriority = D20ActionPriority.None;
            for (int i = 0; i < _rules.Count; i++)
            {
                D20ActionPriorityRule rule = _rules[i];
                if (!D20ActionIntentContracts.IsSupported(rule.Kind))
                    errors.Add("unsupported_rule_" + rule.Kind);
                if (!seen.Add(rule.Kind)) errors.Add("duplicate_rule_" + rule.Kind);
                if (rule.Priority == D20ActionPriority.None)
                    errors.Add("missing_priority_" + rule.Kind);
                if (!seenPriorities.Add(rule.Priority))
                    errors.Add("duplicate_priority_" + rule.Priority);
                if (i > 0 && rule.Priority <= previousPriority)
                    errors.Add("priority_order_invalid_" + rule.Kind);
                if (rule.ExclusiveGroup == D20ActionExclusiveGroup.None)
                    errors.Add("missing_group_" + rule.Kind);
                previousPriority = rule.Priority;
            }
            return errors;
        }

        private static D20ActionPriorityRule Rule(
            D20ActionIntentKind kind,
            D20ActionPriority priority,
            D20ActionExclusiveGroup group,
            D20ActionPrerequisite prerequisites,
            bool interruptible)
        {
            return new D20ActionPriorityRule
            {
                Kind = kind,
                Priority = priority,
                ExclusiveGroup = group,
                Prerequisites = prerequisites,
                Interruptible = interruptible,
            };
        }

        private static D20ActionPrerequisite MissingPrerequisites(
            D20ActionIntent intent,
            D20ActionPriorityRule rule,
            D20ActionPriorityContext context)
        {
            D20ActionPrerequisite missing = D20ActionPrerequisite.None;
            D20ActionPrerequisite prerequisites = rule.Prerequisites;
            if ((prerequisites & D20ActionPrerequisite.CharacterAvailable) != 0 &&
                !context.CharacterAvailable)
                missing |= D20ActionPrerequisite.CharacterAvailable;
            if ((prerequisites & D20ActionPrerequisite.OwnerPermission) != 0 &&
                !context.OwnerPermission)
                missing |= D20ActionPrerequisite.OwnerPermission;
            if ((prerequisites & D20ActionPrerequisite.Target) != 0 &&
                !context.HasTarget && string.IsNullOrEmpty(intent.TargetId))
                missing |= D20ActionPrerequisite.Target;

            D20ActionPrerequisite presenceMask = prerequisites &
                (D20ActionPrerequisite.PresencePort |
                 D20ActionPrerequisite.PresenceShip |
                 D20ActionPrerequisite.PresenceVoyage);
            if (presenceMask != D20ActionPrerequisite.None &&
                !MatchesPresence(presenceMask, context.PresenceState))
                missing |= presenceMask;
            return missing;
        }

        private static bool MatchesPresence(D20ActionPrerequisite presenceMask,
            D20PlayerPresenceState state)
        {
            if (state == D20PlayerPresenceState.Port &&
                (presenceMask & D20ActionPrerequisite.PresencePort) != 0)
                return true;
            if (state == D20PlayerPresenceState.Ship &&
                (presenceMask & D20ActionPrerequisite.PresenceShip) != 0)
                return true;
            if (state == D20PlayerPresenceState.Voyage &&
                (presenceMask & D20ActionPrerequisite.PresenceVoyage) != 0)
                return true;
            return false;
        }
    }
}
