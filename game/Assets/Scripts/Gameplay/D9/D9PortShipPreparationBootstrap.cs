// ============================================================
// D9PortShipPreparationBootstrap —— InitialHarbor 港口服务页
//
// 只读取 D9 snapshot、提交整备意图，并把保存交给 D3/D4/D7 入口。
// 本类不直接写 money、cargo、hull、crew 或 wage 数值。
// ============================================================

using System;
using System.Text;
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
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D9
{
    public sealed class D9PortShipPreparationBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D9PortShipPreparationBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D9PortShipPreparationBootstrap").AddComponent<D9PortShipPreparationBootstrap>();
            Debug.Log("[D9-A01] 港口与船舶整备入口已创建");
        }

        private D9PortShipPreparationSystem _preparation;
        private D3RuntimeBootstrap _d3;
        private Whalebone.Gameplay.D4.D4RuntimeBootstrap _d4;
        private ShipState _ship;
        private bool _ready;
        // The service page is closed while the player is in the primary HUD.
        // The canvas can still be created and refreshed in the background, but
        // it must not claim keyboard input until OpenPreparationPage is called.
        private bool _pageOpen;
        private string _lastMessage = "港口整备页等待 D7/D3/D4 状态。";
        private Text _statusText;
        private Text _detailText;
        private Text _hintText;
        private GameObject _panel;
        private D9PortShipPreparationSnapshot _snapshot;
        private int _pageOpenedFrame = -1;

        /// <summary>D9 UI/音频只读入口；快照仍由整备系统生成。</summary>
        public bool IsReady => _ready;
        public bool HasInputFocus => _pageOpen;
        public D9PortShipPreparationSnapshot Snapshot => _snapshot == null ? null : _snapshot.Clone();
        public string LastMessage => _lastMessage;

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

            bool defaultOpenPressed = Input.GetKeyDown(KeyCode.P);
            bool openPressed = D16InputRouter.WasPressedThisFrame(D16InputAction.D9OpenPortPreparation) ||
                defaultOpenPressed;
            if (openPressed && (_pageOpen ||
                (RuntimeHudCoordinator.PrimaryInputAllowed && D7InputAllowed())))
            {
                // D29 can call OpenPreparationPage from its own Update on the
                // same frame as P. Do not let D9 interpret that same key as a
                // close request immediately afterward.
                if (_pageOpen && Time.frameCount != _pageOpenedFrame) ClosePreparationPage();
                else OpenPreparationPage();
            }
            if (_panel != null) _panel.SetActive(_pageOpen);
            if (_pageOpen && D7InputAllowed())
            {
                if (D16InputRouter.WasPressedThisFrame(D16InputAction.D9ClosePortPreparation) ||
                    Input.GetKeyDown(KeyCode.Escape))
                {
                    D7ControlCoordinator.MarkEscapeHandledThisFrame();
                    ClosePreparationPage();
                }
                else if (WasD9Input(D16InputAction.D9EnterBerth, KeyCode.B)) EnterAndBerth();
                else if (WasD9Input(D16InputAction.D9BuySalt, KeyCode.M)) BuySalt();
                else if (WasD9Input(D16InputAction.D9BuyTimber, KeyCode.X)) BuyTimber();
                else if (WasD9Input(D16InputAction.D9VisitTavern, KeyCode.T)) VisitTavern();
                else if (WasD9Input(D16InputAction.D9BuyWater, KeyCode.S)) BuyWater();
                else if (WasD9Input(D16InputAction.D9RepairHull, KeyCode.R)) RepairHull();
                else if (WasD9Input(D16InputAction.D9RecruitCrew, KeyCode.Q)) RecruitIsaac();
                else if (WasD9Input(D16InputAction.D9SettleWages, KeyCode.W)) SettleWages();
                else if (WasD9Input(D16InputAction.D9Upgrade, KeyCode.U)) UpgradeHull();
                else if (WasD9Input(D16InputAction.D9Depart, KeyCode.D)) Depart();
            }
            Refresh();
        }

        /// <summary>由 D29 或路线测试打开整备页，并同步 HUD 互斥状态。</summary>
        public void OpenPreparationPage()
        {
            _pageOpen = true;
            _pageOpenedFrame = Time.frameCount;
            if (_panel != null) _panel.SetActive(true);
            RuntimeHudCoordinator.ShowTemporaryCanvas("D9PortServicePage");
            Debug.Log("[D9/UI] 港口整备页已打开");
        }

        /// <summary>整备页的唯一关闭入口，关闭后恢复玩家主 HUD。</summary>
        public void ClosePreparationPage()
        {
            _pageOpen = false;
            if (_panel != null) _panel.SetActive(false);
            RuntimeHudCoordinator.RestorePrimaryHud();
            Debug.Log("[D9/UI] 港口整备页已关闭，主 HUD 已恢复");
        }

        private bool EnsureReadyForAction()
        {
            if (_ready && _preparation != null && _d3 != null && _d3.Snapshot != null &&
                _d3.TradeState != null && _ship != null)
                return true;

            _lastMessage = "港口整备 owner 尚未就绪，请稍后再试。";
            if (_hintText != null) _hintText.text = _lastMessage;
            Debug.LogWarning("[D9/UI] " + _lastMessage);
            return false;
        }

        private void TryBindRuntime()
        {
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d4 == null) _d4 = FindFirstObjectByType<Whalebone.Gameplay.D4.D4RuntimeBootstrap>(FindObjectsInactive.Include);
            _ship = GameState.CurrentShip;
            if (_d3 == null || !_d3.IsReady || _d3.TradeState == null || _ship == null) return;

            if (_preparation == null)
                _preparation = new D9PortShipPreparationSystem(_d3.EconomySystem, _d3.EconomySystem.EventLog);

            _ready = true;
            if (_d3.Snapshot != null && _d3.Snapshot.VoyageStatus == D3VoyageStatus.InPort)
            {
                var entry = _preparation.EnterPort(_d3.Snapshot, _ship, CurrentPortId());
                if (entry.Accepted) _lastMessage = entry.Message;
                var berth = _preparation.AssignBerth(_ship, CurrentPortId(),
                    CurrentPortId() == PortCatalog.StartingPortId
                        ? D9PortShipPreparationSystem.SantaMargaritaBerthId
                        : D9PortShipPreparationSystem.GreenCayBerthId,
                    _d3.TradeState.Money);
                if (berth.Accepted) _lastMessage = berth.Message;
                _d3.CommitPreparationState();
            }
            Refresh();
        }

        private void EnterAndBerth()
        {
            if (!EnsureReadyForAction()) return;
            var entry = _preparation.EnterPort(_d3.Snapshot, _ship, CurrentPortId());
            if (entry.Accepted)
            {
                var berth = _preparation.AssignBerth(_ship, CurrentPortId(),
                    CurrentPortId() == PortCatalog.StartingPortId
                        ? D9PortShipPreparationSystem.SantaMargaritaBerthId
                        : D9PortShipPreparationSystem.GreenCayBerthId,
                    _d3.TradeState.Money);
                SetResult(berth.Accepted ? berth : entry);
            }
            else SetResult(entry);
            // 泊位通常会在进入整备页时自动分配，因此再次点击可能不会
            // 改变领域快照。仍要给玩家一条可区分的动作反馈，说明这是
            // 一次幂等的泊位检查/确认，而不是静默无响应。
            _lastMessage = "泊位操作已处理：" + _lastMessage;
            Refresh();
            CommitPreparation();
        }

        private void BuySalt()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.Buy(_d3.TradeState, _ship, CurrentPortId(), "salt", 1);
            SetResult(result);
            CommitPreparation();
        }

        private void BuyTimber()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.Buy(_d3.TradeState, _ship, CurrentPortId(), D9PortShipPreparationSystem.RepairMaterialId, 1);
            SetResult(result);
            CommitPreparation();
        }

        private void BuyWater()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.BuySupply(_d3.TradeState, _ship, CurrentPortId(), "water", 5);
            SetResult(result);
            CommitPreparation();
        }

        private void RepairHull()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.Repair(_d3.TradeState, _ship, CurrentPortId(), 10);
            SetResult(result);
            CommitPreparation();
        }

        private void VisitTavern()
        {
            if (!EnsureReadyForAction()) return;
            var social = FindFirstObjectByType<Whalebone.Gameplay.D10.D10TavernSocialBootstrap>(
                FindObjectsInactive.Include);
            if (social != null)
            {
                // D10 is a separate modal owner. Close D9 before handing the
                // focus over so the hidden preparation page cannot continue
                // consuming its own shortcuts behind the tavern page.
                ClosePreparationPage();
                social.OpenSocialPage();
                _lastMessage = "已切换到 D10 酒馆社会页；酒馆行为由所属系统裁决。";
            }
            else
            {
                _lastMessage = "D10 酒馆社会页尚未就绪，请按 V 打开。";
            }
            Refresh();
        }

        private void RecruitIsaac()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.Recruit(_d3.TradeState, _ship, CurrentPortId(),
                GameState.CrewAgents, D9CrewRecruitmentCatalog.CreateIsaacReed());
            SetResult(result);
            CommitPreparation();
        }

        private void SettleWages()
        {
            if (!EnsureReadyForAction()) return;
            var result = _preparation.SettleWages(_d3.TradeState, _ship, CurrentPortId(), GameState.CrewAgents);
            SetResult(result);
            CommitPreparation();
        }

        private void UpgradeHull()
        {
            if (!EnsureReadyForAction()) return;
            if (_d4 == null || !_d4.IsReady || _d4.ShipyardSystem == null)
            {
                SetResult(D9PreparationResult.Fail("shipyard", "not_ready", "船坞系统尚未就绪。", _d3.TradeState.Money));
                return;
            }
            int reputation = 0;
            if (GameState.CurrentPlayer != null && GameState.CurrentPlayer.Reputation != null)
                GameState.CurrentPlayer.Reputation.TryGetValue("england", out reputation);
            var result = _preparation.Upgrade(_d3.TradeState, _ship, _d4.ShipyardSystem, reputation, D4UpgradeSlot.Hull);
            SetResult(result);
            CommitPreparation();
        }

        private void Depart()
        {
            if (!EnsureReadyForAction()) return;
            var ready = _preparation.PrepareDeparture(_ship, CurrentPortId(), _d3.TradeState.Money);
            if (!ready.Accepted)
            {
                SetResult(ready);
                return;
            }
            bool sailing = _d3.RequestDeparture();
            if (sailing)
                _preparation.RecordDeparture(_d3.Snapshot, _ship,
                    _d3.Snapshot == null ? 0 : _d3.Snapshot.CurrentDay,
                    _d3.Snapshot == null ? 0 : _d3.Snapshot.SaveRevision);
            _lastMessage = sailing ? "已离港，航行系统已接管状态。" : "航线系统拒绝了离港请求。";
            CommitPreparation();
        }

        private void CommitPreparation()
        {
            if (_d3 != null) _d3.CommitPreparationState();
            Refresh();
        }

        private string CurrentPortId()
        {
            return _d3 == null || _d3.Snapshot == null || string.IsNullOrEmpty(_d3.Snapshot.CurrentLocationId)
                ? PortCatalog.StartingPortId : _d3.Snapshot.CurrentLocationId;
        }

        private void SetResult(D9PreparationResult result)
        {
            _lastMessage = result == null ? "整备没有返回结果。" : result.Message;
            Refresh();
            Debug.Log("[D9-A01] " + _lastMessage);
        }

        private void Refresh()
        {
            if (!_ready || _d3 == null || _ship == null) return;
            _snapshot = _preparation.BuildSnapshot(_d3.Snapshot, _d3.TradeState, _ship,
                _d4 == null ? null : _d4.ShipyardSystem, GameState.CrewAgents);
            if (_statusText != null)
                _statusText.text = "D9 港口整备\n" + _snapshot.PortDisplayName + " | 金币 " + DisplayNumber(_snapshot.Money) +
                    " | 泊位 " + (_snapshot.Moored ? "已靠泊" : "未靠泊");
            if (_detailText != null)
            {
                var ship = _snapshot.Ship;
                var cargo = new StringBuilder();
                foreach (var pair in ship.Cargo) cargo.Append(pair.Key).Append("×").Append(DisplayNumber(pair.Value)).Append(" ");
                var upgrades = new StringBuilder();
                foreach (var pair in ship.Upgrades) upgrades.Append(pair.Key).Append(" ").Append(DisplayNumber(pair.Value)).Append("级 ");
                _detailText.text = "船舶 " + ship.ShipTypeId +
                    " | 重量 " + DisplayNumber(ship.Weight) + "/" + DisplayNumber(ship.CargoCapacity) +
                    " | 吃水 " + DisplayNumber((decimal)ship.Draft, 1) + "m" +
                    "\n船员 " + DisplayNumber(ship.CrewCount) + "/" + DisplayNumber(ship.CrewCapacity) +
                    " | 货物 " + (cargo.Length == 0 ? "空" : cargo.ToString()) +
                    "\n补给 水 " + DisplayNumber(ship.Water) + " 食物 " + DisplayNumber(ship.Food) +
                    " | 损伤 " + DisplayNumber(ship.Damage) + "（船体 " + DisplayNumber(ship.Hull) + "/" + DisplayNumber(ship.HullMax) + "）" +
                    "\n锚泊 " + (ship.Moored ? "是" : "否") + " | 泊位 " +
                    (string.IsNullOrEmpty(ship.BerthId) ? "无" : ship.BerthId) +
                    "\n改装 " + (upgrades.Length == 0 ? "尚无" : upgrades.ToString());
            }
            if (_hintText != null)
                _hintText.text = _lastMessage + " | P整备页 B泊位 M买盐 T酒馆 S补给 R修船 X木材 Q招募 W工资 U船坞 D离港";
        }

        private static string DisplayNumber(int value)
        {
            return D16DisplayFormatter.Number(value, Localization.IsEnglish ? "en-US" : "zh-CN");
        }

        private static string DisplayNumber(decimal value, int decimals)
        {
            return D16DisplayFormatter.Number(value, Localization.IsEnglish ? "en-US" : "zh-CN", decimals);
        }

        private bool D7InputAllowed()
        {
            return Whalebone.Gameplay.D7.D7ControlCoordinator.GameplayInputAllowed;
        }

        private static bool WasD9Input(D16InputAction action, KeyCode fallback)
        {
            // Input System is the configurable primary path. The legacy edge is
            // only a Windows Player compatibility fallback for the same intent;
            // it does not create a second domain command or change ownership.
            return D16InputRouter.WasPressedThisFrame(action) || Input.GetKeyDown(fallback);
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D9PortServicePage", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            // Keep the temporary page hidden until OpenPreparationPage asks
            // RuntimeHudCoordinator to make this surface active.
            canvas.enabled = false;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var summary = new GameObject("D9Summary", typeof(RectTransform), typeof(Image));
            summary.transform.SetParent(canvasGo.transform, false);
            var summaryRect = (RectTransform)summary.transform;
            summaryRect.anchorMin = new Vector2(0f, 1f); summaryRect.anchorMax = new Vector2(0f, 1f);
            summaryRect.pivot = new Vector2(0f, 1f); summaryRect.sizeDelta = new Vector2(900f, 290f);
            // The scene's harbor title occupies the top-center safe zone.
            // Keep the long D9 summary below it so a temporary page never
            // paints its diagnostics over the location title.
            summaryRect.anchoredPosition = new Vector2(28f, -196f);
            var summaryImage = summary.GetComponent<Image>();
            summaryImage.sprite = UiSprites.White;
            summaryImage.color = new Color(0.02f, 0.05f, 0.08f, 0.84f);
            _statusText = CreateText(summary, "Status", "D9 正在等待运行时…", 22,
                Vector2.zero, new Vector2(860f, 58f), TextAnchor.UpperLeft);
            SetTopLeft(_statusText.rectTransform, new Vector2(20f, -18f));
            _detailText = CreateText(summary, "ShipSnapshot", "", 18,
                Vector2.zero, new Vector2(860f, 202f), TextAnchor.UpperLeft);
            SetTopLeft(_detailText.rectTransform, new Vector2(20f, -78f));

            var hintBand = new GameObject("D9HintBand", typeof(RectTransform), typeof(Image));
            hintBand.transform.SetParent(canvasGo.transform, false);
            var hintRect = (RectTransform)hintBand.transform;
            hintRect.anchorMin = new Vector2(0f, 0f); hintRect.anchorMax = new Vector2(0f, 0f);
            hintRect.pivot = new Vector2(0f, 0f); hintRect.sizeDelta = new Vector2(1800f, 52f);
            hintRect.anchoredPosition = new Vector2(28f, 20f);
            var hintImage = hintBand.GetComponent<Image>();
            hintImage.sprite = UiSprites.White;
            hintImage.color = new Color(0.02f, 0.05f, 0.08f, 0.82f);
            _hintText = CreateText(hintBand, "Hint", "", 17,
                Vector2.zero, new Vector2(1760f, 48f), TextAnchor.MiddleLeft);
            Stretch(_hintText.rectTransform);

            _panel = new GameObject("D9PortServiceControls", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = (RectTransform)_panel.transform;
            panelRect.anchorMin = new Vector2(1f, 1f); panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f); panelRect.sizeDelta = new Vector2(380f, 740f);
            panelRect.anchoredPosition = new Vector2(-28f, -110f);
            var image = _panel.GetComponent<Image>(); image.sprite = UiSprites.White; image.color = new Color(0.02f, 0.05f, 0.08f, 0.84f);
            var firstServiceButton = CreateButton(_panel, "Berth", "进入/泊位 [B]", new Vector2(0f, -35f), EnterAndBerth);
            CreateButton(_panel, "Market", "市场：买盐 [M]", new Vector2(0f, -100f), BuySalt);
            CreateButton(_panel, "Timber", "市场：买木材 [X]", new Vector2(0f, -165f), BuyTimber);
            CreateButton(_panel, "Tavern", "酒馆 [T]", new Vector2(0f, -230f), VisitTavern);
            CreateButton(_panel, "Supplies", "补给水×5 [S]", new Vector2(0f, -295f), BuyWater);
            CreateButton(_panel, "Repair", "修船 10 点 [R]", new Vector2(0f, -360f), RepairHull);
            CreateButton(_panel, "Recruit", "招募艾萨克 [Q]", new Vector2(0f, -425f), RecruitIsaac);
            CreateButton(_panel, "Wages", "发工资 [W]", new Vector2(0f, -490f), SettleWages);
            CreateButton(_panel, "Shipyard", "船坞船体改装 [U]", new Vector2(0f, -555f), UpgradeHull);
            CreateButton(_panel, "Depart", "离港 [D]", new Vector2(0f, -620f), Depart);
            CreateButton(_panel, "Close", "关闭整备页 [P / Esc]", new Vector2(0f, -685f), ClosePreparationPage);
            D16ModalFocusScope.Attach(_panel, firstServiceButton == null ? null : firstServiceButton.gameObject);
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(0.5f, 1f); rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f); rect.sizeDelta = new Vector2(330f, 48f); rect.anchoredPosition = position;
            var image = go.GetComponent<Image>(); image.sprite = UiSprites.White; image.color = new Color(0.12f, 0.34f, 0.52f, 0.98f);
            var button = go.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
            var text = CreateText(go, "Label", label, 20, Vector2.zero, new Vector2(320f, 44f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string value, int size, Vector2 position, Vector2 sizeDelta, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = sizeDelta; rect.anchoredPosition = position;
            var text = go.GetComponent<Text>(); text.font = Fonts.Default; text.fontSize = size; text.alignment = alignment;
            text.color = Color.white; text.text = value; text.raycastTarget = false; return text;
        }

        private static void SetTopLeft(RectTransform rect, Vector2 position)
        { rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f); rect.anchoredPosition = position; }
        private static void SetBottomLeft(RectTransform rect, Vector2 position)
        { rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(0f, 0f); rect.pivot = new Vector2(0f, 0f); rect.anchoredPosition = position; }
        private static void Stretch(RectTransform rect)
        { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null) eventSystem = new GameObject("D9EventSystem").AddComponent<EventSystem>();
            var input = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (input == null) input = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            input.enabled = true; input.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>()) if (module != input) module.enabled = false;
            eventSystem.gameObject.SetActive(true); EventSystem.current = eventSystem;
        }
    }
}
