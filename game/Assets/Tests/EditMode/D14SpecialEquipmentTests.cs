// D14-A03 特殊武器/船体模块：唯一 ID、来源授权、强度边界、装配、损坏、维修、出售、收缴、恢复和存档。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D14;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14SpecialEquipmentTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_equipment_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void SpecialEquipmentLifecycleUsesStableIdsAuthoritiesAndSaveRestore()
        {
            var log = CreateSources();
            var fixture = new AuthorityFixture();
            var system = new D14SpecialEquipmentSystem(log, null, fixture, fixture, fixture, fixture, fixture);

            var acquired = system.Acquire(new D14EquipmentAcquireRequest
            {
                OperationId = "acquire_moonwake", DefinitionId = "item_special_weapon_moonwake_cutlass", SourceEventId = "collection_moonwake",
                AuthorizationEventId = "harbor_armory_auth", HistoricalReviewEventId = "history_moonwake_review", HistoricalWarningAcknowledged = true,
                OwnerRef = "player", WorldDay = 1, SaveRevision = 1, CauseEventIds = new List<string> { "collection_moonwake", "harbor_armory_auth" },
            });
            Assert.IsTrue(acquired.Accepted, acquired.Message);
            var instanceId = D14SpecialEquipmentIds.CreateInstanceId("item_special_weapon_moonwake_cutlass", "collection_moonwake");
            Assert.AreEqual(instanceId, acquired.Equipment.InstanceId);
            Assert.AreEqual(70, acquired.Equipment.Strength);

            var install = system.Install(new D14EquipmentInstallRequest
            {
                OperationId = "install_moonwake", InstanceId = instanceId, ShipId = "player_ship", ShipTypeId = "ship_moonwake",
                RequestedSlot = D14EquipmentSlot.DeckArmament, RequestedStrength = 100, WorldDay = 1, SaveRevision = 2,
                CauseEventIds = new List<string> { acquired.EventId },
            });
            Assert.IsTrue(install.Accepted, install.Message);
            Assert.AreEqual(D14EquipmentStatus.Installed, system.Find(instanceId).Status);
            Assert.AreEqual(70, system.Find(instanceId).Strength);

            var damaged = system.Damage(new D14EquipmentDamageRequest
            {
                OperationId = "damage_moonwake", InstanceId = instanceId, ShipId = "player_ship", CombatSourceEventId = "combat_damage",
                DamageContext = "gun_mount_hit", WorldDay = 2, SaveRevision = 3, CauseEventIds = new List<string> { install.EventId, "combat_damage" },
            });
            Assert.IsTrue(damaged.Accepted, damaged.Message);
            Assert.AreEqual(D14EquipmentStatus.Damaged, system.Find(instanceId).Status);
            Assert.AreEqual(68, system.Find(instanceId).Durability);

            var maintained = system.Maintain(new D14EquipmentMaintainRequest
            {
                OperationId = "maintain_moonwake", InstanceId = instanceId, ShipId = "player_ship", RepairMaterialEventId = "inventory_repair",
                RequestedDurability = 100, WorldDay = 2, SaveRevision = 4, CauseEventIds = new List<string> { damaged.EventId, "inventory_repair" },
            });
            Assert.IsTrue(maintained.Accepted, maintained.Message);
            Assert.AreEqual(D14EquipmentStatus.Maintained, system.Find(instanceId).Status);
            Assert.AreEqual(100, system.Find(instanceId).Durability);

            var sold = system.Sell(new D14EquipmentSellRequest
            {
                OperationId = "sell_moonwake", InstanceId = instanceId, SellerRef = "player", LegalSourceEventId = "sale_legal",
                WorldDay = 3, SaveRevision = 5, CauseEventIds = new List<string> { maintained.EventId, "sale_legal" },
            });
            Assert.IsTrue(sold.Accepted, sold.Message);
            Assert.AreEqual(D14EquipmentStatus.Sold, system.Find(instanceId).Status);
            Assert.AreEqual(33, sold.Operation.SaleValue);
            Assert.IsTrue(fixture.SaleCalled);

            var grapnel = system.Acquire(new D14EquipmentAcquireRequest
            {
                OperationId = "acquire_grapnel", DefinitionId = "item_tool_rescue_grapnel", SourceEventId = "reward_grapnel",
                AuthorizationEventId = "legal_tool_auth", OwnerRef = "player", WorldDay = 3, SaveRevision = 6,
                CauseEventIds = new List<string> { "reward_grapnel", "legal_tool_auth" },
            });
            Assert.IsTrue(grapnel.Accepted, grapnel.Message);
            var seized = system.Seize(new D14EquipmentSeizeRequest
            {
                OperationId = "seize_grapnel", InstanceId = grapnel.Equipment.InstanceId, LegalSourceEventId = "seizure_order", EvidenceRef = "evidence_grapnel",
                WorldDay = 3, SaveRevision = 7, CauseEventIds = new List<string> { grapnel.EventId, "seizure_order" },
            });
            Assert.IsTrue(seized.Accepted, seized.Message);
            Assert.AreEqual(D14EquipmentStatus.Seized, system.Find(grapnel.Equipment.InstanceId).Status);
            Assert.AreEqual(D14EquipmentLegalStatus.Evidence, system.Find(grapnel.Equipment.InstanceId).LegalStatus);

            var signal = system.Acquire(new D14EquipmentAcquireRequest
            {
                OperationId = "acquire_signal", DefinitionId = "item_signal_lantern_amber", SourceEventId = "reward_signal",
                AuthorizationEventId = "legal_tool_auth", OwnerRef = "player", WorldDay = 4, SaveRevision = 8,
                CauseEventIds = new List<string> { "reward_signal", "legal_tool_auth" },
            });
            Assert.IsTrue(signal.Accepted, signal.Message);
            fixture.LoseNextDamage = true;
            var lost = system.Damage(new D14EquipmentDamageRequest
            {
                OperationId = "lose_signal", InstanceId = signal.Equipment.InstanceId, CombatSourceEventId = "combat_loss",
                DamageContext = "washed_overboard", WorldDay = 4, SaveRevision = 9, CauseEventIds = new List<string> { signal.EventId, "combat_loss" },
            });
            Assert.IsTrue(lost.Accepted, lost.Message);
            Assert.AreEqual(D14EquipmentStatus.Lost, system.Find(signal.Equipment.InstanceId).Status);
            var recovered = system.Recover(new D14EquipmentRecoverRequest
            {
                OperationId = "recover_signal", InstanceId = signal.Equipment.InstanceId, RecoverySourceEventId = "reward_recover",
                AuthorizationEventId = "legal_tool_auth", OwnerRef = "player", WorldDay = 5, SaveRevision = 10,
                CauseEventIds = new List<string> { lost.EventId, "reward_recover" },
            });
            Assert.IsTrue(recovered.Accepted, recovered.Message);
            Assert.AreEqual(D14EquipmentStatus.Recovered, system.Find(signal.Equipment.InstanceId).Status);
            Assert.AreEqual(signal.Equipment.InstanceId, recovered.Equipment.InstanceId);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 装备测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14SpecialEquipmentRuntime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14SpecialEquipmentRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(saved.Equipment.Count, loaded.Equipment.Count);
            Assert.AreEqual(saved.Operations.Count, loaded.Operations.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = new D14SpecialEquipmentSystem(null, null, fixture, fixture, fixture, fixture, fixture, loaded);
            var duplicate = restored.Sell(new D14EquipmentSellRequest
            {
                OperationId = "sell_moonwake", InstanceId = instanceId, SellerRef = "player", LegalSourceEventId = "sale_legal", WorldDay = 3, SaveRevision = 5,
            });
            Assert.IsTrue(duplicate.Accepted && duplicate.Duplicate);
            Assert.AreEqual(saved.Equipment.Count, restored.Snapshot.Equipment.Count);
            Assert.AreEqual(saved.Operations.Count, restored.Snapshot.Operations.Count);
        }

        [Test]
        public void HistoricalWarningAndStrengthBoundaryRejectAgentLikeOverrides()
        {
            var log = CreateSources();
            var fixture = new AuthorityFixture { ReturnOverCapStrength = true };
            var system = new D14SpecialEquipmentSystem(log, null, fixture, fixture, fixture, fixture, fixture);
            var warning = system.Acquire(new D14EquipmentAcquireRequest
            {
                OperationId = "acquire_warning_without_ack", DefinitionId = "item_firearm_matchlock_carbine", SourceEventId = "reward_firearm",
                AuthorizationEventId = "legal_tool_auth", HistoricalReviewEventId = "history_firearm_review", OwnerRef = "player",
                HistoricalWarningAcknowledged = false, WorldDay = 1, SaveRevision = 20,
            });
            Assert.IsFalse(warning.Accepted);
            Assert.AreEqual("historical_warning_unacknowledged", warning.Code);

            var overCap = system.Acquire(new D14EquipmentAcquireRequest
            {
                OperationId = "acquire_over_cap", DefinitionId = "item_firearm_matchlock_carbine", SourceEventId = "reward_firearm_2",
                AuthorizationEventId = "legal_tool_auth", HistoricalReviewEventId = "history_firearm_review", OwnerRef = "player",
                HistoricalWarningAcknowledged = true, WorldDay = 1, SaveRevision = 21,
            });
            Assert.IsFalse(overCap.Accepted);
            Assert.AreEqual("strength_over_cap", overCap.Code);
            Assert.AreEqual(0, system.Snapshot.Equipment.Count);
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "collection_moonwake", "CollectionAtlasSystem"); AddSource(log, "reward_grapnel", D14SpecialEquipmentIds.RewardOwner);
            AddSource(log, "reward_signal", D14SpecialEquipmentIds.RewardOwner); AddSource(log, "reward_firearm", D14SpecialEquipmentIds.RewardOwner); AddSource(log, "reward_firearm_2", D14SpecialEquipmentIds.RewardOwner); AddSource(log, "reward_recover", D14SpecialEquipmentIds.RewardOwner);
            AddSource(log, "harbor_armory_auth", "HarborAuthority"); AddSource(log, "legal_tool_auth", D14SpecialEquipmentIds.LegalOwner); AddSource(log, "history_moonwake_review", D14SpecialEquipmentIds.HistoricalReviewOwner); AddSource(log, "history_firearm_review", D14SpecialEquipmentIds.HistoricalReviewOwner);
            AddSource(log, "inventory_grant", D14SpecialEquipmentIds.InventoryOwner); AddSource(log, "inventory_repair", D14SpecialEquipmentIds.InventoryOwner); AddSource(log, "inventory_sale", D14SpecialEquipmentIds.InventoryOwner); AddSource(log, "inventory_recover", D14SpecialEquipmentIds.InventoryOwner);
            AddSource(log, "ship_install", D14SpecialEquipmentIds.ShipOwner); AddSource(log, "ship_maintain", D14SpecialEquipmentIds.ShipOwner); AddSource(log, "ship_remove", D14SpecialEquipmentIds.ShipOwner);
            AddSource(log, "combat_damage", D14SpecialEquipmentIds.CombatOwner); AddSource(log, "combat_loss", D14SpecialEquipmentIds.CombatOwner); AddSource(log, "economy_sale", D14SpecialEquipmentIds.EconomyOwner); AddSource(log, "sale_legal", D14SpecialEquipmentIds.LegalOwner); AddSource(log, "seizure_order", D14SpecialEquipmentIds.LegalOwner);
            return log;
        }

        private static void AddSource(EventLog log, string id, string owner)
        {
            log.Append(new WorldEvent(id, 0, "test_source", owner, new EventResult { ResultCode = "source_ready", Summary = "D14-A03 测试来源事实。", FactIds = new List<string> { "source:" + id } }, new[] { "player" }, new[] { "test:d14_a03" }));
        }

        private sealed class AuthorityFixture : ID14EquipmentRewardAuthority, ID14EquipmentShipAuthority, ID14EquipmentCombatAuthority, ID14EquipmentTradeAuthority, ID14EquipmentLegalAuthority
        {
            public bool ReturnOverCapStrength;
            public bool LoseNextDamage;
            public bool SaleCalled;

            public D14EquipmentAcquireDecision Acquire(D14EquipmentAcquireRequest request, D14SpecialEquipmentDefinition definition, FactEvent sourceFact, FactEvent authorizationFact)
            {
                return new D14EquipmentAcquireDecision
                {
                    Allowed = !ReturnOverCapStrength, Code = ReturnOverCapStrength ? "strength_over_cap" : "acquired", Message = "fixture acquisition",
                    SourceOwnerSystem = definition.AcquisitionOwnerSystem, AcquisitionOwnerSystem = definition.AcquisitionOwnerSystem, SourceEventId = request.SourceEventId,
                    AuthorizationEventId = request.AuthorizationEventId, HistoricalReviewSourceEventId = request.HistoricalReviewEventId,
                    InventorySourceEventId = "inventory_grant", InstanceId = D14SpecialEquipmentIds.CreateInstanceId(definition.DefinitionId, request.SourceEventId),
                    StrengthAfter = ReturnOverCapStrength ? definition.StrengthCap + 1 : Math.Min(definition.StrengthCap, 70), DurabilityAfter = definition.MaxDurability,
                    Status = D14EquipmentStatus.Acquired, LegalStatus = definition.RequiresAuthorization ? D14EquipmentLegalStatus.Licensed : D14EquipmentLegalStatus.Legal,
                };
            }

            public D14EquipmentDecision Recover(D14EquipmentRecoverRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent sourceFact)
            {
                return new D14EquipmentDecision { Allowed = true, Code = "recovered", Message = "fixture recovery", SourceOwnerSystem = D14SpecialEquipmentIds.RewardOwner, SourceEventId = request.RecoverySourceEventId, InstanceId = equipment.InstanceId, Status = D14EquipmentStatus.Recovered, StrengthAfter = 20, DurabilityAfter = 60, InventorySourceEventId = "inventory_recover", AuthorizationEventId = request.AuthorizationEventId };
            }

            public D14EquipmentDecision Install(D14EquipmentInstallRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition)
            {
                return new D14EquipmentDecision { Allowed = true, Code = "installed", Message = "fixture install", SourceOwnerSystem = D14SpecialEquipmentIds.ShipOwner, SourceEventId = "ship_install", ShipSourceEventId = "ship_install", InstanceId = equipment.InstanceId, Status = D14EquipmentStatus.Installed, Slot = definition.Slot, StrengthAfter = equipment.Strength, DurabilityAfter = equipment.Durability };
            }

            public D14EquipmentDecision Maintain(D14EquipmentMaintainRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition)
            {
                return new D14EquipmentDecision { Allowed = true, Code = "maintained", Message = "fixture maintain", SourceOwnerSystem = D14SpecialEquipmentIds.ShipOwner, SourceEventId = "ship_maintain", ShipSourceEventId = "ship_maintain", InventorySourceEventId = "inventory_repair", InstanceId = equipment.InstanceId, Status = D14EquipmentStatus.Maintained, Slot = definition.Slot, StrengthAfter = equipment.Strength, DurabilityAfter = definition.MaxDurability };
            }

            public D14EquipmentDecision ApplyDamage(D14EquipmentDamageRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent combatFact)
            {
                bool lost = LoseNextDamage; LoseNextDamage = false;
                return new D14EquipmentDecision { Allowed = true, Code = lost ? "equipment_lost" : "equipment_damaged", Message = "fixture damage", SourceOwnerSystem = D14SpecialEquipmentIds.CombatOwner, SourceEventId = request.CombatSourceEventId, InstanceId = equipment.InstanceId, Status = lost ? D14EquipmentStatus.Lost : D14EquipmentStatus.Damaged, StrengthAfter = lost ? 0 : Math.Max(0, equipment.Strength - 20), DurabilityAfter = lost ? 0 : Math.Max(0, equipment.Durability - 32) };
            }

            public D14EquipmentDecision Sell(D14EquipmentSellRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact)
            {
                SaleCalled = true;
                return new D14EquipmentDecision { Allowed = true, Code = "sold", Message = "fixture sale", SourceOwnerSystem = D14SpecialEquipmentIds.EconomyOwner, SourceEventId = "economy_sale", EconomySourceEventId = "economy_sale", InventorySourceEventId = "inventory_sale", ShipSourceEventId = equipment.Status == D14EquipmentStatus.Installed ? "ship_remove" : null, InstanceId = equipment.InstanceId, Status = D14EquipmentStatus.Sold, StrengthAfter = equipment.Strength, DurabilityAfter = equipment.Durability, SaleValue = 33 };
            }

            public D14EquipmentDecision Seize(D14EquipmentSeizeRequest request, D14EquipmentInstance equipment, D14SpecialEquipmentDefinition definition, FactEvent legalFact)
            {
                return new D14EquipmentDecision { Allowed = true, Code = "seized", Message = "fixture seizure", SourceOwnerSystem = D14SpecialEquipmentIds.LegalOwner, SourceEventId = request.LegalSourceEventId, LegalSourceEventId = request.LegalSourceEventId, InstanceId = equipment.InstanceId, Status = D14EquipmentStatus.Seized, LegalStatus = D14EquipmentLegalStatus.Evidence, StrengthAfter = equipment.Strength, DurabilityAfter = equipment.Durability };
            }
        }
    }
}
