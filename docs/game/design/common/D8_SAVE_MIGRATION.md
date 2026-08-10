# D8 存档迁移规范：D7 SQLite → save_version=3

状态：D8-A05 冻结稿  
日期：2026-08-10  
唯一实现：`game/Assets/Scripts/Systems/Save/SaveMigration.cs`  
入口：`Whalebone.Systems.Save.SaveSystem.MigrateToV3()`

## 1. 目标与不变项

D7 的 SQLite 存档以 `PRAGMA user_version=1` 标识。D8-A05 将它投影到 D8 v3 统一实体模型，但不删除、不重命名、不重写 D7 的 `players`、`ships`、`crew_agents` 字段。v3 目标版本写在新增的 `save_metadata.save_version=3`，不是把 SQLite 的 `user_version` 改成 3。

迁移只做结构和字段投影，不裁决玩法结果，不发放奖励，不推进任务，不改变经济、关系或世界状态。新实体缺少 D7 来源时，以安全默认值进入 v3 投影，并记录 `migration_flags`。

支持范围：

- 来源：`user_version=1` 且包含 D7 `players`、`ships`、`crew_agents` 表；
- 目标：`save_version=3`；
- 空档：无数据库、空文件、无 D7 表或无玩家行，返回 `Empty`，不创建文件；
- `user_version=0`、大于 1 或其他未知值：返回 `UnsupportedVersion`，不写入迁移表；
- 已有完整 `save_metadata`、`v3_entities` 且 `save_version=3`：返回 `AlreadyMigrated`，不重复投影。

## 2. v3 存储结构

迁移在同一 SQLite 事务中创建两张附加表：

| 表 | 用途 | 写者 | 关键字段 |
|---|---|---|---|
| `save_metadata` | 迁移状态与存档头 | SaveMigration | `save_version`、`world_revision`、`last_event_id`、`migration_flags`、`migration_status`、`source_user_version` |
| `v3_entities` | v3 实体的稳定 ID 与 JSON 投影 | SaveMigration | `entity_type`、`entity_id`、`schema_version`、`save_version`、`payload_json` |

写入后，`save_metadata` 至少包含：

```text
save_version       = 3
world_revision     = 0（D7 没有可迁移的统一世界修订号）
last_event_id      = D3 eventIds 最后一个，或 D4 routeLog 最后一个 id；无来源时为空
migration_flags    = JSON 字符串数组
migration_status   = migrated
source_user_version= 1
```

`world_revision`、`last_event_id` 和 `migration_flags` 都由 SaveMigration 写入；UI、Agent 和玩法系统不能直接改迁移结果。

## 3. D7 → v3 字段迁移表

### Player

| D7 字段 | v3 位置 | 规则 |
|---|---|---|
| `id` | `id`、`legacy_d7.id` | 原值保留，作为稳定 ID |
| `name` | `display_name_key`、`legacy_d7.name` | 原语义保留 |
| `money` | `balance.value`、`legacy_d7.money` | 允许负数；不把负债改成奖励或清零 |
| `skills_json` | `legacy_d7.skills` | 合法对象投影；损坏时 v3 使用空对象并记 `partial_recovery`，D7 原字符串不动 |
| `traits_json` | `legacy_d7.traits` | 合法数组投影；损坏时空数组 |
| `reputation_json` | `legacy_d7.reputation` | 合法对象投影；不在迁移中重新计算 |
| `inventory_json` | `legacy_d7.inventory` | 合法对象投影；损坏时空对象 |
| `ship_id` | `ship_ids[0]`、`legacy_d7.ship_id` | 原值保留，Ship 的 `ownership_ref` 仅作为迁移推断 |
| `location_json` | `legacy_d7.location`、`location_ref` | 原对象保留；`dayIndex` 进入 `created_day/updated_day` |
| `play_seconds` | `legacy_d7.play_seconds` | 原值保留 |
| `location_json.d3/d4/d5` | `legacy_d7.d3_runtime_json` 等 | 保留原运行时快照文本，迁移不解释玩法 |

### Ship

| D7 字段 | v3 位置 | 规则 |
|---|---|---|
| `id`、`ship_type_id`、`name` | 对应 v3 字段及 `legacy_d7` | 原值保留 |
| `hull` | `legacy_d7.hull` | 原值保留 |
| `ship_type_defs.hull_max` | 仅用于 `hull_integrity.value` | 不属于 D7 Ship 存档字段；按 `hull / hull_max × 100` 推导并记标志 |
| `cargo_json`、`supplies_json`、`upgrades_json`、`state_json`、`ship_log_json` | `legacy_d7` | 合法 JSON 投影；坏字段采用空对象/数组 |
| `cannons`、`loaded_count`、`crew_json` | `legacy_d7` 及 v3 `crew_ids` | 原值/引用保留 |
| 缺少的 `mass`、`cargo_capacity`、`cabin_ids` 等 | v3 新字段 | 使用零值或空集合；不猜测玩法数值 |

### Crew

| D7 字段 | v3 位置 | 规则 |
|---|---|---|
| `id`、`name`、`role`、`culture` | 对应 v3 字段及 `legacy_d7` | 原值保留 |
| `traits_json` | `trait_ids`、`legacy_d7.traits_json` | 合法数组投影 |
| `stats_json.health/fatigue` | `health.value`、`fatigue.value` | 字段存在时沿用；缺失时默认 100/0 |
| `stats_json`、`persona_json`、`memory_json`、`relations_json` | `legacy_d7` | 原字符串保留，不在迁移中展开成关系或记忆玩法 |
| 缺少的关系、目标、任务、班次、债务 | v3 新字段 | 空集合/零值/`unknown`，记录默认标志 |

其余 D8 实体不由 D7 行直接产生。D8-A05 不伪造内容数据：`Relation`、`Island`、`LandParcel`、`Building`、`Item`、`Animal`、`FishSchool`、`Pet`、`FinanceAccount`、`Quest`、`WorldEvent`、`Reward`、`AppearanceLoadout` 均不插入 `v3_entities`；Player 的 `quest_ids`、财务 `credit` 等新字段只放安全默认值。

## 4. 默认值和部分损坏策略

迁移区分“必需字段”和“可恢复字段”：

- 必需：Player `id`、`ship_id`，以及被指向的 Ship 行。缺失时失败，不产生 v3 提交；
- 可恢复：D7 JSON 字段、Crew 的可选 JSON、D7 没有来源的 v3 字段。解析失败时使用空对象、空数组、0 或 `unknown`，并写入如 `partial_recovery:players.inventory_json` 的标志；
- D7 原始列永远不因恢复而被覆盖，故后续可以依据原字符串进行人工修复或重新迁移；
- 未知枚举、未知 D7 运行时内容不在迁移器中重新解释，按原始 JSON 保存并采用 `unknown` 状态。

通用迁移标志包括：

```text
d7_source_user_version_1
new_fields_defaulted
world_revision_defaulted
last_event_id_defaulted
finance_account_defaulted
quest_defaulted
world_entities_defaulted
rewards_defaulted
appearance_loadout_defaulted
hull_integrity_derived_from_d7_hull
ownership_ref_inferred_from_player_ship_id
```

## 5. 中断、失败、重复和恢复

迁移流程为：读取与校验 → `BEGIN IMMEDIATE TRANSACTION` → 写 v3 表 → 写 `save_metadata` → `COMMIT`。任何提交前异常都执行 `ROLLBACK`，所以原 D7 表和原始字段不会被覆盖，半成品 v3 表也不会留下可被误认为完成的元数据。

- 中断：返回 `Failed`、`migration_interrupted`，消息说明原档未覆盖；
- 写入失败：返回 `Failed`、`migration_write_failed`，回滚并保留原档；
- 缺少必需字段：返回 `Failed`、`source_required_field_missing` 或 `source_ship_missing`；
- 未知版本：返回 `UnsupportedVersion`，不创建迁移表；
- 迁移状态半成品：只有 `v3_entities` 没有 `save_metadata`，或元数据没有主记录/实体行，返回失败，不擅自续写；
- 重复执行：读取完整 `save_metadata` 后返回 `AlreadyMigrated`，不重复创建实体、不发奖励、不推进世界、不追加事件；
- 恢复：调用方保留原 `save.db`，根据 `ErrorCode` 修复或重新复制后重试。迁移器不删除原文件，也不自动改名覆盖备份。

## 6. 测试矩阵

`game/Assets/Tests/EditMode/SaveSystemTests.cs` 覆盖：

| 存档类型 | 验证 |
|---|---|
| 空档 | 返回 `Empty`，不创建数据库 |
| D7 基础档 | 三核心实体、元数据、D7 字段仍在 |
| 航行中档 | open_sea 位置、D3 event id、D7 快照保留 |
| 损伤船档 | hull 原值与推导后的 integrity 同时存在 |
| 负债档 | 负余额保留，FinanceAccount 新字段安全默认 |
| 半途任务档 | D4 快照保留，Quest 不伪造，quest_ids 为空 |
| 重复迁移 | 返回 `AlreadyMigrated`，实体计数和 payload 不变 |
| 中断迁移 | 回滚、原 D7 可加载、无 `save_metadata` |
| 未知版本 | 拒绝写入，原 D7 可加载 |
| 部分损坏 | 坏 JSON 采用默认值，D7 原字符串保留 |

建议命令：

```text
dotnet build game/Save.Tests.csproj --no-restore
```

Unity Edit Mode Test Runner 应执行 `Save.Tests` 程序集中的 `D8A05_*` 测试。若仓库中其他程序集编译失败，应在交接中区分基线阻断与迁移测试结果。

## 7. 跨端边界

本迁移格式是 game 本地存档格式，不是跨端契约。`save_metadata` 和 `v3_entities` 不进入 `contracts/`、server 或 web；如果未来需要云存档或服务端读取，必须另开契约任务，按 D8 数据边界冻结表建立版本化 DTO，不直接暴露 SQLite 表。
