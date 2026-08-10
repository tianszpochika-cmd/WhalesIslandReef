# D14-A04 奖励系统

状态：已实现并通过纵切片验收。

## 1. 边界

`D14RewardSystem` 是 `RewardSystem` 的唯一奖励状态写者，负责：

- 奖励目录、稀有度和唯一性规则；
- 任务/探索/战斗/图鉴等系统提交的 pending 请求；
- 发放、领取、丢失、恢复状态；
- 来源、内容 ID、所属系统结算引用和审计记录；
- 与共享 `EventLog`、`SaveSystem` 的幂等接线。

它不写 `Inventory/Cargo`、现金、价格、图鉴、装备耐久或关系数值。请求、Agent 摘要和 UI 都不能携带最终数量、金额或物品结果；所属系统以已登记事实完成自己的 canonical 写入，RewardSystem 只保存引用。

## 2. 目录和稀有度

目录 ID 使用 D8 的 `reward_` 前缀，当前代表性条目覆盖五档：

| 稀有度 | ID | 内容引用 | 唯一 |
|---|---|---|---|
| Ordinary | `reward_d14_ordinary_supply` | `item_chart_compass` | 否 |
| Professional | `reward_d14_professional_rescue` | `item_tool_rescue_grapnel` | 否 |
| Rare | `reward_d14_rare_indigo_dye` | `item_dye_indigo` | 否 |
| Unique | `reward_d14_unique_moonwake_armory` | `item_special_weapon_moonwake_cutlass` | 是 |
| WorldClass | `reward_d14_worldclass_reinforced_keel` | `item_ship_module_reinforced_keel` | 是 |

目录只给出路由和内容引用，不给最终数量或金钱。每条定义带 `source_ref`、历史标签、审查状态、授权引用、允许来源 owner、恢复窗口和 `test_id`。

## 3. 处理流程

1. `QuestSystem` 或其他所属系统提交 `D14RewardRequest`；RewardSystem 验证 `SourceEventId + SourceSaveRevision` 在 EventLog 中存在，并检查来源 owner 是否被目录允许。
2. 请求进入 `Pending`，写 `event_d14_reward_request_*` 和审计；此时不发放任何物品或钱。`RequestTaskReward` 只做这一步。
3. RewardSystem 收到 `Grant` 后调用 `ID14RewardSettlementAuthority`。authority 必须返回 RewardSystem 的发放决定和 Inventory/Economy/Collection 的来源事件引用；缺失或跨 owner 的引用会拒绝。
4. RewardSystem 对来源事实调用 `EventLog.ClaimSettlement`，首次才写 `Granted`。同一来源在读档后再次结算会得到 `source_already_settled`，不会重复发放。
5. `Claim` 只把奖励记录标为 `Claimed` 并保存领取事实；它不复制所属系统的最终库存或现金状态。

唯一奖励的活动记录按 `reward_id + recipient_ref` 检查；`Granted`、`Claimed`、`Lost` 和 `Recovered` 都阻止第二个实例。恢复沿用原 `GrantId`，不会生成第二份唯一奖励。

## 4. 丢失恢复和审计

`MarkLost` 只能消费 Inventory、Ship 或 FactionLegal 的丢失/收缴事实，写入 `Failed + route_d14_reward_recover_lost`，并记录丢失原因和时间。`Recover` 必须同时满足：

- 当前记录状态为 `Lost`；
- 恢复来源事实存在；
- 尚在目录定义的恢复窗口内；
- `ID14RewardRecoveryAuthority` 同意，并返回 RewardSystem/Inventory/Economy/Collection 的可追踪来源。

请求、发放、领取、丢失和恢复各自有审计 ID。恢复审计同时保留原 `grant_id`、丢失事实、恢复事实和路线，读档不会重复创建实例。

## 5. 存档与测试

`D14RewardRuntimeSnapshot` 序列化到 `location_json.d14Reward`，保存请求、grant、状态、来源引用、审计、应用键和共享 EventLog。`SaveSystem` 只序列化/还原，不裁决奖励。

`D14RewardSystemTests` 覆盖：

1. 五档稀有度的 request → grant → claim；
2. 任务请求保持 pending，不产生 grant；
3. AgentReply 不能提交最终 reward；
4. Unique/WorldClass 防重复；
5. 丢失、窗口内恢复、同一 `grant_id` 和审计链；
6. 保存/读档后重复领取与恢复不重复结算。

验收：D14-A04 runner 2/2；Systems、Save.Tests 0 warning/0 error；内容注册表 121 entries / 13 prefixes；D14 Reward Systems 无网络或 LLM。
