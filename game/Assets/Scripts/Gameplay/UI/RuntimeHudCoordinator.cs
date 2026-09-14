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
using Whalebone.Gameplay.UI.D16;
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
            "D25PlayerHudCanvas",
            "D29InteractionHudCanvas",
            "D10TavernSocialPage",
            "D30VisualLoopCanvas"
        };

        private bool _paused;
        private string _temporaryCanvasName;
        private static int _primaryInputClaimFrame = -1;

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

        /// <summary>
        /// 让主 HUD 在当前 Unity 帧内声明一次性输入归属。
        /// D29 的按钮/快捷键和 D3 等历史键盘 owner 可能在同一帧看到同一个
        /// legacy 边沿；声明成功后，后续 owner 必须让路，避免一次操作提交两次。
        /// </summary>
        public static bool TryClaimPrimaryInput()
        {
            if (!PrimaryInputAllowed) return false;
            _primaryInputClaimFrame = Time.frameCount;
            return true;
        }

        /// <summary>
        /// D29 的快捷键只能在主 HUD 拥有输入焦点时处理。
        /// D3/D4/D9/D10 等临时页面打开后，按键必须交给当前页面，
        /// 否则同一个 R/M/C 等按键会同时触发两个 owner 的命令。
        /// </summary>
        public static bool PrimaryInputAllowed
        {
            get
            {
                // D16 模态页可能不暂停世界时间（例如无障碍设置/本地诊断），
                // 但它仍然拥有当前键盘焦点。主 HUD 不能在此期间继续消费
                // M/I/T/R 等快捷键，否则同一按键会同时进入模态页和玩法层。
                if (D16InputRouter.GameplayInputBlocked) return false;
                if (_primaryInputClaimFrame == Time.frameCount) return false;
                if (!Application.isFocused && !Application.isBatchMode) return false;

                var coordinators = FindObjectsByType<RuntimeHudCoordinator>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                foreach (var coordinator in coordinators)
                {
                    if (coordinator != null && (coordinator._paused ||
                        !string.IsNullOrEmpty(coordinator._temporaryCanvasName)))
                        return false;
                }

                var pauses = FindObjectsByType<HarborPauseController>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
                foreach (var pause in pauses)
                {
                    if (pause != null && pause.IsPaused) return false;
                }

                var dialogues = FindObjectsByType<Whalebone.Gameplay.UI.Dialogue.D2DialogueController>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var dialogue in dialogues)
                {
                    if (dialogue != null && dialogue.IsOpen) return false;
                }

                // D9 is dynamically created and historically could be present
                // while its canvas was hidden by a different coordinator. Read
                // its explicit page-focus state as a second, owner-local guard
                // so B/M/R/C/P never leak back into the primary HUD.
                var d9Pages = FindObjectsByType<Whalebone.Gameplay.D9.D9PortShipPreparationBootstrap>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var page in d9Pages)
                {
                    if (page != null && page.HasInputFocus) return false;
                }

                var d3Pages = FindObjectsByType<Whalebone.Gameplay.D3.D3RuntimeBootstrap>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var page in d3Pages)
                {
                    if (page != null && page.IsTradeOpen) return false;
                }

                var d4Pages = FindObjectsByType<Whalebone.Gameplay.D4.D4RuntimeBootstrap>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var page in d4Pages)
                {
                    if (page != null && page.HasVisibleContentPage) return false;
                }

                var d10Pages = FindObjectsByType<Whalebone.Gameplay.D10.D10TavernSocialBootstrap>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var page in d10Pages)
                {
                    if (page != null && page.HasInputFocus) return false;
                }

                return true;
            }
        }

        private void ApplyHudPolicy()
        {
            bool showPrimary = string.IsNullOrEmpty(_temporaryCanvasName);
            bool d25PlayerHudActive = Whalebone.Gameplay.D25.D25PlayerHud.IsActive;
            bool showTemporaryD3 = _temporaryCanvasName == "D3Canvas";
            bool showTemporaryD2 = _temporaryCanvasName == "D2HintCanvas";
            bool showTemporaryD4 = _temporaryCanvasName == "D4Canvas";
            bool showTemporaryD10 = _temporaryCanvasName == "D10TavernSocialPage";
            bool showTemporaryD9 = _temporaryCanvasName == "D9PortServicePage";
            bool showTemporaryWarehouse = _temporaryCanvasName == "D29WarehousePanel";
            bool showPrimaryInteractionHud = showPrimary || showTemporaryWarehouse;
            bool showPrimaryPlayerHud = showPrimary;
            SetCanvasState("D5Canvas", false, false); // D5 debug hidden, no raycast block in player mode
            SetCanvasState("D3Canvas", showTemporaryD3, showTemporaryD3 && !_paused);
            SetCanvasState("D2HintCanvas", showTemporaryD2, showTemporaryD2 && !_paused);
            SetCanvasState("D4Canvas", showTemporaryD4, showTemporaryD4 && !_paused);
            SetCanvasState("D10TavernSocialPage", showTemporaryD10, showTemporaryD10 && !_paused);
            SetCanvasState("D9PortServicePage", showTemporaryD9, showTemporaryD9 && !_paused);
            SetCanvasState("D30VisualLoopCanvas", showPrimary, false);
            SetCanvasState("D25PlayerHudCanvas", showPrimaryPlayerHud, showPrimaryPlayerHud && !_paused);
            SetCanvasState("D29InteractionHudCanvas", showPrimaryInteractionHud,
                showPrimaryInteractionHud && !_paused);
            foreach (var name in LegacyHudNames)
                if (name != "D3Canvas" && name != "D2HintCanvas" &&
                    name != "D4Canvas" && name != "D10TavernSocialPage" &&
                    name != "D9PortServicePage" &&
                    name != "D30VisualLoopCanvas" &&
                    name != "D25PlayerHudCanvas" &&
                    name != "D29InteractionHudCanvas")
                    SetCanvasState(name, false, false);
            SetD2HintCanvasState(showTemporaryD2, showTemporaryD2 && !_paused);
            // D25 owns the single player-facing route layer. D19RouteHud remains
            // available as a legacy compatibility component, but must not render
            // at the same time or the old footer will overlap the new HUD.
            SetCanvasState("D19RouteHud", showPrimary && !d25PlayerHudActive,
                showPrimary && !_paused && !d25PlayerHudActive);
        }

        private static void SetCanvasState(string objectName, bool visible, bool interactive)
        {
            // 一个运行时页面可能有宿主对象和子 Canvas，也可能在场景切换
            // 期间短暂存在多个同名兼容 Canvas。只改 GameObject.Find 返回的
            // 第一个对象会留下可见/可点击的重复层，表现为面板堆叠。
            // 这里同时处理直接命中的宿主和全部同名 Canvas，保证策略是
            // “同名对象全部同步”，而不是“找到一个就算完成”。
            var direct = GameObject.Find(objectName);
            if (direct != null)
                ApplyCanvasState(direct, visible, interactive);

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas == null || canvas.gameObject.name != objectName) continue;
                ApplyCanvasState(canvas.gameObject, visible, interactive);
            }
        }

        private static void ApplyCanvasState(GameObject go, bool visible, bool interactive)
        {
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
