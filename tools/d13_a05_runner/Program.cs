using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Save.Tests;

internal static class Program
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string path);

    private static int Main()
    {
        SetDllDirectory(Path.Combine(Environment.CurrentDirectory, "game", "Assets", "Plugins", "x86_64"));
        var type = typeof(D13IslandGovernanceSystemTests);
        var names = new[]
        {
            "PopulationGovernanceAndSaveRestoreKeepAuthoritySourcesAndIdempotency",
            "ShortageAndDeterministicSecurityIncidentChangeHealthAndCanRecover",
            "HousingJobAndAuthorityBoundariesPreventUnownedWrites",
        };
        int failed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex));
            }
            finally
            {
                try { type.GetMethod("TearDown", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null); }
                catch (Exception ex) { Console.WriteLine("[CLEANUP] " + (ex.InnerException ?? ex)); }
            }
        }
        Console.WriteLine(failed == 0 ? "D13-A05 tests PASS count=3" : "D13-A05 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
