# 第 14 章开发任务提示词：D22 UI/UX 全面重构

> 本章目标：消除截图中的面板堆叠、调试文字污染、死按钮和焦点冲突，建立可读、可点击、可本地化、支持键鼠/手柄的正式 UI。执行顺序：`D22-S01 → D22-A01—A07 → D22-S02`。只有 S02 可以重组正式主 Canvas/场景 UI 根。

## D22-S01：冻结信息架构、页面层级、输入与分辨率规范

<details>
<summary>展开/复制：D22-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D22 UX 总控 Agent。请直接完成 D22-S01。

任务编号：D22-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；读取 D19—D21 已完成成果和当前运行截图。若 D19—D21 尚未全部集成，可先冻结架构，但必须对实际 UI 做清点；D18-S03 负责最终运行时资源晋级。
并发规则：本任务完成前不启动 D22-A01—A07；只冻结规范，不重构正式 Canvas。

开始前读取：AGENTS/CONTEXT/规范、D18 视觉/合法 UI 资源、ui 全套设计、D16 本地化/可访问性/输入、D9—D14 玩法入口、当前所有 Canvas/EventSystem/OnGUI/IMGUI/按钮/快捷键代码和用户提供的 UI 堆叠截图。

执行内容：
- 清点所有可见 HUD、调试叠层、教程、通知、交互按钮、地图、市场、库存、船员、船舶、资产/金融、任务、设置和弹窗；记录实际脚本、Prefab、场景、快捷键和点击处理器；
- 冻结层级：World、PlayerHUD、ContextPrompt、Notification、Panel、Modal、Tutorial、Pause、Debug；规定一次允许显示的层和互斥/返回行为；
- 设计主 HUD 和主要页面线框/布局，明确每个面板的信息优先级、打开/关闭、焦点、暂停策略、空状态、加载、成功、失败和禁用反馈；
- 冻结 16:9、16:10、21:9、4K 缩放及 1280×720 最低分辨率规则、安全区、字体最小尺寸、颜色/对比度和长中英文；
- 定义键鼠/手柄导航、Escape/B 返回、焦点恢复、鼠标锁定、游戏输入屏蔽和 EventSystem 唯一性；
- 为 A01—A07 划分唯一写入边界，主 Canvas/主场景只给 S02；
- 建立“所有可见按钮必须 Handler 成功、明确禁用原因或明确未开放”的死按钮零容忍门禁。

只允许修改 UX 规范、UI 清点/线框文档、交接和记忆日志；禁止改正式 Canvas、场景、代码、Systems、存档和 contracts；不提交或推送 Git。

验收：完整 UI 清点表、层级/互斥状态机、主要页面线框、分辨率矩阵、输入焦点规则、按钮处理器矩阵、A01—A07 边界和 S02 验收脚本齐全；git diff --check。

结果必须保存到：
- 主成果：docs/game/design/common/D22_UI_INFORMATION_ARCHITECTURE.md、game/tools/results/D22/D22-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D22/D22-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出实际 UI 根/脚本、已发现死按钮/堆叠原因、冻结规则和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A01：调试 HUD 与玩家 HUD 完全分离

<details>
<summary>展开/复制：D22-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的调试界面治理 Agent。请直接完成 D22-A01。

任务编号：D22-A01（异步 Debug UI 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：DebugOverlay/Diagnostics UI 相关脚本与专属 Prefab、game/Assets/UI/D22/Debug/、测试/文档。禁止重组正式主 Canvas 和玩法面板。

开始前读取：D22 UI 架构/清点表、当前 D5/D7/D9 等 RuntimeBootstrap 中 OnGUI/IMGUI/调试文字、性能监控、自动化状态、教程和事件日志实现。

执行内容：
- 将 D5/D7/D9 阶段标签、FPS/自动化/日志/只读状态等开发信息统一纳入 DebugOverlayGate 或等价单一入口；
- 玩家模式和正常 Play 默认关闭全部开发叠字；开发模式可通过明确设置/命令切换并记住本机偏好，但不写玩家存档业务数据；
- 区分玩家需要的任务/教程/反馈与开发日志，不能把新手引导一起隐藏；
- 调试层必须有独立 Canvas/排序层/安全边界，可滚动或分页，不能覆盖正式按钮并拦截点击；
- 保留自动化测试读取接口，不能为了画面干净删除诊断事实；
- 对每个旧 OnGUI/调试入口记录迁移结果，禁止遗漏重复绘制；
- 添加默认关闭、开发开启、点击穿透/拦截、分辨率和旧测试兼容测试。

硬约束：只改表现/诊断入口，不改各系统状态、事件、自动化逻辑和日志事实；不改 Systems/存档/contracts、主 Canvas/场景；不删除测试依赖字段；不提交或推送 Git。

验证：Unity 编译和测试；正常模式截图无 D5/D7/D9/FPS/自动化堆叠，开发模式信息仍可用；Debug 关闭时不抢鼠标/手柄焦点；git diff --check。

结果必须保存到：
- 主成果：DebugOverlay 代码/Prefab/测试、docs/game/design/common/D22_DEBUG_UI_SEPARATION.md、game/tools/results/D22/D22-A01/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出迁移入口、默认状态、兼容性、截图/测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A02：正式响应式 HUD、状态条、任务与通知

<details>
<summary>展开/复制：D22-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的正式 HUD Agent。请直接完成 D22-A02。

任务编号：D22-A02（异步 HUD Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：game/Assets/UI/D22/HUD/、game/Assets/Prefabs/UI/D22/HUD/、game/Assets/Scripts/Presentation/D22/Hud/、测试/文档。禁止修改主 Canvas、场景和其他面板。

开始前读取：D22 架构/线框、D18 合法 UI/输入资源、D16 本地化/可访问性、D9—D14 玩家关键状态和现有 HUD 数据来源。

执行内容：
- 制作响应式 PlayerHUD Prefab，包含地点/时间/天气、角色关键状态、船舶/航行状态、资金/货物摘要、当前目标和精简通知区；
- 港内/步行/掌舵/航行/战斗等模式只显示相关信息，避免把所有系统状态同时铺在屏幕；
- HUD 通过只读 ViewModel/Presenter 订阅已有状态，不直接改钱、库存、船速、任务或天气；
- 任务和通知有优先级、合并、超时、历史入口，错误/危险不能被普通日志挤掉；
- 支持中英长文本、字体回退、颜色/图标双编码、UI Scale 和安全区；
- 提供 1280×720、1920×1080、2560×1080、3840×2160 预设截图和布局测试；
- 没有数据时显示明确空状态，不显示 null/内部 ID/调试字段。

硬约束：不改系统数值/所有权/存档，不在 Systems 加 UI；不改主 Canvas/场景、EventSystem、其他面板；工作场景/Prefab 阶段只能使用准入目录中的 `intake_eligible` UI/字体/音频资源；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；四分辨率中无重叠/裁切，港内/步行/航行模式信息正确，长中英文可读，Presenter 只读；git diff --check。

结果必须保存到：
- 主成果：上述 HUD Prefab/Presenter/测试、docs/game/design/common/D22_RESPONSIVE_PLAYER_HUD.md、game/tools/results/D22/D22-A02/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 HUD 模式、分辨率截图、数据来源、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A03：市场、库存、船员、地图、船坞、资产和设置页面

<details>
<summary>展开/复制：D22-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的主要游戏面板 Agent。请直接完成 D22-A03。

任务编号：D22-A03（异步页面 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：game/Assets/UI/D22/Panels/、game/Assets/Prefabs/UI/D22/Panels/、game/Assets/Scripts/Presentation/D22/Panels/、测试/文档。禁止修改主 Canvas/场景、Context Prompt 和 EventSystem。

开始前读取：D22 架构/线框、D18 UI 资源、D9 市场/货物/船员、D12 地图、D13 资产金融、D14 战斗、D16 设置/本地化成果和当前各操作 API/结果码。

执行内容：
- 建立统一 Shell 和页面 Prefab：市场、库存/货舱、船员/关系、海图/发现、船舶/船坞改装、任务/日志、产权/农业/金融代表页、角色/服装、设置/可访问性；
- 页面只通过现有 Query/ViewModel 读取，通过已有 Command/意图接口提交；成功后刷新，失败显示实际 result code 的本地化信息；
- 每页具备标题、返回、筛选/排序（适用时）、空状态、加载、禁用原因、确认危险操作和键鼠/手柄导航元数据；
- 不把所有 2,000+ 条内容一次实例化；长列表采用分页、虚拟化或受控加载；
- 价格、库存、债务、关系、改装、资产状态不能由 UI 自己计算最终结果；
- 建立至少每页打开/关闭、空/有数据、成功/失败/禁用、长文本和焦点测试；
- 当前没有后端功能的入口必须明确 Disabled/ComingLater，不得显示可点击但无响应按钮。

硬约束：不新建第二套业务状态，不写 Systems 数值，不绕过 contracts；不改主 Canvas/场景/EventSystem/HUD；不硬编码中英文；不提交或推送 Git。

验证：Unity 编译/测试；所有页面 Prefab 完整、无 Missing Script，代表数据可渲染，所有可见 Action 有 handler 或禁用原因；记录页面/按钮矩阵；git diff --check。

结果必须保存到：
- 主成果：上述页面/Shell/Presenter/测试、docs/game/design/common/D22_GAMEPLAY_PANELS.md、game/tools/results/D22/D22-A03/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出页面、按钮矩阵、未开放功能、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A04：情境交互提示、结果反馈与教程重排

<details>
<summary>展开/复制：D22-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的交互反馈与教程 Agent。请直接完成 D22-A04。

任务编号：D22-A04（异步交互 UI 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：game/Assets/UI/D22/Interaction/、game/Assets/Prefabs/UI/D22/Interaction/、game/Assets/Scripts/Presentation/D22/Interaction/、测试/文档。禁止修改主 Canvas、玩法系统和主要页面。

开始前读取：D22 架构、D18 Input Prompts/UI 资源、D6/D7 新手引导、D9—D14 交互 result code、当前世界交互检测和用户截图中的右侧按钮列。

执行内容：
- 将“进入泊位、市场买盐/木材、上下船、发现岛、保存、离港”等固定按钮墙改为情境提示：只有目标在范围、状态允许且当前层级合适时显示；
- 提示包含动作名、当前绑定按键/手柄图标、禁用原因、长按/确认（适用时）和执行结果；
- 同时存在多个目标时有选择/优先级，不把十几个按钮垂直堆叠；
- 统一成功、失败、冷却、距离不足、资金不足、舱满、忙碌、未解锁和网络/Agent 降级反馈；
- 重排首次航程教程为逐步卡片/目标，只显示当前步骤，可跳过、重开并支持中英；
- Presenter 只读取 Interaction Candidate 和系统 Result，不直接执行数值修改；
- 建立距离/状态、多个目标、键鼠/手柄图标、成功失败、教程前进/跳过/读档测试。

硬约束：不改交互合法性、经济/库存/航行裁决，不新建快捷键绕过现有意图；不改主 Canvas/场景/主要面板/EventSystem；不硬编码内部 ID；不提交或推送 Git。

验证：Unity 编译/测试；右侧固定按钮墙不再需要，代表 10+ 交互均有上下文提示和真实反馈；教程无重叠且可恢复；git diff --check。

结果必须保存到：
- 主成果：上述 Interaction/Tutorial UI/Presenter/测试、docs/game/design/common/D22_CONTEXT_INTERACTION_TUTORIAL.md、game/tools/results/D22/D22-A04/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出替换的旧按钮、交互矩阵、教程步骤、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A05：EventSystem、点击、焦点、暂停与手柄导航修复

<details>
<summary>展开/复制：D22-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的 UI 输入与死按钮修复 Agent。请直接完成 D22-A05。

任务编号：D22-A05（异步输入基础设施任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：UI Input/Focus/EventSystem 适配代码、game/Assets/Prefabs/UI/D22/Input/、相关测试/文档。禁止重排主 Canvas 和修改玩法强逻辑。

开始前读取：D22 UI 清点/焦点规范、D16 输入/重绑定、当前 EventSystem/InputSystemUIInputModule/GraphicRaycaster/CanvasGroup、鼠标锁定、暂停、相机输入、所有可见按钮处理器和用户报告的无响应按钮。

执行内容：
- 确保运行时只有一个有效 EventSystem/UI Input Module，解决重复 EventSystem、透明 Raycast Target、Canvas 排序、CanvasGroup blocksRaycasts 和调试层抢点击；
- 建立明确 UI 状态机：Gameplay、Panel、Modal、Pause、Tutorial、Debug；打开 UI 时按规则锁定/释放鼠标并屏蔽游戏输入，关闭后恢复原焦点；
- 实现键鼠、手柄方向/确认/取消、首选控件、焦点记忆和无可选项回退；
- 审计每个可见按钮：handler 存在、事件触发一次、成功/失败有反馈；无功能按钮必须禁用并显示原因；
- 防止一次点击触发多次购买/保存/出航，保留系统幂等而不是只在 UI 隐藏；
- 处理暂停、场景重载、读档、设备切换、窗口失焦和弹窗嵌套；
- 建立自动点击/导航矩阵、重复触发、焦点恢复和输入隔离 PlayMode 测试。

硬约束：输入层只产意图，不写业务数值；不改 Systems/Command 裁决、存档/contracts；不重构主 Canvas/页面样式；不删除失败按钮而掩盖功能缺口；不提交或推送 Git。

验证：Unity 编译/测试；清点表所有可见按钮 100% 为 working 或 disabled-with-reason；鼠标/手柄/键盘路径可用；无重复购买/保存/离港；git diff --check。

结果必须保存到：
- 主成果：UI Input/Focus/EventSystem 适配/测试、docs/game/design/common/D22_UI_INPUT_FOCUS_BUTTON_AUDIT.md、game/tools/results/D22/D22-A05/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告必须给出按钮总数、working/disabled/blocked 数、根因、测试计数和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A06：统一视觉主题、图标、字体、本地化与可访问性

<details>
<summary>展开/复制：D22-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的 UI 视觉系统与可访问性 Agent。请直接完成 D22-A06。

任务编号：D22-A06（异步 UI Design System 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS。
单一写者：game/Assets/UI/D22/DesignSystem/、主题 ScriptableObject/样式组件、合法字体/图标正式副本、测试/文档。禁止修改主 Canvas 和业务页面布局。

开始前读取：D22 UI 架构、D18 合法 UI/Input/字体资源、D16 本地化/字幕/色盲/低刺激成果、当前字体和 Localization key、DESIGN_PILLARS 时代方向。

执行内容：
- 建立 1675 年航海风格但清晰现代可用的 Design System：颜色、排版、间距、按钮、输入框、标签、卡片、列表、Tooltip、弹窗、危险/成功/禁用状态；
- 使用合法 Kenney 或项目已有图标建立统一图标映射，键位图标从当前绑定动态选择；
- 接入许可证明确的中英字体与回退链，保留许可证；禁止 UI 文本散落硬编码；
- 达到关键文本/控件对比度和最小字号，颜色信息同时用图标/文字表达；
- 提供 UI Scale、字幕大小、色盲友好配色、低刺激/减少动态选项接口；
- 建立样式演示页和自动检查：缺字体、缺 Sprite、颜色/字号、Localization key、长文本、RTL 不作为本期目标但不能崩溃；
- 不在本任务把主题手工复制到所有页面，提供可复用组件供 S02 统一应用。

硬约束：只使用准入目录中的 `intake_eligible` CC0 资源或已审计 OFL 资源并保留许可证；不改业务逻辑、主 Canvas/页面结构、EventSystem/输入裁决；不把本地化字符串写入 Systems；正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；样式演示页中中英、4K/720p、键鼠/手柄图标、可访问性状态完整；无缺字/粉色 Sprite；git diff --check。

结果必须保存到：
- 主成果：上述 Design System/字体/图标/测试、docs/game/design/common/D22_UI_DESIGN_SYSTEM_ACCESSIBILITY.md、game/tools/results/D22/D22-A06/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出资源许可 ID、组件、可访问性检查、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-A07：UI 分辨率、点击与导航自动化验收矩阵

<details>
<summary>展开/复制：D22-A07 完整提示词</summary>

```text
你是 WhalesIslandReef 的 UI 自动化 QA Agent。请直接完成 D22-A07。

任务编号：D22-A07（异步测试任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01 PASS；应读取 A01—A06 的 Prefab/接口，若尚未齐备先搭测试框架并在最终复跑。
单一写者：game/Assets/Tests/D22/UI/、必要的只读测试夹具、game/tools/results/D22/D22-A07/、文档/交接。不得修改业务 UI 实现以让测试通过。

开始前读取：D22 架构/分辨率/按钮矩阵、A01—A06 成果、现有 Unity Test Framework、D16 输入/本地化测试和当前核心 PlayMode 测试。

执行内容：
- 建立可重复 UI 测试夹具，覆盖 1280×720、1920×1080、1920×1200、2560×1080、3840×2160 和至少一个安全区场景；
- 自动检测 RectTransform 越界、关键控件重叠、0 尺寸、不可见文字、缺字体/Sprite、多个 EventSystem、透明 Raycast 拦截和缺 handler；
- 覆盖玩家 HUD、Context Prompt、教程、市场、库存、船员、地图、船坞、金融/资产、设置、暂停、Modal；
- 对每页执行键鼠点击和手柄/键盘导航，检查确认/取消、焦点恢复、输入隔离、重复点击与 disabled reason；
- 覆盖中英长文本、UI Scale、色盲/低刺激、Debug 开关和场景重载；
- 生成机器 JSON、JUnit/XML（按现有约定）、截图矩阵和人工待检清单；自动视觉检测不能冒充人工审美验收。

硬约束：测试不能通过改业务数值、跳过失败或关闭控件；禁止用固定延时掩盖竞态，优先等待明确状态；不得修改主场景/业务 Prefab/Systems；不提交或推送 Git。

验证：测试自身含正/负夹具，能捕获重叠、死按钮和重复 EventSystem；运行聚焦测试并记录计数；测试结果可在干净本机重复；git diff --check。

结果必须保存到：
- 主成果：上述 UI Tests/夹具、docs/game/design/common/D22_UI_AUTOMATION_MATRIX.md、game/tools/results/D22/D22-A07/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出测试矩阵、通过/失败数、人工待检项和 PASS/PARTIAL/BLOCKED。
```
</details>

## D22-S02：正式 UI 串行集成与死按钮零容忍验收

<details>
<summary>展开/复制：D22-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D22 正式 UI 集成与验收 Agent。请直接完成 D22-S02。

任务编号：D22-S02（同步集成任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D22-S01、D22-A01—A07 全部有可用成果；D19-S02、D20-S02、D21-S02 建议 PASS。
并发规则：你是正式主 Canvas、InitialHarbor UI 根、EventSystem 和 UI 场景引用的唯一写者；集成期间暂停其他 UI 写入。

开始前读取：全部 D22 成果/交接/机器结果、D19—D21 验收、当前正式 Canvas/OnGUI/EventSystem、现有核心回归和用户原始截图。

执行内容：
1. 按架构串行接入 Debug、HUD、Context Prompt、主要 Panels、Design System、Input/Focus；确保正式场景只有一个 EventSystem；
2. 正常玩家模式默认隐藏开发叠层，移除旧按钮墙和重复面板的正式显示路径，但保留必要兼容代码直到测试证明可删除；
3. 将所有可见按钮接到真实 handler，或禁用并显示原因；逐项核对按钮矩阵；
4. 验证 HUD 模式切换、教程、市场/库存/船员/地图/船坞/资产/设置、暂停/Modal、鼠标锁定、手柄导航和返回焦点；
5. 运行 A07 全矩阵、D16 输入/本地化、D19—D21 以及核心 PlayMode 回归；
6. 输出所有目标分辨率的玩家模式截图，不得打开 Debug 来遮盖空白；执行人工 10 分钟 UI 路径检查；
7. 对没有真实实现的功能保持明确 Disabled，不得做点击后只改文字的假功能。

硬约束：UI 不写业务强逻辑，不改存档/contracts/server；不删除系统事件/诊断；只用合法资源；不把自动截图当最终审美人工批准；不提交或推送 Git。

通过门槛：
- 1920×1080 玩家画面无调试文字/教程/HUD/按钮墙堆叠；全部目标分辨率无关键重叠/裁切；
- 可见按钮 working 或 disabled-with-reason 比例 100%，无已知点击无反应；
- 鼠标、键盘、手柄导航和焦点恢复通过，Gameplay 与 UI 输入不同时触发；
- 中英、UI Scale、字幕/颜色/低刺激可用；
- Unity 无新增错误，A07 和核心回归通过，截图/机器结果齐全。

结果必须保存到：
- 主成果：正式 UI 集成 Prefab/场景引用/测试、docs/game/design/common/D22_UI_UX_ACCEPTANCE.md、game/tools/results/D22/D22-S02/；
- 交接报告：docs/game/design/common/handoffs/D22/D22-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED；报告必须给出按钮统计、分辨率矩阵、输入路径、测试计数、截图和未完成项。
```
</details>
