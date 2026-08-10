// ============================================================
// EnvironmentSystem —— D9 天气/海况唯一写者
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D9
{
    /// <summary>
    /// 天气状态与海况影响的唯一写者。Ship/Ecology 只能读取 Snapshot 副本。
    /// </summary>
    public sealed class EnvironmentSystem
    {
        public const string StormEventId = "event_d9_weather_storm";
        public const string OwnerSystem = "EnvironmentSystem";

        private D9WeatherSeaSnapshot _snapshot;

        public EnvironmentSystem(D9WeatherSeaSnapshot initial = null)
        {
            _snapshot = initial == null ? D9WeatherSeaSnapshot.CreateDefault() : initial.Clone();
            _snapshot.Validate();
        }

        public D9WeatherSeaSnapshot Snapshot
        {
            get { return _snapshot.Clone(); }
        }

        /// <summary>EnvironmentSystem 自己写天气；不创建事实事件。</summary>
        public D9WeatherChangeResult SetWeather(D9WeatherState weather, int worldDay, string locationRef = null)
        {
            if (!Enum.IsDefined(typeof(D9WeatherState), weather))
                return D9WeatherChangeResult.Rejected("unknown_weather", "未知天气状态，未改变当前天气。", _snapshot);

            if (_snapshot.Weather == weather && worldDay == _snapshot.WorldDay)
                return D9WeatherChangeResult.Rejected("weather_unchanged", "天气状态没有变化。", _snapshot);

            ApplyWeather(weather, worldDay, locationRef, "");
            return D9WeatherChangeResult.Success(_snapshot, "天气已更新为" + weather + "。", "", null);
        }

        /// <summary>
        /// 追加并应用天气事实。event_id + save_revision 由 EventLog 负责幂等，
        /// AppliedEventIds 负责即使天气快照单独读档也不重复改变影响。
        /// </summary>
        public D9WeatherChangeResult ApplyWeatherEvent(string eventId, D9WeatherState weather,
            int worldDay, string locationRef, long saveRevision, EventLog eventLog = null)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                return D9WeatherChangeResult.Rejected("event_id_required", "天气事件缺少 event_id。", _snapshot);
            if (!Enum.IsDefined(typeof(D9WeatherState), weather))
                return D9WeatherChangeResult.Rejected("unknown_weather", "天气事件包含未知天气状态。", _snapshot);
            if (_snapshot.AppliedEventIds.Contains(eventId))
                return D9WeatherChangeResult.Duplicate(_snapshot, eventId, null);

            FactEvent fact = null;
            if (eventLog != null)
            {
                var append = eventLog.Append(BuildWorldEvent(eventId, weather, worldDay, locationRef, saveRevision));
                fact = append.Fact;
                if (append.IsDuplicate)
                {
                    _snapshot.AppliedEventIds.Add(eventId);
                    _snapshot.LastEventId = eventId;
                    return D9WeatherChangeResult.Duplicate(_snapshot, eventId, fact);
                }
            }

            ApplyWeather(weather, worldDay, locationRef, eventId);
            return D9WeatherChangeResult.Success(_snapshot, "天气事件已结算：" + eventId + "。", eventId, fact);
        }

        public D9WeatherChangeResult ApplyStormEvent(int worldDay, string locationRef,
            long saveRevision, EventLog eventLog = null)
        {
            return ApplyWeatherEvent(StormEventId, D9WeatherState.Storm, worldDay,
                locationRef, saveRevision, eventLog);
        }

        /// <summary>推进环境时间；不自动再次触发天气事件。</summary>
        public D9WeatherSeaSnapshot AdvanceWorldDay(int days = 1, string locationRef = null)
        {
            int safeDays = Math.Max(0, days);
            _snapshot.WorldDay = Math.Max(0, _snapshot.WorldDay + safeDays);
            _snapshot.Revision = Math.Max(0, _snapshot.Revision + safeDays);
            if (!string.IsNullOrEmpty(locationRef)) _snapshot.LocationRef = locationRef;
            return Snapshot;
        }

        public void Restore(D9WeatherSeaSnapshot saved)
        {
            if (saved == null) throw new ArgumentNullException("saved");
            saved.Validate();
            _snapshot = saved.Clone();
        }

        /// <summary>D7/D3 旧档没有 D9 快照时的兼容恢复。</summary>
        public void RestoreLegacy(D3WeatherType legacyWeather, int worldDay, string locationRef = null)
        {
            D9WeatherState state = D9WeatherProfiles.FromLegacy(legacyWeather);
            ApplyWeather(state, Math.Max(0, worldDay), locationRef, "");
        }

        /// <summary>把天气只读投影发布到 D3 快照；不把 D3 变成天气写者。</summary>
        public void PublishTo(D3RuntimeSnapshot runtime)
        {
            if (runtime == null) throw new ArgumentNullException("runtime");
            runtime.Weather = _snapshot.LegacyWeather;
            runtime.WeatherSea = Snapshot;
        }

        private void ApplyWeather(D9WeatherState weather, int worldDay, string locationRef, string eventId)
        {
            var profile = D9WeatherProfiles.Get(weather);
            _snapshot.ApplyProfile(profile);
            _snapshot.WorldDay = Math.Max(0, worldDay);
            _snapshot.Revision = Math.Max(0, _snapshot.Revision + 1);
            if (!string.IsNullOrEmpty(locationRef)) _snapshot.LocationRef = locationRef;
            _snapshot.LastEventId = eventId ?? "";
            if (!string.IsNullOrEmpty(eventId) && !_snapshot.AppliedEventIds.Contains(eventId))
                _snapshot.AppliedEventIds.Add(eventId);
        }

        private static WorldEvent BuildWorldEvent(string eventId, D9WeatherState weather,
            int worldDay, string locationRef, long saveRevision)
        {
            string safeLocation = string.IsNullOrWhiteSpace(locationRef)
                ? "nearshore_saint_marys" : locationRef;
            var profile = D9WeatherProfiles.Get(weather);
            var result = new EventResult
            {
                ResultCode = "weather_applied",
                Summary = "天气变为" + weather + "，海况影响已发布。",
                Values = new Dictionary<string, string>
                {
                    ["weather_state"] = weather.ToString(),
                    ["wind_speed"] = profile.WindSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["visibility"] = profile.Visibility.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["current_speed"] = profile.CurrentSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["hull_risk"] = profile.HullRisk.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["fish_activity_multiplier"] = profile.FishActivityMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture),
                },
            };
            return new WorldEvent(eventId, Math.Max(0, worldDay), safeLocation, OwnerSystem,
                result, null, null, null, Math.Max(0L, saveRevision));
        }
    }
}
