# VC-G01 PlayMode 前置审计交接

日期：2026-09-05  
状态：`PASS_WITHIN_SCOPE / AUTOMATED_PLAYMODE_PRECHECK`  
`runtime_proof=false`

## 结论

VC-G01 相关的自动化 PlayMode 前置测试已通过 `15/15`：D19S02R01 路线 `1/1`、D16 输入路由 `10/10`、D19 交互 `2/2`、D19 路线 `2/2`。这证明合成输入下的输入路由、起始港口登船循环、路线状态转换和现有 D19 交互测试没有新增代码层失败。

## 证据

- [机器结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_PLAYMODE_RESULT.json)
- [InitialHarbor 路线 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_ROUTE_PLAYMODE.xml)
- [D16 输入 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE.xml)
- [D19 交互 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_INTERACTION_PLAYMODE.xml)
- [D19 路线 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_ROUTE_PLAYMODE.xml)
- [自动化路线探针结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-S02-R01/D19_S02_R01_ROUTE_LOG.json)

全仓库 Python 回归 `python -m unittest discover -s tools/tests -p 'test_*.py'`：`1085/1085 OK`；该回归包含 D27/D35 静态审计和 VC-G01 相关源码检查。

## 边界

- 测试通过的是 Unity Test Framework 的 `InputTestFixture` 合成设备事件和 PlayMode 场景，不是人工操作 standalone Player。
- 自动化路线只覆盖短时、确定性状态转换；不能证明真实 5—10 分钟路线、镜头构图、模型是否穿插、天气画面或目标设备性能。
- 因此 VC-G01 仍保持 `IN_PROGRESS`，VC-G02/VC-G03 继续锁定，D25—D31 正式状态不变。

## 下一步

启动最新 [RERUN48 Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe)，进入 `InitialHarbor`，完成真实 WASD/转向、E 登船/下船、I/M/T 入口、Esc 关闭恢复以及首屏观察；关闭后读取路线探针和 Player 日志，才能进行 VC-G01 正式判断。

## 2026-09-06 native-last 前置门禁重排

本 handoff 的原生 Player 下一步被新顺序覆盖：原生窗口和人工物理键盘不再是当前立即动作，而是必须等 `VC-G01-P0`—`P4` 无未解释阻塞后执行的最后一道门禁。历史 `15/15` 合成 PlayMode 结果和脚本驱动 Player 结果仍然保留，但都不等价于 native/manual PASS。

### 本轮实现

- D29 主快捷 HUD 不再附着 `D16ModalFocusScope`；仓库面板继续拥有自己的 modal scope。
- `HarborInteractionStabilizer` 每帧协调 `EventSystem.sendNavigationEvents`：gameplay 状态关闭 UI 导航，暂停/临时模态页面恢复导航，并清理残留 selection。
- 新增 `D16PrimaryHudNavigationPlayModeTests.GameplayMovementKeysDoNotNavigatePrimaryHud`，验证 W/S 轴方向、Market selection 不漂移、主 HUD 无 modal scope；定向 `1/1 PASS`。
- 修正 D16 A03 诊断关闭测试的同名 `Close` 全局查找，改为在诊断 modal 内查找；定向 `1/1 PASS`。这是测试定位修正，不是放宽产品行为。
- 更新 D31 静态审计和 D29/D30 矩阵，明确 D29 主 HUD 是 non-modal，modal surfaces 为 D29-Warehouse/D4/D9/D10；定向 Python 审计 `34/34 PASS`。

### 当前前置结果

- [VC-G01 pre-native gate JSON](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PRE_NATIVE_GATE_20260906.json)：整体 `IN_PROGRESS`，native `LOCKED`。
- [D16 主 HUD 导航定向 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PRIMARY_HUD_NAVIGATION_PLAYMODE_R3.xml)：`1/1 PASS`。
- [D16 A03 定向 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_A03_AFTER_UI_GATING_R2.xml)：`1/1 PASS`。
- [D19 单一主 HUD 定向 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_HUD_AFTER_UI_GATING_R2.xml)：`1/1 PASS`。
- [D19S02 当前 R03 活动路线定向 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_S02_AFTER_R03_ROUTE_R3.xml)：`1/1 PASS`；活动路线碰撞 `6/6`、NavMesh `6/6`，旧灰盒坐标诊断 `1/6` 仍单独保留。
- [全量 EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_AFTER_UI_GATING_20260906.xml)：`593/593 PASS`。
- [D16 PlayMode 清洁全量快照](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_AFTER_R03_ROUTE_R4.xml)：`68/71 PASS、2 FAIL、1 SKIP`。修正前的 D19 HUD 断言和 D19S02 旧路线失败已不再出现在清洁结果中；剩余两项均为 D16 A01 连续采样不足。
- 当前 [D16 A01 性能报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_a01_performance_window_report.json) 为 `SampleCount=0/100、Passed=false`；全量 Python 本轮对应 `1109/1110`，唯一失败是该报告未通过的保护性断言，不能改成 PASS。

### 交接任务板

- **已完成（限定范围）**：P0 契约/顺序冻结；P1 主 HUD 输入归属修复和静态审计；P2 的三个定向焦点/单一 HUD 检查。
- **进行中**：P2 处理清洁全量中剩余的两项 D16 采样失败；P4 D16 性能采样 harness 和报告；P3 旧灰盒坐标兼容性解释继续留痕。
- **阻塞**：D16 A01 在当前 batchmode harness 中 `0/100` 采样；完整 D16 assembly 尚未清零。旧 D19 灰盒坐标仍仅 `1/6`，但当前 R03 活动路线已完成 `6/6` 碰撞和 `6/6` NavMesh 的限定范围检查。
- **最后一步**：只有 P2/P3/P4 清零后，才能启动 RERUN48 做原生窗口/人工物理键盘和画面验收；在此之前不启动 native、不解锁 VC-G02。

## 2026-09-06 P3 当前 R03 路线证据

`D19S02ScenePlayModeTests` 已把当前 R03 活动对象与历史兼容锚点分开记录。最新定向运行 [VC_G01_D19_S02_AFTER_R03_ROUTE_R3.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_S02_AFTER_R03_ROUTE_R3.xml) 为 `1/1 PASS`，结构化报告 [D19_S02_PLAYMODE_SMOKE.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-S02/Preflight/D19_S02_PLAYMODE_SMOKE.json) 显示：活动运行时路线锚点 `6/6` 有碰撞面、`6/6` 有 NavMesh；稳定兼容锚点 `6`；旧坐标诊断 `1/6`。

这不是降低阈值或隐藏失败：测试仍输出旧坐标结果，且旧坐标兼容性不升级为 PASS。P3 当前状态为 `PASS_WITHIN_SCOPE`（R03 活动路线），剩余全量 PlayMode 与性能采样门禁仍阻止 native-last 解锁。

## 2026-09-06 P2/P4 清洁复跑结果

### 性能 probe ownership 根因

前一轮 `SampleCount=0/100` 不是把阈值写宽即可解决的问题：`D16WorldPerformanceController` 的全局 probe 查找命中了 R03 会停用的旧 R02 presentation root。现在 controller 自己确保活动 `D16RuntimePerformanceProbe`，并且 disabled legacy probe 不在销毁时覆盖报告；PlayMode 断言也检查 probe active、owner 和 host。

### 最新结果

- [probe ownership 定向 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PROBE_OWNERSHIP_R2.xml)：`1/1 PASS`；active/owned/host 正确，短运行 sample count `3`。
- [D16 A01 渲染态 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_A01_RENDERED_R5.xml) 与 [报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_a01_performance_window_report.json)：60.004s，`234/100` samples，`Passed=true`，RTX 4070 Ti/D3D11；GPU counter 无正样本，`GpuSampleAvailable=false` 保留。
- [D16InputRouter 测试类](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_INPUT_ROUTER_ALL_R2.xml)：`10/10 PASS`。A03 在 W 输入前显式取得 `Keyboard.current` 后，full class 不再受 fixture 顺序影响；日志记录 ship distance `0.0276` 和 speed `0.000 -> 0.349`。
- [完整 PlayMode snapshot](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_ALL_AFTER_INPUT_DIAGNOSTIC_R7.xml)：`70/71 PASS`、`0 FAIL`、`1 SKIP`、退出码 0。唯一跳过是明确要求独立 work scene 的 D19A01 测试。
- [PlayMode 结构化结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_PLAYMODE_RESULT.json) 已把上述 probe ownership、rendered A01、输入测试类和 full snapshot 纳入 pre-native gate；`runtime_proof=false` 仍保留。

当前判断：`VC-G01-P2/P3/P4=PASS_WITHIN_SCOPE`，但 `VC-G01-NATIVE=LOCKED`；合成 PlayMode、渲染性能和脚本 Player 证据不能替代人工物理键盘、原生窗口观察或完整视觉验收。
