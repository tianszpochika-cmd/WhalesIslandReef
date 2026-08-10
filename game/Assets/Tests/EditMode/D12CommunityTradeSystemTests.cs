using System.Collections.Generic;
using System.IO;
using System;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D12CommunityTradeSystemTests
    {
        [Test]
        public void EtiquetteLanguageKnowledgeAndHelpProduceDifferentOutcomes()
        {
            var log = new EventLog();
            var system = CreateSystem(log);
            var inventory = NewInventory();
            Add(inventory, "salt", 2);

            var proposal = system.ProposeTrade(new D12CommunityTradeIntent
            {
                RequestId = "proposal-01",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                GiveItemId = "salt",
                GiveQuantity = 1,
                WantItemId = "cocoa",
                WantQuantity = 1,
                LanguageId = "wrong_language",
                LanguageLevel = 0,
                KnowledgeLevel = 0,
                Etiquette = D12CommunityEtiquette.DirectBargain,
                WorldDay = 1,
                SaveRevision = 1,
            });
            Assert.IsNotNull(proposal);
            Assert.AreEqual(0, log.Count, "Agent/表达提案不能直接落事实。");

            var misunderstanding = system.SettleTrade(proposal, inventory);
            Assert.IsFalse(misunderstanding.Succeeded);
            Assert.AreEqual("misunderstood_language", misunderstanding.Code);
            Assert.AreEqual(D12CommunityTradeDisposition.Misunderstood, misunderstanding.Disposition);

            var knowledgeGate = system.SettleTrade(new D12CommunityTradeIntent
            {
                RequestId = "knowledge-gate",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                GiveItemId = "salt", GiveQuantity = 1,
                WantItemId = "cocoa", WantQuantity = 1,
                LanguageId = "green_cay_trade_tongue", LanguageLevel = 2,
                KnowledgeLevel = 0,
                Etiquette = D12CommunityEtiquette.GiftBeforeTrade,
                WorldDay = 2,
                SaveRevision = 2,
            }, inventory);
            Assert.IsFalse(knowledgeGate.Succeeded);
            Assert.AreEqual("knowledge_required", knowledgeGate.Code);

            var help = system.RequestHelp(new D12CommunityHelpRequest
            {
                RequestId = "help-01",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                LanguageId = "wrong_language",
                LanguageLevel = 0,
                KnowledgeLevel = 0,
                WorldDay = 3,
                SaveRevision = 3,
                CauseEventIds = new List<string> { misunderstanding.EventId },
            });
            Assert.IsTrue(help.Succeeded, help.Message);
            Assert.AreEqual("community_help_offered", help.Code);
            Assert.Greater(help.CulturalKnowledgeAfter, 0);

            var wrongEtiquette = system.SettleTrade(new D12CommunityTradeIntent
            {
                RequestId = "trade-wrong-etiquette",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                GiveItemId = "salt",
                GiveQuantity = 1,
                WantItemId = "cocoa",
                WantQuantity = 1,
                LanguageId = "green_cay_trade_tongue",
                LanguageLevel = 2,
                KnowledgeLevel = help.CulturalKnowledgeAfter,
                Etiquette = D12CommunityEtiquette.DirectBargain,
                WorldDay = 4,
                SaveRevision = 4,
                CauseEventIds = new List<string> { help.EventId },
            }, inventory);
            Assert.IsFalse(wrongEtiquette.Succeeded);
            Assert.AreEqual("etiquette_mismatch", wrongEtiquette.Code);

            var completed = system.SettleTrade(new D12CommunityTradeIntent
            {
                RequestId = "trade-correct",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                GiveItemId = "salt",
                GiveQuantity = 1,
                WantItemId = "cocoa",
                WantQuantity = 1,
                LanguageId = "green_cay_trade_tongue",
                LanguageLevel = 2,
                KnowledgeLevel = help.CulturalKnowledgeAfter,
                Etiquette = D12CommunityEtiquette.GiftBeforeTrade,
                WorldDay = 5,
                SaveRevision = 5,
                CauseEventIds = new List<string> { help.EventId },
            }, inventory);
            Assert.IsTrue(completed.Succeeded, completed.Message);
            Assert.AreEqual(D12CommunityTradeDisposition.Completed, completed.Disposition);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "cocoa"));
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "salt"));
            Assert.IsTrue(completed.Fact.CauseIds.Contains(help.EventId));
            Assert.Greater(completed.LongTermReputationAfter, 0);
            Assert.Greater(completed.TrustAfter, 0);
        }

        [Test]
        public void BoundariesInventoryAndUnknownCommunityAreExplicitlyHandled()
        {
            var system = CreateSystem(new EventLog());
            var inventory = NewInventory();
            Add(inventory, "salt", 1);

            var protectedResult = system.SettleTrade(Intent("protected", D12CommunityTradeIds.GreenCayMangroveLocation,
                D12CommunityEtiquette.GiftBeforeTrade, 2, 1, "green_cay_trade_tongue", 2), inventory);
            Assert.IsFalse(protectedResult.Succeeded);
            Assert.AreEqual("protected_area_refused", protectedResult.Code);
            Assert.IsFalse(string.IsNullOrEmpty(protectedResult.RecoveryRouteId));

            var outsideResult = system.SettleTrade(Intent("outside", "another_community_land", D12CommunityEtiquette.GiftBeforeTrade,
                2, 2, "green_cay_trade_tongue", 2), inventory);
            Assert.IsFalse(outsideResult.Succeeded);
            Assert.AreEqual("land_boundary_refused", outsideResult.Code);

            var emptyResult = system.SettleTrade(new D12CommunityTradeIntent
            {
                RequestId = "empty",
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                GiveItemId = "salt", GiveQuantity = 1,
                WantItemId = "medicine", WantQuantity = 99,
                LanguageId = "green_cay_trade_tongue", LanguageLevel = 2, KnowledgeLevel = 2,
                Etiquette = D12CommunityEtiquette.GiftBeforeTrade,
                WorldDay = 3, SaveRevision = 3,
            }, inventory);
            Assert.IsFalse(emptyResult.Succeeded);
            Assert.AreEqual("community_out_of_stock", emptyResult.Code);

            var unknown = system.SettleTrade(new D12CommunityTradeIntent
            {
                RequestId = "unknown-community",
                CommunityId = "community_missing",
                LocationRef = "nowhere",
                GiveItemId = "salt", GiveQuantity = 1,
                WantItemId = "cocoa", WantQuantity = 1,
                WorldDay = 1, SaveRevision = 4,
            }, inventory);
            Assert.IsFalse(unknown.Succeeded);
            Assert.AreEqual("community_unknown", unknown.Code);
        }

        [Test]
        public void SaveRestorePreservesCommunityReputationAndPreventsDuplicateBarter()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_community_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D12 社群测试船长" });
                var log = new EventLog();
                var system = CreateSystem(log);
                var inventory = NewInventory();
                Add(inventory, "salt", 1);
                var trade = system.SettleTrade(new D12CommunityTradeIntent
                {
                    RequestId = "save-trade",
                    CommunityId = D12CommunityTradeIds.FreeCoveCommunityId,
                    ActorRef = "player",
                    LocationRef = D12CommunityTradeIds.FreeCoveTradeLocation,
                    GiveItemId = "salt", GiveQuantity = 1,
                    WantItemId = "timber", WantQuantity = 1,
                    LanguageId = "free_cove_trade_tongue", LanguageLevel = 1, KnowledgeLevel = 1,
                    Etiquette = D12CommunityEtiquette.FormalGreeting,
                    WorldDay = 5, SaveRevision = 50,
                }, inventory);
                Assert.IsTrue(trade.Succeeded, trade.Message);
                var snapshot = system.Snapshot;
                SaveSystem.SaveD12Runtime(new D12RuntimeSnapshot
                {
                    MapDiscovery = new D12MapDiscoverySnapshot(),
                    CommunityTrade = snapshot,
                });
                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loaded = SaveSystem.LoadD12Runtime();
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.CommunityTrade);
                var restoredLog = EventLog.FromSnapshot(loaded.CommunityTrade.EventLog);
                var restored = CreateSystem(restoredLog, loaded.CommunityTrade);
                var reputationBefore = restored.GetReputation("player", D12CommunityTradeIds.FreeCoveCommunityId);
                int timberBefore = D3InventorySystem.CountCargo(inventory, "timber");
                var duplicate = restored.SettleTrade(new D12CommunityTradeIntent
                {
                    RequestId = "save-trade",
                    CommunityId = D12CommunityTradeIds.FreeCoveCommunityId,
                    ActorRef = "player",
                    LocationRef = D12CommunityTradeIds.FreeCoveTradeLocation,
                    GiveItemId = "salt", GiveQuantity = 1,
                    WantItemId = "timber", WantQuantity = 1,
                    LanguageId = "free_cove_trade_tongue", LanguageLevel = 1, KnowledgeLevel = 1,
                    Etiquette = D12CommunityEtiquette.FormalGreeting,
                    WorldDay = 5, SaveRevision = 50,
                }, inventory);
                Assert.IsTrue(duplicate.Duplicate);
                Assert.AreEqual(timberBefore, D3InventorySystem.CountCargo(inventory, "timber"));
                Assert.AreEqual(reputationBefore.LongTermReputation,
                    restored.GetReputation("player", D12CommunityTradeIds.FreeCoveCommunityId).LongTermReputation);
            }
            finally
            {
                SaveSystem.DbPath = null;
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    var path = dbPath + suffix;
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
        }

        [Test]
        public void CatalogRequiresSensitiveContentLabelsReviewStatusAndDistinctInstitutions()
        {
            var communities = D12CommunityTradeCatalog.CreateVerticalSlice();
            Assert.AreEqual(2, communities.Count);
            Assert.AreNotEqual(communities[0].SocialInstitution, communities[1].SocialInstitution);
            foreach (var community in communities)
            {
                community.Validate();
                Assert.IsFalse(string.IsNullOrEmpty(community.SensitiveContentLabel));
                Assert.AreEqual(D12CommunityContentReviewStatus.ApprovedWithWarning, community.ContentReviewStatus);
            }
            foreach (var boundary in D12CommunityTradeCatalog.CreateBoundaries()) boundary.Validate();
        }

        private static D12CommunityTradeSystem CreateSystem(EventLog log, D12CommunityTradeSnapshot snapshot = null)
        {
            return new D12CommunityTradeSystem(
                D12CommunityTradeCatalog.CreateVerticalSlice(),
                D12CommunityTradeCatalog.CreateBoundaries(), log, snapshot);
        }

        private static D3InventoryState NewInventory()
        {
            return new D3InventoryState(20, 40);
        }

        private static void Add(D3InventoryState inventory, string itemId, int quantity)
        {
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, itemId, quantity, out string error), error);
        }

        private static D12CommunityTradeIntent Intent(string requestId, string location,
            D12CommunityEtiquette etiquette, int day, long revision, string languageId, int knowledge)
        {
            return new D12CommunityTradeIntent
            {
                RequestId = requestId,
                CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                ActorRef = "player",
                LocationRef = location,
                GiveItemId = "salt", GiveQuantity = 1,
                WantItemId = "cocoa", WantQuantity = 1,
                LanguageId = languageId,
                LanguageLevel = 2,
                KnowledgeLevel = knowledge,
                Etiquette = etiquette,
                WorldDay = day,
                SaveRevision = revision,
            };
        }
    }
}
