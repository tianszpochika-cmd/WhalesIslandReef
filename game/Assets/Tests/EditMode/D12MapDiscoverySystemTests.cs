using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public class D12MapDiscoverySystemTests
    {
        [Test]
        public void MapLayersAndSources_ProgressivelyRevealOnlyAcquiredInformation()
        {
            var log = new EventLog();
            var system = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), log);

            Assert.AreEqual(0, system.GetMapReadModel().VisibleFeatures.Count);
            var proposal = system.ProposeCommunityKnowledge(
                D12MapDiscoveryCatalog.UnknownCoastId,
                "community:green_cay:elder",
                "green_cay:elder_testimony",
                "testimony:coastline:01",
                "老人说退潮时能看见一段陌生沙岸。",
                1,
                0.90f);
            Assert.IsNotNull(proposal);
            Assert.AreEqual(0, system.GetMapReadModel().VisibleFeatures.Count,
                "Agent 提案不能直接解锁地图。");

            var rumor = system.AcceptCommunityKnowledge(new D12CommunityKnowledgeAcceptance
            {
                Proposal = proposal,
                CommunityReceiptId = "community_receipt_green_cay_01",
                CommunityConfirmed = true,
                SaveRevision = 1,
            });
            Assert.IsTrue(rumor.Accepted, rumor.Message);
            Assert.AreEqual(D12DiscoveryStage.Rumor, rumor.Stage);
            Assert.AreEqual(D12MapCredibilityBand.Rumored, rumor.CredibilityBand);
            Assert.AreEqual(1, system.GetMapReadModel().VisibleFeatures.Count);
            Assert.AreEqual(string.Empty, system.GetMapReadModel().VisibleFeatures[0].LocationRef,
                "传闻只能显示区域，不显示精确海岸位置。");

            var trade = system.RecordTradeKnowledge(
                D12MapDiscoveryCatalog.ReefId,
                "chart_vendor_santa_margarita",
                "trade_chart:reef:01",
                "商贩旧图在港外东侧画了一个浅色水纹标记。",
                "santa_margarita",
                2,
                2);
            Assert.IsTrue(trade.Accepted, trade.Message);
            Assert.AreEqual(D12DiscoveryStage.Approximate, trade.Stage);
            Assert.AreEqual(D12MapLayer.NavigationHazards,
                system.GetMapReadModel().VisibleFeatures[1].Layer);

            var quest = system.RecordQuestKnowledge(
                D12MapDiscoveryCatalog.ReefId,
                "quest_first_chart",
                "event_d9_treasure_clue",
                "任务日志把东礁与退潮窗口联系起来。",
                "green_cay",
                3,
                3);
            Assert.IsTrue(quest.Accepted, quest.Message);
            Assert.GreaterOrEqual((int)quest.Stage, (int)D12DiscoveryStage.Reliable);
            Assert.GreaterOrEqual(quest.Credibility, 0.80f);

            var telescope = system.UseTelescope(new D12TelescopeRequest
            {
                FeatureId = D12MapDiscoveryCatalog.FishingGroundId,
                DistanceNauticalMiles = 4f,
                Visibility = 0.80f,
                LocationRef = "open_sea",
                WorldDay = 4,
                SaveRevision = 4,
            });
            Assert.IsTrue(telescope.Succeeded, telescope.Message);
            Assert.AreEqual(D12MapFeatureType.FishingGround,
                system.GetMapReadModel().VisibleFeatures[2].Type);
            Assert.AreEqual(D12MapDiscoveryIds.TelescopeToolId, telescope.ToolId);

            var sounding = system.UseSoundingLine(new D12SoundingLineRequest
            {
                FeatureId = D12MapDiscoveryCatalog.ReefId,
                LineLengthMeters = 10f,
                LocationRef = "nearshore_saint_marys",
                WorldDay = 5,
                SaveRevision = 5,
            });
            Assert.IsTrue(sounding.Succeeded, sounding.Message);
            Assert.IsTrue(sounding.BottomReached);
            Assert.AreEqual(1.6f, sounding.MeasuredDepthMeters, 0.001f);
            var reefView = FindView(system.GetMapReadModel(), D12MapDiscoveryCatalog.ReefId);
            Assert.AreEqual(D12DiscoveryStage.Verified, reefView.Stage);
            Assert.IsTrue(reefView.HasMeasuredDepth);
            Assert.AreEqual(1.6f, reefView.MeasuredDepthMeters, 0.001f);

            foreach (var fact in log.Facts)
                Assert.AreEqual(D12MapDiscoveryIds.SystemOwner, fact.OwnerSystem);
        }

        [Test]
        public void ToolsExplainFailure_AndDoNotWriteStateWithoutUsableEvidence()
        {
            var system = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), new EventLog());
            var shortLine = system.UseSoundingLine(new D12SoundingLineRequest
            {
                FeatureId = D12MapDiscoveryCatalog.ShoalId,
                LineLengthMeters = 1f,
                WorldDay = 1,
                SaveRevision = 1,
            });
            Assert.IsFalse(shortLine.Succeeded);
            Assert.AreEqual("line_too_short", shortLine.Code);
            Assert.AreEqual(D12DiscoveryStage.Unknown,
                system.GetFeatureState(D12MapDiscoveryCatalog.ShoalId).Stage);

            var lowVisibility = system.UseTelescope(new D12TelescopeRequest
            {
                FeatureId = D12MapDiscoveryCatalog.UnknownCoastId,
                DistanceNauticalMiles = 2f,
                Visibility = 0.10f,
                WorldDay = 1,
                SaveRevision = 2,
            });
            Assert.IsFalse(lowVisibility.Succeeded);
            Assert.AreEqual("visibility_too_low", lowVisibility.Code);

            var noCommunityReceipt = system.AcceptCommunityKnowledge(new D12CommunityKnowledgeAcceptance
            {
                Proposal = system.ProposeCommunityKnowledge(
                    D12MapDiscoveryCatalog.UnknownCoastId, "agent:crew", "rumor", "evidence", "听说北边有岸。", 1, 0.3f),
                CommunityConfirmed = false,
                SaveRevision = 3,
            });
            Assert.IsFalse(noCommunityReceipt.Accepted);
            Assert.AreEqual("community_confirmation_required", noCommunityReceipt.Code);
            Assert.AreEqual(0, system.EventLog.Count);
        }

        [Test]
        public void D12Snapshot_SaveLoad_PreservesDiscoveryAndSharedFacts()
        {
            string dbPath = Path.Combine(Path.GetTempPath(), "wb_d12_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = dbPath;
            try
            {
                SaveSystem.NewGame(new NewGameParams { CaptainName = "D12 测试船长" });
                var log = new EventLog();
                var system = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), log);
                var discovery = system.UseSoundingLine(new D12SoundingLineRequest
                {
                    FeatureId = D12MapDiscoveryCatalog.ShoalId,
                    LineLengthMeters = 5f,
                    LocationRef = "nearshore_saint_marys",
                    WorldDay = 7,
                    SaveRevision = 70,
                });
                Assert.IsTrue(discovery.Succeeded, discovery.Message);

                SaveSystem.SaveD12Runtime(new D12RuntimeSnapshot { MapDiscovery = system.Snapshot });
                SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
                {
                    CurrentLocationId = "nearshore_saint_marys",
                    VoyageStatus = D3VoyageStatus.Sailing,
                    EventLog = log.CaptureSnapshot(),
                });

                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load());
                var loaded = SaveSystem.LoadD12Runtime();
                var loadedD3 = SaveSystem.LoadD3Runtime();
                Assert.IsNotNull(loaded);
                Assert.IsNotNull(loaded.MapDiscovery);
                Assert.IsNotNull(loadedD3);
                Assert.IsNotNull(loadedD3.EventLog);
                Assert.AreEqual(1, loadedD3.EventLog.Facts.Count);

                var restoredLog = EventLog.FromSnapshot(loadedD3.EventLog);
                var restored = new D12MapDiscoverySystem(
                    D12MapDiscoveryCatalog.CreateVerticalSlice(), restoredLog, loaded.MapDiscovery);
                var view = FindView(restored.GetMapReadModel(), D12MapDiscoveryCatalog.ShoalId);
                Assert.AreEqual(D12DiscoveryStage.Verified, view.Stage);
                Assert.IsTrue(view.HasMeasuredDepth);
                Assert.AreEqual(2.4f, view.MeasuredDepthMeters, 0.001f);
                Assert.AreEqual(discovery.Discovery.Fact.EventId, restoredLog.Facts[0].EventId);
            }
            finally
            {
                SaveSystem.DbPath = null;
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                {
                    string path = dbPath + suffix;
                    if (File.Exists(path))
                    {
                        try { File.Delete(path); } catch { }
                    }
                }
            }
        }

        private static D12MapFeatureView FindView(D12MapReadModel readModel, string featureId)
        {
            foreach (var feature in readModel.VisibleFeatures)
                if (feature.FeatureId == featureId) return feature;
            Assert.Fail("缺少海图显示特征：" + featureId);
            return null;
        }
    }
}
