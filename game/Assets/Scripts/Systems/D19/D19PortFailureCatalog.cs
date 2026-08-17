// ============================================================
// D19PortFailureCatalog —— D19 港口失败码目录（纯 C#，D19-L17）
//
// 归属：Whalebone.Systems.D19（D19 唯一失败码/重试建议目录，D19-L17）
// 目的：为 D19 港口编排提供唯一业务失败码、本地化 key、重试建议与参数白名单。
//       每个业务失败只有一个稳定 code（同一业务失败不会被多个字符串表示）；
//       后续系统（L03/L05—L10/L19—L22/L25—L32）无 UI 依赖即可消费。
// 边界：
//   - 不引用 Unity（Transform/Canvas/Input/网络/LLM）；不写任何中英文显示文案；
//   - 只读消费 D19PortLifecycleContracts（D19-L02）的 D19PortFailureCode 枚举
//     与 D19PortLocalizationKeys（D19-L18）的既有 key 常量；不修改这两个文件；
//   - 不吞掉既有系统的原始失败事实：每条目记录真实来源 API 与原始失败事实
//     （D3 TradeResult.Code / D9 D9PreparationResult.Code / SaveMigrationStatus /
//      D3InventorySystem bool+error / D4 ShipyardResult.Accepted 等，全部来自源码
//      盘点的真实字段）；D4 等无稳定码的系统只保留 Accepted=false 事实，不编造映射；
//   - D19 新领域（泊位租期/权限/登船/离港流程）无既有失败码时明确标注 planned，
//     不补造 D2—D4 失败码映射。
// key 说明：LocKey 优先引用 D19PortLocalizationKeys 既有常量；新领域 key 在本文件
// 定义（命名规则与 L18 一致：d19.<domain>.<name>），并列入 CatalogDefinedKeys 供
// L18 后续扩展注册表消费。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 业务失败域（D19-L17）。十个域与任务卡一一对应；每域至少一个稳定码。
    /// </summary>
    public enum D19FailureDomain
    {
        /// <summary>占位：非法域。</summary>
        None = 0,

        /// <summary>未知港口。</summary>
        Port,

        /// <summary>权限不足。</summary>
        Permission,

        /// <summary>非法生命周期。</summary>
        Lifecycle,

        /// <summary>泊位冲突/过期。</summary>
        Berth,

        /// <summary>服务关闭/不可用。</summary>
        Service,

        /// <summary>资金失败。</summary>
        Trade,

        /// <summary>库存/容量失败。</summary>
        Inventory,

        /// <summary>登船失败。</summary>
        Boarding,

        /// <summary>离港失败。</summary>
        Departure,

        /// <summary>保存冲突/不支持旧存档。</summary>
        Save
    }

    /// <summary>
    /// 重试建议（D19-L17）。由命令层结合幂等语义执行，本目录只给建议。
    /// </summary>
    public enum D19FailureRetryPolicy
    {
        /// <summary>占位：非法策略。</summary>
        None = 0,

        /// <summary>不可重试：结果已确定，重复提交无意义或可能造成副作用。</summary>
        NoRetry,

        /// <summary>可安全重试：相同输入重复投递由命令 ID 幂等保证，无副作用。</summary>
        RetrySafe,

        /// <summary>修正后重试：必须修正参数或前置状态后才能重试。</summary>
        RetryAfterCorrection
    }

    /// <summary>
    /// 失败码目录条目（D19-L17，只读数据）。
    /// Code/LocKey/Retry/ParamWhitelist 为消费方唯一可信来源；
    /// LifecycleCode/OriginalSource/OriginalFailureFact 保留既有系统原始事实，不吞掉。
    /// </summary>
    public sealed class D19FailureEntry
    {
        /// <summary>稳定业务失败码（d19.&lt;domain&gt;.&lt;name&gt;，唯一）。</summary>
        public string Code;

        /// <summary>稳定本地化 key（L18 常量或本目录定义键；不含显示文案）。</summary>
        public string LocKey;

        /// <summary>业务失败域。</summary>
        public D19FailureDomain Domain;

        /// <summary>重试建议。</summary>
        public D19FailureRetryPolicy Retry;

        /// <summary>参数白名单：允许随失败事实透传的稳定标量参数名。
        /// 仅允许小写 snake_case 的原始值（ID/数量/版本/状态名），
        /// 禁止泄露内部对象、快照、Unity 对象或引用类型字段名。</summary>
        public string[] ParamWhitelist;

        /// <summary>对应的生命周期失败码（D19PortLifecycleContracts，只读引用；
        /// None=无对应生命周期码）。生命周期码是子原因细节，不是独立业务码。</summary>
        public D19PortFailureCode LifecycleCode;

        /// <summary>原始失败事实的来源 API（真实存在，源码盘点）。</summary>
        public string OriginalSource;

        /// <summary>原始失败事实（真实字段/稳定码；不含显示文案）。</summary>
        public string OriginalFailureFact;

        /// <summary>补充说明（缺口/相邻事实/待办）。</summary>
        public string Note;

        public D19FailureEntry(
            string code,
            string locKey,
            D19FailureDomain domain,
            D19FailureRetryPolicy retry,
            string[] paramWhitelist,
            D19PortFailureCode lifecycleCode,
            string originalSource,
            string originalFailureFact,
            string note)
        {
            Code = code ?? string.Empty;
            LocKey = locKey ?? string.Empty;
            Domain = domain;
            Retry = retry;
            ParamWhitelist = paramWhitelist ?? new string[0];
            LifecycleCode = lifecycleCode;
            OriginalSource = originalSource ?? string.Empty;
            OriginalFailureFact = originalFailureFact ?? string.Empty;
            Note = note ?? string.Empty;
        }
    }

    /// <summary>
    /// D19 港口失败码目录（D19-L17，所有者：本目录）。
    /// 冻结条目表 + 查找/校验 API；纯数据，无 UI、无 Unity、无副作用。
    /// </summary>
    public static class D19PortFailureCatalog
    {
        /// <summary>业务码前缀（与 L18 key 规则一致）。</summary>
        public const string CodePrefix = "d19";

        /// <summary>业务码命名规则：d19.&lt;domain&gt;.&lt;name&gt;，每段 [a-z0-9_]+。</summary>
        public static bool IsValidCode(string code)
        {
            if (string.IsNullOrEmpty(code) || !code.StartsWith(CodePrefix + ".", StringComparison.Ordinal))
            {
                return false;
            }
            string[] parts = code.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }
            foreach (string part in parts)
            {
                if (part.Length == 0)
                {
                    return false;
                }
                foreach (char c in part)
                {
                    if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>任务卡要求的十个业务失败域（每域至少一条目）。</summary>
        public static readonly IReadOnlyList<D19FailureDomain> RequiredDomains = new List<D19FailureDomain>
        {
            D19FailureDomain.Port,
            D19FailureDomain.Permission,
            D19FailureDomain.Lifecycle,
            D19FailureDomain.Berth,
            D19FailureDomain.Service,
            D19FailureDomain.Trade,
            D19FailureDomain.Inventory,
            D19FailureDomain.Boarding,
            D19FailureDomain.Departure,
            D19FailureDomain.Save
        };

        // ---------- 本目录新增的本地化 key（L18 尚未注册；供 L18 后续扩展消费） ----------

        /// <summary>权限不足（L18 扩展待办）。</summary>
        public const string KeyPermissionDenied = "d19.failure.permission_denied";

        /// <summary>泊位过期（L18 扩展待办）。</summary>
        public const string KeyBerthExpired = "d19.failure.berth_expired";

        /// <summary>资金不足（L18 扩展待办）。</summary>
        public const string KeyFundsInsufficient = "d19.failure.funds_insufficient";

        /// <summary>库存操作失败（L18 扩展待办）。</summary>
        public const string KeyInventoryOperationFailed = "d19.failure.inventory_operation_failed";

        /// <summary>容量不足（L18 扩展待办）。</summary>
        public const string KeyCapacityExceeded = "d19.failure.capacity_exceeded";

        /// <summary>登船失败（L18 扩展待办）。</summary>
        public const string KeyBoardingFailed = "d19.failure.boarding_failed";

        /// <summary>离港失败（L18 扩展待办）。</summary>
        public const string KeyDepartureFailed = "d19.failure.departure_failed";

        /// <summary>本目录定义的全部新 key（供 L18 注册表扩展与 L45 一致性核对）。</summary>
        public static readonly IReadOnlyList<string> CatalogDefinedKeys = new List<string>
        {
            KeyPermissionDenied,
            KeyBerthExpired,
            KeyFundsInsufficient,
            KeyInventoryOperationFailed,
            KeyCapacityExceeded,
            KeyBoardingFailed,
            KeyDepartureFailed
        };

        // ---------- 冻结条目表 ----------

        /// <summary>冻结失败码目录。Code 全局唯一；同一业务失败只有一条目。</summary>
        public static readonly IReadOnlyList<D19FailureEntry> Entries = new List<D19FailureEntry>
        {
            // 1. 未知港口
            new D19FailureEntry(
                "d19.port.unknown_port",
                D19PortLocalizationKeys.FailureUnknownPort,
                D19FailureDomain.Port,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "port_id" },
                D19PortFailureCode.UnknownPort,
                "D9.D9PortShipPreparationSystem.PrepareDeparture",
                "D9PreparationResult.Accepted=false, Code=invalid_port（离港港口不存在）",
                "相邻事实：D3.TradeEconomySystem.Buy/Sell 对未知港口/货物返回 TradeResult.Code=invalid_reference，原码保留，不合并"),

            // 2. 权限不足
            new D19FailureEntry(
                "d19.port.permission_denied",
                KeyPermissionDenied,
                D19FailureDomain.Permission,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "port_id", "service", "actor_id" },
                D19PortFailureCode.None,
                "D19-L23 D19PortAuthorityValidator（计划，D19 新领域）",
                "无既有稳定失败码；D4.D4ShipyardSystem.TryUnlockShip/TryUpgrade 声望门槛仅暴露 ShipyardResult.Accepted=false（Message 为显示文本，不映射）",
                "既有 D14PortAccessStatus（Restricted/Denied/Blockaded）为 D14 章节域，D19 不越权引用其枚举"),

            // 3. 非法生命周期
            new D19FailureEntry(
                "d19.lifecycle.invalid_transition",
                D19PortLocalizationKeys.FailureInvalidTransition,
                D19FailureDomain.Lifecycle,
                D19FailureRetryPolicy.NoRetry,
                new[] { "from_state", "reason_id" },
                D19PortFailureCode.InvalidTransition,
                "D19.D19PortLifecycleContracts.CanTransition",
                "返回 false 且 ResolveRejectionCode=InvalidTransition；非法迁移原状态不变（L02 冻结迁移表）",
                string.Empty),

            // 4. 泊位冲突
            new D19FailureEntry(
                "d19.berth.occupied",
                D19PortLocalizationKeys.FailureBerthOccupied,
                D19FailureDomain.Berth,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "berth_id", "port_id" },
                D19PortFailureCode.BerthOccupied,
                "D19-L06 D19BerthSystem（计划，D19 新领域）",
                "无既有占用失败码；相邻事实：D9.D9PortShipPreparationSystem.AssignBerth 对非本港泊位返回 Code=unknown_berth，原码保留，不合并",
                "泊位占用/租期事实由 L06 泊位逻辑裁决"),

            // 5. 泊位过期
            new D19FailureEntry(
                "d19.berth.expired",
                KeyBerthExpired,
                D19FailureDomain.Berth,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "berth_id", "port_id", "reserved_world_day" },
                D19PortFailureCode.None,
                "D19-L06 D19BerthSystem（计划，D19 新领域）",
                "无既有失败码；租期过期属 D19 新领域，不编造 D2—D4 映射",
                "reserved_world_day 为稳定逻辑日整数，不泄露任何对象"),

            // 6. 服务关闭
            new D19FailureEntry(
                "d19.service.unavailable",
                D19PortLocalizationKeys.FailureServiceUnavailable,
                D19FailureDomain.Service,
                D19FailureRetryPolicy.RetrySafe,
                new[] { "port_id", "service" },
                D19PortFailureCode.ServiceUnavailable,
                "D9.D9PortShipPreparationSystem.CheckService",
                "D9PreparationResult.Code 属于 {invalid_state, invalid_port, not_moored}（服务前置未满足）；服务开放事实由 D9.BuildSnapshot.Services 读模型提供",
                "RetrySafe：服务前置可能随后满足；相同输入重试由命令 ID 幂等保证"),

            // 7. 资金不足
            new D19FailureEntry(
                "d19.trade.funds_insufficient",
                KeyFundsInsufficient,
                D19FailureDomain.Trade,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "commodity_id", "required_amount", "available_amount" },
                D19PortFailureCode.None,
                "D3.TradeEconomySystem / D9.D9PortShipPreparationSystem / D4.D4ShipyardSystem",
                "D3 TradeResult.Succeeded=false Code=not_enough_money；D9 D9PreparationResult.Code=not_enough_money；D4 ShipyardResult.Accepted=false（无稳定码，不编造）",
                "金额参数为稳定整数，不泄露货币对象/快照"),

            // 8. 库存操作失败
            new D19FailureEntry(
                "d19.inventory.operation_failed",
                KeyInventoryOperationFailed,
                D19FailureDomain.Inventory,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "item_id", "quantity" },
                D19PortFailureCode.None,
                "D3.D3InventorySystem",
                "TryAddCargo/TryRemoveCargo/TryAddPersonal 返回 false 且 out string error（无稳定码；error 为显示文本，不映射）",
                string.Empty),

            // 9. 容量不足
            new D19FailureEntry(
                "d19.inventory.capacity_exceeded",
                KeyCapacityExceeded,
                D19FailureDomain.Inventory,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "item_id", "quantity", "capacity", "used" },
                D19PortFailureCode.None,
                "D3.D3InventorySystem",
                "CanAddCargo=false 及 FreeCargo/FreeWeight/UsedCargo/UsedVolume 读数（真实容量事实）；与 TradeResult.Code=market_empty（市场缺货）不同，不合并",
                string.Empty),

            // 10. 登船失败
            new D19FailureEntry(
                "d19.boarding.failed",
                KeyBoardingFailed,
                D19FailureDomain.Boarding,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "member_id", "state" },
                D19PortFailureCode.AlreadyBoarded,
                "D19-L08 D19BoardingWorkflow（计划，D19 新领域）",
                "生命周期子原因 ∈ {NotMoored, NotBerthed, AlreadyBoarded}（L02 冻结迁移表）；子原因是细节，业务码唯一",
                "state 为生命周期枚举名（稳定标识），非对象引用"),

            // 11. 离港失败
            new D19FailureEntry(
                "d19.departure.failed",
                KeyDepartureFailed,
                D19FailureDomain.Departure,
                D19FailureRetryPolicy.RetryAfterCorrection,
                new[] { "ship_id", "port_id", "destination_id" },
                D19PortFailureCode.NotReadyToDepart,
                "D9.D9PortShipPreparationSystem.PrepareDeparture / Depart",
                "D9PreparationResult.Code 属于 {not_moored, no_crew, insufficient_supplies, invalid_port, not_in_port, invalid_destination}（真实码保留）",
                string.Empty),

            // 12. 保存冲突
            new D19FailureEntry(
                "d19.save.conflict",
                D19PortLocalizationKeys.MigrationSaveConflict,
                D19FailureDomain.Save,
                D19FailureRetryPolicy.RetrySafe,
                new[] { "save_revision", "command_id" },
                D19PortFailureCode.None,
                "Save.SaveSystem.AppendD9SaveCheckpoint / D3.TradeResult.SaveRevision / D9.D9PreparationResult.Duplicate",
                "revision 与 duplicate 机制真实存在（D9 duplicate_settlement 事实、SaveRevision 字段）；无独立 conflict 失败码，不编造；D19-L20 投影检测冲突",
                "RetrySafe：刷新修订号后重新提交由命令 ID 幂等保证"),

            // 13. 不支持旧存档
            new D19FailureEntry(
                "d19.save.unsupported_version",
                D19PortLocalizationKeys.MigrationSaveUnsupported,
                D19FailureDomain.Save,
                D19FailureRetryPolicy.NoRetry,
                new[] { "save_version", "supported_version" },
                D19PortFailureCode.None,
                "Save.SaveMigration",
                "SaveMigrationStatus.UnsupportedVersion（真实枚举）；SaveMigrationResult.ErrorCode/Message/OriginalPreserved 保留",
                "NoRetry：需人工迁移（SaveMigration），不自动重试")
        };

        /// <summary>全部业务码（顺序固定，确定性）。</summary>
        public static IReadOnlyList<string> AllCodes
        {
            get
            {
                List<string> codes = new List<string>(Entries.Count);
                foreach (D19FailureEntry entry in Entries)
                {
                    codes.Add(entry.Code);
                }
                return codes;
            }
        }

        /// <summary>按业务码查找条目；不存在返回 null。</summary>
        public static D19FailureEntry Find(string code)
        {
            foreach (D19FailureEntry entry in Entries)
            {
                if (entry.Code == code)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>按业务码解析条目；返回 false 表示未登记（调用方必须显式处理，不静默成功）。</summary>
        public static bool TryResolve(string code, out D19FailureEntry entry)
        {
            entry = Find(code);
            return entry != null;
        }

        /// <summary>
        /// 生命周期失败码 → 业务条目（只读引用 D19PortLifecycleContracts 枚举）。
        /// 返回 null 表示该生命周期码无业务目录条目（生命周期内部细节，不作为独立业务消息）。
        /// </summary>
        public static D19FailureEntry ForLifecycleCode(D19PortFailureCode lifecycleCode)
        {
            foreach (D19FailureEntry entry in Entries)
            {
                if (entry.LifecycleCode == lifecycleCode)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// 参数白名单禁词：任何内部对象/Unity 对象/快照/引用类参数名不得出现。
        /// </summary>
        public static readonly string[] ForbiddenParamTokens =
        {
            "transform", "unity", "gameobject", "component", "snapshot",
            "canvas", "input", "pointer", "object", "ref"
        };

        private static bool IsValidParamName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 40)
            {
                return false;
            }
            if (!(name[0] >= 'a' && name[0] <= 'z'))
            {
                return false;
            }
            foreach (char c in name)
            {
                if (!((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                {
                    return false;
                }
            }
            foreach (string token in ForbiddenParamTokens)
            {
                if (name.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsKnownKey(string locKey)
        {
            foreach (string key in D19PortLocalizationKeys.AllKeys)
            {
                if (key == locKey)
                {
                    return true;
                }
            }
            foreach (string key in CatalogDefinedKeys)
            {
                if (key == locKey)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 目录自检（确定性）：重复业务码、重复/非法 key、非法码格式、非法参数名、
        /// 未登记 key、缺域条目、非法重试策略。返回空列表表示目录健全；
        /// 任何问题必须显式处理，绝不静默回退。
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();
            HashSet<string> seenCodes = new HashSet<string>();
            HashSet<string> seenKeys = new HashSet<string>();
            HashSet<D19FailureDomain> seenDomains = new HashSet<D19FailureDomain>();

            foreach (D19FailureEntry entry in Entries)
            {
                if (entry == null)
                {
                    problems.Add("entries contain a null entry");
                    continue;
                }
                if (!IsValidCode(entry.Code))
                {
                    problems.Add("invalid code format: " + entry.Code);
                }
                if (!seenCodes.Add(entry.Code))
                {
                    problems.Add("duplicate code: " + entry.Code);
                }
                if (!D19PortLocalizationKeys.IsValidKey(entry.LocKey))
                {
                    problems.Add("invalid loc key: " + entry.LocKey);
                }
                if (!seenKeys.Add(entry.LocKey))
                {
                    problems.Add("duplicate loc key: " + entry.LocKey);
                }
                if (!IsKnownKey(entry.LocKey))
                {
                    problems.Add("loc key not registered in L18 AllKeys or catalog keys: " + entry.LocKey);
                }
                if (entry.Domain == D19FailureDomain.None)
                {
                    problems.Add("entry has None domain: " + entry.Code);
                }
                else
                {
                    seenDomains.Add(entry.Domain);
                }
                if (entry.Retry == D19FailureRetryPolicy.None)
                {
                    problems.Add("entry has None retry policy: " + entry.Code);
                }
                if (entry.ParamWhitelist == null || entry.ParamWhitelist.Length == 0)
                {
                    problems.Add("entry has empty param whitelist: " + entry.Code);
                }
                else
                {
                    foreach (string name in entry.ParamWhitelist)
                    {
                        if (!IsValidParamName(name))
                        {
                            problems.Add("invalid whitelist param '" + name + "' in " + entry.Code);
                        }
                    }
                }
                if (string.IsNullOrEmpty(entry.OriginalSource) || string.IsNullOrEmpty(entry.OriginalFailureFact))
                {
                    problems.Add("entry missing original failure fact: " + entry.Code);
                }
            }

            foreach (D19FailureDomain domain in RequiredDomains)
            {
                if (!seenDomains.Contains(domain))
                {
                    problems.Add("required domain missing: " + domain);
                }
            }

            problems.Sort(StringComparer.Ordinal);
            return problems;
        }
    }
}
