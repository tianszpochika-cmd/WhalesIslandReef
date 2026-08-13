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

using System.Collections.Generic;
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
                {"settings_accessibility", "无障碍与键位"},
                {"settings_diagnostics", "本地诊断与隐私"},
                {"settings_controls", "键位说明"},
                {"controls_text", "W/A/S/D 或方向键：航行转向\n鼠标：视角\n1-5：技能快捷键\nEsc：菜单"},

                // 关于
                {"about_title", "关于"},
                {"about_version", "版本：v0.5.0-D5"},
                {"about_dev", "独立开发者（鲸骨航线）"},
                {"about_compliance", "素材授权：原创/已授权资产；\n适龄提示：本游戏含轻度暴力与酒精描写，适合 12 岁以上。\n若使用 AI 生成素材，将在此列明授权记录。"},
                {"about_exit", "退出"},
                {"pause_title", "游戏暂停"},
                {"pause_resume", "继续游戏"},
                {"pause_accessibility", "无障碍设置"},
                {"pause_diagnostics", "本地诊断与隐私"},
                {"pause_finance", "金融与债务"},
                {"pause_main_menu", "返回主菜单"},
                {"pause_exit", "退出游戏"},
                {"finance_title", "金融与债务"},
                {"finance_subtitle", "只读显示 FinanceSystem 已裁决的账户、债务和风险；本页面不会提交任何金融操作。"},
                {"finance_empty", "当前存档没有可显示的金融快照或债务。"},
                {"finance_revision", "快照修订"},
                {"finance_account", "账户"},
                {"finance_cash", "现金"},
                {"finance_credit_used", "已用授信 / 授信上限"},
                {"finance_credit_score", "信用评分"},
                {"finance_debt", "债务"},
                {"finance_state", "状态"},
                {"finance_principal", "本金"},
                {"finance_interest", "累计利息"},
                {"finance_penalty", "滞纳金"},
                {"finance_total_due", "当前应付"},
                {"finance_due_day", "到期日"},
                {"finance_grace_day", "宽限至"},
                {"finance_risk", "风险说明"},
                {"finance_worst_case", "最坏结果"},
                {"finance_close", "关闭"},
                {"finance_state_Active", "有效"},
                {"finance_state_PastDue", "逾期"},
                {"finance_state_Extended", "已展期"},
                {"finance_state_Defaulted", "违约"},
                {"finance_state_Collection", "催收中"},
                {"finance_state_SeizurePending", "待查封"},
                {"finance_state_Seized", "已查封"},
                {"finance_state_Paid", "已偿清"},
                {"finance_state_BankruptcyProtected", "破产保护中"},
                {"finance_state_WrittenOff", "已核销"},
                {"diagnostics_title", "本地诊断与隐私"},
                {"diagnostics_subtitle", "默认关闭。只有你主动开启后，游戏才会在本机记录脱敏的运行时诊断。"},
                {"diagnostics_privacy", "不会上传或联网；不会写入原始报错、堆栈、存档正文或对话内容。\n只保留类别、场景、版本和长度统计；记录最多保留两个 512 KiB 文件。\n这不是原生崩溃转储，删除按钮只处理本模块的本地诊断文件。"},
                {"diagnostics_consent", "允许记录脱敏运行时诊断"},
                {"diagnostics_enabled", "已允许：只保存脱敏统计，不采集原始文本。"},
                {"diagnostics_disabled", "未允许：不会记录新的本地诊断。"},
                {"diagnostics_delete", "撤回同意并删除本地诊断"},
                {"diagnostics_deleted", "已撤回同意并删除本地诊断。"},
                {"a11y_title", "无障碍与键位"},
                {"a11y_subtitle", "设置会立即保存；打开面板时玩法输入会暂时锁定。"},
                {"a11y_sensitive_title", "内容提示"},
                {"a11y_sensitive_body", "此内容包含敏感主题。你可以继续查看，也可以跳过该内容。"},
                {"a11y_sensitive_continue", "继续查看"},
                {"a11y_sensitive_skip", "跳过内容"},
                {"a11y_text_scale", "文字大小"},
                {"a11y_high_contrast", "高对比度"},
                {"a11y_color_vision", "色觉辅助"},
                {"a11y_color_none", "标准"},
                {"a11y_color_deuteranopia", "红绿色弱（绿）"},
                {"a11y_color_protanopia", "红绿色弱（红）"},
                {"a11y_color_tritanopia", "蓝黄色弱"},
                {"a11y_reduced_motion", "减弱界面动态"},
                {"a11y_low_stimulus", "低刺激色彩"},
                {"a11y_subtitles", "显示字幕"},
                {"a11y_subtitle_speaker", "显示说话者"},
                {"a11y_sensitive_warnings", "敏感内容警告"},
                {"a11y_bindings_title", "操作重绑定"},
                {"a11y_keyboard", "键盘"},
                {"a11y_gamepad", "手柄"},
                {"a11y_wait_keyboard", "请按下要绑定的键；ESC 取消"},
                {"a11y_wait_gamepad", "请按下要绑定的手柄键；ESC 取消"},
                {"a11y_binding_saved", "绑定已保存"},
                {"a11y_binding_conflict", "该按键已被占用，请换一个"},
                {"a11y_binding_missing", "绑定失败，请恢复默认后重试"},
                {"a11y_reset", "恢复默认"},
                {"a11y_close", "关闭"},
                {"a11y_action_move_forward", "向前移动"},
                {"a11y_action_move_back", "向后移动"},
                {"a11y_action_move_left", "向左移动"},
                {"a11y_action_move_right", "向右移动"},
                {"a11y_action_sprint", "奔跑"},
                {"a11y_action_jump", "跳跃"},
                {"a11y_action_interact", "交互 / 上下船"},
                {"a11y_action_pause", "暂停菜单"},
                {"a11y_action_camera_cycle", "切换视角"},
                {"a11y_action_map", "打开地图"},
                {"a11y_action_trade", "贸易界面"},
                {"a11y_action_character_preset", "角色预设"},
                {"a11y_action_language", "切换语言"},
                {"a11y_action_quality", "画质设置"},
                {"a11y_action_achievement", "成就界面"},
                {"a11y_action_save", "保存状态"},
                {"a11y_action_d3_close_trade", "关闭贸易界面"},
                {"a11y_action_d3_start_voyage", "开始航程"},
                {"a11y_action_d3_advance_voyage", "推进航程"},
                {"a11y_action_d3_gather", "采集资源"},
                {"a11y_action_d3_land_settle", "登陆并结算"},
                {"a11y_action_d4_faction", "势力互动"},
                {"a11y_action_d4_start_combat", "开始炮战"},
                {"a11y_action_d4_reload", "装填火炮"},
                {"a11y_action_d4_round_shot", "选择实心弹"},
                {"a11y_action_d4_chain_shot", "选择链弹"},
                {"a11y_action_d4_grape_shot", "选择葡萄弹"},
                {"a11y_action_d4_fire", "开火"},
                {"a11y_action_d4_board", "接舷"},
                {"a11y_action_d4_escape_combat", "脱离战斗"},
                {"a11y_action_d4_upgrade", "船舶改装"},
                {"a11y_action_d4_observe_animal", "观察动物"},
                {"a11y_action_d4_treat_crew", "治疗船员"},
                {"a11y_action_d4_advance_mainline", "推进主线"},
                {"a11y_action_d5_discover_island", "发现岛屿"},
                {"a11y_action_d5_visit_island", "访问岛屿"},
                {"a11y_action_d9_open_port", "打开港口整备"},
                {"a11y_action_d9_close_port", "关闭港口整备"},
                {"a11y_action_d9_enter_berth", "进入泊位"},
                {"a11y_action_d9_buy_salt", "购买盐"},
                {"a11y_action_d9_buy_timber", "购买木材"},
                {"a11y_action_d9_visit_tavern", "进入酒馆"},
                {"a11y_action_d9_buy_water", "购买淡水"},
                {"a11y_action_d9_repair_hull", "修理船体"},
                {"a11y_action_d9_recruit_crew", "招募船员"},
                {"a11y_action_d9_settle_wages", "结算工资"},
                {"a11y_action_d9_upgrade", "升级船只"},
                {"a11y_action_d9_depart", "离开港口"},
                {"a11y_action_d10_open_tavern", "打开酒馆"},
                {"a11y_action_d10_close_tavern", "关闭酒馆"},
                {"a11y_action_d10_board", "打开悬赏板"},
                {"a11y_action_d10_security_job", "接取安保工作"},
                {"a11y_action_d10_rest", "休息"},
                {"a11y_action_d10_eat", "用餐"},
                {"a11y_action_d10_drink", "饮酒"},
                {"a11y_action_d10_rumor", "听取传闻"},
                {"a11y_action_d10_poker", "参加扑克"},
                {"a11y_action_d10_dance", "参加舞会"},
                {"a11y_action_d10_night_talk", "进行夜谈"},
                {"a11y_action_d10_recruit", "招募船员"},
                {"a11y_action_d10_skip_sensitive", "跳过敏感内容"},
                // D9 音频事件字幕
                {"subtitle_departure_prepared", "出航准备完成"},
                {"subtitle_departure", "起航"},
                {"subtitle_fishing_catch", "捕获鱼获"},
                {"subtitle_weather_storm", "风暴来袭"},
                {"subtitle_crew_report", "船员报告"},
                {"subtitle_combat_warning", "发现敌对巡逻"},
                {"subtitle_combat_retreat", "已脱离战斗"},
                {"subtitle_treasure_clue", "发现宝藏线索"},
                {"subtitle_treasure_resolved", "宝藏事件已解决"},
                {"subtitle_returned_to_harbor", "已返回港口"},
                {"subtitle_trade_settled", "贸易结算完成"},
                {"subtitle_wage_settled", "工资已结算"},
                {"subtitle_ship_repaired", "船只修理完成"},
                {"subtitle_save_checkpoint", "存档检查点已完成"},
                {"dialogue_send", "发送"},
                {"dialogue_close", "关闭"},
            };

        // 英文表
        private static readonly System.Collections.Generic.Dictionary<string, string> En =
            new System.Collections.Generic.Dictionary<string, string>
            {
                {"title", "WhalesIslandReef"},
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
                {"settings_accessibility", "Accessibility & Controls"},
                {"settings_diagnostics", "Local Diagnostics & Privacy"},
                {"settings_controls", "Controls"},
                {"controls_text", "W/A/S/D or Arrows: steer\nMouse: camera\n1-5: skill hotkeys\nEsc: menu"},

                {"about_title", "About"},
                {"about_version", "Version: v0.5.0-D5"},
                {"about_dev", "Solo Developer (Whalebone)"},
                {"about_compliance", "Assets: original/licensed.\nAge rating: mild violence & alcohol, 12+.\nAI-generated assets will be listed here with licenses."},
                {"about_exit", "Quit"},
                {"pause_title", "Game Paused"},
                {"pause_resume", "Resume"},
                {"pause_accessibility", "Accessibility"},
                {"pause_diagnostics", "Local Diagnostics & Privacy"},
                {"pause_finance", "Finance & Debt"},
                {"pause_main_menu", "Main Menu"},
                {"pause_exit", "Quit Game"},
                {"finance_title", "Finance & Debt"},
                {"finance_subtitle", "Read-only view of FinanceSystem decisions, debts and risks; this page submits no financial operation."},
                {"finance_empty", "This save has no finance snapshot or debt to display."},
                {"finance_revision", "Snapshot revision"},
                {"finance_account", "Account"},
                {"finance_cash", "Cash"},
                {"finance_credit_used", "Credit used / limit"},
                {"finance_credit_score", "Credit score"},
                {"finance_debt", "Debt"},
                {"finance_state", "State"},
                {"finance_principal", "Principal"},
                {"finance_interest", "Accrued interest"},
                {"finance_penalty", "Late penalty"},
                {"finance_total_due", "Current due"},
                {"finance_due_day", "Due day"},
                {"finance_grace_day", "Grace until"},
                {"finance_risk", "Risk explanation"},
                {"finance_worst_case", "Worst-case outcome"},
                {"finance_close", "Close"},
                {"finance_state_Active", "Active"},
                {"finance_state_PastDue", "Past due"},
                {"finance_state_Extended", "Extended"},
                {"finance_state_Defaulted", "Defaulted"},
                {"finance_state_Collection", "In collection"},
                {"finance_state_SeizurePending", "Seizure pending"},
                {"finance_state_Seized", "Seized"},
                {"finance_state_Paid", "Paid"},
                {"finance_state_BankruptcyProtected", "Bankruptcy protected"},
                {"finance_state_WrittenOff", "Written off"},
                {"diagnostics_title", "Local Diagnostics & Privacy"},
                {"diagnostics_subtitle", "Off by default. Diagnostics are recorded on this device only after you opt in."},
                {"diagnostics_privacy", "Nothing is uploaded or sent over the network; raw errors, stacks, save data and dialogue are omitted.\nOnly category, scene, version and length statistics are kept; at most two 512 KiB files are retained.\nThis is not a native crash dump. Delete only affects this module's local diagnostic files."},
                {"diagnostics_consent", "Allow redacted runtime diagnostics"},
                {"diagnostics_enabled", "Enabled: only redacted statistics are stored; raw text is not collected."},
                {"diagnostics_disabled", "Disabled: no new local diagnostics will be recorded."},
                {"diagnostics_delete", "Revoke consent and delete local diagnostics"},
                {"diagnostics_deleted", "Consent revoked and local diagnostics deleted."},
                {"a11y_title", "Accessibility & Controls"},
                {"a11y_subtitle", "Changes save immediately; gameplay input is locked while this panel is open."},
                {"a11y_sensitive_title", "Content warning"},
                {"a11y_sensitive_body", "This content includes sensitive themes. You can continue or skip it."},
                {"a11y_sensitive_continue", "Continue"},
                {"a11y_sensitive_skip", "Skip content"},
                {"a11y_text_scale", "Text size"},
                {"a11y_high_contrast", "High contrast"},
                {"a11y_color_vision", "Color vision"},
                {"a11y_color_none", "Standard"},
                {"a11y_color_deuteranopia", "Deuteranopia"},
                {"a11y_color_protanopia", "Protanopia"},
                {"a11y_color_tritanopia", "Tritanopia"},
                {"a11y_reduced_motion", "Reduced interface motion"},
                {"a11y_low_stimulus", "Low-stimulus colors"},
                {"a11y_subtitles", "Show subtitles"},
                {"a11y_subtitle_speaker", "Show speaker labels"},
                {"a11y_sensitive_warnings", "Sensitive-content warnings"},
                {"a11y_bindings_title", "Control rebinding"},
                {"a11y_keyboard", "Keyboard"},
                {"a11y_gamepad", "Gamepad"},
                {"a11y_wait_keyboard", "Press the key to bind; ESC cancels"},
                {"a11y_wait_gamepad", "Press the gamepad button to bind; ESC cancels"},
                {"a11y_binding_saved", "Binding saved"},
                {"a11y_binding_conflict", "That input is already used"},
                {"a11y_binding_missing", "Binding failed; restore defaults and retry"},
                {"a11y_reset", "Restore defaults"},
                {"a11y_close", "Close"},
                {"a11y_action_move_forward", "Move forward"},
                {"a11y_action_move_back", "Move back"},
                {"a11y_action_move_left", "Move left"},
                {"a11y_action_move_right", "Move right"},
                {"a11y_action_sprint", "Sprint"},
                {"a11y_action_jump", "Jump"},
                {"a11y_action_interact", "Interact / board"},
                {"a11y_action_pause", "Pause menu"},
                {"a11y_action_camera_cycle", "Cycle camera"},
                {"a11y_action_map", "Open map"},
                {"a11y_action_trade", "Trade screen"},
                {"a11y_action_character_preset", "Character preset"},
                {"a11y_action_language", "Switch language"},
                {"a11y_action_quality", "Quality settings"},
                {"a11y_action_achievement", "Achievements"},
                {"a11y_action_save", "Save state"},
                {"a11y_action_d3_close_trade", "Close trade screen"},
                {"a11y_action_d3_start_voyage", "Start voyage"},
                {"a11y_action_d3_advance_voyage", "Advance voyage"},
                {"a11y_action_d3_gather", "Gather resources"},
                {"a11y_action_d3_land_settle", "Land and settle"},
                {"a11y_action_d4_faction", "Faction interaction"},
                {"a11y_action_d4_start_combat", "Start naval combat"},
                {"a11y_action_d4_reload", "Reload cannons"},
                {"a11y_action_d4_round_shot", "Select round shot"},
                {"a11y_action_d4_chain_shot", "Select chain shot"},
                {"a11y_action_d4_grape_shot", "Select grape shot"},
                {"a11y_action_d4_fire", "Fire"},
                {"a11y_action_d4_board", "Board vessel"},
                {"a11y_action_d4_escape_combat", "Disengage combat"},
                {"a11y_action_d4_upgrade", "Upgrade ship"},
                {"a11y_action_d4_observe_animal", "Observe animal"},
                {"a11y_action_d4_treat_crew", "Treat crew"},
                {"a11y_action_d4_advance_mainline", "Advance mainline"},
                {"a11y_action_d5_discover_island", "Discover island"},
                {"a11y_action_d5_visit_island", "Visit island"},
                {"a11y_action_d9_open_port", "Open port preparation"},
                {"a11y_action_d9_close_port", "Close port preparation"},
                {"a11y_action_d9_enter_berth", "Enter berth"},
                {"a11y_action_d9_buy_salt", "Buy salt"},
                {"a11y_action_d9_buy_timber", "Buy timber"},
                {"a11y_action_d9_visit_tavern", "Visit tavern"},
                {"a11y_action_d9_buy_water", "Buy fresh water"},
                {"a11y_action_d9_repair_hull", "Repair hull"},
                {"a11y_action_d9_recruit_crew", "Recruit crew"},
                {"a11y_action_d9_settle_wages", "Settle wages"},
                {"a11y_action_d9_upgrade", "Upgrade ship"},
                {"a11y_action_d9_depart", "Depart port"},
                {"a11y_action_d10_open_tavern", "Open tavern"},
                {"a11y_action_d10_close_tavern", "Close tavern"},
                {"a11y_action_d10_board", "Open side-job board"},
                {"a11y_action_d10_security_job", "Take security job"},
                {"a11y_action_d10_rest", "Rest"},
                {"a11y_action_d10_eat", "Eat"},
                {"a11y_action_d10_drink", "Drink"},
                {"a11y_action_d10_rumor", "Hear rumor"},
                {"a11y_action_d10_poker", "Play poker"},
                {"a11y_action_d10_dance", "Join dance"},
                {"a11y_action_d10_night_talk", "Night talk"},
                {"a11y_action_d10_recruit", "Recruit crew"},
                {"a11y_action_d10_skip_sensitive", "Skip sensitive content"},
                // D9 audio-event subtitles
                {"subtitle_departure_prepared", "Departure preparation complete"},
                {"subtitle_departure", "Underway"},
                {"subtitle_fishing_catch", "Catch secured"},
                {"subtitle_weather_storm", "Storm approaching"},
                {"subtitle_crew_report", "Crew report"},
                {"subtitle_combat_warning", "Hostile patrol sighted"},
                {"subtitle_combat_retreat", "Combat disengaged"},
                {"subtitle_treasure_clue", "Treasure clue found"},
                {"subtitle_treasure_resolved", "Treasure event resolved"},
                {"subtitle_returned_to_harbor", "Returned to harbor"},
                {"subtitle_trade_settled", "Trade settled"},
                {"subtitle_wage_settled", "Wages settled"},
                {"subtitle_ship_repaired", "Ship repairs complete"},
                {"subtitle_save_checkpoint", "Save checkpoint complete"},
                {"dialogue_send", "Send"},
                {"dialogue_close", "Close"},
            };

        /// <summary>当前语言（由 wb_lang 决定）。</summary>
        public static bool IsEnglish
        {
            get { return PlayerPrefs.GetString(LangPrefsKey, "zh") == "en"; }
        }

        /// <summary>统一设置语言，供运行时 HUD 与主菜单共用。</summary>
        public static void SetLanguage(string language)
        {
            PlayerPrefs.SetString(LangPrefsKey, language == "en" ? "en" : "zh");
            PlayerPrefs.Save();
        }

        /// <summary>按 key 取文案（无对应 key 时返回 key 本身，便于排查漏配）。</summary>
        public static string Get(string key)
        {
            var table = IsEnglish ? En : Zh;
            string value;
            return table.TryGetValue(key, out value) ? value : key;
        }

        /// <summary>
        /// 校验表现层静态文案表的结构完整性。
        ///
        /// 这不是历史/内容 locale 包的替代物；它只防止 UI 自有 key 在中英两表
        /// 之间漂移，或因为空值让运行时静默显示 key。调用方只读结果，不修改表。
        /// </summary>
        public static List<string> ValidateTables()
        {
            var errors = new List<string>();
            foreach (var pair in Zh)
            {
                string english;
                if (!En.TryGetValue(pair.Key, out english))
                    errors.Add("missing_en:" + pair.Key);
                if (string.IsNullOrWhiteSpace(pair.Value))
                    errors.Add("empty_zh:" + pair.Key);
                if (En.TryGetValue(pair.Key, out english) && string.IsNullOrWhiteSpace(english))
                    errors.Add("empty_en:" + pair.Key);
            }

            foreach (var pair in En)
            {
                if (!Zh.ContainsKey(pair.Key))
                    errors.Add("missing_zh:" + pair.Key);
            }

            return errors;
        }
    }
}
