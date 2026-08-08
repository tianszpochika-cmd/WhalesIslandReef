// ============================================================
// SettingsStore —— 设置持久化（PlayerPrefs）
//
// 归属：Gameplay/UI（表现层设置，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 读写音量（主/音乐/音效）、画质档位、语言，键名固定 wb_*
//   2. 启动时读取并应用到 AudioListener / QualitySettings / UI
//   3. 提供事件回调（语言/音量变化时 UI 刷新）
//
// 键名（固定契约，勿改）：
//   wb_volume_master / wb_volume_music / wb_volume_sfx（0~1 float）
//   wb_quality（0=低 1=中 2=高 int）
//   wb_lang（"zh" / "en" string）
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.UI
{
    /// <summary>
    /// 设置存储与应用的静态入口。
    /// </summary>
    public static class SettingsStore
    {
        // ---- 键名（P1A 契约，禁止改动）----
        public const string KeyMaster = "wb_volume_master";
        public const string KeyMusic  = "wb_volume_music";
        public const string KeySfx    = "wb_volume_sfx";
        public const string KeyQuality = "wb_quality";
        public const string KeyLang    = "wb_lang";

        // ---- 读取 ----
        public static float MasterVolume
        {
            get { return PlayerPrefs.GetFloat(KeyMaster, 1f); }
            set { PlayerPrefs.SetFloat(KeyMaster, Mathf.Clamp01(value)); }
        }

        public static float MusicVolume
        {
            get { return PlayerPrefs.GetFloat(KeyMusic, 0.8f); }
            set { PlayerPrefs.SetFloat(KeyMusic, Mathf.Clamp01(value)); }
        }

        public static float SfxVolume
        {
            get { return PlayerPrefs.GetFloat(KeySfx, 0.8f); }
            set { PlayerPrefs.SetFloat(KeySfx, Mathf.Clamp01(value)); }
        }

        /// <summary>画质档位 0/1/2 → 低/中/高。</summary>
        public static int QualityLevel
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(KeyQuality, 1), 0, 2); }
            set { PlayerPrefs.SetInt(KeyQuality, Mathf.Clamp(value, 0, 2)); }
        }

        /// <summary>语言 "zh"/"en"。</summary>
        public static string Lang
        {
            get { return PlayerPrefs.GetString(KeyLang, "zh"); }
            set { PlayerPrefs.SetString(KeyLang, value == "en" ? "en" : "zh"); }
        }

        // ---- 应用 ----
        /// <summary>把已存的设置应用到引擎（启动时调用一次）。</summary>
        public static void ApplyAll()
        {
            // 主音量 → AudioListener（P1A：音乐/音效尚无独立 AudioSource，先统一映射）
            AudioListener.volume = MasterVolume;
            // 画质
            QualitySettings.SetQualityLevel(QualityLevel, true);
        }
    }
}
