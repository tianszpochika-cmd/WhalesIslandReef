// ============================================================
// D19PreparationPortAdapter —— 出航准备适配器（纯 C#，D19-L15）
//
// 归属：Whalebone.Systems.D19。本适配器只做"出航准备"的只读汇总与命令适配：
//   每条检查保留原所有者（D9/D3/D4/D2）、原始失败码（D9 真实 Code）与
//   D19 稳定失败码/本地化 key（L17 目录 + L18 key 注册表）。
//
// 红线（与任务卡和 D19-L01 基线一致）：
//   * D19 绝不自动补货、绝不扣钱、绝不修改船员、绝不替航海系统启动航程：
//     本文件不存在任何对 D9 PrepareDeparture / Depart / BuySupply / Recruit
//     等副作用入口的调用；唯一产出是只读汇总（D19DepartureReadinessSummary）
//     与纯数据离港意图（D19DepartureIntent，供 L09 离港工作流确定性读取）。
//   * 读数全部来自所有者公开读模型/状态快照，不重新推导数值：
//     - D9.D9PortShipPreparationSnapshot（D9PortShipPreparationSystem.
//       BuildSnapshot 的投影：Moored/InPort/CrewCount/Water/Food/Hull/
//       CargoCapacity/Weight/BerthId）——所有者 D9；
//     - D3.D3RuntimeSnapshot（VoyageStatus/CurrentLocationId/DestinationId）
//       ——所有者 D3 Voyage owner；
//     - D4/Save ShipState（Hull/CrewCapacity/Supplies/Cargo 事实来源）
//       ——所有者 D4 ShipSystem；
//     - D2 SailingSnapshot 属航行表现层，出航准备不引用（航线公式在 D3）。
//   * 未知/冲突/缺输入一律保守拒绝：Status=Unknown 计入 HasUnknown，
//     CanDepart=false，绝不静默通过。
//   * 无 Unity（Transform/Canvas/Input/网络/LLM）、无 DateTime/真实时钟、
//     无 UI 文案（Detail 只含稳定事实与数字，不含显示字符串）。
// ============================================================

using System;
using System.Collections.Generic;

using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Save;

namespace Whalebone.Systems.D19
{
    /// <summary>出航准备检查状态（三值，未知/冲突保守拒绝）。</summary>
    public enum D19DepartureCheckStatus
    {
        /// <summary>占位：未初始化。</summary>
        None = 0,

        /// <summary>通过（Ready）。</summary>
        Ready,

        /// <summary>明确不满足（携带原所有者原始失败码）。</summary>
        NotReady,

        /// <summary>输入缺失/冲突/无法判定（保守拒绝，不计为通过）。</summary>
        Unknown
    }

    /// <summary>
    /// 单条出航准备检查结果。
    /// 保留：原所有者（Owner）、原始失败码（OriginalCode，D9 真实 Code，
    /// 无所有者门槛时为空）、D19 稳定失败码（FailureCode）与其目录条目
    /// （FailureCodeId + LocKey，取自 D19PortFailureCatalog）。
    /// </summary>
    public sealed class D19DepartureCheckItem
    {
        /// <summary>稳定检查 ID（departure.&lt;domain&gt;，固定顺序）。</summary>
        public string CheckId;

        /// <summary>原所有者（读模型/状态事实来源）。</summary>
        public string Owner;

        /// <summary>检查状态。</summary>
        public D19DepartureCheckStatus Status = D19DepartureCheckStatus.None;

        /// <summary>原所有者原始失败码（如 D9 "not_moored"；通过或无数值门槛时为空）。</summary>
        public string OriginalCode;

        /// <summary>D19 稳定失败码（L02 枚举；通过时为 None）。</summary>
        public D19PortFailureCode FailureCode = D19PortFailureCode.None;

        /// <summary>L17 目录业务码（如 "d19.departure.failed"；通过时为空）。</summary>
        public string FailureCodeId;

        /// <summary>L17/L18 本地化 key（通过时为空）。</summary>
        public string LocKey;

        /// <summary>稳定观察事实（仅数字/键，无显示文案）。</summary>
        public string Detail;

        /// <summary>是否通过。</summary>
        public bool Passed
        {
            get { return Status == D19DepartureCheckStatus.Ready; }
        }
    }

    /// <summary>
    /// 出航准备只读汇总（确定性：固定检查顺序、排序明细；可供 L09 逐项读取）。
    /// 本汇总说明"可否离港"而**不拥有任何数值**：所有数字仍属原所有者。
    /// </summary>
    public sealed class D19DepartureReadinessSummary
    {
        /// <summary>目标港口稳定键。</summary>
        public string PortId;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId;

        /// <summary>目的地稳定键（可为空 = 未选择航线）。</summary>
        public string DestinationId;

        /// <summary>逻辑世界日（调用方传入，仅透传）。</summary>
        public int WorldDay;

        /// <summary>固定顺序的检查条目（确定性）。</summary>
        public readonly List<D19DepartureCheckItem> Items = new List<D19DepartureCheckItem>();

        /// <summary>是否可离港（全部条目 Ready 且无 Unknown；未知/冲突保守拒绝）。</summary>
        public bool CanDepart;

        /// <summary>是否存在未知/冲突条目（保守拒绝信号）。</summary>
        public bool HasUnknown;

        /// <summary>第一个阻断条目（固定顺序；全 Ready 时为 null）。</summary>
        public D19DepartureCheckItem FirstBlocker;

        /// <summary>阻断检查 ID（固定顺序）。</summary>
        public List<string> BlockingCheckIds = new List<string>();

        /// <summary>阻断原始失败码（与 BlockingCheckIds 一一对应；无码条目为空串）。</summary>
        public List<string> BlockingOriginalCodes = new List<string>();

        public D19DepartureReadinessSummary Clone()
        {
            D19DepartureReadinessSummary copy = new D19DepartureReadinessSummary
            {
                PortId = PortId,
                ShipId = ShipId,
                DestinationId = DestinationId,
                WorldDay = WorldDay,
                CanDepart = CanDepart,
                HasUnknown = HasUnknown,
            };
            foreach (D19DepartureCheckItem item in Items)
            {
                copy.Items.Add(new D19DepartureCheckItem
                {
                    CheckId = item.CheckId,
                    Owner = item.Owner,
                    Status = item.Status,
                    OriginalCode = item.OriginalCode,
                    FailureCode = item.FailureCode,
                    FailureCodeId = item.FailureCodeId,
                    LocKey = item.LocKey,
                    Detail = item.Detail,
                });
            }
            copy.BlockingCheckIds.AddRange(BlockingCheckIds);
            copy.BlockingOriginalCodes.AddRange(BlockingOriginalCodes);
            foreach (D19DepartureCheckItem item in copy.Items)
            {
                if (item.Status != D19DepartureCheckStatus.Ready)
                {
                    copy.FirstBlocker = item;
                    break;
                }
            }
            return copy;
        }
    }

    /// <summary>
    /// 纯数据离港意图（命令适配产出，供 L09 离港工作流确定性读取并决定是否
    /// 调用所有者命令链）。本适配器不执行它：启动航程的唯一入口是 D9
    /// Depart（经 L09 编排），D19 从不直接调用。
    /// </summary>
    public sealed class D19DepartureIntent
    {
        /// <summary>稳定命令 ID（调用方生成，跨重试不变）。</summary>
        public string CommandId;

        /// <summary>目标船稳定 ID。</summary>
        public string ShipId;

        /// <summary>出发港口稳定键。</summary>
        public string PortId;

        /// <summary>目的地稳定键。</summary>
        public string DestinationId;

        /// <summary>逻辑世界日。</summary>
        public int WorldDay;

        /// <summary>出航准备汇总（CanDepart=false 时 L09 必须拒绝执行）。</summary>
        public D19DepartureReadinessSummary Summary;

        /// <summary>是否可执行（= Summary.CanDepart；未知/冲突为 false）。</summary>
        public bool Executable;
    }

    /// <summary>
    /// 出航准备适配器（D19-L15）。纯只读汇总 + 命令适配：
    /// 不调用任何副作用入口、不写任何状态、不创建事件、无 Unity。
    /// </summary>
    public sealed class D19PreparationPortAdapter
    {
        // 稳定检查 ID（固定顺序 = 汇总顺序，L09 逐项消费）
        private const string CheckPort = "departure.port";
        private const string CheckMoored = "departure.moored";
        private const string CheckCrew = "departure.crew";
        private const string CheckSupplies = "departure.supplies";
        private const string CheckCargo = "departure.cargo";
        private const string CheckShip = "departure.ship";
        private const string CheckRoute = "departure.route";
        private const string CheckLifecycle = "departure.lifecycle";

        private const string DepartureBusinessCode = "d19.departure.failed";

        /// <summary>D19 稳定失败载体构造（L17 目录条目承载 code+key；重试策略取自目录）。</summary>
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

        private static D19DepartureCheckItem Item(
            string checkId, string owner, D19DepartureCheckStatus status,
            string originalCode, string detail)
        {
            D19DepartureCheckItem item = new D19DepartureCheckItem
            {
                CheckId = checkId,
                Owner = owner,
                Status = status,
                OriginalCode = originalCode ?? string.Empty,
                Detail = detail ?? string.Empty,
            };
            if (status != D19DepartureCheckStatus.Ready)
            {
                item.FailureCode = D19PortFailureCode.NotReadyToDepart;
                item.FailureCodeId = DepartureBusinessCode;
                D19FailureEntry entry = D19PortFailureCatalog.Find(DepartureBusinessCode);
                item.LocKey = entry == null ? string.Empty : entry.LocKey;
            }
            return item;
        }

        /// <summary>
        /// 构建出航准备只读汇总。全部读数取自所有者公开读模型；
        /// 任一输入缺失/冲突 → 对应条目 Unknown（保守拒绝）。
        /// </summary>
        public D19DepartureReadinessSummary BuildSummary(
            D3RuntimeSnapshot runtime,
            D3TradeState trade,
            ShipState ship,
            D9PortShipPreparationSnapshot d9,
            string portId,
            string destinationId,
            int worldDay = 0,
            D19PortVisitState? visitState = null)
        {
            D19DepartureReadinessSummary summary = new D19DepartureReadinessSummary
            {
                PortId = portId ?? string.Empty,
                ShipId = ship == null ? string.Empty : ship.Id,
                DestinationId = destinationId ?? string.Empty,
                WorldDay = worldDay,
            };

            // 1. 港口有效性（所有者 D9.PrepareDeparture，原始码 invalid_port）
            {
                string owner = "D9.D9PortShipPreparationSystem.PrepareDeparture";
                if (string.IsNullOrEmpty(portId))
                {
                    summary.Items.Add(Item(CheckPort, owner, D19DepartureCheckStatus.NotReady, "invalid_port", "port_id=empty"));
                }
                else if (d9 == null)
                {
                    summary.Items.Add(Item(CheckPort, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else if (!string.IsNullOrEmpty(d9.PortId) && d9.PortId != portId)
                {
                    summary.Items.Add(Item(CheckPort, owner, D19DepartureCheckStatus.Unknown, "invalid_port", "d9_port_id=" + d9.PortId + ",requested=" + portId));
                }
                else
                {
                    summary.Items.Add(Item(CheckPort, owner, D19DepartureCheckStatus.Ready, string.Empty, string.Empty));
                }
            }

            // 2. 靠泊/泊位（所有者 D9.PrepareDeparture，原始码 not_moored）
            {
                string owner = "D9.D9PortShipPreparationSystem.PrepareDeparture";
                if (d9 == null)
                {
                    summary.Items.Add(Item(CheckMoored, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else if (!d9.Moored)
                {
                    summary.Items.Add(Item(CheckMoored, owner, D19DepartureCheckStatus.NotReady, "not_moored", "moored=false"));
                }
                else
                {
                    summary.Items.Add(Item(CheckMoored, owner, D19DepartureCheckStatus.Ready, string.Empty, "moored=true"));
                }
            }

            // 3. 船员（所有者 D9.PrepareDeparture，原始码 no_crew；数量来自 D9 投影）
            {
                string owner = "D9.D9PortShipPreparationSystem.PrepareDeparture";
                if (d9 == null)
                {
                    summary.Items.Add(Item(CheckCrew, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else if (d9.Ship.CrewCount <= 0)
                {
                    summary.Items.Add(Item(CheckCrew, owner, D19DepartureCheckStatus.NotReady, "no_crew", "crew_count=" + d9.Ship.CrewCount));
                }
                else
                {
                    summary.Items.Add(Item(CheckCrew, owner, D19DepartureCheckStatus.Ready, string.Empty, "crew_count=" + d9.Ship.CrewCount));
                }
            }

            // 4. 补给（所有者 D9.PrepareDeparture，原始码 insufficient_supplies；水/食都须 ≥1）
            {
                string owner = "D9.D9PortShipPreparationSystem.PrepareDeparture";
                if (d9 == null)
                {
                    summary.Items.Add(Item(CheckSupplies, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else if (d9.Ship.Water <= 0 || d9.Ship.Food <= 0)
                {
                    summary.Items.Add(Item(
                        CheckSupplies, owner, D19DepartureCheckStatus.NotReady,
                        "insufficient_supplies", "water=" + d9.Ship.Water + ",food=" + d9.Ship.Food));
                }
                else
                {
                    summary.Items.Add(Item(CheckSupplies, owner, D19DepartureCheckStatus.Ready, string.Empty,
                        "water=" + d9.Ship.Water + ",food=" + d9.Ship.Food));
                }
            }

            // 5. 货舱一致性（所有者 D3.D3InventorySystem 读数；D9 无货舱离港门槛，本条目为 D19 一致性护栏。
            //    当 D9 投影与 D3 原始读数同时可用时做交叉核对，读数冲突 → Unknown 保守拒绝）
            {
                string owner = "D3.D3InventorySystem（读数）/D9.D9PortShipPreparationSnapshot";
                if (d9 == null)
                {
                    summary.Items.Add(Item(CheckCargo, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else
                {
                    bool negative = false;
                    foreach (KeyValuePair<string, int> pair in d9.Ship.Cargo)
                    {
                        if (pair.Value < 0)
                        {
                            negative = true;
                            break;
                        }
                    }
                    bool overflow = d9.Ship.Weight > d9.Ship.CargoCapacity;
                    if (negative || overflow)
                    {
                        summary.Items.Add(Item(CheckCargo, owner, D19DepartureCheckStatus.NotReady, string.Empty,
                            "weight=" + d9.Ship.Weight + ",capacity=" + d9.Ship.CargoCapacity + ",negative=" + negative));
                    }
                    else if (trade != null && trade.Inventory != null &&
                             D3InventorySystem.UsedCargo(trade.Inventory) != d9.Ship.Weight)
                    {
                        summary.Items.Add(Item(CheckCargo, owner, D19DepartureCheckStatus.Unknown, string.Empty,
                            "d9_weight=" + d9.Ship.Weight + ",d3_used_cargo=" +
                            D3InventorySystem.UsedCargo(trade.Inventory)));
                    }
                    else
                    {
                        summary.Items.Add(Item(CheckCargo, owner, D19DepartureCheckStatus.Ready, string.Empty,
                            "weight=" + d9.Ship.Weight + ",capacity=" + d9.Ship.CargoCapacity));
                    }
                }
            }

            // 6. 船只完整性（所有者 D4 ShipSystem / ShipState.Hull；D9 无耐久离港门槛，本条目为 D19 完整性护栏）
            {
                string owner = "D4 ShipSystem / Whalebone.Systems.Save.ShipState";
                if (d9 == null)
                {
                    summary.Items.Add(Item(CheckShip, owner, D19DepartureCheckStatus.Unknown, string.Empty, "d9_snapshot=missing"));
                }
                else if (d9.Ship.Hull <= 0)
                {
                    summary.Items.Add(Item(CheckShip, owner, D19DepartureCheckStatus.NotReady, string.Empty, "hull=" + d9.Ship.Hull));
                }
                else if (d9.Ship.Hull > d9.Ship.HullMax)
                {
                    summary.Items.Add(Item(CheckShip, owner, D19DepartureCheckStatus.Unknown, string.Empty,
                        "hull=" + d9.Ship.Hull + ",hull_max=" + d9.Ship.HullMax));
                }
                else
                {
                    summary.Items.Add(Item(CheckShip, owner, D19DepartureCheckStatus.Ready, string.Empty,
                        "hull=" + d9.Ship.Hull + ",hull_max=" + d9.Ship.HullMax));
                }
            }

            // 7. 航线/目的地（所有者 D9.Depart，原始码 not_in_port / invalid_destination）
            {
                string owner = "D9.D9PortShipPreparationSystem.Depart";
                if (runtime == null)
                {
                    summary.Items.Add(Item(CheckRoute, owner, D19DepartureCheckStatus.Unknown, string.Empty, "runtime=missing"));
                }
                else if (runtime.VoyageStatus != D3VoyageStatus.InPort)
                {
                    summary.Items.Add(Item(CheckRoute, owner, D19DepartureCheckStatus.NotReady, "not_in_port",
                        "voyage_status=" + runtime.VoyageStatus.ToString()));
                }
                else if (string.IsNullOrEmpty(destinationId))
                {
                    summary.Items.Add(Item(CheckRoute, owner, D19DepartureCheckStatus.NotReady, "invalid_destination", "destination_id=empty"));
                }
                else if (destinationId == runtime.CurrentLocationId)
                {
                    summary.Items.Add(Item(CheckRoute, owner, D19DepartureCheckStatus.NotReady, "invalid_destination",
                        "destination_id=current_location=" + destinationId));
                }
                else
                {
                    summary.Items.Add(Item(CheckRoute, owner, D19DepartureCheckStatus.Ready, string.Empty,
                        "destination=" + destinationId));
                }
            }

            // 8. 生命周期状态（可选输入；所有者 D19PortVisitSystem（L02 迁移表）：
            //    离港允许前态为 Boarded 或 PortVisit。未提供输入时不生成条目。）
            if (visitState.HasValue)
            {
                string owner = "D19.D19PortLifecycleContracts（D19PortVisitState）";
                if (visitState.Value == D19PortVisitState.Boarded || visitState.Value == D19PortVisitState.PortVisit)
                {
                    summary.Items.Add(Item(CheckLifecycle, owner, D19DepartureCheckStatus.Ready, string.Empty,
                        "visit_state=" + visitState.Value.ToString()));
                }
                else
                {
                    summary.Items.Add(Item(CheckLifecycle, owner, D19DepartureCheckStatus.NotReady, string.Empty,
                        "visit_state=" + visitState.Value.ToString() + ",allowed={Boarded,PortVisit}"));
                }
            }

            // 汇总（确定性：按固定检查顺序取首个阻断项）
            bool canDepart = true;
            bool hasUnknown = false;
            D19DepartureCheckItem firstBlocker = null;
            foreach (D19DepartureCheckItem item in summary.Items)
            {
                if (item.Status == D19DepartureCheckStatus.Unknown)
                {
                    hasUnknown = true;
                    canDepart = false;
                }
                else if (item.Status == D19DepartureCheckStatus.NotReady)
                {
                    canDepart = false;
                }
                if (item.Status != D19DepartureCheckStatus.Ready && firstBlocker == null)
                {
                    firstBlocker = item;
                }
                if (item.Status != D19DepartureCheckStatus.Ready)
                {
                    summary.BlockingCheckIds.Add(item.CheckId);
                    summary.BlockingOriginalCodes.Add(item.OriginalCode ?? string.Empty);
                }
            }
            summary.HasUnknown = hasUnknown;
            summary.CanDepart = canDepart;
            summary.FirstBlocker = firstBlocker;
            return summary;
        }

        /// <summary>
        /// 命令适配：把"离港请求"适配为纯数据意图（含只读汇总）。
        /// 本方法不执行任何命令、不产生任何状态变更；执行决定权在 L09 离港工作流。
        /// </summary>
        public D19DepartureIntent BuildDepartureIntent(
            string commandId,
            D3RuntimeSnapshot runtime,
            D3TradeState trade,
            ShipState ship,
            D9PortShipPreparationSnapshot d9,
            string portId,
            string destinationId,
            int worldDay = 0,
            D19PortVisitState? visitState = null)
        {
            D19DepartureReadinessSummary summary = BuildSummary(
                runtime, trade, ship, d9, portId, destinationId, worldDay, visitState);
            return new D19DepartureIntent
            {
                CommandId = commandId ?? string.Empty,
                ShipId = ship == null ? string.Empty : ship.Id,
                PortId = portId ?? string.Empty,
                DestinationId = destinationId ?? string.Empty,
                WorldDay = worldDay,
                Summary = summary,
                Executable = summary.CanDepart,
            };
        }

        /// <summary>只读自检：本适配器的失败码/业务码全部能在 L17 目录解析。</summary>
        public static List<string> Validate()
        {
            List<string> problems = new List<string>();
            D19FailureEntry departure = D19PortFailureCatalog.Find(DepartureBusinessCode);
            if (departure == null)
            {
                problems.Add("catalog entry missing: " + DepartureBusinessCode);
            }
            else if (string.IsNullOrEmpty(departure.LocKey))
            {
                problems.Add("catalog entry missing LocKey: " + DepartureBusinessCode);
            }
            return problems;
        }
    }
}
