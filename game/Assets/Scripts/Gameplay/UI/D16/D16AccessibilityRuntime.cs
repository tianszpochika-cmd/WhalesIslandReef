using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A03 的轻量运行时适配器：在场景切换和设置变化后统一应用文字缩放、换行和高对比度。
    /// 具体玩法 UI 仍由所属模块创建，本类不写玩法数据。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class D16AccessibilityRuntime : MonoBehaviour
    {
        private static D16AccessibilityRuntime _instance;
        private readonly Dictionary<int, int> _baseFontSizes = new Dictionary<int, int>();
        private readonly Dictionary<int, Color> _baseGraphicColors = new Dictionary<int, Color>();
        private Coroutine _applyRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (_instance != null) return;
            var existing = FindFirstObjectByType<D16AccessibilityRuntime>(FindObjectsInactive.Include);
            if (existing != null)
            {
                _instance = existing;
                return;
            }

            var go = new GameObject("D16AccessibilityRuntime");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<D16AccessibilityRuntime>();
            go.AddComponent<D16SubtitleOverlay>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            if (GetComponent<D16SubtitleOverlay>() == null)
                gameObject.AddComponent<D16SubtitleOverlay>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            D16AccessibilitySettingsStore.Changed += OnSettingsChanged;
        }

        private void Start()
        {
            QueueApply();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            D16AccessibilitySettingsStore.Changed -= OnSettingsChanged;
            if (_instance == this) _instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            QueueApply();
        }

        private void OnSettingsChanged(Whalebone.Systems.D16.D16AccessibilitySettings settings)
        {
            QueueApply();
        }

        private void QueueApply()
        {
            if (_applyRoutine != null) StopCoroutine(_applyRoutine);
            _applyRoutine = StartCoroutine(ApplyNextFrame());
        }

        private IEnumerator ApplyNextFrame()
        {
            yield return null;
            ApplyToSceneUi();
            _applyRoutine = null;
        }

        private void ApplyToSceneUi()
        {
            var settings = D16AccessibilitySettingsStore.Load();
            var liveIds = new HashSet<int>();
            foreach (var graphic in FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (graphic == null || graphic.gameObject == gameObject) continue;
                var id = graphic.GetInstanceID();
                liveIds.Add(id);
                if (!_baseGraphicColors.ContainsKey(id)) _baseGraphicColors[id] = graphic.color;

                var text = graphic as Text;
                if (text != null)
                {
                    if (!_baseFontSizes.ContainsKey(id)) _baseFontSizes[id] = text.fontSize;
                    text.fontSize = Mathf.Max(1, Mathf.RoundToInt(_baseFontSizes[id] * settings.TextScale));
                    text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                    if (text.font == null) text.font = Whalebone.Gameplay.UI.Fonts.Default;
                    var fallbackText = text.GetComponent<D16FontFallbackText>();
                    if (fallbackText == null) fallbackText = text.gameObject.AddComponent<D16FontFallbackText>();
                    fallbackText.RefreshNow();
                }

                var baseColor = _baseGraphicColors[id];
                graphic.color = text != null && settings.HighContrast
                    ? Color.white
                    : ApplyVisualColor(baseColor, settings);
            }

            var stale = new List<int>();
            foreach (var id in _baseGraphicColors.Keys)
                if (!liveIds.Contains(id)) stale.Add(id);
            foreach (var id in stale)
            {
                _baseFontSizes.Remove(id);
                _baseGraphicColors.Remove(id);
            }

            ApplyReducedMotion(settings);

            Debug.Log("[D16/A03] UI settings applied: locale=" + settings.Locale +
                      ", textScale=" + settings.TextScale.ToString("0.##") +
                      ", colorVision=" + settings.ColorVisionMode +
                      ", reducedMotion=" + settings.ReducedMotion +
                      ", subtitles=" + settings.Subtitles +
                      ", lowStimulus=" + settings.LowStimulus);
        }

        private static Color ApplyVisualColor(Color source, Whalebone.Systems.D16.D16AccessibilitySettings settings)
        {
            float r = source.r;
            float g = source.g;
            float b = source.b;
            switch (settings.ColorVisionMode)
            {
                case Whalebone.Systems.D16.D16AccessibilityColorVisionMode.Deuteranopia:
                    r = 0.625f * source.r + 0.375f * source.g;
                    g = 0.7f * source.r + 0.3f * source.g;
                    b = source.b;
                    break;
                case Whalebone.Systems.D16.D16AccessibilityColorVisionMode.Protanopia:
                    r = 0.567f * source.r + 0.433f * source.g;
                    g = 0.558f * source.r + 0.442f * source.g;
                    b = source.b;
                    break;
                case Whalebone.Systems.D16.D16AccessibilityColorVisionMode.Tritanopia:
                    r = source.r;
                    g = 0.95f * source.g + 0.05f * source.b;
                    b = 0.433f * source.g + 0.567f * source.b;
                    break;
            }

            var mapped = new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), source.a);
            if (settings.LowStimulus)
            {
                var neutral = new Color(0.16f, 0.19f, 0.23f, mapped.a);
                mapped = Color.Lerp(neutral, mapped, 0.58f);
            }
            return mapped;
        }

        private static void ApplyReducedMotion(Whalebone.Systems.D16.D16AccessibilitySettings settings)
        {
            foreach (var animator in FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (animator == null || animator.GetComponentInParent<Canvas>() == null) continue;
                animator.speed = settings.ReducedMotion ? 0f : 1f;
            }
        }
    }

    /// <summary>
    /// 适配 UGUI legacy Text 的整段文本字体回退。UGUI Text 只能绑定一个 Font，
    /// 因此这里在文本变化时选择能完整覆盖整段内容的候选字体，不拆分或伪造字符。
    /// </summary>
    public sealed class D16FontFallbackText : MonoBehaviour
    {
        private Text _text;
        private string _lastText;
        private Font _lastObservedFont;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void LateUpdate()
        {
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (_text == null) _text = GetComponent<Text>();
            if (_text == null) return;
            var value = _text.text ?? string.Empty;
            if (value == _lastText && _text.font == _lastObservedFont) return;
            _text.font = Whalebone.Gameplay.UI.Fonts.ResolveForText(value, _text.font);
            _lastText = value;
            _lastObservedFont = _text.font;
        }
    }
}
