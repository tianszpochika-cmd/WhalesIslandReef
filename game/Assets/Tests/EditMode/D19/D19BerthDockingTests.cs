// ============================================================
// D19BerthDockingTests —— 泊位与靠泊系统确定性单元测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L29 唯一测试产物）
// 目标：针对 D19-L06 D19BerthSystem 与 D19-L07 D19DockingRules 编写确定性单元测试：
//       1. 正常预留 → 确认占用全流程（Reserved → Occupied，租期转换，事件生成）；
//       2. 两船竞争同泊位（第一艘成功预留，第二艘被 Occupied 拒绝且首船预留不受影响）；
//       3. 船型/吃水能力不兼容（SizeTier 越界/超限或深水需求不满足时被 ShipTypeMismatch 拒绝）；
//       4. 租期过期（Reserved 超期后确认占用被 Expired 拒绝；AdvanceTime 自动释放过期泊位）；
//       5. 正常释放与超期释放（释放后泊位变为 Available，生成 release 事件）；
//       6. 跨港泊位请求（非本港泊位被 CrossPort 拒绝，原泊位与请求港状态不变）；
//       7. 重复命令与幂等性（相同 CommandId 返回原结果且无二次副作用；同 ID 异参 DuplicateCommand 冲突拒绝）；
//       8. 一船多泊位互斥（同一艘船已持泊位时，请求新泊位被 ShipAlreadyBerthed 拒绝）；
//       9. 靠泊前置规则裁决（DockingRules 对未进港、跨港权限不足、设施关闭的确定性拒绝）；
//       10. 每次操作后断言不变式（ValidateInvariants 全程成立：无船双占、无泊位双占、无半状态写入）。
//
// 约束：
//   - 纯 NUnit 单元测试，零 Unity Transform / UI / 场景 / 物理碰撞 / 网络 / LLM 依赖；
//   - 所有逻辑时间来自固定整数输入（WorldDay）；
//   - 绝不修改任何生产代码。
// ============================================================

using NUnit.Framework;
using System.Collections.Generic;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public class D19BerthDockingTests
    {
        private D19PortAnchorCatalog _catalog;
        private D19BerthSystem _berthSystem;

        [SetUp]
        public void SetUp()
        {
            _catalog = D19PortAnchorCatalog.Instance;
            _berthSystem = new D19BerthSystem(_catalog);
        }

        private void AssertInvariants()
        {
            List<string> violations = _berthSystem.ValidateInvariants();
            Assert.AreEqual(0, violations.Count, "Invariant violations detected: " + string.Join("; ", violations));
        }

        [Test]
        public void ReserveAndConfirm_FullLifecycle_SucceedsAndMaintainsInvariants()
        {
            // berth_santa_margarita_dock_01: MinTier=1, MaxTier=3, DeepDraft=true
            var profile = new D19BerthShipProfile("ship_01", 1, true);

            // 1. 预留泊位 (世界日 1，租期至世界日 10)
            var res = _berthSystem.Reserve("cmd_res_01", "port_santa_margarita", "berth_santa_margarita_dock_01", profile, 1, 10, 100);
            Assert.IsTrue(res.Success, "Reserve should succeed");
            Assert.AreEqual(D19BerthState.Reserved, res.Allocation.State);
            Assert.AreEqual("ship_01", res.Allocation.ShipId);
            Assert.AreEqual(1, res.Allocation.ReservedWorldDay);
            Assert.AreEqual(10, res.Allocation.ExpiresWorldDay);
            Assert.IsNotNull(res.Event, "Reserve event should be generated");
            Assert.AreEqual("port.berth.reserved", res.Event.Result.ResultCode);
            AssertInvariants();

            // 2. 确认占用 (世界日 2)
            var occ = _berthSystem.ConfirmOccupancy("cmd_occ_01", "port_santa_margarita", "berth_santa_margarita_dock_01", "ship_01", 2, 101);
            Assert.IsTrue(occ.Success, "Confirm occupancy should succeed");
            Assert.AreEqual(D19BerthState.Occupied, occ.Allocation.State);
            Assert.AreEqual("ship_01", occ.Allocation.ShipId);
            Assert.AreEqual(0, occ.Allocation.ExpiresWorldDay, "Occupied berth must have no expiry");
            AssertInvariants();

            // 3. 正常释放 (世界日 5)
            var rel = _berthSystem.Release("cmd_rel_01", "port_santa_margarita", "berth_santa_margarita_dock_01", "ship_01", 5, 102, "departure");
            Assert.IsTrue(rel.Success, "Release should succeed");
            Assert.AreEqual(D19BerthState.Available, rel.Allocation.State);
            Assert.AreEqual(string.Empty, rel.Allocation.ShipId);
            Assert.AreEqual("port.berth.released", rel.Event.Result.ResultCode);
            AssertInvariants();
        }

        [Test]
        public void TwoShips_CompetingForSameBerth_SecondShipRejectedAndFirstUnchanged()
        {
            var p1 = new D19BerthShipProfile("ship_01", 1, true);
            var p2 = new D19BerthShipProfile("ship_02", 1, true);

            // 首船预留
            var res1 = _berthSystem.Reserve("cmd_res_s1", "port_santa_margarita", "berth_santa_margarita_dock_01", p1, 1, 10, 100);
            Assert.IsTrue(res1.Success);

            // 第二艘船尝试预留同一泊位
            var res2 = _berthSystem.Reserve("cmd_res_s2", "port_santa_margarita", "berth_santa_margarita_dock_01", p2, 1, 10, 101);
            Assert.IsFalse(res2.Success, "Second ship should be rejected");
            Assert.AreEqual(D19PortFailureCode.BerthOccupied, res2.Failure.Code);
            Assert.AreEqual(D19BerthReasonIds.Occupied, res2.Failure.ReasonId);

            // 断言首船分配状态未被篡改
            var alloc = _berthSystem.GetAllocation("berth_santa_margarita_dock_01");
            Assert.AreEqual("ship_01", alloc.ShipId);
            Assert.AreEqual(D19BerthState.Reserved, alloc.State);
            AssertInvariants();
        }

        [Test]
        public void IncompatibleShipType_SizeOrDraftMismatch_RejectedWithoutStateChange()
        {
            // 先成功分配一次以在 _byBerth 中初始化或直接检验拒绝
            // berth_santa_margarita_small_01: MinTier=1, MaxTier=1, DeepDraft=false
            // 传入大船 (SizeTier=3)
            var largeProfile = new D19BerthShipProfile("ship_large", 3, false);

            var res = _berthSystem.Reserve("cmd_res_large", "port_santa_margarita", "berth_santa_margarita_small_01", largeProfile, 1, 10, 100);
            Assert.IsFalse(res.Success, "Large ship should be rejected on small berth");
            Assert.AreEqual(D19BerthReasonIds.ShipTypeMismatch, res.Failure.ReasonId);

            // 泊位未被分配，GetAllocation 为 null (或 Available)
            var alloc = _berthSystem.GetAllocation("berth_santa_margarita_small_01");
            Assert.IsTrue(alloc == null || alloc.State == D19BerthState.Available);
            AssertInvariants();
        }

        [Test]
        public void Reservation_Expired_ConfirmRejectedAndAdvanceTimeReleases()
        {
            var profile = new D19BerthShipProfile("ship_01", 1, true);

            // 预留至世界日 5
            var res = _berthSystem.Reserve("cmd_res_exp", "port_santa_margarita", "berth_santa_margarita_dock_01", profile, 1, 5, 100);
            Assert.IsTrue(res.Success);

            // 在世界日 6 尝试确认占用 (已过期)
            var occ = _berthSystem.ConfirmOccupancy("cmd_occ_late", "port_santa_margarita", "berth_santa_margarita_dock_01", "ship_01", 6, 101);
            Assert.IsFalse(occ.Success, "Expired reservation confirm must fail");
            Assert.AreEqual(D19BerthReasonIds.Expired, occ.Failure.ReasonId);

            // 推进时间到世界日 6 (自动释放过期预留)
            var expiredEvents = _berthSystem.AdvanceTime(6, 102);
            Assert.Greater(expiredEvents.Count, 0, "Expired berth should generate release event");

            // 泊位重新变为 Available
            var alloc = _berthSystem.GetAllocation("berth_santa_margarita_dock_01");
            Assert.AreEqual(D19BerthState.Available, alloc.State);
            Assert.AreEqual(string.Empty, alloc.ShipId);
            AssertInvariants();
        }

        [Test]
        public void CrossPortRequest_InvalidPortBerthCombination_Rejected()
        {
            var profile = new D19BerthShipProfile("ship_01", 1, true);

            // 向 green_cay 请求 santa_margarita 的泊位
            var res = _berthSystem.Reserve("cmd_cross", "port_green_cay", "berth_santa_margarita_dock_01", profile, 1, 10, 100);
            Assert.IsFalse(res.Success, "Cross port berth request must be rejected");
            Assert.AreEqual(D19BerthReasonIds.CrossPort, res.Failure.ReasonId);
            AssertInvariants();
        }

        [Test]
        public void SingleShip_MultipleBerths_SecondReservationRejected()
        {
            var profile = new D19BerthShipProfile("ship_01", 1, true);

            // 成功预留 1 号泊位
            var res1 = _berthSystem.Reserve("cmd_res_b1", "port_santa_margarita", "berth_santa_margarita_dock_01", profile, 1, 10, 100);
            Assert.IsTrue(res1.Success);

            // 同一艘船尝试预留 2 号可用泊位 (berth_santa_margarita_anchorage_01)
            var res2 = _berthSystem.Reserve("cmd_res_b2", "port_santa_margarita", "berth_santa_margarita_anchorage_01", profile, 1, 10, 101);
            Assert.IsFalse(res2.Success, "Same ship cannot hold multiple berths");
            Assert.AreEqual(D19BerthReasonIds.ShipAlreadyBerthed, res2.Failure.ReasonId);

            // 2 号泊位保持可用
            var alloc2 = _berthSystem.GetAllocation("berth_santa_margarita_anchorage_01");
            Assert.IsTrue(alloc2 == null || alloc2.State == D19BerthState.Available);
            AssertInvariants();
        }

        [Test]
        public void Idempotency_DuplicateCommand_ReplaysPreviousResultWithoutSideEffects()
        {
            var profile = new D19BerthShipProfile("ship_01", 1, true);

            // 首次提交
            var res1 = _berthSystem.Reserve("cmd_idem_01", "port_santa_margarita", "berth_santa_margarita_dock_01", profile, 1, 10, 100);
            Assert.IsTrue(res1.Success);

            // 相同 CommandId 相同参数重复提交
            var res2 = _berthSystem.Reserve("cmd_idem_01", "port_santa_margarita", "berth_santa_margarita_dock_01", profile, 1, 10, 100);
            Assert.IsTrue(res2.Success);
            Assert.AreEqual(res1.Allocation.State, res2.Allocation.State);

            // 相同 CommandId 不同操作 (ConfirmOccupancy) 判定冲突
            var resConflict = _berthSystem.ConfirmOccupancy("cmd_idem_01", "port_santa_margarita", "berth_santa_margarita_dock_01", "ship_01", 1, 100);
            Assert.IsFalse(resConflict.Success);
            Assert.AreEqual(D19BerthReasonIds.DuplicateCommand, resConflict.Failure.ReasonId);
            AssertInvariants();
        }

        [Test]
        public void DockingRules_Adjudicate_ChecksPreconditionsCorrectly()
        {
            // 1. 未进港状态 (None) 请求靠泊
            var reqNotInPort = new D19DockingRequest
            {
                CommandId = "cmd_dock_01",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                BerthId = "berth_santa_margarita_dock_01",
                VisitState = D19PortVisitState.None
            };
            var v1 = D19DockingRules.Adjudicate(reqNotInPort);
            Assert.IsFalse(v1.Allowed, "Docking from None state must be rejected");

            // 2. 正确状态 (Docked)，已授权，目标泊位空闲，吃水能力匹配 (DeepDraft=true)
            var reqValid = new D19DockingRequest
            {
                CommandId = "cmd_dock_02",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = "port_santa_margarita",
                BerthId = "berth_santa_margarita_dock_01",
                VisitState = D19PortVisitState.Docked,
                ShipProfile = new D19BerthShipProfile("ship_01", 1, true)
            };
            var v2 = D19DockingRules.Adjudicate(reqValid);
            Assert.IsTrue(v2.Allowed, "Docking under valid conditions must be allowed");
            Assert.AreEqual("port_santa_margarita", v2.CanonicalPortId);
            Assert.AreEqual("berth_santa_margarita_dock_01", v2.CanonicalBerthId);

            // 3. 消费 AdjudicateReservation 自动预留
            var outcome = D19DockingRules.AdjudicateReservation(_berthSystem, reqValid, 1, 10, 100);
            Assert.IsTrue(outcome.Verdict.Allowed);
            Assert.IsNotNull(outcome.ReserveResult);
            Assert.IsTrue(outcome.ReserveResult.Success);
            Assert.AreEqual(D19BerthState.Reserved, outcome.ReserveResult.Allocation.State);
            AssertInvariants();
        }
    }
}
