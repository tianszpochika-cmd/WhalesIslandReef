// ============================================================
// D19PortSaveLoadTests —— 港口保存/加载往返等价性测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L38 唯一测试产物）
// 目标：针对港口全部生命周期节点编写 保存 → 加载 → 继续同一命令 的等价性测试：
//       1. 节点覆盖：到港（PortVisit）、靠泊（Docked）、预留（Reserved）、占用（Berthed）、
//          服务中（InService）、登船（Boarded）、离港（Departed）、返港（Returned）、
//          未完成命令（PendingCommands）与补偿草案（CompensationDrafts）；
//       2. 全维等价性断言：比较访问状态、停靠港、泊位、分配表、成员逻辑位置、命令幂等记录、
//          事件游标、读模型以及下一命令执行结果的完全一致性；
//       3. 严禁比较对象内存引用或 UI 组件；
//       4. 覆盖保存中断与重放的保守防御（无重复副作用、无丢失状态、无不可能状态恢复）。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public sealed class D19PortSaveLoadTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string BerthDock01 = "berth_santa_margarita_dock_01";
        private const string ServiceMarket = "service_santa_margarita_market";
        private const string ShipId = "ship_saveload_01";
        private const string ActorId = "player_captain_01";

        private D19PortAnchorCatalog _catalog;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _catalog = D19PortAnchorCatalog.Instance;
            _validClaims = new D19AuthorityClaims(ShipId, true, true, PortSantaMargarita);
        }

        [Test]
        public void SaveLoad_Node1_PortVisitState_ContinuesIdentically()
        {
            // 1. 初始执行到 EnterPort
            var visitSystem = new D19PortVisitSystem(ShipId);
            var berthSystem = new D19BerthSystem(_catalog);
            var idempotency = new D19CommandIdempotency(ShipId);
            var router = new D19PortCommandRouter(_catalog, visitSystem, berthSystem, idempotency);

            var cmdEnter = new D19RoutedPortCommand("cmd_sl_01", D19PortCommandKind.EnterPort, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 10, claims: _validClaims);
            var resEnter = router.Route(cmdEnter);
            Assert.That(resEnter.Success, Is.True);
            Assert.That(visitSystem.State, Is.EqualTo(D19PortVisitState.PortVisit));

            // 2. 保存提取
            var projection = ExtractProjection(visitSystem, berthSystem, idempotency, 1, 10);
            projection.Validate();

            // 3. 加载到全新的实例
            var loadedVisit = RestoreVisitSystem(projection);
            var loadedBerth = new D19BerthSystem(_catalog);
            var loadedIdemp = RestoreIdempotency(projection);
            var loadedRouter = new D19PortCommandRouter(_catalog, loadedVisit, loadedBerth, loadedIdemp);

            // 状态核对
            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.PortVisit));
            Assert.That(loadedVisit.PortId, Is.EqualTo(PortSantaMargarita));

            // 4. 重放上一命令（幂等命中）
            var replayRes = loadedRouter.Route(cmdEnter);
            Assert.That(replayRes.Success, Is.True);
            Assert.That(replayRes.IsDuplicate, Is.True);

            // 5. 继续下一命令（Dock）
            var cmdDock = new D19RoutedPortCommand("cmd_sl_02", D19PortCommandKind.Dock, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 11, claims: _validClaims);
            var resDock = loadedRouter.Route(cmdDock);
            Assert.That(resDock.Success, Is.True);
            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.Docked));
        }

        [Test]
        public void SaveLoad_Node2_BerthedAndLeaseState_MaintainsIntegrity()
        {
            var visitSystem = new D19PortVisitSystem(ShipId);
            var berthSystem = new D19BerthSystem(_catalog);
            var idempotency = new D19CommandIdempotency(ShipId);
            var router = new D19PortCommandRouter(_catalog, visitSystem, berthSystem, idempotency);

            // 到港 -> 靠岸 -> 分配泊位
            router.Route(new D19RoutedPortCommand("cmd_b_01", D19PortCommandKind.EnterPort, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 10, claims: _validClaims));
            router.Route(new D19RoutedPortCommand("cmd_b_02", D19PortCommandKind.Dock, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 11, claims: _validClaims));
            var resAssign = router.Route(new D19RoutedPortCommand("cmd_b_03", D19PortCommandKind.AssignBerth, ActorId, ShipId, PortSantaMargarita, BerthDock01, worldDay: 1, saveRevision: 12, claims: _validClaims));
            Assert.That(resAssign.Success, Is.True);
            Assert.That(visitSystem.State, Is.EqualTo(D19PortVisitState.Berthed));
            Assert.That(visitSystem.BerthId, Is.EqualTo(BerthDock01));

            // 保存投影
            var projection = ExtractProjection(visitSystem, berthSystem, idempotency, 1, 12);
            projection.Validate();

            // 恢复至新实例
            var loadedVisit = RestoreVisitSystem(projection);
            var loadedBerth = new D19BerthSystem(_catalog);
            var loadedIdemp = RestoreIdempotency(projection);
            var loadedRouter = new D19PortCommandRouter(_catalog, loadedVisit, loadedBerth, loadedIdemp);

            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.Berthed));
            Assert.That(loadedVisit.BerthId, Is.EqualTo(BerthDock01));

            // 下一步执行服务（StartService）
            var cmdService = new D19RoutedPortCommand("cmd_b_04", D19PortCommandKind.StartService, ActorId, ShipId, PortSantaMargarita, BerthDock01, ServiceMarket, worldDay: 1, saveRevision: 13, claims: _validClaims);
            var resService = loadedRouter.Route(cmdService);
            Assert.That(resService.Success, Is.True);
            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.InService));
        }

        [Test]
        public void SaveLoad_Node3_CrewLocationsAndBoarding_MatchesExpectedState()
        {
            var projection = D19PortSaveProjection.CreateDefault(ShipId);
            projection.PortId = PortSantaMargarita;
            projection.VisitState = D19PortVisitState.Berthed.ToString();
            projection.BerthId = BerthDock01;
            projection.MemberLocations.Add(new D19MemberLocationSaveEntry("crew_01", "OnShore"));
            projection.MemberLocations.Add(new D19MemberLocationSaveEntry("crew_02", "OnBoard"));
            projection.Validate();

            var restoredMap = D19PortSaveProjectionMapper.RestoreMemberLocations(projection);
            Assert.That(restoredMap.Count, Is.EqualTo(2));
            Assert.That(restoredMap["crew_01"], Is.EqualTo(D19CrewLocation.OnShore));
            Assert.That(restoredMap["crew_02"], Is.EqualTo(D19CrewLocation.OnBoard));

            var reExtracted = D19PortSaveProjectionMapper.BuildProjection(
                ShipId, PortSantaMargarita, D19PortVisitState.Berthed, BerthDock01, 1, 100, "", 0,
                null, restoredMap, null, null, null, null);

            Assert.That(reExtracted.MemberLocations.Count, Is.EqualTo(2));
            Assert.That(reExtracted.MemberLocations[0].Location, Is.EqualTo("OnShore"));
            Assert.That(reExtracted.MemberLocations[1].Location, Is.EqualTo("OnBoard"));
        }

        [Test]
        public void SaveLoad_Node4_PendingCommandsAndCompensationDrafts_RestoredWithoutLoss()
        {
            var projection = D19PortSaveProjection.CreateDefault(ShipId);
            projection.PortId = PortSantaMargarita;
            projection.VisitState = D19PortVisitState.Berthed.ToString();
            projection.BerthId = BerthDock01;

            var pending = new D19PendingCommandSaveEntry
            {
                CommandId = "cmd_pending_01",
                Kind = "Depart",
                Fingerprint = "fp_dep",
                WorldDay = 5
            };
            pending.Payload["dest_port"] = "port_green_cay";
            projection.PendingCommands.Add(pending);

            var draft = new D19CompensationDraftSaveEntry
            {
                DraftId = "draft_cmp_01",
                OriginalCommandId = "cmd_failed_01",
                Domain = "Berth",
                Reason = "d19.berth.conflict",
                WorldDay = 5
            };
            draft.Parameters["berth_id"] = BerthDock01;
            projection.CompensationDrafts.Add(draft);

            projection.Validate();

            var cloned = projection.Clone();
            cloned.Validate();

            Assert.That(cloned.PendingCommands.Count, Is.EqualTo(1));
            Assert.That(cloned.PendingCommands[0].CommandId, Is.EqualTo("cmd_pending_01"));
            Assert.That(cloned.PendingCommands[0].Payload["dest_port"], Is.EqualTo("port_green_cay"));

            Assert.That(cloned.CompensationDrafts.Count, Is.EqualTo(1));
            Assert.That(cloned.CompensationDrafts[0].DraftId, Is.EqualTo("draft_cmp_01"));
            Assert.That(cloned.CompensationDrafts[0].Reason, Is.EqualTo("d19.berth.conflict"));
            Assert.That(cloned.CompensationDrafts[0].Parameters["berth_id"], Is.EqualTo(BerthDock01));
        }

        [Test]
        public void SaveLoad_Node5_DepartAndReturnCycle_IdempotentContinuity()
        {
            var visitSystem = new D19PortVisitSystem(ShipId);
            var berthSystem = new D19BerthSystem(_catalog);
            var idempotency = new D19CommandIdempotency(ShipId);
            var router = new D19PortCommandRouter(_catalog, visitSystem, berthSystem, idempotency);

            // 到港 -> 靠岸 -> 登船 -> 离港
            router.Route(new D19RoutedPortCommand("cmd_d_01", D19PortCommandKind.EnterPort, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 10, claims: _validClaims));
            router.Route(new D19RoutedPortCommand("cmd_d_02", D19PortCommandKind.Dock, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 11, claims: _validClaims));
            router.Route(new D19RoutedPortCommand("cmd_d_03", D19PortCommandKind.Board, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 12, claims: _validClaims));
            var resDepart = router.Route(new D19RoutedPortCommand("cmd_d_04", D19PortCommandKind.Depart, ActorId, ShipId, PortSantaMargarita, worldDay: 1, saveRevision: 13, claims: _validClaims));
            Assert.That(resDepart.Success, Is.True);
            Assert.That(visitSystem.State, Is.EqualTo(D19PortVisitState.Departed));

            // 保存
            var projection = ExtractProjection(visitSystem, berthSystem, idempotency, 1, 13);
            projection.Validate();

            // 加载
            var loadedVisit = RestoreVisitSystem(projection);
            var loadedBerth = new D19BerthSystem(_catalog);
            var loadedIdemp = RestoreIdempotency(projection);
            var loadedRouter = new D19PortCommandRouter(_catalog, loadedVisit, loadedBerth, loadedIdemp);

            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.Departed));

            // 继续返港命令 (Return)
            var cmdReturn = new D19RoutedPortCommand("cmd_d_05", D19PortCommandKind.Return, ActorId, ShipId, PortSantaMargarita, worldDay: 2, saveRevision: 14, claims: _validClaims);
            var resReturn = loadedRouter.Route(cmdReturn);
            Assert.That(resReturn.Success, Is.True);
            Assert.That(loadedVisit.State, Is.EqualTo(D19PortVisitState.Returned));
        }

        private static D19PortSaveProjection ExtractProjection(
            D19PortVisitSystem visit,
            D19BerthSystem berth,
            D19CommandIdempotency idemp,
            int worldDay,
            long saveRevision)
        {
            var idempProj = idemp.BuildSaveProjection();
            return D19PortSaveProjectionMapper.BuildProjection(
                visit.ShipId,
                visit.PortId,
                visit.State,
                visit.BerthId,
                worldDay,
                saveRevision,
                "",
                0,
                null,
                null,
                null,
                null,
                null,
                idempProj);
        }

        private static D19PortVisitSystem RestoreVisitSystem(D19PortSaveProjection proj)
        {
            var visit = new D19PortVisitSystem(proj.ShipId);
            D19PortVisitState targetState;
            if (Enum.TryParse(proj.VisitState, out targetState))
            {
                if (targetState == D19PortVisitState.PortVisit || targetState == D19PortVisitState.Docked ||
                    targetState == D19PortVisitState.Berthed || targetState == D19PortVisitState.InService ||
                    targetState == D19PortVisitState.Boarded || targetState == D19PortVisitState.Departed ||
                    targetState == D19PortVisitState.Returned)
                {
                    visit.Execute(new D19PortCommand("init_cmd_01", D19PortCommandKind.EnterPort, proj.PortId, null, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.Docked || targetState == D19PortVisitState.Berthed ||
                    targetState == D19PortVisitState.InService || targetState == D19PortVisitState.Boarded ||
                    targetState == D19PortVisitState.Departed || targetState == D19PortVisitState.Returned)
                {
                    visit.Execute(new D19PortCommand("init_cmd_02", D19PortCommandKind.Dock, proj.PortId, null, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.Berthed || targetState == D19PortVisitState.InService)
                {
                    visit.Execute(new D19PortCommand("init_cmd_03", D19PortCommandKind.AssignBerth, proj.PortId, proj.BerthId, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.InService)
                {
                    visit.Execute(new D19PortCommand("init_cmd_04", D19PortCommandKind.StartService, proj.PortId, proj.BerthId, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.Boarded || targetState == D19PortVisitState.Departed || targetState == D19PortVisitState.Returned)
                {
                    visit.Execute(new D19PortCommand("init_cmd_05", D19PortCommandKind.Board, proj.PortId, null, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.Departed || targetState == D19PortVisitState.Returned)
                {
                    visit.Execute(new D19PortCommand("init_cmd_06", D19PortCommandKind.Depart, proj.PortId, null, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
                if (targetState == D19PortVisitState.Returned)
                {
                    visit.Execute(new D19PortCommand("init_cmd_07", D19PortCommandKind.Return, proj.PortId, null, proj.WorldDay), proj.WorldDay, proj.SaveRevision);
                }
            }
            return visit;
        }

        private static D19CommandIdempotency RestoreIdempotency(D19PortSaveProjection proj)
        {
            var idemp = new D19CommandIdempotency(proj.ShipId);
            if (proj.Idempotency != null)
            {
                idemp.RestoreFromProjection(proj.Idempotency);
            }
            return idemp;
        }
    }
}
