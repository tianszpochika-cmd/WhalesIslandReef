// ============================================================
// D19ArrivalReturnTests —— 返港与再靠泊确定性测试（D19-L31）
//
// 归属：Whalebone.Tests.EditMode.D19（所有者：D19-L31 测试套件）
// 目标覆盖：
//   1. 合法返港（Legal Arrival / Return）：在航（Departed）状态成功返港并分配泊位，推进至 Berthed/Returned，产生航程结束意图与 L16 事件草案；
//   2. 无空泊位返港（No Target Berth / Direct Return）：不指定泊位返港，成功推进至 Returned，无泊位分配，产出航程结束意图；
//   3. 泊位规则拒绝（Docking Rules Rejected / 船型不匹配或已占用）：目标泊位被占用或吃水/尺寸不兼容时拒绝，在航状态不变，绝不产生半写；
//   4. 非法前态拒绝（Invalid Prestate）：非在航前态（如 InService/Boarded）拒绝返港，绝不把在航状态伪写为返港；
//   5. 航程结束意图生成（Voyage End Intent）：生成纯数据意图，供 D3 结算消费，不越权直接修改货币/库存/船员；
//   6. 重复返港命令幂等控制（Idempotency Replay）：同命令同参重放返回相同结果，绝无第二次占泊/事件副作用；同命令异参冲突拒绝；
//   7. 目标港服务状态恢复（Service Recovery）：返港成功后稳定恢复目标港口可用服务列表（按字典序稳定排序）；
//   8. 中断与状态机拒绝补偿（Compensation Protection）：状态机拒绝推进时自动触发泊位释放与补偿事件草案（含 cause_ids），绝无孤儿泊位或双重航程状态。
//
// 铁律与约束：
//   * 纯确定性测试，零 GameObject、零 Transform、零 Canvas、零 UI、零网络/真实时钟/NavMesh/Player；
//   * 无孤儿泊位、无双重航程状态、失败不会把在航状态伪写为返港；
//   * 事件与失败 key/code 可确定性复现与重放；
//   * 只测试不改生产文件，不修改 Save 根模型。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.EditMode.D19
{
    [TestFixture]
    public sealed class D19ArrivalReturnTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string BerthSantaMargaritaDock01 = "berth_santa_margarita_dock_01";
        private const string BerthSantaMargaritaSmall01 = "berth_santa_margarita_small_01";
        private const string ShipAlpha = "ship_alpha_01";
        private const string ShipBeta = "ship_beta_02";
        private const string ActorPlayer = "player_actor_01";

        private D19PortAnchorCatalog _catalog;
        private D19ArrivalWorkflow _arrivalWorkflow;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _catalog = D19PortAnchorCatalog.Instance;
            _arrivalWorkflow = new D19ArrivalWorkflow(_catalog);
            _validClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );
        }

        // ============================================================
        // 1. 合法返港与泊位分配
        // ============================================================

        [Test]
        public void Arrival_Success_FromDeparted_AllocatesBerth_MovesToBerthed_GeneratesVoyageEndIntent()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            var visitSystem = new D19PortVisitSystem(ShipAlpha);

            // 初始化 visitSystem 到 Departed 状态
            visitSystem.Execute(new D19PortCommand("c_enter", D19PortCommandKind.EnterPort, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_dock", D19PortCommandKind.Dock, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_board", D19PortCommandKind.Board, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_depart", D19PortCommandKind.Depart, PortSantaMargarita), 1, 1);
            Assert.AreEqual(D19PortVisitState.Departed, visitSystem.State);

            // dock_01 要求 deepDraft: true
            var shipProfile = new D19BerthShipProfile(ShipAlpha, sizeTier: 1, deepDraft: true);

            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_001",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaDock01,
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: shipProfile,
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims,
                ConfirmOccupancy = true
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request, berthSystem, visitSystem);

            Assert.IsTrue(result.Success, "返港流程应成功");
            Assert.AreEqual(D19PortVisitState.Berthed, result.StateAfter, "分配泊位后应迁移至 Berthed");
            Assert.IsTrue(result.BerthAllocated);
            Assert.IsTrue(result.BerthConfirmed);
            Assert.AreEqual(BerthSantaMargaritaDock01, result.AllocatedBerthId);

            // 验证泊位状态为 Occupied
            Assert.AreEqual(D19BerthState.Occupied, berthSystem.GetAllocation(BerthSantaMargaritaDock01).State);

            // 验证航程结束意图
            Assert.IsNotNull(result.VoyageEndIntent);
            Assert.IsTrue(result.VoyageEndIntent.Executable);
            Assert.IsTrue(result.VoyageEndIntent.SettlementReady);
            Assert.AreEqual(PortSantaMargarita, result.VoyageEndIntent.PortId);

            // 验证服务恢复列表非空且按字典序排序
            Assert.Greater(result.AvailableServices.Count, 0);
            for (int i = 1; i < result.AvailableServices.Count; i++)
            {
                Assert.LessOrEqual(string.Compare(result.AvailableServices[i - 1], result.AvailableServices[i], StringComparison.Ordinal), 0);
            }
        }

        // ============================================================
        // 2. 无空泊位返港（直接到港，无泊位分配）
        // ============================================================

        [Test]
        public void Arrival_Success_NoTargetBerth_MovesToReturned_NoBerthAllocated()
        {
            var visitSystem = new D19PortVisitSystem(ShipAlpha);
            visitSystem.Execute(new D19PortCommand("c_enter", D19PortCommandKind.EnterPort, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_dock", D19PortCommandKind.Dock, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_board", D19PortCommandKind.Board, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_depart", D19PortCommandKind.Depart, PortSantaMargarita), 1, 1);

            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_no_berth",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: string.Empty, // 无目标泊位
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: new D19BerthShipProfile(ShipAlpha, 1, true),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request, null, visitSystem);

            Assert.IsTrue(result.Success, "无泊位返港应成功进入 Returned");
            Assert.AreEqual(D19PortVisitState.Returned, result.StateAfter);
            Assert.IsFalse(result.BerthAllocated);
            Assert.AreEqual(string.Empty, result.AllocatedBerthId);
            Assert.IsNotNull(result.VoyageEndIntent);
            Assert.IsTrue(result.VoyageEndIntent.SettlementReady);
        }

        // ============================================================
        // 3. 泊位规则拒绝（已被他人占用 / 船型不兼容）
        // ============================================================

        [Test]
        public void Arrival_BerthOccupiedByOther_Rejects_KeepsDepartedState_NoSideEffect()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            // 另一艘船已占用该泊位 (dock_01)
            berthSystem.Reserve("cmd_res_other", PortSantaMargarita, BerthSantaMargaritaDock01, new D19BerthShipProfile(ShipBeta, 1, true), untilWorldDay: 10, currentWorldDay: 1, saveRevision: 1);
            berthSystem.ConfirmOccupancy("cmd_occ_other", PortSantaMargarita, BerthSantaMargaritaDock01, ShipBeta, currentWorldDay: 1, saveRevision: 1);

            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_occupied",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaDock01, // 已被占用
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: new D19BerthShipProfile(ShipAlpha, 1, true),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request, berthSystem);

            Assert.IsFalse(result.Success, "泊位已被占用应拒绝返港靠泊");
            Assert.AreEqual(D19PortVisitState.Departed, result.StateAfter, "状态绝不可伪写为返港");
            Assert.IsFalse(result.BerthAllocated);
            Assert.AreEqual(D19PortFailureCode.BerthOccupied, result.Failure.Code);
            Assert.AreEqual(ShipBeta, berthSystem.GetAllocation(BerthSantaMargaritaDock01).ShipId, "原占用者保持不变");
        }

        [Test]
        public void Arrival_ShipTierIncompatible_Rejects_NoBerthAllocation()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            // small_01 的尺寸上限是 1，尝试以 tier 3 的船进入
            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_incompatible",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaSmall01,
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: new D19BerthShipProfile(ShipAlpha, sizeTier: 3, deepDraft: false),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request, berthSystem);

            Assert.IsFalse(result.Success, "船型尺寸不匹配应拒绝");
            Assert.AreEqual(D19PortVisitState.Departed, result.StateAfter);
            Assert.IsFalse(result.BerthAllocated);
        }

        // ============================================================
        // 4. 非法前态拒绝
        // ============================================================

        [Test]
        public void Arrival_InvalidPrestate_BoardedOrInService_Rejects()
        {
            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_invalid_prestate",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaDock01,
                currentVisitState: D19PortVisitState.InService, // 还在港口服务中不能执行返港
                voyageStatus: D3VoyageStatus.InPort,
                shipProfile: new D19BerthShipProfile(ShipAlpha, 1, true),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request);

            Assert.IsFalse(result.Success, "非法前态应拒绝");
            Assert.AreEqual(D19PortVisitState.InService, result.StateAfter, "状态必须保持原态");
            Assert.AreEqual(D19PortFailureCode.InvalidTransition, result.Failure.Code);
        }

        // ============================================================
        // 5. 重复返港命令幂等控制
        // ============================================================

        [Test]
        public void Arrival_DuplicateCommand_SameParams_ReplaysResultWithoutSecondSideEffect()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_idem_01",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaDock01,
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: new D19BerthShipProfile(ShipAlpha, 1, true),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims,
                ConfirmOccupancy = true
            };

            // 第一次执行
            D19ArrivalWorkflowResult res1 = _arrivalWorkflow.Execute(request, berthSystem);
            Assert.IsTrue(res1.Success);
            Assert.IsFalse(res1.DuplicateSubmission);

            // 第二次执行（相同 CommandId，相同参数）
            D19ArrivalWorkflowResult res2 = _arrivalWorkflow.Execute(request, berthSystem);
            Assert.IsTrue(res2.Success);
            Assert.IsTrue(res2.DuplicateSubmission);
            Assert.AreEqual(res1.StateAfter, res2.StateAfter);
            Assert.AreEqual(res1.AllocatedBerthId, res2.AllocatedBerthId);
            Assert.AreEqual(0, res2.Events.Count, "幂等重放绝无第二次事件副作用");
        }

        // ============================================================
        // 6. 中断与状态机拒绝自动补偿（消除孤儿泊位）
        // ============================================================

        [Test]
        public void Arrival_VisitSystemRejects_TriggersCompensation_ReleasesBerth_NoOrphanBerth()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            var visitSystem = new D19PortVisitSystem(ShipAlpha);
            // 故意让 visitSystem 处于 None 状态（从 None 执行 Return 命令会被状态机拦截）
            // 这样在 arrival workflow 预留泊位成功后推进 visitSystem 时会失败，触发补偿

            var request = new D19ArrivalWorkflowRequest(
                commandId: "cmd_arrive_comp_test",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                targetBerthId: BerthSantaMargaritaDock01,
                currentVisitState: D19PortVisitState.Departed,
                voyageStatus: D3VoyageStatus.Sailing,
                shipProfile: new D19BerthShipProfile(ShipAlpha, 1, true),
                worldDay: 5,
                saveRevision: 50
            )
            {
                AuthorityClaims = _validClaims,
                ConfirmOccupancy = true
            };

            D19ArrivalWorkflowResult result = _arrivalWorkflow.Execute(request, berthSystem, visitSystem);

            Assert.IsFalse(result.Success, "底层状态机拒绝返港");
            Assert.IsTrue(result.Compensated, "必须标记已执行补偿");
            Assert.AreEqual(D19PortVisitState.Departed, result.StateAfter, "状态回滚为原在航状态");
            Assert.IsFalse(result.BerthAllocated, "泊位分配标志置 false");

            // 关键断言：泊位必须已被补偿释放回 Available，绝无孤儿泊位
            Assert.AreEqual(D19BerthState.Available, berthSystem.GetAllocation(BerthSantaMargaritaDock01).State, "泊位必须释放回 Available，消除孤儿占用");

            // 产生补偿事件草案
            Assert.AreEqual(1, result.Events.Count);
            WorldEvent compEvent = result.Events[0];
            Assert.AreEqual("port.compensation.applied", compEvent.Result.ResultCode);
            Assert.IsTrue(compEvent.CauseIds.Count > 0, "补偿事件必须引用前因事件 ID");
        }
    }
}
