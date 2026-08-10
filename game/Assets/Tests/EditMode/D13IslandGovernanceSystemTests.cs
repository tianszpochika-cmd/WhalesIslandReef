// D13-A05 岛屿治理测试：人口/家庭、住房/岗位、供水食物、教育宗教、税工资、政策社群、治安和存档幂等。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D13;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D13IslandGovernanceSystemTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d13_governance_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void PopulationGovernanceAndSaveRestoreKeepAuthoritySourcesAndIdempotency()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "治理测试船长", ShipTypeId = "small_fishing" });
            var log = CreateSources();
            var governance = NewGovernance(log, true, true, true, true, true, true);
            var island = D13GovernanceIdsIsland;

            var alice = governance.MigrateIn(new D13MigrationInRequest
            {
                OperationId = "migration_alice", IslandId = island, ResidentId = "resident_alice", HouseholdId = "family_alice",
                FamilyName = "Alvarez", ReligionId = "religion_catholic_parish", ReligionContentId = "religion_catholic_parish_saint_marys",
                SourceEventId = "migration_alice_source", ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(alice.Succeeded, alice.Message);
            var child = governance.MigrateIn(new D13MigrationInRequest
            {
                OperationId = "migration_beto", IslandId = island, ResidentId = "resident_beto", HouseholdId = "family_alice",
                FamilyName = "Alvarez", Role = D13ResidentRole.Child, SourceEventId = "migration_beto_source", ReviewAcknowledged = true,
                WorldDay = 1, SaveRevision = 2,
            });
            Assert.IsTrue(child.Succeeded, child.Message);
            Assert.AreEqual(2, governance.FindHousehold("family_alice").MemberIds.Count);

            var job = governance.AssignJob(new D13JobAssignmentRequest
            {
                OperationId = "job_alice", IslandId = island, ResidentId = "resident_alice", JobId = D13IslandGovernanceCatalog.JobDockhand,
                WorldDay = 2, SaveRevision = 3,
            });
            Assert.IsTrue(job.Succeeded, job.Message);
            Assert.AreEqual(D13EmploymentStatus.Employed, governance.FindResident("resident_alice").EmploymentStatus);

            var education = governance.EnrollEducation(new D13EducationRequest
            {
                OperationId = "education_beto", IslandId = island, ResidentId = "resident_beto", WorldDay = 2, SaveRevision = 4,
            });
            Assert.IsTrue(education.Succeeded, education.Message);
            var religion = governance.PracticeReligion(new D13ReligionPracticeRequest
            {
                OperationId = "religion_alice", IslandId = island, ResidentId = "resident_alice", ReligionId = "religion_catholic_parish",
                ReligionContentId = "religion_catholic_parish_saint_marys", ReviewAcknowledged = true, WorldDay = 2, SaveRevision = 5,
            });
            Assert.IsTrue(religion.Succeeded, religion.Message);

            var agreement = governance.ApplyCommunityAgreement(new D13GovernanceAgreementRequest
            {
                OperationId = "agreement_green_cay", IslandId = island, AgreementId = "agreement_green_cay", CommunityId = "community_green_cay",
                ReviewAcknowledged = true, WorldDay = 2, SaveRevision = 6,
            });
            Assert.IsTrue(agreement.Succeeded, agreement.Message);
            Assert.AreEqual(D13GovernanceContentReviewStatus.ApprovedWithWarning, agreement.Agreement.Content.ReviewStatus);

            var policy = governance.ApplyPolicy(new D13GovernancePolicyRequest
            {
                OperationId = "policy_water_watch", IslandId = island, TaxRateBps = 900, WaterRationingLevel = 0, FoodRationingLevel = 0,
                EducationOpen = true, ReligionArrangementId = "religion_catholic_parish", SecurityPostureId = "watch_strict", WorldDay = 2, SaveRevision = 7,
            });
            Assert.IsTrue(policy.Succeeded, policy.Message);
            Assert.AreEqual(900, governance.GetReadModel().Policy.TaxRateBps);

            var day = governance.AdvanceDay(new D13GovernanceDayRequest
            {
                OperationId = "day_full_supply", IslandId = island, Seed = 11, WorldDay = 3, SaveRevision = 8,
            });
            Assert.IsTrue(day.Succeeded, day.Message);
            Assert.AreEqual(day.Provision.FoodRequired, day.Provision.FoodGranted);
            Assert.AreEqual(100, governance.FindResident("resident_alice").Health);

            var wage = governance.PayWage(new D13GovernanceWageRequest
            {
                OperationId = "wage_alice", IslandId = island, ResidentId = "resident_alice", WorldDay = 3, SaveRevision = 9,
            });
            Assert.IsTrue(wage.Succeeded, wage.Message);
            Assert.AreEqual(0, governance.FindResident("resident_alice").WageDue);
            var tax = governance.AssessTax(new D13GovernanceTaxRequest
            {
                OperationId = "tax_family_alice", IslandId = island, HouseholdId = "family_alice", TaxBase = 100,
                WorldDay = 3, SaveRevision = 10,
            });
            Assert.IsTrue(tax.Succeeded, tax.Message);
            Assert.AreEqual(9, tax.Tax.AmountDue);

            var agent = governance.ManageAgent(new D13GovernanceAgentRequest
            {
                OperationId = "agent_family_manager", IslandId = island, ResidentId = "resident_alice", AgentRef = "agent_island_manager",
                ManagementRole = "water_and_food_coordinator", WorldDay = 3, SaveRevision = 11,
            });
            Assert.IsTrue(agent.Succeeded, agent.Message);

            var snapshot = governance.CaptureSnapshot();
            snapshot.Validate();
            SaveSystem.SaveD13GovernanceRuntime(snapshot);
            Assert.IsTrue(SaveSystem.Load(), "治理快照必须随现有 SaveSystem 恢复。");
            var loaded = SaveSystem.LoadD13GovernanceRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(snapshot.Residents.Count, loaded.Residents.Count);
            Assert.AreEqual(snapshot.Households.Count, loaded.Households.Count);
            Assert.AreEqual(snapshot.Taxes[0].AmountDue, loaded.Taxes[0].AmountDue);
            Assert.AreEqual(snapshot.AgentAssignments[0].AgentRef, loaded.AgentAssignments[0].AgentRef);

            var restored = NewGovernance(new EventLog(), true, true, true, true, true, true, loaded);
            var duplicate = restored.MigrateIn(new D13MigrationInRequest
            {
                OperationId = "migration_alice", IslandId = island, ResidentId = "resident_alice", HouseholdId = "family_alice",
                ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(duplicate.Succeeded && duplicate.Duplicate);
            Assert.AreEqual(snapshot.Residents.Count, restored.GetReadModel().Residents.Count);
            Assert.AreEqual(snapshot.Revision, restored.Revision);
        }

        [Test]
        public void ShortageAndDeterministicSecurityIncidentChangeHealthAndCanRecover()
        {
            var log = CreateSources();
            var governance = NewGovernance(log, true, false, true, true, true, true);
            var resident = governance.MigrateIn(new D13MigrationInRequest
            {
                OperationId = "migration_shortage", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_shortage", HouseholdId = "family_shortage",
                ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 1,
            });
            Assert.IsTrue(resident.Succeeded, resident.Message);
            var before = governance.FindResident("resident_shortage");
            D13GovernanceResult day = null;
            for (int i = 0; i < 40 && governance.GetReadModel().Incidents.Count == 0; i++)
            {
                day = governance.AdvanceDay(new D13GovernanceDayRequest
                {
                    OperationId = "shortage_day_" + i, IslandId = D13GovernanceIdsIsland, Seed = i, WorldDay = 2 + i, SaveRevision = 2 + i,
                });
                Assert.IsTrue(day.Succeeded, day.Message);
            }
            var after = governance.FindResident("resident_shortage");
            Assert.Less(after.Health, before.Health, "缺水/缺粮必须降低健康。");
            Assert.Less(after.Satisfaction, before.Satisfaction, "缺水/缺粮和失业必须降低满意度。");
            Assert.IsTrue(governance.GetReadModel().Incidents.Count > 0, "治安事件由世界日和 seed 确定性生成。");
            var incident = governance.GetReadModel().Incidents[0];
            var resolved = governance.ResolveSecurityIncident(new D13SecurityIncidentRequest
            {
                OperationId = "resolve_security_" + incident.IncidentId, IslandId = D13GovernanceIdsIsland, IncidentId = incident.IncidentId,
                ResolutionId = "route_security_mediation", WorldDay = 50, SaveRevision = 50,
            });
            Assert.IsTrue(resolved.Succeeded, resolved.Message);
            Assert.IsTrue(resolved.Incident.Resolved);
            Assert.IsNotEmpty(resolved.Incident.ResolutionId);
            Assert.IsTrue(governance.EventLog.Find(resolved.EventId, 50) != null);
        }

        [Test]
        public void HousingJobAndAuthorityBoundariesPreventUnownedWrites()
        {
            var definition = D13IslandGovernanceCatalog.FindIsland(D13IslandGovernanceCatalog.SaintMarysIslandId);
            definition.HousingCapacity = 1;
            var log = CreateSources();
            var governance = NewGovernance(log, true, true, true, true, true, true, null, definition, new[]
            {
                new D13GovernanceJobDefinition { JobId = "job_one_slot", DisplayNameKey = "job.one", Slots = 1, WagePerDay = 10, RequiredEducation = 0, SecurityContribution = 1, Tags = new List<string> { "labor" } },
            });
            Assert.IsTrue(governance.MigrateIn(new D13MigrationInRequest { OperationId = "housing_first", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_first", HouseholdId = "family_first", ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 1 }).Succeeded);
            var housingFull = governance.MigrateIn(new D13MigrationInRequest { OperationId = "housing_second", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_second", HouseholdId = "family_second", ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 2 });
            Assert.IsFalse(housingFull.Succeeded);
            Assert.AreEqual("housing_full", housingFull.Code);
            Assert.IsTrue(governance.AssignJob(new D13JobAssignmentRequest { OperationId = "job_first", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_first", JobId = "job_one_slot", WorldDay = 2, SaveRevision = 3 }).Succeeded);
            var jobFull = governance.AssignJob(new D13JobAssignmentRequest { OperationId = "job_second", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_second", JobId = "job_one_slot", WorldDay = 2, SaveRevision = 4 });
            Assert.IsFalse(jobFull.Succeeded);
            Assert.AreEqual("job_or_resident_missing", jobFull.Code);

            var denied = NewGovernance(log, false, true, true, true, true, true);
            var deniedMigration = denied.MigrateIn(new D13MigrationInRequest { OperationId = "denied_migration", IslandId = D13GovernanceIdsIsland, ResidentId = "resident_denied", HouseholdId = "family_denied", ReviewAcknowledged = true, WorldDay = 1, SaveRevision = 1 });
            Assert.IsFalse(deniedMigration.Succeeded);
            Assert.AreEqual(0, denied.GetReadModel().Residents.Count);
            var proposal = governance.ProposePolicy(new D13GovernancePolicyProposal { ProposalId = "agent_policy_proposal", IslandId = D13GovernanceIdsIsland, TaxRateBps = 3000, EducationOpen = false });
            Assert.IsTrue(proposal.ProposedByAgent);
            Assert.AreNotEqual(3000, governance.GetReadModel().Policy.TaxRateBps, "Agent 提案不能直接写税率。");
        }

        private static string D13GovernanceIdsIsland { get { return D13IslandGovernanceCatalog.SaintMarysIslandId; } }

        private static D13IslandGovernanceSystem NewGovernance(EventLog log, bool residence, bool fullProvision, bool economy, bool policy, bool community, bool agent,
            D13GovernanceRuntimeSnapshot initial = null, D13IslandGovernanceDefinition definition = null, IEnumerable<D13GovernanceJobDefinition> jobs = null)
        {
            return new D13IslandGovernanceSystem(new FixtureResidence(residence), new FixtureProvision(log, fullProvision), new FixtureEconomy(log, economy),
                new FixturePolicy(log, policy), new FixtureCommunity(log, community), new FixtureAgent(log, agent), log, initial, definition, jobs);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            foreach (var id in new[] { "property_rights", "legal_rights", "migration_alice_source", "migration_beto_source", "migration_shortage_source", "economy_wage", "economy_tax", "policy_approved", "community_agreement", "agent_approved", "food_supply", "water_supply" })
                AddSource(log, id, id == "property_rights" ? D13GovernanceIds.PropertyOwner : id == "legal_rights" ? D13GovernanceIds.LegalOwner : id == "food_supply" || id == "water_supply" ? D13GovernanceIds.EcologyOwner : id == "community_agreement" ? D13GovernanceIds.CommunityOwner : id == "agent_approved" ? D13GovernanceIds.AgentOwner : D13GovernanceIds.EconomyOwner);
            return log;
        }

        private static void AddSource(EventLog log, string eventId, string owner)
        {
            log.Append(new WorldEvent(eventId, 0, D13GovernanceIdsIsland, owner, new EventResult { ResultCode = "fixture_source", Summary = "fixture source", FactIds = new List<string> { "fixture:" + eventId } }, new[] { D13GovernanceIds.PlayerRef }, new[] { "fixture" }, new string[0], 0));
        }

        private sealed class FixtureResidence : ID13GovernanceResidenceAuthority
        {
            private readonly bool _allowed;
            public FixtureResidence(bool allowed) { _allowed = allowed; }
            public D13GovernanceResidenceDecision ValidateResidence(D13IslandGovernanceDefinition definition, D13MigrationInRequest request, int worldDay)
            {
                return _allowed ? new D13GovernanceResidenceDecision { Allowed = true, Code = "residence_approved", PropertySourceEventId = "property_rights", LegalSourceEventId = "legal_rights", SourceOwnerSystem = D13GovernanceIds.PropertyOwner } : new D13GovernanceResidenceDecision { Allowed = false, Code = "property_dispute", Message = "fixture residence denied" };
            }
        }

        private sealed class FixtureProvision : ID13GovernanceProvisionAuthority
        {
            private readonly EventLog _log; private readonly bool _full;
            public FixtureProvision(EventLog log, bool full) { _log = log; _full = full; }
            public D13GovernanceProvisionDecision Provide(D13GovernanceProvisionRequest request)
            {
                return new D13GovernanceProvisionDecision { Allowed = true, Code = "provision_approved", FoodUnits = _full ? request.FoodRequired : 0, WaterUnits = _full ? request.WaterRequired : 0, FoodSourceEventId = "food_supply", WaterSourceEventId = "water_supply", FoodSourceOwnerSystem = D13GovernanceIds.EcologyOwner, WaterSourceOwnerSystem = D13GovernanceIds.EcologyOwner };
            }
        }

        private sealed class FixtureEconomy : ID13GovernanceEconomyAuthority
        {
            private readonly bool _allowed;
            public FixtureEconomy(EventLog log, bool allowed) { _allowed = allowed; }
            public D13GovernanceWageDecision ApproveWage(D13ResidentState resident, D13GovernanceJobDefinition job, int worldDay, long saveRevision)
            {
                return new D13GovernanceWageDecision { Allowed = _allowed, Code = "wage_approved", Amount = job.WagePerDay, Currency = D13GovernanceIds.Currency, LedgerRef = "ledger_governance_fixture", SourceEventId = "economy_wage", SourceOwnerSystem = D13GovernanceIds.EconomyOwner };
            }
            public D13GovernanceTaxDecision AssessTax(string householdId, int taxBase, int taxRateBps, int worldDay, long saveRevision)
            {
                var due = taxBase * taxRateBps / 10000;
                return new D13GovernanceTaxDecision { Allowed = _allowed, Code = "tax_approved", AmountDue = due, AmountCollected = due, LedgerRef = "ledger_tax_fixture", SourceEventId = "economy_tax", SourceOwnerSystem = D13GovernanceIds.EconomyOwner };
            }
        }

        private sealed class FixturePolicy : ID13GovernancePolicyAuthority
        {
            private readonly EventLog _log; private readonly bool _allowed;
            public FixturePolicy(EventLog log, bool allowed) { _log = log; _allowed = allowed; }
            public D13GovernancePolicyDecision ApprovePolicy(D13IslandGovernanceDefinition definition, D13GovernancePolicyRequest request, D13GovernancePolicyState currentState)
            {
                return new D13GovernancePolicyDecision { Allowed = _allowed, Code = "policy_approved", TaxRateBps = request.TaxRateBps, WaterRationingLevel = request.WaterRationingLevel, FoodRationingLevel = request.FoodRationingLevel, EducationOpen = request.EducationOpen, ReligionArrangementId = request.ReligionArrangementId, SecurityPostureId = request.SecurityPostureId, SourceEventId = "policy_approved", SourceOwnerSystem = D13GovernanceIds.LegalOwner };
            }
        }

        private sealed class FixtureCommunity : ID13GovernanceCommunityAuthority
        {
            private readonly EventLog _log; private readonly bool _allowed;
            public FixtureCommunity(EventLog log, bool allowed) { _log = log; _allowed = allowed; }
            public D13GovernanceAgreementDecision ValidateAgreement(D13IslandGovernanceDefinition definition, D13GovernanceAgreementRequest request, int worldDay)
            {
                return new D13GovernanceAgreementDecision { Allowed = _allowed, Code = "agreement_approved", AgreementId = request.AgreementId, CommunityId = request.CommunityId, Status = "active", SourceEventId = "community_agreement", SourceOwnerSystem = D13GovernanceIds.CommunityOwner, Content = new D13GovernanceContentTag { ContentId = "agreement_green_cay_content", HistoricalTag = "inferred", SensitiveContentLabel = "community_history", ContentWarning = "共同体历史语境需审查", ReviewStatus = D13GovernanceContentReviewStatus.ApprovedWithWarning, SourceNote = "fixture_review", Tags = new List<string> { "community", "agreement", "reviewed" } } };
            }
        }

        private sealed class FixtureAgent : ID13GovernanceAgentAuthority
        {
            private readonly EventLog _log; private readonly bool _allowed;
            public FixtureAgent(EventLog log, bool allowed) { _log = log; _allowed = allowed; }
            public D13GovernanceAgentDecision ApproveManagement(D13IslandGovernanceDefinition definition, D13GovernanceAgentRequest request, int worldDay)
            {
                return new D13GovernanceAgentDecision { Allowed = _allowed, Code = "agent_approved", AgentRef = request.AgentRef, ManagementRole = request.ManagementRole, SourceEventId = "agent_approved", SourceOwnerSystem = D13GovernanceIds.AgentOwner };
            }
        }
    }
}
