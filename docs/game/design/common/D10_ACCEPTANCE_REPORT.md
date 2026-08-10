# D10 同步集成验收报告（D10-S01）

日期：2026-08-10  
前置：D10-A01、D10-A02、D10-A03、D10-A04、D10-A05、D10-A06；D9-S02 已通过。

## 结论

**D10-S01 通过。** 完整验收链已由纯 C# 集成测试覆盖，未复制关系、行为或外观裁决；未修改经济、奖励、价格、工资、航速或战斗公式。

集成测试：`game/Assets/Tests/EditMode/D10S01AcceptanceIntegrationTests.cs`  
测试入口：`D10.Tests.D10S01AcceptanceIntegrationTests.FullAcceptance_WageArrearsRescueFavoritismBehaviorIdentityAndSaveLoad`

## 验收矩阵

| 验收步骤 | 结果 | 事实/状态证据 | 唯一 owner |
|---|---|---|---|
| 玩家拖欠工资 | PASS | D9 工资结算返回 `not_enough_money`；忠诚下降；没有错误追加 `wages_paid` | `D9PortShipPreparationSystem` / Economy |
| 救援船员 | PASS | `event_d10_rescue` 进入共享 `EventLog`，关系边绑定该事实 | `D10RelationSystem`（A03） |
| 公开偏袒一次 | PASS | `event_d10_public_favoritism` 进入共享日志，并产生旁观船员的嫉妒关系边 | `D10RelationSystem`（A03） |
| 关系面板显示原因 | PASS | A01 canonical 的 Trust/Affinity 两条边均由只读面板显示救援、偏袒事实；原因均 `Found=true` | `RelationSystem`（A01）只写；`D10RelationPanelReadModel` 只读 |
| 船员行为发生变化 | PASS | CrewSystem 轮班推进 24 小时后从休息进入 `Slacking`，事件带 `event_d9_crew_report` cause | `D10CrewBehaviorSystem` |
| 更换服装与称谓后港口检查 | PASS | 港口上下文允许称谓和外观修改；身份系统委托 Appearance owner 装备 `outfit_missing_tailor_placeholder` | `D10PlayerIdentitySystem` + `D10AppearanceSystem` |
| 保存/读档一致 | PASS | D3 共享 EventLog、D10 阶段投影、A01 canonicalGraph、D5 外观、身份和 `stats_json.behavior` 均恢复；关系面板与港口检查仍可读 | `SaveSystem` 只复制快照 |

## 单一写者与契约检查

- A01 `RelationSystem` 先应用已进入 EventLog 的事实；A03 `D10RelationSystem.RefreshFromCanonicalGraph` 只刷新阶段投影，不写 A01 数值。
- `D10CrewBehaviorSystem` 是岗位、轮班、疲劳、健康和行为状态的唯一裁决入口；Agent/UI 不直接写状态。
- `D10AppearanceSystem` 独占外观所有权和活动槽位；`D10PlayerIdentitySystem` 只写身份表达、历史和证件，并通过 writer 委托换装。
- `SaveSystem.SaveD3Runtime` 保存唯一共享 EventLog；`SaveD10Runtime`、`SaveD10RelationGraph`、`SaveD5Runtime`、`SaveD10Identity` 分别保存既有 owner 快照。
- 本任务没有新增或修改跨端 contracts；没有在 `Systems` 加入网络或 LLM。
- 测试保留工资失败分支，明确断言它不是 `wages_paid`；集成层没有伪造经济事实或直接改变经济结果。

## 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`：通过，0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：通过，0 warning / 0 error。
- D10-S01 集成测试：通过，`D10-S01 acceptance PASS`。
- `python tools/d9_s02_acceptance_check.py --json`：D9-S02 PASS，`blocked_count=0`（前置复核）。
- `python -m unittest discover -s tools/tests -p "test_*.py"`：通过（工作区现有 Python 门禁）。
- AgentCore `dotnet build`、`--mock`、`--config-check`：沿用 D10 前置已通过；本任务未改 AgentCore。
- Systems 红线扫描：D10 Systems 未新增 `System.Net`、`HttpClient`、`UnityWebRequest`、`WebSocket` 或 LLM 实现。
- `git diff --check`：通过；仅有工作区既有换行提示，无新增空白错误。

## 遗留问题与 D13 放行

1. 现有 D9 工资失败路径会改变士气/忠诚但不追加专门的 `event_d9_wage_arrears` 事实；本测试保持该真实失败语义，并用现有 `event_d9_crew_report` 作为行为状态机的可追溯输入。若 D13 需要把“拖欠工资”直接显示为关系原因，应由 D9 工资 owner 后续登记失败事实，不能由关系/UI/Agent 补写。
2. 主 Unity Editor 的最终 EditMode XML 归档仍依赖释放并行 Unity 实例；本任务已完成测试程序集编译和同一测试的纯 C# 运行验证。

**允许 D13 使用关系/角色数据。** D13 可以只读 `D10RelationRuntimeSnapshot`、A01 canonical graph、`D10RelationPanelReadModel`、CrewAgent Persona/岗位和 `D10CrewBehaviorState`；关系原因必须继续从共享 EventLog 读取，任何状态变化仍须回到对应 owner 裁决。
