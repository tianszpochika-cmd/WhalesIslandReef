// ============================================================
// D22ViewModelComposer —— D22-L27 纯逻辑 ViewModel 组合器
//
// Composer 只组合 D22 已有快照与上游公开 adapter 投影。缺少模块、版本不匹配
// 或 adapter 失败会保留稳定 reason/localization key，不会复制或修改业务事实。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Whalebone.Systems.D22
{
    /// <summary>上游读模型在 ViewModel 中的可解释状态。</summary>
    public enum D22ViewModelModuleStatus
    {
        Missing = 0,
        Available = 1,
        Blocked = 2,
        VersionMismatch = 3,
        Invalid = 4,
        Unknown = 5,
    }

    /// <summary>供 D23/Runtime 共同使用的稳定上游模块 ID。</summary>
    public static class D22ViewModelModuleIds
    {
        public const string Market = "module.market";
        public const string Port = "module.port";
        public const string Ship = "module.ship";
        public const string Inventory = "module.inventory";
        public const string Crew = "module.crew";
        public const string Quest = "module.quest";
        public const string Save = "module.save";

        public static IReadOnlyList<string> GetOrderedIds()
        {
            return new ReadOnlyCollection<string>(new List<string>
            {
                Market, Port, Ship, Inventory, Crew, Quest, Save,
            });
        }

        /// <summary>将文档允许的短名归一化为稳定 module ID。</summary>
        public static string Normalize(string moduleId)
        {
            switch (moduleId ?? string.Empty)
            {
                case "market": return Market;
                case "port": return Port;
                case "ship": return Ship;
                case "inventory": return Inventory;
                case "crew": return Crew;
                case "quest": return Quest;
                case "save": return Save;
                default: return moduleId ?? string.Empty;
            }
        }
    }

    /// <summary>上游 adapter 输出的稳定标量投影；不包含可变 owner 引用。</summary>
    [Serializable]
    public sealed class D22ViewModelField
    {
        public string Key = string.Empty;
        public string Value = string.Empty;

        public D22ViewModelField Clone()
        {
            return new D22ViewModelField
            {
                Key = Key ?? string.Empty,
                Value = Value ?? string.Empty,
            };
        }
    }

    /// <summary>上游公开 read-model adapter 的结果快照。</summary>
    [Serializable]
    public sealed class D22ViewModelSourceSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string ModuleId = string.Empty;
        public string OwnerSystem = string.Empty;
        public bool Available = true;
        public string ReasonCode = string.Empty;
        public string LocalizationKey = string.Empty;
        public string Fingerprint = string.Empty;
        public List<D22ViewModelField> Fields = new List<D22ViewModelField>();

        public D22ViewModelSourceSnapshot Clone()
        {
            var result = new D22ViewModelSourceSnapshot
            {
                SchemaVersion = SchemaVersion,
                ModuleId = ModuleId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                Available = Available,
                ReasonCode = ReasonCode ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
            };
            if (Fields != null)
            {
                foreach (var field in Fields)
                    result.Fields.Add(field == null ? null : field.Clone());
            }
            return result;
        }
    }

    /// <summary>上游公开只读快照的注入边界。</summary>
    public interface ID22ViewModelSourceAdapter
    {
        string ModuleId { get; }
        int ExpectedSchemaVersion { get; }
        D22ViewModelSourceSnapshot ReadSnapshot();
    }

    /// <summary>便于 Runtime/测试接线的无状态公开 adapter 注册。</summary>
    public sealed class D22ViewModelSourceRegistration : ID22ViewModelSourceAdapter
    {
        private readonly string moduleId;
        private readonly int expectedSchemaVersion;
        private readonly D22ViewModelSourceSnapshotProvider provider;

        public D22ViewModelSourceRegistration(
            string moduleId,
            D22ViewModelSourceSnapshotProvider provider,
            int expectedSchemaVersion = D22ViewModelSourceSnapshot.CurrentSchemaVersion)
        {
            this.moduleId = moduleId ?? string.Empty;
            this.provider = provider;
            this.expectedSchemaVersion = expectedSchemaVersion;
        }

        /// <summary>以一个公开快照创建无状态 adapter；快照仍会在 Compose 时复制。</summary>
        public static D22ViewModelSourceRegistration FromSnapshot(
            string moduleId,
            D22ViewModelSourceSnapshot snapshot,
            int expectedSchemaVersion = D22ViewModelSourceSnapshot.CurrentSchemaVersion)
        {
            return new D22ViewModelSourceRegistration(
                moduleId,
                delegate { return snapshot; },
                expectedSchemaVersion);
        }

        public string ModuleId { get { return moduleId; } }

        public int ExpectedSchemaVersion
        {
            get { return expectedSchemaVersion; }
        }

        public D22ViewModelSourceSnapshot ReadSnapshot()
        {
            return provider == null ? null : provider();
        }
    }

    /// <summary>不持有 owner 状态的只读快照提供委托。</summary>
    public delegate D22ViewModelSourceSnapshot D22ViewModelSourceSnapshotProvider();

    /// <summary>ViewModel 组合输入；每个 D22 快照都会在组合时复制。</summary>
    [Serializable]
    public sealed class D22ViewModelCompositionInput
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D22PanelTransitionSnapshot PanelState;
        public D22FocusSnapshot FocusState;
        public D22PauseReadModel PauseState;
        public D22NotificationQueueSnapshot NotificationState;
        public D22LoadStateReadModel LoadState;
        public D22AccessibilityReadModel AccessibilityState;
        public List<ID22ViewModelSourceAdapter> Sources =
            new List<ID22ViewModelSourceAdapter>();

        public D22ViewModelCompositionInput Clone()
        {
            var result = new D22ViewModelCompositionInput
            {
                SchemaVersion = SchemaVersion,
                PanelState = PanelState == null ? null : PanelState.Clone(),
                FocusState = FocusState == null ? null : FocusState.Clone(),
                PauseState = PauseState == null ? null : PauseState.Clone(),
                NotificationState = NotificationState == null ? null : NotificationState.Clone(),
                LoadState = LoadState == null ? null : LoadState.Clone(),
                AccessibilityState = AccessibilityState == null ? null : AccessibilityState.Clone(),
            };
            if (Sources != null) result.Sources.AddRange(Sources);
            return result;
        }
    }

    /// <summary>一个上游模块的稳定 ViewModel 投影。</summary>
    [Serializable]
    public sealed class D22ViewModelModuleState
    {
        public string ModuleId = string.Empty;
        public string OwnerSystem = string.Empty;
        public int SchemaVersion;
        public int ExpectedSchemaVersion;
        public D22ViewModelModuleStatus Status = D22ViewModelModuleStatus.Missing;
        public bool Available;
        public string ReasonCode = string.Empty;
        public string LocalizationKey = string.Empty;
        public string Fingerprint = string.Empty;
        public List<D22ViewModelField> Fields = new List<D22ViewModelField>();

        public D22ViewModelModuleState Clone()
        {
            var result = new D22ViewModelModuleState
            {
                ModuleId = ModuleId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                SchemaVersion = SchemaVersion,
                ExpectedSchemaVersion = ExpectedSchemaVersion,
                Status = Status,
                Available = Available,
                ReasonCode = ReasonCode ?? string.Empty,
                LocalizationKey = LocalizationKey ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
            };
            if (Fields != null)
            {
                foreach (var field in Fields)
                    result.Fields.Add(field == null ? null : field.Clone());
            }
            return result;
        }
    }

    /// <summary>D22-L27 稳定组合结果。</summary>
    [Serializable]
    public sealed class D22ViewModel
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool IsValid = true;
        public bool IsDegraded;
        public string ActiveScreenId = string.Empty;
        public string ActivePanelId = string.Empty;
        public string FocusedControlId = D22FocusControlIds.None;
        public bool IsPaused;
        public bool IsUnrecoverableError;
        public D22PanelTransitionSnapshot PanelState = new D22PanelTransitionSnapshot();
        public D22FocusSnapshot FocusState = new D22FocusSnapshot();
        public D22PauseReadModel PauseState = new D22PauseReadModel();
        public D22NotificationQueueSnapshot NotificationState = new D22NotificationQueueSnapshot();
        public D22LoadStateReadModel LoadState = new D22LoadStateReadModel();
        public D22AccessibilityReadModel AccessibilityState = new D22AccessibilityReadModel();
        public List<D22ViewModelModuleState> Modules = new List<D22ViewModelModuleState>();
        public List<string> ReasonCodes = new List<string>();
        public List<string> LocalizationKeys = new List<string>();
        public string CompositionFingerprint = string.Empty;

        /// <summary>按固定模块顺序查找投影。</summary>
        public bool TryGetModule(string moduleId, out D22ViewModelModuleState module)
        {
            string normalized = D22ViewModelModuleIds.Normalize(moduleId);
            if (Modules != null)
            {
                foreach (var candidate in Modules)
                {
                    if (candidate != null && string.Equals(candidate.ModuleId, normalized,
                        StringComparison.Ordinal))
                    {
                        module = candidate.Clone();
                        return true;
                    }
                }
            }
            module = null;
            return false;
        }

        public D22ViewModel Clone()
        {
            var result = new D22ViewModel
            {
                SchemaVersion = SchemaVersion,
                IsValid = IsValid,
                IsDegraded = IsDegraded,
                ActiveScreenId = ActiveScreenId ?? string.Empty,
                ActivePanelId = ActivePanelId ?? string.Empty,
                FocusedControlId = FocusedControlId ?? D22FocusControlIds.None,
                IsPaused = IsPaused,
                IsUnrecoverableError = IsUnrecoverableError,
                PanelState = PanelState == null ? null : PanelState.Clone(),
                FocusState = FocusState == null ? null : FocusState.Clone(),
                PauseState = PauseState == null ? null : PauseState.Clone(),
                NotificationState = NotificationState == null ? null : NotificationState.Clone(),
                LoadState = LoadState == null ? null : LoadState.Clone(),
                AccessibilityState = AccessibilityState == null ? null : AccessibilityState.Clone(),
                CompositionFingerprint = CompositionFingerprint ?? string.Empty,
            };
            if (Modules != null)
            {
                foreach (var module in Modules)
                    result.Modules.Add(module == null ? null : module.Clone());
            }
            if (ReasonCodes != null) result.ReasonCodes.AddRange(ReasonCodes);
            if (LocalizationKeys != null) result.LocalizationKeys.AddRange(LocalizationKeys);
            return result;
        }
    }

    /// <summary>
    /// 组合 D22 自有读模型和上游公开 adapter。实例本身只保存 adapter 委托，
    /// 不缓存任何 owner snapshot；每次 Compose 都重新读取并复制输入。
    /// </summary>
    public sealed class D22ViewModelComposer
    {
        public const int CurrentSchemaVersion = D22ViewModel.CurrentSchemaVersion;
        public const string OwnerSystem = "D22ViewModelComposer";

        private readonly List<ID22ViewModelSourceAdapter> configuredSources =
            new List<ID22ViewModelSourceAdapter>();

        public D22ViewModelComposer() { }

        public D22ViewModelComposer(IEnumerable<ID22ViewModelSourceAdapter> sources)
        {
            if (sources == null) return;
            foreach (var source in sources)
                if (source != null) configuredSources.Add(source);
        }

        /// <summary>组合一次新的防御性 ViewModel。</summary>
        public D22ViewModel Compose(D22ViewModelCompositionInput input)
        {
            var model = new D22ViewModel();
            if (input == null)
            {
                model.IsValid = false;
                AddIssue(model, "d22.viewmodel.missing_input", "ui.d22.viewmodel.missing_input");
                FinalizeFingerprint(model);
                return model;
            }
            if (input.SchemaVersion != D22ViewModelCompositionInput.CurrentSchemaVersion)
            {
                model.IsValid = false;
                AddIssue(model, "d22.viewmodel.schema_mismatch", "ui.d22.viewmodel.schema_mismatch");
            }

            model.PanelState = input.PanelState == null
                ? new D22PanelTransitionSnapshot()
                : input.PanelState.Clone();
            model.FocusState = input.FocusState == null
                ? new D22FocusSnapshot()
                : input.FocusState.Clone();
            model.PauseState = input.PauseState == null
                ? new D22PauseReadModel()
                : input.PauseState.Clone();
            model.NotificationState = input.NotificationState == null
                ? new D22NotificationQueueSnapshot()
                : input.NotificationState.Clone();
            model.LoadState = input.LoadState == null
                ? new D22LoadStateReadModel()
                : input.LoadState.Clone();
            model.AccessibilityState = input.AccessibilityState == null
                ? new D22AccessibilityReadModel()
                : input.AccessibilityState.Clone();

            if (input.PanelState == null)
                AddIssue(model, "d22.viewmodel.panel_state_missing", "ui.d22.viewmodel.panel_state_missing");
            if (input.FocusState == null)
                AddIssue(model, "d22.viewmodel.focus_state_missing", "ui.d22.viewmodel.focus_state_missing");
            if (input.PauseState == null)
                AddIssue(model, "d22.viewmodel.pause_state_missing", "ui.d22.viewmodel.pause_state_missing");
            if (input.NotificationState == null)
                AddIssue(model, "d22.viewmodel.notification_state_missing", "ui.d22.viewmodel.notification_state_missing");
            if (input.LoadState == null)
                AddIssue(model, "d22.viewmodel.load_state_missing", "ui.d22.viewmodel.load_state_missing");
            if (input.AccessibilityState == null)
                AddIssue(model, "d22.viewmodel.accessibility_state_missing", "ui.d22.viewmodel.accessibility_state_missing");

            model.ActiveScreenId = model.PanelState.ActiveScreenId ?? string.Empty;
            model.ActivePanelId = model.PanelState.ActivePanelId ?? string.Empty;
            model.FocusedControlId = model.FocusState.FocusedControlId ?? D22FocusControlIds.None;
            model.IsPaused = model.PauseState.IsPaused;
            model.IsUnrecoverableError = model.PanelState.IsUnrecoverableError;

            List<ID22ViewModelSourceAdapter> sources = input.Sources != null && input.Sources.Count > 0
                ? input.Sources
                : configuredSources;
            ComposeModules(model, sources);
            AddOwnerStateIssues(model);
            model.IsDegraded = model.ReasonCodes.Count > 0;
            FinalizeFingerprint(model);
            return model;
        }

        /// <summary>无状态静态便捷入口。</summary>
        public static D22ViewModel ComposeSnapshot(D22ViewModelCompositionInput input)
        {
            return new D22ViewModelComposer().Compose(input);
        }

        /// <summary>ComposeSnapshot 的语义别名。</summary>
        public static D22ViewModel Build(D22ViewModelCompositionInput input)
        {
            return ComposeSnapshot(input);
        }

        private static void ComposeModules(
            D22ViewModel model,
            IList<ID22ViewModelSourceAdapter> sources)
        {
            var byId = new Dictionary<string, List<ID22ViewModelSourceAdapter>>(
                StringComparer.Ordinal);
            if (sources != null)
            {
                foreach (var source in sources)
                {
                    if (source == null) continue;
                    string id = D22ViewModelModuleIds.Normalize(source.ModuleId);
                    List<ID22ViewModelSourceAdapter> bucket;
                    if (!byId.TryGetValue(id, out bucket))
                    {
                        bucket = new List<ID22ViewModelSourceAdapter>();
                        byId.Add(id, bucket);
                    }
                    bucket.Add(source);
                }
            }

            foreach (var requiredId in D22ViewModelModuleIds.GetOrderedIds())
            {
                List<ID22ViewModelSourceAdapter> bucket;
                if (!byId.TryGetValue(requiredId, out bucket) || bucket.Count == 0)
                {
                    model.Modules.Add(MissingModule(requiredId));
                    AddIssue(model, "d22.viewmodel.module_missing:" + requiredId,
                        "ui.d22.viewmodel.module_missing");
                    continue;
                }
                if (bucket.Count > 1)
                {
                    model.Modules.Add(InvalidModule(requiredId, "d22.viewmodel.duplicate_module"));
                    AddIssue(model, "d22.viewmodel.duplicate_module:" + requiredId,
                        "ui.d22.viewmodel.duplicate_module");
                    continue;
                }
                model.Modules.Add(ReadModule(bucket[0], requiredId, model));
            }

            var unknownIds = new List<string>();
            foreach (var pair in byId)
                if (!IsRequiredModule(pair.Key)) unknownIds.Add(pair.Key);
            unknownIds.Sort(StringComparer.Ordinal);
            foreach (var unknownId in unknownIds)
            {
                model.Modules.Add(InvalidModule(unknownId, "d22.viewmodel.unknown_module"));
                AddIssue(model, "d22.viewmodel.unknown_module:" + unknownId,
                    "ui.d22.viewmodel.unknown_module");
            }
        }

        private static D22ViewModelModuleState ReadModule(
            ID22ViewModelSourceAdapter adapter,
            string requiredId,
            D22ViewModel model)
        {
            int expected = adapter.ExpectedSchemaVersion <= 0
                ? D22ViewModelSourceSnapshot.CurrentSchemaVersion
                : adapter.ExpectedSchemaVersion;
            D22ViewModelSourceSnapshot snapshot;
            try
            {
                snapshot = adapter.ReadSnapshot();
            }
            catch (Exception)
            {
                AddIssue(model, "d22.viewmodel.adapter_failed:" + requiredId,
                    "ui.d22.viewmodel.adapter_failed");
                return new D22ViewModelModuleState
                {
                    ModuleId = requiredId,
                    ExpectedSchemaVersion = expected,
                    Status = D22ViewModelModuleStatus.Blocked,
                    Available = false,
                    ReasonCode = "d22.viewmodel.adapter_failed",
                    LocalizationKey = "ui.d22.viewmodel.adapter_failed",
                };
            }
            if (snapshot == null)
            {
                AddIssue(model, "d22.viewmodel.adapter_unavailable:" + requiredId,
                    "ui.d22.viewmodel.adapter_unavailable");
                return new D22ViewModelModuleState
                {
                    ModuleId = requiredId,
                    ExpectedSchemaVersion = expected,
                    Status = D22ViewModelModuleStatus.Blocked,
                    Available = false,
                    ReasonCode = "d22.viewmodel.adapter_unavailable",
                    LocalizationKey = "ui.d22.viewmodel.adapter_unavailable",
                };
            }

            D22ViewModelSourceSnapshot copy = snapshot.Clone();
            string suppliedId = D22ViewModelModuleIds.Normalize(copy.ModuleId);
            var module = new D22ViewModelModuleState
            {
                ModuleId = requiredId,
                OwnerSystem = copy.OwnerSystem ?? string.Empty,
                SchemaVersion = copy.SchemaVersion,
                ExpectedSchemaVersion = expected,
                Available = copy.Available,
                ReasonCode = copy.ReasonCode ?? string.Empty,
                LocalizationKey = copy.LocalizationKey ?? string.Empty,
                Fingerprint = copy.Fingerprint ?? string.Empty,
            };
            CopyAndSortFields(module.Fields, copy.Fields);
            if (!string.Equals(suppliedId, requiredId, StringComparison.Ordinal))
            {
                module.Status = D22ViewModelModuleStatus.Invalid;
                module.Available = false;
                module.ReasonCode = "d22.viewmodel.module_id_mismatch";
                module.LocalizationKey = "ui.d22.viewmodel.module_id_mismatch";
                AddIssue(model, "d22.viewmodel.module_id_mismatch:" + requiredId,
                    "ui.d22.viewmodel.module_id_mismatch");
                return module;
            }
            if (copy.SchemaVersion != expected)
            {
                module.Status = D22ViewModelModuleStatus.VersionMismatch;
                module.Available = false;
                module.ReasonCode = "d22.viewmodel.version_mismatch";
                module.LocalizationKey = "ui.d22.viewmodel.version_mismatch";
                AddIssue(model, "d22.viewmodel.version_mismatch:" + requiredId,
                    "ui.d22.viewmodel.version_mismatch");
                return module;
            }
            if (!copy.Available)
            {
                module.Status = D22ViewModelModuleStatus.Blocked;
                AddIssue(model,
                    string.IsNullOrEmpty(module.ReasonCode)
                        ? "d22.viewmodel.adapter_unavailable:" + requiredId
                        : module.ReasonCode + ":" + requiredId,
                    string.IsNullOrEmpty(module.LocalizationKey)
                        ? "ui.d22.viewmodel.adapter_unavailable"
                        : module.LocalizationKey);
                if (string.IsNullOrEmpty(module.ReasonCode))
                    module.ReasonCode = "d22.viewmodel.adapter_unavailable";
                if (string.IsNullOrEmpty(module.LocalizationKey))
                    module.LocalizationKey = "ui.d22.viewmodel.adapter_unavailable";
                return module;
            }
            module.Status = D22ViewModelModuleStatus.Available;
            module.Available = true;
            return module;
        }

        private static void AddOwnerStateIssues(D22ViewModel model)
        {
            if (model.LoadState != null && !model.LoadState.IsValid &&
                !string.IsNullOrEmpty(model.LoadState.ReasonCode))
                AddIssue(model, model.LoadState.ReasonCode,
                    string.IsNullOrEmpty(model.LoadState.StatusLocalizationKey)
                        ? "ui.d22.viewmodel.load_state_invalid"
                        : model.LoadState.StatusLocalizationKey);
            if (model.AccessibilityState != null && !model.AccessibilityState.IsValid &&
                !string.IsNullOrEmpty(model.AccessibilityState.ReasonCode))
                AddIssue(model, model.AccessibilityState.ReasonCode,
                    "ui.d22.viewmodel.accessibility_invalid");
        }

        private static D22ViewModelModuleState MissingModule(string moduleId)
        {
            return new D22ViewModelModuleState
            {
                ModuleId = moduleId,
                Status = D22ViewModelModuleStatus.Missing,
                Available = false,
                ReasonCode = "d22.viewmodel.module_missing",
                LocalizationKey = "ui.d22.viewmodel.module_missing",
            };
        }

        private static D22ViewModelModuleState InvalidModule(string moduleId, string reasonCode)
        {
            return new D22ViewModelModuleState
            {
                ModuleId = moduleId ?? string.Empty,
                Status = D22ViewModelModuleStatus.Invalid,
                Available = false,
                ReasonCode = reasonCode ?? "d22.viewmodel.invalid_module",
                LocalizationKey = "ui.d22.viewmodel.invalid_module",
            };
        }

        private static bool IsRequiredModule(string moduleId)
        {
            foreach (var required in D22ViewModelModuleIds.GetOrderedIds())
                if (string.Equals(required, moduleId, StringComparison.Ordinal)) return true;
            return false;
        }

        private static void CopyAndSortFields(
            IList<D22ViewModelField> target,
            IList<D22ViewModelField> fields)
        {
            if (target == null || fields == null) return;
            foreach (var field in fields)
                if (field != null) target.Add(field.Clone());
            var list = target as List<D22ViewModelField>;
            if (list != null)
            {
                list.Sort(delegate (D22ViewModelField left, D22ViewModelField right)
                {
                    int key = string.Compare(left.Key ?? string.Empty, right.Key ?? string.Empty,
                        StringComparison.Ordinal);
                    return key != 0
                        ? key
                        : string.Compare(left.Value ?? string.Empty, right.Value ?? string.Empty,
                            StringComparison.Ordinal);
                });
            }
        }

        private static void AddIssue(D22ViewModel model, string reasonCode, string localizationKey)
        {
            string reason = reasonCode ?? string.Empty;
            string key = localizationKey ?? string.Empty;
            if (reason.Length > 0 && !model.ReasonCodes.Contains(reason))
                model.ReasonCodes.Add(reason);
            if (key.Length > 0 && !model.LocalizationKeys.Contains(key))
                model.LocalizationKeys.Add(key);
        }

        private static void FinalizeFingerprint(D22ViewModel model)
        {
            var builder = new StringBuilder();
            Append(builder, model.SchemaVersion);
            Append(builder, model.IsValid);
            Append(builder, model.IsDegraded);
            Append(builder, model.ActiveScreenId);
            Append(builder, model.ActivePanelId);
            Append(builder, model.FocusedControlId);
            Append(builder, model.IsPaused);
            Append(builder, model.IsUnrecoverableError);
            foreach (var module in model.Modules)
            {
                if (module == null) continue;
                Append(builder, module.ModuleId);
                Append(builder, module.OwnerSystem);
                Append(builder, module.Status.ToString());
                Append(builder, module.SchemaVersion);
                Append(builder, module.ExpectedSchemaVersion);
                Append(builder, module.Available);
                Append(builder, module.ReasonCode);
                Append(builder, module.LocalizationKey);
                Append(builder, module.Fingerprint);
                foreach (var field in module.Fields)
                {
                    if (field == null) continue;
                    Append(builder, field.Key);
                    Append(builder, field.Value);
                }
            }
            AppendPanelState(builder, model.PanelState);
            AppendFocusState(builder, model.FocusState);
            AppendPauseState(builder, model.PauseState);
            AppendNotificationState(builder, model.NotificationState);
            AppendLoadState(builder, model.LoadState);
            AppendAccessibilityState(builder, model.AccessibilityState);
            foreach (var reason in model.ReasonCodes) Append(builder, reason);
            foreach (var key in model.LocalizationKeys) Append(builder, key);
            model.CompositionFingerprint = builder.ToString();
        }

        private static void AppendPanelState(
            StringBuilder builder,
            D22PanelTransitionSnapshot state)
        {
            if (state == null)
            {
                Append(builder, "panel:null");
                return;
            }
            Append(builder, "panel");
            Append(builder, state.SchemaVersion);
            Append(builder, state.ActiveScreenId);
            Append(builder, state.ActivePanelId);
            Append(builder, state.IsUnrecoverableError);
            Append(builder, state.ErrorCode);
            Append(builder, state.LastOperation.ToString());
            Append(builder, state.LastReasonCode);
            if (state.OpenPanels != null)
            {
                foreach (var panel in state.OpenPanels)
                {
                    if (panel == null) continue;
                    Append(builder, panel.PanelId);
                    Append(builder, panel.ScreenId);
                    Append(builder, panel.OwnerId);
                    Append(builder, panel.ParentPanelId);
                    Append(builder, panel.IsOpen);
                }
            }
        }

        private static void AppendFocusState(
            StringBuilder builder,
            D22FocusSnapshot state)
        {
            if (state == null)
            {
                Append(builder, "focus:null");
                return;
            }
            Append(builder, "focus");
            Append(builder, state.SchemaVersion);
            Append(builder, state.ActiveScreenId);
            Append(builder, state.ActivePanelId);
            Append(builder, state.ActiveModalId);
            Append(builder, state.FocusedControlId);
            Append(builder, state.ReturnFocusToken);
            Append(builder, state.InputMode.ToString());
            if (state.ModalStack != null)
            {
                foreach (var modal in state.ModalStack)
                {
                    if (modal == null) continue;
                    Append(builder, modal.ModalId);
                    Append(builder, modal.OwnerId);
                    Append(builder, modal.ModalKind.ToString());
                    Append(builder, modal.ParentModalId);
                    Append(builder, modal.FocusedControlId);
                    Append(builder, modal.FallbackFocusControlId);
                    if (modal.ReturnFocus != null)
                        Append(builder, modal.ReturnFocus.ToStableString());
                }
            }
        }

        private static void AppendPauseState(
            StringBuilder builder,
            D22PauseReadModel state)
        {
            if (state == null)
            {
                Append(builder, "pause:null");
                return;
            }
            Append(builder, "pause");
            Append(builder, state.SchemaVersion);
            Append(builder, state.State.ToString());
            Append(builder, state.IsPaused);
            Append(builder, state.CanPause);
            Append(builder, state.CanResume);
            Append(builder, state.LastIntent.ToString());
            Append(builder, state.FailureCode.ToString());
            Append(builder, state.ReasonCode);
            Append(builder, state.FeedbackCode);
            Append(builder, state.CorrelationId);
            Append(builder, state.ContextId);
            Append(builder, state.UnpausableReasonCode);
            Append(builder, state.WorldDay);
            Append(builder, state.SaveRevision);
        }

        private static void AppendNotificationState(
            StringBuilder builder,
            D22NotificationQueueSnapshot state)
        {
            if (state == null)
            {
                Append(builder, "notification:null");
                return;
            }
            Append(builder, "notification");
            Append(builder, state.SchemaVersion);
            Append(builder, state.NextSequence);
            if (state.Notifications != null)
            {
                foreach (var notification in state.Notifications)
                {
                    if (notification == null) continue;
                    Append(builder, notification.EventId);
                    Append(builder, notification.DeduplicationKey);
                    Append(builder, notification.CorrelationId);
                    Append(builder, notification.SourceOwner);
                    Append(builder, notification.LocalizationKey);
                    Append(builder, notification.TitleLocalizationKey);
                    Append(builder, notification.BodyLocalizationKey);
                    Append(builder, notification.Priority.ToString());
                    Append(builder, notification.CreatedWorldDay);
                    Append(builder, notification.ExpiresWorldDay);
                    Append(builder, notification.RequiresAcknowledgement);
                    Append(builder, notification.Persistence.ToString());
                    Append(builder, notification.State.ToString());
                    Append(builder, notification.Sequence);
                    Append(builder, notification.AcknowledgedWorldDay);
                }
            }
            if (state.ProcessedEvents != null)
            {
                foreach (var processed in state.ProcessedEvents)
                {
                    if (processed == null) continue;
                    Append(builder, processed.EventId);
                    Append(builder, processed.Fingerprint);
                    Append(builder, processed.DeduplicationKey);
                    Append(builder, processed.ExistingNotificationId);
                }
            }
        }

        private static void AppendLoadState(
            StringBuilder builder,
            D22LoadStateReadModel state)
        {
            if (state == null)
            {
                Append(builder, "load:null");
                return;
            }
            Append(builder, "load");
            Append(builder, state.SchemaVersion);
            Append(builder, state.IsValid);
            Append(builder, state.Operation.ToString());
            Append(builder, state.State.ToString());
            Append(builder, state.SlotId);
            Append(builder, state.SaveRevision);
            Append(builder, state.SourceRevision);
            Append(builder, state.TargetRevision);
            Append(builder, state.ProgressPercent);
            Append(builder, state.IsBusy);
            Append(builder, state.IsTerminal);
            Append(builder, state.Succeeded);
            Append(builder, state.CanRetry);
            Append(builder, state.CanRecover);
            Append(builder, state.HasMissingModules);
            Append(builder, state.FailureCode.ToString());
            Append(builder, state.ErrorCode);
            Append(builder, state.ReasonCode);
            Append(builder, state.StatusLocalizationKey);
            Append(builder, state.FeedbackCode);
            if (state.Modules != null)
            {
                foreach (var module in state.Modules)
                {
                    if (module == null) continue;
                    Append(builder, module.ModuleId);
                    Append(builder, module.Status.ToString());
                    Append(builder, module.Required);
                    Append(builder, module.ModuleSchemaVersion);
                    Append(builder, module.ErrorCode);
                }
            }
            if (state.MissingModuleIds != null)
                foreach (var moduleId in state.MissingModuleIds) Append(builder, moduleId);
        }

        private static void AppendAccessibilityState(
            StringBuilder builder,
            D22AccessibilityReadModel state)
        {
            if (state == null)
            {
                Append(builder, "accessibility:null");
                return;
            }
            Append(builder, "accessibility");
            Append(builder, state.SchemaVersion);
            Append(builder, state.IsValid);
            Append(builder, state.Locale);
            Append(builder, state.TextScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Append(builder, state.HighContrast);
            Append(builder, state.ColorVisionMode.ToString());
            Append(builder, state.ReducedMotion);
            Append(builder, state.LowStimulus);
            Append(builder, state.Subtitles);
            Append(builder, state.SubtitleSpeakerLabels);
            Append(builder, state.SensitiveWarnings);
            Append(builder, state.ReasonCode);
            if (state.ValidationCodes != null)
                foreach (var code in state.ValidationCodes) Append(builder, code);
        }

        private static void Append(StringBuilder builder, string value)
        {
            value = value ?? string.Empty;
            builder.Append(value.Length).Append(':').Append(value).Append('|');
        }

        private static void Append(StringBuilder builder, int value)
        {
            Append(builder, value.ToString());
        }

        private static void Append(StringBuilder builder, long value)
        {
            Append(builder, value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        private static void Append(StringBuilder builder, bool value)
        {
            Append(builder, value ? "1" : "0");
        }
    }
}
