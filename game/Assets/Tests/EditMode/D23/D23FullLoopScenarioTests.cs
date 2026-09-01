using Whalebone.Tests.D23;

namespace Whalebone.Tests.D23
{
    public static class D23FullLoopScenarioTests
    {
        public static int Run()
        {
            D23FullLoopReport report = D23FullLoopScenarioSupport.Run("full-loop");
            D23ScenarioAssertions.Check(report.Assertions > 0,
                "D23 full loop must execute real assertions.");
            D23ScenarioAssertions.Check(report.TickCount == 40 && report.Arrived &&
                report.Returned && report.D19FactCount == report.SharedFactCount,
                "D23 full loop report must prove the full 40-tick return path.");
            return report.Assertions + 2;
        }
    }
}
