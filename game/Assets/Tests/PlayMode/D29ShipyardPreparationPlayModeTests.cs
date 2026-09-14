using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.D29
{
    /// <summary>
    /// Q17：D29 只负责打开页面、显示结果和恢复焦点；修船、材料、金币及船坞等级
    /// 仍由 D9/D3/D4 owner 裁决。本测试通过运行时生成的真实按钮覆盖成功、拒绝、
    /// 重复、取消和异常恢复，不把 UI 文案当成业务成功证据。
    /// </summary>
    public sealed class D29ShipyardPreparationPlayModeTests
    {
        [UnityTest]
        public IEnumerator D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException()
        {
            var d29Type = FindLoadedType("Whalebone.Gameplay.D29.D29InteractionHud");
            var d3Type = FindLoadedType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            var d4Type = FindLoadedType("Whalebone.Gameplay.D4.D4RuntimeBootstrap");
            var d9Type = FindLoadedType("Whalebone.Gameplay.D9.D9PortShipPreparationBootstrap");
            var hudType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.IsNotNull(d29Type, "D29InteractionHud 未加载。");
            Assert.IsNotNull(d3Type, "D3RuntimeBootstrap 未加载。");
            Assert.IsNotNull(d4Type, "D4RuntimeBootstrap 未加载。");
            Assert.IsNotNull(d9Type, "D9PortShipPreparationBootstrap 未加载。");
            Assert.IsNotNull(hudType, "RuntimeHudCoordinator 未加载。");

            SaveSystem.NewGame(new NewGameParams
            {
                CaptainName = "D29-Q17-Shipyard-Preparation-Test",
                ShipTypeId = "small_fishing"
            });
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var d29 = UnityEngine.Object.FindFirstObjectByType(d29Type, FindObjectsInactive.Include) as Component;
            var d3 = UnityEngine.Object.FindFirstObjectByType(d3Type, FindObjectsInactive.Include) as Component;
            var d4 = UnityEngine.Object.FindFirstObjectByType(d4Type, FindObjectsInactive.Include) as Component;
            var d9 = UnityEngine.Object.FindFirstObjectByType(d9Type, FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(d29, "D29InteractionHud 没有创建。");
            Assert.IsNotNull(d3, "D3RuntimeBootstrap 没有创建。");
            Assert.IsNotNull(d4, "D4RuntimeBootstrap 没有创建。");
            Assert.IsNotNull(d9, "D9PortShipPreparationBootstrap 没有创建。");

            var d29PreparationButton = ReadField<Button>(d29, d29Type, "_preparationButton");
            var d29ShipyardButton = ReadField<Button>(d29, d29Type, "_shipyardButton");
            var d29Status = ReadField<Text>(d29, d29Type, "_status");
            var d9Panel = ReadField<GameObject>(d9, d9Type, "_panel");
            var d9LastMessageField = d9Type.GetField("_lastMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            var d9Preparation = ReadField<D9PortShipPreparationSystem>(d9, d9Type, "_preparation");
            var d4ShipyardField = d4Type.GetField("_shipyard", BindingFlags.Instance | BindingFlags.NonPublic);
            var d4FactionsField = d4Type.GetField("_factions", BindingFlags.Instance | BindingFlags.NonPublic);
            var primaryInputAllowed = hudType.GetProperty("PrimaryInputAllowed", BindingFlags.Static | BindingFlags.Public);
            var restorePrimaryHud = hudType.GetMethod("RestorePrimaryHud", BindingFlags.Static | BindingFlags.Public);
            var d3Trade = d3Type.GetProperty("TradeState", BindingFlags.Instance | BindingFlags.Public);
            var d3Ready = d3Type.GetProperty("IsReady", BindingFlags.Instance | BindingFlags.Public);
            var d4Ready = d4Type.GetProperty("IsReady", BindingFlags.Instance | BindingFlags.Public);
            var d9Ready = d9Type.GetProperty("IsReady", BindingFlags.Instance | BindingFlags.Public);
            var d4Visible = d4Type.GetProperty("HasVisibleContentPage", BindingFlags.Instance | BindingFlags.Public);
            var d4Shipyard = d4Type.GetProperty("ShipyardSystem", BindingFlags.Instance | BindingFlags.Public);
            var d4Close = d4Type.GetMethod("CloseContentPage", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(d29PreparationButton, "D29 缺少港口整备按钮。");
            Assert.IsNotNull(d29ShipyardButton, "D29 缺少船坞按钮。");
            Assert.IsNotNull(d29Status, "D29 缺少状态反馈文本。");
            Assert.IsNotNull(d9Panel, "D9 缺少港口服务面板。");
            Assert.IsNotNull(d9LastMessageField, "D9 缺少结果反馈字段。");
            Assert.IsNotNull(d9Type.GetProperty("HasInputFocus", BindingFlags.Instance | BindingFlags.Public),
                "D9 缺少页面焦点只读接口。");
            Assert.IsNotNull(d3Trade, "D3 缺少 TradeState 只读接口。");
            Assert.IsNotNull(d3Ready, "D3 缺少 IsReady。");
            Assert.IsNotNull(d4Ready, "D4 缺少 IsReady。");
            Assert.IsNotNull(d9Ready, "D9 缺少 IsReady。");
            Assert.IsNotNull(d4Visible, "D4 缺少临时页可见性接口。");
            Assert.IsNotNull(d4Shipyard, "D4 缺少船坞只读接口。");
            Assert.IsNotNull(d4Close, "D4 缺少临时页关闭入口。");
            Assert.IsNotNull(d4ShipyardField, "D4 缺少可恢复的船坞 owner 字段。");
            Assert.IsNotNull(d4FactionsField, "D4 缺少势力 owner 字段。");
            Assert.IsNotNull(primaryInputAllowed, "RuntimeHudCoordinator 缺少 PrimaryInputAllowed。");
            Assert.IsNotNull(restorePrimaryHud, "RuntimeHudCoordinator 缺少 RestorePrimaryHud。");

            float deadline = Time.realtimeSinceStartup + 12f;
            while ((!((bool)d3Ready.GetValue(d3)) || !((bool)d4Ready.GetValue(d4)) ||
                    !((bool)d9Ready.GetValue(d9)) || d9Preparation == null) &&
                   Time.realtimeSinceStartup < deadline)
            {
                d9Preparation = ReadField<D9PortShipPreparationSystem>(d9, d9Type, "_preparation");
                yield return null;
            }

            Assert.IsTrue((bool)d3Ready.GetValue(d3), "D3 owner 没有 ready。");
            Assert.IsTrue((bool)d4Ready.GetValue(d4), "D4 owner 没有 ready。");
            Assert.IsTrue((bool)d9Ready.GetValue(d9), "D9 owner 没有 ready。");
            Assert.IsNotNull(d9Preparation, "D9 整备 owner 没有绑定。");

            var trade = d3Trade.GetValue(d3) as D3TradeState;
            var ship = GameState.CurrentShip;
            Assert.IsNotNull(trade, "D3 TradeState 为空。");
            Assert.IsNotNull(ship, "新档没有 CurrentShip。");
            Assert.IsNotNull(trade.Inventory, "D3 货舱 owner 为空。");
            Assert.IsTrue(ship.HullMax > 10, "测试船体上限不足以覆盖修理路径。");

            // 测试夹具只安排边界，不替 UI/系统结算业务：货舱仍通过 D3InventorySystem
            // 写入，船体/金币则由真实 D9/D3/D4 owner 在按钮调用中修改。
            trade.Inventory.Cargo.Clear();
            trade.Money = 1000;
            GameState.CurrentPlayer.Money = 1000;
            ship.Hull = ship.HullMax - 10;
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId, 1, out string timberError), timberError);
            Assert.IsTrue(ship.State != null && ship.State.ContainsKey("moored") && ship.State["moored"] >= 0.5f,
                "测试前船舶没有处于 D9 靠泊状态。");

            // D29 -> D9 的真实页面打开：页面获得焦点时主 HUD 必须让路。
            d29PreparationButton.onClick.Invoke();
            yield return null;
            var d9HasFocus = d9Type.GetProperty("HasInputFocus", BindingFlags.Instance | BindingFlags.Public);
            Assert.IsTrue((bool)d9HasFocus.GetValue(d9), "D29 港口整备按钮没有打开 D9 页面。");
            Assert.IsTrue(d9Panel.activeSelf, "D9 页面控件未激活。");
            Assert.IsFalse((bool)primaryInputAllowed.GetValue(null), "D9 页面打开后主 HUD 仍占用输入。");

            var repairButton = d9Panel.transform.Find("Repair")?.GetComponent<Button>();
            var closePreparationButton = d9Panel.transform.Find("Close")?.GetComponent<Button>();
            Assert.IsNotNull(repairButton, "D9 页面缺少 Repair 按钮。");
            Assert.IsNotNull(closePreparationButton, "D9 页面缺少 Close 按钮。");

            int moneyBeforeRepair = trade.Money;
            int hullBeforeRepair = ship.Hull;
            int timberBeforeRepair = D3InventorySystem.CountCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId);
            repairButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(hullBeforeRepair + 10, ship.Hull, "真实 Repair 按钮没有只修复 10 点船体。");
            Assert.AreEqual(moneyBeforeRepair - D9PortShipPreparationSystem.RepairMoneyPerPoint * 10,
                trade.Money, "修船成功没有按 D9 owner 费用扣款。");
            Assert.AreEqual(timberBeforeRepair - 1,
                D3InventorySystem.CountCargo(trade.Inventory, D9PortShipPreparationSystem.RepairMaterialId),
                "修船成功没有由 D3InventorySystem 消耗 1 份木材。");
            Assert.That((string)d9LastMessageField.GetValue(d9), Does.Contain("修船完成"),
                "成功反馈没有来自 D9 owner。");
            Assert.IsTrue((bool)d9HasFocus.GetValue(d9), "修船结果不应意外关闭整备页。");

            // 同帧重复点击不能造成第二笔业务变更；当前船体已经完整，第二次应是
            // owner 拒绝而不是 UI 继续显示上一笔成功。
            int moneyAfterRepair = trade.Money;
            int hullAfterRepair = ship.Hull;
            int timberAfterRepair = D3InventorySystem.CountCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId);
            repairButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyAfterRepair, trade.Money, "重复修船不能再次扣款。");
            Assert.AreEqual(hullAfterRepair, ship.Hull, "重复修船不能再次改变船体。");
            Assert.AreEqual(timberAfterRepair,
                D3InventorySystem.CountCargo(trade.Inventory, D9PortShipPreparationSystem.RepairMaterialId),
                "重复修船不能再次消耗材料。");
            Assert.That((string)d9LastMessageField.GetValue(d9), Does.Contain("完整"),
                "重复修船的拒绝结果必须覆盖旧成功提示。");

            // 同一 saveRevision 的重复结算走 D9 领域幂等事实，不依赖截图或 mock。
            trade.Money = 1000;
            GameState.CurrentPlayer.Money = 1000;
            ship.Hull = ship.HullMax - 20;
            trade.Inventory.Cargo.Clear();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId, 2, out timberError), timberError);
            const long duplicateRevision = 917001L;
            var firstRepair = d9Preparation.Repair(trade, ship, PortCatalog.StartingPortId, 10,
                0, duplicateRevision);
            int moneyAfterFirstFact = trade.Money;
            int hullAfterFirstFact = ship.Hull;
            int timberAfterFirstFact = D3InventorySystem.CountCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId);
            var duplicateRepair = d9Preparation.Repair(trade, ship, PortCatalog.StartingPortId, 10,
                0, duplicateRevision);
            Assert.IsTrue(firstRepair.Accepted && !firstRepair.Duplicate, "D9 首次带 revision 修船没有成功。");
            Assert.IsTrue(duplicateRepair.Accepted && duplicateRepair.Duplicate,
                "D9 相同 saveRevision 的重复修船没有返回幂等结果。");
            Assert.AreEqual(moneyAfterFirstFact, trade.Money, "D9 幂等重复没有保持金币不变。");
            Assert.AreEqual(hullAfterFirstFact, ship.Hull, "D9 幂等重复没有保持船体不变。");
            Assert.AreEqual(timberAfterFirstFact,
                D3InventorySystem.CountCargo(trade.Inventory, D9PortShipPreparationSystem.RepairMaterialId),
                "D9 幂等重复没有保持材料不变。");

            // 材料不足：真实 Repair 按钮走同一 owner，失败时钱、船体和材料都保持不变，
            // 且失败反馈不能被先前的成功文案覆盖。
            ship.Hull = ship.HullMax - 10;
            trade.Money = 1000;
            GameState.CurrentPlayer.Money = 1000;
            trade.Inventory.Cargo.Clear();
            int moneyBeforeNoMaterial = trade.Money;
            int hullBeforeNoMaterial = ship.Hull;
            repairButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(moneyBeforeNoMaterial, trade.Money, "材料不足时不能扣金币。");
            Assert.AreEqual(hullBeforeNoMaterial, ship.Hull, "材料不足时不能改变船体。");
            Assert.AreEqual(0, D3InventorySystem.CountCargo(trade.Inventory,
                D9PortShipPreparationSystem.RepairMaterialId), "材料不足夹具没有保持空材料状态。");
            Assert.That((string)d9LastMessageField.GetValue(d9), Does.Contain("木材"),
                "材料不足反馈必须明确说明缺少木材。");
            Assert.IsTrue((bool)d9HasFocus.GetValue(d9), "材料不足拒绝不能留下失效页面焦点。");

            // 取消：使用真实 Close 按钮恢复主 HUD 和键盘输入。
            closePreparationButton.onClick.Invoke();
            yield return null;
            Assert.IsFalse((bool)d9HasFocus.GetValue(d9), "关闭整备页后 owner 仍持有输入焦点。");
            Assert.IsFalse(d9Panel.activeSelf, "关闭整备页后控件仍可见。");
            Assert.IsTrue((bool)primaryInputAllowed.GetValue(null), "关闭整备页后主 HUD 没有恢复输入。");

            // 船坞成功：D29 按钮调用真实 D4 owner，等级和金币变化均来自 D4ShipyardSystem。
            if (GameState.CurrentPlayer.Reputation == null)
                GameState.CurrentPlayer.Reputation = new System.Collections.Generic.Dictionary<string, int>();
            GameState.CurrentPlayer.Reputation["england"] = 10;
            GameState.CurrentPlayer.Money = 2000;
            var factions = d4FactionsField.GetValue(d4) as D4FactionSystem;
            Assert.IsNotNull(factions, "D4 势力 owner 为空。");
            factions.ApplyReputation("england", 10, "Q17 测试准备船坞成功条件");
            var shipyard = d4Shipyard.GetValue(d4) as D4ShipyardSystem;
            Assert.IsNotNull(shipyard, "D4 船坞 owner 为空。");
            int levelBeforeUpgrade = shipyard.Level(D4UpgradeSlot.Hull);
            int moneyBeforeUpgrade = GameState.CurrentPlayer.Money;
            d29ShipyardButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(levelBeforeUpgrade + 1, shipyard.Level(D4UpgradeSlot.Hull),
                "D29 船坞按钮没有只提交一次真实 D4 改装。");
            Assert.Less(GameState.CurrentPlayer.Money, moneyBeforeUpgrade,
                "船坞成功没有由 D4 owner 扣除费用。");
            Assert.That(d29Status.text, Does.Contain("船坞已完成船体改良"),
                "船坞成功反馈没有对应 D4 owner 结果。");
            Assert.IsTrue((bool)d4Visible.GetValue(d4), "船坞成功后 D4 临时页没有保持可见。");
            Assert.IsFalse((bool)primaryInputAllowed.GetValue(null), "船坞页打开后主 HUD 仍占用输入。");
            d4Close.Invoke(d4, null);
            yield return null;
            Assert.IsTrue((bool)primaryInputAllowed.GetValue(null), "关闭船坞页后主 HUD 没有恢复输入。");

            // 船坞拒绝：成功页必须关闭，且 D29 状态必须显示拒绝而不是复用旧成功。
            yield return new WaitForSecondsRealtime(0.40f);
            GameState.CurrentPlayer.Money = 0;
            int levelBeforePoor = shipyard.Level(D4UpgradeSlot.Hull);
            d29ShipyardButton.onClick.Invoke();
            yield return null;
            Assert.AreEqual(levelBeforePoor, shipyard.Level(D4UpgradeSlot.Hull),
                "资金不足的船坞请求不能升级。");
            Assert.That(d29Status.text, Does.Contain("请求未通过"),
                "资金不足的船坞请求没有显示拒绝结果。");
            Assert.IsFalse((bool)d4Visible.GetValue(d4), "船坞拒绝后不能留下空的临时页面。");
            Assert.IsTrue((bool)primaryInputAllowed.GetValue(null), "船坞拒绝后 busy/临时页没有恢复。");

            // 异常恢复：只在测试中把已有 D4 owner 的内部裁决器置空，触发真实 D29
            // catch/recovery 入口；恢复原字段后不污染后续运行时 owner。
            yield return new WaitForSecondsRealtime(0.40f);
            GameState.CurrentPlayer.Money = 2000;
            var originalShipyard = d4ShipyardField.GetValue(d4);
            d4ShipyardField.SetValue(d4, null);
            int moneyBeforeException = GameState.CurrentPlayer.Money;
            try
            {
                d29ShipyardButton.onClick.Invoke();
                yield return null;
                Assert.That(d29Status.text, Does.Contain("请求异常"),
                    "D4 owner 异常没有进入 D29 统一异常反馈。");
                Assert.AreEqual(moneyBeforeException, GameState.CurrentPlayer.Money,
                    "D4 owner 异常不能改变玩家金币。");
                Assert.IsFalse((bool)d4Visible.GetValue(d4), "D4 owner 异常后临时页没有关闭。");
                Assert.IsTrue((bool)primaryInputAllowed.GetValue(null),
                    "D4 owner 异常后 D29 没有恢复主 HUD 输入。");
            }
            finally
            {
                d4ShipyardField.SetValue(d4, originalShipyard);
                restorePrimaryHud.Invoke(null, null);
            }

            Debug.Log("[D29/Q17] preparation_success=true, repair_reject_material=true, " +
                "repair_duplicate_unchanged=true, domain_duplicate_idempotent=true, " +
                "cancel_recovery=true, shipyard_success=true, shipyard_reject=true, " +
                "exception_recovery=true, primary_input_restored=true");
        }

        private static T ReadField<T>(Component owner, Type ownerType, string name)
        {
            var field = ownerType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, ownerType.Name + " 缺少字段 " + name + "。");
            return (T)field.GetValue(owner);
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
    }
}
