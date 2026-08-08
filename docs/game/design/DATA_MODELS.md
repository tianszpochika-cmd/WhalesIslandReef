# 数据模型（DATA_MODELS）— M0 定稿

> 状态：**M0 最小集已定稿**（2026-08-08）｜ M1/M2 实体为骨架待填。
> 约定：字段类型与归属遵循 DESIGN_BOUNDARIES 裁决权表；存档映射 SQLite 表（第 7 节）。
> 与 AgentCore 的关系：`CrewStats` 已在代码中；`Traits/Culture/Role` 为本定稿新增字段，**待 Unity 阶段扩展代码**。

---

## 1. Player（玩家角色）— 归属 Core

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | string PK | 玩家实例 ID（与服务端 user_id 关联） |
| `name` | string | 显示名 |
| `money` | int | 金币（经济系统唯一写者） |
| `skills` | dict\<SkillType,int\> | 玩家技能：领航/贸易/格斗/医术/木工/魅力（默认 1） |
| `traits` | TraitId[] | 玩家特质 3-4 条（引用 TraitDef） |
| `reputation` | dict\<FactionId,int\> | 声望（M1 启用，M0 预留空表） |
| `inventory` | ItemId→count | 随身物品（工具/装备/食材） |
| `shipId` | string FK | 当前拥有的船 |
| `location` | Location | 所在港口 id 或海域坐标 |
| `playSeconds` | int | 累计时长（成就/统计用） |

## 2. Ship（船实例）— 归属 5 船只系统

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | string PK | |
| `shipTypeId` | string FK | 引用 ShipTypeDef |
| `name` | string | 船名（玩家可改） |
| `hull` / `hullMax` | int | 当前/最大耐久（破损→进水→倾斜→沉船链路） |
| `cargo` | dict\<ItemId,int\> | 货舱（商品/货物）；容量受 cargoMax 约束 |
| `cannons` | int | 炮数；装填状态简化：`loadedCount` int |
| `supplies` | { water:int, food:int } | 淡水/食物（生存系统消耗） |
| `crew` | { capacity:int, list:CrewId[] } | 船员槽位 |
| `upgrades` | dict\<Slot,level\> | 改装等级（hull/sail/cannon/medBay/galley/cabin）——M2 启用，M0 留默认 0 |
| `state` | { tilt:float, flooding:int } | 倾斜角/进水——M2 简化物理启用 |
| `shipLog` | LogEntry[] | 航线日志（自动 + AI 摘要） |

## 3. CrewAgent（船员）— 归属 2 船员系统

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | string PK | |
| `name` / `role` | string / RoleId | 姓名 / 岗位（舵手·瞭望·炮手·厨师·医生·木工·普通水手） |
| `culture` | CultureId | 文化群：eng/spa/fra/por/carib |
| `traits` | TraitId[] | 3-4 条（引用 TraitDef，含数值修正） |
| `stats` | CrewStats | ✅ 已在 AgentCore：morale/loyalty/health/fatigue/affinity/skills |
| `persona` | PersonaDef | ✅ 已在 AgentCore：role/traits/backstory/constraints |
| `memory` | AgentMemory | ✅ 已在 AgentCore：三层记忆 + IMemoryStore |
| `relations` | dict\<CrewId\|player,int\> | 关系值（-100~100）；爱情标记 M1 启用 |

> ⚠️ AgentCore 扩展清单（Unity 阶段实施）：`CrewStats` 保持数值；`Persona.Traits` 从 `List<string>` 改为 `TraitId[]` 引用 + 数值修正注入；新增 `culture`、`role`、`relations`。

## 4. TraitDef（特质定义表，纯数据）— 归属 2 船员系统

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | string PK | `trait_lung_disease` |
| `name` / `category` | string / enum | 名称 / 身体·性格·经历·疾病 |
| `desc` | string | 展示文案 |
| `statMods` | dict\<stat,float\> | 数值修正（如 `fatigue_max:-0.2`） |
| `personaNote` | string | 注入 Persona 的文本（"你时常咳嗽…"） |
| `eventMods` | dict\<eventId,float\> | 事件概率修正（如贪婪 `mutiny:+0.05`） |

M0 首批 20 条（身体 8 + 性格 8 + 经历 4）；M1 扩至 50（含疾病 5：坏血病/疟疾/黄热病/痢疾/肺痨）。

## 5. ShipTypeDef（船型定义表，纯数据）— 归属 5 船只系统

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` / `name` | string | `small_fishing` 小渔船 / `sloop` 单桅纵帆船 |
| `speed` / `turn` | float | 航速 / 转向 |
| `cargoMax` / `crewMax` | int | 载重 / 船员上限 |
| `cannonMax` / `hullMax` | int | 炮位 / 耐久 |
| `draft` | float | 吃水（近岸/浅滩判定） |
| `price` | int | 造价（史实锚定，BALANCE 填充） |

M0 首批 2 种；M1 增至 5 种（双桅纵帆/横帆快船/小型盖伦）；M3 全 8 种。

## 6. 支撑类型

```
Location   = { type: "port"|"open_sea", portId?: string, x?: float, y?: float }
ItemId     = string（商品/工具/食材统一 ID 表，BALANCE 填充）
RoleId     = "helmsman"|"lookout"|"gunner"|"cook"|"medic"|"carpenter"|"deckhand"
CultureId  = "eng"|"spa"|"fra"|"por"|"carib"
FactionId  = "england"|"spain"|"france"|"portugal"|"pirates"|"tribes"|"smugglers"
SkillType  = ✅ AgentCore 已定义（Sailing/Navigation/Gunnery/Carpentry/Medicine/Cookery/Trading/Combat）
```

## 7. 存档映射（SQLite，本地 + 后续 Steam Cloud）

```sql
players(id, name, money, skills_json, traits_json, reputation_json, inventory_json, ship_id, location_json, play_seconds)
ships(id, ship_type_id, name, hull, cargo_json, cannons, loaded_count, supplies_json, crew_json, upgrades_json, state_json, ship_log_json)
crew_agents(id, name, role, culture, traits_json, stats_json, persona_json, memory_json, relations_json)
trait_defs(id, name, category, desc, stat_mods_json, persona_note, event_mods_json)   -- 种子数据
ship_type_defs(id, name, speed, turn, cargo_max, crew_max, cannon_max, hull_max, draft, price) -- 种子数据
```

## 8. 契约影响

M0 无跨端契约变更（Player/Ship/CrewAgent 为客户端本地实体；云端仅存 `user/wallet/usage`，已在 TOKEN_SYSTEM 定稿）。若后续做云存档，再扩展 openapi。

## 9. M1/M2 待定稿实体（骨架）

Trader（底价区间/库存/关税）→ M1｜Island（类型/资源/动物/藏宝）→ M1｜EventTemplate（触发/后果）→ M1｜Faction（声望/政策）→ M2｜Animal（物种/行为机/危险度）→ M2｜Item 表 → M1
