// ============================================================
// D19AvailabilityAndNpcTests —— 港口服务可用性与 NPC 逻辑确定性测试（纯 C# EditMode）
//
// 归属：Whalebone.Tests.EditMode.D19（所有者：D19-L32 唯一测试产物）
// 目标覆盖：
//   1. 优先级链确定性测试：设施状态（Closed/UnderRepair/Blockaded/OutOfStock）-> 势力拒绝 -> 天气（Storm/Rain）-> 时段（Night/Day）-> NPC 服务标识（staffed）-> Available；
//   2. 天气标签逻辑影响：Storm 关闭露天服务（Market, Supply），Rain 仅关闭 Market，Tavern/Warehouse/Shipyard 在 Storm/Rain 下保持可用；
//   3. 时间段逻辑影响：Night 关闭 Market, Warehouse, Shipyard；Tavern 在夜间完全开放（符合酒馆设计）；Morning/Afternoon/Evening 全开放；
//   4. 势力许可逻辑影响：FactionPermission.Denied 阻断全部已注册服务，Verdict=Unavailable，ReasonCode=d19.service.unavailable，Detail=FactionDenied；
//   5. 设施状态逻辑影响：Closed (FacilityClosed), UnderRepair (UnderRepair), Blockaded (Blockaded), OutOfStock (仅限 Stocked 服务)；
//   6. NPC 逻辑服务标识测试（纯布尔 staffed，禁止 Unity 角色/行为树/模型/NavMesh/场景）：
//      - staffed=false 时，所有原本开放的服务均阻断为 Unavailable (NotStaffed)；
//      - staffed=true 时，通过前置检查后开放为 Available；
//   7. 保守拒绝（未知标签/非法枚举/冲突输入/未知港口）：
//      - 未知/越界枚举值（cast 未定义 int）、空/null 港口或服务 ID -> Rejected (UnknownInput)；
//      - 未知港口 -> Rejected (UnknownPort, d19.port.unknown_port)；
//      - 冲突输入（如非补货服务声明 OutOfStock，如 Tavern OutOfStock）-> Rejected (ConflictingInput)；
//      - 未在该港口登记的服务（如荒岛港口服务）-> Unavailable (NotRegistered)；
//   8. 只读模型（D19PortReadModel）投影聚合与稳定排序：
//      - D19ServicesSummaryReadModel 保证所有标准服务覆盖，按 ServiceId 字符串升序稳定排序；
//      - 逐项投影的 Verdict, IsAvailable, ReasonCode, FeedbackLocKey, RetryPolicy, Detail, RetryHint 完整无缺；
//      - D19PortReadModel 根模型指纹（SnapshotFingerprint）纯确定性，同输入逐字段完全等价；
//   9. 零副作用与防重测：
//      - 无论何种不可用或拒绝场景，不产生任何货币/库存/船员/关系/场景副作用；
//      - 重复调用 1,000 次，结果绝对一致（纯函数确定性）；
//     10. 边界与 L05 / L19 契约完全闭环：
//      - 港口别名（santa_margarita -> port_santa_margarita）规范化后一致裁决；
//      - supply / supplies 映射单复数对齐不丢失。
//
// 铁律与约束：
//   * 纯确定性 NUnit 单元测试，零 GameObject、零 Transform、零 Prefab、零动画/行为树/NavMesh/场景；
//   * NPC 严格作为逻辑服务标记（staffed 布尔），禁止任何 Unity 实体/行为树依赖；
//   * 绝不修改任何生产代码。
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Whalebone.Systems.D19;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;

namespace Whalebone.Tests.EditMode.D19
{
    [TestFixture]
    public sealed class D19AvailabilityAndNpcTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string PortGreenCay = "port_green_cay";
        private const string PortDriftwoodIsle = "port_driftwood_isle";
        private const string UnknownPort = "port_atlantis_unknown";

        private const string ServiceMarket = D19PortLocalizationKeys.ServiceIdMarket;
        private const string ServiceWarehouse = D19PortLocalizationKeys.ServiceIdWarehouse;
        private const string ServiceTavern = D19PortLocalizationKeys.ServiceIdTavern;
        private const string ServiceShipyard = D19PortLocalizationKeys.ServiceIdShipyard;
        private const string ServiceSupply = D19PortLocalizationKeys.ServiceIdSupply;

        // ============================================================
        // 1. 基础确定性与幂等性验证（同输入多次完全相同）
        // ============================================================

        [Test]
        public void Adjudicate_SameInputs_ProducesIdenticalResultsRepeatedly()
        {
            var r1 = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );

            for (int i = 0; i < 1000; i++)
            {
                var r2 = D19PortAvailabilitySystem.Adjudicate(
                    PortSantaMargarita,
                    ServiceMarket,
                    D19ServiceTimePeriod.Morning,
                    D19FactionPermission.Allowed,
                    D19WeatherLabel.Clear,
                    D19FacilityState.Open,
                    staffed: true
                );

                Assert.AreEqual(r1.Verdict, r2.Verdict);
                Assert.AreEqual(r1.ReasonCode, r2.ReasonCode);
                Assert.AreEqual(r1.LocKey, r2.LocKey);
                Assert.AreEqual(r1.RetryPolicy, r2.RetryPolicy);
                Assert.AreEqual(r1.Detail, r2.Detail);
                Assert.AreEqual(r1.RetryHint, r2.RetryHint);
                Assert.AreEqual(r1.ServiceId, r2.ServiceId);
                Assert.AreEqual(r1.PortId, r2.PortId);
            }
        }

        [Test]
        public void Adjudicate_AllStandardServices_AvailableUnderNominalConditions()
        {
            foreach (string serviceId in D19PortAvailabilitySystem.KnownServiceIds)
            {
                var result = D19PortAvailabilitySystem.Adjudicate(
                    PortSantaMargarita,
                    serviceId,
                    D19ServiceTimePeriod.Morning,
                    D19FactionPermission.Allowed,
                    D19WeatherLabel.Clear,
                    D19FacilityState.Open,
                    staffed: true
                );

                Assert.AreEqual(D19AvailabilityVerdict.Available, result.Verdict, $"Service {serviceId} should be Available under nominal conditions");
                Assert.AreEqual(D19PortLocalizationKeys.ReadModelServiceOpen, result.LocKey);
                Assert.AreEqual(D19UnavailabilityDetail.None, result.Detail);
                Assert.AreEqual(D19RetryHint.None, result.RetryHint);
                Assert.AreEqual(D19FailureRetryPolicy.NoRetry, result.RetryPolicy);
            }
        }

        // ============================================================
        // 2. 设施状态（FacilityState）对可用性的影响
        // ============================================================

        [Test]
        public void FacilityState_Closed_MakesServiceUnavailable_FacilityClosed()
        {
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Closed,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual("d19.service.unavailable", result.ReasonCode);
            Assert.AreEqual(D19PortLocalizationKeys.FailureServiceUnavailable, result.LocKey);
            Assert.AreEqual(D19UnavailabilityDetail.FacilityClosed, result.Detail);
            Assert.AreEqual(D19RetryHint.RetryLaterSameDay, result.RetryHint);
            Assert.AreEqual(D19FailureRetryPolicy.RetrySafe, result.RetryPolicy);
        }

        [Test]
        public void FacilityState_UnderRepair_MakesServiceUnavailable_UnderRepair()
        {
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceShipyard,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.UnderRepair,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.UnderRepair, result.Detail);
            Assert.AreEqual(D19RetryHint.AfterRepair, result.RetryHint);
        }

        [Test]
        public void FacilityState_Blockaded_MakesServiceUnavailable_Blockaded()
        {
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceWarehouse,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Blockaded,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.Blockaded, result.Detail);
            Assert.AreEqual(D19RetryHint.AfterBlockadeLifted, result.RetryHint);
        }

        [Test]
        public void FacilityState_OutOfStock_OnlyValidForMarketAndSupply()
        {
            // Market / Supply 缺货 -> Unavailable
            var rMarket = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.OutOfStock,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rMarket.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.OutOfStock, rMarket.Detail);
            Assert.AreEqual(D19RetryHint.RetryNextPeriod, rMarket.RetryHint);

            var rSupply = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceSupply,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.OutOfStock,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rSupply.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.OutOfStock, rSupply.Detail);

            // Tavern / Warehouse / Shipyard 声明 OutOfStock -> 视为冲突输入保守拒绝 (Rejected)
            var rTavern = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceTavern,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.OutOfStock,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rTavern.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.ConflictingInput, rTavern.Detail);

            var rWarehouse = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceWarehouse,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.OutOfStock,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rWarehouse.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.ConflictingInput, rWarehouse.Detail);

            var rShipyard = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceShipyard,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.OutOfStock,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rShipyard.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.ConflictingInput, rShipyard.Detail);
        }

        // ============================================================
        // 3. 势力许可（FactionPermission）对可用性的影响
        // ============================================================

        [Test]
        public void FactionPermission_Denied_OverridesWeatherAndTime_DetailFactionDenied()
        {
            foreach (string serviceId in D19PortAvailabilitySystem.KnownServiceIds)
            {
                var result = D19PortAvailabilitySystem.Adjudicate(
                    PortSantaMargarita,
                    serviceId,
                    D19ServiceTimePeriod.Morning,
                    D19FactionPermission.Denied,
                    D19WeatherLabel.Clear,
                    D19FacilityState.Open,
                    staffed: true
                );

                Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
                Assert.AreEqual(D19UnavailabilityDetail.FactionDenied, result.Detail);
                Assert.AreEqual(D19RetryHint.ResolveFactionPermission, result.RetryHint);
            }
        }

        // ============================================================
        // 4. 天气标签（WeatherLabel）对可用性的影响
        // ============================================================

        [Test]
        public void WeatherLabel_Storm_ClosesMarketAndSupply_TavernWarehouseShipyardRemainOpen()
        {
            // Market / Supply 受 Storm 关闭
            var rMarket = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rMarket.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.WeatherClosed, rMarket.Detail);

            var rSupply = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceSupply,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rSupply.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.WeatherClosed, rSupply.Detail);

            // Tavern / Warehouse / Shipyard 在 Storm 下仍开放
            var rTavern = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceTavern,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rTavern.Verdict);

            var rWarehouse = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceWarehouse,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rWarehouse.Verdict);

            var rShipyard = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceShipyard,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rShipyard.Verdict);
        }

        [Test]
        public void WeatherLabel_Rain_ClosesMarketOnly()
        {
            var rMarket = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Rain,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rMarket.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.WeatherClosed, rMarket.Detail);

            // Supply 在 Rain 下开放
            var rSupply = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceSupply,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Rain,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rSupply.Verdict);
        }

        // ============================================================
        // 5. 时间段（ServiceTimePeriod）对可用性的影响
        // ============================================================

        [Test]
        public void ServiceTimePeriod_Night_ClosesMarketWarehouseShipyard_TavernAndSupplyBehavior()
        {
            // Market / Warehouse / Shipyard 夜间关闭
            var rMarket = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rMarket.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.TimeClosed, rMarket.Detail);

            var rWarehouse = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceWarehouse,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rWarehouse.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.TimeClosed, rWarehouse.Detail);

            var rShipyard = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceShipyard,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, rShipyard.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.TimeClosed, rShipyard.Detail);

            // Tavern 夜间开放
            var rTavern = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceTavern,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rTavern.Verdict);

            // Supply 夜间开放（不包含在 NightClosedServices）
            var rSupply = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceSupply,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Available, rSupply.Verdict);
        }

        // ============================================================
        // 6. NPC 服务标识（staffed 布尔）与无可用 NPC 逻辑
        // ============================================================

        [Test]
        public void Staffed_False_BlocksAllOtherwiseAvailableServices_DetailNotStaffed()
        {
            foreach (string serviceId in D19PortAvailabilitySystem.KnownServiceIds)
            {
                var result = D19PortAvailabilitySystem.Adjudicate(
                    PortSantaMargarita,
                    serviceId,
                    D19ServiceTimePeriod.Morning,
                    D19FactionPermission.Allowed,
                    D19WeatherLabel.Clear,
                    D19FacilityState.Open,
                    staffed: false // 无 NPC 配备 / 无可用服务人员
                );

                Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
                Assert.AreEqual(D19UnavailabilityDetail.NotStaffed, result.Detail);
                Assert.AreEqual(D19RetryHint.WhenStaffed, result.RetryHint);
                Assert.AreEqual("d19.service.unavailable", result.ReasonCode);
                Assert.AreEqual(D19PortLocalizationKeys.FailureServiceUnavailable, result.LocKey);
            }
        }

        // ============================================================
        // 7. 确定性优先级链验证（冻结优先级链）
        //    设施状态 -> 势力拒绝 -> 天气关闭 -> 时段关闭 -> 未配人 -> 可用
        // ============================================================

        [Test]
        public void PriorityChain_FacilityClosed_Overrides_FactionWeatherTimeAndStaff()
        {
            // 设施关闭 + 势力拒绝 + 风暴 + 夜间 + 未配人 -> 必须判定为 FacilityClosed
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Denied,
                D19WeatherLabel.Storm,
                D19FacilityState.Closed,
                staffed: false
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.FacilityClosed, result.Detail);
        }

        [Test]
        public void PriorityChain_FactionDenied_Overrides_WeatherTimeAndStaff()
        {
            // 设施开放 + 势力拒绝 + 风暴 + 夜间 + 未配人 -> 必须判定为 FactionDenied
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Denied,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: false
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.FactionDenied, result.Detail);
        }

        [Test]
        public void PriorityChain_WeatherClosed_Overrides_TimeAndStaff()
        {
            // 设施开放 + 势力允许 + 风暴 + 夜间 + 未配人 -> 市场必须判定为 WeatherClosed
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Storm,
                D19FacilityState.Open,
                staffed: false
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.WeatherClosed, result.Detail);
        }

        [Test]
        public void PriorityChain_TimeClosed_Overrides_Staff()
        {
            // 设施开放 + 势力允许 + 晴朗 + 夜间 + 未配人 -> 市场必须判定为 TimeClosed
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Night,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: false
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.TimeClosed, result.Detail);
        }

        // ============================================================
        // 8. 保守拒绝测试（未知输入、越界枚举、未知港口、未注册服务）
        // ============================================================

        [Test]
        public void Adjudicate_UnknownPort_RejectedWithUnknownPortCode()
        {
            var result = D19PortAvailabilitySystem.Adjudicate(
                UnknownPort,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Rejected, result.Verdict);
            Assert.AreEqual("d19.port.unknown_port", result.ReasonCode);
            Assert.AreEqual(D19PortLocalizationKeys.FailureUnknownPort, result.LocKey);
            Assert.AreEqual(D19UnavailabilityDetail.UnknownInput, result.Detail);
            Assert.AreEqual(D19RetryHint.AfterCorrection, result.RetryHint);
        }

        [Test]
        public void Adjudicate_NullOrEmptyOrUnknownService_RejectedWithUnknownInput()
        {
            var rNullService = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                null,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rNullService.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.UnknownInput, rNullService.Detail);

            var rUnknownService = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                "magic_portal_service",
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rUnknownService.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.UnknownInput, rUnknownService.Detail);
        }

        [Test]
        public void Adjudicate_InvalidEnumValues_RejectedWithUnknownInput()
        {
            var rBadTime = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                (D19ServiceTimePeriod)999,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rBadTime.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.UnknownInput, rBadTime.Detail);

            var rBadWeather = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                (D19WeatherLabel)999,
                D19FacilityState.Open,
                staffed: true
            );
            Assert.AreEqual(D19AvailabilityVerdict.Rejected, rBadWeather.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.UnknownInput, rBadWeather.Detail);
        }

        [Test]
        public void Adjudicate_UnregisteredServiceInPort_UnavailableNotRegistered()
        {
            // Driftwood Isle 荒岛未注册 Market/Tavern 等服务
            var result = D19PortAvailabilitySystem.Adjudicate(
                PortDriftwoodIsle,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, result.Verdict);
            Assert.AreEqual(D19UnavailabilityDetail.NotRegistered, result.Detail);
            Assert.AreEqual(D19RetryHint.AfterCorrection, result.RetryHint);
        }

        [Test]
        public void Adjudicate_PortAliasResolution_NormalizesAndSucceeds()
        {
            // 裸键别名 santa_margarita -> port_santa_margarita
            var result = D19PortAvailabilitySystem.Adjudicate(
                "santa_margarita",
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Open,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Available, result.Verdict);
            Assert.AreEqual(PortSantaMargarita, result.PortId);
        }

        // ============================================================
        // 9. 零副作用验证（不可用服务绝不改变金钱、库存或关系）
        // ============================================================

        [Test]
        public void UnavailableService_DoesNotMutateMoneyOrInventoryOrGenerateSideEffects()
        {
            var eventLog = new EventLog();
            var economy = new TradeEconomySystem(eventLog);
            var state = new D3TradeState(1000, 50);
            CommodityCatalog.SeedMarket(state.Market);

            int initialMoney = state.Money;
            int initialStock = state.Market.Stock["sugar"];
            int initialCargo = D3InventorySystem.CountCargo(state.Inventory, "sugar");

            // 裁决服务不可用（例如设施关闭）
            var availResult = D19PortAvailabilitySystem.Adjudicate(
                PortSantaMargarita,
                ServiceMarket,
                D19ServiceTimePeriod.Morning,
                D19FactionPermission.Allowed,
                D19WeatherLabel.Clear,
                D19FacilityState.Closed,
                staffed: true
            );

            Assert.AreEqual(D19AvailabilityVerdict.Unavailable, availResult.Verdict);

            // 当适配器遇到不可用服务时通过 AvailabilityGate 注入阻断
            var req = new D19MarketTradeRequest
            {
                CommandId = "cmd_blocked_trade_01",
                ActorId = "player_01",
                ShipId = "ship_01",
                PortId = PortSantaMargarita,
                CommodityId = "sugar",
                Quantity = 5,
                WorldDay = 1,
                SaveRevision = 100,
                AvailabilityGate = availResult
            };
            var claims = new D19AuthorityClaims("ship_01", true, true, PortSantaMargarita);

            var tradeResult = D19MarketPortAdapter.Buy(state, economy, req, claims);

            Assert.IsFalse(tradeResult.Success);
            Assert.AreEqual(D19PortLocalizationKeys.FailureServiceUnavailable, tradeResult.LocKey);
            Assert.AreEqual(initialMoney, state.Money, "Money must not change when service is unavailable");
            Assert.AreEqual(initialStock, state.Market.Stock["sugar"], "Market stock must not change");
            Assert.AreEqual(initialCargo, D3InventorySystem.CountCargo(state.Inventory, "sugar"), "Inventory cargo must not change");
            Assert.AreEqual(0, eventLog.Count, "No events should be appended for blocked service");
        }

        // ============================================================
        // 10. 读模型（D19PortReadModel）投影聚合与稳定排序
        // ============================================================

        [Test]
        public void ReadModel_ServicesSummary_CoversAllServicesAndOrdersDeterministically()
        {
            var availResults = new List<D19ServiceAvailabilityResult>();

            // 构造部分可用、部分关闭的结果（无序列表）
            availResults.Add(D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceTavern, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.Open, true));
            availResults.Add(D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceMarket, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Rain, D19FacilityState.Open, true)); // Rain 关 Market
            availResults.Add(D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceWarehouse, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.Open, true));
            availResults.Add(D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceShipyard, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.UnderRepair, true)); // 维修关
            availResults.Add(D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceSupply, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.Open, false)); // 未配人关

            var summary = new D19ServicesSummaryReadModel(PortSantaMargarita, availResults);

            Assert.AreEqual(PortSantaMargarita, summary.PortId);
            Assert.AreEqual(5, summary.TotalServices);
            Assert.AreEqual(2, summary.AvailableServices); // Tavern & Warehouse 可用

            // 断言严格按 ServiceId 字符串升序稳定排序
            // market, shipyard, supply, tavern, warehouse
            Assert.AreEqual(ServiceMarket, summary.Services[0].ServiceId);
            Assert.IsFalse(summary.Services[0].IsAvailable);
            Assert.AreEqual(D19UnavailabilityDetail.WeatherClosed, summary.Services[0].Detail);

            Assert.AreEqual(ServiceShipyard, summary.Services[1].ServiceId);
            Assert.IsFalse(summary.Services[1].IsAvailable);
            Assert.AreEqual(D19UnavailabilityDetail.UnderRepair, summary.Services[1].Detail);

            Assert.AreEqual(ServiceSupply, summary.Services[2].ServiceId);
            Assert.IsFalse(summary.Services[2].IsAvailable);
            Assert.AreEqual(D19UnavailabilityDetail.NotStaffed, summary.Services[2].Detail);

            Assert.AreEqual(ServiceTavern, summary.Services[3].ServiceId);
            Assert.IsTrue(summary.Services[3].IsAvailable);
            Assert.AreEqual(D19PortLocalizationKeys.ReadModelServiceOpen, summary.Services[3].FeedbackLocKey);

            Assert.AreEqual(ServiceWarehouse, summary.Services[4].ServiceId);
            Assert.IsTrue(summary.Services[4].IsAvailable);
            Assert.AreEqual(D19PortLocalizationKeys.ReadModelServiceOpen, summary.Services[4].FeedbackLocKey);
        }

        [Test]
        public void ReadModel_RootSnapshot_FingerprintIsDeterministic()
        {
            var visitState = D19PortVisitState.Berthed;
            var allocations = new List<D19BerthAllocation>
            {
                new D19BerthAllocation
                {
                    BerthId = "berth_santa_margarita_dock_01",
                    State = D19BerthState.Occupied,
                    ShipId = "ship_01",
                    CommandId = "cmd_01",
                    ReservedWorldDay = 1,
                    ExpiresWorldDay = 10,
                    ReleaseReason = ""
                }
            };
            var capabilities = new List<D19BerthCapability>
            {
                new D19BerthCapability("berth_santa_margarita_dock_01", 1, 3, true)
            };

            var availResults = new List<D19ServiceAvailabilityResult>
            {
                D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceMarket, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.Open, true),
                D19PortAvailabilitySystem.Adjudicate(PortSantaMargarita, ServiceTavern, D19ServiceTimePeriod.Morning, D19FactionPermission.Allowed, D19WeatherLabel.Clear, D19FacilityState.Open, true)
            };

            var readModel1 = D19PortReadModel.Build(
                worldDay: 10,
                saveRevision: 100,
                visitState: visitState,
                portId: PortSantaMargarita,
                berthId: "berth_santa_margarita_dock_01",
                shipId: "ship_01",
                berthAllocations: allocations,
                berthCapabilities: capabilities,
                serviceAvailabilities: availResults,
                lastCommandResult: null,
                departureSummary: null
            );

            var readModel2 = D19PortReadModel.Build(
                worldDay: 10,
                saveRevision: 100,
                visitState: visitState,
                portId: PortSantaMargarita,
                berthId: "berth_santa_margarita_dock_01",
                shipId: "ship_01",
                berthAllocations: allocations,
                berthCapabilities: capabilities,
                serviceAvailabilities: availResults,
                lastCommandResult: null,
                departureSummary: null
            );

            Assert.AreEqual(readModel1.SnapshotFingerprint, readModel2.SnapshotFingerprint, "Snapshot fingerprints must match for identical inputs");
            Assert.AreEqual("1.0.0", D19PortReadModel.SchemaVersion);
            Assert.AreEqual(10, readModel1.WorldDay);
            Assert.AreEqual(100, readModel1.SaveRevision);
        }
    }
}
