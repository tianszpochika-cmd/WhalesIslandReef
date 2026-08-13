using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Save.Tests;

internal static class Program
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetDllDirectory(string path);

    private static void ConfigureSqliteSearchPath()
    {
        // Windows 的 Unity/编辑器目录需要显式加入原生 sqlite 搜索路径。
        // Linux/macOS 不应调用 kernel32；让系统原生加载器按平台规则解析
        // libsqlite3，避免在跨平台验收开始前因 Windows 专用初始化崩溃。
        if (!OperatingSystem.IsWindows()) return;

        string sqliteDirectory = Path.GetFullPath(Path.Combine(
            Environment.CurrentDirectory, "game", "Assets", "Plugins", "x86_64"));
        if (Directory.Exists(sqliteDirectory))
            SetDllDirectory(sqliteDirectory);
    }

    private static int Main(string[] args)
    {
        ConfigureSqliteSearchPath();

        if (args != null && args.Length >= 2)
        {
            if (string.Equals(args[0], "--abort-after-flush", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.AbortAfterAtomicWriteFlush(args[1]);
                return 73;
            }
            if (string.Equals(args[0], "--abort-after-replace", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.AbortAfterAtomicWriteReplace(args[1]);
                return 74;
            }
            if (string.Equals(args[0], "--abort-during-migration", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.AbortDuringMigrationBeforeCommit(args[1]);
                return 75;
            }

            if (args.Length >= 3 && string.Equals(args[0], "--force-kill-after-flush", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.ForceKillAfterAtomicWriteFlush(args[1], args[2]);
                return 76;
            }
            if (args.Length >= 3 && string.Equals(args[0], "--force-kill-after-replace", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.ForceKillAfterAtomicWriteReplace(args[1], args[2]);
                return 76;
            }
            if (args.Length >= 3 && string.Equals(args[0], "--force-kill-during-migration", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.ForceKillDuringMigrationBeforeCommit(args[1], args[2]);
                return 76;
            }
            if (args.Length >= 3 && string.Equals(args[0], "--force-kill-during-save-after-player", StringComparison.Ordinal))
            {
                D16SaveSafetyTests.ForceKillDuringSaveAfterPlayer(args[1], args[2]);
                return 76;
            }
            if (args.Length >= 3 && string.Equals(args[0], "--long-run", StringComparison.Ordinal))
            {
                if (!int.TryParse(args[2], out int iterations))
                {
                    Console.Error.WriteLine("--long-run requires a positive integer iteration count.");
                    return 77;
                }
                D16SaveSafetyTests.RunLongRunArchiveStress(args[1], iterations);
                Console.WriteLine("D16-A02 long-run archive stress PASS iterations=" + iterations);
                return 0;
            }
        }

        var type = typeof(D16SaveSafetyTests);
        var names = new[]
        {
            "BackupAndRestoreKeepOriginalAndCreateRollback",
            "AtomicBytesWriteLeavesNoTemporaryFile",
            "StartupCleanupRemovesOnlyGeneratedOrphanTemporaryFiles",
            "NewGameCreatesBackupBeforeReplacingExistingSave",
            "SaveFailureDoesNotOverwritePreviousValidFile",
            "SaveTransactionRollsBackWhenInterruptedAfterPlayerWrite",
            "RestoreFailureRollsBackMainFileAndSqliteSidecarsTogether",
            "CorruptedLiveDatabaseRestoresLatestValidArchiveAndLoads",
            "MigrationExposesPreMigrationBackupAndRemainsIdempotent",
            "SettlementClaimSurvivesSaveRoundTripAndCannotRepeat",
            "SettlementClaimsRemainIdempotentAcrossRepeatedSaveRoundTrips",
            "UnicodeAndSidecarBackupRestorePreservePortablePathSemantics",
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

        Console.WriteLine("D16-A02 save safety tests PASS count=" + passed);
        return 0;
    }
}
