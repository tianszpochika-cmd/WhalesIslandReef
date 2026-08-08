// ============================================================
// TraitDef —— 特质定义（纯数据，对应 DATA_MODELS §4）
//
// 归属：2 船员系统（当前由 Save 域负责建表与种子落库，P2 特质系统接手运行时消费）
// 说明：
//   - statMods / eventMods 的 key 为参数名（自由文本约定，见下方"数值约定"）；
//   - M0 首批 20 条：身体 8 + 性格 8 + 经历 4（无疾病类，疾病 M1 引入）。
//
// 数值约定（与 AgentCore.CrewStats 的 stat 名对应）：
//   - 属性类（百分比 ±，如 -0.15 = -15%）：health_max / fatigue_max / morale_max /
//     loyalty_max / affinity_max / morale_boost / affinity_boost / mutiny_risk
//   - 技能类（绝对值，1.0 = +1 级）：sailing / navigation / gunnery / carpentry /
//     medicine / cookery / trading / combat
//   - 事件类（概率修正 ±）：mutiny / storm / illness / drink / leave_ship /
//     cargo_loss / bribe
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.Save
{
    /// <summary>特质大类（M0：身体 / 性格 / 经历；疾病 M1 追加）。</summary>
    public enum TraitCategory
    {
        Physical,     // 身体
        Personality,  // 性格
        Experience    // 经历
    }

    /// <summary>
    /// 特质定义（内存形态 = trait_defs 表一行）。
    /// </summary>
    public class TraitDef
    {
        public string Id;
        public string Name;            // 中文名（展示）
        public TraitCategory Category;
        public string Desc;            // 展示文案
        public Dictionary<string, float> StatMods = new Dictionary<string, float>();
        public string PersonaNote;     // 注入 Persona 的文本（"你时常咳嗽…"）
        public Dictionary<string, float> EventMods = new Dictionary<string, float>();

        public TraitDef() { }

        public TraitDef(string id, string name, TraitCategory category, string desc,
                        string personaNote, Dictionary<string, float> statMods = null,
                        Dictionary<string, float> eventMods = null)
        {
            Id = id;
            Name = name;
            Category = category;
            Desc = desc;
            PersonaNote = personaNote;
            if (statMods != null) StatMods = statMods;
            if (eventMods != null) EventMods = eventMods;
        }
    }
}
