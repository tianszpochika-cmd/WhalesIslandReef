# D14-A04 交接报告

## 任务

- 任务编号：D14-A04
- 目标：实现普通、专业、稀有、独特、世界级奖励及唯一性、来源、领取、丢失恢复、任务奖励请求。
- 状态：完成。

## 修改文件

- `game/Assets/Scripts/Systems/D14/D14RewardContracts.cs`
- `game/Assets/Scripts/Systems/D14/D14RewardCatalog.cs`
- `game/Assets/Scripts/Systems/D14/D14RewardSystem.cs`
- `game/Assets/Scripts/Systems/Save/GameState.cs`
- `game/Assets/Scripts/Systems/Save/Models/PlayerState.cs`
- `game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- `game/Whalebone.Systems.csproj`
- `game/Save.Tests.csproj`
- `game/Assets/Tests/EditMode/D14RewardSystemTests.cs`
- `tools/d14_a04_runner/D14A04Runner.csproj`
- `tools/d14_a04_runner/Program.cs`
- `game/Assets/Resources/ContentRegistry/entries/reward/` 下 5 个 D14 条目及 `registry.json` roots。
- `docs/game/design/common/D14_REWARD_SYSTEM.md`

## 新接口和边界

- `D14RewardDefinition`：奖励 ID、稀有度、类型、来源 owner、内容 ID、唯一性、审查、授权和恢复窗口；不包含最终数量/金额。
- `D14RewardRequest` / `D14TaskRewardRequest`：pending 奖励请求；任务只提交请求，不发放奖励。
- `D14RewardGrantRecord`：来源、内容、唯一性、状态、领取/丢失/恢复事件和所属系统结算引用。
- `D14RewardAuditRecord`：每个状态动作的事件、来源、原因和恢复路线。
- `ID14RewardSettlementAuthority`：返回 RewardSystem 发放决定及 Inventory/Economy/Collection 来源引用。
- `ID14RewardRecoveryAuthority`：返回恢复决定及恢复来源引用。
- `D14RewardSystem`：RewardSystem 唯一写奖励请求、grant、领取、丢失、恢复和审计；不写库存、钱、价格、图鉴或装备最终状态。

## 验收结果

- `D14-A04 tests PASS count=2`；
- Systems 编译：0 warning / 0 error；
- Save.Tests 编译：0 warning / 0 error；
- 内容注册表：121 entries / 13 prefixes PASS；
- Save/Load 保留奖励事件、唯一 grant、领取状态、恢复状态和审计；重复领取/恢复返回 duplicate；
- D14 Reward Systems 红线：无网络、LLM 或 Agent 裁决逻辑。

## 后续接线

1. D14-S01 可将 D12 Quest/Collection、D14 Aftercare/Combat 的已落盘事实转换为 `D14RewardRequest`，再由 RewardSystem 统一调用 `Grant`；不得复制奖励或直接改 Inventory/Economy/Collection。
2. D14-A03 特殊装备获取可消费 RewardSystem 的发放来源引用；装备唯一实例仍由 A03/所属 authority 管理。
3. 后续奖励目录扩展必须新增 `reward_` 内容 ID、来源、审查、授权和测试 ID；不能在 AgentReply 或 UI 中加入最终 reward 值。
