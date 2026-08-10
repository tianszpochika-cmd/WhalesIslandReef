# D8-A01 任务交接报告：核心实体与 DTO

- 任务编号：`D8-A01`
- 前置任务：`D8-S01`（已完成）
- 完成日期：`2026-08-10`
- 主成果：`game/AgentCore/V3EntityModels.cs`
- 模型说明：`docs/game/design/common/D8_ENTITY_DTO_CATALOG.md`

## 1. 修改文件

新增或修改：

- `game/AgentCore/V3EntityModels.cs`：新增 `Whalebone.Agent.V3` 纯 C# v3 数据模型、稳定 ID、明确单位类型、D7 兼容载体、存档元数据和 `V3Json` 序列化入口。
- `game/AgentCore/V3EntityModelTests.cs`：新增 AgentCore 自测，覆盖 16 个实体、空/完整实体、旧字段、未知枚举、稳定 ID 和类型边界。
- `game/AgentCore/AgentCoreTests.cs`：在工作区已有测试改动基础上接入 `V3EntityModelTests.RunAll()`；未改变既有 AgentCore 公共类型的字段、方法或行为。
- `docs/game/design/common/D8_ENTITY_DTO_CATALOG.md`：新增模型说明、ID 目录、字段边界、D7 兼容和序列化规则。
- `docs/game/design/common/README.md`：新增模型目录入口。
- `.workbuddy/memory/2026-08-09.md`：追加本任务记忆日志。

未修改：`contracts/`、`server/`、`web/`、`game/Assets/Scripts/Systems/` 业务实现、`PlayerState`、`ShipState`、`CrewAgent` 既有公共类型和 D7 存档字段。

工作区说明：任务执行期间已存在其他工作区改动，包括 `AgentCoreTests.cs` 中的事实摘要测试和 `contracts/agent-reply.schema.json`/其 fixtures；本任务保留这些改动，未覆盖或回退。

## 2. 公共类型

新增公共类型均位于 `Whalebone.Agent.V3`：

- 实体：`PlayerEntity`、`ShipEntity`、`CrewEntity`、`RelationEntity`、`IslandEntity`、`LandParcelEntity`、`BuildingEntity`、`ItemEntity`、`AnimalEntity`、`FishSchoolEntity`、`PetEntity`、`FinanceAccountEntity`、`QuestEntity`、`WorldEventEntity`、`RewardEntity`、`AppearanceLoadoutEntity`；
- 共享模型：`V3EntityBase`、`V3SaveMetadata`、`LocationRefV3`、`ItemHolding`；
- 版本与 ID：`V3ModelVersions`、`V3EntityKind`、`V3StableIds`；
- 明确类型：`MoneyAmount`、`WeightAmount`、`VolumeAmount`、`WorldDay`、`TimeOfDay`、`RelationValue`、`PercentValue`；
- 状态枚举：实体状态、关系、建筑、物品、动物、金融、任务、事件、奖励、锚泊和维修状态枚举；未知值统一包含 `Unknown=0`。

既有 `Whalebone.Agent` 类型（包括 `CrewAgent`、`CrewStats`、`PersonaDefinition`、`AgentReply`）未改公共字段或签名。D7 兼容载体为 `LegacyD7PlayerFields`、`LegacyD7ShipFields`、`LegacyD7CrewFields`，后续由 Systems 适配层使用。

## 3. 数据所有权

DTO 不拥有玩法裁决权。唯一写者继续遵守 D8-S01：

- Player：Player/Core 聚合，Economy/Finance/Progression/Property/Inventory 等只写自己的字段；
- Ship：ShipSystem；货物、费用、外观等系统只写各自分区；
- Crew/Relation：CrewSystem；AgentCore 仅生成提议；
- Island：IslandSystem；Animal/FishSchool 由 EcologySystem 单独负责；
- LandParcel：PropertySystem；Building：BuildingSystem；
- Item：ItemCatalog 写定义，Inventory/Cargo/Economy 写持有量或市场状态；
- Animal/FishSchool：EcologySystem；Pet：AnimalCare/Farming；
- FinanceAccount：FinanceSystem；Quest：QuestSystem；
- WorldEvent：EventLog 追加事实，各领域系统写最终结果；
- Reward：RewardSystem；AppearanceLoadout：AppearanceSystem/Character；
- SaveSystem 只负责序列化、迁移、落库和还原，UI/Agent 不能成为唯一写者。

读取者、存档字段和跨端边界不在本报告重新定义，以 `D8_DATA_BOUNDARY_FREEZE.md` 为唯一依据。

## 4. 版本、默认值和兼容策略

- 实体 `schema_version=1`；目标存档 `save_version=3`；存档级字段位于 `V3SaveMetadata`。
- 空实体通过无参构造得到安全默认值：空字符串、空集合、`Unknown` 枚举和零值/满值百分比。
- 缺少 v3 字段时使用模型默认值；不通过显示名、UI 或 Agent 猜测历史事实。
- `V3StableIds` 使用固定前缀和规范化 slug，不使用随机数、时间或全局 ID 注册状态。
- `V3Json` 使用标准库 `System.Text.Json`；旧对象中的未知字段可忽略，未知枚举字符串/整数严格拒绝并返回 `enum_or_value_rejected`。
- D7 `money`、`hull`、Crew JSON、D3/D4/D5 快照仍保留在 `LegacyD7*Fields` 或原 Systems 类型中；本任务没有删除、重命名或覆盖它们。

## 5. 测试命令与结果

已执行：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
```

结果：通过，0 警告、0 错误。

```text
@('exit') | dotnet run --project game/AgentCore --no-build -- --mock
```

结果：通过。既有 AgentCore/D2 自测和新增 D8-A01 自测全部显示 `[OK]`，新增覆盖包括：

- 16 个实体空构造和默认值；
- 16 个实体完整代表对象序列化/反序列化往返；
- 固定前缀和确定性稳定 ID；
- 金额、重量、体积、世界日、时间、关系值和百分比范围；
- D7 旧字段载入、缺省 v3 字段和未知对象字段；
- 未知枚举严格拒绝；
- `save_version=3` 和迁移 flags 默认值。

本任务未引入 contracts 变更，因此没有运行三端契约同步测试；后续 D8-S02 仍需验证契约漂移。

## 6. 未完成事项

- Systems `PlayerState`/`ShipState`/`CrewAgent` 到 v3 DTO 的实际适配和数据库迁移未实现，留给 D8-A05。
- D8-A01 不实现玩法业务、事件结算、经济/金融、建筑经营、宠物照料、生态恢复或奖励发放。
- `WorldEvent` 的 active contracts Schema 和 server/web 同步未实现，留给 D8-A03。
- 内容 ID 目录、批量内容和来源授权检查留给 D8-A02。
- 数值统一配置留给 D8-A06；本任务没有改变任何平衡参数。

## 7. D8-A05 存档迁移注意事项

1. 从 D7 SQLite `user_version=1` 迁移到目标 `save_version=3` 时，必须保留所有原始 Player、Ship、CrewAgent 字段和 JSON 键。
2. 使用 `LegacyD7PlayerFields`、`LegacyD7ShipFields`、`LegacyD7CrewFields` 做适配边界；不要把 AgentCore DTO 直接变成 SaveSystem 的第二个写者。
3. `money` 与 v3 `balance`、`hull` 与 v3 `hull_integrity`、Crew 健康/疲劳、traits 和 affinity 的重复镜像必须记录来源；不能静默覆盖。
4. `HullMax` 仍从船型定义回查；D7 `ShipState.CreateNew(ownerId)` 的 owner 缺失不能伪造为旧存档字段。
5. D3/D4/D5 快照先按各自版本解析；缺少 v3 字段使用安全默认值，缺失历史事实不得猜测。
6. 迁移失败不得覆盖原档；重复迁移不得重复发奖、重复追加事件或改变世界状态；保存后写入 `save_version/world_revision/last_event_id/migration_flags`。
7. 未知枚举保持拒绝并返回可理解错误；未知对象字段可保留兼容读取策略，但不得把未知字段当成已实现玩法。

## 8. 交接结论

D8-A01 的实体 DTO、稳定 ID、版本字段、默认值、D7 兼容载体和序列化测试已完成，允许 D8-A05 在本任务交接后继续等待依赖条件执行；D8-A05 仍必须遵守 D8-S01 的单一写者和失败保护规则。
