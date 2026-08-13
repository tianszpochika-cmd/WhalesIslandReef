// ============================================================
// D1Acceptance —— D1 一键验收脚本（统筹生成，2026-08-08）
//
// 用途：把 D1 最后三步合成一次菜单点击：
//   ① 运行 EditMode 测试（SaveSystemTests 8 用例，P1B 闸门）
//   ② 全绿 → 自动执行 HarborArtInstaller.Install()（港口 CC0 美化）
//   ③ 打开 InitialHarbor 场景，弹窗提示用户 Play 查看
//
// 设计要点：
//   - 仅 Editor 工具（Tools 菜单），零运行时逻辑
//   - 测试失败则中止后续步骤并弹窗提示（不强行继续）
//   - 测试为异步执行，通过 TestRunnerApi 回调串联后续步骤
//   - 测试结果用反射安全读取（应对不同 Test Framework 版本的字段差异）
//
// API 来源（已逐字段核对 Library/PackageCache/com.unity.test-framework@082c24152fd4）：
//   - TestRunnerApi / ICallbacks / ITestAdaptor / ITestResultAdaptor / Filter / ExecutionSettings
//     → UnityEditor.TestTools.TestRunner.Api
//   - TestMode enum → UnityEngine.TestTools
//   - ITestResultAdaptor 真实属性：PassCount / FailCount / SkipCount（不是 SkippedCount）/ InconclusiveCount / AssertCount
// ============================================================

using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
// TestMode 在两个 namespace 都定义（UnityEditor.TestTools.TestRunner.Api + UnityEngine.TestTools）——
// 用别名明确指向 Api.TestMode（与 Filter.testMode 同 namespace，必须保持一致）
using TestMode = UnityEditor.TestTools.TestRunner.Api.TestMode;

namespace Whalebone.EditorTools
{
    public static class D1Acceptance
    {
        [MenuItem("Tools/Whalebone/D1 一键验收 (测试+美化+打开场景)")]
        public static void Run()
        {
            Debug.Log("[D1Acceptance] 开始：① 运行 EditMode 测试...");
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Callbacks());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
        }

        /// <summary>
        /// D16 本地批处理入口：只运行 Save.Tests，不触发 D1 美化或场景切换。
        /// 放在已被 Unity 识别的 Editor 程序集中，供无界面验收使用。
        /// </summary>
        public static void RunD16SaveTests()
        {
            Save.Tests.D2BatchTestRunner.RunD2();
        }

        /// <summary>安全读取 ITestResultAdaptor 整数属性（属性不存在时返回 0，避免版本差异编译错）。</summary>
        private static int SafeGetInt(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (p == null || p.PropertyType != typeof(int)) return 0;
            return (int)p.GetValue(obj);
        }

        private class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                // 反射读取（防御性：不同 Test Framework 版本字段名可能略有差异）
                int passed = SafeGetInt(result, "PassCount");
                int failed = SafeGetInt(result, "FailCount");
                int skipped = SafeGetInt(result, "SkipCount");
                int inconclusive = SafeGetInt(result, "InconclusiveCount");
                int total = passed + failed + skipped + inconclusive;
                Debug.Log($"[D1Acceptance] ① 测试完成：Passed={passed} Failed={failed} Skipped={skipped} Inconclusive={inconclusive} Total={total}");

                if (failed > 0 || total == 0)
                {
                    Debug.LogError("[D1Acceptance] 测试未全绿，中止后续步骤。请打开 Window>General>Test Runner 查看失败详情。");
                    EditorUtility.DisplayDialog("D1 验收", $"测试失败 {failed} 个（总 {total}），已中止美化。\n请先在 Test Runner 查看并修复。", "OK");
                    return;
                }

                // ② 测试全绿 → 港口美化
                Debug.Log("[D1Acceptance] ② 测试全绿，执行港口 CC0 美化（Install Harbor Art）...");
                try
                {
                    Whalebone.EditorTools.HarborArtInstaller.Install();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[D1Acceptance] 美化脚本异常：" + ex);
                    EditorUtility.DisplayDialog("D1 验收", "美化脚本异常：" + ex.Message, "OK");
                    return;
                }

                // ③ 打开场景
                Debug.Log("[D1Acceptance] ③ 打开 InitialHarbor 场景...");
                EditorSceneManager.OpenScene("Assets/Scenes/InitialHarbor.unity", OpenSceneMode.Single);

                Debug.Log("[D1Acceptance] ✅ D1 一键验收完成：测试全绿 + 港口美化已应用 + 场景已打开。");
                EditorUtility.DisplayDialog("D1 验收", $"✅ 测试全绿（Passed={passed} Total={total}）\n港口美化已应用，InitialHarbor 已打开。\n点 Play 查看效果。", "OK");
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
