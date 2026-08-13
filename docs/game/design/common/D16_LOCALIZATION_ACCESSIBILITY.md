# D16-A03 本地化与可访问性报告

## 最终结论

状态：**IN_PROGRESS，基础实现与 Unity 回归已通过，完整无障碍矩阵未通过**。

D15-S02 已通过；当前已完成设置面板、重绑定校验、字幕承载和 D9 cue 抽样，真实手柄/目标设备、字体覆盖/长文本、色觉/低刺激、多场景焦点和长线仍未完成。本文件不将局部能力误报为 A03 总 PASS。

## 门禁证据

命令：

python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json

历史结果：D15_S02_QA_EXIT=1；当前顺序复验结果：D15-S02 PASS（0 errors、0 warnings）。

与 D16-A03 直接相关的阻断：

- D15-A08 已有 zh-CN/en-US 各 452 个配对 key；
- D15-S02 仍发现 A01—A07 共 2536 个 display_name_key 没有可验证的中英 locale 包证明；
- 9 个 D15 抽样缺明确保存标记，不能把语言/可访问性设置的保存恢复标为通过；
- D15-A08 的任务/音频条目仍有测试和 LOD-N/A 元数据缺口。

## 已读取基线

- D15-A08 目录：任务、音频层和 452 个中英 key；音频目录有 subtitle_key、触发状态、回退和审查字段，但正式二进制仍是 placeholder。
- D10 外观/社交：外观槽位使用稳定 ID 和本地化 key；酒馆/关系敏感内容使用 WarningRequired、SkipSensitiveContent 和审查状态。
- D12 牌局/社群：牌局和社群内容由所属系统裁决；社群敏感内容必须由审查状态、警告和跳过路径驱动。
- 当前 Localization.cs：存在中英文静态字典和 key 查询，但仍是 UI 层本地字典，不是完整 D15 locale 包加载器。
- 当前 SettingsStore.cs：只保存音量、画质和 zh/en 语言设置。
- 当前 HarborPauseController.cs：InitialHarbor 使用 Escape、Time.timeScale 和 InputSystemUIInputModule 实现暂停。
- 当前输入代码：多个 D2—D10 Gameplay Bootstrap 直接读取 Keyboard.current，未见统一的输入意图层、输入重绑定存储和手柄验收入口。
- 当前资源：存在一个 CJK 占位字体文件，未见字体链/缺字回退和中英长文本验收。

## D16-A03 验收矩阵

| 项目 | 当前状态 | 说明 |
|---|---|---|
| 中英双语 key 完整性 | BLOCKED | A08 452 key 配对通过，但 A01—A07 的 2536 key 缺 locale 包证明 |
| 中英长文本布局 | CODE PASS / VISUAL PENDING | 真实 UGUI 长文本已验证换行、Overflow、字体和 2 倍字号；目标分辨率截图与人工排版仍缺 |
| 数字格式 | LOGIC/RUNTIME/DOMAIN-INTEGRATION PASS / DEVICE-VISUAL PENDING | `D16DisplayFormatter` 已接入 D3/D9 航程、贸易和整备表现层；债务专用界面、目标设备和人工可读性仍缺 |
| 键鼠输入 | EDITOR PASS / DEVICE PENDING | 统一意图、模态阻断和 Esc 暂停已有编辑器回归；真实设备仍缺 |
| 手柄输入 | EDITOR PASS / DEVICE PENDING | Unity Input System 合成手柄导航已有回归；物理手柄/目标设备仍缺 |
| 输入重绑定 | EDITOR PASS | 重绑定持久化、冲突拒绝和真实按钮链已有代码级回归；真机仍缺 |
| 字幕 | CODE/RUNTIME PASS / DEVICE PENDING | D9 14 cue 已完成 EventLog→音频→字幕抽样；真实音频素材/设备仍缺 |
| 色盲可读性 | CODE PASS / VISUAL PENDING | 高对比度、Deuteranopia 及颜色变换已在真实 UGUI 上验证；人工场景仍缺 |
| 字体回退 | PARTIAL | CJK 字体资源加载并应用已验证；完整中英字符覆盖和缺字回退仍缺 |
| 暂停 | EDITOR PASS / MULTI-SCENE PENDING | InitialHarbor Esc、菜单焦点和模态恢复已有回归；所有玩法场景仍缺 |
| 低刺激选项 | CODE PASS / VISUAL PENDING | 设置保存、面板开关和真实 UGUI 颜色适配已验证；目标设备人工可读性仍缺 |
| 敏感内容警告/跳过 | CODE PASS / CROSS-SCENE PENDING | 本地化警告、Continue/Skip、Esc/Start 跳过和输入恢复已有真实 MainMenu 回归；跨场景抽样仍缺 |
| 关键价格/警告不被截断 | CODE PASS / VISUAL PENDING | 窄幅长文本和 Overflow 代码已验证；最小窗口、高缩放人工验收仍缺 |

## 基线阶段未做的工作（历史记录）

- 未修改 Localization、Input、UI、Settings、字体或字幕代码；
- 未将逻辑文本硬编码到 Systems；
- 未改变输入层业务数值或任何领域写者；
- 未宣称手柄、重绑定、色盲、字体回退、低刺激或敏感内容 UI 通过；
- 未新增网络或 LLM；
- 未删除本地化缺口内容或把占位 key 标记为完成。

## 重新开工门槛

1. D15-S02 统一 QA 错误归零并重新出具通过报告；
2. 为 A01—A07 建立可扫描的 zh-CN/en-US locale 包或明确的阶段性占位证明；
3. 建立统一 Localization/Input/Accessibility 数据边界，UI 只消费 key、意图和审查快照；
4. 补齐长文本、数字格式、键鼠/手柄、重绑定、字幕、色盲、字体回退、暂停、低刺激和敏感内容警告/跳过测试；
5. 验证关键价格、警告、债务和法律后果在中英文、高缩放、最小窗口下不被截断或隐藏。

## 2026-08-12 输入意图运行时子门禁更新

新增 `game/Assets/Tests/PlayMode/D16InputRouterPlayModeTests.cs`，使用 Unity Input System Test Framework 的真实测试夹具注入合成键盘与手柄事件，验证：

- 手柄 West 键到达 `Interact` 意图，并正确区分持续按下与本帧按下；
- 键盘 W 到达 `MoveForward` 意图并产生正向移动轴；
- 模态层启用 `SetGameplayInputBlocked(true)` 后，键盘和手柄输入均不会进入玩法意图层；
- 测试结束后恢复输入阻断与无障碍设置，避免污染后续用例。

`D16.PlayMode.Tests.asmdef` 已补充 `Unity.InputSystem` 与 `Unity.InputSystem.TestFramework` 引用。Unity 6 focused PlayMode 为 **1/1 PASS**（`tools/results/d16_a03_input_router_single_unity6.xml`），D16 PlayMode 全量为 **12/12 PASS**（`tools/results/d16_a03_input_router_full_playmode_unity6.xml`）。

该子门禁只证明当前编辑器中的输入意图接线与模态阻断，不等同于真实物理手柄、目标设备、字体/长文本、色觉/低刺激场景、多场景焦点和长时间运行验收；D16-A03 继续保持 **IN_PROGRESS**。本轮没有修改 Systems 的领域裁决、存档或网络代码。

## 2026-08-12 暂停菜单焦点与 Esc 集成子门禁更新

新增两条 InitialHarbor PlayMode 回归：

- `D16A03PauseMenuKeepsFocusAcrossAccessibilityAndDiagnostics`：真实调用暂停入口，确认暂停后 EventSystem 焦点落到“继续游戏”；无障碍和本地诊断模态都能打开、阻断玩法输入、关闭后保留暂停状态；最后继续按钮恢复暂停状态和时间流速。
- `D16A03SyntheticEscapeTogglesHarborPauseThroughController`：使用 Unity Input System 合成键盘 Escape，交由 Unity 下一帧和 `HarborPauseController.Update` 处理，实测暂停与恢复日志均出现。

focused Esc 集成 PlayMode 为 **1/1 PASS**（`tools/results/d16_a03_escape_pause_single_unity6.xml`）；加入该用例后的 D16 PlayMode 全量为 **14/14 PASS**（`tools/results/d16_a03_escape_pause_full_d16_playmode_unity6.xml`，日志同名 `.log`）。这关闭了编辑器内“Esc→暂停控制器”和“暂停菜单多模态焦点”子门禁，但仍不等同于真实物理设备、目标硬件、字体/长文本、色觉/低刺激和长时间运行验收；A03 继续 **IN_PROGRESS**。

## 2026-08-11 门禁复验更新

D15-S02 已通过；A01—A07 新增的 `D15_A01_A07` zh-CN/en-US 包已被统一 QA 验证，非 A08 key 缺失证明数为 0，合并可验证 key 为 2,988。D16-A03 当前状态更新为 **PENDING**：长文本布局、数字格式、手柄、重绑定、字幕、色盲、字体回退、低刺激选项和全场景暂停/菜单焦点仍未完成运行时验收。

## 2026-08-11 实现进度更新

当前状态：**IN_PROGRESS，运行时全量验收未完成**。

已实现并接入：

- `D16AccessibilityContracts`：语言、文字缩放、高对比度、色觉模式、低刺激/减弱动态、字幕、敏感内容警告和输入绑定契约；统一校验重复按键、非法语言和缩放范围。
- `D16AccessibilitySettingsStore`：PlayerPrefs 保存/读取表现层偏好和键鼠/手柄绑定；不写存档和领域状态。
- `D16InputRouter`：键盘与手柄映射到统一动作意图，D2 人物移动、奔跑、跳跃、交互、船舶操控、视角切换和 InitialHarbor 暂停已接入。
- `D16AccessibilityRuntime`：场景切换或设置变化后应用文字缩放、自动换行和高对比度，并输出当前设置快照日志。

证据：Systems、Save.Tests、Assembly-CSharp 三套构建均为 0 warning / 0 error；`tools/d16_a03_runner/D16A03Runner.csproj` 的 5 项纯逻辑测试全部通过。

仍未宣称通过：真实手柄设备导航、全场景输入覆盖、字幕音频触发、字体 fallback、色觉滤镜的场景可读性、低刺激视觉效果、最小窗口/高缩放长文本截图，以及 Unity PlayMode 验收。Unity 批处理目前停在 `Licensing::Module`，不能把独立测试当作运行时证据。

## 2026-08-11 Unity 6 可操作链更新

当前状态仍为：**IN_PROGRESS，不关闭 A03 总门禁**。

本轮补齐了玩家可以实际操作的 UI/输入链：

- 主菜单“设置”和 InitialHarbor 暂停菜单都提供“无障碍与键位”入口；面板使用独立模态 Canvas，打开时锁定玩法输入，关闭后恢复，避免菜单与移动/船舶/视角输入串台。
- 支持键盘与手柄按键捕获、冲突拒绝、重置默认键位和即时保存；`D16AccessibilitySettings.TrySetBinding` 是唯一的绑定校验入口。
- `D16AccessibilityRuntime` 已消费文字缩放、字体回退、高对比度、色觉辅助、低刺激和减弱动态设置；新增 `D16SubtitleOverlay` 提供字幕显示/说话人标签/敏感内容开关的运行时承载。

验证证据：`tools/d16_a03_runner` 纯逻辑测试 **6/6 PASS**；Systems、Save.Tests、Assembly-CSharp 和 runner 构建均 **0 warning / 0 error**；Unity 6 `D16.PlayMode.Tests` **3/3 PASS**（含主菜单→设置→无障碍面板→关闭的真实点击链）；Unity 6 EditMode 全量回归 **292/292 PASS**。日志与 XML 分别见 `tools/results/d16_a03_playmode_unity6.log`、`tools/results/d16_a03_playmode_test_results.xml`、`tools/results/d16_a03_editmode_unity6.log`。

仍未宣称通过：真实物理手柄导航与重绑定、实际音频事件到字幕的触发接线、目标设备长文本/最小窗口截图、跨语言字体覆盖、色觉/低刺激场景审查和多场景焦点回归。`D16SubtitleOverlay` 已有显示承载，但当前没有把所有 D7/D9 音频 cue 自动接入字幕，因此 A03 仍保持 IN_PROGRESS。

## 2026-08-11 字幕事件接线更新

`D9AudioPresentationController.PlayCue` 现在在音频 cue 成功播放后发布对应的 `subtitle_<cue>` 文案到 `D16SubtitleOverlay`；14 个 D9 cue 已在 `Localization` 中登记 zh/en 双语文案。该调用仍然是表现层旁路，只读取字幕开关，不写 EventLog、存档或领域状态。

Unity 6 图形设备 PlayMode 回归 **4/4 PASS**，相关程序集和 A03 纯逻辑测试 **6/6 PASS**。本轮没有通过实际 D9 事件场景逐条触发 14 个 cue，因此仍待完成事件触发抽样、真实手柄/目标设备、字体覆盖与长文本截图、色觉/低刺激场景审查及多场景焦点回归；A03 保持 IN_PROGRESS。

## 2026-08-11 D9 真实事件字幕验收

Unity 6 PlayMode 新增的 `D16A03D9EventPublishesAudioCueAndSubtitle` 已通过：在 InitialHarbor 的共享 EventLog 追加 `event_d9_crew_report`，实际观察到 `AudioCue=crew_report`、`SubtitleVisible=true`、字幕文本“船员报告”。证据：`tools/results/d16_a03_d9_subtitle_runtime_report.json`、`tools/results/d16_a03_d9_subtitle_playmode_test_results.xml`、`tools/results/d16_a03_d9_subtitle_playmode_unity6.log`。

这关闭了 D9→音频→字幕的代表性运行链子门禁，但不是 14 个 cue 全部场景验收；真实手柄/目标设备、字体覆盖/长文本、色觉/低刺激、多场景焦点和更广事件抽样仍待完成。

## 2026-08-12 设置页按钮布局与 PlayMode 全量复验

本轮修复了“设置页能看到按钮但点击没有反应”的实际 UI 缺陷。根因是 BtnDiagnostics 与 BtnClose 的 anchored Y 位置分别落在 1080 参考视口外（旧值约为 -430/-505）；面板视觉仍可能显示部分内容，但按钮的真实命中区域已经离开可交互视口，造成按钮看起来存在却收不到指针事件。

- game/Assets/Scripts/Gameplay/UI/MainMenuController.cs：将设置标题、滑杆、无障碍、诊断和关闭按钮重新排入参考视口，保持按钮之间不重叠；未修改 Systems、存档、网络或领域数值裁决。
- game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs：新增 D16A03SettingsButtonsHaveUsableHitTargetsAndOpenRealModals，检查三个按钮存在、可交互、位于参考视口且不互相覆盖，并实际打开/关闭无障碍与诊断模态及设置页。
- dotnet build game/Assembly-CSharp.csproj -v:minimal：0 warning / 0 error。
- Unity 6 D16.PlayMode.Tests 全量：**15/15 PASS**；新增设置按钮测试：**1/1 PASS**。结果文件为 game/tools/results/d16_a03_settings_layout_full_playmode_unity6_final.xml，日志为同名 .log。

该修复关闭了编辑器运行时“设置页按钮越界/不可命中”的子门禁，但 D16-A03 总体仍为 **IN_PROGRESS**：真实物理设备、目标硬件、字体与长文本、色觉/低刺激及长时间运行证据仍未完成。

## 2026-08-12 主菜单面板互斥回归

为覆盖“多个菜单叠加并互相拦截点击”的同类问题，新增 D16A03MainMenuPanelVisibilityIsExclusiveAcrossNavigation。测试依次打开设置、新游戏、关于并返回，验证四个主菜单面板任一时刻只有一个 active；同时保留设置按钮命中区域与真实模态开关测试。

Unity 6 D16.PlayMode.Tests 最新全量为 **16/16 PASS**，其中设置按钮测试和面板互斥测试均 **1/1 PASS**。证据为 game/tools/results/d16_a03_menu_regression_full_playmode_unity6.xml，日志为 tools/results/d16_a03_menu_regression_full_playmode_unity6.log。A03 总体仍为 IN_PROGRESS，外部设备和完整可访问性证据不变。

## 2026-08-12 静态 UI 本地化表结构门禁

为避免 UI 自有文案 key 在中英静态表之间漂移，`game/Assets/Scripts/Gameplay/UI/Localization.cs` 新增只读 `ValidateTables()`：逐项检查 zh/en key 对称以及两侧值非空；它不替代 D15 locale 包，也不改变内容/玩法数据。

`D16RuntimeAcceptanceTests` 新增 `D16A03StaticUiLocalizationTablesAreSymmetricAndNonEmpty`。由于 PlayMode 测试程序集不直接依赖未拆分的 `Assembly-CSharp`，测试通过 Unity 运行时反射加载实际 `Localization` 类型，避免给生产模块倒灌测试引用。

证据：

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- Unity 6 focused PlayMode：**1/1 PASS**，`game/tools/results/d16_a03_localization_tables_playmode_unity6.xml`；
- Unity 6 D16 PlayMode 全量：**17/17 PASS**，`game/tools/results/d16_a03_localization_tables_full_d16_playmode_unity6.xml`，日志为同名 `.log`。

该子门禁关闭“静态 UI 表结构不对称/空值”的代码级风险，但 A03 总体仍为 **IN_PROGRESS**：真实手柄与目标设备、字体覆盖和长文本截图、色觉/低刺激场景审查、多场景焦点及长时间运行证据仍未完成。

## 2026-08-12 长文本、字体与视觉辅助运行时复验

新增 `D16A03AccessibilityRuntimeAppliesLongTextFontScaleAndColorModes`，在真实 MainMenu 场景创建窄幅 UGUI 长文本，调用 `D16AccessibilityRuntime` 实际应用设置，验证中文字体资源、Wrap/Overflow、2 倍字号、高对比度和 Deuteranopia 颜色变换。

- Unity 6 图形 PlayMode `D16.PlayMode.Tests`：**18/18 PASS**；
- 运行时报告：`tools/results/d16_a03_accessibility_runtime_report.json`，相关字段全部为 `true`；
- XML/日志：`tools/results/d16_a03_accessibility_playmode_unity6.xml`、`.log`；
- 工具全量回归：**53/53 PASS**。

该结果只关闭编辑器运行时的字体/长文本/视觉辅助代码子门禁，不替代目标分辨率截图、真实手柄/目标设备、色觉/低刺激人工复核、多场景焦点和长线稳定性。A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 敏感内容警告与跳过运行时复验

本轮补齐敏感内容的 UI 表现层闭环。`D16SensitiveContentOverlay` 通过现有 `D16SensitiveContentPolicy` 与 `SensitiveWarnings` 偏好决定显示警告或直接跳过；显示时锁定当前输入路由，用户可以点击“继续查看”或“跳过内容”，Esc/Start 也走跳过路径，结束后恢复原输入状态。`PublishSensitiveSubtitle(...)` 只负责字幕显示/清除和回调，不获得领域数据写权限。

同步补齐 `Localization.cs` 中 zh-CN/en-US 的敏感内容标题、正文、继续和跳过文案，并新增真实 MainMenu PlayMode 回归 `D16SensitiveContentOverlayTests`，覆盖：

- 警告开启时的真实模态显示与输入阻断；
- 真实 Continue/Skip Button `onClick` 链及输入恢复；
- 关闭敏感警告偏好后的即时跳过，不残留模态层。

证据：Unity 6 D16.PlayMode.Tests 全量 **20/20 PASS**，XML/日志为 `tools/results/d16_a03_sensitive_warning_playmode_unity6.xml` 与 `.log`。这只关闭编辑器运行时敏感内容警告/继续/跳过的代码子门禁，不等价于真机输入、目标分辨率、色觉/低刺激人工可读性、多场景焦点或长线稳定性验收。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 低刺激选项运行时复验

早期基线表把低刺激记为 NOT FOUND，但当前实现已经有 `D16AccessibilitySettingsStore.LowStimulus`、无障碍面板开关和 `D16AccessibilityRuntime.ApplyVisualColor` 消费路径。本轮补上缺失的真实运行时断言：扩展 `D16A03AccessibilityRuntimeAppliesLongTextFontScaleAndColorModes`，在真实 MainMenu UGUI 长文本上保存 LowStimulus 设置、调用运行时适配器并验证颜色实际改变；报告字段 `LowStimulusApplied=true`。

- Unity 6 D16.PlayMode.Tests：**21/21 PASS**；
- 证据：`tools/results/d16_a03_low_stimulus_playmode_unity6.xml`、`tools/results/d16_a03_low_stimulus_playmode_unity6.log`；
- 最新报告：`tools/results/d16_a03_accessibility_runtime_report.json`，字体、长文本、缩放、高对比度、色觉和低刺激字段均为 `true`。

这只关闭编辑器运行时低刺激代码子门禁，不等价于目标设备上的亮度、动画、对比度和长时间可读性人工验收。A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前无障碍面板运行证据复验

Unity 许可证恢复后重新执行此前缺失的滚动面板专项：

- `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`：`tools/results/d16_a03_accessibility_panel_current_unity6.xml`，**1/1 PASS**；
- 真实 MainMenu → 设置 → 无障碍模态链创建 `ScrollRect`，逐项检查当前默认契约动作的键盘/手柄重绑定按钮、可交互状态和滚动内容高度，并点击 Map 重绑定按钮验证进入监听状态；
- 变更后的完整 D16 PlayMode：`tools/results/d16_current_full_after_state_preservation_v2_unity6.xml`，**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped。

该复验关闭了面板结构和本机按钮命中链的代码级缺口；它仍不替代真实物理手柄、键盘导航、目标分辨率/高缩放长文本、色觉与低刺激人工审查、多场景焦点和长线稳定性，因此 D16-A03 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 数字格式运行时 UI 复验

`D16DisplayFormatter` 已有纯逻辑格式契约；本轮新增 `D16A03RuntimeNumberFormattingFeedsUiText`，在真实 MainMenu 场景创建 UGUI `Text`，将 `en-US`/`zh-CN` 数字、负数和百分比结果实际写入文本后断言显示值：`1,234.50`、`-12`、`25%`。

- Unity 6 D16.PlayMode.Tests 全量：**21/21 PASS**；
- 证据：`tools/results/d16_a03_number_format_playmode_unity6.xml`、`tools/results/d16_a03_number_format_playmode_unity6.log`；
- 该测试只验证表现层格式化与 UGUI 消费，不替代债务、重量、时间、价格等具体领域字段的接入抽样。A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 领域数字字段接入复验

本轮把统一 `D16DisplayFormatter` 接入现有表现层读模型和 HUD 消费点：

- `D9PresentationReadModel` 接收当前 locale，并格式化航程日数、金币、重量/容量、吃水、货物数量、补给、损伤、船员、改装等级和市场库存；
- `D9VoyageHudController` 将 `Localization.IsEnglish` 映射为 `en-US`/`zh-CN` 后传入读模型；
- `D3RuntimeBootstrap` 的航程 HUD、贸易库存、买卖单价、当前金币和金币变化改为统一格式；
- `D9PortShipPreparationBootstrap` 的港口金币、船舶重量/吃水/船员/补给/船体/货物/改装改为统一格式。

这些类仍然只读取 Systems 已裁决快照；没有在 Gameplay 中计算价格、重量、债务或航程，也没有改存档、契约或 Systems 写权限。新增 `D16A03D9PresentationReadModelFormatsDomainNumbersBeforeUiConsumption`，通过反射保持测试程序集边界，实际调用主程序集 D9 读模型并断言分组数字、吃水、航程日数和库存数量进入 UI 文本。

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- Unity 6 D16.PlayMode.Tests：**22/22 PASS**；
- 证据：`tools/results/d16_a03_domain_number_full_playmode_unity6_retry3.xml`、`tools/results/d16_a03_domain_number_full_playmode_unity6_retry3.log`。

该结果关闭了领域数字字段的代码级表现接入子门禁，但不替代债务专用界面、真实手柄/目标设备、目标分辨率截图、字体覆盖、色觉/低刺激人工复核、多场景焦点和长线稳定性。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 金融与债务页面接入

新增 `D16FinanceDebtPanel`，从 InitialHarbor 暂停菜单打开，只消费 `GameState.CurrentD13FinanceRuntime` 的 FinanceSystem 快照。页面用滚动文本展示账户现金/授信/信用评分，以及每条债务的状态、本金、利息、滞纳金、当前应付、到期日、风险说明和最坏结果；不写入金融状态、不提交操作请求。

- zh-CN/en-US 页面文案与债务状态文案均登记在 `Localization`，通过 `ValidateTables()` 检查对称性；
- 数字统一使用 `D16DisplayFormatter`；模态打开/关闭沿用 D16 输入阻断与 EventSystem 规则；
- Unity 6 金融页面专项 **1/1 PASS**：`tools/results/d16_a03_finance_debt_single_unity6.xml`；
- 新增页面后的 D16 PlayMode **24/24 PASS**：`tools/results/d16_a03_finance_debt_full_d16_playmode_unity6.xml`。

这只关闭了“没有债务专用只读表现页”的代码子缺口。真实设备、目标分辨率/长文本截图、色觉/低刺激人工复核、多场景焦点和长线稳定性仍未取得证据，D16-A03 总体保持 **IN_PROGRESS**。

## 2026-08-12 手柄触发器重绑定路由回归

`D16InputRouter` 现支持 `LeftTrigger` / `RightTrigger` 作为模拟轴绑定，并按设备、Unity 帧对触发器边沿采样，确保一次性意图只在按下边沿产生一次，持续按住仍可读取保持状态。新增 `D16A03GamepadRebindPersistsAndRoutesTheChangedIntent`，真实保存并重新读取 `Quality → LeftTrigger` 后，用 Unity Input System 合成输入验证按下、保持和下一帧不重复。

- focused Unity 6 PlayMode：**1/1 PASS**，`tools/results/d16_a03_gamepad_rebind_single_unity6.xml`；
- D16 PlayMode 全量：**27/27 PASS**，`tools/results/d16_a03_gamepad_rebind_full_d16_playmode_unity6.xml`；
- `Assembly-CSharp`、`Save.Tests`、`AgentCore` 串行构建：0 warning / 0 error。

该结果只关闭编辑器内手柄扳机重绑定与意图路由的代码回归风险，不等同于真实物理手柄、目标设备、目标分辨率、人工视觉和长线稳定性通过；D16-A03 继续 **IN_PROGRESS**。

## 2026-08-12 核心键位面板与 16 动作滚动布局

统一输入契约已经包含 16 个 `D16InputAction`，但此前 `D16AccessibilityPanel` 只生成前 9 个动作的重绑定控件。直接把剩余动作追加到旧的固定坐标会把状态提示和底部按钮推到卡片边界之外，重现“按钮可见但无法点击”的问题。

本轮在 Gameplay/UI 模块内完成：

- 面板现在展示契约已有的全部 16 个动作：移动、奔跑、跳跃、交互/上下船、暂停、切换视角、地图、贸易、角色预设、语言、画质、成就和保存；
- 16 行绑定控件放入 `ScrollRect` 视口，状态提示、恢复默认和关闭按钮保留在可命中区域；
- 中英文动作标签保持静态表对称，未新增隐含动作或改变领域系统；
- PlayMode 结构测试覆盖视口可滚动、16×键盘/手柄按钮存在且可交互，并点击新增 Map 绑定按钮确认进入监听状态。

本机代码证据：`Assembly-CSharp`、`D16.PlayMode.Tests` 和 A03 standalone runner 均构建 0 warning / 0 error；A03 standalone 6/6 PASS。Unity 6 专项启动因 `Licensing::Module` 未完成而没有生成 XML，不能冒充运行时通过；许可证恢复后需重跑 `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`，并补充真实滚轮、键盘导航、物理手柄和目标分辨率人工检查。

## 2026-08-12 无障碍设置保存跟随实时语言

修复 `D16AccessibilitySettingsStore` 的真实状态一致性缺口：`wb_lang` 由主菜单 `SettingsStore/Localization` 统一拥有；无障碍设置面板没有独立语言控件，因此保存文字大小、字幕、色觉或重绑定时，必须读取当前语言，而不能把打开面板时缓存的旧 `Locale` 写回全局语言键。

- `Load()` 在已有缓存返回前同步当前界面语言；`Save()` 强制以实时 `wb_lang` 生成可访问性快照，保留 `LocaleKey` 作为一致性镜像；未改变任何玩法、存档、契约或 Systems 所有权。
- 新增 `D16A03AccessibilitySavePreservesLiveMainMenuLanguage`：真实加载 MainMenu，建立中文旧快照，模拟主菜单切换英文，再保存无障碍选项，断言 `wb_lang=en` 且快照为 `en-US`。
- Unity 6 focused PlayMode：**1/1 PASS**，`tools/results/d16_a03_live_language_save_unity6.xml`；D16.PlayMode.Tests 全量：**36/36 PASS**，`tools/results/d16_a03_full_after_live_language_fix_unity6.xml`，失败/不确定/跳过均为 0。
- `Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error；`git diff --check` 通过（仅保留工作区既有换行提示）。

该增量关闭“切换语言后修改无障碍选项会回退语言”的代码级风险，但 A03 总体仍为 **IN_PROGRESS**：真实手柄/目标设备、目标分辨率截图、字体覆盖、色觉/低刺激人工复核、多场景焦点和长线稳定性仍缺独立证据。

## 2026-08-12 D16 用户模态互斥与外层输入锁

为解决多个菜单同时存在时高排序 UI 拦截点击的问题，D16 的无障碍、诊断和金融页面统一按用户模态互斥处理：打开一个页面前关闭另外两个页面；诊断和金融页面只在自己确实打开时改变输入阻断，并在关闭/销毁时恢复打开前的外层状态。这样暂停菜单的输入锁不会因为子面板关闭而被错误解除，也不会留下不可见的高层面板继续拦截按钮。

- `D16A03UserModalsAreExclusiveAndRestoreOuterInputBlock` focused Unity 6 PlayMode **1/1 PASS**；
- D16.PlayMode.Tests 全量 **38/38 PASS**，失败/不确定/跳过均为 0；
- `D16A03SensitiveOverlayRestoresOuterInputBlockOnDestroy` focused Unity 6 PlayMode **1/1 PASS**，覆盖销毁后实例重建和外层输入锁保持；
- 本轮没有改变跨端契约、领域系统、存档或网络权限；A03 仍需真实输入设备、目标分辨率、人工视觉和多场景焦点验收。

## 2026-08-12 InitialHarbor 控制闭环回归

新增 `D16A03InitialHarborKeyboardCameraAndBoardingLoopWorks`，在真实 InitialHarbor PlayMode 场景中使用 Unity Input System 合成设备验证：键盘 W 驱动玩家移动、鼠标右键拖动改变相机方向、视角模式循环切换、初始距离内上船、上船后 W 驱动船航行，以及再次交互下船并恢复人物控制。

证据：`tools/results/d16_a03_d2_loop_playmode_unity6.xml`，专项 **1/1 PASS**；日志确认 D2 运行时闭环、相机视角切换、Ship/Character 控制权切换和物理落地。该测试还修正了等待 D2 分阶段接线时必须逐帧让出主线程的问题，避免测试忙等反过来制造假死。

本轮只新增表现层 PlayMode 回归，没有改变 Systems、存档、契约或输入所有权。该结果关闭了“输入映射存在但 InitialHarbor 实际不可操作”的代码级回归风险；真实物理手柄、目标设备/分辨率、人工视觉/焦点和长线稳定性仍缺证据，D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 模态焦点范围与键盘/手柄导航回归

此前各模态只设置了初始选中控件，UGUI 自动导航可能从当前窗口穿透到下层暂停菜单或隐藏面板，造成“按钮可见但确认键没有反应”。本轮在 Gameplay/UI 模块加入 `D16ModalFocusScope`，并接入主菜单四个页面、港口暂停菜单、无障碍、诊断、金融和敏感内容模态：

- 模态启用时重建当前可用 `Selectable` 的显式上下左右导航，所有目标都限制在当前模态层级内；
- 当前选中控件离开模态或失效时自动恢复到初始控件/第一个可用控件；
- 焦点进入 `ScrollRect` 时自动调整内容位置，避免键盘/手柄焦点移到不可见行；
- 组件只管理 UGUI `EventSystem` 选择状态，不写玩法、存档、契约或领域数据。

验证：`D16A03VisibleModalKeepsKeyboardAndGamepadFocusInsideItsScope` 专项 **1/1 PASS**，证据为 `tools/results/d16_a03_focus_scope_playmode_current_unity6.xml`；当前 D16 PlayMode 全量 **45/45 PASS**，证据为 `tools/results/d16_a03_focus_scope_full_current_unity6.xml`；AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests、A03 独立检查均构建通过；工具测试 **69/69 PASS**。

该增量关闭了本机模态焦点逃逸的代码级回归风险，但不替代真实物理手柄、目标设备/分辨率、人工视觉与焦点审查、低刺激/色觉人工验收和长线稳定性。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 模态焦点激活状态复核

补强 `D16ModalFocusScope` 的候选控件筛选：导航范围现在只纳入 `isActiveAndEnabled` 且可交互的 `Selectable`，不把激活层级下的隐藏子面板控件写入上下左右导航。对应测试辅助断言同步采用相同的运行时激活语义。

- `tools/results/d16_a03_focus_scope_active_hierarchy_playmode_current_unity6.xml`：专项 **1/1 PASS**；
- `tools/results/d16_a03_focus_scope_active_hierarchy_full_current_unity6.xml`：D16 PlayMode **45/45 PASS**，失败/不确定/跳过均为 0；
- `Assembly-CSharp` 与 `D16.PlayMode.Tests` 构建均 0 warning / 0 error。

该项仍只证明本机 UGUI 导航链的代码回归稳定；真实手柄、目标设备/分辨率和人工焦点可用性仍待外部证据，A03 保持 **IN_PROGRESS**。

## 2026-08-12 UGUI 动态文本字体回退

本轮补齐动态 UGUI `Text` 的内容驱动字体选择：

- `Fonts.ResolveForText(...)` 会检查整段文本的字符覆盖；包含 CJK 字符时优先使用项目内 `Resources/Fonts/ph_font_noto_sans_cjk_sc`，再按候选字体覆盖情况选择，避免把宿主系统的字体回退误当成目标设备保证；
- `D16FontFallbackText` 挂载在场景 UGUI 文本上，只在文本或当前字体变化时重新选择字体，不拆分文本、不伪造字形，也不写入玩法、存档或领域数据；
- `D16AccessibilityRuntime` 在应用文字缩放、换行和溢出设置时确保每个 UGUI 文本接入该回退组件，动态更新的中文标题、警告和长文本也走同一条路径。

验证过程中专项测试第一次确实暴露出编辑器 `LegacyRuntime.ttf` 对中文返回“可覆盖”的宿主环境假象；该结果没有被计为通过，随后调整为 CJK 文本优先项目字体。修正后的证据为：

- `tools/results/d16_a03_font_fallback_fixed_current_unity6.xml`：专项 **1/1 PASS**；
- `tools/results/d16_a03_font_fallback_full_current_unity6.xml`：D16 PlayMode **52/52 PASS**，failed/inconclusive/skipped 均为 0；
- `Assembly-CSharp` 与 `D16.PlayMode.Tests`：0 warning / 0 error。

该结果只关闭本机动态 UGUI 字体选择的代码回归风险，不替代目标设备上的字体覆盖、目标分辨率截图、长文本人工可读性、色觉/低刺激和真实输入验收。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A03 重绑定滚动视口专项复验

重新执行 `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`，确认当前工作树的无障碍面板没有因为动作数量增加而把按钮挤出卡片或落入不可命中区域。

- `tools/results/d16_a03_binding_scroll_current_20260812.xml`：Unity 6 PlayMode **1/1 PASS**；
- 断言覆盖 `ScrollRect`、viewport/content 配置、全部默认动作的键盘与手柄重绑定按钮、按钮可交互状态，以及 `Map` 动作进入监听状态；
- Unity 日志记录测试运行器完成，未把进程启动返回误认为测试结果；
- 该证据只关闭当前编辑器分辨率下的滚动面板代码回归风险，不替代真实鼠标滚轮、物理手柄、目标分辨率、人工焦点与长文本可读性验收。

D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A03 当前证据

- `tools/d16_a03_runner/D16A03Runner.csproj` 构建 0 warning / 0 error，纯逻辑输入/重绑定/本地化/敏感内容测试 **7/7 PASS**；
- Unity D16 PlayMode 全量 `tools/results/d16_full_current_goal_20260812.xml`：**54/54 PASS**；
- 编辑器内焦点、字体、长文本、低刺激、敏感警告/跳过、暂停和 InitialHarbor 控制闭环已有专项证据。

真实物理手柄、目标分辨率、人工视觉/色觉/低刺激、多场景焦点和长线稳定性仍缺；A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
