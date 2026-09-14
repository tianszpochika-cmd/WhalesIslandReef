// ============================================================
// HarborInteractionStabilizer —— InitialHarbor 输入与 UI 交互稳定层
//
// 港口同时有 D2/D3/D4/D5/暂停/对话的运行时 UI。该类只负责统一
// EventSystem、输入模块、射线和旧提示层，不修改任何玩法数值。
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.UI.D16;

namespace Whalebone.Gameplay.UI
{
    public sealed class HarborInteractionStabilizer : MonoBehaviour
    {
        private float _nextReconcile;
        private bool _legacyPromptHidden;
        private bool _navigationPolicyInitialized;
        private bool _lastNavigationEventsEnabled;
        private InputActionReference _defaultNavigationAction;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<HarborInteractionStabilizer>(FindObjectsInactive.Include) != null) return;
            new GameObject("HarborInteractionStabilizer").AddComponent<HarborInteractionStabilizer>();
            Debug.Log("[D6/Input] 港口交互稳定器已创建");
        }

        private void Awake()
        {
            Reconcile();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Update()
        {
            ReconcileNavigationPolicy();
            if (Time.unscaledTime < _nextReconcile) return;
            _nextReconcile = Time.unscaledTime + 0.75f;
            Reconcile();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) Reconcile();
        }

        private void Reconcile()
        {
            EnsureSingleEventSystem();
            ReconcileNavigationPolicy();
            NormalizeCanvasesAndRaycasts();
        }

        /// <summary>
        /// 正常港口移动时，W/S 只能进入 Gameplay 输入，不应被 UGUI
        /// EventSystem 当作主 HUD 的上下导航。打开暂停、仓库、交易、酒馆
        /// 或无障碍等模态页后，再恢复 UGUI 导航。
        /// </summary>
        private void ReconcileNavigationPolicy()
        {
            var keeper = EventSystem.current;
            if (keeper == null) return;

            bool hasApplicationFocus = Application.isFocused || Application.isBatchMode;
            bool modalOwnsInput = D16InputRouter.GameplayInputBlocked ||
                !RuntimeHudCoordinator.PrimaryInputAllowed;
            bool enableNavigation = hasApplicationFocus && modalOwnsInput;

            var input = keeper.GetComponent<InputSystemUIInputModule>();
            if (input != null)
            {
                if (_defaultNavigationAction == null && input.move != null)
                    _defaultNavigationAction = input.move;

                // sendNavigationEvents=false is not sufficient protection: the
                // Input System UI module can still retain and process its
                // Navigate action around a focus/reconcile transition. Keep
                // pointer/submit wiring intact for modal pages, but remove only
                // the directional action while the primary gameplay HUD owns
                // the keyboard. Modal pages restore the captured default action.
                var expectedNavigationAction = enableNavigation
                    ? _defaultNavigationAction
                    : null;
                if (input.move != expectedNavigationAction)
                    input.move = expectedNavigationAction;
            }

            // Another UI owner can restore InputSystemUIInputModule.move while
            // leaving EventSystem.sendNavigationEvents unchanged.  Treat the
            // directional action as part of the policy's observable state, or
            // the early return would leave W/S wired to HUD navigation after a
            // focus/modal transition.
            bool navigationActionMatches = input == null ||
                input.move == (enableNavigation ? _defaultNavigationAction : null);
            if (_navigationPolicyInitialized &&
                _lastNavigationEventsEnabled == enableNavigation &&
                keeper.sendNavigationEvents == enableNavigation &&
                navigationActionMatches)
                return;

            keeper.sendNavigationEvents = enableNavigation;
            _navigationPolicyInitialized = true;
            _lastNavigationEventsEnabled = enableNavigation;

            if (!enableNavigation && keeper.currentSelectedGameObject != null)
                keeper.SetSelectedGameObject(null);

            Debug.Log("[D6/Input] EventSystem navigation=" +
                (enableNavigation ? "modal" : "gameplay_suppressed"));
        }

        private void EnsureSingleEventSystem()
        {
            var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            EventSystem keeper = EventSystem.current;
            if (keeper == null || !keeper.gameObject.activeInHierarchy)
            {
                foreach (var system in systems)
                {
                    if (system != null && system.gameObject.activeInHierarchy)
                    {
                        keeper = system;
                        break;
                    }
                }
            }
            if (keeper == null)
            {
                var go = new GameObject("EventSystem");
                keeper = go.AddComponent<EventSystem>();
            }

            foreach (var system in systems)
            {
                if (system == null || system == keeper) continue;
                system.enabled = false;
                var module = system.GetComponent<BaseInputModule>();
                if (module != null) module.enabled = false;
                if (system.gameObject.activeSelf) system.gameObject.SetActive(false);
            }

            keeper.gameObject.SetActive(true);
            keeper.enabled = true;
            EventSystem.current = keeper;
            var input = keeper.GetComponent<InputSystemUIInputModule>();
            if (input == null) input = keeper.gameObject.AddComponent<InputSystemUIInputModule>();
            input.enabled = true;
            if (input.actionsAsset == null || (_defaultNavigationAction == null && input.move == null))
                input.AssignDefaultActions();
            foreach (var module in keeper.GetComponents<BaseInputModule>())
                if (module != input) module.enabled = false;
            keeper.UpdateModules();
        }

        private void NormalizeCanvasesAndRaycasts()
        {
            // 场景文件中的旧相机操作提示与 D5 主 HUD 重复，隐藏提示而保留港口标题。
            var legacyHint = GameObject.Find("HarborControls");
            if (legacyHint != null && legacyHint.activeSelf)
            {
                legacyHint.SetActive(false);
                _legacyPromptHidden = true;
            }

            foreach (var text in FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (text != null) text.raycastTarget = false;

            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button == null) continue;
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.raycastTarget = true;
                    if (button.targetGraphic == null) button.targetGraphic = image;
                }
                // The primary gameplay HUD is visible while the player is
                // walking or steering. Even if a Player-side EventSystem
                // briefly retains a selected button, Automatic navigation
                // would let W/S move that selection instead of only moving
                // the character/ship. Modal pages regain Automatic navigation
                // through the same coordinator state below.
                button.navigation = RuntimeHudCoordinator.PrimaryInputAllowed
                    ? new Navigation { mode = Navigation.Mode.None }
                    : new Navigation { mode = Navigation.Mode.Automatic };
            }

            foreach (var input in FindObjectsByType<InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (input != null && input.targetGraphic != null) input.targetGraphic.raycastTarget = true;

            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas == null || !canvas.gameObject.activeInHierarchy) continue;
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null) raycaster.enabled = true;
            }
        }

        public bool LegacyPromptHidden => _legacyPromptHidden;
    }
}
