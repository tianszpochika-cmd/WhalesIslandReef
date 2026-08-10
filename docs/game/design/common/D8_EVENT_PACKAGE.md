# D8-A03 统一事件包

状态：已实现 D8 `schema_version=1` 的本地事实事件层；跨端仅登记契约，不启用服务端世界裁决。

## 1. 目标与边界

`WorldEvent` 是领域系统准备提交的事件包，`FactEvent` 是通过 `EventLog.Append` 后进入事实日志的只读副本。事件层只有一个事实写者：`Whalebone.Systems.Events.EventLog`。`owner_system` 只表示哪个 canonical owner 已经裁决了结果，不把金钱、价格、伤害、奖励、天气或关系的写权限交给事件层。

事件处理顺序固定为：

```text
所属系统裁决输入 → 生成 WorldEvent → EventLog 校验并追加 FactEvent
→ Quest/UI/Agent 读取事实副本 → 所属系统按自己的所有权写入状态
```

Agent 只能读取 `BuildAgentSummary` 生成的已落盘摘要，不能从摘要反写事实，也不能把最终数值放进 AgentReply。

## 2. 字段合同

| 字段 | C# DTO | 语义 | 事实所有权 |
|---|---|---|---|
| `event_id` | `EventId` | 稳定事件 ID；和 `save_revision` 组成去重键 | 触发/所属系统生成，EventLog 追加 |
| `schema_version` | `SchemaVersion` | 事件包版本，当前固定为 `1` | EventSystem 校验 |
| `world_day` | `WorldDay` | 游戏世界日，不使用技术时间戳替代 | Environment/Core 提供，事件记录 |
| `location_ref` | `LocationRef` | 港口、海域、岛屿或场景引用 | 触发/所属系统提供 |
| `actor_ids` | `ActorIds` | 玩家、船员、NPC、船或其他参与者 ID | 触发/所属系统提供 |
| `input_facts` | `InputFacts` | 触发前已经存在的事实引用 | 触发/所属系统提供 |
| `owner_system` | `OwnerSystem` | 裁决结果的 canonical owner 标识 | 所属系统声明；EventLog 不接管 |
| `result` | `Result` | owner 已裁决结果的 opaque 事实码、事实引用和摘要 | 所属系统写入；EventLog 只保存 |
| `cause_ids` | `CauseIds` | 直接因果事实的 `event_id` 列表 | 所属系统提供，EventLog 提供追踪 |
| `save_revision` | `SaveRevision` | 事件所属存档/世界修订号 | Save/World revision 提供 |
| `outcome_status` | `OutcomeStatus` | `normal`、`failed`、`cancelled`、`interrupted`、`recovered` | 所属系统声明，事件层校验生命周期 |
| `recovery_route` | `RecoveryRoute` | 失败或中断事件的可读恢复路径 | 所属系统声明，Quest/UI 读取 |
| `log_sequence` | `LogSequence` | FactEvent 进入日志的追加序号 | EventLog 生成 |

契约单一事实源是 [`contracts/world-event.schema.json`](../../../../contracts/world-event.schema.json)。C# DTO 同时提供 snake_case 访问别名，避免跨端字段名和运行时代码语义漂移。

## 3. 生命周期与恢复

- `Normal`：事实正常完成。
- `Failed`：必须有 `recovery_route`，例如回港维修、补给后重试或转入替代任务。事件层只记录路径，不执行恢复。
- `Cancelled`：由合法的玩家/系统取消；不能伪造为成功结果。
- `Interrupted`：当前流程未完成且必须有 `recovery_route`。恢复由 `WorldEvent.CreateRecovery` 生成新的 `Recovered` 事实。
- `Recovered`：必须在 `cause_ids` 中引用被中断事件；`EventLog.AppendRecovery` 会验证来源确实是 `Interrupted`。

恢复不是覆盖原事件。原中断事实、恢复事实和它们的 `cause_ids` 都保留，因此 Quest、UI、Agent 可以复盘“为什么中断、如何恢复”。

## 4. 因果链与读取投影

事实成功追加后才能读取：

- Quest 使用 `ReadForQuest(eventIds)` 读取目标事件事实，不复制金钱、库存、关系或奖励状态；
- UI 使用 `ReadFactsForUi(maxEntries)` 获取只读副本，按 `log_sequence` 展示；
- Agent 使用 `BuildAgentSummary(maxEntries)` 获取只由事实日志拼成的摘要，再由 AgentCore 的 `AgentMemory.RecordFactSummary` 记录。摘要不产生新事实；
- `TraceCauses(eventId, saveRevision)` / `GetCauseChain` 返回直接和间接因果事实，未找到的历史引用不会被伪造。

## 5. 重复结算保护与读档

唯一去重键为：

```text
event_id + save_revision
```

同键再次 `Append` 时只返回已有 `FactEvent`，不会写入第二条日志。奖励系统或其他领域 owner 在实际写入自己的账本前调用 `EventLog.ClaimSettlement`：

- 首次返回 `Claimed`，`CanApplyOwnerSettlement=true`；
- 已声明结算的事件返回 `AlreadyClaimed`，`CanApplyOwnerSettlement=false`；
- 该门只声明是否首次处理，不发放奖励、不修改金钱/库存/关系等数值。

`EventLog.CaptureSnapshot` 保存事实和结算键；`EventLog.FromSnapshot` 读档时先校验版本、序号、重复键和结算引用，再恢复门状态。D8-A05 负责把这个快照接入 v3 存档封套；在此之前，事件层已经具备读档后的幂等行为和独立测试。

## 6. 版本错误

当前只接受 `schema_version=1` 和事件日志版本 `1`。追加或读档遇到其他版本会抛出 `EventSchemaVersionException`，错误信息明确包含 expected/received，不静默降级、不让 UI 或 Agent 猜字段。

## 7. D7 兼容边界

D3 的 `VoyageEventDefinition`、航程 `seed/eventIds` 和 D4 `RouteLogEntry` 仍是旧快照兼容投影。它们可以被后续迁移器包装为事实引用，但旧的 `waterDelta/foodDelta/hullDelta/moneyDelta` 不会成为 D8 EventLog 的跨系统写值接口。本任务不改 D3/D4 数值裁决逻辑，也不把 D7 文本日志伪装成完整 v3 因果图。

## 8. 跨端边界

`contracts/world-event.schema.json` 是未来只读投影的契约；D8 阶段没有 active endpoint，server/web 不成为世界事件的权威写者。完整本地事实日志由 game 保存；跨端同步必须由后续同步任务按契约同时接入，不得新增私有协议。

