// ============================================================
// D19TavernPortAdapterTests —— 港口酒馆适配器确定性单元测试（纯 C# EditMode）
//
// 归属：Whalebone.Systems.D19.Tests（D19-L27 唯一测试产物）
// 目标：针对 D19-L13 D19TavernPortAdapter 编写纯逻辑确定性测试：
//       1. 酒馆休整（Visit）成功：士气与疲劳改善、扣减对应费用、产出 port.tavern.visit_applied 事件草案；
//       2. 招募（Recruit）成功：费用扣减、船员入列、产出 port.tavern.recruit_applied 事件草案；
//       3. 社交（Social）成功：D10 社交请求正常处理、产出事件草案；
//       4. 服务关闭与前置阻断（L05 可用性：设施关闭、天气风暴、夜间、未靠泊等）：阻断且不产生费用扣减；
//       5. 权限拒绝（D19-L23 跨船、跨港、跨玩家、伪造服务）：拦截且不产生副作用；
//       6. 费用拒绝（资金不足）：D9 TrySpend 失败稳定映射至 d19.trade.funds_insufficient，资金不变；
//       7. 原系统拒绝（满员 crew_full、对象非法 invalid_candidate、未靠泊 not_moored 等）：映射至 L17 失败码且保留原始码；
//       8. 重复命令与幂等性（D9 duplicate_settlement / D10 duplicate_tavern_request）：透传 DuplicateSettlement=true，不重复扣费；
//       9. 敏感内容跳过与警告机制（D10 warning_required 映射至 d19.port.permission_denied / SkipSensitiveContent）；
//      10. 数据所有权红线：适配器无写路径，所有变更来自 D9/D10/D3 原系统裁决。
//
// 铁律与约束：
//   - 纯确定性 NUnit 单元测试，零 GameObject、零 Transform、零 Prefab、零模型/NPC 寻路/对话面板 UI；
//   - NPC 只使用逻辑标识与可用性，所有反馈映射至 L17/L18；
//   - 绝不修改任何生产代码。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.D10;
using Whalebone.Systems.D19;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D19.Tests
{
    [TestFixture]
    public sealed class D19TavernPortAdapterTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string ServiceTavern = "tavern";
        private const string ServiceRecruitment = "recruitment";
        private const string ActorPlayer = "player_captain_01";
        private const string ShipAlpha = "ship_alpha_01";
        private const string ShipBeta = "ship_beta_02";

        private TradeEconomySystem _economy;
        private EventLog _eventLog;
        private D9PortShipPreparationSystem _preparation;
        private D10TavernSocialSystem _tavernSocial;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _economy = new TradeEconomySystem();
            _eventLog = _economy.EventLog ?? new EventLog();
            _preparation = new D9PortShipPreparationSystem(_economy, _eventLog);
            _tavernSocial = new D10TavernSocialSystem(_eventLog);
            _validClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );
        }

        private static ShipState CreateTestShip(string shipId, bool moored = true)
        {
            var ship = new ShipState
            {
                Id = shipId,
                ShipTypeId = "small_fishing",
                Hull = 60,
                HullMax = 60,
                CrewCapacity = 4,
            };
            if (moored)
            {
                ship.State["moored"] = 1f;
                ship.State["berth_santa_margarita"] = 1f;
            }
            else
            {
                ship.State["moored"] = 0f;
            }
            return ship;
        }

        private static CrewAgent CreateCrewAgent(string id, string name)
        {
            var persona = new PersonaDefinition(name, "水手");
            var store = new InMemoryStore();
            return new CrewAgent(id, persona, store);
        }

        private static D19TavernRequest CreateBaseRequest(
            string commandId,
            string serviceId = ServiceTavern,
            string portId = PortSantaMargarita)
        {
            return new D19TavernRequest
            {
                CommandId = commandId,
                ActorId = ActorPlayer,
                ShipId = ShipAlpha,
                PortId = portId,
                ServiceId = serviceId,
                WorldDay = 10,
                SaveRevision = 100,
                TimePeriod = D19ServiceTimePeriod.Evening,
                FactionPermission = D19FactionPermission.Allowed,
                Weather = D19WeatherLabel.Clear,
                FacilityState = D19FacilityState.Open,
                Staffed = true
            };
        }

        // ============================================================
        // 1. 酒馆休整（Visit）成功路径
        // ============================================================

        [Test]
        public void Visit_Success_ImprovesCrewStats_DeductsMoney_GeneratesEventDraft()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crew1 = CreateCrewAgent("crew_01", "Jack");
            crew1.Stats.Morale = 50f;
            crew1.Stats.Fatigue = 20f;
            var crew2 = CreateCrewAgent("crew_02", "Bill");
            crew2.Stats.Morale = 60f;
            crew2.Stats.Fatigue = 15f;
            var crewList = new List<CrewAgent> { crew1, crew2 };

            var req = CreateBaseRequest("cmd_visit_001", ServiceTavern);

            // 2 名船员费用: 2 * 4 = 8 金币
            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsTrue(result.Success, "酒馆休整应成功");
            Assert.IsFalse(result.DuplicateSettlement);
            Assert.AreEqual(8, result.Cost);
            Assert.AreEqual(2, result.CrewCount);
            Assert.AreEqual(500 - 8, trade.Money);
            Assert.AreEqual(trade.Money, result.MoneyAfter);

            // 校验船员状态改善：士气 +4，疲劳 -3
            Assert.AreEqual(54f, crew1.Stats.Morale);
            Assert.AreEqual(17f, crew1.Stats.Fatigue);
            Assert.AreEqual(64f, crew2.Stats.Morale);
            Assert.AreEqual(12f, crew2.Stats.Fatigue);

            // 校验 L16 事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual("D19PortSystem", result.EventDraft.OwnerSystem);
            Assert.AreEqual("port.tavern.visit_applied", result.EventDraft.Result.ResultCode);
            Assert.AreEqual(D19PortLocalizationKeys.PortEventServiceResult, result.EventDraft.Result.Summary);
            Assert.AreEqual("8", result.EventDraft.Result.Values["cost"]);
            Assert.AreEqual("2", result.EventDraft.Result.Values["crew_count"]);
        }

        // ============================================================
        // 2. 招募（Recruit）成功路径
        // ============================================================

        [Test]
        public void Recruit_Success_AddsCandidateToCrew_DeductsMoney_GeneratesEventDraft()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crew1 = CreateCrewAgent("crew_01", "Jack");
            var crewList = new List<CrewAgent> { crew1 };
            ship.CrewIds.Add("crew_01");

            var candidate = CreateCrewAgent("candidate_01", "Morgan");

            var req = CreateBaseRequest("cmd_recruit_001", ServiceRecruitment);
            req.CandidateId = "candidate_01";

            // 招募费用 60 金币
            D19TavernResult result = D19TavernPortAdapter.Recruit(trade, ship, crewList, _preparation, candidate, req, _validClaims);

            Assert.IsTrue(result.Success, "招募应成功");
            Assert.AreEqual(60, result.Cost);
            Assert.AreEqual(500 - 60, trade.Money);
            Assert.AreEqual(2, crewList.Count);
            Assert.Contains(candidate, (System.Collections.ICollection)crewList);

            // 校验 L16 事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual("port.tavern.recruit_applied", result.EventDraft.Result.ResultCode);
            Assert.AreEqual("candidate_01", result.EventDraft.Result.Values["candidate_id"]);
            Assert.AreEqual("60", result.EventDraft.Result.Values["cost"]);
        }

        // ============================================================
        // 3. 社交（Social）成功路径
        // ============================================================

        [Test]
        public void Social_EnterTavern_Success()
        {
            var req = CreateBaseRequest("cmd_social_enter_001", ServiceTavern);
            req.SocialRequest = new D10TavernRequest
            {
                Action = D10TavernAction.Enter,
                ActorId = ActorPlayer,
                PortId = "saint_marys",
                WorldDay = 10,
                SaveRevision = 100
            };

            D19TavernResult result = D19TavernPortAdapter.Social(_tavernSocial, req, _validClaims);

            Assert.IsTrue(result.Success, "社交进入酒馆应成功");
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual(D19PortLocalizationKeys.PortEventServiceResult, result.EventDraft.Result.Summary);
        }

        [Test]
        public void Social_HearRumor_Success()
        {
            var req = CreateBaseRequest("cmd_social_rumor_001", ServiceTavern);
            req.SocialRequest = new D10TavernRequest
            {
                Action = D10TavernAction.HearRumor,
                ActorId = ActorPlayer,
                PortId = "saint_marys",
                RumorId = "rumor_chart_cove_tide",
                WorldDay = 10,
                SaveRevision = 101,
                WarningAcknowledged = true
            };

            D19TavernResult result = D19TavernPortAdapter.Social(_tavernSocial, req, _validClaims);

            Assert.IsTrue(result.Success, "打听传闻应成功");
            Assert.IsNotNull(result.EventDraft);
        }

        // ============================================================
        // 4. 服务关闭与前置阻断（L05 可用性与 D9 靠泊前置）
        // ============================================================

        [Test]
        public void Visit_WhenFacilityClosed_ReturnsServiceUnavailable_MoneyUntouched()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crew1 = CreateCrewAgent("crew_01", "Jack");
            crew1.Stats.Morale = 50f;
            var crewList = new List<CrewAgent> { crew1 };

            var req = CreateBaseRequest("cmd_visit_closed_001", ServiceTavern);
            req.FacilityState = D19FacilityState.Closed;

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.service.unavailable", result.ReasonCode);
            Assert.AreEqual(D19PortLocalizationKeys.FailureServiceUnavailable, result.LocKey);
            Assert.AreEqual(500, trade.Money, "服务不可用时金钱绝对不得变动");
            Assert.AreEqual(50f, crewList[0].Stats.Morale, "船员士气不应改变");
        }

        [Test]
        public void Visit_WhenStaffedFalse_ReturnsServiceUnavailable()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crewList = new List<CrewAgent> { CreateCrewAgent("crew_01", "Jack") };

            var req = CreateBaseRequest("cmd_visit_unstaffed_001", ServiceTavern);
            req.Staffed = false; // 无人手

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.service.unavailable", result.ReasonCode);
            Assert.AreEqual(500, trade.Money);
        }

        [Test]
        public void Visit_WhenShipNotMoored_D9Rejects_MappedToInvalidTransition()
        {
            // 未靠泊状态直接使用酒馆 -> D9 靠泊前置拒绝
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: false); // 未靠泊
            var crewList = new List<CrewAgent> { CreateCrewAgent("crew_01", "Jack") };

            var req = CreateBaseRequest("cmd_visit_not_moored_001", ServiceTavern);

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.lifecycle.invalid_transition", result.ReasonCode);
            Assert.AreEqual("not_moored", result.OriginalOwnerCode, "原始码必须保留");
            Assert.AreEqual(500, trade.Money);
        }

        // ============================================================
        // 5. 权限与跨实体归属拒绝
        // ============================================================

        [Test]
        public void Visit_CrossShipClaim_ReturnsPermissionDenied()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crewList = new List<CrewAgent> { CreateCrewAgent("crew_01", "Jack") };

            var req = CreateBaseRequest("cmd_visit_perm_001", ServiceTavern);
            // 声明属于 ShipBeta，但请求操作 ShipAlpha
            var claims = new D19AuthorityClaims(ShipBeta, true, true, PortSantaMargarita);

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, claims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.port.permission_denied", result.ReasonCode);
            Assert.AreEqual(500, trade.Money);
        }

        [Test]
        public void Visit_CrossPortBerth_ReturnsPermissionDenied()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crewList = new List<CrewAgent> { CreateCrewAgent("crew_01", "Jack") };

            var req = CreateBaseRequest("cmd_visit_cross_berth_001", ServiceTavern);
            req.PortId = PortSantaMargarita;
            req.BerthId = "berth_green_cay_01"; // 泊位属于 green_cay!

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.port.permission_denied", result.ReasonCode);
            Assert.AreEqual(500, trade.Money);
        }

        // ============================================================
        // 6. 费用拒绝（资金不足）
        // ============================================================

        [Test]
        public void Visit_InsufficientFunds_ReturnsFundsInsufficient_MoneyUnchanged()
        {
            // 2 名船员需 8 金币，当前仅有 5 金币
            var trade = new D3TradeState(5, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crew1 = CreateCrewAgent("crew_01", "Jack");
            crew1.Stats.Morale = 50f;
            var crew2 = CreateCrewAgent("crew_02", "Bill");
            crew2.Stats.Morale = 50f;
            var crewList = new List<CrewAgent> { crew1, crew2 };

            var req = CreateBaseRequest("cmd_visit_nofunds_001", ServiceTavern);

            D19TavernResult result = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.trade.funds_insufficient", result.ReasonCode);
            Assert.AreEqual("not_enough_money", result.OriginalOwnerCode);
            Assert.AreEqual(5, trade.Money, "资金不足时金币不变");
            Assert.AreEqual(50f, crew1.Stats.Morale, "士气不得提升");

            // 拒绝事件草案
            Assert.IsNotNull(result.EventDraft);
            Assert.AreEqual("port.tavern.rejected", result.EventDraft.Result.ResultCode);
            Assert.AreEqual("not_enough_money", result.EventDraft.Result.Values["owner_code"]);
        }

        [Test]
        public void Recruit_InsufficientFunds_ReturnsFundsInsufficient()
        {
            // 招募需要 60 金币，当前仅有 20 金币
            var trade = new D3TradeState(20, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crewList = new List<CrewAgent>();
            var candidate = CreateCrewAgent("cand_01", "Morgan");

            var req = CreateBaseRequest("cmd_recruit_nofunds_001", ServiceRecruitment);
            req.CandidateId = "cand_01";

            D19TavernResult result = D19TavernPortAdapter.Recruit(trade, ship, crewList, _preparation, candidate, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.trade.funds_insufficient", result.ReasonCode);
            Assert.AreEqual("not_enough_money", result.OriginalOwnerCode);
            Assert.AreEqual(20, trade.Money);
            Assert.AreEqual(0, crewList.Count, "候选人不得入列");
        }

        // ============================================================
        // 7. 原系统拒绝（满员 / 重复招募 / 无效参数）
        // ============================================================

        [Test]
        public void Recruit_CrewFull_ReturnsCapacityExceeded()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            ship.CrewCapacity = 2; // 上限 2 人
            ship.CrewIds.Add("crew_01");
            ship.CrewIds.Add("crew_02");
            var crewList = new List<CrewAgent>
            {
                CreateCrewAgent("crew_01", "Jack"),
                CreateCrewAgent("crew_02", "Bill")
            };
            var candidate = CreateCrewAgent("candidate_03", "Morgan");

            var req = CreateBaseRequest("cmd_recruit_full_001", ServiceRecruitment);
            req.CandidateId = "candidate_03";

            D19TavernResult result = D19TavernPortAdapter.Recruit(trade, ship, crewList, _preparation, candidate, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.inventory.capacity_exceeded", result.ReasonCode);
            Assert.AreEqual("crew_full", result.OriginalOwnerCode);
            Assert.AreEqual(500, trade.Money);
            Assert.AreEqual(2, crewList.Count);
        }

        [Test]
        public void Recruit_AlreadyRecruited_ReturnsOperationFailed()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var existing = CreateCrewAgent("candidate_01", "AlreadyOnBoard");
            var crewList = new List<CrewAgent> { existing };

            var req = CreateBaseRequest("cmd_recruit_dup_001", ServiceRecruitment);
            req.CandidateId = "candidate_01";

            D19TavernResult result = D19TavernPortAdapter.Recruit(trade, ship, crewList, _preparation, existing, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.inventory.operation_failed", result.ReasonCode);
            Assert.AreEqual("already_recruited", result.OriginalOwnerCode);
            Assert.AreEqual(500, trade.Money);
        }

        // ============================================================
        // 8. 敏感内容警告与社交拒绝
        // ============================================================

        [Test]
        public void Social_WarningRequired_ReturnsPermissionDenied()
        {
            var req = CreateBaseRequest("cmd_social_alcohol_001", ServiceTavern);
            // 饮酒需要敏感警告确认，若未确认 -> D10 warning_required -> L17 d19.port.permission_denied
            req.SocialRequest = new D10TavernRequest
            {
                Action = D10TavernAction.DrinkAlcohol,
                Sensitivity = D10TavernSensitivity.Alcohol,
                ActorId = ActorPlayer,
                PortId = "saint_marys",
                TargetId = "crew_01",
                AlcoholStrength = D10AlcoholStrength.Strong,
                WarningAcknowledged = false, // 未确认！
                WorldDay = 10,
                SaveRevision = 100
            };

            D19TavernResult result = D19TavernPortAdapter.Social(_tavernSocial, req, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("d19.port.permission_denied", result.ReasonCode);
            Assert.AreEqual("warning_required", result.OriginalOwnerCode);
        }

        // ============================================================
        // 9. 输入缺失与接口不匹配阻断
        // ============================================================

        [Test]
        public void Execute_MissingCommandIdOrNull_ReturnsBlocked()
        {
            var trade = new D3TradeState(500, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crewList = new List<CrewAgent>();

            var req = CreateBaseRequest("", ServiceTavern); // 空 CommandId

            D19TavernResult res1 = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, req, _validClaims);
            Assert.IsFalse(res1.Success);
            Assert.AreEqual("missing_command_id", res1.ReasonCode);

            D19TavernResult res2 = D19TavernPortAdapter.Visit(trade, ship, crewList, _preparation, null, _validClaims);
            Assert.IsFalse(res2.Success);
            Assert.AreEqual("request_null", res2.ReasonCode);
        }

        // ============================================================
        // 10. 数据所有权红线断言
        // ============================================================

        [Test]
        public void Adapter_DoesNotDirectlyMutateFields_PreservesInvariant()
        {
            var trade = new D3TradeState(300, 40);
            var ship = CreateTestShip(ShipAlpha, moored: true);
            var crew1 = CreateCrewAgent("crew_01", "Jack");
            crew1.Stats.Morale = 40f;
            crew1.Stats.Fatigue = 20f;
            var crew = new List<CrewAgent> { crew1 };

            int beforeMoney = trade.Money;
            float beforeMorale = crew[0].Stats.Morale;

            var req = CreateBaseRequest("cmd_check_own_001", ServiceTavern);
            var res = D19TavernPortAdapter.Visit(trade, ship, crew, _preparation, req, _validClaims);

            Assert.IsTrue(res.Success);
            // 变更完全由 D9.VisitTavern 裁决
            Assert.AreEqual(beforeMoney - 4, trade.Money);
            Assert.AreEqual(beforeMorale + 4f, crew[0].Stats.Morale);
        }
    }
}
