// D14-S01：战斗/战后跨域因果、特殊装备生命周期、违法后果和奖励读档幂等。

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Whalebone.Systems.D14;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14S01IntegrationTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_s01_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void CombatAftercareEquipmentLegalAndRewardChainIsFactDrivenAndSaveIdempotent()
        {
            var log = CreateSources();
            var owners = new OwnerFixture();
            var system = new D14S01IntegrationSystem(log, owners, owners, owners, owners, owners, owners, owners, owners, owners);

            var combat = new D14CombatResult { Accepted = true, Code = "victory", EventId = "combat_resolved", Fact = log.Find("combat_resolved", 0), Status = D14NavalCombatStatus.Victory };
            var combatRequest = new D14S01CombatIntegrationRequest
            {
                OperationId = "combat_chain_01", PlayerRef = "player", PlayerShipId = "player_ship", CombatResult = combat,
                LegalIntent = new D14S01LegalIntent { ActionKind = "privateer_action", SubjectRef = "player", FactionId = "spain", PortId = "green_cay", PermitRef = "permit_source", EvidenceEventId = "permit_source" },
                Equipment = new D14S01EquipmentLifecycleRequest { OperationId = "eq_damage_from_combat", Operation = D14EquipmentOperationKind.Damage, InstanceId = "item_special_weapon_moonwake_cutlass@collection_moonwake", OwnerRef = "player", ShipId = "player_ship", SourceEventId = "combat_resolved" },
                QuestId = "quest_d14_combat_chain", RelationTargetRef = "crew_isaac_reed", WorldDay = 4, SaveRevision = 0,
            };
            var combatResult = system.IntegrateCombat(combatRequest);
            Assert.IsTrue(combatResult.Accepted, combatResult.Message);
            Assert.AreEqual(1, owners.HullChanges);
            Assert.AreEqual(1, owners.CrewChanges);
            Assert.AreEqual(1, owners.MoneyChanges);
            Assert.AreEqual(1, owners.LegalChanges);
            Assert.AreEqual(1, owners.QuestChanges);
            Assert.AreEqual(1, owners.RelationChanges);
            Assert.AreEqual(1, owners.EquipmentChanges);
            Assert.IsTrue(owners.PortDenied, "违法/私掠后果由 FactionLegal authority 改变港口许可。");
            Assert.Less(owners.ReputationAfter, 0, "势力声望由 FactionLegal authority 改变。");
            var combatDuplicate = system.IntegrateCombat(combatRequest);
            Assert.IsTrue(combatDuplicate.Accepted && combatDuplicate.Duplicate);
            Assert.AreEqual(1, owners.HullChanges);

            var aftercare = new D14AftercareResult { Accepted = true, Code = "captured", EventId = "aftercare_resolved", Fact = log.Find("aftercare_resolved", 0), BoardingCase = new D14BoardingCase { CaseId = "boarding_01", EncounterId = "encounter_01", PlayerShipId = "player_ship", EnemyShipId = "enemy_ship", Status = D14BoardingCaseStatus.Captured, Outcome = D14AftercareOutcome.Captured, CombatSourceEventId = "combat_resolved", LastEventId = "aftercare_resolved" } };
            var aftercareResult = system.IntegrateAftercare(new D14S01AftercareIntegrationRequest
            {
                OperationId = "aftercare_chain_01", PlayerRef = "player", AftercareResult = aftercare,
                LegalIntent = new D14S01LegalIntent { ActionKind = "illegal_prisoner_capture", SubjectRef = "player", FactionId = "spain", PortId = "green_cay", EvidenceEventId = "aftercare_resolved" },
                QuestId = "quest_d14_aftercare_chain", RelationTargetRef = "crew_isaac_reed", WorldDay = 5, SaveRevision = 0,
            });
            Assert.IsTrue(aftercareResult.Accepted, aftercareResult.Message);
            Assert.AreEqual(2, owners.CrewChanges);
            Assert.AreEqual(1, owners.CargoChanges);
            Assert.AreEqual(2, owners.MoneyChanges);
            Assert.AreEqual(2, owners.LegalChanges);
            Assert.AreEqual(2, owners.QuestChanges);
            Assert.AreEqual(2, owners.RelationChanges);

            var acquired = system.IntegrateEquipment(new D14S01EquipmentIntegrationRequest
            {
                OperationId = "equipment_acquire_01", SourceEventId = "equipment_acquire", WorldDay = 5, SaveRevision = 0,
                Equipment = new D14S01EquipmentLifecycleRequest { OperationId = "equipment_acquire_01", Operation = D14EquipmentOperationKind.Acquisition, DefinitionId = "item_special_weapon_moonwake_cutlass", OwnerRef = "player", ShipId = "player_ship", SourceEventId = "equipment_acquire" },
            });
            var installed = system.IntegrateEquipment(new D14S01EquipmentIntegrationRequest
            {
                OperationId = "equipment_install_01", SourceEventId = "equipment_install", WorldDay = 5, SaveRevision = 0,
                Equipment = new D14S01EquipmentLifecycleRequest { OperationId = "equipment_install_01", Operation = D14EquipmentOperationKind.Installation, InstanceId = "item_special_weapon_moonwake_cutlass@collection_moonwake", OwnerRef = "player", ShipId = "player_ship", SourceEventId = "equipment_install" },
            });
            var maintained = system.IntegrateEquipment(new D14S01EquipmentIntegrationRequest
            {
                OperationId = "equipment_maintain_01", SourceEventId = "equipment_maintain", WorldDay = 6, SaveRevision = 0,
                Equipment = new D14S01EquipmentLifecycleRequest { OperationId = "equipment_maintain_01", Operation = D14EquipmentOperationKind.Maintenance, InstanceId = "item_special_weapon_moonwake_cutlass@collection_moonwake", OwnerRef = "player", ShipId = "player_ship", SourceEventId = "equipment_maintain" },
            });
            Assert.IsTrue(acquired.Accepted && installed.Accepted && maintained.Accepted);
            Assert.AreEqual(4, owners.EquipmentChanges, "获取、装配、战斗损坏和维修都由 A03/装备 owner 处理。");

            var reward = system.IntegrateReward(new D14S01RewardIntegrationRequest
            {
                OperationId = "reward_aftercare_01", SourceEventId = aftercareResult.EventId, WorldDay = 5, SaveRevision = 0,
                Reward = new D14S01RewardIntent { RequestId = "reward_request_aftercare_01", RewardId = "reward_d14_unique_moonwake_armory", RecipientRef = "player", TaskId = "quest_d14_aftercare_chain" },
            });
            Assert.IsTrue(reward.Accepted, reward.Message);
            Assert.AreEqual(1, owners.RewardCalls);
            Assert.AreEqual("grant_aftercare_01", reward.Ledger.RewardGrantId);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14-S01 集成测试船长", ShipTypeId = "small_fishing" });
            SaveSystem.SaveD14S01IntegrationRuntime(system.CaptureSnapshot());
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14S01IntegrationRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(system.Snapshot.Ledger.Count, loaded.Ledger.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = new D14S01IntegrationSystem(null, owners, owners, owners, owners, owners, owners, owners, owners, owners, loaded);
            var duplicateReward = restored.IntegrateReward(new D14S01RewardIntegrationRequest
            {
                OperationId = "reward_aftercare_01", SourceEventId = aftercareResult.EventId, WorldDay = 5, SaveRevision = 0,
                Reward = new D14S01RewardIntent { RequestId = "reward_request_aftercare_01", RewardId = "reward_d14_unique_moonwake_armory", RecipientRef = "player", TaskId = "quest_d14_aftercare_chain" },
            });
            Assert.IsTrue(duplicateReward.Accepted && duplicateReward.Duplicate);
            Assert.AreEqual(1, owners.RewardCalls, "读档后重放不能再次请求 RewardSystem 发放。");
            Assert.AreEqual(loaded.Ledger.Count, restored.Snapshot.Ledger.Count);
        }

        [Test]
        public void AgentCannotSubmitFinalCrossDomainConsequence()
        {
            var owners = new OwnerFixture();
            var system = new D14S01IntegrationSystem(CreateSources(), owners, owners, owners, owners, owners, owners, owners, owners, owners);
            var rejected = system.IntegrateReward(new D14S01RewardIntegrationRequest
            {
                OperationId = "agent_reward_01", RequestedBySystem = "AgentReply", SourceEventId = "combat_resolved", WorldDay = 1, SaveRevision = 0,
                Reward = new D14S01RewardIntent { RequestId = "agent_reward_request", RewardId = "reward_d14_unique_moonwake_armory", RecipientRef = "player" },
            });
            Assert.IsFalse(rejected.Accepted);
            Assert.AreEqual("agent_cannot_integrate", rejected.Code);
            Assert.AreEqual(0, owners.RewardCalls);
            Assert.AreEqual(0, system.Snapshot.Revision);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            Add(log, "combat_resolved", D14NavalCombatIds.OwnerSystem);
            Add(log, "aftercare_resolved", D14AftercareIds.OwnerSystem);
            Add(log, "permit_source", D14FactionLegalIds.OwnerSystem);
            Add(log, "ship_hull", D14S01IntegrationIds.ShipOwner); Add(log, "crew_combat", D14S01IntegrationIds.CrewOwner); Add(log, "crew_aftercare", D14S01IntegrationIds.CrewOwner);
            Add(log, "economy_combat", D14S01IntegrationIds.EconomyOwner); Add(log, "economy_aftercare", D14S01IntegrationIds.EconomyOwner); Add(log, "inventory_loot", D14S01IntegrationIds.InventoryOwner);
            Add(log, "legal_privateer", D14S01IntegrationIds.LegalOwner); Add(log, "quest_combat", D14S01IntegrationIds.QuestOwner); Add(log, "quest_aftercare", D14S01IntegrationIds.QuestOwner);
            Add(log, "relation_combat", D14S01IntegrationIds.RelationOwner); Add(log, "relation_aftercare", D14S01IntegrationIds.RelationOwner);
            Add(log, "equipment_damage", D14S01IntegrationIds.EquipmentOwner); Add(log, "equipment_acquire", D14S01IntegrationIds.EquipmentOwner); Add(log, "equipment_install", D14S01IntegrationIds.EquipmentOwner); Add(log, "equipment_maintain", D14S01IntegrationIds.EquipmentOwner);
            Add(log, "reward_grant", D14S01IntegrationIds.RewardOwner);
            return log;
        }

        private static void Add(EventLog log, string id, string owner)
        {
            log.Append(new WorldEvent(id, 0, "d14_s01_test", owner, new EventResult { ResultCode = "source_ready", Summary = "D14-S01 测试来源事实。", FactIds = new List<string> { "source:" + id } }, new[] { "player" }, new[] { "test:d14_s01" }));
        }

        private sealed class OwnerFixture : ID14S01ShipAuthority, ID14S01CrewAuthority, ID14S01CargoAuthority, ID14S01EconomyAuthority, ID14S01LegalAuthority, ID14S01QuestAuthority, ID14S01RelationAuthority, ID14S01EquipmentAuthority, ID14S01RewardAuthority
        {
            public int HullChanges; public int CrewChanges; public int CargoChanges; public int MoneyChanges; public int LegalChanges; public int QuestChanges; public int RelationChanges; public int EquipmentChanges; public int RewardCalls; public int ReputationAfter; public bool PortDenied;

            public D14S01AuthorityDecision ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat) { HullChanges++; return Decision(D14S01IntegrationIds.ShipOwner, "ship_hull"); }
            D14S01AuthorityDecision ID14S01CrewAuthority.ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat) { CrewChanges++; return Decision(D14S01IntegrationIds.CrewOwner, "crew_combat"); }
            public D14S01AuthorityDecision ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare) { CrewChanges++; return Decision(D14S01IntegrationIds.CrewOwner, "crew_aftercare"); }
            D14S01AuthorityDecision ID14S01CargoAuthority.ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare) { CargoChanges++; return Decision(D14S01IntegrationIds.InventoryOwner, "inventory_loot"); }
            D14S01AuthorityDecision ID14S01EconomyAuthority.ApplyCombat(D14S01CombatIntegrationRequest request, D14CombatResult combat) { MoneyChanges++; return Decision(D14S01IntegrationIds.EconomyOwner, "economy_combat"); }
            D14S01AuthorityDecision ID14S01EconomyAuthority.ApplyAftercare(D14S01AftercareIntegrationRequest request, D14AftercareResult aftercare) { MoneyChanges++; return Decision(D14S01IntegrationIds.EconomyOwner, "economy_aftercare"); }
            public D14S01AuthorityDecision Apply(D14S01LegalIntent intent, FactEvent sourceFact, int worldDay, long saveRevision) { LegalChanges++; ReputationAfter -= 20; PortDenied = true; return Decision(D14S01IntegrationIds.LegalOwner, "legal_privateer"); }
            public D14S01AuthorityDecision Observe(string questId, string stage, FactEvent sourceFact, int worldDay, long saveRevision) { QuestChanges++; return Decision(D14S01IntegrationIds.QuestOwner, stage == "Combat" ? "quest_combat" : "quest_aftercare"); }
            public D14S01AuthorityDecision Apply(string relationTargetRef, string stage, FactEvent sourceFact, int worldDay, long saveRevision) { RelationChanges++; return Decision(D14S01IntegrationIds.RelationOwner, stage == "Combat" ? "relation_combat" : "relation_aftercare"); }
            public D14S01AuthorityDecision Apply(D14S01EquipmentLifecycleRequest request, FactEvent sourceFact, int worldDay, long saveRevision) { EquipmentChanges++; var source = request.Operation == D14EquipmentOperationKind.Damage ? "equipment_damage" : request.Operation == D14EquipmentOperationKind.Acquisition ? "equipment_acquire" : request.Operation == D14EquipmentOperationKind.Installation ? "equipment_install" : "equipment_maintain"; return Decision(D14S01IntegrationIds.EquipmentOwner, source); }
            public D14S01AuthorityDecision Grant(D14S01RewardIntent reward, FactEvent sourceFact, int worldDay, long saveRevision) { RewardCalls++; var result = Decision(D14S01IntegrationIds.RewardOwner, "reward_grant"); result.GrantId = "grant_aftercare_01"; return result; }

            private static D14S01AuthorityDecision Decision(string owner, string eventId) { return new D14S01AuthorityDecision { Allowed = true, Code = "applied", Message = "fixture owner applied", SourceOwnerSystem = owner, SourceEventId = eventId }; }
        }
    }
}
