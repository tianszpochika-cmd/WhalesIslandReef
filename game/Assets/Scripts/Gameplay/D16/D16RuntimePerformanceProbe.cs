using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// D16 性能采样器。它只记录引擎指标，不写入游戏存档或领域事件。
    /// GPU 计时由 Unity Profiler/外部采样提供，不能用 CPU 时间冒充。
    /// </summary>
    public sealed class D16RuntimePerformanceProbe : MonoBehaviour
    {
        [Serializable]
        public sealed class Report
        {
            public string Scene;
            public string GraphicsDevice;
            public string GraphicsDeviceType;
            public string GraphicsDeviceVersion;
            public bool SupportsGpuRecorder;
            public bool FrameTimingFeatureEnabled;
            public int SampleCount;
            public float AverageFps;
            public float MinFps;
            public long PeakAllocatedBytes;
            public long PeakReservedBytes;
            public long PeakManagedBytes;
            public float AverageFrameMs;
            public float LongestFrameMs;
            public float LongestFrameAtSeconds;
            public int LongFrameCountOver50Ms;
            public float AverageMainThreadMs = -1f;
            public float GpuTimeMs = -1f;
            public bool GpuSampleAvailable;
            public string GpuCounterName;
            public int ManagedGcGen0Collections;
            public int ManagedGcGen1Collections;
            public int ManagedGcGen2Collections;
            public float D16ControllerAwakeMs;
            public float D16FirstRegistrationRefreshMs;
            public float D16FirstUpdateMs;
            public float D16FirstBudgetEvaluationMs;
            public float D5AwakeMs;
            public float D5RuntimeBuildElapsedMs;
            public bool D5RuntimePhaseTimingsComplete;
            public int D5CompletedRuntimePhaseCount;
            public float D5WaitingForPlayerMs;
            public float D5LoadingSnapshotMs;
            public float D5CreatingSystemsMs;
            public float D5InitializingStateMs;
            public float D5ApplyingCharacterVisualMs;
            public float D5BuildingMapMs;
            public float D2RuntimeBuildElapsedMs;
            public bool D2RuntimePhaseTimingsComplete;
            public int D2CompletedRuntimePhaseCount;
            public float D2EnsureShipMs;
            public float D2EnsureCrewAnchorMs;
            public float D2EnsurePlayerMs;
            public float D2ConfigurePhysicsMs;
            public float D2ConfigureControlsMs;
            public float D2ConfigurePresentationMs;
            public float D3RuntimeReadySeconds = -1f;
            public float D4RuntimeReadySeconds = -1f;
            public float D9RuntimeReadySeconds = -1f;
            public int ActiveAgentCount;
            public int ActiveAnimalCount;
            public int AudibleSourceCount;
            public int ProtectedOverBudgetCount;
            public bool BudgetSatisfied;
            public int LoadedCellCount;
            public int RegistrationCount;
            public int CellTransitionCount;
            public float LastCellTransitionMs;
            public float TotalCellTransitionMs;
            public int LoadingCellCount;
            public int LoadedPresentationObjectCount;
            public int PendingPresentationObjectCount;
            public float MaxPresentationLoadDurationMs;
            public int PresentationLoadCount;
            public int PoolRentCount;
            public int PoolReuseCount;
            public int PoolReturnCount;
            public int PoolPooledCount;
            public int CatalogIslandCellCount;
            public int ActiveIslandDataCellCount;
            public int DataOnlyIslandCellCount;
            public int VisualPresentationIslandCellCount;
            public string ReferenceIslandId;
        }

        [SerializeField] private float _sampleInterval = 0.25f;
        [SerializeField] private bool _writeReportOnDestroy = false;
        private Report _report;
        private float _nextSample;
        private double _fpsTotal;
        private double _frameTotalMs;
        private double _mainThreadTotalMs;
        private double _gpuTotalMs;
        private int _timingSampleCount;
        private int _gpuTimingSampleCount;
        private float _startedAt;
        private int _gcGen0AtStart;
        private int _gcGen1AtStart;
        private int _gcGen2AtStart;
        private readonly FrameTiming[] _frameTimings = new FrameTiming[1];
        private ProfilerRecorder _gpuFrameTimeRecorder;
        private bool _gpuFrameTimeRecorderStarted;
        private float _gpuRecorderStartAt;
        private D16WorldPerformanceController _controller;
        private D16PresentationPool _pool;
        private Component _d5;
        private Component _d2;
        private Component _d3;
        private Component _d4;
        private Component _d9;
        private static readonly string[] GpuCounterCandidates =
        {
            "GPU Frame Time",
            "GPU Time",
            "GPU Frame Time (ms)",
        };

        public Report Snapshot => _report;

        private void Awake()
        {
            _report = new Report
            {
                Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                GraphicsDeviceType = SystemInfo.graphicsDeviceType.ToString(),
                GraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
                SupportsGpuRecorder = SystemInfo.supportsGpuRecorder,
                FrameTimingFeatureEnabled = FrameTimingManager.IsFeatureEnabled(),
                MinFps = 0f,
                GpuCounterName = string.Empty,
            };
            _controller = FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
            _pool = FindFirstObjectByType<D16PresentationPool>(FindObjectsInactive.Include);
            _d5 = FindD5Bootstrap();
            _d2 = FindD2Bootstrap();
            _d3 = FindBootstrap("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            _d4 = FindBootstrap("Whalebone.Gameplay.D4.D4RuntimeBootstrap");
            _d9 = FindBootstrap("Whalebone.Gameplay.D9.D9VerticalSliceRuntimeBootstrap");
            _startedAt = Time.realtimeSinceStartup;
            _gcGen0AtStart = GC.CollectionCount(0);
            _gcGen1AtStart = GC.CollectionCount(1);
            _gcGen2AtStart = GC.CollectionCount(2);
            // 某些 Unity/驱动组合在首次创建 GPU counter 时会同步等待图形线程；
            // 延迟到场景进入稳定态，避免性能探针自己制造首帧长帧。
            _gpuRecorderStartAt = Time.unscaledTime + 1f;
            FrameTimingManager.CaptureFrameTimings();
        }

        private void Update()
        {
            if (!_gpuFrameTimeRecorderStarted && Time.unscaledTime >= _gpuRecorderStartAt)
                StartGpuFrameTimeRecorder();

            float delta = Time.unscaledDeltaTime;
            if (delta > 0.0001f && delta <= 5f)
            {
                float frameMs = delta * 1000f;
                if (frameMs > _report.LongestFrameMs)
                {
                    _report.LongestFrameMs = frameMs;
                    _report.LongestFrameAtSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _startedAt);
                }
                if (frameMs > 50f) _report.LongFrameCountOver50Ms++;
            }

            if (Time.unscaledTime < _nextSample) return;
            _nextSample = Time.unscaledTime + Mathf.Max(0.05f, _sampleInterval);
            if (delta <= 0.0001f || delta > 5f) return;
            float fps = 1f / delta;
            float mainThreadMs = delta * 1000f;
            _report.SampleCount++;
            _fpsTotal += fps;
            _frameTotalMs += mainThreadMs;
            _report.AverageFps = (float)(_fpsTotal / _report.SampleCount);
            _report.MinFps = _report.SampleCount == 1 ? fps : Mathf.Min(_report.MinFps, fps);
            _report.AverageFrameMs = (float)(_frameTotalMs / _report.SampleCount);
            _report.PeakAllocatedBytes = Math.Max(_report.PeakAllocatedBytes, Profiler.GetTotalAllocatedMemoryLong());
            _report.PeakReservedBytes = Math.Max(_report.PeakReservedBytes, Profiler.GetTotalReservedMemoryLong());
            _report.PeakManagedBytes = Math.Max(_report.PeakManagedBytes, GC.GetTotalMemory(false));
            _report.ManagedGcGen0Collections = Math.Max(0, GC.CollectionCount(0) - _gcGen0AtStart);
            _report.ManagedGcGen1Collections = Math.Max(0, GC.CollectionCount(1) - _gcGen1AtStart);
            _report.ManagedGcGen2Collections = Math.Max(0, GC.CollectionCount(2) - _gcGen2AtStart);
            CaptureFrameTiming();
            CaptureRuntimeBudget();
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
                _report.AverageMainThreadMs = (float)(_mainThreadTotalMs / ++_timingSampleCount);
            }
            if (timing.gpuFrameTime > 0f)
            {
                _gpuTotalMs += timing.gpuFrameTime;
                _report.GpuTimeMs = (float)(_gpuTotalMs / ++_gpuTimingSampleCount);
                _report.GpuSampleAvailable = true;
            }

            // FrameTimingManager 在部分 Unity/驱动组合下只返回 CPU 数据；
            // 用 ProfilerRecorder 的 GPU counter 作为同一运行时探针的后备，
            // 不能用 CPU 时间冒充 GPU 时间。两个来源只取一个，避免重复累计。
            if (!_report.GpuSampleAvailable)
                CaptureGpuFrameTimeRecorder();
        }

        private void StartGpuFrameTimeRecorder()
        {
            foreach (var counterName in GpuCounterCandidates)
            {
                try
                {
                    var recorder = ProfilerRecorder.StartNew(
                        ProfilerCategory.Render,
                        counterName,
                        15,
                        ProfilerRecorderOptions.GpuRecorder);
                    if (recorder.Valid)
                    {
                        _gpuFrameTimeRecorder = recorder;
                        _gpuFrameTimeRecorderStarted = true;
                        _report.GpuCounterName = counterName;
                        return;
                    }

                    recorder.Dispose();
                }
                catch (Exception)
                {
                    // 继续尝试下一个明确的 GPU counter；任何异常都不能回退到 CPU 时间。
                }
            }

            // 某些图形 API/驱动不提供这些 counter；报告保持 unavailable，
            // 由目标设备验收继续补齐，而不是写入估算值。
            _gpuFrameTimeRecorderStarted = false;
            _report.GpuCounterName = "unavailable";
        }

        private void CaptureGpuFrameTimeRecorder()
        {
            if (!_gpuFrameTimeRecorderStarted || !_gpuFrameTimeRecorder.Valid)
                return;
            if (_gpuFrameTimeRecorder.Count <= 0)
                return;

            double gpuNanoseconds = _gpuFrameTimeRecorder.LastValueAsDouble;
            if (gpuNanoseconds <= 0d)
                return;

            float gpuMilliseconds = (float)(gpuNanoseconds / 1_000_000d);
            if (gpuMilliseconds <= 0f || float.IsNaN(gpuMilliseconds) || float.IsInfinity(gpuMilliseconds))
                return;

            _gpuTotalMs += gpuMilliseconds;
            _report.GpuTimeMs = (float)(_gpuTotalMs / ++_gpuTimingSampleCount);
            _report.GpuSampleAvailable = true;
        }

        private void CaptureRuntimeBudget()
        {
            if (_controller != null)
            {
                var snapshot = _controller.LastSnapshot;
                if (snapshot != null)
                {
                    _report.ActiveAgentCount = snapshot.ActiveAgentCount;
                    _report.ActiveAnimalCount = snapshot.ActiveAnimalCount;
                    _report.AudibleSourceCount = snapshot.AudibleSourceCount;
                    _report.ProtectedOverBudgetCount = snapshot.ProtectedOverBudgetCount;
                    _report.BudgetSatisfied = snapshot.BudgetSatisfied;
                }
                _report.LoadedCellCount = _controller.LoadedCellCount;
                _report.RegistrationCount = _controller.RegistrationCount;
                _report.CellTransitionCount = _controller.CellTransitionCount;
                _report.LastCellTransitionMs = _controller.LastCellTransitionMs;
                _report.TotalCellTransitionMs = _controller.TotalCellTransitionMs;
                _report.LoadingCellCount = _controller.LoadingCellCount;
                _report.LoadedPresentationObjectCount = _controller.LoadedPresentationObjectCount;
                _report.PendingPresentationObjectCount = _controller.PendingPresentationObjectCount;
                _report.MaxPresentationLoadDurationMs = _controller.MaxPresentationLoadDurationMs;
                _report.PresentationLoadCount = _controller.PresentationLoadCount;
                _report.D16ControllerAwakeMs = _controller.StartupAwakeMs;
                _report.D16FirstRegistrationRefreshMs = _controller.FirstRegistrationRefreshMs;
                _report.D16FirstUpdateMs = _controller.FirstUpdateMs;
                _report.D16FirstBudgetEvaluationMs = _controller.FirstBudgetEvaluationMs;
            }
            if (_d5 == null) _d5 = FindD5Bootstrap();
            if (_d5 != null)
            {
                _report.D5AwakeMs = ReadD5Float("AwakeDurationMs", -1f);
                _report.D5RuntimeBuildElapsedMs = ReadD5Float("RuntimeBuildElapsedMs", -1f);
                _report.D5RuntimePhaseTimingsComplete = ReadD5Bool("RuntimePhaseTimingsComplete", false);
                _report.D5CompletedRuntimePhaseCount = ReadD5Int("CompletedRuntimePhaseCount", -1);
                _report.D5WaitingForPlayerMs = ReadD5Float("WaitingForPlayerMs", -1f);
                _report.D5LoadingSnapshotMs = ReadD5Float("LoadingSnapshotMs", -1f);
                _report.D5CreatingSystemsMs = ReadD5Float("CreatingSystemsMs", -1f);
                _report.D5InitializingStateMs = ReadD5Float("InitializingStateMs", -1f);
                _report.D5ApplyingCharacterVisualMs = ReadD5Float("ApplyingCharacterVisualMs", -1f);
                _report.D5BuildingMapMs = ReadD5Float("BuildingMapMs", -1f);
            }
            if (_d2 == null) _d2 = FindD2Bootstrap();
            if (_d2 != null)
            {
                _report.D2RuntimeBuildElapsedMs = ReadComponentFloat(_d2, "RuntimeBuildElapsedMs", -1f);
                _report.D2RuntimePhaseTimingsComplete = ReadComponentBool(_d2, "RuntimePhaseTimingsComplete", false);
                _report.D2CompletedRuntimePhaseCount = ReadComponentInt(_d2, "CompletedRuntimePhaseCount", -1);
                _report.D2EnsureShipMs = ReadComponentFloat(_d2, "EnsureShipMs", -1f);
                _report.D2EnsureCrewAnchorMs = ReadComponentFloat(_d2, "EnsureCrewAnchorMs", -1f);
                _report.D2EnsurePlayerMs = ReadComponentFloat(_d2, "EnsurePlayerMs", -1f);
                _report.D2ConfigurePhysicsMs = ReadComponentFloat(_d2, "ConfigurePhysicsMs", -1f);
                _report.D2ConfigureControlsMs = ReadComponentFloat(_d2, "ConfigureControlsMs", -1f);
                _report.D2ConfigurePresentationMs = ReadComponentFloat(_d2, "ConfigurePresentationMs", -1f);
            }
            CaptureBootstrapReadiness(ref _d3, "Whalebone.Gameplay.D3.D3RuntimeBootstrap", ref _report.D3RuntimeReadySeconds);
            CaptureBootstrapReadiness(ref _d4, "Whalebone.Gameplay.D4.D4RuntimeBootstrap", ref _report.D4RuntimeReadySeconds);
            CaptureBootstrapReadiness(ref _d9, "Whalebone.Gameplay.D9.D9VerticalSliceRuntimeBootstrap", ref _report.D9RuntimeReadySeconds);
            if (_pool == null) _pool = FindFirstObjectByType<D16PresentationPool>(FindObjectsInactive.Include);
            if (_pool != null)
            {
                _report.PoolRentCount = _pool.RentCount;
                _report.PoolReuseCount = _pool.ReuseCount;
                _report.PoolReturnCount = _pool.ReturnCount;
                _report.PoolPooledCount = _pool.PooledCount;
            }
            if (_controller != null)
            {
                _report.CatalogIslandCellCount = _controller.CatalogIslandCellCount;
                _report.ActiveIslandDataCellCount = _controller.ActiveIslandDataCellCount;
                _report.DataOnlyIslandCellCount = _controller.DataOnlyIslandCellCount;
                _report.VisualPresentationIslandCellCount = _controller.VisualPresentationIslandCellCount;
                _report.ReferenceIslandId = _controller.ReferenceIslandId;
            }
        }

        private static Component FindD5Bootstrap()
        {
            return FindBootstrap("Whalebone.Gameplay.D5.D5RuntimeBootstrap");
        }

        private static Component FindD2Bootstrap()
        {
            return FindBootstrap("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
        }

        private void CaptureBootstrapReadiness(ref Component source, string fullName, ref float readySeconds)
        {
            if (source == null) source = FindBootstrap(fullName);
            if (source == null || readySeconds >= 0f) return;
            if (ReadComponentBool(source, "IsReady", false))
                readySeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _startedAt);
        }

        private static Component FindBootstrap(string fullName)
        {
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                if (behaviour != null && behaviour.GetType().FullName == fullName)
                    return behaviour;
            }
            return null;
        }

        private float ReadD5Float(string propertyName, float fallback)
        {
            object value = ReadD5Property(propertyName);
            return value is float result ? result : fallback;
        }

        private int ReadD5Int(string propertyName, int fallback)
        {
            object value = ReadD5Property(propertyName);
            return value is int result ? result : fallback;
        }

        private bool ReadD5Bool(string propertyName, bool fallback)
        {
            object value = ReadD5Property(propertyName);
            return value is bool result ? result : fallback;
        }

        private object ReadD5Property(string propertyName)
        {
            return ReadComponentProperty(_d5, propertyName);
        }

        private static float ReadComponentFloat(Component source, string propertyName, float fallback)
        {
            object value = ReadComponentProperty(source, propertyName);
            return value is float result ? result : fallback;
        }

        private static int ReadComponentInt(Component source, string propertyName, int fallback)
        {
            object value = ReadComponentProperty(source, propertyName);
            return value is int result ? result : fallback;
        }

        private static bool ReadComponentBool(Component source, string propertyName, bool fallback)
        {
            object value = ReadComponentProperty(source, propertyName);
            return value is bool result ? result : fallback;
        }

        private static object ReadComponentProperty(Component source, string propertyName)
        {
            if (source == null) return null;
            try
            {
                var property = source.GetType().GetProperty(
                    propertyName, BindingFlags.Instance | BindingFlags.Public);
                return property == null ? null : property.GetValue(source, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string SaveReport(string filePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                filePath = Path.Combine(Application.persistentDataPath, "d16_performance_report.json");
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            var json = JsonUtility.ToJson(_report, true);
            File.WriteAllText(filePath, json);
            return filePath;
        }

        private void OnDestroy()
        {
            if (_gpuFrameTimeRecorderStarted)
            {
                _gpuFrameTimeRecorder.Dispose();
                _gpuFrameTimeRecorderStarted = false;
            }
            if (_writeReportOnDestroy && _report != null) SaveReport();
        }
    }
}
