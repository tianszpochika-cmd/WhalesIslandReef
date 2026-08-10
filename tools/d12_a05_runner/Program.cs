using System;
using System.Reflection;
using Save.Tests;

internal static class Program
{
    private static int Main()
    {
        var type = typeof(D12PokerSystemTests);
        var names = new[]
        {
            "EnterBetResolve_UsesFixedDeck_AndRoutesMoneyRewardRelationAndInformation",
            "BetCapAndUnknownAction_AreRejectedWithoutFacts",
            "CheatRisk_ExposedEndsTheHand_AndProducesTraceableConsequences",
            "ExitAndDebt_CreateOwnerRequests_WithoutRefundOrDirectMoneyMutation",
            "SaveLoad_PreservesCardsAndClaimGate_AndDoesNotPayTwice",
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
        Console.WriteLine(failed == 0 ? "D12-A05 tests PASS count=5" : "D12-A05 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
