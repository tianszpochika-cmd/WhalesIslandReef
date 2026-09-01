// ============================================================
// D20ActionIntentContracts —— D20-L04 动作意图契约（纯 C#）
//
// 动作意图只表达“请求做什么”，不表达已经移动、交互、捕鱼、登船、掌舵
// 或受击结算。所属业务系统仍是事实裁决者；本文件只冻结稳定动作词汇、
// 请求关联字段、owner 提示和可序列化校验规则。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L04 冻结的八种动作意图。</summary>
    public enum D20ActionIntentKind
    {
        /// <summary>无动作，占位值，不能提交。</summary>
        None = 0,
        /// <summary>以普通速度移动的请求。</summary>
        Walk = 1,
        /// <summary>以较高速度移动的请求。</summary>
        Run = 2,
        /// <summary>对稳定目标发起通用交互的请求。</summary>
        Interact = 3,
        /// <summary>对稳定目标发起交谈的请求。</summary>
        Talk = 4,
        /// <summary>向钓鱼 owner 提交钓鱼意图。</summary>
        Fish = 5,
        /// <summary>向港口/登船 owner 提交登船意图。</summary>
        Board = 6,
        /// <summary>向船舶 owner 提交掌舵意图。</summary>
        Helm = 7,
        /// <summary>记录受击请求/事实入口，不代表伤害已结算。</summary>
        Hit = 8,
    }

    /// <summary>动作意图在命令链中的结果语义。</summary>
    public enum D20ActionIntentStatus
    {
        /// <summary>已接受，等待所属 owner 裁决。</summary>
        Accepted = 0,
        /// <summary>所属 owner 已裁决成功。</summary>
        Succeeded = 1,
        /// <summary>请求或前置被拒绝。</summary>
        Rejected = 2,
        /// <summary>命令 ID 已处理，返回已有结果。</summary>
        Duplicate = 3,
        /// <summary>请求被显式取消。</summary>
        Cancelled = 4,
        /// <summary>角色当前不可接受该意图。</summary>
        Unavailable = 5,
    }

    /// <summary>动作意图契约的稳定校验码。</summary>
    public enum D20ActionIntentValidationCode
    {
        /// <summary>校验通过。</summary>
        None = 0,
        /// <summary>动作意图载体为空。</summary>
        MissingIntent = 1,
        /// <summary>契约版本不支持。</summary>
        InvalidSchemaVersion = 2,
        /// <summary>缺少命令 ID。</summary>
        MissingCommandId = 3,
        /// <summary>缺少或格式非法的行为者 ID。</summary>
        MissingActorId = 4,
        /// <summary>动作枚举未知或为 None。</summary>
        UnknownIntent = 5,
        /// <summary>ActionId 与动作枚举不一致。</summary>
        ActionIdMismatch = 6,
        /// <summary>缺少该动作定义要求的目标 ID。</summary>
        MissingTargetId = 7,
        /// <summary>缺少该动作定义要求的逻辑位置引用。</summary>
        MissingLocationRef = 8,
        /// <summary>owner 提示与冻结定义不一致。</summary>
        OwnerSystemMismatch = 9,
        /// <summary>世界日或存档修订为负数。</summary>
        NegativeRevision = 10,
        /// <summary>参数键重复或非法。</summary>
        InvalidParameter = 11,
        /// <summary>cause event 引用不合法。</summary>
        InvalidCauseEvent = 12,
    }

    /// <summary>动作意图校验结果，保持验证失败可读且不改变原请求。</summary>
    [Serializable]
    public sealed class D20ActionIntentValidationResult
    {
        /// <summary>是否通过契约校验。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20ActionIntentValidationCode Code;

        /// <summary>机器可读原因键，不承载 UI 文案。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功校验结果。</summary>
        public static D20ActionIntentValidationResult Success()
        {
            return new D20ActionIntentValidationResult
            {
                Valid = true,
                Code = D20ActionIntentValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败校验结果。</summary>
        public static D20ActionIntentValidationResult Failure(
            D20ActionIntentValidationCode code,
            string reasonCode)
        {
            return new D20ActionIntentValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>动作 owner 的只读元数据；不授予 owner 之外的写权限。</summary>
    [Serializable]
    public sealed class D20ActionIntentDefinition
    {
        /// <summary>动作枚举。</summary>
        public D20ActionIntentKind Kind;

        /// <summary>跨端稳定动作键。</summary>
        public string ActionId = string.Empty;

        /// <summary>实际业务裁决 owner 的稳定标识。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>该动作是否要求目标稳定键。</summary>
        public bool RequiresTarget;

        /// <summary>该动作是否要求逻辑位置引用。</summary>
        public bool RequiresLocationRef;

        /// <summary>返回副本。</summary>
        public D20ActionIntentDefinition Clone()
        {
            return new D20ActionIntentDefinition
            {
                Kind = Kind,
                ActionId = ActionId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                RequiresTarget = RequiresTarget,
                RequiresLocationRef = RequiresLocationRef,
            };
        }
    }

    /// <summary>动作意图的键值参数；值只允许是 owner 可解释的稳定引用。</summary>
    [Serializable]
    public sealed class D20ActionIntentParameter
    {
        /// <summary>稳定参数名。</summary>
        public string Key = string.Empty;

        /// <summary>不透明参数值，不承载业务数值结算。</summary>
        public string Value = string.Empty;

        /// <summary>返回副本。</summary>
        public D20ActionIntentParameter Clone()
        {
            return new D20ActionIntentParameter
            {
                Key = Key ?? string.Empty,
                Value = Value ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// 可序列化动作意图。请求方只能提交该载体，不能通过字段直接改变所属域事实。
    /// </summary>
    [Serializable]
    public sealed class D20ActionIntent
    {
        /// <summary>契约版本。</summary>
        public int SchemaVersion = D20ActionIntentContracts.CurrentSchemaVersion;

        /// <summary>跨重试不变的命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>动作枚举。</summary>
        public D20ActionIntentKind Kind = D20ActionIntentKind.None;

        /// <summary>与 Kind 对应的稳定动作键。</summary>
        public string ActionId = string.Empty;

        /// <summary>提交意图的角色稳定 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>适用时的目标稳定 ID；移动和受击可为空。</summary>
        public string TargetId = string.Empty;

        /// <summary>逻辑位置或功能点引用，不是空间坐标。</summary>
        public string LocationRef = string.Empty;

        /// <summary>所属业务 owner 的稳定标识。</summary>
        public string OwnerSystem = string.Empty;

        /// <summary>请求发生的世界日。</summary>
        public int WorldDay;

        /// <summary>请求发生时的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>可供事件链追溯的既有事实事件 ID。</summary>
        public List<string> CauseEventIds = new List<string>();

        /// <summary>owner 可解释的额外稳定引用。</summary>
        public List<D20ActionIntentParameter> Parameters = new List<D20ActionIntentParameter>();

        /// <summary>无参构造器供序列化器创建数据载体。</summary>
        public D20ActionIntent() { }

        /// <summary>按冻结定义创建动作意图，自动填充动作键和 owner 提示。</summary>
        public static D20ActionIntent Create(
            D20ActionIntentKind kind,
            string commandId,
            string actorId,
            string targetId = "",
            string locationRef = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            D20ActionIntentDefinition definition;
            if (!D20ActionIntentContracts.TryGetDefinition(kind, out definition))
                throw new ArgumentException("Unknown D20 action intent kind.", "kind");

            return new D20ActionIntent
            {
                CommandId = commandId ?? string.Empty,
                Kind = kind,
                ActionId = definition.ActionId,
                ActorId = actorId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
                LocationRef = locationRef ?? string.Empty,
                OwnerSystem = definition.OwnerSystem,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D20ActionIntent Clone()
        {
            var result = new D20ActionIntent
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                Kind = Kind,
                ActionId = ActionId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                TargetId = TargetId ?? string.Empty,
                LocationRef = LocationRef ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
            if (CauseEventIds != null)
                result.CauseEventIds.AddRange(CauseEventIds);
            if (Parameters != null)
                foreach (var parameter in Parameters)
                    if (parameter != null) result.Parameters.Add(parameter.Clone());
            return result;
        }

        /// <summary>验证该意图，不会自动修正请求字段。</summary>
        public D20ActionIntentValidationResult Validate()
        {
            return D20ActionIntentContracts.Validate(this);
        }
    }

    /// <summary>D20-L04 稳定动作 ID 和定义目录。</summary>
    public static class D20ActionIntentContracts
    {
        /// <summary>当前动作意图契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>动作意图契约 owner 标识。</summary>
        public const string OwnerSystem = "D20ActionIntentContracts";

        /// <summary>Walk 稳定动作 ID。</summary>
        public const string WalkActionId = "walk";

        /// <summary>Run 稳定动作 ID。</summary>
        public const string RunActionId = "run";

        /// <summary>Interact 稳定动作 ID。</summary>
        public const string InteractActionId = "interact";

        /// <summary>Talk 稳定动作 ID。</summary>
        public const string TalkActionId = "talk";

        /// <summary>Fish 稳定动作 ID。</summary>
        public const string FishActionId = "fish";

        /// <summary>Board 稳定动作 ID。</summary>
        public const string BoardActionId = "board";

        /// <summary>Helm 稳定动作 ID。</summary>
        public const string HelmActionId = "helm";

        /// <summary>Hit 稳定动作 ID。</summary>
        public const string HitActionId = "hit";

        /// <summary>动作命令 ID 的最大长度。</summary>
        public const int CommandIdMaxLength = 160;

        /// <summary>稳定目标/位置引用的最大长度。</summary>
        public const int ReferenceMaxLength = 160;

        private static readonly ReadOnlyCollection<D20ActionIntentDefinition> _definitions =
            new ReadOnlyCollection<D20ActionIntentDefinition>(new List<D20ActionIntentDefinition>
            {
                Definition(D20ActionIntentKind.Walk, WalkActionId, "CharacterSystem", false, false),
                Definition(D20ActionIntentKind.Run, RunActionId, "CharacterSystem", false, false),
                Definition(D20ActionIntentKind.Interact, InteractActionId, "InteractionSystem", true, false),
                Definition(D20ActionIntentKind.Talk, TalkActionId, "DialogueSystem", true, false),
                Definition(D20ActionIntentKind.Fish, FishActionId, "D11FishingHarvestSystem", false, false),
                Definition(D20ActionIntentKind.Board, BoardActionId, "D19PortSystem", true, false),
                Definition(D20ActionIntentKind.Helm, HelmActionId, "ShipSystem", false, false),
                Definition(D20ActionIntentKind.Hit, HitActionId, "HealthSystem", false, false),
            });

        /// <summary>返回冻结动作定义的只读集合。</summary>
        public static IReadOnlyList<D20ActionIntentDefinition> Definitions
        {
            get
            {
                var copy = new List<D20ActionIntentDefinition>(_definitions.Count);
                for (int i = 0; i < _definitions.Count; i++)
                    copy.Add(_definitions[i].Clone());
                return new ReadOnlyCollection<D20ActionIntentDefinition>(copy);
            }
        }

        /// <summary>判断动作枚举是否属于八个冻结意图。</summary>
        public static bool IsSupported(D20ActionIntentKind kind)
        {
            D20ActionIntentDefinition definition;
            return TryGetDefinition(kind, out definition);
        }

        /// <summary>按枚举读取动作定义副本。</summary>
        public static bool TryGetDefinition(D20ActionIntentKind kind,
            out D20ActionIntentDefinition definition)
        {
            for (int i = 0; i < _definitions.Count; i++)
            {
                if (_definitions[i].Kind == kind)
                {
                    definition = _definitions[i].Clone();
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>按稳定动作 ID 读取动作定义，大小写必须精确匹配。</summary>
        public static bool TryGetDefinition(string actionId,
            out D20ActionIntentDefinition definition)
        {
            for (int i = 0; i < _definitions.Count; i++)
            {
                if (string.Equals(_definitions[i].ActionId, actionId,
                    StringComparison.Ordinal))
                {
                    definition = _definitions[i].Clone();
                    return true;
                }
            }
            definition = null;
            return false;
        }

        /// <summary>返回动作的稳定 ID，未知动作返回空字符串。</summary>
        public static string ActionIdFor(D20ActionIntentKind kind)
        {
            D20ActionIntentDefinition definition;
            return TryGetDefinition(kind, out definition) ? definition.ActionId : string.Empty;
        }

        /// <summary>返回动作的业务 owner 提示，未知动作返回空字符串。</summary>
        public static string OwnerSystemFor(D20ActionIntentKind kind)
        {
            D20ActionIntentDefinition definition;
            return TryGetDefinition(kind, out definition) ? definition.OwnerSystem : string.Empty;
        }

        /// <summary>校验动作意图的版本、稳定引用、动作定义和参数唯一性。</summary>
        public static D20ActionIntentValidationResult Validate(D20ActionIntent intent)
        {
            if (intent == null)
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingIntent, "intent_missing");
            if (intent.SchemaVersion != CurrentSchemaVersion)
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.InvalidSchemaVersion, "schema_version_mismatch");
            if (!IsStableReference(intent.CommandId, CommandIdMaxLength))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingCommandId, "missing_command_id");
            if (!D20CharacterIdentityContracts.IsStableCharacterId(intent.ActorId))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingActorId, "missing_actor_id");

            D20ActionIntentDefinition definition;
            if (!TryGetDefinition(intent.Kind, out definition))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.UnknownIntent, "unknown_action_intent");
            if (!string.Equals(intent.ActionId, definition.ActionId, StringComparison.Ordinal))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.ActionIdMismatch, "action_id_mismatch");
            if (!string.Equals(intent.OwnerSystem, definition.OwnerSystem, StringComparison.Ordinal))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.OwnerSystemMismatch, "owner_system_mismatch");
            if (definition.RequiresTarget &&
                !IsStableReference(intent.TargetId, ReferenceMaxLength))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingTargetId, "missing_target_id");
            if (definition.RequiresLocationRef &&
                !IsStableReference(intent.LocationRef, ReferenceMaxLength))
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.MissingLocationRef, "missing_location_ref");
            if (intent.WorldDay < 0 || intent.SaveRevision < 0)
                return D20ActionIntentValidationResult.Failure(
                    D20ActionIntentValidationCode.NegativeRevision, "negative_revision");

            if (intent.CauseEventIds != null)
            {
                foreach (var causeEventId in intent.CauseEventIds)
                {
                    if (!IsStableReference(causeEventId, ReferenceMaxLength))
                        return D20ActionIntentValidationResult.Failure(
                            D20ActionIntentValidationCode.InvalidCauseEvent, "invalid_cause_event");
                }
            }

            if (intent.Parameters != null)
            {
                var keys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var parameter in intent.Parameters)
                {
                    if (parameter == null ||
                        !IsStableReference(parameter.Key, ReferenceMaxLength) ||
                        parameter.Value == null || parameter.Value.Length > ReferenceMaxLength ||
                        !keys.Add(parameter.Key))
                        return D20ActionIntentValidationResult.Failure(
                            D20ActionIntentValidationCode.InvalidParameter, "invalid_action_parameter");
                }
            }

            return D20ActionIntentValidationResult.Success();
        }

        private static D20ActionIntentDefinition Definition(
            D20ActionIntentKind kind,
            string actionId,
            string ownerSystem,
            bool requiresTarget,
            bool requiresLocationRef)
        {
            return new D20ActionIntentDefinition
            {
                Kind = kind,
                ActionId = actionId,
                OwnerSystem = ownerSystem,
                RequiresTarget = requiresTarget,
                RequiresLocationRef = requiresLocationRef,
            };
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator) return false;
            }
            return true;
        }
    }
}
