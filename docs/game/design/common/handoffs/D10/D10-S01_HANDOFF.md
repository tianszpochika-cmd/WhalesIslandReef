# D10-S01 交接报告

日期：2026-08-10  
任务：D10-S01 同步集成与总体验收

## 结果

**D10-S01 通过。** 六个 D10-A 主成果已在同一共享 EventLog 与 SaveSystem 快照链上完成验收，允许 D13 只读使用关系/角色数据。

## 新增成果

- `game/Assets/Tests/EditMode/D10S01AcceptanceIntegrationTests.cs`
  - 完整串联工资拖欠失败、招募/救援、公开偏袒、canonical 关系面板原因、CrewSystem 行为变化、港口称谓/换装、保存读档。
  - 断言工资失败不会伪造 `event_d9_wage_settled`；关系原因全部来自已记录 FactEvent。
- `docs/game/design/common/D10_ACCEPTANCE_REPORT.md`
  - 记录验收矩阵、owner/契约检查、测试命令、结果和遗留问题。
- `docs/README.md`
  - 登记 D10 同步集成验收报告入口。
- `game/Save.Tests.csproj`
  - 将集成测试加入本地生成的 Save.Tests 测试程序集入口。

## 新增/复用配置与接口

本任务没有新增数值配置、枚举或经济/战斗参数。复用的唯一入口如下：

- `D9PortShipPreparationSystem.SettleWages`：工资结算和拖欠失败分支。
- `D9CrewEventSystem.Discuss`：CrewSystem 事实报告。
- `D10CrewBehaviorSystem.EnsureState/AssignShift/Tick`：岗位轮班和确定性行为状态机。
- `D10RelationSystem.Apply`：A03 救援、公开偏袒和关系事实。
- `RelationSystem.ApplyFactEvent`：A01 canonical 数值写入；`RefreshFromCanonicalGraph` 仅刷新 A03 投影。
- `D10RelationPanelReadModel.Build`：只读关系原因面板。
- `D10AppearanceSystem.Purchase/Equip` 与 `D10AppearanceOwnerWriter`：外观所有权和委托换装。
- `D10PlayerIdentitySystem.InspectAtPort/ApplyPortUpdate`：港口身份检查和称谓更新。
- `SaveSystem.SaveD3Runtime/SaveD10Runtime/SaveD10RelationGraph/SaveD5Runtime/SaveD10Identity`：既有快照保存入口。

## 替换的硬编码

**无。** D10-S01 是同步验收任务；没有把测试 fixture 的金币、天数或服装价格写回生产配置，也没有改变任何生产公式或最终经济/战斗平衡。

## 检查命令与测试结果

```text
dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal   PASS (0 warning / 0 error)
dotnet build game/Save.Tests.csproj --no-restore -v:minimal          PASS (0 warning / 0 error)
D10.Tests.D10S01AcceptanceIntegrationTests...                       PASS
python tools/d9_s02_acceptance_check.py --json                     PASS, blocked_count=0
python -m unittest discover -s tools/tests -p "test_*.py"            PASS
git diff --check                                                     PASS
```

测试程序集入口为 Unity EditMode `Save.Tests`；本轮还使用临时纯 C# runner 实际执行上述单测，结果为 `D10-S01 acceptance PASS`，runner 已删除，不作为产品代码交付。

## 单一写者/边界复核

- 未复制关系、行为、身份或外观逻辑；测试仅编排 owner 并读取快照。
- 未修改 `contracts/`；未修改 server/web；未在 Systems 加入网络或 LLM。
- Agent 只产生 Crew 报告表达/提议，不能改变航速、金钱、关系、行为或身份结果。
- A01/A03 双投影通过 canonical refresh 接线，关系 panel 不读取可写引用。
- D3 EventLog 快照和 D10/D5/identity/crew snapshots 在同一次读档后保持一致。

## 已知待办

- D9 工资失败暂没有专用 `event_d9_wage_arrears` FactEvent；不得在 D10/UI/Agent 侧补写。若后续需要“拖欠工资”直接成为关系原因，由 D9 工资 owner 另行登记事件。
- 主 Unity Editor 的正式 XML 归档需在释放并行 Unity 实例后补跑；本任务已完成生成测试程序集编译和纯 C# 实际执行。

## D13 使用说明

D13 已获准只读使用关系/角色数据：从 `D10RelationRuntimeSnapshot`、A01 canonical graph、`D10RelationPanelReadModel`、CrewAgent Persona/岗位和 `D10CrewBehaviorState` 读取；关系变化必须由事实 + 对应 owner 决定，不能由 D13 直接写入。
