// D20-L34 纯逻辑固定种子长回放测试。
// 不使用系统时钟、外部随机源、Unity 或任何外部服务。

using System;
using Whalebone.Systems.D20;

namespace WhalesIslandReef.Tests.D20
{
    public static class D20DeterministicBehaviorTests
    {
        public static int Run()
        {
            int assertions = 0;
            const string animalId = "animal_l34_cat";
            D20AnimalSpeciesDefinition species = null;
            var first = new D20AnimalBehaviorStateMachine(animalId, species);
            var second = new D20AnimalBehaviorStateMachine(animalId, species);
            uint seed = 0x20D034u;
            const int ticks = 1000;
            int recoveryCount = 0;

            for (int tick = 0; tick < ticks; tick++)
            {
                D20AnimalBehaviorInputKind kind;
                string targetId = string.Empty;
                bool recoveryConfirmed = false;
                if (first.CurrentState == D20AnimalBehaviorState.Injured)
                {
                    kind = D20AnimalBehaviorInputKind.Recovery;
                    recoveryConfirmed = true;
                    recoveryCount++;
                }
                else if (tick % 37 == 0)
                {
                    kind = D20AnimalBehaviorInputKind.Injury;
                }
                else
                {
                    int choice = (int)(Next(ref seed) % 6u);
                    switch (first.CurrentState)
                    {
                        case D20AnimalBehaviorState.Rest:
                            kind = choice == 0
                                ? D20AnimalBehaviorInputKind.ThreatDetected
                                : choice == 1
                                    ? D20AnimalBehaviorInputKind.Follow
                                    : D20AnimalBehaviorInputKind.Forage;
                            targetId = kind == D20AnimalBehaviorInputKind.Follow
                                ? "npc_l34_target" : string.Empty;
                            break;
                        case D20AnimalBehaviorState.Forage:
                            kind = choice == 0
                                ? D20AnimalBehaviorInputKind.ThreatDetected
                                : D20AnimalBehaviorInputKind.ForageCompleted;
                            break;
                        case D20AnimalBehaviorState.Alert:
                            kind = choice == 0
                                ? D20AnimalBehaviorInputKind.Flee
                                : D20AnimalBehaviorInputKind.ThreatCleared;
                            break;
                        case D20AnimalBehaviorState.Flee:
                            kind = D20AnimalBehaviorInputKind.ThreatCleared;
                            break;
                        case D20AnimalBehaviorState.Follow:
                            kind = choice == 0
                                ? D20AnimalBehaviorInputKind.ThreatDetected
                                : D20AnimalBehaviorInputKind.FollowTargetLost;
                            break;
                        default:
                            kind = D20AnimalBehaviorInputKind.Rest;
                            break;
                    }
                }

                var input = D20AnimalBehaviorInput.Create(
                    "animal_l34_tick_" + tick, animalId, kind, targetId,
                    worldDay: tick / 4, saveRevision: tick);
                input.RecoveryConfirmed = recoveryConfirmed;
                var left = first.Apply(input);
                var right = second.Apply(input.Clone());
                Check(ref assertions, left.Applied && right.Applied,
                    "both deterministic behavior instances must apply tick " + tick);
                Check(ref assertions, left.StateAfter == right.StateAfter &&
                    left.FollowTargetId == right.FollowTargetId,
                    "fixed seed behavior must replay at tick " + tick);
                Check(ref assertions, first.CurrentState == second.CurrentState &&
                    first.ProjectionRevision == second.ProjectionRevision,
                    "behavior projections must remain equivalent at tick " + tick);
                Check(ref assertions, D20AnimalBehaviorStateMachine.IsSupportedState(
                    first.CurrentState), "behavior state must remain reachable at tick " + tick);
            }

            Check(ref assertions, recoveryCount > 0,
                "long replay must exercise an explicit recovery path");
            Check(ref assertions, first.CurrentState != D20AnimalBehaviorState.Dead &&
                second.CurrentState != D20AnimalBehaviorState.Dead,
                "test input must not create an unrecoverable terminal state");

            var actions = new D20ActionInterruptSystem(new D20ActionInterruptSnapshot
            {
                ActorId = "player_l34",
                PresenceState = D20PlayerPresenceState.Port,
                Availability = D20CharacterAvailability.Available,
            });
            for (int tick = 0; tick < ticks; tick++)
            {
                var intent = D20ActionIntent.Create(D20ActionIntentKind.Walk,
                    "action_l34_start_" + tick, "player_l34",
                    locationRef: "port:port_santa_margarita", worldDay: tick / 4,
                    saveRevision: tick * 2);
                var started = actions.RegisterActiveAction(intent);
                Check(ref assertions, started.Accepted && actions.Snapshot.ActiveIntent != null,
                    "action slot must be reusable at tick " + tick);
                var completed = actions.Complete("action_l34_complete_" + tick,
                    "player_l34", tick / 4, tick * 2 + 1);
                Check(ref assertions, completed.Accepted &&
                    actions.Snapshot.ActiveIntent == null,
                    "action slot must complete without a permanent lock at tick " + tick);
            }
            Check(ref assertions, actions.Snapshot.ActiveIntent == null,
                "long action replay must finish unlocked");
            Check(ref assertions, actions.Snapshot.SaveRevision == (ticks - 1) * 2 + 1,
                "action save revision must be deterministic");
            return assertions;
        }

        private static uint Next(ref uint state)
        {
            unchecked
            {
                state = state * 1664525u + 1013904223u;
            }
            return state;
        }

        private static void Check(ref int assertions, bool condition, string message)
        {
            assertions++;
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
