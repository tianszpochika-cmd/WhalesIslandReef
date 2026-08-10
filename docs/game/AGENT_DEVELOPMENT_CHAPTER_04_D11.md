# 第 4 章开发任务提示词：D11 海洋采集、生态、珍稀动物与宠物

> 本章前置：D9-S02 通过。执行顺序：`D11-A01—D11-A07` 可按模块并行，全部完成后执行 `D11-S01`。

## D11-A01：生态状态机

<details>
<summary>展开/复制：D11-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A01。

任务编号：D11-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 事件/配置成果、D9 天气切片成果和当前 EcologySystem。

执行内容：实现生境、植物、动物、鱼群、捕捞压力、恢复、天气、潮汐、海流、温度、浑浊度和污染的确定性关系。

任务约束：
- 动物禁止使用 LLM；
- EcologySystem 是资源量、鱼群和生境状态的唯一写者；
- 天气/海况只通过统一状态读取；
- UI 只能展示生态状态；
- 不得让 Agent 直接改变鱼群、动物或污染；
- 必须支持保存、读档、事件追踪和测试；
- 不得在 Systems 中加入网络或 LLM。

验收：捕捞压力会影响资源，天气/潮汐/海流等会影响生态，恢复和污染有确定性结果，读档不重复结算。

结果必须保存到：
- 主成果：生态状态机、测试，以及 docs/game/design/common/D11_ECOLOGY_STATE_MACHINE.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A02：完整钓鱼与海上采集

<details>
<summary>展开/复制：D11-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A02。

任务编号：D11-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11_ECOLOGY_STATE_MACHINE.md、D8 内容 ID/配置成果、D9 钓鱼切片代码。

执行内容：扩展手线、岸钓、网、笼、贝类、海草和浅水采集；实现鱼种、深度、时间、潮汐、渔具、品质、处理、腌制、鱼干、鱼油和市场反馈。

任务约束：
- 鱼群和资源量由 EcologySystem 写；
- Inventory/Cargo 写物品库存；
- EconomySystem 写价格和市场反馈；
- UI 不得写鱼获、库存、资源量或价格；
- 不得用随机文本代替物品数据；
- 采集压力和保护区规则必须可追踪；
- 不得在 Systems 中加入网络或 LLM。

验收：不同鱼种/深度/时间/潮汐/工具产生可解释差异，处理品进入库存并能出售，读档后不重复添加鱼获。

结果必须保存到：
- 主成果：钓鱼和海上采集代码、数据、测试，以及 docs/game/design/common/D11_FISHING_HARVESTING.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A03：活鱼舱与岛上水产

<details>
<summary>展开/复制：D11-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A03。

任务编号：D11-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11_ECOLOGY_STATE_MACHINE.md、D8 存档/实体成果、当前船舱和建筑代码。

执行内容：
- 实现船上活鱼舱容积、换水、氧气/水质、拥挤、死亡率、重量和维护；
- 实现岛上鱼塘/近岸网箱建筑；
- 输入鱼苗、饲料和劳力；
- 输出鱼货和生态压力；
- 支持保存、读档、死亡和维护事件。

任务约束：
- 活鱼舱状态由船舶/水产系统唯一写入；
- 岛上鱼塘由建筑/养殖系统写入；
- 生态压力必须由 EcologySystem 接收和裁决；
- UI 不能直接改变水质、死亡率或鱼货；
- 不得让 AgentReply 直接生成鱼货；
- 不得在 Systems 中加入网络或 LLM。

验收：活鱼舱和鱼塘有容量、水质、氧气、拥挤、维护和死亡结果，保存读档不重复结算，生态压力可追踪。

结果必须保存到：
- 主成果：活鱼舱/岛上水产代码、测试，以及 docs/game/design/common/D11_LIVE_FISH_AQUACULTURE.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A04：鲸豚观察与捕鲸路线

<details>
<summary>展开/复制：D11-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A04。

任务编号：D11-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11_ECOLOGY_STATE_MACHINE.md、历史/敏感内容设计、当前动物和任务代码。

执行内容：实现鲸群迁徙、海豚伴航、观测记录、搁浅救助和捕鲸分支。捕鲸必须有许可证、船员/船型要求、处理设施、法律政策、社会声望、生态后果和放弃追踪的替代奖励。

强制审查：批量内容前必须写历史标签和敏感内容说明，不把捕鲸做成无代价高价按钮。

任务约束：
- 动物行为必须是确定性逻辑，禁止 LLM；
- 捕鲸条件和后果由生态、法律、势力和任务系统裁决；
- 不得让 UI 或 Agent 直接发放捕鲸收益；
- 必须有观测、救助、放弃追踪等非捕鲸路线；
- 敏感内容必须有历史标签和审查状态；
- 不得在 Systems 中加入网络或 LLM。

验收：迁徙/观测/救助/捕鲸分支各有记录，许可证和设施影响可验证，生态与社会后果可追踪，替代奖励可用。

结果必须保存到：
- 主成果：鲸豚逻辑、任务、审查说明和测试，以及 docs/game/design/common/D11_WHALE_DOLPHIN_ROUTES.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A05：珍稀动物观察、救助、收藏

<details>
<summary>展开/复制：D11-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A05。

任务编号：D11-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 生态成果、D8 内容 ID/事件/存档成果、当前 Animal/Collection/Quest 代码。

执行内容：实现发现、识别、观察、记录、救助、合法转移、释放、收藏册、收藏家、保护区、非法交易和运输死亡率。图鉴记录首次地点、季节、行为、处理方式和证据。

任务约束：
- 动物行为和死亡率由动物/生态系统裁决；
- 收藏和合法转移必须有来源、地点、时间和证据；
- 非法交易必须进入法律和声望后果；
- UI 不能直接添加图鉴或收藏；
- Agent 不能直接生成稀有动物或奖励；
- 不得在 Systems 中加入网络或 LLM。

验收：发现、观察、救助、释放、收藏和非法交易产生不同可追溯结果，图鉴证据完整，运输死亡率可保存和恢复。

结果必须保存到：
- 主成果：珍稀动物、收藏和图鉴代码、测试，以及 docs/game/design/common/D11_RARE_ANIMAL_COLLECTION.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A06：猫狗宠物

<details>
<summary>展开/复制：D11-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A06。

任务编号：D11-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 生态成果、D8 宠物/实体/存档成果和当前 Pet/Animal 代码。

执行内容：实现领养、来源、健康、饥饿、清洁、恐惧、依恋、位置、喂养、走失、检疫、捕鼠、预警和船员反应。

任务约束：
- 宠物不是战斗单位；
- 宠物不能无条件重生；
- 宠物健康和位置由宠物系统唯一写入；
- UI 只能发起领养、喂养、寻找等请求；
- Agent 只能生成表达，不能直接改变宠物状态；
- 宠物行为不得使用 LLM；
- 不得在 Systems 中加入网络或 LLM。

验收：宠物可领养、喂养、走失、检疫和恢复，状态进入存档，船员反应可追踪，宠物死亡或离失有明确后果。

结果必须保存到：
- 主成果：宠物代码、样例、测试，以及 docs/game/design/common/D11_PETS_CATS_DOGS.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-A07：植被采集与水果资源

<details>
<summary>展开/复制：D11-A07 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D11-A07。

任务编号：D11-A07
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D9-S02、D11-A01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 生态成果、D8 内容 ID/目录成果、当前 Inventory/Cargo 和资源采集代码。

执行内容：实现果实、蔬果、药材、纤维、木材、树脂、染料和毒物的识别、成熟度、工具、采集、误采、保护区和恢复；Inventory/Cargo 写库存，Ecology 写资源量。

任务约束：
- Inventory/Cargo 是库存唯一写者；
- EcologySystem 是资源量和恢复唯一写者；
- 采集工具、成熟度、保护区和误采必须有确定性规则；
- UI 只能显示采集结果；
- Agent 不得直接生成资源或改资源量；
- 不得在 Systems 中加入网络或 LLM。

验收：不同资源有正确识别、成熟度、工具、保护区和恢复结果，误采和毒物有可追踪后果，保存读档不重复添加。

结果必须保存到：
- 主成果：植被/水果资源代码、样例、测试，以及 docs/game/design/common/D11_VEGETATION_HARVEST.md；
- 交接报告：docs/game/design/common/handoffs/D11/D11-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D11-S01：生态与航程集成

<details>
<summary>展开/复制：D11-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D11-S01。

任务编号：D11-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D11-A01、D11-A02、D11-A03、D11-A04、D11-A05、D11-A06、D11-A07 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 各主成果文件和 docs/game/design/common/handoffs/D11/ 下全部交接报告。

集成验收：
- 天气/潮汐改变鱼群；
- 捕捞影响鱼群和价格；
- 活鱼舱读档不重复结算；
- 救助珍稀动物改变图鉴/声望/任务；
- 宠物状态进入船员关系；
- 动物逻辑无 LLM；
- 植被采集进入库存并改变资源量；
- 生态和动物状态可以保存和恢复。

任务约束：
- 不得复制生态、动物、库存或关系逻辑；
- UI 只能读取事件和快照；
- Agent 只能提议，不能裁决生态、鱼获、宠物或奖励；
- 不得跳过敏感内容审查；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D11_ACCEPTANCE_REPORT.md；
- 集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D11/D11-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D11 是否通过、遗留问题和是否允许 D13 使用生态/采集数据。
```
</details>
