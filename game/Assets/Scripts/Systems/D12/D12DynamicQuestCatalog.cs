// ============================================================
// D12DynamicQuestCatalog —— D12-A08 代表性动态任务定义
// ============================================================

using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12DynamicQuestCatalog
    {
        public const string ChartRecoveryQuestId = "quest_d12_saint_marys_chart_recovery";
        public const string CommunityReciprocityQuestId = "quest_d12_green_cay_reciprocity";

        public const string ChartLocatedObjectiveId = "chart_site_located";
        public const string ChartSecuredObjectiveId = "chart_evidence_secured";
        public const string CommunityHelpObjectiveId = "community_help_recorded";

        public const string CommunityChartRouteId = "route_community_chart_witness";
        public const string ReturnToHarborRouteId = "route_return_to_harbor_and_retry";

        public static List<D12DynamicQuestDefinition> CreateVerticalSlice()
        {
            return new List<D12DynamicQuestDefinition>
            {
                CreateChartRecovery(),
                CreateCommunityReciprocity(),
            };
        }

        public static D12DynamicQuestDefinition CreateChartRecovery()
        {
            var result = new D12DynamicQuestDefinition
            {
                QuestId = ChartRecoveryQuestId,
                DisplayNameKey = "quest.d12.saint_marys_chart_recovery.name",
                Title = "圣玛丽航海图的两条路",
                Description = "先确认海图地点，再通过打捞或绿礁村见证保留可追溯的航海证据。",
                Kind = D12DynamicQuestKind.Mainline,
                RewardId = "reward_d12_saint_marys_chart_evidence",
                RewardType = D12QuestRewardType.MapKnowledge,
                DefaultRecoveryRouteId = ReturnToHarborRouteId,
                FailureResultCodes = new List<string> { "salvage_risk_failed", "competitor_interference" },
                RecoveryResultCodes = new List<string> { "salvage_recovered" },
            };
            result.Objectives.Add(new D12QuestObjectiveDefinition
            {
                ObjectiveId = ChartLocatedObjectiveId,
                Description = "确认圣玛丽海图湾的地图位置。",
                TargetOwnerSystem = "ExplorationSystem",
                AcceptedResultCodes = new List<string> { "map_discovery_recorded" },
                MatchValueKey = "feature_id",
                MatchValue = "wreck_saint_marys_chart_cove",
            });
            var secured = new D12QuestObjectiveDefinition
            {
                ObjectiveId = ChartSecuredObjectiveId,
                Description = "取得可复核的航海图证据。",
                TargetOwnerSystem = "ExplorationSystem",
                AcceptedResultCodes = new List<string> { "salvage_success" },
                MatchValueKey = "site_id",
                MatchValue = "wreck_saint_marys_chart_cove",
            };
            secured.AlternativeAcceptances.Add(new D12QuestObjectiveAlternative
            {
                RouteId = CommunityChartRouteId,
                TargetOwnerSystem = D12CommunityTradeIds.SystemOwner,
                MatchValueKey = "community_id",
                MatchValue = "community_green_cay",
                AcceptedResultCodes = new List<string> { "community_trade_completed", "community_help_offered" },
            });
            result.Objectives.Add(secured);
            result.AlternativeRoutes.Add(new D12QuestAlternativeRouteDefinition
            {
                RouteId = ReturnToHarborRouteId,
                Action = "return_to_harbor_and_reprepare",
                Description = "回到已知港口补齐装备或等待下一次航行窗口，再沿原目标重试。",
                ResumeObjectiveId = ChartLocatedObjectiveId,
            });
            result.AlternativeRoutes.Add(new D12QuestAlternativeRouteDefinition
            {
                RouteId = CommunityChartRouteId,
                Action = "ask_green_cay_chart_witness",
                Description = "放弃本次危险打捞，改向绿礁村获取社群见证或帮助事实。",
                ResumeObjectiveId = ChartSecuredObjectiveId,
            });
            result.Validate();
            return result;
        }

        public static D12DynamicQuestDefinition CreateCommunityReciprocity()
        {
            var result = new D12DynamicQuestDefinition
            {
                QuestId = CommunityReciprocityQuestId,
                DisplayNameKey = "quest.d12.green_cay.reciprocity.name",
                Title = "绿礁村的互惠记录",
                Description = "通过统一社群事实记录一次帮助和一次互惠交换。",
                Kind = D12DynamicQuestKind.Side,
                RewardId = "reward_d12_green_cay_reciprocity",
                RewardType = D12QuestRewardType.Reputation,
            };
            result.Objectives.Add(new D12QuestObjectiveDefinition
            {
                ObjectiveId = "community_help",
                Description = "取得绿礁村的帮助事实。",
                TargetOwnerSystem = D12CommunityTradeIds.SystemOwner,
                AcceptedResultCodes = new List<string> { "community_help_offered" },
                MatchValueKey = "community_id",
                MatchValue = "community_green_cay",
            });
            result.Objectives.Add(new D12QuestObjectiveDefinition
            {
                ObjectiveId = "community_trade",
                Description = "完成一次绿礁村互惠交换。",
                TargetOwnerSystem = D12CommunityTradeIds.SystemOwner,
                AcceptedResultCodes = new List<string> { "community_trade_completed" },
                MatchValueKey = "community_id",
                MatchValue = "community_green_cay",
            });
            result.Validate();
            return result;
        }
    }
}
