# D13-A06 交接报告：建筑、工坊、仓库与船坞

任务编号：`D13-A06`  
日期：2026-08-11  
状态：完成，测试通过

## 主成果

- 契约：`game/Assets/Scripts/Systems/D13/D13BuildingContracts.cs`
- 目录：`game/Assets/Scripts/Systems/D13/D13BuildingCatalog.cs`
- 系统：`game/Assets/Scripts/Systems/D13/D13BuildingSystem.cs`
- 测试：`game/Assets/Tests/EditMode/D13BuildingSystemTests.cs`
- 测试 runner：`tools/d13_a06_runner/`
- 存档接线：`PlayerState.D13BuildingRuntime`、`SaveSystem.SaveD13BuildingRuntime/LoadD13BuildingRuntime`、`location_json.d13Buildings`
- 内容目录：仓库、漂木岛绳作工坊、圣玛丽船坞及 `island_driftwood_isle`
- 设计文档：`docs/game/design/common/D13_BUILDINGS_WORKSHOPS_SHIPYARDS.md`

## 新接口与边界

`D13BuildingSystem` 对外提供建设下单、逐日推进、服务、仓库存入、火灾、灭火、占领、维护、读模型、快照和恢复入口。接入真实运行时需要注入：

- `ID13BuildingPropertyAuthority`
- `ID13BuildingWorkforceAuthority`
- `ID13BuildingInventoryAuthority`
- `ID13BuildingEconomyAuthority`
- `ID13BuildingOccupationAuthority`
- `ID13BuildingShipAuthority`

这些 authority 只能返回带 `SourceEventId` 和 owner 的决定；BuildingSystem 会拒绝缺少共享事实的结果。Economy 的价格只进入历史服务收据，Inventory/Cargo 的材料和产出只进入来源/摘要，ShipSystem 只写船体，均未在 A06 复制。

## 验证结果

- `D13-A06 runner`：3/3 PASS；
- `dotnet build game/Whalebone.Systems.csproj --no-restore --nologo`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore --nologo`：0 warning / 0 error；
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：109 entries / 13 prefixes PASS；
- D13 A06 系统目录网络/LLM 红线：无命中。

## 接入注意

1. UI 只提交 request，读取 `D13BuildingReadModel` 和 EventLog；不得从 UI 直接完成建设、扣料、改价、生成产出或清损。
2. 每个操作必须提供稳定 `OperationId` 和 `SaveRevision`；重试同一键会返回 `Duplicate`。
3. 服务价格、材料、工人、产权、法律/占领和船舶结果必须先在共享 EventLog 中存在来源事实。
4. 火灾扑灭不等于维修；灭火后要通过维护服务恢复损伤。占领状态会关闭服务，释放后仍需满足损伤和开放时间规则。
5. 生产、仓储和维修材料的真实库存写入应由 D3 Inventory/Cargo 适配器完成；本测试使用带来源的 authority fixture。

## 遗留

无 A06 阻塞遗留。样例规模和外部 authority 适配器是后续内容/集成工作范围，不是本模块的第二写者。D4 旧船坞投影保持兼容，D13 代码不调用或改写其状态。
