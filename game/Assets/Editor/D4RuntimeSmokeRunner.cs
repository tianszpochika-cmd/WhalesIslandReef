using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>Unity 6 batch-mode D4 runtime 冒烟入口。</summary>
    public static class D4RuntimeSmokeRunner
    {
        public static void RunRuntimeSmoke()
        {
            const string scenePath = "Assets/Scenes/InitialHarbor.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogError("[D4 Smoke] 找不到 InitialHarbor 场景：" + scenePath);
                EditorApplication.Exit(1);
                return;
            }
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }
}
