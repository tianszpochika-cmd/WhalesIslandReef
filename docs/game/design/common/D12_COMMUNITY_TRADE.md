# D12-A04 部落/地方社群交易

任务：`D12-A04`  
版本：`d12.community.trade.v1`  
状态：代表性垂直切片已实现

## 1. 设计结论

D12 社群不是 D3 港口市场的换皮。每个社群有自己的社会制度、交换礼仪、语言和知识门槛、库存边界、土地/保护区边界以及玩家专属的长期声望历史。物物交换只有在这些条件通过后才由 `D12CommunityTradeSystem` 裁决。

代表样例：

| 社群 | 社会制度 | 语言/知识门槛 | 礼仪 | 交换地点 | 保护区 |
|---|---|---:|---|---|---|
| `community_green_cay` | 自治渔猎社群：互惠、长老见证、保护区规约 | 2 / 2 | 先赠礼再交换 | `green_cay_trade_cove` | `green_cay_mangrove_reserve` |
| `community_free_cove` | 自由社群：互助承诺、避难规约、公开见证 | 1 / 1 | 正式问候 | `free_cove_exchange_quay` | `free_cove_wetland_reserve` |

目录定义是不可变配置；运行时库存、声望和事实事件分开保存。

## 2. 所有权

| 数据 | 唯一写者 | 其他系统权限 |
|---|---|---|
| 社群库存、互换结果、礼仪/知识/语言/边界裁决、帮助/拒绝结果 | `D12CommunityTradeSystem` | UI/Agent 只能读取快照或提交意图 |
| 玩家货物数量、重量和货舱 | `D3InventorySystem` | 社群系统只能请求添加/移除，不能改字典 |
| 土地边界/保护区定义 | `PropertyIslandSystem`（本切片读取静态边界定义） | `D12CommunityTradeSystem` 只执行访问检查，不改变边界 |
| 长期声望、信任、知识、礼仪熟悉度及变化历史 | `D12CommunityTradeSystem` | Relation/Faction 不能复制一份社群声望 |
| 事实事件追加与保存投影 | `EventLog` / `SaveSystem` | 只追加/序列化，不裁决库存或声望 |

## 3. Agent、UI 和交易链

```text
Agent/玩家表达
  → D12CommunityTradeIntent（只含物物交换意图和语言/礼仪输入）
  → CommunityTradeSystem 边界、内容审查、语言、知识、礼仪检查
  → EventLog 先追加已裁决事实
  → CommunityTradeSystem 写社群库存/长期声望
  → D3InventorySystem 写玩家货物
  → UI/Quest/Agent 读取事实和只读快照
```

`ProposeTrade` 只返回意图副本，不写 EventLog、库存或声望。`SettleTrade` 才是物物交换的裁决入口；Agent 不携带价格、金钱、最终库存、最终声望或奖励结果。

## 4. 结果规则

| 条件 | 结果 | 事实/恢复 |
|---|---|---|
| 语言 ID/等级不足 | `misunderstood_language`，理解失败 | `recovery_request_translation`；记录误解并降低信任 |
| 知识等级不足 | `knowledge_required`，社群拒绝推进 | `recovery_learn_community_knowledge` |
| 礼仪不匹配 | `etiquette_mismatch`，误解/暂缓 | `recovery_learn_etiquette`；记录原因和信任变化 |
| 非授权土地边界 | `land_boundary_refused` | `recovery_move_to_trade_boundary` |
| 保护区无明确同意 | `protected_area_refused` | `recovery_request_protected_area_consent` |
| 社群库存不足 | `community_out_of_stock` | `recovery_wait_for_restock`；不扣玩家物资 |
| 玩家供物不足/货舱不足 | `offer_missing` / `player_cargo_full` | `recovery_return_with_offer` / `recovery_make_cargo_space` |
| 请求帮助 | `community_help_offered` | 增加文化知识和礼仪熟悉度，不生成货物 |
| 全部条件满足 | `community_trade_completed` | 社群库存收进供物、交出需求物；玩家货物由 D3 写入 |

帮助、拒绝、误解和成功交换均进入事实日志，因此面板可以解释“为什么这次没有成交”以及长期声望如何变化。

## 5. 长期声望

每个 `actor_ref + community_id` 独立保存：

- `long_term_reputation`：-100..100；反映长期互惠/越界记录；
- `trust`：0..100；反映社群当前愿意承担交换风险的程度；
- `cultural_knowledge`：0..100；帮助和已验证社群知识提升；
- `etiquette_familiarity`：0..100；成功遵守礼仪或接受帮助提升；
- `misunderstanding_count`、`help_received_count`、`refusal_count`、`successful_trade_count`；
- `history[]`：每次变化的 `event_id`、`cause_event_ids`、世界日、原因码、增量、变化后值和解释。

成功互惠样例为声望 `+5`、信任 `+4`；帮助为声望 `+1`、信任 `+2`，具体结果仍由系统固定规则裁决。语言/礼仪误解和越界尝试会留下负面原因；保护区拒绝本身不是“库存缺货”，两者使用不同结果码和恢复路线。

## 6. 事件与重复保护

交易事件使用 D8 字段：`event_id`、`schema_version`、`world_day`、`location_ref`、`actor_ids`、`input_facts`、`owner_system`、`result`、`cause_ids`、`save_revision`，并使用 `outcome_status`/`recovery_route` 表达失败和恢复。

事件 owner 固定为 `CommunityTradeSystem`，结果载荷包含社群 ID、交换物品、礼仪/制度标签、声望/信任增量和原因码；事件层不写任何库存或声望。

重复键固定为 `event_id + save_revision`。社群库存同时保留 `applied_event_keys`，D12 社群快照保存 `EventLogSnapshot`。读档后重建系统并重放相同意图，只返回 `duplicate_settlement`，不会再次扣除玩家供物、重复交付需求物或重复增加声望。

## 7. 敏感内容

每个社群和土地/保护区定义都包含：`historical_tag`、`sensitive_content_label`、`content_warning`（社群）和 `content_review_status`。本切片样例使用 `fictional + ApprovedWithWarning`，明确标注殖民接触、土地权利、自由社群/强迫劳动背景和保护区语境；它们不是对现实社群的历史断言，也不把身份处境设计为套利按钮。

## 8. 存档和测试

`D12RuntimeSnapshot` 新增 `CommunityTrade`，由 `SaveSystem.SaveD12Runtime/LoadD12Runtime` 写入既有 `location_json.d12.communityTrade`。旧存档缺少该字段时迁移为空社群快照，不猜测声望或库存。

测试入口：`D12CommunityTradeSystemTests`，覆盖提案无写权、语言/知识/礼仪分支、帮助、土地边界、保护区、库存缺货、未知社群、长期声望因果、SaveSystem 读档和重复物物交换。

定向运行：

```text
dotnet run --project tools/d12_a04_smoke/D12A04Smoke.csproj
```

## 9. 当前边界

- 本切片使用两个代表性虚构社群和 D3 十商品作为交换物，不宣称完整内容规模；新社群必须经过 D8 内容来源/授权/审查/`test_id` 准入。
- 土地/保护区的正式产权、租约、许可证和争议账本由后续 Property/Legal 系统接管；A04 只消费边界定义并记录拒绝事实。
- UI 面板、Agent 语言表达和 Quest 摘要尚未扩展专用展示层，但已有只读快照、事实和原因载荷。
- `Systems/` 未加入网络或 LLM；本系统是确定性裁决层。
