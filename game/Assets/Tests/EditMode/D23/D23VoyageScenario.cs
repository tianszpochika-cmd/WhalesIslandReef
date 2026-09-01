using Whalebone.Systems.D21;

namespace Whalebone.Tests.D23
{
    public static class D23VoyageScenario
    {
        public static int Run()
        {
            var fixture = D23ScenarioFixtureFactory.Create();
            int assertions = 0;
            var first = D21SeaStateSystem.EvaluateDeterministic(fixture.Weather);
            var second = D21SeaStateSystem.EvaluateDeterministic(fixture.Weather.Clone());
            D23ScenarioAssertions.Check(ref assertions, first.Success && second.Success,
                "D23 voyage weather must be accepted by the D21 sea-state owner.");
            D23ScenarioAssertions.Check(ref assertions,
                first.InputFingerprint == second.InputFingerprint &&
                first.DeterministicEventSeed == second.DeterministicEventSeed,
                "D23 voyage weather resolution must be fixed-seed deterministic.");
            var tick = D23ScenarioAssertions.AdvanceOneTick(fixture);
            D23ScenarioAssertions.Check(ref assertions, tick.Accepted && tick.State != null,
                "D23 voyage fixed tick must return a state result.");
            D23ScenarioAssertions.RequireCrossSystemGate(fixture, ref assertions);
            return assertions;
        }
    }
}
