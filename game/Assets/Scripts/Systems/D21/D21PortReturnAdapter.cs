// ============================================================
// D21PortReturnAdapter —— D21 返港/请求靠泊到 D19 的适配器（D21-L10）
//
// D19PortCommandRouter 负责港口访问和泊位事实。本适配器先路由 Return
// 结束航程；若调用方声明目标泊位，再以稳定派生命令路由 AssignBerth。
// 两步结果分开保留，避免把“返港成功”误报成“靠泊成功”，也不直接访问
// D19 Visit/Berth 私有状态。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D21
{
    /// <summary>返港适配器在调用 D19 前发现的输入失败码。</summary>
    public static class D21PortReturnFailureCodes
    {
        public const string RequestRequired = "d21.port.return.request_required";
        public const string CommandIdRequired = "d21.port.return.command_id_required";
        public const string CommandIdInvalid = "d21.port.return.command_id_invalid";
        public const string ActorIdRequired = "d21.port.return.actor_id_required";
        public const string ShipIdRequired = "d21.port.return.ship_id_required";
        public const string PortIdRequired = "d21.port.return.port_id_required";
        public const string InvalidWorldDay = "d21.port.return.invalid_world_day";
        public const string InvalidSaveRevision = "d21.port.return.invalid_save_revision";
        public const string InvalidReference = "d21.port.return.invalid_reference";
        public const string BerthCommandIdTooLong = "d21.port.return.berth_command_id_too_long";
    }

    /// <summary>
    /// D21 返港请求。TargetBerthId 为空时只请求返港；非空时请求在返港
    /// 成功后继续由 D19 分配该泊位。D21 不在本 DTO 中保存泊位占用事实。
    /// </summary>
    [Serializable]
    public sealed class D21PortReturnRequest
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        public string PortId = string.Empty;
        public string TargetBerthId = string.Empty;
        public string RouteId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public D19AuthorityClaims Claims;
        public IList<string> ActorIds = new List<string>();
        public Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        public D21PortReturnRequest() { }

        public D21PortReturnRequest(
            string commandId,
            string actorId,
            string shipId,
            string portId,
            int worldDay,
            long saveRevision,
            D19AuthorityClaims claims = null,
            string targetBerthId = null,
            string routeId = null)
        {
            SchemaVersion = CurrentSchemaVersion;
            CommandId = commandId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            TargetBerthId = targetBerthId ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Claims = claims;
        }
    }

    /// <summary>返港事实被 D19 接受后的只读航程结束意图。</summary>
    [Serializable]
    public sealed class D21VoyageEndEffect
    {
        public string CommandId { get; }
        public string ShipId { get; }
        public string DestinationPortId { get; }
        public string RouteId { get; }
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public bool Applied { get; }

        public D21VoyageEndEffect(
            string commandId,
            string shipId,
            string destinationPortId,
            string routeId,
            int worldDay,
            long saveRevision,
            bool applied)
        {
            CommandId = commandId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            DestinationPortId = destinationPortId ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Applied = applied;
        }
    }

    /// <summary>
    /// 返港/靠泊两步结果。Success 只有在所有请求步骤都成功时为 true；
    /// VoyageEnded 可在靠泊失败时仍为 true，明确表达已发生的 D19 事实。
    /// </summary>
    [Serializable]
    public sealed class D21PortReturnResult
    {
        public bool Success { get; }
        public bool IsDuplicate { get; }
        public bool VoyageEnded { get; }
        public bool ReturnApplied { get; }
        public bool BerthRequested { get; }
        public bool BerthApplied { get; }
        public bool BerthIsDuplicate { get; }
        public string CommandId { get; }
        public string BerthCommandId { get; }
        public string ShipId { get; }
        public string PortId { get; }
        public string TargetBerthId { get; }
        public string FailureCode { get; }
        public string ReasonCode { get; }
        public D19PortVisitState PortStateAfter { get; }
        public D19RoutedCommandResult ReturnResult { get; }
        public D19RoutedCommandResult BerthResult { get; }
        public D21VoyageEndEffect VoyageEnd { get; }

        private D21PortReturnResult(
            bool success,
            bool isDuplicate,
            bool voyageEnded,
            bool returnApplied,
            bool berthRequested,
            bool berthApplied,
            bool berthIsDuplicate,
            string commandId,
            string berthCommandId,
            string shipId,
            string portId,
            string targetBerthId,
            string failureCode,
            string reasonCode,
            D19PortVisitState stateAfter,
            D19RoutedCommandResult returnResult,
            D19RoutedCommandResult berthResult,
            D21VoyageEndEffect voyageEnd)
        {
            Success = success;
            IsDuplicate = isDuplicate;
            VoyageEnded = voyageEnded;
            ReturnApplied = returnApplied;
            BerthRequested = berthRequested;
            BerthApplied = berthApplied;
            BerthIsDuplicate = berthIsDuplicate;
            CommandId = commandId ?? string.Empty;
            BerthCommandId = berthCommandId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            TargetBerthId = targetBerthId ?? string.Empty;
            FailureCode = failureCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            PortStateAfter = stateAfter;
            ReturnResult = returnResult;
            BerthResult = berthResult;
            VoyageEnd = voyageEnd;
        }

        internal static D21PortReturnResult FromRoutes(
            D21PortReturnRequest request,
            string berthCommandId,
            D19RoutedCommandResult returnResult,
            D19RoutedCommandResult berthResult)
        {
            bool returned = returnResult != null && returnResult.Success;
            bool returnDuplicate = returned && returnResult.IsDuplicate;
            bool berthRequested = !string.IsNullOrEmpty(request.TargetBerthId);
            bool berthed = !berthRequested || (berthResult != null && berthResult.Success);
            bool berthDuplicate = berthRequested && berthResult != null && berthResult.IsDuplicate;
            bool success = returned && berthed;
            bool applied = returned && !returnDuplicate;
            string failure = string.Empty;
            if (!returned)
            {
                failure = returnResult == null
                    ? "d21.port.return.router_result_missing"
                    : returnResult.ReasonCode;
            }
            else if (!berthed)
            {
                failure = berthResult == null
                    ? "d21.port.return.berth_router_result_missing"
                    : berthResult.ReasonCode;
            }

            D19PortVisitState state = returnResult == null
                ? D19PortVisitState.None
                : returnResult.StateAfter;
            if (berthResult != null && berthResult.Success)
            {
                state = berthResult.StateAfter;
            }

            return new D21PortReturnResult(
                success,
                returned && returnDuplicate && (!berthRequested || berthDuplicate),
                returned,
                applied,
                berthRequested,
                berthRequested && berthResult != null && berthResult.Success,
                berthDuplicate,
                request.CommandId,
                berthCommandId,
                request.ShipId,
                request.PortId,
                request.TargetBerthId,
                failure,
                failure,
                state,
                returnResult,
                berthResult,
                returned
                    ? new D21VoyageEndEffect(
                        request.CommandId,
                        request.ShipId,
                        request.PortId,
                        request.RouteId,
                        request.WorldDay,
                        request.SaveRevision,
                        applied)
                    : null);
        }

        internal static D21PortReturnResult Rejected(
            D21PortReturnRequest request,
            string failureCode)
        {
            return new D21PortReturnResult(
                false,
                false,
                false,
                false,
                request != null && !string.IsNullOrEmpty(request.TargetBerthId),
                false,
                false,
                request == null ? string.Empty : request.CommandId,
                string.Empty,
                request == null ? string.Empty : request.ShipId,
                request == null ? string.Empty : request.PortId,
                request == null ? string.Empty : request.TargetBerthId,
                failureCode,
                failureCode,
                D19PortVisitState.None,
                null,
                null,
                null);
        }
    }

    /// <summary>D21-L10 返港适配器；D19 是唯一的港口/泊位事实写者。</summary>
    public sealed class D21PortReturnAdapter
    {
        private const int ReferenceMaxLength = 160;
        private const int CommandIdMaxLength = 128;
        private const string BerthCommandSuffix = ":berth";
        private readonly D19PortCommandRouter _router;

        public D21PortReturnAdapter(D19PortCommandRouter router)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        /// <summary>
        /// 路由 Return；若请求了泊位，再路由 AssignBerth。两个命令均由
        /// D19 的公开 Route 入口裁决，适配器不调用任何 D19 内部系统。
        /// </summary>
        public D21PortReturnResult Return(D21PortReturnRequest request)
        {
            string failureCode = Validate(request);
            if (!string.IsNullOrEmpty(failureCode))
            {
                return D21PortReturnResult.Rejected(request, failureCode);
            }

            var returnCommand = new D19RoutedPortCommand(
                request.CommandId,
                D19PortCommandKind.Return,
                request.ActorId,
                request.ShipId,
                request.PortId,
                string.Empty,
                string.Empty,
                request.WorldDay,
                request.SaveRevision,
                request.Claims);
            CopyPayload(request, returnCommand);
            D19RoutedCommandResult returnResult = _router.Route(returnCommand);

            D19RoutedCommandResult berthResult = null;
            string berthCommandId = string.Empty;
            if (returnResult != null && returnResult.Success && !string.IsNullOrEmpty(request.TargetBerthId))
            {
                berthCommandId = request.CommandId + BerthCommandSuffix;
                var berthCommand = new D19RoutedPortCommand(
                    berthCommandId,
                    D19PortCommandKind.AssignBerth,
                    request.ActorId,
                    request.ShipId,
                    request.PortId,
                    request.TargetBerthId,
                    string.Empty,
                    request.WorldDay,
                    request.SaveRevision,
                    request.Claims);
                CopyPayload(request, berthCommand);
                berthResult = _router.Route(berthCommand);
            }

            return D21PortReturnResult.FromRoutes(request, berthCommandId, returnResult, berthResult);
        }

        /// <summary>语义别名，供航程编排器使用。</summary>
        public D21PortReturnResult RequestReturn(D21PortReturnRequest request)
        {
            return Return(request);
        }

        /// <summary>语义别名，强调返港后可选的靠泊请求。</summary>
        public D21PortReturnResult ReturnToPort(D21PortReturnRequest request)
        {
            return Return(request);
        }

        private static void CopyPayload(D21PortReturnRequest request, D19RoutedPortCommand command)
        {
            if (request.ActorIds != null)
            {
                command.ActorIds = new List<string>(request.ActorIds);
            }

            if (request.Parameters != null)
            {
                command.Parameters = new Dictionary<string, string>(request.Parameters, StringComparer.Ordinal);
            }
        }

        private static string Validate(D21PortReturnRequest request)
        {
            if (request == null) return D21PortReturnFailureCodes.RequestRequired;
            if (request.SchemaVersion != D21PortReturnRequest.CurrentSchemaVersion)
                return D21PortReturnFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.CommandId))
                return D21PortReturnFailureCodes.CommandIdRequired;
            if (!IsStableReference(request.CommandId, CommandIdMaxLength))
                return D21PortReturnFailureCodes.CommandIdInvalid;
            if (string.IsNullOrWhiteSpace(request.ActorId))
                return D21PortReturnFailureCodes.ActorIdRequired;
            if (!IsStableReference(request.ActorId, ReferenceMaxLength))
                return D21PortReturnFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.ShipId))
                return D21PortReturnFailureCodes.ShipIdRequired;
            if (!IsStableReference(request.ShipId, ReferenceMaxLength))
                return D21PortReturnFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.PortId))
                return D21PortReturnFailureCodes.PortIdRequired;
            if (!IsStableReference(request.PortId, ReferenceMaxLength))
                return D21PortReturnFailureCodes.InvalidReference;
            if (!string.IsNullOrEmpty(request.TargetBerthId) &&
                !IsStableReference(request.TargetBerthId, ReferenceMaxLength))
                return D21PortReturnFailureCodes.InvalidReference;
            if (!string.IsNullOrEmpty(request.RouteId) && !IsStableReference(request.RouteId, ReferenceMaxLength))
                return D21PortReturnFailureCodes.InvalidReference;
            if (request.WorldDay < 0) return D21PortReturnFailureCodes.InvalidWorldDay;
            if (request.SaveRevision < 0) return D21PortReturnFailureCodes.InvalidSaveRevision;
            if (!string.IsNullOrEmpty(request.TargetBerthId) &&
                !IsStableReference(request.CommandId + BerthCommandSuffix, CommandIdMaxLength))
                return D21PortReturnFailureCodes.BerthCommandIdTooLong;
            return string.Empty;
        }

        private static bool IsStableReference(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length > maxLength || value.Trim() != value)
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool letter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!letter && !digit && !separator) return false;
            }

            return true;
        }
    }
}
