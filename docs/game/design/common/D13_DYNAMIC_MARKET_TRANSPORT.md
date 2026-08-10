# D13-A07 动态市场与运输

任务编号：`D13-A07`  
日期：2026-08-11  
状态：已实现并通过专项验收

## 1. 目标与范围

本模块把 D11 采集、D13 农业/牧场/建筑产出和 D9 贸易接成一条可保存的商品循环：

`生产来源 → Inventory/Cargo 入库 → EconomySystem 市场供给 → 运输装货 → 天气/船体/生态压力损耗 → 到达入库 → 消费或销售 → EconomySystem 更新库存/需求/价格来源`

商品目录覆盖鱼获、果实、蔬菜、加工品、船材、服装、宠物用品、工具、药材、特殊物品和毒物。具体代表定义包括：

| 类别 | 代表内容 | 规则标签 |
|---|---|---|
| 鱼获 | `item_fresh_mackerel` | `fish`、`fresh`、`perishable` |
| 农产品 | `item_fruit_guava`、`item_vegetable_cassava` | `farm`、`fruit` / `vegetable`、`food` |
| 加工品 | 果脯、木薯粉、靛蓝、咖啡、可可、烟叶、香料、药草 | `processed`、作物/用途标签 |
| 船材 | `timber`、`item_fiber_agave`、`item_resin_pine` | `ship_material`、`timber` / `fiber` / `resin` |
| 生活与工具 | `outfit_top_linen_deckhand`、`item_pet_feed`、`item_gear_handline_basic` | `clothing`、`pet`、`tool` |
| 特殊/风险 | `item_special_weapon_moonwake_cutlass`、`item_toxin_datura` | `special`、`authorized`、`toxin`、`dangerous` |

目录只保存不可变定义、单位重量/体积、基础价、需求目标、易损度、合法等级、许可和生产 owner；不保存动态库存、价格或货舱数量。

## 2. 唯一写者与集成边界

| 数据 | 唯一写者 | D13-A07 做什么 |
|---|---|---|
| 价格、市场库存、需求、交易量 | `TradeEconomySystem` / `EconomySystem` | 通过 `D13TradeEconomyAuthority` 调用公开报价和外部市场流入口；只保存价格、市场来源和账本引用 |
| 货舱/库存物品 | `D3InventorySystem` / `Inventory/Cargo` | 通过 `D13D3InventoryAuthority` 调用 `TryAddCargo` / `TryRemoveCargo`；D13 不维护第二份数量 |
| 运输状态、装货、到达、损耗 | `D13DynamicMarketTransportSystem` / 运输 owner | 保存路线、剩余量、损耗量、到达量和状态；不写市场数量或货舱数量 |
| 法律、许可、势力政策 | `FactionLegalSystem` / 法律 authority | 返回带 `SourceEventId` 的合法等级与许可裁决；D13 拒绝缺失来源或非法货物 |
| 生产、消费、资产/生态来源 | 所属生产/消费、船舶和生态系统 | authority 只返回决定与共享事实引用；D13 不生成收成、不裁决消费用途 |
| UI / Agent | UI / Agent | 只能提交 request 或读取 `D13DynamicMarketReadModel`、报价、事件和失败恢复路线 |

`D13DynamicMarketTransportSystem` 是编排器和运输状态 owner，不复制 D9 TradeEconomy、D3 Inventory、D4 Ecology 或 D13 资产/农业状态。没有 authority 时默认拒绝，不使用隐藏默认价格、库存、收成或法律结果。

## 3. 确定性规则

### 3.1 生产、销售与消费

每个请求必须有稳定 `OperationId`、`WorldDay`、`SaveRevision` 和可选因果事件。生产必须同时满足：商品存在、法律允许、生产 owner 在商品允许列表中、数量在请求范围内、生产来源存在；随后由 Inventory/Cargo 入库，再由 EconomySystem 增加市场供给并降低需求。

销售先由 Inventory/Cargo 出库，再取得 EconomySystem 的卖出报价、市场状态和账本引用。消费由消费 owner（岛屿治理、动物照护或船舶系统）裁决用途，Inventory/Cargo 扣除后由 EconomySystem 减少市场库存并增加需求。任何后续 authority 失败都会走明确恢复路线，并回滚已经完成的库存侧临时变化。

每条成功商品流记录：生产/消费/销售类型、商品、数量、地点、市场、单位价、总值、货币、账本、生产/消费 owner、Inventory、Economy、市场、法律、生态和资产来源事件。统一 `EventLog` 记录 `market_production_recorded`、`market_consumption_recorded` 和 `market_sale_recorded` 事实。

### 3.2 运输损耗

当前目录提供三条固定路线，容量为 20—30 单位，基础损耗为 2%—5%。推进运输时按以下确定性公式计算每次请求的损耗：

`损耗率 = min(90%, 路线基础损耗 + 天气风险 + floor((100 - 船体状态)/5) + floor(生态压力/10) + floor(易损度/10))`

天气风险固定为：Calm/TradeWind `0`、Rain `2`、Storm `10`、Hurricane `22`。损耗量为 `ceil(剩余量 × 损耗率 × 天数)`，非零风险至少损失 1 单位，且不会超过剩余量。天气、生态压力和船体状态都必须引用共享来源事实；运输系统写 `Loaded`、`InTransit`、`Arrived`、`Lost`、`Held` 状态及数量守恒记录。

到达时只把扣除损耗后的剩余量交给 Inventory/Cargo；货舱写入失败时运输进入 `Held`，不会凭空消失或重复入库。

### 3.3 法律与风险商品

合法等级分为 `Legal`、`Licensed`、`Restricted`、`Illegal`。烟草、药材、木材、特殊物品等需要各自许可；缺许可或许可不匹配会在扣库存前失败并携带法律复核路线。毒物 `item_toxin_datura` 标为 `Illegal`，即使请求带研究许可也不能通过 D13 市场流；失败事实保留请求、来源和恢复路线。

## 4. 存档与幂等

`D13DynamicMarketRuntimeSnapshot` 通过既有 SaveSystem 接入 `location_json.d13DynamicMarket`，保存：

- 商品流记录及各类来源事件；
- 运输路线、请求量、剩余量、损耗量、到达量、状态和风险分解；
- 运行时修订号和 `event_id + save_revision` 应用键。

存档系统只序列化/恢复快照，不修改价格、市场或 Inventory/Cargo。读档先校验商品/路线引用、数量守恒、来源和唯一 ID；重试相同操作会从 EventLog 或快照应用键返回 `Duplicate`，不重复扣库存、补库存、写市场供给或追加运输损耗。

## 5. 测试与验证

专项测试位于 `game/Assets/Tests/EditMode/D13DynamicMarketSystemTests.cs`，runner 位于 `tools/d13_a07_runner/`，覆盖：

1. 生产→装货→风暴运输→损耗→到达→销售→消费，以及保存/读档后的重复消费保护；
2. Calm 与 Hurricane、船体状态、生态压力对运输损耗的确定性影响；
3. 受限特殊物品的许可门槛、非法毒物拒绝、库存不被失败请求改动。

验证结果：

- `D13-A07 runner`：3/3 PASS；
- `dotnet build game/Whalebone.Systems.csproj --no-restore`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error；
- `D13-A03 runner`：3/3 PASS；`D13-A06 runner`：3/3 PASS；
- 内容注册表：109 entries / 13 prefixes PASS；
- D13 动态市场/运输代码无网络或 LLM 命中。

## 6. 接入注意与遗留

1. 真实 Farming、Fishing、Ranch、Building、AnimalCare、Ship 和 Property/Legal 系统接入时，只需实现对应 authority 并提供统一 EventLog 来源；不要在 D13 生成第二份产物、价格、库存或生态压力。
2. UI 只显示报价的 `Reasons`、运输风险分解、事件和 `RecoveryRouteId`；不得从快照反写任何领域状态。
3. `ApplyExternalMarketFlow` 是 EconomySystem 的公开市场流入口，用于 D13 生产/消费/销售更新市场库存与需求；它不处理玩家现金结算，现金/账本仍由 EconomySystem 所属交易入口裁决。
4. 当前无 A07 阻塞遗留；更多港口、路线、许可内容可作为后续内容配置扩展，不改变唯一写者边界。
