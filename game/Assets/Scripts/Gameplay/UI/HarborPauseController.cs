// ============================================================
// HarborPauseController —— InitialHarbor 暂停与返回入口（D1 收尾）
//
// 归属：Gameplay/UI（表现层）
// 职责：
//   1. ESC 打开/关闭暂停面板
//   2. 继续游戏 / 返回主菜单 / 退出游戏
//   3. 为港口场景补齐 EventSystem + InputSystemUIInputModule
//
// 说明：
//   - 船只航行、人物移动属于 D2，不在此类中伪造控制逻辑
//   - 不写 SaveSystem 数据；返回主菜单后由 MainMenuController 负责继续读档
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Core;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.UI
{
    /// <summary>初始港口场景的 ESC 暂停菜单。</summary>
    public sealed class HarborPauseController : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor")
                return;

            if (FindFirstObjectByType<HarborPauseController>(FindObjectsInactive.Include) != null)
                return;

            new GameObject("HarborPauseController").AddComponent<HarborPauseController>();
            Debug.Log("[HarborPause] 场景加载完成，暂停控制器已创建");
        }

        private GameObject _canvasGo;
        private GameObject _panel;
        private Button _resumeButton;
        private bool _paused;
        private bool _sceneChangeRequested;
        private float _previousTimeScale = 1f;

        public bool IsPaused => _paused;

        private void Awake()
        {
            Time.timeScale = 1f;
            EnsureEventSystem();
            BuildCanvas();
            BuildPanel();
            Debug.Log("[HarborPause] 已就绪：ESC 暂停/恢复，菜单可返回主菜单");
        }

        private void Update()
        {
            if (D7ControlCoordinator.EscapeHandledThisFrame) return;
            if (D16InputRouter.WasPressedThisFrame(D16InputAction.Pause))
                TogglePause();
        }

        private void OnDestroy()
        {
            if (_paused)
                Time.timeScale = _previousTimeScale;
        }

        private void TogglePause()
        {
            if (_paused) ResumeGame();
            else PauseGame();
        }

        private void PauseGame()
        {
            _paused = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _panel.SetActive(true);
            RuntimeHudCoordinator.SetPaused(true);
            Canvas.ForceUpdateCanvases();
            Debug.Log("[HarborPause] 游戏已暂停");

            if (EventSystem.current != null && _resumeButton != null)
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
        }

        private void ResumeGame()
        {
            _paused = false;
            Time.timeScale = _previousTimeScale <= 0f ? 1f : _previousTimeScale;
            _panel.SetActive(false);
            RuntimeHudCoordinator.SetPaused(false);
            Debug.Log("[HarborPause] 游戏已恢复");
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void ReturnToMainMenu()
        {
            if (_sceneChangeRequested)
                return;

            _sceneChangeRequested = true;
            Debug.Log("[HarborPause] 返回主菜单按钮被点击");
            D7SaveCoordinator.SaveAllRuntimeState();
            _paused = false;
            Time.timeScale = 1f;
            RuntimeHudCoordinator.SetPaused(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach (var button in _panel.GetComponentsInChildren<Button>(true))
                button.interactable = false;

            if (GameManager.Instance != null)
                GameManager.Instance.LoadMainMenu();
            else
                SceneManager.LoadScene("MainMenu");
        }

        private static void ExitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            Debug.Log("[HarborPause] 编辑器中执行退出：停止 Play Mode");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystem = eventSystemGo.AddComponent<EventSystem>();
            }

            foreach (var other in FindObjectsByType<EventSystem>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (other != eventSystem) other.gameObject.SetActive(false);
            }

            eventSystem.gameObject.SetActive(true);
            if (EventSystem.current != eventSystem)
                EventSystem.current = eventSystem;

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
            {
                if (module != inputModule) module.enabled = false;
            }
            eventSystem.UpdateModules();
        }

        private void BuildCanvas()
        {
            _canvasGo = new GameObject("HarborPauseCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGo.transform.SetParent(transform, false);

            var canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // 暂停菜单必须压在运行时 HUD 之上，否则 D5 控件会拦截鼠标。
            canvas.sortingOrder = 500;

            var scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPanel()
        {
            _panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(_canvasGo.transform, false);
            Stretch((RectTransform)_panel.transform);

            var panelImage = _panel.GetComponent<Image>();
            panelImage.sprite = UiSprites.White;
            panelImage.color = new Color(0.02f, 0.04f, 0.07f, 0.82f);

            CreateText(_panel, "Title", Localization.Get("pause_title"), 64, new Vector2(0f, 180f));
            _resumeButton = CreateButton(_panel, "Resume", Localization.Get("pause_resume"), new Vector2(0f, 80f), ResumeGame);
            CreateButton(_panel, "Accessibility", Localization.Get("pause_accessibility"), new Vector2(0f, -10f),
                D16AccessibilityPanel.ShowPanel);
            CreateButton(_panel, "Diagnostics", Localization.Get("pause_diagnostics"), new Vector2(0f, -100f),
                D16LocalDiagnosticsPanel.ShowPanel);
            CreateButton(_panel, "Finance", Localization.Get("pause_finance"), new Vector2(0f, -190f),
                D16FinanceDebtPanel.ShowPanel);
            CreateButton(_panel, "MainMenu", Localization.Get("pause_main_menu"), new Vector2(0f, -280f), ReturnToMainMenu);
            CreateButton(_panel, "Exit", Localization.Get("pause_exit"), new Vector2(0f, -370f), ExitGame);

            D16ModalFocusScope.Attach(_panel, _resumeButton == null ? null : _resumeButton.gameObject);
            _panel.SetActive(false);
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position,
            UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 64f);
            rect.anchoredPosition = position;

            var image = buttonGo.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.15f, 0.35f, 0.52f, 0.98f);

            var button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var text = CreateText(buttonGo, "Label", label, 28, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string content, int size, Vector2 position)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(900f, size + 24f);
            rect.anchoredPosition = position;

            var text = textGo.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
