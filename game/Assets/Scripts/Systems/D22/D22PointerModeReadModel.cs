// ============================================================
// D22PointerModeReadModel —— D22-L12 pointer/mouse 需求读模型（纯 C#）
//
// 本文件只把逻辑上下文投影成 Runtime 可消费的 pointer 意图。它不读取
// 或控制设备，不调用 InputSystem，也不写可见性、锁定或 UGUI 状态。
// ============================================================

using System;

namespace Whalebone.Systems.D22
{
    /// <summary>逻辑 pointer 模式；不是设备 API 的直接调用。</summary>
    public enum D22PointerMode
    {
        Default = 0,
        Free = 1,
        Locked = 2,
    }

    /// <summary>Runtime 对 pointer 可见性的逻辑需求。</summary>
    public enum D22PointerVisibilityIntent
    {
        Unspecified = 0,
        Visible = 1,
        Hidden = 2,
    }

    /// <summary>pointer 读模型的稳定失败/降级原因。</summary>
    public enum D22PointerModeFailureCode
    {
        None = 0,
        MissingContext = 1,
        InvalidSchemaVersion = 2,
        InactiveSession = 3,
        InvalidContext = 4,
        LockUnavailable = 5,
    }

    /// <summary>由 Runtime/逻辑 composer 提供的 pointer 上下文快照。</summary>
    [Serializable]
    public sealed class D22PointerModeContext
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool SessionActive = true;
        public bool ContextValid = true;
        public string ActiveScreenId = string.Empty;
        public string ActivePanelId = string.Empty;
        public string ActiveModalId = string.Empty;

        public D22InputModeIntent InputMode = D22InputModeIntent.Unknown;
        public bool RequiresPointer;
        public bool RequiresPointerLock;
        public bool PointerCaptureAllowed = true;
        public bool TextEntryActive;
        public bool IsPaused;
        public string ReasonCode = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22PointerModeContext Clone()
        {
            return new D22PointerModeContext
            {
                SchemaVersion = SchemaVersion,
                SessionActive = SessionActive,
                ContextValid = ContextValid,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                ActiveModalId = ActiveModalId ?? string.Empty,
                InputMode = InputMode,
                RequiresPointer = RequiresPointer,
                RequiresPointerLock = RequiresPointerLock,
                PointerCaptureAllowed = PointerCaptureAllowed,
                TextEntryActive = TextEntryActive,
                IsPaused = IsPaused,
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>Runtime 可消费的 pointer/mouse 需求读模型。</summary>
    [Serializable]
    public sealed class D22PointerModeReadModel
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool IsValid;
        public D22PointerMode Mode = D22PointerMode.Default;
        public D22PointerVisibilityIntent Visibility = D22PointerVisibilityIntent.Visible;
        public bool MouseRequired;
        public bool PointerLockRequested;
        public bool IsPaused;
        public D22InputModeIntent InputMode = D22InputModeIntent.Unknown;
        public D22PointerModeFailureCode FailureCode;
        public string ReasonCode = string.Empty;
        public string ActiveScreenId = string.Empty;
        public string ActivePanelId = string.Empty;
        public string ActiveModalId = string.Empty;

        /// <summary>兼容 Runtime 接线命名；它只表达需求而不控制设备。</summary>
        public bool RequiresMouse
        {
            get { return MouseRequired; }
        }

        /// <summary>兼容 Runtime 接线命名；它只表达锁定意图。</summary>
        public bool RequiresPointerLock
        {
            get { return PointerLockRequested; }
        }

        /// <summary>兼容可见性命名；不调用任何设备 API。</summary>
        public bool ShouldShowPointer
        {
            get { return Visibility == D22PointerVisibilityIntent.Visible; }
        }

        /// <summary>按显式上下文生成确定性读模型。</summary>
        public static D22PointerModeReadModel Project(D22PointerModeContext context)
        {
            if (context == null)
                return Invalid(D22PointerModeFailureCode.MissingContext, "d22.pointer.missing_context", null);
            if (context.SchemaVersion != D22PointerModeContext.CurrentSchemaVersion)
                return Invalid(D22PointerModeFailureCode.InvalidSchemaVersion, "d22.pointer.invalid_schema_version", context);
            if (!context.SessionActive)
                return Invalid(D22PointerModeFailureCode.InactiveSession, "d22.pointer.inactive_session", context);
            if (!context.ContextValid)
                return Invalid(D22PointerModeFailureCode.InvalidContext, "d22.pointer.invalid_context", context);

            bool modalOrText = !string.IsNullOrEmpty(context.ActiveModalId) || context.TextEntryActive;
            bool requireFreePointer = context.RequiresPointer || modalOrText ||
                                      context.InputMode == D22InputModeIntent.Touch;
            if (requireFreePointer || context.IsPaused)
            {
                return Valid(context, D22PointerMode.Free, D22PointerVisibilityIntent.Visible,
                    true, false, modalOrText ? "d22.pointer.modal_or_text_entry" : "d22.pointer.free_requested");
            }

            if (context.RequiresPointerLock && context.PointerCaptureAllowed)
            {
                return Valid(context, D22PointerMode.Locked, D22PointerVisibilityIntent.Hidden,
                    true, true, "d22.pointer.lock_requested");
            }

            if (context.RequiresPointerLock && !context.PointerCaptureAllowed)
            {
                return Valid(context, D22PointerMode.Free, D22PointerVisibilityIntent.Visible,
                    true, false, "d22.pointer.lock_unavailable", D22PointerModeFailureCode.LockUnavailable);
            }

            return Valid(context, D22PointerMode.Default, D22PointerVisibilityIntent.Visible,
                context.RequiresPointer, false,
                string.IsNullOrEmpty(context.ReasonCode) ? "d22.pointer.default" : context.ReasonCode);
        }

        /// <summary>Project 的语义别名。</summary>
        public static D22PointerModeReadModel FromContext(D22PointerModeContext context)
        {
            return Project(context);
        }

        /// <summary>返回防御性副本。</summary>
        public D22PointerModeReadModel Clone()
        {
            return new D22PointerModeReadModel
            {
                SchemaVersion = SchemaVersion,
                IsValid = IsValid,
                Mode = Mode,
                Visibility = Visibility,
                MouseRequired = MouseRequired,
                PointerLockRequested = PointerLockRequested,
                IsPaused = IsPaused,
                InputMode = InputMode,
                FailureCode = FailureCode,
                ReasonCode = ReasonCode ?? string.Empty,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                ActiveModalId = ActiveModalId ?? string.Empty,
            };
        }

        private static D22PointerModeReadModel Valid(
            D22PointerModeContext context,
            D22PointerMode mode,
            D22PointerVisibilityIntent visibility,
            bool mouseRequired,
            bool lockRequested,
            string reasonCode,
            D22PointerModeFailureCode failureCode = D22PointerModeFailureCode.None)
        {
            return new D22PointerModeReadModel
            {
                IsValid = true,
                Mode = mode,
                Visibility = visibility,
                MouseRequired = mouseRequired,
                PointerLockRequested = lockRequested,
                IsPaused = context != null && context.IsPaused,
                InputMode = context == null ? D22InputModeIntent.Unknown : context.InputMode,
                FailureCode = failureCode,
                ReasonCode = reasonCode ?? string.Empty,
                ActiveScreenId = context == null ? string.Empty : context.ActiveScreenId ?? string.Empty,
                ActivePanelId = context == null ? string.Empty : context.ActivePanelId ?? string.Empty,
                ActiveModalId = context == null ? string.Empty : context.ActiveModalId ?? string.Empty,
            };
        }

        private static D22PointerModeReadModel Invalid(
            D22PointerModeFailureCode failureCode,
            string reasonCode,
            D22PointerModeContext context)
        {
            return new D22PointerModeReadModel
            {
                IsValid = false,
                Mode = D22PointerMode.Default,
                Visibility = D22PointerVisibilityIntent.Visible,
                MouseRequired = false,
                PointerLockRequested = false,
                IsPaused = context != null && context.IsPaused,
                InputMode = context == null ? D22InputModeIntent.Unknown : context.InputMode,
                FailureCode = failureCode,
                ReasonCode = reasonCode ?? string.Empty,
                ActiveScreenId = context == null ? string.Empty : context.ActiveScreenId ?? string.Empty,
                ActivePanelId = context == null ? string.Empty : context.ActivePanelId ?? string.Empty,
                ActiveModalId = context == null ? string.Empty : context.ActiveModalId ?? string.Empty,
            };
        }
    }

    /// <summary>显式的 projector 入口，便于 Runtime 只依赖公开读模型 API。</summary>
    public static class D22PointerModeProjector
    {
        public const string OwnerSystem = "D22PointerModeReadModel";

        /// <summary>生成 pointer 需求读模型，不控制设备。</summary>
        public static D22PointerModeReadModel Project(D22PointerModeContext context)
        {
            return D22PointerModeReadModel.Project(context);
        }
    }
}
