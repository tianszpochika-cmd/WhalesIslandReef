// ============================================================
// SaveFileSafety —— D16-A02 本地存档文件保护
//
// 归属：SaveSystem / 本地存储层。
// 职责：
//   1. 在覆盖写入或迁移前建立完整的文件级备份；
//   2. 用临时文件 + flush + 原子替换写入文件；
//   3. 从明确指定的备份恢复，并保留恢复前文件的回滚副本。
//
// 该类不读取或裁决任何玩法数值，不包含网络、LLM 或外部服务依赖。
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace Whalebone.Systems.Save
{
    public sealed class SaveFileBackupInfo
    {
        public string SourcePath { get; internal set; }
        public string BackupPath { get; internal set; }
        public string BackupDirectory { get; internal set; }
        public bool HasWalSidecar { get; internal set; }
        public bool HasShmSidecar { get; internal set; }
    }

    public sealed class SaveFileRestoreInfo
    {
        public string SourcePath { get; internal set; }
        public string RestoredFromPath { get; internal set; }
        public string RollbackPath { get; internal set; }
        public bool SourceReplacedAtomically { get; internal set; }
    }

    public static class SaveFileSafety
    {
        private const string BackupDirectorySuffix = ".backups";
        private const string BackupNamePrefix = "save_";

        // 仅供 Save.Tests 的独立故障注入进程使用；默认值为 null，生产路径不会触发。
        internal static Action<string> TestAfterTemporaryFlushHook { get; set; }
        internal static Action<string> TestAfterAtomicReplaceHook { get; set; }
        internal static Action<string> TestBeforeRestoreSidecarHook { get; set; }

        /// <summary>
        /// 在独立备份目录中创建数据库主文件及其 SQLite sidecar 的一致副本。
        /// 调用方应在数据库连接关闭后调用；本项目当前连接为短连接写入模式。
        /// </summary>
        public static SaveFileBackupInfo CreateBackup(string sourcePath, string backupDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Save path is required.", nameof(sourcePath));
            if (!File.Exists(sourcePath)) return null;

            var source = new FileInfo(sourcePath);
            if (source.Length == 0) return null;

            string directory = backupDirectory;
            if (string.IsNullOrWhiteSpace(directory)) directory = sourcePath + BackupDirectorySuffix;
            Directory.CreateDirectory(directory);

            string backupPath = Path.Combine(directory, BackupNamePrefix +
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + Guid.NewGuid().ToString("N") + ".db");
            CopyFileAtomically(sourcePath, backupPath);

            bool hasWal = CopySidecarIfPresent(sourcePath + "-wal", backupPath + "-wal");
            bool hasShm = CopySidecarIfPresent(sourcePath + "-shm", backupPath + "-shm");
            return new SaveFileBackupInfo
            {
                SourcePath = sourcePath,
                BackupPath = backupPath,
                BackupDirectory = directory,
                HasWalSidecar = hasWal,
                HasShmSidecar = hasShm,
            };
        }

        /// <summary>将 bytes 写入临时文件并原子替换目标文件。</summary>
        public static void WriteBytesAtomically(string destinationPath, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path is required.", nameof(destinationPath));
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            string temporaryPath = destinationPath + ".tmp_" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                TestAfterTemporaryFlushHook?.Invoke(temporaryPath);
                ReplaceAtomically(temporaryPath, destinationPath);
                TestAfterAtomicReplaceHook?.Invoke(destinationPath);
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }

        /// <summary>
        /// 清理本次存档路径下由原子写入留下的、可明确识别的孤儿临时文件。
        ///
        /// 进程可能在 Flush 完成后、原子替换前被操作系统终止；此时临时文件
        /// 无法进入 finally 清理。只匹配本存档主文件及 SQLite sidecar 的
        /// “.tmp_ + 32 位十六进制 GUID”命名，不触碰正式存档、备份或未知文件。
        /// 返回实际删除的文件数量；单个文件删除失败不会阻断读档。
        /// </summary>
        public static int CleanupTemporaryArtifacts(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return 0;

            string fullSourcePath = Path.GetFullPath(sourcePath);
            string directory = Path.GetDirectoryName(fullSourcePath);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return 0;

            int removed = 0;
            string fileName = Path.GetFileName(fullSourcePath);
            removed += CleanupTemporaryFiles(directory, fileName);
            removed += CleanupTemporaryFiles(directory, fileName + "-wal");
            removed += CleanupTemporaryFiles(directory, fileName + "-shm");
            return removed;
        }

        /// <summary>
        /// 用指定备份恢复主文件；恢复前主文件先另存为回滚副本，避免二次事故不可逆。
        /// </summary>
        public static SaveFileRestoreInfo RestoreBackup(string sourcePath, string backupPath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(backupPath))
                throw new ArgumentException("Source and backup paths are required.");
            if (!File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
                throw new FileNotFoundException("Backup file is missing or empty.", backupPath);

            string rollbackPath = null;
            bool hadSource = File.Exists(sourcePath);
            if (hadSource)
            {
                var rollback = CreateBackup(sourcePath);
                rollbackPath = rollback == null ? null : rollback.BackupPath;
            }

            try
            {
                CopyFileAtomically(backupPath, sourcePath);
                TestBeforeRestoreSidecarHook?.Invoke(sourcePath + "-wal");
                CopyOrDeleteSidecar(backupPath + "-wal", sourcePath + "-wal");
                TestBeforeRestoreSidecarHook?.Invoke(sourcePath + "-shm");
                CopyOrDeleteSidecar(backupPath + "-shm", sourcePath + "-shm");
                return new SaveFileRestoreInfo
                {
                    SourcePath = sourcePath,
                    RestoredFromPath = backupPath,
                    RollbackPath = rollbackPath,
                    SourceReplacedAtomically = true,
                };
            }
            catch (Exception restoreError)
            {
                // 主文件已完成替换后，sidecar 仍可能失败（权限、占用、跨平台
                // 文件系统限制）。恢复前的完整副本必须把三件文件一起回滚，
                // 否则 SQLite 可能看到主文件与 WAL/SHM 不匹配的中间状态。
                try
                {
                    if (!string.IsNullOrWhiteSpace(rollbackPath) && File.Exists(rollbackPath))
                    {
                        CopyFileAtomically(rollbackPath, sourcePath);
                        CopyOrDeleteSidecar(rollbackPath + "-wal", sourcePath + "-wal");
                        CopyOrDeleteSidecar(rollbackPath + "-shm", sourcePath + "-shm");
                    }
                    else if (!hadSource)
                    {
                        TryDelete(sourcePath);
                        TryDelete(sourcePath + "-wal");
                        TryDelete(sourcePath + "-shm");
                    }
                }
                catch (Exception rollbackError)
                {
                    throw new IOException(
                        "Save restore failed and rollback also failed; original backup remains available.",
                        new AggregateException(restoreError, rollbackError));
                }

                throw;
            }
        }

        public static IReadOnlyList<string> FindBackups(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return new List<string>();
            string directory = sourcePath + BackupDirectorySuffix;
            if (!Directory.Exists(directory)) return new List<string>();
            var result = new List<string>(Directory.GetFiles(directory, BackupNamePrefix + "*.db"));
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        private static void CopyFileAtomically(string sourcePath, string destinationPath)
        {
            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            string temporaryPath = destinationPath + ".tmp_" + Guid.NewGuid().ToString("N");
            try
            {
                using (var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var target = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    source.CopyTo(target);
                    target.Flush(true);
                }
                ReplaceAtomically(temporaryPath, destinationPath);
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }

        private static bool CopySidecarIfPresent(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath)) return false;
            CopyFileAtomically(sourcePath, destinationPath);
            return true;
        }

        private static void CopyOrDeleteSidecar(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                CopyFileAtomically(sourcePath, destinationPath);
                return;
            }
            TryDelete(destinationPath);
        }

        private static int CleanupTemporaryFiles(string directory, string destinationFileName)
        {
            string prefix = destinationFileName + ".tmp_";
            int removed = 0;
            string[] candidates;
            try
            {
                candidates = Directory.GetFiles(directory, prefix + "*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                return 0;
            }

            foreach (string candidate in candidates)
            {
                string candidateName = Path.GetFileName(candidate);
                if (!IsGeneratedTemporaryName(candidateName, prefix)) continue;
                try
                {
                    File.Delete(candidate);
                    removed++;
                }
                catch
                {
                    // 读档入口不能因清理一个被占用的孤儿临时文件而失败。
                }
            }
            return removed;
        }

        private static bool IsGeneratedTemporaryName(string candidateName, string prefix)
        {
            if (string.IsNullOrEmpty(candidateName) || !candidateName.StartsWith(prefix, StringComparison.Ordinal))
                return false;

            string token = candidateName.Substring(prefix.Length);
            if (token.Length != 32) return false;
            for (int i = 0; i < token.Length; i++)
            {
                char c = token[i];
                bool isHex = (c >= '0' && c <= '9') ||
                             (c >= 'a' && c <= 'f') ||
                             (c >= 'A' && c <= 'F');
                if (!isHex) return false;
            }
            return true;
        }

        private static void ReplaceAtomically(string temporaryPath, string destinationPath)
        {
            if (File.Exists(destinationPath))
            {
                File.Replace(temporaryPath, destinationPath, null, true);
            }
            else
            {
                File.Move(temporaryPath, destinationPath);
            }
        }

        private static void TryDelete(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try { File.Delete(path); } catch { /* 保留临时文件供下次启动诊断 */ }
        }
    }
}
