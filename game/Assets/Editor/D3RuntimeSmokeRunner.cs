using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>
    /// Unity 6 batch-mode 的最小运行时 smoke gate：进入 InitialHarbor，确认 D2 对象、
    /// D3 运行时入口、UI Canvas 和三个地点标记都能在 Play Mode 创建。
    /// 放在 Editor 程序集是为了让它能读取 Assembly-CSharp 的 Gameplay 类型，
    /// 不让测试程序集反向依赖表现层。
    /// </summary>
    public static class D3RuntimeSmokeRunner
    {
        public static void RunRuntimeSmoke()
        {
            var scenePath = "Assets/Scenes/InitialHarbor.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError("[D3 Smoke] 找不到 InitialHarbor 场景：" + scenePath);
                EditorApplication.Exit(1);
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }
}
