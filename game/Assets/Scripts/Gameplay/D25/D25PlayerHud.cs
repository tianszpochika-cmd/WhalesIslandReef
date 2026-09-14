using System;
using UnityEngine;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.D25
{
    /// <summary>
    /// D25 玩家模式的单一信息层。它只显示 D2 输入提示和交互回执，
    /// 不直接调用领域系统，也不接受鼠标射线，避免 HUD 抢占游戏交互。
    /// </summary>
    public sealed class D25PlayerHud : MonoBehaviour
    {
        private static D25PlayerHud _active;

        private GameObject _canvasObject;
        private Text _titleText;
        private Text _statusText;
        private Text _hintText;
        private Text _feedbackText;
        private Image _feedbackImage;
        private D2ControlRouter _router;
        private int _lastFeedbackSequence;
        private float _nextRefresh;
        private float _feedbackUntil;

        public static bool IsActive => _active != null && _active.isActiveAndEnabled;

        private void Awake()
        {
            if (_active != null && _active != this)
            {
                Destroy(gameObject);
                return;
            }

            _active = this;
            BuildCanvas();
            BuildUi();
            D25VisualTheme.LogTextDiagnostics("D25PlayerHud", _canvasObject.transform);
        }

        private void OnDestroy()
        {
            if (_active == this) _active = null;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.15f;
            if (_router == null)
                _router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            Refresh(_router);
        }

        private void BuildCanvas()
        {
            _canvasObject = new GameObject("D25PlayerHudCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            _canvasObject.transform.SetParent(transform, false);

            var canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;

            var scaler = _canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var group = _canvasObject.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.ignoreParentGroups = true;

            var raycaster = _canvasObject.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;
        }

        private void BuildUi()
        {
            var titlePanel = CreatePanel("HarborTitle", new Vector2(24f, -24f), new Vector2(390f, 78f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.82f));
            _titleText = CreateText(titlePanel.transform, "Title", "圣玛格丽特港 · 1675", 24,
                new Vector2(18f, -12f), new Vector2(350f, 32f), TextAnchor.UpperLeft, D25VisualTheme.Paper);
            _statusText = CreateText(titlePanel.transform, "Status", "港口 · 步行中", 15,
                new Vector2(18f, -46f), new Vector2(350f, 24f), TextAnchor.UpperLeft,
                D25VisualTheme.WithAlpha(D25VisualTheme.Paper, 0.82f));

            var hintPanel = CreatePanel("InteractionHint", new Vector2(0f, 30f), new Vector2(820f, 66f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.78f));
            _hintText = CreateText(hintPanel.transform, "Hint", "WASD 移动  ·  Shift 奔跑  ·  E 交互  ·  Esc 暂停", 17,
                new Vector2(0f, 0f), new Vector2(790f, 54f), TextAnchor.MiddleCenter, D25VisualTheme.Paper);

            var feedbackPanel = CreatePanel("Feedback", new Vector2(24f, 118f), new Vector2(650f, 54f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.76f));
            _feedbackImage = feedbackPanel.GetComponent<Image>();
            _feedbackText = CreateText(feedbackPanel.transform, "Text", string.Empty, 16,
                new Vector2(14f, 0f), new Vector2(620f, 50f), TextAnchor.MiddleLeft, D25VisualTheme.Paper);
        }

        private GameObject CreatePanel(string name, Vector2 position, Vector2 size,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_canvasObject.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = panel.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string content, int size,
            Vector2 position, Vector2 dimensions, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>();
            D25VisualTheme.ConfigureText(text, content, size, alignment, color);
            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.62f);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private void Refresh(D2ControlRouter router)
        {
            if (router == null)
            {
                _statusText.text = "港口 · 等待输入路由";
                return;
            }

            string mode = router.Mode.ToString();
            _statusText.text = string.Equals(mode, "Ship", StringComparison.OrdinalIgnoreCase)
                ? "港口 · 船舵控制中"
                : string.Equals(mode, "Locked", StringComparison.OrdinalIgnoreCase)
                    ? "港口 · 界面已暂停行动"
                    : "港口 · 步行中";
            _hintText.text = ShortHint(router.Hint);

            var feedback = router.ReadLastInteractionFeedback();
            if (feedback != null && feedback.Sequence > _lastFeedbackSequence)
            {
                _lastFeedbackSequence = feedback.Sequence;
                if (feedback.Sequence > 0 && !string.Equals(feedback.Code, "router_ready", StringComparison.Ordinal))
                {
                    _feedbackText.text = (feedback.Succeeded ? "✓ " : "⚠ ") + feedback.Message;
                    _feedbackImage.color = feedback.Succeeded
                        ? D25VisualTheme.WithAlpha(new Color(0.10f, 0.30f, 0.23f), 0.86f)
                        : D25VisualTheme.WithAlpha(new Color(0.40f, 0.22f, 0.10f), 0.90f);
                    _feedbackUntil = Time.unscaledTime + 4.5f;
                }
            }

            bool visible = Time.unscaledTime < _feedbackUntil;
            _feedbackText.enabled = visible;
            _feedbackImage.enabled = visible;
        }

        private static string ShortHint(string hint)
        {
            if (string.IsNullOrWhiteSpace(hint)) return "WASD 移动  ·  E 交互  ·  Esc 暂停";
            if (hint.IndexOf("已上船", StringComparison.Ordinal) >= 0)
                return "W/S 加减速  ·  A/D 转向  ·  E 下船  ·  F3 切换视角  ·  Esc 暂停";
            if (hint.IndexOf("F3", StringComparison.Ordinal) >= 0)
                return "WASD 移动  ·  Shift 奔跑  ·  E 交互  ·  F3 视角  ·  Esc 暂停";
            return "WASD 移动  ·  Shift 奔跑  ·  E 交互  ·  F3 视角  ·  Esc 暂停";
        }
    }
}
