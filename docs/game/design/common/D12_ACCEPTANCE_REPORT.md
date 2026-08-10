# D12 探索与社群集成验收报告（D12-S01）

任务：`D12-S01`  
日期：2026-08-11  
结论：**D12 通过**

## 1. 集成范围

D12-S01 将 D12-A01—A08 的既有 owner 接成一条可保存、可恢复、可追溯的玩家航程：

```text
D9 港口传闻
  → D9 QuestSystem 记录多来源线索并交叉验证
  → D12 ExplorationSystem 记录沉船位置
  → 准备海图罗盘、抓钩、绳索、抽水泵
  → Legal/Faction authority 先拒绝，再经法律复核路线恢复
  → D12 SalvageSystem 低潮打捞并产生 pending RewardRequest
  → D12 CommunityTradeSystem 求助、学习礼仪并完成物物交换
  → MiniGameSystem 固定牌局输牌，产生债务/情报请求
  → TarotSystem 经过宗教警告确认，解释保持文本隔离
  → CollectionAtlasSystem 记录宝藏与授权特殊武器
  → SaveSystem 保存 D3 EventLog、D9、D12、D12 Poker 快照
  → 读档恢复全部状态，重复动作返回 Duplicate/AlreadySettled
```

同步测试只编排系统公开接口；没有复制探索、任务、库存、经济、关系、牌局或图鉴状态，也没有新增 UI 写入口。

## 2. 领域 owner 与跨系统事实

| 阶段 | 裁决 owner | 集成读取/提交 | 集成结果 |
|---|---|---|---|
| 传闻/线索 | D9 `D9TreasureQuestSystem` | `HearRumor`、`RecordCatalogClue`、`CrossValidateClues` | 传闻、来源、可靠度、交叉验证和因果进入 EventLog |
| 海图发现 | `ExplorationSystem` | `RecordQuestKnowledge`、`GetMapReadModel` | `wreck_saint_marys_chart_cove` 可见，位置不是由 Agent 解锁 |
| 装备 | Inventory/Item owner | D9/D12 只读装备 ID 快照 | 罗盘、抓钩、绳索、抽水泵满足前置，未由任务/打捞生成 |
| 法律/产权 | 注入的 `ID12SalvageLegalAuthority` | `Rule`、`route_wreck_legal_review` | 首次 `legal_denied`，复核后允许；D12 不自行判定产权 |
| 沉船打捞 | `D12WreckCaveSalvageSystem` | `TrySalvage`、`Recover` | 低潮成功，生成 pending `D12SalvageRewardRequest`，不生成物品 |
| 动态任务 | `QuestSystem` | A08 `Accept`/`ObserveFact` | 主线消费地图/打捞事实，支线消费社群帮助/交易事实；只写任务状态和请求 |
| 社群交易 | `CommunityTradeSystem` + D3 Inventory owner | `RequestHelp`、`ProposeTrade`、`SettleTrade` | 礼仪/知识通过后物物交换成功，社群声望与玩家货物由各自 owner 写入 |
| 牌局 | `MiniGameSystem` | 固定种子低额牌桌 | 确定性输牌，产生债务与情报请求；钱/债务/关系未由牌局直接写入 |
| 宗教语境 | `D12TarotSystem` | warning gate、确认会话、文本解释 | 未确认警告不抽牌；确认后非超自然三牌会话，解释 `affects_state=false` |
| 图鉴/特殊奖励 | `CollectionAtlasSystem` / `RewardSystem` | 已存在事实 + 授权事实 | 宝藏图鉴和 `item_special_weapon_moonwake_cutlass` 成就记录成功；授权来自 `HarborAuthority` 事实 |
| 事实/存档 | `EventLog` / `SaveSystem` | 共享 `EventLogSnapshot` 和各 owner 快照 | 因果链、状态、pending 请求和幂等键完整恢复 |

## 3. 关键验收结论

### 3.1 线索、装备和探索

Agent 的 `BuildClueExpressionProposal` 与海图 `ProposeCommunityKnowledge` 均只返回提案，事实数量保持不变。传闻由 D9 QuestSystem 记录，打捞海图碎片与港口传闻完成不同来源交叉验证；D12 地图只通过 `RecordQuestKnowledge` 进入可见读模型。

集成请求显式携带罗盘、抓钩、绳索和抽水泵。D9 线索装备前置、D12 绳长/水泵检查均由既有 owner 执行，集成层没有添加物品或修改货舱。

### 3.2 沉船、法律与失败恢复

第一次打捞由外部法律适配器明确拒绝，结果是 `legal_denied`，恢复路线为 `route_wreck_legal_review`；法律适配器改为允许后，调用 D12 原有 `Recover` 追加 `salvage_recovered`，再以低潮和确定性风险种子成功打捞。

成功只产生 `D12SalvageRewardRequest(status=pending)`。主线动态任务观察统一打捞事实完成，不读取或复制打捞快照。

### 3.3 社群与牌局

Green Cay 社群先通过 `RequestHelp` 记录 `community_help_offered`，再用正确语言、知识等级和 `GiftBeforeTrade` 礼仪完成 `community_trade_completed`。社群库存、长期声望和玩家货物变化均来自既有系统。

牌局使用 D12 固定种子目录和经济只读快照：余额为 0、允许借贷，入场费/下注产生 pending debt request；确定性牌面结算为 `Loss`，同时返回债务请求、关系提议和情报请求。集成层没有应用这些最终数值。

### 3.4 宗教、历史与授权审查

- Tarot 占卜者的宗教敏感警告未确认时返回 `warning_required`，没有会话或牌面事实；确认后才抽取三张非超自然牌。
- 解释使用 `template` provider，只写解释文本；事件值明确 `affects_state=false`，不改变天气、任务、奖励或关系。
- 社群和保护区目录通过 `Validate()`，状态为 `ApprovedWithWarning`，边界由 `PropertyIslandSystem` 定义并只读检查。
- 特殊武器收藏必须带 `armory_authorization_moonwake` 和 `HarborAuthority` 授权事实；没有授权不能进入图鉴。内容注册表当前为 79 entries / 13 prefixes，检查通过。

## 4. 保存、恢复与重复保护

集成测试保存以下真实快照：

- `location_json.d3`：共享 `EventLogSnapshot` 与玩家货物投影；
- `location_json.d9`：D9 传闻、线索、交叉验证和寻宝奖励请求；
- `location_json.d12`：地图、打捞、社群、Tarot、图鉴和 A08 动态任务日志；
- `location_json.d12Poker`：固定手牌、输牌结算、债务/情报请求和 `settlement_claimed`。

读档后验证：

- 共享事实数量不变，D9 交叉验证计数保持；
- 主线/支线任务仍为 `Completed`，航海日志仍能从 EventLog 解析；
- 同一打捞请求返回 `already_salvaged`，RewardRequest ID 不变；
- 同一社群交换返回 `Duplicate`，不重复修改货物/声望；
- 牌局二次领取返回 `duplicate_settlement`；
- Tarot 同一会话返回 `Duplicate`；
- 同一图鉴证据返回 `Duplicate`，成就不重复解锁。

## 5. UI、Agent 和网络红线

- UI 只读取各系统 read model、快照和 EventLog 事实；D12-S01 没有新增 UI 写入口。
- Agent 只提供线索表达、社区知识提案、解释文本和对话意图，不能裁决奖励、债务、声望、牌局结果或任务完成。
- D9-S02 Systems redline 通过；新增/修改 Systems 代码没有 HTTP、网络客户端或 LLM 裁决路径。
- `AgentCore` 独立编译通过，未向 AgentCore 添加 Unity/第三方依赖。

## 6. 集成修复

集成测试发现 A08 动态任务目录的社群目标 owner 字符串与 A04 统一 owner 不一致：目录原先写为 `D12CommunityTradeSystem`，而实际 `D12CommunityTradeIds.SystemOwner` 为 `CommunityTradeSystem`。已改为引用该唯一 owner 常量，并同步修正 A08 的合成事实测试；没有复制或迁移社群裁决逻辑。

## 7. 验证结果

- `D12-S01` 集成测试：2/2 PASS；
- `D12-A02`：4/4 PASS；`D12-A03`、`D12-A04`、`D12-A06`：PASS；
- `D12-A05`：5/5 PASS（通过 Unity sqlite 原生库目录）；
- `D12-A07`：5/5 PASS；`D12-A08`：4/4 PASS；
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore`：0 warning / 0 error；
- `dotnet build game/Whalebone.Systems.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -t:Rebuild -p:NoWarn=0162`：0 warning / 0 error；
- `python tools/d9_s02_acceptance_check.py`：D9-S02 PASS，`blocked=0`，14 frozen events；
- `python tools/content_registry_check.py game/Assets/Resources/ContentRegistry`：79 entries / 13 prefixes PASS；
- Systems/AgentCore/敏感内容与 owner 红线检查：PASS。

## 8. 最终结论与 D13 授权

**D12 通过。** 本轮无阻塞遗留问题。D12 的探索和社群数据允许 D13 使用，但仅限以下方式：

1. 读取 `D12MapDiscoverySystem.GetMapReadModel()`、`D12CommunityTradeSystem.GetCommunityReadModel/GetReputation()` 和已保存的 D12 快照；
2. 通过共享 `EventLog` 读取有 owner、证据、时间、地点和因果的事实；
3. 消费 pending 请求时仍由 Reward、Economy/Finance、Inventory、Relation、Property/Legal 等 canonical owner 裁决；
4. 不把 Tarot 软线索、Agent 表达或社区传闻直接当作确定事实，不绕过保护区、法律、宗教警告、历史来源和授权字段；
5. D13 不得把探索/社群数据复制成第二份库存、关系、经济或任务状态。
