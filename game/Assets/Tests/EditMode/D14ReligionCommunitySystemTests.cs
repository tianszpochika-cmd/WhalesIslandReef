// D14-A06：节期、饮食、建筑开放、葬礼、誓言、D10/D12 信任、礼仪、警告/跳过和存档。

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D14;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14ReligionCommunitySystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_religion_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = _dbPath + suffix;
                if (File.Exists(path)) try { File.Delete(path); } catch { }
            }
        }

        [Test]
        public void FestivalDietBuildingFuneralOathEtiquetteAndSaveRestoreAreTraceable()
        {
            var log = CreateSources();
            var authority = new AuthorityFixture();
            var system = new D14ReligionCommunitySystem(log, null, authority, authority, authority, authority);

            var festival = system.ParticipateFestival(Request("festival_01", "festival_action", "content_d14_festival_harbor_lantern", "player", 4));
            Assert.IsTrue(festival.Accepted, festival.Message);
            Assert.AreEqual(D14PerformanceLevel.Contextual, festival.PerformanceLevel);
            Assert.AreEqual(1, system.ReadPractice("player", "religion_harbor_parish").FestivalParticipationCount);

            var dietary = system.PracticeDietaryRule(new D14DietaryRequest
            {
                OperationId = "dietary_01", ActorRef = "player", RuleId = "dietary_fast_day", FoodTag = "plant_staple", ContentId = "content_d14_dietary_fast_day", HistoricalEvidenceEventId = "history_review_dietary", SourceEventId = "dietary_action", WorldDay = 5, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_dietary"),
            });
            Assert.IsTrue(dietary.Accepted, dietary.Message);
            Assert.IsTrue(dietary.Dietary.Compliant);
            Assert.AreEqual(10, dietary.Dietary.ComplianceAfter);

            var building = system.RequestBuildingAccess(new D14BuildingAccessRequest
            {
                OperationId = "building_01", ActorRef = "player", RiteId = "building_rite_chapel_opening", LocalHour = 9, ContentId = "content_d14_building_chapel_opening", HistoricalEvidenceEventId = "history_review_building", SourceEventId = "building_action", WorldDay = 5, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_building"),
            });
            Assert.IsTrue(building.Accepted, building.Message);
            Assert.IsTrue(building.Building.Allowed);
            Assert.AreEqual("building_open", building.Building.BuildingSourceEventId);

            var closedBuilding = new D14BuildingAccessRequest
            {
                OperationId = "building_closed_01", ActorRef = "player", RiteId = "building_rite_chapel_opening", LocalHour = 19, ContentId = "content_d14_building_chapel_opening", HistoricalEvidenceEventId = "history_review_building", SourceEventId = "building_action", WorldDay = 5, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_building"),
            };
            var closed = system.RequestBuildingAccess(closedBuilding);
            Assert.IsFalse(closed.Accepted);
            Assert.AreEqual("building_rite_closed", closed.Code);

            var interaction = system.ResolveCommunityInteraction(new D14CommunityInteractionRequest
            {
                OperationId = "community_interaction_01", ActorRef = "player", CommunityId = "community_green_cay", EtiquetteId = "etiquette_formal_greeting", FestivalId = "festival_harbor_lantern", DietaryRuleId = "dietary_fast_day", ContentId = "content_d14_community_greeting", HistoricalEvidenceEventId = "history_review_community", SourceEventId = "community_action", WorldDay = 5, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_community"),
            });
            Assert.IsTrue(interaction.Accepted, interaction.Message);
            Assert.IsTrue(interaction.CommunityInteraction.Allowed);
            Assert.AreEqual(3, interaction.CommunityInteraction.AppliedTrustDelta, "信任由 D12 authority 返回，A06 只保存投影。");

            var oath = system.ProcessOath(new D14OathRequest
            {
                OperationId = "oath_swear_01", ActorRef = "player", TargetRef = "crew_isaac_reed", OathId = "oath_01", ReligionId = "religion_harbor_parish", TrustDomain = D14TrustDomain.CrewRelation, Action = D14OathAction.Swear, ContentId = "content_d14_oath_mutual_aid", HistoricalEvidenceEventId = "history_review_oath", SourceEventId = "oath_action", WorldDay = 5, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_oath"),
            });
            Assert.IsTrue(oath.Accepted, oath.Message);
            Assert.AreEqual(5, oath.Oath.AppliedTrustDelta);
            Assert.AreEqual("d10_trust", oath.Oath.TrustSourceEventId);

            var fulfilled = system.ProcessOath(new D14OathRequest
            {
                OperationId = "oath_fulfill_01", ActorRef = "player", TargetRef = "crew_isaac_reed", OathId = "oath_01", ReligionId = "religion_harbor_parish", TrustDomain = D14TrustDomain.CrewRelation, Action = D14OathAction.Fulfill, ContentId = "content_d14_oath_mutual_aid", HistoricalEvidenceEventId = "history_review_oath", SourceEventId = "oath_fulfill_action", WorldDay = 6, SaveRevision = 0, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_oath"),
            });
            Assert.IsTrue(fulfilled.Accepted, fulfilled.Message);
            Assert.AreEqual(D14OathStatus.Fulfilled, fulfilled.Oath.Status);
            Assert.AreEqual(8, fulfilled.Oath.AppliedTrustDelta);

            var funeral = system.RecordFuneralRite(new D14FuneralRiteRequest
            {
                OperationId = "funeral_01", ActorRef = "player", FuneralId = "aftercare_funeral_01", RiteId = "funeral_rite_remembrance", ContentId = "content_d14_funeral_remembrance", HistoricalEvidenceEventId = "history_review_funeral", SourceEventId = "funeral_source", WorldDay = 7, SaveRevision = 0, PerformanceLevel = D14PerformanceLevel.LowIntensity, WarningAcknowledged = true,
                EvidenceEventIds = Evidence("history_review_funeral"),
            });
            Assert.IsTrue(funeral.Accepted, funeral.Message);
            Assert.AreEqual("Held", funeral.Funeral.Status);

            var skipped = system.ParticipateFestival(Request("festival_skip_01", "festival_skip_action", "content_d14_festival_harbor_lantern", "player", 4, true));
            Assert.IsTrue(skipped.Accepted, skipped.Message);
            Assert.IsTrue(skipped.Review.Skipped);
            Assert.AreEqual(D14PerformanceLevel.Omitted, skipped.PerformanceLevel);
            Assert.AreEqual(1, system.ReadPractice("player", "religion_harbor_parish").FestivalParticipationCount, "跳过不能伪造参与次数。");

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 宗教社群测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14ReligionCommunityRuntime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14ReligionCommunityRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(saved.Festivals.Count, loaded.Festivals.Count);
            Assert.AreEqual(saved.Reviews.Count, loaded.Reviews.Count);
            Assert.AreEqual(saved.Oaths.Count, loaded.Oaths.Count);
            Assert.AreEqual(saved.CommunityInteractions.Count, loaded.CommunityInteractions.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = new D14ReligionCommunitySystem(null, null, authority, authority, authority, authority, loaded);
            var duplicate = restored.ParticipateFestival(Request("festival_01", "festival_action", "content_d14_festival_harbor_lantern", "player", 4));
            Assert.IsTrue(duplicate.Accepted && duplicate.Duplicate);
            Assert.AreEqual(saved.Revision, restored.Snapshot.Revision);
        }

        [Test]
        public void WarningSkipAndAgentExpressionDoNotWriteFinalReligionOrTrust()
        {
            var log = CreateSources();
            var authority = new AuthorityFixture();
            var system = new D14ReligionCommunitySystem(log, null, authority, authority, authority, authority);
            var expression = system.ProposeExpression(new D14ExpressionProposal { ProposalId = "expression_01", ActorRef = "agent", ContentId = "content_d14_community_greeting", Text = "请以正式问候开始。", Context = "community", WorldDay = 1, PerformanceLevel = D14PerformanceLevel.Contextual });
            Assert.IsNotNull(expression);
            Assert.AreEqual(0, system.Snapshot.Revision);

            var warning = Request("warning_01", "festival_action", "content_d14_festival_harbor_lantern", "player", 4);
            warning.WarningAcknowledged = false;
            var rejectedWarning = system.ParticipateFestival(warning);
            Assert.IsFalse(rejectedWarning.Accepted);
            Assert.AreEqual("warning_required", rejectedWarning.Code);
            Assert.AreEqual(0, system.Snapshot.Revision);

            var agentRequest = Request("agent_direct_01", "festival_action", "content_d14_festival_harbor_lantern", "AgentReply", 4);
            agentRequest.WarningAcknowledged = true;
            var rejectedAgent = system.ParticipateFestival(agentRequest);
            Assert.IsFalse(rejectedAgent.Accepted);
            Assert.AreEqual("agent_cannot_write", rejectedAgent.Code);
            Assert.AreEqual(0, system.Snapshot.Revision);
        }

        private static D14FestivalRequest Request(string operationId, string sourceEventId, string contentId, string actor, int day, bool skip = false)
        {
            return new D14FestivalRequest { OperationId = operationId, ActorRef = actor, FestivalId = "festival_harbor_lantern", ContentId = contentId, HistoricalEvidenceEventId = "history_review_festival", SourceEventId = sourceEventId, WorldDay = day, SaveRevision = 0, WarningAcknowledged = !skip, SkipSensitiveContent = skip, PerformanceLevel = D14PerformanceLevel.Contextual, EvidenceEventIds = Evidence("history_review_festival") };
        }

        private static List<string> Evidence(string historyId) { return new List<string> { historyId }; }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            Add(log, "history_review_festival", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "history_review_dietary", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "history_review_building", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "history_review_community", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "history_review_oath", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "history_review_funeral", D14ReligionCommunityIds.HistoricalReviewOwner);
            Add(log, "festival_action", "QuestSystem"); Add(log, "festival_skip_action", "QuestSystem"); Add(log, "dietary_action", "CrewSystem"); Add(log, "building_action", "UIRequestSystem"); Add(log, "community_action", "CommunityTradeSystem"); Add(log, "oath_action", "DialogueSystem"); Add(log, "oath_fulfill_action", "QuestSystem"); Add(log, "funeral_source", "AftercareSystem");
            Add(log, "building_open", D14ReligionCommunityIds.BuildingOwner); Add(log, "community_funeral", D14ReligionCommunityIds.D12CommunityOwner); Add(log, "d10_trust", D14ReligionCommunityIds.D10RelationOwner); Add(log, "d12_trust", D14ReligionCommunityIds.D12CommunityOwner); Add(log, "d12_community", D14ReligionCommunityIds.D12CommunityOwner);
            return log;
        }

        private static void Add(EventLog log, string id, string owner)
        {
            log.Append(new WorldEvent(id, 0, "d14_a06_test", owner, new EventResult { ResultCode = "source_ready", Summary = "D14-A06 测试来源事实。", FactIds = new List<string> { "source:" + id } }, new[] { "player" }, new[] { "test:d14_a06" }));
        }

        private sealed class AuthorityFixture : ID14BuildingScheduleAuthority, ID14FuneralRiteAuthority, ID14D10TrustAuthority, ID14D12CommunityAuthority
        {
            public D14BuildingOpenDecision Evaluate(D14BuildingAccessRequest request, FactEvent sourceFact) { return new D14BuildingOpenDecision { Allowed = true, Code = "open", Message = "fixture", BuildingStatus = "Operational", SourceOwnerSystem = D14ReligionCommunityIds.BuildingOwner, SourceEventId = "building_open" }; }
            public D14FuneralRiteDecision Hold(D14FuneralRiteRequest request, FactEvent sourceFact) { return new D14FuneralRiteDecision { Allowed = true, Code = "held", Message = "fixture", Status = "Held", SourceOwnerSystem = D14ReligionCommunityIds.D12CommunityOwner, SourceEventId = "community_funeral" }; }
            public D14TrustDecision ApplyOath(D14OathRequest request, FactEvent sourceFact, int proposedTrustDelta) { return new D14TrustDecision { Allowed = true, Code = "trust_applied", Message = "fixture", AppliedTrustDelta = proposedTrustDelta, TrustAfter = 50 + proposedTrustDelta, SourceOwnerSystem = D14ReligionCommunityIds.D10RelationOwner, SourceEventId = "d10_trust" }; }
            D14TrustDecision ID14D12CommunityAuthority.ApplyOath(D14OathRequest request, FactEvent sourceFact, int proposedTrustDelta) { return new D14TrustDecision { Allowed = true, Code = "community_trust_applied", Message = "fixture", AppliedTrustDelta = proposedTrustDelta, TrustAfter = 40 + proposedTrustDelta, SourceOwnerSystem = D14ReligionCommunityIds.D12CommunityOwner, SourceEventId = "d12_trust" }; }
            public D14CommunityInteractionDecision Resolve(D14CommunityInteractionRequest request, FactEvent sourceFact, int proposedTrustDelta) { return new D14CommunityInteractionDecision { Allowed = true, Code = "community_etiquette_completed", Message = "礼仪通过", AppliedTrustDelta = 3, TrustAfter = 53, SourceOwnerSystem = D14ReligionCommunityIds.D12CommunityOwner, SourceEventId = "d12_community" }; }
        }
    }
}
