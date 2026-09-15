# VC-G01 当前 Player 路线与交互回归交接

日期：2026-09-06
状态：`IN_PROGRESS`

## 当前结论

RERUN48 Player 已使用 MeshCollider 坐标修复后的最新 R03 Prefab 和当前只读路线探针构建成功，源码/Unity 回归通过；RERUN54 已取得脚本驱动真实 Win32 Player 的移动、转向、E 成功/远离拒绝、I/M/T 页面状态和 Esc 恢复证据，随后三次独立 bootstrap Player 运行又关闭了 I/M/T owner-not-ready reject 证据缺口。VC-G01 仍为 `IN_PROGRESS`：CUA 当前没有可绑定的原生 Windows 窗口，脚本 OS 事件不替代人工物理键盘验收，页面状态也不等同领域业务幂等或完整 D31 收口；VC-G02/VC-G03 不解锁。

## 2026-09-11 Unity Pipeline R7 控制面复核

新的 Unity Pipeline 运行交接已登记为 [UNITY_CLI_RUNTIME_QA_20260911_HANDOFF](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/UNITY_CLI_RUNTIME_QA_20260911_HANDOFF.md)，结果为 [UNITY_CLI_RUNTIME_QA_20260911_RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/UNITY-CLI/UNITY_CLI_RUNTIME_QA_20260911_RESULT.json)。

R7 隐藏 Windows Player 通过 Pipeline 端口 `7801` 可达；InitialHarbor 控制面使用直接 `eval` 场景加载后，得到 W/S、A/D、E 远端拒绝/登船/下船、I/M/T、临时页 Esc 关闭和普通 Esc 暂停/恢复证据。新鲜 Player 的 `simulate_pointer` 虽可达但没有完成 MainMenu→InitialHarbor 点击转场，因此菜单指针路径不登记为 PASS。复测暴露的真实缺陷是模态 owner 关闭页面后暂停控制器同帧重复消费 Esc；现在暂停消费点和 D29/D3/D10/D9/D4 owner 都有门禁/consumed marker。D29 Q15 定向 PlayMode `1/1 PASS`，build `build_6cd0c3272cb4` `totalErrors=0`，Player error console `0`。

这只把 VC-G01 的“可用控制面与交互状态”推进到 `PASS_WITHIN_SCOPE`，不改变 VC-G01 正式状态：Pipeline 的 `simulate_key`/`simulate_pointer` 是 Input System 模拟输入，不是物理键盘/鼠标；本轮没有 Player 截图，`DrawCalls=0` 不是视觉证据；动作/Avatar、游泳、舵轮接触、植被接地和漂浮/穿模仍未收口。因此 VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`，D31 保持 `PARTIAL`。

## 已完成

### 代码层前置审计（不等于 Player 通过）

- [交互逻辑审计结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-LOGIC/VC_G01_LOGIC_RESULT.json)：`PASS_WITHIN_SCOPE`；源码/静态回归 `48/48 PASS`，定向 Unity EditMode `3/3 PASS`。已收口暂停、失焦、临时页、同帧重复快捷键和 D9→D10 页面交接边界。
- [世界/碰撞逻辑审计结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-WORLD-LOGIC/VC_G01_WORLD_LOGIC_RESULT.json)：`PASS_WITHIN_SCOPE`；Python `31/31 PASS`，定向 Unity EditMode `4/4 PASS`。已收口旧层实体碰撞、R03 延迟 MeshCollider、ShoreBoundary 归属、镜头状态恢复和路线探针安全边界。
- 两组修复合入后的全量 Unity EditMode：[VC-G01_ALL_EDITMODE_R3.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_ALL_EDITMODE_R3.xml) 为 `593/593 PASS`，`failed=0`、`skipped=0`；日志：[VC-G01_ALL_EDITMODE_R3.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_ALL_EDITMODE_R3.log)。

### D27/D35 静态前置收口

- D27 R03 表现挂接最新审计为 `PASS`，`r03_attachment_count=1`，NPC/动物/宠物为 `12/8/2`；证据：[D27 审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)、[Unity 审计日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D31/D31-S05/D31_S05_UNITY_D27_AUDIT_R75.log)。
- D35 R03 材质/表现几何静态审计为 `PASS_WITHIN_SCOPE`：12 材质、11 纹理材质、1 个灰泥纯色例外、NormalMap `6/6`，D19 自有层 `391 Renderer / 1 Collider / 39208 vertices`；证据：[R75 材质审计](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R75.json)、[D35 交接](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/VISUAL-CLEAN/D35_R03_SURFACE_MATERIAL_HANDOFF.md)。
- 静态修复合入后的全量 Unity EditMode：[VC-G01_ALL_EDITMODE_R4.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_ALL_EDITMODE_R4.xml) 为 `593/593 PASS`，`failed=0`、`skipped=0`；Python 定向回归为 `98/98 PASS`。
- 自动化 PlayMode 前置：[VC-G01 PlayMode 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_PLAYMODE_RESULT.json) 为 `15/15 PASS`，覆盖 D19S02R01 路线 `1/1`、D16 输入 `10/10`、D19 交互 `2/2`、D19 路线 `2/2`。该结果使用合成输入，只能证明代码/PlayMode 前置逻辑，不能替代 standalone Player 的人工 OS 输入和视觉观察。
- D27/D35 修复及 VC-G01 前置合入后的全仓库 Python 回归为 `1085/1085 OK`，命令为 `python -m unittest discover -s tools/tests -p 'test_*.py'`。
- D35 R75 修复后已重新构建 [RERUN48 Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe)；[构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log) 成功，Unity 批处理日志为 [VC-G01_PLAYER_BUILD_R04_RERUN48.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R04_RERUN48.log)。Player Data 中的 `Assembly-CSharp.dll` 和 `resources.assets` 已在 12:36 更新，因此人工测试使用的 RERUN48 已对应 D35 R75 最新资源。

以上证据只证明代码/编辑器逻辑门槛；`VC-G01` 仍是 `IN_PROGRESS`，因为没有真实 standalone Player 输入、镜头观察或路线探针运行结果。

- [RERUN47 Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN47.exe) 已构建。
- [RERUN47 构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN47.log) 记录构建成功。
- [RERUN48 Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe) 已使用 VC-A02 修复后的 Prefab 构建。
- [RERUN48 构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log) 记录构建成功。
- [RERUN48 Unity 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R02_RERUN48.log) 无 C# 编译错误或 Build failed。
- 坐标对齐后的 Unity 6000.0.80f1 重新编译/重建日志：[VC-G01_PLAYER_BUILD_R03_COORDINATES.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R03_COORDINATES.log)，结果为 Build succeeded、无 `error CS`；仍然是非开发版 `BuildOptions.None`。
- 坐标对齐后的全量 Unity EditMode 回归：[VC-G01_EDITMODE_R02.xml](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_EDITMODE_R02.xml) 为 `586/586 PASS`，`failed=0`、`skipped=0`；日志：[VC-G01_EDITMODE_R02.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_EDITMODE_R02.log)。
- [RERUN48 启动日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_RUNTIME_R02_RERUN48.log) 记录 MainMenu、InputSystem 和 UI 初始化正常；FMOD 无音频设备属于环境警告。
- [Unity 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_BUILD_R01.log) 没有 `error CS` 或 Build failed。
- [路线探针源码回归](C:/Users/Administrator/Desktop/game/first01/tools/tests/test_d19_r03_visual_builder.py) `22/22 PASS`；定向总回归 `73/73 PASS`。
- 路线探针中的市场、仓库、船厂目标点已对齐当前 R03 builder 的正式表现坐标；这是证据探针修正，不是运行时路线通过。
- 启动观察记录了 MainMenu UI/InputSystem 初始化；该观察不是进入港口后的路线通过。

## 待执行的真实输入矩阵

1. 启动 Player 并进入 `InitialHarbor`，确认首屏非空白、角色/海面/建筑可见。
2. 按住 `W`、`A`、`S`、`D` 分别移动，检查位置变化、角色不左右摇摆、松键停止、模态未打开时输入不穿透。
3. 在码头接近船体，按 `E` 登船；记录 `control_mode` 是否变为 `Ship`、反馈是否为 `boarded`、角色是否与船体重叠。
4. 再按 `E` 下船；记录是否回到 `Character`、落点是否在可走面、没有穿模或卡死。
5. 到仓库按 `I`，到市场按 `M`，到酒馆按 `T`；分别记录 owner-ready 成功、owner-not-ready 拒绝、重复按键防重和 `Esc`/同键关闭恢复。
6. 按 `F5` 或 `C`，只记录保存输入和 Player 日志；不得把“输入边沿被看到”当成保存/重载业务成功。
7. 关闭 Player 后读取 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json`、Player 日志和截图，生成最终 `VC_G01_RESULT.json`；若任一项缺证据，状态保持 `PARTIAL` 或 `BLOCKED`。

## 证据位置

- 预期路线探针：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json`
- 运行日志：建议保存为 `game/tools/results/VISUAL-CLEAN/VC-G01_PLAYER_RUNTIME_R02.log`
- 结果文件：[VC-G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)

## 边界

- 探针只观察 Transform、Controller、Router 和真实输入边沿，不直接调用登船、市场或保存方法；业务事实仍由原 owner 裁决。
- 没有真实输入证据时，禁止升级 VC-G01、D29/D30/D31 或 D25—D31 正式状态。
- VC-G01 通过后才进入 VC-G02；VC-G02 仍必须单独验证 14 个固定镜头和 3 个天气变体。

## 2026-09-06 R87 正式回归登记

本次只做结果留痕和回归，没有继续修改视觉坐标。R87 已提供真实 standalone Player 的 `14/14` 固定镜头、`3/3` Calm/Rain/Storm 天气镜头，全部为 `1920x1080` backbuffer；抽查关键镜头未观察到此前目标中的巨大脱离主体绿色前景卡片。该证据属于视觉采集，不属于 VC-G01 的输入路线证明。

### 当前回归

- 全仓库 Python：`1087/1087 PASS`，命令为 `python -m unittest discover -s tools/tests -p 'test_*.py'`。
- Unity 6000.0.80f1 全量 EditMode：`593/593 PASS`，failed/inconclusive/skipped 均为 0，退出码 0。
- [有效 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R5_20260906.xml)；[有效日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R5_20260906.log)。
- [VC-G01 结构化结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json) 已登记 R87、全量 Python 和全量 Unity 回归。

### 门禁结论

`VC-G01` 仍为 `IN_PROGRESS`，`D31` 仍为 `PARTIAL`。本次没有真实 WASD、转向、登船、下船、仓库/市场/酒馆成功/失败/重复/关闭恢复矩阵，所以不能升级 VC-G01，也不能解锁 VC-G02/VC-G03。低模/程序化整体画面质量、性能 1% low 和 D30 实机异常恢复也仍未关闭。

## 2026-09-06 接管审计与启动烟测

### 接管任务板

- **已完成**：只读检查当前分支/HEAD/最近提交；确认现有工作区改动不清理；复读本任务要求的规范、路线图、视觉清理计划、VC-G01 handoff 与结果文件；保留既有 `593/593 Unity EditMode`、`1087/1087 Python`、R87 `14/14 + 3/3` 证据。
- **已完成**：新增 `tools/vc_g01_runtime_startup_smoke.ps1` 及 5 项静态测试，用于已知 RERUN48 的有界启动/优雅退出/探针与 Player.log 新鲜度检查；没有发送合成输入，也没有改场景或视觉坐标。
- **已完成**：新增工具后的全仓库 Python 回归为 `1092/1092 PASS`（失败/跳过 0，退出码 0）；其中新增启动烟测静态测试为 `5/5 PASS`。R87 既有 `1087/1087` 记录作为历史证据保留。
- **进行中**：VC-G01 的 standalone Player 真实输入路线：WASD、转向、E 登船/下船、I/M/T 入口，以及成功、拒绝、重复、Esc 关闭恢复矩阵。
- **阻塞**：当前 CUA `getState()` 仅返回 `apps=[]` 与 Codex In-app Browser，未提供原生 Windows 窗口绑定、截图或键盘输入接口。命令行启动可以证明进程存活和启动日志，不能替代窗口内真实操作。
- **下一步**：由具备原生 Windows 窗口控制能力的环境或用户手动启动 RERUN48，进入 `InitialHarbor` 完成矩阵；关闭后归档探针、Player.log、每步截图和结果。VC-G01 真实通过前保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`。

### 本轮启动证据

- 可复现工具：[VC-G01 启动烟测脚本](C:/Users/Administrator/Desktop/game/first01/tools/vc_g01_runtime_startup_smoke.ps1)。
- 机器摘要：[VC_G01_STARTUP_SMOKE_20260906.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_20260906.json)。本轮 PID `26552`，等待 `15s` 后 `CloseMainWindow=True`、`graceful_exit=True`、`exit_code=0`。
- Player 日志：[WhalesIslandReef Player.log](C:/Users/Administrator/AppData/LocalLow/Whalebone%20Studio/WhalesIslandReef/Player.log)，记录 `MainMenu` UI/InputSystem 初始化和 route probe 安装。
- 探针：[D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json)。本次刷新结果为 `scene=MainMenu`、`player_found=false`、`observations=0`、`events=1`、`runtime_route_proof=false`。
- 当前 Python 回归命令为 `python -m unittest discover -s tools/tests -p 'test_*.py'`，结果 `1092/1092 PASS`；机器摘要和结果 JSON 已同步登记。

该启动烟测关闭了“Player 是否能启动/退出”的环境不确定性，但没有关闭 VC-G01 的真实输入门禁。不得把本轮 `OBSERVED_ONLY`、R87 固定镜头或自动化 PlayMode 合成输入升级为 `PASS`。

## 2026-09-06 路线探针输入覆盖与 R8 验证

### 本轮任务板

- **已完成**：在只读 `D19R03PlayerRouteProbe` 中补齐 `I` 仓库、`M` 市场、`Escape` 关闭的按下边沿和保持态事件；保留 W/A/S/D、E/B/T/V 字段，不调用业务方法、不写领域状态。
- **已完成**：新增 [VC-G01 路线探针静态测试](C:/Users/Administrator/Desktop/game/first01/tools/tests/test_vc_g01_runtime_probe.py)，与世界逻辑定向回归合计 `36/36 PASS`；全仓库 Python 回归为 `1095/1095 PASS`。
- **已完成**：Unity 6000.0.80f1 全量 EditMode R8 为 `593/593 PASS`，失败/不确定/跳过均为 0；本轮未出现 `D19R03PlayerRouteProbe` 的 `CS0618`，也未出现 `error CS` 或 `Build failed`。
- **已完成**：RERUN48 已按当前探针源码重新构建；独立构建摘要报告 `BuildOptions.None` 成功，Player Data 中 `Assembly-CSharp.dll` 在 `03:07` 刷新。
- **进行中**：standalone Player 真实输入路线仍缺：WASD、转向、E 登船/下船、I/M/T 成功/拒绝/重复/关闭恢复矩阵。
- **阻塞**：CUA 实际仍为 `apps=[]`，只有 Codex In-app Browser，没有原生 Windows Player 窗口绑定、键盘输入或截图接口。
- **下一步**：由用户手动或具备原生窗口控制能力的环境启动最新 RERUN48，进入 `InitialHarbor`，完成矩阵并归档探针、Player.log、截图和结果；VC-G01 通过前保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`。

### 本轮证据

- [VC-G01 R8 EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R8_20260906.xml) / [日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R8_20260906.log)：`593/593 PASS`。
- [RERUN48 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R05_RERUN48_20260906.log) / [独立构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log)：构建成功；`Assembly-CSharp.dll` 已刷新。
- [最新启动烟测摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_20260906.json)：PID `30108`，`CloseMainWindow=True`、`graceful_exit=True`、`exit_code=0`；探针仍为 `MainMenu`、`player_found=false`、`observations=0`、`events=1`、`runtime_route_proof=false`。
- [结构化 VC-G01 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json) 已登记上述证据，但正式状态未升级。

新增的探针字段与静态回归只证明“未来真实 Player 输入可被观察和分类”，不证明输入已经发生，也不证明业务成功、拒绝、重复或恢复路径已经通过。

## 2026-09-06 原生 Win32 Player 输入探测与当前门禁

本轮继续执行了 VC-G01，而不是停留在启动烟测。CUA 当前仍返回 `apps=[]`，没有可绑定的原生 Windows 窗口控制面；因此使用项目内可复现的 `tools/vc_g01_runtime_input_probe.ps1`，由脚本启动已知 RERUN48 Player，再通过 `user32` 的 `SetForegroundWindow`、`SendInput` 及 legacy fallback 发送真实 OS 键鼠事件。脚本没有调用游戏业务方法，路线探针仍为只读。

### 已取得的真实 Player 证据

- [运行结果 JSON](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)：最新 PID `26980`，窗口客户区 `1920x1080`，进入 `InitialHarbor`，42/42 个输入/窗口/截图步骤成功，另有 16/16 张截图成功保存，Player 正常关闭。
- [路线探针](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json)：`player_found=true`、`observations=91`、`runtime_route_proof=true`；事件中包含位移校准、交互边沿、仓库/市场/关闭输入和地标观测。
- [Player.log](C:/Users/Administrator/AppData/LocalLow/Whalebone%20Studio/WhalesIslandReef/Player.log) 及[工作区归档副本](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN49.log)：确认 `boarded`、`disembarked`、仓库打开/关闭、市场打开、酒馆打开/关闭，以及暂停/恢复日志；`Esc` 临时页优先关闭后，下一次 `Esc` 能暂停/恢复。
- [截图目录](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/)：包含 InitialHarbor、仓库、市场、酒馆、暂停和恢复状态的窗口截图；例如 `20260906_051416_route_initial_harbor.png`、`20260906_051416_warehouse_open_i.png`、`20260906_051416_market_open_m.png`、`20260906_051416_tavern_open_t.png`、`20260906_051416_pause_open_escape.png`、`20260906_051416_pause_recover_escape.png`。

### 仍未关闭的门禁

- W/A/S/D 和 A/D 转向事件确实发送，探针也看到部分 key 边沿，但 `moving_observations=0`、`max_speed=0`；不能把 spawn/船体校准位移当成移动 PASS，也不能据此证明转向。
- E 当前是在船体附近完成登船/下船成功切换，没有独立的“远离船体按 E 后拒绝”证据。
- I/M/T 已有打开、重复输入和 Esc 关闭/恢复的部分日志与截图，但尚未形成逐项、可审计的 owner-not-ready reject 与 duplicate matrix。

本轮结果已写入 [VC-G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。D29 M/T 快捷键、D7/HarborPause legacy Esc 兼容修复及只读诊断之后，Unity R12 为 `593/593 PASS`，全仓库 Python 为 `1105/1105 PASS`；这些是代码和回归证据，不替代上述未完成的 Player 移动/拒绝矩阵。因此正式状态保持：`VC-G01=IN_PROGRESS`、`D31-P04/D31-S05/D31-G06=PARTIAL`、`VC-G02=LOCKED`、`VC-G03=LOCKED`。解除 VC-G01 门禁仍需要物理键盘或可接管 Unity Input System 的控制面补齐移动、转向和远离船体拒绝路径。

## 2026-09-06 Unity Input System 边界诊断补充

为区分“输入权限/移动逻辑问题”和“注入路径未进入 Unity Input System”，在只读路线探针中增加了 D16 轴、应用焦点、Gameplay/Primary 输入权限、移动输入来源和 legacy fallback 状态观测；没有修改 D2 移动数值或 fallback 选择规则。

- 新构建 [RERUN48 Player](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_Player_RERUN48.exe) 使用 [R09 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R09_RERUN48_20260906.log)；`BuildOptions.None`，构建成功。
- [R12 Unity EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R12_20260906.xml) 为 `593/593 PASS`，failed/inconclusive/skipped 均为 0；日志无 `error CS` 或 `Build failed`，保留项目既有 CS0618 弃用警告。
- 最新真实 Win32 探测 PID `26980`，探针 `91` 个观测，`InitialHarbor`、`player_found=true`、`runtime_route_proof=true`；Player.log、入口模态和暂停恢复结果与上一轮一致。
- 诊断关键值：在 `key_w=true` 的观测中，`Application.isFocused=true`、`GameplayInputAllowed=true`、`PrimaryInputAllowed=true`，但 `D16InputRouter.MoveAxis()=(0,0)`，`MovementInputSource=D16InputRouter`，`LegacyMovementFallbackActive=false`，`moving=false`、`speed=0`。同时 HKCU PlayerPrefs 没有 `wb_a11y_binding_*` 项，D16 仍使用源码默认 W/A/S/D；因此当前证据定位到 Win32 注入没有进入 Unity Input System，而不是重绑定或模态权限阻断，仍不能把移动标记 PASS。
- 新增诊断测试后全仓库 Python 回归为 `1105/1105 PASS`。结构化结果已更新：[VC-G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。

结论不变且更精确：`VC-G01=IN_PROGRESS`。下一次必须使用物理键盘或能驱动 Unity Input System 的原生 Player 控制面补齐 WASD/转向 moving 与速度，并在远离船体位置补 E reject；不得因为本次 D16 轴为零就改变 D2 的 legacy fallback 规则。`VC-G02/VC-G03` 继续锁定。

## 2026-09-06 Unity Input System 键盘设备存在性复核

本轮在已有只读 D16 轴/权限诊断上增加了 `Keyboard.current` 与 W/A/S/D 控件状态观测，目的是区分设备缺失、权限阻断和 OS 注入路径不兼容。探针仍不调用游戏业务方法、不写存档或领域状态。

- RERUN48 使用 [R10 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R10_INPUT_DIAGNOSTIC_20260906.log) 构建成功，`BuildOptions.None`。
- 最新运行摘要 [VC_G01_RUNTIME_INPUT_20260906.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)：PID `21556`，进入 `InitialHarbor`，42/42 步骤、16/16 截图成功，Player 正常关闭。
- [路线探针结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json) 为 `89` 个观测，`Keyboard.current` 在 `89/89` 个样本存在；Input System 的 W/A/S/D 状态分别为 `0/0/0/0` 个按下样本，而 legacy `Input.GetKey` 仍观察到 W/A/S/D（`7/3/5/3` 个样本）。同时应用焦点、Gameplay/Primary 权限均为 true，D16 轴为 `(0,0)`，移动来源为 `D16InputRouter`，legacy fallback 为 false。
- Player.log 已归档为 [RERUN50](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN50.log)。
- 该证据把当前阻塞精确定位为“现有 user32 OS 注入未更新 Unity Input System 键盘状态”，不是设备缺失、重绑定或权限门禁问题；不因此修改 D2 fallback 选择规则，也不把移动升级为 PASS。

任务板保持：

- **已完成**：Player 启动、InitialHarbor 路由、E 近船成功登/下船、I/M/T 入口、Esc 关闭与暂停恢复，以及本轮键盘设备/输入路径诊断留痕。
- **进行中**：WASD 移动、A/D 转向、非零速度、远离船体 E reject，以及完整 success/reject/duplicate/close-recovery 矩阵。
- **阻塞**：CUA 无原生 app surface；当前 SendInput/legacy fallback 不进入 Unity Input System，无法形成移动/转向真实通过证据。
- **下一步**：使用物理键盘或能驱动 Unity Input System 的原生控制面补测；在此之前保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

## 2026-09-06 R13 全量回归与当前交接点

- 新增键盘设备状态字段后，Unity 6000.0.80f1 EditMode R13 为 `593/593 PASS`，failed/inconclusive/skipped 均为 0，退出码 0；证据为 [R13 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R13_20260906.xml) 与 [R13 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R13_20260906.log)。
- 本轮 Python 全量回归为 `1105/1105 OK`，耗时 `220.732s`，退出码 0；该结果验证了探针、结果校验和既有工具回归。
- 当前唯一未关闭的真实运行门禁仍是移动/转向：键盘设备存在，但 Win32 注入没有更新 Unity Input System 控件，因而没有 moving/速度样本；远离船体 E reject 及完整逐项矩阵也仍缺。
- 交接结论：`VC-G01=IN_PROGRESS`、`D31-P04/D31-S05/D31-G06=PARTIAL`、`VC-G02/VC-G03=LOCKED`。需要物理键盘或可驱动 Unity Input System 的原生控制面继续执行，不应再用同一 user32 注入路径重复宣称移动通过。

## 2026-09-06 原生控制面能力探查与最终当前边界

为判断宿主机是否已有可复用的原生输入通道，执行了只读 PnP、G HUB 安装清单/二进制能力和 CUA surface 探查；没有安装/卸载驱动、改设备配置或发送新的游戏输入。

- [主机能力探查结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_NATIVE_CONTROL_SURFACE_DISCOVERY_20260906.json)：Logitech G HUB Virtual Keyboard 为 `HIDClass/OK`，Rapoo Bluetooth keyboard 为 `Bluetooth/OK`，Xbox Wireless Controller 为 `Bluetooth/OK`；`logi_joy_vir_hid` 服务与 virtual HID driver manager 均存在。
- 已安装 G HUB virtual HID manifest 只定义 `--install`/`--uninstall`，二进制帮助字符串另包含 `--status`、`--console`、`--logfile`、`--path`；未发现可安全调用的用户态按键事件命令。该探查不构成输入事件证据。
- CUA 当前仍没有原生 app surface；已有的 user32 `SendInput`/legacy fallback 仍只进入 legacy `Input.GetKey`，没有更新 Unity Input System W/A/S/D 状态。不能把 G HUB 虚拟设备或软件合成事件当作 VC-G01 真实移动 PASS。

当前任务板保持：

- **已完成**：宿主机设备能力边界留痕、R10/R13 编译回归、真实 Player 启动/InitialHarbor/E 登下船/I-M-T 入口/Esc 恢复的限定证据。
- **进行中**：WASD/转向的 Unity Input System moving/速度观测、远离船体 E reject，以及逐项 success/reject/duplicate/close-recovery 矩阵。
- **阻塞**：本 Agent 没有物理键盘控制面或能更新 Unity Input System 的原生 Player 控制能力；现有 G HUB 管理器没有安全的用户态按键事件接口。
- **下一步**：由用户或具备原生窗口/物理输入能力的验收环境操作最新 RERUN48，补齐移动、转向、远离船体 E reject 和完整矩阵；完成前保持 `VC-G01=IN_PROGRESS`，不解锁 `VC-G02/VC-G03`。

## 2026-09-06 RERUN53 真实输入闭环与最新交接点

本节 supersede 上述“旧版 SendInput 未进入 Input System”的运行结论；旧结论对应 32-byte Win32 `INPUT` 结构体的注入器。RERUN53 使用修正后的 x64 union 对齐和当前 R14 Player，仍然只通过真实 OS 事件驱动游戏，路线探针保持只读。

- [R14 Unity EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R14_20260906.xml) 与[日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R14_20260906.log)：`593/593 PASS`，failed/inconclusive/skipped 均为 0；[R14 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R14_TURNING_20260906.log) 报告非开发版 `BuildOptions.None` 构建成功。
- [RERUN53 运行摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)：`2026-09-06 12:13`，PID `31568`，进入 `InitialHarbor`；44/44 输入步骤、17/17 窗口截图成功，Player 优雅退出；Player.log 已归档为 [RERUN53](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN53.log)。
- [路线探针](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json)：104 个观测、31 个移动观测、最大速度 `3.200000047683716`；`Keyboard.current` 存在于 `104/104` 样本，Input System W/A/S/D 按下样本为 `16/3/5/4`，D16 轴非零样本 `28`；`turning_observed=true`，转向事件 `7` 个，最大 `yaw_delta_from_start=180°`。
- E 路径已取得成功和拒绝两类证据：近船 E 产生 `boarded`，再次 E 产生 `disembarked`；W 持续移动至约 `z=12.729` 后，E 产生 `ship_too_far`（探针 `73` 个样本），人物保持 `Character` 模式，窗口截图 [reject_e_far_from_ship](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/20260906_121312_reject_e_far_from_ship.png) 可见“请先靠近码头上的船，再按 E 上船”。
- I/M/T 与 Esc：Player.log 标记 `warehouse_opened/market_opened/tavern_opened/pause_opened/pause_resumed` 均为 true；仓库、市场、暂停和拒绝状态的窗口截图可见，重复 I/M/T 键步骤也已发送并留痕。该证据证明入口和恢复路径在当前运行中响应，但不自动等价于业务幂等或 owner-not-ready reject 通过。
- x64 注入修复：`tools/vc_g01_runtime_input_probe.ps1` 的 `NativeInput` union 已从错误的 `32 bytes` 对齐修正为 `40 bytes`；RERUN53 的 `21/21` 键盘步骤均报告 `send_input_accepted=true`，不再沿用 RERUN51 的零轴诊断。
- 本轮在上述变更后执行全仓库 Python unittest：`1107/1107 PASS`，失败/跳过均为 0，退出码 0；此前 `1105/1105` 作为历史回归记录保留。

### 当前任务板

- **已完成（限定范围）**：standalone Player 启动、InitialHarbor、D16 驱动的 W/A/S/D 移动、A/D 转向、E 登船/下船成功、远离船体 `ship_too_far` reject、I/M/T 入口、Esc 页面关闭和暂停恢复；证据已写入运行 JSON、只读探针、Player.log 归档和截图。
- **进行中**：I/M/T 的逐项 owner-not-ready reject、重复操作结果/幂等性、每个页面的 close/recovery 结果矩阵；需要把快捷键边沿和“业务结果”分开审计。
- **阻塞/边界**：CUA `getState()` 仍返回 `apps=[]`，没有可绑定的原生 Windows app surface；RERUN53 是脚本驱动的真实 Win32 OS 事件证据，不替代人工物理键盘/手柄验收。D31-P04、D31-S05、D31-G06 继续 `PARTIAL`。
- **下一步**：在不修改 D2 移动 fallback 规则的前提下，补齐 I/M/T 矩阵；矩阵完成前保持 `VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`，不得提前推进后续视觉任务。

## 2026-09-06 RERUN54 页面状态矩阵与最新交接点

本轮在 R15 探针构建基础上重新执行完整 standalone Player 序列。页面状态由只读 `D19R03PlayerRouteProbe` 直接读取 D3/D10 owner 状态与 D29 `WarehousePanel` 可见性；输入边沿事件使用独立记录，不把短间隔重复按键合并。没有调用业务方法、直接写入领域状态或用截图猜测页面状态。

- [R15 Unity EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R15_20260906.xml) / [日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R15_20260906.log)：`593/593 PASS`，failed/inconclusive/skipped 均为 0；日志无 `error CS` 或 `Build failed`。
- [R15 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R15_INTERACTION_MATRIX_20260906.log) 与 [独立构建摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN48.log)：RERUN48 非开发版、`BuildOptions.None`，构建成功。
- [RERUN54 运行摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_20260906.json)：PID `25260`，`InitialHarbor`，44/44 输入步骤、17/17 窗口截图成功，Player 优雅退出；[RERUN54 Player.log 归档](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_20260906_RERUN54.log)。
- [RERUN54 只读路线探针](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json)：104 个观测；移动 29 个样本、最大速度 `3.200000047683716`；页面状态事件包含仓库/市场/酒馆的 open/close 转移；`Keyboard.current` 存在于全部样本，D16 轴与转向证据继续成立。

### 页面交互矩阵

| 页面 | 真实输入边沿 | 状态转移 | 当前证据 |
|---|---:|---:|---|
| 仓库 I | `3` | open `2` / close `2` | 重复 I 的 toggle 与 Esc close/recovery 可审计；面板找到 `104/104` 样本 |
| 市场 M | `2` | open `1` / close `1` | 重复 M 保持同一市场页，随后 Esc 关闭；可见样本 `7` |
| 酒馆 T | `2` | open `1` / close `1` | 重复 T 保持同一酒馆页，随后 Esc 关闭；可见样本 `7` |

结构化矩阵字段为 `interaction_matrix.matrix_proof=true`、`final_temporary_pages_closed=true`；仓库/市场/酒馆的 `duplicate_*_proof` 和 `escape_close_recovery_proof` 均为 true。关键窗口截图为 [仓库](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/20260906_124737_warehouse_open_i.png)、[市场](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/20260906_124737_market_open_m.png)、[酒馆](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/20260906_124737_tavern_open_t.png)、[暂停](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_20260906/20260906_124737_pause_open_escape.png)。

### 当前任务板

- **已完成（限定范围）**：RERUN54 真实 Win32 OS 输入下的 Player 启动、InitialHarbor、W/A/S/D 移动、A/D 转向、E 登船/下船成功、远离船体 `ship_too_far` reject，以及 I/M/T 页面打开、短间隔重复输入、Esc close/recovery 状态矩阵；脚本输入 21 个键盘步骤均 `send_input_accepted=true`。
- **进行中**：I/M/T 的 owner-not-ready reject 尚未在真实 bootstrap 窗口发起并取得证据；页面重复状态已证明，但不把它扩大解释为领域业务幂等性。
- **阻塞/边界**：CUA `getState()` 仍返回 `apps=[]`，所以 RERUN54 是脚本驱动的真实 Win32 standalone 证据，不替代人工物理键盘/手柄验收；D31-P04、D31-S05、D31-G06 继续 `PARTIAL`。
- **下一步**：在不修改 D2 fallback 规则、不给 owner 注入状态的前提下，若需要关闭剩余 reject 缺口，应执行有界 owner bootstrap 窗口测试或由具备原生 Player 控制面的验收环境操作；在此之前保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

RERUN54 之后全仓库 Python unittest 为 `1107/1107 PASS`，失败/跳过均为 0；最新结构化结果为 [VC_G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。

## 2026-09-06 R17 bootstrap reject 矩阵与最新状态

本轮没有升级 VC-G01 的正式状态。针对 RERUN54 尚未覆盖的 owner-not-ready 窗口，使用最新非开发版 Player 各自独立启动三次，并在确认新游戏后的 bootstrap 窗口发送目标快捷键；运行时结果由只读 `D19R03PlayerRouteProbe`、带 run marker 的 Player.log 和输入步骤结果共同裁决。

- [R17 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_R17_BOOTSTRAP_TARGETS_20260906.log)：Unity 6000.0.80f1、`BuildOptions.None`、`Build Successful`，MainMenu -> `InitialHarbor` 路由成立；三次运行均优雅退出。
- [I bootstrap 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_BOOTSTRAP_REJECT_I_20260906.json)、[M bootstrap 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_BOOTSTRAP_REJECT_M_20260906.json)、[T bootstrap 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_BOOTSTRAP_REJECT_T_20260906.json)：每个目标均 `player_started=true`、`send_input_accepted=true`、`unready_owner_edge_count=1`、run marker 后 reject 日志存在、目标页面打开转移为 `0`、`target_proof=true`。
- 三个只读探针分别为 [I](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_I_20260906.json)、[M](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_M_20260906.json)、[T](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_BOOTSTRAP_REJECT_T_20260906.json)；对应 Player.log 也已分别归档。
- [I/M/T 聚合结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_BOOTSTRAP_REJECT_MATRIX_20260906.json) 为 `PASS_WITHIN_SCOPE`，`matrix_proof=true`。该结果只关闭 owner-not-ready reject 证据缺口，不等同人工物理键盘验收、领域业务幂等或完整 D31 收口。
- [R16 Unity EditMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R16_BOOTSTRAP_20260906.xml) / [日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_R16_BOOTSTRAP_20260906.log)：`593/593 PASS`，failed/inconclusive/skipped 均为 0；本轮全仓库 Python 回归为 `1110/1110 PASS`。

### 当前任务板

- **已完成（限定范围）**：RERUN54 完整 Player 路线/移动/转向/E 成功与远离拒绝/I-M-T 重复和 Esc close/recovery 矩阵；R17 后 I/M/T 三个独立 bootstrap owner-not-ready reject 目标证据与聚合矩阵。
- **进行中**：VC-G01 剩余人工物理输入/原生 Player 控制面验收边界，以及页面状态之外的领域业务幂等性证明；不能用脚本重复按键或页面可见状态替代。
- **阻塞/边界**：CUA `getState()` 仍为 `apps=[]`，没有可绑定的原生 Windows Player surface；D31-P04、D31-S05、D31-G06 继续 `PARTIAL`，`VC-G02/VC-G03` 继续 `LOCKED`。
- **下一步**：获得可操作的原生 Windows Player/物理输入验收环境后，按现有矩阵补做人工路线证据；在此之前保持 `VC-G01=IN_PROGRESS`，不解锁 VC-G02/VC-G03，也不修改 D2 fallback 规则。

## 2026-09-06 原生控制面阻塞复核

本轮通过 CUA `getState()` 进行实时复核：仍只有 1 个 Codex In-app Browser、0 个原生 app、0 个浏览器 tab，未出现可绑定的 Windows Unity Player surface。该结果与此前 `apps=[]` 证据一致。

因此当前剩余门禁不是代码或测试未执行，而是缺少能完成人工物理键盘/原生 Player 观察的外部控制条件。R17 的 I/M/T owner-not-ready 矩阵、RERUN54 的完整脚本 OS 输入矩阵、R16 `593/593` Unity 回归和 `1110/1110` Python 回归均保留；VC-G01 正式状态继续为 `IN_PROGRESS`，VC-G02/VC-G03 不解锁。

解除条件：提供可绑定原生 Windows Player 的控制面，或由用户在本机物理操作后交付逐步截图、Player.log 和路线探针结果；否则不再重复相同脚本运行、不将 `SendInput` 解释为人工验收、不升级 PASS。

## 2026-09-06 native-last 前置门禁重排与输入冲突修复

本节是当前交接点，覆盖前述“下一步立即做原生 Player”的顺序。原生 Player 现在被明确放到 `VC-G01-P0`—`P4` 之后；这不会撤销 RERUN53/RERUN54/R17 的历史证据，也不会把脚本驱动的真实 Win32 事件降格为无效，只是禁止它们替代最后的人工物理输入和画面观察。

### 已完成

- **P0 计划冻结**：执行计划已改为 `P0 → P1 → P2 → P3 → P4 → NATIVE → VC-G02 → VC-G03`，并登记 native-last 解锁条件。
- **P1 输入归属修复**：D29 主 HUD 移除 modal focus scope；仓库面板保留本地 modal scope；`HarborInteractionStabilizer` 根据 gameplay/modal 状态控制 `EventSystem.sendNavigationEvents`，清除 gameplay 状态残留 selection。
- **P1 静态契约同步**：D31 交互审计和 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 现在区分 non-modal D29 主 HUD 与 D29-Warehouse/D4/D9/D10 modal surfaces。
- **P2 定向证据**：W/S 不导航主 HUD `1/1 PASS`；D16 A03 诊断关闭 `1/1 PASS`；D19 单一主 HUD `1/1 PASS`；相关 Python 静态测试 `34/34 PASS`。
- **回归**：最新 Unity 全量 EditMode `593/593 PASS`；不存在因为本轮 UI 输入策略而产生的 EditMode 回归。

### 当前未完成/阻塞

- **P2 全量 PlayMode**：修正后的清洁快照为 `68/71 PASS、2 FAIL、1 SKIP`。D19 HUD、D16 A03 Close 定位和 D19S02 当前 R03 活动路线失败已消失；剩余两项均为 D16 A01 连续采样不足，另有既有 D19A01 work-scene 条件跳过。
- **P3 场景/碰撞**：当前 R03 活动路线已通过真实运行时对象查询，`6/6` 锚点命中碰撞面、`6/6` 命中 NavMesh；历史 D19 灰盒旧坐标仍单独记录为 `1/6` 兼容性诊断，不宣称旧坐标契约通过。
- **P4 性能**：当前 [D16 A01 报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_a01_performance_window_report.json) 为 NVIDIA RTX 4070 Ti/D3D11，但 `SampleCount=0/100、Passed=false`；全量 Python 本轮为 `1109/1110`，唯一失败正是保护该失败报告不被伪造为 PASS 的测试。
- **NATIVE**：CUA `getState()` 仍为 `apps=[]`，而且即使控制面可用，也必须等 P2/P3/P4 清零。当前 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。

### 当前任务板

| 阶段 | 状态 | 下一动作 |
|---|---|---|
| VC-G01-P0 | PASS_WITHIN_SCOPE | 保持计划与结果 JSON 同步 |
| VC-G01-P1 | PASS_WITHIN_SCOPE | 保持主 HUD non-modal、仓库/临时页 modal 边界 |
| VC-G01-P2 | PASS_WITHIN_SCOPE | 71-case full PlayMode 为 70 PASS、0 FAIL、1 个明确 work-scene SKIP；D16 输入类 10/10 |
| VC-G01-P3 | PASS_WITHIN_SCOPE | 当前 R03 活动路线 6/6 碰撞、6/6 NavMesh；保留旧坐标 1/6 兼容性诊断 |
| VC-G01-P4 | PASS_WITHIN_SCOPE | 渲染态 60s 取得 234/100 真实样本；RERUN49 构建/启动已核验 |
| VC-G01-NATIVE | LOCKED | P1-P4 清零后才执行原生 Player + 人工视觉矩阵 |

主结果：[VC_G01_RESULT.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)；前置聚合：[VC_G01_PRE_NATIVE_GATE_20260906.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PRE_NATIVE_GATE_20260906.json)。

## 2026-09-06 P3 R03 活动路线坐标框架复核

P3 不再把历史 D19 远距灰盒坐标和当前 R03 表现/物理坐标混成一条路线。`D19S02ScenePlayModeTests` 现在同时输出稳定兼容锚点、旧坐标诊断和当前运行时活动对象：玩家、市场、仓库、酒馆、船厂、主码头。

- [定向 PlayMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_S02_AFTER_R03_ROUTE_R3.xml)：`1/1 PASS`。
- [定向 PlayMode 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D19_S02_AFTER_R03_ROUTE_R3.log) 和 [结构化烟测](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-S02/Preflight/D19_S02_PLAYMODE_SMOKE.json)：当前活动路线碰撞 `6/6`、NavMesh `6/6`；稳定锚点数 `6`；旧坐标碰撞诊断 `1/6`。
- [测试实现](C:/Users/Administrator/Desktop/game/first01/game/Assets/Tests/PlayMode/D19S02ScenePlayModeTests.cs) 保留两套结果字段，未降低碰撞阈值；[R03 路线探针](C:/Users/Administrator/Desktop/game/first01/game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs) 使用当前居中 R03 路线坐标。

结论：`VC-G01-P3=PASS_WITHIN_SCOPE` 仅针对当前 R03 活动路线。旧 D19 坐标兼容性/迁移仍有明确证据缺口，不能升级为完整旧坐标契约 PASS；它不再阻塞当前 R03 活动物理前置，但在后续契约清理中必须继续保留和解释。

## 2026-09-06 P2 清洁全量 PlayMode 复跑

- [Unity PlayMode XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_AFTER_R03_ROUTE_R4.xml) 实际结果为 `68/71 PASS、2 FAIL、1 SKIP`，退出码 `2`。
- [Unity PlayMode 日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_AFTER_R03_ROUTE_R4.log) 显示剩余失败仅为 `D16A01RuntimePerformanceWindowSamplesContinuously` 与 `InitialHarborBootstrapsD16AndSamplesRuntime` 的采样不足；D19 HUD 和 D19S02 当前 R03 活动路线不再失败。
- 这次结果已归档到 pre-native gate 与主结果；P2 仍为 `IN_PROGRESS`，因为性能采样属于前置门禁，不能用定向测试替代全量清零。

## 2026-09-06 P2/P4 前置门禁清零与独立 RERUN49 Player

本轮按新的 native-last 顺序继续执行，先清零 P2/P4 的自动化失败，再生成不覆盖历史 RERUN48 证据的 RERUN49 Player。

### 根因修复

- 旧 D16 性能 probe 由全局查找得到，实际命中会被 R03 关闭的旧 R02 presentation root；活动 `D16WorldPerformanceController` 因而没有活动 probe，造成 `SampleCount=0/100`。
- `D16WorldPerformanceController` 现在自己确保 canonical `D16RuntimePerformanceProbe`，并记录 active/owner/host；disabled legacy probe 的 `OnDestroy` 不再写零样本报告覆盖活动结果。
- `D16RuntimeAcceptanceTests` 增加 probe ownership 断言和活动 probe helper；同一测试类的 D16 A03 在发送 W 前显式 `Keyboard.current.MakeCurrent()`，消除 fixture 顺序带来的测试设备 ownership 污染。

### 证据

- [D16 probe ownership PlayMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PROBE_OWNERSHIP_R2.xml)：`1/1 PASS`；运行报告的 `PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、sample count `3`。
- [D16 A01 渲染态报告](C:/Users/Administrator/Desktop/game/first01/tools/results/d16_a01_performance_window_report.json) 与 [XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_A01_RENDERED_R5.xml)：60.004s、`234/100` 样本、`Passed=true`，设备为 RTX 4070 Ti/D3D11；`GpuSampleAvailable=false` 被原样保留，没有伪造 GPU 值。
- [D16 输入测试类](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_INPUT_ROUTER_ALL_R2.xml)：`10/10 PASS`；日志记录 W/S `MoveAxis=(0,1)`、ship distance `0.0276`、速度 `0.000 -> 0.349`。
- [71-case PlayMode 快照](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01-PLAYMODE/VC_G01_D16_PLAYMODE_ALL_AFTER_INPUT_DIAGNOSTIC_R7.xml)：`70 PASS / 0 FAIL / 1 SKIP`，退出码 0；唯一跳过是明确需要独立 work scene 的 D19A01 条件。
- [全量 EditMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_ALL_EDITMODE_AFTER_RERUN49_20260906.xml)：`593/593 PASS`，failed/inconclusive/skipped 均为 0。
- 全仓库 Python `python -m unittest discover -s tools/tests -p 'test_*.py' -q`：`1110/1110 PASS`。此前 `pytest` 不可用是环境 runner 差异，未被当作测试结果。
- [RERUN49 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_BUILD_AFTER_PROBE_OWNERSHIP_R2_20260906.log) / [独立摘要](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_PLAYER_BUILD_RERUN49.log)：非开发版 `BuildOptions.None`、Build Successful，无 `error CS` 或 Build failed。
- [RERUN49 启动烟测](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_STARTUP_SMOKE_AFTER_PROBE_OWNERSHIP_20260906.json)：15s 后优雅退出、exit code 0、探针和 Player.log 刷新；探针停在 MainMenu，故该证据仅为 `STARTUP_OBSERVED_ONLY`。

### 当前任务板与边界

- **已完成（限定范围）**：P0/P1/P2/P3/P4 本机自动化门禁；P2 的 1 个 D19A01 work-scene 条件跳过已显式记录，P4 的真实采样和构建/启动可观测性已闭合。
- **进行中**：VC-G01 总体验收的最后一关 `VC-G01-NATIVE`，包括原生 Player、物理键盘、画面观察和异常恢复矩阵。
- **阻塞**：CUA 仍无可绑定原生 Windows Player surface；本 handoff 不把脚本/合成输入、启动烟测或本机性能结果升级为人工视觉 PASS。D31-P04/S05/G06 继续 `PARTIAL`，VC-G02/VC-G03 继续 `LOCKED`。
- **下一步**：获得原生窗口控制面或用户交付逐步截图、Player.log 与路线探针后，执行 native gate；完成前保持 `VC-G01=IN_PROGRESS`，不解锁 VC-G02。

## 2026-09-06 用户报告四类 Player 缺陷后的定向修复

用户在最新 Player 中报告：人物动作扭曲、海洋不能游泳、上船后人物/船控呈现异常、树木悬空。本轮将“上船后异常”按已有 RERUN49 证据解释为玩家被挂到舵位后悬空：登船观测 `player_y=2.83`，稳定船根缩放为 `2.573956`；同时曾观测到玩家掉至 `y=-3450.21`。如果该条实际指的是船控逻辑而非悬空，需在下一次短复核中单独标注。

### 已完成的最小根因修复

- `D7ShipDeckController` 将世界甲板偏移反变换为船体局部坐标，消除稳定船根缩放对 `D7_HelmPoint` 的放大；新增缩放回归测试。
- `HarborCollisionController` 增加南/西/东三面海岸安全围界，并让所有 `ShoreBoundary*` 被地面射线排除；`D2CharacterController` 增加最近安全位置和越界恢复。游泳仍是未实现功能，不把“不会掉海”升级为游泳通过。
- R03 builder 让棕榈、灌木和岩石使用与 `D19_TerrainRoot` 相同的 `Height(x,z)`；已重新生成 [R03 presentation prefab](C:/Users/Administrator/Desktop/game/first01/game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)。
- 生成的 `D6CrewAnimator` 含绝对 imported Euler 曲线，不再接管当前运行时 rig；动作控制器暂时保持单一程序化/neutral 姿态层，并增加回归测试。

### 本轮证据

- [缺陷复核结果 JSON](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_REVALIDATION_20260906.json)：正式结论 `PARTIAL`，明确保留 native visual pending 与 swimming not implemented。
- [世界逻辑定向 EditMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_EDITMODE_WORLDLOGIC_R2_20260906.xml)：`7/7 PASS`，含围界、缩放船舵位和树木高度契约。
- [动作层定向 EditMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_EDITMODE_ANIMATION_20260906.xml)：`6/6 PASS`，含生成占位 Animator 不接管 raw Euler 姿态。
- [Unity 全量 EditMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_EDITMODE_FULL_20260906.xml)：`597/597 PASS`，failed/inconclusive/skipped 均为 `0`。
- [D17 动作 PlayMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_PLAYMODE_D17_ACTION_20260906.xml)：`1/1 PASS`，真实加载 `InitialHarbor` 并验证角色落地、W→Walk、跳跃、上下船动作过渡及生成 Animator 降级。
- [D19 路线 PlayMode](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_PLAYMODE_D19_ROUTE_20260906.xml)：`1/1 PASS`；[路线报告](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-S02-R01/D19_S02_R01_ROUTE_LOG.json) 记录 walk `5.508m`、ship `1.311m`、board/disembark/save/reload 均为 `true`，采样保持 grounded。
- [R03 定向 Python 回归](C:/Users/Administrator/Desktop/game/first01/game/tools/tests/test_d19_r03_visual_builder.py)：`27/27 PASS`。
- [R03 重建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_R03_BUILD_20260906.log) 与 [RERUN49 Player 构建日志](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_DEFECTS_PLAYER_BUILD_20260906.log)：均正常完成；Player 为非开发版 `BuildOptions.None`、`Build Successful`。

### 当前任务板

- **已完成（代码/自动化限定范围）**：四类报告均完成对应的最小可验证修复；D17/D19 PlayMode、全量 EditMode 回归通过；R03 prefab 与 RERUN49 Player 已重建。
- **进行中**：`VC-G01-NATIVE` 仍需短时画面复核，确认动作姿态、海岸围界、登船落点和树根贴地。
- **阻塞/边界**：本轮没有重新执行全量 Python（该命令超过有界等待窗口，已停止且不计为 PASS）；CUA 仍无可绑定 Windows Player surface。游泳功能仍未实现；`D31-P04/S05/G06=PARTIAL`、`VC-G02/VC-G03=LOCKED`。
- **下一步**：用户只需复核新 RERUN49 的四个点并提供必要截图；在此之前保持 `VC-G01=IN_PROGRESS`，不把代码测试升级为原生视觉 PASS。

## 2026-09-06 D17 拒绝下船 PlayMode 补证

为覆盖“成功之外的拒绝路径”，本轮运行 `D17A03DisembarkRefusesWhenNoWalkableSurfaceExists`，使用无可行走表面的场景条件检查下船请求是否被拒绝，而不是把没有落点误判成成功。

| 验证项 | 证据 | 结果 |
|---|---|---|
| D17 拒绝下船 PlayMode | `VC_G01_PLAYER_DEFECTS_PLAYMODE_D17_DISembark_REJECT_20260906.xml` | `1/1 PASS`，failed/inconclusive/skipped=0，退出码 0 |
| 拒绝日志 | 同名 `.log` | 先记录“已进入船只控制”，随后记录“下船被阻止：附近没有可行走的沙滩、码头或跳板” |
| 当前边界 | `VC_G01_PLAYER_DEFECTS_REVALIDATION_20260906.json` | 仅证明逻辑拒绝保持船控/父子关系，不证明原生画面；游泳仍未实现 |

这项补证关闭了当前自动化缺陷矩阵中的“无安全下船面拒绝”缺口，但不改变 `VC-G01=IN_PROGRESS`。原生 Player 四点画面复核仍是最后门禁，`VC-G02/VC-G03` 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。

## 2026-09-09 Q28/Q29 接管后真实 Player 补证

Q28 已生成独立的非开发版 `Q28-CANDIDATE-R2`，并用 D17A08 专项 `1/1 PASS` 验证游泳 visualRoot 的俯仰上限和高度下限。这个修复只防止旧的 `68°/-0.28m` 程序化姿态把角色压入水面，不等同于正式 walk/run/swim 动作资源；Q03 仍为 `BLOCKED_ASSET`。

Q29 已使用该候选 Player 完成真实 Win32 输入路线。R7 水域运行摘要为 `45/45` 输入步骤、`18/18` 截图、`InitialHarbor`、`152` observations、`71` moving observations、`101` swimming observations，I/M/T、Esc、登船/下船和重复/关闭矩阵均由只读探针报告为 true。R6 使用修正后的远离方向路线，报告 `boarded`、`disembarked` 和 `ship_too_far`，`far_ship_reject_proof=true`。

### 当前状态边界

- 真实运行逻辑/输入证据已补齐，但截图仍暴露中立/低模人物、水中动作质量不足，以及港口低模、岩石/植被层级和潜在穿插/悬浮风险；不能升级为最终视觉 PASS。
- R7 的 `owner_not_ready_reject_proof=false` 是诚实缺口；已有 bootstrap reject 专项单独作为证据，不把页面重复输入扩大解释为领域业务幂等。
- 正式状态保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步是 Q30 独立审查，之后才进入 Q31 最终 Player 门禁。

### 新增证据

- [Q28 RESULT](../../../../../../game/tools/results/LOCAL-QUALITY/Q28/RESULT.json) / [Q28 handoff](../../../../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。
- [Q29 RESULT](../../../../../../game/tools/results/LOCAL-QUALITY/Q29/RESULT.json) / [Q29 handoff](../../../../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。
- [Q29 R7 runtime summary](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q29_R7_20260906.json)。
- [Q29 R7 water probe archive](../../../../../../game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN59_WATER_Q29_R7.json)。
- [Q29 R7 screenshot directory](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q29_r7_20260906/)。

## 2026-09-09 Q30 独立审查与 Q30-R7 真实 Player 复核

Q30 已完成 Q28/Q29 证据审查和最小边界修复。R03 presentation builder 将尖锐岩石/灌木轮廓改为闭合八边形圆钝多环几何，并继续使用 `Height(x,z)` 与 `safeRadius=0.80` 岛内收缩；未验证棕榈资源继续不渲染，保持 `BLOCKED_ASSET` 边界。D2 fallback 玩家游泳 visualRoot 调为 `64°`，物理根不倾斜，未知 imported rig 仍不猜测局部骨骼轴。

### Q30 证据

- [Q30 RESULT](../../../../../../game/tools/results/LOCAL-QUALITY/Q30/RESULT.json)：正式状态 `PARTIAL`。
- [Q30-R7 build](../../../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R7.exe) / [build log](../../../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R7_BUILD.log)：standalone non-development Player，`BuildOptions.None`，Build Successful。
- [Q30-R7 runtime summary](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q30_R7_20260906.json)：真实 Win32 `45/45` 输入、`18` 截图、`152` observations、`69` moving、`102` swimming；route/business/pause/interaction matrix true，fatal error null。
- [Q30-R7 Player.log](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_Q30_R7_20260906.log)：scene、board/disembark、I/M/T、pause/resume marker。
- [Q30-R7 route probe](../../../../../../game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN59_WATER_Q30_R7.json) 和 [screenshots](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q30_r7_20260906/)。
- D17 动作整组 `15/15 PASS`、VC-G01 world logic `12/12 PASS`、Q20/Q21 `1/1`、Python 定向回归 `47/47 PASS`。

### Q30 状态边界

- `动作扭曲`：绝对 imported Euler placeholder Animator 已隔离，fallback 逻辑可驱动且回归通过；程序化人物仍是低模占位，Q03 `BLOCKED_ASSET` 没有解除。
- `海洋不能游泳`：逻辑/移动/Swimming observation 已有真实 Player 证据，但截图仍不是正式游泳动画，视觉保持 `PARTIAL`。
- `上船/舵位`：boarded/disembarked marker 和舵位截图路径成立，最终人物/船/镜头美术仍未收口。
- `树悬浮`：旧尖锐三角轮廓已收圆，未验证 palm 资源继续隔离；所有相机角度的树根/岸线接地与正式植被资源仍待完成。
- `W/S 与右上角菜单`：Q13/Q15 以及 Q30-R7 的实际输入路线支持 gameplay 与 I/M/T/Esc 分权；不将这项逻辑证据扩大为视觉质量通过。

VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。下一步为 Q31 最终 Player 视觉门禁；不要求用户持续交出电脑，只有在自动探针无法判定的最终主观画面质量上才需要人工确认或正式美术资源。

## 2026-09-09 Q30-REMEDIATION-R8 运行交接

用户继续报告四类 Player 问题。本轮完成了代码修复、自动化回归、R03 重建和一次有界原生 Windows Player 路线，不覆盖既有 R7 证据。

### R8 变更

- `D2FallbackHumanoidBuilder` 收紧 fallback 躯干/手臂包络；`D2CharacterAnimationController` 降低程序化姿态幅度，避免普通动作和登船过渡把低模角色拉散。
- R03 岸线岩石降低轮廓并用 `Height(x,z)-0.015m` 贴地；`GroundVegetationPosition` 保持岛内安全半径；未验证棕榈/树资源继续不渲染。
- `HarborInteractionStabilizer` 在 gameplay 状态关闭主 HUD Button 自身 UGUI 导航，模态页面仍可导航，覆盖 W/S 与右侧菜单冲突。
- route probe 增加 Ship 控制态、船体位置/航向汇总和 RERUN60；WaterProbe 先真实 W/A 控船再 E 下船，随后真实 S/W 进出水域。

### R8 证据

- [Q30 R8 result](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_REMEDIATION_R8_RESULT.json)
- [VC-G01 result](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)
- [D16 HUD navigation R2](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_D16_HUD_NAV_REMEDIATION_R2.xml)：`1/1 PASS`。
- [D17 swimming R3](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_D17_SWIM_REMEDIATION_R3.xml)：`1/1 PASS`。
- [R8 Player build](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R8_BUILD.log)：非开发版、`BuildOptions.None`、Build Successful。
- [R8 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q30_R8_20260906.json)：`48` input steps、`19` screenshots、`167` observations、`125` swimming、`22` Ship control observations、船体位移 `5.7000008m`、航向变化 `15.7681885°`；route/business/pause/interaction 为 true，fatal 为 null。
- [R8 Player.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_Q30_R8_20260906.log)、[RERUN60 probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN60.json)、[R8 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q30_r8_20260906/)。
- Python targeted regression：`47/47 PASS`。
- [D17 full action revalidation R9](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_D17_ACTION_REMEDIATION_R9.xml)：`15/15 PASS`。A09 失败先被复现为错误净方向/长时间步组合，随后以严格反向路线和 D2 `50ms` movement step cap 修复并重跑通过；没有降低测试阈值。

### 状态与边界

R8 说明“逻辑是否执行”已经有更强证据：游泳状态进入/维持/返回成功，登船后船体确实移动/转向，主 HUD 不再由 W/S 导航，普通 grounded fallback 断肢观感已收紧。但截图仍是低模占位，港口空间层级/材质/相机和正式树资源仍未收口。因此：`VC-G01=IN_PROGRESS`、`VC-G02=LOCKED`、`VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`，不得把 R8 升级为最终视觉 PASS。

### 后续交接动作

Q31 继续作为最后视觉门禁：固定镜头矩阵、成功/拒绝/重复路径、资源准入和最终视觉质量仍需直接证据。R8 已完成的有界 Player 探针不要求用户让电脑连续运行；如最终需要主观美术判定，再由用户短时查看指定截图/Player。

## 2026-09-09 Q30-REMEDIATION-R16 运行交接

R15 的水域截图仍出现明显棕色空洞环。本轮通过源码追踪确认其来源是 presentation-only `D19_R03_HarborBackdrop/BackdropRock_2`，遂将未具备正式 offshore 资源与接地契约的 backdrop root 保持为空；同时保留 R03 闭合 cap 的 winding containment。该处理不触碰 Systems、AgentCore、碰撞或玩法裁决。

### R16 直接证据

- [Q30 R16 result](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_REMEDIATION_R16_RESULT.json)：状态 `PARTIAL`。
- [R16 Player build](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q30/Q30_CANDIDATE_PLAYER_R16_BUILD.log)：非开发版、`BuildOptions.None`、Build Successful。
- [R16 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q30_R16_20260906.json)：`48` inputs、`19` screenshots、`167` observations、`68` moving、`123` swimming、`22` ship-control；位移 `5.745615m`、航向变化 `15.743011°`；route/business/pause/matrix 均 true，fatal null。
- [RERUN68 probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN68.json)、[R16 Player.log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_Q30_R16_20260906.log)、[R16 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q30_r16_20260906/)。
- [Python targeted regression](C:/Users/Administrator/Desktop/game/first01/tools/tests/test_d19_r03_visual_builder.py)：`53/53 PASS`；[D17 R10](C:/Users/Administrator/Desktop/game/first01/game/game/tools/results/LOCAL-QUALITY/Q30/Q30_D17_ACTION_REMEDIATION_R10.xml)：`15/15 PASS`。

### 结论

R16 游泳截图中已不再出现 R15 识别出的 backdrop 空洞环，但初始港口和水域仍有低模 fallback 人物、建筑/岸线层级与接地风险；正式动作和棕榈/树资源仍未准入。故 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。R16 的真实输入证据不能扩大解释为最终视觉 PASS，也没有证明同一路线的 owner-not-ready I/M/T reject；far-ship reject 仍引用独立路线。

后续仍采用一次一轮、自动结束的有界 Player 探针；不要求用户让电脑连续运行。只有最终主观美术质量超出日志/探针裁决范围时，才需要用户短时查看指定截图或 Player。

## 2026-09-09 Q21-A01 / R17 最新交接

Q21-A01 已将 R03 四个服务建筑的 foundation root 从固定 `y=0.22` 改为 `Height(x,z)-0.02m`，针对已定位的漂浮/嵌地根因完成最小 presentation 修复。定向 Python 为 `55/55 PASS`，R03 rebuild R8 成功，独立非开发版 R17 构建为 `BuildOptions.None / Build Successful`。

R17 真实 Win32 路线证据：`48/48` 输入、`19` 截图、`167` observations、`68` moving、`123` swimming、`22` Ship control；船体最大位移 `5.9082966m`、航向变化 `17.7652588°`，route/business/pause/interaction matrix 为 `true`，fatal error 为 `null`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q21-A01/RESULT.json`，最新候选为 `game/tools/results/LOCAL-QUALITY/Q21-A01/Q21_A01_CANDIDATE_PLAYER_R17.exe`。

这次证据仍不能升级 VC-G01：截图继续显示低模动作/游泳、登船后人物与舵位可读性不足、港口层级/接地以及正式植被资源风险；运行日志还保留 `ShoreBoundary` 在 180 帧 bootstrap 内未出现的警告。故 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步按单一 owner 拆动作、游泳、登船镜头和植被问题，再进入 Q31 最终视觉门禁。

## 2026-09-09 Q21-A02 / R18 最新交接

Q21-A02 只处理 `CameraFollow` 的 ShipDeck 取景：distance 从 `10.5m` 收紧为 `7.5m`，pitch 从 `22°` 调整为 `18°`，target offset y 从 `1.45m` 调整为 `1.05m`；相机仍以已登船玩家为主体，受控船体仍排除自碰撞。定向 Python 为 `42/42 PASS`，R18 非开发版构建为 `BuildOptions.None / Build Successful`。

R18 真实 Win32 路线证据：`48/48` 输入、`19` 截图、`168` observations、`69` moving、`123` swimming、`22` Ship control；船体最大位移 `5.5561728m`、航向变化 `15.1758118°`，route/business/pause/interaction matrix 为 `true`，fatal error 为 `null`。船上截图中人物与舵位比 R17 更可读，但仍是低模 fallback；游泳、动作、港口层级/接地和正式植被未收口。权威结果为 `game/tools/results/LOCAL-QUALITY/Q21-A02/RESULT.json`，最新候选为 `game/tools/results/LOCAL-QUALITY/Q21-A02/Q21_A02_CANDIDATE_PLAYER_R18.exe`。

正式状态保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步继续选择一个动作/游泳/植被表现 owner，不将 R18 的镜头改善扩大解释为完整 Player 视觉通过。

## 2026-09-09 Q21-A03 / R20 与 Q15 R9 接管更新

本轮没有升级 VC-G01。Q21-A03 只处理 fallback 人物的游泳 stroke 与游泳取景，并用独立非开发版 Player R20 重新跑真实 Win32 输入；Q15 同时暴露并修复了一个可自动化前置验证的市场重开缺陷。

### R20 真实运行证据

- 权威 Q21-A03 结果：[Q21-A03 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A03/RESULT.json)。
- 当前 Q31 候选：[Q21_A03_CANDIDATE_PLAYER_R20.exe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A03/Q21_A03_CANDIDATE_PLAYER_R20.exe)，BuildOptions.None，构建日志为 `Q21_A03_CANDIDATE_PLAYER_R20_BUILD.log`。
- 真实 Win32 路线摘要为 `48/48` 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`23` Ship control；船体最大位移 `5.6351428m`、航向变化 `14.6616211°`，route/business/pause/interaction matrix 全为 true，fatal error 为 null。
- Player.log 记录 gameplay 状态 `EventSystem navigation=gameplay_suppressed`，只有临时页面持有 modal 导航；这为 W/S 不应切换主 HUD 提供运行日志证据，但不代替最终画面验收。
- 画面边界仍是 `PARTIAL`：fallback 角色动作/泳姿、船上人物与舵轮手部关系、港口层级/接地和正式树资源仍未收口。

### Q15 当前源码回归

- R7/R8 在第 2 次市场重开稳定失败，失败 XML/日志已保留，不作为通过证据。
- D29 表现层防重状态现在在 market/tavern/board/shipyard/preparation owner 会话关闭后清除；页面打开期间的重复请求仍由 `PrimaryInputAllowed` 拒绝。
- R9 `1/1 PASS`，定向 Python `80/80 PASS`；证据见 `game/tools/results/LOCAL-QUALITY/Q15/Q15_PANEL_FOCUS_PLAYMODE_R9.xml`、`.log` 和 Q15 handoff。

### 状态与后续

正式状态继续 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。Q31 继续 `WAITING_VISUAL`，不把 R20 的真实输入/日志/截图存在扩大为视觉 PASS。后续应按用户仍可复现的单一问题选择 Q03 动作资源、Q21 舵位/镜头或 Q20 植被资源 owner；不要求用户让电脑连续运行，Player 探针继续使用一次启动、一次路线、自动退出的有界方式。

## 2026-09-09 Q21-A04 / R24 舵位表现更新

Q21-A04 只在 D19 R03 presentation builder 内处理“上船人在控制瓢/舵位不可读”：候选船没有 helm/rudder/steering/wheel 语义节点，因此 R21 的开放圆柱、R22 的封口盘、R23 的遮挡修复都被真实截图逐轮否定；R24 改为带轮圈、四根轮辐和中心轮毂的可见轮舵，并抬至人物手部高度。D7 `HelmPoint`、`D7_VisibleHelmAnchor`、D2/D7 船控、碰撞和 Systems 未改动。

### R24 证据

- 权威结果：[Q21-A04 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A04/RESULT.json)。
- 当前 Q31 候选：[Q21_A04_CANDIDATE_PLAYER_R24.exe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A04/Q21_A04_CANDIDATE_PLAYER_R24.exe)，BuildOptions.None，Unity `Build Successful`。
- 定向 Python：`85/85 PASS`；Q26 舵位 PlayMode：`1/1 PASS`。
- 真实 Win32 路线摘要：[VC_G01_RUNTIME_INPUT_Q21_A04_R24_20260906.json](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q21_A04_R24_20260906.json)：`48/48` 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`22` Ship control；船体最大位移 `5.6047745m`、航向变化 `15.3561096°`，route/business/pause/interaction matrix 全为 true，fatal error 为 null。
- R24 board 截图已能读成轮圈/轮辐/轮毂；但角色仍为低模 fallback，泳姿、人物手部关系、港口整体美术和正式树资源仍不合格。

因此 Q21-A04=`PARTIAL`，Q31=`WAITING_VISUAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。R24 是当前候选，不再继续盲调舵位坐标；下一步只按用户仍能复现的动作/游泳/植被缺陷选择 owner。Player 探针仍是一次启动、一次路线、自动退出的有界流程，不要求用户持续开机。

## 2026-09-09 Q20-A01 / R26 最新运行交接

Q20-A01 针对“树在空中飘”完成了一个边界明确的 D26 presentation 修复：旧实现只检查场景根节点上的 `D19_SaintMargaret_Root`，现在遍历每个场景根的 Transform 子树，找到嵌套 legacy root 后禁用其全部 Renderer。没有改 Collider、NavMesh、Systems 或玩法裁决。

### 直接证据

- [Q20-A01 结果](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q20-A01/RESULT.json)：`PARTIAL`。
- 全量 Python 回归：`1139/1139 PASS`，耗时 `281.848s`；退出码为 0，坏 fixture/拒绝分支日志属于测试内容。
- Unity PlayMode `Q20_A01_LEGACY_TREE_GROUNDING_R1.xml`：`1/1 PASS`；日志记录 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`。
- R26 非开发版 Player：`BuildOptions.None`、构建成功；Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 `Assembly-CSharp.dll` SHA256 `D9205E09B173F4FF2302FFBD42AFC240659AF1F2948942067552AD4F9CACAB25`。
- [R26 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q20_A01_R26_20260906.json)：`48` 输入、`19` 截图、`167` observations、`69` moving、`123` swimming、`22` Ship control，船体位移 `5.7648530m`、航向 `16.1363525°`，route/business/pause/interaction matrix true，fatal null。
- [R26 route probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN78.json) 和 [R26 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q20_a01_r26_20260906/) 已归档。

### 当前门禁

R26 证明了新的 legacy Renderer containment 和真实运行路径，不证明正式树/棕榈资源已准入，也不证明树根、动作、泳姿或人物手部关系达到视觉门禁。截图仍显示低模 fallback、正式泳姿缺口、人物手部与轮舵接触不自然和港口层级风险。因此 `VC-G01=IN_PROGRESS`、`Q31=WAITING_VISUAL`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。

后续解除条件：Q20 等待 ready 资源后重做完整植被 bounds/坡面接地/Player 视觉复核；Q03/Q05/Q21 分别处理正式动作、泳姿和人物—舵轮关系。用户不需要持续运行电脑；Player 探针仍是一轮启动、输入、截图、自动退出。

## 2026-09-09 Q21-A05 / R27 fallback 操舵姿态交接

Q21-A05 处理用户报告的“上船人在控制瓢/不是舵”的 fallback 表现层根因。`D2ControlRouter` 实际挂在 `D2RuntimeBootstrap` 宿主，先前角色动画层没有收到 `Ship` 模式通知；现在由路由模式切换调用 `D2CharacterAnimationController.SetShipControlActive`，仅驱动程序化 fallback 的 presentation-only 对称操舵姿态，不改 D7 gameplay `HelmPoint`、船控裁决、碰撞或 Systems 所有权。

### 直接证据

- 权威结果：[Q21-A05 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A05/RESULT.json)：`PARTIAL`。
- R1/R2 失败诊断保留：R1 是错误路由查找，R2 两手到轮舵距离为 `0.9957m`；R3 `Q26A05ProceduralFallbackReachesVisibleHelmInShipMode` 为 `1/1 PASS`，两手距离均为 `0.4114m`。
- 改动后的 D17 动作回归：[R12 XML](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q21-A05_D17_ACTION_FULL_R12.xml)：`15/15 PASS`。
- 定向 Python：`57/57 PASS`；全量 Python：`1141/1141 PASS`，耗时 `280.227s`。
- R27 非开发版 Player：`BuildOptions.None`、构建成功；Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 `Assembly-CSharp.dll` SHA256 `0548872547E17AFE766BE382B4A7C0C9E2AC36CC6DA227E2F06D2856A86FC3F9`。
- [R27 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q21_A05_R27_20260906.json)：`48` 输入、`19` 截图、`168` observations、`69` moving、`124` swimming、`21` Ship control；船体位移 `5.8478036m`、航向变化 `17.3246765°`，route/business/pause/interaction matrix true，fatal null。
- [R27 route probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN79.json) 和 [R27 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q21_a05_r27_20260906/) 已归档。

### 视觉边界与门禁

真实截图确认轮舵局部可读性和 fallback 手臂朝向比前一轮更接近操舵，但仍暴露低模角色、泳姿像碎片/侧躺、自然手部接触不足、树木/植被资源缺口以及建筑/道路/岸线漂浮穿插和空间层级风险。真实路线的 `swimming_observations` 证明逻辑状态进入，不等于“正式游泳画面通过”；`D17 15/15` 也不等于正式动作美术通过。

因此 `Q21-A05=PARTIAL`、`Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06 继续 `PARTIAL`。下一步按单一 owner 处理 Q03 正式动作/Avatar 阻塞或用户可复现的游泳、植被、漂浮/穿模缺陷；不再用舵位坐标调整覆盖正式资源缺口。用户不需要持续开机，R27 采用一次启动、一次输入路线、自动退出的有界流程。

## 2026-09-09 Q05-A01 / R28 游泳 presentation 复核交接

Q05-A01 在同一 D2 presentation owner 内收窄 fallback 游泳摆幅、移除第二个横向 roll，并保留物理玩家根不倾斜、离水恢复和 Ship/Helm 控制边界。该轮没有改 D7 船控裁决、碰撞、Systems 或正式 imported rig。

### 直接证据

- [Q05-A01 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q05-A01/RESULT.json)：`PARTIAL`。
- 定向 Python `53/53 PASS`；全量 Python `1141/1141 PASS`。
- D17 目标 PlayMode `1/1 PASS`：R28 游泳 visualRoot pitch `58°—70°`、roll `0°`、body drop `-0.70m—-0.50m`，10-cycle root/physics/restoration 边界仍通过。
- R28 `BuildOptions.None` Player 构建成功；Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 `Assembly-CSharp.dll` SHA256 `2398E194424306871C4D07E6B251C096892CC43ED1E8153F81386FDC2B44CFF0`。
- [R28 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q05_A01_R28_20260906.json)：真实 native Win32 路线 `48` 输入、`19` 截图、`167` observations、`70` moving、`123` swimming、`22` Ship control；船体位移 `5.8326859m`、航向变化 `17.1057129°`，scene/route/business/pause/interaction matrix true，fatal null。
- [R28 route probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN80.json) 和 [R28 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q05_a01_r28_20260906/) 已归档。

### 视觉结论与门禁

R28 证明了真实输入、游泳状态和船控路径没有被本轮 presentation 变更破坏，但截图没有达到视觉通过：当前角色仍是低模 fallback，游泳仍读作直立/碎片化而非正式四肢泳姿，握舵关系不自然，港口存在树木/植被资源缺口及漂浮/穿模风险。`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`、`Q05-A01=PARTIAL` 继续生效；`Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`，D31-P04/S05/G06 仍为 `PARTIAL`。

下一步应由 Q03 owner 处理合法动作/Avatar 适配，或建立明确的 D20 candidate-only 视觉集成卡；不要继续用有限 fallback 参数调整替代正式资源准入。用户不需要持续运行电脑，R28 仍是一轮启动、真实 OS 输入、截图、自动退出的有界流程。

## 2026-09-09 Q05-A02 / Q05-A03 R29/R31 最新运行交接

Q05-A02 修复程序化 fallback 游泳前臂的确定性 child-space 折叠；Q05-A03 清除 D7 在游泳状态下由 stale grounded 帧重新施加的视觉落地偏移，并更新当前舵位校准。两轮都只在 presentation/test 责任边界内改动，没有改变 Systems、AgentCore、物理玩家根、船控裁决或正式资源准入。

### 直接证据

- [Q05-A02 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q05-A02/RESULT.json)：`PARTIAL`。
- [Q05-A03 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q05-A03/RESULT.json)：`PARTIAL`。
- 定向 Python `56/56 PASS`；全量 Python `1143/1143 PASS`。
- Q05-A02 水域专项 `1/1 PASS`；Q05-A03 Q26 `6/6 PASS`、D17 `15/15 PASS`。
- R31 非开发版 Player：`BuildOptions.None`；Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 `Assembly-CSharp.dll` SHA256 `FEBB28ECB7A89B624255214420768B0FA2E9E4DB9A5A7D2D34498E7260EB14FE`。
- [R31 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q05_A03_R31_20260906.json)：真实 native Win32 路线 `48` 输入、`19` 截图、`164` observations、`72` moving、`122` swimming、`21` Ship control；船体位移 `5.9478812m`、航向变化 `17.8417969°`，route/business/pause/interaction matrix true，fatal null。
- [R31 route probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN83.json) 和 [R31 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q05_a03_r31_20260906/) 已归档。

### 视觉边界与门禁

R31 证明了当前输入/状态逻辑路线仍可运行，也关闭了 stale grounding 的代码级回归；但截图仍显示低模 fallback、直立/碎片化泳姿、自然手部—舵轮接触不足、港口空间层级以及树/植被视觉债务。`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`、`Q05-A02/Q05-A03=PARTIAL`，`Q31=WAITING_VISUAL`、`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。

当前不需要用户让电脑持续运行；若需要主观画面确认，只需短时启动 R31 并按 Q31 清单记录问题。下一步优先解除正式动作/Avatar 与正式植被资源门禁，或明确批准 candidate-only 视觉集成卡；不能把真实输入路线或 PlayMode 通过扩大解释为最终视觉 PASS。

## 2026-09-11 Q26 R3 港口接地 bounded Player 复核

### 本轮完成

- R03 builder 将 `KenneyPirateTower` 从旧的 `(-15, 13)` 无 R03 地形支撑位置移到 `(-12.8, 0.20, 11.8)`；注释明确该候选只拥有表现，不取得 gameplay/collider/NavMesh 权限。
- R03 builder 和 `HarborPresentationController` 都抑制重复 `D6_HarborFoam` Renderer，保留 D6 runtime root 兼容性，并新增 A07 防回归。
- 市场入口移动到初始玩家镜头之外，新增 A08；A09 使用 `Physics.RaycastAll` 并要求命中 `D19_R03_HarborPresentation/D19_TerrainRoot`，避免 legacy 同名地形产生假阳性。
- Unity Pipeline recompile 无错误；`D19R03Q26GameplayDefectPlayModeTests` 为 `9/9 PASS`。静态 Python 回归为 `test_q26_spatial_cleanup.py 4/4` 与 `test_vc_g01_runtime_probe.py 6/6`。
- 最终全量 Python 回归为 `1148/1148 PASS`；旧的 R66 海盗塔坐标断言已更新为新的 R03 authored-terrain 坐标后通过。

### 真实运行证据

- Player：`game/tools/results/LOCAL-QUALITY/Q26/Q26_PIRATE_PORT_GROUNDING_CANDIDATE_R3.exe`，`BuildOptions.None`，`MainMenu;InitialHarbor`。
- 运行：route `-d19-r03-route-rerun84`，真实 native Win32 输入 `48/48`，窗口截图 `19` 张，InitialHarbor，`167` observations，其中 `70` moving、`123` swimming、`22` Ship control；业务页、暂停恢复、交互矩阵为 `true`，`fatal_error=null`，clean close。
- 归档：
  - [Q26 R3 result](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q26/Q26_PIRATE_PORT_GROUNDING_RECHECK_20260911_RESULT.json)
  - [VC-G01 runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q26PIRATEPORTR3_20260911_20260906.json)
  - [R84 route probe](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN84.json)
  - [R3 screenshots](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q26pirateportr3_20260911_20260906/)

### 门禁边界

三张代表性截图已实际检查：海盗塔不再以本次定位的无支撑位置进入游泳路线，市场入口的旧初始前景遮挡已消失；但大块平台/建筑几何、低模角色、直立泳姿和其余层级/接地风险仍然可见。该结果是 scoped `PASS_WITHIN_SCOPE`，不是视觉总门通过。`VC-G01` 继续 `IN_PROGRESS`，`Q31` 继续 `PARTIAL`，`VC-G02/VC-G03` 继续 `LOCKED`；本路线没有覆盖 owner-not-ready 拒绝，且现有 `far_ship_reject_proof=false`。

下一步仍按单一 owner 选择一个可验证的视觉缺陷，生成新的 bounded Player build 和截图；不要同时改动作资源、游泳 presentation、港口层级和植被资源，也不要用这次 9/9 或真实输入路线替代最终视觉门禁。

## 2026-09-11 Q29 R4 UI 导航冲突复核

当前候选 `Q29_UI_NAVIGATION_CANDIDATE_R4` 已用 Unity Pipeline 构建并通过真实 Win32 Player 路线复核。路线摘要为 `48/48` 输入步骤、`19` 张窗口截图、`167` observations、`499` events、`125` swimming observations、`22` Ship control observations，InitialHarbor，clean close，fatal=null。

为避免把“截图看起来没切换”当证据，`D19R03PlayerRouteProbe` 新增只读 UI 诊断。真实路线中 `78` 个 gameplay movement-key 样本全部满足：EventSystem found、`sendNavigationEvents=false`、`InputSystemUIInputModule.move` 未绑定、当前选中控件为空、`gameplay_ui_navigation_suppressed=true`；`gameplay_ui_navigation_conflict_observed=0`。因此本轮没有复现 W/S 驱动右侧 HUD 导航的运行时冲突。

该结果不能升级 VC-G01：R4 截图仍显示低模 fallback、游泳姿态/握舵关系不足和港口几何/层级/接地风险；owner-not-ready I/M/T 拒绝、领域幂等、目标设备性能也不在这条路线内。权威结果见 [Q29 UI 导航复核](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q29/Q29_UI_NAVIGATION_RECHECK_20260911_RESULT.json)。VC-G01 继续 `IN_PROGRESS`，Q31 继续 `PARTIAL`，VC-G02/VC-G03 继续 `LOCKED`。

## 2026-09-11 Q28-CANDIDATE-R3 当前候选 Player 路线复核

### 构建与路线证据

- 当前源码经 Unity Pipeline detached build 生成 `game/tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_R3.exe`，`BuildOptions.None`，场景为 `MainMenu;InitialHarbor`，构建任务成功，Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Pipeline error console `0`。
- 水上路线结果：`48/48` 输入、`19` 张截图、`166` observations，`124` swimming、`21` Ship control，业务页/暂停/交互矩阵均为 true，Player clean close。结果见 `game/tools/results/LOCAL-QUALITY/Q29/Q29_PLAYER_RECHECK_20260911_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q28R3_20260911_20260906.json` 和 `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN60.json`。
- 远距路线结果：`45/45` 输入、`17` 张截图、`123` observations，其中 `81` 次 `ship_too_far` 拒绝，Player clean close；结果见同一 Q29 result、`VC_G01_RUNTIME_INPUT_Q28R3FAR_20260911_20260906.json` 和 `D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN61.json`。

### 状态边界

本轮新增的是当前候选的真实 Windows Player/Win32 输入和拒绝反馈证据，不是最终视觉验收。截图仍显示低模 fallback、游泳/人物—舵位关系不完整，以及港口几何、层级和接地风险；owner-not-ready 拒绝未在本轮覆盖。`Q28/Q29/Q31` 继续 `PARTIAL`，`VC-G01` 继续 `IN_PROGRESS`，`VC-G02/VC-G03` 继续 `LOCKED`。后续仍按 Q03 正式动作/Avatar、Q05 游泳视觉、Q21 舵位关系、Q20 正式植被与接地 owner 处理，不用继续调 fallback 参数冒充资源准入。

## 2026-09-10 用户实际试玩否决与 Q13-A01 输入修复

用户已实际进入当前 R31 Player，否决了“任务动作扭曲、海洋不能游泳、上船人物在控制错误物体/舵位关系错误、树在空中飘”，并再次确认 W/S 移动时右上角 HUD 菜单跟随切换的输入冲突。该反馈使 `Q31=PARTIAL`、人工门禁为 `REJECTED_ON_REPORTED_DEFECTS`；`VC-G01` 仍为 `IN_PROGRESS`，不能沿用此前“等待人工观察”的状态叙述。

Q13-A01 在 `HarborInteractionStabilizer` 中把 `InputSystemUIInputModule.move` 的所有权绑定到玩法/模态状态：玩法态置空，暂停/仓库/市场/酒馆/D16 模态恢复，退出后再次置空。D17-A15 专项 `1/1 PASS`，日志记录 `gameplay_w_s_cycles=10`、`normal_navigation=false`、`pause_navigation=true`、`blocked_navigation=true`、`restored_navigation=false`；源码已重新构建进 R31 Player，内置 Assembly-CSharp SHA256 为 `A67243C1922372973DA9BF6F41AC7A526E538F8705980852C7C6ECA591166E1B`。

本轮另做了一次有界 Win32 Player 尝试，但探针仍报告 `MainMenu`，`scene_marker_seen=false`、`probe_observations=0`、`runtime_route_proof=false`，没有把该次输入尝试伪造为 InitialHarbor route proof。由于同一源码下 D17A06 主菜单→港口生命期 PlayMode 为 `1/1 PASS`，当前只能把它登记为 Win32 探针未观察到转场，待区分点击/菜单状态/Player 路由边界。证据见 `game/tools/results/LOCAL-QUALITY/Q13-A01/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q13-A01/Q13_A01_RUNTIME_ROUTE_PROBE_PLAYER_R32_MENU_TRANSITION_BLOCKED.json` 和 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q13A01R32C_20260906.json`。

当前后续顺序：先诊断 Win32 探针未观察到 MainMenu→InitialHarbor 的点击/菜单状态/Player 路由边界；然后按单一 owner 处理 Q03 正式动作/Avatar、Q05 游泳视觉、Q21 舵位关系、Q20 正式植被/接地。`VC-G02/VC-G03` 继续锁定，不能用 D17/Q26 或历史 R31 路线逻辑证据覆盖用户当前画面否决。

## 2026-09-11 Q26/A10 最新源码 Player R2 真实路线复核

本轮使用已连接 Unity Pipeline 对 A10 场景变换后的源码重新构建 `Q26_A10_SCENE_BOUND_CANDIDATE_R2.exe`，避免把早于 `/ph_crew_male` 分离的旧 Player 当作当前源证据。构建 `build_9d0db92d0afd` 为 `StandaloneWindows64 / BuildOptions.None / Succeeded`，`totalErrors=0`、`totalWarnings=8`；构建细节见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A10_SCENE_BOUND_CANDIDATE_R2_BUILD_RESULT.json`。

### 真实 Win32 路线

- 完整交互路线：`VC_G01_RUNTIME_INPUT_Q26A10R2_20260906.json` + `RERUN85` probe；`46/46` 输入、`17/17` 截图、`122` observations、`36` moving、最大速度 `3.2000002861`，`turning_observed=true`、远距 `E` reject、I/M/T 重复与 Esc close/recovery、业务/暂停/优雅退出均有证据。
- WaterProbe 路线：`VC_G01_RUNTIME_INPUT_Q26A10W1_20260906.json` + `RERUN86` probe；`49/49` 输入、`19/19` 截图、`166` observations、`123` swimming、`22` ship-control observations，船体位移 `5.9714837074m`、航向变化 `17.3263244629°`；movement/swimming/ship control proof 均为 true。
- 运行窗口与日志：`runtime_input_q26a10r2_20260906/`、`runtime_input_q26a10w1_20260906/`，以及 `VC_G01_PLAYER_RUNTIME_INPUT_Q26A10R2_20260906.log`、`VC_G01_PLAYER_RUNTIME_INPUT_Q26A10W1_20260906.log`。

为支持不覆盖历史结果的新增 route 编号，`D19R03PlayerRouteProbe` 登记了 `rerun85/rerun86` 的 allow-list、截图前缀和输出 JSON 名称；定向 Python `6/6 PASS`，Pipeline recompile `failed=false/errors=[]`，运行结束 Editor `ready/stopped`、error console `0`。本轮之前第一次使用 R1 时，`rerun85` 未登记导致 `probe_exists=false`；该次被保留为输入尝试，不被当作运行通过，R2 才是有效 probe 证据。

### 状态边界

这次关闭的是“最新源码候选的真实脚本 Win32 交互证据”缺口，不是视觉门禁。当前截图仍明确暴露低模/程序化角色与直立泳姿、船舵接触可读性风险以及港口几何/层级/接地问题；正式动作资源仍受 `Q03=BLOCKED_ASSET` 约束，`Q20=PARTIAL/BLOCKED_ASSET`，`Q26/Q31=PARTIAL`，`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`。下一步按单一 owner 选择一个可复现视觉缺陷，不同时修改动作、游泳、船舵和植被。

## 2026-09-11 D35 回归基线同步补充

全量 Python 回归曾因 D35 R03 静态审计测试保留旧的 `300 Renderer` 期望而失败；当前连接 Editor 的正式 R03 审计为 `294 Renderer`，该测试契约已同步。定向 D35 测试 `1/1 PASS`，随后全量 `tools/tests` 回归 `1148/1148 PASS`（`168.034s`）。

该同步没有改变 Player、场景、材质或运行时逻辑，也没有扩展视觉证据。权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_D35_RENDERER_CONTRACT_SYNC_20260911_RESULT.json`。VC-G01 继续 `IN_PROGRESS`；Q26/Q31 继续 `PARTIAL`；VC-G02/VC-G03 继续 `LOCKED`。

## 2026-09-11 Q26/A12 完整流程 Player 交互证据闭环

### 环境修正与构建

此前 A11 Player 只打包 `InitialHarbor`。它能启动并运行，但没有经过 MainMenu 的 `SaveSystem.NewGame()`，因此 `GameState.CurrentShip` 不存在，D10 酒馆 owner 保持未就绪；该结果只登记为环境边界，不作为业务 PASS。A12 通过已连接的 Unity Pipeline 构建 `MainMenu + InitialHarbor`，build id=`build_183e12159163`，`StandaloneWindows64 / BuildOptions.None / 0 errors / 1 warning`，Player 与 Assembly-CSharp 哈希已写入 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A12_FULLFLOW_FRAMING_CANDIDATE_R1_BUILD_RESULT.json`。

### 真实 Win32 路线

- 完整路线 `VC_G01_RUNTIME_INPUT_Q26A12R1_20260906.json` / `RERUN85`：`46/46` 输入、`17` 截图、`122` observations、`36` moving；WASD 移动/转向、E 远船拒绝/登船/下船、I 仓库、M 市场、T 酒馆、重复操作稳定、Esc 关闭并恢复、业务页、暂停恢复和 clean close 均有日志与探针证据。D10 owner-ready=`122`，T open/close 各 `1`，重复 T 稳定页和 Esc 恢复均为 true。
- 水域路线 `VC_G01_RUNTIME_INPUT_Q26A12W1_20260906.json` / `RERUN86`：`49/49` 输入、`19` 截图、`166` observations、`69` moving、`124` swimming、`21` Ship control；船体位移 `6.0626301765m`、航向变化 `18.5478515625°`，游泳/船控/交互矩阵/业务/暂停/clean close 均有真实证据。
- Unity `D19R03Q26GameplayDefectPlayModeTests`=`10/10 PASS`、相机契约=`6/6 PASS`、Python 全量=`1148/1148 PASS`；Pipeline recompile `failed=false/errors=[]`，Editor=`ready/stopped`，error console=`0`。

### 状态边界

已检查船控、游泳和酒馆截图。ShipDeck 由 `CameraFollow` 的 `5.5m/20°/target.y=1.25m` 取景调整后，舵轮在该有限范围内更可读；但游泳仍为直立/低模表达，港口几何/层级/接地风险仍可见。故本轮是 `PASS_WITHIN_SCOPE` 的真实交互证据，不是视觉总门：`VC-G01=IN_PROGRESS`、`Q26/Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`。owner-not-ready 拒绝路径本轮没有伪造为 PASS，仍需独立窗口证据；下一步按单一 owner 处理正式动作/Avatar、泳姿或植被/接地中的一个问题。

## 2026-09-11 A16 R03 boundary bridge runtime evidence

### 本轮新增

使用已连接 Unity Pipeline 构建当前源码 `Q26_A16_R03_BOUNDARY_CANDIDATE_R1.exe`，并通过真实 Windows Player/Win32 键鼠路线执行 `RERUN88`。本轮修复了 presentation bridge 对 D6-owned `ShoreBoundary.South/.West/.East` 外围集合的识别：外围边界保持在水域外，旧的精确 `ShoreBoundary` 兼容移动路径仅在该对象实际存在时使用。

### 真实运行证据

- 构建 `build_319d0c427c0d`：`StandaloneWindows64 / BuildOptions.None / MainMenu + InitialHarbor / Succeeded / 0 errors / 8 warnings`。Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA256=`5DED3D4CC4332EAB48898D842BDCA1043965AAFFFBA7E3DEC165D29AA1010AB0`。
- `VC_G01_RUNTIME_INPUT_Q26A16R1_20260906.json`：`46/46` 输入、`17` 截图、`122` observations、`36` moving；scene/movement/turning/swimming/business/pause/far-ship reject/interaction matrix/clean close 有真实 probe、日志和输入证据。
- Player 日志确认 `[D19-R03] route collision window retained: D6-owned R03 perimeter boundaries remain outside water`，并记录 legacy physical colliders disabled=`123`、non-blocking triggers preserved=`22`。本轮 `ship_control_proof=false`，不得把它写成船控通过；A12 WaterProbe 的船控证据仍单独保留。
- Unity 全量 EditMode=`619/619 PASS`，Q26 PlayMode=`10/10 PASS`，VC-G01 world logic EditMode=`13/13 PASS`，Python 全量=`1150/1150 PASS`，定向契约=`20/20 PASS`；recompile `failed=false/errors=[]`，Editor=`ready/stopped`，error console=`0`。

### 状态边界

本轮是 `PASS_WITHIN_SCOPE` 的运行交互与碰撞边界桥接证据，不是 `VC-G01` 视觉通过。A16 初始港口截图仍显示侧面建筑过近/裁切、低细节程序化玩家以及港口几何/接地风险；正式动作/Avatar、泳姿、船控表现和最终视觉仍未收口。故 `VC-G01=IN_PROGRESS`，`Q26/Q31=PARTIAL`，`VC-G02/VC-G03=LOCKED`。下一步应选择单一 owner 的正式动作/Avatar、泳姿或港口几何/接地缺陷，并为其建立新的可验证假设；不得继续把坐标微调或路线运行证据扩大为视觉 PASS。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A16_R03_BOUNDARY_CANDIDATE_R1_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q26A16R1_20260906.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-11 A17 InitialHarbor first-frame framing candidate

### 变更

A16 的真实首帧测量显示玩家左右约 `6.4m` 处的 Market/Warehouse 建筑（可见宽约 `5m`）仍贴近水平视锥边缘。A17 在 `CameraFollow` 中把 InitialHarbor Character 启动构图距离从 `13.5m` 收紧假设为 `18.5m`，只影响第一帧 presentation；未移动建筑、未改碰撞、未改 D2/D7、ShipDeck 或游泳逻辑。为保证证据不覆盖历史结果，新增 `RERUN89` probe/screenshot namespace。

### 运行和回归证据

- Pipeline build `build_d7a1d45e6e1e`：`StandaloneWindows64 / MainMenu + InitialHarbor / Succeeded / 0 errors / 8 warnings`；结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A17_INITIAL_HARBOR_FRAMING_CANDIDATE_R1_BUILD_RESULT.json`。
- A17 native Win32 route `VC_G01_RUNTIME_INPUT_Q26A17R1_20260906.json` + `RERUN89`：`46/46` 输入、`17` 截图、`122` observations、`36` moving；movement/turning/swimming、E 远船拒绝、I/M/T/Esc、业务页、暂停恢复、clean close 均由真实 Player probe/日志取证；本轮 `ship_control_proof=false`。
- A17 初始帧：`runtime_input_q26a17r1_20260906/20260911_112009_route_initial_harbor.png`。与 A16 帧相比，中心路线周围露出更多地面，左右建筑不再占据同样多的近景；但左下仍有裁切，玩家和地面材质仍粗糙，故仅 `IMPROVED_WITHIN_SCOPE`。
- 回归：Python 定向=`20/20 PASS`、全量=`1150/1150 PASS`；相机 PlayMode=`1/1 PASS`；VC-G01 world logic=`13/13 PASS`；Q26 PlayMode=`10/10 PASS`；Unity 全量 EditMode=`619/619 PASS`；recompile 无错误；Editor=`ready/stopped`；error console=`0`。

### 状态边界

A17 关闭的是“首帧侧建筑过近/裁切”的一个 bounded presentation 假设，不是视觉总门。低细节/程序化玩家、直立/低质量泳姿、握舵表现、地面断缝/材质、正式动作/Avatar 资源和 D31 全局视觉仍未收口。故 `VC-G01=IN_PROGRESS`，`Q26/Q31=PARTIAL`，`VC-G02/VC-G03=LOCKED`。后续不再继续对 InitialHarbor 首帧做无新证据的坐标微调，应选择正式动作/Avatar、泳姿或港口几何/接地中的单一 owner。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A17_INITIAL_HARBOR_FRAMING_CANDIDATE_R1_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-11 Q26/A20 游泳镜头跟随与真实 Player 复核

本轮通过 Unity Pipeline 对当前源码完成了一个有界的 CameraFollow 修复：游泳期间镜头自动跟随 D2 物理朝向，直到真实鼠标/右摇杆 look 输入接管；同时拒绝非有限 camera pose/position，消除全量 PlayMode 中实际暴露的 NaN Transform 写入。Q03 candidate-only 测试补了输入/HUD 状态恢复，避免全量顺序污染；正式 InitialHarbor consumer 和 Systems owner 未改变。

### 真实证据

- A20 build `build_3bba5aaa08b8`：StandaloneWindows64，MainMenu + InitialHarbor，`0 errors / 8 warnings`；产物 `Q26_A20_SWIMMING_CAMERA_FINITE_GUARD_CANDIDATE_R1.exe`。
- RERUN92 WaterProbe：真实 standalone Player 原生 Win32 键鼠事件 `49/49`，19 张窗口截图，167 observations，68 moving、119 swimming、21 ship-control；船体位移 `6.00153923034668m`，航向变化 `17.986114501953125°`；business/pause/I-M-T/Esc 矩阵、日志 marker、clean close 成立，fatal=`null`。
- 相关结果：`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q26A20W1_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN92.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_Q26A20W1_20260906.log`。

### 回归与视觉判定

- Python 全量 `1152/1152 PASS`；Unity 全量 EditMode `619/619 PASS`；全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`；Q03 targeted `1/1 PASS`；Pipeline recompile 无错误；Editor=`ready/stopped`；error console=`0`。
- 水域截图现在能读出横向游泳姿态，说明镜头跟随假设在真实 Player 中生效；但角色仍是低模程序化 fallback，初始港口和船控帧仍可见建筑近景/裁切、岸线/地面断缝和粗糙几何。因此本轮登记 `PASS_WITHIN_SCOPE_NOT_VISUAL_PASS`，不升级 VC-G01。
- `far_ship_reject_proof=false` 只表示本次 WaterProbe 没覆盖远距拒绝窗口；owner-not-ready reject 仍未在该序列中推断为通过。

### 当前状态与下一步

`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q26/Q31=PARTIAL` 保持不变。A20 已足以说明“当前源码 Player 可真实移动、游泳、控船并完成业务/暂停路径”，但还不足以说明“角色资源、动作、植被、港口几何视觉已通过”。后续按单一 owner 继续，优先处理正式动作/Avatar 或港口几何/接地，不再继续无新证据的相机坐标微调。

## 2026-09-11 Q26/VC-G01 RERUN93 正常路线与拒绝证据

### 本轮变更

为避免覆盖 A20 的 `RERUN92` 证据，路线探针只新增了独立的 `RERUN93` allow-list、截图前缀和结果文件名；没有改变玩法 owner、场景几何、输入绑定或存档状态。源码变更文件为 `game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs`，配套静态契约为 `tools/tests/test_vc_g01_runtime_probe.py`。

### 构建与真实运行

- Unity Pipeline build `build_9d94c976be2d`：`StandaloneWindows64`、`MainMenu + InitialHarbor`、`BuildOptions.None`、`0 errors / 8 warnings`；Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA256=`80858819CBE56CC4BEC8A0398D4038BF015377FCB491EACFAE90506B2B5C6101`。
- RERUN93 使用真实原生 Win32 键鼠事件：`46/46` 输入、`17` 张窗口截图、`121` observations、`39` moving、`59` swimming；MainMenu→InitialHarbor、WASD 移动/转向、E 登船/下船、远离船只后 E 拒绝、I/M/T 重复操作、Esc 关闭/恢复、业务页和优雅退出均有 Player 探针/日志证据。
- 远船拒绝为 `far_ship_reject_proof=true`；页面矩阵为 `interaction_matrix_proof=true`，其中仓库 `3` 次输入边沿、市场 `2` 次、酒馆 `2` 次，临时页最终关闭。I/M/T owner-not-ready 启动窗口不在本次路线中，保持未证明。
- 本路线进入了 `Ship` control mode 的 `6` 个观察样本，但没有在该普通路线中发送 WaterProbe 船控段，因此船位移 `0m`、航向变化 `0°`，`ship_movement=false`；船控正证据继续引用 A20 `RERUN92` WaterProbe，不把本轮失败边界伪造成通过。

### 截图审查与状态边界

已检查首帧、远船拒绝、仓库和暂停截图。它们证明窗口截图与反馈状态可采集，但仍显示低模/程序化人物、粗糙港口/岸线几何和 UI 遮罩风险；不升级视觉门。

权威结果：

- `game/tools/results/LOCAL-QUALITY/Q26/Q26_G01_RERUN93_CANDIDATE_R1_BUILD_RESULT.json`
- `game/tools/results/LOCAL-QUALITY/Q26/Q26_G01_RERUN93_REAL_PLAYER_RESULT.json`
- `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q26_G01_RERUN93_20260906.json`
- `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN93.json`

本轮只关闭“当前源码正常路线的真实 Player 远船拒绝与页面矩阵证据”这一有限缺口；`Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。下一步仍按单一 owner 处理正式动作/Avatar、泳姿或港口几何/接地中的一个，并为其重新构建 Player；不得用 RERUN93 的输入通过替代视觉验收。

## 2026-09-11 Q21-A06 RERUN94 fallback 前臂复核

本轮只改 D2 程序化 fallback 的左右前臂：在既有 `-35°` 上臂目标上加入 `-10°` child-space 弯曲。Pipeline 只读运行时证明登船后玩家位于 D7 `HelmPoint=(0,0.96,-6.67)`，父节点为 `ph_ship_fishing`，双手位于可见轮圈附近；因此未继续修改 D7 舵位、船体或相机。

### 证据

- 新 Player build=`build_693e29d49f64`，`MainMenu + InitialHarbor`，`0 errors / 1 warning`；Player SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`308B887DFB702E0A7A8467DD0977F965EFAB39B802F3564DABF9D15C0760F6FD`。
- RERUN94 真实 Win32：`46/46` 输入、17 截图、122 observations、36 moving、60 swimming；移动/转向/E 登下船/远船拒绝/I-M-T 重复/Esc/业务/clean close 有证据。本路线没有发送 WaterProbe 船控段，船位移=`0`，不替代 A20 船控证据。
- 截图：`game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q21_a06_rerun94_20260906/20260911_141012_board_or_reject_e.png`。
- 定向 Python=`12/12 PASS`、全量 Python=`1152/1152 PASS`、Q26/A05=`1/1 PASS`、Unity EditMode=`619/619 PASS`。
- 全量 PlayMode 真实结果为 `98 passed / 1 failed / 1 skipped of 100`；失败固定为 Q03 `W -> Walk` 实际为 `Idle`，独立重跑 Q03=`1/1 PASS`。该失败未被隐藏，不能把全量升级为 PASS。

### 状态边界

截图只显示前臂局部改善，仍是低模 fallback，手部与轮圈接触不自然；正式 Avatar/动作、泳姿、植被接地和港口几何仍未通过。因此 `Q21/Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。权威结果：`game/tools/results/LOCAL-QUALITY/Q21-A06/RESULT.json`。

## 2026-09-11 Q03-A02 D20 candidate 状态映射与全量 PlayMode 回归

全量 PlayMode 的 Q03 失败经 Unity Pipeline 诊断为 D20 presentation map 的首帧阈值问题，而不是 VC-G01 交互输入冲突：D16 `axis=(0,1)`、W pressed、D2 `IsMoving=true`、`Action=Walk`，但加速首帧 `snapshotSpeed01=0.041` 被 `Idle <= 0.05` 判定为 Idle。已将纯表现映射 Idle 阈值改为 `0.01`，并增加 `0.01 Idle / 0.04 Walk` EditMode 边界测试。

证据：D20 EditMode `6/6 PASS`、Q03 targeted PlayMode `1/1 PASS`、全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`；recompile `failed=false/errors=[]`；error console=`0`；Editor=`ready`。本轮只关闭 candidate 状态映射和全量确定性缺陷，不改变正式 InitialHarbor consumer。

状态仍为 `Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。同时保留下一张 Q03-A03：日志已确认 D27 表现层在 candidate 安装后重新启用 `17` 个旧 player Renderer（`restored=17`），该所有权冲突会造成重复人物/穿模风险，需单独补回归和修复，不能用本卡的 PlayMode 绿灯覆盖。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A02/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q03/RESULT.json`。

## 2026-09-11 Q03-A03 D27 candidate Renderer 所有权修复

全量日志确认 candidate 安装后的重复人物风险来自 presentation owner 冲突：D20 candidate bridge 隐藏旧 player Renderer，随后 D27 通用 `RestoreRenderers(player)` 曾恢复 `17` 个旧 Renderer。已在 D27 增加最小 guard：检测 `D20PlayerCandidatePresentationBridge.IsInstalled=true` 时跳过 player restore，只保留 crew restore，并把日志语义修正为 `candidateOwned=true/restored=0`；正式 fallback 没有 candidate 时保持正常路径。

证据：recompile `failed=false/errors=[]`；Q03 candidate targeted PlayMode `1/1 PASS`，日志 `playerRenderers=24, restored=0, candidateOwned=True`；全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`；正式 fallback 日志 `playerRenderers=17, restored=0, candidateOwned=False`；error console=`0`、Editor=`ready`。Q03 仍是 candidate-only，formal InitialHarbor consumer 仍为 `0`。

本轮只关闭代码层重复 Renderer/穿模风险，不把它扩大为 native visual PASS。最新 Player 尚未因本卡重建，故 `VC-G01=IN_PROGRESS`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持不变。下一张 Q03-A04 必须用最新源码重建 Player，再以独立 route/screenshot namespace 检查登船、游泳和重复人物帧。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A03/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q03/RESULT.json`。
## 2026-09-11 Q03-A04 RERUN95 latest candidate native route

- 使用 Unity Pipeline 构建独立 `Q03_A04_LATEST_CANDIDATE_PLAYER.exe`，`MainMenu + InitialHarbor`、`BuildOptions.None`、构建 job `bf7fdea8a46644c9999a2af4e91f7b86`；Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- 真实 Win32 RERUN95：`48` 输入步骤、`19` 截图、`166` observations、`70` moving、`123` swimming、`22` ship-control；船体位移=`6.058244m`、航向=`18.174286°`；WASD/转向、E、I/M/T、Esc、业务/暂停矩阵和 clean route 有记录。
- `RERUN95` 是新的 route probe namespace，结果为 `INPUT_ATTEMPTED`；远船 reject 和 owner-not-ready reject 未覆盖。它证明了真实运行路径，但不证明视觉质量。
- 通过 `view_image` 复核后，未在仓库重复操作抽查帧看到第二个 legacy player Renderer；但玩家仍低模/T-like，Fish 游泳姿态抬臂/倾斜，舵轮手部接触弱，港口仍有粗糙/浮空/裁切几何。因此 `VC-G01` 保持 `IN_PROGRESS`，`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`。
- 证据：`game/tools/results/LOCAL-QUALITY/Q03-A04/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A04_RERUN95_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN95.json`。
- 下一卡 `Q03-A05` 只处理候选 D20 Fish clip arm excursion；改源 builder 后重新生成 candidate resource，并用 `RERUN96` 保留新的原生截图命名空间。

## 2026-09-11 Q03-A05 Fish arm envelope and RERUN96

- 候选 generic Fish clip 由 `D20A02AnimationBuilder` 重新生成，upperarm magnitude=`30°→8°`；实际曲线为左 `28.166→20.166`、右 `28.166→36.166`。增加 EditMode 曲线边界断言，候选 Resources controller 已重建。
- 验证：D20 EditMode `7/7`、Q03 candidate PlayMode `1/1`、全量 EditMode `620/620`、全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`（唯一 skip 为 D19 terrain work-scene 显式 smoke prerequisite）、Python `1152/1152`，recompile `failed=false/errors=[]`、error console `0`。
- RERUN96 真实 Win32：`48` 输入、`19` 截图、`167` observations、`120` swimming、`21` ship-control；船体位移=`6.058783m`、航向=`18.267059°`，业务/暂停矩阵成立；远船 reject 和 owner-not-ready 未覆盖。
- `view_image` 复核证明曲线修复生效但效果不足：游泳整体仍蜷曲/抬腿，登船/舵轮接触仍不可信，港口存在浮空/裁切/粗糙几何。`VC-G01` 保持 `IN_PROGRESS`，`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`。
- 证据：`game/tools/results/LOCAL-QUALITY/Q03-A05/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A05_FISH_POSE_RERUN96_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN96.json`。
- 下一卡 `Q03-A06` 处理 candidate Fish whole-body orientation/pose owner，以 RERUN97 保留新证据命名空间。

## 2026-09-11 Q03-A06 Fish whole-body pitch 与 RERUN97

### 有界变更

- 在 `D20A02AnimationBuilder` 生成的 candidate-only Fish clip 上增加 visual child root 的恒定 `32°` pitch 曲线；不写 D2 physical root，不修改正式动作资源或 Systems owner。
- 增加 Fish root curve EditMode 断言、Q03-A06 独立 Player 构建入口及 RERUN97 route probe 命名空间，避免覆盖 RERUN96。

### 证据与边界

- Pipeline recompile=`failed=false/errors=[]`；D20 EditMode=`8/8`、Q03 candidate PlayMode=`1/1`、Python=`6/6`；Editor=`ready/stopped`、error console=`0`。
- Player build job=`ac3663fd60524752998d91601300c7ca`，`MainMenu + InitialHarbor`、`BuildOptions.None`；Player SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- RERUN97 真实 Win32 route=`48` 输入、`19` 截图、`166` observations、`69` moving、`123` swimming、`21` ship-control；船体位移=`6.051456m`、航向变化=`18.186615°`；WASD/转向、E、I/M/T、Esc、业务、暂停恢复和 clean close 有证据。远船 reject 与 owner-not-ready 未覆盖。

### 视觉判定

检查首帧、登船、船控、入水、返岸和仓库帧后，确认 32° 曲线进入资源，但原生 Fish 仍身体前俯、腿部高抬并悬空；登船/舵轮接触仍不可信，港口仍存在粗糙/浮空/裁切几何。因此本轮为 `Q03-A06=PARTIAL`，不升级 `Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A06/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A06_FISH_ORIENTATION_RERUN97_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN97.json`。

### 下一步

`Q03-A07` 先用只读 runtime probe 记录 Fish 状态、candidate visual child/root rotation、关键骨骼 local rotation 与实际 clip/controller 播放状态，确定问题属于 clip 轴向、运行时覆盖还是 prefab 层级；诊断完成前不继续盲调 pitch/limb 角度。

## 2026-09-11 Q03-A07 Fish generic axis contract 与 RERUN100

### 根因与有界修复

Unity Pipeline Editor eval 证实 R02 `thigh_l/thigh_r` 的休息 local Euler Y/Z 为 `180/180`，而旧 generic clip 只写 `localEulerAnglesRaw.x`。Animator 状态切换会把未写入的 Y/Z 归零，失败测量中脚从约 `y=0.08` 抬到约 `y=1.68`，这解释了此前原生 Fish 的抬腿/蜷曲，而不是 D2 physical root 或输入问题。

本卡只修改 candidate generic animation contract：`D20A02AnimationBuilder` 设置 `writeDefaultValues=false`，为 root 写显式 XYZ，为 limb 写动画 X 与 R02 休息 Y/Z；candidate Fish visual child 保持 `90°` pitch。D2 physical root、Systems、正式动作资源和正式 InitialHarbor consumer 均未改变。增加 R02 rest-axis 回归、Fish 曲线回归，以及 RERUN99/RERUN100 独立 route namespace。

### 验证

- Pipeline recompile：`failed=false; errors=[]`；D20 EditMode `9/9`；Q03 candidate PlayMode `1/1`；Unity 全量 EditMode `622/622`（0 failed/0 skipped/0 inconclusive）；Python route contract `6/6`；controller audit `PASS`（15 states、每个 clip 16 条显式曲线）；Editor `ready/stopped`；error console `0`。全量 PlayMode/Python 未在本卡后重跑。
- RERUN99：`48` 输入、`19` 截图、`167` observations、`69` moving、`120` swimming、`21` ship-control；船体位移 `6.002009m`、航向变化 `18.224670°`。全轴修复去除了折叠/抬腿失败，但身体仍斜向。
- RERUN100：`48` 输入、`19` 截图、`166` observations、`67` moving、`109` swimming、`22` ship-control；船体位移 `5.819563m`、航向变化 `16.697662°`；真实移动、游泳、船控、业务 I/M/T、重复操作、Esc 关闭/恢复成立，`gameplay_ui_navigation_suppressed_observed=true`。远船 reject 与 owner-not-ready reject 未覆盖，保持 false。

### 视觉与状态边界

RERUN100 的 `90°` candidate Fish visual-child pitch 让身体真实读作横向，较 RERUN99 有改善；但入水/返岸仍有岸线穿插或悬浮读感，登船/舵轮接触弱，InitialHarbor/仓库仍有低模、浮空、裁切、前景侵入和粗糙几何。因此 `Q03-A07=PARTIAL`，不升级 `Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED`。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A07/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03_A07_FISH_PITCH90_RERUN100_20260906.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN100.json`、`game/tools/results/LOCAL-QUALITY/Q03-A07/Q03_A07_RUNTIME_FISH_POSE_PROBE.json`。

下一步停止继续盲调 Fish pitch，选择 `D31-P04`、`D31-S05` 或 `D31-G06` 的一个环境/接地 owner 做可复现诊断和最小修复；VC-G01 仍需视觉闭环与拒绝路径证据。

## 2026-09-11 Q03-A09 当前源码 Player 与独立 probe archive

### 本轮变更

本轮没有继续盲调角色角度，而是先修复真实路线取证的覆盖风险。`tools/vc_g01_runtime_input_probe.ps1` 现在在带 `EvidenceTag` 的路线中，将固定 RouteFlag 派生的 probe JSON 另存为唯一的 VC-G01 archive，并在 summary 写入 `probe_archive_path` 与 `probe_archive_exists`。这是测试工具改动，不调用玩法方法，也不改变 D2/D7/D19/D29/D10 owner、Systems、输入绑定或场景资源。

### 构建与真实 Player

- Unity Pipeline build=`build_cd0734bfe4bb`，`StandaloneWindows64`，场景为 `MainMenu + Bootstrap + InitialHarbor`，`BuildOptions.None`，`0 errors / 1 warning`。
- Player=`game/tools/results/LOCAL-QUALITY/Q03-A09/Q03_A09_CURRENT_PLAYER.exe`，SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。
- Assembly-CSharp SHA256=`A9072617F5B3432953027E93F43C8043787D04DDC74D62EE8CD8D693AC9E5064`。
- 结果：[Q03-A09 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q03-A09/RESULT.json)、[build result](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q03-A09/Q03_A09_BUILD_RESULT.json)。

### 真实运行证据

当前 Player 窗口为 `1920x1080`，WaterProbe 使用真实原生 Win32 键鼠事件，`49/49` 输入成功，19 张窗口截图，166 observations、69 moving、122 swimming、21 ship-control；船体最大位移=`5.997463m`，航向变化=`18.239227°`。InitialHarbor、WASD/转向、E 登船/下船、I/M/T owner-ready 页面、重复 I/M/T、Esc 关闭/恢复、业务日志和优雅退出成立。

权威文件：

- [runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.json)
- [独立 probe archive](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_ROUTE_PROBE_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.json)
- [Player log](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_PLAYER_RUNTIME_INPUT_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.log)
- 截图目录：`game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q03a09_current_a08_archivefix_20260906/`

远船 `E` 拒绝和 I/M/T owner-not-ready 启动窗口不在本次 WaterProbe 中，保持未证明；summary 的 `far_ship_reject_proof=false` 不是失败被隐藏，而是本次序列的明确边界。

### 视觉审查与状态

首帧、船控、游泳、仓库、市场和暂停截图已复核。真实逻辑路线成立，但角色仍为低模程序化 fallback，游泳读感为横向悬浮/四肢下垂，人物与舵轮接触弱，港口岸线和地面几何粗糙。因此本卡为 `Q03-A09=PARTIAL`，`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 不变；D31-P04/S05/G06 继续 `PARTIAL`。

### 验证与交接

- PowerShell 探针解析=`PASS`。
- `python -m unittest discover -s tools/tests -p 'test_vc_g01_runtime_probe.py' -v`=`6/6 PASS`。
- Player 已优雅退出且无残留进程；Unity Editor=`ready / playMode=stopped`，error console=`0`。
- 下一卡必须选择一个单一视觉 owner：正式 Avatar/动作资源、泳姿表现或港口几何/接地；不得把本轮输入路线通过扩大为视觉门禁通过。

## 2026-09-11 Q03-A10 fallback 游泳手臂轴向复核

Q03-A10 针对当前 fallback 的一个可复现 Transform 组合根因做了最小 D2 表现修改：visualRoot 的 `64°` X 轴倾斜后，中性上臂局部 `-Y` 与旧 `-72°/-18°` 组合仍导致手臂下垂。现在仅对完整程序化 fallback 使用上臂 `150° ± stroke`、前臂 `30° ± compensation`；导入蒙皮保持 neutral，D2 physical root、Systems、正式 Animator 和 InitialHarbor consumer 不变。

- 结果：[Q03-A10 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q03-A10/RESULT.json)；构建：[Q03-A10 build](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q03-A10/Q03_A10_BUILD_RESULT.json)。
- `build_b4b4570bffeb`：`StandaloneWindows64`、`MainMenu + Bootstrap + InitialHarbor`、`0 errors / 8 warnings`；Assembly-CSharp SHA=`ECBE9AB97B10351EE5FBF6A48C773BCDB2F87FC97FFE74666882E1A9DD050409`。
- 独立 RERUN100 archive：[runtime summary](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03A10_SWIM_ARM_AXIS_R2_20260906.json)、[probe archive](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_ROUTE_PROBE_Q03A10_SWIM_ARM_AXIS_R2_20260906.json)，真实 Win32 `49/49` 输入、19 截图、164 observations、69 moving、120 swimming、21 Ship control；业务/重复 I-M-T/Esc/暂停/clean close 成立。
- 游泳截图：[Q03-A10 swimming frame](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/runtime_input_q03a10_swim_arm_axis_r2_20260906/20260911_213014_swimming_water_route.png)。截图确认手臂由下垂改善为朝头/躯干方向前伸，但低模比例、腿脚、自然泳姿、舵轮接触和港口/岸线几何仍不合格。

因此 `Q03-A10=PARTIAL`，VC-G01 继续 `IN_PROGRESS`；`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 不变。远船拒绝和 owner-not-ready I/M/T reject 不在本轮路线中，不能把输入闭环扩大为完整视觉 PASS。下一步选择一个新的单一视觉 owner，停止继续盲调游泳角度。

## 2026-09-12 Pipeline R3 hidden Player 输入归属复核

本轮用 Unity Pipeline runtime control surface 补做了一次不抢占用户桌面的 Player 验证。Development Player `Q29_RUNTIME_PIPELINE_QA_R3.exe` 以隐藏 GUI 窗口运行，runtime server=`127.0.0.1:7900`；MainMenu Continue 通过 `simulate_pointer` down/wait/up 进入 `InitialHarbor`，键盘通过 `simulate_key` down/wait/up 产生真实 Player Input System 事件，PID `57180` 由 runtime `quit` 退出。

### 真实发现

R2 的 runtime state、D3 snapshot、D10 focus 和 Player log 同时证明：`T` 被 D29 酒馆与 D3 出航两个 owner 消费。一次 T 会看到酒馆打开，但 D3 也进入 `Sailing`；之后按 M 的“航行中不能打开市场”是 D3 owner 的正确拒绝。这个缺陷此前的源码测试只测了各 owner 的独立入口，没有测跨 owner 同键竞争。

### 修复

- `D29OpenTavern` 保持 `T`。
- `D3StartVoyage` 默认键改为 `Y`，并同步 D3 fallback、按钮标签、HUD hint、Harbor tutorial、D5 hints。
- 新增 `Save.Tests.D16AccessibilityTests.D29TavernAndD3VoyageHaveDistinctPrimaryKeyboardBindings`。

### R3 证据

- T：`d10Focus=true`、`trade=false`、`voyage=InPort`、`primary=false`；T 不再启动 D3 航程。
- M：关闭酒馆后 `trade=true`、`voyage=InPort`；市场打开成功。
- Y：关闭市场后 `voyage=Sailing`；D3 仍能由独立键正常出航。
- W：1000ms 持续输入让玩家位置 `(0.000,0.195,-2.700)`→`(0.195,0.162,-4.868)`，速度 `3.200`，松键后速度为 `0`。
- E：先 `boarded/mode=Ship`，再 `disembarked/mode=Character`。
- I：仓库 `D29WarehousePanel` 打开并取得临时页焦点；Esc 关闭后主输入恢复；再按 Esc 为 `TimeScale=0`，再次按 Esc 为 `TimeScale=1`。
- 回归：D16Accessibility `8/8`、D16InputRouter `10/10`、D29 Q15 `1/1`、W/S HUD navigation `1/1`；recompile `failed=false/errors=[]`；runtime error console `0`；Player log 无 `ERROR/Exception/Crash/Fatal`。

### 边界和交接

R3 是 Pipeline-enabled Development QA Player，不是发布构建；没有前台截图、物理键盘或人工视觉观察。因此本轮只登记为 `PASS_WITHIN_SCOPE` 的输入归属/运行时状态证据，不升级 VC-G01。Player 警告仍明确正式 Avatar/动作资源未准入、当前为 procedural fallback；低模角色、任务动作/泳姿、握舵接触、树木/植被接地和港口几何风险仍开放。

已恢复安全配置：Editor `developmentBuild=false`；`RuntimePipelineConfig.enableInBuilds=false`；Player 已退出、无残留 PID。权威结果：[VC-G01 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)、[Q29 RESULT](C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q29/RESULT.json)、[Q29 handoff](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。

下一步：保持 `VC-G01=IN_PROGRESS`，选择一个单一视觉 owner 做下一次 bounded Player 修复和证据；Pipeline hidden Player 可继续用于输入归属、状态、日志和错误路径回归，不能替代最终人工视觉门禁。

## 2026-09-12 D7 helm anchor 与 D27 NPC shader 增量

R3 camera evidence 已登记到 `game/tools/results/LOCAL-QUALITY/Q26/Q26_D7_HELM_ANCHOR_REVALIDATION_PIPELINE_20260912_RESULT.json`：D7 读取 D19 authored visible helm anchor 后，Q26/A05 的左右手到舵轮距离为 `0.2608m`；formal/normal hidden Player 路线的状态与错误控制面均可复现，GPU frames 仍显示 formal R02/T-pose-like、程序化低模和港口几何问题。VC-G01 不升级。

随后 D27 owner 修复了 D20 NPC 的 URP shader 缺陷。16 个源材质通过 builder 迁移到 `Universal Render Pipeline/Lit`；最新 hidden Player 的运行时盘点为 `123/123` NPC material slots 使用 URP Lit，`Standard=0`、`InternalErrorShader=0`，runtime error console=`0`，close NPC GPU frame 无 magenta。该增量只关闭 NPC 材质错误，不关闭 NPC 几何、正式角色动作、游泳姿态、接地或最终 D31 视觉门。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_D27_NPC_URP_MATERIAL_REPAIR_PIPELINE_20260912_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。状态保持 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。

## 2026-09-12 Q03-A14 R4 generic arm pose continuation

本轮沿用 Unity Pipeline hidden GUI Player 控制面，针对上一轮 formal R02 close frame 中的横向 T-Pose-like 普通动作做了一个单一 presentation owner 修复。`D20PlayerAnimationPresentationAdapter` 对 generic-path placeholder 的 Idle/Walk/Run/常见交互/Board/Disembark 等状态只覆盖当前上臂 Z 轴，并以每帧一次、暂停跳过的方式避免累积；Fish 和 Helm 明确不在本轮修复范围内。D2 physical root、Systems 和资源所有权没有改变。

### 证据

- D20A02=`10/10 PASS`；Q03 formal=`1/1 PASS`；Q26=`12/12 PASS`；D17=`16/16 PASS`；recompile=`failed=false/errors=[]`。
- Exact build：`build_9918fec2b2fc`，`Q03_A14_FORMAL_PIPELINE_QA_R4_ARM_POSE_DEV2.exe`，`0 errors / 0 warnings`，Assembly-CSharp SHA=`5CEEAA4EB7654C77D1545C4EBF5C0109D2BD892B991BBB0896676A8E76E5E5A7`。
- Hidden runtime PID=`70652`、port=`7900`：MainMenu pointer click→InitialHarbor；formal bridge `installed=true/generic=true`；Idle 与 W→Walk 时双手 Y 均低于上臂；E→`Ship/Helm` 时手与上臂仍在约同一 Y，说明 Helm/IK 是下一独立 owner；runtime error console=`0`；clean close。
- GPU camera：[R4 formal close frame](../../../../../../game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_PIPELINE_QA_R4_ARM_POSE_DEV2_PLAYER_CLOSE_CAMERA_20260912.png)，SHA=`095D9F51FF4FE782C1A916F802B939CF4B0A003612379272E5EEEA1EB60ED785`。可见普通状态手臂已下垂，但 formal 玩家仍暗、港口仍粗糙。

### 状态边界

该证据只登记 `PASS_WITHIN_SCOPE`，不升级 `VC-G01=IN_PROGRESS`。`Q03=BLOCKED_ASSET`、`Q26=PARTIAL`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 保持。它不能证明 authored action/avatar/material readiness、自然泳姿、握舵 IK、树木/地面接地、穿模消失或最终人工视觉验收。源码安全配置已恢复：`RuntimePipelineConfig.enableInBuilds=false`、Editor `developmentBuild=false`，无残留 Player。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R04_ARM_POSE_PIPELINE_20260912_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q03-A14 R6：open_sea 存档与港口路线有界修复

本轮用 Unity Pipeline hidden GUI Player 继续处理 VC-G01 的真实运行主线。先前复现到已有 `open_sea` 存档重新打开 `InitialHarbor` 时，D2 船初始化会把海上坐标应用到港口船体，导致船壳覆盖玩家出生点到水域的直线路径。修复限定在 `D2ShipController`：`InitialHarbor` 保留 D19 作者泊位/船向，非港口场景仍应用 open-sea snapshot；新增 `Q26A13OpenSeaSaveRestoresAuthoredHarborShipRoute` 覆盖该存档边界并恢复静态状态。

### 证据

- R6 build=`build_997e00e6bc28`，`StandaloneWindows64`，`MainMenu + Bootstrap + InitialHarbor`，`0 errors / 7` 条既有弃用 warning；实际产物为 `game/Builds/StandaloneWindows64/WhalesIslandReef.exe`。
- Q26=`13/13 PASS`，D17=`16/16 PASS`，recompile=`failed=false/errors=[]`，Editor=`ready/stopped`，error console=`0`。
- hidden Player PID=`46228` 的 focused route：进入 InitialHarbor 后 `D19_R03_ShipVisualInstance z=-3.60/yaw=180`，船壳 `bounds.max.z=0.00`，玩家出生点 `z=2.50`；S 真实输入到 `z=11.38`，随后 W 路线进入 `x=-1.45,z=18.70`、D2 `_isSwimming=true`、Action=`Swimming`；runtime error console=`0`，clean close。
- Player log 已归档到 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_PIPELINE_QA_R6_OPEN_SEA_ROUTE_PLAYER.log`，SHA256=`2B929A42BC08C86EB60DB2C4086249404EA5A86FC176D9B92829C926730F6F4E`。

### 控制面和视觉边界

一次随后 hidden relaunch 的 `simulate_key` 虽返回 Success，但 D7 snapshot 明确为 `Modal=FocusLost / CharacterInputAllowed=false`，因此没有把那次零位移记成玩法证据。Unity Pipeline 目前没有可靠的 foreground/focus command；控制面必须把“注入成功”和“玩法 owner 消费成功”分开。R6 没有新增截图，且泳姿 probe 仍有 `SwimmingVisualPitchDegrees=64`、`SwimmingVisualBodyDrop≈-0.607`。本轮只关闭“旧海上存档覆盖港口船根并堵住正向路线”这一具体缺陷，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`；formal Avatar/动作、自然泳姿、握舵接触、树木/地面接地、穿模、完整输入矩阵和最终人工视觉门禁仍未通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R06_OPEN_SEA_ROUTE_PIPELINE_20260912_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q26-A14 D31-P04 接地增量

本轮使用 Unity Pipeline connected Editor 做了一次真实 PlayMode 运行时诊断，发现并修复了 VC-G01 所暴露的一个环境物理缺陷。Unity 静态合批后，D19 可见地形的 `MeshFilter.sharedMesh` 变成 `Combined Mesh (root: scene) 4`；D6 原逻辑把它重新写入 `D19_TerrainRoot`，造成 `ShoreRock_0` 的碰撞地面比可见底部高 `0.290m`。修复后 D6 保留场景作者的 `D19_TerrainRoot` MeshCollider 网格，只同步 owner Transform。

验证证据：

- 修复后新鲜 PlayMode terrain mesh=`D19_R03_IslandSurfaceMesh`；9 个 `ShoreRock_*` 接地误差为 `0.018–0.022m`，最大 `0.022m`。
- `Q26A14ShorelineRocksHaveOwnedTerrainContact`=`1/1 PASS`；完整 `D19R03Q26GameplayDefectPlayModeTests`=`14/14 PASS`。
- Player build=`build_0cc514c48042`，`StandaloneWindows64 / 0 errors / 8 warnings`，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；Editor 最终 `ready/stopped`，error console=`0`。

这不是 VC-G01 完成证据：本卡没有启动 native Player、没有前台键盘输入、没有新增截图，也没有把接地回归扩大为完整视觉通过。正式 Avatar/动作、自然泳姿、握舵接触、真实窗口完整输入矩阵、最终画面质量及人工验收仍开放。`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q26/D31-P04/S05/G06=PARTIAL` 保持。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_PIPELINE_20260912_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。下一步继续使用隔离 Player 控制面推进真实交互闭环，或选择新的单一 owner 缺陷；不要把 Pipeline PlayMode 证据当作视觉门禁替代品。

## 2026-09-12 Q31 Pipeline hidden Player runtime recheck

使用当前源码构建的隐藏 GUI Development Player 已通过 Unity Pipeline runtime server 完成一轮有界控制面复核。该运行没有接管用户桌面：Player 未使用 `-batchmode`/`-nographics`，但保持 hidden；MainMenu 通过 Pipeline pointer 进入 InitialHarbor，随后由 `simulate_key` 发送 W/S/A/E/I/M/T/Esc。

### 证据

- Build：`build_b5d99f79b224`，`StandaloneWindows64`，`0 errors / 7 existing deprecation warnings`；EXE SHA256=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`，Assembly-CSharp SHA256=`B1AB1C5806E8762BD15254C6D80DB5D6D89B083256348DE560F5A53DDAE6F83`。
- W/S/A：实际位置、移动状态、速度和 yaw 发生变化；UI `sendNavigationEvents=false` 且没有 selected UI object。
- 水域：Player log 有 `surfaceY=-0.02` 的 entry/exit；E 完成 Character↔Ship；I/T/Esc 的 modal、重复、关闭和暂停恢复状态均有 owner 证据。
- Runtime error console=`0`，runtime quit clean，descriptor 移除；Editor 最终 `ready/stopped`、error console=`0`。

### 不能升级的边界

本轮 `PASS_WITHIN_SCOPE` 只覆盖 hidden Player 的 Pipeline 控制面与有限 owner 状态，不覆盖物理前台键盘、用户视角截图、正式动作/Avatar、自然泳姿、握舵接触、植被接地/穿模、用户 GPU 性能或最终人工验收。初始存档已经是 `Sailing`，M 因而只得到合法拒绝；市场成功没有在本轮伪造或宣称。

VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，Q31 继续 `PARTIAL`，D31-P04/S05/G06 继续 `PARTIAL`。恢复项已确认：`RuntimePipelineConfig.enableInBuilds=false`、Editor `developmentBuild=false`、无残留 Player。权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_RUNTIME_RECHECK_20260912_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q31 Pipeline RenderTexture 相机诊断

在上述当前候选上继续使用 Unity Pipeline runtime 生成四张 hidden Player 相机证据：InitialHarbor、登船船上、游泳路线、舵位近景。舵位近景与 runtime owner probe 一致：`HelmWheel` 和 `HelmStand` 为真实可见对象，玩家 parent=`ph_ship_fishing`，左右 fallback hand 到 wheel 距离均为 `0.2608m`；targeted `Q26A05ProceduralFallbackReachesVisibleHelmInShipMode`=`1/1 PASS`，Editor=`ready/stopped`，console errors=`0`。

这轮没有把相机帧扩大解释为视觉 PASS。帧仍显示低模/占位角色、粗糙场景层级和未定稿泳姿；游泳默认帧不足以作自然泳姿判定。该卡只补上“能否从 Player 画面直接观察舵位”的证据缺口，不能关闭用户已经报告的动作扭曲、游泳观感、控制语义、植被接地或穿模问题。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。

权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_CAMERA_DIAGNOSTIC_20260912_RESULT.json`；四张 PNG 已在结果 JSON 登记 SHA256。恢复项保持 `RuntimePipelineConfig.enableInBuilds=false`、Editor development build 关闭、无残留 Player。

## 2026-09-12 Q20 Player 旧 palm Renderer containment 复核

为了回应旧版试玩中的“树在空中飘”反馈，本轮用当前候选 hidden Player runtime 查询 `ph_palm_tree`，不把 Editor 静态对象名当成画面结论：`InitialHarbor` 中 root 仍 active，但观察到的 `25` 个子 Renderer 全部 `enabled=false`；D26 运行日志为 `vegetation_grounded=26`、`vegetation_hidden_without_terrain=0`，runtime error console=`0`，Player clean close。

该结果只说明当前候选已经隔离旧 palm Renderer；正式棕榈/树资源仍未通过 LocalDevelopmentReady，所有镜头角度的接地/遮挡/穿模仍未关闭，所以 Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS` 不变。权威结果：`game/tools/results/LOCAL-QUALITY/Q20/Q20_PIPELINE_PLAYER_TREE_CONTAINMENT_20260912_RESULT.json`。

## 2026-09-12 Q03-A14 skin owner bounded continuation

本轮通过 Unity Pipeline hidden GUI Player 完成了 D20 presentation skin 的真实运行闭环，且没有接管用户桌面。D20 Builder/bridge 将项目已有的 URP Lit `D20_PlayerSkin_Warm` 绑定到 Candidate/Formal 的 `SuperHero_Male` body；D5/D7 通过只读 `IsD20PresentationOwned` guard 跳过该 renderer，消除了运行时多 writer 覆盖。

证据：D20A02=`10/10`、Q03D20=`3/3`、D19R03Q26=`14/14`、D17=`16/16`；recompile=`failed=false/errors=[]`。`Q03_A14_D20_SKIN_REPAIR_R4.exe` 的 EXE SHA256=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`，Assembly-CSharp SHA256=`8E5D2AA5D1FF42AABA652617D537848F4BF1F5757801673F5FA6BD1510EF59B4`。Candidate/Formal hidden runtime 都取得 `D20_PlayerSkin_Warm / Universal Render Pipeline/Lit / RGBA(0.720,0.420,0.250,1.000)`，W 到达 `Walk`，Formal E 到达 `ph_ship_fishing`，两次 runtime console errors=`0` 且 clean close；四张 RenderTexture 图已登记在独立结果 JSON。

这仍不是 VC-G01 视觉通过：Candidate 服装仍粗糙，Formal 近景虽然不再是黑色剪影但缺完整 outfit，Helm 仍呈舵轮上方的 T-pose-like/高度错位；自然泳姿、树木/地面接地、穿模、完整物理键盘矩阵、用户 GPU 性能和人工最终视觉仍未关闭。故 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q03=BLOCKED_ASSET`、`Q03-A14/Q26/Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL` 保持。

权威结果：[Q03-A14 skin owner](../../../../game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_SKIN_OWNER_PIPELINE_R4_20260912_RESULT.json)、[Q03-A14 parent result](../../../../game/tools/results/LOCAL-QUALITY/Q03-A14/RESULT.json)。下一步仍按单一 owner 选择 formal 完整 outfit/action 资源或 Helm geometry/IK/scale 修复；不得把本卡升级为 VC-G01 PASS。

## 2026-09-12 Q03-A14 R1 Helm pose bounded continuation

本轮使用 `com.unity.pipeline@0.6.0-exp.1` 的 hidden GUI Player 继续 VC-G01，补齐当前 generic formal consumer 的 Helm 状态证据。presentation adapter 对 generic-path `Helm` 增加一次/帧的上臂安全姿态，仍不写 D2 physical root；D20A02=`10/10`、Q03D20=`3/3`、D19R03Q26=`14/14`、D17=`16/16`，recompile=`failed=false/errors=[]`，Editor=`ready/stopped`，error console=`0`。

Exact Player 为 `build_67ffef5d580f` / `Q03_A14_D20_HELM_POSE_PIPELINE_R1.exe`，`0 errors / 8 warnings`。PID=`73608` 通过 Pipeline pointer confirmation 进入 InitialHarbor，`E` 后 `state=Helm`、`parent=ph_ship_fishing`；runtime error console=`0`，clean close。GPU RenderTexture 三帧和 SHA256 见 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_D20_HELM_POSE_PIPELINE_R1_20260912_RESULT.json`。

结论必须保持为有界结果：前方横向 T-pose-like 有所缓解，双手已经低于上臂，但两手仍位于舵轮上方而非自然握舵位置；formal outfit/action/avatar、泳姿、植被接地、穿模、物理前台键盘、用户 GPU 性能和人工最终画面仍未通过。因此 `VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`；下一步只能选择 formal authored action/avatar/IK 或单独 Helm geometry/scale/IK owner。
## 2026-09-12 Q03-A14 R2 Fish pose bounded continuation

Fish 作为独立 presentation owner 继续推进：generic-path Fish 现在每帧一次收窄上臂 bind-pose 轮廓，仍不写 D2 physical root，仍不声称自然划水。当前回归为 D20A02=`10/10`、Q03D20=`3/3`、D19R03Q26=`14/14`、D17=`16/16`，recompile 无错误，Editor=`ready/stopped`、error console=`0`。

Exact Player=`build_35b48f947b48` / `Q03_A14_D20_FISH_POSE_PIPELINE_R2.exe`。一次隐藏路线因 D7 `FocusLost/CharacterInputAllowed=false` 被排除；组件级 exact Player probe 得到 `state=Fish`、`fishCorrectionApplied=True`、root pitch=`90°`，RenderTexture frame 与 SHA256 已登记在 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_D20_FISH_POSE_PIPELINE_R2_20260912_RESULT.json`。

这只证明 generic arm spread 的有界缓解，不证明自然 stroke、持续输入、浮力/接地、formal action/avatar 或最终视觉。`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q03=BLOCKED_ASSET`、`Q03-A14/Q26/Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL` 保持。下一步选择正式 swim/action/avatar owner 或独立 swim-cycle asset。

## 2026-09-12 Q26-A17 D19-R03 生成舵轮高度对齐

VC-G01 继续推进一个可测量的 presentation owner 缺陷。Q31 近景测量显示生成 `HelmWheel.y=1.72m`，fallback 双手中线 `y=1.894419045m`，垂直误差 `0.174419045m`，与用户反馈的“控制物体语义不对”一致。D19-R03 builder 现在生成 `HelmStand.height=0.82m`、`HelmWheel/HelmWheelHub.y=1.89m`；D2/D7 gameplay anchor、船物理、Systems、AgentCore 未修改。

证据：`Q26A17GeneratedHelmWheelSharesFallbackHandHeight=1/1 PASS`，日志 `generatedHelmWheelY=1.8900`、`handMidpointY=1.8944`、`verticalError=0.0044`；完整 `D19R03Q26GameplayDefectPlayModeTests=16/16 PASS`；`build_cfa950c379f9`=`0 errors / 8 warnings`；Editor=`ready/stopped`、error console=`0`。结构化结果：[Q26-A17](../../../../game/tools/results/LOCAL-QUALITY/Q26-A17/RESULT.json)。

边界：Editor-only RenderTexture 近景尝试为均匀灰帧，已明确登记为 `INVALID_GRAY_FRAME`，不计入视觉证据；本卡没有前台物理 Win32 键盘和人工最终画面验收。因此 `VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q26/Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET` 保持。下一步优先 formal Avatar/action admission 或已验证的 hidden Player runtime camera surface，不再无新测量盲调舵轮坐标。

## 2026-09-13 Q03-A16 Formal Humanoid pose adapter与R2 Player运行交接

本轮沿用“单一 Presentation owner + connected Unity Pipeline + hidden GUI Player + 不扩大 PASS”的边界，处理当前 formal R02 的动作/游泳/握舵表现缺口。新增 `-d20FormalHumanoid` opt-in 路径，使用 R02 valid Humanoid Avatar 的 `HumanPoseHandler`；不改变默认 Player，不把 generic action clips 晋级为 authored Humanoid，不写 D2 physical root。

### 证据

- 新 Humanoid PlayMode 契约=`1/1 PASS`；Q03D20=`4/4 PASS`；Q03Formal=`3/3 PASS`；recompile 无错误。
- R2 Player：`build_d87c4e4b5265`，`StandaloneWindows64`，`0 errors / 2 warnings`；EXE SHA=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`，Assembly-CSharp SHA=`69345D2453AF034D9A92374F6848E92752C0D02CE37FAA737EF0A5AA09DA3921`。
- 隐藏 Player PID=`86584` 由 Pipeline pointer 进入 `InitialHarbor`。启动 probe 确认 `avatarValid=true`、`avatarHuman=true`、`adapterReady=true`、`animatorEnabled=false`；W/S 路线确认 `Swimming=True / Fish / apply=True / pitch=70`。
- 正常 E 登船后 `parent=ph_ship_fishing`、`poseState=Helm`；当前左右手到 `HelmWheel` 距离=`0.219m / 0.222m`。R2 舵位与游泳 PNG、Player log 和 hash 见独立结果 JSON。
- Player 通过 runtime `quit` 退出，descriptor/PID 清除；Editor 最终 `ready/stopped`；RuntimePipeline 恢复 `enableInBuilds=false / port=0`。

### 状态边界

该结果只登记 `Q03-A16=PASS_WITHIN_SCOPE`。当前 formal controller 的 15 个 clip 仍 `humanMotion=0/genericRoot=15`，因此 `Q03=BLOCKED_ASSET`。R2 RenderTexture 画面仍显示低保真角色和粗糙港口；游泳帧是在已确认 `Swimming=True` 的活动水域状态下取样，证明姿态提交但不证明自然划水、完整输入路线截图、浮力/接地或最终人工视觉。舵轮距离数据不等价于自然 IK。

Player log 保留 1 条工具层 Error：Agent 误调用不存在的 `get_console_logs` runtime 命令；无 Exception/Crash/Fatal。该边界已进入 `Q03_A16_FORMAL_HUMANOID_PIPELINE_R2_RESULT.json`，没有伪造为零错误。

正式状态保持：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31/Q26=PARTIAL`、`D31-P04/S05/G06=PARTIAL`。下一步选择正式 outfit/action 或 swim/helm 的另一个有测量证据的 owner；不再用隐藏 Player 画面替代人工最终视觉门禁。

## 2026-09-13 Q31 D2 water-overhead guard 与 R2 hidden Player 证据

本轮以 D2 为唯一生产 owner，修复了一个由 hidden Player 真实坐标复现的水下地形放行缺陷。D2 现在从游泳体积上方向下查找可行走顶面，兼容 D19 单面 MeshCollider；若水面与可行走地形重叠，则退出游泳并从上方恢复到地面。没有为 D19 表现房屋或装饰添加未经批准的碰撞体。

- 当前源码 recompile=`failed=false/errorCount=0`；D17A03/A04/A07/A08/A14/A15/A16/整合/A17=`9/9 PASS`。
- R2 Player build=`build_33970c8639b5`，`0 errors / 7 warnings`；前后运行态、PNG、日志和 SHA256 详见 [Q31 当前结果](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)。
- 用户报告坐标从修复前 `swim=true/grounded=false/y=0.400` 变为修复后 `swim=false/grounded=true/y=1.003`；开放海域 `W` 产生 `speed=2.073093` 且保持 `Swimming`。
- runtime error console=`0`，Player log 大写 `ERROR/Exception/Crash/Fatal`=`0`，Player clean close；QA 后 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`，无残留 PID/descriptor。

该结果只登记为 `PASS_WITHIN_SCOPE`。R2 重叠恢复帧仍有近墙/遮挡且看不到玩家，开放水域画面仍为低模/程序化角色；正式动作/Avatar、自然 swim cycle、自然握舵、全镜头树木接地、穿模、物理前台 Win32 输入、目标设备性能和人工最终视觉仍未通过。`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 不变；Q31/Q26/D31-P04/S05/G06 继续 `PARTIAL`，Q03 继续 `BLOCKED_ASSET`，Q20 继续 `PARTIAL/BLOCKED_ASSET`。

权威结果：[Q31 当前 Pipeline 复核](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；[VC-G01 主结果](../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。下一步继续选择一个单一 owner 的真实缺陷做契约测试和 bounded Player 复核，不跳过 VC-G01 的最终视觉门禁。

## 2026-09-13 Q07 R03 服务建筑碰撞 owner 修复

### 诊断与变更

Q31 相机/遮挡诊断确认，`MarketVisual`、`WarehouseVisual`、`TavernVisual`、`ShipyardVisual` 各有 `37 Renderer` 但没有 Collider；现有 `CameraFollow` 的非 Trigger `SphereCast` 因而无法感知建筑可见体积。按单一写者原则，本轮只修改 `HarborCollisionController`，从四个 `Body` Renderer bounds 生成外扩 `0.08m` 的 solid BoxCollider，并统一放在 `D6_HarborCollision/R03BuildingCollision.<Name>` 下。R03 Presentation Prefab 仍 render-only，D2、Systems、AgentCore 未改。

### 证据

- Q07 定向 PlayMode：`R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries=1/1 PASS`。
- 受影响回归：`D17ActionQualityPlayModeTests=16/16 PASS`，`D19R03Q26GameplayDefectPlayModeTests=16/16 PASS`，recompile `failed=false/errorCount=0`。
- 当前源码 Windows Player：`build_c3588a5c004b`，`StandaloneWindows64`，显式 `Development`，`0 errors / 7 warnings`；EXE SHA=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`；Assembly-CSharp SHA=`970F91C0ECF6910C3423762C5591F93F4CC8549CBCE852A48C47D636F0CA5ABB`。
- hidden Player 无前台接管：MainMenu 通过 Pipeline pointer 进入 `InitialHarbor`；四个 building collider 均由 D6 持有、enabled、non-trigger，四条外部 raycast 均命中对应 collider；runtime error console=`0`；Player.log 严格 `ERROR/Exception/Crash/Fatal=0`；runtime `quit` clean close。

### 状态边界

该结果只关闭“服务建筑没有 Harbor 碰撞 owner”这一明确根因，不能扩大为最终相机构图、门口/路线可达性、正式动作/Avatar、自然泳姿、自然握舵、树木接地、穿模、物理前台键鼠、目标设备性能或人工视觉通过。Q07 保持 `VERIFIED`（扩展 owner 范围），Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`。

权威结果：[Q07 当前建筑碰撞结果](../../../../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_BUILDING_COLLISION_PIPELINE_20260913_RESULT.json)；[Q07 交接](../LOCAL-QUALITY/Q07/Q07.md)。

## 2026-09-13 Q30 current-source independent review rerun2

- 当前源码定向 Python=`65/65 PASS`；Unity Pipeline D16Accessibility=`8/8`、D16InputRouter=`10/10`、D17ActionQuality=`16/16`、VcG01WorldLogic=`14/14`；Q07=`1/1`；Q15/Q16/Q17 各=`1/1`。
- Q28 R6 构建成功；Q29 R6 hidden Player 完成 MainMenu、W/模态输入隔离、I/M/T、E、Y、航行中拒绝、Ship W/A、Esc 暂停恢复和 Q07 collider probe；runtime error query=`0`，clean close，配置恢复。
- Q30 仍 `PARTIAL`。Q29 R6 没有有效 compositor-visible screenshot 或前台物理键盘证据；formal action/avatar、自然泳姿/握舵、树木接地/穿模、目标设备性能和最终视觉门禁仍未关闭。VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。

权威结果：[Q30 rerun2](../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)；命令捕获：[Pipeline capture](../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_PIPELINE_CAPTURE.log)。下一步为 Q31 最终 native Player 视觉门禁；没有直接证据不升级 VC-G01。

## 2026-09-13 Q31 当前源码 RenderTexture 诊断 rerun2

Unity Pipeline 已经能在不接管用户桌面的情况下，从 hidden GUI Player 导出有效的当前源码画面。Q29 R6 Player 通过 pointer/key 控制进入 `InitialHarbor` 和开放水域，`S` 路线读到 `swim=true`、`grounded=false`，并生成三张非黑 `1280x720` RenderTexture PNG；传统 ScreenCapture 路径的黑帧已明确排除。完整运行态、PNG 大小/hash、失败路径和边界见 [Q31 visual diagnostic result](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_RESULT.json)。

这解决的是“插件是否具备当前画面诊断能力”，不是视觉验收：有效帧仍显示低模/程序化港口与泳姿，移动状态可见但不证明自然 authored cycle；重叠恢复帧仍存在近墙/玩家不可见的构图问题。capture-only 运行没有重复做 error-console 查询，未伪造为零错误；同一 R6 路线既有 `runtime error console=0` 证据保持独立。VC-G01 仍 `IN_PROGRESS`，VC-G02/VC-G03 仍 `LOCKED`，下一步按单一 owner 处理 formal action/avatar、自然 swim cycle 或 Helm IK/语义。

## 2026-09-13：Q31 生成舵轮暖木对比与 clean Player 复核

本轮继续 VC-G01 的有限缺陷闭环，针对用户反馈的“上船人物在控制瓢”先处理一个 presentation owner 根因：D19-R03 生成 `HelmWheel` 原为近黑蓝灰材质，近景下与船体/阴影混成厚重遮挡环。`D19R03HarborPresentationBuilder` 现在生成低光泽暖木 `RGBA(0.340,0.160,0.055,1)`；没有改 D2 玩家物理、D7 船控、Systems 或 AgentCore。

证据：`Q26A18=1/1`、`D19R03Q26=17/17`、`D17ActionQuality=16/16`、`VcG01WorldLogic=14/14`；recompile 无错误；`build_467144669770`=`StandaloneWindows64 / Succeeded / 0 errors / 0 warnings`。clean hidden GUI Player 通过 Pipeline pointer NewGame/Confirm、`E down/wait/up` 真实进入 `InitialHarbor` 并挂到 `ph_ship_fishing`；运行时材质为 `D7Polish_HelmWheel_0 RGBA(0.340,0.160,0.055,1.000)`，双手位于 `(-0.19,1.89,-6.33)/(0.19,1.89,-6.33)`，错误查询返回 `0`，Player clean close，QA 设置已恢复。

诊断帧：[Q31_QA_HELM_WARM_WOOD_BOARDING_FORWARD_RT_20260913_R2.png](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_QA_HELM_WARM_WOOD_BOARDING_FORWARD_RT_20260913_R2.png)，829168 bytes，SHA256=`A9B02BED4977AAEA721D3B65DD902E26E5905A4C547DD192DEBD109B4162A7B2`。它直接显示暖木舵轮、角色和 fallback 手的空间关系，但属于 hidden Player offscreen RenderTexture，不是 compositor-visible 人工验收帧。

结论：本卡关闭的是“舵轮颜色导致控制对象语义模糊”的有限表现问题，不等于自然 IK/握舵、正式动作/Avatar、自然泳姿、植被接地、穿模、前台物理键盘、目标设备性能或最终视觉通过。第一次探查中的无效 `status` 命令造成的工具层 Error 已被排除，clean relaunch 的错误边界单独登记。`VC-G01=IN_PROGRESS`、`Q31/Q26/D31-P04/S05/G06=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持。

权威结果：[Q31 helm presentation](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_RESULT.json)；[Pipeline capture](../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_PIPELINE_CAPTURE.log)。

## 2026-09-14 Q29 当前源码 Pipeline R7 隔离 Player 交互复核

Q29 使用当前源码重新构建 `Q29_RUNTIME_PIPELINE_QA_R7.exe`，临时启用 Runtime Pipeline build bridge，通过隐藏 GUI Player 的 loopback endpoint 完成一条可复现的控制面路线。MainMenu 的 NewGame/Confirm 采用 `simulate_pointer move/down/wait/up`，键盘采用 `simulate_key down/wait/up`；没有激活用户前台窗口、没有使用 Computer Use、没有发送物理键盘。

- 构建 `build_37e34d20a804`，`StandaloneWindows64`，`MainMenu + Bootstrap + InitialHarbor`，`Succeeded`，BuildReport=`0 errors / 0 warnings`，总大小 `220872966` bytes。EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`；Assembly-CSharp SHA256=`6EBD285D9A70A88996E4E1D6CACD11EA04C4A880BF6A46A999DA6A2761DB822C`。
- 路线 marker：W 角色移动；I 仓库打开且模态内 W 被阻断；M 市场打开/重复/Esc；T 酒馆打开/重复/Esc；E 登船/下船/再次登船；Y 出航；航行中 M 被 D3 owner 拒绝；船上 W/A 位移；Esc 暂停与恢复 `TimeScale=0→1`。入口由 MainMenu 加载 `InitialHarbor` build index `2`。
- Runtime `console --level error` 返回 `0`；Player log 为 `179991` bytes，大小写敏感 `ERROR|EXCEPTION|CRASH|FATAL=0`；runtime `quit` 后 PID 和 descriptor 均不存在。
- 临时 QA 设置已通过 dry-run/confirm 恢复并回读为 `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false / connectWithProfiler=false`；Editor=`ready/stopped`。

本轮只登记为 Q29 子范围 `PASS_WITHIN_SCOPE`，Q29 父状态保持 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`。该证据证明的是当前源码的 Pipeline 注入输入、运行态状态和业务拒绝路径；不能替代前台物理键盘、compositor-visible 截图、正式动作/Avatar、自然泳姿/握舵、植被准入、目标设备性能或最终人工视觉验收。下一步进入 Q30 当前源码独立审查，再进入 Q31 最终 native Player 视觉门禁。

权威结果：[Q29 R7](../../../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q29 R7 capture](../../../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)。

## 2026-09-14 VC-G01 W/S 与主 HUD 输入归属 Editor Pipeline R4

针对用户此前观察到的“W/S 移动时右上角菜单跟着切换”，本轮没有只依赖源代码阅读，而是使用已连接的 Unity Pipeline 在 `InitialHarbor` 真实 PlayMode 生命周期中运行专门的 D16 主 HUD 回归：`D16PrimaryHudNavigationPlayModeTests` 共 `2/2 PASS`。

- `GameplayMovementKeysDoNotNavigatePrimaryHud`：主 HUD 可见时，W/S 仍产生 Gameplay 移动轴，EventSystem 选中项保持不变；正常游戏导航保持关闭。
- `GameplaySuppressionReassertsWhenUiMoveActionIsRestored`：人为恢复 `InputSystemUIInputModule.move` 后，`HarborInteractionStabilizer` 下一轮仍移除该 UI/Navigate action，避免焦点恢复或其他 owner 在后台重新接管 W/S。
- 测试耗时 `6.89s`；Editor 最终 `ready/stopped`；Error Console 查询 `0`，`dropped=false`；本轮无生产代码、测试代码或配置变更。

这只证明当前源码在 Unity PlayMode 中的输入 owner 合同，不证明用户实际启动的旧 Player 构建、前台 Win32 物理键盘焦点或 compositor-visible 画面。若用户仍在新构建中看到 W/S 切换右上角菜单，需要保留具体构建文件/hash和复现步骤，下一轮再做同源 Player 路线复核。结果：[VC-G01 input R4](../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_REAUDIT_20260914_R4_RESULT.json)；日志：[VC-G01 input R4 log](../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_REAUDIT_20260914_R4.log)。

门禁不升级：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31=PARTIAL`。正式动作/Avatar、自然游泳/握舵、植被接地、穿模、前台 Player 画面、目标设备性能和人工最终视觉验收仍未关闭。

## 2026-09-14 VC-G01 hidden Player W/S 与主 HUD R5

在 Editor PlayMode `2/2 PASS` 之后，本轮进一步用当前源码 Q29 R9 Windows Player 的 Runtime Pipeline 控制面复核同一问题。Player 以隐藏 GUI 方式启动，没有夺取用户前台桌面，也没有使用 Computer Use 或物理键盘；通过只读 `SceneManager.LoadScene("InitialHarbor")` 作为 QA 入口，不把 MainMenu 入口写成已验收。

- 初始真实 Player 状态：玩家 `(0.000, 0.150, -2.700)`；`EventSystem.navigation=false`；`InputSystemUIInputModule.move=null`；`selected=<none>`。
- `simulate_key W down` 保持 `500ms` 后，玩家到 `(0.000, 0.125, -4.315)`，位移为 `(0.000, -0.025, -1.615)`；`navigation=false`，选中项仍为 `<none>`。`W up` 成功完成。
- Runtime Error Console=`0`、`dropped=false`；Player PID `30000` clean close，descriptor 删除；Player.log `104190` bytes，严格错误词扫描为 `0`（唯一大小写不敏感命中是 grounding 文本 `error=0.000`）。
- 一次直接引用 `UnityEngine.InputSystem.UI` 的探索性 eval 因 Runtime eval 参考集边界编译失败，随后使用 reflection-only 读取成功；该失败属于工具探针边界，不是产品运行时错误，已保留在结果日志中。

这证明的是当前源码隐藏 Player 控制面中的 W/S 与主 HUD 归属，不证明前台 Win32 焦点、compositor-visible UI、E/I/M/T 画面/业务流、正式动作/Avatar、自然泳姿/握舵、植被接地、目标设备性能或最终人工视觉验收。如果用户在新构建仍看到右上角菜单切换，需要记录实际 EXE hash 与前台复现；旧构建不能代表本次结果。

结果：[VC-G01 Player input R5](../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_PLAYER_REAUDIT_20260914_R5_RESULT.json)；日志：[VC-G01 Player input R5 log](../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_PLAYER_REAUDIT_20260914_R5.log)。

## 2026-09-14 VC-G01 hidden Player full interaction route R6

本轮使用当前源码 Q29 R9 Player 和 Unity Pipeline `com.unity.pipeline@0.6.0-exp.1` 完成一条完整控制面路线。Player 以 hidden GUI 方式启动，未夺取用户前台桌面，未使用 Computer Use 或物理键盘；MainMenu 使用 `simulate_pointer`，游戏内使用 `simulate_key`。

- MainMenu 的 New Game/Confirm 指针路线成功进入 `InitialHarbor` build index `2`；W 按住约 `450ms` 使玩家从 `(0.00,0.15,-2.70)` 移动到 `(0.03,0.12,-4.35)`，W 释放成功。
- I 打开仓库，模态期间 W 前后位置均为 `(0.00,0.17,-0.70)`，证明当前控制面下 gameplay W 被模态页阻断；I 关闭完成。关闭日志出现两条相同成功文本，源自 `CloseWarehouse()` 同时调用 `Show(...)` 与显式 `Debug.Log`，状态只发生一次关闭转换，已登记为日志重复而非重复业务动作。
- M 打开市场，重复 M 未产生第二次打开转换，Esc 关闭；T 打开酒馆，重复 T 未产生第二次打开转换，Esc 关闭并恢复 HUD；E 依次完成登船、下船、再次登船；Esc 完成暂停和恢复。
- Runtime `console --level error` 返回 `0`，`dropped=false`；Player clean close，PID/descriptor 消失；最终 Player.log `181468` bytes，SHA256=`619B705AF5223CB9BE34B76462DC3A1171E059BA5B4FE69F5E1645E5E4E5C7FD`，严格 `ERROR|EXCEPTION|CRASH|FATAL=0`。Editor 最终 `ready/stopped`，Runtime Pipeline 回读 `enableInBuilds=false / port=0`。

这条证据关闭的是当前源码 hidden Player 的交互控制面子范围，不是 VC-G01 最终通过。它没有证明前台 Win32 物理键盘焦点、compositor-visible 截图、正式动作/Avatar、自然游泳/握舵 IK、植被接地/穿模、目标设备性能或人工最终视觉验收。`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 保持。

结果：[VC-G01 R6](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INTERACTION_PLAYER_REAUDIT_20260914_R6_RESULT.json)；日志：[VC-G01 R6 log](../../../../../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INTERACTION_PLAYER_REAUDIT_20260914_R6.log)。

## 2026-09-14 UI focus isolation R1 after user W/S report

针对“W/S 移动时右上角菜单也切换”的逻辑风险，新增 D29 PlayMode 回归并用当前源码 hidden Player 复核。`D29Q15GameplayWasdDoesNotNavigatePrimaryHud=1/1 PASS`；原有 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes=1/1 PASS`；`D16A03GameplayWASDDoesNotNavigateHudAndModalWASDDoesNotLeak=1/1 PASS`。Player R1 经 MainMenu 指针进入 InitialHarbor，W/S 均产生角色位移，同时 EventSystem navigation 保持 `false`、UI move 为 `null`、selected 为 `<none>`；runtime error entries=`0`、clean close、Editor=`ready/stopped`。

本证据只说明当前源码 hidden Player/Editor 控制面没有复现 W/S 驱动主 HUD 导航，不能推断用户正在运行的旧构建、前台 Win32 焦点或 compositor-visible 画面一定无误。R1 build=`build_abd8d26ad740`，EXE/Assembly-CSharp hash 已登记。`VC-G01` 继续 `IN_PROGRESS`，`Q31` 继续 `PARTIAL`；正式动作/Avatar、自然 swim/helm、植被接地/穿模、性能与人工视觉门禁仍开放。结果：[Q31 UI focus R1](../../../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1_RESULT.json)；日志：[R1 log](../../../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1.log)。

## 2026-09-14 Q31 R5 visible native Player UI fix and temporary pause

本轮在不使用 Computer Use、不接管用户桌面的前提下，使用 Unity Pipeline Runtime 控制面临时启动一个可见 Windows Player，完成了当前源码 MainMenu 的真实合成器截图取证。`MainMenuController` 将 New Game 标题从 `y=150` 调整到 `y=210`；接受构建 `build_fc9d95893976` 为 `StandaloneWindows64 / Succeeded / 0 errors / 0 warnings`。`Q31_R5_UI_FIX_NEWGAME.png` 的标题与船长名占位文本已经分离，原先的重叠缺陷在此范围内关闭。

同一 Player 从 MainMenu NewGame/Confirm 进入 `InitialHarbor`，完成 W、E 登船、E 下船以及 W/S 路径截图；runtime error console=`0`，Player 严格错误词=`0`，Player clean close，Runtime Pipeline 和 Development Build 设置已恢复关闭。初始港口和登船帧是真实可见 Player 画面，但仍显示低模/程序化场景；这轮 W/S 路径没有视觉证明进入开放水域，因此自然游泳、正式动作/Avatar、自然握舵 IK、植被全镜头接地/穿模、物理 Win32 键盘和目标设备性能均保持开放。

暂停收口：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL`。后续恢复时从 Q31 R5 选择一个可复现的单一 visual owner，先写回归再修改，不因 Main Menu 的有限修复提前解锁后续门禁。结果：[Q31 R5](../../../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5_RESULT.json)；日志：[R5 log](../../../../../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5.log)；暂停交接：[PAUSE_20260914_Q31_R5](../../../../../../game/tools/results/LOCAL-QUALITY/PAUSE_20260914_Q31_R5.md)。

## 2026-09-14 current Formal Humanoid route rebind

Q03-A23 之后的当前源码复核已确认：默认 `InitialHarbor` 可见呈现不再是旧的纯程序化 fallback，而是 D20 Formal Humanoid consumer；Pipeline route probe 观察到 `HumanoidPoseAdapter=true`、`GenericPathAdapter=false`、7 个 candidate skinned renderer、17 个旧 fallback renderer 隐藏、`SwimmingVisualSupported=true`。这只是 VC-G01 的前置源/路由事实更新，不是 VC-G01 通过。

相关回归：D2=`8/8 PASS`、D17=`16/16 PASS`、Q03Formal=`5/5 PASS`、Q03D20Formal=`2/2 PASS`、recompile errors=`0`。当前 exact Player 为 `build_099a228c638b`，EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。证据：[Q00 exact rebind](../../../../../../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_SOURCE_REBIND_Q03A23_20260914_R2_RESULT.json)、[Q03 current route](../../../../../../game/tools/results/LOCAL-QUALITY/Q03/Q03_CURRENT_DEFAULT_HUMANOID_ROUTE_20260914_LUNA_RESULT.json)。

VC-G01 仍为 `IN_PROGRESS`：没有把 headless/hidden Pipeline 结果当作 compositor-visible 画面、前台物理键盘、自然泳姿/握舵、穿模/植被接地、目标设备性能或人工视觉 PASS。下一步是 Q04/Q05 的表现 owner 复核，之后回到 Q31 的最终 native Player gate。

## 2026-09-15 Q29 R11 入口 smoke 与临时暂停

按用户要求在当前源码建立安全暂停点。R11 使用 Unity Pipeline 临时构建 hidden Windows Player，真实完成 MainMenu NewGame/Confirm pointer entry 并加载 `InitialHarbor`，随后主动停止；没有把未执行的完整交互路线或截图记为 PASS。Q29 R10 仍是最近一次已完成的 hidden Player 全交互证据。

- `build_fb5d1ad58690`=`Succeeded / 0 errors / 7 warnings`；runtime error console=`0`；Player 严格错误词=`0`；PID `2724` 与 descriptor 已在 `quit` 后消失。
- 最终恢复并回读：Runtime Pipeline `enableInBuilds=false / port=0`，Build `developmentBuild=false / allowDebugging=false / connectWithProfiler=false`，Editor=`ready/stopped`。
- 本轮结果：[Q29 R11 paused result](../../../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED_RESULT.json)；暂停交接：[PAUSE_20260915_Q29](../LOCAL-QUALITY/PAUSE_20260915_Q29.md)。

门禁不升级：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q29=PARTIAL`、`D31-P04/S05/G06=PARTIAL`。正式动作/Avatar、自然 swim/helm、植被接地/穿模、前台物理输入、目标设备性能和人工最终视觉仍未收口。恢复入口为 Q30 当前源码独立审查，再进入 Q31 最终 native Player visual gate。

## 2026-09-15 Q30 current-source independent review R5

Q30 R5 已把 owner 回归重新绑定当前 HEAD `94950f36c0d5a3daf4ba719216808a0ae6d71be9`：Python `65/65`、D16InputRouter `11/11`、D17ActionQuality `16/16`、VcG01WorldLogic `14/14` 均通过范围内回归。无生产代码、Prefab、场景、测试契约或持久运行时设置变更；Editor 最终 `ready/stopped`，Runtime Pipeline `enableInBuilds=false / port=0`。

本轮真实捕获 FMOD `Error initializing output device (60)`；即使 Windows PnP 能看到 Realtek/NVIDIA 设备且 AudioSettings 可读，也不能替代成功 normal output 播放证据，音频保持 `BLOCKED_ENVIRONMENT`。Warning `160 / dropped=false` 和 D2/D27 fallback 家族继续说明截图可见动作/植被边界。VC-G01 仍 `IN_PROGRESS`，下一步 Q31 最终 native Player visual gate。

结果：[Q30 R5](../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5_RESULT.json)；日志：[Q30 R5 log](../../../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5.log)。
