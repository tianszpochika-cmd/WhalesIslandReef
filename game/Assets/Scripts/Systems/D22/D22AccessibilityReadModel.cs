// ============================================================
// D22AccessibilityReadModel —— D22-L14 无障碍设置读模型（纯 C#）
//
// D16AccessibilitySettings 是表现偏好 owner 的公开契约。本文件只复制其
// 逻辑设置并报告稳定校验码，不保存 PlayerPrefs、不修改输入绑定或 UI 对象。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Whalebone.Systems.D16;

namespace Whalebone.Systems.D22
{
    /// <summary>无障碍设置的稳定校验码。</summary>
    public enum D22AccessibilityValidationCode
    {
        None = 0,
        MissingSettings = 1,
        InvalidSchemaVersion = 2,
        UnsupportedLocale = 3,
        TextScaleOutOfRange = 4,
        InvalidTextScale = 5,
        InvalidColorVisionMode = 6,
        InputBindingsMissing = 7,
        InputBindingInvalid = 8,
    }

    /// <summary>无障碍投影的校验结果。</summary>
    [Serializable]
    public sealed class D22AccessibilityValidationResult
    {
        public bool Valid;
        public List<string> Codes = new List<string>();

        /// <summary>返回防御性副本。</summary>
        public D22AccessibilityValidationResult Clone()
        {
            var result = new D22AccessibilityValidationResult { Valid = Valid };
            if (Codes != null) result.Codes.AddRange(Codes);
            return result;
        }
    }

    /// <summary>D16 表现偏好的 D22 防御性读模型。</summary>
    [Serializable]
    public sealed class D22AccessibilityReadModel
    {
        public const int CurrentSchemaVersion = 1;
        public const string SourceOwner = "D16AccessibilitySettingsStore";

        public int SchemaVersion = CurrentSchemaVersion;
        public bool IsValid;
        public string Locale = "zh-CN";
        public float TextScale = 1f;
        public bool HighContrast;
        public D16AccessibilityColorVisionMode ColorVisionMode;
        public bool ReducedMotion;
        public bool LowStimulus;
        public bool Subtitles = true;
        public bool SubtitleSpeakerLabels = true;
        public bool SensitiveWarnings = true;
        public string ReasonCode = string.Empty;
        public List<string> ValidationCodes = new List<string>();

        /// <summary>与 TextScale 等价的逻辑缩放访问。</summary>
        public float Zoom
        {
            get { return TextScale; }
        }

        /// <summary>从 D16 公开设置契约生成只读投影。</summary>
        public static D22AccessibilityReadModel Project(D16AccessibilitySettings settings)
        {
            var result = Copy(settings);
            D22AccessibilityValidationResult validation = Validate(settings);
            result.IsValid = validation.Valid;
            result.ValidationCodes = validation.Codes;
            result.ReasonCode = validation.Codes.Count == 0
                ? "d22.accessibility.valid"
                : validation.Codes[0];
            return result;
        }

        /// <summary>Project 的语义别名。</summary>
        public static D22AccessibilityReadModel FromSettings(D16AccessibilitySettings settings)
        {
            return Project(settings);
        }

        /// <summary>只校验设置，不修改 owner 设置实例。</summary>
        public static D22AccessibilityValidationResult Validate(D16AccessibilitySettings settings)
        {
            var result = new D22AccessibilityValidationResult { Valid = true };
            if (settings == null)
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.missing_settings");
                return result;
            }

            if (settings.Locale != "zh-CN" && settings.Locale != "en-US")
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.unsupported_locale");
            }
            if (float.IsNaN(settings.TextScale) || float.IsInfinity(settings.TextScale))
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.invalid_text_scale");
            }
            else if (settings.TextScale < 0.8f || settings.TextScale > 2f)
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.text_scale_out_of_range");
            }
            if (settings.ColorVisionMode < D16AccessibilityColorVisionMode.None ||
                settings.ColorVisionMode > D16AccessibilityColorVisionMode.Tritanopia)
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.invalid_color_vision_mode");
            }
            if (settings.InputBindings == null || settings.InputBindings.Count == 0)
            {
                result.Valid = false;
                result.Codes.Add("d22.accessibility.input_bindings_missing");
            }
            else
            {
                foreach (var binding in settings.InputBindings)
                {
                    if (binding == null)
                    {
                        result.Valid = false;
                        result.Codes.Add("d22.accessibility.input_binding_invalid");
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>返回深拷贝；投影不会暴露 D16 owner 的可变实例。</summary>
        public D22AccessibilityReadModel Clone()
        {
            var result = new D22AccessibilityReadModel
            {
                SchemaVersion = SchemaVersion,
                IsValid = IsValid,
                Locale = Locale ?? string.Empty,
                TextScale = TextScale,
                HighContrast = HighContrast,
                ColorVisionMode = ColorVisionMode,
                ReducedMotion = ReducedMotion,
                LowStimulus = LowStimulus,
                Subtitles = Subtitles,
                SubtitleSpeakerLabels = SubtitleSpeakerLabels,
                SensitiveWarnings = SensitiveWarnings,
                ReasonCode = ReasonCode ?? string.Empty,
            };
            if (ValidationCodes != null) result.ValidationCodes.AddRange(ValidationCodes);
            return result;
        }

        private static D22AccessibilityReadModel Copy(D16AccessibilitySettings settings)
        {
            if (settings == null)
            {
                return new D22AccessibilityReadModel
                {
                    IsValid = false,
                    ReasonCode = "d22.accessibility.missing_settings",
                };
            }
            return new D22AccessibilityReadModel
            {
                IsValid = true,
                Locale = settings.Locale ?? string.Empty,
                TextScale = settings.TextScale,
                HighContrast = settings.HighContrast,
                ColorVisionMode = settings.ColorVisionMode,
                ReducedMotion = settings.ReducedMotion,
                LowStimulus = settings.LowStimulus,
                Subtitles = settings.Subtitles,
                SubtitleSpeakerLabels = settings.SubtitleSpeakerLabels,
                SensitiveWarnings = settings.SensitiveWarnings,
            };
        }
    }

    /// <summary>显式 projector 入口；实际设置读写仍由 D16 owner 负责。</summary>
    public static class D22AccessibilityReadModelProjector
    {
        public const string OwnerSystem = "D22AccessibilityReadModel";

        /// <summary>生成 D22 无障碍读模型。</summary>
        public static D22AccessibilityReadModel Project(D16AccessibilitySettings settings)
        {
            return D22AccessibilityReadModel.Project(settings);
        }
    }
}
