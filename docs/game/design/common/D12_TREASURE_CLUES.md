# D12-A03 线索与寻宝任务

状态：已实现

D12-A03 在 D9 已冻结的 `D9TreasureQuestSystem` 上扩展线索账本，不创建第二个 QuestSystem、EventLog、Inventory、Economy、Relation 或 RewardSystem。寻宝系统只拥有任务状态、线索证据投影、失败恢复路线和奖励请求；实际库存、金钱、关系和奖励结果仍由对应 canonical owner 裁决。

## 1. 线索记录

每条 `D9TreasureClueRecord` 必须保存：

| 字段 | 语义 | 约束 |
|---|---|---|
| `ClueId` | 线索稳定 ID | Quest 内唯一 |
| `SourceKind` | `TavernRumor`、`SalvagedChart`、`CrewTestimony`、`MapDiscovery`、`DirectObservation` | 强类型枚举，不由文本猜测 |
| `SourceRef` | 来源人、地图、观察或任务引用 | 必填 |
| `EvidenceRef` | 来源证据引用 | 必填，不能只靠对白 |
| `ReliabilityPercent` | 0—100 的线索可靠度 | 由 QuestSystem 校验，Agent 不能写入最终值 |
| `RegionRef` / `SiteRef` | 目标区域和登陆点 | 必须匹配寻宝任务定义 |
| `ValidFromWorldDay` / `ValidUntilWorldDay` | 世界日时间窗 | `-1` 表示无单边界；起点不能大于终点 |
| `RequiredEquipmentIds` | 装备前置引用 | 只读取持有快照，不写库存 |
| `AlternativeRouteId` | 失败或替代路线引用 | 只引用 Quest recovery catalog |
| `RewardType` | `MapKnowledge`、`Currency`、`Item`、`Reputation` | 仅作为 RewardSystem 路由，不含金额/数量 |
| `FactEventRef` / `CauseEventIds` | 事实和因果引用 | 由 EventLog 追踪 |

内容目录中的第一张海图现在有三个来源：港口传闻、打捞海图碎片、绿礁村守礁人证词。`RecordCatalogClue` 只从任务目录读取这些结构化字段；`RecordClue` 是强逻辑写入口。`BuildClueExpressionProposal` 只生成给 Agent 的说法提案，不能改变上述字段、状态或事实日志。

## 2. 交叉验证

`CrossValidateClues` 只有在以下条件同时满足时才追加 `event_d12_treasure_cross_validate_*`：

1. 两条线索已经记录并且 ID 不同；
2. 来源类型不同；
3. 区域和登陆点相同；
4. 两条线索各自有来源/证据和事实引用；
5. 该组合尚未验证。

验证会在两条记录上互相写入 `CrossValidatedByClueIds`，并进入 `VerifiedClueIds`。验证事件的 `cause_ids` 指向两条线索事实，所以“为什么相信这个位置”可以沿 EventLog 因果链复盘。仅有 Agent 的肯定表达不会完成验证。

## 3. 时间窗、装备与失败恢复

`D9TreasureLandingRequest` 增加 `ClueId`、`RequireCrossValidation` 和 `EnforceClueWindow`：

- `RequireCrossValidation=true` 时，未验证线索产生 `clue_not_cross_validated` 失败事实，并提供 `route_cross_validate_clue`；玩家获取另一独立来源后即可恢复；
- `EnforceClueWindow=true` 时，世界日不在记录的有效窗口内产生 `clue_window_closed`，提供 `route_wait_tide` 或回港路线；
- 线索或任务的装备列表只作为读取条件，缺少装备产生 `equipment_required`，由 `InventorySystem`/Item owner 负责真正持有量；
- 失败事件不会覆盖旧线索，也不会永久锁住主流程。`Recover` 追加 `event_d9_treasure_recovered`，成功后可以按明确的低潮、向导、补装备或交叉验证路线重新尝试；
- `puzzle_gate=false` 仍写入所有相关事实，宝藏不靠不可解释谜题阻断。

## 4. 奖励与单一写者

成功登陆只生成 `D9TreasureRewardRequest`：

- `RewardType` 标明奖励类别；
- `SourceEventRef` 指向成功的 `event_d9_treasure_resolved`；
- `CauseEventIds` 和 `ClueIds` 指向选用线索；
- `Status=pending` 等待 RewardSystem/所属系统裁决。

该请求不包含金额、物品数量、库存增量、关系变化或最终发放结果。寻宝代码没有写 Inventory、Economy、Relation 或 RewardGrant 字段。

## 5. 存档与合法性检查

`D9TreasureQuestSnapshot` 新增 `Clues`、`VerifiedClueIds` 和 `CrossValidationCount`；`SaveSystem` 在既有 D9 runtime 存档下保存/恢复这些字段，未另建数据库或事件日志。读档时检查：

- Quest/线索 schema 版本和状态枚举；
- 可靠度 0—100；
- 时间窗边界；
- 来源、证据、目标和奖励类型非空/合法；
- 线索 ID、验证 ID 不重复，验证引用的线索必须存在；
- EventLog 的 `event_id + save_revision` 幂等键和奖励结算门仍由 D8 EventLog 负责。

非法线索在追加事实之前被拒绝；存档损坏不会静默生成可靠度或奖励。

## 6. 代码与测试入口

- Quest owner：`game/Assets/Scripts/Systems/D9/D9TreasureQuestSystem.cs`
- D9 存档投影：`game/Assets/Scripts/Systems/D9/D9RuntimeContracts.cs`
- Save 编解码：`game/Assets/Scripts/Systems/Save/SaveSystem.cs`
- Unity EditMode：`game/Assets/Tests/EditMode/D12TreasureClueTests.cs`
- 断网纯 C# 烟测：`tools/d12_a03_smoke/`

覆盖项：多来源登记、来源类型限制、交叉验证因果链、未验证失败与恢复、时间窗失败与恢复、装备读取门槛、奖励请求追溯、非法可靠度拒绝、读档后线索/验证/奖励保持和重复发现保护。

本任务没有调整既有价格、奖励金额、物品数量、战斗数值或经济公式。
