# D8-S01 任务交接报告：冻结 D8 数据边界

- 任务编号：`D8-S01`
- 前置任务：`D7 基线`
- 完成日期：`2026-08-09`
- 主成果：[`docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md`](../../D8_DATA_BOUNDARY_FREEZE.md)
- 交接状态：已完成文档冻结，未实现后续 D8-A 业务、DTO、迁移或内容功能。

## 1. 修改文件

本任务实际修改或新增：

- `docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md`：唯一主冻结表；由原带序号文件迁移为任务要求的固定文件名。
- `docs/game/design/common/handoffs/D8/D8-S01_HANDOFF.md`：本交接报告。
- `docs/game/design/common/README.md`：补充并修正冻结表入口。
- `docs/game/design/README.md`：补充并修正冻结表入口。
- `docs/game/design/common/02_DATA_GOVERNANCE.md`：明确 `created_day/updated_day` 是实体世界时间，`created_at/updated_at` 仅为存储审计元数据。
- `.workbuddy/memory/2026-08-09.md`：记录本任务、验证结果、风险和后续入口。

未修改 `contracts/`；未修改 `game/`、`server/`、`web/` 运行时代码或测试代码；未改变 AgentCore 公共类型。

## 2. 数据边界

冻结表为 D8-A01—D8-A06 的共享入口，完整矩阵包含以下 16 个实体：

`Player`、`Ship`、`Crew`、`Relation`、`Island`、`LandParcel`、`Building`、`Item`、`Animal`、`FishSchool`、`Pet`、`FinanceAccount`、`Quest`、`WorldEvent`、`Reward`、`AppearanceLoadout`。

每个实体均已在主冻结表中写明：

- canonical 唯一写者和字段分区；
- 允许读取者；
- D7 保留存档字段和 v3 存档字段；
- `schema_version`、`save_version`、`revision` 或事件去重责任；
- 当前 D7 测试入口和后续 D8/D11/D13/D14 测试入口；
- game 内部、contracts 投影、server/web 暂不进入的边界。

冻结规则如下：

1. `SaveSystem` 只负责序列化、落库、迁移和还原，不获得任何实体的数值裁决权。
2. canonical 实体统一使用 `schema_version=1` 作为 D8 初始实体版本；目标存档版本为 `save_version=3`；D7 SQLite 当前 `PRAGMA user_version=1` 是迁移输入。
3. 世界实例使用 `created_day`、`updated_day`、`revision`；`created_at`、`updated_at` 只能作为技术审计字段，不能形成第二套玩法时间。
4. Agent 只能通过 `AgentReply` 提议，UI 只能提交请求或读取快照；两者都不能成为实体唯一写者。
5. 完整本地存档保持 game-only。进入 `contracts/` 的只是跨端 DTO、校验镜像或提议/事实投影，不改变 game 的裁决权。

边界分组：

| 分组 | 实体 | D8 边界 |
|---|---|---|
| 需要现有 contracts 投影 | `Player`、`Ship`、`Crew`、`Relation`、`Island`、`Item`、`Animal`、`Quest`、`AppearanceLoadout` | 仅沿用或投影现有 D3/D4/D5/AgentReply 字段；完整 v3 实例和本地数值存档不进入 server/web。 |
| D8 后续需要 contracts 草案 | `WorldEvent` | D8-A03 负责统一事实事件 Schema；在 Schema 和三端同步前不新增 active endpoint。 |
| 暂不进入 server/web | `LandParcel`、`Building`、`FishSchool`、`Pet`、`FinanceAccount`、`Reward` | D8 只建立 game 内部 canonical 模型、默认值和存档责任；后续跨端需求必须先走契约评审。 |

现有 server 的 wallet/账号余额不等于游戏 `FinanceAccount`；现有 OpenAPI 的贸易、生态、世界和外观端点是校验/回显镜像，不是本地游戏事实的替代写者。当前仓库没有独立 `web/` 实现，故不把 web 设为任何实体的写者或权威读写库。

## 3. D7 / v3 差异

### D7 保留

- Player：`Id`、`Name`、`Money`、`Skills`、`Traits`、`Reputation`、`Inventory`、`ShipId`、`Location`、`PlaySeconds`，以及 D3/D4/D5 运行时快照引用。
- Ship：`Id`、`ShipTypeId`、`Name`、`Hull`、`Cargo`、`Cannons`、`LoadedCount`、`Supplies`、`CrewCapacity`、`CrewIds`、`Upgrades`、`State`、`ShipLog`；`HullMax` 继续由船型定义回查，不新增为存档事实。
- Crew：`crew_agents` 的身份、persona、stats、记忆/关系 JSON 占位、装备/特质 ID 等既有列及 JSON；D4 health 和 D5 trait ID 快照继续可读。
- Relation：D7 `CrewStats.Affinity` 标量和空的 `relations_json` 占位，不把空 JSON 误当完整关系图。
- Island/Item/Animal/Quest/AppearanceLoadout：D3/D4/D5 已有的港口/岛屿、commodity/cargo、AnimalState、主线、角色预设和外观字段均保留为兼容投影。
- WorldEvent：D3 event ID/seed 和 D4 route log 作为 legacy 事件来源；D5 世界发现/访问状态作为 legacy projection。
- D3/D4/D5 运行时快照原有字段全部保留，包括贸易报价/订单/结算、航行天气与事件、派系、战斗、生态、健康、船坞、主线、路线日志、世界、角色、性能、Steam、语言和 trait ID。

### v3 新增

- 通用字段：`id`、`schema_version`、`display_name_key`、`source_note`、`historical_tag`、`phase`、`owner_system`、`test_id`。
- 实例字段：`created_day`、`updated_day`、`location_ref`、`state_flags`、`revision`。
- Player：`pronouns`、`identity_history`、`appearance_loadout_id`、`credit` 和更明确的资产/关系/任务/收藏引用集合。
- Ship：分区损伤、质量/容量、舱室和功能点、锚泊、维修、外观、ownership 等实例字段。
- Crew：身份、身体状态、关系边、目标/承诺/债务、任务/轮班、外观/装备/宠物/个人库存引用。
- Relation：`source_id`、`target_id`、`relation_type`、`value`、`confidence`、`cause_event_ids`、`last_changed_day`、`locked_by_story`、`visibility`。
- Island：自然档案、控制者、人口、生态指标、资产引用和实例 revision。
- `LandParcel`、`Building`、`FishSchool`、`Pet`、`FinanceAccount`、`Reward`：D7 无对应完整实体，v3 先冻结 ID、字段形状、默认值、版本和 game-only 边界。
- Item：内容定义字段、分类、标签、玩法角色、依赖/资产/来源/授权/status；持有量、重量、体积、耐久按持有者所属系统存档。
- Animal：独立物种实例、年龄/性别/变体、状态、栖息地、行为、法律和生态 revision。
- Quest：状态、目标、前置、选择、失败/恢复路线、奖励请求、事实事件引用。
- WorldEvent：统一事件包的 `event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`outcome_status`、`recovery_route`、`save_revision` 和去重标记。
- AppearanceLoadout：`loadout_id`、`body`、`hair`、`clothing`、`accessories`、`equipment`、`durability`、`source`，并保留 D5 预设映射。

字段差异的完整逐实体表和 D7 快照字段索引以主冻结表为准；后续任务不得以本报告的摘要表替代主表。

## 4. 冲突与冻结处理

- `money` 同时存在于 PlayerState、D3 快照和 D3 trade 状态：D7 字段不删；D8 认定 FinanceSystem 为余额事实写者，D3 只保留兼容镜像。
- `hull` 同时存在于 ShipState、D3 快照和 D4 combat：ShipSystem 是船体事实写者，D4 是战斗投影；迁移不得三处互写。
- Crew 健康/疲劳同时存在于 CrewStats 和 D4 HealthState：CrewSystem 为 canonical，D4 为旧投影；冲突要按事件/时间处理并显式记录，不能静默覆盖。
- AgentCore persona 文本、Save trait IDs、D5 trait IDs 语义不同：ID 是事实，persona 是投影；D8-A01 只建单向适配。
- `relations_json` 为空：只迁移可证明的 affinity，并标记 legacy；关系图由事实事件产生。
- `ShipState.CreateNew` 接收 `ownerId`，但 D7 ShipState/ships 表没有 `owner_id`：不得伪造旧字段；迁移只能以 `Player.ship_id` 推断当前活动船，并标记 `inferred`。
- DATA_MODELS 对 Island/Ecology 的写者表述与单一写者原则有交叉：IslandSystem 只写 IslandInstance，EcologySystem 只写 Animal/FishSchool/Vegetation，通过事实事件提供生态汇总。
- D4 的 `fish_school` 是 AnimalId marker，不是 FishSchool 实体；不扩张成 server API。
- D5 `shipyard` 是船坞/升级 legacy projection，不是 `BuildingInstance`。
- 新增 `WorldEvent` 跨端需求尚无 active Schema：D8-A03 先建 Schema 和同步边界，再决定 server/web 的只读投影；S01 不擅自新增契约。

## 5. 验收结论

- [x] 16 个核心实体均有 canonical 名称、唯一写者、读取者、存档字段、版本责任和测试入口。
- [x] D7 字段与 v3 新字段已分开记录，且 D7 字段删除禁令明确。
- [x] contracts、game、server、web 的投影/权威边界已明确；本任务未产生未同步契约改动。
- [x] D8-A01—D8-A06 可以从主冻结表定位各自实体、写者和数据边界。
- [x] 未修改领域业务逻辑、AgentCore 公共类型或 Systems 网络/LLM 约束。
- [x] 已记录跨模块冲突和责任任务，未替后续系统擅自决策。
- [ ] D8-A01—D8-A06 的实现、三端同步、迁移器、事件包和配置工作尚未开始，不在 S01 验收范围内。

验证证据：

- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：通过，0 警告、0 错误。
- `npm run build`（工作目录 `server/`）：通过。
- `dotnet test game/Save.Tests.csproj --no-restore -v:minimal`：返回成功；当前工程未输出测试摘要，后续 D8-S02 需补充可见的测试结果汇总。
- `git diff --check`：通过。
- 已抽查 D7 `PlayerState`、`ShipState`、`GameState`、`SaveSystem`、D3/D4/D5 contracts、OpenAPI 路由和 Save/D3/D4/D5/D7/AgentCore 测试入口。

## 6. D8-A01—D8-A06 注意事项

| 任务 | 交接注意事项 |
|---|---|
| D8-A01 | 先读主冻结表；建立纯数据模型、稳定 ID、默认值和序列化测试；保留 D7 Player/Ship/Crew 字段，不改 AgentCore 公共类型；处理重复镜像和未知字段策略。 |
| D8-A02 | ID 注册、来源、授权、owner、phase、test_id 只能做代表样例；不得批量伪造内容；遵守本表的实体/定义与实例分离。 |
| D8-A03 | `WorldEvent` 追加事实，不直接裁决金钱、价格、伤害、天气、关系或奖励；事件要保存 `cause_ids`、`owner_system`、`save_revision`，并保证重复结算保护。 |
| D8-A04 | 先修改 contracts 再同步 game/server（必要时 web）；AgentReply 只能表达意图，禁止最终 money、price、damage、reward、weather、relation 数值；关系结果仍由 CrewSystem 裁决。 |
| D8-A05 | 必须等待 D8-A01；把 D7 `user_version=1` 迁移到 `save_version=3`，保留所有旧字段；迁移失败不得覆盖原档，重复迁移不得重复发奖或改变世界状态。 |
| D8-A06 | 只统一类型、单位、枚举和配置入口，不调整经济/战斗平衡；配置不能成为跨系统的第二个写者。 |

## 7. 是否允许开始异步任务

允许开始：`D8-A01`、`D8-A02`、`D8-A03`、`D8-A04`、`D8-A06`。这些任务必须先读取 `docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md`，并仅在各自责任范围内修改。

暂不允许开始：`D8-A05` 需等待 `D8-A01` 完成并交接实体 DTO/兼容字段；`D8-S02` 需等待全部 D8-A01—D8-A06 完成。

异步任务启动条件不是对后续实现的完成声明；任何跨端新增字段仍必须先补 contracts 并同步三端。
