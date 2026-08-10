// D14-A02 接舷与战后处理验收：authority 裁决、敏感内容、关系因果、保存恢复和非歼灭结局。

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Whalebone.Systems.D10;
using Whalebone.Systems.D14;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D14BoardingAftercareTests
    {
        private string _dbPath;

        [SetUp]
        public void SetUp()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "wb_d14_aftercare_" + Guid.NewGuid().ToString("N") + ".db");
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
        public void BoardingCaptureAftercareRelationAndSaveRestoreAreTraceable()
        {
            var log = CreateSources();
            var combat = StartCombat(log, "encounter_aftercare");
            var relationSystem = new RelationSystem(log, new[] { "player", "p1", "p2", "e1" });
            var fixture = new AuthorityFixture(combat.Snapshot.LastEventId);
            var system = CreateSystem(log, fixture, new D14BoardingAftercareSystem.D14D10RelationAuthority(relationSystem));

            var opened = system.RequestBoarding(new D14BoardingRequest
            {
                OperationId = "open_capture", CaseId = "case_capture", CombatSnapshot = combat.Snapshot,
                PlayerParty = Party(D14ParticipantSide.Player, "p1", "p2"), EnemyParty = Party(D14ParticipantSide.Enemy, "e1"),
                WorldDay = 2, SaveRevision = 10, CauseEventIds = new List<string> { combat.Snapshot.LastEventId },
            });
            Assert.IsTrue(opened.Accepted, opened.Message);
            var resolved = system.ResolveBoarding(new D14BoardingResolutionRequest
            {
                OperationId = "resolve_capture", CaseId = "case_capture", RequestedOutcome = D14AftercareOutcome.Captured,
                WorldDay = 2, SaveRevision = 11, CauseEventIds = new List<string> { opened.EventId },
            });
            Assert.IsTrue(resolved.Accepted, resolved.Message);
            Assert.AreEqual(D14BoardingCaseStatus.Captured, resolved.BoardingCase.Status);
            Assert.AreEqual(2, system.Snapshot.Casualties.Count);
            Assert.AreEqual(1, system.Snapshot.Prisoners.Count);
            Assert.IsTrue(relationSystem.ReadEdge("player", "p1", RelationType.Trust) != null);
            Assert.IsTrue(system.Snapshot.Relations.Count > 0);
            Assert.AreEqual(resolved.EventId, system.Snapshot.Relations[0].CauseEventId);

            var loot = system.RequestLoot(new D14LootRequest
            {
                OperationId = "loot_capture", CaseId = "case_capture", LegalSourceEventId = "legal_source",
                WorldDay = 2, SaveRevision = 12, CauseEventIds = new List<string> { resolved.EventId },
            });
            Assert.IsTrue(loot.Accepted, loot.Message);
            Assert.AreEqual(D14LootStatus.Granted, loot.Loot.Status);
            Assert.AreEqual("item_captured_cutlass", loot.Loot.Items[0].ItemId);
            Assert.IsTrue(fixture.LootCalled);

            var treated = system.Treat(new D14TreatmentRequest
            {
                OperationId = "treat_wounded", CaseId = "case_capture", CasualtyId = "casualty_wounded",
                WorldDay = 2, SaveRevision = 13, CauseEventIds = new List<string> { resolved.EventId },
            });
            Assert.IsTrue(treated.Accepted, treated.Message);
            Assert.AreEqual(D14CasualtyStatus.Recovered, system.FindCasualty("casualty_wounded").Status);

            var review = SensitiveWarning(true, false);
            var interrogation = system.Interrogate(new D14InterrogationRequest
            {
                OperationId = "interrogate_prisoner", CaseId = "case_capture", PrisonerId = "prisoner_e1", Content = review,
                WorldDay = 2, SaveRevision = 14, CauseEventIds = new List<string> { loot.EventId },
            });
            Assert.IsTrue(interrogation.Accepted, interrogation.Message);
            Assert.AreEqual("intel_windward_route", interrogation.Interrogation.InformationRef);

            var skipped = system.Interrogate(new D14InterrogationRequest
            {
                OperationId = "skip_interrogation", CaseId = "case_capture", PrisonerId = "prisoner_e1", Content = SensitiveWarning(false, true),
                WorldDay = 2, SaveRevision = 15, CauseEventIds = new List<string> { interrogation.EventId },
            });
            Assert.IsTrue(skipped.Accepted, skipped.Message);
            Assert.IsTrue(skipped.Interrogation.Skipped);
            Assert.AreEqual(1, fixture.InterrogationCalls);

            var funeral = system.HoldFuneral(new D14FuneralRequest
            {
                OperationId = "hold_funeral", CaseId = "case_capture", CasualtyIds = new List<string> { "casualty_dead" }, Content = SensitiveWarning(true, false),
                WorldDay = 2, SaveRevision = 16, CauseEventIds = new List<string> { resolved.EventId },
            });
            Assert.IsTrue(funeral.Accepted, funeral.Message);
            Assert.AreEqual(D14FuneralStatus.Held, funeral.Funeral.Status);

            var rescue = system.Rescue(new D14RescueRequest
            {
                OperationId = "rescue_prisoner", CaseId = "case_capture", PrisonerIds = new List<string> { "prisoner_e1" },
                WorldDay = 2, SaveRevision = 17, CauseEventIds = new List<string> { interrogation.EventId },
            });
            Assert.IsTrue(rescue.Accepted, rescue.Message);
            Assert.AreEqual(D14PrisonerStatus.Rescued, system.FindPrisoner("prisoner_e1").Status);

            SaveSystem.NewGame(new NewGameParams { CaptainName = "D14 战后测试船长", ShipTypeId = "small_fishing" });
            var saved = system.CaptureSnapshot();
            SaveSystem.SaveD14AftercareRuntime(saved);
            Assert.IsTrue(SaveSystem.Load());
            var loaded = SaveSystem.LoadD14AftercareRuntime();
            Assert.IsNotNull(loaded);
            Assert.AreEqual(saved.Cases.Count, loaded.Cases.Count);
            Assert.AreEqual(saved.Casualties.Count, loaded.Casualties.Count);
            Assert.AreEqual(saved.Prisoners.Count, loaded.Prisoners.Count);
            Assert.AreEqual(saved.Loot.Count, loaded.Loot.Count);
            Assert.AreEqual(saved.Funerals.Count, loaded.Funerals.Count);
            Assert.IsNotNull(loaded.EventLog);

            var restored = CreateSystem(null, fixture, new D14BoardingAftercareSystem.D14D10RelationAuthority(relationSystem), loaded);
            var duplicate = restored.RequestLoot(new D14LootRequest
            {
                OperationId = "loot_capture", CaseId = "case_capture", LegalSourceEventId = "legal_source", WorldDay = 2, SaveRevision = 12,
            });
            Assert.IsTrue(duplicate.Accepted && duplicate.Duplicate);
            Assert.AreEqual(saved.Loot.Count, restored.Snapshot.Loot.Count);
            Assert.AreEqual(saved.EventLog.Facts.Count, restored.SharedEventLog.Count);
        }

        [Test]
        public void RetreatAndSensitiveReviewRejectProvideNonAnnihilationRoutes()
        {
            var log = CreateSources();
            var combat = StartCombat(log, "encounter_retreat");
            var fixture = new AuthorityFixture(combat.Snapshot.LastEventId) { ResolveOutcome = D14AftercareOutcome.Retreated };
            var system = CreateSystem(log, fixture, null);
            var opened = system.RequestBoarding(new D14BoardingRequest
            {
                OperationId = "open_retreat", CaseId = "case_retreat", CombatSnapshot = combat.Snapshot,
                PlayerParty = Party(D14ParticipantSide.Player, "p1"), EnemyParty = Party(D14ParticipantSide.Enemy, "e1"),
                WorldDay = 3, SaveRevision = 20, CauseEventIds = new List<string> { combat.Snapshot.LastEventId },
            });
            Assert.IsTrue(opened.Accepted, opened.Message);
            var retreated = system.ResolveBoarding(new D14BoardingResolutionRequest
            {
                OperationId = "resolve_retreat", CaseId = "case_retreat", RequestedOutcome = D14AftercareOutcome.Retreated,
                WorldDay = 3, SaveRevision = 21, CauseEventIds = new List<string> { opened.EventId },
            });
            Assert.IsTrue(retreated.Accepted, retreated.Message);
            Assert.AreEqual(D14BoardingCaseStatus.Retreated, retreated.BoardingCase.Status);
            Assert.AreEqual(0, system.Snapshot.Prisoners.Count);

            var denied = system.Interrogate(new D14InterrogationRequest
            {
                OperationId = "review_pending", CaseId = "case_retreat", PrisonerId = "missing", Content = new D14SensitiveContentTag
                {
                    ContentId = "content_pending", HistoricalTag = "inferred", SourceNote = "test", ReviewStatus = D14SensitiveReviewStatus.Pending,
                }, WorldDay = 3, SaveRevision = 22,
            });
            Assert.IsFalse(denied.Accepted);
            Assert.AreEqual("prisoner_not_held", denied.Code);
        }

        [Test]
        public void SurrenderFlowsIntoPrisonerAftercareWithoutForcedLoot()
        {
            var log = CreateSources();
            var combat = StartCombat(log, "encounter_surrender");
            var surrender = combat.Surrender(new D14CombatSurrenderRequest
            {
                OperationId = "surrender_combat", WorldDay = 4, SaveRevision = 30, CauseEventIds = new List<string> { combat.Snapshot.LastEventId },
            });
            Assert.IsTrue(surrender.Accepted, surrender.Message);
            var fixture = new AuthorityFixture(surrender.EventId);
            var system = CreateSystem(log, fixture, null);
            var aftercare = system.ProcessSurrender(new D14SurrenderAftercareRequest
            {
                OperationId = "open_surrender", CaseId = "case_surrender", CombatSnapshot = surrender.State == null ? combat.Snapshot : BuildSurrenderSnapshot(combat.Snapshot, surrender.EventId),
                PlayerParty = Party(D14ParticipantSide.Player, "p1"), EnemyParty = Party(D14ParticipantSide.Enemy, "e1"),
                WorldDay = 4, SaveRevision = 31, CauseEventIds = new List<string> { surrender.EventId },
            });
            Assert.IsTrue(aftercare.Accepted, aftercare.Message);
            Assert.AreEqual(D14BoardingCaseStatus.Surrendered, system.FindCase("case_surrender").Status);
            Assert.AreEqual(1, system.Snapshot.Prisoners.Count);
            Assert.AreEqual(0, system.Snapshot.Loot.Count);
        }

        private static D14NavalCombatState BuildSurrenderSnapshot(D14NavalCombatState source, string lastEventId)
        {
            var result = source.Clone(); result.Status = D14NavalCombatStatus.Surrendered; result.LastEventId = lastEventId; return result;
        }

        private static D14BoardingAftercareSystem CreateSystem(EventLog log, AuthorityFixture fixture, ID14RelationAuthority relation, D14AftercareRuntimeSnapshot initial = null)
        {
            return new D14BoardingAftercareSystem(log, fixture, fixture, fixture, fixture, fixture, fixture, fixture, relation ?? fixture, initial);
        }

        private static D14NavalCombatSystem StartCombat(EventLog log, string encounterId)
        {
            var system = new D14NavalCombatSystem(log);
            var weather = D9WeatherSeaSnapshot.CreateDefault(); weather.LastEventId = "weather_source";
            var player = Profile("player_ship", 8f); var enemy = Profile("enemy_ship", 5f);
            var result = system.StartEncounter(new D14CombatStartRequest
            {
                OperationId = "start_" + encounterId, EncounterId = encounterId, PlayerShip = player, EnemyShip = enemy,
                Kinematics = new D14CombatKinematicsState { Distance = 20f, RelativeBearingDegrees = 20f, PlayerHeadingDegrees = 0f, EnemyHeadingDegrees = 180f, PlayerSpeed = 8f, EnemySpeed = 5f },
                Weather = weather, WeatherSourceEventId = "weather_source", WorldDay = 1, SaveRevision = 1, CauseEventIds = new List<string> { "weather_source" },
            });
            Assert.IsTrue(result.Accepted, result.Message);
            return system;
        }

        private static D14CombatShipProfile Profile(string id, float speed)
        {
            var result = new D14CombatShipProfile { ShipId = id, ShipTypeId = "test_sloop", HullMax = 200, Crew = 20, Speed = speed, GunnerSkill = 80, ManeuverSkill = 70 };
            result.Guns.Add(new D14GunState { GunId = "gun_0", Position = D14GunPosition.PortMidships, RoundShot = 2 });
            return result;
        }

        private static List<D14BoardingParticipant> Party(D14ParticipantSide side, params string[] ids)
        {
            var result = new List<D14BoardingParticipant>();
            foreach (var id in ids) result.Add(new D14BoardingParticipant { ParticipantId = id, Side = side, Role = side == D14ParticipantSide.Player ? D14ParticipantRole.Boarder : D14ParticipantRole.Sailor, Weapon = D14BoardingWeapon.Cutlass, Morale = 70, Health = 100 });
            return result;
        }

        private static D14SensitiveContentTag SensitiveWarning(bool acknowledge, bool skip)
        {
            return new D14SensitiveContentTag { ContentId = "content_d14_captivity", HistoricalTag = "primary_source_review", SensitiveLabel = "captivity_and_war", ContentWarning = "包含战争俘虏与死亡后果。", ReviewStatus = D14SensitiveReviewStatus.ApprovedWithWarning, ReviewAcknowledged = acknowledge, SkipSensitiveContent = skip, SourceNote = "D14-A02 test fixture", Tags = new List<string> { "war", "captivity" } };
        }

        private static EventLog CreateSources()
        {
            var log = new EventLog();
            AddSource(log, "weather_source", "EnvironmentSystem"); AddSource(log, "legal_source", D14AftercareIds.LegalOwner);
            AddSource(log, "casualty_source", D14AftercareIds.CasualtyOwner); AddSource(log, "prisoner_source", D14AftercareIds.PrisonerOwner);
            AddSource(log, "loot_source", D14AftercareIds.LootOwner); AddSource(log, "medical_source", D14AftercareIds.MedicalOwner);
            AddSource(log, "interrogation_source", D14AftercareIds.LegalOwner); AddSource(log, "funeral_source", D14AftercareIds.FuneralOwner);
            return log;
        }

        private static void AddSource(EventLog log, string id, string owner)
        {
            log.Append(new WorldEvent(id, 0, "test_source", owner, new EventResult { ResultCode = "source_ready", Summary = "测试 authority 来源事实。", FactIds = new List<string> { "source:" + id } }, new[] { "player", "p1", "e1" }, new[] { "test:d14_a02" }));
        }

        private sealed class AuthorityFixture : ID14CombatBoardingAuthority, ID14CasualtyAuthority, ID14PrisonerAuthority, ID14LootAuthority, ID14MedicalAuthority, ID14InterrogationAuthority, ID14FuneralAuthority, ID14RelationAuthority
        {
            private readonly string _combatSource;
            public D14AftercareOutcome ResolveOutcome = D14AftercareOutcome.Captured;
            public bool LootCalled;
            public int InterrogationCalls;
            public AuthorityFixture(string combatSource) { _combatSource = combatSource; }
            public D14BoardingDecision Evaluate(D14BoardingRequest request) { return new D14BoardingDecision { Allowed = true, Code = "boarding_ready", Message = "fixture allowed", SourceOwnerSystem = D14AftercareIds.CombatOwner, SourceEventId = _combatSource, PlayerMorale = 70, EnemyMorale = 40 }; }
            public D14BoardingDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase) { return new D14BoardingDecision { Allowed = true, Code = "boarding_fixture", Message = "fixture resolved", SourceOwnerSystem = D14AftercareIds.CombatOwner, SourceEventId = _combatSource, Outcome = ResolveOutcome == D14AftercareOutcome.Captured && request.RequestedOutcome == D14AftercareOutcome.Surrendered ? D14AftercareOutcome.Surrendered : ResolveOutcome, PlayerMorale = 65, EnemyMorale = 25, RelationProposals = new List<D14RelationProposal> { new D14RelationProposal { SourceId = "player", TargetId = "p1", Metric = D14RelationMetric.Trust, ProposedDelta = 5, Basis = "combat_rescue_or_command" } } }; }
            public D14CasualtyDecision Resolve(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, D14BoardingDecision decision) { return new D14CasualtyDecision { Allowed = true, Code = "casualties_resolved", Message = "fixture casualties", SourceOwnerSystem = D14AftercareIds.CasualtyOwner, SourceEventId = "casualty_source", Casualties = new List<D14CasualtyRecord> { new D14CasualtyRecord { CasualtyId = "casualty_wounded", ParticipantId = "p1", Side = D14ParticipantSide.Player, Status = D14CasualtyStatus.Wounded, HealthLoss = 40 }, new D14CasualtyRecord { CasualtyId = "casualty_dead", ParticipantId = "p2", Side = D14ParticipantSide.Player, Status = D14CasualtyStatus.Dead, HealthLoss = 100 } } }; }
            public D14PrisonerDecision Register(D14BoardingResolutionRequest request, D14BoardingCase boardingCase, FactEvent aftercareFact) { return new D14PrisonerDecision { Allowed = true, Code = "prisoners_registered", Message = "fixture prisoners", SourceOwnerSystem = D14AftercareIds.PrisonerOwner, SourceEventId = "prisoner_source", Prisoners = new List<D14PrisonerRecord> { new D14PrisonerRecord { PrisonerId = "prisoner_e1", ParticipantId = "e1", Status = D14PrisonerStatus.Held } } }; }
            public D14LootDecision Grant(D14LootRequest request, D14BoardingCase boardingCase, FactEvent aftercareFact) { LootCalled = true; return new D14LootDecision { Allowed = true, Code = "loot_granted", Message = "fixture loot", Status = D14LootStatus.Granted, SourceOwnerSystem = D14AftercareIds.LootOwner, SourceEventId = "loot_source", LegalSourceEventId = request.LegalSourceEventId, Items = new List<D14LootItem> { new D14LootItem { ItemId = "item_captured_cutlass", Quantity = 1, LegalStatus = "legal", InventorySourceEventId = "loot_source" } } }; }
            public D14MedicalDecision Treat(D14TreatmentRequest request, D14CasualtyRecord casualty, FactEvent aftercareFact) { return new D14MedicalDecision { Allowed = true, Code = "treated", Message = "fixture treatment", Status = D14TreatmentStatus.Treated, SourceOwnerSystem = D14AftercareIds.MedicalOwner, SourceEventId = "medical_source", HealthAfter = 90 }; }
            public D14RescueDecision Rescue(D14RescueRequest request, D14BoardingCase boardingCase, IReadOnlyList<D14PrisonerRecord> prisoners, FactEvent aftercareFact) { return new D14RescueDecision { Allowed = true, Code = "rescued", Message = "fixture rescue", SourceOwnerSystem = D14AftercareIds.MedicalOwner, SourceEventId = "medical_source", RescuedPrisonerIds = new List<string>(request.PrisonerIds) }; }
            public D14InterrogationDecision Interrogate(D14InterrogationRequest request, D14PrisonerRecord prisoner, FactEvent aftercareFact) { InterrogationCalls++; return new D14InterrogationDecision { Allowed = true, Code = "intel_recorded", Message = "fixture intelligence", SourceOwnerSystem = D14AftercareIds.LegalOwner, SourceEventId = "interrogation_source", InformationRef = "intel_windward_route" }; }
            public D14FuneralDecision Hold(D14FuneralRequest request, IReadOnlyList<D14CasualtyRecord> casualties, FactEvent aftercareFact) { return new D14FuneralDecision { Allowed = true, Code = "funeral_held", Message = "fixture funeral", Status = D14FuneralStatus.Held, SourceOwnerSystem = D14AftercareIds.FuneralOwner, SourceEventId = "funeral_source" }; }
            public D14RelationDecision Apply(D14RelationProposal proposal, FactEvent aftercareFact) { return new D14RelationDecision { Allowed = true, Code = "relation_applied", Message = "fixture relation", SourceOwnerSystem = D14AftercareIds.RelationOwner, SourceEventId = aftercareFact.EventId, AppliedDelta = proposal.ProposedDelta }; }
        }
    }
}
