using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Whalebone.Gameplay.D25
{
    /// <summary>
    /// D25 视觉表现层启动器。它只创建表现对象并隔离旧的调试 Canvas，
    /// 不禁用 D19/D20/D21 的逻辑组件，也不改变稳定交互节点。
    /// </summary>
    public sealed class D25VisualRuntimeBootstrap : MonoBehaviour
    {
        private static readonly string[] DefaultLegacyVisualNames =
        {
            "D5Canvas",
            "D2HintCanvas",
            "D6TutorialCanvas",
            "D7StatusCanvas",
            "D7RuntimeStatusHud",
            "D19RouteHud",
            "HarborControls",
            "D16LocalDiagnosticsCanvas"
        };

        private float _nextCleanup;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D25VisualRuntimeBootstrap>(FindObjectsInactive.Include) != null) return;

            var root = new GameObject("D25VisualRuntime");
            root.AddComponent<D25VisualRuntimeBootstrap>();
            root.AddComponent<D25HarborAtmosphere>();
            root.AddComponent<D25PlayerHud>();
        }

        private void Awake()
        {
            CleanupLegacyVisualLayers();
            Canvas.ForceUpdateCanvases();
            Debug.Log("[D25/Visual] player presentation layer installed");
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            if (Time.unscaledTime < _nextCleanup) return;
            _nextCleanup = Time.unscaledTime + 0.5f;
            CleanupLegacyVisualLayers();
        }

        private static void CleanupLegacyVisualLayers()
        {
            for (var i = 0; i < DefaultLegacyVisualNames.Length; i++)
            {
                var go = FindSceneObject(DefaultLegacyVisualNames[i]);
                if (go == null || go.name == "D25PlayerHudCanvas") continue;
                var canvas = go.GetComponent<Canvas>();
                // UnityEngine.Object may hold a managed wrapper for a missing
                // native component. Use Unity's overloaded equality instead of
                // C# null-coalescing so the group is recreated when necessary.
                var group = go.GetComponent<CanvasGroup>();
                if (group == null) group = go.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
                if (canvas != null) canvas.enabled = false;

                foreach (var button in go.GetComponentsInChildren<Button>(true))
                {
                    if (button == null) continue;
                    button.interactable = false;
                }
            }

            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text == null || text.transform.GetComponentInParent<D25PlayerHud>() != null) continue;
                if (text.transform.GetComponentInParent<Canvas>() == null) continue;
                text.raycastTarget = false;
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null) continue;
                if (roots[i].name == name) return roots[i];
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                    if (transforms[j] != null && transforms[j].name == name) return transforms[j].gameObject;
            }
            return null;
        }
    }
}
