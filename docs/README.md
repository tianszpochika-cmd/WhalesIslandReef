# 项目文档索引

本目录采用“需求 → 设计 → 开发计划 → 历史证据”的单向结构。新任务只从当前权威入口派生，归档区只读。

## 当前权威入口

- [本地高质量剩余计划（Luna，2026-09-08）](game/LOCAL_QUALITY_REMAINING_PLAN.md)：36 张剩余诊断/修复/验收卡、动作/碰撞/游泳质量门槛、依赖、证据协议和可复制目标；细分 D25—D31，不替代正式结果。
- [本地高质量执行看板与 Q00 交接](../game/tools/results/LOCAL-QUALITY/STATUS.md)：Q00 暂停点事实快照、构建对应关系和 Q00→Q01 当前执行状态；不覆盖 D25—D31 正式状态。
- [本地高质量 Q00 当前源重绑定](game/design/common/handoffs/LOCAL-QUALITY/Q00.md)：Q03-A11 船向修复后的当前工作区、源码/场景/Player 哈希关系、旧证据 stale 边界和下一候选构建条件。
- [本地高质量 Q01 根因与尺度表交接](game/design/common/handoffs/LOCAL-QUALITY/Q01.md)：角色/视觉根、胶囊、船锚点、岛面/水位事实，以及 Q02—Q13 文件白名单；不代表四项缺陷已修复。
- [本地高质量 Q02 动画单一写者交接](game/design/common/handoffs/LOCAL-QUALITY/Q02/Q02.md)：D2 visualRoot 姿态提交、生命周期回归与动作状态边界。
- [本地高质量 Q02-A D7 落地消费者交接](game/design/common/handoffs/LOCAL-QUALITY/Q02-A/Q02-A.md)：D7 测量、D2 消费的落地校准契约与直接写入诊断。
- [本地高质量 Q03 动作资源门禁交接](game/design/common/handoffs/LOCAL-QUALITY/Q03/Q03.md)：D6/D20 动作资源契约审计、隔离回归与 `BLOCKED_ASSET` 边界。
- [本地高质量 Q03-A01 fallback 游泳 containment 交接](game/tools/results/LOCAL-QUALITY/Q03-A01/RESULT.json)：`50/50` 定向、D17 `15/15`、全量 Python `1134/1134` 与 R25 原生路线；只收紧水面视觉根，不解除 Q03 正式动作资源阻塞。
- [本地高质量 Q03-A08-D31-P04 海岸剖面修复结果](game/tools/results/LOCAL-QUALITY/Q03-A08-D31-P04/RESULT.json)：R03 海岸垂直切面改为 1.35m 沙质 shelf/水下过渡，Q03-A08 `1/1`、Q20-Q26 回归通过；headless Pipeline 无 GPU，仍不宣称 post-fix Player 视觉 PASS。
- [本地高质量 Q03-A09 当前源码 Player 与原生路线结果](../game/tools/results/LOCAL-QUALITY/Q03-A09/RESULT.json)：当前源码 Windows Player、Win32 WaterProbe、probe archive 与真实截图证据；Q03-A09=`PARTIAL`，不代表动作/泳姿/港口视觉通过。
- [本地高质量 Q03-A10 fallback 游泳手臂轴向结果](../game/tools/results/LOCAL-QUALITY/Q03-A10/RESULT.json)：D2 fallback 局部轴根因、当前源码 Player、RERUN100 独立 archive 与截图；手臂前伸在有限范围改善，Q03-A10=`PARTIAL`。
- [本地高质量 Q04 游泳状态与水位交接](game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md)：InitialHarbor 浅水/深水重复进入、移动、退出和水位稳定性证据。
- [本地高质量 Q05 游泳姿态交接](game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)：整体验证游泳倾斜、物理根隔离和离水 neutral 恢复。
- [本地高质量 Q06 行走跳跃碰撞交接](game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)：InitialHarbor 10 次跳跃落地与基础正/斜向移动回归。
- [本地高质量 Q07 岸线、码头与跳板碰撞交接](game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)：活动地形/码头/跳板碰撞体与双向小步路线证据。
- [本地高质量 Q08 动作整合检查交接](game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)：因 Q03 动作资源门禁阻塞而保持 BLOCKED，不扩大中立姿态证据。
- [本地高质量 Q09 登船锚点交接](game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)：非单位船根缩放下稳定舵位锚点、十次上下船与远离拒绝。
- [本地高质量 Q10 航行甲板关系交接](game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)：船位置/航向变化时玩家相对稳定船根的局部关系。
- [本地高质量 Q11 安全下船交接](game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)：十次合法下船、无落点拒绝保留船控与恢复验证。
- [本地高质量 Q12 相机遮挡与模式恢复交接](game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)：模式/目标切换、外部遮挡收缩与阻尼恢复。
- [本地高质量 Q13 输入恢复与 UI 导航隔离交接](game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)：W/S 与 UGUI 导航隔离、Esc 模态恢复、D16 输入阻塞让权。
- [本地高质量 Q13-A02 UI 导航策略再断言结果](game/tools/results/LOCAL-QUALITY/Q13-A02/RESULT.json)：修复外部 UI owner 恢复 `move` action 后游戏态未再断言的竞态，并以 D16 定向与 D17 全量 PlayMode 回归留痕。
- [本地高质量 Q14 船岸整合前置检查交接](game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)：入水/上岸、登船、W+D 转向、无落点拒绝与合法下船状态链。
- [本地高质量 Q15 面板焦点与快捷键恢复交接](game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)：D29/D16 的 I/M/T 真实键盘路由、临时页互斥、重复请求拒绝与十轮恢复证据。
- [本地高质量 Q16 交易桥与拒绝反馈交接](game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)：D29/D3 的 owner 未就绪、快速双击、买卖一次变化、余额不足与满仓反馈。
- [本地高质量 Q17 船坞与整备桥交接](game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)：D29/D9/D4 的修船、材料不足、重复/幂等、取消、船坞拒绝与异常恢复。
- [本地高质量 Q18 HUD 与本地化尺寸矩阵交接](game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)：D25/D29 三个 16:9 分辨率的代码级安全矩形和当前原生视觉/英文边界。
- [本地高质量 Q19 船员与反馈审计交接](game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)：D9 离线提议、D10 owner 裁决、关系/行为存档恢复和 AgentMemory 持久化边界。
- [本地高质量 Q20 植被恢复交接](game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)：R03 候选棕榈的真实 bounds 接地审计、当前浮空风险隔离和资源准入阻塞。
- [本地高质量 Q20-A01 legacy tree 隔离结果](game/tools/results/LOCAL-QUALITY/Q20-A01/RESULT.json)：D26 嵌套 `D19_SaintMargaret_Root` Renderer containment、Q26/A03 `1/1`、R26 真实输入路线；不代表正式树资源完成。
- [本地高质量 Q20-A02 直挂 legacy palm 隔离结果](game/tools/results/LOCAL-QUALITY/Q20-A02/RESULT.json)：D26 直挂/嵌套 `ph_palm_tree` Renderer containment、Q26/A03 `1/1`；不代表正式树资源完成。
- [本地高质量 Q20-A03 运行时植被接地守卫结果](game/tools/results/LOCAL-QUALITY/Q20-A03/RESULT.json)：当前 InitialHarbor `24/24` R03 植被接触、旧棕榈 Renderer quarantine `0`；不代表正式资源或全镜头视觉完成。
- [本地高质量 Q21 港口空间审计交接](game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)：R03 服务建筑 footprint、重叠/接地、路网空间组、稳定锚点与碰撞 owner 边界。
- [本地高质量 Q21 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q21/Q21_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码下港口空间专项 `1/1 PASS`；不代表 Player 视觉/物理输入验收。
- [本地高质量 Q22 船海天气审计交接](game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)：R03 水线/船体吃水、泡沫材质、D28 Calm/Rain/Storm 与 D9 鱼类天气投影边界。
- [本地高质量 Q22 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q22/Q22_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码下船海天气专项 `1/1 PASS`；不代表 Player 视觉/物理输入验收。
- [本地高质量 Q23 正式资源集成审计交接](game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)：InitialHarbor 唯一 active R03/D27/船 Prefab、GUID、缺失脚本/材质和关闭重开一致性。
- [本地高质量 Q23 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q23/Q23_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码下正式场景集成专项 `1/1 PASS`；不代表 Player 视觉/输入验收。
- [本地高质量 Q24 保存恢复诊断交接](game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)：陆地/出海 Save round-trip、D2 安全出生锚点和中断保存保护边界。
- [本地高质量 Q24 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q24/Q24_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码保存恢复专项 `3/3 PASS`；不代表 Player 视觉/输入验收。
- [本地高质量 2026-09-15 临时安全暂停交接](game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915.md)：Q24 存档恢复诊断完成后的安全暂停边界、未完成项与 Q25 恢复入口。
- [本地高质量 Q24 当前 Pipeline 复核 Luna 2026-09-15](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)：当前源码 `3/3 PASS` 的存档 owner 证据；保持 `PARTIAL`，不代表原生 Player 或最终视觉通过。
- [本地高质量 Q25 首航流程整合诊断交接](game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)：D30 阶段投影、D23 首航/返港/保存投影和 InitialHarbor 路线回归边界。
- [本地高质量 Q25 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q25/Q25_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码 D30/D23 首航投影 `2/2 PASS`、InitialHarbor 路线 `2/2 PASS`；不代表 Player 手动/视觉验收。
- [本地高质量 Q26 真实 PlayMode 缺陷诊断交接](game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)：真实 W 游泳、登船可见舵锚点、R03 植被接地、动作/输入回归与性能/原生 Player 未完成边界。
- [本地高质量 Q26 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RECHECK_20260911_RESULT.json)：当前源码 Q26 缺陷类 `6/6 PASS`；不代表 standalone Player、截图或性能门禁通过。
- [本地高质量 Q26 Pipeline 舵位坐标复跑](../game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RERUN_20260911_RESULT.json)：当前源码 `6/6 PASS`，记录左右手与 `HelmWheel` 的真实运行时坐标；不等于最终截图视觉通过。
- [本地高质量 Q26/A10 船员出生点分离结果](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CREW_ANCHOR_SEPARATION_REGRESSION_PIPELINE_20260911.json)：完整 Q26 `10/10 PASS`，`ph_crew_male` 与玩家出生点保持 `2.5125m` 水平间距且仍在 `3.5m` 交互范围内；不等于 native Player 视觉通过。
- [本地高质量 Q26/A10 当前源码 Player R2 构建结果](../game/tools/results/LOCAL-QUALITY/Q26/Q26_A10_SCENE_BOUND_CANDIDATE_R2_BUILD_RESULT.json)：Unity Pipeline `build_9d0db92d0afd`、0 errors；真实 Win32 完整路线与 WaterProbe 分别取得 RERUN85/RERUN86 只读运行证据，但截图视觉仍为 PARTIAL。
- [VC-G01 Q26/A10 R2 真实路线结果](../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)：当前源码候选覆盖移动/转向、E 成功与远距拒绝、I/M/T/Esc、游泳和登船船控；VC-G01 仍 `IN_PROGRESS`。
- [本地高质量 Q27 D16 预算评估性能剖析交接](game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)：真实 Player 性能剖析否定 D16 预算评估为主要瓶颈；Q28 候选包与 Q29 路线仍待执行。
- [本地高质量 Q28 新候选 Player 与游泳视觉根保护交接](game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)：Q28-CANDIDATE-R2 构建和 D17 游泳视觉根保护；正式动作资源与植被视觉仍为 PARTIAL/BLOCKED_ASSET。
- [本地高质量 Q28 当前候选重建复核](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_RECHECK_20260911_RESULT.json)：当前源码 Q28-CANDIDATE-R3 非开发版 Player 构建证据；不代表视觉门禁通过。
- [本地高质量 Q29 真实 Player 输入路线交接](game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)：真实 Win32 输入、InitialHarbor、水域游泳样本、登船/下船、I/M/T/Esc、远船拒绝与截图证据。
- [本地高质量 Q29 当前候选 Win32 路线复核](../game/tools/results/LOCAL-QUALITY/Q29/Q29_PLAYER_RECHECK_20260911_RESULT.json)：Q28-CANDIDATE-R3 当前水域、登/下船、菜单、Esc 和远船拒绝路线；不代表最终视觉通过。
- [本地高质量 Q29 UI 导航冲突复核](../game/tools/results/LOCAL-QUALITY/Q29/Q29_UI_NAVIGATION_RECHECK_20260911_RESULT.json)：Q29 R4 真实 Win32 W/A/S/D 采样确认游戏态 UGUI 导航已抑制；0 次冲突事件；截图视觉仍为 PARTIAL。
- [本地高质量 Q30 动作、游泳与港口几何缺陷审查交接](game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)：R03 岩石/灌木几何收圆、fallback 动作与 HUD 输入隔离、D17/世界逻辑/空间审计回归，以及 Q30-R16 backdrop 隔离后的真实 Player 船控/游泳输入与视觉边界；R16 结果见 `game/tools/results/LOCAL-QUALITY/Q30/Q30_REMEDIATION_R16_RESULT.json`。
- [本地高质量 Q30 当前候选独立复核](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json)：当前 Q28-CANDIDATE-R3、Q29 两条真实路线和 Python `1144/1144` 全量回归的独立边界复核；仍不是最终视觉 PASS。
- [本地高质量 Q21-A01 房屋根节点接地修复交接](game/design/common/handoffs/LOCAL-QUALITY/Q21-A01.md)：R03 服务建筑按 `Height(x,z)` 贴地、55/55 定向回归、R17 真实 Player 路线与仍未收口的动作/游泳/登船镜头/植被边界。
- [本地高质量 Q21-A02 船上镜头可读性修复交接](game/design/common/handoffs/LOCAL-QUALITY/Q21-A02.md)：ShipDeck 取景收紧、42/42 定向回归、R18 舵位主体截图和动作/游泳/植被未收口边界。
- [本地高质量 Q21-A03 游泳表现与输入回归交接](game/design/common/handoffs/LOCAL-QUALITY/Q21-A03.md)：fallback 游泳 stroke/取景最小调整、80/80 定向回归、R20 真实 Win32 路线，以及低模动作/泳姿/舵位/植被仍为 `PARTIAL` 的边界。
- [本地高质量 Q21-A04 轮舵表现可读性修复交接](game/design/common/handoffs/LOCAL-QUALITY/Q21-A04.md)：R21→R24 轮舵几何/遮挡修复、85/85 定向回归、Q26 舵位 PlayMode `1/1` 和 R24 真实 Win32 路线；角色动作/游泳/植被/整体港口视觉仍为 `PARTIAL`。
- [本地高质量 Q21-A05 fallback 操舵姿态交接](game/design/common/handoffs/LOCAL-QUALITY/Q21-A05.md)：D2 Ship 模式通知、R1/R2 失败诊断、R3 Q26/A05 `1/1`、57/57 定向、1141/1141 全量 Python 与 R27 原生路线；正式动作/泳姿/植被和整体视觉仍为 `PARTIAL`。
- [本地高质量 Q31 人工试玩与 VC-G01 预验收交接](game/design/common/handoffs/LOCAL-QUALITY/Q31.md)：集中交付 Q21-A05/R27 当前候选、10—15 分钟试玩清单、问题模板和已知视觉缺陷；用户已反馈缺陷，Q31 当前为 `PARTIAL`，不代表 VC-G01 通过。
- [本地高质量 Q31 当前候选视觉门禁复核](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json)：Q28-CANDIDATE-R3 的真实路线与截图复核；运行证据存在，但截图与资源门禁仍不满足视觉通过。

- [统一需求总纲 v3.0](game/REQUIREMENTS.md)：所有系统、玩法、数量和完成定义；
- [统一设计总览](game/design/README.md)：领域结构、数据、边界和设计流程；
- [玩法设计总目录](game/design/gameplay/README.md)：经营、捕捞、动物、牌局、关系、装扮、探索和奖励；
- [D7 后统一开发路线](game/DEV_ROADMAP.md)：D8—D16 历史执行线、D17 保留基线和 D18—D24 本地开发轨；
- [D18—D24 画面与完整本地可玩路线](game/D18_D24_VISUAL_PLAYABLE_ROADMAP.md)：免费资源、样板岛、角色、船海、UI、完整闭环和扩量原则；
- [D18 免费资源库准入验收（S02 v2）](game/design/common/D18_FREE_ASSET_LIBRARY_ADMISSION_ACCEPTANCE.md)：资源证据准入、受控开发解锁与 D18-S03 运行时晋级门；
- [Agent 开发任务总索引](game/AGENT_DEVELOPMENT_TASKS.md)：列出全部章节、编号和执行顺序；实际提示词位于 D8—D16、D18—D24 独立章节文件，以及 D16/D17 剩余门禁第 9B 章；
- [D19 港口业务逻辑详细提示词](game/AGENT_DEVELOPMENT_CHAPTER_11_D19_PROMPTS.md)：48 个可直接复制的纯逻辑开发任务、精确前置和结果留痕位置；
- [D21 船舶、海况、天气与航海逻辑验收](game/design/common/D21_LOGIC_ACCEPTANCE.md)：D21 逻辑汇合门禁、确定性航程接口和证据状态；
- [D23 纯逻辑场景验收](game/design/common/D23_LOGIC_ACCEPTANCE.md) 与 [D23 全链路接口](game/design/common/D23_LOGIC_FULL_LOOP_INTERFACE.md)：固定种子跨系统循环、回放/长跑/中断证据和 Runtime Deferred 边界；
- [D24 逻辑候选版基线](game/design/common/D24_LOGIC_CANDIDATE_BASELINE.md)、[迁移兼容](game/design/common/D24_LOGIC_MIGRATION_COMPATIBILITY.md)、[逻辑覆盖](game/design/common/D24_LOGIC_CONTENT_COVERAGE.md) 与 [候选版说明](game/design/common/D24_LOGIC_RELEASE_NOTES.md)：D18—D23 回归、Save migration、覆盖边界和逻辑候选版定义；
- [D24 逻辑边界审计](game/design/common/D24_LOGIC_BOUNDARY_AUDIT.md)、[状态复核](game/design/common/D24_LOGIC_STATUS_REVIEW.md) 与 [候选验收](game/design/common/D24_LOGIC_CANDIDATE_ACCEPTANCE.md)：纯逻辑收口证据、任务矩阵和 Runtime handoff 前置条件；
- [D24 Runtime 验证延期清单](game/design/common/D24_RUNTIME_VALIDATION_BACKLOG.md)：后续实机、画面、输入、设备、性能和发行检查入口，当前仅为 Runtime Deferred；
- [D24 后续 Runtime 阶段交接](game/design/common/D24_NEXT_RUNTIME_PHASE_HANDOFF.md)：候选逻辑闭合后的有限 Runtime 首轮入口与停止规则，不构成 RuntimeApproved；
- [D25—D31 视觉与交互开发工作文档](game/VISUAL_DEVELOPMENT_WORK_PLAN_D25_D31.md)：画面、角色、船海天气、UI、视觉交互和质量收口的任务提示词与执行顺序；
- [D35 R03 表面材质与表现几何补充交接](game/design/common/handoffs/VISUAL-CLEAN/D35_R03_SURFACE_MATERIAL_HANDOFF.md)：R75 地表材质绑定、R64/R66 表现基线恢复和 D27 嵌套挂接边界；
- [D25—D31 视觉任务状态看板](game/VISUAL_TASK_STATUS_D25_D31.md)：43 个视觉 Runtime 任务的状态、证据目录和下游依赖；
- [D21 Runtime Deferred 登记](game/design/common/D21_RUNTIME_DEFERRED_REGISTER.md)：船体、驾驶、物理、海水表现、设备和发布阶段待测边界；
- [D16/D17 剩余门禁任务卡](game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATES.md) 与 [详细提示词](game/AGENT_DEVELOPMENT_CHAPTER_09B_D16_D17_GATE_PROMPTS.md)：47 个可复制分派的 `D16-C` / `D17-C` 任务；协议见 [GATE_TASK_PROTOCOL.md](game/GATE_TASK_PROTOCOL.md)，状态见 [GATE_TASK_STATUS.md](game/GATE_TASK_STATUS.md)；
- [技术架构](ARCHITECTURE.md)：三端、契约和目录规则；
- [开发规范](DEVELOPMENT_STANDARDS.md)：工程边界和验收要求；
- [D8 数值与枚举统一配置](game/design/common/D8_NUMERIC_ENUM_CONFIG.md)：统一单位、范围、枚举、所有权和配置检查入口；
- [D9 船员 Agent 垂直切片](game/design/common/D9_CREW_AGENT_VERTICAL_SLICE.md)：核心船员 Persona、记忆、提议、关系因果裁决和离线模板；
- [D10 船员岗位与行为状态](game/design/common/D10_CREW_BEHAVIOR_STATE.md)：轮班、岗位效率、疲劳健康、拒绝/怠工、请假调岗、离队和叛变前兆；
- [D10 同步集成验收报告](game/design/common/D10_ACCEPTANCE_REPORT.md)：工资、救援、偏袒、关系因果、行为、港口身份外观与读档一致性验收；
- [D11 钓鱼与海上采集](game/design/common/D11_FISHING_HARVESTING.md)：鱼种、深度、时间、潮汐、渔具、浅水贝类/海草、处理品、市场反馈与幂等读档；
- [D12 海图、测深与发现系统](game/design/common/D12_MAP_DISCOVERY.md)：海图分层、未知海岸、暗礁/浅滩/鱼场、工具、可信度、证据和存档边界；
- [D12 线索与寻宝任务](game/design/common/D12_TREASURE_CLUES.md)：多来源线索、可靠度、交叉验证、时间窗、装备前置、替代路线和奖励追溯；
- [D12 沉船、洞穴与打捞系统](game/design/common/D12_WRECK_CAVE_SALVAGE.md)：沉船/洞穴位置、潮汐、装备、风险、竞争者、法律裁决、失败恢复和奖励请求幂等；
- [D13 产权与资产交易](game/design/common/D13_PROPERTY_ASSET_MARKET.md)：岛屿、地块、房屋、仓库、工坊、酒馆、船坞、农地与渔场的调查、估值、权利交易、共同治理、抵押、继承和争议；
- [D15 植被批量目录](game/design/common/D15_VEGETATION_BATCH_CATALOG.md)：1,000—2,000 植物条目的分类、模型/材质占位状态、LOD、风场、生境、用途、来源/授权和抽样加载门禁；
- [D19 港口建筑样板](game/design/common/D19_PORT_ARCHITECTURE.md)：圣玛格丽特港口模块化 Prefab、稳定交互锚点、碰撞、LOD、材质复用和工作场景验证；
- [D19 圣玛格丽特正式场景验收](game/design/common/D19_SAINT_MARGARET_ACCEPTANCE.md)：InitialHarbor 串行集成、稳定交互 ID、兼容节点、固定镜头、PlayMode 回归、性能边界和回滚证据；
- [D13 金融、信用与保险](game/design/common/D13_FINANCE_CREDIT_INSURANCE.md)：现金、赊账、贷款、担保、利息、展期、保险证据、拒赔、违约、催收、查封与破产保护；
- [D13 农业、果园与蔬菜](game/design/common/D13_FARMING_ORCHARDS.md)：地块、种子、土壤、水源、季节、播种、病虫害、风暴、收获、轮作、加工与销售；
- [环境搭建](game/SETUP.md)：本机运行与测试；
- [契约目录](../contracts/)。

## 已完成基线

D1—D7 的范围、提示词和验收文件已经移入 [历史归档](archive/README.md)。它们用于追溯已完成事实，不再作为未来需求或开发入口。

## 设计领域

- [船舶](game/design/ship/README.md)
- [玩法](game/design/gameplay/README.md)
- [角色](game/design/character/README.md)
- [植被](game/design/vegetation/README.md)
- [动物](game/design/animals/README.md)
- [岛屿](game/design/islands/README.md)
- [环境](game/design/environment/README.md)
- [建筑](game/design/buildings/README.md)
- [道具](game/design/items/README.md)
- [经济与金融](game/design/economy/README.md)
- [势力、宗教与法律](game/design/factions/README.md)
- [战斗](game/design/combat/README.md)
- [任务与事件](game/design/quests/README.md)
- [UI](game/design/ui/README.md)
- [音频](game/design/audio/README.md)

## 归档原则

归档文件不可作为新需求的事实源。若归档内容与当前文档冲突，以 `REQUIREMENTS.md`、`design/` 和 `DEV_ROADMAP.md` 为准。

## 当前本地质量交接入口（2026-09-11）

- [Q03-A11 结果](game/tools/results/LOCAL-QUALITY/Q03-A11/RESULT.json)：D19-R03 船体视觉正向与 D2 航行轴对齐；真实 Player 路线已验证交互闭环，但状态仍为 `PARTIAL`。
- [Q03-A11 交接](game/design/common/handoffs/LOCAL-QUALITY/Q03-A11/Q03-A11.md)：变更、构建、截图、运行探针和正式边界。
- [VC-G01 主结果](game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)：最新 Q03-A11 证据已登记；`VC-G01=IN_PROGRESS`，`VC-G02/VC-G03=LOCKED`。
- [视觉清理执行计划](game/VISUAL_CLEANUP_EXECUTION_PLAN.md)：依赖顺序与下一张单一 owner 卡。

## 当前本地质量接管入口（2026-09-12）

- [Q00 当前接管结果](../game/tools/results/LOCAL-QUALITY/Q00/RESULT.json)：当前 source/build/runtime 关系与 headless 视觉边界。
- [Q01 当前根因诊断](../game/tools/results/LOCAL-QUALITY/Q01/RESULT.json)：root/scale/owner/tolerance 和四项缺陷边界。
- [Q02 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q02/Q02_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：动画 owner/lifecycle 与 D17 全量 PlayMode。
- [Q04 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：水域十循环在当前 D17 全量中的证据。
- [Q05 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：游泳 visualRoot/物理根分离与离水恢复证据。
- [Q06 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：跳跃落地与基础小步移动稳定性证据。
- [Q07 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：地形、码头、跳板活动碰撞与双向小步证据。
- [Q26-A16 R03 terrain owner 运行时复核](../game/tools/results/LOCAL-QUALITY/Q26/Q26_A16_TERRAIN_OWNER_RUNTIME_REVALIDATION_20260912_RESULT.json)：限定 R03 hierarchy 的 authored MeshCollider owner 与同名节点防回归，Q26 仍为 PARTIAL。
- [Q26-A17 生成舵轮高度对齐](../game/tools/results/LOCAL-QUALITY/Q26-A17/RESULT.json)：D19-R03 fallback 舵轮与双手中线误差由 `0.1744m` 降至 `0.0044m`，Q26 PlayMode `16/16 PASS`；不代表完整视觉门通过。
- [Q09 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：非单位船根舵位锚点、上下船循环和远距拒绝证据。
- [Q10—Q14 当前回归结果](../game/tools/results/LOCAL-QUALITY/STATUS.md)：当前 D17 全量中的航行甲板、下船、相机、输入焦点和船岸整合证据；各任务的独立 JSON 见对应 `Q10`—`Q14` 目录。
- [Q15 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：I/M/T 面板焦点、重复请求拒绝与 Gameplay 恢复证据。
- [Q16 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：交易 owner 桥接、一次性变化和拒绝反馈证据。
- [Q17 当前回归结果](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json)：船坞/整备成功、拒绝、幂等、取消和异常恢复证据。
- [Q03-A13 formal consumer](../game/tools/results/LOCAL-QUALITY/Q03-A13/RESULT.json) 与 [Q03-A14 formal Player build](../game/tools/results/LOCAL-QUALITY/Q03-A14/RESULT.json)：显式 formal 路径与原生运行阻塞边界。
- [Q03-A15 formal action admission gate](../game/tools/results/LOCAL-QUALITY/Q03-A15/RESULT.json)：15 个当前动作 clip 的 `humanMotion=0/genericRoot=15` 事实与保持 `BLOCKED_ASSET` 的回归门禁。
- [本地质量剩余计划](../game/LOCAL_QUALITY_REMAINING_PLAN.md)：Q00→Q35 依赖顺序、状态规则与下一张可执行卡。

## 2026-09-13 本地质量接管复核

- [Q00 当前接管与 Q03-A16 R2 对应结果](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A16_20260913_RESULT.json)：脏工作区、Pipeline 恢复、源码/Player/Data hash 关系和 GPU timeout 边界。
- [Q01 当前根因/尺度/owner 结果](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_20260913_RESULT.json)：InitialHarbor connected Pipeline 运行态表、缺陷根因边界与 Q02-Q20 白名单。
- [Q02 当前 Pipeline 回归结果](../game/tools/results/LOCAL-QUALITY/Q02/Q02_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)：D2 动画 `8/8` 与 D17 `16/16` 当前源码回归。
- [本次接管交接](game/design/common/handoffs/LOCAL-QUALITY/Q00.md)、[Q01 交接](game/design/common/handoffs/LOCAL-QUALITY/Q01.md)、[Q02 交接](game/design/common/handoffs/LOCAL-QUALITY/Q02/Q02.md)。下一张可执行卡为 Q04 当前水域 owner 回归。
- [Q04 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q04 交接](game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md)：D2 水域 owner `1/1 PASS`，下一张可执行卡为 Q05。
- [Q05 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q05 交接](game/design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)：whole-visual-root 游泳姿态契约 `1/1 PASS`，下一张可执行卡为 Q06。
- [Q06 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q06 交接](game/design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)：跳跃/接地/小步移动契约 `1/1 PASS`，下一张可执行卡为 Q07。
- [Q07 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q07 交接](game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)：地形/码头/跳板双向路线契约 `1/1 PASS`，下一步先登记 Q08 阻塞再进入 Q09。
- [Q08 当前依赖阻塞](../game/tools/results/LOCAL-QUALITY/Q08/RESULT.json) 与 [Q08 交接](game/design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)：Q03 formal authored action 仍未满足，下一张独立卡为 Q09。
- [Q09 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q09 交接](game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)：缩放锚点/登船生命周期/远距拒绝 `1/1 PASS`，下一张可执行卡为 Q10。
- [Q10 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q10 交接](game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)：航行中相对甲板稳定性 `1/1 PASS`，下一张可执行卡为 Q11。
- [Q11 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q11 交接](game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)：合法/无落点下船契约 `1/1 PASS`，下一张可执行卡为 Q12。
- [Q12 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q12 交接](game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)：相机模式/遮挡恢复契约 `1/1 PASS`，下一张可执行卡为 Q13。
- [Q13 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q13 交接](game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)：输入 owner/UI 导航隔离契约 `1/1 + 10/10 PASS`，下一张可执行卡为 Q14。
- [Q14 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q14 交接](game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)：船岸有界状态链 `1/1 PASS`，下一张可执行卡为 Q15。
- [Q15 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q15 交接](game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)：I/M/T 面板十轮互斥、重复请求拒绝与 Gameplay 恢复 `1/1 PASS`，下一张可执行卡为 Q16。
- [Q16 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q16 交接](game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)：交易 owner 未就绪、单 owner、买卖一次变化及余额/仓位拒绝 `1/1 PASS`，下一张可执行卡为 Q17。
- [Q17 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q17 交接](game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)：船坞/整备成功、拒绝、幂等、取消、异常恢复与主输入恢复 `1/1 PASS`，下一张可执行卡为 Q18。
- [Q18 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q18 交接](game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)：三分辨率 HUD 几何 `3/3 PASS`、D31 Visual Logic `36/36 PASS`、D29 Python `30/30 OK`，但原生视觉仍为 `PARTIAL`，下一张可执行卡为 Q19。
- [Q19 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q19 交接](game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)：离线船员提议/owner/关系/AgentMemory 恢复 `1/1 PASS`、Save.Tests `377/377 PASS`、AgentCore `0/0`，下一张可执行卡为 Q20。
- [Q20 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q20 交接](game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)：候选 bounds 与旧树隔离 `1/1 + 1/1 PASS`，但正式 catalog `BLOCKED / 0 / 0`，Q20 仍 `PARTIAL/BLOCKED_ASSET`，下一张可执行卡为 Q21。
- [Q21 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q21 交接](game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)：静态服务 footprint/空间组/碰撞 owner `1/1 PASS`，但原生 Player 路线/镜头仍开放，Q21 仍 `PARTIAL`，下一张可执行卡为 Q22。
- [Q22 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q22 交接](game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)：静态水线/船体吃水/D9-D28 天气 owner `1/1 PASS`，Q22 仍 `PARTIAL`，不覆盖 Player 游泳、船海镜头、音效、性能和最终视觉，下一张可执行卡为 Q23。
- [Q23 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q23 交接](game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)：InitialHarbor 正式 R03/船 Prefab 唯一集成和重开一致性 `1/1 PASS`，Q23 仍 `PARTIAL`，不覆盖 Player 视觉/输入/动作/游泳/船控/性能，下一张可执行卡为 Q24。
- [Q24 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q24 交接](game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)：陆地/出海存档及坏档保护 `3/3 PASS`，Q24 仍 `PARTIAL`，schema 无独立游泳状态且不覆盖 Player 视觉/输入，下一张可执行卡为 Q25。
- [Q25 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q25 交接](game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)：D30/D23 首航诊断 `2/2 PASS`、InitialHarbor 路线 `2/2 PASS`，Q25 仍 `PARTIAL`，下一张可执行卡为 Q26 测量基线。
- [Q25 当前 Pipeline 复核 Luna 2026-09-15](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)：当前源 D30/D23 首航 `2/2 PASS`、InitialHarbor PlayMode `2/2 PASS`；保留 D2/D27 警告、音频 fallback 和性能 pending 边界，不代表原生 Player 或最终视觉通过。
- [Q26 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json) 与 [Q26 交接](game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)：当前 D19-R03 缺陷 PlayMode `17/17 PASS`，独立 Q26A01 `1/1 PASS` 记录真实 W→游泳状态 marker；Q26 仍 `PARTIAL`，下一张可执行卡为 Q27 性能/预算证据。
- [Q27 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json) 与 [Q27 交接](game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)：D16 预算 `6/6 PASS`、连续窗口 `1/1 PASS`，当前 Null Device 为 `30.7857 FPS / 1103.0729ms` 最长帧且 GPU 不可用；Q27 保持 `PARTIAL`，下一张可执行卡为 Q28 新候选 Player 构建。
- [Q28 当前 R5 构建复核](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q28 交接](game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)：connected Unity Pipeline 生成 `StandaloneWindows64` R5 候选，`0` 构建错误、`9` 警告、哈希已登记；`enableInBuilds=false` 使 Pipeline 不进入 Player，Q28 仍 `PARTIAL`，下一张可执行卡为 Q29 隔离 Player 路线/输入证据。

## 当前本地质量接管入口（2026-09-13 Q29）

- [Q29 当前 R5 隔离 Player 结果](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)：connected Unity Pipeline 通过隐藏 Windows Player 完成 W/菜单互斥/E/Y/船控/Esc 的真实输入与状态证据。
- [Q29 R5 Pipeline console capture](../game/tools/results/LOCAL-QUALITY/Q29/Q29_RUNTIME_PIPELINE_QA_R5_PIPELINE_CONSOLE_CAPTURE.log)：运行 marker、owner 拒绝、错误查询和优雅退出记录；它不是 OS Player.log，也不包含截图。
- Q29 仍为 `PARTIAL`，VC-G01 仍为 `IN_PROGRESS`；下一张可执行卡是 Q30 当前源码独立证据审查，随后才是 Q31 最终 Player 视觉门禁。

## 当前本地质量接管入口（2026-09-13 Q31）

- [Q31 当前 Pipeline 复核](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json) 与 [Q31 交接](game/design/common/handoffs/LOCAL-QUALITY/Q31.md)：D2 水下可行走地形防护、9 项 D17 定向回归和 hidden Player RenderTexture/游泳位移证据；Q31 仍 `PARTIAL`。
- [VC-G01 主结果](../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json) 与 [VC-G01 交接](game/design/common/handoffs/VISUAL-CLEAN/VC-G01_HANDOFF.md)：D2 状态缺陷已做有界修复，但 formal action/avatar、自然泳姿、场景视觉和人工门禁仍未通过；VC-G01 仍 `IN_PROGRESS`。

## 2026-09-13 Q07 服务建筑碰撞 owner 修复

- [Q07 当前建筑碰撞结果](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_BUILDING_COLLISION_PIPELINE_20260913_RESULT.json) 与 [Q07 交接](game/design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)：定位并修复四栋可见服务建筑 `37 Renderer / 0 Collider` 的 Harbor owner 缺口。
- Q07 定向 `1/1 PASS`，D17 `16/16 PASS`，D19R03Q26 `16/16 PASS`；hidden Development Player `build_c3588a5c004b` 验证 4 个 D6-owned non-trigger collider 与 4 条 raycast，runtime error console=`0`，clean close。
- 该结果只扩展 Q07 的碰撞 owner 范围，不升级 Q31/VC-G01 的最终视觉状态：Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`。

## 2026-09-13 Q15-Q17 当前源码 Pipeline rerun2

- [Q15 rerun2](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)：D29/D16 I-M-T 面板十轮互斥、重复请求拒绝、Gameplay 恢复 `1/1 PASS`。
- [Q16 rerun2](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)：D29/D3 owner 未就绪、单 owner、买卖单变化和余额/仓位拒绝 `1/1 PASS`。
- [Q17 rerun2](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)：D29/D9/D4 成功、拒绝、幂等、取消、异常恢复和主输入恢复 `1/1 PASS`。
- 三项均为当前源码 Pipeline 证据，Editor=`ready/stopped`、Error Console=`0`；范围仍不含 native Player 截图、前台物理输入、最终视觉或性能。下一张可执行卡为 Q18。

## 2026-09-13 Q28 当前源码 R6 候选

- [Q28 R6 构建结果](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)：Q07 Harbor collision owner 改动后重新构建 `StandaloneWindows64` 候选，`build_d0507c23b205`，`0` errors、`1` warning，artifact hash 已登记。
- R6 保持 `developmentBuild=false / enableInBuilds=false / port=0`，因此没有把构建成功误报成 Player 控制面或视觉通过；Q28 仍 `PARTIAL`，下一张可执行卡为 Q29 R6 隔离 Player 路线与证据。

## 2026-09-13 Q29 当前源码 R6 隔离 Player rerun2

- [Q29 R6 结果](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json)：hidden Windows Player 通过 Unity Pipeline `7901` 完成当前源码输入/owner 路线；I/M/T、W 模态阻断、E、Y、航行中 M 拒绝、船 W/A、Esc 均有真实 runtime 证据。
- [Q29 R6 marker](../game/tools/results/LOCAL-QUALITY/Q29/Q29_RUNTIME_PIPELINE_QA_R6_PIPELINE_CONSOLE_CAPTURE.log)：包含被丢弃的错误 `press` 探索序列、正确 `down/wait/up` 路线、Q07 四 collider probe、error console 与 clean close。
- R6 仍不含截图、前台物理键盘、动作/泳姿/树木/穿模视觉或目标设备性能结论；Q29=`PARTIAL`、VC-G01=`IN_PROGRESS`，下一张可执行卡为 Q30 独立审查。

## 2026-09-13 Q30 current-source review rerun2

- Q30 当前复核结果：`65/65` 定向 Python、D16Accessibility `8/8`、D16InputRouter `10/10`、D17ActionQuality `16/16`、VcG01WorldLogic `14/14`；Q07 building collision `1/1`；Q15/Q16/Q17 各 `1/1`。
- Q28 R6 构建成功，Q29 R6 hidden Player 路线完成、错误查询 `0`、clean close；但没有有效 screenshot 或前台物理键盘证据。
- Q30 继续 `PARTIAL`，Q31 继续作为最终 native Player visual gate；Q03/Q20/Q27 和 VC-G01 的边界不变。

权威结果：`game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json`；命令捕获：`game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_PIPELINE_CAPTURE.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md`。

## 2026-09-13 Q31 当前源码 RenderTexture 诊断能力

Unity Pipeline hidden GUI Player 已可通过 `Camera.Render -> RenderTexture -> ReadPixels -> EncodeToPNG` 导出有效当前源码画面，不接管用户桌面。Q29 R6 的 `InitialHarbor`、开放水域静态和移动游泳帧均非黑并已登记 SHA256；传统 ScreenCapture 黑帧仍明确标记为无效。该能力只让低模/程序化泳姿和近墙遮挡可被诊断，不替代前台物理键盘、目标 GPU 性能或人工视觉验收。Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，下一步继续单一 presentation owner。

结果：[Q31 visual diagnostic](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_RESULT.json)；日志：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_PIPELINE_CAPTURE.log)。

## 2026-09-13 Q31 生成舵轮暖木对比复核

- [Q31 helm presentation result](../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_RESULT.json)：`HelmWheel` 暖木对比变更、Q26A18/回归、hidden Player 上船 probe、RenderTexture 帧与边界。
- [Q31 helm Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_PIPELINE_CAPTURE.log)：build、pointer/key、材质/父节点/手部状态、error console 和 clean close 的原始留痕。
- 本轮只证明舵轮表现语义的有限改善；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`，不提前解锁 VC-G02/VC-G03。

## 2026-09-13 Q00/Q01 after Q03-A17 current-source rebinding

- [Q00 current takeover rebinding](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A17_20260913_RESULT.json) 与 [Q01 current root-cause rebinding](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A17_20260913_RESULT.json)：同一 connected Unity Pipeline Editor、Q03-A17 exact Player/source hash、InitialHarbor 根/尺度/owner direct eval 与状态边界。
- 两张卡均为 `VERIFIED_WITH_SCOPE`，不替代前台物理键盘、compositor-visible 截图、自然动作/泳姿或最终视觉门禁；下一步为 Q04 当前 D2 水域 owner 回归。
- [Q04 current water owner rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A07 `1/1 PASS`，仅证明 D2 水域状态 owner；下一步为 Q05。
- [Q05 current swim pose contract rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A08 `1/1 PASS`，仅证明 whole-visual-root 倾斜/恢复与物理根隔离；下一步为 Q06。
- [Q06 current jump/grounded rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A09 `1/1 PASS`，仅证明基础跳跃/接地/小步移动；下一步为 Q07。
- [Q07 current collision owner/route rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：服务建筑 `1/1` 与 D17A10 `1/1 PASS`，仅证明测试范围内碰撞 owner/路线；Q08 仍受 Q03 阻塞，下一独立卡为 Q09。
- [Q08 current dependency block after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260913_Q03A17_R1_RESULT.json)：Q03 formal action/avatar admission 仍为 `BLOCKED_ASSET`，没有运行或伪造 Q08 整合 PASS；下一独立卡为 Q09。
- [Q09 current deck/boarding rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A11 `1/1 PASS`，覆盖缩放锚点、十次登/下船与远距拒绝；下一步为 Q10。
- [Q10 current ship-motion/deck rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A12 `1/1 PASS`，覆盖十个船舶快照下的相对甲板稳定；下一步为 Q11。
- [Q11 current safe-disembark rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A13 `1/1 PASS`，覆盖十次合法下船、无落点拒绝和恢复；下一步为 Q12。
- [Q12 current camera rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A14 `1/1 PASS`，覆盖相机模式、受控遮挡收缩和阻尼恢复；下一步为 Q13。
- [Q13 current input/UI isolation rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A15 `1/1`、D16 router `10/10 PASS`，验证自动化 W/S-HUD ownership；下一步为 Q14，前台物理键盘仍未证明。
- [Q14 current shore/water/deck integration rerun after Q03-A17](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)：D17A16 `1/1 PASS`，覆盖有界船岸状态链；下一步为 Q15，`imported_neutral` 仍不是正式动作通过。

## 2026-09-13 Q15 当前源码 Pipeline Q03-A17 R1

Q15 使用当前源码和同一连接的 Unity Pipeline Editor 重跑 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`，结果 `1/1 PASS`（测试 `12.4690213s`、Pipeline `12.58s`）。Console marker seq `21529` 记录仓库/市场/酒馆各十轮、临时页互斥、重复请求拒绝与 Gameplay 恢复；Editor=`ready/stopped`，error console=`0`。同时保留 D27 readiness wait 与 D2 procedural Player fallback warning 作为视觉/runtime debt。

本证据仍只覆盖 D29/D16 PlayMode owner 与焦点逻辑，不覆盖前台物理键鼠、native Player 截图、渲染面板、性能、Q03 动作准入或 VC-G01。结果：[Q15 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；下一步 Q16。

## 2026-09-13 Q16 当前源码 Pipeline Q03-A17 R1

Q16 使用当前源码和同一连接的 Unity Pipeline Editor 重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`，结果 `1/1 PASS`（测试 `1.9034709s`、Pipeline `2.02s`）。Console marker seq `21609` 记录 owner 未就绪、快速双击单 owner、买卖各一次变化、余额不足和满仓不变；Editor=`ready/stopped`，error console=`0`。

本证据仍只覆盖 D29/D3 交易 owner 桥接和拒绝反馈，不覆盖原生 Player 鼠标/截图、渲染面板、性能、完整首航、Q03 动作准入或 VC-G01。结果：[Q16 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q16 marker](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q17。

## 2026-09-13 Q17 当前源码 Pipeline Q03-A17 R1

Q17 使用当前源码和同一连接的 Unity Pipeline Editor 重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`，结果 `1/1 PASS`（测试 `2.6833445s`、Pipeline `2.79s`）。Console marker seq `21700` 记录整备/修船/船坞成功、拒绝、幂等、取消、异常恢复和主输入恢复；Editor=`ready/stopped`，error console=`0`。预期的空 owner 异常路径只留下 Warning，测试确认状态不变并恢复。

本证据仍只覆盖 D29/D9/D4 业务 owner 桥接和恢复，不覆盖原生 Player 键鼠/截图、最终视觉、性能、完整首航、Q03 动作准入或 VC-G01。结果：[Q17 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q17 marker](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q18。

## 2026-09-13 Q18 当前源码 Pipeline Q03-A17 R1

Q18 使用当前源码完成 `D31Q18HudResolutionTests` `3/3 PASS`、`WhalesIslandReef.D31.VisualLogic.Tests` `36/36 PASS`，以及项目根 Python `tools.tests.test_d29_interaction_hud` `30/30 OK`；Editor=`ready/stopped`，error console=`0`。

本证据仍只覆盖代码级 HUD 分辨率/布局、D31 visual-stage 逻辑和 D29 静态回归，不覆盖原生 Player 截图、字体栅格化、英文最长文案、物理键鼠、性能或最终视觉。Q18 继续 `PARTIAL`。结果：[Q18 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；下一步 Q19。

## 2026-09-13 Q19 当前源码 Pipeline Q03-A17 R1

Q19 使用当前源码完成船员交互审计 `1/1 PASS`、`Save.Tests` 全程序集 `377/377 PASS`，并完成 AgentCore `dotnet build` `0 warnings / 0 errors`；Editor=`ready/stopped`，error console=`0`。

本证据只覆盖离线船员提议→D10 owner 裁决、关系/行为事实和 AgentMemory/domain SQLite 恢复，不覆盖在线 LLM、原生 Player 对话 UI/输入/视觉、动作/游泳/碰撞、性能或正式视觉门。Q19 继续 `VERIFIED`。结果：[Q19 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；下一步 Q20。

## 2026-09-13 Q20 当前源码 Pipeline Q03-A17 R1

Q20 当前源码完成候选资源 EditMode `1/1 PASS` 与 R03 旧树/植被接地 PlayMode `1/1 PASS`；marker 为 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`。D18 catalog 仍 `BLOCKED / 0 / 0`，builder=`RenderAuthoredPalms=false`，正式 Prefab 棕榈引用=`0`，因此 Q20 保持 `PARTIAL/BLOCKED_ASSET`。

本轮 PlayMode 还真实捕获 FMOD 输出设备 Error seq `21805`，已记录为环境边界；`clear_console` 已发出但不能抹除已捕获事实。结果：[Q20 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q20 marker](../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q21 当前港口空间审计。

## 2026-09-13 Q21 当前源码 Pipeline Q03-A17 R1

Q21 当前源码重跑 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` 为 `1/1 PASS`（测试 `0.9323411s`、Pipeline `0.97s`）；marker 确认四个服务建筑各 `37 Renderer / 0 missing`、六个空间组、23 个碰撞体（1 R03 terrain / 22 D27 owner）和无 pairwise XZ 重叠。以 Console seq `21868` 为基线，本轮新增 Error 为 `0`；Q20 FMOD seq `21805` 作为历史环境边界保留。

Q21 继续 `PARTIAL`：本轮只证明静态 R03 空间/owner 合同，不覆盖 native Player 路线、镜头遮挡、键鼠、植被准入、性能或最终视觉。结果：[Q21 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q21 marker](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q22 当前船海天气审计。

## 2026-09-13 Q22 当前源码 Pipeline Q03-A17 R1

Q22 当前源码重跑船海天气 owner 审计为 `1/1 PASS`（测试 `0.1840829s`、Pipeline `0.19s`）；水面 `96×96`、sea level `-0.0397`、泡沫 `28`、水材质缺失 `0`、船壳底部 `-0.0400`、可见船体 `min.y=0.0600`，D9/D28 Calm-Rain-Storm 指标均重新取证。以 Console seq `21878` 为基线，新增 Error/Warning=`0`，Q20 FMOD seq `21805` 作为历史环境边界保留。

Q22 继续 `PARTIAL`：静态合同不覆盖 native Player 游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉。结果：[Q22 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q22 marker](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q23 当前正式资源集成审计。

## 2026-09-13 Q23 当前源码 Pipeline Q03-A17 R1

Q23 当前源码重跑 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently` 为 `1/1 PASS`（测试 `0.3876112s`、Pipeline `0.39s`）；active roots=`34`、R03/船实例=`1/1`、R03 Renderer=`361`、缺失材质/脚本=`0`、terrain collider=`1`、船体底部 `-0.0400`，GUID 解析和重开一致性均通过。以 Console seq `21890` 为基线，新增 Error/Warning=`0`，Q20 FMOD seq `21805` 作为历史环境边界保留。

Q23 继续 `PARTIAL`：正式场景集成合同不覆盖 native Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉。结果：[Q23 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q23 marker](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q24 当前存档恢复诊断。

## 2026-09-13 Q24 当前源码 Pipeline Q03-A17 R1

Q24 当前源码重跑 `D19Q24SaveRestoreDiagnosticTests` 为 `3/3 PASS`（Pipeline `1.28s`）；陆地往返、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与 `D2_PlayerSpawnAnchor` 安全恢复、以及 `after_player` 故障后的上一份好档保护均通过。以 Console seq `21900` 为基线，新增 Error/Warning=`0`，Q20 FMOD seq `21805` 作为历史环境边界保留。

Q24 继续 `PARTIAL`：当前 `PlayerState` 没有独立游泳姿态/体积/动画字段，本卡不覆盖 native Player 输入、镜头、动作、游泳、船控、树木、音效、性能或最终视觉。结果：[Q24 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q24 marker](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q25 当前首航/循环集成诊断。

## 2026-09-13 Q25 当前源码 Pipeline Q03-A17 R1

Q25 当前源码重跑 D30/D23 首航 EditMode=`2/2 PASS`（Pipeline `0.49s`）和 InitialHarbor route PlayMode=`2/2 PASS`（Pipeline `5.42s`，测试 `3.2954252s/1.9745871s`）；覆盖七阶段投影、40 tick 首航/返港/保存及交易关闭/保存、出航/推进/返港。以 Console seq `21911` 为基线无新增 Error，但有 12 条真实 D27/D2 Warning，D31 runtime performance approval 仍 pending。

Q25 继续 `PARTIAL`：owner 链和有界路线不覆盖完整人工输入、相机、动作/游泳/上船视觉、树木、音效或性能。结果：[Q25 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q25 marker](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；下一步 Q26 当前测量基线。

## 2026-09-13 当前源码 Pipeline 交互诊断留痕

Q31 当前 Pipeline 诊断已补充 `InitialHarbor` 运行态、仓库 modal 与 W 输入隔离证据，并新增 D16 输入回归：目标测试 `1/1 PASS`，`D16InputRouterPlayModeTests` `11/11 PASS`。RenderTexture 诊断帧仍显示 procedural fallback，不能替代前台物理输入或正式视觉验收；Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`。结果：[Q31 R1](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_HUD_VISUAL_DIAGNOSTIC_20260913_R1_RESULT.json)；交接：[Q31](game/design/common/handoffs/LOCAL-QUALITY/Q31/Q31.md)。

## 2026-09-13 Q26-A19 D19 舵轮支架材质语义修复

通过 Unity Pipeline 验证 D19-R03 候选/生成舵轮支架的木质材质 owner：修复前定向测试实际 `0/1 FAIL`（`R03_Stone`），修复后 `Q26A19=1/1 PASS`，完整 Q26=`18/18 PASS`，hidden Development Player 读到 `D7Polish_HelmStand_0 / brown_planks_03_albedo`。结果：[Q26-A19](../game/tools/results/LOCAL-QUALITY/Q26-A19/RESULT.json)；交接：[Q26-A19](game/design/common/handoffs/LOCAL-QUALITY/Q26-A19/Q26-A19.md)。Q26/Q31/VC-G01 仍分别为 `PARTIAL`/`PARTIAL`/`IN_PROGRESS`。

## 2026-09-13 Q26-A21 默认程序化泳姿连续性

新增 Q26-A21 回归并用 Unity Pipeline hidden Player 复核开放水域：默认 Player=`Procedural/Swimming`，72 帧测得 `maxArmDelta=6.47°`、`maxFootDelta=1.94°`；Pipeline W 从 `(0.00,0.40,30.00)` 移到 `(0.45,0.40,29.87)`。定向 `1/1 PASS`，完整 Q26=`19/19 PASS`。这只证明 procedural fallback 的连续相位和水中移动，不证明自然 authored swim 或最终视觉门禁。结果：[Q26-A21](../game/tools/results/LOCAL-QUALITY/Q26-A21/RESULT.json)；交接：[Q26-A21](game/design/common/handoffs/LOCAL-QUALITY/Q26-A21/Q26-A21.md)。

## 2026-09-15 Local Quality resumed on current source

恢复执行后，Q00/Q01/Q09 已重新绑定到当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63`；旧 Q08 安全暂停证据保持历史不可变。Q00 current-source exact Player build、Q01 根因/owner 对账和 Q09 当前甲板锚点回归均已登记：

- [Q00 current-source result](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_SOURCE_REBIND_Q03A23_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q00.md)
- [Q01 current root-cause result](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A23_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q01.md)
- [Q09 current pipeline result](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)

三张卡均保持有界状态，未把 Pipeline/hidden Player 证据升级为前台输入、最终视觉或 D31 整体 PASS；下一张为 Q10。

Q10 已继续完成当前源航行中甲板关系回归：[Q10 result](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。D17A12=`1/1 PASS`，十个船舶运动快照下玩家相对船根 transform 稳定，并恢复安全下船；下一张为 Q11，仍不等价于原生 Player 视觉或人工输入验收。

Q11 已继续完成当前源安全下船回归：[Q11 result](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。D17A13=`1/1 PASS`，十次合法下船、无落点拒绝和状态恢复通过；下一张为 Q12。

Q12 已继续完成当前源相机回归：[Q12 result](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。D17A14=`1/1 PASS`，十次模式切换、受控遮挡收缩与恢复通过；下一张为 Q13。

Q13 已继续完成当前源输入/UI 回归：[Q13 result](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。D17A15=`1/1`、D16InputRouter=`11/11`、D16PrimaryHudNavigation=`2/2`；下一张为 Q14，前台 Win32 输入仍未由 Pipeline 测试替代。

Q14 已继续完成当前源岸水船整合回归：[Q14 result](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。D17A16=`1/1 PASS`，有界 shore→water→shore 与登船/转向/拒绝/合法下船链通过；下一张为 Q15。

Q15 已继续完成当前源面板焦点回归：[Q15 result](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。D29Q15=`1/1 PASS`，I/M/T owner、三类面板十轮互斥、重复拒绝和 Gameplay 恢复通过；下一张为 Q16。

Q16 已继续完成当前源交易桥回归：[Q16 result](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。D29Q16=`1/1 PASS`，成功、拒绝、忙碌/重复和一次请求一次变化通过；下一张为 Q17。

Q17 已继续完成当前源船坞/整备回归：[Q17 result](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。D29Q17=`1/1 PASS`，成功、拒绝、重复、取消、异常恢复和主输入恢复通过；下一张为 Q18。

Q18 已继续完成当前源 HUD/visual-logic 回归：[Q18 result](../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。分辨率矩阵 `3/3`、D31 VisualLogic `36/36`、Python HUD `30/30`；Q18 仍为 `PARTIAL`，下一张为 Q19。

Q19 已继续完成当前源船员/存档回归：[Q19 result](../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [log](../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。船员交互审计 `1/1`、Save.Tests `377/377`、AgentCore `0/0`；Q19 仍为离线 owner/save 范围内 `VERIFIED`，下一张为 Q20。

Q20 已继续完成当前源候选植被/旧 Renderer 隔离回归：[Q20 result](../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [log](../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。候选 EditMode `1/1`、R03 接地/隔离 PlayMode `1/1`；D18 仍 `BLOCKED / 0 / 0`，Q20 保持 `PARTIAL/BLOCKED_ASSET`，下一张为 Q21。

Q21 已继续完成当前源港口空间回归：[Q21 result](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [log](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。静态服务空间 `1/1`，Q21 仍为 `PARTIAL`，下一张为 Q22。

Q22 已继续完成当前源船海天气回归：[Q22 result](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [log](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。静态 sea/ship/weather 合同 `1/1`，Q22 仍为 `PARTIAL`，下一张为 Q23。

Q23 已继续完成当前源正式场景集成回归：[Q23 result](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json) / [log](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log) / [handoff](game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。InitialHarbor 集成/重开 `1/1`，Q23 仍为 `PARTIAL`，下一张为 Q24。

Q24→Q26 已继续完成当前源码有界质量链：[Q24 safe pause](game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915.md)、[Q25 result](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)、[Q26 R5 result](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5_RESULT.json)、[Q26 R5 log](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5.log)、[Q26 handoff](game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。Q26 全量新程序集 `21/21 PASS`，但仍为 `PARTIAL`；Q26A22 的测试时钟隔离和 stale-assembly 诊断已留痕，下一张执行卡为 `Q27_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 Q27 current-source performance revalidation R4

Q27 在 Q26 R5 后重新绑定到当前 HEAD，D16 预算 EditMode=`6/6 PASS`，60 秒 headless PlayMode=`1/1 PASS`；报告为 `Null Device`，`162` samples、平均 `30.9174 FPS`、最长帧 `1190.5581ms`、GPU timing unavailable，探针 owner/预算合同为真实 `true`。Console 取证为 `Error=0 / Warning=192`，已知 D27/D2 fallback 家族已登记，随后清空；Q27 继续 `PARTIAL`，不能替代目标设备性能或原生 Player/VC-G01 视觉输入门。结果：[Q27 R4](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5_RESULT.json)；日志：[Q27 R4 log](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5.log)；交接：[Q27](game/design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。下一步 Q28 当前源码候选 Player 构建。

## 2026-09-15 Q28 current-source candidate R13 build and D17 guard

Q28 在 Q27 R4 后生成当前源码 R13：`build_73c3a973e2d8` 为 `StandaloneWindows64 / Succeeded / 0 errors / 1 warning`，唯一 warning 是 `enableInBuilds=false`，候选不带 Player Runtime Pipeline。D17A08=`1/1 PASS`，10 周期 rootAngle=`64°`、rootOffsetY=`0.012..0.034`、手头对齐=`0.992..1.000`；Q28 继续 `PARTIAL`，因为本轮未启动 Player、未发物理输入、未采集 compositor 截图或测量 GPU/目标设备性能。结果：[Q28 R13](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4_RESULT.json)；日志：[Q28 R13 log](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4.log)；交接：[Q28](game/design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。下一步 Q29 当前 Player 路线复核。

## 2026-09-15 Q29 R11 current-source entry smoke and temporary safe pause

Q29 R11 从当前 HEAD 构建 Pipeline-enabled hidden Windows Player，dry-run 有效，`build_fb5d1ad58690`=`StandaloneWindows64 / Succeeded / 0 errors / 7 warnings / 243870645 bytes / 34076ms`。通过真实 Pipeline pointer 完成 MainMenu NewGame/Confirm，进入 `InitialHarbor` build index=`2` 后按用户要求暂停；未把未执行的完整输入路线或画面结论记为 PASS。`quit` 后 PID/descriptor 消失，Runtime error=`0`，Player 严格错误词=`0`，临时 Runtime/Build 设置已恢复，Editor=`ready/stopped`。

Q29 父任务继续 `PARTIAL`、VC-G01 继续 `IN_PROGRESS`、VC-G02/VC-G03 继续 `LOCKED`、D31-P04/S05/G06 继续 `PARTIAL`。暂停交接：[PAUSE_20260915_Q29](game/design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915_Q29.md)；结果：[Q29 R11 paused result](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED_RESULT.json)；日志：[Q29 R11 paused log](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED.log)。恢复入口为 Q30 当前源码独立审查，再进入 Q31 最终 native Player visual gate。
