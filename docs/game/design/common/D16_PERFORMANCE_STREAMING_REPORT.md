# D16-A01 性能、分区流式与模拟预算报告

## 结论

状态：**IN_PROGRESS，已进入实现和运行时性能验收，完整门禁未通过**。

D15-S02 已通过，D16-A01 已取得分区表现流式、LOD/对象池夹具和本机 60 秒性能窗口证据；目标/低端设备、真实大场景、最终资产 LOD、音频释放和长线门禁仍未通过。本文件不把局部证据扩大为 A01 总 PASS。

## 门禁证据

- D15-S02 主报告：docs/game/design/common/D15_BATCH_CONTENT_QA_REPORT.md。
- 当前统一复核命令：
  - python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json
  - 历史结果：D15_S02_QA_EXIT=1；当前顺序复验结果：D15-S02 PASS（0 errors、0 warnings）。
- 当前已记录的阻断类别：
  1. plant_red_mangrove、plant_shallow_seaweed 生境字段不一致；
  2. D15-A06 资源审查/LOD 字段缺失；
  3. D15-A07 宠物外观存在 18 次对 12 个未定义 ID 的引用；
  4. D15-A08 任务/音频缺少测试、LOD-N/A 或资源状态字段；
  5. A01—A07 的 2536 个 display key 没有可验证的中英本地化包证明；
  6. Save.Tests 被既有 D10CrewBehaviorState 缺失阻断。
- 另外，24 个 D15 抽样中有 9 个没有明确的 save/restore 标记，不能将目录 JSON 往返当作运行时存档恢复验收。

## 已读取的性能相关基线

- game/Assets/Scripts/Systems/D5/D5PerformanceSystem.cs：已有 1080p/60 目标、帧时间采样、平均/最低 FPS 报告；没有 D16 分区流式、LOD、对象池、模拟等级、动物活跃上限或加载/GC 指标。
- game/Assets/Scripts/Gameplay/D7/D7AudioController.cs：已有港口环境、脚步、上下船和船舶循环音频通道，缺少按分区/距离的卸载策略。
- game/Assets/Scripts/Gameplay/D9/D9AudioPresentation.cs：按事实事件和天气快照播放提示音，缺少 D16 音频资源生命周期和卸载指标。
- D15-A01 岛屿目录已声明 streaming cell、LOD 档位和保存键，但目录声明不等于 Unity 运行时流式性能实现。
- D15 冻结要求 LOD 不得删除交互、损伤分区、稳定 ID、事实事件或存档状态；性能降级只能改变表现或模拟频率。

## 本次没有做的工作

- 未修改 game/Assets/Scripts/Systems/ 或任何领域强逻辑；
- 未新增网络、LLM 或跨端私有协议；
- 未删除 D15 内容或更换内容质量状态；
- 未实现加载/卸载、对象池、LOD 切换、Agent 模拟等级、动物活跃上限和音频卸载；
- 未运行 Unity 场景性能基准，因此没有 CPU、GPU、内存、加载时间、GC、同屏 Agent/动物数量的可用结果；
- 未创建会改变世界、事件或存档事实的降级路径。

## 重新开工条件

1. D15-S02 责任任务关闭全部内容 QA 阻断，并使统一 QA 错误数为 0；
2. 修复 D10CrewBehaviorState 后通过 Save.Tests；
3. 对缺少保存标记的 9 个样本完成运行时保存、卸载、恢复和重复加载验证；
4. 由产品/技术负责人确认 D16-A01 可重新进入；
5. 重新开工后建立不改变强逻辑事实的性能测试场景，记录 CPU、GPU、内存、加载时间、GC、同屏 Agent/动物、LOD、对象池命中率和音频释放结果。

## 2026-08-11 门禁复验更新

D15-S02 已通过统一 QA（8 batches、24 samples、0 errors、0 warnings），D10 AgentCore/Systems/Save.Tests standalone build 也已通过。因此本任务的前置内容门禁已解除，当前状态更新为 **PENDING**，不是已完成。尚未实现或测量分区流式、LOD、对象池、模拟等级、动物上限、音频卸载和 CPU/GPU/内存/GC 基线；下一步从可重复的代表场景性能采样开始。

## 2026-08-11 D16-A01 实现进度更新

当前状态：**IN_PROGRESS，运行时实测尚未通过**。

本轮已落地的最小可运行基础：

- `game/Assets/Scripts/Systems/D16/D16PerformanceBudgetSystem.cs`：确定性表现预算裁决；按稳定 ID 排序，保护玩家/当前交互对象，限制 Agent、动物和音频数量；只返回预算建议，不写领域状态、事件或存档。
- `game/Assets/Scripts/Gameplay/D16/D16WorldPerformanceController.cs`：InitialHarbor 自动入口；低频刷新登记对象，按帧压力切换预算压力，登记当前玩家、船和港口船员；流式单元只切换 `PresentationRoot`。
- `game/Assets/Scripts/Gameplay/D16/D16PresentationRegistration.cs`：表现层 Animator、Renderer、LODGroup 和 AudioSource 降级；不关闭 Collider，不冻结强逻辑对象。
- `game/Assets/Scripts/Gameplay/D16/D16PresentationPool.cs`：只复用表现 prefab，并记录 Rent/Reuse/Return/Pooled 计数。
- `game/Assets/Scripts/Gameplay/D16/D16AudioResourceScope.cs`：按 scope 和显式资源前缀管理可释放音频；默认拒绝卸载可能被其他系统共享的 Resources 音频。
- `game/Assets/Scripts/Gameplay/D16/D16RuntimePerformanceProbe.cs`：记录 FPS、帧时间、FrameTiming CPU/GPU（若平台提供）、内存、预算数量、流式切换和对象池计数；未取得 GPU 数据时保留不可用状态，不以 CPU 时间冒充 GPU。

可复核证据：

- `dotnet build game/Whalebone.Systems.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；包含新增 Unity D16 脚本的静态编译检查；
- `dotnet run --project tools/d16_a01_runner/D16A01Runner.csproj --no-restore`：5 个纯预算测试通过；
- `python tools/d16_a01_static_baseline.py game/Assets/Scenes/InitialHarbor.unity --json-out tools/results/d16_a01_static_baseline.json`：静态场景清单为 24 个 GameObject、17 个 MeshRenderer；运行时 CPU/GPU/GC 字段明确为空；
- `python -m unittest tools.tests.test_d16_a01_static_baseline -v`：1/1 通过。

仍未宣称通过的内容：

- Unity Editor/Player 中的真实运行时采样、GPU 计时、加载/卸载耗时、GC 峰值和低端设备帧率；
- 实际配置 `D16WorldStreamCell` 的岛屿/海域场景分区；当前 InitialHarbor 没有被自动改造成会隐藏逻辑实体的流式单元；
- 具有真实 LODGroup 的最终资产抽样、动物数量上限在真实生态场景中的验收和音频资源实际卸载；
- Unity EditMode/PlayMode 结果。当前环境的 Unity batch runner 停在 `Licensing::Module`，未生成测试 XML，不能用静态编译替代运行时证据。

## 2026-08-11 Unity 6 A01 运行时证据更新

当前状态：**IN_PROGRESS（运行时闭环已通过，目标设备性能门禁仍未关闭）**。

本轮完成：

- `D16WorldStreamCell.ConfigureRuntime`：允许场景引导层安全配置表现流式单元；不创建、删除或修改领域实体。
- `D16WorldPerformanceController.ConfigureRuntimeFocus`：为运行时场景和验收夹具指定观察焦点；性能控制器仍只裁决表现预算。
- InitialHarbor 自动登记 `Sea` 与 `Beach` 为表现流式单元：`harbor.sea`（140/220m）和 `harbor.beach`（100/180m）。两者仅切换表现根，不承担领域状态或碰撞写入。
- `D16RuntimeAcceptanceTests.D16A01RuntimeFixtureExercisesStreamingLodAndPool`：真实 Unity 6 PlayMode 夹具覆盖近处加载、远处卸载、LOD 降级/恢复、碰撞保持启用和对象池复用。

可复核证据：

- Unity 6 PlayMode：`D16.PlayMode.Tests` 2/2 PASS；XML：`tools/results/d16_a01_production_test_results.xml`；日志：`tools/results/d16_a01_production_unity6.log`。
- 夹具报告：`tools/results/d16_a01_runtime_fixture_report.json`，`CellTransitionCount=2`、对象池 `Rent=2/Reuse=1/Return=2`，上述布尔验收项全部为 true。
- InitialHarbor 报告：`tools/results/d16_runtime_playmode_report.json`，`LoadedCellCount=2`、`RegistrationCount=3`、`SampleCount=3`、`BudgetSatisfied=true`。
- `dotnet build game/Assembly-CSharp.csproj --no-restore`：0 warning / 0 error。

仍未关闭的 A01 门禁：

- 当前 Unity 证据使用 `Null Device`；GPU 计时不可用，不能替代目标 Windows 设备的 CPU/GPU/内存/GC/FPS 基准。
- 仍需代表性岛屿/海域场景的实际加载时间、LODGroup 最终资产抽样、Agent/动物数量上限、音频资源释放和长时间稳定性证据。
- 对象池和流式闭环已在运行时可复核，但不能据此把 D16-A01 标记为 PASS。

## 2026-08-11 本机图形设备与持续采样更新

当前状态：**IN_PROGRESS（本机基础性能窗口通过，目标设备与完整长线门禁仍未关闭）**。

- Unity 6 在非 `-nographics` 模式识别到本机 `NVIDIA GeForce RTX 4070 Ti`；`D16.PlayMode.Tests` 现为 **4/4 PASS**。
- 新增 `D16A01RuntimePerformanceWindowSamplesContinuously`，连续运行约 8 秒并取得 32 个探针样本：平均 FPS **59.98**、最低 FPS **59.72**、平均帧时间 **16.67 ms**、平均主线程时间 **4.01 ms**、预算满足、保护对象超预算 **0**。
- 持续采样报告：`tools/results/d16_a01_performance_window_report.json`；PlayMode XML/日志：`tools/results/d16_a01_performance_window_playmode_test_results.xml`、`tools/results/d16_a01_performance_window_playmode_unity6.log`。
- Unity FrameTiming 在本机仍未返回 GPU 毫秒（`GpuSampleAvailable=false`）；内存峰值已记录，但 8 秒窗口不能替代低端/目标设备、长时间运行、最终资产 LOD、音频释放和真实岛屿/海域规模基准。因此 A01 继续保持 IN_PROGRESS。

## 2026-08-11 A01 本机长时性能窗口更新

当前状态：**IN_PROGRESS（本机长时采样子门禁通过，目标/低端设备门禁仍未关闭）**。

- 将 `D16A01RuntimePerformanceWindowSamplesContinuously` 从 8 秒提升为 60 秒连续窗口，并要求至少 100 个探针样本；实际取得 **234 samples**，Unity 6 PlayMode **5/5 PASS**。
- 本机图形设备为 `NVIDIA GeForce RTX 4070 Ti`；窗口 **60.005 秒**，平均 FPS **59.96**，最低 FPS **54.88**，平均帧时间 **16.68 ms**，平均主线程 **3.67 ms**，`BudgetSatisfied=true`，`ProtectedOverBudgetCount=0`。
- 内存峰值已记录：allocated **226,577,166 bytes**、reserved **602,488,832 bytes**、managed **630,415,360 bytes**；Unity FrameTiming 仍未提供 GPU 毫秒（`GpuSampleAvailable=false`）。
- 最新证据：`tools/results/d16_a01_performance_window_report.json`、`tools/results/d16_a01_long_window_playmode_unity6.xml`、`tools/results/d16_a01_long_window_playmode_unity6.log`。

这证明性能探针、预算、流式/LOD/对象池和本机 60 秒播放窗口可以连续工作；它仍不替代目标 Windows/低端设备基准、真实岛屿/海域规模、最终资产 LOD、Agent/动物数量上限、音频释放和 D16-S01 全流程长线验收，因此 A01 保持 IN_PROGRESS。

## 2026-08-11 A01 高负载表现预算夹具

新增 `D16A01RuntimeBudgetCapsHighLoadWithoutFreezingProtectedObjects`，在真实 InitialHarbor PlayMode 中创建 95 个非保护 Agent、1 个受保护交互 Agent、128 个动物和 40 个音频表现登记。夹具对象仅包含 `D16PresentationRegistration`，不包含领域状态、事件或存档组件。

Unity 6 图形回归 `tools/results/d16_a01_budget_stress_playmode_unity6.xml`：**7/7 PASS**；报告 `tools/results/d16_a01_budget_stress_report.json`：267 个候选全部进入快照，ActiveAgent **32/32**、ActiveAnimal **64/64**、AudibleSource **24/24**，受保护对象保持 Full 与音频开启，非保护 Agent 观察到 Frozen，ProtectedOverBudgetCount=0，BudgetSatisfied=true。

为使 PlayMode 测试直接读取预算快照，`D16.PlayMode.Tests.asmdef` 已显式引用 `Whalebone.Systems`；Assembly-CSharp 与 D16.PlayMode.Tests 构建均为 0 warning / 0 error。该夹具关闭了 A01 的运行时高负载预算子门禁，但不替代目标/低端设备、真实大场景和最终资产性能验收。

## 2026-08-12 当前本机复验

最新 D16 PlayMode 全量回归为 **14/14 PASS**，其中包含流式/LOD/对象池夹具和 60 秒性能窗口。当前报告 `tools/results/d16_a01_performance_window_report.json` 记录：

- Unity 6000.0.80f1，NVIDIA GeForce RTX 4070 Ti；
- 60.0049 秒、233 samples、平均 FPS 59.95、最低 FPS 57.09；
- `BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；
- FrameTiming 未提供 GPU 毫秒（`GpuSampleAvailable=false`）。

这只是本机图形设备证据；目标/低端设备、真实大场景与最终资产规模、GPU 计时、音频释放和跨平台长线仍未完成，D16-A01 保持 **IN_PROGRESS**。

## 2026-08-12 图形模式性能窗口复验

- 使用 Unity 6 非 `-nographics` PlayMode 重新执行 D16.PlayMode.Tests，结果 **17/17 PASS**：`tools/results/d16_a01_gpu_window_playmode_unity6.xml`；原始日志：`tools/results/d16_a01_gpu_window_playmode_unity6.log`。
- 本次真实设备识别为 `NVIDIA GeForce RTX 4070 Ti`；InitialHarbor 60.005 秒、234 samples，平均 FPS **59.893**、最低 FPS **47.781**、平均主线程 **3.540 ms**，`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；当前报告：`tools/results/d16_a01_performance_window_report.json`。
- FrameTiming 仍未提供 GPU 毫秒（`GpuSampleAvailable=false`），所以没有把 CPU/帧率结果冒充 GPU 通过。Unity 日志保留了 LicensingClient 握手警告；测试 XML 本身无失败/不确定用例。

该复验只更新本机图形设备证据，不关闭目标/低端设备、真实岛屿/海域规模、最终资产 LOD、音频资源释放和跨平台长线门禁；D16-A01 继续 **IN_PROGRESS**。

## 2026-08-12 A01 流式音频作用域接线回归

本轮修复了一个真实的 A01 代码缺口：`D16AudioResourceScope` 原本可以独立加载/释放音频引用，但没有接入 `D16WorldStreamCell` 的加载状态变化，因此海域/岛屿表现单元卸载时不会触发本单元音频引用释放。

### 实现

- `D16AudioResourceScope.TrackLoadedClip(...)` 允许外部表现加载器登记已取得的 `AudioClip`，并沿用 scope 级引用计数和逐引用归属记录；普通 `Load(...)` 复用同一登记路径。
- `D16WorldStreamCell` 自动从 `PresentationRoot` 发现 `D16AudioResourceScope`，也支持场景引导层显式传入 scope。
- 已加载单元切换到卸载状态时，单元调用 scope 的 `ReleaseAll()`；未明确归属的共享音频只减少引用并记录跳过次数，不调用 `Resources.UnloadAsset`。
- 该链路只影响表现资源生命周期，不停用碰撞，不删除领域实体，不写事件或存档。

### 真实验证

- 单项 Unity 6 PlayMode：`D16A01StreamingCellReleasesOwnedAudioScopeWithoutUnloadingSharedClip` **1/1 PASS**，证据 `tools/results/d16_a01_audio_scope_single_unity6_retry3.xml` / `.log`；覆盖单元卸载和“明确归属引用先释放、共享引用后释放”的反向顺序。
- Unity 6 D16 PlayMode 全量：**23/23 PASS**，证据 `tools/results/d16_a01_audio_scope_full_d16_playmode_unity6_retry2.xml` / `.log`。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。

这关闭了 A01“流式单元与音频作用域没有接线”的代码/编辑器运行时子门禁；测试夹具使用运行时生成的共享 clip，不能替代真实最终音频二进制、目标/低端设备内存曲线、代表性大场景加载和跨平台长线证据。因此 D16-A01 仍为 **IN_PROGRESS**。

## 2026-08-12 高压力预算不改事实/结算状态回归

为直接验证“表现降级不会丢失事件、存档和事实”，在 `D16PerformanceBudgetTests` 新增 `HighPressureEvaluationDoesNotMutateEventFactsOrSettlementClaims`：

- 先创建并声明一条真实 `EventLog` 事实；
- 执行高压力 `D16PerformanceBudgetSystem.Evaluate`，包含受保护 Agent、远景 Agent 和动物候选；
- 比较评估前后的 EventLog 快照，确认事实数量、序号、事件 ID、保存版本、结果码和已结算键全部不变；
- 该测试只验证性能预算裁决的读/返回边界，不让性能系统获得事件、存档或领域数值写权限。

验证：`dotnet build game/Save.Tests.csproj --no-restore -v:minimal`、`dotnet build tools/d16_a01_runner/D16A01Runner.csproj --no-restore -v:minimal` 均 0 warning / 0 error；独立 A01 runner **6/6 PASS**。静态场景基线 `python tools/d16_a01_static_baseline.py game/Assets/Scenes/InitialHarbor.unity --json-out tools/results/d16_a01_static_baseline.json` 通过。

该结果关闭 A01 的“纯预算评估可能修改 EventLog 结算状态”代码级风险，但不替代真实大场景、目标/低端设备、GPU/内存、最终资产 LOD、音频释放和跨平台长线证据；D16-A01 继续 **IN_PROGRESS**。

## 2026-08-12 D5 角色材质分帧后的当前回归

为减少 D5 角色预设在首次进入 InitialHarbor 时集中复制材质造成的启动压力，`game/Assets/Scripts/Gameplay/D5/D5RuntimeBootstrap.cs` 现在将角色渲染器材质应用拆成每帧最多 4 个渲染器；这只属于表现层调度，不改变角色、输入、存档或领域状态。

- `dotnet restore game/Assembly-CSharp.csproj --ignore-failed-sources --nologo` 后，`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- 当前源码 Unity 6 PlayMode 全量：`tools/results/d16_d5_budgeted_full_playmode_unity6_run2.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 同一轮 60 秒窗口报告 `tools/results/d16_a01_performance_window_report.json`：60.0059 秒、233 samples、平均 FPS **60.024**、最低采样 FPS **59.565**、平均帧时间 **16.662 ms**、平均主线程 **4.399 ms**。
- 全帧诊断保留：最长全帧 **182.086 ms**（启动后 **0.0326 秒**）、超过 50 ms **1 次**；预算满足、受保护对象超预算 **0**；GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。峰值 allocated/reserved/managed 分别为 227,970,697 / 598,228,992 / 638,681,088 bytes。

结论：材质分帧没有引入 D16 功能回归，稳定窗口指标满足本机预算，但没有消除启动阶段长帧；当前证据更支持继续定位场景加载、资源/Shader 预热和多模块 Bootstrap 首帧初始化，而不是把材质复制认定为唯一根因。A01 仍 **IN_PROGRESS**，目标/低端设备、真实大场景、最终资产和函数级归因缺口不变。

## 2026-08-12 D5 分阶段运行时初始化回归

在 D5 表现接线模块内将首次运行拆为 `WaitingForPlayer → LoadingSnapshot → CreatingSystems → InitializingState → ApplyingCharacterVisual → BuildingMap` 六个阶段。读档、系统实例化、角色材质队列和 12 个海图标记仍按原顺序完成，但不再集中在同一帧；`D5RuntimeBootstrap.IsReady` 只有海图与角色表现完成后才置为 true。

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- Unity 6 PlayMode 全量：`tools/results/d16_d5_staged_bootstrap_full_playmode_unity6.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 当前 60 秒报告 `tools/results/d16_a01_performance_window_report.json`：60.0057 秒、234 samples、平均 FPS **59.952**、最低采样 FPS **55.709**、平均帧时间 **16.680 ms**、平均主线程 **4.337 ms**。
- 全帧诊断：最长全帧 **176.615 ms**（启动后 **0.0302 秒**）、超过 50 ms **1 次**；预算满足、受保护对象超预算 **0**；GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。

结论：分阶段初始化降低了本机当前窗口的首段长帧，但没有完成函数级根因归因，也没有替代目标/低端设备和真实大场景证据。A01 继续 **IN_PROGRESS**；下一步仍是对场景资源/Shader 预热与其它 Bootstrap 的首帧开销做独立采样，不能把这次小幅改善写成最终性能通过。

## 2026-08-12 A01 性能指标扩展后的最终本机回归

性能窗口报告现在额外保存真实的加载、接线和托管 GC 指标：

- Unity 6 PlayMode 全量证据：`tools/results/d16_a01_final_metrics_full_playmode_unity6.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `SceneLoadSeconds=0.2062`；D16 控制器/探针就绪等待 `RuntimeBootstrapReadySeconds=0.0006`；D5 运行时就绪 `D5RuntimeReadySeconds=0.0739`。
- 60.0156 秒、233 samples、平均 FPS **59.906**、最低采样 FPS **50.252**、平均帧时间 **16.695 ms**、平均主线程 **4.354 ms**。
- 托管 GC：Gen0 **23**、Gen1 **23**、Gen2 **23**；峰值 allocated/reserved/managed 分别为 223,946,141 / 581,451,776 / 639,528,960 bytes。
- 最长全帧 **179.453 ms**（启动后 **0.0318 秒**）、超过 50ms **1 次**；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。

这些字段解决了 A01 报告缺少加载耗时和 GC 事实的问题，但没有解决启动长帧，也没有覆盖目标/低端设备、真实大场景或 GPU 计时；A01 继续 **IN_PROGRESS**。

## 2026-08-12 A01 本机事实复验

- `d16_a01_static_baseline.py` 已重新扫描 InitialHarbor：24 个 GameObject、17 个 MeshRenderer，运行时指标仍按设计留空。
- `D16A01Runner` 已重新构建并执行，纯预算测试 **6/6 PASS**。
- 静态脚本的 Collider 汇总只覆盖 MeshCollider/CapsuleCollider，不能单独证明实际场景碰撞；运行时 CharacterController、船体/甲板碰撞和目标设备物理表现仍需真实 Unity 场景或目标设备证据。

本机复验没有扩大 A01 结论：目标/低端设备、真实大场景、最终资产 LOD、GPU/内存、最终音频、跨平台和长线仍未完成，A01 继续 **IN_PROGRESS**。

## 2026-08-12 本机硬件基线采集

- 新增只读工具 `tools/d16_a01_hardware_baseline.py`，使用系统信息和 `nvidia-smi` 记录性能窗口对应主机，不把主机信息写入游戏存档或领域事实。
- 当前证据 `tools/results/d16_a01_hardware_baseline.json`：Windows 10、20 个逻辑处理器、物理内存 34,134,740,992 bytes、NVIDIA GeForce RTX 4070 Ti、驱动 596.36、显存 12,282 MiB；GPU 利用率/显存/温度为一次瞬时遥测快照。
- 工具结果明确 `scope=local_host_only`、`is_target_device_evidence=false`、`is_low_end_device_evidence=false`，并声明不测量游戏帧 GPU 时间、真实大场景加载和长线稳定性；因此 A01 仍 **IN_PROGRESS**。
- 新增 `tools/tests/test_d16_a01_hardware_baseline.py`：解析数值/NA 和“不得声明目标设备”两项测试通过。

## 2026-08-12 GPU 计时 fallback 复验

性能探针增加了 Unity `ProfilerRecorder` 的 `GPU Frame Time` 后备路径：只有 GPU counter 真实返回正值才累计 `GpuTimeMs`，否则保留 `GpuSampleAvailable=false`。`Assembly-CSharp` 与 `D16.PlayMode.Tests` 构建均 0 warning / 0 error；Unity 图形专项 **1/1 PASS**，60.017 秒、233 samples、平均 FPS 64.06、最低 FPS 2.90、平均主线程 4.33 ms。

本次实测仍未取得 GPU counter，报告原样保留 `GpuTimeMs=-1`；最低 FPS 的 2.90 outlier 也未被过滤。该结果只关闭 GPU 计时 fallback 的代码缺口，不关闭目标/低端设备、真实大场景、最终资产 LOD、GPU/内存目标、音频释放和长线性能门禁。

## 2026-08-12 GPU 计时 fallback 资源预热复测

同一图形 PlayMode 专项在资源预热后再次运行，结果仍为 **1/1 PASS**（`tools/results/d16_a01_gpu_recorder_playmode_unity6_retry.xml`）：60.015 秒、234 samples、平均 FPS **65.646**、最低 FPS **2.911**、平均主线程 **4.279 ms**。GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。

约 2.9 FPS 的长帧在两次独立窗口中都出现，属于需要继续定位的可复现性能问题；本报告不通过删除极端样本、平均值或测试夹具改写该结果。A01 仍 **IN_PROGRESS**，后续必须补最长帧定位、目标/低端设备和真实大场景证据。

## 2026-08-12 启动长帧定位与材质分帧回归

为减少表现层初始化对首帧的集中占用，`D7MaterialPolishController` 改为每帧最多处理 8 个渲染器，并将性能探针扩展为记录全帧最长帧、发生时间和超过 50ms 的次数。D5 烟测 `tools/results/d17_material_budgeted_d5_smoke.log` 与 D16 PlayMode 全量 `tools/results/d16_after_material_budgeted_full_playmode_unity6.xml` 分别通过，后者为 **38/38 PASS**；工具测试为 **61/61 PASS**。

全量回归当前报告为：60.006 秒、234 samples、平均 FPS 59.91、最低采样 FPS 52.50、最长全帧 189.883ms（启动后 0.034 秒）、超过 50ms 为 1 次。冷启动专项仍有约 347—354ms 的启动长帧，且材质分帧后仍出现，说明材质层不是唯一原因；现阶段应继续排查冷启动场景加载、资源/Shader 预热和多模块首帧接线。该证据不替代目标/低端设备、真实大场景和最终资产性能验收，A01 继续 **IN_PROGRESS**。

## 2026-08-12 延迟 GPU counter 复验

GPU `ProfilerRecorder` 已从 `Awake` 延迟到场景开始运行 1 秒后启动，避免测量器本身阻塞首帧。冷启动专项仍出现约 349ms 长帧，故 GPU counter 初始化不是唯一根因；延迟方案下 D16 PlayMode 全量为 **38/38 PASS**（`tools/results/d16_after_deferred_gpu_full_playmode_unity6.xml`）。

最新报告为：60.006 秒、234 samples、平均 FPS **59.964**、最低采样 FPS **56.714**、最长全帧 **179.763ms**（启动后 0.032 秒）、超过 50ms 为 1 次，`GpuSampleAvailable=false`、`GpuTimeMs=-1`。长帧仍集中在冷启动/编辑器资源和多模块首帧初始化方向；目标/低端设备、真实大场景和函数级归因仍待完成，A01 保持 **IN_PROGRESS**。

## 2026-08-12 当前源码 D16 PlayMode 全量复验

- 使用 `-runTests -testPlatform PlayMode -assemblyNames D16.PlayMode.Tests` 重新执行当前工作区运行时回归；XML `tools/results/d16_current_full_playmode_unity6.xml` 为 **38/38 PASS**，失败、不确定和跳过均为 0。
- 当前性能报告 `tools/results/d16_a01_performance_window_report.json`：60.006 秒、234 samples、平均 FPS 59.996、最低采样 FPS 59.438、平均主线程 4.375ms、最长全帧 180.456ms（启动后 0.032 秒）、超过 50ms 为 1 次；预算满足，受保护对象超预算 0。
- GPU counter 当前仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`），没有用 CPU 时间替代 GPU，也没有过滤启动长帧。
- 这次复验确认性能/流式/角色物理/上下船/镜头/菜单输入锁/诊断等代码级回归没有失败；A01 仍不能 PASS，目标/低端设备、真实大场景、最终资源和函数级启动长帧归因仍缺，D16-S01/S02 不启动。

## 2026-08-12 当前门禁复核

- 当前源码 D16 PlayMode 全量仍为 **38/38 PASS**，性能报告仍记录 60.006 秒、234 samples、平均 FPS 59.996、最低采样 FPS 59.438、平均主线程 4.375ms、最长全帧 180.456ms（启动后 0.032 秒）、超过 50ms 为 1 次；
- `GpuSampleAvailable=false`、`GpuTimeMs=-1` 原样保留；没有以 CPU 时间替代 GPU，也没有删除冷启动长帧；
- 本机代码/预算子门禁通过不等于 A01 总体通过：目标/低端设备、真实大场景、最终资源 LOD、音频释放和函数级冷启动归因仍缺。

## 2026-08-12 D16 控制器阶段耗时归因复验

- `D16WorldPerformanceController` 新增只读阶段计时：`Awake`、首次注册扫描、首次 `Update` 和首次预算评估；这些指标只用于性能观测，不改变预算裁决或领域状态。
- Unity 全量 `tools/results/d16_a01_controller_phase_full_playmode_unity6.xml`：**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped；`Assembly-CSharp` 构建 0 warning / 0 error。
- 当前 60 秒窗口：234 samples、平均 FPS **59.982**、最低采样 FPS **58.314**、平均主线程 **4.447ms**；最长全帧 **188.765ms**（启动后 0.0311 秒）、超过 50ms 1 次，`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`。
- D16 阶段耗时：`Awake=0.428ms`、首次注册扫描 `0.102ms`、首次 `Update=0.244ms`、首次预算评估 `0.027ms`。因此当前约 188.8ms 冷启动长帧不能归因于 D16 控制器单一阶段；后续优先排查其它 Bootstrap、场景资源/Shader 预热和首帧加载链路。
- 场景加载 **0.2155s**、D16 探针 Ready **0.0006s**、D5 Ready **0.0754s**；托管 GC Gen0/1/2 为 **21/21/21**。GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`），没有用 CPU 时间替代 GPU。

本轮只关闭了 D16 控制器的函数级观测缺口，不关闭目标/低端设备、真实大场景、最终资产和跨平台性能门禁；A01 继续 **IN_PROGRESS**。

## 2026-08-12 D2 分阶段组装与稳定登记修复

- `D2RuntimeBootstrap` 将 InitialHarbor 的运行时组装拆为船、船员锚点、玩家、物理、控制、表现六个阶段，每帧推进一个阶段；玩法状态、输入裁决和存档所有权未迁移。
- 发现并修复一个真实回归：玩家由 `ph_crew_male` 复制时会继承船员的 D16 表现登记 ID，导致玩家与船员合并为一个预算候选项。D16 默认登记现在会对运行时目标校正稳定 ID：玩家为 `player.avatar`，船为 `player.ship`，船员为 `crew.harbor_anchor`。
- A01 高负载单项 `tools/results/d16_a01_d2_staged_budget_focus_fixed_playmode_unity6.xml`：**1/1 PASS**；候选数 **267/267**，表现登记 **267/267**，Agent/动物/音频预算满足，受保护对象未被冻结。
- 当前 D16 全量 `tools/results/d16_a01_d2_staged_fixed_full_playmode_unity6.xml`：**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped；Assembly-CSharp 构建 0 warning / 0 error。
- 新窗口：60.016 秒、234 samples、平均 FPS **59.963**、最低采样 FPS **56.360**、平均主线程 **4.316ms**；最长全帧 **173.320ms**（启动后 0.0239 秒）、超过 50ms 1 次，预算满足。场景加载 **0.2011s**、D5 Ready **0.1064s**、GC Gen0/1/2 **22/22/22**。

分阶段组装将本机启动长帧从上一轮约 188.8ms 降到约 173.3ms，但仍未完成目标/低端设备、真实大场景、GPU/内存和跨平台性能证据；A01 继续 **IN_PROGRESS**。

## 2026-08-12 D2 控制闭环后的最新性能窗口

- 当前源码 D16 PlayMode 全量 `tools/results/d16_current_full_after_d2_control_loop_unity6.xml`：**39/39 PASS**，0 failed / 0 inconclusive / 0 skipped；其中 InitialHarbor 键盘移动、鼠标视角拖动、视角切换、上船/船控/下船专项 `tools/results/d16_a03_d2_loop_playmode_unity6.xml` 为 **1/1 PASS**。
- 当前 `tools/results/d16_a01_performance_window_report.json`：窗口 **60.0156s**、**234 samples**、平均 FPS **59.975**、最低 FPS **57.264**、平均帧时间 **16.674ms**、平均主线程 **4.434ms**。
- 冷启动诊断：最长全帧 **175.977ms**，发生在启动后 **0.0244s**；超过 50ms **1 次**。场景加载 **0.2039s**、D5 Ready **0.1063s**、托管 GC Gen0/1/2 **22/22/22**；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`。
- GPU 计时仍为 `GpuSampleAvailable=false`、`GpuTimeMs=-1`，没有用 CPU 时间替代 GPU，也没有删除冷启动长帧样本。D16-A01 仍 **IN_PROGRESS**；下一步应在目标/低端设备和代表性大场景继续采集，并继续对其它 Bootstrap、场景资源/Shader 预热做函数级归因。

## 2026-08-12 D16-A01 默认登记保护标志回归修复

发现 `D16WorldPerformanceController` 过去只在稳定 ID 变化时重配默认登记。若预制体已经带有正确的 `player.avatar` ID、但玩家保护/类别标志被错误序列化，预算层会认为它不是保护对象并可能降级。现新增 `D16PresentationRegistration.MatchesRuntimeConfiguration(...)`，默认登记现在同时校验稳定 ID、Agent/动物/音频、玩家相关、交互和表现层标志；不匹配时只重新接线表现登记，不写领域状态、事件或存档。

- 新增 PlayMode 回归 `D16A01DefaultRegistrationRepairsFlagsWhenStableIdAlreadyMatches`：**1/1 PASS**，证据 `tools/results/d16_a01_registration_flags_unity6.xml`。
- 当前 D16 PlayMode 全量：**40/40 PASS**，证据 `tools/results/d16_current_full_after_registration_guard_unity6.xml`；`Assembly-CSharp` 构建 0 warning / 0 error。
- 最新本机 60 秒窗口：234 samples、平均 FPS **59.941**、最低 FPS **52.646**、平均主线程 **4.228ms**、最长全帧 **171.049ms**（启动后 0.0242s）、超过 50ms 1 次；预算满足、保护对象超预算 0，GPU counter 仍 unavailable。

该修复关闭一个真实的“稳定 ID 正确但保护标志错误”代码风险，但不替代目标/低端设备、真实大场景、最终资产 LOD、GPU/内存和跨平台证据；D16-A01 继续 **IN_PROGRESS**。

## 2026-08-12 对象池复用表现状态复位回归

- 发现真实代码风险：`D16PresentationPool` 回收表现实例后，下一次租借可能继承上一次的 Frozen、LOD、静音或 Animator 状态；`D16PresentationRegistration.Apply` 也可能把原本禁用的 Animator 错误打开。
- 修复 `game/Assets/Scripts/Gameplay/D16/D16PresentationRegistration.cs`：缓存 Renderer/Animator/Audio 的原始启用状态，新增 `ResetPresentationState()`，租借前恢复原始状态并清除 `LODGroup.ForceLOD`；降级应用现在保持原本禁用的 Animator 不被打开。
- 修复 `game/Assets/Scripts/Gameplay/D16/D16PresentationPool.cs`：实例挂载到新父节点后、激活前，对子树内所有登记执行表现状态复位。该操作只作用于表现层，不写领域状态、碰撞体、事件或存档。
- 新增真实 PlayMode 回归 `D16A01RuntimeFixtureExercisesStreamingLodAndPool` 的对象池复用断言：专项 **1/1 PASS**，证据 `tools/results/d16_a01_pool_reset_unity6_v2.xml`；全量 `D16.PlayMode.Tests` **40/40 PASS**，证据 `tools/results/d16_current_full_after_pool_reset_unity6_v2.xml`。
- 最新本机 60 秒窗口：234 samples、平均 FPS **59.937**、最低 FPS **54.577**、平均主线程 **4.194ms**、最长全帧 **170.477ms**（启动后 0.0241s）、超过 50ms **1 次**；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`。
- GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`），没有以 CPU 时间替代 GPU，也没有删除启动长帧样本。

该修复关闭了一个本机对象池状态泄漏风险；D16-A01 仍 **IN_PROGRESS**，目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线证据仍需补齐。

## 2026-08-12 对象池复位专项增强回归

- 将 `D16A01RuntimeFixtureExercisesStreamingLodAndPool` 的对象池断言扩展为同时检查 Renderer、Audio 和 Animator 基线：对象被 Frozen/静音后归还，再次租借时 Renderer/Audio 恢复，原本禁用的 Animator 仍保持禁用。
- `tools/results/d16_a01_pool_reset_unity6_v3.xml`：专项 **1/1 PASS**；`tools/results/d16_current_full_after_pool_reset_unity6_v3.xml`：D16 PlayMode **40/40 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 最新本机窗口：60.015s、235 samples、平均 FPS **59.979**、最低 FPS **58.817**、平均主线程 **4.357ms**、最长全帧 **183.410ms**（启动后 0.0260s）、超过 50ms 1 次；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`。
- `GpuSampleAvailable=false`、`GpuTimeMs=-1` 原样保留，未将 GPU 不可用包装成通过，也未删除启动长帧。

该增强只提高对象池状态复用的验证覆盖，不关闭目标/低端设备、真实大场景、最终资产、GPU/内存、跨平台和长线门禁；A01 继续 **IN_PROGRESS**。

## 2026-08-12 A01 主机与静态场景基线刷新

- 主机快照 `tools/results/d16_a01_hardware_baseline_current.json`：Windows 10、20 logical processors、约 31.8 GiB 内存、NVIDIA GeForce RTX 4070 Ti（driver 596.36、12,282 MiB VRAM），工作区剩余空间约 666.9 GiB。
- 该快照明确 `measurement_kind=host_hardware_snapshot`、`scope=local_host_only`，`is_target_device_evidence=false`、`is_low_end_device_evidence=false`；不测量游戏 GPU 毫秒、目标机性能、真实大场景加载或长线稳定性。
- 静态清单 `tools/results/d16_a01_static_baseline_current.json`：InitialHarbor YAML 中 24 GameObjects、17 MeshRenderer、17 MeshFilter、0 个静态 Collider；运行时指标保持 null，必须由 Unity 运行时采样提供。
- 当前结论：主机/静态基线已留痕，但不能替代目标/低端设备和真实运行时大场景证据；A01 继续 **IN_PROGRESS**。

## 2026-08-12 A01 表现降级状态保护回归

本轮补齐了“降级不丢失事件、存档和事实”的本机代码证据。新增 PlayMode 回归 `D16A01PresentationDowngradePreservesFactsSaveAndSettlement`，在真实 InitialHarbor 播放循环中：

- 使用真实 `EventLog` 追加事实并建立已结算幂等门；
- 捕获 `EventLogSnapshot`，连续应用 Reduced、Background、Frozen、Full 四种表现降级；
- 检查事实数量、事件 ID、归属、事实内容、保存序列、已结算状态、快照恢复事实和碰撞体均保持不变；
- 夹具明确为 presentation-only，不把 D16 表现层变成事件/存档写者。

证据：

- `tools/results/d16_a01_state_preservation_unity6_v2.xml`：**1/1 PASS**；
- `tools/results/d16_a01_state_preservation_report.json`：`FactPreserved=true`、`SaveSnapshotPreserved=true`、`SettlementClaimPreserved=true`、`RestoredFactPresent=true`、`ColliderStayedEnabled=true`；报告同时区分事实 `FactSaveRevision=17` 与事件日志序列 `EventLogNextSequenceBefore/After=1`。

该回归关闭了 A01 的本机“表现降级可能破坏事实/保存投影/幂等门”的代码风险；它不替代 SaveSystem 文件级断电、跨平台、目标设备和 D16-S01 长线证据，因此 D16-A01 继续 **IN_PROGRESS**。

### 全量回归

- `tools/results/d16_current_full_after_state_preservation_v2_unity6.xml`：D16 PlayMode **41/41 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 当前 60.015 秒窗口：234 samples，平均 FPS **59.972**，最低采样 FPS **58.806**，平均主线程 **4.356ms**，最长全帧 **174.150ms**（启动后 0.0256 秒），超过 50ms 1 次；
- 场景加载 **0.2038s**、D5 Ready **0.1067s**、GC Gen0/1/2 **22/22/22**；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；
- GPU 计时仍不可用：`GpuSampleAvailable=false`、`GpuTimeMs=-1`，没有用 CPU 指标替代 GPU。

## 2026-08-12 当前工作树 Unity 6 全量回归

- 使用当前工作树、Unity 6 图形模式执行 `D16.PlayMode.Tests`；`tools/results/d16_current_full_continuation_unity6.xml` 为 **41/41 PASS**，failed/inconclusive/skipped 均为 0，耗时 71.808s。
- 当前性能窗口 `tools/results/d16_a01_performance_window_report.json`：60.015s、234 samples、平均 FPS 59.934、最低采样 FPS 53.428、平均主线程 4.331ms；最长全帧 172.579ms（启动后 0.0246s），超过 50ms 1 次。
- 场景加载 0.1998s、D5 Ready 0.1066s、GC Gen0/1/2 为 22/22/22；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；GPU counter 仍不可用（`GpuSampleAvailable=false`、`GpuTimeMs=-1`）。
- 该结果确认当前代码回归没有新增失败，但仍只覆盖本机 Unity 图形窗口；目标/低端设备、真实大场景、最终资产、GPU/内存目标、跨平台和长线证据未关闭，A01 继续 **IN_PROGRESS**。

## 2026-08-12 A01 性能报告补齐同屏与流式计数

- 发现验收报告包装类没有把运行时探针已采集的同屏/流式计数落盘；补齐 `ActiveAgentCount`、`ActiveAnimalCount`、`AudibleSourceCount`、`LoadedCellCount`、`RegistrationCount`、`CellTransitionCount` 和对象池计数等字段。
- 当前工作树重新执行 D16 PlayMode：`tools/results/d16_current_full_metrics_fields_unity6.xml` 为 **41/41 PASS**，failed/inconclusive/skipped 均为 0，耗时 71.780s。
- 新生成报告中的真实值包括：Agent `1`、Animal `0`、Audio `1`、LoadedCell `2`、Registration `3`；没有用非零占位值填充不存在的动物或对象池活动。
- 工具回归：A01 性能报告专项 **2/2 PASS**，全量工具测试 **68/68 PASS**；AgentCore、Save.Tests、Assembly-CSharp 构建均 0 warning / 0 error。
- 该修复只补齐验收证据输出，不改变 D16 预算裁决；A01 仍缺目标/低端设备、真实大场景、GPU/内存目标、最终资产和长线证据，继续 **IN_PROGRESS**。

## 2026-08-12 表现流式加载分帧与加载指标接线

### 修改

- `game/Assets/Scripts/Gameplay/D16/D16WorldStreamCell.cs` 不再在加载时把表现树的所有子对象集中恢复到同一帧；它会记录第一次看到的 `activeSelf` 意图，只对原本活跃的子对象按批次跨帧激活。
- 流式单元新增 `IsLoading`、`LoadingProgress`、`LoadedPresentationObjectCount`、`PendingPresentationObjectCount`、`LastLoadDurationMs` 和 `LoadCount`；卸载会停止未完成的表现加载，保持根节点卸载并释放该单元音频作用域。
- `D16WorldPerformanceController` 与 `D16RuntimePerformanceProbe.Report` 接入加载中单元数、已加载/待加载表现对象数、最近完成表现加载耗时和加载次数；`D16RuntimeAcceptanceTests` 的 A01 性能报告以及 Python 契约测试同步覆盖这些字段。
- 该模块仍只操作 `PresentationRoot` 子树；不创建、删除或冻结领域实体，不修改事件、存档、碰撞或玩法数值。

### 原始验证

- `D16A01StreamingCellLoadsPresentationAcrossFramesAndPreservesInactiveIntent`：Unity 6 PlayMode **1/1 PASS**，证据 `tools/results/d16_a01_staged_streaming_unity6.xml`。
- 该回归实际观察到部分加载进度，确认 37 个活跃表现子对象分批恢复，原本明确禁用的对象保持禁用，卸载后第二次加载仍保持相同意图。
- 当前工作树 Unity 6 D16 PlayMode 全量：`tools/results/d16_a01_staged_streaming_full_current_unity6.xml`，**42/42 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时约 72.09s。
- 当前 60 秒性能窗口专项：`tools/results/d16_a01_staged_streaming_performance_current_unity6.xml`，**1/1 PASS**；报告 `tools/results/d16_a01_performance_window_report.json` 已包含新加载字段，`PresentationLoadCount=2`、`LoadingCellCount=0`、`PendingPresentationObjectCount=0`。
- 当前本机窗口真实值：60.0128s、236 samples、平均 FPS **63.368**、最低 FPS **3.508**、最长全帧 **285.071ms**（启动后 0.0893s）、超过 50ms 1 次、平均主线程 **4.430ms**；场景加载 **0.2458s**、D5 Ready **0.1692s**、GC Gen0/1/2 为 **22/22/22**。
- GPU 仍不可用：`GpuSampleAvailable=false`、`GpuTimeMs=-1`；未用 CPU 时间替代 GPU，也未删除最低 FPS 或最长帧样本。A01 性能窗口只证明采样链和新字段可复验，不能解释为性能达标。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；A01 性能报告专项 **2/2 PASS**。

### 验收边界

本轮关闭的是表现流式分帧和加载指标的本机代码缺口，不关闭目标/低端设备、真实大场景、最终资产 LOD/音频、GPU/内存目标、跨平台和长线稳定性证据；A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 远景建筑代理表现接线

- 新增 `D16RemoteBuildingProxy`，将建筑的完整表现根与远景代理表现根作为两个明确的表现层入口；它只生成表现预算候选，不接管建筑的逻辑碰撞体、交互状态或其他领域数据。
- `D16WorldPerformanceController` 注册远景建筑代理并按 `Full/Reduced/Background/Frozen` 决策切换表现：远景使用代理根，近景恢复完整根，冻结时隐藏两者；逻辑碰撞体不参与该切换。
- 新增 `D16A01RemoteBuildingProxyUsesBackgroundTierWithoutTouchingLogicCollider`：Unity 6 PlayMode 聚焦结果 `tools/results/d16_a01_remote_building_proxy_unity6_rerun.xml`，**1/1 PASS**，验证远景/近景表现切换和逻辑碰撞体持续启用。
- 当前工作树 D16 PlayMode 全量 `tools/results/d16_a01_remote_building_proxy_full_current_unity6.xml`：**43/43 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时约 72.42s。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；新源码已加入 `Whalebone.Gameplay.D16.Runtime.csproj` 的显式编译清单。

### 验收边界

本轮关闭的是远景建筑表现代理的运行时代码和逻辑碰撞隔离回归，不代表项目已经拥有最终建筑代理美术、真实大场景批量配置、目标/低端设备、GPU/内存、跨平台或长线稳定性证据；当前仍未达到 A01 完成条件，A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D3D12 GPU 计时复核

- 强制 Unity 6 使用 Direct3D 12 执行 A01 性能窗口：`tools/results/d16_a01_performance_d3d12_current_unity6.xml`，**1/1 PASS**，窗口约 60.45s。
- 日志确认图形设备为 Direct3D 12、RTX 4070 Ti；运行时报告真实记录平均 FPS **64.904**、最低采样 FPS **3.609**、最长全帧 **277.109ms**、超过 50ms 1 次、平均主线程 **4.676ms**。
- `GpuSampleAvailable=false`、`GpuTimeMs=-1` 仍保持原值；D3D12 复核没有得到可用 GPU counter，因此没有用 CPU 时间或估算值填充 GPU 字段。
- 该复核加强了“GPU 计时链当前不可用”的证据，但不关闭目标/低端设备、真实大场景、GPU/内存目标、最终资源、跨平台和长线门禁；A01 继续 **IN_PROGRESS**。

## 2026-08-12 GPU counter 候选探测与 D3D12 回归

- `D16RuntimePerformanceProbe` 现在依次尝试明确的 GPU-only counter（`GPU Frame Time`、`GPU Time`、`GPU Frame Time (ms)`），只有 Unity 返回有效的 GPU recorder 才记录计数器名称；不回退到 CPU 或估算时间。
- 性能窗口报告新增 `GpuCounterName`，用于区分“句柄可创建但没有样本”和“没有可用计数器”。本轮 D3D12/RTX 4070 Ti 选择到 `GPU Frame Time`，但 `GpuSampleAvailable=false`、`GpuTimeMs=-1`，因此仍按不可用处理。
- 回归证据 `tools/results/d16_a01_performance_gpu_counter_probe_d3d12_unity6.xml`：**1/1 PASS**，耗时约 60.44s；平均 FPS **65.904**、最低采样 FPS **3.608**、最长全帧 **277.177ms**。

### 验收边界

本轮只提高 GPU 计时探针的可观测性并保留不可用事实，没有把有效句柄误写成有效 GPU 样本；目标/低端设备、真实大场景、GPU/内存目标、最终资源、跨平台和长线证据仍未满足，A01 继续 **IN_PROGRESS**。

### GPU 探测后的全量回归

- `tools/results/d16_a01_gpu_counter_probe_full_current_unity6.xml`：当前工作树 D16 PlayMode **43/43 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时约 72.36s。
- 聚焦 D3D12 GPU 探测与全量回归均未发现新增 Assertion/NullReference/MissingReference/Unhandled exception；该结果只证明代码回归稳定，不改变 A01 的外部证据结论。

### 2026-08-12 最终自动化复核

- 全量工具测试：`python -m unittest discover -s tools/tests -p 'test_*.py' -q`，**69/69 PASS**（108.107s）。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests 均 0 warning / 0 error；Systems 边界扫描和 `git diff --check` 均 PASS。

## 2026-08-12 共享音频作用域卸载隔离

### 修改

- `D16AudioResourceScope` 新增 `CaptureLoadedResourcePaths()` 与 `ReleaseResourcesAddedSince(...)`，支持按资源路径快照释放，而不是无差别清空整个作用域。
- `D16WorldStreamCell` 在每次进入加载态时记录音频基线；卸载时只释放本次单元加载期间新增的引用。多个流式单元共用一个音频作用域时，其他单元的音频引用不会被误删。
- 新增 `D16A01SharedAudioScopeKeepsOtherCellReferencesOnUnload`，并修正旧音频用例的生命周期顺序，使其模拟“单元加载后登记资源”的真实运行时路径。
- 修改范围仍限于 D16 Gameplay 表现层和 PlayMode 验收；没有修改 Systems、领域事实、事件、存档、契约或网络代码。

### 验证

- 聚焦共享作用域用例：`tools/results/d16_a01_shared_audio_scope_current_unity6.xml`，**1/1 PASS**。
- 原有音频生命周期用例：`tools/results/d16_a01_audio_scope_legacy_current_unity6.xml`，**1/1 PASS**。
- 当前 D16 PlayMode 全量：`tools/results/d16_a01_shared_audio_scope_full_fixed_unity6.xml`，**47/47 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal` 与 `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal` 均 0 warning / 0 error。

### 验收边界

本轮关闭了共享音频作用域在流式卸载时的本机代码风险；仍不能替代最终音频资源抽样、目标/低端设备内存曲线、真实大场景、GPU 计时、跨平台和长线性能证据。D16-A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

### 同路径多 owner 加固

- 进一步增加 `TrackLoadedClipForOwner(...)`、`ReleaseOwner(...)` 和 `D16WorldStreamCell.TrackAudioResource(...)`；同一 scope/同一路径由多个流式单元持有时，只有最后一个 owner 与无 owner 引用都释放后才释放底层 clip。
- `D16WorldStreamCell` 在卸载和销毁时先释放自己的 owner，再处理无 owner 的新增路径；兼容既有外部 `TrackLoadedClip(...)` 调用。
- 更新后的聚焦证据 `tools/results/d16_a01_shared_audio_owner_current_unity6.xml`：**1/1 PASS**；验证同路径共享、owner 卸载、最后一个无 owner 引用释放的顺序。
- 更新后的全量证据 `tools/results/d16_a01_shared_audio_owner_full_current_unity6.xml`：D16 PlayMode **47/47 PASS**，0 failed / 0 inconclusive / 0 skipped。

该加固仍属于 D16 表现资源生命周期，不改变玩法事实、事件、存档、碰撞或 Systems 写者；A01 外部性能和最终资产门禁继续保持 **IN_PROGRESS**。

## 2026-08-12 预加载音频 owner 生命周期补强

- 修复 `D16WorldStreamCell.TrackAudioResource(...)` 在单元尚未第一次进入加载态时登记音频会退化为无 owner 引用的问题；现在登记入口会先建立该单元的稳定 owner，预加载、首次加载、卸载和销毁都沿用同一归属。
- 新增 `D16A01AudioRegisteredBeforeFirstLoadIsOwnedByCell`，覆盖“startLoaded=false → 首次加载前登记音频 → 加载 → 卸载”的真实生命周期，确认 scope 不残留该路径且不能再由无 owner API 重复释放。
- 聚焦证据 `tools/results/d16_a01_preload_audio_owner_current_unity6.xml`：**1/1 PASS**；全量证据 `tools/results/d16_a01_preload_audio_full_current_unity6.xml`：**48/48 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `Assembly-CSharp` 与 `D16.PlayMode.Tests` 均 0 warning / 0 error；该修复仍只关闭 A01 本机表现音频生命周期缺口，目标/低端设备、最终资源、GPU/内存、跨平台和长线证据仍缺，A01 继续 **IN_PROGRESS**。

## 2026-08-12 未加载单元音频清理与销毁回归

### 修改

- 修复 `D16WorldStreamCell.SetLoadedInternal(...)` 的卸载早退路径：当单元已经处于 `unloaded`、表现根为空，或从未进入过加载态时，也会统一释放该单元的 owner 音频与已记录的加载基线；不会清空共享 scope 中其他单元的引用。
- `OnDestroy()` 复用同一清理入口，并避免在没有加载基线时把整个音频 scope 误当作本单元资源释放。
- 新增 `D16A01UnloadedCellReleasesOwnedAudioWithoutPresentationRoot` 与 `D16A01DestroyBeforeFirstLoadReleasesOwnedAudio`，覆盖未加载重复卸载、无表现根和首次加载前销毁三条边界。

### 验证

- 聚焦证据 `tools/results/d16_a01_unloaded_audio_current_unity6.xml`：**1/1 PASS**。
- 聚焦证据 `tools/results/d16_a01_destroy_before_load_current_unity6.xml`：**1/1 PASS**。
- 当前 D16 PlayMode 全量 `tools/results/d16_a01_audio_lifecycle_full_current_unity6.xml`：**50/50 PASS**，0 failed / 0 inconclusive / 0 skipped，约 73.36s。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal` 与 `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal` 均 0 warning / 0 error。

### 验收边界

本轮关闭的是未加载表现单元的本机音频 owner 生命周期缺口，不等于 A01 的目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线性能证据通过；A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 流式单元对象池重新配置清理

### 修改

- 修复 `D16WorldStreamCell.ConfigureRuntime(...)` 重新配置对象时覆盖旧 scope/表现根的问题：重新配置前停止旧加载协程、释放旧 owner 与加载基线，并停用旧表现根。
- 新增 `D16A01ReconfigureReleasesPreviousAudioOwnerAndPresentationRoot`，验证同一流式对象从第一组表现配置复用到第二组配置时，旧音频不残留、旧根不继续显示，新 owner 仍可正常卸载。

### 验证

- 聚焦证据 `tools/results/d16_a01_reconfigure_audio_current_unity6.xml`：**1/1 PASS**。
- 当前 D16 PlayMode 全量 `tools/results/d16_a01_reconfigure_full_current_unity6.xml`：**51/51 PASS**，0 failed / 0 inconclusive / 0 skipped，约 73.32s。
- `Assembly-CSharp` 与 `D16.PlayMode.Tests` 均 0 warning / 0 error。

### 验收边界

本轮关闭的是对象池重新配置时的本机表现资源生命周期风险，不代表真实岛屿最终表现资源、目标/低端设备性能、GPU/内存、跨平台和长线证据已完成；A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 岛屿数据流注册与可见性事实

### 修改

- 新增 `game/Assets/Scripts/Gameplay/D16/D16IslandDataStreamRegistry.cs`，只读 `D5IslandCatalog.All` 和当前 D5 存档快照，为现有 12 个岛屿目录建立稳定的数据流单元：岛屿 ID、名称、海图坐标、模拟等级和 metadata active 状态。
- `D16WorldPerformanceController` 与 `D16RuntimePerformanceProbe` 接出目录数量、当前活动数据单元、data-only 单元、视觉表现根数量和参考岛屿 ID；不写入 IslandSystem、领域事实、事件或存档。
- 视觉表现只能通过显式 `RegisterVisualPresentation(...)` 接入；没有把海图目录、UI 标记或数据单元冒充 3D 岛屿模型。当前 InitialHarbor 没有真实岛屿 3D 资源，因此保持可核验的 data-only 状态。

### 验证

- 岛屿数据流聚焦用例 `tools/results/d16_a01_island_data_stream_current_unity6.xml`：**1/1 PASS**。
- D16 PlayMode 全量 `tools/results/d16_a01_island_data_stream_full_current_unity6.xml`：**53/53 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 60 秒性能窗口 `tools/results/d16_a01_performance_window_current_unity6.xml`：**1/1 PASS**；`tools/results/d16_a01_performance_window_report.json` 记录 235 个采样、平均 64.54 FPS、12 个目录单元、7 个活动数据单元、12 个 data-only、0 个视觉表现根，参考岛屿 `santa_margarita`。
- `test_d16_a01_performance_report.py`：**2/2 PASS**；工具测试全集：**69/69 PASS**；`Assembly-CSharp`、`D16.PlayMode.Tests`、D16 Runtime 工程均 0 warning / 0 error。

### 验收边界

本轮只补齐了基于既有 D5 内容目录的数据流注册和真实性指标，不生成或伪造岛屿 3D 资源，不改变玩法事实、碰撞、事件、存档、contracts 或 Systems 写者。当前报告的 0 个视觉表现根是未完成资源的明确证据；A01 仍缺真实岛屿/海域资源、目标/低端设备、GPU/内存、跨平台和长线证据，继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D3D11 本机性能窗口与 GPU 能力事实

### 验证

- 使用 Unity `6000.0.80f1`、`-force-d3d11` 执行 `D16A01RuntimePerformanceWindowSamplesContinuously`；结果 `tools/results/d16_a01_performance_capability_current.xml` 为 **1/1 PASS**，无 failed/inconclusive/skipped。
- 当前报告 `tools/results/d16_a01_performance_window_report.json` 记录：`GraphicsDevice=NVIDIA GeForce RTX 4070 Ti`、`GraphicsDeviceType=Direct3D11`、`GraphicsDeviceVersion=Direct3D 11.0 [level 11.1]`、`SupportsGpuRecorder=true`、`FrameTimingFeatureEnabled=true`。
- 60 秒窗口采集 234 个样本，平均 65.82 FPS；场景加载 0.263s；最长帧 302.78ms，发生在启动后约 0.089s；`GpuSampleAvailable=false`、`GpuTimeMs=-1`。

### 结论边界

本轮补齐了真实 D3D11 图形设备和 GPU 计时能力状态字段，并增加了“GPU 可用标记不得没有正帧时间”的断言；没有用 CPU 时间冒充 GPU 时间。启动长帧和 GPU 计时缺失仍需继续归因/目标环境采样；本机 RTX 结果不替代目标/低端设备证据，A01 继续 **IN_PROGRESS**。

随后使用同一 D3D11 参数执行全量 `D16.PlayMode.Tests`：`tools/results/d16_full_after_performance_capability_unity6.xml` 为 **53/53 PASS**，0 failed / 0 inconclusive / 0 skipped；性能探针字段改动没有造成 D16 输入、UI、镜头、上下船、存档或其他运行时回归。

## 2026-08-12 D5 启动阶段函数级归因

### 修改

- `D5RuntimeBootstrap` 新增只读阶段计时：等待玩家、加载快照、创建系统、初始化状态、应用角色表现、构建海图，以及 Awake 和总构建耗时。
- `D16RuntimePerformanceProbe` 通过公开诊断属性的反射读取 D5 阶段事实，避免 D16 Runtime 程序集反向引用 D5 程序集，保持程序集依赖单向；性能层不写玩法、存档或领域状态。
- A01 性能报告和工具校验新增 6/6 阶段完整性门槛，阶段数据缺失时不把性能窗口写成通过。

### 验证

- `tools/results/d16_a01_phase_attribution_current_unity6.xml`：Unity 6 D3D11 图形 PlayMode 聚焦 **1/1 PASS**；
- 当前报告 `tools/results/d16_a01_performance_window_report.json`：60.014 秒、234 samples、平均 65.71 FPS、最低 3.49 FPS、最长全帧 286.43ms（启动后 0.091s）；GPU 计时仍 unavailable；
- 归因数据：D5 Awake 3.424ms，总构建 227.823ms；等待玩家 91.676ms、加载快照 0.006ms、创建系统 1.200ms、初始化状态 0.518ms、角色表现 0.681ms、海图构建 2.407ms，6/6 完整；
- `tools/results/d16_full_after_phase_attribution_20260812.xml`：D16 PlayMode **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped；`test_d16_a01_performance_report.py` **2/2 PASS**；
- D16 Runtime、Assembly-CSharp、D16.PlayMode.Tests 构建均 0 warning / 0 error。

### 验收边界

本轮把冷启动从“只有长帧现象”推进到 D5 阶段级可追踪证据，但 286ms 长帧仍未被完全归因；当前结果也不替代目标/低端设备、真实大场景、最终岛屿资源、GPU/内存、跨平台和长线证据。A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D2 启动阶段函数级归因

### 修改

- `D2RuntimeBootstrap` 新增只读阶段计时：创建船、船员锚点、玩家、物理配置、控制配置、表现配置，以及总构建耗时和 6/6 完整性标记。
- `D16RuntimePerformanceProbe` 通过类型名和公开诊断属性读取 D2 阶段事实，不反向引用 D2 程序集；性能层不写玩法、碰撞、存档或领域状态。
- A01 性能窗口新增 D2 阶段完整性门槛，D2 诊断缺失时不会生成不完整的性能通过结果。

### 验证

- `tools/results/d16_a01_d2_phase_attribution_current_unity6.xml`：Unity 6 D3D11 图形 PlayMode 聚焦 **1/1 PASS**；
- 当前报告：60.014 秒、235 samples、平均 65.05 FPS、最低 3.56 FPS、最长全帧 280.86ms（启动后 0.088s），GPU 计时 unavailable；
- D2 阶段：总构建 165.364ms；创建船 0.168ms、船员锚点 0.168ms、玩家 1.024ms、物理 13.506ms、控制 6.580ms、表现 8.711ms，6/6 完整；
- 与 D5 阶段归因同时落盘：D5 总构建 227.037ms，六阶段完整；
- `tools/results/d16_full_after_d2_phase_attribution_20260812.xml`：D16 PlayMode 全量 **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 长帧仍未完全归因，原始最低 FPS/最长帧保留，不把平均 FPS 扩大解释为目标设备通过。

### 验收边界

本轮关闭 A01 本机 D2 启动阶段观测子缺口；场景资源、D3/D4/D6/D7/D10 等其他启动链路、目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线证据仍未闭合。A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 D3/D4/D9 启动链就绪归因

### 修改

- `D16RuntimePerformanceProbe` 在性能模块内部按公开 `IsReady` 属性观测 D3 贸易航程、D4 内容层和 D9 港口垂直切片入口，不反向引用这些程序集，也不写玩法状态、存档或领域事件。
- A01 性能窗口报告新增 `D3RuntimeReadySeconds`、`D4RuntimeReadySeconds`、`D9RuntimeReadySeconds`；任一入口未就绪时，性能窗口会失败，不会把缺失数据静默当成 0。

### 原始验证

- D3D11 图形 PlayMode 聚焦 `tools/results/d16_a01_d3_d4_d9_readiness_graphics_current_unity6.xml`：**1/1 PASS**。
- 全量回归后的当前报告 `tools/results/d16_a01_performance_window_report.json`：234 samples、平均 **59.96 FPS**、最低 **54.99 FPS**、最长帧 **178.06ms**（启动后 0.0246s）；D3 **0.1024s**、D4 **0.1024s**、D9 **0.1024s** 就绪。聚焦测试 XML 仍独立保留为 1/1 PASS 证据。
- 当前报告保持 `GraphicsDevice=NVIDIA GeForce RTX 4070 Ti`、`GraphicsDeviceType=Direct3D11`；`GpuSampleAvailable=false`、`GpuTimeMs=-1`，没有用 CPU 时间替代 GPU 时间。
- 同一 D3D11 参数执行 D16 全量：`tools/results/d16_full_after_d3_d4_d9_readiness_20260812.xml` 为 **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `python -m unittest tools.tests.test_d16_a01_performance_report -q`：**2/2 PASS**；工具测试全集：**71/71 PASS**；`Assembly-CSharp`、`D16.PlayMode.Tests` 构建均 0 warning / 0 error。

### 验收边界

本轮只补齐 D3/D4/D9 启动链的本机就绪观测，不代表目标/低端设备、真实大场景、最终岛屿/海域资源、GPU/内存、跨平台或长线性能门禁通过。最长启动帧仍未完全归因，A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A01 当前报告刷新

当前 Unity 6 D3D11 全量回归 `tools/results/d16_full_current_goal_20260812.xml` 为 **54/54 PASS**。全量回归后性能报告 `tools/results/d16_a01_performance_window_report.json` 为：60.015 秒、233 samples、平均 **59.98 FPS**、最低 **57.50 FPS**、最长帧 **181.47ms**；D2/D3/D4/D5/D9 启动归因字段均有值，GPU 帧时间仍 unavailable。该结果只证明本机采样链和代码回归，不替代目标/低端设备、真实大场景、最终资源、GPU/内存和长线证据；A01 继续 **IN_PROGRESS**。
