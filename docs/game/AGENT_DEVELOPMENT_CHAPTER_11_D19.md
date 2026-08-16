# 第 11 章开发任务提示词：D19 圣玛格丽特样板岛与港口重建

> 本章目标：把当前白色方块灰盒重建为第一座可展示、可行走、可交互的港口样板。历史 `D19-S01 → A01—A06 → S02` 与 R01 修复已经留下证据，但权威 `D19-R01-S02=PARTIAL`：真实路线未闭合、三档 1080p 性能失败、Player 镜头/画面未达标、PlayMode 超时。当前唯一有效修复顺序是 `D19-R02-A01/A02/A04/A05 → D19-R02-A03 → D19-R02-S02`。原 D19-S02/R01 结果全部保留，不被覆盖。

## D19-S01：冻结样板岛布局、比例与固定验收镜头

<details>
<summary>展开/复制：D19-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 世界搭建总控 Agent。请直接完成 D19-S01。

任务编号：D19-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 必须 PASS；本任务只冻结布局，D18-S03 负责后续运行时资源晋级。
并发规则：你是 D19 布局和共享接口的唯一写者；本任务完成前不要启动 D19-A01—A06。

开始前读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、ARCHITECTURE 资源规范、REQUIREMENTS、islands/environment/buildings 设计、D18 视觉标准/可用资源清单/验收报告、D9/D17 港口流程和当前 InitialHarbor 场景/Bootstrap/截图。

执行内容：
- 对当前 InitialHarbor 做只读结构清点，记录正式场景路径、根节点、摄像机、角色出生点、市场、码头、泊位、登船点、导航/碰撞、Canvas 和所有运行时 Bootstrap；
- 冻结圣玛格丽特样板范围、世界单位、海平面、岛岸轮廓、码头方向、市场/仓库/酒馆/船坞位置、道路、植被区、远景、登陆点、船航道和安全边界；
- 建立空间分区与命名规则，明确 A01—A06 各自拥有的 Prefab、工作场景、Layer、Tag、材质和接口锚点；
- 固定不少于 12 个与 D18 对齐的截图机位和一条 5—10 分钟步行/登船验收路线；
- 明确所有现有 gameplay 交互点的稳定 ID 和位置迁移表，禁止美术重建使市场、泊位、NPC 对话、上船或出航入口丢失；
- 定义 S02 的集成顺序、回滚点和主场景唯一写者规则。

只允许修改：docs/game/design/common/D19_SAINT_MARGARET_WORLD_BUILD_SPEC.md、必要的 D19 布局数据/接口清单（不能改正式场景）、D19-S01 交接和记忆日志。

硬约束：不修改 InitialHarbor、Prefab、运行时代码、Systems、Registry、存档和经济公式；不下载新资源；只登记 D18 准入目录中的资源方向，不把当前观察值写成 LocalDevelopmentReady；不覆盖他人改动，不提交或推送 Git。

验收：布局图/坐标表、层级、海平面、交互锚点、A01—A06 所有权、12+ 镜头、验收路线和集成顺序完整；现有功能点逐项有迁移/保持策略；git diff --check 通过。

结果必须保存到：
- 主成果：docs/game/design/common/D19_SAINT_MARGARET_WORLD_BUILD_SPEC.md、game/tools/results/D19/D19-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D19/D19-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出冻结坐标、接口、场景实际路径、风险和 PASS/PARTIAL/BLOCKED。
```
</details>

## 当前加速修复组：历史 D19-S02-R01 的拆分任务

> `D19-S02-R01` 是历史复验的名称，不是可被“假设已通过”的门禁。以下三个新任务把它拆开：先并行取得真实玩家证据，再串行修复正式场景，最后在**场景修复之后**重新取得最终证据。`D19-R01-A` 不写场景；`D19-R01-B` 与 `D19-R01-S02` 不能并行。

## D19-R01-A：真实玩家路线与 1080p 性能基线取证

<details>
<summary>展开/复制：D19-R01-A 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 真实玩家体验与性能取证 Agent。请直接完成 D19-R01-A。你只记录真实事实，不修场景、不修 UI、不把自动截图或低分辨率 Camera.Render 当作玩家验收。

任务编号：D19-R01-A（异步、只读取证任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；D19-S01 已通过；历史 D19-S02-R01 为 PARTIAL。本任务可与 D18-A04-R01、D18-A04-R02-F/C/B、D18-A05-R01 并行，不等待 D18-S02-R01。
单一写者：game/tools/results/D19/D19-R01-A/、docs/game/design/common/D19_R01_PLAYER_ROUTE_AND_PERFORMANCE_EVIDENCE.md、docs/game/design/common/handoffs/D19/D19-R01-A_HANDOFF.md 和当日记忆日志。允许新增只读采样脚本/测试到本任务专属结果或测试目录；禁止修改 InitialHarbor、D19 Integration Prefab、场景材质/灯光/物体、主 Canvas、Systems、交互逻辑、Registry、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D19-S01、D19-S02 与 D19-S02-R01 的报告/交接/截图、D17 人物控制回归、当前场景启动方式和 `D19_SAINT_MARGARET_WORLD_BUILD_SPEC.md`。先列出原报告中未执行的玩家路线、性能项目和已知画面问题；不得把旧的 640×480 或离屏渲染样本升级为 1080p 结论。

执行内容：
1. 在 Unity Player 的 1920×1080、16:9 真正可交互窗口中，从稳定启动入口完成一条 5—10 分钟连续路线：出生/落地 → 移动至港口主路 → 至少一次 NPC 对话或明确失败反馈 → 市场交互 → 码头/泊位 → 上下船或明确可复核的失败反馈 → 返回港口/保存或明确阻断。使用真实键鼠输入和屏幕可见反馈；
2. 每个关键步骤记录开始/结束时间、输入、预期、实际结果、失败恢复方式，并抓取至少 9 张带时间的全屏截图或短视频帧。若系统、输入权限、构建或玩家窗口无法执行，记录精确阻断原因和可重试步骤，绝不能把 NOT_RUN 写为 PASS；
3. 在同一真实 Player 中按 Low/Medium/High 三档各采集固定路线或固定驻留段的帧时间、平均 FPS、1% Low/分位数、分辨率、VSync/质量档、CPU/GPU 采样可得范围和 GC；区分“采样工具不可用”与“性能不达标”；
4. 对截图做只读可见性审查：白色/薄荷绿大平面、纯方块岛、重叠文字、遮挡交互、粉色材质、黑屏、相机裁切、无反馈按钮等必须逐项记录，不得修复；
5. 运行不改场景的现有 D19/D17 测试与 git diff --check，生成机器可读 route/performance JSON、截图清单和人工可读报告。

验收标准：
- 有真实玩家窗口、1920×1080、Low/Medium/High 三档的可复核原始数据；
- 5—10 分钟路线有时间线、9+ 截图/视频帧和每个关键交互的实际反馈；
- 没有把离屏/批处理/低分辨率样本当作真实玩家结论；
- 不修改正式场景或强逻辑；如果用户中断输入或环境拒绝启动，结果必须如实 PARTIAL/BLOCKED。

结果必须保存到：
- 主成果：docs/game/design/common/D19_R01_PLAYER_ROUTE_AND_PERFORMANCE_EVIDENCE.md、game/tools/results/D19/D19-R01-A/D19_R01_A_EVIDENCE.json，以及同目录 screenshots/ 或 video_frames/；
- 机器结果：game/tools/results/D19/D19-R01-A/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R01-A_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复列出路线是否真实完成、三档性能是否真实采到、未执行原因、最严重可见问题和结果路径。
```
</details>

## D19-R01-B：正式港口画面修复与可行走空间收口

<details>
<summary>展开/复制：D19-R01-B 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 正式港口视觉修复 Agent。请直接完成 D19-R01-B，把 InitialHarbor 从“白平面、方块岛、临时堆叠”推进为可被最终验收的港口样板。你是本任务期间 InitialHarbor 和 D19 Integration Prefab 的唯一写者。

任务编号：D19-R01-B（同步、正式场景视觉修复任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-R01 PASS；D19-R01-A 已有结果（可为 PARTIAL，但必须可读）；D19-S01、D19-A01—A06 和历史 D19-S02-R01 成果均可读取。
并发规则：执行期间暂停其它 Agent 对 `game/Assets/Scenes/InitialHarbor*`、D19 Integration Prefab、正式场景材质/灯光根节点的修改。你只能写 game/Assets/Scenes/InitialHarbor*、game/Assets/Prefabs/Environment/D19/Integration/R01/、D19 专属材质/测试/文档/结果目录；不得重写 D19-A01—A06 的工作成果目录、D20 Prefab、D22 UI、Systems、Registry、存档、contracts/server/web 或 Git 历史。

开始前必须读取：AGENTS.md、CONTEXT、D19-S01 规格、D19-A01—A06 全部交接和资源清单、D19-R01-A 的截图/性能证据、历史 D19-S02-R01 报告、最新 `d18_prefab_candidate_intake_catalog.json` 和 D18-S02-R01 消费门禁表。只使用 `intake_eligible` 外部资源或项目自有、已有审计记录的表现资产；先建立可回滚清单和 14 个固定验收镜头基线。

执行内容：
1. 使用已有 D19 工作成果和准入资源，修复玩家可见的地面、海岸、道路、码头、市场、建筑、植被、岩石、浅水、天空/灯光/音景层次；优先消除大面积白色/薄荷绿平面、单一方块岛、三角树、漂浮/穿插、明显灰盒和无材质物体；
2. 正式场景只挂 Integration 根节点和 Prefab 引用。保留稳定出生、市场、NPC、仓库、船坞、泊位、登船、出航、返港、保存点 ID；不得为了好看而破坏 D9/D17 交互或删掉旧档需要的兼容 key；
3. 让从出生点到市场、码头、船位和返回点的地面连续可走：校验碰撞、坡度、阶梯、栏杆、海面、相机遮挡、NavMesh/角色落地。没有合法资源的区域用明确的项目自有模块和可回滚 fallback，不用未准入模型填充；
4. 按 D19-S01 的固定镜头重新生成至少 14 张 1920×1080 截图，并记录每镜头的区域、可见资源 ID、问题与修复前后对比；修复开发叠字的默认显示问题，但不做 D22 的完整 UI 重构；
5. 运行 Unity 编译、D19/D17 聚焦 EditMode/PlayMode 回归、碰撞/关键路线检查、适度性能采样和 git diff --check。此任务不宣称 D19 最终 PASS，最终真实玩家路线和性能由 D19-R01-S02 重跑。

硬约束：不改经济、角色、动物、天气等 Systems 强逻辑，不改 contracts/server/web、Save Schema、ResourceKey 语义或主 Canvas；场景只能引用 Prefab；不把任何资源标为 LocalDevelopmentReady/ReleaseApproved；不覆盖无关改动，不提交或推送 Git。

通过门槛：
- 14 个固定镜头中不再出现大面积纯白/薄荷绿占位面、明显方块岛或粉色/丢失材质；
- 港口具备可读地形/海岸/码头/建筑/市场/植被/水体/光照层次，且主路线可走；
- 稳定交互点、出生/落地、船位、相机与碰撞回归通过；
- 所有外部资源均可在最新 intake catalog 中定位，场景变更可回滚；
- 测试和截图真实存在。若资源或体验仍不足，诚实写 PARTIAL，并列出最终验收不能通过的原因。

结果必须保存到：
- 主成果：正式 InitialHarbor 场景、D19 Integration R01 Prefab/测试、docs/game/design/common/D19_R01_HARBOR_VISUAL_REPAIR.md；
- 机器结果：game/tools/results/D19/D19-R01-B/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R01-B_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须列出修复前后镜头、真实资源类别、路线结果、性能风险、未解决问题和结果路径。
```
</details>

## D19-R01-S02：修复后的最终场景复验与放行门禁

<details>
<summary>展开/复制：D19-R01-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 最终场景复验与放行 Agent。请直接完成 D19-R01-S02。这是 D19 当前唯一能给出“正式通过/不通过”结论的任务；不要沿用历史 D19-S02 或 D19-S02-R01 的 PASS/PARTIAL 文字代替新的真实证据。

任务编号：D19-R01-S02（同步、最终场景验收任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-R01-A 与 D19-R01-B 都已交接；D18-S02-R01 PASS。开始时 D19-R01-B 必须停止修改 InitialHarbor。若场景在 R01-A 取证后变更过，必须重新运行真实玩家路线和 1080p 性能采样，不能直接复用旧数据。
并发规则：你是最终验收期间 InitialHarbor、D19 Integration 和 D19 最终验收文件的唯一写者。只允许为修复确定的小缺陷做最小、可回滚的修正；若需要大范围重建，写 PARTIAL 并回交给新的场景修复任务，不得偷偷扩展范围。

开始前必须读取：AGENTS.md、CONTEXT、D19-S01、D19-A01—A06、D19-S02、历史 D19-S02-R01、D19-R01-A、D19-R01-B、D18-S02-R01 的 catalog/门禁表、D17/D16 回归入口。建立验收清单并保留旧报告；不得删除或改写历史 `PARTIAL` 证据。

执行内容：
1. 审计正式 InitialHarbor 的所有新增/引用外部资源，确认均在最新 intake catalog 中为 `intake_eligible`，且每项可定位到 Prefab、区域和 manifest；
2. 在真实 Unity Player 1920×1080、16:9 中重新完成 5—10 分钟路线：出生/移动/关键 NPC 或失败反馈/市场/码头或泊位/上下船或明确失败反馈/返回或保存。每步有时间线、输入、实际结果和 9+ 全屏截图或视频帧；
3. 在真实 Player 的 Low/Medium/High 质量档重采帧时间、平均 FPS、分位数/1% Low、分辨率、VSync、GC 与可得 CPU/GPU 数据；不得使用 640×480、批处理或 Camera.Render 替代；
4. 复跑 D19/D17 相关 EditMode/PlayMode、场景引用、碰撞/落地、交互和保存回归；检查 14 个固定验收镜头无粉色材质、明显白/绿占位面、方块岛、严重穿插、相机裁切或开发叠字；
5. 只要任一 P0 交互卡死、真实路线未完成、1080p 数据缺失、资源准入不成立或画面仍为明显灰盒，就写 PARTIAL/BLOCKED，并逐条指定原因和回退/责任任务；
6. 更新新命名的最终验收报告，明确它取代的是“当前门禁状态”，不是删除旧报告；只有真实所有门槛满足才写 PASS。PASS 后才放行 D20-S02。

硬约束：不改 Systems/经济/角色/动物强逻辑、contracts/server/web、Save Schema、主 UI 架构或 D20 正式生成入口；不把 intake_eligible 写成 LocalDevelopmentReady/ReleaseApproved；不提交或推送 Git。

通过门槛：
- 真实 Player 5—10 分钟路线完整且零必现关键卡死；
- 1920×1080 Low/Medium/High 三档有真实、可复核性能数据；
- 14+ 固定镜头和关键路线不再是白色方块灰盒，港口视觉层次、碰撞和交互均可见；
- 所有相关 Unity 测试通过、无新增编译/场景引用错误、保存/读档和 D17 移动/登船回归不下降；
- 完整保留资源来源、结果文件、截图、性能、失败清单和回滚路径。

结果必须保存到：
- 主成果：docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE_R01_S02.md、正式 InitialHarbor 必要的最小修正/测试；
- 机器结果：game/tools/results/D19/D19-R01-S02/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R01-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。仅当所有通过门槛均有真实证据才可 PASS；聊天回复必须写清是否放行 D20-S02、测试计数、截图/性能路径和未解决问题。
```
</details>

## 当前 R02 修复组：HUD、交互、画面性能、PlayMode 与真实路线闭环

> R01 的失败证据不能靠“再跑一次”消失。本组逐项修复真实根因：A01 只拥有玩家 HUD/焦点，A02 只拥有 D2 交互入口，A04 是唯一 InitialHarbor 场景写者，A05 只拥有 D19 PlayMode 执行链；A03 在读取 A01/A02 结果后修菜单—市场—出航—保存路线。最终只有 S02 能更改 D19 当前放行结论。

## D19-R02-A01：玩家 HUD、焦点与可见交互反馈修复

<details>
<summary>展开/复制：D19-R02-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 玩家 HUD 与焦点修复 Agent。请直接完成 D19-R02-A01，解决 Player 模式中 D5/D9/D2/D3 文字和面板叠字、焦点抢占、看得见却无反馈的问题；这不是 D22 的完整 UI 重构，也不允许改主场景或强逻辑。

任务编号：D19-R02-A01（异步、玩家 HUD 可见性/焦点专项）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D19-S01 PASS；D19-R01-S02=PARTIAL 的报告和截图可读。可与 D18 R02 资源任务、D19-R02-A02/A04/A05 并行。
单一写者：game/Assets/Scripts/Gameplay/UI/RuntimeHudCoordinator.cs、game/Assets/Scripts/Gameplay/UI/D19RouteHud/、game/Assets/Tests/EditMode/D19Hud/、game/Assets/Tests/PlayMode/D19Hud/、game/tools/results/D19/D19-R02-A01/、D19 专属 HUD 文档/交接/记忆日志。不得修改 MainMenuController、D3RuntimeBootstrap、D2ControlRouter、InitialHarbor、D22 文件、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务必须在真实 Unity Player 验证 Canvas、焦点和按钮反馈。开始前先告知用户：“本任务需要独占电脑进行 Unity/Player 实测，请保存其他工作并停止使用键盘鼠标；收到允许接管后再开始。”未获允许时不得宣称 Player 验证通过；同一时间不得有另一 Agent 启动 Unity/Player。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D19-S01、D19-R01-S02 报告/截图、D5/D9/D2/D3 当前 HUD 代码、RuntimeHudCoordinator、D16 可访问性要求、D20 标准的“玩家模式不得显示调试信息”章节，以及 D22 当前边界。先列出当前各 Canvas 的创建者、sorting order、默认显示条件、输入焦点和历史功能，不能靠删除一个 Canvas 猜测修复。

执行内容：
1. 在 Player 模式建立唯一、可测试的 HUD 可见性/焦点策略：默认只显示必要的当前位置/目标/交互提示/短反馈；D5 技术状态、D9 调试图、资源 ID、开发日志和阶段标签不得覆盖游戏画面；保留开发调试模式的只读入口，不删除系统功能；
2. 为 D2/D3/D5 等既有面板定义互斥显示、暂停/恢复、Escape/关闭、鼠标焦点和 EventSystem 规则。任何可见按钮必须能点击并产生动作、禁用原因或明确的失败反馈；不能让不可见 Canvas 吞键盘/鼠标；
3. 不新增第二套游戏状态。HUD 只能读取现有控制模式、交互状态、操作结果和本地化文本；失败信息必须来自既有公开结果或可读原因，不能在 UI 里裁决金钱、船只、对话、存档或任务；
4. 发布只读交接接口 game/tools/results/D19/D19-R02-A01/D19_R02_A01_HUD_ROUTE_INTERFACE.json，至少列出公开调用方式、各 UI 模式的显示规则、A03 可调用而不得回写的入口、默认 Player 模式、开发模式和回退；
5. 在真实 1920×1080 Player 中验证：启动、移动、打开/关闭一项港口操作、暂停/恢复、鼠标/键盘切换和至少一个失败反馈。保存全屏截图/短帧、Canvas/焦点诊断和 EditMode/PlayMode 结果；运行 git diff --check。

通过门槛：
- 默认 Player 镜头无大面积叠字、任务阶段标签、资源/性能调试信息或互相遮挡的可操作面板；
- 同一时刻只有一个可获取 UI 焦点的面板，关闭/暂停/恢复不吞输入；
- 关键交互有可理解的成功、禁用或失败反馈，不能是死按钮；
- 生产逻辑、D22 主 UI 架构和正式场景均未被本任务改写。

结果必须保存到：
- 主成果：上述 HUD 协调代码/测试、docs/game/design/common/D19_R02_PLAYER_HUD_FOCUS_REPAIR.md、game/tools/results/D19/D19-R02-A01/D19_R02_A01_HUD_ROUTE_INTERFACE.json；
- 机器结果：game/tools/results/D19/D19-R02-A01/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须写明电脑独占是否实际完成、默认 Player HUD 是否已实际截图验证、可见/隐藏面板规则、未解决问题和结果路径。
```
</details>

## D19-R02-A02：NPC、登船与泊位交互可靠性及失败反馈修复

<details>
<summary>展开/复制：D19-R02-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 港口 D2 交互可靠性修复 Agent。请直接完成 D19-R02-A02，找出真实 Player 中 E 键 NPC 对话、码头/泊位、登船/下船没有状态反馈的根因，并在 D2 公开入口内修复。不要改 HUD 协调器、D3 市场/航程、InitialHarbor 或任何 Systems 裁决。

任务编号：D19-R02-A02（异步、D2 交互入口专项）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D19-S01 PASS；D19-R01-S02=PARTIAL。可与 D19-R02-A01/A04/A05 和 D18 R02 任务并行；可读取 A01 交接，但不得修改 A01 文件。
单一写者：game/Assets/Scripts/Gameplay/D2/、game/Assets/Tests/EditMode/D19Interaction/、game/Assets/Tests/PlayMode/D19Interaction/、game/tools/results/D19/D19-R02-A02/、D19 专属交互文档/交接/记忆日志。不得修改 RuntimeHudCoordinator、MainMenuController、D3 目录、InitialHarbor、D7/D17/Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：必须在真实 Unity Player 中以键鼠重复验证 NPC、泊位、登船/下船和失败反馈。开始前通知用户需要独占电脑并等待允许；没有真实 Player 独占，不得把静态/PlayMode 结果写成已修复。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D2/D7/D17 控制边界、D19-S01 稳定锚点、D19-R01-S02 路线/日志/截图、D2ControlRouter、现有 D2 交互测试、D19-R02-A01 的公开 HUD 接口（若已存在）。先建立从按键到 D2ControlRouter、距离/模式检查、公开动作调用、结果/提示的调用链，不得猜测场景对象是根因。

执行内容：
1. 用真实 Player 与可重复测试定位 E/提示交互失败的根因：输入到达、当前角色/船模式、交互距离、目标稳定 ID、控制权、冷却、公开动作返回值、提示显示必须逐层记录；
2. 只在 D2 表现/控制入口修复可复现问题，保持 D2ControlRouter 是上下船/对话请求的统一入口。不得创建绕过 D7/ShipSystem/NPC/SaveSystem 的第二条路径，也不得修改任何经济、关系、船航行或任务事实；
3. 为 NPC 对话、太远、目标不存在、当前模式不允许、船不可用、上船成功、下船成功分别提供可读结果/失败原因，供 A01 的 HUD 接口显示；不直接写 Canvas，不复制 UI 逻辑；
4. 新增/修复 EditMode 与 PlayMode 用例，至少覆盖十轮靠近/远离、NPC 对话、登船、下船、暂停/恢复、读档后的交互。使用 D19 稳定锚点和 D2 公开 API，不依赖屏幕固定坐标；
5. 在真实 Player 以键鼠完成 NPC 成功或明确失败、泊位状态、登船、下船的重复验证，保存屏幕证据、输入/状态日志和机器 JSON；运行 Unity 编译、聚焦测试和 git diff --check。

通过门槛：
- 每个关键交互都有从输入到公开返回结果的可复核证据；
- 成功会发生真实模式/状态转换，失败会给清楚反馈，不能无反应；
- D17 角色移动、落地、上下船、相机和读档兼容回归不下降；
- 没有修改 UI 写者、正式场景、D3、Systems 或存档所有权。

结果必须保存到：
- 主成果：D2 交互修复/测试、docs/game/design/common/D19_R02_INTERACTION_RELIABILITY.md、game/tools/results/D19/D19-R02-A02/D19_R02_A02_INTERACTION_ROUTE.json；
- 机器结果：game/tools/results/D19/D19-R02-A02/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须列出电脑独占、真实成功/失败次数、每个关键交互的实际结果、D2 修改边界和结果路径。
```
</details>

## D19-R02-A03：主菜单、市场、出航、保存与重载的真实路线修复

<details>
<summary>展开/复制：D19-R02-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 玩家主路线修复 Agent。请直接完成 D19-R02-A03：让真实 Player 从 Main Menu 新游戏进入 InitialHarbor，并能在既有 UI/公开系统入口中完成市场、出航/返港、保存和重载的可见闭环。不得重写 HUD、D2 交互、场景或 SaveSystem。

任务编号：D19-R02-A03（异步、菜单与 D3 路线表现专项）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D19-R02-A01 的 HUD route interface/交接存在；D19-R02-A02 已至少留下可读结果；D19-S01 PASS。A02 可以是 PARTIAL，但不得用聊天结果替代文件。不得与 D19-R02-S02 并行。
单一写者：game/Assets/Scripts/Gameplay/UI/MainMenuController.cs、game/Assets/Scripts/Gameplay/D3/、game/Assets/Tests/EditMode/D19Route/、game/Assets/Tests/PlayMode/D19Route/、game/tools/results/D19/D19-R02-A03/、D19 专属路线文档/交接/记忆日志。不得修改 RuntimeHudCoordinator、D2 目录、InitialHarbor、D5/D7/Systems、SaveSystem、Registry、存档 schema、contracts/server/web 或 Git 历史。

电脑独占规则：本任务必须通过真实 Unity Player 点击/键鼠完成菜单、新游戏、市场、出航/返港、保存和重载。开始前先通知用户需要独占电脑并等待允许；若用户未允许或 Player 无法启动，只交付静态诊断并如实 PARTIAL/BLOCKED。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D19-S01、D19-R01-S02 的完整失败路线、D19-R02-A01 HUD interface、D19-R02-A02 交接、MainMenuController、D3RuntimeBootstrap、D3/Save 的公开 API、D17/D2 上下船边界和现有 MainMenu/Save/D3 测试。先确认 New Game 的实际按钮/事件链、场景切换、D3 市场/航程入口和保存/加载回执，不得假定历史单元测试等于真实 Player 成功。

执行内容：
1. 修复可复现的菜单表现/调用问题，使 Main Menu 的 New Game → 角色确认 → InitialHarbor 实际切换可见；只调用 SaveSystem/GameManager/D3 的既有公开 API，不修改 Systems 或用延迟/假日志冒充场景切换；
2. 通过 A01 的只读 HUD 接口显示市场、航程、返港、保存/加载的状态与失败信息，确保面板关闭/焦点恢复后玩家仍能移动/交互；不得回写 A01 的协调器；
3. 在 D3 表现/Bootstrap 范围修复市场操作、发现/出航、返港和保存回执的可见触发链。所有价格、货物、世界日、航行结果和存档写入仍由已有 System/Save API 裁决，禁止双写或模拟成功；
4. 建立从 New Game 开始的可自动辅助、也可人工重复的路线脚本/日志格式，记录真实输入、稳定 ID、预期、实际状态变化和失败恢复；不得依赖屏幕绝对坐标或用直接加载 InitialHarbor 替代主菜单路径；
5. 在真实 Player 完成至少一轮：New Game → 进入港口 → NPC/明确失败 → 市场操作 → 泊位/登船 → 出航 → 返港 → 保存 → 重启/继续读档。保存真实截图/短帧、日志和测试 XML；运行 Unity 编译、聚焦 EditMode/PlayMode 和 git diff --check。

通过门槛：
- New Game 的真实按钮/确认链能从菜单进入 InitialHarbor，不能仅靠测试直接调用；
- 市场、出航/返港、保存和重载各有真实状态/反馈与证据；
- 面板焦点符合 A01 接口，不破坏 D2 交互或 D17 控制模式；
- 不修改 HUD、D2、场景、SaveSystem/存档 schema、经济或世界裁决。

结果必须保存到：
- 主成果：菜单/D3 路线表现修复与测试、docs/game/design/common/D19_R02_MAIN_ROUTE_REPAIR.md、game/tools/results/D19/D19-R02-A03/D19_R02_A03_ROUTE_INTERFACE.json；
- 机器结果：game/tools/results/D19/D19-R02-A03/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复列出电脑独占、真实完成的路线步骤、读档是否成功、仍失败的步骤、未修改的所有权边界和结果路径。
```
</details>

## D19-R02-A04：InitialHarbor 画面、性能与真实 Player 固定镜头修复

<details>
<summary>展开/复制：D19-R02-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 正式港口画面与性能修复 Agent。请直接完成 D19-R02-A04。你是本任务期间 InitialHarbor 的唯一场景写者：消除真实 Player 中的灰盒/方块/低细节主视觉、调试遮挡以外的场景问题和 1080p 性能超限，同时保留所有港口交互锚点与玩法事实。

任务编号：D19-R02-A04（异步、唯一 InitialHarbor 场景/性能写者）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D18-S02-R01 PASS、D19-S01 PASS、D19-R01-S02=PARTIAL；可读取 D19-R02-A01/A02 的结果，但不能等待或修改其代码。可与其它 R02 的非场景任务并行，但不得与 D19-R02-S02 或 D20-S02-R01 同时修改/运行正式场景。
单一写者：game/Assets/Scenes/InitialHarbor*、game/Assets/Prefabs/Environment/D19/Integration/R02/、game/Assets/Art/Materials/D19/R02/、game/Assets/Settings/D19/R02/、game/Assets/Editor/D19R02/、game/Assets/Tests/EditMode/D19R02Scene/、game/tools/results/D19/D19-R02-A04/、D19 专属场景文档/交接/记忆日志。不得修改 HUD/D2/D3/D20 目录、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务必须在真实 Unity Player 中截图和采样性能。开始前先通知用户需要独占电脑；确认后才启动 Unity。执行期间不允许其它 Agent 打开项目、运行 Test Runner 或操作 Player；结束时关闭 Player、退出 Play Mode并保存本任务改动。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D19-S01、D19-A01—A06、D19-R01-B、D19-R01-S02 的 10 张 Player 帧/性能原始数据/资源审计、最新 intake catalog、D18-S02-R01 资源边界、D20 尚未正式集成的边界与 D19 固定镜头规范。先建立场景 SHA、可回滚副本、17 个稳定游戏锚点和当前渲染/实体/材质/光源/透明层性能清单。

执行内容：
1. 只使用 intake_eligible 外部资源或项目自有且已有审计的表现资产，重构玩家可见的地形/海岸/道路/港口层次、码头/市场/建筑轮廓、植被/岩石/浅水、光照/天空/雾和环境细节；重点消除分面大平面、方块化建筑/道具、明显三角树、漂浮/穿插、空洞远景和丢失材质。不得用浓雾、相机裁切、关闭整个区域或删除玩法入口掩盖问题；
2. 保持/复核出生、NPC、市场、仓库、酒馆、船坞、泊位、登船、出航、返港、保存点的稳定 ID、位置迁移与可达性。场景只挂 Integration 根和 Prefab，不直接散放候选模型；D20 人物/动物正式接入仍等待 D20-S02-R01；
3. 以真实 1920×1080 Player 的剖析数据修性能：优先处理重复 Renderer/材质、阴影距离/级联、透明层、无 LOD 的远景、过量 Animator/音频、灯光和 GPU/CPU 尖峰；采用 LOD、合批、裁剪、质量档和真实降级，不能删除交互、存档、生态或系统事实。必须校正采样器的实际分辨率/显示分辨率、工作集与可得 GPU/CPU telemetry 的一致性；
4. 在真实 Player 重新采集至少 14 个 D19 固定镜头及晴天/阴雨/雾或风暴代表变体，附每镜头区域、可见资源、分辨率、画质档、时间戳和人工 QA。Editor Camera.Render 只能作辅助，不得当作 Player 通过证据；
5. 运行 Unity 编译、场景引用/锚点/碰撞/路线聚焦测试，按 Low/Medium/High 在 1920×1080 真 Player 中预采性能并保存原始帧时间/平均 FPS/1% low/GC/CPU/GPU 可得值。此任务不替代最终验收；只有 S02 能给 D19 PASS。

通过门槛：
- 14 个真实 Player 固定镜头不再有明显灰盒/方块岛、巨大分面占位面、粉色/丢失材质、严重穿插或相机裁切；
- 港口核心路线连续可走，所有稳定锚点和 D9/D17 交互入口仍可定位；
- Low/Medium/High 真实采样比 R01 有可复核改善，且每项性能/telemetry 不一致都被记录，不伪造达标；
- 场景引用可追溯、可回滚；无越权修改 HUD、D2/D3、D20 或强逻辑。

结果必须保存到：
- 主成果：InitialHarbor 的 R02 Integration/材质/设置/测试、docs/game/design/common/D19_R02_HARBOR_VISUAL_PERFORMANCE_REPAIR.md；
- 机器结果：game/tools/results/D19/D19-R02-A04/（含 rollback、screenshots/、performance/ 和机器 JSON）；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复列出电脑独占、场景起止 SHA、镜头数量、真实三档性能、改善/剩余问题和结果路径；不得把预采样写成 D19 最终 PASS。
```
</details>

## D19-R02-A05：D19 PlayMode 测试预构建与真实执行链修复

<details>
<summary>展开/复制：D19-R02-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 PlayMode 测试基础设施修复 Agent。请直接完成 D19-R02-A05，找出并修复 Unity Test Runner 卡在 Unity.PerformanceTesting.Editor.TestRunBuilder 预构建阶段、没有 XML 的根因。不要删除、跳过、隔离复制或降低 D19/D20 测试来伪造通过，也不要修改游戏场景/玩法代码。

任务编号：D19-R02-A05（异步、D19 PlayMode 测试执行链专项）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D19-R01-S02=PARTIAL，历史 PlayMode timeout 日志存在。可与 D19-R02-A01/A02/A04 和 D18 R02 任务并行；不等待 D20，但必须在每次运行前读取当前编译状态。
单一写者：game/Assets/Tests/PlayMode/D19/、game/Assets/Editor/D19S02/、game/Assets/Tests/PlayMode/D19R02/、game/Assets/Tests/PlayMode/D19R02*.asmdef、game/tools/results/D19/D19-R02-A05/、D19 专属测试文档/交接/记忆日志。不得修改 InitialHarbor、D19 Integration、D20 生产/测试文件、Systems、Registry、存档、contracts/server/web、Packages 配置或 Git 历史；若根因需要超出目录的依赖/生产修复，停止并报告精确阻断。

电脑独占规则：本任务需启动 Unity Test Runner/批处理 Unity。开始前明确通知用户需要独占电脑；获得许可后同一时间不得让其它 Agent 打开 Unity 或 Player。不得为了赶时间杀掉用户正在运行的 Unity、删除 Library 或覆盖用户未保存场景。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D19-R01-S02 的 timeout JSON/日志、D19 PlayMode 测试与 Editor runner、D20 现有 test asmdef/命名空间、Package manifest/lock（只读）、当前编译日志和 D17/D19 相关测试入口。先确认历史“WhalesIslandReef.Presentation 缺失”是否仍可在当前主项目复现；不能把陈旧报告当作当前编译事实。

执行内容：
1. 用主项目（不得靠删除 D20 测试的隔离副本）最小复现 D19 PlayMode timeout，保存启动命令、时间线、Editor.log/测试日志、预构建回调、测试过滤条件和 XML 是否产生；
2. 定位 PerformanceTesting prebuild、IPrebuildSetup、程序集引用、异步等待、资源初始化、测试 runner 生命周期或编译错误的真实根因。可以新增 D19 专属诊断/runner/测试辅助，但不得禁用 PerformanceTesting、忽略失败用例、删除测试、把 timeout 改成 pass 或仅运行空测试；
3. 在允许目录内做最小修复，使至少 D19 相关真实 PlayMode 用例完成并生成可解析 XML。若需要修生产代码，必须先输出精确故障报告和责任模块，不越权修改；
4. 复跑 D19 场景路线/锚点的 PlayMode 用例、至少一项 D17 兼容回归和相关 EditMode；记录通过/失败/跳过/超时计数、耗时和所有残余限制；
5. 输出可供 D19-R02-S02 直接复用的命令和机器可读 runner 健康 JSON；运行 git diff --check。

通过门槛：
- 主项目中真实 D19 PlayMode 测试不再停在 prebuild timeout，并实际生成 XML；
- 没有删除/跳过/重命名失败测试或使用隔离副本掩盖主项目问题；
- 当前 D20 引用/编译状态被如实记录，陈旧错误不被当作事实；
- 场景、强逻辑、资源状态和其它 Agent 文件未被越权修改。

结果必须保存到：
- 主成果：D19 专属 runner/测试修复、docs/game/design/common/D19_R02_PLAYMODE_RUNNER_REPAIR.md、game/tools/results/D19/D19-R02-A05/D19_R02_A05_PLAYMODE_HEALTH.json；
- 机器结果：game/tools/results/D19/D19-R02-A05/（含 XML、日志和复现命令）；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复写明电脑独占、主项目真实 PlayMode 计数、XML 路径、根因/剩余阻断和结果路径。
```
</details>

## D19-R02-S02：R02 修复后的正式 Player 场景验收与 D20 放行门禁

<details>
<summary>展开/复制：D19-R02-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 R02 最终场景验收与放行 Agent。请直接完成 D19-R02-S02。这是 R02 当前唯一可以把 D19 写为 PASS 并放行 D20-S02-R01 的任务。它必须用修复后的主项目、真实 Player、真实菜单路线和真实性能/PlayMode 证据，不能复用 R01 的失败数据、Editor 截图或隔离副本。

任务编号：D19-R02-S02（同步、唯一最终场景验收/放行任务）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D18-S02-R01 PASS；D19-R02-A01/A02/A03/A04/A05 的主成果、机器结果和交接都存在；D19-R02-A04 已停止修改 InitialHarbor。任何上游为 PARTIAL 都不自动阻止复验，但未解决 P0 问题必须导致本任务 PARTIAL/BLOCKED。不得与 D20-S02-R01 并行。
并发规则：你是验收期间 InitialHarbor、D19 Integration 和 D19 R02 最终验收文件的唯一写者。只可做小型、可回滚的验收修正；如果需要大范围改场景/代码，写 PARTIAL 并精确回交到对应 A 任务。

电脑独占规则：本任务必须真实操作 Main Menu 和 Unity Player、重启/读档、采集三档性能与 14+ Player 镜头。开始前先告知用户需要独占电脑，只有收到允许后才能启动；执行期间用户和其它 Agent 不得操作键鼠、Unity、Player 或同一项目。未获得独占/未完成真实 Player 时严禁 PASS。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D19-S01、D19-A01—A06、D19-R01-S02 全部失败证据、D19-R02-A01—A05 全部成果、D18 最新 intake catalog、D17/D16 回归入口、当前 InitialHarbor SHA 与回滚点。建立逐项验收清单，保留全部历史 PARTIAL/BLOCKED 报告。

执行内容：
1. 在主项目审计正式场景的外部资源引用，确认每项均在最新 intake catalog 中为 intake_eligible，且能定位到 Prefab、区域和 manifest；不把 D18-S03/LocalDevelopmentReady 提前写为通过；
2. 从真实 Main Menu 开始，在 1920×1080、16:9 Player 中连续完成至少 300 秒路线：新游戏/角色确认 → InitialHarbor 出生落地 → 移动 → NPC 对话或真实可解释失败 → 市场真实操作 → 码头/泊位 → 登船 → 出航 → 返港 → 保存 → 退出/重启 → 继续读档。每一步记录时间、输入、预期、实际状态、失败恢复；不得以直接加载场景或脚本假输入替代；
3. 在同一 Player 采集 14 个固定镜头及不少于 3 个天气/时间代表变体，检查默认 Player HUD 无叠字/调试层，港口无明显灰盒/方块岛/粉色材质/严重穿插/相机裁切；每张带时间戳、质量档、分辨率和区域；
4. 在真实 1920×1080 Player 中按 Low/Medium/High 各完成 warm-up 和固定路线/驻留采样，保存原始帧时间、平均 FPS、1% low、p95、GC、可得 CPU/GPU/工作集、实际/显示分辨率、质量档和 VSync。采样器不一致或 GPU 不可得时必须明确记录；性能必须满足本路线的目标，不能靠删玩法/隐藏区域/停系统掩盖；
5. 使用 D19-R02-A05 的主项目 runner 复跑 D19 PlayMode XML、D19 EditMode、D17 关键移动/落地/登船/相机和保存回归。任何编译、测试、路线或性能失败必须精确归档，不得删除测试或改结论；
6. 写新的 R02 最终验收报告，明确它更新“当前 D19 门禁状态”而不删除 R01。只有所有 P0 条件有真实证据才写 PASS；PASS 时明确放行 D20-S02-R01，否则明确不放行。

通过门槛：
- 真实菜单起步的 300 秒以上路线完整，NPC/市场/登船/出航/返港/保存重载无必现关键卡死；
- 14+ 真 Player 镜头和 3+ 天气/时间变体达到样板港视觉/HUD 门槛；
- 1920×1080 Low/Medium/High 性能及分辨率/telemetry 真实可复核，并满足当前路线性能预算；
- 主项目 D19 PlayMode 有有效 XML，D19 EditMode/D17/保存回归通过；
- 场景资源准入、回滚、结果、截图、日志和失败清单完整。

结果必须保存到：
- 主成果：docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE_R02_S02.md、必要的最小验收修正/测试；
- 机器结果：game/tools/results/D19/D19-R02-S02/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-R02-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须写明电脑独占、是否放行 D20-S02-R01、完整路线秒数、各档性能、Player 镜头数、测试计数、未解决问题和结果路径。
```
</details>

## D19-A01：岛屿地形、海岸、暗礁、洞穴和可行走面

<details>
<summary>展开/复制：D19-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 Unity 地形 Agent。请直接完成 D19-A01。

任务编号：D19-A01（异步场景模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_Terrain_Work.unity、game/Assets/Prefabs/Environment/D19/Terrain/、相关材质/测试和本任务文档。禁止修改正式 InitialHarbor。

开始前读取：通用约束、D19 世界搭建规范、D18 视觉/资源清单、islands 的地质/地形/放置设计、environment 的水体/性能设计、当前角色控制/落地/碰撞/NavMesh 代码和 D17 物理回归。

执行内容：
- 按冻结轮廓制作真实起伏的岛体，包含港口缓坡、海滩、岩岸、内陆高点、道路基底、浅滩/暗礁轮廓和一个可封闭或进入的洞穴/岩缝样板；
- 使用地形或模块网格实现沙、土、草、岩过渡，消除整块纯白平面和垂直方块岛；
- 设置海岸高度、海床、航道深度、角色可达范围、安全阻挡、跌落恢复点和船舶不可穿透的代表碰撞；
- 生成或配置可行走面/NavMesh（按项目现有方案），确保坡度、台阶和洞口不会让角色漂浮/卡死；
- 制作独立 TerrainRoot Prefab/工作场景和集成说明，所有位置锚点与 D19-S01 坐标一致；
- 增加 PlayMode/EditMode 检查：出生落地、关键路线可达、市场/码头锚点未被地形遮挡、暗礁碰撞、越界恢复。

硬约束：工作场景/Prefab 制作阶段只能使用 `d18_prefab_candidate_intake_catalog.json` 中 `intake_eligible=true` 的外部资源；场景只引用 Prefab；不改角色移动公式、Navigation/Ship 强逻辑、InitialHarbor、主 Bootstrap、UI、Registry/存档；不为视觉效果制造不可见逻辑传送；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译、相关测试、工作场景截图（至少地形全景/海岸/道路/洞口/暗礁 5 张）、性能粗测和 git diff --check。记录多边形、Terrain 分辨率、碰撞体和 NavMesh 数据。

结果必须保存到：
- 主成果：上述 D19 Terrain 工作场景/Prefab/材质/测试、docs/game/design/common/D19_TERRAIN_COAST_NAVIGATION.md、game/tools/results/D19/D19-A01/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

不能证明关键路线和落地通过时写 PARTIAL/BLOCKED，不能只凭截图 PASS。
```
</details>

## D19-A02：港口、码头、船坞、市场与建筑模块

<details>
<summary>展开/复制：D19-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的港口建筑 Agent。请直接完成 D19-A02。

任务编号：D19-A02（异步 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_PortArchitecture_Work.unity、game/Assets/Prefabs/Buildings/D19/、game/Assets/Prefabs/Environment/D19/Port/、相关测试/文档。禁止改正式 InitialHarbor。

开始前读取：通用约束、D19 世界规范、D18 PrefabCandidateIntake 准入目录、buildings/shipyard/items 设计、D9 市场/泊位/登船流程和当前交互锚点实现。

执行内容：
- 搭建具有 1675 年加勒比港口辨识度的模块化样板：主码头、次级栈桥、泊位、系船柱、船坞/修理区、市场摊位、仓库、酒馆外观、管理建筑、普通民居和必要围栏/阶梯；
- 保留并可视化市场、仓库、酒馆/NPC、船坞、泊位和出航等现有交互锚点；锚点名称/ID 与 S01 迁移表一致；
- 建筑入口、门、台阶、栏杆、码头边缘和甲板高度必须与角色尺度匹配并有实际碰撞；
- 制作模块化 Prefab、LOD/合批建议、材质复用，避免把整个港口做成一个不可维护网格；
- 工作场景中检查角色路线、相机遮挡、登船视线和船靠泊空间；
- 添加 Prefab 完整性/缺失引用/碰撞/交互锚点测试和截图。

硬约束：工作场景/Prefab 制作阶段只能使用 `d18_prefab_candidate_intake_catalog.json` 中 `intake_eligible=true` 的外部资源；不改交易/船坞/经济强逻辑，不在模型上直接写库存或价格；不改 InitialHarbor、主 Bootstrap、地形/水体文件、UI/Registry/存档；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译与测试；至少输出港口全景、市场、泊位、船坞、酒馆街道、仓库 6 张截图；记录 Prefab 数、材质数、LOD/批处理、碰撞和锚点检查；git diff --check。

结果必须保存到：
- 主成果：上述工作场景/Prefab/测试、docs/game/design/common/D19_PORT_ARCHITECTURE.md、game/tools/results/D19/D19-A02/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告必须列出交互锚点保持情况、已用资源 manifest ID、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D19-A03：植被、岩石、生境与风动布置

<details>
<summary>展开/复制：D19-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的植被与生态表现 Agent。请直接完成 D19-A03。

任务编号：D19-A03（异步 Prefab/布置任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_Biomes_Work.unity、game/Assets/Prefabs/Vegetation/D19/、相关 D19 植被材质/测试/文档。禁止改 InitialHarbor 和 A01 地形工作场景。

开始前读取：D19 世界规范、D18 可用环境资源、vegetation 全套设计、islands 生境/放置规则、D15 植被目录和当前 VegetationSystem/采集点边界。

执行内容：
- 建立海滩、港口扰动区、道路边缘、次生林、岩坡和湿润低地等代表生境 Prefab/规则；
- 用棕榈、阔叶树、灌木、草、藤本、枯木、岩石组合消除孤立三角树和随机散点感；
- 实现风动材质参数、颜色/尺度受控变体、LODGroup、远距裁剪或实例化方案；
- 植被不得穿入道路、码头、建筑、交互点、角色出生点和航道；
- 代表性采集植物要有清晰视觉与稳定 content_id 适配锚点，但本任务不改采集数值；
- 制作可由 S02 贴合 A01 地形的 BiomeRoot Prefab/布置数据，添加密度/碰撞/穿插/LOD 测试和截图。

硬约束：表现层不写资源数量、刷新和生态裁决；不随机生成不可复现结果，种子和规则必须记录；工作场景/Prefab 制作阶段只能使用准入目录中的 `intake_eligible` 外部资源；不改 InitialHarbor、Terrain、建筑、水、Systems/Registry/存档；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译与测试；至少 6 张生境/风动/远景截图；记录实例数、批次数、材质数、LOD 距离和性能样本；检查关键路线零严重穿插；git diff --check。

结果必须保存到：
- 主成果：上述工作场景/Prefab/材质/规则/测试、docs/game/design/common/D19_VEGETATION_BIOMES.md、game/tools/results/D19/D19-A03/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出使用资源、规则种子、实例/性能、穿插检查和 PASS/PARTIAL/BLOCKED。
```
</details>

## D19-A04：海水、岸线、浅水、暗礁与水下表现

<details>
<summary>展开/复制：D19-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的水体技术美术 Agent。请直接完成 D19-A04。

任务编号：D19-A04（异步水体模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_Water_Work.unity、game/Assets/Prefabs/Environment/D19/Water/、D19 水体 Shader/材质/测试/文档。禁止改正式 InitialHarbor。

开始前读取：D19 世界规范、D18 视觉与材质资源、environment 水体/天气/性能设计、ship 浮力/锚泊边界、当前海平面、航行与相机实现。

执行内容：
- 建立与项目当前 URP 兼容的海面方案，至少提供深海/浅水颜色、透明/深度控制、法线或波纹、岸线浪花/湿边、暗礁可见性、远景和水下色调；
- 水体参数与海平面、天气/风接口分离，允许 D21 接入风浪，不把天气裁决写进 Shader；
- 建立 WaterRoot Prefab、材质实例和代表海岸测试场景；
- 处理角色/船/相机穿水时的视觉边界，避免整屏异常透明、排序闪烁和 UI 被后处理影响；
- 提供低/中/高三档方案，记录透明、反射、深度纹理和移动端/低档降级；
- 添加材质/Shader 编译、海平面一致、相机上下水、岸线和性能测试/截图。

硬约束：不实现或改变 ShipSystem 浮力公式、潮汐/天气强逻辑；不改 Terrain/建筑/植被工作文件、InitialHarbor、主相机代码、Registry/存档；不引入未审核渲染插件；外部资源只能使用准入目录中的 `intake_eligible` 条目；海水/Shader 等项目自有表现须记录所有权；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 无 Shader/编译错误；至少晴天、浅水、暗礁、岸线、水下、低画质 6 张截图；记录 GPU/帧时间粗测和三档差异；git diff --check。

结果必须保存到：
- 主成果：上述水体工作场景/Prefab/Shader/材质/测试、docs/game/design/common/D19_WATER_SHORE_UNDERWATER.md、game/tools/results/D19/D19-A04/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

不能在目标 URP 编译或岸线/透明有严重错误时必须 PARTIAL/BLOCKED。
```
</details>

## D19-A05：光照、天空、时间、天气视觉与后处理

<details>
<summary>展开/复制：D19-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的灯光与天气表现 Agent。请直接完成 D19-A05。

任务编号：D19-A05（异步视觉配置任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_Lighting_Work.unity、game/Assets/Settings/D19/Lighting/、game/Assets/Prefabs/Environment/D19/Lighting/、测试/文档。禁止改正式 InitialHarbor。

开始前读取：D18 标准/合法 HDRI 或天空资源、D19 世界规范、environment 光照/天气设计、当前 WeatherSystem/Time 数据、URP/Volume 配置、D16 可访问性和性能目标。

执行内容：
- 建立晴天、阴天/雨、黄昏/夜间、雾、风暴五类代表视觉 Profile，并映射现有天气/时间只读状态；
- 设置主光、环境光、天空、雾、阴影、色调映射、曝光、颜色调整和可选后处理；
- 保证玩家、NPC、建筑入口、码头边缘、海面和交互提示在各状态可读；
- 夜间提供时代合理的局部灯光接口，不把现代霓虹或电灯风格引入港口；
- 提供低/中/高三档阴影、雾、后处理质量；禁止强制景深、运动模糊或高闪烁；
- 制作 LightingRoot/Profiles 和适配器，表现层只读 Weather/Time，不修改天气概率或时间推进；
- 添加状态映射、Profile 完整性、曝光范围和截图测试。

硬约束：不改 WeatherSystem/TimeSystem 逻辑，不在 Systems 加渲染代码；不修改水 Shader、正式场景、主相机输入、UI/Registry/存档；外部资源只能使用准入目录中的 `intake_eligible` 条目；项目自有灯光/天气表现记录所有权；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity/Shader 编译、5 状态 × 昼夜代表截图、关键区域可读性检查、三档性能样本、相关测试和 git diff --check。

结果必须保存到：
- 主成果：上述工作场景/Profiles/适配器/测试、docs/game/design/common/D19_LIGHTING_WEATHER_PRESENTATION.md、game/tools/results/D19/D19-A05/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出 Profile、状态映射、截图、性能、可访问性问题和 PASS/PARTIAL/BLOCKED。
```
</details>

## D19-A06：港口道具、VFX 与环境音景

<details>
<summary>展开/复制：D19-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的环境叙事、VFX 与音景 Agent。请直接完成 D19-A06。

任务编号：D19-A06（异步表现模块任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 PASS。
单一写者：game/Assets/Scenes/D19/Work/D19_Ambience_Work.unity、game/Assets/Prefabs/Props/D19/、game/Assets/Prefabs/VFX/D19/、game/Assets/Audio/D19/、测试/文档。禁止改正式 InitialHarbor。

开始前读取：D18 可用道具/UI音频资源、D19 世界规范、items/audio/buildings 设计、当前 AudioMixer/天气/交互事件接口和性能规范。

执行内容：
- 用木桶、货箱、渔网、绳索、灯、招牌、工具、货物堆、市场陈列和修船物件建立港口生活感；
- 建立可复用的烟、尘、火光、昆虫/海鸟远景、轻浪溅水等轻量 VFX；
- 建立港口、市场、海岸、船坞和夜间的环境音区，包含海浪、风、木结构、远处人群、鸟和代表交互音；
- 音频必须走现有 Mixer/音量设置，支持静音/字幕或视觉替代所需事件标识，不直接播放无法控制的 AudioSource；
- 道具布局不得挡住关键路线/交互，VFX 不得遮挡 UI 或造成持续过度闪烁；
- 制作 AmbienceRoot/PropSet Prefab、对象池/距离衰减方案、完整性测试和截图/音频清单。

硬约束：工作场景/Prefab 制作阶段只能使用准入目录中的 `intake_eligible` 外部资源；不修改经济库存、任务事件、动物生成或天气强逻辑；不改 InitialHarbor、Terrain/建筑/水体文件、Registry/存档；不新增网络；不得标记 LocalDevelopmentReady，正式运行时晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译与测试；至少 5 个区域截图、音频事件/循环/响度清单、关键路线碰撞检查和性能样本；git diff --check。

结果必须保存到：
- 主成果：上述工作场景/Prefab/VFX/Audio/测试、docs/game/design/common/D19_PORT_PROPS_AMBIENCE.md、game/tools/results/D19/D19-A06/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出资源来源 ID、区域、音频/VFX 数、性能、路线检查和 PASS/PARTIAL/BLOCKED。
```
</details>

## D19-S02：正式场景串行集成与样板岛验收

<details>
<summary>展开/复制：D19-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D19 正式场景集成与验收 Agent。请直接完成 D19-S02。

任务编号：D19-S02（同步集成任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D19-S01 和 D19-A01—A06 全部有交接与可集成成果；D18-S02-v2 保持 PASS；D18-S03 不作为本任务的前置，须在 D19—D22 正式集成后统一执行。
并发规则：执行期间你是 InitialHarbor、主场景层级和 D19 集成 Prefab 的唯一写者；暂停其他 Agent 对这些文件的修改。

开始前读取：全部 D19 主成果/工作场景/交接/机器结果、D18 标准和可用资源清单、当前正式 InitialHarbor 及 D9/D17/D16 回归入口。先记录场景备份策略和现有交互点清单。

执行内容：
1. 按 S01 顺序把 Terrain、港口建筑、植被生境、水体、灯光天气、道具/VFX/音景作为 Prefab/独立根节点串行接入正式 InitialHarbor；
2. 移除或默认隐藏被正式资源替代的白色方块地面、三角树和重复灰盒，但保留可回滚记录，不删除仍被测试引用的兼容资产；
3. 迁移并验证玩家出生、市场、NPC、仓库、船坞、泊位、登船、出航、返港、保存点等稳定交互 ID；
4. 检查碰撞、NavMesh、角色落地、相机遮挡、船航道、海平面、UI 可读性和天气切换；
5. 玩家模式默认关闭开发叠字，但 D22 之前不得擅自重构整个 UI；
6. 运行现有完整 PlayMode 回归和 D19 新增测试，完成固定 12+ 镜头截图、5—10 分钟手动路线和性能记录；
7. 对任何无响应交互、路线卡死、穿模、丢引用或明显视觉回退建立阻断清单，不能用截图掩盖。

硬约束：不改 Systems 强逻辑、经济公式、存档 Schema、contracts/server/web；场景只引用 Prefab；只使用已通过 D18-S02-v2 准入目录的资源或项目自有表现资产；不把资源标记 LocalDevelopmentReady/ReleaseApproved；保留旧测试兼容，并为 D18-S03 保留可定位消费者证据；不覆盖无关改动，不提交或推送 Git。

通过门槛：
- 岛不再是单一方块/白平面，港口具备地形、海岸、道路、码头、建筑、植被、水体、灯光和音景；
- 关键路线、出生、市场、登船、出航、返港可用，零必现卡死；
- 12+ 固定镜头无严重缺材质、穿插、纯色占位或相机裁切；
- Unity Console 无新增错误，相关测试和既有核心 PlayMode 回归通过；
- 1080p 性能有真实报告，超预算项有后续责任任务；
- 结果路径、资源 manifest ID、场景变更和回滚方式可复核。

结果必须保存到：
- 主成果：正式 InitialHarbor 场景及 D19 集成 Prefab/测试、docs/game/design/common/D19_SAINT_MARGARET_ACCEPTANCE.md、game/tools/results/D19/D19-S02/；
- 交接报告：docs/game/design/common/handoffs/D19/D19-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。只有 PASS 才表示 D19 完成；交接报告必须列出测试计数、截图路径、性能、已知问题和实际修改文件。
```
</details>
