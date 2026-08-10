// ============================================================
// D9AudioPresentation —— D9 关键事实/状态音效
//
// 音频只响应统一 EventLog 事实和只读天气快照，不调用领域系统，
// 不追加事件，也不修改任何金钱、船体、鱼获或关系状态。
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D3;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Gameplay.D9
{
    public static class D9AudioCueRouter
    {
        public static string CueForEvent(string eventId)
        {
            switch (eventId)
            {
                case "event_d9_departure_prepared": return "departure_prepared";
                case "event_d9_departure": return "departure";
                case "event_d9_fishing_catch": return "fishing_catch";
                case "event_d9_weather_storm": return "weather_storm";
                case "event_d9_crew_report": return "crew_report";
                case "event_d9_nearshore_patrol": return "combat_warning";
                case "event_d9_nearshore_retreat": return "combat_retreat";
                case "event_d9_treasure_clue": return "treasure_clue";
                case "event_d9_treasure_resolved": return "treasure_resolved";
                case "event_d9_returned_to_harbor": return "returned_to_harbor";
                case "event_d9_trade_settled": return "trade_settled";
                case "event_d9_wage_settled": return "wage_settled";
                case "event_d9_ship_repaired": return "ship_repaired";
                case "event_d9_save_checkpoint": return "save_checkpoint";
                default: return string.Empty;
            }
        }
    }

    public sealed class D9AudioPresentationController : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D9AudioPresentationController>(FindObjectsInactive.Include) != null) return;
            new GameObject("D9AudioPresentationController").AddComponent<D9AudioPresentationController>();
        }

        private readonly D9EventFactCursor _cursor = new D9EventFactCursor();
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _oneShot;
        private D3RuntimeBootstrap _d3;
        private D3WeatherType _lastWeather;
        private bool _weatherInitialized;

        public bool IsReady { get; private set; }
        public bool UsesFallbackClips { get; private set; }
        public int OneShotCount { get; private set; }
        public string LastCue { get; private set; }

        private void Awake()
        {
            _oneShot = CreateSource("D9_OneShotSource", 0.12f);
            string[] cues =
            {
                "departure_prepared", "departure", "fishing_catch", "weather_storm", "crew_report",
                "combat_warning", "combat_retreat", "treasure_clue", "treasure_resolved", "returned_to_harbor",
                "trade_settled", "wage_settled", "ship_repaired", "save_checkpoint"
            };
            foreach (var cue in cues) _clips[cue] = LoadOrCreate(cue);
            IsReady = _oneShot != null && _clips.Count == cues.Length;
        }

        private void Update()
        {
            if (!IsReady) return;
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            var eventLog = D9EventLogBridge.Current;
            _cursor.Bind(eventLog);
            foreach (var fact in _cursor.ConsumeNewFacts()) PlayFact(fact);
            RefreshWeatherCue();
        }

        private void RefreshWeatherCue()
        {
            if (_d3 == null || !_d3.IsReady || _d3.Snapshot == null) return;
            var weather = _d3.Snapshot.Weather;
            if (!_weatherInitialized)
            {
                _lastWeather = weather;
                _weatherInitialized = true;
                return;
            }
            if (weather == _lastWeather) return;
            _lastWeather = weather;
            if (weather == D3WeatherType.Storm || weather == D3WeatherType.Hurricane)
                PlayCue("weather_storm");
        }

        private void PlayFact(FactEvent fact)
        {
            if (fact == null) return;
            string cue = D9AudioCueRouter.CueForEvent(fact.EventId);
            if (!string.IsNullOrEmpty(cue)) PlayCue(cue);
        }

        private void PlayCue(string cue)
        {
            AudioClip clip;
            if (_oneShot == null || !_clips.TryGetValue(cue, out clip) || clip == null) return;
            _oneShot.PlayOneShot(clip);
            OneShotCount++;
            LastCue = cue;
        }

        private AudioSource CreateSource(string name, float volume)
        {
            var sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(transform, false);
            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        private AudioClip LoadOrCreate(string cue)
        {
            var loaded = Resources.Load<AudioClip>("D9/Audio/" + cue);
            if (loaded != null) return loaded;
            UsesFallbackClips = true;
            int sampleRate = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(0.22f * sampleRate));
            var clip = AudioClip.Create("D9_Fallback_" + cue, sampleCount, 1, sampleRate, false);
            var samples = new float[sampleCount];
            float frequency = 170f + (Math.Abs(cue.GetHashCode()) % 6) * 55f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - t / 0.22f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.42f * envelope;
            }
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
