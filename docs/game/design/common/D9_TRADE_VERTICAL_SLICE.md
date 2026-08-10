# D9-A03 十商品贸易切片

任务：`D9-A03`  
版本：`d9.trade.vertical.v1`  
状态：已实现，可交给 D9-S02 做全链路接线

## 1. 范围

本切片复用 D9-S01 冻结的 `CommodityCatalog.All` 十种商品：

| ID | 名称 | 单位重量 | 单位体积 | 需求目标 | 基础价 |
|---|---|---:|---:|---:|---:|
| `salt` | 盐 | 1 | 1 | 60 | 4 |
| `sugar` | 糖 | 1 | 1 | 70 | 6 |
| `tobacco` | 烟草 | 1 | 1 | 45 | 12 |
| `rum` | 朗姆酒 | 1 | 1 | 40 | 18 |
| `coffee` | 咖啡 | 1 | 1 | 55 | 14 |
| `cocoa` | 可可 | 1 | 1 | 50 | 16 |
| `spices` | 香料 | 1 | 1 | 35 | 22 |
| `dyes` | 染料 | 1 | 1 | 30 | 20 |
| `timber` | 木材 | 2 | 2 | 65 | 8 |
| `medicine` | 药材 | 1 | 1 | 25 | 28 |

商品定义只在 `CommodityCatalog`；D9-A02 已注册的鱼获作为可扩展市场定义保留，但不计入本十商品冻结范围。

## 2. 数据与所有权

| 数据 | 唯一写者 | 读取者 | 说明 |
|---|---|---|---|
| 商品 ID、名称、基础价、单位重量、单位体积、需求目标 | `CommodityCatalog` | Inventory、Economy、UI | 不在 UI、任务或 AgentReply 复制 |
| 货物数量 | `D3InventorySystem` | Economy、D9 读模型、UI | `TryAddCargo` / `TryRemoveCargo` 是唯一数量修改入口 |
| 货舱体积与重量 | `D3InventorySystem` | UI、整备、交易结果 | 体积使用 `CargoVolume`，重量使用 `UnitWeight` |
| 市场库存、需求、玩家交易量、税费、价格 | `TradeEconomySystem` / 港口定义 | UI、交易结果、存档 | UI 只提交买卖意图和读取 `MarketQuote` |
| 势力政策和天气输入 | 所属世界/环境 owner | `TradeEconomySystem` | Economy 只读取因子，不裁决天气或势力关系 |
| 交易事实 | `EventLog` 追加；owner=`EconomySystem` | Quest、UI、Agent 摘要 | EventLog 不写钱、库存、价格或其他数值 |
| 交易事件快照 | `SaveSystem` 复制 | Economy/EventLog 恢复 | 与 D3 runtime snapshot 同一 `location_json.d3` |

## 3. 价格裁决与可解释原因

`TradeEconomySystem` 唯一生成 `MarketQuote`。买入和卖出均基于同一核心价：

```text
core = 基础价
      × 库存因子
      × 需求因子
      × 天气因子
      × 玩家交易量因子
      × 势力政策因子

买入单价 = round(core × (1 + 港口税率))
卖出单价 = round(core × (1 - 港口税率 × 0.5) × 0.75)
```

报价携带六条 `PriceReason`，每条包含 `kind`、`code`、`explanation` 和 `coefficient`：

- `inventory`：当前港口库存；库存越低，价格因子越高。
- `demand`：当前需求与商品需求目标的差异。
- `weather`：平静、信风、降雨、风暴或飓风因子。
- `player_trade_volume`：当前商品的玩家累计交易量。
- `faction_policy`：势力政策 ID 和政策系数。
- `tax`：港口买入/卖出税率。

缺货由 `MarketQuote.IsOutOfStock` 显示；买入返回 `market_empty`，不会改变金币、货物或市场状态。现有 D3 交易页只把报价原因拼接到显示文本，不拥有任何价格字段的写权限。

## 4. 买卖、库存和事实日志

- 买入先验证市场库存、金币、货舱体积和重量，再由 Economy 扣款、更新市场、调用 `D3InventorySystem.TryAddCargo`。
- 卖出先验证货物数量，再由 Inventory 移除货物，Economy 增加金币并回补市场库存。
- 成功交易追加 D8 `event_d9_trade_settled`，`owner_system=EconomySystem`；每笔交易使用 Economy 生成的 `save_revision`，事件日志的去重键仍是 `event_id + save_revision`。
- `TradeResult` 返回 `CommodityId`、`UnitPrice`、`TotalPrice`、`TaxAmount`、交易后金币、重量、体积、价格原因和事实日志状态。
- 失败交易不追加成功事实；重复的明确修订号返回 `duplicate_settlement`，不再次修改状态。

## 5. 保存与恢复

`D3RuntimeSnapshot` 增加市场需求、玩家交易量、势力政策、交易修订号和 `EventLogSnapshot`。`SaveSystem` 将其写入/读取 `location_json.d3`，事件事实包括所有 D8 字段、结果载荷、因果 ID、恢复路线（如有）、日志序号和已结算键。

读档后先恢复市场驱动数据和 `EventLog`，再由 Economy 恢复下一交易修订号。使用相同 `event_d9_trade_settled + save_revision` 重放时只返回重复结算结果，不重复扣钱或加货。

## 6. 测试

定向测试入口：`game/Assets/Tests/EditMode/D9TradeVerticalSliceTests.cs`、`D9TradeSaveSystemTests.cs`。

覆盖：十商品目录与定义字段、重量/体积双容量、价格六类原因、十商品买卖、缺货、事实追加、快照克隆/恢复和重复交易保护；Unity EditMode 的 SaveSystem 测试另外覆盖 SQLite 存档读档。

纯 C# 可执行验证：

```text
dotnet run --project Temp/D9A03Verify/D9A03Verify.csproj
  D9-A03 tests: 6 passed, 0 failed
```

## 7. 边界

- Economy 不裁决天气、势力关系、奖励、伤害或资产；只读取它们提供给市场的输入因子。
- 事件层只保存已裁决事实，不成为第二个库存、金钱或价格写者。
- D9-A02 鱼获定义的注册适配继续存在，属于鱼获闭环，不扩张本十商品目录。
- 主 Unity 工程完整构建仍可能受工作区已有 Agent/Unity 编译与项目占用问题影响；本任务不修改无关 Agent、网络、LLM 或资产系统。
