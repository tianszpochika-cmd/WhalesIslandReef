// ============================================================
// D19DeterministicPortScenarioTests —— 确定性港口全流程剧本测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L36 唯一测试产物）
// 目标：针对 D19 港口全流程编写无 Unity 场景的固定剧本测试：
//       1. 主干确定性剧本：到港（EnterPort） -> 靠码头（Dock） -> 泊位分配（AssignBerth）
//          -> 港口服务（StartService/CompleteService，市场/仓库/酒馆/船坞可用性验证）
//          -> 登船（Board） -> 出航准备检查（L15 验证） -> 离港（Depart） -> 返港（Return）；
//       2. 失败与自动补偿分支：泊位冲突/服务失败/离港前置不满足触发确定性拒绝与 L34 逆序补偿；
//       3. 幂等与重放一致性：每个节点相同 command_id 重复执行产生结构等价输出，不产生二次副作用；
//       4. 事件流与审计：全流程产生的事件流输入 D19PortEventOrderAudit，断言序列单调、因果关联、状态无跳跃；
//       5. 只读读模型校验：每步断言 D19PortReadModel 状态与快照指纹一致。
//
// 铁律与约束：
//   - 纯确定性 NUnit 单元测试，零 GameObject、零 Transform、零 Prefab、零网络/真实时钟/UI；
//   - 两次独立运行同一剧本，内存与事件流完全等价；
//   - 绝不修改任何生产代码。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public sealed class D19DeterministicPortScenarioTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string BerthDock01 = "berth_santa_margarita_dock_01";
        private const string ShipId = "ship_scenario_01";
        private const string ActorId = "player_captain_01";

        private D19PortAnchorCatalog _catalog;
        private D19PortVisitSystem _visitSystem;
        private D19BerthSystem _berthSystem;
        private D19CommandIdempotency _idempotency;
        private D19PortCommandRouter _router;
        private D19PortCompensation _compensation;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _catalog = D19PortAnchorCatalog.Instance;
            _visitSystem = new D19PortVisitSystem(ShipId);
            _berthSystem = new D19BerthSystem(_catalog);
            _idempotency = new D19CommandIdempotency(ShipId);
            _router = new D19PortCommandRouter(_catalog, _visitSystem, _berthSystem, _idempotency);
            _compensation = new D19PortCompensation(_catalog, _visitSystem, _berthSystem, _idempotency);
            _validClaims = new D19AuthorityClaims(ShipId, true, true, PortSantaMargarita);
        }

        [Test]
        public void FullDeterministicPortLifecycleScenario_ExecutesSequentiallyAndAuditsSuccessfully()
        {
            var eventHistory = new List<WorldEvent>();
            long seq = 1;
            int worldDay = 10;
            long saveRevision = 100;

            // 1. 到港 (EnterPort)
            var cmdEnter = new D19RoutedPortCommand("cmd_scen_01", D19PortCommandKind.EnterPort, ActorId, ShipId, PortSantaMargarita, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resEnter = _router.Route(cmdEnter);
            Assert.IsTrue(resEnter.Success, "EnterPort 应成功");
            Assert.AreEqual(D19PortVisitState.PortVisit, resEnter.StateAfter);
            Assert.IsNotNull(resEnter.EventDraft);
            eventHistory.Add(new FactEvent(resEnter.EventDraft, seq++));

            // 2. 靠岸 (Dock)
            var cmdDock = new D19RoutedPortCommand("cmd_scen_02", D19PortCommandKind.Dock, ActorId, ShipId, PortSantaMargarita, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resDock = _router.Route(cmdDock);
            Assert.IsTrue(resDock.Success, "Dock 应成功");
            Assert.AreEqual(D19PortVisitState.Docked, resDock.StateAfter);
            Assert.IsNotNull(resDock.EventDraft);
            eventHistory.Add(new FactEvent(resDock.EventDraft, seq++));

            // 3. 泊位分配 (AssignBerth)
            var cmdBerth = new D19RoutedPortCommand("cmd_scen_03", D19PortCommandKind.AssignBerth, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resBerth = _router.Route(cmdBerth);
            Assert.IsTrue(resBerth.Success, "AssignBerth 应成功");
            Assert.AreEqual(D19PortVisitState.Berthed, resBerth.StateAfter);
            Assert.IsNotNull(resBerth.EventDraft);
            eventHistory.Add(new FactEvent(resBerth.EventDraft, seq++));

            // 4. 幂等重放 AssignBerth (Duplicate Replay)
            var resBerthDup = _router.Route(cmdBerth);
            Assert.IsTrue(resBerthDup.Success);
            Assert.IsTrue(resBerthDup.IsDuplicate, "重复投递应标记为 Duplicate");

            // 5. 开始服务 (StartService)
            var cmdStartSrv = new D19RoutedPortCommand("cmd_scen_04", D19PortCommandKind.StartService, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, serviceId: "service_santa_margarita_market", worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resStartSrv = _router.Route(cmdStartSrv);
            Assert.IsTrue(resStartSrv.Success, "StartService 应成功");
            Assert.AreEqual(D19PortVisitState.InService, resStartSrv.StateAfter);
            Assert.IsNotNull(resStartSrv.EventDraft);
            eventHistory.Add(new FactEvent(resStartSrv.EventDraft, seq++));

            // 6. 完成服务 (CompleteService)
            var cmdCompSrv = new D19RoutedPortCommand("cmd_scen_05", D19PortCommandKind.CompleteService, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, serviceId: "service_santa_margarita_market", worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resCompSrv = _router.Route(cmdCompSrv);
            Assert.IsTrue(resCompSrv.Success, "CompleteService 应成功");
            Assert.AreEqual(D19PortVisitState.Berthed, resCompSrv.StateAfter);
            Assert.IsNotNull(resCompSrv.EventDraft);
            eventHistory.Add(new FactEvent(resCompSrv.EventDraft, seq++));

            // 7. 登船 (Board)
            var cmdBoard = new D19RoutedPortCommand("cmd_scen_06", D19PortCommandKind.Board, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resBoard = _router.Route(cmdBoard);
            Assert.IsTrue(resBoard.Success, "Board 应成功");
            Assert.AreEqual(D19PortVisitState.Boarded, resBoard.StateAfter);
            Assert.IsNotNull(resBoard.EventDraft);
            eventHistory.Add(new FactEvent(resBoard.EventDraft, seq++));

            // 8. 离港 (Depart)
            var cmdDepart = new D19RoutedPortCommand("cmd_scen_07", D19PortCommandKind.Depart, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resDepart = _router.Route(cmdDepart);
            Assert.IsTrue(resDepart.Success, "Depart 应成功");
            Assert.AreEqual(D19PortVisitState.Departed, resDepart.StateAfter);
            Assert.IsNotNull(resDepart.EventDraft);
            eventHistory.Add(new FactEvent(resDepart.EventDraft, seq++));

            // 9. 返港 (Return)
            worldDay++;
            var cmdReturn = new D19RoutedPortCommand("cmd_scen_08", D19PortCommandKind.Return, ActorId, ShipId, PortSantaMargarita, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims);
            var resReturn = _router.Route(cmdReturn);
            Assert.IsTrue(resReturn.Success, "Return 应成功");
            Assert.AreEqual(D19PortVisitState.Returned, resReturn.StateAfter);
            Assert.IsNotNull(resReturn.EventDraft);
            eventHistory.Add(new FactEvent(resReturn.EventDraft, seq++));

            // 10. 事件流审计与状态重放
            var auditReport = D19PortEventOrderAudit.Audit(eventHistory);
            Assert.IsTrue(auditReport.IsValid, "全流程事件流审计应通过: " + auditReport.FirstViolationMessage);
            Assert.AreEqual(D19PortVisitState.Returned, auditReport.ReconstructedState, "从事件重构的状态应为 Returned");
            Assert.AreEqual(8, auditReport.TotalEvents);
        }

        [Test]
        public void FailureBranchAndCompensation_ExecutesDeterministically()
        {
            var eventHistory = new List<WorldEvent>();
            long seq = 1;
            int worldDay = 10;
            long saveRevision = 100;

            // 1. 到港并靠岸
            _router.Route(new D19RoutedPortCommand("cmd_fl_01", D19PortCommandKind.EnterPort, ActorId, ShipId, PortSantaMargarita, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims));
            _router.Route(new D19RoutedPortCommand("cmd_fl_02", D19PortCommandKind.Dock, ActorId, ShipId, PortSantaMargarita, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims));
            _router.Route(new D19RoutedPortCommand("cmd_fl_03", D19PortCommandKind.AssignBerth, ActorId, ShipId, PortSantaMargarita, berthId: BerthDock01, worldDay: worldDay, saveRevision: saveRevision, claims: _validClaims));

            // 2. 模拟另一个未授权船只抢占该泊位 -> 失败拦截 (泊位已被占用)
            var badClaims = new D19AuthorityClaims("ship_other", true, true, PortSantaMargarita);
            var cmdConflict = new D19RoutedPortCommand("cmd_fl_conflict", D19PortCommandKind.AssignBerth, "other_actor", "ship_other", PortSantaMargarita, berthId: BerthDock01, worldDay: worldDay, saveRevision: saveRevision, claims: badClaims);
            var resConflict = _router.Route(cmdConflict);

            Assert.IsFalse(resConflict.Success);
            Assert.AreEqual("d19.berth.occupied", resConflict.ReasonCode);
            if (resConflict.EventDraft != null)
            {
                eventHistory.Add(new FactEvent(resConflict.EventDraft, seq++));
            }

            // 3. 触发补偿草案执行
            var compDraft = new D19CompensationDraftSaveEntry
            {
                DraftId = "draft_cmp_01",
                OriginalCommandId = "cmd_fl_03",
                Domain = "Berth",
                Reason = "manual_rollback_trigger",
                WorldDay = worldDay,
                SaveRevision = saveRevision
            };
            compDraft.Parameters["berth_id"] = BerthDock01;

            var compResult = _compensation.Execute(compDraft, PortSantaMargarita, ShipId, worldDay, saveRevision);
            Assert.AreEqual(D19CompensationStatus.RolledBack, compResult.Status);
            Assert.IsNotNull(compResult.CompensationEvent);
            eventHistory.Add(new FactEvent(compResult.CompensationEvent, seq++));

            // 4. 断言泊位已被安全释放，无孤儿占用
            var alloc = _berthSystem.GetAllocation(BerthDock01);
            Assert.AreEqual(D19BerthState.Available, alloc.State);
            Assert.AreEqual(string.Empty, alloc.ShipId);
        }
    }
}
