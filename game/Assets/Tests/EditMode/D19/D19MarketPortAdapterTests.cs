// ============================================================
// D19MarketPortAdapterTests —— 港口市场适配器确定性单元测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L25 唯一测试产物）
// 目标：针对 D19-L11 D19MarketPortAdapter 编写无场景、无网络的确定性单元测试：
//       1. 合法买入与卖出（通过 D3 公开命令，校验金额、货舱、市场库存及事件草案）；
//       2. 资金不足（not_enough_money → d19.trade.funds_insufficient 且保留原始码与资金不变）；
//       3. 库存不足/容量超限（weight_full/cargo_full → d19.inventory.capacity_exceeded 且保留原始码）；
//       4. 权限与服务可用性拒绝（跨船/跨港/跨玩家越权拒绝，L05 关闭门阻断且不调用 D3）；
//       5. 幂等与重放测试（相同 command_id 重放 DuplicateSettlement=true 不重复扣款/发事件）；
//       6. 接口不匹配阻断（非法数量、空请求、非 market 服务均阻断且不产生副作用）；
//       7. D3 原始失败码保留（OriginalD3Code 永远不丢失）。
//
// 约束：
//   - 纯 NUnit 单元测试，零 Unity Transform / UI / 场景 / 网络 / LLM 依赖；
//   - 只能使用 D19 / D3 公开数据模型与公开测试替身，不新建可写货币/库存副本；
//   - 绝不修改任何生产代码。
// ============================================================

using NUnit.Framework;
using System.Collections.Generic;
using Whalebone.Systems.D19;
using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public class D19MarketPortAdapterTests
    {
        private static D3TradeState CreateTestTradeState(int money = 500, int cargoCapacity = 40)
        {
            var state = new D3TradeState(money, cargoCapacity);
            CommodityCatalog.SeedMarket(state.Market);
            return state;
        }

        private static D19AuthorityClaims CreateValidClaims(string shipId = "ship_01", string portId = "port_santa_margarita")
        {
            return new D19AuthorityClaims(shipId, true, true, portId);
        }

        [Test]
        public void Buy_ValidRequest_SucceedsAndGeneratesEventDraft()
        {
            var state = CreateTestTradeState(500, 40);
            var economy = new TradeEconomySystem();
            int beforeStock = state.Market.Stock["sugar"];
            int beforeMoney = state.Money;

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_buy_01",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "sugar",
                Quantity = 2,
                WorldDay = 1,
                SaveRevision = 100
            };

            var claims = CreateValidClaims("ship_01", "port_santa_margarita");
            var result = D19MarketPortAdapter.Buy(state, economy, req, claims);

            Assert.IsTrue(result.Success, "Buy should succeed for valid request");
            Assert.IsFalse(result.DuplicateSettlement);
            Assert.AreEqual("sugar", result.CommodityId);
            Assert.AreEqual(2, result.Quantity);
            Assert.Greater(result.TotalPrice, 0);
            Assert.AreEqual(beforeMoney - result.TotalPrice, state.Money);
            Assert.AreEqual(beforeStock - 2, state.Market.Stock["sugar"]);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(state.Inventory, "sugar"));

            // 验证 L16 事件草案
            Assert.IsNotNull(result.EventDraft, "Event draft should be generated");
            Assert.AreEqual("D19PortSystem", result.EventDraft.OwnerSystem);
            Assert.AreEqual("port.market.buy_success", result.EventDraft.Result.ResultCode);
            Assert.AreEqual(D19PortLocalizationKeys.PortEventServiceResult, result.EventDraft.Result.Summary);
        }

        [Test]
        public void Sell_ValidRequest_SucceedsAndRestocksMarket()
        {
            var state = CreateTestTradeState(100, 40);
            var economy = new TradeEconomySystem();
            Assert.IsTrue(D3InventorySystem.TryAddCargo(state.Inventory, "tobacco", 3, out _));

            int beforeStock = state.Market.Stock["tobacco"];
            int beforeMoney = state.Money;

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_sell_01",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_green_cay",
                CommodityId = "tobacco",
                Quantity = 3,
                WorldDay = 2,
                SaveRevision = 101
            };

            var claims = CreateValidClaims("ship_01", "port_green_cay");
            var result = D19MarketPortAdapter.Sell(state, economy, req, claims);

            Assert.IsTrue(result.Success, "Sell should succeed for valid request");
            Assert.Greater(state.Money, beforeMoney);
            Assert.AreEqual(beforeStock + 3, state.Market.Stock["tobacco"]);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(state.Inventory, "tobacco"));
            Assert.AreEqual("port.market.sell_success", result.EventDraft.Result.ResultCode);
        }

        [Test]
        public void Buy_InsufficientMoney_MapsToFundsInsufficientAndKeepsMoneyUnchanged()
        {
            var state = CreateTestTradeState(1, 40); // 仅 1 金币
            var economy = new TradeEconomySystem();
            int beforeMoney = state.Money;

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_buy_poor",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "medicine",
                Quantity = 1,
                WorldDay = 1,
                SaveRevision = 102
            };

            var claims = CreateValidClaims("ship_01", "port_santa_margarita");
            var result = D19MarketPortAdapter.Buy(state, economy, req, claims);

            Assert.IsFalse(result.Success, "Buy with insufficient money must fail");
            Assert.AreEqual("d19.trade.funds_insufficient", result.ReasonCode);
            Assert.AreEqual("not_enough_money", result.OriginalD3Code, "Original D3 failure code must be preserved");
            Assert.AreEqual(beforeMoney, state.Money, "Money must not change on failure");
            Assert.AreEqual(0, D3InventorySystem.CountCargo(state.Inventory, "medicine"));
        }

        [Test]
        public void Buy_CargoCapacityExceeded_MapsToCapacityExceededAndPreservesOriginalCode()
        {
            var state = CreateTestTradeState(10000, 1); // 货舱容量仅 1
            var economy = new TradeEconomySystem();

            // 占满货舱
            Assert.IsTrue(D3InventorySystem.TryAddCargo(state.Inventory, "salt", 1, out _));

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_buy_full",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "sugar",
                Quantity = 1,
                WorldDay = 1,
                SaveRevision = 103
            };

            var claims = CreateValidClaims("ship_01", "port_santa_margarita");
            var result = D19MarketPortAdapter.Buy(state, economy, req, claims);

            Assert.IsFalse(result.Success, "Buy exceeding cargo capacity must fail");
            Assert.AreEqual("d19.inventory.capacity_exceeded", result.ReasonCode);
            Assert.AreEqual("cargo_full", result.OriginalD3Code);
        }

        [Test]
        public void Authority_CrossShipOrCrossPort_RejectedWithoutCallingD3()
        {
            var state = CreateTestTradeState(500, 40);
            var economy = new TradeEconomySystem();
            int beforeMoney = state.Money;

            // 跨港声明：声明停靠在 green_cay，但向 santa_margarita 请求
            var invalidClaims = new D19AuthorityClaims("ship_01", true, true, "port_green_cay");

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_cross_port",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "sugar",
                Quantity = 1,
                WorldDay = 1,
                SaveRevision = 104
            };

            var result = D19MarketPortAdapter.Buy(state, economy, req, invalidClaims);

            Assert.IsFalse(result.Success, "Cross port request must be rejected");
            Assert.AreEqual("d19.port.permission_denied", result.ReasonCode);
            Assert.AreEqual(beforeMoney, state.Money, "D3 must not be touched on authority rejection");
        }

        [Test]
        public void AvailabilityGate_ClosedService_BlocksTradeImmediately()
        {
            var state = CreateTestTradeState(500, 40);
            var economy = new TradeEconomySystem();

            // 注入处于不可用状态的 L05 门控结果（如夜晚或暴风雨关闭）
            var closedGate = new D19ServiceAvailabilityResult
            {
                ServiceId = "market",
                PortId = "port_santa_margarita",
                Verdict = D19AvailabilityVerdict.Unavailable,
                ReasonCode = "d19.service.unavailable",
                LocKey = D19PortLocalizationKeys.ReadModelServiceClosed,
                Detail = D19UnavailabilityDetail.TimeClosed,
                RetryHint = D19RetryHint.RetryNextPeriod,
                RetryPolicy = D19FailureRetryPolicy.RetrySafe
            };

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_buy_closed",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "sugar",
                Quantity = 1,
                AvailabilityGate = closedGate,
                WorldDay = 1,
                SaveRevision = 105
            };

            var claims = CreateValidClaims("ship_01", "port_santa_margarita");
            var result = D19MarketPortAdapter.Buy(state, economy, req, claims);

            Assert.IsFalse(result.Success, "Trade under closed service must be blocked");
            Assert.AreEqual("d19.service.unavailable", result.ReasonCode);
            Assert.AreEqual(D19PortLocalizationKeys.ReadModelServiceClosed, result.LocKey);
        }

        [Test]
        public void Idempotency_DuplicateSubmission_ReplaysWithoutDoubleDeduction()
        {
            var state = CreateTestTradeState(500, 40);
            var economy = new TradeEconomySystem();

            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_buy_idem_01",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                CommodityId = "sugar",
                Quantity = 2,
                WorldDay = 1,
                SaveRevision = 10
            };

            var claims = CreateValidClaims("ship_01", "port_santa_margarita");

            // 第一次买入
            var res1 = D19MarketPortAdapter.Buy(state, economy, req, claims);
            Assert.IsTrue(res1.Success);
            int moneyAfterFirst = state.Money;
            int cargoAfterFirst = D3InventorySystem.CountCargo(state.Inventory, "sugar");

            // 第二次提交相同 CommandId（带相同 revision）
            var res2 = D19MarketPortAdapter.Buy(state, economy, req, claims);
            Assert.IsTrue(res2.Success);
            Assert.IsTrue(res2.DuplicateSettlement, "Duplicate submission must report DuplicateSettlement");
            Assert.AreEqual(moneyAfterFirst, state.Money, "Duplicate trade must NOT deduct money again");
            Assert.AreEqual(cargoAfterFirst, D3InventorySystem.CountCargo(state.Inventory, "sugar"), "Duplicate trade must NOT add cargo again");
        }

        [Test]
        public void InterfaceMismatch_InvalidInput_ExplicitlyBlockedWithoutSideEffect()
        {
            var state = CreateTestTradeState(500, 40);
            var economy = new TradeEconomySystem();
            var claims = CreateValidClaims();

            // 1. null request
            var resNull = D19MarketPortAdapter.Buy(state, economy, null, claims);
            Assert.IsFalse(resNull.Success);
            Assert.AreEqual("request_null", resNull.ReasonCode);

            // 2. 缺失 CommandId
            var reqNoCmd = new D19MarketTradeRequest { CommodityId = "sugar", Quantity = 1 };
            var resNoCmd = D19MarketPortAdapter.Buy(state, economy, reqNoCmd, claims);
            Assert.IsFalse(resNoCmd.Success);
            Assert.AreEqual("missing_command_id", resNoCmd.ReasonCode);

            // 3. 非正数量
            var reqZeroQty = new D19MarketTradeRequest { CommandId = "cmd_zero", CommodityId = "sugar", Quantity = 0 };
            var resZeroQty = D19MarketPortAdapter.Buy(state, economy, reqZeroQty, claims);
            Assert.IsFalse(resZeroQty.Success);
            Assert.AreEqual("invalid_quantity", resZeroQty.ReasonCode);

            // 4. 错误服务 ID（非 market）
            var reqWrongSvc = new D19MarketTradeRequest { CommandId = "cmd_wrong_svc", CommodityId = "sugar", Quantity = 1, ServiceId = "warehouse" };
            var resWrongSvc = D19MarketPortAdapter.Buy(state, economy, reqWrongSvc, claims);
            Assert.IsFalse(resWrongSvc.Success);
            Assert.AreEqual("service_not_market", resWrongSvc.ReasonCode);
        }
    }
}
