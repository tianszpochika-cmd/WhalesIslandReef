# D10-A02 船员岗位与行为状态

状态：D10-A02 已实现（2026-08-10）  
前置：D9-S02 已通过。  
范围：轮班、岗位效率、疲劳、健康、拒绝执行、消极怠工、请假、调岗、离队和叛变前兆。  
非范围：经济价格、工资、奖励、航速公式、伤害和最终叛变结果。

## 1. 唯一写者与边界

`CrewSystem/D10CrewBehaviorSystem` 是本任务的唯一裁决入口，负责写入：

- `CrewAgent.BehaviorState`：岗位、班次、状态、效率、请假/调岗/离队请求、计数和叛变前兆；
- `CrewAgent.Stats.Health` 与 `CrewAgent.Stats.Fatigue` 的 D10 行为结算值；
- 由 `EventLog.Append` 接收的行为事实。

AgentCore 中的 `D10CrewBehaviorState` 与 `D10CrewBehaviorConfiguration` 只保存纯 C# 状态和规则，零 Unity、网络和第三方依赖。Agent/LLM 只能对白、摘要或意向表达；它不能调用本系统的状态裁决方法，也不能写健康、疲劳、岗位、工资、关系、任务或奖励最终值。UI 只能读取快照和事实事件，不能成为状态写者。

岗位效率只返回船员岗位工作的百分比，不直接改航速、战斗伤害、价格、金钱或奖励。最终玩法系统若需要效率，必须读取 `CalculateEfficiency` 的结果并在自己的所有权范围内裁决。

## 2. Canonical 枚举与状态

`CrewShift`：`OffDuty`、`DayWatch`、`NightWatch`。  
`CrewBehaviorStatus`：`Unassigned`、`Resting`、`Working`、`Slacking`、`Refusing`、`Injured`、`LeaveRequested`、`OnLeave`、`TransferRequested`、`DepartureRequested`、`Departed`。  
`CrewMutinyForewarning`：`None`、`Tension`、`Warning`、`Critical`。  
岗位枚举复用 D9 AgentCore 的 `CrewDuty`：`None`、`Helm`、`Navigator`、`Deckhand`、`Lookout`、`Gunner`、`Repair`、`Cook`、`Medic`。

关键转移：

```text
Unassigned --EnsureState--> Resting
Resting --active shift / healthy--> Working
Working --fatigue >= 72 or health < 55--> Slacking
Working/Slacking --fatigue >= 90 or health < 25--> Refusing
任何非离队状态 --health < 15--> Injured
LeaveRequested --CrewSystem approve--> OnLeave --elapsed leave--> Resting
TransferRequested --CrewSystem approve--> Resting（CurrentDuty 更新）
DepartureRequested --CrewSystem approve--> Departed（岗位/班次清空）
```

拒绝、怠工和受伤不会由 LLM 选择。`Tick` 每次只推进 1—24 个离散小时；同一 `world_day + save_revision + 参数` 的 `LastCommandKey` 只结算一次。请假按小时累计，满 24 小时才减少一天，跨小时读档不会丢失余量。

## 3. 确定性规则

每个工作小时：疲劳 `+3`；休息小时：疲劳 `-5`；请假小时：疲劳 `-6`；健康和疲劳均限制在 `[0,100]`。工作条件是当前班次等于已分配班次、岗位非 `None` 且船员不是请假/离队/受伤/拒绝状态。

效率计算顺序为：

```text
岗位基础效率
× 未获资格时的 55% 上限
× 夜班 90% 系数（仅 NightWatch）
× 疲劳分段系数
× 健康分段系数
× 怠工 60% 系数（仅 Slacking）
× Critical 前兆 50% 系数（仅 Critical）
```

`Refusing`、`Injured`、休息班和非当前班次的效率为 `0%`。叛变前兆仅是风险分段的可追踪状态，不是叛变执行：基础风险来自现有 `CrewStats.GetMutinyRisk()`，再加拒绝次数 `0.04/次`、怠工次数 `0.02/次`，系统裁决为 `None < 0.18`、`Tension >= 0.18`、`Warning >= 0.30`、`Critical >= 0.50`。

## 4. 统一配置登记

所有 D10 行为参数只在 `game/Assets/Scripts/Agent/AgentCore/D10CrewBehaviorState.cs` 的 `D10CrewBehaviorConfiguration` 定义。Owner 是 `CrewSystem/D10CrewBehaviorSystem`；来源为 D10-A02 状态规则，健康/疲劳范围复用 D8 numeric canonical `[0,100]`。测试入口是 `D10CrewBehaviorStateTests.RunAll()`，Unity 状态和存档入口是 `D10CrewBehaviorSystemTests`。

| 参数 | 单位 | 默认值 | 最小值 | 最大值 | 拥有者 | 来源 | 测试入口 |
|---|---:|---:|---:|---:|---|---|---|
| `StateSchemaVersion` | 版本号 | 1 | 1 | 1 | CrewSystem | D10 state schema | 状态往返 |
| `WorkFatiguePerHour` | 疲劳点/小时 | 3 | 0 | 100 | CrewSystem | D10-A02 | 规则确定性 |
| `RestFatigueRecoveryPerHour` | 疲劳点/小时 | 5 | 0 | 100 | CrewSystem | D10-A02 | 轮班测试 |
| `LeaveFatigueRecoveryPerHour` | 疲劳点/小时 | 6 | 0 | 100 | CrewSystem | D10-A02 | 请假测试 |
| `SlackingFatigueThreshold` | 疲劳点 | 72 | 0 | 100 | CrewSystem | D10-A02 | 怠工测试 |
| `RefusalFatigueThreshold` | 疲劳点 | 90 | 0 | 100 | CrewSystem | D10-A02 | 拒绝测试 |
| `InjuredHealthThreshold` | 健康点 | 15 | 0 | 100 | CrewSystem | D8 health | 受伤测试 |
| `RefusalHealthThreshold` | 健康点 | 25 | 0 | 100 | CrewSystem | D8 health | 拒绝测试 |
| `SlackingHealthThreshold` | 健康点 | 55 | 0 | 100 | CrewSystem | D8 health | 怠工测试 |
| `TransferHealthMinimum` | 健康点 | 35 | 0 | 100 | CrewSystem | D10-A02 | 调岗测试 |
| `TransferFatigueMaximum` | 疲劳点 | 85 | 0 | 100 | CrewSystem | D10-A02 | 调岗测试 |
| `MaxLeaveDays` | 天 | 7 | 1 | 7 | CrewSystem | D10-A02 | 请假测试 |
| `DutyEfficiencyPercent(Helm/Navigator/Gunner/Medic)` | % | 100 | 0 | 100 | CrewSystem | D10-A02岗位表 | 效率测试 |
| `DutyEfficiencyPercent(Lookout/Cook)` | % | 95 | 0 | 100 | CrewSystem | D10-A02岗位表 | 效率测试 |
| `DutyEfficiencyPercent(Repair)` | % | 90 | 0 | 100 | CrewSystem | D10-A02岗位表 | 效率测试 |
| `DutyEfficiencyPercent(Deckhand)` | % | 85 | 0 | 100 | CrewSystem | D10-A02岗位表 | 效率测试 |
| `DutyUnqualifiedEfficiencyPercent` | % 上限 | 55 | 0 | 100 | CrewSystem | D10-A02 | 资格边界测试 |
| `NightWatchEfficiencyPercent` | % 系数 | 90 | 0 | 100 | CrewSystem | D10-A02 | 夜班效率测试 |
| `SlackingEfficiencyPercent` | % 系数 | 60 | 0 | 100 | CrewSystem | D10-A02 | 怠工效率测试 |
| `CriticalForewarningEfficiencyPercent` | % 系数 | 50 | 0 | 100 | CrewSystem | D10-A02 | 前兆效率测试 |
| `MutinyLoyaltyRiskStart` | 忠诚点 | 40 | 0 | 100 | CrewSystem | D9 CrewStats 原规则 | 前兆风险测试 |
| `MutinyLoyaltyRiskPerPoint` | 风险/忠诚点 | 0.012 | 0 | 1 | CrewSystem | D9 CrewStats 原规则 | AgentCore 风险测试 |
| `MutinyMoraleRiskStart` | 士气点 | 40 | 0 | 100 | CrewSystem | D9 CrewStats 原规则 | 前兆风险测试 |
| `MutinyMoraleRiskPerPoint` | 风险/士气点 | 0.006 | 0 | 1 | CrewSystem | D9 CrewStats 原规则 | AgentCore 风险测试 |
| `MutinyFatigueRiskStart` | 疲劳点 | 70 | 0 | 100 | CrewSystem | D9 CrewStats 原规则 | 前兆风险测试 |
| `MutinyFatigueRiskPerPoint` | 风险/疲劳点 | 0.004 | 0 | 1 | CrewSystem | D9 CrewStats 原规则 | AgentCore 风险测试 |
| `MutinyAffinityRiskIncrement` | 风险 | 0.10 | 0 | 1 | CrewSystem | D9 CrewStats 原规则 | 前兆风险测试 |
| `RefusalRiskPerCount` | 风险/次 | 0.04 | 0 | 1 | CrewSystem | D10-A02 | 前兆风险测试 |
| `SlackingRiskPerCount` | 风险/次 | 0.02 | 0 | 1 | CrewSystem | D10-A02 | 前兆风险测试 |

`D10CrewBehaviorConfiguration.Validate()` 检查阈值顺序、百分比、风险增量、请假天数、岗位效率和 schema 版本；失败返回错误代码列表，不自动修正配置。它由纯 C# 测试覆盖。该配置没有经济或战斗参数，因此本任务不构成最终平衡调整。

## 5. 系统入口与事实事件

公开入口全部返回 `D10CrewBehaviorResult`，拒绝时返回稳定 code，不直接把 Agent 提议当成结果：

- `EnsureState`、`AssignShift`：初始化和轮班；
- `Tick`：按小时推进疲劳、健康影响、工作效率和行为状态；
- `RequestDutyTransfer` / `ApproveDutyTransfer`：资格、健康和疲劳边界由系统检查；
- `RequestLeave` / `ApproveLeave` / `ReturnFromLeave`：请求与批准分离；
- `ApplyHealthChange`：健康变化的 CrewSystem 入口；
- `RequestDeparture` / `ApproveDeparture`：离队请求与裁决分离；
- `EvaluateMutinyForewarning`：只更新前兆，不执行叛变；
- `CalculateEfficiency`：只读确定性效率结果。

每次接受的状态变化生成 `event_d10_crew_behavior_<crew>_<type>_<sequence>_<day>_<revision>`，`owner_system=CrewSystem`，值中包含状态前后、岗位、班次、效率、健康、疲劳和前兆。调用方传入的 `cause_event_ids` 原样去重后进入事实 `cause_ids`；例如健康伤害、轮班安排、D9 船员报告可以成为拒绝、请假或前兆的上游事实。`EventLog` 仍是事实唯一写入口，D10 不复制事实存储。

## 6. 存档与兼容

`CrewAgentCodec` 将行为状态写进既有 `crew_agents.stats_json.behavior`，并将 Persona 的 `primary_duty/qualified_duties` 写进既有 `persona_json`。`role` 列从当前岗位映射为 `deckhand/helm/navigator/lookout/gunner/repair/cook/medic`；旧存档缺失行为对象时使用安全默认值。读档后 `D10CrewBehaviorState.Restore` 做枚举和边界归一化，随后仍必须由 CrewSystem 继续裁决，UI/Agent 不参与恢复写入。

不新增跨端合同或数据库列，不改工资、价格、奖励、战斗和航速公式。共享 `EventLog` 的快照仍由已有事件/运行时存档 owner 负责；D10 行为状态本身由船员行存档保证回读。

## 7. 验收入口

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --mock
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
dotnet build game/Save.Tests.csproj --no-restore -v:minimal
```

纯 C# 测试覆盖状态往返、非法状态归一化、参数合法性和确定性规则；Unity EditMode 测试覆盖班次效率、怠工/拒绝/受伤、请假/调岗/离队、前兆因果和船员存档回读。

## 8. 2026-08-11 standalone build 复验

为解除 D15-S02 前置工程阻断，已将 D10 状态加入 `Whalebone.Agent.Runtime.csproj`，并把当前 Systems asmdef 中遗漏的 D10/D13 源文件同步加入 `Whalebone.Systems.csproj` 的显式编译清单。复验结果：

```text
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal       0 warning / 0 error
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal          0 warning / 0 error
dotnet build game/Save.Tests.csproj --no-restore -v:minimal                 0 warning / 0 error
```

该修复只同步 standalone 工程引用，不改变 D10 状态规则、数值裁决或 Unity Systems 所有权。

## 9. 2026-08-11 工程依赖复验

根据最终验收任务的第一阶段门禁重新核对：

- `game/Assets/Scripts/Agent/AgentCore/D10CrewBehaviorState.cs` 是唯一 D10 状态载体；没有新增第二份类型。
- `game/Whalebone.Agent.Runtime.csproj` 和 `game/AgentCore/AgentCore.csproj` 都显式包含该状态文件；`CrewAgent` 只持有 `BehaviorState`，没有改变数据所有权。
- 本机第一次执行 `Save.Tests --no-restore` 时只因缺少 `game/Temp/obj/Save.Tests/project.assets.json` 失败，不是缺少 D10 类型；补执行 `dotnet restore game/Save.Tests.csproj --ignore-failed-sources` 后恢复正常。
- 复验结果：`dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`、`dotnet build game/Save.Tests.csproj --no-restore -v:minimal` 均为 0 warning / 0 error。

## 10. 2026-08-12 当前工程编译复验

- Unity 刷新后 `Save.Tests` 和 `Assembly-CSharp` 的 `Temp/obj/*/project.assets.json` 可能被清理；本轮先执行受限 `dotnet restore ... --ignore-failed-sources`，再按 `--no-restore` 编译，没有通过缩小测试范围规避问题。
- `AgentCore`、`Save.Tests`、`Assembly-CSharp` 当前均为 **0 warning / 0 error**；D10 状态文件仍只有唯一主源，未新增重复类型。

## 11. 2026-08-12 总纲顺序复验

- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `Save.Tests --no-restore` 首次仅因 Unity 临时 `project.assets.json` 缺失失败；受控 `dotnet restore game/Save.Tests.csproj --ignore-failed-sources` 后再次构建：0 warning / 0 error。
- 本轮没有修改 D10 类型、CrewAgent 数据所有权或测试范围；D10 工程依赖门禁继续 PASS。

## 12. 2026-08-12 Unity 依赖重新生成复验

Unity 刷新工程后，`game/Library/ScriptAssemblies` 中的 Test Runner/UI 程序集曾暂时缺失，导致 `Save.Tests.csproj` 无法解析 Unity 测试类型。这是生成工程产物缺失，不是 D10 源码或类型缺失。已通过 Unity 6 无界面重新导入工程，让 Unity 按 `Packages/manifest.json` 重新生成派生程序集；没有直接编辑生成的 `.csproj` 或复制 DLL。

验证结果：

```text
Unity 6000.0.80f1 batch reimport：退出码 0
dotnet restore game/Save.Tests.csproj -v:minimal：成功
dotnet build game/Save.Tests.csproj --no-restore -v:minimal：0 warning / 0 error
```

生成的 `UnityEditor.TestRunner.dll`、`UnityEngine.TestRunner.dll`、`UnityEditor.UI.dll` 和 `UnityEngine.UI.dll` 已恢复，D10 规则、CrewAgent 数据所有权和测试范围未改变。该证据只解决本机工程依赖门禁，不扩大为 Unity 真机、目标设备或 D16 发布验收。
