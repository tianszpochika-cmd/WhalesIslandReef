using System;
using D11.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D11VegetationHarvestTests);
        var names = new[]
        {
            "CatalogCoversEightResourceKindsAndRequiredTools",
            "IdentificationIsDeterministicAndWrongToolDoesNotTouchEcology",
            "EachResourceKindHasAWorkingMaturityAndToolPath",
            "MisidentifiedToxinProducesTraceableConsequence",
            "ProtectedAreaRejectsHarvestAndLeavesResourceUntouched",
            "EventAndEcologySnapshotPreventDuplicateCargoAfterRestore",
        };
        int failed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod(name).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex).Message);
            }
        }
        Console.WriteLine(failed == 0 ? "D11-A07 tests PASS count=6" :
            "D11-A07 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
