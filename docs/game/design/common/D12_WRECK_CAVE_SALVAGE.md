# D12 沉船、洞穴与打捞系统

任务：`D12-A02`  
状态：已实现，可供 D12-A03/A08 读取  
前置：`D9-S02 = PASS`、`D12-A01 = 完成`

## 1. 目标与唯一写者

沉船和洞穴的难点是位置证据、潮汐窗口、装备准备、可解释风险、竞争者和法律归属，而不是一次点击生成宝物。

`D12WreckCaveSalvageSystem` 是沉船位置、洞穴入口、打捞尝试、失败状态、损失记录、恢复状态和 RewardRequest 的唯一写者。它不写库存、装备耐久、船体、船员伤病、法律/势力声望或最终奖励。

| 数据 | 写者 | D12-A02 只能做什么 |
|---|---|---|
| 沉船/洞穴目录定义 | D12 内容目录 | 读取稳定 ID、位置、窗口、风险和路线 |
| 海图位置是否已知 | `D12MapDiscoverySystem` | 通过 `GetMapReadModel` 读取；未测绘地点不可进入打捞 |
| 打捞状态/失败/损失/恢复 | `D12WreckCaveSalvageSystem` | 唯一写入并追加共享事实 |
| 装备持有与耐久 | Inventory/Item owner | 接收装备快照，不消耗或生成物品 |
| 船体/船员伤病 | Ship/Crew/Medical owner | 读取风险事实后自行裁决 |
| 产权、许可证、查封和势力后果 | Faction/Legal owner | 通过 `ID12SalvageLegalAuthority` 提供裁决 |
| 金钱、物品和奖励 | Economy/Inventory/Reward owner | 接收 `D12SalvageRewardRequest`，自行结算 |

## 2. 纵切片地点

| 地点 | 海图特征 | 潮汐 | 必需装备 | 主要风险 | 恢复路线 |
|---|---|---|---|---|---|
| `wreck_saint_marys_chart_cove` | `wreck_saint_marys_chart_cove` | 低潮 | 抓钩、绳索，水泵可降低进水风险 | 绳索滑脱、进水、竞争者 | 等潮、补装备、法律复核、回港 |
| `cave_green_cay_tide_cave` | `cave_green_cay_tide_cave` | 低潮 | 绳索、提灯、灯油，洞穴标记可降低风险 | 塌方、落水、竞争者 | 回到入口、等潮、补装备、法律复核 |

地点定义只描述真实世界内容；玩家是否已知、尝试次数和宝物请求均保存在运行时快照中。

## 3. 打捞流程

```text
D12-A01 海图出现探索地点
  → Faction/Legal authority 提供归属/许可证裁决
  → 检查潮汐、装备、绳长、照明和位置
  → 使用稳定 attempt_id 与 seed 计算可复盘风险
  → 成功：写 salvage_success 事实 + RewardRequest
  → 失败：写 Failed 事实 + RecoveryRoute + 可选 LossRecord
  → 恢复：追加 Recovered 事实，引用原失败事件
  → 新 attempt_id 重试；已 Salvaged 地点拒绝重复生成请求
```

`D12SalvageRequest` 的装备列表是 Inventory owner 提供的只读快照。沉船最低绳长为 24 米，洞穴最低绳长为 30 米；洞穴要求至少 4 小时照明。潮汐窗口和实际潮位由 Environment 输入，D12 不自行写潮汐。

风险骰由 `D12SalvageRiskCalculator.Roll(site_id, attempt_id, seed)` 确定性生成。基础风险、缺少水泵/洞穴标记、照明临界、潮位偏差和竞争者压力共同改变风险阈值；事件值记录 `risk_roll`、`risk_threshold`、装备、潮汐和竞争者证据，玩家可以复盘失败原因。

## 4. 法律与竞争者边界

打捞开始前调用注入的 `ID12SalvageLegalAuthority.Rule`。D12 只接受包含 `ruling_ref`、`claim_status`、理由和 `SalvageAllowed` 的裁决：

- 无裁决：记录 `legal_ruling_required` 失败与法律复核路线；
- 拒绝：记录 `legal_denied`，不生成奖励请求；
- 允许但存在争议：允许流程继续，但把 claim、势力、许可证和竞争者证据写入事实；
- Agent、UI 和 D12 不能根据 `ClaimRef` 自行宣布拥有权。

竞争者是本次尝试的只读观察输入，包含 `CompetitorRef`、claim、证据引用和压力百分比。压力会提高风险阈值；高压失败记录 `competitor_interference` 和 `CompetitorDispute` 损失，不修改竞争者关系或法律状态。

## 5. 失败、损失与恢复

所有可开始的失败都追加 `WorldEventStatus.Failed`，并带 `RecoveryRoute`。风险型失败另外保存 `D12SalvageLossRecord`：损失类型、严重度、摘要、事实事件和恢复路线。损失是可追踪记录，不冒充船体、船员或库存数值。

洞穴专用 `route_cave_return_to_entrance` 是明确的回头路线；恢复不会覆盖塌方/落水失败，而是追加 `WorldEventStatus.Recovered` 并通过 `cause_ids` 引用原失败事实。恢复完成后才允许新的 attempt_id 重试。

## 6. 奖励与读档幂等

成功事件只包含 `reward_request_id` 和 `D12SalvageRewardRequest`，不包含金额、物品数量或最终奖励。RewardSystem/Inventory owner 后续负责实际结算。

`D12RuntimeSnapshot.Salvage` 保存每个地点的状态、尝试计数、失败/恢复事件引用、损失列表和唯一 RewardRequest；SaveSystem 将其写入既有 `location_json.d12.salvage`。读档时同时恢复 D12 海图快照和共享 EventLog：

- 已 `Salvaged` 的地点返回 `already_salvaged`；
- 相同 `attempt_id + save_revision` 由 EventLog 去重；
- RewardRequest 不会重新生成；
- 原失败、损失和恢复事实仍可追溯。

## 7. 代码与测试入口

- `game/Assets/Scripts/Systems/D12/D12WreckCaveSalvageContracts.cs`
- `game/Assets/Scripts/Systems/D12/D12WreckCaveSalvageCatalog.cs`
- `game/Assets/Scripts/Systems/D12/D12WreckCaveSalvageSystem.cs`
- `game/Assets/Tests/EditMode/D12WreckCaveSalvageTests.cs`
- `tools/d12_a02_runner/`

测试覆盖潮汐、装备、绳索、照明、竞争者、外部法律裁决、洞穴塌方、回头路线、损失恢复以及读档后的奖励请求幂等。
