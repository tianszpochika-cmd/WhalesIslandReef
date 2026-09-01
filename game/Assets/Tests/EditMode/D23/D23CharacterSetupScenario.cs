using Whalebone.Systems.D20;

namespace Whalebone.Tests.D23
{
    public static class D23CharacterSetupScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var presence = fixture.PlayerPresence.Apply(new D20PlayerPresenceRequest
            {
                CommandId = "d23.character.presence",
                PlayerId = fixture.PlayerId,
                DesiredState = D20PlayerPresenceState.Port,
                Reason = D20PlayerPresenceTransitionReason.EnterPort,
                PortId = fixture.PortId,
                WorldDay = fixture.WorldDay,
                SaveRevision = fixture.SaveRevision,
            });
            D23ScenarioAssertions.Check(ref assertions, presence.Accepted,
                "D23 character setup must accept the public D20 port presence command.");
            D23ScenarioAssertions.Check(ref assertions,
                presence.State != null && presence.State.State == D20PlayerPresenceState.Port,
                "D23 character setup must expose a port presence readback.");
            var restoredPresence = new D20PlayerPresenceSystem(presence.State);
            D23ScenarioAssertions.Check(ref assertions,
                restoredPresence.Snapshot.PlayerId == fixture.PlayerId,
                "D23 character setup presence must survive owner snapshot restore.");
            D23ScenarioAssertions.Check(ref assertions,
                D20CharacterIdentityContracts.ImmutableFieldsMatch(
                    fixture.PlayerIdentity, fixture.PlayerIdentity.Clone()),
                "D23 character identity immutable fields must remain stable.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
