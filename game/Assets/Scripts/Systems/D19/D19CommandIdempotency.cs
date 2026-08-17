// ============================================================
// D19CommandIdempotency —— D19 港口命令幂等控制（纯 C#，D19-L22）
//
// 归属：Whalebone.Systems.D19（所有者：本系统，D19-L22）
// 目的：为市场、仓库、泊位、登船、离港与保存命令提供共用幂等语义：
//       * 命令 ID（调用方生成，跨重试不变）+ 命令指纹（参数确定性摘要）；
//       * 相同 ID + 相同指纹 → 返回原结果（Completed），绝无第二个副作用；
//       * 相同 ID + 不同指纹 → 明确冲突（Conflict），状态不变；
//       * 处理中标记（InProgress）：异常/中断残留可审计、可恢复，
//         服务绝不伪造已完成结果；
//       * 冲突/过期策略：过期（按世界日，非真实时钟）与数量上限驱逐，
//         过期后命令 ID 可重新使用；
//       * 可保存的最小投影（BuildSaveProjection / RestoreFromProjection），
//         供 D19-L20 投影到既有 Save 模型。
// 本系统不执行任何副作用：不扣款、不占泊、不发事件。适配器只在
// Outcome==New 时执行副作用并 Complete；其余结果只读返回。
// 约束：不使用全局静态缓存（每船实例）、不使用真实时钟（时间一律
// 以 worldDay 参数传入）、不触碰 Transform/NavMesh/Unity/网络/LLM；
// 只读引用 D19-L02 契约（D19PortFailure），不复制其类型。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 幂等记录的命令域（D19-L22）。覆盖任务卡要求的六类命令
    /// （市场/仓库/泊位/登船/离港/保存）与其余港口服务；适配器按域登记。
    /// </summary>
    public enum D19IdempotencyDomain
    {
        /// <summary>占位：非法域。</summary>
        None = 0,

        /// <summary>市场买卖（D19-L11 适配器）。</summary>
        Market,

        /// <summary>仓库取放（D19-L12 适配器）。</summary>
        Warehouse,

        /// <summary>泊位预留/占用/释放（D19-L06/L10）。</summary>
        Berth,

        /// <summary>登船/下船（D19-L08）。</summary>
        Boarding,

        /// <summary>离港（D19-L09）。</summary>
        Departure,

        /// <summary>保存命令（D19-L20）。</summary>
        Save,

        /// <summary>其他港口服务（酒馆/船坞/补给，D19-L13—L15）。</summary>
        Service
    }

    /// <summary>幂等记录的执行状态（冻结枚举；无全局静态状态）。</summary>
    public enum D19CommandExecutionState
    {
        /// <summary>占位：未知。</summary>
        None = 0,

        /// <summary>处理中标记：已登记未完成（中断残留可审计、可恢复）。</summary>
        InProgress,

        /// <summary>已完成：原结果已登记，重复投递返回原结果。</summary>
        Completed,

        /// <summary>已中止：以安全结果终止（终态，重试须换新命令 ID）。</summary>
        Aborted
    }

    /// <summary>幂等判定结果（稳定语义，适配器据其决定是否执行副作用）。</summary>
    public enum D19IdempotencyOutcome
    {
        /// <summary>占位：未知。</summary>
        None = 0,

        /// <summary>首次登记（已标记 InProgress）：适配器执行副作用并 Complete。</summary>
        New,

        /// <summary>重复投递命中：相同 ID+指纹，返回原结果，不执行任何副作用。</summary>
        Completed,

        /// <summary>处理中残留：相同 ID+指纹但未完成；可恢复，不伪造结果。</summary>
        InProgress,

        /// <summary>明确冲突：相同 ID 但指纹不同；状态不变，禁止执行副作用。</summary>
        Conflict,

        /// <summary>缺失/空命令 ID：无法登记。</summary>
        MissingCommandId,

        /// <summary>未知命令：Complete/Abort 时未登记。</summary>
        UnknownCommand,

        /// <summary>已中止命中：终态安全结果。</summary>
        Aborted
    }

    /// <summary>幂等稳定原因 ID（点分小写，可入事件与存档；非 UI 文案）。</summary>
    public static class D19IdempotencyReasonIds
    {
        public const string Conflict = "d19.idem.conflict";
        public const string MissingCommandId = "d19.idem.missing_command_id";
        public const string InProgress = "d19.idem.in_progress";
        public const string Aborted = "d19.idem.aborted";
        public const string UnknownCommand = "d19.idem.unknown_command";
        public const string Invariant = "d19.idem.invariant";
        public const string Expired = "d19.idem.expired";
    }

    /// <summary>
    /// 幂等判定结果 DTO（D19-L22）。Completed/InProgress/Aborted 命中时携带
    /// 原结果（ResultCode/SummaryKey/Occurrence/WorldDay）与可恢复标记；
    /// 适配器对 Completed/Aborted 直接返回、对 InProgress 走恢复/中止路径。
    /// </summary>
    public sealed class D19IdempotencyResult
    {
        /// <summary>判定结果。</summary>
        public D19IdempotencyOutcome Outcome;

        /// <summary>稳定命令 ID（与请求一致）。</summary>
        public string CommandId = string.Empty;

        /// <summary>命令指纹（与请求一致）。</summary>
        public string Fingerprint = string.Empty;

        /// <summary>原结果稳定机器码（Completed/InProgress/Aborted 命中时有效）。</summary>
        public string ResultCode = string.Empty;

        /// <summary>原结果本地化 key（可为空；禁止 UI 文案）。</summary>
        public string SummaryKey = string.Empty;

        /// <summary>原执行产生的事件数（重复投递应补发 0 个新事件）。</summary>
        public int Occurrence;

        /// <summary>原结果完成的世界日。</summary>
        public int WorldDay;

        /// <summary>是否可恢复（InProgress 时 true；Completed/Aborted 为 false）。</summary>
        public bool Recoverable;

        /// <summary>稳定原因 ID（Conflict/Aborted/InProgress 时有效）。</summary>
        public string ReasonId = string.Empty;

        /// <summary>本结果是否为副作用执行授权（仅 New 为 true）。</summary>
        public bool SideEffectAuthorized { get { return Outcome == D19IdempotencyOutcome.New; } }

        public D19IdempotencyResult(
            D19IdempotencyOutcome outcome,
            string commandId,
            string fingerprint,
            string resultCode = "",
            string summaryKey = "",
            int occurrence = 0,
            int worldDay = 0,
            bool recoverable = false,
            string reasonId = "")
        {
            Outcome = outcome;
            CommandId = commandId ?? string.Empty;
            Fingerprint = fingerprint ?? string.Empty;
            ResultCode = resultCode ?? string.Empty;
            SummaryKey = summaryKey ?? string.Empty;
            Occurrence = occurrence;
            WorldDay = worldDay;
            Recoverable = recoverable;
            ReasonId = reasonId ?? string.Empty;
        }
    }

    /// <summary>存档投影中的单条幂等记录（D19-L22 最小投影，D19-L20 消费）。</summary>
    [Serializable]
    public sealed class D19IdempotencySaveEntry
    {
        public string CommandId = string.Empty;
        public string Fingerprint = string.Empty;
        public string Domain = string.Empty;   // D19IdempotencyDomain 枚举名
        public string State = string.Empty;    // D19CommandExecutionState 枚举名
        public string ResultCode = string.Empty;
        public string SummaryKey = string.Empty;
        public int WorldDay;
        public int Occurrence;
    }

    /// <summary>
    /// 可保存的最小投影（D19-L22）。字段全部为可序列化标量/列表，
    /// 无 Unity 依赖；L20 经 SaveSystem 既有投影接口落盘，不得写 GameState。
    /// </summary>
    [Serializable]
    public sealed class D19IdempotencySaveProjection
    {
        public int SchemaVersion = 1;
        public string ShipId = string.Empty;
        public int RetentionWorldDays = 30;
        public int MaxRetainedEntries = 512;
        public List<D19IdempotencySaveEntry> Entries = new List<D19IdempotencySaveEntry>();
    }

    /// <summary>
    /// 港口命令幂等控制（D19-L22，所有者：本系统）。
    /// 每船一个实例（构造绑定 shipId）；禁止全局静态缓存。
    /// 时间一律以 worldDay 参数传入，禁止真实时钟。
    /// </summary>
    public sealed class D19CommandIdempotency
    {
        private readonly string _shipId;
        private readonly int _retentionWorldDays;
        private readonly int _maxRetainedEntries;
        private readonly List<D19IdempotencyEntry> _entries = new List<D19IdempotencyEntry>();

        /// <summary>绑定船只稳定键（只读）。</summary>
        public string ShipId { get { return _shipId; } }

        /// <summary>当前登记的命令记录数（只读）。</summary>
        public int Count { get { return _entries.Count; } }

        /// <summary>已完成的命令数（只读）。</summary>
        public int CompletedCount
        {
            get
            {
                int count = 0;
                foreach (D19IdempotencyEntry entry in _entries)
                    if (entry.State == D19CommandExecutionState.Completed)
                        count++;
                return count;
            }
        }

        /// <summary>处理中的命令数（只读；中断残留可见）。</summary>
        public int InProgressCount
        {
            get
            {
                int count = 0;
                foreach (D19IdempotencyEntry entry in _entries)
                    if (entry.State == D19CommandExecutionState.InProgress)
                        count++;
                return count;
            }
        }

        public D19CommandIdempotency(string shipId, int retentionWorldDays = 30, int maxRetainedEntries = 512)
        {
            _shipId = shipId ?? string.Empty;
            _retentionWorldDays = retentionWorldDays > 0 ? retentionWorldDays : 30;
            _maxRetainedEntries = maxRetainedEntries > 0 ? maxRetainedEntries : 512;
        }

        /// <summary>
        /// 命令指纹构造：按固定顺序规范化并连接参数部分，得到确定性指纹。
        /// 相同参数顺序必然产生相同指纹；适配器必须保持部分顺序稳定。
        /// </summary>
        public static string BuildFingerprint(params string[] parts)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append("fp1|");
            if (parts == null)
                return builder.ToString();
            foreach (string part in parts)
            {
                if (part == null)
                {
                    builder.Append("|");
                }
                else
                {
                    // 规范化：去首尾空白，并把分隔符转义掉，防止拼接歧义。
                    string normalized = part.Trim();
                    builder.Append(normalized.Replace("|", "\\p"));
                    builder.Append("|");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 命令开始：登记处理中标记。
        /// New（首次，授权执行副作用）/ Completed（原结果命中）/
        /// InProgress（中断残留，可恢复）/ Conflict（同 ID 异指纹）/
        /// Aborted（已中止终态）/ MissingCommandId。
        /// </summary>
        public D19IdempotencyResult Begin(
            D19IdempotencyDomain domain,
            string commandId,
            string fingerprint,
            int worldDay)
        {
            if (domain == D19IdempotencyDomain.None)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Conflict, commandId ?? string.Empty, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.Invariant);
            if (string.IsNullOrWhiteSpace(commandId))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.MissingCommandId, string.Empty, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.MissingCommandId);
            if (string.IsNullOrWhiteSpace(fingerprint))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Conflict, commandId, string.Empty,
                    reasonId: D19IdempotencyReasonIds.Invariant);

            D19IdempotencyEntry entry = FindEntry(commandId);
            if (entry != null)
            {
                if (!string.Equals(entry.Fingerprint, fingerprint, StringComparison.Ordinal))
                    return new D19IdempotencyResult(
                        D19IdempotencyOutcome.Conflict, commandId, fingerprint,
                        reasonId: D19IdempotencyReasonIds.Conflict);
                return HandleExisting(entry, commandId, fingerprint);
            }

            // 过期策略：已完成记录超过保留世界日 → 视同未登记（命令 ID 可复用）。
            EvictExpiredBefore(worldDay);

            D19IdempotencyEntry created = new D19IdempotencyEntry();
            created.CommandId = commandId;
            created.Fingerprint = fingerprint;
            created.Domain = domain;
            created.State = D19CommandExecutionState.InProgress;
            created.WorldDay = worldDay;
            _entries.Add(created);
            EnforceCapacity();
            return new D19IdempotencyResult(
                D19IdempotencyOutcome.New, commandId, fingerprint, worldDay: worldDay);
        }

        /// <summary>
        /// 命令完成：登记原结果（状态 → Completed）。仅 New/InProgress 可完成；
        /// 已完成/已中止命中返回其既有结果；未登记返回 UnknownCommand。
        /// </summary>
        public D19IdempotencyResult Complete(
            D19IdempotencyDomain domain,
            string commandId,
            string fingerprint,
            string resultCode,
            string summaryKey,
            int occurrence,
            int worldDay)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.MissingCommandId, string.Empty, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.MissingCommandId);

            D19IdempotencyEntry entry = FindEntry(commandId);
            if (entry == null)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.UnknownCommand, commandId, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.UnknownCommand);
            if (!string.Equals(entry.Fingerprint, fingerprint, StringComparison.Ordinal))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Conflict, commandId, fingerprint,
                    reasonId: D19IdempotencyReasonIds.Conflict);

            if (entry.State == D19CommandExecutionState.Completed)
                return CompletedHit(entry, commandId, fingerprint);

            if (entry.State == D19CommandExecutionState.Aborted)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Aborted, commandId, fingerprint,
                    resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                    occurrence: entry.Occurrence, worldDay: entry.WorldDay,
                    reasonId: D19IdempotencyReasonIds.Aborted);

            entry.State = D19CommandExecutionState.Completed;
            entry.ResultCode = resultCode ?? string.Empty;
            entry.SummaryKey = summaryKey ?? string.Empty;
            entry.Occurrence = occurrence;
            entry.WorldDay = worldDay;
            return CompletedHit(entry, commandId, fingerprint);
        }

        /// <summary>
        /// 命令中止（异常/中断的安全结果）：状态 → Aborted（终态），
        /// 记录安全结果与原因；重试须使用新命令 ID。
        /// </summary>
        public D19IdempotencyResult Abort(
            D19IdempotencyDomain domain,
            string commandId,
            string fingerprint,
            string resultCode,
            string summaryKey,
            int worldDay)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.MissingCommandId, string.Empty, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.MissingCommandId);

            D19IdempotencyEntry entry = FindEntry(commandId);
            if (entry == null)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.UnknownCommand, commandId, fingerprint ?? string.Empty,
                    reasonId: D19IdempotencyReasonIds.UnknownCommand);
            if (!string.Equals(entry.Fingerprint, fingerprint, StringComparison.Ordinal))
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Conflict, commandId, fingerprint,
                    reasonId: D19IdempotencyReasonIds.Conflict);

            if (entry.State == D19CommandExecutionState.Aborted)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Aborted, commandId, fingerprint,
                    resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                    occurrence: entry.Occurrence, worldDay: entry.WorldDay,
                    reasonId: D19IdempotencyReasonIds.Aborted);
            if (entry.State == D19CommandExecutionState.Completed)
                return CompletedHit(entry, commandId, fingerprint);

            entry.State = D19CommandExecutionState.Aborted;
            entry.ResultCode = resultCode ?? string.Empty;
            entry.SummaryKey = summaryKey ?? string.Empty;
            entry.WorldDay = worldDay;
            return new D19IdempotencyResult(
                D19IdempotencyOutcome.Aborted, commandId, fingerprint,
                resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                occurrence: 0, worldDay: entry.WorldDay,
                reasonId: D19IdempotencyReasonIds.Aborted);
        }

        /// <summary>
        /// 过期策略：移除完成/中止于 thresholdWorldDay 之前的所有终态记录
        /// （处理中标记永不自动清除——中断残留必须保留到人工/补偿解决）。
        /// </summary>
        public int EvictCompletedBefore(int thresholdWorldDay)
        {
            int removed = 0;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                D19IdempotencyEntry entry = _entries[i];
                if (entry.State == D19CommandExecutionState.Completed ||
                    entry.State == D19CommandExecutionState.Aborted)
                {
                    if (entry.WorldDay < thresholdWorldDay)
                    {
                        _entries.RemoveAt(i);
                        removed++;
                    }
                }
            }
            return removed;
        }

        /// <summary>构建可保存的最小投影（D19-L20 消费；字段稳定、无 Unity 依赖）。</summary>
        public D19IdempotencySaveProjection BuildSaveProjection()
        {
            D19IdempotencySaveProjection projection = new D19IdempotencySaveProjection();
            projection.ShipId = _shipId;
            projection.RetentionWorldDays = _retentionWorldDays;
            projection.MaxRetainedEntries = _maxRetainedEntries;
            foreach (D19IdempotencyEntry entry in _entries)
            {
                D19IdempotencySaveEntry saveEntry = new D19IdempotencySaveEntry();
                saveEntry.CommandId = entry.CommandId;
                saveEntry.Fingerprint = entry.Fingerprint;
                saveEntry.Domain = entry.Domain.ToString();
                saveEntry.State = entry.State.ToString();
                saveEntry.ResultCode = entry.ResultCode;
                saveEntry.SummaryKey = entry.SummaryKey;
                saveEntry.WorldDay = entry.WorldDay;
                saveEntry.Occurrence = entry.Occurrence;
                projection.Entries.Add(saveEntry);
            }
            return projection;
        }

        /// <summary>从投影恢复（读档后重建幂等状态；缺省/非法条目被安全跳过）。</summary>
        public void RestoreFromProjection(D19IdempotencySaveProjection projection)
        {
            _entries.Clear();
            if (projection == null)
                return;
            foreach (D19IdempotencySaveEntry saveEntry in projection.Entries)
            {
                if (saveEntry == null || string.IsNullOrWhiteSpace(saveEntry.CommandId) ||
                    string.IsNullOrWhiteSpace(saveEntry.Fingerprint))
                    continue;
                D19CommandExecutionState state;
                if (!TryParseState(saveEntry.State, out state))
                    continue;
                D19IdempotencyDomain domain;
                if (!TryParseDomain(saveEntry.Domain, out domain))
                    continue;
                D19IdempotencyEntry entry = new D19IdempotencyEntry();
                entry.CommandId = saveEntry.CommandId;
                entry.Fingerprint = saveEntry.Fingerprint;
                entry.Domain = domain;
                entry.State = state;
                entry.ResultCode = saveEntry.ResultCode ?? string.Empty;
                entry.SummaryKey = saveEntry.SummaryKey ?? string.Empty;
                entry.WorldDay = saveEntry.WorldDay;
                entry.Occurrence = saveEntry.Occurrence;
                _entries.Add(entry);
            }
        }

        // ---- 内部 ----

        private sealed class D19IdempotencyEntry
        {
            public string CommandId = string.Empty;
            public string Fingerprint = string.Empty;
            public D19IdempotencyDomain Domain = D19IdempotencyDomain.None;
            public D19CommandExecutionState State = D19CommandExecutionState.None;
            public string ResultCode = string.Empty;
            public string SummaryKey = string.Empty;
            public int WorldDay;
            public int Occurrence;
        }

        private D19IdempotencyEntry FindEntry(string commandId)
        {
            foreach (D19IdempotencyEntry entry in _entries)
                if (string.Equals(entry.CommandId, commandId, StringComparison.Ordinal))
                    return entry;
            return null;
        }

        private D19IdempotencyResult HandleExisting(
            D19IdempotencyEntry entry,
            string commandId,
            string fingerprint)
        {
            if (entry.State == D19CommandExecutionState.Completed)
                return CompletedHit(entry, commandId, fingerprint);
            if (entry.State == D19CommandExecutionState.Aborted)
                return new D19IdempotencyResult(
                    D19IdempotencyOutcome.Aborted, commandId, fingerprint,
                    resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                    occurrence: entry.Occurrence, worldDay: entry.WorldDay,
                    reasonId: D19IdempotencyReasonIds.Aborted);
            // InProgress：中断残留，可审计、可恢复；绝不伪造完成结果。
            return new D19IdempotencyResult(
                D19IdempotencyOutcome.InProgress, commandId, fingerprint,
                resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                occurrence: entry.Occurrence, worldDay: entry.WorldDay,
                recoverable: true, reasonId: D19IdempotencyReasonIds.InProgress);
        }

        private static D19IdempotencyResult CompletedHit(
            D19IdempotencyEntry entry,
            string commandId,
            string fingerprint)
        {
            return new D19IdempotencyResult(
                D19IdempotencyOutcome.Completed, commandId, fingerprint,
                resultCode: entry.ResultCode, summaryKey: entry.SummaryKey,
                occurrence: entry.Occurrence, worldDay: entry.WorldDay);
        }

        private void EvictExpiredBefore(int worldDay)
        {
            EvictCompletedBefore(worldDay - _retentionWorldDays);
        }

        private void EnforceCapacity()
        {
            while (_entries.Count > _maxRetainedEntries)
            {
                bool removed = false;
                for (int i = 0; i < _entries.Count; i++)
                {
                    D19IdempotencyEntry entry = _entries[i];
                    if (entry.State == D19CommandExecutionState.Completed ||
                        entry.State == D19CommandExecutionState.Aborted)
                    {
                        _entries.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
                if (!removed)
                    break; // 全为处理中残留：不自动清除（安全网）。
            }
        }

        private static bool TryParseState(string value, out D19CommandExecutionState state)
        {
            state = D19CommandExecutionState.None;
            foreach (D19CommandExecutionState candidate in Enum.GetValues(typeof(D19CommandExecutionState)))
            {
                if (candidate != D19CommandExecutionState.None &&
                    candidate.ToString().Equals(value, StringComparison.Ordinal))
                {
                    state = candidate;
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseDomain(string value, out D19IdempotencyDomain domain)
        {
            domain = D19IdempotencyDomain.None;
            foreach (D19IdempotencyDomain candidate in Enum.GetValues(typeof(D19IdempotencyDomain)))
            {
                if (candidate != D19IdempotencyDomain.None &&
                    candidate.ToString().Equals(value, StringComparison.Ordinal))
                {
                    domain = candidate;
                    return true;
                }
            }
            return false;
        }
    }
}
