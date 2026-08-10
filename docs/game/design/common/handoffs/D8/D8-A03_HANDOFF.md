# D8-A03 任务交接报告

状态：实现完成，待 D8-S02 合并门接入。

## 交付内容

- 新增 `Whalebone.Systems.Events.WorldEvent`、`FactEvent`、`EventResult`、`RecoveryRoute` 等事件 DTO；
- 新增 `EventLog` 事实日志、因果追踪、Quest/UI/Agent 读取投影、事件快照恢复和结算幂等门；
- AgentCore 新增 `AgentMemory.RecordFactSummary`，只接收事件摘要，不获得事件写权限；
- 新增 Unity EditMode 测试 `D8EventPackageTests`；
- 新增跨端字段契约 `contracts/world-event.schema.json`；
- 新增事件包说明 `docs/game/design/common/D8_EVENT_PACKAGE.md`。

## 事件字段

必填字段为：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`save_revision`。D8 同时记录 `outcome_status`、失败/中断必需的 `recovery_route`，以及由 EventLog 生成的 `log_sequence`。

`result` 是 owner 已裁决结果的 opaque 事实载荷：`result_code`、`summary`、`fact_ids` 和字符串 `values`。事件层不解释、不计算、不写入其中的领域数值。

## 所有权

| 数据 | 唯一写者 | 本任务行为 |
|---|---|---|
| WorldEvent/FactEvent 事实日志 | EventSystem/EventLog | 只追加、校验、生成日志序号 |
| 金钱、价格、伤害、天气、关系、奖励、库存 | 各自 canonical owner | EventLog 不接触，不裁决 |
| Quest/UI/Agent 视图 | 各读取方 | 只取事实副本/摘要 |
| 实际奖励发放 | RewardSystem/Economy/Inventory 等 owner | 先调用 `ClaimSettlement`，仅首次声明可执行 |
| 事件快照落盘封套 | SaveSystem（D8-A05 接入） | D8-A03 提供 `EventLogSnapshot`，不改 SaveSystem 数值逻辑 |

## 状态、因果和恢复

- 支持 `Normal`、`Failed`、`Cancelled`、`Interrupted`、`Recovered`；
- `Failed` / `Interrupted` 没有 `recovery_route` 会明确拒绝；
- `WorldEvent.CreateRecovery` 和 `EventLog.AppendRecovery` 只允许从中断事实创建恢复事实；
- `TraceCauses` / `GetCauseChain` 提供直接及间接因果链；
- 同一 `event_id + save_revision` 只保留一条事实。

## 重复结算保护

`EventLog.ClaimSettlement(event_id, save_revision)` 使用事件日志快照中的 `SettledKeys`。首次返回 `Claimed`，读档后再次调用返回 `AlreadyClaimed`；事件层不执行奖励发放，因此不会绕过 RewardSystem 的数据所有权。

## 测试命令与结果

本次实际运行结果：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal  # PASS：0 warning / 0 error
dotnet build game/Whalebone.Systems.csproj -v:minimal              # BLOCKED：已有 Agent.Runtime/Game 命名空间与 D8-A04 类型错误
npm run build  (workdir=server)                                    # PASS
git diff --check                                                    # PASS：exit 0，只有既有换行符提示
ConvertFrom-Json contracts/world-event.schema.json                 # PASS
```

Unity EditMode 入口：`Save.Tests.D8EventPackageTests`。在 NUnit 兼容的源代码验证运行器中 7/7 通过，覆盖字段、失败恢复路由、取消、中断恢复、因果链、版本错误、读档后重复追加/重复结算和不同 `save_revision` 的区分。Unity EditMode 过滤命令未能启动，因为当前仓库已有 Unity 实例占用项目；未强制关闭用户进程。

## 已知边界与后续

- D8-A05 负责把 `EventLogSnapshot` 放入 v3 save envelope，并与 D7 SQLite 迁移联调；
- D8-A04 负责 AgentReply/P2A 契约，必须继续禁止最终 money/price/damage/reward/weather/relation 结果；
- D3/D4 旧事件与文本日志仍是兼容投影，未在本任务批量迁移成完整因果图；
- 当前跨端 schema 仅建立字段事实源，没有 active server/web endpoint；D8-S02 负责检查三端漂移。
- Systems/Save.Tests 的完整 Unity 生成工程验证需先解决仓库已有的 `Whalebone.Game`、`ActionValidationContext` 等未合并类型错误；本任务未越界修改 Agent/Gameplay/Save 模块。
