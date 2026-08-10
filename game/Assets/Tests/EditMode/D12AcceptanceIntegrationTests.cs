using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    /// <summary>
    /// D12-S01 只编排各领域 owner 的真实接口；不复制任何探索、经济、关系、牌局、任务或图鉴状态。
    /// </summary>
    public sealed class D12AcceptanceIntegrationTests
    {
        [Test]
        public void EndToEndVoyage_UsesRumorEquipmentSalvageTradePokerCollectionAndSave()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_s01_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D12 集成验收船长" });
                var log = new EventLog();

                // D9-A03 Quest owner 记录传闻、来源和交叉验证；Agent 表达提议不写事实。
                var treasure = new D9TreasureQuestSystem(log);
                var proposal = treasure.BuildClueExpressionProposal("agent:deckhand", "圣玛丽湾的潮线值得核对。 ");
                Assert.IsNotNull(proposal);
                Assert.AreEqual(0, log.Count);
                var rumor = treasure.HearRumor("player", 1, 1);
                Assert.IsTrue(rumor.Accepted, rumor.Message);
                Assert.IsTrue(treasure.AcceptRumor().Accepted);
                var secondClue = treasure.RecordCatalogClue("clue:first_chart_salvaged_chart", "player", 2, 2);
                Assert.IsTrue(secondClue.Accepted, secondClue.Message);
                var crossValidated = treasure.CrossValidateClues(new D9TreasureClueVerificationRequest
                {
                    ActorRef = "player",
                    ClueId = "clue:first_chart_tide_mark",
                    OtherClueId = "clue:first_chart_salvaged_chart",
                }, 3, 3);
                Assert.IsTrue(crossValidated.Accepted, crossValidated.Message);

                var equipment = new List<string>
                {
                    D9TreasureQuestCatalog.RequiredEquipmentId,
                    D12SalvageIds.GrapnelToolId,
                    D12SalvageIds.RopeToolId,
                    D12SalvageIds.PumpToolId,
                };
                Assert.IsEmpty(treasure.MissingEquipment(equipment));
                var treasureLanding = treasure.TryLand(new D9TreasureLandingRequest
                {
                    ActorRef = "player",
                    LocationRef = D9TreasureQuestCatalog.SiteRef,
                    ClueId = "clue:first_chart_tide_mark",
                    RouteChoice = D9TreasureRouteChoice.WaitForTide,
                    TideWindowOpen = true,
                    EnforceClueWindow = true,
                    RequireCrossValidation = true,
                    EquipmentIds = equipment,
                }, 4, 4);
                Assert.IsTrue(treasureLanding.Accepted, treasureLanding.Message);
                Assert.IsNotNull(treasureLanding.RewardRequest);

                // D12-A01 唯一写入海图状态；D12-A08 只观察随后进入 EventLog 的事实。
                var map = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), log);
                var mapKnowledge = map.RecordQuestKnowledge(
                    D12SalvageIds.WreckMapFeatureId,
                    treasureLanding.Fact.EventId,
                    treasureLanding.Fact.EventId,
                    "传闻交叉验证后确认海图湾沉船位置。",
                    "landmark:saint_marys_chart_cove",
                    5,
                    5);
                Assert.IsTrue(mapKnowledge.Accepted, mapKnowledge.Message);
                Assert.IsNotNull(map.GetFeatureState(D12SalvageIds.WreckMapFeatureId));

                var dynamicQuests = new D12DynamicQuestLogSystem(log);
                var mainAccepted = dynamicQuests.Accept(new D12QuestAcceptRequest
                {
                    QuestId = D12DynamicQuestCatalog.ChartRecoveryQuestId,
                    ActorRef = "player",
                    WorldDay = 5,
                    SaveRevision = 6,
                });
                Assert.IsTrue(mainAccepted.Applied, mainAccepted.Message);
                var mainMapProgress = dynamicQuests.ObserveFact(new D12QuestFactObservationRequest
                {
                    QuestId = D12DynamicQuestCatalog.ChartRecoveryQuestId,
                    FactEventId = mapKnowledge.Fact.EventId,
                    FactSaveRevision = mapKnowledge.Fact.SaveRevision,
                    ActorRef = "player",
                });
                Assert.IsTrue(mainMapProgress.Applied, mainMapProgress.Message);

                // 法律/势力 owner 先拒绝，再通过明确法律复核路线恢复；D12 不推断产权。
                var legal = new IntegrationLegalAuthority { Allowed = false };
                var salvage = new D12WreckCaveSalvageSystem(
                    D12WreckCaveSalvageCatalog.CreateVerticalSlice(), map, log, legal);
                var site = FindSite(D12WreckCaveSalvageCatalog.WreckSiteId);
                var salvageRequest = new D12SalvageRequest
                {
                    AttemptId = "integration_wreck_legal_probe",
                    SiteId = site.SiteId,
                    ActorRef = "player",
                    LocationRef = site.LocationRef,
                    WorldDay = 7,
                    SaveRevision = 7,
                    TideWindowOpen = true,
                    TideStage = D12TideStage.Low,
                    EquipmentIds = new List<string>
                    {
                        D12SalvageIds.GrapnelToolId,
                        D12SalvageIds.RopeToolId,
                        D12SalvageIds.PumpToolId,
                    },
                    RopeMeters = 24,
                    RiskSeed = 1,
                };
                var legalDenied = salvage.TrySalvage(salvageRequest);
                Assert.IsTrue(legalDenied.Accepted);
                Assert.AreEqual(D12SalvageFailureCode.LegalDenied, legalDenied.FailureCode);
                Assert.AreEqual("route_wreck_legal_review", legalDenied.RecoveryRoute.RouteId);
                Assert.Greater(legal.RuleCount, 0);

                legal.Allowed = true;
                var legalRecovered = salvage.Recover(new D12SalvageRecoveryRequest
                {
                    RecoveryAttemptId = "integration_wreck_legal_review",
                    SiteId = site.SiteId,
                    ActorRef = "player",
                    RouteId = "route_wreck_legal_review",
                    LocationRef = site.LocationRef,
                    WorldDay = 8,
                    SaveRevision = 8,
                    TideWindowOpen = true,
                    TideStage = D12TideStage.Low,
                    EquipmentIds = salvageRequest.EquipmentIds,
                });
                Assert.IsTrue(legalRecovered.Recovered, legalRecovered.Message);

                salvageRequest.AttemptId = "integration_wreck_success";
                salvageRequest.WorldDay = 9;
                salvageRequest.SaveRevision = 9;
                salvageRequest.RiskSeed = FindSuccessSeed(site, salvageRequest);
                var salvaged = salvage.TrySalvage(salvageRequest);
                Assert.IsTrue(salvaged.Succeeded, salvaged.Message);
                Assert.IsNotNull(salvaged.RewardRequest);
                Assert.AreEqual("pending", salvaged.RewardRequest.Status);
                var mainSalvageProgress = dynamicQuests.ObserveFact(new D12QuestFactObservationRequest
                {
                    QuestId = D12DynamicQuestCatalog.ChartRecoveryQuestId,
                    FactEventId = salvaged.Fact.EventId,
                    FactSaveRevision = salvaged.Fact.SaveRevision,
                    ActorRef = "player",
                });
                Assert.AreEqual(D12DynamicQuestStatus.Completed, mainSalvageProgress.Status);
                Assert.IsNotNull(mainSalvageProgress.RewardRequest);

                // 社群交易只经 ProposeTrade 产生意图，结算才由社群 owner 写库存/声望并调用 D3 Inventory owner。
                var inventory = new D3InventoryState(20, 40);
                Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, "salt", 1, out string inventoryError), inventoryError);
                var community = new D12CommunityTradeSystem(
                    D12CommunityTradeCatalog.CreateVerticalSlice(),
                    D12CommunityTradeCatalog.CreateBoundaries(), log);
                var sideAccepted = dynamicQuests.Accept(new D12QuestAcceptRequest
                {
                    QuestId = D12DynamicQuestCatalog.CommunityReciprocityQuestId,
                    ActorRef = "player",
                    WorldDay = 10,
                    SaveRevision = 10,
                });
                Assert.IsTrue(sideAccepted.Applied, sideAccepted.Message);
                var help = community.RequestHelp(new D12CommunityHelpRequest
                {
                    RequestId = "integration_green_cay_help",
                    CommunityId = D12CommunityTradeIds.GreenCayCommunityId,
                    ActorRef = "player",
                    LocationRef = D12CommunityTradeIds.GreenCayTradeLocation,
                    LanguageId = "wrong_language",
                    LanguageLevel = 0,
                    KnowledgeLevel = 0,
                    WorldDay = 11,
                    SaveRevision = 11,
                });
                Assert.IsTrue(help.Succeeded, help.Message);
                var sideHelpProgress = dynamicQuests.ObserveFact(new D12QuestFactObservationRequest
                {
                    QuestId = D12DynamicQuestCatalog.CommunityReciprocityQuestId,
                    FactEventId = help.EventId,
                    FactSaveRevision = 11,
                    ActorRef = "player",
                });
                Assert.IsTrue(sideHelpProgress.Applied, sideHelpProgress.Message);

                var tradeIntent = community.ProposeTrade(new D12CommunityTradeIntent
                {
                    RequestId = "integration_green_cay_trade",
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
                    WorldDay = 12,
                    SaveRevision = 12,
                });
                var factCountBeforeSettlement = log.Count;
                Assert.IsNotNull(tradeIntent);
                Assert.IsTrue(tradeIntent.ProposedByAgent);
                Assert.AreEqual(factCountBeforeSettlement, log.Count);
                var trade = community.SettleTrade(tradeIntent, inventory);
                Assert.IsTrue(trade.Succeeded, trade.Message);
                Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, "cocoa"));
                var sideTradeProgress = dynamicQuests.ObserveFact(new D12QuestFactObservationRequest
                {
                    QuestId = D12DynamicQuestCatalog.CommunityReciprocityQuestId,
                    FactEventId = trade.EventId,
                    FactSaveRevision = 12,
                    ActorRef = "player",
                });
                Assert.AreEqual(D12DynamicQuestStatus.Completed, sideTradeProgress.Status);

                // 牌局读取经济快照，输牌仍留下债务/情报请求；牌局系统不直接改钱或债务。
                var pokerTable = D12PokerCatalog.CreateLowStakesTable();
                pokerTable.FixedSeed = FindLossSeed(pokerTable);
                var poker = new D12PokerSystem(new[] { pokerTable }, log);
                var pokerEnter = poker.Apply(PokerRequest(D12PokerAction.Enter, pokerTable, 13, 0, 0));
                Assert.IsTrue(pokerEnter.Accepted, pokerEnter.Message);
                Assert.IsNotNull(pokerEnter.DebtRequest);
                var pokerBet = poker.Apply(PokerRequest(D12PokerAction.PlaceBet, pokerTable, 14, 5, 5));
                Assert.IsTrue(pokerBet.Accepted, pokerBet.Message);
                Assert.IsNotNull(pokerBet.DebtRequest);
                var pokerResolved = poker.Apply(PokerRequest(D12PokerAction.Resolve, pokerTable, 15, 0, 0));
                Assert.IsTrue(pokerResolved.Accepted, pokerResolved.Message);
                Assert.AreEqual(D12PokerOutcome.Loss, pokerResolved.Outcome);
                Assert.IsNotNull(pokerResolved.InformationRequest);
                Assert.GreaterOrEqual(poker.Snapshot.DebtRequests.Count, 2);
                var pokerClaim = poker.ClaimSettlement(pokerResolved.Record.GameId, 15);
                Assert.IsTrue(pokerClaim.Accepted, pokerClaim.Message);

                // 宗教语境必须先经过警告确认；解释是文本投影，不能改变牌面或世界状态。
                var tarot = new D12TarotSystem(log);
                var tarotRequest = TarotRequest(16, 16, "integration_religion_warning", false);
                var warning = tarot.StartSession(tarotRequest);
                Assert.IsTrue(warning.WarningRequired);
                tarotRequest.WarningAcknowledged = true;
                tarotRequest.ActionId = "integration_religion_acknowledged";
                tarotRequest.SaveRevision = 17;
                var session = tarot.StartSession(tarotRequest);
                Assert.IsTrue(session.Succeeded, session.Message);
                Assert.AreEqual(3, session.Cards.Count);
                var explanation = tarot.AttachExplanation(new D12TarotExplanationInput
                {
                    SessionId = session.SessionId,
                    Provider = "template",
                    ExplanationText = "只提供可求证的观察方向，不裁决奖励或天气。",
                    SaveRevision = 17,
                    ActionId = "integration_explanation",
                });
                Assert.IsTrue(explanation.Succeeded, explanation.Message);
                Assert.AreEqual("false", log.Find(explanation.FactEventId, 17).Result.Values["affects_state"]);

                // 图鉴只接受已存在的领域事实；历史/特殊武器必须有授权事实。
                var collection = new D12CollectionAtlasSystem(eventLog: log);
                var treasureEntry = collection.Record(new D12CollectionRecordRequest
                {
                    ContentId = "reward_first_chart",
                    Category = D12CollectionCategory.Treasure,
                    FactEventId = treasureLanding.Fact.EventId,
                    FactSaveRevision = treasureLanding.Fact.SaveRevision,
                    EvidenceRef = treasureLanding.RewardRequest.RequestId,
                    LocationRef = treasureLanding.Fact.LocationRef,
                    WorldDay = treasureLanding.Fact.WorldDay,
                    AcquisitionMethod = D12CollectionAcquisitionMethod.QuestReward,
                });
                Assert.IsTrue(treasureEntry.Accepted, treasureEntry.Message);
                Assert.IsTrue(treasureEntry.AchievementUnlocked);
                var weaponSource = AppendFact(log, "event_integration_moonwake_transfer", 18,
                    "santa_margarita_armory", 18, "ArmorySystem", "authorized_weapon_transferred");
                var weaponAuthorization = AppendFact(log, "event_integration_moonwake_authorization", 18,
                    "santa_margarita_armory", 19, "HarborAuthority", "armory_authorization_granted");
                var weaponEntry = collection.Record(new D12CollectionRecordRequest
                {
                    ContentId = D12CollectionCatalog.SpecialWeaponId,
                    Category = D12CollectionCategory.SpecialWeapon,
                    FactEventId = weaponSource.EventId,
                    FactSaveRevision = weaponSource.SaveRevision,
                    EvidenceRef = "armory:moonwake:transfer_receipt",
                    LocationRef = weaponSource.LocationRef,
                    WorldDay = weaponSource.WorldDay,
                    AcquisitionMethod = D12CollectionAcquisitionMethod.AuthorizedArmory,
                    AuthorizationRef = "armory_authorization_moonwake",
                    AuthorizationFactEventId = weaponAuthorization.EventId,
                    AuthorizationSaveRevision = weaponAuthorization.SaveRevision,
                });
                Assert.IsTrue(weaponEntry.Accepted, weaponEntry.Message);
                Assert.IsTrue(weaponEntry.AchievementUnlocked);

                var agentSummary = dynamicQuests.BuildAgentSummary(8);
                Assert.IsTrue(agentSummary.FactsOnly);
                Assert.IsFalse(agentSummary.CanCreateFacts);
                Assert.IsNotEmpty(dynamicQuests.ReadJournal());

                var d12Snapshot = new D12RuntimeSnapshot
                {
                    MapDiscovery = map.Snapshot,
                    Salvage = salvage.Snapshot,
                    CommunityTrade = community.Snapshot,
                    Tarot = tarot.Snapshot,
                    CollectionAtlas = collection.Snapshot,
                    DynamicQuestLog = dynamicQuests.Snapshot,
                };
                d12Snapshot.Validate();
                var d3Snapshot = new D3RuntimeSnapshot
                {
                    CurrentLocationId = site.LocationRef,
                    VoyageStatus = D3VoyageStatus.Landed,
                    CurrentDay = 18,
                    SaveRevision = 19,
                    EventLog = log.CaptureSnapshot(),
                };
                foreach (var item in inventory.Cargo) d3Snapshot.Cargo[item.Key] = item.Value;

                SaveSystem.SaveD9Runtime(new D9RuntimeSnapshot { TreasureQuest = treasure.Snapshot });
                SaveSystem.SaveD12Runtime(d12Snapshot);
                SaveSystem.SaveD12PokerRuntime(poker.Snapshot);
                SaveSystem.SaveD3Runtime(d3Snapshot);
                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());

                var loadedD9 = SaveSystem.LoadD9Runtime();
                var loadedD12 = SaveSystem.LoadD12Runtime();
                var loadedPoker = SaveSystem.LoadD12PokerRuntime();
                var loadedD3 = SaveSystem.LoadD3Runtime();
                Assert.IsNotNull(loadedD9);
                Assert.IsNotNull(loadedD12);
                Assert.IsNotNull(loadedPoker);
                Assert.IsNotNull(loadedD3);
                Assert.AreEqual(1, loadedD9.TreasureQuest.CrossValidationCount);
                Assert.AreEqual(D12DynamicQuestStatus.Completed,
                    FindQuest(loadedD12.DynamicQuestLog, D12DynamicQuestCatalog.ChartRecoveryQuestId).Status);
                Assert.AreEqual(D12DynamicQuestStatus.Completed,
                    FindQuest(loadedD12.DynamicQuestLog, D12DynamicQuestCatalog.CommunityReciprocityQuestId).Status);
                Assert.IsTrue(loadedPoker.Records[0].SettlementClaimed);
                Assert.AreEqual(1, loadedD3.Cargo["cocoa"]);

                var restoredLog = EventLog.FromSnapshot(loadedD3.EventLog);
                Assert.AreEqual(log.Count, restoredLog.Count);
                var restoredD9 = new D9TreasureQuestSystem(restoredLog, null, loadedD9.TreasureQuest);
                Assert.AreEqual(1, restoredD9.Snapshot.CrossValidationCount);
                var restoredMap = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), restoredLog,
                    loadedD12.MapDiscovery);
                var restoredSalvage = new D12WreckCaveSalvageSystem(
                    D12WreckCaveSalvageCatalog.CreateVerticalSlice(), restoredMap, restoredLog,
                    new IntegrationLegalAuthority { Allowed = true }, loadedD12.Salvage);
                var duplicateSalvage = restoredSalvage.TrySalvage(salvageRequest);
                Assert.IsFalse(duplicateSalvage.Accepted);
                Assert.AreEqual("already_salvaged", duplicateSalvage.Code);
                Assert.AreEqual(salvaged.RewardRequest.RequestId,
                    restoredSalvage.Snapshot.Sites[0].RewardRequest.RequestId);
                var restoredCommunity = new D12CommunityTradeSystem(
                    D12CommunityTradeCatalog.CreateVerticalSlice(),
                    D12CommunityTradeCatalog.CreateBoundaries(), restoredLog, loadedD12.CommunityTrade);
                var restoredInventory = new D3InventoryState(20, 40);
                foreach (var item in loadedD3.Cargo) restoredInventory.Cargo[item.Key] = item.Value;
                var duplicateTrade = restoredCommunity.SettleTrade(tradeIntent, restoredInventory);
                Assert.IsTrue(duplicateTrade.Duplicate);
                var restoredPoker = new D12PokerSystem(new[] { pokerTable }, restoredLog, loadedPoker);
                var duplicateClaim = restoredPoker.ClaimSettlement(pokerResolved.Record.GameId, 15);
                Assert.IsFalse(duplicateClaim.Accepted);
                Assert.AreEqual("duplicate_settlement", duplicateClaim.Code);
                var restoredTarot = new D12TarotSystem(restoredLog, loadedD12.Tarot);
                var duplicateTarot = restoredTarot.StartSession(tarotRequest);
                Assert.IsTrue(duplicateTarot.Duplicate);
                var restoredCollection = new D12CollectionAtlasSystem(eventLog: restoredLog,
                    initialState: loadedD12.CollectionAtlas);
                var duplicateCollection = restoredCollection.Record(new D12CollectionRecordRequest
                {
                    ContentId = "reward_first_chart",
                    Category = D12CollectionCategory.Treasure,
                    FactEventId = treasureLanding.Fact.EventId,
                    FactSaveRevision = treasureLanding.Fact.SaveRevision,
                    EvidenceRef = treasureLanding.RewardRequest.RequestId,
                    LocationRef = treasureLanding.Fact.LocationRef,
                    WorldDay = treasureLanding.Fact.WorldDay,
                    AcquisitionMethod = D12CollectionAcquisitionMethod.QuestReward,
                });
                Assert.IsTrue(duplicateCollection.Duplicate);
                var restoredDynamic = new D12DynamicQuestLogSystem(restoredLog,
                    initialSnapshot: loadedD12.DynamicQuestLog);
                Assert.AreEqual(D12DynamicQuestStatus.Completed,
                    restoredDynamic.ReadQuest(D12DynamicQuestCatalog.ChartRecoveryQuestId).Status);
                Assert.AreEqual(D12DynamicQuestStatus.Completed,
                    restoredDynamic.ReadQuest(D12DynamicQuestCatalog.CommunityReciprocityQuestId).Status);
                Assert.AreEqual(dynamicQuests.ReadJournal().Count, restoredDynamic.ReadJournal().Count);
            }
            finally
            {
                SaveSystem.DbPath = null;
                DeleteSaveFiles(dbPath);
            }
        }

        [Test]
        public void SensitiveContentAndOwnerBoundaries_AreEnforcedInIntegratedReadPath()
        {
            var log = new EventLog();
            var map = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), log);
            var proposal = map.ProposeCommunityKnowledge(
                D12SalvageIds.CaveMapFeatureId, "agent:guide", "npc:green_cay_guide",
                "testimony:green_cay_cave", "可能存在潮汐洞入口。", 1, 0.9f);
            Assert.IsNotNull(proposal);
            Assert.AreEqual(0, log.Count);
            var rejected = map.AcceptCommunityKnowledge(new D12CommunityKnowledgeAcceptance
            {
                Proposal = proposal,
                CommunityConfirmed = false,
                CommunityReceiptId = "",
                SaveRevision = 1,
            });
            Assert.IsFalse(rejected.Accepted);
            Assert.AreEqual("community_confirmation_required", rejected.Code);
            Assert.AreEqual(0, log.Count);

            foreach (var community in D12CommunityTradeCatalog.CreateVerticalSlice())
            {
                community.Validate();
                Assert.AreEqual(D12CommunityContentReviewStatus.ApprovedWithWarning, community.ContentReviewStatus);
            }
            foreach (var boundary in D12CommunityTradeCatalog.CreateBoundaries()) boundary.Validate();

            var tarot = new D12TarotSystem(log);
            var warning = tarot.StartSession(TarotRequest(2, 2, "sensitive-warning-only", false));
            Assert.IsTrue(warning.WarningRequired);
            Assert.AreEqual(0, tarot.Snapshot.Sessions.Count);

            var collectionDefinitions = D12CollectionCatalog.CreateVerticalSlice();
            D12CollectionEntryDefinition specialWeapon = null;
            foreach (var definition in collectionDefinitions)
                if (definition.ContentId == D12CollectionCatalog.SpecialWeaponId) specialWeapon = definition;
            Assert.IsNotNull(specialWeapon);
            Assert.IsTrue(specialWeapon.RequiresAuthorization);
            Assert.AreEqual("armory_authorization_moonwake", specialWeapon.RequiredAuthorizationRef);
        }

        private static D12DynamicQuestState FindQuest(D12DynamicQuestLogSnapshot snapshot, string questId)
        {
            foreach (var quest in snapshot.Quests)
                if (quest != null && quest.QuestId == questId) return quest;
            Assert.Fail("找不到动态任务：" + questId);
            return null;
        }

        private static D12ExplorationSiteDefinition FindSite(string siteId)
        {
            foreach (var site in D12WreckCaveSalvageCatalog.CreateVerticalSlice())
                if (site.SiteId == siteId) return site;
            Assert.Fail("找不到 D12 打捞地点：" + siteId);
            return null;
        }

        private static int FindSuccessSeed(D12ExplorationSiteDefinition site, D12SalvageRequest request)
        {
            var equipment = new HashSet<string>(request.EquipmentIds, StringComparer.Ordinal);
            int threshold = D12WreckCaveSalvageSystem.CalculateRiskThreshold(site, request, equipment);
            for (int seed = 0; seed < 100000; seed++)
                if (D12SalvageRiskCalculator.Roll(site.SiteId, request.AttemptId, seed) >= threshold) return seed;
            Assert.Fail("没有找到成功的确定性打捞种子。");
            return 0;
        }

        private static long FindLossSeed(D12PokerTableDefinition table)
        {
            for (long seed = 0; seed < 100000; seed++)
            {
                var candidate = table.Clone();
                candidate.FixedSeed = seed;
                var system = new D12PokerSystem(new[] { candidate }, new EventLog());
                var enter = system.Apply(PokerRequest(D12PokerAction.Enter, candidate, 1, 0, 0));
                var bet = system.Apply(PokerRequest(D12PokerAction.PlaceBet, candidate, 2, 5, 5));
                if (!enter.Accepted || !bet.Accepted) continue;
                var resolved = system.Apply(PokerRequest(D12PokerAction.Resolve, candidate, 3, 0, 0));
                if (resolved.Accepted && resolved.Outcome == D12PokerOutcome.Loss) return seed;
            }
            Assert.Fail("没有找到确定性输牌种子。");
            return 0;
        }

        private static D12PokerActionRequest PokerRequest(D12PokerAction action,
            D12PokerTableDefinition table, long revision, int day, long amount)
        {
            return new D12PokerActionRequest
            {
                Action = action,
                TableId = table.TableId,
                PlayerId = "player",
                WorldDay = day,
                SaveRevision = revision,
                Amount = amount,
                LocationRef = table.LocationRef,
                Economy = new D12PokerEconomyReadSnapshot
                {
                    AvailableMoney = 0,
                    CurrentDebt = 0,
                    CanBorrow = true,
                    DebtLimit = table.DebtLimit,
                },
            };
        }

        private static D12TarotSessionRequest TarotRequest(int day, long revision, string actionId, bool acknowledged)
        {
            return new D12TarotSessionRequest
            {
                DivinerId = "diviner_saint_marys_keeper",
                QueryTag = "weather",
                ActorRef = "player",
                LocationRef = "island_saint_marys",
                WorldDay = day,
                Seed = 4242,
                SaveRevision = revision,
                WarningAcknowledged = acknowledged,
                ActionId = actionId,
                TrustSourceRef = "d12-s01:religious-review",
                TrustEvidenceRef = "d12-s01:warning-record",
            };
        }

        private static FactEvent AppendFact(EventLog log, string eventId, int worldDay,
            string locationRef, long saveRevision, string owner, string resultCode)
        {
            return log.Append(new WorldEvent(
                eventId,
                worldDay,
                locationRef,
                owner,
                new EventResult
                {
                    ResultCode = resultCode,
                    Summary = "D12-S01 集成测试的所属系统事实。",
                    FactIds = new List<string> { "integration:source" },
                    Values = new Dictionary<string, string>
                    {
                        ["historical_review"] = "approved",
                        ["authorization_ref"] = "armory_authorization_moonwake",
                    },
                },
                new[] { "player" },
                new[] { "integration:source" },
                new string[0],
                saveRevision)).Fact;
        }

        private static void DeleteSaveFiles(string dbPath)
        {
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        private sealed class IntegrationLegalAuthority : ID12SalvageLegalAuthority
        {
            public bool Allowed;
            public int RuleCount;

            public D12SalvageLegalDecision Rule(D12SalvageLegalRequest request)
            {
                RuleCount++;
                return new D12SalvageLegalDecision
                {
                    RulingAvailable = true,
                    SalvageAllowed = Allowed,
                    ClaimStatus = Allowed ? "licensed" : "contested",
                    RulingRef = "d12-s01:legal:ruling:" + request.SiteId,
                    ClaimantRef = "faction:england",
                    ControllingFactionId = "england",
                    PermitRef = Allowed ? "permit:d12-s01" : string.Empty,
                    Reason = Allowed ? "集成测试法律适配器已批准。" : "集成测试法律适配器要求先完成复核。",
                };
            }
        }
    }
}
