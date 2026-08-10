# 统一数据模型 v3.0

> 本文件是跨领域实体和存档形状的唯一设计事实源。领域文档可以补充语义，但不能私自新增同名字段、写者或状态。

## 1. 通用字段

所有可保存内容至少包含：

```text
id: string
schema_version: int
display_name_key: string
source_note: string
historical_tag: historical | inferred | fictional
phase: D7 | D8 | D9 | ... | D16
owner_system: string
test_id: string
```

所有世界实例另外包含 `created_day`、`updated_day`、`location_ref`、`state_flags` 和 `revision`。内容定义与运行时实例分离：Def 不因玩家行为改变，Instance 可以改变。

## 2. 核心实体

### 2.1 PlayerProfile

| 字段 | 说明 | 写者 |
|---|---|---|
| `id` | 玩家 ID | Core |
| `name` / `pronouns` | 姓名、称谓和表达 | Character |
| `identity_history` | 法律身份和变更记录 | Character/Legal |
| `appearance_loadout_id` | 当前外观 | Character |
| `skills` | 航海、贸易、战斗、医术、木工、厨艺、生态等 | Progression |
| `money` / `credit` | 现金和信用引用 | Economy/Finance |
| `reputation` | 各势力声望 | Faction |
| `ship_ids` | 拥有或管理的船 | Property |
| `estate_ids` | 岛屿/地块/建筑资产 | Property |
| `inventory_id` | 随身物品 | Inventory |
| `known_map` / `collection_book` | 地图和图鉴 | Discovery/Collection |

### 2.2 ShipInstance

核心旧字段必须兼容：`id`、`ship_type_id`、`name`、`hull`、`cargo`、`cannons`、`supplies`、`crew`、`upgrades`、`state`、`ship_log`。

新增字段：

- `hull_sections`：船首、船中、船尾、甲板、桅杆、舵、炮位和舱室损伤；
- `mass_state`：空船、货物、人员、补给、模块、吃水；
- `room_instances`：货舱、厨房、医务、住所、活鱼舱、炮药舱等；
- `function_points`：锚机、缆桩、泵、炮位、渔具点、维修点；
- `anchor_state` / `mooring_state`：锚、锚链、缆绳、泊位和走锚；
- `maintenance_orders`：材料、工期、承包商、费用和进度；
- `appearance`：船体涂装、帆、旗帜和装饰，不直接写强逻辑；
- `ownership`：所有、租赁、抵押、船队归属和法律限制。

写者：ShipSystem 写航行/船体/舱室；CargoSystem 写货物；CrewSystem 写岗位；Economy/Finance 写费用和抵押；AppearanceSystem 写外观。

### 2.3 CrewAgent

包含：

- `role`、`skills`、`traits`、`culture`、`religion`、`legal_status`、`occupation`；
- `health`、`fatigue`、`hunger`、`thirst`、`injury`、`disease`；
- `morale`、`loyalty`、`affinity`、`trust`、`fear`、`respect`、`attachment`、`jealousy`；
- `persona`、`memory`、`goals`、`promises`、`debts`、`current_task`、`work_shift`；
- `appearance_loadout_id`、`equipment_ids`、`pet_id`、`personal_inventory_id`。

LLM 只能读取经过筛选的角色快照和记忆摘要；CrewSystem 写关系和状态。

### 2.4 RelationEdge

```text
source_id, target_id, relation_type,
value, confidence, cause_event_ids[],
last_changed_day, locked_by_story, visibility
```

`relation_type` 包含 affinity、trust、loyalty、fear、respect、attachment、jealousy、debt、kinship、romance。关系变化必须绑定事实事件。

### 2.5 World and Economy

| 实体 | 关键字段 | 写者 |
|---|---|---|
| `IslandInstance` | 地貌、气候、水源、生境、控制者、人口、生态指标、资产 | Island/Ecology |
| `LandParcel` | 用途、肥力、水源、租约、所有者、风险 | Property/Island |
| `BuildingInstance` | 功能、等级、库存、工人、损伤、开放、产权 | Building |
| `MarketState` | 库存、需求、价格、关税、封锁、恢复 | Economy |
| `FinanceAccount` | 本金、利息、担保、抵押、到期、违约 | Finance |
| `OwnershipClaim` | 资产、权利类型、证明、期限、争议 | Property/Legal |
| `FarmCycle` | 地块、作物/动物、阶段、水、劳力、病虫害、产出 | Farming |

### 2.6 Ecology and Collection

| 实体 | 关键字段 |
|---|---|
| `VegetationInstance` | 物种、阶段、成熟度、采集、恢复、生境 |
| `AnimalRecord` | 物种、年龄/性别/变体、状态、生境、行为、法律 |
| `FishSchool` | 鱼种、数量、深度、迁徙、捕捞压力、恢复 |
| `PetInstance` | 品种、健康、饥饿、清洁、依恋、位置、照料 |
| `CatchRecord` | 工具、地点、时间、物种、品质、重量、处理 |
| `CollectionEntry` | 证据、首次发现、季节、地点、处理方式、奖励 |

### 2.7 内容、任务与奖励

- `QuestInstance`：状态、目标、前置、选择、失败、恢复、奖励请求和事实日志；
- `WorldEvent`：触发条件、概率、冷却、区域、天气、势力、输入事件、后果和种子；
- `RewardGrant`：奖励 ID、来源、数量、唯一性、领取记录、回收/恢复路径；
- `AppearanceLoadout`：身体、发型、服装、配饰、装备、耐久和来源；
- `MiniGameSession`：牌局规则、下注上限、玩家、种子、结算和债务。

QuestSystem 可以提出 `RewardRequest`，RewardSystem/EconomySystem 才能实际写入奖励。

## 3. 统一事件包

```json
{
  "event_id": "event_fishing_catch_001",
  "schema_version": 1,
  "world_day": 42,
  "location_ref": "reef_014",
  "actor_ids": ["player_001", "crew_003"],
  "input_facts": ["fish_school_014_active", "gear_net_01"],
  "owner_system": "EcologySystem",
  "proposal": null,
  "result": { "catch_id": "catch_042" },
  "cause_ids": [],
  "save_revision": 88
}
```

事件事实先落盘，Agent 摘要只能引用事实；重复事件必须通过 `event_id + save_revision` 防重。

## 4. 存档分层

| 层 | 内容 | 频率 |
|---|---|---|
| 会话 | 输入、临时 UI、当前交互 | 不保证持久 |
| 航程 | 航线、天气、船体、船员岗位、货物 | 每个关键节点 |
| 世界 | 资产、市场、生态、势力、任务、建筑 | 结算/离开区域 |
| 记忆 | 角色摘要、关系原因、日志引用 | 事件后 |

每次保存写 `save_version`、`world_revision`、`last_event_id` 和迁移标记。读取旧版本时先迁移再进入玩法，禁止让 UI 或 Agent 猜字段。

## 5. 版本迁移原则

- 新增字段必须有默认值和来源；
- 枚举改名必须保留旧值映射；
- 删除字段先停止写入，至少跨一个版本只读迁移；
- 不可迁移的资产必须转为“历史记录/补偿任务”，不能静默消失；
- 每个版本有迁移测试：空档、D7 档、半途事件档、资产负债档和损伤船档。
