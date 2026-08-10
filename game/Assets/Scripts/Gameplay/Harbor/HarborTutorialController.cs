// ============================================================
// HarborTutorialController —— InitialHarbor 新手引导闭环
//
// 任务是引导器，不裁决任何经济、航行或好感数值。它只观察既有
// D2/D3/D5 状态，按真实操作推进提示，确保玩家能走完第一条航程。
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D5;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.Dialogue;
using Whalebone.Systems.D3;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.Harbor
{
    public sealed class HarborTutorialController : MonoBehaviour
    {
        private enum TutorialStep
        {
            Move,
            Speak,
            Board,
            Sail,
            Disembark,
            StartVoyage,
            AdvanceVoyage,
            Land,
            Gather,
            Trade,
            Save,
            Complete,
        }

        private static readonly string[] StepNames =
        {
            "移动", "对话", "上船", "操船", "下船", "规划航程", "推进航程", "靠港", "采集", "贸易", "保存"
        };

        private TutorialStep _step = TutorialStep.Move;
        private Transform _player;
        private Transform _ship;
        private Vector3 _startPlayerPosition;
        private Vector3 _sailStartPosition;
        private bool _dialogueOpened;
        private bool _tradeOpened;
        private bool _restoredFromSave;
        private int _saveRevisionAtStep;
        private float _nextUiRefresh;

        private Text _title;
        private Text _stepText;
        private Text _detailText;
        private GameObject _canvas;

        public bool IsComplete => _step == TutorialStep.Complete;
        public string CurrentStep => _step.ToString();
        public int StepIndex => (int)_step;
        public int StepCount => StepNames.Length;
        public float Progress => IsComplete ? 1f : Mathf.Clamp01((float)StepIndex / StepCount);
        public bool ResumeStateApplied => _restoredFromSave;

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            LocateObjects();
            if (_player == null) return;

            AdvanceByObservedState();
            if (Time.unscaledTime >= _nextUiRefresh)
            {
                _nextUiRefresh = Time.unscaledTime + 0.18f;
                RefreshUi();
            }
        }

        private void LocateObjects()
        {
            if (_player == null)
            {
                var player = GameObject.Find("D2_PlayerAvatar");
                if (player != null)
                {
                    _player = player.transform;
                    _startPlayerPosition = _player.position;
                }
            }
            if (_ship == null)
            {
                var ship = GameObject.Find("ph_ship_fishing");
                if (ship != null) _ship = ship.transform;
            }
            RestoreFromSavedState();
        }

        private void RestoreFromSavedState()
        {
            if (_restoredFromSave || _player == null || _ship == null) return;
            _restoredFromSave = true;

            var saved = SaveSystem.LoadD3Runtime();
            if (saved == null) return;
            if (saved.VoyageStatus == D3VoyageStatus.Sailing)
            {
                _step = TutorialStep.AdvanceVoyage;
            }
            else if (saved.VoyageStatus == D3VoyageStatus.Arrived)
            {
                _step = TutorialStep.Land;
            }
            else if (saved.CurrentLocationId != PortCatalog.StartingPortId)
            {
                _step = HasGathered(saved) ? TutorialStep.Trade : TutorialStep.Gather;
            }

            if (_step != TutorialStep.Move)
            {
                Debug.Log("[D7/Tutorial] 已根据存档恢复引导步骤：" + StepNames[(int)_step]);
                RefreshUi();
            }
        }

        private void AdvanceByObservedState()
        {
            var router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            var dialogue = FindFirstObjectByType<D2DialogueController>(FindObjectsInactive.Include);
            var d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            var d5 = FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include);
            var shipController = FindFirstObjectByType<D2ShipController>(FindObjectsInactive.Include);

            switch (_step)
            {
                case TutorialStep.Move:
                    if (HorizontalDistance(_player.position, _startPlayerPosition) >= 1.2f)
                        SetStep(TutorialStep.Speak);
                    break;

                case TutorialStep.Speak:
                    if (dialogue != null && dialogue.IsOpen) _dialogueOpened = true;
                    if (_dialogueOpened && (dialogue == null || !dialogue.IsOpen))
                        SetStep(TutorialStep.Board);
                    break;

                case TutorialStep.Board:
                    if (router != null && router.Mode == Whalebone.Systems.D2.D2ControlMode.Ship)
                    {
                        _sailStartPosition = _ship == null ? Vector3.zero : _ship.position;
                        SetStep(TutorialStep.Sail);
                    }
                    break;

                case TutorialStep.Sail:
                    if (router != null && router.Mode == Whalebone.Systems.D2.D2ControlMode.Ship &&
                        shipController != null &&
                        (_ship == null || HorizontalDistance(_ship.position, _sailStartPosition) >= 1.25f ||
                         Mathf.Abs(shipController.Snapshot.Speed) >= 0.1f))
                        SetStep(TutorialStep.Disembark);
                    break;

                case TutorialStep.Disembark:
                    if (router != null && router.Mode == Whalebone.Systems.D2.D2ControlMode.Character)
                        SetStep(TutorialStep.StartVoyage);
                    break;

                case TutorialStep.StartVoyage:
                    if (d3 != null && d3.IsReady && d3.Snapshot != null &&
                        d3.Snapshot.VoyageStatus == D3VoyageStatus.Sailing)
                        SetStep(TutorialStep.AdvanceVoyage);
                    break;

                case TutorialStep.AdvanceVoyage:
                    if (d3 != null && d3.IsReady && d3.Snapshot != null &&
                        d3.Snapshot.VoyageStatus == D3VoyageStatus.Arrived)
                        SetStep(TutorialStep.Land);
                    break;

                case TutorialStep.Land:
                    if (d3 != null && d3.IsReady && d3.Snapshot != null &&
                        d3.Snapshot.VoyageStatus == D3VoyageStatus.InPort &&
                        d3.Snapshot.CurrentLocationId != PortCatalog.StartingPortId)
                        SetStep(TutorialStep.Gather);
                    break;

                case TutorialStep.Gather:
                    if (d3 != null && d3.IsReady && HasGathered(d3.Snapshot))
                        SetStep(TutorialStep.Trade);
                    break;

                case TutorialStep.Trade:
                    if (d3 != null && d3.IsTradeOpen) _tradeOpened = true;
                    if (_tradeOpened && (d3 == null || !d3.IsTradeOpen))
                        SetStep(TutorialStep.Save);
                    break;

                case TutorialStep.Save:
                    if (d5 != null && d5.IsReady && d5.SaveRevision > _saveRevisionAtStep)
                        SetStep(TutorialStep.Complete);
                    break;

                case TutorialStep.Complete:
                    break;
            }
        }

        private static bool HasGathered(D3RuntimeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.GatheredThisVisit == null) return false;
            foreach (var item in snapshot.GatheredThisVisit)
                if (item.Value > 0) return true;
            return false;
        }

        private void SetStep(TutorialStep next)
        {
            if (_step == next) return;
            _step = next;
            if (_step == TutorialStep.Sail && _ship != null) _sailStartPosition = _ship.position;
            if (_step == TutorialStep.Save)
            {
                var d5 = FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include);
                _saveRevisionAtStep = d5 == null ? 0 : d5.SaveRevision;
            }
            if (_step == TutorialStep.Complete)
            {
                Debug.Log("[D7/Tutorial] 新手引导闭环完成：移动、对话、上下船、操船、航程、采集、贸易、保存");
            }
            else
            {
                Debug.Log("[D7/Tutorial] 进入步骤：" + StepNames[(int)_step]);
            }
            RefreshUi();
        }

        private void BuildUi()
        {
            _canvas = new GameObject("D6TutorialCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            _canvas.transform.SetParent(transform, false);
            var canvas = _canvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 240;
            var scaler = _canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var group = _canvas.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            var panel = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_canvas.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(28f, -88f);
            panelRect.sizeDelta = new Vector2(620f, 168f);
            var panelImage = panel.GetComponent<Image>();
            panelImage.sprite = UiSprites.White;
            panelImage.color = new Color(0.015f, 0.045f, 0.065f, 0.88f);
            panelImage.raycastTarget = false;

            _title = CreateText(panel, "Title", 24, new Vector2(24f, -18f), new Vector2(570f, 36f), TextAnchor.UpperLeft);
            _stepText = CreateText(panel, "Step", 20, new Vector2(24f, -58f), new Vector2(570f, 32f), TextAnchor.UpperLeft);
            _detailText = CreateText(panel, "Detail", 17, new Vector2(24f, -94f), new Vector2(570f, 58f), TextAnchor.UpperLeft);
            RefreshUi();
        }

        private void RefreshUi()
        {
            if (_title == null) return;
            bool english = Localization.IsEnglish;
            if (_step == TutorialStep.Complete)
            {
                _title.text = english ? "First voyage complete" : "第一条航程完成";
                _stepText.text = english ? "You now know the basic harbor loop." : "你已经掌握港口的基础循环。";
                _detailText.text = english ? "Keep exploring: trade, talk with crew, and save often."
                    : "接下来可以自由探索：贸易、对话、航行，并记得经常保存。";
                return;
            }

            int number = (int)_step + 1;
            _title.text = english ? "First Voyage Tutorial" : "第一次航程引导";
            _stepText.text = (english ? "Step " + number + "/11 · " : "步骤 " + number + "/11 · ") +
                (english ? EnglishStepName(_step) : StepNames[(int)_step]);
            _detailText.text = english ? EnglishInstruction(_step) : ChineseInstruction(_step);
        }

        private static string ChineseInstruction(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Move: return "用 WASD 移动人物，先走几步熟悉港口。";
                case TutorialStep.Speak: return "走到老水手身边，按 E 对话；完成后关闭对话面板。";
                case TutorialStep.Board: return "点击右侧“上船/下船 [E]”，或靠近船后按 E。";
                case TutorialStep.Sail: return "上船后按住 W 航行一小段，再按 E 下船。";
                case TutorialStep.Disembark: return "按 E 下船，回到人物控制。";
                case TutorialStep.StartVoyage: return "按 T 规划一条航线，开始 D3 航程。";
                case TutorialStep.AdvanceVoyage: return "按 N 推进航程，直到状态显示已抵达。";
                case TutorialStep.Land: return "按 L 靠港并完成航程结算。";
                case TutorialStep.Gather: return "按 G 采集一次当地资源。";
                case TutorialStep.Trade: return "按 B 打开市场，浏览或买卖一次后关闭贸易面板。";
                case TutorialStep.Save: return "按 F5 或点击右侧“保存状态 [F5]”。";
                default: return string.Empty;
            }
        }

        private static string EnglishInstruction(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Move: return "Use WASD to move around the harbor.";
                case TutorialStep.Speak: return "Reach the old sailor, press E, then close the dialogue.";
                case TutorialStep.Board: return "Click Board/Leave ship [E], or press E near the boat.";
                case TutorialStep.Sail: return "Hold W to sail a short distance, then press E to leave.";
                case TutorialStep.Disembark: return "Press E to return to character control.";
                case TutorialStep.StartVoyage: return "Press T to plan and start a D3 voyage.";
                case TutorialStep.AdvanceVoyage: return "Press N until the voyage reports arrival.";
                case TutorialStep.Land: return "Press L to land and settle the voyage.";
                case TutorialStep.Gather: return "Press G to gather one local resource.";
                case TutorialStep.Trade: return "Press B, perform one market action, then close trade.";
                case TutorialStep.Save: return "Press F5 or click Save state [F5].";
                default: return string.Empty;
            }
        }

        private static string EnglishStepName(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Move: return "Move";
                case TutorialStep.Speak: return "Talk";
                case TutorialStep.Board: return "Board";
                case TutorialStep.Sail: return "Sail";
                case TutorialStep.Disembark: return "Disembark";
                case TutorialStep.StartVoyage: return "Plan voyage";
                case TutorialStep.AdvanceVoyage: return "Advance voyage";
                case TutorialStep.Land: return "Land";
                case TutorialStep.Gather: return "Gather";
                case TutorialStep.Trade: return "Trade";
                case TutorialStep.Save: return "Save";
                default: return "Complete";
            }
        }

        private static Text CreateText(GameObject parent, string name, int size, Vector2 position,
            Vector2 sizeDelta, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = sizeDelta;
            var text = go.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
