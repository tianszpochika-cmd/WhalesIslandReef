# D16-A01 性能 Agent 交接报告

## 1. 任务与状态

- 任务编号：D16-A01
- 执行日期：2026-08-11
- 前置任务：D15-S02
- 当前状态：IN_PROGRESS
- 是否完成实现：否
- 是否允许进入 D16 性能实现：是，完整验收仍未通过

D15-S02 已通过；A01 已进入实现和运行时验收，但目标/低端设备、真实大场景、最终资产和长线性能证据仍未关闭。

## 2. 修改文件

- docs/game/design/common/D16_PERFORMANCE_STREAMING_REPORT.md：阻断报告和开工基线。
- docs/game/design/common/handoffs/D16/D16-A01_HANDOFF.md：本交接报告。
- .workbuddy/memory/2026-08-11.md：阻断记忆日志。

未修改性能代码、Systems、AgentCore、Save/Storage、contracts、server、web 或 D15 内容。

## 3. 门禁与复核

复核命令：

- python tools/d15_s02_content_qa.py game/Assets/Resources/ContentRegistry --json-out tools/results/d15_s02_content_qa_result.json
- 历史结果：D15_S02_QA_EXIT=1；当前 D15-S02 顺序复验为 PASS（0 errors、0 warnings）。

阻断问题：

- 两条植被生境不一致；
- D15-A06 资源历史审查、证据、敏感标签、跳过策略和部分 LOD 字段缺失；
- D15-A07 有 18 次引用指向 12 个未定义宠物外观组合；
- D15-A08 任务/音频测试和 LOD-N/A/asset_state 字段缺失；
- A01—A07 共 2536 个 display key 缺少可验证的中英本地化包证明；
- Save.Tests 被 D10CrewBehaviorState 缺失阻断；
- 24 个 D15 抽样中 9 个缺少明确保存标记。

## 4. 当前性能代码边界

- D5PerformanceSystem 目前只负责帧率目标和帧率采样。
- D7AudioController 和 D9AudioPresentation 已有基础音频播放，但没有分区音频卸载和资源生命周期指标。
- D15-A01 目录存在 streaming cell、LOD 和保存键声明，但没有可替代 D16 运行时性能实现。
- 未改变任何强逻辑结果、存档事实、事件事实或系统写者。

## 5. D16-A01 未完成事项

以下项目全部待 D15-S02 通过后实现：

- 岛屿/海域分区加载和卸载；
- 不丢失实体与事件事实的流式保存/恢复；
- LOD 档位切换；
- 对象池和命中率/回收指标；
- Agent 模拟等级降级；
- 动物活跃上限；
- 远景建筑代理；
- 音频按距离/分区卸载；
- CPU、GPU、内存、加载时间、GC、同屏 Agent/动物数量基准；
- 不同距离和模拟等级下的回归测试。

## 6. 重新开工注意事项

性能降级只能改变表现或模拟频率，不能跳过存档、事件事实、关系、生态结果或任务结果。IslandSystem、ShipSystem、EcologySystem、CrewSystem、SaveSystem 和 Audio/Presentation 层的写入边界必须保持不变；UI 只能显示指标，Agent 不能修改性能裁决。

重新开始前必须：

1. D15-S02 统一 QA 错误数为 0；
2. D10CrewBehaviorState 修复并通过 Save.Tests；
3. 9 个缺少保存标记的样本完成运行时流式存档/恢复抽样；
4. 执行 D16 性能实现、基准、回归和文档验收。

## 2026-08-11 状态更新

D15-S02 已通过统一 QA，D10 standalone build 也已通过；本任务前置门禁已解除。D16-A01 当前为 **PENDING**，实现和性能实测尚未开始，仍需建立真实 CPU/GPU/内存/GC/加载/LOD/对象池/模拟等级基线。

## 2026-08-11 实现进度补充

当前状态改为：**IN_PROGRESS（Unity 运行时实测待环境恢复）**。前置 D15-S02 已通过；本任务已经可以进入实现，但不能把静态编译和纯逻辑测试当作完整性能 PASS。

本轮新增：

- `D16PerformanceBudgetSystem` 与 contracts：稳定 ID 排序、玩家/交互保护、Agent/动物/音频上限、高压力降级、重复/非法登记计数；不写领域状态、事件或存档；
- InitialHarbor 自动入口与默认登记：`D2_PlayerAvatar`、`ph_ship_fishing`、`ph_crew_male`；
- `D16WorldStreamCell`、表现层 Renderer/Animator/LODGroup/AudioSource 降级、只复用表现 prefab 的对象池和安全音频 scope；
- `D16RuntimePerformanceProbe`：FPS、帧时间、FrameTiming CPU/GPU（平台支持时）、内存、预算、流式切换和对象池计数；
- D16-A01 纯预算 runner 与 InitialHarbor 静态基线工具。

已验证：Systems、Save.Tests、Assembly-CSharp standalone build 均 0 warning / 0 error；纯预算测试 5/5；静态基线单测 1/1。静态基线 JSON 明确将 CPU/GPU/GC/运行时对象数量留空。

尚未验证：Unity Editor/Player 真实采样、真实岛屿/海域分区配置、加载卸载耗时、最终资产 LOD、生态数量上限、音频实际卸载和 Unity EditMode/PlayMode。Unity batch runner 停在 `Licensing::Module`，日志保留在 `tools/results/d16_a02_unity_test.log`。

## 2026-08-11 Unity 6 实测更新

当前状态：**IN_PROGRESS（运行时基础烟测已通过，性能目标仍待目标设备证据）**。

- Unity 6 PlayMode `D16RuntimeAcceptanceTests.InitialHarborBootstrapsD16AndSamplesRuntime`：1/1 PASS。
- 已证实 InitialHarbor、相机、D2 玩家、船、D16 控制器、D16 探针和 3 个默认表现登记在真实播放循环中创建；玩家落地日志来自 `BeachGround`。
- 报告：`tools/results/d16_runtime_playmode_report.json`；XML：`tools/results/d16_runtime_playmode_test_results_retry8.xml`；总览：`docs/game/design/common/D16_RUNTIME_ACCEPTANCE_REPORT.md`。
- 批处理使用 `Null Device`，GPU 计时不可用；报告不作为最终 FPS/CPU/GPU/GC 性能 PASS。

下一步是目标 Windows 配置的代表场景性能采样、流式/LOD/对象池/音频释放验证，以及长线稳定性；在这些证据产生前保持 A01 为 IN_PROGRESS。

## 2026-08-11 A01 Unity 6 运行时闭环更新

本轮把 A01 从“只有预算代码和静态基线”推进到“生产场景流式接线 + 可重复运行时夹具”。

### 新增与修改

- `game/Assets/Scripts/Gameplay/D16/D16WorldStreamCell.cs`：新增 `ConfigureRuntime`，仅配置表现根、距离阈值和初始加载状态。
- `game/Assets/Scripts/Gameplay/D16/D16WorldPerformanceController.cs`：新增运行时焦点配置；InitialHarbor 自动建立 `harbor.sea`、`harbor.beach` 两个表现流式单元；不隐藏领域实体，不关闭碰撞层。
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：新增 A01 PlayMode 夹具和 JSON 证据报告。

### 已通过的运行时事实

- `D16.PlayMode.Tests`：2/2 PASS（InitialHarbor 启动烟测 + A01 流式/LOD/对象池夹具）。
- InitialHarbor 报告：`LoadedCellCount=2`、`RegistrationCount=3`、`SampleCount=3`、`BudgetSatisfied=true`。
- A01 夹具报告：加载/卸载、表现根状态跟随、LOD 远近切换、碰撞保持启用、对象池复用全部为 true；流式切换 2 次，对象池 Rent=2、Reuse=1、Return=2。
- `dotnet build game/Assembly-CSharp.csproj --no-restore`：0 warning / 0 error。

### 仍然不能标记完成的原因

本轮仍在 `-nographics`/`Null Device` 环境执行，GPU 计时不可用；尚未取得目标 Windows 设备 CPU/GPU/内存/GC/FPS、最终资产 LOD、音频资源卸载、生态数量上限和长线稳定性证据。因此 A01 保持 **IN_PROGRESS**，下一步是目标设备基准和故障/长线验收，不是直接关闭门禁。

## 2026-08-11 本机图形设备持续采样更新

当前状态：**IN_PROGRESS（本机可重复性能窗口通过，完整目标设备门禁未关闭）**。

- Unity 6 非 `-nographics` PlayMode 回归 `D16.PlayMode.Tests` **4/4 PASS**，包含 A01 流式/LOD/对象池夹具、A01 持续采样窗口、A03 UI 点击链和 InitialHarbor 烟测。
- 持续窗口约 8 秒、32 个样本：平均 FPS 59.98、最低 FPS 59.72、平均帧时间 16.67 ms、平均主线程 4.01 ms、预算满足、保护对象超预算 0；报告为 `tools/results/d16_a01_performance_window_report.json`。
- 本机图形设备为 `NVIDIA GeForce RTX 4070 Ti`；Unity FrameTiming 没有提供 GPU 毫秒，故不把 GPU 计时写成通过。

仍待下一位执行者：目标/低端设备 CPU/GPU/内存/GC/FPS 基准、真实岛屿/海域规模加载、最终资产 LOD、Agent/动物上限、音频卸载和更长时间稳定性。8 秒本机窗口只证明采样链连续工作，不能关闭 A01 总门禁。

## 2026-08-11 本机 60 秒长时窗口

当前状态：**IN_PROGRESS（本机长时子门禁通过；目标设备、资产和发布规模门禁未关闭）**。

- 将 PlayMode 连续采样窗口提升到 **60 秒**，最少样本要求提升到 **100**；Unity 6 `D16.PlayMode.Tests` 最新结果为 **5/5 PASS**。
- `tools/results/d16_a01_performance_window_report.json`：WindowSeconds `60.005`、SampleCount `234`、AverageFps `59.96`、MinFps `54.88`、AverageFrameMs `16.68`、AverageMainThreadMs `3.67`、BudgetSatisfied `true`、ProtectedOverBudgetCount `0`。
- 本机设备为 RTX 4070 Ti；内存峰值 allocated/reserved/managed 分别为约 226.6 MB / 602.5 MB / 630.4 MB；GPU 毫秒仍不可用。
- 可复核文件：`tools/results/d16_a01_long_window_playmode_unity6.xml`、`tools/results/d16_a01_long_window_playmode_unity6.log`。

这一步只关闭“本机 60 秒连续采样”子门禁。目标/低端设备性能、真实大场景加载、最终资产 LOD、生态数量上限、音频资源释放、跨平台发布包和 D16-S01 长线仍需独立证据；不能把本机长时窗口写成 A01 总 PASS。

## 2026-08-11 高负载预算夹具更新

- 新增 `D16A01RuntimeBudgetCapsHighLoadWithoutFreezingProtectedObjects`：95 个非保护 Agent、1 个受保护交互 Agent、128 个动物、40 个音频表现登记，全部为表现层测试对象。
- Unity 6 图形 PlayMode：`tools/results/d16_a01_budget_stress_playmode_unity6.xml` **7/7 PASS**；报告 `tools/results/d16_a01_budget_stress_report.json` 显示候选 267、Agent 32/32、动物 64/64、音频 24/24、BudgetSatisfied=true、保护对象 Full/音频开启、非保护 Agent Frozen、ProtectedOverBudgetCount=0。
- `D16.PlayMode.Tests.asmdef` 补齐 `Whalebone.Systems` 显式依赖；Assembly-CSharp 和 PlayMode 测试工程构建 0 warning / 0 error。

该更新关闭 A01 的运行时高负载预算子门禁；目标/低端设备、真实岛屿/海域规模、最终资产 LOD、音频资源卸载和 D16-S01 长线仍未完成，A01 总状态保持 **IN_PROGRESS**。

## 2026-08-12 图形模式性能窗口复验

### 原始证据

- Unity 6 非 `-nographics` PlayMode：**17/17 PASS**，XML `tools/results/d16_a01_gpu_window_playmode_unity6.xml`，日志 `tools/results/d16_a01_gpu_window_playmode_unity6.log`。
- 设备：`NVIDIA GeForce RTX 4070 Ti`；窗口 60.005 秒、234 samples、平均 FPS 59.893、最低 FPS 47.781、平均主线程 3.540 ms、BudgetSatisfied=true、ProtectedOverBudgetCount=0。
- 报告：`tools/results/d16_a01_performance_window_report.json`；`GpuSampleAvailable=false`，GPU 毫秒仍不可用。

### 解释与边界

本次结果只证明本机图形设备上的运行时采样和预算链路可复验；它不替代目标/低端设备、真实大场景、最终资产 LOD、音频释放、跨平台和 D16-S01 长线验收。日志中存在 Unity LicensingClient 握手警告，但 XML 测试结果为 17/17 通过，警告原样保留。A01 继续 **IN_PROGRESS**。

## 2026-08-12 流式音频作用域接线回归

### 状态

**IN_PROGRESS**。本轮关闭了 A01 的代码/编辑器运行时音频生命周期子门禁，但没有把目标设备、最终音频资产或跨平台性能证据写成通过。

### 修改文件

- `game/Assets/Scripts/Gameplay/D16/D16AudioResourceScope.cs`
- `game/Assets/Scripts/Gameplay/D16/D16WorldStreamCell.cs`
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`
- `docs/game/design/common/D16_PERFORMANCE_STREAMING_REPORT.md`
- `docs/game/design/common/handoffs/D16/D16-A01_HANDOFF.md`
- `.workbuddy/memory/2026-08-12.md`

### 新接口与数据所有权

- 新增 `D16AudioResourceScope.TrackLoadedClip(string, AudioClip)`：外部表现加载器登记已加载 clip，资源引用计数和逐引用归属边界仍由 Audio scope 持有；`ConfigureOwnership(...)` 显式配置可卸载前缀和权限。
- `D16WorldStreamCell.ConfigureRuntime(..., D16AudioResourceScope audioScope = null)` 支持显式 scope；未传入时从表现根自动发现。
- 单元从 loaded→unloaded 时只释放该 scope 的表现音频引用；共享/非明确归属资源不调用资源级卸载。
- 该模块不拥有玩法数值、事件、领域实体或存档事实；流式切换也不关闭碰撞层。

### 原始验证结果

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- Unity 6 focused PlayMode：**1/1 PASS**，`tools/results/d16_a01_audio_scope_single_unity6_retry3.xml`，日志同名 `.log`。
- Unity 6 D16 PlayMode 全量：**23/23 PASS**，`tools/results/d16_a01_audio_scope_full_d16_playmode_unity6_retry2.xml`，日志同名 `.log`。
- 新增回归断言：流式单元卸载后 `LoadedCount=0`，共享 clip 的 `SkippedSharedAssetReleaseCount=1`，表现根同步为 inactive；跨 scope 反向释放时 `UnloadedAssetCount=0`。

### 未完成事项与下游

- 仍缺真实最终音频二进制的加载/释放抽样、目标/低端设备内存曲线、真实大场景、GPU 计时、跨平台和长线存档/事件保持证据。
- 因此 D16-A01 保持 **IN_PROGRESS**；D16-S01/S02 仍禁止启动。下游仍是补齐外部证据后再做 A01 总门禁复核。

## 2026-08-12 高压力预算不改事实/结算状态回归

### 修改文件与边界

- `game/Assets/Tests/EditMode/D16PerformanceBudgetTests.cs`：新增 EventLog 快照保护测试；
- `tools/d16_a01_runner/Program.cs`：将测试纳入纯预算 runner；
- 只验证 D16 性能预算 Systems 的纯读/返回边界，不修改 EventLog、SaveSystem、领域数值、contracts 或网络代码。

### 验证结果

- `dotnet build game/Save.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build tools/d16_a01_runner/D16A01Runner.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet tools/d16_a01_runner/bin/Debug/net9.0/D16A01Runner.dll`：**6/6 PASS**；
- `python tools/d16_a01_static_baseline.py game/Assets/Scenes/InitialHarbor.unity --json-out tools/results/d16_a01_static_baseline.json`：通过，静态基线 24 个 GameObject、17 个 MeshRenderer，运行时指标仍按设计留空。

### 验收结论与未完成事项

高压力预算评估前后 EventLog 事实、序号、保存版本和已结算键保持不变，代码级数据边界已取得证据。A01 总体仍 **IN_PROGRESS**：目标/低端设备、真实大场景、最终资产 LOD、GPU/内存、音频释放、跨平台和长线证据仍缺；D16-S01/S02 不启动。

## 2026-08-12 A01 本机事实复验

- 重新运行 `python tools/d16_a01_static_baseline.py game/Assets/Scenes/InitialHarbor.unity --json-out tools/results/d16_a01_static_baseline.json`，静态基线仍为 24 个 GameObject、17 个 MeshRenderer；`runtime_metrics` 按设计保持未设置。
- 重新构建并运行 `tools/d16_a01_runner/D16A01Runner.csproj`，纯预算测试 **6/6 PASS**，包含高压力评估不修改 EventLog/结算声明的回归。
- 静态工具的 `colliders` 摘要只统计 MeshCollider/CapsuleCollider，不能证明场景中所有碰撞体，也不能替代 Unity 运行时 CharacterController/船体甲板碰撞检查；本轮没有把它写成碰撞通过。

A01 仍 **IN_PROGRESS**：RTX 4070 Ti 本机窗口、预算与流式夹具已取得证据，但目标/低端设备、真实大场景、最终资产 LOD、GPU/内存、最终音频、跨平台和长线证据仍缺。D16-S01/S02 不启动。

## 2026-08-12 本机硬件基线采集

- 新增 `tools/d16_a01_hardware_baseline.py` 与 `tools/results/d16_a01_hardware_baseline.json`；采集 Windows 10、20 逻辑处理器、34,134,740,992 bytes 物理内存、RTX 4070 Ti、驱动 596.36、12,282 MiB 显存的本机快照。
- GPU 利用率/显存/温度只是 `nvidia-smi` 瞬时遥测，不冒充 Unity GPU 帧时间；结果明确不是目标/低端设备证据，也不证明真实大场景和长线稳定性。
- 新增工具单测 **2/2 PASS**；性能模块只写工具结果与报告，不写领域状态、事件、存档或契约。
- A01 总体继续 **IN_PROGRESS**，下游仍需目标/低端设备、真实大场景、最终资产 LOD、音频释放和长线证据。

## 2026-08-12 GPU 计时 fallback 实测

### 修改

- `game/Assets/Scripts/Gameplay/D16/D16RuntimePerformanceProbe.cs` 增加 Unity `ProfilerRecorder` 的 `GPU Frame Time` fallback；只有真实 GPU counter 返回正值时才写入 `GpuTimeMs`，不以 CPU 时间估算 GPU。
- recorder 无法启动或 counter 不可用时，保持 `GpuSampleAvailable=false`，并保留目标设备验收边界。

### 原始验证

- `dotnet build game/Assembly-CSharp.csproj --no-restore --nologo`：0 warning / 0 error；
- `dotnet restore game/D16.PlayMode.Tests.csproj --nologo` 后再构建：0 warning / 0 error；
- Unity 6 图形 PlayMode 专项 `D16A01RuntimePerformanceWindowSamplesContinuously`：**1/1 PASS**，XML `tools/results/d16_a01_gpu_recorder_playmode_unity6.xml`；
- 60.017 秒、233 samples、RTX 4070 Ti、平均 FPS 64.06、最低 FPS 2.90、平均主线程 4.33 ms；GPU counter 仍不可用，`GpuTimeMs=-1`、`GpuSampleAvailable=false`。

### 验收边界

fallback 代码路径已编译并在图形 PlayMode 通过，但当前 Unity 批处理/驱动组合仍没有真实 GPU counter；这次没有把不可用数据写成 PASS。A01 继续 **IN_PROGRESS**，目标/低端设备、真实大场景、最终资产 LOD、GPU/内存目标、音频释放和长线证据仍缺。

## 2026-08-12 GPU 计时 fallback 资源预热复测

为排除首次启动或资源预热造成的偶发卡顿，使用相同 Unity 6 图形 PlayMode 专项再次运行，并保留独立 XML 与日志：`tools/results/d16_a01_gpu_recorder_playmode_unity6_retry.xml` / `.log`。

- Unity 测试结果：**1/1 PASS**；窗口 60.015 秒、234 samples；
- RTX 4070 Ti：平均 FPS **65.646**、最低 FPS **2.911**、平均主线程 **4.279 ms**；
- `GpuSampleAvailable=false`、`GpuTimeMs=-1`，GPU counter 仍不可用；
- 最低 FPS 约 2.9 的长帧在资源预热后仍可复现，因此保留为真实性能问题，不过滤、不改写为性能通过。

本次复测确认采样链和异常长帧具有可重复性，但不能定位长帧根因，也不能替代目标/低端设备、真实大场景和最终资产验收。A01 继续 **IN_PROGRESS**；下一步应在性能模块内补充最长帧定位证据，并在目标设备/代表性大场景上复测。

## 2026-08-12 启动长帧定位与材质分帧回归

### 修改

- `game/Assets/Scripts/Gameplay/D7/D7MaterialPolishController.cs`：材质保真层不再在单帧内复制全部渲染器材质；改为维护待处理队列，每帧最多处理 8 个渲染器，保留纹理、不改变领域状态，并在队列完成后才报告 `IsReady`。
- `game/Assets/Scripts/Gameplay/D16/D16RuntimePerformanceProbe.cs`：增加全帧最长帧、最长帧发生时间和超过 50ms 的帧数，避免 0.25 秒抽样掩盖启动卡顿。
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：将上述长帧诊断字段写入 A01 性能报告。

### 验证

- D5 运行烟测：`tools/results/d17_material_budgeted_d5_smoke.log`，`InitialHarbor verified`；人物碰撞/落地、上下船、镜头循环、动作层和 D7 材质检查通过。
- D16 PlayMode 全量：`tools/results/d16_after_material_budgeted_full_playmode_unity6.xml`，**38/38 PASS**，失败/不确定/跳过均为 0。
- 工具测试全集：**61/61 PASS**。
- 当前全量回归写入 `tools/results/d16_a01_performance_window_report.json`：60.006 秒、234 samples、平均 FPS 59.91、最低采样 FPS 52.50、最长全帧 189.883ms（启动后 0.034 秒）、超过 50ms 为 1 次；GPU counter 仍不可用。

### 结论与边界

独立冷启动窗口仍出现约 347—354ms 的首段长帧；材质层分帧后该长帧仍存在，因此不能把材质层写成唯一根因。热启动/全量回归的长帧显著降低且集中在启动阶段，当前证据更支持“冷启动场景/编辑器资源与多模块初始化需要继续拆分或异步化”的方向，但尚未取得函数级或目标设备证据，不能宣称根因已完全定位。

A01 继续 **IN_PROGRESS**：D5/D16 本机代码回归通过，但目标/低端设备、真实大场景、最终资产 LOD、GPU/内存目标、最终音频、跨平台和长线证据仍缺；D16-S01/S02 不启动。

## 2026-08-12 延迟 GPU counter 复验

为避免 GPU `ProfilerRecorder` 的初始化影响首帧，将 counter 启动从 `Awake` 延迟到场景进入 1 秒后的稳定阶段。冷启动专项结果仍为 **1/1 PASS**，但最长帧约 349ms 仍在启动后 0.161 秒出现；因此 GPU counter 初始化不是唯一根因。

随后执行延迟方案下的 D16 PlayMode 全量回归：`tools/results/d16_after_deferred_gpu_full_playmode_unity6.xml` 为 **38/38 PASS**。最新性能报告 `tools/results/d16_a01_performance_window_report.json` 为 60.006 秒、234 samples、平均 FPS **59.964**、最低采样 FPS **56.714**、最长全帧 **179.763ms**（启动后 0.032 秒）、超过 50ms 为 1 次；GPU counter 仍不可用。

当前结论：持续运行窗口没有发现新的功能回归，长帧集中在冷启动/编辑器资源与 Bootstrap 初始化阶段；函数级拆分、目标设备和真实大场景证据仍未完成，A01 继续 **IN_PROGRESS**。

## 2026-08-12 当前源码全量运行时复验

- Unity 6 `D16.PlayMode.Tests` 当前全量 XML `tools/results/d16_current_full_playmode_unity6.xml`：**38/38 PASS**，0 failed、0 inconclusive、0 skipped。
- 当前性能窗口报告：60.006 秒、234 samples、平均 FPS 59.996、最低采样 FPS 59.438、平均主线程 4.375ms、最长全帧 180.456ms（启动后 0.032 秒）、超过 50ms 为 1 次；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`。
- `GpuSampleAvailable=false`、`GpuTimeMs=-1` 原样保留；未把 GPU 不可用包装成通过，也未删除启动长帧样本。
- 结论：本机代码级运行时回归没有新增失败，A01 仍 **IN_PROGRESS**；目标/低端设备、真实大场景、最终资产和函数级冷启动归因缺口未关闭，D16-S01/S02 不启动。

同一轮按当前 standalone 依赖重新构建并运行 `tools/d16_a01_runner/D16A01Runner.csproj`：构建 0 warning / 0 error，纯预算测试 **6/6 PASS**，包含高压力评估不改变 EventLog/结算声明的回归。该结果仍只属于本机代码子门禁。

## 2026-08-12 当前结果复核

当前 D16 PlayMode 仍为 **38/38 PASS**；性能窗口仍记录最长全帧 180.456ms（启动后 0.032 秒）、超过 50ms 为 1 次，GPU counter 不可用。A01 的目标/低端设备、真实大场景、最终资源和函数级启动归因仍缺，状态保持 **IN_PROGRESS**。

## 2026-08-12 D5 角色材质分帧后的当前证据

- `D5RuntimeBootstrap` 将角色预设材质复制拆为每帧最多 4 个渲染器；队列只作用于表现层，输入和玩法状态不等待该队列。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error（Unity 刷新后先执行受控 restore）。
- Unity 6 D16 PlayMode 全量：`tools/results/d16_d5_budgeted_full_playmode_unity6_run2.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 当前 60 秒报告：233 samples、平均 FPS **60.024**、最低采样 FPS **59.565**、平均帧时间 **16.662 ms**、平均主线程 **4.399 ms**；最长全帧 **182.086 ms**（启动后 0.0326 秒）、超过 50ms 1 次，预算满足，GPU counter 不可用。

该改动没有引入运行时回归，但没有消除启动长帧；冷启动场景/资源/Shader 与多模块 Bootstrap 仍需函数级归因。A01 继续 **IN_PROGRESS**，不把本机窗口扩大解释为目标/低端设备、真实大场景或最终资产通过。

## 2026-08-12 D5 分阶段运行时初始化

- 修改 `game/Assets/Scripts/Gameplay/D5/D5RuntimeBootstrap.cs`：首次运行拆成等待玩家、读档、系统实例化、状态初始化、角色表现、海图构建六阶段；`IsReady` 仍在全部 D5 表现准备完成后才为 true。
- 工程构建：`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`，0 warning / 0 error。
- Unity 6 D16 PlayMode 全量：`tools/results/d16_d5_staged_bootstrap_full_playmode_unity6.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 性能报告当前为 60.0057 秒、234 samples、平均 FPS **59.952**、最低采样 FPS **55.709**、平均主线程 **4.337 ms**；最长全帧 **176.615 ms**（启动后 0.0302 秒）、超过 50ms 1 次，预算满足，GPU counter 不可用。

该优化只改善 D5 首次表现接线的调度，不证明其它 Bootstrap、场景资源、Shader 预热或目标设备性能；A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 加载/GC 指标最终本机回归

- Unity 6 全量：`tools/results/d16_a01_final_metrics_full_playmode_unity6.xml`，**38/38 PASS**，并通过 D5 `IsReady` 门禁。
- 报告新增 `SceneLoadSeconds`、`RuntimeBootstrapReadySeconds`、`D5RuntimeReadySeconds` 和 Gen0/1/2 托管 GC 计数；当前值分别为 0.2062s、0.0006s、0.0739s 和 23/23/23。
- 当前窗口：60.0156s、233 samples、平均 FPS 59.906、最低采样 FPS 50.252、平均主线程 4.354ms；最长全帧 179.453ms（启动后 0.0318s）、超过 50ms 1 次。
- `BudgetSatisfied=true`、受保护对象超预算 0；GPU counter 不可用，`GpuSampleAvailable=false` / `GpuTimeMs=-1` 原样保留。

该结果补齐了 A01 所需的加载和 GC 观测字段，但仍不能关闭启动长帧、目标/低端设备、真实大场景和 GPU/函数归因门禁；A01 继续 **IN_PROGRESS**。

## 2026-08-12 D16 控制器阶段耗时归因复验

- `tools/results/d16_a01_controller_phase_full_playmode_unity6.xml`：Unity 6 当前全量 **38/38 PASS**；`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- 当前性能报告：60.0141 秒、234 samples、平均 FPS **59.982**、最低采样 FPS **58.314**、平均主线程 **4.447ms**；最长全帧 **188.765ms**（启动后 0.0311 秒）、超过 50ms 1 次，预算满足。
- 新增的 D16 控制器阶段计时为：`Awake 0.428ms`、首次注册扫描 `0.102ms`、首次 `Update 0.244ms`、首次预算评估 `0.027ms`。这些值说明控制器不是当前冷启动长帧的唯一根因；下一步应排查其它 Bootstrap、场景资源/Shader 预热和首帧加载链路。
- 场景加载 0.2155s、D5 Ready 0.0754s、GC Gen0/1/2 为 21/21/21；GPU counter 仍 unavailable，保持 `GpuSampleAvailable=false` / `GpuTimeMs=-1`。

A01 仍 **IN_PROGRESS**：目标/低端设备、真实大场景、最终资产、GPU/内存和跨平台/长线证据尚未完成；D16-S01/S02 不启动。

## 2026-08-12 D2 分阶段组装与稳定登记修复

- 修改 `game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`：将 InitialHarbor 运行时组装拆成船、船员锚点、玩家、物理、控制、表现六个逐帧阶段，`IsReady` 只在全部接线完成后为 true。
- 修改 `game/Assets/Scripts/Gameplay/D16/D16WorldPerformanceController.cs`：运行时默认登记对已存在但继承错误稳定 ID 的组件重新绑定 canonical ID；不改变领域数据、存档或预算系统裁决权。
- 根因：玩家从 `ph_crew_male` 实例化时复制了船员的 D16 登记组件，造成重复 `crew.harbor_anchor`；修复后玩家使用 `player.avatar`，候选计数恢复完整。
- 构建：`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`，0 warning / 0 error。
- A01 高负载单项：`tools/results/d16_a01_d2_staged_budget_focus_fixed_playmode_unity6.xml`，**1/1 PASS**，267/267 候选进入预算快照。
- Unity 全量：`tools/results/d16_a01_d2_staged_fixed_full_playmode_unity6.xml`，**38/38 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 当前性能窗口：60.016 秒、234 samples、平均 FPS 59.963、最低 56.360、平均主线程 4.316ms、最长全帧 173.320ms（启动后 0.0239 秒）、超过 50ms 1 次；场景加载 0.2011s、D5 Ready 0.1064s、GC 22/22/22。

A01 仍 **IN_PROGRESS**：分阶段组装改善了本机冷启动长帧但没有关闭目标设备、真实大场景、GPU/内存和跨平台证据；D16-S01/S02 不启动。

## 2026-08-12 默认登记保护标志回归修复

- 修改文件：`game/Assets/Scripts/Gameplay/D16/D16PresentationRegistration.cs`、`game/Assets/Scripts/Gameplay/D16/D16WorldPerformanceController.cs`、`game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`。
- 修复内容：默认运行时登记不再只比较 stable ID，同时验证玩家/交互保护、Agent/动物/音频类别和表现层标志；预制体 ID 正确但标志损坏时会被安全重配。
- 数据所有权：仍只属于 D16 表现预算与登记层；没有修改 Agent、事件、结算、存档或 Systems 数值裁决权，也没有新增跨端契约。
- 验证：`dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`（Unity 刷新后先受控 restore）0 warning / 0 error；专项 **1/1 PASS**（`d16_a01_registration_flags_unity6.xml`）；D16 PlayMode 全量 **40/40 PASS**（`d16_current_full_after_registration_guard_unity6.xml`）。
- 最新窗口记录：234 samples、平均 FPS 59.941、最低 52.646、平均主线程 4.228ms、最长全帧 171.049ms；GPU counter unavailable 原样保留。

结论：关闭本机代码级登记保护风险；A01 仍需目标/低端设备、真实大场景、最终资产 LOD、GPU/内存、音频释放和跨平台证据，状态保持 **IN_PROGRESS**。

## 2026-08-12 对象池复用状态泄漏修复

- 根因：表现对象池只回收/隐藏实例，没有在重新租借前恢复 Renderer、Animator、Audio 和 LOD 的原始表现状态；因此上一个租借者的 Frozen/降级/静音状态可能污染下一个对象。
- 修复：`D16PresentationRegistration` 缓存原始启用状态并提供 `ResetPresentationState()`；`D16PresentationPool.Rent` 在激活前对登记子树执行复位；`Apply` 保留原本禁用 Animator 的基线。
- 数据所有权不变：只修改 D16 表现层状态，不触碰领域实体、碰撞体、输入、事件、结算或存档。
- 构建：`Assembly-CSharp` 0 warning / 0 error。
- 专项：`D16A01RuntimeFixtureExercisesStreamingLodAndPool` **1/1 PASS**，证据 `tools/results/d16_a01_pool_reset_unity6_v2.xml`。
- 全量：D16 PlayMode **40/40 PASS**，证据 `tools/results/d16_current_full_after_pool_reset_unity6_v2.xml`。
- 性能：60.015s、234 samples、平均 FPS 59.937、最低 54.577、平均主线程 4.194ms、最长帧 170.477ms，预算满足；GPU counter unavailable 原样记录。

结论：对象池的本机状态复用风险已修复并回归通过；A01 仍因目标设备、真实大场景、最终资产和跨平台等外部证据保持 **IN_PROGRESS**。

## 2026-08-12 对象池状态覆盖增强

- 回归夹具增加 AudioSource 和原本禁用的 Animator，验证 Frozen/静音后归还的实例不会把状态带给下一个租借者；原本禁用的 Animator 不会被降级逻辑错误打开。
- 专项 `tools/results/d16_a01_pool_reset_unity6_v3.xml`：**1/1 PASS**。
- 全量 `tools/results/d16_current_full_after_pool_reset_unity6_v3.xml`：**40/40 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 最新性能窗口：235 samples、平均 FPS 59.979、最低 58.817、平均主线程 4.357ms、最长帧 183.410ms；预算满足，GPU counter unavailable 原样记录。

本轮只扩展 D16 表现层回归覆盖，没有改变数据所有权或最终资源状态；A01 继续 **IN_PROGRESS**。

## 2026-08-12 主机与静态场景基线

- `tools/results/d16_a01_hardware_baseline_current.json` 记录当前 Windows 主机：20 logical processors、约 31.8 GiB RAM、RTX 4070 Ti / 12,282 MiB VRAM；明确不是目标设备或低端设备证据。
- `tools/results/d16_a01_static_baseline_current.json` 记录 `InitialHarbor.unity` 静态清单：24 GameObjects、17 MeshRenderer、17 MeshFilter、0 静态 Collider；运行时指标未伪造，保持 null。
- 该基线用于解释当前本机结果的硬件和场景范围，不关闭 A01 的目标设备、真实大场景、最终资源规模、GPU/内存及长线门禁。

## 2026-08-12 D2 控制闭环后的 A01 回归快照

- 本轮新增的 InitialHarbor 控制闭环不是 A01 性能门禁，但用于确认性能优化没有破坏玩法接线：`D16A03InitialHarborKeyboardCameraAndBoardingLoopWorks` **1/1 PASS**，全量 D16 PlayMode **39/39 PASS**。
- 最新性能报告 `tools/results/d16_a01_performance_window_report.json`：60.0156s / 234 samples / 平均 FPS 59.975 / 最低 FPS 57.264 / 平均主线程 4.434ms；最长全帧 175.977ms，启动后 0.0244s 发生，超过 50ms 1 次。
- D16 控制器阶段耗时为 Awake 0.478ms、首次注册刷新 0.154ms、首次 Update 0.155ms、首次预算评估 0.018ms；这些数据仍不支持把冷启动长帧归因于 D16 控制器单一阶段。
- 本轮没有把 GPU unavailable、编辑器窗口指标或 RTX 4070 Ti 主机数据扩大解释为目标/低端设备通过。A01 继续 **IN_PROGRESS**，后续交付仍需目标设备、真实大场景、最终资源 LOD、GPU/内存和跨平台证据。

## 2026-08-12 表现降级与事实/存档保护回归

### 状态

**IN_PROGRESS**。本轮关闭了 A01 的本机代码级状态保护缺口，但没有把它扩大为完整 A01 通过。

### 修改文件与边界

- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：新增 `D16A01PresentationDowngradePreservesFactsSaveAndSettlement`；
- 仅新增 D16 PlayMode 验收夹具和结果报告，不修改 `Systems/`、Save 生产逻辑、存档 Schema、领域数值或跨端契约；
- D16 表现层仍只写 Renderer/Animator/Audio，事实日志、保存投影、结算幂等门和碰撞由原系统持有。

### 原始验证

- Unity 6 PlayMode 专项：`tools/results/d16_a01_state_preservation_unity6_v2.xml`，**1/1 PASS**；
- 报告：`tools/results/d16_a01_state_preservation_report.json`；
- 报告关键字段：事实数量 1→1、保存序列保持、事实恢复成功、已结算再次声明返回 `AlreadyClaimed`、碰撞体保持启用；
- 该用例真实经历 Reduced、Background、Frozen、Full 四种表现降级。

### 验收结论与未完成事项

本机代码证据证明表现预算降级不会直接破坏 EventLog 快照、事实内容、结算幂等门或碰撞体。仍缺目标/低端设备、真实大场景、GPU/内存、最终资源、物理断电/跨平台和完整 D16-S01 长线证据；D16-A01 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

### 全量回归结果

- D16 PlayMode 全量 `tools/results/d16_current_full_after_state_preservation_v2_unity6.xml`：**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped；
- 当前性能窗口：60.015 秒、234 samples、平均 FPS 59.972、最低 58.806、平均主线程 4.356ms、最长全帧 174.150ms（启动后 0.0256 秒）；预算满足；
- 场景加载 0.2038s、D5 Ready 0.1067s、GC Gen0/1/2 为 22/22/22；GPU counter unavailable 原样保留。

全量回归确认新增状态保护测试没有破坏 D16 现有输入、菜单、字幕、D2 控制闭环、流式、LOD、对象池和音频生命周期用例；外部门禁和 A01 总体状态不变。

## 2026-08-12 当前工作树 Unity 6 全量复验

### 原始结果

- 当前 Unity 6 图形模式 PlayMode：`tools/results/d16_current_full_continuation_unity6.xml`，**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时 71.808s；
- 当前性能窗口：60.015s、234 samples、平均 FPS 59.934、最低采样 FPS 53.428、平均主线程 4.331ms、最长全帧 172.579ms（启动后 0.0246s）、超过 50ms 1 次；
- 场景加载 0.1998s、D5 Ready 0.1066s、GC Gen0/1/2 为 22/22/22；`BudgetSatisfied=true`、`ProtectedOverBudgetCount=0`；GPU counter unavailable 原样保留。

### 结论与边界

当前工作树的本机 Unity 代码回归保持通过，未新增 D16 测试失败。A01 仍不是总体 PASS：目标/低端设备、真实大场景、最终资源 LOD/音频、GPU/内存目标、跨平台和长线证据仍缺；D16-S01/S02 不启动。

## 2026-08-12 性能报告同屏与流式计数补齐

### 修改与所有权

- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`：A01 性能窗口报告新增同屏 Agent/动物、音频源、加载单元、登记、流式切换和对象池计数的落盘字段及探针映射；
- `tools/tests/test_d16_a01_performance_report.py`：新增字段存在性与非负值检查；
- 只修改验收报告/工具测试模块，不改变 D16 生产预算裁决、领域状态、事件、存档或跨端契约。

### 原始验证

- Unity 6 D16 PlayMode：`tools/results/d16_current_full_metrics_fields_unity6.xml`，**41/41 PASS**，0 failed / 0 inconclusive / 0 skipped，耗时 71.780s；
- 当前报告实际字段：`ActiveAgentCount=1`、`ActiveAnimalCount=0`、`AudibleSourceCount=1`、`LoadedCellCount=2`、`RegistrationCount=3`，对象池计数均为真实 0；
- A01 性能报告专项 **2/2 PASS**，工具测试全量 **68/68 PASS**；三套关键工程构建均 0 warning / 0 error。

### 结论与边界

同屏数量和流式/对象池计数现在有可追踪运行时输出；本机证据仍不替代目标/低端设备、真实大场景、最终资产、GPU/内存和长线性能验收，A01 继续 **IN_PROGRESS**。

## 2026-08-12 表现流式加载分帧回归

### 本轮实现

- `D16WorldStreamCell` 保存表现子树第一次观察到的 `activeSelf` 意图，加载时按 `activationBatchSize` 跨帧激活原本活跃的表现对象。
- 单元暴露 `IsLoading`、`LoadingProgress`、已加载/待加载对象数、最近完成加载耗时和加载次数；卸载停止协程、隐藏表现根并释放已登记的音频引用。
- `D16WorldPerformanceController` 和 `D16RuntimePerformanceProbe.Report` 记录加载中的单元、已加载/待加载表现对象、最大完成加载耗时和加载次数；验收报告包装类与 Python 字段校验同步更新。
- 仍严格限制在表现层，不修改领域实体、碰撞、事件、存档、结算或玩法数值。

### 原始证据

- 专项 `tools/results/d16_a01_staged_streaming_unity6.xml`：**1/1 PASS**。
- 全量 `tools/results/d16_a01_staged_streaming_full_current_unity6.xml`：D16 PlayMode **42/42 PASS**，0 failed / 0 inconclusive / 0 skipped。
- 性能窗口专项 `tools/results/d16_a01_staged_streaming_performance_current_unity6.xml`：**1/1 PASS**；当前报告已落盘 `PresentationLoadCount=2`、`LoadingCellCount=0`、`PendingPresentationObjectCount=0`。
- 当前本机 60 秒窗口：236 samples、平均 FPS 63.368、最低 FPS 3.508、最长全帧 285.071ms；GPU counter unavailable 原样保留。该窗口没有被写成性能达标。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；A01 报告专项 2/2 PASS。

### 交接边界

本轮只关闭表现流式的分帧和指标落盘代码缺口。A01 仍缺目标/低端设备、真实大场景、最终资源 LOD/音频、GPU/内存、跨平台和长线证据；继续保持 **IN_PROGRESS**。D16-S01/S02 不启动。

## 2026-08-12 远景建筑代理表现接线

### 本轮实现

- 新增 `D16RemoteBuildingProxy`，完整表现根与远景代理根仅作为表现层切换目标；预算控制器不改变建筑逻辑碰撞体、交互状态或领域数据。
- `D16WorldPerformanceController` 纳入远景建筑代理候选，按预算层在完整表现、远景代理和冻结隐藏之间切换；代理组件仍属于 D16 表现模块，未向 Systems 增加网络或 LLM 依赖。
- 新增 `D16A01RemoteBuildingProxyUsesBackgroundTierWithoutTouchingLogicCollider`，验证远景切换到代理、近景恢复完整表现，逻辑碰撞体两种状态均保持启用。

### 原始证据

- 聚焦 `tools/results/d16_a01_remote_building_proxy_unity6_rerun.xml`：**1/1 PASS**。
- 全量 `tools/results/d16_a01_remote_building_proxy_full_current_unity6.xml`：D16 PlayMode **43/43 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 交接边界

本轮只补齐运行时远景建筑代理能力和碰撞隔离证据；当前没有把测试夹具当作最终美术交付，项目仍缺实际最终代理资源/批量场景配置以及 A01 的目标/低端设备、真实大场景、GPU/内存、跨平台和长线证据。A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D3D12 GPU 计时复核

- 强制 Direct3D 12 的性能窗口证据：`tools/results/d16_a01_performance_d3d12_current_unity6.xml` **1/1 PASS**，耗时约 60.45s。
- 当前报告真实值：平均 FPS 64.904、最低采样 FPS 3.609、最长全帧 277.109ms、平均主线程 4.676ms；冷启动长帧和长帧计数原样保留。
- 即使使用 D3D12，`GpuSampleAvailable=false`、`GpuTimeMs=-1`，没有用 CPU 或估算数据伪造 GPU 结果。

### 交接边界

该复核只确认 GPU 计时不可用并保留缺口，不构成 GPU 性能通过；目标/低端设备、真实大场景、最终资源、跨平台和长线证据仍未满足，A01 继续 **IN_PROGRESS**。

## 2026-08-12 GPU counter 候选探测回归

### 本轮实现

- `D16RuntimePerformanceProbe` 依次探测三个明确 GPU-only counter，并记录实际选中的 `GpuCounterName`；只有 recorder 有效才选用，采样为空时仍保持 GPU unavailable。
- `D16RuntimeAcceptanceTests` 的性能报告同步落盘 `GpuCounterName`，让句柄状态和样本状态可区分。

### 原始证据

- `tools/results/d16_a01_performance_gpu_counter_probe_d3d12_unity6.xml`：**1/1 PASS**，约 60.44s。
- D3D12/RTX 4070 Ti 报告：`GpuCounterName=GPU Frame Time`、`GpuSampleAvailable=false`、`GpuTimeMs=-1`；平均 FPS 65.904、最低采样 FPS 3.608、最长全帧 277.177ms。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 交接边界

本轮只增强 GPU 计时观测，不构成 GPU 性能通过，也不替代目标/低端设备、真实大场景、跨平台和长线证据；A01 继续 **IN_PROGRESS**。

### 全量回归

- `tools/results/d16_a01_gpu_counter_probe_full_current_unity6.xml`：D16 PlayMode **43/43 PASS**，0 failed / 0 inconclusive / 0 skipped，约 72.36s。
- 聚焦探测和全量回归未发现新增运行时异常；GPU 样本缺口仍按 unavailable 保留。

### 最终自动化复核

- 全量工具测试 **69/69 PASS**（108.107s）。
- AgentCore、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests：均 0 warning / 0 error；Systems 边界扫描与 `git diff --check` PASS。

## 2026-08-12 共享音频作用域卸载隔离

### 本轮实现

- `D16AudioResourceScope` 增加资源路径快照和增量释放入口；释放时仍沿用共享引用计数和明确归属检查。
- `D16WorldStreamCell` 记录每次加载前的音频路径基线，卸载只释放该单元加载期间新增的资源路径，避免调用整个 scope 的 `ReleaseAll()` 误伤其他流式单元。
- 新增跨单元共享作用域 PlayMode 用例；旧用例同步调整为真实的“配置单元 → 单元加载 → 登记该单元音频”顺序。
- 仍只修改 D16 表现层和验收测试；没有修改玩法系统、存档、事件事实、碰撞裁决、contracts 或网络代码。

### 原始验证

- `tools/results/d16_a01_shared_audio_scope_current_unity6.xml`：共享作用域聚焦 **1/1 PASS**。
- `tools/results/d16_a01_audio_scope_legacy_current_unity6.xml`：原有音频生命周期聚焦 **1/1 PASS**。
- `tools/results/d16_a01_shared_audio_scope_full_fixed_unity6.xml`：D16 PlayMode **47/47 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 结论与边界

本轮关闭共享音频 scope 在流式卸载时的代码级引用隔离风险；没有把本机夹具扩大解释为最终音频资源、目标设备内存、真实大场景、GPU、跨平台或长线性能通过。A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

### 同路径多 owner 加固

- `D16AudioResourceScope` 新增 `TrackLoadedClipForOwner(...)`、`ReleaseOwner(...)`；同一 scope/路径可以被多个流式 owner 持有，底层 clip 只在所有 owner 和无 owner 引用都释放后进入释放判断。
- `D16WorldStreamCell.TrackAudioResource(...)` 为表现加载器提供当前单元 owner 入口；单元卸载/销毁先释放 owner，再处理兼容旧入口产生的新增路径。
- `tools/results/d16_a01_shared_audio_owner_current_unity6.xml`：聚焦 **1/1 PASS**；`tools/results/d16_a01_shared_audio_owner_full_current_unity6.xml`：D16 PlayMode **47/47 PASS**。
- `Assembly-CSharp`、`D16.PlayMode.Tests` 均 0 warning / 0 error。

该加固只关闭同路径多 owner 的本机表现引用风险；最终音频资源、目标/低端设备、真实大场景、GPU/内存、跨平台和长线证据仍缺，A01 继续 **IN_PROGRESS**。

## 2026-08-12 预加载音频 owner 生命周期补强

### 修改

- `D16WorldStreamCell.TrackAudioResource(...)` 现在在单元尚未进入加载态时也先建立该单元 owner，避免预加载音频进入无 owner 集合后无法由单元卸载。
- 新增 `D16A01AudioRegisteredBeforeFirstLoadIsOwnedByCell`，验证 startLoaded=false 的单元在首次加载前登记音频，随后加载/卸载可完整回收。
- 未修改 Systems、领域事实、事件、存档、碰撞裁决、contracts 或网络代码；数据仍只属于 D16 表现资源生命周期模块。

### 原始验证

- `tools/results/d16_a01_preload_audio_owner_current_unity6.xml`：**1/1 PASS**。
- `tools/results/d16_a01_preload_audio_full_current_unity6.xml`：D16 PlayMode **48/48 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。

### 结论与边界

本轮关闭预加载音频引用的本机生命周期缺口；A01 的目标/低端设备、真实大场景、最终资产、GPU/内存、跨平台和长线证据仍未取得，A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 未加载单元音频清理与销毁回归

### 本轮实现

- `D16WorldStreamCell.SetLoadedInternal(...)` 的 unloaded 早退路径现在也执行 owner 清理；表现根为空或单元从未进入加载态时，不再把已登记的音频引用留在 scope 中。
- `OnDestroy()` 复用统一清理入口，并仅在存在加载基线时执行增量路径清理，避免没有基线时误释放共享 scope 的其他资源。
- 新增 `D16A01UnloadedCellReleasesOwnedAudioWithoutPresentationRoot`、`D16A01DestroyBeforeFirstLoadReleasesOwnedAudio`，验证无表现根、重复设置 unloaded 和首次加载前销毁。

### 原始验证

- `tools/results/d16_a01_unloaded_audio_current_unity6.xml`：**1/1 PASS**。
- `tools/results/d16_a01_destroy_before_load_current_unity6.xml`：**1/1 PASS**。
- `tools/results/d16_a01_audio_lifecycle_full_current_unity6.xml`：D16 PlayMode **50/50 PASS**，0 failed / 0 inconclusive / 0 skipped，约 73.36s。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`、`dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：均 0 warning / 0 error。

### 交接边界

该轮只关闭 D16 A01 本机表现音频 owner 生命周期风险，不改变玩法事实、事件、存档、碰撞裁决、contracts 或网络代码。A01 仍缺目标/低端设备、真实大场景、最终资源、GPU/内存、跨平台和长线证据；D16-S01/S02 不启动。

## 2026-08-12 流式单元对象池重新配置清理

### 本轮实现

- `D16WorldStreamCell.ConfigureRuntime(...)` 现在在覆盖旧配置前停止旧加载协程、释放旧 audio owner/基线，并停用旧表现根，避免对象池复用时旧 scope 被遗留或旧模型继续显示。
- 新增 `D16A01ReconfigureReleasesPreviousAudioOwnerAndPresentationRoot`，覆盖同一流式对象从第一组表现配置切换到第二组配置的生命周期。

### 原始验证

- `tools/results/d16_a01_reconfigure_audio_current_unity6.xml`：**1/1 PASS**。
- `tools/results/d16_a01_reconfigure_full_current_unity6.xml`：D16 PlayMode **51/51 PASS**，0 failed / 0 inconclusive / 0 skipped，约 73.32s。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`、`dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：均 0 warning / 0 error。

### 交接边界

该轮只关闭对象池重新配置时的 D16 A01 本机表现资源生命周期风险，不改变玩法事实、事件、存档、碰撞裁决、contracts 或网络代码。真实岛屿/海域最终表现资源、目标/低端设备、GPU/内存、跨平台和长线证据仍缺；A01 继续 IN_PROGRESS，D16-S01/S02 不启动。

## 2026-08-12 岛屿数据流注册与可见性事实

### 本轮实现

- 新增 `D16IslandDataStreamRegistry`，从 `D5IslandCatalog.All` 建立 12 个稳定岛屿数据单元，并读取当前 D5 快照计算海图距离模拟等级和 metadata active；该组件是 D16 表现/性能层的只读注册层。
- `D16WorldPerformanceController`、`D16RuntimePerformanceProbe` 和性能 JSON 报告新增目录、活动数据、data-only、视觉根和参考岛屿指标。
- 视觉根只能由明确的 `RegisterVisualPresentation(...)` 注册；当前生产场景没有岛屿 3D 资源，运行时保持 12 个 data-only / 0 个视觉表现根。没有创建占位模型，也没有把海图标记当作 3D 内容。

### 原始验证

- `tools/results/d16_a01_island_data_stream_current_unity6.xml`：**1/1 PASS**。
- `tools/results/d16_a01_island_data_stream_full_current_unity6.xml`：D16 PlayMode **53/53 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `tools/results/d16_a01_performance_window_current_unity6.xml`：性能窗口 **1/1 PASS**；报告记录 12 catalog / 7 active data / 12 data-only / 0 visual presentation。
- `test_d16_a01_performance_report.py`：**2/2 PASS**；工具测试全集 **69/69 PASS**。
- `Assembly-CSharp`、`D16.PlayMode.Tests`、D16 Runtime 工程均 0 warning / 0 error。

## 2026-08-12 D3D11 性能能力字段补强

### 修改

- `D16RuntimePerformanceProbe.Report` 新增 `GraphicsDeviceType`、`GraphicsDeviceVersion`、`SupportsGpuRecorder`、`FrameTimingFeatureEnabled`；运行时直接读取 Unity 能力事实。
- A01 性能窗口报告同步写入上述字段，并拒绝“GPU 采样可用但 GPU 帧时间不为正数”的伪指标状态。
- 未修改 Systems、存档、事件、玩法数值、contracts 或数据所有权。

### 验证

- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error；
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error；
- Unity D3D11 性能窗口 `tools/results/d16_a01_performance_capability_current.xml`：1/1 PASS；真实设备为 RTX 4070 Ti / Direct3D11；234 samples、平均 65.82 FPS、最长帧 302.78ms；GPU 帧时间仍 unavailable；
- 结果文件：`tools/results/d16_a01_performance_window_report.json`。

### 结论

本轮只提高 A01 本机 GPU 能力证据的可解释性，不关闭目标/低端设备、真实大场景、最终岛屿资源、GPU/内存、跨平台和长线门禁。A01 继续 **IN_PROGRESS**，D16-S01/S02 不启动。

随后使用同一 D3D11 参数执行全量 `D16.PlayMode.Tests`：`tools/results/d16_full_after_performance_capability_unity6.xml` 为 **53/53 PASS**，0 failed / 0 inconclusive / 0 skipped；该改动没有造成其他 D16 运行时回归。

### 交接边界

本轮只关闭岛屿目录数据流缺口和指标可见性，不改变 IslandSystem、存档、事件、碰撞、contracts 或网络代码。0 个视觉表现根是当前资源事实，不是 A01 完成标志；真实岛屿/海域最终资产、目标/低端设备、GPU/内存、跨平台、长线和人工门禁仍缺，A01 保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 当前工作区完整回归复验

- 受控恢复 Unity 临时工程依赖后，重新执行 `D16.PlayMode.Tests` 全量：`tools/results/d16_current_full_revalidation_unity6.xml`，**53/53 PASS**，0 failed / 0 skipped / 0 inconclusive。
- 本次只复验现有代码，没有把 0 个视觉表现根、64.54 FPS 的本机性能窗口或自动化绿灯扩大解释为真实资产、目标设备或最终版本通过。
- A01 仍等待目标/低端设备性能、真实岛屿/海域表现资产、GPU/内存、跨平台和长线证据；D16-S01/S02 不启动。

## 2026-08-12 D5 启动阶段函数级归因

### 本轮实现

- `D5RuntimeBootstrap` 增加只读阶段计时：等待玩家、加载快照、创建系统、初始化状态、应用角色表现、构建海图，并记录 Awake、总构建耗时、已完成阶段数和完整性标记。
- `D16RuntimePerformanceProbe` 不直接引用 D5 Runtime 程序集，改为按公开诊断属性读取阶段事实；这保持 D16 Runtime → 领域入口的程序集依赖单向，不改变数据所有权。
- A01 性能窗口把 D5 阶段完整性加入报告门槛；缺少 6 个阶段任一项时，性能窗口会失败而不是静默生成不完整证据。

### 原始验证

- `tools/results/d16_a01_phase_attribution_current_unity6.xml`：Unity 6 D3D11 图形 PlayMode 聚焦 **1/1 PASS**；
- `tools/results/d16_a01_performance_window_report.json`：60.014 秒、234 samples、平均 FPS 65.71、最低 FPS 3.49、最长全帧 286.43ms（启动后 0.091s），GPU 计时 unavailable；
- D5 阶段：Awake 3.424ms、总构建 227.823ms；等待玩家 91.676ms、加载快照 0.006ms、创建系统 1.200ms、初始化状态 0.518ms、角色表现 0.681ms、海图构建 2.407ms，6/6 完整；
- `tools/results/d16_full_after_phase_attribution_20260812.xml`：D16 PlayMode **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped；性能报告工具测试 **2/2 PASS**；
- D16 Runtime、Assembly-CSharp、D16.PlayMode.Tests 构建均 0 warning / 0 error。

### 结论与边界

本轮关闭 A01 “启动阶段没有可追踪阶段证据”的本机代码子缺口；286ms 长帧仍未完全归因，且目标/低端设备、真实大场景、最终资源 LOD、GPU/内存、跨平台和长线证据仍缺。A01 总体保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 D2 启动阶段函数级归因

### 本轮实现

- `D2RuntimeBootstrap` 增加创建船、船员锚点、玩家、物理、控制、表现六阶段的只读计时，以及总构建耗时、完成阶段数和完整性标记。
- `D16RuntimePerformanceProbe` 通过公开诊断属性读取 D2 事实，不反向建立 D16 Runtime → D2 程序集依赖；不改变玩法、碰撞、存档或数据所有权。
- A01 性能窗口同时要求 D2/D5 六阶段完整，避免只报告部分启动链路。

### 原始验证

- `tools/results/d16_a01_d2_phase_attribution_current_unity6.xml`：Unity 6 D3D11 图形 PlayMode 聚焦 **1/1 PASS**；
- 当前性能报告：235 samples、平均 FPS 65.05、最低 FPS 3.56、最长全帧 280.86ms（启动后 0.088s），GPU unavailable；
- D2 总构建 165.364ms：船 0.168ms、船员锚点 0.168ms、玩家 1.024ms、物理 13.506ms、控制 6.580ms、表现 8.711ms，6/6 完整；
- D5 同窗口总构建 227.037ms，6/6 完整；D2/D5 之外的场景资源与其他 Bootstrap 仍未完全归因。
- `tools/results/d16_full_after_d2_phase_attribution_20260812.xml`：D16 PlayMode 全量 **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped。

### 结论与边界

本轮关闭 A01 本机 D2 阶段观测子缺口；280.86ms 长帧仍保留为未完全归因的真实性能问题。目标/低端设备、真实大场景、最终资源 LOD、GPU/内存、跨平台和长线证据仍缺，A01 总体保持 **IN_PROGRESS**，D16-S01/S02 不启动。

## 2026-08-12 A01 D3/D4/D9 启动链就绪归因（追加）

### 状态

**本机子门禁 PASS；D16-A01 总体 IN_PROGRESS。**

### 修改文件

- `game/Assets/Scripts/Gameplay/D16/D16RuntimePerformanceProbe.cs`
- `game/Assets/Tests/PlayMode/D16RuntimeAcceptanceTests.cs`
- `tools/tests/test_d16_a01_performance_report.py`

### 新增诊断接口

- 性能报告新增 D3/D4/D9 三个只读就绪时间字段：`D3RuntimeReadySeconds`、`D4RuntimeReadySeconds`、`D9RuntimeReadySeconds`。
- 诊断层只通过公开 `IsReady` 属性观察入口，未建立 D16 Runtime 对 D3/D4/D9 程序集的反向编译依赖；没有新增契约、存档字段或领域写者。

### 原始验证

- `tools/results/d16_a01_d3_d4_d9_readiness_graphics_current_unity6.xml`：聚焦 **1/1 PASS**。
- 全量回归后的 `tools/results/d16_a01_performance_window_report.json`：D3/D4/D9 分别约 **0.1025s** 就绪；233 samples、平均 59.98 FPS、最低 57.50 FPS、最长帧 181.47ms；D3D11 RTX 4070 Ti；GPU 计时 unavailable。聚焦 XML 仍为独立的 1/1 PASS 证据。
- `tools/results/d16_full_after_d3_d4_d9_readiness_20260812.xml`：全量 **54/54 PASS**，0 failed / 0 inconclusive / 0 skipped。
- `dotnet build game/Assembly-CSharp.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `dotnet build game/D16.PlayMode.Tests.csproj --no-restore -v:minimal`：0 warning / 0 error。
- `python -m unittest tools.tests.test_d16_a01_performance_report -q`：2/2 PASS。
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：71/71 PASS。

### 数据所有权与边界

本轮只修改 D16 性能观测模块和对应测试；D3/D4/D9 仍由各自运行时入口及 Systems 写入领域状态。没有修改 Systems、网络、LLM、contracts、存档、资源授权或 Steam 状态。

### 未完成事项与下游

- A01 仍缺目标/低端设备、真实大场景、最终资源、GPU/内存和长线证据；启动最长帧仍需进一步归因。
- A02—A06 的外部真实性门禁仍未闭合；因此 D16-S01/S02 不启动。
## 2026-08-12 总纲顺序下的 A01 当前复验

### 状态

**本机代码/采样子门禁 PASS；D16-A01 总体 IN_PROGRESS。**

### 当前证据

- 静态场景基线 `tools/results/d16_a01_static_baseline_current_goal.json`：InitialHarbor 24 GameObjects、17 MeshRenderers、0 静态 Collider；运行时碰撞不由该静态报告宣称完成；
- 主机基线 `tools/results/d16_a01_hardware_baseline_current_goal.json`：本机 20 logical CPU、1 GPU；仅为本机基线；
- Unity D3D11 全量 `tools/results/d16_full_current_goal_20260812.xml`：**54/54 PASS**，failed/inconclusive/skipped 均为 0；
- 当前性能窗口：60.015s、233 samples、平均 59.98 FPS、最低 57.50 FPS、最长帧 181.47ms；D2/D3/D4/D5/D9 启动归因字段均存在，GPU 帧时间 unavailable；
- `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：**71/71 PASS**；相关工程在恢复临时依赖后均 0 warning / 0 error。

### 边界

本轮没有生成岛屿最终 3D 资源，也没有把静态数据流、程序几何或本机 RTX 结果当作目标设备、真实大场景、GPU/内存、跨平台或长线证据。A01 继续 IN_PROGRESS；D16-S01/S02 不启动。

## 2026-08-12 工具入口纠正与 A01 基线复验

- `python tools/d16_a01_hardware_baseline.py --json-out tools/results/d16_a01_hardware_baseline_current_goal2_corrected_20260812.json`：成功；本机 20 logical CPU、1 GPU。该报告明确标记为 `local_host_only`，不作为目标/低端设备或游戏 GPU 帧时间证据。
- `python tools/d16_a01_static_baseline.py game/Assets/Scenes/InitialHarbor.unity --json-out tools/results/d16_a01_static_baseline_current_goal2_corrected_20260812.json`：成功；InitialHarbor 为 24 GameObjects、17 MeshRenderers、0 静态 Collider。该静态扫描不宣称运行时碰撞已完成，反而保留当前物理实现的待核对事实。
- 本轮确认硬件脚本不接受 `game` 位置参数，静态脚本必须接收具体场景文件；没有把参数错误记为产品失败，也没有用错误输出生成证据。

结论：A01 本机基线入口复验 PASS；总体仍 **IN_PROGRESS**，因为目标/低端设备、真实大场景/最终资源、GPU/内存、跨平台和长线证据未取得。0 个视觉表现根与 0 个静态 Collider 继续作为必须在 D17/资源接入中解决的产品事实，不得被自动化绿灯覆盖。

## 2026-08-12 当前最终复核

- Unity 6 D17 本机连续故事全量回归 `tools/results/d17_full_after_project_identity_20260812.xml`：**60/60 PASS**；
- 工具测试当前：**77/77 PASS**；`Assembly-CSharp`、`AgentCore`、`Save.Tests`、`D16.PlayMode.Tests` 构建均 0 warning / 0 error；
- A01 总体仍 **IN_PROGRESS**：本机 RTX 4070 Ti 结果只能作为开发机基线，目标/低端设备、真实大场景、最终资源 GPU/内存和长线证据仍缺。

## 2026-08-13 Unity 路径更正后的本机回归快照

- 已确认 Unity 6 编辑器实际路径为 `C:\Program Files\Unity 6000.0.80f1\Editor\Unity.exe`；当前最终 PlayMode 回归 `game/tools/results/d17_playmode_full_final_20260813.xml` 为 **60/60 PASS**。
- 本机回归只证明当前 Windows/RTX 4070 Ti 开发环境没有代码回退；目标/低端设备、真实大场景、最终资源 GPU/内存和长线证据仍缺，A01 总体继续 **IN_PROGRESS**。
