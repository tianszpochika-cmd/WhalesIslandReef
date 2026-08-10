// D13-A04 Ecology bridge。
// 牧场只提交饮水请求；水源可用量由 EcologySystem 唯一写入并保存。

using System;
using System.Collections.Generic;
using System.Globalization;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D4
{
    public sealed partial class D4EcologySystem : ID13RanchWaterAuthority
    {
        private readonly Dictionary<string, D13RanchWaterState> _d13RanchWater =
            new Dictionary<string, D13RanchWaterState>(StringComparer.Ordinal);
        private readonly HashSet<string> _d13RanchWaterAppliedKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private long _d13RanchWaterRevision;

        public D13RanchWaterState EnsureRanch(D13RanchFacilityDefinition definition, int worldDay)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.RanchId)) return null;
            if (_d13RanchWater.TryGetValue(definition.RanchId, out D13RanchWaterState existing)) return existing.Clone();
            var state = new D13RanchWaterState
            {
                RanchId = definition.RanchId,
                SourceId = definition.WaterSourceId,
                Available = definition.WaterCapacity,
                Capacity = definition.WaterCapacity,
                LastUpdatedDay = Math.Max(0, worldDay),
            };
            _d13RanchWater[definition.RanchId] = state;
            return state.Clone();
        }

        public D13RanchWaterState FindRanch(string ranchId)
        {
            return _d13RanchWater.TryGetValue(ranchId ?? string.Empty, out D13RanchWaterState state) ? state.Clone() : null;
        }

        public D13RanchWaterResult ConsumeWater(D13RanchWaterRequest request, EventLog eventLog)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RanchId) || request.Amount <= 0)
                return RejectRanchWater("water_request_invalid", "牧场饮水请求必须带有效数量和牧场。", request == null ? string.Empty : request.EventId);
            if (!_d13RanchWater.TryGetValue(request.RanchId, out D13RanchWaterState state))
                return RejectRanchWater("ranch_water_missing", "EcologySystem 没有该牧场的水源状态。", request.EventId);
            if (!string.Equals(state.SourceId, request.SourceId, StringComparison.Ordinal))
                return RejectRanchWater("water_source_mismatch", "饮水请求没有匹配登记水源。", request.EventId, state);

            string eventId = string.IsNullOrWhiteSpace(request.EventId)
                ? "event_d13_ranch_water_" + request.RanchId
                : request.EventId;
            long revision = Math.Max(0L, request.SaveRevision);
            string key = eventId + "\u001f" + revision.ToString(CultureInfo.InvariantCulture);
            if (_d13RanchWaterAppliedKeys.Contains(key) || (eventLog != null && eventLog.Find(eventId, revision) != null))
            {
                return new D13RanchWaterResult
                {
                    Succeeded = true,
                    Duplicate = true,
                    Code = "duplicate",
                    Message = "该次牧场饮水已结算。",
                    EventId = eventId,
                    State = state.Clone(),
                    Fact = eventLog == null ? null : eventLog.Find(eventId, revision),
                };
            }

            if (state.Available < request.Amount)
                return RejectRanchWater("water_source_empty", "登记水源没有足够饮水，不能隐瞒缺水后果。", eventId, state);
            state.Available -= request.Amount;
            state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, request.WorldDay));
            state.Revision++;
            state.LastEventId = eventId;
            _d13RanchWaterRevision++;
            _d13RanchWaterAppliedKeys.Add(key);
            FactEvent fact = null;
            if (eventLog != null)
            {
                fact = eventLog.Append(new WorldEvent(eventId, Math.Max(0, request.WorldDay), "island_saint_marys",
                    D13RanchIds.EcologyOwner,
                    new EventResult
                    {
                        ResultCode = "ranch_water_consumed",
                        Summary = "EcologySystem 已从登记水源扣除牧场饮水。",
                        FactIds = new List<string> { "ranch_water_fact:" + eventId },
                        Values = new Dictionary<string, string>
                        {
                            ["ranch_id"] = state.RanchId,
                            ["source_id"] = state.SourceId,
                            ["quantity"] = request.Amount.ToString(CultureInfo.InvariantCulture),
                            ["water_available_after"] = state.Available.ToString(CultureInfo.InvariantCulture),
                        },
                    }, new[] { D13RanchIds.PlayerRef }, new[] { "ranch:water" }, new string[0], revision)).Fact;
            }
            return new D13RanchWaterResult
            {
                Succeeded = true,
                Applied = true,
                Code = "ranch_water_consumed",
                Message = "登记水源饮水已由 EcologySystem 结算。",
                EventId = eventId,
                Quantity = request.Amount,
                State = state.Clone(),
                Fact = fact,
            };
        }

        public D13RanchWaterRuntimeSnapshot CaptureRanchSnapshot()
        {
            var result = new D13RanchWaterRuntimeSnapshot { Revision = _d13RanchWaterRevision };
            foreach (var item in _d13RanchWater.Values) result.Sources.Add(item.Clone());
            foreach (var key in _d13RanchWaterAppliedKeys) result.AppliedEventKeys.Add(key);
            return result;
        }

        public void RestoreRanchSnapshot(D13RanchWaterRuntimeSnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Validate();
            _d13RanchWater.Clear();
            _d13RanchWaterAppliedKeys.Clear();
            foreach (var item in snapshot.Sources) _d13RanchWater[item.RanchId] = item.Clone();
            foreach (var key in snapshot.AppliedEventKeys) _d13RanchWaterAppliedKeys.Add(key);
            _d13RanchWaterRevision = snapshot.Revision;
        }

        private static D13RanchWaterResult RejectRanchWater(string code, string message, string eventId, D13RanchWaterState state = null)
        {
            return new D13RanchWaterResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                EventId = eventId,
                State = state == null ? null : state.Clone(),
            };
        }
    }
}
