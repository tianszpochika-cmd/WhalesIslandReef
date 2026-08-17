# 第 14 章：D22 UI 命令、焦点与读模型逻辑（36 项）

> 本章不是 UI 美术/Canvas 重构，而是建立可测试的面板状态、命令验证、反馈、焦点和本地化读模型。任何真实按钮、Canvas、布局和输入设备表现均延期。

## 复制给 Agent 的单任务执行提示词

```text
你是 WhalesIslandReef 的纯逻辑开发 Agent。直接执行任务 [TASK_ID]。阅读 AGENTS.md、docs/CONTEXT.md、docs/DEVELOPMENT_STANDARDS.md、docs/game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md、docs/game/LOGIC_TASK_PROTOCOL.md 和本文件任务卡。

只改 UI 领域的纯 C# 命令、读模型、测试、文档或 contracts。禁止启动 Player/PlayMode、改 Canvas/EventSystem、真实输入、UI 视觉、场景、Prefab 或 Build Settings。UI 不得裁决经济、存档、航行、对话或任务，只能调用公开命令并展示结果。

完成时写 D22 结果 JSON、handoff、记忆日志，运行状态同步与 git diff --check，并如实报告 LOGIC_* 状态。
```

## 任务卡与执行顺序

| ID | 波次 | 写入范围（唯一） | 实施内容与逻辑验收 |
|---|---|---|---|
| D22-L01 | S | `docs/game/design/common/D22_LOGIC_BOUNDARY_BASELINE.md` | 冻结 UI 领域边界、现有面板来源、命令拥有者和 Runtime Canvas 隔离。 |
| D22-L02 | P | `game/Assets/Scripts/Systems/D22/D22ScreenContracts.cs` | 定义屏幕、面板、模态、焦点、层级和关闭原因的稳定 ID。 |
| D22-L03 | P | `game/Assets/Scripts/Systems/D22/D22FocusStateMachine.cs` | 实现逻辑焦点、单一模态拥有者、恢复栈和失焦回退。 |
| D22-L04 | P | `game/Assets/Scripts/Systems/D22/D22PanelExclusivityRules.cs` | 定义可并存/互斥面板与强制关闭的纯规则。 |
| D22-L05 | P | `game/Assets/Scripts/Systems/D22/D22UiCommandContracts.cs` | 定义 UI 请求、命令关联 ID、上下文和结果模型。 |
| D22-L06 | P | `game/Assets/Scripts/Systems/D22/D22UiCommandValidator.cs` | 校验请求权限、上下文、冷却和禁用原因，不写业务状态。 |
| D22-L07 | P | `game/Assets/Scripts/Systems/D22/D22FeedbackCatalog.cs` | 定义成功、失败、警告、确认、重试反馈码和本地化 key。 |
| D22-L08 | P | `game/Assets/Scripts/Systems/D22/D22DisabledReasonResolver.cs` | 将业务前置失败转换为稳定禁用/帮助读模型。 |
| D22-L09 | P | `game/Assets/Scripts/Systems/D22/D22PanelTransitionSystem.cs` | 实现打开、关闭、返回、替换和不可恢复错误的状态转换。 |
| D22-L10 | P | `game/Assets/Scripts/Systems/D22/D22PauseIntentSystem.cs` | 定义暂停意图、不可暂停上下文和恢复语义，不操作 Time.timeScale。 |
| D22-L11 | P | `game/Assets/Scripts/Systems/D22/D22EscapeIntentSystem.cs` | 定义 Escape/返回意图的优先级和无可关闭面板结果，不读键盘。 |
| D22-L12 | P | `game/Assets/Scripts/Systems/D22/D22PointerModeReadModel.cs` | 输出鼠标/光标需求的逻辑读模型，不控制设备。 |
| D22-L13 | P | `game/Assets/Scripts/Systems/D22/D22NotificationQueue.cs` | 实现通知去重、优先级、确认、过期和存档语义。 |
| D22-L14 | P | `game/Assets/Scripts/Systems/D22/D22AccessibilityReadModel.cs` | 建立缩放、对比、字幕、减少动态效果等设置的逻辑读模型。 |
| D22-L15 | P | `game/Assets/Scripts/Systems/D22/D22LocalizationKeyAudit.cs` | 校验 D22 反馈/标签不硬编码用户可见字符串。 |
| D22-L16 | P | `game/Assets/Scripts/Systems/D22/D22LoadStateReadModel.cs` | 定义加载、保存、迁移、失败与恢复的 UI 可读状态。 |
| D22-L17 | P | `game/Assets/Scripts/Systems/D22/D22MarketReadModelAdapter.cs` | 只读适配 D19/D3 市场状态、价格、禁用原因和结果。 |
| D22-L18 | P | `game/Assets/Scripts/Systems/D22/D22PortReadModelAdapter.cs` | 只读适配 D19 港口、泊位、服务与登船状态。 |
| D22-L19 | P | `game/Assets/Scripts/Systems/D22/D22ShipReadModelAdapter.cs` | 只读适配 D21 船况、航线、天气和补给状态。 |
| D22-L20 | P | `game/Assets/Scripts/Systems/D22/D22InventoryReadModelAdapter.cs` | 只读适配库存、容量、货币和失败反馈。 |
| D22-L21 | P | `game/Assets/Scripts/Systems/D22/D22NpcReadModelAdapter.cs` | 只读适配 D20 NPC 可用性、岗位和对话资格。 |
| D22-L22 | P | `game/Assets/Scripts/Systems/D22/D22QuestReadModelAdapter.cs` | 只读适配任务、提示、失败和奖励状态。 |
| D22-L23 | P | `game/Assets/Scripts/Systems/D22/D22SaveReadModelAdapter.cs` | 只读适配 SaveSystem 的槽位、完整性和迁移结果。 |
| D22-L24 | P | `game/Assets/Tests/EditMode/D22/D22FocusAndPanelTests.cs` | 覆盖互斥、嵌套、返回、错误恢复和重复打开。 |
| D22-L25 | P | `game/Assets/Tests/EditMode/D22/D22CommandFeedbackTests.cs` | 覆盖合法/非法请求、禁用原因、反馈映射和业务无副作用。 |
| D22-L26 | S | `game/Assets/Scripts/Systems/D22/D22UiCommandRouter.cs` | 以公开业务接口汇合 UI 命令、结果、反馈和关联 ID。 |
| D22-L27 | S | `game/Assets/Scripts/Systems/D22/D22ViewModelComposer.cs` | 将读模型组合为稳定 view model，处理缺模块和版本回退。 |
| D22-L28 | S | `game/Assets/Tests/EditMode/D22/D22DeterministicUiScenarioTests.cs` | 重放港口、市场、船舶、保存等面板逻辑流程，无 Canvas。 |
| D22-L29 | S | `game/Assets/Tests/EditMode/D22/D22UiStateFuzzTests.cs` | 以固定种子执行至少 1,000 次面板/命令序列，验证无焦点死锁。 |
| D22-L30 | S | `game/Assets/Tests/EditMode/D22/D22UiSaveLoadTests.cs` | 覆盖通知、暂停意图和可恢复面板状态的存档边界。 |
| D22-L31 | S | `tools/d22_logic_boundary_audit.py` | 阻止 Canvas、EventSystem、Input、Unity UI 组件和业务私写。 |
| D22-L32 | S | `docs/game/design/common/D22_LOGIC_UI_INTERFACE.md` | 发布 UI 命令、读模型、错误/本地化 key 与 Runtime 接线规则。 |
| D22-L33 | G | `tools/d22_logic_test_runner.py` | 汇总 D22 静态/单元/确定性测试并写机器结果。 |
| D22-L34 | G | `docs/game/design/common/D22_RUNTIME_DEFERRED_REGISTER.md` | 登记布局、实际按钮、键鼠/手柄、焦点抢占、分辨率和可访问性实测风险。 |
| D22-L35 | G | `docs/game/design/common/D22_LOGIC_STATUS_REVIEW.md` | 审计本章结果、handoff、状态同步和边界违例。 |
| D22-L36 | G | `docs/game/design/common/D22_LOGIC_ACCEPTANCE.md` | 章节逻辑验收，确认可供 D23 调用的 UI 命令/读模型已稳定。 |

## 依赖

L02—L25 在 L01 后并行；L26—L32 串行整合；L33—L36 汇合。依赖 D19/D20/D21 的 read-model adapter 必须等待其对应 `LOGIC_ACCEPTANCE`，否则保持 BLOCKED。
