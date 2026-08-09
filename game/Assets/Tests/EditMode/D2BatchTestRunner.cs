using System;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>
    /// Unity 6 batch-mode entry point for the unified D2/D3 EditMode test gate.
    /// Unity's command-line test switch can exit during first-time asset import,
    /// so this explicit entry point runs after assemblies have loaded.
    /// </summary>
    public static class D2BatchTestRunner
    {
        public static void RunD2()
        {
            var callback = new Callback();
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(callback);

            var settings = new ExecutionSettings(new Filter
            {
                testMode = UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,
                assemblyNames = new[] { "Save.Tests" },
            })
            {
                runSynchronously = true,
            };

            api.Execute(settings);
            if (callback.Result == null)
            {
            Debug.LogError("D2/D3 test runner returned without a result.");
                EditorApplication.Exit(1);
                return;
            }

            var resultPath = Path.Combine("D2_TestResults.xml");
            TestRunnerApi.SaveResultToFile(callback.Result, resultPath);
            Debug.Log($"D2/D3 EditMode tests: passed={callback.Result.PassCount}, failed={callback.Result.FailCount}, skipped={callback.Result.SkipCount}");
            EditorApplication.Exit(callback.Result.FailCount == 0 ? 0 : 1);
        }

        private sealed class Callback : ICallbacks
        {
            public ITestResultAdaptor Result { get; private set; }

            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result) { Result = result; }
        }
    }
}
