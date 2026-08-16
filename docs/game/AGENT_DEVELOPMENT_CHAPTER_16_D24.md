# 第 16 章开发任务提示词：D24 内容扩量、性能、打磨与本地可玩候选版

> 本章目标：在 D23 样板闭环稳定后，把正式表现扩展到 50 岛目录、1,211 植物、300 动物和代表港口/船舶/建筑，并完成性能、UI、音频和人工本地验收。执行顺序：`D24-S01 → D24-A01 → D24-A02—A07 → D24-A08 → D24-A09 → D24-S02`。
>
> D24-S02 只能验收“本地可玩候选版”。D16 外部门禁未关闭时，不得宣称 Steam Ready 或正式发布完成。

## D24-S01：冻结扩量质量等级、数量口径与性能预算

<details>
<summary>展开/复制：D24-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D24 内容扩量与性能总控 Agent。请直接完成 D24-S01。

任务编号：D24-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S02 PASS。
并发规则：本任务完成前不启动 D24-A01—A09；只冻结计划，不批量修改资源或 Registry。

开始前读取：AGENTS/CONTEXT/规范、REQUIREMENTS 数量目标、D15 全部批量目录/QA、D16 A01/A04/A05 当前审计、D18 资源/替换映射、D19—D23 验收、environment streaming/performance 和各领域 QA。

执行内容：
- 对齐权威数量：50 岛、1,211 个现有植物条目（仍满足 1,000—2,000 目标）、300 动物/鱼类、500 道具/服装/装饰、166 船舶资产、60 船员预设/500 NPC 组合及建筑/音乐目录；禁止另建第二套统计；
- 冻结表现质量等级：Q4 圣玛格丽特英雄样板；Q3 至少 12 座首批可登陆/可玩岛和核心角色/船/玩法；Q2 其余目录具有可辨识视觉根、LOD、流式和明确 fallback；ReleaseApproved 仍由 D16 人工门禁决定；
- 明确“内容条目不等于独立模型”：每个条目必须有诚实的 model/material/variant 映射，但共享基础模型必须登记，禁止换色凑独立模型；
- 定义 D24-A01—A09 的唯一写者、Registry 串行规则、资源批次、磁盘/Git 大小上限和回滚点；
- 冻结 50 岛流式、同屏植被/动物/NPC/船、LOD、材质、内存、加载、GC、1080p 三档性能预算；
- 定义自动 QA 抽样比例和人工验收：50 岛全部结构检查，至少 12 岛实际进入；植物不少于 50 条、动物不少于 30 条近景抽查且覆盖全部类别；
- 列出 D16 外部门禁与 D24 本地门禁的区别，禁止状态偷换。

只允许修改 D24 扩量冻结文档、交接和记忆；不改资源/Registry/代码/场景/构建；不提交或推送 Git。

验收：数量来源、Q 等级、抽样、预算、写者边界、批次/回滚和 D16 区分完整；每个后续任务有输入/输出和 PASS 条件；git diff --check。

结果必须保存到：
- 主成果：docs/game/design/common/D24_CONTENT_SCALE_PERFORMANCE_FREEZE.md、game/tools/results/D24/D24-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D24/D24-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出权威数量、质量等级、预算和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A01：正式资源替换批次、去重与 Registry 串行接入

<details>
<summary>展开/复制：D24-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D24 正式资源替换与 Registry 唯一写者 Agent。请直接完成 D24-A01。

任务编号：D24-A01（同步性质的资产数据任务；本章编号保持 A，但禁止与任何 Registry 写入并行）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-S01 PASS；D18-S02-v2、D18-S03、D23-S02 保持 PASS。D18-S02-v2 只代表资源准入，不替代 D18-S03 的运行时晋级证据。
单一写者：D18 replacement map、正式 Art/Animations/Audio/UI 资源晋级目录、Content Registry 中仅与资源引用/asset_state 相关字段、专属工具/测试/文档。执行期间其他 Agent 对 Registry 只读。

开始前读取：D24 冻结、D18 全部 manifest/LocalDevelopmentReady 清单/替换映射、D16 A04/A05 审计与 replacement_preflight、Registry 数据治理、D19—D23 实际运行时消费者。

执行内容：
- 只选择来源、许可证、哈希、导入、Prefab/消费者、碰撞/锚点/LOD 和测试证据完整的 LocalDevelopmentReady 资源；
- 按 P0→P1→P2 批次执行预检，先闭合 D23 完整流程使用的玩家、NPC、圣玛格丽特、起始船、核心 UI/音频，再处理首批 12 岛和扩量资源；
- 将候选源文件整理到 ARCHITECTURE 规定的正式 Models/Textures/Materials/Animations/Audio/UI 目录，避免同一二进制多份复制，保留许可证/manifest 与 GUID 迁移记录；
- 更新 Registry 的 model_ref/material_ref/prefab consumer/asset_state 时保留 content_id、owner、来源和旧存档兼容；只能把有真实消费者的条目标记 integrated；
- ReleaseApproved/人工审查字段保持原状，不由本 Agent签署；LocalDevelopmentReady/integrated 不等于正式发布批准；
- 每批运行 preflight、Registry 全图、断链、重复哈希、GUID/Prefab、Unity 导入和回归；失败整批回滚或保持未集成；
- 输出替换前后权威统计、仓库大小、重复资源、未替换原因和后续 A02—A07 可使用清单。

硬约束：不修改玩法数值、系统 owner、存档 Schema、敏感内容审查决定；不改主场景/强逻辑；不删除旧 fallback 直到兼容测试通过；不伪造 placeholder 降幅；不提交或推送 Git。

验证：D16 A05 审计、D18 manifest 审计、Registry QA、Unity 编译/资源测试、核心 D23 回归、git diff --check。任何断链/非法状态/测试回退时不得 PASS。

结果必须保存到：
- 主成果：正式资源及 Registry 合法引用、docs/game/design/common/D24_ASSET_REPLACEMENT_WAVES.md、game/tools/results/D24/D24-A01/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告必须给出替换前后 integrated/placeholder/data_only 数、每批条目、磁盘增量、人工未批状态、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A02：50 座岛屿视觉根、地貌差异与分区流式入口

<details>
<summary>展开/复制：D24-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 50 岛视觉扩展 Agent。请直接完成 D24-A02。

任务编号：D24-A02（异步世界资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成可用资源清单。
单一写者：game/Assets/Prefabs/Islands/D24/、game/Assets/Scenes/D24/IslandWork/、岛屿视觉配置/测试/文档。禁止写 Registry、正式主场景和流式系统代码。

开始前读取：D24 冻结、D15 50 岛目录/8 地貌模板、islands 全套设计、D19 圣玛格丽特样板、D16 数据流现状、A01 正式资源清单和每个 island_id 自然档案。

执行内容：
- 为权威 50 个 island_id 逐一建立可加载 VisualRoot 配置/Prefab，保持自然档案、地质、山地/坡地、海岸、暗礁、淡水、洞穴、生境、登陆点和社会/港口入口；
- Q4 圣玛格丽特保持 D19 质量；至少 12 座 Q3 岛具备独立轮廓、海岸/登陆、代表地貌、可行走/碰撞和玩法锚点；其余 38 座至少 Q2，有独立几何参数、轮廓/地标/岸型/生境组合和 LOD，不允许只改颜色与名称；
- 复用 8 类地貌模块但记录每岛差异；共享模型不伪装成 50 个独立模型；
- 每岛建立 near/mid/far LOD 或分区块、海图远景、边界、碰撞和安全恢复点；
- 不在本任务写流式加载逻辑，只提供稳定 island_id→VisualRoot/Cell 清单给 A08；
- 建立全部 50 岛结构/引用自动检查，至少 12 岛实际加载、登陆、路线、卸载/重载截图和测试。

硬约束：不修改 IslandSystem/发现/资源/任务/产权强逻辑，不改 Registry/主场景/流式代码；只用 A01 正式可用资源；不复制圣玛格丽特场景 49 次；不提交或推送 Git。

验证：50/50 VisualRoot 无断链/缺材质；12+ 岛实测可加载/登陆，Q 等级和差异证据完整；记录磁盘、三角面、LOD/碰撞、截图；git diff --check。

结果必须保存到：
- 主成果：上述岛屿 Prefab/工作场景/配置/测试、docs/game/design/common/D24_FIFTY_ISLAND_VISUALS.md、game/tools/results/D24/D24-A02/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 50 岛逐项状态、12 岛实测、共享/独立资源、缺口和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A03：1,211 个植被条目的模型映射、生长变体与生境群落

<details>
<summary>展开/复制：D24-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的植被规模化表现 Agent。请直接完成 D24-A03。

任务编号：D24-A03（异步植被资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成；读取 A02 岛屿生境接口但不得修改 A02 文件。
单一写者：game/Assets/Prefabs/Vegetation/D24/、植被材质/LOD/群落配置/测试/文档。禁止写 Registry、EcologySystem、岛屿 Prefab 和正式场景。

开始前读取：D24 冻结、D15 1,211 植物目录/QA、vegetation 全套设计、D19 生境样板、A01 正式资源、D16 placeholder 映射。

执行内容：
- 对权威 1,211 个 plant_id 建立诚实的 visual mapping：base model family、形态/生长阶段、季节/湿润/枯萎材质、LOD、风场、交互代理和 fallback；
- 目标使用不少于 150 个通过审核的基础视觉家族或具有实质网格/形态差异的组合，覆盖乔木、灌木、草本、蕨、苔藓、附生、海滨、湿地、农作物、果树、药材、香料；合法资源不足时按真实数 PARTIAL，禁止纯换色凑 150；
- 内容条目可共享模型，但文档必须区分 base model、material/stage variant 和独立物种条目；
- 建立生境群落/密度/随机种子/禁布区配置，支持 50 岛差异、道路/建筑避让、风动、LOD、远景代理和采集近景代理；
- 表现层只读取 Ecology 状态映射成熟/枯萎/受损，不修改生长、采集、恢复或库存；
- 全量 1,211 映射自动检查；至少 50 条近景样本覆盖全部类别/阶段/毒性药材标签并实际加载测试。

硬约束：不改 plant_id/目录数量/生态数值/Registry；不把同一模型颜色变体称独立模型；不让每个实例有高成本碰撞/Animator；只用 A01 清单；不提交或推送 Git。

验证：1,211/1,211 映射或明确 blocked，无断链；50+ 样本加载/风动/LOD/交互代理测试；记录基础模型真实数、共享率、同屏性能；git diff --check。

结果必须保存到：
- 主成果：上述植被 Prefab/配置/测试、docs/game/design/common/D24_VEGETATION_VISUAL_MAPPING.md、game/tools/results/D24/D24-A03/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 1,211 映射、真实基础模型数、类别覆盖、50 样本、性能和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A04：300 个动物/鱼类条目的模型、动作、群体与 LOD 映射

<details>
<summary>展开/复制：D24-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的动物规模化表现 Agent。请直接完成 D24-A04。

任务编号：D24-A04（异步动物资产任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成；读取 D20 动物样板和 A02 岛屿生境。
单一写者：game/Assets/Prefabs/Animals/D24/、动物动画/材质/群体配置/测试/文档。禁止写 Registry、EcologySystem、岛屿 Prefab 和正式场景。

开始前读取：D24 冻结、D15 300 动物/鱼类目录、animals 设计、D20 动物验收、A01 正式资源和 D16 placeholder 映射。

执行内容：
- 对权威 300 个 animal_id/fish_id 建立 visual mapping：基础模型家族、年龄/性别/颜色变体（仅目录有定义时）、动作集、地面/空中/水中层级、群体代理、LOD、碰撞和 fallback；
- 至少完成 30 个覆盖全部动物类别和行为等级的近景代表条目，目标建立不少于 30 个真实基础动画模型家族；资源不足按真实数 PARTIAL，不得换色凑数；
- 其余条目允许诚实复用最接近模型/远景代理，但不能把物种名不同当作模型已完成；必须标明 shared/fallback/needs_asset；
- 建立鱼群、鸟群和远距动物的实例/群体方案，近景才启用完整骨骼/碰撞；
- 表现只读 Ecology/Animal 状态，不修改种群、食物链、捕猎/捕捞、奖励和刷新；
- 全量 300 映射检查，30+ 代表实际加载/动作/地面水体/LOD/性能测试。

硬约束：动物逻辑禁止 LLM；不改 Registry/目录/生态强逻辑；不伪造模型或动作；珍稀/保护/捕猎敏感状态保持审查；只用 A01 清单；不提交或推送 Git。

验证：300/300 mapping 状态真实，无断链；30+ 代表覆盖陆生、鸟、爬行/小型、淡水/河口、近岸鱼、鲨鳐、海龟、鲸豚、甲壳/软体等目录实际类别；记录同屏性能；git diff --check。

结果必须保存到：
- 主成果：上述动物 Prefab/动画/配置/测试、docs/game/design/common/D24_ANIMAL_VISUAL_MAPPING.md、game/tools/results/D24/D24-A04/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 300 映射、真实模型家族、30 样本、fallback、性能和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A05：港口、建筑、道具、船舶与角色变体扩展

<details>
<summary>展开/复制：D24-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的世界人造内容扩量 Agent。请直接完成 D24-A05。

任务编号：D24-A05（异步资产组合任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成；读取 D19—D21 样板和 A02 岛屿接口。
单一写者：game/Assets/Prefabs/Buildings/D24/、Props/D24/、Ships/D24/Variants/、Characters/D24/Variants/、专属配置/测试/文档。禁止写 Registry、岛屿 Prefab、正式场景和强逻辑。

开始前读取：D15 建筑/道具/船/角色目录与 QA、buildings/items/ship/character 设计、D19 港口、D20 角色、D21 船、A01 正式资源和 D24 质量等级。

执行内容：
- 用模块化组合建立至少 12 座 Q3 岛所需的港口/聚落变体，覆盖码头、市场、仓库、酒馆、船坞、民居、管理/军事/宗教或社群建筑的实际目录允许类型；
- 为 200 功能道具和 300 装饰/服装条目建立诚实视觉映射，优先首航与 12 岛高频内容；共享网格/材质变体必须登记；
- 为现有 20 船体/80 模块/60 功能点目录建立代表 Prefab/挂点/fallback，不要求每个条目独立船模但禁止冒充；
- 为 60 船员预设/500 NPC 组合建立模块化外观映射，至少 20 个样本实际加载，保持文化/职业/审查状态；
- 所有建筑/船/道具有碰撞、交互锚点、LOD/批处理和缺失资源 fallback；表现层不写价格、功能、关系、战斗数值；
- 建立全量映射检查和 12 岛代表组合/抽样加载测试。

硬约束：不改 Registry/目录数量/业务数值/场景；不复制同一资产冒充独立模型；不引入未审资源；不把现代/时代错误物件标可用；不提交或推送 Git。

验证：映射/引用/Prefab/LOD/碰撞测试；12 港口或聚落变体可辨识，代表船/角色/道具抽检通过；记录真实独立模型与复用数、性能；git diff --check。

结果必须保存到：
- 主成果：上述 Prefab/映射/测试、docs/game/design/common/D24_BUILT_WORLD_CONTENT_VARIANTS.md、game/tools/results/D24/D24-A05/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告给出各目录映射统计、真实资产数、12 岛覆盖、缺口和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A06：全游戏 UI 缺陷清零与品质打磨

<details>
<summary>展开/复制：D24-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的 UI 品质打磨 Agent。请直接完成 D24-A06。

任务编号：D24-A06（异步 UI 修复任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成；D22-S02、D23-S02 PASS。
单一写者：D22 正式 UI 组件/样式/Presenter 中经缺陷单明确授权的修复、D24 UI 测试/文档。禁止修改玩法强逻辑和主场景非 UI 内容。

开始前读取：D22 全部验收/按钮矩阵/截图，D23 完整流程结果，D24 冻结，当前 bug/known issues、本地化/可访问性和全部正式 UI Prefab。

执行内容：
- 以 D23 30—45 分钟流程和所有主要页面建立 UI 缺陷矩阵，逐项修复重叠、裁切、层级、可读性、动画、Tooltip、空状态、加载、错误、焦点和重复点击；
- 确保所有可见按钮继续 working 或 disabled-with-reason，新增内容不能恢复死按钮；
- 统一分页/虚拟化、列表性能、数字/价格/日期格式、本地化、图标和主题；
- 检查 720p、1080p、16:10、21:9、4K、UI Scale、中英长文本、键鼠/手柄；
- 玩家模式零开发叠字，Debug 模式不遮挡操作；
- 每个修复都有复现步骤、前后截图和自动/人工验证，不顺便重构业务逻辑。

硬约束：UI 不能写业务数值或绕过 Command；不改 Systems/Schema/世界资源；不隐藏功能缺口；不提交或推送 Git。

验证：D22 A07 全矩阵和 D23 完整流程 UI 回归；缺陷矩阵 P0/P1 为 0，P2 有明确风险；截图/按钮统计；git diff --check。

结果必须保存到：
- 主成果：UI 修复/测试、docs/game/design/common/D24_UI_POLISH_REPORT.md、game/tools/results/D24/D24-A06/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出修复数、剩余级别、按钮/分辨率矩阵、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A07：环境音、船舶音、角色音、音乐主题与最终混音

<details>
<summary>展开/复制：D24-A07 完整提示词</summary>

```text
你是 WhalesIslandReef 的音频与音乐整合 Agent。请直接完成 D24-A07。

任务编号：D24-A07（异步音频任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A01 完成；D19/D21/D22/D23 音频事件接口可用。
单一写者：game/Assets/Audio/D24/、AudioMixer/音频配置、Presentation 音频适配、测试/文档。禁止修改玩法强逻辑和正式场景非音频内容。

开始前读取：audio 全套设计、D15 30 音乐主题/8 音频层目录、D18 合法音频 manifest、D19 港口音景、D21 船海天气音频、D22 设置/字幕/可访问性和 D23 流程事件。

执行内容：
- 整合港口、海岸、船坞、市场、船上、晴/雨/风暴、战斗、发现、经营和菜单等环境/交互音层；
- 为 D15 音乐主题目录建立诚实 mapping；实际合法曲目不足时按真实数标 shared/placeholder/needs_asset，禁止复制同一曲改名凑 30；
- 建立动态音乐状态适配，只读地点/天气/战斗/事件，不修改游戏状态；切换平滑、避免频繁重启；
- 统一 AudioMixer 总音量、音乐、环境、SFX、语音/对话分组，控制响度、峰值、空间衰减、循环、并发和对象池；
- 支持静音、字幕/视觉提示、低刺激和窗口失焦/暂停策略；
- 保留每个外部音频许可证、哈希和来源；逐项验证，不按站点整体推定；
- 建立事件映射、丢资源、循环/峰值、Mixer 设置保存、场景切换和 D23 全流程音频测试。

硬约束：不使用来源不明/NC 音频，不生成/伪造许可证；音频层不写 gameplay；不改 Systems/Schema/世界模型；不提交或推送 Git。

验证：Unity 编译/测试；D23 全流程无明显静音断层/爆音/无限叠加，Mixer/设置有效；输出真实曲目/音效数量、响度与许可证报告；git diff --check。

结果必须保存到：
- 主成果：上述 Audio/Mixer/适配/测试、docs/game/design/common/D24_AUDIO_MUSIC_MIX_REPORT.md、game/tools/results/D24/D24-A07/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出真实音频数、主题映射、许可、混音/测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A08：50 岛流式、LOD、对象池、内存与构建性能

<details>
<summary>展开/复制：D24-A08 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D24 性能与流式集成 Agent。请直接完成 D24-A08。

任务编号：D24-A08（串行性能任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A02—A07 全部有可用成果；D24-A01 完成。
并发规则：执行期间你是 Visual Streaming、LOD/对象池共享配置、质量档和性能测试场景的唯一写者；其他资产 Agent 停止修改被测 Prefab。

开始前读取：D24 冻结与 A02—A07 成果、D16 性能/数据流报告、environment streaming 设计、现有 12 data-only 岛流式注册、D19—D23 性能结果、Unity Profiler/构建配置。

执行内容：
- 建立或扩展只读 WorldVisualStreamer，将权威 50 island_id 的 VisualRoot/Cell 按距离加载/卸载；不改变 IslandSystem 实体和存档事实；
- 接入植被群落、动物群体、NPC、建筑、船舶、VFX、音频的 LOD/Culling/对象池/远距代理和模拟表现等级；
- 提供低/中/高三档并记录每档实际差异；降级只影响表现/频率，不跳过事件、保存或强逻辑；
- 检查 Asset/Scene 重复、纹理/材质、Shader variant、GC、内存泄漏、加载尖峰和场景卸载引用；按项目现有技术选择，不擅自引入大型插件或改变包版本；
- 建立 50 岛全目录流式自动测试、12 岛连续旅行压力测试、D23 十循环、场景反复加载/卸载和 30—60 分钟 soak；
- 采集 CPU/GPU/内存/GC/加载/帧率/同屏实体，超预算时定位并修复，不能删除内容或强逻辑掩盖；
- 生成本地开发构建并运行冒烟；不上传 Steam。

硬约束：不改 gameplay 结果/存档 Schema/contracts，不删除内容条目，不标正式发布；不覆盖外部门禁；不提交或推送 Git。

验证：50/50 视觉根可寻址且卸载不丢逻辑状态；12 岛连续旅行/soak 无增长性泄漏或关键错误；1080p 三档有真实指标；D23/核心回归通过；git diff --check。

结果必须保存到：
- 主成果：Visual Streaming/LOD/Pool/质量配置/测试、docs/game/design/common/D24_PERFORMANCE_STREAMING_REPORT.md、game/tools/results/D24/D24-A08/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A08_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出硬件、构建、50/12 岛结果、CPU/GPU/内存/GC/FPS、回归和 PASS/PARTIAL/BLOCKED。
```
</details>

## D24-A09：人工本地试玩、手柄、分辨率、可访问性与已知问题

<details>
<summary>展开/复制：D24-A09 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D24 人工 QA 组织与证据 Agent。请直接准备、执行可由你真实执行的检查，并对必须由人类/物理设备确认的项目保持诚实。

任务编号：D24-A09（串行人工 QA 门禁任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-A08 完成；D23-S02 PASS。
并发规则：本任务以只读测试和缺陷登记为主。修复 P0/P1 必须退回相应唯一模块 Agent，不在此跨模块随意修改。

开始前读取：D24 冻结、A01—A08 交接/结果、D16 本地化/输入/目标设备外部门禁、D23 完整流程、当前构建和 known issues。

执行内容：
- 制作并执行本地人工检查表：安装/启动、主菜单、新游戏、角色、港口、市场、NPC、登船、航行、天气、采集、战斗、返港、经营、保存/退出/继续；
- 在 1280×720、1080p、16:10、21:9、4K（硬件实际支持范围内）检查 UI/画面；不支持的配置标 Pending，不伪造；
- 用实际键鼠和实际已连接手柄检查移动、相机、UI 导航、重绑定、震动/低刺激、设备热切换；未检测到活动手柄则明确 PENDING_MANUAL；
- 检查中英、字幕、UI Scale、色盲可读、低刺激、暂停、音量、窗口失焦；
- 完成至少一次 30—45 分钟无调试/无作弊人工首航，并记录时间、截图、卡点、死按钮、UI 重叠、性能感受、音频、存档；
- 按 P0 阻断/P1 高/P2 中/P3 低登记缺陷，包含复现、期望、实际、截图/日志、归属任务；
- 不自动把“没有测到”写成 PASS，不替用户或责任审核者签署 D16 外部证据。

硬约束：不修改业务逻辑/资源来掩盖问题；不伪造设备、人工签名、截图、时间和测试；不上传 Steam；不提交或推送 Git。

验收：本机可执行项有真实证据；30—45 分钟人工流程完成；P0=0 才可 PASS，P1 必须为 0 或有用户明确接受；物理设备/分辨率不可测项保持 Pending 并阻止对应结论。

结果必须保存到：
- 主成果：docs/game/design/common/D24_MANUAL_LOCAL_QA.md、缺陷清单和 game/tools/results/D24/D24-A09/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-A09_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出真实设备/分辨率、人工时长、通过/失败/Pending、缺陷数量和 PASS/PARTIAL/BLOCKED。没有人类或物理设备证据时不能替代签署。
```
</details>

## D24-S02：本地可玩候选版最终验收与交接

<details>
<summary>展开/复制：D24-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D24 本地可玩候选版最终同步验收 Agent。请直接完成 D24-S02，严格区分本地候选与正式发布。

任务编号：D24-S02（同步最终验收任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D24-S01、D24-A01—A09 全部有交接；D23-S02 PASS。任何 P0、关键 P1、人工首航缺失、50 岛流式失败或核心资源断链都会阻止 PASS。
并发规则：验收期间冻结 Registry、正式场景、资源、UI、流式和构建配置写入。只允许最小验收修复；大修退回所属任务。

开始前读取：D24 全部成果/交接/机器结果、D23 验收、D16 当前外部门禁和 D18—D22 验收。对每项结论读取真实文件，不以 Agent 聊天回复为证据。

执行内容：
1. 审计数量与表现事实：50 岛 VisualRoot、1,211 植物 mapping、300 动物/鱼 mapping、建筑/道具/船/角色映射、资源许可证/哈希/消费者和真实 placeholder 统计；
2. 运行 D18 manifest/Registry/D15/D16 A05 只读审计、Unity 编译、全部相关 EditMode/PlayMode、AgentCore/contracts/server（若受影响）和 D23 十循环；
3. 运行 50 岛目录加载、12 岛连续旅行、性能 soak、UI 全分辨率、音频、保存恢复和完全离线流程；
4. 核对 A09 人工 30—45 分钟证据、键鼠/手柄/分辨率/可访问性和 P0/P1 缺陷；
5. 生成本地候选构建、版本说明、已知问题、资源署名/许可证汇总、回滚和下阶段建议；不得上传 Steam；
6. 单独列出 D16 仍未关闭的目标设备、跨平台、真实断电、人工审查、隐私和发布证据，不能被本任务覆盖；
7. 对每个需求数量区分 catalog、visual mapping、独立模型、shared variant、integrated 和 ReleaseApproved，避免误报。

本地 PASS 门槛：
- D23 10/10 完整离线循环与一次真实人工 30—45 分钟首航通过；
- 50/50 岛有合法视觉根，至少 12 岛实际旅行/加载通过；1,211 植物和 300 动物 mapping 无断链，抽样满足 D24 冻结；
- 玩家、NPC、船、港口、海水、天气、UI/音频没有回到主要占位画面；
- 可见按钮 100% working/disabled reason，P0=0，未接受 P1=0；
- 保存/重启/离线、性能、UI 矩阵和核心回归通过；
- 所有外部资源有许可证证据，ReleaseApproved 未被 Agent伪造；
- 报告诚实保留 D16 外部阻断。

硬约束：不修改强逻辑或审计口径来过门禁；不签署人工/设备/许可证责任决定；不上传、发布、提交或推送 Git，除非用户另行明确授权。

结果必须保存到：
- 主成果：docs/game/design/common/D24_LOCAL_PLAYABLE_CANDIDATE_ACCEPTANCE.md、版本说明/已知问题/许可证汇总和 game/tools/results/D24/D24-S02/；
- 交接报告：docs/game/design/common/handoffs/D24/D24-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论只能 PASS、PARTIAL 或 BLOCKED。即使 PASS，也必须写“本地可玩候选版通过，不等于 Steam Ready”；报告列出全部统计、测试、人工证据、性能、构建路径、D16 阻断和后续任务。
```
</details>
