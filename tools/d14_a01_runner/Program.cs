using System;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

using Save.Tests;

internal static class Program
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string path);

    private static int Main()
    {
        SetDllDirectory(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "game", "Assets", "Plugins", "x86_64")));
        var type = typeof(D14NavalCombatTests);
        var names = new[]
        {
            "KinematicsDamageFireFloodingAndSaveRestoreAreDeterministic",
            "AmmoKindsAndRetreatOrSurrenderProduceTraceableOutcomes",
        };
        int passed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex));
                return 1;
            }
            finally
            {
                try { type.GetMethod("TearDown", BindingFlags.Public | BindingFlags.Instance).Invoke(test, null); }
                catch (Exception ex) { Console.WriteLine("[CLEANUP] " + (ex.InnerException ?? ex)); }
            }
        }
        Console.WriteLine("D14-A01 tests PASS count=" + passed);
        return 0;
    }
}
