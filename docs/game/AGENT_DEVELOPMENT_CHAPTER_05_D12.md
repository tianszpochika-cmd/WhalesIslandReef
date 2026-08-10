# 第 5 章开发任务提示词：D12 探索、寻宝、部落交易、牌局与收藏

> 本章前置：D9-S02 通过。执行顺序：`D12-A01—D12-A08` 可按模块并行，全部完成后执行 `D12-S01`。

## D12-A01：海图、测深与发现系统

<details>
<summary>展开/复制：D12-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A01。

任务编号：D12-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 内容/事件成果、D9 垂直切片成果和当前 Map/Exploration 代码。

执行内容：实现海图分层、未知海岸、暗礁、浅滩、鱼场、测深绳、望远镜、海图可信度和发现标记。地图信息必须由发现、交易、任务或社群知识逐步获得。

任务约束：
- 地图发现状态由探索系统唯一写入；
- UI 只能显示已经获得的信息；
- Agent 只能提出传闻或知识，不得直接解锁地图；
- 海图可信度必须有来源和证据；
- 不得使用私有地图字段绕过存档和事件；
- 不得在 Systems 中加入网络或 LLM。

验收：未知海岸、暗礁、浅滩和鱼场可以通过不同来源逐步发现，测深和望远镜有可解释作用，读档后发现状态一致。

结果必须保存到：
- 主成果：海图/测深/发现代码、测试，以及 docs/game/design/common/D12_MAP_DISCOVERY.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A02：沉船、洞穴与打捞

<details>
<summary>展开/复制：D12-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A02。

任务编号：D12-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D12-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D12_MAP_DISCOVERY.md、D8 事件/奖励/存档成果和当前沉船/洞穴代码。

执行内容：实现沉船位置、潮汐窗口、打捞工具、绳索、照明、洞穴回头路线、塌方/落水风险、竞争者、法律归属和失败恢复。

任务约束：
- 沉船、洞穴和打捞状态必须有唯一写者；
- 法律归属由法律/势力系统裁决；
- 打捞失败必须有恢复或损失记录；
- UI 不得直接生成物品或奖励；
- Agent 不得直接裁决沉船归属；
- 不得在 Systems 中加入网络或 LLM。

验收：潮汐、装备、风险、竞争者和法律归属影响打捞，失败可恢复或记录损失，读档不重复生成宝物。

结果必须保存到：
- 主成果：沉船/洞穴/打捞代码、测试，以及 docs/game/design/common/D12_WRECK_CAVE_SALVAGE.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A03：线索与寻宝任务

<details>
<summary>展开/复制：D12-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A03。

任务编号：D12-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D12-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D12_MAP_DISCOVERY.md、D8 事件/奖励成果和当前 Quest/Log 代码。

执行内容：实现线索来源、可靠度、交叉验证、时间窗口、装备前置、替代路线和奖励类型。宝藏不能靠无法理解的谜题阻断主流程。

任务约束：
- 线索必须记录来源、可靠度和交叉验证；
- 寻宝任务不得复制库存、经济或关系状态；
- 奖励只能由 RewardSystem 或所属系统裁决；
- Agent 只能提出线索表达；
- 必须支持失败、替代路线和恢复；
- 不得在 Systems 中加入网络或 LLM。

验收：玩家能从多个来源获得和验证线索，装备和时间窗口有效，谜题失败不会永久阻断主流程，奖励可追溯。

结果必须保存到：
- 主成果：线索/寻宝任务代码、测试，以及 docs/game/design/common/D12_TREASURE_CLUES.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A04：部落和地方社群交易

<details>
<summary>展开/复制：D12-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A04。

任务编号：D12-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 关系/事件/内容成果、D9 垂直切片成果和当前社群/贸易代码。

执行内容：实现礼仪、语言/知识门槛、物物交换、社群库存、保护区、土地边界、误解、信任、帮助、拒绝和长期声望。社群不是单一商店皮肤。

任务约束：
- 社群库存、边界和声望必须有明确写者；
- 交易必须尊重保护区、土地边界和文化礼仪；
- Agent 只能生成表达或提出交易意图；
- 物物交换结果由经济/社群系统裁决；
- 不得把社群简化为换皮商店；
- 敏感历史内容必须有标签和审查状态；
- 不得在 Systems 中加入网络或 LLM。

验收：礼仪、知识、库存、边界、误解、信任、帮助和拒绝会产生不同结果，长期声望可以保存和解释。

结果必须保存到：
- 主成果：部落/地方社群交易代码、测试，以及 docs/game/design/common/D12_COMMUNITY_TRADE.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A05：扑克牌牌局

<details>
<summary>展开/复制：D12-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A05。

任务编号：D12-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 事件/存档/奖励成果、D10 社交成果和当前牌局代码。

执行内容：实现牌组、规则、入场费、下注上限、胜负、作弊风险、退出、债务、情报和关系影响。牌局使用固定种子和结算记录，禁止读档刷钱。

任务约束：
- 牌局结算由牌局系统唯一写入；
- 金钱、债务和奖励必须经过所属经济/奖励系统；
- 固定种子、牌局记录和存档版本必须防止重复结算；
- UI 不能修改牌面、胜负或奖金；
- Agent 只能生成对话和提议；
- 不得在 Systems 中加入网络或 LLM。

验收：牌局可进入、下注、退出和结算，作弊风险和关系/情报后果可见，读档不能刷钱或重复领奖。

结果必须保存到：
- 主成果：扑克牌规则、牌局结算、测试，以及 docs/game/design/common/D12_POKER_SYSTEM.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A06：塔罗/牌面占卜

<details>
<summary>展开/复制：D12-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A06。

任务编号：D12-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 事件成果、D10 社群/关系成果和当前牌面/占卜代码。

执行内容：实现牌面、占卜者、文化/宗教信任度、解释偏差、软线索、求证和错误后果。塔罗不能产生真实超自然结果；LLM 只能生成解释文本。

任务约束：
- 牌面和占卜结果由系统确定，不能由 LLM 裁决；
- 解释只能是软线索，不得直接改天气、奖励、关系或任务结果；
- 文化/宗教信任度必须有来源和事件；
- UI 只展示牌面和解释；
- 敏感宗教内容必须有警告和跳过；
- 不得在 Systems 中加入网络或 LLM。

验收：牌面、解释偏差、求证和错误后果可记录，塔罗不会产生不可验证的真实超自然结果。

结果必须保存到：
- 主成果：塔罗/牌面系统、测试，以及 docs/game/design/common/D12_TAROT_DIVINATION.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A07：收藏、图鉴与成就

<details>
<summary>展开/复制：D12-A07 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A07。

任务编号：D12-A07
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 内容 ID/事件/存档成果，以及 D10/D11 已完成的图鉴相关模块。

执行内容：实现动物、植物、鱼类、船、服装、地图、历史文件、宝藏和特殊武器收藏；每条记录必须有证据、地点、时间和获得方式。

任务约束：
- 收藏/图鉴状态由收藏系统唯一写入；
- 记录必须引用统一 content_id 和事实事件；
- UI 不能直接解锁成就；
- Agent 不能直接生成收藏证据；
- 历史文件和特殊武器必须有来源与授权；
- 不得在 Systems 中加入网络或 LLM。

验收：收藏记录有证据、地点、时间和获得方式，重复获得不会重复解锁，保存读档后状态一致。

结果必须保存到：
- 主成果：收藏/图鉴/成就代码、测试，以及 docs/game/design/common/D12_COLLECTION_ATLAS.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-A08：动态任务与航海日志

<details>
<summary>展开/复制：D12-A08 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D12-A08。

任务编号：D12-A08
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 WorldEvent/存档/奖励成果、D12 任务和探索成果。

执行内容：实现任务目标、事件因果、日志事实、Agent 摘要、失败恢复、替代路线和任务奖励请求。主线/支线不能复制自己的经济、关系和库存状态。

任务约束：
- 任务只拥有任务状态，不拥有金钱、库存、关系或奖励的最终写权；
- 日志事实必须来自统一事件；
- Agent 摘要不能创造事实；
- 奖励必须请求 RewardSystem 或所属系统裁决；
- 任务必须支持失败恢复和替代路线；
- 不得在 Systems 中加入网络或 LLM。

验收：任务目标、因果、日志、摘要、失败恢复和替代路线可保存，主线/支线没有私有经济或库存。

结果必须保存到：
- 主成果：动态任务和航海日志代码、测试，以及 docs/game/design/common/D12_DYNAMIC_QUEST_LOG.md；
- 交接报告：docs/game/design/common/handoffs/D12/D12-A08_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D12-S01：探索与社群集成

<details>
<summary>展开/复制：D12-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D12-S01。

任务编号：D12-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D12-A01、D12-A02、D12-A03、D12-A04、D12-A05、D12-A06、D12-A07、D12-A08 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D12 各主成果文件和 docs/game/design/common/handoffs/D12/ 下全部交接报告。

集成验收流程：玩家从传闻获得线索，准备装备，登陆沉船/洞穴，进行部落交易，可能输牌获得债务或情报，取得图鉴/特殊奖励，保存后恢复完整因果链。

任务约束：
- 不得复制探索、任务、经济、关系、牌局或收藏逻辑；
- UI 只能读取状态和事件；
- Agent 只能提议线索、解释和对话，不能裁决奖励、债务、声望或任务结果；
- 不得跳过法律、宗教和历史审查；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D12_ACCEPTANCE_REPORT.md；
- 集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D12/D12-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D12 是否通过、遗留问题和是否允许 D13 使用探索/社群数据。
```
</details>
