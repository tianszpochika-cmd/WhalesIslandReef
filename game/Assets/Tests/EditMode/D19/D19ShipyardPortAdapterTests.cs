// ============================================================
// D19ShipyardPortAdapterTests —— 船坞港口适配器确定性单元测试（纯 C# EditMode）
//
// 归属：Whalebone.Tests.EditMode.D19（D19-L28 唯一测试产物）
// 目标：针对 D19-L14 D19ShipyardPortAdapter 编写纯逻辑确定性测试：
//       1. 合法改装（Upgrade）与船型解锁（UnlockShip）请求透传至 D4 裁决；
//       2. 服务关闭（L05 可用性 / 设施关闭 / 恶劣天气 / 夜间）前置阻断且不调用 D4；
//       3. 权限拒绝（D19-L23 跨船 / 跨港 / 伪造服务）拦截且不产生副作用；
//       4. 船型不支持（未知船型）与改装不支持（未知槽位、已满级、检修未实现）诚实拒绝；
//       5. D4 底层拒绝（资金不足、港口/势力声望不足）稳定映射至 UnderlyingRejected；
//       6. 重复命令幂等控制（相同 command_id 重放既有结果，不二次扣费，不重复改装）；
//       7. 适配器不直接修改船只属性与全局金钱，成功状态完全来自 D4 裁决，失败不留下半改装状态；
//       8. 成功与拒绝分别产出合规的 L16 事件草案（port.shipyard.applied / port.rejected）。
//
// 铁律与约束：
//   - 纯确定性 NUnit 单元测试，零 GameObject、零 Transform、零 Prefab、零模型/材质、零 UI/网络；
//   - 绝不修改任何生产代码。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D4;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.EditMode.D19
{
    [TestFixture]
    public sealed class D19ShipyardPortAdapterTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string ServiceShipyard = "shipyard";
        private const string ActorPlayer = "player_captain_01";
        private const string ShipAlpha = "ship_alpha_01";
        private const string ShipBeta = "ship_beta_02";

        private D4ShipyardSystem _shipyardSystem;
        private D19ShipyardPortAdapter _adapter;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            var initial = new ShipyardState();
            _shipyardSystem = new D4ShipyardSystem(initial);
            _adapter = new D19ShipyardPortAdapter(_shipyardSystem);
            _validClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );
        }

        private static D19ShipyardRequest CreateBaseRequest(
            string commandId,
            D19ShipyardOperation op,
            int money = 1000,
            int reputation = 50,
            D19PortVisitState visitState = D19PortVisitState.Berthed)
        {
            return new D19ShipyardRequest
            {
                CommandId = commandId,
                Operation = op,
                ActorId = ActorPlayer,
                ShipId = ShipAlpha,
                PortId = PortSantaMargarita,
                ServiceId = ServiceShipyard,
                Money = money,
                Reputation = reputation,
                WorldDay = 10,
                SaveRevision = 100,
                Claims = new D19AuthorityClaims(ShipAlpha, true, true, PortSantaMargarita),
                TimePeriod = D19ServiceTimePeriod.Morning,
                FactionPermission = D19FactionPermission.Allowed,
                Weather = D19WeatherLabel.Clear,
                FacilityState = D19FacilityState.Open,
                Staffed = true,
                VisitState = visitState
            };
        }

        // ============================================================
        // 1. 合法改装（Upgrade）与船型解锁（UnlockShip）透传成功
        // ============================================================

        [Test]
        public void Upgrade_ValidRequest_PassesToD4_SucceedsAndGeneratesAppliedEvent()
        {
            // Hull 1 级需要 cost: 180 * 1 = 180, rep: 5
            var req = CreateBaseRequest("cmd_upg_hull_01", D19ShipyardOperation.Upgrade, money: 500, reputation: 20);
            req.Slot = D4UpgradeSlot.Hull;

            int initialLevel = _shipyardSystem.Level(D4UpgradeSlot.Hull);
            Assert.AreEqual(0, initialLevel);

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsTrue(result.Accepted, "合法改装请求应被接受");
            Assert.AreEqual(D19ShipyardOutcome.Accepted, result.Outcome);
            Assert.AreEqual(180, result.Cost);
            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(500 - 180, result.MoneyAfter);
            Assert.IsFalse(result.DuplicateSubmission);

            // 断言 D4 状态真实变更（来自 D4 裁决）
            Assert.AreEqual(1, _shipyardSystem.Level(D4UpgradeSlot.Hull));
            Assert.AreEqual(15, _shipyardSystem.GetModifiers().HullBonus);

            // 断言 L16 事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual("D19PortSystem", result.EventDraft.OwnerSystem);
            Assert.AreEqual(D19ShipyardProtocol.ResultCodeApplied, result.EventDraft.Result.ResultCode);
            Assert.AreEqual(D19PortLocalizationKeys.PortEventServiceResult, result.EventDraft.Result.Summary);
            Assert.AreEqual("hull", result.EventDraft.Result.Values["shipyard.slot"]);
            Assert.AreEqual("1", result.EventDraft.Result.Values["shipyard.new_level"]);
            Assert.AreEqual("180", result.EventDraft.Result.Values["shipyard.cost"]);
        }

        [Test]
        public void UnlockShip_ValidRequest_PassesToD4_SucceedsAndUnlocks()
        {
            // sloop: Price = 850, UnlockReputation = 10
            var req = CreateBaseRequest("cmd_unlock_sloop_01", D19ShipyardOperation.UnlockShip, money: 1000, reputation: 20);
            req.ShipTypeId = "sloop";

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsTrue(result.Accepted, "合法解锁船型请求应被接受");
            Assert.AreEqual(D19ShipyardOutcome.Accepted, result.Outcome);
            Assert.AreEqual(850, result.Cost);
            Assert.AreEqual("sloop", result.ShipTypeId);
            Assert.AreEqual(1000 - 850, result.MoneyAfter);

            // 断言 D4 状态
            Assert.IsTrue(_shipyardSystem.Snapshot.UnlockedShipTypes.Contains("sloop"));

            // 事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual(D19ShipyardProtocol.ResultCodeApplied, result.EventDraft.Result.ResultCode);
            Assert.AreEqual("sloop", result.EventDraft.Result.Values["shipyard.ship_type"]);
            Assert.AreEqual("850", result.EventDraft.Result.Values["shipyard.cost"]);
        }

        // ============================================================
        // 2. 检修（Repair）显式不支持（D4 无公开 API 诚实 BLOCKED）
        // ============================================================

        [Test]
        public void Repair_ReturnsUnsupportedModification_HonestBlocked()
        {
            var req = CreateBaseRequest("cmd_repair_01", D19ShipyardOperation.Repair, money: 500);

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnsupportedModification, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonRepairUnimplemented, result.ReasonId);
            Assert.AreEqual(D19FailureRetryPolicy.NoRetry, result.Retry);
            Assert.AreEqual(500, result.MoneyAfter, "费用绝不扣减");
        }

        // ============================================================
        // 3. 服务关闭与前置条件拒绝（可用性 / 势力 / 天气 / 时段 / 访问状态）
        // ============================================================

        [Test]
        public void Upgrade_WhenFacilityClosed_ReturnsServiceUnavailable()
        {
            var req = CreateBaseRequest("cmd_upg_closed_01", D19ShipyardOperation.Upgrade);
            req.Slot = D4UpgradeSlot.Sail;
            req.FacilityState = D19FacilityState.Closed;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.ServiceUnavailable, result.Outcome);
            Assert.AreEqual("d19.service.unavailable", result.FailureCode);
            Assert.AreEqual(D19PortLocalizationKeys.FailureServiceUnavailable, result.LocKey);
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Sail), "底层状态保持不变");
        }

        [Test]
        public void Upgrade_WhenNightTime_ReturnsServiceUnavailable()
        {
            // 船坞夜间关闭
            var req = CreateBaseRequest("cmd_upg_night_01", D19ShipyardOperation.Upgrade);
            req.Slot = D4UpgradeSlot.Sail;
            req.TimePeriod = D19ServiceTimePeriod.Night;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.ServiceUnavailable, result.Outcome);
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Sail));
        }

        [Test]
        public void Upgrade_WhenLifecycleNotInBerthed_ReturnsServiceUnavailable()
        {
            // 开始服务前置必须 Berthed（若处于 None / Docked / Departed 则拒绝）
            var req = CreateBaseRequest("cmd_upg_not_berthed_01", D19ShipyardOperation.Upgrade, visitState: D19PortVisitState.Docked);
            req.Slot = D4UpgradeSlot.Cannon;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.ServiceUnavailable, result.Outcome);
            Assert.AreEqual(D19PortFailureCode.ServiceUnavailable, result.LifecycleCode);
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Cannon));
        }

        // ============================================================
        // 4. 权限与跨实体归属拒绝
        // ============================================================

        [Test]
        public void Upgrade_CrossShipOrUnauthorized_ReturnsNotAuthorized()
        {
            var req = CreateBaseRequest("cmd_upg_perm_01", D19ShipyardOperation.Upgrade);
            req.Slot = D4UpgradeSlot.MedBay;
            // 声明属于 ShipBeta，但请求操作 ShipAlpha
            req.Claims = new D19AuthorityClaims(ShipBeta, true, true, PortSantaMargarita);

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.NotAuthorized, result.Outcome);
            Assert.AreEqual("d19.port.permission_denied", result.FailureCode);
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.MedBay));
        }

        [Test]
        public void Upgrade_ServiceMismatch_ReturnsInvalidRequest()
        {
            var req = CreateBaseRequest("cmd_upg_mismatch_01", D19ShipyardOperation.Upgrade);
            req.ServiceId = "market"; // 非 shipyard 服务

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.InvalidRequest, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonServiceMismatch, result.ReasonId);
        }

        // ============================================================
        // 5. 船型与改装槽位不支持（未知槽位 / 满级 / 未知船型）
        // ============================================================

        [Test]
        public void Upgrade_UnknownSlot_ReturnsUnsupportedModification()
        {
            var req = CreateBaseRequest("cmd_upg_unknown_slot_01", D19ShipyardOperation.Upgrade);
            req.Slot = (D4UpgradeSlot)999; // 非法枚举

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnsupportedModification, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonUnknownSlot, result.ReasonId);
        }

        [Test]
        public void Upgrade_AlreadyMaxLevel_ReturnsUnsupportedModification()
        {
            // 将 Galley 改装至满级 3
            int money = 10000;
            _shipyardSystem.TryUpgrade(D4UpgradeSlot.Galley, ref money, 100);
            _shipyardSystem.TryUpgrade(D4UpgradeSlot.Galley, ref money, 100);
            _shipyardSystem.TryUpgrade(D4UpgradeSlot.Galley, ref money, 100);
            Assert.AreEqual(3, _shipyardSystem.Level(D4UpgradeSlot.Galley));

            var req = CreateBaseRequest("cmd_upg_max_level_01", D19ShipyardOperation.Upgrade, money: 5000, reputation: 100);
            req.Slot = D4UpgradeSlot.Galley;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnsupportedModification, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonMaxLevel, result.ReasonId);
            Assert.AreEqual(3, _shipyardSystem.Level(D4UpgradeSlot.Galley), "等级保持为 3");
        }

        [Test]
        public void UnlockShip_UnknownShipType_ReturnsUnsupportedModification()
        {
            var req = CreateBaseRequest("cmd_unlock_unknown_01", D19ShipyardOperation.UnlockShip);
            req.ShipTypeId = "alien_spaceship_999";

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnsupportedModification, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonUnknownShipType, result.ReasonId);
        }

        // ============================================================
        // 6. D4 底层拒绝（资金不足 / 声望不足）
        // ============================================================

        [Test]
        public void Upgrade_InsufficientFunds_D4Rejects_MappedToUnderlyingRejected()
        {
            // Hull level 1 cost = 180, money = 50 -> 不足
            var req = CreateBaseRequest("cmd_upg_nofunds_01", D19ShipyardOperation.Upgrade, money: 50, reputation: 50);
            req.Slot = D4UpgradeSlot.Hull;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnderlyingRejected, result.Outcome);
            Assert.AreEqual(D19ShipyardProtocol.ReasonUnderlyingRejected, result.ReasonId);
            Assert.AreEqual(50, result.MoneyAfter, "金钱不得变动");
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Hull), "绝不留下半改装状态");

            // 拒绝事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual(D19ShipyardProtocol.ResultCodeRejected, result.EventDraft.Result.ResultCode);
            Assert.AreEqual(D19ShipyardProtocol.ReasonUnderlyingRejected, result.EventDraft.Result.Values["port.reject_reason"]);
        }

        [Test]
        public void Upgrade_InsufficientReputation_D4Rejects_MappedToUnderlyingRejected()
        {
            // Hull level 1 需要 rep: 5, reputation = 0 -> 不足
            var req = CreateBaseRequest("cmd_upg_norep_01", D19ShipyardOperation.Upgrade, money: 1000, reputation: 0);
            req.Slot = D4UpgradeSlot.Hull;

            D19ShipyardAdapterResult result = _adapter.Execute(req);

            Assert.IsFalse(result.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.UnderlyingRejected, result.Outcome);
            Assert.AreEqual(1000, result.MoneyAfter);
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Hull));
        }

        // ============================================================
        // 7. 幂等控制（相同 command_id 重复投递不重复扣款/升级）
        // ============================================================

        [Test]
        public void Upgrade_DuplicateSubmission_ReturnsCachedResultWithoutSecondSideEffect()
        {
            var req = CreateBaseRequest("cmd_upg_idem_01", D19ShipyardOperation.Upgrade, money: 1000, reputation: 50);
            req.Slot = D4UpgradeSlot.Sail; // Sail level 1 cost = 150

            // 第一次投递：成功
            D19ShipyardAdapterResult first = _adapter.Execute(req);
            Assert.IsTrue(first.Accepted);
            Assert.IsFalse(first.DuplicateSubmission);
            Assert.AreEqual(1, first.NewLevel);
            Assert.AreEqual(850, first.MoneyAfter);
            Assert.AreEqual(1, _shipyardSystem.Level(D4UpgradeSlot.Sail));

            // 第二次投递（相同 command_id）：重放缓存结果，绝无第二次升级
            D19ShipyardAdapterResult second = _adapter.Execute(req);
            Assert.IsTrue(second.Accepted);
            Assert.IsTrue(second.DuplicateSubmission);
            Assert.AreEqual(first.Cost, second.Cost);
            Assert.AreEqual(first.NewLevel, second.NewLevel);
            Assert.AreEqual(first.MoneyAfter, second.MoneyAfter);
            Assert.IsNull(second.EventDraft, "重复命中时不应产生新的事件草案");

            // 校验 D4 状态仍然是 1 级（绝未升级到 2 级）
            Assert.AreEqual(1, _shipyardSystem.Level(D4UpgradeSlot.Sail));
        }

        // ============================================================
        // 8. 缺失 command_id / 空请求安全拒绝
        // ============================================================

        [Test]
        public void Execute_MissingCommandIdOrNull_ReturnsMissingCommandId()
        {
            var req = CreateBaseRequest("", D19ShipyardOperation.Upgrade);
            D19ShipyardAdapterResult res1 = _adapter.Execute(req);

            Assert.IsFalse(res1.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.MissingCommandId, res1.Outcome);
            Assert.AreEqual(D19PortLocalizationKeys.FailureMissingCommandId, res1.LocKey);

            D19ShipyardAdapterResult res2 = _adapter.Execute(null);
            Assert.IsFalse(res2.Accepted);
            Assert.AreEqual(D19ShipyardOutcome.MissingCommandId, res2.Outcome);
        }

        // ============================================================
        // 9. 适配器数据所有权红线断言
        // ============================================================

        [Test]
        public void Adapter_DoesNotDirectlyMutateShipProperties_PreservesInvariant()
        {
            // 创建无升级的初始状态
            var state = _shipyardSystem.Snapshot;
            Assert.AreEqual(0, state.Upgrades["Hull"]);
            Assert.AreEqual(0, state.Upgrades["Sail"]);

            var req = CreateBaseRequest("cmd_check_owner_01", D19ShipyardOperation.Upgrade, money: 500, reputation: 20);
            req.Slot = D4UpgradeSlot.Hull;

            _adapter.Execute(req);

            // 成功变更只能由 D4 的 TryUpgrade 引起
            Assert.AreEqual(1, _shipyardSystem.Level(D4UpgradeSlot.Hull));
            Assert.AreEqual(0, _shipyardSystem.Level(D4UpgradeSlot.Sail));
        }
    }
}
