# D19-R02-S02 圣玛格丽特港最终场景复验

结论：**PARTIAL**（2026-08-15）。

这是当前 D19 门禁状态的新一轮真实验收，不删除或改写历史 `D19-S02`、`D19-S02-R01`、`D19-R01-S02` 或 A01—A05 报告。旧报告中的 PARTIAL/BLOCKED 仍可追溯；本文件只决定本轮是否放行 D20-S02-R01。

## 验收范围与保护基线

- D18-S02-R01 资源准入刷新为 PASS；本轮没有提升资源状态。当前 `LocalDevelopmentReady=0`、`ReleaseApproved=0`。
- InitialHarbor 本轮没有改写。当前 SHA-256 为 `051830CED5E2DA9C85019991F76A361AF0DAF1015ECCCB7A4D6A2F3B4520015B`，D19-R02-A04 的回滚快照仍在 [rollback_InitialHarbor_20260815_150433.unity](../../../../game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_150433.unity)。
- D19 Integration Prefab 的 SHA-256 为 `B0FB811CF957DF895DE5FD68FFEAAFA2460E1D277F77A8BCD27DE19811A49E8E`。
- 真实 x86 Player 首次新游戏失败于 `sqlite3` 缺失；失败证据保留。随后用 x86_64 Player 完成实际键鼠运行，不把 x86 失败隐藏。

## 真实 Player 路线

实际 x86_64 Player 会话从 16:45:24 到 16:52:47，墙钟约 443 秒，超过 300 秒最低时长；随后重新启动并用 Continue 验证读档。路线并未完整通过：市场和 D3 航程/保存/Continue 有证据，但 W 移动、仓库、酒馆、码头、登船和 NPC 反馈没有被真实证明。

逐节点证据见 [D19_R02_S02_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_ROUTE_LOG.json)。关键结果：

| 节点 | 结果 |
|---|---|
| 主菜单→新游戏→出生/落地 | 观察到；x64 Player 成功进入港口 |
| 市场 | `B` 打开交易页，鼠标关闭成功 |
| 仓库/酒馆 | 未验证 |
| NPC 对话/失败反馈 | `E` 无可见反馈，失败 |
| 码头/泊位/登船 | 未验证；不能用 D3 日志代替真人接近交互点 |
| 出航/推进/返港 | D3 日志确认状态事件，物理登船和返港路线未完整验证 |
| 保存/退出/Continue | D3/Save 日志和 Continue Player 日志确认 |

路线截图为真实 Player 帧，位于 [D19-R02-S02/screenshots](../../../../game/tools/results/D19/D19-R02-S02/screenshots)。本轮不把“运行时间超过 300 秒”写成“完整路线通过”。

## 固定镜头与视觉检查

已生成 14 个固定镜头文件 `fixed_01`—`fixed_14`，但 Computer Use 读回尺寸为 `1099×648` 逻辑像素，未证明 D18 要求的 `1920×1080` 输出。14 个镜头中未观察到粉色缺材质或大面积纯白平面；但仍观察到低细节方块/灰盒式地形、宽阔平坦绿/橙表面、简单建筑轮廓以及底部 D2/D3 路由文字。固定镜头视觉门禁失败，具体机器结论见 [D19_R02_S02_VISUAL_QA.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_VISUAL_QA.json)。天气/时间三变体本轮为 0/3，未伪造截图。

## 1080p 性能

Low、Medium、High 均由真实 x86_64 Player 运行，warm-up 30 秒、采样 60 秒、每秒一个进程样本，三档各 60/60 样本存活。工作集约为 Low `330.69 MB`、Medium `322.36 MB`、High `323.10 MB`。全局 GPU 使用率约 38.55%—39.15%，这是 `nvidia-smi` 全局计数，不是 Unity GPU 帧时间。

当前运行时未输出 FPS、1% low、p95 帧时间、CPU/GPU 帧时间或 GC；窗口采集为 `1099×648`，也不能证明真实 backbuffer 为 `1920×1080`。所以本轮性能是 `INCOMPLETE`，不能按 D18 的平均 60 FPS / 1% low / 帧时间 / GC 门槛放行。原始结果和汇总见 [D19_R02_S02_PERFORMANCE_1080P.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P.json)。

## 回归与资源审计

- D19 PlayMode：8 PASS、0 FAIL、1 SKIP；[D19_R02_S02_D19_PLAYMODE.xml](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D19_PLAYMODE.xml)。
- D17 PlayMode：7 PASS、0 FAIL；[D19_R02_S02_D17_PLAYMODE.xml](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D17_PLAYMODE.xml)。
- D19 EditMode：34 PASS、0 FAIL；[D19_R02_S02_D19_EDITMODE.xml](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D19_EDITMODE.xml)。
- D18 intake 单元测试：11/11 PASS；完整命令和结果写入 [D19_R02_S02_COMMANDS.txt](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_COMMANDS.txt)。
- 当前 intake 目录声明 30 个 manifest、1,895 个候选文件、1,865 个 `intake_eligible` 文件，状态 PASS；只读 manifest 审计（`packs` 下 28 个 manifest）0 错误，状态 PASS。两者扫描口径不同：前者含增量/包记录，后者只审计 `.manifest.json` 文件；本轮没有改 catalog。
- `LocalDevelopmentReady=0`、`ReleaseApproved=0` 保持真实；没有把 intake 证据偷换成运行时就绪。

回归仍记录 D2 接地冲突：日志反复出现 R02 地表接触后恢复到旧 `BeachGround`，这不是本轮验收可以忽略的视觉/碰撞问题。

## 门禁结论与责任顺序

本轮不允许开始 D20-S02-R01。必须先由 D19 路线/交互责任任务补齐真实移动、NPC 反馈、仓库/酒馆/码头/登船；由 D19 场景责任任务消除固定镜头中的灰盒/方块表现并解决接地冲突；再由性能责任任务提供真实 1920×1080 backbuffer、FPS/1% low、CPU/GPU 帧时间和 GC 证据。之后重新执行本任务，新增 3 个天气变体和完整保存/恢复路线。

机器总结果见 [D19_R02_S02_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_MACHINE_RESULT.json)，交接见 [D19-R02-S02_HANDOFF.md](handoffs/D19/D19-R02-S02_HANDOFF.md)。

## RERUN2 真实 Player 复验（2026-08-15）

本节是当前门禁的增量复验，不删除或覆盖上文及历史 `D19-S02`、`D19-S02-R01`、`D19-R01-S02` 证据。RERUN2 使用重新构建的 x86_64 Player；D19-R02-A04 已停止写 InitialHarbor，场景本轮未修改。最新场景 SHA-256 为 `53861A681D9C59C4635CC3D037868C37F1D06CB54A9F22B6DE88F12B2026DCEB`，Integration Prefab SHA-256 为 `52F5C259E6124FEF43CA86D02F64AD6FAAEB0F7EC70FF03DC84541327ABD0FE4`。历史机器结果仍保留在 `D19_R02_S02_MACHINE_RESULT.json`；RERUN2 机器结果在 [D19_R02_S02_RERUN2_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN2_MACHINE_RESULT.json)。

### 路线结果

- 真实 Player 从新游戏进入港口；`D2/Physics` 日志确认玩家落在 `D19_TerrainRoot`，玩家视觉可见。
- 真实 `B` 打开市场，鼠标关闭交易页；真实 `T` 产出 D3 航线规划日志，`N` 产出海上状态日志，`C/F5` 显示“存档检查点已完成”并写入 D3 存档日志。
- 退出后重新启动并点击 Continue，Low/Medium/High 日志均出现“存档加载成功，进入游戏”，港口重新打开。
- 连续港口观察时长为 303 秒，超过 300 秒时长门槛；但 `WASD`、`E`、`T` 未形成可见可靠位移/登船/出航反馈，仓库、酒馆/NPC、码头/泊位、物理返港均未完成。因此完整路线仍为 PARTIAL，不能把逻辑日志当作真人路线通过。
- 路线逐节点记录在 [D19_R02_S02_RERUN2_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN2_ROUTE_LOG.json)，RERUN2 新增 20 张真实 Player 截图；历史 14 张固定镜头仍保留，没有把普通重复帧冒充新的固定镜头矩阵。

### 性能结果

RERUN2 以 `-screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -force-d3d11` 启动真实 Player，并分别请求 `-quality 0/1/2`。Low/Medium/High 及连续路线默认档均由 `D16RuntimePerformanceProbe` 写出真实帧数据：

| 请求档位 | 帧数 | 平均 FPS | 最低 FPS | 平均主线程 ms | GPU 帧 ms | GC Gen0 | 峰值 Reserved |
|---|---:|---:|---:|---:|---:|---:|---:|
| Low | 487 | 59.94 | 34.57 | 4.02 | 0.768 | 251 | 202,190,848 |
| Medium | 512 | 59.95 | 36.86 | 4.10 | 0.754 | 266 | 206,385,152 |
| High | 485 | 59.94 | 34.59 | 4.10 | 0.765 | 251 | 202,190,848 |
| 连续路线默认档 | 1,370 | 59.94 | 19.48 | 5.00 | 0.784 | 563 | 210,579,456 |

原始报告分别见 [Low](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN2_LOW_REQUEST.json)、[Medium](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN2_MEDIUM_REQUEST.json)、[High](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN2_HIGH_REQUEST.json) 和 [连续路线](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN2_DEFAULT.json)，汇总见 [D19_R02_S02_PERFORMANCE_1080P_RERUN2.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN2.json)。真实 Player 采样数据已取得，但日志没有运行时质量级别、VSync 或实际 backbuffer 回写；Computer Use 回读仍是 1099×648 逻辑像素，1% low/p95 也没有由探针提供，且最低 FPS 低于目标门槛。因此性能门禁仍不通过，不宣称 1080p 三档 PASS。

### RERUN2 回归与视觉结论

- D19 PlayMode：8 PASS、0 FAIL、1 SKIP；[RERUN2 XML](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D19_PLAYMODE_RERUN2.xml)。
- D17 PlayMode：7 PASS、0 FAIL；[RERUN2 XML](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D17_PLAYMODE_RERUN2.xml)。
- D19 EditMode：34 PASS、0 FAIL；[RERUN2 XML](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_D19_EDITMODE_RERUN2.xml)。
- D18 资源 intake/manifest 只读审计：均 PASS；当前扫描为 29 个 intake manifest、1,894 个候选文件、1,674 个 intake-eligible 文件，`LocalDevelopmentReady=0`、`ReleaseApproved=0`，本轮未改资源状态。
- 新 Player 截图没有观察到粉色材质或大面积纯白平面，但仍清楚看到平坦橙/绿色地表、简单方块式港口构件以及底部调试/路线文字；14 个新固定镜头矩阵和 3 个天气变体没有完成。视觉结论仍为 PARTIAL，机器检查见 [D19_R02_S02_VISUAL_QA_RERUN2.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_VISUAL_QA_RERUN2.json)。

### 当前放行结论

RERUN2 仍为 **PARTIAL**。真实证据新增了保存/Continue、市场、D3 状态和帧时间，但未闭合真实移动/交互路线、物理返港、视觉固定镜头、运行时质量/分辨率证明和性能分位数门槛。`D20-S02-R01` 继续禁止启动；责任顺序为 D19 路线/交互修复 → D19 场景视觉/接地修复 → 真实 Player 固定镜头与三档性能补证 → 再执行 D19-R02-S02。

## RERUN4 当前门禁复验（2026-08-15）

本节是新的当前 D19-R02-S02 门禁结果；原始 PARTIAL、RERUN2 结果和失败证据均保留，没有覆盖历史报告。RERUN4 使用 D2 E 输入兼容修复后的 x64 Player，且 InitialHarbor 未被本任务改写。场景 SHA-256 仍为 `53861A681D9C59C4635CC3D037868C37F1D06CB54A9F22B6DE88F12B2026DCEB`，Integration Prefab SHA-256 仍为 `52F5C259E6124FEF43CA86D02F64AD6FAAEB0F7EC70FF03DC84541327ABD0FE4`。

真实 Player 会话持续约 **312 秒**，实际窗口输入完成并留痕：新游戏/出生落地、E 登船、T 规划航线、N 推进、L 返港结算、E 下船、F5 保存、Alt+F4 退出和主菜单 Continue 重载；重载日志明确出现 `[MainMenu] 存档加载成功，进入游戏`。但仓库、酒馆/NPC、从出生点真实步行到码头/泊位的连续路径仍未验证，因此 312 秒只满足持续时长，不等于完整路线通过。逐节点记录见 [D19_R02_S02_RERUN4_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN4_ROUTE_LOG.json)，RERUN4 真实 Player 帧 18 张位于同目录。

RERUN4 没有重新生成三档精确帧探针，故不把历史 RERUN2 数据重标为当前新证据。性能结果明确为 `INCOMPLETE`；历史真实基线和缺失的 backbuffer、运行时质量档位、VSync、1% low/p95 说明见 [D19_R02_S02_PERFORMANCE_1080P_RERUN4.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN4.json)。

RERUN4 真实帧没有粉色材质或大面积纯白平面，但仍能看到平坦橙/绿地表、低细节方块式港口、红色线性码头构件和调试 HUD；新鲜固定镜头数为 0、天气变体数为 0，见 [D19_R02_S02_VISUAL_QA_RERUN4.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_VISUAL_QA_RERUN4.json)。回归重新通过：D19 EditMode **34/34**，D19 PlayMode **8/8（另 1 项跳过）**，D17 PlayMode **7/7**，D2 A02 专项 EditMode **3/3**、PlayMode **2/2**。

因此当前结论仍为 **PARTIAL**，不得启动 D20-S02-R01。LocalDevelopmentReady=0、ReleaseApproved=0，D18-S03、D21+ 仍未执行。机器结果见 [D19_R02_S02_RERUN4_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN4_MACHINE_RESULT.json)。

### RERUN2 按键保持复核（2026-08-15）

为排除单次按键事件过短的可能性，重新启动的真实 Player 在窗口激活后使用 Win32 `keybd_event` 保持 W 2.5 秒；前后帧分别保存为 `screenshots/rerun2_hold_before.png` 与 `screenshots/rerun2_hold_after_w_decoded.jpg`。玩家仍停留在出生位置，未出现可定位的位移、动作或路线反馈；Player 随后已关闭。该复核仍是 **FAIL_NO_RELIABLE_DISPLACEMENT**，没有改变 RERUN2 的 PARTIAL 结论。机器记录已补入 [D19_R02_S02_RERUN2_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN2_ROUTE_LOG.json) 和 [D19_R02_S02_RERUN2_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN2_MACHINE_RESULT.json)。

## RERUN6 当前门禁复验（2026-08-15）

本节是最新当前门禁结果。此前原始报告、RERUN2、RERUN4 及所有失败证据均保留；本节不覆盖历史结论，只决定当前是否放行 D20-S02-R01。

### 场景与资源边界

- RERUN6 消费 D19-R02-A04 RERUN5E 的最新场景结果。S02 没有做语义场景或 Prefab 修改；只对 InitialHarbor 的 3 处尾随空格做了 YAML 文本卫生清理，使 `git diff --check` 可复核通过。
- 当前 InitialHarbor SHA-256 为 `2D821BA9850C9F1260F27148B105BF1AF1C7C7F539B1A26E2371357AEB271369`；Integration Prefab SHA-256 为 `D0A21A05F4800A0D83971637C40FDDF971BBB5088D8FB13D4520F06046D6BCE3`。A04 的语义回滚快照仍保留：`game/tools/results/D19/D19-R02-A04/rollback_InitialHarbor_20260815_2015_pre_rerun5_intake_detail.unity`。
- 本轮没有改变资源状态或全局目录；LocalDevelopmentReady=0、ReleaseApproved=0。D2 E 输入优先登船的源代码修复属于 D19-R02-A02 RERUN7，本轮只消费其已验证构建。

### 真实 Player 路线

- 使用 x86_64 Windows Player 从 Main Menu 新游戏开始，实际连续运行约 311 秒；包含退出并重新启动后的总观测约 365 秒。未用 Inspector 直接调用替代玩家输入。
- 新游戏/出生落地、E 登船、B 打开市场、Escape 关闭市场、T 规划航线、N 推进海上状态、L 首次未到达时给出可理解失败反馈、继续 N 后真实到达、L 靠港结算、E 下船、F5 保存、Alt+F4 退出，以及 Continue3 重新加载港口均有日志和截图。
- 首次 L 的反馈为“尚未抵达目的地，不能靠港结算”；Route2 继续推进后反馈为“已靠港并完成结算：绿湾村”。Continue3 日志出现 `[MainMenu] 存档加载成功，进入游戏`，并有重载后的港口帧。
- 仓库、酒馆、酒馆 NPC、从出生点步行到码头/泊位的连续物理路线仍未验证；暂停面板也没有形成可复核的独立反馈。因此这不是完整路线 PASS。路线明细见 [D19_R02_S02_RERUN6_ROUTE_LOG.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN6_ROUTE_LOG.json)，原始输入日志见 [RERUN6 输入目录](../../../../game/tools/results/D19/D19-R02-S02)。

### 1080p 性能

- Low、Medium、High 均由真实 Player warm-up 30 秒、采样 60 秒，三档进程样本均为 60/60 存活。工作集范围分别约为 Low `268,984,320—269,062,144`、Medium `268,959,744—268,972,032`、High `268,976,128—269,307,904` bytes。
- 当前采样器没有写出 FPS、1% low、p95、CPU/GPU 帧时间或 GC，运行时质量档位未被证明，窗口回读也不能证明真实 1920×1080 backbuffer。因此性能结论为 INCOMPLETE，不满足 D18 1080p 指标门槛。原始档见 [Low](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERF_LOW_RERUN6.json)、[Medium](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERF_MEDIUM_RERUN6.json)、[High](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERF_HIGH_RERUN6.json)，汇总见 [D19_R02_S02_PERFORMANCE_1080P_RERUN6.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN6.json)。

### 视觉与固定镜头

- 本轮新增真实 Player 帧 22 张，覆盖出生、登船、交易、海上推进、靠港、下船、保存、退出和读档；未观察到粉色材质或大面积纯白平面。
- 但精确 14 个固定玩家镜头矩阵为 0，天气变体为 0；帧中仍可见大面积平坦棕/橙地表、块状/低细节港口构件和简单植被。建筑可读性、固定镜头无灰盒以及天气表现未闭合，机器结论为 PARTIAL。见 [D19_R02_S02_VISUAL_QA_RERUN6.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_VISUAL_QA_RERUN6.json) 与 [RERUN6 截图目录](../../../../game/tools/results/D19/D19-R02-S02/screenshots)。

### 回归、审计与结论

- D19 EditMode 34/34 PASS；D19 PlayMode 8 PASS、0 FAIL、1 SKIP；D17 PlayMode 7/7 PASS；D2 A02 RERUN7 EditMode 3/3、PlayMode 2/2 PASS。
- D18 intake/manifest 只读审计 PASS；Python 单元测试 11/11 PASS；LocalDevelopmentReady=0、ReleaseApproved=0；`git diff --check` exit 0。完整机器汇总见 [D19_R02_S02_RERUN6_MACHINE_RESULT.json](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN6_MACHINE_RESULT.json)。
- 当前结论仍为 **PARTIAL**。真实路线缺仓库/酒馆/NPC/步行码头证据，固定镜头和天气缺失，画面仍有明显灰盒/块状债务，性能遥测缺失；因此不放行 D20-S02-R01。D18-S03、D21+ 未执行。

## RERUN8_AUTHORED 当前门禁复验（2026-08-15）

本节是基于最新 A04 authored-building 构建的增量复验；RERUN6 及更早报告、机器结果、截图和失败证据全部保留。本节不把静态构建证据写成运行时通过，也不改变 D18 资源状态。

### 场景与资源边界

- 当前 InitialHarbor SHA-256：`CE12DC16EBF33960C24F18208C821FCA4A357F147D8501007D719B40712865DC`；D19 R02 Integration Prefab SHA-256：`5051EBAC9703E215F52AFBF7ACCEE3CA6F51BD0EFC1A7B4B7DA1477DF181AF14`。
- A04 Builder 最新源文件加入了项目自有 facade/building visual 层，并生成 `D19_R02_A04_Player_RERUN6_AUTHORED3.exe`；没有改玩法 Systems、Registry、Save Schema、contracts、D20 入口或资源状态。
- 只读 intake 审计：29 个 manifest、1,894 个候选文件、1,674 个 `intake_eligible` 文件、0 个错误；`LocalDevelopmentReady=0`、`ReleaseApproved=0`。结果见 [intake audit](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_INTAKE_AUDIT_RERUN8_AUTHORED.json) 和 [manifest audit](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PACK_MANIFEST_AUDIT_RERUN8_AUTHORED.json)。

### 真实 Player 路线

- 最新干净会话 `D19R02Route300` 从主菜单新游戏进入 InitialHarbor；B 交易页、T/N 航程推进、L 返港结算、E 上下船、F5 保存均由真实键鼠触发，Player.log 记录航线、海上事件、抵达、结算和 Ship→Character 控制切换。
- 同一 Player 会话在港口保持至少 300 秒，未崩溃；退出后 Continue 再次加载，日志出现“存档加载成功，进入游戏”，重载后有 D19_TerrainRoot 落地日志。
- 仍未证明仓库、酒馆、酒馆 NPC 以及从出生点通过真实 WASD 连续走到码头/泊位的路线，所以完整路线门禁仍为 PARTIAL。逐节点证据见 [RERUN8 route log](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN8_ROUTE_LOG.json)，原始日志见 [RERUN8 Player log](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN8_PLAYER_300_ROUTE.log)。

### 真实性能

- Low/Medium/High 均由真实 Windows x64 Player 按 `1920×1080` 启动参数、`-quality 0/1/2` 请求运行，warm-up 30 秒、采样窗口请求 60 秒；原始探针提供 FPS、最低 FPS、平均帧时间、主线程、GPU 帧时间、GC 和峰值内存。
- 真实结果：Low 379 samples / 59.777 FPS / min 13.487 FPS；Medium 591 / 59.921 / min 23.852；High 343 / 59.857 / min 18.893。探针没有写出实际 backbuffer、VSync、运行时质量回读或 1% low，Computer Use 回读为逻辑 `1099×648`，因此性能门禁仍不能写 PASS。详见 [RERUN8 performance](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_PERFORMANCE_1080P_RERUN8_AUTHORED.json)。

### 视觉与回归

- 最新 Player 帧未观察到粉色缺材质或大面积纯白面，但仍清楚看到平坦棕/橙地表、单一蓝色水带、简化块状植被和不可读的港口建筑轮廓；底部路线/控制叠字仍可见。固定镜头新鲜数量 `0/14`，天气变体 `0/3`，所以视觉门禁失败，不以静态 Renderer 数量替代玩家镜头。详见 [RERUN8 visual QA](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_VISUAL_QA_RERUN8_AUTHORED.json)。
- D19 EditMode `34/34`、D19 PlayMode `8 PASS/0 FAIL/1 SKIP`、D17 PlayMode `7/7`、D18 相关 Python 单测 `11/11`；intake/manifest 审计 PASS，`git diff --check` 无 whitespace error。机器总结果见 [RERUN8 machine result](../../../../game/tools/results/D19/D19-R02-S02/D19_R02_S02_RERUN8_MACHINE_RESULT.json)。

### 当前结论

**PARTIAL**。300 秒时长、航程/保存/Continue、真实三档帧数据和静态回归已补证，但完整仓库/酒馆/NPC/步行路线、14 个固定玩家镜头、天气变体、真实 1920×1080 backbuffer/质量证明以及港口视觉灰盒债务仍未闭合。`D20-S02-R01` 继续禁止启动；D18-S03、D21+ 未执行。
