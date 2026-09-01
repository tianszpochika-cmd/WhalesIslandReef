// ============================================================
// D21PortDepartureAdapter —— D21 出航到 D19 港口边界的适配器（D21-L09）
//
// D19PortCommandRouter 是港口访问、泊位和生命周期的唯一写者。本适配器
// 只负责校验 D21 出航请求、构造公开的 D19RoutedPortCommand，并把路由结果
// 投影成 D21 的只读航程启动结果；不复制或改写港口、泊位、船况、货物或船员。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D19;

namespace Whalebone.Systems.D21
{
    /// <summary>
    /// D21 出航请求中的稳定失败码。D19 的拒绝码原样保留在结果中，
    /// 这些码只描述适配器在调用 D19 前发现的输入缺陷。
    /// </summary>
    public static class D21PortDepartureFailureCodes
    {
        public const string RequestRequired = "d21.port.departure.request_required";
        public const string CommandIdRequired = "d21.port.departure.command_id_required";
        public const string CommandIdInvalid = "d21.port.departure.command_id_invalid";
        public const string ActorIdRequired = "d21.port.departure.actor_id_required";
        public const string ShipIdRequired = "d21.port.departure.ship_id_required";
        public const string PortIdRequired = "d21.port.departure.port_id_required";
        public const string InvalidWorldDay = "d21.port.departure.invalid_world_day";
        public const string InvalidSaveRevision = "d21.port.departure.invalid_save_revision";
        public const string InvalidReference = "d21.port.departure.invalid_reference";
    }

    /// <summary>
    /// D21 出航请求。RouteId 与 DestinationPortId 只作为后续航程 owner 的
    /// 不透明引用透传；D19 只接收港口生命周期命令，不被本 DTO 越权写入。
    /// </summary>
    [Serializable]
    public sealed class D21PortDepartureRequest
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string ShipId = string.Empty;
        public string PortId = string.Empty;
        public string BerthId = string.Empty;
        public string RouteId = string.Empty;
        public string DestinationPortId = string.Empty;
        public int WorldDay;
        public long SaveRevision;
        public D19AuthorityClaims Claims;
        public IList<string> ActorIds = new List<string>();
        public Dictionary<string, string> Parameters = new Dictionary<string, string>(StringComparer.Ordinal);

        public D21PortDepartureRequest() { }

        public D21PortDepartureRequest(
            string commandId,
            string actorId,
            string shipId,
            string portId,
            int worldDay,
            long saveRevision,
            D19AuthorityClaims claims = null,
            string routeId = null,
            string destinationPortId = null,
            string berthId = null)
        {
            SchemaVersion = CurrentSchemaVersion;
            CommandId = commandId ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            BerthId = berthId ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            DestinationPortId = destinationPortId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Claims = claims;
        }

        /// <summary>返回防御性副本，避免适配器结果持有调用方的可变列表。</summary>
        public D21PortDepartureRequest Clone()
        {
            var result = new D21PortDepartureRequest
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                PortId = PortId ?? string.Empty,
                BerthId = BerthId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                DestinationPortId = DestinationPortId ?? string.Empty,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                Claims = Claims
            };

            if (ActorIds != null)
            {
                result.ActorIds = new List<string>(ActorIds);
            }

            if (Parameters != null)
            {
                result.Parameters = new Dictionary<string, string>(Parameters, StringComparer.Ordinal);
            }

            return result;
        }
    }

    /// <summary>
    /// 出航后交给 D21 Voyage/Route owner 的只读启动意图。
    /// 本对象不修改航线或船只事实；Applied=false 表示 D19 幂等重放。
    /// </summary>
    [Serializable]
    public sealed class D21VoyageStartEffect
    {
        public string CommandId { get; }
        public string ShipId { get; }
        public string OriginPortId { get; }
        public string DestinationPortId { get; }
        public string RouteId { get; }
        public int WorldDay { get; }
        public long SaveRevision { get; }
        public bool Applied { get; }

        public D21VoyageStartEffect(
            string commandId,
            string shipId,
            string originPortId,
            string destinationPortId,
            string routeId,
            int worldDay,
            long saveRevision,
            bool applied)
        {
            CommandId = commandId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            OriginPortId = originPortId ?? string.Empty;
            DestinationPortId = destinationPortId ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
            Applied = applied;
        }
    }

    /// <summary>出航适配结果；不会暴露任何 D19 私有集合或写入口。</summary>
    [Serializable]
    public sealed class D21PortDepartureResult
    {
        public bool Success { get; }
        public bool IsDuplicate { get; }
        public bool VoyageStarted { get; }
        public bool Applied { get; }
        public string CommandId { get; }
        public string ShipId { get; }
        public string PortId { get; }
        public string RouteId { get; }
        public string DestinationPortId { get; }
        public string FailureCode { get; }
        public string ReasonCode { get; }
        public D19PortVisitState PortStateAfter { get; }
        public D19RoutedCommandResult PortResult { get; }
        public D21VoyageStartEffect VoyageStart { get; }

        private D21PortDepartureResult(
            bool success,
            bool isDuplicate,
            bool voyageStarted,
            bool applied,
            string commandId,
            string shipId,
            string portId,
            string routeId,
            string destinationPortId,
            string failureCode,
            string reasonCode,
            D19PortVisitState portStateAfter,
            D19RoutedCommandResult portResult,
            D21VoyageStartEffect voyageStart)
        {
            Success = success;
            IsDuplicate = isDuplicate;
            VoyageStarted = voyageStarted;
            Applied = applied;
            CommandId = commandId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            PortId = portId ?? string.Empty;
            RouteId = routeId ?? string.Empty;
            DestinationPortId = destinationPortId ?? string.Empty;
            FailureCode = failureCode ?? string.Empty;
            ReasonCode = reasonCode ?? string.Empty;
            PortStateAfter = portStateAfter;
            PortResult = portResult;
            VoyageStart = voyageStart;
        }

        internal static D21PortDepartureResult FromRoute(
            D21PortDepartureRequest request,
            D19RoutedCommandResult portResult)
        {
            bool success = portResult != null && portResult.Success;
            bool duplicate = success && portResult.IsDuplicate;
            bool applied = success && !duplicate;
            string failure = success
                ? string.Empty
                : (portResult == null ? "d21.port.departure.router_result_missing" : portResult.ReasonCode);
            var effect = success
                ? new D21VoyageStartEffect(
                    request.CommandId,
                    request.ShipId,
                    request.PortId,
                    request.DestinationPortId,
                    request.RouteId,
                    request.WorldDay,
                    request.SaveRevision,
                    applied)
                : null;

            return new D21PortDepartureResult(
                success,
                duplicate,
                success,
                applied,
                request.CommandId,
                request.ShipId,
                request.PortId,
                request.RouteId,
                request.DestinationPortId,
                failure,
                portResult == null ? string.Empty : portResult.ReasonCode,
                portResult == null ? D19PortVisitState.None : portResult.StateAfter,
                portResult,
                effect);
        }

        internal static D21PortDepartureResult Rejected(
            D21PortDepartureRequest request,
            string failureCode)
        {
            return new D21PortDepartureResult(
                false,
                false,
                false,
                false,
                request == null ? string.Empty : request.CommandId,
                request == null ? string.Empty : request.ShipId,
                request == null ? string.Empty : request.PortId,
                request == null ? string.Empty : request.RouteId,
                request == null ? string.Empty : request.DestinationPortId,
                failureCode,
                failureCode,
                D19PortVisitState.None,
                null,
                null);
        }
    }

    /// <summary>
    /// D21-L09 港口出航适配器。港口事实只能由注入的 D19 路由器裁决。
    /// </summary>
    public sealed class D21PortDepartureAdapter
    {
        private const int ReferenceMaxLength = 160;
        private const int CommandIdMaxLength = 128;
        private readonly D19PortCommandRouter _router;

        public D21PortDepartureAdapter(D19PortCommandRouter router)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        /// <summary>
        /// 通过 D19 Depart 公共命令请求出航。D21 不直接调用 VisitSystem、BerthSystem
        /// 或任何 D19 工作流内部对象。
        /// </summary>
        public D21PortDepartureResult Depart(D21PortDepartureRequest request)
        {
            string failureCode = Validate(request);
            if (!string.IsNullOrEmpty(failureCode))
            {
                return D21PortDepartureResult.Rejected(request, failureCode);
            }

            var command = new D19RoutedPortCommand(
                request.CommandId,
                D19PortCommandKind.Depart,
                request.ActorId,
                request.ShipId,
                request.PortId,
                request.BerthId,
                string.Empty,
                request.WorldDay,
                request.SaveRevision,
                request.Claims);

            if (request.ActorIds != null)
            {
                command.ActorIds = new List<string>(request.ActorIds);
            }

            if (request.Parameters != null)
            {
                command.Parameters = new Dictionary<string, string>(request.Parameters, StringComparer.Ordinal);
            }

            D19RoutedCommandResult routeResult = _router.Route(command);
            return D21PortDepartureResult.FromRoute(request, routeResult);
        }

        /// <summary>语义别名，供航程命令编排器使用。</summary>
        public D21PortDepartureResult StartVoyage(D21PortDepartureRequest request)
        {
            return Depart(request);
        }

        /// <summary>语义别名，保持“请求出航”命令名称与其它 D21 适配器一致。</summary>
        public D21PortDepartureResult RequestDeparture(D21PortDepartureRequest request)
        {
            return Depart(request);
        }

        private static string Validate(D21PortDepartureRequest request)
        {
            if (request == null) return D21PortDepartureFailureCodes.RequestRequired;
            if (request.SchemaVersion != D21PortDepartureRequest.CurrentSchemaVersion)
                return D21PortDepartureFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.CommandId))
                return D21PortDepartureFailureCodes.CommandIdRequired;
            if (!IsStableReference(request.CommandId, CommandIdMaxLength))
                return D21PortDepartureFailureCodes.CommandIdInvalid;
            if (string.IsNullOrWhiteSpace(request.ActorId))
                return D21PortDepartureFailureCodes.ActorIdRequired;
            if (!IsStableReference(request.ActorId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.ShipId))
                return D21PortDepartureFailureCodes.ShipIdRequired;
            if (!IsStableReference(request.ShipId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (string.IsNullOrWhiteSpace(request.PortId))
                return D21PortDepartureFailureCodes.PortIdRequired;
            if (!IsStableReference(request.PortId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (!string.IsNullOrEmpty(request.BerthId) && !IsStableReference(request.BerthId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (!string.IsNullOrEmpty(request.RouteId) && !IsStableReference(request.RouteId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (!string.IsNullOrEmpty(request.DestinationPortId) &&
                !IsStableReference(request.DestinationPortId, ReferenceMaxLength))
                return D21PortDepartureFailureCodes.InvalidReference;
            if (request.WorldDay < 0) return D21PortDepartureFailureCodes.InvalidWorldDay;
            if (request.SaveRevision < 0) return D21PortDepartureFailureCodes.InvalidSaveRevision;
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
