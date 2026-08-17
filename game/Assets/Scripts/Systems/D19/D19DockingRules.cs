// ============================================================
// D19DockingRules —— 靠泊前置规则的纯校验（纯 C#，D19-L07）
//
// 归属：Whalebone.Systems.D19（D19 靠泊裁决规则，D19-L07）
// 目的：靠泊（AssignBerth）前置的确定性纯校验：港口访问状态、船只逻辑状态、
//       泊位能力/归属、服务可用性、权限与非法状态防护全部通过，才产生
//       "允许靠泊"裁决。失败必须保持所有状态不变并给出 D19-L17 原因。
// 边界：
//   - 只读消费：L03 D19PortVisitSystem（State/PortId/BerthId 快照）、
//     L05 D19PortAvailabilitySystem.Adjudicate 结果、L06 D19BerthSystem
//     分配/能力只读数据（GetAllocation/GetAllocations/TryGetCapability）
//     与 Reserve 命令、L23 D19PortAuthorityValidator、L24 D19InvalidStateGuard；
//     不修改上述任何文件；
//   - 绝不实现物理碰撞、距离、Transform、NavMesh 或自动移动船只；
//   - 不写任何状态：本系统只裁决；唯一的写路径是 AdjudicateReservation
//     在裁决通过后消费 L06 的 Reserve 命令（泊位状态由 L06 拥有并先校验后变更）；
//   - 不引用 Unity（Transform/Canvas/Input/网络/LLM）；不写显示文案。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 靠泊请求事实（D19-L07，调用方明确传入；本系统不读取任何运行时状态）。
    /// 分配事实来自 L06 只读接口；服务可用性来自 L05 裁决结果；
    /// 授权声明来自 L23 的输入约定。
    /// </summary>
    public sealed class D19DockingRequest
    {
        /// <summary>稳定命令 ID（跨重试不变）。</summary>
        public string CommandId = string.Empty;

        /// <summary>目标船稳定键。</summary>
        public string ShipId = string.Empty;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>目标港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>目标泊位稳定键。</summary>
        public string BerthId = string.Empty;

        /// <summary>船只逻辑档案（L06 类型：sizeTier / deepDraft）。</summary>
        public D19BerthShipProfile ShipProfile;

        /// <summary>港口访问状态快照（L03 只读 State）。</summary>
        public D19PortVisitState VisitState;

        /// <summary>目标泊位当前分配（L06 GetAllocation；null = 无记录/空闲）。</summary>
        public D19BerthAllocation RequestedBerthAllocation;

        /// <summary>该船当前持有的泊位分配（L06 只读；null = 无事实）。</summary>
        public D19BerthAllocation ShipCurrentAllocation;

        /// <summary>泊位设施可用性裁决（L05；null = 无该事实，跳过该因子）。</summary>
        public D19ServiceAvailabilityResult FacilityAvailability;

        /// <summary>授权声明（L23 输入；null = 无事实，跳过权限因子）。</summary>
        public D19AuthorityClaims AuthorityClaims;
    }

    /// <summary>
    /// 靠泊裁决结果（D19-L07）。Allowed=false 时 BusinessCode/LocKey/Retry
    /// 来自 L17 目录条目，ReasonId 为精确稳定子原因；不泄露任何对象。
    /// </summary>
    public sealed class D19DockingVerdict
    {
        /// <summary>是否允许靠泊。</summary>
        public bool Allowed;

        /// <summary>L17 业务失败码（Allowed=false 时非空）。</summary>
        public string BusinessCode = string.Empty;

        /// <summary>L18 本地化 key（Allowed=false 时非空）。</summary>
        public string LocKey = string.Empty;

        /// <summary>重试建议（L17 条目）。</summary>
        public D19FailureRetryPolicy Retry;

        /// <summary>精确稳定子原因（小写蛇形）。</summary>
        public string ReasonId = string.Empty;

        /// <summary>解析后的规范港口键。</summary>
        public string CanonicalPortId = string.Empty;

        /// <summary>解析后的规范泊位键。</summary>
        public string CanonicalBerthId = string.Empty;

        public static D19DockingVerdict Allow(string canonicalPortId, string canonicalBerthId)
        {
            D19DockingVerdict verdict = new D19DockingVerdict();
            verdict.Allowed = true;
            verdict.CanonicalPortId = canonicalPortId ?? string.Empty;
            verdict.CanonicalBerthId = canonicalBerthId ?? string.Empty;
            return verdict;
        }
    }

    /// <summary>
    /// 预留执行结果（D19-L07）：裁决通过后消费 L06 Reserve 命令的结果；
    /// 裁决未通过时 ReserveResult 为 null（L06 未被调用，状态不变）。
    /// </summary>
    public sealed class D19DockReservationOutcome
    {
        public D19DockingVerdict Verdict;
        public D19BerthResult ReserveResult;
    }

    /// <summary>
    /// 靠泊前置规则（D19-L07，所有者：本规则系统）。
    /// 确定性裁决管道（首个失败即返回，顺序固定）：命令 ID → 非法状态防护
    /// （港口/泊位归属）→ 港口访问状态 → 船只逻辑状态 → 服务可用性 → 权限
    /// → 船型能力 → 泊位状态 → 状态组合一致性。全部通过才允许靠泊。
    /// 失败保持所有状态不变；本系统不实现任何物理/移动逻辑。
    /// </summary>
    public static class D19DockingRules
    {
        // ---------- 稳定子原因（小写蛇形，可入事件/存档/日志） ----------

        public const string ReasonMissingCommandId = "missing_command_id";
        public const string ReasonInvalidCommandId = "invalid_command_id";
        public const string ReasonNotInPort = "not_in_port";
        public const string ReasonNotMoored = "not_moored";
        public const string ReasonShipAlreadyBerthed = "ship_already_berthed";
        public const string ReasonDuplicateRequest = "duplicate_request";
        public const string ReasonCapabilityMissing = "capability_missing";
        public const string ReasonShipTierIncompatible = "ship_tier_incompatible";
        public const string ReasonDraftIncompatible = "draft_incompatible";
        public const string ReasonReservedByOther = "reserved_by_other";
        public const string ReasonOccupied = "occupied";
        public const string ReasonFacilityClosed = "facility_closed";
        public const string ReasonAuthorityDenied = "authority_denied";
        public const string ReasonUnknownPort = "unknown_port";

        /// <summary>泊位域业务码（L17 已注册；子原因由 ReasonId 精确保留）。</summary>
        private const string BerthCode = "d19.berth.occupied";

        // ------------------------------------------------------------
        // 裁决入口（纯校验，无写路径）
        // ------------------------------------------------------------

        /// <summary>
        /// 靠泊前置确定性裁决。相同输入 → 相同结果；首个失败因子即返回；
        /// 全部通过返回 Allowed=true。任何失败不改变任何状态。
        /// </summary>
        public static D19DockingVerdict Adjudicate(D19DockingRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // 1) 命令 ID（L24）
            D19GuardVerdict commandId = D19InvalidStateGuard.CheckCommandId(request.CommandId);
            if (!commandId.Allowed)
            {
                return FromGuard(commandId);
            }

            // 2) 港口/泊位归属（L24 封装 L04）
            D19GuardVerdict port = D19InvalidStateGuard.CheckPort(D19PortAnchorCatalog.Instance, request.PortId);
            if (!port.Allowed)
            {
                return FromGuard(port);
            }
            D19GuardVerdict berth = D19InvalidStateGuard.CheckBerth(D19PortAnchorCatalog.Instance, request.BerthId, request.PortId);
            if (!berth.Allowed)
            {
                return FromGuard(berth);
            }

            // 3) 港口访问状态（L03 快照；靠泊仅允许 Docked / Returned）
            D19DockingVerdict visit = CheckVisitState(request.VisitState);
            if (visit != null)
            {
                return visit;
            }

            // 4) 船只逻辑状态：本船已持泊位 → 拒绝（同一泊位 = 重复请求）
            if (request.ShipCurrentAllocation != null)
            {
                bool sameBerth = string.Equals(request.ShipCurrentAllocation.BerthId, request.BerthId, StringComparison.Ordinal);
                return Reject(BerthCode, sameBerth ? ReasonDuplicateRequest : ReasonShipAlreadyBerthed,
                    D19PortFailureCode.BerthOccupied, request.BerthId, request.PortId,
                    sameBerth ? D19FailureRetryPolicy.NoRetry : D19FailureRetryPolicy.RetryAfterCorrection);
            }

            // 5) 服务可用性（L05 裁决结果；无事实则跳过）
            if (request.FacilityAvailability != null &&
                request.FacilityAvailability.Verdict != D19AvailabilityVerdict.Available)
            {
                D19DockingVerdict availability = new D19DockingVerdict();
                availability.Allowed = false;
                availability.BusinessCode = string.IsNullOrEmpty(request.FacilityAvailability.ReasonCode)
                    ? "d19.service.unavailable" : request.FacilityAvailability.ReasonCode;
                availability.LocKey = string.IsNullOrEmpty(request.FacilityAvailability.LocKey)
                    ? D19PortLocalizationKeys.FailureServiceUnavailable : request.FacilityAvailability.LocKey;
                availability.Retry = request.FacilityAvailability.RetryPolicy;
                availability.ReasonId = SnakeEnum(request.FacilityAvailability.Detail);
                return availability;
            }

            // 6) 权限（L23）
            if (request.AuthorityClaims != null)
            {
                D19AuthorityContext context = new D19AuthorityContext(
                    D19PortCommandKind.AssignBerth, request.ActorId, request.ShipId,
                    request.PortId, request.BerthId);
                D19AuthorityResult authority = D19PortAuthorityValidator.Validate(context, request.AuthorityClaims, D19PortAnchorCatalog.Instance);
                if (!authority.Allowed)
                {
                    D19DockingVerdict denied = new D19DockingVerdict();
                    denied.Allowed = false;
                    denied.BusinessCode = authority.Failure == null ? "d19.port.permission_denied" : authority.Failure.Code;
                    denied.LocKey = authority.Failure == null
                        ? D19PortFailureCatalog.KeyPermissionDenied : authority.Failure.LocKey;
                    denied.Retry = authority.Failure == null
                        ? D19FailureRetryPolicy.RetryAfterCorrection : authority.Failure.Retry;
                    denied.ReasonId = ReasonAuthorityDenied + "." + SnakeEnum(authority.Cause);
                    return denied;
                }
            }

            // 7) 船型能力（L06 冻结能力表）
            D19BerthCapability capability;
            if (!D19BerthSystem.TryGetCapability(request.BerthId, out capability))
            {
                return Reject(BerthCode, ReasonCapabilityMissing, D19PortFailureCode.BerthOccupied,
                    request.BerthId, request.PortId, D19FailureRetryPolicy.RetryAfterCorrection);
            }
            D19BerthShipProfile ship = request.ShipProfile;
            if (ship != null)
            {
                if (ship.SizeTier < capability.MinTier || ship.SizeTier > capability.MaxTier)
                {
                    return Reject(BerthCode, ReasonShipTierIncompatible, D19PortFailureCode.BerthOccupied,
                        request.BerthId, request.PortId, D19FailureRetryPolicy.RetryAfterCorrection);
                }
                if (capability.DeepDraft && !ship.DeepDraft)
                {
                    return Reject(BerthCode, ReasonDraftIncompatible, D19PortFailureCode.BerthOccupied,
                        request.BerthId, request.PortId, D19FailureRetryPolicy.RetryAfterCorrection);
                }
            }

            // 8) 泊位状态（L06 分配事实；可用才允许）
            D19BerthAllocation allocation = request.RequestedBerthAllocation;
            if (allocation != null && allocation.State != D19BerthState.Available)
            {
                bool bySelf = string.Equals(allocation.ShipId, request.ShipId, StringComparison.Ordinal);
                if (allocation.State == D19BerthState.Reserved)
                {
                    return Reject(BerthCode, bySelf ? ReasonDuplicateRequest : ReasonReservedByOther,
                        D19PortFailureCode.BerthOccupied, request.BerthId, request.PortId,
                        bySelf ? D19FailureRetryPolicy.NoRetry : D19FailureRetryPolicy.RetryAfterCorrection);
                }
                return Reject(BerthCode, bySelf ? ReasonDuplicateRequest : ReasonOccupied,
                    D19PortFailureCode.BerthOccupied, request.BerthId, request.PortId,
                    bySelf ? D19FailureRetryPolicy.NoRetry : D19FailureRetryPolicy.RetryAfterCorrection);
            }

            // 9) 状态组合一致性（L24：目标 Berthed 携带归属本港泊位）
            D19GuardVerdict combination = D19InvalidStateGuard.CheckStateCombination(
                D19PortAnchorCatalog.Instance, D19PortVisitState.Berthed,
                request.PortId, request.BerthId, D19CrewLocation.Unknown);
            if (!combination.Allowed)
            {
                return FromGuard(combination);
            }

            return D19DockingVerdict.Allow(request.PortId, request.BerthId);
        }

        /// <summary>
        /// 预留执行（消费 L06 Reserve 命令）：裁决通过才调用 L06 预留，
        /// 裁决未通过返回 null ReserveResult（L06 未被调用，状态不变）。
        /// </summary>
        public static D19DockReservationOutcome AdjudicateReservation(
            D19BerthSystem berthSystem,
            D19DockingRequest request,
            int currentWorldDay,
            int untilWorldDay,
            long saveRevision)
        {
            if (berthSystem == null)
            {
                throw new ArgumentNullException("berthSystem");
            }
            D19DockingRequest facts = ReadAllocations(berthSystem, request);
            D19DockingVerdict verdict = Adjudicate(facts);
            D19DockReservationOutcome outcome = new D19DockReservationOutcome();
            outcome.Verdict = verdict;
            if (!verdict.Allowed)
            {
                return outcome;
            }
            outcome.ReserveResult = berthSystem.Reserve(
                request.CommandId, request.PortId, request.BerthId,
                request.ShipProfile, currentWorldDay, untilWorldDay, saveRevision,
                string.IsNullOrEmpty(request.ActorId) ? null : new List<string> { request.ActorId });
            return outcome;
        }

        /// <summary>从 L06 只读接口补齐分配事实（GetAllocation / GetAllocations）。</summary>
        public static D19DockingRequest ReadAllocations(D19BerthSystem berthSystem, D19DockingRequest request)
        {
            D19DockingRequest copy = new D19DockingRequest();
            copy.CommandId = request.CommandId;
            copy.ShipId = request.ShipId;
            copy.ActorId = request.ActorId;
            copy.PortId = request.PortId;
            copy.BerthId = request.BerthId;
            copy.ShipProfile = request.ShipProfile;
            copy.VisitState = request.VisitState;
            copy.FacilityAvailability = request.FacilityAvailability;
            copy.AuthorityClaims = request.AuthorityClaims;
            copy.RequestedBerthAllocation = berthSystem.GetAllocation(request.BerthId);
            foreach (D19BerthAllocation allocation in berthSystem.GetAllocations())
            {
                if (string.Equals(allocation.ShipId, request.ShipId, StringComparison.Ordinal) &&
                    !string.Equals(allocation.BerthId, request.BerthId, StringComparison.Ordinal))
                {
                    copy.ShipCurrentAllocation = allocation;
                    break;
                }
            }
            return copy;
        }

        // ------------------------------------------------------------
        // 内部工具
        // ------------------------------------------------------------

        private static D19DockingVerdict CheckVisitState(D19PortVisitState state)
        {
            switch (state)
            {
                case D19PortVisitState.Docked:
                case D19PortVisitState.Returned:
                    return null;
                case D19PortVisitState.None:
                case D19PortVisitState.Departed:
                    return Reject(BerthCode, ReasonNotInPort, D19PortFailureCode.NotInPort, string.Empty, string.Empty, D19FailureRetryPolicy.RetryAfterCorrection);
                case D19PortVisitState.Berthed:
                case D19PortVisitState.InService:
                    return Reject(BerthCode, ReasonShipAlreadyBerthed, D19PortFailureCode.BerthOccupied, string.Empty, string.Empty, D19FailureRetryPolicy.RetryAfterCorrection);
                default:
                    return Reject(BerthCode, ReasonNotMoored, D19PortFailureCode.NotMoored, string.Empty, string.Empty, D19FailureRetryPolicy.RetryAfterCorrection);
            }
        }

        private static D19DockingVerdict FromGuard(D19GuardVerdict guard)
        {
            D19DockingVerdict verdict = new D19DockingVerdict();
            verdict.Allowed = false;
            verdict.BusinessCode = guard.BusinessCode;
            verdict.LocKey = guard.LocKey;
            verdict.Retry = guard.Retry;
            verdict.ReasonId = guard.ReasonId;
            return verdict;
        }

        private static D19DockingVerdict Reject(
            string businessCode,
            string reasonId,
            D19PortFailureCode lifecycleCode,
            string berthId,
            string portId,
            D19FailureRetryPolicy retry)
        {
            D19FailureEntry entry = D19PortFailureCatalog.Find(businessCode);
            D19DockingVerdict verdict = new D19DockingVerdict();
            verdict.Allowed = false;
            if (entry == null)
            {
                throw new InvalidOperationException("docking rules cannot resolve L17 business code: " + businessCode);
            }
            verdict.BusinessCode = entry.Code;
            verdict.LocKey = entry.LocKey;
            verdict.Retry = retry;
            verdict.ReasonId = reasonId ?? string.Empty;
            verdict.CanonicalPortId = portId ?? string.Empty;
            verdict.CanonicalBerthId = berthId ?? string.Empty;
            return verdict;
        }

        /// <summary>枚举名 → 小写蛇形（如 FacilityClosed → facility_closed）。</summary>
        public static string SnakeEnum(Enum value)
        {
            string name = value == null ? string.Empty : value.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            System.Text.StringBuilder builder = new System.Text.StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c >= 'A' && c <= 'Z')
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }
                    builder.Append((char)(c - 'A' + 'a'));
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
    }
}
