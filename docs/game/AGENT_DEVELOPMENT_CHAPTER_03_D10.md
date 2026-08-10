# 第 3 章开发任务提示词：D10 船员社会、关系与角色表达

> 本章前置：D9-S02 通过。执行顺序：`D10-A01—D10-A06` 可按模块并行，全部完成后执行 `D10-S01`。

## D10-A01：多维关系系统

<details>
<summary>展开/复制：D10-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A01。

任务编号：D10-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 数据/事件成果、D9 垂直切片验收报告，以及当前 Crew/Relation 代码。

执行内容：实现 affinity、trust、loyalty、fear、respect、attachment、jealousy、debt、romance 关系边。每次关系变化必须绑定 cause_event_ids，关系面板可以解释变化原因。

任务约束：
- RelationSystem 是关系数值唯一写者；
- UI 只能读取关系快照；
- Agent 只能提出关系变化意图，不能直接改关系数值；
- 关系变化必须来自事实事件；
- 不得用静态全局变量或私有 JSON 绕过存档；
- 不得在 Systems 中加入网络或 LLM。

验收：关系边可保存、读档、追溯 cause_event_ids；关系面板能显示变化原因；未知角色和缺失事件有明确处理。

结果必须保存到：
- 主成果：关系系统、测试，以及 docs/game/design/common/D10_RELATION_MODEL.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-A02：船员岗位与行为升级

<details>
<summary>展开/复制：D10-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A02。

任务编号：D10-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 数据边界、D9 垂直切片成果、当前 Crew/Job/Agent 行为代码。

执行内容：实现轮班、岗位效率、疲劳、健康、拒绝执行、消极怠工、请假、调岗、离队和叛变前兆。行为必须是确定性状态机，LLM 只生成表达。

任务约束：
- 行为状态机由角色/船员强逻辑系统裁决；
- LLM 不能直接决定拒绝、离队、叛变或岗位效果；
- UI 不能修改疲劳、健康、岗位和工资；
- 所有状态必须可保存、读档和测试；
- AgentCore 不得依赖 Unity 或网络。

验收：岗位和轮班影响效率，疲劳和健康影响行为，拒绝/请假/调岗/离队有明确状态和事件记录。

结果必须保存到：
- 主成果：船员岗位与行为代码、测试，以及 docs/game/design/common/D10_CREW_BEHAVIOR_STATE.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-A03：好感、爱情和冲突事件

<details>
<summary>展开/复制：D10-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A03。

任务编号：D10-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过；D10-A01 的关系模型必须可用

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D10_RELATION_MODEL.md、当前任务/事件/角色代码。

执行内容：实现认识 → 信任 → 亲密 → 承诺 → 冲突/稳定/分离阶段。支持玩家—船员和船员—船员关系，包含礼物、守约、救援、偏袒、嫉妒、争执、夜话和个人任务。

任务约束：
- 不做露骨内容；
- 对话不能直接跳过逻辑条件；
- 玩家可以拒绝恋爱路线；
- 关系和阶段由强逻辑系统根据事实事件裁决；
- Agent 只能提出表达和行动建议；
- UI 不得直接改变关系、承诺或冲突结果；
- 所有事件必须可存档和追溯。

验收：关系阶段正确推进或回退，礼物/守约/救援/偏袒/嫉妒等有因果事件，玩家拒绝路线后不会被强制进入恋爱。

结果必须保存到：
- 主成果：关系阶段、恋爱和冲突事件代码、测试，以及 docs/game/design/common/D10_ROMANCE_CONFLICT_EVENTS.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-A04：玩家身份和性别表达

<details>
<summary>展开/复制：D10-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A04。

任务编号：D10-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 数据模型、当前 Player/Identity/Save 代码。

执行内容：实现创建时性别表达、姓名、称谓、年龄段、文化出身和身份历史；支持港口改称谓、外观和证件记录，但不抹除债务、法律、声望和关系历史。

任务约束：
- 性别表达、姓名和称谓必须是可保存的数据；
- 改称谓或外观不等于删除历史；
- 不得通过 UI 绕过法律、债务、声望和关系数据；
- 不得让 Agent 直接修改身份历史；
- 必须考虑本地化和长文本；
- 不得在 Systems 中加入网络或 LLM。

验收：创建、修改、保存、读档、港口检查和历史保留全部有效。

结果必须保存到：
- 主成果：玩家身份数据和测试，以及 docs/game/design/common/D10_PLAYER_IDENTITY_EXPRESSION.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-A05：发型、服装和装扮

<details>
<summary>展开/复制：D10-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A05。

任务编号：D10-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 内容 ID/存档成果、当前角色外观和装备代码。

执行内容：实现身体、发型、帽子、上衣、下装、外套、鞋、手套、饰品、职业装备和伤病表现的组合；支持裁缝购买、试穿、染色、维修、文化/天气/职业标签和存档。

任务约束：
- 外观组合使用统一 outfit/asset ID；
- UI 只能提出试穿或购买请求，装备系统裁决库存和所有权；
- 不得抹除伤病或历史表现；
- 不得把最终价格、奖励或文化限制交给 Agent；
- 资源缺失时提供占位和明确错误；
- 不得在 Systems 中加入网络或 LLM。

验收：服装组合、试穿、购买、染色、维修、标签、保存和读档可用，非法组合有明确提示。

结果必须保存到：
- 主成果：外观/服装代码、样例、测试，以及 docs/game/design/common/D10_APPEARANCE_OUTFIT.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-A06：酒馆社交场景

<details>
<summary>展开/复制：D10-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D10-A06。

任务编号：D10-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9 垂直切片成果、D10 关系/角色成果和当前 Tavern/UI 代码。

执行内容：实现酒馆招募、休息、吃饭、饮酒、传闻、牌局入口、舞会/夜话和副业公告板。酒精必须影响健康、疲劳和冲突，不只是一项消费动画。

任务约束：
- 酒馆只发起行为请求，健康、疲劳、关系和金钱由所属系统裁决；
- 传闻必须标记可靠度和来源；
- 牌局入口不能复制牌局结算逻辑；
- 不得让 Agent 直接发放副业收入或改变关系；
- 敏感内容必须有警告或跳过；
- 不得在 Systems 中加入网络或 LLM。

验收：招募、休息、吃饭、饮酒、传闻、夜话和副业入口可用，酒精产生可追溯的健康/疲劳/冲突后果。

结果必须保存到：
- 主成果：酒馆场景、交互、测试，以及 docs/game/design/common/D10_TAVERN_SOCIAL_SCENE.md；
- 交接报告：docs/game/design/common/handoffs/D10/D10-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D10-S01：角色社会集成

<details>
<summary>展开/复制：D10-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D10-S01。

任务编号：D10-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-A01、D10-A02、D10-A03、D10-A04、D10-A05、D10-A06 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D10 各主成果文件和 docs/game/design/common/handoffs/D10/ 下全部交接报告。

执行完整验收流程：
- 玩家拖欠工资；
- 救援船员；
- 公开偏袒一次；
- 关系面板显示原因；
- 船员行为发生变化；
- 玩家更换服装和称谓后进入港口检查；
- 保存和读档后关系与外观一致。

任务约束：
- 不得复制关系、行为或外观领域逻辑；
- 不得为了通过验收删除失败分支；
- UI 只能读取快照和事件；
- Agent 只能提议，不能裁决关系、工资、行为或身份结果；
- 必须检查单一写者、契约和存档一致性；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D10_ACCEPTANCE_REPORT.md；
- 集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D10/D10-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D10 是否通过、遗留问题和是否允许 D13 使用关系/角色数据。
```
</details>
