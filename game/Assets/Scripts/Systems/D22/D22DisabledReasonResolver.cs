// ============================================================
// D22DisabledReasonResolver —— D22-L08 业务前置失败到禁用读模型（纯 C#）
//
// Resolver 只接受结构化 owner 失败码，不解析按钮颜色、文案或 Canvas 状态。
// 它返回稳定禁用码、帮助本地化 key 和恢复建议，不会修改 owner 或 UI 状态。
// ============================================================

using System;

namespace Whalebone.Systems.D22
{
    /// <summary>D22 禁用/帮助读模型使用的稳定码。</summary>
    public static class D22DisabledReasonCodes
    {
        /// <summary>没有禁用原因。</summary>
        public const string None = "";
        /// <summary>权限不足。</summary>
        public const string PermissionDenied = "d22.disabled.permission_denied";
        /// <summary>上下文不合法。</summary>
        public const string InvalidContext = "d22.disabled.invalid_context";
        /// <summary>缺少目标。</summary>
        public const string MissingTarget = "d22.disabled.missing_target";
        /// <summary>命令冷却中。</summary>
        public const string CooldownActive = "d22.disabled.cooldown_active";
        /// <summary>存档修订过期。</summary>
        public const string StaleRevision = "d22.disabled.stale_revision";
        /// <summary>owner 暂时不可用。</summary>
        public const string OwnerUnavailable = "d22.disabled.owner_unavailable";
        /// <summary>owner 拒绝前置。</summary>
        public const string OwnerRejected = "d22.disabled.owner_rejected";
        /// <summary>等待 owner 结果。</summary>
        public const string OwnerPending = "d22.disabled.owner_pending";
        /// <summary>现金或信用不足。</summary>
        public const string InsufficientFunds = "d22.disabled.insufficient_funds";
        /// <summary>容量不足。</summary>
        public const string CapacityExceeded = "d22.disabled.capacity_exceeded";
        /// <summary>港口或服务不可用。</summary>
        public const string PortUnavailable = "d22.disabled.port_unavailable";
        /// <summary>船舶尚未准备好。</summary>
        public const string ShipNotReady = "d22.disabled.ship_not_ready";
        /// <summary>库存操作不可用。</summary>
        public const string InventoryUnavailable = "d22.disabled.inventory_unavailable";
        /// <summary>任务前置未满足。</summary>
        public const string QuestUnavailable = "d22.disabled.quest_unavailable";
        /// <summary>未分类 owner 失败。</summary>
        public const string UnknownOwnerFailure = "d22.disabled.unknown_owner_failure";
    }

    /// <summary>结构化业务前置失败；失败码来自 owner，不从文案推断。</summary>
    [Serializable]
    public sealed class D22BusinessPreconditionFailure
    {
        /// <summary>业务 owner 稳定标识。</summary>
        public string OwnerSystem = string.Empty;
        /// <summary>owner 返回的稳定失败码。</summary>
        public string OwnerFailureCode = string.Empty;
        /// <summary>命令关联 ID。</summary>
        public string CorrelationId = string.Empty;
        /// <summary>业务目标稳定 ID。</summary>
        public string TargetId = string.Empty;
        /// <summary>失败发生的世界日。</summary>
        public int WorldDay;
        /// <summary>失败发生时的逻辑存档修订。</summary>
        public long SaveRevision;
        /// <summary>owner 是否声明修正后可重试。</summary>
        public bool Retryable;
        /// <summary>是否需要先确认再提交。</summary>
        public bool RequiresConfirmation;

        /// <summary>返回防御性副本。</summary>
        public D22BusinessPreconditionFailure Clone()
        {
            return new D22BusinessPreconditionFailure
            {
                OwnerSystem = OwnerSystem ?? string.Empty,
                OwnerFailureCode = OwnerFailureCode ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                TargetId = TargetId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Retryable = Retryable,
                RequiresConfirmation = RequiresConfirmation,
            };
        }
    }

    /// <summary>稳定禁用原因与帮助入口的不可变语义投影。</summary>
    [Serializable]
    public sealed class D22DisabledReason
    {
        /// <summary>是否确实被禁用。</summary>
        public bool IsDisabled;
        /// <summary>稳定禁用码。</summary>
        public string Code = D22DisabledReasonCodes.None;
        /// <summary>禁用说明本地化 key。</summary>
        public string LocalizationKey = string.Empty;
        /// <summary>帮助/恢复指引本地化 key。</summary>
        public string HelpLocalizationKey = string.Empty;
        /// <summary>owner 原始失败码。</summary>
        public string OriginalOwnerFailureCode = string.Empty;
        /// <summary>原始 owner。</summary>
        public string SourceOwner = string.Empty;
        /// <summary>命令关联 ID。</summary>
        public string CorrelationId = string.Empty;
        /// <summary>目标稳定 ID。</summary>
        public string TargetId = string.Empty;
        /// <summary>是否可以在外部状态修正后重试。</summary>
        public bool CanRetry;
        /// <summary>是否要求确认。</summary>
        public bool RequiresConfirmation;
        /// <summary>对应 D22 反馈码。</summary>
        public string FeedbackCode = string.Empty;
        /// <summary>失败世界日。</summary>
        public int WorldDay;
        /// <summary>失败存档修订。</summary>
        public long SaveRevision;

        /// <summary>返回防御性副本。</summary>
        public D22DisabledReason Clone()
        {
            return new D22DisabledReason
            {
                IsDisabled = IsDisabled,
                Code = Code ?? D22DisabledReasonCodes.None,
                LocalizationKey = LocalizationKey ?? string.Empty,
                HelpLocalizationKey = HelpLocalizationKey ?? string.Empty,
                OriginalOwnerFailureCode = OriginalOwnerFailureCode ?? string.Empty,
                SourceOwner = SourceOwner ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                TargetId = TargetId ?? string.Empty,
                CanRetry = CanRetry,
                RequiresConfirmation = RequiresConfirmation,
                FeedbackCode = FeedbackCode ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
            };
        }
    }

    /// <summary>
    /// D22-L08 将结构化 owner 前置失败映射为稳定禁用/帮助读模型。
    /// </summary>
    public static class D22DisabledReasonResolver
    {
        /// <summary>Resolver 版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>Resolver owner。</summary>
        public const string OwnerSystem = "D22DisabledReasonResolver";

        /// <summary>没有失败输入时返回未禁用读模型。</summary>
        public static D22DisabledReason Resolve(D22BusinessPreconditionFailure failure)
        {
            if (failure == null || string.IsNullOrEmpty(failure.OwnerFailureCode))
                return NotDisabled();

            string code = Normalize(failure.OwnerFailureCode);
            string disabledCode = D22DisabledReasonCodes.UnknownOwnerFailure;
            string locKey = "ui.d22.disabled.unknown_owner_failure";
            string helpKey = "ui.d22.disabled.help.unknown_owner_failure";
            string feedbackCode = D22FeedbackCodes.OwnerRejected;
            bool defaultRetryable = false;

            if (Matches(code, "permission_denied", "forbidden", "unauthorized", "missing_permission"))
            {
                disabledCode = D22DisabledReasonCodes.PermissionDenied;
                locKey = "ui.d22.disabled.permission_denied";
                helpKey = "ui.d22.disabled.help.permission_denied";
                feedbackCode = D22FeedbackCodes.PermissionDenied;
            }
            else if (Matches(code, "invalid_context", "context_mismatch", "not_in_context", "invalid_state"))
            {
                disabledCode = D22DisabledReasonCodes.InvalidContext;
                locKey = "ui.d22.disabled.invalid_context";
                helpKey = "ui.d22.disabled.help.invalid_context";
                feedbackCode = D22FeedbackCodes.ContextMismatch;
            }
            else if (Matches(code, "missing_target", "target_required", "unknown_target", "invalid_reference"))
            {
                disabledCode = D22DisabledReasonCodes.MissingTarget;
                locKey = "ui.d22.disabled.missing_target";
                helpKey = "ui.d22.disabled.help.missing_target";
                feedbackCode = D22FeedbackCodes.InvalidRequest;
            }
            else if (Matches(code, "cooldown", "cooldown_active", "rate_limited", "too_soon"))
            {
                disabledCode = D22DisabledReasonCodes.CooldownActive;
                locKey = "ui.d22.disabled.cooldown_active";
                helpKey = "ui.d22.disabled.help.cooldown_active";
                feedbackCode = D22FeedbackCodes.CooldownActive;
                defaultRetryable = true;
            }
            else if (Matches(code, "stale_revision", "revision_mismatch", "stale_context", "conflict"))
            {
                disabledCode = D22DisabledReasonCodes.StaleRevision;
                locKey = "ui.d22.disabled.stale_revision";
                helpKey = "ui.d22.disabled.help.stale_revision";
                feedbackCode = D22FeedbackCodes.RevisionMismatch;
            }
            else if (Matches(code, "owner_unavailable", "service_unavailable", "dependency_missing", "not_ready"))
            {
                disabledCode = D22DisabledReasonCodes.OwnerUnavailable;
                locKey = "ui.d22.disabled.owner_unavailable";
                helpKey = "ui.d22.disabled.help.owner_unavailable";
                feedbackCode = D22FeedbackCodes.OwnerUnavailable;
                defaultRetryable = true;
            }
            else if (Matches(code, "pending", "owner_pending", "in_progress"))
            {
                disabledCode = D22DisabledReasonCodes.OwnerPending;
                locKey = "ui.d22.disabled.owner_pending";
                helpKey = "ui.d22.disabled.help.owner_pending";
                feedbackCode = D22FeedbackCodes.OwnerPending;
                defaultRetryable = true;
            }
            else if (Matches(code, "insufficient_funds", "funds_insufficient", "not_enough_money", "credit_denied"))
            {
                disabledCode = D22DisabledReasonCodes.InsufficientFunds;
                locKey = "ui.d22.disabled.insufficient_funds";
                helpKey = "ui.d22.disabled.help.insufficient_funds";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "capacity_exceeded", "over_capacity", "cargo_full", "weight_exceeded"))
            {
                disabledCode = D22DisabledReasonCodes.CapacityExceeded;
                locKey = "ui.d22.disabled.capacity_exceeded";
                helpKey = "ui.d22.disabled.help.capacity_exceeded";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "unknown_port", "port_unavailable", "service_closed", "not_in_port"))
            {
                disabledCode = D22DisabledReasonCodes.PortUnavailable;
                locKey = "ui.d22.disabled.port_unavailable";
                helpKey = "ui.d22.disabled.help.port_unavailable";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "ship_not_ready", "not_ready_to_depart", "ship_damaged", "voyage_blocked"))
            {
                disabledCode = D22DisabledReasonCodes.ShipNotReady;
                locKey = "ui.d22.disabled.ship_not_ready";
                helpKey = "ui.d22.disabled.help.ship_not_ready";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "inventory_unavailable", "inventory_operation_failed", "item_missing", "not_enough_stock"))
            {
                disabledCode = D22DisabledReasonCodes.InventoryUnavailable;
                locKey = "ui.d22.disabled.inventory_unavailable";
                helpKey = "ui.d22.disabled.help.inventory_unavailable";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "quest_unavailable", "quest_locked", "prerequisite_missing"))
            {
                disabledCode = D22DisabledReasonCodes.QuestUnavailable;
                locKey = "ui.d22.disabled.quest_unavailable";
                helpKey = "ui.d22.disabled.help.quest_unavailable";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }
            else if (Matches(code, "rejected", "owner_rejected", "invalid_quantity", "already_completed"))
            {
                disabledCode = D22DisabledReasonCodes.OwnerRejected;
                locKey = "ui.d22.disabled.owner_rejected";
                helpKey = "ui.d22.disabled.help.owner_rejected";
                feedbackCode = D22FeedbackCodes.OwnerRejected;
            }

            return new D22DisabledReason
            {
                IsDisabled = true,
                Code = disabledCode,
                LocalizationKey = locKey,
                HelpLocalizationKey = helpKey,
                OriginalOwnerFailureCode = failure.OwnerFailureCode ?? string.Empty,
                SourceOwner = failure.OwnerSystem ?? string.Empty,
                CorrelationId = failure.CorrelationId ?? string.Empty,
                TargetId = failure.TargetId ?? string.Empty,
                CanRetry = failure.Retryable || defaultRetryable,
                RequiresConfirmation = failure.RequiresConfirmation,
                FeedbackCode = feedbackCode,
                WorldDay = failure.WorldDay,
                SaveRevision = failure.SaveRevision,
            };
        }

        /// <summary>无失败输入的简短重载。</summary>
        public static D22DisabledReason Resolve(
            string ownerSystem,
            string ownerFailureCode,
            string correlationId = "",
            string targetId = "",
            int worldDay = 0,
            long saveRevision = 0,
            bool retryable = false,
            bool requiresConfirmation = false)
        {
            return Resolve(new D22BusinessPreconditionFailure
            {
                OwnerSystem = ownerSystem ?? string.Empty,
                OwnerFailureCode = ownerFailureCode ?? string.Empty,
                CorrelationId = correlationId ?? string.Empty,
                TargetId = targetId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = saveRevision,
                Retryable = retryable,
                RequiresConfirmation = requiresConfirmation,
            });
        }

        /// <summary>有结构化失败时返回 true；没有失败时保持未禁用。</summary>
        public static bool TryResolve(
            D22BusinessPreconditionFailure failure,
            out D22DisabledReason reason)
        {
            reason = Resolve(failure);
            return reason.IsDisabled;
        }

        private static D22DisabledReason NotDisabled()
        {
            return new D22DisabledReason
            {
                IsDisabled = false,
                Code = D22DisabledReasonCodes.None,
                LocalizationKey = string.Empty,
                HelpLocalizationKey = string.Empty,
                FeedbackCode = string.Empty,
            };
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool Matches(string value, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                string normalizedCandidate = Normalize(candidate);
                if (string.Equals(value, normalizedCandidate, StringComparison.Ordinal) ||
                    value.EndsWith("." + normalizedCandidate, StringComparison.Ordinal) ||
                    value.EndsWith("/" + normalizedCandidate, StringComparison.Ordinal) ||
                    string.Equals(Compact(value), Compact(normalizedCandidate), StringComparison.Ordinal) ||
                    Compact(value).EndsWith(Compact(normalizedCandidate), StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static string Compact(string value)
        {
            return (value ?? string.Empty).Replace("_", string.Empty)
                .Replace("-", string.Empty).Replace(".", string.Empty).Replace("/", string.Empty);
        }
    }
}
