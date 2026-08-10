// D12-A04 代表性社群目录。定义包含社会制度、边界、来源和审查状态；
// 运行时库存和声望由 D12CommunityTradeSystem 另行持有。

using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public static class D12CommunityTradeCatalog
    {
        public static IReadOnlyList<D12CommunityDefinition> Communities
        {
            get { return CreateVerticalSlice(); }
        }

        public static IReadOnlyList<D12CommunityBoundaryDefinition> Boundaries
        {
            get { return CreateBoundaries(); }
        }

        public static IReadOnlyList<D12CommunityDefinition> CreateVerticalSlice()
        {
            return new List<D12CommunityDefinition>
            {
                new D12CommunityDefinition
                {
                    Id = D12CommunityTradeIds.GreenCayCommunityId,
                    DisplayNameKey = "community.green_cay.name",
                    SourceNote = "D12 representative fictional local community; exchange norms are a gameplay model, not a claim about a real people.",
                    HistoricalTag = "fictional",
                    TestId = "D12-A04-COMMUNITY-GREEN-CAY",
                    SensitiveContentLabel = "colonial_contact_context",
                    ContentWarning = "包含殖民接触、土地边界和历史不对称的提示；社群不是可无限榨取的商店。",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                    SocialInstitution = "自治渔猎社群；以互惠、长老见证和保护区规约组织交换",
                    RequiredLanguageId = "green_cay_trade_tongue",
                    RequiredLanguageLevel = 2,
                    RequiredKnowledgeLevel = 2,
                    RequiredEtiquette = D12CommunityEtiquette.GiftBeforeTrade,
                    LandBoundaryId = "boundary_green_cay_landing",
                    ProtectedAreaId = "reserve_green_cay_mangrove",
                    InitialStock = new Dictionary<string, int>
                    {
                        ["salt"] = 4,
                        ["medicine"] = 2,
                        ["cocoa"] = 3,
                    },
                },
                new D12CommunityDefinition
                {
                    Id = D12CommunityTradeIds.FreeCoveCommunityId,
                    DisplayNameKey = "community.free_cove.name",
                    SourceNote = "D12 representative fictional free settlement; exchange norms are a gameplay model, not a claim about a real people.",
                    HistoricalTag = "fictional",
                    TestId = "D12-A04-COMMUNITY-FREE-COVE",
                    SensitiveContentLabel = "forced_labor_and_flight_context",
                    ContentWarning = "涉及逃亡者、劳动制度和土地安全；内容可跳过且不将身份处境商品化。",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                    SocialInstitution = "自由社群；由互助承诺、避难规约和公开见证维持交换",
                    RequiredLanguageId = "free_cove_trade_tongue",
                    RequiredLanguageLevel = 1,
                    RequiredKnowledgeLevel = 1,
                    RequiredEtiquette = D12CommunityEtiquette.FormalGreeting,
                    LandBoundaryId = "boundary_free_cove_quay",
                    ProtectedAreaId = "reserve_free_cove_wetland",
                    InitialStock = new Dictionary<string, int>
                    {
                        ["timber"] = 2,
                        ["spices"] = 2,
                        ["rum"] = 1,
                    },
                },
            };
        }

        public static IReadOnlyList<D12CommunityBoundaryDefinition> CreateBoundaries()
        {
            return new List<D12CommunityBoundaryDefinition>
            {
                new D12CommunityBoundaryDefinition
                {
                    Id = "boundary_green_cay_landing",
                    CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                    Kind = D12CommunityBoundaryKind.LandBoundary,
                    LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                    RequiresConsent = false,
                    TradeAllowed = true,
                    HistoricalTag = "fictional",
                    SensitiveContentLabel = "land_rights_context",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                },
                new D12CommunityBoundaryDefinition
                {
                    Id = "reserve_green_cay_mangrove",
                    CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                    Kind = D12CommunityBoundaryKind.ProtectedArea,
                    LocationRef = D12CommunityTradeIds.GreenCayMangroveLocation,
                    RequiresConsent = true,
                    TradeAllowed = false,
                    HistoricalTag = "fictional",
                    SensitiveContentLabel = "protected_habitat_context",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                },
                new D12CommunityBoundaryDefinition
                {
                    Id = "boundary_free_cove_quay",
                    CommunityId = D12CommunityTradeIds.FreeCoveCommunityId,
                    Kind = D12CommunityBoundaryKind.LandBoundary,
                    LocationRef = D12CommunityTradeIds.FreeCoveTradeLocation,
                    RequiresConsent = false,
                    TradeAllowed = true,
                    HistoricalTag = "fictional",
                    SensitiveContentLabel = "land_rights_context",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                },
                new D12CommunityBoundaryDefinition
                {
                    Id = "reserve_free_cove_wetland",
                    CommunityId = D12CommunityTradeIds.FreeCoveCommunityId,
                    Kind = D12CommunityBoundaryKind.ProtectedArea,
                    LocationRef = "free_cove_wetland_reserve",
                    RequiresConsent = true,
                    TradeAllowed = false,
                    HistoricalTag = "fictional",
                    SensitiveContentLabel = "protected_habitat_context",
                    ContentReviewStatus = D12CommunityContentReviewStatus.ApprovedWithWarning,
                },
            };
        }

        public static D12CommunityDefinition Find(string communityId)
        {
            foreach (var community in CreateVerticalSlice())
                if (community.Id == communityId) return community;
            return null;
        }
    }
}
