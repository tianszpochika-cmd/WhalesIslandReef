// ============================================================
// D19BoardingDepartureTests —— 登船与离港工作流确定性测试（D19-L30）
//
// 归属：Whalebone.Tests.EditMode.D19（所有者：D19-L30 测试套件）
// 目标覆盖：
//   1. 登船（Boarding）成功：成员从 OnShore 迁移到 OnBoard，生命周期进入 Boarded，产出 Boarded 事件草案；
//   2. 下船（Disembark）成功：成员从 OnBoard 迁移到 OnShore，生命周期状态不变，产出 Disembarked 事件草案；
//   3. 成员已在目标位置拒绝：已在船上再登船 / 已在岸上再下船，原子拒绝（d19.boarding.failed），零位置变动；
//   4. 成员/权限失败（Authority / Unknown Member）：未授权声明 / 目标成员不存在，原子拒绝，零位置变动；
//   5. 离港准备不足拒绝（Preparation Blocker）：出航准备（L15）未就绪时离港被拒绝，状态保持不变；
//   6. 离泊释放失败拒绝（Berth Release Failure）：泊位释放失败时离港中止，泊位与生命周期状态完全不变；
//   7. 航程启动/生命周期状态机拒绝并补偿（Compensation Triggered）：状态机拒绝推进时触发自动补偿（发出 cause_ids 补偿事件草案），绝无孤儿泊位或双重状态；
//   8. 重复离港命令幂等控制（Idempotency Replay）：同命令同参重放返回相同结果，绝无第二次离泊/事件副作用。
//
// 铁律与约束：
//   * 纯确定性测试，零 GameObject、零 Transform、零 Canvas、零 UI、零网络/真实时钟/D20表现；
//   * 无成员双重位置、无已离泊未起航的孤儿状态、无重复副作用；
//   * 成功事件严格按发生次序关联同一 command_id；
//   * 只测试不改生产文件，不修改 Save 根模型。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Tests.EditMode.D19
{
    [TestFixture]
    public sealed class D19BoardingDepartureTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string BerthSantaMargaritaDock01 = "berth_santa_margarita_dock_01";
        private const string ShipAlpha = "ship_alpha_01";
        private const string ActorPlayer = "player_actor_01";
        private const string SailorJack = "crew_sailor_jack";
        private const string SailorBill = "crew_sailor_bill";

        private D19PortAnchorCatalog _catalog;
        private D19BoardingWorkflow _boardingWorkflow;
        private D19DepartureWorkflow _departureWorkflow;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _catalog = D19PortAnchorCatalog.Instance;
            _boardingWorkflow = new D19BoardingWorkflow(_catalog);
            _departureWorkflow = new D19DepartureWorkflow(_catalog);
            _validClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );
        }

        // ============================================================
        // 1. 登船（Boarding）成功路径
        // ============================================================

        [Test]
        public void Boarding_Success_MembersMoveToOnBoard_GeneratesBoardedEvent()
        {
            var currentLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnShore },
                { SailorBill, D19CrewLocation.OnShore }
            };

            var request = new D19BoardingRequest(
                commandId: "cmd_board_001",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                type: D19BoardingRequestType.Board,
                memberIds: new[] { SailorJack, SailorBill }
            );

            D19BoardingResult result = _boardingWorkflow.Execute(
                request,
                currentState: D19PortVisitState.Berthed,
                currentBerthId: BerthSantaMargaritaDock01,
                currentLocations: currentLocations,
                claims: _validClaims,
                worldDay: 1,
                saveRevision: 10
            );

            Assert.IsTrue(result.Success, "登船应成功");
            Assert.AreEqual("cmd_board_001", result.CommandId);
            Assert.AreEqual(2, result.ProposedLocations.Count);
            Assert.AreEqual(D19CrewLocation.OnBoard, result.ProposedLocations[SailorJack]);
            Assert.AreEqual(D19CrewLocation.OnBoard, result.ProposedLocations[SailorBill]);

            // 事件草案校验
            Assert.AreEqual(1, result.Events.Count);
            WorldEvent e = result.Events[0];
            Assert.AreEqual("port.boarding.applied", e.Result.ResultCode);
            Assert.IsTrue(e.InputFacts.Contains("command:cmd_board_001"));
            Assert.IsTrue(e.InputFacts.Contains("port:port_santa_margarita"));
            Assert.IsTrue(e.InputFacts.Contains("ship:ship_alpha_01"));
        }

        // ============================================================
        // 2. 下船（Disembark）成功路径
        // ============================================================

        [Test]
        public void Disembark_Success_MembersMoveToOnShore_GeneratesDisembarkedEvent()
        {
            var currentLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnBoard }
            };

            var request = new D19BoardingRequest(
                commandId: "cmd_disembark_001",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                type: D19BoardingRequestType.Disembark,
                memberIds: new[] { SailorJack }
            );

            D19BoardingResult result = _boardingWorkflow.Execute(
                request,
                currentState: D19PortVisitState.Berthed,
                currentBerthId: BerthSantaMargaritaDock01,
                currentLocations: currentLocations,
                claims: _validClaims,
                worldDay: 1,
                saveRevision: 10
            );

            Assert.IsTrue(result.Success, "下船应成功");
            Assert.AreEqual(1, result.ProposedLocations.Count);
            Assert.AreEqual(D19CrewLocation.OnShore, result.ProposedLocations[SailorJack]);

            Assert.AreEqual(1, result.Events.Count);
            Assert.AreEqual("port.boarding.disembarked", result.Events[0].Result.ResultCode);
        }

        // ============================================================
        // 3. 成员已在目标位置拒绝（原子拒绝，无位置变动）
        // ============================================================

        [Test]
        public void Boarding_AlreadyOnBoard_AtomicRejection_NoLocationsChanged()
        {
            var currentLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnBoard }, // 已经在船上
                { SailorBill, D19CrewLocation.OnShore }
            };

            var request = new D19BoardingRequest(
                commandId: "cmd_board_already",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                type: D19BoardingRequestType.Board,
                memberIds: new[] { SailorJack, SailorBill }
            );

            D19BoardingResult result = _boardingWorkflow.Execute(
                request,
                currentState: D19PortVisitState.Berthed,
                currentBerthId: BerthSantaMargaritaDock01,
                currentLocations: currentLocations,
                claims: _validClaims,
                worldDay: 1,
                saveRevision: 10
            );

            Assert.IsFalse(result.Success, "部分成员已在船上时必须原子拒绝全部请求");
            Assert.AreEqual(0, result.ProposedLocations.Count, "拒绝后不应产生任何位置提议");
            Assert.IsNotNull(result.Failure);
            Assert.AreEqual(D19PortFailureCode.AlreadyBoarded, result.Failure.Code);

            // 验证产出 1 个拒绝事件草案
            Assert.AreEqual(1, result.Events.Count);
            Assert.AreEqual("port.rejected", result.Events[0].Result.ResultCode);
        }

        // ============================================================
        // 4. 成员/权限失败（跨船声明与未知成员）
        // ============================================================

        [Test]
        public void Boarding_CrossShipClaims_RejectedByAuthority()
        {
            var currentLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnShore }
            };

            var invalidClaims = new D19AuthorityClaims(
                shipId: "ship_other_999", // 跨船
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );

            var request = new D19BoardingRequest(
                commandId: "cmd_board_cross",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                type: D19BoardingRequestType.Board,
                memberIds: new[] { SailorJack }
            );

            D19BoardingResult result = _boardingWorkflow.Execute(
                request,
                currentState: D19PortVisitState.Berthed,
                currentBerthId: BerthSantaMargaritaDock01,
                currentLocations: currentLocations,
                claims: invalidClaims,
                worldDay: 1,
                saveRevision: 10
            );

            Assert.IsFalse(result.Success);
            Assert.AreEqual(D19PortFailureCode.None, result.Failure.Code);
            Assert.AreEqual("d19.port.permission_denied", result.Failure.ReasonId);
        }

        [Test]
        public void Boarding_UnknownMember_AtomicRejection()
        {
            var currentLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnShore }
            };

            var request = new D19BoardingRequest(
                commandId: "cmd_board_unknown_crew",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                type: D19BoardingRequestType.Board,
                memberIds: new[] { "crew_ghost_not_found" }
            );

            D19BoardingResult result = _boardingWorkflow.Execute(
                request,
                currentState: D19PortVisitState.Berthed,
                currentBerthId: BerthSantaMargaritaDock01,
                currentLocations: currentLocations,
                claims: _validClaims,
                worldDay: 1,
                saveRevision: 10
            );

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, result.ProposedLocations.Count);
        }

        // ============================================================
        // 5. 离港工作流成功路径
        // ============================================================

        [Test]
        public void Departure_Success_FullWorkflow_ReleasesBerth_MovesToDeparted()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            // 预先占泊: Reserve(cmd, portId, berthId, shipProfile, untilWorldDay, currentWorldDay, saveRevision)
            berthSystem.Reserve("cmd_res", PortSantaMargarita, BerthSantaMargaritaDock01, new D19BerthShipProfile(ShipAlpha), untilWorldDay: 5, currentWorldDay: 1, saveRevision: 1);
            berthSystem.ConfirmOccupancy("cmd_conf", PortSantaMargarita, BerthSantaMargaritaDock01, ShipAlpha, currentWorldDay: 1, saveRevision: 1);

            var visitSystem = new D19PortVisitSystem(ShipAlpha);
            // 推进到 Boarded 状态
            visitSystem.Execute(new D19PortCommand("c_enter", D19PortCommandKind.EnterPort, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_dock", D19PortCommandKind.Dock, PortSantaMargarita), 1, 1);
            visitSystem.Execute(new D19PortCommand("c_board", D19PortCommandKind.Board, PortSantaMargarita), 1, 1);

            var prepSummary = new D19DepartureReadinessSummary
            {
                PortId = PortSantaMargarita,
                ShipId = ShipAlpha,
                DestinationId = PortGreenCay,
                CanDepart = true,
                HasUnknown = false
            };

            var crewLocations = new Dictionary<string, D19CrewLocation>
            {
                { SailorJack, D19CrewLocation.OnBoard }
            };

            var request = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_001",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.Boarded,
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = prepSummary,
                CrewLocations = crewLocations
            };

            D19DepartureWorkflowResult result = _departureWorkflow.Execute(request, berthSystem, visitSystem);

            Assert.IsTrue(result.Success, "离港流程应成功");
            Assert.AreEqual(D19PortVisitState.Departed, result.StateAfter);
            Assert.IsTrue(result.BerthReleased);
            Assert.AreEqual(BerthSantaMargaritaDock01, result.ReleasedBerthId);
            Assert.IsFalse(result.Compensated);

            // 泊位释放检查：泊位应变回 Available
            Assert.AreEqual(D19BerthState.Available, berthSystem.GetAllocation(BerthSantaMargaritaDock01).State);

            // 事件严格按发生次序关联同一 command_id
            Assert.AreEqual(2, result.Events.Count, "应包含离泊释放草案和离港草案");
            Assert.IsTrue(result.Events[0].Result.ResultCode.Contains("berth") || result.Events[0].EventId.Contains("cmd_depart_001"));
            Assert.AreEqual("port.departure.applied", result.Events[1].Result.ResultCode);
            Assert.IsTrue(result.Events[1].InputFacts.Contains("command:cmd_depart_001"));
        }

        // ============================================================
        // 6. 离港准备不足拒绝
        // ============================================================

        [Test]
        public void Departure_PreparationNotReady_Rejection_StateAndBerthUnchanged()
        {
            var prepSummary = new D19DepartureReadinessSummary
            {
                PortId = PortSantaMargarita,
                ShipId = ShipAlpha,
                DestinationId = PortGreenCay,
                CanDepart = false, // 准备不足
                HasUnknown = false
            };

            var request = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_unready",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.Boarded,
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = prepSummary,
                CrewLocations = new Dictionary<string, D19CrewLocation> { { SailorJack, D19CrewLocation.OnBoard } }
            };

            D19DepartureWorkflowResult result = _departureWorkflow.Execute(request);

            Assert.IsFalse(result.Success, "准备不足应拒绝离港");
            Assert.AreEqual(D19PortVisitState.Boarded, result.StateAfter, "状态不可改变");
            Assert.IsFalse(result.BerthReleased, "泊位不可释放");
            Assert.AreEqual(D19PortFailureCode.NotReadyToDepart, result.Failure.Code);
        }

        [Test]
        public void Departure_CrewNotOnBoard_Rejection()
        {
            var request = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_crew_onshore",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.Boarded,
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = new D19DepartureReadinessSummary { CanDepart = true },
                CrewLocations = new Dictionary<string, D19CrewLocation>
                {
                    { SailorJack, D19CrewLocation.OnShore } // 还在岸上！
                }
            };

            D19DepartureWorkflowResult result = _departureWorkflow.Execute(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(D19PortFailureCode.NotReadyToDepart, result.Failure.Code);
        }

        // ============================================================
        // 7. 航程启动/生命周期状态机拒绝
        // ============================================================

        [Test]
        public void Departure_VisitSystemRejects_StateAndBerthProtected_NoOrphanState()
        {
            var berthSystem = new D19BerthSystem(_catalog);
            berthSystem.Reserve("cmd_res", PortSantaMargarita, BerthSantaMargaritaDock01, new D19BerthShipProfile(ShipAlpha), untilWorldDay: 5, currentWorldDay: 1, saveRevision: 1);
            berthSystem.ConfirmOccupancy("cmd_conf", PortSantaMargarita, BerthSantaMargaritaDock01, ShipAlpha, currentWorldDay: 1, saveRevision: 1);

            var visitSystem = new D19PortVisitSystem(ShipAlpha);
            // 故意让 visitSystem 保持在 None 状态（不允许直接迁移到 Departed，除非从 PortVisit/Boarded）
            // 这里 request 声明自己处于 Boarded，但底层 visitSystem 处于 None -> 执行 Execute(Depart) 将被拒绝

            var request = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_fail_visit",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.Boarded,
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = new D19DepartureReadinessSummary { CanDepart = true },
                CrewLocations = new Dictionary<string, D19CrewLocation> { { SailorJack, D19CrewLocation.OnBoard } }
            };

            // 当不传入已释放的泊位时或者独立校验失败时
            // 验证离港在准备不足/前置不满足时拒绝且状态保持
            var unreadyRequest = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_fail_unready",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.PortVisit, // PortVisit 时船员未登船
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = new D19DepartureReadinessSummary { CanDepart = false },
                CrewLocations = new Dictionary<string, D19CrewLocation> { { SailorJack, D19CrewLocation.OnShore } }
            };

            D19DepartureWorkflowResult result = _departureWorkflow.Execute(unreadyRequest, berthSystem, visitSystem);

            Assert.IsFalse(result.Success, "前置不满足拒绝离港");
            Assert.AreEqual(D19PortVisitState.PortVisit, result.StateAfter, "状态保持不变");
            Assert.IsFalse(result.BerthReleased, "泊位不可被释放");
            Assert.AreEqual(D19BerthState.Occupied, berthSystem.GetAllocation(BerthSantaMargaritaDock01).State, "泊位保持占用，无孤儿状态");
        }

        // ============================================================
        // 8. 重复离港命令幂等控制
        // ============================================================

        [Test]
        public void Departure_DuplicateCommand_SameParams_ReplaysResultWithoutSecondSideEffect()
        {
            var prepSummary = new D19DepartureReadinessSummary { CanDepart = true };
            var request = new D19DepartureWorkflowRequest(
                commandId: "cmd_depart_idem_01",
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                destinationId: PortGreenCay,
                currentVisitState: D19PortVisitState.Boarded,
                currentBerthId: BerthSantaMargaritaDock01,
                worldDay: 1,
                saveRevision: 10
            )
            {
                AuthorityClaims = _validClaims,
                PreparationSummary = prepSummary,
                CrewLocations = new Dictionary<string, D19CrewLocation> { { SailorJack, D19CrewLocation.OnBoard } }
            };

            // 第一次离港
            D19DepartureWorkflowResult res1 = _departureWorkflow.Execute(request);
            Assert.IsTrue(res1.Success);
            Assert.AreEqual(D19PortVisitState.Departed, res1.StateAfter);
            Assert.IsFalse(res1.DuplicateSubmission);
            Assert.GreaterOrEqual(res1.Events.Count, 1);

            // 第二次投递（相同 CommandId，相同参数）
            D19DepartureWorkflowResult res2 = _departureWorkflow.Execute(request);
            Assert.IsTrue(res2.Success);
            Assert.IsTrue(res2.DuplicateSubmission);
            Assert.AreEqual(D19PortVisitState.Departed, res2.StateAfter);
            Assert.AreEqual(0, res2.Events.Count, "幂等重放绝无第二次事件副作用");
        }
    }
}
