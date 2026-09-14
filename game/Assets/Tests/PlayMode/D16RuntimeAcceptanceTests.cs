using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Whalebone.Gameplay.D16;
using Whalebone.Systems.D13;
using Whalebone.Systems.D16;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.D16
{
    /// <summary>
    /// D16 的 Unity 6 PlayMode 最小运行时验收。
    /// 它验证真实场景加载后的对象接线和探针采样，不把静态检查当运行时通过。
    /// </summary>
    public sealed class D16RuntimeAcceptanceTests
    {
        private const string SceneName = "InitialHarbor";
        private const string ReportRelativePath = "tools/results/d16_runtime_playmode_report.json";
        private const string A01FixtureReportRelativePath = "tools/results/d16_a01_runtime_fixture_report.json";
        private const string A01PerformanceWindowReportRelativePath = "tools/results/d16_a01_performance_window_report.json";
        private const string A01BudgetStressReportRelativePath = "tools/results/d16_a01_budget_stress_report.json";
        private const string A01StatePreservationReportRelativePath = "tools/results/d16_a01_state_preservation_report.json";
        private const string A03AccessibilityReportRelativePath = "tools/results/d16_a03_accessibility_runtime_report.json";
        private const string A03D9SubtitleReportRelativePath = "tools/results/d16_a03_d9_subtitle_runtime_report.json";
        private const string A03D9CueCoverageReportRelativePath = "tools/results/d16_a03_d9_cue_coverage_report.json";

        [Test]
        public void D16A03StaticUiLocalizationTablesAreSymmetricAndNonEmpty()
        {
            Type localizationType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                localizationType = assembly.GetType("Whalebone.Gameplay.UI.Localization");
                if (localizationType != null) break;
            }

            Assert.IsNotNull(localizationType, "Assembly-CSharp 未加载 Localization。");
            var validate = localizationType.GetMethod("ValidateTables");
            var get = localizationType.GetMethod("Get");
            Assert.IsNotNull(validate, "Localization 缺少静态表校验入口。");
            Assert.IsNotNull(get, "Localization 缺少统一 Get 入口。");

            var errors = (IList)validate.Invoke(null, null);
            Assert.IsEmpty(errors, "静态 UI 本地化表存在结构问题。");
            var menuLabel = (string)get.Invoke(null, new object[] { "menu_new_game" });
            Assert.AreNotEqual("menu_new_game", menuLabel, "已登记 key 不应回退为 key 本身。");
        }

        [UnityTest]
        public IEnumerator D16A03FinanceDebtPanelRendersReadOnlyRuntimeSnapshot()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var previousSnapshot = GameState.CurrentD13FinanceRuntime;
            const string languageKey = "wb_lang";
            var previousLanguage = PlayerPrefs.GetString(languageKey, "zh");
            try
            {
                PlayerPrefs.SetString(languageKey, "zh");
                PlayerPrefs.Save();
                var snapshot = new D13FinanceRuntimeSnapshot { Revision = 7 };
                snapshot.Accounts.Add(new D13FinanceAccountState
                {
                    AccountId = "account_test",
                    OwnerRef = D13FinanceIds.PlayerRef,
                    CashBalance = 1200,
                    CreditLimit = 5000,
                    CreditUsed = 1400,
                    CreditScore = 64,
                });
                snapshot.Debts.Add(new D13FinanceDebtState
                {
                    DebtId = "debt_test",
                    AccountId = "account_test",
                    DebtorRef = D13FinanceIds.PlayerRef,
                    CreditorRef = "merchant_test",
                    Kind = D13FinanceDebtKind.Loan,
                    State = D13FinanceDebtLifecycleState.PastDue,
                    Principal = 2000,
                    InterestAccrued = 300,
                    LatePenalty = 200,
                    DueDay = 12,
                    GraceUntilDay = 15,
                    SourceEventId = "event_test_finance",
                    RiskExplanation = "测试风险说明",
                    WorstCaseOutcome = "测试最坏结果",
                });
                GameState.CurrentD13FinanceRuntime = snapshot;

                var panelType = Type.GetType("Whalebone.Gameplay.UI.D16.D16FinanceDebtPanel, Assembly-CSharp");
                Assert.IsNotNull(panelType, "D16FinanceDebtPanel 未加载。");
                var build = panelType.GetMethod("BuildDebtSummary");
                Assert.IsNotNull(build, "金融页面缺少只读快照投影入口。");
                string summary = (string)build.Invoke(null, new object[] { snapshot });
                StringAssert.Contains("2,000", summary, "债务本金没有进入 UI 投影。");
                StringAssert.Contains("2,500", summary, "当前应付没有使用 FinanceSystem 快照的总额。");
                StringAssert.Contains("测试风险说明", summary, "债务风险说明没有进入 UI 投影。");

                panelType.GetMethod("ShowPanel").Invoke(null, null);
                yield return null;
                var host = GameObject.Find("D16FinanceDebtPanel");
                Assert.IsNotNull(host, "金融与债务页面宿主没有创建。");
                Assert.IsNotEmpty(host.GetComponentsInChildren<Text>(true),
                    "金融与债务页面没有创建文本控件。");
                Assert.IsTrue((bool)panelType.GetProperty("IsOpen").GetValue(null),
                    "金融与债务页面没有打开。");
                panelType.GetMethod("HidePanel").Invoke(null, null);
                yield return null;
                Assert.IsFalse((bool)panelType.GetProperty("IsOpen").GetValue(null),
                    "金融与债务页面关闭后仍保持打开状态。");
            }
            finally
            {
                GameState.CurrentD13FinanceRuntime = previousSnapshot;
                PlayerPrefs.SetString(languageKey, previousLanguage);
                PlayerPrefs.Save();
                var panelType = Type.GetType("Whalebone.Gameplay.UI.D16.D16FinanceDebtPanel, Assembly-CSharp");
                if (panelType != null) panelType.GetMethod("HidePanel")?.Invoke(null, null);
            }
        }

        [UnityTest]
        public IEnumerator D16A03RuntimeNumberFormattingFeedsUiText()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var fixture = new GameObject("D16_A03_NumberFormattingFixture",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = fixture.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var label = new GameObject("FormattedNumberLabel", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(fixture.transform, false);
            var text = label.GetComponent<Text>();
            text.text = D16DisplayFormatter.Number(1234.5m, "en-US", 2);
            Assert.AreEqual("1,234.50", text.text, "英文数字格式没有进入 UGUI 文本。");

            text.text = D16DisplayFormatter.Number(1234.5m, "zh-CN", 2);
            Assert.AreEqual("1,234.50", text.text, "中文数字格式没有进入 UGUI 文本。");
            text.text = D16DisplayFormatter.SignedNumber(-12m, "en-US");
            Assert.AreEqual("-12", text.text, "负数符号格式不正确。");
            text.text = D16DisplayFormatter.Percent(0.25m, "zh-CN");
            Assert.AreEqual("25%", text.text, "百分比格式不正确。");

            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        [Test]
        public void D16A03D9PresentationReadModelFormatsDomainNumbersBeforeUiConsumption()
        {
            var readModelType = FindLoadedType("Whalebone.Gameplay.D9.D9PresentationReadModel");
            var d3Type = FindLoadedType("Whalebone.Systems.D3.D3RuntimeSnapshot");
            var preparationType = FindLoadedType("Whalebone.Systems.D9.D9PortShipPreparationSnapshot");
            var shipType = FindLoadedType("Whalebone.Systems.D9.D9ShipPreparationSnapshot");
            Assert.IsNotNull(readModelType, "D9PresentationReadModel 未被主程序集加载。");
            Assert.IsNotNull(d3Type, "D3RuntimeSnapshot 未被主程序集加载。");
            Assert.IsNotNull(preparationType, "D9PortShipPreparationSnapshot 未被主程序集加载。");
            Assert.IsNotNull(shipType, "D9ShipPreparationSnapshot 未被主程序集加载。");

            var d3 = Activator.CreateInstance(d3Type);
            WritePublicField(d3, "CurrentDay", 2);
            WritePublicField(d3, "TotalDays", 12);
            var market = new System.Collections.Generic.Dictionary<string, int> { ["fish"] = 12345 };
            WritePublicField(d3, "MarketStock", market);

            var preparation = Activator.CreateInstance(preparationType);
            WritePublicField(preparation, "Money", 12345);
            var ship = Activator.CreateInstance(shipType);
            WritePublicField(ship, "Weight", 1234);
            WritePublicField(ship, "CargoCapacity", 9876);
            WritePublicField(ship, "Draft", 1.25f);
            WritePublicField(ship, "CrewCount", 12);
            WritePublicField(ship, "CrewCapacity", 24);
            WritePublicField(ship, "Water", 1000);
            WritePublicField(ship, "Food", 2000);
            WritePublicField(ship, "Damage", 3);
            WritePublicField(ship, "Cargo", new System.Collections.Generic.Dictionary<string, int>
            {
                ["fish"] = 12345,
            });
            WritePublicField(preparation, "Ship", ship);

            var build = readModelType.GetMethod("Build", BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(build, "D9PresentationReadModel 缺少 Build 入口。");
            var snapshot = build.Invoke(null, new[] { d3, null, preparation, null, null, (object)"en-US" });

            StringAssert.Contains("12,345", (string)ReadPublicField(snapshot, "Money"));
            var shipText = (string)ReadPublicField(snapshot, "Ship");
            StringAssert.Contains("1,234/9,876", shipText);
            StringAssert.Contains("1.3m", shipText);
            StringAssert.Contains("航程 2/12 天", (string)ReadPublicField(snapshot, "VoyageStatus"));
            StringAssert.Contains("12,345", (string)ReadPublicField(snapshot, "Market"));
            StringAssert.Contains("fish×12,345", shipText);
        }

        private static void WritePublicField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public).SetValue(target, value);
        }

        private static object ReadPublicField(object target, string name)
        {
            return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public).GetValue(target);
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        [Serializable]
        private sealed class A03AccessibilityReport
        {
            public string Scene;
            public string UnityVersion;
            public bool CjkFontResourceLoaded;
            public bool RuntimeAdapterFound;
            public bool LongTextWrapped;
            public bool LongTextOverflowConfigured;
            public bool TextScaleApplied;
            public bool HighContrastApplied;
            public bool ColorVisionTransformApplied;
            public bool LowStimulusApplied;
            public bool Passed;
            public string FailureReason;
        }

        /// <summary>
        /// 在真实 UI Graphic 上验证长文本、字体、文字缩放和颜色辅助设置。
        /// 这是代码级回归，不替代目标分辨率/真实设备上的人工排版与色觉复核。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A03AccessibilityRuntimeAppliesLongTextFontScaleAndColorModes()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var runtimeType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilityRuntime, Assembly-CSharp");
            var runtime = runtimeType == null
                ? null
                : UnityEngine.Object.FindFirstObjectByType(runtimeType, FindObjectsInactive.Include);
            var apply = runtimeType?.GetMethod("ApplyToSceneUi",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(runtimeType, "D16AccessibilityRuntime 未被 Unity 主程序集加载。");
            Assert.IsNotNull(runtime, "D16AccessibilityRuntime 未创建。");
            Assert.IsNotNull(apply, "D16AccessibilityRuntime 缺少 UI 应用入口。");

            var settingsStoreType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            var currentProperty = settingsStoreType?.GetProperty("Current",
                BindingFlags.Static | BindingFlags.Public);
            var save = settingsStoreType?.GetMethod("Save",
                BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(settingsStoreType, "D16 无障碍设置存储未被 Unity 主程序集加载。");
            Assert.IsNotNull(currentProperty, "D16 无障碍设置存储缺少 Current。");
            Assert.IsNotNull(save, "D16 无障碍设置存储缺少 Save。");

            var originalSettings = currentProperty.GetValue(null);
            var originalClone = originalSettings.GetType().GetMethod("Clone")?.Invoke(originalSettings, null);
            var colorVisionMember = originalClone.GetType().GetField("ColorVisionMode",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(colorVisionMember, "D16 无障碍设置缺少 ColorVisionMode。");
            var fixture = new GameObject("D16_A03_LongTextAccessibilityFixture",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = fixture.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var textObject = new GameObject("LongText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(fixture.transform, false);
            var text = textObject.GetComponent<Text>();
            text.fontSize = 20;
            text.color = new Color(0.12f, 0.31f, 0.47f, 1f);
            text.text = "这是用于 D16-A03 的长文本回归内容：加勒比海港口、船员关系、敏感内容提示与可访问性设置必须在窄屏和放大字号下保持可读。 " +
                "Long text must wrap without silently losing the final line.";
            text.rectTransform.sizeDelta = new Vector2(360f, 180f);
            var originalColor = text.color;
            var originalFontSize = text.fontSize;
            var cjkFont = Resources.Load<Font>("Fonts/ph_font_noto_sans_cjk_sc");

            var report = new A03AccessibilityReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                CjkFontResourceLoaded = cjkFont != null,
                RuntimeAdapterFound = runtime != null,
                LongTextWrapped = false,
                LongTextOverflowConfigured = false,
                TextScaleApplied = false,
                HighContrastApplied = false,
                ColorVisionTransformApplied = false,
                LowStimulusApplied = false,
                Passed = false,
                FailureReason = string.Empty,
            };

            try
            {
                var testSettings = originalClone.GetType().GetMethod("Clone")?.Invoke(originalClone, null);
                WriteMember(testSettings, "TextScale", 2f);
                WriteMember(testSettings, "HighContrast", true);
                WriteMember(testSettings, "LowStimulus", false);
                WriteMember(testSettings, "ColorVisionMode",
                    Enum.ToObject(colorVisionMember.FieldType, 0));
                save.Invoke(null, new[] { testSettings });
                apply.Invoke(runtime, null);
                Canvas.ForceUpdateCanvases();

                report.LongTextWrapped = text.horizontalOverflow == HorizontalWrapMode.Wrap;
                report.LongTextOverflowConfigured = text.verticalOverflow == VerticalWrapMode.Overflow;
                report.TextScaleApplied = text.fontSize == originalFontSize * 2 &&
                    text.font != null && cjkFont != null && text.font == cjkFont;
                report.HighContrastApplied = Approximately(text.color, Color.white);

                var colorSettings = originalClone.GetType().GetMethod("Clone")?.Invoke(originalClone, null);
                WriteMember(colorSettings, "TextScale", 1f);
                WriteMember(colorSettings, "HighContrast", false);
                WriteMember(colorSettings, "LowStimulus", false);
                WriteMember(colorSettings, "ColorVisionMode",
                    Enum.ToObject(colorVisionMember.FieldType, 1));
                save.Invoke(null, new[] { colorSettings });
                apply.Invoke(runtime, null);
                report.ColorVisionTransformApplied = !Approximately(text.color, originalColor);

                var lowStimulusSettings = originalClone.GetType().GetMethod("Clone")?.Invoke(originalClone, null);
                WriteMember(lowStimulusSettings, "TextScale", 1f);
                WriteMember(lowStimulusSettings, "HighContrast", false);
                WriteMember(lowStimulusSettings, "LowStimulus", true);
                WriteMember(lowStimulusSettings, "ColorVisionMode",
                    Enum.ToObject(colorVisionMember.FieldType, 0));
                save.Invoke(null, new[] { lowStimulusSettings });
                apply.Invoke(runtime, null);
                report.LowStimulusApplied = !Approximately(text.color, originalColor);

                if (!report.CjkFontResourceLoaded)
                    report.FailureReason = "Resources/Fonts/ph_font_noto_sans_cjk_sc 未加载。";
                else if (!report.LongTextWrapped || !report.LongTextOverflowConfigured)
                    report.FailureReason = "长文本没有配置换行或溢出策略。";
                else if (!report.TextScaleApplied)
                    report.FailureReason = "文字缩放没有应用到真实 UGUI Text 或字体未加载。";
                else if (!report.HighContrastApplied)
                    report.FailureReason = "高对比度颜色没有应用。";
                else if (!report.ColorVisionTransformApplied)
                    report.FailureReason = "色觉辅助颜色变换没有应用。";
                else if (!report.LowStimulusApplied)
                    report.FailureReason = "低刺激颜色适配没有应用。";
                else report.Passed = true;

                WriteA03AccessibilityReport(report);
                Assert.IsTrue(report.Passed, report.FailureReason);
            }
            finally
            {
                save.Invoke(null, new[] { originalClone });
                UnityEngine.Object.Destroy(fixture);
            }
        }

        /// <summary>
        /// UGUI 文本从内置字体切换到 CJK 主字体时，必须根据实际文本触发字体回退，
        /// 不能只在启动时固定一个 Font。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A03UguiTextUsesCjkFallbackForDynamicText()
        {
            var cjkFont = Resources.Load<Font>("Fonts/ph_font_noto_sans_cjk_sc");
            var builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Assert.IsNotNull(cjkFont, "CJK 主字体资源缺失。");
            Assert.IsNotNull(builtinFont, "UGUI 内置兜底字体缺失。");

            var fixture = new GameObject("D16_A03_FontFallbackFixture");
            var text = fixture.AddComponent<Text>();
            text.font = builtinFont;
            text.text = "港口";
            var fallbackType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16FontFallbackText, Assembly-CSharp");
            Assert.IsNotNull(fallbackType, "D16FontFallbackText 未被 Unity 主程序集加载。");
            var fallback = fixture.AddComponent(fallbackType);
            var refresh = fallbackType.GetMethod("RefreshNow", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(refresh, "D16FontFallbackText 缺少 RefreshNow 入口。");
            refresh.Invoke(fallback, null);
            yield return null;

            Assert.AreEqual(cjkFont, text.font,
                "动态中文文本应从内置字体回退到 CJK 主字体，不能显示为缺字方框。");

            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        [Serializable]
        private sealed class RuntimeReport
        {
            public string Scene;
            public string UnityVersion;
            public string GraphicsDevice;
            public int FramesObserved;
            public float WaitSeconds;
            public bool SceneLoaded;
            public bool CameraPresent;
            public bool PlayerPresent;
            public bool ShipPresent;
            public bool PerformanceControllerPresent;
            public bool PerformanceProbePresent;
            public bool PerformanceProbeActive;
            public bool PerformanceProbeOwnedByController;
            public string PerformanceProbeHost;
            public bool RegistrationBudgetObserved;
            public bool MetricsSampled;
            public bool Passed;
            public string FailureReason;
            public D16RuntimePerformanceProbe.Report Metrics;
        }

        [UnityTest]
        public IEnumerator InitialHarborBootstrapsD16AndSamplesRuntime()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            float startedAt = Time.realtimeSinceStartup;
            int frames = 0;
            D16WorldPerformanceController controller = null;
            D16RuntimePerformanceProbe probe = null;

            while (Time.realtimeSinceStartup - startedAt < 15f)
            {
                frames++;
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                probe = FindActiveRuntimePerformanceProbe();
                bool ready = SceneManager.GetActiveScene().name == SceneName &&
                    Camera.main != null &&
                    GameObject.Find("D2_PlayerAvatar") != null &&
                    GameObject.Find("ph_ship_fishing") != null &&
                    controller != null &&
                    probe != null &&
                    controller.RegistrationCount >= 3 &&
                    probe.Snapshot != null && probe.Snapshot.SampleCount >= 3;
                if (ready) break;
                yield return null;
            }

            var activeScene = SceneManager.GetActiveScene();
            var metrics = probe == null ? null : probe.Snapshot;
            var report = new RuntimeReport
            {
                Scene = activeScene.name,
                UnityVersion = Application.unityVersion,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                FramesObserved = frames,
                WaitSeconds = Time.realtimeSinceStartup - startedAt,
                SceneLoaded = activeScene.name == SceneName,
                CameraPresent = Camera.main != null,
                PlayerPresent = GameObject.Find("D2_PlayerAvatar") != null,
                ShipPresent = GameObject.Find("ph_ship_fishing") != null,
                PerformanceControllerPresent = controller != null,
                PerformanceProbePresent = probe != null,
                PerformanceProbeActive = probe != null && probe.isActiveAndEnabled && probe.gameObject.activeInHierarchy,
                PerformanceProbeOwnedByController = controller != null && probe != null &&
                    probe.transform == controller.transform,
                PerformanceProbeHost = probe == null ? string.Empty : probe.gameObject.name,
                RegistrationBudgetObserved = controller != null && controller.RegistrationCount >= 3,
                MetricsSampled = metrics != null && metrics.SampleCount >= 3,
                Metrics = metrics,
                Passed = false,
                FailureReason = string.Empty,
            };

            if (!report.SceneLoaded) report.FailureReason = "InitialHarbor 未成为活动场景。";
            else if (!report.CameraPresent) report.FailureReason = "场景没有可用相机。";
            else if (!report.PlayerPresent || !report.ShipPresent) report.FailureReason = "D2 玩家或船运行时对象未创建。";
            else if (!report.PerformanceControllerPresent || !report.PerformanceProbePresent)
                report.FailureReason = "D16 性能控制器或采样器未创建。";
            else if (!report.PerformanceProbeOwnedByController)
                report.FailureReason = "D16 活动性能探针未挂在性能控制器上。";
            else if (!report.RegistrationBudgetObserved)
                report.FailureReason = "D16 默认表现登记数量不足。";
            else if (!report.MetricsSampled)
                report.FailureReason = "D16 性能探针未产生足够采样。";
            else report.Passed = true;

            WriteReport(report);
            Assert.IsTrue(report.Passed, report.FailureReason);
        }

        /// <summary>
        /// 稳定 ID 正确不能掩盖运行时类别/保护标志错误。
        /// 该回归先故意制造“ID 正确但玩家保护丢失”的状态，再让默认登记修复，
        /// 确认预算快照仍把玩家识别为受保护对象。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01DefaultRegistrationRepairsFlagsWhenStableIdAlreadyMatches()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16WorldPerformanceController controller = null;
            GameObject player = null;
            D16PresentationRegistration registration = null;
            float setupDeadline = Time.realtimeSinceStartup + 8f;
            while ((controller == null || player == null || registration == null) &&
                   Time.realtimeSinceStartup < setupDeadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                player = GameObject.Find("D2_PlayerAvatar");
                registration = player == null ? null : player.GetComponent<D16PresentationRegistration>();
                if (controller == null || player == null || registration == null)
                    yield return null;
            }

            Assert.IsNotNull(controller, "D16 性能控制器未在 InitialHarbor 创建。");
            Assert.IsNotNull(player, "D2 玩家未在 InitialHarbor 创建。");
            Assert.IsNotNull(registration, "D2 玩家缺少 D16 表现登记。");

            // 模拟一个预制体已经带有正确 stable ID，但序列化保护标志损坏的回归。
            registration.ConfigureRuntime("player.avatar", false, false, false, false, false);
            controller.ConfigureRuntimeFocus(player.transform, 0.05f);

            D16PerformanceDecision playerDecision = null;
            float decisionDeadline = Time.realtimeSinceStartup + 5f;
            while (playerDecision == null && Time.realtimeSinceStartup < decisionDeadline)
            {
                controller.RefreshRegistrations();
                var snapshot = controller.LastSnapshot;
                if (snapshot != null)
                {
                    foreach (var decision in snapshot.Decisions)
                    {
                        if (decision != null && decision.Id == "player.avatar")
                        {
                            playerDecision = decision;
                            break;
                        }
                    }
                }
                if (playerDecision == null) yield return null;
            }

            Assert.IsNotNull(playerDecision, "玩家未进入 D16 预算快照。");
            Assert.IsTrue(registration.MatchesRuntimeConfiguration("player.avatar", false, false, false, true, true),
                "默认登记没有修复 stable ID 相同但保护标志错误的玩家对象。");
            Assert.IsTrue(playerDecision.IsProtected, "玩家在预算快照中没有保持受保护标志。");
            Assert.AreEqual(D16SimulationTier.Full, playerDecision.SimulationTier,
                "玩家不应因错误的预制体标志被降级。");
        }

        [Serializable]
        private sealed class A01PerformanceWindowReport
        {
            public string Scene;
            public string UnityVersion;
            public string GraphicsDevice;
            public string GraphicsDeviceType;
            public string GraphicsDeviceVersion;
            public bool SupportsGpuRecorder;
            public bool FrameTimingFeatureEnabled;
            public float WindowSeconds;
            public int RequiredSampleCount;
            public int SampleCount;
            public float AverageFps;
            public float MinFps;
            public float AverageFrameMs;
            public float LongestFrameMs;
            public float LongestFrameAtSeconds;
            public int LongFrameCountOver50Ms;
            public float AverageMainThreadMs;
            public float GpuTimeMs;
            public bool GpuSampleAvailable;
            public string GpuCounterName;
            public float SceneLoadSeconds;
            public float RuntimeBootstrapReadySeconds;
            public float D5RuntimeReadySeconds;
            public bool PerformanceProbeActive;
            public bool PerformanceProbeOwnedByController;
            public string PerformanceProbeHost;
            public int ManagedGcGen0Collections;
            public int ManagedGcGen1Collections;
            public int ManagedGcGen2Collections;
            public float D16ControllerAwakeMs;
            public float D16FirstRegistrationRefreshMs;
            public float D16FirstUpdateMs;
            public float D16FirstBudgetEvaluationMs;
            public float D5AwakeMs;
            public float D5RuntimeBuildElapsedMs;
            public bool D5RuntimePhaseTimingsComplete;
            public int D5CompletedRuntimePhaseCount;
            public float D5WaitingForPlayerMs;
            public float D5LoadingSnapshotMs;
            public float D5CreatingSystemsMs;
            public float D5InitializingStateMs;
            public float D5ApplyingCharacterVisualMs;
            public float D5BuildingMapMs;
            public float D2RuntimeBuildElapsedMs;
            public bool D2RuntimePhaseTimingsComplete;
            public int D2CompletedRuntimePhaseCount;
            public float D2EnsureShipMs;
            public float D2EnsureCrewAnchorMs;
            public float D2EnsurePlayerMs;
            public float D2ConfigurePhysicsMs;
            public float D2ConfigureControlsMs;
            public float D2ConfigurePresentationMs;
            public float D3RuntimeReadySeconds;
            public float D4RuntimeReadySeconds;
            public float D9RuntimeReadySeconds;
            public long PeakAllocatedBytes;
            public long PeakReservedBytes;
            public long PeakManagedBytes;
            public int ActiveAgentCount;
            public int ActiveAnimalCount;
            public int AudibleSourceCount;
            public int LoadedCellCount;
            public int RegistrationCount;
            public int CellTransitionCount;
            public float LastCellTransitionMs;
            public float TotalCellTransitionMs;
            public int LoadingCellCount;
            public int LoadedPresentationObjectCount;
            public int PendingPresentationObjectCount;
            public float MaxPresentationLoadDurationMs;
            public int PresentationLoadCount;
            public int PoolRentCount;
            public int PoolReuseCount;
            public int PoolReturnCount;
            public int PoolPooledCount;
            public int CatalogIslandCellCount;
            public int ActiveIslandDataCellCount;
            public int DataOnlyIslandCellCount;
            public int VisualPresentationIslandCellCount;
            public string ReferenceIslandId;
            public bool BudgetSatisfied;
            public int ProtectedOverBudgetCount;
            public bool Passed;
            public string FailureReason;
        }

        /// <summary>
        /// 在真实播放循环中保持一段连续采样，验证探针和预算不会只在首帧短暂成立。
        /// 该测试不宣称目标设备 FPS/GPU 性能通过；目标设备门禁仍需独立基准。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01RuntimePerformanceWindowSamplesContinuously()
        {
            float sceneLoadStartedAt = Time.realtimeSinceStartup;
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;
            float sceneLoadSeconds = Time.realtimeSinceStartup - sceneLoadStartedAt;

            D16WorldPerformanceController controller = null;
            D16RuntimePerformanceProbe probe = null;
            Type d5Type = Type.GetType("Whalebone.Gameplay.D5.D5RuntimeBootstrap, Assembly-CSharp");
            object d5 = null;
            float runtimeBootstrapStartedAt = Time.realtimeSinceStartup;
            float readyDeadline = Time.realtimeSinceStartup + 8f;
            while ((controller == null || probe == null || d5 == null) && Time.realtimeSinceStartup < readyDeadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                probe = FindActiveRuntimePerformanceProbe();
                d5 = FindComponent(d5Type);
                if (controller == null || probe == null) yield return null;
            }
            float runtimeBootstrapReadySeconds = Time.realtimeSinceStartup - runtimeBootstrapStartedAt;
            float d5RuntimeReadySeconds = -1f;
            if (ReadMember(d5, "IsReady") is bool d5Ready && d5Ready)
            {
                d5RuntimeReadySeconds = Time.realtimeSinceStartup - runtimeBootstrapStartedAt;
            }
            else
            {
                float d5ReadyDeadline = Time.realtimeSinceStartup + 8f;
                while (d5 != null && !(ReadMember(d5, "IsReady") is bool ready && ready) &&
                       Time.realtimeSinceStartup < d5ReadyDeadline)
                    yield return null;
                if (ReadMember(d5, "IsReady") is bool d5ReadyAfterWait && d5ReadyAfterWait)
                    d5RuntimeReadySeconds = Time.realtimeSinceStartup - runtimeBootstrapStartedAt;
            }

            // 60 秒连续窗口约覆盖 240 个 0.25 秒性能探针周期，能暴露短时
            // 采样看不出的流送、对象池和内存压力问题。它仍然只是本机回归，
            // 不替代目标低端设备上的独立基准记录。
            const float windowSeconds = 60f;
            const int requiredSamples = 100;
            float startedAt = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startedAt < windowSeconds) yield return null;

            var metrics = probe == null ? null : probe.Snapshot;
            var report = new A01PerformanceWindowReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                GraphicsDeviceType = metrics == null ? SystemInfo.graphicsDeviceType.ToString() : metrics.GraphicsDeviceType,
                GraphicsDeviceVersion = metrics == null ? SystemInfo.graphicsDeviceVersion : metrics.GraphicsDeviceVersion,
                SupportsGpuRecorder = metrics != null && metrics.SupportsGpuRecorder,
                FrameTimingFeatureEnabled = metrics != null && metrics.FrameTimingFeatureEnabled,
                WindowSeconds = Time.realtimeSinceStartup - startedAt,
                RequiredSampleCount = requiredSamples,
                SampleCount = metrics == null ? 0 : metrics.SampleCount,
                AverageFps = metrics == null ? 0f : metrics.AverageFps,
                MinFps = metrics == null ? 0f : metrics.MinFps,
                AverageFrameMs = metrics == null ? 0f : metrics.AverageFrameMs,
                LongestFrameMs = metrics == null ? 0f : metrics.LongestFrameMs,
                LongestFrameAtSeconds = metrics == null ? 0f : metrics.LongestFrameAtSeconds,
                LongFrameCountOver50Ms = metrics == null ? 0 : metrics.LongFrameCountOver50Ms,
                AverageMainThreadMs = metrics == null ? -1f : metrics.AverageMainThreadMs,
                GpuTimeMs = metrics == null ? -1f : metrics.GpuTimeMs,
                GpuSampleAvailable = metrics != null && metrics.GpuSampleAvailable,
                GpuCounterName = metrics == null ? string.Empty : metrics.GpuCounterName,
                SceneLoadSeconds = sceneLoadSeconds,
                RuntimeBootstrapReadySeconds = runtimeBootstrapReadySeconds,
                D5RuntimeReadySeconds = d5RuntimeReadySeconds,
                PerformanceProbeActive = probe != null && probe.isActiveAndEnabled && probe.gameObject.activeInHierarchy,
                PerformanceProbeOwnedByController = controller != null && probe != null &&
                    probe.transform == controller.transform,
                PerformanceProbeHost = probe == null ? string.Empty : probe.gameObject.name,
                ManagedGcGen0Collections = metrics == null ? -1 : metrics.ManagedGcGen0Collections,
                ManagedGcGen1Collections = metrics == null ? -1 : metrics.ManagedGcGen1Collections,
                ManagedGcGen2Collections = metrics == null ? -1 : metrics.ManagedGcGen2Collections,
                D16ControllerAwakeMs = metrics == null ? -1f : metrics.D16ControllerAwakeMs,
                D16FirstRegistrationRefreshMs = metrics == null ? -1f : metrics.D16FirstRegistrationRefreshMs,
                D16FirstUpdateMs = metrics == null ? -1f : metrics.D16FirstUpdateMs,
                D16FirstBudgetEvaluationMs = metrics == null ? -1f : metrics.D16FirstBudgetEvaluationMs,
                D5AwakeMs = metrics == null ? -1f : metrics.D5AwakeMs,
                D5RuntimeBuildElapsedMs = metrics == null ? -1f : metrics.D5RuntimeBuildElapsedMs,
                D5RuntimePhaseTimingsComplete = metrics != null && metrics.D5RuntimePhaseTimingsComplete,
                D5CompletedRuntimePhaseCount = metrics == null ? -1 : metrics.D5CompletedRuntimePhaseCount,
                D5WaitingForPlayerMs = metrics == null ? -1f : metrics.D5WaitingForPlayerMs,
                D5LoadingSnapshotMs = metrics == null ? -1f : metrics.D5LoadingSnapshotMs,
                D5CreatingSystemsMs = metrics == null ? -1f : metrics.D5CreatingSystemsMs,
                D5InitializingStateMs = metrics == null ? -1f : metrics.D5InitializingStateMs,
                D5ApplyingCharacterVisualMs = metrics == null ? -1f : metrics.D5ApplyingCharacterVisualMs,
                D5BuildingMapMs = metrics == null ? -1f : metrics.D5BuildingMapMs,
                D2RuntimeBuildElapsedMs = metrics == null ? -1f : metrics.D2RuntimeBuildElapsedMs,
                D2RuntimePhaseTimingsComplete = metrics != null && metrics.D2RuntimePhaseTimingsComplete,
                D2CompletedRuntimePhaseCount = metrics == null ? -1 : metrics.D2CompletedRuntimePhaseCount,
                D2EnsureShipMs = metrics == null ? -1f : metrics.D2EnsureShipMs,
                D2EnsureCrewAnchorMs = metrics == null ? -1f : metrics.D2EnsureCrewAnchorMs,
                D2EnsurePlayerMs = metrics == null ? -1f : metrics.D2EnsurePlayerMs,
                D2ConfigurePhysicsMs = metrics == null ? -1f : metrics.D2ConfigurePhysicsMs,
                D2ConfigureControlsMs = metrics == null ? -1f : metrics.D2ConfigureControlsMs,
                D2ConfigurePresentationMs = metrics == null ? -1f : metrics.D2ConfigurePresentationMs,
                D3RuntimeReadySeconds = metrics == null ? -1f : metrics.D3RuntimeReadySeconds,
                D4RuntimeReadySeconds = metrics == null ? -1f : metrics.D4RuntimeReadySeconds,
                D9RuntimeReadySeconds = metrics == null ? -1f : metrics.D9RuntimeReadySeconds,
                PeakAllocatedBytes = metrics == null ? 0L : metrics.PeakAllocatedBytes,
                PeakReservedBytes = metrics == null ? 0L : metrics.PeakReservedBytes,
                PeakManagedBytes = metrics == null ? 0L : metrics.PeakManagedBytes,
                ActiveAgentCount = metrics == null ? -1 : metrics.ActiveAgentCount,
                ActiveAnimalCount = metrics == null ? -1 : metrics.ActiveAnimalCount,
                AudibleSourceCount = metrics == null ? -1 : metrics.AudibleSourceCount,
                LoadedCellCount = metrics == null ? -1 : metrics.LoadedCellCount,
                RegistrationCount = metrics == null ? -1 : metrics.RegistrationCount,
                CellTransitionCount = metrics == null ? -1 : metrics.CellTransitionCount,
                LastCellTransitionMs = metrics == null ? -1f : metrics.LastCellTransitionMs,
                TotalCellTransitionMs = metrics == null ? -1f : metrics.TotalCellTransitionMs,
                LoadingCellCount = metrics == null ? -1 : metrics.LoadingCellCount,
                LoadedPresentationObjectCount = metrics == null ? -1 : metrics.LoadedPresentationObjectCount,
                PendingPresentationObjectCount = metrics == null ? -1 : metrics.PendingPresentationObjectCount,
                MaxPresentationLoadDurationMs = metrics == null ? -1f : metrics.MaxPresentationLoadDurationMs,
                PresentationLoadCount = metrics == null ? -1 : metrics.PresentationLoadCount,
                PoolRentCount = metrics == null ? -1 : metrics.PoolRentCount,
                PoolReuseCount = metrics == null ? -1 : metrics.PoolReuseCount,
                PoolReturnCount = metrics == null ? -1 : metrics.PoolReturnCount,
                PoolPooledCount = metrics == null ? -1 : metrics.PoolPooledCount,
                CatalogIslandCellCount = metrics == null ? -1 : metrics.CatalogIslandCellCount,
                ActiveIslandDataCellCount = metrics == null ? -1 : metrics.ActiveIslandDataCellCount,
                DataOnlyIslandCellCount = metrics == null ? -1 : metrics.DataOnlyIslandCellCount,
                VisualPresentationIslandCellCount = metrics == null ? -1 : metrics.VisualPresentationIslandCellCount,
                ReferenceIslandId = metrics == null ? string.Empty : metrics.ReferenceIslandId,
                BudgetSatisfied = metrics != null && metrics.BudgetSatisfied,
                ProtectedOverBudgetCount = metrics == null ? -1 : metrics.ProtectedOverBudgetCount,
                Passed = false,
                FailureReason = string.Empty,
            };

            if (SceneManager.GetActiveScene().name != SceneName)
                report.FailureReason = "持续采样期间活动场景发生变化。";
            else if (controller == null || probe == null)
                report.FailureReason = "持续采样期间 D16 控制器或探针未创建。";
            else if (!report.PerformanceProbeActive || !report.PerformanceProbeOwnedByController)
                report.FailureReason = "持续采样期间活动性能探针没有保持在 D16 控制器上。";
            else if (report.D5RuntimeReadySeconds < 0f)
                report.FailureReason = "持续采样期间 D5 运行时未完成接线。";
            else if (report.SampleCount < requiredSamples)
                report.FailureReason = "持续采样数量不足，不能证明探针连续工作。";
            else if (string.IsNullOrWhiteSpace(report.GraphicsDeviceType) ||
                     string.IsNullOrWhiteSpace(report.GraphicsDeviceVersion))
                report.FailureReason = "图形设备能力元数据不完整，不能解释 GPU 计时是否可用。";
            else if (!report.D5RuntimePhaseTimingsComplete || report.D5CompletedRuntimePhaseCount < 6)
                report.FailureReason = "D5 启动阶段计时未完整落盘，不能进行冷启动函数级归因。";
            else if (report.D5RuntimeBuildElapsedMs < 0f || report.D5AwakeMs < 0f)
                report.FailureReason = "D5 启动阶段计时包含无效负值。";
            else if (!report.D2RuntimePhaseTimingsComplete || report.D2CompletedRuntimePhaseCount < 6)
                report.FailureReason = "D2 启动阶段计时未完整落盘，不能进行冷启动函数级归因。";
            else if (report.D2RuntimeBuildElapsedMs < 0f)
                report.FailureReason = "D2 启动阶段计时包含无效负值。";
            else if (report.D3RuntimeReadySeconds < 0f || report.D4RuntimeReadySeconds < 0f ||
                     report.D9RuntimeReadySeconds < 0f)
                report.FailureReason = "D3/D4/D9 运行时就绪时间未完整落盘，不能定位启动链归因。";
            else if (report.GpuSampleAvailable && report.GpuTimeMs <= 0f)
                report.FailureReason = "GPU 采样标记为可用但没有正的 GPU 帧时间，拒绝伪造 GPU 指标。";
            else if (!report.BudgetSatisfied || report.ProtectedOverBudgetCount != 0)
                report.FailureReason = "持续采样期间表现预算未满足或保护对象超预算。";
            else report.Passed = true;

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A01PerformanceWindowReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A01] PASS " : "[D16/A01] FAIL ") + path);
            if (!report.Passed) Debug.LogError("[D16/A01] " + report.FailureReason);
            Assert.IsTrue(report.Passed, report.FailureReason);
        }

        [UnityTest]
        public IEnumerator D16A03AccessibilityPanelOpensAndClosesFromMainMenu()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Button settingsButton = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (settingsButton == null && Time.realtimeSinceStartup < deadline)
            {
                var settingsObject = GameObject.Find("BtnSettings");
                settingsButton = settingsObject == null ? null : settingsObject.GetComponent<Button>();
                if (settingsButton == null) yield return null;
            }
            Assert.IsNotNull(settingsButton, "主菜单设置按钮未创建。");
            settingsButton.onClick.Invoke();
            yield return null;

            var accessibilityButtonObject = GameObject.Find("BtnAccessibility");
            var accessibilityButton = accessibilityButtonObject == null
                ? null
                : accessibilityButtonObject.GetComponent<Button>();
            Assert.IsNotNull(accessibilityButton, "设置面板没有无障碍入口。");
            accessibilityButton.onClick.Invoke();
            yield return null;

            var host = GameObject.Find("D16AccessibilityPanel");
            Assert.IsNotNull(host, "D16AccessibilityPanel 未创建。");
            var modal = FindChild(host.transform, "AccessibilityModal");
            Assert.IsNotNull(modal, "无障碍模态面板节点未创建。");
            Assert.IsTrue(modal.gameObject.activeSelf, "无障碍面板打开后仍是隐藏状态。");

            var close = FindChild(host.transform, "Close");
            Assert.IsNotNull(close, "无障碍面板关闭按钮未创建。");
            var closeButton = close.GetComponent<Button>();
            Assert.IsNotNull(closeButton, "无障碍面板关闭节点不是 Button。");
            closeButton.onClick.Invoke();
            yield return null;
            Assert.IsFalse(modal.gameObject.activeSelf, "无障碍面板关闭后仍拦截界面。");
        }

        [UnityTest]
        public IEnumerator D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var settingsButton = GameObject.Find("BtnSettings")?.GetComponent<Button>();
            Assert.IsNotNull(settingsButton, "主菜单设置按钮未创建。");
            settingsButton.onClick.Invoke();
            yield return null;

            var accessibilityButton = GameObject.Find("BtnAccessibility")?.GetComponent<Button>();
            Assert.IsNotNull(accessibilityButton, "设置面板没有无障碍入口。");
            accessibilityButton.onClick.Invoke();
            yield return null;

            var host = GameObject.Find("D16AccessibilityPanel");
            Assert.IsNotNull(host, "D16AccessibilityPanel 未创建。");
            var scrollObject = FindChild(host.transform, "BindingScroll");
            Assert.IsNotNull(scrollObject, "核心操作重绑定缺少滚动视口。");
            var scroll = scrollObject.GetComponent<ScrollRect>();
            Assert.IsNotNull(scroll, "核心操作重绑定滚动节点不是 ScrollRect。");
            Assert.IsNotNull(scroll.viewport, "重绑定滚动视口未配置。");
            Assert.IsNotNull(scroll.content, "重绑定滚动内容未配置。");
            var bindings = D16AccessibilityDefaults.CreateBindings();
            Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height,
                "全部核心与上下文动作必须在可滚动内容中，不能挤压底部按钮。");
            Assert.Greater(bindings.Count, 16,
                "无障碍面板应展示核心动作之外的 D3-D10 上下文动作。");

            foreach (var binding in bindings)
            {
                var action = binding.Action.ToString();
                var keyboard = scroll.content.Find("Keyboard_" + action);
                var gamepad = scroll.content.Find("Gamepad_" + action);
                Assert.IsNotNull(keyboard, "缺少键盘重绑定按钮：" + action);
                Assert.IsNotNull(gamepad, "缺少手柄重绑定按钮：" + action);
                Assert.IsTrue(keyboard.GetComponent<Button>().interactable,
                    "键盘重绑定按钮不可交互：" + action);
                Assert.IsTrue(gamepad.GetComponent<Button>().interactable,
                    "手柄重绑定按钮不可交互：" + action);
            }

            scroll.content.Find("Keyboard_Map").GetComponent<Button>().onClick.Invoke();
            yield return null;
            var status = FindChild(host.transform, "Status")?.GetComponent<Text>();
            Assert.IsNotNull(status, "重绑定状态提示未创建。");
            Assert.IsFalse(string.IsNullOrWhiteSpace(status.text), "新增动作的重绑定按钮没有进入监听状态。");

            FindChild(host.transform, "Close").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsFalse(FindChild(host.transform, "AccessibilityModal").gameObject.activeSelf,
                "重绑定面板关闭后仍拦截界面。");
        }

        [UnityTest]
        public IEnumerator D16A03AccessibilityPanelRestoresOuterInputBlockState()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var panelType = Type.GetType("Whalebone.Gameplay.UI.D16.D16AccessibilityPanel, Assembly-CSharp");
            var routerType = Type.GetType("Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");
            var coordinatorType = Type.GetType("Whalebone.Gameplay.D7.D7ControlCoordinator, Assembly-CSharp");
            Assert.IsNotNull(panelType, "D16AccessibilityPanel 未被 Unity 主程序集加载。");
            Assert.IsNotNull(routerType, "D16InputRouter 未被 Unity 主程序集加载。");
            Assert.IsNotNull(coordinatorType, "D7ControlCoordinator 未被 Unity 主程序集加载。");

            var setBlocked = routerType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var show = panelType.GetMethod("ShowPanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var hide = panelType.GetMethod("HidePanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var isOpen = panelType.GetProperty("IsOpen",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var gameplayAllowed = coordinatorType.GetProperty("GameplayInputAllowed",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入阻断入口。");
            Assert.IsNotNull(show, "无障碍面板缺少 ShowPanel 入口。");
            Assert.IsNotNull(hide, "无障碍面板缺少 HidePanel 入口。");
            Assert.IsNotNull(isOpen, "无障碍面板缺少 IsOpen 状态。");
            Assert.IsNotNull(gameplayAllowed, "D7 控制协调器缺少统一玩法输入权限。");

            try
            {
                setBlocked.Invoke(null, new object[] { true });
                show.Invoke(null, null);
                yield return null;
                Assert.IsTrue((bool)isOpen.GetValue(null), "无障碍面板没有打开。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "无障碍面板打开后没有保持输入阻断。");
                Assert.IsFalse((bool)gameplayAllowed.GetValue(null),
                    "D16 模态打开后旧 D7 输入权限仍为可用，存在快捷键穿透风险。");

                hide.Invoke(null, null);
                yield return null;
                Assert.IsFalse((bool)isOpen.GetValue(null), "无障碍面板没有关闭。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "关闭内层无障碍面板时错误地解除了外层输入阻断。");
                Assert.IsFalse((bool)gameplayAllowed.GetValue(null),
                    "外层输入阻断仍在时，D7 玩法输入权限被错误恢复。");
            }
            finally
            {
                setBlocked.Invoke(null, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator D16A03SettingsButtonsHaveUsableHitTargetsAndOpenRealModals()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Button settingsButton = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (settingsButton == null && Time.realtimeSinceStartup < deadline)
            {
                settingsButton = GameObject.Find("BtnSettings")?.GetComponent<Button>();
                if (settingsButton == null) yield return null;
            }

            Assert.IsNotNull(settingsButton, "主菜单设置按钮未创建。");
            settingsButton.onClick.Invoke();
            yield return null;

            var panel = GameObject.Find("SettingsPanel");
            Assert.IsNotNull(panel, "设置面板未创建。");
            Assert.IsTrue(panel.activeSelf, "设置面板打开后仍是隐藏状态。");

            var buttonNames = new[] { "BtnAccessibility", "BtnDiagnostics", "BtnClose" };
            var buttons = new Button[buttonNames.Length];
            for (int i = 0; i < buttonNames.Length; i++)
            {
                var buttonObject = panel.transform.Find(buttonNames[i]);
                Assert.IsNotNull(buttonObject, "设置面板缺少按钮：" + buttonNames[i]);
                buttons[i] = buttonObject.GetComponent<Button>();
                Assert.IsNotNull(buttons[i], buttonNames[i] + " 不是 Button。");
                Assert.IsTrue(buttons[i].interactable, buttonNames[i] + " 不可交互。");

                var rect = buttons[i].GetComponent<RectTransform>();
                Assert.IsNotNull(rect, buttonNames[i] + " 缺少 RectTransform。");
                float halfHeight = rect.rect.height * 0.5f;
                Assert.GreaterOrEqual(rect.anchoredPosition.y - halfHeight, -520f,
                    buttonNames[i] + " 的点击区域超出参考视口底部。");
                Assert.LessOrEqual(rect.anchoredPosition.y + halfHeight, 520f,
                    buttonNames[i] + " 的点击区域超出参考视口顶部。");
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                var left = buttons[i].GetComponent<RectTransform>();
                for (int j = i + 1; j < buttons.Length; j++)
                {
                    var right = buttons[j].GetComponent<RectTransform>();
                    float minimumGap = (left.rect.height + right.rect.height) * 0.5f + 4f;
                    Assert.GreaterOrEqual(Mathf.Abs(left.anchoredPosition.y - right.anchoredPosition.y), minimumGap,
                        buttonNames[i] + " 与 " + buttonNames[j] + " 的点击区域重叠。");
                }
            }

            buttons[0].onClick.Invoke();
            yield return null;
            var accessibilityHost = GameObject.Find("D16AccessibilityPanel");
            Assert.IsNotNull(accessibilityHost, "无障碍面板宿主未创建。");
            var accessibilityModal = FindChild(accessibilityHost.transform, "AccessibilityModal");
            Assert.IsNotNull(accessibilityModal, "无障碍模态未创建。");
            Assert.IsTrue(accessibilityModal.gameObject.activeSelf, "无障碍按钮点击后模态未打开。");
            FindChild(accessibilityHost.transform, "Close").GetComponent<Button>().onClick.Invoke();
            yield return null;

            buttons[1].onClick.Invoke();
            yield return null;
            var diagnosticsHost = GameObject.Find("D16LocalDiagnosticsPanel");
            Assert.IsNotNull(diagnosticsHost, "本地诊断面板宿主未创建。");
            var diagnosticsModal = FindChild(diagnosticsHost.transform, "DiagnosticsModal");
            Assert.IsNotNull(diagnosticsModal, "本地诊断模态未创建。");
            Assert.IsTrue(diagnosticsModal.gameObject.activeSelf, "诊断按钮点击后模态未打开。");
            FindChild(diagnosticsHost.transform, "Close").GetComponent<Button>().onClick.Invoke();
            yield return null;

            buttons[2].onClick.Invoke();
            yield return null;
            Assert.IsFalse(panel.activeSelf, "设置面板返回按钮点击后仍显示。");
            Assert.IsTrue(GameObject.Find("MainMenu").activeSelf, "返回后主菜单没有恢复显示。");
        }

        [UnityTest]
        public IEnumerator D16A03AccessibilitySavePreservesLiveMainMenuLanguage()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var settingsStoreType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            Assert.IsNotNull(settingsStoreType, "D16AccessibilitySettingsStore 未被 Unity 主程序集加载。");

            const string languageKey = "wb_lang";
            string originalLanguage = PlayerPrefs.GetString(languageKey, "zh");
            object originalSettings = ReadMember(null, settingsStoreType, "Current");
            object originalSettingsClone = originalSettings?.GetType().GetMethod("Clone",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(originalSettings, null);
            var reset = settingsStoreType.GetMethod("Reset",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var current = settingsStoreType.GetProperty("Current",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var save = settingsStoreType.GetMethod("Save",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(reset, "D16AccessibilitySettingsStore 缺少 Reset 入口。");
            Assert.IsNotNull(current, "D16AccessibilitySettingsStore 缺少 Current 入口。");
            Assert.IsNotNull(save, "D16AccessibilitySettingsStore 缺少 Save 入口。");

            try
            {
                PlayerPrefs.SetString(languageKey, "zh");
                PlayerPrefs.Save();
                reset.Invoke(null, null);

                var staleSettings = current.GetValue(null, null);
                staleSettings = staleSettings.GetType().GetMethod("Clone",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(staleSettings, null);
                Assert.AreEqual("zh-CN", ReadMember(staleSettings, "Locale"),
                    "测试夹具没有先建立中文旧设置快照。");

                // 模拟用户在主菜单刚切换为英文，但没有重新打开无障碍面板。
                PlayerPrefs.SetString(languageKey, "en");
                PlayerPrefs.Save();
                WriteMember(staleSettings, "HighContrast", true);
                save.Invoke(null, new[] { staleSettings });

                Assert.AreEqual("en", PlayerPrefs.GetString(languageKey),
                    "保存无障碍选项不应覆盖主菜单刚选择的语言。");
                var persisted = current.GetValue(null, null);
                Assert.AreEqual("en-US", ReadMember(persisted, "Locale"),
                    "无障碍设置缓存没有跟随实时语言同步。");
            }
            finally
            {
                PlayerPrefs.SetString(languageKey, originalLanguage == "en" ? "en" : "zh");
                PlayerPrefs.Save();
                if (originalSettingsClone != null)
                    save.Invoke(null, new[] { originalSettingsClone });
            }
        }

        [UnityTest]
        public IEnumerator D16A03UserModalsAreExclusiveAndRestoreOuterInputBlock()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var accessibilityType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilityPanel, Assembly-CSharp");
            var diagnosticsType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16LocalDiagnosticsPanel, Assembly-CSharp");
            var financeType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16FinanceDebtPanel, Assembly-CSharp");
            var routerType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");
            Assert.IsNotNull(accessibilityType, "无障碍面板类型未加载。");
            Assert.IsNotNull(diagnosticsType, "本地诊断面板类型未加载。");
            Assert.IsNotNull(financeType, "金融面板类型未加载。");
            Assert.IsNotNull(routerType, "D16InputRouter 类型未加载。");

            var showAccessibility = accessibilityType.GetMethod("ShowPanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var hideAccessibility = accessibilityType.GetMethod("HidePanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var showDiagnostics = diagnosticsType.GetMethod("ShowPanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var hideDiagnostics = diagnosticsType.GetMethod("HidePanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var showFinance = financeType.GetMethod("ShowPanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var hideFinance = financeType.GetMethod("HidePanel",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var setBlocked = routerType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(showAccessibility, "无障碍面板缺少 ShowPanel。");
            Assert.IsNotNull(hideAccessibility, "无障碍面板缺少 HidePanel。");
            Assert.IsNotNull(showDiagnostics, "诊断面板缺少 ShowPanel。");
            Assert.IsNotNull(hideDiagnostics, "诊断面板缺少 HidePanel。");
            Assert.IsNotNull(showFinance, "金融面板缺少 ShowPanel。");
            Assert.IsNotNull(hideFinance, "金融面板缺少 HidePanel。");
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入锁入口。");

            try
            {
                // 模拟暂停菜单或上层模态已经持有输入锁。
                setBlocked.Invoke(null, new object[] { true });
                showAccessibility.Invoke(null, null);
                yield return null;
                Assert.IsTrue(GameObject.Find("AccessibilityModal")?.activeSelf == true,
                    "无障碍模态没有打开。");

                showDiagnostics.Invoke(null, null);
                yield return null;
                Assert.IsFalse(GameObject.Find("AccessibilityModal")?.activeSelf == true,
                    "打开诊断模态后无障碍模态仍在拦截点击。");
                Assert.IsTrue(GameObject.Find("DiagnosticsModal")?.activeSelf == true,
                    "诊断模态没有接管显示层。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "切换到诊断模态时错误解除外层输入锁。");

                showFinance.Invoke(null, null);
                yield return null;
                Assert.IsFalse(GameObject.Find("DiagnosticsModal")?.activeSelf == true,
                    "打开金融模态后诊断模态仍在拦截点击。");
                Assert.IsTrue(GameObject.Find("FinanceDebtModal")?.activeSelf == true,
                    "金融模态没有接管显示层。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "切换到金融模态时错误解除外层输入锁。");

                hideFinance.Invoke(null, null);
                yield return null;
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "关闭内层金融模态时错误解除外层输入锁。");
            }
            finally
            {
                hideAccessibility.Invoke(null, null);
                hideDiagnostics.Invoke(null, null);
                hideFinance.Invoke(null, null);
                setBlocked.Invoke(null, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator D16A03SensitiveOverlayRestoresOuterInputBlockOnDestroy()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var overlayType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16SensitiveContentOverlay, Assembly-CSharp");
            var routerType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");
            var settingsStoreType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            Assert.IsNotNull(overlayType, "敏感内容警告层类型未加载。");
            Assert.IsNotNull(routerType, "D16InputRouter 类型未加载。");
            Assert.IsNotNull(settingsStoreType, "D16AccessibilitySettingsStore 类型未加载。");

            var setBlocked = routerType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var present = overlayType.GetMethod("PresentWarning",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var hide = overlayType.GetMethod("Hide",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var ensure = overlayType.GetMethod("EnsureInstance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var isOpen = overlayType.GetProperty("IsOpen",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(setBlocked, "D16InputRouter 缺少输入锁入口。");
            Assert.IsNotNull(present, "敏感内容警告层缺少 PresentWarning 入口。");
            Assert.IsNotNull(hide, "敏感内容警告层缺少 Hide 入口。");
            Assert.IsNotNull(ensure, "敏感内容警告层缺少实例恢复入口。");
            Assert.IsNotNull(isOpen, "敏感内容警告层缺少 IsOpen 状态。");

            object originalSettings = ReadMember(null, settingsStoreType, "Current");
            try
            {
                if (originalSettings != null)
                {
                    var testSettings = originalSettings.GetType().GetMethod("Clone",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.Invoke(originalSettings, null);
                    WriteMember(testSettings, "SensitiveWarnings", true);
                    InvokeStatic(settingsStoreType, "Save", testSettings);
                }

                setBlocked.Invoke(null, new object[] { true });
                present.Invoke(null, new object[]
                {
                    "D16 敏感内容警告层销毁恢复测试。",
                    true,
                    new Action(() => { }),
                    new Action(() => { })
                });
                yield return null;
                Assert.IsTrue((bool)isOpen.GetValue(null), "敏感内容警告层没有打开。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "敏感内容警告层打开后没有保持外层输入锁。");

                var overlay = ReadMember(null, overlayType, "Instance") as Component;
                Assert.IsNotNull(overlay, "敏感内容警告层实例未创建。");
                UnityEngine.Object.Destroy(overlay.gameObject);
                yield return null;

                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "敏感内容警告层销毁时错误解除外层输入锁。");
                var recreated = ensure.Invoke(null, null) as Component;
                Assert.IsNotNull(recreated, "敏感内容警告层销毁后无法恢复实例。");
                yield return null;
            }
            finally
            {
                hide.Invoke(null, null);
                if (originalSettings != null)
                    InvokeStatic(settingsStoreType, "Save", originalSettings);
                setBlocked.Invoke(null, new object[] { false });
            }
        }

        [UnityTest]
        public IEnumerator D16A03MainMenuPanelVisibilityIsExclusiveAcrossNavigation()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var canvas = GameObject.Find("MainMenuCanvas");
            Assert.IsNotNull(canvas, "主菜单 Canvas 未创建。");

            var mainMenu = canvas.transform.Find("MainMenu");
            var newGame = canvas.transform.Find("NewGamePanel");
            var settings = canvas.transform.Find("SettingsPanel");
            var about = canvas.transform.Find("AboutPanel");
            Assert.IsNotNull(mainMenu, "MainMenu 面板节点未创建。");
            Assert.IsNotNull(newGame, "NewGamePanel 面板节点未创建。");
            Assert.IsNotNull(settings, "SettingsPanel 面板节点未创建。");
            Assert.IsNotNull(about, "AboutPanel 面板节点未创建。");

            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, mainMenu);

            mainMenu.Find("BtnSettings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, settings);

            settings.Find("BtnClose").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, mainMenu);

            mainMenu.Find("BtnNewGame").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, newGame);

            newGame.Find("BtnCancel").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, mainMenu);

            mainMenu.Find("BtnAbout").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, about);

            about.Find("BtnClose").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertOnlyMainMenuPanel(mainMenu, newGame, settings, about, mainMenu);
        }

        [UnityTest]
        public IEnumerator D16A03VisibleModalKeepsKeyboardAndGamepadFocusInsideItsScope()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var canvas = GameObject.Find("MainMenuCanvas");
            Assert.IsNotNull(canvas, "主菜单 Canvas 未创建。");
            var mainMenu = canvas.transform.Find("MainMenu");
            var settings = canvas.transform.Find("SettingsPanel");
            Assert.IsNotNull(mainMenu, "MainMenu 面板节点未创建。");
            Assert.IsNotNull(settings, "SettingsPanel 面板节点未创建。");

            yield return null;
            AssertScopeTargetsStayInside(mainMenu, "主菜单");
            AssertSelectedInside(mainMenu, "主菜单首次焦点");
            Assert.AreSame(mainMenu.Find("BtnNewGame")?.gameObject,
                EventSystem.current.currentSelectedGameObject,
                "主菜单首次提交必须稳定落到新游戏，不能因残留鼠标选择打开设置等其他页面。");

            mainMenu.Find("BtnSettings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            AssertScopeTargetsStayInside(settings, "设置面板");
            AssertSelectedInside(settings, "设置面板打开后的焦点");

            var accessibilityButton = settings.Find("BtnAccessibility")?.GetComponent<Button>();
            Assert.IsNotNull(accessibilityButton, "设置面板无障碍按钮未创建。");
            accessibilityButton.onClick.Invoke();
            yield return null;

            var accessibilityHost = GameObject.Find("D16AccessibilityPanel");
            Assert.IsNotNull(accessibilityHost, "无障碍面板宿主未创建。");
            var modal = FindChild(accessibilityHost.transform, "AccessibilityModal");
            Assert.IsNotNull(modal, "无障碍模态未创建。");
            Assert.IsTrue(modal.gameObject.activeSelf, "无障碍模态没有显示。");
            AssertScopeTargetsStayInside(modal, "无障碍模态");
            AssertSelectedInside(modal, "无障碍模态打开后的焦点");

            var selected = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            Assert.IsFalse(selected != null && selected.transform.IsChildOf(settings),
                "打开无障碍模态后焦点仍落在下层设置面板，确认键会被错误路由。");

            FindChild(modal, "Close").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.IsFalse(modal.gameObject.activeSelf, "关闭无障碍模态后仍然显示。");
            AssertSelectedInside(settings, "无障碍模态关闭后的焦点");
        }

        [UnityTest]
        public IEnumerator D16A06MainMenuBatchPlayerDoesNotInstallHarborSmokeProbe()
        {
            var load = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            Assert.IsNotNull(load, "MainMenu 不在 Build Settings 中。");
            while (!load.isDone) yield return null;
            yield return null;

            Assert.IsNull(GameObject.Find("D3RuntimeSmokeProbe"),
                "正式 Player 在 MainMenu 不应安装只属于 InitialHarbor 的 D3 冒烟探针。");
            Assert.IsNull(GameObject.Find("D5RuntimeSmokeProbe"),
                "正式 Player 在 MainMenu 不应安装只属于 InitialHarbor 的 D5 冒烟探针。");
        }

        private static void AssertScopeTargetsStayInside(Transform scopeRoot, string scopeName)
        {
            var scopeType = Type.GetType(
                "Whalebone.Gameplay.UI.D16.D16ModalFocusScope, Assembly-CSharp");
            Assert.IsNotNull(scopeType, "D16ModalFocusScope 类型未被 Unity 主程序集加载。");
            Assert.IsNotNull(scopeRoot.GetComponent(scopeType),
                scopeName + " 缺少 D16ModalFocusScope。");
            var selectables = scopeRoot.GetComponentsInChildren<Selectable>(true);
            foreach (var selectable in selectables)
            {
                if (selectable == null || !selectable.isActiveAndEnabled) continue;
                var navigation = selectable.navigation;
                Assert.AreEqual(Navigation.Mode.Explicit, navigation.mode,
                    scopeName + " 的控件没有使用显式导航：" + selectable.name);
                AssertNavigationTargetInside(scopeRoot, navigation.selectOnUp, scopeName, selectable, "上");
                AssertNavigationTargetInside(scopeRoot, navigation.selectOnDown, scopeName, selectable, "下");
                AssertNavigationTargetInside(scopeRoot, navigation.selectOnLeft, scopeName, selectable, "左");
                AssertNavigationTargetInside(scopeRoot, navigation.selectOnRight, scopeName, selectable, "右");
            }
        }

        private static void AssertNavigationTargetInside(Transform scopeRoot, Selectable target,
            string scopeName, Selectable source, string direction)
        {
            Assert.IsNotNull(target, scopeName + " 的控件导航目标为空：" + source.name + " / " + direction);
            Assert.IsTrue(target.transform.IsChildOf(scopeRoot),
                scopeName + " 的控件导航穿透到模态外：" + source.name + " / " + direction + " -> " + target.name);
        }

        private static void AssertSelectedInside(Transform scopeRoot, string context)
        {
            Assert.IsNotNull(EventSystem.current, context + "没有 EventSystem。");
            var selected = EventSystem.current.currentSelectedGameObject;
            Assert.IsNotNull(selected, context + "没有选中控件。");
            Assert.IsTrue(selected.transform.IsChildOf(scopeRoot),
                context + "焦点不在当前模态范围内：" + selected.name);
        }

        private static void AssertOnlyMainMenuPanel(Transform mainMenu, Transform newGame,
            Transform settings, Transform about, Transform expected)
        {
            var panels = new[] { mainMenu, newGame, settings, about };
            int activeCount = 0;
            foreach (var panel in panels)
            {
                if (panel.gameObject.activeSelf) activeCount++;
            }

            Assert.AreEqual(1, activeCount, "主菜单导航时出现多个可见面板。");
            Assert.IsTrue(expected.gameObject.activeSelf, "预期面板没有显示：" + expected.name);
        }

        [UnityTest]
        public IEnumerator D16A03PauseMenuKeepsFocusAcrossAccessibilityAndDiagnostics()
        {
            Type pauseType = Type.GetType("Whalebone.Gameplay.UI.HarborPauseController, Assembly-CSharp");
            Type routerType = Type.GetType("Whalebone.Gameplay.UI.D16.D16InputRouter, Assembly-CSharp");
            Assert.IsNotNull(pauseType, "HarborPauseController 未被 Unity 主程序集加载。");
            Assert.IsNotNull(routerType, "D16InputRouter 未被 Unity 主程序集加载。");

            object pauseController = null;
            try
            {
                var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
                Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
                while (!load.isDone) yield return null;

                float deadline = Time.realtimeSinceStartup + 5f;
                while (pauseController == null && Time.realtimeSinceStartup < deadline)
                {
                    var controllerObject = GameObject.Find("HarborPauseController");
                    if (controllerObject != null)
                        pauseController = controllerObject.GetComponent(pauseType);
                    if (pauseController == null) yield return null;
                }

                Assert.IsNotNull(pauseController, "InitialHarbor 暂停控制器未创建。");
                var pauseMethod = pauseType.GetMethod("PauseGame",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(pauseMethod, "暂停控制器缺少 PauseGame 实现。");
                pauseMethod.Invoke(pauseController, null);
                yield return null;

                var pausePanel = GameObject.Find("PausePanel");
                Assert.IsNotNull(pausePanel, "暂停面板未创建。");
                Assert.IsTrue(pausePanel.activeSelf, "暂停后面板没有显示。");
                Assert.IsTrue((bool)pauseType.GetProperty("IsPaused").GetValue(pauseController),
                    "暂停按钮链路没有进入暂停状态。");
                Assert.IsNotNull(EventSystem.current, "InitialHarbor 没有可用 EventSystem。");
                Assert.AreEqual("Resume", EventSystem.current.currentSelectedGameObject?.name,
                    "暂停后焦点没有落到继续游戏按钮。");

                var accessibilityButton = GameObject.Find("Accessibility")?.GetComponent<Button>();
                Assert.IsNotNull(accessibilityButton, "暂停菜单无障碍按钮未创建。");
                accessibilityButton.onClick.Invoke();
                yield return null;

                var accessibilityHost = GameObject.Find("D16AccessibilityPanel");
                Assert.IsNotNull(accessibilityHost, "暂停菜单打开无障碍面板时宿主未创建。");
                var accessibilityModal = FindChild(accessibilityHost.transform, "AccessibilityModal");
                Assert.IsNotNull(accessibilityModal, "暂停菜单无障碍模态节点未创建。");
                Assert.IsTrue(accessibilityModal.gameObject.activeSelf,
                    "暂停菜单无障碍面板打开后仍是隐藏状态。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "无障碍模态打开后没有阻断玩法输入。");

                var accessibilityClose = FindChild(accessibilityHost.transform, "Close")?.GetComponent<Button>();
                Assert.IsNotNull(accessibilityClose, "暂停菜单无障碍面板关闭按钮未创建。");
                accessibilityClose.onClick.Invoke();
                yield return null;
                Assert.IsFalse(accessibilityModal.gameObject.activeSelf,
                    "关闭无障碍面板后仍拦截暂停菜单。");
                Assert.IsTrue(pausePanel.activeSelf, "关闭无障碍面板后暂停菜单没有保留。");
                Assert.IsTrue((bool)pauseType.GetProperty("IsPaused").GetValue(pauseController),
                    "关闭无障碍面板后游戏错误地恢复运行。");

                var diagnosticsButton = GameObject.Find("Diagnostics")?.GetComponent<Button>();
                Assert.IsNotNull(diagnosticsButton, "暂停菜单隐私诊断按钮未创建。");
                diagnosticsButton.onClick.Invoke();
                yield return null;

                var diagnosticsModal = GameObject.Find("DiagnosticsModal");
                Assert.IsNotNull(diagnosticsModal, "暂停菜单打开隐私面板时模态节点未创建。");
                Assert.IsTrue(diagnosticsModal.activeSelf, "隐私诊断模态打开后仍是隐藏状态。");
                Assert.IsTrue((bool)routerType.GetProperty("GameplayInputBlocked").GetValue(null),
                    "隐私诊断模态打开后没有阻断玩法输入。");

                // InitialHarbor 可能同时拥有 D4 对话页、D16 金融页和 D16
                // 诊断页的同名 Close 按钮。必须在当前模态根内取按钮，
                // 否则测试会误点别的页面并把测试定位错误当成产品失败。
                var diagnosticsClose = FindChild(diagnosticsModal.transform, "Close")?.GetComponent<Button>();
                Assert.IsNotNull(diagnosticsClose, "暂停菜单隐私面板关闭按钮未创建。");
                diagnosticsClose.onClick.Invoke();
                yield return null;
                Assert.IsFalse(diagnosticsModal.activeSelf, "关闭隐私面板后仍拦截暂停菜单。");
                Assert.IsTrue(pausePanel.activeSelf, "关闭隐私面板后暂停菜单没有保留。");

                var resumeButton = GameObject.Find("Resume")?.GetComponent<Button>();
                Assert.IsNotNull(resumeButton, "暂停菜单继续按钮未创建。");
                resumeButton.onClick.Invoke();
                yield return null;
                Assert.IsFalse(pausePanel.activeSelf, "点击继续游戏后暂停面板仍然显示。");
                Assert.IsFalse((bool)pauseType.GetProperty("IsPaused").GetValue(pauseController),
                    "点击继续游戏后控制器仍处于暂停状态。");
                Assert.AreEqual(1f, Time.timeScale, 0.001f, "点击继续游戏后时间缩放没有恢复。");
            }
            finally
            {
                Time.timeScale = 1f;
                var unblock = routerType.GetMethod("SetGameplayInputBlocked",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (unblock != null) unblock.Invoke(null, new object[] { false });
            }
        }

        [Serializable]
        private sealed class A03D9SubtitleReport
        {
            public string Scene;
            public string UnityVersion;
            public string EventId;
            public string AudioCue;
            public bool D9RuntimeReady;
            public bool AudioRuntimeReady;
            public bool EventAppended;
            public bool SubtitleVisible;
            public string SubtitleText;
            public bool Passed;
            public string FailureReason;
        }

        /// <summary>
        /// 通过真实 EventLog 追加一条 D9 事实，验证 D9 音频表现和字幕层的实际链路。
        /// 测试只使用内存日志，不调用 SaveProgress，不改变持久化存档。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A03D9EventPublishesAudioCueAndSubtitle()
        {
            const string eventId = "event_d9_crew_report";
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var d9Type = Type.GetType("Whalebone.Gameplay.D9.D9VerticalSliceRuntimeBootstrap, Assembly-CSharp");
            var audioType = Type.GetType("Whalebone.Gameplay.D9.D9AudioPresentationController, Assembly-CSharp");
            var settingsStoreType = Type.GetType("Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            var overlayType = Type.GetType("Whalebone.Gameplay.UI.D16.D16SubtitleOverlay, Assembly-CSharp");
            object d9 = null;
            object audio = null;
            float readyDeadline = Time.realtimeSinceStartup + 12f;
            while (Time.realtimeSinceStartup < readyDeadline)
            {
                d9 = FindComponent(d9Type);
                audio = FindComponent(audioType);
                bool d9Ready = ReadMember(d9, "IsReady") is bool d9Value && d9Value;
                bool audioReady = ReadMember(audio, "IsReady") is bool audioValue && audioValue;
                if (d9Ready && audioReady) break;
                yield return null;
            }

            object originalSettings = ReadMember(null, settingsStoreType, "Current");
            try
            {
                if (settingsStoreType != null && originalSettings != null)
                {
                    var testSettings = originalSettings.GetType().GetMethod("Clone",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(originalSettings, null);
                    WriteMember(testSettings, "Subtitles", true);
                    InvokeStatic(settingsStoreType, "Save", testSettings);
                }

                var report = new A03D9SubtitleReport
                {
                    Scene = SceneManager.GetActiveScene().name,
                    UnityVersion = Application.unityVersion,
                    EventId = eventId,
                    AudioCue = string.Empty,
                    D9RuntimeReady = ReadMember(d9, "IsReady") is bool d9ReadyValue && d9ReadyValue,
                    AudioRuntimeReady = ReadMember(audio, "IsReady") is bool audioReadyValue && audioReadyValue,
                    EventAppended = false,
                    SubtitleVisible = false,
                    SubtitleText = string.Empty,
                    Passed = false,
                    FailureReason = string.Empty,
                };

                yield return null;
                yield return null;
                var eventLog = ReadMember(d9, "EventLog");
                if (eventLog == null) throw new InvalidOperationException("D9 EventLog 未创建。");
                var worldEventType = eventLog.GetType().Assembly.GetType("Whalebone.Systems.Events.WorldEvent");
                if (worldEventType == null) throw new InvalidOperationException("WorldEvent 类型未加载。");
                var worldEvent = Activator.CreateInstance(worldEventType);
                WriteMember(worldEvent, "EventId", eventId);
                WriteMember(worldEvent, "WorldDay", 999);
                WriteMember(worldEvent, "LocationRef", "santa_margarita");
                WriteMember(worldEvent, "OwnerSystem", "D16.A03.PlayModeTest");
                WriteMember(worldEvent, "SaveRevision", 9000001L);
                var result = ReadMember(worldEvent, "Result");
                WriteMember(result, "Summary", "D9 字幕触发验收事件");
                var append = eventLog.GetType().GetMethod("Append", BindingFlags.Instance | BindingFlags.Public,
                    null, new[] { worldEventType }, null);
                if (append == null) throw new InvalidOperationException("EventLog.Append 未找到。");
                append.Invoke(eventLog, new[] { worldEvent });
                report.EventAppended = true;

                float subtitleDeadline = Time.realtimeSinceStartup + 3f;
                while (Time.realtimeSinceStartup < subtitleDeadline)
                {
                    report.AudioCue = ReadMember(audio, "LastCue") as string ?? string.Empty;
                    var overlay = ReadMember(null, overlayType, "Instance");
                    report.SubtitleVisible = ReadMember(null, overlayType, "IsVisible") is bool visible && visible;
                    report.SubtitleText = ReadMember(overlay, "_lastText") as string ?? string.Empty;
                    if (report.AudioCue == "crew_report" && report.SubtitleVisible) break;
                    yield return null;
                }

                if (!report.D9RuntimeReady || !report.AudioRuntimeReady)
                    report.FailureReason = "D9 或音频表现运行时未就绪。";
                else if (!report.EventAppended)
                    report.FailureReason = "测试事件未能追加到共享 EventLog。";
                else if (report.AudioCue != "crew_report")
                    report.FailureReason = "D9 事实没有产生 crew_report 音频 cue。";
                else if (!report.SubtitleVisible || string.IsNullOrWhiteSpace(report.SubtitleText))
                    report.FailureReason = "音频 cue 没有让字幕层显示文本。";
                else report.Passed = true;

                WriteA03D9SubtitleReport(report);
                Assert.IsTrue(report.Passed, report.FailureReason);
            }
            finally
            {
                if (settingsStoreType != null && originalSettings != null)
                    InvokeStatic(settingsStoreType, "Save", originalSettings);
            }
        }

        [Serializable]
        private sealed class A03D9CueSample
        {
            public string EventId;
            public string ExpectedCue;
            public string ActualCue;
            public bool SubtitleVisible;
            public string SubtitleText;
            public bool Passed;
            public string FailureReason;
        }

        [Serializable]
        private sealed class A03D9CueCoverageReport
        {
            public string Scene;
            public string UnityVersion;
            public bool D9RuntimeReady;
            public bool AudioRuntimeReady;
            public int ExpectedCueCount;
            public int PassedCueCount;
            public A03D9CueSample[] Samples;
            public bool Passed;
            public string FailureReason;
        }

        /// <summary>
        /// 逐条追加 D9 的 14 个冻结事件 ID，验证 EventLog → 音频 cue → 字幕的完整覆盖。
        /// 仍然只使用内存事件，不写入持久化存档，也不替代目标设备/真实音频素材验收。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A03AllD9EventsPublishAudioCueAndSubtitle()
        {
            string[] eventIds =
            {
                "event_d9_departure_prepared", "event_d9_departure", "event_d9_fishing_catch",
                "event_d9_weather_storm", "event_d9_crew_report", "event_d9_nearshore_patrol",
                "event_d9_nearshore_retreat", "event_d9_treasure_clue", "event_d9_treasure_resolved",
                "event_d9_returned_to_harbor", "event_d9_trade_settled", "event_d9_wage_settled",
                "event_d9_ship_repaired", "event_d9_save_checkpoint"
            };
            string[] expectedCues =
            {
                "departure_prepared", "departure", "fishing_catch", "weather_storm", "crew_report",
                "combat_warning", "combat_retreat", "treasure_clue", "treasure_resolved", "returned_to_harbor",
                "trade_settled", "wage_settled", "ship_repaired", "save_checkpoint"
            };

            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var d9Type = Type.GetType("Whalebone.Gameplay.D9.D9VerticalSliceRuntimeBootstrap, Assembly-CSharp");
            var audioType = Type.GetType("Whalebone.Gameplay.D9.D9AudioPresentationController, Assembly-CSharp");
            var settingsStoreType = Type.GetType("Whalebone.Gameplay.UI.D16.D16AccessibilitySettingsStore, Assembly-CSharp");
            var overlayType = Type.GetType("Whalebone.Gameplay.UI.D16.D16SubtitleOverlay, Assembly-CSharp");
            object d9 = null;
            object audio = null;
            float readyDeadline = Time.realtimeSinceStartup + 12f;
            while (Time.realtimeSinceStartup < readyDeadline)
            {
                d9 = FindComponent(d9Type);
                audio = FindComponent(audioType);
                bool d9Ready = ReadMember(d9, "IsReady") is bool d9Value && d9Value;
                bool audioReady = ReadMember(audio, "IsReady") is bool audioValue && audioValue;
                if (d9Ready && audioReady) break;
                yield return null;
            }

            object originalSettings = ReadMember(null, settingsStoreType, "Current");
            var report = new A03D9CueCoverageReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                D9RuntimeReady = ReadMember(d9, "IsReady") is bool d9ReadyFinal && d9ReadyFinal,
                AudioRuntimeReady = ReadMember(audio, "IsReady") is bool audioReadyFinal && audioReadyFinal,
                ExpectedCueCount = expectedCues.Length,
                PassedCueCount = 0,
                Samples = new A03D9CueSample[eventIds.Length],
                Passed = false,
                FailureReason = string.Empty,
            };

            try
            {
                if (settingsStoreType != null && originalSettings != null)
                {
                    var testSettings = originalSettings.GetType().GetMethod("Clone",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(originalSettings, null);
                    WriteMember(testSettings, "Subtitles", true);
                    InvokeStatic(settingsStoreType, "Save", testSettings);
                }

                // 让运行时音频游标先完成一次 Bind；否则首条追加事件会被正确地视为绑定前历史。
                yield return null;
                yield return null;
                var eventLog = ReadMember(d9, "EventLog");
                var worldEventType = eventLog?.GetType().Assembly.GetType("Whalebone.Systems.Events.WorldEvent");
                var append = worldEventType == null ? null : eventLog.GetType().GetMethod("Append", BindingFlags.Instance | BindingFlags.Public,
                    null, new[] { worldEventType }, null);
                if (eventLog == null || worldEventType == null || append == null)
                {
                    report.FailureReason = "D9 EventLog 或 WorldEvent.Append 未创建。";
                }
                else
                {
                    for (int i = 0; i < eventIds.Length; i++)
                    {
                        var sample = new A03D9CueSample
                        {
                            EventId = eventIds[i],
                            ExpectedCue = expectedCues[i],
                            ActualCue = string.Empty,
                            SubtitleVisible = false,
                            SubtitleText = string.Empty,
                            Passed = false,
                            FailureReason = string.Empty,
                        };
                        report.Samples[i] = sample;

                        var worldEvent = Activator.CreateInstance(worldEventType);
                        WriteMember(worldEvent, "EventId", eventIds[i]);
                        WriteMember(worldEvent, "WorldDay", 1000 + i);
                        WriteMember(worldEvent, "LocationRef", "santa_margarita");
                        WriteMember(worldEvent, "OwnerSystem", "D16.A03.CueCoverageTest");
                        WriteMember(worldEvent, "SaveRevision", 9100000L + i);
                        var result = ReadMember(worldEvent, "Result");
                        WriteMember(result, "Summary", "D16 A03 cue coverage test");
                        append.Invoke(eventLog, new[] { worldEvent });

                        float cueDeadline = Time.realtimeSinceStartup + 3f;
                        while (Time.realtimeSinceStartup < cueDeadline)
                        {
                            sample.ActualCue = ReadMember(audio, "LastCue") as string ?? string.Empty;
                            var overlay = ReadMember(null, overlayType, "Instance");
                            sample.SubtitleVisible = ReadMember(null, overlayType, "IsVisible") is bool visible && visible;
                            sample.SubtitleText = ReadMember(overlay, "_lastText") as string ?? string.Empty;
                            if (sample.ActualCue == sample.ExpectedCue && sample.SubtitleVisible && !string.IsNullOrWhiteSpace(sample.SubtitleText)) break;
                            yield return null;
                        }

                        if (sample.ActualCue != sample.ExpectedCue)
                            sample.FailureReason = "EventLog 事件没有产生预期音频 cue。";
                        else if (!sample.SubtitleVisible || string.IsNullOrWhiteSpace(sample.SubtitleText))
                            sample.FailureReason = "音频 cue 没有产生可见字幕文本。";
                        else
                        {
                            sample.Passed = true;
                            report.PassedCueCount++;
                        }
                    }
                }

                if (!report.D9RuntimeReady || !report.AudioRuntimeReady)
                    report.FailureReason = "D9 或音频表现运行时未就绪。";
                else if (report.PassedCueCount != report.ExpectedCueCount)
                    report.FailureReason = "14 个 D9 cue 未全部完成 EventLog→音频→字幕链路。";
                else report.Passed = true;

                WriteA03D9CueCoverageReport(report);
                Assert.IsTrue(report.Passed, report.FailureReason);
            }
            finally
            {
                if (settingsStoreType != null && originalSettings != null)
                    InvokeStatic(settingsStoreType, "Save", originalSettings);
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static D16RuntimePerformanceProbe FindActiveRuntimePerformanceProbe()
        {
            var probes = UnityEngine.Object.FindObjectsByType<D16RuntimePerformanceProbe>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < probes.Length; i++)
            {
                var probe = probes[i];
                if (probe != null && probe.isActiveAndEnabled && probe.gameObject.activeInHierarchy)
                    return probe;
            }
            return null;
        }

        private static object FindComponent(Type componentType)
        {
            if (componentType == null) return null;
            foreach (var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (component != null && componentType.IsInstanceOfType(component)) return component;
            return null;
        }

        private static object ReadMember(object target, string memberName)
        {
            return ReadMember(target, target == null ? null : target.GetType(), memberName);
        }

        private static object ReadMember(object target, Type targetType, string memberName)
        {
            if (targetType == null || string.IsNullOrWhiteSpace(memberName)) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic;
            var property = targetType.GetProperty(memberName, flags);
            if (property != null && property.CanRead) return property.GetValue(target, null);
            var field = targetType.GetField(memberName, flags);
            return field == null ? null : field.GetValue(target);
        }

        private static void WriteMember(object target, string memberName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName)) return;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.Public | BindingFlags.NonPublic;
            var targetType = target.GetType();
            var property = targetType.GetProperty(memberName, flags);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value, null);
                return;
            }
            var field = targetType.GetField(memberName, flags);
            if (field != null) field.SetValue(target, value);
        }

        private static void InvokeStatic(Type targetType, string methodName, object argument)
        {
            if (targetType == null) return;
            var method = targetType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null) method.Invoke(null, new[] { argument });
        }

        private static bool Approximately(Color left, Color right)
        {
            return Mathf.Abs(left.r - right.r) < 0.001f &&
                Mathf.Abs(left.g - right.g) < 0.001f &&
                Mathf.Abs(left.b - right.b) < 0.001f &&
                Mathf.Abs(left.a - right.a) < 0.001f;
        }

        private static void WriteA03AccessibilityReport(A03AccessibilityReport report)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A03AccessibilityReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A03] PASS " : "[D16/A03] FAIL ") + path);
        }

        private static void WriteA03D9SubtitleReport(A03D9SubtitleReport report)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A03D9SubtitleReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A03] PASS " : "[D16/A03] FAIL ") + path);
        }

        private static void WriteA03D9CueCoverageReport(A03D9CueCoverageReport report)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A03D9CueCoverageReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A03] PASS " : "[D16/A03] FAIL ") + path);
        }

        [Serializable]
        private sealed class A01FixtureReport
        {
            public string Scene;
            public string UnityVersion;
            public string GraphicsDevice;
            public bool StreamLoadedNearFocus;
            public bool StreamUnloadedFarFromFocus;
            public bool PresentationRootFollowedStreamState;
            public bool LodRendererReducedAtDistance;
            public bool ColliderStayedEnabled;
            public bool LodRendererRestoredNearFocus;
            public bool PoolReusedPresentationInstance;
            public bool PoolResetPresentationState;
            public int CellTransitionCount;
            public int PoolRentCount;
            public int PoolReuseCount;
            public int PoolReturnCount;
            public bool Passed;
            public string FailureReason;
        }

        [Serializable]
        private sealed class A01BudgetStressReport
        {
            public string Scene;
            public string UnityVersion;
            public string GraphicsDevice;
            public int ExpectedCandidateCount;
            public int CandidateCount;
            public int SceneRegistrationCount;
            public string MissingRegistrationIds;
            public int ActiveAgentCount;
            public int ActiveAnimalCount;
            public int AudibleSourceCount;
            public int MaxActiveAgents;
            public int MaxActiveAnimals;
            public int MaxAudibleSources;
            public bool ProtectedCandidateRemainedFull;
            public bool ProtectedAudioRemainedEnabled;
            public bool UnprotectedCandidateWasFrozen;
            public bool PresentationOnlyFixture;
            public bool BudgetSatisfied;
            public int ProtectedOverBudgetCount;
            public bool Passed;
            public string FailureReason;
        }

        [Serializable]
        private sealed class A01StatePreservationReport
        {
            public string Scene;
            public string UnityVersion;
            public string GraphicsDevice;
            public string FactEventId;
            public int EventCountBefore;
            public int EventCountAfter;
            public long FactSaveRevision;
            public long EventLogNextSequenceBefore;
            public long EventLogNextSequenceAfter;
            public bool FactPreserved;
            public bool SaveSnapshotPreserved;
            public bool SettlementClaimPreserved;
            public bool RestoredFactPresent;
            public bool ColliderStayedEnabled;
            public bool PresentationOnlyFixture;
            public bool Passed;
            public string FailureReason;
        }

        /// <summary>
        /// 在真实 InitialHarbor 播放循环中注入表现层压力，验证 Agent/动物/音频预算
        /// 的上限、稳定降级和受保护交互对象不会越过预算或被冻结。
        /// 夹具对象没有领域组件，只验证表现层预算裁决，不改变世界事实或存档。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01RuntimeBudgetCapsHighLoadWithoutFreezingProtectedObjects()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16WorldPerformanceController controller = null;
            float controllerDeadline = Time.realtimeSinceStartup + 5f;
            while (controller == null && Time.realtimeSinceStartup < controllerDeadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller, "D16 性能控制器未在 InitialHarbor 创建。");

            const int unprotectedAgentCount = 95;
            const int animalCount = 128;
            const int audioOnlyCount = 40;
            const int expectedCandidateCount = unprotectedAgentCount + 1 + animalCount + audioOnlyCount + 3;
            var fixture = new GameObject("D16_A01_BudgetStressFixture");
            var focus = new GameObject("D16_A01_BudgetStressFocus");
            focus.transform.position = Vector3.zero;
            controller.ConfigureRuntimeFocus(focus.transform, 0.05f);

            var protectedObject = new GameObject("D16_A01_ProtectedAgent");
            protectedObject.transform.SetParent(fixture.transform, false);
            var protectedRegistration = protectedObject.AddComponent<D16PresentationRegistration>();
            protectedRegistration.ConfigureRuntime("a01.load.protected", true, false, true, true, true);

            for (int i = 0; i < unprotectedAgentCount; i++)
            {
                var agent = new GameObject("D16_A01_LoadAgent_" + i.ToString("D3"));
                agent.transform.SetParent(fixture.transform, false);
                agent.transform.position = new Vector3(8f + (i % 16), 0f, i / 16);
                var registration = agent.AddComponent<D16PresentationRegistration>();
                registration.ConfigureRuntime("a01.load.agent." + i.ToString("D3"), true, false, false, false, false);
            }

            for (int i = 0; i < animalCount; i++)
            {
                var animal = new GameObject("D16_A01_LoadAnimal_" + i.ToString("D3"));
                animal.transform.SetParent(fixture.transform, false);
                animal.transform.position = new Vector3(12f + (i % 16), 0f, i / 16);
                var registration = animal.AddComponent<D16PresentationRegistration>();
                registration.ConfigureRuntime("a01.load.animal." + i.ToString("D3"), false, true, false, false, false);
            }

            for (int i = 0; i < audioOnlyCount; i++)
            {
                var audioObject = new GameObject("D16_A01_LoadAudio_" + i.ToString("D3"));
                audioObject.transform.SetParent(fixture.transform, false);
                audioObject.transform.position = new Vector3(16f + (i % 16), 0f, i / 16);
                var registration = audioObject.AddComponent<D16PresentationRegistration>();
                registration.ConfigureRuntime("a01.load.audio." + i.ToString("D3"), false, false, true, false, false);
            }

            controller.RefreshRegistrations();
            float budgetDeadline = Time.realtimeSinceStartup + 5f;
            while ((controller.LastSnapshot == null || controller.LastSnapshot.CandidateCount < expectedCandidateCount) &&
                   Time.realtimeSinceStartup < budgetDeadline)
                yield return null;

            var snapshot = controller.LastSnapshot;
            var observedDecisionIds = new HashSet<string>(StringComparer.Ordinal);
            if (snapshot != null)
            {
                foreach (var decision in snapshot.Decisions)
                    if (decision != null && !string.IsNullOrEmpty(decision.Id))
                        observedDecisionIds.Add(decision.Id);
            }
            var sceneRegistrations = UnityEngine.Object.FindObjectsByType<D16PresentationRegistration>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            var missingRegistrationIds = new List<string>();
            foreach (var registration in sceneRegistrations)
            {
                if (registration != null && !observedDecisionIds.Contains(registration.StableId))
                    missingRegistrationIds.Add(registration.StableId);
            }
            missingRegistrationIds.Sort(StringComparer.Ordinal);
            bool protectedFull = false;
            bool protectedAudio = false;
            bool unprotectedFrozen = false;
            if (snapshot != null)
            {
                foreach (var decision in snapshot.Decisions)
                {
                    if (decision == null) continue;
                    if (decision.Id == "a01.load.protected")
                    {
                        protectedFull = decision.SimulationTier == Whalebone.Systems.D16.D16SimulationTier.Full;
                        protectedAudio = decision.AudioEnabled;
                    }
                    else if (decision.Id == "a01.load.agent.094")
                    {
                        unprotectedFrozen = decision.SimulationTier == Whalebone.Systems.D16.D16SimulationTier.Frozen;
                    }
                }
            }

            var configuration = new Whalebone.Systems.D16.D16PerformanceBudgetConfiguration();
            var report = new A01BudgetStressReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                ExpectedCandidateCount = expectedCandidateCount,
                CandidateCount = snapshot == null ? 0 : snapshot.CandidateCount,
                SceneRegistrationCount = sceneRegistrations.Length,
                MissingRegistrationIds = string.Join(",", missingRegistrationIds),
                ActiveAgentCount = snapshot == null ? -1 : snapshot.ActiveAgentCount,
                ActiveAnimalCount = snapshot == null ? -1 : snapshot.ActiveAnimalCount,
                AudibleSourceCount = snapshot == null ? -1 : snapshot.AudibleSourceCount,
                MaxActiveAgents = configuration.MaxActiveAgents,
                MaxActiveAnimals = configuration.MaxActiveAnimals,
                MaxAudibleSources = configuration.MaxAudibleSources,
                ProtectedCandidateRemainedFull = protectedFull,
                ProtectedAudioRemainedEnabled = protectedAudio,
                UnprotectedCandidateWasFrozen = unprotectedFrozen,
                PresentationOnlyFixture = true,
                BudgetSatisfied = snapshot != null && snapshot.BudgetSatisfied,
                ProtectedOverBudgetCount = snapshot == null ? -1 : snapshot.ProtectedOverBudgetCount,
                Passed = false,
                FailureReason = string.Empty,
            };

            if (snapshot == null || report.CandidateCount < expectedCandidateCount)
                report.FailureReason = "高负载表现登记没有全部进入预算快照。";
            else if (!report.BudgetSatisfied || report.ActiveAgentCount > report.MaxActiveAgents ||
                     report.ActiveAnimalCount > report.MaxActiveAnimals || report.AudibleSourceCount > report.MaxAudibleSources)
                report.FailureReason = "Agent、动物或音频表现预算超出上限。";
            else if (!report.ProtectedCandidateRemainedFull || !report.ProtectedAudioRemainedEnabled)
                report.FailureReason = "受保护交互对象被错误降级。";
            else if (!report.UnprotectedCandidateWasFrozen)
                report.FailureReason = "高负载下未观察到非保护 Agent 的降级。";
            else if (report.ProtectedOverBudgetCount != 0)
                report.FailureReason = "保护对象出现预算超限。";
            else report.Passed = true;

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A01BudgetStressReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A01] PASS " : "[D16/A01] FAIL ") + path);
            UnityEngine.Object.Destroy(fixture);
            UnityEngine.Object.Destroy(focus);
            yield return null;
            Assert.IsTrue(report.Passed, report.FailureReason);
        }

        /// <summary>
        /// 验证表现预算降级只改变 Renderer/Animator/Audio，不改变事实日志、保存快照、
        /// 结算幂等门或碰撞。这里使用真实 EventLog 的快照/恢复路径，不把一个普通 JSON
        /// 字符串断言当成存档证据；完整文件写入、断电和跨平台仍由 D16-A02 负责。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01PresentationDowngradePreservesFactsSaveAndSettlement()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var fixture = new GameObject("D16_A01_StatePreservationFixture");
            var presentation = GameObject.CreatePrimitive(PrimitiveType.Cube);
            presentation.name = "D16_A01_StatePreservationPresentation";
            presentation.transform.SetParent(fixture.transform, false);
            var registration = presentation.AddComponent<D16PresentationRegistration>();
            registration.ConfigureRuntime("a01.fixture.state-preservation", false, false, false, false, false);
            var collider = presentation.GetComponent<Collider>();

            const string eventId = "event_d16_a01_state_preservation";
            const long saveRevision = 17L;
            var eventLog = new EventLog();
            var append = eventLog.Append(new WorldEvent(
                eventId,
                4,
                "santa_margarita",
                "D16A01TestOwner",
                new EventResult
                {
                    ResultCode = "state_preservation_probe",
                    Summary = "D16 presentation downgrade must not mutate this fact.",
                },
                saveRevision: saveRevision));
            Assert.AreEqual(EventAppendDisposition.Appended, append.Disposition,
                "D16-A01 状态保护夹具无法写入真实 EventLog 事实。");
            var firstClaim = eventLog.ClaimSettlement(eventId, saveRevision);
            Assert.AreEqual(EventSettlementDisposition.Claimed, firstClaim.Disposition,
                "D16-A01 状态保护夹具无法建立结算幂等基线。");
            var savedSnapshot = eventLog.CaptureSnapshot();
            var factBefore = eventLog.Find(eventId, saveRevision);
            Assert.IsNotNull(factBefore, "D16-A01 状态保护夹具缺少已写入事实。");

            D16SimulationTier[] tiers =
            {
                D16SimulationTier.Reduced,
                D16SimulationTier.Background,
                D16SimulationTier.Frozen,
                D16SimulationTier.Full,
            };
            foreach (var tier in tiers)
            {
                registration.Apply(new D16PerformanceDecision
                {
                    Id = "a01.fixture.state-preservation",
                    SimulationTier = tier,
                    AudioEnabled = false,
                });
                Assert.AreEqual(1, eventLog.Count, "表现降级不应删除事实日志记录。");
                Assert.AreEqual(savedSnapshot.NextSequence, eventLog.NextSequence,
                    "表现降级不应改变事实日志序列或保存快照版本。");
                var currentFact = eventLog.Find(eventId, saveRevision);
                Assert.IsNotNull(currentFact, "表现降级后事实日志不可读。");
                Assert.AreEqual(factBefore.EventId, currentFact.EventId,
                    "表现降级不应改变事实事件 ID。");
                Assert.AreEqual(factBefore.OwnerSystem, currentFact.OwnerSystem,
                    "表现降级不应改变事实归属系统。");
                Assert.AreEqual(factBefore.Result.Summary, currentFact.Result.Summary,
                    "表现降级不应改变事实内容。");
                Assert.IsTrue(collider != null && collider.enabled,
                    "表现降级不应关闭碰撞体。");
            }

            var secondClaim = eventLog.ClaimSettlement(eventId, saveRevision);
            Assert.AreEqual(EventSettlementDisposition.AlreadyClaimed, secondClaim.Disposition,
                "表现降级不应清除结算幂等门。");
            var restoredLog = EventLog.FromSnapshot(savedSnapshot);
            var restoredFact = restoredLog.Find(eventId, saveRevision);
            bool factPreserved = eventLog.Count == savedSnapshot.Facts.Count &&
                eventLog.NextSequence == savedSnapshot.NextSequence &&
                restoredFact != null && restoredFact.EventId == eventId;
            bool settlementPreserved = restoredLog.ClaimSettlement(eventId, saveRevision).Disposition ==
                EventSettlementDisposition.AlreadyClaimed;
            bool saveSnapshotPreserved = savedSnapshot.Facts.Count == 1 &&
                savedSnapshot.Facts[0] != null &&
                savedSnapshot.Facts[0].EventId == eventId &&
                savedSnapshot.Facts[0].SaveRevision == saveRevision;

            var report = new A01StatePreservationReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                FactEventId = eventId,
                EventCountBefore = savedSnapshot.Facts.Count,
                EventCountAfter = eventLog.Count,
                FactSaveRevision = saveRevision,
                EventLogNextSequenceBefore = savedSnapshot.NextSequence,
                EventLogNextSequenceAfter = eventLog.NextSequence,
                FactPreserved = factPreserved,
                SaveSnapshotPreserved = saveSnapshotPreserved,
                SettlementClaimPreserved = settlementPreserved,
                RestoredFactPresent = restoredFact != null,
                ColliderStayedEnabled = collider != null && collider.enabled,
                PresentationOnlyFixture = true,
                Passed = factPreserved && saveSnapshotPreserved && settlementPreserved &&
                    restoredFact != null && collider != null && collider.enabled,
                FailureReason = string.Empty,
            };
            if (!report.Passed)
                report.FailureReason = "表现降级后事实、保存快照、结算幂等门或碰撞状态未保持。";

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..",
                A01StatePreservationReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A01] PASS " : "[D16/A01] FAIL ") + path);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
            Assert.IsTrue(report.Passed, report.FailureReason);
        }

        /// <summary>
        /// 用真实 Unity 6 播放循环验证 A01 的最小运行时闭环。
        /// 夹具只创建表现层对象，不能替代目标设备的 CPU/GPU/内存基准。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01RuntimeFixtureExercisesStreamingLodAndPool()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16WorldPerformanceController controller = null;
            float controllerDeadline = Time.realtimeSinceStartup + 5f;
            while (controller == null && Time.realtimeSinceStartup < controllerDeadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller, "D16 性能控制器未在 InitialHarbor 创建。");

            var fixture = new GameObject("D16_A01_RuntimeFixture");
            var focus = new GameObject("D16_A01_Focus");
            focus.transform.position = Vector3.zero;
            controller.ConfigureRuntimeFocus(focus.transform, 0.05f);

            var cellObject = new GameObject("D16_A01_StreamCell");
            cellObject.transform.position = new Vector3(100f, 0f, 0f);
            cellObject.transform.SetParent(fixture.transform, true);
            var presentationRoot = new GameObject("PresentationRoot");
            presentationRoot.transform.SetParent(cellObject.transform, false);
            var cellRendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cellRendererObject.name = "StreamedPresentation";
            cellRendererObject.transform.SetParent(presentationRoot.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.cell", presentationRoot.transform, 10f, 20f, false);

            var lodObject = new GameObject("D16_A01_LodPresentation");
            lodObject.transform.position = new Vector3(400f, 0f, 0f);
            lodObject.transform.SetParent(fixture.transform, true);
            var lodRendererObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lodRendererObject.name = "LodMesh";
            lodRendererObject.transform.SetParent(lodObject.transform, false);
            var lodRenderer = lodRendererObject.GetComponent<Renderer>();
            var lodCollider = lodRendererObject.GetComponent<Collider>();
            var lodGroup = lodObject.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.6f, new[] { lodRenderer }),
                new LOD(0.2f, new[] { lodRenderer }),
                new LOD(0.01f, new[] { lodRenderer }),
            });
            lodGroup.RecalculateBounds();
            var lodRegistration = lodObject.AddComponent<D16PresentationRegistration>();
            lodRegistration.ConfigureRuntime("a01.fixture.lod", false, false, false, false, false);

            var poolObject = new GameObject("D16_A01_PresentationPool");
            poolObject.transform.SetParent(fixture.transform, true);
            var pool = poolObject.AddComponent<D16PresentationPool>();
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            prefab.name = "A01PresentationPrefab";
            prefab.transform.SetParent(fixture.transform, false);
            prefab.AddComponent<Animator>().enabled = false;
            prefab.AddComponent<AudioSource>();
            var pooledRegistration = prefab.AddComponent<D16PresentationRegistration>();
            pooledRegistration.ConfigureRuntime("a01.fixture.pooled", false, false, false, false, false);
            prefab.SetActive(false);

            controller.RefreshRegistrations();
            yield return null;

            focus.transform.position = cellObject.transform.position;
            float streamLoadDeadline = Time.realtimeSinceStartup + 5f;
            while (!cell.IsLoaded && Time.realtimeSinceStartup < streamLoadDeadline) yield return null;
            bool streamLoadedNearFocus = cell.IsLoaded;
            bool presentationLoadedNearFocus = presentationRoot.activeSelf;

            focus.transform.position = Vector3.zero;
            float streamUnloadDeadline = Time.realtimeSinceStartup + 5f;
            while (cell.IsLoaded && Time.realtimeSinceStartup < streamUnloadDeadline) yield return null;
            bool streamUnloadedFarFromFocus = !cell.IsLoaded;
            bool presentationUnloadedFarFromFocus = !presentationRoot.activeSelf;

            float lodReduceDeadline = Time.realtimeSinceStartup + 5f;
            while (lodRenderer.enabled && Time.realtimeSinceStartup < lodReduceDeadline) yield return null;
            bool lodRendererReducedAtDistance = !lodRenderer.enabled;
            bool colliderStayedEnabled = lodCollider.enabled;

            lodObject.transform.position = new Vector3(2f, 0f, 0f);
            float lodRestoreDeadline = Time.realtimeSinceStartup + 5f;
            while (!lodRenderer.enabled && Time.realtimeSinceStartup < lodRestoreDeadline) yield return null;
            bool lodRendererRestoredNearFocus = lodRenderer.enabled;

            var firstInstance = pool.Rent(prefab, fixture.transform, Vector3.zero, Quaternion.identity);
            bool firstReturned = pool.Return(firstInstance);
            var secondInstance = pool.Rent(prefab, fixture.transform, Vector3.one, Quaternion.identity);
            bool poolReusedPresentationInstance = firstReturned && firstInstance == secondInstance &&
                pool.RentCount == 2 && pool.ReuseCount == 1 && pool.ReturnCount == 1;
            var secondRenderer = secondInstance == null ? null : secondInstance.GetComponent<Renderer>();
            var secondAnimator = secondInstance == null ? null : secondInstance.GetComponent<Animator>();
            var secondAudio = secondInstance == null ? null : secondInstance.GetComponent<AudioSource>();
            var secondRegistration = secondInstance == null
                ? null
                : secondInstance.GetComponent<D16PresentationRegistration>();
            if (secondRegistration != null)
                secondRegistration.Apply(new D16PerformanceDecision
                {
                    Id = "a01.fixture.pooled",
                    SimulationTier = D16SimulationTier.Frozen,
                    AudioEnabled = false,
                });
            bool pooledStateWasDegraded = secondRenderer != null && !secondRenderer.enabled &&
                secondAnimator != null && !secondAnimator.enabled &&
                secondAudio != null && !secondAudio.enabled;
            pool.Return(secondInstance);
            var thirdInstance = pool.Rent(prefab, fixture.transform, Vector3.one * 2f, Quaternion.identity);
            var thirdRenderer = thirdInstance == null ? null : thirdInstance.GetComponent<Renderer>();
            var thirdAnimator = thirdInstance == null ? null : thirdInstance.GetComponent<Animator>();
            var thirdAudio = thirdInstance == null ? null : thirdInstance.GetComponent<AudioSource>();
            bool poolResetPresentationState = pooledStateWasDegraded && thirdRenderer != null && thirdRenderer.enabled &&
                thirdAnimator != null && !thirdAnimator.enabled &&
                thirdAudio != null && thirdAudio.enabled;
            pool.Return(thirdInstance);

            var report = new A01FixtureReport
            {
                Scene = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                GraphicsDevice = SystemInfo.graphicsDeviceName,
                StreamLoadedNearFocus = streamLoadedNearFocus,
                StreamUnloadedFarFromFocus = streamUnloadedFarFromFocus,
                PresentationRootFollowedStreamState = presentationLoadedNearFocus && presentationUnloadedFarFromFocus,
                LodRendererReducedAtDistance = lodRendererReducedAtDistance,
                ColliderStayedEnabled = colliderStayedEnabled,
                LodRendererRestoredNearFocus = lodRendererRestoredNearFocus,
                PoolReusedPresentationInstance = poolReusedPresentationInstance,
                PoolResetPresentationState = poolResetPresentationState,
                CellTransitionCount = controller.CellTransitionCount,
                PoolRentCount = pool.RentCount,
                PoolReuseCount = pool.ReuseCount,
                PoolReturnCount = pool.ReturnCount,
                Passed = false,
                FailureReason = string.Empty,
            };

            if (!report.StreamLoadedNearFocus || !report.StreamUnloadedFarFromFocus)
                report.FailureReason = "流式单元未按焦点距离完成加载和卸载。";
            else if (!report.PresentationRootFollowedStreamState)
                report.FailureReason = "PresentationRoot 没有跟随流式状态切换。";
            else if (!report.LodRendererReducedAtDistance || !report.LodRendererRestoredNearFocus)
                report.FailureReason = "LOD 表现没有按距离降级或恢复。";
            else if (!report.ColliderStayedEnabled)
                report.FailureReason = "表现降级错误地关闭了碰撞体。";
            else if (!report.PoolReusedPresentationInstance)
                report.FailureReason = "表现对象池没有复用已回收实例。";
            else if (!report.PoolResetPresentationState)
                report.FailureReason = "对象池复用实例残留上一次租借的降级表现状态。";
            else if (report.CellTransitionCount < 2)
                report.FailureReason = "流式单元没有记录至少两次状态切换。";
            else report.Passed = true;

            WriteA01FixtureReport(report);
            UnityEngine.Object.Destroy(fixture);
            UnityEngine.Object.Destroy(focus);
            yield return null;
            Assert.IsTrue(report.Passed, report.FailureReason);
        }

        /// <summary>
        /// 验证远景建筑在 Background 档使用代理表现，回到近处恢复本体，
        /// 且建筑根节点之外的逻辑碰撞体不被表现预算切换影响。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01RemoteBuildingProxyUsesBackgroundTierWithoutTouchingLogicCollider()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            D16WorldPerformanceController controller = null;
            float controllerDeadline = Time.realtimeSinceStartup + 5f;
            while (controller == null && Time.realtimeSinceStartup < controllerDeadline)
            {
                controller = UnityEngine.Object.FindFirstObjectByType<D16WorldPerformanceController>(FindObjectsInactive.Include);
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller, "D16 性能控制器未在 InitialHarbor 创建。");

            var fixture = new GameObject("D16_A01_RemoteBuildingProxyFixture");
            var focus = new GameObject("D16_A01_RemoteBuildingProxyFocus");
            focus.transform.position = Vector3.zero;

            var building = new GameObject("D16_A01_RemoteBuilding");
            building.transform.position = new Vector3(180f, 0f, 0f);
            building.transform.SetParent(fixture.transform, true);
            var logicCollider = building.AddComponent<BoxCollider>();
            logicCollider.size = new Vector3(4f, 4f, 4f);

            var fullRoot = new GameObject("FullBuildingPresentation");
            fullRoot.transform.SetParent(building.transform, false);
            var fullMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fullMesh.name = "FullBuildingMesh";
            fullMesh.transform.SetParent(fullRoot.transform, false);

            var proxyRoot = new GameObject("RemoteBuildingPresentation");
            proxyRoot.transform.SetParent(building.transform, false);
            var proxyMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            proxyMesh.name = "RemoteBuildingMesh";
            proxyMesh.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            proxyMesh.transform.SetParent(proxyRoot.transform, false);

            var proxy = building.AddComponent<D16RemoteBuildingProxy>();
            proxy.ConfigureRuntime("a01.fixture.remote_building", fullRoot.transform, proxyRoot.transform);
            controller.ConfigureRuntimeFocus(focus.transform, 0.05f);

            bool reachedBackgroundProxy = false;
            float backgroundDeadline = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < backgroundDeadline)
            {
                if (!fullRoot.activeSelf && proxyRoot.activeSelf)
                {
                    reachedBackgroundProxy = true;
                    break;
                }
                yield return null;
            }

            Assert.IsTrue(reachedBackgroundProxy,
                "远景建筑进入 Background 档后应隐藏本体并显示远景代理。");
            Assert.IsTrue(logicCollider.enabled,
                "远景建筑表现切换不得关闭表现根之外的逻辑碰撞体。");

            building.transform.position = Vector3.zero;
            bool restoredFullPresentation = false;
            float restoreDeadline = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < restoreDeadline)
            {
                if (fullRoot.activeSelf && !proxyRoot.activeSelf)
                {
                    restoredFullPresentation = true;
                    break;
                }
                yield return null;
            }

            Assert.IsTrue(restoredFullPresentation,
                "远景建筑回到玩家附近后应恢复本体并隐藏远景代理。");
            Assert.IsTrue(logicCollider.enabled,
                "恢复建筑本体后逻辑碰撞体仍应保持启用。");

            UnityEngine.Object.Destroy(fixture);
            UnityEngine.Object.Destroy(focus);
            yield return null;
        }

        /// <summary>
        /// D5 的 12 个岛屿目录必须进入 D16 的只读数据流索引；当前没有岛屿
        /// 3D 视觉根时，报告必须明确保留 data-only，而不能把海图 marker 当作场景。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01IslandCatalogDataStreamKeepsVisualGapExplicit()
        {
            var fixture = new GameObject("D16_A01_IslandDataStreamFixture");
            var registry = fixture.AddComponent<D16IslandDataStreamRegistry>();
            registry.ConfigureRuntime("santa_margarita");
            yield return null;

            Assert.AreEqual(12, registry.CatalogCellCount,
                "D5 岛屿目录应包含当前冻结的 12 个稳定岛屿 ID。\n");
            Assert.Greater(registry.ActiveCellCount, 0,
                "以当前岛屿为海图参考点时，至少应有一个活动数据单元。\n");
            Assert.AreEqual(12, registry.DataOnlyCellCount,
                "当前工作区没有岛屿 3D 视觉根时，所有目录条目必须明确保持 data-only。\n");
            Assert.AreEqual(0, registry.VisualPresentationCellCount,
                "海图 marker 不得被统计为岛屿 3D 视觉表现。\n");

            var current = registry.FindCell("santa_margarita");
            Assert.IsNotNull(current);
            Assert.IsTrue(current.IsMetadataActive);
            Assert.AreEqual(D16SimulationTier.Full, current.SimulationTier);
            Assert.IsFalse(current.HasVisualPresentation);

            var visualRoot = new GameObject("D16_A01_AuthorizedIslandVisualFixture");
            Assert.IsTrue(registry.RegisterVisualPresentation("santa_margarita", visualRoot.transform));
            Assert.AreEqual(11, registry.DataOnlyCellCount,
                "只有显式绑定视觉根的条目才能离开 data-only。\n");
            Assert.AreEqual(1, registry.VisualPresentationCellCount);

            UnityEngine.Object.Destroy(visualRoot);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 验证表现流式加载不会把整棵表现树集中到同一帧：活跃子对象按批次跨帧恢复，
        /// 场景原本明确禁用的对象保持禁用，卸载后下一次加载仍能恢复同一意图。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01StreamingCellLoadsPresentationAcrossFramesAndPreservesInactiveIntent()
        {
            const int activeChildCount = 37;
            const int activationBatchSize = 8;
            var fixture = new GameObject("D16_A01_StagedStreamingFixture");
            var cellObject = new GameObject("D16_A01_StagedStreamCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var presentationRoot = new GameObject("D16_A01_StagedPresentationRoot");
            presentationRoot.transform.SetParent(cellObject.transform, false);

            for (int i = 0; i < activeChildCount; i++)
            {
                var child = new GameObject("PresentationPart_" + i);
                child.transform.SetParent(presentationRoot.transform, false);
            }
            var intentionallyInactive = new GameObject("IntentionallyInactivePresentationPart");
            intentionallyInactive.transform.SetParent(presentationRoot.transform, false);
            intentionallyInactive.SetActive(false);

            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.staged", presentationRoot.transform, 10f, 20f, false,
                null, activationBatchSize);

            cell.SetLoaded(true);
            bool observedLoading = cell.IsLoading;
            bool observedPartialProgress = cell.LoadedPresentationObjectCount > 0 &&
                cell.LoadedPresentationObjectCount < activeChildCount &&
                cell.PendingPresentationObjectCount > 0;
            while (cell.IsLoading) yield return null;

            Assert.IsTrue(observedLoading, "表现流式加载应进入显式 loading 状态。");
            Assert.IsTrue(observedPartialProgress,
                "表现流式加载应在至少一帧中呈现部分进度，而不是一次性完成。");
            Assert.IsTrue(cell.IsLoaded);
            Assert.AreEqual(1f, cell.LoadingProgress, 0.0001f);
            Assert.AreEqual(activeChildCount, cell.LoadedPresentationObjectCount);
            Assert.AreEqual(0, cell.PendingPresentationObjectCount);
            Assert.IsFalse(intentionallyInactive.activeSelf,
                "流式加载不得覆盖场景原本明确禁用的表现对象。");
            Assert.AreEqual(1, cell.LoadCount);

            cell.SetLoaded(false);
            Assert.IsFalse(cell.IsLoaded);
            Assert.IsFalse(cell.IsLoading);
            Assert.AreEqual(0, cell.LoadedPresentationObjectCount);
            Assert.IsFalse(presentationRoot.activeSelf);

            cell.SetLoaded(true);
            while (cell.IsLoading) yield return null;
            Assert.AreEqual(2, cell.LoadCount,
                "卸载后再次进入流式范围应产生第二次表现加载。");
            Assert.AreEqual(activeChildCount, cell.LoadedPresentationObjectCount);
            Assert.IsFalse(intentionallyInactive.activeSelf);

            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// A01 音频生命周期回归：流式单元卸载时释放本单元登记的音频引用，
        /// 但共享/非明确归属资源只减少引用，不调用 Resources.UnloadAsset。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01StreamingCellReleasesOwnedAudioScopeWithoutUnloadingSharedClip()
        {
            var fixture = new GameObject("D16_A01_AudioStreamFixture");
            var cellObject = new GameObject("D16_A01_AudioStreamCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var presentationRoot = new GameObject("D16_A01_AudioPresentationRoot");
            presentationRoot.transform.SetParent(cellObject.transform, false);

            var scope = presentationRoot.AddComponent<D16AudioResourceScope>();
            var sharedClip = AudioClip.Create("D16_A01_SharedClip", 8, 1, 8000, false);

            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.audio", presentationRoot.transform, 10f, 20f, true, scope);
            Assert.AreEqual(scope, cell.AudioScope);
            // 只有在单元进入加载态后登记的资源，才属于该单元本次加载生命周期。
            Assert.IsTrue(scope.TrackLoadedClip("Shared/ambient", sharedClip));
            Assert.AreEqual(1, scope.LoadedCount);

            cell.SetLoaded(false);
            yield return null;

            Assert.IsFalse(cell.IsLoaded);
            Assert.AreEqual(0, scope.LoadedCount, "流式单元卸载后不应保留音频引用。");
            Assert.AreEqual(1, scope.SkippedSharedAssetReleaseCount,
                "未明确归属的共享音频只能跳过卸载，不能误删全局资源。");
            Assert.IsTrue(presentationRoot.activeSelf == false, "表现根应随单元卸载。");

            var sharedScopeObject = new GameObject("D16_A01_SharedAudioScope");
            sharedScopeObject.transform.SetParent(fixture.transform, false);
            var sharedScope = sharedScopeObject.AddComponent<D16AudioResourceScope>();
            var ownedScopeObject = new GameObject("D16_A01_OwnedAudioScope");
            ownedScopeObject.transform.SetParent(fixture.transform, false);
            var ownedScope = ownedScopeObject.AddComponent<D16AudioResourceScope>();
            ownedScope.ConfigureOwnership(true, "D16Audio/");
            var sharedAcrossScopesClip = AudioClip.Create("D16_A01_SharedAcrossScopes", 8, 1, 8000, false);
            Assert.IsTrue(sharedScope.TrackLoadedClip("Shared/ambient", sharedAcrossScopesClip));
            Assert.IsTrue(ownedScope.TrackLoadedClip("D16Audio/owned", sharedAcrossScopesClip));
            Assert.IsTrue(ownedScope.Release("D16Audio/owned"));
            Assert.IsTrue(sharedScope.Release("Shared/ambient"));
            Assert.AreEqual(0, ownedScope.UnloadedAssetCount,
                "明确归属引用先释放时，仍存在共享引用，不得提前卸载 clip。");
            Assert.AreEqual(1, sharedScope.SkippedSharedAssetReleaseCount,
                "最后释放共享引用时也必须跳过资源级卸载。");

            UnityEngine.Object.Destroy(sharedClip);
            UnityEngine.Object.Destroy(sharedAcrossScopesClip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 多个表现流式单元共用一个音频作用域时，卸载其中一个单元只能释放
        /// 该单元加载期间新增的引用，不能清空其他单元已经存在的引用。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01SharedAudioScopeKeepsOtherCellReferencesOnUnload()
        {
            var fixture = new GameObject("D16_A01_SharedAudioCellFixture");
            var scopeObject = new GameObject("D16_A01_SharedAudioScope");
            scopeObject.transform.SetParent(fixture.transform, false);
            var scope = scopeObject.AddComponent<D16AudioResourceScope>();

            var otherCellClip = AudioClip.Create("D16_A01_OtherCellClip", 8, 1, 8000, false);
            var currentCellClip = AudioClip.Create("D16_A01_CurrentCellClip", 8, 1, 8000, false);
            Assert.IsTrue(scope.TrackLoadedClip("Shared/ambient", otherCellClip));

            var cellObject = new GameObject("D16_A01_CurrentAudioCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var presentationRoot = new GameObject("D16_A01_CurrentAudioPresentation");
            presentationRoot.transform.SetParent(cellObject.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.shared-audio", presentationRoot.transform, 10f, 20f, true, scope);
            Assert.IsTrue(cell.TrackAudioResource("Shared/ambient", otherCellClip));
            Assert.IsTrue(cell.TrackAudioResource("Shared/current-cell", currentCellClip));
            Assert.AreEqual(2, scope.LoadedCount);

            cell.SetLoaded(false);
            yield return null;

            Assert.AreEqual(1, scope.LoadedCount,
                "卸载当前流式单元不能释放共享作用域中其他单元的音频引用。");
            Assert.IsTrue(scope.Release("Shared/ambient"),
                "其他单元的无 owner 音频引用仍应可显式释放。");
            Assert.AreEqual(0, scope.LoadedCount,
                "owner 释放后，最后一个共享路径引用释放时底层 clip 应移除。");
            Assert.IsFalse(scope.Release("Shared/current-cell"),
                "当前单元的音频引用应在卸载时被移除。");

            UnityEngine.Object.Destroy(otherCellClip);
            UnityEngine.Object.Destroy(currentCellClip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 音频可能在表现单元第一次进入视距前就被加载器登记。
        /// 这类引用也必须归属于该单元，不能因为登记时尚未 loaded 而变成泄漏的无 owner 引用。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01AudioRegisteredBeforeFirstLoadIsOwnedByCell()
        {
            var fixture = new GameObject("D16_A01_PreLoadAudioFixture");
            var scopeObject = new GameObject("D16_A01_PreLoadAudioScope");
            scopeObject.transform.SetParent(fixture.transform, false);
            var scope = scopeObject.AddComponent<D16AudioResourceScope>();
            var cellObject = new GameObject("D16_A01_PreLoadAudioCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var presentationRoot = new GameObject("D16_A01_PreLoadAudioPresentation");
            presentationRoot.transform.SetParent(cellObject.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.pre-load-audio", presentationRoot.transform, 10f, 20f,
                false, scope);
            var clip = AudioClip.Create("D16_A01_PreLoadClip", 8, 1, 8000, false);

            Assert.IsTrue(cell.TrackAudioResource("Shared/pre-load", clip));
            Assert.IsFalse(cell.IsLoaded);
            Assert.AreEqual(1, scope.LoadedCount);

            cell.SetLoaded(true);
            cell.SetLoaded(false);
            yield return null;

            Assert.AreEqual(0, scope.LoadedCount,
                "首次加载前登记的音频也必须随流式单元 owner 卸载。");
            Assert.IsFalse(scope.Release("Shared/pre-load"),
                "该路径不应退化为无 owner 引用并等待外部释放。");

            UnityEngine.Object.Destroy(clip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 一个保持 unloaded 的单元也可能在首次进入视距前收到表现音频登记。
        /// 重复设置 unloaded（包括表现根已被移除的情况）仍必须释放该单元 owner，
        /// 不能把 scope 中的音频引用留到场景结束。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01UnloadedCellReleasesOwnedAudioWithoutPresentationRoot()
        {
            var fixture = new GameObject("D16_A01_UnloadedAudioFixture");
            var scopeObject = new GameObject("D16_A01_UnloadedAudioScope");
            scopeObject.transform.SetParent(fixture.transform, false);
            var scope = scopeObject.AddComponent<D16AudioResourceScope>();
            var cellObject = new GameObject("D16_A01_UnloadedAudioCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.unloaded-audio", null, 10f, 20f, false, scope);
            var clip = AudioClip.Create("D16_A01_UnloadedClip", 8, 1, 8000, false);

            Assert.IsTrue(cell.TrackAudioResource("Shared/unloaded", clip));
            Assert.AreEqual(1, scope.LoadedCount);

            cell.SetLoaded(false);
            yield return null;

            Assert.AreEqual(0, scope.LoadedCount,
                "保持 unloaded 且没有表现根的单元也必须释放其 owner 音频。");
            Assert.IsFalse(scope.Release("Shared/unloaded"),
                "重复设置 unloaded 后不应留下无 owner 音频引用。");

            UnityEngine.Object.Destroy(clip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 单元在首次加载前被销毁时，已登记的 owner 音频也必须即时释放。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01DestroyBeforeFirstLoadReleasesOwnedAudio()
        {
            var fixture = new GameObject("D16_A01_DestroyBeforeLoadFixture");
            var scopeObject = new GameObject("D16_A01_DestroyBeforeLoadScope");
            scopeObject.transform.SetParent(fixture.transform, false);
            var scope = scopeObject.AddComponent<D16AudioResourceScope>();
            var cellObject = new GameObject("D16_A01_DestroyBeforeLoadCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var presentationRoot = new GameObject("D16_A01_DestroyBeforeLoadPresentation");
            presentationRoot.transform.SetParent(cellObject.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.destroy-before-load", presentationRoot.transform,
                10f, 20f, false, scope);
            var clip = AudioClip.Create("D16_A01_DestroyBeforeLoadClip", 8, 1, 8000, false);

            Assert.IsTrue(cell.TrackAudioResource("Shared/destroy-before-load", clip));
            Assert.AreEqual(1, scope.LoadedCount);
            UnityEngine.Object.Destroy(cellObject);
            yield return null;

            Assert.AreEqual(0, scope.LoadedCount,
                "首次加载前销毁流式单元时不得遗留 owner 音频。");
            Assert.IsFalse(scope.Release("Shared/destroy-before-load"));

            UnityEngine.Object.Destroy(clip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        /// <summary>
        /// 对象池复用流式单元时，重新配置不能覆盖旧 scope 或遗留旧表现根。
        /// </summary>
        [UnityTest]
        public IEnumerator D16A01ReconfigureReleasesPreviousAudioOwnerAndPresentationRoot()
        {
            var fixture = new GameObject("D16_A01_ReconfigureFixture");
            var scopeObject = new GameObject("D16_A01_ReconfigureScope");
            scopeObject.transform.SetParent(fixture.transform, false);
            var scope = scopeObject.AddComponent<D16AudioResourceScope>();
            var cellObject = new GameObject("D16_A01_ReconfigureCell");
            cellObject.transform.SetParent(fixture.transform, false);
            var firstRoot = new GameObject("D16_A01_ReconfigureFirstRoot");
            firstRoot.transform.SetParent(cellObject.transform, false);
            var cell = cellObject.AddComponent<D16WorldStreamCell>();
            cell.ConfigureRuntime("a01.fixture.reconfigure.first", firstRoot.transform, 10f, 20f, true, scope);
            var firstClip = AudioClip.Create("D16_A01_ReconfigureFirstClip", 8, 1, 8000, false);
            Assert.IsTrue(cell.TrackAudioResource("Shared/reconfigure-first", firstClip));
            Assert.AreEqual(1, scope.LoadedCount);

            var secondRoot = new GameObject("D16_A01_ReconfigureSecondRoot");
            secondRoot.transform.SetParent(cellObject.transform, false);
            cell.ConfigureRuntime("a01.fixture.reconfigure.second", secondRoot.transform, 10f, 20f, false, scope);
            yield return null;

            Assert.AreEqual(0, scope.LoadedCount,
                "重新配置流式单元时必须释放旧配置的 owner 音频。");
            Assert.IsFalse(firstRoot.activeSelf,
                "重新配置流式单元时不能保留旧表现根处于激活状态。");
            Assert.IsFalse(scope.Release("Shared/reconfigure-first"));

            var secondClip = AudioClip.Create("D16_A01_ReconfigureSecondClip", 8, 1, 8000, false);
            Assert.IsTrue(cell.TrackAudioResource("Shared/reconfigure-second", secondClip));
            cell.SetLoaded(false);
            yield return null;
            Assert.AreEqual(0, scope.LoadedCount,
                "重新配置后的新 owner 也必须能随单元卸载。");

            UnityEngine.Object.Destroy(firstClip);
            UnityEngine.Object.Destroy(secondClip);
            UnityEngine.Object.Destroy(fixture);
            yield return null;
        }

        private static void WriteA01FixtureReport(A01FixtureReport report)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", A01FixtureReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/A01] PASS " : "[D16/A01] FAIL ") + path);
            if (!report.Passed) Debug.LogError("[D16/A01] " + report.FailureReason);
        }

        private static void WriteReport(RuntimeReport report)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ReportRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log((report.Passed ? "[D16/PlayMode] PASS " : "[D16/PlayMode] FAIL ") + path);
            if (!report.Passed) Debug.LogError("[D16/PlayMode] " + report.FailureReason);
        }
    }
}
