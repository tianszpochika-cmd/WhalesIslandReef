// D14-A05：势力、声望、私掠、港口法律、检查扣押、罚金赎金、护航、封锁与外交意图。

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D14;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14FactionLegalSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_faction_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void LegalLifecycleIsFactDrivenAndSaveRestoreIsIdempotent()
        {
            var log = CreateSources();
            var authority = new AuthorityFixture(log);
            var system = new D14FactionLegalSystem(log, null, authority, authority, authority);

            var reputation = system.ApplyReputation(new D14ReputationRequest
            {
                OperationId = "rep_spain", FactionId = "spain", Delta = 40, Reason = "护航交付和边境协助",
                SourceEventId = "legal_order", WorldDay = 1, SaveRevision = 0, EvidenceEventIds = new List<string> { "escort_success" }, CauseEventIds = new List<string> { "escort_success" },
            });
            Assert.IsTrue(reputation.Accepted, reputation.Message);
            Assert.AreEqual(40, system.GetReputation("spain"));

            var war = system.DeclareWar(new D14WarRequest
            {
                OperationId = "war_england_spain", FactionA = "england", FactionB = "spain", TreatyRef = "war_declaration_1675",
                SourceEventId = "diplomatic_war", WorldDay = 2, SaveRevision = 0, EvidenceEventIds = new List<string> { "diplomatic_war" },
            });
            Assert.IsTrue(war.Accepted, war.Message);
            Assert.IsTrue(system.IsAtWar("england", "spain"));
            var truce = system.SignTruce(new D14TruceRequest
            {
                OperationId = "truce_england_spain", FactionA = "england", FactionB = "spain", TreatyRef = "truce_1675",
                SourceEventId = "diplomatic_truce", WorldDay = 3, SaveRevision = 0, EvidenceEventIds = new List<string> { "diplomatic_truce" },
            });
            Assert.IsTrue(truce.Accepted, truce.Message);
            Assert.IsFalse(system.IsAtWar("england", "spain"));

            var permit = system.IssuePrivateerPermit(new D14PrivateerPermitRequest
            {
                OperationId = "permit_player_spain", PermitId = "permit_spain_player_01", IssuerFactionId = "spain", HolderRef = "player", TargetFactionId = "england", ExpiresWorldDay = 30,
                SourceEventId = "permit_approval", WorldDay = 4, SaveRevision = 0, EvidenceEventIds = new List<string> { "permit_approval" },
            });
            Assert.IsTrue(permit.Accepted, permit.Message);
            Assert.IsTrue(system.HasPrivateerPermit("player", "spain", 30));

            var escort = system.CompleteEscort(new D14EscortRequest
            {
                OperationId = "escort_contract_01", ContractId = "escort_contract_01", EscorterRef = "player", IssuerFactionId = "spain", RouteId = "route_santa_margarita", ProtectedCargoManifestEventId = "cargo_manifest",
                SourceEventId = "escort_success", WorldDay = 5, SaveRevision = 0, EvidenceEventIds = new List<string> { "escort_success" },
            });
            Assert.IsTrue(escort.Accepted, escort.Message);
            Assert.IsTrue(escort.Escort.Success);

            var inspection = system.Inspect(new D14InspectionRequest
            {
                OperationId = "inspection_illegal_01", InspectionId = "inspection_illegal_01", SubjectRef = "player", PortId = "santa_margarita", ControllingFactionId = "spain", CargoManifestEventId = "cargo_manifest",
                SourceEventId = "inspection_order", WorldDay = 6, SaveRevision = 0, EvidenceEventIds = new List<string> { "inspection_order", "legal_order" },
            });
            Assert.IsTrue(inspection.Accepted, inspection.Message);
            Assert.IsNotNull(inspection.Inspection);
            Assert.IsNotNull(inspection.WantedCase);
            Assert.IsNotNull(inspection.Seizure);
            Assert.IsNotNull(inspection.FinancialCase);
            Assert.AreEqual(2, system.GetWantedLevel("player"));

            var settledFine = system.SettleFinancialCase(new D14FinancialSettlementRequest
            {
                OperationId = "settle_fine_01", CaseId = inspection.FinancialCase.CaseId, SettlementSourceEventId = "economy_settlement_fine",
                SourceEventId = "legal_settlement", WorldDay = 7, SaveRevision = 0, EvidenceEventIds = new List<string> { "economy_settlement_fine" },
            });
            Assert.IsTrue(settledFine.Accepted, settledFine.Message);
            Assert.AreEqual(D14LegalCaseStatus.Paid, settledFine.FinancialCase.Status);
            var cleared = system.ClearWanted(new D14WantedClearRequest
            {
                OperationId = "clear_wanted_01", CaseId = inspection.WantedCase.CaseId, SettlementSourceEventId = "economy_settlement_fine",
                SourceEventId = "legal_settlement", WorldDay = 8, SaveRevision = 0, EvidenceEventIds = new List<string> { "legal_order" },
            });
            Assert.IsTrue(cleared.Accepted, cleared.Message);
            Assert.AreEqual(0, system.GetWantedLevel("player"));

            var ransom = system.IssueRansom(new D14RansomRequest
            {
                OperationId = "ransom_01", CaseId = "ransom_01", SubjectRef = "player", FactionId = "spain", PrisonerRef = "prisoner_01", Reason = "战后释放",
                SourceEventId = "ransom_order", WorldDay = 9, SaveRevision = 0, EvidenceEventIds = new List<string> { "ransom_order" },
            });
            Assert.IsTrue(ransom.Accepted, ransom.Message);
            var ransomSettlement = system.SettleFinancialCase(new D14FinancialSettlementRequest
            {
                OperationId = "settle_ransom_01", CaseId = "ransom_01", SettlementSourceEventId = "economy_settlement_ransom",
                SourceEventId = "legal_settlement", WorldDay = 10, SaveRevision = 0, EvidenceEventIds = new List<string> { "economy_settlement_ransom" },
            });
            Assert.IsTrue(ransomSettlement.Accepted, ransomSettlement.Message);
            Assert.AreEqual(D14LegalCaseStatus.Released, ransomSettlement.FinancialCase.Status);

            var blockade = system.DeclareBlockade(new D14BlockadeRequest
            {
                OperationId = "blockade_green_cay", BlockadeId = "blockade_green_cay", IssuerFactionId = "spain", TargetPortId = "green_cay", Reason = "战争封锁",
                SourceEventId = "blockade_order", WorldDay = 11, SaveRevision = 0, EvidenceEventIds = new List<string> { "blockade_order" },
            });
            Assert.IsTrue(blockade.Accepted, blockade.Message);
            Assert.IsFalse(system.CanEnterPort("green_cay", "player", "england", 11));
            var lifted = system.LiftBlockade(new D14BlockadeRequest
            {
                OperationId = "lift_green_cay", BlockadeId = "blockade_green_cay", IssuerFactionId = "spain", TargetPortId = "green_cay", Reason = "外交复核完成",
                SourceEventId = "blockade_lift", WorldDay = 12, SaveRevision = 0, EvidenceEventIds = new List<string> { "blockade_lift" },
            });
            Assert.IsTrue(lifted.Accepted, lifted.Message);

            var intent = system.ProposeDiplomaticIntent(new D14DiplomaticIntentRequest
            {
                ProposalId = "intent_lift_blockade", ActorRef = "player", RequestedBySystem = "AgentReply", TargetFactionId = "spain", IntentKind = D14DiplomaticIntentKind.RequestLiftBlockade, Summary = "请求外交复核封锁。", WorldDay = 13, SaveRevision = 0,
            });
            Assert.IsTrue(intent.Accepted, intent.Message);
            Assert.AreEqual(48, system.GetReputation("spain"), "Agent/意图不应代替法律系统裁决，只保留已裁决声望事实。");

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 法律测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14FactionLegalRuntime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14FactionLegalRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(saved.Reputation["spain"], loaded.Reputation["spain"]);
            Assert.AreEqual(saved.WantedCases.Count, loaded.WantedCases.Count);
            Assert.AreEqual(saved.FinancialCases.Count, loaded.FinancialCases.Count);
            Assert.AreEqual(saved.Blockades.Count, loaded.Blockades.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = new D14FactionLegalSystem(null, null, authority, authority, authority, loaded);
            var duplicate = restored.ApplyReputation(new D14ReputationRequest
            {
                OperationId = "rep_spain", FactionId = "spain", Delta = 40, Reason = "重复读档操作不应再次写入",
                SourceEventId = "legal_order", WorldDay = 1, SaveRevision = 0, EvidenceEventIds = new List<string> { "escort_success" },
            });
            Assert.IsTrue(duplicate.Accepted && duplicate.Duplicate);
            Assert.AreEqual(saved.Audits.Count, restored.Snapshot.Audits.Count);
        }

        [Test]
        public void AgentCanOnlyProposeIntentAndIllegalAuthorityBoundaryIsRejected()
        {
            var log = CreateSources();
            var authority = new AuthorityFixture(log);
            var system = new D14FactionLegalSystem(log, null, authority, authority, authority);
            var intent = system.ProposeDiplomaticIntent(new D14DiplomaticIntentRequest
            {
                ProposalId = "agent_truce_request", ActorRef = "agent", RequestedBySystem = "AgentReply", TargetFactionId = "spain", IntentKind = D14DiplomaticIntentKind.RequestTruce, Summary = "请法律系统审阅停战。", WorldDay = 1, SaveRevision = 0,
            });
            Assert.IsTrue(intent.Accepted, intent.Message);
            Assert.AreEqual(0, system.GetReputation("spain"));
            Assert.IsFalse(system.IsAtWar("england", "spain"));

            var rejected = system.ApplyReputation(new D14ReputationRequest
            {
                OperationId = "bad_agent_write", ActorRef = "AgentReply", FactionId = "spain", Delta = 99, Reason = "Agent 不能直改", SourceEventId = "legal_order", WorldDay = 2, SaveRevision = 0, EvidenceEventIds = new List<string> { "legal_order" },
            });
            Assert.IsFalse(rejected.Accepted);
            Assert.AreEqual("agent_cannot_write", rejected.Code);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            Add(log, "legal_order", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "diplomatic_war", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "diplomatic_truce", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "permit_approval", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "inspection_order", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "blockade_order", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "blockade_lift", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "ransom_order", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "legal_settlement", D14FactionLegalIds.DiplomacyOwner, "source_ready");
            Add(log, "cargo_manifest", D14FactionLegalIds.InventoryOwner, "cargo_manifest_ready");
            Add(log, "inventory_inspection", D14FactionLegalIds.InventoryOwner, "inventory_inspection_ready");
            Add(log, "inventory_seizure", D14FactionLegalIds.InventoryOwner, "inventory_seizure_ready");
            Add(log, "economy_fine", D14FactionLegalIds.EconomyOwner, "economy_assessment_ready");
            Add(log, "economy_ransom", D14FactionLegalIds.EconomyOwner, "economy_assessment_ready");
            Add(log, "economy_settlement_fine", D14FactionLegalIds.EconomyOwner, "settlement_ready");
            Add(log, "economy_settlement_ransom", D14FactionLegalIds.EconomyOwner, "settlement_ready");
            Add(log, "escort_success", D14FactionLegalIds.EscortOwner, "escort_success");
            return log;
        }

        private static void Add(EventLog log, string id, string owner, string resultCode)
        {
            log.Append(new WorldEvent(id, 0, "d14_test", owner,
                new EventResult { ResultCode = resultCode, Summary = "D14-A05 测试事实。", FactIds = new List<string> { "source:" + id } },
                new[] { "player" }, new[] { "test:d14_a05" }));
        }

        private sealed class AuthorityFixture : ID14FactionLegalCargoAuthority, ID14FactionLegalInventoryAuthority, ID14FactionLegalEconomyAuthority
        {
            private readonly EventLog _log;
            public AuthorityFixture(EventLog log) { _log = log; }
            public D14CargoInspectionDecision Inspect(D14InspectionRequest request, FactEvent manifestFact)
            {
                return new D14CargoInspectionDecision { Allowed = true, CargoLegal = false, Code = "illegal", Message = "fixture", SourceOwnerSystem = D14FactionLegalIds.InventoryOwner, SourceEventId = "inventory_inspection", ProhibitedCargoRefs = new List<string> { "item_illegal_tobacco" } };
            }
            public D14SeizureDecision Seize(D14SeizureRequest request, FactEvent manifestFact, IReadOnlyList<string> prohibitedCargoRefs)
            {
                return new D14SeizureDecision { Allowed = true, Code = "seized", Message = "fixture", SourceOwnerSystem = D14FactionLegalIds.InventoryOwner, SourceEventId = "inventory_seizure", SeizedCargoRefs = new List<string>(prohibitedCargoRefs) };
            }
            public D14EconomicAssessmentDecision AssessFine(D14FineRequest request, FactEvent sourceFact)
            {
                return new D14EconomicAssessmentDecision { Allowed = true, Amount = 80, Code = "assessed", Message = "fixture", SourceOwnerSystem = D14FactionLegalIds.EconomyOwner, SourceEventId = "economy_fine" };
            }
            public D14EconomicAssessmentDecision AssessRansom(D14RansomRequest request, FactEvent sourceFact)
            {
                return new D14EconomicAssessmentDecision { Allowed = true, Amount = 120, Code = "assessed", Message = "fixture", SourceOwnerSystem = D14FactionLegalIds.EconomyOwner, SourceEventId = "economy_ransom" };
            }
            public D14SettlementConfirmationDecision ConfirmSettlement(D14FinancialSettlementRequest request, D14FinancialCaseState financialCase, FactEvent settlementFact)
            {
                return new D14SettlementConfirmationDecision { Allowed = true, Code = "confirmed", Message = "fixture", SourceOwnerSystem = D14FactionLegalIds.EconomyOwner, SourceEventId = settlementFact.EventId };
            }
        }
    }
}
