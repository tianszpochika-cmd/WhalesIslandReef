# D12 牌局系统（D12-A05）

状态：已实现纵切片，2026-08-11 冻结 v1 数据边界。  
范围：一张低额酒馆牌桌、固定种子五张牌对局、下注/退出/作弊风险/结算及跨系统请求。  
不包含：批量牌桌、完整赌场内容、网络同步、LLM 裁决、直接经济或关系玩法。

## 1. 设计结论

牌面、牌局状态、下注上限、作弊风险、胜负、退出和牌局事实由 `MiniGameSystem` 唯一写入。牌局系统不直接增加/扣除金币、不直接增加债务、不直接改变关系、不直接发放奖励；它只生成带源事实的请求，由对应 canonical owner 消费。

`UI` 只能读取牌局读模型和提交动作意图，`Agent` 只能提出对话或动作建议。两者都不能改牌、改种子、改胜负、改奖金或确认奖励。牌局逻辑位于纯 C# Systems 数据层，未加入网络或 LLM。

## 2. 代表内容与规则

当前只登记一张可测试牌桌，避免在 D12-A05 批量创建内容：

| 字段 | 值 |
|---|---|
| `table_id` | `poker_table_low_stakes_01` |
| `ruleset_id` | `poker_caribbean_draw_v1` |
| 地点 | `santa_margarita_tavern` |
| 固定种子 | `120501` |
| 入场费 | 2 gold |
| 下注上限 | 20 gold（单局累计下注） |
| 债务上限 | 40 gold（由 Finance/Economy 实际 owner 再裁决） |
| 基础作弊风险 | 25%，按当前下注额最多再增加 20 个百分点 |

牌局使用标准 52 张牌，按固定种子和稳定 `game_id` 生成确定性洗牌；玩家与庄家各保存五张牌。牌型比较顺序为高牌、对子、两对、三条、顺子、同花、葫芦、四条、同花顺，并使用牌点和牌型内高牌作为确定性破平局依据。读档不会重新发牌。

| 动作 | 结果 |
|---|---|
| `Enter` | 校验地点、牌桌、玩家和经济读取快照；立即保存双方手牌并生成入场费请求 |
| `PlaceBet` | 校验正金额、累计下注上限及可用资金/债务上限；只生成下注扣款请求 |
| `Resolve` | 按已保存牌面生成 Win/Loss/Push、结算记录、奖励请求、关系请求和情报请求 |
| `AttemptCheat` | 使用固定哈希骰；暴露则牌局以 `Cheated` 结束，未暴露仍记录风险和情报事实；每局最多一次 |
| `Exit` | 以 `Fold` 结束，已提交入场费和下注不自动退款；不生成奖励请求 |

赢牌支付 `2 × 当前下注`，平局返还当前下注，输牌和退出支付 0。净变化只写入 `NetProposedDelta` 供后续 owner 读取，不是牌局系统的金币变更。

## 3. 核心数据与版本

`D12PokerRuntimeSnapshot` 的 `schema_version` 为 `1`，存于 `location_json.d12Poker`，并通过 `GameState.CurrentD12PokerRuntime` / `PlayerState.D12PokerRuntime` 接入 SaveSystem。每个对象带自己的 schema 版本；快照保存以下内容：

| 数据 | 关键字段 | 用途 |
|---|---|---|
| `D12PokerSessionState` | `game_id`、`table_id`、`ruleset_id`、`fixed_seed`、入场费、下注上限、当前下注、拟议债务、状态、结果、作弊风险、双方手牌、最后事实 | 进行中或已结束的桌局状态；牌面是存档事实，不读档重算 |
| `D12PokerGameRecord` | `record_id`、源结算事实、结果、牌型、入场费、下注、支付、净拟议变化、作弊风险、双方牌 ID、`settlement_claimed` | 不可变结算追踪与领奖幂等 |
| `AppliedEventKeys` | `event_id + save_revision` | 防止重复动作和读档重放 |
| owner 请求列表 | `request_id`、`source_event_id`、金额/债务/奖励/关系/情报字段、`status` | 跨系统交接；请求状态由消费方回写，不由 UI/Agent 伪造 |
| 快照元数据 | `revision`、`last_world_day`、`last_event_id`、`next_game_sequence` | 稳定 ID、审计顺序和恢复 |

稳定 ID 规则：`poker_game_<table_id>_<sequence>`、`poker_record:<game_id>`，事实以 `event_d12_poker_<action>_<game_id>` 形式生成；经济/债务/关系/情报请求绑定源事件，奖励请求固定为该局唯一 `reward_poker_win:<game_id>`。所有 ID 在快照中去重。

旧 D7 存档没有牌局 canonical 字段；缺少 `d12Poker` 时按空快照处理，不删除任何 D7 `Player`、`Ship`、`CrewAgent` 或其他存档字段。当前 v3 只新增上述 D12 牌局块和既有 D8 EventLog 接线；不存在单独的 D7 牌局迁移路径。

## 4. 数据所有权与跨系统边界

| 数据/动作 | 唯一写者 | 读取者 | D12-A05 输出 |
|---|---|---|---|
| 牌桌配置、固定种子 | 内容配置/牌局系统读取 | UI、牌局系统 | `D12PokerTableDefinition` |
| 牌面、下注、状态、作弊结果、胜负、退出、结算记录 | `MiniGameSystem` | UI、SaveSystem、审计/任务系统 | `D12PokerSessionState`、`D12PokerGameRecord`、`FactEvent` |
| 入场费/下注扣款 | `EconomySystem` | 牌局系统只读余额快照；UI 只读结果 | `D12PokerEconomyRequest`，不直接扣钱 |
| 债务本金与债务账本 | `FinanceSystem`/经济债务 canonical owner | 牌局系统只读当前债务和上限 | `D12PokerDebtRequest`，不直接改债务 |
| 赢牌/平局奖励 | `RewardSystem` 生成/消费奖励流程，金币由 `EconomySystem` 写 | UI、存档、奖励审计 | `D12PokerRewardRequest`，不直接发钱 |
| 玩家—酒馆对手关系 | D10 `RelationSystem` | 牌局系统只生成事实后请求；UI 只读 | `D12PokerRelationImpactRequest`，不能绕过强逻辑 |
| 牌桌线索/情报 | Information/Quest canonical owner | UI、Agent 表达层、任务系统 | `D12PokerInformationRequest`，仅带可靠度与代码 |
| 共享事实链 | D8 `EventLog` 按事实 owner 规则保存 | 各消费系统、SaveSystem | `poker_*` FactEvent；不重复追加 |
| 牌局存档复制 | `SaveSystem` | 牌局恢复 | `location_json.d12Poker`；不裁决领域状态 |

这意味着牌局结果可以先被保存，经济、债务、奖励和关系请求仍处于 `Pending`；后续消费必须以 `source_event_id` 和 `request_id` 幂等处理。若消费方拒绝，回写 `Rejected` 并保留牌局事实，不允许牌局系统私自补发或回滚其他 owner 的账本。

## 5. 作弊、债务和关系/情报后果

作弊风险为确定性百分比：`clamp(base_risk + min(20, current_bet * 20 / bet_cap), 0, 100)`。风险骰由固定种子、稳定局 ID 和 `cheat` 标签哈希得到，因此保存/加载、重复提交和不同客户端不会改变结果。

- 暴露：写 `poker_cheat_exposed`，状态为 `Cheated`，结果为 `Cheated`，支付 0，生成关系 -8 和 `cheat_exposed` 情报请求。
- 暂未暴露：仍写 `poker_cheat_unseen`，状态保持 `InHand`，生成关系 -1 和可靠度 0.55 的 `cheat_attempt_unseen` 情报请求；不能再次作弊。
- 公平输牌：关系 -2；公平赢牌：关系 +2；平局不改变关系。中途退出：关系 -1，并生成 `exit_before_showdown` 情报。
- 资金不足时，牌局只依据 Economy 提供的余额、借贷许可和债务上限计算拟议债务；不能因为进入牌局就直接开债或修改金币。

## 6. 幂等、读档和失败保护

1. 每个动作先由 `EventLog` 以 `event_id + save_revision` 判重，再更新牌局快照；重复事件返回现有事实，不重新发牌、不重新创建结果。
2. `Resolve` 只对 `InHand` 会话有效；会话结束后拒绝再次改变状态。
3. `ClaimSettlement(game_id, save_revision)` 同时检查 EventLog claim gate 和 `SettlementClaimed`。成功后在结算记录中置位；重复领取返回 `duplicate_settlement`，不再返回可支付奖励。
4. 奖励请求的稳定 ID 和请求源事实使 RewardSystem 可以幂等消费；牌局系统从不直接调用金币、债务或库存写入接口。
5. 保存时保留手牌、固定种子、结算记录、请求列表和应用事件键。缺少牌局块时加载为空；未知动作拒绝；未知持久化状态不被当作 `Pending` 自动执行，恢复后须由迁移/人工审计处理。

## 7. Contracts 与端边界

本任务没有新增 `contracts/` schema：完整牌局状态是 game-only，原因是 D8 冻结表把牌面和过程状态留在 game，跨端只暴露既有 D10 酒馆入口意图以及未来明确批准的只读结果投影。当前不让 server/web 成为牌面、胜负、奖金或关系的写者；若未来需要官网或服务端展示，必须先新增版本化 JSON Schema，并由 game 事实桥接，不能复用内部快照作为私有协议。

## 8. 测试入口

编辑器测试入口为 `D12PokerSystemTests`，覆盖：

- 固定牌面、入局、下注、结算、奖励/关系/情报请求及事实 owner；
- 下注上限和未知动作拒绝；
- 作弊暴露及关系/情报后果；
- 退出、债务请求和不退款；
- SaveSystem 保存/加载手牌、结算 claim gate 和不重复领奖。

本地验证命令：

```powershell
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
$env:PATH = (Resolve-Path 'game/Assets/Plugins/x86_64').Path + ';' + $env:PATH
dotnet run --project tools/d12_a05_runner/D12A05Runner.csproj --no-build
```

结果：Systems 0 warning/0 error；Save.Tests 0 warning/0 error；D12-A05 runner 5/5 PASS。
