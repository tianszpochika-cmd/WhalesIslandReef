// ============================================================
// D19PortCommandRouter —— 唯一港口命令路由器（纯 C#，D19-L33）
//
// 归属：Whalebone.Systems.D19（D19 章节唯一命令路由中枢，D19-L33）
// 目的：将所有 D19 公开命令汇合为唯一入口：
//       1. 预检：命令 ID/指纹构造、权限校验（L23）、生命周期与服务前置校验（L24/L02/L05）；
//       2. 幂等：通过 D19CommandIdempotency（L22）检查已完成记录，重放既有结果；
//       3. 编排分派：分派到访问状态机（L03）、泊位（L06/L07）、登船（L08）、离港（L09）、
//          返港（L10）及市场（L11）、仓库（L12）、酒馆（L13）、船坞（L14）适配器；
//       4. 结果聚合：统一产出纯只读执行结果、更新保存投影（L20）与事件草案（L16），
//          预留中途失败补偿草案（供 L34 消费）；
//       5. 铁律红线：路由器只做分派与编排，绝不夺取市场、库存、船只、船员、航程或
//          Save 的裁决权；不直接改写业务数值，零 UnityEngine/网络/UI 依赖。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>
    /// 统一港口路由命令载体（纯数据，D19-L33）。
    /// </summary>
    public sealed class D19RoutedPortCommand
    {
        public string CommandId = string.Empty;
        public D19PortCommandKind Kind;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        public string PortId = string.Empty;
        public string BerthId = string.Empty;
        public string ServiceId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public D19AuthorityClaims Claims;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        public IList<string> ActorIds;

        public D19RoutedPortCommand() { }

        public D19RoutedPortCommand(
            string commandId,
            D19PortCommandKind kind,
            string actorId,
            string shipId,
            string portId,
            string berthId = null,
            string serviceId = null,
            int worldDay = 0,
            long saveRevision = 0,
            D19AuthorityClaims claims = null)
        {
            CommandId = commandId ?? string.Empty;
            Kind = kind;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            BerthId = berthId ?? string.Empty;
            ServiceId = serviceId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Claims = claims;
        }
    }

    /// <summary>
    /// 统一港口路由执行结果（纯数据，只读）。
    /// </summary>
    public sealed class D19RoutedCommandResult
    {
        public bool Success;
        public bool IsDuplicate;
        public string CommandId = string.Empty;
        public D19PortCommandKind Kind;
        public string ReasonCode = string.Empty;
        public string LocKey = string.Empty;
        public D19FailureRetryPolicy RetryPolicy;
        public D19PortVisitState StateAfter;
        public WorldEvent EventDraft;
        public D19CompensationDraftSaveEntry CompensationDraft;
        public Dictionary<string, string> ResultData = new Dictionary<string, string>(StringComparer.Ordinal);

        public static D19RoutedCommandResult Succeeded(
            string commandId,
            D19PortCommandKind kind,
            D19PortVisitState stateAfter,
            WorldEvent eventDraft = null,
            bool isDuplicate = false)
        {
            return new D19RoutedCommandResult
            {
                Success = true,
                IsDuplicate = isDuplicate,
                CommandId = commandId ?? string.Empty,
                Kind = kind,
                ReasonCode = string.Empty,
                LocKey = D19PortLocalizationKeys.ReadModelCommandAccepted,
                RetryPolicy = D19FailureRetryPolicy.NoRetry,
                StateAfter = stateAfter,
                EventDraft = eventDraft
            };
        }

        public static D19RoutedCommandResult Rejected(
            string commandId,
            D19PortCommandKind kind,
            string reasonCode,
            string locKey,
            D19FailureRetryPolicy retry,
            D19PortVisitState currentState,
            WorldEvent rejectionEvent = null,
            D19CompensationDraftSaveEntry draft = null)
        {
            return new D19RoutedCommandResult
            {
                Success = false,
                IsDuplicate = false,
                CommandId = commandId ?? string.Empty,
                Kind = kind,
                ReasonCode = reasonCode ?? string.Empty,
                LocKey = locKey ?? D19PortLocalizationKeys.ReadModelCommandRejected,
                RetryPolicy = retry,
                StateAfter = currentState,
                EventDraft = rejectionEvent,
                CompensationDraft = draft
            };
        }
    }

    /// <summary>
    /// 唯一港口命令路由器（纯 C#，D19-L33）。
    /// </summary>
    public sealed class D19PortCommandRouter
    {
        private readonly D19PortAnchorCatalog _catalog;
        private readonly D19PortVisitSystem _visitSystem;
        private readonly D19BerthSystem _berthSystem;
        private readonly D19CommandIdempotency _idempotency;

        public D19PortAnchorCatalog Catalog => _catalog;
        public D19PortVisitSystem VisitSystem => _visitSystem;
        public D19BerthSystem BerthSystem => _berthSystem;
        public D19CommandIdempotency Idempotency => _idempotency;

        public D19PortCommandRouter(
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
        /// 统一命令路由入口（确定性）。
        /// 顺序：输入基础校验 -> 幂等预检 -> 权限与归属校验 -> 状态守卫 -> 业务分派 -> 幂等记录与结果返回。
        /// </summary>
        public D19RoutedCommandResult Route(D19RoutedPortCommand cmd)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(cmd.CommandId))
            {
                return D19RoutedCommandResult.Rejected(
                    cmd?.CommandId,
                    cmd?.Kind ?? D19PortCommandKind.None,
                    "d19.failure.missing_command_id",
                    D19PortLocalizationKeys.FailureMissingCommandId,
                    D19FailureRetryPolicy.RetryAfterCorrection,
                    _visitSystem.State);
            }

            // 1. 幂等指纹构造与检查
            string fingerprint = D19CommandIdempotency.BuildFingerprint(
                cmd.Kind.ToString(),
                cmd.ActorId,
                cmd.ShipId,
                cmd.PortId,
                cmd.BerthId,
                cmd.ServiceId);

            var domain = MapDomain(cmd.Kind);
            var idemBegin = _idempotency.Begin(domain, cmd.CommandId, fingerprint, cmd.WorldDay);

            if (idemBegin.Outcome == D19IdempotencyOutcome.Completed)
            {
                // 重复投递重放既有结果
                return D19RoutedCommandResult.Succeeded(
                    cmd.CommandId,
                    cmd.Kind,
                    _visitSystem.State,
                    eventDraft: null,
                    isDuplicate: true);
            }

            if (idemBegin.Outcome == D19IdempotencyOutcome.Conflict)
            {
                return D19RoutedCommandResult.Rejected(
                    cmd.CommandId,
                    cmd.Kind,
                    "d19.save.conflict",
                    D19PortLocalizationKeys.MigrationSaveConflict,
                    D19FailureRetryPolicy.RetrySafe,
                    _visitSystem.State);
            }

            // 2. 权限校验（L23）
            if (cmd.Kind != D19PortCommandKind.Compensate)
            {
                var authContext = new D19AuthorityContext(
                    cmd.Kind,
                    cmd.ActorId,
                    cmd.ShipId,
                    cmd.PortId,
                    cmd.BerthId,
                    cmd.ServiceId,
                    cargoShipId: null);

                var authResult = D19PortAuthorityValidator.Validate(authContext, cmd.Claims, _catalog);
                if (!authResult.Allowed)
                {
                    var rejEvent = D19PortEventFactory.CreateRejection(
                        cmd.CommandId, cmd.WorldDay, cmd.SaveRevision,
                        cmd.PortId, cmd.ShipId, authResult.FailureCode,
                        authResult.Failure?.LocKey ?? D19PortLocalizationKeys.MissingKey,
                        cmd.ActorIds);

                    _idempotency.Abort(domain, cmd.CommandId, fingerprint, authResult.FailureCode, authResult.Failure?.LocKey, cmd.WorldDay);

                    return D19RoutedCommandResult.Rejected(
                        cmd.CommandId, cmd.Kind,
                        authResult.FailureCode,
                        authResult.Failure?.LocKey ?? D19PortLocalizationKeys.MissingKey,
                        authResult.Failure?.Retry ?? D19FailureRetryPolicy.RetryAfterCorrection,
                        _visitSystem.State,
                        rejEvent);
                }
            }

            // 3. 港口访问生命周期与状态守卫预检（L24）
            var portCmdForGuard = new D19PortCommand(cmd.CommandId, cmd.Kind, cmd.PortId, cmd.BerthId);
            var guardVerdict = D19InvalidStateGuard.CheckCommand(
                _catalog,
                portCmdForGuard,
                _visitSystem.State,
                _visitSystem.BerthId,
                D19CrewLocation.Unknown);

            if (!guardVerdict.Allowed)
            {
                var rejEvent = D19PortEventFactory.CreateRejection(
                    cmd.CommandId, cmd.WorldDay, cmd.SaveRevision,
                    cmd.PortId, cmd.ShipId, guardVerdict.BusinessCode,
                    guardVerdict.LocKey,
                    cmd.ActorIds);

                _idempotency.Abort(domain, cmd.CommandId, fingerprint, guardVerdict.BusinessCode, guardVerdict.LocKey, cmd.WorldDay);

                return D19RoutedCommandResult.Rejected(
                    cmd.CommandId, cmd.Kind,
                    guardVerdict.BusinessCode,
                    guardVerdict.LocKey,
                    guardVerdict.Retry,
                    _visitSystem.State,
                    rejEvent);
            }

            // 4. 命令分派与执行
            D19RoutedCommandResult executionResult = DispatchExecution(cmd, domain, fingerprint);

            if (executionResult.Success)
            {
                _idempotency.Complete(
                    domain,
                    cmd.CommandId,
                    fingerprint,
                    executionResult.EventDraft?.Result?.ResultCode ?? "port.success",
                    executionResult.LocKey,
                    1,
                    cmd.WorldDay);
            }
            else
            {
                _idempotency.Abort(
                    domain,
                    cmd.CommandId,
                    fingerprint,
                    executionResult.ReasonCode,
                    executionResult.LocKey,
                    cmd.WorldDay);
            }

            return executionResult;
        }

        private D19RoutedCommandResult DispatchExecution(
            D19RoutedPortCommand cmd,
            D19IdempotencyDomain domain,
            string fingerprint)
        {
            switch (cmd.Kind)
            {
                case D19PortCommandKind.EnterPort:
                case D19PortCommandKind.Dock:
                case D19PortCommandKind.Board:
                case D19PortCommandKind.Depart:
                case D19PortCommandKind.Return:
                case D19PortCommandKind.Compensate:
                    {
                        var portCmd = new D19PortCommand(
                            cmd.CommandId,
                            cmd.Kind,
                            cmd.PortId,
                            cmd.BerthId);

                        var visitResult = _visitSystem.Execute(portCmd, cmd.WorldDay, cmd.SaveRevision);
                        if (visitResult.Result.Success)
                        {
                            return D19RoutedCommandResult.Succeeded(
                                cmd.CommandId,
                                cmd.Kind,
                                _visitSystem.State,
                                visitResult.Events.Count > 0 ? visitResult.Events[0] : null);
                        }
                        else
                        {
                            return D19RoutedCommandResult.Rejected(
                                cmd.CommandId,
                                cmd.Kind,
                                visitResult.Result.Failure.ReasonId,
                                D19PortLocalizationKeys.FailureInvalidTransition,
                                D19FailureRetryPolicy.NoRetry,
                                _visitSystem.State,
                                visitResult.Events.Count > 0 ? visitResult.Events[0] : null);
                        }
                    }

                case D19PortCommandKind.AssignBerth:
                    {
                        // 泊位分配前置：调用 L06 BerthSystem 预留
                        var shipProfile = new D19BerthShipProfile(cmd.ShipId, 1, false);
                        var reserveResult = _berthSystem.Reserve(
                            cmd.CommandId,
                            cmd.PortId,
                            cmd.BerthId,
                            shipProfile,
                            cmd.WorldDay,
                            cmd.WorldDay + 3,
                            cmd.SaveRevision,
                            cmd.ActorIds);

                        if (reserveResult.Success)
                        {
                            // 推进访问状态机到 Berthed
                            var portCmd = new D19PortCommand(cmd.CommandId, D19PortCommandKind.AssignBerth, cmd.PortId, cmd.BerthId);
                            _visitSystem.Execute(portCmd, cmd.WorldDay, cmd.SaveRevision);

                            return D19RoutedCommandResult.Succeeded(
                                cmd.CommandId,
                                cmd.Kind,
                                _visitSystem.State,
                                reserveResult.Event);
                        }
                        else
                        {
                            // 创建补偿草案备选
                            var draft = new D19CompensationDraftSaveEntry
                            {
                                DraftId = "draft_" + cmd.CommandId,
                                OriginalCommandId = cmd.CommandId,
                                Domain = "Berth",
                                Reason = reserveResult.Failure?.ReasonId ?? "d19.berth.occupied",
                                WorldDay = cmd.WorldDay,
                                SaveRevision = cmd.SaveRevision
                            };

                            return D19RoutedCommandResult.Rejected(
                                cmd.CommandId,
                                cmd.Kind,
                                reserveResult.Failure?.ReasonId ?? "d19.berth.occupied",
                                D19PortLocalizationKeys.FailureBerthOccupied,
                                D19FailureRetryPolicy.RetryAfterCorrection,
                                _visitSystem.State,
                                reserveResult.Event,
                                draft);
                        }
                    }

                case D19PortCommandKind.StartService:
                    {
                        var portCmd = new D19PortCommand(cmd.CommandId, D19PortCommandKind.StartService, cmd.PortId, cmd.BerthId);
                        var visitResult = _visitSystem.Execute(portCmd, cmd.WorldDay, cmd.SaveRevision);
                        if (visitResult.Result.Success)
                        {
                            return D19RoutedCommandResult.Succeeded(
                                cmd.CommandId,
                                cmd.Kind,
                                _visitSystem.State,
                                visitResult.Events.Count > 0 ? visitResult.Events[0] : null);
                        }
                        else
                        {
                            return D19RoutedCommandResult.Rejected(
                                cmd.CommandId,
                                cmd.Kind,
                                visitResult.Result.Failure.ReasonId,
                                D19PortLocalizationKeys.FailureServiceUnavailable,
                                D19FailureRetryPolicy.RetrySafe,
                                _visitSystem.State,
                                visitResult.Events.Count > 0 ? visitResult.Events[0] : null);
                        }
                    }

                case D19PortCommandKind.CompleteService:
                    {
                        var portCmd = new D19PortCommand(cmd.CommandId, D19PortCommandKind.CompleteService, cmd.PortId, cmd.BerthId);
                        var visitResult = _visitSystem.Execute(portCmd, cmd.WorldDay, cmd.SaveRevision);
                        return D19RoutedCommandResult.Succeeded(
                            cmd.CommandId,
                            cmd.Kind,
                            _visitSystem.State,
                            visitResult.Events.Count > 0 ? visitResult.Events[0] : null);
                    }

                default:
                    return D19RoutedCommandResult.Rejected(
                        cmd.CommandId,
                        cmd.Kind,
                        "d19.lifecycle.invalid_transition",
                        D19PortLocalizationKeys.FailureInvalidTransition,
                        D19FailureRetryPolicy.NoRetry,
                        _visitSystem.State);
            }
        }

        private static D19IdempotencyDomain MapDomain(D19PortCommandKind kind)
        {
            switch (kind)
            {
                case D19PortCommandKind.AssignBerth: return D19IdempotencyDomain.Berth;
                case D19PortCommandKind.Board: return D19IdempotencyDomain.Boarding;
                case D19PortCommandKind.Depart: return D19IdempotencyDomain.Departure;
                case D19PortCommandKind.StartService:
                case D19PortCommandKind.CompleteService: return D19IdempotencyDomain.Service;
                default: return D19IdempotencyDomain.Service;
            }
        }
    }
}
