// ============================================================
// DayNightController —— D2 昼夜表现适配器
//
// 领域时间由 Whalebone.Systems.D2.DayNightSystem 单一写入；本类只把时间
// 映射到光照、环境色和相机背景，不参与航行/好感/存档裁决。
// ============================================================

using UnityEngine;
using Whalebone.Systems.D2;

namespace Whalebone.Gameplay.World
{
    public sealed class DayNightController : MonoBehaviour
    {
        [SerializeField] private float secondsPerDay = 600f;
        [SerializeField] private Light sun;
        [SerializeField] private Camera targetCamera;

        private DayNightSystem _clock;
        private bool _externalPause;

        public DayNightSnapshot Snapshot => _clock == null
            ? new DayNightSnapshot(0, 0.25f, true)
            : _clock.Snapshot;

        private void Awake()
        {
            _clock = new DayNightSystem(secondsPerDay);
            EnsureSun();
            EnsureCamera();
            ApplyVisuals();
        }

        private void Update()
        {
            if (_clock == null) return;
            _clock.SetPaused(_externalPause || Time.timeScale <= 0f);
            _clock.Advance(Time.deltaTime);
            ApplyVisuals();
        }

        public void SetPaused(bool paused)
        {
            _externalPause = paused;
            if (_clock != null) _clock.SetPaused(paused);
        }

        public void ApplySnapshot(DayNightSnapshot snapshot)
        {
            if (_clock == null) _clock = new DayNightSystem(secondsPerDay);
            _clock.ApplySnapshot(snapshot);
            ApplyVisuals();
        }

        private void EnsureSun()
        {
            if (sun != null) return;
            sun = FindFirstObjectByType<Light>();
            if (sun != null) return;

            var go = new GameObject("D2_DirectionalSun");
            go.transform.SetParent(transform, false);
            sun = go.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
        }

        private void EnsureCamera()
        {
            if (targetCamera != null) return;
            targetCamera = Camera.main;
            if (targetCamera == null) targetCamera = FindFirstObjectByType<Camera>();
        }

        private void ApplyVisuals()
        {
            if (_clock == null) return;
            float time = _clock.Snapshot.NormalizedTime;
            float daylight = Mathf.Clamp01(Mathf.Sin(time * Mathf.PI * 2f - Mathf.PI * 0.5f) * 0.5f + 0.5f);
            daylight = Mathf.SmoothStep(0f, 1f, daylight);

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(time * 360f - 90f, -35f, 0f);
                sun.intensity = Mathf.Lerp(0.08f, 1.15f, daylight);
                sun.color = Color.Lerp(new Color(0.34f, 0.43f, 0.7f), new Color(1f, 0.92f, 0.78f), daylight);
            }

            RenderSettings.ambientLight = Color.Lerp(
                new Color(0.025f, 0.045f, 0.11f),
                new Color(0.72f, 0.82f, 0.95f),
                daylight);

            if (targetCamera != null)
                targetCamera.backgroundColor = Color.Lerp(
                    new Color(0.015f, 0.03f, 0.08f),
                    new Color(0.43f, 0.7f, 0.9f),
                    daylight);
        }
    }
}
