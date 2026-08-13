# D16-A03 本地化与可访问性 Agent 交接报告

## 1. 任务信息

- 任务编号：D16-A03
- 执行日期：2026-08-11
- 前置任务：D15-S02
- 当前状态：IN_PROGRESS
- 是否完成实现：否
- 是否允许进入 D16-A03 正式验收：是，完整矩阵仍未通过

## 2. 修改文件

- docs/game/design/common/D16_LOCALIZATION_ACCESSIBILITY.md：阻断报告。
- docs/game/design/common/handoffs/D16/D16-A03_HANDOFF.md：本交接报告。
- .workbuddy/memory/2026-08-11.md：执行记忆日志。

未修改 Localization、Input、UI、Settings、字体、字幕、Systems、AgentCore、contracts、server 或 web。

## 3. 前置条件结论

D15-S02 的早期重新运行曾返回 D15_S02_QA_EXIT=1；该段为历史阻断记录。当前顺序复验为 PASS（0 errors、0 warnings），A01—A07 locale 证明已纳入统一 QA。

基线阶段因此没有实现中英长文本、数字格式、输入重绑定、字幕、色盲、字体回退、低刺激或敏感内容 UI 的正式功能和测试；后续增量记录在本报告末尾，早期描述仅作为历史审计。

## 4. 当前能力与缺口

- Localization.cs 提供少量中英静态 key 表和缺 key 返回 key 的排查策略。
- SettingsStore.cs 仅保存音量、画质和 zh/en 语言。
- HarborPauseController.cs 仅对 InitialHarbor 提供 Escape 暂停和 UI 输入模块。
- D2—D10 多个 Gameplay Bootstrap 直接读取 Keyboard.current，尚未统一为可重绑定输入意图。
- D15 音频目录登记 subtitle_key，但当前表现层没有完整字幕渲染、字体回退和保存恢复验收。
- D10/D14 已有审查状态、警告和跳过逻辑入口；本任务没有替代或复制这些强逻辑裁决。

## 5. 待执行验收

门禁恢复后执行：中英长文本、数字/债务/重量/时间格式、键鼠/手柄、输入冲突与重绑定持久化、字幕、色盲与非颜色提示、字体回退、全场景暂停、低刺激设置、敏感内容警告/跳过，以及关键价格和警告不截断。

每项都要记录测试入口、语言、窗口/缩放、输入设备和是否写入设置；敏感内容必须读取审查数据，跳过只保留事实，不产生奖励、关系或经济结果。

## 6. 重新开工注意事项

先关闭 D15-S02 本地化证明和其他 QA 阻断，再开始 D16-A03。新增文本必须使用本地化 key；Input 层只能发出意图；UI 只能读取系统快照和审查状态。不得把硬编码文案、颜色差异、隐藏警告或 Agent 输出作为可访问性实现。

## 2026-08-11 状态更新

D15-S02 已通过；A01—A07 的 `D15_A01_A07` zh-CN/en-US 包已被统一 QA 验证，非 A08 key 缺失证明数为 0。D16-A03 当前为 **PENDING**，运行时长文本布局、手柄、重绑定、字幕、色盲、字体回退、低刺激和全场景暂停仍待实现/验收。

## 2026-08-11 实现进度补充

当前状态：**IN_PROGRESS，完整运行时矩阵待验收**。

新增 `D16AccessibilityContracts`、`D16AccessibilitySettingsStore`、`D16InputRouter`、`D16AccessibilityRuntime`、`D16AccessibilityTests` 和 `tools/d16_a03_runner`。D2 人物、船舶、交互、视角和暂停入口已使用统一动作意图；设置层只保存表现偏好和输入绑定，不越权写玩法/存档。5 项纯逻辑测试、Systems/Save.Tests/Assembly-CSharp 三套构建均通过，0 warning / 0 error。

未完成：真实手柄、全场景重绑定 UI、字幕渲染触发、字体 fallback、色盲场景抽样、低刺激效果和 Unity PlayMode。不能将 A03 标记为 PASS。

## 2026-08-11 Unity 6 实测更新

当前状态：**IN_PROGRESS（运行时基础接线已通过，完整无障碍矩阵仍待验收）**。

- Unity 6 PlayMode：`InitialHarborBootstrapsD16AndSamplesRuntime` 1/1 PASS；日志确认 D16-A03 UI 设置已应用（zh-CN、文本缩放、字幕开关）。
- Unity 6 EditMode 全量回归：291/291 PASS；D16-A03 纯逻辑测试 5/5 PASS；相关程序集构建 0 warning / 0 error。
- 同一轮修复了 D9 状态 UI 对 Unity 6 已废弃 `Arial.ttf` 的调用，改用 `LegacyRuntime.ttf`；PlayMode 不再因该异常日志失败。

尚未关闭：真实手柄/键鼠重绑定 UI、字幕触发和长文本布局、字体 fallback、多场景暂停焦点、色觉/非颜色提示、低刺激效果与真机回归。证据总览见 `docs/game/design/common/D16_RUNTIME_ACCEPTANCE_REPORT.md`。

## 2026-08-11 可操作 UI 与 Unity 6 回归更新

当前状态：**IN_PROGRESS（可操作基础链通过，完整无障碍门禁未关闭）**。

本轮完成：

- 新增 `D16AccessibilityPanel`，从主菜单设置和 InitialHarbor 暂停菜单均可打开；独立模态层、单 EventSystem/输入模块、玩法输入锁定与恢复已处理。
- 键盘/手柄重绑定捕获、重复绑定拒绝、默认值重置、即时 PlayerPrefs 保存已接入 `D16AccessibilitySettings` 校验契约。
- `D16AccessibilityRuntime` 已把文字缩放、字体回退、高对比度、色觉辅助、低刺激和减弱动态应用到 UGUI；`D16SubtitleOverlay` 已提供字幕显示、说话人标签、敏感内容开关和场景生命周期承载。

证据：

- `tools/d16_a03_runner`：6/6 PASS；相关 standalone 构建 0 warning / 0 error。
- Unity 6 `D16.PlayMode.Tests`：3/3 PASS，包含“主菜单→设置→无障碍与键位→关闭”的点击链；`tools/results/d16_a03_playmode_test_results.xml`。
- Unity 6 EditMode：292/292 PASS；`tools/results/d16_a03_editmode_unity6.log`。

交接时必须保留的未完成项：真实物理手柄与目标设备操作、所有音频 cue 到字幕的触发链、字体覆盖与长文本截图、色觉/低刺激场景可读性、多场景暂停焦点和长时间运行回归。字幕 overlay 目前是承载层，不应误写成“字幕已覆盖全部音频事件”。

## 2026-08-11 D9 字幕事件接线补充

`D9AudioPresentationController.PlayCue` 已在音频成功播放后调用 `D16SubtitleOverlay.PublishKey("subtitle_" + cue, null, 3f)`；`Localization` 新增 14 个 cue 的 zh/en 文案。该接线不改变 D9 事件事实和存档，仅把已有表现 cue 投递给字幕层。

Unity 6 图形设备回归 `D16.PlayMode.Tests` **4/4 PASS**，A03 纯逻辑回归 **6/6 PASS**，Assembly-CSharp 构建 0 warning / 0 error。注意：本轮测试没有逐条制造 D9 领域事件并截图验证字幕，因此“代码已接线”不等于“14 个事件场景全部验收”；真实事件抽样、物理手柄、字体/长文本、色觉/低刺激和多场景焦点仍待完成。

字幕接线后的最终 Unity EditMode 全量回归为 **292/292 PASS**，日志 `tools/results/d16_final_editmode_unity6.log`；没有发现新增编译或既有逻辑回归。

新增真实事件验收：`event_d9_crew_report` 通过共享 EventLog 进入 D9 音频表现，实际得到 `crew_report` cue，并让字幕显示“船员报告”；报告 `tools/results/d16_a03_d9_subtitle_runtime_report.json`，Unity PlayMode 总回归 **5/5 PASS**。A03 仍为 IN_PROGRESS，剩余为物理手柄/目标设备、14 cue 全量抽样、字体/长文本、色觉/低刺激和多场景焦点。

## 2026-08-12 输入意图与模态阻断实测补充

本轮只处理 D16-A03 的 Input/UI 测试子模块，没有改动 Systems 的数值裁决、存档、网络或跨端契约。

- 新增 `game/Assets/Tests/PlayMode/D16InputRouterPlayModeTests.cs` 及对应 `.meta`；通过 Unity Input System Test Framework 创建合成 `Gamepad` 与 `Keyboard`，实测 West→`Interact`、W→`MoveForward`/移动轴，并验证模态层同时阻断两类设备输入。
- `game/Assets/Tests/PlayMode/D16.PlayMode.Tests.asmdef` 新增 `Unity.InputSystem`、`Unity.InputSystem.TestFramework` 引用；测试通过反射访问表现层类型，保持现有程序集依赖方向，不把测试依赖倒灌进生产模块。
- focused Unity 6 PlayMode：**1/1 PASS**，证据 `tools/results/d16_a03_input_router_single_unity6.xml`。
- D16 Unity 6 PlayMode 全量：**12/12 PASS**，证据 `tools/results/d16_a03_input_router_full_playmode_unity6.xml`，日志 `tools/results/d16_a03_input_router_full_playmode_unity6.log`。

该证据关闭了“编辑器内合成键盘/手柄事件未到达统一意图层”的子门禁，但不替代真实物理手柄、目标设备、字体/长文本、色觉/低刺激场景、多场景暂停焦点及长时间运行回归。D16-A03 总体继续 **IN_PROGRESS**。

## 2026-08-12 InitialHarbor 暂停与 Esc 集成补充

本轮继续限定在 D16-A03 的 UI/Input 测试模块：

- `D16A03PauseMenuKeepsFocusAcrossAccessibilityAndDiagnostics` 通过真实 InitialHarbor 暂停控制器和按钮 `onClick` 链，验证暂停后选中“继续游戏”、无障碍/本地诊断模态的输入阻断、关闭后的暂停状态保留，以及继续按钮恢复时间流速。
- `D16A03SyntheticEscapeTogglesHarborPauseThroughController` 使用 Unity Input System 合成键盘 Escape，让 Unity 在下一帧处理事件，实际触发 `HarborPauseController.Update` 的暂停与恢复；日志包含“游戏已暂停”和“游戏已恢复”。
- focused Esc 集成回归：**1/1 PASS**，证据 `tools/results/d16_a03_escape_pause_single_unity6.xml`。
- D16 Unity 6 PlayMode 全量：**14/14 PASS**，证据 `tools/results/d16_a03_escape_pause_full_d16_playmode_unity6.xml`，日志 `tools/results/d16_a03_escape_pause_full_d16_playmode_unity6.log`。

该补充关闭编辑器内 Esc→暂停控制器和暂停菜单多模态焦点子门禁，不改变 D16-A03 总体结论：真实物理输入设备/目标设备、字体与长文本、色觉/低刺激、多场景以外的更广回归和长线稳定性仍需独立证据。未修改 Systems、存档、网络或跨端契约。

## 2026-08-11 D9 14 cue 全量运行时抽样

- 新增 `D16A03AllD9EventsPublishAudioCueAndSubtitle`：逐条向共享 EventLog 追加 14 个冻结 D9 事件 ID，实际验证音频 cue、字幕可见性和本地化文本。
- 最新 Unity 6 图形 PlayMode `D16.PlayMode.Tests` 为 **6/6 PASS**；A01 60 秒性能窗口、无障碍 UI 点击链、D9 14 cue、代表链和 InitialHarbor 烟测均通过。
- `tools/results/d16_a03_d9_cue_coverage_report.json`：ExpectedCueCount `14`、PassedCueCount `14`；14 条均有实际 cue 和可见字幕，例如 `departure_prepared→出航准备完成`、`crew_report→船员报告`、`save_checkpoint→存档检查点已完成`。
- 可复核 XML/日志：`tools/results/d16_a03_full_cue_playmode_unity6_retry2.xml`、`tools/results/d16_a03_full_cue_playmode_unity6_retry2.log`。

这关闭了 A03 的“14 个 D9 cue 逐条 EventLog→音频→字幕”子门禁，但 A03 总体仍为 IN_PROGRESS：真实物理手柄/目标设备、字体覆盖与长文本、色觉/低刺激场景、多场景暂停焦点和长线稳定性仍需独立证据。

## 2026-08-12 设置页按钮命中区域修复与复验

用户反馈设置页多个按钮可见但点击无反应。代码审查确认 BtnDiagnostics、BtnClose 原来位于 1080 参考视口下方，导致其视觉层与真实 UI 命中区域不一致。本轮只修改 MainMenuController 的设置页布局，将标题、设置项、诊断和关闭按钮收回视口并避免重叠。

新增 D16A03SettingsButtonsHaveUsableHitTargetsAndOpenRealModals：

- 读取三个按钮并验证 interactable、视口边界和互不重叠；
- 通过真实按钮 onClick 打开/关闭无障碍模态；
- 通过真实按钮 onClick 打开/关闭诊断模态；
- 点击设置关闭按钮后验证设置面板隐藏、主菜单恢复显示。

验证结果：

- Assembly-CSharp：0 warning / 0 error；
- Unity 6 D16.PlayMode.Tests：**15/15 PASS**；
- 新增设置按钮测试：**1/1 PASS**；
- 证据：game/tools/results/d16_a03_settings_layout_full_playmode_unity6_final.xml 与同名 .log。

该子门禁已关闭，但 A03 仍是 **IN_PROGRESS**。真实物理手柄/目标硬件、字体与长文本、色觉/低刺激以及长线稳定性仍不可由本次编辑器测试替代。

## 2026-08-12 主菜单面板互斥回归

新增 D16A03MainMenuPanelVisibilityIsExclusiveAcrossNavigation，使用真实按钮回调依次打开设置、新游戏、关于并返回，验证 MainMenu、NewGamePanel、SettingsPanel、AboutPanel 四者任一时刻只有一个可见，避免隐藏面板残留造成叠层和点击拦截。

最新 Unity 6 D16.PlayMode.Tests 为 **16/16 PASS**；D16A03SettingsButtonsHaveUsableHitTargetsAndOpenRealModals 与 D16A03MainMenuPanelVisibilityIsExclusiveAcrossNavigation 均为 **1/1 PASS**。证据：game/tools/results/d16_a03_menu_regression_full_playmode_unity6.xml；日志：tools/results/d16_a03_menu_regression_full_playmode_unity6.log。

该回归关闭编辑器内主菜单叠层子门禁，但 A03 仍为 IN_PROGRESS，真实手柄/目标硬件、字体/长文本、色觉/低刺激和长线稳定性仍待完成。

## 2026-08-12 静态 UI 本地化表校验

本轮新增 `Localization.ValidateTables()`，只读检查 UI 表的 zh/en key 集合对称、两侧文案非空；没有把 D15 内容 locale 包或人工长文本审查误写成已完成。

- 修改：`game/Assets/Scripts/Gameplay/UI/Localization.cs`；
- 测试：`D16A03StaticUiLocalizationTablesAreSymmetricAndNonEmpty`；
- Unity 6 focused PlayMode：**1/1 PASS**，`game/tools/results/d16_a03_localization_tables_playmode_unity6.xml`；
- Unity 6 D16 PlayMode 全量：**17/17 PASS**，`game/tools/results/d16_a03_localization_tables_full_d16_playmode_unity6.xml`；
- Assembly-CSharp、Save.Tests：均 0 warning / 0 error。

保留的 A03 未完成项：真实物理手柄/目标设备、字体覆盖与中英长文本视觉验收、色觉/低刺激场景、多场景焦点和长线稳定性。D16-S01/S02 仍不得启动。

## 2026-08-12 长文本、字体与视觉辅助运行时回归

新增 `D16A03AccessibilityRuntimeAppliesLongTextFontScaleAndColorModes`，在真实 MainMenu 场景的 UGUI `Text` 上创建窄幅长文本夹具，通过实际 `D16AccessibilityRuntime.ApplyToSceneUi()` 验证：

- `Resources/Fonts/ph_font_noto_sans_cjk_sc` 中文字体资源可加载并被 Text 使用；
- 长文本配置为 Wrap，并保留最后一行的 Overflow 策略；
- 2 倍文字缩放实际应用到字号；
- 高对比度实际应用为白色；
- Deuteranopia 色觉辅助实际改变 Graphic 颜色。

结果：Unity 6 图形 PlayMode `D16.PlayMode.Tests` **18/18 PASS**；新测试为 1/1 PASS。证据：`tools/results/d16_a03_accessibility_playmode_unity6.xml`、`tools/results/d16_a03_accessibility_playmode_unity6.log`；运行时报告：`tools/results/d16_a03_accessibility_runtime_report.json`，字段全部为 true。

本轮只关闭编辑器运行时的字体/长文本/视觉辅助代码子门禁，不把它误写成真机或人工视觉验收。A03 仍为 **IN_PROGRESS**：真实物理手柄/目标设备、目标分辨率下中英长文本截图、色觉/低刺激人工场景、多场景焦点和长线稳定性仍待证据；D16-S01/S02 仍不得启动。

## 2026-08-12 敏感内容警告、继续与跳过运行时回归

本轮补齐 D16-A03 要求的敏感内容表现层闭环，只修改 UI/Input 表现模块，不修改 Systems、存档、网络、契约或领域数值裁决：

- 新增 `game/Assets/Scripts/Gameplay/UI/D16/D16SensitiveContentOverlay.cs`，通过 `D16SensitiveContentPolicy` 和 `SensitiveWarnings` 设置决定是否显示警告；显示时真实创建模态面板、阻断游戏输入，并提供“继续查看”和“跳过内容”两个按钮；Esc/Start 映射为跳过；关闭后恢复此前输入阻断状态。
- 新增 `PresentWarning(...)` 与 `PublishSensitiveSubtitle(...)` 表现层入口；继续路径才发布字幕，跳过路径清除字幕并执行跳过回调，不写入任何领域状态。
- `Localization.cs` 补齐 zh-CN/en-US 的标题、正文、继续和跳过四组对称 UI 文案。
- 新增 `D16SensitiveContentOverlayTests.cs`，在真实 MainMenu 场景中通过实际 Button `onClick` 验证警告显示、输入阻断、继续、跳过以及关闭警告偏好后的即时跳过路径。

验证结果：

- Unity 6 D16.PlayMode.Tests 全量：**20/20 PASS**；新敏感内容测试通过；
- 证据：`tools/results/d16_a03_sensitive_warning_playmode_unity6.xml`、`tools/results/d16_a03_sensitive_warning_playmode_unity6.log`；
- 先前长文本/字体/视觉辅助测试仍由 `tools/results/d16_a03_accessibility_playmode_unity6.xml` 保留，不以敏感内容测试替代。

该结果关闭编辑器运行时的“敏感内容警告/继续/跳过”代码子门禁，但 A03 总体仍为 **IN_PROGRESS**：真实物理手柄/目标设备、目标分辨率下中英长文本截图、色觉/低刺激人工场景、多场景焦点和长线稳定性仍缺独立证据；D16-S01/S02 仍不得启动。

## 2026-08-12 低刺激选项运行时回归

基线文档曾把低刺激列为 NOT FOUND，但当前代码已有设置存储、无障碍面板开关和 `D16AccessibilityRuntime` 的颜色消费路径。本轮只补 A03 测试门禁：扩展 `D16A03AccessibilityRuntimeAppliesLongTextFontScaleAndColorModes`，在真实 MainMenu UGUI 长文本夹具上保存 LowStimulus 设置并调用实际运行时适配器，断言图形颜色相对基础颜色发生低刺激映射。

- `D16AccessibilitySettingsStore.LowStimulus` 的保存/读取和面板开关不写存档或领域状态；
- `D16AccessibilityRuntime.ApplyVisualColor` 负责表现层映射；
- Unity 6 D16.PlayMode.Tests：**21/21 PASS**；
- 证据：`tools/results/d16_a03_low_stimulus_playmode_unity6.xml`、`.log`；`tools/results/d16_a03_accessibility_runtime_report.json` 的 `LowStimulusApplied=true`。

本轮关闭编辑器运行时低刺激代码子门禁，但不替代目标设备亮度/动画/对比度人工复核。A03 仍为 **IN_PROGRESS**：真实物理设备、目标分辨率截图、色觉/低刺激人工场景、多场景焦点和长线稳定性仍待证据；D16-S01/S02 不启动。

## 2026-08-12 数字格式运行时 UI 回归

新增 `D16A03RuntimeNumberFormattingFeedsUiText`，在真实 MainMenu 场景把 `D16DisplayFormatter` 的中英数字、负数和百分比结果写入 UGUI `Text` 后断言显示值，覆盖 `1,234.50`、`-12` 和 `25%`。

- Unity 6 D16.PlayMode.Tests 全量：**21/21 PASS**；
- 证据：`tools/results/d16_a03_number_format_playmode_unity6.xml`、`tools/results/d16_a03_number_format_playmode_unity6.log`；
- 这是表现层格式化与 UI 消费门禁，不等价于债务、重量、时间和价格等领域字段全部接入。A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 领域数字字段接入回归

### 修改文件

- `game/Assets/Scripts/Gameplay/D9/D9PresentationReadModel.cs`：增加 locale 输入，统一格式化 D9 航程日数、金币、重量/容量、吃水、货物、补给、损伤、船员、改装和市场库存。
- `game/Assets/Scripts/Gameplay/D9/D9VoyageHudController.cs`：把当前 UI 语言传入 D9 读模型。
- `game/Assets/Scripts/Gameplay/D3/D3RuntimeBootstrap.cs`：统一格式化航程、贸易库存、买卖价格、金币和金币变化。
- `game/Assets/Scripts/Gameplay/D9/D9PortShipPreparationBootstrap.cs`：统一格式化港口整备页数值。
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：新增真实主程序集 D9 读模型消费测试；使用反射是为了保持现有 PlayMode 测试程序集不反向引用 `Assembly-CSharp`。

### 数据所有权与边界

本轮只修改 Gameplay 表现层的字符串投影和测试，不修改 `Systems` 的价格/重量/航程/债务裁决，不修改存档字段、contracts、AgentCore 或网络代码。`D16DisplayFormatter` 只格式化输入值，不拥有或变更领域数值。

### 验证结果

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- Unity 6 `D16.PlayMode.Tests`：**22/22 PASS**；
- 原始证据：`tools/results/d16_a03_domain_number_full_playmode_unity6_retry3.xml`、`tools/results/d16_a03_domain_number_full_playmode_unity6_retry3.log`；新增测试为 `D16A03D9PresentationReadModelFormatsDomainNumbersBeforeUiConsumption`。

### 验收结论与未完成事项

领域数字字段的代码级表现接入已通过；A03 总体仍 **IN_PROGRESS**。债务专用页面、目标设备/真机输入、目标分辨率与人工可读性、字体覆盖、色觉/低刺激人工复核、多场景焦点和长线稳定性仍未取得证据。D16-S01/S02 不启动。

## 2026-08-12 金融与债务只读页面子门禁

- 任务编号：D16-A03（金融/债务表现层子门禁）。
- 状态：**PASS（子门禁）/ IN_PROGRESS（D16-A03 总体）**。
- 修改文件：`game/Assets/Scripts/Gameplay/UI/D16/D16FinanceDebtPanel.cs` 及 `.meta`、`game/Assets/Scripts/Gameplay/UI/HarborPauseController.cs`、`game/Assets/Scripts/Gameplay/UI/Localization.cs`、`game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`、`game/Assembly-CSharp.csproj`。
- 新入口：InitialHarbor 暂停菜单的 `Finance` 按钮；页面提供 `ShowPanel`、`HidePanel` 和 `BuildDebtSummary(D13FinanceRuntimeSnapshot)` 只读入口。
- 数据所有权：页面只读取 `GameState.CurrentD13FinanceRuntime`；FinanceSystem 继续拥有账户、债务、利息、滞纳金和风险裁决，Save 域只负责快照持久化；页面不提交还款、展期、催收、查封或破产请求。
- 可访问性边界：金融内容使用滚动文本容器、统一中英文文案和 `D16DisplayFormatter` 数字格式；打开时锁定玩法输入，关闭时恢复之前的输入阻断状态。
- 真实测试：专项 Unity 6 PlayMode **1/1 PASS**，`tools/results/d16_a03_finance_debt_single_unity6.xml`；新增页面后的 D16 PlayMode 全量 **24/24 PASS**，`tools/results/d16_a03_finance_debt_full_d16_playmode_unity6.xml`；`Assembly-CSharp`、`Assembly-CSharp-Editor`、`Save.Tests` 均 0 warning / 0 error。
- 验收结论：D13 只读快照中的本金、利息、滞纳金、当前应付、到期日、风险说明和最坏结果已真实进入 UI，且页面开关链通过；不扩大为 D13 金融玩法全链路或 D16-A03 总体通过。
- 未完成事项：真实物理手柄/目标设备、目标分辨率长文本截图、色觉/低刺激人工复核、多场景焦点和长线稳定性仍缺证据；A03 继续 IN_PROGRESS。
- 下游任务：完成 A01—A06 的外部证据和最终资产审查后，才能进入 D16-S01/S02；不得因本子门禁提前启动 D17。

## 2026-08-12 键位重绑定持久化与意图路由补充

本轮继续限定在 D16-A03 的 Input/UI 测试子模块，没有修改 Systems、存档、网络、contracts 或领域数值裁决。

- 新增 `D16A03KeyboardRebindPersistsAndRoutesTheChangedIntent`：通过真实 `D16AccessibilitySettingsStore` 保存 `Interact → Q`，重新读取设置，再用 Unity Input System 合成键盘 Q，验证 `WasPressedThisFrame` 与 `IsPressed` 都进入同一 `Interact` 意图。
- 首次运行暴露的是测试反射代码取错 `GetBinding` 所在类型，已修正；该次失败不属于生产运行时失败，不作为通过证据。
- 修正后 focused Unity 6 PlayMode：**1/1 PASS**，证据 `tools/results/d16_a03_keyboard_rebind_single_unity6.xml`；D16 PlayMode 全量：**26/26 PASS**，证据 `tools/results/d16_a03_keyboard_rebind_full_d16_playmode_unity6.xml`，日志同名 `.log`。
- Unity 刷新后先恢复临时依赖资产；随后串行 `Assembly-CSharp` 与 `Save.Tests` 构建均为 **0 warning / 0 error**。并行构建曾因共享中间 DLL 被占用而失败，未作为最终证据。

该子门禁关闭了“编辑器内键位保存后仍能路由到统一意图”的代码回归风险，但不替代真实物理手柄、目标设备、目标分辨率人工视觉、色觉/低刺激人工复核和长线稳定性证据。D16-A03 继续 **IN_PROGRESS**；D16-S01/S02 仍不得启动。

## 2026-08-12 手柄触发器重绑定与边沿路由补充

本轮继续限定在 D16-A03 的 Input/UI 测试子模块，没有修改 Systems、存档、网络、contracts 或领域数值裁决。

- `D16InputRouter` 现在支持 `LeftTrigger` / `RightTrigger` 作为可重绑定手柄输入；触发器是模拟轴，不直接当作 `ButtonControl` 读取，运行时按设备和 Unity 帧采样边沿，避免持续按住时每帧重复产生一次性意图。
- 新增 `D16A03GamepadRebindPersistsAndRoutesTheChangedIntent`：通过真实设置存储保存 `Quality → LeftTrigger`，重新读取绑定，用 Unity Input System 合成左扳机按下，验证按下帧 `WasPressedThisFrame`、持续帧 `IsPressed` 以及下一帧不重复触发。
- 首次测试夹具曾尝试把 `Quality` 绑定到已被 `Trade` 占用的 `North`，被现有重复绑定校验正确拒绝；随后改用未占用的 `LeftTrigger` 完成测试。该次失败不是生产运行时失败，不作为通过证据。
- focused Unity 6 PlayMode：**1/1 PASS**，证据 `tools/results/d16_a03_gamepad_rebind_single_unity6.xml`；D16 PlayMode 全量：**27/27 PASS**，证据 `tools/results/d16_a03_gamepad_rebind_full_d16_playmode_unity6.xml`，日志同名 `.log`。
- `Assembly-CSharp`、`Save.Tests`、`AgentCore` 串行构建均为 **0 warning / 0 error**；Unity 测试刷新导致的临时依赖恢复已完成，未把并行中间文件占用失败作为最终证据。

该子门禁关闭了“编辑器内手柄扳机重绑定后无法持久化/重复触发”的代码回归风险，但不替代真实物理手柄、目标设备、目标分辨率人工视觉、色觉/低刺激人工复核和长线稳定性证据。D16-A03 继续 **IN_PROGRESS**；D16-S01/S02 仍不得启动。

## 2026-08-12 核心键位面板全量化与滚动视口

本轮仍限定在 D16-A03 的 Gameplay/UI 输入设置模块，修复“契约已有绑定但面板没有入口，以及新增入口会挤出可点击区域”的结构性缺口。

### 修改内容

- `game/Assets/Scripts/Gameplay/UI/D16/D16AccessibilityPanel.cs`：将面板的 `RebindableActions` 从 9 个扩展为 `D16InputAction` 契约已有的 16 个核心动作，包含地图、贸易、角色预设、语言、画质、成就和保存状态；未新增私有动作，也未改变输入路由或领域写权限。
- 同文件：为 16 行绑定控件创建固定高度的 `ScrollRect`/`Viewport`/`Content`，底部状态提示、恢复默认和关闭按钮保持在 960 高参考卡片内，避免继续出现视觉存在但点击区域越界/被遮挡。
- `game/Assets/Scripts/Gameplay/UI/Localization.cs`：补齐上述 7 个动作的 zh-CN/en-US 标签，并保留静态表对称校验。
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：新增 `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`，检查滚动内容大于视口、16 个键盘/手柄按钮均存在且可交互，并实际调用新增 Map 重绑定按钮确认进入监听状态。

### 验证

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a03_runner/D16A03Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a03_runner/bin/Debug/net9.0/D16A03Runner.dll`：6/6 PASS；
- Unity 6 focused PlayMode 已启动到图形初始化，但本机许可证服务停在 `Licensing::Module`，未生成 `tools/results/d16_a03_binding_scroll_single_unity6.xml`；该结果不计为通过或失败，待许可证服务可用后必须重跑同名专项。

### 边界与待办

本轮没有修改 Systems、存档、contracts、AgentCore、网络或 D3-D10 领域数值；D16-A03 仍为 **IN_PROGRESS**。待 Unity 运行环境恢复后，需补充实际滚轮滚动、键盘导航、物理手柄导航和目标分辨率人工复核；D16-S01/S02 仍不得启动。

## 2026-08-12 模态输入阻断状态恢复修复

本轮仍限定在 D16-A03 的 Gameplay/UI 输入设置模块，修复嵌套模态关闭后输入状态被错误解锁的问题。

### 修改内容

- `D16AccessibilityPanel` 打开时记录当前 `D16InputRouter.GameplayInputBlocked` 状态，关闭或销毁时恢复该状态，而不是无条件写回 `false`；从暂停、诊断或其他模态进入设置页时，内层关闭不会破坏外层输入阻断。
- 面板已打开时重复调用 `ShowPanel` 直接忽略，避免重复捕获“打开前状态”后关闭时恢复成错误值。
- 新增 `D16A03AccessibilityPanelRestoresOuterInputBlockState`，用真实 MainMenu 场景和反射入口验证外层已阻断时打开/关闭无障碍面板仍保持阻断，测试结束再清理状态。

### 验证与边界

- 待执行串行构建：`Assembly-CSharp`、`D16.PlayMode.Tests`；待 Unity 许可证服务恢复后重跑该专项与 D16 全量 PlayMode。
- 本轮不修改 Systems、存档、contracts、AgentCore、网络或领域数值；D16-A03 总体仍 **IN_PROGRESS**。
- 当前本机 Unity batchmode 仍停在 `Licensing::Module`，未生成 XML 的启动结果不计为 PASS；D16-S01/S02 继续不得启动。

## 2026-08-12 D16 模态锁接入 D7 旧输入权限

本轮补齐 D16 模态与旧版 D3-D10 快捷键之间的权限桥接，修改范围为 Gameplay 输入协调，不改变任何领域裁决。

- `D7ControlCoordinator.GameplayInputAllowed` 和 `CameraInputAllowed` 现在同时检查 `D16InputRouter.GameplayInputBlocked`；无障碍、诊断、敏感内容和金融模态打开时，旧模块不再继续接收玩法/镜头快捷键。
- D7 的 ESC 模态处理在 D16 输入锁期间提前让出，避免 ESC 同帧先穿透关闭底层贸易/对话，再由顶层面板关闭。
- `D16A03AccessibilityPanelRestoresOuterInputBlockState` 增加 D7 权限断言，验证外层阻断保持期间旧输入权限持续为 false。

验证：`Assembly-CSharp`、`D16.PlayMode.Tests`、A03 standalone runner 均 0 warning / 0 error；standalone runner 6/6 PASS。Unity 专项仍因本机 `Licensing::Module` 未完成而未生成 XML，不计为运行时 PASS。A03 总体继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 上下文输入契约扩展

本轮只修改 D16-A03 的 Systems/D16 设置契约和对应纯逻辑测试，为后续逐模块迁移旧快捷键准备稳定动作集合。

- `D16InputAction` 新增 D3、D4、D5、D9、D10 的上下文动作，覆盖贸易关闭/航程、炮战、岛屿、港口整备和酒馆社会页；没有复制领域数据或改变任何系统裁决。
- `D16InputBinding` 新增 `Scope`。重复键只在同一作用域内拒绝，不同页面可以合法复用 B/E/P 等物理键，避免把上下文键误判为全局冲突。
- 默认键盘/手柄绑定保留现有操作，并登记所有新增动作；`Save` 增加旧快捷键 C 作为兼容副键，F5 仍为主键。
- 新增 `ContextBindingsMayReuseKeysButSameScopeCannot`，验证跨作用域复用允许、同作用域重复仍拒绝；A03 standalone runner 从 6/6 更新为 **7/7 PASS**。

### 边界与下游

- 本轮未修改 D3/D4/D5/D9/D10 Gameplay Bootstrap，也未声称旧快捷键已经完成迁移；下一步必须按单模块顺序逐个把这些入口改为 `D16InputRouter` 意图读取，并同步面板标签/滚动视口和真实 PlayMode 证据。
- `Assembly-CSharp`、`Save.Tests`、`D16.PlayMode.Tests`、A03 runner 均 0 warning / 0 error；Unity 运行专项仍因 `Licensing::Module` 未初始化而无法产生新的 XML。
- D16-A03 总体仍 **IN_PROGRESS**，D16-S01/S02 继续不得启动。

## 2026-08-12 D3 贸易与航程输入迁移

本轮只修改 D3 Gameplay 表现入口，完成第一个旧快捷键模块的统一意图接入：

- `D3RuntimeBootstrap.Update` 不再直接读取 `Keyboard.current`；贸易开关、开始/推进航程、采集、登陆结算和保存分别读取 `D16InputAction`，其中保存使用 F5 主键并兼容 C 副键。
- 贸易页关闭使用独立 `D3CloseTrade` 意图，避免把上下文关闭动作和全局暂停动作混在一起；所有领域调用仍通过原有 D3 owner 方法。
- 新增 `D16A03D3ContextBindingPersistsAndRoutesTheChangedIntent`，验证 D3 航程动作保存 Q 重绑定后仍进入同一意图。

验证：D3 源码不再出现 `Keyboard.current`/按键字段直读；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。Unity focused PlayMode 尚待许可证服务恢复后执行，当前没有把未生成 XML 的结果计为 PASS。D3 迁移完成不代表 D4/D5/D9/D10 已迁移，A03 总体仍 **IN_PROGRESS**。

## 2026-08-12 D4 内容层输入迁移

本轮只修改 D4 Gameplay 表现入口：

- `D4RuntimeBootstrap.Update` 的势力、炮战开始、装填、弹种选择、开炮、接舷、脱离、改装、动物观察、医疗、主线和保存均改为读取 D16 上下文意图。
- 修复过程中首次构建发现 D4 缺少 `Whalebone.Systems.D16` 引用，已补齐真实工程引用后串行恢复；未注释代码、未缩小测试范围。
- 新增 `D16A03D4ContextBindingPersistsAndRoutesTheChangedIntent`，覆盖 D4 开始炮战动作的保存/重绑定路由。

验证：`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error；D4 源码已无 `Keyboard.current`/按键字段直读。Unity focused PlayMode 待许可证服务恢复后执行，不把缺失 XML 计为 PASS。D5/D9/D10 仍待迁移，A03 总体继续 **IN_PROGRESS**。

## 2026-08-12 D5 岛屿与发布体验输入迁移

本轮只修改 D5 Gameplay 表现入口：

- 岛屿发现、当前岛屿访问改为 `D5DiscoverIsland` / `D5VisitIsland` 上下文意图；角色预设、语言、画质、成就和保存复用 D16 核心动作。
- `D5RuntimeBootstrap` 不再直接读取 `Keyboard.current`，D5 系统仍是世界发现、预设、性能和成就状态的唯一裁决者。
- 新增 `D16A03D5ContextBindingPersistsAndRoutesTheChangedIntent`，验证 D5 发现岛屿动作的保存/重绑定路由。

验证：D5 源码无键盘/手柄直读；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。Unity focused PlayMode 待许可证服务恢复后执行；D9/D10 仍待迁移，A03 总体继续 **IN_PROGRESS**。

## 2026-08-12 D9 港口整备输入迁移

本轮只修改 D9 Gameplay 表现入口，将港口整备页的旧快捷键接入 D16 统一意图：

- 港口页打开、Escape 关闭、靠泊、盐/木材/淡水购买、酒馆入口、修船、招募、发薪、升级和离港均改为读取 `D16InputAction`；页面关闭使用独立 `D9ClosePortPreparation`，不再与全局暂停混用。
- `D9PortShipPreparationBootstrap` 源码无 `Keyboard.current`、`Gamepad.current` 或键盘字段直读；领域请求与数值裁决保持原 owner 链路。
- 新增 `D16A03D9ContextBindingPersistsAndRoutesTheChangedIntent`，覆盖 D9 靠泊动作保存 Q 重绑定后的意图路由。

验证：`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error，D9 直读扫描无匹配。Unity focused PlayMode 仍待本机许可证服务恢复后执行，未把无 XML 结果记为 PASS。D10 尚待迁移，D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D10 酒馆社交输入迁移

本轮只修改 D10 Gameplay 表现入口：

- 酒馆页打开/关闭、悬赏板、安全工作、休息、用餐、饮酒、听传闻、扑克、舞会、夜谈、招募和敏感内容跳过均改为读取 D16 上下文意图；不再直接读取 `Keyboard.current`。
- 新增 `D16A03D10ContextBindingPersistsAndRoutesTheChangedIntent`，验证 D10 打开酒馆动作保存 Q 重绑定后仍进入 `D10OpenTavern` 意图。
- D3/D4/D5/D9/D10 五个旧快捷键模块现已全部完成代码级统一路由迁移；下一步是把所有上下文动作真实列入无障碍设置面板并补齐中英文标签，随后才能做完整 UI 运行证据。

验证：D10 直读扫描无匹配；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。Unity focused PlayMode 仍受本机 `Licensing::Module` 阻断，未生成 XML，不计为通过。D16-A03 仍 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 上下文动作设置面板全量化

本轮仍限定在 D16 Gameplay/UI 输入设置模块，收口“代码已可重绑定但 UI 没有入口”的缺口：

- `D16AccessibilityPanel.RebindableActions` 改为从 `D16AccessibilityDefaults.CreateBindings()` 自动生成，当前契约中的 61 个动作都会生成对应的键盘和手柄按钮，不再维护容易漏项的手写数组。
- `ActionLabel` 与 `Localization.cs` 同步补齐 D3/D4/D5/D9/D10 全部中英文动作标签；面板标题改为通用“操作重绑定”，避免把上下文动作误标为核心动作。
- 既有 `ScrollRect` 保持固定 300 高视口，内容高度按动作数量计算；底部状态提示、恢复默认和关闭按钮仍位于卡片内，不因新增动作进入不可点击区域。
- `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport` 改为按默认契约逐项检查所有动作的键盘/手柄按钮和可交互状态，并断言动作数已超过核心 16 项。

验证：`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests`、A03 standalone runner 均构建 0 warning / 0 error；A03 standalone **7/7 PASS**。D3/D4/D5/D9/D10 直读扫描均无匹配。Unity focused PlayMode 仍受 `Licensing::Module` 阻断，未生成 XML，不计为通过；待许可证恢复后重跑滚动、键盘导航、物理手柄和目标分辨率人工证据。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 D7 暂停/模态输入迁移

全 Gameplay 输入审计发现 `D7ControlCoordinator` 仍直接读取 `Keyboard.current` 的 Escape。该入口负责对话/贸易模态优先关闭，若继续绕过 D16，重绑定和模态锁仍可能出现不一致。

- `D7ControlCoordinator.HandleModalEscape` 已改为读取核心 `D16InputAction.Pause`，保留原有对话优先、贸易其次和 `EscapeHandledThisFrame` 行为。
- D16 的设置/敏感/诊断/金融面板自身仍保留直接设备监听，仅用于其已锁定的顶层模态关闭或重绑定采集，不属于旧 Gameplay 入口。

验证：D7 直接设备扫描无匹配；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。Unity PlayMode 仍受 `Licensing::Module` 阻断，未把无 XML 结果计为 PASS。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 Unity 6 真实运行证据与 D9/D10 夹具修正

### 许可证环境

旧 Unity Hub 3.3.3-c1 所带 Licensing Client 1.11.0 与 Unity 6 编辑器 1.17.4 协议不兼容，导致此前 batchmode 只停在 `Licensing::Module`。本机已更新 Unity Hub 到 3.20.1，外部 Licensing Client 为 1.17.4；Unity 6 已连接 Unity Personal 并成功更新许可证。该项是环境修复，未修改项目代码来掩盖缺失证据。

### 运行与编译证据

- `tools/results/d16_a03_binding_scroll_single_unity6_retry_current_03.xml`：无障碍面板滚动与 61 个动作入口专项 **1/1 PASS**。
- `tools/results/d16_a03_d9_context_unity6_retry_current_01.xml`、`d16_a03_d10_context_unity6_retry_current_01.xml`：D9/D10 上下文重绑定专项各 **1/1 PASS**。
- `tools/results/d16_a03_full_d16_playmode_unity6_retry_current_02.xml`：D16.PlayMode.Tests **34/34 PASS**，失败 0、跳过 0，运行约 66.47 秒。
- Unity 刷新后 `dotnet restore` 仅用于重新生成被 Unity 清理的临时依赖索引；随后 `Assembly-CSharp` 与 `D16.PlayMode.Tests` 均 0 warning / 0 error。

### 真实失败与修复边界

首次全量回归的 2 个失败都在 D9/D10 测试夹具：它们尝试把动作改成同一作用域中已被默认动作占用的 Q，契约正确返回重复键错误。已将这两个夹具改用同作用域未占用的 Z，并同步修改合成输入与清理；没有放宽生产层的重复绑定规则，也没有改变 D9/D10 业务逻辑。

### 当前状态

D3/D4/D5/D7/D9/D10 旧 Gameplay 入口的统一输入路由、D16 面板全动作滚动布局和 Unity 编辑器回归已收口；A03 仍 **IN_PROGRESS**，因为真实物理手柄、目标设备/分辨率、人工视觉/多场景焦点和长线稳定性尚未取得证据。D16-S01/S02 继续不得启动。

## 2026-08-12 无障碍设置保存跟随实时语言

本轮只修改 D16 Gameplay/UI 设置存储模块，修复主菜单语言与无障碍设置缓存可能互相覆盖的问题：

- `D16AccessibilitySettingsStore.Load()` 在返回已有缓存前同步 `SettingsStore.Lang`；
- `Save()` 不再信任调用方手里的旧 `Locale`，而是读取实时 `wb_lang` 后写入可访问性快照的 `LocaleKey` 镜像；
- 语言仍由主菜单 `SettingsStore/Localization` 统一拥有，无障碍设置保存不会改变玩法、存档、契约或 Systems 权限。

新增 `D16A03AccessibilitySavePreservesLiveMainMenuLanguage`，真实加载 MainMenu，先建立中文旧快照，再模拟主菜单切换英文并保存无障碍选项，断言全局语言仍为 `en` 且快照为 `en-US`。

证据：focused Unity 6 PlayMode **1/1 PASS**（`tools/results/d16_a03_live_language_save_unity6.xml`）；D16.PlayMode.Tests 全量 **36/36 PASS**（`tools/results/d16_a03_full_after_live_language_fix_unity6.xml`）；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。该子门禁已关闭，A03 总体仍 **IN_PROGRESS**，真实设备、目标分辨率、人工视觉、多场景焦点和长线稳定性仍待证据。

## 2026-08-12 用户模态互斥与输入锁恢复

### 本轮变更

- `D16AccessibilityPanel.OpenInternal()` 打开无障碍页面前关闭诊断和金融页面；
- `D16LocalDiagnosticsPanel.OpenInternal()` 打开诊断前关闭无障碍和金融页面，并记录外层输入阻断状态；关闭或销毁时只恢复自身打开前的状态；
- `D16FinanceDebtPanel.OpenInternal()` 打开金融页面前关闭无障碍和诊断页面，并增加重复打开/非打开状态保护；
- 新增 `D16A03UserModalsAreExclusiveAndRestoreOuterInputBlock`，验证外层暂停输入锁在同级模态切换和关闭后仍保持。
- `D16SensitiveContentOverlay.OnDestroy()` 只在警告层确实打开时恢复打开前状态；新增销毁/重建回归，避免切场景或重载时无条件解除外层输入锁。

### 验证

focused Unity 6 PlayMode：模态互斥 **1/1 PASS**（`tools/results/d16_a03_modal_exclusive_unity6.xml`），敏感层销毁恢复 **1/1 PASS**（`tools/results/d16_a03_sensitive_overlay_unity6.xml`）。变更后的 D16.PlayMode.Tests 全量 **38/38 PASS**：`tools/results/d16_a03_full_after_sensitive_overlay_fix_unity6.xml`；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error，`git diff --check` 通过（仅既有换行提示）。

### 交接边界

本轮关闭了编辑器内同级用户模态叠层和输入锁误解除的代码回归风险，解释并修复了“按钮显示但点击无反应”的一类根因。仍未把真实鼠标/键盘/手柄、目标设备/分辨率、人工视觉、多场景焦点和长线稳定性标记为通过；D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前 standalone 无障碍 runner 复验

按当前工作区重新构建 `tools/d16_a03_runner/D16A03Runner.csproj`：构建 0 warning / 0 error，7 项无障碍/输入/本地化/敏感内容纯逻辑测试 **7/7 PASS**。这只确认逻辑层没有回归，真实手柄、目标设备焦点、字体视觉和色觉/低刺激验收仍未完成。

## 2026-08-12 InitialHarbor 控制闭环回归

### 修改文件

- `game/Assets/Tests/PlayMode/D16InputRouterPlayModeTests.cs`

### 新增验证

`D16A03InitialHarborKeyboardCameraAndBoardingLoopWorks` 在真实 InitialHarbor 播放循环中覆盖键盘移动、鼠标右键相机拖动、视角模式切换、上船、船舶航行和下船。测试等待 D2 分阶段接线时每帧让出主线程，避免测试本身忙等阻塞被测运行时。

### 原始结果

- Unity 6 PlayMode 专项：**1/1 PASS**；
- 证据：`tools/results/d16_a03_d2_loop_playmode_unity6.xml`、`tools/results/d16_a03_d2_loop_playmode_unity6.log`；
- 日志确认 `[D2] 运行时闭环已接线`、`[Camera] 视角切换`、`[D2/Control] 已进入船只控制`、`[D2/Control] 已退出船只控制`；
- 没有修改 Systems、Save、contracts 或领域数值所有权。

### 验收结论

该专项关闭了 D2 表现层“输入映射已经存在但 InitialHarbor 实际控制链未闭合”的代码回归风险；不等同于 D16-A03 总体通过。真实物理手柄、目标设备/分辨率、人工视觉/焦点、字体覆盖、色觉/低刺激和长线稳定性仍未取得证据，D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前无障碍面板专项复验

### 原始验证

- `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`：`tools/results/d16_a03_accessibility_panel_current_unity6.xml`，**1/1 PASS**；
- 当前 D16 PlayMode 全量：`tools/results/d16_current_full_after_state_preservation_v2_unity6.xml`，**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 专项真实进入 MainMenu 设置和无障碍模态，检查滚动内容超过视口、默认契约动作的键盘/手柄按钮均存在且可交互，并触发 Map 重绑定监听。

### 结论与边界

本机代码级面板结构、按钮命中和监听入口已取得当前 Unity 运行证据；真实物理手柄、键盘导航、目标设备/分辨率、高缩放长文本、色觉/低刺激人工审查、多场景焦点和长线稳定性仍缺证据。A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 模态焦点范围修复交接

### 本轮变更

- 在 `game/Assets/Scripts/Gameplay/UI/D16/D16InputRouter.cs` 内加入 `D16ModalFocusScope`，避免新增脚本未被当前 Unity 显式程序集清单纳入；
- 接入主菜单的 MainMenu/NewGame/Settings/About、`HarborPauseController`、无障碍、诊断、金融和敏感内容模态；
- 统一显式上下左右导航、失焦恢复和 `ScrollRect` 自动可见，解决下层面板抢焦点导致确认键无效的代码风险；
- 测试通过反射读取主程序集类型，保持 `D16.PlayMode.Tests` 与 Assembly-CSharp 的既有程序集边界。

### 当前证据

- 专项：`tools/results/d16_a03_focus_scope_playmode_current_unity6.xml`，`D16A03VisibleModalKeepsKeyboardAndGamepadFocusInsideItsScope` **1/1 PASS**；
- 全量：`tools/results/d16_a03_focus_scope_full_current_unity6.xml`，D16.PlayMode.Tests **45/45 PASS**，失败/不确定/跳过均为 0；
- 构建：AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests、D16A03Runner 均 0 warning / 0 error；工具测试 **69/69 PASS**。

### 验收边界

本轮仅关闭本机 UGUI 模态焦点范围与滚动可见性的代码回归风险；未宣称真实键盘/物理手柄、目标设备与分辨率、人工可读性/色觉/低刺激、多场景焦点和长线稳定性通过。D16-A03 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 焦点候选激活状态复核

- `D16ModalFocusScope` 导航候选改为只接收运行时 `isActiveAndEnabled` 且可交互的控件，避免隐藏子面板被写入导航目标；
- 专项 `tools/results/d16_a03_focus_scope_active_hierarchy_playmode_current_unity6.xml`：**1/1 PASS**；
- 全量 `tools/results/d16_a03_focus_scope_active_hierarchy_full_current_unity6.xml`：**45/45 PASS**；
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 0 warning / 0 error；
- 本轮不改变玩法、存档、契约或 Systems 所有权。真实设备和人工可访问性证据仍缺，A03 继续 **IN_PROGRESS**。

## 2026-08-12 动态 UGUI 字体回退交接

### 本轮实现

- 在 `Fonts` 中加入整段字符覆盖检查和 `ResolveForText(...)`；CJK 文本优先使用项目内 CJK 主字体，其他文本按 preferred/default/fallback 覆盖情况选择；
- 新增 `D16FontFallbackText`，由 `D16AccessibilityRuntime` 为场景 UGUI `Text` 接入，在文本变化时刷新字体；
- 保持 UGUI 单字体模型，不伪造缺字、不把字体选择写入玩法或存档，未修改契约、Systems 和数据所有权。

### 验证与失败记录

- 初次专项运行发现编辑器 `LegacyRuntime.ttf` 对中文的 `HasCharacter` 结果受宿主环境影响，错误地保留了内置字体；该失败被保留并修正为 CJK 文本优先项目字体；
- `tools/results/d16_a03_font_fallback_fixed_current_unity6.xml`：专项 **1/1 PASS**；
- `tools/results/d16_a03_font_fallback_full_current_unity6.xml`：D16 PlayMode **52/52 PASS**；
- `Assembly-CSharp`、`D16.PlayMode.Tests`：0 warning / 0 error。

### 交接边界

本轮仅关闭编辑器运行时动态中文字体选择的代码缺口；目标设备/分辨率、真实手柄、长文本视觉、色觉/低刺激和人工字体覆盖证据仍待补齐。A03 保持 **IN_PROGRESS**，下游 D16-S01/S02 不启动。

## 2026-08-12 重绑定滚动视口专项复验

### 当前证据

- 重新执行 `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`；
- `tools/results/d16_a03_binding_scroll_current_20260812.xml`：Unity 6 PlayMode **1/1 PASS**；
- 真实断言覆盖滚动视口配置、全部默认动作的键盘/手柄重绑定按钮、按钮可交互性和 `Map` 动作进入监听状态；
- 日志 `tools/results/d16_a03_binding_scroll_current_20260812.log` 记录测试运行器完成，未使用启动返回码替代 XML 结果。

### 验收边界

该复验关闭当前编辑器分辨率下“动作列表增长后按钮被挤出、无法命中”的代码回归风险；不宣称真实鼠标滚轮、物理手柄、目标分辨率、人工焦点/长文本可读性或长线稳定性通过。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A03 当前复验

### 状态

**本机输入/无障碍代码子门禁 PASS；D16-A03 总体 IN_PROGRESS。**

### 当前证据

- `dotnet build tools/d16_a03_runner/D16A03Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；纯逻辑输入、重绑定、本地化、敏感警告/跳过测试 **7/7 PASS**；
- Unity D16 PlayMode 全量 `tools/results/d16_full_current_goal_20260812.xml`：**54/54 PASS**；
- 已有 focused 证据覆盖模态焦点、字体回退、长文本、低刺激、敏感内容、暂停、镜头/上下船和输入路由；
- 全量工具回归：**71/71 PASS**。

### 边界

编辑器与自动化结果不替代真实物理手柄、目标分辨率、人工字体/色觉/低刺激审查、多场景焦点和长线稳定性。A03 继续 IN_PROGRESS；D16-S01/S02 不启动。

## 2026-08-12 A03 当前交接快照

- 纯逻辑 runner：7/7 PASS；Unity D16 全量：54/54 PASS；全量工具测试：71/71 PASS。
- 当前已具备可复核的模态焦点、重绑定滚动、动态字体回退、暂停/镜头/上下船输入链本机证据；没有把编辑器自动化结果扩大解释为真机或人工可访问性通过。
- 未完成：真实物理手柄、目标设备/分辨率、键盘导航与焦点人工复核、色觉/低刺激/长文本人工审查、多场景焦点和长线稳定性。

结论：A03 本机代码子门禁保持 PASS，总体 **IN_PROGRESS**；D16-S01/S02 继续等待外部证据。

## 2026-08-12 D17-A03 上下船回归补充

- 上船/航行/安全下船专项 tools/results/d17_a03_boarding_safe_surface_current_20260812.xml：1/1 PASS；
- 无可行走表面时拒绝下船专项 tools/results/d17_a03_safe_disembark_refusal_current_20260812.xml：1/1 PASS；
- 该改动后的 Unity PlayMode 全量 tools/results/d16_full_after_safe_disembark_20260812.xml：56/56 PASS。

这只增加本机上下船失败恢复证据；真实手柄、目标分辨率、人工焦点/可访问性、多场景和长线证据仍缺，A03 总体继续 IN_PROGRESS。

## 2026-08-12 手柄右摇杆镜头回归

- CameraFollow 增加 Gamepad 右摇杆视角控制，保留鼠标镜头路径；输入锁打开时不读取为玩法镜头动作，关闭后恢复。
- tools/results/d17_a04_gamepad_camera_current_20260812.xml：专项 1/1 PASS；
- tools/results/d17_a04_full_after_gamepad_camera_20260812.xml：D16.PlayMode.Tests 全量 57/57 PASS；
- tools/results/d17_a04_d5_smoke_after_gamepad_camera_20260812.log：InitialHarbor verified；
- Assembly-CSharp 与 D16.PlayMode.Tests 构建均 0 warning / 0 error。

本轮只关闭 A03/D17-A04 的本机代码子门禁；真实物理手柄、目标分辨率、人工字体/色觉/低刺激、焦点和长线稳定性仍缺，D16-A03 继续 IN_PROGRESS。

## 2026-08-12 当前最终复核

- 当前工具测试：**77/77 PASS**；Unity 6 本机垂直切片全量：**60/60 PASS**；
- 本机代码已覆盖输入意图、重绑定、暂停、菜单焦点、字体/长文本、低刺激、敏感内容、镜头和上下船回归；
- A03 总体仍 **IN_PROGRESS**：真实物理手柄、目标分辨率、人工焦点/字体/色觉/低刺激和长线稳定性证据仍缺。

## 2026-08-12 物理设备预检

- 运行 `tools/d16_a03_device_probe.ps1` 后，Windows PnP 实际枚举到 `Xbox Wireless Controller` 与 `Bluetooth LE XINPUT compatible input device`；当前显示配置为 `3840x2160`，显卡为 `NVIDIA GeForce RTX 4070 Ti`。
- 预检原始记录：`tools/results/d16_a03_device_probe_raw_current.json`；摘要：`tools/results/d16_a03_device_probe_current.json`。
- 摘要明确保持 `status=IN_PROGRESS`、`physical_input_pass=PENDING_MANUAL`、`focus_pass=PENDING_MANUAL`、`accessibility_manual_pass=PENDING_MANUAL`；本轮没有发送按钮输入，也没有做人工焦点、字体、色觉或低刺激结论。
- 同一份原始预检的 XInput 只读状态为 `NO_ACTIVE_XINPUT_USER`：API 可用，但用户 0—3 均返回 Windows `1167`（设备未连接到 XInput 槽位）；系统 PnP 能看见 Xbox 控制器，但当前没有可读的活动 XInput 用户状态，不能证明 Unity/游戏能收到手柄输入。

这项预检只证明当前机器具备继续做真机输入验收的设备条件，不关闭 D16-A03 外部门禁，也不把设备枚举扩大解释为手柄操作通过。

## 2026-08-13 当前工程复验

- 全量工具回归：**79/79 PASS**；
- `Whalebone.Systems`、`Assembly-CSharp`、`Save.Tests`、`AgentCore`、`D16.PlayMode.Tests`：均 **0 warning / 0 error**；
- `git diff --check`：通过；
- 当前机器仍只有 PnP 设备存在性证据，XInput 用户 0—3 均返回 `1167`，所以 `physical_input_pass`、`focus_pass`、`accessibility_manual_pass` 仍保持 `PENDING_MANUAL`；
- 本机 Unity 6 编辑器当前不可用，未生成新的 PlayMode 证据；已留存的 Unity 6 结果继续作为历史本机基线，不作为本轮真机 PASS。

结论：A03 的本机代码子门禁没有回退，但 D16-A03 总体仍 `IN_PROGRESS`；真实手柄操作、目标分辨率、人工焦点/可访问性和长线证据仍需外部验收。

## 2026-08-13 Unity 路径更正后的最终本机基线

- 已确认 Unity 6 编辑器实际路径为 `C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`；此前“编辑器不可用”的文字是路径检查错误，不是当前工程状态。
- 当前最终本机 PlayMode 证据为 `game/tools/results/d17_playmode_full_final_20260813.xml`：**60/60 PASS**；D17 本机十循环为 `game/tools/results/d17_s02_local_story_ten_cycles_20260813_final.xml`：**1/1 PASS**，10/10 循环通过。
- 这些结果覆盖编辑器内键鼠/合成输入、模态焦点、镜头、上下船、暂停、保存恢复和连续流程；它们不替代真实手柄、目标分辨率、人工字体/色觉/低刺激和长线可访问性验收。
- 物理设备预检仍显示 XInput 用户 0—3 均返回 `1167`，所以 `physical_input_pass`、`focus_pass`、`accessibility_manual_pass` 继续为 `PENDING_MANUAL`；D16-A03 总体保持 **IN_PROGRESS**，D16-S01/S02 不启动。
