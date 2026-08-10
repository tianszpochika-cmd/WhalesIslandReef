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
        var type = typeof(D13AcceptanceIntegrationTests);
        var test = Activator.CreateInstance(type);
        try
        {
            type.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
            type.GetMethod("PropertyWellFarmMarketLoanInsuranceTaxAndSaveRestoreAreConsistent", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
            Console.WriteLine("[OK] PropertyWellFarmMarketLoanInsuranceTaxAndSaveRestoreAreConsistent");
            Console.WriteLine("D13-S01 tests PASS count=1");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FAIL] " + (ex.InnerException ?? ex));
            Console.WriteLine("D13-S01 tests FAIL count=1");
            return 1;
        }
        finally
        {
            try { type.GetMethod("TearDown", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null); }
            catch (Exception ex) { Console.WriteLine("[CLEANUP] " + (ex.InnerException ?? ex)); }
        }
    }
}
