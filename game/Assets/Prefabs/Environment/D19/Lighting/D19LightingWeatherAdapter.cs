// D19-A05 presentation-only bridge.
// It consumes copies of D9 weather and D2 time snapshots and never writes them.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Whalebone.Game.D19;
using Whalebone.Systems.D9;

namespace Whalebone.Game.D19Presentation
{
    public sealed class D19LightingWeatherAdapter : MonoBehaviour
    {
        [SerializeField] private TextAsset profileCatalogJson;
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Volume globalVolume;
        [SerializeField] private Light[] localOilLampInterfaces = new Light[0];
        [SerializeField] private D19LightingQualityTier qualityTier = D19LightingQualityTier.Medium;
        [SerializeField] private bool createRuntimeVolumeIfMissing = true;

        private D19LightingProfileCatalog _catalog;
        private D19LightingReadOnlyState _lastState;
        private D19LightingProfileDefinition _lastProfile;

        public string CurrentProfileId { get { return _lastProfile == null ? string.Empty : _lastProfile.profile_id; } }
        public D19LightingQualityTier QualityTier { get { return qualityTier; } }
        public bool IsReadOnlyAdapter { get { return true; } }

        private void Awake()
        {
            LoadCatalog();
            EnsureTargets();
            if (_catalog != null)
                ApplyReadOnlyState(new D19LightingReadOnlyState("Calm", 0.35f, 0.95f));
        }

        public void ApplyReadOnly(D9WeatherSeaSnapshot weather, float normalizedTime)
        {
            var safe = weather == null ? D9WeatherSeaSnapshot.CreateDefault() : weather.Clone();
            ApplyReadOnlyState(new D19LightingReadOnlyState(safe.Weather.ToString(), normalizedTime, safe.Visibility));
        }

        public void ApplyReadOnlyState(D19LightingReadOnlyState state)
        {
            if (_catalog == null || state == null) return;
            _lastState = state;
            string profileId = D19LightingProfileSelector.SelectProfile(state, _catalog);
            _lastProfile = D19LightingProfileValidator.Find(_catalog, profileId) ??
                D19LightingProfileValidator.Find(_catalog, "clear_day");
            if (_lastProfile == null) return;
            ApplyProfile(_lastProfile, QualityDefinition(_lastProfile));
        }

        public void SetQualityTier(D19LightingQualityTier requestedTier)
        {
            qualityTier = requestedTier;
            if (_lastState != null) ApplyReadOnlyState(_lastState);
        }

        private void LoadCatalog()
        {
            if (profileCatalogJson == null || string.IsNullOrWhiteSpace(profileCatalogJson.text)) return;
            _catalog = JsonUtility.FromJson<D19LightingProfileCatalog>(profileCatalogJson.text);
            if (D19LightingProfileValidator.Validate(_catalog).Length != 0) _catalog = null;
        }

        private void EnsureTargets()
        {
            if (mainDirectionalLight == null)
            {
                var lights = GetComponentsInChildren<Light>(true);
                for (int i = 0; i < lights.Length; i++)
                    if (lights[i].type == LightType.Directional) { mainDirectionalLight = lights[i]; break; }
            }
            if (targetCamera == null) targetCamera = Camera.main;
            if (localOilLampInterfaces == null || localOilLampInterfaces.Length == 0)
            {
                var lights = GetComponentsInChildren<Light>(true);
                var found = new System.Collections.Generic.List<Light>();
                for (int i = 0; i < lights.Length; i++)
                    if (lights[i] != mainDirectionalLight && lights[i].type == LightType.Point && lights[i].name.Contains("OilLamp"))
                        found.Add(lights[i]);
                localOilLampInterfaces = found.ToArray();
            }
            if (globalVolume == null && createRuntimeVolumeIfMissing)
            {
                var volumeObject = new GameObject("D19_Runtime_GlobalVolume");
                volumeObject.transform.SetParent(transform, false);
                globalVolume = volumeObject.AddComponent<Volume>();
                globalVolume.isGlobal = true;
                globalVolume.priority = 100f;
                globalVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            }
        }

        private D19LightingQualityDefinition QualityDefinition(D19LightingProfileDefinition profile)
        {
            switch (qualityTier)
            {
                case D19LightingQualityTier.Low: return profile.low;
                case D19LightingQualityTier.High: return profile.high;
                default: return profile.medium;
            }
        }

        private void ApplyProfile(D19LightingProfileDefinition profile, D19LightingQualityDefinition quality)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.transform.rotation = Quaternion.Euler(profile.sun_rotation_x, profile.sun_rotation_y, profile.sun_rotation_z);
                mainDirectionalLight.color = ToColor(profile.main_light_color);
                mainDirectionalLight.intensity = profile.main_light_intensity;
                mainDirectionalLight.shadowStrength = profile.main_light_shadow_strength;
                mainDirectionalLight.shadows = quality.shadows ? LightShadows.Soft : LightShadows.None;
            }

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ToColor(profile.sky_color);
            RenderSettings.ambientEquatorColor = ToColor(profile.equator_color);
            RenderSettings.ambientGroundColor = ToColor(profile.ground_color);
            RenderSettings.ambientIntensity = profile.ambient_intensity;
            RenderSettings.fog = profile.fog_enabled && quality.fog;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = ToColor(profile.fog_color);
            RenderSettings.fogDensity = profile.fog_density;
            RenderSettings.fogStartDistance = profile.fog_start_m;
            RenderSettings.fogEndDistance = profile.fog_end_m;
            if (targetCamera != null) targetCamera.backgroundColor = ToColor(profile.sky_color);

            ApplyVolume(profile, quality);
            ApplyLocalOilLampInterfaces(profile, quality);
        }

        private void ApplyVolume(D19LightingProfileDefinition profile, D19LightingQualityDefinition quality)
        {
            if (globalVolume == null || globalVolume.profile == null) return;
            var tone = GetOrAdd<Tonemapping>();
            tone.active = quality.post_processing;
            tone.mode.Override(profile.tone_mapping == "Neutral" ? TonemappingMode.Neutral : TonemappingMode.ACES);
            var color = GetOrAdd<ColorAdjustments>();
            color.active = quality.post_processing;
            color.postExposure.Override(profile.exposure_ev);
            color.contrast.Override(profile.contrast);
            color.saturation.Override(profile.saturation);
            color.colorFilter.Override(ToColor(profile.color_filter));
        }

        private T GetOrAdd<T>() where T : VolumeComponent
        {
            T component;
            if (!globalVolume.profile.TryGet(out component)) component = globalVolume.profile.Add<T>();
            return component;
        }

        private void ApplyLocalOilLampInterfaces(D19LightingProfileDefinition profile, D19LightingQualityDefinition quality)
        {
            int limit = profile.local_lights_allowed && quality.local_light_limit > 0 ? quality.local_light_limit : 0;
            for (int i = 0; i < localOilLampInterfaces.Length; i++)
            {
                var lamp = localOilLampInterfaces[i];
                if (lamp == null) continue;
                bool enabled = i < limit;
                lamp.enabled = enabled;
                lamp.color = ToColor(profile.main_light_color);
                lamp.range = 6f;
                lamp.intensity = enabled ? 1.0f : 0f;
                lamp.shadows = LightShadows.None;
            }
        }

        private static Color ToColor(D19ColorDefinition color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }
    }
}
