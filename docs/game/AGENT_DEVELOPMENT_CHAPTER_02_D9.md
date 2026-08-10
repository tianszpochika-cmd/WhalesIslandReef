# 第 2 章开发任务提示词：D9 港口—船—近海—船员垂直切片

> 本章实际执行目标：完成一条可玩的最小航程。前置条件：D8-S02 已通过。执行顺序：`D9-S01 → D9-A01—D9-A07 → D9-S02`。

## D9-S01：冻结垂直切片范围

<details>
<summary>展开/复制：D9-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步架构 Agent。请在当前项目仓库中直接完成 D9-S01，不要只给计划。

任务编号：D9-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、docs/game/REQUIREMENTS.md、docs/game/design/DESIGN_BOUNDARIES.md、docs/game/design/DATA_MODELS.md、D8 验收报告和当前 D7 代码。

必须固定垂直切片范围：
- 一座港口；
- 一个村落；
- 一片近海鱼场；
- 一艘小船；
- 10 个商品；
- 1 个核心船员 Agent；
- 1 个天气事件；
- 1 个小型寻宝；
- 1 场可撤退海战。

执行内容：
- 冻结场景边界；
- 冻结 Prefab 入口；
- 冻结数据样例；
- 冻结事件链；
- 冻结 UI 页面清单；
- 冻结性能预算；
- 编写 D9 验收脚本。

任务约束：
- 只负责垂直切片边界和集成入口，不实现各领域全部逻辑；
- 不删除 D7 已有行为；
- 跨端数据必须使用 contracts；
- 不得让 UI、Agent 或网络层拥有强逻辑裁决权；
- 不得在 Systems 中加入网络或 LLM；
- 不得让多个系统重复定义同一个实体或事件。

验收标准：所有 D9 Agent 都能从冻结文件找到自己的入口、样例、测试边界和性能预算。

结果必须保存到：
- 主成果：docs/game/design/common/D9_VERTICAL_SLICE_FREEZE.md；
- 验收脚本：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D9/D9-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A01：港口与船舶整备

<details>
<summary>展开/复制：D9-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A01。

任务编号：D9-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8 数据/契约成果、当前港口和船舶代码。

执行内容：实现港口进入、泊位、市场、酒馆、船坞、补给、修船、招募、工资结算和离港。船舶必须显示重量、吃水、船员、货物、补给、损伤、锚泊和改装。

验收：买货导致重量变化；修船消耗材料和钱；工资结算影响船员；离港后状态进入航行。

任务约束：
- 船舶状态只能由船舶/整备系统写入；
- UI 只能读取状态快照，不能直接改钱、重量、损伤或工资；
- 不得复制 D8 的实体、契约或所有权逻辑；
- 不得在 Systems 中加入网络或 LLM；
- 只修改港口、船舶整备、相关 UI、测试和文档。

结果必须保存到：
- 主成果：港口/船舶整备代码、测试，以及 docs/game/design/common/D9_PORT_SHIP_PREP.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A02：近海钓鱼最小闭环

<details>
<summary>展开/复制：D9-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A02。

任务编号：D9-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8_DATA_BOUNDARY_FREEZE.md、当前 EcologySystem、EconomySystem、Inventory/Cargo 和 UI。

执行内容：
- 实现一个鱼群；
- 实现一种渔具；
- 实现一个捕捞点；
- 实现一次捕捞结算；
- 实现鲜鱼处理；
- 实现港口出售或烹饪；
- 鱼群位置由 EcologySystem 写；
- 价格由 EconomySystem 写；
- UI 只展示事件和状态。

禁止：
- 不得把钓鱼结果写在 UI 或任务脚本中；
- 不得用随机文本代替鱼获数据；
- 不得让 Agent 直接裁决鱼获数量或价格；
- 不得在 Systems 中加入网络或 LLM。

验收：捕捞、鱼获库存、处理、出售/烹饪、保存和读档完整可用，鱼群和价格的写者唯一。

结果必须保存到：
- 主成果：钓鱼闭环代码、测试，以及 docs/game/design/common/D9_FISHING_VERTICAL_SLICE.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A03：10 商品贸易切片

<details>
<summary>展开/复制：D9-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A03。

任务编号：D9-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8 内容 ID/配置成果和当前 Inventory/Cargo/Economy 代码。

执行内容：实现 10 个商品的库存、重量、体积、价格、需求、税费、买入、卖出、缺货和保存。价格变化必须显示原因：库存、需求、天气、玩家交易量或势力政策。

任务约束：
- 价格由 EconomySystem 唯一裁决；
- UI 只能显示价格原因，不能写价格；
- 交易必须进入事件/存档日志；
- 不得把商品字段私藏在 UI、任务脚本或 AgentReply；
- 不得修改无关金融或资产系统；
- 不得在 Systems 中加入网络或 LLM。

验收：10 个商品可买卖、库存和重量正确、缺货可见、价格原因可解释、保存读档后状态一致。

结果必须保存到：
- 主成果：贸易切片代码、商品样例、测试，以及 docs/game/design/common/D9_TRADE_VERTICAL_SLICE.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A04：一个完整船员 Agent

<details>
<summary>展开/复制：D9-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A04。

任务编号：D9-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8 AgentReply/事件成果、当前 AgentCore 和船员代码。

执行内容：实现一个船员的 Persona、岗位、短期记忆、事件摘要、对话、命令提议、好感变化提议和逻辑裁决。必须提供无模型/断网模板回复。

验收：船员能提出航线或岗位建议，但不能直接改变航速、钱、关系或任务结果；关系变化可以追溯到事实事件。

任务约束：
- LLM 只能生成表达和提议；
- 强逻辑系统裁决航速、金钱、关系、任务和奖励；
- AgentCore 不得依赖 Unity、网络或第三方库；
- 不得把 Persona 当成数据唯一写者；
- 所有关系变化必须绑定 cause_event_ids；
- 必须有无模型模板回复。

结果必须保存到：
- 主成果：AgentCore/船员 Agent 代码、测试，以及 docs/game/design/common/D9_CREW_AGENT_VERTICAL_SLICE.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A05：天气与海况切片

<details>
<summary>展开/复制：D9-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A05。

任务编号：D9-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8 事件/配置成果和当前 EnvironmentSystem、ShipSystem、EcologySystem。

执行内容：实现晴天、雨天和风暴三种状态，以及风、能见度、海流、疲劳、船体风险和鱼群影响。天气状态由 EnvironmentSystem 写，船舶和生态只读取。

任务约束：
- EnvironmentSystem 是天气状态唯一写者；
- 船舶和生态系统只能读取天气快照；
- UI 只能显示天气和影响；
- 不得让 AgentReply 直接修改天气；
- 不得在 Systems 中加入网络或 LLM；
- 所有影响必须可测试、可保存和可恢复。

验收：三种天气能改变规定的海况、疲劳、船体风险或鱼群结果，读档后天气状态和影响不重复结算。

结果必须保存到：
- 主成果：天气/海况代码、测试，以及 docs/game/design/common/D9_WEATHER_SEA_STATE.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A06：小型探索/寻宝任务

<details>
<summary>展开/复制：D9-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A06。

任务编号：D9-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、D8 事件/任务/奖励成果和当前 Quest、Island、Exploration 代码。

执行内容：实现一个包含传闻、线索可靠度、装备前置、登陆、危险、发现、奖励和失败恢复的任务。

任务约束：
- 不可使用必须猜谜的硬门槛；
- 线索可靠度、装备、危险、奖励和失败必须进入统一事件与存档；
- 任务系统不得复制经济、库存或关系状态；
- Agent 只能提出线索表达，不能直接发奖励或改任务结果；
- 不得在 Systems 中加入网络或 LLM。

验收：任务有成功、失败、恢复和替代路线，玩家不因无法猜谜而被永久阻断，保存读档后因果链一致。

结果必须保存到：
- 主成果：任务代码、样例、测试，以及 docs/game/design/common/D9_TREASURE_VERTICAL_SLICE.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-A07：垂直切片 UI、音频与日志

<details>
<summary>展开/复制：D9-A07 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D9-A07。

任务编号：D9-A07
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md、当前事件、状态快照、UI、音频和日志代码。

执行内容：实现航行 HUD、船舶状态、鱼获、市场、船员对话、任务日志、天气提示和关键音效。

任务约束：
- UI 只能消费事件和状态快照，不得写系统数值；
- 音频只能响应事件或状态，不得修改游戏逻辑；
- 日志必须读取统一事件事实，不得创建私有事实；
- 不得在 UI 中计算价格、伤害、奖励、关系或天气结果；
- 不得在 Systems 中加入网络或 LLM；
- 必须覆盖 D9 垂直切片的关键流程。

验收：HUD、市场、鱼获、船员、任务、天气提示和关键音效可在完整航程中正确刷新，读档后不产生重复 UI 结算。

结果必须保存到：
- 主成果：D9 UI/音频/日志代码、测试，以及 docs/game/design/common/D9_UI_AUDIO_LOG.md；
- 交接报告：docs/game/design/common/handoffs/D9/D9-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D9-S02：垂直切片集成验收

<details>
<summary>展开/复制：D9-S02 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D9-S02，不要只写验收意见。

任务编号：D9-S02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-A01、D9-A02、D9-A03、D9-A04、D9-A05、D9-A06、D9-A07 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9_VERTICAL_SLICE_FREEZE.md，以及 docs/game/design/common/handoffs/D9/ 下全部交接报告。

完整执行流程：
整备 → 装货 → 配员 → 出航 → 钓鱼 → 遇雨/风暴 → 船员讨论 → 登陆寻宝 → 返回交易 → 发工资 → 修船 → 保存 → 读档。

验收重点：
- 没有重复结算；
- 没有丢存档；
- 没有跨系统私有字段；
- game/Assets/Scripts/Systems/ 中没有网络或 LLM 代码；
- 船、鱼获、商品、天气、船员、任务和 UI 状态一致；
- D9 通过后 D10、D11、D12 才能进入 READY。

任务约束：
- 不得为了通过验收删除 D7 行为；
- 不得跳过失败测试；
- 不得重新复制一套领域逻辑；
- 不得私自开始 D10-D16 的实现；
- 所有问题必须记录责任模块和修复建议。

结果必须保存到：
- 主成果：docs/game/design/common/D9_ACCEPTANCE_REPORT.md；
- 验收脚本和集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D9/D9-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D9 是否通过，以及是否允许 D10、D11、D12 并行开始。
```
</details>
