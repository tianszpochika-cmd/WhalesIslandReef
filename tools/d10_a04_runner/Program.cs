using System;
using System.IO;
using System.Runtime.InteropServices;
using D10.Tests;

internal static class Program
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string path);

    private static int Main()
    {
        var sqliteDirectory = Path.Combine(Environment.CurrentDirectory, "game", "Assets", "Plugins", "x86_64");
        SetDllDirectory(sqliteDirectory);
        var type = typeof(D10PlayerIdentityTests);
        var names = new[]
        {
            "NewGame_PersistsExpressionNameTitleAgeCultureAndLongText",
            "PortInspectionAndUpdate_ChangesTitleAppearanceAndDocumentWithoutErasingHistory",
            "PortUpdate_RejectsSeaAccessUnknownAppearanceAndInvalidDocumentWithoutPartialWrite",
            "SaveD10Identity_RestoresHistoryAndKeepsDomainReferences",
        };
        int failed = 0;
        foreach (var name in names)
        {
            var test = Activator.CreateInstance(type);
            try
            {
                type.GetMethod("SetUp").Invoke(test, null);
                type.GetMethod(name).Invoke(test, null);
                Console.WriteLine("[OK] " + name);
            }
            catch (Exception ex)
            {
                failed++;
                Console.WriteLine("[FAIL] " + name + ": " + (ex.InnerException ?? ex).Message);
            }
            finally
            {
                try { type.GetMethod("TearDown").Invoke(test, null); } catch { }
            }
        }
        Console.WriteLine(failed == 0 ? "D10-A04 tests PASS count=4" : "D10-A04 tests FAIL count=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
