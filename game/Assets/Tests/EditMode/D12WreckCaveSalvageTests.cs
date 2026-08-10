using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D12;
using Whalebone.Systems.Events;

namespace D12.Tests
{
    public sealed class D12WreckCaveSalvageTests
    {
        [Test]
        public void TideAndEquipmentFailures_KeepRecoveryRoutesAndFacts()
        {
            D12MapDiscoverySystem map;
            EventLog log;
            TestLegalAuthority legal;
            D12WreckCaveSalvageSystem system = CreateSystem(out map, out log, out legal);
            Chart(map, D12SalvageIds.WreckMapFeatureId);

            var missingEquipment = system.TrySalvage(new D12SalvageRequest
            {
                AttemptId = "wreck_missing_equipment",
                SiteId = D12WreckCaveSalvageCatalog.WreckSiteId,
                WorldDay = 2,
                SaveRevision = 2,
                TideWindowOpen = true,
                TideStage = D12TideStage.Low,
                EquipmentIds = new List<string>(),
                RopeMeters = 0,
                LightHoursAvailable = 0,
            });
            Assert.IsTrue(missingEquipment.Accepted);
            Assert.AreEqual(D12SalvageFailureCode.EquipmentRequired, missingEquipment.FailureCode);
            Assert.IsNotNull(missingEquipment.RecoveryRoute);

            var equipmentRecovery = system.Recover(new D12SalvageRecoveryRequest
            {
                RecoveryAttemptId = "wreck_recover_equipment",
                SiteId = D12WreckCaveSalvageCatalog.WreckSiteId,
                RouteId = "route_wreck_prepare_equipment",
                WorldDay = 3,
                SaveRevision = 3,
            });
            Assert.IsTrue(equipmentRecovery.Recovered);

            var tideClosed = system.TrySalvage(new D12SalvageRequest
            {
                AttemptId = "wreck_tide_closed",
                SiteId = D12WreckCaveSalvageCatalog.WreckSiteId,
                WorldDay = 4,
                SaveRevision = 4,
                TideWindowOpen = false,
                TideStage = D12TideStage.High,
                EquipmentIds = WreckEquipment(),
                RopeMeters = 24,
            });
            Assert.IsTrue(tideClosed.Accepted);
            Assert.AreEqual(D12SalvageFailureCode.TideWindowClosed, tideClosed.FailureCode);
            Assert.AreEqual("route_wreck_wait_for_tide", tideClosed.RecoveryRoute.RouteId);
            Assert.GreaterOrEqual(log.Count, 2);
        }

        [Test]
        public void CompetitorAndRiskChangeOutcome_WhileLegalAuthorityRemainsExternal()
        {
            D12MapDiscoverySystem plainMap;
            EventLog plainLog;
            TestLegalAuthority plainLegal;
            var plainSystem = CreateSystem(out plainMap, out plainLog, out plainLegal);
            Chart(plainMap, D12SalvageIds.WreckMapFeatureId);

            D12MapDiscoverySystem contestedMap;
            EventLog contestedLog;
            TestLegalAuthority contestedLegal;
            var contestedSystem = CreateSystem(out contestedMap, out contestedLog, out contestedLegal);
            Chart(contestedMap, D12SalvageIds.WreckMapFeatureId);

            var definition = FindSite(D12WreckCaveSalvageCatalog.WreckSiteId);
            var plainRequest = BaseWreckRequest("wreck_plain", false);
            var contestedRequest = BaseWreckRequest("wreck_competitor", true);
            var equipment = new HashSet<string>(WreckEquipment(), StringComparer.Ordinal);
            int plainThreshold = D12WreckCaveSalvageSystem.CalculateRiskThreshold(definition, plainRequest, equipment);
            int contestedThreshold = D12WreckCaveSalvageSystem.CalculateRiskThreshold(definition, contestedRequest, equipment);
            int seed = FindSeed(definition.SiteId, plainRequest.AttemptId, contestedRequest.AttemptId,
                plainThreshold, contestedThreshold, definition.BaseRiskPercent + definition.CompetitorRiskPercent + 25);
            plainRequest.RiskSeed = seed;
            contestedRequest.RiskSeed = seed;

            var plain = plainSystem.TrySalvage(plainRequest);
            var contested = contestedSystem.TrySalvage(contestedRequest);

            Assert.IsTrue(plain.Succeeded, plain.Message);
            Assert.IsFalse(contested.Succeeded);
            Assert.AreEqual(D12SalvageFailureCode.CompetitorInterference, contested.FailureCode);
            Assert.IsNotNull(contested.Loss);
            Assert.AreEqual(D12SalvageLossKind.CompetitorDispute, contested.Loss.Kind);
            Assert.AreEqual(plainLegal.LastClaimRef, "claim:wreck_saint_marys_english");
            Assert.IsTrue(contestedLegal.RuleCount > 0);
        }

        [Test]
        public void CaveCollapse_RecordsLoss_ReturnsToEntrance_AndCanResume()
        {
            D12MapDiscoverySystem map;
            EventLog log;
            TestLegalAuthority legal;
            var system = CreateSystem(out map, out log, out legal);
            Chart(map, D12SalvageIds.CaveMapFeatureId);
            var definition = FindSite(D12WreckCaveSalvageCatalog.CaveSiteId);
            var request = new D12SalvageRequest
            {
                AttemptId = "cave_collapse",
                SiteId = definition.SiteId,
                WorldDay = 6,
                SaveRevision = 6,
                TideWindowOpen = true,
                TideStage = D12TideStage.Low,
                EquipmentIds = new List<string>
                {
                    D12SalvageIds.RopeToolId,
                    D12SalvageIds.LanternToolId,
                    D12SalvageIds.OilToolId,
                },
                RopeMeters = 30,
                LightHoursAvailable = 4,
                RiskSeed = FindSeedFor(definition.SiteId, "cave_collapse", 0, definition.BaseRiskPercent + definition.CollapseRiskPercent),
            };

            var failed = system.TrySalvage(request);
            Assert.IsTrue(failed.Accepted);
            Assert.AreEqual(D12SalvageFailureCode.CaveCollapse, failed.FailureCode);
            Assert.AreEqual("route_cave_return_to_entrance", failed.RecoveryRoute.RouteId);
            Assert.IsNotNull(failed.Loss);
            Assert.AreEqual(D12SalvageLossKind.CaveCollapse, failed.Loss.Kind);

            var recovered = system.Recover(new D12SalvageRecoveryRequest
            {
                RecoveryAttemptId = "cave_return",
                SiteId = definition.SiteId,
                RouteId = "route_cave_return_to_entrance",
                WorldDay = 7,
                SaveRevision = 7,
                EquipmentIds = new List<string> { D12SalvageIds.RopeToolId },
            });
            Assert.IsTrue(recovered.Recovered);
            Assert.AreEqual(1, system.Snapshot.Sites[1].LossRecords.Count);

            var success = request;
            success.AttemptId = "cave_resume_success";
            success.SaveRevision = 8;
            success.WorldDay = 8;
            success.EquipmentIds.Add(D12SalvageIds.CaveMarkerToolId);
            var successEquipment = new HashSet<string>(success.EquipmentIds, StringComparer.Ordinal);
            int successThreshold = D12WreckCaveSalvageSystem.CalculateRiskThreshold(definition, success, successEquipment);
            success.RiskSeed = FindSeedFor(definition.SiteId, success.AttemptId, successThreshold, 99);
            var resumed = system.TrySalvage(success);
            Assert.IsTrue(resumed.Succeeded, resumed.Message);
            Assert.IsNotNull(resumed.RewardRequest);
            Assert.AreEqual(1, system.Snapshot.Sites[1].LossRecords.Count);
            Assert.Greater(log.Count, 1);
        }

        [Test]
        public void SaveRestore_PreservesSalvageAndDoesNotRegenerateRewardRequest()
        {
            D12MapDiscoverySystem map;
            EventLog log;
            TestLegalAuthority legal;
            var system = CreateSystem(out map, out log, out legal);
            Chart(map, D12SalvageIds.WreckMapFeatureId);
            var request = BaseWreckRequest("wreck_save_success", false);
            var definition = FindSite(D12WreckCaveSalvageCatalog.WreckSiteId);
            var equipment = new HashSet<string>(WreckEquipment(), StringComparer.Ordinal);
            int threshold = D12WreckCaveSalvageSystem.CalculateRiskThreshold(definition, request, equipment);
            request.RiskSeed = FindSeedFor(definition.SiteId, request.AttemptId, threshold, 99);
            var first = system.TrySalvage(request);
            Assert.IsTrue(first.Succeeded, first.Message);
            var rewardRequestId = first.RewardRequest.RequestId;
            var factCount = log.Count;
            var runtime = new D12RuntimeSnapshot
            {
                MapDiscovery = map.Snapshot,
                Salvage = system.Snapshot,
            };
            runtime.Validate();

            var restoredLog = EventLog.FromSnapshot(log.CaptureSnapshot());
            var restoredMap = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), restoredLog, runtime.MapDiscovery);
            var restoredSystem = new D12WreckCaveSalvageSystem(
                D12WreckCaveSalvageCatalog.CreateVerticalSlice(), restoredMap, restoredLog, legal, runtime.Salvage);
            var duplicate = restoredSystem.TrySalvage(request);

            Assert.IsFalse(duplicate.Accepted);
            Assert.AreEqual("already_salvaged", duplicate.Code);
            Assert.AreEqual(rewardRequestId, restoredSystem.Snapshot.Sites[0].RewardRequest.RequestId);
            Assert.AreEqual(factCount, restoredLog.Count);
        }

        private static D12WreckCaveSalvageSystem CreateSystem(out D12MapDiscoverySystem map,
            out EventLog log, out TestLegalAuthority legal)
        {
            log = new EventLog();
            map = new D12MapDiscoverySystem(D12MapDiscoveryCatalog.CreateVerticalSlice(), log);
            legal = new TestLegalAuthority { Allowed = true };
            return new D12WreckCaveSalvageSystem(
                D12WreckCaveSalvageCatalog.CreateVerticalSlice(), map, log, legal);
        }

        private static void Chart(D12MapDiscoverySystem map, string featureId)
        {
            var result = map.Discover(new D12DiscoveryRequest
            {
                FeatureId = featureId,
                SourceRef = "test:direct_sighting",
                EvidenceRef = "test:chart:" + featureId,
                Summary = "测试用直接观察已确认探索地点位置。",
                LocationRef = featureId,
                WorldDay = 1,
                SaveRevision = 1,
                Strength = 0.80f,
            });
            Assert.IsTrue(result.Accepted, result.Message);
        }

        private static D12SalvageRequest BaseWreckRequest(string attemptId, bool competitor)
        {
            return new D12SalvageRequest
            {
                AttemptId = attemptId,
                SiteId = D12WreckCaveSalvageCatalog.WreckSiteId,
                WorldDay = 5,
                SaveRevision = 5,
                TideWindowOpen = true,
                TideStage = D12TideStage.Low,
                EquipmentIds = WreckEquipment(),
                RopeMeters = 24,
                Competitor = new D12SalvageCompetitorObservation
                {
                    Present = competitor,
                    CompetitorRef = competitor ? "competitor:santa_marys_privateer" : null,
                    ClaimRef = competitor ? "claim:wreck_saint_marys_privateer" : null,
                    EvidenceRef = competitor ? "test:competitor" : null,
                    PressurePercent = competitor ? 100 : 0,
                },
            };
        }

        private static List<string> WreckEquipment()
        {
            return new List<string> { D12SalvageIds.GrapnelToolId, D12SalvageIds.RopeToolId };
        }

        private static int FindSeed(string siteId, string plainAttemptId, string contestedAttemptId,
            int plainThreshold, int contestedThreshold, int competitorBoundary)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                int plain = D12SalvageRiskCalculator.Roll(siteId, plainAttemptId, seed);
                int contested = D12SalvageRiskCalculator.Roll(siteId, contestedAttemptId, seed);
                if (plain >= plainThreshold && contested < contestedThreshold && contested < competitorBoundary)
                    return seed;
            }
            Assert.Fail("没有找到确定性风险测试种子。");
            return 0;
        }

        private static int FindSeedFor(string siteId, string attemptId, int minimumInclusive, int maximumInclusive)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                int roll = D12SalvageRiskCalculator.Roll(siteId, attemptId, seed);
                if (roll >= minimumInclusive && roll <= maximumInclusive) return seed;
            }
            Assert.Fail("没有找到确定性风险测试种子。");
            return 0;
        }

        private static D12ExplorationSiteDefinition FindSite(string siteId)
        {
            foreach (var definition in D12WreckCaveSalvageCatalog.CreateVerticalSlice())
                if (definition.SiteId == siteId) return definition;
            Assert.Fail("找不到测试地点：" + siteId);
            return null;
        }

        private sealed class TestLegalAuthority : ID12SalvageLegalAuthority
        {
            public bool Allowed;
            public int RuleCount;
            public string LastClaimRef;

            public D12SalvageLegalDecision Rule(D12SalvageLegalRequest request)
            {
                RuleCount++;
                LastClaimRef = request.ClaimRef;
                return new D12SalvageLegalDecision
                {
                    RulingAvailable = true,
                    SalvageAllowed = Allowed,
                    ClaimStatus = Allowed ? "licensed" : "contested",
                    RulingRef = "test:legal:ruling:" + request.SiteId,
                    ClaimantRef = "faction:england",
                    ControllingFactionId = "england",
                    PermitRef = Allowed ? "permit:test_salvage" : "",
                    Reason = Allowed ? "测试法律 authority 允许当前申请。" : "测试法律 authority 拒绝当前申请。",
                };
            }
        }
    }
}
