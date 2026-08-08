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
using UnityEngine.UI;
using Whalebone.Core;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.UI
{
    /// <summary>
    /// 挂载于 MainMenu 场景的 Bootstrap 空物体。运行后动态构建 UGUI。
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        // ---- 自动挂载：MainMenu 场景启动时创建控制器 ----
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            // 仅当当前场景是 MainMenu 时创建（避免其他场景误挂）
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
                return;

            // 确保 GameManager 存在（P0 单例）
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();

            var host = new GameObject("MainMenuController");
            host.AddComponent<MainMenuController>();
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
            BuildCanvas();
            BuildMainMenu();
            BuildNewGamePanel();
            BuildSettingsPanel();
            BuildAboutPanel();

            SettingsStore.ApplyAll();   // 启动应用持久化设置
            RefreshAllTexts();          // 初始语言应用
            RefreshContinueState();     // 继续按钮初始状态
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

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.transform.SetParent(transform, false);
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
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
            var rt = CreateFullRect(parent, name);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(320, 64);

            img = rt.gameObject.AddComponent<Image>();
            img.sprite = UiSprites.White;
            img.color = new Color(0.15f, 0.28f, 0.4f, 0.92f);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            if (name == "BtnContinue") _continueBtn = btn;

            // 按钮文字（本地化注册）
            var label = CreateText(rt.gameObject, "Label", "", 32, Vector2.zero);
            label.alignment = TextAnchor.MiddleCenter;
            _localized.Add(new LocalizedText { Text = label, Key = textKey });
        }

        // ============================================================
        // 新游戏面板
        // ============================================================
        private void BuildNewGamePanel()
        {
            _newGamePanel = CreatePanel("NewGamePanel");

            CreateLocalizedText(_newGamePanel, "Title", "newgame_title", 48, new Vector2(0, 150));

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
            _captainInput.textComponent = inputText;

            var placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(textArea.transform, false);
            var phRt = (RectTransform)placeholder.transform;
            phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
            var phText = placeholder.AddComponent<Text>();
            phText.font = Fonts.Default;
            phText.fontSize = 26;
            phText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            _captainInput.placeholder = phText;
            _captainInput.textComponent.text = "";
            _localized.Add(new LocalizedText { Text = phText, Key = "newgame_placeholder_name" });

            _newGameErrorText = CreateText(_newGamePanel, "ErrorText", "", 24, new Vector2(0, 0));
            _newGameErrorText.color = Color.red;

            CreateLocalizedButton(_newGamePanel, "BtnConfirm", "newgame_confirm", new Vector2(-100, -110), () =>
            {
                string name = _captainInput.text.Trim();
                if (name.Length == 0)
                {
                    _newGameErrorText.text = Localization.IsEnglish ? "Name required" : "请输入船长名";
                    return;
                }
                _newGameErrorText.text = "";

                // P0 契约：只调用 SaveSystem 公开接口
                SaveSystem.NewGame(new NewGameParams
                {
                    CaptainName = name,
                    ShipTypeId = "small_fishing"   // M0 固定小渔船
                });
                Debug.Log("[MainMenu] NewGame 已发起：" + name);

                // P1C 场景未实现时 LoadInitialHarbor 只打日志不崩溃（GameManager 已容错）
                GameManager.Instance.LoadInitialHarbor();
            });

            CreateLocalizedButton(_newGamePanel, "BtnCancel", "newgame_cancel", new Vector2(100, -110), () => ClosePanel(_newGamePanel));
        }

        // ============================================================
        // 设置面板
        // ============================================================
        private void BuildSettingsPanel()
        {
            _settingsPanel = CreatePanel("SettingsPanel");

            CreateLocalizedText(_settingsPanel, "Title", "settings_title", 48, new Vector2(0, 220));

            CreateSliderRow(_settingsPanel, "settings_volume_master", SettingsStore.MasterVolume, 110f, v => { SettingsStore.MasterVolume = v; AudioListener.volume = v; });
            CreateSliderRow(_settingsPanel, "settings_volume_music", SettingsStore.MusicVolume, 40f, v => SettingsStore.MusicVolume = v);
            CreateSliderRow(_settingsPanel, "settings_volume_sfx", SettingsStore.SfxVolume, -30f, v => SettingsStore.SfxVolume = v);

            // 画质
            CreateLocalizedText(_settingsPanel, "QualityLabel", "settings_quality", 26, new Vector2(-330, -100));
            var qualityDropdown = CreateDropdown(_settingsPanel, new Vector2(0, -100), new[] { "quality_low", "quality_medium", "quality_high" });
            qualityDropdown.Dropdown.value = SettingsStore.QualityLevel;
            qualityDropdown.Dropdown.onValueChanged.AddListener(v =>
            {
                SettingsStore.QualityLevel = v;
                QualitySettings.SetQualityLevel(v, true);
            });

            // 语言
            CreateLocalizedText(_settingsPanel, "LangLabel", "settings_language", 26, new Vector2(-330, -170));
            _langDropdown = CreateDropdown(_settingsPanel, new Vector2(0, -170), new[] { "lang_zh", "lang_en" }).Dropdown;
            _langDropdown.value = SettingsStore.Lang == "en" ? 1 : 0;
            _langDropdown.onValueChanged.AddListener(v =>
            {
                SettingsStore.Lang = v == 1 ? "en" : "zh";
                RefreshAllTexts();   // 即时生效
            });

            // 键位说明（只读）
            CreateLocalizedText(_settingsPanel, "ControlsTitle", "settings_controls", 28, new Vector2(0, -240));
            CreateLocalizedText(_settingsPanel, "ControlsBody", "controls_text", 22, new Vector2(0, -310));

            CreateLocalizedButton(_settingsPanel, "BtnClose", "btn_back", new Vector2(0, -430), () => ClosePanel(_settingsPanel));
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
            go.SetActive(false);   // 初始隐藏，由控制器开关
            return go;
        }

        private void OpenPanel(GameObject panel)
        {
            _mainMenuPanel.SetActive(false);
            panel.SetActive(true);
        }

        private void ClosePanel(GameObject panel)
        {
            panel.SetActive(false);
            _mainMenuPanel.SetActive(true);
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
        public static Font Default
        {
            get
            {
                if (_default == null)
                {
                    // 优先加载 Noto Sans CJK（中文字形；位于 Assets/Resources/Fonts/，统筹修复 2026-08-08）
                    _default = Resources.Load<Font>("Fonts/ph_font_noto_sans_cjk_sc");
                    if (_default == null)
                        _default = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // 兜底（无中文字形）
                }
                return _default;
            }
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
