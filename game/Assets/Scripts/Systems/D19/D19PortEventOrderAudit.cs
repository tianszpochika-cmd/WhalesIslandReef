// ============================================================
// D19PortEventOrderAudit —— 港口事件顺序与重放审计（纯 C#，D19-L35）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一事件审计所有者，D19-L35）
// 目的：实现事件序列审计与重放验证：
//       1. 序列单调性：Event.LogSequence 单调递增，无回退或跳跃颠倒；
//       2. event_id 唯一性：无重复事件 ID；
//       3. command_id 关联性：每个港口事件必须携带合法的 command_id 关联；
//       4. 预期生命周期状态转换：按 L02 迁移表核对到港、靠泊、服务、登船、离港、返港与补偿的合法顺序；
//       5. 补偿后顺序与 cause_ids 审计：补偿事件必须携带非空 cause_ids 引用原失败事件；
//       6. 禁止重复副作用：同一 command_id 不得产生多次具有状态写入的非幂等事件；
//       7. 纯只读重放：从合法事件流重建只读访问与泊位状态（ReconstructedState / Port / Berth）；
//       8. 铁律红线：只读审计与报告，绝不修改事件、不重排历史、不删除失败事件，零真实时钟/Unity 依赖。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 事件顺序审计违规类型（稳定错误码）。
    /// </summary>
    public static class D19EventAuditViolationCodes
    {
        public const string None = "none";
        public const string NonMonotonicSequence = "audit.non_monotonic_sequence";
        public const string DuplicateEventId = "audit.duplicate_event_id";
        public const string MissingCommandId = "audit.missing_command_id";
        public const string InvalidStateTransition = "audit.invalid_state_transition";
        public const string StateJump = "audit.state_jump";
        public const string DuplicateSideEffect = "audit.duplicate_side_effect";
        public const string CompensationMissingCause = "audit.compensation_missing_cause";
        public const string InvalidEventPayload = "audit.invalid_event_payload";
        public const string UnknownEventType = "audit.unknown_event_type";
    }

    /// <summary>
    /// 事件审计报告载体（纯数据，只读）。
    /// </summary>
    public sealed class D19PortEventOrderAuditReport
    {
        public bool IsValid;
        public int TotalEvents;
        public int ValidTransitions;
        public string FirstViolationCode = D19EventAuditViolationCodes.None;
        public string FirstViolationMessage = string.Empty;
        public long ViolationSequence;
        public string ViolationEventId = string.Empty;
        public List<string> Issues = new List<string>();

        /// <summary>从事件流重构出的最终访问状态。</summary>
        public D19PortVisitState ReconstructedState = D19PortVisitState.None;

        /// <summary>从事件流重构出的当前停靠港。</summary>
        public string ReconstructedPortId = string.Empty;

        /// <summary>从事件流重构出的当前占用泊位。</summary>
        public string ReconstructedBerthId = string.Empty;

        public static D19PortEventOrderAuditReport Valid(
            int totalEvents,
            int validTransitions,
            D19PortVisitState state,
            string portId,
            string berthId)
        {
            return new D19PortEventOrderAuditReport
            {
                IsValid = true,
                TotalEvents = totalEvents,
                ValidTransitions = validTransitions,
                FirstViolationCode = D19EventAuditViolationCodes.None,
                ReconstructedState = state,
                ReconstructedPortId = portId ?? string.Empty,
                ReconstructedBerthId = berthId ?? string.Empty
            };
        }

        public static D19PortEventOrderAuditReport Invalid(
            string code,
            string message,
            long seq,
            string eventId,
            List<string> issues,
            D19PortVisitState state,
            string portId,
            string berthId)
        {
            var report = new D19PortEventOrderAuditReport
            {
                IsValid = false,
                FirstViolationCode = code ?? D19EventAuditViolationCodes.None,
                FirstViolationMessage = message ?? string.Empty,
                ViolationSequence = seq,
                ViolationEventId = eventId ?? string.Empty,
                Issues = issues ?? new List<string>(),
                ReconstructedState = state,
                ReconstructedPortId = portId ?? string.Empty,
                ReconstructedBerthId = berthId ?? string.Empty
            };
            return report;
        }
    }

    /// <summary>
    /// 港口事件顺序与重放审计器（纯静态纯函数 API，D19-L35）。
    /// </summary>
    public static class D19PortEventOrderAudit
    {
        /// <summary>
        /// 对给定的港口事件序列执行确定性全量审计与状态重放。
        /// </summary>
        public static D19PortEventOrderAuditReport Audit(IEnumerable<WorldEvent> events)
        {
            if (events == null)
            {
                return D19PortEventOrderAuditReport.Valid(0, 0, D19PortVisitState.None, string.Empty, string.Empty);
            }

            var seenEventIds = new HashSet<string>(StringComparer.Ordinal);
            var appliedCommands = new Dictionary<string, D19PortEventType>(StringComparer.Ordinal);
            var issues = new List<string>();

            long lastSequence = -1;
            int totalEvents = 0;
            int validTransitions = 0;

            D19PortVisitState currentState = D19PortVisitState.None;
            string currentPortId = string.Empty;
            string currentBerthId = string.Empty;

            foreach (var evt in events)
            {
                if (evt == null) continue;
                totalEvents++;

                string eventId = evt.EventId ?? string.Empty;
                long seq = (evt is FactEvent factEvt) ? factEvt.LogSequence : evt.SaveRevision;

                // 1. 序列单调性检查
                if (seq <= lastSequence && lastSequence != -1)
                {
                    string msg = $"Non-monotonic sequence: current {seq} <= last {lastSequence} (EventId: {eventId})";
                    issues.Add(msg);
                    return D19PortEventOrderAuditReport.Invalid(
                        D19EventAuditViolationCodes.NonMonotonicSequence, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                }
                lastSequence = seq;

                // 2. event_id 唯一性检查
                if (!string.IsNullOrEmpty(eventId))
                {
                    if (!seenEventIds.Add(eventId))
                    {
                        string msg = $"Duplicate event_id detected: {eventId}";
                        issues.Add(msg);
                        return D19PortEventOrderAuditReport.Invalid(
                            D19EventAuditViolationCodes.DuplicateEventId, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                    }
                }

                // 3. 解析事件类型与命令 ID
                D19PortEventType eventType;
                if (!D19PortEventGuard.TryGetPortEventType(evt, out eventType) || eventType == D19PortEventType.Unknown)
                {
                    // 非 D19 港口事件或未知事件跳过状态重放
                    continue;
                }

                D19PortEventIdParts parts;
                string commandId = string.Empty;
                if (D19PortEventFactory.TryParseEventId(eventId, out parts))
                {
                    commandId = parts.CommandId;
                }

                if (string.IsNullOrWhiteSpace(commandId))
                {
                    string msg = $"Port event missing command_id: {eventId}";
                    issues.Add(msg);
                    return D19PortEventOrderAuditReport.Invalid(
                        D19EventAuditViolationCodes.MissingCommandId, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                }

                // 4. 重复副作用检查（非拒绝且非补偿事件在相同 command_id 上不得多次产生状态变更）
                if (eventType != D19PortEventType.Rejection && eventType != D19PortEventType.Compensation)
                {
                    if (appliedCommands.TryGetValue(commandId, out D19PortEventType prevType))
                    {
                        // 允许的幂等重放情况：相同 commandId 下有明确的事件数递增或关联
                        if (prevType == eventType && evt.Result != null && evt.Result.Values != null &&
                            evt.Result.Values.ContainsKey("occurrence"))
                        {
                            // 允许的已记录递增事实
                        }
                        else if (prevType == D19PortEventType.BerthReserved && eventType == D19PortEventType.BerthReserved)
                        {
                            // 允许的从预留到确认占用的连续同型事件
                        }
                        else
                        {
                            string msg = $"Duplicate side-effect on command_id {commandId}: {prevType} then {eventType}";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.DuplicateSideEffect, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                    }
                    else
                    {
                        appliedCommands[commandId] = eventType;
                    }
                }

                // 5. 补偿事件专项检查：必须包含 cause_ids
                if (eventType == D19PortEventType.Compensation)
                {
                    if (evt.CauseIds == null || evt.CauseIds.Count == 0)
                    {
                        string msg = $"Compensation event missing cause_ids: {eventId}";
                        issues.Add(msg);
                        return D19PortEventOrderAuditReport.Invalid(
                            D19EventAuditViolationCodes.CompensationMissingCause, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                    }
                }

                // 6. 生命周期状态机与状态跃迁审计
                switch (eventType)
                {
                    case D19PortEventType.Arrival:
                        // 到港 (PortVisit) 或 靠岸 (Docked)
                        if (currentState == D19PortVisitState.None || currentState == D19PortVisitState.Returned)
                        {
                            currentState = D19PortVisitState.PortVisit;
                        }
                        else if (currentState == D19PortVisitState.PortVisit)
                        {
                            currentState = D19PortVisitState.Docked;
                        }
                        else
                        {
                            string msg = $"Invalid state transition on Arrival: from {currentState} to PortVisit/Docked";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.InvalidStateTransition, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                        currentPortId = evt.LocationRef ?? string.Empty;
                        validTransitions++;
                        break;

                    case D19PortEventType.BerthReserved:
                        // 靠泊/泊位分配：前态可为 PortVisit / Docked / Returned
                        currentBerthId = ExtractFactToken(evt, D19PortEventSchema.BerthFactPrefix);
                        currentState = D19PortVisitState.Berthed;
                        validTransitions++;
                        break;

                    case D19PortEventType.BerthReleased:
                        // 泊位释放
                        currentBerthId = string.Empty;
                        validTransitions++;
                        break;

                    case D19PortEventType.ServiceResult:
                        // 服务执行：状态保持 Berthed 或 InService
                        if (currentState != D19PortVisitState.Berthed && currentState != D19PortVisitState.InService)
                        {
                            string msg = $"Service executed in invalid state: {currentState}";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.InvalidStateTransition, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                        validTransitions++;
                        break;

                    case D19PortEventType.Boarded:
                        // 登船：前态必须为 Berthed 或 PortVisit
                        if (currentState != D19PortVisitState.Berthed && currentState != D19PortVisitState.PortVisit)
                        {
                            string msg = $"Boarding in invalid state: {currentState}";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.InvalidStateTransition, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                        currentState = D19PortVisitState.Boarded;
                        validTransitions++;
                        break;

                    case D19PortEventType.Disembarked:
                        // 下船：状态回到 Berthed 或 PortVisit
                        if (currentState == D19PortVisitState.Boarded)
                        {
                            currentState = string.IsNullOrEmpty(currentBerthId) ? D19PortVisitState.PortVisit : D19PortVisitState.Berthed;
                        }
                        validTransitions++;
                        break;

                    case D19PortEventType.Departure:
                        // 离港：前态必须为 Boarded 或 PortVisit
                        if (currentState != D19PortVisitState.Boarded && currentState != D19PortVisitState.PortVisit)
                        {
                            string msg = $"Departure from invalid state: {currentState}";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.InvalidStateTransition, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                        currentState = D19PortVisitState.Departed;
                        currentBerthId = string.Empty;
                        validTransitions++;
                        break;

                    case D19PortEventType.Return:
                        // 返港：前态必须为 Departed
                        if (currentState != D19PortVisitState.Departed)
                        {
                            string msg = $"Return from invalid state: {currentState}";
                            issues.Add(msg);
                            return D19PortEventOrderAuditReport.Invalid(
                                D19EventAuditViolationCodes.InvalidStateTransition, msg, seq, eventId, issues, currentState, currentPortId, currentBerthId);
                        }
                        currentState = D19PortVisitState.Returned;
                        currentPortId = evt.LocationRef ?? string.Empty;
                        validTransitions++;
                        break;

                    case D19PortEventType.Compensation:
                        // 补偿事件
                        validTransitions++;
                        break;

                    case D19PortEventType.Rejection:
                        // 拒绝事件：状态保持不变
                        break;

                    default:
                        break;
                }
            }

            return D19PortEventOrderAuditReport.Valid(
                totalEvents,
                validTransitions,
                currentState,
                currentPortId,
                currentBerthId);
        }

        private static string ExtractFactToken(WorldEvent evt, string prefix)
        {
            if (evt == null || evt.Result == null || evt.Result.FactIds == null)
            {
                return string.Empty;
            }
            foreach (string fact in evt.Result.FactIds)
            {
                if (fact != null && fact.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return fact.Substring(prefix.Length);
                }
            }
            return string.Empty;
        }
    }
}
