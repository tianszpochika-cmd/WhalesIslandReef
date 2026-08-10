# D13-S01 集成验收报告

## 结论

**D13 通过。** 允许 D14 使用 D13 的经济与资产数据，但只能通过各系统只读快照、统一 EventLog 事实和带来源的结果；D14 不得直接写产权、金融账户/债务/保险、作物、建筑、库存、价格或运输状态。

## 验收流程与证据

`D13AcceptanceIntegrationTests.PropertyWellFarmMarketLoanInsuranceTaxAndSaveRestoreAreConsistent` 串行完成以下流程：

1. `PropertySystem` 调查并租赁 `property_farmland_green_cay_cassava`，调查/交易引用 `legal_source` 和 `economy_source`；调查并估值后购买圣玛丽仓库，并由 `FinanceSystem.Spend` 结算产权款。
2. `IslandGovernanceSystem` 迁入居民、分配 `JobDockhand`、应用税率政策、完成食物/供水日推进；工资和税费结果由治理系统保存，再分别由 FinanceSystem 支出。
3. `BuildingSystem` 消耗 Inventory/Cargo 材料，完成 `building_green_cay_water_well` 和 `building_santa_marys_north_warehouse`；水井进入 `Operational` 后才放行农地经营资格，水资源/土壤状态仍由实际 `D4EcologySystem` 写入。
4. `FarmingSystem` 播种木薯、浇水、分配劳力、推进成熟并收获；收成进入 D3 Inventory，收获导致 Ecology 压力增加。
5. `D13DynamicMarketTransportSystem.Sell` 调用 D3 Inventory/Cargo 出库和 `TradeEconomySystem` 价格/市场写入；销售所得由 `FinanceSystem.Receive` 入账，生产/销售事实保留收获因果和 Economy 来源。
6. `FinanceSystem` 以仓库估值和法律来源建立资产抵押贷款，生成本金、利息、到期日、最坏结果说明；购买风暴保险，登记事故与清单证据，提交风暴理赔并得到可追踪赔付。

## 唯一写者检查

| 状态 | 唯一写者 | 集成检查 |
|---|---|---|
| 产权/租赁/估值引用 | PropertySystem；法律与 Economy authority 裁决来源 | 读档后所有者、租户、法律来源保持一致 |
| 账户/贷款/保险/赔付 | FinanceSystem | 现金由 FinanceSystem 结算，贷款与理赔不改 PropertyState |
| 建筑/水井/工期 | BuildingSystem | 水井和仓库运营状态、地图状态可恢复 |
| 作物/收获 | FarmingSystem | 收获后作物清空、收成仅由 D3 Inventory 写入 |
| 土壤/水源/生态压力 | EcologySystem | 实际 Ecology 快照恢复，收获压力保留 |
| 居民/岗位/税/工资 | IslandGovernanceSystem；金额由 Economy authority | 居民健康、供给、工资、税务记录保留 |
| 库存/市场价格/市场状态 | D3 Inventory/Cargo；TradeEconomySystem | 市场销售不在 D13 重写库存或价格 |
| 运输 | D13MarketTransportSystem | 运输状态/损耗由动态市场系统保存 |

## 保存、恢复与幂等

保存了 D3 快照（含共享 EventLog）以及 D13 产权、金融、农业/Ecology、治理、建筑、动态市场快照。读档后验证了：事件数、库存数量、市场流、居民/税务、建筑运营、产权归属、金融债务/保险理赔和 Ecology 环境数量均与保存前一致。

用原始 `OperationId + SaveRevision` 重放销售、抵押贷款和水井建设订单，三个系统均返回 `Duplicate`，共享 EventLog 数量不增加，库存、现金、债务和建筑状态不重复结算。

## 测试门

- D13-S01 集成 runner：1/1 PASS。
- D13-A03 回归：3/3 PASS。
- D13-A06 回归：3/3 PASS。
- D13-A07 回归：3/3 PASS。
- `dotnet build game/Whalebone.Systems.csproj --no-restore`：0 warning / 0 error。
- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error。
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：110 entries / 13 prefixes PASS。
- D13-S01 触及代码红线：无网络导入、URL 或 LLM 裁决路径。

## 遗留问题

无阻塞遗留。水井是本次同步集成补充的单一代表建筑定义；它只证明建设状态到生态水源接入的跨系统因果，不引入水量镜像。更大规模建筑分布、真实 Economy/Legal/治理适配器和客户端只读投影属于后续内容接入范围。

法律、宗教、历史和共同体相关内容继续使用已有标签、来源和审查状态；集成测试中的治理 fixture 明确标记 `ApprovedWithWarning`，未跳过敏感内容审查，也没有把 Agent 放入任何裁决路径。
