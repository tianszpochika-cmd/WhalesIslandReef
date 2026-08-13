# D16 Unity 6 运行时验收报告

## 结论

截至 2026-08-11，D16 的 InitialHarbor 运行时接线已取得真实 Unity 6 PlayMode 证据，但 D16 全阶段仍未完成。当前结论为：**运行时基础烟测 PASS；性能目标、故障注入、真机无障碍、最终资产和发布门禁仍 IN_PROGRESS/BLOCKED**。

## 环境与命令

- Unity：`6000.0.80f1`
- 测试方式：Unity Test Runner `PlayMode`
- 测试程序集：`D16.PlayMode.Tests`
- 基础 smoke XML：`tools/results/d16_runtime_playmode_test_results_retry8.xml`
- A01 生产回归 XML：`tools/results/d16_a01_production_test_results.xml`
- 运行时 JSON：`tools/results/d16_runtime_playmode_report.json`
- 基础 smoke 日志：`tools/results/d16_runtime_playmode_unity6_retry8.log`
- A01 生产回归日志：`tools/results/d16_a01_production_unity6.log`

基础 PlayMode 烟测为 **1/1 PASS**；包含 A01 夹具的最新生产回归为 **2/2 PASS**。同一轮还确认 D2—D5 运行时入口、D6 港口碰撞、D7 音频/角色表现和 D16 性能入口能够共同启动。

## 已证实的运行时事实

- `InitialHarbor` 成为活动场景；相机、`D2_PlayerAvatar` 和 `ph_ship_fishing` 均存在。
- `D16WorldPerformanceController` 和 `D16RuntimePerformanceProbe` 已创建。
- 默认表现登记数量为 3，预算状态满足；探针产生 3 个采样。
- InitialHarbor 生产入口已自动登记 `harbor.sea` 与 `harbor.beach` 两个表现流式单元；基础烟测报告 `LoadedCellCount=2`。
- A01 运行时夹具报告 `tools/results/d16_a01_runtime_fixture_report.json`：流式加载/卸载、LOD 降级/恢复、碰撞保持启用和对象池复用全部通过。
- 日志确认玩家碰撞体已配置，并由 `BeachGround` 落地；Animator 资源已加载，角色中立姿态与视觉落地校准完成。
- D16-A03 运行时文字设置已应用：`locale=zh-CN`、`textScale=1`、字幕开启。
- Unity 6 的 `Arial.ttf` 运行时异常已修复为 `LegacyRuntime.ttf`，PlayMode 测试不再因该异常日志失败。

## 性能数据的边界

本次使用 `-nographics` 的 Unity Editor PlayMode，报告中的 `GraphicsDevice` 为 `Null Device`，GPU 计时不可用；因此报告只能证明运行时接线和探针采样，不可作为最终 CPU/GPU/FPS 性能目标通过证据。当前仍需在目标 Windows 配置和代表场景中完成：

- CPU/GPU/内存/GC/加载时间基线；
- 分区流式切换、LOD、对象池、音频资源释放和低端档降级；
- 同屏 Agent/动物数量上限及长时间稳定性。

## 其他回归证据

- Unity 6 EditMode：`D2/D3 EditMode tests: passed=291, failed=0, skipped=0`。
- 结果 XML：`game/D2_TestResults.xml`。
- AgentCore、Content、D16 Gameplay、Systems、Save.Tests、PlayMode.Tests、Assembly-CSharp 和 Assembly-CSharp-Editor 均已串行构建，0 warning / 0 error。
- D16-A01 纯预算测试 5/5、D16-A02 存档安全测试 5/5、D16-A03 可访问性测试 5/5。
- D15-S02：`batches=8 samples=24 errors=0 warnings=0`；D15 20 项 Python 回归全部通过。

## 尚未关闭的门禁

1. D16-A01：真实目标设备性能、流式/LOD/对象池和长线采样。
2. D16-A02：断电/进程中止、D7—D14 全档案矩阵、跨平台恢复和重复结算演练。
3. D16-A03：真实手柄、重绑定 UI、字幕触发、字体回退、色觉/低刺激场景抽样。
4. D16-A04/A05：488 个结构缺口、40 条 rights 缺口、2262 条 placeholder 和 2 条 integrated 测试元数据缺口。
5. D16-A06：发布 manifest、Unity 目标构建、安装/卸载/覆盖更新与商店授权；Steam 上传尚未执行。
6. D16-S01/S02：六个 A 门禁关闭后，才进行连续长线验收和最终发布决策。

## 下一步执行顺序

先补 D16-A01 的目标设备性能与场景流式证据，再做 A02 故障注入和完整存档矩阵，随后补 A03 真机输入/字幕/可访问性；A04—A06 的静态缺口闭合后进入 D16-S01 长线验收，最后生成 D16-S02 最终交接。

## 2026-08-11 D16-A03 最新回归

D16-A03 已从“只有设置字段/桥接”推进到“玩家可实际操作的设置入口和模态面板”，但总体状态仍为 **IN_PROGRESS**：

- 主菜单设置与 InitialHarbor 暂停菜单均可打开无障碍/键位面板；面板打开时锁定玩法输入，关闭时恢复。
- 键盘/手柄重绑定捕获、冲突拒绝、默认重置和即时保存已接入契约校验；运行时已消费文字缩放、字体回退、高对比度、色觉辅助、低刺激、减弱动态和字幕显示设置。
- Unity 6 正式 PlayMode `D16.PlayMode.Tests` **3/3 PASS**，新增真实 UI 点击链测试；Unity 6 EditMode 全量回归 **292/292 PASS**；D16-A03 纯逻辑回归 **6/6 PASS**。

本次证据：`tools/results/d16_a03_playmode_test_results.xml`、`tools/results/d16_a03_playmode_unity6.log`、`tools/results/d16_a03_editmode_unity6.log`、`tools/d16_a03_runner/`。这些证据只关闭了 A03 的基础运行时接线子门禁，仍不能替代物理手柄/目标设备测试、实际音频 cue→字幕触发、字体覆盖、长文本/最小窗口截图、色觉与低刺激场景审查。

## 2026-08-11 本机图形设备与持续窗口

- Unity 6 非 `-nographics` PlayMode `D16.PlayMode.Tests` **4/4 PASS**；XML/日志：`tools/results/d16_a01_performance_window_playmode_test_results.xml`、`tools/results/d16_a01_performance_window_playmode_unity6.log`。
- A01 持续采样窗口约 8 秒、32 samples，RTX 4070 Ti，平均 FPS 59.98、最低 FPS 59.72、平均帧时间 16.67 ms、平均主线程 4.01 ms、预算满足、保护对象超预算 0；报告：`tools/results/d16_a01_performance_window_report.json`。
- FrameTiming 未提供 GPU 毫秒，目标/低端设备性能、最终资产 LOD、音频释放和更长时间稳定性仍未关闭。
- A03 的 D9 cue→字幕代码接线已完成，但没有把本轮 UI/基础场景 PlayMode 误写为 14 个事件逐条字幕验收；事件抽样和真实设备仍待执行。

最终全量 EditMode 回归：**292/292 PASS**，日志 `tools/results/d16_final_editmode_unity6.log`，结果 XML `game/D2_TestResults.xml`。本次回归后仍保留 A01/A02/A03 的真实设备、长线和事件抽样门禁，以及 A04—A06 的人工/发布门禁。

## 2026-08-12 当前 Unity 6 全量 PlayMode 复验

- 使用 Unity `6000.0.80f1`、图形 PlayMode、`-assemblyNames D16.PlayMode.Tests` 执行当前工作区全量测试；测试完成后由 Unity Test Runner 自然退出，未使用会提前终止测试的 `-quit`。
- 原始结果：`tools/results/d16_full_playmode_current_20260812.xml`；日志：`tools/results/d16_full_playmode_current_20260812.log`。
- XML 统计：**53/53 Passed，0 Failed，0 Skipped，0 Inconclusive**；日志包含 `Test run completed. Exiting with code 0 (Ok). Run completed.`。
- 本次也验证了先前无 XML 的原因：`-quit` 会在测试运行器启动前结束编辑器；无图形全量进程需等待自然结束，不能按命令返回瞬间判断结果。

该证据只证明当前 Unity 6 代码级 PlayMode 回归稳定，不关闭 D16-A01—A06 的目标设备、物理断电/跨平台、真机可访问性、人工历史/授权、最终资源、原生崩溃/隐私等外部门禁；D16-S01/S02 继续保持未启动。

最新 A03 代表性事件链已通过：`event_d9_crew_report` → `crew_report` 音频 cue → 字幕“船员报告”；证据 `tools/results/d16_a03_d9_subtitle_runtime_report.json`。Unity 6 PlayMode 现为 **5/5 PASS**，但 14 个 cue 全量、物理手柄和目标设备仍未完成。

## 2026-08-11 A02 Windows 强制终止与全量 EditMode 更新

- `tools/results/d16_a02_fault_injection_result.json`：受控退出 3/3，Windows 父进程强制终止 3/3；flush 后旧文件保留、replace 后新文件完整且无临时文件、迁移提交前 SQLite 完整并回滚未提交 v3 写入。
- `tools/results/d16_a02_long_run_editmode_unity6.xml`：Unity 6 EditMode **296/296 PASS**。
- `tools/results/d16_a02_archive_matrix_result.json`：基于上述 XML 的 10/10 档案类别覆盖，0 failed/inconclusive。
- 本轮修复了一个 Unity 6 Editor 测试初始化缺口：临时 `AddComponent` 不保证派发私有 `Awake`；只在测试中显式初始化 D9 HUD/音频组件，不改变运行时路径。

这只推进了 A02 的 Windows 进程终止子门禁；物理断电、跨平台、长线存档和 D16-S01 仍未通过。D16 全阶段继续保持 **IN_PROGRESS**。

## 2026-08-11 A02 长线存档更新

- 发现并修复 `NewGame` 重置旧 v3 投影后留下空 `save_metadata` 表、导致后续新档迁移返回 `metadata_incomplete` 的真实缺陷。
- `python tools/d16_a02_long_run.py 100` 通过：100 次新游戏/保存/读取/迁移/重复迁移/恢复，最终 SQLite 完整性为 `ok`，最终玩家事实正确，备份数 399。
- 修复后 Windows 强制终止故障注入仍为 3/3，独立存档安全 runner 仍为 5/5。

这推进了 A02 的本地重复迁移和有限长线压力子门禁，但物理断电、跨平台和 D16-S01 完整链路仍未完成。

修复 `SaveSystem.NewGame` 后重新执行 Unity 6 `D16.PlayMode.Tests`：`tools/results/d16_a02_long_run_playmode_unity6.xml` 为 **5/5 PASS**，InitialHarbor、流式/性能夹具和 D9 事件→音频→字幕代表链均未回归。

## 2026-08-11 A01 本机长时性能窗口

- `D16A01RuntimePerformanceWindowSamplesContinuously` 已提升为 60 秒连续采样，实际运行 **60.005 秒 / 234 samples**；Unity 6 `D16.PlayMode.Tests` 最新 XML `tools/results/d16_a01_long_window_playmode_unity6.xml` 为 **5/5 PASS**。
- 本机 RTX 4070 Ti：平均 FPS **59.96**、最低 FPS **54.88**、平均帧时间 **16.68 ms**、平均主线程 **3.67 ms**、预算满足、保护对象超预算 **0**；报告 `tools/results/d16_a01_performance_window_report.json`。
- 本机内存峰值已记录，Unity FrameTiming 仍无 GPU 毫秒；因此 A01 只关闭本机长时采样子门禁，不关闭目标/低端设备、真实大场景、最终资产 LOD、音频释放和 D16-S01 长线门禁。

## 2026-08-11 A03 D9 14 cue 全量抽样

- 新增真实 PlayMode 覆盖：逐条追加 14 个冻结 D9 事件 ID，验证共享 EventLog → `D9AudioPresentationController` → 字幕 overlay 的链路。
- `tools/results/d16_a03_d9_cue_coverage_report.json`：14/14 cue 通过，所有样本均有预期 `ActualCue`、`SubtitleVisible=true` 和非空本地化文本。
- Unity 6 图形 PlayMode 最新 `tools/results/d16_a03_full_cue_playmode_unity6_retry2.xml` 为 **6/6 PASS**，日志 `tools/results/d16_a03_full_cue_playmode_unity6_retry2.log`。
- 这关闭了 A03 的 14 cue 运行时抽样子门禁；真实物理手柄/目标设备、字体覆盖与长文本截图、色觉/低刺激审查、多场景暂停焦点和长线仍未关闭，A03 继续保持 IN_PROGRESS。

## 2026-08-11 A05 登记元数据修复

- 两个已标记 integrated 的 D15 资源登记补齐 `test_cases`：道具占位资源覆盖加载/模型材质碰撞/存档恢复，服装占位资源覆盖加载/授权船员引用/存档恢复。
- A05 审计最新为 `integrated_missing_visual_metadata_count=0`、非法状态 0；占位统计仍为 2262，故 A05 仍不标记 PASS。

## 2026-08-11 A04 结构与授权审计收口

- A04 最新审计：2590/2590 可视登记均有授权记录，结构错误 0，Pending 历史审查 112，Rejected 0；报告 `tools/results/d16_a04_review_audit.json`。
- 对缺失字段的 112 条条目统一保留 `historical_review=Pending`，来源引用只作为候选复核材料，未把任何未核验历史事实写成 Approved。
- A04 机器子门禁已关闭，但人工历史/敏感表现复核仍未完成，D16 总体继续 IN_PROGRESS。

## 2026-08-11 A06 Windows 构建与启动烟测

- Unity 6 StandaloneWindows64 构建成功，证据 `tools/results/d16_a06_windows_build_unity6.log`；构建产物目录为 `artifacts/d16_a06_windows_build/`。
- 构建 Player 使用 `-batchmode -nographics` 启动 15 秒，主菜单 EventSystem、UI 初始化和 D16 设置应用均出现，日志 `tools/results/d16_a06_windows_player_smoke.log` 未发现 Error/Exception/Crash/No cameras。
- 该证据只关闭 A06 的构建/启动子门禁；安装/卸载/覆盖更新、文件级存档恢复、隐私/崩溃和 Steam 仍未验收。

- `tools/results/d16_a06_release_audit.json` 已消费构建日志和 exe 证据，`unity_windows_build_evidence=true`；A06 当前只保留安装/卸载/更新未实测与 Steam 未授权阻断。

- 工作区 staging 安装/更新/卸载烟测 `tools/results/d16_a06_install_update_smoke_result.json` 通过，更新和卸载均保留 save 哨兵；这不是 OS installer，因此 A06 仍等待正式 installer/update 流程。

## 2026-08-11 A04 全项目源记录复核

- A04 全项目审计 `tools/results/d16_a04_review_audit.json`：3139 条记录、91 条敏感记录、2916 条可视资源，2916/2916 权利记录闭合，结构错误 0，Pending 278，Rejected 0。
- 其中 166 条 D15-A06 源目录副本已补齐 Pending 审查结构；源目录的外部船占位同步补齐显式 Quaternius CC0 authorization。
- 这只关闭了机器结构/权利字段子门禁；人工历史事实、敏感内容表现和警告仍须逐条核验，A04 继续 IN_PROGRESS。

## 2026-08-11 最终 EditMode 回归

- Unity 6 当前工作区全量 EditMode：`tools/results/d16_final_editmode_unity6.xml`，296/296 PASS，失败 0，不可确定 0，耗时约 12.8 秒。
- 工具回归：`python -m unittest discover -s tools/tests -p 'test_*.py'`，44/44 PASS。

## 2026-08-11 D16-A01—A06 证据刷新

本轮按 D16 顺序复验了可在当前 Windows 工作区完成的工程与 QA 门禁：

- A01 静态场景基线重新生成；本机 60 秒图形窗口证据仍为 234 samples、平均 59.96 FPS、最低 54.88 FPS、预算满足；新增高负载 PlayMode 7/7，267 候选按 Agent/动物/音频上限降级。目标/低端设备、最终资产规模和 GPU 毫秒仍未取得。
- A02 `Save.Tests`、D16-A02 runner、D16-A03 runner 构建均为 0 warning / 0 error；存档安全 runner 5/5、故障注入（受控退出与 Windows 强制终止）6/6、100 次长线存档循环通过；物理断电、跨平台和完整 D16-S01 仍未关闭。
- A03 纯逻辑 runner 6/6；14 cue 运行时抽样和现有 Unity 回归证据保留；真实手柄/目标设备、字体覆盖/长文本、色觉/低刺激和多场景焦点仍未关闭。
- A04 全项目审计：3139 条记录、2916 条可视资源、2916/2916 权利记录、结构错误 0、Pending 278、Rejected 0；仅剩人工历史/敏感内容复核。
- A05 审计：2590 条可视登记、328 integrated、2262 placeholder、非法元数据 0；placeholder 仍然阻断最终表现。
- A06 正确项目根目录下的发布审计静态证据完整，Windows 构建、workspace staging 烟测和 `d16_a06_install_update.ps1` 的 Install/Update/Uninstall 均通过；签名 OS 安装包/注册表权限集成未验收，Steam 未授权且未执行。

因此 D16-A01—A06 仍不能整体标记 PASS；本轮只关闭了 A04 的结构字段子门禁，并刷新了可复核证据。

## 2026-08-12 A01 启动阶段归因复验

- D5 Runtime 新增六阶段只读耗时，并由 D16 性能探针纳入窗口报告；没有改变玩法、存档或领域数据所有权。
- `tools/results/d16_a01_phase_attribution_current_unity6.xml` 聚焦 **1/1 PASS**；`tools/results/d16_full_after_phase_attribution_20260812.xml` D16 PlayMode **54/54 PASS**。
- 最新本机窗口保留真实异常：234 samples、平均 65.71 FPS、最低 3.49 FPS、最长全帧 286.43ms；阶段计时 6/6 完整，但长帧尚未完全归因，目标/低端设备和最终资源证据仍缺。

## 2026-08-12 A01 D2 启动阶段归因复验

- `D2RuntimeBootstrap` 六阶段只读计时已接入 D16 性能报告；`tools/results/d16_a01_d2_phase_attribution_current_unity6.xml` 聚焦 **1/1 PASS**。
- 当前窗口：235 samples、平均 65.05 FPS、最低 3.56 FPS、最长全帧 280.86ms；D2 总构建 165.364ms，六阶段完整；D5 六阶段也完整。
- 本轮只补齐本机启动观测，长帧仍未完全归因；目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线门禁继续未完成。

## 2026-08-12 A01 D3/D4/D9 启动链就绪归因

- 新增 D3/D4/D9 `IsReady` 只读观测，性能窗口要求三者均有非负就绪时间；未改变任何领域状态或存档写者。
- 聚焦 `tools/results/d16_a01_d3_d4_d9_readiness_graphics_current_unity6.xml` **1/1 PASS**；D3/D4/D9 均约 0.3447s 就绪。
- 全量回归后的当前图形窗口：233 samples、平均 59.98 FPS、最低 57.50 FPS、最长帧 181.47ms；D3/D4/D9 分别约 0.1025s 就绪；RTX 4070 Ti / Direct3D11；GPU 计时 unavailable。聚焦 XML 仍为 1/1 PASS。
- 全量 `tools/results/d16_full_after_d3_d4_d9_readiness_20260812.xml` **54/54 PASS**；A01 报告校验 2/2；工具测试 71/71；相关工程构建 0 warning / 0 error。
- 该结果只关闭本机启动链观测子门禁，不能替代目标/低端设备、真实大场景、最终资源和长线性能证据；A01 仍 IN_PROGRESS。

## 2026-08-12 本机上下船安全点回归

- D17-A03 上下船安全点专项：tools/results/d17_a03_boarding_safe_surface_current_20260812.xml 为 1/1 PASS；
- 无安全行走面拒绝下船专项：tools/results/d17_a03_safe_disembark_refusal_current_20260812.xml 为 1/1 PASS；
- 修改后的 Unity PlayMode 全量：tools/results/d16_full_after_safe_disembark_20260812.xml 为 56/56 PASS；
- 本轮工具回归为 71/71 PASS。

该结果是本机代码回归证据，不替代目标设备性能、真机输入、人工可访问性、最终资源、断电/跨平台存档或 D16-S01/S02。

## 2026-08-12 D17-A04 手柄镜头回归

- CameraFollow 新增 Gamepad 右摇杆旋转；模态输入锁开启时镜头不响应，解除后恢复。
- tools/results/d17_a04_gamepad_camera_current_20260812.xml：1/1 PASS；
- tools/results/d17_a04_full_after_gamepad_camera_20260812.xml：PlayMode 全量 57/57 PASS；
- tools/results/d17_a04_d5_smoke_after_gamepad_camera_20260812.log：InitialHarbor verified；
- Assembly-CSharp.csproj、D16.PlayMode.Tests.csproj：0 warning / 0 error。

本机证据不替代真实手柄、目标设备/分辨率、人工焦点与可访问性、最终资产或 D17-S02 连续验收；D16-A03 和 D16-S01/S02 状态不变。

## 2026-08-12 D17-A06 失败恢复链回归

- 新增 `D17FailureRecoveryPlayModeTests`，从主菜单真实新建游戏开始，覆盖暂停、返回主菜单、重新新建游戏和二次港口运行时恢复；
- `D17A06PauseReturnMainMenuAndStartNewGameRestoresRuntime` 聚焦结果：1/1 PASS；
- 当前 Unity 6 PlayMode 全量 `tools/results/d17_full_after_failure_recovery_20260812.xml`：**58/58 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 该结果只关闭 D17-A06 的本机代码级生命周期子门禁；D16-A01—A06 的目标设备、人工审核、最终资源、跨平台/断电、native crash/privacy 和 Steam 授权门禁不变，D16-S01/S02 仍未启动。

## 2026-08-12 D17-A02 动作状态链回归

- `tools/results/d17_a02_action_quality_current_20260812.xml`：动作专项 **1/1 PASS**；
- `tools/results/d17_full_after_action_quality_20260812.xml`：Unity 6 PlayMode **59/59 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 覆盖真实 InitialHarbor 玩家状态 `Idle → Walk → Run → Jump → Boarding → Disembarking`；
- 修正动画控制器在物理组件延迟挂载期间的状态默认值，避免把接线竞态显示成跳跃姿态。

这只是 D17 异步表现准备和本机代码证据，不关闭 D16-A01—A06 的目标设备、最终授权资源、人工审查、断电/跨平台、原生崩溃/隐私门禁，也不启动 D16-S01/S02。
