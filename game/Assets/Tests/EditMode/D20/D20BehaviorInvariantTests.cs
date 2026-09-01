// D20-L35 纯逻辑行为边界与不变量测试。

using System;
using Whalebone.Systems.D19;
using Whalebone.Systems.D20;

namespace WhalesIslandReef.Tests.D20
{
    public static class D20BehaviorInvariantTests
    {
        public static int Run()
        {
            int assertions = 0;

            Check(ref assertions, D20NpcBehaviorAuthorityGuard.CanWrite(
                D20NpcBehaviorAuthorityDomain.BehaviorProjection,
                D20NpcBehaviorAuthorityGuard.BehaviorProjectionOwner),
                "behavior projection must have one D20 writer");
            Check(ref assertions, !D20NpcBehaviorAuthorityGuard.CanWrite(
                D20NpcBehaviorAuthorityDomain.Economy, "TradeEconomySystem"),
                "NPC behavior must not write economy facts");
            Check(ref assertions, !D20NpcBehaviorAuthorityGuard.CanWrite(
                D20NpcBehaviorAuthorityDomain.Relationship, "D10RelationSystem"),
                "NPC behavior must not write relationship facts");
            var relationFact = D20NpcBehaviorAuthorityGuard.Evaluate(
                D20NpcBehaviorAuthorityRequest.Create(
                    "npc_l35", D20NpcBehaviorAuthorityDomain.Relationship,
                    D20NpcBehaviorAuthorityOperation.WriteFact,
                    "D10RelationSystem"));
            Check(ref assertions, !relationFact.Allowed,
                "relationship write fact must be rejected");
            var economyProposal = D20NpcBehaviorAuthorityGuard.Evaluate(
                D20NpcBehaviorAuthorityRequest.Create(
                    "npc_l35", D20NpcBehaviorAuthorityDomain.Economy,
                    D20NpcBehaviorAuthorityOperation.SubmitOwnerRequest,
                    "TradeEconomySystem", true));
            Check(ref assertions, economyProposal.Allowed && economyProposal.IsProposal,
                "economy may only be reached as an owner proposal");
            var coreProposal = D20NpcBehaviorAuthorityGuard.Evaluate(
                D20NpcBehaviorAuthorityRequest.Create(
                    "npc_l35", D20NpcBehaviorAuthorityDomain.CoreAgent,
                    D20NpcBehaviorAuthorityOperation.SubmitOwnerRequest,
                    "AgentCore", true));
            Check(ref assertions, !coreProposal.Allowed,
                "AgentCore must not be an NPC behavior owner request");

            var animalService = new D20AnimalBehaviorService();
            var animalInput = D20AnimalBehaviorInput.Create(
                "animal_l35_command", "animal_l35", D20AnimalBehaviorInputKind.Forage,
                worldDay: 1, saveRevision: 1);
            var animalFirst = animalService.Apply(animalInput);
            var animalRevision = animalFirst.Projection.Behavior.ProjectionRevision;
            var animalDuplicate = animalService.Apply(animalInput.Clone());
            Check(ref assertions, animalFirst.Accepted && animalFirst.BehaviorResult.Applied,
                "first animal command must apply");
            Check(ref assertions, animalDuplicate.IsDuplicate && animalDuplicate.Succeeded,
                "same animal command must replay");
            Check(ref assertions, animalDuplicate.Projection.Behavior.ProjectionRevision ==
                animalRevision, "duplicate animal command must not mutate projection");
            var animalConflictInput = animalInput.Clone();
            animalConflictInput.Kind = D20AnimalBehaviorInputKind.Rest;
            var animalConflict = animalService.Apply(animalConflictInput);
            Check(ref assertions, !animalConflict.Accepted &&
                animalConflict.ReasonCode == "animal_behavior_command_id_conflict",
                "reused animal command ID with new input must be rejected");

            var deadAnimal = new D20AnimalBehaviorStateMachine("animal_l35_dead");
            var death = deadAnimal.Apply(D20AnimalBehaviorInput.Create(
                "animal_l35_death", "animal_l35_dead",
                D20AnimalBehaviorInputKind.Death, worldDay: 1, saveRevision: 2));
            var afterDeath = deadAnimal.Apply(D20AnimalBehaviorInput.Create(
                "animal_l35_after_death", "animal_l35_dead",
                D20AnimalBehaviorInputKind.Forage, worldDay: 1, saveRevision: 3));
            Check(ref assertions, death.Applied &&
                deadAnimal.CurrentState == D20AnimalBehaviorState.Dead,
                "death must enter terminal state");
            Check(ref assertions, !afterDeath.Applied &&
                deadAnimal.CurrentState == D20AnimalBehaviorState.Dead,
                "terminal animal must reject later behavior");

            var actionSystem = new D20ActionInterruptSystem(new D20ActionInterruptSnapshot
            {
                ActorId = "player_l35",
                PresenceState = D20PlayerPresenceState.Port,
                Availability = D20CharacterAvailability.Available,
            });
            var active = actionSystem.RegisterActiveAction(D20ActionIntent.Create(
                D20ActionIntentKind.Walk, "action_l35_active", "player_l35",
                locationRef: "port:port_santa_margarita"));
            var wrongCancel = actionSystem.Apply(D20ActionInterruptRequest.Cancel(
                "action_l35_cancel_wrong", "player_l35", "other_action"));
            Check(ref assertions, active.Accepted && !wrongCancel.Accepted &&
                actionSystem.Snapshot.ActiveIntent != null,
                "cancel must not remove another active command");
            var cancel = actionSystem.Apply(D20ActionInterruptRequest.Cancel(
                "action_l35_cancel", "player_l35", "action_l35_active"));
            Check(ref assertions, cancel.Accepted && actionSystem.Snapshot.ActiveIntent == null,
                "matching cancel must clear the action slot");

            var invalidMigration = D20NpcIdentityContracts.Migrate(
                "npc invalid id", "role:merchant", "faction:port",
                D20CharacterAvailability.Available, "character:npc_invalid");
            Check(ref assertions, invalidMigration.MigrationStatus ==
                D20NpcIdentityMigrationStatus.Unresolved &&
                !D20NpcIdentityContracts.IsReadyForRuntime(invalidMigration),
                "illegal NPC migration must remain unresolved");

            var visitSystem = new D19PortVisitSystem("ship_l35");
            var portRouter = new D19PortCommandRouter(
                D19PortAnchorCatalog.Instance, visitSystem, new D19BerthSystem(),
                new D19CommandIdempotency("ship_l35"));
            var portCommand = new D19RoutedPortCommand(
                "port_l35_enter", D19PortCommandKind.EnterPort, "player_l35",
                "ship_l35", "port_santa_margarita", worldDay: 1, saveRevision: 5,
                claims: new D19AuthorityClaims("ship_l35", true, true,
                    "port_santa_margarita"));
            var portService = new D20CharacterCommandService(portRouter: portRouter);
            var portResult = portService.Execute(
                D20CharacterCommand.ForPort("character_l35_port", portCommand));
            Check(ref assertions, portResult.PortResult != null &&
                portResult.PortResult.Success &&
                visitSystem.State == D19PortVisitState.PortVisit,
                "D20 port command must use D19 router and preserve D19 state owner");
            var missingRouter = new D20CharacterCommandService().Execute(
                D20CharacterCommand.ForPort("character_l35_missing_router", portCommand));
            Check(ref assertions, !missingRouter.Accepted &&
                missingRouter.FailureCode == D20CharacterCommandFailureCode.PortRouterMissing,
                "missing D19 router must fail closed");

            return assertions;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
