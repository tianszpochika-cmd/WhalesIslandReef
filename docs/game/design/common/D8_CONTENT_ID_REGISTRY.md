# D8 内容 ID 注册表与目录规范

> 任务：`D8-A02`  
> 状态：代表样例已建立，目录检查器与游戏读取入口已建立  
> 日期：2026-08-10  
> 上游边界：`D8_DATA_BOUNDARY_FREEZE.md`（D8-S01）

本文件是 D8 内容目录的使用规范。它只登记稳定 ID、内容元数据、来源/授权和内容之间的引用关系，不替代任何领域系统的数值裁决，也不把 UI、Agent 或 SaveSystem 变成内容目录写者。

## 1. 与 D8-S01 的关系

- 保留 D8-S01 的实体名称、唯一写者和 game-only / contract projection 边界；本任务没有改变任何强逻辑公式、D7 字段或现有 Catalog。
- 内容定义（Def/Catalog）与运行时实例（Instance/State）分离。注册表中的条目是不可变定义引用，玩家行为产生的状态由冻结表中的 canonical owner 写入。
- `schema_version=1`、`phase=D8`、`owner_system` 和 `test_id` 是目录登记信息；它们不是跨端契约，也不是新的领域系统接口。
- 需求中的 50 座岛、300 个动物、1,000—2,000 条植被和 200 个道具仍是内容目标，不代表本目录已经完成批量生产。

## 2. ID 规则与前缀

ID 必须稳定、唯一、可读、可迁移，使用小写 ASCII `snake_case`，并且文件名必须与 ID 完全一致。展示名称、本地化文本和资源路径都不能作为主键。格式为：

```text
<固定前缀><小写对象名>[_<小写变体>]
```

例如：`ship_moonwake`、`plant_red_mangrove`、`quest_first_chart`。已注册前缀如下：

| 前缀 | 内容类型 | 目录 | canonical owner | 代表样例 |
|---|---|---|---|---|
| `ship_` | 船体/船舶内容定义 | `entries/ship/` | `ShipSystem` | `ship_moonwake` |
| `crew_` | 船员内容定义 | `entries/crew/` | `CrewSystem` | `crew_isaac_reed` |
| `island_` | 岛屿内容定义 | `entries/island/` | `IslandSystem` | `island_saint_marys` |
| `building_` | 建筑内容定义 | `entries/building/` | `BuildingSystem` | `building_saint_marys_dock` |
| `item_` | 道具/货物内容定义 | `entries/item/` | `ItemCatalog` | `item_salted_cod` |
| `animal_` | 动物/物种内容定义 | `entries/animal/` | `EcologySystem` | `animal_green_turtle` |
| `plant_` | 植物/植被内容定义 | `entries/plant/` | `EcologySystem` | `plant_red_mangrove` |
| `fish_` | FishSchool 内容定义 | `entries/fish/` | `EcologySystem` | `fish_mackerel_school` |
| `quest_` | 任务内容定义 | `entries/quest/` | `QuestSystem` | `quest_first_chart` |
| `reward_` | 奖励定义/奖励请求引用 | `entries/reward/` | `RewardSystem` | `reward_first_chart` |
| `outfit_` | 外观/装扮内容定义 | `entries/outfit/` | `AppearanceSystem` | `outfit_weathered_deckhand` |
| `pet_` | 宠物内容定义 | `entries/pet/` | `AnimalCareSystem` | `pet_cabin_cat` |
| `asset_` | 已授权美术/音频/字体资源引用 | `entries/asset/` | `AssetRegistry` | `asset_ph_ship_fishing` |

前缀清单只在 `game/Assets/Resources/ContentRegistry/registry.json` 中登记。新增前缀必须先更新设计规范、owner、目录和检查测试，不能由 UI 或单个领域脚本私自扩展。

## 3. 目录结构

```text
game/Assets/Resources/ContentRegistry/
├── registry.json                 # 前缀、类型、目录、owner、扫描根
└── entries/
    ├── ship/ship_moonwake.json
    ├── crew/crew_isaac_reed.json
    ├── island/island_saint_marys.json
    ├── building/building_saint_marys_dock.json
    ├── item/item_salted_cod.json
    ├── animal/animal_green_turtle.json
    ├── plant/plant_red_mangrove.json
    ├── fish/fish_mackerel_school.json
    ├── quest/quest_first_chart.json
    ├── reward/reward_first_chart.json
    ├── outfit/outfit_weathered_deckhand.json
    ├── pet/pet_cabin_cat.json
    └── asset/*.json
```

目录扫描器位于 `tools/content_registry_check.py`；游戏侧读取器位于 `game/Assets/Content/Registry/ContentRegistryReader.cs`，通过 Unity `Resources` 读取 `ContentRegistry`。读取器只加载目录定义，不写入任何领域状态。

## 4. 条目字段

每个 JSON 条目必须登记以下字段：

| 字段 | 规则 |
|---|---|
| `id` | 全局唯一；必须使用本表前缀；文件名为 `<id>.json` |
| `schema_version` | D8 内容定义固定为 `1` |
| `name` | 当前显示名称；改名不改变 `id` |
| `display_name_key` | 本地化 key，当前要求以 `content.` 开头 |
| `category` | 必须与前缀注册表中的 `entity_type` 一致 |
| `tags` / `description` / `gameplay_role` | 内容语义和玩家可见用途；不写裁决数值 |
| `valid_regions` / `valid_habitats` / `valid_weather` | 内容适用范围标签；不等于运行时状态 |
| `dependencies` / `references` | 其他内容 ID 引用，目标必须存在 |
| `asset_refs` | 资源 ID 引用，目标必须存在且使用 `asset_` |
| `source_refs` / `source_note` | 可追溯来源；至少一个来源引用和一段说明 |
| `source` | 至少含 `kind`、`author`、`reference` |
| `license` | 至少含 `status`、`holder`、`reference`、`commercial_use`、`attribution_required`、`modification_allowed`；正式目录必须允许商业发行 |
| `historical_tag` | 只能是 `historical`、`inferred`、`fictional` |
| `phase` | `D7`—`D16`；本任务代表样例为 `D8` |
| `status` | 使用 `03_CONTENT_ID_CATALOG.md` 的状态值；目录样例为 `prototyped` |
| `owner_system` | 与冻结表和前缀定义一致；是读取/裁决边界的登记，不授予目录写权 |
| `test_id` | 唯一测试入口，本任务格式为 `D8-A02-*` |
| `reviewer` | 登记审阅任务或责任入口 |

### 4.1 来源和授权门槛

项目自有内容使用 `source.kind=project-authored`，来源引用对应设计文档，授权使用 `internal-authored`。本批 `asset_` 样例只登记仓库已存在的 Quaternius CC0 占位船、人形和棕榈叶贴图，并指向：

- `game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md`；
- `game/Assets/Art/Placeholder/PH_QUATERNIUS_LICENSE.txt`。

没有来源、没有许可证、不能商业发行或无法说明修改权限的资源不得进入正式 `entries/asset/` 目录。外部资源的下载日期、原始链接、署名要求等信息应继续补入来源清单；本任务不引入新的外部资源。

## 5. 引用图和孤儿规则

- `references`、`dependencies`、`asset_refs` 中的每个目标都必须是已登记 ID；缺失目标报告为 `missing-reference`，并显示来源条目 ID。
- `asset_refs` 的目标必须是 `asset_`；其他前缀报告为 `invalid-asset-reference`。
- `registry.json.roots` 定义目录图的入口。本任务使用 `quest_first_chart` 作为代表根，扫描器沿三类引用遍历。
- 未从任何 root 到达的条目报告为 `orphan-reference`。这防止样例/批量内容被写入目录却没有任何可追踪入口；未来增加新的独立入口时，应同时更新 roots 或引用链。
- 运行时关系、奖励领取、数量、健康、价格、天气和存档状态不写进这张内容图；它们仍由 D8-S01 冻结的系统 owner 裁决。

## 6. 检查器

执行：

```powershell
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
```

成功输出条目数和前缀数；失败时每行包含：

```text
ERROR <code> entry=<具体内容ID> file=<具体文件> message=<原因>
```

当前检查器会拒绝：

- 重复 ID 或重复 `test_id`；
- 缺少/非法前缀、错误目录、文件名与 ID 不一致、类别与前缀不一致；
- 缺少名称、本地化 key、owner、阶段、历史标签或测试入口；
- 缺少来源、来源说明或授权字段；
- `commercial_use` 不为真、授权字段类型错误或资源许可证不完整；
- 缺失引用、非法引用前缀、错误 `asset_` 引用和孤儿条目；
- 非法 JSON、未知状态、错误 schema 版本和不完整 roots。

扫描器是独立 Python 标准库工具，不依赖 Unity、server、web、LLM 或网络；因此可以在内容提交前运行。

## 7. 代表样例与游戏读取

本任务建立 15 条代表条目：12 条内容样例（船、船员、岛屿、建筑、道具、动物、植物、鱼群、任务、奖励、装扮、宠物）和 3 条已有占位资产引用。它们通过 `quest_first_chart` 连接为一张可扫描的引用图，覆盖所有 13 个 ID 前缀。

游戏侧入口：

```csharp
var registry = Whalebone.Content.ContentRegistryReader.LoadFromResources();
var ship = registry.Find("ship_moonwake");
```

Unity EditMode 测试 `D8ContentRegistryTests` 验证 Resources 读取、代表 ID、来源、授权、owner 和 test_id。D9 可以直接用这些 ID 建立港口—船—近海—船员 Agent 垂直切片，但应继续让领域系统读取定义、让所属系统写实例。

## 8. 后续内容准入

D9 及后续新增条目按以下顺序进入：

1. 先确定需求/设计入口、canonical owner、历史标签和失败恢复；
2. 取得来源、授权、商业发行和修改权限证据；
3. 选择未使用的稳定 ID，并放入对应前缀目录；
4. 填完本表字段，增加至少一个 `test_id` 入口和可追踪引用；
5. 运行目录检查器与对应领域测试；
6. 通过后再接入运行时实例，不在 UI 中复制一份目录。

本任务不批量伪造需求规模，不把 D4/D5 旧 Catalog 自动改写成新目录，也不把 `fish_school` legacy marker 当成 FishSchool 实体数量。

## 9. D9-A02 扩展准入

D9-A02 沿用本文件的 13 个前缀，没有新增 `gear_` 前缀。基础手线的正式目录 ID 为 `item_gear_handline_basic`，条目额外登记运行时逻辑别名 `logic_ref=gear_handline_basic`，以兼容 D9 垂直切片冻结引用；逻辑别名不是新的内容主键。D9 的鲜鱼、盐渍鱼和熟鱼代表条目同样放在 `entries/item/`，并通过 `fish_mackerel_school` 的引用图保持可达。它们使用 `phase=D9`，但目录检查的 `test_id` 仍使用 D8 内容准入格式。
