// ============================================================
// D2DialogueController —— 游戏内船员对话 UI
//
// 归属：Gameplay/UI（表现层）。AgentCore 只负责产生经过校验的 AgentReply；
// 本类显示结果并发出 ReplyReceived 事件，绝不直接改好感或 SQLite。
// ============================================================

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Whalebone.Agent;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.UI.Dialogue
{
    public sealed class D2DialogueController : MonoBehaviour
    {
        public event Action<CrewAgent, AgentReply> ReplyReceived;
        public event Action<bool> ControlLockChanged;

        private GameObject _canvas;
        private GameObject _panel;
        private Text _speakerText;
        private Text _replyText;
        private Text _statusText;
        private InputField _input;
        private Button _sendButton;
        private Button _closeButton;
        private CrewAgent _agent;
        private DialogueSession _session;
        private OllamaClient _local;
        private AgentReply _lastReply;
        private bool _open;
        private bool _busy;

        public bool IsOpen => _open;
        public CrewAgent CurrentAgent => _agent;
        /// <summary>D9 对话页只读最近一次已校验回复；不包含任何裁决后的关系结果。</summary>
        public AgentReply LastReply => _lastReply;

        private void Awake()
        {
            EnsureEventSystem();
            BuildCanvas();
            BuildPanel();
            _canvas.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_open) ControlLockChanged?.Invoke(false);
        }

        public void Open(CrewAgent agent)
        {
            if (agent == null) return;
            EnsureSession(agent);
            _agent = agent;
            _open = true;
            _busy = false;
            _canvas.SetActive(true);
            RefreshLocalizedButtons();
            _speakerText.text = agent.DisplayName;
            _replyText.text = "老水手看向你，等你开口。";
            _statusText.text = "本地 AI 优先；不可用时自动使用模板回复。";
            _input.text = string.Empty;
            _input.ActivateInputField();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ControlLockChanged?.Invoke(true);
            Debug.Log("[D2/Dialogue] 对话打开：" + agent.DisplayName);
        }

        public void Close()
        {
            if (!_open) return;
            _open = false;
            _busy = false;
            _canvas.SetActive(false);
            ControlLockChanged?.Invoke(false);
            Debug.Log("[D2/Dialogue] 对话关闭");
        }

        private void EnsureSession(CrewAgent agent)
        {
            if (_session != null && _agent == agent) return;

            _local = new OllamaClient();
            var mock = new MockLLMClient(
                AgentReplyTestFixtures.ValidNone.Replace("海风正好。", "海风正好，船长。"),
                AgentReplyTestFixtures.ValidNone.Replace("海风正好。", "这片海我熟得很，跟紧潮流。").Replace("happy", "neutral"),
                AgentReplyTestFixtures.ValidNone.Replace("海风正好。", "你愿意听老人的话，我很欣慰。"));
            var router = new LLMRouter(_local, mock);
            _session = new DialogueSession(agent, router, new MutinyMonitor(agent.Stats));

            // 探测只启动一次；若本地服务不存在，LLMRouter 会立即使用 Mock，不阻塞 UI。
            _ = ProbeLocalAsync();
        }

        private async Task ProbeLocalAsync()
        {
            if (_local == null) return;
            try { await _local.ProbeAsync(); }
            catch { /* LLM 不可用由 Router/模板兜底 */ }
        }

        private async void Submit()
        {
            if (!_open || _busy || _session == null) return;
            string playerInput = string.IsNullOrWhiteSpace(_input.text) ? "天气如何？" : _input.text.Trim();
            _busy = true;
            _sendButton.interactable = false;
            _input.interactable = false;
            _replyText.text = "老水手思索中……";
            _statusText.text = "正在等待本地模型或模板回复";

            try
            {
                var reply = await _session.SpeakAsync(playerInput);
                _lastReply = reply;
                _replyText.text = reply.Speech;
                string delta = reply.AffinityChange == 0f
                    ? "好感无变化"
                    : "好感提议 " + reply.AffinityChange.ToString("+#;-#;0");
                _statusText.text = "情绪：" + reply.Emotion + "　" + delta + "　（由逻辑系统裁决）";
                ReplyReceived?.Invoke(_agent, reply);
                Debug.Log("[D2/Dialogue] 回复完成：emotion=" + reply.Emotion + ", affinityProposal=" + reply.AffinityChange);
            }
            catch (Exception ex)
            {
                _replyText.text = "老水手暂时没有回应，但航程不会被阻塞。";
                _statusText.text = "对话异常已降级：" + ex.GetType().Name;
                Debug.LogWarning("[D2/Dialogue] 对话异常，继续使用可玩兜底：" + ex.Message);
            }
            finally
            {
                _busy = false;
                _sendButton.interactable = true;
                _input.interactable = true;
                _input.ActivateInputField();
            }
        }

        private void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                eventSystem = go.AddComponent<EventSystem>();
            }

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
        }

        private void BuildCanvas()
        {
            _canvas = new GameObject("D2DialogueCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas.transform.SetParent(transform, false);
            var canvas = _canvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = _canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPanel()
        {
            _panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(_canvas.transform, false);
            var panelRt = (RectTransform)_panel.transform;
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = new Vector2(0f, -250f);
            panelRt.sizeDelta = new Vector2(1100f, 330f);
            var panelImage = _panel.GetComponent<Image>();
            panelImage.sprite = UiSprites.White;
            panelImage.color = new Color(0.015f, 0.035f, 0.06f, 0.94f);

            _speakerText = CreateText(_panel, "Speaker", "老水手", 30, new Vector2(-430f, 125f), TextAnchor.MiddleLeft);
            _replyText = CreateText(_panel, "Reply", "", 28, new Vector2(0f, 55f), TextAnchor.MiddleCenter);
            _statusText = CreateText(_panel, "Status", "", 20, new Vector2(0f, -12f), TextAnchor.MiddleCenter);
            _input = CreateInput(_panel, new Vector2(-145f, -82f));
            _sendButton = CreateButton(_panel, "Send", Localization.Get("dialogue_send"), new Vector2(300f, -82f), Submit);
            _closeButton = CreateButton(_panel, "Close", Localization.Get("dialogue_close"), new Vector2(440f, -82f), Close);
        }

        private void RefreshLocalizedButtons()
        {
            SetButtonLabel(_sendButton, "dialogue_send");
            SetButtonLabel(_closeButton, "dialogue_close");
        }

        private static void SetButtonLabel(Button button, string key)
        {
            if (button == null) return;
            var text = button.GetComponentInChildren<Text>(true);
            if (text != null) text.text = Localization.Get(key);
        }

        private static Text CreateText(GameObject parent, string name, string content, int size,
            Vector2 position, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(850f, size + 28f);
            var text = go.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        private static InputField CreateInput(GameObject parent, Vector2 position)
        {
            var go = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(430f, 58f);
            var image = go.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.08f, 0.12f, 0.18f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = (RectTransform)textGo.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(14f, 0f);
            textRt.offsetMax = new Vector2(-14f, 0f);
            var text = textGo.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            placeholderGo.transform.SetParent(go.transform, false);
            var placeholderRt = (RectTransform)placeholderGo.transform;
            placeholderRt.anchorMin = Vector2.zero;
            placeholderRt.anchorMax = Vector2.one;
            placeholderRt.offsetMin = new Vector2(14f, 0f);
            placeholderRt.offsetMax = new Vector2(-14f, 0f);
            var placeholder = placeholderGo.GetComponent<Text>();
            placeholder.font = Fonts.Default;
            placeholder.fontSize = 24;
            placeholder.color = new Color(1f, 1f, 1f, 0.45f);
            placeholder.text = "输入你想问老水手的话……";

            var field = go.GetComponent<InputField>();
            field.textComponent = text;
            field.placeholder = placeholder;
            field.lineType = InputField.LineType.SingleLine;
            return field;
        }

        private static Button CreateButton(GameObject parent, string name, string label,
            Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(120f, 58f);
            var image = go.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.18f, 0.38f, 0.58f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var text = labelGo.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            text.raycastTarget = false;
            return button;
        }
    }
}
