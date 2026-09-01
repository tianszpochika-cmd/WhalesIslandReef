using Whalebone.Systems.D22;
using Whalebone.Systems.D21;

namespace Whalebone.Tests.D23
{
    public static class D23ContractCompatibilityTests
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var uiRequest = D22UiCommandRequest.Create(
                D22UiCommandKind.NewGame,
                "d23.contract.command",
                "d23.contract.correlation",
                fixture.PlayerId,
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                worldDay: fixture.WorldDay,
                saveRevision: fixture.SaveRevision);
            D23ScenarioAssertions.Check(ref assertions,
                D22UiCommandContracts.ValidateShape(uiRequest) == D22UiCommandValidationCode.None,
                "D23 UI DTO must satisfy the current D22 shape contract.");
            D23ScenarioAssertions.Check(ref assertions, fixture.Weather.TryValidate(out _),
                "D23 weather DTO must satisfy the current D21 contract.");
            D23ScenarioAssertions.Check(ref assertions,
                D21ShipIdentityContracts.Validate(fixture.ShipIdentity).Valid,
                "D23 ship DTO must satisfy the current D21 identity contract.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
