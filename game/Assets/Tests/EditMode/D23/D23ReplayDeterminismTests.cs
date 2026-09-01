using System;

namespace Whalebone.Tests.D23
{
    public static class D23ReplayDeterminismTests
    {
        public const int ReplayCount = 20;

        public static int Run()
        {
            D23FullLoopReport baseline = D23FullLoopScenarioSupport.Run("replay");
            int assertions = baseline.Assertions;
            for (int index = 1; index < ReplayCount; index++)
            {
                D23FullLoopReport replay = D23FullLoopScenarioSupport.Run("replay");
                assertions += replay.Assertions;
                D23ScenarioAssertions.Check(ref assertions,
                    replay.FinalFingerprint == baseline.FinalFingerprint &&
                    replay.SaveFingerprint == baseline.SaveFingerprint &&
                    replay.EventFingerprint == baseline.EventFingerprint &&
                    replay.TickCount == baseline.TickCount &&
                    replay.D21EventCount == baseline.D21EventCount &&
                    replay.D19FactCount == baseline.D19FactCount,
                    "D23 replay " + index + " diverged from the baseline fingerprint.");
            }
            D23ScenarioAssertions.Check(ref assertions, ReplayCount >= 20,
                "D23 replay determinism must run at least 20 identical executions.");
            return assertions + 1;
        }
    }
}
