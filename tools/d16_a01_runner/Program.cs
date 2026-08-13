using System;
using System.Reflection;
using Save.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D16PerformanceBudgetTests);
        var names = new[]
        {
            "Configuration_RejectsInvalidDistanceOrder",
            "ProtectedInteractionRemainsFullEvenUnderPressure",
            "AgentAndAnimalBudgetsAreStableAndCapped",
            "AudioBudgetDisablesOnlyUnprotectedSources",
            "InvalidAndDuplicateCandidatesAreNotAppliedTwice",
            "HighPressureEvaluationDoesNotMutateEventFactsOrSettlementClaims",
        };
        int passed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex));
                return 1;
            }
        }

        Console.WriteLine("D16-A01 pure budget tests PASS count=" + passed);
        return 0;
    }
}
