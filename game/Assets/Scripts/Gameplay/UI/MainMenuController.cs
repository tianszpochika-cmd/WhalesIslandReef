// ============================================================
// MainMenuController —— 主菜单控制中台（P1A）
//
// 归属：Gameplay/UI（表现层，见 DEVELOPMENT_STANDARDS §1）
// 职责：
//   1. 主菜单四项：新游戏 / 继续 / 设置 / 关于
//   2. 新游戏面板（船长名 InputField + 确认 → SaveSystem.NewGame）
//   3. 继续：HasSave() → Load() 成功切场景；无存档置灰 + 提示
//   4. 设置面板：音量×3 Slider / 画质 Dropdown / 语言 Dropdown（即时生效）
//   5. 关于面板：版本/开发/合规 + 退出
//   6. 所有文案走 Localization（语言切换即时刷新当前 UI）
//
// 说明：
//   - 只调用 SaveSystem 公开接口，不实现不修改（P0 契约）
//   - UGUI 由代码动态构建（场景文件保持最小，纯代码可复现）
//   - 面板文字全部注册到 _localized 列表，语言切换时统一刷新
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Core;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.UI
{
    /// <summary>
    /// 挂载于 MainMenu 场景的 Bootstrap 空物体。运行后动态构建 UGUI。
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ---- 自动挂载：每次进入 MainMenu 场景都创建控制器 ----
        // RuntimeInitializeOnLoadMethod(AfterSceneLoad) 只适合首次启动；
        // 从 InitialHarbor 返回 MainMenu 时必须监听 sceneLoaded，否则场景切换成功但 UI 不会重建。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu")
                return;

            if (FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include) != null)
                return;

            // 确保 GameManager 存在（P0 单例）
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();

            var host = new GameObject("MainMenuController");
            host.AddComponent<MainMenuController>();
            Debug.Log("[MainMenu] 场景加载完成，主菜单控制器已重新创建");
        }

        // ---- 面板根（代码创建）----
        private GameObject _rootCanvas;
        private GameObject _mainMenuPanel;
        private GameObject _newGamePanel;
        private GameObject _settingsPanel;
        private GameObject _aboutPanel;

        // ---- 控件缓存 ----
        private InputField _captainInput;
        private Button _continueBtn;
        private Text _continueHintText;
        private Text _newGameErrorText;
        private Dropdown _langDropdown;

        // ---- 本地化注册表：所有随语言刷新的控件 ----
        private readonly List<LocalizedText> _localized = new List<LocalizedText>();
        private readonly List<LocalizedDropdown> _localizedDropdowns = new List<LocalizedDropdown>();
        private bool _refreshingTexts;   // 防递归标志

        private struct LocalizedText { public Text Text; public string Key; }
        private struct LocalizedDropdown { public Dropdown Dropdown; public string[] Keys; }

        private void Awake()
        {
            EnsureMainMenuCamera();
            BuildCanvas();
            BuildMainMenu();
            BuildNewGamePanel();
            BuildSettingsPanel();
            BuildAboutPanel();
            AttachPanelFocusScopes();

            SettingsStore.ApplyAll();   // 启动应用持久化设置
            RefreshAllTexts();          // 初始语言应用
            RefreshContinueState();     // 继续按钮初始状态
            Debug.Log("[MainMenu] UI 初始化完成：主菜单已显示");
        }

        private void AttachPanelFocusScopes()
        {
            D16ModalFocusScope.Attach(_mainMenuPanel,
                _mainMenuPanel.transform.Find("BtnNewGame")?.gameObject);
            D16ModalFocusScope.Attach(_newGamePanel,
                _captainInput == null ? _newGamePanel.transform.Find("BtnConfirm")?.gameObject : _captainInput.gameObject);
            D16ModalFocusScope.Attach(_settingsPanel,
                _settingsPanel.transform.Find("settings_volume_master_Slider")?.gameObject ??
                _settingsPanel.transform.Find("BtnClose")?.gameObject);
            D16ModalFocusScope.Attach(_aboutPanel,
                _aboutPanel.transform.Find("BtnClose")?.gameObject);
        }

        /// <summary>
        /// 主菜单使用 ScreenSpaceOverlay，不依赖相机；这里补一台只负责清屏的相机，
        /// 避免 Unity Game 视图显示 “No cameras rendering” 造成“切换卡住”的误判。
        /// </summary>
        private void EnsureMainMenuCamera()
        {
            if (FindFirstObjectByType<Camera>(FindObjectsInactive.Include) != null)
                return;

            var cameraGo = new GameObject("MainMenuCamera");
            cameraGo.transform.SetParent(transform, false);
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.055f, 0.09f, 1f);
            camera.cullingMask = 0;
            camera.depth = -100f;
            camera.tag = "MainCamera";
        }

        // ============================================================
        // Canvas 骨架
        // ============================================================
        private void BuildCanvas()
        {
            _rootCanvas = new GameObject("MainMenuCanvas");
            _rootCanvas.transform.SetParent(transform, false);
            var canvas = _rootCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _rootCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            _rootCanvas.AddComponent<GraphicRaycaster>();

            // 不再 DestroyImmediate 旧 EventSystem：Play Mode 下销毁旧对象可能让
            // EventSystem.current 暂时指向失效实例，表现为 UI 能显示但 Button 不收点击。
            var es = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include);
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.transform.SetParent(transform, false);
                es = esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // 主菜单是运行时创建的，不能让其他场景/残留对象继续竞争当前 EventSystem。
            foreach (var other in FindObjectsByType<UnityEngine.EventSystems.EventSystem>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (other != es) other.gameObject.SetActive(false);
            }

            es.gameObject.SetActive(true);
            if (UnityEngine.EventSystems.EventSystem.current != es)
                UnityEngine.EventSystems.EventSystem.current = es;
            es.sendNavigationEvents = true;

            // 保证场景中始终只有一个启用的 UI 输入模块；保留旧模块对象可避免
            // Unity 在当前帧仍持有已销毁模块的引用。
            var inputModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule == null)
                inputModule = es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            inputModule.enabled = true;

            foreach (var module in es.GetComponents<UnityEngine.EventSystems.BaseInputModule>())
            {
                if (module != inputModule) module.enabled = false;
            }

            // 明确重置运行时模块的默认 Point/LeftClick/Submit 动作，避免复用到
            // 没有有效动作引用的旧实例（动态 AddComponent 不会替我们修复旧实例）。
            inputModule.AssignDefaultActions();
            es.UpdateModules();
            Debug.Log("[MainMenu] EventSystem 已就绪：" + es.gameObject.name + " / " +
                      inputModule.GetType().Name + " / actions=Default");
        }

        private static RectTransform CreateFullRect(GameObject parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>
        /// 创建锚点居中、显式 sizeDelta 的子 RectTransform（小 UI 用；勿用 CreateFullRect）。
        /// 修复 P1A CreateMenuButton 误用 CreateFullRect 导致按钮被拉伸到全屏、相互重叠的问题（2026-08-08 统筹修复）。
        /// </summary>
        private static RectTransform CreateAnchoredRect(GameObject parent, string name, Vector2 size, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        // ============================================================
        // 主菜单（标题 + 4 按钮）
        // ============================================================
        private void BuildMainMenu()
        {
            _mainMenuPanel = CreateFullRect(_rootCanvas, "MainMenu").gameObject;

            CreateLocalizedText(_mainMenuPanel, "Title", "title", 96, new Vector2(0, 300));

            CreateMenuButton(_mainMenuPanel, "BtnNewGame", "menu_new_game", new Vector2(0, 80), () => OpenPanel(_newGamePanel));
            CreateMenuButton(_mainMenuPanel, "BtnContinue", "menu_continue", new Vector2(0, 0), () => TryContinue());
            CreateMenuButton(_mainMenuPanel, "BtnSettings", "menu_settings", new Vector2(0, -80), () => OpenPanel(_settingsPanel));
            CreateMenuButton(_mainMenuPanel, "BtnAbout", "menu_about", new Vector2(0, -160), () => OpenPanel(_aboutPanel));

            _continueHintText = CreateText(_mainMenuPanel, "ContinueHint", "", 26, new Vector2(0, -230));
            _continueHintText.color = new Color(1f, 0.6f, 0.3f);
        }

        private void CreateMenuButton(GameObject parent, string name, string textKey, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var rt = CreateAnchoredRect(parent, name, new Vector2(320, 64), pos);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiSprites.White;
            img.color = new Color(0.15f, 0.28f, 0.4f, 0.92f);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            if (name == "BtnContinue") _continueBtn = btn;

            // 按钮文字（本地化注册）
            var label = CreateText(rt.gameObject, "Label", "", 32, Vector2.zero);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.alignment = TextAnchor.MiddleCenter;
            _localized.Add(new LocalizedText { Text = label, Key = textKey });
        }

        // ============================================================
        // 新游戏面板
        // ============================================================
        private void BuildNewGamePanel()
        {
            _newGamePanel = CreatePanel("NewGamePanel");

            // Keep the title clear of the 48px captain-name field and its
            // localized placeholder at the 1920x1080 reference resolution.
            // The previous 150px offset left only a 20px nominal gap between
            // the title Text rect and the input row, which overlapped in the
            // native 1280x720 Player capture.
            CreateLocalizedText(_newGamePanel, "Title", "newgame_title", 48, new Vector2(0, 210));

            // 船长名输入
            CreateLocalizedText(_newGamePanel, "NameLabel", "newgame_captain_name", 26, new Vector2(-280, 70));
            var inputGo = new GameObject("CaptainInput", typeof(RectTransform), typeof(Image), typeof(InputField));
            inputGo.transform.SetParent(_newGamePanel.transform, false);
            var inputRt = (RectTransform)inputGo.transform;
            inputRt.anchoredPosition = new Vector2(80, 70);
            inputRt.sizeDelta = new Vector2(460, 48);
            inputGo.GetComponent<Image>().sprite = UiSprites.White;
            inputGo.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.24f, 0.95f);
            _captainInput = inputGo.GetComponent<InputField>();

            var textArea = new GameObject("TextArea", typeof(RectTransform));
            textArea.transform.SetParent(inputGo.transform, false);
            var taRt = (RectTransform)textArea.transform;
            taRt.anchorMin = Vector2.zero; taRt.anchorMax = Vector2.one;
            taRt.offsetMin = new Vector2(10, 0); taRt.offsetMax = new Vector2(-10, 0);

            var inputText = textArea.AddComponent<Text>();
            inputText.font = Fonts.Default;
            inputText.fontSize = 26;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleLeft;
            inputText.raycastTarget = false;
            _captainInput.textComponent = inputText;

            var placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(textArea.transform, false);
            var phRt = (RectTransform)placeholder.transform;
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            var phText = placeholder.AddComponent<Text>();
            phText.font = Fonts.Default;
            phText.fontSize = 26;
            phText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            phText.raycastTarget = false;
            _captainInput.placeholder = phText;
            _captainInput.textComponent.text = "";
            _localized.Add(new LocalizedText { Text = phText, Key = "newgame_placeholder_name" });

            _newGameErrorText = CreateText(_newGamePanel, "ErrorText", "", 28, new Vector2(0, 40));
            _newGameErrorText.color = Color.red;

            CreateLocalizedButton(_newGamePanel, "BtnConfirm", "newgame_confirm", new Vector2(-100, -110), () =>
            {
                // 诊断日志（2026-08-09 统筹）：确认回调触发即打印，用于定位"点了没反应"
                Debug.Log("[MainMenu] 确认按钮被点击");
                string name = _captainInput.text.Trim();
                if (name.Length == 0)
                {
                    // 输入框为空 → 自动填默认值"新船长"（不阻塞流程；2026-08-08 统筹）
                    name = "新船长";
                    _captainInput.text = name;
                }
                _newGameErrorText.text = "";

                // P0 契约：只调用 SaveSystem 公开接口
                try
                {
                    SaveSystem.NewGame(new NewGameParams
                    {
                        CaptainName = name,
                        ShipTypeId = "small_fishing"   // M0 固定小渔船
                    });
                    Debug.Log("[MainMenu] NewGame 已发起：" + name);

                    // 关闭 NewGame 面板（修复 P1A 漏调用 ClosePanel 导致面板不退出的 bug；2026-08-08 统筹）
                    ClosePanel(_newGamePanel);
                    // P1C 场景未实现时 LoadInitialHarbor 只打日志不崩溃（GameManager 已容错）
                    GameManager.Instance.LoadInitialHarbor();
                }
                catch (System.Exception ex)
                {
                    _newGameErrorText.text = "创建失败：" + ex.Message;
                    Debug.LogError("[MainMenu] NewGame 异常：" + ex);
                }
            });

            CreateLocalizedButton(_newGamePanel, "BtnCancel", "newgame_cancel", new Vector2(100, -110), () => ClosePanel(_newGamePanel));
        }

        // ============================================================
        // 设置面板
        // ============================================================
        private void BuildSettingsPanel()
        {
            _settingsPanel = CreatePanel("SettingsPanel");

            // 所有控件都必须落在 1920x1080 的参考视口内。此前诊断和返回按钮
            // 使用 -430/-505 的坐标，按钮底部超出视口，表现为“看得到但点不到”。
            CreateLocalizedText(_settingsPanel, "Title", "settings_title", 48, new Vector2(0, 250));

            CreateSliderRow(_settingsPanel, "settings_volume_master", SettingsStore.MasterVolume, 160f, v => { SettingsStore.MasterVolume = v; AudioListener.volume = v; });
            CreateSliderRow(_settingsPanel, "settings_volume_music", SettingsStore.MusicVolume, 110f, v => SettingsStore.MusicVolume = v);
            CreateSliderRow(_settingsPanel, "settings_volume_sfx", SettingsStore.SfxVolume, 60f, v => SettingsStore.SfxVolume = v);

            // 画质
            CreateLocalizedText(_settingsPanel, "QualityLabel", "settings_quality", 26, new Vector2(-330, 0));
            var qualityDropdown = CreateDropdown(_settingsPanel, new Vector2(0, 0), new[] { "quality_low", "quality_medium", "quality_high" });
            qualityDropdown.Dropdown.value = SettingsStore.QualityLevel;
            qualityDropdown.Dropdown.onValueChanged.AddListener(v =>
            {
                SettingsStore.QualityLevel = v;
                QualitySettings.SetQualityLevel(v, true);
            });

            // 语言
            CreateLocalizedText(_settingsPanel, "LangLabel", "settings_language", 26, new Vector2(-330, -60));
            _langDropdown = CreateDropdown(_settingsPanel, new Vector2(0, -60), new[] { "lang_zh", "lang_en" }).Dropdown;
            _langDropdown.value = SettingsStore.Lang == "en" ? 1 : 0;
            _langDropdown.onValueChanged.AddListener(v =>
            {
                SettingsStore.Lang = v == 1 ? "en" : "zh";
                RefreshAllTexts();   // 即时生效
            });

            // 键位说明（只读）
            CreateLocalizedButton(_settingsPanel, "BtnAccessibility", "settings_accessibility", new Vector2(0, -130),
                D16AccessibilityPanel.ShowPanel);

            CreateLocalizedButton(_settingsPanel, "BtnDiagnostics", "settings_diagnostics", new Vector2(0, -315),
                D16LocalDiagnosticsPanel.ShowPanel);

            CreateLocalizedText(_settingsPanel, "ControlsTitle", "settings_controls", 28, new Vector2(0, -200));
            var controlsBody = CreateLocalizedText(_settingsPanel, "ControlsBody", "controls_text", 22, new Vector2(0, -245));
            controlsBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            controlsBody.verticalOverflow = VerticalWrapMode.Overflow;

            CreateLocalizedButton(_settingsPanel, "BtnClose", "btn_back", new Vector2(0, -390), () => ClosePanel(_settingsPanel));
        }

        private void CreateSliderRow(GameObject parent, string labelKey, float initial, float y,
                                     UnityEngine.Events.UnityAction<float> onChanged)
        {
            CreateLocalizedText(parent, labelKey + "_Label", labelKey, 26, new Vector2(-360, y));

            var sliderGo = new GameObject(labelKey + "_Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent.transform, false);
            var srt = (RectTransform)sliderGo.transform;
            srt.anchoredPosition = new Vector2(60, y);
            srt.sizeDelta = new Vector2(460, 24);

            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f; slider.maxValue = 1f;
            slider.value = initial;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGo.transform, false);
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0, 0.25f); bgRt.anchorMax = new Vector2(1, 0.75f);
            bg.GetComponent<Image>().sprite = UiSprites.White;
            bg.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.24f, 0.8f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(sliderGo.transform, false);
            var fRt = (RectTransform)fill.transform;
            fRt.anchorMin = Vector2.zero; fRt.anchorMax = Vector2.one;
            fRt.offsetMin = new Vector2(0, 4); fRt.offsetMax = new Vector2(0, -4);
            fill.GetComponent<Image>().sprite = UiSprites.White;
            fill.GetComponent<Image>().color = new Color(0.3f, 0.55f, 0.75f, 0.9f);
            slider.fillRect = fRt;
            slider.targetGraphic = bg.GetComponent<Image>();
            slider.onValueChanged.AddListener(onChanged);
        }

        private LocalizedDropdown CreateDropdown(GameObject parent, Vector2 pos, string[] optionKeys)
        {
            var go = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(300, 42);
            go.GetComponent<Image>().sprite = UiSprites.White;
            go.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.24f, 0.95f);

            var dropdown = go.GetComponent<Dropdown>();

            var label = CreateText(go, "Label", "", 24, Vector2.zero);
            label.alignment = TextAnchor.MiddleCenter;
            dropdown.captionText = label;

            var result = new LocalizedDropdown { Dropdown = dropdown, Keys = optionKeys };
            _localizedDropdowns.Add(result);
            return result;
        }

        // ============================================================
        // 关于面板
        // ============================================================
        private void BuildAboutPanel()
        {
            _aboutPanel = CreatePanel("AboutPanel");

            CreateLocalizedText(_aboutPanel, "Title", "about_title", 48, new Vector2(0, 200));
            CreateLocalizedText(_aboutPanel, "Version", "about_version", 28, new Vector2(0, 90));
            CreateLocalizedText(_aboutPanel, "Dev", "about_dev", 28, new Vector2(0, 30));
            CreateLocalizedText(_aboutPanel, "Compliance", "about_compliance", 22, new Vector2(0, -110));

            CreateLocalizedButton(_aboutPanel, "BtnExit", "about_exit", new Vector2(0, -300), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            CreateLocalizedButton(_aboutPanel, "BtnClose", "btn_back", new Vector2(0, -380), () => ClosePanel(_aboutPanel));
        }

        // ============================================================
        // 通用工具
        // ============================================================
        private GameObject CreatePanel(string name)
        {
            var go = CreateFullRect(_rootCanvas, name).gameObject;
            var img = go.AddComponent<Image>();
            img.sprite = UiSprites.White;
            img.color = new Color(0.05f, 0.09f, 0.13f, 0.97f);
            img.raycastTarget = false;
            go.SetActive(false);   // 初始隐藏，由控制器开关
            return go;
        }

        private void OpenPanel(GameObject panel)
        {
            // 修复 P1A 单 SetActive(false) 在 Canvas 嵌套下失效导致双面板显示（2026-08-08 统筹）
            // 改用 CanvasGroup.alpha + blocksRaycasts 双保险（Unity 6 推荐方式，alpha 0 立即隐藏内容）
            HidePanel(_mainMenuPanel);
            HidePanel(_newGamePanel);
            HidePanel(_settingsPanel);
            HidePanel(_aboutPanel);
            ShowPanel(panel);
            // 强制 Canvas 立即重绘（避免下一帧才生效）
            Canvas.ForceUpdateCanvases();
        }

        private static void HidePanel(GameObject panel)
        {
            if (panel == null) return;
            var cg = GetOrAddCanvasGroup(panel);
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            panel.SetActive(false);
        }

        private static void ShowPanel(GameObject panel)
        {
            if (panel == null) return;
            var cg = GetOrAddCanvasGroup(panel);
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            panel.SetActive(true);
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject panel)
        {
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            return cg;
        }

        private void ClosePanel(GameObject panel)
        {
            panel.SetActive(false);
            ShowPanel(_mainMenuPanel);
            Canvas.ForceUpdateCanvases();
        }

        private Text CreateText(GameObject parent, string name, string content, int size, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(800, size + 24);
            var text = go.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = content;
            // 文本只负责显示，不能覆盖按钮/输入框的 GraphicRaycaster 命中区域。
            text.raycastTarget = false;
            return text;
        }

        private Text CreateLocalizedText(GameObject parent, string name, string key, int size, Vector2 pos)
        {
            var text = CreateText(parent, name, "", size, pos);
            _localized.Add(new LocalizedText { Text = text, Key = key });
            return text;
        }

        private Button CreateLocalizedButton(GameObject parent, string name, string labelKey, Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 58);
            var img = go.GetComponent<Image>();
            img.sprite = UiSprites.White;
            img.color = new Color(0.2f, 0.36f, 0.52f, 0.95f);

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var label = CreateText(go, "Label", "", 26, Vector2.zero);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.alignment = TextAnchor.MiddleCenter;
            _localized.Add(new LocalizedText { Text = label, Key = labelKey });
            return btn;
        }

        // ============================================================
        // 继续按钮逻辑（SaveSystem 调用，P0 契约）
        // ============================================================
        private void TryContinue()
        {
            if (!SaveSystem.HasSave())
            {
                _continueHintText.text = Localization.Get("menu_no_save");
                return;
            }
            bool ok = SaveSystem.Load();
            if (ok)
            {
                Debug.Log("[MainMenu] 存档加载成功，进入游戏");
                GameManager.Instance.LoadInitialHarbor();
            }
            else
            {
                _continueHintText.text = Localization.IsEnglish ? "Load failed" : "读取失败";
            }
        }

        private void RefreshContinueState()
        {
            bool has = SaveSystem.HasSave();
            if (_continueBtn != null) _continueBtn.interactable = has;   // 无存档置灰
            if (!has && _continueHintText != null)
                _continueHintText.text = Localization.Get("menu_no_save");
        }

        // ============================================================
        // 本地化刷新（语言切换即时生效）
        // ============================================================
        private void RefreshAllTexts()
        {
            if (_refreshingTexts) return;   // 防递归（Dropdown 回调互触）
            _refreshingTexts = true;
            try
            {
                foreach (var lt in _localized)
                    if (lt.Text != null) lt.Text.text = Localization.Get(lt.Key);

                foreach (var ld in _localizedDropdowns)
                {
                    if (ld.Dropdown == null) continue;
                    ld.Dropdown.options.Clear();
                    foreach (var key in ld.Keys)
                        ld.Dropdown.options.Add(new Dropdown.OptionData(Localization.Get(key)));
                    ld.Dropdown.RefreshShownValue();
                }

                // 语言变化时重置语言下拉自身高亮（避免选中项残留英文）
                if (_langDropdown != null)
                    _langDropdown.value = SettingsStore.Lang == "en" ? 1 : 0;
            }
            finally
            {
                _refreshingTexts = false;
            }
        }
    }

    /// <summary>内置字体（UGUI 兼容，避免 TMP 资源依赖）。</summary>
    internal static class Fonts
    {
        private static Font _default;
        private static Font _fallback;
        public static Font Default
        {
            get
            {
                if (_default == null)
                {
                    // 优先加载 Noto Sans CJK（中文字形；位于 Assets/Resources/Fonts/，统筹修复 2026-08-08）
                    _default = Resources.Load<Font>("Fonts/ph_font_noto_sans_cjk_sc");
                    if (_default == null)
                        _default = LoadBuiltinFallback();
                }
                return _default;
            }
        }

        /// <summary>UGUI 的第二候选字体；只在主字体无法覆盖整段文本时使用。</summary>
        public static Font Fallback
        {
            get
            {
                if (_fallback == null)
                    _fallback = LoadBuiltinFallback();
                return _fallback;
            }
        }

        /// <summary>
        /// Unity 版本之间的 legacy 内置字体名称并不稳定。先尝试旧名称，
        /// 再尝试 Unity 6 仍提供的 Arial.ttf，避免运行时 Text.font 为空。
        /// </summary>
        private static Font LoadBuiltinFallback()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        /// <summary>
        /// 判断字体是否能覆盖整段文本。空白和控制字符不参与判断，避免无意义的
        /// 换行/制表符把文本错误地判定为缺字。
        /// </summary>
        internal static bool HasAllCharacters(Font font, string text)
        {
            if (string.IsNullOrEmpty(text)) return font != null;
            if (font == null) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                if (char.IsWhiteSpace(character) || char.IsControl(character)) continue;
                if (!font.HasCharacter(character)) return false;
            }
            return true;
        }

        /// <summary>
        /// 为当前 UGUI 文本选择可覆盖整段内容的字体。没有任何候选完整覆盖时，
        /// 保留调用方字体，避免为了“看起来有回退”而丢失更多中文字形。
        /// </summary>
        internal static Font ResolveForText(string text, Font preferred)
        {
            // Unity 编辑器中的 LegacyRuntime.ttf 可能通过宿主系统返回“似乎支持”
            // 中文，但目标设备未必有同样的系统回退。项目已明确提供 CJK 主字体时，
            // 中文文本必须优先使用它，保证中英文设备上的字形来源一致。
            if (ContainsCjkCharacter(text) && HasAllCharacters(Default, text)) return Default;
            if (HasAllCharacters(preferred, text)) return preferred;
            if (HasAllCharacters(Default, text)) return Default;
            if (HasAllCharacters(Fallback, text)) return Fallback;
            return preferred ?? Default;
        }

        private static bool ContainsCjkCharacter(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                int code = text[i];
                if ((code >= 0x3400 && code <= 0x4DBF) ||
                    (code >= 0x4E00 && code <= 0x9FFF) ||
                    (code >= 0xF900 && code <= 0xFAFF)) return true;
            }
            return false;
        }
    }

    /// <summary>UI Image 默认白底 sprite（修复 P1A 漏 sprite bug：Unity Image 不设 sprite 就不渲染）。</summary>
    internal static class UiSprites
    {
        private static Sprite _white;
        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var tex = Texture2D.whiteTexture;
                    _white = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    _white.name = "Whalebone_WhiteSprite";
                }
                return _white;
            }
        }
    }
}
