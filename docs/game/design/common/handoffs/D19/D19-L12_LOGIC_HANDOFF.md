# D19-L12 交接报告

## 任务信息与结论

- 任务编号：`D19-L12`（波次 1，并行）
- 前置：D19-L17 / D19-L18 / D19-L23 均 LOGIC_PASS（已核实）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L12/D19_L12_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19WarehousePortAdapter.cs`（新建，仓库取放编排适配器）
- `game/tools/results/D19/D19-L12/D19_L12_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L12_LOGIC_HANDOFF.md`（本文件，新建）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未直接写任何 D3/D9 字段、未修改其它 D19 文件、contracts、Unity 或表现层；未启动 Player。`.meta` 由 Unity 下次导入时生成（非本任务范围）。

## 真实接口盘点（2026-08-17 源码确认）

- **D3**：`D3InventorySystem`（static）是货舱容量/数量/归属唯一写者：`TryAddCargo` / `TryRemoveCargo` / `CanAddCargo` / `CountCargo` / `FreeCargo` / `UsedVolume` 等，错误串稳定（`invalid_cargo` / `cargo_full` / `weight_full` / `cargo_overflow` / `not_enough_cargo` / `invalid_item`）；`D3InventoryState` 只有船货舱 `Cargo` 与个人物品 `PersonalItems`，**无港口侧仓库存储态**。
- **D9**：`D9PortShipPreparationSystem` 无任何仓库 API（真实盘点：服务仅有市场/补给/修理/招募/酒馆/工资/升级/离港）。
- **结论（如实记录）**：D3/D9 均无港口仓库存储模型 → 本适配器**不持有任何库存集合或数量事实**（不复制库存系统）；"仓库存取"定义为经 `D3InventorySystem`（唯一写者）对船货舱的存取编排：Store=`TryAddCargo`（入仓到货舱）、Withdraw=`TryRemoveCargo`（出仓）。D13 的 `StoreWarehouseItem` 属另一章节、不在 D19-L01 基线冻结依赖清单内，本适配器不引用 D13 类型。

## 编排流程（Store/Withdraw 共用 `Execute`，全部原子）

1. **输入校验**：命令 ID 非空（`d19.warehouse.missing_command_id`）、操作/物品/数量合法（`d19.warehouse.invalid_request`）；
2. **权限（D19-L23）**：`D19PortAuthorityValidator.Validate`（kind=StartService、service=warehouse、CargoShipId 归属声明）——跨船/跨港/伪造服务/未知港/未知服务一律拒绝（L17 `d19.port.permission_denied` / `d19.port.unknown_port`）；
3. **服务可用性（D19-L05）**：`D19PortAvailabilitySystem.Adjudicate`（warehouse 夜间关闭等）——不可用 → `d19.service.unavailable`；
4. **所有者调用（D3InventorySystem 唯一写者）**：适配器零直写字段；
5. **结果**：成功 → `port.warehouse.store.applied` / `port.warehouse.withdraw.applied` + L16 ServiceResult 事件草案（payload：item_id/quantity/operation）+ 所有者读数（ItemCount/FreeCargo）；失败 → L16 Rejection 事件草案（port.reject_reason 入 payload）。

## 确定性结果（全部有测试）

| 场景 | 结果 |
|---|---|
| 未知物品 | `d19.warehouse.unknown_item`（映射 owner invalid_cargo/invalid_item） |
| 容量满/重量满/溢出 | `d19.warehouse.capacity_full` / `weight_full` / `overflow`——**永不写成成功**，无部分写入 |
| 货舱不足 | `d19.warehouse.not_enough_cargo` |
| 跨实体货物（CargoShipId≠ShipId） | 权限拒绝 |
| 重复命令：同命令同参 | 返回存储结果，无副作用（幂等账本） |
| 重复命令：同命令异参 | `d19.warehouse.duplicate_command`，无变更 |
| 缺命令 ID / 非法请求 | `missing_command_id` / `invalid_request` |
| 服务不可用（夜间） | `d19.service.unavailable` |
| 未知港口 / 跨港声明 | `d19.port.unknown_port` / `permission_denied` |

失败载体 `D19PortFailure`：L17 已注册业务码优先（permission/unknown_port/service.unavailable 等），新领域原因 ID 使用同格式 `d19.warehouse.<name>`（**L17 目录待扩展，未修改 L17**）。

## 验证（全部真实执行）

| 检查 | 结果 |
|---|---|
| 临时 dotnet 工程（netstandard2.1，引用真实 Event + D3 + D9 + L02/L04/L05/L16/L17/L18/L23 源码 + 本文件） | 0 警告 / 0 错误 |
| 临时控制台 29 项确定性断言（覆盖上表全部场景 + 幂等无副作用 + 容量失败无部分写入 + 事件草案过 L16 Guard + 只经所有者 API） | ALL CHECKS PASS（29/29） |
| `git diff --check` | exit 0（仅既有 LF→CRLF 警告） |
| `python tools/sync_logic_task_status.py` | 总览重新生成，D19-L12=LOGIC_PASS |

## 明确未宣称

- 未宣称存在港口侧仓库存储模型（如实登记：D3/D9 无此状态，Store 目标 = 船货舱）；
- 未直接写 D3/D9 字段；未复制库存集合；未创建 UI；容量失败从未写成成功；
- 未修改 D19-L23/L17/L05 等前置文件。

## 待办 / 后续注意事项

- L17 后续扩展注册 `d19.warehouse.*` 原因 ID；若未来出现港口仓库容量模型（D3 或 Runtime 阶段），Store 目标语义需扩展评审；
- D13 warehouse 建筑服务的 D19 接入需先更新 D19 基线冻结依赖（D19-L01 范围外，另行任务）；
- D19-L33 命令路由接入时复用本适配器幂等账本与事件草案。
