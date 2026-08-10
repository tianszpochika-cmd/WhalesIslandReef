# D10-A03 交接报告：关系阶段、恋爱与冲突事件

- 任务编号：D10-A03
- 日期：2026-08-10
- 前置 D9-S02：依据现有验收报告与交接报告，状态 PASS
- D10-A01：canonical 关系代码、`D10_RELATION_MODEL.md` 和 A01 handoff 已存在并可用；本任务接入其 `RelationGraphSnapshot`，并以 D8 `RelationEntity` 与 D2/D9 关系基线保持兼容

## 修改文件

### 代码与测试

- `game/Assets/Scripts/Systems/D10/D10RelationRomanceContracts.cs`
- `game/Assets/Scripts/Systems/D10/D10RelationRomanceSystem.cs`
- `game/Assets/Scripts/Systems/D10.meta`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Assets/Tests/EditMode/D10RelationRomanceTests.cs`
- `game/Assets/Tests/EditMode/D10RelationRomanceTests.cs.meta`

### 文档与日志

- `docs/game/design/common/D10_ROMANCE_CONFLICT_EVENTS.md`
- `docs/game/design/common/handoffs/D10/D10-A03_HANDOFF.md`
- `.workbuddy/memory/2026-08-10.md`

## 公共类型与接口

- `D10RelationPhase`：Acquaintance、Trust、Intimacy、Commitment、Conflict、Stable、Separation。
- `D10RomanceConsent`：Undecided、Open、Accepted、Declined、Separated。
- `D10RelationActionRequest`：source/target、动作、世界日、save revision、cause IDs 和偏袒旁观者列表。
- `D10RelationEdge`：兼容 D8 source/target/cause/visibility/last day 字段，扩展关系数值、阶段、恋爱同意、承诺和冲突状态。
- `D10RelationRuntimeSnapshot`：关系边、revision、last event 和 applied event keys。
- `D10RelationSystem.Apply`：唯一关系写入口；先 `EventLog.Append`，成功后再更新关系快照。
- `D10RelationSystem.RefreshFromCanonicalGraph`：只读 A01 canonical 好感/信任/依恋/尊敬/嫉妒边，刷新 A03 阶段投影；不修改 A01 `RelationEdge.Value`。
- `SaveSystem.SaveD10Runtime/LoadD10Runtime`：`location_json.d10` 的唯一存档入口。
- A01 `RelationGraphSnapshot`：作为 `D10RelationRuntimeSnapshot.CanonicalGraph` 保存在同一 D10 容器，不由 A03 阶段系统重写。

## 数据所有权

`D10RelationSystem` 是 CrewSystem 的关系子系统，唯一写入 A03 阶段投影、同意、承诺、嫉妒和冲突状态；A01 `RelationSystem` 继续唯一写入 `canonicalGraph` 的九类关系边。事件 owner 写为 `CrewSystem`，事实由共享 `EventLog` 保存。Inventory/Economy 仍拥有礼物成本和物品；QuestSystem 拥有个人任务完成状态；Agent 只能提出表达/行动建议；UI 只读快照和事实。

## 事件与阶段结论

已实现 12 个可追溯关系事件：

`event_d10_relation_gift`、`event_d10_promise_kept`、`event_d10_rescue`、`event_d10_public_favoritism`、`event_d10_jealousy`、`event_d10_argument`、`event_d10_night_talk`、`event_d10_personal_task`、`event_d10_romance_accepted`、`event_d10_romance_declined`、`event_d10_conflict_stabilized`、`event_d10_relation_separated`。

事件按 `event_id + save_revision` 去重；关系边保存 `cause_event_ids` 和最后事件 ID。公开奖励/偏袒可对旁观者建立嫉妒关系边；争执可进入 Conflict，调解后进入 Stable，主动分离进入 Separation。

恋爱接受必须满足 Intimacy、好感、信任和依恋阈值；一旦玩家或另一方拒绝，后续接受会返回 `romance_route_declined`，不会被对白或 UI 强制重启。

## 测试命令与结果

已执行并通过：

```text
dotnet build game/.codex-d10-a03-compile.csproj --no-restore
dotnet run --project game/.codex-d10-a03-smoke.csproj --no-restore
dotnet build game/Whalebone.Systems.csproj --no-restore
dotnet build game/AgentCore/AgentCore.csproj --no-restore
python tools/d9_s02_acceptance_check.py --json
python tools/d9_vertical_slice_check.py
python -m unittest tools.tests.test_d9_s02_acceptance tools.tests.test_d9_vertical_slice_freeze -v
```

结果：D10 关系/事件纯 C# 临时编译 0 warning、0 error；行为烟测输出 `D10_A03_SMOKE_PASS`，canonical 读取烟测输出 `D10_A03_CANONICAL_REFRESH_PASS`；包含 D10 生产代码的 `Whalebone.Systems`、AgentCore 编译通过；D9-S02 门禁 PASS（16/16 checks，blocked=0），相关 Python unittest 3/3 通过。临时编译工程、烟测源和输出已删除。

未通过/未能执行的工程级验证：

- `dotnet build game/Save.Tests.csproj --no-restore` 被工作区既有 `D10CrewBehaviorSystemTests` 找不到 `D10CrewBehaviorSystem`/`D10CrewBehaviorEventType` 阻断（11 errors），不属于本任务新增关系阶段文件。
- 主 Unity EditMode 未生成 XML；当前有多个 Unity 实例，且主工程日志已有 `AgentMemory.RecordFactSummary` 编译错误。定向入口仍为 `D10.Tests.D10RelationRomanceTests`，待工程锁释放和无关编译错误修复后补跑。

Unity EditMode 入口：`D10.Tests.D10RelationRomanceTests`。本报告生成时尚未获得主 Unity 工程的独占锁，需在工程可用时执行定向测试并归档 XML。

## 未完成事项与冲突

1. A01 canonical 关系模型已完成并接入；本任务没有改写 AgentCore V3 公共类型，也没有把 D10 阶段投影宣称为 A01 的九类关系图。D10-S01 仍需把 A01 事实应用和 `RefreshFromCanonicalGraph` 串成同一运行时流程，并验证阶段读取 canonical graph 后不产生第二份数值裁决。
2. 完整 Unity/生成 Systems 工程回归需要主工程释放 Unity 锁；纯 C# 新增关系代码已独立编译。
3. 事件事实依赖 D9 已有共享 EventLog 保存入口。未来若 D10 有独立运行时启动器，必须绑定同一个 EventLog，不得新建第二份。
4. 礼物成本、个人任务完成、工资/经济和 Agent 对话仍由各自 owner 负责，本任务不包含这些玩法的数值实现。

## 后续注意事项

- 后续迁移保留 `source_id`、`target_id`、`cause_event_ids`、`last_changed_day`、旧 affinity、A01 `canonicalGraph` 和 D10 `d10` 存档字段；不要把两个关系投影合并成第二个写者。D10-S01 接入时先由 A01 `RelationSystem` 裁决 canonical 值，再调用阶段系统的只读刷新入口。
- D10-A02/A04/A05/A06 只能读取关系快照或提交合法动作意图，不得直接改阶段、恋爱同意、承诺或冲突。
- D10-S01 集成时必须用同一个 EventLog 验证拖欠工资、救援、公开偏袒、行为变化、身份/外观和读档恢复。
