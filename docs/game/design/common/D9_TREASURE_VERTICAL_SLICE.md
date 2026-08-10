# D9-A06 第一张海图寻宝垂直切片

任务：`D9-A06`  
状态：已实现 QuestSystem 领域入口、内容样例、统一事件、失败恢复和快照测试  
范围：`quest_first_chart` → `landmark:saint_marys_chart_cove` → `reward_first_chart`

## 1. 设计边界

这条任务的难点是可靠信息、装备、登陆路线和环境危险，不是猜谜。线索明确给出区域、潮线和可靠度；系统不接受答案字符串，也没有必须猜中的硬门槛。

唯一 Quest owner：`D9TreasureQuestSystem`（事件 `owner_system=QuestSystem`）。

QuestSystem 只写：

- 任务状态：传闻可用、已听取、已接受、失败、已恢复、已发现；
- 线索可靠度、来源、区域、时间窗口和危险标签；
- 装备前置的检查结果；
- 失败码、恢复路线和事实引用；
- `RewardRequest` 请求。

QuestSystem 不写：

- 金钱、价格、货物数量、库存、装备持有量或耐久；
- 船体、伤害、天气、关系、声望或奖励发放结果；
- UI 私有任务日志或 Agent 私有事实。

`BuildClueExpressionProposal` 只返回可供 Agent 表达的对白提议。Agent 不能修改可靠度、任务状态或 EventLog。

## 2. 冻结样例

| 对象 | ID/引用 | owner |
|---|---|---|
| 任务 | `quest_first_chart` | QuestSystem |
| 传闻来源 | `rumor:santa_margarita_dockhand` | QuestSystem 读取事实/内容 |
| 线索 | `clue:first_chart_tide_mark` | QuestSystem |
| 线索可靠度 | `72`（0—100） | QuestSystem |
| 区域 | `island_saint_marys` | IslandSystem/位置投影 |
| 登陆点 | `landmark:saint_marys_chart_cove` | D9 逻辑 location_ref |
| 装备 | `item_chart_compass` | ItemCatalog/Inventory 持有状态 |
| 危险 | `surf_break`、`unstable_shore` | QuestSystem 记录标签；环境/船舶系统裁决实际结果 |
| 奖励请求 | `reward_first_chart` | QuestSystem 提出，RewardSystem 裁决 |

`landmark:saint_marys_chart_cove` 是 D9 冻结的逻辑引用，不伪造新的内容前缀；它登记在任务内容的 `site_ref`，而不是被当作可直接发放的内容 ID。

新增内容条目：`game/Assets/Resources/ContentRegistry/entries/item/item_chart_compass.json`。它只定义探索装备语义；数量、持有、耐久仍归 Item/Inventory。

## 3. 流程

```text
RumorAvailable
  → HearRumor：写 event_d9_treasure_clue，记录 source/reliability/site/puzzle_gate=false
  → AcceptRumor：任务状态进入 Accepted
  → TryLand：读取装备快照，不写库存；记录 location/route/equipment/danger
      ├─ 缺装备/危险/潮汐/向导失败：写 event_d9_treasure_resolved(failed)
      │    → route_buy_chart_kit / route_wait_tide / route_village_guide / route_return_to_harbor
      └─ 成功登陆发现：写 event_d9_treasure_resolved(normal)
           → 生成 RewardRequest，不发奖励
  → Recover：写 event_d9_treasure_recovered(recovered)，cause 引用失败事实
  → TryLand：替代路线可再次发现
```

恢复路线：

- `route_buy_chart_kit`：取得 `item_chart_compass` 后重试；
- `route_wait_tide`：等待低潮窗口；
- `route_village_guide`：从 `green_cay` 取得向导；
- `route_return_to_harbor`：回到 `santa_margarita`，保留传闻和失败事实。

恢复不是覆盖失败事件，而是追加 `event_d9_treasure_recovered`，并通过 `cause_ids` 追溯失败原因。玩家不会因无法猜谜或一次登陆失败而永久软锁。

## 4. EventLog 字段边界

所有事实均由 `EventLog.Append` 校验，使用 D8 去重键 `event_id + save_revision`。

### `event_d9_treasure_clue`

- `owner_system=QuestSystem`；
- `result.values` 保存 `quest_id`、`clue_id`、`source_ref`、`region_ref`、`reliability_percent`、`site_ref` 和 `puzzle_gate=false`；
- 不保存 Agent 生成的最终任务结果。

### `event_d9_treasure_resolved`

- 成功或失败都写入；
- `input_facts` 保存任务、线索、登陆点、路线、装备和危险标签；
- `result.values` 保存 `equipment_ready`、`danger_tags`、`danger_outcome`、`failure_code`、`recovery_route_id`；
- 成功时只保存 `reward_request_id`，不保存金钱、物品数量、最终奖励或库存结果；
- 失败时必须带 D8 `recovery_route`。

### `event_d9_treasure_recovered`

- `outcome_status=recovered`；
- `cause_ids` 引用最近一次 `event_d9_treasure_resolved`；
- 保存恢复路线、装备是否就绪和危险标签；
- 不直接完成任务或发放奖励。

## 5. 存档与读档

`D9TreasureSaveEnvelope` 同时携带：

- `D9TreasureQuestSnapshot`：状态、可靠度、装备检查、危险标签、失败码、恢复路线、事实引用和 RewardRequest；
- `EventLogSnapshot`：事实、`NextSequence` 和 settlement keys。

`D9TreasureQuestSystem.RestoreFromSave` 先通过 `EventLog.FromSnapshot` 校验事件版本、序列、重复键和因果引用，再恢复 Quest 快照。它不直接操作 SQLite；统一 SaveSystem 负责把 envelope 写入正式存档。这样可以在 D9-S02 接入现有 SaveSystem 时保持单一存档写者，不在 QuestSystem 复制存档逻辑。

读档后重复调用同一发现不会追加第二条事实，也不会重新生成 RewardRequest；失败和恢复事实保留在因果链中。

## 6. 测试边界

测试入口：`D9-A06-TREASURE`  
代码：`game/Assets/Scripts/Systems/D9/D9TreasureQuestSystem.cs`  
Unity EditMode：`game/Assets/Tests/EditMode/D9TreasureQuestTests.cs`

覆盖：

1. 传闻、可靠度和 Agent 表达提议只读边界；
2. 缺装备失败、取得装备恢复、再次登陆成功；
3. 直达登陆危险、低潮替代路线和窗口未开拒绝；
4. RewardRequest 只生成请求，不包含最终金额/物品数量；
5. 失败与恢复的 EventLog 因果链；
6. 快照保存/恢复和已发现任务的重复发现保护。

性能预算沿用 D9 冻结：保存/读档 ≤500ms；任务系统不在 `Systems` 引入网络或 LLM。

