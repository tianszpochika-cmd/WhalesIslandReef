// ============================================================
// Localization —— 静态文本表（中英双语，为本地化做准备）
//
// 归属：Gameplay/UI（表现层文案，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 静态中英两套字典（key → 文案）
//   2. 按 PlayerPrefs 键 wb_lang 取当前语言（"zh" / "en"）
//   3. Get(key) 统一取文案接口；切换语言后 UI 自行刷新
//
// 说明：
//   - 纯静态、无 Unity 依赖以外的引用，可单测
//   - 新文案先在此登记，禁止散落在各处写死（单一事实源）
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.UI
{
    /// <summary>文本表（P1A 阶段：中英硬编码字典，后续可换资源文件）。</summary>
    public static class Localization
    {
        /// <summary>PlayerPrefs 键：语言。</summary>
        public const string LangPrefsKey = "wb_lang";

        // 中文表
        private static readonly System.Collections.Generic.Dictionary<string, string> Zh =
            new System.Collections.Generic.Dictionary<string, string>
            {
                // 主菜单
                {"title", "鲸骨航线"},
                {"menu_new_game", "新游戏"},
                {"menu_continue", "继续"},
                {"menu_settings", "设置"},
                {"menu_about", "关于"},
                {"menu_no_save", "没有存档"},
                {"btn_back", "返回"},

                // 新游戏
                {"newgame_title", "创建游戏"},
                {"newgame_captain_name", "船长名"},
                {"newgame_placeholder_name", "输入船长名…"},
                {"newgame_confirm", "确认"},
                {"newgame_cancel", "取消"},

                // 设置
                {"settings_title", "设置"},
                {"settings_volume_master", "主音量"},
                {"settings_volume_music", "音乐音量"},
                {"settings_volume_sfx", "音效音量"},
                {"settings_quality", "画质"},
                {"quality_low", "低"},
                {"quality_medium", "中"},
                {"quality_high", "高"},
                {"settings_language", "语言"},
                {"lang_zh", "简体中文"},
                {"lang_en", "English"},
                {"settings_controls", "键位说明"},
                {"controls_text", "W/A/S/D 或方向键：航行转向\n鼠标：视角\n1-5：技能快捷键\nEsc：菜单"},

                // 关于
                {"about_title", "关于"},
                {"about_version", "版本：v0.5.0-D5"},
                {"about_dev", "独立开发者（鲸骨航线）"},
                {"about_compliance", "素材授权：原创/已授权资产；\n适龄提示：本游戏含轻度暴力与酒精描写，适合 12 岁以上。\n若使用 AI 生成素材，将在此列明授权记录。"},
                {"about_exit", "退出"},
            };

        // 英文表
        private static readonly System.Collections.Generic.Dictionary<string, string> En =
            new System.Collections.Generic.Dictionary<string, string>
            {
                {"title", "Whalebone"},
                {"menu_new_game", "New Game"},
                {"menu_continue", "Continue"},
                {"menu_settings", "Settings"},
                {"menu_about", "About"},
                {"menu_no_save", "No save found"},
                {"btn_back", "Back"},

                {"newgame_title", "New Game"},
                {"newgame_captain_name", "Captain Name"},
                {"newgame_placeholder_name", "Enter captain name…"},
                {"newgame_confirm", "Confirm"},
                {"newgame_cancel", "Cancel"},

                {"settings_title", "Settings"},
                {"settings_volume_master", "Master Volume"},
                {"settings_volume_music", "Music Volume"},
                {"settings_volume_sfx", "SFX Volume"},
                {"settings_quality", "Quality"},
                {"quality_low", "Low"},
                {"quality_medium", "Medium"},
                {"quality_high", "High"},
                {"settings_language", "Language"},
                {"lang_zh", "简体中文"},
                {"lang_en", "English"},
                {"settings_controls", "Controls"},
                {"controls_text", "W/A/S/D or Arrows: steer\nMouse: camera\n1-5: skill hotkeys\nEsc: menu"},

                {"about_title", "About"},
                {"about_version", "Version: v0.5.0-D5"},
                {"about_dev", "Solo Developer (Whalebone)"},
                {"about_compliance", "Assets: original/licensed.\nAge rating: mild violence & alcohol, 12+.\nAI-generated assets will be listed here with licenses."},
                {"about_exit", "Quit"},
            };

        /// <summary>当前语言（由 wb_lang 决定）。</summary>
        public static bool IsEnglish
        {
            get { return PlayerPrefs.GetString(LangPrefsKey, "zh") == "en"; }
        }

        /// <summary>按 key 取文案（无对应 key 时返回 key 本身，便于排查漏配）。</summary>
        public static string Get(string key)
        {
            var table = IsEnglish ? En : Zh;
            string value;
            return table.TryGetValue(key, out value) ? value : key;
        }
    }
}
