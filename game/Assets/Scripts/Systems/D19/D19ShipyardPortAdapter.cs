// ============================================================
// D19ShipyardPortAdapter —— 船坞港口适配器（纯 C#，D19-L14）
//
// 归属：Whalebone.Systems.D19（船坞服务的上下文/授权/命令适配，D19-L14）
// 目的：为港口船坞服务提供稳定的请求入口：先做港口服务可用性（D19-L05）
//       与权限（D19-L23）校验，通过后才调用 D4 的真实公开入口
//       （D4.D4ShipyardSystem.TryUpgrade / TryUnlockShip），透传 command_id，
//       并把 D4 结果转换为稳定结果（失败码/本地化 key/事件草案）。
// 裁决边界（冻结）：
//   - D4 对耐久、船体、改装、费用和资格保持唯一裁决：本适配器不写任何
//     船舶数值、不改 D4 状态、不复制 D4 的升级费用/声望门槛规则；
//   - 适配器只转换：结果形态（Accepted/Outcome）、失败码（L17 目录或
//     稳定 reason id）、本地化 key（L18 注册表）、事件草案（L16 工厂）；
//   - 不触碰 Transform/坐标/Prefab/模型/候选资源/网络/LLM；不写 UI 文案；
//   - 相同输入永远得到相同结果（确定性；命令 ID 幂等：重复投递只重放
//     既有结果，不产生第二个副作用、不重复调用 D4）。
// D4 真实公开入口确认（源码盘点，2026-08-16）：
//   - D4ShipyardSystem.TryUpgrade(D4UpgradeSlot slot, ref int money, int reputation) -> ShipyardResult
//   - D4ShipyardSystem.TryUnlockShip(string shipTypeId, int reputation, ref int money) -> ShipyardResult
//   - D4ShipyardSystem.Level(D4UpgradeSlot) / GetModifiers()（只读读模型）
//   - D4ShipCatalog.Find(string shipTypeId)（船型目录只读）
//   - ShipyardResult 字段：Accepted / Cost / NewLevel / ShipTypeId / Message；
//     Message 为显示文本，L17 已冻结"不映射、不编造"；
//   - D4 无公开"检修（耐久修复）"API：本适配器对 Repair 操作显式返回
//     UnsupportedModification（repair_unimplemented），诚实 BLOCKED。
// 下游：D19 港口编排（L33 路由器）/ L28 测试只读消费本适配器结果。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>船坞操作种类（只对应 D4 真实公开入口；Repair 为显式未实现）。</summary>
    public enum D19ShipyardOperation
    {
        /// <summary>占位：非法操作。</summary>
        None = 0,

        /// <summary>检修（耐久修复）。D4 无公开 API，恒为 UnsupportedModification（诚实 BLOCKED）。</summary>
        Repair,

        /// <summary>改装（D4.D4ShipyardSystem.TryUpgrade）。</summary>
        Upgrade,

        /// <summary>解锁船型（D4.D4ShipyardSystem.TryUnlockShip）。</summary>
        UnlockShip,
    }

    /// <summary>船坞适配结果的稳定细目（机器可读；不是独立 L17 业务码，FailureCode 才是权威）。</summary>
    public enum D19ShipyardOutcome
    {
        /// <summary>占位：非法。</summary>
        None = 0,

        /// <summary>D4 接受并应用（成功）。</summary>
        Accepted,

        /// <summary>重复请求：同一 command_id 命中既有结果，重放且无新副作用。</summary>
        DuplicateReplay,

        /// <summary>不支持的操作/改装（未知槽位、已满级、未知船型、检修未实现、非船坞服务）。</summary>
        UnsupportedModification,

        /// <summary>权限拒绝（D19-L23；L17 d19.port.permission_denied）。</summary>
        NotAuthorized,

        /// <summary>服务关闭/前置不满足（D19-L05 或生命周期前置；L17 d19.service.unavailable）。</summary>
        ServiceUnavailable,

        /// <summary>D4 底层拒绝（ShipyardResult.Accepted=false；D4 无稳定失败码，不编造原因）。</summary>
        UnderlyingRejected,

        /// <summary>命令缺少稳定 command_id（L02 MissingCommandId）。</summary>
        MissingCommandId,

        /// <summary>港口未知（L17 d19.port.unknown_port）。</summary>
        UnknownPort,

        /// <summary>请求本身非法（空请求/非法操作/错误服务 ID）。</summary>
        InvalidRequest,
    }

    /// <summary>
    /// 船坞服务协议留痕与稳定 reason id（D19-L14 冻结，只读）。
    /// reason id 全部为点分小写稳定标识，可入事件 payload 与机器留痕；
    /// 其中可对应 L17 条目的使用 L17 业务码，其余为适配器稳定细目。
    /// </summary>
    public static class D19ShipyardProtocol
    {
        /// <summary>本适配器受理的服务稳定 ID（L18 ServiceIdShipyard）。</summary>
        public const string ServiceId = "shipyard";

        /// <summary>成功事件 result_code（L16 要求 port. 前缀）。</summary>
        public const string ResultCodeApplied = "port.shipyard.applied";

        /// <summary>拒绝事件 result_code（L16 CreateRejection 固定）。</summary>
        public const string ResultCodeRejected = "port.rejected";

        public const string ReasonMissingCommandId = "d19.failure.missing_command_id";
        public const string ReasonDuplicateCommand = "d19.failure.duplicate_command";
        public const string ReasonPermissionDenied = "d19.port.permission_denied";
        public const string ReasonServiceUnavailable = "d19.service.unavailable";
        public const string ReasonUnknownPort = "d19.port.unknown_port";
        public const string ReasonInvalidRequest = "port.shipyard.invalid_request";
        public const string ReasonServiceMismatch = "port.shipyard.service_mismatch";
        public const string ReasonUnknownSlot = "port.shipyard.unknown_slot";
        public const string ReasonMaxLevel = "port.shipyard.max_level";
        public const string ReasonUnknownShipType = "port.shipyard.unknown_ship_type";
        public const string ReasonRepairUnimplemented = "port.shipyard.repair_unimplemented";
        public const string ReasonUnderlyingRejected = "port.shipyard.rejected";

        /// <summary>D4 真实公开入口留痕（源码盘点事实；缺失即诚实 BLOCKED）。</summary>
        public static readonly IReadOnlyList<string> D4ApiFacts = new List<string>
        {
            "D4ShipyardSystem.TryUpgrade(D4UpgradeSlot, ref int money, int reputation) -> ShipyardResult",
            "D4ShipyardSystem.TryUnlockShip(string shipTypeId, int reputation, ref int money) -> ShipyardResult",
            "D4ShipyardSystem.Level(D4UpgradeSlot) / GetModifiers() (read-only)",
            "D4ShipCatalog.Find(string shipTypeId) (read-only catalog)",
            "ShipyardResult fields: Accepted, Cost, NewLevel, ShipTypeId, Message",
            "D4 has NO public repair/durability API: Repair is unsupported (honest BLOCKED)",
            "D4 ShipyardResult has NO stable failure code: Accepted=false is mapped to UnderlyingRejected without inventing a reason",
        };
    }

    /// <summary>
    /// 船坞服务请求（D19-L14，调用方构造）。全部为明确传入的事实；
    /// 适配器不读取系统时钟/登录状态/NPC/资源；Money 与 Reputation 原样
    /// 透传给 D4（费用与资格的唯一裁决在 D4）。
    /// </summary>
    public sealed class D19ShipyardRequest
    {
        /// <summary>稳定命令 ID（幂等键；重复投递重放既有结果）。</summary>
        public string CommandId;

        /// <summary>船坞操作。</summary>
        public D19ShipyardOperation Operation;

        /// <summary>改装槽位（Upgrade 用；其余操作忽略）。</summary>
        public D4UpgradeSlot Slot;

        /// <summary>目标船型 ID（UnlockShip 用；其余操作忽略）。</summary>
        public string ShipTypeId;

        /// <summary>请求者携带的金钱（ref 语义：D4 成功后扣减，本适配器只报告 MoneyAfter）。</summary>
        public int Money;

        /// <summary>资格（势力声望；透传给 D4，不做阈值预判）。</summary>
        public int Reputation;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId;

        /// <summary>目标港口稳定键（可用 L04 别名）。</summary>
        public string PortId;

        /// <summary>目标服务稳定 ID（必须为 shipyard，否则 InvalidRequest）。</summary>
        public string ServiceId;

        /// <summary>逻辑世界日（事件逻辑时间）。</summary>
        public int WorldDay;

        /// <summary>存档修订号（事件去重）。</summary>
        public long SaveRevision;

        /// <summary>授权声明（D19-L23；调用方传入，校验器只做一致性比较）。</summary>
        public D19AuthorityClaims Claims;

        /// <summary>逻辑时间段（L05 可用性输入）。</summary>
        public D19ServiceTimePeriod TimePeriod;

        /// <summary>势力许可（L05 可用性输入）。</summary>
        public D19FactionPermission FactionPermission;

        /// <summary>天气逻辑标签（L05 可用性输入）。</summary>
        public D19WeatherLabel Weather;

        /// <summary>设施状态（L05 可用性输入）。</summary>
        public D19FacilityState FacilityState;

        /// <summary>人手标记（L05 可用性输入）。</summary>
        public bool Staffed;

        /// <summary>港口访问状态（生命周期前置：开始服务必须 Berthed）。</summary>
        public D19PortVisitState VisitState;

        /// <summary>事件 actor_ids（可选；不含对象引用）。</summary>
        public IList<string> ActorIds;
    }

    /// <summary>
    /// 船坞适配结果（纯数据；事件草案尚未落日志，由上层 EventLog 追加与去重）。
    /// Accepted=true 时 Cost/NewLevel/ShipTypeId 为 D4 稳定事实。
    /// </summary>
    public sealed class D19ShipyardAdapterResult
    {
        /// <summary>与请求一致的稳定命令 ID。</summary>
        public string CommandId;

        /// <summary>稳定细目（权威业务码见 FailureCode）。</summary>
        public D19ShipyardOutcome Outcome = D19ShipyardOutcome.None;

        /// <summary>命令是否成功（Outcome==Accepted）。</summary>
        public bool Accepted;

        /// <summary>稳定失败码：L17 业务码（可解析时）或稳定 reason id；空表示成功。</summary>
        public string FailureCode = string.Empty;

        /// <summary>稳定原因 id（点分小写；事件 payload 与机器留痕）。</summary>
        public string ReasonId = string.Empty;

        /// <summary>本地化 key（L18 注册表；MissingKey 哨兵=显式缺 key）。</summary>
        public string LocKey = string.Empty;

        /// <summary>重试建议（L17 策略；无目录条目时用保守建议）。</summary>
        public D19FailureRetryPolicy Retry;

        /// <summary>L02 生命周期子原因码（无对应时 None）。</summary>
        public D19PortFailureCode LifecycleCode = D19PortFailureCode.None;

        /// <summary>是否重复投递命中（重放既有结果，无新事件）。</summary>
        public bool DuplicateSubmission;

        /// <summary>D4 调用后的余额（稳定事实；适配器不写回任何系统）。</summary>
        public int MoneyAfter;

        /// <summary>D4 稳定事实：本次费用（Accepted 时有效）。</summary>
        public int Cost;

        /// <summary>D4 稳定事实：新等级（Upgrade Accepted 时有效）。</summary>
        public int NewLevel;

        /// <summary>D4 稳定事实：解锁船型（UnlockShip Accepted 时有效）。</summary>
        public string ShipTypeId = string.Empty;

        /// <summary>事件草案（ServiceResult 或 Rejection；重复命中时为空）。</summary>
        public WorldEvent EventDraft;
    }

    /// <summary>
    /// 船坞港口适配器（D19-L14）。实例按船绑定（构造传入 D4ShipyardSystem 实例），
    /// 与 D19PortVisitSystem（L03）同一幂等模式：_completedResults 按 command_id
    /// 缓存全部结果，重复投递只重放、不重复调用 D4、不产生第二个副作用。
    /// 纯逻辑：无 Unity、无网络、无写路径（Money 只读传入、结果报告）。
    /// </summary>
    public sealed class D19ShipyardPortAdapter
    {
        private readonly D4ShipyardSystem _shipyard;
        private readonly Dictionary<string, D19ShipyardAdapterResult> _completedResults =
            new Dictionary<string, D19ShipyardAdapterResult>();

        public D19ShipyardPortAdapter(D4ShipyardSystem shipyard)
        {
            _shipyard = shipyard ?? throw new ArgumentNullException(nameof(shipyard));
        }

        /// <summary>
        /// 执行船坞请求（确定性）。校验顺序冻结：
        /// command_id → 幂等命中 → 服务匹配 → 权限（L23）→ 可用性（L05）
        /// → 生命周期前置（L02，Berthed）→ 操作分派（D4 唯一裁决）。
        /// </summary>
        public D19ShipyardAdapterResult Execute(D19ShipyardRequest request)
        {
            // 1) 请求与命令 ID（L03 同一口径：空请求/空 command_id → MissingCommandId）
            if (request == null || string.IsNullOrEmpty(request.CommandId))
            {
                return Build(
                    request, D19ShipyardOutcome.MissingCommandId,
                    D19ShipyardProtocol.ReasonMissingCommandId,
                    D19PortFailureCode.MissingCommandId,
                    D19PortLocalizationKeys.FailureMissingCommandId,
                    D19FailureRetryPolicy.RetryAfterCorrection);
            }

            // 2) 幂等命中：重复投递只重放既有结果（无新事件、无 D4 调用）
            D19ShipyardAdapterResult cached;
            if (_completedResults.TryGetValue(request.CommandId, out cached))
            {
                D19ShipyardAdapterResult replay = CloneResult(cached);
                replay.DuplicateSubmission = true;
                return replay;
            }

            if (string.IsNullOrEmpty(request.PortId))
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.UnknownPort,
                    D19ShipyardProtocol.ReasonUnknownPort,
                    D19PortFailureCode.UnknownPort,
                    D19PortLocalizationKeys.FailureUnknownPort,
                    D19FailureRetryPolicy.RetryAfterCorrection));
            }

            // 3) 服务匹配：本适配器只受理 shipyard 服务
            if (!string.Equals(request.ServiceId, D19ShipyardProtocol.ServiceId, StringComparison.Ordinal))
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.InvalidRequest,
                    D19ShipyardProtocol.ReasonServiceMismatch,
                    D19PortFailureCode.None,
                    D19PortLocalizationKeys.MissingKey,
                    D19FailureRetryPolicy.RetryAfterCorrection));
            }

            // 4) 操作种类合法性
            if (!IsDefinedOperation(request.Operation))
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.InvalidRequest,
                    D19ShipyardProtocol.ReasonInvalidRequest,
                    D19PortFailureCode.None,
                    D19PortLocalizationKeys.MissingKey,
                    D19FailureRetryPolicy.RetryAfterCorrection));
            }

            // 5) 港口解析 + 权限（D19-L23）：船/港/服务/货物归属与跨实体拒绝；
            //    只有授权通过才允许触达 D4。
            //    服务键按 L05 冻结口径构造规范键 service_<port>_<service>
            //    （锚点目录只登记完整键，L18/L05 层使用裸服务 ID "shipyard"）。
            D19AnchorResult portResolution = D19PortAnchorCatalog.Instance.ResolvePort(request.PortId);
            if (!portResolution.Success)
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.UnknownPort,
                    D19ShipyardProtocol.ReasonUnknownPort,
                    D19PortFailureCode.UnknownPort,
                    D19PortLocalizationKeys.FailureUnknownPort,
                    D19FailureRetryPolicy.RetryAfterCorrection));
            }
            string canonicalServiceKey = "service_"
                + portResolution.CanonicalKey.Substring("port_".Length)
                + "_" + request.ServiceId;
            D19AuthorityContext authorityRequest = new D19AuthorityContext(
                D19PortCommandKind.StartService,
                request.ActorId,
                request.ShipId,
                request.PortId,
                null,
                canonicalServiceKey,
                null);
            D19AuthorityResult authority = D19PortAuthorityValidator.Validate(
                authorityRequest, request.Claims, D19PortAnchorCatalog.Instance);
            if (!authority.Allowed)
            {
                D19FailureEntry entry = authority.Failure;
                D19ShipyardOutcome outcome = authority.Cause == D19DenialCause.UnknownPort
                    ? D19ShipyardOutcome.UnknownPort
                    : D19ShipyardOutcome.NotAuthorized;
                return Complete(request, Build(
                    request, outcome,
                    entry != null && !string.IsNullOrEmpty(entry.Code) ? entry.Code : D19ShipyardProtocol.ReasonPermissionDenied,
                    D19PortFailureCode.None,
                    entry != null ? entry.LocKey : D19PortLocalizationKeys.MissingKey,
                    entry != null ? entry.Retry : D19FailureRetryPolicy.RetryAfterCorrection));
            }

            // 6) 服务可用性（D19-L05）：设施/势力/天气/时段/人手/登记。
            D19ServiceAvailabilityResult availability = D19PortAvailabilitySystem.Adjudicate(
                request.PortId,
                D19ShipyardProtocol.ServiceId,
                request.TimePeriod,
                request.FactionPermission,
                request.Weather,
                request.FacilityState,
                request.Staffed);
            if (availability.Verdict != D19AvailabilityVerdict.Available)
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.ServiceUnavailable,
                    !string.IsNullOrEmpty(availability.ReasonCode)
                        ? availability.ReasonCode
                        : D19ShipyardProtocol.ReasonServiceUnavailable,
                    D19PortFailureCode.ServiceUnavailable,
                    !string.IsNullOrEmpty(availability.LocKey)
                        ? availability.LocKey
                        : D19PortLocalizationKeys.FailureServiceUnavailable,
                    availability.RetryPolicy != D19FailureRetryPolicy.None
                        ? availability.RetryPolicy
                        : D19FailureRetryPolicy.RetrySafe));
            }

            // 7) 生命周期前置（L02 冻结迁移表：StartService 仅由 Berthed 进入）
            D19PortVisitState target;
            D19PortFailureCode lifecycleFailure;
            if (!D19PortLifecycleContracts.CanTransition(
                    request.VisitState,
                    D19PortTransitionReason.ServiceStarted,
                    out target,
                    out lifecycleFailure))
            {
                return Complete(request, Build(
                    request, D19ShipyardOutcome.ServiceUnavailable,
                    D19ShipyardProtocol.ReasonServiceUnavailable,
                    D19PortFailureCode.ServiceUnavailable,
                    D19PortLocalizationKeys.FailureServiceUnavailable,
                    D19FailureRetryPolicy.RetrySafe));
            }

            // 8) 操作分派：D4 是耐久/船体/改装/费用/资格的唯一裁决者。
            //    适配器只做转换；Money 以局部副本透传（ref），结果报告 MoneyAfter，
            //    绝不写回任何系统。
            int money = request.Money;
            switch (request.Operation)
            {
                case D19ShipyardOperation.Repair:
                    // D4 无公开检修 API：诚实 BLOCKED，绝不伪造检修结果。
                    return Complete(request, Build(
                        request, D19ShipyardOutcome.UnsupportedModification,
                        D19ShipyardProtocol.ReasonRepairUnimplemented,
                        D19PortFailureCode.None,
                        D19PortLocalizationKeys.MissingKey,
                        D19FailureRetryPolicy.NoRetry));

                case D19ShipyardOperation.Upgrade:
                    return Complete(request, DispatchUpgrade(request, ref money));

                case D19ShipyardOperation.UnlockShip:
                    return Complete(request, DispatchUnlockShip(request, ref money));

                default:
                    return Complete(request, Build(
                        request, D19ShipyardOutcome.InvalidRequest,
                        D19ShipyardProtocol.ReasonInvalidRequest,
                        D19PortFailureCode.None,
                        D19PortLocalizationKeys.MissingKey,
                        D19FailureRetryPolicy.RetryAfterCorrection));
            }
        }

        // ------------------------------------------------------------
        // 操作分派（D4 唯一裁决；前置只用 D4 公开只读 API，不复制 D4 规则）
        // ------------------------------------------------------------

        private D19ShipyardAdapterResult DispatchUpgrade(D19ShipyardRequest request, ref int money)
        {
            // 未知槽位：拒绝触达 D4（枚举未定义值 = 不支持）。
            if (!Enum.IsDefined(typeof(D4UpgradeSlot), request.Slot))
            {
                return Build(
                    request, D19ShipyardOutcome.UnsupportedModification,
                    D19ShipyardProtocol.ReasonUnknownSlot,
                    D19PortFailureCode.None,
                    D19PortLocalizationKeys.MissingKey,
                    D19FailureRetryPolicy.RetryAfterCorrection);
            }
            // 已满级：读取 D4 公开只读读模型（Level），不调用 D4、不复制费用/声望规则。
            if (_shipyard.Level(request.Slot) >= 3)
            {
                return Build(
                    request, D19ShipyardOutcome.UnsupportedModification,
                    D19ShipyardProtocol.ReasonMaxLevel,
                    D19PortFailureCode.None,
                    D19PortLocalizationKeys.MissingKey,
                    D19FailureRetryPolicy.RetryAfterCorrection);
            }
            ShipyardResult result = _shipyard.TryUpgrade(request.Slot, ref money, request.Reputation);
            if (!result.Accepted)
            {
                return BuildUnderlyingRejected(request, money);
            }
            D19ShipyardAdapterResult success = Build(
                request, D19ShipyardOutcome.Accepted, string.Empty,
                D19PortFailureCode.None, string.Empty, D19FailureRetryPolicy.NoRetry);
            success.MoneyAfter = money;
            success.Cost = result.Cost;
            success.NewLevel = result.NewLevel;
            success.EventDraft = BuildSuccessEvent(request, success);
            return success;
        }

        private D19ShipyardAdapterResult DispatchUnlockShip(D19ShipyardRequest request, ref int money)
        {
            // 未知船型：D4 公开目录只读预检，不调用 D4。
            if (D4ShipCatalog.Find(request.ShipTypeId ?? string.Empty) == null)
            {
                return Build(
                    request, D19ShipyardOutcome.UnsupportedModification,
                    D19ShipyardProtocol.ReasonUnknownShipType,
                    D19PortFailureCode.None,
                    D19PortLocalizationKeys.MissingKey,
                    D19FailureRetryPolicy.RetryAfterCorrection);
            }
            ShipyardResult result = _shipyard.TryUnlockShip(request.ShipTypeId, request.Reputation, ref money);
            if (!result.Accepted)
            {
                return BuildUnderlyingRejected(request, money);
            }
            D19ShipyardAdapterResult success = Build(
                request, D19ShipyardOutcome.Accepted, string.Empty,
                D19PortFailureCode.None, string.Empty, D19FailureRetryPolicy.NoRetry);
            success.MoneyAfter = money;
            success.Cost = result.Cost;
            success.ShipTypeId = result.ShipTypeId ?? string.Empty;
            success.EventDraft = BuildSuccessEvent(request, success);
            return success;
        }

        /// <summary>
        /// D4 底层拒绝（Accepted=false）。D4 无稳定失败码（L17 冻结事实），
        /// 适配器不编造资金/声望/等级原因：只报告稳定细目 UnderlyingRejected。
        /// </summary>
        private static D19ShipyardAdapterResult BuildUnderlyingRejected(D19ShipyardRequest request, int moneyAfter)
        {
            D19ShipyardAdapterResult rejected = Build(
                request, D19ShipyardOutcome.UnderlyingRejected,
                D19ShipyardProtocol.ReasonUnderlyingRejected,
                D19PortFailureCode.None,
                D19PortLocalizationKeys.MissingKey,
                D19FailureRetryPolicy.RetryAfterCorrection);
            rejected.MoneyAfter = moneyAfter;
            rejected.EventDraft = BuildRejectionEvent(request, rejected.ReasonId);
            return rejected;
        }

        // ------------------------------------------------------------
        // 结果构造与事件草案
        // ------------------------------------------------------------

        private static D19ShipyardAdapterResult Build(
            D19ShipyardRequest request,
            D19ShipyardOutcome outcome,
            string failureCode,
            D19PortFailureCode lifecycleCode,
            string locKey,
            D19FailureRetryPolicy retry)
        {
            return new D19ShipyardAdapterResult
            {
                CommandId = request != null ? request.CommandId : string.Empty,
                Outcome = outcome,
                Accepted = outcome == D19ShipyardOutcome.Accepted,
                FailureCode = failureCode ?? string.Empty,
                ReasonId = failureCode ?? string.Empty,
                LifecycleCode = lifecycleCode,
                LocKey = locKey ?? string.Empty,
                Retry = retry,
                MoneyAfter = request != null ? request.Money : 0,
            };
        }

        /// <summary>成功事件草案（L16 工厂；payload 只含稳定标量事实，无显示文案）。</summary>
        private WorldEvent BuildSuccessEvent(D19ShipyardRequest request, D19ShipyardAdapterResult result)
        {
            var values = new Dictionary<string, string>();
            if (request.Operation == D19ShipyardOperation.Upgrade)
            {
                values["shipyard.slot"] = request.Slot.ToString().ToLowerInvariant();
                values["shipyard.cost"] = result.Cost.ToString();
                values["shipyard.new_level"] = result.NewLevel.ToString();
            }
            else if (request.Operation == D19ShipyardOperation.UnlockShip)
            {
                values["shipyard.ship_type"] = result.ShipTypeId;
                values["shipyard.cost"] = result.Cost.ToString();
            }
            return D19PortEventFactory.CreateServiceResult(
                request.CommandId, request.WorldDay, request.SaveRevision,
                request.PortId, request.ShipId, D19ShipyardProtocol.ServiceId,
                D19ShipyardProtocol.ResultCodeApplied,
                request.ActorIds, null,
                D19PortLocalizationKeys.PortEventServiceResult, null, values);
        }

        /// <summary>拒绝事件草案（L16 CreateRejection：port.reject_reason 存稳定 reason id）。</summary>
        private static WorldEvent BuildRejectionEvent(D19ShipyardRequest request, string reasonId)
        {
            return D19PortEventFactory.CreateRejection(
                request.CommandId, request.WorldDay, request.SaveRevision,
                request.PortId, request.ShipId, reasonId, "", request.ActorIds, "");
        }

        /// <summary>缓存并返回结果（重复投递只重放，不重复产生事件/副作用）。</summary>
        private D19ShipyardAdapterResult Complete(
            D19ShipyardRequest request,
            D19ShipyardAdapterResult result)
        {
            _completedResults[request.CommandId] = result;
            return result;
        }

        private static D19ShipyardAdapterResult CloneResult(D19ShipyardAdapterResult source)
        {
            D19ShipyardAdapterResult clone = new D19ShipyardAdapterResult
            {
                CommandId = source.CommandId,
                Outcome = source.Outcome,
                Accepted = source.Accepted,
                FailureCode = source.FailureCode,
                ReasonId = source.ReasonId,
                LocKey = source.LocKey,
                Retry = source.Retry,
                LifecycleCode = source.LifecycleCode,
                MoneyAfter = source.MoneyAfter,
                Cost = source.Cost,
                NewLevel = source.NewLevel,
                ShipTypeId = source.ShipTypeId,
                EventDraft = null, // 重放不产生新事件草案
            };
            return clone;
        }

        private static bool IsDefinedOperation(D19ShipyardOperation operation)
        {
            return operation == D19ShipyardOperation.Repair
                || operation == D19ShipyardOperation.Upgrade
                || operation == D19ShipyardOperation.UnlockShip;
        }

        // ------------------------------------------------------------
        // 可用性自检（确定性；"完成可用检查"）
        // ------------------------------------------------------------

        /// <summary>
        /// 自检：L17 目录条目/重试策略可解析、L18 key 合法、L23 映射健全、
        /// 事件 key 合规、D4 入口留痕齐全。返回空列表表示健全。
        /// </summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();

            D19FailureEntry permission = D19PortFailureCatalog.Find("d19.port.permission_denied");
            if (permission == null)
            {
                problems.Add("L17 d19.port.permission_denied entry missing");
            }
            else
            {
                if (!D19PortLocalizationKeys.IsValidKey(permission.LocKey))
                {
                    problems.Add("L17 permission loc key invalid: " + permission.LocKey);
                }
                if (permission.Retry == D19FailureRetryPolicy.None)
                {
                    problems.Add("L17 permission retry policy is None");
                }
            }
            D19FailureEntry service = D19PortFailureCatalog.Find("d19.service.unavailable");
            if (service == null)
            {
                problems.Add("L17 d19.service.unavailable entry missing");
            }
            D19FailureEntry unknownPort = D19PortFailureCatalog.Find("d19.port.unknown_port");
            if (unknownPort == null)
            {
                problems.Add("L17 d19.port.unknown_port entry missing");
            }
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.FailureMissingCommandId)
                || !D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.FailureDuplicateCommand)
                || !D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.FailureServiceUnavailable))
            {
                problems.Add("L18 failure key invalid");
            }
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.ServiceShipyard))
            {
                problems.Add("L18 service key invalid: " + D19PortLocalizationKeys.ServiceShipyard);
            }
            if (!D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.PortEventServiceResult)
                || !D19PortLocalizationKeys.IsValidKey(D19PortLocalizationKeys.PortEventRejection))
            {
                problems.Add("L18 port event key invalid");
            }
            if (D19PortLocalizationKeys.PortEventServiceResult != "d19.port.event.service_result"
                && !D19PortEventSchema.LocalizationKeyPattern.IsMatch(D19PortLocalizationKeys.PortEventServiceResult))
            {
                problems.Add("L18 service result key not L16 d19.port.* compatible");
            }
            if (D19ShipyardProtocol.D4ApiFacts == null || D19ShipyardProtocol.D4ApiFacts.Count == 0)
            {
                problems.Add("D4 api facts missing");
            }
            if (D19ShipyardProtocol.ResultCodeApplied != "port.shipyard.applied"
                || !D19ShipyardProtocol.ResultCodeApplied.StartsWith("port.", StringComparison.Ordinal))
            {
                problems.Add("adapter result code must start with port.");
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
            List<string> mappingProblems = D19PortAuthorityValidator.VerifyMapping();
            foreach (string problem in mappingProblems)
            {
                problems.Add("L23: " + problem);
            }

            problems.Sort(StringComparer.Ordinal);
            return problems;
        }
    }
}
