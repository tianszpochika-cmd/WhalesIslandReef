# D9-A01 任务交接报告

任务：`D9-A01` 港口与船舶整备  
日期：2026-08-10  
前置：`D9-S01` 已完成  
状态：实现完成，交由 D9-S02 集成验收

## 主成果

- `game/Assets/Scripts/Systems/D9/D9PortShipPreparationContracts.cs`
- `game/Assets/Scripts/Systems/D9/D9PortShipPreparationSystem.cs`
- `game/Assets/Scripts/Systems/D9/D9CrewRecruitmentCatalog.cs`
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`
- `game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs`：新增经济系统唯一扣款入口 `TrySpend/CanSpend`
- `game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs`：新增 D9 提交/离港接线
- `game/Assets/Scripts/Gameplay/D4/D4RuntimeBootstrap.cs`：暴露既有 `D4ShipyardSystem` 只读入口
- `game/Assets/Tests/EditMode/D9PortShipPreparationTests.cs`
- `docs/game/design/common/D9_PORT_SHIP_PREP.md`

## 入口和流程

`D9PortShipPreparationBootstrap` 在 `InitialHarbor` 自动创建 `D9PortServicePage`。按钮/快捷键只提交以下意图：进入泊位、买盐、买木材、酒馆、补给、修船、招募艾萨克、发工资、船坞改装和离港。

离港采用两段式：D9 整备系统先验证并解除锚泊；D3 `RequestDeparture` 再生成正式航程并将 `D3VoyageStatus` 置为 `Sailing`。这样不会复制 D3 航程公式。

## 状态显示

整备快照覆盖重量、吃水、船员/上限、货物、补给、损伤/船体、锚泊/泊位、D4 改装和金币。页面只读取 `D9PortShipPreparationSnapshot`。

## 测试

本任务新增 EditMode 测试 4 项，覆盖：

1. 买货导致重量变化；
2. 修船同时消耗木材和金币；
3. 工资影响船员、招募进入编制；
4. 离港进入 `Sailing`。

计划执行命令：

```text
Unity 6000.0.80f1 -batchmode -nographics -projectPath game -runTests -testPlatform editmode -testFilter D9.Tests.D9PortShipPreparationTests -testResults <D9-A01-results.xml> -quit
python tools/d9_vertical_slice_check.py
python -m unittest tools.tests.test_d9_vertical_slice_freeze -v
dotnet build game/AgentCore/AgentCore.csproj --no-restore
git diff --check
```

实际结果：

- Unity 6 `D9.Tests.D9PortShipPreparationTests`：4/4 通过；
- Unity 6 EditMode 全量回归：93/93 通过；
- `python tools/d9_vertical_slice_check.py`：通过（3 scenes / 5 prefabs / 10 commodities / 14 events / 6 UI / 7 agents）；
- `python -m unittest tools.tests.test_d9_vertical_slice_freeze -v`：1/1 通过；
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore`：0 warning / 0 error；
- `git diff --check`：通过（仅已有 LF→CRLF 提示）。

## 边界说明

- 未新增跨端契约；D9-A01 是 Unity/Systems 内部整备入口，跨端状态继续由 D8 contracts 与既有 D3/D4/Save 载体承载。
- 未新增网络或 LLM；`Systems/D9` 和 `Gameplay/D9` 均没有网络客户端或模型调用。
- 未删除 D7 行为；复用 D7 的场景、控制和保存协调器。
- 修船材料固定复用 D3 已注册商品 `timber`，未新增 D8 内容 ID。
- 招募样例固定使用 D9-S01 冻结的 `crew_isaac_reed`。
