using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D16;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A06 的本地诊断隐私面板。
    ///
    /// 该面板只负责把 D16LocalDiagnostics 的选择加入、撤回和删除入口呈现给用户，
    /// 不读取存档、不上传数据，也不改变领域状态。独立 Canvas 保证它不会与主菜单、
    /// 暂停菜单或无障碍面板争夺同一层级的点击区域。
    /// </summary>
    [DefaultExecutionOrder(-880)]
    public sealed class D16LocalDiagnosticsPanel : MonoBehaviour
    {
        public static D16LocalDiagnosticsPanel Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        private GameObject _canvasGo;
        private GameObject _modal;
        private Toggle _consentToggle;
        private Text _statusText;
        private Button _closeButton;
        private bool _refreshing;
        private bool _previousGameplayInputBlocked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu" && scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D16LocalDiagnosticsPanel>(FindObjectsInactive.Include) != null) return;
            new GameObject("D16LocalDiagnosticsPanel").AddComponent<D16LocalDiagnosticsPanel>();
        }

        public static void ShowPanel()
        {
            var panel = FindFirstObjectByType<D16LocalDiagnosticsPanel>(FindObjectsInactive.Include);
            if (panel == null)
            {
                string sceneName = SceneManager.GetActiveScene().name;
                if (sceneName != "MainMenu" && sceneName != "InitialHarbor") return;
                panel = new GameObject("D16LocalDiagnosticsPanel").AddComponent<D16LocalDiagnosticsPanel>();
            }

            panel.OpenInternal();
        }

        public static void HidePanel()
        {
            var panel = FindFirstObjectByType<D16LocalDiagnosticsPanel>(FindObjectsInactive.Include);
            if (panel != null) panel.CloseInternal();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureEventSystem();
            BuildCanvas();
            BuildPanel();
            D16ModalFocusScope.Attach(_modal, _closeButton == null ? null : _closeButton.gameObject);
            _modal.SetActive(false);
            IsOpen = false;
        }

        private void Update()
        {
            if (!IsOpen) return;
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
                CloseInternal();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsOpen)
                D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            IsOpen = false;
        }

        private void OpenInternal()
        {
            if (_modal == null || IsOpen) return;

            D16AccessibilityPanel.HidePanel();
            D16FinanceDebtPanel.HidePanel();

            _previousGameplayInputBlocked = D16InputRouter.GameplayInputBlocked;
            IsOpen = true;
            D16InputRouter.SetGameplayInputBlocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            RefreshControls();
            _modal.SetActive(true);
            Canvas.ForceUpdateCanvases();
            if (EventSystem.current != null && _closeButton != null)
                EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
            Debug.Log("[D16/A06] 本地诊断隐私面板已打开：默认关闭、仅本地脱敏记录");
        }

        private void CloseInternal()
        {
            if (_modal == null || !IsOpen) return;
            IsOpen = false;
            D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            _modal.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[D16/A06] 本地诊断隐私面板已关闭：玩法输入已恢复");
        }

        private void GrantConsent()
        {
            D16LocalDiagnostics.GrantConsent();
            SetStatus(Localization.Get("diagnostics_enabled"));
        }

        private void RevokeConsent()
        {
            D16LocalDiagnostics.RevokeConsent();
            SetStatus(Localization.Get("diagnostics_disabled"));
        }

        private void RevokeConsentAndClear()
        {
            D16LocalDiagnostics.RevokeConsentAndClear();
            RefreshControls();
            SetStatus(Localization.Get("diagnostics_deleted"));
        }

        private void RefreshControls()
        {
            _refreshing = true;
            if (_consentToggle != null) _consentToggle.isOn = D16LocalDiagnostics.HasConsent;
            _refreshing = false;
            if (_statusText != null)
                _statusText.text = D16LocalDiagnostics.HasConsent
                    ? Localization.Get("diagnostics_enabled")
                    : Localization.Get("diagnostics_disabled");
        }

        private void SetStatus(string value)
        {
            if (_statusText != null) _statusText.text = value;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();

            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (other != eventSystem) other.gameObject.SetActive(false);
            }

            eventSystem.gameObject.SetActive(true);
            if (EventSystem.current != eventSystem) EventSystem.current = eventSystem;

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
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
            _canvasGo = new GameObject("D16LocalDiagnosticsCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGo.transform.SetParent(transform, false);

            var canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            var scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPanel()
        {
            _modal = new GameObject("DiagnosticsModal", typeof(RectTransform), typeof(Image));
            _modal.transform.SetParent(_canvasGo.transform, false);
            Stretch((RectTransform)_modal.transform);
            var modalImage = _modal.GetComponent<Image>();
            modalImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            modalImage.color = new Color(0.01f, 0.025f, 0.045f, 0.9f);
            modalImage.raycastTarget = true;

            var card = new GameObject("DiagnosticsCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_modal.transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(980f, 640f);
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            cardImage.color = new Color(0.025f, 0.055f, 0.09f, 0.99f);

            CreateText(card, "Title", Localization.Get("diagnostics_title"), 42,
                new Vector2(0f, 255f), new Vector2(880f, 58f));
            CreateText(card, "Subtitle", Localization.Get("diagnostics_subtitle"), 21,
                new Vector2(0f, 195f), new Vector2(820f, 105f));
            CreateText(card, "Privacy", Localization.Get("diagnostics_privacy"), 19,
                new Vector2(0f, 83f), new Vector2(820f, 135f));

            CreateToggleRow(card);
            _statusText = CreateText(card, "Status", string.Empty, 19,
                new Vector2(0f, -75f), new Vector2(820f, 42f));
            _statusText.color = new Color(1f, 0.82f, 0.42f, 1f);

            CreateButton(card, "DeleteAndDisable", Localization.Get("diagnostics_delete"),
                new Vector2(-170f, -165f), RevokeConsentAndClear, new Vector2(360f, 52f), 18);
            _closeButton = CreateButton(card, "Close", Localization.Get("a11y_close"),
                new Vector2(230f, -165f), CloseInternal, new Vector2(220f, 52f), 18);
        }

        private void CreateToggleRow(GameObject card)
        {
            var label = CreateText(card, "ConsentLabel", Localization.Get("diagnostics_consent"), 21,
                new Vector2(-170f, -18f), new Vector2(560f, 44f));
            label.alignment = TextAnchor.MiddleLeft;

            var toggleGo = new GameObject("ConsentToggle", typeof(RectTransform), typeof(Toggle));
            toggleGo.transform.SetParent(card.transform, false);
            var rect = (RectTransform)toggleGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(64f, 36f);
            rect.anchoredPosition = new Vector2(320f, -18f);

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(toggleGo.transform, false);
            Stretch((RectTransform)background.transform);
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            backgroundImage.color = new Color(0.12f, 0.18f, 0.24f, 0.95f);

            var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(toggleGo.transform, false);
            var checkRect = (RectTransform)check.transform;
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.sizeDelta = new Vector2(42f, 28f);
            var checkImage = check.GetComponent<Image>();
            checkImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            checkImage.color = new Color(0.35f, 0.75f, 0.95f, 1f);

            _consentToggle = toggleGo.GetComponent<Toggle>();
            _consentToggle.targetGraphic = backgroundImage;
            _consentToggle.graphic = checkImage;
            _consentToggle.isOn = D16LocalDiagnostics.HasConsent;
            _consentToggle.onValueChanged.AddListener(value =>
            {
                if (_refreshing) return;
                if (value) GrantConsent();
                else RevokeConsent();
            });
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position,
            UnityEngine.Events.UnityAction onClick, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            image.color = new Color(0.13f, 0.36f, 0.56f, 0.98f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var colors = button.colors;
            colors.highlightedColor = new Color(0.25f, 0.55f, 0.8f, 1f);
            colors.pressedColor = new Color(0.08f, 0.22f, 0.35f, 1f);
            button.colors = colors;

            var text = CreateText(go, "Label", label, fontSize, Vector2.zero, size);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string content, int size, Vector2 position,
            Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = position;

            var text = go.GetComponent<Text>();
            text.font = Whalebone.Gameplay.UI.Fonts.Default;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
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
