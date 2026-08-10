using System;
using D12.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D12WreckCaveSalvageTests);
        var names = new[]
        {
            "TideAndEquipmentFailures_KeepRecoveryRoutesAndFacts",
            "CompetitorAndRiskChangeOutcome_WhileLegalAuthorityRemainsExternal",
            "CaveCollapse_RecordsLoss_ReturnsToEntrance_AndCanResume",
            "SaveRestore_PreservesSalvageAndDoesNotRegenerateRewardRequest",
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
        Console.WriteLine(failed == 0 ? "D12-A02 tests PASS count=4" : "D12-A02 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
