// ============================================================
// D19BoardingWorkflow —— D19 登船/下船纯业务工作流（纯 C#）
//
// 归属：Whalebone.Systems.D19（所有者：D19BoardingWorkflow，D19-L08）
// 目的：定义登船/下船请求、成员位置语义（逻辑枚举 + 稳定实体 ID）、
//       允许状态、失败回退与幂等结果；部分成员失败时原子拒绝，
//       不留下"船上/岸上"双重状态。
// 约束：成员位置只使用逻辑枚举（D19CrewLocation）与稳定成员实体 ID，
//       不访问角色 Transform/动画/碰撞/输入；不直接修改 D20、船员或 Save；
//       只读引用 L02/L16/L17/L23/L24 类型，不复制其实现。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 登船/下船请求类型（D19-L08，所有者：D19BoardingWorkflow）。
    /// Board=成员登船（生命周期 Docked/Berthed → Boarded）；Disembark=成员下船（生命周期状态不变）。
    /// </summary>
    public enum D19BoardingRequestType
    {
        /// <summary>占位：非法类型。</summary>
        Unknown = 0,

        /// <summary>登船。</summary>
        Board = 1,

        /// <summary>下船。</summary>
        Disembark = 2
    }

    /// <summary>
    /// 登船/下船请求 DTO（D19-L08，所有者：D19BoardingWorkflow）。
    /// 稳定 CommandId 用于幂等与事件关联；MemberIds 为稳定成员实体 ID。
    /// </summary>
    public sealed class D19BoardingRequest
    {
        /// <summary>稳定命令 ID（调用方生成，跨重试不变）。</summary>
        public string CommandId;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId;

        /// <summary>目标港口稳定键。</summary>
        public string PortId;

        /// <summary>请求类型（登船/下船）。</summary>
        public D19BoardingRequestType Type;

        /// <summary>成员稳定实体 ID 列表（请求移动的成员）。</summary>
        public IList<string> MemberIds = new List<string>();

        public D19BoardingRequest(string commandId, string actorId, string shipId, string portId,
            D19BoardingRequestType type, IList<string> memberIds)
        {
            CommandId = commandId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            Type = type;
            if (memberIds != null)
            {
                foreach (string memberId in memberIds)
                {
                    if (!string.IsNullOrEmpty(memberId))
                    {
                        MemberIds.Add(memberId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 单个成员的位置结果（D19-L08，所有者：D19BoardingWorkflow）。
    /// Verdict.Allowed=false 表示该成员被拒绝（BusinessCode/ReasonId/LifecycleCode 稳定可审计）。
    /// </summary>
    public sealed class D19MemberPosition
    {
        /// <summary>成员稳定实体 ID。</summary>
        public string MemberId;

        /// <summary>目标位置（逻辑枚举：OnShore/OnBoard）。</summary>
        public D19CrewLocation TargetLocation;

        /// <summary>是否实际移动（false = 校验通过但位置未变，仅用于输出语义）。</summary>
        public bool Moved;

        /// <summary>防护结果（D19-L24 语义；Allowed=false 时含稳定业务码与子原因）。</summary>
        public D19GuardVerdict Verdict;

        public D19MemberPosition(string memberId, D19CrewLocation targetLocation, bool moved, D19GuardVerdict verdict)
        {
            MemberId = memberId ?? string.Empty;
            TargetLocation = targetLocation;
            Moved = moved;
            Verdict = verdict;
        }
    }

    /// <summary>
    /// 登船/下船工作流结果（D19-L08，所有者：D19BoardingWorkflow）。
    /// Success=true 时 ProposedLocations 为调用方应应用的成员目标位置（工作流不自行持久化）；
    /// Success=false 时原子拒绝：不产生任何 ProposedLocations、不留下双重状态，
    /// Failure 为失败载体、Rejection 为 L17 业务拒绝判定（含首个失败成员的原因）。
    /// </summary>
    public sealed class D19BoardingResult
    {
        /// <summary>请求是否成功（全部成员校验通过才为 true）。</summary>
        public bool Success;

        /// <summary>与请求一致的稳定命令 ID。</summary>
        public string CommandId;

        /// <summary>每成员结果（校验顺序与请求顺序一致）。</summary>
        public IList<D19MemberPosition> Members = new List<D19MemberPosition>();

        /// <summary>成功时输出：成员 → 目标位置（调用方应用；本工作流不写船员/Save）。</summary>
        public IDictionary<string, D19CrewLocation> ProposedLocations = new Dictionary<string, D19CrewLocation>();

        /// <summary>本次执行的事件草案（幂等命中时为空；成功 1 个，原子拒绝 1 个）。</summary>
        public IList<WorldEvent> Events = new List<WorldEvent>();

        /// <summary>失败载体（Success=false 时有效；L02 生命周期码 + 稳定原因 ID）。</summary>
        public D19PortFailure Failure;

        /// <summary>L17 业务拒绝判定（Success=false 时为首个失败成员的防护结果）。</summary>
        public D19GuardVerdict Rejection;

        public D19BoardingResult(string commandId)
        {
            CommandId = commandId ?? string.Empty;
        }
    }

    /// <summary>
    /// 登船/下船纯业务工作流（D19-L08，所有者：本工作流）。
    /// 状态与位置均为输入快照：生命周期状态/泊位/成员位置由调用方传入，
    /// 输出为 ProposedLocations 与事件草案；本工作流不持有、不持久化任何船员状态。
    /// 同一 CommandId 重复投递返回既有稳定结果且不产生新事件/副作用（幂等）。
    /// </summary>
    public sealed class D19BoardingWorkflow
    {
        private const string FallbackRejectionCode = "port.lifecycle.rejected";
        private const string UnknownMemberReason = "unknown_member";

        private readonly D19PortAnchorCatalog _catalog;
        private readonly Dictionary<string, D19BoardingResult> _completedResults =
            new Dictionary<string, D19BoardingResult>();

        public D19BoardingWorkflow(D19PortAnchorCatalog catalog)
        {
            _catalog = catalog;
        }

        /// <summary>
        /// 执行登船/下船请求（确定性 + 原子拒绝 + 幂等）。
        /// 校验顺序固定：命令 ID → 授权（L23）→ 生命周期状态 → 港口/船一致 → 每成员
        /// （未知成员 → 已在目标位置（L24 CheckCrewLocation））。全部通过才应用移动。
        /// </summary>
        public D19BoardingResult Execute(
            D19BoardingRequest request,
            D19PortVisitState currentState,
            string currentBerthId,
            IDictionary<string, D19CrewLocation> currentLocations,
            D19AuthorityClaims claims,
            int worldDay,
            long saveRevision)
        {
            if (request == null || string.IsNullOrEmpty(request.CommandId))
            {
                return Reject(
                    request, null, "missing_command_id", D19PortFailureCode.MissingCommandId,
                    worldDay, saveRevision, null);
            }

            D19BoardingResult cached;
            if (_completedResults.TryGetValue(request.CommandId, out cached))
            {
                // 幂等命中：返回既有稳定结果（复制字段，Events 置空——无新副作用）。
                D19BoardingResult replay = new D19BoardingResult(cached.CommandId);
                replay.Success = cached.Success;
                replay.Failure = cached.Failure;
                replay.Rejection = cached.Rejection;
                foreach (D19MemberPosition position in cached.Members)
                {
                    replay.Members.Add(position);
                }
                foreach (KeyValuePair<string, D19CrewLocation> pair in cached.ProposedLocations)
                {
                    replay.ProposedLocations[pair.Key] = pair.Value;
                }
                return replay;
            }

            if (request.Type == D19BoardingRequestType.Unknown)
            {
                return Reject(request, request.CommandId, "invalid_boarding_type", D19PortFailureCode.InvalidTransition,
                    worldDay, saveRevision, null);
            }

            if (string.IsNullOrEmpty(request.PortId))
            {
                return Reject(request, request.CommandId, "unknown_port", D19PortFailureCode.UnknownPort,
                    worldDay, saveRevision, null);
            }

            // 授权（L23）：请求者必须有船主/船员声明且船停靠一致。
            D19AuthorityContext context = new D19AuthorityContext(
                request.Type == D19BoardingRequestType.Board ? D19PortCommandKind.Board : D19PortCommandKind.Board,
                request.ActorId, request.ShipId, request.PortId, string.Empty);
            D19AuthorityResult authority = D19PortAuthorityValidator.Validate(context, claims);
            if (!authority.Allowed)
            {
                D19FailureEntry entry = authority.Failure;
                D19PortFailureCode lifecycle = entry != null ? entry.LifecycleCode : D19PortFailureCode.InvalidTransition;
                return Reject(request, request.CommandId,
                    entry != null ? entry.Code : FallbackRejectionCode, lifecycle,
                    worldDay, saveRevision, null);
            }

            // 生命周期状态允许（L24 CheckCommand 语义：Board 必须满足 L02 Boarded 迁移）。
            if (request.Type == D19BoardingRequestType.Board)
            {
                D19PortCommand boardCommand = new D19PortCommand(
                    request.CommandId, D19PortCommandKind.Board, request.PortId, currentBerthId, worldDay);
                D19GuardVerdict lifecycle = D19InvalidStateGuard.CheckCommand(
                    _catalog, boardCommand, currentState, currentBerthId, D19CrewLocation.OnBoard);
                if (!lifecycle.Allowed)
                {
                    return Reject(request, request.CommandId,
                        lifecycle.BusinessCode, lifecycle.LifecycleCode,
                        worldDay, saveRevision, lifecycle);
                }
            }
            else
            {
                // 下船：船必须在港口（Board 之后仍可能 Docked/Berthed/Boarded），不迁移生命周期。
                if (currentState != D19PortVisitState.Boarded
                    && currentState != D19PortVisitState.Docked
                    && currentState != D19PortVisitState.Berthed)
                {
                    D19GuardVerdict verdict = D19InvalidStateGuard.Reject(
                        "d19.boarding.failed", "not_in_port", D19PortFailureCode.NotInPort);
                    return Reject(request, request.CommandId,
                        verdict.BusinessCode, verdict.LifecycleCode, worldDay, saveRevision, verdict);
                }
            }

            if (string.IsNullOrEmpty(request.ShipId) || request.ShipId != claims.ShipId)
            {
                D19GuardVerdict verdict = D19InvalidStateGuard.Reject(
                    "d19.boarding.failed", "cross_ship_request", D19PortFailureCode.InvalidTransition);
                return Reject(request, request.CommandId,
                    verdict.BusinessCode, verdict.LifecycleCode, worldDay, saveRevision, verdict);
            }

            // 每成员校验（顺序固定；任一失败 → 原子拒绝）。
            D19CrewLocation requested =
                request.Type == D19BoardingRequestType.Board ? D19CrewLocation.OnBoard : D19CrewLocation.OnShore;
            D19BoardingResult result = new D19BoardingResult(request.CommandId);
            D19MemberPosition firstRejected = null;
            foreach (string memberId in request.MemberIds)
            {
                D19CrewLocation current;
                if (currentLocations == null || !currentLocations.TryGetValue(memberId, out current))
                {
                    D19GuardVerdict unknown = D19InvalidStateGuard.Reject(
                        "d19.boarding.failed", UnknownMemberReason, D19PortFailureCode.InvalidTransition);
                    D19MemberPosition rejected = new D19MemberPosition(memberId, requested, false, unknown);
                    result.Members.Add(rejected);
                    if (firstRejected == null)
                    {
                        firstRejected = rejected;
                    }
                    continue;
                }

                D19GuardVerdict verdict = D19InvalidStateGuard.CheckCrewLocation(current, requested);
                D19MemberPosition position = new D19MemberPosition(memberId, requested, verdict.Allowed, verdict);
                result.Members.Add(position);
                if (!verdict.Allowed && firstRejected == null)
                {
                    firstRejected = position;
                }
            }

            if (firstRejected != null)
            {
                // 原子拒绝：保留每成员各自的校验结果（不把首个失败 verdict 分发给其它成员），
                // 不应用任何成员移动（无 ProposedLocations），只生成 1 个拒绝事件。
                result.Success = false;
                result.Rejection = firstRejected.Verdict;
                result.Failure = new D19PortFailure(
                    firstRejected.Verdict.LifecycleCode,
                    firstRejected.Verdict.BusinessCode,
                    firstRejected.Verdict.Retry == D19FailureRetryPolicy.RetrySafe);
                result.Events.Add(D19PortEventFactory.CreateRejection(
                    request.CommandId, worldDay, saveRevision, request.PortId, request.ShipId,
                    firstRejected.Verdict.BusinessCode, "", null, ""));
                _completedResults[request.CommandId] = result;
                return result;
            }

            // 全部通过：应用成员移动（输出目标位置，不写船员/Save）。
            foreach (D19MemberPosition position in result.Members)
            {
                result.ProposedLocations[position.MemberId] = position.TargetLocation;
                position.Moved = true;
            }

            if (request.Type == D19BoardingRequestType.Board)
            {
                result.Events.Add(D19PortEventFactory.CreateBoarded(
                    request.CommandId, worldDay, saveRevision, request.PortId, request.ShipId, null, null));
            }
            else
            {
                result.Events.Add(D19PortEventFactory.CreateDisembarked(
                    request.CommandId, worldDay, saveRevision, request.PortId, request.ShipId, null, null));
            }

            result.Success = true;
            _completedResults[request.CommandId] = result;
            return result;
        }

        /// <summary>原子拒绝：不移动任何成员，生成 1 个 L16 拒绝事件草案（L17 业务码）。</summary>
        private D19BoardingResult Reject(
            D19BoardingRequest request,
            string commandId,
            string reasonCode,
            D19PortFailureCode lifecycleCode,
            int worldDay,
            long saveRevision,
            D19GuardVerdict verdict)
        {
            D19BoardingResult result = new D19BoardingResult(commandId ?? string.Empty);
            if (request != null && request.MemberIds != null)
            {
                foreach (string memberId in request.MemberIds)
                {
                    result.Members.Add(new D19MemberPosition(
                        memberId,
                        request.Type == D19BoardingRequestType.Board ? D19CrewLocation.OnBoard : D19CrewLocation.OnShore,
                        false,
                        verdict ?? D19InvalidStateGuard.Reject(reasonCode, reasonCode, lifecycleCode)));
                }
            }
            result.Rejection = verdict;
            result.Failure = new D19PortFailure(
                lifecycleCode, reasonCode, lifecycleCode == D19PortFailureCode.ServiceUnavailable);
            if (!string.IsNullOrEmpty(commandId) && request != null)
            {
                result.Events.Add(D19PortEventFactory.CreateRejection(
                    commandId, worldDay, saveRevision,
                    request.PortId, request.ShipId, reasonCode, "", null, ""));
            }
            if (!string.IsNullOrEmpty(commandId) && request != null)
            {
                _completedResults[commandId] = result;
            }
            return result;
        }
    }
}
