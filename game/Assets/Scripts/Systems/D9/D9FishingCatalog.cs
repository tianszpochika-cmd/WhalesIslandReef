// ============================================================
// D9FishingCatalog —— D9-A02 一个鱼群、一个捕捞点和一件渔具
// ============================================================

using System.Collections.Generic;
using Whalebone.Systems.D3;

namespace Whalebone.Systems.D9
{
    public static class D9FishingCatalog
    {
        public static readonly FishingGearDefinition Handline = new FishingGearDefinition
        {
            Id = D9FishingIds.GearId,
            ContentId = D9FishingIds.GearContentId,
            DisplayName = "基础手线",
            CatchCapacity = 2,
        };

        public static readonly FishingSpotDefinition NearshoreSaintMarys = new FishingSpotDefinition
        {
            Id = D9FishingIds.FishingSpotId,
            FishSchoolId = D9FishingIds.FishSchoolId,
            Habitat = "nearshore/reef_edge",
            WorldX = -4.0f,
            WorldZ = 8.0f,
        };

        public static readonly D3MarketItemDefinition FreshMackerel =
            new D3MarketItemDefinition(D9FishingIds.FreshFishItemId, "鲜鲭鱼", 11, 1, 1, 42);
        public static readonly D3MarketItemDefinition SaltedMackerel =
            new D3MarketItemDefinition(D9FishingIds.SaltedFishItemId, "盐渍鲭鱼", 17, 1, 1, 36);

        public static FishingGearDefinition FindGear(string id)
        {
            return Handline.Id == id ? Handline : null;
        }

        public static FishingSpotDefinition FindSpot(string id)
        {
            return NearshoreSaintMarys.Id == id ? NearshoreSaintMarys : null;
        }

        public static IReadOnlyList<FishingGearDefinition> Gears
        {
            get { return new[] { Handline }; }
        }

        public static IReadOnlyList<FishingSpotDefinition> Spots
        {
            get { return new[] { NearshoreSaintMarys }; }
        }
    }
}
