// ============================================================
// D19PortEventContracts —— D19 港口领域可重放事件契约（D19-L16）
//
// 本文件只定义港口领域事件的稳定契约与工厂：event_id 构造规则、
// event_type 枚举、schema/version、port/ship/actor/command 关联 ID、
// 逻辑时间、不可变 payload 与序列化边界。
//
// 约束（与 D19-L01 基线第 1.6/2 节一致）：
//   * 事件必须接入既有 EventLog（Systems/Event/），不另起事件总线；
//   * 事件不计算或写入任何金钱、价格、伤害、库存、航行数值（owner 裁决）；
//   * 事件不直接含 UI 文案、Unity 对象或可变系统引用；
//   * 顺序来自 EventLog 的单调 log_sequence 与 world_day（逻辑时间），
//     不依赖真实时钟或场景；
//   * 去重键 = event_id + save_revision（EventLog.DedupeKey）；
//   * 未知事件类型/未知版本可被上层安全拒绝（TryGetPortEventType /
//     EnsureSupportedVersion）。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>港口事件 Schema 版本与 ID 常量（冻结）。</summary>
    public static class D19PortEventSchema
    {
        /// <summary>港口领域事件 schema 版本；未知版本必须被上层安全拒绝。</summary>
        public const int CurrentVersion = 1;

        /// <summary>D19 港口领域的 owner_system 值（与 D19-L01 基线一致）。</summary>
        public const string OwnerSystem = "D19PortSystem";

        /// <summary>事件 ID 前缀：d19pe_&lt;type&gt;_&lt;commandId&gt;_&lt;occurrence&gt;。</summary>
        public const string EventIdPrefix = "d19pe_";

        /// <summary>input_facts / result.fact_ids 关联前缀（稳定，不写展示名）。</summary>
        public const string CommandFactPrefix = "command:";
        public const string PortFactPrefix = "port:";
        public const string ShipFactPrefix = "ship:";
        public const string BerthFactPrefix = "berth:";
        public const string ServiceFactPrefix = "service:";

        /// <summary>result_code 必须使用的前缀（稳定机器码，非 UI 文案）。</summary>
        public const string ResultCodePrefix = "port.";

        /// <summary>result.summary 若非空必须是本地化 key（禁止直接写 UI 文案）。</summary>
        public static readonly Regex LocalizationKeyPattern =
            new Regex(@"^d19\.port\.[a-z0-9_.]+$", RegexOptions.Compiled);

        /// <summary>非法值（非字符串）不得进入 payload；所有值必须是 string。</summary>
        public static readonly Regex SafeValuePattern =
            new Regex(@"^[\u0020-\u007E\u4E00-\u9FFF]*$", RegexOptions.Compiled);
    }

    /// <summary>港口领域事件类型（冻结枚举；Unknown 表示未知/不可识别）。</summary>
    public enum D19PortEventType
    {
        Unknown = 0,
        Arrival = 1,        // 到港
        BerthReserved = 2,  // 泊位预留
        BerthReleased = 3,  // 泊位释放
        ServiceResult = 4,  // 服务结果
        Boarded = 5,        // 登船
        Disembarked = 6,    // 下船
        Departure = 7,      // 离港
        Return = 8,         // 返港
        Compensation = 9,   // 补偿
        Rejection = 10,     // 拒绝
    }

    /// <summary>事件 ID 的解析结果：命令 ID、事件类型与同命令内发生序号。</summary>
    public sealed class D19PortEventIdParts
    {
        public string CommandId = string.Empty;
        public D19PortEventType Type = D19PortEventType.Unknown;
        public int Occurrence;
    }

    /// <summary>
    /// 港口领域事件的构造工厂。只生成 WorldEvent（尚未落日志的事实包），
    /// 不执行任何领域状态变更；追加与去重由 EventLog 完成。
    /// </summary>
    public static class D19PortEventFactory
    {
        /// <summary>
        /// 通用构造：全部关联通过 input_facts / actor_ids / cause_ids /
        /// result.fact_ids 表达；payload 只允许 string 值。
        /// </summary>
        public static WorldEvent Create(
            D19PortEventType type,
            string commandId,
            int worldDay,
            long saveRevision,
            string portId,
            string shipId,
            IList<string> actorIds,
            IList<string> causeIds,
            string resultCode,
            string summaryKey,
            IList<string> factIds,
            IDictionary<string, string> values,
            int occurrence = 0)
        {
            if (type == D19PortEventType.Unknown)
                throw new EventValidationException("Unknown port event type cannot be created.");
            if (string.IsNullOrWhiteSpace(commandId))
                throw new EventValidationException("Port event command_id is required.");
            if (string.IsNullOrWhiteSpace(portId))
                throw new EventValidationException("Port event port_id is required.");
            if (string.IsNullOrWhiteSpace(shipId))
                throw new EventValidationException("Port event ship_id is required.");
            if (string.IsNullOrWhiteSpace(resultCode) || !resultCode.StartsWith(D19PortEventSchema.ResultCodePrefix, StringComparison.Ordinal))
                throw new EventValidationException("Port event result_code must start with '" + D19PortEventSchema.ResultCodePrefix + "'.");
            if (!string.IsNullOrEmpty(summaryKey) && !D19PortEventSchema.LocalizationKeyPattern.IsMatch(summaryKey))
                throw new EventValidationException("Port event summary must be a localization key (d19.port.*), never raw UI text: " + summaryKey + ".");

            var inputFacts = new List<string>();
            inputFacts.Add(D19PortEventSchema.CommandFactPrefix + commandId);
            inputFacts.Add(D19PortEventSchema.PortFactPrefix + portId);
            inputFacts.Add(D19PortEventSchema.ShipFactPrefix + shipId);
            if (factIds != null)
                foreach (var factId in factIds)
                    if (!string.IsNullOrWhiteSpace(factId))
                        inputFacts.Add(factId);

            var result = new EventResult();
            result.ResultCode = resultCode;
            result.Summary = summaryKey ?? string.Empty;
            if (factIds != null)
                foreach (var factId in factIds)
                    if (!string.IsNullOrWhiteSpace(factId))
                        result.FactIds.Add(factId);
            if (values != null)
                foreach (var pair in values)
                {
                    if (pair.Value == null)
                        throw new EventValidationException("Port event payload values must be strings: " + pair.Key + ".");
                    if (!D19PortEventSchema.SafeValuePattern.IsMatch(pair.Value))
                        throw new EventValidationException("Port event payload value is not a safe serializable string: " + pair.Key + ".");
                    result.Values[pair.Key] = pair.Value;
                }

            string eventId = BuildEventId(type, commandId, occurrence);
            var actors = new List<string>();
            if (actorIds != null)
                foreach (var actor in actorIds)
                    if (!string.IsNullOrWhiteSpace(actor))
                        actors.Add(actor);
            var causes = new List<string>();
            if (causeIds != null)
                foreach (var cause in causeIds)
                    if (!string.IsNullOrWhiteSpace(cause))
                        causes.Add(cause);

            return new WorldEvent(
                eventId,
                worldDay,
                portId,
                D19PortEventSchema.OwnerSystem,
                result,
                actors,
                inputFacts,
                causes,
                saveRevision,
                WorldEventStatus.Normal);
        }

        /// <summary>稳定事件 ID：d19pe_&lt;type&gt;_&lt;commandId&gt;_&lt;occurrence&gt;。</summary>
        public static string BuildEventId(D19PortEventType type, string commandId, int occurrence = 0)
        {
            if (type == D19PortEventType.Unknown)
                throw new EventValidationException("Unknown port event type cannot build an event id.");
            if (string.IsNullOrWhiteSpace(commandId))
                throw new EventValidationException("Port event command_id is required to build an event id.");
            if (occurrence < 0)
                throw new EventValidationException("Port event occurrence must be >= 0.");
            string token = type.ToString().ToLowerInvariant();
            return D19PortEventSchema.EventIdPrefix + token + "_" + commandId + "_" +
                occurrence.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>解析事件 ID；返回 false 表示未知/格式非法，可被上层安全拒绝。</summary>
        public static bool TryParseEventId(string eventId, out D19PortEventIdParts parts)
        {
            parts = new D19PortEventIdParts();
            if (string.IsNullOrWhiteSpace(eventId))
                return false;
            string prefix = D19PortEventSchema.EventIdPrefix;
            if (!eventId.StartsWith(prefix, StringComparison.Ordinal))
                return false;
            string body = eventId.Substring(prefix.Length);
            int lastSeparator = body.LastIndexOf('_');
            int typeSeparator = body.IndexOf('_');
            int occurrence;
            if (typeSeparator <= 0 || lastSeparator <= typeSeparator ||
                !int.TryParse(body.Substring(lastSeparator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out occurrence) ||
                occurrence < 0)
                return false;
            string commandId = body.Substring(typeSeparator + 1, lastSeparator - typeSeparator - 1);
            string typeToken = body.Substring(0, typeSeparator);
            D19PortEventType type = D19PortEventType.Unknown;
            foreach (D19PortEventType candidate in Enum.GetValues(typeof(D19PortEventType)))
            {
                if (candidate != D19PortEventType.Unknown &&
                    candidate.ToString().Equals(typeToken, StringComparison.OrdinalIgnoreCase))
                {
                    type = candidate;
                    break;
                }
            }
            if (type == D19PortEventType.Unknown)
                return false;
            parts.CommandId = commandId;
            parts.Type = type;
            parts.Occurrence = occurrence;
            return true;
        }

        // ---- 语义化便捷工厂（全部复用 Create，payload 只含稳定事实） ----

        public static WorldEvent CreateArrival(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> actorIds, IList<string> causeIds,
            string summaryKey = "", IDictionary<string, string> extraValues = null)
        {
            return Create(D19PortEventType.Arrival, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.arrival.applied", summaryKey, null, extraValues);
        }

        public static WorldEvent CreateBerthReserved(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, string berthId, string untilWorldDay,
            IList<string> actorIds, IList<string> causeIds, string summaryKey = "")
        {
            var values = new Dictionary<string, string>();
            values[D19PortEventSchema.BerthFactPrefix + "id"] = berthId;
            if (!string.IsNullOrEmpty(untilWorldDay))
                values["berth.until_day"] = untilWorldDay;
            return Create(D19PortEventType.BerthReserved, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.berth.reserved", summaryKey,
                new[] { D19PortEventSchema.BerthFactPrefix + berthId }, values);
        }

        public static WorldEvent CreateBerthReleased(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, string berthId, string reason,
            IList<string> actorIds, IList<string> causeIds, string summaryKey = "")
        {
            var values = new Dictionary<string, string>();
            values["berth.release_reason"] = reason ?? "completed";
            return Create(D19PortEventType.BerthReleased, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.berth.released", summaryKey,
                new[] { D19PortEventSchema.BerthFactPrefix + berthId }, values);
        }

        public static WorldEvent CreateServiceResult(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, string serviceId, string resultCode,
            IList<string> actorIds, IList<string> causeIds, string summaryKey = "",
            IList<string> factIds = null, IDictionary<string, string> values = null)
        {
            var allFacts = new List<string>();
            allFacts.Add(D19PortEventSchema.ServiceFactPrefix + serviceId);
            if (factIds != null)
                allFacts.AddRange(factIds);
            return Create(D19PortEventType.ServiceResult, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, resultCode, summaryKey, allFacts, values);
        }

        public static WorldEvent CreateBoarded(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> actorIds, IList<string> causeIds,
            string summaryKey = "")
        {
            return Create(D19PortEventType.Boarded, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.boarding.applied", summaryKey, null, null);
        }

        public static WorldEvent CreateDisembarked(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> actorIds, IList<string> causeIds,
            string summaryKey = "")
        {
            return Create(D19PortEventType.Disembarked, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.boarding.disembarked", summaryKey, null, null);
        }

        public static WorldEvent CreateDeparture(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> actorIds, IList<string> causeIds,
            string summaryKey = "")
        {
            return Create(D19PortEventType.Departure, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.departure.applied", summaryKey, null, null);
        }

        public static WorldEvent CreateReturn(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> actorIds, IList<string> causeIds,
            string summaryKey = "")
        {
            return Create(D19PortEventType.Return, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causeIds, "port.return.applied", summaryKey, null, null);
        }

        /// <summary>
        /// 补偿事件：cause_ids 必须引用被补偿的原始事件 ID（可审计回滚）。
        /// </summary>
        public static WorldEvent CreateCompensation(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, IList<string> originalEventIds,
            IList<string> actorIds, string summaryKey = "")
        {
            if (originalEventIds == null || originalEventIds.Count == 0)
                throw new EventValidationException("Compensation event requires at least one original event id in cause_ids.");
            return Create(D19PortEventType.Compensation, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, originalEventIds, "port.compensation.applied", summaryKey, null, null);
        }

        /// <summary>
        /// 拒绝事件：result_code 必须为 port.rejected（reason 存 payload，稳定机器码）。
        /// </summary>
        public static WorldEvent CreateRejection(string commandId, int worldDay, long saveRevision,
            string portId, string shipId, string reasonCode, string rejectedEventId,
            IList<string> actorIds, string summaryKey = "")
        {
            var values = new Dictionary<string, string>();
            values["port.reject_reason"] = reasonCode;
            values["port.rejected_event_id"] = rejectedEventId ?? string.Empty;
            var causes = new List<string>();
            if (!string.IsNullOrEmpty(rejectedEventId))
                causes.Add(rejectedEventId);
            return Create(D19PortEventType.Rejection, commandId, worldDay, saveRevision, portId, shipId,
                actorIds, causes, "port.rejected", summaryKey, null, values);
        }
    }

    /// <summary>
    /// 港口事件守卫：未知事件类型/未知版本可被上层安全拒绝；结构不变量在此集中校验。
    /// </summary>
    public static class D19PortEventGuard
    {
        /// <summary>
        /// 校验事件是否为本契约可识别的港口事件；未知类型返回 false（安全拒绝，
        /// 上层可忽略或按未知事件处理）。
        /// </summary>
        public static bool TryGetPortEventType(WorldEvent worldEvent, out D19PortEventType type)
        {
            type = D19PortEventType.Unknown;
            if (worldEvent == null)
                return false;
            if (!string.Equals(worldEvent.OwnerSystem, D19PortEventSchema.OwnerSystem, StringComparison.Ordinal))
                return false;
            return TryParseEventIdInternal(worldEvent.EventId, out type);
        }

        /// <summary>版本守卫：未知 schema 版本抛出 EventSchemaVersionException（安全拒绝）。</summary>
        public static void EnsureSupportedVersion(int schemaVersion)
        {
            if (schemaVersion != D19PortEventSchema.CurrentVersion)
                throw new EventSchemaVersionException(
                    "D19 port event schema_version mismatch: expected " + D19PortEventSchema.CurrentVersion +
                    ", received " + schemaVersion + ".");
        }

        /// <summary>
        /// 结构校验：必须在 append 前调用（WorldEvent.Validate 之外再校验港口关联）。
        /// 失败抛出 EventValidationException；不会产生副作用。
        /// </summary>
        public static void Validate(WorldEvent worldEvent)
        {
            D19PortEventType type;
            if (!TryGetPortEventType(worldEvent, out type))
                throw new EventValidationException("Event is not a recognized D19 port event: " +
                    (worldEvent == null ? "<null>" : worldEvent.EventId) + ".");
            if (worldEvent.SchemaVersion != D19PortEventSchema.CurrentVersion)
                throw new EventSchemaVersionException(
                    "D19 port event schema_version mismatch: expected " + D19PortEventSchema.CurrentVersion +
                    ", received " + worldEvent.SchemaVersion + ".");

            var inputFacts = worldEvent.InputFacts ?? new List<string>();
            bool hasCommand = false;
            bool hasPort = false;
            bool hasShip = false;
            foreach (var fact in inputFacts)
            {
                if (string.IsNullOrWhiteSpace(fact))
                    continue;
                if (fact.StartsWith(D19PortEventSchema.CommandFactPrefix, StringComparison.Ordinal))
                    hasCommand = true;
                else if (fact.StartsWith(D19PortEventSchema.PortFactPrefix, StringComparison.Ordinal))
                    hasPort = true;
                else if (fact.StartsWith(D19PortEventSchema.ShipFactPrefix, StringComparison.Ordinal))
                    hasShip = true;
            }
            if (!hasCommand)
                throw new EventValidationException("Port event must carry a command:<commandId> input fact: " + worldEvent.EventId + ".");
            if (!hasPort)
                throw new EventValidationException("Port event must carry a port:<portId> input fact: " + worldEvent.EventId + ".");
            if (!hasShip)
                throw new EventValidationException("Port event must carry a ship:<shipId> input fact: " + worldEvent.EventId + ".");

            var result = worldEvent.Result;
            if (result == null ||
                string.IsNullOrWhiteSpace(result.ResultCode) ||
                !result.ResultCode.StartsWith(D19PortEventSchema.ResultCodePrefix, StringComparison.Ordinal))
                throw new EventValidationException("Port event result_code must start with 'port.': " + worldEvent.EventId + ".");
            if (!string.IsNullOrEmpty(result.Summary) && !D19PortEventSchema.LocalizationKeyPattern.IsMatch(result.Summary))
                throw new EventValidationException("Port event summary must be a localization key (d19.port.*), never raw UI text: " + worldEvent.EventId + ".");

            if (type == D19PortEventType.Compensation && (worldEvent.CauseIds == null || worldEvent.CauseIds.Count == 0))
                throw new EventValidationException("Compensation event must reference original events in cause_ids: " + worldEvent.EventId + ".");
            if (type == D19PortEventType.Rejection &&
                (result.Values == null || !result.Values.ContainsKey("port.reject_reason")))
                throw new EventValidationException("Rejection event must carry port.reject_reason in values: " + worldEvent.EventId + ".");
        }

        private static bool TryParseEventIdInternal(string eventId, out D19PortEventType type)
        {
            type = D19PortEventType.Unknown;
            D19PortEventIdParts parts;
            if (!D19PortEventFactory.TryParseEventId(eventId, out parts))
                return false;
            type = parts.Type;
            return true;
        }
    }
}
