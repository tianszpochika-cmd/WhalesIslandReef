# 第 9 章开发任务提示词：D16 性能、存档迁移、本地化与发布验收

> 本章前置：D15-S02 通过。执行顺序：`D16-A01—D16-A06` 可按模块并行，全部完成后执行 `D16-S01 → D16-S02`。

## D16-A01：世界流式加载与性能

<details>
<summary>展开/复制：D16-A01 完整提示词</summary>

```text
你是《鲸骨航线》的性能 Agent。请直接完成 D16-A01。

任务编号：D16-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15 批量内容 QA 报告、D9 垂直切片成果、D15 内容目录和当前加载/性能代码。

执行内容：实现岛屿/海域分区加载、LOD、对象池、Agent 模拟等级、动物活跃上限、远景建筑和音频卸载。记录 CPU、GPU、内存、加载时间、GC、同屏 Agent 和动物数量。

任务约束：
- 性能优化不得改变强逻辑结果；
- 降级只改变表现或模拟频率，不能跳过存档和事件事实；
- 流式加载必须可保存和恢复；
- 不得删除内容来掩盖性能问题；
- 不得在 Systems 中加入网络或 LLM。

验收：不同距离/模拟等级下性能指标可记录，加载/卸载不丢失实体和事件，LOD/对象池/音频卸载可验证。

结果必须保存到：
- 主成果：性能代码、基准数据和 docs/game/design/common/D16_PERFORMANCE_STREAMING_REPORT.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-A02：存档、断电和版本迁移

<details>
<summary>展开/复制：D16-A02 完整提示词</summary>

```text
你是《鲸骨航线》的存档 QA Agent。请直接完成 D16-A02。

任务编号：D16-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过；D8-A05 存档迁移必须可用

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8_SAVE_MIGRATION.md、D9/D15 验收报告、当前 Save/Storage 和迁移代码。

执行内容：覆盖 D7、D8、D9、完整资产档、岛屿经营档、负债档、战斗中档、生态变化档、关系档和损坏档。测试重复结算、断电、迁移失败、备份恢复和云存档接口预留。

任务约束：
- 迁移失败不得覆盖原档；
- 断电和写入中断必须有备份/恢复策略；
- 云存档只做接口预留，不擅自接入外部服务；
- 不得删除旧字段或跳过坏档测试；
- 不得在 Systems 中加入网络或 LLM。

验收：所有档案类型可验证，重复结算受保护，断电可恢复，迁移失败有可理解错误，备份恢复可执行。

结果必须保存到：
- 主成果：存档 QA/迁移代码、测试和 docs/game/design/common/D16_SAVE_MIGRATION_QA.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-A03：本地化、可访问性和输入

<details>
<summary>展开/复制：D16-A03 完整提示词</summary>

```text
你是《鲸骨航线》的本地化与可访问性 Agent。请直接完成 D16-A03。

任务编号：D16-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15 本地化目录、D10 外观/社交成果、D12 牌局/社群成果和当前 Localization/Input/UI 代码。

执行内容：实现中英双语、长文本、数字格式、键鼠/手柄、输入重绑定、字幕、色盲可读性、字体回退、暂停、低刺激选项、敏感内容警告和跳过。

任务约束：
- 文本必须使用本地化 key，不得硬编码到逻辑；
- 输入层只产生输入意图，不写业务数值；
- 敏感内容状态由内容审查数据驱动；
- UI 不得因本地化截断而隐藏关键警告或价格；
- 不得在 Systems 中加入网络或 LLM。

验收：中英长文本、数字格式、输入重绑定、字幕、色盲、字体回退、暂停、低刺激和敏感内容选项可测试。

结果必须保存到：
- 主成果：本地化/输入/可访问性代码、测试和 docs/game/design/common/D16_LOCALIZATION_ACCESSIBILITY.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-A04：历史、敏感内容和资产授权审查

<details>
<summary>展开/复制：D16-A04 完整提示词</summary>

```text
你是《鲸骨航线》的历史与内容审查 Agent。请直接完成 D16-A04。

任务编号：D16-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15 批量目录/授权报告、D14 宗教/法律/历史审查成果和全部敏感内容登记。

检查范围：1675 年时间线、势力、宗教、社群、奴役/强迫劳动、捕鲸、珍稀动物、赌场、土地经营和角色身份表达。

执行内容：每条敏感内容必须有标签、表现等级、警告、跳过和审查状态；每个外部资产必须有授权记录。发现事实不确定时记录待核查项，不擅自伪造历史结论。

任务约束：
- 不得删除历史和敏感内容来掩盖问题；
- 不得把审查意见直接写成游戏数值裁决；
- 所有外部资源必须有来源和授权；
- 资产 Agent 和逻辑 Agent 的责任必须分开；
- 不得在 Systems 中加入网络或 LLM。

验收：时间线、势力、宗教、社群、奴役/强迫劳动、捕鲸、赌场、土地、身份和所有外部资产都有审查状态或明确待处理项。

结果必须保存到：
- 主成果：docs/game/design/common/D16_HISTORY_SENSITIVITY_LICENSE_REVIEW.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-A05：画质、动画、音频和 UI 最终表现

<details>
<summary>展开/复制：D16-A05 完整提示词</summary>

```text
你是《鲸骨航线》的最终表现 Agent。请直接完成 D16-A05。

任务编号：D16-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过；D16-A04 的授权/审查阻塞项必须明确

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D15 批量资产 QA、D16 历史/授权审查、D9-D14 验收报告和当前表现代码。

执行内容：完成船体、港口、岛屿、植被、水体、天气、角色、动物、宠物、服装、牌局、特殊奖励和战斗的最终表现替换；检查材质、光照、阴影、风场、混音、动态音乐、碰撞、相机和 UI 可读性。

任务约束：
- 最终表现替换不能改变实体 ID、强逻辑公式和数据所有权；
- 未授权资产不得进入正式构建；
- 画质降级必须有配置，不得删除逻辑；
- 碰撞、锚点和交互不能只做视觉假象；
- 不得在 Systems 中加入网络或 LLM。

验收：重点资产的材质、光照、阴影、风场、水体、天气、动画、音频、碰撞、相机和 UI 都有抽样记录，授权和性能问题已登记。

结果必须保存到：
- 主成果：最终表现资源/配置、测试和 docs/game/design/common/D16_FINAL_PRESENTATION_QA.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-A06：发布技术准备

<details>
<summary>展开/复制：D16-A06 完整提示词</summary>

```text
你是《鲸骨航线》的发布技术 Agent。请直接完成 D16-A06。

任务编号：D16-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D15-S02 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D16 性能/存档/本地化/审查成果、当前构建配置和安装脚本。

执行内容：完成构建配置、崩溃日志、离线模式、模板 Agent 降级、配置校验、安装/卸载、更新、存档备份、隐私说明、商店素材和已知问题列表。真实 Steam 发布动作需要用户授权，不由 Agent 自动执行。

任务约束：
- 不得擅自发布、上传或发送外部消息；
- 离线模式和模板 Agent 不能改变强逻辑裁决；
- 崩溃日志不得泄露不必要的个人数据；
- 安装/卸载/更新不得破坏存档；
- 商店素材必须有授权和版本记录；
- 不得在 Systems 中加入网络或 LLM。

验收：构建、安装/卸载、更新、备份恢复、离线降级、配置校验、崩溃记录、隐私说明和商店素材检查通过。

结果必须保存到：
- 主成果：发布配置、脚本、清单和 docs/game/design/common/D16_RELEASE_TECHNICAL_PREP.md；
- 交接报告：docs/game/design/common/handoffs/D16/D16-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D16-S01：完整版本验收

<details>
<summary>展开/复制：D16-S01 完整提示词</summary>

```text
你是《鲸骨航线》的最终版本验收 Agent。请直接完成 D16-S01，不要只给意见。

任务编号：D16-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D16-A01、D16-A02、D16-A03、D16-A04、D16-A05、D16-A06 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D16 各主成果文件、D16 交接报告，以及 D8-D15 所有阶段验收报告。

必须从新游戏走到长线存档：
创建角色 → 初始船 → 航行 → 钓鱼 → 贸易 → 船员关系 → 探索寻宝 → 牌局/社群交易 → 资产/金融 → 种植/岛屿经营 → 战斗/特殊奖励 → 保存/迁移/恢复。

验收要求：
- 所有阶段核心流程可连续运行；
- 任务、经济、关系、生态、战斗、奖励和存档没有重复结算；
- 断电/迁移/备份恢复可验证；
- 本地化和可访问性关键路径可用；
- 历史/敏感内容和资产授权没有未记录阻塞；
- 性能和发布技术指标有报告；
- Systems 无网络/LLM，AgentCore 无 Unity 依赖；
- 失败项必须记录责任任务、严重度和修复顺序。

任务约束：
- 不得为了通过验收删除失败流程；
- 不得把 BLOCKED 标记为 DONE；
- 不得擅自发布游戏或上传外部服务；
- 不得在验收阶段复制领域逻辑；
- 不得跳过已知问题记录。

结果必须保存到：
- 主成果：docs/game/design/common/D16_FULL_VERSION_ACCEPTANCE_REPORT.md；
- 完整流程测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D16/D16-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确是否允许进入 D16-S02，以及所有阻塞项。
```
</details>

## D16-S02：最终文档和交接

<details>
<summary>展开/复制：D16-S02 完整提示词</summary>

```text
你是《鲸骨航线》的最终文档和交接 Agent。请直接完成 D16-S02。

任务编号：D16-S02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D16-S01 必须通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8-D16 全部主成果文件、全部阶段验收报告、全部交接报告和记忆日志。

执行内容：更新需求状态、设计状态、内容目录、契约版本、存档版本、路线图、测试报告、已知问题、资产授权、历史审查、性能报告和记忆日志。

任务约束：
- 只有实际通过验收的内容才能标记为 PRODUCTION/DONE；
- 未完成或有阻塞的内容必须保持 TODO、IN_PROGRESS 或 BLOCKED；
- 不得删除历史报告和失败记录；
- 不得擅自改变需求、设计或契约事实；
- 不得在 Systems 中加入网络或 LLM；
- 不得替用户执行真实 Steam 发布或外部沟通。

验收要求：需求、设计、内容、契约、存档、路线图、测试、授权、历史审查、性能和已知问题都有最终状态和链接。

结果必须保存到：
- 主成果：docs/game/design/common/D16_FINAL_HANDOFF_INDEX.md；
- 状态更新：对应需求、设计、路线图和报告文件；
- 交接报告：docs/game/design/common/handoffs/D16/D16-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

只有本任务完成后，才能把对应需求标记为 PRODUCTION/DONE。
```
</details>
