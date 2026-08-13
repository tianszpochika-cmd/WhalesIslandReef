using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Whalebone.Gameplay.D16;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D16
{
    public sealed class D16LocalDiagnosticsTests
    {
        [UnityTest]
        public IEnumerator DiagnosticsWritesOnlyAfterConsentAndDeleteRemovesLocalFile()
        {
            D16LocalDiagnostics.RevokeConsentAndClear();

            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16LocalDiagnostics diagnostics = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (diagnostics == null && Time.realtimeSinceStartup < deadline)
            {
                diagnostics = Object.FindFirstObjectByType<D16LocalDiagnostics>(FindObjectsInactive.Include);
                if (diagnostics == null) yield return null;
            }

            Assert.IsNotNull(diagnostics, "D16LocalDiagnostics 未创建。");
            Assert.IsFalse(D16LocalDiagnostics.HasConsent, "诊断记录必须默认关闭。");

            D16LocalDiagnostics.GrantConsent();
            var onLogMessage = typeof(D16LocalDiagnostics).GetMethod("OnLogMessage",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(onLogMessage, "本地诊断缺少日志接收入口。");
            onLogMessage.Invoke(diagnostics, new object[]
            {
                "NullReferenceException: secret player text C:\\Users\\Captain\\save.db",
                "at Secret.Namespace.Method()\nC:\\Users\\Captain\\stack.cs:10",
                LogType.Exception,
            });

            Assert.IsTrue(File.Exists(D16LocalDiagnostics.ReportPath), "同意后没有写入本地诊断文件。");
            string report = File.ReadAllText(D16LocalDiagnostics.ReportPath);
            StringAssert.Contains("\"raw_message_omitted\":true", report);
            StringAssert.Contains("\"raw_stack_omitted\":true", report);
            StringAssert.DoesNotContain("secret player text", report);
            StringAssert.DoesNotContain("Captain", report);
            StringAssert.DoesNotContain("Secret.Namespace", report);
            StringAssert.DoesNotContain("save.db", report);

            D16LocalDiagnostics.RevokeConsentAndClear();
            Assert.IsFalse(D16LocalDiagnostics.HasConsent, "撤回入口没有清除诊断同意。");
            Assert.IsFalse(File.Exists(D16LocalDiagnostics.ReportPath), "撤回并删除后诊断文件仍存在。");
            Assert.IsFalse(File.Exists(D16LocalDiagnostics.RotatedReportPath), "撤回并删除后轮转诊断文件仍存在。");
        }

        [UnityTest]
        public IEnumerator ThreadedDiagnosticsAreQueuedAndRedactedBeforeMainThreadWrite()
        {
            D16LocalDiagnostics.RevokeConsentAndClear();

            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16LocalDiagnostics diagnostics = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (diagnostics == null && Time.realtimeSinceStartup < deadline)
            {
                diagnostics = Object.FindFirstObjectByType<D16LocalDiagnostics>(FindObjectsInactive.Include);
                if (diagnostics == null) yield return null;
            }

            Assert.IsNotNull(diagnostics, "D16LocalDiagnostics 未创建。");
            D16LocalDiagnostics.GrantConsent();

            var threadedHandler = typeof(D16LocalDiagnostics).GetMethod("OnLogMessageThreaded",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(threadedHandler, "本地诊断缺少后台线程日志入口。");
            threadedHandler.Invoke(diagnostics, new object[]
            {
                "NullReferenceException: threaded secret player text C:\\Users\\Captain\\save.db",
                "at Threaded.Secret.Namespace.Method()\nC:\\Users\\Captain\\threaded-stack.cs:10",
                LogType.Exception,
            });

            yield return null;

            Assert.IsTrue(File.Exists(D16LocalDiagnostics.ReportPath), "后台线程诊断没有在主线程落盘。");
            string report = File.ReadAllText(D16LocalDiagnostics.ReportPath);
            StringAssert.Contains("\"raw_message_omitted\":true", report);
            StringAssert.Contains("\"raw_stack_omitted\":true", report);
            StringAssert.Contains("\"category\":\"null_reference\"", report);
            StringAssert.DoesNotContain("threaded secret player text", report);
            StringAssert.DoesNotContain("Threaded.Secret.Namespace", report);
            StringAssert.DoesNotContain("Captain", report);

            D16LocalDiagnostics.RevokeConsentAndClear();
            Assert.IsFalse(File.Exists(D16LocalDiagnostics.ReportPath), "清理后后台线程诊断文件仍存在。");
        }

        [UnityTest]
        public IEnumerator UnexpectedSessionMarkerIsRedactedAndCleanExitRemovesMarker()
        {
            D16LocalDiagnostics.RevokeConsentAndClear();

            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16LocalDiagnostics diagnostics = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (diagnostics == null && Time.realtimeSinceStartup < deadline)
            {
                diagnostics = Object.FindFirstObjectByType<D16LocalDiagnostics>(FindObjectsInactive.Include);
                if (diagnostics == null) yield return null;
            }

            Assert.IsNotNull(diagnostics, "D16LocalDiagnostics 未创建。");
            D16LocalDiagnostics.GrantConsent();
            Assert.IsTrue(File.Exists(D16LocalDiagnostics.SessionMarkerPath),
                "获得同意后没有创建当前诊断会话标记。");

            var endSession = typeof(D16LocalDiagnostics).GetMethod("EndSession",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var beginSession = typeof(D16LocalDiagnostics).GetMethod("BeginSession",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(endSession, "诊断器缺少会话结束入口。");
            Assert.IsNotNull(beginSession, "诊断器缺少会话开始入口。");

            endSession.Invoke(diagnostics, null);
            File.WriteAllText(D16LocalDiagnostics.SessionMarkerPath, D16LocalDiagnostics.SessionMarkerToken);
            beginSession.Invoke(diagnostics, null);
            Assert.IsTrue(File.Exists(D16LocalDiagnostics.ReportPath),
                "检测到上次异常退出后没有生成本地诊断记录。");
            string report = File.ReadAllText(D16LocalDiagnostics.ReportPath);
            StringAssert.Contains("\"category\":\"unexpected_exit\"", report);
            StringAssert.Contains("\"raw_message_omitted\":true", report);
            StringAssert.DoesNotContain(D16LocalDiagnostics.SessionMarkerToken, report,
                "会话标记原文不应进入诊断记录。");

            endSession.Invoke(diagnostics, null);
            Assert.IsFalse(File.Exists(D16LocalDiagnostics.SessionMarkerPath),
                "正常结束会话后仍保留异常退出标记。");
            D16LocalDiagnostics.RevokeConsentAndClear();
        }

        [Test]
        public void RedactedRecordOmitsRawMessageAndStack()
        {
            string json = D16DiagnosticRecord.CreateJson(
                "2026-08-12T00:00:00.0000000Z",
                "Exception",
                "InitialHarbor",
                "6000.0.80f1",
                "WindowsPlayer",
                "secret user text C:\\Users\\Captain\\save.db",
                "at Secret.Namespace.Method()\nC:\\Users\\Captain\\stack.cs:10");

            StringAssert.Contains("\"raw_message_omitted\":true", json);
            StringAssert.Contains("\"raw_stack_omitted\":true", json);
            StringAssert.Contains("\"category\":\"exception\"", json);
            StringAssert.DoesNotContain("secret user text", json);
            StringAssert.DoesNotContain("Captain", json);
            StringAssert.DoesNotContain("Secret.Namespace", json);
            StringAssert.DoesNotContain("save.db", json);
        }

        [Test]
        public void ClassifierKeepsUsefulCategoryWithoutCopyingMessage()
        {
            Assert.AreEqual("null_reference", D16DiagnosticRecord.Classify("Error", "NullReferenceException: private text"));
            Assert.AreEqual("camera", D16DiagnosticRecord.Classify("Error", "No cameras rendering"));
            Assert.AreEqual("input", D16DiagnosticRecord.Classify("Error", "Input quaternion was not normalized"));
            Assert.AreEqual("exception", D16DiagnosticRecord.Classify("Exception", "opaque"));
        }

        [Test]
        public void DiagnosticsContractUsesBoundedTokensAndCounts()
        {
            string json = D16DiagnosticRecord.CreateJson(
                "2026/08/12 user path",
                "Error level with spaces",
                "Initial Harbor/Private",
                "Unity 6000/0",
                "Windows\\User",
                new string('x', 5000),
                "one\ntwo\nthree");

            StringAssert.Contains("\"condition_length\":4096", json);
            StringAssert.Contains("\"stack_line_count\":3", json);
            StringAssert.DoesNotContain("/", json);
            StringAssert.DoesNotContain("\\", json);
        }

        [Test]
        public void DiagnosticFileReplacementKeepsOldTargetUntilAtomicCommit()
        {
            string root = Path.Combine(Path.GetTempPath(), "wb_d16_diagnostics_atomic_" +
                System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            string destination = Path.Combine(root, "session.marker");
            string temporary = destination + ".tmp_" + System.Guid.NewGuid().ToString("N");
            try
            {
                File.WriteAllText(destination, "old-marker");
                File.WriteAllText(temporary, "new-marker");

                var replace = typeof(D16LocalDiagnostics).GetMethod("ReplaceFileAtomically",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(replace, "诊断文件缺少原子替换入口。");
                replace.Invoke(null, new object[] { temporary, destination });

                Assert.AreEqual("new-marker", File.ReadAllText(destination));
                Assert.IsFalse(File.Exists(temporary), "原子提交后临时诊断文件应消失。");
            }
            finally
            {
                try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
            }
        }
    }
}
