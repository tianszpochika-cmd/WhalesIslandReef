# LOCAL-QUALITY 看板

更新时间：2026-09-16

| 任务 | 状态 | 结论/证据 | 下一步 |
|---|---|---|---|
| Q00 | `VERIFIED_WITH_SCOPE` | 2026-09-12 Q03-A14 接管复核：`9878` status lines（439 tracked changed、9439 untracked），HEAD/分支未变；Pipeline 只读采样确认 D2 root/scale/ship/helm 事实，Q02 `8/8` + D17 `16/16`；headless/no-GPU 未截图，不升级视觉 | 保持 Q03/Q20/VC-G01 视觉边界；Q03-A14-RUNTIME 需要隔离 Player 控制面 |
| Q01 | `VERIFIED_WITH_SCOPE` | 2026-09-12 当前根因/尺度复核：D2 root 单位缩放、CharacterController `0.2424/1.8/0.85`、水位 `-0.020`、船根 `2.573956`、D7 helm owner 均实读；四项视觉缺陷仍开放 | Q04/Q05 可继续单一 owner 代码卡；正式 Player/视觉仍受 Q03=`BLOCKED_ASSET` 与 VC-G01 控制 |
| Q02 | `VERIFIED` | 2026-09-12 当前 Pipeline：D2 动画 EditMode `8/8 PASS`、D17 Action Quality `16/16 PASS`（50.13s）；D2 是 visualRoot 唯一提交者；不代表 Player 视觉通过 | Q03：动作资源与姿态质量门槛；保持 `BLOCKED_ASSET` |
| Q02-A | `VERIFIED` | 当前源码 D17A05 契约通过，D7 只测量并把落地偏移交给 D2；运行态 `DirectVisualRootWriteCount=0` | Q03：检查正式动作资源/姿态质量 |
| Q03 | `BLOCKED` | `BLOCKED_ASSET`：D6 clip 是绝对 imported Euler 曲线，D20 资源原始 clip 是 generic path；候选 builder 已展开 R02 visual wrapper、将 4 个 male-peasant outfit renderer 重绑定到同一 Armature（candidate 共 7 个 skinned renderer），并用 generic path adapter 消费曲线，Pipeline PlayMode `1/1 PASS`、D20-A02 EditMode `6/6 PASS`，原生 Player 安装日志 `generic path adapter=True`；candidate bridge 另将 D2 Swimming 映射到现有 Fish fallback 并通过 `SwimmingVisualSupported=true`，但最新截图仍为低模/抬臂/T-like 姿态且有重复 fallback 人物/港口层级风险，RERUN85 现有探针未产出 movement/swim/ship 观测；formal consumer=0、FormalAnimatorReady=false | 提供当前 rig 合法动作资源/正式适配器和正式游泳表现契约，满足 D20/D19 集成门禁后复开；保持 Q03 `BLOCKED_ASSET`、Q08 `BLOCKED_PREREQUISITE`、VC-G01 `IN_PROGRESS` |
| Q03-A01 | `PARTIAL` | 程序化 fallback `swimBodyDrop -0.88m→-0.65m`；定向 Python `50/50`、D17 A08 `1/1`、D17 全量 `15/15`、全量 Python `1134/1134`；R25 原生路线 `48` 输入/19 截图/167 observations/123 swimming/22 Ship control；正式泳姿仍未通过 | 提供正式动作资源或 Avatar/骨骼轴适配器；不升级 Q03/VC-G01 |
| Q03-A02 | `VERIFIED_WITH_SCOPE` | D20 candidate 移动首帧映射阈值修复：真实 D2 `Action=Walk`、`snapshotSpeed01=0.041` 不再被误判为 Idle；D20 EditMode `6/6`、Q03 targeted PlayMode `1/1`、全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`，recompile 无错误、error console `0` | 仅关闭 candidate 表现映射/全量确定性缺陷；Q03 继续 `BLOCKED_ASSET`，下一卡 Q03-A03 处理 D27 恢复旧 Renderer 与 candidate 所有权冲突 |
| Q03-A03 | `VERIFIED_WITH_SCOPE` | D27 检测 D20 candidate ownership 后跳过旧 player Renderer restore；candidate targeted `1/1 PASS`、日志 `candidateOwned=true/restored=0`，全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`，formal fallback `candidateOwned=false/restored=0`，recompile/error console 均干净 | 仅关闭重复人物/Renderer 所有权冲突；Q03 继续 `BLOCKED_ASSET`，下一卡重建最新 candidate Player 做原生登船/游泳/重复人物截图复核 |
| Q03-A04 | `PARTIAL` | 独立 Q03-A04 candidate Player 构建成功；真实 Win32 RERUN95 `48` 输入/19 截图/166 observations/123 swimming/22 ship-control，船体位移 `6.058244m`、航向 `18.174286°`，WASD/转向/E/I/M/T/Esc/业务/暂停矩阵有证据；截图确认低模 T-like 动作、Fish 抬臂和港口几何风险 | 不把输入闭环升为视觉 PASS；Q03 继续 `BLOCKED_ASSET`、VC-G01 `IN_PROGRESS`；下一卡 Q03-A05 只改候选 Fish clip arm excursion并用 RERUN96 复核 |
| Q03-A05 | `PARTIAL` | Candidate Fish upperarm 生成幅度 `30°→8°`，曲线边界与资源重建通过；D20 EditMode `7/7`、Q03 candidate PlayMode `1/1`、全量 EditMode `620/620`、全量 PlayMode `99 passed / 0 failed / 1 skipped of 100`、Python `1152/1152`；RERUN96 `48` 输入/19 截图/167 observations/120 swimming/21 ship-control，船体位移 `6.058783m`、航向 `18.267059°`，但原生游泳仍蜷曲/抬腿，登船舵轮接触和港口几何仍不合格 | 保持 Q03 `BLOCKED_ASSET`、VC-G01 `IN_PROGRESS`；下一卡 Q03-A06 处理 candidate Fish whole-body orientation/pose，不改物理根或正式资源状态 |
| Q03-A06 | `PARTIAL` | Candidate Fish visual-child root pitch `32°` 已进入生成 clip/controller；D20 EditMode `8/8`、Q03 candidate PlayMode `1/1`、Python `6/6`、recompile/error console 干净；RERUN97 `48` 输入/19 截图/166 observations/123 swimming/21 ship-control，船体位移 `6.051456m`、航向 `18.186615°`，但原生身体仍前俯、抬腿并悬空，登船舵轮接触和港口几何仍不合格 | 保持 Q03 `BLOCKED_ASSET`、VC-G01 `IN_PROGRESS`；Q03-A07 先做 Fish 运行时姿态应用链只读诊断，不继续盲调角度 |
| Q03-A07 | `PARTIAL` | Unity Pipeline 确认根因是 generic clip 只写大腿 X 轴，R02 休息 Y/Z `180/180` 被 Animator 默认值归零，脚从约 `y=0.08` 抬到约 `y=1.68`；现已关闭 write defaults 并显式写 root/limb XYZ，D20 EditMode `9/9`、Q03 candidate PlayMode `1/1`、全量 EditMode `622/622`、Python route `6/6`、controller audit `PASS`、error console `0`。RERUN99 `48` 输入/19 截图/167 observations/120 swimming/21 ship-control` 去除抬腿折叠但仍斜；RERUN100 使用 candidate visual-child `90°` pitch，真实 `48` 输入/19 截图/166 observations/109 swimming/22 ship-control`，移动/游泳/船控/业务/暂停证据成立，但岸线穿插/悬浮、低模动作、舵轮接触和港口几何仍不合格 | 保持 Q03 `BLOCKED_ASSET`、VC-G01 `IN_PROGRESS`；停止继续盲调 Fish 角度，选择 D31-P04/D31-S05/D31-G06 中一个环境接地 owner 做有界诊断；远船 reject 与 owner-not-ready reject 仍未覆盖；全量 PlayMode/Python 未在本卡后重跑 |
| Q03-A08-D31-P04 | `PARTIAL` | R03 海岸垂直切面根因已定位并修复：每段改为内边界→1.35m 沙质 shelf→水下深边界；Q03-A08 `1/1 PASS`，Q20-Q23 `1/1`，Q24 `3/3`，Q25 `2/2`，Q26 `10/10 PASS`；生成 mesh `2884` vertices，唯一 terrain owner 保持；Pipeline GameView 明确因 batchmode/headless 无 GPU 阻塞，未宣称 post-fix visual PASS | GPU-capable Player/Editor 补海岸路线截图；保持 D31-P04/VC-G01 `PARTIAL/IN_PROGRESS`，再按依赖选择下一 owner |
| Q03-A10 | `PARTIAL` | D2 fallback 游泳手臂局部轴根因已由运行态采样复现：64° visualRoot pitch 与局部 -Y 中性臂叠加使旧角度下垂；改为 fallback-only 上臂 150°/前臂 30° 显式前伸。D2 EditMode `8/8`、D17A08 `1/1`、D17 全量 `16/16`；Player `build_b4b4570bffeb`；RERUN100 archive `49/49` 输入、19 截图、164 observations、120 swimming、21 ship-control。截图显示手臂前伸改善，但低模比例、腿脚/泳姿、舵轮接触、岸线/港口几何仍不合格 | 停止继续盲调游泳角度；Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`；下一步选择正式 Avatar/动作、舵轮接触或港口接地/几何的单一 owner |
| Q04 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A07 十循环 `PASS`（8.4459547s）；水位、位移和 grounded recovery 通过；不代表 Player 键盘/视觉通过 | Q05/Q08：游泳姿态与代码整合 |
| Q05 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A08 游泳 visualRoot 倾斜/物理根 upright/neutral 恢复 `PASS`（8.1784901s）；正式四肢动作和原生视觉仍受 Q03 `BLOCKED_ASSET` 约束 | 进入 Q06/Q08 代码级整合；保留 Q03/Q31/VC-G01 视觉边界 |
| Q05-A01 | `PARTIAL` | D2 fallback 收窄游泳摆幅并移除横向 roll；静态 `53/53`、全量 Python `1141/1141`、D17A08 `1/1`、R28 真实路线 `48` 输入/19 截图/167 observations/123 swimming/22 Ship control；截图仍是低模直立/碎片化泳姿，不升级 Q03/Q31/VC-G01 | 等待合法动作/Avatar 资源或明确的 candidate-only 视觉集成卡；停止盲调角度 |
| Q05-A02 | `PARTIAL` | 程序化 fallback 前臂从叠加 `-80°` 折叠改为有限 `-18°` 子节点弯曲；定向 Python `56/56`、全量 Python `1143/1143`、Q26 水域专项 `1/1`；R29 真实路线 `48` 输入/19 截图/168 observations/37 swimming/22 Ship control；截图仍未达到正式泳姿 | 保持 Q03 `BLOCKED_ASSET`；不把前臂源修复当作最终视觉通过 |
| Q05-A03 | `PARTIAL` | D7 清除游泳时过期 grounded 视觉偏移、更新舵位校准；Q26 `6/6`、D17 `15/15`、全量 Python `1143/1143`；R31 真实路线 `48` 输入/19 截图/164 observations/122 swimming/21 Ship control；截图仍显示低模游泳/握舵/港口植被视觉缺陷 | 解除 Q03/Q20 资源门禁或建立明确 candidate-only 视觉集成卡；不升级 Q31/VC-G01 |
| Q06 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A09 十跳跃/小步移动 `PASS`（9.7554984s）；障碍/岸线/跳板仍未收口 | Q07：岸线、坡面、跳板与拒绝路径 |
| Q07 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A10 活动地形/码头/跳板双向小步 `PASS`（2.3071401s）；完整三岸点/十次往返/视觉仍未收口 | Q08 受 Q03 阻塞；Q09 前置已满足 |
| Q08 | `BLOCKED` | Q03=`BLOCKED_ASSET`，缺当前 rig 合法地面动作资源/可审计适配器；不伪造输入→姿态整合 PASS | 解锁 Q03 后重跑；不阻止 Q09 |
| Q09 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A11 非单位船根锚点、10 次上下船和 30m 远距拒绝 `PASS`（2.2906652s）；Player 视觉/键盘未覆盖 | Q10：航行中甲板相对关系 |
| Q10 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A12 10 个航行快照相对甲板误差 `0.000000`（1.6780672s） | Q11：合法/非法安全下船与往返恢复 |
| Q11 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A13 10 次合法下船/无落点拒绝 `PASS`（2.269023s） | Q12：相机遮挡与模式恢复 |
| Q12 | `VERIFIED` | 2026-09-12 纳入当前 D17 全量：`16/16 PASS`，其中 D17A14 10 轮相机/遮挡/恢复 `PASS`（2.4606404s）；临时障碍不替代最终视觉门禁 | Q13：输入焦点、Esc/模态与船控释放 |
| Q13 | `VERIFIED` | 2026-09-12 当前 D17 全量：`16/16 PASS`，其中 D17A15 W/S-HUD/Esc 模态归属 `PASS`（2.7867954s）；D16 全 fixture 相邻 NaN 仍公开 | Q14：行走→入水→上岸→登船→转弯→拒绝/合法下船整合 |
| Q13-A01 | `VERIFIED` | 玩法态显式清空 `InputSystemUIInputModule.move`、模态态恢复；D17-A15 `1/1 PASS`，D17A06 主菜单生命期 `1/1 PASS`；Unity Pipeline R7 隐藏 Player 已验证 W/S/A/D、Esc 模态归属和恢复，但 `simulate_key` 不等于物理键盘，视觉仍不覆盖 | 保持 VC-G01 `IN_PROGRESS`；继续用 Pipeline 做有界控制面回归，视觉/物理键盘另行取证 |
| Q13-A02 | `PASS_WITHIN_SCOPE` | `HarborInteractionStabilizer` 重新检查 `InputSystemUIInputModule.move`，外部恢复 `UI/Navigate` 后游戏态再次清除；定向 D16 导航 `2/2 PASS`，D17 全量 `16/16 PASS`，当前 D16 全 fixture 连续两次 `10/10 PASS`；recompile/Editor/error console clean，历史 NaN 失败保留为 watch item | Q14；不升级 native Player 或 VC-G01 视觉门 |
| Q14 | `VERIFIED` | 2026-09-12 当前 D17 全量：`16/16 PASS`，其中集成 D17A16 船岸/入水/转向/拒绝/下船链 `PASS`（1.5996333s）；imported rig 动作质量仍由 Q03 `BLOCKED_ASSET` 限定 | Q15：仓库/市场/酒馆焦点与关闭恢复 |

## 2026-09-12 Q03-A12 资源契约审计追加

Unity Pipeline 已实读并验证 D18 R03 project-owned male Avatar：`isValid=true`、`isHuman=true`、20 human bones、70 skeleton entries；D20 R02 formal appearance 具有单一有效 Humanoid Animator；D20 controller 具有 15 个动作状态和 6 个参数；Resources candidate 的 generic-path 曲线全部解析到同一直接 `Armature` 层级。新增资源契约 EditMode=`2/2 PASS`，D20-A02 EditMode=`9/9 PASS`，Q03 candidate PlayMode=`2/2 PASS`，recompile=`failed=false/errors=[]`，Editor=`ready/stopped`，error console=`0`。

这只证明“候选资源契约存在并可被隔离 consumer 消费”，没有把 candidate bridge 晋级为正常 InitialHarbor consumer。当前 formal consumer 仍为 `0`，正常 Player 仍 `FormalAnimatorReady=false / PoseDriverMode=Procedural`，故 Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。权威结果：[Q03-A12 RESULT](game/tools/results/LOCAL-QUALITY/Q03-A12/RESULT.json)；交接：[Q03-A12 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A12/Q03-A12.md)。下一步为 Q03-A13：单一正式 D2 presentation consumer 接入、连续动作过渡和 exact-build 原生视觉验收。

## 2026-09-12 Q03-A13 显式 formal consumer 逻辑闭环追加

利用已连接的 Unity Pipeline，Formal runtime Resources 已生成到 `Assets/Resources/D20/Formal/`；D20 presentation bridge 增加显式 `InstallForFormal` 与 `-d20Formal` 路由，默认 InitialHarbor 仍不启用。正式 consumer PlayMode=`1/1 PASS`，覆盖 source Avatar 预检、generic-path adapter、W/Shift 驱动 `Idle→Walk→Run`、Animator 不移动 D2 physical root、`Board→Disembark` 和 unsupported action=`0`。

回归证据：资源契约 EditMode=`2/2 PASS`、D20-A02 EditMode=`9/9 PASS`、Q03 candidate PlayMode=`2/2 PASS`、D17-A06 quarantine PlayMode=`1/1 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；清空后 error console=`0`。本卡的 Builder HTTP 5 秒超时由 Unity Console 的实际完成日志确认，未把 timeout 单独当成功证据。

本卡只关闭 explicit formal consumer 的代码/逻辑边界，不关闭正式视觉质量：没有 exact-build Windows Player 截图，低模动作、游泳、握舵接触、港口接地/穿模与树木漂浮风险仍公开。因此 Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。权威结果：[Q03-A13 RESULT](game/tools/results/LOCAL-QUALITY/Q03-A13/RESULT.json)；交接：[Q03-A13 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A13/Q03-A13.md)。下一步为 Q03-A14：构建独立 `-d20Formal` Windows Player 并做真实输入、日志、截图验收，再讨论默认 consumer admission。

## 2026-09-12 Q03-A14 Player 构建追加

Unity Pipeline build=`build_b70c492ca267` 已生成独立 `StandaloneWindows64` Player：`MainMenu + InitialHarbor`、`Succeeded`、总产物 `175167322` bytes、`0 errors / 8 warnings`，下一次运行显式使用 `-d20Formal`。EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，结果见 [Q03-A14 RESULT](game/tools/results/LOCAL-QUALITY/Q03-A14/RESULT.json)，交接见 [Q03-A14 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A14/Q03-A14.md)。

本卡只证明 Player 可构建；没有启动前台 Player、没有物理键盘输入、没有截图。当前 Pipeline 为 headless/no-GPU Editor，Win32 路线若自动执行会抢占用户桌面，因此不把构建成功扩大为 VC-G01 或 Q03 视觉通过。Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。下一步为 `Q03-A14-RUNTIME`：获得隔离原生 Player 控制面，或得到用户明确的前台 probe 授权。

## 2026-09-12 Q00/Q01 接管复核与 Q02/Q04 当前源码回归

- Q00 当前节点：`game/tools/results/LOCAL-QUALITY/Q00/RESULT.json` 的 `current_reaudit_20260912_q03a14`。工作区 `9878` status lines（`439` tracked changed、`9439` untracked），未清理；Pipeline 采样确认 `InitialHarbor`、D2 单位物理根、`ph_ship_fishing` scale≈`2.573956`、D7 helm 锚点和默认 `Procedural/FormalAnimatorReady=false`。
- Q01 当前节点：`game/tools/results/LOCAL-QUALITY/Q01/RESULT.json` 的 `current_reaudit_20260912_q03a14`。根因/owner/容差复核完成；动作、游泳、舵轮、树木四项用户缺陷都没有被 headless 采样升级为视觉通过。
- Q02 当前证据：[Q02 current result](Q02/Q02_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)，D2 动画 EditMode=`8/8 PASS`、D17 Action Quality=`16/16 PASS`（50.13s）。
- Q04 当前证据：[Q04 current result](Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)，同一 D17 全量中的 D17A07 水域十循环=`PASS`（8.4459547s）。

这些结果只关闭当前源码/逻辑回归不确定性，不关闭 Q03 formal Player 原生输入和视觉证据。Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。
| Q15 | `VERIFIED` | 2026-09-12 当前源码 Pipeline：D29 Q15 `1/1 PASS`（13.53s）；I/M/T 各 10 轮打开/重复/关闭/恢复、互斥焦点和 Gameplay 恢复通过；不覆盖原生窗口鼠标/视觉 | Q16：交易/业务状态与错误反馈 |
| Q16 | `VERIFIED` | 2026-09-12 当前源码 Pipeline：D29/D3 交易桥 `1/1 PASS`（2.24s）；owner 未就绪、双击单 owner、买卖一次变化、余额不足/满仓不变均通过；不覆盖原生鼠标/视觉 | Q17：船坞与整备桥 |
| Q17 | `VERIFIED` | 2026-09-12 当前源码 Pipeline：D29/D9/D4 整备与船坞 `1/1 PASS`（3.07s）；成功、材料不足、重复/幂等、取消、资金不足、异常恢复均通过；不覆盖原生鼠标/视觉 | Q18：HUD 与本地化尺寸矩阵 |
| Q18 | `PARTIAL` | 2026-09-12 当前源码 Pipeline：D31Q18 分辨率矩阵 `3/3 PASS`，D31 Visual Logic `36/36 PASS`，Python D29 HUD `30/30 OK`；原生截图、字体栅格、英文最长文案和鼠标命中仍待验 | Q19：船员与反馈审计；Q18 原生视觉缺口留待 Player 控制面 |
| Q19 | `VERIFIED` | 2026-09-12 当前源码 Pipeline：离线 crew/save 审计 `1/1 PASS`，Save.Tests 全程序集 `376/376 PASS`，AgentCore `0 warnings/0 errors`；仅在本卡范围内 VERIFIED，不解锁在线 LLM、native Player 或视觉门禁 | Q20；继续 formal resource/presentation 审计 |
| Q20 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：候选 bounds `1/1 PASS`，Q26/A03 legacy containment `1/1 PASS`（activeLegacyTreeRenderers=0、activeR03Vegetation=24、unsupported=0）；`RenderAuthoredPalms=false`、Prefab 无棕榈、D18 ready/eligible=`0/0`，仍 `BLOCKED_ASSET`；PlayMode 捕获 FMOD 输出设备 Error seq `21805`，已单独登记为环境边界 | 等待 ready 资源并处理/复核音频设备环境；Q20-A01/A02 仅为 containment，不升级完整植被或视觉门禁 |
| Q20-A01 | `PARTIAL` | D26 从场景根扫描扩展到 Transform 子树并隔离 `D19_SaintMargaret_Root` 下全部 Renderer；定向 Python `50/50`、全量 Python `1139/1139`、Q26/A03 PlayMode `1/1`；R26 真实路线 `48/48` 输入/19 截图/167 observations/123 swimming/22 Ship control，船体位移 `5.7648530m`、航向 `16.1363525°`；active legacy tree Renderer `0`，正式树资源仍未准入 | 等待正式树/棕榈 ready 资源；不把 containment 当视觉 PASS |
| Q20-A02 | `PASS_WITHIN_SCOPE` | D26 追加直挂/嵌套 `ph_palm_tree` 遗留树 Renderer quarantine；Unity Pipeline 重编译 `failed=false/errors=[]`，Q26/A03 `1/1 PASS`，Q26 全类回归 `6/6 PASS`，日志 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`，error console `0` | 只关闭 legacy placeholder 表现路径；Q20 仍 `PARTIAL/BLOCKED_ASSET`，等待正式树/棕榈 ready 资源和 Player 视觉证据 |
| Q21 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：R03 静态服务空间 `1/1 PASS`；4 建筑各 `37 Renderer/0 missing`，6 空间组、23 colliders/1 R03 terrain/22 D27 owner、无 pairwise XZ 重叠；以 seq `21868` 为基线无新增 Error，Q20 FMOD seq `21805` 作为历史环境边界保留 | Q22；原生船海/天气、Player 路线/镜头/鼠标与最终视觉仍未覆盖 |
| Q22 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：R03 水面/船体水线/D28 天气 owner 审计 `1/1 PASS`；水面 `96×96`、foam `28`、船壳底线 `-0.0400`、可见船体 `0.0600`、鱼类活动/降水/波动指标均有日志；seq `21878` 后新增 Error/Warning=`0` | Q23；原生船海/游泳/音效/性能与最终视觉仍未覆盖 |
| Q23 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：InitialHarbor R03/船正式集成 `1/1 PASS`；active roots=34、R03/船=1/1、R03 renderers=361、missing material/script=0、terrain collider=1，GUID 和重开一致；seq `21890` 后新增 Error/Warning=`0` | Q24；逐物体接地、原生视觉、输入和性能仍未覆盖 |
| Q24 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：陆地/出海/坏档保护 `3/3 PASS`；船位 `(41.5,-17.25)`、航向 `212`、速度 `2.25`、上一份好档保护均有证据；schema 仍无独立游泳状态/姿态；seq `21900` 后新增 Error/Warning=`0` | Q25；首航/循环集成，原生 Player 门禁仍未覆盖 |
| Q25 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：D30/D23 首航诊断 EditMode `2/2 PASS`（0.49s），InitialHarbor route PlayMode `2/2 PASS`（5.42s）；无新增 Error，但保留 12 条 D27/D2 已知 Warning，D31 performance approval 仍 pending | Q26；真实帧采样/性能基线，随后构建与最终 Player 门禁 |
| Q26 | `PARTIAL` | 2026-09-13 当前源码 Unity Pipeline R1：`D19R03Q26GameplayDefectPlayModeTests`=`17/17 PASS`（41.2s），覆盖 Q26A01/A02/A03/A04/A05/A06/A07/A08/A09/A10/A11/A12/A13/A14/A16/A17/A18；独立 A01=`1/1 PASS`（3.75s，W→swimming/action=Swimming、planarDistance=`0.4095m`、surfaceY=`-0.020`）。完整 error query=`0`，warning query 返回 API 上限 `100`，独立 A01 窗口为 `0 Error / 5 Warning`；Editor `ready/stopped`。日志仍明确未验证蒙皮、程序化 Player fallback、D31 runtime performance pending | 保持 Q26 `PARTIAL`；下一步 Q27 性能/预算证据，不能把 Pipeline PlayMode 结果扩大为 native Player 物理键盘、GPU 截图、正式 Avatar/动作、目标设备性能或 VC-G01 视觉 PASS；结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json` |
| Q27 | `PARTIAL` | 2026-09-13 当前源码 Pipeline R1：D16 预算 EditMode=`6/6 PASS`（0.04s），连续性能 PlayMode=`1/1 PASS`（64.0s）；实际窗口 `60.004829s / 163 samples`，探针 owner=`D16WorldPerformanceController`、`BudgetSatisfied=true`。当前 `Null Device` 平均 `30.7857 FPS`、平均帧 `155.6439ms`、最长帧 `1103.0729ms`、90 帧超过 50ms、GPU unavailable；既有 standalone R3 仍为 `60.014019s/3465 samples`、平均 `58.734367 FPS`、1% Low `21.460548 FPS`、最长帧 `622.258118ms`。本轮只记录真实 headless 风险，不做无证据 D16 优化 | 下一步 Q28 新候选 Player，随后 Q29 自动化路线；不得将 Null Device/standalone 基线扩大为性能或视觉 PASS；结果见 `game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json` |
| Q28 | `PARTIAL` | 当前源码通过 Pipeline dry-run（`valid=true`）并生成新的 Q28-CANDIDATE-R4：`build_81547e384e7e`、`StandaloneWindows64`、`MainMenu + InitialHarbor`、`Succeeded`、`0 errors / 1 warning`、总产物 `175167322` bytes；EXE/Assembly 哈希已登记，未覆盖旧候选。没有启动 Player、物理键盘或截图；Pipeline warning 明确 Player 内不启用 Pipeline，Q03/Q20 与视觉风险保持开放 | Q29 使用 R4 候选做真实路线；若无隔离 Player 控制面，记录具体阻塞并继续离线审计；Q30 独立审查 |
| Q29 | `PARTIAL` | Q29 R4 真实 Win32 路线仍保留：`48/48`、`19` 截图、`167` observations，78 个 gameplay W/A/S/D 样本均 `navigation=false/move=null/selected=null/suppressed=true`，0 次 UI 冲突；2026-09-12 Pipeline R3 hidden GUI Player 新发现并修复 D29 `T=酒馆` 与 D3 `T=出航` 冲突，R3 `T→InPort/酒馆`、`M→市场`、`Y→Sailing`、W/E/I/Esc 实际状态均成立；无截图视觉 PASS，低模/泳姿/握舵/港口几何风险仍在 | 保持 Q29 PARTIAL；选择单一视觉 owner（正式动作/Avatar、游泳表现或港口接地/层级）做下一候选；随后 Q31 最终 Player 门禁；权威结果见 `Q29/RESULT.json` |

## 2026-09-14 Q29 Pipeline R7 当前源码隐藏 Player 交互复核

- `build_37e34d20a804` 生成当前源码 `Q29_RUNTIME_PIPELINE_QA_R7.exe`：`StandaloneWindows64`、Development + DetailedBuildReport、`Succeeded`、`0 errors / 0 warnings`、`220872966` bytes；EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`，Assembly-CSharp SHA256=`6EBD285D9A70A88996E4E1D6CACD11EA04C4A880BF6A46A999DA6A2761DB822C`。
- 通过 Runtime Pipeline `127.0.0.1:7902` 控制隐藏 Player：MainMenu NewGame/Confirm、W、I 仓库与模态 W 阻断、M/T 重复与 Esc、E 上下船/再次登船、Y 出航、航行中 M owner 拒绝、船上 W/A、Esc 暂停/恢复均取得只读状态 marker；没有接管用户前台桌面。
- Player runtime `console --level error` 返回 `0`；Player log 大小写敏感 `ERROR|EXCEPTION|CRASH|FATAL=0`；runtime `quit` 后 PID/descriptor 消失。一次引用 `UnityEngine.InputSystem` 的 eval 编译失败属于 probe 参数边界，已排除在产品 verdict 外。
- QA 后已回读 `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false / connectWithProfiler=false`，Editor=`ready/stopped`。本轮为 hidden Pipeline 输入/状态 `PASS_WITHIN_SCOPE`，Q29 父状态仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`；无截图、前台物理键盘、目标设备性能、正式动作/Avatar 或最终视觉结论。
- 结果：[Q29 R7 result](Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；capture：[Q29 R7 log](Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q29](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。
| Q30 | `PARTIAL` | 当前候选独立复核：Python 定向 `75/75`、全量 `1144/1144`；Q28-CANDIDATE-R3 构建与 Q29 两条 Win32 路线可复现（水域 `48/48` 输入、19 截图、166 observations、124 swimming、21 Ship control；远距 `45/45`、17 截图、123 observations、81 `ship_too_far`），截图仍暴露低模人物、游泳/握舵接触和港口几何/接地风险 | Q31 最终 Player 视觉门禁；Q03/Q20 资源阻塞继续公开；owner-not-ready 继续引用独立证据 |
| Q21-A01 | `PARTIAL` | R03 四个服务建筑改为按 `Height(x,z)-0.02m` 设置 foundation root；定向 Python `55/55`、R03 rebuild PASS、R17 非开发版 Player 构建 PASS；真实路线 `48/48`、19 截图、167 observations、123 swimming、22 Ship control；截图仍显示低模动作/游泳、登船镜头和植被/港口层级风险 | 继续拆动作、游泳、登船镜头、植被 owner 卡；不升级 Q31/VC-G01 |
| Q21-A02 | `PARTIAL` | ShipDeck 相机从 `10.5m/22°/1.45m` 收紧为 `7.5m/18°/1.05m`；定向 Python `42/42`、R18 非开发版 Player 构建 PASS；真实路线 `48/48`、19 截图、168 observations、123 swimming、22 Ship control；人物与舵位可读性改善但低模动作/游泳、港口层级和植被仍未收口 | 继续拆动作/游泳/植被 owner 卡；不升级 Q31/VC-G01 |
| Q21-A03 | `PARTIAL` | fallback 游泳 stroke/取景最小调整；定向 Python `80/80`、R20 非开发版 Player 构建 PASS；真实路线 `48/48`、19 截图、167 observations、123 swimming、23 Ship control，船体位移 `5.6351428m`、航向 `14.6616211°`；低模动作/泳姿、舵位可读性和正式植被仍未收口 | Q31 当前候选改为 R20；不升级 VC-G01 |
| Q21-A04 | `PARTIAL` | D19 presentation-only 轮舵从 R21 开放圆柱、R22 封口、R23 移出遮挡收紧到 R24 轮圈/四轮辐/轮毂；定向 Python `85/85`、R24 非开发版 Player 构建 PASS、Q26 舵位 PlayMode `1/1 PASS`；真实路线 `48/48`、19 截图、167 observations、123 swimming、22 Ship control，船体位移 `5.6047745m`、航向 `15.3561096°`；轮舵可读性改善，但低模动作/泳姿、人物手部关系、港口和正式树资源仍未收口 | 保留 R24 历史证据；不再盲调舵位坐标，按复现问题选择动作/游泳/植被 owner |
| Q21-A05 | `PARTIAL` | D2ControlRouter 现在通知 fallback 进入 Ship 操舵姿态；R1/R2 失败诊断保留，R3 Q26/A05 PlayMode `1/1 PASS`，定向 Python `57/57`、D17 `15/15`、全量 Python `1141/1141`；R27 真实路线 `48/48`、19 截图、168 observations、124 swimming、21 Ship control，船体位移 `5.8478036m`、航向 `17.3246765°`；轮舵局部改善，但低模动作/泳姿、自然握舵、漂浮/穿模、港口和正式树资源仍未收口 | 保留 R27 为当前 Q31 候选；按单一 owner 处理 Q03 动作/Avatar 或游泳/植被/空间层级缺陷 |
| Q21-A06 | `PARTIAL` | D2 fallback 舵轮姿态加入左右 `-10°` 前臂弯曲；Pipeline 运行时证明玩家位于 D7 HelmPoint、双手在轮圈附近；定向 Python `12/12`、全量 Python `1152/1152`、Q26/A05 `1/1`、EditMode `619/619`；新 Player `RERUN94` 为 `46/46` 输入、17 截图、122 observations、60 swimming，业务/暂停/I-M-T-Esc/远船拒绝/clean close 有真实证据；全量 PlayMode 实际 `98/100`，Q03 W→Walk 失败、D19 terrain 预期跳过；截图仍未达到自然握舵/正式人物视觉 | 停止继续调舵轮坐标/前臂角度；先处理 Q03 正式 Avatar/动作资源或新的港口几何/接地 owner，并解决 Q03 全量顺序失败；Q31/VC-G01 不升级 |
| Q31 | `PARTIAL` | 用户已实际进入 R31 Player 并否决任务动作、游泳、操舵关系、树木漂浮及 W/S-HUD 冲突；当前 Q28-CANDIDATE-R3 与最新 Q26/A10-R2 截图复核均未达到视觉门禁：路线可运行，但角色/泳姿/握舵/港口层级仍为风险；`VC-G01` 继续 `IN_PROGRESS` | 选择单一 owner 做正式动作/游泳/舵位/植被接地修复并补 bounded Player evidence；不把路线运行升级为视觉 PASS |
| Q32—Q35 | `PLANNED` | 不继承旧 PASS；Q32 固定镜头视觉门禁、Q33 D31 汇合、Q34 全需求覆盖审计、Q35 后续任务交接均等待 Q31 | 仅在 Q31 人工结果和 VC-G01 条件满足后推进 |

正式权威状态不由本看板覆盖：`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31 相关项保持 `PARTIAL`。

追加证据（2026-09-11 Q03-A08）：R03 重建后通过连接 Unity Pipeline 异步执行 Unity EditMode 全量 `623/623 PASS`，`failed=0 / skipped=0 / inconclusive=0`，耗时 `73.46s`；Editor 最终 `ready / playMode=stopped`，error console=`0`。这只加强 Q03-A08 的编辑器回归证据，不改变 D31-P04、Q20、Q26、Q31、VC-G01 的视觉状态。

追加证据（2026-09-11 Q26/A12）：Unity Pipeline 构建 `build_183e12159163` 使用 `MainMenu + InitialHarbor`，0 errors；完整 Player 路线 `46/46` 输入、17 截图、122 observations、36 moving、D10 owner-ready，水域路线 `49/49` 输入、19 截图、166 observations、124 swimming、21 Ship control，业务/暂停/I-M-T-Esc 矩阵和 clean close 均有真实证据。相机取景定向 `6/6`、Q26 PlayMode `10/10`、Python 全量 `1148/1148`、Editor error console `0`。ShipDeck 取景在有限范围内改善，但直立泳姿、低模角色、港口几何/层级/接地仍拒绝视觉通过；状态不升级，详情见 `game/tools/results/LOCAL-QUALITY/Q26/RESULT.json` 与 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-11 Q26/A16 R03 boundary bridge）：Unity Pipeline `build_319d0c427c0d` 生成 `Q26_A16_R03_BOUNDARY_CANDIDATE_R1.exe`，`MainMenu + InitialHarbor`、`0 errors/8 warnings`；R03 bridge 识别 D6-owned `ShoreBoundary.South/.West/.East` 外围集合并保持其在水域外，Player 日志记录 `route collision window retained: D6-owned R03 perimeter boundaries remain outside water`，legacy physical colliders disabled=`123`、non-blocking triggers preserved=`22`。真实 Win32 `RERUN88` 为 `46/46` 输入、17 截图、122 observations、36 moving，movement/turning/swimming/business/pause/far reject/interaction matrix/clean close 有证据，但本轮 `ship_control_proof=false`。Python 全量=`1150/1150 PASS`、Unity EditMode=`619/619 PASS`、Q26 PlayMode=`10/10 PASS`、VC-G01 world logic=`13/13 PASS`、定向 contract=`20/20 PASS`、error console=`0`。A16 初始港口帧仍有低模角色、侧建筑裁切和港口几何/接地风险，故 Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。详情见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A16_R03_BOUNDARY_CANDIDATE_R1_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-11 Q26/A17 InitialHarbor framing）：基于 A16 的真实截图与 Pipeline 空间测量，将 `CameraFollow` 的 InitialHarbor Character 首帧距离从 `13.5m` 调整为 `18.5m`，未改建筑位置、碰撞、D2/D7 控制、ShipDeck/游泳预设或 F3 输入；新增 `RERUN89` namespace。Pipeline build `build_d7a1d45e6e1e` 成功，`0 errors/8 warnings`；真实 Win32 `46/46` 输入、17 截图、122 observations、36 moving，movement/turning/swimming/business/pause/far reject/interaction matrix/clean close 有证据，`ship_control_proof=false`。Python 定向=`20/20 PASS`、全量=`1150/1150 PASS`，相机 PlayMode=`1/1 PASS`，VC-G01 world logic=`13/13 PASS`，Q26 PlayMode=`10/10 PASS`，Unity 全量 EditMode=`619/619 PASS`，error console=`0`。A17 首帧相较 A16 使中心路线露出更多地面、减轻左右建筑近景占屏，登记 `IMPROVED_WITHIN_SCOPE`；左下仍裁切，低模玩家、地面断缝/材质粗糙仍在，故 Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。详情见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A17_INITIAL_HARBOR_FRAMING_CANDIDATE_R1_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-11 Q26/A20）：CameraFollow 游泳镜头自动跟随 D2 heading，手动 mouse/right-stick look 时交接；有限值保护消除全量 PlayMode 实际暴露的 NaN camera pose；Q03 candidate-only 测试恢复 input/HUD 状态以避免顺序污染。Pipeline build `build_3bba5aaa08b8` 成功，MainMenu + InitialHarbor，0 errors/8 warnings；A20 RERUN92 WaterProbe 真实 Win32 `49/49` 输入、19 截图、167 observations、68 moving、119 swimming、21 Ship control，船体位移=`6.00153923034668m`、航向=`17.986114501953125°`，业务/暂停/交互矩阵/clean close 成立，fatal=null。Python=`1152/1152 PASS`，Unity EditMode=`619/619 PASS`，PlayMode=`99 passed/0 failed/1 skipped of 100`，Q03 targeted=`1/1 PASS`，error console=`0`。截图游泳姿态改善但低模角色、建筑/岸线/地面几何风险仍在；Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。权威结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A20_SWIMMING_CAMERA_FINITE_GUARD_CANDIDATE_R1_BUILD_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q26/RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-11 Q26/VC-G01 RERUN93）：为保留 A20 `RERUN92`，路线探针仅新增独立 `RERUN93` namespace；Pipeline build=`build_9d94c976be2d`，`StandaloneWindows64 / MainMenu + InitialHarbor / BuildOptions.None / 0 errors / 8 warnings`，Player/Assembly-CSharp 哈希已登记。真实 native Win32 正常路线 `46/46` 输入、17 截图、121 observations、39 moving、59 swimming；MainMenu→InitialHarbor、WASD/转向、E 登/下船、远船 E reject、I/M/T 重复、Esc 关闭/恢复、业务页、优雅退出均有探针/日志证据，`far_ship_reject_proof=true`、`interaction_matrix_proof=true`。本路线 Ship mode 仅观察到 6 个样本，未发送 WaterProbe 船控段，船位移/航向均为 0，`ship_movement_proof=false`；继续引用 A20 RERUN92 作为船控正证据。截图仍暴露低模/程序化人物、粗糙港口/岸线几何和 UI 遮罩风险；I/M/T owner-not-ready 窗口未覆盖，不推断为 PASS。`Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变。权威结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_G01_RERUN93_REAL_PLAYER_RESULT.json` 与 `game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-11 Q03-A09 当前源码 Player 与 probe archive）：仅改 `tools/vc_g01_runtime_input_probe.ps1`，为带 `EvidenceTag` 的路线新增独立 `probe_archive_path`，避免固定 `RERUN100` probe 被后续运行覆盖；PowerShell 解析=`PASS`，专用 Python 契约=`6/6 PASS`。Pipeline build=`build_cd0734bfe4bb`，`StandaloneWindows64 / MainMenu + Bootstrap + InitialHarbor / BuildOptions.None / 0 errors / 1 warning`；Player SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`A9072617F5B3432953027E93F43C8043787D04DDC74D62EE8CD8D693AC9E5064`。真实 Win32 WaterProbe=`49/49` 输入、19 截图、166 observations、69 moving、122 swimming、21 Ship control，船体位移=`5.9974632263m`、航向=`18.2392272949°`；移动/游泳/船控/业务/暂停/I-M-T 重复与 Esc/clean close 成立，`far_ship_reject_proof=false`，I/M/T owner-not-ready 未覆盖。实际截图仍是低模程序化人物、横向悬浮/四肢下垂泳姿、弱舵轮接触与粗糙港口岸线/地面几何，因此 `Q03-A09=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 不变。权威结果见 `game/tools/results/LOCAL-QUALITY/Q03-A09/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q03-A09/Q03_A09_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_ROUTE_PROBE_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.json`。

补充验证（2026-09-11 Q03-A09）：D26 legacy tree quarantine 契约 `4/4 PASS`；Unity Pipeline stopped-Editor eval 找到 5 个 `ph_palm_tree` 对象、25 个 legacy Renderer，当前均 disabled。该结果只排除当前停态 legacy 树渲染路径，不解锁正式植被资源，也不替代 Player 视觉门禁。

追加证据（2026-09-11 Q00/Q01/Q02 当前源码重新对齐）：接管审计再次确认分支 `agent/publish-project-changes-20260813`、HEAD `01024cae2823c26b2595a660e6dde5879870dfc8`、工作区 `5330` 条 status（`438` tracked、`4892` untracked），没有 reset/checkout/clean。Unity Pipeline Editor `ready / stopped`、error console=`0`。Q01 targeted runtime eval 确认玩家根单位缩放、CharacterController=`0.242435m/1.8m`、visualRoot=`0.6642`、水位=`-0.02`、舵点=`(0,0.96,-6.67)`；一次过宽表达式超时后拆分采样成功。当前 Q02 D2 EditMode=`8/8 PASS`、D17 PlayMode=`16/16 PASS`，旧 15 项/hash 不再作为当前证据。Q03=`BLOCKED_ASSET`、Q20/D31-P04/S05/G06=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变；A09 新鲜 Player 截图仍显示低模/程序化人物、弱泳姿和舵位/港口接触风险。

追加证据（2026-09-11 Q03-A10 fallback 游泳手臂轴向）：D2 fallback 的 64° visualRoot pitch 与局部 -Y 中性臂组合根因已由运行态采样复现；fallback-only 上臂 `150°`、前臂 `30°` 显式前伸修复已编译进 `build_b4b4570bffeb`。D2 EditMode=`8/8`、D17A08=`1/1`、D17 全量=`16/16`，Player=`0 errors/8 warnings`。RERUN100 archive 为 `49/49` 输入、19 截图、164 observations、69 moving、120 swimming、21 Ship control；截图确认手臂前伸轮廓改善，但低模比例、腿脚、自然泳姿、舵轮接触和港口接地仍未通过。详情见 `game/tools/results/LOCAL-QUALITY/Q03-A10/RESULT.json`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A10/Q03-A10.md`；`Q03-A10=PARTIAL`，Q03/Q31/VC-G01/VC-G02/VC-G03 状态不变。

追加证据（2026-09-11 Q03-A11 ship forward alignment）：D19-R03 Builder 旧的可见船体 `180°` child rotation 与 D2 heading=0 的运动轴相反；修复将稳定船根设为 `180°`、可见 child 设为 identity、舵轮 anchor/轮体做世界位置补偿，D2 登船玩家面向舵轮，并新增 Q26A11 forward-axis contract。Pipeline eval 记录 ship/candidate forward dot=`1.000`；Q26=`11/11`、D17=`16/16`、VC-G01 world logic=`13/13`、Q22/Q23=`1/1`，recompile 无错误，Editor=`ready/stopped`、error console=`0`。Player build=`build_3e69fa3e4248`，`0 errors/8 warnings`；真实 Win32 R3=`48/48` 输入、19 截图、167 observations、69 moving、4 swimming、21 Ship control，船体位移=`6.005729m`、航向变化=`18.372559°`，W/A 后船向水域。截图仅登记 `IMPROVED_WITHIN_SCOPE_NOT_VISUAL_PASS`；程序化低模角色、泳姿、舵轮接触、港口几何/接地仍未通过，远船 reject/owner-not-ready 未覆盖。`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/G03=LOCKED` 保持。权威结果见 `game/tools/results/LOCAL-QUALITY/Q03-A11/RESULT.json` 与 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A11/Q03-A11.md`。

追加证据（2026-09-11 Q00 current source rebinding after Q03-A11）：重新核对当前工作区 `5331` 条 status（`439` tracked、`4892` untracked）、HEAD=`01024cae2823c26b2595a660e6dde5879870dfc8`、Unity Editor=`6000.0.80f1 / PID 31416 / 127.0.0.1:7800 / ready`。当前 build=`build_3e69fa3e4248`，Q03-A11 R3=`48/48` 输入、19 截图、167 observations；旧 R31/A20 Player 标为 stale。一次 broad eval 超时被记录，simple eval 成功，`clear_console` 后 error console=`0`。Q00 仍仅 `VERIFIED` 当前事实关系，不升级 Q03/Q31/VC-G01/D31/VC-G02/VC-G03；权威节点为 `game/tools/results/LOCAL-QUALITY/Q00/RESULT.json` 的 `current_reaudit_20260911_q03a11`。

## 2026-09-12 Q26-A12 / VC-G01 真实 Player 互补路线与 R03 道具接地

- Q26-A12 的实际根因是 R03 表现道具使用分析/固定 Y，而当前 `D19_TerrainRoot` MeshCollider 使用变形网格；`Barrel_2` 与 `NoticePost` 在修复前分别出现 `0.192m`、`0.440m` 接地偏差。Builder 现按自有 MeshCollider raycast 高度并按 Renderer bounds 接地；无 R03 命中的 cliff 候选被移出正式表现集合。
- Unity Pipeline 证据：Q26A12R03=`1/1 PASS`；完整 `D19R03Q26GameplayDefectPlayModeTests`=`12/12 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error console=`0`。
- Python 定向契约=`37/37 PASS`；全量回归=`1152/1152 PASS`。其中 D2 source-contract test 已同步当前显式上下臂/前臂实现；D18 validator 单文件 20 次重复通过。
- 当前源码 Player：`build_6cc1e97f82cc`，`StandaloneWindows64`，`0 errors / 8 warnings`，结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A12_PRESENTATION_GROUNDING_BUILD_RESULT.json`。
- R1 正常路线：`45/45` 输入、17 截图、122 observations、37 moving、51 swimming，远船 E reject、I/M/T 重复、Esc、业务/暂停和 graceful close 成立；未发送船控移动段，ship proof=`false`。
- R2 WaterProbe：`48/48` 输入、19 截图、167 observations、70 moving、2 swimming、21 ship-control，船位移=`5.94493293762207m`、航向=`18.02655029296875°`；船控成立；该分支不覆盖远船 reject。
- 真实截图仍显示程序化低模角色、泳姿/舵轮接触和岸线/地面几何风险，故不升级视觉状态。正式状态保持 `Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET`。
- 权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A12_PRESENTATION_GROUNDING_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`、`game/tools/results/D31/D31-P04/D31_P04_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26-A12.md`。

## 2026-09-12 Q29 Pipeline R3 控制面复核与键位冲突修复

- 使用 Unity Pipeline runtime server 构建并运行 `Q29_RUNTIME_PIPELINE_QA_R3.exe`：`build_317c09412f0e`、`StandaloneWindows64`、`MainMenu + InitialHarbor`、`Succeeded`、`0 errors / 7 warnings`、`220836890` bytes；EXE SHA256=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`。
- 本轮没有接管用户前台桌面：隐藏 GUI Player 保留 Input System，loopback runtime server `7900`；`simulate_pointer` 通过 MainMenu Continue 进入 `InitialHarbor`，`simulate_key` 以 down/wait/up 注入运行时键盘事件。Player PID `57180` 通过 runtime `quit` 优雅退出。
- R2 运行时真实暴露 D29 酒馆与 D3 出航共用 `T` 的跨 owner 冲突：一次 T 同时打开酒馆并使 D3 进入 Sailing，后续 M 被正确拒绝。已将 D3 默认出航主键改为 `Y`，D29 酒馆继续 `T`，并新增 `D29TavernAndD3VoyageHaveDistinctPrimaryKeyboardBindings`。
- R3 状态证据：`T→d10Focus=true/voyage=InPort`；关闭酒馆后 `M→trade=true/voyage=InPort`；关闭市场后 `Y→voyage=Sailing`；W 1000ms 实际位置 `(0.000,0.195,-2.700)→(0.195,0.162,-4.868)`、速度 `3.200`；E `boarded→disembarked`；I `D29WarehousePanel`；Esc 关闭并完成 `TimeScale 0→1` 暂停恢复。
- 回归：D16Accessibility `8/8`、D16InputRouter `10/10`、D29 Q15 `1/1`、W/S HUD navigation `1/1`；recompile `failed=false/errors=[]`；runtime error console `0`；Editor 最终 `ready/stopped`。
- 边界：R3 是 Pipeline-enabled Development QA Player，不是发布构建；没有前台截图、物理键盘或人工视觉 PASS。运行时仍记录未验证蒙皮/程序化 fallback 警告，因此 Q29=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。
- 安全恢复：已恢复 `RuntimePipelineConfig.enableInBuilds=false` 与 Editor `developmentBuild=false`。

权威结果：[Q29 RESULT](Q29/RESULT.json)；交接：[Q29 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。

## 2026-09-12 Q26 D7 helm anchor 与 D27 NPC URP material repair

- D7/D19：`D7ShipDeckController` 消费 authored `D7_VisibleHelmAnchor`；Q26/A05 左右手到舵轮距离由 `0.4102m` 降到 `0.2608m`。`D19R03Q26GameplayDefectPlayModeTests=12/12 PASS`，`D17ActionQualityPlayModeTests=16/16 PASS`。
- D27/D20：扫描确认 NPC Prefab 使用 built-in `Standard` 后，迁移 `Assets/Art/Models/Characters/D20/NPC/Materials` 下 `16` 个材质到 `Universal Render Pipeline/Lit`，保留颜色/金属度/光滑度；`D20A03NpcVisualTests=4/4 PASS`。
- Hidden Player `build_0a90b4c58ed1`：`Q26_D27_NPC_URP_MATERIAL_REPAIR_PIPELINE_20260912.exe`，`StandaloneWindows64`，`0 errors / 0 warnings`；runtime=`59` NPC renderers、`123` material slots、URP Lit=`123`、Standard=`0`、InternalErrorShader=`0`；runtime error console=`0`，Player clean close，GPU close frame 无 magenta NPC。
- 结果：[D7 anchor](Q26/Q26_D7_HELM_ANCHOR_REVALIDATION_PIPELINE_20260912_RESULT.json)、[D27 URP](Q26/Q26_D27_NPC_URP_MATERIAL_REPAIR_PIPELINE_20260912_RESULT.json)、[Q26 parent](Q26/RESULT.json)。Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、Q03=`BLOCKED_ASSET`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。

追加证据（2026-09-12 Q03-A14 R4 generic arm pose）：最新 formal generic-path placeholder 的 Idle/Walk/常见交互状态增加 presentation-only neutral arm correction；每帧最多一次、暂停跳过，Fish/Helm 保持显式排除。D20A02=`10/10 PASS`、Q03 formal=`1/1 PASS`、Q26=`12/12 PASS`、D17=`16/16 PASS`、recompile=`failed=false/errors=[]`。Exact hidden GUI Player build=`build_9918fec2b2fc`、`Q03_A14_FORMAL_PIPELINE_QA_R4_ARM_POSE_DEV2.exe`、`0 errors/0 warnings`，Assembly-CSharp SHA=`5CEEAA4EB7654C77D1545C4EBF5C0109D2BD892B991BBB0896676A8E76E5E5A7`；PID=`70652`、runtime=`7900`，MainMenu→InitialHarbor、Idle/Walk 手部 Y 低于上臂、E→Ship/Helm 边界、runtime error console=`0`、clean close。GPU close frame 已登记；普通状态 T-Pose-like 缺陷有界缓解，但 formal 玩家仍暗/剪影、Fish/Helm/authoritative action/avatar/material、接地/穿模和港口几何仍开放。Q03=`BLOCKED_ASSET`、Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。权威结果：`Q03-A14/Q03_A14_FORMAL_R04_ARM_POSE_PIPELINE_20260912_RESULT.json`、`Q03-A14/RESULT.json`、`../VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q26-A14 R03 shoreline collision-owner repair

- 真实 PlayMode 先复现并保留失败：静态合批后 `D19_R03_IslandSurface.MeshFilter.sharedMesh` 为 `Combined Mesh (root: scene) 4`，`ShoreRock_0` 地形命中误差=`0.290m`，Q26A14 首次结果为失败。
- `HarborCollisionController` 现保留场景作者 `D19_TerrainRoot` 的 MeshCollider.sharedMesh，不再从运行时静态合批后的视觉 MeshFilter 重建碰撞；R03 Builder 继续以同一自有 terrain owner 接地。
- 修复后 terrain mesh=`D19_R03_IslandSurfaceMesh`，9/9 岸线岩石误差=`0.018–0.022m`；Q26A14=`1/1 PASS`，完整 Q26=`14/14 PASS`。
- Player build=`build_0cc514c48042`，`StandaloneWindows64 / 0 errors / 8 warnings / 175181851 bytes`；EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`3E912CCB9F439C66BA1768478801EE11893838019AC6843C6266871B9F7374AD`；Editor=`ready/stopped`、error console=`0`。
- 本卡未启动 Player、未发送前台输入、未采集截图；只关闭一个运行时碰撞/接地缺陷。`Q26=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_PIPELINE_20260912_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_BUILD_RESULT.json`、`game/tools/results/D31/D31-P04/D31_P04_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

## 2026-09-12 Q04 R2 / Q31 Pipeline hidden Player 当前候选复核

- Q04 定向水域复核：`D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles`=`1/1 PASS`，`9.45s`，当前 D17 source hash=`DAFFDD729D0AD4A01DC8874FAB1ACB3BE8DADC132E17B5D7D721971F2E4BDC20`；Editor 最终 `ready/stopped`，error console=`0`。Q04 继续 `VERIFIED`，仅限 D2 water-state owner。
- Q31 hidden GUI Development Player：`build_b5d99f79b224`，`0 errors / 7 existing warnings`；Pipeline pointer 进入 InitialHarbor，W/S/A、runtime shoreline entry/exit、E 登船/下船、I、T、Esc modal 与暂停恢复均有运行时状态证据，runtime error console=`0`，clean close。
- 本次存档初始状态为 `Sailing`，M 只得到合法航行中拒绝；没有改写存档制造市场成功。物理前台键盘、用户视角截图、正式动作/Avatar、自然泳姿、握舵、植被接地、用户 GPU 性能和最终视觉门仍未覆盖。
- 状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`；Q03=`BLOCKED_ASSET`。权威结果：`game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260912_R2_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_RUNTIME_RECHECK_20260912_RESULT.json`。

追加证据（2026-09-12 Q31 Pipeline camera diagnostic）：在 `build_b5d99f79b224` hidden GUI Player 上生成 InitialHarbor、boarded ship、swim route、helm close 四张 RenderTexture PNG；`Q26A05ProceduralFallbackReachesVisibleHelmInShipMode=1/1 PASS`，舵轮与左右 fallback hands 的运行时距离均为 `0.2608m`，Player clean close，Editor=`ready/stopped`，console errors=`0`。相机近景仍明确显示低模/占位角色和粗糙场景，游泳默认帧也不足以判断自然泳姿；因此不升级最终视觉门。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。权威结果：`Q31/Q31_PIPELINE_CAMERA_DIAGNOSTIC_20260912_RESULT.json`、`Q31/RESULT.json`、`../VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json`。

追加证据（2026-09-12 Q20 Pipeline Player tree containment）：当前候选 hidden Player 进入 InitialHarbor 后，runtime eval 观察到 `ph_palm_tree` root active，但其 `25` 个 Renderer 全部 `enabled=0`；D26 同轮日志 `vegetation_grounded=26`、`vegetation_hidden_without_terrain=0`，runtime error console=`0`、clean close。该结果只证明当前旧 palm Renderer 没有作为可见表现输出，不证明正式植被资源准入或全镜头接地/穿模完成。Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。权威结果：`Q20/Q20_PIPELINE_PLAYER_TREE_CONTAINMENT_20260912_RESULT.json`、`Q20/RESULT.json`。

## 2026-09-12 Q03-A14 D20 readable skin owner repair

- D20 Builder/bridge 现在为 Candidate/Formal 的 `SuperHero_Male` 绑定项目拥有的 URP Lit `D20_PlayerSkin_Warm`；D5/D7 识别 D20 bridge-owned renderer 后跳过通用材质写入，避免多 writer 覆盖导致黑色剪影。
- 当前回归：D20A02 EditMode=`10/10 PASS`、Q03D20 PlayMode=`3/3 PASS`、D19R03Q26=`14/14 PASS`、D17=`16/16 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`。
- Hidden R4 Player `Q03_A14_D20_SKIN_REPAIR_R4.exe`：Candidate/Formal 均实测 `D20_PlayerSkin_Warm / URP Lit / RGBA(0.720,0.420,0.250,1.000)`，W 到达 `Walk`，Formal E 到达 `ph_ship_fishing`，runtime errors=`0`，clean close；四张 RenderTexture PNG 已登记 SHA256。
- 视觉边界：Candidate 可读性改善；Formal 仍缺完整 outfit，Helm 仍有舵轮上方的 T-pose-like/高度错位；自然泳姿、树木/地面接地、穿模、用户 GPU 性能和最终人工视觉仍开放。

权威结果：[Q03-A14 skin owner](Q03-A14/Q03_A14_SKIN_OWNER_PIPELINE_R4_20260912_RESULT.json)、[Q03-A14 parent](Q03-A14/RESULT.json)、[VC-G01 handoff](../../docs/game/design/common/handoffs/VISUAL-CLEAN/VC-G01_HANDOFF.md)。状态保持：Q03=`BLOCKED_ASSET`、Q03-A14/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。
## 2026-09-12 Q03-A14 D20 Helm pose Pipeline R1

- 新增 presentation-only generic-path Helm upper-arm safety pose；不写 D2 physical root、Systems 或 AgentCore；D20A02 fixture 在 Helm 断言前恢复 imported arms，避免测试自污染。
- 当前回归：D20A02=`10/10`、Q03D20=`3/3`、D19R03Q26=`14/14`、D17=`16/16`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`、error console=`0`。
- Exact hidden Player build=`build_67ffef5d580f`、`Q03_A14_D20_HELM_POSE_PIPELINE_R1.exe`、`0 errors / 8 warnings`。Pipeline pointer 进入 InitialHarbor，E 后 `state=Helm`、`parent=ph_ship_fishing`，runtime error console=`0`，clean close；三张 RenderTexture 图见 child result。
- runtime probe：hands=`(±0.20,2.09,-6.63)`，wheel center=`(0,1.72,-6.35)`；GPU front frame 证实横向 T-pose-like 有界缓解，但双手仍在舵轮上方，没有自然握舵接触。`Q03-A14=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q26/Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 不变。
- 权威结果：[child result](Q03-A14/Q03_A14_D20_HELM_POSE_PIPELINE_R1_20260912_RESULT.json)、[Q03-A14 parent](Q03-A14/RESULT.json)、[VC-G01 result](../VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。下一步只选择 formal authored action/avatar/IK 或 Helm geometry/scale/IK 一个 owner。
## 2026-09-12 Q03-A14 D20 Fish pose Pipeline R2

- D20 generic-path Fish 增加独立 presentation-only upper-arm silhouette correction；每帧幂等、暂停安全，不写 D2 physical root、Systems 或 AgentCore；Helm 与普通 locomotion correction 保持分离。
- 当前回归：D20A02=`10/10`、Q03D20=`3/3`、D19R03Q26=`14/14`、D17=`16/16`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`、error console=`0`。
- Exact Player build=`build_35b48f947b48`、BuildGuid=`7e812f2f453a4430b9c2d8e37d514431`、`Q03_A14_D20_FISH_POSE_PIPELINE_R2.exe`、`0 errors / 8 warnings`。一次真实 hidden route attempt 被 D7 `FocusLost/CharacterInputAllowed=false` 排除；第二次 exact Player 组件探针得到 `state=Fish`、`fishCorrectionApplied=True`、root pitch=`90°`，GPU 帧已归档，runtime error console=`0`，clean close。
- 组件帧只说明 generic arm spread 有界收窄，不证明自然划水、持续输入、水体浮力/接地或最终视觉；`Q03-A14=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q26/Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 不变。
- 权威结果：[Fish R2 child result](Q03-A14/Q03_A14_D20_FISH_POSE_PIPELINE_R2_20260912_RESULT.json)、[Q03-A14 parent](Q03-A14/RESULT.json)、[VC-G01 result](../VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)。下一步选择正式 swim/action/avatar owner 或专门 swim cycle asset，不扩大 placeholder correction。

## 2026-09-12 Q26-A15 碰撞 owner 契约与 EditMode 回归同步

- Q26-A14 的运行时 owner 已固定为场景 authored `D19_TerrainRoot` `MeshCollider`；本轮修正两个旧 EditMode 夹具，使其提供独立 terrain mesh 并断言该 mesh 不被视觉 surface mesh 替换；同时放宽 `RenderAuthoredPalms = false` 的声明形式断言。
- 初次全量回归的 `3` 个失败被保留并归因，修正后定向 `VcA02CollisionAlignmentTests=1/1`、`VcG01WorldLogicTests=14/14`，全量 Unity EditMode=`629/629 PASS`，failed/skipped/inconclusive=`0`。
- Pipeline recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error console=`0`。`HarborCollisionController` 运行时代码未在本卡修改。
- 状态不升级：Q26=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。结果：[Q26-A15](Q26/Q26_A15_COLLISION_CONTRACT_TEST_SYNC_20260912_RESULT.json)，父结果：[Q26 RESULT](Q26/RESULT.json)。

## 2026-09-12 Q00/Q01/Q02 Luna 当前接管与回归

- Q00 当前工作区审计：`5361` status lines（`459` tracked/staged、`4902` untracked），`git diff --shortstat=459 files changed, 39556 insertions(+), 48895 deletions(-)`；分支/HEAD 保持 `agent/publish-project-changes-20260813` / `01024cae...`，未执行清理。Unity Pipeline 最终 `ready/stopped`，RuntimePipelineConfig=`enableInBuilds=false/port=0`，clear 后 error console=`0`。
- Q01 当前 connected Editor 根因表：`D2_PlayerAvatar=(0.000,0.150,-2.700)`、unit root；`HumanoidVisual scale=0.6642`、renderer=`17`；capsule=`0.242435/1.800/center.y=0.847`；默认 `Procedural/FormalAnimatorReady=false/BoundBoneCount=8`；ship scale=`2.574`；HelmPoint/visible anchor=`(0,1.100,-6.680)`。直接对象查询发现 `D19_TerrainRoot` parent=`D19_EnvironmentRoot` 且 MeshCollider descendants=`0`，与 Q26/A15 authored collider 契约存在待复核差异，不升级碰撞/接地状态。
- Q02 聚焦 `D2CharacterAnimationControllerTests=8/8 PASS`。相关 D17 全量第一次 `15/16`（D17A16 camera-player sqrDistance=`0.0221495014`），同一测试单独=`1/1`，第二次全量=`16/16 PASS`；该间歇性信号已保留，不能隐藏，也不能直接认定 CameraFollow 根因已修复。
- 当前任务板不升级：Q02=`PASS_WITHIN_SCOPE`，Q03=`BLOCKED_ASSET`，Q20/Q26/Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`，最终 physical keyboard/manual visual=`WAITING_VISUAL`。下一张只选 Q12/Q26-A20 camera isolation 或 Q07/Q26 TerrainRoot MeshCollider owner revalidation。

权威结果：[Q00 current](Q00/Q00_CURRENT_TAKEOVER_20260912_LUNA_R1_RESULT.json)、[Q01 current](Q01/Q01_CURRENT_ROOT_CAUSE_20260912_LUNA_R1_RESULT.json)、[Q02 current](Q02/Q02_CURRENT_REVALIDATION_20260912_LUNA_R1_RESULT.json)；父结果与各卡 handoff 已同步。

## 2026-09-12 Q26-A16 R03 terrain owner 运行时复核

- Q01 的同名节点误读已被当前 Editor 运行时复核解释：D19_EnvironmentRoot/D19_TerrainRoot 是禁用的 legacy TerrainCollider；实际 Harbor owner 是 D19_R03_HarborPresentation/D19_TerrainRoot 的启用、非 Trigger MeshCollider，sharedMesh=D19_R03_IslandSurfaceMesh。
- HarborCollisionController（挂在 D2RuntimeBootstrap）实际状态：IsInitialized=True、R03SurfaceInstalled=True、ColliderCount=5、WalkableSurfaceCount=3。同轮视觉 MeshFilter 已为 Combined Mesh (root: scene) 4，碰撞 owner 仍保留 authored mesh。
- 新增 Q26A16HarborCollisionUsesScopedR03TerrainOwner，将 terrain 查询限定在 R03 presentation hierarchy，避免全局 GameObject.Find("D19_TerrainRoot") 误导。定向=1/1 PASS；最终源码完整 Q26 PlayMode=15/15 PASS；recompile=failed=false/errors=[]；Editor=ready/stopped；error console=0。
- 本卡是自动化 owner 防回归，不是 native Player/人工视觉通过。Q26=PARTIAL、Q07 保持既有范围内 VERIFIED、VC-G01=IN_PROGRESS、VC-G02/VC-G03=LOCKED、D31-P04/S05/G06=PARTIAL 不变。

权威结果：[Q26-A16](Q26/Q26_A16_TERRAIN_OWNER_RUNTIME_REVALIDATION_20260912_RESULT.json)、[Q26 RESULT](Q26/RESULT.json)、[Q26 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一步转向 formal action/Avatar 或 swim/helm presentation 的单一 owner 缺陷。

## 2026-09-12 Q03-A15 正式动作资源准入门禁

- 新增 `Q03FormalActionAdmissionGateTests`，通过当前 connected Unity Pipeline 对正式 D20 控制器做资源准入回归：`1/1 PASS`；D20A02 全程序集回归 `13/13 PASS`。
- 实际日志为 `clips=15 / humanMotion=0 / genericRoot=15`，逐项确认 15 个动作仍是 generic transform-path 资源；门禁保持关闭，避免把 explicit `-d20Formal` 诊断路线误当成 authored Humanoid action。
- Pipeline `recompile`=`status=up_to_date`；Editor=`ready / compiling=false / domainReloadInProgress=false / playMode=stopped`；清空后的 error console=`0`。
- 本卡只新增阻塞门禁回归，不修改资源、默认 consumer、D2/D7 或场景。Q03=`BLOCKED_ASSET`、Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。

结果：[Q03-A15](Q03-A15/RESULT.json)；交接：[Q03-A15 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A15/Q03-A15.md)。下一步选择有真实失败证据的几何/接地或 fallback presentation owner，不把 generic formal 诊断路线升级为动作视觉通过。

## 2026-09-12 Q20-A03 运行时植被接地与旧棕榈隔离守卫

- 新增 `D19R03Q20VegetationPlayModeTests`，当前 InitialHarbor PlayMode `1/1 PASS`：可见 R03 植被 `24/24` 与 scoped `D19_TerrainRoot` 有接触，grounding failures=`0`；legacy `ph_palm_tree` roots=`5`，active legacy palm Renderers=`0`。
- 父 EditMode `D19R03Q20VegetationTests=1/1 PASS`；Pipeline 最终 Editor=`ready/stopped`，error console=`0`。
- 这是运行时 containment/接地回归，不是正式棕榈资源恢复或全镜头人工视觉 PASS。D18 ready catalog 仍 `BLOCKED / local_development_ready_count=0 / eligible_entry_count=0`；Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变。

结果：[Q20-A03](Q20-A03/RESULT.json)；交接：[Q20-A03 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20-A03/Q20-A03.md)。

## 2026-09-12 Q26-A17 D19-R03 生成舵轮高度对齐

- Q26-A17 选择 D19 `D19R03HarborPresentationBuilder` 作为唯一 owner。修复前真实测量 `HelmWheel.y=1.72m`、fallback 双手中线 `y=1.894419045m`，垂直误差 `0.174419045m`；修复后生成舵轮 y=`1.89m`。
- 定向 `Q26A17GeneratedHelmWheelSharesFallbackHandHeight=1/1 PASS`，日志 `handMidpointY=1.8944`、`verticalError=0.0044`；完整 `D19R03Q26GameplayDefectPlayModeTests=16/16 PASS`。
- Unity Pipeline recompile=`failed=false/errors=[]`；Windows Player build=`build_cfa950c379f9`，`StandaloneWindows64 / 0 errors / 8 warnings / 175185163 bytes`；EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`0C03B7C8F8FF7CACE7E2B492FBA16767120FB86F78BC0FD9081F7749856C1C28`。
- Editor final=`ready/stopped`、error console=`0`。一次 Editor-only RenderTexture 取景生成均匀灰帧，已登记为无效证据，不升级视觉 PASS；未执行前台物理 Win32 键盘。
- Q26=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`、Q03=`BLOCKED_ASSET` 保持。权威结果：[Q26-A17](Q26-A17/RESULT.json)、[Q26 parent](Q26/RESULT.json)；交接：[Q26-A17 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A17/Q26-A17.md)。

## 2026-09-13 Q03-A16 Formal Humanoid pose adapter与R2 Player校准

- 新增 `-d20FormalHumanoid` opt-in D20 Presentation 路径，使用当前 R02 valid Humanoid Avatar 的 `HumanPoseHandler`；默认 Player、旧 `-d20Formal` generic 路径、D2 physical root、Systems 和 AgentCore 未切换。
- Unity Pipeline 回归：新 Humanoid PlayMode=`1/1 PASS`，Q03D20=`4/4 PASS`，Q03Formal=`3/3 PASS`，recompile 无错误。
- Windows Player `build_d87c4e4b5265`：`StandaloneWindows64 / 0 errors / 2 warnings`。隐藏 Player 进入 `InitialHarbor` 后，W/S 路线到达 `Swimming=True/Fish/pitch=70`；E 登船后左右手到真实 `HelmWheel` 距离=`0.219m/0.222m`。
- R2 hidden Player RenderTexture 帧和 Player log 已登记；Player 已退出，descriptor/PID 清除，Pipeline 恢复 `enableInBuilds=false/port=0`，Editor=`ready/stopped`。
- 结果：[Q03-A16](Q03-A16/Q03_A16_FORMAL_HUMANOID_PIPELINE_R2_RESULT.json)；交接：[Q03-A16 handoff](../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A16/Q03-A16.md)。
- 边界不升级：Q03 仍 `BLOCKED_ASSET`、Q31/VC-G01 仍 `PARTIAL/IN_PROGRESS`、VC-G02/VC-G03 仍 `LOCKED`、D31-P04/S05/G06 仍 `PARTIAL`；当前日志中的 1 条 Error 是误调用不存在的 `get_console_logs` 工具命令，已单独保留，不伪造成零错误或整体视觉通过。

## 2026-09-13 Q00/Q01 接管审计与 Q02 当前 Pipeline 回归

- Q00：[current takeover result](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A16_20260913_RESULT.json)。工作区 `10263` status entries（tracked=`459`、untracked=`9804`），未清理；Q03-A16 R2 源码/Player/Data hash 重新对应。旧 Editor PID `31416` 的 D3D11 GPU timeout 已记录；同项目 `-nographics` Editor PID `53332` 恢复 Pipeline，最终 `ready/stopped`、console=`0`、RuntimePipeline=`enableInBuilds=false/port=0`。
- Q01：[current root-cause result](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_20260913_RESULT.json)。通过 connected Pipeline 在 InitialHarbor 取得 root/visualRoot/capsule/ship/helm/R03 terrain/water 运行态和 writer/tolerance 表；默认仍 `Procedural/FormalAnimatorReady=false`。动作、泳姿、握舵、植被和 W/S-HUD 的视觉/前台边界均明确保持未通过。
- Q02：[current pipeline result](Q02/Q02_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)。D2 动画 EditMode=`8/8 PASS`；D17 Action Quality async PlayMode=`16/16 PASS`（`56.79s`）；0 failed/skipped/inconclusive。该证据只闭合 owner/lifecycle/逻辑回归，不升级 formal action、物理键盘或视觉质量。
- 当前任务板：Q00/Q01/Q02 为有界完成；Q03=`BLOCKED_ASSET`、Q20/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。下一步执行 Q04 当前水域 owner 回归，再进入 Q05/Q06/Q08 可执行检查。

## 2026-09-13 Q04 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles` 完成态为 `1/1 PASS`，测试 `11.1657914s`、总计 `11.26s`；覆盖浅水 10 次与深水 10 次进入—移动—退出。
- 当前 `D17ActionQualityPlayModeTests.cs`、`D2RuntimeBootstrap.cs`、`OceanAdapter.cs` 源码 hash 已写入 Q04 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q04 仍为 `VERIFIED`（只限 D2 water-state owner）；不升级原生 Player W/S、正式泳姿、水面材质、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q04 当前复核](Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q04 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md)。下一张可执行卡：Q05 当前游泳姿态复核。

## 2026-09-13 Q05 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot` 完成态为 `1/1 PASS`，测试 `12.9665234s`、总计 `13.06s`；覆盖 10 次游泳姿态进入/退出循环。
- 当前 D17 测试、D2 动画控制器与 D2 角色控制器源码 hash 已写入 Q05 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q05 仍为 `VERIFIED`（只限 whole-visual-root 倾斜/恢复和物理根分离）；不升级正式泳姿、原生 Player W/S、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q05 当前复核](Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q05 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)。下一张可执行卡：Q06 当前碰撞/落地复核。

## 2026-09-13 Q06 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite` 完成态为 `1/1 PASS`，测试 `13.8529193s`、总计 `13.95s`；覆盖 10 次跳跃—落地及四方向基础小步移动。
- 当前 D17 测试、D2 角色控制器与 HarborCollisionController 源码 hash 已写入 Q06 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q06 仍为 `VERIFIED`（只限跳跃/接地/小步移动稳定性）；不升级墙/坡/岸线/跳板/船甲板完整碰撞、原生 Player W/S、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q06 当前复核](Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q06 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)。下一张可执行卡：Q07 当前碰撞路线复核。

## 2026-09-13 Q07 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps` 完成态为 `1/1 PASS`，测试 `4.6148297s`、总计 `4.71s`；覆盖活动地形、码头、跳板碰撞体及双向小步路线。
- 当前 D17 测试、HarborCollisionController 与 D19 R03 builder 源码 hash 已写入 Q07 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q07 仍为 `VERIFIED`（只限当前自动化地形/码头/跳板路线）；Q08 仍受 Q03=`BLOCKED_ASSET` 约束，不升级原生 Player、完整碰撞路线、截图视觉、Q31、VC-G01 或 D31。

权威结果：[Q07 当前复核](Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q07 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。下一张可执行卡：先登记 Q08 阻塞，再执行 Q09 当前甲板/舵位复核。

## 2026-09-13 Q08 当前依赖复核

- Q08 保持 `BLOCKED`：Q03 当前 15 个 formal clip 仍为 `humanMotion=0 / genericRoot=15`，默认 InitialHarbor formal consumer=`0`，所以没有运行会误导为完整动作整合的 Q08 测试。
- Q05 当前 `D17A08=1/1 PASS`、Q07 当前 `D17A10=1/1 PASS` 已作为依赖快照登记，但只覆盖各自有界契约，不能解除 Q03 formal action 资源门禁。
- 状态不是失败，也不是 PASS；解除需要当前 rig 合法 authored ground-action 资源或可审计 Avatar/骨骼轴适配器，以及同源 Q08 输入→状态→位移→姿态整合回归。

权威结果：[Q08 当前阻塞](Q08/RESULT.json)；交接：[Q08 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。下一张可执行卡：Q09 当前甲板/舵位复核。

## 2026-09-13 Q09 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding` 完成态为 `1/1 PASS`，测试 `4.7154567s`、总计 `4.81s`；覆盖非单位船根、10 次登船/下船和 30m 远距拒绝。
- 当前 D17 测试、D7ShipDeckController、D2ControlRouter 与 D2 角色控制器源 hash 已写入 Q09 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q09 仍为 `VERIFIED`（只限锚点/生命周期/拒绝）；不升级自然握舵、航行中甲板关系、原生 Player、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q09 当前复核](Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q09 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)。下一张可执行卡：Q10 当前航行中甲板关系复核。

## 2026-09-13 Q10 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` 完成态为 `1/1 PASS`，测试 `3.2831358s`、总计 `3.38s`；覆盖 10 个船舶平移/转向快照与玩家相对甲板稳定性。
- 当前 D17 测试、D2ShipController、D7ShipDeckController 与 D2ControlRouter 源码 hash 已写入 Q10 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q10 仍为 `VERIFIED`（只限 snapshot-driven 甲板随动）；不升级开放海、原生 Player WASD、镜头、自然握舵、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q10 当前复核](Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q10 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。下一张可执行卡：Q11 当前安全下船复核。

## 2026-09-13 Q11 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround` 完成态为 `1/1 PASS`，测试 `5.2898463s`、总计 `5.39s`；覆盖 10 次合法下船与无落点拒绝。
- 当前 D17 测试、D7ShipDeckController、D2 角色控制器和 HarborCollisionController 源码 hash 已写入 Q11 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q11 仍为 `VERIFIED`（只限合法/无落点下船状态）；不升级移动船碰撞、全岸线出口、原生 Player、正式动作、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q11 当前复核](Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q11 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。下一张可执行卡：Q12 当前相机复核。

## 2026-09-13 Q12 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose` 完成态为 `1/1 PASS`，测试 `5.0176968s`、总计 `5.11s`；覆盖 10 次相机模式/目标切换、实际 SphereCast 遮挡收缩与阻尼恢复。
- 当前 D17 测试、CameraFollow、D2ControlRouter 与 D2 角色控制器源 hash 已写入 Q12 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q12 仍为 `VERIFIED`（只限 CameraFollow 逻辑）；不升级最终场景构图、渲染穿模、原生 Player、截图视觉、Q03、Q31、VC-G01 或 D31。

权威结果：[Q12 当前复核](Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q12 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。下一张可执行卡：Q13 当前输入焦点复核。

## 2026-09-13 Q13 当前 Pipeline 定向复核

- `D17A15GameplayKeyboardDoesNotNavigateHudAndPauseRestoresModalOwnership`=`1/1 PASS`（测试 `4.4704041s`、总计 `4.57s`）。
- `D16InputRouterPlayModeTests`=`10/10 PASS`（总计 `6.0s`），failed/skipped/inconclusive=`0`；覆盖合成键盘/手柄、重绑定、InitialHarbor 和暂停路由。
- Q13 仍为 `VERIFIED`，仅限 Unity Input System PlayMode 的输入 owner/UI 导航隔离；不等于物理 Win32 键盘、渲染 HUD、E/I/M/T 全流程或 VC-G01 通过。历史 `9/10` 相邻 NaN 证据继续保留。

权威结果：[Q13 当前复核](Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q13 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。下一张可执行卡：Q14 当前船岸整合复核。

## 2026-09-13 Q13 当前 Pipeline 定向复核 R2

- 当前源码通过已连接 Unity Pipeline Editor 重跑 `D17A15GameplayKeyboardDoesNotNavigateHudAndPauseRestoresModalOwnership`：`1/1 PASS`（测试 `4.3906842s`、总计 `4.51s`）。
- 当前源码重跑 `D16InputRouterPlayModeTests`：`10/10 PASS`（总计 `5.62s`），failed/skipped/inconclusive 均为 `0`。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`。Q13 继续 `VERIFIED`，仅限 Unity Input System PlayMode 输入 owner/UI 导航隔离；不升级物理 Win32 键盘、渲染 HUD、E/I/M/T、native Player 或 VC-G01。

权威结果：[Q13 当前复核 R2](Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q13 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。下一张可执行卡：Q14 当前船岸整合复核 R2。

## 2026-09-13 Q14 当前 Pipeline 定向复核

- `D17ActionQualityPlayModeTests.D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence` 完成态为 `1/1 PASS`，测试 `4.4093881s`、总计 `4.51s`；覆盖岸上→入水→上岸→登船→转向→无落点拒绝→合法下船。
- 当前 D17、D2、D7、CameraFollow 与 D16 输入 owner 源 hash 已写入 Q14 结果；Editor 最终 `ready/stopped`，Console 已清空。
- Q14 仍为 `VERIFIED`（只限有界状态链）；`imported_neutral` 仍使正式动作质量为 Q03 阻塞，不升级物理 Win32、E/I/M/T、截图视觉、Q31、VC-G01 或 D31。

权威结果：[Q14 当前复核](Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q14 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。下一张可执行卡：Q15 当前面板焦点/恢复复核。

## 2026-09-13 Q14 当前 Pipeline 定向复核 R2

- 当前源码重跑 `D17ActionQualityPlayModeTests.D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence`：`1/1 PASS`（测试 `3.3597823s`、总计 `3.48s`），覆盖岸上→入水→上岸→登船→转向→无落点拒绝→合法下船。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`；当前源 hash 已写入 Q14 R2 结果。
- Q14 继续 `VERIFIED`（只限有界船岸状态链）；`imported_neutral` 使正式动作质量仍受 Q03 阻塞，不升级物理 Win32、E/I/M/T、截图视觉、Q31、VC-G01 或 D31。

权威结果：[Q14 当前复核 R2](Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q14 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。下一张可执行卡：Q15 当前面板焦点/恢复复核 R2。

## 2026-09-13 Q15 当前源码 Pipeline 复核

- `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes` 完成态为 `1/1 PASS`，测试 `17.3518531s`、总计 `17.45s`；I/M/T 三类面板各执行 10 次打开、重复、关闭和 Gameplay 恢复，互斥与重复请求拒绝成立。
- 本轮没有修改产品代码；当前 D29/D16 源 hash、Pipeline PID `53332`、Editor 最终 `ready/stopped` 与 Console 清空动作已写入 Q15 当前结果。
- Q15 仍为 `VERIFIED`（仅限 D29/D16 PlayMode 焦点/快捷键/恢复）；不等于物理 Win32 键盘/鼠标、原生 Player、渲染视觉、性能或 VC-G01 通过。

权威结果：[Q15 当前复核](Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q15 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。下一张可执行卡：Q16 当前交易桥复核。

## 2026-09-13 Q15 当前源码 Pipeline 复核 Luna R2

- 一次错误类名过滤返回 `total=0`，未计入证据；按当前测试源码正确限定名重跑 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`，结果 `1/1 PASS`，测试 `14.8763723s`、总计 `15.0s`。
- Console marker 为 `shortcut_input=true`、`keys=I,M,T`、`warehouse_cycles=10`、`market_cycles=10`、`tavern_cycles=10`、`exclusive=true`、`repeated_request_rejected=true`、`gameplay_recovery=true`；Editor 最终 `ready/stopped`，error console 查询=`0`。
- Q15 继续 `VERIFIED`，仅限 D29/D16 PlayMode 快捷键 owner、面板互斥与 Gameplay 恢复；不升级物理 Win32 键鼠、native Player、渲染面板、业务 owner、性能或 VC-G01。

权威结果：[Q15 当前复核 R2](Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；marker：[Q15 Console markers](Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_CONSOLE_MARKERS.log)；交接：[Q15 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。下一张可执行卡：Q16 当前交易桥复核 R2。

## 2026-09-13 Q15 当前源码 Pipeline 复核 Q03-A17 R1

- 当前源码重跑 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`：`1/1 PASS`，测试 `12.4690213s`、Pipeline 总计 `12.58s`。
- Console marker seq `21529` 确认仓库、市场、酒馆各 `10` 轮，`exclusive=true`、`repeated_request_rejected=true`、`gameplay_recovery=true`；Editor 最终 `ready/stopped`，error console=`0`。
- 当前运行仍有 D27 表现对象等待初始化和 D2 程序化 Player fallback warning；已记录为 visual/runtime debt，不升级为视觉通过。
- Q15 继续 `PASS_WITHIN_SCOPE`，只限 D29/D16 PlayMode owner/焦点逻辑；前台物理键鼠、native Player、渲染质量、性能、Q03 与 VC-G01 仍开放。

权威结果：[Q15 Q03-A17 R1](Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q15 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。下一张可执行卡：Q16 当前交易桥复核。

## 2026-09-13 Q16 当前源码 Pipeline 复核

- `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest` 完成态为 `1/1 PASS`，测试 `4.7550835s`、总计 `4.85s`；覆盖 owner 未就绪、快速双击单 owner、买卖一次变化、余额不足与满仓拒绝。
- 实际 Console marker 为 `not_ready=true`、`shortcut_double_click_one_owner=true`、`buy_success_one_change=true`、`sell_success_one_change=true`、`insufficient_funds_unchanged=true`、`cargo_full_unchanged=true`；检查前 `80` 条日志、`0` 条 Error，随后已清空 Console。
- Q16 仍为 `VERIFIED`（仅限 D29/D3 交易 owner 桥接与拒绝反馈）；不等于原生 Player 鼠标/视觉/性能、完整首航或 VC-G01 通过。

权威结果：[Q16 当前复核](Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q16 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。下一张可执行卡：Q17 当前船坞/整备桥复核。

## 2026-09-13 Q16 当前源码 Pipeline 复核 Luna R2

- 当前源码重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`：`1/1 PASS`（测试 `2.8561068s`、总计 `2.97s`）。
- Console marker 为 `not_ready=true`、`shortcut_double_click_one_owner=true`、`buy_success_one_change=true`、`sell_success_one_change=true`、`insufficient_funds_unchanged=true`、`cargo_full_unchanged=true`；Editor 最终 `ready/stopped`，error console 查询=`0`。
- Q16 继续 `VERIFIED`，仅限 D29/D3 交易 owner 桥接和拒绝反馈；不升级 native Player 鼠标/视觉、性能、完整航程或 VC-G01。

权威结果：[Q16 当前复核 R2](Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；marker：[Q16 Console marker](Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_CONSOLE_MARKER.log)；交接：[Q16 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。下一张可执行卡：Q17 当前船坞/整备桥复核 R2。

## 2026-09-13 Q16 当前源码 Pipeline 复核 Q03-A17 R1

- 当前源码重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`：`1/1 PASS`，测试 `1.9034709s`、Pipeline 总计 `2.02s`。
- Console marker seq `21609` 确认 owner 未就绪、快速双击单 owner、买卖各一次变化、余额不足不变和满仓不变；Editor 最终 `ready/stopped`，error console=`0`。
- Q16 继续 `PASS_WITHIN_SCOPE`，只限 D29/D3 交易 owner 桥接和拒绝反馈；不升级 native Player 鼠标/视觉、性能、完整首航、Q03 或 VC-G01。

权威结果：[Q16 Q03-A17 R1](Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q16 Q03-A17 R1 marker](Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；交接：[Q16 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。下一张可执行卡：Q17 当前船坞/整备桥复核。

## 2026-09-13 Q17 当前源码 Pipeline 复核

- `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException` 完成态为 `1/1 PASS`，测试 `4.1822541s`、总计 `4.28s`；覆盖修船成功、材料不足、重复/域幂等、取消、船坞成功/拒绝、异常恢复和主输入恢复。
- 实际 marker 为 `preparation_success=true`、`repair_reject_material=true`、`repair_duplicate_unchanged=true`、`domain_duplicate_idempotent=true`、`cancel_recovery=true`、`shipyard_success=true`、`shipyard_reject=true`、`exception_recovery=true`、`primary_input_restored=true`；检查前 `88` 条日志、`0` 条 Error，随后已清空 Console。
- Q17 仍为 `VERIFIED`（仅限 D29/D9/D4 owner 桥接与反馈恢复）；不等于物理键鼠、原生 Player、最终视觉、性能、完整首航或 VC-G01 通过。

权威结果：[Q17 当前复核](Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q17 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。下一张可执行卡：Q18 当前 HUD/本地化尺寸矩阵复核。

## 2026-09-13 Q17 当前源码 Pipeline 复核 Luna R2

- 当前源码重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`：`1/1 PASS`（测试 `3.7202408s`、总计 `3.84s`）。
- Console marker 确认整备成功、材料拒绝、重复/域幂等、取消恢复、船坞成功/拒绝、异常恢复和主输入恢复均为 true；Editor 最终 `ready/stopped`，error console 查询=`0`，预期异常分支为 Warning。
- Q17 继续 `VERIFIED`，仅限 D29/D9/D4 owner 桥接与反馈恢复；不升级物理键鼠、native Player、截图视觉、性能、完整首航或 VC-G01。

权威结果：[Q17 当前复核 R2](Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；marker：[Q17 Console marker](Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_CONSOLE_MARKER.log)；交接：[Q17 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。下一张可执行卡：Q18 当前 HUD/本地化尺寸矩阵复核 R2。

## 2026-09-13 Q17 当前源码 Pipeline 复核 Q03-A17 R1

- 当前源码重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`：`1/1 PASS`，测试 `2.6833445s`、Pipeline 总计 `2.79s`。
- Console marker seq `21700` 确认整备/修船/船坞成功、材料/资金拒绝、重复幂等、取消、异常恢复和主输入恢复；Editor 最终 `ready/stopped`，error console=`0`。
- 空 owner 异常路径按契约记录 Warning，测试确认状态不变并恢复；不将该 warning 伪装成 Error 或隐藏。
- Q17 继续 `PASS_WITHIN_SCOPE`，只限 D29/D9/D4 业务 owner 桥接和恢复；不升级 native Player 键鼠、截图、最终视觉、性能、Q03 或 VC-G01。

权威结果：[Q17 Q03-A17 R1](Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q17 Q03-A17 R1 marker](Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；交接：[Q17 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。下一张可执行卡：Q18 当前 HUD/本地化尺寸复核。

## 2026-09-13 Q18 当前源码 Pipeline 复核

- `D31Q18HudResolutionTests`=`3/3 PASS`（1280×720、1920×1080、2560×1440；`0.02s`），`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`（`0.03s`）；项目根 `python -m unittest tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- 本轮没有修改产品代码；当前 D31/D29 测试和布局策略 hash、Pipeline PID `53332`、Editor 最终 `ready/stopped` 与 Console 清空动作已写入 Q18 当前结果。Console 检查前 `12` 条日志、`0` Error、`0` Warning。
- Q18 保持 `PARTIAL`：仅证明代码级 16:9 HUD 几何、D31 visual-stage 逻辑与 D29 静态回归；不等于原生 Player 截图、字体/英文最长文案、物理键鼠、性能或最终视觉通过。

权威结果：[Q18 当前复核](Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q18 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。下一张可执行卡：Q19 当前船员/反馈审计。

## 2026-09-13 Q18 当前源码 Pipeline 复核 Luna R2

- 当前源码 `D31Q18HudResolutionTests`=`3/3 PASS`（1280×720、1920×1080、2560×1440），`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`；项目根 Python `tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`。Q18 继续 `PARTIAL`，仅限代码级 16:9 HUD 几何、D31 visual-stage 逻辑与 D29 静态回归；不升级 Player 截图、字体/英文最长文案、物理键鼠、性能或最终视觉。

权威结果：[Q18 当前复核 R2](Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q18 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。下一张可执行卡：Q19 当前船员/反馈审计 R2。

## 2026-09-13 Q18 当前源码 Pipeline 复核 Q03-A17 R1

- 当前源码 `D31Q18HudResolutionTests`=`3/3 PASS`（1280×720、1920×1080、2560×1440），`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`；项目根 Python `tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`。
- Q18 继续 `PARTIAL`，仅限代码级 16:9 HUD 几何、D31 visual-stage 逻辑与 D29 静态回归；不升级 Player 截图、字体/英文最长文案、物理键鼠、性能或最终视觉。

权威结果：[Q18 Q03-A17 R1](Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q18 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。下一张可执行卡：Q19 当前船员/反馈审计。

## 2026-09-13 Q19 当前源码 Pipeline 复核

- `Q19CrewInteractionAuditTests` 目标复跑为 `1/1 PASS`（测试 `0.3789095s`、总计 `0.39s`）；`Save.Tests` 全程序集为 `377/377 PASS`（总计 `82.16s`）；`dotnet build game/AgentCore/AgentCore.csproj --nologo` 为 `0 warnings / 0 errors`。
- 覆盖离线模板不可用边界、typed duty proposal、owner 岗位拒绝/批准、D10 关系事实与重复保护、EventLog/行为/关系/AgentMemory SQLite 恢复；目标复跑 Console `13` 条、`0` Warning、`0` Error，随后已清空。
- Q19 仍为 `VERIFIED`（仅限离线船员/owner/save）；在线 LLM、原生 Player 对话 UI/输入/视觉、动作/游泳/碰撞、性能和正式视觉门仍开放。

权威结果：[Q19 当前复核](Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q19 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。下一张可执行卡：Q20 当前 formal resource/presentation 审计。

## 2026-09-13 Q19 当前源码 Pipeline 复核 Luna R2

- 当前源码 `Q19CrewInteractionAuditTests`=`1/1 PASS`（测试 `0.428208s`、总计 `0.45s`）；`Save.Tests`=`377/377 PASS`（总计 `32.88s`）；`dotnet build game/AgentCore/AgentCore.csproj --nologo`=`0 warnings / 0 errors`。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`。Q19 继续 `VERIFIED`，仅限离线船员提议/owner/存档恢复；不升级在线 LLM、native Player 对话 UI/输入/视觉、性能或正式视觉门。

权威结果：[Q19 当前复核 R2](Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q19 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。下一张可执行卡：Q20 当前 formal resource/presentation 审计 R2。

## 2026-09-13 Q19 当前源码 Pipeline 复核 Q03-A17 R1

- 当前源码 `Q19CrewInteractionAuditTests`=`1/1 PASS`；`Save.Tests`=`377/377 PASS`，failed/skipped/inconclusive 均为 `0`；`dotnet build game/AgentCore/AgentCore.csproj --nologo`=`0 warnings / 0 errors`。
- Editor 最终 `ready/stopped`，编译/域重载均为 false，error console 查询=`0`。Q19 继续 `VERIFIED`，仅限离线船员提议/owner/关系事实/AgentMemory 存档恢复。
- 不升级在线 LLM、native Player 对话 UI/输入/视觉、动作/游泳/碰撞表现、性能或正式视觉门禁。

权威结果：[Q19 Q03-A17 R1](Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q19 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。下一张可执行卡：Q20 当前 formal resource/presentation 审计。

## 2026-09-13 Q20 当前源码 Pipeline 复核

- 候选资源 EditMode `D19R03Q20VegetationTests`=`1/1 PASS`（`0.01s`）；R03 旧树/植被接地 PlayMode `Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence`=`1/1 PASS`（`7.1s`），marker 为 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`。
- 当前 D18 catalog 仍 `status=BLOCKED / local_development_ready_count=0 / eligible_entry_count=0`；builder 仍 `RenderAuthoredPalms=false`，正式 Prefab 棕榈引用仍 `0`。Q20 因此保持 `PARTIAL/BLOCKED_ASSET`。
- Editor 最终 `ready/stopped`，Console 检查前 `62` 条、`0` Error、`6` Warning，随后已清空；PlayMode 轮询短暂断连后最终状态文件仍完整 `1/1 PASS`，不改变资源阻塞结论。

权威结果：[Q20 当前复核](Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q20 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。下一张可执行卡：Q21 当前港口空间审计。

## 2026-09-13 Q20 当前源码 Pipeline 复核 Luna R2

- 当前候选资源 EditMode=`1/1 PASS`（测试 `0.0017069s`、总计 `0.01s`）；R03 旧树/植被接地 PlayMode=`1/1 PASS`（测试 `2.2746441s`、总计 `2.4s`）。
- marker 为 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`；D18 catalog 仍 `BLOCKED / 0 / 0`，builder=`RenderAuthoredPalms=false`，正式 Prefab 棕榈引用=`0`。
- Q20 继续 `PARTIAL/BLOCKED_ASSET`，仅限候选接地与旧 Renderer 隔离；Editor 最终 `ready/stopped`，error console=`0`，不升级正式棕榈、全镜头视觉或 VC-G01。

权威结果：[Q20 当前复核 R2](Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q20 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。下一张可执行卡：Q21 当前港口空间审计 R2。

## 2026-09-13 Q21 当前源码 Pipeline 复核

- `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`（测试 `0.1934504s`、总计 `0.21s`）；四个服务建筑各 `37 Renderer / 0 missing`，`min.y≈0.225`，pairwise XZ 无重叠，六个空间组与稳定路线锚点保留。
- 实际 marker 确认 `colliders=23 / terrain_colliders=1 / external_collider_owners=D27_R03_CharacterWildlifePresentation`；Editor 最终 `ready/stopped`，Console `10` 条、`0` Error、`0` Warning 后已清空。
- Q21 仍为 `PARTIAL`：仅限静态 R03 空间/owner 合同；不等于原生 Player 镜头、键鼠可达性、植被准入、性能或最终视觉通过。

权威结果：[Q21 当前复核](Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q21 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。下一张可执行卡：Q22 当前船海天气审计。

## 2026-09-13 Q21 当前源码 Pipeline 复核 Luna R2

- Q21 当前源码 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`（测试 `0.1980183s`、Pipeline `0.23s`）；四个服务建筑各 `37 Renderer / 0 missing`，六个空间组和 pairwise XZ 无重叠，碰撞边界为 `23 / 1 R03 terrain / 22 D27 owner`。
- Editor 最终 `ready/stopped`，error console=`0`，检查后已清空；当前 test/builder/Prefab/InitialHarbor hash 见 R2 结果。
- Q21 仍为 `PARTIAL`：仅限静态 R03 空间/owner 合同；不等于原生 Player 镜头、键鼠可达性、植被准入、性能或最终视觉通过。

权威结果：[Q21 当前复核 R2](Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q21 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。下一张可执行卡：Q22 当前船海天气审计 R2。

## 2026-09-13 Q22 当前源码 Pipeline 复核 Luna R2

- Q22 当前源码 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`（测试 `0.1646664s`、Pipeline `0.17s`）；水面 `96×96`、sea level `-0.0397`、泡沫 `28`、水材质缺失 `0`、船体/船壳水线和 D9/D28 Calm-Rain-Storm 指标均与合同一致。
- Editor 最终 `ready/stopped`，error console=`0`，检查后已清空；当前 test/运行时/Prefab hash 已登记到 Q22 R2 结果。
- Q22 仍为 `PARTIAL`：仅限静态水线/船体/天气 owner 合同；不等于真实 Player 游泳、船海镜头、泡沫/雾/音效、性能、物理键鼠或最终视觉通过。

权威结果：[Q22 当前复核 R2](Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q22 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。下一张可执行卡：Q23 当前正式资源集成审计。

## 2026-09-13 Q22 当前源码 Pipeline 复核

- `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`（测试 `0.1823856s`、总计 `0.19s`）；实际 marker 确认水面 `96×96`、sea level `-0.0397`、泡沫 `28`、水材质缺失 `0`、船壳底线 `-0.0400`、可见船体 `min.y=0.0600`、R03 表现音频源 `0`，D9 鱼类活动和 D28 降水/波动指标均符合合同。
- 当前 connected Pipeline Editor PID `53332` 最终 `ready/stopped`；检查前 Console `12` 条、`0` Error、`0` Warning，随后已清空。
- Q22 继续为 `PARTIAL`：本轮只证明静态船海天气 owner 合同，不覆盖原生 Player 游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉。Q03、Q20、VC-G01 与 D31 既有边界保持不变。

权威结果：[Q22 当前复核](Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q22 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。下一张可执行卡：Q23 当前正式资源集成审计。

## 2026-09-13 Q23 当前源码 Pipeline 复核

- `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`（测试 `0.3831589s`、总计 `0.39s`）；关闭并重开 InitialHarbor 后 active roots、R03/船实例、Renderer、缺失项、碰撞体和水线保持一致。
- 实际 marker 确认 `active_roots=34`、R03/船实例各 `1`、活动 R03 Renderer `361`、缺失材质/脚本 `0`、地形碰撞体 `1`、船体底线 `-0.0400`，港口/船/D27 GUID 均解析有效；Editor 最终 `ready/stopped`，Console `10` 条、`0` Error、`0` Warning 后已清空。
- Q23 继续为 `PARTIAL`：仅限正式场景 Prefab 集成和重开稳定性；不覆盖原生 Player 镜头/键鼠/动作/游泳/船控/音效/性能/逐物体接地或最终视觉。下一张可执行卡：Q24 当前存档恢复诊断。

权威结果：[Q23 当前复核](Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q23 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。下一张可执行卡：Q24 当前存档恢复诊断。

## 2026-09-13 Q23 当前源码 Pipeline 复核 Luna R2

- Q23 当前源码 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`（测试 `0.3594421s`、Pipeline `0.37s`）；重开后一致保持 active roots=`34`、R03/船实例=`1/1`、R03 renderers=`361`、missing material/script=`0`、terrain collider=`1`、ship hull bottom=`-0.0400`，GUID 解析有效。
- Editor 最终 `ready/stopped`，error console=`0`，检查后已清空；当前 scene/Prefab/test hash 已登记到 Q23 R2 结果。
- Q23 仍为 `PARTIAL`：仅限正式场景 Prefab 集成/重开稳定性；不等于原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过。

权威结果：[Q23 当前复核 R2](Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q23 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。下一张可执行卡：Q24 当前存档恢复诊断。

## 2026-09-13 Q24 当前源码 Pipeline 复核

- `D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`（总计 `1.22s`）；陆地往返、出海船位/航向/速度与港口安全出生锚点恢复、注入 `after_player` 故障后保护上一份好档均通过。
- 当前 Save/D2/D7/test hash 已写入结果；Editor 最终 `ready/stopped`，Console `11` 条、`0` Error、`0` Warning 后已清空。
- Q24 继续为 `PARTIAL`：当前 `PlayerState` schema 没有独立游泳状态/姿态字段；本卡不覆盖原生 Player 输入、镜头、动作、游泳视觉、船控、树木、音效、性能或最终视觉。下一张可执行卡：Q25 当前首航/循环集成诊断。

权威结果：[Q24 当前复核](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q24 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。下一张可执行卡：Q25 当前首航/循环集成诊断。

## 2026-09-13 Q24 当前源码 Pipeline 复核 Luna R2

- Q24 当前源码 `D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`（Pipeline `1.19s`）；陆地往返、出海船位/航向/速度与 `D2_PlayerSpawnAnchor` 安全恢复、`after_player` 故障后的上一份好档保护均通过。
- 当前 Save/D2/D7/test hash 已登记；Editor 最终 `ready/stopped`，error console=`0`，检查后已清空。
- Q24 仍为 `PARTIAL`：`PlayerState` 没有独立游泳状态/姿态字段；本卡不覆盖原生 Player 输入、镜头、动作、游泳视觉、船控、树木、音效、性能或最终视觉。

权威结果：[Q24 当前复核 R2](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q24 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。下一张可执行卡：Q25 当前首航/循环集成诊断。

## 2026-09-13 Q25 当前源码 Pipeline 复核

- `D19Q25FirstVoyageLoopDiagnosticTests`=`2/2 PASS`（总计 `0.47s`）；D30 七阶段投影、D23 40 tick 首航/抵达/返港/存档投影及失败/重复保护通过。
- `D19R02A03RoutePlayModeTests`=`2/2 PASS`（总计 `4.72s`）；覆盖 InitialHarbor 交易关闭/保存与出航/推进/返港。运行日志确认 D2/D5/D25/D26/D27/D28/D29/D31 已初始化，但保留程序化 Player fallback、D27 启动等待 Warning 和 D31 性能 pending 边界。
- Pipeline Console API 报告 `126` 条、返回窗口 `100` 条；返回窗口未观察到 Error、观察到 `4` 条已知 Warning，随后已清空；Editor 最终 `ready/stopped`。Q25 继续为 `PARTIAL`，不升级完整人工游玩、原生键鼠、视觉、动作、游泳、船控或性能。下一张可执行卡：Q26 当前测量基线。

权威结果：[Q25 当前复核](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q25 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一张可执行卡：Q26 当前测量基线。

## 2026-09-13 Q25 当前源码 Pipeline 复核 Luna R2

- Q25 当前源码 D30/D23 首航 EditMode=`2/2 PASS`（Pipeline `0.47s`），InitialHarbor route PlayMode=`2/2 PASS`（Pipeline `4.51s`）；覆盖阶段投影、40 tick 首航/返港/保存，以及交易关闭/保存、出航/推进/返港。
- 运行日志仍保留程序化 Player fallback、D27 等待提示和 D31 performance pending；这些不是测试失败。Editor 最终 `ready/stopped`，error console=`0`，检查后已清空。
- Q25 仍为 `PARTIAL`：不覆盖完整人工游玩、原生 WASD/E/I/M/T/Esc、镜头、动作/游泳/上船视觉、树木接地、音效或性能。

权威结果：[Q25 当前复核 R2](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q25 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一张可执行卡：Q26 当前测量基线。

## 2026-09-13 Q26 当前源码 Pipeline 复核

- `D19R03Q26GameplayDefectPlayModeTests`=`16/16 PASS`（总计 `36.31s`，failed/skipped/inconclusive=`0`）；覆盖键盘游泳、E 拒绝/登船/下船、连续水域移动、舵轮、树木/植被、泡沫、船轴、自有地形和 A13/A14/A16/A17 回归。
- 独立 Q26A01 当前源码复跑=`1/1 PASS`（总计 `3.38s`），实际 marker 为 `keyboard=W / swimming=True / action=Swimming / planarDistance=0.4781 / surfaceY=-0.020`。这是运行状态/位移证据，不是游泳姿态截图 PASS。
- 全量 Console API 报告 `949` 条但返回窗口上限为 `100`（窗口 `0` Error、`11` Warning）；A01 独立窗口 `65` 条（`0` Error、`6` Warning），随后已清空；Editor 最终 `ready/stopped`。Q26 仍为 `PARTIAL`，不升级 native Player GPU 画面、正式 Avatar/动作、目标设备性能或 VC-G01。下一张可执行卡：Q27 当前性能/预算证据。

权威结果：[Q26 当前复核](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q26 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一张可执行卡：Q27 当前性能/预算证据。

## 2026-09-13 Q26 当前源码 Pipeline 复核 Luna R2

- `D19R03Q26GameplayDefectPlayModeTests`=`17/17 PASS`（总计 `36.21s`）；独立 Q26A01=`1/1 PASS`（总计 `3.14s`），实际 marker=`keyboard=W / swimming=True / action=Swimming / planarDistance=0.4683 / surfaceY=-0.020`。
- 当前运行日志仍明确未验证导入蒙皮、procedural/neutral Player fallback 和 D31 runtime performance pending；Editor 最终 `ready/stopped`，error console=`0`，检查后已清空。
- Q26 仍为 `PARTIAL`：这是代码级状态/owner/碰撞回归证据，不是原生 Player 物理键盘、GPU 画面、游泳姿态、正式 Avatar/动作、目标设备性能或 VC-G01 视觉通过。

权威结果：[Q26 当前复核 R2](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)；交接：[Q26 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一张可执行卡：Q27 当前性能/预算证据。

## 2026-09-13 Q27 当前源码 Pipeline 复核

- `D16PerformanceBudgetTests`=`6/6 PASS`（`0.02s`）；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`（测试 `62.5509786s`），探针 owner 和 `BudgetSatisfied=true`。
- `Null Device` 下实际 60.0038 秒/164 samples：平均 `30.5038 FPS`、平均帧时间 `151.8086ms`、最长帧 `1153.7593ms`、超过 50ms 长帧 `84`、GPU timing 不可用。既有非 headless R3 参考仍有 `21.4605 FPS 1% Low` 与 `622.2581ms` 最长帧。
- 测试前 Console `63` 条、`0` Error、`6` Warning，随后已清空；Editor 最终 `ready/stopped`。Q27 继续为 `PARTIAL`，不升级目标设备性能、视觉或 Q28-Q31。下一张可执行卡：Q28 新候选 Player 构建。

权威结果：[Q27 当前复核](Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；交接：[Q27 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。下一张可执行卡：Q28 新候选 Player 构建。

## 2026-09-13 Q28 当前源码 Pipeline 构建复核

- connected Unity Pipeline `build`/`build_status` 生成 Q28 R5：`build_d39c84c19144`，dry-run=`valid=true`，目标 `StandaloneWindows64`，场景 `MainMenu.unity + InitialHarbor.unity`，`Succeeded`，总产物 `175190134` bytes，耗时 `42221ms`，`0` 构建错误、`9` 警告。
- 新候选 `Q28_CANDIDATE_PLAYER_R5.exe` SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；内置 `Assembly-CSharp.dll` SHA256=`40CE82FFB9A86A7DB9A4FE605BB2C60229493EA847EA4754028F521F7DA059BF`；未覆盖 R1-R4。
- 构建后 Editor=`ready/stopped`，Console 构建前 `12` 条且 `0` Error，清空后 `0` 条。主要 warning 明确说明 `enableInBuilds=false`，Pipeline 不进入 Player；本卡没有 Player、前台窗口、物理键盘、截图、GPU 或视觉结论。
- Q28 保持 `PARTIAL`；权威结果：[Q28 当前 R5 结果](Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)，下一张可执行卡为 Q29 隔离 Player 路线/输入证据。

## 2026-09-13 Q29 当前 Pipeline 隔离 Player 交互复核

- 当前源码构建 `build_83c89c977e86` 生成 `Q29_RUNTIME_PIPELINE_QA_R5.exe`：`StandaloneWindows64`、`Development + DetailedBuildReport`、`Succeeded`、`0 errors / 0 warnings`；EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`，Assembly-CSharp SHA256=`69345D2453AF034D9A92374F6848E92752C0D02CE37FAA737EF0A5AA09DA3921`。
- 通过 Unity Pipeline runtime `127.0.0.1:7900` 隐藏运行 Player：MainMenu → InitialHarbor、W 移动/Walk、仓库模态 W 阻断、I/M/T 打开/重复/关闭、E 登船/下船、Y 出航、航行中 M owner 拒绝、Ship W/A 速度与转向、Esc `TimeScale=0→1` 均有真实 runtime marker；Player PID `75896` 优雅退出，错误查询返回 `0` 条。
- 本轮没有前台截图或物理键盘；运行结束已恢复 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`。R5 结果与 marker 归档见 [Q29 当前 R5 结果](Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 和 [R5 console capture](Q29/Q29_RUNTIME_PIPELINE_QA_R5_PIPELINE_CONSOLE_CAPTURE.log)。
- Q29 继续 `PARTIAL`：该证据只覆盖隐藏 Player 输入/状态，不覆盖截图可见的动作、游泳姿态、穿模、树木接地、材质、镜头和目标设备性能；VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。下一张可执行卡：Q30 当前源码独立证据审查。

## 2026-09-13 Q30 当前源码独立证据审查

- 当前源码定向 Python 回归 `65/65 PASS`；Unity Pipeline 目标回归为 `D16Accessibility=8/8`、`D16InputRouter=10/10`、`D17ActionQuality=16/16`、`VcG01WorldLogic=14/14`，Q26 当前缺陷套件 `16/16` 与 Q26A01 `1/1` 既有证据继续有效。
- Python 全量 discovery 本轮只得到持续点号输出，约 `215s` 后停止本轮启动的 PID `16100`，无完成 verdict；该项记录为 `INCOMPLETE`，不计 PASS。
- Q29 R5 隐藏 Player 真实运行补充了 W/菜单模态隔离、I/M/T 重复/关闭、E 生命周期、Y 出航、航行中 M 拒绝、Ship W/A 和 Esc `0→1` 的状态证据；没有截图，因此动作、泳姿、舵位接触、树木接地、材质、镜头和目标设备性能仍为 `PARTIAL/UNVERIFIED`。
- Q30 保持 `PARTIAL`；Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`、Q27=`PARTIAL`，VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`。权威结果：[Q30 当前复核](Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)，交接：[Q30 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。下一张可执行卡：Q31 最终 native Player 视觉门禁。

## 2026-09-13 Q31 当前源码 D2/hidden Player 复核

- D2 新增水下可行走地形防护：从游泳体积上方向下探测 D19 单面 MeshCollider 的可行走顶面，重叠时退出游泳并恢复到地面；新增 `D17A17WaterOverlapRejectsSwimmingUnderWalkableTerrain`。
- 当前源码 recompile=`failed=false/errorCount=0`；D17 水域/镜头/输入/整合/防护定向序列=`9/9 PASS`。R2 hidden Player build=`build_33970c8639b5`，`0 errors / 7 warnings`；用户报告坐标由修复前 `swim=true/grounded=false/y=0.400` 变为修复后 `swim=false/grounded=true/y=1.003`。
- 开放水域 hidden Player `W` 产生 `speed=2.073093 / action=Swimming`；runtime error console=`0`，Player clean close，descriptor/PID 清除；QA 后 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`。
- 证据见 [Q31 当前 Pipeline 结果](Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q31 交接](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。重叠恢复帧仍近墙/遮挡且看不到玩家，开放水域仍低模/程序化，因此 Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`，不升级最终视觉门。

## 2026-09-13 Q07 R03 服务建筑碰撞 owner 修复

- [Q07 当前建筑碰撞结果](Q07/Q07_CURRENT_BUILDING_COLLISION_PIPELINE_20260913_RESULT.json) 与 [Q07 交接](../../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)：诊断确认四栋可见服务建筑此前均为 `37 Renderer / 0 Collider`；`HarborCollisionController` 现在把四个 `Body` bounds 转为 D6-owned、enabled、non-trigger 的外壳 BoxCollider。
- 验证：Q07 定向 `1/1 PASS`；D17 `16/16 PASS`；D19R03Q26 `16/16 PASS`；当前源码 Development Player `build_c3588a5c004b` 成功，0 errors/7 warnings；hidden Player 进入 `InitialHarbor` 后四个碰撞体与四条 raycast 均命中 D6 owner，runtime error console `0`，Player.log 严格 `ERROR/Exception/Crash/Fatal=0`，clean close。
- 边界：这只关闭“可见服务建筑没有 Harbor 碰撞 owner”的明确根因，不等于相机构图、门口/路线语义、动作/泳姿、树木、物理前台输入、目标设备性能或最终视觉通过。Q07 保持 `VERIFIED`（扩展 owner 范围），Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`。

## 2026-09-13 Q15-Q17 当前源码 Pipeline rerun2

- Q15 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`：`1/1 PASS`，测试 `20.1427708s` / Pipeline `20.25s`；I/M/T 各 10 轮、互斥、重复请求拒绝与 Gameplay 恢复 marker 均为真。
- Q16 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`：`1/1 PASS`，测试 `3.7009758s` / Pipeline `3.81s`；owner 未就绪、双击单 owner、买卖各一次变化、余额不足和满仓不变 marker 均为真。
- Q17 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`：`1/1 PASS`，测试 `4.319115s` / Pipeline `4.43s`；修船/船坞成功、拒绝、幂等、取消、异常恢复和主输入恢复 marker 均为真。
- 三张卡均为当前源码、无产品代码改动；Editor 最终 `ready/stopped`、`compiling=false`、`domainReloadInProgress=false`，Error Console 查询 `0`，marker 检查后已清空 Console。三份 rerun JSON 已分别登记在 Q15/Q16/Q17 目录。
- 范围边界不变：这些证据只关闭 PlayMode 逻辑/owner 桥，不关闭前台物理键鼠、native Player 截图、动作/泳姿/穿模/植被视觉、目标设备性能或 VC-G01。下一张可执行卡为 Q18。

## 2026-09-13 Q28 当前源码 R6 候选重建

- Q07 HarborCollisionController 在 R5 之后发生了当前源码改动，因此通过 connected Unity Pipeline 重新构建唯一新命名空间 `Q28_CANDIDATE_PLAYER_R6.exe`，build id=`build_d0507c23b205`，`StandaloneWindows64 / Succeeded / 0 errors / 1 warning`，耗时 `28095ms`。
- R6 EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；`Assembly-CSharp.dll` SHA256=`99A80591C21B15C693B5A518C68C87570828A76B9C9BB0062E99EB1CAFB0A7E8`；包含当前 HarborCollisionController SHA256=`E04E24C957F25966C687C49B271629E411762BC0168BF43940256E100EFD71FD`。
- 构建设置保持非开发包：`developmentBuild=false / allowDebugging=false / connectWithProfiler=false / enableInBuilds=false / port=0`；Editor=`ready/stopped`、Error Console=`0` 后清空。没有 Player、前台窗口、物理输入或截图结论。
- Q28 仍 `PARTIAL`；权威结果：[Q28 R6 当前构建](Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)，下一张可执行卡为 Q29 R6 隔离 Player 路线与证据。

## 2026-09-13 Q29 当前源码 R6 隔离 Player rerun2

- R6 `build_6d7be34d6558`：`StandaloneWindows64 / Development + DetailedBuildReport / Succeeded / 0 errors / 0 warnings`，通过 hidden Windows Player 在 `127.0.0.1:7901` 执行当前源码路线。
- 使用正确的 `down/wait/up` 注入后，MainMenu NewGame/Confirm、W 移动、仓库模态 W 阻断、I/M/T 打开/重复/Esc、E 上下船/再上船、Y 出航、航行中 M owner 拒绝、Ship W/A 位移、Esc `0→1` 均有状态和日志证据；同一 R6 还读到四个 Q07 D6-owned non-trigger building collider。
- Runtime Error Console=`0`；Player PID clean close；Player log strict uppercase `ERROR/EXCEPTION/CRASH/FATAL=0`。QA 后已复核 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`。
- 第一次 `action=press` 探索序列被明确丢弃，不计 PASS；R6 结果见 [Q29 rerun2](Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)，原始 marker 见 [R6 console capture](Q29/Q29_RUNTIME_PIPELINE_QA_R6_PIPELINE_CONSOLE_CAPTURE.log)。Q29 仍 `PARTIAL`：没有前台物理键盘、截图、动作/泳姿/植被/穿模视觉或目标设备性能证据。下一张可执行卡为 Q30 独立审查。

## 2026-09-13 Q30 current-source independent review rerun2

- 当前源码定向 Python=`65/65 PASS`；有界全量 discovery 无 completion verdict，记录为 `INCOMPLETE`。
- Unity Pipeline 当前回归：D16Accessibility=`8/8`、D16InputRouter=`10/10`、D17ActionQuality=`16/16`、VcG01WorldLogic=`14/14`；Q07 building collision owner=`1/1`；Q15/Q16/Q17 rerun2 各=`1/1`。
- Q28 R6=`Succeeded / 0 errors / 1 warning`；Q29 R6 hidden Player 路线完成，runtime error query=`0`，clean close，QA 配置已恢复。
- Q30 保持 `PARTIAL`：截图、前台物理键盘、正式动作/Avatar、正式植被、目标设备性能和最终视觉仍未关闭。Q31 是下一张可执行卡；VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。

权威结果：[Q30 rerun2](Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)；命令捕获：[Pipeline capture](Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_PIPELINE_CAPTURE.log)；交接：[Q30 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-13 Q31 当前源码 RenderTexture 视觉诊断 rerun2

- Unity Pipeline hidden GUI Player 已在不抢占桌面的条件下取得三张有效当前源码 RenderTexture PNG：InitialHarbor、开放水域静态帧、按住 S 的移动游泳帧；全部为非黑且 SHA256 已登记。
- 传统 ScreenCapture 路径仍为 `INVALID_BLACK_FRAME`，保留为失败路径证据，不计视觉结论。有效帧直接显示当前低模/程序化港口与游泳轮廓，确认游泳状态/位移与视觉质量仍需分开判定。
- Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`；本卡没有升级错误查询、前台物理键盘或人工视觉状态。

权威结果：[Q31 current visual diagnostic](Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_RESULT.json)；[Pipeline capture](Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_PIPELINE_CAPTURE.log)；交接：[Q31 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-13 Q31 生成舵轮暖木对比与 Player 复核

- `D19R03HarborPresentationBuilder` 将生成 `HelmWheel` 从近黑蓝灰改为低光泽暖木 `RGBA(0.340,0.160,0.055,1)`；新增 `Q26A18GeneratedHelmWheelUsesWarmWoodContrast` 防止材质退化为黑色遮挡环。
- 验证：recompile `failed=false/errors=[]`；Q26A18 `1/1`、D19R03Q26 `17/17`、D17 `16/16`、VcG01WorldLogic `14/14`；`build_467144669770`=`Succeeded / 0 errors / 0 warnings`。
- Unity Pipeline clean hidden GUI Player 通过 pointer NewGame/Confirm 和 `E down/wait/up` 到达 `InitialHarbor` 并上船；`parent=ph_ship_fishing`、材质 `D7Polish_HelmWheel_0 RGBA(0.340,0.160,0.055,1.000)`、双手同高；runtime error console=`0`，clean close，QA 后 `enableInBuilds=false / port=0 / developmentBuild=false`。
- [Q31 helm result](Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_RESULT.json) 与 [Pipeline capture](Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_PIPELINE_CAPTURE.log) 已登记；[有效 RenderTexture 帧](Q31/Q31_QA_HELM_WARM_WOOD_BOARDING_FORWARD_RT_20260913_R2.png) SHA256=`A9B02BED4977AAEA721D3B65DD902E26E5905A4C547DD192DEBD109B4162A7B2`。
- 该帧只证明有限的舵轮可读性改善；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 不变。自然 IK/握舵、正式动作/Avatar、自然泳姿、植被接地、穿模、前台物理键盘、目标设备性能和人工最终验收仍开放。

## 2026-09-13 Q00/Q01 接管 R2 与 Q04 当前水域重绑定

- Q00/Q01 使用 connected Unity Pipeline PID `53332` 对当前工作区重做有界接管/根因采样；当前 worktree=`10562` status entries（tracked=`459`、untracked=`10103`），`git diff --shortstat=459 files changed, 40082 insertions(+), 49167 deletions(-)`，无清理/reset/checkout。Editor 最终 `ready/stopped`、RuntimePipeline=`enableInBuilds=false / port=0`、error console 查询=`0`。
- Q01 当前运行事实：D2 根=`(0.0000,0.1497,-2.7000)`、单位缩放；HumanoidVisual=`0.6642`、17 renderers；D2 初始 grounded、非 swimming、source=`D16InputRouter`；默认 `Procedural / FormalAnimatorReady=false`；`ph_ship_fishing` scale=`2.5740`；D7 游戏舵点与可见舵锚点=`(0,1.100,-6.680)`；scoped R03 MeshCollider enabled/non-trigger。Q03/Q20/VC-G01/D31 视觉边界不升级。
- Q04 由于旧结果绑定较早 D2 hash，本轮用当前源码重跑 D17A07，`1/1 PASS`，测试=`10.0865023s`、总计=`10.2s`，Editor 健康、error console=`0`。Q04 仍只表示 D2 水域 owner 的十次浅/深水逻辑，不表示 Player W/S、前台焦点、正式泳姿或水面视觉通过。
- 权威结果：[Q00 Luna R2](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_LUNA_R2_20260913_RESULT.json)、[Q01 Luna R2](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_LUNA_R2_20260913_RESULT.json)、[Q04 Luna R2](Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。下一张执行卡：Q05 当前 Pipeline 游泳表现复核。

## 2026-09-13 Q05 当前源码游泳表现契约重绑定 R2

- 旧 Q05 当前结果绑定较早 D2 hash，本轮使用当前源码重跑 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`，完成 `1/1 PASS`；测试 `12.338054s`、Pipeline `12.45s`。
- 证据确认 whole-visual-root 游泳倾斜、离水 neutral 恢复和物理根 upright；Editor=`ready/stopped`，error console 查询=`0`。正式四肢/头部动作、自然泳姿、native Player W/S、前台焦点和最终视觉仍开放。
- Q05 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`；权威结果：[Q05 Luna R2](Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。下一张执行卡：Q06 当前 Pipeline 地形/落地复核。

## 2026-09-13 Q06 当前源码跳跃/接地契约重绑定 R2

- 使用当前源码重跑 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite`，结果 `1/1 PASS`；测试 `13.2231615s`、Pipeline `13.34s`。
- 证据确认十次 TryJump—落地循环和出生点附近正/斜/横/反向小步路径保持 grounded、非游泳、Transform 有限；Editor=`ready/stopped`，error console 查询=`0`。
- Q06 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，不覆盖墙/坡/岸线/跳板/船甲板、视觉网格接触、native Player 键盘或 VC-G01。权威结果：[Q06 Luna R2](Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。下一张执行卡：Q07 当前碰撞路线复核。

## 2026-09-13 Q07 当前源码碰撞 owner/路线复核 R2

- 当前源码重跑四栋服务建筑碰撞 owner：`1/1 PASS`，测试 `2.284088s`；重跑地形/码头/跳板双向路线：`1/1 PASS`，测试 `4.0585166s`。
- 两次执行后 Editor=`ready/stopped`、error console 查询=`0`；当前 HarborCollisionController、Q07 building test 与 D17 test hash 均已登记。
- Q07 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，范围仅限 Harbor collision owner 和自动化路线；不覆盖所有坡墙/岸线、视觉网格接触、船上路线、native WASD、穿模或 VC-G01。下一步：登记 Q08 的 Q03 资源阻塞，再进入 Q09。
- 权威结果：[Q07 Luna R2](Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q08 当前依赖阻塞复核 R2

- Q03 formal 资源门仍未解除：15 个 formal clip 仍为 `humanMotion=0 / genericRoot=15`，默认 formal consumer=`0`，没有合法 current-rig 地面动作资源/可审计骨骼轴适配器。
- Q05 当前 `D17A08=1/1`、Q07 当前 building owner=`1/1` 且 D17A10=`1/1` 仅是子契约证据，不足以运行或通过 Q08 完整输入→状态→位移→姿态整合。
- Q08 保持 `BLOCKED`，不是失败也不是 PASS；下一张独立可执行卡为 Q09 当前 Pipeline 甲板/舵位路线复核。权威结果：[Q08 Luna R2](Q08/Q08_CURRENT_BLOCKED_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q09 当前源码甲板锚点/登船复核 R2

- 当前源码重跑 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`，结果 `1/1 PASS`；测试 `3.9577833s`、Pipeline `4.07s`。
- 证据覆盖非单位船根舵点、十次真实登/下船和 30m 远距拒绝；Editor=`ready/stopped`，error console 查询=`0`。
- Q09 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`；不覆盖自然握舵/手轮接触、船体视觉清除、航行中甲板关系、native Player 输入或 VC-G01。下一张执行卡：Q10 当前航行中甲板关系复核。权威结果：[Q09 Luna R2](Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q10 当前源码航行甲板关系复核 R2

- 当前源码重跑 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`，结果 `1/1 PASS`；测试 `2.8314083s`、Pipeline `2.95s`。
- 十个变化中的船舶平移/转向快照保持登船玩家相对局部位置/旋转稳定，并能恢复正常下船；Editor=`ready/stopped`，error console 查询=`0`。
- Q10 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，不覆盖开放海、native W/S/A/D、船舶碰撞、镜头、自然握舵或 VC-G01。下一张执行卡：Q11 当前安全下船复核。权威结果：[Q10 Luna R2](Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q11 当前源码安全下船复核 R2

- 当前源码重跑 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`，结果 `1/1 PASS`；测试 `4.1991542s`、Pipeline `4.31s`。
- 十次合法下船、活动 terrain bounds 外无落点拒绝、恢复快照后再下船均通过；Editor=`ready/stopped`，error console 查询=`0`。
- Q11 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`；不覆盖移动船舶视觉碰撞、所有岸边/跳板出口、native Player、正式动作、相机或 VC-G01。下一张执行卡：Q12 当前相机复核。权威结果：[Q11 Luna R2](Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q12 当前源码相机复核 R2

- 当前源码重跑 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`，结果 `1/1 PASS`；测试 `4.44514s`、Pipeline `4.56s`。
- 十次模式/目标切换、受控 SphereCast 遮挡收缩和移除后阻尼恢复通过；Editor=`ready/stopped`，error console 查询=`0`。
- Q12 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，不覆盖最终场景镜头构图/穿模、native Player 激活或 VC-G01。下一张执行卡：Q13 当前输入焦点与 HUD 隔离复核。权威结果：[Q12 Luna R2](Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json)。

## 2026-09-13 Q03-A17 Formal Humanoid 游泳姿态候选

- D20 Formal Humanoid 隔离候选收窄游泳肌肉幅度并修复 Q03D20 两帧基线竞态；recompile=`failed=false/errors=[]`，Q03D20=`4/4 PASS`，D17=`16/16 PASS`。
- `build_21e1ee070799` StandaloneWindows64=`0 errors / 1 warning`；hidden Pipeline Player 的 Formal/Humanoid bridge 已安装，组件诊断返回 `Swimming/Fish/pitch=70`；RenderTexture 对比只确认折膝减轻，手臂仍风格化。
- Q03-A17=`PARTIAL`，Q03 仍 `BLOCKED_ASSET`，Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`。结果：[Q03-A17](Q03-A17/Q03_A17_FORMAL_HUMANOID_PIPELINE_R3_RESULT.json)；交接：[Q03-A17 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A17/Q03-A17.md)。

## 2026-09-13 Q00/Q01 当前源码与 Q03-A17 Player 重绑定 R1

- Q00 重新核对 dirty worktree、单一 connected Editor、Q03-A17 两个变更源文件与 `build_21e1ee070799` exact Player hash；最终 Editor=`ready/stopped`、RuntimePipeline=`enableInBuilds=false/port=0`、error console=`0`。
- Q01 通过同一 Pipeline Editor 对 InitialHarbor 做 bounded direct eval：D2 根/视觉根/胶囊/输入 owner、船舵点、R03 MeshCollider 和水面值均有当前运行态证据；动作扭曲、自然游泳、前台输入、握舵接触、穿模、树木全镜头接地仍未被 PASS。
- Q00/Q01 均为 `VERIFIED_WITH_SCOPE`，不升级 Q03=`BLOCKED_ASSET`、Q20/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS` 或 VC-G02/VC-G03=`LOCKED`。下一步已转为 Q04 当前 D2 水域 owner 回归。

结果：[Q00](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A17_20260913_RESULT.json)、[Q01](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A17_20260913_RESULT.json)；聚合：[Q00](Q00/RESULT.json)、[Q01](Q01/RESULT.json)。

## 2026-09-13 Q04 current-source water owner rerun after Q03-A17

- `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles` completed in the connected Editor: `1/1 PASS`, test `12.764308s`, total `12.91s`, failed/skipped/inconclusive=`0`.
- Current D17/D2/OceanAdapter hashes are registered; Editor=`ready/stopped`, error console=`0`. Q04 remains `PASS_WITHIN_SCOPE` for D2 water-state ownership only.
- This does not prove native Player W/S, keyboard focus, authored swim motion, water material, screenshot visual acceptance or VC-G01. Next: Q05.

Result: [Q04 Q03-A17 后 R1](Q04/Q04_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q04](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md)。

## 2026-09-13 Q05 current-source swim pose contract rerun after Q03-A17

- `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot` completed in the connected Editor: `1/1 PASS`, test `12.7597425s`, total `12.9s`, failed/skipped/inconclusive=`0`.
- The current evidence confirms ten whole-visual-root tilt/neutral-restore cycles and an upright physical root; Editor=`ready/stopped`, error console=`0`.
- Q05 remains `PASS_WITHIN_SCOPE`; this is not authored limb/head motion, natural swim quality, native Player input or screenshot visual acceptance. Next: Q06.

Result: [Q05 Q03-A17 后 R1](Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q05](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)。

## 2026-09-13 Q06 current-source jump/grounded rerun after Q03-A17

- `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite` completed in the connected Editor: `1/1 PASS`, test `12.928311s`, total `13.07s`, failed/skipped/inconclusive=`0`.
- Evidence covers ten jump-to-ground cycles and four declared small-step directions with grounded/non-swimming/finite Transform assertions; Editor=`ready/stopped`, error console=`0`.
- Q06 remains `PASS_WITHIN_SCOPE`; walls, slopes, shore/gangplank, ship deck, visual mesh contact and native Player keyboard remain open. Next: Q07.

Result: [Q06 Q03-A17 后 R1](Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q06](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)。

## 2026-09-13 Q07 current Harbor collision owner/route rerun after Q03-A17

- `R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries`: `1/1 PASS`, test `2.1973986s`, total `2.33s`；`D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps`: `1/1 PASS`, test `3.9754098s`, total `4.12s`。
- Current HarborCollision/building/D17 hashes are registered; both runs ended Editor=`ready/stopped`, error console=`0`。
- Q07 remains `PASS_WITHIN_SCOPE` for the tested collision owners and automated bidirectional routes. Q08 remains blocked by Q03 assets; next independent card: Q09.

Result: [Q07 Q03-A17 后 R1](Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q07](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。

## 2026-09-13 Q08 current dependency gate after Q03-A17

- Q08 remains `BLOCKED`: Q03 formal authored ground-action/avatar admission is still closed. Q03-A17 is an opt-in swim candidate and does not satisfy that prerequisite.
- Q05 R1 and Q07 R1 are retained only as bounded sub-contract evidence; no Q08 integration test was run, and no neutral/generic motion was promoted to PASS.
- Unblock requires a current-rig-compatible ground-action clip set or auditable Avatar/bone-axis adapter, followed by a same-source Q08 integration rerun. Independent next card: Q09.

Result: [Q08 current blocked R1](Q08/Q08_CURRENT_BLOCKED_20260913_Q03A17_R1_RESULT.json)；handoff: [Q08](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。

## 2026-09-13 Q09 current scaled deck/boarding rerun after Q03-A17

- `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding` completed: `1/1 PASS`, test `2.447238s`, total `2.55s`, failed/skipped/inconclusive=`0`.
- Evidence covers the non-unit ship-root HelmPoint, ten boarding/disembarking cycles and 30m remote rejection; Editor=`ready/stopped`, error console=`0`.
- Q09 remains `PASS_WITHIN_SCOPE`; natural helm contact, ship motion, native Player input and visual acceptance remain open. Next: Q10.

Result: [Q09 Q03-A17 后 R1](Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q09](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)。

## 2026-09-13 Q10 current ship-motion/deck relation rerun after Q03-A17

- `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` completed: `1/1 PASS`, test `1.649084s`, total `1.75s`, failed/skipped/inconclusive=`0`.
- Ten changing ship translation/heading snapshots kept the boarded player’s relative deck position/rotation stable and restored normal disembark; Editor=`ready/stopped`, error console=`0`.
- Q10 remains `PASS_WITHIN_SCOPE`; it does not prove open-sea navigation, native Player W/S/A/D, ship collision, camera composition or natural helm contact. Next: Q11.

Result: [Q10 Q03-A17 后 R1](Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q10](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。

## 2026-09-13 Q11 current safe-disembark rerun after Q03-A17

- `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround` completed: `1/1 PASS`, test `3.365028s`, total `3.48s`, failed/skipped/inconclusive=`0`.
- Ten legal disembarks, activity-bounds no-ground rejection and restored legal disembark passed; Editor=`ready/stopped`, error console=`0`.
- Q11 remains `PASS_WITHIN_SCOPE`; moving-ship visual collision, all shore exits, native Player input, authored action quality and camera remain open. Next: Q12.

Result: [Q11 Q03-A17 后 R1](Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q11](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。

## 2026-09-13 Q12 current camera rerun after Q03-A17

- `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose` completed: `1/1 PASS`, test `3.2043464s`, total `3.31s`, failed/skipped/inconclusive=`0`.
- Ten camera mode/target cycles, controlled SphereCast obstruction shrink and damped recovery passed; Editor=`ready/stopped`, error console=`0`.
- Q12 remains `PASS_WITHIN_SCOPE`; final scene composition/clipping, native Player activation and VC-G01 remain open. Next: Q13.

Result: [Q12 Q03-A17 后 R1](Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q12](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。

## 2026-09-13 Q13 current input/UI isolation rerun after Q03-A17

- `D17A15GameplayKeyboardDoesNotNavigateHudAndPauseRestoresModalOwnership`: `1/1 PASS`, test `3.0398572s`, total `3.15s`；`D16InputRouterPlayModeTests`: `10/10 PASS`, total `3.72s`。
- Automated evidence confirms W/S gameplay ownership versus UGUI navigation, Esc modal restore and D16 synthetic keyboard/gamepad/rebind routes; Editor=`ready/stopped`, error console=`0`。
- Q13 remains `PASS_WITHIN_SCOPE`: this is Unity Input System test-device evidence, not foreground physical Win32 keyboard or rendered HUD acceptance. Historical D16 `9/10` NaN watch item remains retained. Next: Q14.

Result: [Q13 Q03-A17 后 R1](Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q13](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。

## 2026-09-13 Q14 current shore/water/deck integration rerun after Q03-A17

- `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence` completed: `1/1 PASS`, test `2.3439029s`, total `2.46s`, failed/skipped/inconclusive=`0`.
- The bounded route covered shore→water→shore→boarding→ShipDeck→W+D turn→no-ground rejection→legal disembark; Editor=`ready/stopped`, error console=`0`.
- Q14 remains `PASS_WITHIN_SCOPE`; `imported_neutral` is a safety state, not formal action admission. Physical foreground input, E/I/M/T business, final visuals and VC-G01 remain open. Next: Q15.

Result: [Q14 Q03-A17 后 R1](Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；handoff: [Q14](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。

## 2026-09-13 Q31 当前 Pipeline HUD/WASD 与视觉诊断 R1

- 状态：`PARTIAL`；VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`。
- 已证实：Pipeline 隐藏 Player 可进入 `InitialHarbor`；`I` 仓库 modal 打开后，`W` 不移动玩家且不改变 UI 选择；关闭后导航策略恢复。新增回归测试目标 `1/1 PASS`，D16 输入类 `11/11 PASS`。
- 未证实：前台 Win32 物理键盘、正式 Avatar/动作、自然游泳、植被接地/穿模、目标设备性能和最终视觉。RenderTexture 仍为 procedural fallback 诊断帧，不升格为视觉 PASS。
- 结果：[Q31 R1](Q31/Q31_CURRENT_PIPELINE_HUD_VISUAL_DIAGNOSTIC_20260913_R1_RESULT.json)；交接：[Q31](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31/Q31.md)。

## 2026-09-13 Q26-A19 D19 helm support wood owner

- 状态：`PASS_WITHIN_SCOPE`；父任务 Q26=`PARTIAL`，Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`。
- 修复：候选 `HelmStand/stand/pedestal/base` 和生成 fallback 支架现在使用 ship wood；`HelmWheel/rudder/steering` 使用 helm material。修复前真实定向 `0/1 FAIL`（`R03_Stone`），修复后 runtime=`D7Polish_HelmStand_0 / brown_planks_03_albedo`。
- 验证：Q26-A19 `1/1 PASS`；`D19R03Q26GameplayDefectPlayModeTests=18/18 PASS`；release-like QA=`0 errors/2 warnings`；Development QA=`0 errors/1 warning`；Player clean close；Editor=`ready/stopped`；Runtime Pipeline 已关闭。
- 结果：[Q26-A19](Q26-A19/RESULT.json)；捕获：[Pipeline capture](Q26-A19/Q26_A19_HELM_SUPPORT_WOOD_PIPELINE_CAPTURE.log)；交接：[Q26-A19](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A19/Q26-A19.md)。RenderTexture 仍仅是诊断证据，不升级完整视觉门禁。

## 2026-09-13 Q26-A21 procedural swim continuity

- 状态：`PASS_WITHIN_SCOPE`；父任务 Q26=`PARTIAL`，Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，Q03=`BLOCKED_ASSET`。
- 变更：新增 `Q26A21ProceduralSwimPoseHasMeasuredContinuousStroke`，要求默认 Player 的 `Procedural/Swimming` 表现源、水面附近有界 pitch/body-drop，并测量 72 帧上臂/脚部相位变化。
- 验证：定向 `1/1 PASS`；`D19R03Q26GameplayDefectPlayModeTests=19/19 PASS`；marker=`pitch=64.0/bodyDrop=-0.601/maxArmDelta=6.47/maxFootDelta=1.94`；hidden Player W 移动、RenderTexture 有效、Player clean close、Editor ready/stopped、Runtime Pipeline 已关闭。
- 结果：[Q26-A21](Q26-A21/RESULT.json)；捕获：[Pipeline capture](Q26-A21/Q26_A21_SWIM_POSE_PIPELINE_CAPTURE.log)；交接：[Q26-A21](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A21/Q26-A21.md)。这不是 formal authored swim 或人工视觉 PASS。

## 2026-09-13 Q00/Q01 当前来源与根因重绑定：Q26-A19/Q26-A21 R1

- Q00 已把当前 D19 builder/prefab、D2 runtime、Q26-A21 测试 hash 与 `Q31_Q26A19_HELM_WOOD_DEV_R1.exe` 的精确边界重新登记；Q26-A21 测试文件是在该 Player 构建之后加入的，不宣称已进入该二进制。
- Q01 只读复核保持 `VERIFIED_WITH_SCOPE`：当前根/尺度/owner 数值与 Q26-A19/A21 前一致；一次探索性 Pipeline eval timeout/reset 被排除，最终 Editor=`ready/stopped`、error console=`0`。
- 状态不升级：Q03=`BLOCKED_ASSET`、Q20/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。隐藏 Player、Pipeline 注入、RenderTexture 不替代前台物理键盘或人工视觉验收。

结果：[Q00 current rebinding](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A21_20260913_RESULT.json)、[Q01 current diagnosis](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A21_20260913_RESULT.json)；交接：[Q00](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q00.md)、[Q01](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q01.md)。下一张执行卡：Q05 当前 Pipeline 游泳表现回归。

## 2026-09-13 Q26-A22 D29 HUD bootstrap 输入 owner

- 状态：`PASS_WITHIN_SCOPE`；Q26=`PARTIAL`，Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`。
- 生产修复：D29 非模态玩法 HUD 在 `AssignDefaultActions()` 后立即清空 `InputSystemUIInputModule.move`，消除等待稳定器下一帧期间的 W/S→UGUI 导航窗口；D16 测试增加即时断言。
- 验证：定向 `1/1 PASS` + `1/1 PASS`；`D16InputRouterPlayModeTests=11/11 PASS`；`D19R03Q26GameplayDefectPlayModeTests=19/19 PASS`；`build_236bb62b66a6=0 errors/0 warnings`。
- Unity Pipeline hidden Player 已真实执行 MainMenu→InitialHarbor、W、I 仓库打开/关闭、模态 W、Esc 暂停/恢复、clean quit；runtime error console=`0`，最终 Editor=`ready/stopped`，Runtime Pipeline 与 Development Build 已恢复关闭。
- 边界：这是插件注入 Input System 的运行证据，不是前台 Win32 物理键盘或人工视觉 PASS；正式 Avatar/action、自然泳姿、植被/穿模、舵位接触、目标设备性能仍开放。runtime `eval` 因 build 未带 script-debugging 返回 `Platform Not Supported`，已排除。

结果：[Q26-A22](Q26-A22/RESULT.json)；捕获：[Pipeline log](Q26-A22/Q26_A22_UI_BOOTSTRAP_PIPELINE_CAPTURE.log)；交接：[Q26-A22](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A22/Q26-A22.md)。Q31 交互矩阵已在独立结果中完成；下一步选择一个有明确 owner 和当前失败证据的剩余缺陷。

## 2026-09-13 Q31 current Player interaction matrix on Q26-A22

- Unity Pipeline hidden Player 完成 E 登船/下船、港内 M 成功、M 重复无重复打开、T 打开/重复无重复打开、Esc 关闭，以及 Y/N 出航后 M 业务拒绝。
- 运行日志明确记录 `Ship/Character` 控制权、作者下船锚点、市场/酒馆成功与重复无新 marker、航行中市场拒绝与 D29 feedback；final runtime error console=`0`，Player clean close。
- 状态仍为 Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`；注入事件不是前台 Win32 物理键盘或人工视觉 PASS。

结果：[Q31 interaction matrix](Q31/Q31_CURRENT_PIPELINE_INTERACTION_MATRIX_Q26A22_20260913_RESULT.json)；日志：[Pipeline capture](Q31/Q31_CURRENT_PIPELINE_INTERACTION_MATRIX_Q26A22_20260913.log)；交接：[Q31](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-13 Q05 current-source swim contract after Q26-A22

- Q05 当前源定向 PlayMode `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot` 完成 `1/1 PASS`；测试 `11.7257264s`，Pipeline 总计 `11.96s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；十次 whole-visual-root 游泳倾斜/离水 neutral 恢复、D2 物理根 upright 均有当前源证据。Editor=`ready/stopped`；历史 timeout seq `28710` 之后 Error 查询为空。
- Q05 仍为 `PASS_WITHIN_SCOPE`，不代表 authored 四肢/头部动作、自然泳姿、前台 Win32 输入或人工视觉 PASS；Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS` 保持。下一步 Q06。

结果：[Q05 Q26-A22 R1](Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q05](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)。

## 2026-09-13 Q06 current-source jump/grounded contract after Q26-A22

- Q06 当前源定向 PlayMode `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite` 完成 `1/1 PASS`；测试 `14.3624642s`，Pipeline 总计 `14.6s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；十次真实跳跃—落地、出生点附近正/斜/横/反向小步移动的 grounded、非游泳和有限 Transform 契约均通过。Editor=`ready/stopped`；自 seq `30360` 起 Error 查询为空。
- Q06 仍为 `PASS_WITHIN_SCOPE`，不代表墙/坡/岸线/跳板/船甲板视觉接触、前台 Win32 输入或人工视觉 PASS；Q07 是下一张当前源码碰撞路线卡。

结果：[Q06 Q26-A22 R1](Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q06](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)。

## 2026-09-13 Q07 current Harbor collision owner/route after Q26-A22

- Q07 当前源重跑服务建筑 collision owner：`1/1 PASS`（测试 `2.7650275s`、总计 `2.96s`）；重跑 D17A10 terrain/dock/gangplank：`1/1 PASS`（测试 `5.0460624s`、总计 `5.29s`）。两项均无 failed/skipped/inconclusive。
- 本轮没有修改生产代码或配置；四栋服务建筑 D6-owned solid collision 与 D19 terrain/DockGround/GangplankGround 自动化双向路线均通过。Editor=`ready/stopped`；自 seq `30424` 起 Error 查询为空。
- Q07 仍为 `PASS_WITHIN_SCOPE`，不代表所有坡墙/岸线、视觉网格接触、船舷/上下船、前台 Win32 输入或人工视觉 PASS。Q08 仍因 Q03 `BLOCKED_ASSET` 阻塞，下一步先登记 Q08，再进入 Q09。

结果：[Q07 Q26-A22 R1](Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q07](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。

## 2026-09-13 Q08 formal-action prerequisite gate after Q26-A22

- Q03 admission gate `FormalActionAdmissionRemainsClosedForGenericOnlyController`=`1/1 PASS`，实际 marker=`formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`。
- Q05 当前 D17A08 与 Q07 当前 collision/route 子契约均已通过，但 Q08 整合测试本轮 `NOT_RUN`；neutral-only/generic diagnostic 不能冒充完整 input→state→motion→pose 证据。
- Q08 保持 `BLOCKED`（不是失败、不是 PASS）；Q03=`BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`。解除需要 current-rig authored ground-action 资源或可审计 Avatar/bone-axis adapter，并重跑同源 Q08。

结果：[Q08 Q26-A22](Q08/Q08_CURRENT_BLOCKED_20260913_Q26A22_RESULT.json)；交接：[Q08](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。下一步 Q09。

## 2026-09-13 Q09 current deck/boarding owner after Q26-A22

- Q09 当前源 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding` 完成 `1/1 PASS`；测试 `5.2753674s`，Pipeline 总计 `5.51s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；非单位船根锚点、十次登/下船生命周期和 30m 远距拒绝均通过。Editor=`ready/stopped`；自 seq `30582` 起 Error 查询为空。
- Q09 仍为 `PASS_WITHIN_SCOPE`，不代表自然握舵、航行中甲板关系、船体视觉、前台 Win32 输入或人工视觉 PASS。下一步 Q10。

结果：[Q09 Q26-A22 R1](Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q09](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)。

## 2026-09-13 Q10 current ship-motion/deck relation after Q26-A22

- Q10 当前源 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` 完成 `1/1 PASS`；测试 `3.7330901s`，Pipeline 总计 `3.98s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；十个船舶平移/转向快照中登船玩家相对船根的位置/旋转稳定，并可恢复正常下船。Editor=`ready/stopped`；自 seq `30742` 起 Error 查询为空。
- Q10 仍为 `PASS_WITHIN_SCOPE`，不代表开放海航行、前台 W/S/A/D、船舶碰撞、相机构图、自然握舵或人工视觉 PASS。下一步 Q11。

结果：[Q10 Q26-A22 R1](Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q10](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。

## 2026-09-13 Q11 current safe-disembark owner after Q26-A22

- Q11 当前源 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround` 完成 `1/1 PASS`；测试 `5.4638275s`，Pipeline 总计 `5.71s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；十次合法下船和 terrain bounds 外无落点拒绝均通过。Editor=`ready/stopped`；自 seq `30818` 起 Error 查询为空。
- Q11 仍为 `PASS_WITHIN_SCOPE`，不代表移动船舶视觉碰撞、所有岸边/跳板出口、前台 Win32 输入、正式动作、相机或人工视觉 PASS。下一步 Q12。

结果：[Q11 Q26-A22 R1](Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json)；交接：[Q11](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。

## 2026-09-13 Q12 current CameraFollow owner after Q26-A22

- Q12 当前源 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose` 完成 `1/1 PASS`；测试 `5.5607355s`，Pipeline 总计 `5.81s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；十次模式/目标切换、真实 SphereCast 外部遮挡收缩和移除后的阻尼恢复保持通过，`ResolvedDistance=10.478m`、`collisionChecks=109`，Editor=`ready/stopped`，自 seq `30989` 起 Error 查询为空。
- Q12 保持 `PASS_WITHIN_SCOPE`，不代表最终相机构图、渲染穿模、native Player 输入或人工视觉 PASS。下一步 Q13 当前输入焦点复核。

结果：[Q12 Q26-A22 R1](Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q12](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。

## 2026-09-13 Q13 current input/UI ownership after Q26-A22

- Q13 当前源 `D17A15GameplayKeyboardDoesNotNavigateHudAndPauseRestoresModalOwnership` 完成 `1/1 PASS`（测试 `7.9642195s`、Pipeline `8.22s`）；`D16InputRouterPlayModeTests` 完成 `11/11 PASS`（Pipeline `9.7s`），包含 W/S 不导航 HUD、模态 W/S 不泄漏、InitialHarbor 键盘/登船、Esc 暂停和重绑定路径。
- 本轮没有修改生产代码或配置；Editor=`ready/stopped`，自 seq `30989` 起 Error 查询为空，当前测试与输入/UI owner hash 已登记。
- Q13 保持 `PASS_WITHIN_SCOPE`，不代表前台 Win32 物理键盘、渲染 HUD、E/I/M/T 业务流程、原生 Player 或人工视觉 PASS。下一步 Q14 当前船岸整合复核。

结果：[Q13 Q26-A22 R1](Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q13](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。

## 2026-09-13 Q14 current shore/water/deck state-chain after Q26-A22

- Q14 当前源 `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence` 完成 `1/1 PASS`；测试 `5.7407475s`，Pipeline 总计 `6.01s`，failed/skipped/inconclusive=`0`。
- 本轮没有修改生产代码或配置；岸上→入水→回岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法下船链保持通过，Editor=`ready/stopped`，自 seq `31455` 起 Error 查询为空。
- Q14 保持 `PASS_WITHIN_SCOPE`；`imported_neutral=true` 仍受 Q03 formal action 门禁限制，不代表正式动作、native Player E/I/M/T、最终视觉或人工验收 PASS。下一步 Q15。

结果：[Q14 Q26-A22 R1](Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q14](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。

## 2026-09-13 Q15 current D29/D16 panel focus after Q26-A22

- Q15 当前源 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes` 完成 `1/1 PASS`；测试 `16.5811208s`，Pipeline 总计 `16.85s`，failed/skipped/inconclusive=`0`。
- Console marker `seq=31660` 记录 `I/M/T` shortcut owner；`seq=31749` 记录仓库/市场/酒馆各 10 次、exclusive、重复请求拒绝和 Gameplay 恢复全部为真。Editor=`ready/stopped`，自 seq `31535` 起 Error 查询为空。
- 本轮 warning（D27 presentation wait、D2 未验证 ph_crew_male/程序化 Player fallback）仍作为视觉/runtime 债务记录，不折算为 PASS。Q15 仍不代表 native Player 键鼠、渲染面板、业务 owner 真运行结果或 VC-G01。下一步 Q16。

结果：[Q15 Q26-A22 R1](Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q15](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。

## 2026-09-13 Q16 current D29/D3 trade bridge after Q26-A22

- Q16 当前源 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest` 完成 `1/1 PASS`；测试 `4.0648674s`，Pipeline 总计 `4.28s`，failed/skipped/inconclusive=`0`。
- marker `seq=31829` 记录未就绪拒绝、市场双击单 owner、买卖各一次状态变化、余额不足和满仓状态不变；Editor=`ready/stopped`，自 seq `31752` 起 Error 查询为空。
- Q16 保持 `PASS_WITHIN_SCOPE`，不代表原生 Player 鼠标命中、渲染面板、性能、完整首航、正式动作或 VC-G01。下一步 Q17。

结果：[Q16 Q26-A22 R1](Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q16](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。

## 2026-09-13 Q17 current preparation/shipyard owner after Q26-A22

- Q17 当前源 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException` 完成 `1/1 PASS`；测试 `5.5880272s`，Pipeline 总计 `5.81s`，failed/skipped/inconclusive=`0`。
- marker `seq=31923` 记录修船成功、材料拒绝、重复/同 revision 幂等、取消、船坞成功/拒绝、异常恢复和主输入恢复；Editor=`ready/stopped`，自 seq `31834` 起 Error 查询为空。
- Q17 保持 `PASS_WITHIN_SCOPE`；预期空 owner Warning 仍为反馈路径，不代表原生 Player、截图、最终视觉、性能、完整首航或 VC-G01。下一步 Q18。

结果：[Q17 Q26-A22 R1](Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q17](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。

## 2026-09-13 Q18 current HUD/layout code gate after Q26-A22

- Q18 当前源 `D31Q18HudResolutionTests` 为 `3/3 PASS`（1280×720、1920×1080、2560×1440）；`WhalesIslandReef.D31.VisualLogic.Tests` 为 `36/36 PASS`；仓库根 Python `tools.tests.test_d29_interaction_hud` 为 `30/30 OK`。
- 本轮没有修改生产代码或配置；Editor=`ready/stopped`，自 seq `31926` 起 Error 查询为空。首次 Python 子目录导入错误已记录为工作目录问题，不作为产品失败。
- Q18 仍为 `PARTIAL`，不代表原生 Player 截图、字体栅格化、英文最长文案、鼠标命中、前台键盘、性能或最终视觉 PASS。下一步 Q19。

结果：[Q18 Q26-A22 R1](Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q18](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。

## 2026-09-13 Q19 current offline crew/save owner after Q26-A22

- Q19 `Q19CrewInteractionAuditTests` 为 `1/1 PASS`；`Save.Tests` 全程序集为 `377/377 PASS`；`dotnet build AgentCore` 为 `0 warnings / 0 errors`。
- 本轮没有修改生产代码或配置；Editor=`ready/stopped`，自 seq `31938` 起 Error 查询为空，当前 AgentCore/CrewAgentCodec/Save 测试 hash 已登记。
- Q19 继续为 `VERIFIED`，只覆盖离线船员提议、owner 裁决、关系事实和 AgentMemory/domain SQLite 恢复；不代表在线 LLM、原生 Player 对话 UI/输入/视觉、动画、游泳、碰撞表现或正式视觉门禁。下一步 Q20。

结果：[Q19 Q26-A22 R1](Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q19](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。

## 2026-09-13 Q20 current vegetation containment after Q26-A22

- 候选资源 `D19R03Q20VegetationTests` 为 `1/1 PASS`；R03 接地/旧 Renderer 隔离 `Q26A03...` 为 `1/1 PASS`；marker=`activeLegacyTreeRenderers=0, activeR03Vegetation=24, unsupportedR03Vegetation=0`。
- 资源门禁仍为 `BLOCKED / 0 / 0`，`RenderAuthoredPalms=false`，正式 R03 Prefab 棕榈引用仍为 `0`；Editor=`ready/stopped`，自 seq `32012` 起 Error 查询为空。
- Q20 继续 `PARTIAL/BLOCKED_ASSET`，不代表正式棕榈恢复、全镜头接地/遮挡/穿模、native Player 视觉或 VC-G01。下一步 Q21。

结果：[Q20 Q26-A22 R1](Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q20](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-13 Q21 current harbor space audit after Q26-A22

- 当前源码 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` 为 `1/1 PASS`；四个服务建筑各 `37 Renderer / 0 missing`，marker=`4 buildings / 4 footprints / 6 required groups / 23 colliders / 1 terrain collider`，X/Z footprint 两两分离。
- Pipeline Editor=`ready/stopped`，自 seq `32090` 起 Error 查询为空；本轮没有产品代码或配置变更，当前 test/builder/Prefab/InitialHarbor hash 已重新登记。
- Q21 继续 `PARTIAL`：仅覆盖当前源码静态建筑空间、接地和碰撞归属，不代表原生 Player 镜头/遮挡/键鼠、植被资源门禁、性能或最终视觉，也不升级 VC-G01。

结果：[Q21 Q26-A22 R1](Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q21](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

## 2026-09-13 Q22 current sea/weather audit after Q26-A22

- 当前源码 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts` 为 `1/1 PASS`；marker=`water 96×96, sea_level=-0.0397, beach_min_y=-0.0048, foam=28, water_missing_materials=0, ship_min_y=0.0600, hull_bottom_y=-0.0400, presentation_audio=0, fish=1.00/0.90/0.55, weather=0/560/980`。
- 清空 Console 后以 seq `32100` 为基线，新增 Error=`0`、Warning=`0`；Pipeline Editor=`ready/stopped`，证据采集后 Console 已清空。本轮没有产品代码或配置变更，当前源码 hash 已登记。
- Q22 继续 `PARTIAL`：仅覆盖静态海平面/船吃水/天气 owner 合同，不代表原生 Player 游泳、船海镜头、泡沫/雾/音效、性能、物理键鼠或最终视觉，也不升级 VC-G01/D31。

结果：[Q22 Q26-A22 R1](Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q22](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。

## 2026-09-13 Q23 current formal scene integration audit after Q26-A22

- 当前源码 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently` 为 `1/1 PASS`；marker=`active_roots=34, r03_instances=1, ship_instances=1, r03_renderers=361, missing_materials=0, missing_scripts=0, terrain_colliders=1, ship_hull_bottom_y=-0.0400, reopen_consistency=true`。
- 清空 Console 后以 seq `32126` 为基线，新增 Error=`0`、Warning=`0`；Pipeline Editor=`ready/stopped`，证据采集后 Console 已清空。本轮没有产品代码或配置变更，当前 scene/Prefab/test hash 已登记。
- Q23 继续 `PARTIAL`：仅覆盖当前 InitialHarbor 正式 Prefab 唯一性、来源、脚本/材质完整性和重开一致性，不代表原生 Player 镜头/键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉，也不升级 VC-G01/D31。

结果：[Q23 Q26-A22 R1](Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q23](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。

## 2026-09-13 Q24 current save/restore diagnostic after Q26-A22

- `D19Q24SaveRestoreDiagnosticTests` 为 `3/3 PASS`：陆地 round-trip、出海船位/港口安全出生锚点、坏档事务保护均通过；出海 `(41.5,-17.25)`、航向 `212`、速度 `2.25`，好档保护 `money=812 / hull=47 / crew_morale=86`。
- 清空 Console 后以 seq `32138` 为基线，新增 Error=`0`、Warning=`0`；Pipeline Editor=`ready/stopped`，证据采集后 Console 已清空。本轮没有生产代码或配置变更，当前 Save/D2/D7/test hash 已登记。
- Q24 继续 `PARTIAL`：`PlayerState` 没有独立游泳姿态/体积/动画字段，不代表游泳存档、原生 Player 输入/视觉、动作、树木、音效、性能或最终视觉，也不升级 VC-G01/D31。

结果：[Q24 Q26-A22 R1](Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q24](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。

## 2026-09-13 Q25 current first-voyage and route audit after Q26-A22

- D30/D23 EditMode `2/2 PASS`（Pipeline `0.49s`）；InitialHarbor route PlayMode `2/2 PASS`（Pipeline `6.73s`），覆盖七阶段投影、40 tick 首航/抵达/返港/存档投影、交易关闭/保存与出航/推进/返港。
- 以 seq `32160` 为基线，新增 Error=`0`、Warning=`12`；Warning 全部属于已知 D27 readiness、D2 未验证角色/程序化回退和绝对 Euler 动画回退边界。Editor=`ready/stopped`，证据采集后 Console 已清空；本轮没有产品代码或配置变更。
- Q25 继续 `PARTIAL`：不代表完整人工游玩、原生 Player WASD/E/I/M/T/Esc、镜头、动作、游泳、上船、树木接地、音效、性能或最终视觉，也不升级 VC-G01/D31。

结果：[Q25 Q26-A22 R1](Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q25](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。

## 2026-09-13 Q26 current gameplay/input owner revalidation after Q26-A22

- 当前 `D19R03Q26GameplayDefectPlayModeTests` 为 `19/19 PASS`（Pipeline `48.33s`）；定向 A01 W 游泳 `1/1 PASS`，marker=`swimming=True, action=Swimming, planarDistance=0.3291`；定向 A04 E 路径 `1/1 PASS`，marker=`remote_e_rejected=true, real_e_boarded=true, real_e_disembarked=true`。
- 完整套件以 seq `32297` 为基线 Error=`0`，Warning 查询返回 `100` 条 API 窗口上限；A01/A04 各自 Error=`0`、Warning=`6`，均为已知 D27/D2 fallback/readiness。Editor=`ready/stopped`，证据采集后 Console 已清空；本轮没有产品代码或配置变更。
- Q26 继续 `PARTIAL`：这只证明当前源码 Input System/PlayMode owner 合同，不代表原生 Player 前台物理键盘、GPU 画面、正式 Avatar/动作、自然泳姿、目标设备性能或 VC-G01 视觉，也不升级 D31。

结果：[Q26 Q26-A22 R1](Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q26](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一步 Q27。

## 2026-09-13 Q27 current performance diagnostic after Q26-A22

- `D16PerformanceBudgetTests` EditMode=`6/6 PASS`（`0.02s`）；`D16A01RuntimePerformanceWindowSamplesContinuously` PlayMode=`1/1 PASS`（Pipeline `64.97s`，测试 `64.742428s`）。实际连续窗口 `60.011505s / 162 samples`，探针 owner=`D16WorldPerformanceController`，`PerformanceProbeActive=true`、`BudgetSatisfied=true`、`Passed=true`。
- 本轮 connected Pipeline Editor 为 `Null Device / GraphicsDeviceType=Null`，GPU timing 不可用；平均 `30.2886086 FPS`、平均帧 `158.3737030ms`、最长帧 `1120.6174316ms`、超过 `50ms` 的长帧 `84`、最小 FPS `0.8923652`。这是 headless 诊断风险，不是目标设备性能或全局流畅 PASS；既有 standalone R3 仍单独保留为 `58.734367 FPS / 21.460548 FPS 1% Low / 622.258118ms longest frame / GPU unavailable`。
- Console lower-bound seq `33585`：新增 Error=`0`、Warning=`6`（D27/D2 readiness/fallback）；Editor=`ready/stopped`，证据采集后 Console 已清空；本轮没有产品代码或配置变更，也没有基于 Null Device 指标做无证据优化。
- Q27 继续 `PARTIAL`：GPU/目标设备性能、平滑性、native Player 截图与 VC-G01 视觉门禁仍开放。下一步 Q28 新候选 Player 构建，再进入 Q29 自动化路线。

结果：[Q27 Q26-A22 R1](Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json)；交接：[Q27](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-13 Q31 connected Editor Pipeline capability boundary

- Connected Unity Pipeline Editor `127.0.0.1:7800` 可执行 `editor_play` / `editor_stop`、读取 `InitialHarbor` 层级并运行 `D16Accessibility=8/8`；最终 `Editor=ready/stopped`。本轮没有修改生产代码或运行时配置。
- `screenshot view=game` 虽返回成功路径，但停止态与 Play Mode 两张 `13556 bytes` PNG 完全相同，SHA256=`557E66D338223BD72B1ED5B680E747BB159FE626F832D752C1B9D8E9B3CFBE98`，均为无效灰帧；Console `seq=33714` 为 `RenderTexture.Create failed`。`view=scene` 因无活动 Scene view 未捕获。
- 结论是插件可用于 Editor 控制、层级/逻辑诊断和 hidden Player Pipeline 路线，但当前 Null Device Editor 截图不能作为视觉证据。Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。

结果：[Q31 Editor Pipeline capability](Q31/Q31_CURRENT_PIPELINE_EDITOR_CAPABILITY_20260913_R1_RESULT.json)；交接：[Q31](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

- 同一 Pipeline 只读 AssetDatabase 审计找到 `Assets/Animations/D20/Player/Clips` 的 15 个 generic D20 Player clip，但 `swim` 名称查询为 0；`Walk/Helm` 样例绑定使用 `Armature/root/...`。棕榈名称查询找到 12 个候选/占位 GameObject，均在 `Assets/Art/Candidates/...` 或 `Assets/Prefabs/Placeholder/...`，没有执行正式准入或场景提升。
- 因此 Q03 formal authored action/swim 与 Q20 formal vegetation gate 的真实阻塞是“适配/准入未满足”，不是可以直接把候选文件当正式资源；Q31、VC-G01 状态不变。
- `Assets/Resources/D20/Formal/D20_PlayerAnimation.controller` 的只读结构为 15 状态、无 `Swimming` 状态、0 transitions，继续证明 generic clip/controller 不能替代正式自然泳姿与动作状态机；没有写入 controller 或默认 Player。

## 2026-09-13 当前接管审计与 Q01 R3 状态板

- Q00/Q01：`VERIFIED_WITH_SCOPE`。当前默认 Player 已绑定 Q26-A22 `build_236bb62b66a6`，但证据仍限于 connected Pipeline、隐藏 Player/直接运行态与源绑定；无前台 Win32 物理键盘、compositor-visible screenshot 或人工视觉 PASS。
- Q03：`BLOCKED_ASSET`；Q08：`BLOCKED`；Q20/Q26/Q31：`PARTIAL`；VC-G01：`IN_PROGRESS`；VC-G02/VC-G03：`LOCKED`。D31-P04/D31-S05/D31-G06 的既有 PARTIAL 不升级。
- Q01 取样确认物理根为单位缩放、视觉子树为 0.66、默认仍为 procedural animation、R03 MeshCollider owner 与 HarborCollision 初始化可追溯；本轮没有修改生产代码、Prefab、场景或运行时配置。
- 下一张可执行卡为 `Q04_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`（水域 owner/进出水循环），之后按依赖推进 Q05/Q06/Q07/Q09；不继续无证据盲调视觉坐标。

结果：[Q00 Q26-A22 R3](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A22_20260913_LUNA_R3_RESULT.json)、[Q01 Q26-A22 R3](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 Q04 当前水域 owner 回归 R3

- Q04：`PASS_WITHIN_SCOPE`。当前源 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles` PlayMode 完成 `1/1 PASS`，测试耗时 `9.094856s`、Pipeline `9.3s`；Editor 最终 `ready/stopped`。
- 以 console seq `33835` 为下界，新增 Error=`0`；Warning=`6`，均为既有 D27/D2 readiness/fallback，不改变 Q03/Q20/Q26/Q31 或 VC-G01 边界。
- Q04 只证明 D2 水域 owner 的十次浅水/深水进出、移动和恢复回归；不证明原生 Player W/S、自然泳姿、前台焦点、水材质、截图或视觉验收。
- 下一张可执行卡为 `Q05_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q04 Q26-A22 R3](Q04/Q04_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q04](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q04.md)。

## 2026-09-13 Q05 当前游泳表现回归 R3

- Q05：`PASS_WITHIN_SCOPE`。当前源 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot` PlayMode 完成 `1/1 PASS`，测试耗时 `10.4941761s`、Pipeline `10.7s`；Editor 最终 `ready/stopped`。
- 该测试证明 visualRoot 游泳倾斜/退出恢复和物理根竖直分离；不证明正式 authored 动作/泳姿、自然人形游泳、原生 Player W/S、前台焦点、截图或 VC-G01。
- 以 seq `33835` 为下界累计 Error=`0`、Warning=`12`，均为 Q04/Q05 已知 D27/D2 readiness/fallback；没有生产代码或配置改动。
- 下一张可执行卡为 `Q06_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q05 Q26-A22 R3](Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q05](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q05.md)。

## 2026-09-13 Q06 当前移动/跳跃/接地回归 R3

- Q06：`PASS_WITHIN_SCOPE`。当前源 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite` PlayMode 完成 `1/1 PASS`，测试耗时 `11.1847479s`、Pipeline `11.4s`；Editor 最终 `ready/stopped`。
- 该测试证明十次跳跃/落地与正向、对角、反向小步移动保持有限、接地、非游泳；不证明墙/坡面/岸边/跳板/甲板、视觉接触、原生 Player 输入或 VC-G01。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。
- 下一张可执行卡为 `Q07_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q06 Q26-A22 R3](Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q06](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q06.md)。

## 2026-09-13 Q07 当前碰撞 owner 与岸线路线回归 R3

- Q07：`PASS_WITHIN_SCOPE`。四栋服务建筑 HarborCollision owner 测试 `1/1 PASS`（`1.9401197s`）；地形/码头/跳板双向步行测试 `1/1 PASS`（`2.997678s`）；Editor 最终 `ready/stopped`。
- 以 seq `33835` 为下界新增 Error=`0`；本轮没有生产代码或配置改动。
- Q07 只证明建筑碰撞 owner 与较小范围自动路线；不证明所有岸坡、视觉网格接触、船上/登船路线、原生 Player WASD、截图或 VC-G01。
- 下一步先登记 Q08 的 formal authored action/swim asset 阻塞，再按依赖进入 Q09。

结果：[Q07 Q26-A22 R3](Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q07](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07.md)。

## 2026-09-13 Q08 formal action admission blocker R3

- Q08：`BLOCKED`，不是失败也不是 PASS。当前 EditMode gate `FormalActionAdmissionRemainsClosedForGenericOnlyController` 本身 `1/1 PASS`，但 seq `34292` 明确记录 `formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`。
- 当前 Editor 最终 `ready/stopped`，seq `34289` 之后 Error=`0`；没有修改产品代码、Prefab、场景或运行时配置，也没有运行会过度宣称完成的 Q08 integration suite。
- 解除条件是当前 rig 可兼容 authored ground-action clip set，或可审计 Avatar/bone-axis adapter 及导入证据；然后重跑 Q08 action-state/movement/swim/grounding 集成。Q31 原生 Player/视觉仍独立。
- 下一步进入 `Q09_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q08 Q26-A22 R3](Q08/Q08_CURRENT_BLOCKED_20260913_Q26A22_LUNA_R3_RESULT.json)；交接：[Q08](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q08.md)。

## 2026-09-13 Q09 当前甲板锚点/登船生命周期回归 R3

- Q09：`PASS_WITHIN_SCOPE`。当前源 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding` PlayMode 完成 `1/1 PASS`，测试耗时 `3.2919116s`、Pipeline `3.52s`；Editor 最终 `ready/stopped`。
- 该测试证明缩放船体 gameplay helm anchor、十次登船/下船生命周期和远端拒绝；不证明原生 Player E、自然手舵接触、船上运动、UI 焦点、截图或 VC-G01。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。
- 下一张可执行卡为 `Q10_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q09 Q26-A22 R3](Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q09](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q09.md)。

## 2026-09-13 Q10 当前船上相对运动回归 R3

- Q10：`PASS_WITHIN_SCOPE`。当前源 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` PlayMode 完成 `1/1 PASS`，测试耗时 `2.2846007s`、Pipeline `2.56s`；Editor 最终 `ready/stopped`。
- 该测试证明船体 snapshot 位移/转向时 boarded player 相对甲板稳定并可恢复安全下船；不证明原生 Player WASD、完整航海、船碰撞、自然握舵、镜头/视觉或 VC-G01。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。
- 下一张可执行卡为 `Q11_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q10 Q26-A22 R3](Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q10](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q10.md)。

## 2026-09-13 Q11 当前安全下船回归 R3

- Q11：`PASS_WITHIN_SCOPE`。当前源 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround` PlayMode 完成 `1/1 PASS`，测试耗时 `3.6895698s`、Pipeline `3.93s`；Editor 最终 `ready/stopped`。
- 该测试证明十次合法下船恢复和无地面拒绝保留船上控制；不证明原生 Player 输入、移动船碰撞、连续视觉相机、formal action quality 或 VC-G01。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。
- 下一张可执行卡为 `Q12_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q11 Q26-A22 R3](Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q11](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q11.md)。

## 2026-09-13 Q12 当前相机模式/遮挡回归 R3

- Q12：`PASS_WITHIN_SCOPE`。当前源 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose` PlayMode 完成 `1/1 PASS`，测试耗时 `3.1757306s`、Pipeline `3.39s`；Editor 最终 `ready/stopped`。
- 该测试证明 CameraFollow 模式/目标切换、受控遮挡缩短与阻尼恢复；不证明最终场景真实墙/船栏/低物件视觉构图、原生 Player、截图或 VC-G01。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。
- 下一张可执行卡为 `Q13_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`（UI/键盘焦点合同）。

结果：[Q12 Q26-A22 R3](Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q12](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q12.md)。

## 2026-09-13 Q13 当前输入/UI ownership 回归 R3

- Q13：`PASS_WITHIN_SCOPE`。D17A15=`1/1 PASS`、D16InputRouter=`11/11 PASS`、D16PrimaryHudNavigation=`2/2 PASS`；覆盖 W/S 不导航主 HUD、模态 W/S 不泄漏、Esc/pause/focus 恢复及 UI move action 抑制重申。
- Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；本轮没有生产代码或配置改动。
- 该结果证明逻辑 ownership，不证明前台 Windows Player 焦点、真实物理键盘、渲染 HUD、E/I/M/T 业务流、截图或 VC-G01；用户反馈的实际窗口冲突仍需 Q31/VC-G01。
- 下一张可执行卡为 `Q14_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q13 Q26-A22 R3](Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q13](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13.md)。

## 2026-09-13 Q14 当前岸-水-船整合路线回归 R3

- Q14：`PASS_WITHIN_SCOPE`。当前源 `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence` PlayMode 完成 `1/1 PASS`，测试耗时 `2.2802981s`、Pipeline `2.5s`；Editor 最终 `ready/stopped`。
- 覆盖岸→水→岸、登船、ShipDeck target、船上 W+D 转向、无地面拒绝和合法下船；formal imported-rig animation quality 明确为 `BLOCKED_BY_Q03`。
- 以 seq `33835` 为下界新增 Error=`0`；没有生产代码或配置改动。Q03/Q08、原生 Player E/I/M/T、重复/拒绝业务路径、连续视觉和 VC-G01 仍开放。
- 下一张可执行卡为 `Q15_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q14 Q26-A22 R3](Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q14](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14.md)。

## 2026-09-13 Q15 当前 I/M/T 面板与焦点回归 R3

- Q15：`PASS_WITHIN_SCOPE`。当前源 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes` PlayMode 完成 `1/1 PASS`，测试耗时 `12.655612s`、Pipeline `12.86s`；Editor 最终 `ready/stopped`。
- Console marker 真实记录 `I/M/T` shortcut dispatch、仓库/市场/酒馆各 10 次、互斥、重复请求拒绝和 gameplay recovery；以 seq `35283` 为下界新增 Error=`0`。
- 该结果证明当前 D29/D16 逻辑 ownership，不证明原生 Player 键鼠焦点、渲染面板、业务 owner 真值、截图或 VC-G01；没有生产代码或配置改动。
- 下一张可执行卡为 `Q16_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q15 Q26-A22 R3](Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q15](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15.md)。

## 2026-09-13 Q16 当前交易桥接回归 R3

- Q16：`PASS_WITHIN_SCOPE`。当前源 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest` PlayMode 完成 `1/1 PASS`，测试耗时 `1.923169s`、Pipeline `2.13s`；Editor 最终 `ready/stopped`。
- marker 记录 owner 未就绪、双击单 owner、买卖各单次变化、金币不足和货舱不足不变；以 seq `35498` 为下界新增 Error=`0`。
- 该结果证明 D29/D3 逻辑桥接，不证明原生 Player 鼠标命中、渲染市场画面、完整航海、截图或 VC-G01；没有生产代码或配置改动。
- 下一张可执行卡为 `Q17_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q16 Q26-A22 R3](Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q16](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16.md)。

## 2026-09-13 Q17 当前整备/船坞桥接回归 Q26-A22 Luna R3

- Q17：`PASS_WITHIN_SCOPE`。当前源 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException` PlayMode 完成 `1/1 PASS`，测试耗时 `2.7098296s`、Pipeline `2.89s`；Editor 最终 `ready/stopped`。
- seq `35667` marker 记录整备成功、材料不足拒绝、重复/同 revision 幂等、取消恢复、船坞成功/拒绝、异常恢复和主输入恢复均为 `true`；seq `35580` 之后新增 Error=`0`。seq `35665` 空 owner 异常是测试故障注入的预期 Warning，状态不变与恢复已由测试断言。
- 本轮没有生产代码、Prefab、场景或运行时配置改动。该结果只证明 D29/D9/D4 owner 桥接逻辑，不证明原生 Player、前台物理键鼠、渲染面板、截图、性能、完整首航或 VC-G01。
- 下一张可执行卡为 `Q18_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q17 Q26-A22 Luna R3](Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q17](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。

## 2026-09-14 Q18 当前 HUD/视觉逻辑回归 Luna

- Q18 继续 `PARTIAL`。当前源 `D31Q18HudResolutionTests`=`3/3 PASS`（1280×720、1920×1080、2560×1440）；`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`；仓库根 `python -m unittest tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- Pipeline Editor 最终 `ready/stopped`，编译与域重载均为 false；Console 本轮共 `16` 条日志，`Error=0 / Warning=0`，随后已清空。本轮没有修改生产代码、Prefab、场景或运行时配置。
- 该结果只证明代码级 16:9 HUD 几何、D31 visual-stage 逻辑和 D29 静态回归，不证明原生 Player/GPU 截图、字体栅格化、英文最长文案、物理鼠标命中、前台键盘、性能或最终视觉；Q18 不升级为 PASS。
- 下一张可执行卡为 `Q19_CURRENT_PIPELINE_REAUDIT`。

结果：[Q18 20260914 Luna](Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q18 capture](Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q18](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。

## 2026-09-14 Q19 当前船员/存档回归 Luna

- Q19=`VERIFIED`。当前 Pipeline `Q19CrewInteractionAuditTests`=`1/1 PASS`；`Save.Tests` 全程序集=`377/377 PASS`，failed/skipped/inconclusive 均为 `0`；`dotnet build game/AgentCore/AgentCore.csproj --nologo`=`0 warnings / 0 errors`。
- Pipeline Console 共 `78` 条日志，`Error=0 / Warning=10`；warning 为已知 D2 动作/rig 准入提示，已保留；Editor 最终 `ready/stopped`，随后清空 Console。
- Q19 只覆盖离线船员提议、owner 裁决、D10 关系/行为事实和 AgentMemory/domain SQLite 恢复；不代表在线 LLM、原生 Player 对话 UI/输入/视觉、动画、游泳、碰撞表现、性能或正式视觉门禁通过。下一步 Q20。

结果：[Q19 20260914 Luna](Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q19 capture](Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q19](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。

## 2026-09-14 Q20 当前植被接地/遗留 Renderer 隔离回归 Luna

- Q20=`PARTIAL/BLOCKED_ASSET`。当前候选 `D19R03Q20VegetationTests`=`1/1 PASS`；R03 接地/legacy quarantine `Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence`=`1/1 PASS`，marker=`activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0 / vegetation_grounded=26 / vegetation_hidden_without_terrain=0`。
- 资源门禁仍为 `status=BLOCKED / local_development_ready_count=0 / eligible_entry_count=0`；builder 仍 `RenderAuthoredPalms=false`，正式 R03 Prefab 棕榈引用为 `0`。本轮没有晋升候选资源或修改生产代码。
- Console `Error=0 / Warning=6`，warning 为已知 D2/D27 表现初始化族，已登记；Editor `ready/stopped`，取证后清空。该证据不代表正式棕榈、全镜头接地/遮挡/穿模、原生 Player/GPU 视觉或 VC-G01 通过。下一步 Q21。

结果：[Q20 20260914 Luna](Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q20 capture](Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q20](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-14 Q21 当前港口空间静态回归 Luna

- Q21=`PARTIAL`。当前 Pipeline `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`；marker=`buildings=4 / footprints=4 / required_groups=6 / colliders=23 / terrain_colliders=1 / external_collider_owners=D27_R03_CharacterWildlifePresentation`，四建筑各 `37 Renderer / 0 missing`，bounds `min.y≈0.225`，XZ 无重叠。
- Console `Error=0 / Warning=0`，Editor `ready/stopped`，取证后清空；本轮无生产代码、Prefab、场景或运行时配置变更。
- Q21 只证明静态 R03 空间/owner 合同和稳定锚点，不代表原生 Player 镜头、遮挡、键鼠路线、鼠标命中、植被准入、性能或最终视觉通过。下一步 Q22。

结果：[Q21 20260914 Luna](Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q21 capture](Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q21](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

## 2026-09-14 Q22 当前船海天气静态回归 Luna

- Q22=`PARTIAL`。当前 Pipeline `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`；marker=`water 96×96 / sea_level -0.0397 / beach_min_y -0.0048 / foam=28 / water_missing=0 / ship_min_y 0.0600 / hull_bottom -0.0400 / presentation_audio=0 / fish=1.00/0.90/0.55 / precipitation=0/560/980`，测试断言的波动倍率为 `1.00/1.40/2.35`。
- Console `Error=0 / Warning=0`，Editor `ready/stopped`，取证后清空；本轮无生产代码、Prefab、场景或运行时配置变更。
- Q22 只证明静态水线/船体/天气 owner 合同，不代表真实 Player 游泳、船海镜头、泡沫/雾/音效、性能、物理键盘或最终视觉通过。下一步 Q23。

结果：[Q22 20260914 Luna](Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q22 capture](Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q22](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。

## 2026-09-14 Q23 当前正式场景集成回归 Luna

- Q23=`PARTIAL`。当前 Pipeline `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`；marker=`active_roots=34 / r03_instances=1 / ship_instances=1 / r03_renderers=361 / missing_materials=0 / missing_scripts=0 / terrain_colliders=1 / ship_hull_bottom_y=-0.0400`，港口/船/D27 GUID 解析有效且重开一致。
- Console `Error=0 / Warning=0`，Editor `ready/stopped`，取证后清空；本轮无生产代码、Prefab、场景或运行时配置变更。
- Q23 只证明 InitialHarbor 的 R03/D27/船 Prefab 挂接、唯一性、缺失项和重开稳定性，不代表原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过。下一步 Q24。

结果：[Q23 20260914 Luna](Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q23 capture](Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q23](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。

## 2026-09-14 Q24 当前存档恢复回归 Luna

- Q24=`PARTIAL`。当前 Pipeline `D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`：陆地 round-trip、出海船位/航向/速度与 D2_PlayerSpawnAnchor 安全恢复、以及失败保存不覆盖上一份好档均通过。
- 具体值为出海位置 `(41.5,-17.25)`、航向 `212`、速度 `2.25`；Console `Error=0 / Warning=0`，Editor `ready/stopped`，取证后清空。
- `PlayerState` 没有独立游泳姿态/体积/动画字段；Q24 不代表游泳存档、原生 Player WASD/E/I/M/T/Esc、镜头、动作、树木、音效或性能通过。下一步 Q25。

结果：[Q24 20260914 Luna](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q24 capture](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q24](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。

## 2026-09-13 Q18 当前 HUD/视觉逻辑回归 Q26-A22 Luna R3

- Q18：继续 `PARTIAL`。当前源 `D31Q18HudResolutionTests` 完成 `3/3 PASS`，覆盖 1280×720、1920×1080、2560×1440；`WhalesIslandReef.D31.VisualLogic.Tests` 完成 `36/36 PASS`；仓库根 `python -m unittest tools.tests.test_d29_interaction_hud` 完成 `30/30 OK`。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35670` 为下界新增 Error=`0`，随后清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 该结果只证明代码级 16:9 HUD 几何、D31 visual-stage 逻辑和 D29 静态回归，不证明原生 Player 截图、字体栅格化、英文最长文案、物理鼠标/前台键盘、性能或最终视觉；Q18 不升级为 PASS。
- 下一张可执行卡为 `Q19_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q18 Q26-A22 Luna R3](Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q18](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。

## 2026-09-13 Q19 当前船员/存档回归 Q26-A22 Luna R3

- Q19：`VERIFIED`（限离线船员提议、owner 裁决、D10 关系/行为事实和 AgentMemory/domain SQLite 保存恢复）。`Q19CrewInteractionAuditTests`=`1/1 PASS`；`Save.Tests`=`377/377 PASS`；`dotnet build game/AgentCore/AgentCore.csproj --nologo`=`0 warnings / 0 errors`。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35682` 为下界新增 Error=`0`，游标 `35756`，随后清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 在线 LLM、原生 Player 对话 UI/输入/视觉、动画、游泳、碰撞表现、性能和正式视觉门禁仍不在 Q19 通过范围。
- 下一张可执行卡为 `Q20_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q19 Q26-A22 Luna R3](Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q19](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。

## 2026-09-13 Q20 当前植被接地/遗留 Renderer 隔离回归 Q26-A22 Luna R3

- Q20：继续 `PARTIAL/BLOCKED_ASSET`。`D19R03Q20VegetationTests`=`1/1 PASS`；`Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence`=`1/1 PASS`；marker=`activeLegacyTreeRenderers=0, activeR03Vegetation=24, unsupportedR03Vegetation=0`，D26 表现层记录 `vegetation_grounded=26, vegetation_hidden_without_terrain=0`。
- 资源门禁仍未解除：ready catalog=`BLOCKED`、`local_development_ready_count=0`、`eligible_entry_count=0`；builder=`RenderAuthoredPalms=false`；正式 R03 prefab 棕榈引用=`0`。本轮没有资源晋升或正式棕榈恢复。
- Editor 最终 `ready/stopped`、编译与域重载均为 false；seq `35756` 之后捕获 1 条真实 FMOD 输出设备 Error（环境音频设备边界），已记录并清空 Console。该错误不被伪装成植被失败，也不被静默删除。
- 下一张可执行卡为 `Q21_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q20 Q26-A22 Luna R3](Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q20](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-13 Q21 当前港口空间回归 Q26-A22 Luna R3

- Q21：继续 `PARTIAL`。当前源 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` 完成 `1/1 PASS`，测试耗时 `0.2043205s`、Pipeline `0.21s`；四个服务建筑各 `37 Renderer / 0 missing`，`4` 个 footprint、`6` 个空间组、`23` 个碰撞体（R03 terrain=`1`、D27 owner=`22`）均通过，pairwise XZ 无重叠。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35835` 为下界新增 Error=`0`，游标 `35845`，随后清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 该结果只证明静态 R03 空间/owner 合同，不证明原生 Player 相机、遮挡、键盘路线、鼠标命中、植被准入、性能或最终视觉。
- 下一张可执行卡为 `Q22_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q21 Q26-A22 Luna R3](Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q21](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

## 2026-09-13 Q22 当前船海天气回归 Q26-A22 Luna R3

- Q22：继续 `PARTIAL`。当前源 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts` 完成 `1/1 PASS`，测试耗时 `0.1091705s`、Pipeline `0.12s`；marker 确认水面 `96×96`、sea level `-0.0397`、泡沫 `28`、船体 `min.y=0.0600`、船壳底部 `-0.0400`、R03 表现音频源 `0`、D9 鱼类活动 `1.00/0.90/0.55`、D28 降水 `0/560/980`。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35845` 为下界新增 Error=`0`，游标 `35857`，随后清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 该结果只证明静态水线/船体/天气 owner 合同，不证明原生 Player 海里游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉。
- 下一张可执行卡为 `Q23_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q22 Q26-A22 Luna R3](Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q22](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。

## 2026-09-13 Q23 当前正式场景集成回归 Q26-A22 Luna R3

- Q23：继续 `PARTIAL`。当前源 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently` 完成 `1/1 PASS`，测试耗时 `0.2767029s`、Pipeline `0.28s`；`active_roots=34`、R03/船实例各 `1`、活动 R03 Renderer=`361`、缺失材质/脚本=`0`、terrain collider=`1`、船体碰撞底部=`-0.0400`、重开一致性=`true`。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35857` 为下界新增 Error=`0`，游标 `35867`，随后清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 该结果只证明正式场景 Prefab 唯一性、GUID、缺失项和重开稳定性，不证明原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉。
- 下一张可执行卡为 `Q24_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q23 Q26-A22 Luna R3](Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q23](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。

## 2026-09-13 Q24 当前存档恢复回归 Q26-A22 Luna R3

- Q24：继续 `PARTIAL`。当前源 `D19Q24SaveRestoreDiagnosticTests` 完成 `3/3 PASS`，Pipeline `1.57s`：陆地 round-trip、出海船位/航向/速度与 `D2_PlayerSpawnAnchor` 安全恢复、`after_player` 故障后上一份好档保护均通过。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；以 seq `35867` 为下界新增 Error=`0`，游标 `35878`，随后清空 Console。本轮没有生产代码、Prefab、场景或存档 schema 改动。
- `PlayerState` 仍没有独立游泳状态/姿态字段；该结果不证明游泳存档、原生 Player WASD/E/I/M/T/Esc、镜头、动画、船控、树木、音效、性能或最终视觉。
- 下一张可执行卡为 `Q25_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`。

结果：[Q24 Q26-A22 Luna R3](Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)；交接：[Q24](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。

## 2026-09-14 Q25 当前首航/循环整合回归 Q26-A22 Luna R3

- Q25：继续 `PARTIAL`。当前源 `D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.24s`），`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `2.84s`）；覆盖 D30 七阶段投影、D23 40 tick 首航/抵达/返港/保存，以及 InitialHarbor 交易关闭/保存、出航/推进/返港。
- 以 Console seq `35878` 为下界新增 Error=`0`；返回窗口 `100` 条、游标 `36021`、`dropped=false`，真实 Warning=`12`：D27 启动等待、D2 未验证皮肤/程序化低模回退、generated D6CrewAnimator 绝对 Euler 曲线保留 procedural/neutral layer。D31 runtime performance approval 仍 pending，D7 音频为 `fallback=True`。
- Editor 最终 `ready/stopped`，编译与域重载均为 false；证据采集后已清空 Console。本轮没有生产代码、Prefab、场景或运行时配置改动。
- 该结果只证明当前 D30/D23 owner 链和有限 InitialHarbor 路线，不证明原生 Player 键鼠、动作/游泳/上船视觉、镜头、树木接地、音效播放、性能或最终视觉；Q25 不升级为 PASS。

结果：[Q25 Q26-A22 Luna R3](Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_RESULT.json)；Console marker：[Q25 marker](Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_CONSOLE_MARKER.log)；交接：[Q25](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一张可执行卡为 `Q26_CURRENT_PIPELINE_REAUDIT`。
## 2026-09-14 Q26 当前游泳/登船/植被回归与程序化脚部相位修复 Q26-A22 Luna R3

- Q26 首轮全量 PlayMode 真实发现 `18/19 PASS、1 FAIL`：`Q26A21ProceduralSwimPoseHasMeasuredContinuousStroke` 的 72 帧 `maxFootDelta=0.993856549` 未达到 `>1.0` 合同。按 D2 表现层 owner 将程序化游泳脚部摆幅从 `3f` 调整为命名常量 `4f`。
- 修复后定向 A21=`1/1 PASS`（`maxArmDelta=3.64`、`maxFootDelta=1.09`），Q26 全量=`19/19 PASS`（Pipeline `25.53s`）；A01 W 游泳=`1/1 PASS`（`planarDistance=0.4358`），A04 E 拒绝远船/真实登船/下船=`1/1 PASS`。
- 以 seq `36021` 为全量下界新增 Error=`0`；Warning 返回 `100` 且 `dropped=true`，如实保留 API 窗口边界。A01/A04 各自 Error=`0`、Warning=`6`。Editor 最终 `ready/stopped`，证据采集后已清空 Console。本轮有一个明确的 D2 表现层代码修复，未改 Systems、玩法状态、导入蒙皮或 AgentCore。
- 该结果只关闭程序化 fallback 脚部相位的可测量回归，不证明原生 Player 前台物理键盘、GPU/截图、正式 Avatar/动作、自然泳姿、植被/穿模最终视觉、目标设备性能或 VC-G01；Q26 保持 `PARTIAL`。

结果：[Q26 Q26-A22 Luna R3](Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_RESULT.json)；Console marker：[Q26 marker](Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_CONSOLE_MARKER.log)；交接：[Q26](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一张可执行卡为 `Q27_CURRENT_PIPELINE_REAUDIT`。
## 2026-09-14 Q27 当前 D16 性能预算/运行窗口回归 Q26-A22 Luna R3

- Q27：继续 `PARTIAL`。`D16PerformanceBudgetTests`=`6/6 PASS`（Pipeline `0.02s`）；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`（Pipeline `61.59s`，测试 `61.388231s`），实际窗口 `60.0062256s / 233 samples`。
- 探针 owner 与预算合同成立：`PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`。Connected Editor 为 `Null Device`，GPU timing 不可用；平均 `54.39256 FPS`、平均帧 `28.26567ms`、最长帧 `608.78851ms`、`81` 帧超过 `50ms`，属于诊断风险而非目标设备性能 PASS。
- 以 seq `38454` 为下界新增 Error=`0`、Warning=`6`，游标 `38524`、返回 `70`、`dropped=false`；Editor 最终 `ready/stopped`，证据采集后清空 Console。本轮没有新增生产代码或配置改动。
- 该结果只确认 D16 budget/探针在当前源码下可运行，并不关闭 GPU、目标设备性能、长帧风险、原生 Player 输入/视觉或 VC-G01；Q27 不升级为 PASS。

结果：[Q27 Q26-A22 Luna R3](Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_RESULT.json)；Console marker：[Q27 marker](Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_CONSOLE_MARKER.log)；交接：[Q27](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。下一张可执行卡为 Q28 新候选 Player 构建。

## 2026-09-14 Q28 当前源码 Q26-A22 Luna R3 候选 Player 与 D17 泳姿根守卫

- Q28：继续 `PARTIAL`。先做 `recompile`，结果 `up_to_date`；随后构建 `Q28_CANDIDATE_PLAYER_R9.exe`，`build_11f9905f3b10` 为 `StandaloneWindows64 / Succeeded`，显式场景 `MainMenu.unity + InitialHarbor.unity`，`0 errors / 1 warning`，构建数据 `175194470` bytes，耗时 `2868ms`。EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，内置 Assembly SHA256=`FF699A91A9EE7F9E5B78F984C38B6442371EB82B06F8ABA6E12CA304A91738DB`。
- 唯一构建 warning 为 `RuntimePipelineConfig enableInBuilds=false`，所以 R9 不含 Player Runtime Pipeline；没有把 Editor Pipeline 能力扩大为 Player 控制面。
- `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`=`1/1 PASS`，Pipeline=`8.44s`、测试=`8.1967475s`；`rootAngle=64.00°`、`rootOffsetY=0.011..0.034`、`maxArmMotion=159.51`、`maxForearmMotion=36.00`、`maxLegMotion=7.00`。该守卫只证明当前程序化视觉根边界和物理根 upright，不证明 authored animation 或最终视觉。
- Console seq `38524..38655`：返回 `100`、`dropped=false`、Error=`0`、Warning=`16`；Editor 最终 `ready/stopped`，证据采集后已清空 Console。本轮没有新增生产代码/config 修改。
- R8 因 BuildReport 的未编译代码 warning 保留但不作为权威候选；R9 是 recompile 后的权威候选。Q28 仍不关闭 Player 前台物理输入、截图、GPU/目标设备性能、视觉质量或 VC-G01。

结果：[Q28 R9](Q28/Q28_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_RESULT.json)；Console marker：[Q28 marker](Q28/Q28_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_LUNA_R3_CONSOLE_MARKER.log)；交接：[Q28](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。下一张可执行卡为 Q29：使用 R9 做隔离真实输入/路线证据；若无批准的隔离控制面，则记录具体阻塞而不升级状态。

## 2026-09-14 Q31 当前源码 Pipeline 交互闭环 R4

- 使用 `Q31_PIPELINE_CURRENT_CANDIDATE_R4.exe`、`build_824c76e4cc26` 和临时 `Development + AllowDebugging`，通过 Unity Pipeline hidden Player 完成 MainMenu→InitialHarbor、W/S 行走与游泳、E 上下船、远船 E 拒绝、I 仓库模态 W 隔离、M 市场成功/重复/Esc、T 酒馆/Esc、Esc 暂停恢复。`console --level error=0 entries`，Player clean close。
- 结果：交互逻辑为 `PASS_WITHIN_SCOPE`，不是前台物理键盘或视觉 PASS。四张 `1280x720` 有效 RenderTexture 的 hash、运行状态和边界记录在 [Q31 R4 RESULT](Q31/Q31_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_R4_RESULT.json)，命令时序在 [R4 capture log](Q31/Q31_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_R4_PIPELINE_CAPTURE.log)。
- 运行时树探针显示旧树根 `957` 个 Renderer 启用数 `0`、五个 `ph_palm_tree` 启用 Renderer 数 `0`；本轮没有复现可见 legacy 树，但不关闭人工“树在空中飘”否决或 Q20 正式植被门。
- QA 结束后已恢复 `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false`，Editor=`ready/stopped`。已知 D2 低模/未验证蒙皮、D27 等待、D31 性能待批准和 D7 audio fallback Warning 如实保留。
- 状态保持：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`。下一步选择一个有明确 owner 的 presentation 缺陷；不提前进入 VC-G02/VC-G03。

## 2026-09-14 Q31 connected Editor Pipeline runtime probe

- 使用 `com.unity.pipeline@0.6.0-exp.1` 在现有 connected Unity Editor 中启动并停止 PlayMode；一次性 `UnityPipelinePoseAndInputProbe.cs` 通过 `run_script` 编译执行，未修改生产场景、Prefab 或运行时配置。
- 初始状态真实返回 `Character/Idle`、程序化低模 `renderer_count=17 / skinned_renderer_count=0`、8 个绑定骨骼；EventSystem UI 导航关闭、move action 未绑定、选择为空。
- 真实 `D2ControlRouter.TryToggleShip()` 上船成功：parent=`ph_ship_fishing`、`control_mode=Ship`、玩家位于 `HelmPoint`，左右手到 `HelmWheel` 分别约 `0.194m`；再次切换成功下船。QA-only 开放水域定位后，正常运行时返回 `Character/Swimming`、`IsSwimming=true`、grounded=`false`、程序化视觉根 pitch=`64°`。
- `capture_game_view` 受 `Null Device/GraphicsDeviceType=Null` 限制不可用；异步 PlayMode 测试没有拿到终态，未计为 PASS。结果：[Q31 Editor runtime probe](Q31/Q31_PIPELINE_EDITOR_RUNTIME_PROBE_20260914_RESULT.json)；日志：[probe log](Q31/Q31_PIPELINE_EDITOR_RUNTIME_PROBE_20260914.log)。
- 状态不变：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`。该卡只确认插件的运行诊断能力，不关闭正式动作、自然泳姿、接触/IK、植被接地、前台物理输入、可见截图或目标设备性能。

## 2026-09-14 Q00 当前源与 Q31 R4 Player 重绑定

- Q00 当前工作区审计：`5390` status entries，tracked changed=`459`，untracked=`4931`，`459 files changed, 40339 insertions(+), 49178 deletions(-)`；未执行 reset、checkout、clean、stash 或大范围覆盖。
- 使用同一 connected Unity Pipeline Editor（Unity `6000.0.80f1`、PID `53332`、`127.0.0.1:7800`、`com.unity.pipeline@0.6.0-exp.1`）重新进入并退出 PlayMode。4 秒早期探针因 bootstrap 尚未创建 Player，明确记为 `NOT_READY`；追加等待 8 秒后的有效快照为 `InitialHarbor / Character / Idle / Procedural / grounded=true / swimming=false / bound_bones=8 / renderer=17 / skinned=0`，UI navigation=false、move action=null、selection为空。最终 Editor=`ready/stopped`。
- 当前人物/游泳源码 hash 已与最近证据重绑：D2 animation=`097D8210...A66A42`，D2 runtime=`0A0A17E0...980029`，D17 action test=`DAFFDD72...4BDC20`，Q26-A21 swim continuity test=`0BAA85F1...476E38D`。Q26-A21 是测试契约，不是 formal action 资源准入。
- 当前默认 Player 为 Q31 R4：`build_824c76e4cc26`，EXE SHA=`98751D0D...EDBCB42E81`，Assembly-CSharp SHA=`6EBD285D...1DB822C`，`StandaloneWindows64 / 0 errors / 7 warnings`。所有相关生产源时间早于 R4 构建，后续只新增了位于 `game/AgentScripts`、不进入 Player 的 ephemeral 诊断 helper；因此本卡没有发现 Player 与当前生产源错绑。
- Q00 状态：`VERIFIED_WITH_SCOPE`。这只闭合当前源/测试/Player 关系，不升级 `Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。结果：[Q00 20260914](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A22_20260914_RESULT.json)；日志：[Q00 capture](Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A22_20260914.log)。下一卡为 Q01 当前根因复核。

## 2026-09-14 Q01 当前根因/尺度/owner Pipeline Q26-A22 Luna Q01

- Q01 继续 `VERIFIED_WITH_SCOPE`。connected Pipeline Editor 当前运行态重新测得：初始 D2 root parent 为空、lossyScale=`(1,1,1)`、`Idle/Procedural`、grounded=`true`、swimming=`false`、renderer=`17`、skinned=`0`、bound bones=`8`；上船后 parent=`ph_ship_fishing`、mode=`Ship`，玩家/HelmPoint 约 `(0,1.10,-6.68)`，左右手到 HelmWheel 约 `0.194m`；开放水域诊断返回 `Character/Swimming`、swimming=`true`、grounded=`false`、pitch=`64°`。
- 直接 owner/tolerance 字段：CharacterController=`radius 0.2424353212m / height 1.7999997139m / center.y 0.8468632698m / skinWidth 0.0193948261m`；HumanoidVisual localScale=`0.6642063856`；船 scale=`2.5739560127`；logical water=`-0.0199999996`、halfExtent=`600`；deck collision ready；HarborCollision=`initialized/R03 installed/colliders 9/walkable 3/building 4`。
- 本轮只改动了 `AgentScripts` 临时探针的 JSON 展平字段，没有改生产代码、场景、Prefab、Systems 或 Player。Pipeline 编译 diagnostics=`[]`，Editor 最终 `ready/stopped`。
- 该结果不证明 formal Avatar、自然泳姿、前台物理键盘、握舵/甲板无穿模、树木接地、可见截图或目标设备性能；Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`、Q26=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 均保持。下一步 `Q02_CURRENT_SOURCE_OWNER_LIFECYCLE_REAUDIT`。

结果：[Q01 20260914](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260914_LUNA_Q01_RESULT.json)；日志：[Q01 capture](Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260914_LUNA_Q01.log)；交接：[Q01](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q01.md)。

## 2026-09-14 Q02 当前源码 owner/lifecycle 回归 Q26-A22 Luna

- Q02=`PASS_WITHIN_SCOPE`。插件实际执行 `D2CharacterAnimationControllerTests` EditMode=`8/8 PASS`（0 failed/skipped/inconclusive，0.56s）和 `D17ActionQualityPlayModeTests` PlayMode=`16/16 PASS`（0 failed/skipped/inconclusive，81.03s）。
- 覆盖 D2 visualRoot 单一写者、十次 enable/disable 生命周期、未验证蒙皮 quarantine、水域十循环、游泳根分离、跳跃/地形/跳板、十次登船、船动/下船、相机恢复、W/S 不导航 HUD、模态 Esc/pause 及岸水船整合。
- 当前源码 hash、测试状态、Editor final=`ready/stopped` 已写入 Q02 result；从已记录 cursor `40526` 查询的新增 Error=`0`，证据后已清空 Console。历史 FMOD/headless capture/旧 test-runner timeout 不归入本轮。
- Q02 只关闭当前源码 owner/lifecycle 回归；不关闭 Q03 formal asset、自然泳姿、原生前台键盘、握舵/甲板不穿模、植被接地、截图、目标设备性能或 VC-G01。下一步先保持 Q03=`BLOCKED_ASSET`，再进入 Q04 水域 owner 复核。

结果：[Q02 20260914](Q02/Q02_CURRENT_SOURCE_OWNER_LIFECYCLE_20260914_LUNA_RESULT.json)；日志：[Q02 capture](Q02/Q02_CURRENT_SOURCE_OWNER_LIFECYCLE_20260914_LUNA.log)；交接：[Q02](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q02/Q02.md)。

## 2026-09-14 Q03 formal action/avatar 资产门复核 Q26-A22 Luna

- Q03 继续 `BLOCKED_ASSET`。`Q03FormalActionAdmissionGateTests`=`1/1 PASS_AS_BLOCKED_GATE`，marker=`clips=15; humanMotion=0; genericRoot=15`；`Q03FormalAvatarActionResourceContractTests`=`2/2 PASS_WITHIN_SCOPE`。
- 解释边界：资源接口/Avatar 合同存在不等于动作质量准入；当前 15 个 formal controller clip 全部 generic-only，门禁保持关闭，默认 InitialHarbor consumer=`0`、`Procedural/FormalAnimatorReady=false`。
- 本轮无生产代码、资源、场景、Prefab、默认 consumer 或 Player 变更；Editor 最终 `ready/stopped`。Q03 不升级 VC-G01、VC-G02/VC-G03、D31，也不替代原生 Player 视觉/输入证据。
- 下一步转到不依赖 formal action 资源的 `Q04_CURRENT_WATER_OWNER_REAUDIT`；解除 Q03 仍需合法 current-rig authored action set 或可审计 Avatar/骨骼轴适配器，并完成独立视觉门。

结果：[Q03 20260914](Q03/Q03_CURRENT_FORMAL_ACTION_GATE_20260914_LUNA_RESULT.json)；日志：[Q03 capture](Q03/Q03_CURRENT_FORMAL_ACTION_GATE_20260914_LUNA.log)；交接：[Q03](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q03/Q03.md)。

## 2026-09-14 Q04 当前水域 owner 回归 Q26-A22 Luna

- Q04=`PASS_WITHIN_SCOPE`。当前源码 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles` 通过 Pipeline 实际完成 `1/1 PASS`（测试 `10.378946s`、Pipeline `10.81s`，0 failed/skipped/inconclusive）。
- 覆盖十次浅水岸缘进入/退出、十次深水进入/移动/退出、OceanAdapter 水面查询、非零位移、水面稳定和 grounded recovery；日志观察到 `surfaceY=-0.02`、游泳 `playerY=0.42`、恢复地面 `surfaceY=0.09`。没有注入 IsSwimming、替换水面或关闭碰撞。
- Editor 最终 `ready/stopped`，本轮 Q04 时间段未观察到 Error；保留历史 console buffer 与已知 fallback/readiness warning，不将其抹平。无生产代码/场景/Prefab/config 变更。
- 该结果不证明原生 Player W/S/焦点、自然或正式泳姿、水材质画面、截图、目标设备性能或 VC-G01。下一步 `Q05_CURRENT_SWIMMING_PRESENTATION_REAUDIT`。

结果：[Q04 20260914](Q04/Q04_CURRENT_WATER_OWNER_20260914_LUNA_RESULT.json)；日志：[Q04 capture](Q04/Q04_CURRENT_WATER_OWNER_20260914_LUNA.log)；交接：[Q04](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md)。

## 2026-09-14 Q05 当前游泳表现契约回归 Q26-A22 Luna

- Q05=`PASS_WITHIN_SCOPE`。`D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot` 通过 `1/1`（测试 `13.0625438s`、Pipeline `13.49s`，0 failed/skipped/inconclusive）。
- 十次循环实际日志记录 visualRoot `rootAngle=64°`、rootOffsetY=`0.013..0.034m` 样本，D2 physics root 保持 upright，离水后 neutral restore 通过；没有旋转 CharacterController。
- 本轮没有生产代码/场景/Prefab/config 变更；Editor 最终 `ready/stopped`。该结果仍不证明 authored 四肢/头部泳姿、自然泳姿、前台 W/S、截图或目标设备性能，Q03/Q31/VC-G01 不升级。下一步 `Q06_CURRENT_MOVEMENT_COLLISION_REAUDIT`。

结果：[Q05 20260914](Q05/Q05_CURRENT_SWIMMING_PRESENTATION_20260914_LUNA_RESULT.json)；日志：[Q05 capture](Q05/Q05_CURRENT_SWIMMING_PRESENTATION_20260914_LUNA.log)；交接：[Q05](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)。

## 2026-09-14 Q06 当前移动/接地回归 Q26-A22 Luna

- Q06=`PASS_WITHIN_SCOPE`。`D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite` 通过 `1/1`（测试 `14.033055s`、Pipeline `14.5s`，0 failed/skipped/inconclusive）。
- 覆盖十次真实 TryJump→落地，以及出生点附近正向/斜向/横向/反向小步移动；grounded、non-swimming、finite Transform 均成立。运行时胶囊 worldHeight=`1.80m`、worldRadius=`0.24m`、worldStepOffset=`0.35m`，D19 R03 collision owner 保持。
- 无生产代码/场景/Prefab/config 变更；Editor 最终 `ready/stopped`。该结果不关闭墙/坡/岸线/跳板/船甲板视觉接触、native Player 键盘或 VC-G01。下一步 `Q07_CURRENT_COLLISION_ROUTE_REAUDIT`。

结果：[Q06 20260914](Q06/Q06_CURRENT_MOVEMENT_COLLISION_20260914_LUNA_RESULT.json)；日志：[Q06 capture](Q06/Q06_CURRENT_MOVEMENT_COLLISION_20260914_LUNA.log)；交接：[Q06](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)。

## 2026-09-14 Q07 当前碰撞路线回归 Q26-A22 Luna

- Q07=`PASS_WITHIN_SCOPE`。`R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries`=`1/1 PASS`（2.9602606s）与 `D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps`=`1/1 PASS`（5.3727658s）均通过当前 connected Unity Pipeline，失败/跳过/不确定均为 `0`。
- 运行时确认 D6_HarborCollision owner、4 个服务建筑 collider、DockGround=`0.50`、GangplankGround=`0.88`，非可行走碰撞攀爬被阻止且高度恢复；无生产代码/场景/Prefab/config 变更，Editor 最终 `ready/stopped`。
- 该结果只关闭当前服务建筑/码头/跳板的逻辑碰撞子契约，不证明所有岸线/斜坡/墙体、视觉接触、船甲板、native Player 输入、树木接地、无穿模或 VC-G01。Q08 仍受 Q03 formal action 资产门阻塞；下一张独立可执行卡为 Q09。

结果：[Q07 20260914](Q07/Q07_CURRENT_COLLISION_ROUTE_20260914_LUNA_RESULT.json)；日志：[Q07 capture](Q07/Q07_CURRENT_COLLISION_ROUTE_20260914_LUNA.log)；交接：[Q07](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。

## 2026-09-14 Q08 当前依赖阻塞复核 Q26-A22 Luna

- Q08=`BLOCKED`。当前 Q03 门禁真实结果为 `Q03FormalActionAdmissionGateTests=1/1 PASS_AS_BLOCKED_GATE`，marker=`formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`；默认 InitialHarbor formal consumer=`0`、FormalAnimatorReady=`false`。
- Q05 当前 D17A08=`1/1 PASS`、Q07 当前服务建筑 owner/D17A10=`1/1 + 1/1 PASS` 仅是限定子契约，因此 Q08 action-state/movement/swim/grounding 整合本轮 `NOT_RUN`。没有产品代码/配置变更，也没有用 neutral-only 或 generic diagnostic 伪造 PASS。
- Q08 是阻塞而非失败或通过；解除条件为当前 rig 可审计的 authored ground-action 资源或骨骼轴适配器，并以同源测试重跑整合套件。该阻塞不阻止 Q09，也不替代 Q31 native Player/视觉门禁。

结果：[Q08 20260914](Q08/Q08_CURRENT_BLOCKED_20260914_LUNA_RESULT.json)；交接：[Q08](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。下一张独立卡为 Q09。

## 2026-09-14 Q09 当前甲板锚点/登船回归 Q26-A22 Luna

- Q09=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`=`1/1 PASS`，测试耗时 `5.5399619s`、Pipeline `6.0s`，失败/跳过/不确定均为 `0`。
- 运行日志确认十次真实上下船、`maxDeckHeightDelta=0.0000`、`maxDeckAnchorDistance=0.0000`、船根缩放 `(2.57,2.57,2.57)`；远距拒绝路径保持 Character/null parent。执行后控制台筛查 `Error=0 / Warning=0`，Editor `ready/stopped`，取证后已清空。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭自然握舵接触、航行中船动关系、安全下船边界、船体视觉清除、native Player 输入或 VC-G01。下一步 Q10。

结果：[Q09 20260914](Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q09 capture](Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q09](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)。

## 2026-09-14 Q10 当前航行中甲板关系回归 Q26-A22 Luna

- Q10=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`=`1/1 PASS`，测试耗时 `3.9655668s`、Pipeline `4.45s`，失败/跳过/不确定均为 `0`。
- 10 个 D2 航行快照记录最大船位移 `1.498m`、最大航向变化 `32.000°`，玩家相对甲板位置/旋转误差均为 `0.000000`，并恢复正常下船。执行后 `Error=0`，另有 6 条已知 D2/D27 fallback/初始化 warning，已登记而未抹平；Editor `ready/stopped`，取证后已清空。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭开放海航行、native Player W/S/A/D、船舶碰撞、镜头构图、自然握舵、正式动作资源或 VC-G01。下一步 Q11。

结果：[Q10 20260914](Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q10 capture](Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q10](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。

## 2026-09-14 Q11 当前安全下船回归 Q26-A22 Luna

- Q11=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`=`1/1 PASS`，测试耗时 `5.4956044s`、Pipeline `5.98s`，失败/跳过/不确定均为 `0`。
- 日志确认十次合法下船、terrain bounds 外无落点拒绝，最终 `grounded=True / Character`；执行后 `Error=0`，另有 1 条预期的 `disembark_unavailable` Warning，已登记；Editor `ready/stopped`，取证后已清空。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭移动船舶视觉碰撞、所有岸边/跳板出口、native Player 输入、正式动作质量、相机质量或 VC-G01。下一步 Q12。

结果：[Q11 20260914](Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q11 capture](Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q11](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。

## 2026-09-14 Q12 当前相机遮挡/恢复回归 Q26-A22 Luna

- Q12=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`=`1/1 PASS`，测试耗时 `5.9748085s`、Pipeline `6.47s`，失败/跳过/不确定均为 `0`。
- 日志确认十次模式循环，受控障碍使距离从 `5.500m` 收缩到 `3.746m`，移除后恢复 `5.500m`，`collisionChecks=109`。执行后 `Error=0`，另有 6 条已知 D2/D27 fallback/初始化 warning，已登记；Editor `ready/stopped`，取证后已清空。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭最终场景相机构图、渲染穿模、native Player 激活或 VC-G01。下一步 Q13。

结果：[Q12 20260914](Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q12 capture](Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q12](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。

## 2026-09-14 Q13 当前输入/UI 隔离回归 Q26-A22 Luna

- Q13=`PASS_WITHIN_SCOPE`。D17A15=`1/1 PASS`（5.8881073s/6.35s），D16InputRouter=`11/11 PASS`（Pipeline 10.23s），D16PrimaryHudNavigation=`2/2 PASS`（Pipeline 6.88s），失败/跳过/不确定均为 `0`。
- D17A15 记录十轮 W/S 后 `normal_navigation=false`、暂停/阻塞/恢复导航状态正确；D16 覆盖 InitialHarbor 键盘/登船、Esc、重绑定、合成键盘/手柄；Primary HUD 覆盖外部恢复 Move action 后重新断言隔离。三组 `Error=0`，warning `6/7/12` 为已知 D2/D27 fallback/初始化项并已登记。
- 无产品代码/场景/Prefab/config 变更。该结果证明 Unity Input System/UI owner 逻辑，不证明前台 Win32 键盘、渲染 HUD、E/I/M/T、正式动作资源、原生 Player 视觉或 VC-G01。下一步 Q14。

结果：[Q13 20260914](Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q13 capture](Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q13](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。

## 2026-09-14 Q14 当前船岸整合回归 Q26-A22 Luna

- Q14=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence`=`1/1 PASS`，测试耗时 `4.2337608s`、Pipeline `4.73s`，失败/跳过/不确定均为 `0`。
- 日志确认 `shore_water_shore=true`、`board_owner=Ship`、`ship_input_turn=true`、`no_ground_rejection=true`、`legal_disembark=true`。当前 marker `imported_neutral=False` 已核对为程序化 Player 分支诊断，不是正式动作资源通过；Q03 继续 `BLOCKED_ASSET`。
- 执行后 `Error=0 / Warning=7`（含预期无落点拒绝和已知 D2/D27 fallback/初始化 warning），已登记；Editor `ready/stopped`，取证后已清空。无产品代码/场景/Prefab/config 变更。
- 该结果不关闭正式动作、前台 Win32 键盘、E/I/M/T 业务、连续截图、最终视觉或 VC-G01。下一步 Q15。

结果：[Q14 20260914](Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q14 capture](Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q14](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。

## 2026-09-14 Q15 当前 I/M/T 面板焦点回归 Q26-A22 Luna

- Q15=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`=`1/1 PASS`，测试耗时 `18.118572s`、Pipeline `18.63s`，失败/跳过/不确定均为 `0`，Console `Error=0 / Warning=0`。
- marker 确认 `shortcut_input=true`、`keys=I,M,T`、`legacy_keydown_reached_owner=true`；仓库/市场/酒馆各 10 次循环，互斥、重复请求拒绝和 Gameplay 恢复均为真。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭前台 Win32 键鼠、渲染面板、Player 中业务 owner 真实结果、正式动作资源、性能或 VC-G01。下一步 Q16。

结果：[Q15 20260914](Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q15 capture](Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q15](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。

## 2026-09-14 Q16 当前交易桥回归 Q26-A22 Luna

- Q16=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`=`1/1 PASS`，测试耗时 `3.7348475s`、Pipeline `4.11s`，失败/跳过/不确定均为 `0`。
- marker 确认 owner 未就绪、快速双击单 owner、买卖一次变化、余额不足/满仓不变均为真；执行后 `Error=0`，另有 6 条已知 D2/D27 fallback/初始化 warning，已登记；Editor `ready/stopped`，取证后已清空。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭原生 Player 鼠标、渲染面板、性能、完整航程、正式动作资源或 VC-G01。下一步 Q17。

结果：[Q16 20260914](Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q16 capture](Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q16](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。

## 2026-09-14 Q17 当前船坞/整备桥回归 Q26-A22 Luna

- Q17=`PASS_WITHIN_SCOPE`。当前 Pipeline PlayMode `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`=`1/1 PASS`，测试耗时 `4.9706462s`、Pipeline `5.38s`，失败/跳过/不确定均为 `0`。
- marker 确认修船成功/材料不足/重复与同 revision 幂等、取消、船坞成功/资金不足、异常恢复和主输入恢复全部为真；`Error=0`，另有 7 条 warning，其中空 owner 异常是预期故障注入反馈路径，已登记。
- 无产品代码/场景/Prefab/config 变更。该结果不关闭原生 Player 键鼠、截图、最终视觉、性能、完整航程、正式动作或 VC-G01。下一步 Q18。

结果：[Q17 20260914](Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q17 capture](Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q17](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。

## 2026-09-14 Q25 当前首航/路线回归 Luna

- Q25 继续 `PARTIAL`。当前 Pipeline `D19Q25FirstVoyageLoopDiagnosticTests`=`2/2 PASS`（`0.51s`），`D19R02A03RoutePlayModeTests`=`2/2 PASS`（`7.08s`）；覆盖 D30 七阶段投影、D23 40 tick 首航/返港/存档投影，以及 InitialHarbor 交易关闭/保存、出航/推进/返港。
- 实际 marker 为 `preparation>market>tavern>shipyard>boarded>departed>returned`、`d23_ticks=40`、`arrived=true`、`returned=true`、`initial_harbor_route=2/2`。Console 总量 `129`、API 返回窗口 `100`，返回窗口 Error=`0`、Warning=`11`；不将 capped window 写成总 Warning=0。测试后 Editor `ready/stopped`，编译与域重载均为 false，Console 已清空。
- 本轮无生产代码、Prefab、场景或运行时配置改动。Q25 只证明 D30/D23 owner 路线和有限 InitialHarbor 路线回归，不证明完整人工游玩、原生 Player 输入、镜头、动作/游泳/上船视觉、树木接地、音效、性能或最终视觉。下一张可执行卡为 `Q26_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q25 20260914 Luna](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q25 capture](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q25](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。

## 2026-09-14 Q26 当前 PlayMode/输入回归 Luna

- Q26 继续 `PARTIAL`。清 Console 后当前 Pipeline `D19R03Q26GameplayDefectPlayModeTests`=`19/19 PASS`（`50.57s`）；定向 A01 W 游泳=`1/1 PASS`（`4.77s`），定向 A04 E 拒绝/登船/下船=`1/1 PASS`（`4.26s`）。
- A01 marker=`keyboard=W / swimming=True / planarDistance=0.1725 / surfaceY=-0.020`；A04 marker=`remote_e_rejected=true / real_e_boarded=true / real_e_disembarked=true / final_mode=Character`。全量 marker 还确认 `Q26A21 maxArmDelta=6.64 / maxFootDelta=1.99`、旧树 `0`、R03 植被 `24`、`vegetation_grounded=26`。
- 全量 Console 取证返回 Error=`0`、Warning=`102`、Log=`898`；Warning 为已知 D2/D27 fallback/readiness 家族，不降格隐藏。Editor `ready/stopped`，取证后 Console 已清空。本轮无生产代码、Prefab、场景或运行时配置改动。
- 结果只证明当前源码的有界 gameplay/owner/Input System 回归，不证明前台 Win32 物理键盘、Player GPU/截图、正式 Avatar/动作、自然泳姿、树木/穿模视觉、音效、目标设备性能或最终 VC-G01。下一张可执行卡为 `Q27_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q26 20260914 Luna](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q26 capture](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q26](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-14 Q27 当前性能窗口回归 Luna

- Q27 继续 `PARTIAL`。`D16PerformanceBudgetTests`=`6/6 PASS`（`0.03s`）；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`（Pipeline `63.63s`，测试 `63.2218578s`），窗口 `60.0172844s / 162 samples`。
- 当前 Editor 为 `Null Device / GraphicsDeviceType=Null`；平均 `28.6217 FPS`、最小 `0.8574 FPS`、平均帧 `165.513ms`、最长帧 `1166.361ms`、长帧 `85`、GPU unavailable。`PerformanceProbeActive/OwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`、`Passed=true`。
- Console 取证 Error=`0`、Warning=`6`、Log=`58`；保留 D2/D27 fallback/readiness warning、D7 audio `fallback=True`、D31 performance pending。Editor `ready/stopped`，取证后 Console 已清空。本轮无生产代码、Prefab、场景或运行时配置改动。
- 这些是 connected headless 诊断，不是目标设备性能或流畅度 PASS；下一张可执行卡为 `Q28` 新候选 Player build_id。
- 结果：[Q27 20260914 Luna](Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q27 capture](Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；原始报告：[D16 window](../../../tools/results/d16_a01_performance_window_report.json)；交接：[Q27](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-14 Q00/Q31 当前源码绑定与 W/S HUD 焦点隔离 R1

- Q00 source rebind=`VERIFIED_WITH_SCOPE`。当前 D2 runtime SHA256=`3D854D172925703EE556FC0FD2AF7E384B19B5AEE32246178625C1A9A32F2B4A`，Q26-A22 test SHA256=`B604A367A1DE24D3C02251B8E3FB4D3E707CB5C099842A0274F1870BDE9F7F72`；R1 build/Player 与 Q29 R9 EXE/Assembly-CSharp hash 一致，修正了旧 Q00 R4 绑定。
- Q31 child scope=`PASS_WITHIN_SCOPE`，父状态仍 `PARTIAL`。隐藏当前源码 Player 经 MainMenu 进入 InitialHarbor，W/S 改变角色位置，`navigation=false / move=<null> / selected=<none>` 保持；runtime error entries=`0`、clean close、Editor=`ready/stopped`、Runtime Pipeline=`enableInBuilds=false / port=0`。
- 新增 `D29Q15GameplayWasdDoesNotNavigatePrimaryHud=1/1 PASS`；既有 D29 panel recovery=`1/1 PASS`；D16 input router=`1/1 PASS`。本轮只加入测试守卫，没有改生产逻辑。
- VC-G01 仍 `IN_PROGRESS`。没有前台物理键盘、compositor-visible 截图、正式 Avatar/action、自然泳姿/握舵、植被/穿模、目标设备性能或人工视觉 PASS；Q03/Q20/D31 依赖门保持原状态。
- 结果：[Q00 rebind](Q00/Q00_CURRENT_SOURCE_REBIND_Q29R9_Q26A22_20260914_R1_RESULT.json)、[Q31 UI focus R1](Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1_RESULT.json)；日志：[Q31 R1](Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1.log)；交接：[Q31](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q28 当前候选 R10 构建与 D17 守卫 Luna

- Q28 继续 `PARTIAL`。dry-run=`valid=true / validationErrors=[]`；`build_a76b35e7f109` 返回 `Succeeded`，R10 总构建数据 `175194470` bytes、耗时 `270070ms`，但 BuildReport 明确 `totalErrors=1`（Pipeline `/api/exec` 主线程超时 `30000ms`）和 `totalWarnings=1`（`enableInBuilds=false`），不登记为零错误构建。
- R10 EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；`Assembly-CSharp.dll` SHA256=`FF699A91A9EE7F9E5B78F984C38B6442371EB82B06F8ABA6E12CA304A91738DB`；非开发构建设置与 Pipeline disabled 边界已核验。
- 当前 D17 A08=`1/1 PASS`（Pipeline `13.92s`），rootAngle=`64.00°`、rootOffsetY=`0.014..0.033`；D17 后 Console Error=`0`、Warning=`6`、Log=`104`，Editor `ready/stopped`，取证后清空。本轮无生产代码、Prefab、场景或运行时配置改动。
- R10 只具备带 BuildReport error 边界的候选身份和 D17 视觉根守卫，不证明 Player 前台窗口、物理键盘、截图、GPU、目标设备性能、正式动作或 VC-G01。下一张可执行卡为 `Q29_CURRENT_PLAYER_ROUTE_REAUDIT`。
- 结果：[Q28 20260914 Luna](Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q28 capture](Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q28](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。

## 2026-09-14 Q30 当前源码独立审查 Luna

- Q30 继续 `PARTIAL`。先发现并修复一个真实的静态测试契约回归：`test_d19_r03_visual_builder.py` 仍断言旧近黑色舵轮，而当前 D19-R03 owner 已使用 Q26-A18 收口的暖木 `0.34/0.16/0.055`；原始结果为 `64/65`，同步断言后定向 Python 回归恢复 `65/65 PASS`。无 Unity 生产源、Prefab、场景或运行时配置变更。
- Connected Pipeline 当前结果：D16Accessibility=`8/8`、D16InputRouter=`11/11`、D17ActionQuality=`16/16`、VcG01WorldLogic=`14/14`、Q07 建筑碰撞=`1/1`、Q15/Q16/Q17 各=`1/1`。PlayMode 走 async 路径；同步 HTTP 拒绝是 Pipeline 的 domain-reload 传输边界，不作为测试 verdict。
- Console Error=`0`；Warning 返回 `141` 条、`dropped=false`，为已知 D2/D27 fallback/readiness/animation 警告和 Q17 预期异常路径反馈，已登记并清 Console；Editor=`ready/stopped`。
- Q29 R7 当前源码隐藏 Player 路线作为依赖证据仍为 W、I 模态 W 阻断、M/T 重复和 Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A、Esc 暂停恢复；runtime error=`0`、Player 严格错误词=`0`、clean close。
- 本卡不关闭 compositor-visible 前台 Player 画面、Win32 物理键鼠、正式 authored action/avatar、自然泳姿/握舵、正式植被准入、全镜头接地/穿模或目标设备性能。`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一张可执行卡为 Q31 最终 native Player 视觉门禁。

结果：[Q30 当前审查 Luna](Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q30 capture](Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q30](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-14 Q31 R4 RenderTexture 直接复核

- 已直接复核当前源码 R4 四张有效 `1280x720` hidden Player RenderTexture：初始港口、静态游泳、移动游泳、登船舵轮；文件存在且 hash 与 R4 结果一致。
- 初始港口仍是低模/程序化场景，建筑/道路/岸线/地表层级粗糙；游泳帧有水平 fallback、伸展四肢和水面阴影，但不证明 authored swim cycle/自然接触；登船帧暖棕舵轮比旧近黑版本清楚，但不证明自然 IK/上船动作。
- 固定镜头未复现 visible legacy tree/palm renderer；只作为当前资源隔离证据，不等于正式植被准入或全镜头接地 PASS。
- Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。offscreen RenderTexture 不是 compositor-visible 前台窗口或人工最终视觉验收。

结果：[Q31 R4](Q31/Q31_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_R4_RESULT.json)；[Q31 canonical](Q31/RESULT.json)；交接：[Q31](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q20-A04 candidate palm Player validation Luna

- Q20-A04=`PASS_WITHIN_SCOPE` only. `tree_palm.fbx` is integrated as `KenneyNaturePalmCandidate` with `1` active Renderer and `0` Collider; D18 remains `Candidate/ImportVerified`, not LocalDevelopmentReady.
- EditMode candidate guard=`1/1 PASS`; PlayMode `D19R03Q20VegetationPlayModeTests`=`2/2 PASS`, failed/skipped/inconclusive=`0`. New guard measures owned `D19_TerrainRoot` contact and preserves presentation-only collision ownership.
- Build `build_9d09e6c266c9`=`Succeeded / 0 errors / 0 warnings`; hidden GUI WindowsPlayer PID `33724`, Pipeline `7903`, explicit InitialHarbor scene load, candidate `boundsMinY=1.0939`, scoped terrain `y=1.0699`, error=`0.0240m`, runtime error console=`0`, strict Player error tokens=`0`, clean close.
- Three valid `1280x720` Player RenderTexture diagnostics are registered under `Q20-A04`; the isolated candidate frame is readable, while the scene-composition frame is partly occluded by dock geometry. They do not close compositor-visible/manual visual acceptance.
- Temporary build/runtime settings were restored and read back as `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false / connectWithProfiler=false`. Q20 remains `PARTIAL/BLOCKED_ASSET`; Q31 remains `PARTIAL`; VC-G01 remains `IN_PROGRESS`; VC-G02/VC-G03 remain `LOCKED`.
- Result: `Q20-A04/RESULT.json`; capture: `Q20-A04/Q20_A04_VALIDATION_CAPTURE_20260914.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md`. Next task: `Q21`.

## 2026-09-14 Q21 current-source revalidation after Q20-A04

- Q20-A04 regenerated the shared D19-R03 Prefab, so the previous Q21 hash-bound evidence was not reused. The current static test `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` passed `1/1` (test `0.369373s`, Pipeline `0.69s`).
- Current markers remain `4 buildings / 4 footprints / 6 required groups / 23 colliders / 1 R03 terrain collider / 22 D27 owner colliders`; each service building has `37 Renderer / 0 missing`, pairwise XZ overlap=`0`, stable anchors retained.
- Console baseline `48188` to `48201` produced new Error=`0`, Warning=`0`; Editor ended `ready/stopped`. Current builder/Prefab/InitialHarbor hashes are recorded in the R2 result.
- Q21 remains `PARTIAL`: this is static space/owner evidence only; native Player camera/occlusion, mouse hit testing, physical keyboard, vegetation readiness, performance and final visual acceptance remain open. Next task: `Q22`.
- Result: `Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json`; log: `Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md`.

## 2026-09-14 Q22/Q23 current-source revalidation after Q20-A04

- Q20-A04 changed the shared D19-R03 Harbor Presentation Prefab, so the older Q22/Q23 result files were stale on Prefab hash and Renderer count. Re-ran both current-source EditMode contracts through the connected Unity Pipeline: Q22 `1/1 PASS` (`0.1779873s`, Pipeline `0.19s`) and Q23 `1/1 PASS` (`0.3673445s`, Pipeline `0.38s`).
- Q22 current marker seq `48240`: water `96×96`, sea `-0.0397`, beach min.y `-0.0048`, foam `28`, missing water materials `0`, visible ship min.y `0.0600`, hull bottom `-0.0400`, fish `1.00/0.90/0.55`, precipitation `0/560/980`, wave multiplier `1.00/1.40/2.35`.
- Q23 current marker seq `48255`: active roots `34`, R03/ship instances `1/1`, R03 renderers `362`, missing materials/scripts `0/0`, terrain colliders `1`, hull bottom `-0.0400`, GUIDs valid, reopen consistency `true`.
- Console seq `48201→48263` produced no new Error or Warning; Editor ended `ready/stopped`. Current dirty-worktree hashes and markers are in the R2 result/log artifacts. Q22 and Q23 remain `PARTIAL`; neither proves native Player camera/input, animation, swimming, ship control, audio, performance, vegetation all-camera grounding or final visual acceptance. Next task: `Q24_CURRENT_PIPELINE_REAUDIT`.
- Result/log/handoff: `Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json`, `Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json`, and the corresponding Q22/Q23 handoffs.

## 2026-09-14 Q24 current-source revalidation after Q20-A04

- Re-ran `D19Q24SaveRestoreDiagnosticTests` through the connected Unity Pipeline: `3/3 PASS`, total Pipeline duration `1.33s`; failed-save protection, land round-trip and open-sea round-trip all passed.
- Current evidence preserves land port/clock/money/cargo/crew, open-sea ship position `(41.5,-17.25)`/heading `212`/speed `2.25`, D2 harbor-safe player resume and the last good snapshot after an injected `after_player` failure.
- Console seq `48263→48280` produced Error=`0`, Warning=`0`; Editor ended `ready/stopped`. Q24 remains `PARTIAL`: `PlayerState` has no independent swimming state/pose/animation persistence, and native Player input, animation, swimming, boarding-control, tree grounding, audio, performance and final visual remain open.
- Result/log/handoff: `Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json`, `Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md`. Next task: `Q25_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q25 current-source revalidation after Q20-A04

- EditMode `D19Q25FirstVoyageLoopDiagnosticTests`=`2/2 PASS` (Pipeline `0.50s`); current markers confirm D23 `40` tick first voyage/arrival/return/save projection and D30 `preparation>market>tavern>shipyard>boarded>departed>returned` with failure/duplicate stability.
- PlayMode `D19R02A03RoutePlayModeTests`=`2/2 PASS` (Pipeline `8.03s`); covers bounded InitialHarbor trade-close/save, departure, advance and return.
- Console seq `48280→48435` returned `155` entries, Error=`0`, Warning=`12`: D27 readiness waits `6`, unverified `ph_crew_male` `2`, procedural player fallback `2`, absolute-Euler animation `2`. `D31 runtime performance approval remains pending`; D7 audio reports `fallback=True`.
- Q25 remains `PARTIAL`; current owner-route regression is proven only within this bounded scope. Native Player input/camera/action/swimming/boarding visuals, tree grounding, audio, target-device performance and final visual acceptance remain open. Result/log/handoff: Q25 Luna R2 artifacts and `docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md`. Next task: `Q26_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q26 current-source revalidation after Q20-A04

- Current `D19R03Q26GameplayDefectPlayModeTests`=`19/19 PASS`, Pipeline `53.18s`, failed/skipped/inconclusive=`0`.
- Markers: A03 legacy tree `0`/R03 vegetation `24`/unsupported `0`; A04 remote E rejected and real board/disembark pass; A06 swim displacement `0.6119`; A07 foam `28`/legacy `0`; A17 hand vertical error `0.0044`; A21 procedural swim `pitch=64.0`, `maxArmDelta=7.09`, `maxFootDelta=2.83`, formal asset unverified.
- Console seq `48435→49572`: Error=`0`, Warning=`114`, dropped=`false` (D27 readiness `57`, D2 procedural fallback `19`, unverified `ph_crew_male` `19`, absolute-Euler animation `19`). D31 runtime performance approval remains pending; D7 audio fallback is true.
- Q26 remains `PARTIAL`: current code-level owner/Input System and candidate vegetation contracts pass, but foreground physical keyboard, authored Avatar/action, natural swimming/boarding presentation, tree/intersection visual acceptance, audio, target-device performance and VC-G01 final visual remain open. Result/log/handoff: Q26 Luna R2 artifacts and `docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md`. Next task: `Q27_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q27 current-source revalidation after Q20-A04

- `D16PerformanceBudgetTests`=`6/6 PASS` (Pipeline `0.02s`); `D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS` (Pipeline `63.42s`, test `62.9555894s`).
- Current Null Device report: `60.0137634s`, `163` samples, average `28.147253 FPS`, min `0.815553 FPS`, average frame `164.826813ms`, longest `1226.161255ms`, `85` frames over `50ms`, main thread `18.020201ms`, GPU unavailable. Probe owner and D16 budget contract are true.
- Console seq `49572→49642`: Error=`0`, Warning=`6`; logs retain D7 audio fallback and D31 runtime performance pending. Q27 remains `PARTIAL`; Null Device numbers are diagnostic risk only, not target-device/GPU/smoothness PASS. Result/log/handoff: Q27 Luna R2 artifacts and `docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md`. Next task: `Q28_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q26-A23 current procedural swim hand-silhouette revalidation

- D2 presentation candidate: keep `ProceduralSwimmingUpperArmReachDegrees=150f`; change only `ProceduralSwimmingForearmReachDegrees=30f→60f`. The earlier 175f/170f upper-arm candidates were rejected by D17/A08 (`maxArmMotion=179.306046/179.473953`, expected `<175f`) and were rolled back.
- Current connected Pipeline PlayMode: Q26/A23=`1/1 PASS`, D17/A08=`1/1 PASS`, Q26/A21=`1/1 PASS`; full `D19R03Q26GameplayDefectPlayModeTests=21/21 PASS`, failed/skipped/inconclusive=`0`.
- Current-source QA Player `build_b0549b8e9093`/`buildGuid=2ff52d6d4ff044cbba815af31c5a4d1b` succeeded with `0 errors/7 warnings`; InitialHarbor runtime `Swimming/Swimming/Procedural`, 24 hand-head samples max=`0.135m` against `0.220m`; W simulation moved the player and left swimming active; runtime error entries=`0`; clean close and descriptor removal confirmed.
- Temporary Runtime Pipeline/Development settings were restored and read back as `enableInBuilds=false/port=0/developmentBuild=false/allowDebugging=false/connectWithProfiler=false`; Editor=`ready/stopped`. Q26 remains `PARTIAL`, Q05 remains within its existing scope, Q03=`BLOCKED_ASSET`, Q31=`PARTIAL`, VC-G01=`IN_PROGRESS`, VC-G02/VC-G03=`LOCKED`.
- Result/log/handoff: `Q26/Q26_A23_SWIM_FOREARM_PIPELINE_20260914_LUNA_RESULT.json`, `Q26/Q26_A23_SWIM_FOREARM_PIPELINE_20260914_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md`. Next executable card remains a separate dependency-scoped quality task; formal animation and final visual gates are not bypassed.

## 2026-09-14 Q20-A05 candidate grounding via exact R03 MeshCollider

- Fixed the two D19-R03 candidate placement paths so `AddCandidate` and `AddCandidatePreserveMaterials` use `GroundHeight()`: the build-time `D19_TerrainRoot` MeshCollider is sampled first, with analytical `Height()` only as fallback. This is a minimal presentation-owner change; gameplay systems, navigation and formal resource state were not changed.
- Rebuilt the R03 Harbor Presentation through Unity Pipeline menu `Whalebone/D19 R03/Build Harbor Presentation`. The first 30-second client request timed out at the tooling layer; the Editor stayed responsive and the retry with a 120-second timeout returned success. Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`.
- Focused evidence: Q20 candidate grounding=`1/1 PASS`, Q26/A12 presentation props=`1/1 PASS`, Q26/A14 shoreline rocks=`1/1 PASS`; full `D19R03Q26GameplayDefectPlayModeTests=21/21 PASS`, failed/skipped/inconclusive=`0`. Editor=`ready/stopped`, runtime Pipeline=`enableInBuilds=false/port=0`.
- Q20 remains `PARTIAL/BLOCKED_ASSET`; Q26/Q31 remain `PARTIAL`; VC-G01 remains `IN_PROGRESS`; VC-G02/VC-G03 remain `LOCKED`; D31-P04/S05/G06 remain `PARTIAL`. This card does not prove formal palm admission, all-camera no-clipping, compositor-visible Player visual, foreground physical input or target-device performance. A retained tool-layer timeout `seq=57632` is recorded in the result rather than hidden.

Result: `game/tools/results/LOCAL-QUALITY/Q20/Q20_A05_CANDIDATE_GROUND_COLLIDER_REAUDIT_20260914_LUNA_RESULT.json`; log: `game/tools/results/LOCAL-QUALITY/Q20/Q20_A05_CANDIDATE_GROUND_COLLIDER_REAUDIT_20260914_LUNA.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md`. Next: Q21 current-source spatial revalidation after the rebuilt R03 Prefab.

## 2026-09-14 Q21 current-source spatial revalidation after Q20-A05

- Because Q20-A05 rebuilt the shared R03 Prefab, Q21 was rerun against the new hash: `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`, test `0.472424s`, Pipeline `0.49s`.
- Current marker: `4` buildings, `4` footprints, `6` required groups, `23` colliders, `1` R03 terrain collider, external owner `D27_R03_CharacterWildlifePresentation`, pairwise XZ overlap `0`; Market/Warehouse/Tavern/Shipyard each `37 Renderer / 0 missing`. Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`.
- Editor=`ready/stopped`; Q21 marker seq=`59091..59097`; no new Q21 Error in the returned Console tail. Retained timeout seq=`57632` is the prior Q20-A05 tooling boundary, not a Q21 test failure.
- Q21 remains `PARTIAL`: this is static space/owner evidence only and does not close Player camera/occlusion, mouse/foreground physical keyboard route, vegetation readiness, performance or final visual acceptance. Next: Q22 current-source revalidation after the rebuilt R03 Prefab.

Result: `game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`; log: `game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md`.

## 2026-09-14 Q22 current-source sea/ship/weather revalidation after Q20-A05

- Q20-A05 rebuilt the shared R03 Prefab, so Q22 was rerun against current hashes: `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`, test `0.1744739s`, Pipeline `0.18s`.
- Marker seq=`59106`: water `96×96`, sea `-0.0397`, beach min.y `-0.0048`, foam `28`, water missing materials `0`, ship min.y `0.0600`, hull bottom `-0.0400`, presentation audio `0`, fish `1.00/0.90/0.55`, precipitation `0/560/980`, wave multiplier `1.00/1.40/2.35`; Calm/Rain/Storm markers seq=`59102..59105`.
- Harbor Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`; Ship Prefab SHA256=`CCCCA72A4C359DA4A45CCC62F114740A92800C1D65A38B07549BA34F3591B1FC`; Editor=`ready/stopped`; Console tail=`40`, dropped=false, no error-level entry. Duplicate test collector messages are informational and recorded.
- Q22 remains `PARTIAL`: static sea/ship/weather owner evidence only; Player swimming/camera/audio, physical input, performance and final visual remain open. Next: Q23 current-source revalidation after the rebuilt R03 Prefab.

Result: `game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`; log: `game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md`.

## 2026-09-14 Q23 current-source formal scene integration after Q20-A05

- Re-ran `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently` against the rebuilt R03 Prefab: `1/1 PASS`, test `0.7007015s`, Pipeline `0.71s`.
- Marker seq=`59115`: active roots `34`, R03/ship instances `1/1`, active R03 renderers `362`, missing materials/scripts `0/0`, terrain colliders `1`, ship hull bottom `-0.0400`, Harbor/Ship/D27 GUIDs resolved, `reopen_consistency=true`. Current Renderer `362` is recorded as new evidence; old `361/369` counts remain historical.
- Current Harbor Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`; Ship Prefab SHA256=`CCCCA72A4C359DA4A45CCC62F114740A92800C1D65A38B07549BA34F3591B1FC`; Editor=`ready/stopped`; no error-level entry in returned Console tail.
- Q23 remains `PARTIAL`: formal integration/reopen only; native Player camera/route/mouse/physical keyboard, action/swimming/ship control, audio, performance, per-object grounding and final visual remain open. Next: Q24 current-source save/restore diagnostic.

Result: `game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`; log: `game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md`.

## 2026-09-14 Q24 current-source save/restore revalidation after Q20-A05

- `Save.Tests.D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`, Pipeline `1.47s`; failed-save protection, land round-trip, and open-sea position/heading/speed plus `D2_PlayerSpawnAnchor` recovery all passed.
- Current Q24 source hashes and result are bound to the post-Q20-A05 workspace. Editor=`ready/stopped`, compiling/domain reload=`false`, returned Console tail Error=`0`; duplicate collector messages and retained historical Q20 tool timeout remain separated from this run.
- Q24 remains `PARTIAL`: `PlayerState` has no independent swimming pose/volume/animation save field. This is Save owner evidence only, not native Player input/swimming/action/boarding/tree/audio/performance/final visual acceptance.
- Result/log/handoff: `Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`, `Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md`. Next: `Q25_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q25 current-source first-voyage/loop revalidation after Q20-A05

- `D19Q25FirstVoyageLoopDiagnosticTests`=`2/2 PASS`, Pipeline `0.42s`; `D19R02A03RoutePlayModeTests`=`2/2 PASS`, Pipeline `7.26s`. D30 seven-stage projection, D23 `40` tick first-voyage/arrival/return/save projection and bounded InitialHarbor trade/depart/return route all passed.
- Console seq `59130→59278`: `148` entries, Error=`0`, Warning=`12`, dropped=`false`; warnings are D27 readiness, D2 unverified skin/procedural fallback and absolute-Euler animation families. D31 runtime performance approval remains pending; D7 audio fallback remains true.
- Q25 remains `PARTIAL`: owner-route regression only; no complete human play, native Player WASD/E/I/M/T/Esc, camera/action/swimming/boarding visual, tree grounding, audio, target-device performance or final visual acceptance is claimed.
- Result/log/handoff: `Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`, `Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md`. Next: `Q26_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q26 current-source full PlayMode revalidation after Q20-A05

- `D19R03Q26GameplayDefectPlayModeTests`=`21/21 PASS`, Pipeline `60.75s`; W swimming, E remote rejection/real board-disembark, legacy-tree suppression, R03 vegetation/foam, 42 grounded targets, shoreline rocks, helm anchors and Q26-A22 cooldown all passed within the suite.
- Key markers: A01 planar displacement=`0.2915`; A03 legacy/R03/unsupported=`0/24/0`; A06 swim displacement=`0.6049`; A12 grounded=`42`; A14 rocks=`9`; A21 `maxArmDelta=6.84/maxFootDelta=2.74`; A23 `maxHandAboveHead=0.134`, formal asset boundary=`unverified`.
- Console seq `59278→60535`: `1257` entries, Error=`0`, Warning=`126`, Log=`1131`, dropped=`false`; D27 readiness, D2 fallback/skin, absolute-Euler animation, D31 performance pending and D7 audio fallback remain recorded.
- Q26 remains `PARTIAL`: code-level owner/collision/injected Input System only; no foreground physical input, compositor-visible visual, formal Avatar/action/natural swim/IK/helm, all-camera vegetation/intersection, audio, target-device performance or VC-G01 final acceptance is claimed.
- Result/log/handoff: `Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R4_RESULT.json`, `Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R4.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md`. Next: `Q27_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q27 current-source D16 performance revalidation after Q26 R4

- `D16PerformanceBudgetTests`=`6/6 PASS`, Pipeline `0.02s`; `D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`, Pipeline `64.06s`, test `63.4356615s`.
- Null Device report: `60.00715255737305s`, `162` samples, average `27.9046 FPS`, minimum `0.7995 FPS`, average frame `170.0344ms`, longest `1250.8234ms`, `88` frames over `50ms`, GPU unavailable; probe owner and D16 budget contract true.
- Console seq `60535→60605`: `70` entries, Error=`0`, Warning=`6`, dropped=`false`; known D27/D2 fallback/readiness family, D31 performance pending and D7 audio fallback retained.
- Q27 remains `PARTIAL`: Null Device metrics are diagnostic risk only, not target-device/GPU/smoothness PASS. Result/log/handoff: `Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`, `Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md`. Next: `Q28_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-14 Q28 current-source R12 candidate build after Q26 R4

- Unity Pipeline dry-run valid; `build_0cc5cd156fe3` `StandaloneWindows64` BuildReport=`Succeeded`, `0 errors / 1 warning`, `175212534` bytes, `33602ms`. Warning: Runtime Pipeline `enableInBuilds=false`, so Player control plane is not embedded.
- New R12 EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`; Assembly-CSharp SHA256=`2A6124CC995FDDEF11E6DD2D15C246D91D818931B58338D3730B8C6EA83B312D`.
- D17-A08 current source guard=`1/1 PASS`, Pipeline `14.55s`; 10 cycles root angle=`64.00°`, root offset=`0.015..0.034`, hand/head=`0.993..1.000`. Editor ready/stopped; Runtime Pipeline disabled/port 0.
- Q28 remains `PARTIAL`: build/guard evidence only; no Player foreground, physical input, compositor screenshot, GPU/target performance or final visual acceptance. Result/log/handoff: `Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json`, `Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md`. Next: `Q29_CURRENT_PLAYER_ROUTE_REAUDIT`.

## 2026-09-14 Q30 current-source independent review R4 after Q29 R10

- Python targeted regression: 65/65 PASS; Unity Pipeline D16 input router 11/11, D17 action quality 16/16 and VcG01 world logic 14/14.
- Q29 R10 hidden Player route parses as PASS_WITHIN_SCOPE with runtime error=0, strict Player error-token count=0 and clean close. D2 SailingSystem defines S as deceleration/stop rather than reverse, so no production change was opened for the observed stationary ship after S.
- Q30 remains PARTIAL: no foreground compositor visual, physical Win32 input, formal action/avatar, natural swim/helm, approved vegetation, all-camera grounding, target-device performance or final visual acceptance. Editor ready/stopped; error=0; warnings=157 and dropped=false; console cleared. Result/log/handoff: Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4_RESULT.json, Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4.log, docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md. Next: Q31 final native Player visual gate.

## 2026-09-14 Q29 current-source Pipeline R10 full hidden Player route

- Fresh Unity Pipeline build `build_b5958f0eeca0`: StandaloneWindows64, Succeeded, `0 errors / 0 warnings`, `220891074` bytes, `66197ms`; Player Runtime Pipeline was temporarily enabled for this run and restored afterward.
- Natural MainMenu NewGame/Confirm route and hidden Player controls covered W, I modal W blocking, M/T repeated open and Esc, E board/disembark/reboard, Y voyage, in-voyage M rejection, ship W/A/S/D and Esc pause/resume. Deterministic direct-scene probes and early owner-not-ready T are recorded separately and are not full-route acceptance.
- Runtime error console returned `0`; Player closed cleanly, descriptor was removed, strict Player log `ERROR|EXCEPTION|CRASH|FATAL` count was `0`. Runtime Pipeline readback is `enableInBuilds=false / port=0`.
- Q29 child scope is `PASS_WITHIN_SCOPE`; parent remains `PARTIAL`. No final foreground/compositor visual, physical Win32 input, authored action/avatar, natural swim/helm, all-camera intersection, target-device performance or final visual acceptance is claimed. Result/log/handoff: `Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10_RESULT.json`, `Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md`. Next: Q30 independent review of current Q28/Q29 evidence and targeted defect backlog.

## 2026-09-14 Q31 R5 safe pause point

- Current-source visible native Player build `build_fc9d95893976` succeeded with `0 errors / 0 warnings`; the New Game title/captain-name overlap was fixed by moving the title from `y=150` to `y=210` and verified in `Q31_R5_UI_FIX_NEWGAME.png`.
- The same visible Player reached `InitialHarbor`, produced bounded W/E/W/S frames, returned runtime error console `0`, Player strict error-token count `0`, and exited cleanly. Runtime Pipeline and development build settings were restored to disabled/non-development values.
- Q31 remains `PARTIAL`; VC-G01 remains `IN_PROGRESS`; VC-G02/VC-G03 remain `LOCKED`; D31-P04/S05/G06 remain `PARTIAL`. The harbor is still low-poly/procedural, this route did not visually prove open-water swimming, and formal action/avatar, natural swim/helm, vegetation/intersection, physical input and target-device performance remain open.
- Development is temporarily paused at this safe point. Resume from `Q31 R5` with one single-owner visual defect. Result/log/handoff: `Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5_RESULT.json`, `Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260914_Q31_R5.md`.

## 2026-09-15 current-source rebind after resumed execution

- Q00=`VERIFIED_WITH_SCOPE`: source HEAD=`cee7a9a0581cdaa4aa0a7eee7345694815770d63`; exact current-source Player `build_c0aabd621f0d` succeeded with `0 errors / 8 warnings`; Pipeline runtime probe, open-water transition and boarding state are recorded without claiming compositor or foreground input.
- Q01=`PASS_WITHIN_SCOPE`: current owner/root-cause reconciliation recorded for action distortion, swimming, boarding/contact, vegetation and WASD/HUD focus. Visual and native-input boundaries remain open.
- Q09=`PASS_WITHIN_SCOPE`: D17A11 current-source PlayMode `1/1 PASS`; ten scaled boarding/disembarking cycles and remote rejection passed with zero measured deck/anchor drift; next Q10.
- Q08 historical safe-point result remains immutable at its prior source HEAD; it is not silently re-labeled as a current-head test. Q31=`PARTIAL`, VC-G01=`IN_PROGRESS`, VC-G02/VC-G03=`LOCKED`, Q20=`PARTIAL/BLOCKED_ASSET`, D31-P04/S05/G06=`PARTIAL`.
- Result/log/handoff: `Q00_CURRENT_SOURCE_REBIND_Q03A23_20260915_LUNA_RESULT.json`, `Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A23_20260915_LUNA_RESULT.json`, `Q09_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`; next `Q10_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q10 current-source ship-motion relation revalidation

- Q10=`PASS_WITHIN_SCOPE`: `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` completed `1/1 PASS` on current HEAD `cee7a9a0`; ten motion snapshots retained the boarded player's relative position/rotation with measured max errors `0.000000`, then restored safe disembark state.
- The run measured max ship translation `1.513m` and heading change `32°`; Editor finished `ready/stopped`, Error query=`0`. Six known warnings were retained as a warning boundary.
- Result/log/handoff: `Q10_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q10_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md`; next `Q11_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q11 current-source safe-disembark revalidation

- Q11=`PASS_WITHIN_SCOPE`: D17A13 current-source PlayMode `1/1 PASS`; ten legal disembark cycles, no-ground rejection and final `grounded=True / mode=Character` passed; Error query=`0`.
- The expected no-walkable-point rejection Warning is retained as behavior evidence and not treated as a zero-warning claim.
- Result/log/handoff: `Q11_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q11_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md`; next `Q12_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q12 current-source camera revalidation

- Q12=`PASS_WITHIN_SCOPE`: D17A14 current-source PlayMode `1/1 PASS`; ten camera mode/target cycles, obstruction contraction `5.500m→3.882m`, recovery to `5.500m`, and `109` collision checks passed; Error query=`0`.
- This is a bounded CameraFollow regression, not final compositor-visible framing or manual visual acceptance.
- Result/log/handoff: `Q12_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q12_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md`; next `Q13_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q13 current-source input/UI revalidation

- Q13=`PASS_WITHIN_SCOPE`: D17A15=`1/1 PASS`, D16InputRouter=`11/11 PASS`, D16PrimaryHudNavigation=`2/2 PASS`; W/S-HUD isolation, Esc modal restore, keyboard/gamepad and rebind routes passed; final Error query=`0`.
- Six known D2/D27 fallback/readiness warnings remain explicitly recorded; no zero-warning claim and no foreground Win32 input claim.
- Result/log/handoff: `Q13_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q13_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md`; next `Q14_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q14 current-source shore/water/deck integration revalidation

- Q14=`PASS_WITHIN_SCOPE`: D17A16 current-source PlayMode `1/1 PASS`; marker confirms shore→water→shore, Ship owner, ship turn, no-ground rejection and legal disembark; `imported_neutral=False`; Error query=`0`.
- Expected rejection warning remains explicit; no authored action, native input or final visual claim.
- Result/log/handoff: `Q14_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q14_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md`; next `Q15_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q15 current-source panel-focus revalidation

- Q15=`PASS_WITHIN_SCOPE`: D29Q15 current-source PlayMode `1/1 PASS`; I/M/T shortcut owner, warehouse/market/tavern ten-cycle exclusivity, duplicate rejection and Gameplay recovery passed; Error query=`0`.
- This is owner/focus logic evidence only; no foreground Player business-visual or native input claim.
- Result/log/handoff: `Q15_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q15_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md`; next `Q16_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q16 current-source trade-bridge revalidation

- Q16=`PASS_WITHIN_SCOPE`: D29Q16 current-source PlayMode `1/1 PASS`; not-ready rejection, duplicate owner guard, buy/sell one-change success and unchanged rejection states passed; Error query=`0`.
- Result/log/handoff: `Q16_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q16_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md`; next `Q17_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q17 current-source shipyard/preparation revalidation

- Q17=`PASS_WITHIN_SCOPE`: D29Q17 current-source PlayMode `1/1 PASS`; success/rejection/duplicate/cancel/exception recovery and primary input restoration passed; Error query=`0`.
- Controlled exception Warning is retained as behavior evidence; no zero-warning claim.
- Result/log/handoff: `Q17_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q17_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md`; next `Q18_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q18 current-source HUD/layout revalidation

- Q18=`PARTIAL`: D31 HUD resolution matrix `3/3 PASS`, D31 VisualLogic assembly `36/36 PASS`, repository-root Python HUD suite `30/30 OK`; Error query=`0`.
- The initial wrong filter returned `0 tests` and was excluded; the corrected assembly filter is the recorded 36/36 evidence.
- Native Player screenshots, GPU font rasterization, longest English text, physical mouse/Win32 keyboard and final visual overlap remain open.
- Result/log/handoff: `Q18_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q18_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md`; next `Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`.

## 2026-09-15 Q19 current-source crew/save revalidation

- Q19=`VERIFIED`: `Q19CrewInteractionAuditTests`=`1/1 PASS`（测试 `0.3604646s`、Pipeline `0.37s`）；`Save.Tests`=`377/377 PASS`（failed/skipped/inconclusive 均为 `0`，Pipeline `32.14s`）；`dotnet build AgentCore`=`0 warnings / 0 errors`。
- Current HEAD=`cee7a9a0`，Editor=`ready/stopped`，Error query=`0`。合并测试后 Warning=`27`，均为已知 D2/D27/D29 行为边界，已保留证据并清空 Console；不做零 Warning 声明。
- Q19 只证明离线 crew proposal→owner decision→relation facts→AgentMemory/domain SQLite save/restore；online LLM、native Player 对话 UI/input/visual、动作、游泳、碰撞表现、性能和正式视觉门禁仍未证明。
- Result/log/handoff: `Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md`; next `Q20_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-15 Q20 current-source vegetation revalidation

- Q20=`PARTIAL/BLOCKED_ASSET`: candidate EditMode `1/1 PASS` (`0.0021969s`); R03 grounding/legacy quarantine PlayMode `1/1 PASS` (`3.0194955s`; Pipeline `3.12s`). Marker=`activeLegacyTreeRenderers=0`, `activeR03Vegetation=24`, `vegetation_grounded=26`, `vegetation_hidden_without_terrain=0`.
- Resource gate remains `BLOCKED / local_development_ready_count=0 / eligible_entry_count=0`; `RenderAuthoredPalms=false`; formal authored palm references=`0`; candidate tree_palm references=`1`. No formal asset promotion or palm restoration.
- Editor=`ready/stopped`; Error query=`0`; Warning=`6` known D2/D27 warnings retained and Console cleared. Candidate/containment scope does not prove full-camera visual grounding, occlusion/no-clipping, native Player/GPU visual quality, performance or VC-G01.
- Result/log/handoff: `Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md`; next `Q21_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-15 Q21 current-source harbor-space revalidation

- Q21=`PARTIAL`: `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`（测试 `0.189254s`、Pipeline `0.20s`）；四服务建筑 `37 Renderer / 0 missing`，`23` colliders=`1` R03 terrain + `22` D27 owner，pairwise XZ overlap=`0`，稳定锚点保留。
- Editor=`ready/stopped`; Error=`0`; Warning=`0`; Console 已清空。当前只证明静态空间/renderer/material/collider owner 合同，不证明 native Player 镜头/遮挡/键鼠可达性、性能或最终视觉。
- Result/log/handoff: `Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md`; next `Q22_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-15 Q22 current-source sea/weather revalidation

- Q22=`PARTIAL`: `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`（测试 `0.1758288s`、Pipeline `0.18s`）；water=`96x96`, sea_level=`-0.0397`, foam=`28`, ship_min_y=`0.0600`, hull_bottom=`-0.0400`, fish=`1.00/0.90/0.55`, precipitation=`0/560/980`, wave=`1.00/1.40/2.35`。
- Editor=`ready/stopped`; Error=`0`; Warning=`0`; Console 已清空。静态海水/船体/天气合同不证明 native Player 游泳、船海镜头、泡沫/雾/音效、物理输入、性能或最终视觉。
- Result/log/handoff: `Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md`; next `Q23_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-15 Q23 current-source formal scene integration revalidation

- Q23=`PARTIAL`: `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`（测试 `0.3685381s`、Pipeline `0.38s`）；active roots=`34`、R03/ship=`1/1`、R03 renderers=`362`、missing materials/scripts=`0/0`、terrain colliders=`1`、hull bottom=`-0.0400`、reopen=`true`。
- Editor=`ready/stopped`; Error=`0`; Warning=`0`; Console 已清空。当前只证明正式场景 Prefab 唯一性、GUID/资源来源、静态缺失项和重开稳定性，不证明 native Player 镜头/键鼠/动作/游泳/船控/音效/性能/逐物体接地/最终视觉。
- Result/log/handoff: `Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json`, `Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md`; next `Q24_CURRENT_PIPELINE_REAUDIT`.

## 2026-09-15 Q24 current-source save/restore revalidation and safe pause

- Q24=`PARTIAL`：`D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`（Pipeline `1.19s`），覆盖失败保存保护、陆地 port/clock/money/cargo/crew round-trip、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 和 `D2_PlayerSpawnAnchor` 安全恢复。
- 当前 Unity Editor PID=`63572`，最终 `ready/stopped`；Error=`0`、Warning=`0`；取证后 Console 已清空。Q24 测试类没有专用 `[Q24]` marker，结果与耗时来自完成态 Pipeline `test_status`，未伪造运行 marker。
- Q24 只证明 SaveSystem/D2 的有界恢复和上一份好档保护，不证明 native Player、游泳/动作、船控视觉、植被、音效、性能或完整游戏。临时安全暂停点已建立，没有活动测试进程；恢复入口为 Q25 当前首航/循环集成诊断。
- 结果/日志/交接：[Q24 result](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)、[Q24 log](Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)、[Q24 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)、[pause handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915.md)。

## 临时暂停任务板

- 已完成：Q00/Q01/Q09—Q17 各自有界当前源回归；Q18、Q20—Q24 已形成明确结果边界；Q19 离线 owner/save 范围 `VERIFIED`。
- 进行中：无活动执行进程；项目开发在 Q24 安全点临时暂停。
- 阻塞/未完成：Q18/Q20—Q24 的完整 Player/视觉边界、Q25—Q35、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、D31-P04/S05/G06=`PARTIAL`。
- 下一步：恢复后执行 `Q25_CURRENT_PIPELINE_REAUDIT`，继续首航/循环集成诊断；不得将 Q24 存档 PASS 扩大为完整游戏 PASS。

## 2026-09-15 Q25 current-source first-voyage/loop revalidation

- Q25=`PARTIAL`：D30/D23 `D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.50s`），InitialHarbor `D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `5.64s`）。
- 当前 marker：D30 `preparation>market>tavern>shipyard>boarded>departed>returned`、failure/duplicate stable；D23 `40` ticks、arrived/returned、`d19_facts=172`、`d21_events=163`、save restored、duplicate owner checks；PlayMode 两个路线用例均通过。
- PlayMode Console=`Error 0 / Warning 12`，警告家族为 D27 readiness `6`、D2 未验证 `ph_crew_male` `2`、程序化低模回退 `2`、generated D6CrewAnimator 绝对 Euler `2`；D7 audio=`fallback=True`，D31 runtime performance approval=`pending`。Editor=`ready/stopped`，Console 已清空。
- Q25 只证明 D30/D23 owner 路线和有限 InitialHarbor 路线，不证明 native Player、完整人工游玩、动作/游泳/上船视觉、植被、音效、性能或最终视觉。下一步 Q26 当前源码测量基线。
- 结果/日志/交接：[Q25 result](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)、[Q25 log](Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)、[Q25 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。

## 2026-09-15 Q26 current-source PlayMode revalidation R5

- Q26=`PARTIAL`：重编译后的 `D19R03Q26GameplayDefectPlayModeTests`=`21/21 PASS`，Pipeline=`56.21s`，failed/skipped/inconclusive=`0`；`recompile_status=completed / failed=false / errors=[]`。
- 本轮只修改 Q26A22 测试隔离：保存/恢复 `Time.timeScale`，测试内按 `Time.time` 等待 D2 的 `Time.time` 再入水迟滞。此前全量 `20/21` 的旧失败被定位为 stale test assembly（堆栈仍指向旧断言行 `237`），新程序集运行后出现 timing marker 并稳定通过；D2 产品代码未改。
- Q26A22 marker=`before_exit time=29.383 / timeScale=1.000`、`after_wait time=29.952 > deadline=29.833`、`immediate_reentry=False`、`reentered_after_cooldown=True`、`position=(0.00,0.40,30.10)`；A01/A02/A04/A06/A12/A14/A17/A21/A23 等同轮 marker 均通过。
- Console=`Error 0 / Warning 98`；warning families=`D27 readiness 48`、`D2 procedural fallback 17`、`D2 unverified ph_crew_male 16`、`D2 absolute-Euler animator 17`。Info 查询达到 `1000` 条 API 窗口并抽取 Q26 marker；取证后 Console 已清空。Editor=`ready/stopped`；Runtime Pipeline=`enableInBuilds=false / port=0`。
- Q26 有界 gameplay/owner/collision/Input System 注入/procedural fallback 证据通过，但不等于 native Player 前台 Win32 输入、最终视觉、正式 Avatar/action、自然 IK/泳姿、全镜头植被/穿模、音频或性能通过；Q26 保持 `PARTIAL`。下一张执行卡为 `Q27_CURRENT_PIPELINE_REAUDIT`。
- 结果/日志/交接：[Q26 R5 result](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5_RESULT.json)、[Q26 R5 log](Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5.log)、[Q26 handoff](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-15 Q27 current-source performance revalidation R4 after Q26 R5

- Q27=`PARTIAL`：`D16PerformanceBudgetTests`=`6/6 PASS`（Pipeline `0.03s`）；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`（Pipeline `63.69s`，test `63.5862239s`）。
- Null Device report：`60.2771263s`、`162` samples、average `30.9174 FPS`、minimum `0.8399 FPS`、average frame `157.7774ms`、longest `1190.5581ms`、`84` frames over `50ms`、average main thread `16.4134ms`、GPU unavailable；probe owner and budget contract true.
- Console after capture: Error=`0`、Warning=`192`、dropped=`false`; families=`D27 readiness 96 / D2 procedural fallback 32 / D2 unverified ph_crew_male 32 / D2 absolute-Euler 32`; Console cleared afterward, Editor=`ready/stopped`.
- No D16 production code/configuration changed. Q27 remains `PARTIAL`: Null Device metrics are diagnostic risk only, not target-device/GPU/smoothness/native Player visual or input PASS. Next=`Q28_CURRENT_PIPELINE_REAUDIT`.
- Result/log/handoff: `Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5_RESULT.json`, `Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md`.

## 2026-09-15 Q28 current-source candidate R13 build and D17 guard after Q27 R4

- Q28=`PARTIAL`：dry-run=`valid=true / validationErrors=[]`；`build_73c3a973e2d8` 为 `StandaloneWindows64 / Succeeded / 0 errors / 1 warning / 198187532 bytes / 13923ms`。唯一 warning 为 `RuntimePipelineConfig enableInBuilds=false`，R13 不带 Player Runtime Pipeline。
- R13 EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；`Assembly-CSharp.dll` SHA256=`45451540097E21BC0E57ED98DDABC0FF3EF1FEC7CC0743A1BAA8AA34F2C8B74E`；构建设置=`developmentBuild=false / allowDebugging=false / connectWithProfiler=false / enableInBuilds=false / port=0`。
- D17A08=`1/1 PASS`（Pipeline `14.02s`、test `13.9233925s`）；10 周期 rootAngle=`64.00°`、rootOffsetY=`0.012..0.034`、hand/head=`0.992..1.000`。这是根安全守卫，不是正式 authored animation/avatar 或最终画面通过。
- D17 后 Console=`Error 0 / Warning 187 / dropped=false`，families=`Pipeline disabled 1 / D27 readiness 93 / D2 procedural fallback 31 / D2 unverified ph_crew_male 31 / D2 absolute-Euler 31`；Editor=`ready/stopped`，取证后清空。
- R13 只证明当前候选包身份、BuildReport 和 D17 root guard；不证明 Player 前台、物理输入、截图、GPU/目标设备性能、正式动作或 VC-G01。下一步=`Q29_CURRENT_PLAYER_ROUTE_REAUDIT`。
- Result/log/handoff: `Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4_RESULT.json`, `Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4.log`, `docs/game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md`.

## 2026-09-15 Q29 R11 entry smoke and temporary safe pause

- Q29 R11 从当前 HEAD `3dbde02f` 构建 `build_fb5d1ad58690`，dry-run 有效，BuildReport=`Succeeded / 0 errors / 7 warnings / 243870645 bytes / 34076ms`；hidden Player 通过真实 Pipeline pointer 从 MainMenu 进入 `InitialHarbor` build index=`2`。
- 本轮按用户暂停要求在入口后停止，没有把未执行的 W/A/S/D、E、I、M、T、Esc 或完整业务路线记为 PASS；Q29 R10 仍是最近一次完成的 hidden Player 全交互证据。
- `quit` 后 PID=`2724` 和 descriptor 均消失；Runtime error=`0`、Player 严格错误词=`0`；最终 Runtime=`enableInBuilds=false / port=0`、Build=`developmentBuild=false`、Editor=`ready/stopped`。
- Q29=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。暂停交接：[PAUSE_20260915_Q29](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915_Q29.md)；结果/日志：`Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED_RESULT.json` / `Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED.log`。

## 2026-09-15 Q30 current-source independent review R5

- Q30 R5 在当前 HEAD `94950f36c0d5a3daf4ba719216808a0ae6d71be9` 完成：Python `65/65`、D16InputRouter `11/11`、D17ActionQuality `16/16`、VcG01WorldLogic `14/14` 均通过范围内回归；本轮无生产代码/测试契约/持久配置变更。
- Editor 最终 `ready/stopped`；Runtime Pipeline `enableInBuilds=false / port=0`；Build `developmentBuild=false / allowDebugging=false / connectWithProfiler=false`。
- Console 边界：取证前真实 FMOD output-device 初始化错误 `1`，Warning `160` 且 `dropped=false`。FMOD/音频维持 `BLOCKED_ENVIRONMENT`；不能以 Windows 设备存在或 AudioSettings 配置代替播放证据。取证后执行 `clear_console`；两次误用 CLI 命令造成的 ResolveCommand 仅为工具噪声，单独记录并排除。
- 状态不变：Q30=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步 Q31 最终 native Player visual gate。

结果：[Q30 R5](Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5_RESULT.json)；日志：[Q30 R5 log](Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5.log)；交接：[Q30](../../../docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-15 Q31 R6 current-source visible native Player route

- `build_9f35f8fcb2f4`：`StandaloneWindows64 / Succeeded / 0 errors / 0 warnings / 243870645 bytes`；Unity Pipeline 可见原生 Player `1600x900` 从 MainMenu NewGame/Confirm 进入 `InitialHarbor`，完成 E 登船/下船并写出真实 compositor-visible 帧。
- W/A/D 下船路线在约 `z=-4.94 / -18.48 / -23.36` 出现边界停滞；本轮没有权威 `IsSwimming=true` marker，既有 S/open-water 结果不与本轮混算。
- 会话在最终 runtime console、严格 Player 日志和 clean-close 断言前中断；接管后 Player PID/descriptor 均不存在，Runtime Pipeline 已恢复 `enableInBuilds=false / port=0`，正确 `game` Editor=`127.0.0.1:7800 / ready`。
- 状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步是 connected Editor Pipeline 碰撞/状态诊断，不是盲目调坐标。

结果/日志：`Q31_CURRENT_NATIVE_PLAYER_VISUAL_20260915_LUNA_R6_RESULT.json` / `Q31_CURRENT_NATIVE_PLAYER_VISUAL_20260915_LUNA_R6.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md`。

## 2026-09-16 Q31 R7 visible native S-route and swim-presentation boundary

- 当前源码可见 Player 经 Pipeline pointer 进入 `InitialHarbor`；相机平面前方=`-Z`，W 是船/港口侧，S 是去水边的 `+Z` 路线。
- S 分段到约 `(0.33,0.40,18.80)` 时真实 runtime `IsSwimming=true / IsGrounded=false / D16InputRouter`；释放后仍为游泳态。该子范围=`PASS_WITHIN_SCOPE`，不等于自然泳姿视觉通过。
- 四张真实 `1600x900` 帧确认角色水平扭曲、四肢弯折、水面接触不可信；InitialHarbor 还观察到 `3.8–4.5 FPS`、平均帧 `224–261ms`。视觉与性能均保持开放。
- runtime console error=`0`、Player.log 严格错误行=`0`、Pipeline `quit` 后 PID/descriptor 消失；Runtime Pipeline 恢复 `enableInBuilds=false / port=0`。
- 状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步锁定 `D2CharacterAnimationController`/D7 presentation 与 formal avatar asset contract 的单一 owner。

结果/日志：`Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7_RESULT.json` / `Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7.log`；Editor 诊断：`Q31_CURRENT_EDITOR_SHORELINE_DIAGNOSTIC_20260916_R1.json`。

## 2026-09-16 Q31 R12 — Formal swim safe envelope

R12 真实 Development Player 通过 Unity Pipeline 从 InitialHarbor 沿 S 路线到达 `IsSwimming=true / IsGrounded=false`；D20 Formal Fish pose ready/apply=`true`。D20 泳姿 body 下移 `0.85m`、前臂/小腿伸展后，真实帧显示四肢和身体位置改善，但单臂划水与水面接触仍不自然，结果为 `IMPROVED_WITHIN_SCOPE`，不是自然泳姿 PASS。

验证：build `build_8eca102a61b4`，`0 errors / 0 warnings`；`D17A03CharacterEntersWaterAndReportsSwimmingAction=1/1 PASS`；runtime console error entries=`0`；Player.log 严格错误行=`0`；clean quit；Runtime Pipeline 恢复 `enableInBuilds=false / port=0`。状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步检查 action/boarding presentation 单一 owner。

结果/日志：`Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12_RESULT.json` / `Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12.log`。

## 2026-09-16 Q31 R13 — boarding/helm route boundary

## 2026-09-16 Q31 R13 安全暂停

本轮在 R13 的真实 Player 交互边界处收口并临时暂停开发。Player 已通过 Unity Pipeline `quit --exitCode 0` 退出，PID 与 `.unity-pipeline-runtime-port` 均已消失；连接的 `game` Editor 为 `ready/stopped`，Runtime Pipeline 已恢复 `enableInBuilds=false / port=0`。R12/R13 结果、截图、日志和 focused PlayMode 证据已登记，未把它们扩大为最终视觉通过。

- 状态保持：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。
- 未关闭：正式 action/avatar、自然 swim/helm IK、植被接地/穿模、前台物理输入、目标设备性能和最终人工视觉。
- 交接：[PAUSE_20260916_Q31_R13](../../../../docs/game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260916_Q31_R13.md)；结果：[pause result](Q31/Q31_PAUSE_CHECKPOINT_20260916_R13_RESULT.json)。
- 恢复入口：`Q31 R14`，只处理一个单一 owner 的可见缺陷，先 focused regression，再用当前源码构建 Player 复验；不进入 VC-G02/G03。

R13 复用 R12 Development Player 完成 E 登船、E 下船、重复登船和 W/A 船控。真实运行时：登船后 parent=`ph_ship_fishing`、D7 `PlayerBoarded=true`、玩家位于 HelmPoint；稳定后 D20 Formal candidate/formal state=`Helm`、ready/apply=`true`；W 推进船只，W+A 改变 heading。D17A02 新增 Board→Helm 契约后 `1/1 PASS`，D17A03=`1/1 PASS`，Editor recompile=`failed=false/errors=[]`。

真实 RenderTexture 帧证明玩家处于可见舵轮前且双手接近轮盘，但低模/程序化几何、正式 boarding/helm clip 和自然 hand IK 仍开放。状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步转向另一个单一 owner 缺陷。

结果/日志：`Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13_RESULT.json` / `Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13.log`。
