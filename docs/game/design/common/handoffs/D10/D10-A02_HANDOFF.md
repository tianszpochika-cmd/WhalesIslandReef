# D10-A02 任务交接报告

日期：2026-08-10  
任务：D10-A02 船员轮班、岗位效率与行为状态  
前置：D9-S02（PASS，`blocked_count=0`）

## 交付结果

已完成确定性 CrewSystem 状态机：轮班、岗位效率、疲劳、健康、拒绝执行、消极怠工、请假、调岗、离队和叛变前兆。LLM/Agent 没有行为状态写入口；状态、健康和疲劳由 `CrewSystem/D10CrewBehaviorSystem` 裁决，事件由共享 `EventLog` 追加。

## 新增与修改文件

- `game/Assets/Scripts/Agent/AgentCore/D10CrewBehaviorState.cs`：D10 状态载体、canonical 枚举、纯 C# 配置和合法性检查；包含请假小时累计字段，支持跨小时存档。
- `game/AgentCore/AgentCore.csproj`：链接 D10 状态和纯 C# 测试入口。
- `game/Assets/Scripts/Agent/AgentCore/D10CrewBehaviorStateTests.cs`：状态往返、非法状态归一化、规则确定性和配置合法性测试。
- `game/Assets/Scripts/Agent/AgentCore/CrewAgent.cs`、`game/AgentCore/CrewAgent.cs`：挂载可保存的 `BehaviorState`，快照只读投影岗位/班次/状态/效率。
- `game/Assets/Scripts/Agent/AgentCore/AgentModels.cs`、`game/AgentCore/AgentModels.cs`：将既有叛变风险系数集中读取 D10 配置，数值保持原值。
- `game/Assets/Scripts/Systems/D10/D10CrewBehaviorSystem.cs`：CrewSystem 唯一状态裁决入口、幂等 Tick、岗位/班次/健康/疲劳规则和 D10 事实事件。
- `game/Assets/Scripts/Systems/Save/CrewAgentCodec.cs`：在既有 `stats_json.behavior` 保存/恢复行为状态，在 `persona_json` 保存岗位资格，并按当前岗位映射既有 RoleId。
- `game/Assets/Tests/EditMode/D10CrewBehaviorSystemTests.cs`：Unity EditMode 测试，覆盖效率、怠工/拒绝/受伤、请假/调岗/离队、前兆因果和存档回读。
- `docs/game/design/common/D10_CREW_BEHAVIOR_STATE.md`：状态机、边界、配置登记、事实事件和存档说明。
- `docs/game/design/common/README.md`、`docs/game/design/README.md`、`docs/README.md`：新增 D10 设计入口。

## 新增配置登记

统一入口为 `D10CrewBehaviorConfiguration`，配置说明表已完整登记单位、默认值、最小值、最大值、拥有者、来源和测试入口：

- 每小时疲劳变化：工作 `+3`、休息恢复 `5`、请假恢复 `6`；
- 状态阈值：疲劳怠工 `72`、疲劳拒绝 `90`、健康受伤 `15`、健康拒绝 `25`、健康怠工 `55`；
- 调岗边界：最低健康 `35`、最高疲劳 `85`；请假上限 `7` 天；
- 岗位基础效率：Helm/Navigator/Gunner/Medic `100%`、Lookout/Cook `95%`、Repair `90%`、Deckhand `85%`；
- 效率系数：无资格上限 `55%`、夜班 `90%`、怠工 `60%`、Critical 前兆 `50%`；
- 前兆增量：拒绝 `0.04/次`、怠工 `0.02/次`；原有 CrewStats 忠诚/士气/疲劳/好感风险系数集中登记，原值不变。

`D10CrewBehaviorConfiguration.Validate()` 检查 schema、阈值顺序、百分比、风险增量、岗位效率和请假上限；返回错误列表，不进行自动平衡修正。

## 替换的硬编码

- `CrewStats.GetMutinyRisk()` 中原有忠诚、士气、疲劳和好感风险系数不再散落为字面量，改为读取 `D10CrewBehaviorConfiguration`；默认值与公式不变。
- `CrewAgentCodec` 原来固定写入 `role=deckhand`，现在从 D10 `CurrentDuty` 映射到既有 RoleId；旧存档缺少行为对象时仍安全回落为默认岗位。
- D10 阈值、效率和行为计数全部只在 `D10CrewBehaviorConfiguration`/`D10CrewBehaviorSystem` 使用，没有在 UI、Agent 或其他 Systems 复制第二套裁决值。

本任务没有修改价格、奖励、工资数值、航速公式、战斗伤害或最终经济/战斗平衡。

## 配置检查与测试结果

已执行：

```text
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --config-check
dotnet run --project game/AgentCore/AgentCore.csproj --no-build -- --mock
dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
dotnet build game/Save.Tests.csproj --no-restore -v:minimal
python tools/d9_s02_acceptance_check.py --json
python -m unittest tools.tests.test_d9_s02_acceptance tools.tests.test_d9_vertical_slice_freeze tools.tests.test_content_registry_check -v
python tools/d9_vertical_slice_check.py
git diff --check
```

结果：

- AgentCore：0 警告、0 错误；`--mock` 全部自测通过，D10 状态/规则/配置 4 项通过；
- `--config-check`：D8 统一配置检查全部通过；D10 配置合法性检查包含在 AgentCore 全量自测中并通过；
- Whalebone.Systems：0 警告、0 错误；
- Save.Tests：0 警告、0 错误，D10 EditMode 测试程序集已编译并包含测试文件；
- D9-S02 acceptance：PASS，`blocked_count=0`，冻结流程和 14 个事件链通过；
- D9 Python 单元测试：10/10 passed；垂直切片冻结检查 PASS；
- Systems 网络/LLM 红线扫描：D10 行为代码无网络、LLM 或 Unity 依赖；
- `git diff --check`：通过（仅报告仓库既有换行风格提示）。

Unity Editor 当前已有实例占用项目，未在本轮强制关闭或抢占编辑器执行 EditMode Runner；因此 Unity 测试本轮以 `Save.Tests.csproj` 编译作为工程侧验证，打开当前 Unity 6 工程后可直接运行 `D10CrewBehaviorSystemTests`。

## 接入约定

运行时创建 `D10CrewBehaviorSystem` 时注入当前共享 `EventLog`，并把上游事实 ID 传入 `causeEventIds`。Agent 只可展示 `D10CrewBehaviorState` 快照或生成表达；所有 Request/Approve/Tick/ApplyHealthChange 必须经 CrewSystem 入口，禁止 UI 直接写 `Stats` 或 `BehaviorState`。
