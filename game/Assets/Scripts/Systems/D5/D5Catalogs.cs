using System.Collections.Generic;

namespace Whalebone.Systems.D5
{
    public static class D5IslandCatalog
    {
        public static readonly IReadOnlyList<D5IslandDefinition> All = new List<D5IslandDefinition>
        {
            Island("santa_margarita", "圣玛格丽特港", "Sainte-Marguerite", D5IslandType.TownPort, D5Biome.Palm, "spain", 18, 1.00f, -0.80f, -0.10f),
            Island("green_cay", "绿礁岛", "Green Cay", D5IslandType.Village, D5Biome.Reef, "tribes", 22, 0.92f, -0.55f, 0.35f),
            Island("driftwood_isle", "漂木荒岛", "Driftwood Isle", D5IslandType.Wilderness, D5Biome.Beach, "smugglers", 38, 1.10f, -0.25f, -0.55f),
            Island("port_royal", "皇家港", "Port Royal", D5IslandType.TownPort, D5Biome.Mangrove, "england", 35, 1.15f, -0.05f, 0.55f),
            Island("havana", "哈瓦那", "Havana", D5IslandType.Fortress, D5Biome.Palm, "spain", 62, 1.28f, 0.22f, 0.70f),
            Island("tortuga", "托尔图加", "Tortuga", D5IslandType.Special, D5Biome.Reef, "pirates", 48, 1.35f, 0.55f, 0.35f),
            Island("martinique", "马提尼克", "Martinique", D5IslandType.TownPort, D5Biome.Rainforest, "france", 30, 1.12f, 0.70f, -0.10f),
            Island("madeira_roadstead", "马德拉锚地", "Madeira Roadstead", D5IslandType.TownPort, D5Biome.Palm, "portugal", 28, 1.08f, 0.45f, -0.58f),
            Island("mangrove_key", "红树林礁", "Mangrove Key", D5IslandType.Village, D5Biome.Mangrove, "tribes", 44, 0.88f, 0.05f, -0.80f),
            Island("treasure_cay", "宝藏礁", "Treasure Cay", D5IslandType.Special, D5Biome.Beach, "smugglers", 72, 1.55f, -0.48f, -0.88f),
            Island("crown_fort", "王冠要塞", "Crown Fort", D5IslandType.Fortress, D5Biome.Reef, "england", 75, 1.24f, -0.72f, 0.82f),
            Island("jaguar_isle", "美洲豹岛", "Jaguar Isle", D5IslandType.Wilderness, D5Biome.Rainforest, "tribes", 88, 1.18f, 0.88f, 0.75f),
        };

        public static D5IslandDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var island in All) if (island.Id == id) return island;
            return null;
        }

        private static D5IslandDefinition Island(string id, string zh, string en, D5IslandType type, D5Biome biome,
            string faction, int danger, float trade, float x, float y)
        {
            return new D5IslandDefinition
            {
                Id = id, NameZh = zh, NameEn = en, Type = type, Biome = biome,
                FactionId = faction, Danger = danger, TradeMultiplier = trade, MapX = x, MapY = y,
            };
        }
    }

    public static class D5TraitCatalog
    {
        public static readonly IReadOnlyList<D5TraitDefinition> All = new List<D5TraitDefinition>
        {
            // 身体 15
            Trait("strong", "强壮", "Strong", D5TraitCategory.Body, "hull_work", 8, "搬运和修船时更有力气。"),
            Trait("weak", "瘦弱", "Frail", D5TraitCategory.Body, "fatigue_limit", -8, "体力消耗更快，讨厌长时间劳作。"),
            Trait("lung_disease", "肺病", "Lung Disease", D5TraitCategory.Body, "health_decay", -7, "你时常咳嗽，爬桅杆很吃力。"),
            Trait("hardy_stomach", "肠胃好", "Hardy Stomach", D5TraitCategory.Body, "disease_resist", 8, "污水和粗粮也很难击倒你。"),
            Trait("presbyopia", "老花眼", "Presbyopia", D5TraitCategory.Body, "night_watch", -5, "夜间瞭望时需要更近的距离。"),
            Trait("injured_leg", "伤腿", "Injured Leg", D5TraitCategory.Body, "move_speed", -6, "长途步行会让旧伤复发。"),
            Trait("keen_eyes", "好视力", "Keen Eyes", D5TraitCategory.Body, "sight", 9, "远处的帆影和鸟群逃不过你的眼睛。"),
            Trait("heat_resistant", "抗热", "Heat Resistant", D5TraitCategory.Body, "tropical_fatigue", 8, "加勒比正午的热浪对你影响较小。"),
            Trait("cold_sensitive", "怕冷", "Cold Sensitive", D5TraitCategory.Body, "storm_health", -5, "冷雨和夜风会迅速消耗你的状态。"),
            Trait("nimble", "手脚灵活", "Nimble", D5TraitCategory.Body, "boarding", 7, "你在湿滑甲板上仍能保持平衡。"),
            Trait("broad_shoulders", "宽肩", "Broad Shoulders", D5TraitCategory.Body, "cannon_reload", 6, "推炮和拉缆绳时有明显优势。"),
            Trait("seasick", "晕船", "Seasick", D5TraitCategory.Body, "voyage_fatigue", -8, "海况恶化时你脸色最先变白。"),
            Trait("night_vision", "夜视", "Night Vision", D5TraitCategory.Body, "night_watch", 8, "无月夜里你仍能辨认礁石轮廓。"),
            Trait("scarred", "伤疤累累", "Scarred", D5TraitCategory.Body, "intimidation", 5, "你的伤疤让陌生人下意识保持距离。"),
            Trait("frail", "体质虚弱", "Delicate", D5TraitCategory.Body, "health_max", -6, "你需要规律饮水和休息。"),

            // 性格 20
            Trait("greedy", "贪婪", "Greedy", D5TraitCategory.Personality, "profit", 8, "每一枚金币都值得冒一次险。"),
            Trait("cowardly", "怯懦", "Cowardly", D5TraitCategory.Personality, "morale_combat", -8, "炮声响起时你会先寻找掩体。"),
            Trait("loyal", "忠诚", "Loyal", D5TraitCategory.Personality, "loyalty", 9, "你答应过的事，即使逆风也会做到。"),
            Trait("alcoholic", "嗜酒", "Heavy Drinker", D5TraitCategory.Personality, "morale_rest", 5, "酒馆里的热闹能让你暂时忘记疲劳。"),
            Trait("suspicious", "多疑", "Suspicious", D5TraitCategory.Personality, "inspection", 6, "你总能先察觉货单上的不对劲。"),
            Trait("impulsive", "冲动", "Impulsive", D5TraitCategory.Personality, "combat_damage", 6, "你出手很快，但不总是等待命令。"),
            Trait("cautious", "谨慎", "Cautious", D5TraitCategory.Personality, "risk", 8, "你宁愿多花一天，也不驶入未知暗礁。"),
            Trait("generous", "豪爽", "Generous", D5TraitCategory.Personality, "crew_morale", 7, "你愿意把战利品分给一起冒险的人。"),
            Trait("vengeful", "记仇", "Vengeful", D5TraitCategory.Personality, "rival_damage", 7, "旧账不会因为海风而消失。"),
            Trait("optimistic", "乐天", "Optimistic", D5TraitCategory.Personality, "morale", 8, "再坏的天气也总会过去。"),
            Trait("superstitious", "迷信", "Superstitious", D5TraitCategory.Personality, "event_warning", 4, "你会给每一次反常的海鸟找个解释。"),
            Trait("disciplined", "自律", "Disciplined", D5TraitCategory.Personality, "sailing", 8, "值班铃响时你从不迟到。"),
            Trait("proud", "骄傲", "Proud", D5TraitCategory.Personality, "reputation", 6, "你不愿在众人面前示弱。"),
            Trait("patient", "耐心", "Patient", D5TraitCategory.Personality, "trade", 7, "你能等到最合适的价格和潮汐。"),
            Trait("hot_tempered", "暴躁", "Hot Tempered", D5TraitCategory.Personality, "conflict", -5, "小摩擦也可能在你手里变成斗殴。"),
            Trait("curious", "好奇", "Curious", D5TraitCategory.Personality, "exploration", 8, "未标记的海岸线总在召唤你。"),
            Trait("thrifty", "节俭", "Thrifty", D5TraitCategory.Personality, "supply", 7, "你知道什么东西必须留到下一个港口。"),
            Trait("ambitious", "野心勃勃", "Ambitious", D5TraitCategory.Personality, "career", 8, "小渔船只是你计划的第一步。"),
            Trait("compassionate", "富有同情心", "Compassionate", D5TraitCategory.Personality, "medical", 7, "你不会把受伤的人留在礁石上。"),
            Trait("fatalistic", "宿命论", "Fatalistic", D5TraitCategory.Personality, "storm_morale", 3, "你把无法控制的海况交给命运。"),

            // 经历 10
            Trait("old_sailor", "老水手", "Old Sailor", D5TraitCategory.Background, "sailing", 10, "你看过太多天气，知道何时该收帆。"),
            Trait("former_privateer", "前私掠者", "Former Privateer", D5TraitCategory.Background, "combat", 9, "你熟悉私掠证的文字陷阱和炮术礼节。"),
            Trait("fugitive", "逃犯", "Fugitive", D5TraitCategory.Background, "wanted", -5, "某个港口的守卫仍记得你的脸。"),
            Trait("missionary", "传教士", "Missionary", D5TraitCategory.Background, "culture", 6, "你会先问候对方的信仰，再谈交易。"),
            Trait("ex_navy", "前海军", "Former Navy", D5TraitCategory.Background, "discipline", 9, "你熟悉舰队编制和标准旗语。"),
            Trait("fallen_noble", "落难贵族", "Fallen Noble", D5TraitCategory.Background, "negotiation", 7, "你仍记得如何在宴席上辨认权力。"),
            Trait("doctor_apprentice", "医生学徒", "Doctor's Apprentice", D5TraitCategory.Background, "medicine", 10, "你会把草药和清水用在正确的人身上。"),
            Trait("island_guide", "岛屿向导", "Island Guide", D5TraitCategory.Background, "island_safety", 9, "你认得红树林里不该踩下去的泥。"),
            Trait("dock_runner", "码头跑腿", "Dock Runner", D5TraitCategory.Background, "trade", 8, "你能在港口谣言变旧前找到买家。"),
            Trait("cartographer", "制图师", "Cartographer", D5TraitCategory.Background, "map", 10, "你的海图会把暗礁和风向标得很清楚。"),

            // 疾病 5
            Trait("scurvy", "坏血病", "Scurvy", D5TraitCategory.Disease, "health_decay", -8, "缺少新鲜蔬果时牙龈会先发出警告。"),
            Trait("malaria", "疟疾", "Malaria", D5TraitCategory.Disease, "health_decay", -10, "蚊虫密集的湿地会让你发热。"),
            Trait("yellow_fever", "黄热病", "Yellow Fever", D5TraitCategory.Disease, "health_decay", -12, "湿热港湾对你的身体格外危险。"),
            Trait("dysentery", "痢疾", "Dysentery", D5TraitCategory.Disease, "health_decay", -9, "污染饮水会让航程变得难以忍受。"),
            Trait("consumption", "肺痨", "Consumption", D5TraitCategory.Disease, "health_decay", -6, "长期虚弱使你不适合连续值夜。"),
        };

        public static D5TraitDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var trait in All) if (trait.Id == id) return trait;
            return null;
        }

        public static int Count(D5TraitCategory category)
        {
            int count = 0;
            foreach (var trait in All) if (trait.Category == category) count++;
            return count;
        }

        private static D5TraitDefinition Trait(string id, string zh, string en, D5TraitCategory category,
            string modifier, int value, string persona)
        {
            return new D5TraitDefinition
            {
                Id = id, NameZh = zh, NameEn = en, Category = category,
                Modifier = modifier, Value = value, Persona = persona,
            };
        }
    }

    public static class D5CharacterCatalog
    {
        public static readonly IReadOnlyList<D5CharacterPresetDefinition> All = new List<D5CharacterPresetDefinition>
        {
            Preset("dockhand_male", "码头水手", "Dockhand", "sun_bronzed", "short", "brown", "deckhand", 1.00f, 1.00f, 0x3A78A1),
            Preset("navigator_female", "女领航员", "Navigator", "warm_brown", "braid", "black", "navigator", 0.98f, 0.96f, 0xA56B3F),
            Preset("privateer", "私掠船长", "Privateer", "weathered", "ponytail", "black", "privateer", 1.04f, 1.05f, 0x9A3D36),
            Preset("merchant", "商人", "Merchant", "olive", "curly", "auburn", "merchant", 1.02f, 1.02f, 0xB28C45),
            Preset("island_scout", "岛屿向导", "Island Scout", "deep_brown", "coils", "black", "scout", 0.97f, 0.98f, 0x4A9A71),
            Preset("navy_officer", "海军军官", "Navy Officer", "pale", "tied", "brown", "navy", 1.03f, 1.04f, 0x4560A4),
            Preset("old_sailor", "老水手", "Old Sailor", "sun_bronzed", "balding", "gray", "old_sailor", 1.00f, 0.94f, 0x8D6E63),
            Preset("scholar", "自然学者", "Naturalist", "warm_brown", "loose", "dark_brown", "scholar", 1.01f, 1.00f, 0x7C5AA6),
        };

        public static D5CharacterPresetDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var preset in All) if (preset.Id == id) return preset;
            return null;
        }

        private static D5CharacterPresetDefinition Preset(string id, string zh, string en, string skin, string hair,
            string hairColor, string outfit, float face, float body, int accent)
        {
            return new D5CharacterPresetDefinition
            {
                Id = id, NameZh = zh, NameEn = en, SkinTone = skin, HairStyle = hair,
                HairColor = hairColor, Outfit = outfit, FaceScale = face, BodyScale = body, AccentColor = accent,
            };
        }
    }

    public static class D5AchievementCatalog
    {
        public static readonly IReadOnlyList<D5AchievementDefinition> All = new List<D5AchievementDefinition>
        {
            Achievement("first_voyage", "第一次航行", "First Voyage", "完成第一次航程。", "Complete your first voyage."),
            Achievement("discover_all_islands", "海图全开", "Chart the Caribbean", "发现全部 12 个岛屿。", "Discover all 12 islands."),
            Achievement("trait_collector", "百态水手", "Many Kinds of Sailors", "见证 10 种不同特质。", "Witness 10 different traits."),
            Achievement("customized_captain", "船长肖像", "Captain's Portrait", "选择一个基础角色预设。", "Choose a character preset."),
            Achievement("first_trade", "第一笔交易", "First Trade", "完成一次合法贸易。", "Complete a legal trade."),
            Achievement("privateer_victory", "合法猎手", "Licensed Hunter", "持私掠证赢得一次炮战。", "Win a battle with a privateer permit."),
            Achievement("medical_ready", "医务整备", "Medical Ready", "完成一次船员治疗。", "Treat a crew member."),
            Achievement("mainline_complete", "航线终点", "End of the Line", "完成三章主线教学。", "Complete the three-chapter tutorial."),
        };

        public static D5AchievementDefinition Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var achievement in All) if (achievement.Id == id) return achievement;
            return null;
        }

        private static D5AchievementDefinition Achievement(string id, string zh, string en, string descriptionZh, string descriptionEn)
        {
            return new D5AchievementDefinition
            {
                Id = id, NameZh = zh, NameEn = en,
                DescriptionZh = descriptionZh, DescriptionEn = descriptionEn,
            };
        }
    }
}
