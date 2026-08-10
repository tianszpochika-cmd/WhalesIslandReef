# D9-A06 任务交接报告

任务：`D9-A06` 第一张海图寻宝  
日期：2026-08-10  
前置：D9-S01 已完成  
状态：实现完成，交由 D9-S02 接入统一 SaveSystem 和完整航程验收

## 主成果

- `game/Assets/Scripts/Systems/D9/D9TreasureQuestSystem.cs`
  - `D9TreasureQuestSystem`：唯一 QuestSystem owner；
  - `D9TreasureQuestCatalog.CreateFirstChart()`：D9 冻结样例；
  - `D9TreasureClueExpressionProposal`：Agent 只能提出线索表达；
  - `D9TreasureRewardRequest`：只提出奖励请求，不发放奖励；
  - `D9TreasureSaveEnvelope`：Quest 快照与 EventLog 快照的统一接入形状。
- `game/Assets/Tests/EditMode/D9TreasureQuestTests.cs`
- `game/Assets/Resources/ContentRegistry/entries/item/item_chart_compass.json`
- 更新 `game/Assets/Resources/ContentRegistry/entries/quest/quest_first_chart.json`
- 更新 `docs/game/design/common/D9_VERTICAL_SLICE_FREEZE.md` 的 D9-A06 入口矩阵
- `tools/tests/test_d9_treasure_vertical_slice.py`
- 设计说明：`docs/game/design/common/D9_TREASURE_VERTICAL_SLICE.md`

## 任务流程

```text
传闻 → 可靠度 → 接受 → 检查海图罗盘 → 登陆
  ├─ 缺装备/错误地点/危险/潮汐或向导失败
  │    → failed 事实 → 回港、买装备、等低潮或找村落向导
  └─ 成功发现 → RewardRequest(reward_first_chart) → RewardSystem 裁决
```

没有答案字符串、猜谜解析或永久硬门槛。线索明确包含来源、区域、潮汐窗口、可靠度和危险标签。

## 事件与恢复

| 事件 | owner | 结果 |
|---|---|---|
| `event_d9_treasure_clue` | `QuestSystem` | 记录传闻来源、可靠度、区域、登陆点和 `puzzle_gate=false` |
| `event_d9_treasure_resolved` | `QuestSystem` | 成功/失败；装备、路线、危险、失败码和恢复路线进入 `input_facts/result.values` |
| `event_d9_treasure_recovered` | `QuestSystem` | `outcome_status=recovered`，`cause_ids` 引用失败事实 |

统一去重键沿用 D8：`event_id + save_revision`。恢复不是覆盖失败事件，因果链保留失败原因。

恢复路线：

- `route_buy_chart_kit`：取得 `item_chart_compass`；
- `route_wait_tide`：等待低潮窗口；
- `route_village_guide`：向绿礁村寻求向导；
- `route_return_to_harbor`：回到圣玛格丽特港保留线索后重试。

## 所有权边界

- QuestSystem 写任务状态、可靠度、危险标签、失败/恢复路线和事实引用；
- ItemCatalog/InventorySystem 写装备定义、持有量和耐久；本任务只读取装备 ID 集合；
- RewardSystem 读取 `D9TreasureRewardRequest` 并裁决/领取 RewardGrant；
- EconomySystem、ShipSystem、CrewSystem、FactionSystem 不被任务系统复制；
- Agent 只产生表达提议，不能改可靠度、任务结果或奖励；
- UI/日志读取快照和 EventLog，不创建私有事实；
- `Systems/D9` 没有网络或 LLM 代码。

## 保存/读档边界

`D9TreasureSaveEnvelope` 同时保存：

- Quest 状态、可靠度、装备检查、危险标签、失败码、恢复路线、事实引用和 RewardRequest；
- D8 `EventLogSnapshot` 的 facts、NextSequence 和 settlement keys。

`D9TreasureQuestSystem.RestoreFromSave` 使用 `EventLog.FromSnapshot` 恢复因果日志，避免读档后重复发现和重复生成请求。SQLite 的正式写入仍属于 SaveSystem；D9-S02 必须把此 envelope 纳入现有 `location_json`/v3 存档，而不能新增第二个存档数据库。

## 测试命令与结果

```text
python tools/content_registry_check.py game/Assets/Resources/ContentRegistry
# PASS: 20 entries / 13 prefixes

python -m unittest tools.tests.test_content_registry_check tools.tests.test_d9_treasure_vertical_slice -v
# PASS: 9/9

python tools/d9_vertical_slice_check.py
# PASS: 3 scenes / 5 prefabs / 10 commodities / 14 events / 6 UI / 7 agents

dotnet restore tools/d9_a06_smoke/D9A06Smoke.csproj
dotnet build tools/d9_a06_smoke/D9A06Smoke.csproj --no-restore
dotnet run --project tools/d9_a06_smoke/D9A06Smoke.csproj --no-build
# PASS: D9-A06 smoke（成功/失败/恢复/存档恢复/重复发现保护）
```

纯 C# smoke harness 位于 `tools/d9_a06_smoke/`，仅用于验证 D9 文件与 D8 EventLog 的编译/运行；Unity 主工程的生成 csproj 尚未因当前编辑器占用刷新新资产。

当前完整 `dotnet build game/Assembly-CSharp.csproj --no-restore` 和 `Whalebone.Systems.csproj` 仍被仓库已有 `AgentModels.cs(123,124)` 缺失 `CrewDuty` 基线错误阻断，错误不来自 D9-A06。D9-A06 文件已通过独立 Systems/EventLog 编译和 smoke。

## 交给 D9-S02 的接线项

1. 将 `D9TreasureSaveEnvelope` 接入现有 SaveSystem 的正式存档封套；
2. 当前/恢复后的唯一 EventLog 创建完成后，再绑定 D9-A07 `D9EventLogBridge`；
3. RewardSystem 按 `reward_first_chart` 请求执行真正奖励结算和 `ClaimSettlement`，QuestSystem 不代发；
4. 运行 D9 全航程：整备 → 装货 → 配员 → 出航 → 钓鱼 → 风暴 → 船员讨论 → 登陆寻宝 → 返回交易 → 工资 → 修船 → 保存 → 读档；
5. 继续修复 D9-S02 已登记的其他 owner 事件生产者缺失；当前只剩 D9-A02 handoff 未补齐，不要在 D9-A06 内补写 Ship/Economy/Ecology/Environment/Combat 事实。
