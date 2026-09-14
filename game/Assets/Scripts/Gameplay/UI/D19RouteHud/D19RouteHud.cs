// ============================================================
// D19RouteHud —— D19 玩家 HUD 与路线反馈轻量显示
//
// 只读显示 D2ControlRouter.Hint、D3 当前状态和成功/失败反馈。
// 不得由 HUD 改变任何交易、航行、存档或任务事实。
// 仅在玩家模式下显示，D5 调试面板默认隐藏。
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.UI.D19RouteHud
{
    public sealed class D19RouteHud : MonoBehaviour
    {
        private Text _hintText;
        private Text _statusText;
        private Text _feedbackText;
        private GameObject _canvas;

        private void Awake()
        {
            EnsureEventSystem();
            BuildCanvas();
            BuildUI();
            _canvas.SetActive(false);
        }

        private void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<EventSystem>();
            }
            // This legacy HUD is display-only.  HarborInteractionStabilizer and
            // D29 own the single InputSystemUIInputModule; adding a
            // StandaloneInputModule here lets W/S navigate the visible
            // shortcut buttons while the same key is moving the player.
        }

        private void BuildCanvas()
        {
            _canvas = new GameObject("D19RouteHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas.transform.SetParent(transform, false);
            var canvas = _canvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150; // below D5 but above D2/D3
            var scaler = _canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildUI()
        {
            var panel = new GameObject("RoutePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_canvas.transform, false);
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 54f);
            rt.sizeDelta = new Vector2(820f, 104f);
            var panelImage = panel.GetComponent<Image>();
            panelImage.sprite = UiSprites.White;
            panelImage.color = new Color(0.02f, 0.03f, 0.08f, 0.78f);

            _hintText = CreateText(panel, "Hint", "WASD 移动，E 交互", 20, new Vector2(-20f, 22f), TextAnchor.MiddleLeft);
            _statusText = CreateText(panel, "Status", "港口航行已就绪", 16, new Vector2(0f, -4f), TextAnchor.MiddleCenter);
            _feedbackText = CreateText(panel, "Feedback", "", 15, new Vector2(0f, -28f), TextAnchor.MiddleCenter);
        }

        private static Text CreateText(GameObject parent, string name, string content, int size, Vector2 position, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(820f, size + 28f);
            var text = go.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        public void UpdateHud(string hint, string status, string feedback = "")
        {
            _hintText.text = hint;
            _statusText.text = status;
            _feedbackText.text = feedback;
            if (!_canvas.activeSelf) _canvas.SetActive(true);
        }

        public void Hide()
        {
            if (_canvas.activeSelf) _canvas.SetActive(false);
        }
    }
}
