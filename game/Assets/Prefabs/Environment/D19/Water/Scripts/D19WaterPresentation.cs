using System;
using UnityEngine;

namespace WhalesIslandReef.D19.Water
{
    public enum D19WaterQualityTier
    {
        Low,
        Medium,
        High,
    }

    [Serializable]
    public struct D19WaterFrameParameters
    {
        public float SeaLevelY;
        public Vector2 WindDirection;
        public float WindSpeed;
        public float WaveAmplitude;
        public float WaveLength;
        public float WaveSpeed;
        public float Transparency;
        public float Turbidity;
        public D19WaterQualityTier Quality;

        public static D19WaterFrameParameters Default
        {
            get
            {
                return new D19WaterFrameParameters
                {
                    SeaLevelY = 0f,
                    WindDirection = new Vector2(0.8f, 0.6f),
                    WindSpeed = 4f,
                    WaveAmplitude = 0.12f,
                    WaveLength = 18f,
                    WaveSpeed = 0.6f,
                    Transparency = 0.78f,
                    Turbidity = 0.22f,
                    Quality = D19WaterQualityTier.Medium,
                };
            }
        }
    }

    /// <summary>
    /// D19 water presentation owner. It accepts visual wind/wave parameters only;
    /// it never reads or writes WeatherSystem, ShipSystem, tide, navigation or save data.
    /// </summary>
    [ExecuteAlways]
    public sealed class D19WaterPresentation : MonoBehaviour
    {
        private static readonly int WaterLevelId = Shader.PropertyToID("_WaterLevel");
        private static readonly int WindDirectionId = Shader.PropertyToID("_WindDirection");
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");
        private static readonly int WaveAmplitudeId = Shader.PropertyToID("_WaveAmplitude");
        private static readonly int WaveLengthId = Shader.PropertyToID("_WaveLength");
        private static readonly int WaveSpeedId = Shader.PropertyToID("_WaveSpeed");
        private static readonly int WaterTransparencyId = Shader.PropertyToID("_WaterTransparency");
        private static readonly int TurbidityId = Shader.PropertyToID("_Turbidity");
        private static readonly int CameraYId = Shader.PropertyToID("_CameraY");

        [Header("Frozen D19 interface")]
        [SerializeField] private float seaLevelY;
        [SerializeField] private Vector2 windDirection = new Vector2(0.8f, 0.6f);
        [SerializeField] private float windSpeed = 4f;
        [SerializeField] private float waveAmplitude = 0.12f;
        [SerializeField] private float waveLength = 18f;
        [SerializeField] private float waveSpeed = 0.6f;
        [SerializeField, Range(0f, 1f)] private float transparency = 0.78f;
        [SerializeField, Range(0f, 1f)] private float turbidity = 0.22f;
        [SerializeField] private D19WaterQualityTier quality = D19WaterQualityTier.Medium;

        [Header("Owned renderers")]
        [SerializeField] private Renderer[] waterRenderers = new Renderer[0];
        [SerializeField] private Renderer[] foamRenderers = new Renderer[0];
        [SerializeField] private Renderer[] underwaterRenderers = new Renderer[0];

        private MaterialPropertyBlock _propertyBlock;

        public float SeaLevelY => seaLevelY;
        public D19WaterQualityTier Quality => quality;
        public Vector2 WindDirection => windDirection;
        public float WindSpeed => windSpeed;
        public float WaveAmplitude => waveAmplitude;
        public float Transparency => transparency;

        private void OnEnable()
        {
            NormalizeParameters();
            ApplyPresentation();
        }

        private void OnValidate()
        {
            NormalizeParameters();
            ApplyPresentation();
        }

        private void LateUpdate()
        {
            ApplyPresentation();
        }

        public void ApplyExternalFrame(D19WaterFrameParameters frame)
        {
            seaLevelY = SanitizeFinite(frame.SeaLevelY, 0f);
            windDirection = frame.WindDirection.sqrMagnitude < 0.0001f
                ? new Vector2(0f, 1f)
                : frame.WindDirection.normalized;
            windSpeed = Mathf.Max(0f, SanitizeFinite(frame.WindSpeed, 0f));
            waveAmplitude = Mathf.Clamp(SanitizeFinite(frame.WaveAmplitude, 0.12f), 0f, 2f);
            waveLength = Mathf.Clamp(SanitizeFinite(frame.WaveLength, 18f), 1f, 128f);
            waveSpeed = Mathf.Clamp(SanitizeFinite(frame.WaveSpeed, 0.6f), 0f, 8f);
            transparency = Mathf.Clamp01(SanitizeFinite(frame.Transparency, 0.78f));
            turbidity = Mathf.Clamp01(SanitizeFinite(frame.Turbidity, 0.22f));
            quality = frame.Quality;
            ApplyPresentation();
        }

        public void SetQuality(D19WaterQualityTier tier)
        {
            quality = tier;
            ApplyPresentation();
        }

        public bool IsSeaLevelConsistent(float worldY, float tolerance = 0.05f)
        {
            return Mathf.Abs(worldY - seaLevelY) <= Mathf.Max(0f, tolerance);
        }

        public void ConfigureOwnedRenderers(Renderer[] water, Renderer[] foam, Renderer[] underwater)
        {
            waterRenderers = water ?? new Renderer[0];
            foamRenderers = foam ?? new Renderer[0];
            underwaterRenderers = underwater ?? new Renderer[0];
            ApplyPresentation();
        }

        public D19WaterFrameParameters ReadFrameParameters()
        {
            return new D19WaterFrameParameters
            {
                SeaLevelY = seaLevelY,
                WindDirection = windDirection,
                WindSpeed = windSpeed,
                WaveAmplitude = waveAmplitude,
                WaveLength = waveLength,
                WaveSpeed = waveSpeed,
                Transparency = transparency,
                Turbidity = turbidity,
                Quality = quality,
            };
        }

        private void ApplyPresentation()
        {
            NormalizeParameters();
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
            Vector4 wind = new Vector4(windDirection.x, windDirection.y, 0f, 0f);
            float cameraY = Camera.main == null ? seaLevelY + 5f : Camera.main.transform.position.y;

            ApplyRendererParameters(waterRenderers, wind, cameraY);
            ApplyRendererParameters(foamRenderers, wind, cameraY);
            ApplyRendererParameters(underwaterRenderers, wind, cameraY);
            ApplyQualityKeywords(waterRenderers);
        }

        private void ApplyRendererParameters(Renderer[] renderers, Vector4 wind, float cameraY)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null) continue;
                _propertyBlock.Clear();
                _propertyBlock.SetFloat(WaterLevelId, seaLevelY);
                _propertyBlock.SetVector(WindDirectionId, wind);
                _propertyBlock.SetFloat(WindSpeedId, windSpeed);
                _propertyBlock.SetFloat(WaveAmplitudeId, waveAmplitude);
                _propertyBlock.SetFloat(WaveLengthId, waveLength);
                _propertyBlock.SetFloat(WaveSpeedId, waveSpeed);
                _propertyBlock.SetFloat(WaterTransparencyId, transparency);
                _propertyBlock.SetFloat(TurbidityId, turbidity);
                _propertyBlock.SetFloat(CameraYId, cameraY);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void ApplyQualityKeywords(Renderer[] renderers)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.sharedMaterials == null) continue;
                Material[] materials = renderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Material material = materials[m];
                    if (material == null || material.shader == null ||
                        material.shader.name != "WhalesIslandReef/D19/WaterSurface") continue;
                    material.DisableKeyword("_D19_WATER_LOW");
                    material.DisableKeyword("_D19_WATER_MEDIUM");
                    material.DisableKeyword("_D19_WATER_HIGH");
                    material.EnableKeyword(quality == D19WaterQualityTier.Low
                        ? "_D19_WATER_LOW"
                        : quality == D19WaterQualityTier.High
                            ? "_D19_WATER_HIGH"
                            : "_D19_WATER_MEDIUM");
                }
            }
        }

        private void NormalizeParameters()
        {
            seaLevelY = SanitizeFinite(seaLevelY, 0f);
            windDirection = windDirection.sqrMagnitude < 0.0001f
                ? new Vector2(0f, 1f)
                : windDirection.normalized;
            windSpeed = Mathf.Max(0f, SanitizeFinite(windSpeed, 4f));
            waveAmplitude = Mathf.Clamp(SanitizeFinite(waveAmplitude, 0.12f), 0f, 2f);
            waveLength = Mathf.Clamp(SanitizeFinite(waveLength, 18f), 1f, 128f);
            waveSpeed = Mathf.Clamp(SanitizeFinite(waveSpeed, 0.6f), 0f, 8f);
            transparency = Mathf.Clamp01(SanitizeFinite(transparency, 0.78f));
            turbidity = Mathf.Clamp01(SanitizeFinite(turbidity, 0.22f));
        }

        private static float SanitizeFinite(float value, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
        }
    }
}
