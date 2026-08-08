// ============================================================
// SeedData —— 存档种子数据（DATA_MODELS §4/§5：trait_defs 20 条 + ship_type_defs 2 条）
//
// 归属：2 船员系统 / 5 船只系统 的纯数据（Save 域负责幂等落库）
// 结构：身体 8 + 性格 8 + 经历 4（M0 首批；疾病类 M1 引入）
// 数值约定见 TraitDef.cs 文件头。
// 落库方式：INSERT OR IGNORE（按 id 幂等，重复执行不产生脏数据）
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// 特质种子（M0 首批 20 条）。
    /// </summary>
    public static class TraitSeed
    {
        private static List<TraitDef> _all;

        /// <summary>全部 20 条特质（懒加载只读）。</summary>
        public static IReadOnlyList<TraitDef> All => _all ?? (_all = Build());

        private static List<TraitDef> Build()
        {
            return new List<TraitDef>
            {
                // ---------------- 身体 8 ----------------
                new TraitDef("trait_strong", "强壮", TraitCategory.Physical,
                    "体格魁梧，干重活不费劲，耐受力好。",
                    "你一身腱子肉，扛缆绳、搬货箱都抢在头一个。",
                    new Dictionary<string, float> { ["health_max"] = 0.10f, ["fatigue_max"] = -0.10f }),

                new TraitDef("trait_weak", "瘦弱", TraitCategory.Physical,
                    "身形单薄，容易累，扛不动重物。",
                    "你从小营养不良，晒一天太阳就头晕。",
                    new Dictionary<string, float> { ["health_max"] = -0.10f, ["fatigue_max"] = 0.15f }),

                new TraitDef("trait_lung_disease", "肺病", TraitCategory.Physical,
                    "旧病缠身，劳作过度会咳得直不起腰。",
                    "你时常咳嗽，稍微劳累就喘不上气。",
                    new Dictionary<string, float> { ["health_max"] = -0.25f, ["fatigue_max"] = 0.20f },
                    new Dictionary<string, float> { ["illness"] = 0.05f }),

                new TraitDef("trait_good_eyes", "好视力", TraitCategory.Physical,
                    "眼神锐利，能看清远处桅杆上的旗语。",
                    "你站在船头就能数清岸上的人影。",
                    new Dictionary<string, float> { ["navigation"] = 1.0f, ["lookout_range"] = 0.15f }),

                new TraitDef("trait_medic_apprentice", "医学徒", TraitCategory.Physical,
                    "粗通医术，认得几十种草药，会包扎止血。",
                    "你曾在药剂师门下当过学徒，认得几十种草药。",
                    new Dictionary<string, float> { ["medicine"] = 1.0f, ["health_max"] = 0.05f }),

                new TraitDef("trait_fallen_noble", "落难贵族", TraitCategory.Physical,
                    "出身望族，家道中落后流落码头，举止仍讲究。",
                    "你原本是贵族，如今家道中落流落码头，举止仍透着讲究。",
                    new Dictionary<string, float> { ["fatigue_max"] = 0.10f, ["trading"] = 0.5f }),

                new TraitDef("trait_former_navy", "前海军", TraitCategory.Physical,
                    "在皇家海军服役多年，令行禁止、体魄强健。",
                    "你在皇家海军服役过七年，令行禁止刻进了骨头里。",
                    new Dictionary<string, float> { ["gunnery"] = 1.0f, ["discipline"] = 0.10f }),

                new TraitDef("trait_good_stomach", "酒量好", TraitCategory.Physical,
                    "千杯不醉，船上酒宴永远最后一个倒下。",
                    "千杯不醉，船上酒宴你永远最后一个倒下。",
                    new Dictionary<string, float> { ["fatigue_max"] = -0.05f, ["morale_max"] = 0.05f },
                    new Dictionary<string, float> { ["drink"] = 0.10f }),

                // ---------------- 性格 8 ----------------
                new TraitDef("trait_greedy", "贪婪", TraitCategory.Personality,
                    "钱比命重，看到金币眼睛就发亮。",
                    "钱比命重，看到金币眼睛就发亮。",
                    new Dictionary<string, float> { ["loyalty_max"] = -0.10f },
                    new Dictionary<string, float> { ["mutiny"] = 0.05f, ["bribe"] = 0.10f }),

                new TraitDef("trait_cowardly", "怯懦", TraitCategory.Personality,
                    "遇事总想缩到人群后面，怕见血。",
                    "遇事总想缩到人群后面。",
                    new Dictionary<string, float> { ["combat"] = -1.0f },
                    new Dictionary<string, float> { ["mutiny"] = -0.05f }),

                new TraitDef("trait_loyal", "忠诚", TraitCategory.Personality,
                    "认定的人，刀架脖子也不背叛。",
                    "认定的人，刀架脖子也不背叛。",
                    new Dictionary<string, float> { ["loyalty_max"] = 0.15f },
                    new Dictionary<string, float> { ["mutiny"] = -0.10f }),

                new TraitDef("trait_drunkard", "嗜酒", TraitCategory.Personality,
                    "清醒时是好水手，醉起来六亲不认。",
                    "清醒时是个好水手，酒醒之前别让他上桅杆。",
                    new Dictionary<string, float> { ["fatigue_max"] = 0.05f, ["loyalty_max"] = -0.05f },
                    new Dictionary<string, float> { ["mutiny"] = 0.03f }),

                new TraitDef("trait_hearty", "豪爽", TraitCategory.Personality,
                    "好交朋友，请客从不眨眼，船上人缘好。",
                    "好交朋友，请客从不眨眼。",
                    new Dictionary<string, float> { ["morale_boost"] = 0.05f },
                    new Dictionary<string, float> { ["drink"] = 0.10f }),

                new TraitDef("trait_grudge", "记仇", TraitCategory.Personality,
                    "得罪过你的人，十年后还能认出他。",
                    "得罪过你的人，十年后还能认出他。",
                    new Dictionary<string, float> { ["affinity_max"] = -0.10f },
                    new Dictionary<string, float> { ["mutiny"] = 0.03f }),

                new TraitDef("trait_optimist", "乐天", TraitCategory.Personality,
                    "天塌下来也先笑一笑，船上氛围的调和剂。",
                    "天塌下来也先笑一笑。",
                    new Dictionary<string, float> { ["morale_max"] = 0.10f, ["fatigue_max"] = -0.05f }),

                new TraitDef("trait_cautious", "谨慎", TraitCategory.Personality,
                    "凡事多想一步，从不打无准备的仗。",
                    "凡事多想一步，从不打无准备的仗。",
                    new Dictionary<string, float> { ["mutiny_risk"] = -0.05f, ["cargo_loss"] = -0.05f }),

                // ---------------- 经历 4 ----------------
                new TraitDef("trait_old_sailor", "老水手", TraitCategory.Experience,
                    "四十年海上生涯，闭着眼都知道风向要变。",
                    "四十年的海上生涯，闭着眼都知道风向要变。",
                    new Dictionary<string, float> { ["sailing"] = 1.0f, ["navigation"] = 0.5f },
                    new Dictionary<string, float> { ["storm"] = -0.10f }),

                new TraitDef("trait_ex_privateer", "前私掠者", TraitCategory.Experience,
                    "挂着私掠许可证抢过西班牙船，现在只求安稳。",
                    "挂着私掠许可证抢过西班牙船，现在只求安稳。",
                    new Dictionary<string, float> { ["gunnery"] = 1.0f, ["combat"] = 0.5f },
                    new Dictionary<string, float> { ["mutiny"] = 0.03f }),

                new TraitDef("trait_fugitive", "逃犯", TraitCategory.Experience,
                    "背着悬赏令，最怕靠岸时遇上差役。",
                    "你背着悬赏令，最怕靠岸时遇上差役。",
                    null,
                    new Dictionary<string, float> { ["mutiny"] = 0.02f, ["leave_ship"] = 0.05f }),

                new TraitDef("trait_missionary", "传教士", TraitCategory.Experience,
                    "在船上布道，劝大家少喝酒多祈祷。",
                    "你在船上布道，劝大家少喝酒多祈祷。",
                    new Dictionary<string, float> { ["medicine"] = 0.5f, ["morale_boost"] = 0.05f },
                    new Dictionary<string, float> { ["mutiny"] = -0.05f }),
            };
        }

        /// <summary>按 id 查找（找不到返回 null）。</summary>
        public static TraitDef Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var t in All)
                if (t.Id == id) return t;
            return null;
        }
    }

    /// <summary>
    /// 船型种子（M0 首批 2 条）。
    /// </summary>
    public static class ShipTypeSeed
    {
        private static List<ShipTypeDef> _all;

        public static IReadOnlyList<ShipTypeDef> All => _all ?? (_all = Build());

        private static List<ShipTypeDef> Build()
        {
            return new List<ShipTypeDef>
            {
                new ShipTypeDef("small_fishing", "小渔船", 4.0f, 6.0f,
                    cargoMax: 40, crewMax: 4, cannonMax: 0, hullMax: 60,
                    draft: 0.8f, price: 300),   // 史实：沿海小渔船 30-50 镑 ≈ 300 银币；吃水 <1m 可进浅湾

                new ShipTypeDef("sloop", "单桅纵帆船", 7.0f, 5.0f,
                    cargoMax: 120, crewMax: 12, cannonMax: 8, hullMax: 150,
                    draft: 1.6f, price: 3000),  // 史实：1675 加勒比贸易/私掠主力（200-400 镑 ≈ 3000 银币）
            };
        }

        /// <summary>按 id 查找（找不到返回 null）。</summary>
        public static ShipTypeDef Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var t in All)
                if (t.Id == id) return t;
            return null;
        }
    }
}
