# D8-A05 交接报告：D7 → v3 存档迁移

- 任务编号：D8-A05
- 日期：2026-08-10
- 前置：D8-S01、D8-A01
- 状态：实现完成；仓库整体 dotnet 编译受并行 AgentCore 基线错误阻断，Unity Edit Mode 测试需在基线修复后执行

## 修改文件

- `game/Assets/Scripts/Systems/Save/SaveMigration.cs`：新增 D7 → save_version=3 迁移器、`migration_flags` 元数据、v3 实体投影、默认值、错误码、事务回滚和幂等读取。
- `game/Assets/Scripts/Systems/Save/SaveMigration.cs.meta`：Unity 脚本资产元数据。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：新增 `SaveSystem.MigrateToV3()` 公共入口；未改变既有 HasSave/NewGame/Save/Load 签名和 D7 表写入语义。
- `game/Assets/Tests/EditMode/SaveSystemTests.cs`：新增空档、D7、航行中、损伤船、负债、半途任务、重复迁移、中断回滚、未知版本和部分损坏测试。
- `docs/game/design/common/D8_SAVE_MIGRATION.md`：迁移规范、字段表、失败保护、测试矩阵和跨端边界。
- `docs/game/design/common/README.md`：增加迁移规范入口。
- `docs/game/design/common/handoffs/D8/D8-A05_HANDOFF.md`：本报告。
- `.workbuddy/memory/2026-08-10.md`：追加本任务记忆日志。

## 支持的旧版本

仅支持 D7 SQLite `PRAGMA user_version=1`。空档由迁移器返回 `Empty`；`user_version=0`、大于 1 或其他未知值返回 `UnsupportedVersion`，不写入迁移表。目标版本为 `save_metadata.save_version=3`；SQLite `user_version` 保持 1，以维持 D7 读取兼容。

## 字段迁移摘要

| D7 来源 | v3 结果 | 处理 |
|---|---|---|
| Player `id/name/money/skills/traits/reputation/inventory/ship_id/location/play_seconds` | Player `id/display_name_key/balance/legacy_d7` | 原字段语义保留；JSON 坏字段默认空对象/数组 |
| Ship `id/ship_type_id/name/hull/cargo/cannons/loaded_count/supplies/crew/upgrades/state/ship_log` | Ship `id/ship_type_id/legacy_d7/crew_ids` | 原列不删除；`hull_integrity` 由 `hull_max` 推导，不回写 D7 |
| Crew `id/name/role/culture/traits/stats/persona/memory/relations` | Crew `id/role_id/culture_id/trait_ids/legacy_d7` | health/fatigue 缺失时默认 100/0 |
| D7 没有的 Finance/Quest/World/Reward/Appearance 等实体 | 不插入实体行 | 使用安全默认值和 `migration_flags`，不伪造玩法内容 |

## 数据边界与所有权

`SaveMigration` 是本地存档迁移唯一写者；读取者是 `SaveSystem` 和后续 game Save/Storage 适配层。`save_metadata` 和 `v3_entities` 属于 game 内部存档，不进入 `contracts/`、server 或 web。迁移不改变 AgentCore 公共类型，不把 Unity 运行时对象写入实体，不把 UI/Agent 设为写者。

## 失败保护

- 所有 v3 写入在 `BEGIN IMMEDIATE TRANSACTION` 内完成；提交前异常执行 `ROLLBACK`。
- D7 三张原表从不删除、不覆盖；部分 JSON 恢复只影响 v3 投影并记录标志。
- 必需 Player/Ship 标识缺失时拒绝迁移。
- 未知版本、迁移半成品和元数据缺失不会被自动猜测续写。
- 完整 save_version=3 再次执行只返回 `AlreadyMigrated`，不会重复发奖、追加事件或推进世界。

## 冲突与并行注意

- 仓库中并行 AgentCore 改动当前使完整 `Save.Tests` 项目构建提前失败：缺少 `Whalebone.Game` 与 `ActionValidationContext` 类型；本任务未修改这些文件。
- 当前工作区的 D8-A06 数值配置改动已让迁移器中的 Crew 默认值/夹值引用 `NumericConfiguration`。该引用未被本任务移除；D8-A06 必须保证其类型继续位于可被 Systems 引用的程序集，或由双方确认后再调整，不要让存档迁移偷偷拥有数值裁决权。

## 测试命令与结果

执行：

```text
dotnet build game/Save.Tests.csproj --no-restore
```

结果：未通过，失败发生在工作区已有的 AgentCore 并行改动：`MutinyMonitor.cs` / `AgentModels.cs` 缺少 `Whalebone.Game` 类型，`AgentReplyValidator.cs` 缺少 `ActionValidationContext`。构建输出未报告本次迁移文件的编译错误，但因项目引用链提前失败，不能宣称全套测试通过。

独立 SQLite 烟测：编译 `SaveMigration`、`SaveJson`、`SqliteDb`、`SqliteNative` 和当前数值配置后，D7 样本完成首次迁移、元数据写入和重复迁移保护，输出 `D8A05_HARNESS_OK`。

需在 Unity Edit Mode Test Runner 执行 `D8A05_*` 测试；当前环境未完成该轮测试，不能宣称 Unity 测试通过。

## 未完成事项

- 需要在并行 AgentCore 基线恢复后重新生成 Unity 项目并执行完整 Save.Tests。
- 若未来要让 server/web 读取 v3 存档，必须另开 contracts 任务；本任务明确不创建跨端契约。
- world_revision 在 D7 没有来源，当前安全值为 0；D8 世界事件任务需在自己的写者边界内定义后续增量语义。

## D8-A05 验收结论

迁移代码、存档模型扩展、测试用例和设计文档已保存；D7 字段不删除，失败不覆盖原档，重复迁移受保护，未知版本和部分损坏有明确策略。最终自动化验收受并行 AgentCore 编译阻断，待基线恢复后复跑。
