// D19-A05 lighting presentation model.
// This file contains presentation data and a read-only weather/time mapper.
// EnvironmentSystem and DayNightSystem remain the only domain writers.

using System;

namespace Whalebone.Game.D19
{
    public enum D19LightingQualityTier
    {
        Low,
        Medium,
        High,
    }

    [Serializable]
    public struct D19ColorDefinition
    {
        public float r;
        public float g;
        public float b;
        public float a;
    }

    [Serializable]
    public sealed class D19LightingQualityDefinition
    {
        public bool shadows;
        public bool fog;
        public bool post_processing;
        public int shadow_distance_m;
        public int shadow_cascades;
        public int local_light_limit;
    }

    [Serializable]
    public sealed class D19LightingProfileDefinition
    {
        public string profile_id;
        public string display_name_key;
        public string source_note;
        public string historical_tag;
        public string phase;
        public string owner_system;
        public string test_id;
        public D19ColorDefinition sky_color;
        public D19ColorDefinition equator_color;
        public D19ColorDefinition ground_color;
        public D19ColorDefinition fog_color;
        public D19ColorDefinition main_light_color;
        public D19ColorDefinition color_filter;
        public float main_light_intensity;
        public float main_light_shadow_strength;
        public float sun_rotation_x;
        public float sun_rotation_y;
        public float sun_rotation_z;
        public float ambient_intensity;
        public bool fog_enabled;
        public float fog_density;
        public float fog_start_m;
        public float fog_end_m;
        public float exposure_ev;
        public float contrast;
        public float saturation;
        public string tone_mapping;
        public bool depth_of_field;
        public bool motion_blur;
        public float flicker_hz;
        public bool local_lights_allowed;
        public string local_light_fixture;
        public string sky_reference_asset;
        public D19LightingQualityDefinition low;
        public D19LightingQualityDefinition medium;
        public D19LightingQualityDefinition high;
    }

    [Serializable]
    public sealed class D19LightingMappingDefinition
    {
        public string weather_state;
        public float time_start;
        public float time_end;
        public float max_visibility;
        public int priority;
        public string profile_id;
    }

    [Serializable]
    public sealed class D19LightingProfileCatalog
    {
        public int schema_version;
        public string task;
        public string owner_system;
        public string external_asset_policy;
        public D19LightingProfileDefinition[] profiles;
        public D19LightingMappingDefinition[] mappings;
    }

    /// <summary>Read-only projection supplied by D9/D2 presentation bridges.</summary>
    public sealed class D19LightingReadOnlyState
    {
        public string WeatherState;
        public float NormalizedTime;
        public float Visibility;

        public D19LightingReadOnlyState(string weatherState, float normalizedTime, float visibility)
        {
            WeatherState = string.IsNullOrWhiteSpace(weatherState) ? "Calm" : weatherState;
            NormalizedTime = NormalizeTime(normalizedTime);
            Visibility = Clamp01(visibility);
        }

        private static float NormalizeTime(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0.35f;
            value %= 1f;
            return value < 0f ? value + 1f : value;
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }
    }

    public static class D19LightingProfileSelector
    {
        public static string SelectProfile(D19LightingReadOnlyState state, D19LightingProfileCatalog catalog)
        {
            if (state == null || catalog == null || catalog.mappings == null) return "clear_day";

            D19LightingMappingDefinition best = null;
            for (int i = 0; i < catalog.mappings.Length; i++)
            {
                var mapping = catalog.mappings[i];
                if (mapping == null || !WeatherMatches(mapping.weather_state, state.WeatherState)) continue;
                if (!TimeInWindow(state.NormalizedTime, mapping.time_start, mapping.time_end)) continue;
                if (state.Visibility > mapping.max_visibility) continue;
                if (best == null || mapping.priority > best.priority) best = mapping;
            }
            return best == null || string.IsNullOrWhiteSpace(best.profile_id) ? "clear_day" : best.profile_id;
        }

        public static bool TimeInWindow(float time, float start, float end)
        {
            if (end - start >= 0.999999f) return true;
            float normalized = new D19LightingReadOnlyState("Calm", time, 1f).NormalizedTime;
            float a = new D19LightingReadOnlyState("Calm", start, 1f).NormalizedTime;
            float b = new D19LightingReadOnlyState("Calm", end, 1f).NormalizedTime;
            return a <= b ? normalized >= a && normalized <= b : normalized >= a || normalized <= b;
        }

        private static bool WeatherMatches(string mappingWeather, string weather)
        {
            return string.IsNullOrWhiteSpace(mappingWeather) || mappingWeather == "*" ||
                string.Equals(mappingWeather, weather, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static class D19LightingProfileValidator
    {
        private static readonly string[] RequiredProfiles =
            { "clear_day", "overcast_rain", "twilight_night", "fog", "storm" };

        public static string[] Validate(D19LightingProfileCatalog catalog)
        {
            var errors = new System.Collections.Generic.List<string>();
            if (catalog == null) return new[] { "catalog_null" };
            if (catalog.schema_version != 1) errors.Add("schema_version_must_be_1");
            if (!string.Equals(catalog.task, "D19-A05", StringComparison.Ordinal)) errors.Add("task_must_be_D19-A05");
            if (!string.Equals(catalog.owner_system, "D19-A05", StringComparison.Ordinal)) errors.Add("catalog_owner_system_invalid");
            if (string.IsNullOrWhiteSpace(catalog.external_asset_policy)) errors.Add("external_asset_policy_missing");
            if (catalog.profiles == null || catalog.profiles.Length != RequiredProfiles.Length)
                errors.Add("profile_count_must_be_5");
            else
            {
                for (int i = 0; i < RequiredProfiles.Length; i++)
                {
                    var profile = Find(catalog, RequiredProfiles[i]);
                    if (profile == null) errors.Add("missing_profile:" + RequiredProfiles[i]);
                    else ValidateProfile(profile, errors);
                }
            }

            if (catalog.mappings == null || catalog.mappings.Length < 5) errors.Add("mapping_count_too_small");
            else
            {
                for (int i = 0; i < catalog.mappings.Length; i++)
                {
                    var mapping = catalog.mappings[i];
                    if (mapping == null || Find(catalog, mapping.profile_id) == null)
                        errors.Add("mapping_profile_missing:" + i);
                    if (mapping != null)
                    {
                        if (!Finite(mapping.time_start) || !Finite(mapping.time_end) ||
                            mapping.time_start < 0f || mapping.time_start > 1f ||
                            mapping.time_end < 0f || mapping.time_end > 1f)
                            errors.Add("mapping_time_out_of_range:" + i);
                        if (!Finite(mapping.max_visibility) || mapping.max_visibility < 0f || mapping.max_visibility > 1f)
                            errors.Add("mapping_visibility_out_of_range:" + i);
                    }
                }
            }
            return errors.ToArray();
        }

        public static D19LightingProfileDefinition Find(D19LightingProfileCatalog catalog, string profileId)
        {
            if (catalog == null || catalog.profiles == null) return null;
            for (int i = 0; i < catalog.profiles.Length; i++)
                if (catalog.profiles[i] != null && catalog.profiles[i].profile_id == profileId)
                    return catalog.profiles[i];
            return null;
        }

        private static void ValidateProfile(D19LightingProfileDefinition profile, System.Collections.Generic.List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(profile.owner_system) || profile.owner_system != "D19-A05")
                errors.Add("owner_system_invalid:" + profile.profile_id);
            if (string.IsNullOrWhiteSpace(profile.source_note)) errors.Add("source_note_missing:" + profile.profile_id);
            if (string.IsNullOrWhiteSpace(profile.test_id)) errors.Add("test_id_missing:" + profile.profile_id);
            if (string.IsNullOrWhiteSpace(profile.sky_reference_asset)) errors.Add("sky_reference_missing:" + profile.profile_id);
            if (!Finite(profile.main_light_intensity) || profile.main_light_intensity < 0f || profile.main_light_intensity > 2f)
                errors.Add("main_light_intensity_out_of_range:" + profile.profile_id);
            if (!Finite(profile.main_light_shadow_strength) || profile.main_light_shadow_strength < 0f || profile.main_light_shadow_strength > 1f)
                errors.Add("shadow_strength_out_of_range:" + profile.profile_id);
            if (!Finite(profile.fog_density) || !Finite(profile.fog_start_m) || !Finite(profile.fog_end_m) ||
                profile.fog_density < 0f || profile.fog_density > 0.2f || profile.fog_start_m < 0f || profile.fog_end_m <= profile.fog_start_m)
                errors.Add("fog_range_invalid:" + profile.profile_id);
            if (!Finite(profile.exposure_ev) || profile.exposure_ev < -4f || profile.exposure_ev > 4f)
                errors.Add("exposure_out_of_range:" + profile.profile_id);
            if (!Finite(profile.contrast) || !Finite(profile.saturation) || profile.contrast < -100f || profile.contrast > 100f || profile.saturation < -100f || profile.saturation > 100f)
                errors.Add("color_adjustment_out_of_range:" + profile.profile_id);
            if (!Finite(profile.flicker_hz) || profile.depth_of_field || profile.motion_blur || profile.flicker_hz < 0f || profile.flicker_hz > 2f)
                errors.Add("unsafe_motion_effect:" + profile.profile_id);
            ValidateColor("sky", profile.profile_id, profile.sky_color, errors);
            ValidateColor("equator", profile.profile_id, profile.equator_color, errors);
            ValidateColor("ground", profile.profile_id, profile.ground_color, errors);
            ValidateColor("fog", profile.profile_id, profile.fog_color, errors);
            ValidateColor("main_light", profile.profile_id, profile.main_light_color, errors);
            ValidateColor("filter", profile.profile_id, profile.color_filter, errors);
            ValidateQuality(profile.profile_id, profile.low, errors);
            ValidateQuality(profile.profile_id, profile.medium, errors);
            ValidateQuality(profile.profile_id, profile.high, errors);
        }

        private static void ValidateQuality(string profileId, D19LightingQualityDefinition quality, System.Collections.Generic.List<string> errors)
        {
            if (quality == null) { errors.Add("quality_missing:" + profileId); return; }
            if (quality.shadow_distance_m < 0 || quality.shadow_distance_m > 128 || quality.shadow_cascades < 0 || quality.shadow_cascades > 4)
                errors.Add("quality_shadow_range_invalid:" + profileId);
            if (quality.local_light_limit < 0 || quality.local_light_limit > 8)
                errors.Add("quality_local_light_range_invalid:" + profileId);
        }

        private static void ValidateColor(string field, string profileId, D19ColorDefinition color, System.Collections.Generic.List<string> errors)
        {
            if (!Finite(color.r) || !Finite(color.g) || !Finite(color.b) || !Finite(color.a) ||
                color.r < 0f || color.r > 1f || color.g < 0f || color.g > 1f ||
                color.b < 0f || color.b > 1f || color.a < 0f || color.a > 1f)
                errors.Add("color_out_of_range:" + field + ":" + profileId);
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
