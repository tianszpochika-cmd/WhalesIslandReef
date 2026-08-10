# D14 综合验收报告

任务：D14-S01  
日期：2026-08-11  
结论：**D14 通过**

## 验收范围

本轮集成只拥有跨域因果账本，不复制战斗、战后、特殊装备、奖励、势力/法律、宗教/社群或 D10 关系状态。`D14S01IntegrationSystem` 消费已存在的 Combat/Aftercare 事实，调用各 owner authority，并把 authority 来源事件写入集成事件和存档。

## 集成流程结果

| 验收项 | 来源事实 | 唯一写入者 | 结果 |
|---|---|---|---|
| 战斗改变船体 | CombatSystem 事实 | ShipSystem authority | 通过 |
| 战斗改变船员 | CombatSystem/Aftercare 事实 | CrewSystem authority | 通过 |
| 战后货物结算 | Aftercare 事实 | InventorySystem authority | 通过 |
| 战斗/战后金钱后果 | Combat/Aftercare 事实 | EconomySystem authority | 通过 |
| 违法/私掠影响港口与势力 | 战斗/战后事实、法律证据 | FactionLegalSystem authority | 通过 |
| 任务事实 | 战斗/战后事件 | QuestSystem authority | 通过 |
| 关系后果 | 战斗/战后事件 | RelationSystem authority | 通过 |
| 特殊武器获取、装配、损坏、维修 | 装备来源事实 | D14-A03/所属装备 authority | 通过 |
| 奖励来源与读档幂等 | Combat/Aftercare 事实 | RewardSystem authority | 通过 |

测试中的 owner fixture 只模拟各所属系统的最终写入，集成层仅记录 owner 返回的 `SourceEventId`；真实 A01—A06 系统的领域测试继续作为独立门禁。

## 因果与 Agent 边界

- 每次集成操作必须消费已存在的 `FactEvent`，并将来源、authority 事件和集成事件串入 `cause_ids`/`input_event_ids`。
- `OperationId + SaveRevision` 进入集成账本；读档后重复调用在再次触发 owner 前返回 `Duplicate`。
- Agent/AgentReply 不能提交跨域最终后果，只能提出战术或外交意图。
- 集成层不含伤害、船体、船员、货物、金额、声望、港口、任务、关系、装备强度或奖励值。
- 历史/敏感内容仍由 A02/A06 的审查、警告和跳过机制负责；S01 不绕过审查。

## 存档

新增 `location_json.d14S01Integration`，保存集成版本、账本、来源事件、authority 事件、奖励 grant 引用、法律行为、应用键和共享 `EventLogSnapshot`。保存/读取只复制快照，不裁决领域结果。

## 验证结果

- D14-S01 集成测试：2/2 PASS。
- D14-A01：2/2 PASS；D14-A02：3/3 PASS；D14-A03：2/2 PASS；D14-A04：2/2 PASS；D14-A05：2/2 PASS；D14-A06：2/2 PASS。
- D13-S01 前置回归：1/1 PASS。
- `Whalebone.Systems`、`Save.Tests`、`AgentCore`：0 警告、0 错误。
- Content Registry：124 entries / 13 prefixes，通过。
- D14 Systems 网络/LLM 红线：通过。

## 最终结论

**D14：通过。**

遗留问题：无阻塞性遗留。后续生产接入真实 Ship/Crew/Inventory/Economy/Quest/Relation/Reward adapter 时，必须保持本报告的 owner、来源事件和幂等接口；D14 当前仍是代表性内容规模，不代表 D15 的批量内容数量已完成。

**允许 D15 开始批量内容生产：是。** D15 新内容必须使用统一 Content ID、历史来源/审查状态、所属系统 owner、来源事件和测试 ID，不得在批量内容中新增第二份裁决逻辑。

