# D12-S01 交接报告：探索与社群集成

任务：`D12-S01`  
日期：2026-08-11  
结论：**D12 通过，允许 D13 使用探索/社群只读数据**

## 交付文件

- 主验收报告：`docs/game/design/common/D12_ACCEPTANCE_REPORT.md`
- 集成测试：`game/Assets/Tests/EditMode/D12AcceptanceIntegrationTests.cs`
- 独立运行器：`tools/d12_s01_runner/`
- owner 接线修复：`game/Assets/Scripts/Systems/D12/D12DynamicQuestCatalog.cs`
- A08 合成事实测试 owner 同步：`game/Assets/Tests/EditMode/D12DynamicQuestLogTests.cs`
- 记忆日志：`.workbuddy/memory/2026-08-11.md`

## 集成链

集成测试使用真实 D12/D9 系统公开 API，按以下顺序执行：传闻→D9 多来源线索交叉验证→装备前置→海图任务知识→法律拒绝与复核→低潮沉船打捞→动态主线完成→Green Cay 帮助/礼仪/物物交换→动态支线完成→固定种子输牌和债务/情报请求→Tarot 敏感宗教警告与文本解释→宝藏图鉴和授权特殊武器图鉴→保存→读档→重复动作判重。

没有新增第二套经济、库存、关系、任务、探索、牌局或图鉴状态；同步层只保存/恢复既有快照和共享事实。

## 重要接口边界

| 接口 | 交接规则 |
|---|---|
| `D12MapDiscoverySystem.GetMapReadModel()` | D13 只读已获得的地图信息，不读目录真实水深/未知位置 |
| `D12CommunityTradeSystem.GetCommunityReadModel/GetReputation()` | D13 只读社群库存/声望投影，不直接改物资、声望或边界 |
| `EventLog` / `EventLogSnapshot` | 事实必须带 owner、证据和因果；D13 不得从 Agent 文本造事实 |
| `D12SalvageRewardRequest`、`D12QuestRewardRequest`、牌局债务/情报请求 | 只作为 pending 请求交给 canonical owner，不能在 D13 直接兑现 |
| Tarot/历史/特殊武器内容 | 保留 warning、review、source、authorization 和 license 字段，不得跳过审核 |

## 修复记录

A08 动态任务目录的社群目标 owner 与 A04 实际事实 owner 不一致，导致真实 `community_help_offered` 无法推进动态任务。已将目录目标改为 `D12CommunityTradeIds.SystemOwner`（`CommunityTradeSystem`），并同步 A08 合成事实测试。该修复只统一 owner 标识，没有复制社群交易逻辑。

## 验证结果

- `dotnet run --project tools/d12_s01_runner/D12S01Runner.csproj --no-build`：`D12-S01 integration PASS count=2`；
- D12-A02 4/4、A05 5/5、A07 5/5、A08 4/4；A03/A04/A06 全部 PASS；
- AgentCore、Whalebone.Systems、Save.Tests：0 warning / 0 error；
- D9-S02：PASS，`blocked=0`，14 frozen events；
- Content Registry：79 entries / 13 prefixes，PASS；
- 无 Systems 网络/LLM 裁决；法律、宗教警告、历史授权均在集成测试中验证。

## D13 使用许可

D13 可以读取探索和社群的只读快照、地图/声望 read model、共享事实和带来源的 pending 请求。D13 不得直接写这些状态，也不得把软线索、社区传闻、解释文本或未消费请求当作确定奖励、债务、库存、关系或任务结果。
