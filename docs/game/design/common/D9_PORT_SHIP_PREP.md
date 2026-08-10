# D9-A01 港口与船舶整备

版本：`d9.port_ship_prep.v1`  
任务：`D9-A01`  
前置：`D9-S01` 已完成，垂直切片边界固定为 `InitialHarbor`、`santa_margarita`、`small_fishing`。

## 交付范围

本任务在 D7/D8 入口上补齐一条可执行的港口整备链：

`进入港口 → 分配泊位 → 市场 → 酒馆 → 补给 → 修船 → 招募 → 发工资 → 船坞改装 → 离港`

运行时代码分为两层：

- `game/Assets/Scripts/Systems/D9/D9PortShipPreparationSystem.cs`：无 Unity、网络或 LLM 的强逻辑入口。
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`：`InitialHarbor` 的 `D9PortServicePage`，只读快照和提交意图。

## 所有权与跨系统调用

| 状态 | 唯一写者 | D9-A01 调用方式 |
|---|---|---|
| 金币、商品价格、市场库存、货物数量 | `TradeEconomySystem` / `D3InventorySystem` | `Buy`、`TrySpend` |
| 船坞升级等级与升级费用 | `D4ShipyardSystem` | `Upgrade` 委托 `TryUpgrade` |
| 船体、吃水定义、锚泊/泊位、补给 | Ship/整备系统 | `Repair`、`BuySupply`、`EnterPort`、`PrepareDeparture` |
| 船员名册 | Crew/整备系统 | `Recruit` 统一追加 `CrewAgent` 和 `Ship.CrewIds` |
| 工资后的士气/忠诚 | 工资结算 owner | `SettleWages`，使用 `NumericConfiguration` clamp |
| 航行状态、目的地、航程消耗 | `D3RuntimeBootstrap` / Voyage owner | UI 先 `PrepareDeparture`，再调用 `RequestDeparture` |

D9 不复制 D8 实体。`D9PortShipPreparationSnapshot` 是 UI 读模型，不是新的 canonical 实体；`D9PreparationResult` 是一次意图的裁决结果。UI 不直接写 `Money`、`Cargo`、`Hull`、`Supplies`、`CrewStats` 或工资字段。

## 船舶状态快照

整备页必须显示并且只从 `BuildSnapshot` 读取：

- 重量：`D3InventorySystem.UsedCargo`；
- 吃水：`ShipTypeSeed` 的 `Draft`；
- 船员：`CrewAgents`/`Ship.CrewIds` 与 `CrewCapacity`；
- 货物：D3 inventory 的数量投影；
- 补给：`ShipState.Supplies[water|food]`；
- 损伤：`max(0, HullMax - Hull)`；
- 锚泊/泊位：`ShipState.State[moored|berth_*]`；
- 改装：D4 `ShipyardState.Upgrades` 的只读投影。

当前垂直切片泊位：

- `santa_margarita` → `building_saint_marys_dock`；
- `green_cay` → `berth_green_cay_01`。

未知港口、荒岛、未知泊位和未靠泊服务都会被拒绝。

## 结算规则

- 市场买货：继续使用 D3 价格公式与库存检查；买入货物后 `UsedCargo` 必须同步增加。
- 补给：只允许 `water`/`food`，数量为正且不超过整备边界；费用通过 D3 Economy 扣款。
- 修船：每修复 1 点船体消耗 8 金币；每 10 点修复消耗 1 件 `timber`；材料、金币、损伤和上限都先检查，失败不改变状态。
- 酒馆：按船员数支付服务费，效果为士气增加、疲劳降低；数值由整备系统 clamp。
- 招募：D9 只冻结 `crew_isaac_reed` 样例；编制满、重复对象或金币不足时拒绝。
- 工资：每名船员每天 25 金币；支付成功增加士气/忠诚，支付失败不扣钱但降低士气/忠诚。
- 船坞：改装费用和等级上限只由 `D4ShipyardSystem` 决定；D9 不复制升级公式。
- 离港：必须在注册港口、已靠泊、有至少一名船员且水/食物各至少一份。D9 系统测试入口 `Depart` 会把状态置为 `Sailing`；Unity 入口交给 D3 `RequestDeparture` 生成真实航程。

## 拒绝码

`invalid_state`、`invalid_port`、`still_sailing`、`voyage_failed`、`unknown_berth`、`not_moored`、`invalid_quantity`、`quantity_exceeds_damage`、`missing_material`、`not_enough_money`、`already_repaired`、`unknown_supply`、`crew_full`、`already_recruited`、`invalid_candidate`、`invalid_days`、`no_crew`、`insufficient_supplies`、`invalid_destination`、`not_in_port`、`shipyard_rejected`。

这些拒绝发生在强逻辑层，UI 只显示 `D9PreparationResult.Message`。Agent/LLM 不拥有港口整备写权，也不能绕过这些检查。

## 验收入口

EditMode：`game/Assets/Tests/EditMode/D9PortShipPreparationTests.cs`

- `BuyCargoChangesPreparationWeight`：买木材后重量从 0 变为 4；
- `RepairConsumesTimberAndMoney`：船体 40→50，同时扣木材和 80 金币；
- `WagesChangeCrewAndRecruitmentFillsShipRoster`：发工资提高船员士气，招募 `crew_isaac_reed` 入列；
- `DepartureLeavesPortAndEntersSailing`：离港后 `D3VoyageStatus.Sailing` 且解除锚泊。

Unity 页面入口名固定为 `D9PortServicePage`，与 D9-S01 冻结的 `D9_PortServicePage` 对齐；不存在第二套港口/船舶领域协议。
