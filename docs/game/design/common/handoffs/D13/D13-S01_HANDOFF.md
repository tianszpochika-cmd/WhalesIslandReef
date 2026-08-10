# D13-S01 交接报告

## 已完成

- 新增 `D13AcceptanceIntegrationTests` 和 `tools/d13_s01_runner`，覆盖产权租赁/购买、治理居民/岗位/税/工资、建筑水井/仓库、农业收获、动态市场销售、资产贷款、风暴保险理赔和 SaveSystem 恢复。
- 增加 `building_green_cay_water_well` 目录/定义；BuildingSystem 写建设状态，EcologySystem 继续写水源、土壤和生态量。农业资格适配器要求水井已运营，建立确定性的“建井→种植”因果。
- 共享 D3 EventLog 作为跨域事实来源；各领域快照由既有 `SaveSystem.SaveD13*Runtime` 单独保存，不复制任何领域状态。
- 读档后用相同操作键重放销售、贷款、建设订单，均返回 Duplicate，未重复添加事件、库存、现金、债务或建筑。

## 主要接口边界

- `PropertySystem`：只读产权和法律来源，所有权/租赁仍由 PropertySystem 写入。
- `BuildingSystem`：只写建筑、订单、工期、损伤和运营状态；水井服务不写水量。
- `FarmingSystem`：只写作物状态和收获事实；D3 Inventory 写种子/收成；D4 Ecology 写环境。
- `D13DynamicMarketTransportSystem`：编排市场流/运输状态；D3 Inventory 和 TradeEconomySystem 分别写库存与市场。
- `FinanceSystem`：唯一写现金、贷款、利息、保险和赔付；产权抵押 authority 只返回估值/法律来源。
- `IslandGovernanceSystem`：唯一写人口、岗位、健康、满意度、税务和工资记录；FinanceSystem 负责实际现金变动。
- UI/Agent 仅提交请求或读取快照/事件；没有在 Systems 中加入网络或 LLM。

## 验证

- `dotnet build game/Whalebone.Systems.csproj --no-restore`：PASS。
- `dotnet build game/Save.Tests.csproj --no-restore`：PASS。
- `dotnet run --project tools/d13_s01_runner/D13S01Runner.csproj --no-build`：`D13-S01 tests PASS count=1`。
- A03/A06/A07 回归分别 3/3 PASS。
- ContentRegistry：110 entries / 13 prefixes PASS。

## D14 使用许可

**允许。** D14 可以读取 D13 资产产权、金融风险、农业/生态、建筑、治理和动态市场的快照与 EventLog 事实，用于提议或显示；不得绕过所属系统直接修改这些状态。
