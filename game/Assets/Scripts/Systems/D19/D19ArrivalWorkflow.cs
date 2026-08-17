// ============================================================
// D19ArrivalWorkflow —— 返港/到港工作流确定性编排（纯 C#，D19-L10）
//
// 归属：Whalebone.Systems.D19（所有者：D19ArrivalWorkflow，D19-L10）
// 目的：实现返港全流程的确定性编排：
//       1. 幂等校验（D19-L22）：拦截重复提交，保障确定性与零二次副作用；
//       2. 权限与安全校验（D19-L23 / D19-L24）：命令 ID、目标港口/泊位归属、权限声明；
//       3. 在航/返港前态校验（D19-L02 / D19-L24）：允许前态（Departed、None、Returned）；
//       4. 泊位分配与规则校验（D19-L07 / D19-L06）：按泊位规则请求预留/占用；
//       5. 航程结束意图（D19VoyageEndIntent）：产生航程结束纯数据意图，供 D3/D9 结算消费；
//       6. 港口服务状态恢复：收集/恢复目标港口可用服务状态列表；
//       7. 状态机推进（D19-L03）：生命周期迁移至 Returned（或 Berthed），生成 L16 事件草案；
//       8. 中途失败确定性补偿：若泊位分配后后续状态机或流程被拒绝，
//          自动释放泊位并发出可审计补偿草案，绝不留下孤儿泊位或双重航程状态。
//
// 红线与约束（D19-L01 基线）：
//   * 不得从真实位置、坐标、碰撞、距离或场景判断到港（纯逻辑事实驱动）；
//   * 不得直接结束 D2 航程或直接写 D3TradeState / 库存 / 船员数值；
//   * 零 UnityEngine.UI / Canvas / Transform / 物理碰撞 / 场景 API / 网络 / LLM 依赖；
//   * 纯函数式确定性与单向依赖，同输入得到相同输出；
//   * 任何失败、冲突或中断均有确定性结果与 L16/L17 留痕。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 航程结束意图（纯数据 DTO，供 D3 VoyageSettlementSystem / D9 消费）。
    /// D19 绝不直接结算货币、库存或修改船员数值，只产生此意图。
    /// </summary>
    public sealed class D19VoyageEndIntent
    {
        /// <summary>稳定命令 ID。</summary>
        public string CommandId = string.Empty;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId = string.Empty;

        /// <summary>抵达港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>逻辑世界日。</summary>
        public int WorldDay;

        /// <summary>意图是否可执行结算。</summary>
        public bool Executable;

        /// <summary>是否已准备好进行航程结算。</summary>
        public bool SettlementReady;

        /// <summary>意图摘要说明。</summary>
        public string Summary = string.Empty;

        /// <summary>补充说明。</summary>
        public string Note = string.Empty;

        public D19VoyageEndIntent Clone()
        {
            return new D19VoyageEndIntent
            {
                CommandId = CommandId,
                ShipId = ShipId,
                PortId = PortId,
                WorldDay = WorldDay,
                Executable = Executable,
                SettlementReady = SettlementReady,
                Summary = Summary,
                Note = Note
            };
        }
    }

    /// <summary>
    /// 返港/到港请求 DTO（纯数据，调用方构造）。
    /// </summary>
    public sealed class D19ArrivalWorkflowRequest
    {
        /// <summary>稳定命令 ID（跨重试不变，幂等与事件关联键）。</summary>
        public string CommandId = string.Empty;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId = string.Empty;

        /// <summary>目标抵达港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>请求分配的目标泊位稳定键（可选；若非空则请求泊位分配）。</summary>
        public string TargetBerthId = string.Empty;

        /// <summary>当前港口访问状态快照（预期为 Departed、None 或 Returned）。</summary>
        public D19PortVisitState CurrentVisitState = D19PortVisitState.None;

        /// <summary>D3 航程状态快照（Sailing / Arrived / InPort 等）。</summary>
        public D3VoyageStatus VoyageStatus = D3VoyageStatus.InPort;

        /// <summary>船只能力画像（L06 泊位尺寸档位与深水能力检查）。</summary>
        public D19BerthShipProfile ShipProfile;

        /// <summary>权限声明（L23 权威校验）。</summary>
        public D19AuthorityClaims AuthorityClaims;

        /// <summary>泊位设施可用性裁决（L05 可选输入）。</summary>
        public D19ServiceAvailabilityResult FacilityAvailability;

        /// <summary>泊位预留租期（世界日天数，默认 7 天）。</summary>
        public int ReservationTermDays = 7;

        /// <summary>是否在预留成功后立即确认占用（转 Occupied）。</summary>
        public bool ConfirmOccupancy;

        /// <summary>逻辑世界日。</summary>
        public int WorldDay;

        /// <summary>存档修订号。</summary>
        public long SaveRevision;

        public D19ArrivalWorkflowRequest()
        {
        }

        public D19ArrivalWorkflowRequest(
            string commandId,
            string actorId,
            string shipId,
            string portId,
            string targetBerthId,
            D19PortVisitState currentVisitState,
            D3VoyageStatus voyageStatus = D3VoyageStatus.Sailing,
            D19BerthShipProfile shipProfile = null,
            int worldDay = 0,
            long saveRevision = 0)
        {
            CommandId = commandId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            TargetBerthId = targetBerthId ?? string.Empty;
            CurrentVisitState = currentVisitState;
            VoyageStatus = voyageStatus;
            ShipProfile = shipProfile;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
        }
    }

    /// <summary>
    /// 返港/到港工作流执行结果（纯数据）。
    /// </summary>
    public sealed class D19ArrivalWorkflowResult
    {
        public string CommandId = string.Empty;
        public bool Success;
        public bool DuplicateSubmission;

        /// <summary>返港后的港口访问状态（成功时为 Returned 或 Berthed；失败时保持原状态）。</summary>
        public D19PortVisitState StateAfter = D19PortVisitState.None;

        /// <summary>分配的泊位 ID（若无泊位则为空）。</summary>
        public string AllocatedBerthId = string.Empty;

        /// <summary>是否成功分配了泊位。</summary>
        public bool BerthAllocated;

        /// <summary>泊位是否已确认占用。</summary>
        public bool BerthConfirmed;

        /// <summary>航程结束意图。</summary>
        public D19VoyageEndIntent VoyageEndIntent;

        /// <summary>恢复的可用的港口服务稳定键列表（稳定排序）。</summary>
        public List<string> AvailableServices = new List<string>();

        /// <summary>是否发生了失败回滚/补偿。</summary>
        public bool Compensated;

        /// <summary>失败载体（L02 枚举码 + L17 业务原因 ID）。</summary>
        public D19PortFailure Failure;

        /// <summary>防护/权限详细判定（Success=false 时有效）。</summary>
        public D19GuardVerdict FailureVerdict;

        /// <summary>本次执行生成的事件草案（幂等重放时为空）。</summary>
        public IList<WorldEvent> Events = new List<WorldEvent>();

        /// <summary>补充说明（机器可读留痕）。</summary>
        public string Note = string.Empty;

        public D19ArrivalWorkflowResult(string commandId)
        {
            CommandId = commandId ?? string.Empty;
        }

        public D19ArrivalWorkflowResult Clone()
        {
            D19ArrivalWorkflowResult copy = new D19ArrivalWorkflowResult(CommandId)
            {
                Success = Success,
                DuplicateSubmission = DuplicateSubmission,
                StateAfter = StateAfter,
                AllocatedBerthId = AllocatedBerthId,
                BerthAllocated = BerthAllocated,
                BerthConfirmed = BerthConfirmed,
                VoyageEndIntent = VoyageEndIntent == null ? null : VoyageEndIntent.Clone(),
                Compensated = Compensated,
                Failure = Failure,
                FailureVerdict = FailureVerdict,
                Note = Note
            };
            foreach (string s in AvailableServices)
            {
                copy.AvailableServices.Add(s);
            }
            foreach (WorldEvent e in Events)
            {
                copy.Events.Add(e == null ? null : e.Clone());
            }
            return copy;
        }
    }

    /// <summary>
    /// 返港/到港工作流确定性编排器（D19-L10，唯一编排入口）。
    /// </summary>
    public sealed class D19ArrivalWorkflow
    {
        private const string UnknownPortReason = "unknown_port";
        private const string InvalidPrestateReason = "invalid_arrival_prestate";
        private const string MissingCommandIdReason = "missing_command_id";
        private const string BerthAllocationFailedReason = "berth_allocation_failed";
        private const string VisitSystemRejectedReason = "visit_system_rejected";

        private readonly D19PortAnchorCatalog _anchorCatalog;
        private readonly D19CommandIdempotency _idempotency;
        private readonly Dictionary<string, D19ArrivalWorkflowResult> _completedResults =
            new Dictionary<string, D19ArrivalWorkflowResult>(StringComparer.Ordinal);

        public D19ArrivalWorkflow(
            D19PortAnchorCatalog anchorCatalog = null,
            D19CommandIdempotency idempotency = null)
        {
            _anchorCatalog = anchorCatalog ?? D19PortAnchorCatalog.Instance;
            _idempotency = idempotency;
        }

        /// <summary>
        /// 执行返港全流程编排（确定性 + 泊位规则 + 航程意图 + 服务恢复 + 补偿保护 + 幂等）。
        /// 可选传入 berthSystem（用于泊位预留/占用）与 visitSystem（用于访问状态机迁移）。
        /// </summary>
        public D19ArrivalWorkflowResult Execute(
            D19ArrivalWorkflowRequest request,
            D19BerthSystem berthSystem = null,
            D19PortVisitSystem visitSystem = null)
        {
            if (request == null || string.IsNullOrEmpty(request.CommandId))
            {
                return Reject(request, D19PortFailureCode.MissingCommandId, MissingCommandIdReason,
                    "Command ID is required for arrival workflow.");
            }

            // 1. 幂等检查（本地缓存 + L22 幂等管理器）
            D19ArrivalWorkflowResult cached;
            if (_completedResults.TryGetValue(request.CommandId, out cached))
            {
                D19ArrivalWorkflowResult replay = cached.Clone();
                replay.DuplicateSubmission = true;
                replay.Events.Clear(); // 幂等重放不产生二次事件
                return replay;
            }

            string fingerprint = BuildFingerprint(request);
            if (_idempotency != null)
            {
                D19IdempotencyResult rec = _idempotency.Begin(
                    D19IdempotencyDomain.Berth,
                    request.CommandId,
                    fingerprint,
                    request.WorldDay);

                if (rec.Outcome == D19IdempotencyOutcome.Completed)
                {
                    D19ArrivalWorkflowResult replay = new D19ArrivalWorkflowResult(request.CommandId)
                    {
                        Success = true,
                        DuplicateSubmission = true,
                        StateAfter = string.IsNullOrEmpty(request.TargetBerthId) ? D19PortVisitState.Returned : D19PortVisitState.Berthed,
                        AllocatedBerthId = request.TargetBerthId,
                        BerthAllocated = !string.IsNullOrEmpty(request.TargetBerthId),
                        BerthConfirmed = request.ConfirmOccupancy,
                        Note = "Idempotency replay from L22 manager."
                    };
                    return replay;
                }

                if (rec.Outcome == D19IdempotencyOutcome.Conflict)
                {
                    return Reject(request, D19PortFailureCode.DuplicateCommand, "duplicate_command_conflict",
                        "Idempotency conflict: command ID already used with different parameters.");
                }
            }

            // 2. 基础格式与安全校验（L24）
            D19GuardVerdict cmdVerdict = D19InvalidStateGuard.CheckCommandId(request.CommandId);
            if (!cmdVerdict.Allowed)
            {
                return RejectFromGuard(request, cmdVerdict);
            }

            // 3. 目标港口合法性校验（L04 / L24）
            D19GuardVerdict portVerdict = D19InvalidStateGuard.CheckPort(_anchorCatalog, request.PortId);
            if (!portVerdict.Allowed)
            {
                return RejectFromGuard(request, portVerdict);
            }

            var portResolution = _anchorCatalog.ResolvePort(request.PortId);
            string canonicalPortId = portResolution.CanonicalKey;

            // 4. 权限校验（L23）
            if (request.AuthorityClaims != null)
            {
                D19AuthorityContext authContext = new D19AuthorityContext(
                    D19PortCommandKind.Return,
                    request.ActorId,
                    request.ShipId,
                    canonicalPortId,
                    request.TargetBerthId);

                D19AuthorityResult authResult = D19PortAuthorityValidator.Validate(
                    authContext,
                    request.AuthorityClaims,
                    _anchorCatalog);

                if (!authResult.Allowed)
                {
                    D19GuardVerdict authVerdict = new D19GuardVerdict
                    {
                        Allowed = false,
                        BusinessCode = authResult.Failure != null ? authResult.Failure.Code : "d19.port.permission_denied",
                        LocKey = authResult.LocKey,
                        Retry = authResult.Failure != null ? authResult.Failure.Retry : D19FailureRetryPolicy.RetryAfterCorrection,
                        LifecycleCode = D19PortFailureCode.None,
                        ReasonId = authResult.Cause.ToString()
                    };
                    return RejectFromGuard(request, authVerdict);
                }
            }

            // 5. 在航/返港前态校验
            // 允许的前置状态：Departed（航程返港）、None（初次到港）、Returned（已返港重新对接泊位/服务）
            if (request.CurrentVisitState != D19PortVisitState.Departed &&
                request.CurrentVisitState != D19PortVisitState.None &&
                request.CurrentVisitState != D19PortVisitState.Returned)
            {
                return Reject(request, D19PortFailureCode.InvalidTransition, InvalidPrestateReason,
                    $"Cannot execute arrival workflow from state '{request.CurrentVisitState}'. Expected Departed, None, or Returned.");
            }

            // 6. 泊位规则与预留/占用编排（L07 / L06）
            bool berthAllocated = false;
            bool berthConfirmed = false;
            string allocatedBerthId = string.Empty;
            WorldEvent berthReservedEventDraft = null;
            WorldEvent berthOccupiedEventDraft = null;

            if (!string.IsNullOrEmpty(request.TargetBerthId))
            {
                // 校验泊位归属
                D19GuardVerdict berthGuard = D19InvalidStateGuard.CheckBerth(_anchorCatalog, request.TargetBerthId, canonicalPortId);
                if (!berthGuard.Allowed)
                {
                    return RejectFromGuard(request, berthGuard);
                }

                // 靠泊前置规则纯校验（L07）
                D19DockingRequest dockReq = new D19DockingRequest
                {
                    CommandId = $"{request.CommandId}_dock",
                    ShipId = request.ShipId,
                    ActorId = request.ActorId,
                    PortId = canonicalPortId,
                    BerthId = request.TargetBerthId,
                    ShipProfile = request.ShipProfile,
                    VisitState = D19PortVisitState.Returned, // 返港后状态
                    FacilityAvailability = request.FacilityAvailability,
                    AuthorityClaims = request.AuthorityClaims
                };

                if (berthSystem != null)
                {
                    dockReq = D19DockingRules.ReadAllocations(berthSystem, dockReq);
                }

                D19DockingVerdict dockVerdict = D19DockingRules.Adjudicate(dockReq);
                if (!dockVerdict.Allowed)
                {
                    D19FailureEntry entry = D19PortFailureCatalog.Find(dockVerdict.BusinessCode);
                    D19PortFailure failure = new D19PortFailure(
                        D19PortFailureCode.BerthOccupied,
                        dockVerdict.ReasonId,
                        dockVerdict.Retry != D19FailureRetryPolicy.NoRetry);

                    return Reject(request, D19PortFailureCode.BerthOccupied, dockVerdict.ReasonId,
                        $"Berth docking rules rejected: {dockVerdict.BusinessCode} ({dockVerdict.ReasonId})", failure);
                }

                // 执行泊位预留（L06）
                if (berthSystem != null)
                {
                    int termDays = request.ReservationTermDays > 0 ? request.ReservationTermDays : 7;
                    int untilDay = request.WorldDay + termDays;

                    D19BerthResult reserveResult = berthSystem.Reserve(
                        $"{request.CommandId}_res",
                        canonicalPortId,
                        request.TargetBerthId,
                        request.ShipProfile ?? new D19BerthShipProfile(request.ShipId, 1, false),
                        request.WorldDay,
                        untilDay,
                        request.SaveRevision,
                        string.IsNullOrEmpty(request.ActorId) ? null : new[] { request.ActorId });

                    if (!reserveResult.Success)
                    {
                        return Reject(request,
                            reserveResult.Failure != null ? reserveResult.Failure.Code : D19PortFailureCode.BerthOccupied,
                            reserveResult.Failure != null ? reserveResult.Failure.ReasonId : BerthAllocationFailedReason,
                            "Berth system failed to reserve requested berth.");
                    }

                    berthAllocated = true;
                    allocatedBerthId = request.TargetBerthId;
                    berthReservedEventDraft = reserveResult.Event;

                    // 若请求确认占用，执行 ConfirmOccupancy
                    if (request.ConfirmOccupancy)
                    {
                        D19BerthResult confirmResult = berthSystem.ConfirmOccupancy(
                            $"{request.CommandId}_occ",
                            canonicalPortId,
                            request.TargetBerthId,
                            request.ShipId,
                            request.WorldDay,
                            request.SaveRevision,
                            string.IsNullOrEmpty(request.ActorId) ? null : new[] { request.ActorId });

                        if (!confirmResult.Success)
                        {
                            // 确认占用失败：回滚预留（避免孤儿泊位）
                            berthSystem.Release(
                                $"{request.CommandId}_rollback",
                                canonicalPortId,
                                request.TargetBerthId,
                                request.ShipId,
                                request.WorldDay,
                                request.SaveRevision,
                                "failed_confirm_rollback");

                            return Reject(request,
                                confirmResult.Failure != null ? confirmResult.Failure.Code : D19PortFailureCode.BerthOccupied,
                                confirmResult.Failure != null ? confirmResult.Failure.ReasonId : "berth_occupancy_failed",
                                "Berth system failed to confirm occupancy; rolled back reservation.");
                        }

                        berthConfirmed = true;
                        berthOccupiedEventDraft = confirmResult.Event;
                    }
                }
                else
                {
                    berthAllocated = true;
                    allocatedBerthId = request.TargetBerthId;
                    berthConfirmed = request.ConfirmOccupancy;
                }
            }

            // 7. 生命周期状态机推进（L03 访问状态机）与失败补偿保护
            D19PortVisitState finalState = D19PortVisitState.Returned;
            WorldEvent returnEventDraft = null;

            if (visitSystem != null)
            {
                D19PortCommandKind cmdKind = request.CurrentVisitState == D19PortVisitState.None
                    ? D19PortCommandKind.EnterPort
                    : D19PortCommandKind.Return;

                D19PortCommand returnCmd = new D19PortCommand(
                    request.CommandId,
                    cmdKind,
                    canonicalPortId,
                    null,
                    request.WorldDay);

                D19PortVisitResult visitRes = visitSystem.Execute(returnCmd, request.WorldDay, request.SaveRevision);
                if (visitRes.Result == null || !visitRes.Result.Success)
                {
                    // 状态机迁移失败：执行确定性补偿（若已分配泊位，释放泊位，绝不留下孤儿泊位）
                    if (berthAllocated && berthSystem != null && !string.IsNullOrEmpty(allocatedBerthId))
                    {
                        berthSystem.Release(
                            $"{request.CommandId}_comp_rel",
                            canonicalPortId,
                            allocatedBerthId,
                            request.ShipId,
                            request.WorldDay,
                            request.SaveRevision,
                            "arrival_rejected_compensation");
                    }

                    D19ArrivalWorkflowResult compensatedResult = new D19ArrivalWorkflowResult(request.CommandId)
                    {
                        Success = false,
                        Compensated = true,
                        StateAfter = request.CurrentVisitState,
                        AllocatedBerthId = string.Empty,
                        BerthAllocated = false,
                        BerthConfirmed = false,
                        Failure = visitRes.Result != null ? visitRes.Result.Failure : new D19PortFailure(D19PortFailureCode.InvalidTransition, VisitSystemRejectedReason),
                        Note = "Visit system rejected return transition; compensated allocated berth."
                    };

                    List<string> originalEvents = new List<string>();
                    if (berthReservedEventDraft != null) originalEvents.Add(berthReservedEventDraft.EventId);
                    else originalEvents.Add($"d19pe_return_{request.CommandId}_0");

                    WorldEvent compEvent = D19PortEventFactory.CreateCompensation(
                        $"{request.CommandId}_comp",
                        request.WorldDay,
                        request.SaveRevision,
                        canonicalPortId,
                        request.ShipId,
                        originalEvents,
                        string.IsNullOrEmpty(request.ActorId) ? null : new[] { request.ActorId },
                        D19PortLocalizationKeys.PortEventCompensation);

                    compensatedResult.Events.Add(compEvent);
                    _completedResults[request.CommandId] = compensatedResult;
                    return compensatedResult;
                }

                finalState = visitRes.Result.State;
                if (visitRes.Events != null && visitRes.Events.Count > 0)
                {
                    returnEventDraft = visitRes.Events[0];
                }

                // 若分配了泊位且状态机当前处于 Returned，进一步分配泊位至 Berthed
                if (berthAllocated && !string.IsNullOrEmpty(allocatedBerthId))
                {
                    D19PortCommand berthCmd = new D19PortCommand(
                        $"{request.CommandId}_assign",
                        D19PortCommandKind.AssignBerth,
                        canonicalPortId,
                        allocatedBerthId,
                        request.WorldDay);

                    D19PortVisitResult berthVisitRes = visitSystem.Execute(berthCmd, request.WorldDay, request.SaveRevision);
                    if (berthVisitRes.Result != null && berthVisitRes.Result.Success)
                    {
                        finalState = berthVisitRes.Result.State;
                    }
                }
            }
            else
            {
                finalState = (berthAllocated && !string.IsNullOrEmpty(allocatedBerthId))
                    ? D19PortVisitState.Berthed
                    : D19PortVisitState.Returned;
            }

            // 8. 产生航程结束意图（纯数据，供 D3 结算系统消费）
            D19VoyageEndIntent voyageEndIntent = new D19VoyageEndIntent
            {
                CommandId = request.CommandId,
                ShipId = request.ShipId,
                PortId = canonicalPortId,
                WorldDay = request.WorldDay,
                Executable = true,
                SettlementReady = true,
                Summary = "Arrival completed; ship returned to port and ready for voyage settlement.",
                Note = $"Returned to {canonicalPortId} on day {request.WorldDay}."
            };

            // 9. 恢复目标港口可用服务状态列表（稳定排序）
            List<string> recoveredServices = new List<string>();
            foreach (string serviceKey in _anchorCatalog.ServiceKeys)
            {
                var resolvedService = _anchorCatalog.ResolveService(serviceKey, canonicalPortId);
                if (resolvedService.Success && string.Equals(resolvedService.Port, canonicalPortId, StringComparison.Ordinal))
                {
                    recoveredServices.Add(resolvedService.CanonicalKey);
                }
            }
            recoveredServices.Sort(StringComparer.Ordinal);

            // 10. 构造成功结果与事件草案
            if (returnEventDraft == null)
            {
                returnEventDraft = D19PortEventFactory.CreateReturn(
                    request.CommandId,
                    request.WorldDay,
                    request.SaveRevision,
                    canonicalPortId,
                    request.ShipId,
                    string.IsNullOrEmpty(request.ActorId) ? null : new[] { request.ActorId },
                    null,
                    D19PortLocalizationKeys.PortEventReturn);
            }

            D19ArrivalWorkflowResult successResult = new D19ArrivalWorkflowResult(request.CommandId)
            {
                Success = true,
                StateAfter = finalState,
                AllocatedBerthId = allocatedBerthId,
                BerthAllocated = berthAllocated,
                BerthConfirmed = berthConfirmed,
                VoyageEndIntent = voyageEndIntent,
                AvailableServices = recoveredServices,
                Compensated = false,
                Note = "Arrival workflow successfully executed."
            };

            successResult.Events.Add(returnEventDraft);
            if (berthReservedEventDraft != null)
            {
                successResult.Events.Add(berthReservedEventDraft);
            }
            if (berthOccupiedEventDraft != null)
            {
                successResult.Events.Add(berthOccupiedEventDraft);
            }

            // 11. 登记幂等完成
            if (_idempotency != null)
            {
                _idempotency.Complete(
                    D19IdempotencyDomain.Berth,
                    request.CommandId,
                    fingerprint,
                    "port.return.applied",
                    D19PortLocalizationKeys.PortEventReturn,
                    0,
                    request.WorldDay);
            }

            _completedResults[request.CommandId] = successResult;
            return successResult;
        }

        // ---------- 辅助拒绝与指纹构建 ----------

        private D19ArrivalWorkflowResult Reject(
            D19ArrivalWorkflowRequest request,
            D19PortFailureCode failureCode,
            string reasonId,
            string explanation,
            D19PortFailure customFailure = null)
        {
            string cmdId = request != null ? request.CommandId : string.Empty;
            string portId = request != null ? request.PortId : string.Empty;
            string shipId = request != null ? request.ShipId : string.Empty;
            int worldDay = request != null ? request.WorldDay : 0;
            long saveRevision = request != null ? request.SaveRevision : 0;
            D19PortVisitState origState = request != null ? request.CurrentVisitState : D19PortVisitState.None;

            D19FailureEntry entry = D19PortFailureCatalog.Find(reasonId);
            bool recoverable = entry == null || entry.Retry != D19FailureRetryPolicy.NoRetry;

            D19ArrivalWorkflowResult result = new D19ArrivalWorkflowResult(cmdId)
            {
                Success = false,
                StateAfter = origState,
                Failure = customFailure ?? new D19PortFailure(failureCode, reasonId, recoverable),
                Note = explanation
            };

            if (!string.IsNullOrEmpty(cmdId) && !string.IsNullOrEmpty(portId) && !string.IsNullOrEmpty(shipId))
            {
                WorldEvent rejectionEvent = D19PortEventFactory.CreateRejection(
                    cmdId,
                    worldDay,
                    saveRevision,
                    portId,
                    shipId,
                    $"port.reject.{reasonId}",
                    $"d19pe_return_{cmdId}_0",
                    string.IsNullOrEmpty(request?.ActorId) ? null : new[] { request.ActorId },
                    D19PortLocalizationKeys.PortEventRejection);

                result.Events.Add(rejectionEvent);
            }

            if (!string.IsNullOrEmpty(cmdId))
            {
                _completedResults[cmdId] = result;
            }
            return result;
        }

        private D19ArrivalWorkflowResult RejectFromGuard(
            D19ArrivalWorkflowRequest request,
            D19GuardVerdict verdict)
        {
            string cmdId = request != null ? request.CommandId : string.Empty;
            string portId = request != null ? request.PortId : string.Empty;
            string shipId = request != null ? request.ShipId : string.Empty;
            int worldDay = request != null ? request.WorldDay : 0;
            long saveRevision = request != null ? request.SaveRevision : 0;
            D19PortVisitState origState = request != null ? request.CurrentVisitState : D19PortVisitState.None;

            D19PortFailureCode fCode = verdict.LifecycleCode != D19PortFailureCode.None
                ? verdict.LifecycleCode
                : D19PortFailureCode.InvalidTransition;

            D19ArrivalWorkflowResult result = new D19ArrivalWorkflowResult(cmdId)
            {
                Success = false,
                StateAfter = origState,
                Failure = new D19PortFailure(fCode, verdict.ReasonId, verdict.Retry != D19FailureRetryPolicy.NoRetry),
                FailureVerdict = verdict,
                Note = $"Guard rejected: {verdict.BusinessCode} ({verdict.ReasonId})"
            };

            if (!string.IsNullOrEmpty(cmdId) && !string.IsNullOrEmpty(portId) && !string.IsNullOrEmpty(shipId))
            {
                WorldEvent rejectionEvent = D19PortEventFactory.CreateRejection(
                    cmdId,
                    worldDay,
                    saveRevision,
                    portId,
                    shipId,
                    $"port.reject.{verdict.ReasonId}",
                    $"d19pe_return_{cmdId}_0",
                    string.IsNullOrEmpty(request?.ActorId) ? null : new[] { request.ActorId },
                    D19PortLocalizationKeys.PortEventRejection);

                result.Events.Add(rejectionEvent);
            }

            if (!string.IsNullOrEmpty(cmdId))
            {
                _completedResults[cmdId] = result;
            }
            return result;
        }

        private static string BuildFingerprint(D19ArrivalWorkflowRequest request)
        {
            return D19CommandIdempotency.BuildFingerprint(
                request.ShipId,
                request.PortId,
                request.TargetBerthId ?? string.Empty,
                request.CurrentVisitState.ToString(),
                request.WorldDay.ToString());
        }
    }
}
