// ============================================================
// D22UiCommandValidator —— D22-L06 UI 命令前置校验（纯 C#）
//
// 校验器只读取请求、焦点/面板上下文和外部权限快照；它不登记命令、不写
// 冷却、不修改业务状态。重复命令必须由调用方把只读幂等索引传入。
// ============================================================

using System;

namespace Whalebone.Systems.D22
{
    /// <summary>D22 UI 命令校验结果。</summary>
    [Serializable]
    public sealed class D22UiCommandValidationResult
    {
        /// <summary>请求是否可以交给 owner。</summary>
        public bool Valid;
        /// <summary>是否命中同内容重复命令。</summary>
        public bool Duplicate;
        /// <summary>是否命中同 ID 不同内容的冲突重放。</summary>
        public bool DuplicateConflict;
        /// <summary>稳定校验码。</summary>
        public D22UiCommandValidationCode Code;
        /// <summary>机器可读原因 key。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>禁用原因稳定码。</summary>
        public string DisabledReasonCode = string.Empty;
        /// <summary>已有回执的关联 ID。</summary>
        public string ExistingCorrelationId = string.Empty;
        /// <summary>当前请求指纹。</summary>
        public string RequestFingerprint = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22UiCommandValidationResult Clone()
        {
            return new D22UiCommandValidationResult
            {
                Valid = Valid,
                Duplicate = Duplicate,
                DuplicateConflict = DuplicateConflict,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
                DisabledReasonCode = DisabledReasonCode ?? string.Empty,
                ExistingCorrelationId = ExistingCorrelationId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
            };
        }

        /// <summary>转换为不含业务事实的 D22 命令结果。</summary>
        public D22UiCommandResult ToCommandResult(D22UiCommandRequest request)
        {
            var result = new D22UiCommandResult
            {
                CommandId = request == null ? string.Empty : request.CommandId ?? string.Empty,
                CorrelationId = request == null ? string.Empty : request.CorrelationId ?? string.Empty,
                Outcome = Valid
                    ? D22UiCommandOutcome.Accepted
                    : (Duplicate ? D22UiCommandOutcome.Duplicate : D22UiCommandOutcome.Rejected),
                ValidationCode = Code,
                Accepted = Valid,
                Duplicate = Duplicate,
                DisabledReasonCode = DisabledReasonCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
            return result;
        }
    }

    /// <summary>
    /// D22-L06 权限、上下文、冷却、禁用和重复命令的纯校验器。
    /// </summary>
    public static class D22UiCommandValidator
    {
        /// <summary>校验器版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>校验器 owner。</summary>
        public const string OwnerSystem = "D22UiCommandValidator";

        /// <summary>按确定性顺序执行完整请求校验。</summary>
        public static D22UiCommandValidationResult Validate(
            D22UiCommandRequest request,
            D22UiCommandContext context)
        {
            if (request == null)
                return Reject(D22UiCommandValidationCode.MissingRequest, "d22.command.missing_request");

            D22UiCommandValidationCode shapeCode = D22UiCommandContracts.ValidateShape(request);
            if (shapeCode != D22UiCommandValidationCode.None)
                return Reject(shapeCode, ReasonFor(shapeCode));
            if (context == null)
                return Reject(D22UiCommandValidationCode.MissingContext, "d22.command.missing_context");
            if (context.SchemaVersion != D22UiCommandContracts.CurrentSchemaVersion)
                return Reject(D22UiCommandValidationCode.InvalidSchemaVersion, "d22.command.invalid_schema_version");
            if (!context.SessionActive)
                return Reject(D22UiCommandValidationCode.SessionInactive, "d22.command.session_inactive");
            if (!context.ContextValid)
                return Reject(D22UiCommandValidationCode.ContextMismatch, "d22.command.context_mismatch");
            if (!string.IsNullOrEmpty(context.ActiveScreenId) &&
                !D22ScreenContracts.IsKnownScreenId(context.ActiveScreenId))
                return Reject(D22UiCommandValidationCode.UnknownScreenId, "d22.command.unknown_context_screen");
            if (!string.IsNullOrEmpty(context.ActivePanelId) &&
                !D22ScreenContracts.IsKnownPanelId(context.ActivePanelId))
                return Reject(D22UiCommandValidationCode.UnknownPanelId, "d22.command.unknown_context_panel");
            if (!string.IsNullOrEmpty(context.FocusedControlId) &&
                !string.Equals(context.FocusedControlId, D22FocusControlIds.None, StringComparison.Ordinal) &&
                !D22ScreenContracts.IsStableId(context.FocusedControlId))
                return Reject(D22UiCommandValidationCode.FocusMismatch, "d22.command.invalid_context_focus");

            if (!string.IsNullOrEmpty(context.ActorId) &&
                !string.Equals(context.ActorId, request.ActorId, StringComparison.Ordinal))
                return Reject(D22UiCommandValidationCode.PermissionDenied, "d22.command.permission_denied");
            if (!context.PermissionGranted)
                return Reject(D22UiCommandValidationCode.PermissionDenied, "d22.command.permission_denied");
            if (!IsAllowedKind(request.Kind, context))
                return Reject(D22UiCommandValidationCode.PermissionDenied, "d22.command.permission_denied");
            if (!context.CommandEnabled || !string.IsNullOrEmpty(context.DisabledReasonCode))
            {
                return Reject(
                    D22UiCommandValidationCode.Disabled,
                    "d22.command.disabled",
                    context.DisabledReasonCode);
            }

            if (string.IsNullOrEmpty(context.ActiveScreenId) ||
                !string.Equals(context.ActiveScreenId, request.ScreenId, StringComparison.Ordinal))
                return Reject(D22UiCommandValidationCode.ContextMismatch, "d22.command.context_mismatch");

            D22UiCommandDefinition definition;
            D22UiCommandContracts.TryGetDefinition(request.Kind, out definition);
            if (definition != null && definition.RequiresCommandPanel)
            {
                string expectedPanelId = string.IsNullOrEmpty(context.ActiveModalId)
                    ? context.ActivePanelId
                    : context.ActiveModalId;
                if (string.IsNullOrEmpty(expectedPanelId) ||
                    !string.Equals(expectedPanelId, request.PanelId, StringComparison.Ordinal))
                    return Reject(D22UiCommandValidationCode.ContextMismatch, "d22.command.context_mismatch");
            }
            else if (!string.IsNullOrEmpty(request.PanelId))
            {
                string expectedPanelId = string.IsNullOrEmpty(context.ActiveModalId)
                    ? context.ActivePanelId
                    : context.ActiveModalId;
                if (!string.IsNullOrEmpty(expectedPanelId) &&
                    !string.Equals(expectedPanelId, request.PanelId, StringComparison.Ordinal))
                    return Reject(D22UiCommandValidationCode.ContextMismatch, "d22.command.context_mismatch");
            }

            if (!string.IsNullOrEmpty(context.ActiveModalId))
            {
                if (!D22ScreenContracts.IsKnownModalId(context.ActiveModalId) ||
                    !string.Equals(context.ActiveModalId, request.PanelId, StringComparison.Ordinal))
                    return Reject(D22UiCommandValidationCode.FocusMismatch, "d22.command.modal_focus_mismatch");
            }
            else
            {
                D22PanelDefinition panel;
                if (D22ScreenContracts.TryGetPanelDefinition(request.PanelId, out panel) &&
                    panel.SurfaceKind == D22SurfaceKind.Modal)
                    return Reject(D22UiCommandValidationCode.FocusMismatch, "d22.command.modal_focus_mismatch");
            }

            if (!string.IsNullOrEmpty(request.FocusedControlId) &&
                !string.Equals(request.FocusedControlId, D22FocusControlIds.None, StringComparison.Ordinal) &&
                !string.Equals(request.FocusedControlId, context.FocusedControlId, StringComparison.Ordinal))
                return Reject(D22UiCommandValidationCode.FocusMismatch, "d22.command.focus_mismatch");

            if (request.WorldDay != context.WorldDay || request.SaveRevision != context.SaveRevision)
                return Reject(D22UiCommandValidationCode.RevisionMismatch, "d22.command.revision_mismatch");
            if (context.CooldownUntilWorldDay > context.WorldDay)
                return Reject(D22UiCommandValidationCode.CooldownActive, "d22.command.cooldown_active");

            string fingerprint = request.Fingerprint();
            if (context.ProcessedCommands != null)
            {
                foreach (var processed in context.ProcessedCommands)
                {
                    if (processed == null ||
                        !string.Equals(processed.CommandId, request.CommandId, StringComparison.Ordinal))
                        continue;
                    if (string.Equals(processed.Fingerprint, fingerprint, StringComparison.Ordinal))
                    {
                        return new D22UiCommandValidationResult
                        {
                            Valid = false,
                            Duplicate = true,
                            Code = D22UiCommandValidationCode.DuplicateCommand,
                            ReasonCode = "d22.command.duplicate_command",
                            ExistingCorrelationId = processed.CorrelationId ?? string.Empty,
                            RequestFingerprint = fingerprint,
                        };
                    }
                    return new D22UiCommandValidationResult
                    {
                        Valid = false,
                        Duplicate = false,
                        DuplicateConflict = true,
                        Code = D22UiCommandValidationCode.DuplicateCommandConflict,
                        ReasonCode = "d22.command.duplicate_command_conflict",
                        ExistingCorrelationId = processed.CorrelationId ?? string.Empty,
                        RequestFingerprint = fingerprint,
                    };
                }
            }

            return new D22UiCommandValidationResult
            {
                Valid = true,
                Duplicate = false,
                DuplicateConflict = false,
                Code = D22UiCommandValidationCode.None,
                ReasonCode = "d22.command.valid",
                RequestFingerprint = fingerprint,
            };
        }

        /// <summary>只返回是否允许；不会登记命令或改变任何状态。</summary>
        public static bool IsAllowed(D22UiCommandRequest request, D22UiCommandContext context)
        {
            return Validate(request, context).Valid;
        }

        private static bool IsAllowedKind(D22UiCommandKind kind, D22UiCommandContext context)
        {
            if (context.AllowedCommandKinds == null || context.AllowedCommandKinds.Count == 0)
                return true;
            foreach (var allowed in context.AllowedCommandKinds)
                if (allowed == kind) return true;
            return false;
        }

        private static D22UiCommandValidationResult Reject(
            D22UiCommandValidationCode code,
            string reasonCode,
            string disabledReasonCode = "")
        {
            return new D22UiCommandValidationResult
            {
                Valid = false,
                Duplicate = false,
                DuplicateConflict = false,
                Code = code,
                ReasonCode = reasonCode ?? string.Empty,
                DisabledReasonCode = disabledReasonCode ?? string.Empty,
            };
        }

        private static string ReasonFor(D22UiCommandValidationCode code)
        {
            switch (code)
            {
                case D22UiCommandValidationCode.MissingRequest: return "d22.command.missing_request";
                case D22UiCommandValidationCode.InvalidSchemaVersion: return "d22.command.invalid_schema_version";
                case D22UiCommandValidationCode.MissingCommandId: return "d22.command.missing_command_id";
                case D22UiCommandValidationCode.MissingCorrelationId: return "d22.command.missing_correlation_id";
                case D22UiCommandValidationCode.MissingActorId: return "d22.command.missing_actor_id";
                case D22UiCommandValidationCode.UnknownCommandKind: return "d22.command.unknown_kind";
                case D22UiCommandValidationCode.ActionIdMismatch: return "d22.command.action_id_mismatch";
                case D22UiCommandValidationCode.InvalidIdentifier: return "d22.command.invalid_identifier";
                case D22UiCommandValidationCode.MissingScreenId: return "d22.command.missing_screen_id";
                case D22UiCommandValidationCode.UnknownScreenId: return "d22.command.unknown_screen_id";
                case D22UiCommandValidationCode.MissingPanelId: return "d22.command.missing_panel_id";
                case D22UiCommandValidationCode.UnknownPanelId: return "d22.command.unknown_panel_id";
                case D22UiCommandValidationCode.MissingTargetId: return "d22.command.missing_target_id";
                case D22UiCommandValidationCode.InvalidParameter: return "d22.command.invalid_parameter";
                case D22UiCommandValidationCode.RevisionMismatch: return "d22.command.revision_mismatch";
                case D22UiCommandValidationCode.OwnerSystemMismatch: return "d22.command.owner_mismatch";
                default: return "d22.command.invalid_request";
            }
        }
    }
}
