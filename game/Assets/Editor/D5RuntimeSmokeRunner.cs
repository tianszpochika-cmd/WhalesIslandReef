using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Whalebone.EditorTools;

namespace Save.Tests
{
    /// <summary>Unity 6 batch-mode D5 runtime 冒烟入口。</summary>
    public static class D5RuntimeSmokeRunner
    {
        public static void RunRuntimeSmoke()
        {
            const string scenePath = "Assets/Scenes/InitialHarbor.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogError("[D5 Smoke] 找不到 InitialHarbor 场景：" + scenePath);
                EditorApplication.Exit(1);
                return;
            }
            // 批处理验收不依赖用户先手动点击港口安装菜单，确保正式动作资源先生成。
            D6AnimationAssetInstaller.EnsureAfterHarborInstall();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }
}
