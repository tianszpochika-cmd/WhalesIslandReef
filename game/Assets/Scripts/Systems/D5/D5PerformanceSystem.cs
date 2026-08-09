using System;

namespace Whalebone.Systems.D5
{
    /// <summary>目标分辨率、质量档位和帧率采样的唯一裁决者。</summary>
    public sealed class D5PerformanceSystem
    {
        private readonly D5PerformanceState _state;
        private double _fpsTotal;

        public D5PerformanceSystem(D5PerformanceState initial = null)
        {
            _state = initial == null ? new D5PerformanceState() : initial.Clone();
            _state.TargetWidth = 1920;
            _state.TargetHeight = 1080;
            _state.TargetFps = 60;
            _fpsTotal = _state.AverageFps * Math.Max(0, _state.SampleCount);
        }

        public D5PerformanceState Snapshot => _state.Clone();

        public void SetQuality(D5QualityLevel quality) { _state.Quality = quality; }

        public void RecordFrame(float deltaSeconds)
        {
            if (deltaSeconds <= 0.0001f || deltaSeconds > 5f) return;
            float fps = 1f / deltaSeconds;
            _state.LastFps = fps;
            _state.MinFps = _state.SampleCount == 0 ? fps : Math.Min(_state.MinFps, fps);
            _state.SampleCount++;
            _fpsTotal += fps;
            _state.AverageFps = (float)(_fpsTotal / _state.SampleCount);
            _state.MeetsTarget = _state.SampleCount > 0 && _state.MinFps >= _state.TargetFps;
        }

        public D5PerformanceReport Report()
        {
            bool meets = _state.SampleCount > 0 && _state.MinFps >= _state.TargetFps;
            _state.MeetsTarget = meets;
            return new D5PerformanceReport
            {
                MeetsTarget = meets,
                TargetWidth = _state.TargetWidth,
                TargetHeight = _state.TargetHeight,
                TargetFps = _state.TargetFps,
                LastFps = _state.LastFps,
                MinFps = _state.MinFps,
                AverageFps = _state.AverageFps,
                SampleCount = _state.SampleCount,
                Message = meets ? "当前采样达到 1080p/60 目标。" : "性能目标尚未由当前采样证明。",
            };
        }
    }
}
