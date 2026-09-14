using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D4;
using Whalebone.Gameplay.D9;
using Whalebone.Gameplay.D10;
using Whalebone.Gameplay.UI;

namespace Whalebone.Gameplay.D31
{
    /// <summary>
    /// D31 的短时 Player 交互取证探针。
    ///
    /// 这是一个显式命令行 opt-in 的验证工具：它在独立 Windows Player 中
    /// 通过 UGUI EventSystem 的 pointerClick 入口执行当前 UI 按钮，记录按钮
    /// 是否存在、是否可交互、页面互斥、焦点归属和可见反馈。它不写正式场景
    /// 资产；保存按钮只做入口存在性检查，不执行正式存档写入。
    ///
    /// 注意：pointerClick 不是物理鼠标/键盘输入，因此本探针可以证明运行时
    /// UI 回调链和反馈链，但不能单独把 D29/D30/D31 的物理输入门禁升级为 PASS。
    /// </summary>
    public sealed class D31InteractionEvidenceProbe : MonoBehaviour
    {
        private const string Flag = "-d31-interaction-evidence";
        private const string RunPrefix = "-d31-interaction-evidence-run=";
        private const string OutputPrefix = "-d31-interaction-evidence-output=";
        private const float WaitSeconds = 0.25f;
        private const float MaxWaitSeconds = 8f;

        [Serializable]
        private sealed class CaseRecord
        {
            public string id;
            public string surface;
            public string control;
            public string input;
            public string invocation;
            public string status;
            public bool button_found;
            public bool interactable_before;
            public bool interactable_after;
            public bool feedback_changed;
            public bool focus_owned_after;
            public bool primary_hud_restored_after;
            public string feedback_before;
            public string feedback_after;
            public string active_canvases_before;
            public string active_canvases_after;
            public string selected_before;
            public string selected_after;
            public string error;
        }

        [Serializable]
        private sealed class Result
        {
            public string schema = "D31_INTERACTION_EVIDENCE_PLAYER_V1";
            public string task = "D31-S05";
            public string attempt;
            public string evidence_mode = "standalone Windows x64 Player; UGUI EventSystem pointerClick probe";
            public bool runtime_proof = true;
            public bool physical_os_input = false;
            public bool formal_scene_asset_modified = false;
            public bool isolated_save_used;
            public string isolated_save_path;
            public string unity_version;
            public string graphics_device;
            public string graphics_api;
            public int screen_width;
            public int screen_height;
            public string initial_scene;
            public string final_scene;
            public string[] expected_surfaces;
            public int expected_case_count;
            public int case_count;
            public int observed_case_count;
            public int disabled_case_count;
            public int missing_case_count;
            public int error_case_count;
            public bool all_buttons_found;
            public bool all_cases_observed;
            public CaseRecord[] cases;
            public string[] blockers;
        }

        private sealed class Snapshot
        {
            public string feedback;
            public string activeCanvases;
            public string selected;
        }

        private readonly List<CaseRecord> _cases = new List<CaseRecord>();
        private Result _result;
        private bool _wrote;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!HasFlag(Flag)) return;
            if (FindFirstObjectByType<D31InteractionEvidenceProbe>(FindObjectsInactive.Include) != null)
                return;

            var host = new GameObject("D31InteractionEvidenceProbe");
            DontDestroyOnLoad(host);
            host.AddComponent<D31InteractionEvidenceProbe>();
        }

        private void Awake()
        {
            _result = new Result
            {
                attempt = ReadRunLabel(),
                isolated_save_used = false,
                isolated_save_path = string.Empty,
                unity_version = Application.unityVersion,
                graphics_device = SystemInfo.graphicsDeviceName,
                graphics_api = SystemInfo.graphicsDeviceType.ToString(),
                screen_width = Screen.width,
                screen_height = Screen.height,
                initial_scene = SceneManager.GetActiveScene().name,
                expected_surfaces = new[] { "D29", "D3", "D4", "D9", "D10" },
            };
        }

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return WaitForScene("InitialHarbor", MaxWaitSeconds);
            if (SceneManager.GetActiveScene().name != "InitialHarbor")
            {
                yield return StartNewGameFromMainMenu();
                yield return WaitForScene("InitialHarbor", MaxWaitSeconds);
            }

            yield return WaitForD29(MaxWaitSeconds);
            yield return new WaitForSecondsRealtime(0.75f);
            yield return RunD29Suite();
            yield return RunD3Suite();
            yield return RunD4Suite();
            yield return RunD9Suite();
            yield return RunD10Suite();
            // 离港会把 D3 owner 从 InPort 切换到 Sailing，因此放在所有
            // 港内页面和酒馆用例之后，作为本次探针的最后一个业务动作。
            yield return RunD9DepartCase();

            _result.final_scene = SceneManager.GetActiveScene().name;
            // D29: 11 (including warehouse close, repeat and save presence),
            // D3: 5, D4: 9, D9: 11, D10: 12.
            _result.expected_case_count = 48;
            _result.case_count = _cases.Count;
            _result.observed_case_count = CountStatus("OBSERVED") + CountStatus("DUPLICATE_GUARD_OBSERVED");
            _result.disabled_case_count = CountStatus("DISABLED");
            _result.missing_case_count = CountStatus("MISSING");
            _result.error_case_count = CountStatus("ERROR");
            _result.all_buttons_found = _result.missing_case_count == 0;
            _result.all_cases_observed = _result.observed_case_count == _result.case_count;
            _result.cases = _cases.ToArray();
            _result.blockers = new[]
            {
                "本探针使用 UGUI pointerClick，不是物理 OS 鼠标/键盘输入。",
                "它只覆盖本次独立 Player 中发现的运行时控件，不替代人工/物理输入矩阵。",
                "D31-G06 仍需真实负向、禁用、异常恢复、最终画质和性能门禁。",
            };
            WriteResult();
            Application.Quit();
        }

        private IEnumerator StartNewGameFromMainMenu()
        {
            var newGame = null as Button;
            yield return WaitForButton(null, "BtnNewGame", value => newGame = value, MaxWaitSeconds);
            if (newGame != null) Click(newGame);
            yield return new WaitForSecondsRealtime(WaitSeconds);

            var confirm = null as Button;
            yield return WaitForButton(null, "BtnConfirm", value => confirm = value, MaxWaitSeconds);
            if (confirm != null) Click(confirm);
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private IEnumerator RunD29Suite()
        {
            yield return RunButtonCase("D29-MARKET", "D29", "Market", "B/click", "D29InteractionHudCanvas");
            yield return CloseD3();
            yield return RunWarehouseCase();
            yield return RunButtonCase("D29-TAVERN", "D29", "Tavern", "click", "D29InteractionHudCanvas");
            yield return CloseD10();
            yield return RunButtonCase("D29-SHIPYARD", "D29", "Shipyard", "click", "D29InteractionHudCanvas");
            yield return CloseD4();
            yield return RunButtonCase("D29-BOARD", "D29", "Board", "E/click", "D29InteractionHudCanvas");
            yield return new WaitForSecondsRealtime(WaitSeconds);
            yield return RunButtonCase("D29-BOARD-RESTORE", "D29", "Board", "E/click", "D29InteractionHudCanvas");
            yield return RunButtonCase("D29-WEATHER", "D29", "Weather", "R/click", "D29InteractionHudCanvas");
            // 保存会写入正式 Player 的持久化介质。这里仅验证当前构建中
            // 保存入口存在且可交互，真实保存/Continue 证据沿用 R16 路线。
            yield return RunPresenceCase("D29-SAVE", "D29", "Save", "C/click", "D29InteractionHudCanvas");
            yield return RunButtonCase("D29-PREPARATION", "D29", "Preparation", "P/click", "D29InteractionHudCanvas");
            yield return CloseD9();
            yield return RunRepeatCase();
        }

        private IEnumerator RunRepeatCase()
        {
            var button = FindButton("D29InteractionHudCanvas", "Market");
            if (button == null)
            {
                AddMissing("D29-MARKET-REPEAT", "D29", "Market", "B/click");
                yield break;
            }

            var before = Capture();
            bool interactableBefore = button.interactable;
            if (interactableBefore) Click(button);
            // Market opens D3Canvas, which correctly makes the D29 CanvasGroup
            // non-interactable. Close that modal immediately so the second
            // invocation reaches D29's one-shot policy instead of being
            // swallowed by Button.IsInteractable on a hidden parent group.
            CloseD3Immediate();
            RuntimeHudCoordinator.RestorePrimaryHud();
            yield return null;
            Click(button);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            var afterSecond = Capture();
            var status = afterSecond.feedback.IndexOf("重复点击", StringComparison.Ordinal) >= 0
                ? "DUPLICATE_GUARD_OBSERVED" : "NO_VISIBLE_FEEDBACK";
            AddCase("D29-MARKET-REPEAT", "D29", "Market", "B/click twice", status,
                true, interactableBefore, button.interactable, before, afterSecond,
                ContainsSelected("D29InteractionHudCanvas"), IsPrimaryHudRestored(), string.Empty);
            yield return CloseD3();
        }

        private IEnumerator RunWarehouseCase()
        {
            var button = FindButton("D29InteractionHudCanvas", "Warehouse");
            if (button == null)
            {
                AddMissing("D29-WAREHOUSE", "D29", "Warehouse", "I/click");
                yield break;
            }

            var before = Capture();
            bool interactableBefore = button.interactable;
            if (interactableBefore) Click(button);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            var afterOpen = Capture();
            bool focusOwned = ContainsSelected("WarehousePanel");
            var close = FindButton("D29InteractionHudCanvas", "Close", "WarehousePanel");
            if (close != null && close.interactable) Click(close);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            var afterClose = Capture();
            var status = afterOpen.activeCanvases.IndexOf("D29InteractionHudCanvas", StringComparison.Ordinal) >= 0
                ? "OBSERVED" : "NO_VISIBLE_FEEDBACK";
            AddCase("D29-WAREHOUSE", "D29", "Warehouse", "I/click", status,
                true, interactableBefore, button.interactable, before, afterOpen, focusOwned,
                IsPrimaryHudRestored(), string.Empty);
            AddCase("D29-WAREHOUSE-CLOSE", "D29", "Close", "I/Escape/click", IsPrimaryHudRestored() ? "OBSERVED" : "NO_VISIBLE_FEEDBACK",
                close != null, close != null && close.interactable, close == null ? false : close.interactable,
                afterOpen, afterClose, !ContainsSelected("WarehousePanel"), IsPrimaryHudRestored(), string.Empty);
        }

        private IEnumerator RunD3Suite()
        {
            yield return EnsurePrimary();
            var open = FindButton("D29InteractionHudCanvas", "Market");
            if (open != null && open.interactable) Click(open);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            string[] controls = { "Previous", "Buy", "Sell", "Next", "Close" };
            for (var i = 0; i < controls.Length; i++)
            {
                yield return RunButtonCase("D3-" + controls[i].ToUpperInvariant(), "D3", controls[i], "click", "D3Canvas");
                if (controls[i] == "Close") break;
            }
            yield return CloseD3();
        }

        private IEnumerator RunD4Suite()
        {
            yield return EnsurePrimary();
            var open = FindButton("D29InteractionHudCanvas", "Shipyard");
            if (open != null && open.interactable) Click(open);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            string[] controls = { "Faction", "Combat", "Reload", "Fire", "Upgrade", "Animal", "Medical", "Mainline", "Close" };
            for (var i = 0; i < controls.Length; i++)
            {
                yield return RunButtonCase("D4-" + controls[i].ToUpperInvariant(), "D4", controls[i], "click", "D4Canvas");
            }
            yield return CloseD4();
        }

        private IEnumerator RunD9Suite()
        {
            yield return EnsurePrimary();
            var open = FindButton("D29InteractionHudCanvas", "Preparation");
            if (open != null && open.interactable) Click(open);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            string[] controls = { "Berth", "Market", "Timber", "Tavern", "Supplies", "Repair", "Recruit", "Wages", "Shipyard", "Close" };
            for (var i = 0; i < controls.Length; i++)
            {
                yield return RunButtonCase("D9-" + controls[i].ToUpperInvariant(), "D9", controls[i], "click", "D9PortServicePage");
                if (controls[i] == "Tavern")
                {
                    yield return CloseD10();
                    var d9 = FindFirstObjectByType<D9PortShipPreparationBootstrap>(FindObjectsInactive.Include);
                    if (d9 != null) d9.OpenPreparationPage();
                    yield return new WaitForSecondsRealtime(WaitSeconds);
                }
            }
            yield return CloseD9();
        }

        private IEnumerator RunD10Suite()
        {
            yield return EnsurePrimary();
            var open = FindButton("D29InteractionHudCanvas", "Tavern");
            if (open != null && open.interactable) Click(open);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            string[] controls = { "Close", "Recruit", "Rest", "Meal", "Drink", "Rumor", "Poker", "Dance", "NightTalk", "Board", "SecurityJob", "Skip" };
            // Run the close control last so every action is observed inside the modal.
            for (var i = 1; i < controls.Length; i++)
                yield return RunButtonCase("D10-" + controls[i].ToUpperInvariant(), "D10", controls[i], "click", "D10TavernSocialPage");
            yield return RunButtonCase("D10-CLOSE", "D10", "Close", "Escape/click", "D10TavernSocialPage");
            yield return CloseD10();
        }

        private IEnumerator RunD9DepartCase()
        {
            yield return EnsurePrimary();
            var open = FindButton("D29InteractionHudCanvas", "Preparation");
            if (open != null && open.interactable) Click(open);
            yield return new WaitForSecondsRealtime(WaitSeconds);
            yield return RunButtonCase("D9-DEPART", "D9", "Depart", "D/click", "D9PortServicePage");
            yield return CloseD9();
        }

        private IEnumerator RunButtonCase(string id, string surface, string control, string input, string rootName)
        {
            var button = FindButton(rootName, control);
            if (button == null)
            {
                AddMissing(id, surface, control, input);
                yield break;
            }

            var before = Capture();
            bool interactableBefore = button.interactable;
            string status = "NO_VISIBLE_FEEDBACK";
            string error = string.Empty;
            if (!interactableBefore)
            {
                AddCase(id, surface, control, input, "DISABLED", true, false, button.interactable,
                    before, Capture(), ContainsSelected(rootName), IsPrimaryHudRestored(), string.Empty);
                yield break;
            }

            Exception invokeException = null;
            try
            {
                Click(button);
            }
            catch (Exception exception)
            {
                invokeException = exception;
            }

            if (invokeException != null)
            {
                error = invokeException.GetType().Name + ": " + invokeException.Message;
                AddCase(id, surface, control, input, "ERROR", true, true, button.interactable,
                    before, Capture(), ContainsSelected(rootName), IsPrimaryHudRestored(), error);
                yield break;
            }

            yield return new WaitForSecondsRealtime(WaitSeconds);
            var after = Capture();
            status = after.feedback != before.feedback || after.activeCanvases != before.activeCanvases
                ? "OBSERVED" : "NO_VISIBLE_FEEDBACK";
            AddCase(id, surface, control, input, status, true, true,
                button.interactable, before, after, ContainsSelected(rootName), IsPrimaryHudRestored(), error);
        }

        private IEnumerator RunPresenceCase(string id, string surface, string control, string input, string rootName)
        {
            var button = FindButton(rootName, control);
            if (button == null)
            {
                AddMissing(id, surface, control, input);
                yield break;
            }

            var before = Capture();
            AddCase(id, surface, control, input, "PRESENT_ONLY_NO_MUTATION", true,
                button.interactable, button.interactable, before, before,
                ContainsSelected(rootName), IsPrimaryHudRestored(), "save_invocation_skipped");
            yield return null;
        }

        private void AddMissing(string id, string surface, string control, string input)
        {
            AddCase(id, surface, control, input, "MISSING", false, false, false,
                Capture(), Capture(), false, IsPrimaryHudRestored(), "button_not_found");
        }

        private void AddCase(string id, string surface, string control, string input, string status,
            bool found, bool beforeInteractable, bool afterInteractable, Snapshot before,
            Snapshot after, bool focusOwned, bool primaryRestored, string error)
        {
            _cases.Add(new CaseRecord
            {
                id = id,
                surface = surface,
                control = control,
                input = input,
                invocation = "ExecuteEvents.pointerClickHandler",
                status = status,
                button_found = found,
                interactable_before = beforeInteractable,
                interactable_after = afterInteractable,
                feedback_changed = before.feedback != after.feedback || before.activeCanvases != after.activeCanvases,
                focus_owned_after = focusOwned,
                primary_hud_restored_after = primaryRestored,
                feedback_before = Limit(before.feedback),
                feedback_after = Limit(after.feedback),
                active_canvases_before = before.activeCanvases,
                active_canvases_after = after.activeCanvases,
                selected_before = before.selected,
                selected_after = after.selected,
                error = error ?? string.Empty,
            });
        }

        private IEnumerator EnsurePrimary()
        {
            CloseD3Immediate();
            CloseD4Immediate();
            CloseD9Immediate();
            CloseD10Immediate();
            RuntimeHudCoordinator.RestorePrimaryHud();
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private IEnumerator CloseD3()
        {
            CloseD3Immediate();
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private IEnumerator CloseD4()
        {
            CloseD4Immediate();
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private IEnumerator CloseD9()
        {
            CloseD9Immediate();
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private IEnumerator CloseD10()
        {
            CloseD10Immediate();
            yield return new WaitForSecondsRealtime(WaitSeconds);
        }

        private static void CloseD3Immediate()
        {
            var d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d3 != null) d3.CloseTradeFromCoordinator();
        }

        private static void CloseD4Immediate()
        {
            var d4 = FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include);
            if (d4 != null) d4.CloseContentPage();
        }

        private static void CloseD9Immediate()
        {
            var d9 = FindFirstObjectByType<D9PortShipPreparationBootstrap>(FindObjectsInactive.Include);
            if (d9 != null) d9.ClosePreparationPage();
        }

        private static void CloseD10Immediate()
        {
            var d10 = FindFirstObjectByType<D10TavernSocialBootstrap>(FindObjectsInactive.Include);
            if (d10 != null) d10.CloseSocialPage();
        }

        private static void Click(Button button)
        {
            if (button == null || EventSystem.current == null) return;
            var data = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
            };
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
        }

        private Snapshot Capture()
        {
            return new Snapshot
            {
                feedback = ReadFeedback(),
                activeCanvases = ReadActiveCanvases(),
                selected = EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null
                    ? string.Empty : EventSystem.current.currentSelectedGameObject.name,
            };
        }

        private string ReadFeedback()
        {
            var values = new List<string>();
            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < texts.Length; i++)
            {
                var text = texts[i];
                if (text == null || !text.gameObject.activeInHierarchy || string.IsNullOrWhiteSpace(text.text)) continue;
                var name = text.gameObject.name;
                if (!name.Contains("Status") && !name.Contains("Message") && !name.Contains("Hint") &&
                    !name.Contains("Contents") && !name.Contains("Detail") && !name.Contains("Result") &&
                    !name.Contains("Catalog")) continue;
                var value = name + "=" + text.text.Replace('\n', '|');
                if (!values.Contains(value)) values.Add(value);
            }
            // FindObjectsByType 的返回顺序不是交互证据的一部分。收集全部
            // 相关文本并排序，避免 D4/D9 的 Hint 或 D3 的 Result 被前六个
            // 无关状态栏挤掉，造成“按钮已执行但无反馈”的假阴性。
            values.Sort(StringComparer.Ordinal);
            return string.Join(" || ", values.ToArray());
        }

        private static string ReadActiveCanvases()
        {
            var names = new List<string>();
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas == null || !canvas.enabled || !canvas.gameObject.activeInHierarchy) continue;
                var group = canvas.GetComponent<CanvasGroup>();
                if (group != null && (group.alpha <= 0.001f || !group.interactable || !group.blocksRaycasts)) continue;
                if (canvas.gameObject.name == "D29InteractionHudCanvas" ||
                    canvas.gameObject.name == "D29WarehousePanel" ||
                    canvas.gameObject.name == "D3Canvas" ||
                    canvas.gameObject.name == "D4Canvas" ||
                    canvas.gameObject.name == "D9PortServicePage" ||
                    canvas.gameObject.name == "D10TavernSocialPage")
                    names.Add(canvas.gameObject.name);
            }
            names.Sort(StringComparer.Ordinal);
            return string.Join(",", names.ToArray());
        }

        private static Button FindButton(string rootName, string buttonName, string childRoot = null)
        {
            var root = FindRoot(rootName);
            if (root == null) return null;
            var search = string.IsNullOrEmpty(childRoot) ? root.transform : FindChild(root.transform, childRoot);
            if (search == null) return null;
            var buttons = search.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && buttons[i].gameObject.name == buttonName) return buttons[i];
            return null;
        }

        private static Button FindGlobalButton(string buttonName)
        {
            var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && buttons[i].gameObject.name == buttonName && buttons[i].gameObject.activeInHierarchy)
                    return buttons[i];
            return null;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindRoot(string name)
        {
            var direct = GameObject.Find(name);
            if (direct != null) return direct;
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
                if (canvases[i] != null && canvases[i].gameObject.name == name) return canvases[i].gameObject;
            return null;
        }

        private static bool ContainsSelected(string rootName)
        {
            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            var root = FindRoot(rootName);
            return selected != null && root != null && selected.transform.IsChildOf(root.transform);
        }

        private static bool IsPrimaryHudRestored()
        {
            // There can be more than one compatibility HUD object during a
            // scene transition. FindRoot() intentionally returns one object,
            // but that made this diagnostic disagree with ReadActiveCanvases()
            // when the first same-named object was stale or hidden. Treat the
            // primary HUD as restored when any live, interactive D29 canvas is
            // present; this is the same observable contract used by the active
            // canvas reader and does not mutate runtime state.
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (canvas == null || canvas.gameObject.name != "D29InteractionHudCanvas" ||
                    !canvas.enabled || !canvas.gameObject.activeInHierarchy) continue;
                var group = canvas.GetComponent<CanvasGroup>();
                if (group == null || (group.alpha > 0.5f && group.interactable && group.blocksRaycasts))
                    return true;
            }
            return false;
        }

        private static IEnumerator WaitForScene(string sceneName, float seconds)
        {
            var deadline = Time.realtimeSinceStartup + seconds;
            while (SceneManager.GetActiveScene().name != sceneName && Time.realtimeSinceStartup < deadline)
                yield return null;
        }

        private static IEnumerator WaitForD29(float seconds)
        {
            var deadline = Time.realtimeSinceStartup + seconds;
            while (FindRoot("D29InteractionHudCanvas") == null && Time.realtimeSinceStartup < deadline)
                yield return null;
        }

        private static IEnumerator WaitForButton(GameObject ignoredRoot, string buttonName, Action<Button> setter, float seconds)
        {
            var deadline = Time.realtimeSinceStartup + seconds;
            Button button = null;
            while (button == null && Time.realtimeSinceStartup < deadline)
            {
                button = FindGlobalButton(buttonName);
                if (button == null) yield return null;
            }
            setter(button);
        }

        private int CountStatus(string status)
        {
            var count = 0;
            for (var i = 0; i < _cases.Count; i++)
                if (_cases[i].status == status) count++;
            return count;
        }

        private static string Limit(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= 480) return value ?? string.Empty;
            return value.Substring(0, 480) + "…";
        }

        private void WriteResult()
        {
            if (_wrote || _result == null) return;
            _wrote = true;
            var path = Path.Combine(ReadOutputDirectory(),
                "D31_S05_INTERACTION_EVIDENCE_PLAYER_" + ReadRunLabel() + ".json");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(_result, true));
                Debug.Log("[D31-S05] Interaction evidence result written: " + path);
            }
            catch (Exception exception)
            {
                Debug.LogError("[D31-S05] Interaction evidence result write failed: " + exception);
            }
        }

        private void OnApplicationQuit()
        {
            WriteResult();
        }

        private void OnDestroy()
        {
            WriteResult();
        }

        private static bool HasFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string ReadOutputDirectory()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith(OutputPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                var raw = args[i].Substring(OutputPrefix.Length).Trim().Trim('"');
                if (!string.IsNullOrEmpty(raw)) return raw;
            }

            return Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "tools/results/D31/D31-S05");
        }

        private static string ReadRunLabel()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].StartsWith(RunPrefix, StringComparison.OrdinalIgnoreCase)) continue;
                var raw = args[i].Substring(RunPrefix.Length).Trim();
                if (string.IsNullOrEmpty(raw)) break;
                var chars = raw.ToCharArray();
                for (var c = 0; c < chars.Length; c++)
                    if (!(char.IsLetterOrDigit(chars[c]) || chars[c] == '-' || chars[c] == '_')) chars[c] = '_';
                return new string(chars);
            }
            return "R39";
        }
    }
}
