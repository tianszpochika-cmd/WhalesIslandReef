// D13-A01 产权资产测试：覆盖调查、估值、所有交易路径、法律争议、事实来源和存档恢复。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13PropertyAssetSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_property_" + Guid.NewGuid().ToString("N") + ".db");
            SaveSystem.DbPath = _dbPath;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.DbPath = null;
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                string path = _dbPath + suffix;
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        [Test]
        public void Catalog_ContainsIslandParcelHouseWarehouseWorkshopTavernShipyardFarmlandAndFishery()
        {
            var kinds = new HashSet<D13PropertyKind>(D13PropertyCatalog.All.Select(item => item.Kind));
            Assert.AreEqual(9, D13PropertyCatalog.All.Count);
            Assert.AreEqual(9, kinds.Count);
            foreach (var definition in D13PropertyCatalog.All)
            {
                definition.Validate();
                Assert.IsTrue(definition.Id.StartsWith("property_", StringComparison.Ordinal));
                Assert.IsTrue(definition.SourceNote.Length > 0);
                Assert.AreEqual("D13", definition.Phase);
            }
        }

        [Test]
        public void FullLifecycle_UsesLegalAndEconomyAuthorities_AndRestoresWithoutDuplicateSettlement()
        {
            var legal = new ScriptedLegalAuthority();
            var economy = new ScriptedEconomyAuthority();
            var eventLog = new EventLog();
            var system = NewSystem(eventLog, legal, economy);

            var warehouse = "property_warehouse_santa_marys_north_store";
            var survey = system.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = warehouse,
                OperationId = "survey_warehouse",
                EvidenceRefs = new List<string> { "title_deed_warehouse", "harbor_register_1675" },
                WorldDay = 1,
                SaveRevision = 1,
            });
            Assert.IsTrue(survey.Succeeded, survey.Message);
            Assert.AreEqual(D13PropertyIds.LegalOwner, survey.Survey.SourceAuthority);
            Assert.IsNotNull(eventLog.Find(survey.EventId, 1));

            var quote = system.RequestValuation(new D13PropertyValuationRequest
            {
                PropertyId = warehouse,
                OperationId = "quote_warehouse",
                WorldDay = 1,
                SaveRevision = 2,
            });
            Assert.IsTrue(quote.Succeeded, quote.Message);
            Assert.AreEqual(D13PropertyIds.EconomyOwner, quote.Valuation.SourceOwnerSystem);
            Assert.Greater(quote.Valuation.Amount, 0);

            var purchaseRequest = new D13PropertyTransactionRequest
            {
                PropertyId = warehouse,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "purchase_warehouse",
                CauseEventIds = new List<string> { survey.EventId, quote.EventId },
                WorldDay = 2,
                SaveRevision = 3,
            };
            var purchase = system.Purchase(purchaseRequest);
            Assert.IsTrue(purchase.Succeeded, purchase.Message);
            Assert.AreEqual(D13PropertyIds.PlayerRef, purchase.Property.OwnerRef);
            Assert.AreEqual(D13OwnershipKind.PrivateOwnership, purchase.Property.Ownership);
            Assert.IsNotNull(purchase.Fact);
            Assert.AreEqual(D13PropertyIds.SystemOwner, purchase.Fact.OwnerSystem);
            Assert.IsTrue(purchase.Fact.Result.Values["legal_source"].Length > 0);
            Assert.IsTrue(purchase.Fact.Result.Values["economy_source"].Length > 0);

            var mortgage = system.Mortgage(new D13PropertyTransactionRequest
            {
                PropertyId = warehouse,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "mortgage_warehouse",
                CauseEventIds = new List<string> { purchase.EventId },
                WorldDay = 3,
                SaveRevision = 4,
            });
            Assert.IsTrue(mortgage.Succeeded, mortgage.Message);
            Assert.AreEqual(D13OwnershipKind.Mortgaged, mortgage.Property.Ownership);
            Assert.AreEqual("ledger_mortgage_warehouse", mortgage.Property.MortgageRef);

            var sale = system.Sell(new D13PropertyTransactionRequest
            {
                PropertyId = warehouse,
                ApplicantRef = D13PropertyIds.PlayerRef,
                CounterpartyRef = "buyer_saint_marys",
                OperationId = "sale_warehouse",
                CauseEventIds = new List<string> { mortgage.EventId },
                WorldDay = 4,
                SaveRevision = 5,
            });
            Assert.IsTrue(sale.Succeeded, sale.Message);
            Assert.AreEqual("buyer_saint_marys", sale.Property.OwnerRef);

            var farmland = "property_farmland_green_cay_cassava";
            PrepareSurveyAndValuation(system, farmland, 10);
            var lease = system.Lease(new D13PropertyTransactionRequest
            {
                PropertyId = farmland,
                ApplicantRef = D13PropertyIds.PlayerRef,
                LeaseEndDay = 30,
                OperationId = "lease_farmland",
                WorldDay = 11,
                SaveRevision = 12,
            });
            Assert.IsTrue(lease.Succeeded, lease.Message);
            Assert.AreEqual(D13OwnershipKind.Leasehold, lease.Property.Ownership);
            Assert.AreEqual(D13PropertyIds.PlayerRef, lease.Property.LeaseHolderRef);

            var fishery = "property_fishery_green_cay_shallow_reef";
            PrepareSurveyAndValuation(system, fishery, 20);
            var coGovern = system.CoGovern(new D13PropertyTransactionRequest
            {
                PropertyId = fishery,
                ApplicantRef = D13PropertyIds.PlayerRef,
                CoGovernanceRefs = new List<string> { "community_green_cay", D13PropertyIds.PlayerRef },
                OperationId = "cogovern_fishery",
                WorldDay = 21,
                SaveRevision = 22,
            });
            Assert.IsTrue(coGovern.Succeeded, coGovern.Message);
            Assert.AreEqual(D13OwnershipKind.CoGovernance, coGovern.Property.Ownership);
            CollectionAssert.Contains(coGovern.Property.CoGovernanceRefs, "community_green_cay");

            var house = "property_house_santa_marys_quayside";
            PrepareSurveyAndValuation(system, house, 30);
            var housePurchase = system.Purchase(new D13PropertyTransactionRequest
            {
                PropertyId = house,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "purchase_house",
                WorldDay = 31,
                SaveRevision = 32,
            });
            Assert.IsTrue(housePurchase.Succeeded, housePurchase.Message);
            var transfer = system.Transfer(new D13PropertyTransactionRequest
            {
                PropertyId = house,
                ApplicantRef = D13PropertyIds.PlayerRef,
                CounterpartyRef = "family_heir",
                OperationId = "transfer_house",
                WorldDay = 33,
                SaveRevision = 34,
            });
            Assert.IsTrue(transfer.Succeeded, transfer.Message);
            Assert.AreEqual(D13OwnershipKind.Transferred, transfer.Property.Ownership);
            Assert.AreEqual("family_heir", transfer.Property.OwnerRef);

            var workshop = "property_workshop_driftwood_ropewalk";
            PrepareSurveyAndValuation(system, workshop, 40);
            var workshopPurchase = system.Purchase(new D13PropertyTransactionRequest
            {
                PropertyId = workshop,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "purchase_workshop",
                WorldDay = 41,
                SaveRevision = 42,
            });
            Assert.IsTrue(workshopPurchase.Succeeded, workshopPurchase.Message);
            var inherit = system.Inherit(new D13PropertyTransactionRequest
            {
                PropertyId = workshop,
                ApplicantRef = D13PropertyIds.PlayerRef,
                CounterpartyRef = "family_heir",
                OperationId = "inherit_workshop",
                WorldDay = 43,
                SaveRevision = 44,
            });
            Assert.IsTrue(inherit.Succeeded, inherit.Message);
            Assert.AreEqual(D13OwnershipKind.Inherited, inherit.Property.Ownership);
            Assert.AreEqual("family_heir", inherit.Property.OwnerRef);

            var dispute = system.OpenDispute(new D13PropertyTransactionRequest
            {
                PropertyId = warehouse,
                ApplicantRef = "claimant_saint_marys",
                DisputeId = "dispute_warehouse_1675",
                OperationId = "open_warehouse_dispute",
                WorldDay = 50,
                SaveRevision = 51,
            });
            Assert.IsTrue(dispute.Succeeded, dispute.Message);
            Assert.AreEqual(D13OwnershipKind.Disputed, dispute.Property.Ownership);
            Assert.AreEqual(D13PropertyDisputeStatus.Open, dispute.Property.DisputeStatus);

            var resolve = system.ResolveDispute(new D13PropertyTransactionRequest
            {
                PropertyId = warehouse,
                ApplicantRef = "claimant_saint_marys",
                CounterpartyRef = "claimant_saint_marys",
                DisputeId = "dispute_warehouse_1675",
                OperationId = "resolve_warehouse_dispute",
                CauseEventIds = new List<string> { dispute.EventId },
                WorldDay = 52,
                SaveRevision = 53,
            });
            Assert.IsTrue(resolve.Succeeded, resolve.Message);
            Assert.AreEqual(D13PropertyDisputeStatus.ResolvedForClaimant, resolve.Property.DisputeStatus);
            Assert.AreEqual("claimant_saint_marys", resolve.Property.OwnerRef);

            int factsBeforeDuplicate = eventLog.Count;
            var duplicatePurchase = system.Purchase(purchaseRequest);
            Assert.IsTrue(duplicatePurchase.Succeeded);
            Assert.IsTrue(duplicatePurchase.Duplicate);
            Assert.AreEqual(factsBeforeDuplicate, eventLog.Count);

            var propertySnapshot = system.CaptureSnapshot();
            propertySnapshot.Validate();
            var restoredEventLog = new EventLog(eventLog.CaptureSnapshot());
            var restored = NewSystem(restoredEventLog, legal, economy, propertySnapshot);
            Assert.AreEqual("claimant_saint_marys", restored.FindState(warehouse).OwnerRef);
            Assert.AreEqual(D13OwnershipKind.Leasehold, restored.FindState(farmland).Ownership);
            Assert.AreEqual(D13OwnershipKind.CoGovernance, restored.FindState(fishery).Ownership);
            var duplicateAfterLoad = restored.Purchase(purchaseRequest);
            Assert.IsTrue(duplicateAfterLoad.Duplicate, "读档后的同一产权事件不应再次转移或追加事实。");
            Assert.AreEqual(eventLog.Count, restoredEventLog.Count);
        }

        [Test]
        public void AuthorityDenial_DoesNotChangeOwnershipOrInventPrice()
        {
            var legal = new ScriptedLegalAuthority { Allow = false };
            var economy = new ScriptedEconomyAuthority { Allow = false };
            var system = NewSystem(new EventLog(), legal, economy);
            var propertyId = "property_parcel_green_cay_mangrove_edge";
            var initial = system.FindState(propertyId);

            var deniedSurvey = system.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = propertyId,
                OperationId = "denied_survey",
                WorldDay = 1,
                SaveRevision = 1,
            });
            Assert.IsFalse(deniedSurvey.Succeeded);
            Assert.AreEqual(initial.OwnerRef, system.FindState(propertyId).OwnerRef);
            Assert.IsNull(system.GetReadModel().Single(item => item.Definition.Id == propertyId).LatestValuation);

            legal.Allow = true;
            var approvedSurvey = system.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = propertyId,
                OperationId = "approved_survey",
                WorldDay = 2,
                SaveRevision = 2,
            });
            Assert.IsTrue(approvedSurvey.Succeeded, approvedSurvey.Message);
            var deniedValuation = system.RequestValuation(new D13PropertyValuationRequest
            {
                PropertyId = propertyId,
                OperationId = "denied_quote",
                WorldDay = 2,
                SaveRevision = 3,
            });
            Assert.IsFalse(deniedValuation.Succeeded);

            economy.Allow = true;
            var valuation = system.RequestValuation(new D13PropertyValuationRequest
            {
                PropertyId = propertyId,
                OperationId = "approved_quote",
                WorldDay = 3,
                SaveRevision = 4,
            });
            Assert.IsTrue(valuation.Succeeded, valuation.Message);
            var purchase = system.Purchase(new D13PropertyTransactionRequest
            {
                PropertyId = propertyId,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OfferedAmount = 1,
                OperationId = "denied_purchase",
                WorldDay = 4,
                SaveRevision = 5,
            });
            Assert.IsTrue(purchase.Succeeded, purchase.Message);
            Assert.AreEqual(valuation.Valuation.Amount, purchase.Amount, "交易金额必须来自 Economy authority，不能采用 UI/Agent 提供的报价。");
            Assert.AreEqual(D13PropertyIds.PlayerRef, purchase.Property.OwnerRef);
        }

        [Test]
        public void SaveSystem_PersistsPropertyOwnershipSurveyValuationAndDisputeProjection()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "产权测试船长" });
            var eventLog = new EventLog();
            var system = NewSystem(eventLog, new ScriptedLegalAuthority(), new ScriptedEconomyAuthority());
            var propertyId = "property_warehouse_santa_marys_north_store";
            system.Investigate(new D13PropertySurveyRequest { PropertyId = propertyId, OperationId = "save_survey", WorldDay = 1, SaveRevision = 1 });
            system.RequestValuation(new D13PropertyValuationRequest { PropertyId = propertyId, OperationId = "save_quote", WorldDay = 1, SaveRevision = 2 });
            var purchase = system.Purchase(new D13PropertyTransactionRequest
            {
                PropertyId = propertyId,
                ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "save_purchase",
                WorldDay = 2,
                SaveRevision = 3,
            });
            Assert.IsTrue(purchase.Succeeded, purchase.Message);
            SaveSystem.SaveD13Runtime(system.CaptureSnapshot());

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var restored = SaveSystem.LoadD13Runtime();
            Assert.IsNotNull(restored);
            Assert.AreEqual(D13PropertyIds.PlayerRef,
                restored.Properties.Single(item => item.PropertyId == propertyId).OwnerRef);
            Assert.IsTrue(restored.Surveys.Single(item => item.PropertyId == propertyId).Approved);
            Assert.Greater(restored.Valuations.Single(item => item.PropertyId == propertyId).Amount, 0);
        }

        private static D13PropertyAssetSystem NewSystem(EventLog log, ScriptedLegalAuthority legal,
            ScriptedEconomyAuthority economy, D13PropertyRuntimeSnapshot initial = null)
        {
            return new D13PropertyAssetSystem(log, D13PropertyCatalog.All, legal, economy, initial);
        }

        private static void PrepareSurveyAndValuation(D13PropertyAssetSystem system, string propertyId, long seed)
        {
            Assert.IsTrue(system.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = propertyId,
                OperationId = "survey_" + propertyId,
                WorldDay = (int)seed,
                SaveRevision = seed + 1,
            }).Succeeded);
            Assert.IsTrue(system.RequestValuation(new D13PropertyValuationRequest
            {
                PropertyId = propertyId,
                OperationId = "quote_" + propertyId,
                WorldDay = (int)seed,
                SaveRevision = seed + 2,
            }).Succeeded);
        }

        private sealed class ScriptedLegalAuthority : ID13PropertyLegalAuthority
        {
            public bool Allow = true;

            public D13LegalDecision Investigate(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertySurveyRequest request)
            {
                return new D13LegalDecision
                {
                    Allowed = Allow,
                    Code = Allow ? "title_confirmed" : "title_disputed",
                    Message = Allow ? "产权登记与调查证据一致。" : "产权文书存在争议。",
                    LegalStatus = definition.ProtectedArea ? D13PropertyLegalStatus.Protected : D13PropertyLegalStatus.Registered,
                    TitleHolderRef = definition.DefaultOwnerRef,
                    SourceEventId = "legal_survey_" + request.OperationId,
                };
            }

            public D13LegalDecision Authorize(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyTransactionRequest request, D13PropertySurveyRecord survey)
            {
                if (request.Kind == D13PropertyTransactionKind.OpenDispute)
                    return new D13LegalDecision
                    {
                        Allowed = true,
                        Code = "dispute_registered",
                        Message = "争议已由登记机关收案。",
                        LegalStatus = D13PropertyLegalStatus.Disputed,
                        DisputeStatus = D13PropertyDisputeStatus.Open,
                        SourceEventId = "legal_dispute_open_" + request.OperationId,
                    };
                if (request.Kind == D13PropertyTransactionKind.ResolveDispute)
                    return new D13LegalDecision
                    {
                        Allowed = true,
                        Code = "dispute_resolved",
                        Message = "争议依据文书与见证记录解决。",
                        LegalStatus = D13PropertyLegalStatus.Registered,
                        DisputeStatus = D13PropertyDisputeStatus.ResolvedForClaimant,
                        ResolvedOwnerRef = request.CounterpartyRef,
                        SourceEventId = "legal_dispute_resolve_" + request.OperationId,
                    };
                return new D13LegalDecision
                {
                    Allowed = true,
                    Code = "transfer_authorized",
                    Message = "法律与势力 authority 已批准该权利操作。",
                    LegalStatus = definition.ProtectedArea ? D13PropertyLegalStatus.Protected : D13PropertyLegalStatus.Registered,
                    SourceEventId = "legal_transaction_" + request.OperationId,
                };
            }
        }

        private sealed class ScriptedEconomyAuthority : ID13PropertyEconomyAuthority
        {
            public bool Allow = true;

            public D13EconomyValuationDecision Quote(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyValuationRequest request)
            {
                return new D13EconomyValuationDecision
                {
                    Approved = Allow,
                    Code = Allow ? "valuation_approved" : "valuation_denied",
                    Message = Allow ? "经济系统估值完成。" : "经济系统拒绝估值。",
                    Quote = new D13ValuationQuote
                    {
                        PropertyId = definition.Id,
                        Amount = 100 + definition.AreaUnits,
                        Currency = "coin",
                        ValuationBasis = "fixture_market_register",
                        SourceOwnerSystem = D13PropertyIds.EconomyOwner,
                        SourceEventId = "economy_quote_" + request.OperationId,
                        Reasons = new List<string> { "fixture_market_register", definition.Kind.ToString() },
                    },
                };
            }

            public D13EconomyTransactionDecision Approve(D13PropertyDefinition definition, D13PropertyState state,
                D13PropertyTransactionRequest request, D13ValuationQuote valuation)
            {
                return new D13EconomyTransactionDecision
                {
                    Approved = Allow,
                    Code = Allow ? "ledger_approved" : "ledger_denied",
                    Message = Allow ? "经济系统已批准账本请求。" : "经济系统拒绝账本请求。",
                    Amount = valuation == null ? 0 : valuation.Amount,
                    LedgerRef = request.Kind == D13PropertyTransactionKind.Mortgage
                        ? "ledger_" + request.OperationId : "ledger_settlement_" + request.OperationId,
                    SourceOwnerSystem = D13PropertyIds.EconomyOwner,
                    SourceEventId = "economy_settlement_" + request.OperationId,
                };
            }
        }
    }
}
