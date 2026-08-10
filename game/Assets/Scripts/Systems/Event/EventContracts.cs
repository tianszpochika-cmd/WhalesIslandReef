// ============================================================
// EventContracts —— D8 统一 WorldEvent / FactEvent DTO
//
// 事件层只记录已由所属领域系统裁决的事实。它不计算或写入金钱、价格、
// 伤害、奖励、天气、关系等领域数值；这些数值仍由各自 canonical owner 写入。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Whalebone.Systems.Events
{
    public static class EventSchema
    {
        public const int CurrentVersion = 1;
        public const int EventLogVersion = 1;
    }

    /// <summary>事件生命周期；Duplicate 是提交结果，不是事实状态。</summary>
    public enum WorldEventStatus
    {
        Normal,
        Failed,
        Cancelled,
        Interrupted,
        Recovered,
    }

    public enum EventAppendDisposition
    {
        Appended,
        Duplicate,
    }

    public enum EventSettlementDisposition
    {
        Claimed,
        AlreadyClaimed,
    }

    [Serializable]
    public sealed class EventResult
    {
        /// <summary>由 owner_system 定义的结果码；事件层不解释其业务含义。</summary>
        public string ResultCode = "completed";

        /// <summary>给 UI/Agent 的事实描述，不是裁决逻辑。</summary>
        public string Summary = string.Empty;

        /// <summary>可追踪的事实/实体引用。</summary>
        public List<string> FactIds = new List<string>();

        /// <summary>owner_system 提供的只读结果载荷；事件层按 opaque 文本保存。</summary>
        public Dictionary<string, string> Values = new Dictionary<string, string>();

        public string result_code { get { return ResultCode; } set { ResultCode = value; } }
        public string summary { get { return Summary; } set { Summary = value; } }
        public List<string> fact_ids { get { return FactIds; } set { FactIds = value; } }
        public Dictionary<string, string> values { get { return Values; } set { Values = value; } }

        public EventResult Clone()
        {
            var result = new EventResult
            {
                ResultCode = ResultCode,
                Summary = Summary,
            };
            if (FactIds != null) result.FactIds.AddRange(FactIds);
            if (Values != null)
                foreach (var pair in Values) result.Values[pair.Key] = pair.Value;
            return result;
        }
    }

    [Serializable]
    public sealed class RecoveryRoute
    {
        public string RouteId;
        public string Action;
        public string ResumeFromEventId;
        public string Description;

        public string route_id { get { return RouteId; } set { RouteId = value; } }
        public string action { get { return Action; } set { Action = value; } }
        public string resume_from_event_id { get { return ResumeFromEventId; } set { ResumeFromEventId = value; } }
        public string description { get { return Description; } set { Description = value; } }

        public RecoveryRoute Clone()
        {
            return new RecoveryRoute
            {
                RouteId = RouteId,
                Action = Action,
                ResumeFromEventId = ResumeFromEventId,
                Description = Description,
            };
        }

        internal void Validate(string eventId)
        {
            if (string.IsNullOrWhiteSpace(RouteId))
                throw new EventValidationException("recovery_route.route_id is required for event " + eventId + ".");
            if (string.IsNullOrWhiteSpace(Action))
                throw new EventValidationException("recovery_route.action is required for event " + eventId + ".");
        }
    }

    /// <summary>
    /// 尚未写入日志的领域事件包。WorldEvent 只携带输入、owner 结果和因果引用，
    /// 不负责执行任何领域状态变更。
    /// </summary>
    [Serializable]
    public class WorldEvent
    {
        public string EventId;
        public int SchemaVersion = EventSchema.CurrentVersion;
        public int WorldDay;
        public string LocationRef;
        public List<string> ActorIds = new List<string>();
        public List<string> InputFacts = new List<string>();
        public string OwnerSystem;
        public EventResult Result = new EventResult();
        public List<string> CauseIds = new List<string>();
        public long SaveRevision;
        public WorldEventStatus OutcomeStatus = WorldEventStatus.Normal;
        public RecoveryRoute RecoveryRoute;

        // snake_case aliases keep the C# DTO addressable by the contract field names.
        public string event_id { get { return EventId; } set { EventId = value; } }
        public int schema_version { get { return SchemaVersion; } set { SchemaVersion = value; } }
        public int world_day { get { return WorldDay; } set { WorldDay = value; } }
        public string location_ref { get { return LocationRef; } set { LocationRef = value; } }
        public List<string> actor_ids { get { return ActorIds; } set { ActorIds = value; } }
        public List<string> input_facts { get { return InputFacts; } set { InputFacts = value; } }
        public string owner_system { get { return OwnerSystem; } set { OwnerSystem = value; } }
        public EventResult result { get { return Result; } set { Result = value; } }
        public List<string> cause_ids { get { return CauseIds; } set { CauseIds = value; } }
        public long save_revision { get { return SaveRevision; } set { SaveRevision = value; } }
        public WorldEventStatus outcome_status { get { return OutcomeStatus; } set { OutcomeStatus = value; } }
        public RecoveryRoute recovery_route { get { return RecoveryRoute; } set { RecoveryRoute = value; } }

        public WorldEvent() { }

        public WorldEvent(string eventId, int worldDay, string locationRef, string ownerSystem,
            EventResult result = null, IEnumerable<string> actorIds = null,
            IEnumerable<string> inputFacts = null, IEnumerable<string> causeIds = null,
            long saveRevision = 0, WorldEventStatus outcomeStatus = WorldEventStatus.Normal,
            RecoveryRoute recoveryRoute = null)
        {
            EventId = eventId;
            WorldDay = worldDay;
            LocationRef = locationRef;
            OwnerSystem = ownerSystem;
            Result = result == null ? new EventResult() : result.Clone();
            if (actorIds != null) ActorIds.AddRange(actorIds);
            if (inputFacts != null) InputFacts.AddRange(inputFacts);
            if (causeIds != null) CauseIds.AddRange(causeIds);
            SaveRevision = saveRevision;
            OutcomeStatus = outcomeStatus;
            RecoveryRoute = recoveryRoute == null ? null : recoveryRoute.Clone();
        }

        protected WorldEvent(WorldEvent source)
        {
            EventId = source.EventId;
            SchemaVersion = source.SchemaVersion;
            WorldDay = source.WorldDay;
            LocationRef = source.LocationRef;
            OwnerSystem = source.OwnerSystem;
            SaveRevision = source.SaveRevision;
            OutcomeStatus = source.OutcomeStatus;
            Result = source.Result == null ? null : source.Result.Clone();
            RecoveryRoute = source.RecoveryRoute == null ? null : source.RecoveryRoute.Clone();
            if (source.ActorIds != null) ActorIds.AddRange(source.ActorIds);
            if (source.InputFacts != null) InputFacts.AddRange(source.InputFacts);
            if (source.CauseIds != null) CauseIds.AddRange(source.CauseIds);
        }

        public static WorldEvent CreateRecovery(WorldEvent interrupted, string recoveryEventId,
            int worldDay, long saveRevision, string locationRef, string ownerSystem,
            EventResult result = null, IEnumerable<string> actorIds = null)
        {
            if (interrupted == null)
                throw new EventValidationException("An interrupted event is required to create a recovery event.");
            if (interrupted.OutcomeStatus != WorldEventStatus.Interrupted)
                throw new EventValidationException("Only an interrupted event can be recovered: " + interrupted.EventId + ".");

            return new WorldEvent(
                recoveryEventId,
                worldDay,
                locationRef,
                ownerSystem,
                result,
                actorIds,
                new[] { "recovery_of:" + interrupted.EventId },
                new[] { interrupted.EventId },
                saveRevision,
                WorldEventStatus.Recovered);
        }

        /// <summary>事件日志的唯一去重键：event_id + save_revision。</summary>
        public string DedupeKey
        {
            get
            {
                return (EventId ?? string.Empty) + "\u001f" +
                    SaveRevision.ToString(CultureInfo.InvariantCulture);
            }
        }

        public virtual WorldEvent Clone() { return new WorldEvent(this); }

        internal void Validate(int expectedSchemaVersion)
        {
            if (SchemaVersion != expectedSchemaVersion)
                throw new EventSchemaVersionException(
                    "WorldEvent schema_version mismatch: expected " + expectedSchemaVersion +
                    ", received " + SchemaVersion + ".");
            if (string.IsNullOrWhiteSpace(EventId))
                throw new EventValidationException("event_id is required.");
            if (WorldDay < 0)
                throw new EventValidationException("world_day must be >= 0 for event " + EventId + ".");
            if (string.IsNullOrWhiteSpace(LocationRef))
                throw new EventValidationException("location_ref is required for event " + EventId + ".");
            if (ActorIds == null)
                throw new EventValidationException("actor_ids must be present for event " + EventId + ".");
            if (InputFacts == null)
                throw new EventValidationException("input_facts must be present for event " + EventId + ".");
            ValidateIdList(ActorIds, "actor_ids");
            ValidateIdList(InputFacts, "input_facts");
            if (string.IsNullOrWhiteSpace(OwnerSystem))
                throw new EventValidationException("owner_system is required for event " + EventId + ".");
            if (Result == null)
                throw new EventValidationException("result must be present for event " + EventId + ".");
            if (string.IsNullOrWhiteSpace(Result.ResultCode))
                throw new EventValidationException("result.result_code is required for event " + EventId + ".");
            if (Result.FactIds == null || Result.Values == null)
                throw new EventValidationException("result.fact_ids and result.values must be present for event " + EventId + ".");
            ValidateIdList(Result.FactIds, "result.fact_ids");
            if (CauseIds == null)
                throw new EventValidationException("cause_ids must be present for event " + EventId + ".");
            ValidateIdList(CauseIds, "cause_ids");
            if (SaveRevision < 0)
                throw new EventValidationException("save_revision must be >= 0 for event " + EventId + ".");
            if (!Enum.IsDefined(typeof(WorldEventStatus), OutcomeStatus))
                throw new EventValidationException("outcome_status is unknown for event " + EventId + ".");

            if (OutcomeStatus == WorldEventStatus.Failed || OutcomeStatus == WorldEventStatus.Interrupted)
            {
                if (RecoveryRoute == null) throw new EventValidationException(
                    "recovery_route is required for " + OutcomeStatus + " event " + EventId + ".");
                RecoveryRoute.Validate(EventId);
            }

            if (OutcomeStatus == WorldEventStatus.Recovered && CauseIds.Count == 0)
                throw new EventValidationException("A recovered event must reference its interrupted cause.");
        }

        private static void ValidateIdList(List<string> values, string fieldName)
        {
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value))
                    throw new EventValidationException(fieldName + " cannot contain an empty id.");
        }
    }

    /// <summary>已进入事实日志的事件；读取者只能取得副本。</summary>
    [Serializable]
    public sealed class FactEvent : WorldEvent
    {
        public long LogSequence;

        internal FactEvent(WorldEvent source, long logSequence) : base(source)
        {
            LogSequence = logSequence;
        }

        public FactEvent() { }

        public override WorldEvent Clone() { return new FactEvent(this, LogSequence); }

        public FactEvent CloneFact() { return new FactEvent(this, LogSequence); }
    }

    [Serializable]
    public sealed class EventLogSnapshot
    {
        public int SchemaVersion = EventSchema.EventLogVersion;
        public long NextSequence;
        public List<FactEvent> Facts = new List<FactEvent>();
        public List<string> SettledKeys = new List<string>();

        public EventLogSnapshot Clone()
        {
            var result = new EventLogSnapshot { SchemaVersion = SchemaVersion, NextSequence = NextSequence };
            if (Facts != null)
                foreach (var fact in Facts) if (fact != null) result.Facts.Add(fact.CloneFact());
            if (SettledKeys != null) result.SettledKeys.AddRange(SettledKeys);
            return result;
        }
    }

    public sealed class EventAppendResult
    {
        public EventAppendDisposition Disposition;
        public FactEvent Fact;
        public bool IsDuplicate { get { return Disposition == EventAppendDisposition.Duplicate; } }
        public bool Succeeded { get { return Fact != null; } }

        internal static EventAppendResult Appended(FactEvent fact)
        {
            return new EventAppendResult { Disposition = EventAppendDisposition.Appended, Fact = fact };
        }

        internal static EventAppendResult Duplicate(FactEvent fact)
        {
            return new EventAppendResult { Disposition = EventAppendDisposition.Duplicate, Fact = fact };
        }
    }

    public sealed class EventSettlementResult
    {
        public EventSettlementDisposition Disposition;
        public FactEvent Fact;
        public bool IsDuplicate { get { return Disposition == EventSettlementDisposition.AlreadyClaimed; } }
        public bool CanApplyOwnerSettlement { get { return !IsDuplicate; } }

        internal static EventSettlementResult Claimed(FactEvent fact)
        {
            return new EventSettlementResult { Disposition = EventSettlementDisposition.Claimed, Fact = fact };
        }

        internal static EventSettlementResult AlreadyClaimed(FactEvent fact)
        {
            return new EventSettlementResult { Disposition = EventSettlementDisposition.AlreadyClaimed, Fact = fact };
        }
    }

    [Serializable]
    public class EventValidationException : Exception
    {
        public EventValidationException(string message) : base(message) { }
    }

    [Serializable]
    public sealed class EventSchemaVersionException : EventValidationException
    {
        public EventSchemaVersionException(string message) : base(message) { }
    }
}
