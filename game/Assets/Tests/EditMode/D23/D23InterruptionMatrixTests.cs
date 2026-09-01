namespace Whalebone.Tests.D23
{
    public static class D23InterruptionMatrixTests
    {
        public static int Run()
        {
            int assertions = D23FullLoopScenarioSupport.RunInterruptionMatrix("interrupt");
            D23ScenarioAssertions.Check(ref assertions, assertions > 0,
                "D23 interruption matrix must execute real owner boundary assertions.");
            return assertions + 1;
        }
    }
}
