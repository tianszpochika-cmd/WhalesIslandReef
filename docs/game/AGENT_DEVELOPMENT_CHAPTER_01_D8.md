# 第 1 章开发任务提示词：D8 数据、契约、存档与内容 ID

> 本文件只包含可直接交给 Agent 的 D8 任务提示词。每个任务都自带项目约束、读取顺序、实现目标、验收标准和结果保存位置。
>
> 任务总顺序：`D8-S01 → D8-A01/A02/A03/A04/A06 → D8-A05 → D8-S02`。其中 D8-A05 必须等待 D8-A01。

## D8-S01：冻结 D8 数据边界

<details>
<summary>展开/复制：D8-S01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-S01，不要只给方案或分析。

任务编号：D8-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D7 基线

开始前必须读取：
1. AGENTS.md；
2. docs/CONTEXT.md；
3. docs/game/REQUIREMENTS.md；
4. docs/game/DESIGN_PILLARS.md；
5. docs/game/design/DESIGN_BOUNDARIES.md；
6. docs/game/design/DATA_MODELS.md；
7. docs/game/AGENT_DEVELOPMENT_TASKS.md；
8. 当前 D7 代码、契约和测试。

执行内容：
- 对照 DATA_MODELS.md、DESIGN_BOUNDARIES.md 和当前代码，列出 D7 已有字段与 v3 新字段；
- 建立 Player、Ship、Crew、Relation、Island、LandParcel、Building、Item、Animal、FishSchool、Pet、FinanceAccount、Quest、WorldEvent、Reward、AppearanceLoadout 的实体清单；
- 为每个实体确认唯一写者、读取者、存档字段、版本号和测试入口；
- 明确哪些实体只在 game 内部，哪些需要进入 contracts，哪些暂不进入 server/web；
- 将最终冻结结果写入 docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；已有同名文件时更新原文件，不创建重复版本。

任务约束：
- 只允许修改 docs/ 和必要的 contracts 草案；不得修改领域业务逻辑；
- 不得删除 D7 存档字段；
- 不得改变 AgentCore 现有公共类型；
- 不得把数据所有权交给 UI 或 Agent；
- 不得批量创建玩法内容；
- 不得在 game/Assets/Scripts/Systems/ 中加入网络或 LLM 代码；
- 交叉模块发现冲突时，先记录冲突，不要擅自替其他系统做决定。

验收标准：
- D8-A01 到 D8-A06 都能从冻结表找到自己的实体和写者；
- 每个实体都有清晰的数据所有权、存档字段和版本号；
- D7 与 v3 的字段差异有记录；
- 文档没有无法解释的跨端边界冲突。

结果必须保存到：
- 主成果：docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md；
- 如果 docs/game/design/common/handoffs/D8/ 不存在，创建该目录。

D8-S01_HANDOFF.md 必须包含：任务编号、修改文件、数据边界、D7/v3 差异、冲突、验收结论、D8-A01—D8-A06 的注意事项和是否允许开始异步任务。

完成后在聊天中列出以上文件路径，但聊天回复不能替代文件交付。
```
</details>

## D8-A01：核心实体与 DTO

<details>
<summary>展开/复制：D8-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A01，不要只给方案。

任务编号：D8-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/DESIGN_PILLARS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- 当前 AgentCore、Systems、契约和测试。

执行内容：
- 创建或补齐 v3 核心实体的纯数据模型和稳定 ID；
- 保留 D7 Player、Ship、CrewAgent 的兼容字段；
- 为金额、重量、体积、时间、状态、关系范围等使用明确类型；
- 不把 MonoBehaviour、Renderer、GameObject 或其他 Unity 运行时对象放进 AgentCore 核心实体；
- 为实体提供默认值、空值策略、版本号和序列化测试；
- 让空实体、完整实体、未知枚举和旧字段都能测试；
- 保证 game/AgentCore/ 可以独立执行 dotnet build；
- 将模型说明写入 docs/game/design/common/D8_ENTITY_DTO_CATALOG.md。

任务约束：
- game/AgentCore/ 必须保持纯 C#，不得引入 Unity 或第三方依赖；
- 不得删除 D7 字段；
- 不得改变其他系统的数据所有权；
- 不得实现无关的船战、经济或关系玩法；
- 不得使用网络、LLM 或静态全局变量绕过模型边界；
- 只修改 game/AgentCore/、D8-S01 指定的 Systems 数据层、对应测试和必要文档。

验收标准：
- AgentCore build 通过；
- 序列化和反序列化测试通过；
- 旧字段兼容；
- 未知枚举有明确策略；
- 每个实体都能回答“谁写、谁读、怎样存档”。

结果必须保存到：
- 主成果：game/AgentCore/ 或任务允许的 Systems 数据层；
- 模型说明：docs/game/design/common/D8_ENTITY_DTO_CATALOG.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出修改文件、公共类型、数据所有权、测试命令和结果、未完成事项以及 D8-A05 的迁移注意事项。
```
</details>

## D8-A02：内容 ID 与目录注册表

<details>
<summary>展开/复制：D8-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A02。

任务编号：D8-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- 当前 docs/、game/Assets/Settings/ 和内容数据目录。

执行内容：
- 建立 ship_、crew_、island_、building_、item_、animal_、plant_、fish_、quest_、reward_、outfit_、pet_、asset_ ID 前缀；
- 为每条内容登记名称、本地化 key、来源、授权、历史标签、阶段、所属系统和 test_id；
- 编写重复 ID、缺失引用、非法前缀、缺少来源、缺少授权和孤儿引用检查器；
- 建立少量代表样例验证目录结构；
- 先建立代表样例，不批量伪造 50 个岛、300 个动物、1000 个植被或 200 个道具；
- 将目录规范写入 docs/game/design/common/D8_CONTENT_ID_REGISTRY.md。

任务约束：
- 不得改变 D8-S01 的 ID 规则；
- 不得把无来源或无授权的外部资源放入正式目录；
- 不得让 UI 私自维护内容目录；
- 不得修改其他系统的强逻辑公式；
- 只修改 docs/、game/Assets/Settings/、内容数据目录、目录检查器、对应测试和必要文档。

验收标准：
- 目录可以被工具扫描；
- 重复 ID、非法前缀、孤儿引用、缺少来源和缺少授权都会使检查失败；
- 代表样例可以被 game 读取；
- 检查错误信息能定位具体内容 ID。

结果必须保存到：
- 主成果：docs/game/design/common/D8_CONTENT_ID_REGISTRY.md；
- 内容目录和检查器：任务允许的内容目录；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出 ID 前缀、目录字段、检查命令、代表样例、测试结果和后续 D9 可使用的入口。
```
</details>

## D8-A03：统一玩法事件包

<details>
<summary>展开/复制：D8-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A03。

任务编号：D8-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- 当前 Systems/AgentCore 事件层和测试。

执行内容：
- 实现统一 WorldEvent/FactEvent；
- 事件必须包含 event_id、schema_version、world_day、location_ref、actor_ids、input_facts、owner_system、result、cause_ids、save_revision；
- 支持正常、失败、取消、中断、恢复和重复结算保护；
- 事件事实先写入日志，再提供给 Quest、UI 和 Agent 摘要读取；
- 事件系统不能直接写入其他系统的数值；
- 为事件因果链、版本和读档后的重复保护编写测试；
- 将事件包说明写入 docs/game/design/common/D8_EVENT_PACKAGE.md。

任务约束：
- 事件系统不得直接裁决金钱、价格、伤害、奖励、天气或关系数值；
- 不得让 Agent 直接写入最终结果；
- 不得在 game/Assets/Scripts/Systems/ 中加入网络或 LLM 代码；
- 不得绕过数据所有权；
- 只修改 Systems/AgentCore 事件层、事件 DTO、对应测试和必要文档。

验收标准：
- 同一事件读档后不会重复发放奖励；
- 失败事件有 recovery_route；
- 中断事件可以恢复；
- 事件因果可以追踪；
- 事件版本不匹配时有明确错误。

结果必须保存到：
- 主成果：事件层代码、测试和 docs/game/design/common/D8_EVENT_PACKAGE.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出事件字段、所有权、重复结算保护、测试命令和结果、已知边界。
```
</details>

## D8-A04：AgentReply 与 P2A 扩展

<details>
<summary>展开/复制：D8-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A04，并严格执行契约先行。

任务编号：D8-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- contracts/agent-reply.schema.json；
- 当前 AgentCore AgentReply 模型；
- 当前 server gateway 校验代码和测试。

执行内容：
- 扩展 AgentReply 的对话、情绪、动作意图、关系提议、谈判语气和拒绝原因；
- 将 P2A 目标扩展到 60 个；
- 每个 Action 必须有注册名、参数 Schema、权限、前置、失败码和执行者；
- 实现非法字段、超范围数值、未知动作、未授权对象和越权目标的拒绝；
- 先修改 contracts，再同步 game、AgentCore、server，必要时同步 web；
- 增加 Schema、AgentCore、server 和 Unity Mock 测试；
- 将 Action 注册表写入 docs/game/design/common/D8_P2A_ACTION_REGISTRY.md。

严格禁止 AgentReply 直接包含最终 money、price、damage、reward、weather 或 relation 数值结果。AgentReply 只能提出意图，最终结果必须由所属强逻辑系统裁决。

任务约束：
- 不得使用单端私有协议；
- 不得绕过 Schema；
- 不得让 LLM 直接修改游戏数值；
- 不得在 Systems 中加入网络或 LLM 代码；
- 跨端数据必须经过 contracts；
- 只修改 contracts、AgentCore、server gateway、必要的 web 同步、测试和必要文档。

验收标准：
- Schema 测试、AgentCore 测试、server 校验测试和至少一个 Unity Mock 执行测试全部通过；
- game/server/web 不得有 Schema 漂移；
- 60 个 P2A Action 都有完整注册信息；
- 非法动作、越权对象和非法参数都会被拒绝。

结果必须保存到：
- 主成果：contracts、AgentCore、server/web 同步代码和 docs/game/design/common/D8_P2A_ACTION_REGISTRY.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出契约版本、Action 清单、拒绝逻辑、跨端同步文件、测试命令和结果。
```
</details>

## D8-A05：D7 存档迁移

<details>
<summary>展开/复制：D8-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A05。

任务编号：D8-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01、D8-A01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- docs/game/design/common/D8_ENTITY_DTO_CATALOG.md；
- D7 存档结构；
- 当前 Save/Storage 代码和测试。

执行内容：
- 为 D7 存档增加 v3 迁移器；
- 保留旧 Ship、Crew、Player 字段语义；
- 为新字段提供默认值；
- 明确处理迁移中断、重复执行、缺字段、未知版本和部分损坏；
- 保存后写入 save_version、world_revision、last_event_id 和 migration_flags；
- 测试空档、D7 档、航行中档、损伤船档、负债档和半途任务档；
- 迁移失败时不得覆盖原始存档，并给出可理解的恢复信息；
- 将迁移说明写入 docs/game/design/common/D8_SAVE_MIGRATION.md。

任务约束：
- 不得删除 D7 字段；
- 不得因为新字段缺失而丢弃旧存档；
- 不得让重复迁移重复发放奖励或重复改变世界状态；
- 不得覆盖迁移失败前的原档；
- 不得修改无关玩法系统；
- 只修改 Save/Storage、存档模型、迁移器、存档测试和必要文档。

验收标准：
- 旧存档能加载到统一模型；
- 新字段有安全默认值；
- 重复迁移受到保护；
- 迁移失败不覆盖原档；
- 错误信息明确；
- 规定的存档测试全部通过。

结果必须保存到：
- 主成果：Save/Storage 代码、测试和 docs/game/design/common/D8_SAVE_MIGRATION.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出支持的旧版本、字段迁移表、失败保护、测试存档类型、测试命令和结果。
```
</details>

## D8-A06：统一数值和枚举配置

<details>
<summary>展开/复制：D8-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请在当前项目仓库中直接完成 D8-A06。

任务编号：D8-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-S01 必须完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- 现有 Settings、纯 C# 配置和参数定义。

执行内容：
- 统一金额、重量、体积、距离、时间、关系范围、健康范围、天气状态、稀有度、法律状态和品质枚举；
- 将可调参数放入 game/Assets/Settings/ 或纯 C# 统一配置入口；
- 清理同一参数散落在多个系统中的重复硬编码；
- 为每个参数登记单位、默认值、最小值、最大值、拥有者、来源和测试入口；
- 增加配置合法性检查；
- 将配置说明写入 docs/game/design/common/D8_NUMERIC_ENUM_CONFIG.md。

重要限制：本任务不调整最终经济平衡、价格、奖励或战斗数值，只完成统一参数入口，不改变玩法公式。

任务约束：
- 不得把最终平衡调整混入本任务；
- 不得保留关键参数的无主硬编码；
- 不得让 UI 或 Agent 成为参数裁决者；
- 不得在 Systems 中加入网络或 LLM 代码；
- 只修改 game/Assets/Settings/、纯 C# 配置、配置读取代码、配置测试和必要文档。

验收标准：
- 所有统一参数有明确类型、单位、默认值、边界、所有者、来源和测试入口；
- 非法配置能被检查器发现；
- 业务系统可以从统一入口读取参数；
- 配置测试通过；
- 没有改变最终经济或战斗平衡。

结果必须保存到：
- 主成果：game/Assets/Settings/ 或纯 C# 配置、测试和 docs/game/design/common/D8_NUMERIC_ENUM_CONFIG.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出新增配置、替换的硬编码、配置检查命令和测试结果。
```
</details>

## D8-S02：D8 合并门

<details>
<summary>展开/复制：D8-S02 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请在当前项目仓库中直接完成 D8-S02，不要只写验收意见。

任务编号：D8-S02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D8-A01、D8-A02、D8-A03、D8-A04、D8-A05、D8-A06 全部完成

开始前必须读取：
- AGENTS.md；
- docs/CONTEXT.md；
- docs/game/REQUIREMENTS.md；
- docs/game/design/DESIGN_BOUNDARIES.md；
- docs/game/design/DATA_MODELS.md；
- docs/game/design/common/D8_DATA_BOUNDARY_FREEZE.md；
- docs/game/design/common/D8_ENTITY_DTO_CATALOG.md；
- docs/game/design/common/D8_CONTENT_ID_REGISTRY.md；
- docs/game/design/common/D8_EVENT_PACKAGE.md；
- docs/game/design/common/D8_P2A_ACTION_REGISTRY.md；
- docs/game/design/common/D8_SAVE_MIGRATION.md；
- docs/game/design/common/D8_NUMERIC_ENUM_CONFIG.md；
- docs/game/design/common/handoffs/D8/ 下的全部 D8 交接报告。

严格按以下顺序执行：
1. 合并所有实体和契约变更；
2. 检查 game/server/web 是否存在 Schema 漂移；
3. 运行 AgentCore build；
4. 运行 server build；
5. 运行契约测试；
6. 运行存档迁移测试；
7. 运行配置检查和必要的 Unity Mock 测试；
8. 运行 git diff --check；
9. 检查 game/Assets/Scripts/Systems/ 中没有网络或 LLM 代码；
10. 检查 game/AgentCore/ 没有 Unity 或第三方依赖；
11. 检查每份数据只有一个系统写入；
12. 将验收报告写入 docs/game/design/common/D8_ACCEPTANCE_REPORT.md。

任务约束：
- 不得为了通过测试删除 D7 功能；
- 不得跳过失败测试；
- 不得私自修改 D9-D16 任务；
- 不得把未完成任务标记为完成；
- 不得在同步任务中复制一套领域逻辑；
- 不得用测试开关隐藏问题；
- 发现阻塞时必须记录责任任务和修复顺序。

验收标准：
- D7 存档仍然可以读取；
- v3 实体模型可以正常序列化；
- AgentReply Schema 在各端一致；
- P2A 非法动作会被拒绝；
- WorldEvent 不会重复结算；
- 内容 ID 检查器可以发现错误；
- 统一配置可以被读取；
- 所有规定测试通过；
- 没有违反项目铁律。

结果必须保存到：
- 主成果：docs/game/design/common/D8_ACCEPTANCE_REPORT.md；
- 任务交接报告：docs/game/design/common/handoffs/D8/D8-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确：D8 是否通过、是否允许进入 D9；如果不通过，列出阻塞项、责任任务和修复顺序。
```
</details>
