using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.D25;
using Whalebone.Systems.D2;

namespace Whalebone.Gameplay.D30
{
    /// <summary>
    /// D30 视觉路线协调器。
    /// 它只把 D2/D3 的公开只读状态投影为路线进度条和过渡提示，不推进航行、
    /// 不保存、不修改库存/关系/天气事实。没有状态 owner 时显示明确的等待态。
    /// </summary>
    public sealed class D30VisualLoopCoordinator : MonoBehaviour
    {
        public enum VisualStage
        {
            Harbor = 0,
            Market = 1,
            Tavern = 2,
            Shipyard = 3,
            Ship = 4,
            Voyage = 5,
            Return = 6,
        }

        private const string RootName = "D30VisualLoopCoordinator";
        private Text _stageText;
        private Text _transitionText;
        private D2ControlRouter _router;
        private D3RuntimeBootstrap _d3;
        private VisualStage _stage;
        private float _nextRefresh;
        private float _transitionUntil;
        private string _lastEventId = string.Empty;
        private float _lastEventTime = -1f;

        private const float RepeatGuardSeconds = 0.35f;

        public static VisualStage CurrentStage
        {
            get
            {
                var instance = FindFirstObjectByType<D30VisualLoopCoordinator>(FindObjectsInactive.Include);
                return instance == null ? VisualStage.Harbor : instance._stage;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D30VisualLoopCoordinator>(FindObjectsInactive.Include) != null)
                return;
            new GameObject(RootName).AddComponent<D30VisualLoopCoordinator>();
        }

        private void Awake()
        {
            BuildOverlay();
        }

        private void Update()
        {
            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + 0.2f;
                ResolveOwners();
                RefreshStage();
            }
            if (_transitionText != null)
                _transitionText.enabled = Time.unscaledTime < _transitionUntil;
        }

        public static void Notify(string eventId)
        {
            var instance = FindFirstObjectByType<D30VisualLoopCoordinator>(FindObjectsInactive.Include);
            if (instance == null) return;
            instance.NotifyInternal(eventId);
        }

        private void ResolveOwners()
        {
            if (_router == null) _router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
        }

        private void RefreshStage()
        {
            // 港内的 Market/Tavern/Shipyard 是表现事件投影，不会直接出现在
            // D3 航行枚举中；没有新的 owner 状态覆盖时必须保留它们，避免
            // Notify 后下一次轮询立刻把路线条闪回“港口”。
            var next = _stage;
            if (_d3 != null && _d3.IsReady && _d3.Snapshot != null)
            {
                string status = _d3.Snapshot.VoyageStatus.ToString();
                if (status.IndexOf("Sail", StringComparison.OrdinalIgnoreCase) >= 0)
                    next = VisualStage.Voyage;
                else if (status.IndexOf("Arriv", StringComparison.OrdinalIgnoreCase) >= 0)
                    next = VisualStage.Return;
                else if (_stage == VisualStage.Ship || _stage == VisualStage.Voyage || _stage == VisualStage.Return)
                    next = VisualStage.Harbor;
            }
            if (_router != null && _router.Mode == D2ControlMode.Ship)
                next = VisualStage.Ship;
            if (next != _stage)
            {
                _stage = next;
                RefreshText();
            }
        }

        private void NotifyInternal(string eventId)
        {
            var decision = D30VisualStagePolicy.Evaluate((int)_stage, eventId,
                _lastEventId, Time.unscaledTime, _lastEventTime, RepeatGuardSeconds);
            if (!string.IsNullOrEmpty(decision.NormalizedEventId))
            {
                _lastEventId = decision.NormalizedEventId;
                _lastEventTime = Time.unscaledTime;
            }

            _stage = (VisualStage)decision.NextStage;
            var duplicate = decision.IsDuplicate;
            var stageChanged = decision.IsStageAdvance;
            if (_transitionText != null)
            {
                _transitionText.text = duplicate
                    ? "当前：" + StageLabel(_stage) + " · 重复事件已忽略，状态未改变"
                    : stageChanged
                    ? StageLabel(_stage) + " · 已切换表现上下文"
                    : "当前：" + StageLabel(_stage) + " · 请求未通过，状态未改变";
            }
            _transitionUntil = Time.unscaledTime + 2.4f;
            RefreshText();
        }

        private void RefreshText()
        {
            if (_stageText == null) return;
            _stageText.text = "港口  ›  市场  ›  酒馆  ›  船坞  ›  船上  ›  航行  ›  返港\n" +
                "当前：" + StageLabel(_stage);
        }

        private static string StageLabel(VisualStage stage)
        {
            switch (stage)
            {
                case VisualStage.Market: return "市场";
                case VisualStage.Tavern: return "酒馆";
                case VisualStage.Shipyard: return "船坞";
                case VisualStage.Ship: return "船上";
                case VisualStage.Voyage: return "航行";
                case VisualStage.Return: return "返港";
                default: return "港口";
            }
        }

        private void BuildOverlay()
        {
            var canvasObject = new GameObject("D30VisualLoopCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 145;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var raycaster = canvasObject.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            var panel = new GameObject("StagePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            // D25 owns the harbor title in the top-left and the legacy world
            // title may occupy the top-center safe zone. Keep the route
            // progress band below that zone so it cannot cover either title
            // in a 16:9 Player window.
            panelRect.anchoredPosition = new Vector2(0f, -174f);
            panelRect.sizeDelta = new Vector2(730f, 66f);
            var image = panel.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.72f);
            image.raycastTarget = false;
            _stageText = CreateText(panel.transform, "Stage", string.Empty, 14,
                new Vector2(0f, 5f), new Vector2(710f, 42f), TextAnchor.MiddleCenter,
                D25VisualTheme.Paper);
            _transitionText = CreateText(panel.transform, "Transition", string.Empty, 13,
                new Vector2(0f, -23f), new Vector2(710f, 22f), TextAnchor.MiddleCenter,
                D25VisualTheme.Accent);
            _transitionText.enabled = false;
            RefreshText();
            D25VisualTheme.LogTextDiagnostics("D30VisualLoop", canvasObject.transform);
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
            outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }
    }
}
