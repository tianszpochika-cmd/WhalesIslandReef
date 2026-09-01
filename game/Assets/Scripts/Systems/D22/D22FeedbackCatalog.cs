// ============================================================
// D22FeedbackCatalog —— D22-L07 反馈码、本地化 key 与重试策略（纯 C#）
//
// 反馈目录只保存机器码和本地化 key，不保存用户可见文案。owner 的原始结果
// 通过 source owner 与 reason code 关联；Runtime 负责在表现边缘解析文案。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>反馈严重程度。</summary>
    public enum D22FeedbackSeverity
    {
        /// <summary>普通信息。</summary>
        Info = 0,
        /// <summary>成功反馈。</summary>
        Success = 1,
        /// <summary>可恢复警告。</summary>
        Warning = 2,
        /// <summary>失败反馈。</summary>
        Error = 3,
        /// <summary>需要用户确认。</summary>
        Confirmation = 4,
    }

    /// <summary>反馈语义分类。</summary>
    public enum D22FeedbackCategory
    {
        /// <summary>成功。</summary>
        Success = 0,
        /// <summary>失败。</summary>
        Failure = 1,
        /// <summary>警告。</summary>
        Warning = 2,
        /// <summary>确认。</summary>
        Confirmation = 3,
        /// <summary>重试建议。</summary>
        Retry = 4,
    }

    /// <summary>反馈的重试策略。</summary>
    public enum D22FeedbackRetryPolicy
    {
        /// <summary>不应重试。</summary>
        NoRetry = 0,
        /// <summary>相同命令 ID 可安全重试。</summary>
        SafeRetry = 1,
        /// <summary>修正参数/前置后才可重试。</summary>
        RetryAfterCorrection = 2,
        /// <summary>等待 owner 结果后再决定。</summary>
        WaitForOwner = 3,
        /// <summary>确认后再重试。</summary>
        ConfirmThenRetry = 4,
    }

    /// <summary>反馈码的稳定目录键。</summary>
    public static class D22FeedbackCodes
    {
        /// <summary>命令已接受。</summary>
        public const string CommandAccepted = "d22.success.command_accepted";
        /// <summary>owner 已完成。</summary>
        public const string CommandCompleted = "d22.success.command_completed";
        /// <summary>面板已打开。</summary>
        public const string PanelOpened = "d22.success.panel_opened";
        /// <summary>面板已关闭。</summary>
        public const string PanelClosed = "d22.success.panel_closed";
        /// <summary>请求结构非法。</summary>
        public const string InvalidRequest = "d22.failure.invalid_request";
        /// <summary>权限不足。</summary>
        public const string PermissionDenied = "d22.failure.permission_denied";
        /// <summary>上下文不匹配。</summary>
        public const string ContextMismatch = "d22.failure.context_mismatch";
        /// <summary>焦点不匹配。</summary>
        public const string FocusMismatch = "d22.failure.focus_mismatch";
        /// <summary>冷却中。</summary>
        public const string CooldownActive = "d22.failure.cooldown_active";
        /// <summary>重复命令。</summary>
        public const string DuplicateCommand = "d22.warning.duplicate_command";
        /// <summary>重复命令内容冲突。</summary>
        public const string DuplicateConflict = "d22.failure.duplicate_conflict";
        /// <summary>owner 拒绝。</summary>
        public const string OwnerRejected = "d22.failure.owner_rejected";
        /// <summary>owner/依赖不可用。</summary>
        public const string OwnerUnavailable = "d22.failure.owner_unavailable";
        /// <summary>修订不一致。</summary>
        public const string RevisionMismatch = "d22.failure.revision_mismatch";
        /// <summary>缺少上下文。</summary>
        public const string MissingContext = "d22.failure.missing_context";
        /// <summary>命令被禁用。</summary>
        public const string Disabled = "d22.failure.disabled";
        /// <summary>owner 尚未完成。</summary>
        public const string OwnerPending = "d22.warning.owner_pending";
        /// <summary>焦点已经回退。</summary>
        public const string FocusRecovered = "d22.warning.focus_recovered";
        /// <summary>上下文已过期。</summary>
        public const string StaleContext = "d22.warning.stale_context";
        /// <summary>破坏性命令需确认。</summary>
        public const string DestructiveConfirmation = "d22.confirmation.destructive_action";
        /// <summary>未保存变更需确认。</summary>
        public const string UnsavedChangesConfirmation = "d22.confirmation.unsaved_changes";
        /// <summary>可安全重试。</summary>
        public const string SafeRetry = "d22.retry.safe";
        /// <summary>修正后重试。</summary>
        public const string RetryAfterCorrection = "d22.retry.after_correction";
        /// <summary>当前不可重试。</summary>
        public const string RetryUnavailable = "d22.retry.unavailable";
        /// <summary>未知反馈码的审计回退。</summary>
        public const string Unknown = "d22.failure.unknown";
    }

    /// <summary>反馈目录条目；只包含稳定标识，不包含显示文案。</summary>
    [Serializable]
    public sealed class D22FeedbackEntry
    {
        /// <summary>稳定反馈码。</summary>
        public string Code = string.Empty;
        /// <summary>本地化 key。</summary>
        public string LocalizationKey = string.Empty;
        /// <summary>反馈分类。</summary>
        public D22FeedbackCategory Category;
        /// <summary>严重程度。</summary>
        public D22FeedbackSeverity Severity;
        /// <summary>重试策略。</summary>
        public D22FeedbackRetryPolicy RetryPolicy;
        /// <summary>是否需要确认。</summary>
        public bool RequiresConfirmation;
        /// <summary>原始结果的 owner 提示。</summary>
        public string SourceOwner = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22FeedbackEntry Clone()
        {
            return new D22FeedbackEntry
            {
                Code = Code ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                Category = Category,
                Severity = Severity,
                RetryPolicy = RetryPolicy,
                RequiresConfirmation = RequiresConfirmation,
                SourceOwner = SourceOwner ?? string.Empty,
            };
        }
    }

    /// <summary>带关联 ID 的反馈读模型。</summary>
    [Serializable]
    public sealed class D22FeedbackRecord
    {
        /// <summary>反馈码。</summary>
        public string Code = string.Empty;
        /// <summary>本地化 key；永远不是最终显示文案。</summary>
        public string LocalizationKey = string.Empty;
        /// <summary>关联 ID。</summary>
        public string CorrelationId = string.Empty;
        /// <summary>来源 owner。</summary>
        public string SourceOwner = string.Empty;
        /// <summary>原始 owner reason code。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>分类。</summary>
        public D22FeedbackCategory Category;
        /// <summary>严重程度。</summary>
        public D22FeedbackSeverity Severity;
        /// <summary>重试策略。</summary>
        public D22FeedbackRetryPolicy RetryPolicy;
        /// <summary>是否要求确认。</summary>
        public bool RequiresConfirmation;
        /// <summary>目录是否找到该码。</summary>
        public bool CatalogHit;

        /// <summary>返回防御性副本。</summary>
        public D22FeedbackRecord Clone()
        {
            return new D22FeedbackRecord
            {
                Code = Code ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                SourceOwner = SourceOwner ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                Category = Category,
                Severity = Severity,
                RetryPolicy = RetryPolicy,
                RequiresConfirmation = RequiresConfirmation,
                CatalogHit = CatalogHit,
            };
        }
    }

    /// <summary>
    /// D22-L07 成功/失败/警告/确认/重试反馈目录。
    /// </summary>
    public static class D22FeedbackCatalog
    {
        /// <summary>反馈目录版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>反馈目录 owner。</summary>
        public const string OwnerSystem = "D22FeedbackCatalog";

        private static readonly ReadOnlyCollection<D22FeedbackEntry> Entries =
            new ReadOnlyCollection<D22FeedbackEntry>(new List<D22FeedbackEntry>
            {
                Entry(D22FeedbackCodes.CommandAccepted, "ui.d22.feedback.success.command_accepted", D22FeedbackCategory.Success, D22FeedbackSeverity.Success, D22FeedbackRetryPolicy.WaitForOwner, false, OwnerSystem),
                Entry(D22FeedbackCodes.CommandCompleted, "ui.d22.feedback.success.command_completed", D22FeedbackCategory.Success, D22FeedbackSeverity.Success, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.PanelOpened, "ui.d22.feedback.success.panel_opened", D22FeedbackCategory.Success, D22FeedbackSeverity.Success, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.PanelClosed, "ui.d22.feedback.success.panel_closed", D22FeedbackCategory.Success, D22FeedbackSeverity.Success, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.InvalidRequest, "ui.d22.feedback.failure.invalid_request", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.PermissionDenied, "ui.d22.feedback.failure.permission_denied", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.ContextMismatch, "ui.d22.feedback.failure.context_mismatch", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.FocusMismatch, "ui.d22.feedback.failure.focus_mismatch", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.CooldownActive, "ui.d22.feedback.failure.cooldown_active", D22FeedbackCategory.Failure, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.WaitForOwner, false, OwnerSystem),
                Entry(D22FeedbackCodes.DuplicateCommand, "ui.d22.feedback.warning.duplicate_command", D22FeedbackCategory.Warning, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.DuplicateConflict, "ui.d22.feedback.failure.duplicate_conflict", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.OwnerRejected, "ui.d22.feedback.failure.owner_rejected", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, "domain.owner"),
                Entry(D22FeedbackCodes.OwnerUnavailable, "ui.d22.feedback.failure.owner_unavailable", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.SafeRetry, false, "domain.owner"),
                Entry(D22FeedbackCodes.RevisionMismatch, "ui.d22.feedback.failure.revision_mismatch", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.MissingContext, "ui.d22.feedback.failure.missing_context", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.Disabled, "ui.d22.feedback.failure.disabled", D22FeedbackCategory.Failure, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.OwnerPending, "ui.d22.feedback.warning.owner_pending", D22FeedbackCategory.Warning, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.WaitForOwner, false, "domain.owner"),
                Entry(D22FeedbackCodes.FocusRecovered, "ui.d22.feedback.warning.focus_recovered", D22FeedbackCategory.Warning, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.StaleContext, "ui.d22.feedback.warning.stale_context", D22FeedbackCategory.Warning, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.DestructiveConfirmation, "ui.d22.feedback.confirmation.destructive_action", D22FeedbackCategory.Confirmation, D22FeedbackSeverity.Confirmation, D22FeedbackRetryPolicy.ConfirmThenRetry, true, OwnerSystem),
                Entry(D22FeedbackCodes.UnsavedChangesConfirmation, "ui.d22.feedback.confirmation.unsaved_changes", D22FeedbackCategory.Confirmation, D22FeedbackSeverity.Confirmation, D22FeedbackRetryPolicy.ConfirmThenRetry, true, OwnerSystem),
                Entry(D22FeedbackCodes.SafeRetry, "ui.d22.feedback.retry.safe", D22FeedbackCategory.Retry, D22FeedbackSeverity.Info, D22FeedbackRetryPolicy.SafeRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.RetryAfterCorrection, "ui.d22.feedback.retry.after_correction", D22FeedbackCategory.Retry, D22FeedbackSeverity.Info, D22FeedbackRetryPolicy.RetryAfterCorrection, false, OwnerSystem),
                Entry(D22FeedbackCodes.RetryUnavailable, "ui.d22.feedback.retry.unavailable", D22FeedbackCategory.Retry, D22FeedbackSeverity.Warning, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
                Entry(D22FeedbackCodes.Unknown, "ui.d22.feedback.failure.unknown", D22FeedbackCategory.Failure, D22FeedbackSeverity.Error, D22FeedbackRetryPolicy.NoRetry, false, OwnerSystem),
            });

        /// <summary>返回反馈条目防御性副本。</summary>
        public static IReadOnlyList<D22FeedbackEntry> GetEntries()
        {
            var result = new List<D22FeedbackEntry>();
            foreach (var entry in Entries) result.Add(entry.Clone());
            return new ReadOnlyCollection<D22FeedbackEntry>(result);
        }

        /// <summary>查找稳定反馈条目。</summary>
        public static bool TryGet(string code, out D22FeedbackEntry entry)
        {
            foreach (var candidate in Entries)
            {
                if (string.Equals(candidate.Code, code, StringComparison.Ordinal))
                {
                    entry = candidate.Clone();
                    return true;
                }
            }
            entry = null;
            return false;
        }

        /// <summary>创建关联到某次命令的反馈读模型。</summary>
        public static D22FeedbackRecord Create(
            string code,
            string correlationId = "",
            string sourceOwner = "",
            string reasonCode = "")
        {
            D22FeedbackEntry entry;
            bool hit = TryGet(code, out entry);
            if (!hit) TryGet(D22FeedbackCodes.Unknown, out entry);
            return new D22FeedbackRecord
            {
                Code = code ?? string.Empty,
                LocalizationKey = entry == null ? string.Empty : entry.LocalizationKey,
                CorrelationId = correlationId ?? string.Empty,
                SourceOwner = string.IsNullOrEmpty(sourceOwner)
                    ? (entry == null ? string.Empty : entry.SourceOwner)
                    : sourceOwner,
                ReasonCode = reasonCode ?? string.Empty,
                Category = entry == null ? D22FeedbackCategory.Failure : entry.Category,
                Severity = entry == null ? D22FeedbackSeverity.Error : entry.Severity,
                RetryPolicy = entry == null ? D22FeedbackRetryPolicy.NoRetry : entry.RetryPolicy,
                RequiresConfirmation = entry != null && entry.RequiresConfirmation,
                CatalogHit = hit,
            };
        }

        /// <summary>把 D22 命令校验码映射到反馈码。</summary>
        public static string ForValidation(D22UiCommandValidationCode code)
        {
            switch (code)
            {
                case D22UiCommandValidationCode.None: return D22FeedbackCodes.CommandAccepted;
                case D22UiCommandValidationCode.PermissionDenied: return D22FeedbackCodes.PermissionDenied;
                case D22UiCommandValidationCode.ContextMismatch: return D22FeedbackCodes.ContextMismatch;
                case D22UiCommandValidationCode.FocusMismatch: return D22FeedbackCodes.FocusMismatch;
                case D22UiCommandValidationCode.CooldownActive: return D22FeedbackCodes.CooldownActive;
                case D22UiCommandValidationCode.DuplicateCommand: return D22FeedbackCodes.DuplicateCommand;
                case D22UiCommandValidationCode.DuplicateCommandConflict: return D22FeedbackCodes.DuplicateConflict;
                case D22UiCommandValidationCode.Disabled: return D22FeedbackCodes.Disabled;
                case D22UiCommandValidationCode.RevisionMismatch: return D22FeedbackCodes.RevisionMismatch;
                case D22UiCommandValidationCode.MissingContext: return D22FeedbackCodes.MissingContext;
                default: return D22FeedbackCodes.InvalidRequest;
            }
        }

        /// <summary>把焦点失败码映射到反馈码。</summary>
        public static string ForFocusFailure(D22FocusFailureCode code)
        {
            switch (code)
            {
                case D22FocusFailureCode.None: return D22FeedbackCodes.CommandAccepted;
                case D22FocusFailureCode.NoFallbackFocus: return D22FeedbackCodes.FocusMismatch;
                case D22FocusFailureCode.ModalStackEmpty: return D22FeedbackCodes.InvalidRequest;
                case D22FocusFailureCode.BlockingModalAlreadyOwned: return D22FeedbackCodes.PermissionDenied;
                default: return D22FeedbackCodes.ContextMismatch;
            }
        }

        private static D22FeedbackEntry Entry(
            string code,
            string localizationKey,
            D22FeedbackCategory category,
            D22FeedbackSeverity severity,
            D22FeedbackRetryPolicy retryPolicy,
            bool requiresConfirmation,
            string sourceOwner)
        {
            return new D22FeedbackEntry
            {
                Code = code,
                LocalizationKey = localizationKey,
                Category = category,
                Severity = severity,
                RetryPolicy = retryPolicy,
                RequiresConfirmation = requiresConfirmation,
                SourceOwner = sourceOwner,
            };
        }
    }
}
