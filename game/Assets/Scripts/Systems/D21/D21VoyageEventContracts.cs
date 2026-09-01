// ============================================================
// D21VoyageEventContracts —— D21-L18 航程事实事件契约
//
// 事件是所属 owner 已裁决结果的不可变包，不在这里改变船、天气、补给、
// 船员或生态状态。event_id + save_revision 是去重键；tick、command、
// cause 和 result 都以稳定字符串/整数序列化，便于中断后继续和确定性重放。
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D21
{
    /// <summary>D21 航程事件类型；Unknown 只能用于拒绝未知输入。</summary>
    public enum D21VoyageEventType
    {
        Unknown = 0,
        VoyageStarted = 1,
        CourseChanged = 2,
        VoyageTickAdvanced = 3,
        RiskQualified = 4,
        RiskResolved = 5,
        ProvisionConsumed = 6,
        Stopped = 7,
        Arrived = 8,
        Interrupted = 9,
        Recovered = 10,
        Rejected = 11,
    }

    /// <summary>事件结果生命周期；Duplicate 是追加结果，也可被保存以供审计。</summary>
    public enum D21VoyageEventOutcome
    {
        Normal = 0,
        Failed = 1,
        Cancelled = 2,
        Interrupted = 3,
        Recovered = 4,
        Duplicate = 5,
    }

    /// <summary>航程事件结果的稳定载荷；值只能是可序列化字符串。</summary>
    [Serializable]
    public sealed class D21VoyageEventResult
    {
        public bool Succeeded = true;
        public bool Applied = true;
        public string ResultCode = string.Empty;
        public List<string> FactIds = new List<string>();
        public Dictionary<string, string> Values = new Dictionary<string, string>(StringComparer.Ordinal);

        public string result_code { get { return ResultCode; } set { ResultCode = value; } }
        public List<string> fact_ids { get { return FactIds; } set { FactIds = value; } }
        public Dictionary<string, string> values { get { return Values; } set { Values = value; } }

        public D21VoyageEventResult Clone()
        {
            var result = new D21VoyageEventResult
            {
                Succeeded = Succeeded,
                Applied = Applied,
                ResultCode = ResultCode ?? string.Empty,
            };
            if (FactIds != null) result.FactIds.AddRange(FactIds);
            if (Values != null)
                foreach (KeyValuePair<string, string> pair in Values)
                    result.Values[pair.Key] = pair.Value;
            return result;
        }

        internal void Validate(string eventId)
        {
            if (string.IsNullOrWhiteSpace(ResultCode))
                throw new InvalidOperationException("D21 event result_code is required for " + eventId + ".");
            if (FactIds == null || Values == null)
                throw new InvalidOperationException("D21 event result collections are required for " + eventId + ".");
            ValidateIds(FactIds, "result.fact_ids", eventId);
            foreach (KeyValuePair<string, string> pair in Values)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
                    throw new InvalidOperationException("D21 event result values must contain non-null string pairs for " + eventId + ".");
            }
        }

        internal static void ValidateIds(List<string> values, string fieldName, string eventId)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
                    throw new InvalidOperationException("D21 event " + fieldName + " contains an empty or duplicate id for " + eventId + ".");
            }
        }
    }

    /// <summary>一条 D21 航程事实事件；所有集合在构造/clone 时保持独立。</summary>
    [Serializable]
    public sealed class D21VoyageEvent
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string EventId = string.Empty;
        public D21VoyageEventType EventType;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public int WorldDay;
        public int MinuteOfDay;
        public int TickIndex;
        public long SaveRevision;
        public long VoyageSeed;
        public string CommandId = string.Empty;
        public string OwnerSystem = D21VoyageEventContracts.OwnerSystem;
        public List<string> InputFacts = new List<string>();
        public List<string> CauseIds = new List<string>();
        public D21VoyageEventResult Result = new D21VoyageEventResult();
        public D21VoyageEventOutcome Outcome = D21VoyageEventOutcome.Normal;
        public string RecoveryRouteId = string.Empty;
        public long LogSequence;

        // 对接通用事件/JSON 命名时提供的稳定别名。
        public D21VoyageEventType Type { get { return EventType; } set { EventType = value; } }
        public string event_id { get { return EventId; } set { EventId = value; } }
        public int schema_version { get { return SchemaVersion; } set { SchemaVersion = value; } }
        public string event_type { get { return EventType.ToString(); } set { EventType = ParseType(value); } }
        public string voyage_id { get { return VoyageId; } set { VoyageId = value; } }
        public string ship_id { get { return ShipId; } set { ShipId = value; } }
        public string route_id { get { return RouteId; } set { RouteId = value; } }
        public int world_day { get { return WorldDay; } set { WorldDay = value; } }
        public int minute_of_day { get { return MinuteOfDay; } set { MinuteOfDay = value; } }
        public int tick_index { get { return TickIndex; } set { TickIndex = value; } }
        public long save_revision { get { return SaveRevision; } set { SaveRevision = value; } }
        public long voyage_seed { get { return VoyageSeed; } set { VoyageSeed = value; } }
        public string command_id { get { return CommandId; } set { CommandId = value; } }
        public string owner_system { get { return OwnerSystem; } set { OwnerSystem = value; } }
        public List<string> input_facts { get { return InputFacts; } set { InputFacts = value; } }
        public List<string> cause_ids { get { return CauseIds; } set { CauseIds = value; } }
        public D21VoyageEventResult result { get { return Result; } set { Result = value; } }
        public D21VoyageEventOutcome outcome_status { get { return Outcome; } set { Outcome = value; } }
        public string recovery_route_id { get { return RecoveryRouteId; } set { RecoveryRouteId = value; } }
        public long log_sequence { get { return LogSequence; } set { LogSequence = value; } }

        public string DedupeKey
        {
            get
            {
                return (EventId ?? string.Empty) + "\u001f" + SaveRevision.ToString(CultureInfo.InvariantCulture);
            }
        }

        public D21VoyageEvent Clone()
        {
            var result = new D21VoyageEvent
            {
                SchemaVersion = SchemaVersion,
                EventId = EventId ?? string.Empty,
                EventType = EventType,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                WorldDay = WorldDay,
                MinuteOfDay = MinuteOfDay,
                TickIndex = TickIndex,
                SaveRevision = SaveRevision,
                VoyageSeed = VoyageSeed,
                CommandId = CommandId ?? string.Empty,
                OwnerSystem = OwnerSystem ?? string.Empty,
                Result = Result == null ? null : Result.Clone(),
                Outcome = Outcome,
                RecoveryRouteId = RecoveryRouteId ?? string.Empty,
                LogSequence = LogSequence,
            };
            if (InputFacts != null) result.InputFacts.AddRange(InputFacts);
            if (CauseIds != null) result.CauseIds.AddRange(CauseIds);
            return result;
        }

        public void Validate()
        {
            D21VoyageEventContracts.Validate(this);
        }

        public WorldEvent ToWorldEvent()
        {
            return D21VoyageEventContracts.ToWorldEvent(this);
        }

        private static D21VoyageEventType ParseType(string value)
        {
            D21VoyageEventType type;
            return Enum.TryParse(value, true, out type) ? type : D21VoyageEventType.Unknown;
        }
    }

    /// <summary>事件追加的幂等结果。</summary>
    [Serializable]
    public sealed class D21VoyageEventAppendResult
    {
        public bool Succeeded;
        public bool IsDuplicate;
        public string Code = string.Empty;
        public D21VoyageEvent Event;
    }

    /// <summary>事件日志最小保存投影；不保存任何其他 owner 的数值状态。</summary>
    [Serializable]
    public sealed class D21VoyageEventLogSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public long NextSequence = 1;
        public List<D21VoyageEvent> Events = new List<D21VoyageEvent>();

        public D21VoyageEventLogSnapshot Clone()
        {
            var result = new D21VoyageEventLogSnapshot
            {
                SchemaVersion = SchemaVersion,
                NextSequence = NextSequence,
            };
            if (Events != null)
                foreach (D21VoyageEvent item in Events)
                    if (item != null) result.Events.Add(item.Clone());
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || NextSequence < 1)
                throw new InvalidOperationException("D21 voyage event log schema or sequence is invalid.");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            long previousSequence = 0;
            if (Events == null) throw new InvalidOperationException("D21 voyage event log events are required.");
            foreach (D21VoyageEvent item in Events)
            {
                if (item == null) throw new InvalidOperationException("D21 voyage event log cannot contain null events.");
                item.Validate();
                if (item.LogSequence <= previousSequence || item.LogSequence >= NextSequence)
                    throw new InvalidOperationException("D21 voyage event log sequence is not monotonic.");
                if (!keys.Add(item.DedupeKey))
                    throw new InvalidOperationException("D21 voyage event log contains a duplicate event key.");
                previousSequence = item.LogSequence;
            }
        }
    }

    /// <summary>事件日志只追加 D21 已裁决事件，并通过 event_id + revision 去重。</summary>
    public sealed class D21VoyageEventLog
    {
        private readonly List<D21VoyageEvent> _events = new List<D21VoyageEvent>();
        private readonly Dictionary<string, D21VoyageEvent> _eventIndex =
            new Dictionary<string, D21VoyageEvent>(StringComparer.Ordinal);
        private long _nextSequence = 1;

        public IReadOnlyList<D21VoyageEvent> Events { get { return _events.AsReadOnly(); } }
        public long NextSequence { get { return _nextSequence; } }

        public D21VoyageEventAppendResult Append(D21VoyageEvent candidate)
        {
            if (candidate == null)
                return new D21VoyageEventAppendResult { Succeeded = false, Code = "missing_event" };
            D21VoyageEvent copy = candidate.Clone();
            try
            {
                copy.Validate();
                D21VoyageEvent existing;
                if (_eventIndex.TryGetValue(copy.DedupeKey, out existing))
                {
                    return new D21VoyageEventAppendResult
                    {
                        Succeeded = true,
                        IsDuplicate = true,
                        Code = "duplicate_event",
                        Event = existing.Clone(),
                    };
                }
                copy.LogSequence = _nextSequence;
                _events.Add(copy);
                _eventIndex[copy.DedupeKey] = copy;
                _nextSequence = checked(_nextSequence + 1);
                return new D21VoyageEventAppendResult
                {
                    Succeeded = true,
                    IsDuplicate = false,
                    Code = "event_appended",
                    Event = copy.Clone(),
                };
            }
            catch (OverflowException)
            {
                return new D21VoyageEventAppendResult { Succeeded = false, Code = "sequence_overflow" };
            }
            catch (InvalidOperationException)
            {
                return new D21VoyageEventAppendResult { Succeeded = false, Code = "invalid_event" };
            }
        }

        public D21VoyageEventLogSnapshot BuildSnapshot()
        {
            var result = new D21VoyageEventLogSnapshot { NextSequence = _nextSequence };
            foreach (D21VoyageEvent item in _events) result.Events.Add(item.Clone());
            return result;
        }

        public static D21VoyageEventLog Restore(D21VoyageEventLogSnapshot snapshot)
        {
            if (snapshot == null) throw new InvalidOperationException("D21 voyage event log snapshot is required.");
            snapshot.Validate();
            var result = new D21VoyageEventLog();
            foreach (D21VoyageEvent item in snapshot.Events)
            {
                D21VoyageEventAppendResult append = result.Append(item);
                if (!append.Succeeded || append.IsDuplicate)
                    throw new InvalidOperationException("D21 voyage event log snapshot cannot be restored.");
            }
            if (result._nextSequence != snapshot.NextSequence)
                throw new InvalidOperationException("D21 voyage event log next sequence cannot be restored.");
            return result;
        }
    }

    /// <summary>D21 事件工厂/守卫和通用 WorldEvent 适配入口。</summary>
    public static class D21VoyageEventContracts
    {
        public const int CurrentSchemaVersion = D21VoyageEvent.CurrentSchemaVersion;
        public const string OwnerSystem = "D21VoyageSystem";
        public const string EventIdPrefix = "d21ve_";
        public const string CausalIdPrefix = "cause:";

        public static D21VoyageEvent Create(
            D21VoyageEventType eventType,
            string voyageId,
            string shipId,
            string routeId,
            D21TickContext context,
            string resultCode,
            IEnumerable<string> inputFacts = null,
            IEnumerable<string> causeIds = null,
            D21VoyageEventOutcome outcome = D21VoyageEventOutcome.Normal,
            string commandId = "",
            string recoveryRouteId = "",
            int ordinal = 0,
            IDictionary<string, string> values = null,
            IEnumerable<string> resultFactIds = null)
        {
            if (context == null) throw new InvalidOperationException("D21 event tick context is required.");
            context.Validate();
            var result = new D21VoyageEventResult
            {
                Succeeded = outcome == D21VoyageEventOutcome.Normal || outcome == D21VoyageEventOutcome.Recovered,
                Applied = outcome != D21VoyageEventOutcome.Failed && outcome != D21VoyageEventOutcome.Cancelled &&
                    outcome != D21VoyageEventOutcome.Interrupted,
                ResultCode = resultCode ?? string.Empty,
            };
            if (resultFactIds != null) result.FactIds.AddRange(resultFactIds);
            if (values != null)
                foreach (KeyValuePair<string, string> pair in values) result.Values[pair.Key] = pair.Value;

            var eventFacts = new List<string>();
            if (inputFacts != null) eventFacts.AddRange(inputFacts);
            var eventCauses = new List<string>();
            if (causeIds != null) eventCauses.AddRange(causeIds);
            var item = new D21VoyageEvent
            {
                EventId = BuildEventId(eventType, voyageId, context.TickIndex, ordinal),
                EventType = eventType,
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                RouteId = routeId ?? string.Empty,
                WorldDay = context.WorldDay,
                MinuteOfDay = context.MinuteOfDay,
                TickIndex = context.TickIndex,
                SaveRevision = context.SaveRevision,
                VoyageSeed = context.VoyageSeed,
                CommandId = commandId ?? string.Empty,
                OwnerSystem = OwnerSystem,
                InputFacts = eventFacts,
                CauseIds = eventCauses,
                Result = result,
                Outcome = outcome,
                RecoveryRouteId = recoveryRouteId ?? string.Empty,
            };
            item.Validate();
            return item;
        }

        public static D21VoyageEvent CreateWithEventId(
            D21VoyageEventType eventType,
            string eventId,
            string voyageId,
            string shipId,
            string routeId,
            D21TickContext context,
            D21VoyageEventResult result,
            IEnumerable<string> inputFacts = null,
            IEnumerable<string> causeIds = null,
            D21VoyageEventOutcome outcome = D21VoyageEventOutcome.Normal,
            string commandId = "",
            string recoveryRouteId = "")
        {
            if (context == null) throw new InvalidOperationException("D21 event tick context is required.");
            context.Validate();
            var item = new D21VoyageEvent
            {
                EventId = eventId ?? string.Empty,
                EventType = eventType,
                VoyageId = voyageId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                RouteId = routeId ?? string.Empty,
                WorldDay = context.WorldDay,
                MinuteOfDay = context.MinuteOfDay,
                TickIndex = context.TickIndex,
                SaveRevision = context.SaveRevision,
                VoyageSeed = context.VoyageSeed,
                CommandId = commandId ?? string.Empty,
                OwnerSystem = OwnerSystem,
                Result = result == null ? new D21VoyageEventResult() : result.Clone(),
                Outcome = outcome,
                RecoveryRouteId = recoveryRouteId ?? string.Empty,
            };
            if (inputFacts != null) item.InputFacts.AddRange(inputFacts);
            if (causeIds != null) item.CauseIds.AddRange(causeIds);
            item.Validate();
            return item;
        }

        public static string BuildEventId(
            D21VoyageEventType eventType,
            string voyageId,
            int tickIndex,
            int ordinal = 0)
        {
            if (eventType == D21VoyageEventType.Unknown || string.IsNullOrWhiteSpace(voyageId) ||
                tickIndex < 0 || ordinal < 0)
                throw new InvalidOperationException("D21 event id input is invalid.");
            return EventIdPrefix + eventType.ToString().ToLowerInvariant() + "_" + voyageId + "_t" +
                tickIndex.ToString(CultureInfo.InvariantCulture) + "_o" + ordinal.ToString(CultureInfo.InvariantCulture);
        }

        public static string BuildCausalId(string parentEventId, string relation)
        {
            if (string.IsNullOrWhiteSpace(parentEventId) || string.IsNullOrWhiteSpace(relation))
                throw new InvalidOperationException("D21 causal id requires parent event and relation.");
            return CausalIdPrefix + parentEventId + ":" + relation;
        }

        public static void Validate(D21VoyageEvent item)
        {
            if (item == null) throw new InvalidOperationException("D21 voyage event is required.");
            if (item.SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D21 voyage event schema_version mismatch.");
            if (string.IsNullOrWhiteSpace(item.EventId) || item.EventType == D21VoyageEventType.Unknown ||
                !Enum.IsDefined(typeof(D21VoyageEventType), item.EventType))
                throw new InvalidOperationException("D21 voyage event id or type is invalid.");
            if (string.IsNullOrWhiteSpace(item.VoyageId) || string.IsNullOrWhiteSpace(item.ShipId) ||
                string.IsNullOrWhiteSpace(item.RouteId) || string.IsNullOrWhiteSpace(item.OwnerSystem))
                throw new InvalidOperationException("D21 voyage event ids and owner_system are required.");
            if (!string.Equals(item.OwnerSystem, OwnerSystem, StringComparison.Ordinal))
                throw new InvalidOperationException("D21 voyage event owner_system is not D21.");
            if (item.WorldDay < 0 || item.MinuteOfDay < 0 || item.MinuteOfDay >= D21TickContext.MinutesPerDay ||
                item.TickIndex < 0 || item.SaveRevision < 0 || item.VoyageSeed < 0 || item.LogSequence < 0)
                throw new InvalidOperationException("D21 voyage event counters are outside their ranges.");
            if (!Enum.IsDefined(typeof(D21VoyageEventOutcome), item.Outcome))
                throw new InvalidOperationException("D21 voyage event outcome is unknown.");
            if (item.InputFacts == null || item.CauseIds == null)
                throw new InvalidOperationException("D21 voyage event fact collections are required.");
            D21VoyageEventResult.ValidateIds(item.InputFacts, "input_facts", item.EventId);
            D21VoyageEventResult.ValidateIds(item.CauseIds, "cause_ids", item.EventId);
            if (item.Result == null) throw new InvalidOperationException("D21 voyage event result is required.");
            item.Result.Validate(item.EventId);
            if ((item.Outcome == D21VoyageEventOutcome.Failed || item.Outcome == D21VoyageEventOutcome.Interrupted) &&
                string.IsNullOrWhiteSpace(item.RecoveryRouteId))
                throw new InvalidOperationException("D21 failed/interrupted event requires recovery_route_id.");
            if (item.Outcome == D21VoyageEventOutcome.Recovered && item.CauseIds.Count == 0)
                throw new InvalidOperationException("D21 recovered event requires a cause event.");
        }

        public static WorldEvent ToWorldEvent(D21VoyageEvent item)
        {
            if (item == null) throw new InvalidOperationException("D21 voyage event is required.");
            item.Validate();
            WorldEventStatus status = WorldEventStatus.Normal;
            if (item.Outcome == D21VoyageEventOutcome.Failed) status = WorldEventStatus.Failed;
            else if (item.Outcome == D21VoyageEventOutcome.Cancelled) status = WorldEventStatus.Cancelled;
            else if (item.Outcome == D21VoyageEventOutcome.Interrupted) status = WorldEventStatus.Interrupted;
            else if (item.Outcome == D21VoyageEventOutcome.Recovered) status = WorldEventStatus.Recovered;

            var facts = new List<string> { "voyage:" + item.VoyageId, "ship:" + item.ShipId };
            facts.AddRange(item.InputFacts);
            var result = new EventResult
            {
                ResultCode = item.Result.ResultCode,
                Summary = string.Empty,
            };
            result.FactIds.AddRange(item.Result.FactIds);
            foreach (KeyValuePair<string, string> pair in item.Result.Values) result.Values[pair.Key] = pair.Value;
            RecoveryRoute recovery = null;
            if (!string.IsNullOrEmpty(item.RecoveryRouteId))
                recovery = new RecoveryRoute
                {
                    RouteId = item.RecoveryRouteId,
                    Action = status == WorldEventStatus.Recovered ? "resume" : "recover",
                    ResumeFromEventId = item.EventId,
                };
            var actors = new List<string> { item.ShipId };
            return new WorldEvent(item.EventId, item.WorldDay, item.RouteId, OwnerSystem,
                result, actors, facts, item.CauseIds, item.SaveRevision, status, recovery);
        }
    }

    /// <summary>D21 事件的确定性 JSON 编解码器；不依赖 Unity JsonUtility 或第三方库。</summary>
    public static class D21VoyageEventSerializer
    {
        public static string Serialize(D21VoyageEvent item)
        {
            if (item == null) throw new InvalidOperationException("D21 voyage event is required.");
            item.Validate();
            var sb = new StringBuilder(512);
            sb.Append('{');
            Field(sb, "event_id", item.EventId); sb.Append(',');
            Field(sb, "schema_version", item.SchemaVersion); sb.Append(',');
            Field(sb, "event_type", item.EventType.ToString()); sb.Append(',');
            Field(sb, "voyage_id", item.VoyageId); sb.Append(',');
            Field(sb, "ship_id", item.ShipId); sb.Append(',');
            Field(sb, "route_id", item.RouteId); sb.Append(',');
            Field(sb, "world_day", item.WorldDay); sb.Append(',');
            Field(sb, "minute_of_day", item.MinuteOfDay); sb.Append(',');
            Field(sb, "tick_index", item.TickIndex); sb.Append(',');
            Field(sb, "save_revision", item.SaveRevision); sb.Append(',');
            Field(sb, "voyage_seed", item.VoyageSeed); sb.Append(',');
            Field(sb, "command_id", item.CommandId); sb.Append(',');
            Field(sb, "owner_system", item.OwnerSystem); sb.Append(',');
            Field(sb, "input_facts", item.InputFacts); sb.Append(',');
            Field(sb, "cause_ids", item.CauseIds); sb.Append(',');
            sb.Append("\"result\":"); WriteResult(sb, item.Result); sb.Append(',');
            Field(sb, "outcome_status", item.Outcome.ToString()); sb.Append(',');
            Field(sb, "recovery_route_id", item.RecoveryRouteId); sb.Append(',');
            Field(sb, "log_sequence", item.LogSequence);
            sb.Append('}');
            return sb.ToString();
        }

        public static string SerializeMany(IEnumerable<D21VoyageEvent> items)
        {
            var sb = new StringBuilder(1024);
            sb.Append('[');
            bool first = true;
            if (items != null)
                foreach (D21VoyageEvent item in items)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append(Serialize(item));
                }
            sb.Append(']');
            return sb.ToString();
        }

        public static D21VoyageEvent Deserialize(string json)
        {
            try
            {
                Dictionary<string, object> root = D21CanonicalJson.ParseObject(json);
                var item = new D21VoyageEvent
                {
                    EventId = D21CanonicalJson.GetString(root, "event_id"),
                    SchemaVersion = D21CanonicalJson.GetInt(root, "schema_version"),
                    EventType = ParseEnum<D21VoyageEventType>(D21CanonicalJson.GetString(root, "event_type")),
                    VoyageId = D21CanonicalJson.GetString(root, "voyage_id"),
                    ShipId = D21CanonicalJson.GetString(root, "ship_id"),
                    RouteId = D21CanonicalJson.GetString(root, "route_id"),
                    WorldDay = D21CanonicalJson.GetInt(root, "world_day"),
                    MinuteOfDay = D21CanonicalJson.GetInt(root, "minute_of_day"),
                    TickIndex = D21CanonicalJson.GetInt(root, "tick_index"),
                    SaveRevision = D21CanonicalJson.GetLong(root, "save_revision"),
                    VoyageSeed = D21CanonicalJson.GetLong(root, "voyage_seed"),
                    CommandId = D21CanonicalJson.GetString(root, "command_id"),
                    OwnerSystem = D21CanonicalJson.GetString(root, "owner_system"),
                    Outcome = ParseEnum<D21VoyageEventOutcome>(D21CanonicalJson.GetString(root, "outcome_status")),
                    RecoveryRouteId = D21CanonicalJson.GetString(root, "recovery_route_id"),
                    LogSequence = D21CanonicalJson.GetLong(root, "log_sequence"),
                };
                item.InputFacts = D21CanonicalJson.GetStringList(root, "input_facts");
                item.CauseIds = D21CanonicalJson.GetStringList(root, "cause_ids");
                Dictionary<string, object> resultObject = D21CanonicalJson.GetObject(root, "result");
                item.Result = ReadResult(resultObject);
                item.Validate();
                return item;
            }
            catch (Exception ex) when (!(ex is D21EventSerializationException))
            {
                throw new D21EventSerializationException("D21 voyage event JSON is invalid: " + ex.Message, ex);
            }
        }

        public static bool TryDeserialize(string json, out D21VoyageEvent item, out string error)
        {
            try
            {
                item = Deserialize(json);
                error = string.Empty;
                return true;
            }
            catch (D21EventSerializationException ex)
            {
                item = null;
                error = ex.Message;
                return false;
            }
        }

        private static D21VoyageEventResult ReadResult(Dictionary<string, object> root)
        {
            var result = new D21VoyageEventResult
            {
                Succeeded = D21CanonicalJson.GetBool(root, "succeeded"),
                Applied = D21CanonicalJson.GetBool(root, "applied"),
                ResultCode = D21CanonicalJson.GetString(root, "result_code"),
                FactIds = D21CanonicalJson.GetStringList(root, "fact_ids"),
                Values = D21CanonicalJson.GetStringMap(root, "values"),
            };
            return result;
        }

        private static void WriteResult(StringBuilder sb, D21VoyageEventResult result)
        {
            if (result == null) throw new InvalidOperationException("D21 event result is required.");
            sb.Append('{');
            Field(sb, "succeeded", result.Succeeded); sb.Append(',');
            Field(sb, "applied", result.Applied); sb.Append(',');
            Field(sb, "result_code", result.ResultCode); sb.Append(',');
            Field(sb, "fact_ids", result.FactIds); sb.Append(',');
            sb.Append("\"values\":"); WriteMap(sb, result.Values);
            sb.Append('}');
        }

        private static void Field(StringBuilder sb, string name, string value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':'); D21CanonicalJson.WriteString(sb, value ?? string.Empty);
        }

        private static void Field(StringBuilder sb, string name, int value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Field(StringBuilder sb, string name, long value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void Field(StringBuilder sb, string name, bool value)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':').Append(value ? "true" : "false");
        }

        private static void Field(StringBuilder sb, string name, List<string> values)
        {
            D21CanonicalJson.WriteString(sb, name); sb.Append(':'); WriteList(sb, values);
        }

        private static void WriteList(StringBuilder sb, IEnumerable<string> values)
        {
            sb.Append('[');
            bool first = true;
            if (values != null)
                foreach (string value in values)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    D21CanonicalJson.WriteString(sb, value ?? string.Empty);
                }
            sb.Append(']');
        }

        private static void WriteMap(StringBuilder sb, IDictionary<string, string> values)
        {
            sb.Append('{');
            bool first = true;
            var keys = new List<string>();
            if (values != null)
                foreach (string key in values.Keys) keys.Add(key);
            keys.Sort(StringComparer.Ordinal);
            foreach (string key in keys)
            {
                if (!first) sb.Append(',');
                first = false;
                D21CanonicalJson.WriteString(sb, key); sb.Append(':');
                D21CanonicalJson.WriteString(sb, values[key] ?? string.Empty);
            }
            sb.Append('}');
        }

        private static T ParseEnum<T>(string value) where T : struct
        {
            T parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : default(T);
        }
    }

    [Serializable]
    public sealed class D21EventSerializationException : Exception
    {
        public D21EventSerializationException(string message) : base(message) { }
        public D21EventSerializationException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>供 D21 事件和 Save projection 共用的无第三方 JSON 中间解析器。</summary>
    internal static class D21CanonicalJson
    {
        public static string Stringify(object value)
        {
            var sb = new StringBuilder(256);
            WriteValue(sb, value);
            return sb.ToString();
        }

        public static Dictionary<string, object> ParseObject(string json)
        {
            object value = new Parser(json ?? string.Empty).Parse();
            Dictionary<string, object> result = value as Dictionary<string, object>;
            if (result == null) throw new FormatException("D21 JSON root must be an object.");
            return result;
        }

        public static string GetString(Dictionary<string, object> root, string key)
        {
            object value = Required(root, key);
            string result = value as string;
            if (result == null) throw new FormatException("D21 JSON field '" + key + "' must be a string.");
            return result;
        }

        public static int GetInt(Dictionary<string, object> root, string key)
        {
            long value = GetLong(root, key);
            if (value < int.MinValue || value > int.MaxValue)
                throw new FormatException("D21 JSON field '" + key + "' is outside int range.");
            return (int)value;
        }

        public static long GetLong(Dictionary<string, object> root, string key)
        {
            object value = Required(root, key);
            if (value is long) return (long)value;
            if (value is int) return (int)value;
            if (value is double)
            {
                double number = (double)value;
                if (number >= long.MinValue && number <= long.MaxValue && Math.Truncate(number) == number)
                    return (long)number;
            }
            throw new FormatException("D21 JSON field '" + key + "' must be an integer.");
        }

        public static bool GetBool(Dictionary<string, object> root, string key)
        {
            object value = Required(root, key);
            if (value is bool) return (bool)value;
            throw new FormatException("D21 JSON field '" + key + "' must be a boolean.");
        }

        public static Dictionary<string, object> GetObject(Dictionary<string, object> root, string key)
        {
            Dictionary<string, object> value = Required(root, key) as Dictionary<string, object>;
            if (value == null) throw new FormatException("D21 JSON field '" + key + "' must be an object.");
            return value;
        }

        public static List<string> GetStringList(Dictionary<string, object> root, string key)
        {
            var values = Required(root, key) as List<object>;
            if (values == null) throw new FormatException("D21 JSON field '" + key + "' must be an array.");
            var result = new List<string>();
            foreach (object value in values)
            {
                string text = value as string;
                if (text == null) throw new FormatException("D21 JSON array '" + key + "' must contain strings.");
                result.Add(text);
            }
            return result;
        }

        public static List<object> GetArray(Dictionary<string, object> root, string key)
        {
            List<object> values = Required(root, key) as List<object>;
            if (values == null) throw new FormatException("D21 JSON field '" + key + "' must be an array.");
            return values;
        }

        public static Dictionary<string, string> GetStringMap(Dictionary<string, object> root, string key)
        {
            Dictionary<string, object> values = GetObject(root, key);
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object> pair in values)
            {
                string text = pair.Value as string;
                if (text == null) throw new FormatException("D21 JSON map '" + key + "' must contain strings.");
                result[pair.Key] = text;
            }
            return result;
        }

        public static void WriteString(StringBuilder sb, string value)
        {
            sb.Append('"');
            string text = value ?? string.Empty;
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private static void WriteValue(StringBuilder sb, object value)
        {
            if (value == null) { sb.Append("null"); return; }
            string text = value as string;
            if (text != null) { WriteString(sb, text); return; }
            if (value is bool) { sb.Append((bool)value ? "true" : "false"); return; }
            if (value is int) { sb.Append(((int)value).ToString(CultureInfo.InvariantCulture)); return; }
            if (value is long) { sb.Append(((long)value).ToString(CultureInfo.InvariantCulture)); return; }
            if (value is double)
            {
                double number = (double)value;
                if (double.IsNaN(number) || double.IsInfinity(number)) throw new FormatException("D21 JSON number is not finite.");
                sb.Append(number.ToString("R", CultureInfo.InvariantCulture));
                return;
            }
            IDictionary dictionary = value as IDictionary;
            if (dictionary != null)
            {
                var keys = new List<string>();
                foreach (DictionaryEntry entry in dictionary) keys.Add(Convert.ToString(entry.Key, CultureInfo.InvariantCulture));
                keys.Sort(StringComparer.Ordinal);
                sb.Append('{');
                for (int i = 0; i < keys.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteString(sb, keys[i]); sb.Append(':');
                    WriteValue(sb, dictionary[keys[i]]);
                }
                sb.Append('}');
                return;
            }
            IList list = value as IList;
            if (list != null)
            {
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteValue(sb, list[i]);
                }
                sb.Append(']');
                return;
            }
            throw new FormatException("D21 JSON value type is unsupported.");
        }

        private static object Required(Dictionary<string, object> root, string key)
        {
            object value;
            if (root == null || !root.TryGetValue(key, out value))
                throw new FormatException("D21 JSON field '" + key + "' is required.");
            return value;
        }

        private sealed class Parser
        {
            private readonly string _json;
            private int _index;

            public Parser(string json) { _json = json; }

            public object Parse()
            {
                SkipWhitespace();
                object value = ParseValue();
                SkipWhitespace();
                if (_index != _json.Length) throw new FormatException("D21 JSON has trailing content.");
                return value;
            }

            private object ParseValue()
            {
                SkipWhitespace();
                if (_index >= _json.Length) throw new FormatException("D21 JSON ended unexpectedly.");
                char c = _json[_index];
                if (c == '{') return ParseObject();
                if (c == '[') return ParseArray();
                if (c == '"') return ParseString();
                if (c == 't') { Expect("true"); return true; }
                if (c == 'f') { Expect("false"); return false; }
                if (c == 'n') { Expect("null"); return null; }
                return ParseNumber();
            }

            private Dictionary<string, object> ParseObject()
            {
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                _index++;
                SkipWhitespace();
                if (Take('}')) return result;
                while (true)
                {
                    SkipWhitespace();
                    if (_index >= _json.Length || _json[_index] != '"') throw new FormatException("D21 JSON object key is invalid.");
                    string key = ParseString();
                    SkipWhitespace();
                    if (!Take(':')) throw new FormatException("D21 JSON object colon is missing.");
                    result[key] = ParseValue();
                    SkipWhitespace();
                    if (Take('}')) return result;
                    if (!Take(',')) throw new FormatException("D21 JSON object separator is invalid.");
                }
            }

            private List<object> ParseArray()
            {
                var result = new List<object>();
                _index++;
                SkipWhitespace();
                if (Take(']')) return result;
                while (true)
                {
                    result.Add(ParseValue());
                    SkipWhitespace();
                    if (Take(']')) return result;
                    if (!Take(',')) throw new FormatException("D21 JSON array separator is invalid.");
                }
            }

            private string ParseString()
            {
                if (!Take('"')) throw new FormatException("D21 JSON string is invalid.");
                var sb = new StringBuilder();
                while (_index < _json.Length)
                {
                    char c = _json[_index++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (_index >= _json.Length) throw new FormatException("D21 JSON string escape is incomplete.");
                    char e = _json[_index++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_index + 4 > _json.Length) throw new FormatException("D21 JSON unicode escape is incomplete.");
                            string hex = _json.Substring(_index, 4);
                            _index += 4;
                            sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            break;
                        default: throw new FormatException("D21 JSON string escape is unknown.");
                    }
                }
                throw new FormatException("D21 JSON string is not closed.");
            }

            private object ParseNumber()
            {
                int start = _index;
                while (_index < _json.Length && "-+0123456789.eE".IndexOf(_json[_index]) >= 0) _index++;
                string value = _json.Substring(start, _index - start);
                if (value.Length == 0) throw new FormatException("D21 JSON number is invalid.");
                if (value.IndexOf('.') >= 0 || value.IndexOf('e') >= 0 || value.IndexOf('E') >= 0)
                    return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                return long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            private void SkipWhitespace()
            {
                while (_index < _json.Length && char.IsWhiteSpace(_json[_index])) _index++;
            }

            private void Expect(string text)
            {
                if (_index + text.Length > _json.Length || !string.Equals(_json.Substring(_index, text.Length), text, StringComparison.Ordinal))
                    throw new FormatException("D21 JSON literal is invalid.");
                _index += text.Length;
            }

            private bool Take(char expected)
            {
                if (_index < _json.Length && _json[_index] == expected)
                {
                    _index++;
                    return true;
                }
                return false;
            }
        }
    }
}
