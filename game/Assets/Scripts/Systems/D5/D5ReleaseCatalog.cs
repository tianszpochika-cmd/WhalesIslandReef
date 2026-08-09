using System.Collections.Generic;

namespace Whalebone.Systems.D5
{
    public sealed class D5StoreMetadata
    {
        public string AppName = "WhalesIslandReef";
        public string ShortDescriptionZh = "1675 年加勒比海航海沙盒：经营船只、理解船员、探索 12 座热带岛屿。";
        public string ShortDescriptionEn = "A 1675 Caribbean sailing sandbox about ships, crew, trade and twelve tropical islands.";
        public string AgeRatingNote = "含轻度战斗、酒精描写与历史冲突；不含成人内容。";
        public IReadOnlyList<string> SupportedLanguages = new[] { "zh-CN", "en" };
    }

    public static class D5ReleaseCatalog
    {
        public static D5StoreMetadata StoreMetadata => new D5StoreMetadata();

        public static IReadOnlyList<string> RequiredSteamArtifacts => new List<string>
        {
            "capsule_hero", "header_library", "header_store", "screenshots_01_05", "trailer_60s",
            "achievement_icons_8", "cloud_save_migration_note", "zh_cn_store_copy", "en_store_copy",
        };
    }
}
