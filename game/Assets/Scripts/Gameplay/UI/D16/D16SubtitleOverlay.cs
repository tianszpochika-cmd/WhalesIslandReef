using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// 轻量字幕显示层。音频/对话表现层只需发布已经本地化的文本，
    /// 字幕开关和说话者标签由 D16 设置统一裁剪。
    /// </summary>
    public sealed class D16SubtitleOverlay : MonoBehaviour
    {
        public static D16SubtitleOverlay Instance { get; private set; }
        public static bool IsVisible { get; private set; }

        private GameObject _root;
        private Text _text;
        private float _hideAt;
        private string _lastText;
        private string _lastSpeaker;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            D16AccessibilitySettingsStore.Changed += OnSettingsChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            BuildCanvas();
            Hide();
        }

        private void OnDestroy()
        {
            D16AccessibilitySettingsStore.Changed -= OnSettingsChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this) Instance = null;
            IsVisible = false;
        }

        private void Update()
        {
            if (IsVisible && _hideAt > 0f && Time.unscaledTime >= _hideAt)
                Hide();
        }

        public static void PublishKey(string textKey, string speakerKey = null, float duration = 4f)
        {
            PublishLocalized(Localization.Get(textKey),
                string.IsNullOrWhiteSpace(speakerKey) ? null : Localization.Get(speakerKey), duration);
        }

        public static void PublishLocalized(string subtitle, string speaker = null, float duration = 4f)
        {
            var overlay = EnsureInstance();
            if (overlay == null) return;
            if (!D16AccessibilitySettingsStore.Current.Subtitles || string.IsNullOrWhiteSpace(subtitle))
            {
                overlay.Hide();
                return;
            }

            overlay._lastText = subtitle.Trim();
            overlay._lastSpeaker = speaker;
            overlay._text.text = overlay.FormatText();
            overlay._hideAt = duration <= 0f ? 0f : Time.unscaledTime + duration;
            overlay._root.SetActive(true);
            IsVisible = true;
        }

        public static void Clear()
        {
            if (Instance != null) Instance.Hide();
        }

        private static D16SubtitleOverlay EnsureInstance()
        {
            if (Instance != null) return Instance;
            var runtime = FindFirstObjectByType<D16AccessibilityRuntime>(FindObjectsInactive.Include);
            if (runtime != null)
                return runtime.GetComponent<D16SubtitleOverlay>() ?? runtime.gameObject.AddComponent<D16SubtitleOverlay>();
            var go = new GameObject("D16AccessibilityRuntime");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<D16AccessibilityRuntime>();
            var created = go.GetComponent<D16SubtitleOverlay>();
            return created == null ? go.AddComponent<D16SubtitleOverlay>() : created;
        }

        private void OnSettingsChanged(D16AccessibilitySettings settings)
        {
            if (settings == null || !settings.Subtitles)
            {
                Hide();
                return;
            }

            if (IsVisible && _text != null)
                _text.text = FormatText();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Hide();
        }

        private string FormatText()
        {
            var settings = D16AccessibilitySettingsStore.Current;
            if (settings.SubtitleSpeakerLabels && !string.IsNullOrWhiteSpace(_lastSpeaker))
                return "[" + _lastSpeaker.Trim() + "]\n" + _lastText;
            return _lastText ?? string.Empty;
        }

        private void Hide()
        {
            if (_root != null) _root.SetActive(false);
            IsVisible = false;
            _hideAt = 0f;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("D16SubtitleCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 650;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            _root = new GameObject("SubtitleRoot", typeof(RectTransform), typeof(Image));
            _root.transform.SetParent(canvasGo.transform, false);
            var rootRect = (RectTransform)_root.transform;
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.sizeDelta = new Vector2(1050f, 132f);
            rootRect.anchoredPosition = new Vector2(0f, 80f);
            var background = _root.GetComponent<Image>();
            background.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            background.color = new Color(0.01f, 0.025f, 0.04f, 0.9f);
            background.raycastTarget = false;

            var textGo = new GameObject("SubtitleText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(_root.transform, false);
            var textRect = (RectTransform)textGo.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(28f, 14f);
            textRect.offsetMax = new Vector2(-28f, -14f);
            _text = textGo.GetComponent<Text>();
            _text.font = Whalebone.Gameplay.UI.Fonts.Default;
            _text.fontSize = 28;
            _text.alignment = TextAnchor.MiddleCenter;
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Overflow;
            _text.color = Color.white;
            _text.raycastTarget = false;
        }
    }
}
