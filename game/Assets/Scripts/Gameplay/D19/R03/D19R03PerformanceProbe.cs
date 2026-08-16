using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace Whalebone.Gameplay.D19.R03
{
    /// <summary>
    /// Read-only performance evidence for D19-R03-A03.
    /// The probe samples the actual standalone Player after a 30 second warm-up
    /// and writes a 60 second frame-time window. It never changes gameplay,
    /// save data, or the D16 performance controller.
    /// </summary>
    public sealed class D19R03PerformanceProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class Result
        {
            public string schema = "D19_R03_A03_PERFORMANCE_PLAYER_V1";
            public string task = "D19-R03-A03";
            public string evidence_mode = "real standalone Windows x64 Player; per-frame runtime sampling";
            public string scene;
            public string quality_argument;
            public string quality_name;
            public int requested_quality_index = -1;
            public int actual_quality_index = -1;
            public string unity_version;
            public string graphics_device;
            public string graphics_api;
            public string graphics_device_version;
            public int screen_width;
            public int screen_height;
            public int display_width;
            public int display_height;
            public bool fullscreen;
            public string fullscreen_mode;
            public int vsync_count;
            public int target_frame_rate;
            public float warmup_seconds_requested = 30f;
            public float sample_seconds_requested = 60f;
            public float warmup_seconds_actual;
            public float sample_seconds_actual;
            public int sample_count;
            public float average_fps;
            public float p95_frame_ms;
            public float one_percent_low_fps;
            public float min_fps;
            public float average_frame_ms;
            public float longest_frame_ms;
            public int long_frame_count_over_50ms;
            public long peak_allocated_bytes;
            public long peak_reserved_bytes;
            public long peak_managed_bytes;
            public int managed_gc_gen0;
            public int managed_gc_gen1;
            public int managed_gc_gen2;
            public float average_main_thread_ms = -1f;
            public float gpu_frame_ms = -1f;
            public bool gpu_sample_available;
            public string gpu_counter_name;
            public string completion;
        }

        private const float WarmupSeconds = 30f;
        private const float SampleSeconds = 60f;
        private readonly List<float> _frameMilliseconds = new List<float>(4096);
        private Result _result;
        private float _startedAt;
        private float _sampleStartedAt;
        private bool _sampling;
        private bool _wrote;
        private double _fpsTotal;
        private double _frameTotalMs;
        private double _mainThreadTotalMs;
        private double _gpuTotalMs;
        private int _timingSamples;
        private int _gpuSamples;
        private int _gcGen0Start;
        private int _gcGen1Start;
        private int _gcGen2Start;
        private readonly FrameTiming[] _frameTimings = new FrameTiming[1];
        private ProfilerRecorder _gpuRecorder;
        private bool _gpuRecorderStarted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string quality;
            if (!TryReadQualityFlag(out quality)) return;
            if (FindObjectOfType<D19R03PerformanceProbe>() != null) return;
            var host = new GameObject("D19R03PerformanceProbe");
            DontDestroyOnLoad(host);
            var probe = host.AddComponent<D19R03PerformanceProbe>();
            probe._requestedQuality = quality;
            if (HasFlag("-d19-r03-perf-harbor") &&
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "InitialHarbor")
                UnityEngine.SceneManagement.SceneManager.LoadScene("InitialHarbor");
        }

        private string _requestedQuality;
        private string _runLabel;

        private static bool TryReadQualityFlag(out string quality)
        {
            quality = string.Empty;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith("-d19-r03-perf=", StringComparison.OrdinalIgnoreCase)) continue;
                quality = args[i].Substring("-d19-r03-perf=".Length).Trim().ToLowerInvariant();
                return quality == "low" || quality == "medium" || quality == "high";
            }
            return false;
        }

        private static bool HasFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(_requestedQuality))
                TryReadQualityFlag(out _requestedQuality);
            _runLabel = ReadRunLabel();
            int requestedIndex = FindQualityIndex(_requestedQuality);
            if (requestedIndex >= 0 && requestedIndex < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(requestedIndex, true);

            _result = new Result
            {
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                quality_argument = _requestedQuality ?? string.Empty,
                quality_name = QualitySettings.names.Length > 0 ? QualitySettings.names[QualitySettings.GetQualityLevel()] : string.Empty,
                requested_quality_index = requestedIndex,
                actual_quality_index = QualitySettings.GetQualityLevel(),
                unity_version = Application.unityVersion,
                graphics_device = SystemInfo.graphicsDeviceName,
                graphics_api = SystemInfo.graphicsDeviceType.ToString(),
                graphics_device_version = SystemInfo.graphicsDeviceVersion,
                screen_width = Screen.width,
                screen_height = Screen.height,
                display_width = Display.main == null ? 0 : Display.main.systemWidth,
                display_height = Display.main == null ? 0 : Display.main.systemHeight,
                fullscreen = Screen.fullScreen,
                fullscreen_mode = Screen.fullScreenMode.ToString(),
                vsync_count = QualitySettings.vSyncCount,
                target_frame_rate = Application.targetFrameRate,
                gpu_counter_name = string.Empty,
                completion = "warming_up"
            };
            _startedAt = Time.realtimeSinceStartup;
            _sampleStartedAt = _startedAt + WarmupSeconds;
            _gcGen0Start = GC.CollectionCount(0);
            _gcGen1Start = GC.CollectionCount(1);
            _gcGen2Start = GC.CollectionCount(2);
            FrameTimingManager.CaptureFrameTimings();
        }

        private static int FindQualityIndex(string requestedQuality)
        {
            if (!string.IsNullOrEmpty(requestedQuality))
            {
                string desired = requestedQuality.Trim();
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    if (string.Equals(QualitySettings.names[i], desired, StringComparison.OrdinalIgnoreCase))
                        return i;
                }
            }

            // Keep an auditable fallback for projects that expose fewer than the
            // canonical Low/Medium/High profiles. The emitted quality_name remains
            // the runtime truth and cannot be mistaken for a requested label.
            return QualitySettings.GetQualityLevel();
        }

        private static string ReadRunLabel()
        {
            var args = Environment.GetCommandLineArgs();
            const string prefix = "-d19-r03-perf-run=";
            for (int i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                string raw = args[i].Substring(prefix.Length).Trim();
                if (string.IsNullOrEmpty(raw)) return string.Empty;
                var chars = raw.ToCharArray();
                for (int c = 0; c < chars.Length; c++)
                {
                    if (!(char.IsLetterOrDigit(chars[c]) || chars[c] == '-' || chars[c] == '_'))
                        chars[c] = '_';
                }
                return new string(chars);
            }
            return string.Empty;
        }

        private void Update()
        {
            if (_wrote) return;
            float now = Time.realtimeSinceStartup;
            float frameMs = Time.unscaledDeltaTime * 1000f;
            if (!_sampling)
            {
                if (now < _sampleStartedAt) return;
                _sampling = true;
                _sampleStartedAt = now;
                _result.completion = "sampling";
                _result.scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                TryStartGpuRecorder();
            }

            if (frameMs >= 0.1f && frameMs <= 5000f)
            {
                _frameMilliseconds.Add(frameMs);
                double fps = 1000d / frameMs;
                _fpsTotal += fps;
                _frameTotalMs += frameMs;
                _result.sample_count = _frameMilliseconds.Count;
                _result.average_fps = (float)(_fpsTotal / _frameMilliseconds.Count);
                _result.average_frame_ms = (float)(_frameTotalMs / _frameMilliseconds.Count);
                if (frameMs > _result.longest_frame_ms) _result.longest_frame_ms = frameMs;
                if (frameMs > 50f) _result.long_frame_count_over_50ms++;
                _result.peak_allocated_bytes = Math.Max(_result.peak_allocated_bytes, Profiler.GetTotalAllocatedMemoryLong());
                _result.peak_reserved_bytes = Math.Max(_result.peak_reserved_bytes, Profiler.GetTotalReservedMemoryLong());
                _result.peak_managed_bytes = Math.Max(_result.peak_managed_bytes, GC.GetTotalMemory(false));
                _result.managed_gc_gen0 = Math.Max(0, GC.CollectionCount(0) - _gcGen0Start);
                _result.managed_gc_gen1 = Math.Max(0, GC.CollectionCount(1) - _gcGen1Start);
                _result.managed_gc_gen2 = Math.Max(0, GC.CollectionCount(2) - _gcGen2Start);
                CaptureFrameTiming();
            }

            if (now - _sampleStartedAt >= SampleSeconds)
            {
                FinalizeResult(now - _sampleStartedAt);
                WriteResult();
            }
        }

        private void TryStartGpuRecorder()
        {
            string[] candidates = { "GPU Frame Time", "GPU Time", "GPU Frame Time (ms)" };
            foreach (var name in candidates)
            {
                try
                {
                    var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, name, 15, ProfilerRecorderOptions.GpuRecorder);
                    if (recorder.Valid)
                    {
                        _gpuRecorder = recorder;
                        _gpuRecorderStarted = true;
                        _result.gpu_counter_name = name;
                        return;
                    }
                    recorder.Dispose();
                }
                catch (Exception) { }
            }
            _result.gpu_counter_name = "unavailable";
        }

        private void CaptureFrameTiming()
        {
            FrameTimingManager.CaptureFrameTimings();
            ulong count = FrameTimingManager.GetLatestTimings(1, _frameTimings);
            if (count == 0) return;
            var timing = _frameTimings[0];
            if (timing.cpuMainThreadFrameTime > 0f)
            {
                _mainThreadTotalMs += timing.cpuMainThreadFrameTime;
                _result.average_main_thread_ms = (float)(_mainThreadTotalMs / ++_timingSamples);
            }
            if (timing.gpuFrameTime > 0f)
            {
                _gpuTotalMs += timing.gpuFrameTime;
                _result.gpu_frame_ms = (float)(_gpuTotalMs / ++_gpuSamples);
                _result.gpu_sample_available = true;
            }
            if (!_result.gpu_sample_available && _gpuRecorderStarted && _gpuRecorder.Valid && _gpuRecorder.Count > 0)
            {
                double nanoseconds = _gpuRecorder.LastValueAsDouble;
                if (nanoseconds > 0d)
                {
                    _gpuTotalMs += nanoseconds / 1000000d;
                    _result.gpu_frame_ms = (float)(_gpuTotalMs / ++_gpuSamples);
                    _result.gpu_sample_available = true;
                }
            }
        }

        private void FinalizeResult(float sampleSeconds)
        {
            _result.sample_seconds_actual = sampleSeconds;
            _result.warmup_seconds_actual = Mathf.Max(0f, _sampleStartedAt - _startedAt);
            if (_frameMilliseconds.Count == 0)
            {
                _result.completion = "no_samples";
                return;
            }
            _frameMilliseconds.Sort();
            int p95Index = Mathf.Clamp(Mathf.CeilToInt(_frameMilliseconds.Count * 0.95f) - 1, 0, _frameMilliseconds.Count - 1);
            int onePercentCount = Mathf.Max(1, Mathf.CeilToInt(_frameMilliseconds.Count * 0.01f));
            double slowestOnePercentMs = 0d;
            for (int i = _frameMilliseconds.Count - onePercentCount; i < _frameMilliseconds.Count; i++)
                slowestOnePercentMs += _frameMilliseconds[i];
            slowestOnePercentMs /= onePercentCount;
            _result.p95_frame_ms = _frameMilliseconds[p95Index];
            _result.one_percent_low_fps = (float)(1000d / slowestOnePercentMs);
            _result.min_fps = 1000f / _frameMilliseconds[_frameMilliseconds.Count - 1];
            _result.completion = "complete";
        }

        private void OnApplicationQuit() { WriteResult(); }
        private void OnDestroy()
        {
            if (_gpuRecorderStarted) _gpuRecorder.Dispose();
            WriteResult();
        }

        private void WriteResult()
        {
            if (_wrote || _result == null) return;
            _wrote = true;
            if (_result.completion == "sampling") FinalizeResult(Time.realtimeSinceStartup - _sampleStartedAt);
            string quality = string.IsNullOrEmpty(_requestedQuality) ? "unknown" : _requestedQuality;
            string runSuffix = string.IsNullOrEmpty(_runLabel) ? string.Empty : "_" + _runLabel;
            // The R03 Player is built beside its evidence files. Keeping the
            // report beside the executable avoids relying on platform-specific
            // persistentDataPath locations and makes the raw run auditable.
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "D19_R03_A03_PERFORMANCE_" + quality + runSuffix + ".json");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(_result, true));
                Debug.Log("[D19-R03-A03] Performance result written: " + path);
            }
            catch (Exception ex)
            {
                Debug.LogError("[D19-R03-A03] Performance result write failed: " + ex);
            }
        }
    }
}
