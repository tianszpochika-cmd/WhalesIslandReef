// D20NpcInteractionAvailability —— D20-L18 NPC 交互资格的纯 C# 只读评估。
//
// 该文件把行为状态、角色可用性、可达性、拒绝和冷却统一成稳定的结果码。
// 它只读取 L14/L15 的契约与目录，不启动交易、对话、关系或库存写入，
// 因而按钮/表现层不能用“可见”替代真实业务 owner 的裁决。

using System;

namespace Whalebone.Systems.D20
{
    /// <summary>NPC 可被玩家请求的交互类别。</summary>
    public enum D20NpcInteractionKind
    {
        Unknown = 0,
        Talk = 1,
        Trade = 2,
        Service = 3,
        Inspect = 4,
        Rumor = 5,
    }

    /// <summary>NPC 交互资格的稳定失败码。</summary>
    public enum D20NpcInteractionFailureCode
    {
        None = 0,
        InvalidRequest = 1,
        MissingNpcId = 2,
        UnknownInteraction = 3,
        Dead = 4,
        Departed = 5,
        Injured = 6,
        Unreachable = 7,
        Cooldown = 8,
        Refused = 9,
        Busy = 10,
        Unavailable = 11,
        Alarm = 12,
        RestRequired = 13,
        RoleUnknown = 14,
        RoleCapabilityMissing = 15,
    }

    /// <summary>交互不可用时的确定性重试策略。</summary>
    public enum D20NpcInteractionRetryPolicy
    {
        None = 0,
        RetryWhenReachable = 1,
        RetryAfterCooldown = 2,
        RetryAfterRefusalCleared = 3,
        RetryWhenNotBusy = 4,
        RetryAfterOwnerRecovery = 5,
        RetryWhenDangerClears = 6,
        RetryAfterRest = 7,
        Never = 8,
    }

    /// <summary>交互资格请求的稳定校验码。</summary>
    public enum D20NpcInteractionRequestValidationCode
    {
        None = 0,
        MissingRequest = 1,
        InvalidSchemaVersion = 2,
        MissingNpcId = 3,
        InvalidRoleId = 4,
        UnknownInteraction = 5,
        UnknownBehaviorState = 6,
        UnknownAvailability = 7,
        InvalidLocationRef = 8,
        NegativeCooldown = 9,
        NegativeWorldDay = 10,
        NegativeSaveRevision = 11,
        UnknownDangerLevel = 12,
    }

    /// <summary>交互资格请求校验结果。</summary>
    [Serializable]
    public sealed class D20NpcInteractionRequestValidationResult
    {
        /// <summary>请求是否有效。</summary>
        public bool Valid;

        /// <summary>稳定校验码。</summary>
        public D20NpcInteractionRequestValidationCode Code;

        /// <summary>机器可读原因。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>创建成功结果。</summary>
        public static D20NpcInteractionRequestValidationResult Success()
        {
            return new D20NpcInteractionRequestValidationResult
            {
                Valid = true,
                Code = D20NpcInteractionRequestValidationCode.None,
                ReasonCode = string.Empty,
            };
        }

        /// <summary>创建失败结果。</summary>
        public static D20NpcInteractionRequestValidationResult Failure(
            D20NpcInteractionRequestValidationCode code,
            string reasonCode)
        {
            return new D20NpcInteractionRequestValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }

        /// <summary>返回防御性副本。</summary>
        public D20NpcInteractionRequestValidationResult Clone()
        {
            return new D20NpcInteractionRequestValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>只读 NPC 交互资格请求；不含坐标和引擎对象。</summary>
    [Serializable]
    public sealed class D20NpcInteractionAvailabilityRequest
    {
        /// <summary>请求契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>请求契约版本。</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>岗位引用，由 L15 目录解释。</summary>
        public string RoleId = string.Empty;

        /// <summary>要查询的交互类别。</summary>
        public D20NpcInteractionKind InteractionKind = D20NpcInteractionKind.Unknown;

        /// <summary>行为状态机提供的当前状态。</summary>
        public D20NpcBehaviorState BehaviorState = D20NpcBehaviorState.Unknown;

        /// <summary>身份/健康 owner 提供的动作可用性。</summary>
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;

        /// <summary>行为 owner 是否报告当前工作占用。</summary>
        public bool IsBusy;

        /// <summary>语义交互点是否可达；不表示寻路结果或坐标。</summary>
        public bool IsReachable = true;

        /// <summary>owner 是否明确拒绝本次交互。</summary>
        public bool IsRefused;

        /// <summary>健康 owner 是否报告受伤。</summary>
        public bool IsInjured;

        /// <summary>剩余逻辑冷却 tick，必须由上游以确定性值提供。</summary>
        public int CooldownRemainingTicks;

        /// <summary>健康/疲劳 owner 是否要求休息。</summary>
        public bool IsRestRequired;

        /// <summary>当前逻辑危险等级。</summary>
        public D20NpcDangerLevel DangerLevel = D20NpcDangerLevel.None;

        /// <summary>可选的稳定交互点引用。</summary>
        public string LocationRef = string.Empty;

        /// <summary>请求对应的世界日。</summary>
        public int WorldDay;

        /// <summary>请求对应的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>创建交互资格请求。</summary>
        public static D20NpcInteractionAvailabilityRequest Create(
            string npcId,
            string roleId,
            D20NpcInteractionKind interactionKind,
            D20NpcBehaviorState behaviorState,
            D20CharacterAvailability availability,
            bool isReachable = true,
            bool isBusy = false,
            bool isRefused = false,
            bool isInjured = false,
            int cooldownRemainingTicks = 0,
            bool isRestRequired = false,
            D20NpcDangerLevel dangerLevel = D20NpcDangerLevel.None,
            string locationRef = "",
            int worldDay = 0,
            long saveRevision = 0)
        {
            return new D20NpcInteractionAvailabilityRequest
            {
                NpcId = npcId ?? string.Empty,
                RoleId = roleId ?? string.Empty,
                InteractionKind = interactionKind,
                BehaviorState = behaviorState,
                Availability = availability,
                IsReachable = isReachable,
                IsBusy = isBusy,
                IsRefused = isRefused,
                IsInjured = isInjured,
                CooldownRemainingTicks = cooldownRemainingTicks,
                IsRestRequired = isRestRequired,
                DangerLevel = dangerLevel,
                LocationRef = locationRef ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
            };
        }

        /// <summary>返回深拷贝。</summary>
        public D20NpcInteractionAvailabilityRequest Clone()
        {
            return new D20NpcInteractionAvailabilityRequest
            {
                SchemaVersion = SchemaVersion,
                NpcId = NpcId ?? string.Empty,
                RoleId = RoleId ?? string.Empty,
                InteractionKind = InteractionKind,
                BehaviorState = BehaviorState,
                Availability = Availability,
                IsBusy = IsBusy,
                IsReachable = IsReachable,
                IsRefused = IsRefused,
                IsInjured = IsInjured,
                CooldownRemainingTicks = CooldownRemainingTicks,
                IsRestRequired = IsRestRequired,
                DangerLevel = DangerLevel,
                LocationRef = LocationRef ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>验证请求；失败时保留原输入供审计。</summary>
        public D20NpcInteractionRequestValidationResult Validate()
        {
            return D20NpcInteractionAvailability.Validate(this);
        }
    }

    /// <summary>统一的只读交互资格结果。</summary>
    [Serializable]
    public sealed class D20NpcInteractionAvailabilityResult
    {
        /// <summary>是否可向所属业务 owner 提交交互意图。</summary>
        public bool Available;

        /// <summary>所有字段都来自只读评估，不代表业务已成功。</summary>
        public bool IsReadOnly = true;

        /// <summary>NPC 稳定 ID。</summary>
        public string NpcId = string.Empty;

        /// <summary>交互类别。</summary>
        public D20NpcInteractionKind InteractionKind;

        /// <summary>评估时的行为状态。</summary>
        public D20NpcBehaviorState BehaviorState;

        /// <summary>评估时的可用性。</summary>
        public D20CharacterAvailability Availability;

        /// <summary>需要的岗位能力。</summary>
        public D20NpcRoleCapability RequiredCapability;

        /// <summary>稳定失败码；可用时为 None。</summary>
        public D20NpcInteractionFailureCode FailureCode;

        /// <summary>稳定原因码。</summary>
        public string ReasonCode = string.Empty;

        /// <summary>供 UI/Presentation 使用的稳定本地化键。</summary>
        public string LocalizationKey = string.Empty;

        /// <summary>建议重试策略。</summary>
        public D20NpcInteractionRetryPolicy RetryPolicy;

        /// <summary>评估时仍剩余的冷却 tick。</summary>
        public int CooldownRemainingTicks;

        /// <summary>评估对应的世界日。</summary>
        public int WorldDay;

        /// <summary>评估对应的存档修订。</summary>
        public long SaveRevision;

        /// <summary>返回深拷贝。</summary>
        public D20NpcInteractionAvailabilityResult Clone()
        {
            return new D20NpcInteractionAvailabilityResult
            {
                Available = Available,
                IsReadOnly = true,
                NpcId = NpcId ?? string.Empty,
                InteractionKind = InteractionKind,
                BehaviorState = BehaviorState,
                Availability = Availability,
                RequiredCapability = RequiredCapability,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                RetryPolicy = RetryPolicy,
                CooldownRemainingTicks = CooldownRemainingTicks,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>
    /// D20-L18 NPC 交互资格的单一只读评估入口。返回 Available 只说明
    /// 前置允许提交请求，绝不代表交易、关系、库存或对话事实已经成功。
    /// </summary>
    public static class D20NpcInteractionAvailability
    {
        /// <summary>评估器 owner 标识。</summary>
        public const string OwnerSystem = "D20NpcInteractionAvailability";

        /// <summary>验证交互资格请求。</summary>
        public static D20NpcInteractionRequestValidationResult Validate(
            D20NpcInteractionAvailabilityRequest request)
        {
            if (request == null)
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.MissingRequest,
                    "npc_interaction_request_missing");
            if (request.SchemaVersion != D20NpcInteractionAvailabilityRequest.CurrentSchemaVersion)
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.InvalidSchemaVersion,
                    "npc_interaction_schema_version_mismatch");
            if (!D20NpcIdentityContracts.IsStableNpcId(request.NpcId))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.MissingNpcId,
                    "npc_interaction_npc_id_missing");
            if (!D20NpcIdentityContracts.IsStableReference(request.RoleId, true))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.InvalidRoleId,
                    "npc_interaction_role_id_invalid");
            if (!IsSupportedInteraction(request.InteractionKind))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.UnknownInteraction,
                    "npc_interaction_kind_unknown");
            if (!D20NpcBehaviorStateMachine.IsSupportedState(request.BehaviorState))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.UnknownBehaviorState,
                    "npc_interaction_behavior_state_unknown");
            if (!D20CharacterIdentityContracts.IsSupportedAvailability(request.Availability))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.UnknownAvailability,
                    "npc_interaction_availability_unknown");
            if (!D20NpcIdentityContracts.IsStableReference(request.LocationRef, true))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.InvalidLocationRef,
                    "npc_interaction_location_ref_invalid");
            if (request.CooldownRemainingTicks < 0)
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.NegativeCooldown,
                    "npc_interaction_cooldown_negative");
            if (request.WorldDay < 0)
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.NegativeWorldDay,
                    "npc_interaction_world_day_negative");
            if (request.SaveRevision < 0)
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.NegativeSaveRevision,
                    "npc_interaction_save_revision_negative");
            if (!D20NpcScheduleInputContracts.IsSupportedDangerLevel(request.DangerLevel))
                return D20NpcInteractionRequestValidationResult.Failure(
                    D20NpcInteractionRequestValidationCode.UnknownDangerLevel,
                    "npc_interaction_danger_level_unknown");

            return D20NpcInteractionRequestValidationResult.Success();
        }

        /// <summary>评估是否可以提交交互意图。</summary>
        public static D20NpcInteractionAvailabilityResult Evaluate(
            D20NpcInteractionAvailabilityRequest request)
        {
            D20NpcInteractionAvailabilityResult result = CreateBaseResult(request);
            D20NpcInteractionRequestValidationResult validation = Validate(request);
            if (!validation.Valid)
                return Reject(result, D20NpcInteractionFailureCode.InvalidRequest,
                    validation.ReasonCode, D20NpcInteractionRetryPolicy.Never);

            result.NpcId = request.NpcId;
            result.InteractionKind = request.InteractionKind;
            result.BehaviorState = request.BehaviorState;
            result.Availability = request.Availability;
            result.RequiredCapability = RequiredCapabilityFor(request.InteractionKind);
            result.CooldownRemainingTicks = request.CooldownRemainingTicks;
            result.WorldDay = request.WorldDay;
            result.SaveRevision = request.SaveRevision;

            if (request.Availability == D20CharacterAvailability.Dead)
                return Reject(result, D20NpcInteractionFailureCode.Dead,
                    "npc_interaction_dead", D20NpcInteractionRetryPolicy.Never);
            if (request.Availability == D20CharacterAvailability.Departed)
                return Reject(result, D20NpcInteractionFailureCode.Departed,
                    "npc_interaction_departed", D20NpcInteractionRetryPolicy.Never);
            if (request.IsInjured ||
                request.BehaviorState == D20NpcBehaviorState.Injured ||
                request.Availability == D20CharacterAvailability.Incapacitated)
                return Reject(result, D20NpcInteractionFailureCode.Injured,
                    "npc_interaction_injured", D20NpcInteractionRetryPolicy.RetryAfterOwnerRecovery);
            if (request.BehaviorState == D20NpcBehaviorState.Alarm ||
                D20NpcScheduleInputContracts.IsAlarmDanger(request.DangerLevel))
                return Reject(result, D20NpcInteractionFailureCode.Alarm,
                    "npc_interaction_alarm", D20NpcInteractionRetryPolicy.RetryWhenDangerClears);
            if (!request.IsReachable)
                return Reject(result, D20NpcInteractionFailureCode.Unreachable,
                    "npc_interaction_unreachable", D20NpcInteractionRetryPolicy.RetryWhenReachable);
            if (request.CooldownRemainingTicks > 0)
                return Reject(result, D20NpcInteractionFailureCode.Cooldown,
                    "npc_interaction_cooldown", D20NpcInteractionRetryPolicy.RetryAfterCooldown);
            if (request.IsRefused || request.BehaviorState == D20NpcBehaviorState.Refuse)
                return Reject(result, D20NpcInteractionFailureCode.Refused,
                    "npc_interaction_refused", D20NpcInteractionRetryPolicy.RetryAfterRefusalCleared);
            if (request.IsRestRequired)
                return Reject(result, D20NpcInteractionFailureCode.RestRequired,
                    "npc_interaction_rest_required", D20NpcInteractionRetryPolicy.RetryAfterRest);
            if (request.IsBusy || request.Availability == D20CharacterAvailability.Busy ||
                request.BehaviorState == D20NpcBehaviorState.Work ||
                request.BehaviorState == D20NpcBehaviorState.Trade ||
                request.BehaviorState == D20NpcBehaviorState.Talk)
                return Reject(result, D20NpcInteractionFailureCode.Busy,
                    "npc_interaction_busy", D20NpcInteractionRetryPolicy.RetryWhenNotBusy);
            if (request.Availability == D20CharacterAvailability.Unavailable)
                return Reject(result, D20NpcInteractionFailureCode.Unavailable,
                    "npc_interaction_unavailable", D20NpcInteractionRetryPolicy.RetryAfterOwnerRecovery);

            D20NpcRoleDefinition role = D20NpcRoleCatalog.Find(request.RoleId);
            if (role == null)
                return Reject(result, D20NpcInteractionFailureCode.RoleUnknown,
                    "npc_interaction_role_unknown", D20NpcInteractionRetryPolicy.Never);
            if (result.RequiredCapability != D20NpcRoleCapability.None &&
                (role.Capabilities & result.RequiredCapability) != result.RequiredCapability)
                return Reject(result, D20NpcInteractionFailureCode.RoleCapabilityMissing,
                    "npc_interaction_role_capability_missing", D20NpcInteractionRetryPolicy.Never);

            result.Available = true;
            result.FailureCode = D20NpcInteractionFailureCode.None;
            result.ReasonCode = "npc_interaction_available";
            result.LocalizationKey = "npc.interaction.available";
            result.RetryPolicy = D20NpcInteractionRetryPolicy.None;
            return result;
        }

        /// <summary>只返回资格布尔值，完整原因请使用 Evaluate。</summary>
        public static bool CanInteract(
            D20NpcInteractionAvailabilityRequest request)
        {
            return Evaluate(request).Available;
        }

        /// <summary>验证交互枚举是否支持。</summary>
        public static bool IsSupportedInteraction(D20NpcInteractionKind interactionKind)
        {
            return interactionKind >= D20NpcInteractionKind.Talk &&
                   interactionKind <= D20NpcInteractionKind.Rumor;
        }

        /// <summary>返回交互所需的岗位能力。</summary>
        public static D20NpcRoleCapability RequiredCapabilityFor(
            D20NpcInteractionKind interactionKind)
        {
            switch (interactionKind)
            {
                case D20NpcInteractionKind.Talk:
                    return D20NpcRoleCapability.Dialogue;
                case D20NpcInteractionKind.Trade:
                    return D20NpcRoleCapability.Trade;
                case D20NpcInteractionKind.Service:
                    return D20NpcRoleCapability.Service;
                case D20NpcInteractionKind.Inspect:
                    return D20NpcRoleCapability.Inspection;
                case D20NpcInteractionKind.Rumor:
                    return D20NpcRoleCapability.Rumor;
                default:
                    return D20NpcRoleCapability.None;
            }
        }

        private static D20NpcInteractionAvailabilityResult CreateBaseResult(
            D20NpcInteractionAvailabilityRequest request)
        {
            return new D20NpcInteractionAvailabilityResult
            {
                Available = false,
                IsReadOnly = true,
                NpcId = request == null ? string.Empty : request.NpcId ?? string.Empty,
                InteractionKind = request == null
                    ? D20NpcInteractionKind.Unknown : request.InteractionKind,
                BehaviorState = request == null
                    ? D20NpcBehaviorState.Unknown : request.BehaviorState,
                Availability = request == null
                    ? D20CharacterAvailability.Unknown : request.Availability,
                RequiredCapability = request == null
                    ? D20NpcRoleCapability.None : RequiredCapabilityFor(request.InteractionKind),
                FailureCode = D20NpcInteractionFailureCode.InvalidRequest,
                ReasonCode = string.Empty,
                LocalizationKey = "npc.interaction.unavailable",
                RetryPolicy = D20NpcInteractionRetryPolicy.Never,
                CooldownRemainingTicks = request == null ? 0 : request.CooldownRemainingTicks,
                WorldDay = request == null ? 0 : request.WorldDay,
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
        }

        private static D20NpcInteractionAvailabilityResult Reject(
            D20NpcInteractionAvailabilityResult result,
            D20NpcInteractionFailureCode failureCode,
            string reasonCode,
            D20NpcInteractionRetryPolicy retryPolicy)
        {
            result.Available = false;
            result.IsReadOnly = true;
            result.FailureCode = failureCode;
            result.ReasonCode = reasonCode ?? string.Empty;
            result.LocalizationKey = "npc.interaction." +
                InteractionKeyFor(failureCode);
            result.RetryPolicy = retryPolicy;
            return result;
        }

        private static string InteractionKeyFor(
            D20NpcInteractionFailureCode failureCode)
        {
            switch (failureCode)
            {
                case D20NpcInteractionFailureCode.Dead: return "dead";
                case D20NpcInteractionFailureCode.Departed: return "departed";
                case D20NpcInteractionFailureCode.Injured: return "injured";
                case D20NpcInteractionFailureCode.Unreachable: return "unreachable";
                case D20NpcInteractionFailureCode.Cooldown: return "cooldown";
                case D20NpcInteractionFailureCode.Refused: return "refused";
                case D20NpcInteractionFailureCode.Busy: return "busy";
                case D20NpcInteractionFailureCode.Unavailable: return "unavailable";
                case D20NpcInteractionFailureCode.Alarm: return "alarm";
                case D20NpcInteractionFailureCode.RestRequired: return "rest_required";
                case D20NpcInteractionFailureCode.RoleUnknown: return "role_unknown";
                case D20NpcInteractionFailureCode.RoleCapabilityMissing:
                    return "role_capability_missing";
                case D20NpcInteractionFailureCode.InvalidRequest:
                    return "invalid_request";
                default: return "unavailable";
            }
        }
    }
}
