# D11-A03 活鱼舱与岛上水产

状态：完成代表性切片（2026-08-11）  
任务：D11-A03  
前置：D9-S02 已通过；D11-A01 的生态状态机成果已在当前工作区接入

## 1. 范围与唯一写者

本切片把船上短期暂养与岛上长期养殖拆成两个状态域：

| 状态/结果 | 唯一写者 | 允许读取者 | 备注 |
|---|---|---|---|
| 活鱼舱容量、鱼群、重量、体积、氧气、水质、拥挤、维护、死亡 | `D11LiveFishHoldSystem` | Ship、Inventory、Quest、UI、Agent 摘要 | 船上运输/短期供给，不繁殖 |
| 鱼塘/近岸网箱建筑实例、鱼群、饲料、劳力、维护和死亡 | `D11IslandAquacultureSystem`（建筑/养殖 owner） | Island、Building、Inventory、Quest、UI、Agent 摘要 | 养殖建筑是独立实例，不复用船坞升级 |
| 鱼货持有量 | `D3InventorySystem` | Economy、Ship、UI、Quest | 水产系统只提交/调用库存写入口 |
| 鱼群与生境生态压力 | `D4EcologySystem` / `EcologySystem` | Island、Fishing、Quest、UI、Agent 摘要 | A03 只提交输入，压力数值由 EcologySystem 计算 |
| 事实日志 | `EventLog` | Quest、UI、Agent 摘要、Save | 只追加 `WorldEvent`，读取返回副本 |
| 序列化 | `SaveSystem` | Save/Load | 不计算死亡、产量、压力或鱼货 |

UI 不持有任何上述写引用；AgentReply 不能携带最终鱼货数量、死亡数量、压力或水质结果。

## 2. 内容样例与 DTO 字段

配置入口为 `D11AquacultureCatalog`，代表样例为：

| 类型 | ID | 关键配置 |
|---|---|---|
| 活鱼舱 | `room_live_fish_hold` | 容积 12、单鱼体积 1、单鱼重量 1、每日换水能力 4、维护劳力 1 |
| 淡水鱼塘 | `building_fish_pond_basic` | 容积 24、每日换水能力 3、最大鱼数 24 |
| 近岸网箱 | `building_nearshore_net_cage_basic` | 容积 32、每日换水能力 5、最大鱼数 32 |
| 鱼种 | `mackerel` / `fish_mackerel_school` | 鱼苗 `item_fish_fry_mackerel`、饲料 `item_fish_feed_basic`、产出 `item_fresh_mackerel` |

`D11AquacultureFacilityState` 的保存字段包括：

```text
schema_version, facility_id, kind, location_ref, habitat_id
capacity_volume, unit_fish_volume, unit_fish_weight, max_fish
water_exchange_per_day, oxygen, water_quality, crowding
maintenance, maintenance_debt, feed_stock, labor_stock
last_world_day, last_event_id, applied_event_keys
fish[]: species_id, cargo_item_id, quantity, unit_weight, unit_volume
```

因此重量和体积是状态可读字段的确定性投影：`total_weight = Σ(quantity × unit_weight)`，`used_volume = Σ(quantity × unit_volume)`，不会藏在 UI 或 Agent 文本里。

## 3. 结算流程

### 3.1 活鱼舱

1. `StockFry` 检查 `Inventory/Cargo` 中鱼苗、剩余容积和最大鱼数；事实先写入 `EventLog`，之后由 InventorySystem 扣鱼苗、由活鱼舱 owner 增加鱼群。
2. `Maintain` 按世界日确定性推进：换水提高氧气/水质；拥挤、缺氧、污染水质和维护债务提高死亡率；劳力不足增加维护债务。
3. 维护结算一次性写入维护事实，其中包含预定死亡结果，再更新活鱼舱状态；若有死亡，追加带有维护事件 `cause_ids` 的死亡审计事实。
4. `Harvest` 先检查货舱重量/体积，再追加事实，随后移除活鱼并通过 InventorySystem 写入 `item_fresh_mackerel`。

船上活鱼舱没有繁殖/自动增殖入口；鱼货只能来自已存在的活鱼状态。

### 3.2 鱼塘/网箱

1. `Build` 建立一个独立建筑实例；未知建筑类型、重复实例和缺少实例 ID 会返回明确错误。
2. `SupplyInputs` 通过 InventorySystem 消耗鱼苗和饲料，并记录劳力库存；系统不直接修改 Crew 数值。
3. `Advance` 使用饲料和劳力进行确定性维护/生长，按氧气、水质、拥挤、维护和饲料不足计算死亡，消耗对应输入。
4. `Advance`/`Harvest` 在事实进入 D11 水产日志后调用 `EcologySystem.ApplyAquaculturePressure`。生态系统根据鱼存量、饲料消耗、鱼货产出和死亡重新计算压力，写入自己的污染、浑浊度、鱼群压力和生态修订号。
5. `Harvest` 先由 InventorySystem 接收鱼货，再从建筑养殖状态移除鱼；容量、重量和货物数量都可从快照读取。

生态压力的最终值不由水产系统传入；水产系统只传入 `fish_quantity`、`feed_consumed`、`output_quantity` 和 `mortality` 等事实输入。

## 4. 确定性水质与死亡规则

每日维护使用稳定哈希，不使用运行时随机数。以换水是否达到 `water_exchange_per_day`、劳力是否达到 `maintenance_labor_per_day` 为输入：

```text
crowding = used_volume / capacity_volume
oxygen += exchange ? 0.20 : -0.10 - crowding × 0.12
water_quality += exchange ? 0.14 : -0.10 - crowding × 0.16
maintenance += labor ? 0.08 : -0.12
```

每条鱼的死亡阈值由鱼种基础死亡率、拥挤、缺氧、水质、维护债务和鱼塘饲料不足组成，再用 `species + facility + day + seed + fish_index` 稳定哈希判定。相同快照、世界日和种子得到相同死亡结果。

鱼塘在饲料达到 80% 需求且有维护劳力时确定性增长；增长仍受建筑容积和最大鱼数限制。活鱼舱不使用鱼塘增长规则。

## 5. 事件与因果链

事件 owner 和结果示例：

| 事件 | `owner_system` | 结果码 | 因果 |
|---|---|---|---|
| `event_d11_aquaculture_build_*` | `BuildingAquacultureSystem` | `aquaculture_facility_built` | 建筑登记，无前置因果 |
| `event_d11_live_fish_stock_*` | `LiveFishHoldSystem` | `live_fish_stocked` | 鱼苗事实 |
| `event_d11_aquaculture_inputs_*` | `BuildingAquacultureSystem` | `aquaculture_inputs_stocked` | 建筑事件 |
| `event_d11_live_fish_maintenance_*` | `LiveFishHoldSystem` | `live_fish_hold_maintained` | 上次活鱼舱事件 |
| `event_d11_aquaculture_advance_*` | `BuildingAquacultureSystem` | `aquaculture_maintained` | 建筑输入/上次养殖事件 |
| `event_d11_live_fish_death_*` | `LiveFishHoldSystem` | `live_fish_died` | 对应维护事件 |
| `event_d11_aquaculture_death_*` | `BuildingAquacultureSystem` | `aquaculture_fish_died` | 对应养殖维护事件 |
| `event_d11_live_fish_harvest_*` | `LiveFishHoldSystem` | `live_fish_harvested` | 上次活鱼舱事件 |
| `event_d11_aquaculture_harvest_*` | `BuildingAquacultureSystem` | `aquaculture_fish_harvested` | 上次养殖事件 |

事件唯一键为 `event_id + save_revision`。系统同时保存 `applied_event_keys`，所以只有状态快照或事实日志单独恢复时也会拒绝重复结算。`EventLog.TraceCauses` 可从鱼货回溯到维护、输入和建筑事实。

死亡审计事实在维护状态完成后追加，因为维护事件已经携带由 owner 计算的死亡结果；它不再写第二份死亡数值，只提供可读、可追溯的审计记录。

## 6. 保存与读档

`SaveSystem.SaveD11AquacultureRuntime` 将 `D11AquacultureRuntimeSnapshot` 写入现有 `location_json.d11Aquaculture`，其中包含：

- `LiveFishHold` 快照；
- 岛上鱼塘/网箱实例列表；
- D11 水产 `EventLogSnapshot`（事实、日志序号和 settlement claims）。

生态压力由 EcologySystem 写入 `D4RuntimeSnapshot.Ecology`，所以保存完整状态时同时保存 D11 水产快照和 D4 快照；SaveSystem 只复制/还原两者。旧存档缺少 `d11Aquaculture` 时返回空/默认水产快照，不生成鱼苗、鱼货或生态压力。

## 7. 测试入口

Unity EditMode：`D11.Tests.D11AquacultureTests`，覆盖：

- 活鱼舱容积、重量、换水、氧气、水质、拥挤、维护、确定性死亡和死亡事件；
- 鱼塘建造、鱼苗/饲料/劳力输入、维护、鱼货输出、生态压力和因果链；
- 保存/读档、事件日志恢复和读档后重复保护；
- 未知建筑和缺少生态系统的明确错误。

不依赖 Unity 的 smoke：`tools/d11_a03_smoke/D11A03Smoke.csproj`，验证同一组核心接口的编译和运行时闭环。

## 8. 已知边界

- 本任务只提供三个代表性养殖定义，未承诺完整版本的鱼种、建筑和内容规模；新的正式内容 ID 需先经过 D8 内容目录与授权流程。
- 当前全量 Systems/Save.Tests 重跑受工作区并行 D11-A05 变更的 `D11PetSystem.cs(330)` 重复局部变量 `lostRevision` 阻断；这不是 D11-A03 水产逻辑，A03 不修改它。
- Unity UI、船舶总重量汇总、Economy 价格和建筑产权/税费仍由各自后续系统接入；本切片只暴露可读重量/体积和明确的输入输出边界。
