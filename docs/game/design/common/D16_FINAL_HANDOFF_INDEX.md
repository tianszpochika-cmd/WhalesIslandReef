# D16 最终文档与交接索引

## 当前状态

状态：**D15-S02 已通过；D16-A01—A06 均已开始并保留未完成门禁；D16-S01/S02 未完成。**

D15 的内容门禁已解除：统一 QA 为 `batches=8 samples=24 errors=0 warnings=0`。这不等价于完整版本发布通过；性能、真实存档恢复、全局可访问性、历史/授权闭包、最终表现和发布准备仍需独立验收。

## 阶段状态总表

| 阶段 | 状态 | 证据/说明 |
|---|---|---|
| D8 | PASS | `docs/game/design/common/D8_ACCEPTANCE_REPORT.md` |
| D9 | PASS | `docs/game/design/common/D9_ACCEPTANCE_REPORT.md` |
| D10 | PASS | `docs/game/design/common/D10_ACCEPTANCE_REPORT.md`；standalone AgentCore/Systems/Save.Tests 已复验 0/0 |
| D11 | PASS | `docs/game/design/common/D11_ACCEPTANCE_REPORT.md` |
| D12 | PASS | `docs/game/design/common/D12_ACCEPTANCE_REPORT.md` |
| D13 | PASS | `docs/game/design/common/D13_ACCEPTANCE_REPORT.md` |
| D14 | PASS | `docs/game/design/common/D14_ACCEPTANCE_REPORT.md` |
| D15-S02 | PASS | `docs/game/design/common/D15_BATCH_CONTENT_QA_REPORT.md`；0 errors / 0 warnings |
| D16-A01 | IN_PROGRESS | 当前 Unity 6 图形 PlayMode 全量 54/54；D3D11 RTX 4070 Ti 60 秒窗口 235 samples/平均 65.05 FPS、最低 3.56 FPS、最长全帧 280.86ms，GPU 帧时间 unavailable；D2/D5 启动六阶段归因均完整，目标/低端设备、真实大场景、最终资产 LOD、GPU/内存和长线仍待验收 |
| D16-A02 | IN_PROGRESS | Windows 强制终止 3/3、档案矩阵 10/10、100 次长线压力、多条结算与 SQLite 恢复已留证；Save.Tests 当前 304/304；物理断电、跨平台和完整生产矩阵仍待验收 |
| D16-A03 | IN_PROGRESS | 当前 Unity 6 PlayMode 全量 54/54，覆盖输入、暂停、镜头/上下船、菜单焦点、无障碍和字体代码回归；真机输入、目标设备/分辨率和人工视觉仍待验收 |
| D16-A04 | IN_PROGRESS | 结构错误 0、权利记录闭合；267 条 Pending 仍需人工历史/敏感内容复核 |
| D16-A05 | IN_PROGRESS | 完整 `game` 根目录 2918 条可视登记、654 integrated、2264 条 placeholder；最终资产仍阻断 |
| D16-A06 | IN_PROGRESS | 19 项本机/安装器检查、Windows 构建和正式便携安装器正/负向烟测通过；审计已完整列出 7 条项目级发布阻断，目标机器、原生崩溃/隐私审查和 Steam 政策仍未闭合 |
| D16-S01 | BLOCKED / NOT STARTED | 前置门禁记录已建立；六个 D16-A 门禁全部真实 PASS 后才能执行连续长线全版本验收；主报告见 `D16_FULL_VERSION_ACCEPTANCE_REPORT.md` |
| D16-S02 | BLOCKED BY S01 | 需要 D16-S01 主报告、交接和全量 DoD 证据 |

## 2026-08-11 Unity 6 运行时实测更新

已用 Unity `6000.0.80f1` 正式 PlayMode Test Runner 执行 `D16.PlayMode.Tests`：`InitialHarborBootstrapsD16AndSamplesRuntime` **1/1 PASS**。证据见 `D16_RUNTIME_ACCEPTANCE_REPORT.md`、`tools/results/d16_runtime_playmode_test_results_retry8.xml` 和 `tools/results/d16_runtime_playmode_report.json`。

本次只关闭“运行时基础接线”这一子门禁，不改变 A01—A03 的总体状态：无图形设备的批处理采样不能替代目标设备 CPU/GPU 性能；A02 仍缺断电/进程中止与全档案矩阵；A03 仍缺真实手柄、字幕、字体/色觉/低刺激场景验收。Unity EditMode 全量回归为 `291/291 PASS`。

## 2026-08-11 A01 流式/LOD/对象池运行时更新

最新 Unity 6 PlayMode 生产回归为 `D16.PlayMode.Tests` **2/2 PASS**：基础 InitialHarbor 烟测 + A01 运行时夹具。InitialHarbor 报告现为 `LoadedCellCount=2`（海面、沙滩表现流式单元）、`RegistrationCount=3`、`SampleCount=3`、`BudgetSatisfied=true`。夹具报告确认近处加载、远处卸载、LOD 降级/恢复、碰撞保持启用和对象池复用均通过。

这只关闭了 A01 的“运行时表现闭环”子门禁；当前测试仍为 `Null Device`，GPU 计时不可用，目标设备性能、最终资产 LOD、音频资源卸载、生态数量上限和长线稳定性仍未完成，因此 D16-A01 保持 **IN_PROGRESS**。

## 2026-08-11 A02 存档故障注入与矩阵更新

D16-A02 已取得 Unity 6 EditMode `291/291 PASS`、独立存档安全 `5/5 PASS`、进程中止演练 `3/3 PASS` 和档案矩阵 `10/10` 类别覆盖证据。进程中止演练证明原子文件写入与 SQLite 迁移在指定中断点后可恢复，但不等同物理断电、强制终止、跨平台文件系统和长线存档压力，因此 A02 仍为 **IN_PROGRESS**。

证据：`tools/results/d16_a02_fault_injection_result.json`、`tools/results/d16_a02_archive_matrix_result.json`、`tools/results/d16_a02_editmode_unity6.log`、`game/D2_TestResults.xml`。

## 后续执行顺序

```text
D15-S02 PASS
  ↓
D16-A01 → D16-A02 → D16-A03 → D16-A04 → D16-A05 → D16-A06
  ↓
D16-S01 连续长线验收
  ↓
D16-S02 最终文档、交接与发布决策
```

每个 D16-A 任务必须独立留下：实现/检查结果、测试命令、失败样本、报告、handoff 和当日记忆。任何未取得真实证据的能力保持 `PENDING/BLOCKED`，不能因为设计存在或目录字段齐全而标记 `DONE`。

## D16 各门禁验收目标

### D16-A01 性能与流式

建立可重复的 CPU/GPU/内存/GC/加载时间基线；验证分区流式、LOD、对象池、远景建筑、动物活跃上限、Agent 模拟等级、音频卸载和低端画质降级。至少保留代表场景、采样方法、阈值和实测结果。

### D16-A02 存档与迁移

运行 `Save.Tests` 和迁移矩阵，覆盖旧版本、重复迁移、部分损坏、未知字段、临时文件提交、文件级备份、断电恢复和重复结算保护。禁止把 JSON round-trip 当成真实断电恢复。

### D16-A03 本地化与可访问性

验证全量 zh-CN/en-US key、字体 fallback、字幕/文本缩放、色盲与低刺激选项、输入重绑定、暂停/退出/菜单焦点和键盘/手柄路径；不能只检查静态字典。

### D16-A04 历史、敏感内容与授权

完成 1675 时间线、势力、宗教、社群、奴役/强迫劳动、捕鲸、珍稀动物、赌场、土地和身份内容的证据矩阵；每个 ApprovedWithWarning 必须有 warning/skip 证据，不能伪造外部授权或历史结论。

### D16-A05 最终表现

按场景抽样替换并验收船体、港口、岛屿、植被、水体、天气、角色、动物、宠物、服装、动画、音频和 UI。必须同时验证材质、阴影、碰撞、锚点、相机、可读性和画质降级；placeholder 状态要显式保留。

### D16-A06 发布技术准备

验证 Unity 构建配置、安装/卸载、覆盖更新、隐私与崩溃处理、离线模式、文件级存档恢复、发布 manifest 和商店元数据。未执行真实发布或 Steam 上传前，不标记发布完成。

## 相关证据

- D15 主报告：`docs/game/design/common/D15_BATCH_CONTENT_QA_REPORT.md`
- D15 交接：`docs/game/design/common/handoffs/D15/D15-S02_HANDOFF.md`
- D15 QA JSON：`tools/results/d15_s02_content_qa_result.json`
- D16-A01—A06 报告：`docs/game/design/common/D16_*.md`
- D16-A01—A06 交接：`docs/game/design/common/handoffs/D16/`
- 最终需求事实源：`docs/game/REQUIREMENTS.md`

## 2026-08-11 A03 最新证据

- `tools/d16_a03_runner`：6/6 纯逻辑测试通过；Systems、Save.Tests、Assembly-CSharp 和 runner 构建 0 warning / 0 error。
- Unity 6 `D16.PlayMode.Tests`：3/3 PASS，包含主菜单设置入口和无障碍模态面板打开/关闭链；证据 `tools/results/d16_a03_playmode_test_results.xml`。
- Unity 6 EditMode：292/292 PASS；证据 `tools/results/d16_a03_editmode_unity6.log`。
- A03 仍保持 IN_PROGRESS：真实手柄与目标设备、所有音频 cue 的字幕触发、字体覆盖/长文本、色觉/低刺激场景审查和多场景焦点仍未完成。

## 2026-08-11 A01/A03/A04—A06 最新证据

- A01 本机 RTX 4070 Ti 持续窗口：约 8 秒、32 samples、平均 59.98 FPS、最低 59.72 FPS、预算满足；GPU 毫秒不可用，不能替代目标设备门禁。
- A03 已通过代表性 D9 事件→音频→字幕链；Unity PlayMode 5/5 和纯逻辑 6/6 通过，但 14 个事件逐条触发与真实设备仍待抽样。
- A04/A05/A06 审计已重跑，结果分别保留 488 条结构错误、2262 条 placeholder + 2 条测试字段缺口；阻断状态发布清单已落地，但 Unity 构建/安装更新仍未实测；D16 总体仍 IN_PROGRESS。

最终 EditMode 全量回归 `game/D2_TestResults.xml` 为 **292/292 PASS**，日志 `tools/results/d16_final_editmode_unity6.log`；这只证明当前代码没有回归，不改变 A01—A06 和 S01/S02 的未关闭门禁。

## 2026-08-11 A02 最新证据

- Windows 真实强制终止故障注入：`tools/results/d16_a02_fault_injection_result.json` 的 `real_force_kill_passed=true`，三个边界均在 ready 标记后由父进程终止；受控退出对照仍为 3/3。
- Unity 6 EditMode 最新结果：`tools/results/d16_a02_long_run_editmode_unity6.xml`，296/296 PASS。
- 档案矩阵已基于最新 XML 重跑：`tools/results/d16_a02_archive_matrix_result.json`，10/10 类别、0 failed/inconclusive。
- A02 仍不标记 PASS：物理断电、跨平台恢复、长线存档压力、重复结算压力和 D16-S01 完整链路尚未完成。

## 2026-08-11 A02 长线压力最新证据

- 修复 `SaveSystem.NewGame` 遗留空 v3 投影表导致后续新档迁移失败的问题；现在覆盖旧档会删除 `save_metadata`/`v3_entities` 表后重新迁移。
- `tools/results/d16_a02_long_run_result.json`：100 次连续新游戏→保存→读取→迁移→重复迁移→恢复通过，最终 SQLite 完整、玩家事实正确、备份数 399。
- A02 仍保持 IN_PROGRESS：该压力循环不是物理断电、跨平台或 D16-S01 全游戏长线验收。

- 修复后 Unity 6 `D16.PlayMode.Tests` 最新结果 `tools/results/d16_a02_long_run_playmode_unity6.xml` 为 5/5 PASS；这只是运行时回归证据，不关闭 A02 的物理断电/跨平台门禁。

## 2026-08-11 A01 本机长时窗口最新证据

- A01 连续采样窗口已从 8 秒提升为 60 秒；`tools/results/d16_a01_performance_window_report.json` 记录 60.005 秒、234 samples、平均 59.96 FPS、最低 54.88 FPS、预算满足、保护对象超预算 0。
- Unity 6 `D16.PlayMode.Tests` 最新结果 `tools/results/d16_a01_long_window_playmode_unity6.xml` 为 **5/5 PASS**；完整日志为 `tools/results/d16_a01_long_window_playmode_unity6.log`。
- 这只证明本机 RTX 4070 Ti 上的连续性能采样链和当前预算在 60 秒窗口内稳定；GPU 毫秒、目标/低端设备、真实大场景与最终资产规模、音频释放及 D16-S01 仍未验收，A01 保持 IN_PROGRESS。

## 2026-08-11 A03 14 cue 全量抽样最新证据

- `tools/results/d16_a03_d9_cue_coverage_report.json`：14 个冻结 D9 事件逐条追加并通过 EventLog→音频 cue→字幕链，14/14 有预期 cue、可见字幕和本地化文本。
- Unity 6 `D16.PlayMode.Tests` 最新结果 `tools/results/d16_a03_full_cue_playmode_unity6_retry2.xml` 为 **6/6 PASS**；日志 `tools/results/d16_a03_full_cue_playmode_unity6_retry2.log`。
- A03 的 14 cue 运行时抽样子门禁已关闭；真实物理手柄/目标设备、字体覆盖与长文本、色觉/低刺激、多场景焦点和长线稳定性仍未验收，A03 保持 IN_PROGRESS。

## 2026-08-11 A05 登记元数据最新证据

- `tools/results/d16_a05_asset_audit.json` 最新结果：2590 条可视登记、328 integrated、2262 placeholder、非法状态 0、integrated 缺 LOD/测试字段 **0**。
- 为两个 integrated D15 占位登记补齐了 `test_cases`；审计单测 `python -m unittest tools.tests.test_d16_a04_a06_audits -v` 为 **4/4 PASS**。
- A05 仍保持 IN_PROGRESS：2262 个 placeholder、最终资产替换、材质/动画/碰撞/锚点/相机、低画质和音频/授权闭包仍未完成。

## 2026-08-11 A04 结构与授权最新证据

- A04 审计最新 `tools/results/d16_a04_review_audit.json`：2812 条记录、2590/2590 可视登记授权闭合、结构错误 0、Pending 历史审查 112、Rejected 0。
- 新增批处理工具只把缺字段条目标为 `Pending`，沿用已有 `source_refs` 作为候选复核引用；未修改历史标签为批准、未删除条目、未把项目 license 当外部授权。
- A04 仍保持 IN_PROGRESS：112 条 Pending 需要人工历史/敏感表现复核；机器结构与授权子门禁已关闭。

## 2026-08-11 A06 Windows 构建最新证据

- Unity 6 StandaloneWindows64 构建已成功：`tools/results/d16_a06_windows_build_unity6.log`，`Build Finished, Result: Success`，退出码 0；产物目录 `artifacts/d16_a06_windows_build/`。
- 构建后的 Windows Player 15 秒 `-batchmode -nographics` 启动烟测通过：主菜单 EventSystem/UI/D16 设置日志存在且无 Error/Exception/Crash/No cameras；日志 `tools/results/d16_a06_windows_player_smoke.log`。
- A06 仍保持 IN_PROGRESS：安装/卸载/覆盖更新、文件级备份恢复、隐私/崩溃正式验证和 Steam 上传授权未完成。

- A06 最新审计 `tools/results/d16_a06_release_audit.json` 已识别本地 Windows 构建证据（`unity_windows_build_evidence=true`）；剩余静态阻断为安装/卸载/覆盖更新未实测和 Steam 未授权，审计单测 6/6 PASS。

- 工作区 staging 安装→覆盖更新→卸载烟测已通过：`tools/results/d16_a06_install_update_smoke_result.json`，安装/更新各 180 文件，用户存档哨兵在更新和卸载后均保留；当前只剩正式 installer/update 流程缺失与 Steam 未授权。

## 2026-08-11 全量复核补充

- 最终 Unity EditMode 回归 `tools/results/d16_final_editmode_unity6.xml`：296/296 PASS；工具测试全集 44/44 PASS。
- A04 已把 D15-A06 源目录副本纳入审计留痕：全项目 3139 条记录、2916/2916 可视资源权利闭合、结构错误 0、Pending 278。Pending 仅代表待人工核验，不能当作批准。
- D16 当前仍不可发布：A01 目标/低端设备与真实大场景性能、A02 物理断电/跨平台、A03 真机与字体/色觉、A04 人工历史审查、A05 2262 个占位资产、A06 正式 installer/update 与 Steam 授权仍未关闭。

## 2026-08-11 D16-A01—A06 当前复验快照

- D16-A02：Save.Tests、runner 0/0；安全 runner 5/5；故障注入 6/6；100 次长线存档循环通过。物理断电、跨平台和完整 S01 仍未通过。
- D16-A01：高负载 PlayMode `7/7 PASS`；267 候选下 Agent 32/32、动物 64/64、音频 24/24，保护对象不冻结；目标/低端设备、真实大场景和最终资产性能仍未验收。
- D16-A03：纯逻辑 6/6，14 cue 抽样证据保留；真实输入设备、字体/长文本、色觉/低刺激、多场景焦点和长线仍未完成。
- D16-A04：`tools/results/d16_a04_review_audit.json` 为 3139 条记录、2916/2916 权利闭合、结构错误 0、Pending 278；只剩人工历史/敏感内容复核。
- D16-A05：`tools/results/d16_a05_asset_audit.json` 为 2590 可视登记、328 integrated、2262 placeholder、元数据错误 0；未把占位资源标为最终。
- D16-A06：`tools/results/d16_a06_release_audit.json` 在 `game` 项目根目录下静态证据完整；workspace staging 与 `d16_a06_install_update.ps1` 三步流程 PASS；签名 OS 安装包/注册表权限集成未验收，Steam 未授权/未执行。

本快照不改变 D16-S01 未开始和 D16-S02 BLOCKED 的事实，也不构成发布或 Steam 上传授权。

## 2026-08-12 A03 输入意图子门禁最新证据

- 新增 `game/Assets/Tests/PlayMode/D16InputRouterPlayModeTests.cs`，使用 Unity Input System Test Framework 实测合成键盘、手柄事件到统一意图层，以及模态输入阻断；未改变生产 Systems 的数据所有权。
- focused Unity 6 PlayMode `tools/results/d16_a03_input_router_single_unity6.xml`：**1/1 PASS**。
- D16 Unity 6 PlayMode 全量 `tools/results/d16_a03_input_router_full_playmode_unity6.xml`：**12/12 PASS**，日志 `tools/results/d16_a03_input_router_full_playmode_unity6.log`。
- A03 仍为 **IN_PROGRESS**：真实物理设备/目标设备、字体与长文本、色觉/低刺激、多场景焦点和长线稳定性仍缺独立证据；该子门禁不能推动 D16-S01/S02 开始。

## 2026-08-12 A03 暂停与 Esc 集成最新证据

- `D16A03PauseMenuKeepsFocusAcrossAccessibilityAndDiagnostics` 通过 InitialHarbor 暂停→无障碍/隐私模态→关闭→继续游戏链，验证 EventSystem 焦点、玩法输入阻断、暂停状态和时间流速恢复。
- `D16A03SyntheticEscapeTogglesHarborPauseThroughController` 通过 Unity Input System 合成键盘 Escape，实际触发暂停控制器的暂停/恢复；focused 结果 `tools/results/d16_a03_escape_pause_single_unity6.xml` 为 **1/1 PASS**。
- 加入上述集成回归后的 D16 PlayMode `tools/results/d16_a03_escape_pause_full_d16_playmode_unity6.xml` 为 **14/14 PASS**，日志 `tools/results/d16_a03_escape_pause_full_d16_playmode_unity6.log`。
- 这只关闭编辑器内 Esc 和暂停焦点子门禁；A03 仍为 **IN_PROGRESS**，D16-S01/S02 仍未开始/被整体门禁阻塞。

## 2026-08-12 A05 船体占位 LOD 元数据回归修复

- 发现并修复 `game/Assets/Content/D15/A06/entries/assets/asset_ph_ship_fishing.json` 源目录副本缺少 `lod_profile` 的真实回归；补齐 `source/generated` 四级声明，与 Resources 注册表保持一致。
- D16-A05 最新审计：2918 条可视登记、654 integrated、2264 placeholder、125 data_only、非法元数据 0、集成资源缺 LOD/测试字段 0；唯一阻断为 `placeholder_assets_remain`。
- D15-A06 catalog PASS；D16-A04/A05/A06 专项审计测试 **12/12 PASS**。该修复不把占位模型标为最终资产，D16-A05 仍为 **IN_PROGRESS**。

## 2026-08-12 A04 队列化更新

- 新增 `tools/d16_a04_review_queue.py`，将完整 `game` 目录的 278 条 Pending 记录按责任范围整理为机器可读与 Markdown 队列。
- `tools/results/d16_a04_review_queue.json`：`status=BLOCKED`、`pending_count=278`，其中 `asset_history_and_authorization=187`、`historical_content=91`。
- `tools/results/d16_a04_review_queue.md`：逐条保留路径、来源/证据引用、资产状态和人工决定项。
- 队列工具单测 `python -m unittest tools.tests.test_d16_a04_a06_audits -v` 为 **10/10 PASS**；全量工具回归 `python -m unittest discover -s tools/tests -p 'test_*.py'` 为 **46/46 PASS**；它不修改 Registry、不批准记录、不改变 D16-A04 的阻断结论。

D16-A04 仍为 **IN_PROGRESS**，D16-S01/S02 仍未开始/被阻塞。

## 2026-08-12 A04 安全虚构子门禁更新

- `tools/d16_a04_approve_explicit_fictional.py` 在严格预览断言后应用 11 条明确虚构、项目自产记录；其余内容未被批量批准。
- `tools/results/d16_a04_fictional_gate_apply.json`：`changed_count=11`；记录附带 `review_decision_basis.not_a_historical_fact_claim=true`。
- 最新 A04 审计：`Approved=2780`、`ApprovedWithWarning=91`、`Pending=267`、`structural_error_count=0`、`Rejected=0`；唯一阻断仍为 `pending_historical_reviews`。
- Content Registry `2812 entries` PASS，D15-S02 `8 batches / 24 samples / 0 errors / 0 warnings` PASS。

该更新只关闭明确虚构内容的范围审查，不改变 A04 总体状态，也不允许进入 D16-S01。

## 2026-08-12 A04/A05 元数据闭包与全量工具回归

- `asset_ph_crew_male`、`asset_ph_palm_leaves` 已补齐真实源路径、运行时 Prefab 路径、占位状态、LOD/测试字段和独立 Quaternius CC0 authorization；没有改变资源为最终资产。
- D16-A04 最新审计：3139 条记录、2918/2918 可视资源权利闭合、结构错误 0、Pending 267；唯一阻断为人工历史/敏感表现复核。
- D16-A05 最新审计：2592 可视登记、328 integrated、2264 placeholder、非法状态 0、集成资源缺 LOD/测试字段 0；唯一阻断为占位资源仍在。
- 注册表 2812 条 PASS，D15-S02 为 8 batches / 24 samples / 0 errors / 0 warnings PASS；全量工具回归 `python -m unittest discover -s tools/tests -p 'test_*.py' -v` 为 **48/48 PASS**。
- D16-A04/A05/A06 专项测试现为 **12/12 PASS**；D16-S01/S02 仍未开始，不能据此宣称 D16 完成或可发布。

## 2026-08-12 A01/A02/D15 当前复验快照

- A01 当前本机 60 秒窗口：233 samples、平均 59.95 FPS、最低 57.09 FPS、预算满足、保护对象超预算 0；GPU 计时不可用，目标/低端设备和真实大场景仍缺证据。
- A02 当前 Save.Tests **293/293 PASS**；故障注入受控 3/3、Windows 强制终止 3/3、档案矩阵 10/10、100 次存档循环通过；物理断电和跨平台仍缺证据。
- D15-S02 当前重新复验仍为 8 batches / 24 samples / 0 errors / 0 warnings PASS；全量工具测试 48/48 PASS。
- A05 的 `asset_ph_ship_fishing` 源目录 LOD 缺口已修复；A05 当前只剩 2264 placeholder 阻断，不能进入最终表现 PASS。

## 2026-08-12 A06 本地隐私诊断子门禁更新

- 新增 `game/Assets/Scripts/Gameplay/D16/D16DiagnosticRecord.cs` 与 `D16LocalDiagnostics.cs`：运行时诊断默认关闭，只有显式同意后才记录 Error/Exception/Assert；记录写入独立的 `Application.persistentDataPath/D16Diagnostics`，不读取存档、不联网、不上传。
- 记录只保留时间、严重级别、错误类别、场景、Unity 版本、平台和长度/行数统计；原始错误文本与堆栈字段固定省略，并提供 `D16LocalDiagnostics.RevokeConsentAndClear` 删除入口和 512 KiB 轮转上限。
- Unity 6 PlayMode `D16LocalDiagnosticsTests` 为 **3/3 PASS**；`Assembly-CSharp` 与 Editor 程序集构建均 0 warning / 0 error。
- 新增诊断后的完整 D16 PlayMode 回归为 **10/10 PASS**，结果 `tools/results/d16_post_diagnostics_playmode_unity6.xml`；全量工具回归为 **48/48 PASS**。
- `tools/d16_a06_release_audit.py game` 当前所有静态检查为 true，唯一阻断仍是 `steam_upload_not_authorized`；发布清单保持 `release_status=blocked`，未执行 Steam 或外部上传。
- 该子门禁不等于完整隐私验收：设置界面/用户说明、原生崩溃转储策略、目标设备实测和最终人工隐私审查仍待完成；D16-A06 与 D16-S01/S02 继续保持未完成。

## 2026-08-12 A02 存档路径与 sidecar 子门禁更新

- Save/Storage 新增中文与空格路径、主数据库、`-wal`、`-shm` sidecar 备份恢复测试；独立 D16-A02 runner **6/6 PASS**，Save.Tests 与 runner 构建均 0 warning / 0 error。
- Unity 6 EditMode `Save.Tests` 全量 **293/293 PASS**，结果 `tools/results/d16_a02_save_tests_editmode_unity6.xml`。
## 2026-08-12 A06 用户可见隐私入口子门禁更新

- 新增独立高层模态 `D16LocalDiagnosticsPanel`，从主菜单设置和 InitialHarbor 暂停菜单均可进入；用户可看到默认关闭、脱敏字段、不联网/不上传、文件轮转范围和删除边界。
- `ConsentToggle`、`DeleteAndDisable`、`Close` 的真实点击链已通过；面板打开只阻断玩法输入，未新增 EventSystem 竞争，也不遮挡已有面板的层级之外区域。
- Unity 6 PlayMode 单项 **1/1 PASS**：`tools/results/d16_a06_privacy_panel_playmode_unity6.xml`；D16 PlayMode 全量 **11/11 PASS**：`tools/results/d16_a06_privacy_full_d16_playmode_unity6.xml`。
- `Assembly-CSharp` 构建 **0 warning / 0 error**；A04/A05/A06 专项审计 **12/12 PASS**；A06 静态审计 `missing_static_evidence=[]`。
- 该子门禁不关闭 D16-A06 总体：原生崩溃转储、目标设备、正式 OS installer/update、人工隐私审查和 Steam 授权仍未完成；`D16ReleaseManifest.json` 继续为 `blocked`，D16-S01/S02 仍未开始。
- 故障注入受控边界 3/3、Windows `Process.kill()` 边界 3/3，100 次新游戏→保存→迁移→重复迁移→恢复循环通过。
- 该证据只覆盖当前 Windows/.NET 的路径与 sidecar 语义；物理断电、Linux/macOS 实机、跨平台恢复和 D16-S01 仍未完成，D16-A02 继续 **IN_PROGRESS**。

## 2026-08-12 A02 当前存档证据复验

- Unity 6 `Save.Tests` 全量 `tools/results/d16_a02_save_tests_current_editmode_unity6.xml`：**293/293 PASS**。
- 故障注入当前复验：受控边界 3/3、Windows `Process.kill()` 边界 3/3、`passed=true`。
- 当前档案矩阵：292 个 EditMode 用例、D7—D14/损坏档 10/10 类别真实 Passed 覆盖、0 failed/inconclusive。
- 当前 100 次存档压力：SQLite 完整性通过、最终事实/迁移幂等/399 个备份通过。
- 这只强化 D16-A02 的 Windows/.NET 子门禁；物理断电、跨平台文件系统、完整重复结算压力和 D16-S01 仍缺证据，A02 继续 **IN_PROGRESS**。

## 2026-08-12 A06 安装更新烟测复验

- 工作区限定 `tools/d16_a06_install_update_smoke.py` 复验 PASS：安装 180 文件、覆盖更新 180 文件，卸载后用户存档哨兵保留。
- 最新 `tools/results/d16_a06_release_audit.json` 的 `missing_static_evidence=[]`，本地构建与工作区部署烟测均有证据；唯一阻断为 `steam_upload_not_authorized`。
- 不把工作区烟测扩大解释为正式 OS installer、目标设备、原生崩溃或 Steam 发布验收；A06 仍 IN_PROGRESS。

## D16-A03 设置页按钮布局修复复验（2026-08-12）

- 修复 MainMenuController 中设置页诊断/关闭按钮落在参考视口外，造成“看得到但点不到”；同时重新排布设置项，避免菜单内容互相覆盖。
- 新增设置按钮真实命中区域与模态开关 PlayMode 测试：D16A03SettingsButtonsHaveUsableHitTargetsAndOpenRealModals。
- Unity 6 D16.PlayMode.Tests 最新全量 **15/15 PASS**，新增测试 **1/1 PASS**；证据位于 game/tools/results/d16_a03_settings_layout_full_playmode_unity6_final.xml 与 .log。
- 该修复只关闭设置页 UI 命中区域子门禁；A03 及 D16-S01/S02 仍等待真实设备、目标硬件、字体/长文本、色觉/低刺激和长线稳定性证据。

## D16-A03 主菜单面板互斥复验（2026-08-12）

- 新增主菜单面板互斥回归，依次检查设置、新游戏、关于和返回流程，确保 MainMenu、NewGamePanel、SettingsPanel、AboutPanel 不会同时显示。
- 最新 Unity 6 D16.PlayMode.Tests 为 **16/16 PASS**；设置按钮命中区域和面板互斥测试均 **1/1 PASS**。
- 证据：game/tools/results/d16_a03_menu_regression_full_playmode_unity6.xml；日志：tools/results/d16_a03_menu_regression_full_playmode_unity6.log。
- 该子门禁已关闭；D16-A03 总体和 D16-S01/S02 仍等待真实设备、目标硬件、字体/长文本、色觉/低刺激及长线稳定性证据。

## 2026-08-12 D16-R02 安装更新安全回归

- `tools/d16_a06_install_update.ps1` 已增加更新失败回滚和已有安装拒绝覆盖保护；独立用户数据目录继续不参与卸载。
- PowerShell 合同测试 **1/1 PASS**；D16-A04/A06 专项测试 **13/13 PASS**；全量工具测试 **49/49 PASS**。
- 发布审计已把工作区部署脚本与正式 Windows 安装器分开：前者存在且 staging 烟测通过，后者当前不存在且没有正式安装器烟测证据。
- D16-R02 仅完成本地部署安全子门禁；正式 MSI/MSIX/签名 Setup、目标机器安装/权限/卸载和 Steam 仍未执行。A06、D16-S01、D16-S02 保持未完成。

## 2026-08-12 A06 发布清单运行契约校验

- 发布审计已覆盖产品身份、Unity/目标平台、D15 内容 QA 状态、locale、离线模式、存档兼容/备份策略和安装状态字段；故意破坏 `offline_mode` 的负例测试通过拦截。
- A06 专项测试 **14/14 PASS**；工具全量测试 **50/50 PASS**；真实清单运行契约为 PASS。
- 这只关闭 A06 的配置校验子门禁；正式 OS installer、目标设备、原生崩溃、人工审查和 Steam 授权仍未完成，A06 继续 IN_PROGRESS。

## 2026-08-12 Agent 与离线代码级回归

- TemplateFallback 已固定为显式 schema v1.1 no-op 提议，模型失败不会改变 CrewStats；Unity fallback 证据 **295/295 PASS**。
- D5 默认离线桥接、纯本地云队列和关闭云功能边界已回归；最新 Unity `Save.Tests` **297/297 PASS**，D5 相关 **10/10 PASS**。
- 四个 C# 工程串行编译均为 0 warning / 0 error；D10 CrewAgent 状态与 AgentCore/Unity 镜像同步问题已修复。
- 这些只是 A06 代码级子门禁；真实断网重启、目标设备、跨平台、正式 OS installer、人工审查和 Steam 授权仍缺证据，A06、D16-S01/S02 保持未完成。

## 2026-08-12 当前验收快照

本节是当前工作区状态，早期章节保留为历史审计记录，不再代表最新数值。

| 阶段 | 当前状态 | 当前证据与剩余阻断 |
|---|---|---|
| D15-S02 | PASS | `tools/results/d15_s02_content_qa_result.json`：8 batches、24 samples、0 errors、0 warnings |
| Save.Tests | PASS | 当前 Unity Save.Tests 运行结果与 AgentCore/Systems/Assembly-CSharp 构建均已复验；详见 D10/A02 交接 |
| D16-A01 | IN_PROGRESS | 图形模式 Unity PlayMode 最新 23/23 PASS；流式音频作用域接线单项 1/1 PASS；纯预算/事实保护 6/6；RTX 4070 Ti 60 秒窗口 234 samples，目标/低端设备、真实大场景、最终资产 LOD、最终音频二进制和跨平台长线仍缺 |
| D16-A02 | IN_PROGRESS | Windows 强制终止、档案矩阵 10/10、100 次存档压力和多条结算幂等 7/7 通过；当前 Unity Save.Tests 302/302 PASS；物理断电、Linux/macOS 实机和完整跨平台恢复仍缺 |
| D16-A03 | IN_PROGRESS | 编辑器输入/暂停/本地化表/菜单链、数字/长文本/字体/视觉辅助/低刺激、敏感内容警告/继续/跳过、D9 领域数字读模型、金融只读页面和键鼠/手柄重绑定回归通过（D16 PlayMode 37/37；含 D17 低模人物为 38/38）；真机输入、目标设备、目标分辨率截图、色觉/低刺激人工复核和长线稳定性仍缺 |
| D16-A04 | IN_PROGRESS | 3139 条记录、结构错误 0、可视授权记录 2918/2918；267 条 Pending 仍需人工历史/敏感内容复核 |
| D16-A05 | IN_PROGRESS | 2918 条可视登记、654 integrated、2264 placeholder；真实最终资产替换仍缺 |
| D16-A06 | IN_PROGRESS | 19 项本机/安装器检查、Windows 构建、正式便携安装器正/负向烟测通过；审计记录 7 条项目级阻断，目标机器/原生崩溃/最终隐私审查和 Steam 政策仍缺 |
| D16-S01 | NOT STARTED | A01—A06 未全部 PASS，禁止启动 |
| D16-S02 | BLOCKED BY S01 | 缺少 D16-S01 主报告和交接报告 |

当前不标记 PRODUCTION/DONE，不执行 Steam 上传或外部发布。

### 2026-08-12 当前增量：D16-A03 视觉辅助运行时回归

- D16-A03 Unity 6 图形 PlayMode 全量更新为 **18/18 PASS**；新增真实 UGUI 长文本/中文字体/字号/高对比度/色觉变换测试，报告 `tools/results/d16_a03_accessibility_runtime_report.json` 全字段通过。
- 该结果仅关闭编辑器运行时代码子门禁；真实手柄/目标设备、目标分辨率截图、色觉/低刺激人工复核、多场景焦点和长线稳定性仍未完成。
- D16-A03 继续 **IN_PROGRESS**；D16-S01/S02 仍未启动。

### 2026-08-12 当前增量：D16-A06 本地诊断文件链

- D16-A06 新增真实 MainMenu 文件链回归：默认关闭不写、同意后只写脱敏 ndjson、撤回删除后主/轮转文件均清除；Unity 6 D16 PlayMode 全量 **19/19 PASS**。
- A06 静态审计仍为 `missing_static_evidence=[]`，正式安装器正/负向烟测与清单状态一致；原生崩溃转储、目标设备矩阵、人工隐私审查和 Steam 授权仍未完成。
- D16-A06 继续 **IN_PROGRESS**；D16-S01/S02 仍未启动。

### 2026-08-12 当前增量：D16-A03 敏感内容警告与跳过

- 新增 `D16SensitiveContentOverlay`，接入现有敏感内容策略与偏好；真实模态按钮支持继续/跳过，Esc/Start 走跳过，且能恢复此前输入阻断状态。
- 新增中英四组 UI 文案及 `D16SensitiveContentOverlayTests`；Unity 6 D16.PlayMode.Tests 全量 **20/20 PASS**。
- 证据：`tools/results/d16_a03_sensitive_warning_playmode_unity6.xml`、`.log`。
- 该增量只关闭编辑器运行时敏感内容警告/继续/跳过代码子门禁；A03 仍缺真机/目标设备、人工视觉与长线证据，D16-S01/S02 仍未启动。

### 2026-08-12 当前增量：D16-A03 低刺激运行时适配

- 低刺激设置已有实际保存、面板开关与运行时颜色消费；新增断言验证真实 UGUI 颜色被低刺激映射。
- Unity 6 D16.PlayMode.Tests 仍为 **20/20 PASS**；`tools/results/d16_a03_accessibility_runtime_report.json` 的 `LowStimulusApplied=true`。
- 证据：`tools/results/d16_a03_low_stimulus_playmode_unity6.xml`、`.log`。
- 该增量只关闭编辑器低刺激代码子门禁；目标设备人工可读性、真机输入、多场景焦点和长线证据仍缺，D16-S01/S02 仍未启动。

### 2026-08-12 当前增量：D16-A03 数字格式运行时 UI

- 新增 `D16A03RuntimeNumberFormattingFeedsUiText`，在真实 MainMenu UGUI `Text` 中消费 `D16DisplayFormatter` 的中英数字、负数和百分比结果。
- Unity 6 D16.PlayMode.Tests 全量更新为 **21/21 PASS**；证据：`tools/results/d16_a03_number_format_playmode_unity6.xml`、`.log`。
- 这只关闭格式化与 UGUI 消费子门禁；债务/重量/时间/价格等领域接入、真实设备和人工视觉证据仍缺，D16-S01/S02 仍未启动。

### 2026-08-12 当前增量：D16-A03 领域数字字段接入

- D9 航行 HUD/读模型、D9 港口整备页和 D3 航程/贸易页现已消费 `D16DisplayFormatter`，覆盖航程日数、金币、库存、重量、吃水、补给、船体、船员、改装和买卖价格；不改变 Systems 的数值所有权。
- 新增 `D16A03D9PresentationReadModelFormatsDomainNumbersBeforeUiConsumption`，Unity 6 D16.PlayMode.Tests 全量 **22/22 PASS**。
- 证据：`tools/results/d16_a03_domain_number_full_playmode_unity6_retry3.xml`、`.log`；Assembly-CSharp 与 Save.Tests 均 0 warning / 0 error。
- 该增量只关闭代码级领域数字表现接入；债务专用界面、真机/目标设备、人工视觉、多场景焦点和长线稳定性仍缺，A03 继续 IN_PROGRESS，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A01 流式音频作用域接线

- `D16WorldStreamCell` 现在会自动发现或接收 `D16AudioResourceScope`；已加载单元切换到卸载时释放本单元音频引用，明确共享资源不会被误调用 `Resources.UnloadAsset`。
- 新增 `D16A01StreamingCellReleasesOwnedAudioScopeWithoutUnloadingSharedClip`，focused Unity 6 PlayMode **1/1 PASS**；D16 PlayMode 全量 **23/23 PASS**。
- 证据：`tools/results/d16_a01_audio_scope_single_unity6_retry3.xml`、`tools/results/d16_a01_audio_scope_full_d16_playmode_unity6_retry2.xml` 及对应日志；Assembly-CSharp 0 warning / 0 error。
- 该增量只关闭代码/编辑器运行时音频生命周期子门禁；真实最终音频文件、目标/低端设备、真实大场景、GPU/内存和跨平台长线证据仍缺，D16-A01 继续 IN_PROGRESS，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A03 金融与债务只读页面

- A03 新增只读 `D16FinanceDebtPanel`，从 InitialHarbor 暂停菜单进入；它只消费 FinanceSystem 快照，不获得金融写权限。
- 页面专项 Unity 6 PlayMode：**1/1 PASS**，`tools/results/d16_a03_finance_debt_single_unity6.xml`。
- 加入页面后的 D16 PlayMode 全量：**24/24 PASS**，`tools/results/d16_a03_finance_debt_full_d16_playmode_unity6.xml`。
- `Assembly-CSharp`、`Assembly-CSharp-Editor`、`Save.Tests`：0 warning / 0 error。
- 该子门禁不改变 A03 总体状态；真实设备、目标分辨率视觉审查、跨场景焦点和长线证据仍缺失，D16-S01/S02/D17 继续保持未启动。

### 2026-08-12 当前增量：D16-A06 后台线程诊断可靠性

- `D16LocalDiagnostics` 已接入后台线程日志回调；仅把分类、长度和行数等脱敏元数据放入有界队列，由主线程分批落盘，并在撤回同意时清空队列。
- D16-A06 专项 **5/5 PASS**：`tools/results/d16_a06_diagnostics_threaded_playmode_unity6.xml`。
- D16 PlayMode 全量 **25/25 PASS**：`tools/results/d16_a06_full_d16_playmode_unity6.xml`。
- Assembly-CSharp、Assembly-CSharp-Editor、Save.Tests、AgentCore 均 0 warning / 0 error；未修改 Systems、contracts、存档或网络逻辑。
- 仍缺原生崩溃/目标机器/跨平台断电/人工隐私证据，A06 继续 IN_PROGRESS；D16-S01/S02/D17 仍未启动。

### 2026-08-12 当前增量：D16-A02 结算幂等跨存档回归

- 新增 Save 测试 `SettlementClaimSurvivesSaveRoundTripAndCannotRepeat`：真实写入 D3 EventLog 快照、清空内存、重新加载后，重复结算仍被拒绝，重复事实追加仍被去重。
- focused Unity 6 EditMode **1/1 PASS**：`tools/results/d16_a02_settlement_roundtrip_editmode_unity6.xml`。
- Unity 6 `Save.Tests` 全量 **298/298 PASS**：`tools/results/d16_a02_save_tests_after_settlement_editmode_unity6.xml`；Save.Tests 编译 0 warning / 0 error。
- 该子门禁不改变 A02 总体状态；物理断电、Linux/macOS、目标设备、全结算压力和 D16-S01 仍缺证据。

### 2026-08-12 当前增量：D16-A03 键位重绑定持久化

- 新增 `D16A03KeyboardRebindPersistsAndRoutesTheChangedIntent`：真实保存 `Interact → Q` 后重新读取，并以 Unity Input System 合成 Q，确认按下/保持状态均进入统一 `Interact` 意图。
- focused PlayMode **1/1 PASS**：`tools/results/d16_a03_keyboard_rebind_single_unity6.xml`；D16 PlayMode 全量更新为 **26/26 PASS**：`tools/results/d16_a03_keyboard_rebind_full_d16_playmode_unity6.xml`，对应日志同名 `.log`。
- 该结果只关闭编辑器内键位持久化与路由子门禁；真实物理手柄、目标设备、人工视觉和长线证据仍缺，D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A03 手柄触发器重绑定

- `D16InputRouter` 新增 `LeftTrigger` / `RightTrigger` 支持，并以每帧边沿采样处理模拟触发器，避免按住时重复触发一次性意图；未修改重复绑定拒绝规则。
- 新增手柄重绑定持久化与统一意图路由回归：focused PlayMode **1/1 PASS**，`tools/results/d16_a03_gamepad_rebind_single_unity6.xml`；D16 PlayMode 全量更新为 **27/27 PASS**，`tools/results/d16_a03_gamepad_rebind_full_d16_playmode_unity6.xml`，对应日志同名 `.log`。
- `Assembly-CSharp`、`Save.Tests`、`AgentCore` 串行构建均 0 warning / 0 error。该结果只关闭编辑器内手柄扳机重绑定代码子门禁；真实物理手柄、目标设备、人工视觉和长线证据仍缺，D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A02 多条结算事实多轮幂等

- 新增 24 条结算事实 × 5 轮真实 Save/Load 往返回归；重复结算声明全部拦截，重复事实追加全部去重。
- 独立 D16-A02 runner **7/7 PASS**；`Save.Tests` 与 runner 构建均 0 warning / 0 error。
- 该证据只补强当前 Windows/.NET 的多条结算幂等子门禁；物理断电、跨平台恢复、目标设备、全 D7—D14 组合和 D16-S01 仍缺，D16-A02 继续 **IN_PROGRESS**。

### 2026-08-12 当前增量：真实 SQLite 损坏档恢复

- `D16SaveSafetyTests.CorruptedLiveDatabaseRestoresLatestValidArchiveAndLoads` 覆盖真实 SQLite 主档损坏、`Load()` 拒绝损坏档、从最新备份原子恢复、保留 rollback 和恢复后重新读档。
- `D16A02Runner` 当前 **9/9 PASS**；`Save.Tests` 与 runner 构建均 0 warning / 0 error。
- Unity 6 `Save.Tests` 本轮 **302/302 PASS**：`tools/results/d16_a02_save_tests_after_sqlite_recovery_editmode_unity6.xml`；档案矩阵以该 XML 重算为 10/10 类别、0 failed/inconclusive。
- 这只关闭 Windows/.NET 本机损坏档恢复子门禁；物理断电、跨平台恢复、目标设备和完整 D7—D14 生产组合仍缺，A02 继续 **IN_PROGRESS**。

### 2026-08-12 当前增量：D16-A04 人工复核队列文件复验

- 重新生成 `tools/results/d16_a04_review_queue.json` 与 `.md`，当前 `status=BLOCKED`、267 条 Pending、12 批；范围为资产/授权与历史复核 186 条、普通历史内容 81 条。
- 队列工具只整理完整 `game` 根目录的来源、证据、警告/跳过字段和人工决定项，不写回 Registry、不自动批准；D16-A04 仍 **IN_PROGRESS**，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A01 性能预算事实保护

- 新增高压力预算评估前后 EventLog 快照不变回归；独立 A01 runner **6/6 PASS**，Save.Tests 与 runner 构建均 0 warning / 0 error。
- 静态场景基线复验通过；该结果只关闭性能预算纯逻辑的数据边界风险，不替代目标/低端设备、真实大场景、最终资产 LOD、GPU/内存、音频释放和跨平台长线证据。

### 2026-08-12 当前增量：D16-A03 核心键位面板全量化

- `D16AccessibilityPanel` 已从 9 个可重绑定动作扩展到契约已有的 16 个动作，新增地图、贸易、角色预设、语言、画质、成就和保存状态的中英文标签。
- 16 行键盘/手柄按钮已放入 `ScrollRect` 视口，底部状态提示、恢复默认和关闭按钮保留在参考卡片的可命中区域；新增 PlayMode 结构测试覆盖视口尺寸、16×2 按钮和新增 Map 监听入口。
- `Assembly-CSharp`、`D16.PlayMode.Tests`、A03 standalone runner 构建均 0 warning / 0 error；A03 standalone **6/6 PASS**。
- Unity 6 专项因本机 `Licensing::Module` 未完成而未生成 XML，不能计作运行时通过；许可证恢复后需重跑专项并做滚轮、键盘导航、物理手柄和目标分辨率人工复核。A03 仍 IN_PROGRESS，D16-S01/S02/D17 不启动。

### 2026-08-12 当前增量：D16-A03 模态输入阻断状态恢复

- `D16AccessibilityPanel` 记录打开前的 `D16InputRouter.GameplayInputBlocked` 状态；关闭/销毁时恢复，重复 `ShowPanel` 不重复捕获状态，避免内层模态关闭后错误解锁外层玩法输入。
- 新增 `D16A03AccessibilityPanelRestoresOuterInputBlockState`，覆盖 MainMenu 中外层阻断→打开无障碍→关闭无障碍的真实场景链路。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 0 warning / 0 error；Unity 专项受 `Licensing::Module` 阻断，未生成 XML，不能计为 PASS。A03 仍 IN_PROGRESS，D16-S01/S02/D17 不启动。

- 同一修复将 D16 模态阻断接入 `D7ControlCoordinator.GameplayInputAllowed` / `CameraInputAllowed`，旧 D3-D10 快捷键和镜头不再穿透无障碍、诊断、敏感或金融模态；D7 ESC 在锁定期间让出给顶层模态。
- 该桥接只解决模态权限穿透，不把尚未迁移的 D3-D10 旧快捷键宣称为全部可重绑定输入；A03 总体仍 IN_PROGRESS。

### 2026-08-12 当前增量：D16-A03 上下文输入契约扩展

- `D16InputAction` 已登记 D3/D4/D5/D9/D10 的上下文动作，`D16InputBinding.Scope` 允许不同页面合法复用物理键，同时拒绝同一作用域内重复键。
- 默认绑定保留现有键位，`Save` 使用 F5 主键并兼容 C 副键；新增纯逻辑回归 `ContextBindingsMayReuseKeysButSameScopeCannot`。
- A03 standalone runner **7/7 PASS**；`Assembly-CSharp`、`Save.Tests`、`D16.PlayMode.Tests` 和 runner 构建均 0 warning / 0 error。
- 旧 D3-D10 Gameplay 入口尚未逐个迁移，UI 全量动作标签和 Unity/真机证据仍待完成；A03 继续 IN_PROGRESS，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A03 D3 输入迁移

- `D3RuntimeBootstrap` 已将贸易、航程、采集、登陆结算、保存和贸易关闭迁移到 `D16InputRouter`；D3 源码不再直接读取 `Keyboard.current`。
- 新增 `D16A03D3ContextBindingPersistsAndRoutesTheChangedIntent`，覆盖 D3 上下文动作保存 Q 重绑定后的意图路由；C# 构建 0 warning / 0 error。
- Unity focused PlayMode 尚因 `Licensing::Module` 阻断未执行，不计为运行时 PASS；D4/D5/D9/D10 仍待迁移，A03 继续 IN_PROGRESS。

### 2026-08-12 当前增量：D16-A03 D4 输入迁移

- `D4RuntimeBootstrap` 已将势力、炮战、装填、弹种、开炮、接舷、脱离、改装、动物、医疗、主线和保存迁移到 D16 上下文意图，源码无 `Keyboard.current` 直读。
- 新增 `D16A03D4ContextBindingPersistsAndRoutesTheChangedIntent`；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。
- Unity focused PlayMode 尚未执行（许可证阻断），不计为 PASS；D5/D9/D10 仍待迁移，A03 继续 IN_PROGRESS。

### 2026-08-12 当前增量：D16-A03 D5 输入迁移

- `D5RuntimeBootstrap` 已将发现岛屿、访问岛屿、角色预设、语言、画质、成就和保存迁移到 D16 意图，源码无 `Keyboard.current` 直读。
- 新增 `D16A03D5ContextBindingPersistsAndRoutesTheChangedIntent`；三工程构建均 0 warning / 0 error。
- Unity focused PlayMode 尚未执行（许可证阻断），不计为 PASS；D9/D10 仍待迁移，A03 继续 IN_PROGRESS。

### 2026-08-12 当前增量：D16-A03 D9/D10 输入迁移

- D9 港口整备入口已将打开、独立关闭、靠泊、物资购买、酒馆入口、修船、招募、发薪、升级和离港接入 D16 上下文意图；源码直读扫描无匹配。
- D10 酒馆社会入口已将打开、独立关闭、悬赏板、安全工作、休息、饮食、社交、招募和敏感内容跳过接入 D16 上下文意图；源码直读扫描无匹配。
- D9/D10 上下文重绑定 PlayMode 测试已补齐；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error。
- D3/D4/D5/D9/D10 代码级迁移完成，但 UI 全量动作入口、本地化标签、Unity/真机证据仍待完成；D16-A03 继续 IN_PROGRESS，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A03 上下文动作设置面板全量化

- `D16AccessibilityPanel` 现在从默认绑定契约生成全部 61 个动作的键盘/手柄入口，D3/D4/D5/D9/D10 不再存在“已登记但面板无入口”的缺口。
- `ActionLabel` 与 `Localization.cs` 已补齐新增动作的中英文标签；滚动视口内容高度动态计算，底部按钮保持在固定可命中区域。
- 结构测试已按默认契约逐项检查按钮存在和可交互性；`Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests`、A03 runner 均构建通过，runner 7/7 PASS。
- Unity 许可证阻断仍未产生新的 XML，不能计作运行时 PASS；D16-A03 继续 IN_PROGRESS，D16-S01/S02 不启动。

### 2026-08-12 当前增量：D16-A03 D7 输入审计补漏

- `D7ControlCoordinator` 原本直接读取键盘 Escape，现已改为读取核心 `D16InputAction.Pause`；对话/贸易的模态关闭优先级与本帧防穿透逻辑保持不变。
- 全 Gameplay 旧入口的设备直读扫描已覆盖 D3/D4/D5/D7/D9/D10；D16 自有顶层模态的直接监听仅用于关闭/重绑定采集。
- `Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 继续 0 warning / 0 error；Unity 运行 XML 仍受许可证阻断，D16-A03 不关闭。

## 2026-08-12 D16-A03 Unity 运行环境恢复与完整回归

- 旧 Unity Hub 3.3.3-c1 搭配的 Licensing Client 1.11.0 无法处理 Unity 6 编辑器 1.17.4 协议；已将本机 Unity Hub 更新到 3.20.1，外部 Licensing Client 为 1.17.4。Unity 6 已实际连接 `Product: Unity Personal` 并更新许可证；这是本机环境修复，不是伪造项目证据。
- `tools/results/d16_a03_binding_scroll_single_unity6_retry_current_03.xml`：无障碍面板滚动/全部动作专项 **1/1 PASS**。
- D9/D10 上下文重绑定专项分别为 **1/1 PASS**：`d16_a03_d9_context_unity6_retry_current_01.xml`、`d16_a03_d10_context_unity6_retry_current_01.xml`。
- 首次完整回归为 32/34，失败仅来自测试夹具把 D9/D10 动作重绑定到各自作用域中已被占用的 Q；契约按设计拒绝同作用域重复键。已将夹具改用未占用的 Z，未放宽生产重复绑定规则。
- 修复后 `tools/results/d16_a03_full_d16_playmode_unity6_retry_current_02.xml` 为 **34/34 PASS**，失败 0、跳过 0；当前 `Assembly-CSharp` 与 `D16.PlayMode.Tests` 串行构建均 0 warning / 0 error。

本次只关闭 A03 的编辑器/Unity 运行回归子门禁。真实物理手柄、目标设备/分辨率、人工视觉与多场景焦点、长线稳定性仍缺，因此 D16-A03 继续 **IN_PROGRESS**；D16-S01/S02 不启动。

## 2026-08-12 D16-A04 单条人工决定工具

- 新增 `tools/d16_a04_apply_review_decision.py`：默认 dry-run，一次只能处理一个 Pending ID；要求真实存在的本地证据、责任审核者和明确决定依据。
- 敏感条目必须明确 warning、skip 和 presentation；验证失败不写回，写回使用临时文件、fsync 和 replace。
- D16-A04 专项测试 **19/19 PASS**，工具测试全集 **58/58 PASS**；真实审计仍为 Pending 267，未提交新的人工批准。
- 该工具只写人工审查结果字段，不改变历史标签、授权、资源状态、玩法数值或 Systems 所有权；A04、D16-S01/S02 继续未完成。

## 2026-08-12 D16-A05 占位资源替换队列

- 新增 `tools/d16_a05_replacement_queue.py`，生成 `tools/results/d16_a05_replacement_queue.json` / `.md`；当前 2,264 个 placeholder、51 批，状态 **BLOCKED**。
- 队列确认 2,262 条缺最终资源文件、2,262 条缺运行时 Prefab/消费者、2 条需替换已登记占位源与 Prefab、15 条仍需审查批准；工具不改 `asset_state`。
- 专项测试 **2/2 PASS**；A05 仍为 **IN_PROGRESS**，不得将纯色/程序几何/占位模型写成最终资产。

## 2026-08-12 D16-A03 无障碍设置语言一致性修复

- 修复 `D16AccessibilitySettingsStore`：语言由主菜单 `SettingsStore/Localization` 统一拥有，读取已有缓存和保存无障碍设置时都跟随实时 `wb_lang`，不再把旧 Locale 写回全局语言。
- 新增 `D16A03AccessibilitySavePreservesLiveMainMenuLanguage`，覆盖“中文旧快照→主菜单切英文→保存无障碍选项”的真实 MainMenu 链路。
- focused Unity 6 PlayMode **1/1 PASS**；D16.PlayMode.Tests 全量 **36/36 PASS**；Assembly-CSharp、D16.PlayMode.Tests、Save.Tests 构建均 0 warning / 0 error。
- 该子门禁已关闭；A03 总体仍 IN_PROGRESS，真实设备/目标分辨率/人工视觉/多场景焦点/长线证据缺失，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 用户模态互斥与外层输入锁

修复与“菜单叠加、按钮可见但点击无反应”直接相关的 UI 状态风险。无障碍、诊断和金融三个 D16 用户模态现在互斥打开；打开任一模态会先关闭其它同级模态。诊断面板关闭时也不再无条件解除外层暂停/输入锁，而是恢复打开前保存的状态；金融面板同步增加打开状态保护，避免重复关闭或对象销毁时误改全局输入状态。

- 新增 `D16A03UserModalsAreExclusiveAndRestoreOuterInputBlock`，覆盖 MainMenu 中“外层输入已阻断→打开无障碍→打开诊断→打开金融→关闭金融”的真实链路；
- 新增 `D16A03SensitiveOverlayRestoresOuterInputBlockOnDestroy`，覆盖敏感内容警告层销毁/重建时外层输入锁的恢复；
- focused Unity 6 PlayMode：模态互斥 **1/1 PASS**（`tools/results/d16_a03_modal_exclusive_unity6.xml`），敏感层销毁恢复 **1/1 PASS**（`tools/results/d16_a03_sensitive_overlay_unity6.xml`）；D16.PlayMode.Tests 全量：**38/38 PASS**，失败/不确定/跳过均为 0，`tools/results/d16_a03_full_after_sensitive_overlay_fix_unity6.xml`；
- `Assembly-CSharp`、`D16.PlayMode.Tests`、`Save.Tests` 均 0 warning / 0 error；`git diff --check` 通过（仅保留工作区既有换行提示）。

该增量关闭编辑器内同级用户模态叠层和输入锁误解除的代码级风险，但不替代真实鼠标/键盘/手柄、目标设备/分辨率、人工视觉、多场景焦点和长线稳定性证据。D16-A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D15 与 D16-A04—A06 当前权威审计复验

- D15 Content Registry：2,812 entries / 13 prefixes；D15-A01—A08 全部退出码 0；D15-S02 为 8 batches / 24 samples / 0 errors / 0 warnings，继续 **PASS**。
- D16-A04：完整 `game` 根目录 3,139 条记录，结构错误 0，授权 2,918/2,918；Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0，仍受 `pending_historical_reviews` 阻断。
- D16-A05：可视资源 2,918 条，其中 data_only 125、integrated 654、placeholder 2,264；非法元数据 0、集成资源视觉元数据缺口 0，仍受 `placeholder_assets_remain` 阻断。本轮没有把占位资源升级为最终资源。
- D16-A06：19 项本机/安装器检查全为 true，`missing_static_evidence=[]`，`technical_preparation_status=PASS`，但发布清单仍有目标设备、原生崩溃、隐私、A01—A05 及 Steam 政策等阻断项。

本轮只刷新权威结果并同步交接，不自动批准人工审核、不虚构最终资产、不执行 Steam 上传。D16-A04—A06 继续 **IN_PROGRESS**，D16-S01/S02 仍不得启动。

## 2026-08-12 全量工具与工程构建复验

- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**61/61 PASS**，0 failures、0 errors；D15-A06 catalog/assembly smoke 也通过。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore --nologo`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore --nologo`：0 warning / 0 error；
- `dotnet build game/Assembly-CSharp.csproj --no-restore --nologo`：0 warning / 0 error。

本轮只验证现有修复没有回归，不扩大为 A01—A06 总门禁通过；目标设备、跨平台断电、真机可访问性、人工审查和最终资源仍按当前阻断清单处理。

## 2026-08-12 D16-A02 跨平台测试器初始化修复

- `tools/d16_a02_runner/Program.cs` 不再无条件调用 Windows `kernel32.SetDllDirectory`；仅 Windows 配置项目内 sqlite 搜索目录，Linux/macOS 走平台原生加载器。
- `tools/d16_a02_runner/D16A02Runner.csproj` 已改用仓库相对依赖路径，并支持通过 `UnityManagedDir` 指定平台 Unity 托管目录，避免跨平台构建依赖 Windows 绝对路径。
- 当前 Windows 回归：runner **9/9 PASS**；故障注入受控中止 **3/3**、强制终止 **3/3**，总结果 `passed=true`；runner 构建 0 warning / 0 error。
- 该修复只让跨平台验证入口具备正确的平台分支，不等于已经取得 Linux/macOS 或物理断电证据；D16-A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16-A04 Pending 证据分组复核

- 当前 267 条 Pending 中，资产历史/授权 186 条、普通历史内容 81 条；258 条历史标签为 `inferred`，9 条为 `historical`。
- 264 条来源为项目自创，3 条为外部资产；没有显式 `fictional`/`fictionalized` 标记，因此不能由机器根据“项目自创”或 CC0 许可自动完成历史审查。
- 本轮没有新增批准或改写审查字段；A04 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16-A01 GPU 计时 fallback 复验

- 性能探针新增 `ProfilerRecorder` GPU fallback，只有真实 counter 返回正值才记录 GPU 毫秒；Assembly-CSharp、D16.PlayMode.Tests 构建 0 warning / 0 error。
- Unity 图形专项 1/1 PASS，60.017 秒、233 samples、平均 FPS 64.06、最低 FPS 2.90、平均主线程 4.33 ms；GPU counter 仍 unavailable，未伪造 GPU 数值。
- A01 继续 **IN_PROGRESS**：目标/低端设备、真实大场景、最终资产 LOD、GPU/内存目标、音频释放与长线证据仍缺；D16-S01/S02 不启动。

## 2026-08-12 D16-A01 GPU 计时 fallback 资源预热复测

- 相同 Unity 6 图形专项二次运行仍为 **1/1 PASS**，证据 `tools/results/d16_a01_gpu_recorder_playmode_unity6_retry.xml` / `.log`；60.015 秒、234 samples、平均 FPS 65.646、最低 FPS 2.911、平均主线程 4.279 ms。
- GPU counter 仍不可用，保持 `GpuSampleAvailable=false`、`GpuTimeMs=-1`；约 2.9 FPS 长帧可重复出现，未被过滤或包装成 PASS。
- A01 仍 IN_PROGRESS；D16-S01/S02 继续不启动，下一步是最长帧定位和目标/低端设备、真实大场景复测。

## 2026-08-12 D16-A01 启动长帧定位与材质分帧回归

- `D7MaterialPolishController` 改为每帧最多处理 8 个渲染器；性能探针新增最长全帧、发生时间和超过 50ms 计数。
- D5 烟测 `d17_material_budgeted_d5_smoke.log` 通过；D16 PlayMode 全量 `d16_after_material_budgeted_full_playmode_unity6.xml` 为 **38/38 PASS**；工具测试 **61/61 PASS**。
- 当前热启动/全量报告：60.006 秒、234 samples、平均 FPS 59.91、最低采样 FPS 52.50、最长全帧 189.883ms（启动后 0.034 秒）、超过 50ms 为 1 次；GPU counter 仍 unavailable。
- 冷启动仍有约 347—354ms 长帧，材质分帧后仍存在，暂定为冷启动场景/资源/多模块初始化方向，未宣称根因完全定位。A01 继续 IN_PROGRESS；D16-S01/S02 不启动。

## 2026-08-12 D16-A01 延迟 GPU counter 复验

- GPU `ProfilerRecorder` 改为场景启动 1 秒后延迟创建；冷启动仍有约 349ms 长帧，确认 counter 初始化不是唯一根因。
- 延迟方案下 D16 PlayMode 全量 `d16_after_deferred_gpu_full_playmode_unity6.xml` 为 **38/38 PASS**；最新性能报告：60.006 秒、234 samples、平均 FPS 59.964、最低采样 FPS 56.714、最长全帧 179.763ms（启动后 0.032 秒）、超过 50ms 为 1 次，GPU counter 仍 unavailable。
- 长帧仍集中于冷启动/编辑器资源与多模块首帧接线；A01 继续 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 当前源码 Windows Player 构建与启动复验

- 使用当前源码重新执行 Unity 6 `-batchmode -quit -projectPath game -buildWindows64Player`，日志 `tools/results/d16_a06_windows_build_unity6_current.log` 明确记录 `Build Finished, Result: Success`，batchmode 返回码为 0。
- 构建产物 `artifacts/d16_a06_windows_build/WhalesIslandReef.exe` SHA-256 为 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- 对该 Windows Player 执行 `-batchmode -nographics -quit` 启动烟测：`tools/results/d16_a06_windows_player_smoke_current_0812.log` 出现 `MainMenu` EventSystem、`InputSystemUIInputModule`、UI 初始化和 D16 设置应用；错误扫描未发现 Error/Exception/Crash/No cameras。Player 在无图形批处理模式下不会自行退出，已结束本次验收启动的 Player 进程。
- 该结果关闭 A06 的“当前源码 Windows 构建/启动”本机子门禁，但不替代真实目标设备、原生崩溃行为、最终人工隐私审查，也不改变 A01—A05 和 Steam 未授权阻断；D16-A06、D16-S01、D16-S02 仍未完成/不得启动。

## 2026-08-12 当前源码 D16 PlayMode 全量复验

- `D16.PlayMode.Tests` 通过 `-runTests -testPlatform PlayMode -assemblyNames D16.PlayMode.Tests` 当前 XML：**38/38 PASS**，失败/不确定/跳过均为 0，证据 `tools/results/d16_current_full_playmode_unity6.xml`。
- 性能报告当前为 60.006 秒、234 samples、平均 FPS 59.996、最低采样 FPS 59.438、最长全帧 180.456ms（启动后 0.032 秒）、超过 50ms 为 1 次；预算满足，GPU counter 不可用且未被伪造。
- 该结果只确认本机代码级运行时回归没有新增失败；A01 的目标设备/低端设备、真实大场景、最终资源和冷启动函数级归因仍缺，D16-S01/S02 继续不启动。

## 2026-08-12 D16 门禁当前权威复核

- A04：3,139 条记录，结构错误 0，授权记录 2,918/2,918；Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0；仍受 `pending_historical_reviews` 阻断；
- A05：当前审计统计 data_only 125、integrated 328、placeholder 2,264，非法元数据 0，集成资源视觉元数据缺口 0；实际源文件 FBX 2、OBJ 159、PNG 6、音频二进制 0；仍受 `placeholder_assets_remain` 阻断；
- A06：以 `game` 为项目根执行审计，19 项本机/安装器检查全为 true，`missing_static_evidence=[]`，`technical_preparation_status=PASS`；发布仍被 A01—A06 外部证据、A04/A05 和 Steam 未授权策略阻断；
- 工具测试全集当前为 **63/63 PASS**；D16-S01 尚未开始，D16-S02 继续 BLOCKED BY S01。

本节采用本轮实际审计输出；旧章节中的 A05 数量与 61/61 仅作为历史快照保留，不覆盖当前结果。

## 2026-08-12 工具回归统计修正

在 D16-A02 存档覆盖矩阵加入 3 个精确匹配/反例测试后，`python -m unittest discover -s tools/tests -p 'test_*.py' -q` 当前实际结果为 **66/66 PASS**。此前的 63/63、61/61 等数字保留为历史快照；当前 D16 门禁状态和阻断原因不变。

## 2026-08-12 当前权威门禁刷新

- D16-A01：D2 分阶段组装后 Unity PlayMode **38/38 PASS**；A01 高负载候选 **267/267**、单项 **1/1 PASS**；本机窗口最长帧 173.320ms，目标/低端设备、真实大场景和 GPU/内存证据仍缺。
- D16-A02：当前 Save.Tests **302/302 PASS**；严格矩阵 **10/10 类别、31/31 精确匹配**；runner **9/9 PASS**；受控/强杀故障注入各 **3/3 PASS**；100 次长线通过。物理断电、Linux/macOS/跨平台仍缺。
- 当前工具测试实际为 **68/68 PASS**；D16-A03—A06 仍因真机/人工、历史审核、placeholder、目标设备/隐私等外部门禁保持 IN_PROGRESS。
- D16-S01 仍 NOT STARTED，D16-S02 继续 BLOCKED BY S01；未执行 Steam 上传、外部发布或外部消息发送。

## 2026-08-12 D2 控制闭环与当前门禁刷新

- D16 PlayMode 当前全量为 **39/39 PASS**，失败、不确定、跳过均为 0；新增真实 `InitialHarbor` 控制闭环专项 `D16A03InitialHarborKeyboardCameraAndBoardingLoopWorks` 为 **1/1 PASS**，覆盖键盘移动、鼠标右键拖动视角、视角循环、上船、船舶航行和下船；D16-A03 输入路由 fixture 当前 **10/10 PASS**。
- 控制闭环日志已记录 `[D2] 运行时闭环已接线`、`[Camera] 视角切换：船上视角`、`[D2/Control] 已进入船只控制` 和 `[D2/Control] 已退出船只控制`；这关闭了本机代码链路缺口，但不替代真实设备、目标分辨率、焦点和人工体验证据。
- 当前性能报告 `tools/results/d16_a01_performance_window_report.json`：60.0156 秒、234 samples、平均 FPS 59.975、最低 FPS 57.264、平均主线程 4.434ms；最长全帧 **175.977ms**（启动后 0.0244 秒）、超过 50ms 1 次；`BudgetSatisfied=true`，GPU counter 不可用且保持 `GpuTimeMs=-1`。D16-A01 仍 IN_PROGRESS。
- D16-A02 本机代码门禁仍为 Save.Tests **302/302 PASS**、严格矩阵 **10/10 类别 / 31/31 精确匹配**、runner **9/9 PASS**、受控/强杀故障注入各 **3/3 PASS**、100 次长线通过；物理断电、Linux/macOS 和目标设备证据仍缺。
- D16-A04 当前 Pending **267**；D16-A05 当前 placeholder **2,264**；D16-A06 本机/安装器技术准备 **19/19 true**，但目标设备、原生崩溃、隐私和外部发布边界仍缺。A01—A06 未全部真实收口，D16-S01/S02 继续不启动，Steam 未上传。

## 2026-08-12 D16-A01 登记保护标志回归后的门禁刷新

- D16-A01 新增“stable ID 正确但玩家保护标志错误”真实回归，专项 **1/1 PASS**；当前 D16 PlayMode 全量 **40/40 PASS**，Assembly-CSharp 构建 0 warning / 0 error。
- 最新本机性能窗口：234 samples、平均 FPS 59.941、最低 FPS 52.646、平均主线程 4.228ms、最长全帧 171.049ms（启动后 0.0242 秒）、超过 50ms 1 次；预算满足，GPU counter unavailable，A01 仍 IN_PROGRESS。
- 本轮只修复表现登记配置一致性，不改变领域状态、事件、存档或契约；目标/低端设备、真实大场景、最终资产、人工审核、跨平台和发布证据仍未关闭。

## 2026-08-12 D16-A01 对象池复用状态复位

- 真实缺口：表现实例从 `D16PresentationPool` 再次租借时会残留上一次的 Frozen/LOD/静音/Animator 状态。
- 修复文件：`game/Assets/Scripts/Gameplay/D16/D16PresentationRegistration.cs`、`game/Assets/Scripts/Gameplay/D16/D16PresentationPool.cs`；新增对象池复用回归断言。
- 专项证据：`tools/results/d16_a01_pool_reset_unity6_v2.xml`，**1/1 PASS**。
- 全量证据：`tools/results/d16_current_full_after_pool_reset_unity6_v2.xml`，**40/40 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 性能证据：`tools/results/d16_a01_performance_window_report.json`，234 samples，平均 FPS 59.937，最低 54.577，平均主线程 4.194ms，最长全帧 170.477ms，预算满足；GPU counter unavailable 原样保留。
- 状态：关闭本机代码级对象池表现状态泄漏风险；A01—A06 外部门禁尚未全部满足，D16-S01/S02 继续不启动。

## 2026-08-12 当前 D16-A04/A05/A06 审计基线

- A04 当前证据 `tools/results/d16_a04_review_audit_current.json`：3,139 条记录、可视授权 2,918/2,918、结构错误 0、Approved 2,780、ApprovedWithWarning 91、Pending 267、Rejected 0；队列 `d16_a04_review_queue_current.json` 分 12 批。
- A05 当前证据 `tools/results/d16_a05_asset_audit_current.json`：可视登记 2,918、data_only 125、integrated 654、placeholder 2,264；非法元数据 0、集成资源元数据缺口 0；队列 `d16_a05_replacement_queue_current.json` 分 51 批。
- A06 当前证据 `tools/results/d16_a06_release_audit_current.json`：19/19 技术检查 true、`missing_static_evidence=[]`、technical preparation PASS，但 release 仍 blocked。
- 本轮只刷新可追踪审计和队列，没有自动批准人工审核、替换/升级 placeholder 或执行 Steam 上传；D16-S01/S02 继续不启动。

## 2026-08-12 D16-A01 对象池状态覆盖增强后的回归

- 对象池专项现在同时验证 Renderer、Audio 和 Animator 基线：`tools/results/d16_a01_pool_reset_unity6_v3.xml` **1/1 PASS**。
- D16 PlayMode 全量 `tools/results/d16_current_full_after_pool_reset_unity6_v3.xml` **40/40 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 最新性能报告：235 samples、平均 FPS 59.979、最低 58.817、平均主线程 4.357ms、最长全帧 183.410ms（启动后 0.0260s），预算满足；GPU counter unavailable 原样保留。
- 这是本机表现复用回归覆盖增强，不改变 A01—A06 外部门禁；D16-S01/S02 继续不启动。

## 2026-08-12 A01 主机与静态场景基线

- 主机证据 `tools/results/d16_a01_hardware_baseline_current.json`：20 logical processors、约 31.8 GiB RAM、RTX 4070 Ti / 12,282 MiB VRAM；明确 `is_target_device_evidence=false`、`is_low_end_device_evidence=false`。
- 静态场景证据 `tools/results/d16_a01_static_baseline_current.json`：InitialHarbor 24 GameObjects、17 MeshRenderer、17 MeshFilter、0 静态 Collider；运行时字段为 null。
- 该快照只限定本机/静态范围，不替代目标/低端设备、真实大场景和运行时 GPU/内存证据；A01 继续 **IN_PROGRESS**。

## 2026-08-12 D16-A02 当前中断与长线复验

- `tools/results/d16_a02_fault_injection_result.json`：受控写入中止 3/3、Windows 强制终止 3/3，旧文件/新文件、SQLite 完整性、迁移回滚和备份检查均通过。
- `python tools/d16_a02_long_run.py 100`：100 次长线通过，SQLite integrity `ok`，最终事实 `长线存档船长-99 / 599`，备份数 399。
- 证据范围仍限 Windows/.NET；物理断电、Linux/macOS、跨平台恢复、目标设备和完整 D7—D14 组合缺失，A02 继续 **IN_PROGRESS**。

## 2026-08-12 Save.Tests 当前 v2 证据

- Unity 6 `Save.Tests` 当前 XML `tools/results/d16_current_save_tests_editmode_unity6_v2.xml`：**302/302 PASS**，failed/inconclusive/skipped 均为 0；
- Unity 刷新后先恢复工程依赖，再按顺序构建 AgentCore、Save.Tests、Assembly-CSharp，均 **0 warning / 0 error**；
- 该证据只覆盖当前 Windows/.NET 本机存档回归，不替代物理断电、Linux/macOS/跨平台、目标设备和完整 D7—D14 生产组合；A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 表现降级状态保护证据

- 新增专项 `D16A01PresentationDowngradePreservesFactsSaveAndSettlement`：`tools/results/d16_a01_state_preservation_unity6_v2.xml` **1/1 PASS**；
- 真实 `EventLog` 事实、`EventLogSnapshot`、已结算幂等门和碰撞体在 Reduced/Background/Frozen/Full 四档表现降级后均保持；报告为 `tools/results/d16_a01_state_preservation_report.json`；
- 这是 D16 表现层本机代码证据，不替代 SaveSystem 文件级断电/跨平台恢复、目标设备、真实大场景和长线 S01；A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 状态保护后的全量回归

- D16 PlayMode 全量 `tools/results/d16_current_full_after_state_preservation_v2_unity6.xml`：**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 性能窗口 60.015 秒、234 samples、平均 FPS 59.972、最低 58.806、平均主线程 4.356ms、最长全帧 174.150ms；`BudgetSatisfied=true`，GPU 计时保持 unavailable；
- 本轮只增加 D16 PlayMode 状态保护证据，不改变 A01—A06 的外部门禁；A01 仍缺目标/低端设备、真实大场景、最终资产和跨平台/长线证据，D16-S01/S02 不启动。

## 2026-08-12 A03 无障碍面板专项复验

- `D16A03AccessibilityPanelExposesAllCoreBindingsInScrollableViewport`：`tools/results/d16_a03_accessibility_panel_current_unity6.xml` **1/1 PASS**；
- 真实 MainMenu 设置→无障碍模态链检查滚动视口、契约动作的键盘/手柄按钮和重绑定监听；
- D16 PlayMode 当前全量 `d16_current_full_after_state_preservation_v2_unity6.xml`：**41/41 PASS**；
- 这是本机代码级证据，不替代真实手柄、目标分辨率/可读性、人工焦点和长线证据；A03 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 最终本机边界扫描与审计刷新

- `game/Assets/Scripts/Systems/` 可执行 C# 行过滤注释后未发现 LLM、HttpClient、UnityWebRequest、Socket 或 WebSocket 依赖；Systems 网络/LLM 红线保持满足。
- `git diff --check` 退出码 0；仅报告现有工作区的 LF→CRLF 提示，没有空白错误。
- 当前权威审计仍为：A04 `3,139` 条记录 / `267` Pending / 结构错误 `0`；A05 `2,918` 可视登记 / `2,264` placeholder / 非法元数据 `0`；A06 技术准备 `19/19`，release `blocked`。
- 本轮没有修改审查决定、授权记录、最终资产状态、契约或发布状态；没有执行 Steam 上传。D16-S01 仍 **NOT STARTED**，D16-S02 仍 **BLOCKED BY S01**，项目不标记 `PRODUCTION/DONE`。

## 2026-08-12 当前工作树 Unity 6 全量回归刷新

- 当前图形模式 PlayMode XML `tools/results/d16_current_full_continuation_unity6.xml`：**41/41 PASS**，failed/inconclusive/skipped 均为 0，耗时 71.808s。
- 当前性能窗口：234 samples、平均 FPS 59.934、最低 53.428、平均主线程 4.331ms、最长全帧 172.579ms（启动后 0.0246s）、超过 50ms 1 次；场景加载 0.1998s、D5 Ready 0.1066s、GC 22/22/22；预算满足，GPU counter unavailable。
- 这是当前本机 Unity 代码回归证据，不替代目标/低端设备、真实大场景、最终资产、跨平台和长线证据；A01—A06 未全部 PASS，D16-S01/S02 继续不启动。

## 2026-08-12 A01 性能报告同屏计数补齐

- 修复 A01 验收报告只采集帧率/内存而遗漏同屏计数的问题；报告现在落盘 Agent、Animal、Audio、LoadedCell、Registration、流式切换和对象池计数。
- 当前 Unity 6 D16 PlayMode `tools/results/d16_current_full_metrics_fields_unity6.xml`：**41/41 PASS**；A01 报告实际为 Agent `1`、Animal `0`、Audio `1`、LoadedCell `2`、Registration `3`。
- A01 性能报告专项 **2/2 PASS**，工具全量 **68/68 PASS**，三套关键工程构建 0 warning / 0 error；这是证据输出修复，不改变 A01 总体 **IN_PROGRESS** 或 D16-S01/S02 门禁。

## 2026-08-12 当前工作树 Save.Tests 回归刷新

- Unity 6 EditMode `tools/results/d16_current_save_tests_continuation_unity6.xml`：**302/302 PASS**，failed/inconclusive/skipped 均为 0；
- 依赖恢复后按顺序构建 AgentCore、Save.Tests、Assembly-CSharp：均 **0 warning / 0 error**；
- 该结果只刷新当前 Windows/.NET 存档回归，不替代物理断电、Linux/macOS/跨平台文件系统、目标设备和完整 D7—D14 组合；A02 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
- 当前 Windows 主机未安装可用的 WSL/Linux 发行版；Linux/macOS/跨平台证据必须由对应真实环境补齐，不能由本机结果替代。

## 2026-08-12 A05 声明路径闭包审计

- A05 审计新增对 `resource_path`、`runtime_prefab_path`、`model_path`、`audio_file_ref`、`source_path` 的只读存在性检查，并处理仓库相对路径、Unity 项目相对路径和 Markdown 锚点。
- 当前结果：334/334 resource、2/2 runtime Prefab、320/320 model、38/38 audio reference、3/3 source path 均存在，声明路径断链为 0；visual 2918、integrated 654、placeholder 2264，非法元数据和视觉元数据缺口均为 0。
- `python -m unittest tools.tests.test_d16_a04_a06_audits -v`：**21/21 PASS**；
- 全量工具回归 `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**69/69 PASS**（97.745s）；
- 该项只关闭已声明入口的机器断链风险，不代表最终授权/历史审查/最终美术完成；A05 仍被 `placeholder_assets_remain` 阻断，D16-S01/S02 不启动。

## 2026-08-12 A06 检查项计数校准

- A06 审计新增 `steam_upload_policy_safe`，明确检查清单中的 Steam 上传授权和执行状态均为 `false`，且不执行任何上传。
- 当前权威结果 `tools/results/d16_a06_release_audit_current.json`：**19/19 checks true**、`missing_static_evidence=[]`、技术准备 `PASS`、发布状态 `blocked`。
- 项目级阻断仍完整保留：A01 目标性能、A02 断电/跨平台、A03 真机可访问性、A04 人工审查、A05 placeholder、A06 目标设备/原生崩溃/隐私、Steam 未授权；A06 总体继续 `IN_PROGRESS`。
- A06 专项审计测试 **21/21 PASS**；该修复只校准审计覆盖，不启动 D16-S01/S02。

全量工具回归在 A06 修复后重新执行：**69/69 PASS**（95.562s）；本轮没有改变 A01—A06 的外部证据阻断。

## 2026-08-12 D16-A01 表现流式分帧与加载指标

- `D16WorldStreamCell` 现在按批次跨帧激活表现子对象，并保留场景原始的活跃/禁用意图；卸载会停止未完成表现加载并释放单元音频引用。
- 新增专项 `tools/results/d16_a01_staged_streaming_unity6.xml` **1/1 PASS**；当前工作树 D16 PlayMode 全量 `tools/results/d16_a01_staged_streaming_full_current_unity6.xml` **42/42 PASS**。
- A01 性能窗口专项 `tools/results/d16_a01_staged_streaming_performance_current_unity6.xml` **1/1 PASS**；报告新增加载中单元、已加载/待加载表现对象、完成加载耗时和加载次数字段。
- 当前窗口的最低 FPS 3.508、最长帧 285.071ms、GPU counter unavailable 均原样保留；这是采样链/代码回归证据，不是性能目标通过。
- A01 仍受目标/低端设备、真实大场景、最终资产、GPU/内存、跨平台和长线证据阻断；D16-S01/S02 继续不启动。

## 2026-08-12 D16-A01 远景建筑代理表现接线

- 新增 `D16RemoteBuildingProxy` 与控制器注册/预算切换：远景使用代理表现根，近景恢复完整表现，冻结时隐藏表现；逻辑碰撞体不受影响。
- 聚焦证据 `tools/results/d16_a01_remote_building_proxy_unity6_rerun.xml`：**1/1 PASS**；全量证据 `tools/results/d16_a01_remote_building_proxy_full_current_unity6.xml`：**43/43 PASS**，0 failed / 0 inconclusive / 0 skipped。
- D16 PlayMode 测试工程构建：0 warning / 0 error；新增源码已纳入显式编译清单。
- 该轮只完成运行时表现能力和隔离回归，不等同于最终建筑代理美术或真实场景配置完成；A01 仍 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 D16-A01 D3D12 GPU 计时复核

- 强制 Direct3D 12 的 A01 性能窗口 `tools/results/d16_a01_performance_d3d12_current_unity6.xml`：**1/1 PASS**，约 60.45s。
- 该窗口平均 FPS 64.904、最低采样 FPS 3.609、最长帧 277.109ms、平均主线程 4.676ms；GPU 计时仍为 unavailable（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。
- 这是更强的缺口证据，不是性能目标通过；A01 仍 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 D16-A01 GPU counter 候选探测

- `D16RuntimePerformanceProbe` 新增 GPU-only counter 候选探测，并在性能报告写入 `GpuCounterName`；不把有效句柄或 CPU 时间当成 GPU 样本。
- D3D12 回归 `tools/results/d16_a01_performance_gpu_counter_probe_d3d12_unity6.xml`：**1/1 PASS**；选择到 `GPU Frame Time`，但样本仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。
- 当前窗口平均 FPS 65.904、最低采样 FPS 3.608、最长帧 277.177ms；该结果仍不能关闭 A01，D16-S01/S02 不启动。

## 2026-08-12 GPU counter 探测后的全量回归

- 当前工作树 D16 PlayMode `tools/results/d16_a01_gpu_counter_probe_full_current_unity6.xml`：**43/43 PASS**，0 failed / 0 inconclusive / 0 skipped，约 72.36s。
- 探测代码的全量回归通过，但 A01 仍因 GPU 样本、目标/低端设备、真实大场景、最终资源、跨平台和长线证据未闭合而保持 IN_PROGRESS。

## 2026-08-12 D16-A01 最终自动化复核

- 全量工具测试 **69/69 PASS**（108.107s）。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests 均 0 warning / 0 error；Systems 边界扫描、`git diff --check` 均 PASS。
- 这些是自动化/本机构建证据，不替代 A01 的目标设备、真实大场景、GPU 样本、最终资源、跨平台和长线验收。

## 2026-08-12 D16-A06 异常退出会话标记

- `D16LocalDiagnostics` 增加同意后创建、正常退出清理、下次启动检测的固定 marker；检测结果只落盘 `unexpected_exit` 脱敏分类，不保存原始错误、堆栈、路径、存档或对话内容。
- 聚焦证据 `tools/results/d16_a06_session_marker_playmode_current_unity6.xml`：**1/1 PASS**；全量 `tools/results/d16_a06_session_marker_full_current_unity6.xml`：**44/44 PASS**。
- 该项只补托管诊断可观测性，不等于原生崩溃转储、目标设备/隐私人工验收通过；A06 继续 IN_PROGRESS，发布清单继续 blocked。

### A06 当前自动化复核

- 权威发布审计 **19/19 checks true**、`missing_static_evidence=[]`、技术准备 PASS、发布状态 blocked。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests 均 0 warning / 0 error；全量工具测试 **69/69 PASS**（97.455s）。
- Systems 边界扫描与 `git diff --check` PASS；不把自动化绿灯扩大为原生崩溃、目标设备或人工隐私门禁通过。

## 2026-08-12 A02 孤儿原子写入临时文件清理

- `SaveFileSafety.CleanupTemporaryArtifacts` 已接入 `SaveSystem.HasSave()`；只清理当前存档及 SQLite sidecar 的规范 GUID 临时文件，未知命名、正式存档和备份不受影响。
- Unity 6 focused `D16SaveSafetyTests`：**10/10 PASS**；当前 `Save.Tests` 全量：**303/303 PASS**；严格档案矩阵 **10/10 类别、31/31 精确匹配**；独立 runner **10/10 PASS**。
- 这是本机存档安全代码增量，不关闭物理断电、Linux/macOS/跨平台、目标设备或完整生产组合门禁；A02 继续 `IN_PROGRESS`，发布门禁和 D16-S01/S02 状态不变。

## 2026-08-12 D16-A03 模态焦点范围增量

- Gameplay/UI 新增 `D16ModalFocusScope`，将主菜单、暂停和敏感/无障碍/诊断/金融模态的 UGUI 显式导航限制在当前可见层，并自动维护滚动焦点可见；
- `tools/results/d16_a03_focus_scope_playmode_current_unity6.xml`：专项 **1/1 PASS**；`tools/results/d16_a03_focus_scope_full_current_unity6.xml`：D16 PlayMode **45/45 PASS**；
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests、D16A03Runner 构建通过；工具测试 **69/69 PASS**；
- 该项只关闭本机代码级焦点逃逸风险。真实设备、人工视觉/焦点、A01/A02/A04/A05/A06 外部证据仍未闭合，发布清单继续 blocked，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 焦点候选激活状态复核

- `D16ModalFocusScope` 现在只把运行时激活且可交互的控件纳入显式导航，隐藏子面板不再成为导航目标；
- 专项 **1/1 PASS**：`tools/results/d16_a03_focus_scope_active_hierarchy_playmode_current_unity6.xml`；
- 全量 **45/45 PASS**：`tools/results/d16_a03_focus_scope_active_hierarchy_full_current_unity6.xml`；
- 该增量只强化本机 UI 回归，不关闭真实设备/人工证据；A01、A02、A04、A05、A06 和发布清单状态不变。

## 2026-08-12 D15-S02 下游门禁复验

- Save.Tests 在恢复 Unity 临时依赖索引后构建 0 warning / 0 error；
- Content Registry 2,812 entries / 13 prefixes，D15-A01—A08 全部专项退出码 0；
- D15-S02 8 batches / 24 samples / 0 errors / 0 warnings，PASS；
- 工具测试全集 69/69 PASS，9/9 保存恢复样本 verified；
- 该复验确认 D15-S02 没有回退，但不关闭 D16-A01—A06 外部真实门禁，也不启动 D16-S01/S02。

## 2026-08-12 A06 MainMenu Player 启动探针隔离复验

- 修复 `D3RuntimeSmokeProbe` 的安装条件：只有 `InitialHarbor` 活动场景安装 D3 港口探针，正式 Player 的 `MainMenu` 启动不再被误判为港口 smoke 超时。
- Unity 6 聚焦 **1/1 PASS**：`tools/results/d16_a06_mainmenu_smoke_probe_playmode_current_unity6.xml`；D16 PlayMode 全量 **46/46 PASS**：`tools/results/d16_a06_smoke_probe_full_current_unity6.xml`。
- 当前源码 Windows Player 构建成功：`artifacts/d16_a06_windows_build_smoke_probe/`；启动日志确认 D3D11、EventSystem/InputSystemUIInputModule、MainMenu UI、D16 设置均初始化，且无 D3 Smoke/No cameras/Exception/Crash。Player 不响应 `-quit`，本次只记为“启动日志通过、自然退出未证实”。
- 该结果只关闭本机启动误触发，不关闭目标设备、原生崩溃、人工隐私、A01—A05 项目级阻断或 Steam 政策门禁；A06 仍 **IN_PROGRESS**，D16-S01/S02 仍不启动。

## 2026-08-12 A06 D5 MainMenu 启动探针隔离复验

- 根因：-d5Smoke 在正式 Player 的首场景 MainMenu 也创建港口探针，超时后把主菜单启动误报为失败；
- 修复：D5RuntimeSmokeProbe 现在仅在活动场景为 InitialHarbor 时安装；
- 聚焦证据：tools/results/d16_a06_d5_probe_guard_current_unity6.xml **1/1 PASS**；全量证据 tools/results/d16_a06_d5_guard_full_current_unity6.xml **46/46 PASS**；
- Windows Player 构建：artifacts/d16_a06_windows_build_d5_guard/，tools/results/d16_a06_windows_build_d5_guard_current.log 为 Success；
- A06 总体仍 IN_PROGRESS，发布清单仍 blocked；目标设备、原生崩溃、隐私人工审查、A01—A05 外部证据和 Steam 授权未闭合。

## 2026-08-12 A01 共享音频作用域卸载隔离复验

- `D16AudioResourceScope` 与 `D16WorldStreamCell` 已改为按流式单元加载基线做增量音频引用释放，避免共享作用域被单个单元卸载误清空。
- 聚焦共享作用域用例 **1/1 PASS**；原有音频生命周期用例 **1/1 PASS**；当前 D16 PlayMode 全量 **47/47 PASS**。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 0 warning / 0 error。
- 该轮只关闭 A01 本机表现层引用隔离风险；目标设备/真实大场景/最终资源/GPU/内存/跨平台/长线证据仍未完成，A01 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

### 同路径多 owner 加固

- 新增 `D16AudioResourceScope` owner 持有与释放入口、`D16WorldStreamCell.TrackAudioResource(...)`；同一路径的多个流式 owner 不再互相误释放。
- 聚焦 `tools/results/d16_a01_shared_audio_owner_current_unity6.xml` **1/1 PASS**；全量 `tools/results/d16_a01_shared_audio_owner_full_current_unity6.xml` **47/47 PASS**。
- 该轮只补齐本机表现资源生命周期边界；A01 外部性能/最终资产门禁仍为 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 D16-A03 动态 UGUI 字体回退

- `Fonts.ResolveForText(...)` 与 `D16FontFallbackText` 已接入 UGUI 动态文本；CJK 文本优先使用项目内 CJK 字体，并按整段字符覆盖选择候选字体。
- 初次专项验证暴露了编辑器内置字体对中文覆盖判断受宿主环境影响的问题，修正后 `d16_a03_font_fallback_fixed_current_unity6.xml` **1/1 PASS**；D16 PlayMode 全量 `d16_a03_font_fallback_full_current_unity6.xml` **52/52 PASS**。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 0 warning / 0 error；本机代码子门禁已闭合，但目标设备字体、分辨率、人工可读性和真实输入证据仍缺，D16-A03 及 D16-S01/S02 保持未完成。

## 2026-08-12 A01 预加载音频 owner 生命周期补强

- 修复流式单元在首次进入加载态前登记音频时缺少 owner 的路径；`D16WorldStreamCell.TrackAudioResource(...)` 现在会先建立稳定 owner，后续加载、卸载和销毁均可回收该引用。
- 聚焦 `tools/results/d16_a01_preload_audio_owner_current_unity6.xml` **1/1 PASS**；D16 PlayMode 全量 `tools/results/d16_a01_preload_audio_full_current_unity6.xml` **48/48 PASS**。
- 该修复只关闭 A01 本机表现资源生命周期缺口，不改变 A01 外部性能/最终资产门禁；D16-S01/S02 仍不启动。

## 2026-08-12 A01 未加载单元音频清理与销毁回归

- 修复 `D16WorldStreamCell` 在保持 unloaded、表现根为空或首次加载前销毁时的音频 owner 清理早退；`OnDestroy()` 与重复 unloaded 现在复用安全清理入口，不会误释放没有本单元加载基线的共享 scope 资源。
- 新增 `D16A01UnloadedCellReleasesOwnedAudioWithoutPresentationRoot`、`D16A01DestroyBeforeFirstLoadReleasesOwnedAudio`。
- 聚焦 `tools/results/d16_a01_unloaded_audio_current_unity6.xml`、`tools/results/d16_a01_destroy_before_load_current_unity6.xml` 均 **1/1 PASS**；当前 D16 PlayMode 全量 `tools/results/d16_a01_audio_lifecycle_full_current_unity6.xml` **50/50 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 构建均 0 warning / 0 error；该轮仍只关闭本机 A01 生命周期代码缺口，A01 外部真实性能/最终资产门禁继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 流式单元对象池重新配置清理

- 修复 `D16WorldStreamCell.ConfigureRuntime(...)` 重新配置时覆盖旧 scope/表现根的生命周期缺口；现在先停止旧加载、释放旧 owner/基线并停用旧表现根。
- 新增 `D16A01ReconfigureReleasesPreviousAudioOwnerAndPresentationRoot`。
- 聚焦 `tools/results/d16_a01_reconfigure_audio_current_unity6.xml` **1/1 PASS**；当前 D16 PlayMode 全量 `tools/results/d16_a01_reconfigure_full_current_unity6.xml` **51/51 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 两套工程构建 0 warning / 0 error；本轮仍只关闭 A01 本机对象池生命周期风险，真实岛屿/海域最终资源和外部设备/跨平台/长线证据未闭合，A01 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 岛屿数据流注册与可见性事实

- 新增 `D16IslandDataStreamRegistry`，将 D5 的 12 个岛屿目录接入 D16 只读数据流，并把海图距离模拟等级、活动数据单元和视觉表现根数量写入性能报告；不生成或伪造 3D 岛屿资源。
- 聚焦 `d16_a01_island_data_stream_current_unity6.xml` **1/1 PASS**；当前 D16 PlayMode 全量 `d16_a01_island_data_stream_full_current_unity6.xml` **53/53 PASS**；性能窗口 `d16_a01_performance_window_current_unity6.xml` **1/1 PASS**。
- 当前报告事实：12 个目录单元、7 个活动数据单元、12 个 data-only、0 个视觉表现根；工具测试 **69/69 PASS**，相关工程构建 0 warning / 0 error。
- 该增量只关闭 A01 本机数据流可见性缺口；真实岛屿/海域资产、目标/低端设备、GPU/内存、跨平台和长线证据仍缺，A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D16 完整 PlayMode 当前复验

- 受控恢复 Unity 临时依赖后，重新执行 `D16.PlayMode.Tests` 全量：`tools/results/d16_current_full_revalidation_unity6.xml` **53/53 PASS**，0 failed / 0 skipped / 0 inconclusive。
- 本次确认 D16 本机代码回归没有受临时工程资产清理影响；A01—A06 的目标设备、人工审查、真实最终资源、跨平台存档、原生崩溃和隐私门禁仍未闭合，发布清单继续 blocked，D16-S01/S02 不启动。

## 2026-08-12 A05 垂直切片优先交接队列

- `tools/d16_a05_replacement_queue.py` 新增只读 `vertical_slice_first` 范围；当前队列 2,264 个 placeholder、52 批，第一批为 `asset_ph_crew_male`、`building_saint_marys_dock`、`building_small_boat_pier`、`island_saint_marys`。
- `asset_ph_ship_fishing` 已有 CC0 授权且登记为 `integrated`，不属于 placeholder 替换队列；船控、上下船、碰撞、锚点和最终表现仍需运行时验收。
- 队列专项 **3/3 PASS**；本轮没有批准、替换资源或修改 `asset_state`。A05 仍 IN_PROGRESS，D16-S01/S02 继续不启动。

- 队列变更后的全量工具回归：**70/70 PASS**；A05 审计仍为 visual 2,918、data_only 125、integrated 654、placeholder 2,264，声明路径断链 0。

## 2026-08-12 A02/A06 当前门禁快照

- A02 当前证据：Save.Tests **303/303 PASS**；严格矩阵 **10/10 类别、31/31 精确用例**；故障注入受控/强制终止 **6/6 PASS**；100 次长线 Save/Load/迁移/恢复 **PASS**。物理断电、Linux/macOS/跨平台和目标设备仍未验证。
- A06 当前证据：`d16_a06_release_audit.py` **19/19 checks true**、`missing_static_evidence=[]`、技术准备 PASS；发布清单仍 blocked，7 条项目级阻断保持不变，Steam 未执行。
- 本快照只更新证据索引，不关闭 A02/A06 的外部真实性门禁；D16-S01/S02 和 D17 继续不启动。

## 2026-08-12 A02 sidecar 成组回滚与当前工作树复验

- `SaveFileSafety.RestoreBackup(...)` 现在在 WAL/SHM 恢复失败时回滚主 SQLite 文件与两个 sidecar，避免留下不一致的半恢复状态；新增测试故障注入入口仅供测试使用。
- focused Unity 6：`tools/results/d16_a02_sidecar_rollback_current_20260812.xml` **1/1 PASS**；Save.Tests 全量 `tools/results/d16_a02_save_tests_after_sidecar_rollback_20260812.xml` **304/304 PASS**。
- 严格档案矩阵 `tools/results/d16_a02_archive_matrix_sidecar_rollback_20260812.json`：10/10 类别、31/31 精确用例匹配；D16A02Runner 真实重建 0 warning / 0 error，执行 **11/11 PASS**。
- 存档修复后 D16 PlayMode 全量 `tools/results/d16_full_after_sidecar_rollback_20260812.xml`：**53/53 PASS**。
- A02 仍 **IN_PROGRESS**：上述证据仅覆盖 Windows/.NET 代码级回归；物理断电、Linux/macOS/真实跨平台文件系统和目标设备仍未取得，D16-S01/S02 不启动。

## 2026-08-12 A06 诊断文件原子替换复验

- `D16LocalDiagnostics` 会话标记写入与日志轮转改为同目录原子替换；失败时保留旧目标，不再先删除旧日志。
- `tools/results/d16_a06_diagnostics_atomic_current_20260812.xml` 聚焦 **8/8 PASS**；`tools/results/d16_full_after_diagnostics_atomic_20260812.xml` 当前 D16 PlayMode **54/54 PASS**。
- 该证据只关闭本机诊断文件替换的代码级缺口；A06 目标设备/native crash/最终隐私人工审查、A01—A05 外部门禁及 Steam 授权仍未闭合，发布清单继续 blocked，D16-S01/S02 不启动。

## 2026-08-12 A01 D5 启动阶段函数级归因

- D5 运行时新增等待玩家、读档、系统创建、状态初始化、角色表现和海图构建六阶段的只读耗时；D16 探针通过公开属性读取，未建立反向程序集依赖。
- `tools/results/d16_a01_phase_attribution_current_unity6.xml` 聚焦 **1/1 PASS**；最新 `tools/results/d16_a01_performance_window_report.json` 为 234 samples、平均 65.71 FPS、最低 3.49 FPS、最长全帧 286.43ms（启动后 0.091s），GPU 计时 unavailable。
- 当前阶段归因：D5 总构建 227.823ms，其中等待玩家 91.676ms、加载快照 0.006ms、系统创建 1.200ms、状态初始化 0.518ms、角色表现 0.681ms、海图构建 2.407ms，6/6 完整。
- `tools/results/d16_full_after_phase_attribution_20260812.xml` D16 PlayMode **54/54 PASS**；本轮只关闭本机阶段观测子门禁，长帧根因、目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线仍未闭合，A01 保持 IN_PROGRESS。

## 2026-08-12 A01 D2 启动阶段归因复验

- `D2RuntimeBootstrap` 六阶段计时已接入 D16 性能报告；D2 总构建 165.364ms，物理 13.506ms、控制 6.580ms、表现 8.711ms，6/6 完整。
- `tools/results/d16_a01_d2_phase_attribution_current_unity6.xml` 聚焦 **1/1 PASS**；当前窗口 235 samples、平均 65.05 FPS、最低 3.56 FPS、最长全帧 280.86ms，GPU 帧时间 unavailable。
- 该增量只关闭本机 D2 启动观测子缺口；D2/D5 之外的场景资源与其他 Bootstrap、目标/低端设备、真实大场景、最终资源和长线证据仍未闭合，A01 保持 IN_PROGRESS。

## 2026-08-12 A01 D3/D4/D9 启动链就绪归因复验

- D16 性能探针新增 D3/D4/D9 公开 `IsReady` 就绪时间字段；只读观测，不改玩法、存档、契约或领域写者。
- D3D11 聚焦证据 `tools/results/d16_a01_d3_d4_d9_readiness_graphics_current_unity6.xml` **1/1 PASS**；D3/D4/D9 均约 0.3447s 就绪。
- 全量回归后的当前性能报告：233 samples、平均 59.98 FPS、最低 57.50 FPS、最长帧 181.47ms；D3/D4/D9 分别约 0.1025s 就绪；RTX 4070 Ti / Direct3D11，GPU 计时 unavailable。聚焦 XML 仍为 1/1 PASS。
- 全量证据 `tools/results/d16_full_after_d3_d4_d9_readiness_20260812.xml` **54/54 PASS**；工具测试 71/71，两个工程构建 0 warning / 0 error。
- 该轮只关闭 A01 本机启动链可观测子缺口；目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线证据仍缺，A01 保持 IN_PROGRESS，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 D16 六项当前快照

| 任务 | 当前状态 | 当前权威事实 |
|---|---|---|
| D16-A01 | IN_PROGRESS | Unity D3D11 全量 54/54；233 samples，平均 59.98 FPS，最低 57.50 FPS；目标设备/真实大场景/最终资源/GPU/内存/长线证据缺失 |
| D16-A02 | IN_PROGRESS | runner 11/11、故障中止 6/6、100 次长线、304 用例矩阵 10/10；物理断电/跨平台证据缺失 |
| D16-A03 | IN_PROGRESS | 纯逻辑 7/7、Unity 全量 54/54；真机、目标分辨率、人工视觉/焦点和长线证据缺失 |
| D16-A04 | IN_PROGRESS | 3,139 条记录，结构错误 0，Pending 267；需人工逐条审查 |
| D16-A05 | IN_PROGRESS | 2,918 visual，2,264 placeholder，声明路径断链 0；最终授权资源缺失 |
| D16-A06 | IN_PROGRESS | 本机技术准备 19/19，发布清单 blocked，7 条项目级阻断；Steam 未执行 |

本快照不启动 D16-S01/S02。完整长线验收必须等待六项真实门禁全部通过，不能用本机自动化绿灯替代外部设备、人工审查或最终资源。

## 2026-08-12 纠正入口后的当前证据补充

- A01：硬件基线与 InitialHarbor 静态基线已用正确参数重新生成；本机仅 20 logical CPU / 1 GPU，场景静态扫描为 24 GameObjects / 17 MeshRenderers / 0 静态 Collider。静态结果不等于运行时物理完成。
- A02：档案矩阵用 `d16_a02_save_tests_after_sidecar_rollback_20260812.xml` 重新校验，304/304 用例、10/10 类别、31/31 精确匹配；100 次长线循环 PASS，SQLite 完整性正常，备份 399。
- D10/Save.Tests：Unity 重新生成派生 Test Runner/UI 程序集后，Save.Tests 恢复为 0 warning / 0 error；相关说明已写入 D10 主文档、D10-A02 交接和当日记忆。

这些增量只加强本机证据链和记录一致性，不改变六项门禁的总体状态：D16-A01—A06 仍未全部 PASS，D16-S01/S02 与 D17 正式验收继续不启动。

## 2026-08-12 D17-A03 上下船安全点本机回归补充

- D2ControlRouter 上船优先使用 D7_HelmPoint；下船只接受 BeachGround、DockGround、GangplankGround，找不到安全行走面时拒绝下船并保持船上控制。
- tools/results/d17_a03_boarding_safe_surface_current_20260812.xml：1/1 PASS；
- tools/results/d17_a03_safe_disembark_refusal_current_20260812.xml：1/1 PASS；
- tools/results/d16_full_after_safe_disembark_20260812.xml：Unity PlayMode 56/56 PASS；
- tools/results/d17_a03_d5_smoke_after_safe_disembark_20260812.log：InitialHarbor verified；
- 工具回归：71/71 PASS。

上述证据只增强本机 D17-A03 基线，不关闭 D16-A01—A06 的目标设备、人工审核、最终资源、跨平台存档和原生崩溃/隐私门禁。

## 2026-08-12 D17-A04 手柄镜头本机回归

- CameraFollow 已接入 Gamepad 右摇杆；D7/D16 模态输入锁开启时，镜头 yaw 不变化，锁解除后镜头方向恢复响应。
- 专项 tools/results/d17_a04_gamepad_camera_current_20260812.xml：1/1 PASS；
- 当前 Unity 6 PlayMode 全量 tools/results/d17_a04_full_after_gamepad_camera_20260812.xml：57/57 PASS；
- D5 烟测 tools/results/d17_a04_d5_smoke_after_gamepad_camera_20260812.log：Camera check、Physics check、D7 Smoke 和 InitialHarbor verified 均出现；
- Assembly-CSharp 与 D16.PlayMode.Tests：0 warning / 0 error。

该增量只证明本机合成手柄和代码路径；A03 真实物理手柄、目标分辨率、人工焦点/可访问性和长线证据仍缺，D16-S01/S02 不启动。

## 2026-08-12 D17-A06 失败恢复本机回归

- 交接文档：`docs/game/design/common/handoffs/D17/D17-A06_FAILURE_RECOVERY_HANDOFF.md`；
- 新增 PlayMode 回归覆盖“新建→进入港口→ESC 暂停→返回主菜单→再次新建→再次进入港口”；
- 聚焦测试 1/1 PASS；全量 `tools/results/d17_full_after_failure_recovery_20260812.xml` **58/58 PASS**；
- 仅证明当前 Windows 工作区的代码级生命周期恢复成立，不关闭 D16 外部真实性门禁或 D17-S02 正式连续验收。

## 2026-08-12 D17-A02 动作状态链本机回归

- `game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs`：依赖组件尚未完成挂载时保持待机，不把生命周期空窗误判为跳跃；
- 新增 `game/Assets/Tests/PlayMode/D17ActionQualityPlayModeTests.cs`，覆盖 `Idle → Walk → Run → Jump → Boarding → Disembarking`；
- 专项 `tools/results/d17_a02_action_quality_current_20260812.xml`：**1/1 PASS**；
- 当前 Unity 6 PlayMode 全量 `tools/results/d17_full_after_action_quality_20260812.xml`：**59/59 PASS**，0 failed / 0 inconclusive / 0 skipped；
- `Assembly-CSharp`：0 warning / 0 error。

该证据只关闭 D17-A02 的本机动作状态链子门禁；真人最终动画资源、动作捕捉质量、目标设备、人工操作和 D17-S02 仍未验收。D16-A01—A06 仍为 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 当前最终一致性快照

- 本机自动化基线：Unity 6 D17 连续流程 **60/60 PASS**，工具测试 **77/77 PASS**，关键 C# 工程构建 0 warning / 0 error；
- D16-A04：3,139 条登记、结构错误 0、Pending 267，人工审查仍未完成；
- D16-A05：2,918 条可视登记、654 integrated、2,264 placeholder，最终授权资源仍未接入；
- D16-A06：本机技术准备 19/19 true，发布清单仍有 7 条项目级阻断；
- 外部门禁审计：`tools/results/d16_external_gate_audit_current_20260812.json` 为 **BLOCKED**，A01—A06 六个真实证据文件均缺失；
- D16-S01 继续 `BLOCKED / NOT STARTED`，D16-S02 继续 `BLOCKED BY S01`，不以本机垂直切片回归替代正式验收。

## 2026-08-12 D16-R02 当前事实刷新

- 早期 R02 交接中的“正式 installer 缺失”属于历史检查点；当前正式便携安装器已存在于 `artifacts/d16_a06_formal_installer/WhalesIslandReefSetup.exe`。
- `tools/results/d16_a06_formal_installer_smoke_result.json` 已实际记录首次安装、非法更新拒绝并保留旧安装/用户存档、合法更新、卸载和用户存档保留均通过。
- R02 当前准确状态：**本机安装器子门禁 PASS，任务总体 PARTIAL / IN_PROGRESS**。A06 仍缺目标机器兼容性、原生崩溃审查和最终人工隐私复核；发布清单 7 条阻断与 Steam 未授权政策保持不变。

## 2026-08-12 A03 物理设备预检

- 当前 Windows PnP 实际枚举到 `Xbox Wireless Controller` 和 `Bluetooth LE XINPUT compatible input device`，屏幕配置为 `3840x2160`，显卡为 `NVIDIA GeForce RTX 4070 Ti`。
- 预检文件：`tools/results/d16_a03_device_probe_current.json` 与 `tools/results/d16_a03_device_probe_raw_current.json`。
- 预检状态保持 `IN_PROGRESS/PENDING_MANUAL`：没有发送按钮输入，没有生成 PASS，也没有替代真机焦点、字体、色觉、低刺激和目标分辨率人工验收。
- 原始预检的 XInput 状态为 `NO_ACTIVE_XINPUT_USER`，API 可用但用户 0—3 均返回 `1167`；PnP 枚举到手柄不等于游戏已收到活动手柄输入，A03 仍需实际唤醒/操作设备并留存人工证据。

## 2026-08-13 当前门禁刷新

- 工具全量回归：**79/79 PASS**；A04/A05/A06 只读快照已刷新为 `tools/results/d16_a04_review_audit_current_20260813.json`、`tools/results/d16_a05_asset_audit_current_20260813.json`、`tools/results/d16_a06_release_audit_current_20260813.json`。
- 当前机器事实：A04 Pending 267、A05 placeholder 2,264、A06 本机技术准备 19/19；A03 XInput API 可用但用户 0—3 均返回 1167，仍未形成真机 PASS。
- 六个外部 evidence envelope 仍缺失，D16-S01 继续 `BLOCKED / NOT STARTED`，D16-S02 继续等待 S01；没有启动正式 D17 或 D18+。

## 2026-08-13 A04 全量逐条复核材料

- 新增只读 `tools/d16_a04_review_packet.py`，生成 `tools/results/d16_a04_review_packet_current_20260813.json` 和 `docs/game/design/common/D16_A04_REVIEW_PACKET_CURRENT.md`。
- 当前包覆盖 267 条 Pending、12 批，状态 `HUMAN_DECISION_REQUIRED`；引用缺口 0，267/267 条具有本地文件型复核入口。
- 专项测试 4/4 PASS；所有决定模板仍为 Pending，没有自动批准、Registry 写入或历史/授权/敏感内容字段修改。
- A04 继续 IN_PROGRESS，D16-S01/S02 仍等待真实责任审核和其余外部门禁。

### 2026-08-13 复核包全量回归

- 工具全量回归 **96/96 PASS**，A04 复核包专项 4/4 PASS、D16-S01 启动预检专项 4/4 PASS，格式检查通过。
- A04 数字保持 3,139 / 2,918 授权 / Pending 267 / 结构错误 0；A05 仍 2,264 placeholder；A06 本机技术准备 19/19。
- 外部门禁仍 BLOCKED；没有新的人工决定、最终资源、目标设备或 Steam 证据，D16-S01/S02 不启动。

## 2026-08-13 D16-S01 启动前预检

- 新增只读 `tools/d16_s01_readiness.py`，当前结果 `tools/results/d16_s01_readiness_current_20260813.json` 为 `BLOCKED`。
- 预检明确列出六项 external gate 缺失、A04 Pending 267、A05 placeholder 2,264；不会启动故事或改变任何资产/审核状态。
- 专项测试 4/4 PASS；Steam 上传仍未授权，D16-S01/S02 继续未启动。

## 2026-08-13 A05 真实候选包留痕

- Kenney CC0 候选包：`docs/game/design/common/D16_A05_KENNEY_CANDIDATE_PACKET_01.md`；机器清单：`tools/results/d16_a05_candidate_packet_01_20260813.json`。
- 候选目录：`game/Assets/Art/Candidates/Kenney/`；状态 `CANDIDATE_PENDING_REVIEW`，人工决定 `Pending`，没有 Registry 或 `asset_state` 变更。
- 候选专项 2/2、工具全量 98/98 通过；这只是 A05 来源准备，最终资源门禁、A04 人工审查和 D16 外部门禁仍未关闭。
- Unity 导入级复核 `tools/results/d16_a05_candidate_import_current_20260813.json` 为 7/7 PASS；候选包/导入专项合计 4/4 PASS。未修改 Registry、Prefab、场景引用或 `asset_state`。

### 2026-08-13 最终本机验证

- 工具全量 **100/100 PASS**，Unity 候选导入 **7/7 PASS**，`git diff --check` 通过。
- A05 仍为 2,264 placeholder，A04 仍 Pending 267；外部门禁仍 BLOCKED，D16-S01/S02、正式 D17 和 D18+ 未完成。


## 2026-08-18 D16-C11 本轮增量节（S02交接与D16章节放行决策）

- **章节状态审定**:
  - `D16-C01` 至 `D16-C08` 已全部达成 `GATE_PASS`（包含 A04 267 条 Pending 清零、A05 方案 B 垂直切片 4 项基础资源本地接入与预检通过）；
  - `D16-C12` 旁路任务已达成 `GATE_PASS`（安装器烟测通过、S01 夹具就绪、Steam 保持未授权未执行）；
  - `D16-C09` 与 `D16-C10` 状态为 `GATE_BLOCKED`（因缺少外部真实目标机 60 秒性能采样、物理断电双平台恢复、物理手柄无障碍签字与目标发布材料，S01 长线故事未执行）。
- **可发布与不可发布边界**:
  - **可发布/本地可运行部分**: 本地垂直切片（4 项基础资产、Gameplay/Systems 逻辑、SaveSystem 基础恢复、开发机运行环境）具备本地可测性；
  - **严禁外部发布部分**: 严禁执行 Steam 商店上传与外部公开发行，严禁宣称已具备生产级全设备性能通过。
- **剩余风险与回滚路径**:
  - 剩余风险：真实低端机卡顿、极端物理断电数据丢失、手柄兼容性盲区；
  - 回滚路径：若切片资产出现异常，回退至 ContentRegistry placeholder 原始备份。
- **D17 放行裁决**:
  - 根据协议，D17-C01 属于后续阶段工作，因 D16 外部硬件证据未齐，D16 章节整体放行结论为：`BLOCKED_PENDING_EXTERNAL_HARDWARE`。
- **开发阶段推迟**（2026-08-18）: 真机四门与 S01 正式长线暂不列入开发必做；C09/C10/C11 保持 `GATE_BLOCKED`。见 [D16_EXTERNAL_HARDWARE_DEFERRED.md](D16_EXTERNAL_HARDWARE_DEFERRED.md)。
