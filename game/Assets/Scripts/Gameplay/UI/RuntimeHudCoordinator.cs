// ============================================================
// RuntimeHudCoordinator —— InitialHarbor 运行时 HUD 唯一显示策略
//
// D2/D3/D4/D5 的逻辑入口可以同时存在，但港口屏幕只允许一套主 HUD
// 接收鼠标输入。D5 作为当前主 HUD；D2/D3/D4 旧版说明层保留逻辑，
// 仅隐藏表现与射线，避免重复菜单互相遮挡。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.UI
{
    public sealed class RuntimeHudCoordinator : MonoBehaviour
    {
        private static readonly string[] LegacyHudNames =
        {
            "D2HintCanvas",
            "D3Canvas",
            "D4Canvas"
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
            if (FindFirstObjectByType<RuntimeHudCoordinator>(FindObjectsInactive.Include) != null) return;
            new GameObject("RuntimeHudCoordinator").AddComponent<RuntimeHudCoordinator>();
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
            SetCanvasState("D5Canvas", showPrimary, showPrimary && !_paused);
            foreach (var name in LegacyHudNames)
                SetCanvasState(name, name == _temporaryCanvasName, name == _temporaryCanvasName && !_paused);
        }

        private static void SetCanvasState(string objectName, bool visible, bool interactive)
        {
            var go = GameObject.Find(objectName);
            if (go == null) return;

            var group = go.GetComponent<CanvasGroup>();
            if (group == null) group = go.AddComponent<CanvasGroup>();
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible && interactive;
            group.blocksRaycasts = visible && interactive;
        }
    }
}
