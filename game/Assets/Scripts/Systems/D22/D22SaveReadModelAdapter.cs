// ============================================================
// D22SaveReadModelAdapter —— D22-L23 存档只读 UI 适配器（纯 C#）
//
// SaveSystem/SaveMigration 只提供公开状态结果；本适配器不读取 SaveSystem
// 的 Unity 依赖、不执行文件 I/O，也不触发保存/加载/迁移。Slot 完整性、
// 缺失模块、版本和失败码都以只读状态投影给 UI，缺失来源 fail-closed。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D22
{
    /// <summary>存档 slot 的公开完整性快照；不携带路径或凭据。</summary>
    [Serializable]
    public sealed class D22SaveSlotOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string SlotId = string.Empty;
        public bool Exists;
        public bool IsReadable;
        public bool IntegrityOk;
        public long SaveRevision;
        public int WorldDay;
        public string ErrorCode = string.Empty;
        public string ReasonCode = string.Empty;
        public string LocalizationKey = string.Empty;
    }

    /// <summary>SaveSystem/SaveMigration 的 D22 输入边界。</summary>
    [Serializable]
    public sealed class D22SaveOwnerSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public int SourceSchemaVersion = 1;
        public int WorldDay;
        public long SaveRevision;
        public D22LoadStateSnapshot LoadState;
        public List<D22SaveSlotOwnerSnapshot> Slots = new List<D22SaveSlotOwnerSnapshot>();
        public SaveMigrationResult MigrationResult;
        public string DisabledReasonCode = string.Empty;
        public string DisabledLocalizationKey = string.Empty;
        public List<string> MissingModuleIds = new List<string>();
    }

    /// <summary>保存 slot 的只读完整性投影。</summary>
    [Serializable]
    public sealed class D22SaveSlotReadModel
    {
        public string SlotId { get; }
        public bool Exists { get; }
        public bool IsReadable { get; }
        public bool IntegrityOk { get; }
        public long SaveRevision { get; }
        public int WorldDay { get; }
        public string ErrorCode { get; }
        public string ReasonCode { get; }
        public string DisabledReasonCode { get; }
        public string LocalizationKey { get; }
        public string SourceLocalizationKey { get; }

        internal D22SaveSlotReadModel(D22SaveSlotOwnerSnapshot source)
        {
            SlotId = source == null ? string.Empty : source.SlotId ?? string.Empty;
            Exists = source != null && source.Exists;
            IsReadable = source != null && source.IsReadable;
            IntegrityOk = source != null && source.IntegrityOk;
            SaveRevision = source == null ? 0 : source.SaveRevision;
            WorldDay = source == null ? 0 : source.WorldDay;
            ErrorCode = source == null ? "d22.save.slot_missing" : source.ErrorCode ?? string.Empty;
            ReasonCode = source == null ? "d22.save.slot_missing" : source.ReasonCode ?? string.Empty;
            DisabledReasonCode = Exists && IsReadable && IntegrityOk ? string.Empty :
                (string.IsNullOrEmpty(ReasonCode) ? "d22.save.slot_unavailable" : ReasonCode);
            LocalizationKey = D22ReadModelAdapterSupport.Key("save", "slot");
            SourceLocalizationKey = source == null ? string.Empty : source.LocalizationKey ?? string.Empty;
        }

        private D22SaveSlotReadModel(
            string slotId,
            bool exists,
            bool isReadable,
            bool integrityOk,
            long saveRevision,
            int worldDay,
            string errorCode,
            string reasonCode,
            string disabledReasonCode,
            string localizationKey,
            string sourceLocalizationKey)
        {
            SlotId = slotId ?? string.Empty;
            Exists = exists;
            IsReadable = isReadable;
            IntegrityOk = integrityOk;
            SaveRevision = saveRevision;
            WorldDay = worldDay;
            ErrorCode = errorCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            DisabledReasonCode = disabledReasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            SourceLocalizationKey = sourceLocalizationKey ?? string.Empty;
        }

        public D22SaveSlotReadModel Clone()
        {
            return new D22SaveSlotReadModel(
                SlotId, Exists, IsReadable, IntegrityOk, SaveRevision, WorldDay,
                ErrorCode, ReasonCode, DisabledReasonCode, LocalizationKey, SourceLocalizationKey);
        }
    }

    /// <summary>加载状态中的模块只读投影。</summary>
    [Serializable]
    public sealed class D22SaveModuleReadModel
    {
        public string ModuleId { get; }
        public string Status { get; }
        public bool Required { get; }
        public int ModuleSchemaVersion { get; }
        public string ErrorCode { get; }
        public string LocalizationKey { get; }

        internal D22SaveModuleReadModel(D22LoadModuleReadModel source)
        {
            ModuleId = source == null ? string.Empty : source.ModuleId ?? string.Empty;
            Status = source == null ? string.Empty : source.Status.ToString();
            Required = source != null && source.Required;
            ModuleSchemaVersion = source == null ? 0 : source.ModuleSchemaVersion;
            ErrorCode = source == null ? "d22.save.module_missing" : source.ErrorCode ?? string.Empty;
            LocalizationKey = D22ReadModelAdapterSupport.Key("save", "module");
        }

        private D22SaveModuleReadModel(
            string moduleId,
            string status,
            bool required,
            int moduleSchemaVersion,
            string errorCode,
            string localizationKey)
        {
            ModuleId = moduleId ?? string.Empty;
            Status = status ?? string.Empty;
            Required = required;
            ModuleSchemaVersion = moduleSchemaVersion;
            ErrorCode = errorCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
        }

        public D22SaveModuleReadModel Clone()
        {
            return new D22SaveModuleReadModel(
                ModuleId, Status, Required, ModuleSchemaVersion, ErrorCode, LocalizationKey);
        }
    }

    /// <summary>D22LoadStateReadModel 的不可变副本。</summary>
    [Serializable]
    public sealed class D22SaveLoadStateReadModel
    {
        public bool HasProjection { get; }
        public bool IsValid { get; }
        public string Operation { get; }
        public string State { get; }
        public string SlotId { get; }
        public long SaveRevision { get; }
        public long SourceRevision { get; }
        public long TargetRevision { get; }
        public int ProgressPercent { get; }
        public bool IsBusy { get; }
        public bool IsTerminal { get; }
        public bool Succeeded { get; }
        public bool CanRetry { get; }
        public bool CanRecover { get; }
        public bool HasMissingModules { get; }
        public string FailureCode { get; }
        public string ErrorCode { get; }
        public string ReasonCode { get; }
        public string LocalizationKey { get; }
        public string FeedbackCode { get; }
        public IReadOnlyList<D22SaveModuleReadModel> Modules { get; }
        public IReadOnlyList<string> MissingModuleIds { get; }

        internal D22SaveLoadStateReadModel(D22LoadStateReadModel source)
        {
            HasProjection = source != null;
            IsValid = source != null && source.IsValid;
            Operation = source == null ? string.Empty : source.Operation.ToString();
            State = source == null ? string.Empty : source.State.ToString();
            SlotId = source == null ? string.Empty : source.SlotId ?? string.Empty;
            SaveRevision = source == null ? 0 : source.SaveRevision;
            SourceRevision = source == null ? 0 : source.SourceRevision;
            TargetRevision = source == null ? 0 : source.TargetRevision;
            ProgressPercent = source == null ? 0 : source.ProgressPercent;
            IsBusy = source != null && source.IsBusy;
            IsTerminal = source != null && source.IsTerminal;
            Succeeded = source != null && source.Succeeded;
            CanRetry = source != null && source.CanRetry;
            CanRecover = source != null && source.CanRecover;
            HasMissingModules = source != null && source.HasMissingModules;
            FailureCode = source == null ? string.Empty : source.FailureCode.ToString();
            ErrorCode = source == null ? "d22.save.load_state_missing" : source.ErrorCode ?? string.Empty;
            ReasonCode = source == null ? "d22.save.load_state_missing" : source.ReasonCode ?? string.Empty;
            LocalizationKey = D22ReadModelAdapterSupport.Key("save", "load_state");
            FeedbackCode = source == null ? D22FeedbackCodes.OwnerUnavailable : source.FeedbackCode ?? string.Empty;
            var modules = new List<D22SaveModuleReadModel>();
            if (source != null && source.Modules != null)
                foreach (var module in source.Modules)
                    if (module != null) modules.Add(new D22SaveModuleReadModel(module));
            modules.Sort(CompareModules);
            Modules = new ReadOnlyCollection<D22SaveModuleReadModel>(modules);
            MissingModuleIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(
                source == null ? null : source.MissingModuleIds));
        }

        private D22SaveLoadStateReadModel(
            bool hasProjection,
            bool isValid,
            string operation,
            string state,
            string slotId,
            long saveRevision,
            long sourceRevision,
            long targetRevision,
            int progressPercent,
            bool isBusy,
            bool isTerminal,
            bool succeeded,
            bool canRetry,
            bool canRecover,
            bool hasMissingModules,
            string failureCode,
            string errorCode,
            string reasonCode,
            string localizationKey,
            string feedbackCode,
            IEnumerable<D22SaveModuleReadModel> modules,
            IEnumerable<string> missingModuleIds)
        {
            HasProjection = hasProjection;
            IsValid = isValid;
            Operation = operation ?? string.Empty;
            State = state ?? string.Empty;
            SlotId = slotId ?? string.Empty;
            SaveRevision = saveRevision;
            SourceRevision = sourceRevision;
            TargetRevision = targetRevision;
            ProgressPercent = progressPercent;
            IsBusy = isBusy;
            IsTerminal = isTerminal;
            Succeeded = succeeded;
            CanRetry = canRetry;
            CanRecover = canRecover;
            HasMissingModules = hasMissingModules;
            FailureCode = failureCode ?? string.Empty;
            ErrorCode = errorCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            LocalizationKey = localizationKey ?? string.Empty;
            FeedbackCode = feedbackCode ?? string.Empty;
            var copiedModules = new List<D22SaveModuleReadModel>();
            if (modules != null)
                foreach (var module in modules)
                    if (module != null) copiedModules.Add(module.Clone());
            copiedModules.Sort(CompareModules);
            Modules = new ReadOnlyCollection<D22SaveModuleReadModel>(copiedModules);
            MissingModuleIds = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(missingModuleIds));
        }

        public D22SaveLoadStateReadModel Clone()
        {
            return new D22SaveLoadStateReadModel(
                HasProjection, IsValid, Operation, State, SlotId, SaveRevision, SourceRevision,
                TargetRevision, ProgressPercent, IsBusy, IsTerminal, Succeeded, CanRetry,
                CanRecover, HasMissingModules, FailureCode, ErrorCode, ReasonCode,
                LocalizationKey, FeedbackCode, Modules, MissingModuleIds);
        }

        private static int CompareModules(D22SaveModuleReadModel left, D22SaveModuleReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.ModuleId,
                right == null ? null : right.ModuleId);
        }
    }

    /// <summary>迁移结果的 UI 安全只读投影；不暴露 BackupPath。</summary>
    [Serializable]
    public sealed class D22SaveMigrationReadModel
    {
        public bool HasProjection { get; }
        public string Status { get; }
        public int SourceUserVersion { get; }
        public int SaveVersion { get; }
        public int WorldRevision { get; }
        public string LastEventId { get; }
        public string ErrorCode { get; }
        public bool OriginalPreserved { get; }
        public bool Succeeded { get; }
        public IReadOnlyList<string> MigrationFlags { get; }
        public string LocalizationKey { get; }

        internal D22SaveMigrationReadModel(SaveMigrationResult source)
        {
            HasProjection = source != null;
            Status = source == null ? string.Empty : source.Status.ToString();
            SourceUserVersion = source == null ? 0 : source.SourceUserVersion;
            SaveVersion = source == null ? 0 : source.SaveVersion;
            WorldRevision = source == null ? 0 : source.WorldRevision;
            LastEventId = source == null ? string.Empty : source.LastEventId ?? string.Empty;
            ErrorCode = source == null ? string.Empty : source.ErrorCode ?? string.Empty;
            OriginalPreserved = source != null && source.OriginalPreserved;
            Succeeded = source != null && source.Succeeded;
            MigrationFlags = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(
                source == null ? null : source.MigrationFlags));
            LocalizationKey = D22ReadModelAdapterSupport.Key("save", "migration");
        }

        private D22SaveMigrationReadModel(
            bool hasProjection,
            string status,
            int sourceUserVersion,
            int saveVersion,
            int worldRevision,
            string lastEventId,
            string errorCode,
            bool originalPreserved,
            bool succeeded,
            IEnumerable<string> migrationFlags,
            string localizationKey)
        {
            HasProjection = hasProjection;
            Status = status ?? string.Empty;
            SourceUserVersion = sourceUserVersion;
            SaveVersion = saveVersion;
            WorldRevision = worldRevision;
            LastEventId = lastEventId ?? string.Empty;
            ErrorCode = errorCode ?? string.Empty;
            OriginalPreserved = originalPreserved;
            Succeeded = succeeded;
            MigrationFlags = new ReadOnlyCollection<string>(D22ReadModelAdapterSupport.SortedDistinct(migrationFlags));
            LocalizationKey = localizationKey ?? string.Empty;
        }

        public D22SaveMigrationReadModel Clone()
        {
            return new D22SaveMigrationReadModel(
                HasProjection, Status, SourceUserVersion, SaveVersion, WorldRevision,
                LastEventId, ErrorCode, OriginalPreserved, Succeeded, MigrationFlags, LocalizationKey);
        }
    }

    /// <summary>D22-L23 存档 UI 读模型。</summary>
    [Serializable]
    public sealed class D22SaveReadModel : D22ReadModelAdapterModel
    {
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public D22SaveLoadStateReadModel LoadState { get; }
        public D22SaveMigrationReadModel Migration { get; }
        public IReadOnlyList<D22SaveSlotReadModel> Slots { get; }
        public D22AdapterOutcomeReadModel LastOperation { get; }

        private D22SaveReadModel(
            D22ReadModelAdapterMetadata metadata,
            int worldDay,
            long saveRevision,
            D22SaveLoadStateReadModel loadState,
            D22SaveMigrationReadModel migration,
            IEnumerable<D22SaveSlotReadModel> slots,
            D22AdapterOutcomeReadModel lastOperation)
            : base(metadata)
        {
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            LoadState = loadState == null ? null : loadState.Clone();
            Migration = migration == null ? null : migration.Clone();
            var copied = new List<D22SaveSlotReadModel>();
            if (slots != null)
                foreach (var slot in slots)
                    if (slot != null) copied.Add(slot.Clone());
            copied.Sort(CompareSlots);
            Slots = new ReadOnlyCollection<D22SaveSlotReadModel>(copied);
            LastOperation = lastOperation == null ? null : lastOperation.Clone();
        }

        public D22SaveReadModel Clone()
        {
            return new D22SaveReadModel(CopyMetadata(), WorldDay, SaveRevision, LoadState, Migration, Slots, LastOperation);
        }

        public static D22SaveReadModel Project(D22SaveOwnerSnapshot snapshot)
        {
            var issues = new List<D22ReadModelAdapterIssue>();
            var missing = new List<string>();
            if (snapshot == null)
            {
                missing.Add("save.snapshot");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_snapshot", "d22.save.snapshot_missing", D22ReadModelAdapterSupport.Key("save", "blocked"),
                    "d22.save.snapshot_missing", "SaveSystem", "save.snapshot", true));
                return Empty(issues, missing);
            }
            if (snapshot.SchemaVersion != D22SaveOwnerSnapshot.CurrentSchemaVersion)
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_schema_version", "d22.save.schema_version_mismatch", D22ReadModelAdapterSupport.Key("save", "invalid"),
                    "d22.save.schema_version_mismatch", "D22SaveOwnerSnapshot", "d22.save", true));
            if (snapshot.SourceSchemaVersion <= 0)
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "invalid_source_schema_version", "d22.save.source_schema_version_invalid", D22ReadModelAdapterSupport.Key("save", "invalid"),
                    "d22.save.source_schema_version_invalid", "SaveSystem", "save.snapshot", true));
            D22ReadModelAdapterSupport.ValidateRevisionAndDay(snapshot.SaveRevision, snapshot.WorldDay, "SaveSystem", "save.snapshot", issues);

            D22SaveLoadStateReadModel loadState = null;
            if (snapshot.LoadState == null)
            {
                missing.Add("save.load_state");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_load_state", "d22.save.load_state_missing", D22ReadModelAdapterSupport.Key("save", "partial"),
                    "d22.save.load_state_missing", "SaveSystem", "save.load_state", false));
            }
            else
            {
                var projected = D22LoadStateReadModel.Project(snapshot.LoadState);
                loadState = new D22SaveLoadStateReadModel(projected);
                if (!projected.IsValid)
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "invalid_load_state", "d22.save.load_state_invalid", D22ReadModelAdapterSupport.Key("save", "invalid"),
                        "d22.save.load_state_invalid", "D22LoadStateReadModel", "save.load_state", false));
                }
                if (projected.HasMissingModules)
                {
                    issues.Add(D22ReadModelAdapterSupport.Issue(
                        "missing_save_modules", "d22.save.modules_missing", D22ReadModelAdapterSupport.Key("save", "partial"),
                        "d22.save.modules_missing", "SaveSystem", "save.modules", false));
                }
                D22ReadModelAdapterSupport.ValidateSourceCode(projected.ErrorCode, "SaveSystem", "save.load_state", issues);
                D22ReadModelAdapterSupport.ValidateSourceCode(projected.ReasonCode, "SaveSystem", "save.load_state", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(projected.StatusLocalizationKey, "D22LoadStateReadModel", "save.load_state", issues);
            }

            var slots = new List<D22SaveSlotReadModel>();
            if (snapshot.Slots == null)
            {
                missing.Add("save.slots");
                issues.Add(D22ReadModelAdapterSupport.Issue(
                    "missing_slots", "d22.save.slots_missing", D22ReadModelAdapterSupport.Key("save", "partial"),
                    "d22.save.slots_missing", "SaveSystem", "save.slots", false));
            }
            else
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var slot in snapshot.Slots)
                {
                    if (slot == null || !D22ReadModelAdapterSupport.IsStableId(slot.SlotId) || !seen.Add(slot.SlotId))
                    {
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_slot", "d22.save.slot_invalid", D22ReadModelAdapterSupport.Key("save", "partial"),
                            "d22.save.slot_invalid", "SaveSystem", "save.slots", false));
                        continue;
                    }
                    if (slot.SchemaVersion != D22SaveSlotOwnerSnapshot.CurrentSchemaVersion)
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_slot_schema", "d22.save.slot_schema_invalid", D22ReadModelAdapterSupport.Key("save", "partial"),
                            "d22.save.slot_schema_invalid", "SaveSystem", "save.slots", false));
                    if (slot.SaveRevision < 0 || slot.WorldDay < 0)
                        issues.Add(D22ReadModelAdapterSupport.Issue(
                            "invalid_slot_revision", "d22.save.slot_revision_invalid", D22ReadModelAdapterSupport.Key("save", "partial"),
                            "d22.save.slot_revision_invalid", "SaveSystem", "save.slots", false));
                    D22ReadModelAdapterSupport.ValidateSourceCode(slot.ErrorCode, "SaveSystem", "save.slots", issues);
                    D22ReadModelAdapterSupport.ValidateSourceCode(slot.ReasonCode, "SaveSystem", "save.slots", issues);
                    D22ReadModelAdapterSupport.ValidateSourceKey(slot.LocalizationKey, "SaveSystem", "save.slots", issues);
                    slots.Add(new D22SaveSlotReadModel(slot));
                }
            }

            D22SaveMigrationReadModel migration = null;
            D22AdapterOutcomeReadModel operation = null;
            string sourceError = string.Empty;
            string sourceReason = snapshot.DisabledReasonCode ?? string.Empty;
            string sourceLoc = snapshot.DisabledLocalizationKey ?? string.Empty;
            if (snapshot.MigrationResult != null)
            {
                migration = new D22SaveMigrationReadModel(snapshot.MigrationResult);
                sourceError = snapshot.MigrationResult.ErrorCode ?? string.Empty;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? sourceError : sourceReason;
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceError, "SaveMigration", "save.migration", issues);
                operation = D22ReadModelAdapterSupport.Outcome(
                    "save", "SaveMigration", true, snapshot.MigrationResult.Succeeded,
                    false, sourceError, sourceReason, sourceLoc,
                    snapshot.MigrationResult.Succeeded ? string.Empty :
                        (string.IsNullOrEmpty(sourceReason) ? "d22.save.migration_failed" : sourceReason), issues);
            }
            else if (loadState != null && !string.IsNullOrEmpty(loadState.ErrorCode))
            {
                sourceError = loadState.ErrorCode;
                sourceReason = string.IsNullOrEmpty(sourceReason) ? loadState.ReasonCode : sourceReason;
                operation = D22ReadModelAdapterSupport.Outcome(
                    "save", "SaveSystem", true, loadState.Succeeded, false,
                    sourceError, sourceReason, loadState.LocalizationKey,
                    loadState.Succeeded ? string.Empty : "d22.save.load_state_failed", issues);
            }
            if (!string.IsNullOrEmpty(sourceReason))
            {
                D22ReadModelAdapterSupport.ValidateSourceCode(sourceReason, "D22SaveOwnerSnapshot", "save.snapshot", issues);
                D22ReadModelAdapterSupport.ValidateSourceKey(sourceLoc, "D22SaveOwnerSnapshot", "save.snapshot", issues);
            }

            bool invalid = snapshot.SchemaVersion != D22SaveOwnerSnapshot.CurrentSchemaVersion;
            bool blocked = snapshot.LoadState == null && snapshot.Slots == null && snapshot.MigrationResult == null;
            var status = D22ReadModelAdapterSupport.ResolveStatus(invalid, blocked, issues.Count > 0, issues);
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "save", "SaveSystem", snapshot.SourceSchemaVersion, snapshot.SaveRevision, snapshot.WorldDay, status,
                sourceError, sourceError, sourceReason,
                string.IsNullOrEmpty(sourceReason) ? string.Empty : sourceReason,
                sourceLoc, MergeMissing(missing, snapshot.MissingModuleIds), issues);
            return new D22SaveReadModel(metadata, snapshot.WorldDay, snapshot.SaveRevision, loadState, migration, slots, operation);
        }

        public static D22SaveReadModel Adapt(D22SaveOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        public static D22SaveReadModel FromSnapshot(D22SaveOwnerSnapshot snapshot)
        {
            return Project(snapshot);
        }

        private static D22SaveReadModel Empty(
            List<D22ReadModelAdapterIssue> issues,
            IEnumerable<string> missing)
        {
            var metadata = D22ReadModelAdapterSupport.BuildMetadata(
                "save", "SaveSystem", 0, 0, 0, D22ReadModelAdapterStatus.Blocked,
                "d22.save.source_missing", string.Empty, "d22.save.source_missing",
                "d22.save.source_missing", string.Empty, missing, issues);
            return new D22SaveReadModel(metadata, 0, 0, null, null,
                new List<D22SaveSlotReadModel>(), null);
        }

        private static List<string> MergeMissing(List<string> current, IEnumerable<string> extra)
        {
            var result = new List<string>();
            if (current != null) result.AddRange(current);
            if (extra != null) result.AddRange(extra);
            return D22ReadModelAdapterSupport.SortedDistinct(result);
        }

        private static int CompareSlots(D22SaveSlotReadModel left, D22SaveSlotReadModel right)
        {
            return D22ReadModelAdapterSupport.CompareNullable(left == null ? null : left.SlotId,
                right == null ? null : right.SlotId);
        }
    }
}
