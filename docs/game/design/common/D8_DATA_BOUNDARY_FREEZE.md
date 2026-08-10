# D8 数据边界冻结表

> 任务：D8-S01（冻结 D8 数据边界）
> 状态：冻结基线，供 D8-A01—D8-A06 使用
> 冻结日期：2026-08-09
> 适用范围：D7 已有存档、D8 v3 统一实体、跨端 DTO 边界；不包含具体玩法实现和批量内容生产。

本表是 D8-A01—D8-A06 的共享入口。它只冻结“实体是什么、谁能写、谁能读、怎样存档、怎样版本迁移、是否跨端”，不授权任何 Agent 提前实现领域玩法。

## 1. 权威与冻结原则

字段语义以以下顺序仲裁：

1. `docs/game/design/DATA_MODELS.md`：实体、通用字段、事件包和存档分层的唯一设计事实源；
2. `docs/game/design/DESIGN_BOUNDARIES.md`：系统裁决权和 Agent/UI 禁写边界；
3. 当前 D7 代码：判断哪些字段已经存在、哪些字段必须兼容；
4. 本表：把上述事实映射成 D8 Agent 可直接执行的实体边界。

D8 仍然遵守以下规则：

- SaveSystem 是持久化写入/读取入口，不是经济、战斗、关系或奖励的数值裁决者；
- 每个实体有一个 canonical owner（唯一实体写者）。实体内部若有跨系统字段，采用“字段分区系统裁决 → canonical owner 汇总”的单向流程；不得由 UI、Agent 或 SaveSystem 直接修改事实；
- Agent 只能读取经过筛选的快照并提交提议。提议必须经过 `AgentReply` Schema、边界校验和所属系统裁决；
- D7 字段不删除、不改语义。D8 只增加规范化字段、默认值和迁移映射；旧字段在至少一个迁移版本内保留只读兼容；
- 内容定义（Def/Catalog）和运行时实例（Instance/State）分离。定义字段不能被玩家行为覆盖。

## 2. 版本冻结

| 层级 | 冻结值 | 责任 | 说明 |
|---|---:|---|---|
| D7 SQLite 存储版本 | `PRAGMA user_version=1` | `SaveSystem` | 当前代码事实；是 D7 来源版本，不等于 v3 实体 Schema 版本。 |
| D8 canonical 实体 Schema | `schema_version=1` | 各实体 canonical owner | D8 首次建立统一实体形状；`phase` 标识 `D7` 或 `D8` 字段来源。 |
| D8 v3 存档封套 | `save_version=3` | `SaveSystem` / Save-Migration | D8-A05 的目标存档版本；必须保留 D7 原字段并写入 `world_revision`、`last_event_id`、`migration_flags`。 |
| 世界实例修订号 | `revision`、全局 `world_revision` | 对应实体 owner / SaveSystem | 单调递增；用于读档继续和重复结算保护。 |
| 事实事件 Schema | `schema_version=1` | `EventSystem` | 事件唯一去重键为 `event_id + save_revision`；事件只追加，不反向拥有领域数值。 |
| 迁移来源标记 | `source_version=D7` 或实际来源版本 | `Save-Migration` | 不用删除字段解决兼容；未知字段按“保留并忽略”处理，损坏数据按明确恢复路径处理。 |

### 2.1 通用存档字段

所有 D8 canonical 可保存实体必须具备：

```text
id
schema_version
display_name_key
source_note
historical_tag
phase
owner_system
test_id
```

世界实例另外具备：

```text
created_day
updated_day
location_ref
state_flags
revision
```

`created_day/updated_day` 是游戏世界时间字段。`created_at/updated_at` 只能作为存储审计元数据，不能另造一套游戏实体时间语义。

## 3. D7 已有字段保留清单

下表列的是当前代码和 D7 存档实际存在的字段；“保留”表示 D8 迁移必须能读取并还原，不表示 D8 继续让旧字段成为新的唯一事实源。

| 实体 | D7 实际字段/镜像 | 当前存储位置 | D8 保留规则 |
|---|---|---|---|
| Player | `id`、`name`、`money`、`skills`、`traits`、`reputation`、`inventory`、`ship_id`、`location`、`play_seconds`；`LocationRef` 的 `type`、`portId`、`x`、`y`、`heading`、`speed`、`dayIndex`、`normalizedTime` | `players` 表；D3/D4/D5 快照在 `players.location_json` | 全部保留。`ship_id` 迁移为 `ship_ids` 的首项；`inventory` 迁移为库存引用/持有量；`money` 作为经济账户初始余额来源。 |
| Ship | `id`、`ship_type_id`、`name`、`hull`、`cannons`、`loaded_count`、`supplies`、`crew_capacity`、`crew_ids`、`upgrades`、`state`、`ship_log`；`HullMax` 由 `ship_type_defs.hull_max` 回查，不落库 | `ships` 表；D3/D4 船航行/船坞镜像在 `location_json.d3/d4` | 核心旧字段不得删。`hull_max` 继续以船型定义为事实源；D8 新损伤、舱室、锚泊等字段提供默认值。 |
| Crew | `id`、`name`、`role`、`culture`、`traits_json`；`stats_json` 的 `morale`、`loyalty`、`health`、`fatigue`、`affinity`、`skills`；`persona_json` 的 `name`、`role`、`traits`、`constraints`、`backstory`、`speech_style`；`memory_json`、`relations_json` 占位 | `crew_agents` 表；D4 `health.crew[]`；D5 `crewTraitIds` | 旧列和 JSON 键都保留。`memory_json` 的空骨架和 `relations_json` 的空对象必须可迁移；不得把空占位误判为完整 v3 关系/记忆模型。 |
| Relation | `CrewStats.Affinity`、`AgentReply.AffinityChange`；`relations_json={}` 占位 | AgentCore 内存、`crew_agents.relations_json` | 旧 affinity 映射为 `relation_type=affinity` 的 `value` 提议/结果；缺少 `source_id/target_id/cause_event_ids` 时由迁移器补默认并标记来源，不得伪造因果。 |
| Island | D3 `PortDefinition`：`id`、`displayName`、`type`、`worldX`、`worldZ`、`tariffRate`、`specialtyCoefficients`、`gatheringLimits`；D5 `D5IslandDefinition`：`id`、`nameZh`、`nameEn`、`type`、`biome`、`factionId`、`danger`、`tradeMultiplier`、`mapX`、`mapY`；D5 世界状态：`currentIslandId`、`discoveredIslands`、`visitedIslands`、`mapRevision` | D3/D5 目录和 `location_json.d3/d5.world` | 目录字段保持；D5 发现/访问状态迁移为 Island instance 的状态引用，不把“12 座岛”误标为 v3 目标的 50 座已完成内容。 |
| LandParcel | 无 D7 实体字段 | 无 | 不得从 `estate_ids` 或地图坐标猜造地块。D8-A01 只建立空值/版本策略；D13 再实现产权和经营字段。 |
| Building | 无 `BuildingInstance`；D4 只有 `shipyard.shipTypeId`、`unlockedShipTypes`、`upgrades` 这种船坞状态镜像 | `location_json.d4.shipyard` | 船坞镜像保留为 D7 兼容投影，不直接迁移成建筑实例；建筑 owner 和建筑存档由 D8-A01 预留。 |
| Item | D3 `CommodityDefinition`：`id`、`displayName`、`basePrice`、`cargoVolume`；`cargo`、`personalItems`、玩家 `inventory`、`marketStock` 的 `ItemId→count` | `players.inventory_json`、`ships.cargo_json`、`location_json.d3` | 商品 ID、基础价、货舱体积和数量全部保留；数量仍由 Inventory/Cargo/Economy 各自裁决，不能把 JSON 数量升级为 Item 定义字段。 |
| Animal | D4 `AnimalDefinition`：`id`、`name`、`habitat`、`danger`、`collectionValue`、`marine`、`rare`；`AnimalState`：`animalId`、`behavior`、`health`、`observed`、`collectionValue`；`EcologyState` 的 `habitatId`、`animals`、`collectionCount` | D4 `location_json.d4.ecology`；`d4-content.schema.json` | 旧 `animalId` 可作为 species/legacy instance 引用；行为、健康、观察和收藏值不得丢失。 |
| FishSchool | 只有 `D4AnimalId.FishSchool` 和 `AnimalState` 的 `animalId=fish_school` 占位，没有鱼群实体 | D4 生态快照可能出现 | 占位值保留为 legacy marker；不得把它当成真实鱼群数量。FishSchool 的 v3 字段全部走默认值并等待 D11 实体实现。 |
| Pet | 无 D7 实体字段 | 无 | 不得用 Crew 的 `pet_id` 反推宠物对象；先保留空引用策略。 |
| FinanceAccount | 只有玩家 `money`、D3 `D3TradeState.Money` 和交易结果的 `money_after`，没有账户/债务模型 | Player/D3 运行时快照 | 余额可作为迁移输入；不生成本金、利息、抵押或违约的假数据。 |
| Quest | D4 `MainlineMission`：`id`、`chapter`、`title`、`description`、`requiredFlag`、`grantsFlag`、`requiredFaction`、`requiredReputation`；`MainlineState`：`chapter`、`missionIndex`、`flags`、`completedMissions` | `location_json.d4.mainline`；`D4MainlineSystem` | 主线状态和完成列表保留；它们只是 QuestInstance 的 legacy projection，不得复制 money/inventory/relation。 |
| WorldEvent | D3 `VoyageEventDefinition` 的 `id`、`displayName`、`description`、`waterDelta`、`foodDelta`、`hullDelta`、`moneyDelta`；航程 `seed`/`eventIds`；D4 `RouteLogEntry` 的 `id`、`day`、`kind`、`originId`、`destinationId`、`text` | D3 `VoyageState`/D3 快照、D4 route log | 旧事件 ID、种子和日志文本保留为事实来源；旧 delta 不能成为跨系统直接写值的接口。 |
| Reward | D5 `D5AchievementDefinition` 与 `D5SteamState.UnlockedAchievements` 是成就/外部桥接，不是 `RewardGrant` | `location_json.d5.steam` | 已解锁成就保留；不得把成就 ID 伪装成可领取奖励或凭空补发物品/金钱。 |
| AppearanceLoadout | D5 `D5CharacterPresetDefinition`：`id`、`nameZh`、`nameEn`、`skinTone`、`hairStyle`、`hairColor`、`outfit`、`faceScale`、`bodyScale`、`accentColor`；运行时 `D5CharacterState`：`presetId`、`skinTone`、`hairStyle`、`hairColor`、`outfit`、`faceScale`、`bodyScale` | `location_json.d5.character`；`D5CharacterSystem` | 预设与当前运行时外观全部保留；D8 增加稳定 loadout ID 和扩展槽，不让 UI 直接写外观强逻辑。 |

### 3.1 D7 快照字段索引

为避免“D3/D4/D5 快照”被实现 Agent 当成未定义黑盒，以下列出当前 `D3RuntimeSnapshot`、`D4RuntimeSnapshot` 和 `D5RuntimeSnapshot` 的字段名。它们全部属于 D7 兼容面，D8-A05 迁移后再由 canonical 实体引用或拆分。

| 快照 | 当前字段 |
|---|---|
| `PlayerState.D3Runtime` | `currentLocationId`、`destinationId`、`voyageStatus`、`weather`、`currentDay`、`totalDays`、`voyageSeed`、`water`、`food`、`hull`、`hullMax`、`money`、`cargo`、`personalItems`、`marketStock`、`gatheredThisVisit`、`eventIds` |
| `PlayerState.D4Runtime.Factions` | `reputation`、`atWar`、`embargoedPorts`、`privateerPermits`、`wantedLevel` |
| `PlayerState.D4Runtime.Shipyard` | `shipTypeId`、`unlockedShipTypes`、`upgrades` |
| `PlayerState.D4Runtime.Combat` | `status`、`enemyId`、`playerHull`、`playerHullMax`、`enemyHull`、`enemyHullMax`、`distance`、`wind`、`playerCannons`、`loadedCount`、`weapon`、`enemyCannons`、`enemyLoadedCount`、`enemyCrew`、`playerCrew`、`turn`、`boardingScore` |
| `PlayerState.D4Runtime.Ecology` | `habitatId`、`animals[]`、`collectionCount`；每个 `animals[]` 元素有 `animalId`、`behavior`、`health`、`observed`、`collectionValue` |
| `PlayerState.D4Runtime.Health` | `crew[]`、`medicalSupplies`；每个 `crew[]` 元素有 `crewId`、`health`、`fatigue`、`diseases[]` |
| `PlayerState.D4Runtime.Mainline` | `chapter`、`missionIndex`、`flags`、`completedMissions` |
| `PlayerState.D4Runtime.RouteLog[]` | 每个条目有 `id`、`day`、`kind`、`originId`、`destinationId`、`text` |
| `PlayerState.D5Runtime.World` | `currentIslandId`、`discoveredIslands`、`visitedIslands`、`mapRevision` |
| `PlayerState.D5Runtime.Character` | `presetId`、`skinTone`、`hairStyle`、`hairColor`、`outfit`、`faceScale`、`bodyScale` |
| `PlayerState.D5Runtime.Performance` | `quality`、`targetWidth`、`targetHeight`、`targetFps`、`lastFps`、`minFps`、`averageFps`、`sampleCount`、`meetsTarget` |
| `PlayerState.D5Runtime.Steam` | `appId`、`offlineMode`、`cloudEnabled`、`pendingCloudRevision`、`lastSync`、`unlockedAchievements` |
| `PlayerState.D5Runtime` | `language`、`playerTraitIds`、`crewTraitIds`，以及上面的 `World`、`Character`、`Performance`、`Steam` |

## 4. v3 新字段清单

以下字段是 v3/D8 规范新增或规范化的字段。`通用`表示第 2.1 节字段；`实例`表示世界实例字段。若 D7 有同名但语义较窄的字段，D8-A05 必须走显式映射。

| 实体 | v3/D8 新字段或规范化字段 | 默认/迁移要求 |
|---|---|---|
| Player | 通用字段；`pronouns`、`identity_history`、`appearance_loadout_id`、`credit`、`ship_ids`、`estate_ids`、`inventory_id`、`known_map`、`collection_book`；实例 `created_day`、`updated_day`、`state_flags`、`revision` | `pronouns` 为空；`ship_ids=[ship_id]`；`estate_ids=[]`；`inventory_id` 由旧玩家库存生成稳定引用；`credit` 为 0 或由 Finance 迁移；地图/图鉴从 D3/D5 发现和生态记录迁移。 |
| Ship | 通用/实例字段；`hull_sections`、`mass_state`、`room_instances`、`function_points`、`anchor_state`、`mooring_state`、`maintenance_orders`、`appearance`、`ownership` | 旧 `hull` 作为整体耐久；新分区按船型默认健康状态；质量/吃水由 ShipSystem 重算；船权利状态默认 `owned`，不得覆盖旧船归属。 |
| Crew | 通用/实例字段；`religion`、`legal_status`、`occupation`、`hunger`、`thirst`、`injury`、`disease`、`respect`、`fear`、`attachment`、`jealousy`、结构化 `memory`、`goals`、`promises`、`debts`、`current_task`、`work_shift`、`appearance_loadout_id`、`equipment_ids`、`pet_id`、`personal_inventory_id` | 旧 stats/persona 保留；新增数值按系统默认值填充；AgentCore 现有公共类型不改，D8-A01 使用 DTO/适配层承载扩展。 |
| Relation | 通用字段；`source_id`、`target_id`、`relation_type`、`value`、`confidence`、`cause_event_ids`、`last_changed_day`、`locked_by_story`、`visibility` | 旧 affinity 只能迁移为一条可追溯性受限的 affinity 边；未知原因使用 `legacy_d7_relation` 标记，不伪造事件 ID。 |
| Island | 通用/实例字段；`terrain/geology`、`climate`、`water_sources`、`habitats`、`population`、`ecology_metrics`、`asset_ids`、`controller_ref` | D7 目录字段保留；自然档案和社会结构默认空集合/待设计值，不从展示名推断。 |
| LandParcel | 通用/实例字段；`purpose`、`fertility`、`water_source_ref`、`lease`、`owner_ref`、`risk` | 空地块不得自动拥有者；未迁移的 `estate_ids` 只保存为待调查资产引用。 |
| Building | 通用/实例字段；`function`、`level`、`inventory`、`worker_ids`、`damage`、`open_state`、`ownership` | D7 船坞镜像只作迁移输入；建筑库存、工人和损伤默认空/零，不能复制船坞升级为建筑等级。 |
| Item | 通用定义字段；`category`、`tags`、`description`、`gameplay_role`、`dependencies`、`asset_refs`、`source_refs`、`license`、`status`；D8-A01 需确定 `weight`、`volume`、`durability` 的明确类型 | 旧 `cargoVolume` 映射到 `volume`；`weight`、`durability` 无历史值时使用目录默认，不把价格当重量或耐久。 |
| Animal | 通用/实例字段；独立 `species`、`age`、`sex`、`variant`、`status`、`habitat`、`behavior`、`legal` | 旧 `animalId`、`health`、`observed`、`collectionValue` 全保留；年龄/性别/变体/法律状态使用明确 unknown 默认值。 |
| FishSchool | 通用/实例字段；`fish_species`、`quantity`、`depth`、`migration`、`fishing_pressure`、`recovery` | D7 `fish_school` 只映射 `species=unknown`、数量/压力/恢复待生态系统初始化；禁止在迁移时生成鱼群数量。 |
| Pet | 通用/实例字段；`breed`、`health`、`hunger`、`cleanliness`、`attachment`、`location`、`care_state` | 默认无 Pet 实例；Crew 的 `pet_id` 为空或保留未解析引用。 |
| FinanceAccount | 通用字段；`account_type`、`owner_ref`、`principal`、`interest`、`guarantee`、`mortgage`、`due_day`、`default_state`、`ledger_refs` | 旧 `money` 只迁移成账户余额/现金入口；D8 不创建贷款、保险、抵押或债务结果。 |
| Quest | 通用/实例字段；`status`、`objectives`、`prerequisites`、`choices`、`failure_state`、`recovery_route`、`reward_requests`、`fact_event_ids` | `reward_requests` 只能是请求；RewardSystem 才能写 RewardGrant。旧主线 flags/完成列表只读迁移。 |
| WorldEvent | 事件包字段：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`proposal`、`result`、`cause_ids`、`save_revision`；D8-A03 的 `outcome_status`、`recovery_route`、重复结算保护 | 事件先追加事实再由所属系统写状态；`proposal` 不代表结果；同一 `event_id + save_revision` 不得重复发奖。 |
| Reward | 通用字段；`reward_id`、`source`、`quantity`、`unique`、`claim_record`、`reclaim/recovery_route` | 默认未领取；数量、金钱、物品和产权必须由 RewardSystem/Economy/Inventory 等所属系统实际结算，Agent/Quest 不可直写。 |
| AppearanceLoadout | 通用/实例字段；`loadout_id`、`body`、`hair`、`clothing`、`accessories`、`equipment`、`durability`、`source` | D5 `presetId/skinTone/hairStyle/hairColor/outfit/faceScale/bodyScale` 映射为旧兼容层；新槽位默认空集合或目录默认。 |

## 5. 核心实体冻结矩阵

说明：`唯一写者`是领域事实的 canonical owner；`存档写入口`统一是 `SaveSystem`，只负责序列化/迁移/还原，不获得实体裁决权。`读取者`列的是允许读取快照的系统；UI 和 Agent 只能作为读取者或提议者。

| 实体（canonical 名） | 唯一写者 / 字段分区 | 读取者 | 冻结存档字段 | 版本责任 | 测试入口 | 跨端/Server/Web 边界 |
|---|---|---|---|---|---|---|
| **Player** (`PlayerProfile`) | `Player/Core` 聚合入口；Character 写姓名/称谓/身份/外观引用，Progression 写技能，Economy/Finance 写余额/信用，Faction 写声望，Property 写资产引用，Inventory 写库存，Discovery/Collection 写地图/图鉴。SaveSystem 只落库。 | Ship、Crew、Relation、Quest、UI；Agent 只读筛选后的玩家快照；Server 只读 Agent 上下文。 | D7 全部 PlayerState + `location` + D3/D4/D5 快照；v3 通用/实例字段、`pronouns`、`identity_history`、`appearance_loadout_id`、`credit`、ID 引用集合。 | D7 来源 `user_version=1`；canonical `schema_version=1`；目标存档 `save_version=3`。 | `SaveSystemTests`、`D5SaveSystemTests`、D7 `D7ControlStateTests`；D8-A01/A05 增加空档、D7 档、完整档迁移测试。 | **Contract DTO only**：现有 D3 trade、D5 world/customization 可投影；完整本地存档不进 server/web。官网暂不持有 Player 权威状态。 |
| **Ship** (`ShipInstance`) | `ShipSystem` 写航行/船体/舱室/锚泊/损伤；Cargo 写货物；Crew 写岗位引用；Economy/Finance 写费用/抵押；Appearance 写外观；canonical 聚合 owner 为 Ship boundary。 | Player、Crew、Economy、Environment、Combat、Quest、UI；Agent 只能读摘要并提议操作。 | `ships` 表旧 12 类字段、`crew_json`、`ship_log_json`；D3/D4 航行/战斗/船坞镜像；v3 分区损伤、质量、舱室、功能点、锚泊、维修、外观、所有权和通用/实例字段。 | 船型定义的 `hull_max` 继续独立版本；实例 canonical `schema_version=1`；迁移必须保留 `HullMax` 回查规则。 | `SaveSystemTests`、`D3VoyageTests`、`D3InventoryTests`、D4 `D4CombatTests`、`D5SaveSystemTests`。 | **现有 Contract DTO + game-only full save**：D3 voyage/trade 和 D4 combat 继续是只读镜像；server 不替代 ShipSystem，web 不保存 ShipInstance。 |
| **Crew** (`CrewAgent`) | `CrewSystem` 写角色状态、岗位、关系引用和记忆引用；AgentCore 只生成提议；Character/Appearance 写装扮；Inventory 写个人物品。 | Player、Ship、Relation、Quest/Event、UI、AgentCore；Server gateway 只读最小 persona/记忆摘要。 | `crew_agents` 全部旧列及 JSON；D4 health；D5 trait IDs；v3 身份、身体状态、关系引用、目标/承诺/债务、任务/轮班、外观/装备/宠物/个人库存引用和通用/实例字段。 | 保留 D7 `relations_json`/`memory_json` 占位；canonical `schema_version=1`；AgentReply 版本与实体版本分开。 | `AgentCoreTests`、`D2DialogueTests`、`CrewRelationSystemTests`、`SaveSystemTests`、`D4EcologyMedicalTests`。 | **Contract DTO**：Agent chat 需要 Crew 的最小快照；`agent-reply.schema.json` 是提议契约。完整 Crew 存档、记忆和数值不进入 server/web 权威库。 |
| **Relation** (`RelationEdge`) | `CrewSystem` 唯一写事实关系；`AgentReply` 只能提议 affinity/action；EventSystem 提供 cause 引用；UI/Agent 不写值。 | Crew、Player、Quest/Event、UI、Agent 摘要；Faction 可读跨势力后果。 | D7 affinity 标量 + 空 `relations_json`；v3 `source_id`、`target_id`、`relation_type`、`value`、`confidence`、`cause_event_ids`、`last_changed_day`、`locked_by_story`、`visibility`。 | 关系边 `schema_version=1`；旧 affinity 迁移为 legacy edge；每次变化要求 fact event 引用。 | `CrewRelationSystemTests`；D8-A01/A03 增加 edge clamp、cause、重复事件和读档测试。 | **Contract DTO / proposal only**：AgentReply 已跨端；完整 RelationEdge 不进入 server/web，server 不裁决关系结果。 |
| **Island** (`IslandInstance`) | `IslandSystem` 唯一写岛屿实例；EcologySystem 只写自己的 Animal/FishSchool/Vegetation 实体并通过事实事件提供生态汇总；Property/Building 不直接改岛屿字段。 | Player、Ship、Environment、Ecology、Property、Building、Faction、Quest、UI。 | D3 port 目录、D5 island 定义和世界发现/访问快照；v3 自然档案、控制者、人口、生态指标、资产引用和通用/实例字段。 | 目录 Def 与 Instance 分开版本；canonical `schema_version=1`；D5 12 岛状态作为 legacy projection。 | `D3ContractTests`、`D3EconomyTests`、`D5ContentTests`、`D5SaveSystemTests`。 | **现有 Contract DTO + game-only full instance**：D5 `world_state` 可投影；完整自然/人口/资产状态暂不进 server/web。 |
| **LandParcel** | `PropertySystem` 唯一写用途、肥力、水源、租约、所有者和风险；Island/Building 只读并发出请求。 | Player、Island、Building、Finance、Faction/Legal、Quest、UI。 | v3 通用/实例字段 + `purpose`、`fertility`、`water_source_ref`、`lease`、`owner_ref`、`risk`；无 D7 字段可迁移。 | canonical `schema_version=1`；D8 只定义默认值和空引用；D13 再升级业务版本。 | D8-A01 的空/完整实体和序列化测试；D13 Property tests 是后续入口。 | **Game-only（D8）**：不建 server/web DTO，不从 `estate_ids` 反推地块。 |
| **Building** (`BuildingInstance`) | `BuildingSystem` 唯一写功能、等级、库存、工人、损伤、开放状态和建筑 ownership 引用；PropertySystem 只写独立 OwnershipClaim。 | Island、LandParcel、Player、Economy、Finance、Shipyard、Quest、UI。 | v3 通用/实例字段 + `function`、`level`、`inventory`、`worker_ids`、`damage`、`open_state`、`ownership`；D4 船坞状态只作 legacy projection。 | canonical `schema_version=1`；D8 预留存档形状；D13 实现经营字段时必须保持 ID 和迁移。 | D8-A01 空/完整实体测试；D13 Building/Property tests 后续补齐。 | **Game-only（D8）**：不进入 server/web；D9 只可用本地代表样例。 |
| **Item** (`ItemDefinition` + 持有量引用) | `ItemCatalog/ItemSystem` 唯一写不可变定义；Inventory/Cargo 写持有量/耐久/处理；Economy 写市场库存和价格。不能用一个 owner 同时写定义和余额。 | Player、Ship、Building、Economy、Quest、Reward、UI；Agent 只读允许的物品摘要。 | D3 commodity 字段、cargo/personal/inventory/market 数量；v3 通用内容字段、`category`、`tags`、`gameplay_role`、依赖/资产/来源/授权/status；D8-A01 确定 weight/volume/durability 类型。 | Definition canonical `schema_version=1`；持有量随 Player/Ship/Building 所属存档版本；旧 `cargoVolume` 映射为 volume。 | `D3ContractTests`、`D3InventoryTests`、`D3EconomyTests`、`D4ContentTests`。 | **现有 Contract DTO + game-only holdings**：D3 commodity/trade 可跨端；完整库存、耐久和资产归属暂不进 server/web。 |
| **Animal** (`AnimalRecord`) | `EcologySystem` 唯一写物种实例、健康、行为、观察、法律和生态压力；AnimalCare 只能写 Pet/照料实体。 | Island、FishSchool、Player、Quest、Collection、UI；Agent 只读观察摘要。 | D4 AnimalDefinition/AnimalState/EcologyState 全字段；v3 独立 species、年龄/性别/变体、status、habitat、behavior、legal 和通用/实例字段。 | canonical `schema_version=1`；D4 `animalId` 作为兼容引用；生态状态按 `revision` 保存。 | `D4ContentTests`、`D4EcologyMedicalTests`、`SaveSystemTests`。 | **现有 Contract DTO + game-only full record**：D4 ecology mirror 可跨端；server/web 只能读/校验投影，不写动物状态。 |
| **FishSchool** | `EcologySystem` 唯一写鱼种、数量、深度、迁徙、捕捞压力和恢复；Fishing/Catch 只能提交捕捞请求。 | Island、Ship、Environment、Item、Quest、UI；Agent 只读观察报告。 | v3 通用/实例字段 + `fish_species`、`quantity`、`depth`、`migration`、`fishing_pressure`、`recovery`；D7 只有 `fish_school` 占位标记。 | canonical `schema_version=1`；D7 marker 只读；D11 建立行为/恢复迁移测试。 | D8-A01 空/完整实体测试；D11 FishSchool tests 后续补齐。 | **Game-only（D8）**：不把 D4 的 fish_school marker 扩成 server API；D11 冻结后再评估只读观察 DTO。 |
| **Pet** (`PetInstance`) | `AnimalCare/Farming` 唯一写健康、饥饿、清洁、依恋、位置、照料和死亡/走失状态；Crew 只保存 pet_id 引用。 | Crew、Player、Ship、Island、Item、Quest、UI。 | v3 通用/实例字段 + `breed`、`health`、`hunger`、`cleanliness`、`attachment`、`location`、`care_state`；无 D7 实体字段。 | canonical `schema_version=1`；默认无实例；D11 负责后续版本。 | D8-A01 空实体/未知引用测试；D11 Pet tests 后续补齐。 | **Game-only（D8）**：不进入 server/web。 |
| **FinanceAccount** | `FinanceSystem` 唯一写本金、利息、担保、抵押、到期和违约；Economy 只提交合法结算输入；Player 只读余额。 | Player、Property、Building、Ship、Economy、Quest、UI。 | v3 通用字段 + `account_type`、`owner_ref`、`principal`、`interest`、`guarantee`、`mortgage`、`due_day`、`default_state`、`ledger_refs`；D7 money 作为迁移输入。 | canonical `schema_version=1`；D8 默认无账户/余额入口；D13 再启用金融状态。 | D8-A01 空/负债/资产负债档迁移测试；D13 Finance tests 后续补齐。 | **Game-only（D8）**：不进入 server/web；服务端 wallet 是 Agent 点数/账号余额，不是游戏 FinanceAccount。 |
| **Quest** (`QuestInstance`) | `QuestSystem` 唯一写任务状态、目标、选择、失败和恢复引用；EventSystem 写事实事件；RewardSystem 写实际奖励；Agent/UI 只能建议/读取。 | Player、Crew、Island、WorldEvent、Reward、Faction、Economy、UI、Agent 摘要。 | D4 mainline `chapter/missionIndex/flags/completedMissions`；v3 `status`、`objectives`、`prerequisites`、`choices`、`failure_state`、`recovery_route`、`reward_requests`、`fact_event_ids`。 | canonical `schema_version=1`；D4 主线作为 legacy projection；Quest 不复制 money/inventory/relation。 | `D4MainlineLogTests`、`SaveSystemTests`；D8-A01/A03 增加失败/恢复/事实引用测试。 | **Contract projection only**：现有 D4 mainline mirror 继续只读；完整任务状态暂不进 server/web。 |
| **WorldEvent** | `EventSystem/EventLog` 唯一追加事件包；事件的 `owner_system` 只标识哪一系统裁决结果，不把裁决权移交 EventSystem；各领域系统写自己的状态。 | 所属领域系统、Quest、Reward、UI、Agent 摘要、Save/Migration；Server/Web 只可读投影。 | D3 event IDs/seeds、D4 route log；v3 事件包全字段、`outcome_status`、`recovery_route`、`save_revision` 和去重标记。 | event `schema_version=1`；`event_id + save_revision` 去重；D8-A03 负责跨域事件包和恢复测试。 | `D3VoyageTests`、`D4MainlineLogTests`；D8-A03 事件包测试、D8-A05 读档重复结算测试。 | **D8 requires Contract schema, no active endpoint yet**：D8-A03 建立统一事件契约前不接 server/web；游戏内事件日志是权威。 |
| **Reward** (`RewardGrant`) | `RewardSystem` 唯一写发放、领取、唯一性、回收和恢复；Economy/Inventory/Property 只负责各自结果账本；Quest 只能写 RewardRequest。 | Quest、Player、Inventory、Ship、Finance、Collection、UI、Agent 摘要。 | D5 成就解锁只作为 legacy external grant marker；v3 `reward_id`、`source`、`quantity`、`unique`、`claim_record`、`reclaim/recovery_route` 和通用字段。 | canonical `schema_version=1`；D8 默认未领取；D14 扩展前不创建实际奖励内容。 | D8-A01 空/唯一/重复领取测试；D8-A03 重复事件不重复发奖；D14 Reward tests 后续补齐。 | **Game-only（D8）**：不进入 server/web，不允许 AgentReply 携带最终 reward。 |
| **AppearanceLoadout** | `AppearanceSystem/Character` 唯一写外观组合和耐久/来源；D5 `D5CharacterSystem` 是旧兼容裁决入口；UI 只提交试穿/选择请求。 | Player、Crew、Ship（船舶外观）、Character UI、Quest/Collection；Agent 只读可见外观标签。 | D5 character state/preset 全字段；v3 `loadout_id`、`body`、`hair`、`clothing`、`accessories`、`equipment`、`durability`、`source` 和通用/实例字段。 | canonical `schema_version=1`；D5 preset 映射必须可逆；未识别槽位保留 legacy source。 | `D5ContentTests`、`D5SaveSystemTests`；D8-A01 外观序列化/未知槽位测试。 | **Existing Contract DTO + game-only full loadout**：D5 `character_state`/customization 继续是只读投影；完整 loadout 和耐久不进 server/web。 |

## 6. game / contracts / server / web 分层结论

### 6.1 进入 contracts 的实体投影

进入 `contracts/` 的是跨端 DTO/事件投影，不等于把实体的裁决权交给服务端：

| 投影 | 当前状态 | D8 后续责任 |
|---|---|---|
| Crew 最小 Agent 快照 + `AgentReply` | `contracts/agent-reply.schema.json` 已存在；OpenAPI `/agent/chat` 已引用 | D8-A04 扩展 Schema/AgentCore/server 校验，禁止最终数值结果字段。 |
| Item/Ship/Island 的贸易与航程投影 | `contracts/d3-trade.schema.json`、OpenAPI `/trade/*` 已存在 | D8-A01 只补 DTO 映射和版本字段，不破坏 D3 字段。 |
| Animal/生态投影 | `contracts/d4-content.schema.json`、OpenAPI `/animals/encounter` 已存在 | FishSchool 不能直接复用 AnimalState；D11 冻结后再加专用观察 DTO。 |
| Island 世界发现/AppearanceLoadout/外观投影 | `contracts/d5-release.schema.json`、OpenAPI `/world/islands`、`/profile/customization` 已存在 | D8 保留 D5 字段兼容；完整 v3 实例仍为 game-only。 |
| Relation 提议 | 通过 `AgentReply.affinity_change` 进入，完整 RelationEdge 不跨端 | D8-A04 不能让 server 写关系结果；D8-A03 为结果补 fact event。 |
| WorldEvent 统一事实包 | 当前没有 D8 统一 Schema | D8-A03 新增前先确认 game/server/web 的同步任务；在此之前只在 game 内部使用。 |

### 6.2 暂不进入 server/web 的实体

以下实体在 D8 只建立 game 内部 canonical 模型和存档责任：`LandParcel`、`Building`、`FishSchool`、`Pet`、`FinanceAccount`、`Reward`；此外 Player/Ship/Crew/Island/Item/Animal/Quest/AppearanceLoadout 的**完整存档实例**也不进入 server/web。原因是本产品当前是单机本地权威，server 负责账号、点数和 Agent 网关，不是游戏世界裁决服务器；`web/` 当前尚未建立，也不能成为实体写者。

任何后续需要跨端的新增字段必须先有 contracts，再同步 game/server/web；不能因为某个 UI 或服务端镜像暂时方便而绕过本表。

## 7. 已知冲突与冻结决议

| 冲突 | 冻结决议 | 影响任务 |
|---|---|---|
| `money` 同时存在于 PlayerState、D3RuntimeSnapshot、D3TradeState；`hull` 同时存在于 ShipState、D3 snapshot、D4 combat | D7 字段全部保留为兼容镜像；D8 canonical owner 分别是 FinanceSystem 和 ShipSystem；迁移器标记旧镜像来源，禁止三处继续互写。 | D8-A01、D8-A05 |
| Crew 健康/疲劳同时存在于 CrewStats 和 D4 `HealthState` | CrewSystem 的 Crew canonical 状态为主；D4 health 是旧快照投影，迁移时按事件/时间优先级合并，冲突不得静默覆盖。 | D8-A01、D8-A05 |
| `traits` 同时有 AgentCore persona 文本、Save trait IDs、D5 trait IDs | ID 是数据事实；Persona 文本是展示/LLM 投影；保留 D7 两种字段，D8-A01 建立单向适配。 | D8-A01 |
| `relations_json` 为空，只有 affinity 标量 | 不把空 JSON 当成完整关系图；只迁移可证明的 affinity，并标记 legacy 来源；其余关系由事实事件产生。 | D8-A01、D8-A03、D8-A04 |
| `ShipState.CreateNew` 接收 `ownerId`，但 D7 `ShipState`/`ships` 表没有持久化 `owner_id` | D7 不回填不存在的字段；当前活动船的归属只能由 `Player.ship_id` 作为迁移线索，并标记为 inferred；D8 `ownership` 由 Property/Legal 继续裁决。 | D8-A01、D8-A05、D13 |
| DATA_MODELS 的 Island writer 写作 `Island/Ecology`，但唯一写者原则要求单一 owner | IslandSystem 负责 IslandInstance 字段；EcologySystem 负责 Animal/FishSchool/Vegetation，并通过事件提供生态汇总，IslandSystem 决定是否写入岛屿摘要。 | D8-A01、D8-A03、D8-A06 |
| `common/02_DATA_GOVERNANCE.md` 曾要求 `created_at/updated_at`，DATA_MODELS 定义 `created_day/updated_day` | 游戏实体统一使用世界日字段；技术审计时间只能是存储元数据，不得作为第二套实体字段。 | D8-A01、D8-A05 |
| D4 `fish_school` 是 AnimalId 枚举值，但 v3 要求 FishSchool 实体 | 保留 D4 marker；不把 marker 当实体数量或行为模型；D11 冻结专用 FishSchool。 | D8-A01、D11 |
| D5 `shipyard` 状态可能被误当 Building | 仅作为 Shipyard/upgrade legacy projection；BuildingInstance 另建 ID、owner 和存档字段。 | D8-A01、D13 |
| 现有 OpenAPI 端点写着“镜像”，容易被误解为 server 权威 | 统一解释为校验/回显 DTO；本地 game 系统仍是数值裁决者；D8 不扩大 server 权限。 | D8-A04、D8-S02 |

## 8. D8-A01—D8-A06 使用说明

| 任务 | 只允许依据本表做什么 | 不得提前做什么 |
|---|---|---|
| D8-A01 | 建立实体 DTO、稳定 ID、默认值、序列化和空值策略；保留 D7 字段 | 不改 AgentCore 现有公共类型；不把 game-only 实体接入 server/web。 |
| D8-A02 | 注册前缀、来源、授权、owner、phase、test_id；只做代表样例 | 不批量生产 50 岛、300 动物、200 道具等内容。 |
| D8-A03 | 建立 WorldEvent/FactEvent 事实包、失败/恢复/重复保护 | 不让 EventSystem 直接写经济、关系、动物或奖励数值。 |
| D8-A04 | 扩展 AgentReply/P2A 提议契约和三端校验 | 不让 AgentReply 携带最终 money、price、damage、reward、weather 或 relation 结果。 |
| D8-A05 | 把 D7 `user_version=1` 存档迁移到 `save_version=3`，保留旧字段 | 不删除 D7 字段、不覆盖不可迁移原档、不把 UI 猜测当迁移规则。 |
| D8-A06 | 统一金额、重量、体积、时间、关系/健康范围、天气、稀有度、法律、品质的配置入口 | 不在本任务调整经济平衡或让配置成为跨系统写者。 |

## 9. D8-S01 验收结论

- [x] 16 个核心实体均有 canonical 名称和现有类型映射；
- [x] D7 已有字段与 v3 新字段分开列出，D7 字段保留规则明确；
- [x] 每个实体均有唯一写者、读取者、存档字段、版本责任和测试入口；
- [x] game-only、contracts 投影、server/web 暂不进入范围已明确；
- [x] 重复镜像、空关系占位、鱼群 marker、船坞/建筑混淆和时间字段冲突已记录决议；
- [x] 未修改领域业务逻辑、AgentCore 公共类型或现有 D7 存档字段；
- [ ] D8-A01—D8-A06 的实现、契约三端同步和迁移测试仍由后续任务完成，不在 D8-S01 中提前标记完成。
