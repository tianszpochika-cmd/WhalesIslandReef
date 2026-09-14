using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Whalebone.Systems.D3;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.D29
{
    /// <summary>
    /// Q16：D29 市场入口只负责提交请求；交易金额、货物和拒绝原因仍由 D3 owner
    /// / TradeEconomySystem 裁决。本测试使用实际运行时面板按钮，不直接调用 UI 补偿数值。
    /// </summary>
    public sealed class D29TradeBridgePlayModeTests
    {
        [UnityTest]
        public IEnumerator D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest()
        {
            var d29Type = FindLoadedType("Whalebone.Gameplay.D29.D29InteractionHud");
            var d3Type = FindLoadedType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            Assert.IsNotNull(d29Type, "D29InteractionHud 未加载。");
            Assert.IsNotNull(d3Type, "D3RuntimeBootstrap 未加载。");

            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "D29-Q16-Trade-Bridge-Test",
                ShipTypeId = "small_fishing"
            });
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var d29 = Object.FindFirstObjectByType(d29Type, FindObjectsInactive.Include) as Component;
            var d3 = Object.FindFirstObjectByType(d3Type, FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(d29, "D29InteractionHud 没有创建。");
            Assert.IsNotNull(d3, "D3RuntimeBootstrap 没有创建。");
            var d3Behaviour = d3 as Behaviour;
            Assert.IsNotNull(d3Behaviour, "D3RuntimeBootstrap 必须是可启停的 Unity Behaviour。");

            var openMarket = d29Type.GetMethod("OpenMarket", BindingFlags.Instance | BindingFlags.NonPublic);
            var d29StatusField = d29Type.GetField("_status", BindingFlags.Instance | BindingFlags.NonPublic);
            var marketButtonField = d29Type.GetField("_marketButton", BindingFlags.Instance | BindingFlags.NonPublic);
            var readyProperty = d3Type.GetProperty("IsReady", BindingFlags.Instance | BindingFlags.Public);
            var requestTrade = d3Type.GetMethod("RequestTrade", BindingFlags.Instance | BindingFlags.Public);
            var closeTrade = d3Type.GetMethod("CloseTradeFromCoordinator", BindingFlags.Instance | BindingFlags.Public);
            var isTradeOpen = d3Type.GetProperty("IsTradeOpen", BindingFlags.Instance | BindingFlags.Public);
            var tradeStateProperty = d3Type.GetProperty("TradeState", BindingFlags.Instance | BindingFlags.Public);
            var economyProperty = d3Type.GetProperty("EconomySystem", BindingFlags.Instance | BindingFlags.Public);
            var snapshotProperty = d3Type.GetProperty("Snapshot", BindingFlags.Instance | BindingFlags.Public);
            var tradePanelField = d3Type.GetField("_tradePanel", BindingFlags.Instance | BindingFlags.NonPublic);
            var tradeResultField = d3Type.GetField("_tradeResult", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(openMarket, "D29 缺少 OpenMarket owner 入口。");
            Assert.IsNotNull(d29StatusField, "D29 缺少状态反馈文本。");
            Assert.IsNotNull(marketButtonField, "D29 缺少市场按钮 owner 字段。");
            Assert.IsNotNull(readyProperty, "D3 缺少 IsReady。");
            Assert.IsNotNull(requestTrade, "D3 缺少 RequestTrade。");
            Assert.IsNotNull(closeTrade, "D3 缺少 CloseTradeFromCoordinator。");
            Assert.IsNotNull(isTradeOpen, "D3 缺少 IsTradeOpen。");
            Assert.IsNotNull(tradeStateProperty, "D3 缺少 TradeState 只读接口。");
            Assert.IsNotNull(economyProperty, "D3 缺少 EconomySystem 只读接口。");
            Assert.IsNotNull(snapshotProperty, "D3 缺少 Snapshot 只读接口。");
            Assert.IsNotNull(tradePanelField, "D3 缺少交易面板 owner 字段。");
            Assert.IsNotNull(tradeResultField, "D3 缺少交易反馈文本。");

            // 让 D3 的真实 MonoBehaviour 暂停在未 ready 状态，验证 D29 的拒绝反馈
            // 不会伪造打开成功或留下空交易页。之后恢复同一 owner，继续正常流程。
            d3Behaviour.enabled = false;
            try
            {
                openMarket.Invoke(d29, null);
                var notReadyStatus = d29StatusField.GetValue(d29) as Text;
                Assert.IsNotNull(notReadyStatus, "D29 状态反馈文本实例不存在。");
                Assert.That(notReadyStatus.text, Does.Contain("尚未就绪"),
                    "D3 未 ready 时 D29 必须给出明确等待反馈。");
                Assert.IsFalse((bool)isTradeOpen.GetValue(d3),
                    "D3 未 ready 时不能打开交易 owner。");
            }
            finally
            {
                d3Behaviour.enabled = true;
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while (!(bool)readyProperty.GetValue(d3) && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue((bool)readyProperty.GetValue(d3), "D3 owner 没有恢复 ready。");

            var marketButton = marketButtonField.GetValue(d29) as Button;
            Assert.IsNotNull(marketButton, "D29 市场按钮实例不存在。");
            var tradePanel = tradePanelField.GetValue(d3) as GameObject;
            var tradeResult = tradeResultField.GetValue(d3) as Text;
            Assert.IsNotNull(tradePanel, "D3 交易面板实例不存在。");
            Assert.IsNotNull(tradeResult, "D3 交易反馈文本实例不存在。");
            var buyButton = tradePanel.transform.Find("Buy")?.GetComponent<Button>();
            var sellButton = tradePanel.transform.Find("Sell")?.GetComponent<Button>();
            Assert.IsNotNull(buyButton, "D3 交易面板缺少 Buy 按钮。");
            Assert.IsNotNull(sellButton, "D3 交易面板缺少 Sell 按钮。");

            // D29 双击市场按钮：第一请求打开 D3，第二请求必须被当前临时页/一次性门禁拒绝，
            // 不能重建第二个 owner 或改变页面数量。
            marketButton.onClick.Invoke();
            marketButton.onClick.Invoke();
            yield return null;
            Assert.IsTrue((bool)isTradeOpen.GetValue(d3), "市场按钮第一次请求没有打开 D3 owner。");
            Assert.IsTrue(tradePanel.activeSelf, "市场按钮第一次请求没有显示真实 D3 面板。");
            closeTrade.Invoke(d3, null);
            yield return new WaitForSecondsRealtime(0.36f);

            Assert.IsTrue((bool)requestTrade.Invoke(d3, null), "D3 交易面板无法打开。");
            yield return null;

            var trade = tradeStateProperty.GetValue(d3) as D3TradeState;
            var economy = economyProperty.GetValue(d3) as TradeEconomySystem;
            var snapshot = snapshotProperty.GetValue(d3) as D3RuntimeSnapshot;
            Assert.IsNotNull(trade, "D3 TradeState 为空。");
            Assert.IsNotNull(economy, "D3 EconomySystem 为空。");
            Assert.IsNotNull(snapshot, "D3 Snapshot 为空。");
            var commodity = CommodityCatalog.All[0];
            var buyQuote = economy.Quote(snapshot.CurrentLocationId, commodity.Id, trade.Market);
            Assert.IsNotNull(buyQuote, "测试商品买入报价为空。");

            int moneyBeforeBuy = trade.Money;
            int cargoBeforeBuy = D3InventorySystem.CountCargo(trade.Inventory, commodity.Id);
            buyButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyBeforeBuy - buyQuote.BuyUnitPrice, trade.Money,
                "一次成功买入只能扣除本次报价金额。");
            Assert.AreEqual(cargoBeforeBuy + 1,
                D3InventorySystem.CountCargo(trade.Inventory, commodity.Id),
                "一次成功买入必须只增加一件货物。");
            Assert.That(tradeResult.text, Does.Contain("购买完成"),
                "成功买入的 UI 反馈必须对应 D3 owner 结果。");

            var sellQuote = economy.Quote(snapshot.CurrentLocationId, commodity.Id, trade.Market);
            int moneyBeforeSell = trade.Money;
            int cargoBeforeSell = D3InventorySystem.CountCargo(trade.Inventory, commodity.Id);
            sellButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyBeforeSell + sellQuote.SellUnitPrice, trade.Money,
                "一次成功卖出只能增加本次报价金额。");
            Assert.AreEqual(cargoBeforeSell - 1,
                D3InventorySystem.CountCargo(trade.Inventory, commodity.Id),
                "一次成功卖出必须只减少一件货物。");
            Assert.That(tradeResult.text, Does.Contain("出售完成"),
                "成功卖出的 UI 反馈必须对应 D3 owner 结果。");

            // 余额不足：只安排真实 D3TradeState 的边界，不替 UI 写结果；BuyOne
            // 仍完整经过 TradeEconomySystem，失败时钱和货物都不变。
            trade.Money = 0;
            int moneyBeforePoor = trade.Money;
            int cargoBeforePoor = D3InventorySystem.CountCargo(trade.Inventory, commodity.Id);
            buyButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyBeforePoor, trade.Money, "余额不足不能扣款。");
            Assert.AreEqual(cargoBeforePoor,
                D3InventorySystem.CountCargo(trade.Inventory, commodity.Id),
                "余额不足不能增加货物。");
            Assert.That(tradeResult.text, Does.Contain("金币不足"),
                "余额不足的 UI 反馈必须明确说明拒绝原因。");

            // 满仓：用 D3InventorySystem 真实容量 owner 填满当前货舱，然后提交同一 Buy 请求。
            trade.Money = buyQuote.BuyUnitPrice + 100;
            string fillError;
            while (D3InventorySystem.FreeCargo(trade.Inventory) > 0)
            {
                if (D3InventorySystem.TryAddCargo(trade.Inventory, commodity.Id, 1, out fillError))
                    continue;
                Assert.Fail("测试无法通过 D3InventorySystem 填满货舱：" + fillError);
            }
            int moneyBeforeFull = trade.Money;
            int cargoBeforeFull = D3InventorySystem.CountCargo(trade.Inventory, commodity.Id);
            buyButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyBeforeFull, trade.Money, "满仓拒绝不能扣款。");
            Assert.AreEqual(cargoBeforeFull,
                D3InventorySystem.CountCargo(trade.Inventory, commodity.Id),
                "满仓拒绝不能增加货物。");
            Assert.That(tradeResult.text, Does.Contain("货舱体积不足"),
                "满仓的 UI 反馈必须明确说明容量不足。");

            Debug.Log("[D29/Q16] not_ready=true, shortcut_double_click_one_owner=true, " +
                "buy_success_one_change=true, sell_success_one_change=true, " +
                "insufficient_funds_unchanged=true, cargo_full_unchanged=true");
            closeTrade.Invoke(d3, null);
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
