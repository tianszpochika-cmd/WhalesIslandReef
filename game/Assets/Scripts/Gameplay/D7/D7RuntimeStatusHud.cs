// ============================================================
// D7RuntimeStatusHud —— 首航控制反馈
//
// 只显示当前控制权、动作、速度、视角和模态锁，不接收鼠标射线，
// 让玩家能直接判断“按键没反应”是暂停/对话/贸易锁定还是控制权变化。
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using Whalebone.Gameplay;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.Dialogue;

namespace Whalebone.Gameplay.D7
{
    public sealed class D7RuntimeStatusHud : MonoBehaviour
    {
        private Text _text;
        private float _nextRefresh;

        public bool IsReady => _text != null;

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            if (_text == null || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.12f;

            var coordinator = D7ControlCoordinator.Instance;
            var router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            var player = GameObject.Find("D2_PlayerAvatar");
            var animation = player == null ? null : player.GetComponent<D2CharacterAnimationController>();
            var character = player == null ? null : player.GetComponent<D2CharacterController>();
            var camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();
            var follow = camera == null ? null : camera.GetComponent<CameraFollow>();

            string authority = coordinator == null ? "人物" : coordinator.AuthorityLabel;
            string modal = coordinator == null ? "无" : coordinator.ModalLabel;
            string action = animation == null ? "待机" : ActionLabel(animation.Action);
            string speed = character == null ? "0.0" : character.CurrentSpeed.ToString("F1");
            string view = follow == null ? "人物视角" : follow.ViewModeLabel;
            _text.text = "D7 控制  |  " + authority + "  |  动作：" + action +
                         "  |  速度：" + speed + "  |  视角：" + view +
                         (modal == "无" ? string.Empty : "  |  输入锁：" + modal);
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D7StatusCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var textGo = new GameObject("D7Status", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(canvasGo.transform, false);
            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-28f, -74f);
            rect.sizeDelta = new Vector2(760f, 32f);
            _text = textGo.GetComponent<Text>();
            _text.font = Fonts.Default;
            _text.fontSize = 16;
            _text.alignment = TextAnchor.UpperRight;
            _text.color = new Color(0.92f, 0.96f, 1f, 0.88f);
            _text.raycastTarget = false;
        }

        private static string ActionLabel(D2CharacterAction action)
        {
            switch (action)
            {
                case D2CharacterAction.Walk: return "走路";
                case D2CharacterAction.Run: return "奔跑";
                case D2CharacterAction.Jump: return "跳跃";
                case D2CharacterAction.Boarding: return "上船";
                case D2CharacterAction.Disembarking: return "下船";
                default: return "待机";
            }
        }
    }
}
