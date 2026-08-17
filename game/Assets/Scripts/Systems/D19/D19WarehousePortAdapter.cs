// ============================================================
// D19WarehousePortAdapter —— 港口仓库取放编排适配器（纯 C#，D19-L12）
//
// 归属：Whalebone.Systems.D19。本适配器编排"港口仓库服务"的存取流程：
//   校验港口服务可用性（D19-L05）与权限（D19-L23）→ 透传命令 ID →
//   调用原库存/容量所有者（D3InventorySystem）→ 返回规范 D19 结果与
//   L16 事件草案。
//
// 关键事实（2026-08-17 源码盘点，如实记录）：
//   * D3/D9 均无"港口侧仓库存储态"（D3InventoryState 只有船货舱 Cargo 与
//     个人物品 PersonalItems；D9 无仓库 API）。因此本适配器不持有任何
//     库存集合或数量事实（不复制库存系统），"仓库存取"定义为经
//     D3InventorySystem（容量/数量/归属唯一写者）对船货舱的存取编排：
//     Store = TryAddCargo（入库到货舱），Withdraw = TryRemoveCargo（出库）。
//   * D13 存在 warehouse 建筑服务（D13BuildingSystem.StoreWarehouseItem），
//     属 D13 章节、不在 D19-L01 基线冻结依赖清单内，本适配器不引用 D13 类型。
//
// 红线（与 D19-L01 基线 §3 一致）：
//   * 绝不直接写 D3TradeState/D3InventoryState 的任何字段（Cargo 字典、
//     PersonalItems、容量）；只经 D3InventorySystem 公开 API；
//   * 绝不复制库存集合、绝不创建 UI、绝不把容量失败写成成功；
//   * 无 Unity（Transform/Canvas/Input/网络/LLM），无 DateTime/真实时钟；
//   * 失败/成功都不产生半写；同一命令重复投递无副作用。
// ============================================================

using System;
using System.Collections.Generic;

using Whalebone.Systems.D3;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D19
{
    /// <summary>仓库取放操作。</summary>
    public enum D19WarehouseOperation
    {
        None = 0,
        Store,
        Withdraw
    }

    /// <summary>仓库系统稳定原因 ID（机器可读；L17 已注册码优先，其余为同格式新领域 ID，待 L17 扩展）。</summary>
    public static class D19WarehouseReasonIds
    {
        /// <summary>未知物品（L17 d19.inventory.operation_failed 承载）。</summary>
        public const string UnknownItem = "d19.warehouse.unknown_item";
        /// <summary>货舱容量满（L17 d19.inventory.capacity_exceeded 承载）。</summary>
        public const string CapacityFull = "d19.warehouse.capacity_full";
        /// <summary>货舱重量满（L17 d19.inventory.capacity_exceeded 承载）。</summary>
        public const string WeightFull = "d19.warehouse.weight_full";
        /// <summary>数量溢出（L17 d19.inventory.capacity_exceeded 承载）。</summary>
        public const string Overflow = "d19.warehouse.overflow";
        /// <summary>货舱不足（L17 d19.inventory.operation_failed 承载）。</summary>
        public const string NotEnoughCargo = "d19.warehouse.not_enough_cargo";
        /// <summary>请求非法（数量/物品/操作缺失）。</summary>
        public const string InvalidRequest = "d19.warehouse.invalid_request";
        /// <summary>同命令 ID 不同参数（重复命令冲突）。</summary>
        public const string DuplicateCommand = "d19.warehouse.duplicate_command";
        /// <summary>命令缺少稳定 command_id。</summary>
        public const string MissingCommandId = "d19.warehouse.missing_command_id";
        /// <summary>成功哨兵。</summary>
        public const string NoConflict = "none";

        public static readonly IReadOnlyList<string> AllReasonIds = new List<string>
        {
            UnknownItem, CapacityFull, WeightFull, Overflow, NotEnoughCargo,
            InvalidRequest, DuplicateCommand, MissingCommandId
        };
    }

    /// <summary>仓库存取请求（调用方构造；适配器不读取任何登录/场景状态）。</summary>
    public sealed class D19WarehouseRequest
    {
        /// <summary>稳定命令 ID（跨重试不变，幂等/事件关联键）。</summary>
        public string CommandId;

        /// <summary>请求者稳定 ID。</summary>
        public string ActorId;

        /// <summary>目标船稳定 ID（仓库服务操作的货舱归属船）。</summary>
        public string ShipId;

        /// <summary>货物归属声明（必须等于 ShipId，否则跨船拒绝）。</summary>
        public string CargoShipId;

        /// <summary>目标港口稳定键。</summary>
        public string PortId;

        /// <summary>仓库服务稳定键（service_&lt;port&gt;_warehouse，L04）。</summary>
        public string ServiceId;

        /// <summary>目标物品 ID（CommodityCatalog 或已注册货物定义）。</summary>
        public string ItemId;

        /// <summary>数量（&gt; 0）。</summary>
        public int Quantity;

        /// <summary>操作（Store/Withdraw）。</summary>
        public D19WarehouseOperation Operation = D19WarehouseOperation.None;

        /// <summary>逻辑世界日。</summary>
        public int WorldDay;

        /// <summary>存档修订号（事件关联）。</summary>
        public long SaveRevision;

        /// <summary>以下为服务可用性输入（调用方从领域状态提供；适配器只读透传）。</summary>
        public D19ServiceTimePeriod TimePeriod = D19ServiceTimePeriod.Afternoon;
        public D19FactionPermission FactionPermission = D19FactionPermission.Allowed;
        public D19WeatherLabel Weather = D19WeatherLabel.Clear;
        public D19FacilityState FacilityState = D19FacilityState.Open;
        public bool Staffed = true;

        public D19WarehouseRequest(string commandId, D19WarehouseOperation operation,
            string actorId, string shipId, string portId, string serviceId, string itemId, int quantity,
            string cargoShipId = null, int worldDay = 0, long saveRevision = 0)
        {
            CommandId = commandId ?? string.Empty;
            Operation = operation;
            ActorId = actorId ?? string.Empty;
            ShipId = shipId ?? string.Empty;
            CargoShipId = cargoShipId ?? shipId;
            PortId = portId ?? string.Empty;
            ServiceId = serviceId ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            Quantity = quantity;
            WorldDay = worldDay;
            SaveRevision = saveRevision;
        }
    }

    /// <summary>仓库操作结果（成功/失败 + 所有者读数 + L16 事件草案）。</summary>
    public sealed class D19WarehouseResult
    {
        public string CommandId = string.Empty;
        public D19WarehouseOperation Operation = D19WarehouseOperation.None;
        public bool Success;
        public D19PortFailure Failure;
        /// <summary>实际应用数量（成功或幂等命中时有效）。</summary>
        public int AppliedQuantity;
        /// <summary>操作后所有者读数：该物品在货舱的数量。</summary>
        public int ItemCount;
        /// <summary>操作后所有者读数：货舱剩余容量。</summary>
        public int FreeCargo;
        /// <summary>L16 事件草案（成功=服务结果；失败=拒绝；可能为 null）。</summary>
        public WorldEvent Event;
        /// <summary>幂等参数指纹（同命令比较用）。</summary>
        public string ParamsFingerprint = string.Empty;

        public D19WarehouseResult Clone()
        {
            return new D19WarehouseResult
            {
                CommandId = CommandId,
                Operation = Operation,
                Success = Success,
                Failure = Failure,
                AppliedQuantity = AppliedQuantity,
                ItemCount = ItemCount,
                FreeCargo = FreeCargo,
                Event = Event == null ? null : Event.Clone(),
                ParamsFingerprint = ParamsFingerprint
            };
        }
    }

    /// <summary>
    /// 港口仓库取放编排适配器（D19-L12）。纯编排：无库存状态、无 Unity、无 UI。
    /// 容量/数量/归属的唯一写者是 D3InventorySystem；本适配器只调用其公开 API。
    /// </summary>
    public sealed class D19WarehousePortAdapter
    {
        private readonly Dictionary<string, D19WarehouseResult> _applied =
            new Dictionary<string, D19WarehouseResult>(StringComparer.Ordinal);

        // ------------------------------------------------------------
        // 失败构造（L17 已注册码 + 同格式原因 ID；Recoverable 取 L17 重试策略）
        // ------------------------------------------------------------

        private static D19PortFailure BuildFailure(D19PortFailureCode code, string reasonId)
        {
            bool recoverable = true;
            D19FailureEntry entry = D19PortFailureCatalog.Find(reasonId);
            if (entry != null)
            {
                recoverable = entry.Retry != D19FailureRetryPolicy.NoRetry;
            }
            return new D19PortFailure(code, reasonId, recoverable);
        }

        private static D19WarehouseResult Fail(D19WarehouseRequest request,
            D19PortFailureCode code, string reasonId)
        {
            return new D19WarehouseResult
            {
                CommandId = request == null ? string.Empty : request.CommandId,
                Operation = request == null ? D19WarehouseOperation.None : request.Operation,
                Success = false,
                Failure = BuildFailure(code, reasonId)
            };
        }

        private static string Fingerprint(D19WarehouseRequest request)
        {
            return request.Operation.ToString() + "|" + request.PortId + "|" + request.ShipId +
                "|" + request.ItemId + "|" + request.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>幂等检查：同命令同指纹 → 存储结果；同命令异指纹 → 重复命令失败。null = 可执行。</summary>
        private D19WarehouseResult ReplayCheck(D19WarehouseRequest request)
        {
            D19WarehouseResult stored;
            if (_applied.TryGetValue(request.CommandId, out stored))
            {
                if (stored.ParamsFingerprint == Fingerprint(request))
                {
                    return stored.Clone();
                }
                D19WarehouseResult conflict = Fail(request, D19PortFailureCode.DuplicateCommand, D19WarehouseReasonIds.DuplicateCommand);
                conflict.Event = BuildRejectionDraft(request, D19WarehouseReasonIds.DuplicateCommand);
                return conflict;
            }
            return null;
        }

        // ------------------------------------------------------------
        // 事件草案（L16 工厂；不追加 EventLog）
        // ------------------------------------------------------------

        private WorldEvent BuildRejectionDraft(D19WarehouseRequest request, string reasonId)
        {
            if (string.IsNullOrEmpty(request.PortId) || string.IsNullOrEmpty(request.ShipId))
            {
                return null;
            }
            try
            {
                return D19PortEventFactory.CreateRejection(
                    request.CommandId, request.WorldDay, request.SaveRevision,
                    request.PortId, request.ShipId, reasonId, null, new[] { request.ActorId });
            }
            catch (EventValidationException)
            {
                return null;
            }
        }

        private WorldEvent BuildServiceDraft(D19WarehouseRequest request, string resultCode, string canonicalPortId, string canonicalServiceId, int applied)
        {
            var values = new Dictionary<string, string>
            {
                ["warehouse.item_id"] = request.ItemId,
                ["warehouse.quantity"] = applied.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["warehouse.operation"] = request.Operation == D19WarehouseOperation.Store ? "store" : "withdraw"
            };
            return D19PortEventFactory.CreateServiceResult(
                request.CommandId, request.WorldDay, request.SaveRevision,
                canonicalPortId, request.ShipId, canonicalServiceId, resultCode,
                new[] { request.ActorId }, null, "",
                new[] { D19PortEventSchema.ServiceFactPrefix + canonicalServiceId }, values);
        }

        // ------------------------------------------------------------
        // 主入口：Store / Withdraw 共用编排流程
        // ------------------------------------------------------------

        public D19WarehouseResult Execute(D19WarehouseRequest request, D3InventoryState inventory,
            D19AuthorityClaims claims = null, D19PortAnchorCatalog catalog = null)
        {
            if (request == null || inventory == null)
            {
                return Fail(request, D19PortFailureCode.InvalidTransition, D19WarehouseReasonIds.InvalidRequest);
            }
            D19WarehouseResult replay = ReplayCheck(request);
            if (replay != null)
            {
                return replay;
            }
            if (string.IsNullOrEmpty(request.CommandId))
            {
                return Fail(request, D19PortFailureCode.MissingCommandId, D19WarehouseReasonIds.MissingCommandId);
            }
            if (request.Operation != D19WarehouseOperation.Store && request.Operation != D19WarehouseOperation.Withdraw)
            {
                return Fail(request, D19PortFailureCode.InvalidTransition, D19WarehouseReasonIds.InvalidRequest);
            }
            if (request.Quantity <= 0 || string.IsNullOrEmpty(request.ItemId))
            {
                D19WarehouseResult invalid = Fail(request, D19PortFailureCode.InvalidTransition, D19WarehouseReasonIds.InvalidRequest);
                invalid.Event = BuildRejectionDraft(request, D19WarehouseReasonIds.InvalidRequest);
                return invalid;
            }

            // 1) 权限（D19-L23）：仓库服务命令 + 货物归属声明（跨船/跨港/伪造服务拒绝）
            D19AuthorityContext context = new D19AuthorityContext(
                D19PortCommandKind.StartService, request.ActorId, request.ShipId, request.PortId,
                null, request.ServiceId, request.CargoShipId);
            D19AuthorityResult authority = D19PortAuthorityValidator.Validate(context, claims, catalog ?? D19PortAnchorCatalog.Instance);
            if (!authority.Allowed)
            {
                string code = string.IsNullOrEmpty(authority.FailureCode) ? "d19.port.permission_denied" : authority.FailureCode;
                D19WarehouseResult denied = Fail(request, D19PortFailureCode.None, code);
                denied.Event = BuildRejectionDraft(request, code);
                return denied;
            }
            string canonicalPort = authority.CanonicalPortId;
            string canonicalService = authority.CanonicalServiceId;

            // 2) 服务可用性（D19-L05）：Warehouse 夜间关闭等规则
            D19ServiceAvailabilityResult availability = D19PortAvailabilitySystem.Adjudicate(
                canonicalPort, D19PortLocalizationKeys.ServiceIdWarehouse,
                request.TimePeriod, request.FactionPermission, request.Weather,
                request.FacilityState, request.Staffed);
            if (availability.Verdict != D19AvailabilityVerdict.Available)
            {
                D19WarehouseResult unavailable = Fail(request, D19PortFailureCode.ServiceUnavailable, "d19.service.unavailable");
                unavailable.Event = BuildRejectionDraft(request, "d19.service.unavailable");
                return unavailable;
            }

            // 3) 所有者调用（D3InventorySystem 唯一写者；适配器不触碰字段）
            string ownerError;
            bool ownerOk;
            if (request.Operation == D19WarehouseOperation.Store)
            {
                ownerOk = D3InventorySystem.TryAddCargo(inventory, request.ItemId, request.Quantity, out ownerError);
            }
            else
            {
                ownerOk = D3InventorySystem.TryRemoveCargo(inventory, request.ItemId, request.Quantity, out ownerError);
            }
            if (!ownerOk)
            {
                D19WarehouseResult failure = MapOwnerError(request, ownerError);
                failure.Event = BuildRejectionDraft(request, failure.Failure.ReasonId);
                return failure;
            }

            string resultCode = request.Operation == D19WarehouseOperation.Store
                ? "port.warehouse.store.applied" : "port.warehouse.withdraw.applied";
            D19WarehouseResult result = new D19WarehouseResult
            {
                CommandId = request.CommandId,
                Operation = request.Operation,
                Success = true,
                AppliedQuantity = request.Quantity,
                ItemCount = D3InventorySystem.CountCargo(inventory, request.ItemId),
                FreeCargo = D3InventorySystem.FreeCargo(inventory),
                Event = BuildServiceDraft(request, resultCode, canonicalPort, canonicalService, request.Quantity),
                ParamsFingerprint = Fingerprint(request)
            };
            _applied[request.CommandId] = result.Clone();
            return result;
        }

        /// <summary>便捷入口：Store（入仓）与 Withdraw（出仓）。</summary>
        public D19WarehouseResult Store(D19WarehouseRequest request, D3InventoryState inventory,
            D19AuthorityClaims claims = null, D19PortAnchorCatalog catalog = null)
        {
            request.Operation = D19WarehouseOperation.Store;
            return Execute(request, inventory, claims, catalog);
        }

        public D19WarehouseResult Withdraw(D19WarehouseRequest request, D3InventoryState inventory,
            D19AuthorityClaims claims = null, D19PortAnchorCatalog catalog = null)
        {
            request.Operation = D19WarehouseOperation.Withdraw;
            return Execute(request, inventory, claims, catalog);
        }

        public D19WarehouseResult GetApplied(string commandId)
        {
            D19WarehouseResult stored;
            return _applied.TryGetValue(commandId, out stored) ? stored.Clone() : null;
        }

        // ------------------------------------------------------------
        // 所有者错误映射（确定性；容量失败绝不写成成功）
        // ------------------------------------------------------------

        private static D19WarehouseResult MapOwnerError(D19WarehouseRequest request, string ownerError)
        {
            switch (ownerError)
            {
                case "cargo_full":
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.CapacityFull);
                case "weight_full":
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.WeightFull);
                case "cargo_overflow":
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.Overflow);
                case "not_enough_cargo":
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.NotEnoughCargo);
                case "invalid_cargo":
                case "invalid_item":
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.UnknownItem);
                default:
                    return Fail(request, D19PortFailureCode.None, D19WarehouseReasonIds.InvalidRequest);
            }
        }
    }
}
