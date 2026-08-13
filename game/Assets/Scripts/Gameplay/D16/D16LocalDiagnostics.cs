using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// D16-A06 本地、可选择加入的运行时诊断记录器。
    /// 默认关闭；只写入独立诊断目录，不读取或覆盖存档，不上传外部服务。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D16LocalDiagnostics : MonoBehaviour
    {
        public const string ConsentKey = "D16.LocalDiagnostics.Consent";
        public const string DirectoryName = "D16Diagnostics";
        public const string ReportFileName = "runtime-diagnostics.ndjson";
        public const string RotatedReportFileName = "runtime-diagnostics.1.ndjson";
        public const string SessionMarkerFileName = "session.marker";
        public const string SessionMarkerToken = "d16_session_marker_v1";
        public const long MaxReportBytes = 512L * 1024L;
        private const int MaxPendingDiagnostics = 64;
        private const int MaxDrainPerFrame = 8;

        private static D16LocalDiagnostics _instance;
        private static volatile bool _consentCached;
        private readonly object _pendingLock = new object();
        private readonly Queue<PendingDiagnostic> _pendingDiagnostics = new Queue<PendingDiagnostic>();
        private bool _writing;
        private bool _sessionActive;

        public static bool HasConsent => _consentCached;

        public static string DiagnosticsDirectoryPath =>
            Path.Combine(Application.persistentDataPath, DirectoryName);

        public static string ReportPath =>
            Path.Combine(DiagnosticsDirectoryPath, ReportFileName);

        public static string RotatedReportPath =>
            Path.Combine(DiagnosticsDirectoryPath, RotatedReportFileName);

        public static string SessionMarkerPath =>
            Path.Combine(DiagnosticsDirectoryPath, SessionMarkerFileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (_instance != null) return;
            var existing = FindFirstObjectByType<D16LocalDiagnostics>(FindObjectsInactive.Include);
            if (existing != null)
            {
                _instance = existing;
                return;
            }
            var host = new GameObject("D16LocalDiagnostics");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<D16LocalDiagnostics>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _consentCached = PlayerPrefs.GetInt(ConsentKey, 0) == 1;
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += OnLogMessage;
            Application.logMessageReceivedThreaded += OnLogMessageThreaded;
            BeginSession();
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessage;
            Application.logMessageReceivedThreaded -= OnLogMessageThreaded;
            EndSession();
            ClearPendingDiagnostics();
            if (_instance == this) _instance = null;
        }

        private void OnApplicationQuit()
        {
            EndSession();
        }

        private void Update()
        {
            if (!HasConsent)
            {
                ClearPendingDiagnostics();
                return;
            }

            int drained = 0;
            while (drained < MaxDrainPerFrame)
            {
                PendingDiagnostic pending;
                lock (_pendingLock)
                {
                    if (_pendingDiagnostics.Count == 0) break;
                    pending = _pendingDiagnostics.Dequeue();
                }

                try
                {
                    if (HasConsent) WriteMetadata(pending);
                }
                catch
                {
                    // 磁盘权限、空间或路径异常不能反向影响主线程游戏循环。
                }
                drained++;
            }
        }

        /// <summary>由设置 UI 或发布前隐私确认页显式调用。</summary>
        public static void GrantConsent()
        {
            _consentCached = true;
            PlayerPrefs.SetInt(ConsentKey, 1);
            PlayerPrefs.Save();
            _instance?.BeginSession();
        }

        /// <summary>撤回同意但保留已有本地记录，便于用户先导出再删除。</summary>
        public static void RevokeConsent()
        {
            _consentCached = false;
            if (_instance != null)
            {
                _instance.EndSession();
                _instance.ClearPendingDiagnostics();
            }
            PlayerPrefs.DeleteKey(ConsentKey);
            PlayerPrefs.Save();
        }

        /// <summary>用户主动删除诊断数据；只操作本模块的两个固定文件。</summary>
        public static void ClearLocalReports()
        {
            DeleteExactFile(ReportPath);
            DeleteExactFile(RotatedReportPath);
            DeleteExactFile(SessionMarkerPath);
            if (_instance != null && HasConsent) WriteSessionMarker();
        }

        /// <summary>撤回同意并立即清理本模块已有诊断记录。</summary>
        public static void RevokeConsentAndClear()
        {
            RevokeConsent();
            ClearLocalReports();
        }

        private void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (!HasConsent || _writing) return;
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;

            _writing = true;
            try
            {
                WriteMetadata(CreateMetadata(condition, stackTrace, type));
            }
            catch
            {
                // 诊断系统不能因磁盘权限或空间不足再次制造错误，也不能影响存档。
            }
            finally
            {
                _writing = false;
            }
        }

        /// <summary>
        /// Unity 可能从后台线程调用该回调。这里只做纯 C# 的分类和长度统计，
        /// 不访问 Unity API、不读写文件，也不把原始错误文本放入队列。
        /// </summary>
        private void OnLogMessageThreaded(string condition, string stackTrace, LogType type)
        {
            if (!HasConsent || !IsSupportedType(type)) return;

            PendingDiagnostic pending = CreateMetadata(condition, stackTrace, type);
            lock (_pendingLock)
            {
                if (_pendingDiagnostics.Count >= MaxPendingDiagnostics) return;
                _pendingDiagnostics.Enqueue(pending);
            }
        }

        private static bool IsSupportedType(LogType type)
        {
            return type == LogType.Error || type == LogType.Exception || type == LogType.Assert;
        }

        private static PendingDiagnostic CreateMetadata(string condition, string stackTrace, LogType type)
        {
            return new PendingDiagnostic
            {
                utc = DateTime.UtcNow.ToString("O"),
                severity = type.ToString(),
                category = D16DiagnosticRecord.Classify(type.ToString(), condition),
                conditionLength = condition == null ? 0 : condition.Length,
                stackLineCount = CountLines(stackTrace),
            };
        }

        private void WriteMetadata(PendingDiagnostic pending)
        {
            string line = D16DiagnosticRecord.CreateJsonFromMetadata(
                pending.utc,
                pending.severity,
                pending.category,
                SceneManager.GetActiveScene().name,
                Application.unityVersion,
                Application.platform.ToString(),
                pending.conditionLength,
                pending.stackLineCount);
            AppendLine(line);
        }

        private void BeginSession()
        {
            if (_sessionActive || !HasConsent) return;

            try
            {
                if (File.Exists(SessionMarkerPath))
                {
                    WriteMetadata(new PendingDiagnostic
                    {
                        utc = DateTime.UtcNow.ToString("O"),
                        severity = "Session",
                        category = "unexpected_exit",
                        conditionLength = 0,
                        stackLineCount = 0,
                    });
                }

                WriteSessionMarker();
                _sessionActive = true;
            }
            catch
            {
                // 诊断会话标记失败不能阻塞启动，也不能影响存档或玩法循环。
            }
        }

        private void EndSession()
        {
            _sessionActive = false;
            DeleteExactFile(SessionMarkerPath);
        }

        private static void WriteSessionMarker()
        {
            Directory.CreateDirectory(DiagnosticsDirectoryPath);
            string temporaryPath = SessionMarkerPath + ".tmp_" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 128, true))
                {
                    writer.Write(SessionMarkerToken);
                    writer.Flush();
                    stream.Flush(true);
                }

                ReplaceFileAtomically(temporaryPath, SessionMarkerPath);
            }
            finally
            {
                DeleteExactFile(temporaryPath);
            }
        }

        private void ClearPendingDiagnostics()
        {
            lock (_pendingLock)
            {
                _pendingDiagnostics.Clear();
            }
        }

        private static int CountLines(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            int count = 1;
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\n') count++;
            return Math.Min(count, 4096);
        }

        private struct PendingDiagnostic
        {
            public string utc;
            public string severity;
            public string category;
            public int conditionLength;
            public int stackLineCount;
        }

        private static void AppendLine(string line)
        {
            Directory.CreateDirectory(DiagnosticsDirectoryPath);
            if (File.Exists(ReportPath) && new FileInfo(ReportPath).Length >= MaxReportBytes)
            {
                // 不要先删除旧轮转文件：进程中断或目标文件系统不支持替换时，
                // 原始报告必须仍然存在，下一次启动才能继续诊断。
                ReplaceFileAtomically(ReportPath, RotatedReportPath);
            }
            File.AppendAllText(ReportPath, line + Environment.NewLine, Encoding.UTF8);
        }

        /// <summary>
        /// 在同一目录内替换诊断文件。目标存在时使用原子替换，目标不存在时
        /// 使用同目录移动；任何失败都会保留旧目标，不先删除它。
        /// </summary>
        private static void ReplaceFileAtomically(string temporaryPath, string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(temporaryPath) || string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Diagnostic file paths are required.");

            if (File.Exists(destinationPath))
                File.Replace(temporaryPath, destinationPath, null, true);
            else
                File.Move(temporaryPath, destinationPath);
        }

        private static void DeleteExactFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // 删除失败不能影响游戏运行；下次用户可再次执行删除入口。
            }
        }
    }
}
