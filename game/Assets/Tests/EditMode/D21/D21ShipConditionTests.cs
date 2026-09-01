// D21ShipConditionTests —— D21-L20 船舶身份、组件、船况与货舱纯逻辑测试
//
// 这些测试只调用 D21/D19 的公开纯 C# API，不启动 Unity 场景，不读取真实时间，
// 也不把 D3 货物数量写入口带入测试。每个失败路径都验证状态没有半提交。
// ============================================================

using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D21;

namespace Whalebone.Systems.D21.Tests
{
    [TestFixture]
    public sealed class D21ShipConditionTests
    {
        private const string ShipId = "ship_l20_01";

        [Test]
        public void ShipIdentity_CanonicalTypeAndDefaultProjectionValidate()
        {
            Assert.That(D21ShipTypeIds.CanonicalIds, Has.Count.EqualTo(20));
            Assert.That(D21ShipTypeIds.IsCanonical(D21ShipTypeIds.Sloop), Is.True);
            Assert.That(D21ShipTypeIds.IsCanonical(D21ShipTypeIds.LegacySmallFishing), Is.False);

            D21ShipIdentity identity = D21ShipIdentityContracts.CreateDefault(
                ShipId, D21ShipTypeIds.Sloop, "owner_l20", "Logic Test Ship");
            D21ShipIdentityValidationResult validation = identity.Validate();

            Assert.That(validation.Valid, Is.True);
            Assert.That(identity.ShipId, Is.EqualTo(ShipId));
            Assert.That(identity.ShipTypeId, Is.EqualTo(D21ShipTypeIds.Sloop));
            Assert.That(identity.ShipTypeContentId, Is.Empty);
            Assert.That(identity.OwnerId, Is.EqualTo("owner_l20"));

            string migratedType;
            Assert.That(D21ShipIdentityContracts.TryMapDeprecatedShipTypeId(
                D21ShipTypeIds.LegacySmallFishing, out migratedType), Is.True);
            Assert.That(migratedType, Is.EqualTo(D21ShipTypeIds.FishingBoat));
        }

        [Test]
        public void HullStarter_RequiredComponentsAndAtomicRejectionAreEnforced()
        {
            D21HullComponentConfiguration starter =
                D21HullComponentSystem.CreateStarterConfiguration(ShipId, D21ShipTypeIds.Sloop);
            D21HullComponentValidationResult starterValidation =
                D21HullComponentSystem.ValidateConfiguration(starter);

            Assert.That(starterValidation.Valid, Is.True);
            Assert.That(starter.Components, Has.Count.EqualTo(6));

            var system = new D21HullComponentSystem(starter);
            D21HullComponentConfiguration before = system.Snapshot;

            D21HullComponentResult removeRequired = system.Remove("l20-remove-hull", "hull.main", 1);
            Assert.That(removeRequired.Accepted, Is.False);
            Assert.That(removeRequired.ReasonCode, Is.EqualTo("hull_primary_count_invalid"));
            Assert.That(system.Snapshot.SaveRevision, Is.EqualTo(before.SaveRevision));
            Assert.That(system.Snapshot.Components, Has.Count.EqualTo(before.Components.Count));

            var rigging = new D21HullComponentState
            {
                ComponentId = "rigging.l20",
                Kind = D21HullComponentKind.Rigging,
                OperationalState = D21HullComponentOperationalState.Available,
            };
            D21HullComponentResult install = system.Install("l20-install-rigging", rigging, 1);
            Assert.That(install.Accepted, Is.True);
            Assert.That(system.Snapshot.SaveRevision, Is.EqualTo(1));

            D21HullComponentResult disableRudder = system.Apply(new D21HullComponentCommand
            {
                CommandId = "l20-disable-rudder",
                Kind = D21HullComponentCommandKind.SetOperationalState,
                ShipId = ShipId,
                ComponentId = "rudder.primary",
                NextOperationalState = D21HullComponentOperationalState.Disabled,
                WorldDay = 1,
                SaveRevision = 1,
                CauseEventId = "fact:l20:condition",
            });
            Assert.That(disableRudder.Accepted, Is.False);
            Assert.That(system.Snapshot.SaveRevision, Is.EqualTo(1));
            Assert.That(system.Snapshot.Components.Find(c => c.ComponentId == "rudder.primary").OperationalState,
                Is.EqualTo(D21HullComponentOperationalState.Available));
        }

        [Test]
        public void DamageRepair_RequiresOwnerFactsAndRestoresSeaworthinessAtomically()
        {
            D21ShipConditionState healthy = D21ShipConditionSystem.CreateHealthyState(ShipId, 100d, 50d);
            var system = new D21ShipConditionSystem(healthy);
            D21ShipDamageEntry structural = new D21ShipDamageEntry
            {
                DamageId = "damage:l20:structural",
                Kind = D21ShipDamageKind.Structural,
                ComponentId = "hull.main",
                Severity = D21ShipDamageSeverity.Critical,
                Amount = 80d,
                Active = true,
                SourceFactId = "fact:l20:combat",
                SourceKind = D21DamageSourceKind.CombatFact,
                WorldDay = 2,
                SaveRevision = 0,
            };

            D21ShipConditionResult damage = system.ApplyDamage(
                "l20-apply-damage", ShipId, structural, 0, "event:l20:combat");
            Assert.That(damage.Accepted, Is.True);
            Assert.That(damage.State.Durability, Is.EqualTo(20d));
            Assert.That(damage.State.Seaworthiness, Is.EqualTo(D21ShipSeaworthiness.Disabled));
            Assert.That(damage.State.DamageEntries, Has.Count.EqualTo(1));

            D21ShipConditionState beforeInvalidRepair = system.Snapshot();
            D21ShipConditionResult invalidRepair = system.TryRepair(
                "l20-invalid-repair",
                ShipId,
                D21RepairKind.StructuralRepair,
                20d,
                new D21ShipRepairContext
                {
                    Location = D21RepairLocation.Onboard,
                    OwnerPermission = true,
                    HasQualifiedCrew = true,
                    HasMaterials = true,
                    CanRepairAtSea = false,
                },
                beforeInvalidRepair.SaveRevision,
                "event:l20:invalid-repair");
            Assert.That(invalidRepair.Accepted, Is.False);
            Assert.That(system.Snapshot().SaveRevision, Is.EqualTo(beforeInvalidRepair.SaveRevision));
            Assert.That(system.Snapshot().Durability, Is.EqualTo(beforeInvalidRepair.Durability));

            D21ShipConditionResult repaired = system.TryRepair(
                "l20-full-dock-repair",
                ShipId,
                D21RepairKind.FullDockRepair,
                1d,
                QualifiedShipyardRepair(),
                beforeInvalidRepair.SaveRevision,
                "event:l20:repair");
            Assert.That(repaired.Accepted, Is.True);
            Assert.That(repaired.State.Durability, Is.EqualTo(repaired.State.MaxDurability));
            Assert.That(repaired.State.Flooding, Is.EqualTo(0d));
            Assert.That(repaired.State.TiltDegrees, Is.EqualTo(0d));
            Assert.That(repaired.State.DamageEntries[0].Active, Is.False);
            Assert.That(repaired.State.Seaworthiness, Is.EqualTo(D21ShipSeaworthiness.Navigable));
        }

        [Test]
        public void DamageBoundaries_ClampFloodingAndPreventNegativeDurability()
        {
            var floodingSystem = new D21ShipConditionSystem(
                D21ShipConditionSystem.CreateHealthyState(ShipId, 100d, 50d));
            D21ShipConditionResult flooding = floodingSystem.ApplyDamage(
                "l20-flooding", ShipId, new D21ShipDamageEntry
                {
                    DamageId = "damage:l20:flooding",
                    Kind = D21ShipDamageKind.Flooding,
                    Severity = D21ShipDamageSeverity.Major,
                    Amount = 100d,
                    Active = true,
                    SourceFactId = "fact:l20:storm",
                    SourceKind = D21DamageSourceKind.WeatherFact,
                    WorldDay = 2,
                }, 0, "event:l20:storm");
            Assert.That(flooding.Accepted, Is.True);
            Assert.That(flooding.State.Flooding, Is.EqualTo(50d));
            Assert.That(flooding.State.Seaworthiness, Is.EqualTo(D21ShipSeaworthiness.Sinking));

            var structuralSystem = new D21ShipConditionSystem(
                D21ShipConditionSystem.CreateHealthyState(ShipId, 100d, 50d));
            D21ShipConditionResult lost = structuralSystem.ApplyDamage(
                "l20-overkill", ShipId, new D21ShipDamageEntry
                {
                    DamageId = "damage:l20:overkill",
                    Kind = D21ShipDamageKind.Structural,
                    Severity = D21ShipDamageSeverity.Critical,
                    Amount = 1000d,
                    Active = true,
                    SourceFactId = "fact:l20:grounding",
                    SourceKind = D21DamageSourceKind.GroundingFact,
                    WorldDay = 2,
                }, 0, "event:l20:grounding");
            Assert.That(lost.Accepted, Is.True);
            Assert.That(lost.State.Durability, Is.EqualTo(0d));
            Assert.That(lost.State.Seaworthiness, Is.EqualTo(D21ShipSeaworthiness.Lost));
        }

        [Test]
        public void CargoLoad_CapacityAndDangerRulesProduceOwnerOnlyDecision()
        {
            var snapshot = new D21CargoCapacitySnapshot
            {
                ShipId = ShipId,
                MaxWeight = 100d,
                MaxVolume = 100d,
                SaveRevision = 7,
                ExistingLines = new List<D21CargoLineSnapshot>
                {
                    Cargo("timber", 4, 5d, 4d, D21CargoDangerClass.None, ""),
                },
                StorageContext = new D21CargoStorageContext
                {
                    HasFireSeparation = true,
                    HasPowderMagazine = true,
                    HasVentilation = true,
                    HasLiveHold = true,
                },
            };
            D21CargoCapacitySnapshot before = snapshot.Clone();

            var request = new D21CargoCapacityEvaluationRequest
            {
                Snapshot = snapshot,
                Candidate = Cargo("water", 2, 5d, 6d, D21CargoDangerClass.None, ""),
                MutationKind = D21CargoMutationKind.Add,
                RequestId = "l20-cargo-load",
                CauseEventId = "event:l20:load",
            };
            D21CargoCapacityDecision allowed = D21CargoCapacityRules.Evaluate(request);
            Assert.That(allowed.Valid, Is.True);
            Assert.That(allowed.Allowed, Is.True);
            Assert.That(allowed.ProjectedWeight, Is.EqualTo(30d));
            Assert.That(allowed.ProjectedVolume, Is.EqualTo(28d));
            Assert.That(snapshot.ExistingLines[0].Quantity, Is.EqualTo(before.ExistingLines[0].Quantity));

            D21CargoMutationRequest ownerRequest;
            string ownerError;
            Assert.That(D21CargoCapacityRules.TryBuildOwnerMutationRequest(
                request, out ownerRequest, out ownerError), Is.True);
            Assert.That(ownerError, Is.Null);
            Assert.That(ownerRequest.ShipId, Is.EqualTo(ShipId));
            Assert.That(ownerRequest.ExpectedSaveRevision, Is.EqualTo(7));
            Assert.That(ownerRequest.Line.ItemId, Is.EqualTo("water"));

            D21CargoCapacityDecision overloaded = D21CargoCapacityRules.Evaluate(new D21CargoCapacityEvaluationRequest
            {
                Snapshot = snapshot,
                Candidate = Cargo("stone", 30, 5d, 5d, D21CargoDangerClass.None, ""),
                MutationKind = D21CargoMutationKind.Add,
                RequestId = "l20-cargo-overload",
                CauseEventId = "event:l20:overload",
            });
            Assert.That(overloaded.Valid, Is.True);
            Assert.That(overloaded.Allowed, Is.False);
            Assert.That(overloaded.IsOverloaded, Is.True);
            Assert.That(overloaded.DepartureAllowed, Is.False);
            Assert.That(overloaded.ReasonCode, Is.EqualTo(D21CargoCapacityFailureCode.CapacityExceeded));
            Assert.That(snapshot.ExistingLines[0].Quantity, Is.EqualTo(4));

            D21CargoCapacityDecision dangerous = D21CargoCapacityRules.Evaluate(new D21CargoCapacityEvaluationRequest
            {
                Snapshot = new D21CargoCapacitySnapshot
                {
                    ShipId = ShipId,
                    MaxWeight = 100d,
                    MaxVolume = 100d,
                    StorageContext = new D21CargoStorageContext(),
                },
                Candidate = Cargo("powder", 1, 1d, 1d, D21CargoDangerClass.Explosive, "hold:l20"),
                MutationKind = D21CargoMutationKind.Add,
                RequestId = "l20-cargo-danger",
                CauseEventId = "event:l20:danger",
            });
            Assert.That(dangerous.Valid, Is.True);
            Assert.That(dangerous.Allowed, Is.False);
            Assert.That(dangerous.ReasonCode, Is.EqualTo(D21CargoCapacityFailureCode.DangerousGoodsConflict));
            Assert.That(dangerous.DangerousGoodsConflicts, Is.Not.Empty);
        }

        private static D21ShipRepairContext QualifiedShipyardRepair()
        {
            return new D21ShipRepairContext
            {
                Location = D21RepairLocation.Shipyard,
                IsAtPort = true,
                IsMoored = true,
                OwnerPermission = true,
                HasQualifiedCrew = true,
                HasMaterials = true,
                CauseFactId = "fact:l20:maintenance",
            };
        }

        private static D21CargoLineSnapshot Cargo(
            string itemId,
            int quantity,
            double unitWeight,
            double unitVolume,
            D21CargoDangerClass danger,
            string compartment)
        {
            return new D21CargoLineSnapshot
            {
                ItemId = itemId,
                Quantity = quantity,
                UnitWeight = unitWeight,
                UnitVolume = unitVolume,
                DangerClass = danger,
                StorageCompartmentId = compartment,
            };
        }
    }
}
