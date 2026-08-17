// ============================================================
// D19PortAuthorityValidator —— 港口权限校验器（纯 C#，D19-L23）
//
// 归属：Whalebone.Systems.D19（D19 唯一港口权限校验器，D19-L23）
// 目的：基于调用方明确传入的 actor_id / ship_id / port_id / service_id /
//       berth_id / 货物归属与授权声明，判定"谁能请求何种港口命令"。
// 边界（冻结）：
//   - 纯函数：相同输入永远得到相同允许/拒绝结果（可重放）；
//   - 只返回允许/拒绝 + D19-L17 失败码目录条目（code + LocKey）与稳定拒绝原因；
//   - 绝不"修正"归属、绝不创建授权、绝不查询网络、绝不读取当前登录状态
//     （船舶当前停靠港由调用方作为声明传入，校验器只做一致性比较）；
//   - 拒绝跨船、跨港、跨玩家与伪造服务访问；
//   - 无任何跨实体状态写入：本文件不持有可变状态，不写任何系统字段；
//   - 不引用 Unity（Transform/Canvas/Input/网络/LLM），不写任何中英文显示文案。
// 只读消费：
//   - D19PortAnchorCatalog（D19-L04）：port/berth/service 稳定键解析与跨港拒绝；
//   - D19PortFailureCatalog（D19-L17）：允许/拒绝结果只引用其条目（code+key）；
//   - D19PortLifecycleContracts（D19-L02）：D19PortCommandKind。
// 下游：D19-L11—L15 适配器在调用所有者（D3/D9/D4）之前调用 Validate。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 稳定拒绝原因（机器可读诊断；不是独立业务失败码，业务码由 L17 条目承载）。
    /// </summary>
    public enum D19DenialCause
    {
        /// <summary>占位：允许或非法。</summary>
        None = 0,

        /// <summary>请求/声明对象为空或非法。</summary>
        InvalidInput,

        /// <summary>缺少 actor_id。</summary>
        EmptyActor,

        /// <summary>缺少 ship_id。</summary>
        EmptyShip,

        /// <summary>缺少 port_id（非 Compensate 命令）。</summary>
        EmptyPort,

        /// <summary>AssignBerth 缺少 berth_id。</summary>
        MissingBerth,

        /// <summary>服务命令缺少 service_id。</summary>
        MissingService,

        /// <summary>actor 与 ship 无任何授权/成员关系（跨玩家访问）。</summary>
        CrossPlayer,

        /// <summary>请求船与声明船/货物归属船不一致（跨船访问）。</summary>
        CrossShip,

        /// <summary>请求港与声明停靠港不一致或未声明（跨港访问）。</summary>
        CrossPort,

        /// <summary>port_id 无法在锚点目录解析（→ L17 d19.port.unknown_port）。</summary>
        UnknownPort,

        /// <summary>berth_id 未知或种类不符。</summary>
        UnknownBerth,

        /// <summary>泊位归属其他港口（跨港）。</summary>
        CrossPortBerth,

        /// <summary>service_id 未知或种类不符。</summary>
        UnknownService,

        /// <summary>服务归属其他港口（跨港）。</summary>
        CrossPortService,

        /// <summary>非服务命令携带服务 ID（伪造服务访问）。</summary>
        ForgedService
    }

    /// <summary>
    /// 被校验的港口命令请求（D19-L23，调用方构造）。
    /// 全部字段为明确传入的事实；校验器不补猜、不读取任何运行时状态。
    /// </summary>
    public sealed class D19AuthorityContext
    {
        /// <summary>港口命令种类（D19-L02 D19PortCommandKind）。</summary>
        public D19PortCommandKind Kind;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId;

        /// <summary>目标港口稳定键（可用 D19-L04 别名）。</summary>
        public string PortId;

        /// <summary>目标泊位稳定键（仅 AssignBerth 必需；其它命令若携带必须属于请求港）。</summary>
        public string BerthId;

        /// <summary>目标服务稳定键（仅 StartService/CompleteService 必需且允许）。</summary>
        public string ServiceId;

        /// <summary>本请求触及的货物归属船 ID；空 = 不涉及货物。
        /// 非空时必须等于 ShipId（拒绝跨船货物操作）。</summary>
        public string CargoShipId;

        public D19AuthorityContext(D19PortCommandKind kind, string actorId, string shipId, string portId,
            string berthId = null, string serviceId = null, string cargoShipId = null)
        {
            Kind = kind;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            BerthId = berthId;
            ServiceId = serviceId;
            CargoShipId = cargoShipId;
        }
    }

    /// <summary>
    /// 授权声明（D19-L23，调用方明确传入；校验器绝不创建/修正这些声明）。
    /// </summary>
    public sealed class D19AuthorityClaims
    {
        /// <summary>声明所指向的船（必须等于请求 ShipId，否则跨船拒绝）。</summary>
        public string ShipId;

        /// <summary>声明：actor 是该船船主（玩家）。</summary>
        public bool ActorOwnsShip;

        /// <summary>声明：actor 是该船成员/船员。</summary>
        public bool ActorIsCrewOfShip;

        /// <summary>声明：该船当前停靠港口稳定键（校验器只做一致性比较，不读取登录/场景状态）。</summary>
        public string ShipCurrentPortId;

        public D19AuthorityClaims(string shipId, bool actorOwnsShip, bool actorIsCrewOfShip, string shipCurrentPortId)
        {
            ShipId = shipId ?? string.Empty;
            ActorOwnsShip = actorOwnsShip;
            ActorIsCrewOfShip = actorIsCrewOfShip;
            ShipCurrentPortId = shipCurrentPortId ?? string.Empty;
        }
    }

    /// <summary>
    /// 权限校验结果（D19-L23）。
    /// Allowed=false 时 Failure 为 D19-L17 目录条目（稳定 code + LocKey），
    /// Cause 为稳定拒绝原因；附带解析出的规范键供下游适配器使用。
    /// </summary>
    public sealed class D19AuthorityResult
    {
        /// <summary>是否允许。</summary>
        public bool Allowed;

        /// <summary>稳定拒绝原因（Allowed=true 时为 None）。</summary>
        public D19DenialCause Cause = D19DenialCause.None;

        /// <summary>L17 失败码目录条目（Allowed=false 时非空；code+key 唯一来源）。</summary>
        public D19FailureEntry Failure;

        /// <summary>解析后的规范港口键。</summary>
        public string CanonicalPortId = string.Empty;

        /// <summary>解析后的规范泊位键（未提供时为空）。</summary>
        public string CanonicalBerthId = string.Empty;

        /// <summary>解析后的规范服务键（未提供时为空）。</summary>
        public string CanonicalServiceId = string.Empty;

        /// <summary>L17 条目 LocKey（Allowed=false 时有效）。</summary>
        public string LocKey
        {
            get { return Failure == null ? string.Empty : Failure.LocKey; }
        }

        /// <summary>L17 条目业务码（Allowed=false 时有效）。</summary>
        public string FailureCode
        {
            get { return Failure == null ? string.Empty : Failure.Code; }
        }

        public static D19AuthorityResult Allow(D19AuthorityContext request, D19PortAnchorCatalog catalog)
        {
            return new D19AuthorityResult
            {
                Allowed = true,
                CanonicalPortId = request == null ? string.Empty : ResolveCanonicalPort(request.PortId, catalog),
                CanonicalBerthId = request == null ? string.Empty : ResolveCanonicalBerth(request.BerthId, catalog),
                CanonicalServiceId = request == null ? string.Empty : ResolveCanonicalService(request.ServiceId, catalog),
            };
        }

        public static D19AuthorityResult Deny(D19DenialCause cause)
        {
            return new D19AuthorityResult
            {
                Allowed = false,
                Cause = cause,
                Failure = D19PortAuthorityValidator.CauseToEntry(cause),
            };
        }

        private static string ResolveCanonicalPort(string portId, D19PortAnchorCatalog catalog)
        {
            if (string.IsNullOrEmpty(portId) || catalog == null)
            {
                return string.Empty;
            }
            D19AnchorResult result = catalog.ResolvePort(portId);
            return result.Success ? result.CanonicalKey : string.Empty;
        }

        private static string ResolveCanonicalBerth(string berthId, D19PortAnchorCatalog catalog)
        {
            if (string.IsNullOrEmpty(berthId) || catalog == null)
            {
                return string.Empty;
            }
            D19AnchorResult result = catalog.ResolveBerth(berthId);
            return result.Success ? result.CanonicalKey : string.Empty;
        }

        private static string ResolveCanonicalService(string serviceId, D19PortAnchorCatalog catalog)
        {
            if (string.IsNullOrEmpty(serviceId) || catalog == null)
            {
                return string.Empty;
            }
            D19AnchorResult result = catalog.ResolveService(serviceId);
            return result.Success ? result.CanonicalKey : string.Empty;
        }
    }

    /// <summary>
    /// 港口权限校验器（D19-L23）。纯函数、可重放、零状态写入。
    /// 下游适配器（L11—L15）必须在调用所有者（D3/D9/D4）之前调用 Validate。
    /// </summary>
    public static class D19PortAuthorityValidator
    {
        /// <summary>需要服务 ID 的命令种类（StartService/CompleteService）。</summary>
        public static readonly IReadOnlyList<D19PortCommandKind> ServiceCommands = new List<D19PortCommandKind>
        {
            D19PortCommandKind.StartService,
            D19PortCommandKind.CompleteService
        };

        /// <summary>需要泊位 ID 的命令种类（AssignBerth）。</summary>
        public static readonly IReadOnlyList<D19PortCommandKind> BerthCommands = new List<D19PortCommandKind>
        {
            D19PortCommandKind.AssignBerth
        };

        /// <summary>不需要港口上下文的命令种类（Compensate：回退自身命令）。</summary>
        public static readonly IReadOnlyList<D19PortCommandKind> PortlessCommands = new List<D19PortCommandKind>
        {
            D19PortCommandKind.Compensate
        };

        /// <summary>默认锚点目录（D19-L04 冻结数据）。</summary>
        public static D19AuthorityResult Validate(D19AuthorityContext request, D19AuthorityClaims claims)
        {
            return Validate(request, claims, D19PortAnchorCatalog.Instance);
        }

        /// <summary>
        /// 校验权限（显式目录重载，供确定性测试使用）。
        /// 返回允许或拒绝（含 L17 条目与稳定原因）；无任何副作用。
        /// </summary>
        public static D19AuthorityResult Validate(
            D19AuthorityContext request,
            D19AuthorityClaims claims,
            D19PortAnchorCatalog catalog)
        {
            if (request == null)
            {
                return D19AuthorityResult.Deny(D19DenialCause.InvalidInput);
            }

            // 1. 输入完整性
            if (string.IsNullOrEmpty(request.ActorId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.EmptyActor);
            }
            if (string.IsNullOrEmpty(request.ShipId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.EmptyShip);
            }
            bool portless = IsPortlessCommand(request.Kind);
            if (!portless && string.IsNullOrEmpty(request.PortId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.EmptyPort);
            }

            // 2. 授权声明一致性（跨船/跨玩家拒绝）
            bool hasClaims = claims != null;
            string claimedShip = hasClaims ? claims.ShipId : string.Empty;
            if (!string.IsNullOrEmpty(claimedShip)
                && !string.Equals(claimedShip, request.ShipId, StringComparison.Ordinal))
            {
                return D19AuthorityResult.Deny(D19DenialCause.CrossShip);
            }
            if (hasClaims && !claims.ActorOwnsShip && !claims.ActorIsCrewOfShip)
            {
                return D19AuthorityResult.Deny(D19DenialCause.CrossPlayer);
            }

            // 3. 港口解析（拒绝未知港口；L17 d19.port.unknown_port）
            string canonicalPort = string.Empty;
            if (!portless)
            {
                D19AnchorResult portResult = catalog == null
                    ? new D19AnchorResult { ErrorCode = D19AnchorErrorCodes.UnknownPortKey }
                    : catalog.ResolvePort(request.PortId);
                if (!portResult.Success)
                {
                    return D19AuthorityResult.Deny(D19DenialCause.UnknownPort);
                }
                canonicalPort = portResult.CanonicalKey;
            }

            // 4. 停靠港一致性（拒绝跨港；声明由调用方传入，校验器不读取登录状态）
            if (!portless)
            {
                string claimedLocation = hasClaims ? claims.ShipCurrentPortId : string.Empty;
                string canonicalLocation = string.Empty;
                if (!string.IsNullOrEmpty(claimedLocation) && catalog != null)
                {
                    D19AnchorResult locationResult = catalog.ResolvePort(claimedLocation);
                    canonicalLocation = locationResult.Success ? locationResult.CanonicalKey : string.Empty;
                }
                if (string.IsNullOrEmpty(canonicalLocation)
                    || !string.Equals(canonicalLocation, canonicalPort, StringComparison.Ordinal))
                {
                    return D19AuthorityResult.Deny(D19DenialCause.CrossPort);
                }
            }

            // 5. 泊位（AssignBerth 必需；其余命令携带时必须属于请求港）
            bool needsBerth = IsBerthCommand(request.Kind);
            if (needsBerth && string.IsNullOrEmpty(request.BerthId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.MissingBerth);
            }
            if (!string.IsNullOrEmpty(request.BerthId))
            {
                D19AnchorResult berthResult = catalog == null
                    ? new D19AnchorResult { ErrorCode = D19AnchorErrorCodes.UnknownBerthKey }
                    : catalog.ResolveBerth(request.BerthId, canonicalPort);
                if (!berthResult.Success)
                {
                    return D19AuthorityResult.Deny(berthResult.ErrorCode == D19AnchorErrorCodes.CrossPortBerth
                        ? D19DenialCause.CrossPortBerth
                        : D19DenialCause.UnknownBerth);
                }
            }

            // 6. 服务（仅服务命令允许且必需；其它命令携带服务 ID = 伪造服务访问）
            bool needsService = IsServiceCommand(request.Kind);
            if (needsService && string.IsNullOrEmpty(request.ServiceId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.MissingService);
            }
            if (!needsService && !string.IsNullOrEmpty(request.ServiceId))
            {
                return D19AuthorityResult.Deny(D19DenialCause.ForgedService);
            }
            if (!string.IsNullOrEmpty(request.ServiceId))
            {
                D19AnchorResult serviceResult = catalog == null
                    ? new D19AnchorResult { ErrorCode = D19AnchorErrorCodes.UnknownServiceKey }
                    : catalog.ResolveService(request.ServiceId, canonicalPort);
                if (!serviceResult.Success)
                {
                    return D19AuthorityResult.Deny(serviceResult.ErrorCode == D19AnchorErrorCodes.CrossPortService
                        ? D19DenialCause.CrossPortService
                        : D19DenialCause.UnknownService);
                }
            }

            // 7. 货物归属（拒绝跨船货物操作；声明由调用方传入）
            if (!string.IsNullOrEmpty(request.CargoShipId)
                && !string.Equals(request.CargoShipId, request.ShipId, StringComparison.Ordinal))
            {
                return D19AuthorityResult.Deny(D19DenialCause.CrossShip);
            }

            return D19AuthorityResult.Allow(request, catalog);
        }

        // ------------------------------------------------------------
        // 拒绝原因 → L17 失败码目录条目（唯一映射；UnknownPort 用 L17 未知港条目）
        // ------------------------------------------------------------

        /// <summary>拒绝原因到 L17 目录条目的唯一映射（UnknownPort 例外指向 d19.port.unknown_port）。</summary>
        public static D19FailureEntry CauseToEntry(D19DenialCause cause)
        {
            switch (cause)
            {
                case D19DenialCause.UnknownPort:
                    return D19PortFailureCatalog.Find("d19.port.unknown_port");
                default:
                    return D19PortFailureCatalog.Find("d19.port.permission_denied");
            }
        }

        /// <summary>
        /// 映射健全性自检（确定性）：每个非 None 拒绝原因都能解析到 L17 目录条目。
        /// 返回空列表表示健全；任何问题必须显式处理。
        /// </summary>
        public static List<string> VerifyMapping()
        {
            List<string> problems = new List<string>();
            foreach (D19DenialCause cause in Enum.GetValues(typeof(D19DenialCause)))
            {
                if (cause == D19DenialCause.None)
                {
                    continue;
                }
                D19FailureEntry entry = CauseToEntry(cause);
                if (entry == null)
                {
                    problems.Add("cause without L17 entry: " + cause);
                }
            }
            D19FailureEntry permission = D19PortFailureCatalog.Find("d19.port.permission_denied");
            if (permission == null)
            {
                problems.Add("L17 permission entry missing: d19.port.permission_denied");
            }
            D19FailureEntry unknownPort = D19PortFailureCatalog.Find("d19.port.unknown_port");
            if (unknownPort == null)
            {
                problems.Add("L17 unknown_port entry missing: d19.port.unknown_port");
            }
            problems.Sort(StringComparer.Ordinal);
            return problems;
        }

        // ------------------------------------------------------------
        // 命令种类判定（确定性）
        // ------------------------------------------------------------

        private static bool IsServiceCommand(D19PortCommandKind kind)
        {
            foreach (D19PortCommandKind candidate in ServiceCommands)
            {
                if (candidate == kind)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsBerthCommand(D19PortCommandKind kind)
        {
            foreach (D19PortCommandKind candidate in BerthCommands)
            {
                if (candidate == kind)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsPortlessCommand(D19PortCommandKind kind)
        {
            foreach (D19PortCommandKind candidate in PortlessCommands)
            {
                if (candidate == kind)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
