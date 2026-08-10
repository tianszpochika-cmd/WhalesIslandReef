# D12-A04 任务交接报告

日期：2026-08-11  
任务：D12-A04 部落/地方社群交易

## 交付结论

已完成两个制度不同的代表性社群交易切片。社群拥有独立库存、语言/知识门槛、礼仪、土地边界、保护区和长期声望；物物交换不是 D3 商店买卖。Agent 只能生成 `D12CommunityTradeIntent`，最终结果由 `D12CommunityTradeSystem` 裁决。

## 事件字段、所有权与结果

事件使用 D8 `WorldEvent/FactEvent` 字段：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`save_revision`，并在失败/拒绝时写 `outcome_status=failed` 与 `recovery_route`。

| 数据 | 唯一写者 |
|---|---|
| 社群定义的社会制度、语言/知识/礼仪门槛、敏感标签/审查状态 | D12 内容目录（不可变定义） |
| 社群库存、物物交换、帮助/拒绝/误解结果 | `D12CommunityTradeSystem` |
| 土地边界/保护区定义 | `PropertyIslandSystem`；A04 只读并执行访问检查 |
| 玩家货物数量和容量 | `D3InventorySystem` |
| 长期声望、信任、知识、礼仪熟悉度、历史解释 | `D12CommunityTradeSystem` |
| 事实日志和存档投影 | `EventLog` / `SaveSystem` |

## 重复保护与存档

交易/帮助事件使用 `event_id + save_revision`，社群库存保留 `applied_event_keys`；D12 社群快照同时保存库存、声望历史和 `EventLogSnapshot`。读档后相同物物交换只返回 `duplicate_settlement`，不会再次修改玩家货物、社群库存或长期声望。

`D12RuntimeSnapshot.CommunityTrade` 已接入 `SaveSystem` 的 `location_json.d12.communityTrade`；缺失字段按空快照迁移。

## 新接口

- `D12CommunityTradeSystem.ProposeTrade`：只生成 Agent/玩家意图副本；
- `D12CommunityTradeSystem.SettleTrade`：边界、审查、语言、知识、礼仪、库存和货物容量裁决；
- `D12CommunityTradeSystem.RequestHelp`：写帮助事实并提升知识/礼仪熟悉度；
- `D12CommunityTradeSystem.GetCommunityReadModel/GetReputation`：只读快照；
- `D12CommunityTradeCatalog.CreateVerticalSlice`：Green Cay 与 Free Cove 社群样例；
- `D12CommunityTradeSnapshot`：社群库存、声望历史、修订号和事实日志投影。

## 测试命令与结果

```powershell
dotnet run --project tools/d12_a04_smoke/D12A04Smoke.csproj
```

结果：`D12-A04 smoke PASS: proposal/help/barter/boundary/save`。

已登记 Unity EditMode：`D12CommunityTradeSystemTests`，覆盖提案无写权、语言、知识、礼仪、帮助、保护区、土地边界、缺货、未知社群、因果声望、SaveSystem 读档和重复物物交换。

前置门禁：`python tools/d9_s02_acceptance_check.py` 应保持 `D9-S02 PASS: blocked=0`；本轮尚未改变 D9 领域代码。

## 已知边界

- 当前仓库全量构建受并行 D12-A06 的未完成 Tarot 接线阻断：`Whalebone.Systems` 首个错误为 `D12MapDiscoveryContracts.cs(271)` 缺少 `D12TarotRuntimeSnapshot` 项目引用，`Save.Tests` 当前错误为 `SaveSystem.cs` 缺少 `D12TarotToObj/D12TarotFromObj`；A04 smoke 已用独立纯 C# 项目验证核心逻辑，A04 未修改 D12-A06 文件或其项目接线。
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry` 当前有并行 D12-A07 的 6 项历史资产/特殊武器 owner、test_id 和 orphan-reference 错误；A04 没有新增 ContentRegistry 条目。
- 土地正式产权、租约、许可证和争议由后续 Property/Legal 系统接管；A04 只消费边界定义。
- 本切片未扩展 UI 专用面板和 Agent 表达文本；展示层应读取原因载荷和快照，禁止写库存/声望/交换结果。
- 仅使用两个虚构社群样例和 D3 十商品；完整社群内容、来源授权和历史审查需继续走 D8 准入。
