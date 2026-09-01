// D20-L33 纯逻辑保存/加载等价性测试。
// 外部 owner 的生态状态只以公开快照引用保存，不复制为第二事实源。

using System;
using Whalebone.Systems.D20;

namespace WhalesIslandReef.Tests.D20
{
    public static class D20CharacterSaveLoadTests
    {
        public static int Run()
        {
            int assertions = 0;
            const string playerId = "player_l33";
            const string npcId = "npc_harbor_clerk_001";
            const string animalId = "animal_l33_wild";

            var identity = D20CharacterIdentityContracts.CreateDefault(
                playerId, D20CharacterKind.Player, "character_captain_001", "origin:l33");

            var presenceSystem = new D20PlayerPresenceSystem();
            var presenceRequest = new D20PlayerPresenceRequest
            {
                CommandId = "presence_l33_001",
                PlayerId = playerId,
                DesiredState = D20PlayerPresenceState.Port,
                Reason = D20PlayerPresenceTransitionReason.EnterPort,
                PortId = "port_santa_margarita",
                LocationRef = "port:port_santa_margarita",
                WorldDay = 3,
                SaveRevision = 30,
            };
            var presenceResult = presenceSystem.Apply(presenceRequest);
            Check(ref assertions, presenceResult.Accepted, "presence must save after owner acceptance");

            var actionSystem = new D20ActionInterruptSystem(new D20ActionInterruptSnapshot
            {
                ActorId = playerId,
                PresenceState = D20PlayerPresenceState.Port,
                Availability = D20CharacterAvailability.Available,
                WorldDay = 3,
                SaveRevision = 30,
            });
            var action = D20ActionIntent.Create(D20ActionIntentKind.Walk,
                "action_l33_001", playerId, locationRef: "port:port_santa_margarita",
                worldDay: 3, saveRevision: 31);
            var actionResult = actionSystem.RegisterActiveAction(action);
            Check(ref assertions, actionResult.Accepted && actionResult.CurrentIntent != null,
                "active action must be present before save");

            var schedule = new D20NpcScheduleInput
            {
                NpcId = npcId,
                RoleId = "role:harbor_clerk",
                WorldDay = 3,
                TimeSlot = D20NpcTimeSlot.Morning,
                ScheduledDuty = D20NpcDutyKind.Trade,
                Weather = D20NpcWeatherKind.Clear,
                DangerLevel = D20NpcDangerLevel.None,
                Availability = D20CharacterAvailability.Available,
                IsWorkPeriod = true,
                IsBusy = false,
                IsRestRequired = false,
                IsInjured = false,
                ShelterAvailable = true,
                LocationRef = "port:port_santa_margarita",
                SaveRevision = 32,
            };
            schedule.Validate();
            var npcMachine = new D20NpcBehaviorStateMachine();
            var npcTransition = npcMachine.Tick(schedule);
            Check(ref assertions, npcTransition.Succeeded, "NPC schedule must produce behavior projection");
            var npcIdentity = D20NpcIdentityContracts.Migrate(
                "npc_harbor_clerk", "role:harbor_clerk", "faction:port",
                D20CharacterAvailability.Available, "character_npc_harbor_clerk");
            npcIdentity.Validate();

            var animalIdentity = D20AnimalIdentityContracts.CreateDefault(
                animalId, D20AnimalSpeciesCatalog.AgoutiForestContentId,
                D20AnimalSpeciesCatalog.AgoutiForestContentId,
                D20AnimalClassification.Terrestrial,
                D20AnimalPopulationKind.Wild);
            animalIdentity.Validate();
            var animalMachine = new D20AnimalBehaviorStateMachine(animalId,
                D20AnimalSpeciesCatalog.Find(D20AnimalSpeciesCatalog.AgoutiForestContentId));
            var animalTransition = animalMachine.Apply(D20AnimalBehaviorInput.Create(
                "animal_l33_001", animalId, D20AnimalBehaviorInputKind.Forage,
                worldDay: 3, saveRevision: 33));
            Check(ref assertions, animalTransition.Applied &&
                animalTransition.StateAfter == D20AnimalBehaviorState.Forage,
                "wild animal state must be present before save");

            var petOwnership = new D20PetOwnershipSnapshot();
            petOwnership.Records.Add(new D20PetOwnershipRecord
            {
                PetId = "pet_l33_001",
                OwnerId = playerId,
                AcquiredSaveRevision = 34,
                LastOwnershipEventId = "event:pet:l33",
            });
            petOwnership.Validate();

            var events = new D20CharacterSaveEventCursor
            {
                LastEventId = "event:d20:l33",
                LastEventSequence = 2,
                LastEventSaveRevision = 34,
            };
            events.AppliedEventKeys.Add("command:l33:presence");
            events.CauseEventIds.Add("event:root:l33");
            var upstreamWildlife = new D20SaveOwnerSnapshotReference
            {
                OwnerSystem = D20AnimalIdentityContracts.EcologyOwnerSystem,
                SnapshotId = "ecology:l33",
                SnapshotSchemaVersion = 1,
                Revision = 8,
                LastEventId = "event:ecology:l33",
            };
            var projection = D20CharacterSaveProjectionMapper.BuildProjection(
                playerId, 3, 34, identity, presenceSystem.CaptureSnapshot(),
                actionSystem.CaptureSnapshot(), npcIdentities: new[] { npcIdentity },
                npcBehaviors: new[] { npcMachine.CaptureSnapshot() },
                animalIdentities: new[] { animalIdentity },
                animalBehaviors: new[] { animalMachine.CaptureSnapshot() },
                petOwnership: petOwnership, events: events,
                upstreamSnapshots: new[] { upstreamWildlife },
                causeEventIds: new[] { "event:root:l33" });
            projection.Validate();

            var loaded = projection.Clone();
            loaded.Validate();
            Check(ref assertions, loaded.PlayerIdentity.CharacterId == playerId,
                "player identity must round-trip");
            Check(ref assertions, loaded.PlayerPresence.State == D20PlayerPresenceState.Port,
                "presence state must round-trip");
            Check(ref assertions, loaded.PlayerAction.ActiveIntent.ActionId == action.ActionId,
                "active action must round-trip");
            Check(ref assertions, loaded.NpcBehaviors[0].Behavior.RoleId == "role:harbor_clerk" &&
                loaded.NpcBehaviors[0].Behavior.StateSinceTimeSlot == D20NpcTimeSlot.Morning,
                "NPC schedule-derived projection must round-trip");
            Check(ref assertions, loaded.AnimalBehaviors[0].State == D20AnimalBehaviorState.Forage,
                "wild animal behavior state must round-trip");
            Check(ref assertions, loaded.PetOwnership.Records[0].OwnerId == playerId,
                "pet ownership mapping must round-trip");
            Check(ref assertions, loaded.UpstreamSnapshots[0].OwnerSystem ==
                D20AnimalIdentityContracts.EcologyOwnerSystem &&
                loaded.UpstreamSnapshots[0].Revision == 8,
                "wild status must remain an owner snapshot reference");
            Check(ref assertions, loaded.Events.AppliedEventKeys[0] == "command:l33:presence",
                "event cursor must round-trip");

            var restoredAction = new D20ActionInterruptSystem(loaded.PlayerAction);
            var restoredPresence = new D20PlayerPresenceSystem(loaded.PlayerPresence);
            var restoredAnimal = new D20AnimalBehaviorStateMachine(
                loaded.AnimalBehaviors[0].AnimalId,
                D20AnimalSpeciesCatalog.Find(loaded.AnimalIdentities[0].ContentId),
                loaded.AnimalBehaviors[0].State,
                loaded.AnimalBehaviors[0].FollowTargetId);
            restoredAnimal.Restore(loaded.AnimalBehaviors[0]);
            var restoredNpc = new D20NpcBehaviorStateMachine(
                loaded.NpcBehaviors[0].Behavior);
            Check(ref assertions, restoredAction.Snapshot.ActiveIntent.ActionId ==
                actionSystem.Snapshot.ActiveIntent.ActionId,
                "restored action snapshot must equal source");
            Check(ref assertions, restoredPresence.Snapshot.State == presenceSystem.Snapshot.State,
                "restored presence snapshot must equal source");
            Check(ref assertions, restoredAnimal.CaptureSnapshot().State ==
                animalMachine.CaptureSnapshot().State,
                "restored animal snapshot must equal source");
            Check(ref assertions, restoredNpc.CaptureSnapshot().State ==
                npcMachine.CaptureSnapshot().State,
                "restored NPC snapshot must equal source");

            return assertions;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
