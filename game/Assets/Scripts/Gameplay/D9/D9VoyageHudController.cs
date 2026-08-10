// ============================================================
// D9VoyageHudController —— D9 航行 HUD、日志、任务与提示
//
// 这是只读表现层：读取 D3/D4/D9 快照、D2 已校验对话回复和 D8 EventLog。
// 所有按钮/系统数值写入仍由既有领域入口负责，本类不提交裁决请求。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Agent;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D4;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.Dialogue;

namespace Whalebone.Gameplay.D9
{
    public sealed class D9VoyageHudController : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D9VoyageHudController>(FindObjectsInactive.Include) != null) return;
            new GameObject("D9VoyageHudController").AddComponent<D9VoyageHudController>();
        }

        private readonly D9EventFactCursor _cursor = new D9EventFactCursor();
        private D3RuntimeBootstrap _d3;
        private D4RuntimeBootstrap _d4;
        private D9PortShipPreparationBootstrap _preparation;
        private D2DialogueController _dialogue;
        private AgentReply _lastReply;
        private Text _header;
        private Text _shipText;
        private Text _flowText;
        private Text _journalText;
        private float _nextRefresh;
        private D9VoyagePresentationSnapshot _snapshot;

        public bool IsReady => _header != null;
        public int RefreshCount { get; private set; }
        public D9VoyagePresentationSnapshot Snapshot => _snapshot;

        private void Awake()
        {
            BuildUi();
        }

        private void OnDestroy()
        {
            if (_dialogue != null) _dialogue.ReplyReceived -= OnReplyReceived;
        }

        private void Update()
        {
            BindRuntimeObjects();
            if (!IsReady || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.12f;
            Refresh();
        }

        private void BindRuntimeObjects()
        {
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d4 == null) _d4 = FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_preparation == null)
                _preparation = FindFirstObjectByType<D9PortShipPreparationBootstrap>(FindObjectsInactive.Include);

            var dialogue = FindFirstObjectByType<D2DialogueController>(FindObjectsInactive.Include);
            if (ReferenceEquals(_dialogue, dialogue)) return;
            if (_dialogue != null) _dialogue.ReplyReceived -= OnReplyReceived;
            _dialogue = dialogue;
            if (_dialogue != null)
            {
                _dialogue.ReplyReceived += OnReplyReceived;
                _lastReply = _dialogue.LastReply;
            }
        }

        private void OnReplyReceived(CrewAgent agent, AgentReply reply)
        {
            _lastReply = reply;
            Refresh();
        }

        private void Refresh()
        {
            var eventLog = D9EventLogBridge.Current;
            _cursor.Bind(eventLog);
            // 消费游标只推进事实序列；展示内容仍完整读取 EventLog 副本。
            _cursor.ConsumeNewFacts();
            _snapshot = D9PresentationReadModel.Build(
                _d3 == null ? null : _d3.Snapshot,
                _d4 == null ? null : _d4.Snapshot,
                _preparation == null ? null : _preparation.Snapshot,
                eventLog,
                _lastReply);

            _header.text = "D9 航行 HUD  |  " + _snapshot.Location + " → " + _snapshot.Destination +
                "  |  " + _snapshot.VoyageStatus + "  |  " + _snapshot.Weather +
                "  |  " + _snapshot.Money;
            _shipText.text = "船舶状态：" + _snapshot.Ship + "\n" + _snapshot.Crew;
            _flowText.text = "市场：" + _snapshot.Market +
                "\n鱼获：" + _snapshot.Catch +
                "\n船员对话：" + _snapshot.Dialogue +
                "\n任务日志：" + _snapshot.Quest +
                "\n天气提示：" + _snapshot.WeatherPrompt +
                "\n战况：" + _snapshot.Combat;

            var journal = "统一事实日志（EventLog，只读）\n";
            if (_snapshot.Facts.Count == 0)
            {
                journal += "尚未接收到 D8 事实；本页不会生成私有日志。";
            }
            else
            {
                for (int i = 0; i < _snapshot.Facts.Count; i++)
                {
                    var fact = _snapshot.Facts[i];
                    journal += "#" + fact.LogSequence + " " + fact.EventId + " | " + fact.Text +
                        " | owner=" + fact.OwnerSystem + "\n";
                }
            }
            journal += "存档反馈：" + _snapshot.SaveRestore;
            _journalText.text = journal;
            RefreshCount++;
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("D9VoyageHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 230;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _header = CreateText(canvasObject, "Header", new Vector2(30f, -30f), new Vector2(1080f, 38f), 20, TextAnchor.UpperLeft);
            _shipText = CreateText(canvasObject, "ShipStatus", new Vector2(30f, -78f), new Vector2(1120f, 115f), 17, TextAnchor.UpperLeft);
            _flowText = CreateText(canvasObject, "FlowStatus", new Vector2(30f, -210f), new Vector2(1200f, 190f), 16, TextAnchor.UpperLeft);
            _journalText = CreateText(canvasObject, "Journal", new Vector2(30f, -425f), new Vector2(1220f, 300f), 14, TextAnchor.UpperLeft);
        }

        private static Text CreateText(GameObject parent, string name, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.90f, 0.95f, 1f, 0.90f);
            text.raycastTarget = false;
            return text;
        }
    }
}
