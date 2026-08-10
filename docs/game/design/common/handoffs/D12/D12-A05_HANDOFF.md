# D12-A05 交接报告：牌局系统

- 任务编号：D12-A05
- 日期：2026-08-11
- 前置：D9-S02 已通过；D12-A01 地图发现与 D10 酒馆/关系边界已存在
- 状态：完成纵切片，可交给后续 owner 做消费接线；允许开始不依赖未完成经济/奖励消费实现的异步文档或测试任务

## 1. 修改文件

主成果：

- `game/Assets/Scripts/Systems/D12/D12PokerContracts.cs`
- `game/Assets/Scripts/Systems/D12/D12PokerCatalog.cs`
- `game/Assets/Scripts/Systems/D12/D12PokerSystem.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`：接入 `location_json.d12Poker`、`GameState`/`PlayerState` 读写及完整牌局编解码；保留工作区并行 D12 兼容 helper
- `game/Assets/Tests/EditMode/D12PokerSystemTests.cs`
- `game/Save.Tests.csproj`：已确认已有 D12 测试登记，本任务复用该登记
- `tools/d12_a05_runner/D12A05Runner.csproj`
- `tools/d12_a05_runner/Program.cs`
- `docs/game/design/common/D12_POKER_SYSTEM.md`
- `docs/game/design/common/handoffs/D12/D12-A05_HANDOFF.md`
- `.workbuddy/memory/2026-08-11.md`

未修改 `contracts/`；未向 `game/Assets/Scripts/Systems/` 加入网络或 LLM；未修改 UI、AgentCore 公共类型或无关玩法系统。

## 2. 数据边界与公共类型

`MiniGameSystem` 是牌面、牌局状态、下注、作弊风险、胜负、退出、结算记录及 D12 牌局事实的唯一写者。公共 DTO 位于 `Whalebone.Systems.D12`：

- `D12PokerTableDefinition`、`D12PokerCatalog`：牌桌和固定种子配置；
- `D12PokerActionRequest`、`D12PokerActionResult`：外部动作意图及只读结果；
- `D12PokerSessionState`、`D12PokerGameRecord`：过程快照和结算账本；
- `D12PokerEconomyRequest`、`D12PokerDebtRequest`、`D12PokerRewardRequest`、`D12PokerRelationImpactRequest`、`D12PokerInformationRequest`：带 `request_id`/`source_event_id` 的跨 owner 请求；
- `D12PokerRuntimeSnapshot`：版本 1 的存档根，含会话、牌面、记录、请求、事件键和恢复元数据；
- 明确类型：`long` 金额/债务/种子/修订号，`int` 世界日/关系变化，`D12Poker...` 枚举表示状态、结果、作弊风险、牌型、花色、点数和请求状态。

所有牌面和状态保存在 game 内部。经济/Finance、Reward、D10 Relation、Information/Quest 只读取对应请求并由各自 owner 写 canonical 状态。UI 只能读和发起意图；Agent 只能提议对话/行动；二者没有直接写入点。

## 3. D7 / v3 差异

| 类别 | 处理 |
|---|---|
| D7 保留字段 | `Player`、`Ship`、`CrewAgent` 及既有存档字段未删除、未改语义 |
| D7 牌局字段 | D7 没有 `MiniGameSession`/扑克 canonical 存档字段，因此没有旧牌局字段迁移；旧存档缺失 `d12Poker` 时按空快照加载 |
| v3 新增 | `D12PokerRuntimeSnapshot`、`D12PokerSessionState`、双方五张牌、固定种子、下注/入场费/拟议债务、作弊风险、结果、结算记录、请求列表、`settlement_claimed`、应用事件键和 `location_json.d12Poker` |
| 既有 D8/D10 复用 | 牌局事实进入共享 EventLog；D10 酒馆原有 `D10PokerEntryIntent` 仍是入口投影，不被改造成胜负/奖励协议 |

## 4. 规则、所有权和幂等

- 一张低额桌：`poker_table_low_stakes_01`，`poker_caribbean_draw_v1`，固定种子 `120501`，入场费 2，累计下注上限 20，债务上限 40，基础作弊风险 25%。
- 标准 52 张牌，玩家/庄家各保存五张；牌型和破平局比较纯确定性；保存后不重新发牌。
- 赢牌支付 `2 × 当前下注`，平局返还当前下注，输牌/退出/作弊暴露支付 0。金额只是 Reward/Economy 请求中的提议值。
- 进入、下注、作弊、结算、退出均写 `FactEvent`；事实键使用 `event_id + save_revision`。
- 经济扣款由 `EconomySystem` 写；债务由 Finance/Economy 债务 owner 写；奖励由 RewardSystem 消费并委托 Economy 写金币；关系由 D10 `RelationSystem` 裁决；情报由 Information/Quest owner 消费。
- 经济请求 ID 绑定源事件，避免相同金额的多次下注被错误合并；奖励请求固定为单局唯一 ID。
- `ClaimSettlement` 同时使用 EventLog claim gate 和结算记录 `SettlementClaimed`；重复领取返回 `duplicate_settlement`，不会再产生可支付奖励。

## 5. 测试命令和结果

已执行：

```powershell
dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162
dotnet build tools/d12_a05_runner/D12A05Runner.csproj --no-restore
$env:PATH = (Resolve-Path 'game/Assets/Plugins/x86_64').Path + ';' + $env:PATH
dotnet run --project tools/d12_a05_runner/D12A05Runner.csproj --no-build
```

结果：

- `Whalebone.Systems`：0 warning / 0 error；
- `Save.Tests`：0 warning / 0 error；
- `D12A05Runner`：5/5 PASS；覆盖固定种子、入局/下注/结算、未知动作/下注上限、作弊暴露、退出/债务、SaveSystem 读档和不重复领奖。

## 6. 冲突与未完成事项

1. 当前工作区同时存在 D12-A02～A07 的并行 SaveSystem 改动，其中 Tarot、CommunityTrade、Salvage 等有重叠的 DTO 编解码 helper。牌局路径已明确接到完整 `D12PokerToObj`/`D12PokerFromObj`，并保留其他任务的兼容 helper；没有删除 D7 字段或覆盖其他 D12 领域状态。
2. 牌局系统目前只产生 Economy/Finance/Reward/Relation/Information 请求；对应 owner 的消费、拒绝回写、跨系统任务追踪不在 D12-A05 范围内。后续接线不能把这些写权限移回牌局系统。
3. 当前没有新增 `contracts/`，因为牌面和过程状态按 D8 冻结表为 game-only；若 D13 或 server/web 需要公开结果，必须先走版本化契约评审。
4. Unity EditMode 本轮未由命令行启动；对应测试已登记并由 Save.Tests 编译，独立 runner 已执行行为验证。

## 7. 验收结论和后续注意事项

验收结论：通过。牌局可进入、下注、退出和固定牌面结算；作弊暴露/未暴露、债务拟议、关系/情报后果均有可追溯事实；保存后手牌和结算 claim gate 恢复；重复结算/领奖不会重复产生奖励；UI/Agent 没有直接经济或胜负写入口。

后续任务注意：

- 经济/Finance/Reward 消费请求必须以 `source_event_id`、`request_id` 和 `save_revision` 做幂等键，拒绝时保留牌局事实；不能在读档时重新执行 `Enter` 或 `Resolve`。
- RelationSystem 只消费 `D12PokerRelationImpactRequest` 或共享事实，不能让 UI 直接应用 delta；Information/Quest 只将可靠度作为线索输入，不能把情报直接变成必赢条件。
- 若新增牌桌或规则，必须新增版本化规则集和固定种子测试，不得修改已保存局的规则/牌面；规则迁移应另立任务。
- 任何 server/web 投影必须走 `contracts/`，禁止序列化 `D12PokerRuntimeSnapshot` 作为私有跨端协议。

允许开始：允许开始不依赖牌局 owner 决策的异步文档、只读 UI 读模型和测试任务；经济/奖励/关系实际消费接线应在各自 owner 明确接口后再合并。
