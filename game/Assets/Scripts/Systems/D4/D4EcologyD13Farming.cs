// D13-A03 EcologySystem bridge。
// Soil, water source, pollution and farming ecological pressure remain owned by
// D4EcologySystem; FarmingSystem only submits typed environment operations.

using System;
using System.Collections.Generic;
using System.Globalization;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D4
{
    public sealed partial class D4EcologySystem : ID13FarmingEcologyAuthority
    {
        private readonly Dictionary<string, D13FarmingEcologyEnvironmentState> _d13FarmingEnvironments =
            new Dictionary<string, D13FarmingEcologyEnvironmentState>(StringComparer.Ordinal);
        private long _d13FarmingRevision;
        private readonly HashSet<string> _d13FarmingAppliedEventKeys = new HashSet<string>(StringComparer.Ordinal);

        public D13FarmingEcologyEnvironmentState EnsurePlot(D13FarmingPlotDefinition definition, int worldDay, int seed)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.PlotId)) return null;
            if (_d13FarmingEnvironments.TryGetValue(definition.PlotId, out D13FarmingEcologyEnvironmentState existing))
                return existing.Clone();
            int fertility = 62 + StableFarming(definition.PlotId + ":fertility:" + seed) % 24;
            int capacity = D13FarmingConfiguration.Int(D13FarmingConfiguration.DefaultWaterCapacity);
            var state = new D13FarmingEcologyEnvironmentState
            {
                PlotId = definition.PlotId,
                SoilType = definition.SoilType,
                Fertility = Math.Max(0, Math.Min(100, fertility)),
                Moisture = 50,
                WaterAvailable = capacity,
                WaterCapacity = capacity,
                WaterSourceId = definition.WaterSourceId,
                LastUpdatedDay = Math.Max(0, worldDay),
            };
            _d13FarmingEnvironments[definition.PlotId] = state;
            return state.Clone();
        }

        public D13FarmingEcologyEnvironmentState FindPlot(string plotId)
        {
            return _d13FarmingEnvironments.TryGetValue(plotId ?? string.Empty, out D13FarmingEcologyEnvironmentState state)
                ? state.Clone() : null;
        }

        public D13FarmingEnvironmentResult Apply(D13FarmingEnvironmentRequest request, EventLog eventLog)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlotId))
                return Reject("invalid_request", "农业生态环境请求缺少地块。", request == null ? string.Empty : request.EventId);
            if (!_d13FarmingEnvironments.TryGetValue(request.PlotId, out D13FarmingEcologyEnvironmentState state))
                return Reject("farming_plot_missing", "EcologySystem 没有该地块的土壤/水源状态。", request.EventId);
            string eventId = string.IsNullOrWhiteSpace(request.EventId)
                ? "event_d13_farming_ecology_" + request.PlotId + "_" + request.Operation
                : request.EventId;
            string key = eventId + "@" + Math.Max(0L, request.SaveRevision).ToString(CultureInfo.InvariantCulture);
            if (_d13FarmingAppliedEventKeys.Contains(key) || (eventLog != null && eventLog.Find(eventId, Math.Max(0L, request.SaveRevision)) != null))
                return new D13FarmingEnvironmentResult
                {
                    Succeeded = true,
                    Duplicate = true,
                    Code = "duplicate",
                    Message = "农业生态环境事实已结算。",
                    EventId = eventId,
                    Environment = state.Clone(),
                    Fact = eventLog == null ? null : eventLog.Find(eventId, Math.Max(0L, request.SaveRevision)),
                };

            int quantity = 0;
            switch (request.Operation)
            {
                case D13FarmingEcologyOperation.Planting:
                    state.Fertility = Clamp(state.Fertility - 1, 0, 100);
                    state.Moisture = Clamp(state.Moisture - 5, 0, 100);
                    state.EcologyPressure += 1;
                    quantity = 1;
                    break;
                case D13FarmingEcologyOperation.Watering:
                    if (request.Amount <= 0) return Reject("water_amount_invalid", "浇水量必须大于零。", eventId, state);
                    quantity = Math.Min(request.Amount, state.WaterAvailable);
                    if (quantity <= 0) return Reject("water_source_empty", "水源当前没有可用水量。", eventId, state);
                    state.WaterAvailable -= quantity;
                    state.Moisture = Clamp(state.Moisture + quantity * 2, 0, 100);
                    state.EcologyPressure = Math.Max(0, state.EcologyPressure + (quantity > 40 ? 1 : 0));
                    break;
                case D13FarmingEcologyOperation.PestInfestation:
                    quantity = Math.Max(1, request.Amount);
                    state.PestPressure = Clamp(state.PestPressure + quantity, 0, 100);
                    state.EcologyPressure += Math.Max(1, quantity / 20);
                    break;
                case D13FarmingEcologyOperation.PestTreatment:
                    quantity = Math.Max(1, request.Amount);
                    state.PestPressure = Math.Max(0, state.PestPressure - quantity * 25);
                    state.Pollution = Clamp(state.Pollution + (quantity > 1 ? 1 : 0), 0, 100);
                    break;
                case D13FarmingEcologyOperation.Storm:
                    quantity = Clamp(request.StormIntensity <= 0 ? D13FarmingConfiguration.Int(D13FarmingConfiguration.DefaultStormIntensity) : request.StormIntensity, 1, 100);
                    state.StormDamage = Clamp(state.StormDamage + quantity, 0, 100);
                    state.Moisture = Clamp(state.Moisture + quantity / 2, 0, 100);
                    state.Fertility = Clamp(state.Fertility - quantity / 12, 0, 100);
                    state.EcologyPressure += Math.Max(1, quantity / 20);
                    break;
                case D13FarmingEcologyOperation.Harvest:
                    quantity = Math.Max(1, request.Amount);
                    state.Fertility = Clamp(state.Fertility - quantity, 0, 100);
                    state.Moisture = Clamp(state.Moisture - Math.Max(1, quantity / 2), 0, 100);
                    state.EcologyPressure += quantity;
                    break;
                case D13FarmingEcologyOperation.Rotation:
                    quantity = Math.Max(1, request.Amount);
                    state.Fertility = Clamp(state.Fertility + quantity, 0, 100);
                    state.PestPressure = Math.Max(0, state.PestPressure - quantity);
                    state.EcologyPressure = Math.Max(0, state.EcologyPressure - 1);
                    break;
                case D13FarmingEcologyOperation.AdvanceDay:
                    quantity = Math.Max(1, request.Days);
                    int moistureLoss = request.Rainfall ? Math.Max(0, quantity * 2) : quantity * 5;
                    state.Moisture = Clamp(state.Moisture - moistureLoss, 0, 100);
                    state.WaterAvailable = Clamp(state.WaterAvailable + (request.Rainfall ? quantity * 12 : quantity * 4), 0, state.WaterCapacity);
                    state.PestPressure = Math.Max(0, state.PestPressure - quantity * 2);
                    state.EcologyPressure = Math.Max(0, state.EcologyPressure - quantity);
                    break;
                default:
                    return Reject("ecology_operation_invalid", "未知的农业生态操作。", eventId, state);
            }

            state.LastUpdatedDay = Math.Max(state.LastUpdatedDay, Math.Max(0, request.WorldDay));
            state.Revision++;
            _d13FarmingRevision++;
            state.LastEventId = eventId;
            _d13FarmingAppliedEventKeys.Add(key);
            FactEvent fact = AppendEnvironmentEvent(eventId, request, state, quantity, eventLog);
            return new D13FarmingEnvironmentResult
            {
                Succeeded = true,
                Applied = true,
                Code = "farming_ecology_applied",
                Message = "土壤、水源和生态压力已由 EcologySystem 结算。",
                EventId = eventId,
                Quantity = quantity,
                Environment = state.Clone(),
                Fact = fact,
            };
        }

        public D13FarmingEcologyRuntimeSnapshot CaptureSnapshot()
        {
            var result = new D13FarmingEcologyRuntimeSnapshot { Revision = _d13FarmingRevision };
            foreach (var item in _d13FarmingEnvironments.Values) result.Environments.Add(item.Clone());
            foreach (var key in _d13FarmingAppliedEventKeys) result.AppliedEventKeys.Add(key);
            return result;
        }

        public void RestoreSnapshot(D13FarmingEcologyRuntimeSnapshot snapshot)
        {
            if (snapshot == null) return;
            snapshot.Validate();
            _d13FarmingEnvironments.Clear();
            _d13FarmingAppliedEventKeys.Clear();
            foreach (var item in snapshot.Environments) _d13FarmingEnvironments[item.PlotId] = item.Clone();
            foreach (var key in snapshot.AppliedEventKeys) _d13FarmingAppliedEventKeys.Add(key);
            _d13FarmingRevision = snapshot.Revision;
        }

        private static FactEvent AppendEnvironmentEvent(string eventId, D13FarmingEnvironmentRequest request,
            D13FarmingEcologyEnvironmentState state, int quantity, EventLog eventLog)
        {
            if (eventLog == null) return null;
            var worldEvent = new WorldEvent(eventId, Math.Max(0, request.WorldDay), "island_saint_marys",
                D13FarmingIds.EcologyOwner,
                new EventResult
                {
                    ResultCode = "farming_ecology_applied",
                    Summary = "EcologySystem 已写入农业土壤、水源和生态压力。",
                    FactIds = new List<string> { "farming_ecology_fact:" + eventId },
                    Values = new Dictionary<string, string>
                    {
                        ["plot_id"] = state.PlotId,
                        ["operation"] = request.Operation.ToString(),
                        ["quantity"] = quantity.ToString(CultureInfo.InvariantCulture),
                        ["fertility_after"] = state.Fertility.ToString(CultureInfo.InvariantCulture),
                        ["moisture_after"] = state.Moisture.ToString(CultureInfo.InvariantCulture),
                        ["water_available_after"] = state.WaterAvailable.ToString(CultureInfo.InvariantCulture),
                        ["ecology_pressure_after"] = state.EcologyPressure.ToString(CultureInfo.InvariantCulture),
                        ["pest_pressure_after"] = state.PestPressure.ToString(CultureInfo.InvariantCulture),
                        ["storm_damage_after"] = state.StormDamage.ToString(CultureInfo.InvariantCulture),
                    },
                },
                new[] { D13FarmingIds.PlayerRef },
                new[] { "farming:environment" },
                new string[0],
                Math.Max(0, request.SaveRevision));
            return eventLog.Append(worldEvent).Fact;
        }

        private static D13FarmingEnvironmentResult Reject(string code, string message, string eventId,
            D13FarmingEcologyEnvironmentState state = null)
        {
            return new D13FarmingEnvironmentResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                EventId = eventId,
                Environment = state == null ? null : state.Clone(),
            };
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static int StableFarming(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }
    }
}
