// ============================================================
// D19PortVisitSystem —— D19 港口到港/离港访问状态机（纯 C#）
//
// 归属：Whalebone.Systems.D19（所有者：D19PortVisitSystem，D19-L03）
// 目的：以不可变输入（D19PortCommand）与明确命令 ID 驱动 D19-L02 的合法迁移，
//       输出新状态、事件草案（L16 D19PortEventFactory）与 L17 失败码；
//       同一命令重复投递返回既有稳定结果（幂等，无第二个副作用）；
//       非法迁移原状态不变，仅产生拒绝事件草案。
// 约束：不触碰 Transform/NavMesh/真实时间/坐标；不直接修改任何航海系统内部数值；
//       状态只由本系统拥有；外部系统经 Execute() 公开适配接口消费。
// 依赖：D19PortLifecycleContracts（L02）、D19PortEventContracts（L16）、
//       D19PortFailureCatalog（L17）；只读引用，不复制其类型。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 访问状态机执行结果（D19-L03，所有者：D19PortVisitSystem）。
    /// Result 为稳定命令结果；Events 为事件草案（尚未落日志，由上层 EventLog 追加与去重）。
    /// </summary>
    public sealed class D19PortVisitResult
    {
        /// <summary>稳定命令结果（成功或失败）。</summary>
        public D19PortCommandResult Result;

        /// <summary>本次执行生成的事件草案（幂等命中时为空）。</summary>
        public IList<WorldEvent> Events = new List<WorldEvent>();

        public D19PortVisitResult(D19PortCommandResult result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// 港口到港/离港访问状态机（D19-L03，所有者：本系统）。
    /// 一个系统实例绑定一艘船（构造传入 shipId）；状态只由本系统拥有。
    /// 外部系统通过 Execute() 只读消费结果；不得直接读写内部字段。
    /// </summary>
    public sealed class D19PortVisitSystem
    {
        private const string FallbackRejectionCode = "port.lifecycle.rejected";

        private readonly string _shipId;
        private D19PortVisitState _state = D19PortVisitState.None;
        private string _portId = string.Empty;
        private string _berthId = string.Empty;
        private readonly Dictionary<string, D19PortCommandResult> _completedResults =
            new Dictionary<string, D19PortCommandResult>();

        /// <summary>当前访问状态（只读）。</summary>
        public D19PortVisitState State { get { return _state; } }

        /// <summary>当前港口稳定键（只读）。</summary>
        public string PortId { get { return _portId; } }

        /// <summary>当前泊位稳定键（仅 Berthed/InService 时非空，只读）。</summary>
        public string BerthId { get { return _berthId; } }

        /// <summary>绑定船只稳定键（只读）。</summary>
        public string ShipId { get { return _shipId; } }

        public D19PortVisitSystem(string shipId)
        {
            _shipId = shipId ?? string.Empty;
        }

        /// <summary>
        /// 执行港口命令（幂等 + 确定性）。
        /// 同一 CommandId 重复投递返回既有稳定结果且不产生新事件/状态副作用；
        /// 非法迁移原状态不变，仅生成拒绝事件草案。
        /// </summary>
        public D19PortVisitResult Execute(D19PortCommand command, int worldDay, long saveRevision)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandId))
            {
                return Reject(null, D19PortFailureCode.MissingCommandId, worldDay, saveRevision);
            }

            D19PortCommandResult cached;
            if (_completedResults.TryGetValue(command.CommandId, out cached))
            {
                // 幂等命中：返回既有稳定结果，无新事件、无状态副作用。
                return new D19PortVisitResult(cached);
            }

            if (string.IsNullOrEmpty(command.PortId))
            {
                return Reject(command, D19PortFailureCode.UnknownPort, worldDay, saveRevision);
            }

            D19PortTransitionReason reason;
            if (!TryMapCommandKind(command.Kind, out reason))
            {
                return Reject(command, D19PortFailureCode.InvalidTransition, worldDay, saveRevision);
            }

            D19PortVisitState target;
            D19PortFailureCode failureCode;
            if (!D19PortLifecycleContracts.CanTransition(_state, reason, out target, out failureCode))
            {
                return Reject(command, failureCode, worldDay, saveRevision);
            }

            if (target == D19PortVisitState.Berthed && string.IsNullOrEmpty(command.BerthId))
            {
                return Reject(command, D19PortFailureCode.UnknownBerth, worldDay, saveRevision);
            }

            // 迁移成功：状态只在本系统内变更。
            _state = target;
            _portId = command.PortId;
            if (target == D19PortVisitState.Berthed)
            {
                _berthId = command.BerthId;
            }
            if (target == D19PortVisitState.Departed || target == D19PortVisitState.Returned)
            {
                _berthId = string.Empty;
            }

            D19PortVisitResult visitResult = new D19PortVisitResult(
                new D19PortCommandResult(command.CommandId, true, _state, null));
            visitResult.Events.Add(BuildSuccessEvent(command, reason, worldDay, saveRevision));
            _completedResults[command.CommandId] = visitResult.Result;
            return visitResult;
        }

        /// <summary>命令种类 → 生命周期迁移原因；未知/补偿命令无迁移规则。</summary>
        private static bool TryMapCommandKind(D19PortCommandKind kind, out D19PortTransitionReason reason)
        {
            switch (kind)
            {
                case D19PortCommandKind.EnterPort:
                    reason = D19PortTransitionReason.EnterPortAccepted;
                    return true;
                case D19PortCommandKind.Dock:
                    reason = D19PortTransitionReason.DockAssigned;
                    return true;
                case D19PortCommandKind.AssignBerth:
                    reason = D19PortTransitionReason.BerthAssigned;
                    return true;
                case D19PortCommandKind.StartService:
                    reason = D19PortTransitionReason.ServiceStarted;
                    return true;
                case D19PortCommandKind.CompleteService:
                    reason = D19PortTransitionReason.ServiceCompleted;
                    return true;
                case D19PortCommandKind.Board:
                    reason = D19PortTransitionReason.Boarded;
                    return true;
                case D19PortCommandKind.Depart:
                    reason = D19PortTransitionReason.Departed;
                    return true;
                case D19PortCommandKind.Return:
                    reason = D19PortTransitionReason.Returned;
                    return true;
                default:
                    // Compensate 无迁移规则（补偿策略归 D19-L34）；未知种类一律拒绝。
                    reason = D19PortTransitionReason.None;
                    return false;
            }
        }

        /// <summary>构建成功事件草案（L16 工厂；resultCode 为稳定机器码）。</summary>
        private WorldEvent BuildSuccessEvent(
            D19PortCommand command,
            D19PortTransitionReason reason,
            int worldDay,
            long saveRevision)
        {
            switch (command.Kind)
            {
                case D19PortCommandKind.EnterPort:
                    return D19PortEventFactory.Create(
                        D19PortEventType.Arrival, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.arrival.applied", "", null, null);
                case D19PortCommandKind.Dock:
                    return D19PortEventFactory.Create(
                        D19PortEventType.Arrival, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.dock.applied", "", null, null);
                case D19PortCommandKind.AssignBerth:
                {
                    var values = new Dictionary<string, string>();
                    values["port.berth_id"] = command.BerthId;
                    values["port.berth_lease_world_day"] = "0"; // 租期由 D19-L06 泊位系统裁决，此处占位。
                    return D19PortEventFactory.Create(
                        D19PortEventType.BerthReserved, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.berth.reserved", "", null, values);
                }
                case D19PortCommandKind.StartService:
                    return D19PortEventFactory.Create(
                        D19PortEventType.ServiceResult, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.service.started", "", null, null);
                case D19PortCommandKind.CompleteService:
                    return D19PortEventFactory.Create(
                        D19PortEventType.ServiceResult, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.service.completed", "", null, null);
                case D19PortCommandKind.Board:
                    return D19PortEventFactory.Create(
                        D19PortEventType.Boarded, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.boarding.applied", "", null, null);
                case D19PortCommandKind.Depart:
                    return D19PortEventFactory.Create(
                        D19PortEventType.Departure, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.departure.applied", "", null, null);
                case D19PortCommandKind.Return:
                    return D19PortEventFactory.Create(
                        D19PortEventType.Return, command.CommandId, worldDay, saveRevision,
                        command.PortId, _shipId, null, null, "port.return.applied", "", null, null);
                default:
                    throw new InvalidOperationException("未映射命令种类不允许构建成功事件。");
            }
        }

        /// <summary>拒绝路径：原状态不变，生成拒绝事件草案（L17 稳定失败码）。</summary>
        private D19PortVisitResult Reject(
            D19PortCommand command,
            D19PortFailureCode failureCode,
            int worldDay,
            long saveRevision)
        {
            string commandId = command != null ? command.CommandId : string.Empty;
            string portId = command != null ? command.PortId : string.Empty;
            string reasonCode = ResolveFailureCode(failureCode);
            D19PortVisitResult visitResult = new D19PortVisitResult(
                new D19PortCommandResult(
                    commandId,
                    false,
                    _state,
                    new D19PortFailure(failureCode, reasonCode, failureCode == D19PortFailureCode.BerthOccupied || failureCode == D19PortFailureCode.ServiceUnavailable)));
            if (!string.IsNullOrEmpty(commandId) && !string.IsNullOrEmpty(portId))
            {
                // 只有带命令 ID 的拒绝才生成事件草案（事件必须能关联命令）。
                WorldEvent rejection = D19PortEventFactory.CreateRejection(
                    commandId, worldDay, saveRevision, portId, _shipId, reasonCode, "", null, "");
                visitResult.Events.Add(rejection);
            }
            if (!string.IsNullOrEmpty(commandId) && command != null)
            {
                _completedResults[commandId] = visitResult.Result;
            }
            return visitResult;
        }

        /// <summary>L17 目录解析失败码；未登记码使用稳定回退码（保留原码于失败载体）。</summary>
        private static string ResolveFailureCode(D19PortFailureCode lifecycleCode)
        {
            D19FailureEntry entry = D19PortFailureCatalog.ForLifecycleCode(lifecycleCode);
            return entry != null && !string.IsNullOrEmpty(entry.Code) ? entry.Code : FallbackRejectionCode;
        }
    }
}
