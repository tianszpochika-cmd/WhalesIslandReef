// ============================================================
// D22LoadStateReadModel —— D22-L16 加载/保存状态读模型（纯 C#）
//
// SaveSystem/domain owner 提供输入快照，D22 只把它投影成 UI 可读状态。
// 迁移、恢复、缺少模块和失败都保留稳定机器码，不执行真实存档或业务写入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>存档边界操作种类。</summary>
    public enum D22LoadOperationKind
    {
        None = 0,
        Load = 1,
        LoadGame = Load,
        Save = 2,
        SaveGame = Save,
        Migrate = 3,
        Migration = Migrate,
        Recover = 4,
        Recovery = Recover,
    }

    /// <summary>加载/保存/迁移状态的逻辑生命周期。</summary>
    public enum D22LoadLifecycleState
    {
        Idle = 0,
        Loading = 1,
        Saving = 2,
        Migrating = 3,
        Recovering = 4,
        Succeeded = 5,
        Recovered = 6,
        Failed = 7,
        MissingModule = 8,
        Cancelled = 9,
    }

    /// <summary>owner 快照中单个模块的状态。</summary>
    public enum D22LoadModuleStatus
    {
        Present = 0,
        Missing = 1,
        MissingModule = Missing,
        Incompatible = 2,
        IncompatibleModule = Incompatible,
        Failed = 3,
        Recovered = 4,
    }

    /// <summary>加载状态读模型的稳定失败码。</summary>
    public enum D22LoadFailureCode
    {
        None = 0,
        MissingSnapshot = 1,
        InvalidSchemaVersion = 2,
        InvalidSlotId = 3,
        InvalidProgress = 4,
        InvalidModule = 5,
        LoadFailed = 6,
        SaveFailed = 7,
        MigrationFailed = 8,
        RecoveryFailed = 9,
        MissingModule = 10,
        IncompatibleModule = 11,
        CorruptData = 12,
        InvalidOperationState = 13,
        InvalidRevision = 14,
    }

    /// <summary>来自 SaveSystem 或领域 owner 的结构化状态快照。</summary>
    [Serializable]
    public sealed class D22LoadStateSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D22LoadOperationKind Operation;
        public D22LoadLifecycleState State = D22LoadLifecycleState.Idle;
        public string SlotId = string.Empty;
        public long SaveRevision;
        public long SourceRevision;
        public long TargetRevision;
        public int ProgressPercent;
        public string ErrorCode = string.Empty;
        public string ReasonCode = string.Empty;
        public bool Retryable;
        public bool RecoveryAvailable;
        public List<D22LoadModuleState> Modules = new List<D22LoadModuleState>();
        public List<string> MissingModuleIds = new List<string>();

        /// <summary>返回深拷贝。</summary>
        public D22LoadStateSnapshot Clone()
        {
            var result = new D22LoadStateSnapshot
            {
                SchemaVersion = SchemaVersion,
                Operation = Operation,
                State = State,
                SlotId = SlotId ?? string.Empty,
                SaveRevision = SaveRevision,
                SourceRevision = SourceRevision,
                TargetRevision = TargetRevision,
                ProgressPercent = ProgressPercent,
                ErrorCode = ErrorCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                Retryable = Retryable,
                RecoveryAvailable = RecoveryAvailable,
            };
            if (Modules != null)
                foreach (var module in Modules) result.Modules.Add(module == null ? null : module.Clone());
            if (MissingModuleIds != null) result.MissingModuleIds.AddRange(MissingModuleIds);
            return result;
        }
    }

    /// <summary>owner 快照中的模块状态。</summary>
    [Serializable]
    public sealed class D22LoadModuleState
    {
        public string ModuleId = string.Empty;
        public D22LoadModuleStatus Status = D22LoadModuleStatus.Present;
        public bool Required = true;
        public int ModuleSchemaVersion;
        public string ErrorCode = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22LoadModuleState Clone()
        {
            return new D22LoadModuleState
            {
                ModuleId = ModuleId ?? string.Empty,
                Status = Status,
                Required = Required,
                ModuleSchemaVersion = ModuleSchemaVersion,
                ErrorCode = ErrorCode ?? string.Empty,
            };
        }
    }

    /// <summary>D22 面向 UI 的模块只读投影。</summary>
    [Serializable]
    public sealed class D22LoadModuleReadModel
    {
        public string ModuleId = string.Empty;
        public D22LoadModuleStatus Status = D22LoadModuleStatus.Present;
        public bool Required = true;
        public int ModuleSchemaVersion;
        public string ErrorCode = string.Empty;

        /// <summary>返回防御性副本。</summary>
        public D22LoadModuleReadModel Clone()
        {
            return new D22LoadModuleReadModel
            {
                ModuleId = ModuleId ?? string.Empty,
                Status = Status,
                Required = Required,
                ModuleSchemaVersion = ModuleSchemaVersion,
                ErrorCode = ErrorCode ?? string.Empty,
            };
        }
    }

    /// <summary>加载/保存/迁移/恢复的 UI 可读状态。</summary>
    [Serializable]
    public sealed class D22LoadStateReadModel
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool IsValid;
        public D22LoadOperationKind Operation;
        public D22LoadLifecycleState State = D22LoadLifecycleState.Idle;
        public string SlotId = string.Empty;
        public long SaveRevision;
        public long SourceRevision;
        public long TargetRevision;
        public int ProgressPercent;
        public bool IsBusy;
        public bool IsTerminal;
        public bool Succeeded;
        public bool CanRetry;
        public bool CanRecover;
        public bool HasMissingModules;
        public D22LoadFailureCode FailureCode;
        public string ErrorCode = string.Empty;
        public string ReasonCode = string.Empty;
        public string StatusLocalizationKey = string.Empty;
        public string FeedbackCode = string.Empty;
        public List<D22LoadModuleReadModel> Modules = new List<D22LoadModuleReadModel>();
        public List<string> MissingModuleIds = new List<string>();

        /// <summary>从 owner 公开快照生成 D22 读模型。</summary>
        public static D22LoadStateReadModel Project(D22LoadStateSnapshot snapshot)
        {
            if (snapshot == null)
                return Invalid(D22LoadFailureCode.MissingSnapshot, "d22.load.missing_snapshot", string.Empty);
            if (snapshot.SchemaVersion != D22LoadStateSnapshot.CurrentSchemaVersion)
                return Invalid(D22LoadFailureCode.InvalidSchemaVersion, "d22.load.invalid_schema_version", snapshot.SlotId);
            if (snapshot.ProgressPercent < 0 || snapshot.ProgressPercent > 100)
                return Invalid(D22LoadFailureCode.InvalidProgress, "d22.load.invalid_progress", snapshot.SlotId);
            if (snapshot.SaveRevision < 0 || snapshot.SourceRevision < 0 || snapshot.TargetRevision < 0)
                return Invalid(D22LoadFailureCode.InvalidRevision, "d22.load.invalid_revision", snapshot.SlotId);
            if (RequiresSlot(snapshot.Operation) && !D22ScreenContracts.IsStableId(snapshot.SlotId))
                return Invalid(D22LoadFailureCode.InvalidSlotId, "d22.load.invalid_slot_id", snapshot.SlotId);
            if (!IsValidOperationState(snapshot.Operation, snapshot.State))
                return Invalid(D22LoadFailureCode.InvalidOperationState, "d22.load.invalid_operation_state", snapshot.SlotId);

            var result = new D22LoadStateReadModel
            {
                IsValid = true,
                Operation = snapshot.Operation,
                State = snapshot.State,
                SlotId = snapshot.SlotId ?? string.Empty,
                SaveRevision = snapshot.SaveRevision,
                SourceRevision = snapshot.SourceRevision,
                TargetRevision = snapshot.TargetRevision,
                ProgressPercent = snapshot.ProgressPercent,
                ErrorCode = snapshot.ErrorCode ?? string.Empty,
                ReasonCode = snapshot.ReasonCode ?? string.Empty,
                CanRetry = snapshot.Retryable,
                CanRecover = snapshot.RecoveryAvailable,
            };

            var seenModuleIds = new HashSet<string>(StringComparer.Ordinal);
            if (snapshot.Modules != null)
            {
                foreach (var module in snapshot.Modules)
                {
                    if (module == null || !D22ScreenContracts.IsStableId(module.ModuleId) ||
                        !seenModuleIds.Add(module.ModuleId) ||
                        module.Status < D22LoadModuleStatus.Present || module.Status > D22LoadModuleStatus.Recovered)
                        return Invalid(D22LoadFailureCode.InvalidModule, "d22.load.invalid_module", snapshot.SlotId);
                    result.Modules.Add(new D22LoadModuleReadModel
                    {
                        ModuleId = module.ModuleId,
                        Status = module.Status,
                        Required = module.Required,
                        ModuleSchemaVersion = module.ModuleSchemaVersion,
                        ErrorCode = module.ErrorCode ?? string.Empty,
                    });
                    if (module.Status == D22LoadModuleStatus.Missing ||
                        module.Status == D22LoadModuleStatus.Incompatible ||
                        module.Status == D22LoadModuleStatus.Failed)
                        AddMissingModule(result.MissingModuleIds, module.ModuleId);
                    if (module.Status == D22LoadModuleStatus.Incompatible)
                        result.FailureCode = D22LoadFailureCode.IncompatibleModule;
                }
            }
            if (snapshot.MissingModuleIds != null)
            {
                foreach (var moduleId in snapshot.MissingModuleIds)
                {
                    if (!D22ScreenContracts.IsStableId(moduleId))
                        return Invalid(D22LoadFailureCode.InvalidModule, "d22.load.invalid_module", snapshot.SlotId);
                    AddMissingModule(result.MissingModuleIds, moduleId);
                }
            }

            result.HasMissingModules = result.MissingModuleIds.Count > 0;
            if (result.HasMissingModules &&
                result.State != D22LoadLifecycleState.Succeeded &&
                result.State != D22LoadLifecycleState.Recovered)
            {
                result.State = D22LoadLifecycleState.MissingModule;
                result.FailureCode = result.FailureCode == D22LoadFailureCode.None
                    ? D22LoadFailureCode.MissingModule
                    : result.FailureCode;
                result.CanRecover = true;
            }

            if (result.FailureCode == D22LoadFailureCode.None && result.State == D22LoadLifecycleState.Failed)
                result.FailureCode = FailureFor(result.Operation, result.ErrorCode);
            if (result.State == D22LoadLifecycleState.MissingModule && result.FailureCode == D22LoadFailureCode.None)
                result.FailureCode = D22LoadFailureCode.MissingModule;
            result.IsBusy = result.State == D22LoadLifecycleState.Loading ||
                            result.State == D22LoadLifecycleState.Saving ||
                            result.State == D22LoadLifecycleState.Migrating ||
                            result.State == D22LoadLifecycleState.Recovering;
            result.Succeeded = result.State == D22LoadLifecycleState.Succeeded ||
                               result.State == D22LoadLifecycleState.Recovered;
            if (result.State == D22LoadLifecycleState.Failed)
            {
                if (result.FailureCode == D22LoadFailureCode.LoadFailed ||
                    result.FailureCode == D22LoadFailureCode.SaveFailed ||
                    result.FailureCode == D22LoadFailureCode.MigrationFailed)
                    result.CanRetry = true;
                result.IsTerminal = !result.CanRetry && !result.CanRecover;
            }
            result.StatusLocalizationKey = LocalizationKeyFor(result.State);
            result.FeedbackCode = FeedbackFor(result.State, result.FailureCode);
            if (string.IsNullOrEmpty(result.ReasonCode))
                result.ReasonCode = ReasonFor(result.State, result.FailureCode);
            return result;
        }

        /// <summary>Project 的语义别名。</summary>
        public static D22LoadStateReadModel FromSnapshot(D22LoadStateSnapshot snapshot)
        {
            return Project(snapshot);
        }

        /// <summary>返回防御性副本。</summary>
        public D22LoadStateReadModel Clone()
        {
            var result = new D22LoadStateReadModel
            {
                SchemaVersion = SchemaVersion,
                IsValid = IsValid,
                Operation = Operation,
                State = State,
                SlotId = SlotId ?? string.Empty,
                SaveRevision = SaveRevision,
                SourceRevision = SourceRevision,
                TargetRevision = TargetRevision,
                ProgressPercent = ProgressPercent,
                IsBusy = IsBusy,
                IsTerminal = IsTerminal,
                Succeeded = Succeeded,
                CanRetry = CanRetry,
                CanRecover = CanRecover,
                HasMissingModules = HasMissingModules,
                FailureCode = FailureCode,
                ErrorCode = ErrorCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                StatusLocalizationKey = StatusLocalizationKey ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
            };
            if (Modules != null)
                foreach (var module in Modules) result.Modules.Add(module == null ? null : module.Clone());
            if (MissingModuleIds != null) result.MissingModuleIds.AddRange(MissingModuleIds);
            return result;
        }

        private static D22LoadStateReadModel Invalid(
            D22LoadFailureCode failureCode,
            string reasonCode,
            string slotId)
        {
            return new D22LoadStateReadModel
            {
                IsValid = false,
                State = D22LoadLifecycleState.Failed,
                FailureCode = failureCode,
                ReasonCode = reasonCode ?? string.Empty,
                StatusLocalizationKey = "ui.d22.load.failed",
                FeedbackCode = D22FeedbackCodes.OwnerRejected,
                SlotId = slotId ?? string.Empty,
                IsTerminal = true,
            };
        }

        private static bool RequiresSlot(D22LoadOperationKind operation)
        {
            return operation == D22LoadOperationKind.Load || operation == D22LoadOperationKind.Save ||
                   operation == D22LoadOperationKind.Migrate || operation == D22LoadOperationKind.Recover;
        }

        private static bool IsValidOperationState(D22LoadOperationKind operation, D22LoadLifecycleState state)
        {
            if (operation < D22LoadOperationKind.None || operation > D22LoadOperationKind.Recover ||
                state < D22LoadLifecycleState.Idle || state > D22LoadLifecycleState.Cancelled)
                return false;
            if (operation == D22LoadOperationKind.Load)
                return state == D22LoadLifecycleState.Loading || state == D22LoadLifecycleState.Succeeded ||
                       state == D22LoadLifecycleState.Failed || state == D22LoadLifecycleState.MissingModule ||
                       state == D22LoadLifecycleState.Recovering || state == D22LoadLifecycleState.Recovered ||
                       state == D22LoadLifecycleState.Cancelled;
            if (operation == D22LoadOperationKind.Save)
                return state == D22LoadLifecycleState.Saving || state == D22LoadLifecycleState.Succeeded ||
                       state == D22LoadLifecycleState.Failed || state == D22LoadLifecycleState.Cancelled;
            if (operation == D22LoadOperationKind.Migrate)
                return state == D22LoadLifecycleState.Migrating || state == D22LoadLifecycleState.Succeeded ||
                       state == D22LoadLifecycleState.Failed || state == D22LoadLifecycleState.MissingModule ||
                       state == D22LoadLifecycleState.Cancelled;
            if (operation == D22LoadOperationKind.Recover)
                return state == D22LoadLifecycleState.Recovering || state == D22LoadLifecycleState.Recovered ||
                       state == D22LoadLifecycleState.Failed || state == D22LoadLifecycleState.MissingModule ||
                       state == D22LoadLifecycleState.Cancelled;
            return state == D22LoadLifecycleState.Idle;
        }

        private static void AddMissingModule(List<string> moduleIds, string moduleId)
        {
            if (!moduleIds.Contains(moduleId)) moduleIds.Add(moduleId);
        }

        private static D22LoadFailureCode FailureFor(D22LoadOperationKind operation, string errorCode)
        {
            string normalized = (errorCode ?? string.Empty).ToLowerInvariant();
            if (normalized.Contains("missing") && normalized.Contains("module")) return D22LoadFailureCode.MissingModule;
            if (normalized.Contains("incompatible")) return D22LoadFailureCode.IncompatibleModule;
            if (normalized.Contains("corrupt")) return D22LoadFailureCode.CorruptData;
            if (normalized.Contains("migration") || normalized.Contains("migrate")) return D22LoadFailureCode.MigrationFailed;
            if (operation == D22LoadOperationKind.Save) return D22LoadFailureCode.SaveFailed;
            if (operation == D22LoadOperationKind.Recover) return D22LoadFailureCode.RecoveryFailed;
            return D22LoadFailureCode.LoadFailed;
        }

        private static string LocalizationKeyFor(D22LoadLifecycleState state)
        {
            switch (state)
            {
                case D22LoadLifecycleState.Loading: return "ui.d22.load.loading";
                case D22LoadLifecycleState.Saving: return "ui.d22.load.saving";
                case D22LoadLifecycleState.Migrating: return "ui.d22.load.migrating";
                case D22LoadLifecycleState.Recovering: return "ui.d22.load.recovering";
                case D22LoadLifecycleState.Succeeded: return "ui.d22.load.succeeded";
                case D22LoadLifecycleState.Recovered: return "ui.d22.load.recovered";
                case D22LoadLifecycleState.MissingModule: return "ui.d22.load.missing_module";
                case D22LoadLifecycleState.Cancelled: return "ui.d22.load.cancelled";
                case D22LoadLifecycleState.Failed: return "ui.d22.load.failed";
                default: return "ui.d22.load.idle";
            }
        }

        private static string ReasonFor(D22LoadLifecycleState state, D22LoadFailureCode failureCode)
        {
            if (failureCode != D22LoadFailureCode.None)
                return "d22.load.failure." + failureCode.ToString().ToLowerInvariant();
            return "d22.load.state." + state.ToString().ToLowerInvariant();
        }

        private static string FeedbackFor(D22LoadLifecycleState state, D22LoadFailureCode failureCode)
        {
            if (state == D22LoadLifecycleState.Succeeded || state == D22LoadLifecycleState.Recovered)
                return D22FeedbackCodes.CommandCompleted;
            if (state == D22LoadLifecycleState.Loading || state == D22LoadLifecycleState.Saving ||
                state == D22LoadLifecycleState.Migrating || state == D22LoadLifecycleState.Recovering)
                return D22FeedbackCodes.OwnerPending;
            if (state == D22LoadLifecycleState.MissingModule || failureCode == D22LoadFailureCode.MissingModule ||
                failureCode == D22LoadFailureCode.IncompatibleModule)
                return D22FeedbackCodes.OwnerUnavailable;
            if (state == D22LoadLifecycleState.Failed)
                return D22FeedbackCodes.OwnerRejected;
            return D22FeedbackCodes.CommandAccepted;
        }
    }

    /// <summary>公开的只读投影入口。</summary>
    public static class D22LoadStateProjector
    {
        public const string OwnerSystem = "D22LoadStateReadModel";

        /// <summary>把 owner 快照投影成 UI 状态，不执行加载或保存。</summary>
        public static D22LoadStateReadModel Project(D22LoadStateSnapshot snapshot)
        {
            return D22LoadStateReadModel.Project(snapshot);
        }
    }
}
