// ============================================================
// DayNightSystem —— D2 可存档昼夜时钟（纯 C#）
// ============================================================

using System;

namespace Whalebone.Systems.D2
{
    public sealed class DayNightSystem
    {
        private readonly float _secondsPerDay;
        private DayNightSnapshot _state;

        public DayNightSystem(float secondsPerDay = 600f, DayNightSnapshot? initial = null)
        {
            _secondsPerDay = IsPositive(secondsPerDay) ? secondsPerDay : 600f;
            _state = (initial ?? new DayNightSnapshot(0, 0.25f)).Normalized();
        }

        public DayNightSnapshot Snapshot => _state.Normalized();

        public void Advance(float deltaTime)
        {
            if (_state.Paused) return;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return;

            float next = _state.NormalizedTime + deltaTime / _secondsPerDay;
            int elapsedDays = (int)Math.Floor(next);
            if (elapsedDays > 0) _state.DayIndex += elapsedDays;
            _state.NormalizedTime = next - elapsedDays;
            _state = _state.Normalized();
        }

        public void SetPaused(bool paused)
        {
            _state.Paused = paused;
        }

        public void Reset(int dayIndex = 0, float normalizedTime = 0.25f)
        {
            _state = new DayNightSnapshot(dayIndex, normalizedTime, false);
        }

        public void ApplySnapshot(DayNightSnapshot snapshot)
        {
            _state = snapshot.Normalized();
        }

        private static bool IsPositive(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        }
    }
}
