namespace Whalebone.Tests.D23
{
    public static class D23LongRunScenarioTests
    {
        public static int Run()
        {
            int assertions = D23FullLoopScenarioSupport.RunLongRun("long-run");
            D23ScenarioAssertions.Check(ref assertions, assertions > 50,
                "D23 long run must execute more than 50 assertions over 50 logic ticks.");
            return assertions + 1;
        }
    }
}
