using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Whalebone.Systems.D13;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.UI.D16
{
    /// <summary>
    /// D16-A03 的金融与债务只读页面。
    /// 页面只消费 FinanceSystem 生成并由 Save 域持久化的快照，不提交金融操作，
    /// 也不计算或改写任何金融数值。
    /// </summary>
    [DefaultExecutionOrder(-860)]
    public sealed class D16FinanceDebtPanel : MonoBehaviour
    {
        public static D16FinanceDebtPanel Instance { get; private set; }
        public static bool IsOpen { get; private set; }

        private GameObject _canvasGo;
        private GameObject _modal;
        private Text _bodyText;
        private Button _closeButton;
        private bool _previousGameplayInputBlocked;

        public static void ShowPanel()
        {
            var panel = EnsureInstance();
            if (panel != null) panel.OpenInternal();
        }

        public static void HidePanel()
        {
            if (Instance != null) Instance.CloseInternal();
        }

        /// <summary>把只读金融快照投影成当前语言的文本，不修改传入快照。</summary>
        public static string BuildDebtSummary(D13FinanceRuntimeSnapshot snapshot)
        {
            if (snapshot == null)
                return Localization.Get("finance_empty");

            string locale = Localization.IsEnglish ? "en-US" : "zh-CN";
            var lines = new StringBuilder();
            lines.AppendLine(Localization.Get("finance_revision") + ": " +
                D16DisplayFormatter.Number(snapshot.Revision, locale));

            bool hasContent = false;
            if (snapshot.Accounts != null)
            {
                foreach (var account in snapshot.Accounts)
                {
                    if (account == null) continue;
                    hasContent = true;
                    lines.AppendLine();
                    lines.AppendLine(Localization.Get("finance_account") + ": " + SafeToken(account.AccountId));
                    lines.AppendLine("  " + Localization.Get("finance_cash") + ": " +
                        D16DisplayFormatter.Number(account.CashBalance, locale));
                    lines.AppendLine("  " + Localization.Get("finance_credit_used") + ": " +
                        D16DisplayFormatter.Number(account.CreditUsed, locale) + " / " +
                        D16DisplayFormatter.Number(account.CreditLimit, locale));
                    lines.AppendLine("  " + Localization.Get("finance_credit_score") + ": " +
                        D16DisplayFormatter.Number(account.CreditScore, locale));
                }
            }

            if (snapshot.Debts != null)
            {
                foreach (var debt in snapshot.Debts)
                {
                    if (debt == null) continue;
                    hasContent = true;
                    lines.AppendLine();
                    lines.AppendLine(Localization.Get("finance_debt") + ": " + SafeToken(debt.DebtId));
                    lines.AppendLine("  " + Localization.Get("finance_state") + ": " + StateLabel(debt.State));
                    lines.AppendLine("  " + Localization.Get("finance_principal") + ": " +
                        D16DisplayFormatter.Number(debt.Principal, locale));
                    lines.AppendLine("  " + Localization.Get("finance_interest") + ": " +
                        D16DisplayFormatter.Number(debt.InterestAccrued, locale));
                    lines.AppendLine("  " + Localization.Get("finance_penalty") + ": " +
                        D16DisplayFormatter.Number(debt.LatePenalty, locale));
                    lines.AppendLine("  " + Localization.Get("finance_total_due") + ": " +
                        D16DisplayFormatter.Number(debt.TotalDue, locale));
                    lines.AppendLine("  " + Localization.Get("finance_due_day") + ": " +
                        D16DisplayFormatter.Number(debt.DueDay, locale));
                    if (debt.GraceUntilDay > 0)
                        lines.AppendLine("  " + Localization.Get("finance_grace_day") + ": " +
                            D16DisplayFormatter.Number(debt.GraceUntilDay, locale));
                    lines.AppendLine("  " + Localization.Get("finance_risk") + ": " + SafeText(debt.RiskExplanation));
                    lines.AppendLine("  " + Localization.Get("finance_worst_case") + ": " + SafeText(debt.WorstCaseOutcome));
                }
            }

            return hasContent ? lines.ToString().TrimEnd() : Localization.Get("finance_empty");
        }

        private static D16FinanceDebtPanel EnsureInstance()
        {
            if (Instance != null) return Instance;
            var existing = FindFirstObjectByType<D16FinanceDebtPanel>(FindObjectsInactive.Include);
            if (existing != null) return existing;

            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "InitialHarbor" && sceneName != "MainMenu") return null;
            return new GameObject("D16FinanceDebtPanel").AddComponent<D16FinanceDebtPanel>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureEventSystem();
            BuildCanvas();
            BuildPanel();
            D16ModalFocusScope.Attach(_modal, _closeButton == null ? null : _closeButton.gameObject);
            _modal.SetActive(false);
            IsOpen = false;
        }

        private void Update()
        {
            if (!IsOpen) return;
            if ((Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame))
                CloseInternal();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsOpen)
                D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            IsOpen = false;
        }

        private void OpenInternal()
        {
            if (_modal == null) return;
            if (IsOpen) return;

            D16AccessibilityPanel.HidePanel();
            D16LocalDiagnosticsPanel.HidePanel();

            _previousGameplayInputBlocked = D16InputRouter.GameplayInputBlocked;
            _bodyText.text = BuildDebtSummary(GameState.CurrentD13FinanceRuntime);
            _modal.SetActive(true);
            IsOpen = true;
            D16InputRouter.SetGameplayInputBlocked(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Canvas.ForceUpdateCanvases();
            if (EventSystem.current != null && _closeButton != null)
                EventSystem.current.SetSelectedGameObject(_closeButton.gameObject);
            Debug.Log("[D16/A03] 金融与债务只读页面已打开：未提交任何金融操作");
        }

        private void CloseInternal()
        {
            if (_modal == null || !IsOpen) return;
            _modal.SetActive(false);
            IsOpen = false;
            D16InputRouter.SetGameplayInputBlocked(_previousGameplayInputBlocked);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[D16/A03] 金融与债务只读页面已关闭");
        }

        private static string StateLabel(D13FinanceDebtLifecycleState state)
        {
            string value = Localization.Get("finance_state_" + state);
            return value == "finance_state_" + state ? state.ToString() : value;
        }

        private static string SafeToken(string value)
        {
            return SafeText(value, "-");
        }

        private static string SafeText(string value, string fallback = "-")
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();

            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
                if (other != eventSystem) other.gameObject.SetActive(false);

            eventSystem.gameObject.SetActive(true);
            EventSystem.current = eventSystem;
            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
                if (module != inputModule) module.enabled = false;
            eventSystem.UpdateModules();
        }

        private void BuildCanvas()
        {
            _canvasGo = new GameObject("D16FinanceDebtCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasGo.transform.SetParent(transform, false);
            var canvas = _canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 850;
            var scaler = _canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private void BuildPanel()
        {
            _modal = new GameObject("FinanceDebtModal", typeof(RectTransform), typeof(Image));
            _modal.transform.SetParent(_canvasGo.transform, false);
            Stretch((RectTransform)_modal.transform);
            var modalImage = _modal.GetComponent<Image>();
            modalImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            modalImage.color = new Color(0.01f, 0.02f, 0.04f, 0.9f);
            modalImage.raycastTarget = true;

            var card = new GameObject("FinanceDebtCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(_modal.transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(1200f, 880f);
            var cardImage = card.GetComponent<Image>();
            cardImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            cardImage.color = new Color(0.025f, 0.055f, 0.09f, 0.99f);

            CreateText(card, "Title", Localization.Get("finance_title"), 40,
                new Vector2(0f, 382f), new Vector2(1080f, 58f));
            CreateText(card, "Subtitle", Localization.Get("finance_subtitle"), 18,
                new Vector2(0f, 330f), new Vector2(1060f, 58f));
            _bodyText = CreateScrollableBody(card);
            _closeButton = CreateButton(card, "Close", Localization.Get("finance_close"),
                new Vector2(0f, -395f), CloseInternal, new Vector2(280f, 54f), 19);
        }

        private static Text CreateScrollableBody(GameObject card)
        {
            var scrollGo = new GameObject("FinanceDebtScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(card.transform, false);
            var scrollRectTransform = (RectTransform)scrollGo.transform;
            scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRectTransform.sizeDelta = new Vector2(1080f, 620f);
            scrollRectTransform.anchoredPosition = new Vector2(0f, -20f);
            var scrollImage = scrollGo.GetComponent<Image>();
            scrollImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            scrollImage.color = new Color(0.01f, 0.025f, 0.045f, 0.86f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            Stretch((RectTransform)viewport.transform);
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            viewportImage.color = new Color(0f, 0f, 0f, 0f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.sizeDelta = Vector2.zero;
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 20);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(Text));
            bodyGo.transform.SetParent(content.transform, false);
            var body = bodyGo.GetComponent<Text>();
            body.font = Whalebone.Gameplay.UI.Fonts.Default;
            body.fontSize = 20;
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.color = Color.white;
            body.raycastTarget = false;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 42f;
            scroll.verticalNormalizedPosition = 1f;
            return body;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position,
            UnityEngine.Events.UnityAction onClick, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var image = go.GetComponent<Image>();
            image.sprite = Whalebone.Gameplay.UI.UiSprites.White;
            image.color = new Color(0.13f, 0.36f, 0.56f, 0.98f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var colors = button.colors;
            colors.highlightedColor = new Color(0.25f, 0.55f, 0.8f, 1f);
            colors.pressedColor = new Color(0.08f, 0.22f, 0.35f, 1f);
            button.colors = colors;
            var text = CreateText(go, "Label", label, fontSize, Vector2.zero, size);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string value, int size,
            Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = position;
            var text = go.GetComponent<Text>();
            text.font = Whalebone.Gameplay.UI.Fonts.Default;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
