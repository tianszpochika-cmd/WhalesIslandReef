# D8-A01 v3 实体 DTO 目录

> 任务：D8-A01
> 状态：已实现纯 C# 数据模型和序列化自测
> 代码入口：`game/AgentCore/V3EntityModels.cs`
> 测试入口：`game/AgentCore/V3EntityModelTests.cs`
> 依赖：`D8_DATA_BOUNDARY_FREEZE.md`

本目录只定义 v3 数据形状、稳定 ID、单位类型、默认值、兼容字段和序列化边界，不实现玩法结算。实体的唯一写者、读取者、存档责任以 D8-S01 冻结表为准；本目录不改变任何系统所有权。

## 1. 代码边界

- 命名空间为 `Whalebone.Agent.V3`，不修改现有 `Whalebone.Agent` 公共类型。
- `game/AgentCore/` 仅引用 .NET 标准库；没有 Unity、MonoBehaviour、Renderer、GameObject、网络或 LLM 依赖。
- v3 类型是 DTO/存档模型，不拥有经济、战斗、关系、生态、任务或奖励裁决逻辑。
- `SaveSystem` 后续可以把 D7 Systems 类型适配到 `LegacyD7*Fields`，但本任务不直接改 D7 存档代码。

## 2. 版本与通用字段

| 项目 | 冻结值 |
|---|---|
| 实体 `schema_version` | `1`，由 `V3ModelVersions.EntitySchemaVersion` 提供 |
| 目标存档 `save_version` | `3`，由 `V3ModelVersions.TargetSaveVersion` 提供 |
| 共同字段 | `id`、`schema_version`、`display_name_key`、`source_note`、`historical_tag`、`phase`、`owner_system`、`test_id`、`created_day`、`updated_day`、`location_ref`、`state_flags`、`revision` |
| 存档级字段 | `save_version`、`world_revision`、`last_event_id`、`migration_flags`，位于 `V3SaveMetadata` |
| 时间字段 | `WorldDay` 为非负世界日；`TimeOfDay` 为 0..1；不使用技术时间替代玩法时间 |
| 缺省版本 | 缺少 `schema_version` 时由模型默认成 `1`；D8-A05 再负责旧存档迁移判断 |

空实体使用 `new XxxEntity()`：必需字符串为空、集合为空、枚举为 `Unknown`、数值按安全默认值初始化。可选引用统一为空字符串或空集合，不用伪造 ID；`LegacyD7*Fields` 始终存在，用于保留 D7 字段形状。

## 3. 明确单位与范围类型

| 类型 | 含义 | 默认/范围 |
|---|---|---|
| `MoneyAmount` | 金币或账本金额，整数金币 | `0`；允许负值表示债务/净额，不做结算 |
| `WeightAmount` | 重量，公吨 | `0`；负输入夹为 `0` |
| `VolumeAmount` | 体积，立方米 | `0`；负输入夹为 `0` |
| `WorldDay` | 世界日 | `0` 起；负输入夹为 `0` |
| `TimeOfDay` | 一日内归一化时间 | `0`；夹到 `0..1` |
| `RelationValue` | 关系值 | `-100..100`，默认 `0` |
| `PercentValue` | 健康、疲劳、耐久、风险、压力等百分比 | `0..100`；默认按字段语义为 `0` 或 `100` |

模型层只做类型边界和安全默认值，不推进时间、不修改关系、不计算价格、利息、伤害、奖励或生态恢复。

## 4. 稳定 ID 规则

`V3StableIds.Create(kind, slug)` 是纯函数：先把 slug 转为小写 ASCII，非字母数字字符折叠为下划线，再拼接固定前缀；不使用随机数、当前时间或静态运行时注册表。相同 kind 和 slug 永远得到相同 ID。`IsValid` 检查前缀和 `[a-z0-9_]+` 后缀。

| 实体 | `V3EntityKind` | 前缀 | 代码类型 |
|---|---|---|---|
| Player | `Player` | `player_` | `PlayerEntity` |
| Ship | `Ship` | `ship_` | `ShipEntity` |
| Crew | `Crew` | `crew_` | `CrewEntity` |
| Relation | `Relation` | `relation_` | `RelationEntity` |
| Island | `Island` | `island_` | `IslandEntity` |
| LandParcel | `LandParcel` | `parcel_` | `LandParcelEntity` |
| Building | `Building` | `building_` | `BuildingEntity` |
| Item | `Item` | `item_` | `ItemEntity` |
| Animal | `Animal` | `animal_` | `AnimalEntity` |
| FishSchool | `FishSchool` | `fish_` | `FishSchoolEntity` |
| Pet | `Pet` | `pet_` | `PetEntity` |
| FinanceAccount | `FinanceAccount` | `finance_` | `FinanceAccountEntity` |
| Quest | `Quest` | `quest_` | `QuestEntity` |
| WorldEvent | `WorldEvent` | `event_` | `WorldEventEntity` |
| Reward | `Reward` | `reward_` | `RewardEntity` |
| AppearanceLoadout | `AppearanceLoadout` | `outfit_` | `AppearanceLoadoutEntity` |

内容目录的来源、授权、phase 和 `test_id` 规则仍由 D8-A02 负责；本任务只提供实体 ID 生成和验证入口。

## 5. 实体字段目录与边界

| 实体 | v3 DTO 主要字段 | D7 兼容字段 | 数据边界摘要 |
|---|---|---|---|
| `PlayerEntity` | 状态、身份代际、称谓/外观引用、typed balance/credit、船员/船/关系/任务/收藏引用 | `LegacyD7PlayerFields` 保留 `id/name/money/skills/traits/reputation/inventory/ship_id/location/play_seconds` 和 D3/D4/D5 原始快照 JSON 边界 | Player/Core 聚合；Finance/Economy/Progression 等只写自己的字段；完整存档 game-only |
| `ShipEntity` | 状态、`hull_integrity`、重量、体积、舱室、功能点、锚泊、维修、ownership、外观 | `LegacyD7ShipFields` 保留 `id/ship_type_id/name/hull/cargo/cannons/loaded_count/supplies/crew_capacity/crew_ids/upgrades/state/ship_log`；`hull_max` 不落实体 | ShipSystem 为船舶事实写者；船型定义继续提供 hull max |
| `CrewEntity` | 身份、岗位/文化 ID、状态、trait IDs、health/fatigue、关系/目标/承诺/债务/任务/轮班/装扮/装备/宠物/个人物品引用 | `LegacyD7CrewFields` 保留 `id/name/role/culture/traits_json/stats_json/persona_json/memory_json/relations_json` | CrewSystem 为角色事实写者；AgentCore 只提议；空 JSON 不当完整关系/记忆模型 |
| `RelationEntity` | source/target、关系类型、typed value/confidence、cause events、变更日、剧情锁、可见性 | D7 affinity 由适配层放入 RelationValue；空 `relations_json` 只保留占位 | CrewSystem 写关系结果；AgentReply 只能提议 |
| `IslandEntity` | 地形、地质、气候、水源、生境、人口、生态指标、资产、控制者 | D3 PortDefinition、D5 island/world projection 由后续适配 | IslandSystem 写岛实例；Animal/FishSchool 由 EcologySystem 写 |
| `LandParcelEntity` | purpose、fertility、water source、lease、owner、risk | D7 无对应完整实体；不从 `estate_ids` 猜造 | PropertySystem 写；D8 game-only |
| `BuildingEntity` | function、level、ItemHolding inventory、worker IDs、damage、open state、ownership | D4 shipyard 只作为旧镜像，不转成建筑实例 | BuildingSystem 写；不把 Shipyard 升级当 Building |
| `ItemEntity` | category、tags、description、gameplay role、依赖/资产/来源/授权/status、单位重量/体积/耐久 | D3 commodity、cargo/personal/inventory/market 数量由后续适配保留 | ItemCatalog 写不可变定义；Inventory/Cargo/Economy 写持有量/市场状态 |
| `AnimalEntity` | species、年龄日、性别、变体、status、habitat、behavior、health、法律状态、生态压力 | D4 AnimalDefinition/AnimalState/EcologyState 作为 legacy projection | EcologySystem 写动物事实；模型不实现行为 |
| `FishSchoolEntity` | 鱼种、数量、深度、迁徙、捕捞压力、恢复 | D7 `fish_school` 只作为 marker，不生成数量 | EcologySystem 写；D8 game-only，D11 再扩展 |
| `PetEntity` | breed、health、hunger、cleanliness、attachment、location、care state | D7 无完整实体；Crew 只保留未来 pet 引用 | AnimalCare/Farming 写；D8 game-only |
| `FinanceAccountEntity` | 账户类型、所有者、本金、利息、担保、抵押、到期日、违约状态、账本引用 | D7 money 仅作为迁移输入，不伪造贷款/利息 | FinanceSystem 写；server wallet 不等于该实体 |
| `QuestEntity` | status、目标/前置/选择、失败、恢复路线、奖励请求、事实事件引用 | D4 mainline 的 chapter/mission/flags/completed 作为 legacy projection | QuestSystem 写任务状态；Event/Reward 系统只写各自事实 |
| `WorldEventEntity` | event ID、world day、继承的 location ref、actor IDs、input facts、result、cause IDs、outcome、recovery、save revision、去重 key | D3 event IDs/seeds、D4 route log 作为 legacy 来源 | EventLog 追加事件；各领域系统写自己的最终结果；D8-A03 负责统一契约 |
| `RewardEntity` | reward ID、来源、item/quantity/money、unique、claim record、status、reclaim/recovery | D5 achievement marker 只作为外部 legacy grant 标记 | RewardSystem 写发放/领取；Agent/Quest 不写最终奖励 |
| `AppearanceLoadoutEntity` | loadout ID、body/hair/clothing/accessories/equipment、durability、source | D5 preset、skinTone、hairStyle、hairColor、outfit、faceScale、bodyScale 由适配保留 | AppearanceSystem/Character 写；UI 只能提交选择请求 |

完整的唯一写者、读取者、存档字段、版本和测试入口继续以 D8-S01 冻结矩阵为准；本目录只说明 DTO 承载方式。

## 6. D7 兼容策略

1. 现有 `PlayerState`、`ShipState`、`CrewAgent` 和 `CrewAgentCodec` 公共字段没有删除或改名。
2. `LegacyD7PlayerFields`、`LegacyD7ShipFields`、`LegacyD7CrewFields` 只做 AgentCore 边界镜像，供 D8-A05 的 Systems 适配层使用。
3. D7 `money`、`hull`、Crew 健康/疲劳、traits 和 affinity 仍是旧字段；v3 typed 字段是 canonical 迁移目标，不在本任务进行冲突裁决。
4. D7 D3/D4/D5 快照不直接引用 Unity/Systems 类型；Player 兼容载体保存原始 JSON 边界，后续迁移器按各自版本解析。
5. `HullMax` 仍由 `ship_type_defs.hull_max` 回查，不在 `LegacyD7ShipFields` 伪造存档值。
6. 缺失新字段使用 DTO 默认值；缺失的历史事实不通过 UI、Agent 或展示字段猜测。

## 7. 序列化与未知枚举策略

`V3Json` 是唯一入口：

- 使用标准库 `System.Text.Json`，字段名按 DTO 的 `JsonPropertyName` 固定为 snake_case；枚举使用 camelCase 字符串；
- 默认允许忽略未知对象字段，以便旧存档向前读取；
- 所有枚举包含 `Unknown=0`，缺字段时保留 `Unknown`；
- 禁止未知枚举整数和未知枚举字符串静默转换，`TryDeserialize` 返回 `enum_or_value_rejected[:path]`；
- 序列化保留默认字段和空集合，避免空实体与完整实体产生不可解释的缺字段差异；
- `V3SaveMetadata` 独立保存 `save_version/world_revision/last_event_id/migration_flags`，不取得领域写权限。

## 8. 测试覆盖

`V3EntityModelTests.RunAll()` 已接入 AgentCore 自检，覆盖：

- 16 个实体的空构造、默认枚举、空集合和 D7 兼容载体；
- 16 个实体的完整代表对象序列化/反序列化往返；
- 稳定 ID 的重复生成、固定前缀、非法类型和非法前缀；
- 金额、重量、体积、世界日、时间、关系和百分比边界；
- D7 旧字段载入、缺少 v3 字段默认值、未知新增对象字段忽略；
- 未知枚举严格拒绝；
- 存档级目标版本和迁移 flags 默认值。

执行入口：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
@('exit') | dotnet run --project game/AgentCore --no-build -- --mock
```

后续 D8-A05 必须在不删除 D7 字段的前提下，将 Systems 存档映射到这些 DTO，并为迁移失败、重复迁移和部分损坏档案增加测试；不得把本目录的默认值当成已经完成的玩法结算。
