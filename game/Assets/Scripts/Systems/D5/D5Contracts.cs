// ============================================================
// D5Contracts —— D5 打磨上架阶段的纯 C# 数据契约
//
// 强逻辑层不引用 Unity、网络或 LLM。Unity、Server 镜像和 SaveSystem
// 只能通过这些快照/结果交换数据。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D5
{
    public enum D5IslandType { TownPort, Village, Wilderness, Special, Fortress }
    public enum D5Biome { Mangrove, Palm, Reef, Rainforest, Beach }
    public enum D5TraitCategory { Body, Personality, Background, Disease }
    public enum D5QualityLevel { Low, Medium, High }

    [Serializable]
    public sealed class D5IslandDefinition
    {
        public string Id;
        public string NameZh;
        public string NameEn;
        public D5IslandType Type;
        public D5Biome Biome;
        public string FactionId;
        public int Danger;
        public float TradeMultiplier;
        public float MapX;
        public float MapY;
    }

    [Serializable]
    public sealed class D5WorldState
    {
        public string CurrentIslandId = "santa_margarita";
        public List<string> DiscoveredIslands = new List<string> { "santa_margarita", "green_cay", "driftwood_isle" };
        public List<string> VisitedIslands = new List<string> { "santa_margarita" };
        public int MapRevision;

        public D5WorldState Clone()
        {
            var result = new D5WorldState
            {
                CurrentIslandId = CurrentIslandId,
                MapRevision = Math.Max(0, MapRevision),
                DiscoveredIslands = new List<string>(),
                VisitedIslands = new List<string>(),
            };
            CopyUnique(DiscoveredIslands, result.DiscoveredIslands);
            CopyUnique(VisitedIslands, result.VisitedIslands);
            return result;
        }

        private static void CopyUnique(List<string> source, List<string> target)
        {
            if (source == null) return;
            foreach (var value in source)
                if (!string.IsNullOrEmpty(value) && !target.Contains(value)) target.Add(value);
        }
    }

    [Serializable]
    public sealed class D5TraitDefinition
    {
        public string Id;
        public string NameZh;
        public string NameEn;
        public D5TraitCategory Category;
        public string Modifier;
        public int Value;
        public string Persona;
    }

    [Serializable]
    public sealed class D5CharacterPresetDefinition
    {
        public string Id;
        public string NameZh;
        public string NameEn;
        public string SkinTone;
        public string HairStyle;
        public string HairColor;
        public string Outfit;
        public float FaceScale = 1f;
        public float BodyScale = 1f;
        public int AccentColor;
    }

    [Serializable]
    public sealed class D5CharacterState
    {
        public string PresetId = "dockhand_male";
        public string SkinTone = "sun_bronzed";
        public string HairStyle = "short";
        public string HairColor = "brown";
        public string Outfit = "deckhand";
        public float FaceScale = 1f;
        public float BodyScale = 1f;

        public D5CharacterState Clone()
        {
            return new D5CharacterState
            {
                PresetId = PresetId,
                SkinTone = SkinTone,
                HairStyle = HairStyle,
                HairColor = HairColor,
                Outfit = Outfit,
                FaceScale = FaceScale,
                BodyScale = BodyScale,
            };
        }
    }

    [Serializable]
    public sealed class D5PerformanceState
    {
        public D5QualityLevel Quality = D5QualityLevel.Medium;
        public int TargetWidth = 1920;
        public int TargetHeight = 1080;
        public int TargetFps = 60;
        public float LastFps;
        public float MinFps;
        public float AverageFps;
        public int SampleCount;
        public bool MeetsTarget;

        public D5PerformanceState Clone()
        {
            return new D5PerformanceState
            {
                Quality = Quality,
                TargetWidth = TargetWidth,
                TargetHeight = TargetHeight,
                TargetFps = TargetFps,
                LastFps = LastFps,
                MinFps = MinFps,
                AverageFps = AverageFps,
                SampleCount = SampleCount,
                MeetsTarget = MeetsTarget,
            };
        }
    }

    [Serializable]
    public sealed class D5PerformanceReport
    {
        public bool MeetsTarget;
        public int TargetWidth;
        public int TargetHeight;
        public int TargetFps;
        public float LastFps;
        public float MinFps;
        public float AverageFps;
        public int SampleCount;
        public string Message;
    }

    [Serializable]
    public sealed class D5AchievementDefinition
    {
        public string Id;
        public string NameZh;
        public string NameEn;
        public string DescriptionZh;
        public string DescriptionEn;
    }

    [Serializable]
    public sealed class D5SteamState
    {
        public string AppId = "UNCONFIGURED";
        public bool OfflineMode = true;
        public bool CloudEnabled = true;
        public int PendingCloudRevision;
        public string LastSync = "never";
        public List<string> UnlockedAchievements = new List<string>();

        public D5SteamState Clone()
        {
            var result = new D5SteamState
            {
                AppId = AppId,
                OfflineMode = OfflineMode,
                CloudEnabled = CloudEnabled,
                PendingCloudRevision = Math.Max(0, PendingCloudRevision),
                LastSync = LastSync,
                UnlockedAchievements = new List<string>(),
            };
            if (UnlockedAchievements != null)
                foreach (var id in UnlockedAchievements)
                    if (!string.IsNullOrEmpty(id) && !result.UnlockedAchievements.Contains(id)) result.UnlockedAchievements.Add(id);
            return result;
        }
    }

    [Serializable]
    public sealed class D5RuntimeSnapshot
    {
        public D5WorldState World = new D5WorldState();
        public D5CharacterState Character = new D5CharacterState();
        public D5PerformanceState Performance = new D5PerformanceState();
        public D5SteamState Steam = new D5SteamState();
        public string Language = "zh";
        public List<string> PlayerTraitIds = new List<string>();
        public Dictionary<string, List<string>> CrewTraitIds = new Dictionary<string, List<string>>();

        public D5RuntimeSnapshot Clone()
        {
            var result = new D5RuntimeSnapshot
            {
                World = World == null ? new D5WorldState() : World.Clone(),
                Character = Character == null ? new D5CharacterState() : Character.Clone(),
                Performance = Performance == null ? new D5PerformanceState() : Performance.Clone(),
                Steam = Steam == null ? new D5SteamState() : Steam.Clone(),
                Language = Language == "en" ? "en" : "zh",
                PlayerTraitIds = new List<string>(),
                CrewTraitIds = new Dictionary<string, List<string>>(),
            };
            if (PlayerTraitIds != null) result.PlayerTraitIds.AddRange(PlayerTraitIds);
            if (CrewTraitIds != null)
                foreach (var pair in CrewTraitIds)
                    result.CrewTraitIds[pair.Key] = pair.Value == null ? new List<string>() : new List<string>(pair.Value);
            return result;
        }
    }
}
