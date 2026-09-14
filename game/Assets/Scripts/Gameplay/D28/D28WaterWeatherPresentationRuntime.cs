using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D28
{
    /// <summary>
    /// D28 水面与天气表现层。
    /// Calm/Rain/Storm 是表现预览状态；真正的天气事实仍由 D21 owner 提供，
    /// 这里不推进时间、不扣补给、不改变航行风险，也不写存档。
    /// </summary>
    public sealed class D28WaterWeatherPresentationRuntime : MonoBehaviour
    {
        public enum PresentationWeather
        {
            Calm = 0,
            Rain = 1,
            Storm = 2,
        }

        private const string RootName = "D28WaterWeatherPresentation";
        private const string PreferredPresentationName = "D19_R03_HarborPresentation";

        private ParticleSystem _precipitation;
        private ParticleSystemRenderer _precipitationRenderer;
        private PresentationWeather _weather;
        private bool _applied;
        private bool _started;
        private float _nextRetry;
        private static float _surfaceWaveMultiplier = 1f;

        public bool IsReady => _started;

        public static PresentationWeather CurrentWeather
        {
            get
            {
                var instance = FindFirstObjectByType<D28WaterWeatherPresentationRuntime>(FindObjectsInactive.Include);
                return instance == null ? PresentationWeather.Calm : instance._weather;
            }
        }

        public static float SurfaceWaveMultiplier => _surfaceWaveMultiplier;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D28WaterWeatherPresentationRuntime>(FindObjectsInactive.Include) != null)
                return;
            new GameObject(RootName).AddComponent<D28WaterWeatherPresentationRuntime>();
        }

        private void Start()
        {
            BuildPrecipitation();
            ApplyIfReady();
            SetWeather(PresentationWeather.Calm);
            _started = true;
        }

        private void Update()
        {
            AnchorPrecipitationToCamera();
            if (!_applied && Time.unscaledTime >= _nextRetry)
            {
                _nextRetry = Time.unscaledTime + 0.5f;
                ApplyIfReady();
            }
        }

        public static bool TryCycleWeather()
        {
            var instance = FindFirstObjectByType<D28WaterWeatherPresentationRuntime>(FindObjectsInactive.Include);
            if (instance == null || !instance.IsReady) return false;
            var next = (PresentationWeather)(((int)instance._weather + 1) % 3);
            instance.SetWeather(next);
            return true;
        }

        /// <summary>兼容旧键盘/测试入口；需要结果的调用方使用 TryCycleWeather。</summary>
        public static void CycleWeather()
        {
            TryCycleWeather();
        }

        public static void SetWeatherPreview(string value)
        {
            var instance = FindFirstObjectByType<D28WaterWeatherPresentationRuntime>(FindObjectsInactive.Include);
            if (instance == null) return;
            PresentationWeather next;
            if (!Enum.TryParse(value, true, out next)) next = PresentationWeather.Calm;
            instance.SetWeather(next);
        }

        public void SetWeather(PresentationWeather weather)
        {
            _weather = weather;
            float fogEnd = 280f;
            float rainRate = 0f;
            float rainSpeed = 0f;
            float windX = 0f;
            float windZ = 0f;
            var precipitationColor = new Color(0.78f, 0.90f, 0.96f, 0.78f);
            Color fogColor = new Color(0.43f, 0.64f, 0.62f, 1f);
            Color ambient = new Color(0.50f, 0.58f, 0.51f, 1f);
            _surfaceWaveMultiplier = 1f;
            switch (weather)
            {
                case PresentationWeather.Rain:
                    fogEnd = 190f;
                    rainRate = 560f;
                    rainSpeed = 18f;
                    windX = -2.8f;
                    windZ = -1.2f;
                    _surfaceWaveMultiplier = 1.40f;
                    precipitationColor = new Color(0.72f, 0.86f, 0.94f, 0.50f);
                    fogColor = new Color(0.32f, 0.43f, 0.46f, 1f);
                    ambient = new Color(0.40f, 0.47f, 0.48f, 1f);
                    break;
                case PresentationWeather.Storm:
                    fogEnd = 120f;
                    rainRate = 980f;
                    rainSpeed = 24f;
                    windX = -7.5f;
                    windZ = -3.2f;
                    _surfaceWaveMultiplier = 2.35f;
                    precipitationColor = new Color(0.62f, 0.78f, 0.88f, 0.66f);
                    fogColor = new Color(0.16f, 0.23f, 0.28f, 1f);
                    ambient = new Color(0.28f, 0.33f, 0.36f, 1f);
                    break;
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = Mathf.Min(58f, fogEnd * 0.28f);
            RenderSettings.fogEndDistance = fogEnd;
            RenderSettings.ambientLight = ambient;
            if (_precipitation != null)
            {
                var main = _precipitation.main;
                main.startColor = precipitationColor;
                main.startSpeed = new ParticleSystem.MinMaxCurve(rainSpeed);
                var emission = _precipitation.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(rainRate);
                var velocity = _precipitation.velocityOverLifetime;
                velocity.x = new ParticleSystem.MinMaxCurve(windX);
                velocity.y = new ParticleSystem.MinMaxCurve(-rainSpeed);
                velocity.z = new ParticleSystem.MinMaxCurve(windZ);
                if (rainRate <= 0f) _precipitation.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                else if (!_precipitation.isPlaying) _precipitation.Play();
            }
            if (_precipitationRenderer != null && _precipitationRenderer.sharedMaterial != null)
            {
                var material = _precipitationRenderer.sharedMaterial;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", precipitationColor);
                if (material.HasProperty("_Color")) material.SetColor("_Color", precipitationColor);
            }
            Debug.Log("[D28/WeatherPresentation] 表现预览=" + weather + "; domain weather remains owner-controlled");
        }

        private void AnchorPrecipitationToCamera()
        {
            if (_precipitation == null) return;
            var camera = Camera.main;
            if (camera == null) return;

            // The weather layer is a presentation volume, not a world-state
            // object. Keep it above the active view so fixed evidence shots
            // and player camera movement both receive readable rain.
            _precipitation.transform.position = camera.transform.position + Vector3.up * 16f;
        }

        private void ApplyIfReady()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            var presentation = FindSceneObject(PreferredPresentationName);
            if (presentation == null) return;

            var renderers = presentation.GetComponentsInChildren<Renderer>(true);
            var animatedWater = 0;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null) continue;
                string normalized = renderer.name.ToLowerInvariant();
                if (normalized.IndexOf("water", StringComparison.Ordinal) < 0 &&
                    normalized.IndexOf("foam", StringComparison.Ordinal) < 0)
                    continue;
                renderer.enabled = true;
                if (renderer.GetComponent<D28WaterMotion>() == null)
                {
                    renderer.gameObject.AddComponent<D28WaterMotion>();
                    animatedWater++;
                }
            }
            _applied = true;
            Debug.Log("[D28/Water] 水面/泡沫表现已接线：renderers=" + renderers.Length +
                ", animatedSurfaces=" + animatedWater);
        }

        private void BuildPrecipitation()
        {
            var precipitationObject = new GameObject("D28Precipitation");
            precipitationObject.transform.SetParent(transform, false);
            precipitationObject.transform.localPosition = Vector3.up * 16f;
            _precipitation = precipitationObject.AddComponent<ParticleSystem>();

            var main = _precipitation.main;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = 1400;
            main.startLifetime = 1.10f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.018f);
            main.startColor = new Color(0.78f, 0.90f, 0.96f, 0.50f);

            var emission = _precipitation.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(0f);
            var shape = _precipitation.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(86f, 1f, 86f);
            var velocity = _precipitation.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);

            _precipitationRenderer = precipitationObject.GetComponent<ParticleSystemRenderer>();
            _precipitationRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            _precipitationRenderer.lengthScale = 0.85f;
            _precipitationRenderer.velocityScale = 0.45f;
            _precipitationRenderer.cameraVelocityScale = 0.2f;
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Particles/Standard Unlit");
            if (shader != null)
            {
                var material = new Material(shader);
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
                if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", main.startColor.color);
                if (material.HasProperty("_Color")) material.SetColor("_Color", main.startColor.color);
                material.renderQueue = 3000;
                _precipitationRenderer.sharedMaterial = material;
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null) continue;
                if (root.name == name) return root;
                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                    if (transforms[j] != null && transforms[j].name == name) return transforms[j].gameObject;
            }
            return null;
        }
    }

    /// <summary>不依赖天气事实的轻量水面垂向波动。</summary>
    public sealed class D28WaterMotion : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.025f;
        [SerializeField] private float frequency = 0.8f;
        private Vector3 _basePosition;

        private void Awake()
        {
            _basePosition = transform.localPosition;
        }

        private void Update()
        {
            transform.localPosition = _basePosition + Vector3.up *
                (Mathf.Sin(Time.unscaledTime * frequency + GetInstanceID() * 0.013f) * amplitude *
                    D28WaterWeatherPresentationRuntime.SurfaceWaveMultiplier);
        }

    }
}
