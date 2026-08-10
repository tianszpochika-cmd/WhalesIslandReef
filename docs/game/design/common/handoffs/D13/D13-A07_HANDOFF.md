# D13-A07 交接报告：动态市场与运输

任务编号：`D13-A07`  
日期：2026-08-11  
状态：完成，专项测试通过

## 主成果

- 契约：`game/Assets/Scripts/Systems/D13/D13DynamicMarketContracts.cs`
- 商品/路线目录：`game/Assets/Scripts/Systems/D13/D13DynamicMarketCatalog.cs`
- 编排与运输状态：`game/Assets/Scripts/Systems/D13/D13DynamicMarketTransportSystem.cs`
- D3 适配器：`game/Assets/Scripts/Systems/D13/D13DynamicMarketAdapters.cs`
- Economy 市场流入口：`game/Assets/Scripts/Systems/D3/TradeEconomySystem.cs`
- 测试：`game/Assets/Tests/EditMode/D13DynamicMarketSystemTests.cs`
- 测试 runner：`tools/d13_a07_runner/`
- 存档接线：`PlayerState.D13DynamicMarketRuntime`、`GameState.CurrentD13DynamicMarketRuntime`、`SaveSystem.SaveD13DynamicMarketRuntime/LoadD13DynamicMarketRuntime`、`location_json.d13DynamicMarket`
- 设计文档：`docs/game/design/common/D13_DYNAMIC_MARKET_TRANSPORT.md`

## 新接口与边界

`D13DynamicMarketTransportSystem` 对外提供 `Quote`、`Produce`、`Consume`、`Sell`、`CreateShipment`、`AdvanceShipment`、`ArriveShipment`、`GetReadModel`、`CaptureSnapshot` 和 `Restore`。接入真实系统时注入：

- `ID13MarketLegalAuthority`
- `ID13MarketProductionAuthority`
- `ID13MarketConsumptionAuthority`
- `ID13MarketInventoryAuthority`
- `ID13MarketEconomyAuthority`
- `ID13TransportAssetAuthority`

适配规则：

1. `D13D3InventoryAuthority` 只调用 `D3InventorySystem.TryAddCargo/TryRemoveCargo`，不保留第二份货舱数量。
2. `D13TradeEconomyAuthority` 只调用 `TradeEconomySystem.Quote/ApplyExternalMarketFlow`；价格、市场库存、需求和交易量仍由 EconomySystem 写入。
3. `D13DynamicMarketTransportSystem` 只写商品流历史、运输状态和运输损耗；生产、消费、销售的领域结果必须有所属系统来源。
4. 许可、特殊物品、毒物和路线法律状态必须由 `FactionLegalSystem` 返回；Agent/UI 不能改变许可或市场结果。

## 验证结果

- `D13-A07 runner`：3/3 PASS；
- `D13-A06 runner`：3/3 PASS 回归；
- `D13-A03 runner`：3/3 PASS 回归；
- `dotnet build game/Whalebone.Systems.csproj --no-restore`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore`：0 warning / 0 error；
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：109 entries / 13 prefixes PASS；
- D13 A07 Systems 目录网络/LLM 红线：无命中。

## 接入注意

1. 所有操作都必须提供稳定 `OperationId` 和 `SaveRevision`；重试同一 `event_id + save_revision` 返回 `Duplicate`。
2. 运输推进必须带天气事实、生态压力事实和 Ship/Property 资产事实；缺失时失败并返回 `route_market_transport_review`。
3. 货物到达前不直接写目的地数量；到达只请求 Inventory/Cargo 入库，入库拒绝则运输状态为 `Held`。
4. 生产失败、市场失败、法律拒绝和运输失败均写统一事实并提供恢复路线；失败补偿不能由 UI 或 Agent 自行执行。
5. 读模型只供 UI/Agent 读取；Agent 可解释报价原因或建议路线，但不能裁决生产、消费、许可、运输损耗、价格、债务或奖励。

## 遗留

无 A07 阻塞遗留。后续可增加港口/路线/商品配置，但必须继续由 D3 Economy、D3 Inventory、运输、法律、生态和所属生产/消费系统保持唯一写者。
