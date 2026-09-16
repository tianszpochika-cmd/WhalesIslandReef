# 本地高质量候选版剩余计划（Luna 执行版）

生效：2026-09-08。用户授权：暂停原执行 Agent，由本次规划拆分剩余工作；后续仍由 Luna 主开发，要求动作顺畅、无明显 Bug、无明显穿模。本文是 D25—D31 / VISUAL-CLEAN 的执行细分，不更改产品愿景、系统所有权、既有验收门禁，不表示新建 D32+ 产品阶段。

## 1. 交付目标与范围

交付一份可定位到源码/资源版本的 Windows 非开发版 Player：从新游戏进入港口，完成步行、游泳及上岸、登船/航行/下船、港口经营、船员交互、出航返港、保存退出和继续游戏；主路线无明显穿模、悬空、扭曲、输入失效、重复扣款或存档丢失。画面遵循既有美术方向，不用隐藏必需内容来冒充完成。

这是高质量**本地切片**，不是全部 50 岛、22 类玩法或 Steam 正式版完成。D31 仍是 Visual Candidate；正式发布门禁单列。任务末尾安排完整需求覆盖审计，按实际缺口生成后续任务，不降低完整愿景。

## 2. 暂停快照（只读核对，不是本次重新测试）

- 执行任务「最新主任务」`01a072d5-a84e-7a83-a0ea-061057f26af4` 已 idle，最新轮 interrupted；未向它发消息或启动开发。
- HEAD `01024cae`，2026-09-02 `Complete D18-D24 pure logic implementation`。规划开始前 Git porcelain：398 个已跟踪修改、4712 个未跟踪条目；不是文件内容完成率。
- D18—D24 是逻辑基线。D25—D31 正式表仍有场景/运行门禁 PARTIAL，VC-G01 IN_PROGRESS，VC-G02/G03 LOCKED。事实源仍为原任务 RESULT.json，不能按本计划任务计数升级。
- 9 月 6 日正式记录：597/597 EditMode、部分路线/拒绝下船 PlayMode 通过；这些不覆盖后续修改。
- 9 月 7 日结果：`VC_G01_WORLD_LOGIC_POST_PALM_DISABLED.xml` 12/12；`VC_G01_D17_SWIMMING_RERUN58_R3.xml` 和 `VC_G01_D17_SHORELINE_RERUN58_R3.xml` 各 1/1。它们早于 9 月 8 日 00:10 的动作源码修改，不能证明最后修改完成。
- `D2CharacterAnimationController.cs` 已有 Swimming/ApplySwimmingPose；正式 9 月 6 日 JSON 仍写游泳未实现，存在状态滞后，需 Q00/Q01 对齐。
- `D19R03HarborPresentationBuilder.cs` 的 `RenderAuthoredPalms=false`：悬空棕榈目前是禁用处理，恢复正确资源尚欠账。
- 最近可见执行证据提到 RERUN58 构建/路线，不能直接认定它包含最后动作补丁。必须重新建立构建来源关系。

2026-09-09 当前接管复核：Q00 已用 Q21-A05/R27 替换旧的 RERUN58 窗口激活阻塞事实。R27 的真实 Win32 路由证明当前 Player 可进入 InitialHarbor 并观察到移动、游泳、船控和交互矩阵，但 `19` 张截图仍显示动作/游泳/握舵/植被视觉缺陷，因此不升级 VC-G01。Q01 同步确认 Q02/Q02-A 已消除测试路径上的 D7 visualRoot 竞争写入；Q03 合法动作资源阻塞保持不变。后续以单一 owner 继续推进表现修复，Q31 仍是最终视觉门禁。

2026-09-11 Q29 R4 UI 导航复核：在真实 Win32 Player 运行中，78 个 gameplay W/A/S/D 样本均确认 EventSystem 存在但 `sendNavigationEvents=false`、UI `move` 未绑定、当前选中控件为空，0 次 UI 导航冲突事件。该结果关闭的是当前候选的输入冲突复现缺口，不是视觉 PASS；R4 截图仍显示低模 fallback、泳姿/握舵不足和港口几何/接地风险。Q29/Q31/VC-G01 状态保持 `PARTIAL/PARTIAL/IN_PROGRESS`。

2026-09-09 Q05-A01/R28 复核：R28 已成为当前原生 Player 候选。它保留真实 Win32 输入和 Q20/Q21 边界，Q05-A01 的有限 fallback 游泳摆幅/roll 修正通过定向与全量回归，但 `19` 张截图仍未达到正式动作、游泳、握舵、植被和无漂浮/穿模要求；因此 Q03/Q20 资源门禁、Q31 `WAITING_VISUAL` 和 VC-G01 `IN_PROGRESS` 不变。下一步优先处理合法动作/Avatar 适配或明确 D20 candidate-only 视觉集成。

证据入口：

- [VC-G01 结果](../../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RESULT.json)
- [现有清理计划](VISUAL_CLEANUP_EXECUTION_PLAN.md)
- [D25—D31 正式状态](VISUAL_TASK_STATUS_D25_D31.md)
- [完整需求](REQUIREMENTS.md)、[边界](design/DESIGN_BOUNDARIES.md)、[D16 延期说明](design/common/D16_EXTERNAL_HARDWARE_DEFERRED.md)

## 3. Luna 执行协议

1. 初次接管遵守 AGENTS/CONTEXT 必读项；后续每张卡只加载本文通用规则、该卡、直接依赖交接、对应源码/设计。不要每轮重读全部历史 RERUN。
2. 默认单 Agent 串行；一张卡一个产品模块，测试及对应文档可随同修改。跨模块根因先登记接口/所有权，再建 `Qxx-a/b` 子卡串行处理。场景/Prefab 正式集成只由 Q23 写者执行；业务系统不能由表现层直接改数值。
3. 所有卡初始 PLANNED。状态：PLANNED → ACTIVE → IMPLEMENTED → VERIFIED；诊断/审查卡可直接 VERIFIED。VERIFIED 仅表示卡内证据完整，不表示旧 G 门禁通过。视觉卡自动化通过、人工未测时为 WAITING_VISUAL；外部依赖用 BLOCKED 并写解除条件。
4. 开卡先写现象、根因证据/待证假设、允许文件、验证方式。诊断任务不强制改代码；实现卡若已满足，复核后直接留证，不为凑任务重写。
5. 同一假设最多两轮修改验证；无改善就停止该分支，整理坐标层级/对象身份/输入来源等证据，提出不同假设。可做独立任务；所有剩余路径都受阻时汇报，不无限等待、不偷偷删功能。
6. 参数从既有配置/模型尺度确定，不散落新魔法数。先记录测量方法和阈值再修复；不能因失败放宽阈值。正式设计冲突单列，不擅自迁移技术栈。
7. 针对当前变化运行定向编译/行为测试；Q08/Q14/Q25/Q30 是整合检查点。无需每张卡跑所有 Python/Unity 测试。源码字符串存在、JSON 可解析、退出码 0 均不能独立证明玩法正确。
8. Unity 测试沿用已经成功的命令模板，先确认实际 asmdef/filter；本项目已有 `-quit` 导致无 XML 的历史，未确认兼容前测试命令不加 `-quit`。完成以当前 run 的 XML 非零用例、失败数、日志和进程结果交叉判断；无 XML/零用例不能 PASS。不以固定 20 秒结束长任务；观察日志/进程是否前进，并定期更新。
9. 不 reset/clean/自动 stash，不覆盖旧 build/log，不读密钥。证据使用独立 run_id；源码变动后仅失效受影响证据，但最终候选必须有同一构建的完整路线与视觉记录。
10. 本文允许预验收截图/脚本输入诊断；正式 VC-G01-NATIVE 仍遵守原物理输入/人工视觉门槛。缺原生控制面时准备一次集中人工验收包，不重复探测驱动，不伪造人工结果。WAITING_VISUAL 不阻止无依赖的代码任务，但阻止正式门禁升级。
11. 一个小任务完成后留下短交接并继续可执行的下一张卡，不要求用户逐卡发“继续”。新付费资产、外部账号/发布权限、核心需求变更不在本计划默认授权内。

## 2026-09-16 Q31 R13 临时暂停快照

按当前收口要求，开发在 Q31 R13 的真实 Player 交互证据后暂时暂停。R12/R13 已分别留下游泳和登船/船控结果；R13 的 D17A02/D17A03 均为 `1/1 PASS`，Player clean quit 后进程/descriptor 消失，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。这只是可恢复的工程检查点，不是高质量候选完成或 VC-G01 通过。

- 当前状态：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`。
- 未关闭门禁：正式 action/avatar、自然 swim/helm IK、植被接地/穿模、前台物理输入、目标设备性能、音频和人工最终视觉。
- 工作区策略：保留所有既有跟踪修改和未跟踪历史证据，不执行清理/reset/checkout/覆盖。
- 恢复入口：`Q31 R14`；先选一个单一 owner 缺陷并建立 focused regression，再做当前源码 native Player 复验。VC-G01 未关闭前不得进入 VC-G02/G03。

详细交接：[PAUSE_20260916_Q31_R13](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260916_Q31_R13.md)；机器结果：[Q31 pause result](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_R13_RESULT.json)。

## 4. 统一质量门槛（本批新增验收要求）

不承诺数学意义零 Bug；承诺指定覆盖范围无已知 P0/P1 和明显可见 P2。P0=崩溃/坏档/经济数据破坏；P1=主线卡死、穿过实体/掉出地图、控制丢失、错误登船、严重动作扭曲；P2=明显滑步/跳姿态/悬浮、镜头穿墙、遮挡关键 UI、音画反馈错误。其他轻微缺陷也必须列出，不隐瞒。

| 质量项 | 必须验证的动作与门槛 |
|---|---|
| 角色动作 | idle→走→停、转向、倒退、跑/跳（已支持时）、入水→游泳→上岸、登/下船。正常速度观察无 T pose、骨骼翻折、明显瞬移和持续滑步；每个转换至少 10 次，记录速度/状态与短连续画面，不只单帧截图。 |
| 地面与实体 | 平地/斜坡/坡顶、建筑墙角、码头缝、跳板、船舷，每类至少 3 处代表点，记录具体坐标；正撞/斜撞/沿边移动，每条往返 10 次，无进入实体、掉落或卡死。可见地面与碰撞一致。 |
| 测量容差 | 接触点/胶囊 skinWidth 导致的数值接触不等于穿模。Q01 按实际单位、胶囊半径、skinWidth 和模型足底建立合理容差表；记录持续时间和穿透深度。物理查询与正常镜头画面双证据，不能只凭 bounds 重叠宣判。 |
| 游泳 | 浅/深水边界各进出 10 次，不频繁抖动切态、不水面行走；头/躯干姿态合理，上岸恢复地面控制与动画。不能以加空气墙代替游泳；不新增深海潜水。 |
| 船与玩家 | 静止、加减速、转弯、已支持海况下甲板关系稳定，无穿甲板/船体/瞬移；登下船各 10 次，远离/无落点拒绝不改变所属状态。 |
| 镜头 | 墙边、船舷、转角、角色换模式时无穿墙透视、卡内模或高频来回收缩；切模式能看清玩家和前方。 |
| UI/业务 | 打开/取消/重复/条件不足/owner 未就绪/关闭恢复；一笔成功操作只发生一次业务变更。失败反馈与 owner 结果一致，临时页关闭后输入恢复。 |
| 存档 | 新档、正常存取、退出 Continue、兼容样例/损坏样例（在隔离测试目录）均有结果。财物、船员、世界时间和合法位置一致，无重复扣款或丢失；不破坏用户真存档。 |
| 性能 | 按既有 D31/D16 更严格预算。新增本机候选底线：1080p 中档代表路线预热后采样 ≥180 秒，平均 ≥60FPS、1% low ≥45FPS，非加载阶段无重复 >100ms 卡顿。记录低/中/高三档实测，不要求三档都自动等价 PASS；帧时间原始样本、硬件、构建、路线/天气固定。1% low=1000/最慢 1% 帧的平均毫秒数。 |
| 稳定性 | 定向 10 次转换通过后，同一候选连续 30 分钟完整游玩及一次退出读档；无新增未处理异常、输入死锁或明显内存持续增长。长跑失败针对原因修，不重跑无关全仓检查。 |

这些是本机新增最低门槛，不替代目标机/跨平台/真机手柄和发布验收。未达性能门槛先分析，不能降低画面到不可读来换 FPS。

## 5. 任务卡读取入口与共同交付

路径均相对仓库根。表中 owner 是唯一产品写入范围；“只读”卡仅写结果/文档。相关文件移名时先 rg 定位真实类型，禁止凭名字新造替代实现。

| 缩写 | 首读源码/证据 |
|---|---|
| MOVE | `game/Assets/Scripts/Gameplay/D2/` 中实际角色移动 owner；先搜 IsSwimming/CharacterController；`game/Assets/Scripts/Gameplay/UI/D16/D16InputRouter.cs` 只读接口 |
| ANIM | `game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs`、`game/Assets/Scripts/Gameplay/D7/D7CharacterPresentationController.cs`（只读调用方）；对应 rig/Avatar 配置 |
| WORLD | `game/Assets/Scripts/Gameplay/Harbor/HarborCollisionController.cs`；`game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs` 只读几何来源 |
| DECK | `game/Assets/Scripts/Gameplay/D7/D7ShipDeckController.cs`、`D7ControlCoordinator.cs`；D2ShipController 只读 |
| CAM | `game/Assets/Scripts/Gameplay/CameraFollow.cs` |
| UI | `game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs`、`D29/Logic/`；现有 `docs/game/design/common/D29_D30_INTERACTION_BUTTON_MATRIX.md` |
| LOOP | `game/Assets/Scripts/Gameplay/D30/D30VisualLoopCoordinator.cs`；D7SaveCoordinator 和各业务 owner 只读 |
| ART | `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs`、当前资源准入清单、对应 D25—D28 任务卡 |

每张卡交付：`game/tools/results/LOCAL-QUALITY/Qxx/RESULT.json`，以及 `docs/game/design/common/handoffs/LOCAL-QUALITY/Qxx.md`；更新当日 `.workbuddy/memory/YYYY-MM-DD.md`，新增文档登记 docs/README.md。建议 RESULT 字段：task_id/status/parent_task/depends_on/source_head/dirty_source_manifest/build_id/run_id/changed_files/checks（命令、时间、结果、证据路径）/visual_review/remaining_defects/next_task。没运行的项明确 NOT_RUN；不要生成虚假占位 PASS。

维护一个 `game/tools/results/LOCAL-QUALITY/STATUS.md` 简短看板，从各 RESULT 派生；原 Dxx/VC 状态仍各自权威。每张卡先记 ACTIVE，完成记依据。源文件+依赖资源哈希清单用于脏工作区构建识别，单独 HEAD 不够。不要扫描/哈希整个 Library。

## 6. 剩余 36 张任务卡

所有任务初始 PLANNED。下列“前置”指技术依赖；视觉最终放行集中在 Q31。执行顺序为表序；有明确独立分支可顺延，不跨正式门禁。

### A. 接管与动作（Q00—Q08，对应 VC-G01、D27）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q00 暂停点复原 / 无 | 只读 / 本文 §2、VC-G01 RESULT、最后动作 diff | 列最近代码、Prefab、Player、测试时间和覆盖关系；给出最后补丁是否编译/测试的证据，列缺口。登记活动进程，不能启动第二个同工程 Unity。输出当前事实快照，禁止把历史 PASS 直接继承。 |
| Q01 根因与尺度表 / Q00 | 只读诊断 / MOVE、ANIM、DECK、WORLD | 画出实际角色根/visualRoot/骨骼/胶囊/甲板/岛面父子与世界变换表；记录各状态写者、单位和碰撞容差。复现四类已报缺陷，给证据或标未复现；形成 Q02—Q13 精确文件白名单。无需代码变更。 |
| Q02 动画唯一写者 / Q01 | D2 动画 / ANIM | 确认 Animator、程序化姿态、visualRoot 的所有权；修最后未验证补丁涉及的竞争写入/累计旋转。10 次 enable/disable 与状态切换无姿态累积，编译+相关测试通过。若需改 D7 消费者先拆子卡。 |
| Q03 地面动作混合 / Q02 | D2 动画 / ANIM | 实际速度驱动 idle/走/跑/转向，平滑进入退出，停止无持续摆腿；先确认模型可用动画能力，缺合法适配资源记 BLOCKED_ASSET，不能声称 neutral pose 即高质量完成。连续序列满足动作门槛。 |
| Q04 游泳状态与水位 / Q01 | D2 移动 / MOVE | 复核已有 IsSwimming，修水位/浅深水判定与边界抖动；以真实水位接口为准，记录迟滞与合法岸边恢复。浅深水各 10 次，状态和速度正确；不改动画骨骼。 |
| Q05 游泳姿态 / Q02,Q04 | D2 动画 / ANIM | 核对暂停前补丁，修水平朝向/头躯干/四肢节奏，禁止通过旋转碰撞胶囊来摆姿态。停止游动与上岸恢复初始局部变换；侧前后连续画面无直立水上行走。 |
| Q06 行走跳跃碰撞 / Q01 | D2 移动 / MOVE | 检查速度、deltaTime、碰撞 sweep/CharacterController、地面判定；正/斜撞墙及已支持跳跃 10 次无穿越、落地弹跳/卡死；不要删除合法操作避开失败。 |
| Q07 岸线与跳板 / Q04,Q06 | Harbor 碰撞 / WORLD | 以活动地形/码头真实几何建立碰撞，检查坡角、台阶、缝隙及上岸路线；3 处岸点/跳板双向通行和拒绝坡面，10 次无掉穿；渲染几何若有误另派 ART 子卡，不混改。 |
| Q08 动作整合检查 / Q03,Q05,Q07 | 只读测试 / 对应 EditMode、D17ActionQualityPlayModeTests | 对最新源码执行实际 rig+场景的动作/接地/游泳相关回归。验证输入→状态→位移→姿态，源码字符串不能作主要断言；提供连续视觉预检。缺人工标 WAITING_VISUAL，不阻止 Q09。 |

### B. 船、镜头与控制（Q09—Q14，对应 VC-G01、D28）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q09 登船锚点 / Q01,Q07 | D7 甲板 / DECK | 用实际缩放和甲板表面求合法落点，核对父子变换、胶囊恢复。静止近船登船 10 次无抬升、穿甲板；远离/非法状态拒绝不改 parent/mode。 |
| Q10 航行甲板关系 / Q09 | D7 甲板 / DECK | 加减速/转弯时人物相对船稳定，验证已支持甲板活动模式与舵位模式切换。只修甲板随动；船动力缺陷另拆 D2ShipController 卡。无逐帧位置争抢和船体内部落点。 |
| Q11 安全下船 / Q09,Q10 | D7 甲板 / DECK | 岸边、跳板、无落点、移动中按既定设计成功或拒绝；10 次往返，拒绝不丢船控，成功恢复地面碰撞/动画，不能传送进墙/水下。 |
| Q12 相机遮挡 / Q07,Q10 | CameraFollow / CAM | 墙角/船舷/低遮挡物下检查探测半径、忽略自身船体和恢复阻尼。左右绕行/模式切换 10 次，无墙内视角与反复拉近抖动；保留遮挡保护。 |
| Q13 输入恢复 / Q08,Q11 | D16 输入 / UI/D16/D16InputRouter.cs | 失焦→回焦、按住键开关暂停/临时页、切船控时释放旧状态，避免粘键/双消费。10 次切换无移动丢失或 UI 导航吞 WASD；其他模块错误另拆卡。 |
| Q14 船岸整合检查 / Q08,Q11,Q12,Q13 | 只读测试 / D17、D19 路线测试 | 行走→入水→上岸→登船→转弯→拒绝下船→合法下船完整序列，核对 actor owner、碰撞、镜头、动画与输入。每个失败必须关联独立缺陷，不用平均成功率代替全部通过。 |

### C. UI 与经营（Q15—Q19，对应 D29、VC-G01）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q15 面板焦点 / Q13 | D29 UI / UI | 仓库/市场/酒馆互斥；重复快捷键行为遵循现有约定，Esc 关闭后恢复 Gameplay，主 HUD 不占模态焦点；三页逐项 open/repeat/close/recovery 10 次。 |
| Q16 交易桥 / Q15 | D29 UI / UI+业务只读接口 | 审计买卖/存取成功、余额不足、满仓、owner 未就绪与快速双击，界面禁用/提示匹配业务结果；记录前后钱/货/库存，一次请求一次变化。若 owner 重复扣款另建所属系统子卡，UI 不能补钱。 |
| Q17 船坞与整备桥 / Q15 | D29 UI / UI+船坞只读接口 | 修理/整备各成功、材料不足、重复、取消和异常，不能成功提示覆盖拒绝；请求结束解除 busy，页面关闭恢复输入。 |
| Q18 HUD 与本地化 / Q15 | D29 UI / UI+现有 HUD contract | 1280×720、1920×1080、2560×1440，中/英文最长代表文本、已有 UI 缩放档；关键按钮/数值不裁切重叠、无调试面板遮挡。读取原生截图，表列逐分辨率结果。 |
| Q19 船员与反馈审计 / Q16 | 只读 / D9_CREW_AGENT_VERTICAL_SLICE、D10_ACCEPTANCE_REPORT、实际调用链 | 选一个既有船员循环：交谈→合法岗位/关系事件→记忆→存取恢复，核对规则后果和拒绝/离线反馈。LLM 只能提议；无联网条件分开记离线 PASS/在线未测。缺陷生成单 owner 子卡，禁止重建船员系统。 |

### D. 资源与空间（Q20—Q23，对应 D25—D28、VC-G02）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q20 植被恢复 / Q01 | R03 表现生成器 / ART | 定位 disabled palm 的真实 pivot/mesh bounds/坐标基准，先在工作 Prefab 验证一个合法资源，成功后恢复该组。坡面根部接地、冠部不遮必经视线；不能用全禁用算 VERIFIED。资源不足明确欠账。 |
| Q21 港口空间 / Q07,Q20 | R03 表现生成器 / ART | 检查建筑/道具 footprint、重叠、悬空、道路/岸线贴合和可达性；保留交互 ID，镜头/路线覆盖关键位置；只写表现，发现 collider 问题返回 Q07 子卡。 |
| Q22 船海天气审计 / Q10,Q21 | 只读 / D28 卡与实际资源 | Calm/Rain/Storm 检查水线、海面接缝、船体吃水可读性、鱼类生境、粒子遮挡、材质丢失和音效重复；生成按 D28/D27/音频各自 owner 的有界修复子卡。无证据不得升级 ART/SCENE 状态。 |
| Q23 正式资源集成 / Q14,Q18,Q20,Q21,Q22及其修复 | 正式场景/Prefab 唯一写者 | 用已有生成/构建入口集成已验工作资源，记录 GUID、来源、许可和生成参数；不直接改生成物后被 builder 覆盖。检查 InitialHarbor 中旧/新 root 唯一、active 层级、collider 对齐，无 missing script/material；重新打开一致。 |

### E. 航程、性能与最终门禁（Q24—Q33）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q24 保存恢复诊断 / Q19,Q23 | 只读 / D7SaveCoordinator、存档 owner、LOOP | 隔离样例验证陆地/船控/游泳存档语义（按既有设计恢复合法状态），资金货物船员一致；坏档不覆盖好档。每个缺陷按 Save/表现 owner 拆子卡。 |
| Q25 首航流程整合 / Q16,Q17,Q19,Q24及修复 | D30 表现协调 / LOOP | 整备→买货→船员岗位→出航→既有目的地交易/返港→保存→Continue。检查失败不推进阶段、恢复不重复发奖。跨系统问题退各 owner 修复，D30 不代裁决。完成当前源码相关集成回归。 |
| Q26 测量基线 / Q23,Q25 | 只读；必要时 D16 性能探针子卡 | 校验帧采样真实、active probe 唯一、GPU unavailable 不填零；冻结路线、天气、配置。中档 ≥180 秒，低/高对照，生成 §4 指标和最慢帧定位；未达门槛登记真实 FAIL。 |
| Q27 定向优化 / Q26 | 测量识别的一个 owner | 优先一个已证实瓶颈（LOD、阴影、分配、重复更新等），开卡前写路径和预期收益；相同条件前后比较，帧时间改善且画面/玩法无退化。其他瓶颈拆 Q27-a/b；不加无证据对象池。 |
| Q28 候选构建 / Q23,Q25,Q27 | 构建工具/证据 | 复用已成功 builder，生成独立非开发版目录+唯一 build_id，记录源码/配置/资源哈希、构建日志、命令、exe 路径。启动到游戏，不把 MainMenu 烟测当首航成功。 |
| Q29 自动化运行验收 / Q28 | 只读运行/测试工具 | 对该 build 跑所有前述成功/拒绝/重复/恢复路线，记录真实输入来源、状态和短画面；10 次关键转换与 30 分钟连续路线，检查异常/内存趋势。不替代人工门禁。 |
| Q30 独立回归审查 / Q29 | 只读审查，可用同一 Luna 单独一轮 | 不沿用实现者结论，核对 diff、任务验收、最新 XML/构建对应关系；运行必要 Unity 全量 EditMode/适用 PlayMode 与 Python 回归，按改动触及情况验证 AgentCore/server/contracts。skip 必须逐项解释；相关失败阻断。生成未关闭 P0/P1/P2 清单。 |
| Q31 人工试玩与 VC-G01 / Q30且无已知严重缺陷 | 人工验收/证据 | 一次提供唯一 Player、10—15 分钟操作单和问题模板，用户实际检查动作、碰撞、泳姿、船控、UI。用户确认之前 WAITING_VISUAL；单帧/合成输入不代替。收到缺陷退所属卡并重新验证受影响项。原 VC-G01 条件全部满足才由证据升级。 |
| Q32 VC-G02 视觉门禁 / Q31 | 只读验收 | 同一候选 14 固定镜头+3 天气，逐图审查且有移动中的连续画面；无明显穿模/悬空/空白/遮挡；许可证/替换欠账核对。截图数量不等于质量通过。 |
| Q33 VC-G03 / D31 汇合 / Q32 | 状态/证据唯一写者 | 将所有卡映射 D25—D31 的 P/S/G 原门槛，逐项检查依赖和来源；缺任何原要求保持 PARTIAL，派明确补证卡。通过后调用既有同步脚本，不手改生成状态表伪造通过。交付候选说明与已知轻微问题；保持外部发布门禁。 |

### F. 候选版之后（Q34—Q35；不伪称全游戏完成）

| ID / 前置 | 单一 owner / 首读 | 执行与明确验收 |
|---|---|---|
| Q34 全需求覆盖审计 / Q33；可提前只读准备 | 只读 / REQUIREMENTS、D24 覆盖、实际业务入口 | GP-01—GP-22、内容目标、服务端/官网/发布逐行映射：代码、真实资源、Player 入口、存档、测试、人工验收各列；标已证实/缺失/未核对，不从目录条目数推断可玩内容数。 |
| Q35 后续任务交接 / Q34 | 文档规划 | 按已存在需求的真实缺口，生成一个依赖清楚的下一批小任务包；每卡沿用本计划格式并指向 owner。优先船员特色与核心循环，然后其他玩法和扩量；D16-C09/C10/C11 外部发布轨独立。给出当前可开始卡、阻塞条件和完整版本剩余边界，不擅自废止需求或发布。 |

## 7. 防止重复工作与门禁死锁

- Q00 复用可追溯的旧证据，只补最后修改及其依赖的缺口；卡数不是新增功能数。
- Q08/Q14 是技术检查点，原生最终确认集中 Q31；代码合格但视觉待验时，下游技术工作可继续，正式 VC 门禁不能先过。
- 如果 Q22/Q24/Q26/Q30 发现缺陷，新增子卡必须写唯一 owner、依赖和验收，修完回原检查点。不得让“审计”变成无限制跨模块改造。
- Q28 后改代码/资源产生新 build_id。受影响卡重验；最终 Q29—Q32 必须能追溯到最终候选，不能拼接不同构建的成功证据。
- 首批明确执行 Q00→Q01→Q02，随后按依赖推进。候选质量或必需人工验收未满足时不宣称目标完成。

## 8. 给 Luna 的目标提示词

```text
你继续担任本项目主要执行 Agent，采用 Luna，按小任务持续开发。
项目：C:\Users\Administrator\Desktop\game\first01

先遵守 AGENTS.md 和 docs/CONTEXT.md，再阅读：
docs/game/LOCAL_QUALITY_REMAINING_PLAN.md
这是用户新授权的剩余执行计划；保留原 D25—D31 和 VC 门禁，保留已有代码与证据。

目标：按 Q00—Q35 和必要的单模块修复子卡，交付动作顺畅、无已知严重 Bug、
无明显穿模/悬空、游泳和上下船正确、经营/完整航程/存档恢复可靠的本地候选版；
完成现有正式门槛后，再输出完整需求覆盖审计和下一批具体任务。
不把 D31、测试数量、禁用问题资源或一次截图成功当作全游戏完成。

从 Q00 暂停点核对开始，接续最后未验证的泳姿修改；执行 Q01 根因/尺度审计，
再按依赖逐卡实现、定向验证、留痕和继续。一次只写一个产品模块，
不要重读全部历史、盲目反复试坐标或每张卡都跑全仓回归。
连续两轮同假设无改善就换成证据诊断；每个卡片结果必须对应实际源码/构建。

已有功能满足卡片条件就复核复用，不为了交付而重写。诊断/验收卡可不改代码。
不覆盖未提交工作，不停止不属于本次任务的进程，不自动发布或购买资源。
依照计划的性能、碰撞、动作、业务和存档标准验证；不能降低阈值来通过。

每张卡完成后更新 RESULT、简短 handoff、看板与当日日志，并继续下一可执行卡。
遇到外部依赖时记录解除条件并继续独立工作；到 Q31 集中提供 Player 和试玩清单，
需要用户真实验收时明确等待，不伪造人工 PASS。
汇报当前任务、已证实结果、未解决问题和下一个动作，不停在计划或承诺。
```

## 2026-09-09 Q05-A02 / Q05-A03 执行留痕

Q05-A02 在 D2 presentation owner 内修复程序化 fallback 游泳前臂的确定性子节点折叠，并把 Q26 水域诊断标签从 `shoreline_swim_continuous` 修正为 `water_swim_continuous`。定向 Python `56/56 PASS`、全量 Python `1143/1143 PASS`、Unity 水域专项 `1/1 PASS`；R29 真实 Win32 路线 `48` 输入/19 截图/168 observations/37 swimming/22 Ship control。截图仍显示低模与正式动作资源缺口，因此保持 `PARTIAL`，Q03 继续 `BLOCKED_ASSET`。

Q05-A03 修复 D7 在游泳状态下使用过期 grounded 帧重新施加视觉落地偏移的问题，更新当前舵位校准断言，构建 R31 并进行真实路线复核。Q26 `6/6 PASS`、D17 `15/15 PASS`、全量 Python `1143/1143 PASS`；R31 `48` 输入/19 截图/164 observations/72 moving/122 swimming/21 Ship control，船体位移 `5.9478812m`、航向变化 `17.8417969°`，route/business/pause/interaction matrix 为 `true`，fatal 为 `null`。

两轮都没有关闭视觉门禁：R31 截图仍确认低模动作/泳姿、自然握舵、港口空间和植被问题。后续优先解除 Q03 合法动作/Avatar 资源和 Q20 正式树/棕榈资源阻塞；资源未到位时不继续无证据坐标微调，也不升级 Q31、VC-G01、VC-G02/VC-G03。

## 2026-09-09 Q00/Q01 当前复核与 Q04 R9

Q00/Q01 已切换到 Q05-A03/R31 当前证据链：工作区保留不清理；原生 Win32 路线进入 InitialHarbor，记录 48 输入、19 截图、164 observations、72 moving、122 swimming、21 Ship control，船体位移 5.9478812m、航向变化 17.8417969°，fatal null。R31 证明的是有限真实输入路线，截图视觉仍为 PARTIAL；动作、游泳、上船问题按当前画面分别记录为部分复现，树木旧 Renderer 路径受控但正式 pivot/资源视觉仍未确认。

Q04 已用现源码执行 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles`：`Q04_WATER_TEN_CYCLES_PLAYMODE_R9.xml` 为 total=1/passed=1/failed=0/inconclusive=0/skipped=0，Unity code 0 退出且无残留 Unity 进程。R9 日志记录浅水/深水重复进入、非零游泳位移/速度、`surfaceY=-0.02`、`playerY=0.42` 和回岸 grounded recovery。Q04 `VERIFIED` 仅覆盖 D2 水域逻辑，不覆盖原生键盘路线、游泳姿态或水面视觉。

当前顺序保持：Q03 合法动作/Avatar 资源门禁仍为 `BLOCKED_ASSET`；Q05/Q08 继续代码级姿态/整合验证；Q09 处理船舵/可见船体关系；Q20 处理植被 pivot/资源准入；Q31 继续 `WAITING_VISUAL`。不得因 Q04 逻辑通过而解锁 VC-G01、VC-G02、VC-G03 或 D31。

## 2026-09-10 Q13-A01 与用户人工否决

用户已实际进入当前 R31 Player，否决任务动作扭曲、海洋不能游泳、上船人物与舵/控制对象关系错误、树木漂浮，以及 W/S 移动时右上角 HUD 菜单跟随切换。Q31 已从 `WAITING_VISUAL` 改为 `PARTIAL`，人工门禁为 `REJECTED_ON_REPORTED_DEFECTS`；VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。

针对 W/S 冲突，Q13-A01 在 `HarborInteractionStabilizer` 中按玩法/模态状态管理 `InputSystemUIInputModule.move`：玩法态置空，模态态恢复。D17-A15 `1/1 PASS`，日志记录 10 轮 W/S 与 pause/D16 block/unblock 的导航状态；当前源码已编入 R31 Player。一次有界 Win32 尝试未被探针观察到进入 InitialHarbor，Editor D17A06 主菜单生命期却为 `1/1 PASS`，因此先诊断点击/菜单状态/Player 路由边界，再复跑 VC-G01；不把自动化 PASS 扩大为画面通过。

## 2026-09-11 Unity Pipeline 与 D20-A02-R02 隔离门禁

已用 `com.unity.pipeline@0.6.0-exp.1` 在后台 Unity Editor 完成一次 D20-A02-R02 隔离兼容性复核：R02 男性 Humanoid Prefab、D20 15 状态 Controller、Presentation Adapter、9 个 Socket 与“独立物理根承载可视子节点”契约最终 `6/6 PASS`；同一 Editor 的 D17 PlayMode 过滤回归为 `21/21 PASS`。首轮直接把可视 Prefab 当物理根时 Animator.Update 造成 Y=`0.08m`，该失败被保留并作为集成形状约束，未被改成假通过。

这只推进了 Q03 的隔离适配器证据，不解除 Q03 `BLOCKED_ASSET`，不注册 InitialHarbor formal consumer，不升级 Q08/Q31/VC-G01。`capture_game_view` 因后台 Editor 的 `-batchmode -nographics` 无 GPU 而不可用，本轮没有新增视觉截图；正式动作、游泳姿态、握舵、植被接地和漂浮/穿模仍按 Q03/Q05/Q20/Q21 单一 owner 处理。

## 2026-09-11 Q20-A02 直挂 legacy palm placeholder Renderer 隔离

本轮在 Unity Pipeline 的 InitialHarbor PlayMode 前置审计中发现 5 个直接场景根 `ph_palm_tree`；样本 Renderer bounds `min.y=0.280`，其中两个与 R03 自有地形命中存在 `0.088m/0.292m` 的高度差，另外三个没有自有地形命中。D26 旧逻辑只隔离嵌套 `D19_SaintMargaret_Root`，现改为同时隔离 `D19_SaintMargaret_Root` 与 `ph_palm_tree` 子树；Q26/A03 同步扩大识别范围，避免直接根漏检。

Pipeline 重编译 `failed=false/errors=[]`；Q26/A03 `1/1 PASS`，随后 Q26 全类回归 `6/6 PASS`（W 游泳、连续水中运动、E 登/下船与远端拒绝、舵位和树/植被审计）；`activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q20-A02/RESULT.json` 和 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q20-A02/Q20-A02.md`。

Q20-A02 只登记为 `PASS_WITHIN_SCOPE`：不恢复 authored palms、不改变 D18 ready catalog、Collider/NavMesh/Systems 或 Player 视觉状态。`Q20=PARTIAL/BLOCKED_ASSET`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持不变；后台 Editor 无 GPU，未新增截图。

Q00 当前接管复核：工作区仍保持 dirty，`8469` 条状态、`8057` 条未跟踪项未清理；当前 Q20-A02 源码与 Pipeline 证据已重新绑定。Q26 全类 `6/6 PASS`、Q21 港口空间 `1/1 PASS`、Q22 船海天气 `1/1 PASS`、error console `0`。这只更新暂停点与证据关系，不改变 Q03/Q20/Q31/VC-G01 的正式状态。

Q21/Q22 当前源码复核结果分别登记在 `game/tools/results/LOCAL-QUALITY/Q21/Q21_PIPELINE_RECHECK_20260911_RESULT.json` 和 `game/tools/results/LOCAL-QUALITY/Q22/Q22_PIPELINE_RECHECK_20260911_RESULT.json`。两张卡均为 `PASS_WITHIN_SCOPE` 的静态合同复核，父卡继续 `PARTIAL`，下一步进入 Q23 正式资源集成审计，不把静态结果扩大为 Player 视觉通过。

Q23 当前源码 Pipeline 复核已登记在 `game/tools/results/LOCAL-QUALITY/Q23/Q23_PIPELINE_RECHECK_20260911_RESULT.json`：正式场景唯一 active R03/D27/船 Prefab、重开一致性和活动材质/脚本合同 `1/1 PASS`，error console `0`。Q23 仍为 `PARTIAL`，下一步进入 Q24 存档恢复诊断。

Q24 当前源码 Pipeline 复核已登记在 `game/tools/results/LOCAL-QUALITY/Q24/Q24_PIPELINE_RECHECK_20260911_RESULT.json`：陆地/出海 round-trip、出生锚点和坏档保护 `3/3 PASS`，error console `0`。Q24 仍为 `PARTIAL`，因为当前 schema 没有独立游泳状态/姿态字段；下一步进入 Q25 首航/循环整合。

Q25 当前源码 Pipeline 复核已登记在 `game/tools/results/LOCAL-QUALITY/Q25/Q25_PIPELINE_RECHECK_20260911_RESULT.json`：D30/D23 首航投影 EditMode `2/2 PASS`，InitialHarbor 路线 PlayMode `2/2 PASS`，error console `0`。Q25 仍为 `PARTIAL`，因为该证据不覆盖完整人工路线、原生 Player 输入/视觉、音效或性能；下一步进入 Q26 测量基线。

Q26 当前源码 Pipeline PlayMode 复核已登记在 `game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RECHECK_20260911_RESULT.json`：完整 `D19R03Q26GameplayDefectPlayModeTests` 为 `6/6 PASS`，error console `0`；A03 当前日志确认旧树 Renderer `0`、R03 植被 `24`、unsupported `0`。Q26 仍为 `PARTIAL`，因为该证据仍是 Editor/Input System 注入边界，不能替代 standalone Player 的物理输入、截图、GPU/目标设备性能和最终 VC-G01；下一步按既定依赖进入 Q27/Q28。

## 2026-09-11 Q28/Q29 当前源码候选 Player 复核

Q28 已用当前源码通过 Unity Pipeline 后台 detached build 生成 `Q28-CANDIDATE-R3`：`BuildOptions.None`、`MainMenu;InitialHarbor`，构建任务成功，Player SHA256 `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Pipeline error console `0`。证据见 `game/tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_RECHECK_20260911_RESULT.json`；Q28 继续 `PARTIAL`，该结果只证明当前候选可构建，不是视觉通过。

Q29 在该当前候选上完成两次有界真实 Windows Player/Win32 输入路线：水上路线 `48/48` 输入、`19` 张截图、`166` observations、`124` swimming、`21` Ship control，业务页/暂停/交互矩阵均为 true；远距离路线 `45/45` 输入、`17` 张截图、`123` observations、`81` 次 `ship_too_far` 拒绝，均正常退出。结果见 `game/tools/results/LOCAL-QUALITY/Q29/Q29_PLAYER_RECHECK_20260911_RESULT.json`。两条路线只证明当前候选的移动/游泳/船控/业务/暂停/远距拒绝运行证据，截图仍暴露低模 fallback、港口几何/层级/接地风险；owner-not-ready 拒绝未在本轮覆盖，Q29、Q31、VC-G01 继续保持未完成状态。

## 2026-09-11 Q30 当前候选独立复核

Q30 对 Q20-A02、Q28-CANDIDATE-R3 和 Q29 当前路线做独立复核：定向 Python `75/75 PASS`，全量 `python -m unittest discover -s tools/tests -p 'test_*.py'` 为 `1144/1144 PASS`；Q28 构建及 Q29 水域/远距路线的运行证据均可追溯。结果见 `game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json`。该复核确认逻辑路线和测试契约无回归，但截图仍显示低模角色、游泳/握舵接触和港口几何/接地风险；Q30/Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`。

## 2026-09-11 Q31 当前候选视觉门禁复核

Q31 对 Q28-CANDIDATE-R3 的 Q29 真实路线与截图做了工程复核：路线运行证据成立，但截图仍未满足角色动作、游泳姿态、人物—舵轮接触、港口几何/接地和正式资源门禁。结果见 `game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_CANDIDATE_RECHECK_20260911_RESULT.json`；`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`，不进入 VC-G02。下一步选择单一 owner-scoped 缺陷修复并补新的 Player evidence。

## 2026-09-11 Q03 candidate-only runtime bridge

在不改变正式 InitialHarbor consumer 的前提下，使用 Unity Pipeline 新增 D20 R02 candidate-only bridge、候选资源 builder 和 PlayMode 集成测试。连接 Editor 重编译 `failed=false/errors=[]`，候选桥接测试 `1/1 PASS`；候选 `BuildOptions.None` Player 启动后在 `1920x1080` 窗口执行了 `45` 个真实 Win32 输入步骤和 `17` 张截图，候选安装日志、InitialHarbor marker、业务页、Esc 暂停/恢复和 clean close 均有证据。结果见 `game/tools/results/LOCAL-QUALITY/Q03/Q03_D20_R02_CANDIDATE_RUNTIME_RECHECK_20260911_RESULT.json`。

本结果只登记为 `PASS_WITHIN_SCOPE`：candidate visual 已挂到 D2 物理根下、root motion 已关闭、D2 仍拥有移动/碰撞；现有 RERUN85 probe 不识别新 flag，movement/swimming/ship/far-reject 未证明，候选 controller 也没有 Swimming state。原生截图仍为低模/抬臂/T-like 姿态，故 Q03 继续 `BLOCKED_ASSET`，formal consumer=`0`，Q08 `BLOCKED_PREREQUISITE`，Q31 `PARTIAL`，VC-G01 `IN_PROGRESS`，VC-G02/VC-G03 `LOCKED`。该 candidate bridge 不能替代人工视觉门禁，也不能宣称任务动作扭曲或海洋泳姿已修复。

## 2026-09-11 Q03 generic clip-path adapter follow-up

只读 Unity Pipeline 审计确认 D20 clips 为 generic path 曲线，而 R02 source Prefab 的 `Armature` 被额外 wrapper 包裹；有效 Humanoid Avatar 与这组 generated clips 不构成正式动作契约。候选 Builder 现在只对 candidate 资源展开 wrapper、把 `Armature` 提升到 Animator 根并清除 candidate Avatar，Bridge 显式报告 `UsesGenericPathAdapter=true`，不改变 source R02、formal Player 或 InitialHarbor。

这次修复先由候选 PlayMode 断言暴露 adapter 每帧把 `AlwaysAnimate` 覆盖为 `CullUpdateTransforms`，修正后候选测试 `1/1 PASS`；D20-A02 EditMode `6/6 PASS`、recompile `failed=false/errors=[]`、清空陈旧 timeout 后 Console error=`0`。最新 native candidate smoke `1920x1080`、`45` 输入步骤、`17` 张截图、candidate generic adapter 日志、业务/暂停恢复和 clean close 成立；结果见 `game/tools/results/LOCAL-QUALITY/Q03/Q03_D20_R02_CANDIDATE_RUNTIME_RECHECK_20260911_RESULT.json`。

同一重编译后的正式 D17 PlayMode 过滤回归为 `21/21 PASS`（`failed=0/skipped=0/inconclusive=0`，约 `129.9s`），说明 opt-in candidate bridge 没有污染现有 D2 owner/输入/水域/船控代码级合同；这仍不是原生视觉或正式 Q03 通过。

该 generic adapter 只证明 generated clip 曲线现在被候选消费；candidate bridge 另将 D2 `Swimming` 映射到现有 `Fish` generic presentation fallback，但不提供正式 authored Swimming clip。截图仍为低模黑色/抬臂姿态，港口几何与泳姿问题仍在，RERUN85 probe 未识别。因此 Q03 继续 `BLOCKED_ASSET`，formal consumer=`0`，Q08 `BLOCKED_PREREQUISITE`，Q31 `PARTIAL`，VC-G01 `IN_PROGRESS`，VC-G02/VC-G03 `LOCKED`。

## 2026-09-11 Q03 candidate Swimming presentation fallback

D2 物理层已经有 `D2CharacterAction.Swimming`，但 candidate bridge 原先把它视为 unsupported。本轮只在 opt-in candidate bridge 内把它映射到 D20 controller 的现有 `Fish` generic state，并增加 `SwimmingVisualSupported=true` 诊断；不改正式 controller、InitialHarbor consumer 或 Systems owner。Candidate PlayMode `1/1 PASS`、D20-A02 EditMode `6/6 PASS`、recompile `failed=false/errors=[]`。该结果只关闭候选的状态消费缺口，不关闭正式泳姿资源、连续 native swimming 或截图视觉门禁。

Q26 舵位专项随后重跑为 `6/6 PASS`，A05 记录左右手与 `HelmWheel` 同一 Y/Z 平面、距离 `0.1927m`；这只关闭了坐标级诊断，不关闭截图视觉门禁。结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_PIPELINE_RERUN_20260911_RESULT.json`。

## 2026-09-11 Q03 candidate outfit rebind follow-up

Candidate Builder 进一步复用 D18 male-peasant outfit 的 4 个 SkinnedMeshRenderer，并按相对骨骼路径重绑定到展开后的 D20 R02 `Armature/root`；outfit 自带的第二套 Armature/LODGroup 仅在 candidate 资源中移除。Candidate prefab 当前为 `7` 个 skinned renderer，颜色与轮廓可读性有所改善，正式 R02 source/InitialHarbor 未改变。

本轮证据：D20-A02 EditMode `6/6 PASS`、candidate PlayMode `1/1 PASS`、recompile `failed=false/errors=[]`、清空后 error console=`0`、Player 构建成功；最新 native smoke 为 `45` 输入步骤/`17` 截图/`1920x1080`/业务与 Esc 恢复成立/clean close。截图仍显示低模抬臂/T-like 姿态、登船帧浅色重复人物和港口几何/层级缺陷，且 RERUN85 probe 未识别，故只记 `PASS_WITHIN_SCOPE`，Q03 仍 `BLOCKED_ASSET`、VC-G01 仍 `IN_PROGRESS`。

## 2026-09-11 Q26/A10 船员出生点重叠修复

通过已连接 Unity Pipeline 对候选 Player 登船前的额外浅色人物做对象归属审计后确认，它来自 InitialHarbor 场景根 `/ph_crew_male`，不是 D20 candidate bridge 重复渲染；原位置与玩家出生锚点约 `0.25m` 水平重叠。按单一表现/交互 owner 用 Pipeline `set_transform` 将其移到 `(2.50,1.10,-2.95)` 并 `save_scene`，D2 船员交互范围 `3.5m` 保留。

新增 Q26/A10 断言要求玩家/出生锚点到船员至少 `1.25m` 且仍可交互；当前 `player_to_crew=2.5125m`、`spawn_to_crew=2.5125m`。专项 `1/1 PASS`，完整 Q26 `10/10 PASS`，recompile `failed=false/errors=[]`，error console `0`，Editor `ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CREW_ANCHOR_SEPARATION_PIPELINE_20260911.json`、`Q26_CREW_ANCHOR_SEPARATION_REGRESSION_PIPELINE_20260911.json`。

这只关闭 NPC/出生点空间重叠，不升级 native Player 视觉门禁；低模/T-like 动作、直立泳姿、港口几何层级、正式动作/棕榈资源仍未收口。Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。

## 2026-09-11 Q26/A10 最新源码 Player R2 交互闭环

A10 场景变换后，使用 Unity Pipeline 重编译并生成 `Q26_A10_SCENE_BOUND_CANDIDATE_R2.exe`；构建 `build_9d0db92d0afd` 成功，`BuildOptions.None`，`totalErrors=0`、`totalWarnings=8`。R2 构建、哈希和边界见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A10_SCENE_BOUND_CANDIDATE_R2_BUILD_RESULT.json`。

最新源码真实 Win32 证据：完整路线 `RERUN85` 为 `46/46` 输入、`17` 截图、`122` observations、`36` moving、最大速度 `3.2000002861`，turning、远船 `E` reject、I/M/T 重复、Esc close/recovery、业务/暂停/clean close 均有 probe/日志；WaterProbe `RERUN86` 为 `49/49` 输入、`19` 截图、`166` observations、`123` swimming、`22` Ship control，船体位移 `5.9714837074m`、航向 `17.3263244629°`，三类 observation proof 为 true。证据分别见 `VC_G01_RUNTIME_INPUT_Q26A10R2_20260906.json`、`VC_G01_RUNTIME_INPUT_Q26A10W1_20260906.json`、`D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN85.json` 和 `RERUN86.json`。

为避免新 route 编号静默关闭探针，`D19R03PlayerRouteProbe` 增加 rerun85/rerun86 allow-list、截图前缀和结果文件名；定向 Python `6/6 PASS`，Pipeline recompile `failed=false/errors=[]`，Editor 最终 `ready/stopped`、error console `0`。首次 R1 因未登记 route flag 而仅为 `INPUT_ATTEMPTED`，不计入通过；R2 才是有效当前源运行证据。

这次只关闭 VC-G01 当前候选的真实脚本交互证据缺口，不关闭视觉门禁。截图仍有低模/程序化动作与直立泳姿、船舵接触可读性及港口几何/接地问题；Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。下一步选择一个单一 owner 的视觉缺陷修复，不同时调动作、游泳、舵位和植被。

## 2026-09-11 D35 静态回归契约同步

Q26/A10 之后的全量 Python 回归首次只剩 D35 R03 静态材质审计失败，根因是测试仍期待旧的 `300 Renderer`，而当前连接 Unity Editor 生成的权威审计为 `294 Renderer / 1 Collider / 40402 vertices`。已在 D35 工具与测试中同步该事实；没有修改生产 Unity 资源，也没有改变任何视觉门禁状态。

定向测试 `1/1 PASS`，全量 `python -m unittest discover -s tools/tests -p 'test_*.py' -q` 为 `1148/1148 PASS`（`168.034s`）。结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_D35_RENDERER_CONTRACT_SYNC_20260911_RESULT.json`。这只修复静态验证契约，Q26/Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`。

## 2026-09-11 Q26/A11-A12 真实流程与取景复核

本轮利用 Unity Pipeline 完成了一个有界的 `CameraFollow` 船控取景修复，并用包含 `MainMenu + InitialHarbor` 的 A12 Player 重跑真实 Win32 路线。相机参数为 `5.5m/20°/target.y=1.25m`，对应静态契约 `6/6 PASS`、Q26 PlayMode `10/10 PASS`。A12 构建 `build_183e12159163` 成功，0 errors；完整路线 `46/46` 输入、17 截图、D10 owner-ready，水域路线 `49/49` 输入、19 截图、124 swimming、21 Ship control，Python 全量 `1148/1148 PASS`，Editor error console=`0`。

A11 的 InitialHarbor-only 路线没有经过 NewGame，D10 owner 未就绪，因此不作为酒馆业务成功证据；A12 已补齐该环境前置，I/M/T 重复、Esc 关闭/恢复、成功路径和远船拒绝由真实日志/探针登记。截图复核仅接受船控取景在本范围内改善，直立低模泳姿、港口几何/层级/接地仍拒绝视觉通过。Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 不变；下一步按单一 owner 处理正式动作/Avatar、泳姿或植被/接地，不继续无假设地调坐标。

## 2026-09-11 Q26/A16 R03 perimeter-boundary bridge recheck

A16 将当前 R03 边界桥接修复编译进新的 `MainMenu + InitialHarbor` Player。根因是 D19 presentation bridge 等待旧的精确 `ShoreBoundary`，而 D6 当前真实 owner 提供的是位于水域外的 `.South/.West/.East` 外围边界集合；本轮改为识别 owned perimeter set，并保留外围墙在水域外，避免把 South wall 错移到游泳路线。

验证证据：Unity Pipeline `build_319d0c427c0d` 成功，`0 errors/8 warnings`；Python 全量 `1150/1150 PASS`；Unity 全量 EditMode `619/619 PASS`；VC-G01 world logic `13/13 PASS`；Q26 PlayMode `10/10 PASS`；定向契约 `20/20 PASS`；recompile `failed=false/errors=[]`；Editor `ready/stopped`，error console=`0`。真实 Win32 `RERUN88` 为 `46/46` 输入、17 截图、122 observations、36 moving，移动/转向/游泳/业务/暂停/远船拒绝/交互矩阵/clean close 成立；日志记录 `route collision window retained: D6-owned R03 perimeter boundaries remain outside water`、legacy colliders disabled=`123`、triggers preserved=`22`。本轮 ship control proof 为 false，继续引用 A12 WaterProbe 作为独立船控证据。

A16 初始港口截图仍显示侧面建筑过近/裁切、低细节程序化玩家和粗糙港口几何/接地。因此这轮只关闭 stale boundary bridge warning 与当前运行证据缺口，不升级视觉门禁；Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。下一步选择正式动作/Avatar、泳姿或港口几何/接地中的一个 owner-scoped 缺陷，并先写出可验证假设。

## 2026-09-11 Q26/A17 InitialHarbor first-frame framing candidate

A17 根据 A16 的空间测量，对 `CameraFollow` 的 InitialHarbor Character 首帧做了唯一 presentation 变更：`13.5m -> 18.5m`。玩家左右 `6.4m` 的 Market/Warehouse 服务建筑不再把近侧立面推到视锥边缘；建筑位置、路网、碰撞和交互 owner 没有改动。`D19R03PlayerRouteProbe` 只增加 `RERUN89` 独立 evidence namespace。

真实验证：Pipeline build `build_d7a1d45e6e1e` 成功、`0 errors/8 warnings`；A17 Win32 `46/46` 输入、17 截图、122 observations、36 moving，movement/turning/swimming/business/pause/far reject/interaction matrix/clean close 成立；Python `20/20` 定向、`1150/1150` 全量，Unity 相机 PlayMode `1/1`、VC-G01 world logic `13/13`、Q26 PlayMode `10/10`、全量 EditMode `619/619`，error console=`0`。A17 对比截图登记为 `IMPROVED_WITHIN_SCOPE`，但左下建筑仍部分裁切，低模玩家、地面断缝和材质粗糙仍在。

该候选关闭一个首帧构图假设，不升级视觉门禁；Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。停止继续微调初始相机，下一步选择正式动作/Avatar、泳姿或港口几何/接地中的一个新的 owner-scoped 缺陷。

## 2026-09-11 Q26/A20 游泳镜头与全量顺序回归

A20 在 `CameraFollow` 中让游泳镜头持续跟随 D2 heading，并在真实 mouse/right-stick look 输入时交回手动控制；同时加入有限值保护，消除全量 PlayMode 中实际暴露的 NaN camera pose。Q03 candidate-only PlayMode 测试在启动阶段恢复 D16 gameplay input/HUD 状态并显式启用候选角色，避免顺序污染；正式 InitialHarbor consumer 未改变。RERUN92 为独立 route/probe/screenshot namespace。

验证：Pipeline build `build_3bba5aaa08b8` 成功，MainMenu + InitialHarbor，0 errors/8 warnings；A20 WaterProbe Player `49/49` 输入、19 截图、167 observations、68 moving、119 swimming、21 Ship control，船体位移 `6.00153923034668m`、航向 `17.986114501953125°`，业务/暂停/交互矩阵/clean close 成立，fatal=null。Python 全量 `1152/1152 PASS`；Unity EditMode `619/619 PASS`；PlayMode `99 passed / 0 failed / 1 skipped of 100`；Q03 targeted `1/1 PASS`；error console=0，Editor=ready/stopped。

截图判定：游泳现在能读出横向姿态，但低模程序化角色、港口建筑近景/裁切、岸线/地面断缝和材质粗糙仍在。`far_ship_reject_proof=false` 仅表示本次 WaterProbe 未覆盖远距拒绝窗口。Q26/Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`。下一步选择正式动作/Avatar 或港口几何/接地的单一 owner，不再继续无新证据地微调首帧相机。

## 2026-09-11 Q03-A08 / D31-P04 shoreline profile update

本轮将下一步从 Fish pitch 盲调转为 D31-P04 的单一环境接地 owner。R03 岛面原有 perimeter 到 `y=-0.42` 的厚石色竖壁已由 `D19R03HarborPresentationBuilder` 改为 64 段、每段 6 顶点的砂色斜坡—下沉 shelf：宽度 `1.35m`、shelf `y=-0.24`、底部 `y=-0.42`，并保持 `D19_TerrainRoot` 的唯一 MeshCollider。

实际验证：Q03-A08=`1/1 PASS`；Q20/Q21/Q22/Q23 各 `1/1 PASS`；Q24=`3/3`；Q25=`2/2`；Q26=`10/10`；重编译 `failed=false/errors=[]`；Editor=`ready/stopped`、error console=`0`。专项首次因旧 Prefab 顶点数不匹配失败，实际重建后通过，作为 stale-Prefab 防护证据。

边界不变：连接的 Unity Editor 为 batchmode/headless，`capture_game_view --source screen` 返回 `No GPU available (batchmode/headless); cannot capture.`，所以本次只通过几何契约和回归，未升级视觉 PASS。Q03 仍 `BLOCKED_ASSET`，Q20/D31-P04/S05/G06 仍 `PARTIAL`，VC-G01 `IN_PROGRESS`，VC-G02/VC-G03 `LOCKED`。下一步必须用 GPU-capable Player/Editor 取得当前源 post-fix 帧，再选下一个单一 owner。

补充回归：R03 重建后连接的 Unity Pipeline EditMode 全量 `623/623 PASS`，`failed=0/skipped=0/inconclusive=0`，耗时 `73.46s`；Editor 回到 `ready/stopped`、error console=`0`。该证据只确认编辑器契约未回归，不替代 GPU Player 的 post-fix 视觉判断。

## 2026-09-11 Q03-A09 当前源码 Player 与 probe archive

本轮先完成测试取证工具的最小改进：`tools/vc_g01_runtime_input_probe.ps1` 在提供 `EvidenceTag` 时，将固定 RouteFlag 派生的底层 probe JSON 另存为 VC-G01 独立归档，并在 summary 登记 `probe_archive_path/probe_archive_exists`，避免后续同一 allow-list route 覆盖历史证据。该改动只属于测试工具，不改变玩法 owner、输入绑定或场景资源。

Unity Pipeline build `build_cd0734bfe4bb` 生成当前源码 Player，`StandaloneWindows64 / MainMenu + Bootstrap + InitialHarbor / BuildOptions.None / 0 errors / 1 warning`；Player SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`A9072617F5B3432953027E93F43C8043787D04DDC74D62EE8CD8D693AC9E5064`。PowerShell 解析=`PASS`，`test_vc_g01_runtime_probe.py`=`6/6 PASS`。

真实 standalone Player 使用原生 Win32 WaterProbe：`49/49` 输入、19 截图、166 observations、69 moving、122 swimming、21 Ship control；船体最大位移=`5.997463m`、航向变化=`18.239227°`，移动/游泳/船控/业务/暂停/I-M-T 重复/Esc/clean close 成立。远船拒绝和 I/M/T owner-not-ready 窗口未覆盖，保持 false/未证明。截图复核确认逻辑闭环成立，但角色仍为低模程序化 fallback，泳姿呈横向悬浮/四肢下垂，舵轮接触和港口岸线/地面几何仍不合格。

结果见 `game/tools/results/LOCAL-QUALITY/Q03-A09/RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q03-A09/Q03_A09_BUILD_RESULT.json`、`game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_RUNTIME_INPUT_Q03A09_CURRENT_A08_ARCHIVEFIX_20260906.json` 和独立 probe archive。状态保持 `Q03-A09=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。下一步选择正式 Avatar/动作资源、泳姿表现或港口几何/接地中的一个单一 owner，不把真实路线通过升级为视觉 PASS。

## 2026-09-11 Q00/Q01/Q02 当前源重新对齐

接管后按 Q00→Q01→Q02 依赖顺序重新执行：Q00 记录当前脏工作区、Unity Pipeline、Q03-A09 Player/Assembly 哈希和 native route 关系；Q01 通过 connected Editor targeted eval 重新登记根/尺度/胶囊/水位/船锚点/owner/tolerance，并明确一次过宽表达式超时没有形成证据；Q02 用当前 D2 源文件重新执行 D2 EditMode `8/8 PASS` 与 D17 PlayMode `16/16 PASS`。这些结果只闭合暂停点、owner 和逻辑状态的当前证据关系，仍不解锁 Q03 的合法动作/Avatar 资源门禁。

当前 Player 真实路线为 `49/49` 输入、19 截图、166 observations、69 moving、122 swimming、21 Ship control；实际截图继续确认低模程序化人物的横向悬浮泳姿、有限四肢动作、舵轮接触和港口几何风险，因此 Q31/VC-G01 不升级。下一张执行卡必须是具有具体根因的单一 owner 变更或资源审计；不再进行无新证据的坐标盲调。

## 2026-09-11 Q03-A10 fallback 游泳手臂轴向复核

本轮根据 Q01/Q02 当前证据选择了 D2 fallback 表现层的单一根因：`64°` visualRoot pitch 与上臂局部 `-Y` 中性轴组合，使旧上臂/前臂角度在真实 Player 中仍呈下垂。修复只对完整程序化 fallback 使用 `150°` 上臂前伸和 `30°` 前臂补偿；导入蒙皮仍 neutral，未改变 D2 physical root、Systems 或正式 Animator。

验证：connected Editor recompile `failed=false/errors=[]`；D2 EditMode `8/8`、D17A08 `1/1`、D17 全量 `16/16`；Q03-A10 Player `build_b4b4570bffeb`，`0 errors/8 warnings`；原生 RERUN100 archive `49/49` 输入、19 截图、164 observations、69 moving、120 swimming、21 Ship control，业务/重复 I-M-T/Esc/暂停/clean close 成立。截图只确认手臂前伸轮廓改善，低模比例、腿脚、自然泳姿、舵轮接触和港口接地仍不合格。

权威结果：[Q03-A10 RESULT](../game/tools/results/LOCAL-QUALITY/Q03-A10/RESULT.json)、[Q03-A10 handoff](design/common/handoffs/LOCAL-QUALITY/Q03-A10/Q03-A10.md)。状态保持：`Q03-A10=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。下一步停止继续盲调游泳角度，选择正式 Avatar/动作资源、舵轮接触或港口接地/几何中的一个单一 owner，并先写可复现假设。

## 2026-09-11 Q03-A11 船体视觉正向与 D2 航行轴对齐

本轮选择 D19-R03/D2 的单一逻辑—表现交界缺陷：可见船体 child 原先额外旋转 `180°`，稳定船根与 D2 heading=0 的航行轴不一致，真实 W 会把船推向港口服务建筑。修复将稳定船根初始 heading 设为 `180°`、候选可见船体 child 改为 identity，并对舵轮 anchor/轮体做世界位置等价补偿；登船玩家面向舵轮，未改变 Systems 航行裁决。新增 Q26A11 forward-axis contract。

证据：Player build `build_3e69fa3e4248`，`0 errors/8 warnings`；Q26 `11/11`、D17 `16/16`、VC-G01 world logic `13/13`、Q22 `1/1`、Q23 `1/1`；recompile 无错误，Editor=`ready/stopped`，error console=`0`。真实 Win32 R3 为 `48/48` 输入、`19/19` 截图、`167` observations、`69` moving、`4` swimming、`21` ship-control，船体位移 `6.005729m`、航向变化 `18.372559°`；船向证据 `ship.forward` 与可见候选 `forward` dot=`1.000`，W/A 后船向水域移动。

这只证明本卡范围内的交互和船向根因已闭环，不升级完整视觉门禁：程序化低模角色、泳姿、舵轮接触、港口几何/接地仍未通过；远船 reject 与 owner-not-ready 本次未覆盖。故 `Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`D31-P04/D31-S05/D31-G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持。权威结果：[Q03-A11 RESULT](../game/tools/results/LOCAL-QUALITY/Q03-A11/RESULT.json)；交接：[Q03-A11 handoff](design/common/handoffs/LOCAL-QUALITY/Q03-A11/Q03-A11.md)。下一步只选择一个 D31 环境/接地 owner，或等待合法正式 Avatar/动作资源后再关闭 Q03 asset gate。

## 2026-09-11 Q00 当前源重绑定（Q03-A11 之后）

Q00 已重新核对当前工作区和最新 Player，不再继承 Q03-A10/A20/R31 的旧关系：status=`5331`（tracked=`439`、untracked=`4892`），HEAD=`01024cae2823c26b2595a660e6dde5879870dfc8`，Unity Pipeline Editor=`6000.0.80f1 / PID 31416 / 127.0.0.1:7800 / ready`。当前 Player 是 `build_3e69fa3e4248`；其 R3 原生路线为 `48/48` 输入、19 截图、167 observations、69 moving、4 swimming、21 ship-control，旧 Player 证据已标记 stale。一次 broad Pipeline eval 超时已记录并清空 console，之后 simple eval 和 error console=`0` 复核成功。

Q00 只完成“暂停点—源码—场景—构建—路线证据关系”核对，不改变 `Q03=BLOCKED_ASSET`、`Q31=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。权威节点：[Q00 RESULT](../../game/tools/results/LOCAL-QUALITY/Q00/RESULT.json) 的 `current_reaudit_20260911_q03a11`；下一步选择一个具体 D31 环境/接地 owner 或执行正式 Avatar/Animator 资源审计，源码变化后必须生成新 Player。

## 2026-09-12 Q26-A12 R03 presentation prop 接地与互补 Player 路线

本轮将下一张执行卡限定为 D31-P04/R03 的 presentation prop 接地 owner。实际审计发现分析/固定高度与当前变形 `D19_TerrainRoot` MeshCollider 不一致；修复改为按该唯一 owner 做 raycast，并按 Renderer bounds 对路缘、木桶、告示牌、台阶、街石、市场摊位墙和已支持 Pirate/Path 道具接地。无 R03 terrain 命中的 cliff 候选移出正式表现集合。修复前失败边界已保留：`Barrel_2` 偏差 `0.192m`、`NoticePost` 偏差 `0.440m`。

验证为 Q26A12R03=`1/1 PASS`、完整 Q26=`12/12 PASS`、Python 定向=`37/37 PASS`、全量=`1152/1152 PASS`、Pipeline recompile=`failed=false/errors=[]`、Editor=`ready/stopped`、error console=`0`。当前源码 Player `build_6cc1e97f82cc` 构建 `0 errors / 8 warnings`。R1 正常路线 `45/45` 输入、17 截图、122 observations、远船 reject 成立；R2 WaterProbe `48/48` 输入、19 截图、167 observations，船体位移 `5.94493293762207m`、航向 `18.02655029296875°`。两条路线互补，不能合并成单条完整矩阵；owner-not-ready 仍未证明。

截图确认港口/船控路线可运行，但低模程序化角色、泳姿、舵轮接触和岸线/地面几何仍未达到视觉门禁。保持 `Q26=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`。下一步只选一个具体 owner（正式 Avatar/动作、舵轮接触或岸线几何），源变更后重建 Player 并使用新的 route namespace。

## 2026-09-12 Q03-A12 正式 Avatar / 动作资源契约审计

本轮利用已连接的 `com.unity.pipeline@0.6.0-exp.1` 做只读资源审计，并新增 `Q03FormalAvatarActionResourceContractTests`。Pipeline 实读 D18 R03 project-owned male Avatar 为 `isValid=true / isHuman=true`，含 20 个 human bones 和 70 个 skeleton entries；D20 R02 formal appearance 具有单一有效 Humanoid Animator；D20 Player Controller 具有 15 个动作状态、15 个真实 Clip 和 6 个参数。对展平后的 Resources candidate，测试还验证 `Animator.avatar=null` 的 generic-path 边界与全部 D20 曲线路径都落在同一个直接 `Armature` 层级。

证据：资源契约 EditMode `2/2 PASS`；D20-A02 EditMode `9/9 PASS`；Q03 candidate PlayMode `2/2 PASS`；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error console=`0`。代码变更只增加资产契约测试，没有修改 `Systems/`、D2 physical root、InitialHarbor 正式 consumer 或资源所有权。权威结果：[Q03-A12 RESULT](../game/tools/results/LOCAL-QUALITY/Q03-A12/RESULT.json)；交接：[Q03-A12 handoff](design/common/handoffs/LOCAL-QUALITY/Q03-A12/Q03-A12.md)。

状态边界必须保持：这证明“候选资源契约存在并能被隔离 consumer 消费”，不等于正常 Player 已接入。正常 InitialHarbor 的 formal consumer 仍为 `0`，运行态仍 `FormalAnimatorReady=false / PoseDriverMode=Procedural`；因此 `Q03=BLOCKED_ASSET`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 不变。下一卡为 `Q03-A13`：在单一正式 D2 presentation owner 下接入资源后，验证连续动作过渡和 exact-build 原生视觉结果。
## 2026-09-12 Q03-A13 explicit formal consumer checkpoint

Q03-A13 has now been implemented and verified through the connected Unity Pipeline. A formal presentation consumer can be installed explicitly with `-d20Formal`; its PlayMode contract consumes the audited R02 resource, follows D2 `Idle -> Walk -> Run`, reaches `Board -> Disembark`, and leaves the D2 physical root as the sole movement owner. The normal InitialHarbor path remains unchanged and does not silently consume this route.

Evidence: formal consumer PlayMode `1/1 PASS`; Q03 resource contract EditMode `2/2 PASS`; D20-A02 EditMode `9/9 PASS`; candidate integration PlayMode `2/2 PASS`; D17-A06 quarantine PlayMode `1/1 PASS`; Pipeline recompile `failed=false/errors=[]`; final Editor `ready/stopped`; cleared error console `0`. Result: `game/tools/results/LOCAL-QUALITY/Q03-A13/RESULT.json`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A13/Q03-A13.md`.

This is a logic/consumer checkpoint only. The generated D20 clips remain generic transform-path clips and the runtime adapter intentionally keeps `Animator.avatar=null`; no authored Humanoid action-quality claim is made. No exact-build Windows Player screenshot route was run in this card, so Q03 remains `BLOCKED_ASSET`, VC-G01 remains `IN_PROGRESS`, VC-G02/VC-G03 remain `LOCKED`, and D31-P04/S05/G06 remain `PARTIAL`.

Next: Q03-A14 must build a separate `-d20Formal` Windows Player and collect real-input logs plus native screenshots for movement, turning, boarding/disembarking, swimming, helm contact, UI modal recovery, grounding and duplicate/legacy renderer checks. Only after that evidence may the default InitialHarbor consumer be considered.

## 2026-09-12 Q03-A14 Player build checkpoint

The connected Unity Pipeline successfully built an independent `StandaloneWindows64` Player from `MainMenu + InitialHarbor` for the explicit `-d20Formal` route. Build `build_b70c492ca267` returned `Succeeded`, `0 errors`, `8 warnings`, and `175167322` total output bytes. The artifact is `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_PLAYER_R01.exe`; EXE SHA256 is `D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`.

This build does not constitute runtime or visual acceptance. No foreground Player was started, no physical keyboard was sent, and no screenshot was counted. The current Pipeline connection is a headless/no-GPU Editor; an automatic Win32 route would take the user's desktop foreground. Q03 remains `BLOCKED_ASSET`, VC-G01 remains `IN_PROGRESS`, VC-G02/VC-G03 remain `LOCKED`, and D31-P04/S05/G06 remain `PARTIAL`. Result: `game/tools/results/LOCAL-QUALITY/Q03-A14/RESULT.json`; handoff: `docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A14/Q03-A14.md`.

Next: `Q03-A14-RUNTIME` needs an isolated native Player control surface, or explicit authorization for a short foreground route probe. It must run `-d20Formal` and archive real input, Player log, screenshots, UI modal/focus behavior, swimming/helm/grounding observations, and duplicate-renderer checks before default consumer admission is considered.

## 2026-09-12 Q00/Q01 接管复核与当前源码回归

Q00/Q01 已按当前工作区重新执行，不继承上一轮旧 Player 或旧 hash。Q00 只读采样通过同一个 Unity Pipeline Editor 完成：`InitialHarbor` 能进入 PlayMode，D2 physical root 为单位缩放且无 parent，CharacterController 为 `radius=0.2424 / height=1.8 / center.y=0.85`，水面来源为 `-0.020m`；船根 uniform scale 约 `2.573956`，D7 helm 锚点 ready。默认运行态仍是 `Procedural / FormalAnimatorReady=false / BoundBoneCount=8`，因此正式 D20 consumer 仍是显式 `-d20Formal` 候选，不是默认接入。

Q01 只读根因、尺度、owner、容差和四项缺陷边界已写入 `game/tools/results/LOCAL-QUALITY/Q01/RESULT.json` 的 `current_reaudit_20260912_q03a14`。当前状态为 `VERIFIED_WITH_SCOPE`：动作扭曲、游泳姿态/材质、握舵接触、树木接地均未被 headless 运行态升级为视觉通过。Q02/Q04 的当前源码回归已重跑：D2 动画 EditMode=`8/8 PASS`，D17 Action Quality=`16/16 PASS`（50.13s），其中 D17A07 十次浅/深水循环通过（8.4459547s）。

### 当前任务板

| 分类 | 当前事实 |
|---|---|
| 已完成（有界） | Q00 当前 source/build/runtime 关系复核；Q01 root/scale/owner/tolerance 诊断；Q02 当前动画 owner/lifecycle 回归；Q04 当前水域 owner 回归 |
| 进行中 | Q03-A14-RUNTIME formal Player 原生证据；VC-G01 视觉/运行验收；D31-P04/S05/G06 环境质量；Q31 最终候选门禁 |
| 阻塞 | Pipeline Editor 本身仍为 headless/no-GPU，但已连接的 Pipeline runtime 可以启动隐藏 GUI Player、注入 pointer/key、读取运行时 owner 状态并生成 GPU RenderTexture；尚缺可靠 foreground focus/物理 Win32 键盘和人工最终画面验收；Q03=`BLOCKED_ASSET`、VC-G02/VC-G03=`LOCKED` |
| 下一步 | 继续用隐藏 Pipeline Player 做可证实的逻辑/运行时/相机诊断；每个视觉修复只选一个 owner 并保留“不通过”的画面边界。最后才需要 foreground physical keyboard/user visual gate，不能用 `simulate_key` 或 RenderTexture 替代 |

结果与交接：Q00/Q01/Q02/Q04 各自的 current result 与 handoff 已更新。任何后续源代码变更都必须选单一 owner、重建 exact Player、重新绑定证据，不得用 Pipeline 逻辑测试或固定 token 替代真实画面。

Q05 当前复核也已完成：D17A08 随同 D17 全量 PlayMode `16/16 PASS`，其中游泳 visualRoot 倾斜、物理根保持 upright、离水 neutral 恢复通过。Q05 仍是 `VERIFIED` 的受限 whole-visual-root 契约，不是正式泳姿或原生 Player 视觉通过。下一张不依赖原生窗口的可执行卡为 Q06/Q08 代码级整合；Q03-A14-RUNTIME 仍等待隔离 Player 控制面。

Q06 当前复核同样完成：D17A09 随同 D17 全量 `16/16 PASS`，10 次跳跃落地与出生点附近正/斜/横/反向小步移动通过。Q06 仍只覆盖基础移动/跳跃稳定性，Q07 的真实岸线、码头、跳板完整路线和 Q31 视觉/原生输入边界保持开放。

Q07 当前复核完成：D17A10 随同 D17 全量 `16/16 PASS`，活动 `D19_TerrainRoot`、`DockGround`、`GangplankGround` 的启用/非 trigger 状态、真实接地和双向小步通过。Q07 仍不关闭完整视觉/Player 路线；Q08 因 Q03 formal 动作门禁继续阻塞，Q09 可独立推进。

Q09 当前复核完成：D17A11 随同 D17 全量 `16/16 PASS`，非单位船根下舵位锚点、10 次登/下船和 30m 远距拒绝通过。Q09 仍不证明自然握舵视觉接触或原生 Player 输入；下一张独立卡为 Q10 航行中甲板相对关系。

Q10—Q14 也已绑定到同一轮当前 D17 全量 `16/16 PASS`：D17A12 相对甲板误差 `0.000000`；D17A13 十次合法下船/无落点拒绝；D17A14 相机模式/真实遮挡收缩/恢复；D17A15 W/S 与 UGUI/Esc 模态输入归属；集成 D17A16 行走→入水→上岸→登船→转向→拒绝/合法下船。它们都是当前 PlayMode 逻辑/状态证据，不是原生 Player 画面；Q03 formal 动作门禁、D16 全 fixture 相邻 CameraFollow NaN、Q15 业务面板和 VC-G01 仍开放。

Q15 当前源码复核也完成：`D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`=`1/1 PASS`，I/M/T 三类面板各执行 10 次打开、重复、关闭和 Gameplay 恢复，互斥与重复拒绝通过。Q15 只收口 D29/D16 焦点逻辑，Q16 继续交易/业务状态，原生 Player 鼠标/画面仍属于 VC-G01。

Q16 当前源码复核完成：`D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`=`1/1 PASS`，D29/D3 交易 owner 的未就绪、快速双击、一次性买卖、余额不足和满仓拒绝均通过。Q16 不覆盖原生 Player 鼠标/视觉；下一张为 Q17 船坞/整备桥。

Q17 当前源码复核完成：`D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`=`1/1 PASS`，D29/D9/D4 的成功、材料不足、重复/幂等、取消、资金不足和异常恢复通过。Q17 不覆盖原生 Player 视觉；下一张为 Q18 HUD/本地化尺寸矩阵。

2026-09-12 Q18 当前源码复核完成：Unity Pipeline 的 `D31Q18HudResolutionTests` 为 `3/3 PASS`，完整 `WhalesIslandReef.D31.VisualLogic.Tests` 为 `36/36 PASS`，Python `tools.tests.test_d29_interaction_hud` 为 `30/30 OK`。Q18 继续保持 `PARTIAL`：这些结果只覆盖代码级 16:9 安全矩形和 HUD/D31/D29 逻辑，不覆盖原生 Player 截图、真实字体栅格化、英文最长文案、鼠标命中或最终视觉质量；下一张为 Q19 当前源码船员与反馈审计。

2026-09-12 Q19 当前源码复核完成：`Q19CrewInteractionAuditTests`=`1/1 PASS`，`Save.Tests`=`376/376 PASS`，`dotnet build game/AgentCore/AgentCore.csproj --nologo`=`0 warnings / 0 errors`。Q19 继续保持 `VERIFIED`，范围限于离线提议、owner 裁决、关系事实和 AgentMemory SQLite 读档恢复；不覆盖在线 LLM、native Player UI/输入或视觉门禁。下一张为 Q20 formal resource/presentation 审计。

2026-09-12 Q20 当前源码复核完成：候选棕榈 bounds/材质审计 `1/1 PASS`，Q26/A03 legacy tree containment `1/1 PASS`，日志为 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`。但 `RenderAuthoredPalms=false`、正式 Prefab 无棕榈、D18 ready/eligible=`0/0`，所以 Q20 继续 `PARTIAL/BLOCKED_ASSET`；下一张为 Q21 港口空间审计。

2026-09-12 Q21 当前源码复核完成：`R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`；4 个服务建筑各 `37 Renderer/0 missing`，6 个空间组、23 个实例碰撞体/1 个 R03 terrain collider，pairwise XZ 无重叠，稳定锚点保留。Q21 继续 `PARTIAL`，因为原生 Player 路线、镜头、鼠标命中、植被准入和最终视觉仍未覆盖；下一张为 Q22。

2026-09-12 Q22 当前源码复核完成：`R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`；水面 `96×96`、foam `28`、可见船体/船壳水线和 D9/D28 Calm-Rain-Storm 指标通过，R03 无重复 AudioSource。Q22 继续 `PARTIAL`，因为 Player 游泳、船海镜头、音效、性能、物理输入和最终视觉仍未覆盖；下一张为 Q23。

2026-09-12 Q23 当前源码复核完成：`InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`；active roots=`34`、R03/船实例=`1/1`、R03 renderers=`361`、missing material/script=`0`、terrain collider=`1`，GUID 与重开一致。Q23 继续 `PARTIAL`，不覆盖 Player 视觉/输入/性能或逐物体接地；下一张为 Q24 存档恢复诊断。

2026-09-12 Q24 当前源码复核完成：`D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`，覆盖陆地 round-trip、出海船状态/港口安全出生点和坏档事务保护。当前 schema 无独立游泳状态/姿态字段，因此 Q24 继续 `PARTIAL`，下一张为 Q25 首航/循环集成。

2026-09-12 Q25 当前源码复核完成：D30/D23 首航诊断 `2/2 PASS`，InitialHarbor route PlayMode `2/2 PASS`；运行日志的 performance approval 仍 pending。Q25 继续 `PARTIAL`，不把投影/有界路线扩大为完整人工输入或视觉通过；下一张为 Q26。

2026-09-12 Q26 当前源码全类 Pipeline 回归完成：`D19R03Q26GameplayDefectPlayModeTests`=`12/12 PASS`（14.83s）。逐项日志覆盖键盘游泳、连续水中位移、E 远距拒绝/合法登船/下船、舵轮手部距离、旧树隔离、R03 植被与泡沫唯一 owner、市场入口、海盗塔地形、船向、船员锚点和 42 个道具接地；Editor 最终 `ready/stopped`、error console=`0`。轮询期间一次连接拒绝后重连完成，未把拒绝响应当作证据。当前日志仍明确 `ph_crew_male` 未验证、Player 使用 procedural/neutral pose fallback，因此 Q26 继续 `PARTIAL`，不升级原生 Player 视觉、GPU、物理键盘或正式 Avatar/动作门禁；结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json`。下一张进入 Q27 当前性能/预算证据。

2026-09-12 Q27 当前源码 Pipeline 复核完成：D16 预算 EditMode=`6/6 PASS`；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`，连续窗口 `60.0079308s`，`234` samples。探针所有权成立、D16 budget satisfied；但连接编辑器为 `Null Device`，平均 `56.3350 FPS`、平均帧时间 `26.5407ms`、最长帧 `612.8386ms`、81 帧超过 `50ms`、GPU sample 不可用，因此仅登记为 headless runtime diagnostic。既有有效 standalone R3 继续为 `60.014019s/3465 samples/58.734367 FPS/21.460548 FPS 1% Low/622.258118ms longest frame/GPU unavailable`。Q27 继续 `PARTIAL`，没有据此做无证据优化；结果见 `game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json`，下一步 Q28 新候选构建。

2026-09-12 Q28 当前源码候选构建完成：Pipeline dry-run=`valid=true`，随后 `build_81547e384e7e` 生成新的 `Q28_CANDIDATE_PLAYER_R4.exe`，`StandaloneWindows64`、`MainMenu + InitialHarbor`、`Succeeded`、`0 errors / 1 warning`、`175167322 bytes`；EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly SHA=`4A8B4D2DE7D5C79211B85B3EE171E1CD6A182AFB41D167525985D88AE076BD33`。未启动 Player、未发送物理键盘、未采集截图；唯一 warning 为 Player 内禁用 Pipeline。Q28 继续 `PARTIAL`，结果见 `game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260912_RESULT.json`，下一步 Q29 运行路线或记录隔离控制面阻塞。

## 2026-09-12 Q29 Pipeline hidden Player 真实控制面复核

本轮把 Unity Pipeline 的 runtime 控制面用于当前主线，而不是停在“插件已安装”的能力说明。为避免接管用户桌面，构建 `Q29_RUNTIME_PIPELINE_QA_R3.exe` 为 Development Player，使用隐藏 GUI 窗口（保留 Input System，不使用 `-batchmode -nographics`）并通过 loopback runtime server 控制；构建结束后恢复 `developmentBuild=false` 和 `RuntimePipelineConfig.enableInBuilds=false`。

### 先修复真实运行暴露的输入冲突

R2 runtime state 和 Player log 交叉显示：`T` 同时被 D29 主 HUD 消费为酒馆，D3 又消费为出航；因此同一次 T 会打开酒馆并启动 `Sailing`，随后 M 的“航行中不能打开市场”拒绝是 owner 正确行为。该问题属于真实运行时输入归属缺陷，不能靠截图或静态 readiness 代替。

本轮最小修改：

- D29 酒馆继续使用 `T`。
- D3 出航默认键改为 `Y`；同步 D3 按钮、HUD、Harbor tutorial、D5 hints。
- 增加 D16 EditMode 回归，明确 `D29OpenTavern=T` 与 `D3StartVoyage=Y` 不得重叠。

### 验证

- Unity Pipeline Editor：D16Accessibility `8/8 PASS`；D16InputRouter `10/10 PASS`；D29 Q15 `1/1 PASS`；`GameplayMovementKeysDoNotNavigatePrimaryHud` `1/1 PASS`；recompile `failed=false/errors=[]`；Editor `ready/stopped`。
- Player build：`build_317c09412f0e`，`StandaloneWindows64`，`MainMenu + InitialHarbor`，`Succeeded`，`0 errors / 7 warnings`，`220836890` bytes；EXE SHA=`98751D0DFF0DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`。
- Runtime route：MainMenu Continue 的 pointer down/wait/up 进入 `InitialHarbor`；`T` 得到 `d10Focus=true/voyage=InPort`；关闭酒馆后 `M` 得到 `trade=true/voyage=InPort`；关闭市场后 `Y` 得到 `voyage=Sailing`；W 1000ms 产生真实位置变化和速度 `3.200`；E 得到 `boarded→disembarked`；I 得到 `D29WarehousePanel`；Esc 关闭、暂停和恢复均成立。
- Runtime error console=`0`；Player log 无 `ERROR/Exception/Crash/Fatal`；运行警告仍明确 `ph_crew_male` 未验证和 Player 使用 procedural fallback。

本轮因此关闭“D29/D3 T 键冲突”这一代码/运行时输入缺陷，但不关闭视觉质量：没有前台截图、物理键盘或人工画面观察，低模角色、任务动作/泳姿、握舵接触、树木/植被接地和港口几何风险仍由 Q03/Q20/Q31/VC-G01 管理。Q29 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`，D31-P04/S05/G06 保持 `PARTIAL`。

结果：`game/tools/results/LOCAL-QUALITY/Q29/RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md`。下一步仍是选择单一视觉 owner 做 bounded Player 修复/证据，再进入 Q31 最终视觉门禁；Pipeline 可继续承担隐藏 Player 的逻辑、输入归属和状态证据，不可替代人工视觉验收。

## 2026-09-12 Q26 bounded visual owner continuation

Q26 的 D7 helm anchor 已完成有界复核：D19 authored visible anchor 由 D7 消费，Q26/A05 手-舵轮距离降至 `0.2608m`，Q26/D17 targeted PlayMode 分别 `12/12`、`16/16` PASS。随后 D27/D20 NPC material owner 已把 16 个 built-in `Standard` 材质迁移为 URP Lit；hidden Player 运行时盘点 `123/123` slots 为 URP Lit，错误材质为 `0`，close GPU frame 无 magenta。

这两项只关闭舵轮接触测量和 NPC shader 缺陷，不升级 Q26 或 VC-G01 的完整视觉状态。formal R02 action/avatar、程序化低模、游泳姿态、树木/地面接地、港口几何和最终人工视觉门仍开放。下一张应选择一个单一 owner，优先 formal R02 Avatar/action/material readiness 或 geometry/grounding，不再无证据重复调坐标。

权威结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_D7_HELM_ANCHOR_REVALIDATION_PIPELINE_20260912_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q26/Q26_D27_NPC_URP_MATERIAL_REPAIR_PIPELINE_20260912_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q26/RESULT.json`。

2026-09-12 Q03-A14 R6 open-sea route bounded continuation：hidden Player 复现并修复了已有 `open_sea` 存档重开 `InitialHarbor` 时覆盖 D19 作者船根、让船壳挡住出生点正向路线的问题。`D2ShipController` 现在只在非 `InitialHarbor` 应用 open-sea snapshot；`Q26A13OpenSeaSaveRestoresAuthoredHarborShipRoute` 注入/恢复静态存档并验证 `ph_ship_fishing z=-5.2/yaw=180` 与船壳不挡路。Pipeline `build_997e00e6bc28`=`0 errors / 7` 条既有弃用 warning；focused hidden Player PID=`46228` 真实 S/W 路线达到 `_isSwimming=true`，runtime error console=`0`，clean close；Q26=`13/13 PASS`、D17=`16/16 PASS`、recompile 无错误。随后 hidden relaunch 证明没有可靠 focus command：D7 `Modal=FocusLost/CharacterInputAllowed=false` 时 simulate_key Success 不等于玩法消费，已从证据中排除。R6 结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R06_OPEN_SEA_ROUTE_PIPELINE_20260912_RESULT.json`；Q03-A14/Q26 继续 `PARTIAL`、Q03=`BLOCKED_ASSET`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一步仍选择 formal Avatar/action 或 geometry/grounding 单一 owner，并把 hidden Player focus 条件单独纳入 VC-G01 运行矩阵。

2026-09-12 Q03-A14 R4 formal generic arm pose bounded continuation：对 generic-path placeholder 的 Idle/Walk/Run/常见交互/上下船状态增加幂等的 presentation-only neutral shoulder drop；暂停时跳过，Fish/Helm 不纳入。D20A02=`10/10 PASS`、Q03 formal=`1/1 PASS`、Q26=`12/12 PASS`、D17=`16/16 PASS`；最新 `build_9918fec2b2fc` 的 hidden GUI Player `Q03_A14_FORMAL_PIPELINE_QA_R4_ARM_POSE_DEV2.exe` 为 `0 errors / 0 warnings`，真实 Pipeline runtime 证明 MainMenu→InitialHarbor、formal bridge generic 安装、Idle/Walk 手部低于上臂、E→Ship/Helm 边界、runtime error console=`0` 和 clean close，GPU frame 已归档。该卡只关闭普通状态的横向 T-Pose-like 风险，不关闭 formal authored action/avatar/material readiness、Fish/Helm IK、自然泳姿、接地/穿模或港口最终视觉；`Q03=BLOCKED_ASSET`、`Q26=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 保持。结果：`game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_FORMAL_R04_ARM_POSE_PIPELINE_20260912_RESULT.json`；下一步选 formal authored action/Avatar 或 geometry/grounding 的单一 owner。

## 2026-09-12 Q26-A14 R03 shoreline collision-owner repair

本轮从 Q26/A14 的真实 PlayMode 失败开始，而不是继续猜坐标：静态合批后，D19 可见地形的 `MeshFilter.sharedMesh` 变成 `Combined Mesh (root: scene) 4`，D6 将它写入 `D19_TerrainRoot` 后，`ShoreRock_0` 的碰撞命中点高于可见底部 `0.290m`。修复限定在 D31-P04 物理/接地 owner：`HarborCollisionController` 保留场景已保存的 `D19_TerrainRoot` MeshCollider 网格，只同步 Transform，不再从静态合批后的视觉网格重建碰撞；Builder 继续对岸线岩石使用同一自有 terrain owner。

证据：修复后 terrain mesh=`D19_R03_IslandSurfaceMesh`；9/9 岸线岩石接地误差=`0.018–0.022m`，最大=`0.022m`；Q26A14=`1/1 PASS`；完整 Q26=`14/14 PASS`；Pipeline recompile=`failed=false/errors=[]`；Player build=`build_0cc514c48042`，`0 errors / 8 warnings`，EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；Editor=`ready/stopped`、error console=`0`。

状态边界：本卡没有启动 Player、没有前台输入、没有新增截图，只关闭一个可复现的运行时碰撞/接地缺陷。`Q26=PARTIAL`、`D31-P04/S05/G06=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED` 不变。下一步继续 VC-G01 隔离 Player 真实交互/画面闭环，或选择另一个单一 owner 缺陷；不把 Q26 PlayMode 或构建结果扩大为完整视觉 PASS。

结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_PIPELINE_20260912_RESULT.json`；构建：`game/tools/results/LOCAL-QUALITY/Q26/Q26_A14_SHORELINE_GROUNDING_BUILD_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md`。

## 2026-09-12 Q04 R2 与 Q31 Pipeline hidden Player 控制面续接

Q04 通过已连接 Unity Pipeline 对当前源码的 D17A07 水域测试做了独立定向复核：`1/1 PASS`，`9.45s`，Editor 最终 ready/stopped、error console=`0`。该结果只确认 D2 水域 owner 的进入、移动、退出和 grounded recovery，不改变 Q04 之外的视觉边界。

Q31 随后使用当前源码构建并运行隐藏 GUI Development Player：MainMenu→InitialHarbor、W/S 移动、A 转向、shoreline entry/exit、E 登船/下船、I 仓库、T 酒馆重复/关闭、Esc modal 与暂停恢复均取得 runtime owner 状态证据，runtime error console=`0`，clean close。存档已处于 `Sailing`，因此 M 的本轮结果是合法拒绝；此前 InPort 成功只保留在 Q29 R3 的独立证据中。

这轮把“插件能否承担当前主线逻辑/输入证据”从假设变成了当前构建上的有界事实，但仍没有前台物理键盘、用户视角截图或最终视觉验收。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。下一步选择一个单一 owner 缺陷做可验证修复；不再盲目调整视觉坐标，也不把 Pipeline runtime 状态扩大成 Q31 PASS。

结果：`game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260912_R2_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_RUNTIME_RECHECK_20260912_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q04/Q04.md`、`docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md`。

2026-09-12 Q31 Pipeline camera diagnostic：当前候选 hidden Player 通过 RenderTexture 取证生成四张相机帧，并以 targeted Q26A05 `1/1 PASS` 复核 `HelmWheel` 与左右 fallback hands 的距离均为 `0.2608m`。这把“舵位是否存在/owner 是否对齐”与“用户是否接受动作语义”分开：前者有运行证据，后者仍被低模/占位动作和粗糙画面阻塞。游泳路线帧未达到可判定近景，不把“状态进入水域”升级成自然泳姿通过。结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_CAMERA_DIAGNOSTIC_20260912_RESULT.json`；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。下一步选择 formal Avatar/action 资源 owner 或经批准的 candidate-only 视觉集成，避免继续盲调坐标。

2026-09-12 Q20 Pipeline Player tree containment：当前 Q31 候选 runtime eval 看到 `ph_palm_tree` root 仍 active，但 `25/25` 子 Renderer 均 `enabled=false`；D26 `vegetation_grounded=26`、`vegetation_hidden_without_terrain=0`，Player clean close。该诊断把当前旧树是否仍在画面中的疑点收窄为“当前候选已隔离旧 Renderer”，但不打开 `RenderAuthoredPalms`，也不改变 Q20 的 `PARTIAL/BLOCKED_ASSET`，因为正式植被资源和全镜头接地仍未准入。结果：`game/tools/results/LOCAL-QUALITY/Q20/Q20_PIPELINE_PLAYER_TREE_CONTAINMENT_20260912_RESULT.json`。

## 2026-09-12 Q03-A14 D20 readable skin owner repair

本轮使用 Unity Pipeline hidden GUI Player 继续 VC-G01 当前主线，但选择了一个边界明确的 presentation owner，而不是继续猜测坐标。D20 Builder/bridge 为 Candidate/Formal 的 `SuperHero_Male` 使用项目拥有的 URP Lit `D20_PlayerSkin_Warm`；D5/D7 对 bridge-owned renderer 只读跳过，避免通用材质 polish 覆盖 D20 skin。Formal 测试同时等待初始 `Land→Idle`，修复了真实出现的首帧 flaky assertion。

证据：D20A02=`10/10 PASS`、Q03D20=`3/3 PASS`、D19R03Q26=`14/14 PASS`、D17=`16/16 PASS`、recompile=`failed=false/errors=[]`。R4 hidden Player Candidate/Formal 均报告 `D20_PlayerSkin_Warm / URP Lit / RGBA(0.720,0.420,0.250,1.000)`，W=`Walk`，Formal E=`ph_ship_fishing`，runtime errors=`0`，clean close；四张 GPU RenderTexture 帧登记在 `game/tools/results/LOCAL-QUALITY/Q03-A14/Q03_A14_SKIN_OWNER_PIPELINE_R4_20260912_RESULT.json`。

边界：Candidate 黑色剪影改善，但 Candidate outfit、Formal outfit/action/avatar、Helm IK/比例、自然泳姿、树木/地面接地、穿模、港口几何、用户 GPU 性能和人工视觉仍未通过。Q03=`BLOCKED_ASSET`、Q03-A14/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。下一步只能选择 formal 完整 outfit/action 资源或 Helm geometry/IK/scale 一个 owner。

## 2026-09-12 Q26-A15 EditMode 契约同步

Q26-A14 的碰撞 owner 修复后，全量 EditMode 初跑为 `629/626/3`；失败保留为旧测试夹具没有 authored `D19_TerrainRoot` `MeshCollider`，以及植被测试把 `RenderAuthoredPalms` 的 `const` 语法写死。本轮只修改 `VC_A02CollisionAlignmentTests.cs`、`VC_G01WorldLogicTests.cs`：加入独立 terrain collider mesh、断言其保留且不取代视觉 mesh，并将植被断言改为稳定值契约。

定向测试为 `1/1` 与 `14/14 PASS`，全量 Unity EditMode 恢复为 `629/629 PASS`；recompile `failed=false/errors=[]`，Editor `ready/stopped`，error console=`0`。`HarborCollisionController` 未改动。结果见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_A15_COLLISION_CONTRACT_TEST_SYNC_20260912_RESULT.json`；Q26 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`，该证据不关闭 native Player/人工视觉门。

## 2026-09-12 Q00/Q01/Q02 Luna 当前接管、根因表与回归

本轮重新执行 Q00→Q01，并继续完成 Q02 聚焦回归，未清理工作区。Q00 当前 `git status --short` 为 `5361` 行（`459` tracked/staged、`4902` untracked），`git diff --shortstat` 为 `459 files changed, 39556 insertions(+), 48895 deletions(-)`；分支仍为 `agent/publish-project-changes-20260813`、HEAD=`01024cae...`。Unity Pipeline/Editor 最终 `ready/stopped`，RuntimePipelineConfig=`enableInBuilds=false/port=0`，清空后 error console=`0`。

Q01 当前 connected Editor facts：`D2_PlayerAvatar=(0.000,0.150,-2.700)`、unit root；`HumanoidVisual` localScale=`0.6642`、renderer=`17`；CharacterController=`0.242435/1.800/center.y=0.847`；默认 `Procedural/FormalAnimatorReady=false/BoundBoneCount=8`；ship scale=`2.574`；`D7_HelmPoint` 与 visible anchor=`(0,1.100,-6.680)`。直接对象查询显示 `D19_TerrainRoot` parent=`D19_EnvironmentRoot` 且 MeshCollider descendants=`0`，可见 `D19_R03_IslandSurface` 为静态合批 mesh；该差异只进入 Q07/Q26 复核，不升级接地 PASS。四项用户缺陷仍分别受 formal action/avatar、游泳视觉、握舵接触和正式植被资源门禁约束。

Q02 当前 `D2CharacterAnimationControllerTests=8/8 PASS`。相关 D17 全量第一次为 `15/16`，唯一失败 D17A16 游泳镜头（camera-player horizontal sqrDistance=`0.0221495014`）；同测试单独 `1/1 PASS`，第二次全量 `16/16 PASS`。该间歇性信号被记录为 Q12/Q26-A20 watch item，不以一次失败直接改 CameraFollow，也不把它从历史中抹掉。

当前任务板不升级：Q02=`PASS_WITHIN_SCOPE`；Q03=`BLOCKED_ASSET`、Q20/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`；最终 native foreground keyboard/manual visual=`WAITING_VISUAL`。下一步只选择 Q12/Q26-A20 camera isolation 或 Q07/Q26 TerrainRoot MeshCollider owner revalidation 中的一张单一 owner 卡。

权威记录：`game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_20260912_LUNA_R1_RESULT.json`、`Q01/Q01_CURRENT_ROOT_CAUSE_20260912_LUNA_R1_RESULT.json`、`Q02/Q02_CURRENT_REVALIDATION_20260912_LUNA_R1_RESULT.json`；各父 `RESULT.json`、handoff 和 `STATUS.md` 已同步。

## 2026-09-12 Q26-A16 R03 terrain owner 运行时复核

Q01 采样中的 D19_TerrainRoot 差异已用当前 connected Editor 重新核对：InitialHarbor 存在两个同名节点。旧的 D19_EnvironmentRoot/D19_TerrainRoot 是禁用的 legacy TerrainCollider；D19_R03_HarborPresentation/D19_TerrainRoot 才是 HarborCollisionController 使用的实际 owner，启用、非 Trigger，MeshCollider.sharedMesh=D19_R03_IslandSurfaceMesh。可见 D19_R03_IslandSurface 在同轮静态合批后为 Combined Mesh (root: scene) 4，碰撞 owner 没有被视觉合批网格替换；Harbor runtime state 为 IsInitialized=True、R03SurfaceInstalled=True、ColliderCount=5、WalkableSurfaceCount=3。

本卡只新增 D19R03Q26GameplayDefectPlayModeTests.cs 的 Q26A16HarborCollisionUsesScopedR03TerrainOwner 回归断言：查找限定在 R03 presentation hierarchy，禁止把全局 GameObject.Find(D19_TerrainRoot) 当作 owner proof，并记录同名 legacy node。最终源码 Pipeline 定向=1/1 PASS，完整 Q26 PlayMode=15/15 PASS，recompile failed=false/errors=[]，Editor ready/stopped，error console=0。运行时 HarborCollisionController 与 R03 builder 未修改。

状态边界：Q26-A16 关闭同名节点误读并保护已有 collision owner，不关闭 native Player 物理前台键盘、截图、formal Avatar/action、自然泳姿/握舵、完整岸线路线或 VC-G01。Q26 保持 PARTIAL，Q07 保持既有范围 VERIFIED，VC-G01 保持 IN_PROGRESS，VC-G02/VC-G03 保持 LOCKED，D31-P04/S05/G06 保持 PARTIAL。下一步转向 formal action/Avatar 或 swim/helm presentation 的另一个单一 owner，不在没有新失败证据时继续盲调 terrain 坐标。

结果：game/tools/results/LOCAL-QUALITY/Q26/Q26_A16_TERRAIN_OWNER_RUNTIME_REVALIDATION_20260912_RESULT.json；父结果：game/tools/results/LOCAL-QUALITY/Q26/RESULT.json；交接：docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md。

## 2026-09-12 Q03-A15 正式动作资源准入门禁

本轮把 Q03-A14 的资源阻塞转成可持续回归的门禁，而不是用 generic-path 诊断结果冒充正式动作完成。新增 `Q03FormalActionAdmissionGateTests`，对当前 D20 controller 统计 `15` 个动作 clip：`humanMotion=0`、`hasGenericRootTransform=15`，因此明确保持 formal action admission closed。

验证：Unity Pipeline 门禁 `1/1 PASS`；`D20.Tests.A02` 全程序集 `13/13 PASS`；Pipeline `recompile=status=up_to_date`；Editor `ready/stopped`；清空后 error console=`0`。本卡没有修改产品逻辑、场景、Prefab、资源准入标记或默认 consumer。

状态边界：Q03 仍为 `BLOCKED_ASSET`，Q03-A15 的 PASS 只表示“阻塞判断有回归保护”；Q26/Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`，VC-G02/VC-G03 仍 `LOCKED`。下一步等待合法 authored Humanoid action/avatar 资源，或选择另一个有真实失败证据的几何/接地或 fallback presentation owner；停止无证据的 generic 动作角度试调。

结果：`game/tools/results/LOCAL-QUALITY/Q03-A15/RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A15/Q03-A15.md`。

## 2026-09-12 Q20-A03 运行时植被接地与旧棕榈隔离守卫

本轮把用户报告的“树在空中飘”中可由当前源码证明的部分收窄为运行时守卫：新增 `D19R03Q20VegetationPlayModeTests`，限定 R03 自有 `D19_TerrainRoot`，对当前 `D19_R03_VegetationAndRocks` 下的可见 Renderer 做 bounds 向下接触采样，并显式断言旧 `ph_palm_tree` 子 Renderer 保持隔离。

真实证据：PlayMode `1/1 PASS`，`visibleR03Vegetation=24`、`grounded=24`、`failures=0`、`legacyPalmRoots=5`、`activeLegacyPalmRenderers=0`；父 EditMode `D19R03Q20VegetationTests=1/1 PASS`；Editor=`ready/stopped`，error console=`0`。结果和原始留痕见 `game/tools/results/LOCAL-QUALITY/Q20-A03/RESULT.json`、`Q20_A03_PIPELINE_PLAYMODE.xml`、`Q20_A03_PIPELINE_PLAYMODE.log`。

边界不变：该守卫证明当前候选的逻辑接触与旧树 quarantine，不证明用户视角所有镜头的接地、遮挡、穿模或正式棕榈资源完成。D18 ready catalog 仍 `BLOCKED / 0 / 0`；Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。下一步按依赖推进 Q21，不打开未准入资源。

## 2026-09-12 Q13-A02 UI 导航策略再断言

针对用户报告的 W/S 与右上角菜单联动风险，本轮在 `HarborInteractionStabilizer` 的 UI owner 内补上可观察状态检查：即使 `sendNavigationEvents` 没有改变，只要 `InputSystemUIInputModule.move` 被其他 UI owner 恢复，游戏态也会再次执行清除策略。新增 PlayMode 回归覆盖“外部恢复 UI/Navigate 后一帧再次清除”。

证据：定向 `D16PrimaryHudNavigationPlayModeTests=2/2 PASS`（4.87s）；当前 D17 全量=`16/16 PASS`（100.67s）；当前完整 `D16InputRouterPlayModeTests` 连续两次=`10/10 PASS`（12.58s、13.2s），未再复现历史 CameraFollow NaN；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error console=`0`。结果：`game/tools/results/LOCAL-QUALITY/Q13-A02/RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q13-A02/Q13-A02.md`。

状态边界：Q13 仍为 `VERIFIED`，本卡是 UI 输入策略防回归；历史 `9/10` CameraFollow NaN 证据仍保留为间歇性 watch item，不因两次当前通过而永久关闭。本卡不是前台物理键盘或最终视觉验收。Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持。下一步按依赖进入 Q14，并继续保留人工/原生 Player 视觉门。

## 2026-09-12 Q26-A17：生成舵轮高度对齐

本轮从 Q31 近景 owner 测量继续推进一个明确的 D19 presentation 缺陷。修复前生成 `HelmWheel` 中心 `y=1.72m`，fallback 左右手中线 `y=1.894419045m`，垂直误差 `0.174419045m`；该差距对应用户看到的“人物在控制一个无关物体”的视觉语义风险。

最小变更限定在 `D19R03HarborPresentationBuilder.EnsureVisibleHelm`：生成 `HelmStand` 高度由 `0.70m` 改为 `0.82m`，`HelmWheel` 与 `HelmWheelHub` 中心 y 改为 `1.89m`。D2/D7 gameplay anchor、船舶物理、Systems 和 AgentCore 未改。新增 `Q26A17GeneratedHelmWheelSharesFallbackHandHeight`，在真实 InitialHarbor PlayMode 登船后测量轮心与双手中线，要求误差 `<0.06m`。

### 验证与边界

- 修复后定向测试=`1/1 PASS`；真实日志 `generatedHelmWheelY=1.8900`、`handMidpointY=1.8944`、`verticalError=0.0044`。
- 完整 `D19R03Q26GameplayDefectPlayModeTests`=`16/16 PASS`，`0 failed / 0 skipped / 0 inconclusive`，`51.85s`；Pipeline recompile=`failed=false/errors=[]`。
- Windows Player build=`build_cfa950c379f9`，`StandaloneWindows64`，`0 errors / 8 warnings`；EXE SHA=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA=`0C03B7C8F8FF7CACE7E2B492FBA16767120FB86F78BC0FD9081F7749856C1C28`；Editor 最终 `ready/stopped`，error console=`0`。
- Editor-only 当前源 RenderTexture 尝试产生均匀灰帧，登记为 `INVALID_GRAY_FRAME`，不计入视觉证据；本轮没有前台物理 Win32 键盘和人工视觉验收。

Q26、Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`，Q03 继续 `BLOCKED_ASSET`。下一步选择新的真实失败 owner，优先 formal Avatar/action admission 或已验证的 hidden Player runtime camera surface；没有新测量时不再继续调舵轮坐标。结果：[Q26-A17](game/tools/results/LOCAL-QUALITY/Q26-A17/RESULT.json)；交接：[Q26-A17 handoff](game/design/common/handoffs/LOCAL-QUALITY/Q26-A17/Q26-A17.md)。

## 2026-09-13：Q03-A16 Formal Humanoid pose adapter与R2 Player运行证据

本轮继续使用 Unity Pipeline hidden GUI Player，把“动作扭曲/游泳/握舵”收敛为一个可验证的 D20 Presentation owner。新增 `-d20FormalHumanoid` opt-in 路径，保留 R02 Humanoid Avatar wrapper，以 `HumanPoseHandler` 提交站立、移动、游泳、上下船和舵位轮廓；不写 D2 physical root，不打开当前 generic action admission。

验证结果：新 Humanoid PlayMode=`1/1 PASS`，Q03D20=`4/4 PASS`，Q03Formal=`3/3 PASS`；R2 Windows Player=`build_d87c4e4b5265`，`0 errors / 2 warnings`。隐藏 Player 真实输入路线进入 `Swimming=True/Fish/pitch=70`；正常 `E` 登船后，左右手到 `HelmWheel` 距离为 `0.219m/0.222m`。两张 R2 GPU 帧、运行日志、EXE 与 Assembly-CSharp hash 已写入独立结果 JSON。

该卡仅为 `PASS_WITHIN_SCOPE`：当前 15 个 formal clips 仍 `humanMotion=0/genericRoot=15`，故 Q03 继续 `BLOCKED_ASSET`；画面仍为低保真范围证据，不能证明自然划水、完整 IK、正式 outfit/action、树木/地面接地、穿模消失、用户 GPU 性能、前台物理键盘或人工最终视觉。Player 已优雅退出，Pipeline 设置已恢复 `enableInBuilds=false/port=0`。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A16/Q03_A16_FORMAL_HUMANOID_PIPELINE_R2_RESULT.json`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A16/Q03-A16.md`。Player 日志中的 1 条工具 Error 为误调用不存在的 `get_console_logs` 命令，已保留为边界证据；无 Exception/Crash/Fatal。

## 2026-09-13 Q00/Q01/Q02 当前接管与 Pipeline 回归

本轮从 Q00 重新核对暂停点并继续执行 Q01、Q02，没有清理当前 dirty worktree。Q00 当前 status=`10263`（tracked=`459`、untracked=`9804`），分支/HEAD 未变；Q03-A16 R2 的三个源文件 hash、Player SHA256 和 Assembly-CSharp SHA256 与结果文件完全对应。旧 Editor 因 D3D11 GPU timeout 终止后，同项目 `-nographics` Editor PID=`53332` 恢复 Pipeline，最终 `ready/stopped`、console=`0`、RuntimePipeline=`enableInBuilds=false/port=0`。

Q01 使用 connected Pipeline `open_scene -> editor_play -> targeted eval -> editor_stop` 实读 InitialHarbor：D2 root=`(0.0000,0.1497,-2.7000)`、unit scale；HumanoidVisual scale=`0.6642`、17 renderers；capsule=`0.2424/1.8000/0.8469/skin0.0194`；默认表现=`Procedural/FormalAnimatorReady=false/Bones8`；ship scale=`2.5740`；D7 helm/visible anchor=`(0,1.1,-6.68)`；scoped R03 MeshCollider 为 enabled、非 Trigger、shared=`D19_R03_IslandSurfaceMesh`。四类视觉缺陷及 W/S-HUD 风险均按“逻辑可达 ≠ 前台视觉通过”记录，未升级状态。

Q02 在同一 Pipeline Editor 重跑当前源码：D2 动画 EditMode=`8/8 PASS`；D17 Action Quality async PlayMode=`16/16 PASS`、`56.79s`、0 failed/skipped/inconclusive，包含十次浅/深水循环、游泳 visualRoot/物理根分离、上/下船拒绝和 Gameplay/UI 输入归属。Q02 仍只为 `PASS_WITHIN_SCOPE`，不替代 authored action、physical keyboard 或人工视觉门。

权威结果和交接：[Q00](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A16_20260913_RESULT.json)、[Q01](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_20260913_RESULT.json)、[Q02](../game/tools/results/LOCAL-QUALITY/Q02/Q02_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json)；[Q00 handoff](design/common/handoffs/LOCAL-QUALITY/Q00.md)、[Q01 handoff](design/common/handoffs/LOCAL-QUALITY/Q01.md)、[Q02 handoff](design/common/handoffs/LOCAL-QUALITY/Q02/Q02.md)。下一张可执行卡：Q04 当前水域 owner 回归。

## 2026-09-13 Q04 当前 Pipeline 定向复核

Q04 在 Q00/Q01/Q02 接管序列后再次使用已连接的 Unity Pipeline Editor 定向执行 D17A07。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `11.1657914s`、总耗时 `11.26s`；测试内部覆盖浅水岸缘 10 次和深水 10 次进入—移动—退出，并继续断言水面高度、非零游泳位移和 grounded recovery。

本轮没有修改产品代码，当前三项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q04 handoff。Q04 保持 `VERIFIED`，状态边界仍是 D2 水域 owner；原生 Player W/S、正式泳姿、自然游泳、水面材质、截图视觉和 VC-G01 不因此通过。下一步进入 Q05 当前 Pipeline 定向复核。

## 2026-09-13 Q05 当前 Pipeline 定向复核

Q05 在 Q04 定向水域复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A08。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `12.9665234s`、总耗时 `13.06s`；测试内部覆盖 10 次游泳循环，断言 visualRoot 游泳倾斜、物理根 upright 和离水 neutral 恢复。

本轮没有修改产品代码，当前三项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q05 handoff。Q05 保持 `VERIFIED`，状态边界仍是受限的 whole-visual-root 契约；正式四肢/头部动作、自然泳姿、原生 Player W/S、截图视觉和 VC-G01 不因此通过。下一步进入 Q06 当前 Pipeline 定向复核。

## 2026-09-13 Q06 当前 Pipeline 定向复核

Q06 在 Q05 定向游泳姿态复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A09。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `13.8529193s`、总耗时 `13.95s`；测试内部覆盖 10 次真实跳跃—落地循环，以及出生点附近正向、斜向、横向和反向小步移动。

本轮没有修改产品代码，当前三项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q06 handoff。Q06 保持 `VERIFIED`，状态边界仍是跳跃/接地/小步移动稳定性；墙、坡、岸线、跳板、船甲板完整碰撞路线、原生 Player W/S、截图视觉和 VC-G01 不因此通过。下一步进入 Q07 当前 Pipeline 碰撞路线复核。

## 2026-09-13 Q07 当前 Pipeline 定向复核

Q07 在 Q06 定向接地复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A10。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `4.6148297s`、总耗时 `4.71s`；测试内部覆盖活动 `D19_TerrainRoot`、`DockGround`、`GangplankGround` 的启用/非 Trigger 状态，并经 CharacterController 执行双向 8 步小步路线。

本轮没有修改产品代码，当前三项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q07 handoff。Q07 保持 `VERIFIED`，状态边界仍是当前自动化地形/码头/跳板路线；Q08 继续受 Q03 formal 资源阻塞。下一步先登记 Q08 阻塞，再进入 Q09 当前 Pipeline 甲板/舵位复核。

## 2026-09-13 Q08 当前依赖复核：保持阻塞

Q08 沿依赖顺序复核后仍不能运行完整整合门：Q03 当前 15 个 formal clip 继续为 `humanMotion=0 / genericRoot=15`，默认 InitialHarbor formal consumer 仍为 `0`，状态为 `BLOCKED_ASSET`。Q05 当前 D17A08 和 Q07 当前 D17A10 均已在同一 connected Pipeline 上 `1/1 PASS`，但它们只证明有界的泳姿根分离和地形/码头/跳板碰撞，不能替代 authored action 资源。

因此本轮明确记录“不运行 Q08 整合测试”的决定，不把 neutral-only、generic diagnostic 或固定 token 当作完整输入→状态→位移→姿态证据。Q08 结果与交接已更新；解除条件仍为合法 authored ground-action 资源或可审计 Avatar/骨骼轴适配器，并在同一源码边界重新执行 Q08。该阻塞不阻止独立 Q09，下一步进入 Q09 当前 Pipeline 甲板/舵位复核。

## 2026-09-13 Q09 当前 Pipeline 定向复核

Q09 在 Q08 阻塞登记后继续使用已连接的 Unity Pipeline Editor 执行 D17A11。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `4.7154567s`、总耗时 `4.81s`；测试内部覆盖非单位船根的 HelmPoint/可见舵位锚点、10 次登船—下船生命周期和 30m 远距登船拒绝。

本轮没有修改产品代码，当前四项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q09 handoff。Q09 保持 `VERIFIED`，状态边界仍是缩放锚点/生命周期/拒绝；自然握舵、航行中甲板关系、原生 Player、截图视觉和 VC-G01 不因此通过。下一步进入 Q10 当前 Pipeline 甲板相对关系复核。

## 2026-09-13 Q10 当前 Pipeline 定向复核

Q10 在 Q09 定向锚点复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A12。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `3.2831358s`、总耗时 `3.38s`；测试内部覆盖 10 个船舶平移/转向快照，并根据船根 TransformPoint/rotation 断言玩家相对甲板位置和旋转稳定。

本轮没有修改产品代码，当前四项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q10 handoff。Q10 保持 `VERIFIED`，状态边界仍是 snapshot-driven 甲板随动；开放海、原生 Player、镜头、自然握舵、截图视觉和 VC-G01 不因此通过。下一步进入 Q11 当前安全下船复核。

## 2026-09-13 Q11 当前 Pipeline 定向复核

Q11 在 Q10 定向甲板随动复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A13。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `5.2898463s`、总耗时 `5.39s`；测试内部覆盖 10 次合法下船、基于真实活动碰撞 bounds 的无落点拒绝，以及模式/parent/碰撞/grounded 恢复。

本轮没有修改产品代码，当前四项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q11 handoff。Q11 保持 `VERIFIED`，状态边界仍是合法/无落点下船；移动船碰撞、全岸线出口、原生 Player、正式动作、相机、截图视觉和 VC-G01 不因此通过。下一步进入 Q12 当前 Pipeline 相机复核。

## 2026-09-13 Q12 当前 Pipeline 定向复核

Q12 在 Q11 定向安全下船复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A14。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `5.0176968s`、总耗时 `5.11s`；测试内部覆盖 10 次 Character/Overview/ShipDeck 模式与目标切换、受控外部 BoxCollider 的实际 SphereCast 遮挡收缩和移除后的阻尼恢复。

本轮没有修改产品代码，当前四项源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q12 handoff。Q12 保持 `VERIFIED`，状态边界仍是 CameraFollow 逻辑；最终场景构图、渲染穿模、原生 Player、截图视觉和 VC-G01 不因此通过。下一步进入 Q13 当前 Pipeline 输入焦点复核。

## 2026-09-13 Q13 当前 Pipeline 定向复核

Q13 在 Q12 相机复核后继续使用已连接的 Unity Pipeline Editor 执行当前源码输入回归：D17A15 `1/1 PASS`（测试 `4.4704041s`、总计 `4.57s`），D16InputRouterPlayModeTests `10/10 PASS`（总计 `6.0s`），均无 failed/skipped/inconclusive。

本轮没有修改产品代码，当前 D17/D16 测试及 D16InputRouter、HarborInteractionStabilizer、RuntimeHudCoordinator 源 hash、Pipeline 状态和清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q13 handoff。Q13 保持 `VERIFIED`，只证明 Unity Input System PlayMode 的 W/S—UI 导航隔离和暂停让权；不证明物理 Win32 键盘、渲染 HUD、E/I/M/T 业务或 VC-G01。下一步进入 Q14 当前船岸整合复核。

## 2026-09-13 Q13 当前 Pipeline 定向复核 R2

本轮没有修改产品代码；使用当前连接的 Unity Pipeline Editor 重跑 D17A15 与完整 D16 输入路由 fixture。D17A15=`1/1 PASS`（测试 `4.3906842s`、总计 `4.51s`），D16=`10/10 PASS`（总计 `5.62s`），无 failed/skipped/inconclusive；Editor=`ready/stopped`，error console=`0`。

本轮结果只更新当前源码证据，不扩大状态边界：Q13 仍为 `VERIFIED`，证据限于 Unity Input System PlayMode 的 W/S—UI 导航隔离、Esc 模态恢复、合成键盘/手柄和重绑定；前台物理键盘、渲染 HUD、E/I/M/T 业务、native Player 视觉与 VC-G01 仍开放。权威结果为 `game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q14 R2。

## 2026-09-13 Q14 当前 Pipeline 定向复核

Q14 在 Q13 输入 owner 复核后继续使用已连接的 Unity Pipeline Editor 执行 D17A16 集成路线。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `4.4093881s`、总耗时 `4.51s`；测试内部覆盖行走→入水→上岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法下船。

本轮没有修改产品代码，当前 D17/D2/D7/CameraFollow/D16 源 hash、Pipeline 测试状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q14 handoff。Q14 保持 `VERIFIED`，但 `imported_neutral` 仍不是 formal authored action 通过；物理 Win32、E/I/M/T、截图视觉和 VC-G01 不因此通过。下一步进入 Q15 当前面板焦点/恢复复核。

## 2026-09-13 Q14 当前 Pipeline 定向复核 R2

本轮没有修改产品代码；使用当前连接的 Unity Pipeline Editor 重跑 D17A16 船岸整合链，结果 `1/1 PASS`（测试 `3.3597823s`、总计 `3.48s`），无 failed/skipped/inconclusive；Editor=`ready/stopped`，编译/域重载均为 false，error console=`0`。

本轮只更新当前源码整合证据，不扩大状态边界：Q14 仍为 `VERIFIED`，证据限于行走→入水→上岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法下船的有界状态与 owner 链；`imported_neutral` 仍使正式动作质量受 Q03 阻塞，前台物理键盘、E/I/M/T、最终视觉和 VC-G01 仍开放。权威结果为 `game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q15 R2。

## 2026-09-13 Q17 当前 Pipeline 定向复核

Q17 在 Q16 交易 owner 复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）执行 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `4.1822541s`、总耗时 `4.28s`；测试内部覆盖修船成功、材料不足、重复/域幂等、取消、船坞成功/资金不足、异常恢复和主输入恢复。

本轮没有修改产品代码，实际 Console marker、当前 D29/D9/D4 源 hash、Pipeline 状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q17 handoff。marker 检查前共有 `88` 条日志、`0` 条 Error；预期的空 owner 异常路径仅记录 Warning 并由测试确认恢复。Q17 保持 `VERIFIED`，只限业务 owner 桥接；物理键鼠、原生 Player、最终视觉、性能、完整首航和 VC-G01 仍不在此证据范围。下一步进入 Q18 当前 HUD/本地化尺寸矩阵复核。

## 2026-09-13 Q18 当前 Pipeline 定向复核

Q18 在 Q17 业务 owner 复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）完成当前代码级 HUD/视觉逻辑复核：`D31Q18HudResolutionTests` 为 `3/3 PASS`（1280×720、1920×1080、2560×1440；`0.02s`），完整 `WhalesIslandReef.D31.VisualLogic.Tests` 为 `36/36 PASS`（`0.03s`）。随后从项目根执行 `python -m unittest tools.tests.test_d29_interaction_hud`，结果为 `30/30 OK`。

本轮没有修改产品代码，当前源 hash、Pipeline 状态、Editor `ready/stopped` 和 Console 清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q18 handoff。Q18 继续保持 `PARTIAL`：原生 Player 截图、字体栅格化、英文最长文案、鼠标命中、前台键盘、性能和最终视觉仍需后续 Player 门禁。第一次 Python 错误来自在 `game/` 子目录运行，已在结果中区分记录。下一步进入 Q19 当前船员与反馈审计。

## 2026-09-13 Q19 当前 Pipeline 定向复核

Q19 在 Q18 当前 HUD 代码级复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）执行 `Q19CrewInteractionAuditTests`，目标复跑为 `1/1 PASS`（测试 `0.3789095s`、总计 `0.39s`）。同一 Pipeline Editor 的 `Save.Tests` 全程序集为 `377/377 PASS`（总计 `82.16s`）；`dotnet build game/AgentCore/AgentCore.csproj --nologo` 为 `0 warnings / 0 errors`。

本轮没有修改产品代码，当前源 hash、Pipeline 状态、Editor `ready/stopped` 和 Console 清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q19 handoff。目标复跑覆盖离线模板不可用、typed duty proposal、owner 岗位拒绝/批准、关系事实/重复保护和 EventLog/行为/关系/AgentMemory SQLite 恢复；Console 检查为 `13` 条、`0` Warning、`0` Error。Q19 保持 `VERIFIED`，只限离线船员与存档 owner；在线 LLM、原生 Player UI/输入/视觉、动作/游泳/碰撞、性能和正式视觉门仍未通过。下一步进入 Q20 当前 formal resource/presentation 审计。

## 2026-09-13 Q20 当前 Pipeline 定向复核

Q20 在 Q19 船员/存档复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）执行候选资源 EditMode `D19R03Q20VegetationTests`，结果为 `1/1 PASS`（Pipeline `0.01s`、测试 `0.0020841s`）；随后执行 R03 旧树/植被接地 PlayMode，结果为 `1/1 PASS`（Pipeline `7.1s`、测试 `6.8460968s`），运行 marker 为 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`。

本轮没有修改产品代码，当前测试、builder、Prefab、D26 runtime 与 D18 catalog hash、Pipeline 状态、Editor `ready/stopped` 和 Console 清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q20 handoff。catalog 仍为 `BLOCKED / 0 / 0`，`RenderAuthoredPalms=false` 且正式 Prefab 棕榈引用为 `0`，所以 Q20 保持 `PARTIAL/BLOCKED_ASSET`。PlayMode 轮询发生短暂连接拒绝后重连，最终状态文件仍完整 `1/1 PASS`；本轮不升级正式植被、原生 Player 视觉或 VC-G01。下一步进入 Q21 当前港口空间审计。

## 2026-09-13 Q21 当前 Pipeline 定向复核

Q21 在 Q20 资源门禁复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）执行 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `0.1934504s`、总耗时 `0.21s`；四个服务建筑各 `37 Renderer / 0 missing`，可见 bounds `min.y≈0.225`，六个 pairwise XZ footprint 无重叠，六个空间组、稳定 Spawn/Disembark 锚点与显式碰撞 owner 均保留。

本轮没有修改产品代码，当前 test/builder/Prefab/InitialHarbor hash、Pipeline 状态、Editor `ready/stopped` 与 Console 清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q21 handoff。Console 检查前为 `10` 条日志、`0` Error、`0` Warning，随后已清空。Q21 继续 `PARTIAL`：原生 Player 镜头/遮挡/键鼠可达性、植被准入、性能和最终视觉仍未被本卡证明。下一步进入 Q22 当前船海天气审计。

## 2026-09-13 Q15 当前 Pipeline 定向复核

Q15 在 Q14 船岸有界状态链复核后继续使用已连接的 Unity Pipeline Editor（PID `53332`）执行 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `17.3518531s`、总耗时 `17.45s`；I/M/T 三类面板各覆盖 10 次打开、重复请求、关闭和 Gameplay 恢复，临时页互斥与重复请求拒绝成立。

本轮没有修改产品代码，当前 D29/D16 源 hash、Pipeline 状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q15 handoff。Q15 保持 `VERIFIED`，只限 D29/D16 PlayMode 焦点/快捷键/恢复；物理 Win32 键鼠、原生 Player、渲染视觉、性能和 VC-G01 仍不在此证据范围。下一步进入 Q16 当前交易桥复核。

## 2026-09-13 Q16 当前 Pipeline 定向复核

Q16 在 Q15 面板焦点复核后继续使用同一连接的 Unity Pipeline Editor 执行 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `4.7550835s`、总耗时 `4.85s`；实际 Console marker 确认 owner 未就绪、快速双击单 owner、买卖各一次变化、余额不足不变和满仓不变，检查前共有 `80` 条日志、`0` 条 Error，随后已清空 Console。

本轮没有修改产品代码，当前 D29/D3 源 hash、Pipeline 状态、Editor `ready/stopped` 与清空 Console 的动作已写入 `game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q16 handoff。Q16 保持 `VERIFIED`，只限 D29/D3 交易 owner 桥接与拒绝反馈；原生 Player 鼠标/视觉/性能、完整首航和 VC-G01 仍不在此证据范围。下一步进入 Q17 当前船坞/整备桥复核。

## 2026-09-13 Q25 当前 Pipeline 定向复核

Q25 在 Q24 存档恢复诊断后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑当前首航链。`D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.47s`），覆盖 D30 七阶段投影、D23 40 tick 首航/抵达/返港/存档投影及失败/重复保护；`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `4.72s`，测试耗时 `2.699671s` 与 `1.9185608s`），覆盖 InitialHarbor 交易关闭/保存与出航/推进/返港。

## 2026-09-13 Q25 当前 Pipeline 定向复核 Luna R2

Q25 在 Q24 当前存档恢复复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑当前首航链。`D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.47s`），覆盖 D30 七阶段投影、D23 40 tick 首航/抵达/返港/存档投影及失败/重复保护；`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `4.51s`，测试耗时 `2.4763038s` 与 `1.9159675s`），覆盖 InitialHarbor 交易关闭/保存与出航/推进/返港。

本轮没有修改产品代码；运行日志仍记录程序化 Player fallback、D27 等待提示和 D31 runtime performance pending，不能把它们当作视觉/性能通过。当前 test/support/route hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q25 handoff。Q25 保持 `PARTIAL`，下一步进入 Q26 当前测量基线。

本轮没有修改产品代码；运行日志确认 D2/D5/D25/D26/D27/D28/D29/D31 owner 已初始化，但程序化 Player fallback、D27 启动等待提示和 D31 runtime performance pending 仍是开放质量边界。Console API 报告 `126` 条、返回窗口 `100` 条；返回窗口未观察到 Error、观察到 `4` 条已知 Warning，随后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。当前测试和源码 hash 见 `game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q25 handoff。Q25 保持 `PARTIAL`，下一步进入 Q26 当前测量基线。

## 2026-09-13 Q24 当前 Pipeline 定向复核

Q24 在 Q23 正式场景资源集成复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）执行 `D19Q24SaveRestoreDiagnosticTests`。完成态为 `total=3 / passed=3 / failed=0 / skipped=0 / inconclusive=0`，Pipeline 总计 `1.22s`；陆地 port/clock/money/cargo/crew round-trip、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与 `D2_PlayerSpawnAnchor` 安全恢复、以及 `after_player` 故障后保护上一份好档均通过。

## 2026-09-13 Q24 当前 Pipeline 定向复核 Luna R2

Q24 在 Q23 当前正式场景集成复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑 `D19Q24SaveRestoreDiagnosticTests`，结果为 `3/3 PASS`（Pipeline `1.19s`）；陆地 port/clock/money/cargo/crew round-trip、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与 `D2_PlayerSpawnAnchor` 安全恢复、以及 `after_player` 故障后保护上一份好档均通过。

本轮没有修改生产代码；当前 Save/D2/D7/test hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q24 handoff。`PlayerState` 仍没有独立游泳状态/姿态字段，Q24 保持 `PARTIAL`，下一步进入 Q25 当前首航/循环集成诊断。

本轮没有修改生产代码；测试前 Console `11` 条、`0` Error、`0` Warning，测试后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。当前 Save/D2/D7/test hash 与 schema 边界见 `game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q24 handoff。`PlayerState` 仍无独立游泳状态/姿态字段，Q24 保持 `PARTIAL`，下一步进入 Q25。

## 2026-09-13 Q23 当前 Pipeline 定向复核

Q23 在 Q22 船海天气 owner 审计后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）执行 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `0.3831589s`、总计 `0.39s`；关闭并重开 InitialHarbor 后，active roots=`34`、R03/船实例各 `1`、活动 R03 Renderer=`361`、缺失材质/脚本=`0`、地形碰撞体=`1`、船体碰撞底部=`-0.0400` 均一致，港口/船/D27 GUID 解析有效。

本轮没有修改产品代码；测试前 Console `10` 条、`0` Error、`0` Warning，测试后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。当前测试、scene/Prefab hash 与边界见 `game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q23 handoff。Q23 保持 `PARTIAL`：静态正式资源集成通过不等于原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过；下一步进入 Q24。

## 2026-09-13 Q23 当前 Pipeline 定向复核 Luna R2

Q23 在 Q22 当前船海天气复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`，结果为 `1/1 PASS`（测试 `0.3594421s`、总计 `0.37s`）。关闭并重开 InitialHarbor 后，active roots=`34`、R03/船实例各 `1`、活动 R03 Renderer=`361`、缺失材质/脚本=`0`、地形碰撞体=`1`、船体碰撞底部=`-0.0400` 均一致，港口/船/D27 GUID 解析有效。

本轮没有修改产品代码；当前 scene/Prefab/test hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q23 handoff。Q23 保持 `PARTIAL`：静态正式资源集成通过不等于原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过；下一步进入 Q24 当前存档恢复诊断。

## 2026-09-13 Q21 当前 Pipeline 定向复核 Luna R2

Q21 使用当前工作区源码在 connected Unity Pipeline 中重跑 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`，结果为 `1/1 PASS`（测试 `0.1980183s`、总计 `0.23s`）。实际 marker 确认四个服务建筑各 `37 Renderer / 0 missing`，当前 bounds、六个空间组、pairwise XZ 无重叠，以及 `23` 个碰撞体中的 `1` 个 R03 terrain 与 `22` 个 D27 owner collider 均符合合同。

本轮没有修改产品代码；当前 hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q21 handoff。Q21 继续 `PARTIAL`：静态空间/owner 证据不证明原生 Player 镜头、遮挡、键鼠可达性、植被准入、性能或最终视觉。下一步进入 Q22 当前船海天气审计 R2。

## 2026-09-13 Q22 当前 Pipeline 定向复核 Luna R2

Q22 在 Q21 当前源码空间复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`，结果为 `1/1 PASS`（测试 `0.1646664s`、总计 `0.17s`）。实际 marker 确认水面 `96×96`、sea level `-0.0397`、`R03_Beach min.y=-0.0048`、泡沫 `28`、水材质缺失 `0`、可见船体 `min.y=0.0600`、船壳碰撞底部 `-0.0400`、R03 表现音频源 `0`，D9 鱼类活动 `1.00/0.90/0.55`，D28 降水 `0/560/980`、波动倍率 `1.00/1.40/2.35`。

本轮没有修改产品代码；当前 hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q22 handoff。Q22 保持 `PARTIAL`：静态水线/船体/天气 owner 合同不证明真实 Player 海里游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉。下一步进入 Q23 当前正式资源集成审计。

## 2026-09-13 Q22 当前 Pipeline 定向复核

Q22 在 Q21 静态港口空间审计后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）执行 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`。完成态为 `total=1 / passed=1 / failed=0 / skipped=0 / inconclusive=0`，测试耗时 `0.1823856s`、总计 `0.19s`；实际 marker 确认水面 `96×96`、sea level `-0.0397`、`R03_Beach min.y=-0.0048`、泡沫 `28`、水材质缺失 `0`、可见船体 `min.y=0.0600`、船壳碰撞底部 `-0.0400`、R03 表现音频源 `0`，D9 鱼类活动 `1.00/0.90/0.55`，D28 降水 `0/560/980`、波动倍率 `1.00/1.40/2.35`。

本轮没有修改产品代码；测试前 Console `12` 条、`0` Error、`0` Warning，测试后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。当前测试、运行时和 Prefab hash 及边界见 `game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 与 Q22 handoff。Q22 保持 `PARTIAL`：静态水线/船体/天气 owner 合同通过不等于真实 Player 海里游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉通过；下一步进入 Q23。

## 2026-09-13 Q26 当前 Pipeline 定向复核

Q26 在 Q25 首航/循环诊断后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑当前 `D19R03Q26GameplayDefectPlayModeTests`。完成态为 `16/16 PASS`，Pipeline 总计 `36.31s`，failed/skipped/inconclusive 均为 `0`；覆盖键盘游泳、E 拒绝/登船/下船、连续水域移动、舵轮、树木/植被、泡沫、市场入口、海盗塔接地、船员锚点、船轴、存档重开路线、自有地形碰撞和生成舵轮高度。

随后独立重跑 `Q26A01RealKeyboardInputMovesPlayerWhileSwimming`，结果 `1/1 PASS`（Pipeline `3.38s`，测试 `3.2825784s`），实际 marker 为 `keyboard=W, swimming=True, action=Swimming, planarDistance=0.4781, surfaceY=-0.020`；这属于运行状态/位移证据，不是游泳姿态截图 PASS。全量 Console API 报告 `949` 条、返回窗口 `100` 条（窗口内 `0` Error、`11` Warning）；独立 A01 窗口报告 `65` 条（`0` Error、`6` Warning），随后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。

本轮没有新增产品代码变更；运行日志仍明确保留程序化 Player fallback、未验证导入蒙皮/中立 procedural path 和 D31 runtime performance pending。当前测试列表、源码 hash 和边界见 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q26 handoff。Q26 保持 `PARTIAL`，不把逻辑/owner 合同扩大为 native Player GPU 画面、正式 Avatar/动作、目标设备性能或 VC-G01 视觉通过；下一步进入 Q27。

## 2026-09-13 Q26 当前 Pipeline 定向复核 Luna R2

Q26 在 Q25 当前首航复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）重跑当前 `D19R03Q26GameplayDefectPlayModeTests`，完成态为 `17/17 PASS`（Pipeline `36.21s`），并独立重跑 Q26A01=`1/1 PASS`（Pipeline `3.14s`，测试 `3.0117097s`）。全量覆盖键盘游泳、E 拒绝/登船/下船、连续水域移动、舵轮、树木/植被、泡沫、市场入口、船轴、船员锚点、自有地形、存档路线、舵轮高度与暖木对比；A01 marker 为 `keyboard=W / swimming=True / action=Swimming / planarDistance=0.4683 / surfaceY=-0.020`。

本轮没有新增产品代码变更；当前 test/Camera/terrain/presentation hash、Editor `ready/stopped`、error console=`0` 与清空动作已写入 `game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json` 和 Q26 handoff。Q26 保持 `PARTIAL`：Pipeline 状态/owner/碰撞证据不等于原生 Player 物理键盘、GPU 画面、自然泳姿、正式 Avatar/动作、目标设备性能或 VC-G01 视觉通过。下一步进入 Q27 当前性能/预算证据。

## 2026-09-13 Q27 当前 Pipeline 定向复核

Q27 在 Q26 缺陷套件复核后继续使用 connected Unity Pipeline Editor（PID `53332`，Unity 6000.0.80f1，`-batchmode -nographics`）执行 `D16PerformanceBudgetTests` 与 `D16A01RuntimePerformanceWindowSamplesContinuously`。预算 EditMode=`6/6 PASS`（`0.02s`），连续性能 PlayMode=`1/1 PASS`（测试 `62.5509786s`）；实际窗口 `60.0037537s`、`164` samples，`PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`。

当前报告为 `Null Device / GraphicsDeviceType=Null`，GPU timing 不可用；平均 `30.5038 FPS`、平均帧时间 `151.8086ms`、最长帧 `1153.7593ms`、超过 `50ms` 长帧 `84`、最小 FPS `0.8667`。既有非 headless standalone R3 参考继续保留为独立证据：`60.014019s / 3465 samples / 58.734367 FPS / 21.460548 FPS 1% Low / 622.258118ms longest frame / GPU unavailable`。这些数字是诊断风险，不是目标设备性能通过。

测试前 Console `63` 条、`0` Error、`6` Warning，测试后已清空，Editor 最终 `ready / compiling=false / domainReloadInProgress=false / playMode=stopped`。当前 report/hash 和边界见 `game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 及 Q27 handoff。Q27 保持 `PARTIAL`，未基于 headless 指标做无证据优化，下一步进入 Q28 新候选 Player 构建。

同日 R1 复核进一步记录了当前 headless 运行的真实风险：EditMode=`6/6 PASS`（`0.04s`），PlayMode=`1/1 PASS`（Pipeline `64.0s`，测试 `63.8459749s`），实际连续窗口 `60.004829s / 163 samples`。探针 owner 和预算合同仍成立，但当前 `Null Device / GraphicsDeviceType=Null` 平均 `30.7857 FPS`、平均帧 `155.6439ms`、最长帧 `1103.0729ms`、超过 `50ms` 的长帧 `90`，GPU timing 不可用；既有非 headless standalone R3 只作为分离参考，不被本轮升级。Console lower-bound seq `23300` 查询为 `0 Error / 6 Warning`，Editor `ready/stopped`，测试后已 clear。Q27 仍 `PARTIAL`，没有据此修改 D16 生产逻辑；结果：[Q27 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)，marker：[Q27 marker](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)。

## 2026-09-13 Q28 当前 Pipeline 构建复核

Q28 使用 connected Unity Pipeline `build`/`build_status` 生成不覆盖历史产物的 `Q28_CANDIDATE_PLAYER_R5.exe`。dry-run=`valid=true`、`validationErrors=[]`；目标 `StandaloneWindows64`，显式场景为 `MainMenu.unity + InitialHarbor.unity`，未提供 Development/AllowDebugging/ConnectWithProfiler 选项。BuildReport=`Succeeded`，总产物 `175190134` bytes，耗时 `42221ms`，`0` errors、`9` warnings；R5 EXE 与内置 Assembly-CSharp hash 已登记。

构建后 Editor=`ready / compiling=false / domainReloadInProgress=false / playMode=stopped`；Console 检查前 `12` 条（`0` Error、`9` Warning、`3` Log），显式清空后 `0` 条。主要 warning 是 `RuntimePipelineConfig enableInBuilds=false`，即 Pipeline 不进入该 Player；本卡不宣称 Player 控制面、物理输入、截图、GPU 或视觉通过。权威结果见 `game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 和 Q28 handoff。Q28 继续 `PARTIAL`，下一步是 Q29 隔离 Player 路线/输入证据；若控制面不可用，登记具体阻塞并保持未通过。

## 2026-09-13 Q29 当前 Pipeline 隔离 Player 路线完成（有限范围）

Q29 当前源码 R5 候选已由 connected Unity Pipeline 构建并在隐藏 Windows Player 中完成有界交互路线：MainMenu → InitialHarbor、W/Walk、仓库模态阻断 W、I/M/T 打开/重复/Esc 恢复、E 登船/下船、Y 出航、航行中 M owner 拒绝、Ship W/A 速度与转向、Esc 暂停/恢复。Player 进程通过 runtime `quit` 优雅退出，`console --level error` 路线后返回 `0` 条。

本轮临时启用 `enableInBuilds=true / port=7900 / developmentBuild=true` 仅用于 QA，完成后已用 Pipeline 的 `dry_run` + `confirm=true` 恢复 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`。结构化结果为 `game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json`，marker 归档为 `game/tools/results/LOCAL-QUALITY/Q29/Q29_RUNTIME_PIPELINE_QA_R5_PIPELINE_CONSOLE_CAPTURE.log`。

状态规则不变：Q29 仍 `PARTIAL`，因为本轮没有截图、前台物理键盘、目标设备性能或最终视觉判断；Q03 正式动作资源仍 `BLOCKED_ASSET`，Q20 正式植被资源仍 `PARTIAL/BLOCKED_ASSET`，VC-G01 仍 `IN_PROGRESS`，VC-G02/VC-G03 仍 `LOCKED`。下一步是 Q30 当前源码独立证据审查与缺陷矩阵更新，然后才进入 Q31 最终 Player 视觉门禁。

## 2026-09-13 Q30 当前源码独立审查完成（保持 PARTIAL）

本轮把 Q28 当前构建、Q29 R5 隐藏 Player 路线、当前 Unity 定向回归和历史截图边界合并到独立缺陷矩阵。定向 Python 为 `65/65 PASS`；Unity Pipeline 目标为 `D16Accessibility 8/8`、`D16InputRouter 10/10`、`D17ActionQuality 16/16`、`VcG01WorldLogic 14/14`；Q26 当前 `16/16` 与 Q26A01 `1/1` 保留。全量 Python discovery 约 `215s` 没有完成 verdict，已停止本轮 PID `16100`，记为 `INCOMPLETE`。

矩阵结论：W/S—HUD 冲突在当前 hidden Player 的仓库模态场景未复现，逻辑为 `PASS_WITHIN_SCOPE`，前台物理输入/截图仍未验证；动作扭曲、游泳画面、上船/舵位接触、树木接地均继续 `PARTIAL`。Q03 formal action/Avatar 仍 `BLOCKED_ASSET`，Q20 formal vegetation 仍 `PARTIAL/BLOCKED_ASSET`，Q27 性能仍 `PARTIAL`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json`，交接见 `docs/game/design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md`。

下一步切换到 Q31 最终 native Player 视觉门禁：固定截图矩阵、完整成功/拒绝/重复路径、镜头角度和资源准入复核。没有直接截图证据前，VC-G01 保持 `IN_PROGRESS`，VC-G02/VC-G03 保持 `LOCKED`。

## 2026-09-13 Q31 当前源码 D2/Player 复核（保持 PARTIAL）

本轮使用 connected Unity Pipeline 的 hidden GUI Player 完成 Q31 当前源码复核，并针对真实复现的水面/岛面垂直重叠缺陷做了 D2 最小修复。D2 现在从游泳体积上方向下查找可行走顶面，避免 D19 单面 MeshCollider 因水下向上射线背面问题而误放行；检测到上方地形后退出游泳并重定位到可行走面。新增 `D17A17WaterOverlapRejectsSwimmingUnderWalkableTerrain`，在同一次 D2 物理调用中锁定退出、接地、重定位和日志。

证据：当前源码 recompile 无错误；D17 相关定向序列 `9/9 PASS`；`build_33970c8639b5` 为 `StandaloneWindows64 / Succeeded / 0 errors / 7 warnings`。R2 hidden Player 在用户报告坐标由修复前 `swim=true/grounded=false/y=0.400` 变为修复后 `swim=false/grounded=true/y=1.003`；开放水域按下 `W` 产生 `speed=2.073093` 且保持 `Swimming`。runtime error console=`0`、Player clean close、QA 设置恢复，Editor=`ready/stopped`。完整 hash、PNG、日志和状态边界见 `game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_20260913_RESULT.json` 与 Q31 handoff。

本轮只关闭 D2 水下地形状态缺陷，不等于完整游泳视觉通过。重叠恢复帧仍有近墙/遮挡且看不到玩家；开放水域仍是低模/程序化泳姿。用户已报告的动作扭曲、舵位语义、树木悬浮/全镜头接地、穿模、正式动作/Avatar、物理前台输入、目标设备性能和人工视觉门禁仍开放。因此 `Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`、`Q27=PARTIAL`、`D31-P04/S05/G06=PARTIAL` 保持。

下一步仍按单一 owner 选择 formal action/avatar、自然 swim cycle、Helm IK/语义或 Q20 资源准入中的一个明确缺陷；先补可重复契约测试，再使用 hidden Player 取证，最后才执行 VC-G01 人工视觉门禁。

## 2026-09-13 Q07 R03 服务建筑碰撞 owner 修复

Q31 的相机/遮挡诊断继续向上游追查后，确认 `MarketVisual`、`WarehouseVisual`、`TavernVisual`、`ShipyardVisual` 各有 `37 Renderer` 但均为 `0 Collider`。由于 `CameraFollow` 的障碍检测使用非 Trigger `SphereCast`，根因是 Harbor 碰撞 owner 缺失，不是单独调相机距离。

本轮只修改 `HarborCollisionController`：从四栋建筑的 `Body` Renderer bounds 派生外扩 `0.08m` 的 solid BoxCollider，统一放入 `D6_HarborCollision/R03BuildingCollision.<Name>`；不改表现 Prefab、不改 D2、不改 Systems。新增 Q07 定向 PlayMode 测试后，结果为 `1/1 PASS`；D17 全套 `16/16 PASS`；D19R03Q26 全套 `16/16 PASS`；recompile 无错误。

当前源码 hidden Windows Player `build_c3588a5c004b` 成功（`0 errors / 7 warnings`），进入 `InitialHarbor` 后四个碰撞体均由 D6 持有、启用且非 Trigger，四条外部射线均命中对应 collider，runtime error console=`0`，Player.log 严格 Error/Exception/Crash/Fatal=`0`，clean close。结果：[Q07 当前建筑碰撞结果](../../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_BUILDING_COLLISION_PIPELINE_20260913_RESULT.json)；交接：[Q07 handoff](design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。

状态边界：Q07 保持 `VERIFIED`（扩展到四个服务建筑的 Harbor owner）；Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`。这不关闭最终相机构图、门口/路线语义、正式动作/Avatar、自然泳姿、树木准入、物理前台输入、目标设备性能或人工视觉门禁。下一步继续选择单一 owner 缺陷，保持 `Q31 → VC-G01` 依赖顺序。

## 2026-09-13 Q28 当前源码 R6 候选重建

由于 Q07 修改了 `HarborCollisionController`，本轮不复用 R5，而是用 connected Unity Pipeline 生成新的 `Q28_CANDIDATE_PLAYER_R6.exe`（`build_d0507c23b205`）。dry-run 有效，实际构建 `StandaloneWindows64 / Succeeded / 0 errors / 1 warning`，耗时 `28095ms`；EXE 与内置 `Assembly-CSharp.dll` hash 已写入 Q28 rerun2 结果。

构建后仍为非开发候选：`developmentBuild=false`、`allowDebugging=false`、`connectWithProfiler=false`、`enableInBuilds=false`、`port=0`；Editor=`ready/stopped`、Error Console=`0`。本轮只证明 Q07 后的当前源码可以生成新的候选包，不证明 Player 运行、输入、截图、GPU 性能或视觉质量；Q28 继续 `PARTIAL`，下一步 Q29 使用 R6 进行隔离 Player 路线取证。

## 2026-09-13 Q29 当前源码 R6 隔离 Player rerun2

Q28 R6 之后，使用 connected Unity Pipeline 临时打开 `enableInBuilds=true / port=7901 / developmentBuild=true`，构建并启动隐藏 `Q29_RUNTIME_PIPELINE_QA_R6.exe`。第一次探索使用 `simulate_key press`，I/M/T 未打开页面，后续 Esc 进入暂停；该错误注入序列被丢弃。重启 Player 后使用 `down/wait/up` 正确协议完成干净路线：NewGame/Confirm、W、仓库 W 阻断、I/M/T 重复/关闭、E 上下船/再上船、Y、航行中 M 拒绝、船 W/A 位移、Esc 暂停/恢复；真实 runtime marker 和 Player log 已归档。

同一 R6 运行时还确认 Q07 四个服务建筑 collider 全部位于 `D6_HarborCollision`、启用且非 Trigger。Runtime Error Console=`0`、Player clean close、Player log 严格错误 token=`0`；随后恢复并读取 `enableInBuilds=false / port=0 / developmentBuild=false`，Editor=`ready/stopped`。结果见 `game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json` 与 `Q29_RUNTIME_PIPELINE_QA_R6_PIPELINE_CONSOLE_CAPTURE.log`。

状态边界：这是真实 hidden Player 的输入/状态/owner 证据，不是前台物理键盘、截图、动作/泳姿/树木/穿模视觉或目标设备性能证据；Q29 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`。下一步进入 Q30 当前源码独立审查，随后才是 Q31 最终 native Player 视觉门禁。

## 2026-09-13 Q15-Q17 当前源码 Pipeline rerun2

本轮对依赖链上的 Q15、Q16、Q17 逐项重新执行当前源码 PlayMode 测试，未修改产品代码。Q15 `1/1 PASS`（测试 `20.1427708s`、Pipeline `20.25s`），Q16 `1/1 PASS`（`3.7009758s`、`3.81s`），Q17 `1/1 PASS`（`4.319115s`、`4.43s`）。Pipeline Console 实际 marker 分别覆盖 I/M/T 十轮互斥与恢复、交易 owner 成功/拒绝/单变化、修船/船坞成功/拒绝/幂等/取消/异常恢复；三轮 Error Console 均为 `0`，Editor 最终为 `ready/stopped`。

结果文件：

- `game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json`
- `game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json`
- `game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_RERUN2_RESULT.json`

状态继续按范围判定：Q15/Q16/Q17 各自保持 `VERIFIED`，不升级 Q31 或 VC-G01。三张卡仍不证明前台物理键鼠、native Player 截图、动作/泳姿/穿模/树木视觉、目标设备性能或完整游戏验收；下一步进入 Q18 当前 HUD/本地化尺寸复核。

Q15 随后又以当前正确测试限定名单独复核：错误类名过滤的 `total=0` 未计入证据；正确限定名运行 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes` 为 `1/1 PASS`（测试 `14.8763723s`、Pipeline `15.0s`）。Console marker 确认 `I/M/T` owner、三页各十轮互斥/重复拒绝/Gameplay 恢复，Editor=`ready/stopped`，error console=`0`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q16 R2。

Q16 当前源码随后重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`：`1/1 PASS`（测试 `2.8561068s`、Pipeline `2.97s`），Console marker 确认未就绪、双击单 owner、买卖单次变化、余额不足和满仓不变；Editor=`ready/stopped`，error console=`0`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q17 R2。

Q17 当前源码随后重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`：`1/1 PASS`（测试 `3.7202408s`、Pipeline `3.84s`），Console marker 确认整备/船坞成功、材料/资金拒绝、重复幂等、取消、异常恢复和主输入恢复；Editor=`ready/stopped`，error console=`0`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q18 R2。

Q18 当前源码随后完成代码级复核：D31 HUD 分辨率矩阵=`3/3 PASS`，D31 visual logic=`36/36 PASS`，Python D29 静态 UI=`30/30 OK`；Editor=`ready/stopped`，error console=`0`。Q18 继续 `PARTIAL`，因为本轮没有真实 Player 截图、字体栅格化、英文最长文案、物理键鼠或最终视觉证据。权威结果为 `game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q19 R2。

Q19 当前源码随后完成离线船员与存档复核：定向审计=`1/1 PASS`，Save.Tests=`377/377 PASS`，AgentCore=`0 warnings / 0 errors`，Editor=`ready/stopped`，error console=`0`。Q19 继续 `VERIFIED`，仅限离线提议/owner/关系/行为/AgentMemory 恢复；在线 LLM、native Player 对话 UI/视觉与性能仍开放。权威结果为 `game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q20 R2。

## 2026-09-13 Q20 当前源码 Pipeline Q03-A17 R1

Q20 当前源码复核完成候选资源 EditMode `D19R03Q20VegetationTests`=`1/1 PASS`（测试 `0.0015829s`、总计 `0.01s`）和 R03 旧树/植被接地 PlayMode `Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence`=`1/1 PASS`（测试 `2.7703397s`、总计 `2.94s`）。marker 确认 `activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`。

正式资源门仍未解除：D18 catalog=`BLOCKED / 0 / 0`、builder=`RenderAuthoredPalms=false`、正式 Prefab 棕榈引用=`0`，因此 Q20 继续 `PARTIAL/BLOCKED_ASSET`。本轮 PlayMode 真实捕获 FMOD 输出设备 Error seq `21805`（`Error initializing output device`）；虽发出 `clear_console`，该已捕获记录仍保留，已写入 R1 结果与 marker，不将其隐藏为 Error Console 0。下一步为 Q21 当前港口空间审计。

权威结果：`game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md`。

## 2026-09-13 Q21 当前源码 Pipeline Q03-A17 R1

Q21 当前源码在同一 connected Unity Pipeline Editor 中重跑 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`，结果 `1/1 PASS`（测试 `0.9323411s`、Pipeline `0.97s`）。实际 marker 确认四个服务建筑各 `37 Renderer / 0 missing`，四个 footprint 无 pairwise XZ 重叠，六个空间组存在，碰撞边界为 `23` 个实例碰撞体、`1` 个 R03 terrain、`22` 个 D27 owner collider。

本轮以 Console seq `21868` 为基线检查新增错误，结果为 `0`；Q20 已捕获的 FMOD 输出设备 Error seq `21805` 仍在保留历史中，作为环境/runtime follow-up 登记，不归因于 Q21。Editor 最终 `ready/stopped`，没有修改产品代码。Q21 继续 `PARTIAL`：静态空间/owner 合同不等于 native Player 路线、镜头、键鼠、植被准入、性能或最终视觉通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md`。下一步为 Q22 当前船海天气审计。

Q20 当前源码随后复核候选植被和旧树隔离：EditMode=`1/1 PASS`，Q26A03 PlayMode=`1/1 PASS`，marker=`activeLegacyTreeRenderers=0 / activeR03Vegetation=24 / unsupportedR03Vegetation=0`；但 D18 catalog 仍 `BLOCKED / 0 / 0`，正式棕榈未准入，故 Q20 保持 `PARTIAL/BLOCKED_ASSET`。权威结果为 `game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`，下一步 Q21 R2。

## 2026-09-13 Q30 rerun2 current-source review

Q30 使用当前源码和 connected Unity Pipeline 完成独立复核：定向 Python `65/65 PASS`；D16Accessibility `8/8`、D16InputRouter `10/10`、D17ActionQuality `16/16`、VcG01WorldLogic `14/14`；Q07 `1/1`、Q15/Q16/Q17 各 `1/1`；Q28 R6 构建成功，Q29 R6 hidden Player 路线完成并 clean close。全量 Python discovery 在有界 `215s` 内没有完成 verdict，因此不计 PASS。

本卡保持 `Q30=PARTIAL`。当前真实证据只覆盖逻辑/owner 和隐藏 Player 状态；没有把截图缺失、前台物理键盘缺失、Q03/Q20 资源门禁或目标设备性能边界伪装为通过。下一步执行 Q31 最终 native Player visual gate；环境不具备有效截图时必须保留阻塞并继续非视觉 owner 检查。

## Q31 当前源码 RenderTexture 诊断 rerun2（2026-09-13）

Unity Pipeline 的 hidden GUI Player 控制面已经具备可复用的当前画面诊断路径：pointer/key 路线进入 `InitialHarbor` 和开放水域后，运行时 `Camera.Render` 到 RenderTexture，再通过 `ReadPixels` 导出 PNG。三张当前源码帧非黑并已 hash 登记；传统 `ScreenCapture` 仍输出黑帧，因此被保留为失败路径，不作为视觉证据。

这次能力验证把“没有画面所以无法定位”收窄为“画面可诊断，但产品画面仍不达标”：开放水域确有 `swim=true`、位移和水平程序化轮廓，仍不能宣称自然泳姿；重叠恢复帧仍近墙/玩家不可见；正式动作/Avatar、握舵语义、树木接地、穿模、前台物理输入、性能和人工门禁继续开放。Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`，不进入 VC-G02。

权威结果：`game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_VISUAL_DIAGNOSTIC_20260913_RERUN2_RESULT.json`；下一步按单一 presentation owner 做最小修复和 bounded Player 复核。

## 2026-09-13 Q31 helm presentation bounded continuation

已完成一个单一 D19-R03 presentation owner 修复：生成 `HelmWheel` 由近黑蓝灰改为暖木对比色，并由 `Q26A18` 锁定运行时材质不能退化为黑色遮挡环。当前源码回归为 Q26A18 `1/1`、D19R03Q26 `17/17`、D17 `16/16`、VcG01WorldLogic `14/14`；`build_467144669770` 无构建错误/警告。Unity Pipeline clean hidden Player 通过真实 pointer/key 控制上船，运行时确认 `ph_ship_fishing`、材质色、手部高度和 error console=`0`，并生成有效 RenderTexture 帧。

本卡仅改善“控制对象颜色/语义不清”的有限表现根因，不升级 Q31/VC-G01：自然 IK/握舵、formal action/avatar、自然泳姿、树木接地、穿模、前台物理键盘、目标设备性能、compositor-visible 截图与人工验收仍开放。下一步继续按单一 owner 选择有实际失败证据的问题，不盲目调整坐标。

结果：[Q31 helm presentation](../game/tools/results/LOCAL-QUALITY/Q31/Q31_HELM_PRESENTATION_PIPELINE_20260913_RESULT.json)；交接：[Q31 handoff](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-13 Q00/Q01 接管 R2 与 Q04 当前源码重绑定

本轮完成当前工作区的 Q00/Q01 有界接管复核，并修正 Q04 旧 hash 证据不能代表当前源码的问题。工作区保持原样：`10562` status entries（tracked=`459`、untracked=`10103`），`459 files changed, 40082 insertions(+), 49167 deletions(-)`；没有 reset、checkout、clean 或大范围覆盖。

Q01 通过 PID `53332` 的 connected Pipeline Editor 采样 `InitialHarbor`：D2 root=`(0.0000,0.1497,-2.7000)`、unit scale；HumanoidVisual=`0.6642`、17 renderers；CharacterController=`0.2424m/1.8000m/center.y=0.8469m/skinWidth=0.0194m`；D2 初始 grounded、非 swimming、D16InputRouter；默认 `Procedural/FormalAnimatorReady=false`；`ph_ship_fishing` scale=`2.5740`；D7_HelmPoint 与 D7_VisibleHelmAnchor=`(0,1.100,-6.680)`；scoped R03 MeshCollider enabled/non-trigger。一次探索性反射枚举失败被排除，直接 eval 与最终 Editor 状态均有证据。

Q04 已使用当前 D2/D17/OceanAdapter hash 重跑 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles`，结果 `1/1 PASS`。这只关闭当前水域 owner 的逻辑回归边界，仍不关闭 Player W/S、前台键盘焦点、正式泳姿、水材质、截图视觉或 VC-G01。

当前执行指针：Q00/Q01 有界接管完成；Q04=`PASS_WITHIN_SCOPE`；下一张为 Q05 当前 Pipeline 游泳表现复核。Q03=`BLOCKED_ASSET`、Q20/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 继续保持。

权威结果：`game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_LUNA_R2_20260913_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_LUNA_R2_20260913_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q05 当前源码游泳表现契约重绑定 R2

Q05 发现旧当前结果仍绑定较早的 D2 hash，因此使用当前源码通过 connected Pipeline 重跑 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`。完成态为 `1/1 PASS`，测试耗时 `12.338054s`、总耗时 `12.45s`，Editor=`ready/stopped`、error console=`0`。

本轮只验证 whole-visual-root 倾斜、离水 neutral 恢复和物理根 upright；它不证明正式四肢/头部动作、自然泳姿、native Player W/S、前台键盘焦点、水面材质或 VC-G01。Q05 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q06 当前 Pipeline 地形/落地复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q06 当前源码跳跃/接地契约重绑定 R2

Q06 使用当前源码重跑 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite`，完成态为 `1/1 PASS`，测试耗时 `13.2231615s`、总耗时 `13.34s`，Editor=`ready/stopped`、error console=`0`。

本轮确认十次真实 TryJump—落地循环和出生点附近正/斜/横/反向小步的 grounded、非游泳、有限 Transform 契约；不证明墙、坡、岸线、Ramp/跳板、船甲板、视觉网格接触、native Player 键盘或 VC-G01。Q06 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q07 当前碰撞路线复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q07 当前源码 Harbor 碰撞 owner/路线复核 R2

Q07 使用当前源码在 connected Pipeline 重跑服务建筑碰撞 owner 测试与 D17A10 地形/码头/跳板双向路线。前者 `1/1 PASS`（`2.284088s`），后者 `1/1 PASS`（`4.0585166s`）；Editor=`ready/stopped`，error console=`0`。

这只确认 D6-owned 服务建筑 solid colliders，以及已覆盖的 D19 terrain、DockGround、GangplankGround 自动化路线。它不证明所有坡墙/岸线、视觉网格接触、船舷/上下船、native Player WASD 或最终穿模视觉。Q07 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`；下一步先登记 Q08 的 Q03 `BLOCKED_ASSET` 依赖阻塞，再进入 Q09。

权威结果：`game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q08 当前依赖阻塞复核 R2

Q08 仍不能运行：Q03 的 15 个 formal clip 继续报告 `humanMotion=0 / genericRoot=15`，默认 InitialHarbor formal consumer 为 `0`，没有合法 current-rig 地面动作资源或可审计骨骼轴适配器。Q05/Q07 当前子契约虽已分别通过，但不能替代正式动作整合前置。

本轮明确保持 `Q08=BLOCKED`，不把 neutral-only/generic motion 伪装为整合 PASS。解阻需要合法动作资源或可审计 adapter、同源 Q08 integration rerun 和独立 visual/manual gate。Q08 阻塞不阻止 Q09，下一步进入 Q09 当前 Pipeline 甲板/舵位路线复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q09 当前源码甲板锚点/登船复核 R2

Q09 使用当前源码重跑 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`，完成 `1/1 PASS`，测试耗时 `3.9577833s`、总耗时 `4.07s`，Editor=`ready/stopped`、error console=`0`。

本轮确认非单位船根下 HelmPoint/可见舵位关系、10 次登/下船和 30m 远距拒绝；不证明自然握舵、船体/甲板视觉清除、航行中相对关系、native Player 输入或 VC-G01。Q09 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q10 当前航行中甲板关系复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q10 当前源码航行甲板关系复核 R2

Q10 使用当前源码重跑 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`，完成 `1/1 PASS`，测试耗时 `2.8314083s`、总耗时 `2.95s`，Editor=`ready/stopped`、error console=`0`。

本轮确认十个船舶平移/转向快照中登船玩家相对稳定船根的局部位置/旋转不漂移，并能恢复正常下船；不证明开放海航行、native W/S/A/D、船舶碰撞、镜头构图、自然握舵或 VC-G01。Q10 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q11 当前安全下船复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q11 当前源码安全下船复核 R2

Q11 使用当前源码重跑 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`，完成 `1/1 PASS`，测试耗时 `4.1991542s`、总耗时 `4.31s`，Editor=`ready/stopped`、error console=`0`。

本轮确认十次合法下船、活动 terrain bounds 外无落点拒绝和恢复快照后合法下船；不证明移动船舶视觉碰撞、所有岸边/跳板出口、native Player、正式动作、相机或 VC-G01。Q11 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q12 当前相机复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q12 当前源码相机复核 R2

Q12 使用当前源码重跑 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`，完成 `1/1 PASS`，测试耗时 `4.44514s`、总耗时 `4.56s`，Editor=`ready/stopped`、error console=`0`。

本轮确认十次相机模式/目标切换、受控 SphereCast 障碍收缩和阻尼恢复；不证明最终场景墙角/船舷/低矮物体构图、渲染穿模、native Player 激活或 VC-G01。Q12 保持 `VERIFIED` / `PASS_WITHIN_SCOPE`，下一步执行 Q13 当前输入焦点与 HUD 隔离复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260913_LUNA_R2_RESULT.json`。

## 2026-09-13 Q03-A17 Formal Humanoid 游泳姿态候选

本卡只在显式 `-d20FormalHumanoid` 路径调整 D20 Formal Humanoid 的游泳肌肉包络，并为 Q03D20 物理根基线等待两帧以消除初始化竞态。当前源码 recompile 无错误，Q03D20=`4/4 PASS`、D17=`16/16 PASS`；新的 StandaloneWindows64 hidden Player 构建 `build_21e1ee070799` 为 `0 errors / 1 warning`。

Unity Pipeline 运行态确认 Formal/Humanoid bridge、`Swimming/Fish/pitch=70`，前后 RenderTexture 帧显示折膝减轻，但手臂仍抬起/风格化，低模候选仍不是自然 authored swim cycle。因此 Q03-A17=`PARTIAL`，Q03 继续 `BLOCKED_ASSET`，Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`。该诊断使用只读组件定位，不是完整前台物理输入或人工视觉验收。

权威结果：`game/tools/results/LOCAL-QUALITY/Q03-A17/Q03_A17_FORMAL_HUMANOID_PIPELINE_R3_RESULT.json`；捕获日志：`game/tools/results/LOCAL-QUALITY/Q03-A17/Q03_A17_FORMAL_HUMANOID_PIPELINE_R3_CAPTURE.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q03-A17/Q03-A17.md`。下一步保持候选隔离，等待合法 authored action/Avatar 资源或新的明确 presentation owner。

## 2026-09-13 Q00/Q01 当前接管与根因复核：Q03-A17 R1

Q00 重新绑定当前 dirty worktree、PID `53332` 的单一 connected Unity Pipeline Editor、Q03-A17 的两个产品源文件及 exact Player `build_21e1ee070799`。本轮没有清理工作区或覆盖既有改动；Editor 最终 `ready/stopped`、RuntimePipeline=`enableInBuilds=false/port=0`、error console=`0`。PlayMode 切换瞬间的一次 eval 可达性竞态被记录为工具时序事件，没有被当作产品通过证据。

Q01 随后对 InitialHarbor 进行只读 direct eval，确认 D2 物理根为单位缩放、视觉根为 `0.66`、胶囊和 D16 输入 owner 当前值，默认表现仍是 `Procedural/FormalAnimatorReady=false`；确认非单位船根、游戏舵点/可见舵点一致、R03 MeshCollider 为 enabled/non-trigger 且旧 TerrainCollider disabled。该诊断把用户报告的动作扭曲、游泳、上船穿模、树木浮空和 W/S-HUD 冲突分别落回正确 owner，并明确没有任何视觉 PASS。

Q00/Q01 结果均为 `VERIFIED_WITH_SCOPE`。下一张实际执行卡为 Q04 当前 D2 水域 owner 回归，然后进入 Q05；Q03=`BLOCKED_ASSET`、Q20/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED` 保持不变。

权威结果：`game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q03A17_20260913_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A17_20260913_RESULT.json`。

## 2026-09-13 Q04 当前水域 owner 回归：Q03-A17 后 R1

Q04 使用当前 D17/D2/OceanAdapter 源在 connected Unity Pipeline Editor 中重跑 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles`，完成 `1/1 PASS`，测试耗时 `12.764308s`、总耗时 `12.91s`，Editor=`ready/stopped`、error console=`0`。十次浅水与十次深水进入—移动—退出、非零位移和 grounded recovery 继续有真实 PlayMode 证据。

Q04 只确认 D2 水域状态 owner，不能替代 native Player W/S、前台焦点、自然泳姿、水材质或截图视觉；Q03/Q31/VC-G01 状态不升级。下一张执行卡为 Q05 当前游泳表现契约复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q05 当前游泳表现契约回归：Q03-A17 后 R1

Q05 使用当前 D17/D2 源在 connected Unity Pipeline Editor 中重跑 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`，完成 `1/1 PASS`，测试耗时 `12.7597425s`、总耗时 `12.9s`，Editor=`ready/stopped`、error console=`0`。证据覆盖十次 whole-visual-root 游泳倾斜/离水 neutral 恢复和物理根 upright。

Q05 仍只代表安全的视觉根姿态契约，不代表 authored 四肢/头部动作、自然泳姿、native Player W/S、前台焦点或截图视觉；Q03/Q31/VC-G01 不升级。下一张执行卡为 Q06 当前地形/落地复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q06 当前跳跃/接地 owner 回归：Q03-A17 后 R1

Q06 使用当前 D17/D2/HarborCollision 源在 connected Unity Pipeline Editor 中重跑 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite`，完成 `1/1 PASS`，测试耗时 `12.928311s`、总耗时 `13.07s`，Editor=`ready/stopped`、error console=`0`。证据覆盖十次跳跃恢复以及正向、斜向、横向和反向小步移动的 grounded、非游泳与有限 Transform 契约。

Q06 只确认出生点附近基础运动稳定性，不替代墙/坡/岸线/跳板/船甲板路线、视觉网格接触或 native Player 键盘；Q03/Q31/VC-G01 状态不升级。下一张执行卡为 Q07 当前地形/服务路线复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q07 当前 Harbor 碰撞 owner/路线回归：Q03-A17 后 R1

Q07 在同一 connected Unity Pipeline Editor 中串行重跑服务建筑 collision owner 与地形/码头/跳板路线：建筑定向测试 `1/1 PASS`（测试 `2.1973986s`、总 `2.33s`），D17A10 `1/1 PASS`（测试 `3.9754098s`、总 `4.12s`）。两项均无失败、跳过或 inconclusive，Editor=`ready/stopped`、error console=`0`。

Q07 只确认已覆盖的 Harbor collision owner 和自动化双向路线，不替代所有墙/坡/岸线、视觉网格接触、船甲板/上下船或 native Player WASD。Q07 保持 `PASS_WITHIN_SCOPE`；Q08 仍因 Q03=`BLOCKED_ASSET` 阻塞，下一张独立执行卡为 Q09。

权威结果：`game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q08 当前依赖阻塞复核：Q03-A17 后 R1

Q08 继续保持 `BLOCKED`，没有运行整合测试，也没有修改产品代码。Q03 formal authored ground-action/avatar 资源门仍未解除；Q03-A17 仅是显式 opt-in 游泳候选，不能替代地面动作准入。Q05 R1 和 Q07 R1 仅作为有界子契约证据保留，不拼接成完整输入—状态—位移—动作—姿态通过。

解除需要当前 rig 可审计的合法地面动作资源或骨骼轴适配器，并对同源 Q08 action-state/movement/swim/grounding suite 重跑。该阻塞不阻止独立 Q09，下一张执行卡为 Q09 当前甲板/舵位路线复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260913_Q03A17_R1_RESULT.json`。

## 2026-09-13 Q09 当前甲板/登船 owner 回归：Q03-A17 后 R1

Q09 使用当前 D17/D7ShipDeck/D2ControlRouter 源在 connected Unity Pipeline Editor 中重跑 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`，完成 `1/1 PASS`，测试耗时 `2.447238s`、总耗时 `2.55s`，Editor=`ready/stopped`、error console=`0`。证据覆盖非单位船根舵位锚点、十次登/下船生命周期和 30m 远距拒绝。

Q09 只确认当前甲板/登船状态 owner 契约，不替代自然握舵、手轮接触、船体视觉清除、航行中相对关系、native Player 输入或 VC-G01。下一张执行卡为 Q10 当前航行中甲板关系复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q10 当前航行中甲板关系回归：Q03-A17 后 R1

Q10 使用当前 D17/D2ShipController/D7ShipDeck 源在 connected Unity Pipeline Editor 中重跑 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`，完成 `1/1 PASS`，测试耗时 `1.649084s`、总耗时 `1.75s`，Editor=`ready/stopped`、error console=`0`。十个船舶平移/转向快照中，登船玩家相对船根的局部位置/旋转保持稳定，并能正常下船。

Q10 只确认 snapshot-driven 甲板关系，不替代开放海航行、native W/S/A/D、船舶碰撞、镜头构图、自然握舵或 VC-G01。下一张执行卡为 Q11 当前安全下船复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q11 当前安全下船 owner 回归：Q03-A17 后 R1

Q11 使用当前 D17/D2ShipController/D2ControlRouter/D7ShipDeck 源在 connected Unity Pipeline Editor 中重跑 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`，完成 `1/1 PASS`，测试耗时 `3.365028s`、总耗时 `3.48s`，Editor=`ready/stopped`、error console=`0`。证据覆盖十次合法下船、活动 terrain bounds 外无落点拒绝和恢复航行快照后的合法下船。

Q11 只确认合法/无落点下船状态契约，不替代移动船舶视觉碰撞、所有岸边/跳板出口、native Player 输入、正式动作质量或相机质量。下一张执行卡为 Q12 当前相机复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q12 当前相机 owner 回归：Q03-A17 后 R1

Q12 使用当前 D17/CameraFollow 源在 connected Unity Pipeline Editor 中重跑 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`，完成 `1/1 PASS`，测试耗时 `3.2043464s`、总耗时 `3.31s`，Editor=`ready/stopped`、error console=`0`。证据覆盖十次相机模式/目标切换、受控 BoxCollider/SphereCast 遮挡收缩和移除后的阻尼恢复。

Q12 只确认 CameraFollow 的模式/遮挡逻辑，不替代最终场景墙角/船舷/低道具构图、渲染穿模、native Player 激活或 VC-G01。下一张执行卡为 Q13 当前输入焦点与 HUD 隔离复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q13 当前输入焦点/HUD 隔离回归：Q03-A17 后 R1

Q13 在同一 connected Unity Pipeline Editor 中串行重跑 D17A15 与 `D16InputRouterPlayModeTests`：分别 `1/1 PASS`（测试 `3.0398572s`、总 `3.15s`）和 `10/10 PASS`（总 `3.72s`），Editor=`ready/stopped`、error console=`0`。证据覆盖 W/S 玩法输入与 UGUI 导航隔离、Esc 模态恢复、D16 合成键盘/手柄/重绑定和 InitialHarbor/暂停路由。

这仍是 Unity Input System test-device 的输入 owner 证据，不是前台物理 Win32 键盘、渲染 HUD 或 E/I/M/T 业务验收；历史 D16 `9/10` CameraFollow NaN watch item 保留。Q13 保持 `PASS_WITHIN_SCOPE`，下一张执行卡为 Q14 当前船岸整合复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q14 当前船岸整合 owner 回归：Q03-A17 后 R1

Q14 使用当前 D17 源在 connected Unity Pipeline Editor 中重跑 `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence`，完成 `1/1 PASS`，测试耗时 `2.3439029s`、总耗时 `2.46s`，Editor=`ready/stopped`、error console=`0`。证据覆盖岸上→入水→回岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法下船的有界状态链。

Q14 的 `imported_neutral=true` 仍受 Q03 正式动作资源门禁限制，不是动作质量通过；本卡不替代前台物理键盘、E/I/M/T 业务或最终视觉。下一张执行卡为 Q15 当前面板焦点/恢复复核。

权威结果：`game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`。

## 2026-09-13 Q15 当前源码 Pipeline Q03-A17 R1

- Q15 当前重跑 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes` 为 `1/1 PASS`（测试 `12.4690213s`、Pipeline `12.58s`），I/M/T 十轮互斥、重复请求拒绝和 Gameplay 恢复 marker 均为真。
- Editor=`ready/stopped`、Error Console=`0`；运行日志中的 D27 readiness wait 与 D2 procedural Player fallback warning 已单独留痕，不能作为视觉通过。
- Q15 仍只关闭 D29/D16 owner/焦点逻辑，不关闭 native Player、前台物理输入、最终视觉、Q03 动作准入、性能或 VC-G01。下一张可执行卡为 Q16 当前交易桥复核。

结果：[Q15 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q15](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。

## 2026-09-13 Q16 当前源码 Pipeline Q03-A17 R1

- Q16 当前重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest` 为 `1/1 PASS`（测试 `1.9034709s`、Pipeline `2.02s`），覆盖 owner 未就绪、快速双击单 owner、买卖各一次变化、余额不足与满仓拒绝不变。
- Editor=`ready/stopped`、Error Console=`0`；当前 marker seq `21609` 和原始 marker 文件已登记。
- Q16 仍只关闭 D29/D3 交易 owner 桥，不关闭 native Player 鼠标/视觉、完整首航、性能、Q03 动作准入或 VC-G01。下一张可执行卡为 Q17 当前船坞/整备桥复核。

结果：[Q16 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q16](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。

## 2026-09-13 Q17 当前源码 Pipeline Q03-A17 R1

- Q17 当前重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException` 为 `1/1 PASS`（测试 `2.6833445s`、Pipeline `2.79s`），覆盖整备/修船/船坞成功、材料/资金拒绝、重复幂等、取消、异常恢复和主输入恢复。
- Editor=`ready/stopped`、Error Console=`0`；空 owner 异常路径只留下预期 Warning，测试确认状态不变并恢复。
- Q17 仍只关闭 D29/D9/D4 业务 owner 桥，不关闭 native Player 键鼠/截图、最终视觉、性能、完整首航、Q03 动作准入或 VC-G01。下一张可执行卡为 Q18 当前 HUD/本地化尺寸复核。

结果：[Q17 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q17](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。

## 2026-09-13 Q18 当前源码 Pipeline Q03-A17 R1

- Q18 当前回归：`D31Q18HudResolutionTests`=`3/3 PASS`、`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`、Python `tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- Editor=`ready/stopped`、Error Console=`0`；三项自动化证据均已登记。
- Q18 仍为 `PARTIAL`：只关闭代码级 HUD/visual-stage/static UI 回归，不关闭 native Player 截图、字体栅格化、英文最长文案、物理键鼠、性能或最终视觉。下一张可执行卡为 Q19 当前船员/反馈审计。

结果：[Q18 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q18](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。

## 2026-09-13 Q19 当前源码 Pipeline Q03-A17 R1

- Q19 当前回归：`Q19CrewInteractionAuditTests`=`1/1 PASS`、`Save.Tests`=`377/377 PASS`、AgentCore `dotnet build`=`0 warnings / 0 errors`。
- Editor=`ready/stopped`、Error Console=`0`；证据限于离线船员提议、D10 owner 裁决、关系/行为事实和 AgentMemory/domain SQLite 恢复。
- Q19 仍为 `VERIFIED`，不关闭在线 LLM、native Player 对话 UI/输入/视觉、动作/游泳/碰撞、性能或正式视觉门。下一张可执行卡为 Q20 当前 formal resource/presentation 审计。

结果：[Q19 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；交接：[Q19](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。

## 2026-09-13 Q22 当前源码 Pipeline Q03-A17 R1

Q22 当前源码重跑 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`，结果 `1/1 PASS`（测试 `0.1840829s`、Pipeline `0.19s`）。实际 marker 确认水面 `96×96`、sea level `-0.0397`、泡沫 `28`、水材质缺失 `0`、可见船体 `min.y=0.0600`、船壳底部 `-0.0400`、R03 表现音频源 `0`，以及 D9/D28 Calm-Rain-Storm 指标 `1.00/0.90/0.55`、`0/560/980`、`1.00/1.40/2.35`。

以 Console seq `21878` 为基线，Q22 运行期间新增 Error/Warning 均为 `0`；Q20 FMOD seq `21805` 作为历史环境边界继续保留。Editor 最终 `ready/stopped`，没有修改产品代码。Q22 继续 `PARTIAL`：静态船海天气 owner 合同不等于 native Player 游泳、镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md`。下一步为 Q23 当前正式资源集成审计。

## 2026-09-13 Q23 当前源码 Pipeline Q03-A17 R1

Q23 当前源码重跑 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`，结果 `1/1 PASS`（测试 `0.3876112s`、Pipeline `0.39s`）。实际 marker 确认 `active_roots=34`、R03/船实例各 `1`、活动 R03 Renderer=`361`、缺失材质/脚本=`0`、R03 terrain collider=`1`、船体底部 `-0.0400`，港口/船/D27 GUID 有效，关闭并重开后指标一致。

以 Console seq `21890` 为基线，Q23 运行期间新增 Error/Warning 均为 `0`；Q20 FMOD seq `21805` 作为历史环境边界继续保留。Editor 最终 `ready/stopped`，没有修改产品代码。Q23 继续 `PARTIAL`：正式场景集成/重开合同不等于 native Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md`。下一步为 Q24 当前存档恢复诊断。

## 2026-09-13 Q24 当前源码 Pipeline Q03-A17 R1

Q24 当前源码重跑 `D19Q24SaveRestoreDiagnosticTests`，结果 `3/3 PASS`（Pipeline `1.28s`）。陆地 port/clock/money/cargo/crew round-trip、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与 `D2_PlayerSpawnAnchor` 安全恢复、以及 `after_player` 故障后保护上一份好档均通过。

以 Console seq `21900` 为基线，Q24 运行期间新增 Error/Warning 均为 `0`；Q20 FMOD seq `21805` 作为历史环境边界继续保留。Editor 最终 `ready/stopped`，没有修改生产代码。当前 `PlayerState` 仍无独立游泳状态/姿态字段，Q24 继续 `PARTIAL`：存档 owner 通过不等于 native Player 输入、游泳、动作、船控、性能或最终视觉通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md`。下一步为 Q25 当前首航/循环集成诊断。

## 2026-09-13 Q25 当前源码 Pipeline Q03-A17 R1

Q25 当前源码重跑 D30/D23 首航 EditMode=`2/2 PASS`（Pipeline `0.49s`）和 InitialHarbor route PlayMode=`2/2 PASS`（Pipeline `5.42s`，测试耗时 `3.2954252s` 与 `1.9745871s`）。覆盖 D30 七阶段投影、D23 40 tick 首航/抵达/返港/存档投影，以及交易关闭/保存、出航/推进/返港路线。

以 Console seq `21911` 为基线，Q25 运行期间新增 Error=`0`，但保留 `12` 条 D27/D2 Warning；D31 runtime performance approval 仍 pending。Editor 最终 `ready/stopped`，没有修改产品代码。Q25 继续 `PARTIAL`：owner 链/有界路线不等于完整人工输入、动作/游泳/上船视觉、树木、音效、性能或最终 Player 通过。

权威结果：`game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json`；marker：`game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log`；交接：`docs/game/design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md`。下一步为 Q26 当前测量基线。

## 2026-09-13 Q26 当前源码 Pipeline Q03-A17 R1

Q26 当前源码重跑 `D19R03Q26GameplayDefectPlayModeTests`，结果 `17/17 PASS`（Pipeline `41.2s`），覆盖 Q26A01/A02/A03/A04/A05/A06/A07/A08/A09/A10/A11/A12/A13/A14/A16/A17/A18。独立重跑 `Q26A01RealKeyboardInputMovesPlayerWhileSwimming`，结果 `1/1 PASS`（Pipeline `3.75s`，测试 `3.5628066s`），marker 为 `keyboard=W, swimming=True, action=Swimming, planarDistance=0.4095`，水面 `surfaceY=-0.020`。

以完整套件 Console seq `22056` 为基线，新增 Error 查询返回 `0`；warning 查询返回 API 窗口上限 `100`，因此不宣称“无警告”。独立 A01 以 seq `23240` 查询，新增 `0 Error / 5 Warning`，警告是 D27 表现对象等待、未验证导入蒙皮、程序化低模 Player 和绝对 Euler 曲线回退。Editor 最终 `ready / stopped`，测试后执行了 Console clear；Q20 FMOD seq `21805` 仍作为历史环境边界保留。

Q26 继续 `PARTIAL`：本轮关闭的是当前源码 Input System/PlayMode 的有界逻辑与 owner 回归，不是原生 Player 前台物理键盘、GPU/截图、正式 Avatar/动作、目标设备性能或 VC-G01 最终视觉验收。没有新增产品代码变更；下一步按依赖进入 Q27 性能/预算证据。

权威结果：[Q26 Q03-A17 R1](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_RESULT.json)；marker：[Q26 marker](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q03A17_20260913_R1_CONSOLE_MARKER.log)；交接：[Q26](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-13 Q31 当前源码 Pipeline HUD/WASD 与视觉诊断 R1

本轮通过已连接 Unity Pipeline Editor 和隐藏 Windows Player 完成了当前源码的有限运行诊断：MainMenu→`InitialHarbor`、`S` 游泳态定位、`I` 仓库打开/关闭、仓库 modal 内 `W` 不移动且选择不变化均有运行态与 Player 日志证据。新增 `D16A03GameplayWASDDoesNotNavigateHudAndModalWASDDoesNotLeak`，强制 recompile 后目标测试 `1/1 PASS`，`D16InputRouterPlayModeTests` 全类 `11/11 PASS`。

两帧 RenderTexture 仅用于诊断当前 procedural fallback 画面；它们显示低细节场景/Player，不足以通过正式视觉验收。该次 Player 日志严格错误词扫描为 `0`，但运行期 console 在采集/`eval` 后暂时不可达，故不升级为 runtime console 完整通过。前台 Win32 物理键盘、正式动作/Avatar、自然游泳、树木接地、穿模和目标设备性能仍未闭环。

Q31 保持 `PARTIAL`，VC-G01 保持 `IN_PROGRESS`；不要提前推进 VC-G02/VC-G03。结果：[Q31 R1](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_HUD_VISUAL_DIAGNOSTIC_20260913_R1_RESULT.json)；交接：[Q31](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31/Q31.md)。

## 2026-09-13 Q26-A19 D19 舵轮支架材质语义修复

- 选择单一 D19 presentation owner，修复候选语义 `HelmStand` early return 导致的 `R03_Stone` 支架；fallback stand/hub 与候选 `stand/pedestal/base` 现在使用 ship wood，wheel/rudder/steering 使用 helm material。
- 修复前真实定向结果为 `0/1 FAIL`（runtime=`R03_Stone`）；修复后 `Q26A19GeneratedHelmSupportUsesShipWoodMaterial=1/1 PASS`，等待 D7 polish 后 runtime=`D7Polish_HelmStand_0 / brown_planks_03_albedo`。
- 回归：`D19R03Q26GameplayDefectPlayModeTests=18/18 PASS`，`43.68s`；release-like QA build=`0 errors/2 warnings`；Development QA build=`0 errors/1 warning`；Player clean close；Editor=`ready/stopped`；Runtime Pipeline 已恢复关闭。
- 结果：[Q26-A19](../game/tools/results/LOCAL-QUALITY/Q26-A19/RESULT.json)；捕获日志：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q26-A19/Q26_A19_HELM_SUPPORT_WOOD_PIPELINE_CAPTURE.log)；交接：[Q26-A19](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A19/Q26-A19.md)。
- 状态保持：Q26=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。RenderTexture 仅是 hidden Player 诊断画面，不构成前台视觉或人工最终验收；下一步选择新的可测量 owner，不再盲调舵轮坐标。

## 2026-09-13 Q26-A21 默认程序化泳姿连续性

- 新增 PlayMode 回归 `Q26A21ProceduralSwimPoseHasMeasuredContinuousStroke`：默认 Player 进入 `Swimming` 后要求 `Procedural` 姿态源、pitch=`64.0°`、bodyDrop=`-0.601m`，72 帧内测得上臂最大变化 `6.47°`、脚部最大变化 `1.94°`。
- 定向 `1/1 PASS`；完整 `D19R03Q26GameplayDefectPlayModeTests=19/19 PASS`（`48.01s`）；recompile=`failed=false/errors=[]`；Editor=`ready/stopped`；error logs=`0`。
- hidden Development Player 通过 Pipeline 在开放水域执行 W，从 `(0.00,0.40,30.00)` 到 `(0.45,0.40,29.87)`，保持 `Swimming / grounded=false / speed=2.1`；有效静态/移动 RenderTexture 与 Player log 已登记，strict error token=`0`，clean close。
- 结果：[Q26-A21](../game/tools/results/LOCAL-QUALITY/Q26-A21/RESULT.json)；捕获：[Pipeline capture](../game/tools/results/LOCAL-QUALITY/Q26-A21/Q26_A21_SWIM_POSE_PIPELINE_CAPTURE.log)；交接：[Q26-A21](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A21/Q26-A21.md)。
- 状态保持：Q26=`PARTIAL`、Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、Q03=`BLOCKED_ASSET`。这证明连续 procedural fallback，不证明自然 authored swim；下一步应等待/准入正式 swim/action/avatar 资源，不再把回退角度当最终泳姿盲调。

## 2026-09-13 Q26-A22 D29 bootstrap 输入 owner 修复

本轮将用户报告的 W/S 与右上角菜单冲突收窄为一个可执行代码窗口：D29 `AssignDefaultActions()` 后的启动帧仍可能保留 `InputSystemUIInputModule.move`。生产修复在同一 D29 owner 内立即清空该 action；D16 主 HUD 测试增加即时断言，避免把下一帧稳定器行为误当成启动时安全。

验证已完成：D16 两项定向 `1/1 PASS`，D16 输入类 `11/11 PASS`，Q26 完整套件 `19/19 PASS`；`build_236bb62b66a6` 为 `0 errors / 0 warnings`。Unity Pipeline hidden Player 真实走 MainMenu→InitialHarbor、W、I、模态 W、Esc 暂停/恢复和 clean quit，runtime error console=`0`，设置已恢复为 `enableInBuilds=false / port=0 / developmentBuild=false`。

状态不升级：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。注入 Input System 不是前台 Win32 物理键盘；formal Avatar/action、自然 authored swim、植被/穿模、舵位 IK、目标设备性能和最终视觉仍需独立证据。Q31 当前 Player 交互矩阵已在后续条目完成；下一步从 formal action/avatar 资源门禁或另一个有明确 owner/失败证据的缺陷中选择一项，不以 readiness 或截图猜测 PASS。

结果：[Q26-A22](../game/tools/results/LOCAL-QUALITY/Q26-A22/RESULT.json)；交接：[Q26-A22](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q26-A22/Q26-A22.md)。

## 2026-09-13 Q31 当前 Player 交互矩阵（Q26-A22 构建）

Q31 当前 hidden Player 已通过 Unity Pipeline 完成 `E` 登船/下船、港内 `M` 成功、重复 `M` 无重复打开、`T` 打开/重复无重复打开、`Esc` 关闭，以及 `Y/N` 出航后 `M` 被 owner 拒绝。日志记录 `Ship/Character` 控制权、作者下船锚点、市场/酒馆 modal 导航和 D29 拒绝反馈；runtime error console=`0`，Player clean close。

这轮将成功、拒绝、重复操作从“计划覆盖”变成当前 Player 的实际运行记录，但所有键仍由 Pipeline 注入，不能替代前台物理键盘或人工视觉验收。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS` 不变；下一步从 formal action/avatar 资源门禁、自然泳姿、植被/穿模或自然舵位接触中选择有明确 owner 和失败证据的一项。

结果：[Q31 interaction matrix](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_INTERACTION_MATRIX_Q26A22_20260913_RESULT.json)；交接：[Q31](../docs/game/design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-13 当前接管续行：Q00/Q01 对 Q26-A19/Q26-A21 的来源重绑定

Q00 已完成当前工作区、connected Unity Pipeline、D19 builder/prefab、D2 runtime 与 Q26-A21 测试源的重新绑定，并明确 `Q31_Q26A19_HELM_WOOD_DEV_R1.exe` 只包含 Q26-A19 生产变更；Q26-A21 是其后加入的测试文件，不能被当作 Player 二进制内容。Q01 已重新确认根/尺度/owner 边界，保持 `VERIFIED_WITH_SCOPE`，不把隐藏 Pipeline W、RenderTexture、程序化泳姿连续性或材质 owner 证据升级为前台物理键盘、自然动作、自然游泳、无穿模或人工视觉通过。

本轮 Pipeline 最终为 Unity `6000.0.80f1` / `com.unity.pipeline@0.6.0-exp.1` / PID `53332` / `ready/stopped` / RuntimePipeline `enableInBuilds=false, port=0` / error query `0`。一次探索性 eval timeout/reset 被排除而未转化为产品 PASS/FAIL。任务板保持：Q03=`BLOCKED_ASSET`、Q20/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`；下一张依赖卡为 Q05 当前游泳表现回归，然后继续 Q06/Q07/Q09。

权威结果：`game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A21_20260913_RESULT.json`、`game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A21_20260913_RESULT.json`。

## 2026-09-13 当前接管续行：Q05 Q26-A22 后当前源复核

Q05 使用 Q00/Q01 当前来源边界和 Q26-A22 后的 connected Unity Pipeline Editor 重跑 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`，结果 `1/1 PASS`（测试 `11.7257264s`、总计 `11.96s`）。本轮没有修改生产代码或配置；十次 whole-visual-root 游泳倾斜/离水 neutral 恢复、物理根 upright 均保持当前源通过。历史 Pipeline server timeout 只保留为环境证据，Q05 不以此升级为更大视觉状态。

Q05 仍为 `PASS_WITHIN_SCOPE`，不证明 authored 四肢/头部动作、自然泳姿、前台 Win32 输入或人工视觉。结果：`game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张执行卡为 `Q06_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q06 Q26-A22 后当前源复核

Q06 使用 Q05 当前源复核之后的 connected Unity Pipeline Editor 重跑 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite`，结果 `1/1 PASS`（测试 `14.3624642s`、总计 `14.6s`）。本轮没有修改生产代码或配置；十次真实跳跃—落地和出生点附近四方向小步移动仍保持 grounded、非游泳、Transform 有限。Q06 仍为 `PASS_WITHIN_SCOPE`，不扩展为所有岸线/船甲板碰撞或视觉接触通过。

结果：`game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张执行卡为 `Q07_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q07 Q26-A22 后当前源复核

Q07 使用当前源码在 connected Unity Pipeline Editor 串行重跑服务建筑碰撞 owner 与 terrain/dock/gangplank 路线：前者 `1/1 PASS`（测试 `2.7650275s`、总计 `2.96s`），后者 `1/1 PASS`（测试 `5.0460624s`、总计 `5.29s`）。本轮没有修改生产代码或配置；当前四栋服务建筑 D6 collision owner 和已覆盖的双向地形/码头/跳板路线保持通过。

Q07 仍为 `PASS_WITHIN_SCOPE`，不扩展为所有岸线/坡墙、视觉接触、船舷/上下船或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张先处理 `Q08_BLOCKED_THEN_Q09`。

## 2026-09-13 当前接管续行：Q08 Q26-A22 formal action gate

本轮没有运行 Q08 整合套件，而是用当前源码执行 Q03 formal action admission gate：`1/1 PASS`，marker 明确为 `formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`。Q05/Q07 当前子契约均通过，但不能替代正式动作资源，因此 Q08 保持 `BLOCKED`，不是失败也不是 PASS。

解除条件仍为 current-rig authored ground-action 资源或可审计 Avatar/bone-axis adapter，并在同源源码/构建边界重跑 Q08 action-state、movement、swim、grounding integration suite。结果：`game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260913_Q26A22_RESULT.json`；下一张执行卡为 `Q09_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q09 Q26-A22 后当前源复核

Q09 使用当前源码在 connected Unity Pipeline Editor 重跑 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`，结果 `1/1 PASS`（测试 `5.2753674s`、总计 `5.51s`）。本轮没有修改生产代码或配置；非单位船根 HelmPoint/可见舵位锚点、十次登/下船和 30m 远距拒绝均保持通过。

Q09 仍为 `PASS_WITHIN_SCOPE`，不扩展为自然握舵、航行中甲板关系、船体视觉、前台 Win32 输入或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张执行卡为 `Q10_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q10 Q26-A22 后当前源复核

Q10 使用当前源码在 connected Unity Pipeline Editor 重跑 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`，结果 `1/1 PASS`（测试 `3.7330901s`、总计 `3.98s`）。本轮没有修改生产代码或配置；十个船舶平移/转向快照中登船玩家相对船根的位置/旋转保持稳定，并可正常恢复下船。

Q10 仍为 `PASS_WITHIN_SCOPE`，不扩展为开放海航行、前台 W/S/A/D、船舶碰撞、相机构图、自然握舵或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张执行卡为 `Q11_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q11 Q26-A22 后当前源复核

Q11 使用当前源码在 connected Unity Pipeline Editor 重跑 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`，结果 `1/1 PASS`（测试 `5.4638275s`、总计 `5.71s`）。本轮没有修改生产代码或配置；十次合法下船、活动 terrain bounds 外无落点拒绝和状态恢复均通过。

Q11 仍为 `PASS_WITHIN_SCOPE`，不扩展为移动船舶视觉碰撞、所有岸边/跳板出口、前台 Win32 输入、正式动作、相机或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_RESULT.json`；下一张执行卡为 `Q12_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q12 Q26-A22 后当前源复核

Q12 使用当前源码在 connected Unity Pipeline Editor 重跑 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`，结果 `1/1 PASS`（测试 `5.5607355s`、总计 `5.81s`）。本轮没有修改生产代码或配置；十次相机模式/目标切换、受控外部 BoxCollider 的 SphereCast 遮挡收缩、移除障碍后的 20 帧阻尼恢复和有限相机姿态均保持通过，`ResolvedDistance=10.478m`、`collisionChecks=109`。

Q12 仍为 `PASS_WITHIN_SCOPE`，不扩展为最终墙角/船舷/低道具构图、渲染穿模、native Player 输入或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q13_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q13 Q26-A22 后当前源复核

Q13 使用当前源码在 connected Unity Pipeline Editor 串行重跑 D17A15 与 D16 输入路由整类：D17A15 `1/1 PASS`（测试 `7.9642195s`、总计 `8.22s`）；`D16InputRouterPlayModeTests` `11/11 PASS`（总计 `9.7s`）。整类结果包含 `D16A03GameplayWASDDoesNotNavigateHudAndModalWASDDoesNotLeak`，直接覆盖 W/S 不应切换右上角 HUD 导航，以及 Esc 暂停模态恢复。

本轮没有修改生产代码或配置；Editor=`ready/stopped`，自 seq `30989` 起 Error 查询为空。Q13 仍为 `PASS_WITHIN_SCOPE`，不扩展为前台 Win32 物理键盘、渲染 HUD、E/I/M/T 业务、native Player 或人工视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q14_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q14 Q26-A22 后当前源复核

Q14 使用当前源码在 connected Unity Pipeline Editor 重跑 `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence`，结果 `1/1 PASS`（测试 `5.7407475s`、总计 `6.01s`）。本轮没有修改生产代码或配置；岸上→入水→回岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法甲板恢复→合法下船的有界状态链保持通过。

Q14 仍为 `PASS_WITHIN_SCOPE`；`imported_neutral=true` 仍受 Q03 formal action 门禁限制，不扩展为正式动作、前台物理键盘、E/I/M/T 业务、native Player 视觉或人工验收通过。结果：`game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q15_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q15 Q26-A22 后当前源复核

Q15 使用当前源码在 connected Unity Pipeline Editor 重跑 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`，结果 `1/1 PASS`（测试 `16.5811208s`、总计 `16.85s`）。Console marker 确认 `I/M/T` shortcut owner、仓库/市场/酒馆各 10 次循环、临时页互斥、重复请求拒绝和 Gameplay 恢复。

本轮没有修改生产代码或配置；D27 表现等待和 D2 未验证角色/程序化 Player 回退 warning 继续作为视觉/runtime 债务保留。Q15 仍为 `PASS_WITHIN_SCOPE`，不扩展为 native Player 键鼠、渲染面板、业务 owner 真运行结果、性能或 VC-G01。结果：`game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q16_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q16 Q26-A22 后当前源复核

Q16 使用当前源码在 connected Unity Pipeline Editor 重跑 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`，结果 `1/1 PASS`（测试 `4.0648674s`、总计 `4.28s`）。marker 确认未就绪拒绝、双击单 owner、买卖单次状态变化、余额不足和满仓状态不变。

本轮没有修改生产代码或配置；Q16 仍为 `PASS_WITHIN_SCOPE`，不扩展为原生 Player 鼠标命中、渲染面板、性能、完整首航、正式动作或 VC-G01。结果：`game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q17_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q17 Q26-A22 后当前源复核

Q17 使用当前源码在 connected Unity Pipeline Editor 重跑 `D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException`，结果 `1/1 PASS`（测试 `5.5880272s`、总计 `5.81s`）。marker 确认修船成功、材料不足拒绝、重复/同 revision 幂等、取消、船坞成功/拒绝、异常恢复和主输入恢复。

本轮没有修改生产代码或配置；空 owner 预期 Warning 仍作为反馈路径记录。Q17 仍为 `PASS_WITHIN_SCOPE`，不扩展为原生 Player 键鼠、截图、最终视觉、性能、完整首航或 VC-G01。结果：`game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q18_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q18 Q26-A22 后当前源复核

Q18 使用当前源码在 connected Unity Pipeline Editor 重跑 HUD 分辨率与 D31 visual-stage 回归：`D31Q18HudResolutionTests` `3/3 PASS`（1280×720、1920×1080、2560×1440）；`WhalesIslandReef.D31.VisualLogic.Tests` `36/36 PASS`。从仓库根目录执行 `python -m unittest tools.tests.test_d29_interaction_hud`，结果 `30/30 OK`。

本轮没有修改生产代码或配置；首次 Python 子目录导入错误按工作目录问题记录。Q18 仍为 `PARTIAL`，不扩展为原生 Player 截图、字体栅格化、英文最长文案、鼠标命中、前台键盘、性能或最终视觉通过。结果：`game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q19_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q19 Q26-A22 后当前源复核

Q19 使用当前源码在 connected Unity Pipeline Editor 重跑船员审计 `1/1 PASS` 和 `Save.Tests` 全程序集 `377/377 PASS`；随后 `dotnet build game/AgentCore/AgentCore.csproj --nologo` 为 `0 warnings / 0 errors`。

本轮没有修改生产代码或配置。Q19 继续为 `VERIFIED`，只覆盖离线船员提议→owner 裁决、关系事实和 AgentMemory/domain SQLite 保存恢复，不扩展为在线 LLM、原生 Player 对话 UI/输入/视觉、动画、游泳、碰撞表现、性能或正式视觉门禁。结果：`game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q20_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q20 Q26-A22 后当前源复核

Q20 使用当前源码重跑候选资源与运行时 containment：`D19R03Q20VegetationTests` `1/1 PASS`；`Q26A03ActiveLegacyTreeRenderersAndR03VegetationHaveGroundEvidence` `1/1 PASS`，marker 为 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`unsupportedR03Vegetation=0`。

资源门禁仍为真实 `BLOCKED / 0 / 0`，`RenderAuthoredPalms=false`，正式 R03 Prefab 棕榈引用为 `0`。Q20 继续 `PARTIAL/BLOCKED_ASSET`，不扩展为正式棕榈恢复、全镜头接地/遮挡/穿模、native Player 视觉或 VC-G01。结果：`game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q21_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q21 Q26-A22 后当前源复核

Q21 使用当前源码重跑港口服务建筑空间合同：`R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` `1/1 PASS`；四个建筑各 `37 Renderer / 0 missing`，聚合 marker 为 `4 buildings / 4 footprints / 6 required groups / 23 colliders / 1 terrain collider`，X/Z footprint 两两分离。

本轮没有修改产品代码或配置，Editor 结束为 `ready/stopped`，自 seq `32090` 起无新增 Error。Q21 继续 `PARTIAL`，不扩展为原生 Player 镜头/遮挡/键鼠、植被正式资源门禁、性能或最终视觉。结果：`game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q22_CURRENT_PIPELINE_REAUDIT_Q26A22`。

## 2026-09-13 当前接管续行：Q22 Q26-A22 后当前源复核

Q22 使用当前源码在 connected Unity Pipeline Editor 重跑 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`：`1/1 PASS`，测试 `0.1713682s`、Pipeline `0.18s`。marker 再次确认水面 `96×96`、sea level `-0.0397`、岸线 `-0.0048`、泡沫 `28`、船体可见底部 `0.0600`、船壳底部 `-0.0400`、D9 鱼类活动 `1.00/0.90/0.55`、D28 降水 `0/560/980`、波动倍率 `1.00/1.40/2.35`。

清空 Console 后以 seq `32100` 为基线，新增 Error/Warning 均为 `0`，Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有修改产品代码或配置；Q22 继续 `PARTIAL`，不扩展为原生 Player 游泳、船海镜头、泡沫/雾/音效、性能、物理键鼠或最终视觉。结果：`game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q23_CURRENT_PIPELINE_REAUDIT_20260913_R1`。

## 2026-09-13 当前接管续行：Q23 Q26-A22 后当前源复核

Q23 使用当前源码在 connected Unity Pipeline Editor 重跑 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`：`1/1 PASS`，测试 `0.425465s`、Pipeline `0.43s`。marker 确认 `active_roots=34`、R03/船实例各 `1`、活动 R03 Renderer=`361`、缺失材质/脚本=`0`、terrain collider=`1`、ship hull bottom=`-0.0400`，港口/船/D27 GUID 有效且 `reopen_consistency=true`。

清空 Console 后以 seq `32126` 为基线，新增 Error/Warning 均为 `0`，Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有修改产品代码或配置；Q23 继续 `PARTIAL`，不扩展为原生 Player 镜头/键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉。结果：`game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q24_CURRENT_PIPELINE_REAUDIT_20260913_R1`。

## 2026-09-13 当前接管续行：Q24 Q26-A22 后当前源复核

Q24 使用当前源码在 connected Unity Pipeline Editor 执行 `D19Q24SaveRestoreDiagnosticTests`：`3/3 PASS`，Pipeline `1.21s`；陆地 port/clock/money/cargo/crew round-trip、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与 `D2_PlayerSpawnAnchor` 安全恢复、`after_player` 故障后上一份好档保护均通过。

清空 Console 后以 seq `32138` 为基线，新增 Error/Warning 均为 `0`，Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有修改生产代码；`PlayerState` 仍无独立游泳状态/姿态字段，Q24 继续 `PARTIAL`，不扩展为原生 Player 输入、视觉、动作、游泳、树木、音效、性能或最终视觉。结果：`game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 `Q25_CURRENT_PIPELINE_REAUDIT_20260913_R1`。

## 2026-09-13 当前接管续行：Q25 Q26-A22 后当前源复核

Q25 使用当前源码分段重跑：`D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.49s`），`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `6.73s`）。覆盖 D30 七阶段、D23 40 tick 首航/抵达/返港/存档投影，以及 InitialHarbor 交易关闭/保存、出航/推进/返港。

以 Console seq `32160` 为基线，新增 Error=`0`、Warning=`12`，Warning 为 D27 readiness、D2 未验证角色/程序化回退和绝对 Euler 动画回退；Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有修改产品代码或配置；Q25 继续 `PARTIAL`，下一张执行卡为 `Q26_CURRENT_PIPELINE_REAUDIT_20260913_R1`。结果：`game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`。

## 2026-09-13 当前接管续行：Q26 Q26-A22 后当前源复核

Q26 使用当前源码重跑 `D19R03Q26GameplayDefectPlayModeTests`：`19/19 PASS`、Pipeline `48.33s`；定向 A01 W 游泳 `1/1 PASS`（`planarDistance=0.3291`），定向 A04 E 拒绝/登船/下船 `1/1 PASS`（`remote_e_rejected=true / real_e_boarded=true / real_e_disembarked=true`）。

完整套件 Error=`0`，Warning 查询返回 `100` 条 API 窗口上限；定向 A01/A04 各 Error=`0`、Warning=`6`，Warning 为 D27/D2 已知 fallback/readiness。Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有修改产品代码或配置；Q26 继续 `PARTIAL`，下一张执行卡为 `Q27_CURRENT_PIPELINE_REAUDIT_20260913_R1`。结果：`game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`。

## 2026-09-13 当前接管续行：Q27 Q26-A22 后当前源性能诊断

Q27 使用当前源码在 connected Unity Pipeline Editor 执行 `D16PerformanceBudgetTests` EditMode=`6/6 PASS`（`0.02s`）和 `D16A01RuntimePerformanceWindowSamplesContinuously` PlayMode=`1/1 PASS`（Pipeline `64.97s`、测试 `64.742428s`）。实际连续窗口 `60.011505126953125s / 162 samples`，`PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`、`Passed=true`。

本轮报告为 `Null Device / GraphicsDeviceType=Null`，GPU timing 不可用；平均 `30.2886086 FPS`、平均帧 `158.3737030ms`、最长帧 `1120.6174316ms`、超过 `50ms` 的长帧 `84`、最小 FPS `0.8923652`。这些是 headless 诊断风险，不是目标设备性能或全局流畅 PASS；既有有效 standalone R3 仍独立保留为 `60.014019s / 3465 samples / 58.734367 FPS / 21.460548 FPS 1% Low / 622.258118ms longest frame / GPU unavailable`。

以 Console lower-bound seq `33585` 查询，新增 Error=`0`、Warning=`6`（D27/D2 readiness/fallback）；Editor 最终 `ready/stopped`，证据采集后 Console 已清空。本轮没有产品代码或配置变更，也没有基于 Null Device 指标做无证据 D16 优化。Q27 继续 `PARTIAL`，GPU/目标设备性能、平滑性、native Player 截图和 VC-G01 视觉门禁仍开放。结果：`game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_R1_RESULT.json`；下一张执行卡为 Q28 新候选 Player 构建，随后 Q29 自动化路线。

## 2026-09-13：Q31 connected Editor Pipeline capability boundary

本轮用已连接的 Unity Pipeline Editor 做能力验证，不改生产代码或运行时配置。`editor_play` / `editor_stop`、`get_scene_hierarchy` 和 EditMode `D16Accessibility=8/8` 均有真实结果，且 Editor 最终回到 `ready/stopped`。

Editor 当前是 `Null Device / GraphicsDeviceType=Null`。`screenshot view=game` 的停止态和 Play Mode 两次调用都产生同一张 `INVALID_GRAY_FRAME`（`13556 bytes`，SHA256=`557E66D338223BD72B1ED5B680E747BB159FE626F832D752C1B9D8E9B3CFBE98`），并在 Console 产生 `RenderTexture.Create failed`；Scene view 因未打开而没有捕获。这个结果只能说明 Editor screenshot surface 被图形设备阻塞，不能替代已有 hidden Player RenderTexture，也不能升级视觉门禁。

因此 Q31 仍为 `PARTIAL`、VC-G01 仍为 `IN_PROGRESS`，VC-G02/VC-G03 仍锁定。后续继续选择单一 presentation owner，优先使用已有 hidden Player RenderTexture 或真正可用的 Player 图形设备；不把灰帧算作截图 PASS。结果：[Q31 Editor Pipeline capability](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_EDITOR_CAPABILITY_20260913_R1_RESULT.json)。

同一 Pipeline 只读 AssetDatabase 审计补充了资源边界：D20 Player 目录确有 15 个 generic clip（Board/Helm/Fish/Walk 等），但没有 `swim` 名称 clip，Walk/Helm 的曲线绑定样例仍是 `Armature/root/...`；棕榈查询找到 12 个候选/占位 GameObject，但都位于 Candidates 或 Placeholder 路径。候选文件存在不等于 Q03 formal action/swim 或 Q20 formal vegetation 已准入，因此不做默认场景提升，继续保留 Q03=`BLOCKED_ASSET`、Q20=`PARTIAL/BLOCKED_ASSET`。

`Assets/Resources/D20/Formal/D20_PlayerAnimation.controller` 也已由 Pipeline 只读检查：15 个状态中没有 `Swimming`，且没有 transition。下一步需要正式泳姿 clip/骨骼适配和可审计的状态迁移，再考虑接入；不通过复用 Fish 或空 clip 冒充完成。

## 2026-09-13 当前接管续行：Q00/Q01 Q26-A22 R3

本轮完成只读接管审计并把当前来源、构建、运行态和状态边界追加到 Q00/Q01 聚合结果、handoff、STATUS 和当日日志。工作区保持原状：`459` 条 tracked status entries、`10407` 个未跟踪文件、`459 files changed, 40338 insertions(+), 49178 deletions(-)`；没有 reset、checkout、clean、stash 或大范围覆盖。

当前唯一 same-project connected Pipeline Editor 为 Unity `6000.0.80f1`、`com.unity.pipeline@0.6.0-exp.1`、PID `53332`、`127.0.0.1:7800`，最终 `ready/stopped`。默认 Player 是 Q26-A22 `build_236bb62b66a6`，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，Assembly-CSharp SHA256=`50C6D5CC251BB8AB90DE99507F4BD3B6A708EAA34661BA7A65EC9CBDA7A90855`；Q03-A17 formal route 仍仅 opt-in。

Q01 有界直接取样确认 D2 物理根=(0,0.15,-2.70)、单位根缩放、视觉子树=0.66、默认 procedural/FormalAnimatorReady=false、CharacterController=`0.2424353/1.8`、ship scale=`2.57`、游戏/可见舵位=`(0,1.10,-6.68)`、R03 MeshCollider owner=`D19_R03_IslandSurfaceMesh`、逻辑水面=`-0.02`、可见 wave sample=`-0.06`。这些是 owner/尺度诊断，不是视觉修复或前台输入通过。一条 PlayMode 稳定期探索 eval timeout 被排除，随后三条 bounded eval 成功，Editor 最终 ready/stopped，seq `33835` 之后无新错误。

状态不变：Q03=`BLOCKED_ASSET`、Q08=`BLOCKED`、Q20/Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`。下一张可执行卡为 `Q04_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3`，随后 Q05/Q06/Q07/Q09；必须继续使用真实当前源验证，不能用 hidden Player、RenderTexture readiness 或 Null Device 灰帧升级视觉门禁。结果：[Q00 R3](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A22_20260913_LUNA_R3_RESULT.json)、[Q01 R3](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q04 Q26-A22 R3

Q04 使用当前源码和同一 connected Pipeline Editor 定向运行 `D17A07WaterEntryExitSurvivesTenShallowAndDeepCycles`，完成 `1/1 PASS`，测试耗时 `9.094856s`、Pipeline `9.3s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`、Warning=`6`，Warning 为已知 D27/D2 readiness/fallback。没有修改生产代码、Prefab、场景或运行时配置。

Q04=`PASS_WITHIN_SCOPE` 只证明当前 InitialHarbor D2 水域 owner 的十次浅水/深水进出、移动、表面高度和恢复回归；不证明原生 Player W/S、前台键盘焦点、自然 authored swim pose、水材质视觉、compositor 截图、目标设备性能或 VC-G01。下一张可执行卡为 Q05 当前游泳表现回归。结果：[Q04 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q05 Q26-A22 R3

Q05 使用当前源码和同一 connected Pipeline Editor 定向运行 `D17A08SwimmingVisualRootTiltsAndRestoresWithoutTiltingPhysicsRoot`，完成 `1/1 PASS`，测试耗时 `10.4941761s`、Pipeline `10.7s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界累计 Error=`0`、Warning=`12`，Warning 为已知 D27/D2 readiness/fallback。没有修改生产代码、Prefab、场景或运行时配置。

Q05=`PASS_WITHIN_SCOPE` 只证明当前表现层可以在游泳状态下倾斜 visualRoot、保持物理根竖直并在退出后恢复；不证明正式 authored/natural swim、原生 Player W/S、前台键盘焦点、截图、目标设备性能或 VC-G01。下一张可执行卡为 Q06 当前移动/游泳与碰撞回归。结果：[Q05 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q06 Q26-A22 R3

Q06 使用当前源码和同一 connected Pipeline Editor 定向运行 `D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite`，完成 `1/1 PASS`，测试耗时 `11.1847479s`、Pipeline `11.4s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`。没有修改生产代码、Prefab、场景或运行时配置。

Q06=`PASS_WITHIN_SCOPE` 只证明当前 InitialHarbor 十次跳跃/落地与正向、对角、反向小步移动保持有限、接地和非游泳；不证明墙/坡面/岸边/跳板/甲板、视觉接触、原生 Player 输入、目标设备性能或 VC-G01。下一张可执行卡为 Q07。结果：[Q06 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q07 Q26-A22 R3

Q07 使用当前源码和同一 connected Pipeline Editor 串行运行建筑碰撞 owner 与岸线路线两个测试：`R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries`=`1/1 PASS`（`1.9401197s`），`D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps`=`1/1 PASS`（`2.997678s`）。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q07=`PASS_WITHIN_SCOPE` 只证明四栋服务建筑的 HarborCollision owner 和较小范围地形/码头/跳板双向步行合同；不证明所有岸坡、视觉网格接触、船上/登船路线、原生 Player WASD、目标设备性能或 VC-G01。下一步先记录 Q08 的 formal authored action/swim 资源阻塞，再进入 Q09。结果：[Q07 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q08 formal action blocker R3

Q08 重新运行当前 formal action admission gate `FormalActionAdmissionRemainsClosedForGenericOnlyController`，测试本身 `1/1 PASS`，但 seq `34292` 直接记录 `formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`。Editor 最终 `ready/stopped`，seq `34289` 之后 Error=`0`；本轮没有修改产品代码、Prefab、场景或运行时配置，也没有运行不具备前置条件的 Q08 integration suite。

因此 Q08 继续 `BLOCKED`，不是失败也不是 PASS。解除需要当前 rig 可兼容 authored ground-action clip set，或可审计 Avatar/bone-axis adapter 与导入证据；之后再跑 Q08 action-state/movement/swim/grounding 集成。Q31 原生 Player 和视觉验收保持独立。下一步为 Q09 当前 Pipeline 回归。结果：[Q08 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260913_Q26A22_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q09 Q26-A22 R3

Q09 使用当前源码和同一 connected Pipeline Editor 运行 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding`，完成 `1/1 PASS`，测试耗时 `3.2919116s`、Pipeline `3.52s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q09=`PASS_WITHIN_SCOPE` 只证明缩放船体 gameplay helm anchor、十次登船/下船生命周期和远端拒绝；不证明原生 Player E、自然手舵接触、船上运动、UI 焦点、截图或 VC-G01。下一张可执行卡为 Q10。结果：[Q09 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q10 Q26-A22 R3

Q10 使用当前源码和同一 connected Pipeline Editor 运行 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`，完成 `1/1 PASS`，测试耗时 `2.2846007s`、Pipeline `2.56s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q10=`PASS_WITHIN_SCOPE` 只证明当前 D2 sailing snapshot 路径中船体位移/转向时 boarded player 相对甲板稳定并能恢复安全下船；不证明原生 Player WASD、完整航海、船碰撞、自然握舵、镜头/视觉或 VC-G01。下一张可执行卡为 Q11。结果：[Q10 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-13 当前接管续行：Q11 Q26-A22 R3

Q11 使用当前源码和同一 connected Pipeline Editor 运行 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`，完成 `1/1 PASS`，测试耗时 `3.6895698s`、Pipeline `3.93s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q11=`PASS_WITHIN_SCOPE` 只证明十次合法下船恢复和无地面拒绝保留船上控制；不证明原生 Player 输入、移动船碰撞、连续视觉相机、formal action quality 或 VC-G01。下一张可执行卡为 Q12。结果：[Q11 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q26A22_LUNA_R3_RESULT.json)。
## 2026-09-13 当前接管续行：Q12 Q26-A22 R3

Q12 使用当前源码和同一 connected Pipeline Editor 运行 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`，完成 `1/1 PASS`，测试耗时 `3.1757306s`、Pipeline `3.39s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q12=`PASS_WITHIN_SCOPE` 只证明 CameraFollow 模式/目标切换、受控遮挡缩短与阻尼恢复；不证明最终场景墙/船栏/低物件视觉构图、原生 Player、截图、目标设备性能或 VC-G01。下一张可执行卡为 Q13 UI/键盘焦点合同。结果：[Q12 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。
## 2026-09-13 当前接管续行：Q13 Q26-A22 R3

Q13 使用当前源码和同一 connected Pipeline Editor 串行运行三组输入/UI 回归：D17A15=`1/1 PASS`（`3.3452959s`）、D16InputRouter=`11/11 PASS`（`4.86s`）、D16PrimaryHudNavigation=`2/2 PASS`（`2.79s`）。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。

Q13=`PASS_WITHIN_SCOPE` 证明逻辑层 W/S ownership、主 HUD 不导航、模态 W/S 不泄漏、Esc/pause/focus 恢复和 UI move action 抑制重申；不证明前台 Windows Player 焦点、真实物理键盘、渲染 HUD、E/I/M/T 业务流、截图或 VC-G01。下一张可执行卡为 Q14 当前整合路线。结果：[Q13 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。
## 2026-09-13 当前接管续行：Q14 Q26-A22 R3

Q14 使用当前源码和同一 connected Pipeline Editor 运行 `D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence`，完成 `1/1 PASS`，测试耗时 `2.2802981s`、Pipeline `2.5s`。Editor 最终 `ready/stopped`；以 seq `33835` 为下界新增 Error=`0`；没有修改生产代码、Prefab、场景或运行时配置。岸→水→岸、登船/ShipDeck target、船上 W+D 转向、无地面拒绝和合法下船均在范围内；formal imported-rig animation quality 仍 `BLOCKED_BY_Q03`。

Q14=`PASS_WITHIN_SCOPE` 只关闭可执行岸水船 state/owner precheck，不关闭 Q03/Q08、原生 Player E/I/M/T、重复/拒绝业务路径、连续视觉、目标设备性能或 VC-G01。下一张可执行卡为 Q15。结果：[Q14 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。
## 2026-09-13 当前接管续行：Q15 Q26-A22 R3

Q15 使用当前源码和同一 connected Pipeline Editor 运行 `D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes`，完成 `1/1 PASS`，测试耗时 `12.655612s`、Pipeline `12.86s`。真实 Console marker 记录 `shortcut_input=true, keys=I,M,T, legacy_keydown_reached_owner=true` 及 `warehouse_cycles=10, market_cycles=10, tavern_cycles=10, exclusive=true, repeated_request_rejected=true, gameplay_recovery=true`；Editor 最终 `ready/stopped`，以 seq `35283` 为下界 Error=`0`。没有修改生产代码、Prefab、场景或运行时配置。

Q15=`PASS_WITHIN_SCOPE` 只证明 D29/D16 shortcut dispatch、临时页互斥、重复请求拒绝和 gameplay focus 恢复；不证明原生 Player 键鼠焦点、渲染面板、业务 owner 真值、截图、目标设备性能或 VC-G01。下一张可执行卡为 Q16。结果：[Q15 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。
## 2026-09-13 当前接管续行：Q16 Q26-A22 R3

Q16 使用当前源码和同一 connected Pipeline Editor 运行 `D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest`，完成 `1/1 PASS`，测试耗时 `1.923169s`、Pipeline `2.13s`。marker seq=`35575` 记录 owner 未就绪、双击单 owner、买卖各单次变化、金币不足不变和货舱不足不变；Editor 最终 `ready/stopped`，以 seq `35498` 为下界 Error=`0`。没有修改生产代码、Prefab、场景或运行时配置。

Q16=`PASS_WITHIN_SCOPE` 只证明 D29/D3 交易逻辑桥接和拒绝/成功反馈；不证明原生 Player 鼠标命中、渲染市场画面、完整航海、截图、目标设备性能或 VC-G01。下一张可执行卡为 Q17。结果：[Q16 Q26-A22 R3](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q26A22_20260913_LUNA_R3_RESULT.json)。

## 2026-09-14 Q31 connected Editor Pipeline runtime probe

Unity Pipeline 已经可以承担一部分“不接管用户桌面”的运行时诊断：连接现有 Editor、启动/停止 PlayMode、编译执行一次性 C# 探针，并读取当前角色/船/水域/UI owner 的真实运行状态。本轮验证了 Character/Ship 切换、`ph_ship_fishing` parent、HelmPoint/HelmWheel 手部距离、开放水域 `IsSwimming=true`，以及 gameplay 时 UI navigation=false、move action=null。

该能力仍不是最终视觉或真机验收：Editor `Null Device` 无有效 Game View；QA-only 定位不等于完整移动路线；异步测试无终态不计 PASS；程序化低模、formal Avatar/action、自然 swim/IK、植被接地、前台物理键盘和目标设备性能仍是开放门。依赖顺序保持 `VC-G01 → VC-G02 → VC-G03 → D31`，下一张可执行卡应选择有明确 owner 的正式资源/视觉缺陷或在取得可见 GPU/Player 控制面后补最终交互证据，不能用 Pipeline 诊断结果提前收口。

结果：[Q31 Editor runtime probe](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PIPELINE_EDITOR_RUNTIME_PROBE_20260914_RESULT.json)；交接：[Q31 handoff](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q00 当前源与 Q31 R4 Player 重绑定

Q00 已按当前工作区重新核对，而不是继承旧 Player 关系：`5390` status entries、tracked changed=`459`、untracked=`4931`，HEAD=`01024cae...`，工作区未清理。通过同一 connected Unity Pipeline Editor 进入/退出 PlayMode；4 秒时 Player 尚未由 bootstrap 创建，追加等待 8 秒后得到有效 `InitialHarbor / Character / Idle / Procedural / grounded=true / swimming=false` 快照，Editor 最终 `ready/stopped`。

当前 D2 animation/runtime/test hash 与 Q31 R4 默认 Player 已重新登记：D2 animation=`097D8210...A66A42`，D2 runtime=`0A0A17E0...980029`，R4=`build_824c76e4cc26`、EXE=`98751D0D...EDBCB42E81`、Assembly-CSharp=`6EBD285D...1DB822C`。所有相关生产源码时间早于 R4 构建；后续新增的 Pipeline pose probe 位于 `AgentScripts`，只做 ephemeral Editor 诊断，不进入 Player。Q00=`VERIFIED_WITH_SCOPE`，Q03/Q31/VC-G01/Q20/D31 状态不变。

结果：[Q00 20260914](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_PIPELINE_Q26A22_20260914_RESULT.json)；交接：[Q00 handoff](design/common/handoffs/LOCAL-QUALITY/Q00.md)。下一张执行卡为 `Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260914`，再按依赖继续 Q04/Q05；不把 bootstrap 时序、hidden Player 或 Null Device 诊断扩大为视觉通过。

## 2026-09-14 Q01 当前根因/尺度/owner Pipeline Q26-A22 Luna Q01

- Q01 只读卡已完成当前运行态重绑定，状态保持 `VERIFIED_WITH_SCOPE`。同一 connected Editor 中完成初始、上船/下船和 QA-only 开放水域采样；Editor 最终 `ready/stopped`，临时探针编译 diagnostics=`[]`。
- 新的直接事实：D2 root unit scale、初始 Character/Idle/Procedural/grounded，board 后 parent=`ph_ship_fishing`/Ship，open-water 后 Character/Swimming/grounded=false；CharacterController=`0.2424353212/1.7999997139/0.8468632698/0.0193948261`；visualRoot localScale=`0.6642063856`；ship scale=`2.5739560127`；logical water=`-0.0199999996`；HarborCollision=`9/3/4`。
- 本轮仅扩展 `game/AgentScripts/LocalQuality/UnityPipelinePoseAndInputProbe.cs` 的平面诊断输出，没有产品代码/场景/Prefab/Player 变更。保持“不可用截图猜测升级”：Pipeline/Null Device 不替代 foreground Player、physical Win32 input、formal Avatar、自然泳姿、树接地或目标设备性能。
- 状态表：Q01=`VERIFIED_WITH_SCOPE`；Q02 进入下一张 owner/lifecycle re-audit；Q03=`BLOCKED_ASSET`；Q20=`PARTIAL/BLOCKED_ASSET`；Q26/Q31=`PARTIAL`；VC-G01=`IN_PROGRESS`；VC-G02/VC-G03=`LOCKED`；D31-P04/S05/G06=`PARTIAL`。结果：[Q01 20260914](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q26A22_20260914_LUNA_Q01_RESULT.json)。

## 2026-09-14 Q02 当前源码 owner/lifecycle 回归 Q26-A22 Luna

- Q02 当前源码回归完成，状态为 `PASS_WITHIN_SCOPE`：D2 动画 EditMode=`8/8 PASS`（0.56s），D17 Action Quality PlayMode=`16/16 PASS`（81.03s），失败/跳过/不确定均为 `0`。
- 该轮验证 owner/lifecycle、水域十循环、游泳根分离、登船/船动/下船、相机恢复和逻辑 W/S-HUD/Esc 归属；结果只证明代码/状态层，不证明原生 Player、正式动作、自然泳姿、握舵接触、树木接地、截图或目标设备性能。
- 当前源码 hash 与测试 JSON 已落在 Q02 结果；Editor 最终 `ready/stopped`，post-run error query 从 cursor `40526` 返回 `0`，随后清空 Console。没有生产代码变更。
- 依赖更新：Q02 可以收口当前 owner/lifecycle 证据；Q03 仍 `BLOCKED_ASSET`，必须保留正式 Avatar/action 资源缺口；下一张可执行卡为 `Q04_CURRENT_WATER_OWNER_REAUDIT`，不得把 Q02 通过扩大为 VC-G01。

## 2026-09-14 Q03 formal action/avatar 资产门复核 Q26-A22 Luna

- Q03 资产门复核完成，状态继续 `BLOCKED_ASSET`。当前 controller 的 15 个 action clip 实读为 `humanMotion=0`、`genericRoot=15`；`Q03FormalActionAdmissionGateTests`=`1/1 PASS_AS_BLOCKED_GATE`，表示门禁正确保持关闭。
- `Q03FormalAvatarActionResourceContractTests`=`2/2 PASS_WITHIN_SCOPE` 只证明 formal appearance 的 Avatar/action 接口存在，以及 candidate generic path 曲线可解析；不能把 generic transform-path 资源解释为正式 authored locomotion/swim/action 通过。
- 本轮无产品资源/代码/Prefab/场景/Player 变更；默认 consumer 仍为 `0`、`Procedural/FormalAnimatorReady=false`。Q03 继续阻塞 Q08/formal visual admission，但不阻塞独立水域逻辑回归。
- 下一张执行卡为 `Q04_CURRENT_WATER_OWNER_REAUDIT`，重点验证逻辑水域 owner、浅/深水进出、回岸和重复循环；仍不宣称前台 Player、截图或自然泳姿通过。

## 2026-09-14 Q04 当前水域 owner 回归 Q26-A22 Luna

- Q04 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A07=`1/1 PASS`，测试耗时 `10.378946s`、Pipeline `10.81s`，失败/跳过/不确定均为 `0`。
- 真实 PlayMode 日志覆盖十次浅水岸缘与十次深水进入—移动—退出，`surfaceY=-0.02`、游泳 `playerY=0.42`、重新接触地面 `surfaceY=0.09`；未注入 IsSwimming、未替换水面、未关闭碰撞。
- 该卡只关闭当前 D2/OceanAdapter 水域状态 owner 回归，不关闭前台 Player W/S、自然泳姿、材质画面、截图或 VC-G01。Editor 为 `ready/stopped`，无生产代码变更。
- 下一卡为 `Q05_CURRENT_SWIMMING_PRESENTATION_REAUDIT`，继续把泳姿表现与水域逻辑分离，不旋转物理胶囊伪造视觉通过。

## 2026-09-14 Q05 当前游泳表现契约回归 Q26-A22 Luna

- Q05 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A08=`1/1 PASS`，测试耗时 `13.0625438s`、Pipeline `13.49s`，失败/跳过/不确定均为 `0`。
- 十次循环记录 visualRoot pitch=`64°`、rootOffsetY=`0.013..0.034m` 样本，物理根保持 upright，离水 neutral 恢复；该卡未旋转 CharacterController，也没有产品代码变更。
- Q05 只关闭程序化 whole-visual-root 的姿态隔离/恢复契约，不关闭 authored limb/head action、自然泳姿、前台 Player W/S、截图、性能或 VC-G01。下一张为 `Q06_CURRENT_MOVEMENT_COLLISION_REAUDIT`。

## 2026-09-14 Q06 当前跳跃/接地/小步移动回归 Q26-A22 Luna

- Q06 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A09=`1/1 PASS`，测试耗时 `14.033055s`、Pipeline `14.5s`，失败/跳过/不确定均为 `0`。
- 十次 TryJump→grounded、正向/斜向/横向/反向小步移动保持 grounded、non-swimming、finite Transform；worldHeight=`1.80m`、worldRadius=`0.24m`、worldStepOffset=`0.35m`。无产品代码变更。
- Q06 只关闭基础移动/跳跃/接地稳定性，不关闭墙/坡/岸线/跳板/船甲板视觉接触、原生 Player 键盘或 VC-G01。下一步 `Q07_CURRENT_COLLISION_ROUTE_REAUDIT`。

## 2026-09-14 Q07 当前碰撞路线回归 Q26-A22 Luna

- Q07 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：`R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries`=`1/1 PASS`（2.9602606s），`D17A10TerrainDockAndGangplankSupportBidirectionalWalkSteps`=`1/1 PASS`（5.3727658s），失败/跳过/不确定均为 `0`。
- 真实 PlayMode 日志确认 D6_HarborCollision 为 owner，4 个服务建筑 collider 已安装；DockGround=`0.50`、GangplankGround=`0.88`，非可行走碰撞攀爬被阻止并恢复高度。无产品代码/场景/Prefab/config 变更，Editor 最终 `ready/stopped`。
- Q07 只关闭服务建筑/码头/跳板的当前逻辑碰撞子契约，不关闭所有岸线/坡/墙、视觉贴合、船甲板、native Player 键盘、植被接地、无穿模或 VC-G01。Q08 仍被 Q03 formal action 资产门阻塞；下一张独立可执行卡为 `Q09_CURRENT_DECK_ANCHOR_REAUDIT`。

## 2026-09-14 Q08 当前 formal action 依赖阻塞复核 Q26-A22 Luna

- Q08 保持 `BLOCKED`。Q03 当前门禁为 `1/1 PASS_AS_BLOCKED_GATE`，实测 marker=`formalActionAdmission=BLOCKED; clips=15; humanMotion=0; genericRoot=15`；默认 InitialHarbor formal consumer=`0`、FormalAnimatorReady=`false`。
- Q05 的 D17A08 与 Q07 的建筑 owner/D17A10 证据均已在各自范围内通过，但不能合并成“输入→状态→位移→动作→姿态”的正式整合证据。本轮 Q08 套件 `NOT_RUN`，没有产品代码或配置变更，也没有使用 neutral-only/generic diagnostic 伪造 PASS。
- 解除条件：提供当前 rig 可审计的 authored ground-action 资源或骨骼轴适配器，并以同源源码/构建重跑 Q08 action-state、movement、swim、grounding suite。Q08 阻塞不阻止 Q09；Q31 native Player 与视觉门禁仍独立。
- 结果：[Q08 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_BLOCKED_20260914_LUNA_RESULT.json)；下一张独立执行卡为 `Q09_CURRENT_DECK_ANCHOR_REAUDIT`。

## 2026-09-14 Q09 当前甲板锚点/登船回归 Q26-A22 Luna

- Q09 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A11=`1/1 PASS`，测试耗时 `5.5399619s`、Pipeline `6.0s`，失败/跳过/不确定均为 `0`。
- 运行日志确认十次真实 TryToggleShip 登船—下船、`maxDeckHeightDelta=0.0000`、`maxDeckAnchorDistance=0.0000`、shipScale=`(2.57,2.57,2.57)`，并覆盖 30m 远距拒绝；执行后 Console `Error=0 / Warning=0`，Editor `ready/stopped`。
- 本轮无产品代码/场景/Prefab/config 变更。Q09 只关闭缩放锚点/重复生命周期/远距拒绝子契约，不关闭自然握舵、航行中船动关系、安全下船边界、视觉清除、native Player 输入或 VC-G01。下一卡为 `Q10_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q09 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q09 capture](../../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q09](../design/common/handoffs/LOCAL-QUALITY/Q09/Q09.md)。

## 2026-09-14 Q10 当前航行中甲板关系回归 Q26-A22 Luna

- Q10 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A12=`1/1 PASS`，测试耗时 `3.9655668s`、Pipeline `4.45s`，失败/跳过/不确定均为 `0`。
- 十个 D2 航行快照中最大船位移 `1.498m`、最大航向变化 `32.000°`，玩家相对位置/旋转误差均为 `0.000000`，并恢复正常下船。Console 为 `Error=0 / Warning=6`；6 条已知 D2/D27 fallback/初始化 warning 已登记，不作为零警告通过。
- 本轮无产品代码/场景/Prefab/config 变更。Q10 只关闭 snapshot 驱动的相对甲板稳定性，不关闭开放海航行、native Player W/S/A/D、船舶碰撞、镜头构图、自然握舵、正式动作资源或 VC-G01。下一卡为 `Q11_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q10 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q10 capture](../../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q10](../design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。

## 2026-09-14 Q11 当前安全下船回归 Q26-A22 Luna

- Q11 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A13=`1/1 PASS`，测试耗时 `5.4956044s`、Pipeline `5.98s`，失败/跳过/不确定均为 `0`。
- 日志确认十次合法 TryToggleShip 下船、terrain bounds 外无落点拒绝，最终 grounded=`True`、mode=`Character`；Console 为 `Error=0 / Warning=1`，唯一 warning 是预期的 `disembark_unavailable` 拒绝，已登记而未隐藏。
- 本轮无产品代码/场景/Prefab/config 变更。Q11 只关闭当前合法/无落点下船状态与恢复子契约，不关闭移动船舶视觉碰撞、所有岸边/跳板出口、native Player 输入、正式动作、相机或 VC-G01。下一卡为 `Q12_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q11 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q11 capture](../../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q11](../design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。

## 2026-09-14 Q12 当前相机遮挡/恢复回归 Q26-A22 Luna

- Q12 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A14=`1/1 PASS`，测试耗时 `5.9748085s`、Pipeline `6.47s`，失败/跳过/不确定均为 `0`。
- 十次人物/总览/船上模式循环中，受控 BoxCollider 使相机距离 `5.500m→3.746m`，障碍移除后恢复 `5.500m`，遮挡检查 `109` 次；Console 为 `Error=0 / Warning=6`，已知 D2/D27 warning 已保留并登记。
- 本轮无产品代码/场景/Prefab/config 变更。Q12 只关闭 CameraFollow 的模式/目标/受控遮挡恢复子契约，不关闭最终场景构图、渲染穿模、native Player 激活或 VC-G01。下一卡为 `Q13_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q12 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q12 capture](../../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q12](../design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。

## 2026-09-14 Q13 当前输入/UI 隔离回归 Q26-A22 Luna

- Q13 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A15=`1/1 PASS`（测试 `5.8881073s`、Pipeline `6.35s`），D16InputRouter=`11/11 PASS`（Pipeline `10.23s`），D16PrimaryHudNavigation=`2/2 PASS`（Pipeline `6.88s`），失败/跳过/不确定均为 `0`。
- 实际 marker 覆盖十轮 W/S 后 `normal_navigation=false`、Esc 模态让权、D16 W/S/InitialHarbor/登船/重绑定/合成输入，以及 UI Move action 被外部恢复后的重新隔离。三组 Error 均为 `0`；warning `6/7/12` 为已知 D2/D27 fallback/初始化项，已保留。
- 本轮无产品代码/场景/Prefab/config 变更。Q13 只关闭 Unity Input System/UI owner 逻辑，不关闭前台 Win32 键盘、渲染 HUD、E/I/M/T 业务流、正式动作、原生 Player 视觉或 VC-G01。下一卡为 `Q14_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q13 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q13 capture](../../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q13](../design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。

## 2026-09-14 Q14 当前船岸整合回归 Q26-A22 Luna

- Q14 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D17A16=`1/1 PASS`，测试耗时 `4.2337608s`、Pipeline `4.73s`，失败/跳过/不确定均为 `0`。
- 运行日志确认岸上→入水→回岸→登船→ShipDeck→W+D 转向→无落点拒绝→合法下船链；marker=`imported_neutral=False` 已对照测试代码确认是程序化 Player 分支，不是正式动作资源通过，Q03 仍 `BLOCKED_ASSET`。
- Console 为 `Error=0 / Warning=7`，含预期无落点拒绝和已知 D2/D27 fallback/初始化 warning，已保留；Editor `ready/stopped`。无产品代码/场景/Prefab/config 变更。
- Q14 只关闭有界船岸 state/owner 链，不关闭正式动作、前台 Win32 键盘、E/I/M/T、连续截图、最终视觉或 VC-G01。下一卡为 `Q15_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q14 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q14 capture](../../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q14](../design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。

## 2026-09-14 Q15 当前 I/M/T 面板焦点回归 Q26-A22 Luna

- Q15 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D29Q15=`1/1 PASS`，测试耗时 `18.118572s`、Pipeline `18.63s`，失败/跳过/不确定均为 `0`，Console `Error=0 / Warning=0`。
- 真实 marker 确认 I/M/T shortcut 到达 D29 owner，仓库/市场/酒馆各 `10` 次打开/重复/关闭/恢复，`exclusive=true`、`repeated_request_rejected=true`、`gameplay_recovery=true`。
- 本轮无产品代码/场景/Prefab/config 变更。Q15 只关闭 D29/D16 的快捷键 owner 与模态焦点逻辑，不关闭前台物理键鼠、渲染面板、业务 owner 真实结果、正式动作、性能或 VC-G01。下一卡为 `Q16_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q15 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q15 capture](../../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q15](../design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。

## 2026-09-14 Q16 当前交易桥回归 Q26-A22 Luna

- Q16 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D29Q16=`1/1 PASS`，测试耗时 `3.7348475s`、Pipeline `4.11s`，失败/跳过/不确定均为 `0`。
- 真实 marker 确认 `not_ready`、快速双击单 owner、买卖一次变化、余额不足/满仓不变均通过；Console `Error=0 / Warning=6`，已知 D2/D27 warning 已保留。
- 本轮无产品代码/场景/Prefab/config 变更。Q16 只关闭 D29/D3 交易 owner 与拒绝反馈子契约，不关闭原生 Player 鼠标、渲染面板、性能、完整航程、正式动作或 VC-G01。下一卡为 `Q17_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q16 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q16 capture](../../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q16](../design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。

## 2026-09-14 Q17 当前船坞/整备桥回归 Q26-A22 Luna

- Q17 当前源码定向回归完成，状态为 `PASS_WITHIN_SCOPE`：D29Q17=`1/1 PASS`，测试耗时 `4.9706462s`、Pipeline `5.38s`，失败/跳过/不确定均为 `0`。
- 真实 marker 确认修船/材料不足/重复与同 revision 幂等、取消、船坞成功/拒绝、异常恢复和 primary input 恢复；Console `Error=0 / Warning=7`，空 owner 异常 warning 为预期故障注入路径，已保留。
- 本轮无产品代码/场景/Prefab/config 变更。Q17 只关闭 D29/D9/D4 owner bridge 子契约，不关闭原生 Player 键鼠、截图、视觉、性能、完整航程、正式动作或 VC-G01。下一卡为 `Q18_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q17 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q17 capture](../../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q17](../design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。

## 2026-09-14 Q18 当前 HUD/视觉逻辑回归 Luna

- Q18 继续 `PARTIAL`。使用当前连接的 Unity Pipeline Editor 完成 `D31Q18HudResolutionTests`=`3/3 PASS`（1280×720、1920×1080、2560×1440）、`WhalesIslandReef.D31.VisualLogic.Tests`=`36/36 PASS`，以及仓库根 Python `tools.tests.test_d29_interaction_hud`=`30/30 OK`。
- 本轮无生产代码、Prefab、场景或运行时配置变更；Editor 最终 `ready/stopped`，Console `16` 条日志中 `Error=0 / Warning=0`，随后已清空。
- 该证据只关闭代码级 16:9 HUD 几何、D31 visual-stage 逻辑和 D29 静态回归子契约，不关闭原生 Player/GPU 截图、字体栅格化、英文最长文案、物理鼠标/前台键盘、性能或最终视觉门禁。
- 结果：[Q18 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q18 capture](../../game/tools/results/LOCAL-QUALITY/Q18/Q18_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q18](../design/common/handoffs/LOCAL-QUALITY/Q18/Q18.md)。下一张可执行卡为 `Q19_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-14 Q19 当前船员/存档回归 Luna

- Q19 当前源码定向回归完成，状态为 `VERIFIED`：`Q19CrewInteractionAuditTests`=`1/1 PASS`；`Save.Tests`=`377/377 PASS`，failed/skipped/inconclusive 均为 `0`；`dotnet build AgentCore`=`0 warnings / 0 errors`。
- Pipeline Console 为 `Error=0 / Warning=10`，10 条为已知 D2 动作/rig 准入 warning，已登记；Editor `ready/stopped`，取证后清空。
- 本轮无生产代码、Prefab、场景或运行时配置变更。Q19 只关闭离线 crew proposal/owner/relation/AgentMemory-save 子契约，不关闭在线 LLM、原生 Player 对话 UI/输入/视觉、动作、游泳、碰撞、性能或正式视觉门禁。
- 结果：[Q19 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q19 capture](../../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q19](../design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。下一张可执行卡为 `Q20_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-14 Q20 当前植被接地/遗留 Renderer 隔离回归 Luna

- Q20 继续 `PARTIAL/BLOCKED_ASSET`。当前候选资源测试 `D19R03Q20VegetationTests`=`1/1 PASS`，R03 接地/遗留 Renderer 隔离测试=`1/1 PASS`；marker=`activeLegacyTreeRenderers=0, activeR03Vegetation=24, unsupportedR03Vegetation=0, vegetation_grounded=26, vegetation_hidden_without_terrain=0`。
- D18 ready catalog 仍为 `BLOCKED / 0 / 0`，builder 仍 `RenderAuthoredPalms=false`，正式 R03 Prefab 棕榈引用仍为 `0`。没有把 `palm-straight.fbx` 晋升为正式资源，也没有修改生产代码、Prefab、场景或运行时配置。
- Console 为 `Error=0 / Warning=6`，Editor `ready/stopped`，取证后清空。Q20 只关闭候选 bounds/当前 R03 接地/legacy quarantine 子契约，不关闭正式棕榈、全镜头视觉、原生 Player/GPU 或 VC-G01。下一张可执行卡为 `Q21_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q20 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q20 capture](../../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q20](../design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-14 Q21 当前港口空间静态回归 Luna

- Q21 继续 `PARTIAL`。当前源码 `R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`；marker=`4 buildings / 4 footprints / 6 required groups / 23 colliders / 1 R03 terrain collider / 22 D27 owner colliders`，四服务建筑各 `37 Renderer / 0 missing`、bounds `min.y≈0.225`、XZ 无重叠。
- 本轮无生产代码、Prefab、场景或运行时配置变更；Editor `ready/stopped`，Console `Error=0 / Warning=0`，取证后清空。
- Q21 只关闭静态 R03 space/owner/anchor 子契约，不关闭原生 Player 镜头/遮挡/键鼠可达性、鼠标命中、植被准入、性能或最终视觉。下一张可执行卡为 `Q22_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q21 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q21 capture](../../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q21](../design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

## 2026-09-14 Q22 当前船海天气静态回归 Luna

- Q22 继续 `PARTIAL`。当前源码 `R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`；marker 确认水面 `96×96`、sea level `-0.0397`、泡沫 `28`、船体/船壳 `0.0600/-0.0400`、R03 音频源 `0`、D9 鱼类活动 `1.00/0.90/0.55`、D28 降水 `0/560/980`，测试同时断言波动倍率 `1.00/1.40/2.35`。
- 本轮无生产代码、Prefab、场景或运行时配置变更；Editor `ready/stopped`，Console `Error=0 / Warning=0`，取证后清空。
- Q22 只关闭静态 sea/ship/weather owner 子契约，不关闭真实 Player 游泳、船海镜头、泡沫/雾/音效播放、性能、物理键鼠或最终视觉。下一张可执行卡为 `Q23_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q22 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q22 capture](../../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q22](../design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。

## 2026-09-14 Q23 当前正式场景集成回归 Luna

- Q23 继续 `PARTIAL`。当前源码 `InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`；marker=`active_roots=34 / r03_instances=1 / ship_instances=1 / r03_renderers=361 / missing_materials=0 / missing_scripts=0 / terrain_colliders=1 / ship_hull_bottom_y=-0.0400`，港口/船/D27 GUID 有效，重开一致。
- 本轮无生产代码、Prefab、场景或运行时配置变更；Editor `ready/stopped`，Console `Error=0 / Warning=0`，取证后清空。
- Q23 只关闭正式场景 Prefab 挂接/唯一性/缺失项/重开子契约，不关闭原生 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉。下一张可执行卡为 `Q24_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q23 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q23 capture](../../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q23](../design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。

## 2026-09-14 Q24 当前存档恢复回归 Luna

- Q24 继续 `PARTIAL`。当前源码 `D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`：陆地 port/clock/money/cargo/crew 恢复、出海船位 `(41.5,-17.25)`/航向 `212`/速度 `2.25` 与港口安全出生锚点恢复、失败保存保护上一份好档均通过。
- 本轮无生产代码、Prefab、场景或存档 schema 变更；Editor `ready/stopped`，Console `Error=0 / Warning=0`，取证后清空。
- `PlayerState` 仍无独立游泳状态/姿态字段。Q24 只关闭 Save owner 的陆地/出海/坏档保护子契约，不关闭原生 Player 输入、游泳视觉、动作、树木、音效、性能或最终视觉。下一张可执行卡为 `Q25_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q24 20260914 Luna](../../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q24 capture](../../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q24](../design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。

## 2026-09-14 Q25 当前首航/路线回归 Luna

- Q25 继续 `PARTIAL`。`D19Q25FirstVoyageLoopDiagnosticTests`=`2/2 PASS`（Pipeline `0.51s`）；`D19R02A03RoutePlayModeTests`=`2/2 PASS`（Pipeline `7.08s`）。
- 覆盖 D30 `preparation>market>tavern>shipyard>boarded>departed>returned`、D23 `40` tick 首航/抵达/返港/存档投影，以及有限 InitialHarbor 交易关闭/保存、出航/推进/返港。Console 总量 `129`，API 返回 `100` 条，其中 Error=`0`、Warning=`11`；接口有上限，不宣称总 Warning 为零。
- 本轮无生产代码、Prefab、场景或运行时配置改动。结果只证明当前 owner 路线回归，不关闭完整人工游玩、原生 Player 输入、镜头、动作/游泳/上船视觉、树木接地、音效、性能或最终视觉。下一张卡为 `Q26_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q25 当前复核 Luna](../../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q25 capture](../../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q25](../design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。

## 2026-09-14 Q26 当前 PlayMode/输入回归 Luna

- Q26 继续 `PARTIAL`。清 Console 后 `D19R03Q26GameplayDefectPlayModeTests`=`19/19 PASS`（Pipeline `50.57s`）；A01 W 游泳=`1/1 PASS`（`4.77s`）；A04 E 拒绝/登船/下船=`1/1 PASS`（`4.26s`）。
- A01 marker=`keyboard=W / swimming=True / planarDistance=0.1725 / surfaceY=-0.020`；A04 marker=`remote_e_rejected=true / real_e_boarded=true / real_e_disembarked=true / final_mode=Character`。Q26A21 `pitch=64.0 / maxArmDelta=6.64 / maxFootDelta=1.99`；Q26A03 旧树 `0`、R03 植被 `24`、unsupported `0`；D26 `vegetation_grounded=26 / vegetation_hidden_without_terrain=0`。
- 全量 Console 取证返回 Error=`0`、Warning=`102`、Log=`898`，接口上限为 `1000`；已知 D2/D27 fallback/readiness Warning 继续保留。Editor `ready/stopped`，取证后 Console 已清空。本轮无生产代码、Prefab、场景或运行时配置改动。
- 本轮只关闭当前源码 PlayMode/Input System 的有界回归，不关闭前台 Win32 物理键盘、compositor-visible Player 画面、正式 Avatar/动作、自然泳姿、树木/穿模视觉、音效、目标设备性能或 VC-G01。下一张卡为 `Q27_CURRENT_PIPELINE_REAUDIT`。
- 结果：[Q26 当前复核 Luna](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q26 capture](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q26](../design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-14 Q27 当前性能窗口回归 Luna

- Q27 继续 `PARTIAL`。`D16PerformanceBudgetTests`=`6/6 PASS`；`D16A01RuntimePerformanceWindowSamplesContinuously`=`1/1 PASS`，窗口 `60.0172844s / 162 samples`，Pipeline `63.63s`。
- 报告为 connected Editor `Null Device / GraphicsDeviceType=Null`：平均 `28.6217 FPS`、最小 `0.8574 FPS`、平均帧 `165.513ms`、最长帧 `1166.361ms`、超过 `50ms` 长帧 `85`、GPU timing unavailable；探针 owner 与 `BudgetSatisfied=true` 合同成立。
- Console Error=`0`、Warning=`6`、Log=`58`；警告为 D2/D27 fallback/readiness，D7 audio `fallback=True`、D31 runtime performance approval pending 继续保留。无生产代码、Prefab、场景或运行时配置改动；Editor `ready/stopped`，Console 已清空。
- Null Device 采样只作为诊断风险，不关闭目标设备性能、GPU、流畅度、原生 Player 视觉/输入或 VC-G01。下一张卡为 `Q28` 新候选 Player build_id。
- 结果：[Q27 当前复核 Luna](../../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q27 capture](../../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；原始报告：[D16 window](../../tools/results/d16_a01_performance_window_report.json)；交接：[Q27](../design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-14 Q28 当前候选 R10 构建与 D17 守卫 Luna

- Q28 继续 `PARTIAL`。dry-run=`valid=true / validationErrors=[]`；`build_a76b35e7f109` 返回 `Succeeded`，但 BuildReport `totalErrors=1`（Incremental player build 的 Pipeline `/api/exec` 主线程超时 `30000ms`）和 `totalWarnings=1`（`enableInBuilds=false`），所以不写成零错误构建。
- R10 为当前源码 `StandaloneWindows64` 非开发候选，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，`Assembly-CSharp.dll` SHA256=`FF699A91A9EE7F9E5B78F984C38B6442371EB82B06F8ABA6E12CA304A91738DB`。`developmentBuild=false / allowDebugging=false / connectWithProfiler=false / runtimePipeline.enableInBuilds=false / port=0`。
- D17 A08 当前源码=`1/1 PASS`，Pipeline `13.92s`；rootAngle=`64.00°`、rootOffsetY=`0.014..0.033`、hand/head=`0.929..0.989`。Console Error=`0`、Warning=`6`、Log=`104`，Editor `ready/stopped`，取证后清空。本轮无生产代码、Prefab、场景或运行时配置改动。
- R10 不能关闭 Player 前台窗口、物理 WASD/E/I/M/T/Esc、截图、GPU/目标设备性能、正式 Avatar/动作、自然泳姿、树木/穿模视觉或 VC-G01。下一张卡为 `Q29_CURRENT_PLAYER_ROUTE_REAUDIT`。
- 结果：[Q28 当前复核 Luna](../../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q28 capture](../../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q28](../design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。

## 2026-09-14 Q29 当前源码 Pipeline R7 隔离 Player 交互复核

Q29 使用当前源码而不是旧候选重新构建 `Q29_RUNTIME_PIPELINE_QA_R7.exe`，临时启用 `enableInBuilds=true / port=7902 / developmentBuild=true`，通过隐藏 GUI Player 的 loopback Runtime Pipeline 完成了一条干净路线。MainMenu 的 NewGame/Confirm 使用 `simulate_pointer move/down/wait/up`，键盘使用 `simulate_key down/wait/up`；没有激活用户前台窗口、没有使用 Computer Use、没有发送物理键盘。

- 构建 `build_37e34d20a804`：`StandaloneWindows64`、`MainMenu + Bootstrap + InitialHarbor`、`Development + DetailedBuildReport`、`Succeeded`、`0 errors / 0 warnings`、`220872966` bytes。EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`；`Assembly-CSharp.dll` SHA256=`6EBD285D9A70A88996E4E1D6CACD11EA04C4A880BF6A46A999DA6A2761DB822C`。
- 当前运行 marker：W 角色移动；I 仓库与模态内 W 阻断；M 市场打开/重复/Esc；T 酒馆打开/重复/Esc；E 登船/下船/再次登船；Y 出航；航行中 M 被 D3 owner 拒绝；船上 W/A 位移；Esc `TimeScale=0→1` 暂停恢复。入口从 MainMenu 加载 `InitialHarbor` build index `2`。
- Runtime `console --level error` 返回 `0`；Player log 大小 `179991` bytes、大小写敏感 `ERROR|EXCEPTION|CRASH|FATAL=0`；runtime `quit` 后 PID 和 descriptor 均不存在。
- QA 设置已经 `dry_run + confirm=true` 恢复并回读为 `enableInBuilds=false / port=0 / developmentBuild=false / allowDebugging=false / connectWithProfiler=false`；Editor=`ready/stopped`。一次引用 `UnityEngine.InputSystem` 的 eval 编译失败仅属于 probe 参数边界，接受的 UnityEngine-only reflection probe 成功。

本轮状态只升级为 Q29 子范围 `PASS_WITHIN_SCOPE`，Q29 父状态保持 `PARTIAL`。隐藏 Pipeline 输入/状态证据不能替代前台物理键盘、compositor-visible 截图、目标设备性能、正式动作/Avatar、自然泳姿、植被准入或最终视觉验收。VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`；下一步进入 Q30 当前源码独立审查，再进入 Q31 最终 native Player 视觉门禁。

权威结果：[Q29 R7](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q29 R7 log](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q29](../design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。

## 2026-09-14 Q30 当前源码独立审查 Luna

Q30 继续 `PARTIAL`。本轮先发现并修复一个真实的静态测试契约回归：D19-R03 当前舵轮 owner 已采用已登记的暖木 `0.34/0.16/0.055`，但 Python 测试仍断言上一版近黑色值；原始定向套件实际为 `64/65`，同步断言后恢复 `65/65 PASS`。没有修改 Unity 生产源、Prefab、场景或运行时配置。

- Connected Unity Pipeline 当前源码结果：`D16AccessibilityTests=8/8`、`D16InputRouterPlayModeTests=11/11`、`D17ActionQualityPlayModeTests=16/16`、`VcG01WorldLogicTests=14/14`；Q07 建筑碰撞=`1/1`；Q15/Q16/Q17 各=`1/1`。PlayMode 使用 async 路径；Pipeline 对同步 HTTP PlayMode 的 domain-reload 拒绝不计为测试 verdict。
- Editor Error Console=`0`；Warning 返回 `141` 条且 `dropped=false`，为已知 D2/D27 fallback/readiness/animation 警告和 Q17 预期异常路径反馈；取证后清 Console，Editor=`ready/stopped`。
- Q29 R7 当前源码隐藏 Player 路线作为依赖证据已覆盖 W、I 模态 W 阻断、M/T 重复与 Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A、Esc 暂停恢复，runtime error=`0`、严格 Player 错误词=`0`、clean close。

本轮只收口测试契约与当前源码 owner 回归，不关闭 compositor-visible 前台 Player 画面、Win32 物理键鼠、正式 authored action/avatar、自然泳姿/握舵、正式植被准入、全镜头接地/穿模或目标设备性能。Q30=`PARTIAL`，VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`；下一张可执行卡仍为 Q31 最终 native Player 视觉门禁。

结果：[Q30 当前审查 Luna](../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q30 capture](../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA.log)；交接：[Q30](../design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-14 Q31 R4 RenderTexture 直接复核

对当前源码 R4 hidden Player 的四张有效 `1280x720` RenderTexture 做了直接图像复核：初始港口、静态游泳、移动游泳、登船舵轮均存在且 hash 与 R4 result 一致。图像能证明当前 Player 真的渲染出港口/水面/程序化玩家/暖木舵轮，但不构成最终视觉 PASS。

- 初始港口帧仍呈现低模/程序化场景，建筑外壳、道路、岸线和地表层级边界粗糙；游泳帧显示水平 fallback 玩家、伸展四肢和水面阴影，但没有 authored swim cycle、自然水接触或正式 Avatar 证据；登船帧显示暖棕舵轮和船上玩家，但不证明自然 IK/上船动作。
- 固定镜头未复现 visible legacy tree/palm renderer；这只是当前资源隔离和镜头证据，不等于正式植被准入或全镜头接地通过。
- Q31 继续 `PARTIAL`，VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`；offscreen RenderTexture 不是 compositor-visible 前台窗口或人工最终视觉验收。下一步仍是 formal action/avatar、natural swim/helm contact、vegetation all-camera grounding 或可用前台 visual gate。

结果：[Q31 R4](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_REAUDIT_Q26A22_20260914_R4_RESULT.json)；[Q31 canonical](../../game/tools/results/LOCAL-QUALITY/Q31/RESULT.json)；交接：[Q31](../design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q20-A04 candidate palm controlled Player evidence

Q20-A04 在单一 D19-R03 presentation owner 内接入一个 `Candidate/ImportVerified` 的 `tree_palm.fbx`，对象名为 `KenneyNaturePalmCandidate`。实现复用了现有候选 bounds-grounding 逻辑，并增加 PlayMode 守卫：候选必须只有 `1` 个可见 Renderer、命中自有 `D19_TerrainRoot`、接地误差不超过 `0.08m`，且不拥有 Collider。没有修改正式资源准入，`RenderAuthoredPalms=false`、正式 Prefab 棕榈引用=`0`、D18 ready catalog=`BLOCKED / 0 / 0` 保持。

证据：EditMode `1/1 PASS`；`D19R03Q20VegetationPlayModeTests` `2/2 PASS`；`build_9d09e6c266c9` `Succeeded / 0 errors / 0 warnings`。使用隐藏 GUI WindowsPlayer（Pipeline `7903`）进入 `InitialHarbor` 后，真实 runtime eval 读取到 `Renderer=1`、`Collider=0`、bounds `1.0939..4.1231`；忽略触发器后 `D19_TerrainRoot y=1.0699`，接地误差 `0.0240m`。Player error console=`0`、严格错误词=`0`、clean close；设置已恢复到 Editor runtime disabled/non-development。

Player 真实 RenderTexture 证据为三张 `1280x720` PNG：组合帧显示候选但局部被 dock 遮挡，隔离帧可读，候选/terrain layer 帧仅作诊断。它们不等价于前台物理 Win32 输入、全镜头地面/遮挡/穿模、目标设备性能或人工最终视觉。因此子卡为 `PASS_WITHIN_SCOPE`，Q20 仍 `PARTIAL/BLOCKED_ASSET`，Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`，下一依赖任务为 Q21。详见 [Q20-A04 RESULT](../../game/tools/results/LOCAL-QUALITY/Q20-A04/RESULT.json) 和 [Q20-A04 handoff](design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

## 2026-09-14 Q21 current-source revalidation after Q20-A04

Q20-A04 重新生成了 Q21 依赖的 D19-R03 Prefab，因此重新绑定 Q21 当前 hash，而不是复用旧证据。`R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored`=`1/1 PASS`，marker 保持 `4 buildings / 4 footprints / 6 required groups / 23 colliders / 1 terrain collider / 22 D27 owner colliders`；四栋服务建筑各 `37 Renderer / 0 missing`，pairwise XZ overlap=`0`，稳定 Spawn/Disembark 锚点保留。Console 新增 Error=`0`、Warning=`0`，Editor=`ready/stopped`。

这次只完成静态空间/owner 重新验证，不关闭原生 Player 镜头/遮挡、鼠标命中、物理键盘路线、植被准入、性能或最终视觉；Q21 继续 `PARTIAL`，下一步为 Q22。详见 [Q21 R2](../../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)、[Q21 handoff](design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

## 2026-09-14 Q22/Q23 current-source revalidation after Q20-A04

Q20-A04 重新生成了共享 D19-R03 Harbor Presentation Prefab，故 Q22/Q23 的旧 hash-bound 结果不能继续作为当前证据。本轮通过 Unity Pipeline 重新执行当前源码审计：Q22=`1/1 PASS`（测试 `0.1779873s`，Pipeline `0.19s`），Q23=`1/1 PASS`（测试 `0.3673445s`，Pipeline `0.38s`）。

- Q22 marker seq `48240`：`96×96` 水面、sea `-0.0397`、beach min.y `-0.0048`、foam `28`、缺失水材质 `0`、船壳底部 `-0.0400`，天气/鱼类指标保持合同值。
- Q23 marker seq `48255`：活动根 `34`、R03/船实例 `1/1`、R03 Renderer `362`、缺失材质/脚本 `0/0`、地形碰撞体 `1`、GUID 有效、重开一致 `true`。
- Console seq `48201→48263` 新增 Error=`0`、Warning=`0`，Editor=`ready/stopped`。两卡均继续 `PARTIAL`，不关闭真实 Player 镜头、输入、动画、游泳、船控、音效、性能、植被全镜头接地或最终视觉边界。

结果：[Q22 R2](../../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)、[Q23 R2](../../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；交接：[Q22](design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)、[Q23](design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。下一步：`Q24_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-14 Q24 current-source revalidation after Q20-A04

通过连接的 Unity Pipeline 重新执行 `D19Q24SaveRestoreDiagnosticTests`，结果 `3/3 PASS`，Pipeline 总耗时 `1.33s`。失败保存保护、陆地 port/clock/money/cargo/crew round-trip、出海船位/航向/速度恢复与 `D2_PlayerSpawnAnchor` 安全出生点均有当前证据；Console seq `48263→48280` 新增 Error=`0`、Warning=`0`，Editor=`ready/stopped`。

Q24 仍为 `PARTIAL`：`PlayerState` 没有独立游泳状态/姿态/动画存档字段，本轮不宣称游泳持久化或原生 Player 通过；输入、动作、游泳、登船控制、树木接地、音效、性能和最终视觉仍属于后续边界。

结果：[Q24 R2](../../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；交接：[Q24](design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。下一步：`Q25_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-14 Q25 current-source revalidation after Q20-A04

Q25 当前源码两段路线均通过：EditMode `D19Q25FirstVoyageLoopDiagnosticTests=2/2 PASS`（Pipeline `0.50s`），PlayMode `D19R02A03RoutePlayModeTests=2/2 PASS`（Pipeline `8.03s`）。D23 marker 为 `40` tick、arrived/returned/save restored；D30 marker 为 `preparation>market>tavern>shipyard>boarded>departed>returned`，失败与重复均稳定。

本轮 Console seq `48280→48435` 返回 `155` 条，Error=`0`、Warning=`12`；12 条为已登记的 D27 readiness、D2 未验证 rig/程序化 fallback、absolute Euler 动画家族，另有 D31 runtime performance pending 与 D7 audio fallback 日志。Q25 继续 `PARTIAL`，不关闭人工 Player 输入、镜头、动作/游泳/上船、植被接地、音效、性能或最终视觉边界。

结果：[Q25 R2](../../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；交接：[Q25](design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一步：`Q26_CURRENT_PIPELINE_REAUDIT`，建立真实帧/性能测量边界。

## 2026-09-14 Q26 current-source revalidation after Q20-A04

当前 Q26 PlayMode 套件 `D19R03Q26GameplayDefectPlayModeTests`=`19/19 PASS`，Pipeline `53.18s`。A03 确认旧树 Renderer=`0`、R03 vegetation=`24`、unsupported=`0`；A04 远端 E 拒绝/真实登船下船通过；A06 水中连续位移 `0.6119`；A17 舵轮手位误差 `0.0044`；A21 程序化泳姿测量 `pitch=64.0 / maxArmDelta=7.09 / maxFootDelta=2.83`，正式资源仍未验证。

Console seq `48435→49572` Error=`0`、Warning=`114`、无丢弃；Warning 家族是 D27 readiness、D2 procedural/未验证蒙皮/absolute Euler。D31 runtime performance 仍 pending，D7 audio fallback 仍 true。Q26 继续 `PARTIAL`，不把代码级合同通过解释成前台键盘、正式 Avatar/动作、自然泳姿、植被穿模视觉、音效、目标设备性能或 VC-G01 视觉通过。

结果：[Q26 R2](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；交接：[Q26](design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一步：`Q27_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-14 Q27 current-source revalidation after Q20-A04

Q27 重新执行 D16 budget 和 60 秒 headless runtime probe：EditMode `6/6 PASS`（Pipeline `0.02s`），PlayMode `1/1 PASS`（Pipeline `63.42s`）。Null Device 报告窗口 `60.0137634s`、`163` samples、平均 `28.147253 FPS`、平均帧 `164.826813ms`、最长 `1226.161255ms`、>50ms 长帧 `85`、平均主线程 `18.020201ms`、GPU unavailable；PerformanceProbe owner 与 BudgetSatisfied 均真实为 true。

Console seq `49572→49642` Error=`0`、Warning=`6`，D31 performance pending 与 D7 audio fallback 保留。Q27 继续 `PARTIAL`，不把 Null Device 数字升级为目标设备性能或流畅度通过；下一步：`Q28_CURRENT_PIPELINE_REAUDIT`，生成当前源码候选 Player build。

结果：[Q27 R2](../../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；交接：[Q27](design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-14 Q28 current-source R11 candidate build and D17 guard

本轮不复用带 API timeout 边界的 R10，使用 Q20-A04 之后当前源码生成新的 `build_b36e843e5abd` / R11。dry-run=`valid=true / validationErrors=[]`；`StandaloneWindows64` 非开发版 BuildReport 为 `Succeeded`、总构建数据 `175212022` bytes、耗时 `32087ms`、`0 errors / 1 warning`。唯一 warning 为 `RuntimePipelineConfig enableInBuilds=false`，所以 R11 不带 Player Runtime Pipeline 控制面。

- 结果：[Q28 R11](../../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2_RESULT.json)；日志：[Q28 R11 capture](../../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R2.log)；交接：[Q28](design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。
- 新产物：[Q28-CANDIDATE-R11](../../game/tools/results/LOCAL-QUALITY/Q28/Q28_CANDIDATE_PLAYER_R11.exe)，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`；Assembly-CSharp SHA256=`FF699A91A9EE7F9E5B78F984C38B6442371EB82B06F8ABA6E12CA304A91738DB`。
- 当前 D17 A08=`1/1 PASS`，Pipeline `13.99s`、测试 `13.3473876s`；10 周期 `rootAngle=64.00°`、`rootOffsetY=0.014..0.034`、手/头对齐=`0.929..0.990`。这是视觉根安全守卫，不是正式动作/Avatar 或最终画面通过。
- D17 后增量 Console `Error=0 / Warning=6 / Log=112`，`dropped=false`；Editor=`ready/stopped`，Runtime Pipeline 回读 `enableInBuilds=false / port=0`。本轮没有启动前台 Player、发送物理键鼠、采集 compositor 截图或测量 GPU。

Q28 父任务继续 `PARTIAL`；R11 只关闭当前候选构建的 BuildReport timeout 边界。下一步保持依赖顺序进入 `Q29_CURRENT_PLAYER_ROUTE_REAUDIT`，并明确区分隔离 Player 输入/状态证据与 VC-G01 所需的前台视觉证据。

## 2026-09-14 Q26 shoreline hysteresis regression R3

针对 Q29 R8 隐藏 Player 岸线探针观察到的 `8/8/8` 游泳进入/退出/落地恢复循环，本轮在 D2 所有者内加入最小 `0.35s` 岸线重入滞回，并新增 Q26 A22 回归。`D19R03Q26GameplayDefectPlayModeTests` 当前为 `20/20 PASS`；A22 marker=`immediate_reentry=False, reentered_after_cooldown=True`。Q26 仍为 `PARTIAL`，因为这只修复并证明状态边界，不等价于自然泳姿、正式 Avatar、前台画面、植被接地或最终视觉通过。

结果：[Q26 R3](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q26 R3 log](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q26](design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-14 Q29 current-source Pipeline R9

使用 Q26 R3 之后当前源码重新构建 `build_bb39e1a3233d` / `Q29_RUNTIME_PIPELINE_QA_R9.exe`，临时开启 Pipeline `127.0.0.1:7905` 和 Development Player，完成后恢复并回读为 runtime disabled、non-development、Editor ready/stopped。构建 `Succeeded`，`0 errors / 7 warnings`，`220891074` bytes；EXE SHA256=`98751D0DFF00DD3ADE563C2B505A0E9F7A46E8E8895864212E1B884EDBCB42E81`；Assembly-CSharp SHA256=`C21752E45595A58E7E9E21C44D3C833AD41ECAA8A3468915955C52EA8133ED2B`。

- 通过隐藏 Windows Player Runtime Pipeline 完成深水自然入水、真实 W 游泳位移、岸线真实 A 退出和退出后的稳定状态；R9 同类路线 `swimming_enter=3 / swimming_exit=2 / ground_recover=3`，未复现 R8 的立即振荡。
- Pipeline console error=`0`，Player log 严格错误词=`0`，PID `2452` clean close，descriptor 删除。插件命令面没有 screenshot，因此截图数为 `0`。
- 本轮入口使用只读 `SceneManager.LoadScene(InitialHarbor)` 与确定性起点作为 QA setup，不声称 MainMenu；没有前台物理键盘、compositor 截图、正式动作/Avatar、自然 IK、全镜头树木接地或目标设备性能证据。

Q29 子范围为 `PASS_WITHIN_SCOPE`，Q29 父任务继续 `PARTIAL`；VC-G01 继续 `IN_PROGRESS`，VC-G02/VC-G03 继续 `LOCKED`，D31-P04/S05/G06 继续 `PARTIAL`。结果：[Q29 R9](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R9_RESULT.json)；日志：[Q29 R9 capture](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R9.log)；交接：[Q29](design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。下一步：Q30 独立证据审查，之后 Q31 最终 native/visual gate。

## 2026-09-14 Q30 current-source independent review R3

本轮没有修改 Unity 生产源、Prefab、场景或 Runtime Pipeline 设置，只同步了一个静态测试契约：当前 R03 `AddCandidateDetails` 已将 `tree_palm.fbx` 作为 `Candidate/ImportVerified` 候选接入，同时 detached tree/grass 保持 audit-only；旧测试仍要求已删除的 `deferred from the visible prefab` 文案。初始定向 Python 为 `64/65`，修正后为 `65/65 PASS`。

- Connected Unity Pipeline 最终：D16 Accessibility=`8/8`，D16 Input Router=`11/11`，D17 完整复跑=`16/16`，VcG01WorldLogic=`14/14`，Q07/Q15/Q16/Q17 各=`1/1`。
- D17 首次完整运行曾为 `15/16`，A08 第 10 周期左上臂 idle 基线误差 `82.6231079°`；聚焦 A08=`1/1`、完整复跑=`16/16`。因此将其保留为间歇稳定性风险，不视为无风险通过。
- Editor Error=`0`；Warning=`141`、`dropped=false`，属于已知 D2/D27 fallback/readiness/animation 和 Q17 预期异常路径；Console 已清空，Editor=`ready/stopped`。

Q30 继续 `PARTIAL`；Q29 R9 隐藏 Pipeline 依赖仍为 `PASS_WITHIN_SCOPE`，但没有截图命令、前台物理输入、正式动作/Avatar、自然泳姿/IK、植被全镜头接地或目标设备性能证据。VC-G01=`IN_PROGRESS`，VC-G02/VC-G03=`LOCKED`，D31-P04/S05/G06=`PARTIAL`。结果：[Q30 R3](../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q30 R3 capture](../../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q30](design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。下一步：Q31 最终 native/visual gate。

## 2026-09-14 Q31 R5 hidden Player backbuffer capture boundary

本轮继续使用已连接的 Unity Pipeline `com.unity.pipeline@0.6.0-exp.1`，以当前源码构建的 `Q29_RUNTIME_PIPELINE_QA_R9.exe` 启动隐藏 Windows Player，并调用现有 `D31VisualEvidenceProbe`：`-d31-visual-evidence -d31-visual-evidence-run=Q31_R5`。本轮没有修改生产代码、测试代码、场景、Prefab 或运行时配置。

- Player 使用真实 RTX 4070 Ti / Direct3D11 启动，最终场景为 `InitialHarbor`；探针请求固定 14 张、天气 3 张，共 17 张 `1280x720` PNG，文件全部写出，天气序列也完整执行，Player clean exit，`Player.log` 严格 `ERROR|EXCEPTION|CRASH|FATAL=0`。
- 直接用 `view_image` 检查 `capture_02_d31_fixed_spawn.png`，得到完全黑色帧；17 个 PNG 都是相同的 `12152` bytes、SHA256=`5CAAF573FFC294827DF39B5DE5BF9F433AC2D1F443C29FF696D19FD12D36C47B`。因此这些文件不能用于判断港口、游泳、舵轮、HUD、树木接地、穿模或任何视觉缺陷。
- 结论是隐藏窗口 `ScreenCapture` backbuffer 没有暴露可用视觉内容，这是截图/合成器边界，不是“画面正确”或“画面错误”的证据。不能用 Player 启动成功、文件存在或无错误日志替代视觉 PASS。

结果：[Q31 R5 Result](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_BACKBUFFER_REAUDIT_20260914_R5_RESULT.json)；日志：[Q31 R5 capture](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_BACKBUFFER_REAUDIT_20260914_R5.log)；原始 Player 结果：[D31 Q31 R5](../../game/tools/results/LOCAL-QUALITY/Q29/D31_P04_VISUAL_EVIDENCE_PLAYER_Q31_R5.json)。

门禁保持：`Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL`、`Q03=BLOCKED_ASSET`、`Q20=PARTIAL/BLOCKED_ASSET`。本轮没有证明前台 compositor/Win32 物理输入、正式动作/Avatar、自然游泳/IK/握舵、植被全镜头接地、目标设备性能或人工视觉验收。

## 2026-09-14 VC-G01 hidden Player W/S 与主 HUD R5

当前源码 Q29 R9 隐藏 Windows Player 通过 Runtime Pipeline 复核了用户报告的 W/S 菜单冲突：InitialHarbor QA 入口使用只读直接场景加载；初始 `navigation=false / move=null / selected=<none>`，W 按住 `500ms` 后玩家从 `(0.000,0.150,-2.700)` 到 `(0.000,0.125,-4.315)`，位移 `(0.000,-0.025,-1.615)`，选中项仍为 `<none>`，W 释放成功。Runtime Error Console=`0`，Player clean close，descriptor 删除。

这只证明当前源码隐藏 Player 控制面中 W/S 与主 HUD 的 owner 隔离，不证明前台物理 Win32 键盘、真实 MainMenu 用户路径、compositor-visible UI、正式动作/Avatar、自然泳姿/握舵、植被接地、目标设备性能或最终视觉验收。一次直接 InputSystem namespace eval 失败已作为工具边界保留，reflection-only 复核成功。`VC-G01=IN_PROGRESS`、`Q31=PARTIAL`、`VC-G02/VC-G03=LOCKED` 不变。结果：[VC-G01 Player input R5](../tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_PLAYER_REAUDIT_20260914_R5_RESULT.json)；日志：[VC-G01 Player input R5 log](../tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_PLAYER_REAUDIT_20260914_R5.log)。

## 2026-09-14 VC-G01 W/S 与主 HUD 输入归属 Editor Pipeline R4

## 2026-09-14 VC-G01 full hidden Player interaction route R6

利用已连接的 Unity Pipeline `com.unity.pipeline@0.6.0-exp.1`，对当前源码 Q29 R9 Windows Player 完成了完整的隐藏运行交互路线：MainMenu New Game/Confirm、W 移动、I 仓库及模态 W 阻断、M 市场重复/Esc、T 酒馆重复/Esc、E 登船/下船/再次登船、Esc 暂停/恢复。MainMenu 最终进入 `InitialHarbor`；W 将玩家从 `(0.00,0.15,-2.70)` 移动到 `(0.03,0.12,-4.35)`；模态仓库期间位置保持 `(0.00,0.17,-0.70)`。

运行时 error console=`0`、Player clean close、descriptor 删除；Player.log `181468` bytes，严格错误词=`0`，Editor=`ready/stopped`，Runtime Pipeline 已恢复 `enableInBuilds=false / port=0`。仓库关闭的两条相同日志已检查为同一状态的一次关闭转换（`Show` 与显式 `Debug.Log` 重复写日志），不是重复打开/关闭业务动作。

本卡仅将 VC-G01 hidden Player 的交互控制面证据从 W/S 子范围扩展到完整 I/M/T/E/Esc 路线，结果为 `PASS_WITHIN_SCOPE`；不会升级 `VC-G01=IN_PROGRESS`、`Q31=PARTIAL`。前台物理键盘、compositor-visible 截图、正式动作/Avatar、自然泳姿/握舵 IK、植被接地/穿模、目标设备性能和人工最终视觉验收仍是后续门禁。

结果：[VC-G01 R6](../tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INTERACTION_PLAYER_REAUDIT_20260914_R6_RESULT.json)；日志：[VC-G01 R6 log](../tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INTERACTION_PLAYER_REAUDIT_20260914_R6.log)；交接：[VC-G01](design/common/handoffs/VISUAL-CLEAN/VC-G01_HANDOFF.md)。

针对用户此前观察到的 W/S 与右上角主 HUD 切换冲突，连接 Unity Pipeline 在 `InitialHarbor` PlayMode 运行 `D16PrimaryHudNavigationPlayModeTests`，结果 `2/2 PASS`：W/S 保留 Gameplay 移动轴且不改变主 HUD 选中项；人为恢复 UI/Navigate move action 后，`HarborInteractionStabilizer` 重新移除它。Editor=`ready/stopped`，Error Console=`0`，`dropped=false`，本轮无生产/测试/配置变更。

这只关闭当前源码的 Editor PlayMode 输入 owner 子合同，不等于前台 Win32 物理键盘、同源 Player 构建、compositor-visible UI、最终视觉或性能通过。`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`Q31=PARTIAL` 保持。若新构建仍复现冲突，需要登记实际构建 hash 与复现路线再做 Player 级复核。结果：[VC-G01 input R4](../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_REAUDIT_20260914_R4_RESULT.json)；日志：[VC-G01 input R4 log](../game/tools/results/VISUAL-CLEAN/VC-G01/VC_G01_INPUT_ROUTER_REAUDIT_20260914_R4.log)。

## 2026-09-14 D31-S05 current-source Pipeline R75

D31-S05 当前源码回归通过，但正式门禁保持 `PARTIAL`。Python 使用项目现有 unittest 入口得到 `60/60 PASS`；`python -m pytest` 因当前环境没有 pytest 模块而不可用，已记录为工具环境边界。Unity Pipeline 依次通过 `D31VisualStagePolicyTests=30/30`、`D31HudLayoutPolicyTests=3/3`、`D31Q18HudResolutionTests=3/3`、`D29TradeBridgePlayModeTests=1/1`、`D29ShipyardPreparationPlayModeTests=1/1`。

Editor 最终 `ready/stopped`，Runtime Pipeline `enableInBuilds=false / port=0`，无 Player 进程、无 7905 监听。本轮没有修改生产/测试/场景/Prefab/运行时配置，因此这是当前源码合同回归证据，不是新的视觉修复或 Player 视觉通过。历史 Console 中保留两个工具层错误（无效命令 `status`、无效命令 `get_runtime_settings`）；正确命令为 `get_runtime_pipeline_settings`，所以本轮不宣称 Editor Error Console=0。

D31-S05、D31-G06 继续 `PARTIAL`；VC-G01 继续 `IN_PROGRESS`，Q31 继续 `PARTIAL`。下一步仍按依赖进入 D31-G06 当前门禁复核，只有拿到前台物理输入/compositor 截图、完整 Player 负向/禁用/焦点/重复矩阵、D30 Player 异常恢复、正式 Avatar/动作/自然泳姿/IK、最终画质和目标设备性能证据后，才允许升级状态。

结果：[D31-S05 R75](../../game/tools/results/D31/D31-S05/D31_S05_CURRENT_PIPELINE_REAUDIT_20260914_R75_RESULT.json)；日志：[R75 log](../../game/tools/results/D31/D31-S05/D31_S05_CURRENT_PIPELINE_REAUDIT_20260914_R75.log)；交接：[D31-S05](design/common/handoffs/D31/D31-S05_HANDOFF.md)。

## 2026-09-14 接管续行：Q00 当前源码重绑定与 Q31 W/S 游戏态 HUD 焦点隔离

本轮先修正证据链：旧 Q00 current-reaudit 仍引用 Q26-A22 之前的 D2 runtime SHA256=`0A0A17...` 和 Q31 R4 build，不能代表最后一次游泳边界修复后的源码。当前绑定已更新为 D2 runtime `3D854D172925703EE556FC0FD2AF7E384B19B5AEE32246178625C1A9A32F2B4A`、Q26-A22 test `B604A367A1DE24D3C02251B8E3FB4D3E707CB5C099842A0274F1870BDE9F7F72`，并确认 R1 与 Q29 R9 的 EXE/Assembly-CSharp hash 一致。

针对用户报告的 W/S 与右上角 HUD 切换，新增 `D29Q15GameplayWasdDoesNotNavigatePrimaryHud`。Editor PlayMode 新测试、原 D29 面板互斥恢复、D16 输入路由回归分别为 `1/1 PASS`。当前源码 R1 hidden Windows Player 通过 Pipeline MainMenu 指针进入 `InitialHarbor`，W/S 都产生角色位移；两次期间 `EventSystem.sendNavigationEvents=false`、`InputSystemUIInputModule.move=null`、`currentSelectedGameObject=<none>` 保持不变。Runtime error entries=`0`，Player clean close，Editor=`ready/stopped`，Runtime Pipeline 已恢复 disabled/port 0。

这张卡只证明逻辑 owner/控制面边界，不证明前台 Win32 物理键盘、compositor-visible 画面或人工视觉验收；因此 Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`，Q03 `BLOCKED_ASSET`，Q20/Q26/D31 视觉/性能依赖保持。下一步继续按依赖处理正式 Avatar/action、自然 swim/helm、植被接地/穿模和最后的前台 Player gate，不能把这次 hidden Player 结果升级为整体完成。

结果：[Q00 source rebind](../../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_SOURCE_REBIND_Q29R9_Q26A22_20260914_R1_RESULT.json)；[Q31 UI focus R1](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1_RESULT.json)；日志：[Q31 R1 log](../../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_PIPELINE_UI_FOCUS_REAUDIT_20260914_R1.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-14 Q26-A23 当前游泳手部轮廓候选

本轮把前一轮 Player 运行时测得的“手高于头部”从视觉描述转成 D2 表现层世界坐标合同：保持上臂 `150f`，将程序化前臂合成角调整为 `60f`，新增 Q26/A23 在 36 帧内约束 `maxHandAboveHead<=0.22m`。上臂 `175f`、`170f` 两个候选均因 D17/A08 的既有 `<175f` 上臂包络失败而回滚，避免用截图猜测继续扩大扭曲风险。

当前证据：Q26/A23、D17/A08、Q26/A21 各 `1/1 PASS`；完整 Q26=`21/21 PASS`。Current-source QA Player `build_b0549b8e9093` 运行到 InitialHarbor 后，24 次手—头采样最大 `0.135m`，状态为 `Swimming/Swimming/Procedural`；重新启用输入后 W 500ms 产生真实位移，Player error console=`0`，clean close。构建有 7 条既有弃用 warning，未隐藏。临时 Runtime Pipeline 与 Development 设置已恢复默认关闭，Editor=`ready/stopped`。

本卡仅收口程序化 fallback 手部轮廓和隐藏 Player 状态/输入子范围，不提升 Q05 既有范围以外的自然泳姿结论；Q03 仍 `BLOCKED_ASSET`，Q26/Q31 仍 `PARTIAL`，VC-G01 仍 `IN_PROGRESS`，VC-G02/VC-G03 仍 `LOCKED`。正式 Avatar/action、自然 IK/握舵、前台 Win32 输入、compositor-visible 截图、植被/穿模全镜头、目标设备性能仍按依赖后置。

结果：[Q26-A23 JSON](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_A23_SWIM_FOREARM_PIPELINE_20260914_LUNA_RESULT.json)；[Q26-A23 log](../../game/tools/results/LOCAL-QUALITY/Q26/Q26_A23_SWIM_FOREARM_PIPELINE_20260914_LUNA.log)；交接：[Q26](design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-14 Q20-A05 exact-collider candidate grounding

本轮针对 R03 表现构建器中候选物件仍用解析 `Height()` 放置、而真实 `D19_TerrainRoot` 为变形 MeshCollider 的偏差风险，做了最小 owner 修复：`AddCandidate` 与 `AddCandidatePreserveMaterials` 改为 `GroundHeight()`，优先对构建中的实际 MeshCollider 向下 Raycast，碰撞体不可用时才回退到 `Height()`。通过 Unity Pipeline 菜单重建 Prefab；首次 30 秒请求超时仅记为工具层边界，120 秒重试成功。

验证：候选 Palm 接地 `1/1 PASS`；Q26/A12 港口候选接地 `1/1 PASS`；Q26/A14 岸线岩石接地 `1/1 PASS`；完整 Q26=`21/21 PASS`，失败/跳过/不确定均为 `0`。Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`；Editor=`ready/stopped`；Runtime Pipeline=`enableInBuilds=false/port=0`。

这张卡只关闭候选/表现层的精确碰撞体接地兼容性，不恢复正式棕榈资源，不证明全镜头穿模/遮挡、前台物理输入、compositor-visible Player 视觉或目标设备性能。Q20=`PARTIAL/BLOCKED_ASSET`、Q26/Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。权威结果：[Q20-A05](../game/tools/results/LOCAL-QUALITY/Q20/Q20_A05_CANDIDATE_GROUND_COLLIDER_REAUDIT_20260914_LUNA_RESULT.json)；日志：[Q20-A05 log](../game/tools/results/LOCAL-QUALITY/Q20/Q20_A05_CANDIDATE_GROUND_COLLIDER_REAUDIT_20260914_LUNA.log)；下一步为 Q21 当前源码港口空间复核。

## 2026-09-14 Q21 current-source revalidation after Q20-A05

Q20-A05 重建共享 R03 Prefab 后，重新执行 Q21 空间审计，避免沿用旧 Prefab hash。`R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored` 为 `1/1 PASS`，测试耗时 `0.472424s`、Pipeline `0.49s`；marker 确认 `4` 个服务建筑、`4` 个 footprint、`6` 个 required groups、`23` 个碰撞体（R03 terrain=`1`、外部 D27 owner=`22`）、pairwise XZ overlap=`0`，四建筑各 `37 Renderer / 0 missing`。

当前 Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`，builder/test/scene hash 及 marker 已写入 Q21 R3 结果。Editor=`ready/stopped`，Q21 新 Console marker seq=`59091..59097`，返回 tail 中无新的 Q21 Error；Q20-A05 的历史工具层 timeout seq=`57632` 单独保留。

Q21 继续 `PARTIAL`：静态空间/owner 合同通过不等于 Player 镜头、遮挡、鼠标/前台物理键盘路线、植被准入、性能或最终视觉通过。结果：[Q21 R3](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q21 R3 log](../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；下一步为 Q22 当前源码船海天气复核。

## 2026-09-14 Q22 current-source revalidation after Q20-A05

Q20-A05 重建共享 R03 Prefab 后，重新执行 Q22 海/船/天气审计：`R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts`=`1/1 PASS`，测试 `0.1744739s`、Pipeline `0.18s`。marker seq=`59106` 确认 water=`96×96`、sea=`-0.0397`、beach min.y=`-0.0048`、foam=`28`、water missing materials=`0`、ship min.y=`0.0600`、hull bottom=`-0.0400`、presentation audio=`0`、fish=`1.00/0.90/0.55`、precipitation=`0/560/980`、wave multiplier=`1.00/1.40/2.35`；weather preview/reset seq=`59102..59105`。

当前 Harbor Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`，Ship Prefab SHA256=`CCCCA72A4C359DA4A45CCC62F114740A92800C1D65A38B07549BA34F3591B1FC`，Editor=`ready/stopped`。Console tail=`40` 无 Error，测试收集器重复 RunStarted/RunFinished 仅为工具日志，已写入结果。

Q22 继续 `PARTIAL`：静态海线/船体/天气合同通过不等于 Player 游泳、船海镜头、音效、物理键鼠、性能或最终视觉通过。结果：[Q22 R3](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q22 R3 log](../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；下一步为 Q23 当前源码正式资源集成复核。

## 2026-09-14 Q23 current-source formal scene integration after Q20-A05

Q20-A05 重建共享 R03 Prefab 后，重新执行正式场景集成/重开测试：`InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently`=`1/1 PASS`，测试 `0.7007015s`、Pipeline `0.71s`。marker seq=`59115`：active roots=`34`、R03/船实例=`1/1`、活动 R03 Renderer=`362`、缺失材质/脚本=`0/0`、R03 terrain collider=`1`、ship hull bottom=`-0.0400`、GUID 全部有效、`reopen_consistency=true`。

Renderer `362` 是当前 Prefab 的真实 marker，旧证据中的 `361/369` 保留为历史状态，不相互覆盖。当前 Harbor Prefab SHA256=`F28686F12236FA9F9FA5DD087E0B4EF4959CAD57C31A52FC241056D806FAE163`，Ship Prefab SHA256=`CCCCA72A4C359DA4A45CCC62F114740A92800C1D65A38B07549BA34F3591B1FC`；Editor=`ready/stopped`；Console tail 无 Error，重复 test collector 日志已登记。

Q23 继续 `PARTIAL`：正式场景集成/重开通过不等于 Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过。结果：[Q23 R3](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q23 R3 log](../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；下一步为 Q24 当前源码存档恢复诊断。

## 2026-09-14 Q24 current-source save/restore revalidation after Q20-A05

Q20-A05 重建共享 R03 Prefab 后，重新执行 `D19Q24SaveRestoreDiagnosticTests`，结果 `3/3 PASS`（Pipeline `1.47s`）：坏档保护、陆地 port/clock/money/cargo/crew round-trip、出海船位/航向/速度与 `D2_PlayerSpawnAnchor` 安全恢复均通过。当前 source hash、Editor `ready/stopped` 与 Console tail `Error=0` 已登记。

Q24 继续 `PARTIAL`：`PlayerState` 没有独立游泳姿态/体积/动画存档字段，本轮只证明 Save owner 的陆地/出海状态和上一份好档保护，不证明原生 Player 输入、游泳/动作/上船控制、树木接地、音效、性能或最终视觉。结果：[Q24 R3](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q24 R3 log](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q24](design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。下一步为 Q25 当前源码首航/循环诊断。

## 2026-09-14 Q25 current-source first-voyage/loop revalidation after Q20-A05

Q25 两段当前源码路线均通过：`D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.42s`），`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `7.26s`）。marker 确认 D30 七阶段投影、D23 `40` tick 首航/抵达/返港/存档投影，以及有限 InitialHarbor 交易关闭/保存和出航/推进/返港。

本轮 Console seq `59130→59278` 共 `148` 条，Error=`0`、Warning=`12`；Warning 为已知 D27 readiness、D2 未验证蒙皮/程序化 fallback、绝对 Euler 动画曲线，另有 D31 性能 pending 与 D7 音频 fallback。Q25 继续 `PARTIAL`：owner 路线通过不等于完整人工游玩、原生 Player 输入、动作/游泳/上船视觉、树木接地、音效、性能或最终视觉通过。结果：[Q25 R3](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q25 R3 log](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q25](design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一步为 Q26 当前源码复核。
## 2026-09-14 Q31 R5 safe pause point

Q31 R5 已完成一个有限但真实可见的 native Player UI 修复闭环：`MainMenuController` 将 New Game 标题从 `y=150` 调整到 `y=210`，接受构建 `build_fc9d95893976` 为 `0 errors / 0 warnings`，可见 `1280x720` Player 截图确认标题与船长名占位文本不再重叠。随后同一 Player 进入 `InitialHarbor` 完成 W、E 登船/下船和 W/S 路径，runtime error console=`0`、Player 严格错误词=`0`、clean close，临时 Runtime Pipeline 与 Development Build 配置已恢复。

该结果只关闭上述 UI 缺陷范围。港口/登船画面仍为低模/程序化表现，本轮 W/S 未视觉证明进入开放水域；正式动作/Avatar、自然泳姿/握舵、植被接地/穿模、前台物理输入、目标设备性能及最终人工视觉仍开放。开发在 `Q31=PARTIAL`、`VC-G01=IN_PROGRESS`、`VC-G02/VC-G03=LOCKED`、`D31-P04/S05/G06=PARTIAL` 的安全点临时暂停。恢复入口：[Q31 R5 result](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_UI_FIX_AND_VISUAL_GATE_20260914_R5_RESULT.json)；[暂停交接](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260914_Q31_R5.md)。

## 2026-09-14 Q29 current-source Pipeline R10 full hidden Player interaction route

通过 Unity Pipeline 临时启用 Player Runtime Pipeline 后，使用当前源码构建 `build_b5958f0eeca0`：`StandaloneWindows64 / Succeeded / 0 errors / 0 warnings / 220891074 bytes / 66197ms`。同一候选 Player 隐藏启动，完成 MainMenu NewGame/Confirm 指针入口以及 InitialHarbor 的真实运行时控制面探针。

自然入口证据覆盖 W 移动、I 仓库打开与模态内 W 阻断、M 市场重复打开与 Esc 关闭、T 酒馆打开/重复操作/Esc、E 登船/下船/再次登船、Y 出航、航行中 M 拒绝、船上 W/A/S/D、Esc 暂停/恢复。T/E/Y/航行拒绝/船控使用自然 NewGame→Confirm 路线；早期 I/M 和 owner 未就绪 T 另记为确定性诊断边界，不与完整路线混算。

运行结束 `console --level error` 返回 `0` 条；Player 正常退出，descriptor 清除，Player.log 严格 `ERROR|EXCEPTION|CRASH|FATAL` 计数为 `0`。随后已通过 Pipeline 恢复 Runtime Pipeline `enableInBuilds=false / port=0`、development build=false。Q29 子任务在此范围内 `PASS_WITHIN_SCOPE`，但父任务仍 `PARTIAL`：尚未证明前台 compositor 画面、Win32 物理输入、正式 Avatar/action/natural swim/helm、全镜头穿模、目标设备性能或最终视觉验收。结果：[Q29 R10](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10_RESULT.json)；日志：[Q29 R10 log](../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R10.log)；交接：[Q29](design/common/handoffs/LOCAL-QUALITY/Q29/Q29.md)。下一步为 Q30 独立复核当前 Q28/Q29 证据与缺陷清单。

## 2026-09-14 Q30 current-source independent review R4 after Q29 R10

Q30 R4 重新执行当前源码定向 Python 65/65、D16 Input Router 11/11、D17 Action Quality 16/16、VcG01WorldLogic 14/14，均无失败；并核对 Q29 R10 隐藏 Player 真实路线、runtime error=0、严格 Player 错误词=0、clean close。D2 航行系统明确 S 为减速/停止而非倒车，本轮不将其误判为输入缺陷。

Q30 仍为 PARTIAL：逻辑/owner 证据在范围内通过，但没有关闭 compositor-visible 前台画面、Win32 物理输入、正式动作/Avatar、自然泳姿/握舵接触、正式植被准入、全镜头接地/穿模、目标设备性能或最终视觉门禁。Editor 最终 ready/stopped，Error Console=0，Warning=157 且 dropped=false，取证后已清空。结果：[Q30 R4](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4_RESULT.json)；日志：[Q30 R4 log](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260914_R4.log)；交接：[Q30](design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。下一步为 Q31 最终 native Player visual gate。

## 2026-09-14 Q26 current-source full PlayMode revalidation after Q20-A05

完整 `D19R03Q26GameplayDefectPlayModeTests` 在当前源码与重建后的 R03 Prefab 上为 `21/21 PASS`，Pipeline `60.75s`。W 游泳、E 拒绝远端/真实登船下船、旧树清零/R03 植被、泡沫、42 个候选接地目标、岸线岩石、舵轮锚点与 Q26-A22 迟滞均有真实 marker；A21/A23 仍明确为程序化 fallback，formal asset boundary=`unverified`。

Console seq=`59278→60535` 共 `1257` 条，Error=`0`、Warning=`126`、Log=`1131`、未丢弃；D27 readiness、D2 未验证蒙皮/程序化 fallback、absolute-Euler 动画、D31 性能 pending、D7 音频 fallback 均保留为质量债务。Q26 继续 `PARTIAL`：这轮只关闭当前代码级 owner/碰撞/注入式 Input System 回归，不证明前台物理键盘、compositor-visible 画面、正式 Avatar/自然动作游泳/握舵、全镜头穿模、音频、目标设备性能或 VC-G01 最终视觉。结果：[Q26 R4](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R4_RESULT.json)；日志：[Q26 R4 log](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R4.log)；交接：[Q26](design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。下一步为 Q27 当前源码性能/视觉质量诊断。

## 2026-09-14 Q27 current-source D16 performance revalidation after Q26 R4

Q27 当前 D16 预算合同 `6/6 PASS`，60 秒 headless 性能窗口 `1/1 PASS`（Pipeline `64.06s`，测试 `63.4356615s`）。报告为 `Null Device / GraphicsDeviceType=Null`，GPU timing unavailable；`162` samples、平均 `27.9046 FPS`、平均帧 `170.0344ms`、最长帧 `1250.8234ms`、超过 `50ms` 长帧 `88`，预算探针 owner/contract 为 true。

这些数字是连接 Editor headless 诊断风险，不是目标设备性能或流畅度 PASS。Console seq=`60535→60605` 新增 `70` 条，Error=`0`、Warning=`6`；D27/D2 fallback、D31 performance pending、D7 audio fallback 均登记。Q27 继续 `PARTIAL`，没有基于 Null Device 做无证据优化。结果：[Q27 R3](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q27 R3 log](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q27](design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。下一步为 Q28 当前源码新候选 Player build_id。

## 2026-09-14 Q28 current-source R12 candidate build after Q26 R4

通过 Unity Pipeline 对 `MainMenu.unity + InitialHarbor.unity` 做 dry-run 与正式构建，得到全新 `build_0cc5cd156fe3`：`StandaloneWindows64 / Succeeded / 0 errors / 1 warning / 175212534 bytes / 33602ms`。随后 D17-A08 视觉根守卫 `1/1 PASS`，10 周期 rootAngle=`64.00°`、rootOffsetY=`0.015..0.034`、手头对齐=`0.993..1.000`。

唯一构建 warning 是 Runtime Pipeline `enableInBuilds=false`，所以 R12 不带 Player Runtime Pipeline；D17 也只是根保护证据。Q28 继续 `PARTIAL`：没有启动 Player、前台物理输入、compositor 截图、GPU/目标设备性能或最终视觉证据。结果：[Q28 R12](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q28 R12 log](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q28](design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。下一步为 Q29 当前源码 Player 路线/控制面审查。

## 2026-09-14 Q25 current-source first-voyage/loop revalidation after Q20-A05

Q25 两段当前源码路线均通过：`D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.42s`），`D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `7.26s`）。marker 确认 D30 七阶段投影、D23 `40` tick 首航/抵达/返港/存档投影，以及有限 InitialHarbor 交易关闭/保存和出航/推进/返港。

本轮 Console seq `59130→59278` 共 `148` 条，Error=`0`、Warning=`12`；Warning 为已知 D27 readiness、D2 未验证蒙皮/程序化 fallback、绝对 Euler 动画曲线，另有 D31 性能 pending 与 D7 音频 fallback。Q25 继续 `PARTIAL`：owner 路线通过不等于完整人工游玩、原生 Player 输入、动作/游泳/上船视觉、树木接地、音效、性能或最终视觉通过。结果：[Q25 R3](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3_RESULT.json)；日志：[Q25 R3 log](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260914_LUNA_R3.log)；交接：[Q25](design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一步为 Q26 当前源码复核。

## 2026-09-14 current-source Q00-Q03 reconciliation after Q03-A23

本轮接管审计保留全部 dirty worktree，不执行 reset、checkout、clean、stash 或大范围覆盖。Q00/Q01/Q02 完成当前源—运行态—测试关系重绑定；Q03 完成当前 Formal Humanoid 资源/路由代码门复核。旧的 Q03 `BLOCKED_ASSET` 节点保留为历史证据，不被删除；当前状态只在新证据范围内更新。

当前任务板：

- 已完成（有界）：Q00 `VERIFIED_WITH_SCOPE`；Q01 `PASS_WITHIN_SCOPE`；Q02 `PASS_WITHIN_SCOPE`；Q03 当前 Formal Humanoid action/avatar/default route `PASS_WITHIN_SCOPE`；Q04/Q05/Q06 已用 Q03-A23 当前源分别完成 `1/1 PASS` 定向回归；D2=`8/8`、D17=`16/16`、Q03Formal=`5/5`、Q03D20Formal=`2/2`，recompile errors=`0`。
- 进行中：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`；需要最终 Player 画面、前台键盘焦点和人工视觉证据。
- 阻塞/未关闭：VC-G02=`LOCKED`、VC-G03=`LOCKED`；Q20=`PARTIAL/BLOCKED_ASSET`；D31-P04/S05/G06=`PARTIAL`。没有把 Pipeline headless 或 hidden Player 结果当成这些门的 PASS。
- 下一步：Q07 当前碰撞路线复核 → 继续 Q08/Q09 等依赖卡；最后回到 Q31/VC-G01 做 compositor-visible/native Player 终验。

当前源 exact Player 为 `build_099a228c638b`，`0 errors / 8 warnings`，EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`。该构建证明 source binding，不证明画面质量、真实 Win32 输入、目标设备性能或完整流程完成。权威结果见 Q00/Q01/Q02/Q03 新增 current result/log 和对应 handoff。

## 2026-09-15 Q08 current-source integration safe pause

在上一节 Q00-Q03 reconciliation 基础上，Q07 与 Q08 已完成当前源码有界复核：

- Q07 当前碰撞路线为 `PASS_WITHIN_SCOPE`：服务建筑 owner、R03 terrain、dock 与 gangplank 双向步行证据已登记在 [Q07 current result](../../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_COLLISION_ROUTE_20260914_LUNA_RESULT.json) 与 [Q07 handoff](design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。
- Q08 当前 Q03-A23 Formal Humanoid 路由为 `PASS_WITHIN_SCOPE`：Q03 Formal EditMode=`5/5`、Q03D20 Formal PlayMode=`2/2`、D17 最终完整回归=`16/16`、recompile errors=`0`、Error Console=`0`。结果：[Q08 current result](../../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_SOURCE_INTEGRATION_Q03A23_20260915_LUNA_RESULT.json)；日志：[Q08 log](../../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_SOURCE_INTEGRATION_Q03A23_20260915_LUNA.log)；交接：[Q08](design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。
- Q08 本轮只对 `CameraFollow` 做了最小表现层修复：游泳进入首帧和明显自动航向变化时安全定位相机，之后保持平滑/手动接管。中间 A15 近阈值失败仍被保留为间歇稳定性风险，不能解释成零风险。
- Q08 旧的 `BLOCKED` 记录继续有效，但仅针对 Q03-A23 以前的 generic-only 路由；新结果是当前源替代节点，不倒写历史。

### 临时暂停点

这里是一个安全收口点：代码级 action/movement/swim/grounding/input-modal 整合已有真实 Pipeline 证据，但最终 compositor-visible Player、前台 Win32 物理输入、人工视觉、全镜头穿模/植被接地、目标设备性能与音频仍未关闭。状态保持：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、Q20=`PARTIAL/BLOCKED_ASSET`、D31-P04/S05/G06=`PARTIAL`。本轮不继续 Q09，也不把 Q08 升级为 D31 或整机完成。

恢复顺序：`Q09_CURRENT_DECK_ANCHOR_REAUDIT` → Q10 及后续依赖卡 → Q31/VC-G01 compositor/native Player 最终门。恢复入口和边界见 [Q08 handoff](design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。

## 2026-09-15 Q00/Q01 current-source rebind and Q09 resume

用户恢复持续目标后，本轮从提交 `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 重新建立当前证据边界；旧 Q08 安全暂停记录保留为历史，不倒写。

- Q00=`VERIFIED_WITH_SCOPE`：当前源 exact Player `build_c0aabd621f0d`，BuildReport=`Succeeded`，`0 errors / 8 warnings`；connected Pipeline runtime facts、open-water state 和 boarding state 已重新绑定到当前 HEAD。
- Q01=`PASS_WITHIN_SCOPE`：动作/游泳/握舵/树木/WASD-HUD 五类报告完成当前 owner 与根因对账；视觉未验证项、自然姿态、接触和前台焦点仍未被误报为 PASS。
- Q09=`PASS_WITHIN_SCOPE`：当前源 D17A11=`1/1 PASS`，船体缩放约 `2.57` 下十次登船—下船无锚点漂移，约 `30m` 远距请求拒绝；下一步 Q10。

当前任务板：已完成有界 Q00/Q01/Q02/Q03/Q04/Q05/Q06/Q07/Q08/Q09；进行中为 Q10→Q30 依赖链和 Q31/VC-G01；阻塞/未关闭为 Q20=`PARTIAL/BLOCKED_ASSET`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。最终 compositor-visible Player、真实前台输入、人工视觉、自然动作/泳姿/握舵、植被接地、目标设备性能和 D31 整体收口仍未完成。

权威证据：[Q00 current result](../../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_SOURCE_REBIND_Q03A23_20260915_LUNA_RESULT.json)、[Q01 current result](../../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_PIPELINE_Q03A23_20260915_LUNA_RESULT.json)、[Q09 current result](../../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)。下一张执行卡：`Q10_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q10 current-source ship-motion relation revalidation

Q10 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 `D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable`：`1/1 PASS`。十个船舶平移/转向快照下，登船玩家相对船根位置/旋转误差均为 `0.000000`，最大船位移 `1.513m`、最大航向变化 `32°`，并在恢复 baseline 后安全下船。Q10 仍只关闭相对 transform/状态生命周期，不关闭 native 输入、镜头、握舵自然度或视觉门。

当前下一张执行卡为 `Q11_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`；Q31/VC-G01 和 D31 视觉/人工门仍按原边界保持未关闭。

## 2026-09-15 Q11 current-source safe-disembark revalidation

Q11 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 `D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround`：`1/1 PASS`。十次合法下船、无地面拒绝和最终 `grounded=True / mode=Character` 均有 runtime marker；预期拒绝 Warning 原样保留。Q11 只关闭合法/无落点状态与恢复子契约，不关闭移动船视觉碰撞、全岸线出口、native 输入、动作质量或相机门。

当前下一张执行卡为 `Q12_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q12 current-source camera revalidation

Q12 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 `D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose`：`1/1 PASS`。十次相机模式/目标切换、真实受控 BoxCollider 遮挡收缩（`5.500m → 3.882m`）和障碍移除后的恢复（`5.500m`）通过，`collisionChecks=109`。Q12 只关闭有界 CameraFollow 行为，不关闭最终构图或人工视觉门。

当前下一张执行卡为 `Q13_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q13 current-source input/UI revalidation

Q13 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成三组输入/UI 回归：D17A15=`1/1 PASS`（W/S 十轮不导航 HUD、暂停让权后恢复），D16InputRouter=`11/11 PASS`（键盘/手柄/重绑定/InitialHarbor 路径），D16PrimaryHudNavigation=`2/2 PASS`（UI Move action 恢复后 suppression 重新确立）。最终 Error=`0`；6 条已知 D2/D27 Warning 保留。Q13 只关闭 Input System 运行时路由合同，不关闭前台 Win32 焦点、渲染 HUD、业务面板或 VC-G01。

当前下一张执行卡为 `Q14_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q14 current-source shore/water/deck integration revalidation

Q14 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 D17A16=`1/1 PASS`：shore→water→shore、Ship owner、船上转向、无地面拒绝与合法下船 marker 均为 true；`imported_neutral=False` 已明确保留为正式资源边界。Q14 只关闭有界逻辑整合链，不关闭动作资源、原生输入、最终视觉或 VC-G01。

当前下一张执行卡为 `Q15_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q15 current-source panel-focus revalidation

Q15 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 D29Q15=`1/1 PASS`：I/M/T shortcut owner 到达、仓库/市场/酒馆各十轮、互斥、重复请求拒绝和 Gameplay 恢复均有 marker。Q15 只关闭业务面板焦点/快捷键逻辑，不关闭前台 Player 画面、业务真实渲染或 VC-G01。

当前下一张执行卡为 `Q16_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q16 current-source trade-bridge revalidation

Q16 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 D29Q16=`1/1 PASS`：owner 未就绪拒绝、快捷键双击单 owner、买卖成功各一次变化、余额不足和满仓状态不变均有 marker。Q16 只关闭 D3/D29 交易桥逻辑，不关闭 Player 视觉、在线/存档数据或 VC-G01。

当前下一张执行卡为 `Q17_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q17 current-source shipyard/preparation revalidation

Q17 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成 D29Q17=`1/1 PASS`：整备成功/拒绝/重复、船坞成功/拒绝、取消、受控异常恢复、domain 幂等和主输入恢复均有 marker。Q17 只关闭 D29/D9/D4 owner 恢复逻辑，不关闭 Player UI、业务真实渲染或 VC-G01。

当前下一张执行卡为 `Q18_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q18 current-source HUD/layout revalidation

Q18 已完成当前源代码级验证：分辨率矩阵 `3/3 PASS`、正确程序集过滤的 D31 VisualLogic `36/36 PASS`、仓库根 Python HUD 回归 `30/30 OK`。错误的 `testName` 程序集过滤返回 `0 tests`，已排除。Q18 仍为 `PARTIAL`，因为原生 Player/字体栅格/英文长文案/物理鼠标与最终视觉门未关闭。

当前下一张执行卡为 `Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA`。

## 2026-09-15 Q19 current-source crew/save revalidation

Q19 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成当前源复核：`Q19CrewInteractionAuditTests=1/1 PASS`（测试 `0.3604646s`、Pipeline `0.37s`）、`Save.Tests=377/377 PASS`（Pipeline `32.14s`），`dotnet build AgentCore=0 warnings / 0 errors`。证据：[Q19 current result](../../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)；[Q19 log](../../game/tools/results/LOCAL-QUALITY/Q19/Q19_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)；[Q19 handoff](design/common/handoffs/LOCAL-QUALITY/Q19/Q19.md)。

Q19 仍为 `VERIFIED`，只覆盖离线船员提议→owner 裁决、关系事实和 AgentMemory/domain SQLite 保存恢复；在线 LLM、原生 Player 对话 UI/输入/视觉、动作、游泳、碰撞表现、性能和正式视觉门禁仍未证明。测试后 Error=`0`；合并测试的 `27` 条已知 Warning 已记录并清空。下一张执行卡为 `Q20_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 Q20 current-source vegetation revalidation

Q20 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成当前源复核：候选资源 EditMode=`1/1 PASS`（测试 `0.0021969s`、Pipeline `0.01s`），R03 接地/legacy quarantine PlayMode=`1/1 PASS`（测试 `3.0194955s`、Pipeline `3.12s`）。marker 为 `activeLegacyTreeRenderers=0`、`activeR03Vegetation=24`、`vegetation_grounded=26`；Error=`0`，Warning=`6` 后清空。证据：[Q20 current result](../../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)；[Q20 log](../../game/tools/results/LOCAL-QUALITY/Q20/Q20_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)；[Q20 handoff](design/common/handoffs/LOCAL-QUALITY/Q20/Q20.md)。

资源门禁仍为 `BLOCKED / 0 / 0`，`RenderAuthoredPalms=false`，正式 authored palm 引用 `0`，候选引用 `1`；Q20 保持 `PARTIAL/BLOCKED_ASSET`，不把候选接地或旧 Renderer 隔离升级成正式资源/视觉通过。下一张执行卡为 `Q21_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 Q21 current-source harbor-space revalidation

Q21 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成当前源空间审计：`R03HarborPresentation_ServiceFootprints_AreSeparatedGroundedAndRouteAnchored=1/1 PASS`（测试 `0.189254s`、Pipeline `0.20s`）。四个服务建筑各 `37 Renderer / 0 missing`，`23` 个碰撞体中 `1` 个 R03 terrain、`22` 个 D27 owner，pairwise XZ overlap=`0`，Spawn/Disembark 锚点保留；Error=`0`、Warning=`0`。证据：[Q21 current result](../../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)；[Q21 log](../../game/tools/results/LOCAL-QUALITY/Q21/Q21_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)；[Q21 handoff](design/common/handoffs/LOCAL-QUALITY/Q21/Q21.md)。

Q21 仍为 `PARTIAL`：静态空间/owner 合同不等于 native Player 镜头、遮挡、鼠标命中、物理键盘路线、性能或最终视觉通过。下一张执行卡为 `Q22_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 Q22 current-source sea/weather revalidation

Q22 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成当前源审计：`R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts=1/1 PASS`（测试 `0.1758288s`、Pipeline `0.18s`）。marker 覆盖水面 `96×96`、sea level `-0.0397`、泡沫 `28`、船体/船壳吃水、D9 鱼类活动 `1.00/0.90/0.55`、D28 降水 `0/560/980` 与波动倍率 `1/1.40/2.35`；Error=`0`、Warning=`0`。证据：[Q22 current result](../../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)；[Q22 log](../../game/tools/results/LOCAL-QUALITY/Q22/Q22_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)；[Q22 handoff](design/common/handoffs/LOCAL-QUALITY/Q22/Q22.md)。

Q22 仍为 `PARTIAL`：静态海水/船体/天气 owner 合同不等于 native Player 游泳、船海镜头、泡沫/雾/音效播放、物理输入、性能或最终视觉通过。下一张执行卡为 `Q23_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 Q23 current-source formal scene integration revalidation

Q23 已在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63` 上完成当前源审计：`InitialHarbor_R03ResourcesHaveOneActivePrefabOwnedIntegrationAndReopenConsistently=1/1 PASS`（测试 `0.3685381s`、Pipeline `0.38s`）。marker 为活动根 `34`、R03/船实例 `1/1`、R03 Renderer `362`、缺失材质/脚本 `0/0`、R03 terrain collider `1`、船壳底部 `-0.0400`、`reopen_consistency=true`；Error=`0`、Warning=`0`。证据：[Q23 current result](../../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)；[Q23 log](../../game/tools/results/LOCAL-QUALITY/Q23/Q23_CURRENT_PIPELINE_REAUDIT_20260915_LUNA.log)；[Q23 handoff](design/common/handoffs/LOCAL-QUALITY/Q23/Q23.md)。

Q23 仍为 `PARTIAL`：正式场景集成/重开合同不等于 native Player 镜头、键鼠、动作、游泳、船控、音效、性能、逐物体接地或最终视觉通过。下一张执行卡为 `Q24_CURRENT_PIPELINE_REAUDIT`。

## 2026-09-15 临时安全暂停点（Q24）

本轮在当前 HEAD `cee7a9a0581cdaa4aa0a7eee7345694815770d63`、Unity `6000.0.80f1`、Pipeline Editor PID=`63572` 上完成 Q24 当前源存档恢复诊断：`D19Q24SaveRestoreDiagnosticTests`=`3/3 PASS`，Pipeline `1.19s`。失败保存保护、陆地 port/clock/money/cargo/crew round-trip、出海船位/航向/速度与 `D2_PlayerSpawnAnchor` 安全恢复均通过；Error/Warning 分级查询均为 `0`，Editor 最终 `ready/stopped`，取证后 Console 已清空。

Q24 继续为 `PARTIAL`，因为 `PlayerState` 没有独立游泳状态/姿态字段，且上述证据不覆盖原生 Player 前台输入、动作、游泳、船控/镜头、树木接地、音效、性能、完整首航或最终视觉。现在把 Q24 标记为**临时安全暂停点**：没有活动测试进程，Q25 是明确且可独立恢复的下一张卡。暂停不等于产品完成，不提前升级 Q25—Q35、VC-G01、VC-G02、VC-G03 或 D31 状态。

权威交接：[临时安全暂停交接](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915.md)、[Q24 current result](../game/tools/results/LOCAL-QUALITY/Q24/Q24_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)、[Q24 handoff](design/common/handoffs/LOCAL-QUALITY/Q24/Q24.md)。恢复时从 `Q25_CURRENT_PIPELINE_REAUDIT` 开始，不继承旧 Player/RERUN 的视觉或输入结论。

## 2026-09-15 Q25 current-source first-voyage/loop revalidation

Q25 已在当前 HEAD `0d64fbae6a46459bb11e92c8d7c357091dc948c4`、Unity `6000.0.80f1`、Pipeline Editor PID=`63572` 上完成两段当前源回归：D30/D23 `D19Q25FirstVoyageLoopDiagnosticTests` EditMode=`2/2 PASS`（Pipeline `0.50s`），InitialHarbor `D19R02A03RoutePlayModeTests` PlayMode=`2/2 PASS`（Pipeline `5.64s`）。实际 D30/D23 marker 覆盖七阶段投影、失败/重复稳定、40 tick 首航、到达、返港、`d19_facts=172`、`d21_events=163`、保存恢复和重复 owner 检查；两个 PlayMode 用例覆盖系统所有权出航/推进/返港和交易关闭/保存路线。

PlayMode 运行证据同时记录 Error=`0`、Warning=`12`（D27 readiness `6`、D2 未验证 `ph_crew_male` `2`、程序化低模回退 `2`、generated D6CrewAnimator 绝对 Euler `2`），D7 音频 `fallback=True`，D31 runtime performance approval=`pending`；Editor 最终 `ready/stopped`，取证后 Console 已清空。Q25 继续 `PARTIAL`，不把 owner 路线扩大为原生 Player、完整人工游玩、动作/游泳/上船视觉、植被、音效、性能或最终视觉通过。

权威证据：[Q25 current result](../game/tools/results/LOCAL-QUALITY/Q25/Q25_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_RESULT.json)、[Q25 handoff](design/common/handoffs/LOCAL-QUALITY/Q25/Q25.md)。下一张执行卡为 `Q26_CURRENT_PIPELINE_REAUDIT`，先建立当前源码真实帧/性能测量与缺陷基线。

## 2026-09-15 Q26 current-source PlayMode revalidation R5

Q26 全量 `D19R03Q26GameplayDefectPlayModeTests` 在 connected Unity Pipeline Editor（PID=`63572`）使用新重编译程序集完成 `21/21 PASS`，Pipeline=`56.21s`，failed/skipped/inconclusive=`0`。本轮只修改 Q26A22 测试隔离：恢复 `Time.timeScale=1`，按 `Time.time` 等待 D2 再入水迟滞，并登记 timing marker；没有修改产品 D2、Prefab、场景或运行时配置。

此前全量的 `20/21` 观察被保留为诊断证据：结果堆栈仍指向旧测试程序集断言行 `237`，与当前工作文件断言行 `252` 不一致。执行 Pipeline `recompile` 后确认 `completed / failed=false / errors=[]`，新的 `before_exit/after_wait` marker 出现，故本轮才作为权威结果。

Q26A22 实际 marker=`immediate_reentry=False, reentered_after_cooldown=True, position=(0.00,0.40,30.10)`，并确认 `after_wait time=29.952 > deadline=29.833 / timeScale=1.000`。全量 Error=`0`；Warning=`98`（D27 readiness、D2 未验证导入蒙皮/程序化回退/absolute-Euler 动画）已登记，取证后 Console 清空；Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。

Q26 继续 `PARTIAL`：当前有界 gameplay/owner/collision/Input System 注入和 procedural fallback 合同通过，但不证明前台 Win32 物理键盘、compositor-visible Player、正式 Avatar/action、自然泳姿/IK/握舵、全镜头植被/穿模、音频、目标设备性能或 VC-G01 最终视觉。任务板更新为：已完成 Q25/Q26 有界回归；进行中下一张 `Q27_CURRENT_PIPELINE_REAUDIT`；阻塞/未关闭 Q20 `PARTIAL/BLOCKED_ASSET`、Q31 `PARTIAL`、VC-G01 `IN_PROGRESS`、VC-G02/VC-G03 `LOCKED`、D31-P04/S05/G06 `PARTIAL`。结果：[Q26 R5](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5_RESULT.json)；日志：[Q26 R5 log](../game/tools/results/LOCAL-QUALITY/Q26/Q26_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R5.log)；交接：[Q26 handoff](design/common/handoffs/LOCAL-QUALITY/Q26/Q26.md)。

## 2026-09-15 Q27 current-source performance revalidation R4 after Q26 R5

Q27 已在当前 HEAD `00554ed52f137906beaeb88fdd39fbb87a70523a` 重新绑定：`D16PerformanceBudgetTests` EditMode=`6/6 PASS`（Pipeline `0.03s`）；`D16A01RuntimePerformanceWindowSamplesContinuously` PlayMode=`1/1 PASS`（Pipeline `63.69s`，测试 `63.5862239s`）。报告窗口为 `60.2771263s / 162 samples`，平均 `30.9174 FPS`、最小 `0.8399 FPS`、平均帧 `157.7774ms`、最长帧 `1190.5581ms`、超过 `50ms` 长帧 `84`、平均主线程 `16.4134ms`；GraphicsDevice=`Null Device`、GPU timing unavailable。

探针 owner/预算合同真实成立：`PerformanceProbeActive=true`、`PerformanceProbeOwnedByController=true`、host=`D16WorldPerformanceController`、`BudgetSatisfied=true`、`Passed=true`。取证后的 Console 为 `Error=0 / Warning=192 / dropped=false`，warning families=`D27 readiness 96`、`D2 procedural fallback 32`、`D2 unverified ph_crew_male 32`、`D2 absolute-Euler 32`；随后 `clear_console`，Editor=`ready/stopped`。

Q27 仍为 `PARTIAL`：本轮没有修改 D16 生产代码或配置，也没有基于 Null Device 数字做无证据优化；该结果不能关闭 GPU/目标设备性能、长帧风险、原生 Player 视觉/输入或 VC-G01。下一步为 `Q28_CURRENT_PIPELINE_REAUDIT`，建立当前源码新的候选 Player build_id。结果：[Q27 R4](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5_RESULT.json)；日志：[Q27 R4 capture](../game/tools/results/LOCAL-QUALITY/Q27/Q27_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R4_AFTER_Q26_R5.log)；交接：[Q27](design/common/handoffs/LOCAL-QUALITY/Q27/Q27.md)。

## 2026-09-15 Q28 current-source candidate R13 build and D17 guard after Q27 R4

Q28 已在当前 HEAD `762d351ab79bd165efff8d18227f62dfba2e133f` 通过 Unity Pipeline dry-run 和正式构建生成新的 `build_73c3a973e2d8`：`StandaloneWindows64 / Succeeded / 0 errors / 1 warning / 198187532 bytes / 13923ms`。唯一 warning 为 `RuntimePipelineConfig enableInBuilds=false`，R13 不带 Player Runtime Pipeline；EXE SHA256=`D5CB971878002AF0FADCC7CFD86C4368691D7596B3174C99449A14B26896A6BE`，`Assembly-CSharp.dll` SHA256=`45451540097E21BC0E57ED98DDABC0FF3EF1FEC7CC0743A1BAA8AA34F2C8B74E`。

当前源 D17A08=`1/1 PASS`（Pipeline `14.02s`、测试 `13.9233925s`），10 周期 rootAngle=`64.00°`、rootOffsetY=`0.012..0.034`、手/头对齐=`0.992..1.000`；这是根安全守卫，不是正式动作/Avatar 或最终视觉通过。D17 后 Console=`Error 0 / Warning 187 / dropped=false`，包含 Pipeline disabled、D27 readiness 和 D2 procedural/unverified/absolute-Euler fallback；取证后清空，Editor=`ready/stopped`。

Q28 继续 `PARTIAL`：R13 只关闭当前候选包的 BuildReport/身份边界和 D17 root guard，不关闭 Player 前台窗口、物理键盘、输入路线、截图、GPU/目标设备性能、正式动作资源或 VC-G01。下一步为 `Q29_CURRENT_PLAYER_ROUTE_REAUDIT`。结果：[Q28 R13](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4_RESULT.json)；日志：[Q28 R13 log](../game/tools/results/LOCAL-QUALITY/Q28/Q28_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R13_AFTER_Q27_R4.log)；交接：[Q28](design/common/handoffs/LOCAL-QUALITY/Q28/Q28.md)。

## 2026-09-15 Q29 current-source Pipeline R11 entry smoke and temporary safe pause

按用户要求在当前源码建立安全暂停点。本轮没有修改生产代码、测试代码、Prefab、场景或持久运行时配置；只临时打开 Pipeline Player QA 设置构建并运行隐藏 Player，结束后通过 `dry_run + confirm=true` 恢复默认值。

- 当前 HEAD=`3dbde02fe12d2e78d386ce52054303ff1a916e46`；dry-run=`valid=true / validationErrors=[]`；`build_fb5d1ad58690`=`StandaloneWindows64 / Succeeded / 0 errors / 7 warnings / 243870645 bytes / 34076ms`。7 条 warning 为 obsolete `FindObjectOfType`，没有构建错误。
- R11 Player 使用 Runtime Pipeline `127.0.0.1:7907` 隐藏启动，分辨率 `1600x900`。真实 pointer move/down/外部等待/up 打开 MainMenu New Game，Confirm 后加载 `InitialHarbor` build index=`2`。随后按暂停要求停止，没有将未执行的 W/A/S/D、E、I、M、T、Esc 和完整业务重复/拒绝路径记为 PASS。
- R11 runtime error console=`0`；Player.log 严格 `ERROR|EXCEPTION|CRASH|FATAL=0`；`quit` 后 PID `2724` 与 descriptor 均消失。R10 仍是 Q29 最近一次完成的 hidden Player 全交互路线，本轮 R11 是入口 smoke/安全收口记录，不替代 R10。
- 最终回读：Runtime `enableInBuilds=false / port=0`；Build `developmentBuild=false / allowDebugging=false / connectWithProfiler=false`；Editor=`ready/stopped`。不清理已有未跟踪历史证据与构建产物。

暂停交接：[PAUSE_20260915_Q29](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260915_Q29.md)；结果：[Q29 R11 paused result](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED_RESULT.json)；日志：[Q29 R11 paused log](../../game/tools/results/LOCAL-QUALITY/Q29/Q29_CURRENT_PIPELINE_REAUDIT_20260915_LUNA_R11_PAUSED.log)。

暂停状态：Q29=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。恢复入口为 Q30 当前源码独立审查，再进入 Q31 最终 native Player visual gate；formal action/avatar、natural swim/helm、植被接地/穿模、前台物理键盘、目标设备性能和人工最终视觉仍开放。

## 2026-09-15 Q30 current-source independent review R5

- Q30 R5 重新绑定当前 HEAD `94950f36c0d5a3daf4ba719216808a0ae6d71be9`，没有修改生产代码、测试契约或持久运行时配置。Python 定向 `65/65 PASS`；D16InputRouter `11/11 PASS`；D17ActionQuality `16/16 PASS`；VcG01WorldLogic `14/14 PASS`。
- 安全回读：Editor=`ready/stopped`；Runtime Pipeline=`enableInBuilds=false / port=0`；Build=`StandaloneWindows64 / non-development / debugging=false / profiler=false`。
- 新增明确环境边界：本轮 Editor Console 真实捕获 FMOD `Error initializing output device (60)`；Windows 音频设备存在且 Unity AudioSettings 有配置，但 normal output 未证明成功。音频保持 `BLOCKED_ENVIRONMENT`，不得把配置存在写成播放通过。Warning=`160 / dropped=false`，按 D27/D2/D6/expected/uncategorized 家族登记。
- Q30 仍 `PARTIAL`；VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。下一张执行卡为 Q31 最终 native Player visual gate；需要同时保留 compositor、前台物理输入和音频环境边界。

结果：[Q30 R5](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5_RESULT.json)；日志：[Q30 R5 log](../game/tools/results/LOCAL-QUALITY/Q30/Q30_CURRENT_SOURCE_INDEPENDENT_REVIEW_20260915_LUNA_R5.log)；交接：[Q30 handoff](design/common/handoffs/LOCAL-QUALITY/Q30/Q30.md)。

## 2026-09-15 Q31 R6 visible native Player diagnostic boundary

- 当前 HEAD `9fd08f711e7b699a32a420d82a0a5f1d4ac38a0c` 已通过 Unity Pipeline 构建并启动可见原生 Windows Player：`build_9f35f8fcb2f4`，`StandaloneWindows64 / Succeeded / 0 errors / 0 warnings / 243870645 bytes`。
- MainMenu NewGame/Confirm → `InitialHarbor`、E 登船/下船和 `1600x900` compositor-visible 截图已取得；这些是有效视觉诊断证据，不是最终视觉 PASS。
- 下船后的 W/A/D 有界路线出现约 `z=-4.94 / -18.48 / -23.36` 停滞。本轮没有权威 `IsSwimming=true` marker；既有 S/open-water 证据继续单独保留，不能混算成完整游泳通过。
- 会话在最终 runtime console、严格 Player 日志和 clean-close 断言前中断。接管后 Player PID/descriptor 均不存在，`RuntimePipelineConfig.json` 已读回 `enableInBuilds=false / port=0`，正确 `game` Editor 为 Pipeline `127.0.0.1:7800 / ready`。
- 状态保持：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。
- 下一执行卡：使用 connected Editor Pipeline 复现路线并记录相机朝向、角色 `CharacterController`、地形/建筑 Collider 命中、OceanAdapter 水面查询与 `IsSwimming` 转换；先确定单一 owner，再写最小回归和生产修复。不得用截图、固定 token 或构造测试位置升级状态。

结果：[Q31 R6](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_VISUAL_20260915_LUNA_R6_RESULT.json)；日志：[Q31 R6 log](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_VISUAL_20260915_LUNA_R6.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

## 2026-09-16 Q31 R7 S-route and swim-presentation defect boundary

- 当前源码可见 Player 通过 Pipeline pointer 进入 `InitialHarbor`；相机平面前方为 `-Z`，W 是船/港口侧路线，S 是 `+Z` 水域路线。
- S 分段真实运行从 `(0.00,0.15,-2.70)` 到约 `(0.33,0.40,18.80)`，状态为 `IsSwimming=true / IsGrounded=false / D16InputRouter`；释放后游泳态保持。结论是 S→游泳状态 `PASS_WITHIN_SCOPE`，不是自然泳姿通过。
- 真实可见帧暴露新的明确 owner 候选：`D2CharacterAnimationController`/D7 presentation 与 formal avatar asset contract。角色在岸线以扭曲水平姿态和弯折四肢呈现，水面接触不可信；这比继续调岸线坐标更优先。
- 同一 Player 的 InitialHarbor runtime_status 观察为约 `3.8–4.5 FPS`、平均帧 `224–261ms`；记为严重环境运行观察，另开性能卡，不直接升级或关闭目标设备性能门禁。
- R7 已完成 error console=`0`、Player.log 严格错误行=`0`、clean quit、PID/descriptor 消失和 Runtime Pipeline `enableInBuilds=false / port=0` 恢复。
- 状态保持：Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL`。
- 下一执行卡：围绕泳姿 presentation 单一 owner 写 focused regression，确认 formal Avatar 是否有可用的 Swimming state/clip；若无，则在现有 D2 presentation owner 内做最小可视修复，并再次使用 visible Player 验证。

结果：[Q31 R7](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7_RESULT.json)；日志：[R7 log](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_ROUTE_20260916_R7.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

### 2026-09-16 Q31 R12 — swim envelope remediation recorded

- 已完成：D20 Formal Humanoid 泳姿 envelope 最小修复；body presentation 下移 `0.85m`，前臂/小腿保持伸展，并加入 focused PlayMode safety contract。
- 已验证：R12 Development Player build `0 errors / 0 warnings`；S 路线进入真实 `IsSwimming=true`；D17A03 `1/1 PASS`；runtime console 和 Player.log 严格错误边界为 `0`；clean quit 和配置恢复成功。
- 仍开放：真实帧中姿态虽改善，仍有单臂划水、风格化动作和水面接触不可信；Q31=`PARTIAL`，VC-G01=`IN_PROGRESS`。
- 下一执行卡：检查 action/boarding presentation 的单一 owner，或将剩余泳姿缺陷保持为可追踪待办；不再盲调岸线坐标。

结果：[Q31 R12](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12_RESULT.json)；日志：[R12 log](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_SWIM_FORMAL_SAFE_20260916_R12.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

### 2026-09-16 Q31 R13 — boarding/helm route recorded

- 已验证：R12 Player 的 E 登船/下船/重复登船和 W/A 船控路线；D7 parent/PlayerBoarded、D2 mode、Formal D20 `Helm` state 均有真实运行时证据。
- 已加回归：D17A02 现在强制检查 D2 `Boarding`→D20 Formal `Board`→稳定 `Helm`，重新编译后 `1/1 PASS`；D17A03 `1/1 PASS`。
- 仍开放：RenderTexture 帧虽显示角色位于可见舵轮且双手接近轮盘，低模几何、正式动作资源和自然 hand IK 仍未闭合；Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`。
- 下一执行卡：转向下一个单一 owner 的动作或植被缺陷，或将正式 boarding/helm animation/IK 记录为独立待办，不把逻辑路线通过扩大成最终视觉通过。

结果：[Q31 R13](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13_RESULT.json)；日志：[R13 log](../game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_NATIVE_PLAYER_BOARDING_HELM_ROUTE_20260916_R13.log)；交接：[Q31](design/common/handoffs/LOCAL-QUALITY/Q31.md)。

### 2026-09-16 Q00/Q01/Q04 current-source takeover after R13

- 已完成：从 R13 安全暂停点重新做 Q00 当前工作区/Player/Editor 对账；完成 Q01 当前物理根、D20 Formal presentation、尺度、船/水域/碰撞 owner 根因采样；按依赖重新验证 Q04 水域 owner。
- 证据：工作区 `1464` 条 status lines、`1457` 条未跟踪、`7` 条 tracked/staged，未清理。R12 Player `build_8eca102a61b4` 为 R13 复用的准确二进制；D20 生产 patch 已由 HEAD `0a711417` 记录，R13 的 D17A02 测试断言不声称进入 Player。D17A02/D17A03/D17A07 均 `1/1 PASS`，recompile=`up_to_date`，console seq `400` 后 Error delta=`0`，Editor=`ready/stopped`。
- 状态：Q00=`VERIFIED_WITH_SCOPE`、Q01=`PASS_WITHIN_SCOPE`、Q04=`PASS_WITHIN_SCOPE`；这些状态仅覆盖源/owner/逻辑回归边界。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持。
- 下一步：Q05 当前游泳 presentation 复核，然后回到 Q31 R14 选择一个单一 visual owner；不得把 Pipeline 逻辑回归、R12/R13 Player 状态或 RenderTexture 局部帧扩大为最终视觉通过。

权威结果：[Q00 R1](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_Q31R13_20260916_LUNA_R1_RESULT.json)、[Q01 R1](../game/tools/results/LOCAL-QUALITY/Q01/Q01_CURRENT_ROOT_CAUSE_Q31R13_20260916_LUNA_R1_RESULT.json)、[Q04 R1](../game/tools/results/LOCAL-QUALITY/Q04/Q04_CURRENT_WATER_OWNER_Q31R13_20260916_LUNA_R1_RESULT.json)；交接：[Q00/Q01/Q04/Q31](design/common/handoffs/LOCAL-QUALITY/Q00.md)。

## 2026-09-16 Q05 current-source transition guard after Q31 R13

Q05 接管复核先将一条真实失败登记为回归：新增的 visual-root 上限断言发现，D7 上岸校准偏移会在 D2 进入 `Swimming` 的首帧抬高程序化 fallback 到 `0.396687269m`。根因和失败证据见 [Q05 R1](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_SWIMMING_PRESENTATION_REGRESSION_Q31R13_20260916_LUNA_R1_RESULT.json)。

修复严格限制在 `D2CharacterAnimationController.UpdateAction`：进入 `Swimming` 时清除 grounded-only D7 offset。当前源码重新编译后，D17A08/D17A03/D17A07/D17A02 均为 `1/1 PASS`；runtime probe 记录 visualRoot local Y=`0.0144426823m`、grounding offset=`0`，Error Console 增量=`0`，Editor=`ready/stopped`。Q05 恢复 `PASS_WITHIN_SCOPE`，但不解锁 VC-G01 或 native Player 最终视觉。权威结果：[Q05 R2](../game/tools/results/LOCAL-QUALITY/Q05/Q05_CURRENT_SWIMMING_PRESENTATION_Q31R13_20260916_LUNA_R2_RESULT.json)；[Q05 handoff](design/common/handoffs/LOCAL-QUALITY/Q05/Q05.md)。下一张执行卡是 `Q06_CURRENT_SOURCE_REAUDIT_Q31R13_20260916_LUNA`。

## 2026-09-16 Q06 current-source re-audit after Q05 transition guard

Q06 在 Q05 D2 transition guard 修复后重新绑定当前源码：`D17A09TenJumpCyclesAndGroundedDiagonalMotionStayFinite=1/1 PASS`，Pipeline=`10.57s`、test=`10.4788045s`。十次真实 TryJump→落地、正向/斜向/横向/反向小步均保持 grounded、non-swimming、finite Transform；没有关闭碰撞、增加空气墙、放宽阈值或注入非法位置。Error Console 增量=`0`，证据后清空，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。

Q06 保持 `PASS_WITHIN_SCOPE`，只覆盖 InitialHarbor 基础跳跃/接地/出生点小步稳定性，不关闭墙、坡、岸线、Ramp/跳板、船甲板视觉接触、native Player 键盘、植被、性能或 VC-G01。权威结果：[Q06 R1](../game/tools/results/LOCAL-QUALITY/Q06/Q06_CURRENT_SOURCE_REAUDIT_Q31R13_20260916_LUNA_R1_RESULT.json)；[Q06 handoff](design/common/handoffs/LOCAL-QUALITY/Q06/Q06.md)。下一张执行卡是 `Q07_CURRENT_COLLISION_ROUTE_REAUDIT_Q31R13_20260916_LUNA`。

## 2026-09-16 Q07 current-source collision-route re-audit after Q31 R13

Q07 当前源码复核未修改 Harbor 碰撞生产代码、场景、Prefab 或配置。四栋 R03 服务建筑 collision owner 测试 `1/1 PASS`（test=`1.4001556s`），D19 terrain/DockGround/GangplankGround 双向路线测试 `1/1 PASS`（test=`2.994615s`）；Error Console 增量=`0`，证据后清空，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。

Q07 保持 `PASS_WITHIN_SCOPE`，只关闭已覆盖的建筑碰撞 owner 与地形/码头/跳板自动化路线；不关闭所有坡墙岸线、视觉网格接触、完整船甲板/上下船、native Player WASD、植被、性能或 VC-G01。Q08 继续因正式动作/Avatar 资源阻塞，下一张独立执行卡是 `Q09_CURRENT_DECK_ANCHOR_REAUDIT_Q31R13_20260916_LUNA`。权威结果：[Q07 R1](../game/tools/results/LOCAL-QUALITY/Q07/Q07_CURRENT_SOURCE_REAUDIT_Q31R13_20260916_LUNA_R1_RESULT.json)；[Q07 handoff](design/common/handoffs/LOCAL-QUALITY/Q07/Q07.md)。

## 2026-09-16 Q08 current-source integration re-audit after Q31 R13

Q08 当前依赖门已重新验证：Q03Formal EditMode=`5/5 PASS`、Q03D20Formal PlayMode=`2/2 PASS`；完整 `D17ActionQualityPlayModeTests`=`16/16 PASS`，Pipeline=`52.88s`，没有失败、跳过或不确定项。覆盖移动/跳跃/游泳/岸线、接地/表现 owner、碰撞路线、缩放甲板/上下船、船动相对关系、安全下船、相机交接、W/S 不导航 HUD、Esc 模态恢复和综合路线。

长套件后 Error 查询返回 `0`，但日志缓冲 `dropped=true`，所以不作更强的零丢失日志声明；证据后已清空 Console，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。Q08 恢复为 `PASS_WITHIN_SCOPE`，仅代表 connected Editor 代码级整合，不代表 native Player/compositor、前台键盘、自然 authored action/swim/helm、植被、性能、音频或 VC-G01 完成。权威结果：[Q08 R1](../game/tools/results/LOCAL-QUALITY/Q08/Q08_CURRENT_SOURCE_INTEGRATION_Q31R13_20260916_LUNA_R1_RESULT.json)；[Q08 handoff](design/common/handoffs/LOCAL-QUALITY/Q08/Q08.md)。下一张执行卡是 `Q09_CURRENT_DECK_ANCHOR_REAUDIT_Q31R13_20260916_LUNA`。

## 2026-09-16 Q09 current-source deck-anchor re-audit after Q31 R13

Q09 当前源码 `D17A11StableDeckAnchorSupportsTenBoardingCyclesAndRejectsRemoteBoarding=1/1 PASS`，Pipeline=`3.12s`、test=`3.0245131s`；十次非单位船缩放上下船、HelmPoint 稳定关系和约 30m 远距拒绝通过。Error 查询返回 `0` 但 `dropped=true`，证据后清空 Console，Editor=`ready/stopped`，Runtime Pipeline=`enableInBuilds=false / port=0`。

Q09=`PASS_WITHIN_SCOPE` 只覆盖缩放锚点/上下船生命周期/远距拒绝；自然握舵、航行中船动相对关系、安全下船、视觉、native Player、植被、性能和 VC-G01 仍开放。下一张执行卡为 `Q10_CURRENT_PIPELINE_REAUDIT_Q31R13_20260916_LUNA`。结果：[Q09 R1](../game/tools/results/LOCAL-QUALITY/Q09/Q09_CURRENT_SOURCE_REAUDIT_Q31R13_20260916_LUNA_R1_RESULT.json)。

## 2026-09-16 temporary safe pause after Q09 R1

本轮在 Q09 完成后暂停开发，建立可恢复的安全收口点。Q05-Q09 的当前源码证据、Q05 的真实回归修复和对应 handoff 已登记；没有把这些 Editor/PlayMode 结果扩大为 native Player 或最终视觉通过。Q31=`PARTIAL`、VC-G01=`IN_PROGRESS`、VC-G02/VC-G03=`LOCKED`、D31-P04/S05/G06=`PARTIAL` 保持不变。恢复入口是 `Q10_CURRENT_PIPELINE_REAUDIT_Q31R13_20260916_LUNA`，暂停结果见 [Q31 Q09 R1 pause checkpoint](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_Q09_R1_RESULT.json) 与 [pause handoff](design/common/handoffs/LOCAL-QUALITY/PAUSE_20260916_Q09_R1.md)。

## 2026-09-16 Q00/Q10 resume after Q09 safe pause

恢复后先做 Q00 当前接管重绑定：本地 HEAD 与远程均为 `8413feeffa5cf4e1367c3aaff73890f30bb39d2f`，工作区 `1456` 条 status lines 全部为既有未跟踪历史/生成证据，tracked diff=`0`，未执行清理；Unity Editor Pipeline=`6000.0.80f1 / PID 34276 / 127.0.0.1:7800 / ready`，Runtime Pipeline=`enableInBuilds=false / port=0`。当前源 D2/D7/D20/D17 hash 已重新登记。R12/R13 Player `build_8eca102a61b4` 来自旧 source head `9fd08f...`，早于 Q05 D2 Swimming-entry transition guard，`exact_current_source_match=false`，不得继承为当前 Player 验收。权威证据：[Q00 R2](../game/tools/results/LOCAL-QUALITY/Q00/Q00_CURRENT_TAKEOVER_Q31Q09_20260916_LUNA_R2_RESULT.json)、[Q00 handoff](design/common/handoffs/LOCAL-QUALITY/Q00.md)。

随后执行 Q10 当前源码 Pipeline 定向回归，没有修改生产代码、Prefab、场景或持久配置。`D17A12ShipMotionKeepsBoardedPlayerRelativeDeckStable` 第二次运行 `1/1 PASS`（Pipeline=`1.96s`、test=`1.8683108s`），第一次独立运行也为 `1/1 PASS`；真实 marker 为 `motion_samples=10`、最大相对位置/旋转误差=`0.000000`、最大船位移=`1.510m`、最大航向变化=`32.000°`。Error Console=`0`、dropped=`false`；已知 warning tail 原样保留并在取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q10=`PASS_WITHIN_SCOPE` 只关闭 snapshot 驱动的相对甲板与状态恢复边界，不关闭前台/native Player、开放海、船碰撞、相机/握舵视觉、正式动作/Avatar、植被、性能、音频或 VC-G01。权威证据：[Q10 R1](../game/tools/results/LOCAL-QUALITY/Q10/Q10_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q10 handoff](design/common/handoffs/LOCAL-QUALITY/Q10/Q10.md)。下一张执行卡为 `Q11_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q11 紧随 Q10 完成当前源码定向回归，没有修改生产代码、Prefab、场景或持久配置。`D17A13SafeDisembarkSurvivesTenLegalCyclesAndRejectsNoGround=1/1 PASS`（Pipeline=`3.23s`、test=`3.1345655s`），marker 为 `legal_disembark_cycles=10`、`no_ground_rejection=true`、`final_grounded=True`、`final_mode=Character`。无落点拒绝 warning 被保留为预期行为证据；Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q11=`PASS_WITHIN_SCOPE` 只关闭当前合法/无落点下船状态契约与恢复，不关闭移动船体视觉碰撞、全岸线/跳板路线、native Player、正式动作/Avatar、相机、植被、性能、音频或 VC-G01。权威证据：[Q11 R1](../game/tools/results/LOCAL-QUALITY/Q11/Q11_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q11 handoff](design/common/handoffs/LOCAL-QUALITY/Q11/Q11.md)。下一张执行卡为 `Q12_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q12 随后完成当前源码 CameraFollow 定向回归，没有生产代码、Prefab、场景或持久配置变更。`D17A14CameraModesAndExternalObstructionRecoverWithoutInvalidPose=1/1 PASS`（Pipeline=`3.41s`、test=`3.3152951s`），marker 为 `mode_cycles=10`、`obstructed=true`、`distanceBefore=5.500`、`distanceBlocked=3.790`、`distanceRecovered=5.494`、`collisionChecks=109`。受控 BoxCollider 的 SphereCast 收缩与移除后的阻尼恢复成立；Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q12=`PASS_WITHIN_SCOPE` 只关闭相机模式/目标和受控遮挡恢复，不关闭最终世界穿模/构图、native Player、性能、音频或 VC-G01。权威证据：[Q12 R1](../game/tools/results/LOCAL-QUALITY/Q12/Q12_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q12 handoff](design/common/handoffs/LOCAL-QUALITY/Q12/Q12.md)。下一张执行卡为 `Q13_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q13 紧随 Q12 完成当前源码输入/UI 定向回归，没有生产代码、Prefab、场景或持久配置变更。D17A15=`1/1 PASS`（Pipeline=`3.51s`）、D16InputRouter=`11/11 PASS`（Pipeline=`5.18s`）、D16PrimaryHudNavigation=`2/2 PASS`（Pipeline=`2.93s`）；D17A15 marker 为 `gameplay_w_s_cycles=10`、`normal_navigation=false`、`pause_navigation=true`、`blocked_navigation=true`、`restored_navigation=false`。真实日志还记录 `EventSystem navigation=modal -> gameplay_suppressed`、W/S 主选择保持 null 和 ship W gameplay route。Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q13=`PASS_WITHIN_SCOPE` 只关闭 Unity Input System/运行时的 W/S-HUD 隔离、Esc 模态恢复和 UI Move 防竞态；不关闭前台 Win32 物理键盘、渲染 HUD、E/I/M/T、原生 Player、正式动作/Avatar、植被、性能、音频或 VC-G01。权威证据：[Q13 R1](../game/tools/results/LOCAL-QUALITY/Q13/Q13_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q13 handoff](design/common/handoffs/LOCAL-QUALITY/Q13/Q13.md)。下一张执行卡为 `Q14_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q14 随后完成当前源码船岸整合定向回归，没有生产代码、Prefab、场景或持久配置变更。`D17A16IntegratedShoreWaterDeckTurnAndSafeDisembarkSequence=1/1 PASS`（Pipeline=`2.71s`、test=`2.6117578s`），marker 为 `shore_water_shore=true`、`board_owner=Ship`、`ship_input_turn=true`、`no_ground_rejection=true`、`legal_disembark=true`、`imported_neutral=False`。Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q14=`PASS_WITHIN_SCOPE` 只关闭岸→水→岸、登船/转向、无落点拒绝和合法下船的 owner/state 链；`imported_neutral=false` 仍是程序化 Player 分支，不是正式动作资源通过；前台/native Player、E/I/M/T、最终视觉、植被、性能、音频和 VC-G01 仍开放。权威证据：[Q14 R1](../game/tools/results/LOCAL-QUALITY/Q14/Q14_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q14 handoff](design/common/handoffs/LOCAL-QUALITY/Q14/Q14.md)。下一张执行卡为 `Q15_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q15 随后完成当前源码 D29/D16 面板焦点定向回归，没有生产代码、Prefab、场景或持久配置变更。`D29Q15WarehouseMarketTavernAreExclusiveAndRecoverTenTimes=1/1 PASS`（Pipeline=`13.67s`、test=`13.5745984s`），真实 marker 为 `shortcut_input=true`、`keys=I,M,T`、`legacy_keydown_reached_owner=true`，三页各 `10` 次循环、互斥、重复请求拒绝和 Gameplay 恢复均为真。Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q15=`PASS_WITHIN_SCOPE` 只关闭 D29/D16 PlayMode 快捷键 owner/模态焦点边界，不关闭前台 Win32 键鼠、原生 Player 面板渲染/命中、业务 owner 真实结果、正式动作、性能、音频或 VC-G01。权威证据：[Q15 R1](../game/tools/results/LOCAL-QUALITY/Q15/Q15_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q15 handoff](design/common/handoffs/LOCAL-QUALITY/Q15/Q15.md)。下一张执行卡为 `Q16_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q16 随后完成当前源码 D29/D3 交易桥定向回归，没有生产代码、Prefab、场景或持久配置变更。`D29Q16TradeBridgeCoversSuccessRejectBusyAndOneChangePerRequest=1/1 PASS`（Pipeline=`2.30s`、test=`2.1992274s`），marker 为 `not_ready=true`、`shortcut_double_click_one_owner=true`、`buy_success_one_change=true`、`sell_success_one_change=true`、`insufficient_funds_unchanged=true`、`cargo_full_unchanged=true`；真实 D3 owner 反馈购买/出售完成。Error Console=`0`、dropped=`false`，取证后清空，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q16=`PASS_WITHIN_SCOPE` 只关闭交易 owner 桥接和成功/拒绝状态，不关闭 native Player 鼠标/市场渲染、在线/存档数据、完整航程、正式动作、性能、音频或 VC-G01。权威证据：[Q16 R1](../game/tools/results/LOCAL-QUALITY/Q16/Q16_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q16 handoff](design/common/handoffs/LOCAL-QUALITY/Q16/Q16.md)。下一张执行卡为 `Q17_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。

Q17 随后完成当前源码 D29/D9/D4 整备与船坞桥复核，没有生产代码、Prefab、场景或持久配置变更：`D29Q17ShipyardAndPreparationCoverSuccessRejectDuplicateCancelAndException=1/1 PASS`，Pipeline=`3.08s`、test=`2.9858148s`。marker 的修船成功、材料拒绝、重复不变、同 revision 幂等、取消恢复、船坞成功/资金拒绝、异常恢复和主输入恢复均为 `true`；D4 空 owner 只记录为受控 Warning，Error=`0`、dropped=`false`，证据后清 Console，Editor=`ready/stopped`，Runtime Pipeline 关闭。Q17=`PASS_WITHIN_SCOPE` 只关闭当前 owner 逻辑/反馈边界，不关闭前台/native Player、原生鼠标/键盘、最终视觉、正式动作/Avatar、植被、性能、音频、完整首航或 VC-G01。结果/日志/交接：[Q17 R1](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1_RESULT.json)、[Q17 log](../game/tools/results/LOCAL-QUALITY/Q17/Q17_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA_R1.log)、[Q17 handoff](design/common/handoffs/LOCAL-QUALITY/Q17/Q17.md)。本轮安全暂停点：[Q31 Q17 R1](../game/tools/results/LOCAL-QUALITY/Q31/Q31_PAUSE_CHECKPOINT_20260916_Q17_R1_RESULT.json)，恢复入口为 `Q18_CURRENT_PIPELINE_REAUDIT_Q31Q09_20260916_LUNA`。
