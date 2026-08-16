# D18—D20 R03 最终状态仪表板

日期：2026-08-16  
总状态：**BLOCKED（停在 D19-R03-S01 正式门禁）**

## 结论

本轮已经完成 D18 角色资源补齐/复审、D19 港口路线诊断、R03 表现层构建、D20-A01-R02 隔离玩家工作台和真实 Player 性能取证，但没有达到 `D19-R02-S02=PASS`，因此不能启动 `D20-A02-R02` 的后续正式门禁或 `D20-S02-R01`，也不能报告 D18—D20 R03 计划完成。

历史 PARTIAL/BLOCKED 结果均保留；本文件是当前状态汇总，不替换历史报告。

## 任务矩阵

| 任务 | 当前结论 | 真实摘要 | 证据 |
|---|---|---|---|
| D18-R03-A01 | PASS（隔离资源工作台） | 33 个候选文件本地/哈希/隔离导入可复核；2 个 Humanoid 基体、6 个独立发型、8 个服装组合；0 个正式运行时消费者 | `game/tools/results/D18/D18-R03-A01/D18_R03_A01_MACHINE_RESULT_RERUN1.json` |
| D18-R03-S01 | PASS（只读资源消费门禁） | 33/33 文件、哈希与 Unity 导入记录通过；允许 D20-A01-R02 隔离工作，仍不授予运行时晋级 | `game/tools/results/D18/D18-R03-S01/D18_R03_S01_MACHINE_RESULT_RERUN1.json` |
| D19-R03-A01 RERUN2 | PARTIAL | 真实 Player 584.5516 秒；移动、落地、Character/Ship、3 次交互观察、12 张截图；完整路线和保存重载未闭合 | `game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN2_MACHINE_RESULT.json` |
| D19-R03-A02 | PARTIAL | R03 表现层实际进入 Player；最新 audit 199 Renderer/198 enabled/3,639 顶点/0 R03 Collider；旧代理消失但平地和块状建筑债务仍在 | `game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT.json` |
| D19-R03-A03 | PARTIAL | 真实 Player 三档 1920×1080 采样；请求 low/medium/high 实际是 Very Low/Low/Medium；性能数据真实，但不能替代最终路线/视觉门禁 | `game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT.json` |
| D19-R03-S01 | BLOCKED | A01—A03 有真实进展，但完整路线、保存重载、14 镜头、3 天气和视觉债务门槛未闭合；正式 D19 重验未启动 | `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT.json` |
| D19-R02-S02 | PARTIAL（历史当前门禁） | RERUN8 的仓库/酒馆/NPC 物理路线、固定镜头、天气和精确质量档仍不完整 | `game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN8_MACHINE_RESULT.json` |
| D20-A01-R02 | PASS（隔离工作台） | 2 个有效 Humanoid Avatar、6 发型、8 套服装和 1920×1080 Player 工作台证据；正式消费者 0 | `game/tools/results/D20/D20-A01-R02/D20_A01_R02_MACHINE_RESULT.json` |
| D20-A02-R01 | PASS（动作工作层） | 15 状态/15 Clip/6 参数，Root Motion=false；A02 EditMode 5/5、PlayMode 1/1、D17 7/7；正式消费者 0 | `game/tools/results/D20/D20-A02-R01/D20_A02_R01_MACHINE_RESULT.json` |
| D20-A02-R02 | BLOCKED/未启动 | 依赖玩家外观装配和上游门禁 | 无 R03 结果 |
| D20-S02-R01 | BLOCKED/未启动 | 硬依赖 D19 正式 PASS 和 D20 A01/A02 条件 | `game/tools/results/D20/D20-S02/D20_S02_MACHINE_RESULT.json`（历史阻断） |

## 已通过的子门槛

- D18-S02-R02 资源准入仍为 PASS；`LocalDevelopmentReady=0`、`ReleaseApproved=0` 未被修改。
- D18 R03 隔离审计：本地文件、SHA-256、Unity 导入记录可追溯；没有把隔离结果写成运行时就绪。
- D19 R03-A02/A03 Unity builder、compile 和 Player build 均退出码 0；没有改 Systems、AgentCore、Registry、存档 Schema 或 contracts。
- D19 R03-A03 的真实 Player 读回 `1920x1080`、VSync/GPU/GC/内存和 1% low；High 请求的实际项目档名为 `Medium`，已如实保留。
- 旧 D19 静态结果：D19 EditMode 34/0、D19 PlayMode 8/0/1 skipped、D17 PlayMode 7/0、D18 Python 11/0；这些是子门槛，不能替代真实 Player 通过。

## 真实运行数据

### 路线

- Player：`D19_R03_A01_Player_RERUN2.exe`。
- 时长：584.5516 秒。
- 真实截图：12 张，目录 `game/tools/results/D19/D19-R03-A01/screenshots/route_rerun2_*.jpg`。
- 输入边沿诊断：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_INPUT_CONTROL_TEST.json`；它只记录 computer-use 短按未形成稳定可见控制，不是路线 PASS。
- 观察到：出生落地、位移、Character/Ship 模式、码头/泊位邻域、酒馆坐标邻域和部分 E 交互。
- 未证明：市场、仓库、船坞、返港、保存输入、保存后退出/重载恢复；连续按住 WASD 也未证明。
- `route_rerun2_save_03.jpg` 只是 Player 截图，不是保存成功证明。

### 性能

| 请求档位 | 实际 Unity 档名 | 分辨率 | 采样 | 1% low | GPU | GC Gen0 | 峰值 Reserved |
|---|---|---|---:|---:|---:|---:|---:|
| low | Very Low | 1920×1080 | 60.014s | 59.08 FPS | 0.103ms | 81 | 206,385,152 B |
| medium | Low | 1920×1080 | 60.016s | 58.29 FPS | 0.119ms | 104 | 210,579,456 B |
| high | Medium | 1920×1080 | 60.005s | 42.73 FPS | 0.496ms | 124 | 210,579,456 B |

这些数据来自真实 Windows Player，不是离屏 `Camera.Render`；但项目没有名为 High 的质量档，所以不能把第三行改写为 High。

### 视觉

- R03 A02 已把旧 `D19_SaintMargaret_Root` 957 个渲染器禁用，保留稳定碰撞/锚点；最新 R03 根为 199 Renderer、198 enabled。
- 真实 Player 抽查 4 张出生镜头（含最新 `r03_a02_spawn_rerun6.jpg`）：无粉色缺材质，旧前景代理减少；仍可见大面积平面、弱建筑轮廓、块状远景和底部路线/开发信息。
- 固定 Player 镜头：0/14；天气变体：0/3；所以视觉门禁未通过。

## 测试与验证

- 新增结果 JSON 格式校验：8/8 通过。
- `python -m unittest discover -s tools/tests -p 'test*.py'`：124 个测试，120 通过、4 失败、退出码 1。
- 4 个失败均在既有 `test_d18_s02_r01_intake_refresh.py`，原因是测试仍期待 D18-S02-R01 的旧 task/计数（期望 1833、1 类别、旧上游键），当前磁盘真实 catalog 已是 R02/R03 版本；本轮未篡改历史测试或 catalog。
- Unity compile/build：本轮 D19 R03-A01/A02 均退出码 0。
- `git diff --check`：退出码 2；输出包含大量 LF→CRLF 警告和 `game/Assets/Scenes/InitialHarbor.unity` 的既有尾随空白（3459、3460、6404、6412、6614、6615）。没有为追求绿灯清理或覆盖场景序列化内容。
- 当前 Unity/Player 进程已关闭。

## R03 硬阻断与责任

1. **D19-R03-A01 / D19-R02-S02**：真实 Player 输入边沿不足以形成可复核连续路线，保存/重载闭环缺失；责任：继续在 D19 R03-A01 范围修复真实输入/交互取证，随后重跑 S01。
2. **D19-R03-A02**：出生固定镜头仍有平面/块状建筑债务，14 镜头和天气取证缺失；责任：D19 R03-A02 或新的 D19 视觉修复任务；不得用雾、裁切或旧截图掩盖。
3. **D20-A02-R02 / D20-S02-R01**：A01-R02 隔离工作台已通过，但动作兼容、正式外观装配和运行时消费者仍未完成；责任：D20 动作/正式集成任务，不能提前标 LocalDevelopmentReady。
4. **测试基线**：4 个旧 R01 catalog 单测需要后续任务决定“更新断言到新版本”还是保留历史隔离测试；本轮不改测试口径。

## 下一步

继续前必须先完成 D19 路线/保存取证和港口视觉门槛，生成新的 D19-R03-S01 结果；只有 S01 放行并且正式 `D19-R02-S02=PASS` 后，才可开始 D20-A01-R02/A02-R02 和最终 D20-S02-R01。D18-S03、D21+ 仍未开始。

## RERUN7_FULLSCREEN 最新增量（2026-08-16）

- D19-R03-A01 新增真实 Player 主菜单复验，但系统安全中心防火墙提示拦截 Player 焦点；主菜单显示成功，New Game 未被真实输入触发，未进入 InitialHarbor。
- 本轮新增 1 张真实主菜单截图、2 份机器可读结果和 2 份 Player 日志；结果均为 `PARTIAL`，没有改写 RERUN2/RERUN4/RERUN6 历史。
- 证据：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN7_MACHINE_RESULT.json`、`D19_R03_A01_RERUN7_ROUTE_LOG.json`、`screenshots/mainmenu_rerun7_system_blocker.jpg`。
- R03-S01 继续 `BLOCKED`：完整路线、保存/重载、14 个固定 Player 镜头、3 天气变体和最终视觉收口仍未完成；D19-R02-S02、D20-A01-R02、D20-A02-R02、D20-S02-R01 均不放行。

## RERUN10/RERUN17 最新增量（2026-08-16）

- A01 RERUN10 使用不带 Development/AllowDebugging 的 Player，仍被未触碰的 Windows 安全提示拦截在 MainMenu；没有新增路线、交互或保存通过事实。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RERUN10_MACHINE_RESULT.json`。
- A03 RERUN17 已闭合性能子任务的真实档名问题：Low/Medium/High 三档均为 1920×1080、30 秒预热 + 60 秒采样，结果：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN17.json`。
- D19-R03-S01 仍 BLOCKED，最新汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN17.json`。D19-R02-S02、D20 正式集成继续不放行。

## RERUN18 固定镜头更新（2026-08-16）

- A02 RERUN15 已真实输出 14/14 个 1920×1080 固定镜头，但人工视觉仍失败于平坦色块、代理几何、屋顶重叠和开发 HUD，天气变体仍 0/3。
- S01 最新机器结论仍 BLOCKED：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN18.json`。

## RERUN19 当前门禁（2026-08-16）

- A02 RERUN17 已真实输出 14/14 个 1920×1080 Player 固定镜头，且目标调色、三角树/浅色竖杆表现债务有所改善；人工复核仍为平坦地表/码头、低细节块状建筑、近岸代理几何，天气为 Calm-only（0/3），所以 A02 仍 PARTIAL。
- A01 RERUN10 仍被未触碰的 Windows 安全提示阻断在 MainMenu，没有新游戏、完整路线或保存重载证据。
- A03 RERUN17 的 Low/Medium/High 性能子任务保持 PASS。
- 新 S01 汇总 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN19.json` 继续 BLOCKED；D19-R02-S02 与 D20-S02-R01 不放行。

## RERUN20 真实 Player 路线/恢复增量（2026-08-16）

- A01 RERUN10 使用不带 `-popupwindow` 的真实 Windows Player，持续 704.296936 秒，1920x1080；真实观察到移动、Character/Ship 控制切换、E 交互、保存输入，16 张路线 PNG 均为 1920x1080。
- 路线到达出生点、码头、泊位、酒馆，未到达市场、仓库、船坞、返港；不可行走碰撞体恢复提示仍在。
- 关闭 Player 后，新 Player 从 MainMenu Continue 加载成功，InitialHarbor 重建并记录 D19_TerrainRoot 落地。保存—退出—Continue 恢复为有限范围通过，完整离港—返港保存闭环仍未通过。
- 新 S01 结果 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN20.json` 继续 BLOCKED；`D19-R02-S02`、D20 正式集成不放行。A02 视觉/天气和 A03 性能结论沿用其真实证据。

## RERUN29 当前有效证据（2026-08-16）

- A01 最新真实 Player：`694.13134765625s`、1920×1080、16 张截图；保存反馈和登船/下船有日志证据，但市场/仓库/船坞/返港与同轮重载未闭合。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN29_MACHINE_RESULT.json`。
- A02 最新静态 Integration 审计：315 Renderer、314 enabled、6457 顶点、1 个受控路线碰撞体、8 个 intake-eligible 外部候选；视觉债务和天气矩阵仍未闭合。结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN28_ROUTE_GROUND.json`。
- S01 最新门禁：`BLOCKED`，不放行 D19-R02-S02 或 D20-S02-R01。结果：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN29.json`。

## RERUN35 当前增量（2026-08-16）

- D19-R03-A02 在 R03 Integration 生成器内收敛屋面、门廊、市场入口和远景尺度；Unity 构建/审计成功，315 Renderer、314 enabled、6457 顶点、1 个路线 Collider、8 个 intake-eligible 环境候选。
- RERUN35 真实 Player 1920×1080 运行 254.287841796875 秒并生成 5 张探针截图；固定镜头 Player 在几何更新后重新生成 14/14 张 1920×1080 backbuffer 截图。屋顶遮挡有所收敛，但平地/低细节建筑/代理岩体/HUD 仍在，天气仍 Calm-only，A02 继续 **PARTIAL**。
- 机器结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_MACHINE_RESULT_RERUN35_VISUAL_PATCH.json`、`D19_R03_A02_MACHINE_RESULT_RERUN35_FIXED.json`。真实短测没有形成移动/保存证据，不替代 RERUN29 长路线。
- D19-R03-S01 的 RERUN29 门禁结论仍 **BLOCKED**，D19-R02-S02 与 D20-S02-R01 继续不放行。

## RERUN39 / RERUN36_CURRENT 当前增量（2026-08-16）

- D19-R03-A01 当前最新独立真实 Player 证据为 RERUN39：约 211.79 秒、1920×1080，菜单/新游戏/上船/下船/移动输入/保存反馈真实可见；市场、仓库、船坞、返港和同轮 Continue 未闭合，结论 **PARTIAL**。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN39_MACHINE_RESULT.json`。
- D19-R03-A03 已在当前 R03 场景重采性能：Medium/High 1% low 约 59.65/59.64 FPS；Low 真实 1% low=7.903 FPS，故当前场景性能结论 **PARTIAL**。结果：`game/tools/results/D19/D19-R03-A03/D19_R03_A03_MACHINE_RESULT_RERUN36_CURRENT.json`。
- Low RERUN36_CURRENT2 复采为 1% low=59.624 FPS，但首轮 7.903 FPS 仍是有效最差值；按三轮取最差规则，不能将性能门禁改写为 PASS。
- D19-R03-S01 继续 **BLOCKED**：完整路线、同轮保存/退出/Continue、Low 性能门槛、14 镜头视觉收口和 Rain/Storm 仍未完成；`D19-R02-S02` 与 `D20-S02-R01` 继续锁定。

## RERUN41 当前增量（2026-08-16）

- A01 RERUN41 使用带专属探针参数的真实 Player 运行 `59.76485061645508s`、请求 1920×1080；Player/controller/router、移动、交互和保存输入为 true，实际到达出生、市场、酒馆、码头、泊位；仓库、船坞、返港未到达。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN41_MACHINE_RESULT.json`。
- RERUN41 运行时探针和日志已落盘：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN41.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN41_PLAYER_RERUN2.log`。当前运行截图数为 0，未把历史截图冒充当前证据。
- 新 S01 汇总：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN41_CURRENT.json`，结论仍 **BLOCKED**。D19-R02-S02 与 D20-S02-R01 继续锁定；D18-S03、D21+ 未开始。

## RERUN42 当前增量（2026-08-16）

- A01 RERUN42 使用独立非 Development Player，真实运行 `200.1754150390625s`、请求 1920×1080；探针观察到 Player/controller/router、移动和交互输入，当前轮产生 6 张截图。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN42_MACHINE_RESULT.json`。
- 本轮只到达码头与泊位，未到达市场、仓库、酒馆、船坞、返港或出生锚点；保存输入 false，未形成同轮 Continue。探针/日志：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN42.json`、`game/tools/results/D19/D19-R03-A01/D19_R03_A01_ROUTE_RERUN42_PLAYER.log`。
- RERUN42 仍为 **PARTIAL**，S01 仍 **BLOCKED**；D19-R02-S02 与 D20-S02-R01 继续锁定。

## RERUN44 当前增量（2026-08-16）

- D19-R03-A01 在 Integration 桥接层禁用 3 个旧重复码头/跳板碰撞体后，重新构建并运行独立真实 Player `193.38430786132813s`、请求 1920×1080；Player/controller/router、移动输入和交互输入为 true，保存输入为 false，当前轮生成 7 张截图。结果：`game/tools/results/D19/D19-R03-A01/D19_R03_A01_REAL_PLAYER_RERUN44_MACHINE_RESULT.json`。
- 运行时船位为 `(0,0,0)`，玩家与船体不重叠，最终位移约 `0.045m`；本轮仅到达码头/泊位，市场、仓库、酒馆、船坞、返港及保存/重载未闭合。重复碰撞修复没有被解释为路线通过；旧 RERUN42/RERUN41 及更早证据保持不变。
- D19-R03-S01 新机器门禁：`game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN44_CURRENT.json`，结论仍 **BLOCKED**。当前性能 Low 首轮异常、视觉债务、14 镜头闭合和 Rain/Storm 证据仍未完成；D19-R02-S02、D20 正式集成继续锁定。

## RERUN45/RERUN46 当前增量（2026-08-16）

- R03 Integration 桥接继续只做集成层收敛：在保留稳定节点与 D6 所有权的前提下，禁用 D19-A01 旧地形 Collider 1 个；RERUN45/RERUN46 Player 日志均确认 `legacy terrain colliders disabled=1`。
- RERUN45 独立真实 Player `120.1856460571289s`、2 张截图，到达酒馆/码头/泊位；RERUN46 `126.42691040039063s`、4 张截图，到达出生/酒馆/码头/泊位；两轮保存输入均 false。结果分别为 `D19_R03_A01_REAL_PLAYER_RERUN45_MACHINE_RESULT.json`、`D19_R03_A01_REAL_PLAYER_RERUN46_MACHINE_RESULT.json`。
- 旧地形 Collider 修复使路线新增离开初始区域的真实证据，但没有闭合 300 秒、市场/仓库/船坞/出航/返港/保存重载；RERUN46 仍有不可行走碰撞恢复日志。因此 S01 仍 **BLOCKED**，D19-R02-S02 与 D20 正式集成继续锁定。
- 最新门禁机器结果为 `game/tools/results/D19/D19-R03-S01/D19_R03_S01_MACHINE_RESULT_RERUN46_CURRENT.json`；旧门禁和历史路线结果均保留。
