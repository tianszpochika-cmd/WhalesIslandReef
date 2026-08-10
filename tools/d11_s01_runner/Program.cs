using System;
using System.Reflection;

internal static class Program
{
    private static readonly string[] TestNames =
    {
        "WeatherAndTideChangeFishSchoolProjection",
        "FishingChangesFishPressureAndEconomyPriceInputs",
        "LiveFishHoldRestoreDoesNotDuplicateSettlement",
        "WildlifeRescueUpdatesCodexFactionAndQuestFact",
        "PetFactsEnterCrewAttachmentRelations",
        "VegetationHarvestEntersCargoAndChangesEcology",
        "AnimalBehaviorIsDeterministicWithoutLanguageModel",
        "EcologyAndAnimalSnapshotsRestoreWithFacts",
    };

    private static int Main()
    {
        var type = Type.GetType("D11.Tests.D11AcceptanceIntegrationTests, D11S01Runner");
        if (type == null) throw new InvalidOperationException("D11-S01 test type was not compiled.");
        int failed = 0;
        foreach (var name in TestNames)
        {
            object instance = Activator.CreateInstance(type);
            try
            {
                MethodInfo method = type.GetMethod(name);
                if (method == null) throw new InvalidOperationException("Missing test: " + name);
                method.Invoke(instance, null);
                Console.WriteLine("[OK] " + name);
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex).Message);
            }
        }
        Console.WriteLine(failed == 0
            ? "D11-S01 integration tests PASS count=8"
            : "D11-S01 integration tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
