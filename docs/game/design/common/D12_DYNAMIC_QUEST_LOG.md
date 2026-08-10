# D12 动态任务与航海日志（D12-A08）

状态：已实现，D12-A08 验收通过。

## 目标与边界

D12-A08 提供动态主线/支线任务的确定性状态机，以及基于统一事实日志的航海日志投影。任务系统只拥有以下数据：

- 任务状态（Available、Active、Failed、Recovered、Completed、Abandoned）；
- 目标定义、目标进度与已应用事实引用；
- 失败事实、恢复路线、替代路线和完成事实引用；
- 发给 `RewardSystem` 的 pending `D12QuestRewardRequest`。

任务不拥有也不写入金钱、库存、船员关系、图鉴或奖励结果。任务完成只提出奖励请求；金额、物品、声望、图鉴解锁等最终结果由对应 owner 裁决和写入。

`D12DynamicQuestLogSystem` 位于 `game/Assets/Scripts/Systems/D12/`，只依赖本地 `EventLog` 和 D12 契约，不含网络、LLM 或 Unity 依赖。

## 数据与唯一写者

| 数据 | 唯一写者 | D12-A08 的使用方式 |
|---|---|---|
| 世界事实、结果、因果 | `EventLog` / 产生事实的领域系统 | 任务只观察已有 `FactEvent`，不复制事实载荷 |
| 任务状态、目标进度、失败/恢复/替代路线 | `QuestSystem`（本实现） | 由任务输入和已验证事实确定性更新 |
| 金钱、库存、关系、图鉴和实际奖励 | 各自领域 owner / `RewardSystem` | 任务只发带来源事实和因果的 pending request |
| 存档编码 | `SaveSystem` | 只序列化/反序列化 `D12RuntimeSnapshot`，不裁决任务 |
| UI | 表现层 | 只能读取任务结果、日志快照和 EventLog 事实 |
| Agent | Agent/对话层 | 只能读取事实摘要并提议，不能创建事实或裁决任务/奖励 |

D12-A08 是通用动态任务日志，不复制 D9 的寻宝经济、库存、关系或奖励状态；D12-A03 继续在既有 `D9TreasureQuestSystem` 上扩展线索账本，两个 owner 不互相复制。

## 任务定义与确定性目标

契约文件为 `D12DynamicQuestLogContracts.cs`，目录为 `D12DynamicQuestCatalog.cs`。每个目标必须声明目标 ID、描述、领域 owner、接受的统一事件结果码和所需次数；需要限定实体时，再声明事件 `Result.Values` 的键和值。

当前垂直切片目录包含：

1. 主线 `quest_d12_saint_marys_chart_recovery`：先由 `ExplorationSystem` 记录 `map_discovery_recorded`，再由 `ExplorationSystem` 记录 `salvage_success`。奖励请求只携带 `reward_d12_saint_marys_chart_evidence` 和 `MapKnowledge` 类型。
2. 支线 `quest_d12_green_cay_reciprocity`：消费 `D12CommunityTradeSystem` 产生的 `community_help_offered` / `community_trade_completed` 事实，提出 `Reputation` 类型奖励请求。

目标匹配同时检查：

- `Result.ResultCode` 是否在目标允许集合；
- `FactEvent.OwnerSystem` 是否为目标 owner；
- 可选的 `Result.Values[MatchValueKey]` 是否匹配目标实体；
- 同一 `event_id + save_revision` 是否已经应用。

因此天气、潮汐、打捞、社区交易等领域系统仍由各自 owner 裁决；任务不会根据 Agent 文本或 UI 输入自行推断成功。

## 因果、日志和 Agent 摘要

任务接受、替代路线选择和任务完成会通过 `EventLog.Append` 产生 `QuestSystem` 事实，分别使用稳定事件前缀 `event_d12_quest_accepted_`、`event_d12_quest_alternative_` 和 `event_d12_quest_completed_`。任务观察到的地图、打捞、社区等事实只保存 `D12QuestFactRef(event_id, save_revision)`，不另存一份事件事实。

`ReadJournal()` 先用引用从 `EventLog` 解析事实，再生成只读 `D12QuestJournalEntry`。引用缺失时读档恢复失败，避免日志产生“幽灵事实”。完成事实和奖励请求的 `CauseEventIds` 来源于已登记的事实引用，能够追溯任务接受、领域目标事实和替代路线事实。

`BuildAgentSummary()` 委托统一 `EventLog.BuildAgentSummary()` 与 `ReadFactsForUi()`，返回 `FactsOnly=true`、`CanCreateFacts=false` 的摘要。摘要是已有事实的压缩表达，不允许创造目标完成、失败、奖励或世界状态。

## 失败、恢复和替代路线

任务定义显式列出 `FailureResultCodes`、`RecoveryResultCodes` 和默认恢复路线。当前主线把 `salvage_risk_failed`、`competitor_interference` 视为失败，并要求 `route_return_to_harbor_and_retry`；失败事实本身仍保留在日志中。

失败后可以：

- 由领域系统产生 `salvage_recovered` 等恢复事实，任务进入 `Recovered` 并保留失败/恢复事实引用；或
- 调用 `SelectAlternativeRoute`，由任务系统写入 `quest_alternative_route_selected` 事实，记录失败事实为因果，并恢复到定义中的目标；或
- 走目标声明的 `D12QuestObjectiveAlternative`，例如使用 `D12CommunityTradeSystem` 的 `community_help_offered` / `community_trade_completed` 作为替代证据。

路线选择只更新任务路线状态和事实引用，不直接返还物资、不修改关系、不生成奖励。

## 奖励请求与幂等

任务完成时生成一个稳定 `RequestId` 的 `D12QuestRewardRequest`，状态为 `pending`，包含 `RewardId`、任务 ID、完成事实 ID、保存修订号、奖励路由类别、`RewardSystem` owner 和因果事件 ID；不包含金额、物品数量或关系数值。

重复观察同一事实会返回 `Duplicate`，不会再次追加日志、完成事实或奖励请求。读档恢复会重新校验任务定义、目标/路线 ID 和全部事实引用；因此保存、加载、再次观察不能重复推进任务或生成奖励请求。

## 存档

`D12RuntimeSnapshot.DynamicQuestLog` 已接入 `SaveSystem`，路径为 `location_json.d12.dynamicQuestLog`。保存前执行快照校验；读档会恢复任务、目标进度、事实引用、失败/恢复路线和 pending 奖励请求。旧存档缺少该字段时使用空的 schema v1 快照，不改变已有领域状态。

EventLog 仍是事实唯一来源；动态日志快照只是任务状态和引用的恢复材料。恢复后的事实引用必须在当前 EventLog 中存在，否则拒绝恢复，避免读档重复添加或凭空补事实。

## 测试覆盖

`game/Assets/Tests/EditMode/D12DynamicQuestLogTests.cs` 与 `tools/d12_a08_runner/` 覆盖：

- 统一地图/打捞事实推进多目标，并只生成 pending RewardRequest；
- 失败、默认恢复和替代路线的事实因果链；
- 日志只来自 EventLog，Agent 摘要不能创建事实；
- 保存/恢复、事实引用校验和重复观察幂等。

本任务验证结果：

- D12-A08 独立测试：4/4 PASS；
- D12-A02、A03、A04、A06、A07 回归：全部 PASS；
- `Whalebone.Systems`：0 warning / 0 error；
- `Save.Tests`：0 warning / 0 error；
- D9-S02 前置验收：PASS，`blocked=0`，14 个冻结事件；
- 内容注册表：79 entries / 13 prefixes，PASS；
- `git diff --check`：通过（仅报告工作区既有换行风格提示，无新增空白错误）。
