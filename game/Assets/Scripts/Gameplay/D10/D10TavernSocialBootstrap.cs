// ============================================================
// D10TavernSocialBootstrap —— InitialHarbor 酒馆社会场景表现层入口
//
// UI 只提交 D10TavernSocialSystem 请求并显示 owner 返回结果；
// 金钱、健康、疲劳、关系和招募结果不在 MonoBehaviour 内修改。
// V 打开/关闭酒馆社会页；敏感内容首击显示警告，二次确认或跳过。
// ============================================================

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D3;
using Whalebone.Systems.D16;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D10
{
    public sealed class D10TavernSocialBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D10TavernSocialBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D10TavernSocialBootstrap").AddComponent<D10TavernSocialBootstrap>();
            Debug.Log("[D10-A06] 酒馆社会场景入口已创建");
        }

        private D3RuntimeBootstrap _d3;
        private ShipState _ship;
        private D10TavernSocialSystem _tavern;
        private D10CrewBehaviorSystem _crewBehavior;
        private D10RelationSystem _relations;
        private D9PortShipPreparationSystem _preparation;
        private D10TavernSceneSnapshot _scene;
        private bool _ready;
        private bool _pageOpen;
        private bool _entered;
        private bool _alcoholAcknowledged;
        private bool _gamblingAcknowledged;
        private bool _socialAcknowledged;
        private bool _sideJobAcknowledged;
        private string _lastMessage = "酒馆社会页等待 D3 航行状态。";
        private Text _statusText;
        private Text _catalogText;
        private Text _hintText;
        private GameObject _panel;

        private void Awake()
        {
            EnsureEventSystem();
            BuildUi();
        }

        private void Update()
        {
            if (!_ready)
            {
                TryBindRuntime();
                return;
            }

            if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10OpenTavern))
                TogglePage();
            if (_panel != null) _panel.SetActive(_pageOpen);
            if (_pageOpen && D7ControlCoordinator.GameplayInputAllowed)
            {
                if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10CloseTavern)) _pageOpen = false;
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Board)) OpenBoard();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10SecurityJob)) TakeSecurityJob();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Rest)) Rest();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Eat)) Eat();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Drink)) Drink();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Rumor)) HearRumor();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Poker)) Poker();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Dance)) Dance();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10NightTalk)) NightTalk();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10Recruit)) Recruit();
                else if (D16InputRouter.WasPressedThisFrame(D16InputAction.D10SkipSensitive)) SkipSensitive();
            }
            Refresh();
        }

        private void TryBindRuntime()
        {
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            _ship = GameState.CurrentShip;
            if (_d3 == null || !_d3.IsReady || _d3.TradeState == null || _ship == null) return;

            var eventLog = _d3.EconomySystem.EventLog;
            _tavern = new D10TavernSocialSystem(eventLog);
            _crewBehavior = new D10CrewBehaviorSystem(eventLog);
            _relations = new D10RelationSystem(eventLog);
            _preparation = new D9PortShipPreparationSystem(_d3.EconomySystem, eventLog);
            _scene = _tavern.BuildSceneSnapshot(CurrentPortId(), CurrentWorldDay());
            _ready = true;
            Refresh();
        }

        private void TogglePage()
        {
            _pageOpen = !_pageOpen;
            if (_pageOpen) EnsureEntered();
        }

        /// <summary>D9 港口页的兼容跳转入口；真正行为仍由本页请求/owner 链结算。</summary>
        public void OpenSocialPage()
        {
            _pageOpen = true;
            if (_ready) EnsureEntered();
            if (_panel != null) _panel.SetActive(true);
        }

        private void EnsureEntered()
        {
            if (_entered || !_ready) return;
            var result = _tavern.EnterTavern(ActorId(), CurrentPortId(), CurrentWorldDay(), CurrentRevision());
            if (!result.Accepted)
            {
                _lastMessage = result.Message;
                return;
            }
            var resolution = _tavern.Resolve(result.Request, BuildOwners());
            _entered = resolution.Accepted;
            _lastMessage = resolution.Message;
        }

        private void Recruit()
        {
            Submit(() => _tavern.RequestRecruitment(ActorId(), CurrentPortId(),
                D9CrewRecruitmentCatalog.IsaacReedId, CurrentWorldDay(), CurrentRevision()));
        }

        private void Rest()
        {
            var crewId = SelectedCrewId();
            if (crewId == null) { _lastMessage = "当前没有可休息的船员。"; return; }
            Submit(() => _tavern.RequestRest(ActorId(), CurrentPortId(), crewId,
                D10TavernConfiguration.DefaultRestHours,
                CurrentWorldDay(), CurrentRevision()));
        }

        private void Eat()
        {
            var crewId = SelectedCrewId();
            if (crewId == null) { _lastMessage = "当前没有可用餐的船员。"; return; }
            Submit(() => _tavern.RequestMeal(ActorId(), CurrentPortId(), crewId,
                CurrentWorldDay(), CurrentRevision()));
        }

        private void Drink()
        {
            var crewId = SelectedCrewId();
            if (crewId == null) { _lastMessage = "当前没有可饮酒的船员。"; return; }
            Submit(() => _tavern.RequestAlcohol(ActorId(), CurrentPortId(), crewId,
                D10TavernConfiguration.DefaultAlcoholStrength, _alcoholAcknowledged,
                CurrentWorldDay(), CurrentRevision()));
            _alcoholAcknowledged = true;
        }

        private void HearRumor()
        {
            if (_scene == null || _scene.Rumors.Count == 0) return;
            var rumor = _scene.Rumors[0];
            Submit(() => _tavern.HearRumor(ActorId(), CurrentPortId(), rumor.RumorId,
                CurrentWorldDay(), CurrentRevision()));
        }

        private void Poker()
        {
            var tableId = _scene == null || _scene.PokerTables.Count == 0
                ? "poker_table_low_stakes_01" : _scene.PokerTables[0].TableId;
            Submit(() => _tavern.RequestPokerEntry(ActorId(), CurrentPortId(), tableId,
                _gamblingAcknowledged, CurrentWorldDay(), CurrentRevision()));
            _gamblingAcknowledged = true;
        }

        private void Dance()
        {
            var crewId = SelectedCrewId();
            if (crewId == null) { _lastMessage = "当前没有可邀请参加舞会的船员。"; return; }
            Submit(() => _tavern.RequestDance(ActorId(), CurrentPortId(), crewId,
                _socialAcknowledged, CurrentWorldDay(), CurrentRevision()));
            _socialAcknowledged = true;
        }

        private void NightTalk()
        {
            var crewId = SelectedCrewId();
            if (crewId == null) { _lastMessage = "当前没有可进行夜话的船员。"; return; }
            Submit(() => _tavern.RequestNightTalk(ActorId(), CurrentPortId(), crewId,
                _socialAcknowledged, CurrentWorldDay(), CurrentRevision()));
            _socialAcknowledged = true;
        }

        private void OpenBoard()
        {
            Submit(() => _tavern.OpenSideJobBoard(ActorId(), CurrentPortId(),
                CurrentWorldDay(), CurrentRevision()));
        }

        private void TakeSecurityJob()
        {
            Submit(() => _tavern.RequestSideJobOffer(ActorId(), CurrentPortId(),
                "sidejob_dock_fight_01", _sideJobAcknowledged,
                CurrentWorldDay(), CurrentRevision()));
            _sideJobAcknowledged = true;
        }

        private void SkipSensitive()
        {
            var skipped = _tavern.SkipSensitiveContent(D10TavernSensitivity.ExploitativeWork);
            _lastMessage = skipped.Message;
            _alcoholAcknowledged = false;
            _gamblingAcknowledged = false;
            _socialAcknowledged = false;
            _sideJobAcknowledged = false;
        }

        private void Submit(Func<D10TavernRequestResult> requestFactory)
        {
            var request = requestFactory == null ? null : requestFactory();
            if (request == null)
            {
                _lastMessage = "酒馆没有返回请求结果。";
                return;
            }
            if (!request.Accepted)
            {
                _lastMessage = request.Status == D10TavernRequestStatus.WarningRequired
                    ? request.Warning + " 再次按同一按钮确认，或按 X 跳过。"
                    : request.Message;
                return;
            }

            var resolution = _tavern.Resolve(request.Request, BuildOwners());
            _lastMessage = resolution.Message;
            if (resolution.Accepted && _d3 != null)
            {
                _d3.CommitPreparationState();
                if (resolution.Relation != null && resolution.Relation.Succeeded && GameState.CurrentPlayer != null)
                {
                    try { SaveSystem.SaveD10Runtime(resolution.Relation.Snapshot); }
                    catch (Exception ex) { Debug.LogWarning("[D10-A06] 关系快照保存失败：" + ex.Message); }
                }
            }
        }

        private D10TavernOwnerServices BuildOwners()
        {
            return new D10TavernOwnerServices
            {
                Economy = _d3.EconomySystem,
                TradeState = _d3.TradeState,
                Ship = _ship,
                Preparation = _preparation,
                CrewBehavior = _crewBehavior,
                Relations = _relations,
                Crew = GameState.CrewAgents,
                RecruitCandidateResolver = candidateId => candidateId == D9CrewRecruitmentCatalog.IsaacReedId
                    ? D9CrewRecruitmentCatalog.CreateIsaacReed() : null,
                RelationTargetId = ActorId(),
            };
        }

        private string SelectedCrewId()
        {
            return GameState.CrewAgents == null || GameState.CrewAgents.Count == 0
                ? null : GameState.CrewAgents[0].Id;
        }

        private string ActorId()
        {
            return GameState.CurrentPlayer == null || string.IsNullOrEmpty(GameState.CurrentPlayer.Id)
                ? "player:captain" : GameState.CurrentPlayer.Id;
        }

        private string CurrentPortId()
        {
            return _d3 == null || _d3.Snapshot == null || string.IsNullOrEmpty(_d3.Snapshot.CurrentLocationId)
                ? PortCatalog.StartingPortId : _d3.Snapshot.CurrentLocationId;
        }

        private int CurrentWorldDay()
        {
            return _d3 == null || _d3.Snapshot == null ? 0 : Math.Max(0, _d3.Snapshot.CurrentDay);
        }

        private long CurrentRevision()
        {
            return _d3 == null || _d3.Snapshot == null ? 0 : Math.Max(0, _d3.Snapshot.SaveRevision);
        }

        private void Refresh()
        {
            if (!_ready || _d3 == null || _scene == null) return;
            if (_statusText != null)
                _statusText.text = "D10 酒馆社会场景\n" + _scene.DisplayName + " | 金币 " + _d3.TradeState.Money +
                    " | 船员 " + GameState.CrewAgents.Count;
            if (_catalogText != null)
            {
                string rumor = _scene.Rumors.Count == 0 ? "无传闻" :
                    _scene.Rumors[0].Text + "（来源：" + _scene.Rumors[0].SourceKind +
                    "，可靠度：" + _scene.Rumors[0].ReliabilityLabel + "）";
                string jobs = _scene.SideJobs.Count == 0 ? "无公告" :
                    _scene.SideJobs[0].Summary + "；另有" + _scene.SideJobs.Count + "项公告";
                _catalogText.text = "传闻：" + rumor + "\n副业：" + jobs +
                    "\n牌局：入口交给 MiniGameSystem，不在酒馆结算。";
            }
            if (_hintText != null)
                _hintText.text = _lastMessage + " | V酒馆 R休息 E吃饭 A饮酒 H传闻 P牌局 D舞会 N夜话 J公告 K护卫 X跳过";
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D10TavernSocialPage", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 170;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _statusText = CreateText(canvasGo, "Status", "D10 酒馆等待运行时…", 22,
                new Vector2(28f, -28f), new Vector2(900f, 70f), TextAnchor.UpperLeft);
            SetTopLeft(_statusText.rectTransform, new Vector2(28f, -28f));
            _catalogText = CreateText(canvasGo, "Catalog", "", 17,
                new Vector2(28f, -120f), new Vector2(950f, 230f), TextAnchor.UpperLeft);
            SetTopLeft(_catalogText.rectTransform, new Vector2(28f, -120f));
            _hintText = CreateText(canvasGo, "Hint", "", 17, Vector2.zero,
                new Vector2(1800f, 48f), TextAnchor.MiddleLeft);
            SetBottomLeft(_hintText.rectTransform, new Vector2(28f, 20f));

            _panel = new GameObject("D10TavernSocialControls", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = (RectTransform)_panel.transform;
            panelRect.anchorMin = new Vector2(1f, 1f); panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f); panelRect.sizeDelta = new Vector2(430f, 735f);
            panelRect.anchoredPosition = new Vector2(-28f, -100f);
            var image = _panel.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.03f, 0.06f, 0.10f, 0.92f);
            CreateButton(_panel, "Recruit", "招募艾萨克 [Q]", new Vector2(0f, -35f), Recruit);
            CreateButton(_panel, "Rest", "休息 4 小时 [R]", new Vector2(0f, -95f), Rest);
            CreateButton(_panel, "Meal", "吃饭 [E]", new Vector2(0f, -155f), Eat);
            CreateButton(_panel, "Drink", "饮酒：烈酒 [A]", new Vector2(0f, -215f), Drink);
            CreateButton(_panel, "Rumor", "听传闻 [H]", new Vector2(0f, -275f), HearRumor);
            CreateButton(_panel, "Poker", "进入低额牌局 [P]", new Vector2(0f, -335f), Poker);
            CreateButton(_panel, "Dance", "舞会 [D]", new Vector2(0f, -395f), Dance);
            CreateButton(_panel, "NightTalk", "夜话 [N]", new Vector2(0f, -455f), NightTalk);
            CreateButton(_panel, "Board", "副业公告板 [J]", new Vector2(0f, -515f), OpenBoard);
            CreateButton(_panel, "SecurityJob", "护卫副业 [K]", new Vector2(0f, -575f), TakeSecurityJob);
            CreateButton(_panel, "Skip", "跳过敏感内容 [X]", new Vector2(0f, -635f), SkipSensitive);
            _panel.SetActive(false);
        }

        private static Button CreateButton(GameObject parent, string name, string label,
            Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = new Vector2(370f, 48f);
            rect.anchoredPosition = position;
            var image = go.GetComponent<Image>(); image.sprite = UiSprites.White;
            image.color = new Color(0.12f, 0.34f, 0.52f, 0.98f);
            var button = go.GetComponent<Button>(); button.targetGraphic = image;
            button.onClick.AddListener(action);
            var text = CreateText(go, "Label", label, 19, Vector2.zero,
                new Vector2(360f, 44f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string value, int size,
            Vector2 position, Vector2 sizeDelta, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = position;
            var text = go.GetComponent<Text>(); text.font = Fonts.Default; text.fontSize = size;
            text.alignment = alignment; text.color = Color.white; text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetTopLeft(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f); rect.anchoredPosition = position;
        }

        private static void SetBottomLeft(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f); rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>();
        }
    }
}
