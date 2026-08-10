# D8-A02 内容 ID 与目录注册表交接报告

- 任务编号：`D8-A02`
- 前置任务：`D8-S01` 已完成
- 完成日期：`2026-08-10`
- 主成果：[`docs/game/design/common/D8_CONTENT_ID_REGISTRY.md`](../D8_CONTENT_ID_REGISTRY.md)
- 内容根目录：`game/Assets/Resources/ContentRegistry/`
- 交接状态：完成代表样例、扫描器、游戏读取入口和对应测试；未批量生产内容。

## 1. 修改文件

本任务新增或修改：

- `docs/game/design/common/D8_CONTENT_ID_REGISTRY.md`：目录规范、前缀、字段、来源/授权门槛、引用图和 D9 入口；
- `docs/game/design/common/README.md`、`docs/game/design/README.md`：补充目录规范入口；
- `game/Assets/Resources/ContentRegistry/registry.json`：注册表 manifest；
- `game/Assets/Resources/ContentRegistry/entries/<prefix>/`：15 条代表 JSON 条目；
- `game/Assets/Content/Registry/Whalebone.Content.Runtime.asmdef`：内容读取程序集；
- `game/Assets/Content/Registry/ContentRegistryReader.cs`：Unity Resources/目录读取器和 ID 查找；
- `game/Assets/Tests/EditMode/Save.Tests.asmdef`：引用内容读取程序集；
- `game/Assets/Tests/EditMode/D8ContentRegistryTests.cs`：游戏读取、字段、来源、授权测试；
- `tools/content_registry_check.py`：独立目录检查器；
- `tools/tests/test_content_registry_check.py`：检查器故障注入测试；
- `.workbuddy/memory/2026-08-10.md`：本任务记忆日志。

未修改 `contracts/`、`server/`、`web/`、`game/Assets/Scripts/Systems/` 的强逻辑、D7 Catalog、AgentCore 公共类型和 UI 目录。

## 2. ID 前缀

| 前缀 | 目录 | canonical owner | 代表样例 |
|---|---|---|---|
| `ship_` | `entries/ship/` | `ShipSystem` | `ship_moonwake` |
| `crew_` | `entries/crew/` | `CrewSystem` | `crew_isaac_reed` |
| `island_` | `entries/island/` | `IslandSystem` | `island_saint_marys` |
| `building_` | `entries/building/` | `BuildingSystem` | `building_saint_marys_dock` |
| `item_` | `entries/item/` | `ItemCatalog` | `item_salted_cod` |
| `animal_` | `entries/animal/` | `EcologySystem` | `animal_green_turtle` |
| `plant_` | `entries/plant/` | `EcologySystem` | `plant_red_mangrove` |
| `fish_` | `entries/fish/` | `EcologySystem` | `fish_mackerel_school` |
| `quest_` | `entries/quest/` | `QuestSystem` | `quest_first_chart` |
| `reward_` | `entries/reward/` | `RewardSystem` | `reward_first_chart` |
| `outfit_` | `entries/outfit/` | `AppearanceSystem` | `outfit_weathered_deckhand` |
| `pet_` | `entries/pet/` | `AnimalCareSystem` | `pet_cabin_cat` |
| `asset_` | `entries/asset/` | `AssetRegistry` | `asset_ph_ship_fishing` |

ID 规则保持 D8-S01/既有 `03_CONTENT_ID_CATALOG.md` 的稳定、小写 `snake_case`、展示名不作主键原则；文件名必须等于 ID。没有新增或改写 D8-S01 的实体边界。

## 3. 目录字段

每条正式目录内容登记：

`id`、`schema_version`、`name`、`display_name_key`、`category`、`tags`、`description`、`gameplay_role`、`valid_regions`、`valid_habitats`、`valid_weather`、`dependencies`、`references`、`asset_refs`、`source_refs`、`source_note`、`source`、`license`、`historical_tag`、`phase`、`status`、`owner_system`、`test_id`、`reviewer`。

其中 `source` 至少登记 `kind`、`author`、`reference`；`license` 至少登记 `status`、`holder`、`reference`、`commercial_use`、`attribution_required`、`modification_allowed`。正式目录拒绝缺来源、缺授权、不可商业发行的资源。

## 4. 代表样例

15 条样例覆盖全部 13 个前缀：

- 内容定义：`ship_moonwake`、`crew_isaac_reed`、`island_saint_marys`、`building_saint_marys_dock`、`item_salted_cod`、`animal_green_turtle`、`plant_red_mangrove`、`fish_mackerel_school`、`quest_first_chart`、`reward_first_chart`、`outfit_weathered_deckhand`、`pet_cabin_cat`；
- 已有授权占位资源引用：`asset_ph_ship_fishing`、`asset_ph_crew_male`、`asset_ph_palm_leaves`。

`quest_first_chart` 是注册表 root；引用图串联船、岛、建筑、船员、宠物、装扮、生态、鱼群、奖励、道具和资产。没有伪造 50 个岛、300 个动物、1,000—2,000 条植被或 200 个道具。

## 5. 检查命令与规则

正式目录扫描：

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

检查器测试：

```powershell
python -m unittest discover -s tools/tests -p "test_content_registry_check.py" -v
```

Unity 内容读取测试入口：`D8.Tests.D8ContentRegistryTests`，在 Unity Test Runner 的 `EditMode` 中执行；读取 API 为：

```csharp
Whalebone.Content.ContentRegistryReader.LoadFromResources()
```

检查器失败会定位 `entry=<具体内容ID>` 和文件路径，覆盖：重复 ID/`test_id`、非法前缀、错误目录、缺少引用、缺来源、缺授权、非法资产引用、孤儿条目、错误 schema/phase/status 和无效 JSON。

## 6. 测试结果

- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：通过，`15 entries, 13 prefixes`；
- `python -m unittest discover -s tools/tests -p "test_content_registry_check.py" -v`：通过，7/7；覆盖正式目录、重复 ID、非法前缀、缺失引用、缺失来源/授权、商业授权关闭和孤儿条目；
- Unity 6 最小副本 `Content.Tests.dll`：通过，`D8.Tests.D8ContentRegistryTests` 2/2，XML 结果 `result="Passed"`；验证 Resources 读取 15 条样例以及来源/授权/owner/test_id 字段；
- `git diff --check`：通过（仅有工作区既存的换行风格提示，无 whitespace error）；
- 主工程 Unity 批处理未强行抢占：当前主工程已有 Unity 实例占用；同版本完整副本的既有 `Assets/Scripts/Agent/AgentCore/AgentReplyValidator.cs(25,122)` 编译错误阻断了整仓测试，但最小内容副本已独立通过读取测试。该错误不在本任务修改范围内。

## 7. D9 可使用入口

1. 从 `game/Assets/Resources/ContentRegistry/registry.json` 读取前缀和 root；
2. 用 `ContentRegistryReader.LoadFromResources()` 加载定义，用 `Find("<prefix>...")` 获取代表条目；
3. D9 港口—船—近海—船员 Agent 垂直切片可先使用 `ship_moonwake`、`island_saint_marys`、`building_saint_marys_dock`、`crew_isaac_reed` 和 `quest_first_chart`；
4. 运行时实例仍由 D8-S01 冻结的 canonical owner 写入；目录只读，不在 UI 复制；
5. 新条目必须先补来源/授权和 `test_id`，再运行扫描器与领域测试；
6. `fish_school` legacy marker 不得当成 FishSchool 数量，D11 仍需建立专用实体和行为/恢复测试。

## 8. 未完成与边界

- 本任务没有接入 server/web，也没有新增 contracts；
- 本任务没有把 D4/D5 旧 Catalog 自动迁移成新目录；
- 本任务没有写运行时经济、战斗、生态、关系、奖励或外观数值；
- 批量内容生产留给 D15，垂直切片扩展按 D9 任务拆分；
- 完整主工程 Unity 测试需在既有 `AgentReplyValidator.cs` 编译错误修复、且不被另一个 Unity 实例占用后复跑。
