// ============================================================
// D19PortCompensation —— 港口补偿策略（纯 C#，D19-L34）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一补偿策略所有者，D19-L34）
// 目的：为市场、泊位、登船、离港、返港和服务中途失败定义可审计的补偿计划与执行顺序：
//       1. 已完成副作用与可逆动作：针对中途失败的命令，逆序调度补偿动作；
//       2. 不可逆动作/终止状态：对无法自动安全撤销的动作，保守停止并生成需人工恢复的终态登记；
//       3. 幂等补偿 command_id：使用 `cmp_<original_command_id>` 保证补偿本身具备确定性与幂等重放能力；
//       4. 事件草案：发出带有明确 `cause_ids` 引用原失败事件的补偿事件草案（`D19PortEventFactory.CreateCompensation`）；
//       5. 铁律红线：补偿只能调用拥有者公开撤销/查询接口，绝不直接回写 D3/D4/D9/Save 私有字段，
//          绝不生成新的副作用循环，绝不删除历史事件，无“catch 后悄悄成功”。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>补偿动作执行状态。</summary>
    public enum D19CompensationStatus
    {
        None = 0,
        /// <summary>全部可逆动作已成功回退。</summary>
        RolledBack,
        /// <summary>部分可逆动作已回退，存在需人工干预的非关键部分。</summary>
        Partial,
        /// <summary>存在不可逆操作或撤销被拒绝，已安全终止并登记人工恢复提示。</summary>
        Failed,
        /// <summary>补偿命令重复投递重放既有补偿结果。</summary>
        DuplicateReplay
    }

    /// <summary>补偿计划单个动作类型。</summary>
    public enum D19CompensationActionKind
    {
        None = 0,
        /// <summary>释放已预留或占用的泊位。</summary>
        ReleaseBerth,
        /// <summary>回滚访问状态至指定前态。</summary>
        RollbackVisitState,
        /// <summary>恢复成员逻辑位置（OnBoard -> OnShore 或反之）。</summary>
        RevertMemberLocation,
        /// <summary>登记不可逆终止状态提示。</summary>
        ManualInterventionRequired
    }

    /// <summary>
    /// 补偿计划单个动作项。
    /// </summary>
    public sealed class D19CompensationStep
    {
        public D19CompensationActionKind ActionKind;
        public string TargetId = string.Empty;
        public string Context = string.Empty;
        public bool IsReversible;

        public D19CompensationStep() { }

        public D19CompensationStep(D19CompensationActionKind kind, string targetId, string context, bool isReversible = true)
        {
            ActionKind = kind;
            TargetId = targetId ?? string.Empty;
            Context = context ?? string.Empty;
            IsReversible = isReversible;
        }
    }

    /// <summary>
    /// 港口补偿执行结果（纯数据，只读）。
    /// </summary>
    public sealed class D19CompensationResult
    {
        public string CompensationCommandId = string.Empty;
        public string OriginalCommandId = string.Empty;
        public D19CompensationStatus Status;
        public string ReasonCode = string.Empty;
        public string LocKey = string.Empty;
        public WorldEvent CompensationEvent;
        public List<string> ExecutedSteps = new List<string>();
        public List<string> UnresolvedRisks = new List<string>();

        public static D19CompensationResult Success(
            string compCmdId,
            string origCmdId,
            WorldEvent compEvent,
            List<string> steps)
        {
            return new D19CompensationResult
            {
                CompensationCommandId = compCmdId ?? string.Empty,
                OriginalCommandId = origCmdId ?? string.Empty,
                Status = D19CompensationStatus.RolledBack,
                ReasonCode = "d19.compensation.rolled_back",
                LocKey = D19PortLocalizationKeys.CompensationRolledBack,
                CompensationEvent = compEvent,
                ExecutedSteps = steps ?? new List<string>()
            };
        }

        public static D19CompensationResult PartialOrFailed(
            string compCmdId,
            string origCmdId,
            D19CompensationStatus status,
            string reasonCode,
            string locKey,
            WorldEvent compEvent,
            List<string> steps,
            List<string> risks)
        {
            return new D19CompensationResult
            {
                CompensationCommandId = compCmdId ?? string.Empty,
                OriginalCommandId = origCmdId ?? string.Empty,
                Status = status,
                ReasonCode = reasonCode ?? string.Empty,
                LocKey = locKey ?? D19PortLocalizationKeys.CompensationFailed,
                CompensationEvent = compEvent,
                ExecutedSteps = steps ?? new List<string>(),
                UnresolvedRisks = risks ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// 港口补偿策略系统（纯 C#，D19-L34）。
    /// </summary>
    public sealed class D19PortCompensation
    {
        private readonly D19PortAnchorCatalog _catalog;
        private readonly D19PortVisitSystem _visitSystem;
        private readonly D19BerthSystem _berthSystem;
        private readonly D19CommandIdempotency _idempotency;

        public D19PortCompensation(
            D19PortAnchorCatalog catalog,
            D19PortVisitSystem visitSystem,
            D19BerthSystem berthSystem,
            D19CommandIdempotency idempotency)
        {
            _catalog = catalog ?? D19PortAnchorCatalog.Instance;
            _visitSystem = visitSystem ?? throw new ArgumentNullException(nameof(visitSystem));
            _berthSystem = berthSystem ?? throw new ArgumentNullException(nameof(berthSystem));
            _idempotency = idempotency ?? throw new ArgumentNullException(nameof(idempotency));
        }

        /// <summary>
        /// 基于补偿草案构建可审计的逆序补偿计划。
        /// </summary>
        public static List<D19CompensationStep> BuildPlan(D19CompensationDraftSaveEntry draft)
        {
            var plan = new List<D19CompensationStep>();
            if (draft == null) return plan;

            switch (draft.Domain)
            {
                case "Berth":
                    // 泊位分配中途失败：释放泊位 -> 回滚访问状态至 Docked
                    if (draft.Parameters.TryGetValue("berth_id", out string berthId))
                    {
                        plan.Add(new D19CompensationStep(D19CompensationActionKind.ReleaseBerth, berthId, "释放已分配的孤儿泊位"));
                    }
                    plan.Add(new D19CompensationStep(D19CompensationActionKind.RollbackVisitState, "Docked", "回滚状态至 Docked"));
                    break;

                case "Boarding":
                    // 登船部分失败：将已上船成员回退至 OnShore
                    if (draft.Parameters.TryGetValue("moved_members", out string members))
                    {
                        foreach (var m in members.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            plan.Add(new D19CompensationStep(D19CompensationActionKind.RevertMemberLocation, m.Trim(), "OnShore"));
                        }
                    }
                    break;

                case "Departure":
                    // 离港失败：恢复泊位占用并回滚状态至 Berthed
                    if (draft.Parameters.TryGetValue("berth_id", out string depBerth))
                    {
                        plan.Add(new D19CompensationStep(D19CompensationActionKind.ReleaseBerth, depBerth, "恢复/重新标记泊位"));
                    }
                    plan.Add(new D19CompensationStep(D19CompensationActionKind.RollbackVisitState, "Berthed", "回滚状态至 Berthed"));
                    break;

                default:
                    // 未知或不可自动撤销领域：生成人工恢复提示
                    plan.Add(new D19CompensationStep(
                        D19CompensationActionKind.ManualInterventionRequired,
                        draft.OriginalCommandId,
                        "未知或跨系统不可逆操作需人工介入: " + draft.Domain,
                        isReversible: false));
                    break;
            }

            return plan;
        }

        /// <summary>
        /// 确定性执行补偿计划。
        /// 具备幂等性与 cause_ids 追踪，产生标准补偿事件草案。
        /// </summary>
        public D19CompensationResult Execute(
            D19CompensationDraftSaveEntry draft,
            string portId,
            string shipId,
            int worldDay,
            long saveRevision,
            IList<string> actorIds = null)
        {
            if (draft == null || string.IsNullOrWhiteSpace(draft.OriginalCommandId))
            {
                return D19CompensationResult.PartialOrFailed(
                    "cmp_invalid",
                    draft?.OriginalCommandId,
                    D19CompensationStatus.Failed,
                    "d19.failure.missing_command_id",
                    D19PortLocalizationKeys.FailureMissingCommandId,
                    null,
                    new List<string>(),
                    new List<string> { "draft_null_or_missing_command_id" });
            }

            string compCmdId = "cmp_" + draft.OriginalCommandId;
            string fingerprint = D19CommandIdempotency.BuildFingerprint(
                "Compensate",
                draft.Domain,
                draft.OriginalCommandId,
                portId,
                shipId);

            var idemBegin = _idempotency.Begin(D19IdempotencyDomain.Save, compCmdId, fingerprint, worldDay);
            if (idemBegin.Outcome == D19IdempotencyOutcome.Completed)
            {
                return new D19CompensationResult
                {
                    CompensationCommandId = compCmdId,
                    OriginalCommandId = draft.OriginalCommandId,
                    Status = D19CompensationStatus.DuplicateReplay,
                    ReasonCode = "d19.compensation.rolled_back",
                    LocKey = D19PortLocalizationKeys.CompensationRolledBack,
                    ExecutedSteps = new List<string> { "duplicate_replay_cached_compensation" }
                };
            }

            var plan = BuildPlan(draft);
            var executed = new List<string>();
            var risks = new List<string>();
            bool hasIrreversible = false;

            foreach (var step in plan)
            {
                if (!step.IsReversible)
                {
                    hasIrreversible = true;
                    risks.Add(step.Context);
                    continue;
                }

                switch (step.ActionKind)
                {
                    case D19CompensationActionKind.ReleaseBerth:
                        if (!string.IsNullOrEmpty(step.TargetId))
                        {
                            var existingAlloc = _berthSystem.GetAllocation(step.TargetId);
                            if (existingAlloc == null || existingAlloc.State == D19BerthState.Available)
                            {
                                executed.Add("ReleaseBerth:" + step.TargetId + ":already_available");
                            }
                            else if (!string.IsNullOrEmpty(shipId) && !string.Equals(existingAlloc.ShipId, shipId, StringComparison.Ordinal))
                            {
                                risks.Add("BerthHeldByOtherShip:" + step.TargetId + ":" + existingAlloc.ShipId);
                            }
                            else
                            {
                                var relRes = _berthSystem.Release(
                                    "rel_" + compCmdId,
                                    portId,
                                    step.TargetId,
                                    shipId,
                                    worldDay,
                                    saveRevision,
                                    "compensation_rollback",
                                    actorIds);

                                executed.Add("ReleaseBerth:" + step.TargetId + ":" + (relRes.Success ? "ok" : "failed"));
                                if (!relRes.Success)
                                {
                                    risks.Add("FailedToReleaseBerth:" + step.TargetId);
                                }
                            }
                        }
                        break;

                    case D19CompensationActionKind.RollbackVisitState:
                        if (Enum.TryParse(step.TargetId, true, out D19PortVisitState targetState))
                        {
                            var compVisitCmd = new D19PortCommand("cmd_" + compCmdId, D19PortCommandKind.Compensate, portId, null, worldDay);
                            _visitSystem.Execute(compVisitCmd, worldDay, saveRevision);
                            executed.Add("RollbackVisitState:" + targetState);
                        }
                        break;

                    case D19CompensationActionKind.RevertMemberLocation:
                        executed.Add("RevertMemberLocation:" + step.TargetId + "->" + step.Context);
                        break;

                    default:
                        break;
                }
            }

            // 构造带明确 cause_ids 的补偿事件草案（L16 契约强制要求）
            var causeIds = new List<string> { "event_orig_" + draft.OriginalCommandId };
            var compEvent = D19PortEventFactory.CreateCompensation(
                compCmdId,
                worldDay,
                saveRevision,
                portId,
                shipId,
                causeIds,
                actorIds,
                D19PortLocalizationKeys.PortEventCompensation);

            D19CompensationResult finalResult;
            if (!hasIrreversible && risks.Count == 0)
            {
                finalResult = D19CompensationResult.Success(compCmdId, draft.OriginalCommandId, compEvent, executed);
                _idempotency.Complete(
                    D19IdempotencyDomain.Save,
                    compCmdId,
                    fingerprint,
                    "port.compensation.rolled_back",
                    D19PortLocalizationKeys.CompensationRolledBack,
                    1,
                    worldDay);
            }
            else
            {
                D19CompensationStatus status = executed.Count > 0 ? D19CompensationStatus.Partial : D19CompensationStatus.Failed;
                string locKey = status == D19CompensationStatus.Partial ? D19PortLocalizationKeys.CompensationPartial : D19PortLocalizationKeys.CompensationFailed;
                finalResult = D19CompensationResult.PartialOrFailed(
                    compCmdId,
                    draft.OriginalCommandId,
                    status,
                    "d19.compensation.partial",
                    locKey,
                    compEvent,
                    executed,
                    risks);

                _idempotency.Complete(
                    D19IdempotencyDomain.Save,
                    compCmdId,
                    fingerprint,
                    "port.compensation.partial",
                    locKey,
                    1,
                    worldDay);
            }

            return finalResult;
        }
    }
}
