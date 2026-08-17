# D19-L11 交接报告

## 任务信息与结论

- 任务编号：`D19-L11`（波次 1，并行；精确前置 D19-L17/L18/L23 均 LOGIC_PASS 已核验；D3 真实公开市场命令经 D19-L01 盘点确认存在：TradeEconomySystem.Buy/Sell/Quote + 幂等 revision 入口）
- 日期：2026-08-17
- 结论：**LOGIC_PASS**
- 机器结果：`game/tools/results/D19/D19-L11/D19_L11_LOGIC_RESULT.json`

## 实际修改文件（唯一写入范围）

- `game/Assets/Scripts/Systems/D19/D19MarketPortAdapter.cs`（新建，纯 C# 适配器）
- `game/tools/results/D19/D19-L11/D19_L11_LOGIC_RESULT.json`（新建）
- `docs/game/design/common/handoffs/D19/D19-L11_LOGIC_HANDOFF.md`（本文件）
- `.workbuddy/memory/2026-08-17.md`（追加当日记忆）

未修改 D3、库存/货币所有者、Save、UI 或其它 D19 文件；未启动 Player；未自建货币/库存模型。

## 冻结语义

### 执行管线（确定性）

1. **输入校验**（接口不匹配 → Blocked，未调用 D3）：null 请求/D3TradeState/TradeEconomySystem、缺 command_id/commodity_id、quantity<=0、serviceId≠market。
2. **权限/服务前置（L23）**：`D19PortAuthorityValidator.Validate(StartService 上下文, claims)`——跨船/跨玩家/跨港/未知港泊位服务拒绝。注意：L23 的 ServiceId 期望 **L04 锚点目录键**（`service_<port>_market`），适配器按已解析端口构造后传入；请求方仍使用 L18 裸 ID（market）。
3. **可用性门（L05）**：注入的 `AvailabilityGate`（非 Available → 阻断，ReasonCode/LocKey/Retry 取自门结果）；未注入时执行市场登记最小前置（服务登记于该港）。
4. **D3 公开调用**：`economy.Buy/Sell(state, d3PortId, commodityId, quantity, priceContext, saveRevision)` 幂等入口；端口键 `port_X → X`（D3 裸键）；价格上下文缺省按 D3 自身默认口径从市场状态只读构建；**活 state 只透传，不缓存**。
5. **投影**：D3 成功 → Success（DuplicateSettlement 透传幂等）；D3 失败 → L17 投影表（见下）+ L18 key + L16 事件草案（ServiceResult，result_code `port.market.buy_*`/`sell_*`，summary `d19.port.event.service_result`，values 全为安全字符串）。

### D3→L17 投影表（冻结，OriginalD3Code 永远保留）

| D3 码 | L17 码 | 说明 |
|---|---|---|
| not_enough_money | d19.trade.funds_insufficient | 货币裁决 |
| market_empty | d19.service.unavailable | 近似：缺货=服务无法履约 |
| weight_full / cargo_full | d19.inventory.capacity_exceeded | 库存裁决 |
| invalid_reference | d19.service.unavailable | 近似：引用无法被市场服务满足 |
| invalid_quantity | d19.inventory.operation_failed | 近似：数量非法 |
| invalid_state | d19.service.unavailable | 状态缺失 |
| invalid_revision | d19.save.conflict | 修订号冲突 |
| duplicate_settlement | d19.save.conflict | 幂等去重（Succeeded=true 透传） |

未登记 D3 码 → Blocked（`d3_code_unmapped:<code>`），绝不静默。

### 输出（L22/L20 可消费）

`D19MarketTradeResult`：Success/DuplicateSettlement/CommandId/ReasonCode（L17）/LocKey（L18）/RetryPolicy（L17 条目）/CommodityId/Quantity/TotalPrice/MoneyAfter/**SaveRevision（D3 透传）**/OriginalD3Code/EventDraft（L16 工厂构造，未追加）/Note。

## 验证结果（全部真实执行）

| 验证 | 结果 |
|---|---|
| 临时 dotnet 工程编译（链接真实 D19 八文件 + D3 四文件 + Event 契约 + AgentCore 配置） | 0 warning 0 error |
| 33 项确定性断言 | 33/33 PASS（成功买入/卖出、幂等重放不双花、L17 投影+原始码保留、权限/可用性门、接口不匹配阻断且钱不变、事件草案可解析、无隐藏状态） |
| `git diff --check` | PASS（仅既有 LF→CRLF 警告） |
| `python tools/sync_logic_task_status.py` | PASS：D19-L11=LOGIC_PASS |

开发期真实修复：L23 权威校验期望 L04 目录服务键（`service_<port>_market`）而请求方传 L18 裸 ID（market）→ 适配器按已解析端口构造目录键，复验全过；未修改 L23/L04/L18 文件。

## 明确未宣称

- 没有可绕过 D3 的买卖路径：所有交易必经 TradeEconomySystem 公开入口（harness 以钱/库存真实变化与阻断路径零变化验证）。
- 价格、货币、库存一切数值仍由 D3 独占裁决；适配器无缓存、无写路径。
- 事件草案未追加（追加/去重由 EventLog 与 L22 路由负责）。

## 待办 / 后续注意事项

- L33 命令路由注入真实 command_id/claims/AvailabilityGate；L20 消费 SaveRevision 投影；L22 以 CommandId+revision 幂等。
- L12（仓库）/L13（酒馆）/L14（船坞）/L15（补给）适配器按本文件结构与 L17/L18 接口对齐。
- 若 L23 后续放宽 ServiceId 语义，本适配器目录键构造需同步（L45 一致性核对）。
