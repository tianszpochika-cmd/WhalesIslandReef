using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A03 的真实运行时设置面板。
    ///
    /// 面板是独立的高层模态 Canvas：打开后阻断玩法输入、保留 EventSystem 导航，
    /// 只通过 D16AccessibilitySettingsStore 保存表现偏好和键位，不写存档或领域状态。
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class D16AccessibilityPanel : MonoBehaviour
    {
        private enum ListeningDevice
        {
            None,
            Keyboard,
            Gamepad,
        }

        private static readonly D16InputAction[] RebindableActions = BuildRebindableActions();

        private static D16InputAction[] BuildRebindableActions()
        {
            var bindings = D16AccessibilityDefaults.CreateBindings();
            var actions = new D16InputAction[bindings.Count];
            for (int i = 0; i < bindings.Count; i++)
                actions[i] = bindings[i].Action;
            return actions;
        }

        public static D16AccessibilityPanel Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        private GameObject _canvasGo;
        private GameObject _root;
        private Text _statusText;
        private Text _scaleValueText;
        private Button _colorVisionButton;
        private Slider _textScaleSlider;
        private Button _closeButton;
        private readonly Dictionary<D16InputAction, Button> _keyboardButtons =
            new Dictionary<D16InputAction, Button>();
        private readonly Dictionary<D16InputAction, Button> _gamepadButtons =
            new Dictionary<D16InputAction, Button>();
        private D16AccessibilitySettings _settings;
        private D16InputAction _listeningAction;
        private ListeningDevice _listeningDevice;
        private bool _refreshing;
        private bool _previousGameplayInputBlocked;
        private bool _inputBlockStateCaptured;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu" && scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D16AccessibilityPanel>(FindObjectsInactive.Include) != null) return;
            new GameObject("D16AccessibilityPanel").AddComponent<D16AccessibilityPanel>();
        }

        public static void ShowPanel()
        {
            var panel = FindFirstObjectByType<D16AccessibilityPanel>(FindObjectsInactive.Include);
            if (panel == null)
            {
                if (SceneManager.GetActiveScene().name != "MainMenu" &&
                    SceneManager.GetActiveScene().name != "InitialHarbor")
                    return;
                panel = new GameObject("D16AccessibilityPanel").AddComponent<D16AccessibilityPanel>();
            }

            panel.OpenInternal();
        }

        public static void HidePanel()
        {
            var panel = FindFirstObjectByType<D16AccessibilityPanel>(FindObjectsInactive.Include);
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
            _settings = D16AccessibilitySettingsStore.Current.Clone();
            BuildCanvas();
            BuildPanel();
            D16ModalFocusScope.Attach(_root, _closeButton == null ? null : _closeButton.gameObject);
            _root.SetActive(false);
            IsOpen = false;
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (_listeningDevice != ListeningDevice.None)
            {
                CaptureBinding();
                return;
            }

            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
                CloseInternal();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_inputBlockStateCaptured)
                D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            _inputBlockStateCaptured = false;
            IsOpen = false;
        }

        private void OpenInternal()
        {
            if (_root == null) return;
            if (IsOpen) return;

            // 统一用户可打开的 D16 模态层，避免旧模态仍在高层 Canvas 上拦截点击。
            D16LocalDiagnosticsPanel.HidePanel();
            D16FinanceDebtPanel.HidePanel();

            _settings = D16AccessibilitySettingsStore.Current.Clone();
            _listeningDevice = ListeningDevice.None;
            _refreshing = true;
            RefreshControls();
            _refreshing = false;

            _previousGameplayInputBlocked = D16InputRouter.GameplayInputBlocked;
            _inputBlockStateCaptured = true;
            IsOpen = true;
            D16InputRouter.SetGameplayInputBlocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _root.SetActive(true);
            Canvas.ForceUpdateCanvases();
            if (EventSystem.current != null && _closeButton != null)
                EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
            SetStatus(string.Empty);
            Debug.Log("[D16/A03] 无障碍设置面板已打开：玩法输入已锁定");
        }

        private void CloseInternal()
        {
            if (_root == null) return;
            _listeningDevice = ListeningDevice.None;
            IsOpen = false;
            if (_inputBlockStateCaptured)
                D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            _inputBlockStateCaptured = false;
            _root.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[D16/A03] 无障碍设置面板已关闭：玩法输入已恢复到打开前状态");
        }

        private void CaptureBinding()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _listeningDevice = ListeningDevice.None;
                SetStatus(string.Empty);
                return;
            }

            string value;
            if (_listeningDevice == ListeningDevice.Keyboard &&
                D16InputRouter.TryGetPressedKeyboardKey(out value))
            {
                FinishBinding(D16InputBindingDevice.PrimaryKeyboard, value);
            }
            else if (_listeningDevice == ListeningDevice.Gamepad &&
                     D16InputRouter.TryGetPressedGamepadButton(out value))
            {
                FinishBinding(D16InputBindingDevice.GamepadButton, value);
            }
        }

        private void FinishBinding(D16InputBindingDevice device, string value)
        {
            var candidate = _settings.Clone();
            string error;
            if (!candidate.TrySetBinding(_listeningAction, device, value, out error))
            {
                _listeningDevice = ListeningDevice.None;
                SetStatus(Localization.Get("a11y_binding_conflict"));
                Debug.LogWarning("[D16/A03] 重绑定被拒绝：" + error);
                return;
            }

            _settings = candidate;
            D16AccessibilitySettingsStore.Save(_settings);
            _listeningDevice = ListeningDevice.None;
            RefreshControls();
            SetStatus(Localization.Get("a11y_binding_saved"));
        }

        private void BeginBinding(D16InputAction action, ListeningDevice device)
        {
            _listeningAction = action;
            _listeningDevice = device;
            SetStatus(Localization.Get(device == ListeningDevice.Keyboard
                ? "a11y_wait_keyboard"
                : "a11y_wait_gamepad"));
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void RestoreDefaults()
        {
            D16AccessibilitySettingsStore.Reset();
            _settings = D16AccessibilitySettingsStore.Current.Clone();
            RefreshControls();
            SetStatus(Localization.Get("a11y_binding_saved"));
        }

        private void ApplySettings()
        {
            if (_refreshing) return;
            D16AccessibilitySettingsStore.Save(_settings);
            RefreshControls();
        }

        private void CycleColorVision()
        {
            var next = ((int)_settings.ColorVisionMode + 1) % 4;
            _settings.ColorVisionMode = (D16AccessibilityColorVisionMode)next;
            ApplySettings();
        }

        private void RefreshControls()
        {
            if (_settings == null) _settings = D16AccessibilitySettingsStore.Current.Clone();

            _refreshing = true;
            if (_textScaleSlider != null) _textScaleSlider.value = _settings.TextScale;
            if (_scaleValueText != null)
                _scaleValueText.text = _settings.TextScale.ToString("0.0x");
            if (_colorVisionButton != null)
                SetButtonText(_colorVisionButton, ColorVisionLabel(_settings.ColorVisionMode));
            if (_highContrastToggle != null) _highContrastToggle.isOn = _settings.HighContrast;
            if (_reducedMotionToggle != null) _reducedMotionToggle.isOn = _settings.ReducedMotion;
            if (_lowStimulusToggle != null) _lowStimulusToggle.isOn = _settings.LowStimulus;
            if (_subtitlesToggle != null) _subtitlesToggle.isOn = _settings.Subtitles;
            if (_subtitleSpeakerToggle != null) _subtitleSpeakerToggle.isOn = _settings.SubtitleSpeakerLabels;
            if (_sensitiveWarningsToggle != null) _sensitiveWarningsToggle.isOn = _settings.SensitiveWarnings;

            foreach (var action in RebindableActions)
            {
                var binding = FindBinding(action);
                if (binding == null) continue;
                if (_keyboardButtons.ContainsKey(action))
                    SetButtonText(_keyboardButtons[action], DisplayKey(binding.PrimaryKeyboard));
                if (_gamepadButtons.ContainsKey(action))
                    SetButtonText(_gamepadButtons[action], DisplayKey(binding.GamepadButton));
            }

            _refreshing = false;
        }

        private D16InputBinding FindBinding(D16InputAction action)
        {
            if (_settings == null || _settings.InputBindings == null) return null;
            foreach (var binding in _settings.InputBindings)
                if (binding != null && binding.Action == action) return binding;
            return null;
        }

        private void SetStatus(string value)
        {
            if (_statusText != null) _statusText.text = value ?? string.Empty;
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
            eventSystem.sendNavigationEvents = true;

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
                if (module != inputModule) module.enabled = false;
            eventSystem.UpdateModules();
        }

        private void BuildCanvas()
        {
            _canvasGo = new GameObject("D16AccessibilityCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGo.transform.SetParent(transform, false);
            var canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 800;
            var scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPanel()
        {
            _root = new GameObject("AccessibilityModal", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(_canvasGo.transform, false);
            Stretch((RectTransform)_root.transform);
            var overlay = _root.GetComponent<Image>();
            overlay.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            overlay.color = new Color(0.01f, 0.02f, 0.04f, 0.78f);
            overlay.raycastTarget = true;

            var card = new GameObject("AccessibilityCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_root.transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            // 全部核心与 D3/D4/D5/D9/D10 上下文动作在固定视口内滚动，底部按钮始终留在可命中区域。
            cardRect.sizeDelta = new Vector2(980f, 960f);
            card.GetComponent<Image>().sprite = Whalebone.Gameplay.UI.UiSprites.White;
            card.GetComponent<Image>().color = new Color(0.025f, 0.055f, 0.09f, 0.98f);

            CreateText(card, "Title", Localization.Get("a11y_title"), 42, new Vector2(0f, 420f));
            CreateText(card, "Subtitle", Localization.Get("a11y_subtitle"), 19, new Vector2(0f, 380f));
            CreateText(card, "ScaleLabel", Localization.Get("a11y_text_scale"), 22, new Vector2(-310f, 330f));
            _scaleValueText = CreateText(card, "ScaleValue", "1.0x", 20, new Vector2(330f, 330f));
            _textScaleSlider = CreateSlider(card, new Vector2(20f, 330f), 0.8f, 2f,
                value =>
                {
                    _settings.TextScale = value;
                    ApplySettings();
                });

            CreateToggleRow(card, "HighContrast", "a11y_high_contrast", 280f,
                _settings == null ? false : _settings.HighContrast,
                value => { _settings.HighContrast = value; ApplySettings(); });
            CreateText(card, "ColorLabel", Localization.Get("a11y_color_vision"), 20, new Vector2(-265f, 230f));
            _colorVisionButton = CreateButton(card, "ColorVision", ColorVisionLabel(D16AccessibilityColorVisionMode.None),
                new Vector2(130f, 230f), CycleColorVision, new Vector2(260f, 38f), 18);
            CreateToggleRow(card, "ReducedMotion", "a11y_reduced_motion", 180f, false,
                value => { _settings.ReducedMotion = value; ApplySettings(); });
            CreateToggleRow(card, "LowStimulus", "a11y_low_stimulus", 135f, false,
                value => { _settings.LowStimulus = value; ApplySettings(); });
            CreateToggleRow(card, "Subtitles", "a11y_subtitles", 90f, true,
                value => { _settings.Subtitles = value; ApplySettings(); });
            CreateToggleRow(card, "SubtitleSpeaker", "a11y_subtitle_speaker", 45f, true,
                value => { _settings.SubtitleSpeakerLabels = value; ApplySettings(); });
            CreateToggleRow(card, "SensitiveWarnings", "a11y_sensitive_warnings", 0f, true,
                value => { _settings.SensitiveWarnings = value; ApplySettings(); });

            CreateText(card, "BindingsTitle", Localization.Get("a11y_bindings_title"), 24, new Vector2(0f, -38f));
            CreateText(card, "KeyboardHeader", Localization.Get("a11y_keyboard"), 16, new Vector2(-65f, -70f));
            CreateText(card, "GamepadHeader", Localization.Get("a11y_gamepad"), 16, new Vector2(120f, -70f));
            BuildBindingRows(card);

            _statusText = CreateText(card, "Status", string.Empty, 18, new Vector2(0f, -410f));
            _statusText.color = new Color(1f, 0.78f, 0.35f);
            CreateButton(card, "Reset", Localization.Get("a11y_reset"), new Vector2(-170f, -450f),
                RestoreDefaults, new Vector2(250f, 42f), 18);
            _closeButton = CreateButton(card, "Close", Localization.Get("a11y_close"), new Vector2(170f, -450f),
                CloseInternal, new Vector2(250f, 42f), 18);
        }

        private void BuildBindingRows(GameObject card)
        {
            const float rowHeight = 35f;
            const float viewportHeight = 300f;
            float contentHeight = RebindableActions.Length * rowHeight;

            var scrollObject = new GameObject("BindingScroll", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(card.transform, false);
            var scrollRectTransform = (RectTransform)scrollObject.transform;
            scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
            scrollRectTransform.sizeDelta = new Vector2(820f, viewportHeight);
            scrollRectTransform.anchoredPosition = new Vector2(0f, -238f);

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            var viewport = (RectTransform)viewportObject.transform;
            Stretch(viewport);
            var viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            viewportImage.color = new Color(0.02f, 0.06f, 0.09f, 0.72f);

            var contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            var content = (RectTransform)contentObject.transform;
            content.anchorMin = new Vector2(0.5f, 1f);
            content.anchorMax = new Vector2(0.5f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(820f, contentHeight);
            content.anchoredPosition = Vector2.zero;

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35f;
            scroll.inertia = true;

            BuildBindingRows(contentObject, contentHeight, rowHeight);
        }

        private void BuildBindingRows(GameObject content, float contentHeight, float rowHeight)
        {
            for (int i = 0; i < RebindableActions.Length; i++)
            {
                var action = RebindableActions[i];
                float y = contentHeight * 0.5f - rowHeight * 0.5f - i * rowHeight;
                CreateText(content, "Action_" + action, ActionLabel(action), 17, new Vector2(-300f, y));
                var keyboard = CreateButton(content, "Keyboard_" + action, "", new Vector2(-65f, y),
                    () => BeginBinding(action, ListeningDevice.Keyboard), new Vector2(150f, 30f), 16);
                var gamepad = CreateButton(content, "Gamepad_" + action, "", new Vector2(120f, y),
                    () => BeginBinding(action, ListeningDevice.Gamepad), new Vector2(150f, 30f), 16);
                _keyboardButtons[action] = keyboard;
                _gamepadButtons[action] = gamepad;
            }
        }

        private void CreateToggleRow(GameObject card, string name, string labelKey, float y, bool fallback,
            Action<bool> onChanged)
        {
            var label = CreateText(card, name + "Label", Localization.Get(labelKey), 20, new Vector2(-265f, y));
            label.alignment = TextAnchor.MiddleLeft;
            var toggleGo = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleGo.transform.SetParent(card.transform, false);
            var rect = (RectTransform)toggleGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(60f, 34f);
            rect.anchoredPosition = new Vector2(285f, y);

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
            checkRect.sizeDelta = new Vector2(38f, 26f);
            check.GetComponent<Image>().sprite = Whalebone.Gameplay.UI.UiSprites.White;
            check.GetComponent<Image>().color = new Color(0.35f, 0.75f, 0.95f, 1f);

            var toggle = toggleGo.GetComponent<Toggle>();
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = fallback;
            toggle.onValueChanged.AddListener(value =>
            {
                if (!_refreshing) onChanged(value);
            });

            if (name == "HighContrast") _highContrastToggle = toggle;
            if (name == "ReducedMotion") _reducedMotionToggle = toggle;
            if (name == "LowStimulus") _lowStimulusToggle = toggle;
            if (name == "Subtitles") _subtitlesToggle = toggle;
            if (name == "SubtitleSpeaker") _subtitleSpeakerToggle = toggle;
            if (name == "SensitiveWarnings") _sensitiveWarningsToggle = toggle;
        }

        private Toggle _highContrastToggle;
        private Toggle _reducedMotionToggle;
        private Toggle _lowStimulusToggle;
        private Toggle _subtitlesToggle;
        private Toggle _subtitleSpeakerToggle;
        private Toggle _sensitiveWarningsToggle;

        private Slider CreateSlider(GameObject parent, Vector2 position, float min, float max,
            UnityEngine.Events.UnityAction<float> onChanged)
        {
            var go = new GameObject("TextScaleSlider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(480f, 26f);
            rect.anchoredPosition = position;
            var slider = go.GetComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = _settings == null ? 1f : _settings.TextScale;

            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(go.transform, false);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = new Vector2(0f, 0.25f);
            backgroundRect.anchorMax = new Vector2(1f, 0.75f);
            background.GetComponent<Image>().sprite = Whalebone.Gameplay.UI.UiSprites.White;
            background.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.24f, 0.95f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(go.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = new Vector2(0f, 0.25f);
            fillRect.anchorMax = new Vector2(0.5f, 0.75f);
            fill.GetComponent<Image>().sprite = Whalebone.Gameplay.UI.UiSprites.White;
            fill.GetComponent<Image>().color = new Color(0.3f, 0.65f, 0.9f, 0.95f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(go.transform, false);
            var handleRect = (RectTransform)handle.transform;
            handleRect.sizeDelta = new Vector2(24f, 36f);
            handle.GetComponent<Image>().sprite = Whalebone.Gameplay.UI.UiSprites.White;
            handle.GetComponent<Image>().color = new Color(0.75f, 0.92f, 1f, 1f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private Button CreateButton(GameObject parent, string name, string label, Vector2 position,
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

            var text = CreateText(go, "Label", label, fontSize, Vector2.zero);
            Stretch(text.rectTransform);
            return button;
        }

        private Text CreateText(GameObject parent, string name, string content, int size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(780f, size + 18f);
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

        private static void SetButtonText(Button button, string value)
        {
            if (button == null) return;
            var label = button.transform.Find("Label");
            var text = label == null ? null : label.GetComponent<Text>();
            if (text != null) text.text = value;
        }

        private static string DisplayKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "—" : value;
        }

        private static string ColorVisionLabel(D16AccessibilityColorVisionMode mode)
        {
            switch (mode)
            {
                case D16AccessibilityColorVisionMode.Deuteranopia: return Localization.Get("a11y_color_deuteranopia");
                case D16AccessibilityColorVisionMode.Protanopia: return Localization.Get("a11y_color_protanopia");
                case D16AccessibilityColorVisionMode.Tritanopia: return Localization.Get("a11y_color_tritanopia");
                default: return Localization.Get("a11y_color_none");
            }
        }

        private static string ActionLabel(D16InputAction action)
        {
            switch (action)
            {
                case D16InputAction.MoveForward: return Localization.Get("a11y_action_move_forward");
                case D16InputAction.MoveBack: return Localization.Get("a11y_action_move_back");
                case D16InputAction.MoveLeft: return Localization.Get("a11y_action_move_left");
                case D16InputAction.MoveRight: return Localization.Get("a11y_action_move_right");
                case D16InputAction.Sprint: return Localization.Get("a11y_action_sprint");
                case D16InputAction.Jump: return Localization.Get("a11y_action_jump");
                case D16InputAction.Interact: return Localization.Get("a11y_action_interact");
                case D16InputAction.Pause: return Localization.Get("a11y_action_pause");
                case D16InputAction.CameraCycle: return Localization.Get("a11y_action_camera_cycle");
                case D16InputAction.Map: return Localization.Get("a11y_action_map");
                case D16InputAction.Trade: return Localization.Get("a11y_action_trade");
                case D16InputAction.CharacterPreset: return Localization.Get("a11y_action_character_preset");
                case D16InputAction.Language: return Localization.Get("a11y_action_language");
                case D16InputAction.Quality: return Localization.Get("a11y_action_quality");
                case D16InputAction.Achievement: return Localization.Get("a11y_action_achievement");
                case D16InputAction.Save: return Localization.Get("a11y_action_save");
                case D16InputAction.D3CloseTrade: return Localization.Get("a11y_action_d3_close_trade");
                case D16InputAction.D3StartVoyage: return Localization.Get("a11y_action_d3_start_voyage");
                case D16InputAction.D3AdvanceVoyage: return Localization.Get("a11y_action_d3_advance_voyage");
                case D16InputAction.D3Gather: return Localization.Get("a11y_action_d3_gather");
                case D16InputAction.D3LandAndSettle: return Localization.Get("a11y_action_d3_land_settle");
                case D16InputAction.D4Faction: return Localization.Get("a11y_action_d4_faction");
                case D16InputAction.D4StartCombat: return Localization.Get("a11y_action_d4_start_combat");
                case D16InputAction.D4Reload: return Localization.Get("a11y_action_d4_reload");
                case D16InputAction.D4SelectRoundShot: return Localization.Get("a11y_action_d4_round_shot");
                case D16InputAction.D4SelectChainShot: return Localization.Get("a11y_action_d4_chain_shot");
                case D16InputAction.D4SelectGrapeShot: return Localization.Get("a11y_action_d4_grape_shot");
                case D16InputAction.D4Fire: return Localization.Get("a11y_action_d4_fire");
                case D16InputAction.D4Board: return Localization.Get("a11y_action_d4_board");
                case D16InputAction.D4EscapeCombat: return Localization.Get("a11y_action_d4_escape_combat");
                case D16InputAction.D4Upgrade: return Localization.Get("a11y_action_d4_upgrade");
                case D16InputAction.D4ObserveAnimal: return Localization.Get("a11y_action_d4_observe_animal");
                case D16InputAction.D4TreatCrew: return Localization.Get("a11y_action_d4_treat_crew");
                case D16InputAction.D4AdvanceMainline: return Localization.Get("a11y_action_d4_advance_mainline");
                case D16InputAction.D5DiscoverIsland: return Localization.Get("a11y_action_d5_discover_island");
                case D16InputAction.D5VisitIsland: return Localization.Get("a11y_action_d5_visit_island");
                case D16InputAction.D9OpenPortPreparation: return Localization.Get("a11y_action_d9_open_port");
                case D16InputAction.D9ClosePortPreparation: return Localization.Get("a11y_action_d9_close_port");
                case D16InputAction.D9EnterBerth: return Localization.Get("a11y_action_d9_enter_berth");
                case D16InputAction.D9BuySalt: return Localization.Get("a11y_action_d9_buy_salt");
                case D16InputAction.D9BuyTimber: return Localization.Get("a11y_action_d9_buy_timber");
                case D16InputAction.D9VisitTavern: return Localization.Get("a11y_action_d9_visit_tavern");
                case D16InputAction.D9BuyWater: return Localization.Get("a11y_action_d9_buy_water");
                case D16InputAction.D9RepairHull: return Localization.Get("a11y_action_d9_repair_hull");
                case D16InputAction.D9RecruitCrew: return Localization.Get("a11y_action_d9_recruit_crew");
                case D16InputAction.D9SettleWages: return Localization.Get("a11y_action_d9_settle_wages");
                case D16InputAction.D9Upgrade: return Localization.Get("a11y_action_d9_upgrade");
                case D16InputAction.D9Depart: return Localization.Get("a11y_action_d9_depart");
                case D16InputAction.D10OpenTavern: return Localization.Get("a11y_action_d10_open_tavern");
                case D16InputAction.D10CloseTavern: return Localization.Get("a11y_action_d10_close_tavern");
                case D16InputAction.D10Board: return Localization.Get("a11y_action_d10_board");
                case D16InputAction.D10SecurityJob: return Localization.Get("a11y_action_d10_security_job");
                case D16InputAction.D10Rest: return Localization.Get("a11y_action_d10_rest");
                case D16InputAction.D10Eat: return Localization.Get("a11y_action_d10_eat");
                case D16InputAction.D10Drink: return Localization.Get("a11y_action_d10_drink");
                case D16InputAction.D10Rumor: return Localization.Get("a11y_action_d10_rumor");
                case D16InputAction.D10Poker: return Localization.Get("a11y_action_d10_poker");
                case D16InputAction.D10Dance: return Localization.Get("a11y_action_d10_dance");
                case D16InputAction.D10NightTalk: return Localization.Get("a11y_action_d10_night_talk");
                case D16InputAction.D10Recruit: return Localization.Get("a11y_action_d10_recruit");
                case D16InputAction.D10SkipSensitive: return Localization.Get("a11y_action_d10_skip_sensitive");
                default: return action.ToString();
            }
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
