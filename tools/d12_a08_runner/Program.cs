using System;
using System.Reflection;
using Save.Tests;
using D12.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D12DynamicQuestLogTests);
        var names = new[]
        {
            "ObjectivesConsumeUnifiedFactsAndOnlyRequestReward",
            "FailureRecoveryAndAlternativeRouteRemainTraceable",
            "AgentSummaryAndJournalOnlyExposeEventLogFacts",
            "SaveRestorePreservesQuestJournalAndDuplicateProtection",
        };
        int failed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex).Message);
            }
        }
        Console.WriteLine(failed == 0 ? "D12-A08 tests PASS count=4" : "D12-A08 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
