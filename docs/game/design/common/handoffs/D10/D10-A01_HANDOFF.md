# D10-A01 交接报告：多维关系系统

## 结论

D10-A01 已完成。D9-S02 前置门禁已复核通过：`D9-S02 PASS: blocked=0, frozen=14`。

## 主成果

- `game/Assets/Scripts/Systems/D10/D10RelationContracts.cs`
  - `RelationEdge`、`RelationChangeIntent`、`RelationGraphSnapshot`、`RelationApplyResult`；
  - schema/version、关系类型、可见性、错误状态和明确版本异常。
- `game/Assets/Scripts/Systems/D10/D10RelationSystem.cs`
  - `RelationSystem.ApplyFactEvent(FactEvent, RelationChangeIntent)` 是 canonical 关系值唯一写入口；
  - 九类关系边：`affinity`、`trust`、`loyalty`、`fear`、`respect`、`attachment`、`jealousy`、`debt`、`romance`；
  - 事实必须已进入 EventLog，且事件必须引用关系两端角色；
  - 每条边记录 `cause_event_ids`、`last_changed_day`、`last_change_save_revision` 和 `applied_event_keys`。
- `game/Assets/Scripts/Systems/D10/D10RelationPanelReadModel.cs`
  - 只读关系面板快照；显示事实摘要、事件 ID、owner、日期和状态；
  - 对未知角色和缺失原因事件返回明确状态/文案。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
  - 将 A01 canonical graph 接入现有 `D10RelationRuntimeSnapshot.CanonicalGraph`；
  - `SaveD10RelationGraph`/`LoadD10RelationGraph` 只复制快照，不裁决关系。
- `game/Assets/Scripts/Systems/D2/CrewRelationSystem.cs`
  - 旧对白/AgentReply 入口保留兼容签名，但不再直接写 `CrewStats.Affinity`；
  - 提议必须通过已记录 FactEvent 转入 `RelationSystem`。
- `game/Assets/Tests/EditMode/D10RelationModelTests.cs`
  - 覆盖九类关系、因果链、重复保护、版本/缺失/未知处理、面板解释和 SQLite 存档读档。
- 设计文档：[D10_RELATION_MODEL.md](../../D10_RELATION_MODEL.md)。

## 事件字段、所有权与写入链

关系变化使用 D8 `FactEvent` 的固定字段：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`save_revision`。事件先由所属领域系统 `EventLog.Append(WorldEvent)` 写入，返回 `FactEvent` 后才允许关系系统应用。

| 数据 | 唯一写者 | 其他系统权限 |
|---|---|---|
| 事实事件 | 产生事实的领域 owner + EventLog 追加门 | Quest/UI/Agent 只读 |
| `RelationEdge.Value`、cause、日期、修订号、重复键 | `RelationSystem` | Agent 只能意图；UI/Save 只读/序列化 |
| D10 阶段/恋爱投影 | 现有 D10 阶段系统 | 只可读取 A01 canonical graph，不得私藏 A01 数值 |
| SQLite `location_json.d10` | SaveSystem | 只复制快照，不改变关系结果 |

## 重复结算保护

事件键统一为 `event_id + save_revision`，并在边级 `applied_event_keys` 保存。首次应用写入关系值；同一事实再次应用（包括读档恢复后）返回 `duplicate_relation_event`，`AppliedDelta=0`，不会重复叠加。EventLog 自身仍负责事实追加去重和快照恢复校验。

## 测试命令和结果

- `python tools/d9_s02_acceptance_check.py` → PASS，`blocked=0, frozen=14`。
- `dotnet build game/Whalebone.Systems.csproj --no-restore` → 0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore` → 0 error；工作区已有 `D10CrewBehaviorState.cs` 报 7 个 CS0162 不可达代码警告；使用 `-p:NoWarn=0162` 复核后为 0 warning / 0 error。
- D10-A01 定向 NUnit 兼容执行器调用 `D10RelationModelTests` 5 项 → `D10-A01 tests PASS`。
- `git diff --check` → 通过。

主 Unity 编辑器当前已有实例占用，未强制关闭或抢占；Unity EditMode XML 需在释放工程锁后执行。测试代码已纳入 `Save.Tests.csproj`，可由 Unity Test Runner 执行。

## 已知边界

- A01 的 canonical graph 已进入现有 D10 runtime 容器，但 D10 恋爱/阶段投影仍使用 `D10RelationEdge`；两者不应各自再保存一份 A01 数值。后续 D10-S01 应验证阶段投影读取 canonical 关系，不复制裁决公式。
- 关系系统不直接裁决金币、债务转账、工资、价格、伤害、奖励、天气或势力数值；`debt` 边只保存关系域欠债事实。
- 缺失事件不会补造因果；旧 D7 affinity 迁移若没有事实，只能保留 legacy 来源标记，不能伪造 `event_id`。
- 角色注册目录由 Crew/Character 编排层提供；未知 source/target 明确拒绝，未实现跨存档角色自动补注册。
- 完整 Unity EditMode 回归和 D10-S01 集成尚未在本任务内宣称通过。
