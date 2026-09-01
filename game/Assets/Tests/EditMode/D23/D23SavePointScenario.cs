using Whalebone.Systems.D20;
using Whalebone.Systems.D21;

namespace Whalebone.Tests.D23
{
    public static class D23SavePointScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            string json = fixture.ShipSave.ToJson();
            var restoredShip = D21ShipSaveProjection.FromJson(json);
            D23ScenarioAssertions.Check(ref assertions,
                restoredShip.ComputeFingerprint() == fixture.ShipSave.ComputeFingerprint(),
                "D23 ship save projection must round-trip deterministically.");
            var restoredPort = fixture.PortSave.Clone();
            restoredPort.Validate();
            D23ScenarioAssertions.Check(ref assertions, restoredPort.ShipId == fixture.ShipId,
                "D23 port save projection must preserve ship ownership reference.");
            var identityClone = fixture.PlayerIdentity.Clone();
            identityClone.Validate();
            D23ScenarioAssertions.Check(ref assertions,
                D20CharacterIdentityContracts.ImmutableFieldsMatch(fixture.PlayerIdentity, identityClone),
                "D23 character identity must survive save projection cloning.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
