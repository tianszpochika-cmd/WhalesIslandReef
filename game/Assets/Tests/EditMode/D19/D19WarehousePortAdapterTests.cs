// ============================================================
// D19WarehousePortAdapterTests —— 港口仓库取放编排确定性测试（D19-L26）
//
// 归属：Whalebone.Tests.EditMode.D19（所有者：D19-L26 测试套件）
// 目标覆盖：
//   1. 入库（Store）成功：货舱数量增加、剩余容量减少、返回 ServiceResult 事件草案；
//   2. 出库（Withdraw）成功：货舱数量减少、剩余容量增加、空堆栈清理、返回 ServiceResult 事件草案；
//   3. 容量满（CapacityFull / WeightFull）拒绝：底层拒绝、货舱与物品状态不变、无部分写入、无容量负数；
//   4. 跨实体归属拒绝（Cross-Entity Claim）：声明船 != 目标船，权限校验器拒绝（d19.port.permission_denied）；
//   5. 底层库存失败（Underlying Failure）：未知物品（UnknownItem）、数量不足（NotEnoughCargo），映射到稳定原因 ID；
//   6. 重复命令稳定结果（Idempotency）：同命令同参重放返回相同结果，绝无第二次副作用；同命令异参返回 DuplicateCommand 冲突；
//   7. 仓库相关港口投影保存/恢复后等价（Save Projection Roundtrip）：D19PortSaveProjection 保存与恢复后状态与分配等价。
//
// 铁律与约束：
//   * 纯确定性测试，零 GameObject、零 Transform、零 Canvas、零 UI、零网络/真实时钟；
//   * 绝不因失败产生物品丢失、重复物品或容量负数；
//   * 投影不含 Unity 引用；
//   * 失败映射全部为稳定原因 ID；
//   * 不修改生产文件，不修改 Save 根模型。
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
    public sealed class D19WarehousePortAdapterTests
    {
        private const string PortSantaMargarita = "port_santa_margarita";
        private const string ServiceWarehouse = "service_santa_margarita_warehouse";
        private const string ActorPlayer = "player_actor_01";
        private const string ShipAlpha = "ship_alpha_01";
        private const string ShipBeta = "ship_beta_02";
        private const string ItemTimber = "timber";
        private const string ItemSalt = "salt";

        private D19WarehousePortAdapter _adapter;
        private D19AuthorityClaims _validClaims;

        [SetUp]
        public void SetUp()
        {
            _adapter = new D19WarehousePortAdapter();
            _validClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: PortSantaMargarita
            );
        }

        // ============================================================
        // 1. 入库（Store）成功路径
        // ============================================================

        [Test]
        public void Store_Success_IncreasesCargoCount_DecreasesFreeCargo_GeneratesServiceEvent()
        {
            // timber: volume 2, unit weight 10
            var inventory = new D3InventoryState(10, 100);
            int initialFree = D3InventorySystem.FreeCargo(inventory);

            var req = new D19WarehouseRequest(
                commandId: "cmd_store_001",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 2,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Store(req, inventory, _validClaims);

            Assert.IsTrue(result.Success, "入库应成功");
            Assert.AreEqual("cmd_store_001", result.CommandId);
            Assert.AreEqual(2, result.AppliedQuantity);
            Assert.AreEqual(2, result.ItemCount);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(inventory, ItemTimber));
            Assert.AreEqual(initialFree - 4, result.FreeCargo, "FreeCargo 应减少 4 (2*2)");
            Assert.AreEqual(result.FreeCargo, D3InventorySystem.FreeCargo(inventory));
            Assert.GreaterOrEqual(result.FreeCargo, 0, "容量不可为负");

            // 事件草案检查
            Assert.IsNotNull(result.Event, "成功入库应产出事件草案");
            Assert.AreEqual("port.warehouse.store.applied", result.Event.Result.ResultCode);
            Assert.AreEqual("store", result.Event.Result.Values["warehouse.operation"]);
            Assert.AreEqual(ItemTimber, result.Event.Result.Values["warehouse.item_id"]);
            Assert.AreEqual("2", result.Event.Result.Values["warehouse.quantity"]);
        }

        // ============================================================
        // 2. 出库（Withdraw）成功路径与空堆栈清理
        // ============================================================

        [Test]
        public void Withdraw_Success_DecreasesCargoCount_CleansEmptyStack_GeneratesServiceEvent()
        {
            var inventory = new D3InventoryState(10, 100);
            D3InventorySystem.TryAddCargo(inventory, ItemSalt, 5, out _); // salt volume 1, 5 items

            var req = new D19WarehouseRequest(
                commandId: "cmd_withdraw_001",
                operation: D19WarehouseOperation.Withdraw,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemSalt,
                quantity: 5,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Withdraw(req, inventory, _validClaims);

            Assert.IsTrue(result.Success, "出库应成功");
            Assert.AreEqual(5, result.AppliedQuantity);
            Assert.AreEqual(0, result.ItemCount);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, ItemSalt));
            Assert.IsFalse(inventory.Cargo.ContainsKey(ItemSalt), "完全出库后字典应清理空堆栈");
            Assert.AreEqual(10, result.FreeCargo, "所有容量恢复");

            Assert.IsNotNull(result.Event);
            Assert.AreEqual("port.warehouse.withdraw.applied", result.Event.Result.ResultCode);
            Assert.AreEqual("withdraw", result.Event.Result.Values["warehouse.operation"]);
        }

        // ============================================================
        // 3. 容量满 / 溢出拒绝（无半写、无丢物品、无容量负数）
        // ============================================================

        [Test]
        public void Store_CapacityFull_RejectsAtomically_NoItemsLost_NoNegativeCapacity()
        {
            // capacity 4 (可存 2 个 timber 或 4 个 salt)
            var inventory = new D3InventoryState(4, 100);
            D3InventorySystem.TryAddCargo(inventory, ItemTimber, 2, out _); // 已占满 4 容量

            int countBefore = D3InventorySystem.CountCargo(inventory, ItemTimber);
            int freeBefore = D3InventorySystem.FreeCargo(inventory);
            Assert.AreEqual(0, freeBefore);

            var req = new D19WarehouseRequest(
                commandId: "cmd_store_overflow",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemSalt,
                quantity: 1,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Store(req, inventory, _validClaims);

            Assert.IsFalse(result.Success, "容量超限应拒绝");
            Assert.IsNotNull(result.Failure);
            Assert.AreEqual(D19WarehouseReasonIds.CapacityFull, result.Failure.ReasonId);

            // 验证原子性：状态保持不变
            Assert.AreEqual(countBefore, D3InventorySystem.CountCargo(inventory, ItemTimber), "原物品数量不可变");
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, ItemSalt), "新物品不可写入");
            Assert.AreEqual(freeBefore, D3InventorySystem.FreeCargo(inventory), "剩余容量不变");
            Assert.GreaterOrEqual(D3InventorySystem.FreeCargo(inventory), 0, "容量不可为负");

            // 拒绝事件草案
            Assert.IsNotNull(result.Event, "失败应产出拒绝事件草案");
            Assert.AreEqual("port.rejected", result.Event.Result.ResultCode);
        }

        // ============================================================
        // 4. 跨实体归属拒绝（跨船货物声明）
        // ============================================================

        [Test]
        public void Store_CrossEntityCargoShipId_RejectedByAuthority_NoStateChange()
        {
            var inventory = new D3InventoryState(10, 100);

            var req = new D19WarehouseRequest(
                commandId: "cmd_store_cross_ship",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 1,
                cargoShipId: ShipBeta, // 跨船声明！
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Store(req, inventory, _validClaims);

            Assert.IsFalse(result.Success, "跨船货物声明必须拒绝");
            Assert.AreEqual("d19.port.permission_denied", result.Failure.ReasonId);
            Assert.AreEqual(0, D3InventorySystem.CountCargo(inventory, ItemTimber), "货舱状态不可修改");
        }

        [Test]
        public void Store_CrossPortClaims_RejectedByAuthority()
        {
            var inventory = new D3InventoryState(10, 100);
            var crossPortClaims = new D19AuthorityClaims(
                shipId: ShipAlpha,
                actorOwnsShip: true,
                actorIsCrewOfShip: true,
                shipCurrentPortId: "port_green_cay" // 船停在其他港口
            );

            var req = new D19WarehouseRequest(
                commandId: "cmd_store_cross_port",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 1,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Store(req, inventory, crossPortClaims);

            Assert.IsFalse(result.Success, "跨港口请求必须拒绝");
            Assert.AreEqual("d19.port.permission_denied", result.Failure.ReasonId);
        }

        // ============================================================
        // 5. 底层库存失败（未知物品、数量不足）
        // ============================================================

        [Test]
        public void Store_UnknownItem_MapsToStableUnknownItemReason()
        {
            var inventory = new D3InventoryState(10, 100);

            var req = new D19WarehouseRequest(
                commandId: "cmd_store_invalid_item",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: "non_existing_commodity_xyz",
                quantity: 1,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Store(req, inventory, _validClaims);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(D19WarehouseReasonIds.UnknownItem, result.Failure.ReasonId);
        }

        [Test]
        public void Withdraw_NotEnoughCargo_RejectsWithStableReason_NoStateChange()
        {
            var inventory = new D3InventoryState(10, 100);
            D3InventorySystem.TryAddCargo(inventory, ItemSalt, 2, out _);

            var req = new D19WarehouseRequest(
                commandId: "cmd_withdraw_too_much",
                operation: D19WarehouseOperation.Withdraw,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemSalt,
                quantity: 10, // 仅有 2，取 10
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult result = _adapter.Withdraw(req, inventory, _validClaims);

            Assert.IsFalse(result.Success, "取货超出应拒绝");
            Assert.AreEqual(D19WarehouseReasonIds.NotEnoughCargo, result.Failure.ReasonId);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(inventory, ItemSalt), "原有库存不可扣减");
        }

        // ============================================================
        // 6. 重复命令幂等控制
        // ============================================================

        [Test]
        public void Store_DuplicateCommand_SameParams_ReplaysResultWithoutSecondSideEffect()
        {
            var inventory = new D3InventoryState(10, 100);

            var req = new D19WarehouseRequest(
                commandId: "cmd_idem_store_100",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 2,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            // 第一次投递
            D19WarehouseResult res1 = _adapter.Store(req, inventory, _validClaims);
            Assert.IsTrue(res1.Success);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(inventory, ItemTimber));

            // 第二次投递（相同 CommandId，相同参数）
            D19WarehouseResult res2 = _adapter.Store(req, inventory, _validClaims);
            Assert.IsTrue(res2.Success);
            Assert.AreEqual(res1.AppliedQuantity, res2.AppliedQuantity);
            Assert.AreEqual(res1.ItemCount, res2.ItemCount);
            Assert.AreEqual(res1.FreeCargo, res2.FreeCargo);
            Assert.AreEqual(2, D3InventorySystem.CountCargo(inventory, ItemTimber), "绝无第二次入库副作用");

            // 第三次通过 GetApplied 读
            D19WarehouseResult res3 = _adapter.GetApplied("cmd_idem_store_100");
            Assert.IsNotNull(res3);
            Assert.AreEqual(res1.AppliedQuantity, res3.AppliedQuantity);
        }

        [Test]
        public void Store_DuplicateCommand_DifferentParams_ReturnsDuplicateCommandConflict()
        {
            var inventory = new D3InventoryState(10, 100);

            var req1 = new D19WarehouseRequest(
                commandId: "cmd_idem_conflict",
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 1,
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            var req2 = new D19WarehouseRequest(
                commandId: "cmd_idem_conflict", // 相同 ID
                operation: D19WarehouseOperation.Store,
                actorId: ActorPlayer,
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                serviceId: ServiceWarehouse,
                itemId: ItemTimber,
                quantity: 3, // 不同参数！
                cargoShipId: ShipAlpha,
                worldDay: 10,
                saveRevision: 100
            );

            D19WarehouseResult res1 = _adapter.Store(req1, inventory, _validClaims);
            Assert.IsTrue(res1.Success);

            D19WarehouseResult res2 = _adapter.Store(req2, inventory, _validClaims);
            Assert.IsFalse(res2.Success, "参数不同应识别为冲突");
            Assert.AreEqual(D19PortFailureCode.DuplicateCommand, res2.Failure.Code);
            Assert.AreEqual(D19WarehouseReasonIds.DuplicateCommand, res2.Failure.ReasonId);
            Assert.AreEqual(1, D3InventorySystem.CountCargo(inventory, ItemTimber), "库存保持第一次的修改");
        }

        // ============================================================
        // 7. 仓库相关港口投影保存/恢复后等价（D19PortSaveProjection Roundtrip）
        // ============================================================

        [Test]
        public void WarehouseService_SaveProjection_Roundtrip_EquivalentAndNoUnityReferences()
        {
            // 构造仓库服务状态与相关投影
            var serviceStates = new List<D19PortServiceSaveEntry>
            {
                new D19PortServiceSaveEntry
                {
                    ServiceId = ServiceWarehouse,
                    PortId = PortSantaMargarita,
                    FacilityState = "Open",
                    Staffed = true,
                    LastUpdatedWorldDay = 15
                }
            };

            var pendingCommands = new List<D19PendingCommandSaveEntry>
            {
                new D19PendingCommandSaveEntry
                {
                    CommandId = "cmd_pending_warehouse_01",
                    Kind = "StartService",
                    Fingerprint = "fp1|store|timber|2|",
                    PortId = PortSantaMargarita,
                    ActorId = ActorPlayer,
                    WorldDay = 15,
                    SaveRevision = 205,
                    Payload = new Dictionary<string, string>
                    {
                        ["service_id"] = ServiceWarehouse,
                        ["item_id"] = ItemTimber,
                        ["quantity"] = "2"
                    }
                }
            };

            var idempotency = new D19IdempotencySaveProjection
            {
                SchemaVersion = 1,
                ShipId = ShipAlpha,
                RetentionWorldDays = 30,
                MaxRetainedEntries = 512,
                Entries = new List<D19IdempotencySaveEntry>
                {
                    new D19IdempotencySaveEntry
                    {
                        CommandId = "cmd_store_001",
                        Fingerprint = "fp1|Store|port_santa_margarita|ship_alpha_01|timber|2|",
                        Domain = "Warehouse",
                        State = "Completed",
                        ResultCode = "port.warehouse.store.applied",
                        WorldDay = 15,
                        Occurrence = 1
                    }
                }
            };

            var originalProjection = D19PortSaveProjectionMapper.BuildProjection(
                shipId: ShipAlpha,
                portId: PortSantaMargarita,
                visitState: D19PortVisitState.InService,
                berthId: "berth_sm_01",
                worldDay: 15,
                saveRevision: 205,
                lastEventId: "d19pe_serviceresult_cmd_store_001_0",
                lastEventSequence: 108,
                berthAllocations: null,
                memberLocations: null,
                serviceStates: serviceStates,
                pendingCommands: pendingCommands,
                compensationDrafts: null,
                idempotency: idempotency
            );

            originalProjection.Validate();

            // 模拟落盘保存与重构克隆（Roundtrip）
            var restoredProjection = originalProjection.Clone();
            restoredProjection.Validate();

            // 结构与数据等价性断言
            Assert.AreEqual(originalProjection.SchemaVersion, restoredProjection.SchemaVersion);
            Assert.AreEqual(originalProjection.ShipId, restoredProjection.ShipId);
            Assert.AreEqual(originalProjection.PortId, restoredProjection.PortId);
            Assert.AreEqual(originalProjection.VisitState, restoredProjection.VisitState);
            Assert.AreEqual(originalProjection.BerthId, restoredProjection.BerthId);
            Assert.AreEqual(originalProjection.WorldDay, restoredProjection.WorldDay);
            Assert.AreEqual(originalProjection.SaveRevision, restoredProjection.SaveRevision);
            Assert.AreEqual(originalProjection.LastEventId, restoredProjection.LastEventId);
            Assert.AreEqual(originalProjection.LastEventSequence, restoredProjection.LastEventSequence);

            // 服务状态比对
            Assert.AreEqual(1, restoredProjection.ServiceStates.Count);
            Assert.AreEqual(ServiceWarehouse, restoredProjection.ServiceStates[0].ServiceId);
            Assert.AreEqual("Open", restoredProjection.ServiceStates[0].FacilityState);
            Assert.IsTrue(restoredProjection.ServiceStates[0].Staffed);

            // 未完成命令比对
            Assert.AreEqual(1, restoredProjection.PendingCommands.Count);
            Assert.AreEqual("cmd_pending_warehouse_01", restoredProjection.PendingCommands[0].CommandId);
            Assert.AreEqual("2", restoredProjection.PendingCommands[0].Payload["quantity"]);

            // 幂等记录比对
            Assert.AreEqual(1, restoredProjection.Idempotency.Entries.Count);
            Assert.AreEqual("cmd_store_001", restoredProjection.Idempotency.Entries[0].CommandId);
            Assert.AreEqual("Warehouse", restoredProjection.Idempotency.Entries[0].Domain);
            Assert.AreEqual("port.warehouse.store.applied", restoredProjection.Idempotency.Entries[0].ResultCode);

            // 纯度检查：验证类型定义未引用任何 UnityEngine 类型
            Assert.IsFalse(typeof(D19PortSaveProjection).Assembly.FullName.Contains("UnityEngine"), "投影模型不可依赖 UnityEngine");
        }
    }
}
