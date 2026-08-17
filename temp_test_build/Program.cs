using System;
using Whalebone.Systems.D19.Tests;

public class Program
{
    public static int Main()
    {
        Console.WriteLine("Executing D19LegacySaveTests...");
        var tester = new D19LegacySaveTests();

        try
        {
            Console.WriteLine("Running Test 1: LegacyFixture1_MissingD19Block_ReturnsSafeDefaultProjection...");
            tester.LegacyFixture1_MissingD19Block_ReturnsSafeDefaultProjection();
            Console.WriteLine("-> PASS 1");

            Console.WriteLine("Running Test 2: LegacyFixture2_DeprecatedAliases_MigratedToCanonicalWithHints...");
            tester.LegacyFixture2_DeprecatedAliases_MigratedToCanonicalWithHints();
            Console.WriteLine("-> PASS 2");

            Console.WriteLine("Running Test 3: LegacyFixture3_SchemaVersion0_UpgradedTo1WithDefaults...");
            tester.LegacyFixture3_SchemaVersion0_UpgradedTo1WithDefaults();
            Console.WriteLine("-> PASS 3");

            Console.WriteLine("Running Test 4: LegacyFixture4_FutureOrNegativeVersion_ExplicitlyRejected...");
            tester.LegacyFixture4_FutureOrNegativeVersion_ExplicitlyRejected();
            Console.WriteLine("-> PASS 4");

            Console.WriteLine("Running Test 5: LegacyFixture5_DataLossInBerthedState_RejectedWithoutForgingBerth...");
            tester.LegacyFixture5_DataLossInBerthedState_RejectedWithoutForgingBerth();
            Console.WriteLine("-> PASS 5");

            Console.WriteLine("Running Test 6: LegacyFixture6_CrossPortBerthConflict_Rejected...");
            tester.LegacyFixture6_CrossPortBerthConflict_Rejected();
            Console.WriteLine("-> PASS 6");

            Console.WriteLine("Running Test 7: LegacyFixture7_CorruptedImpossibleStates_Rejected...");
            tester.LegacyFixture7_CorruptedImpossibleStates_Rejected();
            Console.WriteLine("-> PASS 7");

            Console.WriteLine("Running Test 8: LegacyFixture8_MigrationIdempotencyAndNoSideEffects...");
            tester.LegacyFixture8_MigrationIdempotencyAndNoSideEffects();
            Console.WriteLine("-> PASS 8");

            Console.WriteLine("ALL 8 D19-L39 LEGACY SAVE COMPATIBILITY TESTS PASSED!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TEST FAILED: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
}
