# 第 8 章开发任务提示词：D15 批量内容、资产与目录生产

> 本章前置：D14-S01 通过。执行顺序：`D15-S01 → D15-A01*—D15-A08* → D15-S02`。批量生产前必须先有代表样例和统一目录。

## D15-S01：内容生产冻结

<details>
<summary>展开/复制：D15-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步内容生产 Agent。请直接完成 D15-S01。

任务编号：D15-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D14-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 内容 ID/目录成果、D14 验收报告和所有代表样例。

执行内容：冻结内容表、ID 前缀、数量目标、目录字段、资产质量等级、来源/授权字段、LOD 预算、测试抽样比例和历史审查状态。

任务约束：
- 批量生产不是复制空壳；
- 每类内容必须先有代表样例；
- 所有内容使用统一 ID、来源、授权、阶段、行为和测试字段；
- 资产 Agent 不得改变强逻辑公式或数据所有权；
- 不得批量生成未经过历史/敏感内容审查的内容；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D15_CONTENT_PRODUCTION_FREEZE.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：D15-A01*—D15-A08* 都能找到目标数量、质量等级、目录字段、授权字段、抽样比例和审查要求。
```
</details>

## D15-A01*：50 座岛屿

<details>
<summary>展开/复制：D15-A01* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A01*。

任务编号：D15-A01*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01 必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D8 内容 ID 注册表、D11 生态成果、D13 岛屿/建筑成果。

执行内容：实现 8 类主地貌模板，生产 50 座岛的自然档案、海岸、山地、洞穴、淡水、暗礁、生境、资源、建筑和任务入口。每座岛都要能流式加载、保存和恢复。

任务约束：
- 每座岛必须有唯一 island_ ID、来源、授权、历史标签、资源、生境和测试入口；
- 岛屿自然结构必须与生态、建筑、任务和航海边界一致；
- 资产 Agent 不得写天气、生态、经济、任务或奖励裁决；
- 缺少模型时使用可替换占位资源，不伪造已完成表现；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：岛屿内容/资产和目录，以及 docs/game/design/common/D15_ISLAND_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：50 座岛 ID 唯一、目录完整、自然结构和入口可扫描，至少抽样运行流式加载、保存和恢复。
```
</details>

## D15-A02*：1,000—2,000 植被条目

<details>
<summary>展开/复制：D15-A02* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A02*。

任务编号：D15-A02*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表植被样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D8 内容 ID 注册表、D11 植被成果和当前资产目录工具。

执行内容：按乔木、灌木、草本、蕨类、苔藓、附生、海滨、湿地、农作物、果树、药材、香料和变体生产 1,000—2,000 条目。每条记录模型/材质/LOD/风场/生境/用途/阶段/来源/测试。

任务约束：
- 每条内容使用 plant_ ID；
- 必须有生境、用途、LOD、来源、授权和测试字段；
- 不得创建没有模型/材质状态的空壳正式条目；
- 植被 Agent 不得改变生态公式或采集裁决；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：植被目录/资产和 docs/game/design/common/D15_VEGETATION_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：数量在 1,000—2,000 范围，ID/来源/授权/生境/LOD/测试完整，抽样可被目录扫描和加载。
```
</details>

## D15-A03*：300 动物条目

<details>
<summary>展开/复制：D15-A03* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A03*。

任务编号：D15-A03*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表动物样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D8 内容 ID 注册表、D11 生态/动物成果和性能规范。

执行内容：按陆生、鸟类、爬行、昆虫、淡水/河口、近岸鱼、鲨鳐、海龟、鲸豚、甲壳类/软体和海岸鸟生产 300 条目。每条记录行为等级、生境、刷新、食物链、危险、法律和性能预算。

任务约束：
- 每条内容使用 animal_ 或 fish_ ID；
- 行为逻辑由动物/生态系统提供，资产 Agent 不写裁决；
- 动物禁止使用 LLM；
- 每条内容必须有生境、行为等级、法律、来源、授权和性能预算；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：动物目录/资产和 docs/game/design/common/D15_ANIMAL_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：300 条目 ID 唯一，生境/行为/食物链/危险/法律/性能字段完整，抽样可加载并由逻辑系统接管行为。
```
</details>

## D15-A04*：建筑和港口

<details>
<summary>展开/复制：D15-A04* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A04*。

任务编号：D15-A04*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表建筑和港口样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D13 建筑成果、D8 内容 ID 注册表和性能规范。

执行内容：生产 80 类功能建筑、180 个文化/势力/材料变体和 600—1,200 个实例。实例必须有位置、所有者、功能、库存、损伤、开放规则、交互点、音频和测试。

任务约束：
- 建筑使用 building_ ID，实例使用唯一 instance_id；
- 资产只提供模型、碰撞、锚点、目录和实例数据，不写建筑状态裁决；
- 位置、所有者、库存、损伤和开放规则必须由对应系统写；
- 资产缺失时必须标记占位状态；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：建筑/港口目录、资产和 docs/game/design/common/D15_BUILDING_PORT_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：类别、变体和实例数量满足目标，位置/所有者/功能/库存/损伤/交互/音频/测试字段完整。
```
</details>

## D15-A05*：道具、服装和装饰

<details>
<summary>展开/复制：D15-A05* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A05*。

任务编号：D15-A05*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表道具/服装样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D8 内容 ID/目录成果、D10 外观成果、D14 武器成果。

执行内容：生产 200 个功能道具、300 个装饰对象，并覆盖工具、武器、渔具、种子、食物、鱼获处理、服装、发型、饰品、家具、船舱和收藏品。

任务约束：
- 功能道具和装饰都必须有唯一 ID、分类、来源、授权和测试字段；
- 资产 Agent 不写价格、伤害、奖励、耐久或装备裁决；
- 功能数据必须接入现有系统，不能只有模型；
- 不得批量创建无材质/无碰撞/无用途的正式资源；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：道具/服装/装饰目录、资产和 docs/game/design/common/D15_ITEM_OUTFIT_DECOR_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：200 个功能道具和 300 个装饰对象可扫描，分类/来源/授权/功能入口/测试完整，抽样可加载和交互。
```
</details>

## D15-A06*：船体、模块和功能点

<details>
<summary>展开/复制：D15-A06* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A06*。

任务编号：D15-A06*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表船体和模块样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D9 船舶成果、D14 武器/模块成果和当前船体资产目录。

执行内容：生产 20 种船体、80 个改装模块、60 个功能点的模型/碰撞/锚点/动画/音频/UI 数据。不得只有模型没有可交互数据。

任务约束：
- 船体、模块和功能点使用统一 ship_/asset_ ID；
- 资产 Agent 不写重量、吃水、伤害、速度、价格或模块效果公式；
- 功能点必须有交互入口、锚点、碰撞、音频和测试字段；
- 船舶系统负责装配和强逻辑裁决；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：船体/模块/功能点目录、资产和 docs/game/design/common/D15_SHIP_HULL_MODULE_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：20/80/60 数量目标满足，模型、碰撞、锚点、动画、音频、UI 和交互数据完整，抽样可以装配和测试。
```
</details>

## D15-A07*：角色与宠物

<details>
<summary>展开/复制：D15-A07* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A07*。

任务编号：D15-A07*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表角色和宠物样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D10 角色/外观成果、D11 动物/宠物成果和性能规范。

执行内容：生产至少 60 个船员预设、500—1,000 个普通 NPC 实例组合、服装/发型/职业/文化组合、猫狗和家禽/牲畜变体。

任务约束：
- 行为逻辑由角色/动物系统提供，资产 Agent 不写裁决；
- 每个角色/宠物必须有 ID、文化/职业标签、来源、授权、外观组合和测试字段；
- 不得为资产直接添加关系、工资、健康、奖励或战斗结果；
- 不能用复制粘贴制造无差异空壳；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：角色/宠物目录和资产，以及 docs/game/design/common/D15_CHARACTER_PET_BATCH_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：船员预设、NPC 实例组合和宠物变体达到目标，字段完整，行为入口由逻辑系统接管，抽样可以加载。
```
</details>

## D15-A08*：任务、音乐和本地化内容

<details>
<summary>展开/复制：D15-A08* 完整提示词</summary>

```text
你是《鲸骨航线》的资产/内容 Agent。请直接完成 D15-A08*。

任务编号：D15-A08*
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S01、代表任务/音乐/本地化样例必须完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D8 事件/内容 ID 成果、D12 任务成果、D14 历史审查成果和现有音频/本地化目录。

执行内容：生产 5 章主线、120 个支线模板、30 个音乐主题、港口/航行/战斗/风暴/夜晚/势力/牌局/关系的音频层，以及中英双语 key。

任务约束：
- 任务内容必须使用统一 quest_ ID、事件和奖励请求；
- 音乐和音频必须有来源、授权、触发条件和混音标签；
- 本地化必须使用 key，不得把文本硬编码进逻辑；
- 历史和敏感内容必须有标签、警告和审查状态；
- 资产 Agent 不得写任务、奖励、关系和经济裁决；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：任务/音乐/本地化目录和资产，以及 docs/game/design/common/D15_QUEST_AUDIO_LOCALIZATION_CATALOG.md；
- 交接报告：docs/game/design/common/handoffs/D15/D15-A08_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

验收：主线/支线/音乐/音频/中英 key 达到目标，来源/授权/触发/审查字段完整，缺 key 和孤儿引用可被检查器发现。
```
</details>

## D15-S02：批量内容 QA

<details>
<summary>展开/复制：D15-S02 完整提示词</summary>

```text
你是《鲸骨航线》的同步内容 QA Agent。请直接完成 D15-S02。

任务编号：D15-S02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-A01*、D15-A02*、D15-A03*、D15-A04*、D15-A05*、D15-A06*、D15-A07*、D15-A08* 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15_CONTENT_PRODUCTION_FREEZE.md、D15 各批量目录文件和 docs/game/design/common/handoffs/D15/ 下全部交接报告。

检查内容：
- 数量；
- 唯一 ID；
- 重复模型；
- 缺材质；
- 缺碰撞；
- 缺 LOD；
- 缺本地化；
- 缺授权；
- 错误生境；
- 错误历史年代；
- 缺测试；
- 孤儿引用。

执行抽样：每种内容至少抽样运行一次，检查加载、保存、交互入口、行为接管和目录扫描，并写生产验收报告。

任务约束：
- 不得为了通过 QA 删除问题内容；
- 不得把未验证资源标记为完成；
- 资产问题和逻辑问题必须分开登记责任任务；
- 不得在 Systems 中加入网络或 LLM；
- 不得擅自修改强逻辑公式或数量目标。

结果必须保存到：
- 主成果：docs/game/design/common/D15_BATCH_CONTENT_QA_REPORT.md；
- QA 脚本/抽样结果：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D15/D15-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D15 是否通过、剩余问题、责任任务和是否允许进入 D16。
```
</details>
