# D19-R03-A03 真实 Player 1080p 性能取证

结论：**PARTIAL**。本报告只结算性能子任务，不把性能通过误写为 D19 最终 PASS。

## 采集方式

历史基线使用 `game/tools/results/D19/D19-R03-A03/D19_R03_A03_Player_RERUN1.exe`；该组原始记录保留。最新复采使用 `game/tools/results/D19/D19-R03-A03/D19_R03_A03_Player_RERUN16.exe`，Unity `6000.0.80f1`，NVIDIA GeForce RTX 4070 Ti，Direct3D11。RERUN16 使用 `-screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -force-d3d11 -d19-r03-perf-harbor`，由性能探针明确加载 `InitialHarbor` 作为孤立场景基准；运行时探针记录 `Screen.width/height` 而非从截图推断。每档预热 30 秒、采样 60 秒。

| 请求档 | 实际 Unity 档名/索引 | Screen | VSync | 平均 FPS | 1% low FPS | GPU ms | 样本 |
|---|---|---:|---:|---:|---:|---:|---:|
| low | Very Low / 0 | 1920×1080 | 0 | 5234.08 | 59.08 | 0.103 | 95,816 |
| medium | Low / 1 | 1920×1080 | 0 | 5072.22 | 58.29 | 0.119 | 46,818 |
| high | Medium / 2 | 1920×1080 | 1 | 61.29 | 42.73 | 0.496 | 3,616 |

上表是 RERUN1 的历史记录，保留用于对照。RERUN13 修复了生成网格持久化后，RERUN16 在真实 Player 的 `InitialHarbor` 重新采样如下：

| 请求档 | 实际 Unity 档名/索引 | 场景 | Screen | VSync | 平均 FPS | 1% low FPS | 主线程 ms | GPU ms | GC Gen0 | Reserved |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|
| low | Very Low / 0 | InitialHarbor | 1920×1080 | 0 | 60.00 | 59.72 | 3.759 | 1.544 | 132 | 208,482,304 |
| medium | Low / 1 | InitialHarbor | 1920×1080 | 0 | 59.99 | 59.53 | 3.775 | 1.536 | 132 | 204,288,000 |
| high | Medium / 2 | InitialHarbor | 1920×1080 | 1 | 59.99 | 59.56 | 3.764 | 1.523 | 132 | 204,288,000 |

RERUN16 原始 JSON：

- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_low_rerun16.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_medium_rerun16.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_high_rerun16.json`
- Player 画面：`game/tools/results/D19/D19-R03-A03/perf_harbor_rerun16_low.jpg`、`perf_harbor_rerun16_medium.jpg`、`perf_harbor_rerun16_high.jpg`

原始 JSON：

- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_low.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_medium.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_high.json`
- 汇总：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_SUMMARY.json`

## 解释与限制

运行时质量档名称证明当前项目的索引 0/1/2 实际为 Very Low/Low/Medium；请求参数标签保留为 low/medium/high，不能把索引 2 伪称 High。RERUN16 三次数据均明确记录 `InitialHarbor`、真实分辨率、VSync、GC、内存、GPU counter 和 1% low，但这不能证明港口最终视觉、天气、路线或 D19 最终验收通过。RERUN1 的 `MainMenu` 场景名仅属于历史基线，不能覆盖 RERUN16 的港口采样。

机器结果：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT.json`。

## RERUN17 补充（2026-08-16）

RERUN17 修复的是取证探针的质量档解析，不是 QualitySettings 或玩法逻辑：探针现在按运行时 `QualitySettings.names` 解析请求档名。新的非调试 Player 采样按 `low/medium/high` 得到实际 `Low / Medium / High`（索引 1/2/3），三档均为真实 Windows Player、InitialHarbor、1920×1080、30 秒预热 + 60 秒采样；每档都有 GPU Frame Time、1% low、GC 和内存读回。原 RERUN16 的 Very Low/Low/Medium 记录不覆盖，继续保留作为历史对照。

| 请求档 | 实际档名/索引 | 平均 FPS | 1% low FPS | 主线程 ms | GPU ms | Reserved | GC Gen0/1/2 |
|---|---|---:|---:|---:|---:|---:|---:|
| low | Low / 1 | 59.994 | 59.526 | 5.162 | 1.495 | 265,302,016 | 122/122/122 |
| medium | Medium / 2 | 59.995 | 59.559 | 5.129 | 1.555 | 265,302,016 | 122/122/122 |
| high | High / 3 | 59.996 | 59.647 | 5.131 | 1.480 | 265,302,016 | 122/122/122 |

原始 JSON：

- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_low_rerun17.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_medium_rerun17.json`
- `game/tools/results/D19/D19-R03-A03/D19_R03_A03_PERFORMANCE_high_rerun17.json`
- 汇总：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN17.json`

因此 D19-R03-A03 的性能子任务本轮为 **PASS（仅限性能取证范围）**。这不关闭 D19-R03-A01 的完整玩家路线/保存重载，也不关闭 D19-R03-A02 的固定镜头、天气和视觉债务门禁。
