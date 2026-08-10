# D9-A02 任务交接报告

> 项目：鲸骨航线　> 任务：D9-A02　> 日期：2026-08-10
>
> 前置：D9-S01 已完成并通过冻结检查

## 1. 交付摘要

已完成一个可保存/恢复的近海捕捞闭环：`nearshore_saint_marys` 捕捞点 → `fish_mackerel_school` 鱼群 → `gear_handline_basic` 基础手线 → 鲜鲭鱼入货舱 → 盐渍处理或港口烹饪 → 港口出售。

未批量生成鱼种、鱼场、渔具或道具；D8 前缀规则未改变。

## 2. ID 前缀与代表样例

D8 既有 13 个前缀全部保留：

`ship_`、`crew_`、`island_`、`building_`、`item_`、`animal_`、`plant_`、`fish_`、`quest_`、`reward_`、`outfit_`、`pet_`、`asset_`。

D9-A02 样例：

| 引用 | 正式内容 ID/位置 | 说明 |
|---|---|---|
| `fish_mackerel_school` | `fish_mackerel_school` | 唯一代表鱼群 |
| `nearshore_saint_marys` | D9 逻辑捕捞点 | 不新增前缀 |
| `gear_handline_basic` | `item_gear_handline_basic` | 条目 `logic_ref` 保留冻结逻辑别名 |
| `item_fresh_mackerel` | `entries/item/item_fresh_mackerel.json` | 捕捞产物 |
| `item_salted_mackerel` | `entries/item/item_salted_mackerel.json` | 处理产物 |
| `item_cooked_mackerel` | `entries/item/item_cooked_mackerel.json` | 港口厨房产物 |

正式条目字段统一包含：`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`；渔具额外包含 `logic_ref`。

## 3. 写者和入口

- 生态：`game/Assets/Scripts/Systems/D4/D4EcologySystem.cs` 的 `SeedFishSchool`、`MoveFishSchool`、`PreviewFishCatch`、`CommitFishCatch`；鱼群位置、生物量和压力唯一由 EcologySystem 写入。
- 捕捞编排：`game/Assets/Scripts/Systems/D9/D9FishingSystem.cs`；不自行裁决数量或价格。
- 库存：`game/Assets/Scripts/Systems/D3/D3InventorySystem.cs`；鱼获走货舱，熟鱼走随身物品。
- 经济：`game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs`；鱼货以扩展市场物品接入既有报价/供需/税费公式，价格、市场库存和金币仍由 EconomySystem 写入。
- 运行时只读展示/入口：`game/Assets/Scripts/Gameplay/D9/D9FishingRuntimeBootstrap.cs`；页面只显示鱼群状态、货舱和最新事实。
- 存档：D3 `EventLogSnapshot` 与货舱/交易状态、D4 `Ecology.FishSchools` 均进入既有 `SaveSystem` 快照路径。

## 4. 检查命令

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
python -m unittest tools.tests.test_content_registry_check -v
python tools/d9_vertical_slice_check.py
python tools/d9_s02_acceptance_check.py
```

Unity EditMode：

```text
D9FishingVerticalSliceTests
```

## 5. 测试结果

- 内容目录检查：通过，20 条条目、13 个前缀；错误格式包含具体 `entry=<内容 ID>`。
- 内容检查器单元测试：通过，7/7；覆盖重复 ID、非法前缀、缺失引用、缺少来源/授权和孤儿引用。
- D9-S01 冻结检查：通过，scenes=3、prefabs=5、commodities=10、events=14、ui_pages=6、agents=7。
- D9-S02 下游入口检查：`fishing_entry` 通过，已发现 `fish_mackerel_school`、`item_gear_handline_basic`、`logic_ref=gear_handline_basic` 和生产捕捞事实入口；其余 D9-A02 之外的 A03/A04/A05/A06 交接链由对应任务负责。
- 纯 C# D9-A02 smoke：通过，捕获 2 条、恢复后事实数 3、出售后金币 518；覆盖重复事实、处理、出售、烹饪和快照恢复。
- Unity `SaveSystemTests.D9FishingState_SaveLoad_RestoresFishCargoAndSchool` 已加入，覆盖 D3 鱼获货舱和 D4 鱼群状态的 SQLite 保存/读档路径。
- `git diff --check`：已执行，无空白错误。
- Unity EditMode 当前被现有 Unity 编辑器锁定，batchmode 未生成结果 XML；需在编辑器空闲后执行 `D9FishingVerticalSliceTests`。主工程生成的 `Whalebone.Systems.csproj` 还未刷新 D9 源文件，并同时保留工作区既有 `AgentModels.cs` 缺 `CrewDuty`，因此本轮以 D9 纯 C# 编译/smoke 和目录/冻结门禁作为可复现验证。

## 6. D9 后续入口

D9-A03 可直接从 `TradeEconomySystem.RegisterMarketItem/EnsureMarketItem` 接入其他特殊货物，但不要复制价格公式；D9-A05 可继续使用 D3 `EventLogSnapshot` 和 `D9EventLogBridge`；D9-A07 只读 HUD 可读取 `event_d9_fishing_catch` 的 `result.values`（`fish_school_id`、`fish_item_id`、`quantity`、`gear_id`）。

D9 后续若扩展鱼种，必须先增加正式 `fish_`/`item_` 条目、来源/授权、owner 和测试入口，再在 EcologySystem 与 EconomySystem 增加对应定义；不能由 UI 或任务脚本创建目录或鱼获结果。
