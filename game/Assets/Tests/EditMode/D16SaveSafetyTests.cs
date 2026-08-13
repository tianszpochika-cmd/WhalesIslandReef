using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Threading;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Save.Tests
{
    public sealed class D16SaveSafetyTests
    {
        private string _root;
        private string _source;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "wb_d16_save_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _source = Path.Combine(_root, "save.db");
            SaveSystem.DbPath = _source;
            SaveFileSafety.TestAfterTemporaryFlushHook = null;
            SaveFileSafety.TestAfterAtomicReplaceHook = null;
            SaveFileSafety.TestBeforeRestoreSidecarHook = null;
            SaveMigration.TestProcessAbortHook = null;
            SaveSystem.TestTransactionBoundaryHook = null;
            GameState.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SaveFileSafety.TestAfterTemporaryFlushHook = null;
            SaveFileSafety.TestAfterAtomicReplaceHook = null;
            SaveFileSafety.TestBeforeRestoreSidecarHook = null;
            SaveMigration.TestProcessAbortHook = null;
            SaveSystem.TestTransactionBoundaryHook = null;
            SaveSystem.DbPath = null;
            GameState.Reset();
            if (Directory.Exists(_root))
            {
                try { Directory.Delete(_root, true); } catch { }
            }
        }

        /// <summary>
        /// 由 D16-A02 独立子进程调用：在临时文件 flush 完成、原子替换尚未开始时退出。
        /// 父进程随后检查旧文件仍完整、临时文件可诊断且不会被误当成主档。
        /// </summary>
        public static void AbortAfterAtomicWriteFlush(string path)
        {
            SaveFileSafety.TestAfterTemporaryFlushHook = _ => Environment.Exit(73);
            SaveFileSafety.WriteBytesAtomically(path, new byte[] { 0x6E, 0x65, 0x77, 0x2D, 0x66, 0x69, 0x6C, 0x65 });
        }

        /// <summary>由独立子进程调用：原子替换完成后立即退出，确认目标文件已是完整新版本。</summary>
        public static void AbortAfterAtomicWriteReplace(string path)
        {
            SaveFileSafety.TestAfterAtomicReplaceHook = _ => Environment.Exit(74);
            SaveFileSafety.WriteBytesAtomically(path, new byte[] { 0x6E, 0x65, 0x77, 0x2D, 0x72, 0x65, 0x70, 0x6C, 0x61, 0x63, 0x65 });
        }

        /// <summary>
        /// 由 D16-A02 父进程调用：到达临界点后写入 ready 标记并等待父进程强制终止。
        /// 这里不能主动退出，否则只能验证受控退出而不是操作系统终止后的文件状态。
        /// </summary>
        public static void ForceKillAfterAtomicWriteFlush(string path, string readyMarkerPath)
        {
            SaveFileSafety.TestAfterTemporaryFlushHook = _ => WaitForExternalForceTermination(readyMarkerPath);
            SaveFileSafety.WriteBytesAtomically(path, new byte[] { 0x6E, 0x65, 0x77, 0x2D, 0x66, 0x6F, 0x72, 0x63, 0x65 });
        }

        /// <summary>到达原子替换完成边界后等待父进程强制终止。</summary>
        public static void ForceKillAfterAtomicWriteReplace(string path, string readyMarkerPath)
        {
            SaveFileSafety.TestAfterAtomicReplaceHook = _ => WaitForExternalForceTermination(readyMarkerPath);
            SaveFileSafety.WriteBytesAtomically(path, new byte[] { 0x6E, 0x65, 0x77, 0x2D, 0x66, 0x6F, 0x72, 0x63, 0x65, 0x2D, 0x72, 0x65, 0x70, 0x6C, 0x61, 0x63, 0x65 });
        }

        /// <summary>
        /// 由 D16-A02 独立子进程调用：在 D8 v3 迁移提交前退出，模拟进程中止。
        /// 下一次进程打开数据库时应由 SQLite 回滚未提交事务，D7 表保持可读。
        /// </summary>
        public static void AbortDuringMigrationBeforeCommit(string path)
        {
            SaveSystem.DbPath = path;
            SaveSystem.NewGame(new NewGameParams { CaptainName = "进程中止验收船长" });
            // 与 SaveSystem.MigrateToV3 的文件级保护顺序一致：先建立备份，再进入可中止迁移。
            SaveFileSafety.CreateBackup(path);
            SaveMigration.TestProcessAbortHook = () => Environment.Exit(75);
            SaveMigration.Migrate(path, new SaveMigrationOptions
            {
                FaultPoint = SaveMigrationFaultPoint.ProcessAbortBeforeCommit,
            });
        }

        /// <summary>迁移事务提交前等待父进程强制终止，验证 SQLite 下次打开时回滚未提交写入。</summary>
        public static void ForceKillDuringMigrationBeforeCommit(string path, string readyMarkerPath)
        {
            SaveSystem.DbPath = path;
            SaveSystem.NewGame(new NewGameParams { CaptainName = "强制终止验收船长" });
            SaveFileSafety.CreateBackup(path);
            SaveMigration.TestProcessAbortHook = () => WaitForExternalForceTermination(readyMarkerPath);
            SaveMigration.Migrate(path, new SaveMigrationOptions
            {
                FaultPoint = SaveMigrationFaultPoint.ProcessAbortBeforeCommit,
            });
        }

        /// <summary>
        /// 新建一个已保存的档案，修改玩家金钱后在 Save() 写入玩家表、但事务尚未提交时等待
        /// 父进程强制终止。下次打开数据库应看到旧金钱，而不是半写入的混合快照。
        /// </summary>
        public static void ForceKillDuringSaveAfterPlayer(string path, string readyMarkerPath)
        {
            SaveSystem.DbPath = path;
            SaveSystem.NewGame(new NewGameParams { CaptainName = "保存中断验收船长" });
            GameState.CurrentPlayer.Money = 777;
            SaveSystem.TestTransactionBoundaryHook = boundary =>
            {
                if (string.Equals(boundary, "after_player", StringComparison.Ordinal))
                    WaitForExternalForceTermination(readyMarkerPath);
            };
            SaveSystem.Save();
        }

        /// <summary>
        /// 在同一临时档上重复执行新游戏、保存、读取、迁移和重复迁移，
        /// 检查长线写入不会丢失船长事实，也不会重复生成 v3 投影。
        /// </summary>
        public static void RunLongRunArchiveStress(string path, int iterations)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A save path is required.", nameof(path));
            if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            SaveSystem.DbPath = path;
            for (int i = 0; i < iterations; i++)
            {
                string captain = "长线存档船长-" + i;
                int money = 500 + i;
                SaveSystem.NewGame(new NewGameParams { CaptainName = captain });
                GameState.CurrentPlayer.Money = money;
                SaveSystem.Save();
                AssertLoadedFact(captain, money, "save/load", i);

                var migrated = SaveSystem.MigrateToV3();
                if (migrated == null || migrated.Status != SaveMigrationStatus.Migrated)
                    throw new InvalidOperationException("iteration=" + i + " expected Migrated, got " + (migrated == null ? "null" : migrated.Status.ToString()));

                var repeated = SaveSystem.MigrateToV3();
                if (repeated == null || repeated.Status != SaveMigrationStatus.AlreadyMigrated)
                    throw new InvalidOperationException("iteration=" + i + " expected AlreadyMigrated, got " + (repeated == null ? "null" : repeated.Status.ToString()));

                GameState.Reset();
                if (!SaveSystem.Load())
                    throw new InvalidOperationException("iteration=" + i + " could not reload after migration.");
                AssertLoadedFact(captain, money, "migration/load", i);
            }
        }

        private static void AssertLoadedFact(string expectedCaptain, int expectedMoney, string phase, int iteration)
        {
            if (!SaveSystem.HasSave() || GameState.CurrentPlayer == null ||
                GameState.CurrentPlayer.Name != expectedCaptain || GameState.CurrentPlayer.Money != expectedMoney)
            {
                throw new InvalidOperationException(
                    "iteration=" + iteration + " phase=" + phase + " lost player fact: expected=" +
                    expectedCaptain + "/" + expectedMoney);
            }
        }

        private static void WaitForExternalForceTermination(string readyMarkerPath)
        {
            if (string.IsNullOrWhiteSpace(readyMarkerPath))
                throw new ArgumentException("A ready marker path is required.", nameof(readyMarkerPath));

            string directory = Path.GetDirectoryName(readyMarkerPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(readyMarkerPath, "ready");

            // 父进程看到标记后调用 Process.kill/TerminateProcess；标记不删除，
            // 因为删除会让子进程继续执行并把测试变成普通受控返回。
            while (File.Exists(readyMarkerPath))
                Thread.Sleep(10);
        }

        [Test]
        public void BackupAndRestoreKeepOriginalAndCreateRollback()
        {
            File.WriteAllText(_source, "valid-save-v1");
            var backup = SaveFileSafety.CreateBackup(_source);
            Assert.IsNotNull(backup);
            Assert.IsTrue(File.Exists(backup.BackupPath));
            Assert.AreEqual("valid-save-v1", File.ReadAllText(backup.BackupPath));

            File.WriteAllText(_source, "corrupted-write");
            var restored = SaveFileSafety.RestoreBackup(_source, backup.BackupPath);

            Assert.IsTrue(restored.SourceReplacedAtomically);
            Assert.AreEqual("valid-save-v1", File.ReadAllText(_source));
            Assert.IsNotNull(restored.RollbackPath);
            Assert.IsTrue(File.Exists(restored.RollbackPath));
            Assert.AreEqual("corrupted-write", File.ReadAllText(restored.RollbackPath));
        }

        [Test]
        public void AtomicBytesWriteLeavesNoTemporaryFile()
        {
            SaveFileSafety.WriteBytesAtomically(_source, new byte[] { 1, 2, 3, 5, 8 });

            CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 5, 8 }, File.ReadAllBytes(_source));
            Assert.IsEmpty(Directory.GetFiles(_root, "save.db.tmp_*"));
        }

        [Test]
        public void RestoreFailureRollsBackMainFileAndSqliteSidecarsTogether()
        {
            string walPath = _source + "-wal";
            string shmPath = _source + "-shm";
            File.WriteAllText(_source, "original-main");
            File.WriteAllText(walPath, "original-wal");
            File.WriteAllText(shmPath, "original-shm");
            var backup = SaveFileSafety.CreateBackup(_source);
            Assert.IsNotNull(backup);

            File.WriteAllText(_source, "current-main-before-restore");
            File.WriteAllText(walPath, "current-wal-before-restore");
            File.WriteAllText(shmPath, "current-shm-before-restore");

            bool injected = false;
            SaveFileSafety.TestBeforeRestoreSidecarHook = _ =>
            {
                if (injected) return;
                injected = true;
                SaveFileSafety.TestBeforeRestoreSidecarHook = null;
                throw new IOException("simulated sidecar restore failure");
            };

            Assert.Throws<IOException>(() => SaveFileSafety.RestoreBackup(_source, backup.BackupPath));
            Assert.AreEqual("current-main-before-restore", File.ReadAllText(_source));
            Assert.AreEqual("current-wal-before-restore", File.ReadAllText(walPath));
            Assert.AreEqual("current-shm-before-restore", File.ReadAllText(shmPath));
            Assert.IsEmpty(Directory.GetFiles(_root, "save.db.tmp_*"));
        }

        [Test]
        public void StartupCleanupRemovesOnlyGeneratedOrphanTemporaryFiles()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "临时文件清理船长" });
            byte[] original = File.ReadAllBytes(_source);
            string generatedOrphan = _source + ".tmp_0123456789abcdef0123456789abcdef";
            string walOrphan = _source + "-wal.tmp_fedcba9876543210fedcba9876543210";
            string unknownTemporary = _source + ".tmp_not_a_generated_token";
            File.WriteAllBytes(generatedOrphan, new byte[] { 0x01, 0x02 });
            File.WriteAllBytes(walOrphan, new byte[] { 0x03, 0x04 });
            File.WriteAllBytes(unknownTemporary, new byte[] { 0x05, 0x06 });

            Assert.IsTrue(SaveSystem.HasSave(), "孤儿临时文件不应阻断可读存档检查。");
            Assert.IsFalse(File.Exists(generatedOrphan));
            Assert.IsFalse(File.Exists(walOrphan));
            Assert.IsTrue(File.Exists(unknownTemporary), "未知命名文件不得被宽泛清理。");
            CollectionAssert.AreEqual(original, File.ReadAllBytes(_source), "清理不得改写正式存档。");
        }

        [Test]
        public void NewGameCreatesBackupBeforeReplacingExistingSave()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "第一次船长" });
            SaveSystem.NewGame(new NewGameParams { CaptainName = "第二次船长" });

            var backups = SaveFileSafety.FindBackups(_source);
            Assert.IsNotEmpty(backups);
            Assert.IsTrue(SaveSystem.HasSave());
            Assert.AreEqual("第二次船长", GameState.CurrentPlayer.Name);
        }

        [Test]
        public void SaveFailureDoesNotOverwritePreviousValidFile()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "可恢复船长" });
            GameState.CurrentPlayer.Identity.PlayerId = "wrong-owner";

            Assert.Throws<InvalidOperationException>(() => SaveSystem.Save());

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load());
            Assert.AreEqual("可恢复船长", GameState.CurrentPlayer.Name);
        }

        [Test]
        public void SaveTransactionRollsBackWhenInterruptedAfterPlayerWrite()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "事务回滚船长" });
            GameState.CurrentPlayer.Money = 777;
            SaveSystem.TestTransactionBoundaryHook = boundary =>
            {
                if (string.Equals(boundary, "after_player", StringComparison.Ordinal))
                    throw new InvalidOperationException("simulated interruption after player write");
            };

            Assert.Throws<InvalidOperationException>(() => SaveSystem.Save());

            SaveSystem.TestTransactionBoundaryHook = null;
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load(), "事务中断后旧存档必须仍然可读。");
            Assert.AreEqual("事务回滚船长", GameState.CurrentPlayer.Name);
            Assert.AreEqual(500, GameState.CurrentPlayer.Money,
                "玩家表已写入但事务未提交时，不能留下半套新快照。");
            Assert.IsNotNull(GameState.CurrentShip);
            Assert.AreEqual(1, GameState.CrewAgents.Count);
        }

        [Test]
        public void CorruptedLiveDatabaseRestoresLatestValidArchiveAndLoads()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "可恢复 SQLite 船长" });
            SaveSystem.NewGame(new NewGameParams { CaptainName = "待损坏 SQLite 船长" });

            var backups = SaveFileSafety.FindBackups(_source);
            Assert.IsNotEmpty(backups, "覆盖真实 SQLite 存档前必须先生成备份。");

            File.WriteAllBytes(_source, new byte[] { 0x00, 0x01, 0x02, 0x03, 0xFF });
            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load(), "损坏主档存在有效备份时，读档入口必须自动恢复。");
            Assert.AreEqual("可恢复 SQLite 船长", GameState.CurrentPlayer.Name);

            var rollbackBackups = SaveFileSafety.FindBackups(_source);
            Assert.Greater(rollbackBackups.Count, backups.Count,
                "自动恢复前的损坏主档必须保留为新的回滚备份。");
        }

        [Test]
        public void MigrationExposesPreMigrationBackupAndRemainsIdempotent()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "迁移船长" });
            var first = SaveSystem.MigrateToV3();
            var second = SaveSystem.MigrateToV3();

            Assert.AreEqual(SaveMigrationStatus.Migrated, first.Status);
            Assert.IsNotNull(first.BackupPath);
            Assert.IsTrue(File.Exists(first.BackupPath));
            Assert.AreEqual(SaveMigrationStatus.AlreadyMigrated, second.Status);
            Assert.IsTrue(second.Succeeded);
        }

        [Test]
        public void SettlementClaimSurvivesSaveRoundTripAndCannotRepeat()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "结算幂等船长" });

            var log = new EventLog();
            var worldEvent = new WorldEvent(
                "d16_settlement_roundtrip",
                4,
                "saint_marys_port",
                "TradeEconomySystem",
                new EventResult { ResultCode = "trade_settled", Summary = "测试交易事实已记录" },
                saveRevision: 17);

            var appended = log.Append(worldEvent);
            Assert.IsFalse(appended.IsDuplicate);
            Assert.IsNotNull(appended.Fact);

            var firstClaim = log.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
            Assert.IsFalse(firstClaim.IsDuplicate);
            var immediateDuplicate = log.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
            Assert.IsTrue(immediateDuplicate.IsDuplicate);

            SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
            {
                CurrentLocationId = "saint_marys_port",
                SaveRevision = worldEvent.SaveRevision,
                EventLog = SaveSystem.CaptureD3TradeEventLog(log),
            });

            GameState.Reset();
            Assert.IsTrue(SaveSystem.Load(), "结算幂等测试档无法重新加载。");

            var restored = SaveSystem.RestoreD3TradeEventLog();
            Assert.AreEqual(1, restored.Count);
            var restoredDuplicate = restored.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
            Assert.IsTrue(restoredDuplicate.IsDuplicate, "读档后同一结算事实不能再次通过结算门。");
            var duplicateAppend = restored.Append(worldEvent);
            Assert.IsTrue(duplicateAppend.IsDuplicate, "读档后同一事实不能追加第二条日志。");
            Assert.AreEqual(1, restored.Count);
        }

        [Test]
        public void SettlementClaimsRemainIdempotentAcrossRepeatedSaveRoundTrips()
        {
            SaveSystem.NewGame(new NewGameParams { CaptainName = "结算压力船长" });

            var log = new EventLog();
            var events = new List<WorldEvent>();
            for (int i = 0; i < 24; i++)
            {
                var worldEvent = new WorldEvent(
                    "d16_settlement_stress_" + i,
                    20 + i,
                    "saint_marys_port",
                    "TradeEconomySystem",
                    new EventResult { ResultCode = "trade_settled", Summary = "结算压力事实已记录" },
                    saveRevision: 100 + i);

                var appended = log.Append(worldEvent);
                Assert.IsFalse(appended.IsDuplicate, "初次追加不应被视为重复：" + worldEvent.EventId);
                var claim = log.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
                Assert.IsFalse(claim.IsDuplicate, "初次结算声明不应被视为重复：" + worldEvent.EventId);
                events.Add(worldEvent);
            }

            for (int round = 0; round < 5; round++)
            {
                SaveSystem.SaveD3Runtime(new D3RuntimeSnapshot
                {
                    CurrentLocationId = "saint_marys_port",
                    SaveRevision = 123,
                    EventLog = SaveSystem.CaptureD3TradeEventLog(log),
                });

                GameState.Reset();
                Assert.IsTrue(SaveSystem.Load(), "结算压力轮次无法重新加载：" + round);

                var restored = SaveSystem.RestoreD3TradeEventLog();
                Assert.AreEqual(events.Count, restored.Count, "结算事实数量在轮次中变化：" + round);
                foreach (var worldEvent in events)
                {
                    var duplicateClaim = restored.ClaimSettlement(worldEvent.EventId, worldEvent.SaveRevision);
                    Assert.IsTrue(duplicateClaim.IsDuplicate,
                        "重复结算在轮次 " + round + " 未被拦截：" + worldEvent.EventId);
                    var duplicateAppend = restored.Append(worldEvent);
                    Assert.IsTrue(duplicateAppend.IsDuplicate,
                        "重复事实在轮次 " + round + " 未被去重：" + worldEvent.EventId);
                }

                log = restored;
            }
        }

        [Test]
        public void UnicodeAndSidecarBackupRestorePreservePortablePathSemantics()
        {
            string portableRoot = Path.Combine(_root, "船长档案 空格");
            Directory.CreateDirectory(portableRoot);
            string portableSource = Path.Combine(portableRoot, "save.db");
            string walPath = portableSource + "-wal";
            string shmPath = portableSource + "-shm";

            File.WriteAllText(portableSource, "portable-save-v1");
            File.WriteAllText(walPath, "portable-wal-v1");
            File.WriteAllText(shmPath, "portable-shm-v1");

            var backup = SaveFileSafety.CreateBackup(portableSource);
            Assert.IsNotNull(backup);
            Assert.IsTrue(backup.HasWalSidecar);
            Assert.IsTrue(backup.HasShmSidecar);
            Assert.AreEqual("portable-wal-v1", File.ReadAllText(backup.BackupPath + "-wal"));
            Assert.AreEqual("portable-shm-v1", File.ReadAllText(backup.BackupPath + "-shm"));

            File.WriteAllText(portableSource, "corrupted-portable-save");
            File.Delete(walPath);
            File.WriteAllText(shmPath, "stale-shm");

            var restored = SaveFileSafety.RestoreBackup(portableSource, backup.BackupPath);
            Assert.IsTrue(restored.SourceReplacedAtomically);
            Assert.AreEqual("portable-save-v1", File.ReadAllText(portableSource));
            Assert.AreEqual("portable-wal-v1", File.ReadAllText(walPath));
            Assert.AreEqual("portable-shm-v1", File.ReadAllText(shmPath));
            Assert.IsNotNull(restored.RollbackPath);
            Assert.IsTrue(File.Exists(restored.RollbackPath));
        }
    }
}
