# 第 7 章开发任务提示词：D14 海战、特殊武器、奖励与势力后果

> 本章前置：D13-S01 通过。执行顺序：`D14-A01—D14-A06` 可按模块并行，全部完成后执行 `D14-S01`。

## D14-A01：海战裁决

<details>
<summary>展开/复制：D14-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A01。

任务编号：D14-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 事件/配置成果、D9 船舶成果、D13 经济/资产成果和当前 Combat/ShipSystem。

执行内容：实现距离、风向、角度、航速、炮位、弹种、装填、命中、船体分区损伤、火灾、进水、撤退、投降和战斗日志。战斗公式由 Combat/ShipSystem 写。

任务约束：
- Combat/ShipSystem 是战斗公式、损伤和命中结果的唯一写者；
- UI 只能发起瞄准/开火请求和显示结果；
- Agent 不能直接裁决命中、伤害或投降；
- 战斗结果必须写入事件和存档；
- 必须支持撤退和投降；
- 不得在 Systems 中加入网络或 LLM。

验收：距离、风向、角度、航速、炮位、弹种、装填和船体损伤可追踪，火灾/进水/撤退/投降可保存。

结果必须保存到：
- 主成果：海战裁决代码、测试，以及 docs/game/design/common/D14_NAVAL_COMBAT_RULES.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-A02：接舷、俘虏与战后处理

<details>
<summary>展开/复制：D14-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A02。

任务编号：D14-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01、D14-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D14_NAVAL_COMBAT_RULES.md、D10 关系成果、D13 法律/资产成果和当前 Crew/Combat 代码。

执行内容：实现接舷条件、人员、武器、士气、伤亡、俘虏、投降、救援、审讯、战利品、葬礼、治疗和船员关系后果。必须有撤退和非歼灭结局。

任务约束：
- 伤亡、俘虏、战利品和治疗由所属系统裁决；
- UI 和 Agent 不能直接发放战利品或改变伤亡；
- 必须支持救援、投降、撤退和非歼灭结局；
- 敏感内容必须有历史标签、警告和跳过；
- 关系后果必须绑定事实事件；
- 不得在 Systems 中加入网络或 LLM。

验收：接舷、投降、俘虏、救援、治疗、战利品和葬礼有可保存后果，玩家可选择撤退或非歼灭结局。

结果必须保存到：
- 主成果：接舷/俘虏/战后处理代码、测试，以及 docs/game/design/common/D14_BOARDING_PRISONERS_AFTERCARE.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-A03：特殊武器与船体模块

<details>
<summary>展开/复制：D14-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A03。

任务编号：D14-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 内容 ID/目录成果、D9 船舶成果、D14_NAVAL_COMBAT_RULES.md 和当前 Item/ShipModule 代码。

执行内容：实现特殊炮架、火枪、信号器具、捕缆/救援工具、历史武器和独特船体模块的唯一 ID、来源、获取、维护、强度边界、损坏、出售、收缴和恢复。

任务约束：
- 每个武器和模块必须使用统一 ID、来源和授权记录；
- 强度边界由 Combat/ShipSystem 裁决；
- RewardSystem 或所属系统裁决获取和奖励；
- UI 不能直接装配、修复或生成武器；
- Agent 不能直接改变伤害或耐久；
- 历史武器必须有历史标签和审查状态；
- 不得在 Systems 中加入网络或 LLM。

验收：特殊武器和模块可获得、装配、损坏、维修、出售、收缴和恢复，唯一 ID、来源和强度边界完整。

结果必须保存到：
- 主成果：特殊武器/船体模块代码、数据、测试，以及 docs/game/design/common/D14_SPECIAL_WEAPONS_MODULES.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-A04：奖励系统

<details>
<summary>展开/复制：D14-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A04。

任务编号：D14-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 WorldEvent/Content ID/存档成果、D12 收藏成果和当前 RewardSystem。

执行内容：实现普通、专业、稀有、独特和世界级奖励；RewardSystem 唯一写入；支持唯一性、防重复领取、来源日志、丢失恢复和任务奖励请求。

任务约束：
- RewardSystem 是奖励唯一写者；
- 任务只能提出奖励请求，不能直接发放；
- AgentReply 不能包含最终 reward；
- 奖励必须绑定来源事件、内容 ID 和领取记录；
- 读档不能重复领取；
- 丢失恢复必须有审计记录；
- 不得在 Systems 中加入网络或 LLM。

验收：不同稀有度奖励可发放，唯一奖励不会重复，来源日志完整，丢失恢复可控，保存读档一致。

结果必须保存到：
- 主成果：奖励系统、测试，以及 docs/game/design/common/D14_REWARD_SYSTEM.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-A05：势力、私掠、封锁与法律

<details>
<summary>展开/复制：D14-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A05。

任务编号：D14-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D13 经济/资产成果、D14 战斗/奖励成果和当前 Faction/Legal 代码。

执行内容：实现声望、战争、停战、私掠证、检查、通缉、护航、港口禁入、扣押、罚金、赎金、封锁和外交后果。

任务约束：
- Faction/LegalSystem 唯一写入声望、法律和港口许可状态；
- 战斗只提交事实事件，不直接写外交结果；
- UI 只能显示法律和势力状态；
- Agent 只能提出外交或谈判意图；
- 私掠、封锁、扣押和罚金必须有来源、时间和证据；
- 不得在 Systems 中加入网络或 LLM。

验收：声望、战争、停战、私掠、检查、通缉、护航、禁入、扣押、罚金、赎金和封锁会产生可追踪后果。

结果必须保存到：
- 主成果：势力/法律系统、测试，以及 docs/game/design/common/D14_FACTION_PRIVATEERING_LAW.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-A06：宗教、社群与历史审查接入

<details>
<summary>展开/复制：D14-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D14-A06。

任务编号：D14-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D10 社会/角色成果、D12 社群成果、D14 势力/法律成果和历史审查设计。

执行内容：实现节期、饮食、建筑开放、葬礼、誓言、信任、社群礼仪、内容警告和跳过选项；所有敏感内容带历史标签和审查状态。

任务约束：
- 宗教和社群状态必须有明确写者；
- 内容表现不等于现实价值判断，必须有历史标签和上下文；
- 敏感内容必须支持警告、跳过和表现等级；
- Agent 只能生成表达，不能裁决宗教、法律或声望结果；
- 不得无来源创建历史细节；
- 不得在 Systems 中加入网络或 LLM。

验收：节期、饮食、建筑、葬礼、誓言和礼仪会影响社群交互，敏感内容有标签、警告、跳过和审查状态。

结果必须保存到：
- 主成果：宗教/社群/历史审查接入代码和说明，以及 docs/game/design/common/D14_RELIGION_HISTORY_REVIEW.md；
- 交接报告：docs/game/design/common/handoffs/D14/D14-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D14-S01：战斗与势力集成

<details>
<summary>展开/复制：D14-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D14-S01。

任务编号：D14-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D14-A01、D14-A02、D14-A03、D14-A04、D14-A05、D14-A06 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D14 各主成果文件和 docs/game/design/common/handoffs/D14/ 下全部交接报告。

集成验收：战斗会改变船体、船员、货物、金钱、法律、声望、任务和关系；特殊武器能获得/装配/损坏/维修；私掠或违法行为改变港口和势力；读档不重复奖励。

任务约束：
- 不得复制战斗、奖励、法律、势力或关系逻辑；
- UI 只能读取结果和事件；
- Agent 只能提出战术或外交意图，不能裁决伤害、奖励或法律后果；
- 必须检查存档、唯一写者和历史审查；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D14_ACCEPTANCE_REPORT.md；
- 集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D14/D14-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D14 是否通过、遗留问题和是否允许 D15 开始批量内容生产。
```
</details>
