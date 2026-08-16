# 第 15 章开发任务提示词：D23 30—45 分钟完整本地可玩闭环

> 本章目标：把已存在的港口、角色、船、UI 和玩法系统串成一段可从新游戏连续玩到返港经营并保存的真实流程。执行顺序：`D23-S01 → D23-A01—A08 → D23-S02`。异步任务只实现各段适配与测试，主流程编排、正式场景和共享 Bootstrap 仅由 S02 修改。

## D23-S01：冻结 30—45 分钟首航故事、状态与验收脚本

<details>
<summary>展开/复制：D23-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D23 可玩体验总设计与同步 Agent。请直接完成 D23-S01。

任务编号：D23-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S02、D20-S02、D21-S02、D22-S02 必须 PASS；若其中某章 PARTIAL/BLOCKED，本任务只能列阻断并停止，不能冻结不存在的能力。
并发规则：本任务完成前不启动 D23-A01—A08；不修改正式流程代码。

开始前读取：AGENTS/CONTEXT/规范、REQUIREMENTS/玩法总目录、D9 首航、D11—D14 各玩法验收、D19—D22 验收、当前新游戏/教程/港口/航行/保存流程和所有可用系统接口。

执行内容：
- 冻结一条 30—45 分钟首航脚本：主菜单/新游戏 → 角色预设 → 港口出生 → 与关键 NPC 对话 → 领取/确认目标 → 市场补给/整理货舱与船员 → 上船离港 → 海图/天气/航行 → 钓鱼或采集 → 发现地点/事件 → 代表性冲突或战斗 → 返港 → 售卖/结算 → 资产/农业/金融代表选择 → 保存退出 → 重开继续；
- 为每一步定义 start condition、玩家动作、系统 owner、成功 result code、失败分支、UI/音画反馈、存档点、超时/恢复、下一步和测试 ID；
- 明确强制步骤与可选步骤，避免教程要求玩家同时完成所有 D10—D14 内容；
- 设定初始资源、目标和数值只能引用现有配置/系统，不在本任务发明第二套经济；
- 定义完全离线可玩条件：LLM/服务端不可用时使用现有模板/本地降级，不能卡住主线；
- 为 A01—A08 划分唯一模块、共享 Journey State 接口和 S02 串行顺序；
- 建立 10 轮自动/半自动验收脚本和一次真实人工 30—45 分钟检查表。

只允许修改流程规格/状态接口文档、测试脚本设计、交接和记忆；禁止改代码、场景、Systems、存档 Schema、contracts；不提交或推送 Git。

验收：每一步可映射到现有真实系统；没有“点击后直接把钱/任务状态改掉”的假流程；成功、失败、离线、读档和恢复完整；A01—A08 边界明确；git diff --check。

结果必须保存到：
- 主成果：docs/game/design/common/D23_FIRST_VOYAGE_PLAYABLE_STORY.md、game/tools/results/D23/D23-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D23/D23-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出预计时长、步骤数、系统接口、阻断和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A01：主菜单、新游戏、角色创建与首段教程

<details>
<summary>展开/复制：D23-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的新游戏入口 Agent。请直接完成 D23-A01。

任务编号：D23-A01（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/NewGameFlow/、game/Assets/Prefabs/D23/NewGame/、专属测试场景/测试/文档。禁止修改正式主菜单场景、InitialHarbor 和共享 Journey Coordinator。

开始前读取：D23 首航脚本、D20 玩家预设、D22 UI/Input、当前 NewGame/Save 初始化、D8 数据/存档 Schema、D6/D7 教程和本地化。

执行内容：
- 建立可供 S02 接入的主菜单/新游戏流程模块：新游戏、继续、设置、退出；无存档时继续禁用并说明；
- 角色创建至少支持 D20 可用的性别/体型方向、发型、外观/服装预览和名称输入；只提交现有合法 CharacterPreset/Appearance Command；
- 新游戏初始化必须调用现有 Save/NewGame 事务，不由 UI 手工写钱、船、船员或任务；
- 进入港口后只显示当前教程步骤，支持跳过/重开；
- 处理无可用预设、非法名称、存档创建失败、重复点击、返回、关闭游戏和中英长文本；
- 建立新游戏成功/失败、继续有无存档、预设切换、重复点击幂等、教程跳过/恢复和读档测试。

硬约束：不改 Save/Character/Economy 强逻辑和 Schema；不绕过 command/result；不修改正式场景/主 UI 根/共享协调器；不伪造角色资源；不提交或推送 Git。

验证：Unity 编译/测试；成功新档包含现有系统规定的玩家/船/船员事实，失败不产生半档；所有按钮 working/disabled reason；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_NEW_GAME_CHARACTER_TUTORIAL.md、game/tools/results/D23/D23-A01/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出流程、实际 Command/Result、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A02：港口对话、补给、船员准备、登船与离港段

<details>
<summary>展开/复制：D23-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的首航港口准备段 Agent。请直接完成 D23-A02。

任务编号：D23-A02（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/PortPreparationFlow/、game/Assets/Prefabs/D23/PortPreparation/、专属测试/文档。禁止改正式场景、市场/NPC/船强逻辑和共享协调器。

开始前读取：D23 首航脚本、D9 港口/船员/市场/离港流程、D20 NPC 表现、D21 登船/停靠、D22 UI、当前关键 NPC/市场/库存/泊位/离港 Command 与 Result。

执行内容：
- 建立关键 NPC 对话/任务确认、市场补给、查看货舱、确认船员、前往泊位、登船和离港的流程适配；
- 每一步调用现有系统接口并等待真实结果，资金/库存/船员/任务/泊位只由所属系统写；
- 在世界交互和 UI 中提供下一目标、距离/条件、成功/失败和返回提示，不生成固定按钮墙；
- 处理资金不足、库存满、NPC 忙碌、货物已有、船员缺席、船不在泊位、登船失败、重复购买/重复离港和离线 Agent 降级；
- 流程可跳过已完成步骤并能从中途存档恢复，不重复赠送资源或结算；
- 建立成功路径与所有代表失败/恢复/幂等测试。

硬约束：不手写经济/库存/船员/航行数值，不改 Systems/存档 Schema/场景/共享 UI 根；适配器只编排已存在意图/结果；不提交或推送 Git。

验证：Unity 编译/测试；完整港口准备可从新档完成，失败有原因，重复执行不重复扣款/发货/推进；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_PORT_PREPARATION_FLOW.md、game/tools/results/D23/D23-A02/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出步骤、Command/Result、失败分支、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A03：钓鱼、海上采集、库存处理与贸易结算段

<details>
<summary>展开/复制：D23-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的采集与贸易可玩段 Agent。请直接完成 D23-A03。

任务编号：D23-A03（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/HarvestTradeFlow/、game/Assets/Prefabs/D23/HarvestTrade/、专属测试/文档。禁止改 Fishing/Ecology/Inventory/Economy 强逻辑和正式场景。

开始前读取：D23 首航脚本、D11 钓鱼/采集成果、D9/D13 贸易经济、D22 HUD/库存/市场、当前鱼场/渔具/货舱/处理品/买卖接口。

执行内容：
- 建立首航代表流程：找到合法鱼场/采集点 → 选择已有渔具/动作 → 等待系统结果 → 查看鱼获/失败原因 → 可选处理 → 放入货舱 → 返港出售；
- 所有鱼种、天气、深度、工具、成功率、资源压力、库存和价格由现有系统裁决；流程只提交意图、展示结果；
- 提供可读视觉/UI 反馈：位置是否合适、工具缺失、咬钩/收线、货舱满、生态限制、交易报价和结算；
- 处理重复捕捞/重复出售、离开区域、中断、暂停、读档、坏输入和库存不足；
- 首航至少能合法获得并出售一种鱼获/采集品，同时失败路径可恢复；
- 添加成功、工具缺失、位置错误、舱满、重复操作、生态限制、买卖失败和读档幂等测试。

硬约束：不修改成功率/价格/库存/生态数值，不绕过系统发物品/钱，不改 Systems、Schema、场景/主 UI；不生成假鱼获；不提交或推送 Git。

验证：Unity 编译/测试；完整采集到出售路径通过，系统账本/库存/生态事实一致，重复不多发奖励；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_HARVEST_TRADE_FLOW.md、game/tools/results/D23/D23-A03/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出使用系统、成功/失败路径、账本/幂等测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A04：船员对话、关系反馈与完全离线 Agent 降级

<details>
<summary>展开/复制：D23-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的船员社会与离线可玩性 Agent。请直接完成 D23-A04。

任务编号：D23-A04（异步流程/降级任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/CrewSocialFlow/、允许的 Agent 接入适配层（不能在 Systems）、专属 UI/测试/文档。禁止改 AgentCore Schema、关系/工资裁决和正式场景。

开始前读取：D9 Crew Agent 垂直切片、D10 关系/岗位/行为、DESIGN_BOUNDARIES、AgentReply Schema/验证、当前本地模板/服务端网关/超时降级和 D22 对话 UI。

执行内容：
- 建立首航关键船员对话与关系反馈流程：主动对话、任务相关回应、航行事件评论、拒绝/忙碌/关系变化提示；
- LLM/服务端可用时仍必须经过 AgentReply Schema 和系统裁决；不可用、超时、无账号或余额不足时自动使用现有本地模板/规则回复，主线不得卡死；
- Agent 只提议，工资、关系、岗位、任务和资源变化由对应系统裁决；
- 对话 UI 显示说话者、正文、可选回应/继续、等待、取消、超时/降级标识，但不暴露 token/内部调试信息；
- 处理重复回复、过期请求、场景切换、暂停、存档/读档和恶意/无效 AgentReply；
- 添加完全断网、超时、Schema 无效、本地模板、在线模拟成功、重复请求幂等和关系结果测试。

硬约束：Systems 和 AgentCore 不加入 HttpClient/LLM/Unity 依赖；不得绕过 contracts；不得把 LLM 文本直接当数值；不改 Schema/强逻辑/正式场景；不需要真实付费 API 或外部账号；不提交或推送 Git。

验证：AgentCore 独立 build（若触及相关项目）、Unity 编译/测试、contracts 测试；网络完全关闭时首航对话可完成；无效回复被拒绝且不改状态；git diff --check。

结果必须保存到：
- 主成果：上述流程/适配/测试、docs/game/design/common/D23_CREW_SOCIAL_OFFLINE_FALLBACK.md、game/tools/results/D23/D23-A04/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出在线/离线边界、Schema/裁决、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A05：离港航行、海图探索、天气和事件段

<details>
<summary>展开/复制：D23-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的首航探索段 Agent。请直接完成 D23-A05。

任务编号：D23-A05（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/VoyageExploreFlow/、game/Assets/Prefabs/D23/VoyageExplore/、专属测试/文档。禁止改 Navigation/Weather/Discovery/Event 强逻辑和正式场景。

开始前读取：D23 首航脚本、D9 航行/天气/航线、D12 海图/发现/线索、D21 船海表现、D22 HUD/地图、当前离港/目的地/发现/事件/返港接口。

执行内容：
- 建立离港后选定代表目的地、跟随航向/海图提示、经历至少一种天气变化、触发可恢复事件、发现地点/标记并返航的流程适配；
- 船速、风、天气、航程、发现条件、事件概率和奖励全部由已有系统裁决；
- HUD/海图显示方向、距离、天气、危险和发现反馈，不泄露未发现精确内容；
- 处理错误方向、暗礁/浅水警告、船停住、天气恶化、事件拒绝/失败、目的地已发现、场景加载失败和中途读档；
- 首航流程可以在完全离线下完成，不依赖实时服务器；
- 添加成功航程、偏航、天气、已发现、事件失败、读档继续、重复发现幂等和返港测试。

硬约束：不写航行/天气/发现/奖励数值，不绕过事件系统；不改 Systems/Schema/正式场景/主 UI；不把航线自动传送冒充驾驶（测试夹具可受控模拟但必须标明）；不提交或推送 Git。

验证：Unity 编译/测试；代表航程可实际驾驶完成，发现和事件只结算一次，读档恢复位置/目标/天气事实；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_VOYAGE_EXPLORE_FLOW.md、game/tools/results/D23/D23-A05/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出航程步骤、系统结果、失败恢复、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A06：产权、种植、经营与金融代表性选择段

<details>
<summary>展开/复制：D23-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的岛屿经营与金融首航切片 Agent。请直接完成 D23-A06。

任务编号：D23-A06（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/ManagementFinanceFlow/、game/Assets/Prefabs/D23/ManagementFinance/、专属测试/文档。禁止改 Property/Farming/Finance/Economy 强逻辑和正式场景。

开始前读取：D13 产权、金融、农业验收与设计、D23 首航脚本、D22 资产/金融 UI、当前调查/租用或购买/播种/贷款或赊账/时间推进 Command 和结果。

执行内容：
- 为首航返港后提供一个小而真实的代表选择，例如调查一个可用地块、查看租赁/购买条件、播种一项作物或选择不经营；另提供查看信用/小额融资条件但不强迫借款；
- 所有产权、价格、利息、作物、时间、风险和资格由现有系统裁决；流程只展示并提交 Command；
- 清楚显示成本、收益不是保证、时间、债务、抵押/失败风险和取消；
- 处理资金不足、资格不足、地块占用、季节/种子/水源缺失、贷款拒绝、重复购买/播种和读档；
- 首航主线允许玩家拒绝/暂缓，不能因不借款或不买地卡死；
- 添加成功/拒绝/取消/重复/读档/账本一致性测试。

硬约束：不直接改钱、产权、债务、作物和时间，不弱化金融风险提示，不写第二套公式；不改 Systems/Schema/正式场景/主 UI；不提交或推送 Git。

验证：Unity 编译/测试；代表选择真实走系统，账本/产权/作物/债务一致，拒绝也能继续主线；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_MANAGEMENT_FINANCE_FLOW.md、game/tools/results/D23/D23-A06/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出可选项、风险/结果、账本测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A07：代表性海战、奖励、法律与势力后果段

<details>
<summary>展开/复制：D23-A07 完整提示词</summary>

```text
你是 WhalesIslandReef 的首航战斗与后果切片 Agent。请直接完成 D23-A07。

任务编号：D23-A07（异步流程模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS。
单一写者：game/Assets/Scripts/Gameplay/D23/CombatConsequenceFlow/、game/Assets/Prefabs/D23/CombatConsequence/、专属测试/文档。禁止改 Combat/Reward/Faction/Law 强逻辑和正式场景。

开始前读取：D14 海战/特殊武器/奖励/势力法律验收、D21 船损表现、D23 首航脚本、D22 战斗 HUD、当前战斗意图/伤害/胜负/投降/逃离/奖励请求/势力事件接口。

执行内容：
- 建立一个可选或低风险代表冲突：遭遇敌对/海盗目标后可战斗、逃离或按系统允许的替代方式处理；
- 战斗输入和 UI 只提交现有 Aim/Fire/Repair/Escape 等意图，命中、伤害、弹药、冷却、胜负由系统裁决；
- 显示目标、射界/冷却、己方船体、受损、逃离条件和结果；
- 胜利/失败/逃离后通过 Reward/Law/Faction 现有接口结算一次，显示战利品、声望/法律或关系后果；
- 处理无弹药、目标无效、重复开火/结算、玩家沉没/失败恢复、读档和完全离线；
- 建立胜利、失败、逃离、无弹药、重复奖励幂等、势力/法律后果和存档恢复测试。

硬约束：不改命中/伤害/奖励/声望/法律数值，不从 UI 发奖励，不绕过系统；不改 Systems/Schema/正式场景/主 UI；不把自动胜利作为正式流程；不提交或推送 Git。

验证：Unity 编译/测试；三种结果都可继续或明确失败恢复，奖励一次，势力/法律事实可追溯；git diff --check。

结果必须保存到：
- 主成果：上述流程模块/Prefab/测试、docs/game/design/common/D23_COMBAT_CONSEQUENCE_FLOW.md、game/tools/results/D23/D23-A07/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出结果分支、系统裁决、幂等/恢复、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-A08：全流程保存、退出、重开、失败与卡死恢复

<details>
<summary>展开/复制：D23-A08 完整提示词</summary>

```text
你是 WhalesIslandReef 的首航可靠性与恢复 Agent。请直接完成 D23-A08。

任务编号：D23-A08（异步测试/恢复适配任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01 PASS；读取 A01—A07 的流程状态接口，不能修改它们的业务实现。
单一写者：game/Assets/Scripts/Gameplay/D23/JourneyRecovery/、game/Assets/Tests/D23/Recovery/、结果/文档。禁止改 SaveSystem 数据所有权和正式场景。

开始前读取：D8/D16 保存迁移/事务/中断测试、D23 首航状态设计、A01—A07 接口、当前保存、场景切换、错误恢复和主菜单继续流程。

执行内容：
- 定义并实现 Journey 恢复适配：从每个合法存档点恢复 UI 目标和流程提示，但真实玩家/船/货物/钱/任务/关系/天气/产权仍从各系统存档读取；
- 覆盖港口准备中、已登船、航行中、采集后、事件/战斗后、返港结算后、经营选择后保存退出重开；
- 对非法/过期流程状态进行安全重建或回到最近稳定步骤，不重复购买、奖励、发现、战斗结算、贷款或播种；
- 处理场景加载失败、目标 Prefab 缺失、角色落水/卡住、船不在合法位置、UI 焦点丢失和离线服务不可用；
- 提供“返回安全点/重载最近存档/回主菜单”明确选项，不静默吞错；
- 添加流程中断、应用重启模拟、损坏 Journey 状态、幂等、十次重复保存加载和既有 Save 测试回归。

硬约束：不改变 Save Schema/事务所有权，除非发现确需迁移且必须停止并报告跨模块阻断；不直接修正业务数值；不删除坏档或覆盖原档；不改正式场景/主 UI；不提交或推送 Git。

验证：Unity/独立 Save 相关测试；代表存档点全部恢复，坏状态不损坏业务档，重复结算为 0；记录测试计数和恢复日志；git diff --check。

结果必须保存到：
- 主成果：上述 Recovery 适配/测试、docs/game/design/common/D23_JOURNEY_SAVE_RECOVERY.md、game/tools/results/D23/D23-A08/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-A08_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出存档点、失败注入、幂等、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D23-S02：首航主流程串行集成、十循环与人工可玩验收

<details>
<summary>展开/复制：D23-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D23 主流程集成与可玩性验收 Agent。请直接完成 D23-S02。

任务编号：D23-S02（同步集成任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D23-S01、D23-A01—A08 全部有可用成果；D19—D22 S02 全部 PASS。
并发规则：你是 Journey Coordinator、正式主菜单/InitialHarbor/航行入口、共享 Bootstrap 和 D23 集成测试的唯一写者；集成期间暂停相关修改。

开始前读取：全部 D23 主成果/交接/机器结果、D19—D22 验收、现有 D17 十循环、Save/Agent 离线回归和 D23 首航脚本。

执行内容：
1. 建立单一 Journey Coordinator，只编排 A01—A08 的步骤和稳定 result，不复制各系统状态；
2. 将新游戏、港口准备、采集贸易、船员离线对话、航行探索、经营金融、战斗后果和保存恢复串行接入正式场景/UI；
3. 每步支持成功、失败/取消、跳过可选项、读档恢复；任何奖励/扣款/关系/发现/战斗结算只由原系统发生一次；
4. 在网络完全断开条件下运行完整流程；LLM 不可用不能卡住；
5. 执行至少 10 轮自动或确定性完整故事循环，每轮从新档或隔离档开始，并执行一次真实人工 30—45 分钟流程；
6. 运行 D17、D19—D22、Save、AgentCore/contracts 和核心 PlayMode 全量回归；
7. 记录每轮耗时、步骤、失败/恢复、经济账本、存档、FPS、死按钮、UI 重叠和 Console 错误；
8. 不允许为了测试通过使用正式流程看不到的作弊按钮、自动传送或直接改状态；测试夹具只能驱动真实公开意图并明确标记。

硬约束：不改各 Systems 数值/所有权、Schema/contracts/server 语义；协调器不能成为第二个业务系统；前置必须包含 D18-S03 PASS，且只能使用 D18-S03 真实晋级的 LocalDevelopmentReady 资源；不把本地通过写成 Steam Ready；不提交或推送 Git。

通过门槛：
- 玩家能从主菜单连续完成 30—45 分钟脚本并返港保存/重开继续；
- 10/10 循环全部通过，零重复结算、丢档、关键卡死、必现死按钮或严重 UI 重叠；
- 完全离线下流程通过，无效 AgentReply 不改变状态；
- 环境、角色、船海和 UI 正式表现贯穿全流程，不回退到主视图白色方块/柱状角色/简单船；
- 所有相关回归通过，性能/截图/测试结果可复核；
- 若人工 30—45 分钟验收无人执行，只能 PARTIAL，不得 PASS。

结果必须保存到：
- 主成果：Journey Coordinator/正式集成/测试、docs/game/design/common/D23_FULL_PLAYABLE_LOOP_ACCEPTANCE.md、game/tools/results/D23/D23-S02/；
- 交接报告：docs/game/design/common/handoffs/D23/D23-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED；报告必须列出 10 轮逐轮结果、人工签名/日期、离线结果、账本/存档、测试计数、截图、性能和阻断项。
```
</details>
