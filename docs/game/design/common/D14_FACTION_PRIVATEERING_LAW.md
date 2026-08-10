# D14-A05 势力、私掠与法律系统

状态：已实现并通过纵切片验收。

## 1. 边界与唯一写者

`D14FactionLegalSystem` 是 D14-A05 的 `FactionLegalSystem` 唯一写者，负责：

- 势力声望、势力关系、战争与停战；
- 私掠证的发证、期限、撤销语义；
- 港口开放、限制、禁入、封锁和护航要求；
- 检查、通缉、扣押、罚金、赎金和结案状态；
- 护航结果带来的声望后果；
- 外交/谈判意图的 pending 记录；
- 共享 `EventLog`、快照和审计。

战斗系统只提交战斗/护航事实；Inventory/Cargo 只返回货物清单与扣押决定；EconomySystem 只评估罚金、赎金及结算。A05 不直接写金钱、库存、价格、战斗结果或奖励。UI/Agent 只能读取快照和提交外交意图。

## 2. 确定性规则

| 领域 | 规则 |
|---|---|
| 声望 | 每次请求必须有 EventLog 来源、时间、证据和理由；结果限制在 -100..100。 |
| 战争/停战 | 战争登记为 `AtWar`；仅现存战争关系可进入 `Truce`；同一操作键读档后返回 duplicate。 |
| 私掠证 | 发证势力声望至少 30；证书有唯一 ID、持有人、目标、起止日；世界日超过到期日不可使用。 |
| 检查 | 港口/控制势力必须匹配；货单必须来自 InventorySystem；Inventory authority 才能裁决违禁和扣押。 |
| 通缉 | 严重度 1—5；同一对象保留最高当前等级；清除需要 EconomySystem 的罚金/赎金结算事实。 |
| 护航 | 只读取 EscortSystem 来源事实的结果码；`escort_success/success/completed/protected_cargo` 为成功，否则为失败，声望分别 +8/-5。 |
| 港口许可 | `Denied`/`Blockaded`/高通缉拒绝入港；`Restricted` 要求控制势力声望不低于 0；`EscortRequired` 必须先完成护航。 |
| 封锁 | 目标可为势力或港口；宣布将对应关系/港口设为 Blockaded，解除后港口回 Open、关系回 Neutral；两次操作必须分别有来源、时间、证据。 |
| 罚金/赎金 | 金额仅由 EconomySystem 评估；A05 只保存义务和结案状态，罚金为 Paid、赎金为 Released。 |

这些规则不使用随机数、网络或 LLM；Agent 不能把摘要中的建议变成声望、法律或奖励事实。

## 3. 事实、证据与恢复

所有法律结果都追加 `event_d14_faction_legal_*` 事实，并保留 `source_event_id`、`evidence_event_ids`、`cause_ids`、`world_day` 和 `save_revision`。扣押、通缉、封锁和禁入审计提供恢复路线：

- `route_d14_seizure_legal_release`：引用 Inventory 释放/复核事实；
- `route_d14_wanted_appeal_or_settlement`：引用 Economy 结算或法律申诉事实；
- `route_d14_blockade_diplomatic_review`：引用外交复核/解除封锁事实。

恢复路线是可追踪的法律出口，不在系统内偷偷返还库存、现金或声望。

## 4. 内容与历史审查

势力基本定义复用 D4 `D4FactionCatalog`，A05 不复制第二份势力名录；A05 只添加来源、历史标签和审查状态。新增内容 ID：

- `asset_privateer_letter_of_marque`；
- `asset_blockade_order`；
- `asset_cargo_seizure_writ`。

三项内容均为项目自有历史风格样例，带 `historical_tag`、`reviewer`、`source_refs` 和内部授权，不代表未经审查的史实断言。

## 5. 存档与测试

`D14FactionLegalRuntimeSnapshot` 序列化到 `location_json.d14FactionLegal`，包含声望、关系、私掠证、港口、通缉、检查、扣押、金融案件、护航、封锁、外交意图、审计、应用键和共享 EventLog。SaveSystem 只验证、复制和恢复，不拥有法律状态写权限。

`D14FactionLegalSystemTests` 覆盖：

1. 声望 → 战争 → 停战 → 私掠证 → 护航；
2. 货物检查触发通缉、扣押和罚金；
3. 罚金结算、通缉清除、赎金释放；
4. 港口封锁/解除和入港判断；
5. Agent 只能提交外交意图，缺少证据的直改请求被拒；
6. 保存/读档后法律状态、来源、审计和重复操作一致。

验收：D14-A05 runner 2/2；Systems、Save.Tests 0 warning/0 error；内容注册表检查通过；D14 A05 Systems 无网络或 LLM。
