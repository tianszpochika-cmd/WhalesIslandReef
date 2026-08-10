// D13-S01 跨域集成验收：产权 -> 建筑/水源 -> 治理劳力 -> 农业/Ecology -> 市场 -> 金融 -> 存档。
// 测试只通过各系统公开入口和 authority 契约注入，不直接写领域状态；
// D3InventorySystem / TradeEconomySystem / D4EcologySystem 是既有 owner 的真实实现。

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13AcceptanceIntegrationTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_s01_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void PropertyWellFarmMarketLoanInsuranceTaxAndSaveRestoreAreConsistent()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "D13 集成船长", ShipTypeId = "small_fishing" });
            var log = CreateSources();
            var inventory = new D3InventoryState(500);
            D13FarmingCatalog.RegisterInventoryDefinitions();
            D13DynamicMarketCatalog.RegisterInventoryDefinitions();

            var property = new D13PropertyAssetSystem(log, null, new LegalAuthority(), new PropertyEconomyAuthority());
            var finance = new D13FinanceSystem(log, new CollateralAuthority(property));
            var account = finance.OpenAccount(new D13FinanceAccountRequest
            {
                OperationId = "open_player_account", AccountId = "account_player", OwnerRef = D13FinanceIds.PlayerRef,
                InitialCash = 5000, CreditLimit = 1000, SourceOwnerSystem = D13FinanceIds.EconomyOwner,
                SourceEventId = "economy_account", WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(account.Succeeded, account.Message);

            var farmland = LeaseFarmland(property);
            var warehouse = BuyWarehouse(property, finance, farmland.EventId);
            Assert.AreEqual(D13OwnershipKind.Leasehold, farmland.Property.Ownership);
            Assert.AreEqual(D13PropertyIds.PlayerRef, farmland.Property.LeaseHolderRef);
            Assert.AreEqual(D13PropertyIds.PlayerRef, warehouse.Property.OwnerRef);

            var governance = NewGovernance(log);
            Assert.IsTrue(governance.MigrateIn(new D13MigrationInRequest
            {
                OperationId = "migration_worker", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                ResidentId = "resident_worker", HouseholdId = "family_worker", FamilyName = "Alvarez",
                ReligionId = "religion_catholic_parish", ReligionContentId = "religion_catholic_parish_saint_marys",
                SourceEventId = "migration_worker_source", ReviewAcknowledged = true, WorldDay = 2, SaveRevision = 4,
            }).Succeeded);
            Assert.IsTrue(governance.AssignJob(new D13JobAssignmentRequest
            {
                OperationId = "assign_worker", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                ResidentId = "resident_worker", JobId = D13IslandGovernanceCatalog.JobDockhand,
                WorldDay = 2, SaveRevision = 5,
            }).Succeeded);
            Assert.IsTrue(governance.ApplyPolicy(new D13GovernancePolicyRequest
            {
                OperationId = "policy_island", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                TaxRateBps = 900, WaterRationingLevel = 0, FoodRationingLevel = 0, EducationOpen = true,
                ReligionArrangementId = "religion_catholic_parish", SecurityPostureId = "watch_strict",
                WorldDay = 2, SaveRevision = 6,
            }).Succeeded);
            var day = governance.AdvanceDay(new D13GovernanceDayRequest
            {
                OperationId = "governance_supply_day", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                Seed = 17, WorldDay = 3, SaveRevision = 7,
            });
            Assert.IsTrue(day.Succeeded, day.Message);
            Assert.AreEqual(day.Provision.FoodRequired, day.Provision.FoodGranted);
            Assert.AreEqual(day.Provision.WaterRequired, day.Provision.WaterGranted);

            SeedInventory(inventory, "item_fiber_agave", 12, log, "seed_inventory_fiber");
            SeedInventory(inventory, "item_resin_pine", 8, log, "seed_inventory_resin");
            SeedInventory(inventory, "item_seed_cassava", 1, log, "seed_inventory_cassava");
            SeedInventory(inventory, D13FarmingCatalog.PestTreatmentItemId, 1, log, "seed_inventory_treatment");

            var building = NewBuilding(property, governance, inventory, log);
            Build(building, D13BuildingCatalog.WellId, "property_farmland_green_cay_cassava", "island_green_cay", 8, 10);
            Build(building, D13BuildingCatalog.WarehouseId, "property_warehouse_santa_marys_north_store", "island_saint_marys", 9, 11);
            Assert.AreEqual(D13BuildingLifecycle.Operational, building.FindState(D13BuildingCatalog.WellId).Lifecycle);
            Assert.AreEqual(D13BuildingLifecycle.Operational, building.FindState(D13BuildingCatalog.WarehouseId).Lifecycle);
            Assert.IsTrue(building.IsOpenAtHour(D13BuildingCatalog.WarehouseId, 12));

            var ecology = new D4EcologySystem();
            var farming = new D13FarmingSystem(inventory, ecology, new WellFarmingAuthority(property, building),
                new NoSaleFarmingEconomy(), log);
            var planted = farming.Plant(new D13FarmingPlantRequest
            {
                OperationId = "plant_cassava", PlotId = D13FarmingCatalog.CassavaPlotId,
                CropId = D13FarmingCatalog.CassavaCropId, SeedItemId = "item_seed_cassava", Seed = 31,
                WorldDay = 65, SaveRevision = 12, LocationRef = "green_cay",
            });
            Assert.IsTrue(planted.Succeeded, planted.Message);
            Assert.IsTrue(farming.Water(new D13FarmingWaterRequest
            {
                OperationId = "water_cassava", PlotId = D13FarmingCatalog.CassavaPlotId, WaterUnits = 20,
                WorldDay = 65, SaveRevision = 13, LocationRef = "green_cay",
            }).Succeeded);
            Assert.IsTrue(farming.AssignLabor(new D13FarmingLaborRequest
            {
                OperationId = "labor_cassava", PlotId = D13FarmingCatalog.CassavaPlotId, LaborUnits = 2,
                WorldDay = 65, SaveRevision = 14, LocationRef = "green_cay",
            }).Succeeded);
            var growth = farming.AdvanceDay(new D13FarmingAdvanceRequest
            {
                OperationId = "advance_cassava", PlotId = D13FarmingCatalog.CassavaPlotId, Days = 5,
                Seed = 37, Rainfall = true, WorldDay = 70, SaveRevision = 15, LocationRef = "green_cay",
            });
            Assert.IsTrue(growth.Succeeded, growth.Message);
            var harvest = farming.Harvest(new D13FarmingHarvestRequest
            {
                OperationId = "harvest_cassava", PlotId = D13FarmingCatalog.CassavaPlotId,
                Seed = 41, WorldDay = 71, SaveRevision = 16, LocationRef = "green_cay",
            });
            Assert.IsTrue(harvest.Succeeded, harvest.Message);
            Assert.Greater(harvest.Quantity, 0);
            Assert.Greater(ecology.FindPlot(D13FarmingCatalog.CassavaPlotId).EcologyPressure, 0);
            Assert.Greater(D3InventorySystem.CountCargo(inventory, "item_vegetable_cassava"), 0);
            var storm = ecology.Apply(new D13FarmingEnvironmentRequest
            {
                EventId = "event_d13_s01_storm", PlotId = D13FarmingCatalog.CassavaPlotId,
                Operation = D13FarmingEcologyOperation.Storm, CropId = D13FarmingCatalog.CassavaCropId,
                Amount = 1, Days = 1, StormIntensity = 80, WorldDay = 72, Seed = 43, SaveRevision = 16,
            }, log);
            Assert.IsTrue(storm.Succeeded, storm.Message);
            Assert.IsNotNull(storm.Fact);

            var marketState = new D3MarketState();
            var tradeEconomy = new TradeEconomySystem(log);
            var market = new D13DynamicMarketTransportSystem(new MarketLegalAuthority(log), null, null,
                new D13D3InventoryAuthority(inventory, log), new D13TradeEconomyAuthority(tradeEconomy, marketState, log),
                new TransportAssetAuthority(log), log);
            var sold = market.Sell(new D13MarketSaleRequest
            {
                OperationId = "sell_cassava", ItemId = "item_vegetable_cassava", Quantity = 1,
                SellerId = D13MarketTransportIds.PlayerRef, LocationId = PortCatalog.GreenCayId,
                InventoryHolderId = "player_hold", MarketId = PortCatalog.GreenCayId, Weather = D3WeatherType.Calm,
                WorldDay = 72, SaveRevision = 17, CauseEventIds = new List<string> { harvest.EventId },
            });
            Assert.IsTrue(sold.Succeeded, sold.Message);
            Assert.Greater(sold.Flow.TotalValue, 0);
            Assert.IsTrue(finance.Receive(new D13FinanceCashRequest
            {
                OperationId = "receive_harvest_sale", AccountId = "account_player", Amount = sold.Flow.TotalValue,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = sold.Flow.EconomySourceEventId,
                Description = "销售木薯收成入账", WorldDay = 72, SaveRevision = 18,
            }).Succeeded);

            var wage = governance.PayWage(new D13GovernanceWageRequest
            {
                OperationId = "pay_worker", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                ResidentId = "resident_worker", WorldDay = 72, SaveRevision = 19,
            });
            Assert.IsTrue(wage.Succeeded, wage.Message);
            Assert.IsTrue(finance.Spend(new D13FinanceCashRequest
            {
                OperationId = "spend_worker_wage", AccountId = "account_player", Amount = wage.Wage.Amount,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_wage",
                Description = "支付登记工资", WorldDay = 72, SaveRevision = 20,
            }).Succeeded);
            var tax = governance.AssessTax(new D13GovernanceTaxRequest
            {
                OperationId = "assess_island_tax", IslandId = D13IslandGovernanceCatalog.SaintMarysIslandId,
                HouseholdId = "family_worker", TaxBase = 100, WorldDay = 72, SaveRevision = 21,
            });
            Assert.IsTrue(tax.Succeeded, tax.Message);
            Assert.IsTrue(finance.Spend(new D13FinanceCashRequest
            {
                OperationId = "spend_island_tax", AccountId = "account_player", Amount = tax.Tax.AmountDue,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_tax",
                Description = "支付治理税费", WorldDay = 72, SaveRevision = 22,
            }).Succeeded);

            var loan = finance.RequestLoan(new D13FinanceLoanRequest
            {
                OperationId = "warehouse_asset_loan", AccountId = "account_player", DebtorRef = D13FinanceIds.PlayerRef,
                CreditorRef = "merchant_bank", Kind = D13FinanceDebtKind.PropertyMortgage, Principal = 200, TermDays = 30,
                LedgerRef = "ledger_property_loan", SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_loan",
                Collateral = new List<D13FinanceCollateralRequest> { new D13FinanceCollateralRequest { AssetRef = "property_warehouse_santa_marys_north_store", AssetKind = "warehouse" } },
                WorldDay = 72, SaveRevision = 23,
            });
            Assert.IsTrue(loan.Succeeded, loan.Message);
            Assert.AreEqual("property_warehouse_santa_marys_north_store", loan.Debt.Collateral[0].AssetRef);
            Assert.IsFalse(string.IsNullOrWhiteSpace(loan.Debt.WorstCaseOutcome));

            var quote = finance.QuoteInsurance(new D13FinanceInsuranceQuoteRequest
            {
                OperationId = "quote_storm_insurance", AccountId = "account_player", InsuredRef = D13FinanceIds.PlayerRef,
                Kind = D13FinanceInsuranceKind.Storm, AssetRef = "property_farmland_green_cay_cassava", CoverageAmount = 500,
                TermDays = 30, WorldDay = 72, SaveRevision = 24,
            });
            Assert.IsTrue(quote.Succeeded, quote.Message);
            Assert.Greater(quote.Quote.Premium, 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(quote.Quote.WorstCaseOutcome));
            var insurance = finance.BuyInsurance(new D13FinanceInsuranceRequest
            {
                OperationId = "buy_storm_insurance", AccountId = "account_player", InsuredRef = D13FinanceIds.PlayerRef,
                Kind = D13FinanceInsuranceKind.Storm, AssetRef = "property_farmland_green_cay_cassava", CoverageAmount = 500,
                TermDays = 30, SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_insurance",
                WorldDay = 72, SaveRevision = 25,
            });
            Assert.IsTrue(insurance.Succeeded, insurance.Message);
            AddSource(log, "storm_manifest", D13MarketTransportIds.EnvironmentOwner);
            Assert.IsTrue(finance.RegisterEvidence(new D13FinanceEvidenceRequest
            {
                OperationId = "register_storm_incident", AccountId = "account_player", EvidenceId = "evidence_storm_incident",
                Kind = D13FinanceEvidenceKind.Incident, FactEventId = storm.EventId, FactSaveRevision = storm.Fact.SaveRevision,
                Description = "风暴事故事实", WorldDay = 73, SaveRevision = 26,
            }).Succeeded);
            Assert.IsTrue(finance.RegisterEvidence(new D13FinanceEvidenceRequest
            {
                OperationId = "register_storm_manifest", AccountId = "account_player", EvidenceId = "evidence_storm_manifest",
                Kind = D13FinanceEvidenceKind.Manifest, FactEventId = "storm_manifest", FactSaveRevision = 0,
                Description = "风暴损失清单", WorldDay = 73, SaveRevision = 27,
            }).Succeeded);
            var claim = finance.SubmitClaim(new D13FinanceClaimRequest
            {
                OperationId = "claim_storm_loss", PolicyId = insurance.Policy.PolicyId, IncidentEventId = storm.EventId,
                IncidentTag = "storm", LossAmount = 300, EvidenceIds = new List<string> { "evidence_storm_incident", "evidence_storm_manifest" },
                WorldDay = 73, SaveRevision = 28,
            });
            Assert.IsTrue(claim.Succeeded, claim.Message);
            Assert.Greater(claim.Claim.ApprovedPayout, 0);
            Assert.AreEqual(D13PropertyIds.PlayerRef, property.FindState("property_warehouse_santa_marys_north_store").OwnerRef);

            var eventCount = log.Count;
            var propertySnapshot = property.CaptureSnapshot();
            var financeSnapshot = finance.CaptureSnapshot();
            var farmingSnapshot = farming.CaptureSnapshot();
            var governanceSnapshot = governance.CaptureSnapshot();
            var buildingSnapshot = building.CaptureSnapshot();
            var marketSnapshot = market.CaptureSnapshot();
            propertySnapshot.Validate(); financeSnapshot.Validate(); farmingSnapshot.Validate();
            governanceSnapshot.Validate(); buildingSnapshot.Validate(); marketSnapshot.Validate();
            SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
            {
                CurrentLocationId = PortCatalog.GreenCayId, CurrentDay = 73, SaveRevision = 28,
                Cargo = new Dictionary<string, int>(inventory.Cargo), MarketStock = new Dictionary<string, int>(marketState.Stock),
                MarketDemand = new Dictionary<string, int>(marketState.Demand), PlayerTradeVolume = new Dictionary<string, int>(marketState.PlayerTradeVolume),
                EventLog = log.CaptureSnapshot(),
            });
            SaveSystem.SaveD13Runtime(propertySnapshot);
            SaveSystem.SaveD13FinanceRuntime(financeSnapshot);
            SaveSystem.SaveD13FarmingRuntime(farmingSnapshot);
            SaveSystem.SaveD13GovernanceRuntime(governanceSnapshot);
            SaveSystem.SaveD13BuildingRuntime(buildingSnapshot);
            SaveSystem.SaveD13DynamicMarketRuntime(marketSnapshot);

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            var loadedD3 = SaveSystem.LoadD3Runtime();
            var loadedProperty = SaveSystem.LoadD13Runtime();
            var loadedFinance = SaveSystem.LoadD13FinanceRuntime();
            var loadedFarming = SaveSystem.LoadD13FarmingRuntime();
            var loadedGovernance = SaveSystem.LoadD13GovernanceRuntime();
            var loadedBuilding = SaveSystem.LoadD13BuildingRuntime();
            var loadedMarket = SaveSystem.LoadD13DynamicMarketRuntime();
            Assert.IsNotNull(loadedD3); Assert.IsNotNull(loadedProperty); Assert.IsNotNull(loadedFinance);
            Assert.IsNotNull(loadedFarming); Assert.IsNotNull(loadedGovernance); Assert.IsNotNull(loadedBuilding); Assert.IsNotNull(loadedMarket);
            Assert.AreEqual(eventCount, loadedD3.EventLog.Facts.Count);
            Assert.AreEqual(propertySnapshot.Properties.Count, loadedProperty.Properties.Count);
            Assert.AreEqual(financeSnapshot.Debts.Count, loadedFinance.Debts.Count);
            Assert.AreEqual(farmingSnapshot.Ecology.Environments.Count, loadedFarming.Ecology.Environments.Count);
            Assert.AreEqual(governanceSnapshot.Taxes.Count, loadedGovernance.Taxes.Count);
            Assert.AreEqual(buildingSnapshot.Buildings.Count, loadedBuilding.Buildings.Count);
            Assert.AreEqual(marketSnapshot.Flows.Count, loadedMarket.Flows.Count);
            Assert.AreEqual(inventory.Cargo["item_vegetable_cassava"], loadedD3.Cargo["item_vegetable_cassava"]);

            var restoredLog = EventLog.FromSnapshot(loadedD3.EventLog);
            var restoredInventory = new D3InventoryState(500);
            foreach (var item in loadedD3.Cargo) restoredInventory.Cargo[item.Key] = item.Value;
            var restoredProperty = new D13PropertyAssetSystem(restoredLog, null, new LegalAuthority(), new PropertyEconomyAuthority(), loadedProperty);
            var restoredFinance = new D13FinanceSystem(restoredLog, new CollateralAuthority(restoredProperty), loadedFinance);
            var restoredGovernance = NewGovernance(restoredLog, loadedGovernance);
            var restoredBuilding = NewBuilding(restoredProperty, restoredGovernance, restoredInventory, restoredLog, loadedBuilding);
            var restoredEcology = new D4EcologySystem();
            var restoredFarming = new D13FarmingSystem(restoredInventory, restoredEcology, new WellFarmingAuthority(restoredProperty, restoredBuilding), new NoSaleFarmingEconomy(), restoredLog, loadedFarming);
            var restoredMarketState = new D3MarketState();
            foreach (var item in loadedD3.MarketStock) restoredMarketState.Stock[item.Key] = item.Value;
            foreach (var item in loadedD3.MarketDemand) restoredMarketState.Demand[item.Key] = item.Value;
            foreach (var item in loadedD3.PlayerTradeVolume) restoredMarketState.PlayerTradeVolume[item.Key] = item.Value;
            var restoredTradeEconomy = new TradeEconomySystem(restoredLog);
            var restoredMarket = new D13DynamicMarketTransportSystem(new MarketLegalAuthority(restoredLog), null, null,
                new D13D3InventoryAuthority(restoredInventory, restoredLog), new D13TradeEconomyAuthority(restoredTradeEconomy, restoredMarketState, restoredLog),
                new TransportAssetAuthority(restoredLog), restoredLog, loadedMarket);
            Assert.AreEqual(D13BuildingLifecycle.Operational, restoredBuilding.FindState(D13BuildingCatalog.WellId).Lifecycle);
            Assert.AreEqual(D13BuildingLifecycle.Operational, restoredBuilding.FindState(D13BuildingCatalog.WarehouseId).Lifecycle);
            Assert.AreEqual(D13PropertyIds.PlayerRef, restoredProperty.FindState("property_farmland_green_cay_cassava").LeaseHolderRef);
            Assert.AreEqual(D13PropertyIds.PlayerRef, restoredProperty.FindState("property_warehouse_santa_marys_north_store").OwnerRef);
            Assert.AreEqual(claim.Claim.ApprovedPayout, restoredFinance.FindClaim(claim.Claim.ClaimId).ApprovedPayout);
            Assert.Greater(restoredFarming.GetReadModel().Environments.Count, 0);
            Assert.AreEqual(eventCount, restoredLog.Count);

            var duplicateSale = restoredMarket.Sell(new D13MarketSaleRequest
            {
                OperationId = "sell_cassava", ItemId = "item_vegetable_cassava", Quantity = 1,
                SellerId = D13MarketTransportIds.PlayerRef, LocationId = PortCatalog.GreenCayId,
                InventoryHolderId = "player_hold", MarketId = PortCatalog.GreenCayId, WorldDay = 72, SaveRevision = 17,
            });
            Assert.IsTrue(duplicateSale.Duplicate);
            var duplicateLoan = restoredFinance.RequestLoan(new D13FinanceLoanRequest
            {
                OperationId = "warehouse_asset_loan", AccountId = "account_player", DebtorRef = D13FinanceIds.PlayerRef,
                CreditorRef = "merchant_bank", Kind = D13FinanceDebtKind.PropertyMortgage, Principal = 200, TermDays = 30,
                LedgerRef = "ledger_property_loan", SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_loan",
                Collateral = new List<D13FinanceCollateralRequest> { new D13FinanceCollateralRequest { AssetRef = "property_warehouse_santa_marys_north_store", AssetKind = "warehouse" } },
                WorldDay = 72, SaveRevision = 23,
            });
            Assert.IsTrue(duplicateLoan.Duplicate);
            var duplicateOrder = restoredBuilding.PlaceConstructionOrder(new D13ConstructionOrderRequest
            {
                OperationId = "build_well", BuildingId = D13BuildingCatalog.WellId, IslandId = "island_green_cay",
                PropertyId = "property_farmland_green_cay_cassava", Workers = 1, WorldDay = 8, SaveRevision = 8,
            });
            Assert.IsTrue(duplicateOrder.Duplicate);
            Assert.AreEqual(eventCount, restoredLog.Count, "读档后重放相同 operation_id/save_revision 不能追加事实或重复结算。");
        }

        private D13PropertyResult LeaseFarmland(D13PropertyAssetSystem property)
        {
            Assert.IsTrue(property.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = "property_farmland_green_cay_cassava", OperationId = "survey_farmland", WorldDay = 1, SaveRevision = 2,
            }).Succeeded);
            var lease = property.Lease(new D13PropertyTransactionRequest
            {
                PropertyId = "property_farmland_green_cay_cassava", ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "lease_farmland", LeaseEndDay = 200, WorldDay = 1, SaveRevision = 3,
            });
            Assert.IsTrue(lease.Succeeded, lease.Message);
            return lease;
        }

        private D13PropertyResult BuyWarehouse(D13PropertyAssetSystem property, D13FinanceSystem finance, string cause)
        {
            Assert.IsTrue(property.Investigate(new D13PropertySurveyRequest
            {
                PropertyId = "property_warehouse_santa_marys_north_store", OperationId = "survey_warehouse", WorldDay = 1, SaveRevision = 30,
            }).Succeeded);
            var valuation = property.RequestValuation(new D13PropertyValuationRequest
            {
                PropertyId = "property_warehouse_santa_marys_north_store", OperationId = "value_warehouse", WorldDay = 1, SaveRevision = 31,
            });
            Assert.IsTrue(valuation.Succeeded, valuation.Message);
            var purchase = property.Purchase(new D13PropertyTransactionRequest
            {
                PropertyId = "property_warehouse_santa_marys_north_store", ApplicantRef = D13PropertyIds.PlayerRef,
                OperationId = "buy_warehouse", OfferedAmount = valuation.Amount, WorldDay = 1, SaveRevision = 32,
                CauseEventIds = new List<string> { cause },
            });
            Assert.IsTrue(purchase.Succeeded, purchase.Message);
            var paid = finance.Spend(new D13FinanceCashRequest
            {
                OperationId = "pay_warehouse_purchase", AccountId = "account_player", Amount = purchase.Amount,
                SourceOwnerSystem = D13FinanceIds.EconomyOwner, SourceEventId = "economy_source", Description = "支付仓库产权交易",
                WorldDay = 1, SaveRevision = 33,
            });
            Assert.IsTrue(paid.Succeeded, paid.Message);
            return purchase;
        }

        private static void Build(D13BuildingSystem system, string buildingId, string propertyId, string islandId, long orderRevision, int day)
        {
            var definition = system.FindDefinition(buildingId);
            var placed = system.PlaceConstructionOrder(new D13ConstructionOrderRequest
            {
                OperationId = "build_" + (buildingId == D13BuildingCatalog.WellId ? "well" : "warehouse"),
                BuildingId = buildingId, IslandId = islandId, PropertyId = propertyId, Workers = definition.WorkerSlots,
                WorldDay = day, SaveRevision = orderRevision,
            });
            Assert.IsTrue(placed.Succeeded, placed.Message);
            for (var i = 0; i < definition.BuildDaysRequired; i++)
            {
                var progress = system.AdvanceConstruction(new D13ConstructionProgressRequest
                {
                    OperationId = "advance_" + buildingId + "_" + i, BuildingId = buildingId, IslandId = islandId,
                    PropertyId = propertyId, Workers = definition.WorkerSlots, WorldDay = day + i + 1,
                    SaveRevision = orderRevision + i + 1,
                });
                Assert.IsTrue(progress.Succeeded, progress.Message);
            }
        }

        private static void SeedInventory(D3InventoryState inventory, string itemId, int quantity, EventLog log, string eventId)
        {
            string error;
            Assert.IsTrue(D3InventorySystem.TryAddCargo(inventory, itemId, quantity, out error), error);
            AddSource(log, eventId, D13BuildingIds.InventoryOwner);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "economy_account", D13FinanceIds.EconomyOwner);
            AddSource(log, "economy_source", D13PropertyIds.EconomyOwner);
            AddSource(log, "economy_loan", D13FinanceIds.EconomyOwner);
            AddSource(log, "economy_insurance", D13FinanceIds.EconomyOwner);
            AddSource(log, "economy_building", D13BuildingIds.EconomyOwner);
            AddSource(log, "economy_wage", D13GovernanceIds.EconomyOwner);
            AddSource(log, "economy_tax", D13GovernanceIds.EconomyOwner);
            AddSource(log, "legal_source", D13PropertyIds.LegalOwner);
            AddSource(log, "property_rights", D13GovernanceIds.PropertyOwner);
            AddSource(log, "legal_rights", D13GovernanceIds.LegalOwner);
            AddSource(log, "policy_approved", D13GovernanceIds.LegalOwner);
            AddSource(log, "food_supply", D13GovernanceIds.EcologyOwner);
            AddSource(log, "water_supply", D13GovernanceIds.EcologyOwner);
            AddSource(log, "migration_worker_source", D13GovernanceIds.EconomyOwner);
            AddSource(log, "workforce_source", D13BuildingIds.WorkforceOwner);
            AddSource(log, "occupation_source", D13BuildingIds.OccupationOwner);
            AddSource(log, "market_legal", D13MarketTransportIds.LegalOwner);
            AddSource(log, "transport_asset", D13MarketTransportIds.ShipOwner);
            return log;
        }

        private static void AddSource(EventLog log, string eventId, string owner)
        {
            log.Append(new WorldEvent(eventId, 0, "d13_s01_fixture", owner,
                new EventResult { ResultCode = "fixture_source", Summary = "D13-S01 authority fixture source", FactIds = new List<string> { "fixture:" + eventId } },
                new[] { "player" }, new[] { "fixture" }, new string[0], 0));
        }

        private static D13BuildingSystem NewBuilding(D13PropertyAssetSystem property, D13IslandGovernanceSystem governance,
            D3InventoryState inventory, EventLog log, D13BuildingRuntimeSnapshot initial = null)
        {
            return new D13BuildingSystem(new BuildingPropertyAuthority(property), new BuildingWorkforceAuthority(log),
                new BuildingInventoryAuthority(inventory, log), new BuildingEconomyAuthority(log),
                new BuildingOccupationAuthority(log), new BuildingShipAuthority(log), log, initial);
        }

        private static D13IslandGovernanceSystem NewGovernance(EventLog log, D13GovernanceRuntimeSnapshot initial = null)
        {
            return new D13IslandGovernanceSystem(new GovernanceResidenceAuthority(), new GovernanceProvisionAuthority(),
                new GovernanceEconomyAuthority(), new GovernancePolicyAuthority(), new GovernanceCommunityAuthority(),
                new GovernanceAgentAuthority(), log, initial);
        }

        private sealed class LegalAuthority : ID13PropertyLegalAuthority
        {
            public D13LegalDecision Investigate(D13PropertyDefinition definition, D13PropertyState state, D13PropertySurveyRequest request)
            {
                return new D13LegalDecision { Allowed = true, Code = "registered", LegalStatus = definition.Kind == D13PropertyKind.Farmland ? D13PropertyLegalStatus.Leaseable : D13PropertyLegalStatus.Registered, TitleHolderRef = state.OwnerRef, SourceOwnerSystem = D13PropertyIds.LegalOwner, SourceEventId = "legal_source" };
            }

            public D13LegalDecision Authorize(D13PropertyDefinition definition, D13PropertyState state, D13PropertyTransactionRequest request, D13PropertySurveyRecord survey)
            {
                return new D13LegalDecision { Allowed = true, Code = "transaction_approved", LegalStatus = definition.Kind == D13PropertyKind.Farmland ? D13PropertyLegalStatus.Leaseable : D13PropertyLegalStatus.Registered, TitleHolderRef = state.OwnerRef, ResolvedOwnerRef = request.ApplicantRef, SourceOwnerSystem = D13PropertyIds.LegalOwner, SourceEventId = "legal_source" };
            }
        }

        private sealed class PropertyEconomyAuthority : ID13PropertyEconomyAuthority
        {
            public D13EconomyValuationDecision Quote(D13PropertyDefinition definition, D13PropertyState state, D13PropertyValuationRequest request)
            {
                return new D13EconomyValuationDecision { Approved = true, Code = "valuation_approved", Quote = new D13ValuationQuote { PropertyId = definition.Id, Amount = 250, Currency = "coin", ValuationBasis = "fixture_market_quote", SourceOwnerSystem = D13PropertyIds.EconomyOwner, SourceEventId = "economy_source" } };
            }

            public D13EconomyTransactionDecision Approve(D13PropertyDefinition definition, D13PropertyState state, D13PropertyTransactionRequest request, D13ValuationQuote valuation)
            {
                return new D13EconomyTransactionDecision { Approved = true, Code = "transaction_price_approved", Amount = request.Kind == D13PropertyTransactionKind.Lease ? 25 : valuation == null ? 250 : valuation.Amount, LedgerRef = "ledger_property_fixture", SourceOwnerSystem = D13PropertyIds.EconomyOwner, SourceEventId = "economy_source" };
            }
        }

        private sealed class CollateralAuthority : ID13FinanceCollateralAuthority
        {
            private readonly D13PropertyAssetSystem _property;
            public CollateralAuthority(D13PropertyAssetSystem property) { _property = property; }
            public D13FinanceCollateralDecision ValidateCollateral(D13FinanceCollateralRequest request, string debtorRef, int worldDay)
            {
                var state = _property.FindState(request.AssetRef);
                return state == null ? new D13FinanceCollateralDecision { Allowed = false, Code = "asset_missing" } : new D13FinanceCollateralDecision
                {
                    Allowed = true, Code = "collateral_approved", ClaimValue = state.LatestValuationAmount,
                    ValuationSourceOwnerSystem = D13FinanceIds.EconomyOwner, ValuationSourceEventId = "economy_source",
                    LegalSourceOwnerSystem = D13FinanceIds.LegalOwner, LegalSourceEventId = state.LegalSourceEventId ?? "legal_source",
                };
            }
            public D13FinanceSeizureDecision AuthorizeSeizure(D13FinanceDebtState debt, D13FinanceCollateralState collateral, D13FinanceCollectionRequest request)
            {
                return new D13FinanceSeizureDecision { Allowed = true, Code = "seizure_approved", LegalSourceOwnerSystem = D13FinanceIds.LegalOwner, LegalSourceEventId = "legal_source" };
            }
        }

        private sealed class BuildingPropertyAuthority : ID13BuildingPropertyAuthority
        {
            private readonly D13PropertyAssetSystem _property;
            public BuildingPropertyAuthority(D13PropertyAssetSystem property) { _property = property; }
            public D13BuildingSiteDecision ValidateSite(D13BuildingDefinition definition, D13BuildingOperationRequest request)
            {
                var state = _property.FindState(definition.PropertyId);
                var allowed = state != null && (state.OwnerRef == D13PropertyIds.PlayerRef || state.LeaseHolderRef == D13PropertyIds.PlayerRef);
                return allowed ? new D13BuildingSiteDecision { Allowed = true, Code = "site_approved", PropertySourceEventId = state.LastEventId, LegalSourceEventId = state.LegalSourceEventId, SourceOwnerSystem = D13BuildingIds.PropertyOwner } : new D13BuildingSiteDecision { Allowed = false, Code = "property_right_required" };
            }
        }

        private sealed class BuildingWorkforceAuthority : ID13BuildingWorkforceAuthority
        {
            private readonly EventLog _log;
            public BuildingWorkforceAuthority(EventLog log) { _log = log; }
            public D13BuildingWorkerDecision ValidateWorkers(D13BuildingDefinition definition, int workers, string actorRef, int worldDay)
            {
                return new D13BuildingWorkerDecision { Allowed = workers > 0 && workers <= definition.WorkerSlots, Code = "workforce_approved", Workers = workers, SourceEventId = "workforce_source", SourceOwnerSystem = D13BuildingIds.WorkforceOwner };
            }
        }

        private sealed class BuildingInventoryAuthority : ID13BuildingInventoryAuthority
        {
            private readonly D3InventoryState _inventory; private readonly EventLog _log;
            public BuildingInventoryAuthority(D3InventoryState inventory, EventLog log) { _inventory = inventory; _log = log; }
            public D13BuildingInventoryDecision ConsumeConstructionMaterials(D13BuildingDefinition definition, D13BuildingOperationRequest request)
            {
                foreach (var material in definition.ConstructionMaterials)
                {
                    string error;
                    if (!D3InventorySystem.TryRemoveCargo(_inventory, material.ItemId, material.Quantity, out error)) return new D13BuildingInventoryDecision { Allowed = false, Code = error };
                }
                var id = "inventory_build_" + request.BuildingId + "_" + request.SaveRevision;
                AddSource(_log, id, D13BuildingIds.InventoryOwner);
                return new D13BuildingInventoryDecision { Allowed = true, Code = "materials_consumed", SourceEventId = id, SourceOwnerSystem = D13BuildingIds.InventoryOwner };
            }
            public D13BuildingInventoryDecision ApplyService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request)
            {
                foreach (var material in service.Inputs)
                {
                    string error;
                    if (!D3InventorySystem.TryRemoveCargo(_inventory, material.ItemId, material.Quantity, out error)) return new D13BuildingInventoryDecision { Allowed = false, Code = error };
                }
                foreach (var output in service.Outputs)
                {
                    string error;
                    if (!D3InventorySystem.TryAddCargo(_inventory, output.ItemId, output.Quantity, out error)) return new D13BuildingInventoryDecision { Allowed = false, Code = error };
                }
                var id = "inventory_service_" + request.OperationId;
                AddSource(_log, id, D13BuildingIds.InventoryOwner);
                return new D13BuildingInventoryDecision { Allowed = true, Code = "service_inventory_applied", SourceEventId = id, SourceOwnerSystem = D13BuildingIds.InventoryOwner };
            }
            public D13BuildingInventoryDecision StoreWarehouseItem(D13BuildingDefinition definition, D13WarehouseStoreRequest request)
            {
                string error;
                if (!D3InventorySystem.TryRemoveCargo(_inventory, request.ItemId, request.Quantity, out error)) return new D13BuildingInventoryDecision { Allowed = false, Code = error };
                var id = "inventory_store_" + request.OperationId;
                AddSource(_log, id, D13BuildingIds.InventoryOwner);
                return new D13BuildingInventoryDecision { Allowed = true, Code = "warehouse_stored", WarehouseUsed = request.Quantity, WarehouseCapacity = definition.StorageCapacity, SourceEventId = id, SourceOwnerSystem = D13BuildingIds.InventoryOwner };
            }
        }

        private sealed class BuildingEconomyAuthority : ID13BuildingEconomyAuthority
        {
            private readonly EventLog _log;
            public BuildingEconomyAuthority(EventLog log) { _log = log; }
            public D13BuildingEconomyDecision QuoteService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingOperationRequest request)
            {
                return new D13BuildingEconomyDecision { Allowed = true, Code = "service_price_approved", ServiceId = service.ServiceId, Amount = 8, Currency = D13BuildingIds.Currency, LedgerRef = "ledger_building_fixture", SourceEventId = "economy_building", SourceOwnerSystem = D13BuildingIds.EconomyOwner };
            }
        }

        private sealed class BuildingOccupationAuthority : ID13BuildingOccupationAuthority
        {
            private readonly EventLog _log;
            public BuildingOccupationAuthority(EventLog log) { _log = log; }
            public D13BuildingOccupationDecision AuthorizeOccupation(D13BuildingDefinition definition, D13BuildingOccupationRequest request)
            {
                return new D13BuildingOccupationDecision { Allowed = true, Code = "occupation_free", State = D13BuildingOccupationState.Free, SourceEventId = "occupation_source", SourceOwnerSystem = D13BuildingIds.OccupationOwner };
            }
        }

        private sealed class BuildingShipAuthority : ID13BuildingShipAuthority
        {
            private readonly EventLog _log;
            public BuildingShipAuthority(EventLog log) { _log = log; }
            public D13BuildingShipServiceDecision ApproveShipService(D13BuildingDefinition definition, D13BuildingServiceDefinition service, D13BuildingShipServiceRequest request)
            {
                return new D13BuildingShipServiceDecision { Allowed = true, Code = "ship_service_approved", ShipId = request.ShipId, AppliedHullPoints = 10, SourceEventId = "transport_asset", SourceOwnerSystem = D13BuildingIds.ShipOwner };
            }
        }

        private sealed class WellFarmingAuthority : ID13FarmingLandAuthority
        {
            private readonly D13FarmingPropertyAuthority _property;
            private readonly D13BuildingSystem _building;
            public WellFarmingAuthority(D13PropertyAssetSystem property, D13BuildingSystem building) { _property = new D13FarmingPropertyAuthority(property); _building = building; }
            public D13FarmingLandDecision ValidatePlot(D13FarmingPlotDefinition definition, string actorRef, int worldDay)
            {
                var well = _building.FindState(D13BuildingCatalog.WellId);
                if (well == null || well.Lifecycle != D13BuildingLifecycle.Operational) return new D13FarmingLandDecision { Allowed = false, Code = "water_well_required", Message = "农业地块必须先有已运营的水井建筑。" };
                return _property.ValidatePlot(definition, actorRef, worldDay);
            }
        }

        private sealed class NoSaleFarmingEconomy : ID13FarmingEconomyAuthority
        {
            public D13FarmingSaleDecision QuoteAndApproveSale(string itemId, int quantity, string marketRef, string sellerRef, int worldDay, long saveRevision)
            {
                return new D13FarmingSaleDecision { Allowed = false, Code = "d13_s01_market_owner", Message = "集成销售统一交给 D13DynamicMarketTransportSystem。" };
            }
        }

        private sealed class MarketLegalAuthority : ID13MarketLegalAuthority
        {
            private readonly EventLog _log;
            public MarketLegalAuthority(EventLog log) { _log = log; }
            public D13MarketLegalDecision ValidateGood(D13MarketGoodDefinition definition, string locationId, string permitId, D13MarketOperationRequest request)
            {
                return new D13MarketLegalDecision { Allowed = true, Code = "market_legal", LegalStatus = D13MarketLegalStatus.Legal, SourceEventId = "market_legal", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
            }
            public D13MarketLegalDecision ValidateRoute(D13MarketRouteDefinition route, string permitId, D13MarketShipmentRequest request)
            {
                return new D13MarketLegalDecision { Allowed = true, Code = "route_legal", LegalStatus = D13MarketLegalStatus.Legal, SourceEventId = "market_legal", SourceOwnerSystem = D13MarketTransportIds.LegalOwner };
            }
        }

        private sealed class TransportAssetAuthority : ID13TransportAssetAuthority
        {
            private readonly EventLog _log;
            public TransportAssetAuthority(EventLog log) { _log = log; }
            public D13TransportAssetDecision ValidateTransportAsset(D13MarketRouteDefinition route, D13MarketShipmentAdvanceRequest request)
            {
                return new D13TransportAssetDecision { Allowed = true, Code = "transport_ready", ConditionPercent = 100, SourceEventId = "transport_asset", SourceOwnerSystem = D13MarketTransportIds.ShipOwner };
            }
        }

        private sealed class GovernanceResidenceAuthority : ID13GovernanceResidenceAuthority
        {
            public D13GovernanceResidenceDecision ValidateResidence(D13IslandGovernanceDefinition definition, D13MigrationInRequest request, int worldDay)
            {
                return new D13GovernanceResidenceDecision { Allowed = true, Code = "residence_approved", PropertySourceEventId = "property_rights", LegalSourceEventId = "legal_rights", SourceOwnerSystem = D13GovernanceIds.PropertyOwner };
            }
        }

        private sealed class GovernanceProvisionAuthority : ID13GovernanceProvisionAuthority
        {
            public D13GovernanceProvisionDecision Provide(D13GovernanceProvisionRequest request)
            {
                return new D13GovernanceProvisionDecision { Allowed = true, Code = "provision_approved", FoodUnits = request.FoodRequired, WaterUnits = request.WaterRequired, FoodSourceEventId = "food_supply", WaterSourceEventId = "water_supply", FoodSourceOwnerSystem = D13GovernanceIds.EcologyOwner, WaterSourceOwnerSystem = D13GovernanceIds.EcologyOwner };
            }
        }

        private sealed class GovernanceEconomyAuthority : ID13GovernanceEconomyAuthority
        {
            public D13GovernanceWageDecision ApproveWage(D13ResidentState resident, D13GovernanceJobDefinition job, int worldDay, long saveRevision)
            {
                return new D13GovernanceWageDecision { Allowed = true, Code = "wage_approved", Amount = job.WagePerDay, Currency = D13GovernanceIds.Currency, LedgerRef = "ledger_governance_fixture", SourceEventId = "economy_wage", SourceOwnerSystem = D13GovernanceIds.EconomyOwner };
            }
            public D13GovernanceTaxDecision AssessTax(string householdId, int taxBase, int taxRateBps, int worldDay, long saveRevision)
            {
                var due = taxBase * taxRateBps / 10000;
                return new D13GovernanceTaxDecision { Allowed = true, Code = "tax_approved", AmountDue = due, AmountCollected = due, LedgerRef = "ledger_tax_fixture", SourceEventId = "economy_tax", SourceOwnerSystem = D13GovernanceIds.EconomyOwner };
            }
        }

        private sealed class GovernancePolicyAuthority : ID13GovernancePolicyAuthority
        {
            public D13GovernancePolicyDecision ApprovePolicy(D13IslandGovernanceDefinition definition, D13GovernancePolicyRequest request, D13GovernancePolicyState currentState)
            {
                return new D13GovernancePolicyDecision { Allowed = true, Code = "policy_approved", TaxRateBps = request.TaxRateBps, WaterRationingLevel = request.WaterRationingLevel, FoodRationingLevel = request.FoodRationingLevel, EducationOpen = request.EducationOpen, ReligionArrangementId = request.ReligionArrangementId, SecurityPostureId = request.SecurityPostureId, SourceEventId = "policy_approved", SourceOwnerSystem = D13GovernanceIds.LegalOwner };
            }
        }

        private sealed class GovernanceCommunityAuthority : ID13GovernanceCommunityAuthority
        {
            public D13GovernanceAgreementDecision ValidateAgreement(D13IslandGovernanceDefinition definition, D13GovernanceAgreementRequest request, int worldDay)
            {
                return new D13GovernanceAgreementDecision { Allowed = true, Code = "agreement_approved", AgreementId = request.AgreementId, CommunityId = request.CommunityId, Status = "active", SourceEventId = "policy_approved", SourceOwnerSystem = D13GovernanceIds.CommunityOwner, Content = new D13GovernanceContentTag { ContentId = "d13_s01_fixture_content", HistoricalTag = "inferred", SensitiveContentLabel = "governance_fixture", ContentWarning = "测试内容", ReviewStatus = D13GovernanceContentReviewStatus.ApprovedWithWarning, SourceNote = "D13-S01 fixture", Tags = new List<string> { "reviewed" } } };
            }
        }

        private sealed class GovernanceAgentAuthority : ID13GovernanceAgentAuthority
        {
            public D13GovernanceAgentDecision ApproveManagement(D13IslandGovernanceDefinition definition, D13GovernanceAgentRequest request, int worldDay)
            {
                return new D13GovernanceAgentDecision { Allowed = true, Code = "agent_approved", AgentRef = request.AgentRef, ManagementRole = request.ManagementRole, SourceEventId = "policy_approved", SourceOwnerSystem = D13GovernanceIds.AgentOwner };
            }
        }
    }
}
