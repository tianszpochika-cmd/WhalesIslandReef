using Whalebone.Systems.D22;

namespace Whalebone.Tests.D23
{
    public static class D23UiCommandScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var request = D22UiCommandRequest.Create(
                D22UiCommandKind.NewGame,
                "d23.ui.new-game",
                "d23.ui.correlation",
                fixture.PlayerId,
                D22ScreenIds.MainMenu,
                D22PanelIds.MainMenu,
                worldDay: fixture.WorldDay,
                saveRevision: fixture.SaveRevision);
            var validation = D22UiCommandValidator.Validate(request, fixture.UiContext);
            D23ScenarioAssertions.Check(ref assertions, validation.Valid,
                "D23 UI command must validate through the D22 public command envelope.");
            D23ScenarioAssertions.Check(ref assertions,
                validation.Code == D22UiCommandValidationCode.None,
                "D23 UI command must expose a stable no-error validation code.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
