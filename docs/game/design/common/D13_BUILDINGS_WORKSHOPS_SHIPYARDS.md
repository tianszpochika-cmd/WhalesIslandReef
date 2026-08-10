# D13-A06 建筑、工坊、仓库与船坞

任务：`D13-A06`  
日期：2026-08-11  
状态：已实现并通过自动化测试

## 1. 范围

本模块提供一组可保存的建筑纵切片：码头、仓库、工坊和船坞。它们共享建设订单、材料、工人、工期、开放时间、服务、产出、损伤、火灾、占领、维护和地图可见状态；定义与运行时状态分离。

代表内容 ID：

| 类型 | 内容 ID | 产权 ID | 岛屿 | 工期/工人 | 开放时间 | 服务/产出 |
|---|---|---|---|---:|---|---|
| 码头 | `building_saint_marys_dock` | `property_shipyard_santa_marys_dock` | `island_saint_marys` | 2 日 / 3 人 | 06:00—22:00 | 泊位；维护 |
| 仓库 | `building_santa_marys_north_warehouse` | `property_warehouse_santa_marys_north_store` | `island_saint_marys` | 3 日 / 2 人 | 全天 | 仓储；维护 |
| 工坊 | `building_driftwood_ropewalk_workshop` | `property_workshop_driftwood_ropewalk` | `island_driftwood_isle` | 4 日 / 3 人 | 08:00—20:00 | 纤维+树脂→靛蓝染料；维护 |
| 船坞 | `building_santa_marys_shipyard` | `property_shipyard_santa_marys_dock` | `island_saint_marys` | 5 日 / 4 人 | 06:00—22:00 | 树脂+船体维修；维护 |

建设材料由 `D13BuildingCatalog` 定义，运行时材料扣除和仓储物品写入仍属于 `Inventory/Cargo`。服务价格不在建筑定义内硬编码，由 `EconomySystem` authority 返回带货币、账本和来源事件的报价。

## 2. 唯一写者与 authority 边界

| 数据 | 唯一写者 | D13-A06 的行为 |
|---|---|---|
| 建筑生命周期、建设进度、建设订单、损伤、火灾、开放/地图投影、占领投影、建筑服务事实 | `BuildingSystem` | 接受请求并按确定性规则写入 `D13BuildingState`、订单、火灾和服务记录 |
| 产权、地块资格和法律登记 | `PropertySystem` / `FactionLegalSystem` | 通过 `ID13BuildingPropertyAuthority` 返回产权及法律来源；BuildingSystem 不改产权 |
| 材料、仓库物品、服务输入和输出 | `Inventory/Cargo` | 通过 `ID13BuildingInventoryAuthority` 消耗材料、存入物品、处理服务产出；BuildingSystem 只保存来源和摘要 |
| 工人、岗位和劳务可用性 | `IslandGovernanceSystem` | 通过 `ID13BuildingWorkforceAuthority` 返回实际人数和来源事实 |
| 服务价格、货币和账本 | `EconomySystem` | 通过 `ID13BuildingEconomyAuthority` 裁决报价；建筑系统只保存历史收据 |
| 船体维修结果 | `ShipSystem` | 船坞服务必须先获得 `ID13BuildingShipAuthority` 批准，BuildingSystem 不改船体数值 |
| 占领/争议 | `FactionLegalSystem` | 通过 `ID13BuildingOccupationAuthority` 返回 `Free`、`Occupied` 或 `Contested` 及势力来源 |
| UI、Agent | UI / Agent | 只能提交 request 或读取 `D13BuildingReadModel` 与事件；不能完成建设、改价、加产出或清除损伤 |

没有注入 authority 时，系统使用拒绝适配器并返回明确恢复路线，不使用隐藏默认材料、价格、法律或船体结果。

## 3. 确定性操作规则

### 3.1 建设订单

`PlaceConstructionOrder` 必须同时满足：请求中的建筑、岛屿和产权 ID 完全匹配；产权 authority 和法律来源存在；工人数在 `1..WorkerSlots`；Inventory/Cargo 返回已消费材料的来源事件。成功后建筑进入 `UnderConstruction`，地图显示 `Construction`，订单记录起始日、到期日、材料快照、工人来源和产权来源。

`AdvanceConstruction` 每次成功请求只推进一天，并再次验证工人来源。达到定义工期后订单变为 `Completed`，建筑进入 `Operational`，地图显示 `Operational`。不能通过 UI 或 Agent 直接跳过工期；同一个 `event_id + save_revision` 重复提交返回 `Duplicate`，不重复扣料或推进。

### 3.2 开放时间和服务

非维护服务要求建筑已运营、未占领、无重大/火灾/战争损伤，且 `WorldHour` 落在定义的半开区间 `[OpenFromHour, OpenToHour)`。服务请求按顺序取得工人、Economy 报价、船舶批准（如需要）和 Inventory/Cargo 结算。结果收据保存：服务 ID、价格、货币、账本引用、经济来源、库存来源、船舶来源和产出摘要。

仓库存入只允许 `Warehouse` 定义，仍受开放时间、容量和 Inventory/Cargo authority 约束；仓储价格同样来自 EconomySystem。工坊生产不在 BuildingSystem 内生成物品，只把输入/输出定义交给库存 authority；船坞维修不在 BuildingSystem 内修改船体。

### 3.3 火灾、损伤和维护

`ApplyFire` 必须携带共享 EventLog 中存在的原因事实，严重度固定为 1—3 级，每级增加 25 点损伤并限制在 0—100；建筑生命周期为 `Closed`，地图显示 `Fire`。火灾记录保留原因、严重度、损伤、日期和恢复路线。

`ExtinguishFire` 必须有治理/劳务来源。扑灭后仍保持关闭：累计损伤 `>=50` 为 `Major`，否则为 `Minor`，地图显示 `Damaged`，不能把灭火当成自动维修。`building_maintenance` 是独立服务，要求建筑未被占领且仍可维修；每次按固定 50 点恢复，归零后才恢复 `Operational`/`Operational` 地图状态。损伤、火灾和维护都由 BuildingSystem 唯一写入，库存只处理维护材料。

### 3.4 占领与地图可见变化

占领/争议必须由势力/法律 authority 返回，不在 BuildingSystem 内猜测归属。非 `Free` 状态会关闭建筑、记录势力和来源事件、地图显示 `Occupied`；释放占领后按当前生命周期和损伤恢复显示。地图层只读取 `D13BuildingReadModel.Buildings[].MapState`，不反写系统。

可见状态包括：`Hidden`、`Planned`、`Construction`、`Operational`、`Damaged`、`Fire`、`Occupied`、`Closed`。这些状态是建筑状态的只读投影，不是 UI 自己推导的另一套状态。

## 4. 存档、恢复和事实因果

`PlayerState.D13BuildingRuntime` 通过 `SaveSystem.SaveD13BuildingRuntime` 写入既有 `location_json.d13Buildings` 节点，保存：

- 建筑定义对应的生命周期、进度、工人、损伤、占领和地图状态；
- 建设订单、材料来源、服务收据、价格账本和库存/船舶来源；
- 火灾原因、扑灭状态、恢复路线；
- `Revision`、最后事实和应用事件键。

读档会校验定义与运行时 ID、枚举、唯一 ID、损伤范围和来源字段；旧存档无该节点时使用空快照。恢复后的应用键与共享 EventLog 同时参与幂等判断，避免重复建设、重复扣材料、重复服务、重复产出、重复维护和重复火灾结算。

所有成功和失败结果先追加统一 `EventLog` 事实，再更新 BuildingSystem 状态；失败结果携带产权复核、材料复核、劳务复核、价格复核、火灾处置、占领复核或船坞服务复核路线。不存在网络调用、LLM 调用或 Agent 裁决路径。

### 4.1 D13-S01 水井接线

同步集成补充 `building_green_cay_water_well`（`D13BuildingKind.Well`）。水井建设、工期、材料和运营状态仍由 `BuildingSystem` 唯一写入；`WellService` 只登记水源接入事实，不生成水量。农业地块的经营资格适配器要求该建筑已进入 `Operational` 后才允许播种，实际土壤、湿度、水源和生态压力仍由 `EcologySystem` 唯一写入。这样“建水井 → 种蔬菜 → Ecology 改变”的因果链可保存、可追踪，且没有新增第二个水资源写者。

## 5. 测试与内容目录

`D13BuildingSystemTests` 覆盖：

1. 材料/工人/工期/工坊产出来源，服务报价和保存读档幂等；
2. 仓库存入、火灾、灭火、重大损伤、维护、占领、关闭服务和地图状态恢复；
3. 船坞价格由 EconomySystem 提供、船体结果由 ShipSystem authority 提供，以及船舶拒绝边界。

D8 内容目录新增仓库、工坊、船坞和漂木岛条目，所有引用均可从 registry root 到达；没有新增外部素材或许可证负担。

## 6. 已知边界

本批是 D13 建筑纵切片，不声称批量完成全部岛屿、建筑或美术资源。D4 的旧船坞升级逻辑仍作为历史兼容投影存在，但 D13-A06 的建设、损伤、服务、地图和存档入口由 `D13BuildingSystem` 独立裁决；两者不共享写状态，也不复制经济、库存或船体逻辑。
