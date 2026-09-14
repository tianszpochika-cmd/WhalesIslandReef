using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.D29
{
    /// <summary>
    /// Q15：D29 主 HUD 的仓库/市场/酒馆临时页焦点和互斥回归。
    /// 业务事实仍由 D3/D10 owner 裁决；这里只通过公开/既有 UI owner 入口
    /// 验证页面打开、重复请求、关闭和 Gameplay 输入恢复。
    /// </summary>
    public sealed class D29PanelFocusPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator D29Q15GameplayWasdDoesNotNavigatePrimaryHud()
        {
            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "D29-Q15-Gameplay-Input-Focus-Test",
                ShipTypeId = "small_fishing"
            });

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem, "InitialHarbor 缺少 EventSystem。");
            var uiInputModule = eventSystem.GetComponent(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            Assert.IsNotNull(uiInputModule, "InitialHarbor 缺少 InputSystemUIInputModule。");
            var moveProperty = uiInputModule.GetType().GetProperty("move",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(moveProperty, "InputSystemUIInputModule 缺少 move 绑定。");

            for (int settle = 0; settle < 4; settle++) yield return null;
            Assert.IsFalse(eventSystem.sendNavigationEvents,
                "游戏态主 HUD 不应把 W/S 交给 EventSystem 导航。");
            Assert.IsNull(moveProperty.GetValue(uiInputModule),
                "游戏态 UI move action 必须解绑，避免 W/S 驱动 HUD 选择。");
            Assert.IsNull(eventSystem.currentSelectedGameObject,
                "游戏态主 HUD 不应保留可被 W/S 移动的选中控件。");

            Keyboard keyboard = null;
            try
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();

                Press(keyboard.wKey, queueEventOnly: true);
                yield return null;
                Assert.IsFalse(eventSystem.sendNavigationEvents,
                    "W 按下后不能恢复 UI 导航。");
                Assert.IsNull(moveProperty.GetValue(uiInputModule),
                    "W 按下后不能恢复 InputSystemUIInputModule.move。");
                Assert.IsNull(eventSystem.currentSelectedGameObject,
                    "W 按下后不能产生 HUD 选中项。");
                Release(keyboard.wKey, queueEventOnly: true);
                yield return null;

                Press(keyboard.sKey, queueEventOnly: true);
                yield return null;
                Assert.IsFalse(eventSystem.sendNavigationEvents,
                    "S 按下后不能恢复 UI 导航。");
                Assert.IsNull(moveProperty.GetValue(uiInputModule),
                    "S 按下后不能恢复 InputSystemUIInputModule.move。");
                Assert.IsNull(eventSystem.currentSelectedGameObject,
                    "S 按下后不能产生 HUD 选中项。");
                Release(keyboard.sKey, queueEventOnly: true);
                yield return null;

                Debug.Log("[D29/Q15] gameplay_wasd_ui_isolated=true, navigation=false, selected=<none>");
            }
            finally
            {
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey, queueEventOnly: true);
                    Release(keyboard.sKey, queueEventOnly: true);
                    InputSystem.RemoveDevice(keyboard);
                }
            }
        }

        [UnityTest]
        public IEnumerator D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes()
        {
            var d29Type = FindLoadedType("Whalebone.Gameplay.D29.D29InteractionHud");
            var d3Type = FindLoadedType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            var d10Type = FindLoadedType("Whalebone.Gameplay.D10.D10TavernSocialBootstrap");
            var hudType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.IsNotNull(d29Type, "D29InteractionHud 未加载。");
            Assert.IsNotNull(d3Type, "D3RuntimeBootstrap 未加载。");
            Assert.IsNotNull(d10Type, "D10TavernSocialBootstrap 未加载。");
            Assert.IsNotNull(hudType, "RuntimeHudCoordinator 未加载。");

            // 使用真实的新游戏入口准备 Player/Ship；直接加载 InitialHarbor
            // 不会自动填充 GameState.CurrentShip，而 D10 owner 按契约应拒绝
            // 在缺少真实船状态时进入 ready。
            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "D29-Q15-Panel-Focus-Test",
                ShipTypeId = "small_fishing"
            });
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var d29 = Object.FindFirstObjectByType(d29Type, FindObjectsInactive.Include) as Component;
            var d3 = Object.FindFirstObjectByType(d3Type, FindObjectsInactive.Include) as Component;
            var d10 = Object.FindFirstObjectByType(d10Type, FindObjectsInactive.Include) as Component;
            var coordinator = Object.FindFirstObjectByType(hudType, FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(d29, "D29InteractionHud 没有创建。");
            Assert.IsNotNull(d3, "D3RuntimeBootstrap 没有创建。");
            Assert.IsNotNull(d10, "D10TavernSocialBootstrap 没有创建。");
            Assert.IsNotNull(coordinator, "RuntimeHudCoordinator 没有创建。");

            var d3Ready = d3Type.GetProperty("IsReady");
            var d10Ready = d10Type.GetProperty("IsReady");
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((!(bool)d3Ready.GetValue(d3) || !(bool)d10Ready.GetValue(d10)) &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue((bool)d3Ready.GetValue(d3), "D3 owner 没有进入 ready。");
            Assert.IsTrue((bool)d10Ready.GetValue(d10), "D10 owner 没有进入 ready。");

            var warehouseField = d29Type.GetField("_warehousePanel",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var toggleInfo = d29Type.GetMethod("ToggleInfo",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var openMarket = d29Type.GetMethod("OpenMarket",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var openTavern = d29Type.GetMethod("OpenTavern",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var isTradeOpen = d3Type.GetProperty("IsTradeOpen");
            var closeTrade = d3Type.GetMethod("CloseTradeFromCoordinator");
            var hasTavernFocus = d10Type.GetProperty("HasInputFocus");
            var closeTavern = d10Type.GetMethod("CloseSocialPage");
            var temporaryCanvas = hudType.GetField("_temporaryCanvasName",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var primaryInputAllowed = hudType.GetProperty("PrimaryInputAllowed");
            Assert.IsNotNull(warehouseField, "D29 缺少仓库 panel owner 字段。");
            Assert.IsNotNull(toggleInfo, "D29 缺少仓库 ToggleInfo owner 入口。");
            Assert.IsNotNull(openMarket, "D29 缺少 OpenMarket owner 入口。");
            Assert.IsNotNull(openTavern, "D29 缺少 OpenTavern owner 入口。");
            Assert.IsNotNull(isTradeOpen, "D3 缺少 IsTradeOpen。");
            Assert.IsNotNull(closeTrade, "D3 缺少 CloseTradeFromCoordinator。");
            Assert.IsNotNull(hasTavernFocus, "D10 缺少 HasInputFocus。");
            Assert.IsNotNull(closeTavern, "D10 缺少 CloseSocialPage。");
            Assert.IsNotNull(temporaryCanvas, "RuntimeHudCoordinator 缺少临时页状态。");
            Assert.IsNotNull(primaryInputAllowed, "RuntimeHudCoordinator 缺少 PrimaryInputAllowed。");

            var eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem, "InitialHarbor 缺少 EventSystem。");
            for (int settle = 0; settle < 3; settle++) yield return null;
            Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                "Q15 开始时主 HUD 必须拥有 Gameplay 输入。");
            Assert.IsFalse(eventSystem.sendNavigationEvents,
                "Q15 开始时 UI 导航不能占用 Gameplay 键盘。");

            Keyboard shortcutKeyboard = null;
            try
            {
                for (int cycle = 0; cycle < 10; cycle++)
                {
                    toggleInfo.Invoke(d29, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    var warehouse = warehouseField.GetValue(d29) as GameObject;
                    Assert.IsNotNull(warehouse, "仓库 panel 未创建。");
                    Assert.IsTrue(warehouse.activeSelf, "第 " + cycle + " 次仓库打开没有显示 panel。");
                    Assert.AreEqual("D29WarehousePanel", temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次仓库打开没有登记临时页 owner。");
                    Assert.IsFalse((bool)isTradeOpen.GetValue(d3),
                        "仓库打开时市场不能同时持有 trade owner。");
                    Assert.IsFalse((bool)hasTavernFocus.GetValue(d10),
                        "仓库打开时酒馆不能同时持有输入焦点。");
                    Assert.IsFalse((bool)primaryInputAllowed.GetValue(null),
                        "仓库打开时主 HUD 不能继续拥有快捷键输入。");
            Assert.IsTrue(eventSystem.sendNavigationEvents,
                        "仓库打开时 UI 导航没有取得模态输入权。");

                    // I 的重复/切换语义由 D29 owner 保持：第二次调用关闭仓库，
                    // 不创建第二个 panel，也不留下临时页焦点。
                    toggleInfo.Invoke(d29, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    Assert.IsFalse(warehouse.activeSelf, "第 " + cycle + " 次仓库关闭没有隐藏 panel。");
                    Assert.IsNull(temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次仓库关闭仍残留临时页 owner。");
                    Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                        "第 " + cycle + " 次仓库关闭后主 HUD 没有恢复输入权。");
                    Assert.IsFalse(eventSystem.sendNavigationEvents,
                        "第 " + cycle + " 次仓库关闭后 UI 导航仍占用 Gameplay 键盘。");
                    Assert.IsNull(eventSystem.currentSelectedGameObject,
                        "第 " + cycle + " 次仓库关闭后仍保留模态 UI 焦点。");
                }

                // 入口层不能只靠反射调用通过；用实际 Input System 键盘事件
                // 让 D29.Update 消费 legacy Input.GetKeyDown 的 I/M/T 映射，
                // 验证 Player 中真正会走到同一组 owner 方法。
                shortcutKeyboard = InputSystem.AddDevice<Keyboard>();
                shortcutKeyboard.MakeCurrent();
                Press(shortcutKeyboard.iKey, queueEventOnly: true);
                yield return null;
                Release(shortcutKeyboard.iKey, queueEventOnly: true);
                yield return null;
                var keyboardWarehouse = warehouseField.GetValue(d29) as GameObject;
                Assert.IsNotNull(keyboardWarehouse, "I 快捷键没有创建仓库 panel。");
                Assert.IsTrue(keyboardWarehouse.activeSelf, "I 快捷键没有打开仓库。");
                Press(shortcutKeyboard.iKey, queueEventOnly: true);
                yield return null;
                Release(shortcutKeyboard.iKey, queueEventOnly: true);
                yield return null;
                Assert.IsFalse(keyboardWarehouse.activeSelf, "I 快捷键没有关闭仓库。");

                Press(shortcutKeyboard.mKey, queueEventOnly: true);
                yield return null;
                Release(shortcutKeyboard.mKey, queueEventOnly: true);
                yield return null;
                Assert.IsTrue((bool)isTradeOpen.GetValue(d3), "M 快捷键没有打开 D3 市场 owner。");
                closeTrade.Invoke(d3, null);
                yield return new WaitForSecondsRealtime(0.36f);

                Press(shortcutKeyboard.tKey, queueEventOnly: true);
                yield return null;
                Release(shortcutKeyboard.tKey, queueEventOnly: true);
                yield return null;
                Assert.IsTrue((bool)hasTavernFocus.GetValue(d10), "T 快捷键没有打开 D10 酒馆 owner。");
                closeTavern.Invoke(d10, null);
                yield return new WaitForSecondsRealtime(0.36f);
                InputSystem.RemoveDevice(shortcutKeyboard);
                shortcutKeyboard = null;
                Debug.Log("[D29/Q15] shortcut_input=true, keys=I,M,T, legacy_keydown_reached_owner=true");

                for (int cycle = 0; cycle < 10; cycle++)
                {
                    openMarket.Invoke(d29, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    Assert.IsTrue((bool)isTradeOpen.GetValue(d3),
                        "第 " + cycle + " 次市场打开没有进入 D3 trade owner。");
                    Assert.AreEqual("D3Canvas", temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次市场打开没有登记 D3 临时页。");
                    Assert.IsFalse((bool)hasTavernFocus.GetValue(d10),
                        "市场打开时酒馆不能同时持有输入焦点。");
                    Assert.IsFalse((bool)primaryInputAllowed.GetValue(null),
                        "市场打开时主 HUD 不能继续拥有快捷键输入。");
                    Assert.IsTrue(eventSystem.sendNavigationEvents,
                        "市场打开时 UI 导航没有取得模态输入权。");

                    // 重复 M 请求必须被输入 owner 拒绝，不能叠加第二层交易页。
                    openMarket.Invoke(d29, null);
                    yield return null;
                    Assert.IsTrue((bool)isTradeOpen.GetValue(d3),
                        "市场重复请求不能关闭或重建当前 trade owner。");
                    closeTrade.Invoke(d3, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    Assert.IsFalse((bool)isTradeOpen.GetValue(d3),
                        "第 " + cycle + " 次市场关闭没有离开 D3 trade owner。");
                    Assert.IsNull(temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次市场关闭仍残留临时页 owner。");
                    Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                        "第 " + cycle + " 次市场关闭后主 HUD 没有恢复输入权。");
                    Assert.IsFalse(eventSystem.sendNavigationEvents,
                        "第 " + cycle + " 次市场关闭后 UI 导航仍占用 Gameplay 键盘。");
                    yield return new WaitForSecondsRealtime(0.36f);
                }

                for (int cycle = 0; cycle < 10; cycle++)
                {
                    openTavern.Invoke(d29, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    Assert.IsTrue((bool)hasTavernFocus.GetValue(d10),
                        "第 " + cycle + " 次酒馆打开没有获得 D10 输入焦点。");
                    Assert.AreEqual("D10TavernSocialPage", temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次酒馆打开没有登记 D10 临时页。");
                    Assert.IsFalse((bool)isTradeOpen.GetValue(d3),
                        "酒馆打开时市场不能同时持有 trade owner。");
                    var warehouse = warehouseField.GetValue(d29) as GameObject;
                    Assert.IsFalse(warehouse != null && warehouse.activeSelf,
                        "酒馆打开时仓库不能同时显示。");
                    Assert.IsFalse((bool)primaryInputAllowed.GetValue(null),
                        "酒馆打开时主 HUD 不能继续拥有快捷键输入。");
                    Assert.IsTrue(eventSystem.sendNavigationEvents,
                        "酒馆打开时 UI 导航没有取得模态输入权。");

                    // 重复 T 请求不能重复创建酒馆页或重置 owner 状态。
                    openTavern.Invoke(d29, null);
                    yield return null;
                    Assert.IsTrue((bool)hasTavernFocus.GetValue(d10),
                        "酒馆重复请求不能丢失当前 D10 输入焦点。");
                    closeTavern.Invoke(d10, null);
                    for (int settle = 0; settle < 2; settle++) yield return null;
                    Assert.IsFalse((bool)hasTavernFocus.GetValue(d10),
                        "第 " + cycle + " 次酒馆关闭没有释放 D10 输入焦点。");
                    Assert.IsNull(temporaryCanvas.GetValue(coordinator),
                        "第 " + cycle + " 次酒馆关闭仍残留临时页 owner。");
                    Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                        "第 " + cycle + " 次酒馆关闭后主 HUD 没有恢复输入权。");
                    Assert.IsFalse(eventSystem.sendNavigationEvents,
                        "第 " + cycle + " 次酒馆关闭后 UI 导航仍占用 Gameplay 键盘。");
                    Assert.IsNull(eventSystem.currentSelectedGameObject,
                        "第 " + cycle + " 次酒馆关闭后仍保留模态 UI 焦点。");
                    yield return new WaitForSecondsRealtime(0.36f);
                }

                Debug.Log("[D29/Q15] warehouse_cycles=10, market_cycles=10, tavern_cycles=10, " +
                    "exclusive=true, repeated_request_rejected=true, gameplay_recovery=true");
            }
            finally
            {
                var warehouse = warehouseField.GetValue(d29) as GameObject;
                if (warehouse != null && warehouse.activeSelf) toggleInfo.Invoke(d29, null);
                if ((bool)isTradeOpen.GetValue(d3)) closeTrade.Invoke(d3, null);
                if ((bool)hasTavernFocus.GetValue(d10)) closeTavern.Invoke(d10, null);
                if (shortcutKeyboard != null && shortcutKeyboard.added)
                    InputSystem.RemoveDevice(shortcutKeyboard);
            }
        }

        private static System.Type FindLoadedType(string fullName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
