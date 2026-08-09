using System.Collections.Generic;

namespace Whalebone.Systems.D5
{
    /// <summary>D5 目录和 UI 的中英文单一事实源。</summary>
    public static class D5LocalizationSystem
    {
        private static readonly Dictionary<string, string> Zh = new Dictionary<string, string>
        {
            { "d5_title", "D5 打磨上架" }, { "d5_world", "加勒比海图" }, { "d5_islands", "岛屿" },
            { "d5_traits", "船长特质" }, { "d5_customization", "角色预设" }, { "d5_performance", "性能" },
            { "d5_steam", "Steam 桥接" }, { "d5_discover", "发现下一个岛" }, { "d5_visit", "驶向当前岛" },
            { "d5_preset_next", "切换角色预设" }, { "d5_language", "切换中英" }, { "d5_quality", "切换画质" },
            { "d5_achievement", "解锁成就" }, { "d5_save", "保存 D5 状态" }, { "d5_target", "目标 1920×1080 / 60 FPS" },
        };

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "d5_title", "D5 Release Polish" }, { "d5_world", "Caribbean Chart" }, { "d5_islands", "Islands" },
            { "d5_traits", "Captain Traits" }, { "d5_customization", "Character Preset" }, { "d5_performance", "Performance" },
            { "d5_steam", "Steam Bridge" }, { "d5_discover", "Discover Next Island" }, { "d5_visit", "Sail to Current Island" },
            { "d5_preset_next", "Cycle Character Preset" }, { "d5_language", "Toggle Chinese/English" }, { "d5_quality", "Cycle Quality" },
            { "d5_achievement", "Unlock Achievement" }, { "d5_save", "Save D5 State" }, { "d5_target", "Target 1920x1080 / 60 FPS" },
        };

        public static string Get(string key, string language)
        {
            string value;
            var table = language == "en" ? En : Zh;
            if (table.TryGetValue(key, out value)) return value;
            if (key != null && key.StartsWith("island_"))
            {
                var island = D5IslandCatalog.Find(key.Substring("island_".Length));
                if (island != null) return language == "en" ? island.NameEn : island.NameZh;
            }
            if (key != null && key.StartsWith("trait_"))
            {
                var trait = D5TraitCatalog.Find(key.Substring("trait_".Length));
                if (trait != null) return language == "en" ? trait.NameEn : trait.NameZh;
            }
            if (key != null && key.StartsWith("preset_"))
            {
                var preset = D5CharacterCatalog.Find(key.Substring("preset_".Length));
                if (preset != null) return language == "en" ? preset.NameEn : preset.NameZh;
            }
            return key ?? "";
        }

        public static IReadOnlyList<string> AllKeys()
        {
            var keys = new List<string>(Zh.Keys);
            foreach (var island in D5IslandCatalog.All) keys.Add("island_" + island.Id);
            foreach (var trait in D5TraitCatalog.All) keys.Add("trait_" + trait.Id);
            foreach (var preset in D5CharacterCatalog.All) keys.Add("preset_" + preset.Id);
            return keys;
        }
    }
}
