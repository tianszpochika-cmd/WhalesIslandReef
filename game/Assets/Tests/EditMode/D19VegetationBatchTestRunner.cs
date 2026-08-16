using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace WhalesIslandReef.Tests.EditMode
{
    /// <summary>Batch entry point for the D19-A03 vegetation gate.</summary>
    public static class D19VegetationBatchTestRunner
    {
        public static void RunD19A03()
        {
            Callback callback = new Callback();
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(callback);
            ExecutionSettings settings = new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "Save.Tests" },
                testNames = new[] { "WhalesIslandReef.Tests.EditMode.D19VegetationBiomeTests" },
            })
            {
                runSynchronously = true,
            };
            api.Execute(settings);
            if (callback.Result == null)
            {
                Debug.LogError("D19-A03 test runner returned without a result.");
                EditorApplication.Exit(1);
                return;
            }
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string resultPath = Path.Combine(projectRoot, "tools/results/D19/D19-A03/D19_A03_EDITMODE_TESTS.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            TestRunnerApi.SaveResultToFile(callback.Result, resultPath);
            Debug.Log("D19-A03 EditMode tests: passed=" + callback.Result.PassCount +
                ", failed=" + callback.Result.FailCount + ", skipped=" + callback.Result.SkipCount);
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
