// D13-A05 岛屿治理代表目录。历史/宗教/社群字段显式带标签和审查状态。

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D13
{
    public static class D13IslandGovernanceCatalog
    {
        public const string SaintMarysIslandId = "island_saint_marys";
        public const string SaintMarysPropertyId = "property_estate_saint_marys";
        public const string HouseholdAnchor = "household_anchor";
        public const string JobDockhand = "job_dockhand";
        public const string JobTeacher = "job_teacher";
        public const string JobGuard = "job_guard";
        public const string JobFisher = "job_fisher";

        private static readonly IReadOnlyList<D13IslandGovernanceDefinition> Islands = new List<D13IslandGovernanceDefinition>
        {
            new D13IslandGovernanceDefinition
            {
                IslandId = SaintMarysIslandId, DisplayNameKey = "content.island.island_saint_marys.governance", PropertyId = SaintMarysPropertyId,
                WaterServiceRef = "water_service_saint_marys", FoodServiceRef = "food_store_saint_marys", CommunityAgreementId = "community_green_cay",
                ReligionContentId = "religion_catholic_parish_saint_marys", DefaultReligionArrangementId = "religion_catholic_parish",
                HousingCapacity = 8, EducationCapacity = 3, SecurityBase = 72, FoodUnitsPerResident = 2, WaterUnitsPerResident = 2, DefaultTaxRateBps = 800,
                Tags = new List<string> { "port", "mixed_population", "governance", "1675_caribbean" },
                Content = Content("island_saint_marys_governance", "inferred", "colonial_port_governance", "涉及港口权力、居民权利和历史制度的治理投影。", D13GovernanceContentReviewStatus.ApprovedWithWarning, "D13-A05 岛屿治理样例", "island", "governance", "historical_context"),
                ReligionContent = Content("religion_catholic_parish_saint_marys", "inferred", "religious_context", "宗教节期、饮食、葬礼和承诺的语境化规则，不把信仰映射成善恶。", D13GovernanceContentReviewStatus.ApprovedWithWarning, "D10 角色/宗教设计与 D13-A05 审查样例", "religion", "parish", "ritual", "sensitive_reviewed"),
            },
        };

        private static readonly IReadOnlyList<D13GovernanceJobDefinition> Jobs = new List<D13GovernanceJobDefinition>
        {
            Job(JobDockhand, "content.job.dockhand", 4, 12, 0, 3, "dock", "labor"),
            Job(JobTeacher, "content.job.teacher", 1, 16, 2, 0, "education", "school"),
            Job(JobGuard, "content.job.guard", 2, 14, 1, 14, "security", "watch"),
            Job(JobFisher, "content.job.fisher", 3, 13, 0, 2, "fishing", "food"),
        };

        public static IReadOnlyList<D13IslandGovernanceDefinition> AllIslands { get { return Islands; } }
        public static IReadOnlyList<D13GovernanceJobDefinition> AllJobs { get { return Jobs; } }

        public static D13IslandGovernanceDefinition FindIsland(string islandId)
        {
            foreach (var item in Islands) if (item != null && item.IslandId == islandId) return item;
            return null;
        }

        public static D13GovernanceJobDefinition FindJob(string jobId)
        {
            foreach (var item in Jobs) if (item != null && item.JobId == jobId) return item;
            return null;
        }

        private static D13GovernanceJobDefinition Job(string id, string displayNameKey, int slots, int wage, int education, int security, params string[] tags)
        {
            return new D13GovernanceJobDefinition { JobId = id, DisplayNameKey = displayNameKey, Slots = slots, WagePerDay = wage, RequiredEducation = education, SecurityContribution = security, Tags = new List<string>(tags) };
        }

        private static D13GovernanceContentTag Content(string id, string historicalTag, string sensitiveLabel, string warning, D13GovernanceContentReviewStatus status, string sourceNote, params string[] tags)
        {
            return new D13GovernanceContentTag { ContentId = id, HistoricalTag = historicalTag, SensitiveContentLabel = sensitiveLabel, ContentWarning = warning, ReviewStatus = status, SourceNote = sourceNote, Tags = new List<string>(tags) };
        }
    }
}
