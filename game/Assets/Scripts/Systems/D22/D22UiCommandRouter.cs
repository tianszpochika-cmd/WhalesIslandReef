// ============================================================
// D22UiCommandRouter —— D22-L26 UI 命令路由（纯 C#）
//
// Router 只拥有 UI 命令的校验、关联、反馈和幂等回执。业务事实仍由
// 注入的公开 owner handler 裁决；没有 owner 时必须显式 Blocked。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>公开 owner handler 返回的业务命令生命周期。</summary>
    public enum D22UiOwnerCommandOutcome
    {
        Pending = 0,
        Succeeded = 1,
        Rejected = 2,
        Blocked = 3,
        Cancelled = 4,
    }

    /// <summary>注入到 D22 router 的纯公开 owner 处理委托。</summary>
    public delegate D22UiOwnerCommandResult D22UiOwnerCommandHandler(D22UiCommandRequest request);

    /// <summary>
    /// 业务 owner 的结构化回执。该 DTO 只描述 owner 已返回的结果，不能由 UI
    /// 直接构造来改变业务事实；生产接线应把它作为公开 owner endpoint 的回执。
    /// </summary>
    [Serializable]
    public sealed class D22UiOwnerCommandResult
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D22UiOwnerCommandOutcome Outcome = D22UiOwnerCommandOutcome.Pending;
        public string CommandId = string.Empty;
        public string CorrelationId = string.Empty;
        public string OwnerSystem = string.Empty;
        public string OwnerResultCode = string.Empty;
        public string FeedbackCode = string.Empty;
        public string DisabledReasonCode = string.Empty;
        public string ReasonCode = string.Empty;
        public bool BusinessStateChanged;

        /// <summary>返回防御性副本。</summary>
        public D22UiOwnerCommandResult Clone()
        {
            return new D22UiOwnerCommandResult
            {
                SchemaVersion = SchemaVersion,
                Outcome = Outcome,
                CommandId = CommandId ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                OwnerResultCode = OwnerResultCode ?? string.Empty,
                FeedbackCode = FeedbackCode ?? string.Empty,
                DisabledReasonCode = DisabledReasonCode ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                BusinessStateChanged = BusinessStateChanged,
            };
        }

        /// <summary>创建 owner 成功回执。</summary>
        public static D22UiOwnerCommandResult Succeeded(
            string ownerSystem,
            string ownerResultCode,
            bool businessStateChanged = false,
            string feedbackCode = "",
            string reasonCode = "")
        {
            return Create(ownerSystem, D22UiOwnerCommandOutcome.Succeeded, ownerResultCode,
                businessStateChanged, feedbackCode, reasonCode, string.Empty);
        }

        /// <summary>创建 owner 等待回执。</summary>
        public static D22UiOwnerCommandResult Pending(
            string ownerSystem,
            string ownerResultCode = "",
            string reasonCode = "")
        {
            return Create(ownerSystem, D22UiOwnerCommandOutcome.Pending, ownerResultCode,
                false, string.Empty, reasonCode, string.Empty);
        }

        /// <summary>创建 owner 拒绝回执。</summary>
        public static D22UiOwnerCommandResult Rejected(
            string ownerSystem,
            string ownerResultCode,
            string reasonCode = "",
            string disabledReasonCode = "")
        {
            return Create(ownerSystem, D22UiOwnerCommandOutcome.Rejected, ownerResultCode,
                false, string.Empty, reasonCode, disabledReasonCode);
        }

        /// <summary>创建 owner 缺失或暂不可用回执。</summary>
        public static D22UiOwnerCommandResult Blocked(
            string ownerSystem,
            string reasonCode = "",
            string disabledReasonCode = "")
        {
            return Create(ownerSystem, D22UiOwnerCommandOutcome.Blocked, "owner_unavailable",
                false, D22FeedbackCodes.OwnerUnavailable, reasonCode, disabledReasonCode);
        }

        /// <summary>创建 owner 取消回执。</summary>
        public static D22UiOwnerCommandResult Cancelled(
            string ownerSystem,
            string ownerResultCode = "",
            string reasonCode = "")
        {
            return Create(ownerSystem, D22UiOwnerCommandOutcome.Cancelled, ownerResultCode,
                false, string.Empty, reasonCode, string.Empty);
        }

        /// <summary>创建结构化 owner 回执。</summary>
        public static D22UiOwnerCommandResult Create(
            string ownerSystem,
            D22UiOwnerCommandOutcome outcome,
            string ownerResultCode = "",
            bool businessStateChanged = false,
            string feedbackCode = "",
            string reasonCode = "",
            string disabledReasonCode = "")
        {
            return new D22UiOwnerCommandResult
            {
                Outcome = outcome,
                OwnerSystem = ownerSystem ?? string.Empty,
                OwnerResultCode = ownerResultCode ?? string.Empty,
                FeedbackCode = feedbackCode ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
                DisabledReasonCode = disabledReasonCode ?? string.Empty,
                BusinessStateChanged = businessStateChanged,
            };
        }
    }

    /// <summary>一个 owner 的公开 handler 注册；不持有 owner 的内部状态。</summary>
    public sealed class D22UiCommandOwnerRegistration
    {
        public string OwnerSystem = string.Empty;
        public D22UiOwnerCommandHandler Handler;

        public D22UiCommandOwnerRegistration() { }

        public D22UiCommandOwnerRegistration(
            string ownerSystem,
            D22UiOwnerCommandHandler handler)
        {
            OwnerSystem = ownerSystem ?? string.Empty;
            Handler = handler;
        }

        public bool IsUsable
        {
            get { return !string.IsNullOrEmpty(OwnerSystem) && Handler != null; }
        }
    }

    /// <summary>Router 对一次命令的反馈组合结果。</summary>
    [Serializable]
    public sealed class D22UiCommandRouteResult
    {
        public D22UiCommandResult CommandResult = new D22UiCommandResult();
        public D22FeedbackRecord Feedback = new D22FeedbackRecord();
        public bool OwnerInvoked;

        /// <summary>兼容调用方使用 Result 命名。</summary>
        public D22UiCommandResult Result
        {
            get { return CommandResult; }
            set { CommandResult = value ?? new D22UiCommandResult(); }
        }

        public D22UiCommandRouteResult Clone()
        {
            return new D22UiCommandRouteResult
            {
                CommandResult = CommandResult == null ? new D22UiCommandResult() : CommandResult.Clone(),
                Feedback = Feedback == null ? new D22FeedbackRecord() : Feedback.Clone(),
                OwnerInvoked = OwnerInvoked,
            };
        }
    }

    /// <summary>可由 SaveSystem 合并的 D22 UI 命令幂等投影。</summary>
    [Serializable]
    public sealed class D22UiRoutedCommandRecord
    {
        public string CommandId = string.Empty;
        public string Fingerprint = string.Empty;
        public string CorrelationId = string.Empty;
        public D22UiCommandResult Result = new D22UiCommandResult();

        public D22UiRoutedCommandRecord Clone()
        {
            return new D22UiRoutedCommandRecord
            {
                CommandId = CommandId ?? string.Empty,
                Fingerprint = Fingerprint ?? string.Empty,
                CorrelationId = CorrelationId ?? string.Empty,
                Result = Result == null ? new D22UiCommandResult() : Result.Clone(),
            };
        }
    }

    /// <summary>Router UI 投影的版本化快照；不包含任何业务 owner snapshot。</summary>
    [Serializable]
    public sealed class D22UiCommandRouterSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public List<D22UiRoutedCommandRecord> ProcessedCommands =
            new List<D22UiRoutedCommandRecord>();

        /// <summary>兼容 Items 命名。</summary>
        public List<D22UiRoutedCommandRecord> Items
        {
            get { return ProcessedCommands; }
            set { ProcessedCommands = value ?? new List<D22UiRoutedCommandRecord>(); }
        }

        public D22UiCommandRouterSnapshot Clone()
        {
            var result = new D22UiCommandRouterSnapshot { SchemaVersion = SchemaVersion };
            if (ProcessedCommands != null)
            {
                foreach (var item in ProcessedCommands)
                    result.ProcessedCommands.Add(item == null ? null : item.Clone());
            }
            return result;
        }
    }

    /// <summary>Router UI 投影恢复结果。</summary>
    [Serializable]
    public sealed class D22UiCommandRouterRestoreResult
    {
        public bool Accepted;
        public bool Changed;
        public string ReasonCode = string.Empty;
        public D22UiCommandRouterSnapshot Snapshot = new D22UiCommandRouterSnapshot();

        public D22UiCommandRouterRestoreResult Clone()
        {
            return new D22UiCommandRouterRestoreResult
            {
                Accepted = Accepted,
                Changed = Changed,
                ReasonCode = ReasonCode ?? string.Empty,
                Snapshot = Snapshot == null ? new D22UiCommandRouterSnapshot() : Snapshot.Clone(),
            };
        }
    }

    /// <summary>
    /// D22-L26 命令聚合器。Router 不直接写港口、市场、库存、船舶、NPC、任务
    /// 或 Save；所有业务效果必须来自注入的公开 owner handler 回执。
    /// </summary>
    public sealed class D22UiCommandRouter
    {
        public const int CurrentSchemaVersion = D22UiCommandRouterSnapshot.CurrentSchemaVersion;
        public const string OwnerSystem = "D22UiCommandRouter";

        private readonly List<D22UiCommandOwnerRegistration> ownerHandlers =
            new List<D22UiCommandOwnerRegistration>();
        private readonly List<D22UiRoutedCommandRecord> processedCommands =
            new List<D22UiRoutedCommandRecord>();

        public D22UiCommandRouter() { }

        public D22UiCommandRouter(IEnumerable<D22UiCommandOwnerRegistration> registrations)
        {
            if (registrations == null) return;
            foreach (var registration in registrations) Register(registration);
        }

        public D22UiCommandRouter(params D22UiCommandOwnerRegistration[] registrations)
            : this((IEnumerable<D22UiCommandOwnerRegistration>)registrations) { }

        /// <summary>当前注册的公开 owner 数量。</summary>
        public int RegisteredOwnerCount { get { return ownerHandlers.Count; } }

        /// <summary>当前缓存的有效命令回执数量。</summary>
        public int ProcessedCommandCount { get { return processedCommands.Count; } }

        /// <summary>注册或替换一个公开 owner handler。</summary>
        public bool Register(D22UiCommandOwnerRegistration registration)
        {
            if (registration == null || !registration.IsUsable) return false;
            string ownerSystem = registration.OwnerSystem.Trim();
            if (ownerSystem.Length == 0) return false;

            for (int index = 0; index < ownerHandlers.Count; index++)
            {
                if (string.Equals(ownerHandlers[index].OwnerSystem, ownerSystem,
                    StringComparison.Ordinal))
                {
                    ownerHandlers[index] = new D22UiCommandOwnerRegistration(
                        ownerSystem, registration.Handler);
                    return true;
                }
            }
            ownerHandlers.Add(new D22UiCommandOwnerRegistration(ownerSystem, registration.Handler));
            return true;
        }

        /// <summary>以 owner ID 注册公开 handler。</summary>
        public bool Register(string ownerSystem, D22UiOwnerCommandHandler handler)
        {
            return Register(new D22UiCommandOwnerRegistration(ownerSystem, handler));
        }

        /// <summary>移除 owner handler；不影响已缓存的命令回执。</summary>
        public bool Unregister(string ownerSystem)
        {
            if (string.IsNullOrEmpty(ownerSystem)) return false;
            for (int index = 0; index < ownerHandlers.Count; index++)
            {
                if (!string.Equals(ownerHandlers[index].OwnerSystem, ownerSystem,
                    StringComparison.Ordinal)) continue;
                ownerHandlers.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>返回公开 owner ID 的确定性注册顺序副本。</summary>
        public IReadOnlyList<string> GetRegisteredOwners()
        {
            var result = new List<string>();
            foreach (var registration in ownerHandlers)
                result.Add(registration.OwnerSystem ?? string.Empty);
            return new ReadOnlyCollection<string>(result);
        }

        /// <summary>路由命令并返回只读命令结果。</summary>
        public D22UiCommandResult Route(
            D22UiCommandRequest request,
            D22UiCommandContext context)
        {
            return RouteDetailed(request, context).CommandResult.Clone();
        }

        /// <summary>路由命令并返回命令、反馈与 owner 调用证据。</summary>
        public D22UiCommandRouteResult RouteDetailed(
            D22UiCommandRequest request,
            D22UiCommandContext context)
        {
            D22UiCommandContext validationContext = MergeProcessedContext(context);
            D22UiCommandValidationResult validation =
                D22UiCommandValidator.Validate(request, validationContext);

            if (validation.Duplicate)
            {
                D22UiRoutedCommandRecord duplicateRecord =
                    FindRecord(request == null ? string.Empty : request.CommandId,
                        validation.RequestFingerprint);
                if (duplicateRecord != null)
                    return Replay(duplicateRecord);

                D22UiCommandResult duplicate = validation.ToCommandResult(request);
                duplicate.Outcome = D22UiCommandOutcome.Duplicate;
                duplicate.Duplicate = true;
                duplicate.Accepted = false;
                duplicate.FeedbackCode = D22FeedbackCodes.DuplicateCommand;
                return BuildRoute(duplicate, false);
            }

            if (validation.DuplicateConflict)
            {
                D22UiCommandResult conflict = validation.ToCommandResult(request);
                conflict.Outcome = D22UiCommandOutcome.Rejected;
                conflict.Duplicate = false;
                conflict.Accepted = false;
                conflict.FeedbackCode = D22FeedbackCodes.DuplicateConflict;
                return BuildRoute(conflict, false);
            }

            if (!validation.Valid)
            {
                D22UiCommandResult rejected = validation.ToCommandResult(request);
                rejected.FeedbackCode = D22FeedbackCatalog.ForValidation(validation.Code);
                return BuildRoute(rejected, false);
            }

            D22UiCommandDefinition definition;
            if (request == null ||
                !D22UiCommandContracts.TryGetDefinition(request.Kind, out definition))
            {
                return BuildRoute(CreateBlockedResult(request, string.Empty,
                    "d22.router.unknown_owner_definition", D22DisabledReasonCodes.OwnerUnavailable),
                    false);
            }

            string expectedOwner = definition.OwnerSystem ?? string.Empty;
            D22UiCommandOwnerRegistration registration = FindOwner(expectedOwner);
            if (registration == null)
            {
                D22UiCommandResult blocked = CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_unavailable", D22DisabledReasonCodes.OwnerUnavailable);
                Remember(request, blocked);
                return BuildRoute(blocked, false);
            }

            D22UiOwnerCommandResult ownerResult;
            try
            {
                ownerResult = registration.Handler(request.Clone());
            }
            catch (Exception)
            {
                ownerResult = D22UiOwnerCommandResult.Blocked(expectedOwner,
                    "d22.router.owner_handler_failed", D22DisabledReasonCodes.OwnerUnavailable);
            }

            D22UiCommandResult routed = NormalizeOwnerResult(request, expectedOwner, ownerResult);
            Remember(request, routed);
            return BuildRoute(routed, true);
        }

        /// <summary>返回 UI 命令幂等投影的防御性副本。</summary>
        public D22UiCommandRouterSnapshot Snapshot()
        {
            var snapshot = new D22UiCommandRouterSnapshot();
            foreach (var record in processedCommands)
                snapshot.ProcessedCommands.Add(record == null ? null : record.Clone());
            return snapshot;
        }

        /// <summary>SaveSystem 可调用的显式 UI 投影别名；不执行保存。</summary>
        public D22UiCommandRouterSnapshot CreateSaveSnapshot()
        {
            return Snapshot();
        }

        /// <summary>从外部提供的 UI 投影恢复幂等索引，不写入业务 owner。</summary>
        public D22UiCommandRouterRestoreResult Restore(D22UiCommandRouterSnapshot snapshot)
        {
            var result = new D22UiCommandRouterRestoreResult
            {
                Accepted = false,
                Changed = false,
                ReasonCode = "d22.router.invalid_snapshot",
                Snapshot = Snapshot(),
            };
            if (snapshot == null)
            {
                result.ReasonCode = "d22.router.missing_snapshot";
                return result;
            }
            if (snapshot.SchemaVersion != CurrentSchemaVersion || snapshot.ProcessedCommands == null)
            {
                result.ReasonCode = "d22.router.invalid_schema_version";
                return result;
            }

            var restored = new List<D22UiRoutedCommandRecord>();
            var commandIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in snapshot.ProcessedCommands)
            {
                if (!IsValidRecord(item) || !commandIds.Add(item.CommandId))
                {
                    result.ReasonCode = "d22.router.invalid_processed_command";
                    return result;
                }
                restored.Add(item.Clone());
            }

            bool changed = !SnapshotsEqual(processedCommands, restored);
            processedCommands.Clear();
            processedCommands.AddRange(restored);
            result.Accepted = true;
            result.Changed = changed;
            result.ReasonCode = changed
                ? "d22.router.snapshot_restored"
                : "d22.router.snapshot_unchanged";
            result.Snapshot = Snapshot();
            return result;
        }

        /// <summary>返回已缓存命令的防御性副本。</summary>
        public IReadOnlyList<D22UiRoutedCommandRecord> GetProcessedCommands()
        {
            var result = new List<D22UiRoutedCommandRecord>();
            foreach (var item in processedCommands)
                result.Add(item == null ? null : item.Clone());
            return new ReadOnlyCollection<D22UiRoutedCommandRecord>(result);
        }

        private D22UiCommandContext MergeProcessedContext(D22UiCommandContext context)
        {
            D22UiCommandContext result = context == null
                ? null
                : context.Clone();
            if (result == null) return null;
            if (result.ProcessedCommands == null)
                result.ProcessedCommands = new List<D22ProcessedCommand>();

            foreach (var record in processedCommands)
            {
                if (record == null || string.IsNullOrEmpty(record.CommandId)) continue;
                bool exists = false;
                foreach (var processed in result.ProcessedCommands)
                {
                    if (processed != null && string.Equals(processed.CommandId, record.CommandId,
                        StringComparison.Ordinal))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    result.ProcessedCommands.Add(new D22ProcessedCommand
                    {
                        CommandId = record.CommandId,
                        Fingerprint = record.Fingerprint,
                        CorrelationId = record.CorrelationId,
                    });
                }
            }
            return result;
        }

        private D22UiCommandOwnerRegistration FindOwner(string ownerSystem)
        {
            foreach (var registration in ownerHandlers)
            {
                if (registration != null && string.Equals(registration.OwnerSystem, ownerSystem,
                    StringComparison.Ordinal)) return registration;
            }
            return null;
        }

        private D22UiRoutedCommandRecord FindRecord(string commandId, string fingerprint)
        {
            foreach (var record in processedCommands)
            {
                if (record == null || !string.Equals(record.CommandId, commandId,
                    StringComparison.Ordinal)) continue;
                if (string.Equals(record.Fingerprint, fingerprint, StringComparison.Ordinal))
                    return record;
            }
            return null;
        }

        private D22UiCommandResult NormalizeOwnerResult(
            D22UiCommandRequest request,
            string expectedOwner,
            D22UiOwnerCommandResult ownerResult)
        {
            if (ownerResult == null)
                return CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_null_result", D22DisabledReasonCodes.OwnerUnavailable);
            if (ownerResult.SchemaVersion != D22UiOwnerCommandResult.CurrentSchemaVersion)
                return CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_schema_mismatch", D22DisabledReasonCodes.OwnerUnavailable);
            if (!string.IsNullOrEmpty(ownerResult.OwnerSystem) &&
                !string.Equals(ownerResult.OwnerSystem, expectedOwner, StringComparison.Ordinal))
                return CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_result_owner_mismatch", D22DisabledReasonCodes.OwnerUnavailable);
            if (!string.IsNullOrEmpty(ownerResult.CommandId) &&
                !string.Equals(ownerResult.CommandId, request.CommandId, StringComparison.Ordinal))
                return CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_result_command_mismatch", D22DisabledReasonCodes.OwnerUnavailable);
            if (!string.IsNullOrEmpty(ownerResult.CorrelationId) &&
                !string.Equals(ownerResult.CorrelationId, request.CorrelationId, StringComparison.Ordinal))
                return CreateBlockedResult(request, expectedOwner,
                    "d22.router.owner_result_correlation_mismatch", D22DisabledReasonCodes.OwnerUnavailable);

            D22UiCommandOutcome outcome = MapOutcome(ownerResult.Outcome);
            string feedbackCode = string.IsNullOrEmpty(ownerResult.FeedbackCode)
                ? DefaultFeedback(request.Kind, outcome)
                : ownerResult.FeedbackCode;
            string reasonCode = string.IsNullOrEmpty(ownerResult.ReasonCode)
                ? DefaultReason(outcome)
                : ownerResult.ReasonCode;
            string disabledCode = ownerResult.DisabledReasonCode ?? string.Empty;
            if (outcome == D22UiCommandOutcome.Blocked && string.IsNullOrEmpty(disabledCode))
                disabledCode = D22DisabledReasonCodes.OwnerUnavailable;
            if (outcome == D22UiCommandOutcome.Rejected && string.IsNullOrEmpty(disabledCode))
                disabledCode = D22DisabledReasonCodes.OwnerRejected;

            return new D22UiCommandResult
            {
                SchemaVersion = D22UiCommandContracts.CurrentSchemaVersion,
                CommandId = request.CommandId ?? string.Empty,
                CorrelationId = request.CorrelationId ?? string.Empty,
                Outcome = outcome,
                ValidationCode = D22UiCommandValidationCode.None,
                Accepted = outcome == D22UiCommandOutcome.Pending ||
                    outcome == D22UiCommandOutcome.Accepted ||
                    outcome == D22UiCommandOutcome.Succeeded,
                Duplicate = false,
                BusinessStateChanged = ownerResult.BusinessStateChanged &&
                    outcome == D22UiCommandOutcome.Succeeded,
                OwnerSystem = expectedOwner,
                OwnerResultCode = ownerResult.OwnerResultCode ?? string.Empty,
                FeedbackCode = feedbackCode,
                DisabledReasonCode = disabledCode,
                ReasonCode = reasonCode,
                SaveRevision = request.SaveRevision,
            };
        }

        private static D22UiCommandResult CreateBlockedResult(
            D22UiCommandRequest request,
            string ownerSystem,
            string reasonCode,
            string disabledReasonCode)
        {
            return new D22UiCommandResult
            {
                SchemaVersion = D22UiCommandContracts.CurrentSchemaVersion,
                CommandId = request == null ? string.Empty : request.CommandId ?? string.Empty,
                CorrelationId = request == null ? string.Empty : request.CorrelationId ?? string.Empty,
                Outcome = D22UiCommandOutcome.Blocked,
                ValidationCode = D22UiCommandValidationCode.None,
                Accepted = false,
                Duplicate = false,
                BusinessStateChanged = false,
                OwnerSystem = ownerSystem ?? string.Empty,
                OwnerResultCode = "owner_unavailable",
                FeedbackCode = D22FeedbackCodes.OwnerUnavailable,
                DisabledReasonCode = disabledReasonCode ?? D22DisabledReasonCodes.OwnerUnavailable,
                ReasonCode = reasonCode ?? "d22.router.owner_unavailable",
                SaveRevision = request == null ? 0 : request.SaveRevision,
            };
        }

        private void Remember(D22UiCommandRequest request, D22UiCommandResult result)
        {
            if (request == null || result == null || string.IsNullOrEmpty(request.CommandId)) return;
            string fingerprint = request.Fingerprint();
            D22UiRoutedCommandRecord existing = FindRecord(request.CommandId, fingerprint);
            if (existing != null) return;
            processedCommands.Add(new D22UiRoutedCommandRecord
            {
                CommandId = request.CommandId ?? string.Empty,
                Fingerprint = fingerprint ?? string.Empty,
                CorrelationId = request.CorrelationId ?? string.Empty,
                Result = result.Clone(),
            });
        }

        private D22UiCommandRouteResult Replay(D22UiRoutedCommandRecord record)
        {
            D22UiCommandResult result = record.Result == null
                ? new D22UiCommandResult()
                : record.Result.Clone();
            result.Outcome = D22UiCommandOutcome.Duplicate;
            result.Duplicate = true;
            result.Accepted = false;
            result.FeedbackCode = D22FeedbackCodes.DuplicateCommand;
            result.ReasonCode = "d22.command.duplicate_command";
            return BuildRoute(result, false);
        }

        private static D22UiCommandRouteResult BuildRoute(
            D22UiCommandResult result,
            bool ownerInvoked)
        {
            result = result ?? new D22UiCommandResult();
            string feedbackCode = string.IsNullOrEmpty(result.FeedbackCode)
                ? D22FeedbackCodes.Unknown
                : result.FeedbackCode;
            result.FeedbackCode = feedbackCode;
            var route = new D22UiCommandRouteResult
            {
                CommandResult = result.Clone(),
                Feedback = D22FeedbackCatalog.Create(
                    feedbackCode,
                    result.CorrelationId,
                    string.IsNullOrEmpty(result.OwnerSystem) ? OwnerSystem : result.OwnerSystem,
                    result.ReasonCode),
                OwnerInvoked = ownerInvoked,
            };
            return route;
        }

        private static D22UiCommandOutcome MapOutcome(D22UiOwnerCommandOutcome outcome)
        {
            switch (outcome)
            {
                case D22UiOwnerCommandOutcome.Succeeded: return D22UiCommandOutcome.Succeeded;
                case D22UiOwnerCommandOutcome.Rejected: return D22UiCommandOutcome.Rejected;
                case D22UiOwnerCommandOutcome.Blocked: return D22UiCommandOutcome.Blocked;
                case D22UiOwnerCommandOutcome.Cancelled: return D22UiCommandOutcome.Cancelled;
                default: return D22UiCommandOutcome.Pending;
            }
        }

        private static string DefaultFeedback(D22UiCommandKind kind, D22UiCommandOutcome outcome)
        {
            if (outcome == D22UiCommandOutcome.Succeeded)
            {
                if (kind == D22UiCommandKind.OpenPanel) return D22FeedbackCodes.PanelOpened;
                if (kind == D22UiCommandKind.ClosePanel) return D22FeedbackCodes.PanelClosed;
                return D22FeedbackCodes.CommandCompleted;
            }
            if (outcome == D22UiCommandOutcome.Pending) return D22FeedbackCodes.OwnerPending;
            if (outcome == D22UiCommandOutcome.Blocked) return D22FeedbackCodes.OwnerUnavailable;
            if (outcome == D22UiCommandOutcome.Rejected) return D22FeedbackCodes.OwnerRejected;
            if (outcome == D22UiCommandOutcome.Duplicate) return D22FeedbackCodes.DuplicateCommand;
            return D22FeedbackCodes.RetryUnavailable;
        }

        private static string DefaultReason(D22UiCommandOutcome outcome)
        {
            switch (outcome)
            {
                case D22UiCommandOutcome.Succeeded: return "d22.router.owner_succeeded";
                case D22UiCommandOutcome.Pending: return "d22.router.owner_pending";
                case D22UiCommandOutcome.Blocked: return "d22.router.owner_blocked";
                case D22UiCommandOutcome.Rejected: return "d22.router.owner_rejected";
                case D22UiCommandOutcome.Cancelled: return "d22.router.owner_cancelled";
                default: return "d22.router.owner_result";
            }
        }

        private static bool IsValidRecord(D22UiRoutedCommandRecord record)
        {
            return record != null &&
                D22UiCommandContracts.IsStableToken(record.CommandId) &&
                !string.IsNullOrEmpty(record.Fingerprint) &&
                D22UiCommandContracts.IsStableToken(record.CorrelationId) &&
                record.Result != null &&
                record.Result.SchemaVersion == D22UiCommandContracts.CurrentSchemaVersion &&
                record.Result.Outcome >= D22UiCommandOutcome.Pending &&
                record.Result.Outcome <= D22UiCommandOutcome.Cancelled &&
                string.Equals(record.Result.CommandId, record.CommandId, StringComparison.Ordinal) &&
                string.Equals(record.Result.CorrelationId, record.CorrelationId, StringComparison.Ordinal);
        }

        private static bool SnapshotsEqual(
            IList<D22UiRoutedCommandRecord> left,
            IList<D22UiRoutedCommandRecord> right)
        {
            if (left == null || right == null || left.Count != right.Count) return false;
            for (int index = 0; index < left.Count; index++)
            {
                D22UiRoutedCommandRecord a = left[index];
                D22UiRoutedCommandRecord b = right[index];
                if (!IsValidRecord(a) || !IsValidRecord(b) ||
                    !string.Equals(a.CommandId, b.CommandId, StringComparison.Ordinal) ||
                    !string.Equals(a.Fingerprint, b.Fingerprint, StringComparison.Ordinal) ||
                    !string.Equals(a.CorrelationId, b.CorrelationId, StringComparison.Ordinal) ||
                    a.Result.Outcome != b.Result.Outcome ||
                    !string.Equals(a.Result.OwnerResultCode, b.Result.OwnerResultCode,
                        StringComparison.Ordinal)) return false;
            }
            return true;
        }
    }
}
