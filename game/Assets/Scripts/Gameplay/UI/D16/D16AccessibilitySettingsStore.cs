using System;
using Whalebone.Systems.D16;
using UnityEngine;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A03 的 UI 设置持久化入口。只写表现层偏好，不写存档或领域状态。
    /// </summary>
    public static class D16AccessibilitySettingsStore
    {
        public const string LocaleKey = "wb_a11y_locale";
        public const string TextScaleKey = "wb_a11y_text_scale";
        public const string HighContrastKey = "wb_a11y_high_contrast";
        public const string ColorVisionKey = "wb_a11y_color_vision";
        public const string ReducedMotionKey = "wb_a11y_reduced_motion";
        public const string LowStimulusKey = "wb_a11y_low_stimulus";
        public const string SubtitlesKey = "wb_a11y_subtitles";
        public const string SubtitleSpeakerKey = "wb_a11y_subtitle_speaker";
        public const string SensitiveWarningsKey = "wb_a11y_sensitive_warnings";
        private const string BindingPrefix = "wb_a11y_binding_";

        private static D16AccessibilitySettings _cached;

        public static event Action<D16AccessibilitySettings> Changed;

        public static D16AccessibilitySettings Current => Load();

        public static D16AccessibilitySettings Load()
        {
            if (_cached != null)
            {
                // wb_lang 是主菜单/Localization 的语言所有者。无障碍设置没有独立
                // 的语言控件，不能因为缓存了旧 Locale 就在下一次保存时覆盖用户刚选的语言。
                _cached.Locale = CurrentUiLocale();
                return _cached;
            }

            var settings = D16AccessibilityDefaults.Create();
            settings.Locale = CurrentUiLocale();
            settings.TextScale = PlayerPrefs.GetFloat(TextScaleKey, settings.TextScale);
            settings.HighContrast = GetBool(HighContrastKey, settings.HighContrast);
            settings.ColorVisionMode = (D16AccessibilityColorVisionMode)PlayerPrefs.GetInt(
                ColorVisionKey, (int)settings.ColorVisionMode);
            settings.ReducedMotion = GetBool(ReducedMotionKey, settings.ReducedMotion);
            settings.LowStimulus = GetBool(LowStimulusKey, settings.LowStimulus);
            settings.Subtitles = GetBool(SubtitlesKey, settings.Subtitles);
            settings.SubtitleSpeakerLabels = GetBool(SubtitleSpeakerKey, settings.SubtitleSpeakerLabels);
            settings.SensitiveWarnings = GetBool(SensitiveWarningsKey, settings.SensitiveWarnings);

            foreach (var binding in settings.InputBindings)
            {
                var prefix = BindingPrefix + binding.Action + "_";
                binding.PrimaryKeyboard = PlayerPrefs.GetString(prefix + "primary", binding.PrimaryKeyboard);
                binding.SecondaryKeyboard = PlayerPrefs.GetString(prefix + "secondary", binding.SecondaryKeyboard);
                binding.GamepadButton = PlayerPrefs.GetString(prefix + "gamepad", binding.GamepadButton);
            }

            settings.Normalize();
            _cached = settings;
            return _cached;
        }

        public static void Save(D16AccessibilitySettings value)
        {
            var settings = value == null ? D16AccessibilityDefaults.Create() : value.Clone();
            settings.Normalize();

            // 语言由现有 SettingsStore/Localization 统一拥有。本面板没有独立的语言
            // 控件，因此保存文字大小、字幕或重绑定时必须读取实时语言，不能把调用方
            // 手里的旧快照写回 wb_lang，避免“改无障碍选项后语言被切回去”。
            settings.Locale = CurrentUiLocale();
            PlayerPrefs.SetString(LocaleKey, settings.Locale);
            PlayerPrefs.SetFloat(TextScaleKey, settings.TextScale);
            SetBool(HighContrastKey, settings.HighContrast);
            PlayerPrefs.SetInt(ColorVisionKey, (int)settings.ColorVisionMode);
            SetBool(ReducedMotionKey, settings.ReducedMotion);
            SetBool(LowStimulusKey, settings.LowStimulus);
            SetBool(SubtitlesKey, settings.Subtitles);
            SetBool(SubtitleSpeakerKey, settings.SubtitleSpeakerLabels);
            SetBool(SensitiveWarningsKey, settings.SensitiveWarnings);

            foreach (var binding in settings.InputBindings)
            {
                var prefix = BindingPrefix + binding.Action + "_";
                PlayerPrefs.SetString(prefix + "primary", binding.PrimaryKeyboard ?? string.Empty);
                PlayerPrefs.SetString(prefix + "secondary", binding.SecondaryKeyboard ?? string.Empty);
                PlayerPrefs.SetString(prefix + "gamepad", binding.GamepadButton ?? string.Empty);
            }

            PlayerPrefs.Save();
            _cached = settings;
            Changed?.Invoke(_cached.Clone());
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(LocaleKey);
            PlayerPrefs.DeleteKey(TextScaleKey);
            PlayerPrefs.DeleteKey(HighContrastKey);
            PlayerPrefs.DeleteKey(ColorVisionKey);
            PlayerPrefs.DeleteKey(ReducedMotionKey);
            PlayerPrefs.DeleteKey(LowStimulusKey);
            PlayerPrefs.DeleteKey(SubtitlesKey);
            PlayerPrefs.DeleteKey(SubtitleSpeakerKey);
            PlayerPrefs.DeleteKey(SensitiveWarningsKey);

            foreach (var binding in D16AccessibilityDefaults.CreateBindings())
            {
                var prefix = BindingPrefix + binding.Action + "_";
                PlayerPrefs.DeleteKey(prefix + "primary");
                PlayerPrefs.DeleteKey(prefix + "secondary");
                PlayerPrefs.DeleteKey(prefix + "gamepad");
            }

            PlayerPrefs.Save();
            _cached = D16AccessibilityDefaults.Create();
            Changed?.Invoke(_cached.Clone());
        }

        public static D16InputBinding GetBinding(D16InputAction action)
        {
            var settings = Load();
            foreach (var binding in settings.InputBindings)
                if (binding.Action == action) return binding;
            return null;
        }

        private static bool GetBool(string key, bool fallback)
        {
            return PlayerPrefs.GetInt(key, fallback ? 1 : 0) != 0;
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        private static string CurrentUiLocale()
        {
            return SettingsStore.Lang == "en" ? "en-US" : "zh-CN";
        }
    }
}
