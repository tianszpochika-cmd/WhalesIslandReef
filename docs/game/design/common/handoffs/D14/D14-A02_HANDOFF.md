# D14-A02 交接报告：接舷、俘虏与战后处理

日期：2026-08-11  
任务：D14-A02  
前置：D13-S01、D14-A01 已通过

## 已交付

- `game/Assets/Scripts/Systems/D14/D14BoardingAftercareContracts.cs`
  - 接舷案件、人员/武器/士气、伤亡、俘虏、战利品、治疗、救援、审讯、葬礼、关系投影、敏感内容和 authority 契约。
- `game/Assets/Scripts/Systems/D14/D14BoardingAftercareSystem.cs`
  - 接舷条件、投降后处理、捕获/撤退/非歼灭结局、统一 EventLog、幂等和 D10 关系适配器。
  - A02 不直接写 Crew、Inventory、Medical、Legal、Community 或 Relation canonical 状态。
- `game/Assets/Scripts/Systems/Save/GameState.cs`
  - 增加 `CurrentD14AftercareRuntime`。
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
  - 增加 `D14AftercareRuntime`。
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
  - 增加 `SaveD14AftercareRuntime`/`LoadD14AftercareRuntime`，接入 `location_json.d14Aftercare`。
- `game/Assets/Tests/EditMode/D14BoardingAftercareTests.cs`
  - 3 项 NUnit 测试，authority 使用带来源 fixture，关系测试实际经过 D10 `RelationSystem` 适配器。
- `tools/d14_a02_runner/`
  - 独立运行入口。
- `docs/game/design/common/D14_BOARDING_PRISONERS_AFTERCARE.md`
  - 所有权、接舷、战后、敏感内容、关系因果和存档规则。

## 新接口/使用约定

1. 通过 `D14CombatBoardingAuthority` 读取 A01 快照；不得从 UI/Agent 直接写 `D14AftercareRuntimeSnapshot`。
2. 生产接线必须注入 `ID14CasualtyAuthority`、`ID14PrisonerAuthority`、`ID14LootAuthority`、`ID14MedicalAuthority`、`ID14InterrogationAuthority`、`ID14FuneralAuthority` 和 `ID14RelationAuthority`。缺省实现会明确拒绝需要外部裁决的操作。
3. 战利品必须带已存在且 owner 为 `FactionLegalSystem` 的法律来源；InventorySystem 返回物品和库存事实后，A02 只保存投影。
4. 审讯/葬礼必须提供历史/敏感审查标签；`ApprovedWithWarning` 要确认或跳过，跳过只记事实不调用敏感 authority。
5. 关系变化通过 `D14D10RelationAuthority` 调用 D10 `RelationSystem.ApplyFactEvent`，A02 不改 `RelationEdge.Value`。
6. `SaveSystem.SaveD14AftercareRuntime(system.CaptureSnapshot())` 是落库入口；读档后重放原 `OperationId + SaveRevision` 返回 duplicate。

## 验证结果

- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore --nologo -v:minimal`：0 warning / 0 error。
- `dotnet run --project tools/d14_a02_runner/D14A02Runner.csproj --no-restore`：`D14-A02 tests PASS count=3`。
- D14-A01 回归：`PASS count=2`。
- D13-S01 回归：`PASS count=1`。
- D14 Systems 红线扫描：无网络、HttpClient、UnityWebRequest、LLM、Agent 裁决路径。

## 交接边界

- A02 保存的是战后案件和所属系统结果投影；实际伤亡/健康、俘虏实体、货舱物品、法律状态、社区仪式和关系值仍由对应 owner 保存。
- A02 允许撤退、投降、释放、救援和非歼灭脱离，不把战斗结果强制转成全歼、掠夺或死亡。
- 后续 D14-A03 可读取案件和战斗事实接入特殊武器/模块；D14-A04/A05/A06 可消费战利品、投降、救援和法律/历史标签，但不得复写 A02 或各所属系统状态。
