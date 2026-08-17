// ============================================================
// D19DepartureWorkflow —— 离港工作流确定性编排（纯 C#，D19-L09）
//
// 归属：Whalebone.Systems.D19（所有者：D19DepartureWorkflow，D19-L09）
// 目的：实现离港全流程的确定性编排：
//       1. 幂等校验（D19-L22）：拦截重复提交，保障确定性与零二次副作用；
//       2. 权限与安全校验（D19-L23 / D19-L24）：命令 ID、港口/目的地归属、权限声明；
//       3. 生命周期状态校验（D19-L02 / D19-L24）：允许前态（Boarded 或 PortVisit 直达）；
//       4. 船员登船位置校验（D19-L08 / D19-L24）：离港前所有申报船员必须全部处于 OnBoard；
//       5. 出航准备汇总校验（D19-L15）：检查项全通过（CanDepart=true，无 Unknown/Blocker）；
//       6. 离泊释放编排（D19-L06）：若持有泊位，执行释放；
//       7. 航程启动与生命周期迁移：推进到 Departed 状态，生成 L16 离港事件草案；
//       8. 中途失败可审计补偿：若离泊后后续步骤失败，自动触发可审计补偿草案，
//          绝不留下“已离泊但未开始航程”的半状态孤儿。
//
// 红线与约束（D19-L01 基线）：
//   * 只能通过公开契约编排，严禁直接修改 SailingSystem、船只、库存或船员内部数值；
//   * 零 UnityEngine.UI / Canvas / Transform / 物理碰撞 / 场景 API / 网络 / LLM 依赖；
//   * 纯函数式确定性与单向依赖，同输入得到相同输出；
//   * 任何失败或中断均有稳定结果与 L16/L17 留痕。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 离港请求 DTO（纯数据，调用方构造）。
    /// </summary>
    public sealed class D19DepartureWorkflowRequest
    {
        /// <summary>稳定命令 ID（跨重试不变，幂等与事件关联键）。</summary>
        public string CommandId = string.Empty;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId = string.Empty;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId = string.Empty;

        /// <summary>出发港口稳定键。</summary>
        public string PortId = string.Empty;

        /// <summary>目的地稳定键（必须为不同于出发港的有效港口）。</summary>
        public string DestinationId = string.Empty;

        /// <summary>当前港口访问状态快照。</summary>
        public D19PortVisitState CurrentVisitState = D19PortVisitState.None;

        /// <summary>当前持有的泊位稳定键（可为空）。</summary>
        public string CurrentBerthId = string.Empty;

        /// <summary>船员位置字典（离港前所有船员必须为 OnBoard）。</summary>
        public IDictionary<string, D19CrewLocation> CrewLocations = new Dictionary<string, D19CrewLocation>(StringComparer.Ordinal);

        /// <summary>权限声明（L23 校验）。</summary>
        public D19AuthorityClaims AuthorityClaims;

        /// <summary>出航准备只读汇总（来自 L15 D19PreparationPortAdapter）。</summary>
        public D19DepartureReadinessSummary PreparationSummary;

        /// <summary>逻辑世界日。</summary>
        public int WorldDay;

        /// <summary>存档修订号。</summary>
        public long SaveRevision;

        public D19DepartureWorkflowRequest()
        {
        }

        public D19DepartureWorkflowRequest(
            string commandId,
            string actorId,
            string shipId,
            string portId,
            string destinationId,
            D19PortVisitState currentVisitState,
            string currentBerthId,
            int worldDay = 0,
            long saveRevision = 0)
        {
            CommandId = commandId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            DestinationId = destinationId ?? string.Empty;
            CurrentVisitState = currentVisitState;
            CurrentBerthId = currentBerthId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
        }
    }

    /// <summary>
    /// 离港工作流执行结果（纯数据）。
    /// </summary>
    public sealed class D19DepartureWorkflowResult
    {
        public string CommandId = string.Empty;
        public bool Success;
        public bool DuplicateSubmission;

        /// <summary>离港后的港口访问状态（成功时为 Departed；失败时保持原状态）。</summary>
        public D19PortVisitState StateAfter = D19PortVisitState.None;

        /// <summary>释放的泊位 ID（若无泊位则为空）。</summary>
        public string ReleasedBerthId = string.Empty;

        /// <summary>是否执行了泊位释放。</summary>
        public bool BerthReleased;

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

        public D19DepartureWorkflowResult(string commandId)
        {
            CommandId = commandId ?? string.Empty;
        }

        public D19DepartureWorkflowResult Clone()
        {
            D19DepartureWorkflowResult copy = new D19DepartureWorkflowResult(CommandId)
            {
                Success = Success,
                DuplicateSubmission = DuplicateSubmission,
                StateAfter = StateAfter,
                ReleasedBerthId = ReleasedBerthId,
                BerthReleased = BerthReleased,
                Compensated = Compensated,
                Failure = Failure,
                FailureVerdict = FailureVerdict,
                Note = Note
            };
            foreach (WorldEvent e in Events)
            {
                copy.Events.Add(e == null ? null : e.Clone());
            }
            return copy;
        }
    }

    /// <summary>
    /// 离港工作流确定性编排器（D19-L09，唯一编排入口）。
    /// </summary>
    public sealed class D19DepartureWorkflow
    {
        private const string UnknownPortReason = "unknown_port";
        private const string InvalidDestinationReason = "invalid_destination";
        private const string CrewNotOnBoardReason = "crew_not_on_board";
        private const string PreparationNotReadyReason = "preparation_not_ready";
        private const string MissingCommandIdReason = "missing_command_id";

        private readonly D19PortAnchorCatalog _anchorCatalog;
        private readonly D19CommandIdempotency _idempotency;
        private readonly Dictionary<string, D19DepartureWorkflowResult> _completedResults =
            new Dictionary<string, D19DepartureWorkflowResult>(StringComparer.Ordinal);

        public D19DepartureWorkflow(
            D19PortAnchorCatalog anchorCatalog = null,
            D19CommandIdempotency idempotency = null)
        {
            _anchorCatalog = anchorCatalog ?? D19PortAnchorCatalog.Instance;
            _idempotency = idempotency;
        }

        /// <summary>
        /// 执行离港全流程编排（确定性 + 原子防护 + 失败补偿 + 幂等）。
        /// 可选传入 berthSystem（用于泊位释放）与 visitSystem（用于访问状态机迁移）。
        /// </summary>
        public D19DepartureWorkflowResult Execute(
            D19DepartureWorkflowRequest request,
            D19BerthSystem berthSystem = null,
            D19PortVisitSystem visitSystem = null)
        {
            if (request == null || string.IsNullOrEmpty(request.CommandId))
            {
                return Reject(request, D19PortFailureCode.MissingCommandId, MissingCommandIdReason,
                    "Command ID is required for departure.");
            }

            // 1. 幂等检查（本地缓存 + L22 幂等记录器）
            D19DepartureWorkflowResult cached;
            if (_completedResults.TryGetValue(request.CommandId, out cached))
            {
                D19DepartureWorkflowResult replay = cached.Clone();
                replay.DuplicateSubmission = true;
                replay.Events.Clear(); // 幂等重放不产生二次事件
                return replay;
            }

            string fingerprint = BuildFingerprint(request);
            if (_idempotency != null)
            {
                D19IdempotencyResult rec = _idempotency.Begin(
                    D19IdempotencyDomain.Departure,
                    request.CommandId,
                    fingerprint,
                    request.WorldDay);

                if (rec.Outcome == D19IdempotencyOutcome.Completed)
                {
                    D19DepartureWorkflowResult replay = new D19DepartureWorkflowResult(request.CommandId)
                    {
                        Success = true,
                        DuplicateSubmission = true,
                        StateAfter = D19PortVisitState.Departed,
                        ReleasedBerthId = request.CurrentBerthId,
                        BerthReleased = !string.IsNullOrEmpty(request.CurrentBerthId),
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

            // 3. 出发港口与目的地合法性（L04 / L24）
            D19GuardVerdict portVerdict = D19InvalidStateGuard.CheckPort(_anchorCatalog, request.PortId);
            if (!portVerdict.Allowed)
            {
                return RejectFromGuard(request, portVerdict);
            }

            if (string.IsNullOrEmpty(request.DestinationId))
            {
                return Reject(request, D19PortFailureCode.NotReadyToDepart, InvalidDestinationReason,
                    "Destination port is required for departure.");
            }

            var destResolution = _anchorCatalog.ResolvePort(request.DestinationId);
            if (!destResolution.Success)
            {
                return Reject(request, D19PortFailureCode.UnknownPort, UnknownPortReason,
                    $"Destination port '{request.DestinationId}' is unknown.");
            }

            var portResolution = _anchorCatalog.ResolvePort(request.PortId);
            if (string.Equals(portResolution.CanonicalKey, destResolution.CanonicalKey, StringComparison.Ordinal))
            {
                return Reject(request, D19PortFailureCode.NotReadyToDepart, InvalidDestinationReason,
                    "Destination port must be different from current departure port.");
            }

            // 4. 权限校验（L23）
            if (request.AuthorityClaims != null)
            {
                D19AuthorityContext authContext = new D19AuthorityContext(
                    D19PortCommandKind.Depart,
                    request.ActorId,
                    request.ShipId,
                    request.PortId,
                    request.CurrentBerthId);

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

            // 5. 生命周期状态校验（L02 / L24）
            D19PortVisitState targetState;
            D19GuardVerdict lifecycleVerdict;
            if (!D19InvalidStateGuard.CanTransition(request.CurrentVisitState, D19PortTransitionReason.Departed, out targetState, out lifecycleVerdict))
            {
                return RejectFromGuard(request, lifecycleVerdict);
            }

            // 6. 船员位置检查（所有船员必须为 OnBoard）
            if (request.CrewLocations != null && request.CrewLocations.Count > 0)
            {
                foreach (KeyValuePair<string, D19CrewLocation> crew in request.CrewLocations)
                {
                    if (crew.Value != D19CrewLocation.OnBoard)
                    {
                        return Reject(request, D19PortFailureCode.NotReadyToDepart, CrewNotOnBoardReason,
                            $"Crew member '{crew.Key}' is not on board ({crew.Value}).");
                    }
                }
            }

            // 7. 出航准备汇总检查（L15）
            if (request.PreparationSummary != null)
            {
                if (!request.PreparationSummary.CanDepart || request.PreparationSummary.HasUnknown)
                {
                    string blockerReason = request.PreparationSummary.FirstBlocker != null
                        ? request.PreparationSummary.FirstBlocker.CheckId
                        : PreparationNotReadyReason;

                    return Reject(request, D19PortFailureCode.NotReadyToDepart, blockerReason,
                        "Departure preparation checklist is not ready or has unknown items.");
                }
            }

            // 8. 离泊释放编排（L06）
            bool berthReleased = false;
            string releasedBerth = string.Empty;
            WorldEvent releaseEventDraft = null;

            if (!string.IsNullOrEmpty(request.CurrentBerthId) && berthSystem != null)
            {
                D19BerthResult releaseResult = berthSystem.Release(
                    $"{request.CommandId}_release",
                    request.PortId,
                    request.CurrentBerthId,
                    request.ShipId,
                    request.WorldDay,
                    request.SaveRevision);

                if (!releaseResult.Success)
                {
                    // 离泊失败：中止流程，状态完全不变
                    return Reject(request, releaseResult.Failure != null ? releaseResult.Failure.Code : D19PortFailureCode.InvalidTransition,
                        releaseResult.Failure != null ? releaseResult.Failure.ReasonId : "berth_release_failed",
                        "Failed to release current berth before departure.");
                }

                berthReleased = true;
                releasedBerth = request.CurrentBerthId;
                releaseEventDraft = releaseResult.Event;
            }

            // 9. 状态机推进（L03 访问状态机）与补偿保护
            D19PortVisitState finalState = targetState;
            WorldEvent departureEventDraft = null;

            if (visitSystem != null)
            {
                D19PortCommand departCmd = new D19PortCommand(
                    request.CommandId,
                    D19PortCommandKind.Depart,
                    request.PortId,
                    null,
                    request.WorldDay);

                D19PortVisitResult visitRes = visitSystem.Execute(departCmd, request.WorldDay, request.SaveRevision);
                if (visitRes.Result == null || !visitRes.Result.Success)
                {
                    // 状态机迁移失败：执行可审计补偿（若已释放泊位，恢复泊位占用草案并发出补偿事件）
                    D19DepartureWorkflowResult compensatedResult = new D19DepartureWorkflowResult(request.CommandId)
                    {
                        Success = false,
                        Compensated = true,
                        StateAfter = request.CurrentVisitState,
                        ReleasedBerthId = releasedBerth,
                        BerthReleased = false,
                        Failure = visitRes.Result != null ? visitRes.Result.Failure : new D19PortFailure(D19PortFailureCode.InvalidTransition, "visit_system_rejected"),
                        Note = "Visit system rejected departure; compensated and state restored."
                    };

                    // 生成可审计补偿事件
                    List<string> originalEvents = new List<string>();
                    if (releaseEventDraft != null) originalEvents.Add(releaseEventDraft.EventId);
                    else originalEvents.Add($"d19pe_departure_{request.CommandId}_0");

                    WorldEvent compEvent = D19PortEventFactory.CreateCompensation(
                        $"{request.CommandId}_comp",
                        request.WorldDay,
                        request.SaveRevision,
                        request.PortId,
                        request.ShipId,
                        originalEvents,
                        new[] { request.ActorId },
                        D19PortLocalizationKeys.CompensationRolledBack);

                    compensatedResult.Events.Add(compEvent);
                    _completedResults[request.CommandId] = compensatedResult;
                    return compensatedResult;
                }

                finalState = visitRes.Result.State;
                if (visitRes.Events != null && visitRes.Events.Count > 0)
                {
                    departureEventDraft = visitRes.Events[0];
                }
            }

            // 10. 构造成功事件与结果
            if (departureEventDraft == null)
            {
                departureEventDraft = D19PortEventFactory.CreateDeparture(
                    request.CommandId,
                    request.WorldDay,
                    request.SaveRevision,
                    request.PortId,
                    request.ShipId,
                    new[] { request.ActorId },
                    null,
                    D19PortLocalizationKeys.PortEventDeparture);
            }

            D19DepartureWorkflowResult successResult = new D19DepartureWorkflowResult(request.CommandId)
            {
                Success = true,
                StateAfter = finalState,
                ReleasedBerthId = releasedBerth,
                BerthReleased = berthReleased,
                Compensated = false,
                Note = "Departure workflow successfully executed."
            };

            if (releaseEventDraft != null)
            {
                successResult.Events.Add(releaseEventDraft);
            }
            successResult.Events.Add(departureEventDraft);

            // 11. 完成幂等登记
            if (_idempotency != null)
            {
                _idempotency.Complete(
                    D19IdempotencyDomain.Departure,
                    request.CommandId,
                    fingerprint,
                    "port.departure.applied",
                    D19PortLocalizationKeys.PortEventDeparture,
                    0,
                    request.WorldDay);
            }

            _completedResults[request.CommandId] = successResult;
            return successResult;
        }

        // ---------- 辅助构建与拒绝处理 ----------

        private D19DepartureWorkflowResult Reject(
            D19DepartureWorkflowRequest request,
            D19PortFailureCode failureCode,
            string reasonId,
            string explanation)
        {
            string cmdId = request != null ? request.CommandId : string.Empty;
            string portId = request != null ? request.PortId : string.Empty;
            string shipId = request != null ? request.ShipId : string.Empty;
            int worldDay = request != null ? request.WorldDay : 0;
            long saveRevision = request != null ? request.SaveRevision : 0;
            D19PortVisitState origState = request != null ? request.CurrentVisitState : D19PortVisitState.None;

            D19FailureEntry entry = D19PortFailureCatalog.Find(reasonId);
            string locKey = entry != null ? entry.LocKey : D19PortLocalizationKeys.FailureNotReadyToDepart;
            bool recoverable = entry == null || entry.Retry != D19FailureRetryPolicy.NoRetry;

            D19DepartureWorkflowResult result = new D19DepartureWorkflowResult(cmdId)
            {
                Success = false,
                StateAfter = origState,
                Failure = new D19PortFailure(failureCode, reasonId, recoverable),
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
                    $"d19pe_departure_{cmdId}_0",
                    new[] { request.ActorId },
                    D19PortLocalizationKeys.PortEventRejection);

                result.Events.Add(rejectionEvent);
            }

            if (!string.IsNullOrEmpty(cmdId))
            {
                _completedResults[cmdId] = result;
            }
            return result;
        }

        private D19DepartureWorkflowResult RejectFromGuard(
            D19DepartureWorkflowRequest request,
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
                : D19PortFailureCode.NotReadyToDepart;

            D19DepartureWorkflowResult result = new D19DepartureWorkflowResult(cmdId)
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
                    $"d19pe_departure_{cmdId}_0",
                    new[] { request.ActorId },
                    D19PortLocalizationKeys.PortEventRejection);

                result.Events.Add(rejectionEvent);
            }

            if (!string.IsNullOrEmpty(cmdId))
            {
                _completedResults[cmdId] = result;
            }
            return result;
        }

        private static string BuildFingerprint(D19DepartureWorkflowRequest request)
        {
            return D19CommandIdempotency.BuildFingerprint(
                request.ShipId,
                request.PortId,
                request.DestinationId,
                request.CurrentBerthId,
                request.WorldDay.ToString());
        }
    }
}
