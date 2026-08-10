# D9-A02 钓鱼垂直切片

> 任务：D9-A02　> 版本：`d9.fishing.v1`　> 日期：2026-08-10
>
> 上游冻结：`D9_VERTICAL_SLICE_FREEZE.md`、`D8_DATA_BOUNDARY_FREEZE.md`

本文件固定《鲸骨航线》D9 近海捕捞的最小可运行闭环。它只建立一个鱼群、一个捕捞点、一件渔具和三种鱼货状态，不把内容目标批量伪造成运行时数据。

## 1. 代表样例

| 类型 | 稳定引用 | 正式内容 ID | 唯一写者 |
|---|---|---|---|
| 鱼群 | `fish_mackerel_school` | `fish_mackerel_school` | `D4EcologySystem`（EcologySystem） |
| 捕捞点 | `nearshore_saint_marys` | 逻辑节点，不占用 D8 内容前缀 | D9 目录只读，位置由 EcologySystem 写入 |
| 渔具 | `gear_handline_basic` | `item_gear_handline_basic`，目录登记 `logic_ref` | ItemCatalog 登记；D9FishingCatalog 读取 |
| 鲜鱼 | `item_fresh_mackerel` | `item_fresh_mackerel` | D3InventorySystem 写入货舱数量 |
| 处理鱼 | `item_salted_mackerel` | `item_salted_mackerel` | D3InventorySystem 写入货舱数量 |
| 烹饪鱼 | `item_cooked_mackerel` | `item_cooked_mackerel` | D3InventorySystem 写入随身数量 |

`gear_handline_basic` 是 D9 冻结要求的逻辑引用。由于 D8-S01 已冻结正式前缀只有 `item_`，渔具作为物品条目登记为 `item_gear_handline_basic`，并用 `logic_ref` 保留冻结引用，不能新增 `gear_` 前缀。

## 2. 目录和字段

正式内容目录位于：

```text
game/Assets/Resources/ContentRegistry/
├── registry.json
└── entries/
    ├── fish/fish_mackerel_school.json
    └── item/
        ├── item_gear_handline_basic.json
        ├── item_fresh_mackerel.json
        ├── item_salted_mackerel.json
        └── item_cooked_mackerel.json
```

每条正式内容沿用 D8 字段：`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。新增渔具额外登记 `logic_ref`，它是运行时别名而不是第二个内容 ID。

运行时目录样例在 `D9FishingCatalog.cs` 中保持为一片近海鱼场和一件基础手线：

```text
nearshore_saint_marys
└── fish_mackerel_school
    └── gear_handline_basic
```

`D9FishingCatalog` 只提供不可变定义；鱼群的运行时位置、生物量和压力不写入目录。

## 3. 写者边界

### 3.1 EcologySystem

`D4EcologySystem` 是本切片的 EcologySystem 实例：

- `SeedFishSchool` 建立鱼群并写入初始位置；
- `MoveFishSchool` 写入鱼群位置；
- `PreviewFishCatch` 只计算确定性预览，不改变状态；
- `CommitFishCatch` 写入生物量和捕捞压力。

鱼获数量由鱼群状态、捕捞点、渔具承载上限、日期和种子经过系统内确定性计算得到。Agent、UI 和任务脚本不能传入或覆盖数量。

### 3.2 Inventory/Cargo

`D3InventorySystem` 是数量唯一写者。D9 通过 `TryAddCargo`、`TryRemoveCargo` 和 `TryAddPersonal` 完成：

1. 捕捞先用 `CanAddCargo` 检查空间；
2. 通过后写入 `item_fresh_mackerel`；
3. 处理以鲜鱼出货、盐渍鱼入货的同一库存操作完成；
4. 烹饪以鲜鱼出货、熟鱼进入随身物品。

非基础商品的单位重量/体积只由 `D3InventorySystem.RegisterCargoDefinition` 注册元数据，不能绕过该系统直接改货舱字典。

### 3.3 EconomySystem

`TradeEconomySystem` 是价格、市场库存和金币的唯一写者。D9 只向其注册鲜鲭鱼与盐渍鲭鱼的市场定义，并调用 `Sell`：

- 价格来自 `MarketQuote` 的供给、需求、天气、交易量、政策和港口税费因子；
- D9 不携带价格参数，也不根据文本或 Agent 输出修改价格；
- 出售前由库存系统移除鱼货，出售成功后由 EconomySystem 写入金币和市场库存，并追加 `event_d9_trade_settled`。

### 3.4 EventLog 和 UI

捕捞完成后，`D9FishingSystem` 仅把 EcologySystem 已结算的结果包装为 `event_d9_fishing_catch`，事件的 `owner_system` 固定为 `EcologySystem`。`event_id + save_revision` 是唯一去重键，重复调用不会重复增鱼。

UI 通过 `D9FishingRuntimeBootstrap` 和 `D9PresentationReadModel` 读取鱼群快照、货舱数量和 EventLog。UI 不追加事件、不领取结算、不写价格、不写库存。

## 4. 闭环流程

```text
进入 nearshore_saint_marys
        ↓
EcologySystem 建立/移动 fish_mackerel_school
        ↓
FishingSystem 读取 gear_handline_basic，提交捕捞意图
        ↓
EcologySystem 预览数量 ── 货舱不足 → 失败且鱼群不变化
        ↓
Inventory/Cargo 写入 item_fresh_mackerel
        ↓
EcologySystem 提交生物量/压力 + EventLog 记录 event_d9_fishing_catch
        ↓
处理为 item_salted_mackerel ── 或 ── 港口厨房烹饪为 item_cooked_mackerel
        ↓
港口出售鲜鱼/盐渍鱼 → EconomySystem 写价格、金币和市场库存
        ↓
D3/D4 快照 + EventLog 保存；读档后恢复鱼群、鱼获和去重门
```

## 5. 保存与读档

- D3 `D3RuntimeSnapshot.EventLog` 保存交易和捕捞事实；`SaveSystem` 已保存 `EventLogSnapshot` 的事实、序号和结算键。
- D4 `D4RuntimeSnapshot.Ecology.FishSchools` 保存鱼群位置、生物量、压力和最后移动日。
- 货舱/随身数量位于 D3 的 `Cargo`、`PersonalItems`，仍按 D3 既有保存入口落库。
- `D9FishingSystem.CaptureSnapshot/Restore` 提供无 Unity 的闭环快照测试入口；运行时接线由 D3/D4 Bootstrap 的 `SaveProgress` 依次落盘。
- 读档后 EventLog 先 Restore，再由 `D9EventLogBridge.Bind` 提供给 UI；旧捕捞事实不会再次写鱼获。

## 6. 检查与测试

内容目录：

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

D9 冻结和下游入口：

```powershell
python tools/d9_vertical_slice_check.py
python tools/d9_s02_acceptance_check.py
```

Unity EditMode 测试：

```text
D9FishingVerticalSliceTests
```

测试覆盖：鱼群位置/生物量由 EcologySystem 写入、鱼获进入货舱、重复捕捞去重、鲜鱼处理、港口出售、港口烹饪，以及快照恢复后重复事实不会再次增加库存。

## 7. 非目标

本切片不新增第二片鱼场，不把 D4 的 `fish_school` legacy marker 当成鱼群数量，不批量生成鱼种/渔具，也不修改天气、贸易或航行的强逻辑公式。
