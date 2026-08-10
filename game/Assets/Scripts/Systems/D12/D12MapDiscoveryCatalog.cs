// ============================================================
// D12MapDiscoveryCatalog —— D12 纵切片海图目录
//
// 这里仅登记世界中可被探索系统裁决的地图特征，不代表玩家已经知道它们。
// 显示和存档都必须经过 D12MapDiscoverySystem 的状态/快照接口。
// ============================================================

using System.Collections.Generic;
using Whalebone.Systems.D9;

namespace Whalebone.Systems.D12
{
    public static class D12MapDiscoveryCatalog
    {
        public const string UnknownCoastId = "coast_unknown_green_cay";
        public const string ReefId = "reef_saint_marys_east";
        public const string ShoalId = "shoal_saint_marys_bar";
        public const string FishingGroundId = D9FishingIds.FishingSpotId;
        public const string SafeChannelId = "channel_saint_marys_inner";
        public const string WreckId = D12SalvageIds.WreckMapFeatureId;
        public const string CaveEntranceId = D12SalvageIds.CaveMapFeatureId;

        public static IReadOnlyList<D12MapFeatureDefinition> CreateVerticalSlice()
        {
            return new[]
            {
                new D12MapFeatureDefinition
                {
                    Id = UnknownCoastId,
                    Type = D12MapFeatureType.UnknownCoast,
                    Layer = D12MapLayer.Coastline,
                    LocationRef = "green_cay",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "绿湾未测海岸",
                    ContentRef = "island_saint_marys",
                    TrueDepthMeters = 0f,
                    IsHazard = false,
                },
                new D12MapFeatureDefinition
                {
                    Id = ReefId,
                    Type = D12MapFeatureType.Reef,
                    Layer = D12MapLayer.NavigationHazards,
                    LocationRef = "nearshore_saint_marys",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "圣玛丽东礁",
                    ContentRef = "island_saint_marys",
                    TrueDepthMeters = 1.6f,
                    IsHazard = true,
                },
                new D12MapFeatureDefinition
                {
                    Id = ShoalId,
                    Type = D12MapFeatureType.Shoal,
                    Layer = D12MapLayer.Bathymetry,
                    LocationRef = "nearshore_saint_marys",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "圣玛丽沙洲",
                    ContentRef = "island_saint_marys",
                    TrueDepthMeters = 2.4f,
                    IsHazard = true,
                },
                new D12MapFeatureDefinition
                {
                    Id = FishingGroundId,
                    Type = D12MapFeatureType.FishingGround,
                    Layer = D12MapLayer.Ecology,
                    LocationRef = D9FishingIds.FishingSpotId,
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "近海鲭鱼鱼场",
                    ContentRef = D9FishingIds.FishSchoolId,
                    TrueDepthMeters = 2.0f,
                    IsHazard = false,
                },
                new D12MapFeatureDefinition
                {
                    Id = SafeChannelId,
                    Type = D12MapFeatureType.SafeChannel,
                    Layer = D12MapLayer.Bathymetry,
                    LocationRef = "santa_margarita_green_cay_channel",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "圣玛丽内航道",
                    ContentRef = "island_saint_marys",
                    TrueDepthMeters = 4.8f,
                    IsHazard = false,
                },
                new D12MapFeatureDefinition
                {
                    Id = WreckId,
                    Type = D12MapFeatureType.Wreck,
                    Layer = D12MapLayer.ExplorationSites,
                    LocationRef = "landmark:saint_marys_chart_cove",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "圣玛丽海图湾沉船",
                    ContentRef = "wreck_saint_marys_chart_cove",
                    TrueDepthMeters = 3.1f,
                    IsHazard = true,
                },
                new D12MapFeatureDefinition
                {
                    Id = CaveEntranceId,
                    Type = D12MapFeatureType.CaveEntrance,
                    Layer = D12MapLayer.ExplorationSites,
                    LocationRef = "landmark:green_cay_tide_cave",
                    RegionRef = "green_cay",
                    DisplayName = "绿湾潮汐洞入口",
                    ContentRef = "cave_green_cay_tide_cave",
                    TrueDepthMeters = 0f,
                    IsHazard = true,
                },
            };
        }
    }
}
