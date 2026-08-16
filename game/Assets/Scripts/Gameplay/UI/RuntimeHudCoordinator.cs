// ============================================================
// RuntimeHudCoordinator —— InitialHarbor 运行时 HUD 唯一显示策略
//
// D2/D3/D4/D5 的逻辑入口可以同时存在，但港口屏幕只允许一套主 HUD
// 接收鼠标输入。D19RouteHud 是玩家模式的主状态层；D2/D3/D4/D5、
// D6 引导、D7 诊断和 D9 读模型保留逻辑，只在需要时显示，避免重复菜单
// 和开发诊断层遮挡玩家视野。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.UI.D19RouteHud;

namespace Whalebone.Gameplay.UI
{
    public sealed class RuntimeHudCoordinator : MonoBehaviour
    {
        private static readonly string[] LegacyHudNames =
        {
            "D2HintCanvas",
            "D2ControlRouter",
            "D3Canvas",
            "D4Canvas",
            "D6TutorialCanvas",
            "D7StatusCanvas",
            "D7RuntimeStatusHud",
            "D9VoyageHud",
            "D9VoyageHudController",
            "D9PortServicePage",
            "D9FishingStatus",
            "D10TavernSocialPage"
        };

        private bool _paused;
        private string _temporaryCanvasName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include) == null)
                new GameObject("RuntimeHudCoordinator").AddComponent<RuntimeHudCoordinator>();
            if (FindFirstObjectByType<D19RouteHudManager>(FindObjectsInactive.Include) == null)
                new GameObject("D19RouteHudManager").AddComponent<D19RouteHudManager>();
        }

        private void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name != "InitialHarbor") return;
            ApplyHudPolicy();
        }

        public static void SetPaused(bool paused)
        {
            var coordinator = FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include);
            if (coordinator == null) return;
            coordinator._paused = paused;
            coordinator.ApplyHudPolicy();
        }

        public static void ApplyNow()
        {
            var coordinator = FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include);
            if (coordinator == null)
                coordinator = new GameObject("RuntimeHudCoordinator").AddComponent<RuntimeHudCoordinator>();
            coordinator.ApplyHudPolicy();
        }

        public static void ShowTemporaryCanvas(string canvasName)
        {
            var coordinator = FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include);
            if (coordinator == null) return;
            coordinator._temporaryCanvasName = canvasName;
            coordinator.ApplyHudPolicy();
        }

        public static void RestorePrimaryHud()
        {
            var coordinator = FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include);
            if (coordinator == null) return;
            coordinator._temporaryCanvasName = null;
            coordinator.ApplyHudPolicy();
        }

        private void ApplyHudPolicy()
        {
            bool showPrimary = string.IsNullOrEmpty(_temporaryCanvasName);
            bool showTemporaryD3 = _temporaryCanvasName == "D3Canvas";
            bool showTemporaryD2 = _temporaryCanvasName == "D2HintCanvas";
            SetCanvasState("D5Canvas", false, false); // D5 debug hidden, no raycast block in player mode
            SetCanvasState("D3Canvas", showTemporaryD3, showTemporaryD3 && !_paused);
            SetCanvasState("D2HintCanvas", showTemporaryD2, showTemporaryD2 && !_paused);
            foreach (var name in LegacyHudNames)
                if (name != "D3Canvas" && name != "D2HintCanvas")
                    SetCanvasState(name, false, false);
            SetD2HintCanvasState(showTemporaryD2, showTemporaryD2 && !_paused);
            SetCanvasState("D19RouteHud", showPrimary, showPrimary && !_paused);
        }

        private static void SetCanvasState(string objectName, bool visible, bool interactive)
        {
            var go = GameObject.Find(objectName);
            if (go == null)
            {
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    if (canvas != null && canvas.gameObject.name == objectName)
                    {
                        go = canvas.gameObject;
                        break;
                    }
                }
            }
            if (go == null) return;

            var group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible && interactive;
            group.blocksRaycasts = visible && interactive;
            var targetCanvas = go.GetComponent<Canvas>();
            if (targetCanvas != null) targetCanvas.enabled = visible;
        }

        private static void SetD2HintCanvasState(bool visible, bool interactive)
        {
            var hints = Object.FindObjectsByType<D2ControlHint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var hint in hints)
            {
                hint.enabled = visible;
                var canvases = hint.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in canvases)
                {
                    if (canvas == null || canvas.gameObject.name != "D2HintCanvas") continue;
                    var group = canvas.GetComponent<CanvasGroup>();
                    if (group == null) group = canvas.gameObject.AddComponent<CanvasGroup>();
                    group.alpha = visible ? 1f : 0f;
                    group.interactable = visible && interactive;
                    group.blocksRaycasts = visible && interactive;
                    canvas.enabled = visible;
                }
            }
        }
    }
}
