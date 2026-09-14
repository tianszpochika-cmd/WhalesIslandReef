using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D4;
using Whalebone.Gameplay.D10;
using Whalebone.Gameplay.D9;
using Whalebone.Gameplay.D28;
using Whalebone.Gameplay.D25;
using Whalebone.Gameplay.D30;
using Whalebone.Gameplay.D31;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D2;
using Whalebone.Systems.D16;

namespace Whalebone.Gameplay.D29
{
    /// <summary>
    /// D29 唯一玩家快捷交互层。
    /// 按钮只调用现有公开 owner 入口或表现层预览入口；它不复制市场、船舶、
    /// 酒馆、存档、天气事实，也不在 UI 内计算任何业务数值。没有 owner 时，
    /// 按钮仍给出明确的可见反馈，而不是静默无响应。
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class D29InteractionHud : MonoBehaviour
    {
        // 控制器与真正的 Canvas 必须使用不同名字。控制器本身是运行时
        // 创建的宿主对象；如果也叫 D29InteractionHudCanvas，GameObject.Find
        // 可能先命中宿主而不是子 Canvas，导致隐藏策略只改了宿主的
        // CanvasGroup，实际 UI 仍然可见并继续参与输入。
        private const string ControllerName = "D29InteractionHudController";
        private const string RootName = "D29InteractionHudCanvas";
        private GameObject _panel;
        private Text _status;
        private D2ControlRouter _router;
        private D3RuntimeBootstrap _d3;
        private D4RuntimeBootstrap _d4;
        private D10TavernSocialBootstrap _d10;
        private D9PortShipPreparationBootstrap _d9;
        private GameObject _warehousePanel;
        private Text _warehouseText;
        private Button _marketButton;
        private Button _warehouseButton;
        private Button _tavernButton;
        private Button _shipyardButton;
        private Button _boardButton;
        private Button _weatherButton;
        private Button _saveButton;
        private Button _preparationButton;
        private float _nextResolve;
        private string _availabilitySignature = string.Empty;
        private string _lastOneShotAction = string.Empty;
        private float _lastOneShotTime = -1f;
        // Availability refreshes run after owner bootstrap and must not erase
        // the latest action result before the player can read it. The action
        // owner remains responsible for replacing this feedback on the next
        // deliberate interaction.
        private bool _hasActionFeedback;
        private readonly System.Collections.Generic.HashSet<KeyCode> _defaultKeyboardKeysHeld =
            new System.Collections.Generic.HashSet<KeyCode>();

        private const float OneShotRepeatGuardSeconds = 0.35f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
            UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D29InteractionHud>(FindObjectsInactive.Include) != null) return;
            new GameObject(ControllerName).AddComponent<D29InteractionHud>();
        }

        private void Awake()
        {
            BuildCanvas();
            ResolveOwners();
            RefreshButtonAvailability();
        }

        private void Update()
        {
            if (Time.unscaledTime >= _nextResolve)
            {
                _nextResolve = Time.unscaledTime + 0.5f;
                ResolveOwners();
                RefreshButtonAvailability();
            }

            // 仓库是 D29 自己拥有的模态页。它会被 RuntimeHudCoordinator 标记为
            // temporary canvas，因此 PrimaryInputAllowed 在仓库打开时为 false。
            // I/Esc 关闭快捷键必须在主 HUD 门禁之前处理，否则打开仓库后 I
            // 会被自己拦截，只剩下 Esc 能关闭，和按钮文案不一致。
            var warehouseVisible = _warehousePanel != null && _warehousePanel.activeSelf;
            bool pausePressed = D16InputRouter.WasPressedThisFrame(D16InputAction.Pause) ||
                WasDefaultKeyboardPressed(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.Escape);
            if (warehouseVisible &&
                (WasD29Input(D16InputAction.D29ToggleWarehouse, KeyCode.I) || pausePressed))
            {
                if (pausePressed) D7ControlCoordinator.MarkEscapeHandledThisFrame();
                CloseWarehouse();
                return;
            }

            // 其他键盘入口与按钮入口共用同一组公开方法，方便键鼠和后续手柄
            // 映射保持一致。临时页面拥有输入焦点时，必须让路给当前页面，
            // 防止 D4/D9 的 R/M/C 等按键被 D29 再次解释。
            if (RuntimeHudCoordinator.PrimaryInputAllowed)
            {
                if (D7ControlCoordinator.GameplayInputAllowed)
                {
                    // VC-G01 的 Player 验收矩阵把 M 定义为市场入口；保留
                    // 旧 B 别名以兼容既有键位，但只能由 D29 主 HUD owner
                    // 在同一门禁下消费，避免与 D9/D4 临时页串层。
                    if (WasD29Input(D16InputAction.D29OpenMarket, KeyCode.M, KeyCode.B)) OpenMarket();
                    if (WasD29Input(D16InputAction.D29OpenTavern, KeyCode.T)) OpenTavern();
                    if (Input.GetKeyDown(KeyCode.R)) CycleWeather();
                    if (Input.GetKeyDown(KeyCode.C)) Save();
                    if (WasD29Input(D16InputAction.D29ToggleWarehouse, KeyCode.I)) ToggleInfo();
                    if (Input.GetKeyDown(KeyCode.Q)) CycleQuality();
                    if (Input.GetKeyDown(KeyCode.P)) OpenPreparation();
                }
            }
        }

        private bool WasD29Input(D16InputAction action, params KeyCode[] legacyKeys)
        {
            bool mappedPressed = D16InputRouter.WasPressedThisFrame(action);
            // Standalone Player builds can receive the Input System edge while
            // Unity's legacy Input.GetKeyDown edge is absent. Read the default
            // keyboard control directly before keeping the legacy fallback, so
            // D29 shortcuts and modal close actions share the same Player-safe
            // input route as D2/HarborPause.
            bool defaultKeyboardPressed = false;
            if (!D16InputRouter.GameplayInputBlocked && legacyKeys != null)
            {
                for (int i = 0; i < legacyKeys.Length; i++)
                {
                    if (WasDefaultKeyboardPressed(legacyKeys[i])) defaultKeyboardPressed = true;
                    if (Input.GetKeyDown(legacyKeys[i])) defaultKeyboardPressed = true;
                }
            }
            return mappedPressed || defaultKeyboardPressed;
        }

        private bool WasDefaultKeyboardPressed(KeyCode keyCode)
        {
            bool isPressed = false;
            if (Keyboard.current != null &&
                System.Enum.TryParse(keyCode.ToString(), true, out Key key))
            {
                var control = Keyboard.current[key];
                isPressed = control != null && control.isPressed;
            }

            bool wasHeld = _defaultKeyboardKeysHeld.Contains(keyCode);
            if (isPressed) _defaultKeyboardKeysHeld.Add(keyCode);
            else _defaultKeyboardKeysHeld.Remove(keyCode);
            return isPressed && !wasHeld;
        }

        private static bool TryClaimPrimaryAction()
        {
            return D7ControlCoordinator.GameplayInputAllowed &&
                RuntimeHudCoordinator.TryClaimPrimaryInput();
        }

        private void ResolveOwners()
        {
            if (_router == null) _router = FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d4 == null) _d4 = FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d10 == null) _d10 = FindFirstObjectByType<D10TavernSocialBootstrap>(FindObjectsInactive.Include);
            if (_d9 == null) _d9 = FindFirstObjectByType<D9PortShipPreparationBootstrap>(FindObjectsInactive.Include);
        }

        private void BuildCanvas()
        {
            EnsureEventSystem();
            var canvasObject = new GameObject(RootName, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 160;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(
                D29InteractionLayoutPolicy.ReferenceWidth,
                D29InteractionLayoutPolicy.ReferenceHeight);
            scaler.matchWidthOrHeight = 0.5f;

            _panel = CreatePanel(canvasObject.transform, "QuickActions",
                new Vector2(-D29InteractionLayoutPolicy.QuickActionsRightInset,
                    -D29InteractionLayoutPolicy.QuickActionsTopInset),
                new Vector2(D29InteractionLayoutPolicy.QuickActionsWidth,
                    D29InteractionLayoutPolicy.QuickActionsHeight),
                new Vector2(1f, 1f), new Vector2(1f, 1f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.92f));
            CreateText(_panel.transform, "Title", "港口快捷交互", 22, new Vector2(0f, -27f),
                new Vector2(290f, 36f), TextAnchor.MiddleCenter, D25VisualTheme.Paper);
            CreateText(_panel.transform, "Subtitle", "所有按钮都有明确反馈 · B/R/C/I/Q 可用", 12,
                new Vector2(0f, -61f), new Vector2(300f, 24f), TextAnchor.MiddleCenter,
                D25VisualTheme.WithAlpha(D25VisualTheme.Paper, 0.72f));

            _marketButton = CreateButton(_panel.transform, "Market", "市场  [M / B]", 0, OpenMarket);
            _warehouseButton = CreateButton(_panel.transform, "Warehouse", "仓库  [I]", 1, ToggleInfo);
            _tavernButton = CreateButton(_panel.transform, "Tavern", "酒馆  [T]", 2, OpenTavern);
            _shipyardButton = CreateButton(_panel.transform, "Shipyard", "船坞：改良船体", 3, UpgradeShip);
            _boardButton = CreateButton(_panel.transform, "Board", "登船 / 下船  [E]", 4, ToggleShip);
            _weatherButton = CreateButton(_panel.transform, "Weather", "天气表现：切换  [R]", 5, CycleWeather);
            _saveButton = CreateButton(_panel.transform, "Save", "保存当前进度  [C]", 6, Save);
            _preparationButton = CreateButton(_panel.transform, "Preparation", "港口整备  [P]", 7, OpenPreparation);

            var info = CreatePanel(_panel.transform, "Info", new Vector2(0f, 30f),
                new Vector2(286f, 48f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Paper, 0.10f));
            _status = CreateText(info.transform, "Status", "准备就绪。", 13, new Vector2(0f, 0f),
                new Vector2(270f, 42f), TextAnchor.MiddleCenter, D25VisualTheme.Paper);
            BuildWarehousePanel(canvasObject.transform);
            D25VisualTheme.LogTextDiagnostics("D29InteractionHud", canvasObject.transform);
        }

        private void BuildWarehousePanel(Transform canvas)
        {
            _warehousePanel = CreatePanel(canvas, D29InteractionLayoutPolicy.WarehouseObjectName, Vector2.zero,
                new Vector2(D29InteractionLayoutPolicy.WarehouseWidth,
                    D29InteractionLayoutPolicy.WarehouseHeight),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                D25VisualTheme.WithAlpha(D25VisualTheme.Ink, 0.97f));
            CreateText(_warehousePanel.transform, "Title", "仓库 / 船舱", 24,
                new Vector2(0f, -30f), new Vector2(560f, 42f), TextAnchor.MiddleCenter,
                D25VisualTheme.Paper);
            _warehouseText = CreateText(_warehousePanel.transform, "Contents", "库存正在读取…", 17,
                new Vector2(0f, -120f), new Vector2(560f, 250f), TextAnchor.UpperLeft,
                D25VisualTheme.Paper);
            var closeButton = CreateButton(_warehousePanel.transform, "Close", "关闭仓库  [I / Esc]", 6, CloseWarehouse);
            D16ModalFocusScope.Attach(_warehousePanel, closeButton == null ? null : closeButton.gameObject);
            _warehousePanel.SetActive(false);
        }

        /// <summary>
        /// D29 是运行时创建的正式交互层，不能假设主菜单留下的 EventSystem
        /// 仍然存在。复用现有实例，缺失时才创建一个 Input System 实例，
        /// 保证鼠标/键盘按钮和键盘快捷键走同一套事件入口。
        /// </summary>
        private static void EnsureEventSystem()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            }

            if (eventSystem == null)
            {
                var go = new GameObject("D29EventSystem", typeof(EventSystem));
                eventSystem = go.GetComponent<EventSystem>();
            }

            // The scene may contain an inactive or stale EventSystem after a
            // temporary page/scene transition. Make the selected instance the
            // active navigation owner before any runtime-created button is
            // presented; otherwise pointer and keyboard events can be dropped
            // even though the GraphicRaycaster exists.
            eventSystem.gameObject.SetActive(true);
            eventSystem.enabled = true;
            if (EventSystem.current != eventSystem) EventSystem.current = eventSystem;
            // D29 is the primary gameplay overlay, not a modal menu.  Do not
            // enable UGUI navigation during its bootstrap frame: W/S must be
            // reserved for D2 movement/ship throttle from the moment the
            // buttons are created.  HarborInteractionStabilizer re-enables
            // navigation only after a real modal owner claims focus.
            eventSystem.sendNavigationEvents = false;

            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
            // AssignDefaultActions restores UI/Navigate, including the move
            // action. D29 is a non-modal gameplay HUD, so leaving that action
            // attached even for one bootstrap frame lets a physical W/S edge
            // move the selected quick-action button before the stabilizer's
            // next Update can reconcile the policy. Keep pointer/submit
            // wiring, but reserve directional input for D2 immediately.
            inputModule.move = null;
            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
            {
                if (module != inputModule) module.enabled = false;
            }
            eventSystem.UpdateModules();
        }

        private Button CreateButton(Transform parent, string name, string label, int index, Action action)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f,
                -D29InteractionLayoutPolicy.QuickActionButtonTop -
                index * D29InteractionLayoutPolicy.QuickActionButtonStep);
            rect.sizeDelta = new Vector2(D29InteractionLayoutPolicy.QuickActionButtonWidth,
                D29InteractionLayoutPolicy.QuickActionButtonHeight);
            var image = buttonObject.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.13f, 0.35f, 0.47f, 0.96f);
            image.raycastTarget = true;
            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            // The right-side quick actions are a non-modal HUD. Explicitly
            // disable their navigation instead of waiting for the periodic
            // stabilizer pass; otherwise a Player key edge during bootstrap
            // can move the selected button with W/S. The warehouse close
            // button is rebuilt by D16ModalFocusScope when its modal opens.
            button.navigation = new Navigation
            {
                mode = parent != null && parent.name == D29InteractionLayoutPolicy.WarehouseObjectName
                    ? Navigation.Mode.Automatic
                    : Navigation.Mode.None
            };
            var colors = button.colors;
            colors.normalColor = new Color(0.13f, 0.35f, 0.47f, 0.96f);
            colors.highlightedColor = new Color(0.20f, 0.50f, 0.58f, 1f);
            colors.pressedColor = new Color(0.82f, 0.57f, 0.25f, 1f);
            colors.disabledColor = new Color(0.20f, 0.24f, 0.25f, 0.55f);
            button.colors = colors;
            button.onClick.AddListener(() => action());
            CreateText(buttonObject.transform, "Label", label, 15, new Vector2(0f, 0f),
                new Vector2(270f, 38f), TextAnchor.MiddleCenter, D25VisualTheme.Paper);
            return button;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 position,
            Vector2 size, Vector2 anchor, Vector2 pivot, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
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
            outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private void ToggleShip()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("board")) return;
            ResolveOwners();
            if (_router == null) { Show("登船 owner 尚未就绪。仍可继续步行。", false); return; }
            try
            {
                var ok = _router.TryToggleShip();
                D30VisualLoopCoordinator.Notify(ok ? "boarded" : "board_failed");
                Show(ok ? "登船/下船命令已提交。" : _router.Hint, ok);
            }
            catch (Exception exception)
            {
                ReportActionException("board_failed", "登船", exception);
            }
        }

        private void OpenMarket()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("market")) return;
            ResolveOwners();
            if (_d3 == null || !_d3.IsReady)
            {
                Show("市场 owner 尚未就绪；按钮已响应，请稍后再试。", false);
                return;
            }
            try
            {
                var opened = _d3.RequestTrade();
                D30VisualLoopCoordinator.Notify(opened ? "market_opened" : "market_failed");
                Show(opened ? "市场面板已打开。" : "市场暂不可用，owner 已拒绝请求。", opened);
            }
            catch (Exception exception)
            {
                ReportActionException("market_failed", "市场", exception);
            }
        }

        private void OpenTavern()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("tavern")) return;
            ResolveOwners();
            if (_d10 == null || !_d10.IsReady)
            {
                Show("酒馆 owner 尚未就绪；按钮已响应，请稍后再试。", false);
                return;
            }
            try
            {
                RuntimeHudCoordinator.ShowTemporaryCanvas("D10TavernSocialPage");
                _d10.OpenSocialPage();
                var entered = _d10.HasEnteredTavern && _d10.HasInputFocus;
                if (!entered)
                {
                    // owner 拒绝进入时不能留下一个获得焦点的空酒馆页，
                    // 否则主 HUD 会被遮挡且后续输入仍会落到失败页面。
                    _d10.CloseSocialPage();
                }
                D30VisualLoopCoordinator.Notify(entered ? "tavern_opened" : "tavern_failed");
                Show(entered ? "酒馆社交面板已打开。" : "酒馆进入请求未通过：" + _d10.LastMessage, entered);
            }
            catch (Exception exception)
            {
                ReportActionException("tavern_failed", "酒馆", exception, _d10.CloseSocialPage);
            }
        }

        private void UpgradeShip()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("shipyard")) return;
            ResolveOwners();
            if (_d4 == null || !_d4.IsReady)
            {
                Show("船坞 owner 尚未就绪；按钮已响应。", false);
                return;
            }
            try
            {
                RuntimeHudCoordinator.ShowTemporaryCanvas("D4Canvas");
                var upgraded = _d4.TryUpgradeHull();
                var shipyardReady = upgraded && _d4.HasVisibleContentPage;
                if (!shipyardReady)
                {
                    // A rejected upgrade must not leave the D4 temporary page
                    // covering the primary HUD with stale or empty content.
                    _d4.CloseContentPage();
                }
                D30VisualLoopCoordinator.Notify(shipyardReady ? "shipyard_opened" : "shipyard_failed");
                Show(shipyardReady ? "船坞已完成船体改良：" + _d4.LastMessage : "船坞请求未通过：" + _d4.LastMessage, shipyardReady);
            }
            catch (Exception exception)
            {
                ReportActionException("shipyard_failed", "船坞", exception, _d4.CloseContentPage);
            }
        }

        private void OpenPreparation()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("preparation")) return;
            ResolveOwners();
            if (_d9 == null || !_d9.IsReady)
            {
                Show("港口整备 owner 尚未就绪；按钮已响应，请稍后再试。", false);
                return;
            }
            try
            {
                RuntimeHudCoordinator.ShowTemporaryCanvas("D9PortServicePage");
                _d9.OpenPreparationPage();
                var opened = _d9.HasInputFocus;
                if (!opened)
                {
                    // owner 调用没有建立焦点时，不能留下一个遮挡主 HUD 的临时页。
                    _d9.ClosePreparationPage();
                }
                D30VisualLoopCoordinator.Notify(opened ? "preparation_opened" : "preparation_failed");
                Show(opened ? "港口整备页已打开。" : "港口整备页打开失败：" + _d9.LastMessage, opened);
            }
            catch (Exception exception)
            {
                ReportActionException("preparation_failed", "港口整备", exception, _d9.ClosePreparationPage);
            }
        }

        private void CycleWeather()
        {
            if (!TryClaimPrimaryAction()) return;
            try
            {
                var changed = D28WaterWeatherPresentationRuntime.TryCycleWeather();
                Show(changed ? "天气表现已切换为 " + D28WaterWeatherPresentationRuntime.CurrentWeather + "。" :
                    "天气表现 owner 尚未就绪，暂不能切换。", changed);
            }
            catch (Exception exception)
            {
                ReportActionException("weather_failed", "天气表现", exception);
            }
        }

        private void CycleQuality()
        {
            if (!TryClaimPrimaryAction()) return;
            try
            {
                var changed = D31VisualQualityRuntime.TryCycleQuality();
                Show(changed ? "画质表现已切换为 " + D31VisualQualityRuntime.CurrentTier + "。" :
                    "画质表现 owner 尚未就绪，暂不能切换。", changed);
            }
            catch (Exception exception)
            {
                ReportActionException("quality_failed", "画质表现", exception);
            }
        }

        private void Save()
        {
            if (!TryClaimPrimaryAction() || !AllowOneShot("save")) return;
            ResolveOwners();
            if (_d3 != null && _d3.IsReady)
            {
                try
                {
                    var ok = _d3.RequestSave();
                    if (!ok) D30VisualLoopCoordinator.Notify("save_failed");
                    Show(ok ? "贸易、库存与航程已保存。" : "保存 owner 拒绝或暂不可用。", ok);
                    return;
                }
                catch (Exception exception)
                {
                    ReportActionException("save_failed", "保存", exception);
                    return;
                }
            }

            try
            {
                Whalebone.Systems.Save.SaveSystem.Save();
                Show("基础存档已提交。", true);
            }
            catch (Exception exception)
            {
                ReportActionException("save_failed", "保存", exception);
            }
        }

        private void ToggleInfo()
        {
            if (_warehousePanel == null)
            {
                Show("仓库面板尚未就绪，无法打开。", false);
                return;
            }
            if (_warehousePanel.activeSelf)
            {
                CloseWarehouse();
                return;
            }

            if (!TryClaimPrimaryAction()) return;

            ResolveOwners();
            if (_d3 == null || !_d3.IsReady)
            {
                // The warehouse is a read-only projection of D3's public
                // snapshot. Never open an empty modal when that owner is
                // missing or still bootstrapping; the keyboard path must
                // obey the same availability boundary as the Button.
                Show("仓库 owner 尚未就绪，暂不能读取库存。", false);
                return;
            }

            _panel.SetActive(false);
            _warehousePanel.SetActive(true);
            RuntimeHudCoordinator.ShowTemporaryCanvas("D29WarehousePanel");
            RefreshWarehouse();
            Debug.Log("[D29/Interaction] success: 仓库面板已打开。");
        }

        private void RefreshWarehouse()
        {
            if (_warehouseText == null)
            {
                Show("仓库内容区域尚未就绪，无法读取库存。", false);
                return;
            }
            ResolveOwners();
            var snapshot = _d3 == null ? null : _d3.Snapshot;
            if (snapshot == null)
            {
                _warehouseText.text = "仓库 owner 尚未就绪。\n请返回主 HUD 后稍候重试。";
                return;
            }

            var cargo = new StringBuilder("货舱\n");
            if (snapshot.Cargo == null || snapshot.Cargo.Count == 0) cargo.Append("  空\n");
            else foreach (var pair in snapshot.Cargo) cargo.Append("  ").Append(pair.Key).Append(" × ").Append(pair.Value).Append('\n');

            var personal = new StringBuilder("个人物品\n");
            if (snapshot.PersonalItems == null || snapshot.PersonalItems.Count == 0) personal.Append("  空\n");
            else foreach (var pair in snapshot.PersonalItems) personal.Append("  ").Append(pair.Key).Append(" × ").Append(pair.Value).Append('\n');

            _warehouseText.text = "当前位置：" + snapshot.CurrentLocationId +
                "    金币：" + snapshot.Money + "\n" +
                "补给：水 " + snapshot.Water + " / 食物 " + snapshot.Food +
                "    船体：" + snapshot.Hull + "/" + snapshot.HullMax + "\n\n" +
                cargo + "\n" + personal;
        }

        private void CloseWarehouse()
        {
            if (_warehousePanel != null) _warehousePanel.SetActive(false);
            if (_panel != null) _panel.SetActive(true);
            RuntimeHudCoordinator.RestorePrimaryHud();
            Show("仓库面板已关闭。", true);
            Debug.Log("[D29/Interaction] success: 仓库面板已关闭。");
        }

        /// <summary>
        /// 一次性命令的短窗口防重。它只保护表现层重复提交，不替代 owner 的
        /// 幂等键和业务拒绝；天气/画质等明确可重复的预览切换不经过此方法。
        /// </summary>
        private bool AllowOneShot(string actionId)
        {
            // The timed guard only covers duplicate presentation submits. Once
            // the owner has closed its page, a later open is a new interaction
            // session and must not inherit the previous session's timestamp.
            // Resolve here because button/reflection/keyboard callers can enter
            // this method without passing through the periodic availability
            // refresh in Update().
            ResolveOwners();
            ResetClosedOwnerOneShot(actionId);
            var now = Time.unscaledTime;
            var decision = D29OneShotPolicy.Evaluate(actionId, _lastOneShotAction,
                now, _lastOneShotTime, OneShotRepeatGuardSeconds);
            if (!decision.Allowed)
            {
                Show("该请求仍在处理中，重复点击已忽略。", false);
                return false;
            }

            _lastOneShotAction = decision.NormalizedActionId;
            _lastOneShotTime = now;
            return true;
        }

        private void ResetClosedOwnerOneShot(string actionId)
        {
            var normalized = (actionId ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.Equals(_lastOneShotAction, normalized, StringComparison.Ordinal)) return;

            bool ownerSessionClosed = false;
            switch (normalized)
            {
                case "market":
                    ownerSessionClosed = _d3 == null || !_d3.IsTradeOpen;
                    break;
                case "tavern":
                    ownerSessionClosed = _d10 == null || !_d10.HasInputFocus;
                    break;
                case "board":
                    ownerSessionClosed = _router == null || _router.Mode != D2ControlMode.Ship;
                    break;
                case "shipyard":
                    ownerSessionClosed = _d4 == null || !_d4.HasVisibleContentPage;
                    break;
                case "preparation":
                    ownerSessionClosed = _d9 == null || !_d9.HasInputFocus;
                    break;
            }

            if (!ownerSessionClosed) return;
            _lastOneShotAction = string.Empty;
            _lastOneShotTime = -1f;
        }

        /// <summary>
        /// 根据公开 owner 就绪状态更新按钮的视觉状态。
        ///
        /// 未就绪入口仍保持可点击：点击后由对应 owner 返回明确的等待/拒绝原因，
        /// 避免“按钮看得到但完全没有反应”。按钮的 interactable 不是业务权限；
        /// 真正的前置条件仍由各 owner 在公开入口内裁决。
        /// </summary>
        private void RefreshButtonAvailability()
        {
            var weatherOwner = FindFirstObjectByType<D28WaterWeatherPresentationRuntime>(FindObjectsInactive.Include);
            var d3Ready = _d3 != null && _d3.IsReady;
            var d10Ready = _d10 != null && _d10.IsReady;
            var d4Ready = _d4 != null && _d4.IsReady;
            var routerReady = _router != null;
            var weatherReady = weatherOwner != null && weatherOwner.IsReady;
            var d9Ready = _d9 != null && _d9.IsReady;
            var signature = string.Concat(
                d3Ready ? '1' : '0',
                d10Ready ? '1' : '0',
                d4Ready ? '1' : '0',
                routerReady ? '1' : '0',
                weatherReady ? '1' : '0',
                d9Ready ? '1' : '0');
            if (signature == _availabilitySignature) return;
            _availabilitySignature = signature;

            var reasons = new StringBuilder();
            SetAvailability(_marketButton, d3Ready);
            SetAvailability(_warehouseButton, d3Ready);
            SetAvailability(_tavernButton, d10Ready);
            SetAvailability(_shipyardButton, d4Ready);
            SetAvailability(_boardButton, routerReady);
            SetAvailability(_weatherButton, weatherReady);
            SetAvailability(_saveButton, true);
            SetAvailability(_preparationButton, d9Ready);

            AppendUnavailable(reasons, !d3Ready, "市场/仓库");
            AppendUnavailable(reasons, !d10Ready, "酒馆");
            AppendUnavailable(reasons, !d4Ready, "船坞");
            AppendUnavailable(reasons, !routerReady, "登船");
            AppendUnavailable(reasons, !weatherReady, "天气表现");
            AppendUnavailable(reasons, !d9Ready, "港口整备");
            if (_status != null && !_hasActionFeedback)
                _status.text = reasons.Length == 0
                    ? "✓ 交互入口已就绪。"
                    : "⚠ 等待系统：" + reasons + "（仍可点击查看原因）";

            Debug.Log("[D29/Interaction] availability=" + signature
                + "; clickable_unready=true; owner_feedback=true");
        }

        private static void SetAvailability(Button button, bool available)
        {
            if (button == null) return;
            // Interactable controls the UI event route, not domain permission.
            // Keep the route alive so an unready owner can explain the reason.
            button.interactable = true;
            var colors = button.colors;
            colors.normalColor = available
                ? new Color(0.13f, 0.35f, 0.47f, 0.96f)
                : new Color(0.20f, 0.27f, 0.29f, 0.96f);
            colors.highlightedColor = available
                ? new Color(0.20f, 0.50f, 0.58f, 1f)
                : new Color(0.30f, 0.38f, 0.39f, 1f);
            colors.pressedColor = new Color(0.82f, 0.57f, 0.25f, 1f);
            button.colors = colors;
        }

        private static void AppendUnavailable(StringBuilder reasons, bool unavailable, string label)
        {
            if (!unavailable) return;
            if (reasons.Length > 0) reasons.Append("、");
            reasons.Append(label);
        }

        private void Show(string message, bool succeeded)
        {
            if (_status == null) return;
            _hasActionFeedback = true;
            var prefix = succeeded ? "✓ " : "⚠ ";
            _status.text = prefix + message;
            Debug.Log("[D29/Interaction] " + (succeeded ? "success: " : "feedback: ") + message);
        }

        /// <summary>
        /// 表现层动作发生未预期异常时的统一恢复出口：尽力关闭当前临时页、
        /// 恢复主 HUD、向 D30 投影失败并显示状态未改变。异常不被重新抛回到
        /// Unity UI 事件链，也不改变任何业务 owner 的裁决结果。
        /// </summary>
        private void ReportActionException(string eventId, string surface, Exception exception, Action closePage = null)
        {
            try
            {
                if (closePage != null) closePage();
                else RuntimeHudCoordinator.RestorePrimaryHud();
            }
            catch (Exception recoveryException)
            {
                Debug.LogWarning("[D29/Interaction] " + surface + " recovery failed: " + recoveryException.Message);
                // A broken owner close callback must not leave a temporary page
                // covering the primary HUD. The coordinator is the last-resort
                // visual recovery path and is intentionally independent of the owner.
                try
                {
                    RuntimeHudCoordinator.RestorePrimaryHud();
                }
                catch (Exception fallbackException)
                {
                    Debug.LogWarning("[D29/Interaction] " + surface + " recovery fallback failed: " + fallbackException.Message);
                }
            }

            D30VisualLoopCoordinator.Notify(eventId);
            Show(surface + "请求异常，状态未改变。", false);
            Debug.LogWarning("[D29/Interaction] " + surface + " action failed: " + exception.Message);
        }
    }
}
