// ============================================================
// D12WreckCaveSalvageCatalog —— 沉船/洞穴纵切片内容
//
// 内容定义不等于玩家已知状态；位置必须先通过 D12-A01 海图发现读模型
// 出现，运行时尝试和失败状态再由 D12WreckCaveSalvageSystem 写入。
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12WreckCaveSalvageCatalog
    {
        public const string WreckSiteId = "wreck_saint_marys_chart_cove";
        public const string CaveSiteId = "cave_green_cay_tide_cave";
        public const string WreckRewardRequestId = "reward_wreck_saint_marys_chart";
        public const string CaveRewardRequestId = "reward_green_cay_chart_cache";

        public static IReadOnlyList<D12ExplorationSiteDefinition> CreateVerticalSlice()
        {
            return new[]
            {
                new D12ExplorationSiteDefinition
                {
                    SiteId = WreckSiteId,
                    Type = D12ExplorationSiteType.Wreck,
                    MapFeatureId = D12SalvageIds.WreckMapFeatureId,
                    LocationRef = "landmark:saint_marys_chart_cove",
                    RegionRef = "saint_marys_nearshore",
                    DisplayName = "圣玛丽海图湾沉船",
                    ClaimRef = "claim:wreck_saint_marys_english",
                    RewardRequestId = WreckRewardRequestId,
                    CompetitorRef = "competitor:santa_marys_privateer",
                    PreferredTideStage = D12TideStage.Low,
                    MinimumRopeMeters = 24,
                    RequiredLightHours = 0,
                    BaseRiskPercent = 22,
                    CollapseRiskPercent = 0,
                    WaterRiskPercent = 24,
                    CompetitorRiskPercent = 18,
                    RequiredEquipmentIds = new List<string>
                    {
                        D12SalvageIds.GrapnelToolId,
                        D12SalvageIds.RopeToolId,
                    },
                    RecoveryOptions = new List<D12SalvageRecoveryOption>
                    {
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_wreck_wait_for_tide",
                            Action = "wait_for_tide",
                            Description = "等待下一次低潮窗口，再从已知锚点重新布置绳索。",
                            RequiredTideStage = D12TideStage.Low,
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_wreck_prepare_equipment",
                            Action = "prepare_equipment",
                            Description = "回港补充抓钩、绳索或抽水泵后重试。",
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_wreck_legal_review",
                            Action = "legal_review",
                            Description = "带着航海日志和归属证据回到法律/势力机构复核打捞权。",
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_wreck_return_to_harbor",
                            Action = "return_to_harbor",
                            Description = "回到已知港口，保留残骸状态和失败事实，之后可重新准备。",
                        },
                    },
                },
                new D12ExplorationSiteDefinition
                {
                    SiteId = CaveSiteId,
                    Type = D12ExplorationSiteType.Cave,
                    MapFeatureId = D12SalvageIds.CaveMapFeatureId,
                    LocationRef = "landmark:green_cay_tide_cave",
                    RegionRef = "green_cay",
                    DisplayName = "绿湾潮汐洞",
                    ClaimRef = "claim:green_cay_community_cave",
                    RewardRequestId = CaveRewardRequestId,
                    CompetitorRef = "competitor:green_cay_guides",
                    PreferredTideStage = D12TideStage.Low,
                    MinimumRopeMeters = 30,
                    RequiredLightHours = 4,
                    BaseRiskPercent = 18,
                    CollapseRiskPercent = 28,
                    WaterRiskPercent = 18,
                    CompetitorRiskPercent = 12,
                    RequiredEquipmentIds = new List<string>
                    {
                        D12SalvageIds.RopeToolId,
                        D12SalvageIds.LanternToolId,
                        D12SalvageIds.OilToolId,
                    },
                    RecoveryOptions = new List<D12SalvageRecoveryOption>
                    {
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_cave_return_to_entrance",
                            Action = "return_to_entrance",
                            Description = "沿绳索标记回到洞穴入口；保留照明消耗、塌方和落水风险记录。",
                            RequiredEquipmentId = D12SalvageIds.RopeToolId,
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_cave_wait_for_tide",
                            Action = "wait_for_tide",
                            Description = "在入口外等待低潮，避免潮汐浪涌再次封住回头路线。",
                            RequiredTideStage = D12TideStage.Low,
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_cave_prepare_equipment",
                            Action = "prepare_equipment",
                            Description = "补充灯油、备用绳索和洞穴标记，再从入口重新进入。",
                        },
                        new D12SalvageRecoveryOption
                        {
                            RouteId = "route_cave_legal_review",
                            Action = "legal_review",
                            Description = "请地方社群或势力机构复核洞穴保护规则和进入许可。",
                        },
                    },
                },
            };
        }
    }
}
