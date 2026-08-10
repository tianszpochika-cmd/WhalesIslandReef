# D9-A03 任务交接报告

任务：`D9-A03`  
日期：2026-08-10  
前置：`D9-S01` 已完成  
状态：完成，等待 D9-S02 统一事件运行时接线

## 交付文件

- 贸易裁决：`game/Assets/Scripts/Systems/D3/CommodityCatalog.cs`、`D3Contracts.cs`、`D3InventorySystem.cs`、`TradeEconomySystem.cs`
- D3 运行时与保存适配：`game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs`、`game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- 测试：`game/Assets/Tests/EditMode/D9TradeVerticalSliceTests.cs`、`D9TradeSaveSystemTests.cs`
- 说明：`docs/game/design/common/D9_TRADE_VERTICAL_SLICE.md`

## 事件字段与所有权

交易事实使用 D8 `WorldEvent` / `FactEvent`，事件 ID 为 `event_d9_trade_settled`。每笔事实包含：

| 字段 | 本切片值/来源 | 所有权 |
|---|---|---|
| `event_id` | 固定 `event_d9_trade_settled` | Economy 生成，EventLog 追加 |
| `schema_version` | D8 当前版本 `1` | EventLog 校验 |
| `world_day` | 当前 D3 交易入口的世界日，默认安全值 `0` | 环境/运行时提供 |
| `location_ref` | 买卖港口 ID | Economy 调用方提供 |
| `actor_ids` | `player` | Economy 调用方提供 |
| `input_facts` | `commodity:<commodity_id>` | Economy 提供 |
| `owner_system` | `EconomySystem` | Economy 声明 |
| `result` | 交易码、摘要、商品、数量、单价、总价、税额等 opaque values | Economy 写，EventLog 只保存 |
| `cause_ids` | 当前交易无前置因果时为空列表 | Economy 提供 |
| `save_revision` | Economy 生成并递增的交易修订号 | Save/World revision 边界；EventLog 用于去重 |
| `log_sequence` | 追加序号 | EventLog 生成 |

商品字段在 `CommodityDefinition`，市场状态在 `D3MarketState`，货物数量只在 `D3InventoryState` 并由 `D3InventorySystem` 写入；UI、任务脚本、AgentReply 没有商品字段副本或价格写入口。

## 重复结算保护

- 成功买卖先由 Economy 完成 canonical 数值裁决，再追加 `FactEvent`。
- D8 `EventLog` 以 `event_id + save_revision` 拒绝重复事实。
- 带明确 `save_revision` 的 `Buy`/`Sell` 在修改状态前查找既有事实；重放返回 `duplicate_settlement`，不重复扣钱、加钱、加货或回补市场。
- `D3RuntimeSnapshot.EventLog` 保存事实、日志序号和 settled keys；读档由 `TradeEconomySystem.RestoreEventLog` 恢复下一交易修订号。

## 测试命令与结果

```text
临时 source-only NUnit harness（执行后已清理 `Temp/D9A03Verify`）：
dotnet run --project Temp/D9A03Verify/D9A03Verify.csproj
  PASS 6/6：十商品、重量/体积、价格原因、十商品买卖、缺货、快照恢复/重放保护

dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal
  未通过：工作区既有 AgentCore 编译错误 AgentModels.cs(123,124) 缺少 CrewDuty；未命中 D9-A03 文件。

dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal
  未通过：同一既有 CrewDuty 错误；本任务贸易文件未报告新增错误。

git diff --check
  通过，无 whitespace error。
```

Unity EditMode 的 `D9TradeSaveSystemTests` 已加入，验证 SQLite 存档中的 D3 市场驱动和 EventLogSnapshot；主工程当前有并行 Unity 实例/既有 Agent 失败，未强行关闭或修改无关模块。

## 已知边界

- `world_day` 仍由 D3 运行时日历提供；当前交易 API 没有自行推进世界日。
- 价格原因是 Economy 输出的可读快照，UI 仍需由 D9-A07/最终表现接线选择本地化文案；UI 不得回写原因或价格。
- D9-A02 鱼获通过已有可扩展 `D3MarketItemDefinition` 注册，不计入冻结十商品；本任务没有增加第 11 个基础商品。
- D9-S02 仍需把所有 D9 owner 的同一 EventLog 和 SaveSystem 恢复入口统一起来，并补跑完整 14 事件链；本交付只负责贸易 owner 和其 D3 存档投影。
