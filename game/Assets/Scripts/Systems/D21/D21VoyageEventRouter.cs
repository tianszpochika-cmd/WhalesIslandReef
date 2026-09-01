// ============================================================
// D21VoyageEventRouter —— D21-L26 航程事件顺序与投影路由
//
// 本路由器只追加已经由 D21 owner 裁决的事件。它不计算航速、补给、天气、
// 船况或港口状态；职责是批次预检、事件顺序、event_id + save_revision
// 幂等、失败补偿载荷和 Save projection 游标一致性。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>航程事件路由结果；事件和投影均为防御性副本。</summary>
    [Serializable]
    public sealed class D21VoyageEventRouteResult
    {
        public bool Accepted;
        public bool IsDuplicate;
        public string Code = string.Empty;
        public string VoyageId = string.Empty;
        public string ShipId = string.Empty;
        public string RouteId = string.Empty;
        public D21VoyageEvent Event;
        public D21ShipSaveProjection SaveProjection;
        public D21VoyageCompensationDraft Compensation;
        public ReadOnlyCollection<D21VoyageEvent> AppendedEvents =
            new ReadOnlyCollection<D21VoyageEvent>(new List<D21VoyageEvent>());

        public D21VoyageEventRouteResult Clone()
        {
            var events = new List<D21VoyageEvent>();
            if (AppendedEvents != null)
            {
                foreach (D21VoyageEvent item in AppendedEvents)
                    if (item != null) events.Add(item.Clone());
            }
            return new D21VoyageEventRouteResult
            {
                Accepted = Accepted,
                IsDuplicate = IsDuplicate,
                Code = Code ?? string.Empty,
                VoyageId = VoyageId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                RouteId = RouteId ?? string.Empty,
                Event = Event == null ? null : Event.Clone(),
                SaveProjection = SaveProjection == null ? null : SaveProjection.Clone(),
                Compensation = Compensation == null ? null : Compensation.Clone(),
                AppendedEvents = new ReadOnlyCollection<D21VoyageEvent>(events),
            };
        }
    }

    /// <summary>
    /// 跨 owner 提交失败时的可审计补偿计划。它只描述后续恢复动作，不能直接
    /// 回写 D19、D20、D3、D9 或其他 owner 的事实。
    /// </summary>
    [Serializable]
    public sealed class D21VoyageCompensationDraft
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string CauseEventId = string.Empty;
        public string RecoveryRouteId = string.Empty;
        public string ReasonCode = string.Empty;
        public List<string> Steps = new List<string>();

        public D21VoyageCompensationDraft Clone()
        {
            var result = new D21VoyageCompensationDraft
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                CauseEventId = CauseEventId ?? string.Empty,
                RecoveryRouteId = RecoveryRouteId ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
            };
            if (Steps != null) result.Steps.AddRange(Steps);
            return result;
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion || string.IsNullOrWhiteSpace(ReasonCode) ||
                string.IsNullOrWhiteSpace(RecoveryRouteId) || Steps == null)
                throw new InvalidOperationException("D21 compensation draft is incomplete.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string step in Steps)
            {
                if (string.IsNullOrWhiteSpace(step) || !seen.Add(step))
                    throw new InvalidOperationException("D21 compensation steps must be unique.");
            }
        }
    }

    /// <summary>
    /// D21 航程事件唯一提交入口。路由器按 voyage_id 隔离事件日志，所有批次先
    /// 在临时日志中验证，成功后才一次性替换 owner 日志和 Save projection。
    /// </summary>
    public sealed class D21VoyageEventRouter
    {
        // The router keeps the complete append-only history. A hot projection
        // switches to a one-event cursor after this threshold; explicit reads
        // still materialize the complete history from the owner log.
        private const int FullProjectionEventThreshold = 256;

        public const string EventRequiredCode = "d21.event.event_required";
        public const string BatchRequiredCode = "d21.event.batch_required";
        public const string EventInvalidCode = "d21.event.invalid";
        public const string MixedVoyageCode = "d21.event.mixed_voyage";
        public const string ProjectionRequiredCode = "d21.event.projection_required";
        public const string ProjectionCursorConflictCode = "d21.event.projection_cursor_conflict";
        public const string FirstEventMustStartCode = "d21.event.first_event_must_start";
        public const string OrderConflictCode = "d21.event.order_conflict";
        public const string EventIdConflictCode = "d21.event.event_id_conflict";
        public const string RecoveryCauseRequiredCode = "d21.event.recovery_cause_required";
        public const string RecoveryStateRequiredCode = "d21.event.recovery_state_required";
        public const string ProjectionInvalidCode = "d21.event.projection_invalid";
        public const string AppendFailedCode = "d21.event.append_failed";

        private readonly Dictionary<string, D21VoyageEventLog> _logs =
            new Dictionary<string, D21VoyageEventLog>(StringComparer.Ordinal);
        private readonly Dictionary<string, Dictionary<string, D21VoyageEvent>> _eventIndexes =
            new Dictionary<string, Dictionary<string, D21VoyageEvent>>(StringComparer.Ordinal);
        private readonly Dictionary<string, D21ShipSaveProjection> _projections =
            new Dictionary<string, D21ShipSaveProjection>(StringComparer.Ordinal);

        /// <summary>返回事件日志的防御性保存快照。</summary>
        public D21VoyageEventLogSnapshot GetLogSnapshot(string voyageId)
        {
            if (string.IsNullOrEmpty(voyageId)) return null;
            D21VoyageEventLog log;
            return _logs.TryGetValue(voyageId, out log) ? log.BuildSnapshot() : null;
        }

        /// <summary>返回路由器当前保存投影的防御性副本。</summary>
        public D21ShipSaveProjection GetProjection(string voyageId)
        {
            if (string.IsNullOrEmpty(voyageId)) return null;
            D21ShipSaveProjection projection;
            if (!_projections.TryGetValue(voyageId, out projection)) return null;
            D21VoyageEventLog log;
            if (!_logs.TryGetValue(voyageId, out log)) return projection.Clone();
            D21ShipSaveProjection result = CloneProjectionForRouting(projection);
            result.Events.EventLog = log.BuildSnapshot();
            result.Events.AppliedEventKeys = new List<string>();
            foreach (D21VoyageEvent item in result.Events.EventLog.Events)
                result.Events.AppliedEventKeys.Add(item.DedupeKey);
            if (result.Events.EventLog.Events.Count == 0)
            {
                result.Events.LastEventId = string.Empty;
                result.Events.LastEventSequence = 0;
                result.Events.LastEventSaveRevision = 0;
            }
            else
            {
                D21VoyageEvent last = result.Events.EventLog.Events[
                    result.Events.EventLog.Events.Count - 1];
                result.Events.LastEventId = last.EventId;
                result.Events.LastEventSequence = last.LogSequence;
                result.Events.LastEventSaveRevision = last.SaveRevision;
            }
            result.Validate();
            return result;
        }

        /// <summary>只做预检，不修改事件日志和投影。</summary>
        public bool CanAppend(IEnumerable<D21VoyageEvent> events, D21ShipSaveProjection projection)
        {
            D21VoyageEventLog ignoredLog;
            List<D21VoyageEvent> ignoredEvents;
            string code;
            return TryPrepare(events, projection, out ignoredLog, out ignoredEvents, out code);
        }

        /// <summary>追加单个事件的便捷入口。</summary>
        public D21VoyageEventRouteResult Append(
            D21VoyageEvent item,
            D21ShipSaveProjection projection)
        {
            if (item == null)
            {
                return Rejected(item, EventRequiredCode, projection);
            }
            return AppendMany(new[] { item }, projection);
        }

        /// <summary>
        /// 对事件批次执行原子追加。任何一项失败都不会提交该批次的部分事件。
        /// </summary>
        public D21VoyageEventRouteResult AppendMany(
            IEnumerable<D21VoyageEvent> events,
            D21ShipSaveProjection projection)
        {
            D21VoyageEventLog workingLog;
            List<D21VoyageEvent> batch;
            string code;
            if (!TryPrepare(events, projection, out workingLog, out batch, out code))
            {
                D21VoyageEvent first = First(events);
                return Rejected(first, code, projection);
            }

            D21VoyageEvent firstItem = batch[0];
            string voyageId = firstItem.VoyageId ?? string.Empty;
            string shipId = firstItem.ShipId ?? string.Empty;
            string routeId = firstItem.RouteId ?? string.Empty;
            D21VoyageEventLog committedLog = workingLog;
            Dictionary<string, D21VoyageEvent> eventIndex = GetOrBuildEventIndex(voyageId, committedLog);
            int pendingAppendCount = 0;
            foreach (D21VoyageEvent item in batch)
            {
                if (!eventIndex.ContainsKey(item.DedupeKey)) pendingAppendCount++;
            }
            if (pendingAppendCount > 0 &&
                committedLog.NextSequence > long.MaxValue - pendingAppendCount)
            {
                return Rejected(firstItem, AppendFailedCode + ":sequence_overflow", projection);
            }
            var appended = new List<D21VoyageEvent>();
            bool allDuplicate = true;
            foreach (D21VoyageEvent item in batch)
            {
                D21VoyageEvent existing;
                eventIndex.TryGetValue(item.DedupeKey, out existing);
                if (existing != null)
                {
                    if (!EventsEquivalent(existing, item))
                    {
                        return Rejected(item, EventIdConflictCode, projection);
                    }
                    continue;
                }
                allDuplicate = false;
                D21VoyageEventAppendResult appendResult = committedLog.Append(item);
                if (appendResult == null || !appendResult.Succeeded || appendResult.Event == null)
                {
                    return Rejected(item, AppendFailedCode + ":" +
                        (appendResult == null ? "missing" : appendResult.Code), projection);
                }
                eventIndex[item.DedupeKey] = appendResult.Event.Clone();
                appended.Add(appendResult.Event.Clone());
            }

            D21ShipSaveProjection committedProjection = UpdateEventCursor(
                projection, committedLog, firstItem.ShipId, firstItem.RouteId);

            _logs[voyageId] = committedLog;
            _eventIndexes[voyageId] = eventIndex;
            _projections[voyageId] = committedProjection.Clone();

            D21VoyageEvent resultEvent = batch[batch.Count - 1];
            D21VoyageEvent routedEvent;
            eventIndex.TryGetValue(resultEvent.DedupeKey, out routedEvent);
            var result = new D21VoyageEventRouteResult
            {
                Accepted = true,
                IsDuplicate = allDuplicate,
                Code = allDuplicate ? "d21.event.duplicate" : "d21.event.appended",
                VoyageId = voyageId,
                ShipId = shipId,
                RouteId = routeId,
                Event = routedEvent == null ? resultEvent.Clone() : routedEvent.Clone(),
                SaveProjection = committedProjection.Clone(),
                AppendedEvents = new ReadOnlyCollection<D21VoyageEvent>(appended),
            };
            result.Compensation = null;
            return result;
        }

        /// <summary>
        /// 在事件追加后刷新航程/锚泊/天气/补给/风险字段，并从路由器日志重建
        /// 事件游标。该方法不追加事件，也不改变任何上游 owner 事实。
        /// </summary>
        public D21ShipSaveProjection ReconcileProjection(
            D21ShipSaveProjection baseProjection,
            string voyageId,
            string shipId,
            string routeId,
            D21VoyageStatus status,
            int lastTickIndex,
            long saveRevision,
            D21VoyageTimeState time,
            D21CourseState course,
            IEnumerable<D21VoyageRiskSeed> riskSeeds,
            D21AnchorMooringState anchor,
            D21WeatherInput weather,
            D21ProvisionLedgerSnapshot ledger,
            string lastEventId)
        {
            if (!D21ShipIdentityContracts.IsStableId(voyageId, 128) ||
                !D21ShipIdentityContracts.IsStableId(shipId, 128) ||
                !D21ShipIdentityContracts.IsStableId(routeId, 128) || saveRevision < 0)
                return null;

            D21VoyageEventLog log;
            if (!_logs.TryGetValue(voyageId, out log))
            {
                log = new D21VoyageEventLog();
                D21ShipSaveProjection supplied = baseProjection;
                if (supplied != null && supplied.Events != null && supplied.Events.EventLog != null &&
                    supplied.Events.EventLog.Events != null && supplied.Events.EventLog.Events.Count > 0)
                {
                    try
                    {
                        log = D21VoyageEventLog.Restore(supplied.Events.EventLog);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }
            }

            D21ShipSaveProjection result = baseProjection == null
                ? D21ShipSaveProjection.CreateDefault(shipId)
                : CloneProjectionForRouting(baseProjection);
            try
            {
                ApplyAggregateProjection(result, voyageId, shipId, routeId, status,
                    lastTickIndex, saveRevision, time, course, riskSeeds, anchor,
                    weather, ledger, lastEventId, log);
                result.Validate();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            _logs[voyageId] = log;
            _eventIndexes[voyageId] = GetOrBuildEventIndex(voyageId, log);
            _projections[voyageId] = result.Clone();
            return result;
        }

        private bool TryPrepare(
            IEnumerable<D21VoyageEvent> source,
            D21ShipSaveProjection projection,
            out D21VoyageEventLog workingLog,
            out List<D21VoyageEvent> batch,
            out string code)
        {
            workingLog = null;
            batch = new List<D21VoyageEvent>();
            code = string.Empty;
            if (source == null)
            {
                code = BatchRequiredCode;
                return false;
            }
            foreach (D21VoyageEvent item in source)
            {
                if (item == null)
                {
                    code = EventInvalidCode;
                    return false;
                }
                D21VoyageEvent copy = item.Clone();
                try
                {
                    copy.Validate();
                }
                catch (InvalidOperationException)
                {
                    code = EventInvalidCode;
                    return false;
                }
                batch.Add(copy);
            }
            if (batch.Count == 0)
            {
                code = BatchRequiredCode;
                return false;
            }
            if (projection == null)
            {
                code = ProjectionRequiredCode;
                return false;
            }
            try
            {
                projection.Validate();
            }
            catch (InvalidOperationException)
            {
                code = ProjectionInvalidCode;
                return false;
            }

            D21VoyageEvent first = batch[0];
            foreach (D21VoyageEvent item in batch)
            {
                if (!string.Equals(item.VoyageId, first.VoyageId, StringComparison.Ordinal) ||
                    !string.Equals(item.ShipId, first.ShipId, StringComparison.Ordinal) ||
                    !string.Equals(item.RouteId, first.RouteId, StringComparison.Ordinal))
                {
                    code = MixedVoyageCode;
                    return false;
                }
            }

            if (!TryCloneOrRestoreLog(first.VoyageId, projection, out workingLog, out code))
                return false;
            if (!ProjectionMatchesLog(projection, workingLog))
            {
                code = ProjectionCursorConflictCode;
                return false;
            }

            Dictionary<string, D21VoyageEvent> eventIndex =
                GetOrBuildEventIndex(first.VoyageId, workingLog);
            var batchKeys = new HashSet<string>(StringComparer.Ordinal);
            D21VoyageEvent previous = LastEvent(workingLog);
            foreach (D21VoyageEvent item in batch)
            {
                if (!batchKeys.Add(item.DedupeKey))
                {
                    code = EventIdConflictCode;
                    return false;
                }

                D21VoyageEvent existing;
                eventIndex.TryGetValue(item.DedupeKey, out existing);
                if (existing != null)
                {
                    if (!EventsEquivalent(existing, item))
                    {
                        code = EventIdConflictCode;
                        return false;
                    }
                    continue;
                }

                if (previous == null)
                {
                    if (item.EventType != D21VoyageEventType.VoyageStarted || item.TickIndex != 0)
                    {
                        code = FirstEventMustStartCode;
                        return false;
                    }
                }
                else
                {
                    if (!IsOrderedAfter(previous, item))
                    {
                        code = OrderConflictCode;
                        return false;
                    }
                    if (item.EventType == D21VoyageEventType.VoyageStarted)
                    {
                        code = OrderConflictCode;
                        return false;
                    }
                    if (item.EventType == D21VoyageEventType.Recovered &&
                        !HasRecoverableCause(workingLog, item) &&
                        !HasArrivedReturnCause(workingLog, item))
                    {
                        code = RecoveryStateRequiredCode;
                        return false;
                    }
                }

                // D21VoyageEventLog.Append is intentionally deferred until every
                // event in the batch has passed validation, preserving atomicity
                // without cloning the complete historical log on every tick.
                previous = item;
            }
            return true;
        }

        private bool TryCloneOrRestoreLog(
            string voyageId,
            D21ShipSaveProjection projection,
            out D21VoyageEventLog result,
            out string code)
        {
            code = string.Empty;
            D21VoyageEventLog current;
            if (_logs.TryGetValue(voyageId, out current))
            {
                // The batch is fully validated before AppendMany mutates this owner
                // log; sharing it here avoids rebuilding the complete history every
                // fixed tick.
                result = current;
                return true;
            }
            if (projection.Events != null && projection.Events.EventLog != null &&
                projection.Events.EventLog.Events != null && projection.Events.EventLog.Events.Count > 0)
            {
                try
                {
                    result = D21VoyageEventLog.Restore(projection.Events.EventLog);
                    _eventIndexes[voyageId] = BuildEventIndex(result);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    result = null;
                    code = ProjectionInvalidCode;
                    return false;
                }
            }
            result = new D21VoyageEventLog();
            _eventIndexes[voyageId] = new Dictionary<string, D21VoyageEvent>(StringComparer.Ordinal);
            return true;
        }

        private static bool ProjectionMatchesLog(
            D21ShipSaveProjection projection,
            D21VoyageEventLog log)
        {
            if (projection == null || projection.Events == null || projection.Events.EventLog == null || log == null)
                return false;
            D21VoyageEventLogSnapshot expected = projection.Events.EventLog;
            if (expected.Events == null || expected.NextSequence != log.NextSequence ||
                (expected.Events.Count != log.Events.Count && expected.Events.Count != 1) ||
                (expected.Events.Count == 0 && log.Events.Count != 0) ||
                (expected.Events.Count == 1 && log.Events.Count == 0))
                return false;
            if (expected.Events.Count == 0)
            {
                return projection.Events.LastEventSequence == 0 &&
                    string.IsNullOrEmpty(projection.Events.LastEventId);
            }
            D21VoyageEvent expectedLast = expected.Events[expected.Events.Count - 1];
            D21VoyageEvent actualLast = log.Events[log.Events.Count - 1];
            if (expectedLast == null || actualLast == null ||
                !string.Equals(expectedLast.DedupeKey, actualLast.DedupeKey, StringComparison.Ordinal) ||
                expectedLast.LogSequence != actualLast.LogSequence)
                return false;
            return projection.Events.LastEventSequence == expectedLast.LogSequence &&
                string.Equals(projection.Events.LastEventId, expectedLast.EventId, StringComparison.Ordinal) &&
                projection.Events.LastEventSaveRevision == expectedLast.SaveRevision;
        }

        private static D21ShipSaveProjection UpdateEventCursor(
            D21ShipSaveProjection baseProjection,
            D21VoyageEventLog log,
            string shipId,
            string routeId)
        {
            D21ShipSaveProjection result = CloneProjectionForRouting(baseProjection);
            if (result.Events == null) result.Events = new D21EventCursorSaveProjection();
            D21VoyageEventLogSnapshot snapshot = BuildCursorSnapshot(log);
            result.Events.EventLog = snapshot;
            result.Events.AppliedEventKeys = new List<string>();
            foreach (D21VoyageEvent item in snapshot.Events)
                result.Events.AppliedEventKeys.Add(item.DedupeKey);
            if (snapshot.Events.Count == 0)
            {
                result.Events.LastEventId = string.Empty;
                result.Events.LastEventSequence = 0;
                result.Events.LastEventSaveRevision = 0;
            }
            else
            {
                D21VoyageEvent last = snapshot.Events[snapshot.Events.Count - 1];
                result.Events.LastEventId = last.EventId;
                result.Events.LastEventSequence = last.LogSequence;
                result.Events.LastEventSaveRevision = last.SaveRevision;
                result.WorldDay = last.WorldDay;
                result.SaveRevision = last.SaveRevision;
            }
            if (result.ShipSnapshot == null)
            {
                result.ShipSnapshot = new D21OwnerSnapshotReference
                {
                    OwnerSystem = "D21ShipSystem",
                    SnapshotId = "ship:" + (shipId ?? string.Empty),
                    SnapshotSchemaVersion = 1,
                    Revision = result.SaveRevision,
                };
            }
            if (result.Voyage == null) result.Voyage = new D21VoyageSaveProjection();
            if (string.IsNullOrEmpty(result.Voyage.ShipId)) result.Voyage.ShipId = shipId ?? string.Empty;
            if (string.IsNullOrEmpty(result.Voyage.RouteId)) result.Voyage.RouteId = routeId ?? string.Empty;
            result.Validate();
            return result;
        }

        private static void ApplyAggregateProjection(
            D21ShipSaveProjection result,
            string voyageId,
            string shipId,
            string routeId,
            D21VoyageStatus status,
            int lastTickIndex,
            long saveRevision,
            D21VoyageTimeState time,
            D21CourseState course,
            IEnumerable<D21VoyageRiskSeed> riskSeeds,
            D21AnchorMooringState anchor,
            D21WeatherInput weather,
            D21ProvisionLedgerSnapshot ledger,
            string lastEventId,
            D21VoyageEventLog log)
        {
            if (result.ShipSnapshot == null)
            {
                result.ShipSnapshot = new D21OwnerSnapshotReference
                {
                    OwnerSystem = "D21ShipSystem",
                    SnapshotId = "ship:" + shipId,
                    SnapshotSchemaVersion = 1,
                    Revision = saveRevision,
                };
            }
            if (result.Anchor == null) result.Anchor = new D21AnchorSaveProjection();
            if (result.Voyage == null) result.Voyage = new D21VoyageSaveProjection();
            if (result.WeatherSeed == null) result.WeatherSeed = new D21WeatherSeedSaveProjection();
            if (result.Provisions == null) result.Provisions = new D21ProvisionLedgerSaveProjection();
            if (result.Events == null) result.Events = new D21EventCursorSaveProjection();

            result.ShipId = shipId ?? string.Empty;
            result.SaveRevision = saveRevision;
            D21VoyageEvent last = LastEvent(log);
            result.WorldDay = last == null ? result.WorldDay : last.WorldDay;

            result.Voyage.VoyageId = voyageId ?? string.Empty;
            result.Voyage.ShipId = shipId ?? string.Empty;
            result.Voyage.RouteId = routeId ?? string.Empty;
            result.Voyage.Status = status.ToString();
            result.Voyage.LastTickIndex = Math.Max(0, lastTickIndex);
            result.Voyage.Time = time == null ? null : time.Clone();
            result.Voyage.Course = course == null ? null : course.Clone();
            result.Voyage.RiskSeeds = new List<D21VoyageRiskSeed>();
            if (riskSeeds != null)
            {
                foreach (D21VoyageRiskSeed seed in riskSeeds)
                    if (seed != null) result.Voyage.RiskSeeds.Add(seed.Clone());
            }

            if (anchor != null)
            {
                result.Anchor.AnchorState = anchor.AnchorState.ToString();
                result.Anchor.MooringId = anchor.MooringState == D21MooringState.Moored
                    ? "mooring:" + (anchor.PortId ?? string.Empty) + ":" + (anchor.BerthId ?? string.Empty)
                    : string.Empty;
                result.Anchor.PortId = anchor.PortId ?? string.Empty;
                result.Anchor.BerthId = anchor.BerthId ?? string.Empty;
                result.Anchor.LastTransitionTick = time == null ? 0 : Math.Max(0, time.ElapsedTicks);
                result.Anchor.IsAnchored = anchor.AnchorState == D21AnchorState.Holding ||
                    anchor.AnchorState == D21AnchorState.Dragging ||
                    anchor.AnchorState == D21AnchorState.Deploying ||
                    anchor.AnchorState == D21AnchorState.Recovering;
                result.Anchor.IsMooringStable = anchor.MooringState == D21MooringState.Moored;
            }

            if (weather != null)
            {
                result.WeatherSeed.VoyageSeed = last == null ? result.WeatherSeed.VoyageSeed : last.VoyageSeed;
                result.WeatherSeed.WeatherSeed = Math.Max(0L, weather.Seed);
                result.WeatherSeed.WeatherSourceId = string.IsNullOrEmpty(weather.SourceId)
                    ? "d9_environment" : weather.SourceId;
                result.WeatherSeed.WeatherSnapshotSchemaVersion = weather.SchemaVersion;
                result.WeatherSeed.WeatherSnapshotRevision = Math.Max(0L, weather.SaveRevision);
                if (!string.IsNullOrEmpty(weather.EventId) &&
                    !result.WeatherSeed.AppliedWeatherEventIds.Contains(weather.EventId))
                    result.WeatherSeed.AppliedWeatherEventIds.Add(weather.EventId);
            }
            if (ledger != null)
            {
                result.Provisions.LedgerRevision = saveRevision;
                result.Provisions.ConsumedWaterUnits = Math.Max(0L, ledger.TotalWaterConsumed);
                result.Provisions.ConsumedFoodUnits = Math.Max(0L, ledger.TotalFoodConsumed);
                result.Provisions.LastAppliedTick = Math.Max(0, ledger.LastTickIndex);
                if (ledger.TotalWaterConsumed > 0 || ledger.TotalFoodConsumed > 0)
                    result.Provisions.LastEventId = lastEventId ?? string.Empty;
                else
                    result.Provisions.LastEventId = string.Empty;
            }

            D21VoyageEventLogSnapshot snapshot = BuildCursorSnapshot(log);
            result.Events.EventLog = snapshot;
            result.Events.AppliedEventKeys = new List<string>();
            foreach (D21VoyageEvent item in snapshot.Events)
                result.Events.AppliedEventKeys.Add(item.DedupeKey);
            if (snapshot.Events.Count == 0)
            {
                result.Events.LastEventId = string.Empty;
                result.Events.LastEventSequence = 0;
                result.Events.LastEventSaveRevision = 0;
            }
            else
            {
                D21VoyageEvent finalEvent = snapshot.Events[snapshot.Events.Count - 1];
                result.Events.LastEventId = finalEvent.EventId;
                result.Events.LastEventSequence = finalEvent.LogSequence;
                result.Events.LastEventSaveRevision = finalEvent.SaveRevision;
                result.WorldDay = finalEvent.WorldDay;
            }
        }

        private static bool IsOrderedAfter(D21VoyageEvent previous, D21VoyageEvent next)
        {
            if (previous == null || next == null) return false;
            if (next.TickIndex > previous.TickIndex) return true;
            if (next.TickIndex < previous.TickIndex) return false;
            int previousRank = EventRank(previous.EventType);
            int nextRank = EventRank(next.EventType);
            return previousRank >= 0 && nextRank > previousRank;
        }

        private static int EventRank(D21VoyageEventType type)
        {
            switch (type)
            {
                case D21VoyageEventType.VoyageStarted: return 0;
                case D21VoyageEventType.CourseChanged: return 10;
                case D21VoyageEventType.RiskQualified: return 20;
                case D21VoyageEventType.RiskResolved: return 30;
                case D21VoyageEventType.ProvisionConsumed: return 40;
                case D21VoyageEventType.VoyageTickAdvanced: return 50;
                case D21VoyageEventType.Arrived: return 60;
                case D21VoyageEventType.Stopped: return 70;
                case D21VoyageEventType.Interrupted: return 80;
                case D21VoyageEventType.Rejected: return 90;
                case D21VoyageEventType.Recovered: return 100;
                default: return -1;
            }
        }

        private static bool HasRecoverableCause(
            D21VoyageEventLog log,
            D21VoyageEvent recovered)
        {
            if (recovered == null || recovered.CauseIds == null || recovered.CauseIds.Count == 0)
                return false;
            foreach (D21VoyageEvent item in log.Events)
            {
                if ((item.Outcome == D21VoyageEventOutcome.Failed ||
                     item.Outcome == D21VoyageEventOutcome.Interrupted) &&
                    recovered.CauseIds.Contains(item.EventId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 正常抵达后的返港提交也使用 Recovered 事件承载“跨 owner 已完成”
        /// 语义。它不是故障恢复，故以最近的 Arrived 事件和 D19 Return
        /// 因果键组成窄门；不放宽普通 Recovered 对 Failed/Interrupted 的
        /// 原有约束，也不允许缺少港口返港因果的伪造事件通过。
        /// </summary>
        private static bool HasArrivedReturnCause(
            D21VoyageEventLog log,
            D21VoyageEvent recovered)
        {
            if (log == null || recovered == null || recovered.CauseIds == null ||
                recovered.CauseIds.Count == 0 || log.Events == null ||
                log.Events.Count == 0)
                return false;

            D21VoyageEvent previous = LastEvent(log);
            if (previous == null || previous.EventType != D21VoyageEventType.Arrived ||
                !recovered.CauseIds.Contains(previous.EventId))
                return false;

            foreach (string causeId in recovered.CauseIds)
            {
                if (!string.IsNullOrEmpty(causeId) &&
                    causeId.StartsWith("port:return:", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static D21VoyageEvent LastEvent(D21VoyageEventLog log)
        {
            if (log == null || log.Events == null || log.Events.Count == 0) return null;
            return log.Events[log.Events.Count - 1];
        }

        private static D21VoyageEvent FindByDedupeKey(
            D21VoyageEventLog log,
            string key)
        {
            if (log == null || log.Events == null) return null;
            foreach (D21VoyageEvent item in log.Events)
                if (item != null && string.Equals(item.DedupeKey, key, StringComparison.Ordinal)) return item;
            return null;
        }

        private Dictionary<string, D21VoyageEvent> GetOrBuildEventIndex(
            string voyageId,
            D21VoyageEventLog log)
        {
            Dictionary<string, D21VoyageEvent> result;
            if (_eventIndexes.TryGetValue(voyageId, out result)) return result;
            result = BuildEventIndex(log);
            _eventIndexes[voyageId] = result;
            return result;
        }

        private static Dictionary<string, D21VoyageEvent> BuildEventIndex(
            D21VoyageEventLog log)
        {
            var result = new Dictionary<string, D21VoyageEvent>(StringComparer.Ordinal);
            if (log != null && log.Events != null)
            {
                foreach (D21VoyageEvent item in log.Events)
                    if (item != null) result[item.DedupeKey] = item;
            }
            return result;
        }

        private static D21VoyageEventLogSnapshot BuildCursorSnapshot(
            D21VoyageEventLog log)
        {
            var result = new D21VoyageEventLogSnapshot
            {
                NextSequence = log == null ? 1 : log.NextSequence,
            };
            if (log == null || log.Events == null || log.Events.Count == 0) return result;
            if (log.Events.Count <= FullProjectionEventThreshold)
            {
                foreach (D21VoyageEvent item in log.Events)
                    if (item != null) result.Events.Add(item.Clone());
                return result;
            }
            result.Events.Add(log.Events[log.Events.Count - 1].Clone());
            return result;
        }

        /// <summary>
        /// Clone aggregate fields without rebuilding a large historical event list.
        /// The event cursor is immutable from the resolution owner's perspective;
        /// explicit full projections are materialized by GetProjection.
        /// </summary>
        internal static D21ShipSaveProjection CloneProjectionForRouting(
            D21ShipSaveProjection source)
        {
            if (source == null) return null;
            var result = new D21ShipSaveProjection
            {
                SchemaVersion = source.SchemaVersion,
                ShipId = source.ShipId ?? string.Empty,
                WorldDay = source.WorldDay,
                SaveRevision = source.SaveRevision,
                ShipSnapshot = source.ShipSnapshot == null ? null : source.ShipSnapshot.Clone(),
                Anchor = source.Anchor == null ? null : source.Anchor.Clone(),
                Voyage = source.Voyage == null ? null : source.Voyage.Clone(),
                WeatherSeed = source.WeatherSeed == null ? null : source.WeatherSeed.Clone(),
                Provisions = source.Provisions == null ? null : source.Provisions.Clone(),
                Events = source.Events == null ? new D21EventCursorSaveProjection() : new D21EventCursorSaveProjection
                {
                    SchemaVersion = source.Events.SchemaVersion,
                    LastEventId = source.Events.LastEventId ?? string.Empty,
                    LastEventSequence = source.Events.LastEventSequence,
                    LastEventSaveRevision = source.Events.LastEventSaveRevision,
                    AppliedEventKeys = source.Events.AppliedEventKeys == null
                        ? new List<string>() : new List<string>(source.Events.AppliedEventKeys),
                    EventLog = source.Events.EventLog == null
                        ? new D21VoyageEventLogSnapshot() : source.Events.EventLog.Clone(),
                },
            };
            if (source.UpstreamSnapshots != null)
            {
                foreach (D21OwnerSnapshotReference reference in source.UpstreamSnapshots)
                    if (reference != null) result.UpstreamSnapshots.Add(reference.Clone());
            }
            return result;
        }

        private static bool EventsEquivalent(D21VoyageEvent left, D21VoyageEvent right)
        {
            if (left == null || right == null) return false;
            return string.Equals(D21VoyageEventSerializer.Serialize(left),
                D21VoyageEventSerializer.Serialize(right), StringComparison.Ordinal);
        }

        private static D21VoyageEvent First(IEnumerable<D21VoyageEvent> events)
        {
            if (events == null) return null;
            foreach (D21VoyageEvent item in events) return item;
            return null;
        }

        private static D21VoyageEventRouteResult Rejected(
            D21VoyageEvent item,
            string code,
            D21ShipSaveProjection projection)
        {
            var result = new D21VoyageEventRouteResult
            {
                Accepted = false,
                IsDuplicate = false,
                Code = code ?? EventInvalidCode,
                VoyageId = item == null ? string.Empty : item.VoyageId ?? string.Empty,
                ShipId = item == null ? string.Empty : item.ShipId ?? string.Empty,
                RouteId = item == null ? string.Empty : item.RouteId ?? string.Empty,
                Event = item == null ? null : item.Clone(),
                SaveProjection = projection == null ? null : projection.Clone(),
            };
            if (string.Equals(result.Code, OrderConflictCode, StringComparison.Ordinal) ||
                result.Code.StartsWith(AppendFailedCode, StringComparison.Ordinal))
            {
                result.Compensation = new D21VoyageCompensationDraft
                {
                    CommandId = item == null ? string.Empty : item.CommandId,
                    CauseEventId = item == null ? string.Empty : item.EventId,
                    RecoveryRouteId = "d21.voyage.recover",
                    ReasonCode = result.Code,
                    Steps = new List<string> { "retain_last_committed_event", "resume_from_last_event" },
                };
            }
            return result;
        }
    }
}
