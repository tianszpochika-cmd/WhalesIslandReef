# D14-A02 接舷、俘虏与战后处理

状态：已实现，D14-A02。`D14BoardingAftercareSystem` 是战后流程编排器，不是 Crew、Inventory、Medical、Legal 或 Relation 的第二写者。

## 1. 所有权边界

| 结果 | 裁决/写入者 | A02 行为 |
|---|---|---|
| 接舷条件、人员对抗、武器和士气、接舷结局 | `CombatSystem` / `ID14CombatBoardingAuthority` | 读取 D14-A01 快照，保存案件投影 |
| 伤亡/受伤/死亡/失踪 | `CrewSystem` / `ID14CasualtyAuthority` | 保存带来源的伤亡记录，不改 CrewAgent/健康 |
| 俘虏登记、释放、转移 | `PrisonerSystem` / `ID14PrisonerAuthority` | 保存 custody 投影，不生成 NPC 或船员 |
| 战利品和库存 | `InventorySystem` + `FactionLegalSystem` / `ID14LootAuthority` | 只保存物品结果，不写货舱、金钱或价格 |
| 救援、治疗、健康恢复 | `MedicalSystem` / `ID14MedicalAuthority` | 保存治疗/救援结果，不改健康值 |
| 审讯是否合法、信息事实 | 法律/历史审查 authority / `ID14InterrogationAuthority` | 保存信息引用；跳过时不调用审讯 authority |
| 葬礼/社区仪式 | `CommunitySystem` / `ID14FuneralAuthority` | 保存仪式结果和审查标签 |
| 船员关系数值 | D10 `RelationSystem` / `D14D10RelationAuthority` | 把已落盘 A02 事实交给 D10，保存 applied delta 投影 |

所有 authority 决定都必须携带 owner 和已存在的 `SourceEventId`；缺失、越权或未经记录的来源会拒绝，不使用隐藏默认奖励或伤亡。

## 2. 接舷条件与人员

`RequestBoarding` 需要：

- D14-A01 `D14NavalCombatState` 快照；
- 双方参与人员 ID、角色、武器、士气、健康和投降意愿；
- 战斗最后事实作为 CombatSystem 来源；
- `OperationId + SaveRevision`。

默认 CombatSystem 接舷规则：战斗状态为 `Engaged/BoardingReady/Surrendered`，双方船员存活，距离不超过 30；已投降时距离限制放宽。人员和武器只作为输入 read model，实际对抗结局由 `ID14CombatBoardingAuthority.Resolve` 返回。

支持的结局：

- `Captured`：非歼灭地控制，随后可以登记俘虏与申请合法战利品；
- `Surrendered`：对手已投降，进入 custody/救援/治疗流程，不自动发放战利品；
- `Retreated`：接舷受阻或玩家主动撤退，不生成俘虏或战利品；
- `NonLethalDisengagement`、`Released`、`Rescued`：明确的非歼灭后果。

`ProcessSurrender` 只接受 D14-A01 已经写入 `Surrendered` 的战斗快照，不能由 Agent 或 UI 直接把战斗改成投降。

## 3. 伤亡、俘虏与救援

伤亡由 `ID14CasualtyAuthority` 返回 `Wounded/Dead/Missing/Recovered` 和健康损失；A02 只绑定案件、来源和葬礼请求标志。治疗调用 `ID14MedicalAuthority.Treat`，只有 authority 返回 `Treated/Stabilized` 后，A02 才把投影改为 `Recovered`。

俘虏由 `ID14PrisonerAuthority.Register` 登记为 `Held`。A02 不复制船员、人口或角色关系，只保存 `PrisonerId/ParticipantId/CustodyOwner/Status`。`Rescue` 必须针对本案仍为 `Held` 的俘虏，并由 MedicalSystem 返回可救援 ID；成功后转为 `Rescued`，不直接生成健康数值。

## 4. 战利品与法律来源

`RequestLoot` 需要本案是 `Captured` 或 `Surrendered`，并提供已进入统一 EventLog、owner 为 `FactionLegalSystem` 的法律来源事件。最终物品、数量、合法状态和 Inventory 来源只能由 `ID14LootAuthority` 返回。

A02 只保存 `D14LootRecord` 和 `D14LootItem`。它不会调用 D3 Inventory、改 Cargo、写金钱、计算价格或把俘虏自动转成商品。投降路径默认不强制申请战利品，允许玩家选择非掠夺的非歼灭结局。

## 5. 审讯、敏感内容、葬礼和跳过

审讯和葬礼必须提供 `D14SensitiveContentTag`：

- `content_id`、历史标签、敏感标签、警告、来源说明和 tags；
- `Pending` 或 `Rejected` 不得进入内容流程；
- `ApprovedWithWarning` 必须显式 `ReviewAcknowledged`，或者设置 `SkipSensitiveContent`；
- 跳过时仍记录 `interrogation_skipped`/`funeral_skipped` 事实，但不调用相应的敏感内容 authority，也不产生虚构信息。

这样既保留历史语境和警告，又允许玩家避开敏感演出；系统不会让 Agent 生成审讯事实、死亡事实或仪式结果。

## 6. 关系后果

接舷 authority 可以提出 `D14RelationProposal`，但 `ProposedDelta` 不是最终数值。A02 先写入 `boarding_resolved` 事实，并把该 `FactEvent` 交给 `D14D10RelationAuthority`；适配器再调用 D10 `RelationSystem.ApplyFactEvent`。D10 会按关系类型边界、已知角色、事实参与者和 `event_id + save_revision` 裁决。

保存的 `D14RelationConsequenceRecord` 同时包含：

- source/target/metric；
- proposed/applied delta；
- `CauseEventId`（A02 战后事实）；
- D10 relation source event。

没有 D10 authority 时 A02 拒绝关系写入，不自行改关系值。

## 7. 事件与存档

公开入口：

- `RequestBoarding` / `ProcessSurrender`；
- `ResolveBoarding`；
- `RequestLoot`；
- `Rescue`；
- `Treat`；
- `Interrogate`；
- `HoldFuneral`。

成功结果写入 `event_d14_aftercare_<operation_id>`，带案件、Combat/Crew/Prisoner/Inventory/Medical/Legal/Community 来源和上游因果。相同 `event_id + save_revision` 在读档后只返回 `duplicate`，不会重复登记俘虏、调用 Inventory、治疗或关系应用。

`D14AftercareRuntimeSnapshot` 通过 `location_json.d14Aftercare` 保存：案件、参与人员、结局、伤亡、俘虏、战利品、治疗、救援、审讯、葬礼、关系投影、应用键和共享 EventLogSnapshot。`SaveSystem` 只复制快照，不裁决任何后果。

## 8. 验收覆盖

`D14BoardingAftercareTests` 验证：

1. 接舷人员/武器/士气输入、捕获、伤亡、俘虏、战利品、关系因果、治疗、审讯、跳过、葬礼、救援和读档；
2. 相同战利品请求读档后返回 duplicate；
3. 撤退结局不产生俘虏，敏感审查仍有边界；
4. A01 `Surrendered` 快照进入俘虏后处理，且不自动发放战利品。

验证：D14-A02 runner 3/3，D14-A01 回归 2/2，D13-S01 回归 1/1；Systems/Save.Tests 编译 0 warning / 0 error；D14 Systems 无网络或 LLM。
