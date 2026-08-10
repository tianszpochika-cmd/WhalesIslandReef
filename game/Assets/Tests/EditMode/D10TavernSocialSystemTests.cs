using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace D10.Tests
{
    /// <summary>D10-A06 酒馆场景的纯 C# 入口、owner 路由和因果链测试。</summary>
    public class D10TavernSocialSystemTests
    {
        [Test]
        public void RumorsHaveSourceAndReliability_PokerOnlyCreatesEntryIntent()
        {
            var scenario = NewScenario();
            var tavern = scenario.Tavern;
            var scene = tavern.BuildSceneSnapshot(PortCatalog.StartingPortId, 2);

            Assert.AreEqual(3, scene.Rumors.Count);
            foreach (var rumor in scene.Rumors)
            {
                Assert.IsNotEmpty(rumor.SourceRef);
                Assert.GreaterOrEqual(rumor.Reliability, 0f);
                Assert.LessOrEqual(rumor.Reliability, 1f);
                Assert.IsNotEmpty(rumor.ReliabilityLabel);
            }
            var firstRumor = tavern.HearRumor("player_test", PortCatalog.StartingPortId,
                scene.Rumors[0].RumorId, 2, 10);
            var secondRumor = tavern.HearRumor("player_test", PortCatalog.StartingPortId,
                scene.Rumors[1].RumorId, 2, 10);
            Assert.IsTrue(firstRumor.Accepted);
            Assert.IsTrue(secondRumor.Accepted);
            Assert.AreNotEqual(firstRumor.Request.FactEventId, secondRumor.Request.FactEventId);

            var warning = tavern.RequestPokerEntry("player_test", PortCatalog.StartingPortId,
                scene.PokerTables[0].TableId, false, 2, 10);
            Assert.IsFalse(warning.Accepted);
            Assert.AreEqual(D10TavernRequestStatus.WarningRequired, warning.Status);

            int moneyBefore = scenario.Trade.Money;
            var request = tavern.RequestPokerEntry("player_test", PortCatalog.StartingPortId,
                scene.PokerTables[0].TableId, true, 2, 10);
            var result = tavern.Resolve(request.Request, scenario.Owners);

            Assert.IsTrue(result.Accepted);
            Assert.IsNotNull(result.PokerEntry);
            Assert.AreEqual("MiniGameSystem", result.PokerEntry.SettlementOwner);
            Assert.AreEqual(moneyBefore, scenario.Trade.Money);
            Assert.IsNull(result.Relation);
        }

        [Test]
        public void StrongAlcoholRoutesHealthFatigueAndConflictThroughOwners()
        {
            var scenario = NewScenario();
            var member = scenario.Crew[0];
            member.Stats.Morale = 20f;
            var tavern = scenario.Tavern;

            var warning = tavern.RequestAlcohol("player_test", PortCatalog.StartingPortId,
                member.Id, D10AlcoholStrength.Strong, false, 3, 20);
            Assert.AreEqual(D10TavernRequestStatus.WarningRequired, warning.Status);

            var request = tavern.RequestAlcohol("player_test", PortCatalog.StartingPortId,
                member.Id, D10AlcoholStrength.Strong, true, 3, 20);
            int moneyBefore = scenario.Trade.Money;
            var result = tavern.Resolve(request.Request, scenario.Owners);

            Assert.IsTrue(result.Accepted);
            Assert.AreEqual(97f, member.Stats.Health, 0.001f);
            Assert.AreEqual(8f, member.Stats.Fatigue, 0.001f);
            Assert.AreEqual(moneyBefore - D10TavernConfiguration.StrongDrinkPriceQuote,
                scenario.Trade.Money);
            Assert.IsNotNull(result.CrewBehavior);
            Assert.AreEqual(D10CrewBehaviorEventType.AlcoholConsumed, result.CrewBehavior.Event.Type);
            CollectionAssert.Contains(result.CrewBehavior.Fact.CauseIds, request.Request.FactEventId);
            Assert.IsNotNull(result.Relation);
            Assert.IsTrue(result.Relation.Succeeded);
            CollectionAssert.Contains(result.Relation.Fact.CauseIds, result.CrewBehavior.Fact.EventId);
            Assert.IsTrue(result.Relation.Relation.ConflictActive);
        }

        [Test]
        public void RestMealRecruitNightTalkAndSensitiveSideJobUseExplicitEntrances()
        {
            var scenario = NewScenario();
            var tavern = scenario.Tavern;
            var member = scenario.Crew[0];
            member.Stats.Fatigue = 30f;

            var rest = tavern.RequestRest("player_test", PortCatalog.StartingPortId,
                member.Id, 4, 4, 30);
            var restResult = tavern.Resolve(rest.Request, scenario.Owners);
            Assert.IsTrue(restResult.Accepted);
            Assert.Less(member.Stats.Fatigue, 30f);
            Assert.AreEqual("CrewSystem", restResult.CrewBehavior.Fact.OwnerSystem);

            var meal = tavern.RequestMeal("player_test", PortCatalog.StartingPortId,
                member.Id, 4, 31);
            var moneyBefore = scenario.Trade.Money;
            var mealResult = tavern.Resolve(meal.Request, scenario.Owners);
            Assert.IsTrue(mealResult.Accepted);
            Assert.AreEqual(moneyBefore - D10TavernConfiguration.MealPriceQuote, scenario.Trade.Money);

            var recruitment = tavern.RequestRecruitment("player_test", PortCatalog.StartingPortId,
                D9CrewRecruitmentCatalog.IsaacReedId, 4, 32);
            var recruitmentResult = tavern.Resolve(recruitment.Request, scenario.Owners);
            Assert.IsTrue(recruitmentResult.Accepted);
            Assert.Contains(D9CrewRecruitmentCatalog.IsaacReedId, scenario.Ship.CrewIds);

            var danceWarning = tavern.RequestDance("player_test", PortCatalog.StartingPortId,
                member.Id, false, 4, 33);
            Assert.AreEqual(D10TavernRequestStatus.WarningRequired, danceWarning.Status);
            var nightTalk = tavern.RequestNightTalk("player_test", PortCatalog.StartingPortId,
                member.Id, true, 4, 34);
            var nightTalkResult = tavern.Resolve(nightTalk.Request, scenario.Owners);
            Assert.IsTrue(nightTalkResult.Accepted);
            Assert.IsTrue(nightTalkResult.Relation.Succeeded);
            CollectionAssert.Contains(nightTalkResult.Relation.Fact.CauseIds, nightTalk.Request.FactEventId);

            var sensitiveJob = tavern.RequestSideJobOffer("player_test", PortCatalog.StartingPortId,
                "sidejob_dock_fight_01", false, 4, 35);
            Assert.AreEqual(D10TavernRequestStatus.WarningRequired, sensitiveJob.Status);
            var skipped = tavern.SkipSensitiveContent(D10TavernSensitivity.ExploitativeWork);
            Assert.AreEqual(D10TavernRequestStatus.Skipped, skipped.Status);
            Assert.IsEmpty(D10TavernConfiguration.Validate());
        }

        private static Scenario NewScenario()
        {
            var log = new EventLog();
            var economy = new TradeEconomySystem(log);
            var trade = new D3TradeState(300, 40);
            CommodityCatalog.SeedMarket(trade.Market);
            var ship = ShipState.CreateNew("ship_test", ShipTypeSeed.Find("small_fishing"), "player_test");
            var crew = new List<CrewAgent>
            {
                new CrewAgent("crew_existing", new PersonaDefinition("老水手", "水手"), new InMemoryStore()),
            };
            ship.CrewIds.Add(crew[0].Id);
            var preparation = new D9PortShipPreparationSystem(economy, log);
            var runtime = new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.StartingPortId,
                VoyageStatus = D3VoyageStatus.InPort,
                CurrentDay = 1,
                SaveRevision = 1,
            };
            Assert.IsTrue(preparation.EnterPort(runtime, ship, PortCatalog.StartingPortId).Accepted);
            Assert.IsTrue(preparation.AssignBerth(ship, PortCatalog.StartingPortId,
                D9PortShipPreparationSystem.SantaMargaritaBerthId).Accepted);

            var behavior = new D10CrewBehaviorSystem(log);
            var relations = new D10RelationSystem(log);
            var tavern = new D10TavernSocialSystem(log);
            var owners = new D10TavernOwnerServices
            {
                Economy = economy,
                TradeState = trade,
                Ship = ship,
                Preparation = preparation,
                CrewBehavior = behavior,
                Relations = relations,
                Crew = crew,
                RelationTargetId = "player_test",
                RecruitCandidateResolver = id => id == D9CrewRecruitmentCatalog.IsaacReedId
                    ? D9CrewRecruitmentCatalog.CreateIsaacReed() : null,
            };
            return new Scenario
            {
                Tavern = tavern,
                Owners = owners,
                Trade = trade,
                Ship = ship,
                Crew = crew,
            };
        }

        private sealed class Scenario
        {
            public D10TavernSocialSystem Tavern;
            public D10TavernOwnerServices Owners;
            public D3TradeState Trade;
            public ShipState Ship;
            public List<CrewAgent> Crew;
        }
    }
}
