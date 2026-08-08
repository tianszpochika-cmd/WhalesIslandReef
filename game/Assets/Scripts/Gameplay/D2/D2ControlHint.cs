using UnityEngine;
using UnityEngine.UI;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.D2
{
    public sealed class D2ControlHint : MonoBehaviour
    {
        private Text _text;
        private D2ControlRouter _router;

        public void Initialize(D2ControlRouter router)
        {
            _router = router;
            if (_text == null) Build();
        }

        private void Update()
        {
            if (_text != null && _router != null) _text.text = _router.Hint;
        }

        private void Build()
        {
            var canvasGo = new GameObject("D2HintCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var textGo = new GameObject("HintText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)textGo.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(32f, -28f);
            rt.sizeDelta = new Vector2(900f, 44f);
            _text = textGo.GetComponent<Text>();
            _text.font = Fonts.Default;
            _text.fontSize = 20;
            _text.alignment = TextAnchor.MiddleLeft;
            _text.color = new Color(1f, 1f, 1f, 0.88f);
            _text.raycastTarget = false;
        }
    }
}
