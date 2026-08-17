// ============================================================
// D19PortAvailabilitySystem —— 港口服务可用性裁决（纯函数，D19-L05）
//
// 纯函数/纯状态：只根据显式传入的输入计算可用/不可用/拒绝。
// 不读取系统时钟、场景天气、NPC GameObject 或资源；NPC 仅以
// 逻辑角色/服务可用性标记（staffed 布尔）表示。不修改资金、
// 库存、关系或场景——本类无任何写路径。
// 相同输入永远得到相同结果（确定性）。
// 原因码全部来自 D19PortFailureCatalog（D19-L17，Find 可解析）；
// 本地化 key 全部来自 D19PortLocalizationKeys（D19-L18，注册表校验）。
// 未知/冲突输入保守拒绝（Rejected），绝不静默成功。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>逻辑时间段（纯逻辑输入，不是系统时钟）。</summary>
    public enum D19ServiceTimePeriod
    {
        None = 0,
        Morning,
        Afternoon,
        Evening,
        Night,
    }

    /// <summary>天气逻辑标签（纯逻辑输入，不读取场景天气）。</summary>
    public enum D19WeatherLabel
    {
        None = 0,
        Clear,
        Rain,
        Storm,
    }

    /// <summary>势力许可（纯逻辑输入）。</summary>
    public enum D19FactionPermission
    {
        None = 0,
        Allowed,
        Denied,
    }

    /// <summary>设施状态（纯逻辑输入，来自建筑/港口域读模型）。</summary>
    public enum D19FacilityState
    {
        None = 0,
        Open,
        Closed,
        OutOfStock,
        UnderRepair,
        Blockaded,
    }

    /// <summary>可用性裁决结论。</summary>
    public enum D19AvailabilityVerdict
    {
        /// <summary>输入非法/冲突，保守拒绝。</summary>
        Rejected = 0,
        Available,
        Unavailable,
    }

    /// <summary>不可用/拒绝的稳定细目（不替代 L17 原因码；ReasonCode 才是消费方权威）。</summary>
    public enum D19UnavailabilityDetail
    {
        None = 0,
        /// <summary>该服务未登记在港口锚点目录（D19-L04）。</summary>
        NotRegistered,
        FacilityClosed,
        OutOfStock,
        UnderRepair,
        Blockaded,
        FactionDenied,
        WeatherClosed,
        TimeClosed,
        NotStaffed,
        /// <summary>未知/非法输入值（保守拒绝）。</summary>
        UnknownInput,
        /// <summary>输入组合自相矛盾（保守拒绝）。</summary>
        ConflictingInput,
    }

    /// <summary>稳定重试提示（逻辑提示，非 UI 文案；权威重试策略来自 L17 目录条目）。</summary>
    public enum D19RetryHint
    {
        None = 0,
        RetrySafe,
        RetryLaterSameDay,
        RetryNextPeriod,
        AfterCorrection,
        WhenStaffed,
        ResolveFactionPermission,
        AfterBlockadeLifted,
        AfterRepair,
    }

    /// <summary>服务可用性裁决结果（纯数据）。</summary>
    public sealed class D19ServiceAvailabilityResult
    {
        public D19AvailabilityVerdict Verdict;
        /// <summary>原因码：必须能由 D19PortFailureCatalog.Find 解析（来自 L17）。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>本地化 key：来自 D19PortLocalizationKeys 注册表（来自 L18）。</summary>
        public string LocKey = string.Empty;
        /// <summary>重试策略：来自 L17 目录条目。</summary>
        public D19FailureRetryPolicy RetryPolicy;
        /// <summary>稳定细目（见 D19UnavailabilityDetail）。</summary>
        public D19UnavailabilityDetail Detail;
        /// <summary>稳定重试提示（逻辑值，非显示文案）。</summary>
        public D19RetryHint RetryHint;
        public string ServiceId = string.Empty;
        public string PortId = string.Empty;

        public override string ToString()
        {
            return Verdict + ":" + ReasonCode + ":" + Detail;
        }
    }

    /// <summary>
    /// 港口服务可用性裁决器（D19-L05，所有者：本文件）。
    /// 纯静态函数 + 冻结规则表；无状态、无写路径、无 Unity 依赖。
    /// </summary>
    public static class D19PortAvailabilitySystem
    {
        // ---------- 冻结已知值词汇（与 L18 服务注册、L04 锚点目录一致） ----------

        /// <summary>L18 已注册服务的稳定 ID（唯一可裁决集合；其余服务 ID 保守拒绝）。</summary>
        public static readonly IReadOnlyList<string> KnownServiceIds = new List<string>
        {
            D19PortLocalizationKeys.ServiceIdMarket,
            D19PortLocalizationKeys.ServiceIdWarehouse,
            D19PortLocalizationKeys.ServiceIdTavern,
            D19PortLocalizationKeys.ServiceIdShipyard,
            D19PortLocalizationKeys.ServiceIdSupply,
        };

        /// <summary>露天/受天气影响的服务（Storm 关闭；Rain 仅关闭市场）。</summary>
        private static readonly HashSet<string> StormSensitiveServices = new HashSet<string>
        {
            D19PortLocalizationKeys.ServiceIdMarket,
            D19PortLocalizationKeys.ServiceIdSupply,
        };

        /// <summary>夜间关闭的服务（开放时间规则，来自建筑设计开放时间字段口径）。</summary>
        private static readonly HashSet<string> NightClosedServices = new HashSet<string>
        {
            D19PortLocalizationKeys.ServiceIdMarket,
            D19PortLocalizationKeys.ServiceIdWarehouse,
            D19PortLocalizationKeys.ServiceIdShipyard,
        };

        /// <summary>OutOfStock 仅对补货类服务有意义；其它服务声明缺货视为冲突输入。</summary>
        private static readonly HashSet<string> StockedServices = new HashSet<string>
        {
            D19PortLocalizationKeys.ServiceIdMarket,
            D19PortLocalizationKeys.ServiceIdSupply,
        };

        /// <summary>
        /// L18 服务 ID → L04 锚点目录服务键后缀（跨任务冻结对齐）。
        /// L18（D19-L18）冻结单数服务 ID（supply）；L04（D19-L04）按
        /// D9PortService 枚举冻结复数后缀（supplies）。本表是 L05 内部
        /// 的只读映射，不修改 L04/L18 文件；未知映射在 Validate 中报缺。
        /// </summary>
        private static readonly Dictionary<string, string> CatalogServiceSuffix =
            new Dictionary<string, string>
            {
                { D19PortLocalizationKeys.ServiceIdMarket, "market" },
                { D19PortLocalizationKeys.ServiceIdWarehouse, "warehouse" },
                { D19PortLocalizationKeys.ServiceIdTavern, "tavern" },
                { D19PortLocalizationKeys.ServiceIdShipyard, "shipyard" },
                { D19PortLocalizationKeys.ServiceIdSupply, "supplies" },
            };

        // ---------- 裁决入口（确定性：相同输入 → 相同结果） ----------

        public static D19ServiceAvailabilityResult Adjudicate(
            string portId,
            string serviceId,
            D19ServiceTimePeriod timePeriod,
            D19FactionPermission factionPermission,
            D19WeatherLabel weather,
            D19FacilityState facilityState,
            bool staffed)
        {
            var result = new D19ServiceAvailabilityResult
            {
                ServiceId = serviceId ?? string.Empty,
                PortId = portId ?? string.Empty,
            };

            // 1) 输入校验（保守拒绝：未知/非法值绝不静默成功）
            if (string.IsNullOrEmpty(portId) || string.IsNullOrEmpty(serviceId))
            {
                return Reject(result, D19UnavailabilityDetail.UnknownInput);
            }
            bool knownService = false;
            foreach (string known in KnownServiceIds)
            {
                if (known == serviceId)
                {
                    knownService = true;
                    break;
                }
            }
            if (!knownService)
            {
                return Reject(result, D19UnavailabilityDetail.UnknownInput);
            }
            if (!IsDefined(timePeriod) || !IsDefined(factionPermission) || !IsDefined(weather) || !IsDefined(facilityState))
            {
                return Reject(result, D19UnavailabilityDetail.UnknownInput);
            }

            // 2) 港口解析（L04 锚点目录：别名解析、未知港口拒绝）
            var portResolution = D19PortAnchorCatalog.Instance.ResolvePort(portId);
            if (!portResolution.Success)
            {
                result.PortId = portId;
                D19FailureEntry unknownPort;
                if (!D19PortFailureCatalog.TryResolve("d19.port.unknown_port", out unknownPort))
                {
                    return Reject(result, D19UnavailabilityDetail.UnknownInput);
                }
                result.Verdict = D19AvailabilityVerdict.Rejected;
                result.ReasonCode = unknownPort.Code;
                result.LocKey = unknownPort.LocKey;
                result.RetryPolicy = unknownPort.Retry;
                result.Detail = D19UnavailabilityDetail.UnknownInput;
                result.RetryHint = D19RetryHint.AfterCorrection;
                return result;
            }
            result.PortId = portResolution.CanonicalKey;

            // 3) 服务必须登记在该港（L04 锚点目录：port + service 键存在）
            string serviceSuffix;
            if (!CatalogServiceSuffix.TryGetValue(serviceId, out serviceSuffix))
            {
                return Reject(result, D19UnavailabilityDetail.UnknownInput);
            }
            string canonicalServiceKey = "service_" + portResolution.CanonicalKey.Substring("port_".Length) + "_" + serviceSuffix;
            bool registered = false;
            foreach (string key in D19PortAnchorCatalog.Instance.ServiceKeys)
            {
                if (key == canonicalServiceKey)
                {
                    registered = true;
                    break;
                }
            }
            if (!registered)
            {
                return Unavailable(result, D19UnavailabilityDetail.NotRegistered, D19RetryHint.AfterCorrection);
            }

            // 4) 冲突输入检测（保守拒绝）
            if (facilityState == D19FacilityState.OutOfStock)
            {
                bool isStocked = false;
                foreach (string stocked in StockedServices)
                {
                    if (stocked == serviceId)
                    {
                        isStocked = true;
                        break;
                    }
                }
                if (!isStocked)
                {
                    return Reject(result, D19UnavailabilityDetail.ConflictingInput);
                }
            }

            // 5) 优先级链（冻结、确定性：设施状态 → 势力 → 天气 → 时段 → 人手 → 可用）
            switch (facilityState)
            {
                case D19FacilityState.Closed:
                    return Unavailable(result, D19UnavailabilityDetail.FacilityClosed, D19RetryHint.RetryLaterSameDay);
                case D19FacilityState.UnderRepair:
                    return Unavailable(result, D19UnavailabilityDetail.UnderRepair, D19RetryHint.AfterRepair);
                case D19FacilityState.Blockaded:
                    return Unavailable(result, D19UnavailabilityDetail.Blockaded, D19RetryHint.AfterBlockadeLifted);
                case D19FacilityState.OutOfStock:
                    return Unavailable(result, D19UnavailabilityDetail.OutOfStock, D19RetryHint.RetryNextPeriod);
                case D19FacilityState.Open:
                case D19FacilityState.None:
                    break;
            }

            if (factionPermission == D19FactionPermission.Denied)
            {
                return Unavailable(result, D19UnavailabilityDetail.FactionDenied, D19RetryHint.ResolveFactionPermission);
            }

            bool stormSensitive = false;
            foreach (string sensitive in StormSensitiveServices)
            {
                if (sensitive == serviceId)
                {
                    stormSensitive = true;
                    break;
                }
            }
            if (weather == D19WeatherLabel.Storm && stormSensitive)
            {
                return Unavailable(result, D19UnavailabilityDetail.WeatherClosed, D19RetryHint.RetryNextPeriod);
            }
            if (weather == D19WeatherLabel.Rain && serviceId == D19PortLocalizationKeys.ServiceIdMarket)
            {
                return Unavailable(result, D19UnavailabilityDetail.WeatherClosed, D19RetryHint.RetryNextPeriod);
            }

            bool nightClosed = false;
            foreach (string closed in NightClosedServices)
            {
                if (closed == serviceId)
                {
                    nightClosed = true;
                    break;
                }
            }
            if (timePeriod == D19ServiceTimePeriod.Night && nightClosed)
            {
                return Unavailable(result, D19UnavailabilityDetail.TimeClosed, D19RetryHint.RetryNextPeriod);
            }

            if (!staffed)
            {
                return Unavailable(result, D19UnavailabilityDetail.NotStaffed, D19RetryHint.WhenStaffed);
            }

            // 6) 可用
            result.Verdict = D19AvailabilityVerdict.Available;
            result.ReasonCode = D19PortFailureCatalog.Find("d19.service.unavailable").Code;
            result.LocKey = D19PortLocalizationKeys.ReadModelServiceOpen;
            result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
            result.Detail = D19UnavailabilityDetail.None;
            result.RetryHint = D19RetryHint.None;
            return result;
        }

        // ---------- 内部构造 ----------

        private static D19ServiceAvailabilityResult Unavailable(
            D19ServiceAvailabilityResult result,
            D19UnavailabilityDetail detail,
            D19RetryHint retryHint)
        {
            D19FailureEntry serviceEntry;
            if (!D19PortFailureCatalog.TryResolve("d19.service.unavailable", out serviceEntry))
            {
                return Reject(result, D19UnavailabilityDetail.UnknownInput);
            }
            result.Verdict = D19AvailabilityVerdict.Unavailable;
            result.ReasonCode = serviceEntry.Code;
            result.LocKey = serviceEntry.LocKey;
            result.RetryPolicy = serviceEntry.Retry;
            result.Detail = detail;
            result.RetryHint = retryHint;
            return result;
        }

        private static D19ServiceAvailabilityResult Reject(
            D19ServiceAvailabilityResult result,
            D19UnavailabilityDetail detail)
        {
            D19FailureEntry serviceEntry;
            if (!D19PortFailureCatalog.TryResolve("d19.service.unavailable", out serviceEntry))
            {
                result.Verdict = D19AvailabilityVerdict.Rejected;
                result.ReasonCode = string.Empty;
                result.LocKey = D19PortLocalizationKeys.MissingKey;
                result.Detail = D19UnavailabilityDetail.UnknownInput;
                result.RetryPolicy = D19FailureRetryPolicy.NoRetry;
                result.RetryHint = D19RetryHint.None;
                return result;
            }
            result.Verdict = D19AvailabilityVerdict.Rejected;
            result.ReasonCode = serviceEntry.Code;
            result.LocKey = serviceEntry.LocKey;
            result.RetryPolicy = serviceEntry.Retry;
            result.Detail = detail;
            result.RetryHint = D19RetryHint.AfterCorrection;
            return result;
        }

        private static bool IsDefined<T>(T value) where T : struct, Enum
        {
            return Enum.IsDefined(typeof(T), value) && !Equals(value, default(T));
        }

        // ---------- 目录自检（确定性） ----------

        /// <summary>
        /// 自检：原因码全部可被 L17 解析、key 全部在 L18 注册表、冻结规则表非空、
        /// 已知服务 ID 全部对应 L18 服务 key。返回空列表表示健全。
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();

            foreach (string serviceId in KnownServiceIds)
            {
                string key;
                if (!D19PortLocalizationKeys.TryGetServiceKey(serviceId, out key))
                {
                    problems.Add("service without L18 key: " + serviceId);
                }
                else if (!D19PortLocalizationKeys.IsValidKey(key))
                {
                    problems.Add("invalid L18 service key: " + key);
                }
                if (!CatalogServiceSuffix.ContainsKey(serviceId))
                {
                    problems.Add("service without catalog suffix mapping: " + serviceId);
                }
            }

            D19FailureEntry serviceEntry;
            if (!D19PortFailureCatalog.TryResolve("d19.service.unavailable", out serviceEntry))
            {
                problems.Add("L17 d19.service.unavailable entry missing");
            }
            else
            {
                if (!D19PortLocalizationKeys.IsValidKey(serviceEntry.LocKey))
                {
                    problems.Add("L17 service entry loc key invalid: " + serviceEntry.LocKey);
                }
            }
            if (!D19PortFailureCatalog.TryResolve("d19.port.unknown_port", out _))
            {
                problems.Add("L17 d19.port.unknown_port entry missing");
            }
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.ReadModelServiceOpen))
            {
                problems.Add("L18 readmodel.service_open key invalid");
            }

            List<string> catalogProblems = D19PortFailureCatalog.Validate();
            foreach (string problem in catalogProblems)
            {
                problems.Add("L17: " + problem);
            }
            D19PortLocalizationKeys.D19KeyValidationResult keyResult = D19PortLocalizationKeys.ValidateRegistry();
            if (keyResult.HasErrors)
            {
                foreach (string key in keyResult.InvalidKeys)
                {
                    problems.Add("L18 invalid key: " + key);
                }
                foreach (string key in keyResult.DuplicateKeys)
                {
                    problems.Add("L18 duplicate key: " + key);
                }
                foreach (D19PortFailureCode code in keyResult.FailureCodesWithoutKeys)
                {
                    problems.Add("L18 failure code without key: " + code);
                }
            }

            problems.Sort(StringComparer.Ordinal);
            return problems;
        }
    }
}
