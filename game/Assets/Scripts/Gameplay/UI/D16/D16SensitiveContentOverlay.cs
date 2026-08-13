using System;
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
    /// D16-A03 的敏感内容表现层提示。
    ///
    /// 本类只显示警告并把“继续/跳过”结果回调给调用方，不写入存档、奖励、关系、经济
    /// 或任何领域事实。调用方必须先从所属系统取得 warning_required/content_warning，
    /// 再把已经本地化的警告文本交给本类。
    /// </summary>
    [DefaultExecutionOrder(-870)]
    public sealed class D16SensitiveContentOverlay : MonoBehaviour
    {
        public static D16SensitiveContentOverlay Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        private GameObject _canvasGo;
        private GameObject _modal;
        private Text _warningText;
        private Button _continueButton;
        private Button _skipButton;
        private Action _onContinue;
        private Action _onSkip;
        private bool _previousGameplayInputBlocked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (Instance != null) return;
            var existing = FindFirstObjectByType<D16SensitiveContentOverlay>(FindObjectsInactive.Include);
            if (existing != null) return;

            var host = new GameObject("D16SensitiveContentOverlay");
            DontDestroyOnLoad(host);
            host.AddComponent<D16SensitiveContentOverlay>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureEventSystem();
            BuildCanvas();
            BuildModal();
            D16ModalFocusScope.Attach(_modal, _continueButton == null ? null : _continueButton.gameObject);
            _modal.SetActive(false);
            IsOpen = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsOpen)
                D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            IsOpen = false;
        }

        private void Update()
        {
            if (!IsOpen) return;
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
                FinishSkip();
        }

        /// <summary>
        /// 根据 D16 敏感内容契约决定显示警告、直接继续或按设置跳过。
        /// `warningText` 必须来自已审查的内容定义；本类不替调用方判断历史真实性。
        /// </summary>
        public static D16SensitiveContentDecision PresentWarning(
            string warningText,
            bool warningRequired,
            Action onContinue,
            Action onSkip)
        {
            var settings = D16AccessibilitySettingsStore.Current;
            var decision = D16SensitiveContentPolicy.Decide(
                warningRequired,
                settings.SensitiveWarnings,
                false,
                false);

            if (decision == D16SensitiveContentDecision.ShowWarning)
            {
                var overlay = EnsureInstance();
                if (overlay == null)
                {
                    onSkip?.Invoke();
                    return D16SensitiveContentDecision.HiddenByPreference;
                }

                overlay.Open(warningText, onContinue, onSkip);
                return decision;
            }

            if (decision == D16SensitiveContentDecision.HiddenByPreference ||
                decision == D16SensitiveContentDecision.SkipContent)
            {
                onSkip?.Invoke();
                return decision;
            }

            onContinue?.Invoke();
            return decision;
        }

        /// <summary>
        /// 将敏感字幕交给同一警告链；跳过只清除表现，不产生任何领域结果。
        /// </summary>
        public static D16SensitiveContentDecision PublishSensitiveSubtitle(
            string subtitle,
            string warningText,
            string speaker = null,
            float duration = 4f,
            Action onSkipped = null)
        {
            return PresentWarning(
                warningText,
                true,
                () => D16SubtitleOverlay.PublishLocalized(subtitle, speaker, duration),
                () =>
                {
                    D16SubtitleOverlay.Clear();
                    onSkipped?.Invoke();
                });
        }

        public static void Hide()
        {
            if (Instance != null) Instance.CloseWithoutCallback();
        }

        private static D16SensitiveContentOverlay EnsureInstance()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<D16SensitiveContentOverlay>(FindObjectsInactive.Include);
            if (existing != null) return existing;

            var host = new GameObject("D16SensitiveContentOverlay");
            DontDestroyOnLoad(host);
            return host.AddComponent<D16SensitiveContentOverlay>();
        }

        private void Open(string warningText, Action onContinue, Action onSkip)
        {
            if (_modal == null) return;

            _onContinue = onContinue;
            _onSkip = onSkip;
            _previousGameplayInputBlocked = D16InputRouter.GameplayInputBlocked;
            _warningText.text = string.IsNullOrWhiteSpace(warningText)
                ? Localization.Get("a11y_sensitive_body")
                : warningText.Trim();
            _modal.SetActive(true);
            IsOpen = true;
            D16InputRouter.SetGameplayInputBlocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Canvas.ForceUpdateCanvases();
            if (EventSystem.current != null && _continueButton != null)
                EventSystem.current.SetSelectedGameObject(_continueButton.gameObject);
            Debug.Log("[D16/A03] 敏感内容警告已打开：等待继续或跳过");
        }

        private void FinishContinue()
        {
            var callback = _onContinue;
            CloseWithoutCallback();
            callback?.Invoke();
        }

        private void FinishSkip()
        {
            var callback = _onSkip;
            CloseWithoutCallback();
            callback?.Invoke();
        }

        private void CloseWithoutCallback()
        {
            if (_modal != null) _modal.SetActive(false);
            IsOpen = false;
            D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            _onContinue = null;
            _onSkip = null;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
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
                if (module != inputModule) module.enabled = false;
            eventSystem.UpdateModules();
        }

        private void BuildCanvas()
        {
            _canvasGo = new GameObject("D16SensitiveContentCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGo.transform.SetParent(transform, false);
            var canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 950;
            var scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildModal()
        {
            _modal = new GameObject("SensitiveContentModal", typeof(RectTransform), typeof(Image));
            _modal.transform.SetParent(_canvasGo.transform, false);
            Stretch((RectTransform)_modal.transform);
            var modalImage = _modal.GetComponent<Image>();
            modalImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            modalImage.color = new Color(0.01f, 0.02f, 0.04f, 0.92f);
            modalImage.raycastTarget = true;

            var card = new GameObject("SensitiveContentCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_modal.transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(900f, 470f);
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            cardImage.color = new Color(0.025f, 0.055f, 0.09f, 0.99f);

            CreateText(card, "Title", Localization.Get("a11y_sensitive_title"), 38,
                new Vector2(0f, 155f), new Vector2(780f, 60f));
            CreateText(card, "Body", Localization.Get("a11y_sensitive_body"), 21,
                new Vector2(0f, 90f), new Vector2(780f, 58f));
            _warningText = CreateText(card, "Warning", string.Empty, 24,
                new Vector2(0f, 5f), new Vector2(780f, 125f));
            _warningText.color = new Color(1f, 0.82f, 0.42f, 1f);

            _continueButton = CreateButton(card, "Continue", Localization.Get("a11y_sensitive_continue"),
                new Vector2(-190f, -145f), FinishContinue, new Vector2(300f, 54f), 19);
            _skipButton = CreateButton(card, "Skip", Localization.Get("a11y_sensitive_skip"),
                new Vector2(190f, -145f), FinishSkip, new Vector2(300f, 54f), 19);
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

        private static Text CreateText(GameObject parent, string name, string content, int size,
            Vector2 position, Vector2 dimensions)
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
