# D19-R03-A03 交接

- 任务：D19-R03-A03
- 结论：**PARTIAL**（性能子任务证据真实完成；D19 最终门禁仍未通过）

## 交付

- 运行时只读探针：`game/Assets/Scripts/Gameplay/D19/R03/D19R03PerformanceProbe.cs`
- Unity 编译：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_UNITY_COMPILE.log`，退出码 0。
- Player 构建：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_PLAYER_BUILD_RERUN1.log`，退出码 0。
- 三档真实结果和汇总见 `game/tools/results/D19/D19-R03-A03/`。

## 真实结果

三档均记录 `Screen=1920x1080`、实际质量索引/名称、VSync、30+60 秒、1% low、GC、内存和 GPU counter。当前质量资产只有 Very Low/Low/Medium 三个实际索引，不能把 High 标签当成 High 画质；因此门禁只记录事实，不擅自改 QualitySettings。

## 未完成

完整 WASD 路线、交互/保存恢复、14 固定 Player 镜头、3 天气变体和最终 D19 视觉质量不属于本性能探针的通过范围，继续由 D19-R03-A01/A02/S01 处理。没有更改任何玩法强逻辑或存档事实。

## RERUN16 港口复采增量（2026-08-16）

- 保留 RERUN1 的历史 MainMenu 记录；本次没有覆盖原始 JSON。性能探针增加了仅用于基准的 `-d19-r03-perf-harbor` 和结果后缀参数，RERUN16 明确加载 `InitialHarbor` 后采样。
- Unity Player build：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_PLAYER_BUILD_RERUN16.log`，构建退出码 0；Player：`D19_R03_A03_Player_RERUN16.exe`。
- 低档请求 → `Very Low / 0`：1920×1080，VSync 0，平均 59.997 FPS，1% low 59.720，主线程 3.759 ms，GPU 1.544 ms，GC Gen0 132，reserved 208,482,304 bytes。
- 中档请求 → `Low / 1`：1920×1080，VSync 0，平均 59.994 FPS，1% low 59.535，主线程 3.775 ms，GPU 1.536 ms，GC Gen0 132，reserved 204,288,000 bytes。
- 高档请求 → `Medium / 2`：1920×1080，VSync 1，平均 59.994 FPS，1% low 59.557，主线程 3.764 ms，GPU 1.523 ms，GC Gen0 132，reserved 204,288,000 bytes。
- 原始结果：`D19_R03_A03_PERFORMANCE_low_rerun16.json`、`medium_rerun16.json`、`high_rerun16.json`；三张真实 Player 基准截图同目录 `perf_harbor_rerun16_*.jpg`。

RERUN16 关闭了“几何修复后没有港口场景性能数据”的缺口，但项目实际画质档名仍不是 Low/Medium/High，且完整路线、14 固定镜头和天气门禁未完成，因此结论仍为 **PARTIAL**，不放行 D19 或 D20。

## RERUN36_CURRENT_SCENE 增量（2026-08-16）

- 为避免复用 RERUN35 几何变更前的性能基线，使用当前 R03 场景构建后的真实 Windows Player 重新采样 Low/Medium/High；每档 1920×1080，warm-up 约 30 秒、sample 约 60 秒，设备为 NVIDIA GeForce RTX 4070 Ti / Direct3D11。
- 原始结果：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_low_rerun36-current.json`、`medium_rerun36-current.json`、`high_rerun36-current.json`；汇总：`D19_R03_A03_MACHINE_RESULT_RERUN36_CURRENT.json`。
- Medium/High 平均 FPS 约 59.995，1% low 约 59.649/59.637，满足当前采样阈值；Low 平均 59.435 但真实 1% low 为 7.903，低于 D18 的 50 FPS 门槛，因此整体结论保持 **PARTIAL**，不丢弃该异常值。
- 该结果只更新当前场景性能事实，不代表路线、天气、视觉或 D19 最终验收通过。
- Low 复采 RERUN36_CURRENT2 真实完成 30.009s warm-up + 60.005s sample，平均 59.995 FPS、1% low 59.624 FPS；但首轮同场景原始结果的 1% low=7.903 FPS 仍保留。按“连续三轮取最差”规则，不能删除异常值或把性能子任务改写为 PASS。
# RERUN17 追加交接（2026-08-16）

- 本轮结论：**PASS（仅 D19-R03-A03 真实 1080p 性能子任务）**。
- 修正：性能探针按运行时 `QualitySettings.names` 解析请求档，避免把项目索引 0/1/2 错写为 Low/Medium/High。
- 真实 Player 证据：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_*_rerun17.json`；汇总 `game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN17.json`。
- 三档：实际 `Low/Medium/High`，1920×1080，30 秒预热 + 60 秒采样，平均 FPS 59.994/59.995/59.996，1% low 59.526/59.559/59.647，GPU Frame Time 1.495/1.555/1.480 ms。
- 仍未完成：真实玩家路线、市场—出航—返港—保存重载、固定镜头/天气和 D19 最终门禁；不能把本 PASS 扩大解释为 D19 PASS。
