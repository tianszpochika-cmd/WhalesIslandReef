# D25—D31 视觉与交互开发工作文档

> 版本：v2.0  
> 当前路线：`D24 纯逻辑候选版` → `D25—D31 视觉 Runtime 候选版`  
> 当前目标：先把 `InitialHarbor` 做成一个统一、可读、可交互的视觉垂直切片，再扩展到更多岛屿和资源。

> 本轮执行快照（2026-09-03）：D25—D31 的表现代码、资源审计和交互桥已全部建立；已修复 D27 等待 D2 玩家实例化的时序缺陷、D29 临时面板互斥/酒馆关闭恢复、D9/D4 临时页的关闭恢复与 HUD 互斥、D9 打开时 D3/D29 `B` 快捷键串层问题、上船后相机把受控船体碰撞体误判为外部遮挡的问题，以及 R24 暴露的市场拒绝提前开空页、仓库缺失静默返回和 D30 失败阶段误报。R25 又修复港内市场/酒馆/船坞表现阶段被下一帧错误重置为“港口”的状态投影问题。R27 修复 R03 港口表现生成器名义支持倒角但实际返回硬盒体的问题，重新生成了倒角表现网格；Editor 审计得到 315 个 Renderer、15,609 个网格顶点和 1 个 R03 路线碰撞体。R28 修复 D31 固定镜头探针被 `CameraFollow` 每帧覆盖的问题，R29 又把旧世界坐标替换为当前 D19-R03 港口坐标系，并在独立 Player 中重新捕获 14/14 固定镜头和 3/3 天气镜头，共 17 张 1920×1080 backbuffer PNG。R30 修复 D29 对 D10/D4/D9 owner 返回值缺失导致的无条件成功提示，并为 D30 增加酒馆、船坞、整备失败事件的阶段保持。Unity fresh compile/domain reload 通过；独立 Windows Player R6/R8/R11/R13/R16/R18/R21/R22/R23/R29 构建成功。R16 已完成 D29/D4/D9 可见入口运行切片和 D30 出航→返港→保存→Continue 路线，R18 验证船控相机自碰撞修复，R19/R20/R24/R25/R28/R29/R30 补齐 D29/D29-Warehouse/D4/D9/D10 的代码级焦点、owner 未就绪反馈、失败路径反馈、港内表现阶段保持、固定镜头证据归属和 owner 结果传播，R14/R22/R23/R29 已捕获固定镜头和 Calm/Rain/Storm 三天气变体；R23 将 D30 路线条移入标题安全区。R26 仅做额度友好的逻辑复核，R27 补充视觉生成器静态回归，R29 完成当前布局 Player 复核，R30 完成代码层结果传播复核：选定 D25—D31 回归 `54/54 PASS`、D31 交互静态审计 `PASS`、D29/D30 矩阵审计 `PASS`、相关结果 JSON `58/58` 可解析、`git diff --check` 通过。R29 证明了固定镜头和天气文件真实落盘，R30 证明了 owner 结果不会在 D29 层被伪装成成功，但这两者都不等于画面质量门禁通过。逐入口负向/禁用/重复点击、D30 异常恢复、最终低模画面质量和性能 1% low 仍按证据门禁保持 `PARTIAL`；天气可读性和路线条遮挡缺陷已得到 R22/R23 的具体修补，R27/R29 已证明表现网格进入 Player，不能将纵向切片误报为 Visual Candidate 完成。

> R30 最新校验覆盖：由于本轮新增 R30 机器审计文件，当前 D25—D31 结果 JSON 总数为 `59/59` 可解析；D31 交互代码层为 `STATIC_LOGIC_ONLY / PASS`，D31 汇合门禁仍为 `PARTIAL`。

> R31 最新校验覆盖：由于本轮新增 R31 机器审计文件，当前 D25—D31 结果 JSON 总数为 `60/60` 可解析；D29 天气/画质入口已具备 owner 缺失反馈，D31 交互代码层仍为 `STATIC_LOGIC_ONLY / PASS`，D31 汇合门禁仍为 `PARTIAL`。

> R32 最新校验覆盖：由于本轮新增 R32 机器审计文件，当前 D25—D31 直接结果 JSON 总数为 `61/61` 可解析（Unity Library 夹具 JSON 不计入）；D29 已具备 owner 驱动的 `Button.interactable`、一次性请求防重，D30 已具备整备成功投影与重复事件保持；D31 交互代码层为 `STATIC_LOGIC_ONLY / PASS`，D31 汇合门禁仍为 `PARTIAL`。

> R33 最新校验覆盖：由于本轮新增 R33 机器审计文件，当前 D25—D31 直接结果 JSON 总数为 `62/62` 可解析（Unity Library 夹具 JSON 不计入）；D29 已补齐首帧 `Button.interactable` 刷新，以及酒馆 owner 拒绝后的临时页关闭/主 HUD 回退；D31 交互代码层为 `STATIC_LOGIC_ONLY / PASS`，选定回归为 `58/58 PASS`，D31 汇合门禁仍为 `PARTIAL`。

> R34 最新校验覆盖：由于本轮新增 R34 机器审计文件，当前 D25—D31 直接结果 JSON 总数为 `63/63` 可解析（Unity Library 夹具 JSON 不计入）；D29 已补齐登船、市场、酒馆、船坞和整备入口的异常恢复与 D30 失败事件投影；D31 交互代码层为 `STATIC_LOGIC_ONLY / PASS`，选定回归为 `59/59 PASS`，D31 汇合门禁仍为 `PARTIAL`。

> R36 最新校验覆盖：D29 保存 owner 拒绝与异常现在统一投影 `save_failed`，整备焦点拒绝会关闭临时页并恢复主 HUD，D30 对保存失败保持当前视觉阶段；D31 交互静态审计 `PASS`，定向回归 `25/25 PASS`，Unity 短编译成功。保存成功不会发送未定义的 D30 成功事件。D31-S05/G06 仍按真实 Player 证据要求保持 `PARTIAL`。

> R37 最新校验覆盖：D30 阶段事件投影已抽到零 Unity 依赖的 `D30VisualStagePolicy` 程序集；成功、失败、未知、重复事件和防重窗口由 Unity EditMode `16/16 PASS` 覆盖，D25—D31 Python 选定回归 `53/53 PASS`，Unity Tundra 短编译成功。该结果只提升代码层可验证性，不关闭真实 Player 负向交互、最终画质或性能门禁；D29-G06、D30-G06、D31-G06 继续 `PARTIAL`。

> R38 最新校验覆盖：D29 一次性请求防重已抽到零 Unity 依赖的 `D29OneShotPolicy` 程序集，并与 D30 阶段投影一起由 Unity EditMode `21/21 PASS` 覆盖；D25—D31 Python 选定回归 `53/53 PASS`，Unity Tundra 短编译成功。该结果仍只提升代码层可验证性，不关闭真实 Player 负向交互、最终画质或性能门禁；D29-G06、D30-G06、D31-G06 继续 `PARTIAL`。

> R43 最新校验覆盖：短时独立 Player UGUI 探针找到 `47/47` 控件，`46` 个观察到（含重复保护 `1`），保存入口 `1` 个仅做存在性检查，`0` missing/disabled/error/no-visible-feedback；仓库关闭后主 HUD 恢复已确认。D31 静态审计 `PASS`、D25—D31 选定回归 `54/54 PASS`、Unity EditMode `21/21 PASS`、Unity 编译 `error CS=0`；因 `pointerClick` 非物理 OS 输入，D29-G06/D30-G06/D31-G06 仍为 `PARTIAL`。

> R53 最新校验覆盖：R03 `MakeRockMesh` 已从六顶点三角锥改为 8 段三环不规则闭合轮廓；相关回归 `50/50 PASS`，Unity Editor 审计为 `307 Renderer / 24884 mesh vertices / 1 Collider`，真实独立 Player 复验为 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，均为 `1920×1080` backbuffer。R53 只关闭岩石轮廓这一项具体视觉缺陷，D31-P04/S05/G06 仍为 `PARTIAL`；低模/程序化建筑、物理输入矩阵、D30 实机异常恢复和性能 1% low 继续后置。

> R57 最新校验覆盖：R54 调校港口天空盒地平线；R55 增加山墙屋顶、地基、立面木构与侧窗板；R56/R57 修复 TextMesh 招牌镜像穿墙并增加摄像机朝向可见性门控。选定视觉/交互静态回归 `55/55 PASS`，Unity Editor 审计为 `367 Renderer / 32828 mesh vertices / 1 Collider`，Prefab 空序列化字段 `0`，真实独立 Player 复验为 `14/14` 固定镜头和 `3/3` 天气镜头，均为 `1920×1080` backbuffer。R57 仅为局部视觉修复，D31-P04/S05/G06 仍为 `PARTIAL`；整体低模质量、物理输入矩阵、D30 实机异常恢复和性能 1% low 继续后置。

> R58 最新校验覆盖：D29 仓库按钮和键盘 `I` 已绑定 D3 owner readiness；owner 未就绪时不再打开空仓库模态，已打开仓库仍可关闭。定向回归 `30/30 PASS`，D25—D31 选定回归 `56/56 PASS`，D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`，Unity Tundra 短编译成功且 `error CS=0`。R58 只关闭代码层空模态缺口，不替代真实 Player 物理输入、D30 异常恢复、最终画质或性能门禁。

> R59 最新校验覆盖：D30 阶段策略现在只把已知成功/失败事件纳入短时防重；未知或空事件持续返回 `Unknown`，不会被误报为 `Duplicate`。Python 定向回归 `38/38 PASS`，Unity EditMode `24/24 PASS`，D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`，Unity 编译/测试退出码 `0`。R59 仍不替代真实 Player 物理输入、D30 实机异常恢复、最终画质或性能门禁。

> R60 最新校验覆盖：D28 天气表现 owner 只有在 `Start` 完成后才报告 `IsReady`；D29 天气按钮和键盘入口使用同一就绪边界，避免初始化期间先显示成功、随后被 Calm 初始化覆盖。Python 定向回归 `40/40 PASS`，D25—D31 选定回归 `58/58 PASS`，Unity EditMode `24/24 PASS`，D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`，Unity 编译/测试退出码 `0`。R60 仍不替代真实 Player 物理输入、D30 实机异常恢复、最终画质或性能门禁。

> R61 最新校验覆盖：全量 Python 回归发现 D35 材质审计仍按 R35 旧基线要求 12/12 材质有纹理、315 Renderer；R61 已将 R52 明确的 R03_Plaster authored-color 例外和当前 367 Renderer / 32828 mesh vertices / 1 Collider 基线写入审计契约。当前材质审计 PASS，11 个材质有纹理、1 个灰泥纯色例外、6/6 法线元数据通过；D25—D31 正式运行门禁仍保持 PARTIAL。

> R62 最新校验覆盖：R03 岛面由方形坐标映射为带确定性海岸起伏的有机椭圆轮廓，同一静态 Mesh 加入浅岸线裙边，现有 `Water` 表现扩展为 52×52 环岛水面；Renderer/Collider 保持 `367/1`，网格顶点为 `33084`。Unity Editor 构建退出码 `0`，相关定向测试 `16/16 PASS`；这只关闭表现几何层缺陷，D31 正式 Player 画质/输入/异常恢复/性能门禁仍为 PARTIAL。

> R71 最新校验覆盖：D29 的 `Q` 画质预览快捷入口已登记到交互矩阵并在主 HUD 明示；当前矩阵为 `29` 个可见入口、`5` 个键盘入口。Python 定向回归 `60/60 PASS`，D29 矩阵审计、D31 交互审计和 HUD 布局审计均为 `PASS / STATIC_LOGIC_ONLY`，Unity EditMode `33/33 PASS`、`error CS=0`。本轮只补齐代码/文档覆盖，不升级 D29-G06、D30-G06 或 D31-G06。

## 0. 这份文档怎么用

本文件是 D25—D31 的执行入口。Agent 接到任务编号后，必须：

1. 先读 `docs/CONTEXT.md`、本文件、任务所属模块文档和 `docs/game/design/common/D24_NEXT_RUNTIME_PHASE_HANDOFF.md`；
2. 只修改任务卡中的写入范围；并行任务不得修改 `game/Assets/Scenes/InitialHarbor.unity`；
3. 完成代码、Prefab、材质或 UI 后，写结果 JSON、交接报告和当日日志；
4. 运行任务卡规定的验证；没有真实证据就写 `PARTIAL` 或 `BLOCKED`，禁止伪造 `PASS`；
5. 执行 `python tools/sync_visual_task_status.py`（若脚本尚未存在，先由 D25-S01 建立）；
6. 将结果记录到 [D25—D31 状态看板](VISUAL_TASK_STATUS_D25_D31.md)。

本路线不重开旧的 `D19-Rxx`、`D20-Axx` 任务。旧结果只可作为现状参考；新的视觉证据必须使用新的 D25—D31 编号。

## 1. 阶段目标和完成定义

### 1.1 D25—D31 的 P0 视觉垂直切片

完成后必须能在当前 Unity 版本中观察到：

- 圣玛格丽特港的地形、海岸、道路、港口、市场、仓库、酒馆和船坞具有连续的空间层次；
- 玩家、至少 8 类岗位 NPC、至少 6 类陆生/鸟类/水生动物有稳定 Prefab、比例、锚点、LOD 和表现回退；
- 至少一艘船具备船体、甲板、桅杆、帆、舵、锚和靠泊位置的可读表现；
- Calm、Rain、Storm 三种天气拥有可观察的天空、海水、光照、雾、雨/浪反馈；
- 玩家模式不显示 GameObject 名称、ID、FPS、事件哈希、调试路线和内部状态叠字；
- HUD、市场、仓库、酒馆、船坞、角色和保存面板互斥显示，按钮有成功/失败/禁用反馈；
- 新游戏 → 港口 → 交易 → 登船 → 出航 → 返港 → 保存 → 重载的关键状态，画面与逻辑读模型一致；
- 真实 Player 固定镜头、路线、天气和 UI 检查有独立证据。

### 1.2 不属于本阶段的内容

本阶段不把以下内容写成完成：D16 外部设备门禁、Steam 发布、跨平台发布、网络/LLM 正式接入、50 个岛全部完成、2000 个道具全部建模、最终商业资源授权。它们仍由各自门禁和后续扩量路线负责。

## 2. 状态语义

| 状态 | 含义 |
|---|---|
| `PLANNED` | 已登记，尚未开始 |
| `IN_PROGRESS` | 正在实现，不能被下游当作完成 |
| `ART_PASS` | 资源/材质/Prefab 本身通过静态审查 |
| `SCENE_PASS` | 已由唯一集成者接入正式场景 |
| `RUNTIME_PASS` | 当前 Player 中有可复核的真实运行过程 |
| `PARTIAL` | 有真实产物，但至少一个验收条件未满足 |
| `BLOCKED` | 缺少前置、资源、工具或权限，无法安全继续 |
| `VISUAL_CANDIDATE` | D25—D31 P0 视觉目标完成；不等同 `RuntimeApproved` 或 `ReleaseApproved` |

### 2.1 本轮已落地的代码范围

- D25：共享视觉主题、港口大气默认值、玩家 HUD、旧调试层隔离和资源准入审计。
- D26：R03 港口表现层唯一运行时恢复器，静态确认 307 个 Renderer/材质槽。
- D27：玩家、NPC、动物表现恢复和动画组件补齐入口，静态确认 34 个角色 Prefab、12 个动物 Prefab。
- D28：船海天气表现入口、Calm/Rain/Storm、雨粒子和水面运动表现。
- D29：统一快捷交互 HUD，市场/仓库/酒馆/船坞/登船/天气/保存/画质均有可见成功或失败反馈路径。
- D30：逻辑只读状态到视觉路线阶段的投影桥，覆盖港口、交易、酒馆、船坞、船上、航行和返港。
- D31：Low/Medium/High 画质表现控制入口及质量审计边界。

这些条目说明“代码入口已存在”，不替代正式场景集成、Player 运行和人工视觉验收。唯一真实状态以 `VISUAL_TASK_STATUS_D25_D31.md` 及各任务结果 JSON 为准。

## 3. 并行和唯一写者规则

### 3.1 可以并行的任务

D25 基线通过后，D26、D27、D28、D29 的 `P` 任务可以并行，前提是每个 Agent 使用自己的工作目录、Prefab、材质或工作场景。它们不得修改正式 `InitialHarbor.unity`，不得修改 `Systems/`、`AgentCore/`、存档 owner 或逻辑契约。

### 3.2 必须串行的任务

下列任务是正式场景或正式 UI 的唯一写者，必须依次执行：

```text
D25-S01 → D25-S06 → D25-G07
→ D26-S05 → D26-G06
→ D27-S05 → D27-G06
→ D28-S05 → D28-G06
→ D29-S05 → D29-G06
→ D30-S05 → D30-G06
→ D31-S05 → D31-G06
```

D26—D29 的独立产物可以提前并行制作，但任何集成 Agent 都必须先读取上一轮场景 SHA、回滚快照和状态看板。

### 3.3 场景集成唯一写者

每一轮只有当前 `S` 任务可以写：

- `game/Assets/Scenes/InitialHarbor.unity`；
- `game/Assets/Scenes/Bootstrap.unity`；
- 当前正式 Build Settings；
- 当前正式 UI Canvas 的根引用。

并行任务只允许写 `game/Assets/Work/Visual/<task>/`、任务专属 Prefab、材质、模型、测试和审计结果。若必须改正式场景才能验证，先停下并记录 `BLOCKED`，不要越权。

## 4. 通用 Agent 提示词

以下文本是所有任务的共同前缀。复制某个任务卡时，把任务卡中的“任务提示词”接在后面即可。

```text
你正在执行 WhalesIslandReef 项目的视觉 Runtime 任务。

开工前必须读取：
1. docs/CONTEXT.md
2. docs/game/VISUAL_DEVELOPMENT_WORK_PLAN_D25_D31.md
3. docs/game/VISUAL_TASK_STATUS_D25_D31.md
4. docs/game/design/common/D24_NEXT_RUNTIME_PHASE_HANDOFF.md
5. 本任务所属模块的现有设计文档和当前代码。

严格遵守：
- 只修改本任务卡声明的模块和目录；
- 并行 P 任务禁止修改 InitialHarbor.unity、Bootstrap.unity、Systems、AgentCore、存档 owner 和逻辑契约；
- 表现层只能读取 Systems/D22 读模型或 D20/D21/D22 公开事件，不能裁决交易、航行、关系、库存、天气、存档或动物事实；
- 不使用未登记来源、外部绝对路径或 Candidate/Pending 资源冒充已批准资源；
- 不用编辑器截图冒充 Player 运行时证据；
- 不注释代码、禁用测试或修改 Git 历史来掩盖失败；
- 发现前置缺失、场景冲突、资源缺失或输入无效时，记录 BLOCKED/PARTIAL 和复现步骤。

完成时必须：
- 运行本任务的构建、静态检查和测试；
- 在 game/tools/results/<章节>/<任务>/ 写机器可读结果 JSON 和原始日志；
- 在 docs/game/design/common/handoffs/<章节>/ 写同编号交接报告；
- 更新 docs/game/VISUAL_TASK_STATUS_D25_D31.md；
- 更新 .workbuddy/memory/YYYY-MM-DD.md；
- 在最终汇报中列出修改文件、真实测试结果、状态、未解决问题和下一个可执行任务。
```

## 5. D25：视觉基础和渲染基线

### D25-S01｜基线冻结与视觉债务清点（S）

**前置：** D24-L35/L36 为 `LOGIC_PASS`，当前 Unity 能打开 `InitialHarbor`。  
**写入范围：** 文档、`tools/sync_visual_task_status.py`、D25 结果目录；不得改变正式场景。  
**任务提示词：**

```text
建立 D25—D31 的视觉基线。清点当前 InitialHarbor 的根对象、正式相机、光源、Canvas、旧视觉代理、D19 R03 表现层、D20 角色 Prefab、D19 水体/植被 Prefab、稳定交互锚点和当前可运行入口。

建立视觉债务表：平坦地面、白色/纯色平面、块状建筑、旧 Renderer、遮挡、缺材质、UI 重叠、调试叠字、死按钮、缺天气、缺动画、缺碰撞、缺 LOD、缺保存恢复证据分别登记文件和责任任务。不要修复其他任务的问题。

建立 tools/sync_visual_task_status.py 和 D25—D31 状态文件的最小可重复格式，写出当前工作树 SHA、Unity 版本、场景 SHA、编译结果和已知未提交变更。若无法证明某项，标记 PARTIAL/BLOCKED。
```

**成功状态：** `PASS`。必须有基线 JSON、清点报告、回滚/快照标识、状态脚本和交接报告。

### D25-P02｜统一视觉主题、材质和质量档（P）

**前置：** D25-S01。  
**写入范围：** `game/Assets/Art/Materials/D25/`、`game/Assets/Scripts/Gameplay/D25/`、任务测试；不写正式场景。  
**任务提示词：**

```text
建立 D25 视觉主题层：加勒比 1675 风格化低多边形颜色、木材/石材/沙土/湿地/帆布/金属/海水的材质实例、Low/Medium/High 三档表现参数。优先复用已经有许可证和导入证据的资源；没有真实资源时使用项目自有材质，不创建随机纯色方块。

把颜色、粗糙度、阴影距离、LOD bias、透明度和后处理参数放在表现层配置中。画质档只能改变渲染表现，不得改变实体、事件、库存、航行、存档或逻辑 Tick。为材质和质量配置添加 EditMode/静态验证，检查 Shader 存在、材质不为粉色、配置范围有效。
```

**成功状态：** `ART_PASS`。必须有材质清单、质量配置、测试结果和无外部绝对路径审计。

### D25-P03｜相机、天空、灯光和雾（P）

**前置：** D25-S01。  
**写入范围：** D25 相机/灯光 Prefab、工作场景、表现脚本；不写正式场景。  
**任务提示词：**

```text
建立 InitialHarbor 的视觉相机和灯光工作层。设计出生点、市场、码头、海岸、船坞和远眺五类构图，保留玩家可行走和交互空间。使用有来源的太阳/天空/环境光关系，确保人物脚底、船体接触面、门窗、道路和交互目标可读；通过雾和远景层隐藏不了几何错误。

建立 Calm/Rain/Storm 的表现参数接口，但不修改 D21 天气事实。所有表现变化只消费公开天气快照；测试相机裁剪面、遮挡保护、曝光和 UI 安全区。输出每个机位的坐标、朝向、目标对象和回退策略。
```

**成功状态：** `ART_PASS`。必须有工作场景/Prefab、机位表、参数审计和渲染预览；不得把预览当作 Runtime 通过。

### D25-P04｜玩家模式 HUD 清理和信息层级（P）

**前置：** D25-S01。  
**写入范围：** D25 UI Prefab/脚本和 UI 测试；不写正式场景。  
**任务提示词：**

```text
设计一个单一、可读的玩家 HUD：左上角港口/时间，右上角天气/船况摘要，底部中央当前交互提示，底部左侧短反馈。把开发调试信息、对象 ID、FPS、事件哈希、路线脚本、内部状态和资源审核状态全部放到显式 DEBUG 层，玩家模式默认关闭。

使用 CanvasScaler 的 1920×1080 参考分辨率和安全区锚点，确保 16:9、窗口缩放和长中文文本不重叠。HUD 只读 D2/D19/D21/D22 公开读模型；按钮/面板暂不在本任务裁决业务。添加面板数量、Canvas sorting order、raycast 和文本溢出测试。
```

**成功状态：** `ART_PASS`。必须有 UI 层级图、Prefab/脚本、不同分辨率静态审计和测试。

### D25-P05｜资源和 Prefab 视觉准入验证器（P）

**前置：** D25-S01、D18 资源准入目录。  
**写入范围：** D25 验证工具、审计 schema、测试；不得修改资源状态。  
**任务提示词：**

```text
建立视觉资源消费前的只读验证器。对每个模型/材质/Prefab 检查来源 key、许可证证据、文件存在、Unity GUID、单位比例、朝向、Renderer、材质、Collider、LOD、Animator、交互锚点和回退 Prefab。Candidate/Pending 只能报告为不可发布或待审核，不得自动升级 LocalDevelopmentReady/ReleaseApproved。

验证器要能区分 ART_PASS、SCENE_PASS 和 RUNTIME_PASS，并为缺项输出稳定错误码。为故意缺材质、缺锚点、缺 LOD、绝对路径和重复 Renderer 建立夹具测试。
```

**成功状态：** `ART_PASS`。必须有 schema、只读审计工具、正反例测试和机器结果。

### D25-S06｜视觉基础正式场景集成（S）

**前置：** D25-P02—P05 达到 `ART_PASS`。  
**写入范围：** `InitialHarbor.unity`、必要的 Bootstrap 引用、D25 集成脚本。  
**任务提示词：**

```text
作为 D25 唯一场景集成者，先保存 InitialHarbor 和 Bootstrap 的回滚副本，再把通过 D25 ART_PASS 的主题、相机、灯光和玩家 HUD 接入正式场景。关闭旧调试 Renderer 和重复 Canvas，但保留稳定交互节点、碰撞 owner、逻辑组件和回退入口。

严禁删除稳定 ID、移动 D19/D20/D21 的逻辑 owner 或把表现 Prefab 变成事实写者。构建 Unity 场景，检查无 Missing Script/Missing Material，确认玩家模式无调试叠字、Canvas 不重叠、主相机和 EventSystem 唯一。
```

**成功状态：** `SCENE_PASS`。必须有场景 SHA 前后对照、回滚文件、Unity 构建日志、静态审计和至少一次当前 Player 冒烟证据。

### D25-G07｜视觉基础汇合门禁（G）

**前置：** D25-S06。  
**任务提示词：**

```text
只做 D25 汇合审计，不顺手修复 D26—D31 内容。复核 D25 的所有结果 JSON、Prefab/材质/相机/HUD/场景证据、Unity 编译、Console 和回滚能力。用当前构建进入 InitialHarbor，确认玩家模式没有调试信息和 Canvas 重叠，记录任何阻断。

若有一项 P0 未满足，结论写 PARTIAL/BLOCKED 并生成精确修复任务；不得因为能启动 Unity 就写 D25 完成。
```

**成功状态：** `PASS` 或真实 `PARTIAL/BLOCKED`，不得伪造。

## 6. D26：港口和岛屿环境

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D26-P01` | P | 起伏地形、沙滩、湿沙、泥地、岩岸、道路和坡度 | `ART_PASS` |
| `D26-P02` | P | 港口、市场、仓库、酒馆、船坞模块，确保门窗/屋顶/用途可读 | `ART_PASS` |
| `D26-P03` | P | 植被、岩石、树木、草丛和风响应，解决块状分布 | `ART_PASS` |
| `D26-P04` | P | 货物、路牌、灯具、渔网、木箱、生活痕迹和港口装饰 | `ART_PASS` |
| `D26-S05` | S | 唯一写者将 P01—P04 接入 InitialHarbor，并对齐碰撞/锚点/NavMesh | `SCENE_PASS` |
| `D26-G06` | G | 固定机位、遮挡、比例、材质、近景/远景环境审计 | `PASS/PARTIAL` |

每个 P 任务必须使用独立工作场景或 Prefab；不得把 D19 R03 旧的平坦地面、白色代理和重复 Renderer 原样复制到正式场景。`D26-S05` 必须保留可回滚快照，并在运行时确认角色不会被旧 `BeachGround` 或重复 Collider 拉回错误高度。

**各任务通用任务提示词：**

```text
只实现本 D26 任务卡的环境表现。先审计 D18 资源准入和 D25 主题，建立工作 Prefab/工作场景，不改 InitialHarbor.unity。所有模型必须有比例、材质、LOD、碰撞或明确的无碰撞表现声明；所有交互对象必须使用已冻结稳定锚点，不私自新增玩法事实。

完成后运行 Unity 导入/Prefab 审计、EditMode 测试和 git diff --check，输出结果 JSON、交接报告、工作场景截图/预览和缺口。只有 D26-S05 可以把通过的结果接入正式场景。
```

## 7. D27：玩家、NPC、动物和动画

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D27-P01` | P | 两种玩家身体轮廓、三种发型、四套时代服装和手持物 | `ART_PASS` |
| `D27-P02` | P | 商人、船员、渔民、酒馆、船坞、守卫、居民等岗位可读 NPC | `ART_PASS` |
| `D27-P03` | P | 猫狗、鸟类、海岸动物、鱼群和至少一种珍稀观察动物 | `ART_PASS` |
| `D27-P04` | P | Humanoid/Generic Animator、动作回退、锚点、LOD、无根运动约束 | `ART_PASS` |
| `D27-S05` | S | 接入出生、行走、对话、交易、登船、观察和宠物跟随表现 | `SCENE_PASS` |
| `D27-G06` | G | 比例、动画、遮挡、岗位识别、交互目标和回退验收 | `PASS/PARTIAL` |

**任务提示词：**

```text
只实现本 D27 角色/动物视觉任务。角色表现只能读取 D20/D10/D11 的公开身份、行为和外观投影，不能修改关系、健康、宠物、生态、库存或位置事实。没有真实独立网格就明确标注复用，不得把换色写成独立人物。

所有 Prefab 必须检查根比例、朝向、Animator、Root Motion、LOD、头/手/携带/登船锚点、碰撞边界和 MissingResourcePlaceholder 回退。P 任务不得修改正式场景；D27-S05 才能接入运行时消费者，并记录真实生成和交互证据。
```

## 8. D28：船体、海水和天气

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D28-P01` | P | 船体、甲板、桅杆、帆、舵、锚、绳索、船舱和靠泊点 | `ART_PASS` |
| `D28-P02` | P | 深海/浅海/湿岸/泡沫/浪线/礁石和海面层次 | `ART_PASS` |
| `D28-P03` | P | Calm、Rain、Storm 的雨、风、浪、能见度和反馈 | `ART_PASS` |
| `D28-P04` | P | 天空、远景海岛、雾、反射、后处理和夜间灯光 | `ART_PASS` |
| `D28-S05` | S | 接入登船、离港、航行、到达、靠岸和天气表现 | `SCENE_PASS` |
| `D28-G06` | G | 三天气、船体可读性、海陆边界和航行视觉回归 | `PASS/PARTIAL` |

**任务提示词：**

```text
只实现本 D28 船舶/海洋/天气表现任务。所有航行、船况、锚泊、天气事实必须由 D21 owner 裁决；表现层只消费只读快照和事件。船体 Prefab 必须区分视觉几何、碰撞 owner、舵位/锚/帆/登船锚点，并提供低模回退。

先在工作场景验证材质和天气参数，P 任务不得修改 InitialHarbor.unity。D28-S05 负责正式场景和航行入口集成；不能用静止水面截图代替航行/天气过程证据。
```

## 9. D29：正式 UI、HUD 和交互面板

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D29-P01` | P | 主 HUD、资源、任务、天气、船况、提示和反馈布局 | `ART_PASS` |
| `D29-P02` | P | 市场、仓库、酒馆、船坞、角色、保存和设置面板 | `ART_PASS` |
| `D29-P03` | P | Button/Focus/Keyboard/Mouse 命令桥接和禁用原因 | `ART_PASS` |
| `D29-P04` | P | 中文文本、缩放、字幕、对比度、键鼠/手柄导航 | `ART_PASS` |
| `D29-S05` | S | 唯一 UI 集成者替换旧堆叠 Canvas 并接入 D22 命令 | `SCENE_PASS` |
| `D29-G06` | G | 逐按钮成功/失败/禁用反馈、互斥和焦点验收 | `PASS/PARTIAL` |

**任务提示词：**

```text
只实现本 D29 UI 任务。所有 UI 业务操作必须发送 D22 已冻结命令，读取 D19/D20/D21/D22 结果和失败码；UI 不直接改交易、航行、关系、库存或存档。每个按钮必须有 owner command、成功反馈、失败反馈、禁用原因和替代入口。

P 任务只写 UI Prefab、样式、测试或工作场景，不改 InitialHarbor.unity。D29-S05 先关闭重复旧 Canvas，再接入唯一正式 Canvas；验证 EventSystem、GraphicRaycaster、Canvas sorting order、模态互斥和 1920×1080/窗口缩放。
```

## 10. D30：视觉可玩交互闭环

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D30-P01` | P | 逻辑状态到视觉状态、提示、错误和结果反馈桥 | `ART_PASS` |
| `D30-P02` | P | 港口/船舶/航行相机、过渡、淡入淡出和可中断处理 | `ART_PASS` |
| `D30-P03` | P | 保存、加载、继续后的角色/船/天气/UI 视觉恢复 | `ART_PASS` |
| `D30-P04` | P | 交互音效、环境声、粒子、海浪和动作反馈 | `ART_PASS` |
| `D30-S05` | S | 完成新游戏到保存重载的真实 Player 路线 | `RUNTIME_PASS` |
| `D30-G06` | G | 路线、按钮、状态一致性、重复点击和异常恢复门禁 | `PASS/PARTIAL` |

**任务提示词：**

```text
只实现本 D30 视觉交互任务。所有反馈必须能追溯到当前逻辑事件、命令回执或只读读模型；禁止用固定延时、假日志、手工改档或静态文字伪造成功。保存/加载必须使用真实 SaveSystem 介质，记录前后状态 fingerprint 和事件游标。

D30-S05 才能执行完整 Player 路线：新游戏→港口→市场交易→仓库/酒馆/船坞→登船→出航→航行→返港→保存→重启→加载→继续。每一步记录可重放输入、可见 UI、日志、状态快照和失败路径。
```

## 11. D31：视觉质量、性能和交互收口

| 任务 | 模式 | 工作内容 | 成功状态 |
|---|---|---|---|
| `D31-P01` | P | LOD、遮挡剔除、批处理、对象池、远景代理 | `ART_PASS` |
| `D31-P02` | P | 贴图、Shader、阴影、透明、粒子和内存预算 | `ART_PASS` |
| `D31-P03` | P | 1920×1080 Low/Medium/High、帧时间和 GC 采样 | `RUNTIME_PASS/PARTIAL` |
| `D31-P04` | P | 14 固定镜头、3 天气镜头、UI/Console/资源回归 | `RUNTIME_PASS/PARTIAL` |
| `D31-S05` | S | 唯一最终整合者建立 Visual Candidate 构建 | `SCENE_PASS` |
| `D31-G06` | G | 视觉、交互和性能候选版总验收 | `VISUAL_CANDIDATE/PARTIAL` |

**任务提示词：**

```text
只实现本 D31 质量任务。优化表现层，不得删除逻辑实体、事件、存档字段、交互锚点、碰撞 owner 或远景事实。三档画质必须记录实际分辨率、质量档、硬件、warm-up、采样窗口、平均 FPS、1% low、CPU/GPU、GC、内存和天气状态。

固定镜头必须来自当前构建，玩家模式不得带 DEBUG 水印或开发叠字。若性能或镜头不达标，结论写 PARTIAL/BLOCKED，明确责任任务和回退，而不是降低阈值或删除内容。
```

## 12. 统一证据目录

每个任务使用以下布局：

```text
game/tools/results/D25/<TASK_ID>/
game/tools/results/D26/<TASK_ID>/
...
docs/game/design/common/handoffs/D25/<TASK_ID>_HANDOFF.md
docs/game/design/common/handoffs/D26/<TASK_ID>_HANDOFF.md
...
```

结果 JSON 至少包含：`task_id`、`status`、`started_at`、`finished_at`、`git_sha`、`unity_version`、`write_scope`、`changed_files`、`commands`、`tests`、`evidence`、`blockers`、`next_task`。运行时任务额外包含 `build_id`、`input_trace`、`scene`、`resolution`、`quality`、`weather`、`save_fingerprint`。

## 13. 当前执行顺序

第一轮先执行：

```text
D25-S01
→ D25-P02 / D25-P03 / D25-P04 / D25-P05（可并行）
→ D25-S06
→ D25-G07
```

D25-G07 通过后，才开始 D26—D29 的四组并行制作；D30 必须等待 D26—D29 的集成门禁，D31 必须等待 D30-G06。任何 `PARTIAL` 不得自动放行下游，必须按结果中的 `next_task` 建立修复任务。

## 14. 本轮执行结果与下一步

### 14.1 已完成的代码级/冒烟级闭环

- D27：表现恢复器等待 D2 真正创建 `D2_PlayerAvatar` 并确认 Renderer 后才完成绑定，避免把场景锚点误认成玩家实例。
- D29：唯一交互 HUD 确保 `EventSystem`/`InputSystemUIInputModule`、按钮目标图形、反馈栏和临时面板互斥；酒馆拥有可见关闭按钮，并兼容 Escape 关闭后恢复主 HUD。
- D31-S05：独立 Windows Player R6/R8/R11/R13/R16/R18/R21/R22/R23 构建成功；R16 的真实路线、R18 相机修复和 R23 HUD 安全区复核记录在 `game/tools/results/D31/` 及配套 handoff 中。
- D31 交互代码级收口：R19/R20 为 D29 主 HUD、仓库、D4、D9、D10 接入焦点范围、清理隐藏页选择并补齐 owner 未就绪反馈；29 个可见入口与 4 个键盘入口的静态逻辑审计 `PASS`，定向回归 `19/19 PASS`，但没有把静态结果当作 Player 运行通过。
- 静态/单元回归：D25—D31 当前纳入回归的测试 `43/43 PASS`；R6/R8/R11/R13/R16/R21/R22/R23 runtime/build log 未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`（D7 的 `error=0.003` 是数值指标，不是错误日志）。

### 14.2 尚未完成的门禁

以下内容仍必须由后续受控 Player/性能回归任务补齐，不能用本轮冒烟替代：

- D30 的港口→交易→仓库/酒馆/船坞→登船→出航→返港→保存→重载完整路线的逐阶段截图与状态 fingerprint；R16 已完成逻辑路线日志，但不能替代这些证据；
- 所有正式按钮的成功/失败/禁用反馈矩阵，以及焦点、异常恢复和重复点击；R16 已完成可见入口触发切片，但尚未完成负向矩阵；
- R14/R22/R23 已完成 14 个固定验收镜头与 Calm/Rain/Storm 三种天气变体的文件捕获；R22/R23 已补齐雨丝可读性，但仍需最终镜头质量判定；
- 1920×1080 下 Low/Medium/High 的实际帧时间、1% low、GC、内存和三天气性能证据；
- 最终 Visual Candidate 场景视觉质量与无调试叠字验收。

因此当前状态看板中的 `PARTIAL` 是有意保留的真实状态。下一执行目标应先在代码层补齐 D29-G06 的失败/禁用/重复点击矩阵与 D30-G06 的异常恢复断言，再用最小 Player 证据复核；随后处理低模画面质量和 D31-P03 性能缺口，最后重新验收 D31-G06。

## 15. R6 受控 Player 证据归档（2026-09-03）

本轮完成一次不依赖编辑器 GameView 的独立 Windows Player R6 受控路线。路线覆盖：

`MainMenu → NewGame → InitialHarbor → I 仓库 → P D9 整备/泊位 → D4 船坞内容 → E 登船 → T 出航 → N×2 → L 靠港结算 → C 保存 → 重启 → Continue`

结果：Unity 构建成功；D27 真实等待 D2 玩家实例化后绑定 Renderer；D9/D4 临时页可关闭并恢复主 HUD；登船、航行、抵达、靠港、结算、保存和 Continue 读取均有运行日志；D25—D31 静态/单元测试 `30/30 PASS`。证据集中在 `game/tools/results/D31/D31-S05/` 的 R6 build/runtime/continue 日志和路线报告。

该证据将 D29-S05、D31-S05 的状态从“缺少 Player 构建证据”推进为“有范围受限的真实纵向切片”；配合 R8/R11 复核，D25—D31 的旧“无 Player 证据”阻断已改为证据一致的 `PARTIAL`。完整 D30 按钮矩阵、14 固定镜头、Calm/Rain/Storm 全过程、Low/Medium/High 性能和最终视觉质量仍需后续任务闭环。

## 16. D29—D30 交互矩阵静态收口（2026-09-03）

已建立机器可读的 [D29_D30_INTERACTION_BUTTON_MATRIX.json](design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json) 和配套 [说明文档](design/common/D29_D30_INTERACTION_BUTTON_MATRIX.md)。矩阵覆盖 29 个可见按钮与 4 个 D4 键盘专用入口；审计确认每个入口都有现存回调、owner 归属和可见反馈路径，结果 `PASS`。D25—D31 静态/单元回归升至 `30/30 PASS`。

## 17. R8 仓库模态互斥复核（2026-09-03）

R8 在 R7 的 D30 顶部路线条安全区修复和 D29 仓库互斥修复后重新构建。受控路线为：

`InitialHarbor → I 仓库只读面板 → Escape 关闭 → 恢复主 HUD`

Player 画面确认仓库面板居中显示时，D25 主 HUD 与 D30 路线条隐藏；关闭后 D25、D30 和 D29 主层恢复。R8 runtime log 未发现异常级 `Exception`、`Error`、`MissingComponent` 或 `Crash`。本次只证明仓库模态互斥，不替代 D30 全路线、29 个按钮逐项运行记录、三天气、固定镜头和性能门禁。

证据：`game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R8.log`、`D31_S05_STANDALONE_RUNTIME_R8.log`、`D31_S05_STANDALONE_ROUTE_R8.md`。本轮状态继续为：D25—D31 任务有真实纵向切片，但尚未达到 `VISUAL_CANDIDATE`。

这一步解决“静态代码里是否存在死按钮”的审计缺口，并通过 R11 补充了主 HUD 的 P 入口、D9/D4 Esc 关闭、标题安全区和临时页输入不串层；不把静态 PASS 当作 Player PASS。D29-G06 仍需要当前构建逐按钮的成功、失败、禁用和互斥运行记录；D30-G06、D31-G06 的完整路线、固定镜头、天气和性能门禁仍保持 `PARTIAL`。

## 18. R11 输入归属与临时页安全区复核（2026-09-03）

R11 在三项交互修复后完成独立 Windows Player 构建：

1. 主 HUD 的 `P` 快捷键可以打开 D9 整备页；
2. D9/D4 长摘要下移到中央港口标题下方；
3. D9/D4 临时页打开时，D29 主快捷键让路，不会把 `R` 等按键再次解释成 D29 天气；
4. D9/D4 均可用 `Esc` 关闭并恢复 D25/D30/D29 主层。

本轮 runtime log 未发现异常级 `Exception`、`MissingComponent` 或 `Crash`。R11 只收口输入归属和页面安全区，不替代 29+4 入口逐项结果、完整 D30 路线、固定镜头、三天气、性能和最终视觉质量。

证据：`game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R11.log`、`D31_S05_STANDALONE_RUNTIME_R11.log`、`D31_S05_STANDALONE_ROUTE_R11.md`。

## 19. R13 D9/D3 输入所有权回归（2026-09-03）

R13 针对 R12 复现的真实 Player 串层缺陷完成修复和回归：D9 整备页打开时，D3 的 `B=交易`、D29 的主快捷键以及 D2/D4/D5 的主输入不再抢占临时页面；D9 的 B/M/X/T/S/R/Q/W/U/D 使用 D16 可重绑定路径并提供同一意图的 Player 兼容边沿。

实际路线：

`MainMenu → NewGame(CodexCaptainR13) → InitialHarbor → P(D9) → B(泊位) → M(买盐) → T(D10酒馆) → Esc → B(主HUD交易) → Esc → P(D9) → U(船坞门禁) → Esc`

结果：D9 的 B 保持整备页并记录“已分配泊位”；M 修改金币/盐库存并给出购买反馈；T 进入 D10、Esc 恢复主 HUD；关闭临时页后主 HUD 的 B 仍能打开 D3 交易；U 显示声望不足的明确拒绝反馈。R13 Unity Player 构建成功，定向模态/输入回归 `15/15 PASS`。

证据：`game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R13.log`、`D31_S05_STANDALONE_RUNTIME_R13.log`、`D31_S05_STANDALONE_ROUTE_R13.md`。

R13 只补充代表性输入所有权和反馈证据，不关闭 29+4 入口逐项运行矩阵、D30 全路线、14 固定镜头、三天气和性能门禁；D29-S05、D30-S05、D31-S05 与对应 G06 继续保持 `PARTIAL`。

## 20. R16 全入口切片、D30 路线与 R18 相机修复（2026-09-03）

### 20.1 已执行

- R16 按 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 执行了 D29 主 HUD、仓库、D4 内容页和 D9 整备页的可见入口点击，并执行 D4 的 `1/2/3/B/E/C` 键盘专用入口；每个已执行入口均产生可见反馈或明确拒绝。
- R16 真实重放了 `新游戏 → D9 泊位 → 登船 → 规划航线 → 盘查 → 平静海面 → 抵达 → 靠港结算 → 保存 → Continue`，对应 D3/D9/D2/Save 日志已归档。
- R18 修复 `CameraFollow` 的受控船体自碰撞过滤，并在进入船控时从 Character 预设切到 ShipDeck 预设；构建成功且受控观察到镜头不再被自身船体碰撞体压成贴脸距离。

### 20.2 证据

- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_FULL_MATRIX.md`
- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_ROUTE_R16_D30.md`
- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R16_FULL_MATRIX.log`
- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_RUNTIME_R16_D30_ROUTE.log`
- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_CONTINUE_R16.log`
- `game/tools/results/D31/D31-S05/D31_S05_STANDALONE_BUILD_R18.log`

### 20.3 当前判断

这轮把 D29/D30/D31 从“没有完整 Player 证据”推进到“有可见入口切片、主要路线、14 固定镜头/三天气捕获和相机修复证据”，但没有关闭最终门禁。D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`；后续只需针对尚未关闭的负向交互、天气可读性、性能和最终画面质量补证，不重复执行已经有证据的整条路线。

## 21. R19 D31 交互代码级焦点收口（2026-09-03）

R19 在不重复 Unity 长时间构建的前提下，完成了交互层的代码级收口：D29 主 HUD、D29 仓库、D4 内容页、D9 整备页和 D10 酒馆页都使用现有 `D16ModalFocusScope`；焦点范围按页面层级建立显式导航；页面 Canvas、CanvasGroup 或根对象不可见/不可交互时，焦点组件清理自己持有的 EventSystem 选择。

R19 的机器结果为 `STATIC_LOGIC_ONLY / PASS`，覆盖 29 个可见入口和 4 个 D4 键盘入口，定向测试 `16/16 PASS`；随后 Unity batchmode 脚本编译以 `Tundra build success (4.21 seconds)` 结束，无 `error CS`。R20 又补齐 D4/D9/D10 owner 未就绪时的可见反馈，完整回归为 `42/42 PASS`、定向交互回归为 `19/19 PASS`，Unity 编译以 `Tundra build success (3.28 seconds)` 结束。上述结果关闭了源码层焦点和静默失败风险，但不替代运行时的逐入口成功/失败/禁用/重复点击、D30 异常恢复、天气可读性、性能和最终画面门禁；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 22. R21—R23 天气可读性与 HUD 安全区复核（2026-09-03）

### 22.1 D28 天气表现修补

R21 首次验证了相机跟随雨层和透明拉伸粒子确实能出现在 Player backbuffer，但视觉强度过高；R22 收敛为可读而不遮挡场景的参数：

- Rain/Storm 雨层跟随当前主相机，不再固定在世界原点；
- 使用透明粒子材质和拉伸渲染，Rain/Storm 分别使用不同的雨量、风向、颜色和水面波动倍率；
- 不修改 D21 天气事实、航行风险、存档或任何业务数值。

R22/R23 的 Player 证据确认 14 个固定镜头和 Calm/Rain/Storm 三天气仍全部落盘，Rain/Storm 雨丝达到可读级别；但低模建筑、材质细节和整体场景质量仍不满足最终 `VISUAL_CANDIDATE`。

### 22.2 D30 路线条安全区

R23 将 D30 路线条从顶部标题副区域继续下移到 `anchoredPosition = (0, -174)`，Player 截图确认港口主标题、副标题和路线信息不再互相压叠。该修改只影响表现层布局，不改变 D30 状态桥或业务 owner。

### 22.3 证据与当前判断

- [R23 视觉证据 JSON](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R23.json)
- [R23 截图目录](../../game/tools/results/D31/D31-P04/visual_evidence_R23/)
- [R23 Player 构建日志](../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R23.log)
- [R21 Unity 编译日志](../../game/tools/results/D31/D31-P04/D31_P04_UNITY_COMPILE_R21.log)

R21/R22/R23 只关闭了“天气完全不可读”和“路线条压住副标题”的具体缺陷，没有关闭低模画质、Player 负向交互矩阵、D30 异常恢复或性能 1% low 门禁；D28-G06、D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 23. R24 失败反馈与交互状态保护（2026-09-03）

R24 继续以代码层为主，修复了三类会让玩家误以为按钮“坏了”的状态错误：

- D29 市场按钮先等待 `D3RuntimeBootstrap.RequestTrade()` 返回；owner 拒绝时不再提前打开空的 `D3Canvas`，并向 D30 表现层发送失败事件；
- D29 仓库面板或内容区域缺失时给出可见失败反馈，关闭仓库时也显示恢复回执；
- D30 收到 `market_failed`、`board_failed` 或未知事件时保持当前阶段，显示“请求未通过，状态未改变”，并防护过早通知时的空过渡文本引用。

R24 的审计范围仍是表现层和输入/状态边界，不修改 D3/D21 业务 owner、交易/航行公式、存档字段或网络逻辑。新增 R24 机器证据为 `STATIC_LOGIC_ONLY / PASS`；选定回归 `46/46 PASS`；Unity batchmode 编译 `Tundra build success (2.95 seconds)`，无 `error CS`。

- [R24 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R24.json)
- [R24 Unity 编译日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R24.log)

R24 关闭了源码层的三类误反馈风险，但没有替代真实 Player 的逐入口成功/失败/禁用/焦点/重复点击矩阵，也没有关闭 D30 异常恢复、最终低模画面质量或性能 1% low 门禁；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 24. R25 港内表现阶段状态投影修复（2026-09-03）

R25 修复 D30 路线条的阶段保持逻辑：`Market`、`Tavern`、`Shipyard` 是由交互事件产生的港内表现阶段，不能因为 D3 仍处于 `InPort` 就在下一次刷新时被重置为 `Harbor`；只有船上、航行或返港等 owner 状态变化才覆盖当前表现阶段。R24 的失败事件保护和空文本防护继续保留。

R25 不修改 D3/D21 业务事实、航行状态、存档或任何数值，只修复 D30 表现投影。验证结果：选定回归 `47/47 PASS`；D31 交互逻辑审计 `STATIC_LOGIC_ONLY / PASS`；Unity batchmode `Tundra build success (4.07 seconds)`，无 `error CS`。

- [R25 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R25.json)
- [R25 Unity 编译日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R25.log)

R25 关闭了港内路线表现被错误重置的源码风险，但仍没有替代真实 Player 的逐阶段截图、失败/禁用/重复点击、D30 异常恢复、最终低模画面质量或性能 1% low 证据；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 25. R26 额度友好逻辑复核（2026-09-03）

R26 不修改源代码、场景、Prefab、资源或业务数据，只复核当前 D25—D31 代码层交付是否一致：

- 选定的 13 个 D25—D31 视觉/交互测试模块 `47/47 PASS`；
- `D31` 交互逻辑审计 `STATIC_LOGIC_ONLY / PASS`，覆盖 29 个可见入口和 4 个键盘专用入口；
- `D29/D30` 入口矩阵审计 `PASS`，覆盖 29 个可见按钮和 4 个键盘入口；
- D25—D31 结果 JSON `57/57` 可解析，`git diff --check` 通过；
- 未重复 Player、固定镜头、性能或长时间 Unity 构建，以控制 Plus 滚动额度；R25 的 Unity 短编译证据继续作为最新编译证据。

R26 只确认代码层交互目标稳定，不升级任何运行时门禁。D29-G06、D30-G06、D31-G06 仍保持 `PARTIAL`，因为真实 Player 的逐入口负向/禁用/重复点击、D30 异常恢复、最终画面质量和性能 1% low 仍属于后置运行时验收。

- [R26 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R26.log)
- [R25 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R25.json)
- [D25—D31 状态看板](VISUAL_TASK_STATUS_D25_D31.md)

## 26. R27 港口表现网格倒角修补（2026-09-03）

### 26.1 修改范围

R27 找到一个直接造成港口“全是方块”的源头：`D19R03HarborPresentationBuilder.MakeBevelBoxMesh` 虽然被建筑、门窗、箱子、码头边缘和船体大量调用，但旧实现实际直接返回 `MakeBoxMesh`。本轮将其改为生成封闭的倒角长方体，包含六个内缩面、十二组倒角面和八个角部三角面，并对三角形绕序做确定性校正。该改动只属于编辑器表现生成器，不改变 D2/D3/D6/D21 业务 owner、输入、碰撞所有权、存档或资源准入状态。

### 26.2 证据

- 生成器源码：[D19R03HarborPresentationBuilder.cs](../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- 静态回归：[test_d19_r03_visual_builder.py](../../tools/tests/test_d19_r03_visual_builder.py)
- Unity 重新生成日志：`game/tools/results/D31/D31-S05/D31_S05_VISUAL_REBUILD_R27.log`
- Unity 短编译日志：`game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R27.log`
- Editor 审计日志：`game/tools/results/D31/D31-S05/D31_S05_VISUAL_AUDIT_R27.log`
- Editor 审计结果：`game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json`
- 更新后的 R03 表现 Prefab：`game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab`

本轮验证：新视觉生成器测试 `3/3 PASS`；D25—D31 选定回归 `50/50 PASS`；Unity Tundra 短编译成功；审计为 `315 Renderer / 15,609 mesh vertices / 1 R03 collider`。R27 没有重新运行 Player、固定镜头或性能采样，以控制 Plus 滚动额度。

### 26.3 当前判断

R27 关闭了“倒角接口实际退化成普通盒体”的代码缺陷，并重新生成了正式 R03 表现 Prefab；它不是最终画质验收。当前仍需后置处理：真实 Player 中确认倒角后的建筑/码头/船体视觉效果，继续清理低模材质与平坦地表，完成逐入口负向/禁用/重复点击、D30 异常恢复和性能 1% low 门禁。因此 D26-S05/D26-G06、D31-P04/D31-S05/D31-G06 继续保持 `PARTIAL`。

## 27. R28—R29 固定镜头归属与当前港口坐标复核（2026-09-03）

### 27.1 R28 发现并修复探针所有权缺陷

R28 首次在真正关闭 `CameraFollow` 后运行 D31 视觉探针，确认旧固定镜头坐标大多属于已退役的世界布局，导致部分 PNG 只拍到空地、地平线或过近的遮挡物。此前截图虽然文件存在，但不能证明镜头拍到了当前港口目标。源码修复包括：

- `D31VisualEvidenceProbe` 获取并暂时禁用当前相机上的 `CameraFollow`；
- 探针退出前恢复原来的 `CameraFollow.enabled` 状态；
- 新增静态测试，防止固定镜头再次被跟随脚本覆盖；
- 不写场景资产、D2/D3/D21 事实、存档或业务数值。

### 27.2 R29 当前布局重拍

R29 将 13 个 InitialHarbor 固定镜头和天气镜头调整到当前 D19-R03 港口坐标范围，保留主菜单镜头，并使用独立 Windows x64 Player 重新捕获：

- 固定镜头：`14/14` 文件存在；
- 天气变体：`Calm/Rain/Storm = 3/3` 文件存在；
- 分辨率：`1920×1080`，`ScreenCapture` backbuffer；
- 相机记录与探针请求坐标一致，证明固定镜头归属已生效；
- R27 生成的倒角表现网格已在 Player 中出现；
- 仍观察到低模/程序化材质、少数镜头被杆件或近景几何遮挡，不能据此升级最终画质门禁。

### 27.3 证据与当前判断

- [R29 视觉证据 JSON](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R29.json)
- [R29 截图目录](../../game/tools/results/D31/D31-P04/visual_evidence_R29/)
- [R29 Player 构建日志](../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R29.log)
- [R28 Player 构建日志](../../game/tools/results/D31/D31-P04/D31_P04_STANDALONE_BUILD_R28.log)
- [D31 视觉探针源码](../../game/Assets/Scripts/Gameplay/D31/D31VisualEvidenceProbe.cs)
- [D31 视觉探针测试](../../tools/tests/test_d31_visual_evidence.py)

本轮选定 D25—D31 回归为 `52/52 PASS`；D31 交互逻辑审计为 `STATIC_LOGIC_ONLY / PASS`，覆盖 29 个可见入口和 4 个键盘入口；D29/D30 矩阵审计为 `PASS`；结果 JSON `58/58` 可解析。R29 关闭了固定镜头证据归属和 R27 Player 复核这两个具体缺口，但 D29/D30/D31 汇合门禁仍因逐入口负向/禁用/重复点击、D30 异常恢复、最终低模画面质量和性能 1% low 证据不足而保持 `PARTIAL`。

## 28. R30 owner 结果传播与失败阶段保护（2026-09-03）

### 28.1 修复内容

R30 处理了 D29 交互层最后一个明确的静态逻辑缺口：D29 原来调用 D10 酒馆和 D4 船坞的 `void` 入口后立即显示成功，无法区分“页面打开”与“owner 接受”。本轮没有把业务裁决搬进 UI，而是给现有 owner 增加只读结果/反馈出口，并让 D29 依据这些出口决定可见反馈和 D30 过渡事件：

- D10 增加 `HasEnteredTavern`、`HasInputFocus`、`LastMessage`，酒馆只有进入裁决成功且页面获得焦点时才通知 `tavern_opened`；
- D4 增加 `TryUpgradeHull()`、`LastMessage`、`HasVisibleContentPage`，保留原 `UpgradeHull()` 兼容入口，船坞只有改装 owner 接受且页面可见时才通知 `shipyard_opened`；
- D9 整备入口读取已有 `HasInputFocus`，不能建立页面焦点时通知 `preparation_failed`；
- D30 将 `tavern_failed`、`shipyard_failed`、`preparation_failed` 与既有失败事件统一为“保持当前阶段、显示请求未通过”，不再用错误的路线阶段掩盖 owner 拒绝；
- 交互静态审计增加上述结果传播和失败事件的必查项，防止后续回退到无条件成功提示。

### 28.2 额度友好验证

- 选定 D25—D31 回归：`54/54 PASS`；
- `d31_interaction_logic_audit.py`：`STATIC_LOGIC_ONLY / PASS`，29 个可见入口 + 4 个键盘入口；
- `d29_d30_interaction_matrix_audit.py`：`PASS`；
- Unity 6000.0.80f1 batchmode 编译：`Tundra build success (3.37 seconds)`，无新增 `error CS`；
- D25—D31 结果 JSON：`59/59` 可解析；`git diff --check` 通过。

证据：

- [R30 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R30.json)
- [R30 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R30.log)
- [R30 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R30.log)

R30 只关闭代码层“owner 拒绝仍被 D29 报成功”的缺陷，不把静态审计当作运行时通过。D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`：真实 Player 的逐入口负向/禁用/重复点击、D30 异常恢复、最终低模画面质量和性能 1% low 仍需后置门禁。

## 29. R31 天气与画质入口的 owner 缺失反馈（2026-09-03）

R31 继续做代码层交互收口，修复 D29 对天气和画质切换的无条件成功提示：

- D28 增加返回布尔结果的 `TryCycleWeather()`，保留 `CycleWeather()` 兼容旧调用；
- D31 增加返回布尔结果的 `TryCycleQuality()`，保留 `CycleQuality()` 兼容旧调用；
- D29 根据 `Try...` 返回值显示成功或“owner 尚未就绪，暂不能切换”，不读取或修改任何业务事实；
- 交互静态审计和回归测试增加天气/画质 owner 缺失路径，避免后续回退。

验证：选定 D25—D31 回归 `55/55 PASS`；D31 交互静态审计 `STATIC_LOGIC_ONLY / PASS`，覆盖 29 个可见入口和 4 个键盘入口；D29/D30 矩阵审计 `PASS`；Unity 6000.0.80f1 batchmode 编译 `Tundra build success (2.89 seconds)`，无新增 `error CS`；在新增 R31 记录前的结果 JSON `59/59` 可解析，新增记录后当前总数为 `60/60`。

证据：

- [R31 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R31.json)
- [R31 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R31.log)
- [R31 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R31.log)

R31 仍只证明源码层反馈正确，不关闭真实 Player 的逐入口负向/禁用/重复点击、D30 异常恢复、最终低模画面质量和性能 1% low 门禁；D29-G06、D30-G06、D31-G06 保持 `PARTIAL`。

## 30. R32 D29 可用性/防重与 D30 重复事件保护（2026-09-03）

### 30.1 代码修复

R32 继续以纯逻辑和低成本验证为主，收口了上一轮仍可直接修复的交互缺陷：

- D29 在 owner 解析后按公开就绪状态更新市场、酒馆、船坞、登船、天气、保存和整备按钮的 `Button.interactable`；不可用入口由状态栏列出原因，owner 恢复后自动重新启用；
- D29 为市场、酒馆、船坞、登船、整备和保存增加 0.35 秒表现层一次性请求防重，重复点击显示“重复点击已忽略”；天气和画质预览切换不经过此防重；
- D30 增加 `preparation_opened` 成功事件分支，避免整备页打开后被当成未知失败事件；
- D30 对相同事件的短窗口重复通知保持当前视觉阶段，并显示“重复事件已忽略，状态未改变”；失败事件仍显示“请求未通过，状态未改变”；
- 删除 D29 可用性 setter 中无意义的对象名称自赋值，避免以无效写操作伪装反馈。

表现层仍只读取 owner 状态，不写交易、库存、航行、关系、存档或天气事实。

### 30.2 额度友好验证

- 选定 D25—D31 回归：`57/57 PASS`；
- `d31_interaction_logic_audit.py`：`STATIC_LOGIC_ONLY / PASS`，29 个可见入口 + 4 个键盘入口；
- `d29_d30_interaction_matrix_audit.py`：`PASS`；
- Unity 6000.0.80f1 batchmode 编译：`Tundra build success (2.89 seconds)`，无新增 `error CS`；
- D25—D31 直接结果 JSON：R32 新增前 `60/60`，新增 R32 机器结果后 `61/61` 可解析；Unity Library 夹具 JSON 不计入。

证据：

- [R32 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R32.json)
- [R32 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R32.log)
- [R32 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R32.log)
- [D29/D30 交互矩阵](design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json)

R32 只证明代码层按钮禁用/可用性、一次性请求防重和 D30 事件投影正确，不把静态证据升级为 Player 运行通过。真实逐入口负向/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终低模画面质量和性能 1% low 仍未闭环；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 31. R33 D29 首帧可用性与酒馆拒绝回退

### 31.1 修复内容

R33 继续按纯逻辑优先策略处理一个可以在不启动完整 Player 的情况下明确关闭的交互缺口：

- D29 `Awake` 在 `BuildCanvas` 和 `ResolveOwners` 后立即调用 `RefreshButtonAvailability`，避免首帧 Button 状态仍是默认可点击、与 owner 就绪状态不一致；
- D29 酒馆入口在调用 D10 `OpenSocialPage` 后再次读取进入结果和页面焦点；当 D10 owner 拒绝进入时调用 `CloseSocialPage`，关闭临时页并恢复主 HUD，之后仍发送 `tavern_failed` 和显示 owner 原因；
- 新增静态单元断言和 D31 交互审计必查项，防止后续删除首帧刷新或失败回退；
- 不改变 D10/D3/D4/D9/D21 业务裁决、存档、输入事实、网络边界或 Unity 场景资产。

### 31.2 额度友好验证

- 选定 D25—D31 回归：`58/58 PASS`；
- `d31_interaction_logic_audit.py`：`STATIC_LOGIC_ONLY / PASS`，29 个可见入口 + 4 个键盘入口；
- `d29_d30_interaction_matrix_audit.py`：`PASS`；
- Unity 6000.0.80f1 batchmode 编译：`Tundra build success (2.54 seconds)`，无新增 `error CS`；
- D25—D31 直接结果 JSON：R33 新增前 `61/61`，新增 R33 机器结果后 `62/62` 可解析（Unity Library 夹具 JSON 不计入）；
- 本轮未重复完整 Player、性能采样或最终画面验收，以遵守 Plus 滚动额度限制。

证据：

- [R33 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R33.json)
- [R33 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R33.log)
- [R33 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R33.log)

R33 只证明代码层首帧可用性与酒馆拒绝回退，不把静态证据升级为 Player 运行通过。真实逐入口负向/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终低模画面质量和性能 1% low 仍未闭环；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 32. R34 D29 多入口异常恢复与 D30 失败投影

### 32.1 修复内容

R34 继续以代码层为当前优先级，关闭交互事件异常时最容易形成“页面卡死、主 HUD 消失、路线条误报”的缺口：

- D29 登船、市场、酒馆、船坞和港口整备入口统一增加异常保护；异常不会穿透 Unity UI 事件链；
- 新增 `ReportActionException`，按入口尽力关闭对应临时页或恢复主 HUD，向 D30 发送 `board_failed`、`market_failed`、`tavern_failed`、`shipyard_failed` 或 `preparation_failed`，并显示“状态未改变”；
- 恢复动作自身异常时只输出带模块前缀的 warning，不再次抛出异常；
- 扩展 D31 静态审计，逐项检查五类失败事件投影；不改变 D2/D3/D4/D9/D10/D21 的业务裁决、存档、网络或事实写者。

### 32.2 额度友好验证

- 选定 D25—D31 回归：`59/59 PASS`；
- `d31_interaction_logic_audit.py`：`STATIC_LOGIC_ONLY / PASS`，29 个可见入口 + 4 个键盘入口；
- `d29_d30_interaction_matrix_audit.py`：`PASS`；
- Unity 6000.0.80f1 batchmode 编译：`Tundra build success (2.55 seconds)`，无新增 `error CS`；
- D25—D31 直接结果 JSON：R34 新增前 `62/62`，新增 R34 机器结果后 `63/63` 可解析（Unity Library 夹具 JSON 不计入）；
- 本轮未重复完整 Player、性能采样或最终画面验收，以遵守 Plus 滚动额度限制。

证据：

- [R34 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R34.json)
- [R34 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R34.log)
- [R34 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R34.log)

R34 只证明源码层异常恢复和 D30 失败投影，不把静态结果升级为 Player 运行通过。真实逐入口负向/禁用/焦点/重复点击矩阵、D30 全阶段实机异常恢复、最终低模画面质量和性能 1% low 仍未闭环；D29-G06、D30-G06、D31-G06 继续保持 `PARTIAL`。

## 33. R35 R03 港口 PBR 材质与程序网格 UV 接入（2026-09-03）

### 33.1 发现与修复

R35 在继续执行 D31 交互/视觉收口时，发现 R03 港口表现虽然已使用倒角网格，但其材质仍主要是纯色材质，程序生成的部分网格没有 UV，已批准的本地 Poly Haven 纹理候选因此没有真正参与表现。此次只处理表现生成器与编辑器生成物，不改变 D2/D3/D6/D21 owner、碰撞所有权、输入、存档、资源事实或运行时业务裁决。

- 将 12/12 个 R03 表现材质绑定到已批准的本地 PBR 底图候选，覆盖草地、泥土、沙地、石材、木板和粗木等表面类型；
- 将 6/6 个法线贴图导入元数据明确设置为线性 `NormalMap`，避免把法线当作颜色采样；
- 为岛面和程序生成的表现网格生成投影 UV，并对独立三角面按主法线方向选择平面投影，减少墙面和斜面纹理拉伸；
- 保留 R03 唯一表现根与单一碰撞所有权，材质/UV 改动限定为 presentation/editor scope，不把编辑器审计误报成 Player 画质通过。

### 33.2 额度友好验证

- Unity 6000.0.80f1 batchmode 重建 R03 Prefab：`Tundra build success (3.68 seconds)`；
- Unity Editor 审计：`315 renderers / 1 collider / 22532 mesh vertices`；
- R35 材质审计（当时的历史快照）：12/12 材质有批准的底图，6/6 法线元数据通过，Prefab 存在，结果 `PASS / EDITOR_STATIC`；后续 R52 将 `R03_Plaster` 改为明确的 authored-color 例外，当前基线由 R61 审计；
- R35 目标回归：5/5 测试通过；D31 交互静态审计、D29/D30 矩阵及选定逻辑回归继续通过；
- Unity 新鲜脚本编译成功，无新增 `error CS`；许可证访问与 Curl 无操作环境警告仍按环境诊断记录，不当作游戏运行错误。

证据：

- [R35 材质/UV 静态审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R35.json)
- [R35 材质回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_REGRESSION_R35.log)
- [R35 Prefab 重建日志](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_REBUILD_R35.log)
- [R35 Editor 审计日志](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_AUDIT_R35.log)
- [R35 Unity 编译日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R35.log)

### 33.3 当前门禁结论

R35 是一次有效的编辑器/静态表现改进，但没有重新取得 Player 画面，因此不能宣称“画面最终通过”。D26-S05、D26-G06、D31-P04、D31-S05、D31-G06 继续保持 `PARTIAL`；真实 Player 逐入口成功/失败/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终低模画面质量和性能 1% low 仍是后续门禁。下一次若有额度，应优先做一次短 Player 视觉复核，再决定是否需要进一步调整材质比例、灯光、遮挡或路线。

## 34. R36 D29 保存/整备拒绝路径与 D30 失败投影收口（2026-09-03）

### 34.1 代码层修复

R36 继续执行“先逻辑、后实机”的路线，处理了两个会让玩家误以为按钮无响应或让临时页面卡住的边界：

- D29 保存入口在 owner 返回拒绝时明确投影 `save_failed`，不显示成功；D3 保存调用和基础存档调用发生异常时统一走 `ReportActionException`，恢复主 HUD、显示“状态未改变”并记录失败反馈；
- D29 港口整备入口调用后如果没有真正建立输入焦点，会立即关闭整备临时页、恢复主 HUD，再投影 `preparation_failed`；
- D30 将 `save_failed` 纳入失败事件集合，保持当前视觉阶段，不会因为保存失败把路线条切换到错误阶段；
- 所有修改仍限定在交互表现桥，未改变 D3/D9/D30 的事实写者、金额、库存、航行、存档格式或业务裁决。

### 34.2 低成本验证

- D31 交互静态审计：`PASS`，覆盖 29 个可见入口和 4 个键盘入口；
- D29/D30 交互矩阵：`PASS`；
- R36 定向测试：`25/25 PASS`；
- Unity 6000.0.80f1 batchmode 编译：`Tundra build success`，无新增 `error CS`；
- R36 仍明确标记 `STATIC_LOGIC_ONLY`，未用代码审计冒充 Player 运行证据。

证据：

- [R36 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R36.json)
- [R36 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R36.log)
- [R36 D25—D31 全量静态回归](../../game/tools/results/D31/D31-S05/D31_S05_FULL_LOGIC_REGRESSION_R36.log)
- [R36 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R36.log)

### 34.3 状态边界

R36 关闭了保存拒绝/异常和整备焦点拒绝的代码层缺口，但不关闭真实 Player 的逐入口成功/失败/禁用/焦点/重复点击、D30 全阶段实机异常恢复、最终画面质量或性能 1% low。D29-G06、D30-G06、D31-S05、D31-G06 继续保持 `PARTIAL`，后续仍应先做一次短 Player 交互复核，再进入更重的最终画面与性能验收。

## 35. R37 D30 视觉阶段策略纯逻辑化与 D31 交互回归（2026-09-03）

### 35.1 代码层改动

R37 针对 D31 交互收口中“阶段事件逻辑只能藏在 MonoBehaviour 内、难以独立验证”的缺口做最小重构：

- 新增 `game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs`，把事件归一化、成功阶段映射、失败/未知保持当前阶段、短窗口重复事件过滤抽成纯 C# 策略；它不读取 Unity API、不改业务 owner、不写存档；
- 新增 `WhalesIslandReef.D30.VisualLogic.Runtime` 程序集，明确该策略的零 Unity 依赖；`D30VisualLoopCoordinator` 只负责读取 Unity 时间和显示策略结果；
- 新增独立 `WhalesIslandReef.D31.VisualLogic.Tests` 测试程序集，覆盖成功事件、`preparation_opened` 同阶段成功、六类失败事件、重复事件、窗口过期和未知/空事件；
- 更新静态审计和表现回归，使测试断言跟随纯逻辑所有权，不再依赖协调器内部的重复 `switch` 文本。

### 35.2 证据与结果

- Unity EditMode：`Whalebone.Tests.D31.D31VisualStagePolicyTests`，`16/16 PASS`；
- D25—D31 选定 Python 回归：`53/53 PASS`；
- Unity 6000.0.80f1 短编译：`Tundra build success`，无新增 `error CS`；
- D31 静态交互审计：`STATIC_LOGIC_ONLY / PASS`，29 个可见入口、4 个键盘入口、D30 纯策略引用和失败/重复结果均有检查；
- R37 结果 JSON：`game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R37.json`。

证据：

- [R37 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R37.json)
- [R37 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R37.log)
- [R37 Unity EditMode 结果](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R37.xml)
- [R37 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R37.log)

### 35.3 状态边界

R37 是代码层 `STATIC_LOGIC_ONLY / PASS`，说明 D30 阶段投影具备可重复的单元证据；它不等于 D29-G06、D30-G06 或 D31-G06 的运行时通过。真实 Player 的逐入口成功/失败/禁用/焦点/重复点击、D30 全阶段异常恢复、最终低模画面质量、Low/Medium/High 性能 1% low 仍由后置 Runtime/Release 门禁负责。

## 36. R38 D29 一次性请求防重纯逻辑化（2026-09-03）

### 36.1 代码层改动

R38 继续收口 D31 交互代码层，把 D29 中依赖 Unity 时间和 MonoBehaviour 的短窗口防重逻辑抽为独立策略：

- 新增 `game/Assets/Scripts/Gameplay/D29/Logic/D29OneShotPolicy.cs`，只判断同一表现动作是否在 0.35 秒防重窗口内，不替代 owner 的幂等键、权限或业务拒绝；
- 新增 `WhalesIslandReef.D29.InteractionLogic.Runtime` 零 Unity 依赖程序集，D29 HUD 通过 `D29OneShotPolicy.Evaluate` 消费结果；
- 扩展 D31 独立 EditMode 测试，覆盖首次请求、同动作窗口内重复、不同动作、窗口外重试和空动作；
- 静态审计新增 D29 纯策略存在性与实际调用检查，防止后续把防重逻辑重新写回 UI 事件方法。

### 36.2 证据与结果

- Unity EditMode：`Whalebone.Tests.D31.D31VisualStagePolicyTests`，`21/21 PASS`；
- D25—D31 选定 Python 回归：`53/53 PASS`；
- Unity 6000.0.80f1 短编译：`Tundra build success`，无新增 `error CS`；
- D31 静态交互审计：`STATIC_LOGIC_ONLY / PASS`，继续覆盖 29 个可见入口和 4 个键盘入口；
- R38 结果 JSON：`game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R38.json`。

证据：

- [R38 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R38.json)
- [R38 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R38.log)
- [R38 Unity EditMode 结果](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R38.xml)
- [R38 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R38.log)

### 36.3 状态边界

R38 是代码层 `STATIC_LOGIC_ONLY / PASS`，证明 D29 防重和 D30 阶段投影都能在独立 Unity 测试程序集内重复验证；它不等于 D29-G06、D30-G06 或 D31-G06 的运行时通过。真实 Player 的逐入口负向/禁用/重复点击、D30 全阶段异常恢复、最终画质和性能 1% low 仍保持后置门禁。

## 37. R40 D31 交互结果反馈与短时 Player 回调探针（2026-09-03）

### 37.1 代码层修补

本轮只处理交互层的可读反馈，不改 D3/D4/D9 的业务裁决：

- D3 商品上一种/下一种现在写入明确的“已切换商品”结果提示；
- D4 势力、炮战、装填、弹种、开炮、接舷、脱离、改装、动物、医疗、主线和保存动作在状态变化后立即刷新 HUD；
- D9 酒馆切页和整备结果在 owner 返回后立即刷新提示，避免动作已执行但当前帧仍显示旧消息；
- D31 交互探针的反馈采集改为收集并排序全部相关 Text，避免全局状态栏前六项把 D3/D4/D9 的结果文字挤出证据；
- 新增显式命令行输出目录参数，保存入口只做存在性检查，探针不写正式场景资产或正式存档。

### 37.2 额度友好验证

- Unity 6000.0.80f1 短编译：`Tundra build success`，`error CS=0`；
- Unity EditMode：`Whalebone.Tests.D31.D31VisualStagePolicyTests`，`21/21 PASS`；
- D25—D31 选定静态回归：`54/54 PASS`；
- 独立 Windows x64 Player 短时 UGUI 回调探针：预期 `47`、找到 `47`、可观察变化 `43`、保存存在性 `1`、缺失 `0`、禁用 `0`、调用异常 `0`；
- 探针运行环境记录为 `1920×1080`、`NVIDIA GeForce RTX 4070 Ti`、`Direct3D11`，初始场景 `MainMenu`，最终场景 `InitialHarbor`；运行日志异常扫描为 0；
- 本轮未执行物理鼠标/键盘矩阵、完整异常恢复路线、长时间 Player 或性能采样，以控制 Plus 滚动额度。

证据：

- [R40 Player 探针汇总](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R40.json)
- [R40 Player 原始结果](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R40.json)
- [R40 探针源码](../../game/Assets/Scripts/Gameplay/D31/D31InteractionEvidenceProbe.cs)
- [R40 Player 构建日志](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R40.log)
- [R40 Player 运行日志](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_RUNTIME_R40.log)
- [R40 静态交互审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R40.json)
- [R40 Unity EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R40.xml)
- [R40 D25—D31 回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R40.log)
- [R40 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R40.log)

### 37.3 当前门禁结论

R40 证明了当前独立 Player 的 UGUI 回调链和大部分可见结果反馈可以运行；它不是物理 OS 输入证据。`D29-G06`、`D30-G06`、`D31-S05`、`D31-G06` 继续保持 `PARTIAL`。仍未关闭：D29 市场重复点击的专门负向证据、已自动泊位状态下 D9 泊位操作的可区分幂等反馈、逐按钮禁用/失败/焦点矩阵、D30 全阶段异常恢复、最终画质质量与 Low/Medium/High 的性能 1% low 门禁。

## 38. R43 D31 交互回调复核、主 HUD 恢复诊断与防重证据（2026-09-03）

### 38.1 本轮最小修补

R43 只处理探针与表现层证据的一致性，不改变 D3/D4/D9 业务 owner：

- D31 交互探针在第一次打开市场后立即关闭 D3 临时页、恢复主 HUD，再执行第二次 Market 调用；这样第二次请求真正到达 D29 一次性防重策略，不会被隐藏父级 `CanvasGroup` 的不可交互状态吞掉；
- `IsPrimaryHudRestored()` 改为检查所有同名的活动 Canvas，只要存在一个真实可见、可交互并阻挡射线的 D29 主 Canvas 即视为恢复；这与活动 Canvas 读取规则一致，避免兼容对象顺序造成假阴性；
- D31 Player 构建器与证据路径升级为 R43，保存入口仍只做存在性检查，不执行正式存档写入；
- 不修改正式场景资产、Prefab、业务数值、存档或外部网络状态。

### 38.2 R43 证据

- 独立 Windows x64 Player：`47/47` 控件找到；`46` 个观察到可见回调/反馈，其中 `1` 个为重复保护；保存入口 `1` 个为 `PRESENT_ONLY_NO_MUTATION`；`0` 缺失、`0` 禁用、`0` 调用异常、`0` 无可见反馈；
- D29 仓库关闭后确认主 HUD 已恢复；D29 市场重复调用观察到“重复点击已忽略”路径；D29/D3/D4/D9/D10 五个表面均进入探针范围；
- Player 环境：`1920×1080`、`NVIDIA GeForce RTX 4070 Ti`、`Direct3D11`，`MainMenu -> InitialHarbor`；运行日志异常级 `Exception/Error/MissingComponent/Crash` 扫描为 `0`；
- Unity EditMode：`Whalebone.Tests.D31.D31VisualStagePolicyTests` 为 `21/21 PASS`；
- D25—D31 选定 Python 回归：`54/54 PASS`；D31 静态交互审计：`STATIC_LOGIC_ONLY / PASS`；Unity 编译：`Tundra build success`，`error CS=0`。

证据：

- [R43 Player 探针汇总](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R43.json)
- [R43 Player 原始结果](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R43.json)
- [R43 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R43.json)
- [R43 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R43.log)
- [R43 Unity EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R43.xml)
- [R43 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R43.log)
- [R43 Player 构建日志](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R43.log)
- [R43 Player 运行日志](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_RUNTIME_R43.log)

### 38.3 当前门禁结论

R43 关闭了 R40 遗留的两个探针假阴性：仓库关闭后的主 HUD 恢复判定，以及市场重复点击的回调可达性。D31 代码层和短时 UGUI 回调链已经有一致证据，但 `D31-S05`、`D31-G06`、`D29-G06`、`D30-G06` 仍保持 `PARTIAL`，因为 `pointerClick` 不是物理 OS 输入。后置门禁仍包括真实物理成功/失败/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终画面质量，以及 Low/Medium/High 的性能 1% low；本轮不以短探针替代这些门禁。

## 39. R45 D29 临时面板互斥修复与 D9 离港覆盖（2026-09-03）

### 39.1 本轮发现与修复

R44 补齐 D9 `Depart` 后，短时 Player 证据暴露了一个真实的 UI 结构缺陷：D29 的运行时控制器宿主对象和它创建的子 Canvas 都叫 `D29InteractionHudCanvas`。`GameObject.Find` 可能先找到没有 `Canvas` 组件的宿主，协调器因此只修改宿主的 `CanvasGroup`，实际子 Canvas 仍保持可见，临时市场/船坞/酒馆/整备页就会与主 HUD 叠加。

R45 做了两项范围受限修复：

- D29 宿主改名为 `D29InteractionHudController`，实际 UI Canvas 保留 `D29InteractionHudCanvas`；
- `RuntimeHudCoordinator` 同时处理直接命中的对象和全部同名 `Canvas`，避免场景切换或兼容对象造成第二个可见/可点击层；
- D31 探针新增 `D9-DEPART`，放在所有港内整备用例完成之后，避免离港改变 D3 状态影响前序用例。

本轮没有改 D3/D4/D9 的业务裁决、数值、存档或正式场景资产。

### 39.2 证据

- [R45 交互汇总](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_RUNTIME_R45.json)
- [R45 Player 原始结果](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_EVIDENCE_PLAYER_R45.json)
- [R45 逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R45.json)
- [R45 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R45.log)
- [R45 EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R45.xml)
- [R45 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R45.log)
- [R45 Player 构建](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_BUILD_R45.log)
- [R45 Player 运行](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_PLAYER_RUNTIME_R45.log)

R45 结果：48/48 控件找到，47 个用例产生可观察结果（重复保护 1 个），保存入口 1 个仍只做存在性检查；missing/disabled/invocation error/no-visible-feedback 均为 0。市场、船坞、酒馆和整备页打开时活动 Canvas 只保留对应临时页，关闭后 `D29InteractionHudCanvas` 恢复；D9 离港状态也被实际观察到。D31 静态审计 PASS，D25—D31 选定回归 `55/55 PASS`，Unity EditMode `21/21 PASS`，编译 `error CS=0`，运行日志异常级扫描为 0。

### 39.3 门禁边界

R45 只关闭了一个真实的代码/UGUI 层叠缺陷，并补齐了探针覆盖；它不等于 `VISUAL_CANDIDATE` 或 `RuntimeApproved`。`D31-S05`、`D31-G06`、`D29-G06`、`D30-G06` 仍为 `PARTIAL`，因为物理 OS 输入的成功/失败/禁用/焦点/重复点击矩阵、D30 全阶段异常恢复、最终低模画面质量以及 Low/Medium/High 的 1% low 性能门禁仍未完成。保存按钮本轮仍不执行正式写盘，避免污染用户存档。

## 40. R46 D30 失败事件全视觉阶段逻辑收口（2026-09-03）

### 40.1 本轮目标与实现

本轮继续按“先完成代码与纯逻辑，再把实机演示和性能放到后置门禁”的路线处理 D30 异常恢复。D30 的失败事件分类不再只依赖成功事件 `switch` 的隐式 `default`，而是由 `D30VisualStagePolicy.IsFailureEvent(string eventId)` 提供明确、可测试的失败白名单；`Evaluate` 对未知事件仍保持 `Unknown`，只有明确失败事件才投影为 `Failed` 并保持当前视觉阶段。

### 40.2 逻辑覆盖与证据

- 6 类已登记失败事件 × 7 个视觉阶段 = 42 个组合全部验证：失败事件始终保持当前视觉阶段，结果为 `Failed`；
- 空事件、成功事件、未知事件不会误判为失败；
- Unity EditMode `23/23 PASS`；D25—D31 选定 Python 回归 `55/55 PASS`；D31 静态审计 `PASS`；Unity 短编译成功，`error CS=0`；
- 本轮没有重新运行 Player，也没有宣称物理输入、最终画质或性能通过。

证据：

- [R46 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R46.json)
- [R46 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R46.log)
- [R46 Unity EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R46.xml)
- [R46 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R46.log)

### 40.3 门禁边界

R46 只关闭了 D30 失败事件到视觉阶段保持的源码层缺口，不升级 `D30-G06`、`D31-S05` 或 `D31-G06`。仍待后置处理的内容是 Player 异常注入、真实物理鼠标/键盘的成功/失败/禁用/焦点/重复点击矩阵、最终低模画面质量，以及 Low/Medium/High 的 1% low 性能门禁。由于当前目标是控制 Plus 滚动额度并优先完成逻辑层，本轮不启动长时间实机采样。

## 41. R47 D29 动态可用性与键盘/手柄焦点导航修复（2026-09-03）

### 41.1 发现与修复

代码复核发现：D29 在 owner 延迟就绪后会更新 `Button.interactable`，但 D16 模态焦点组件原先只在首次挂载/启用时建立 `Selectable` 导航列表。这样按钮虽然恢复可点击，仍可能无法通过键盘或手柄焦点导航到达。

R47 做了范围受限修复：

- `D16ModalFocusScope` 新增 `RefreshNavigation()`，重新收集当前活动且可交互的控件，并重建显式上下左右导航；
- D29 保存主 HUD 的焦点范围，在 owner 驱动的可用性签名变化后刷新导航；
- 不修改交易、航行、天气、库存、关系、存档、场景事实或网络边界。

### 41.2 证据

- [R47 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R47.json)
- [R47 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R47.log)
- [R47 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R47.log)

验证结果：针对性 D29/D31 Python 测试 `24/24 PASS`；D25—D31 选定测试 `51/51 PASS`；D31 静态交互审计 `PASS`；Unity `Tundra build success (2.46 seconds)`，`error CS=0`。

### 41.3 门禁边界

R47 关闭了动态焦点导航的源码缺口，但仍是 `STATIC_LOGIC_ONLY`。不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`；真实 Player 的物理输入矩阵、异常注入、最终画质和性能门禁继续后置。

## 42. R48 临时 Canvas 初始化可见性与 HUD 隔离修复（2026-09-03）

### 42.1 发现与修复

复核发现 D4、D9、D10 的临时页面在 `Awake` 创建时默认启用 Canvas。即使 `RuntimeHudCoordinator` 通常会在下一帧隐藏它们，启动窗口仍可能先绘制临时页面的标题、目录或提示，造成主 HUD 与临时页短暂叠层。

R48 将 `D4Canvas`、`D9PortServicePage` 和 `D10TavernSocialPage` 的 Canvas 初始状态设为禁用。打开页面时仍由 `RuntimeHudCoordinator.ShowTemporaryCanvas(...)` 统一启用，关闭页面时由统一恢复入口隐藏；不修改任何领域事实、业务数值、存档或场景资产。

### 42.2 证据

- [R48 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R48.json)
- [R48 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R48.log)
- [R48 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R48.log)

验证结果：针对性 D29/D31 Python 测试 `25/25 PASS`；D25—D31 选定测试 `52/52 PASS`；D31 静态交互审计 `PASS`；Unity `Tundra build success (2.88 seconds)`，`error CS=0`。

### 42.3 门禁边界

R48 关闭了临时 Canvas 初始化闪现的源码缺口，但仍是 `STATIC_LOGIC_ONLY`。不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`；真实 Player 物理输入、异常注入、最终画质和性能门禁继续后置。

## 43. R49 跨阶段注册表契约回归修复（2026-09-03）

### 43.1 发现与修复

定向回归暴露出 D16-C08 垂直切片落地后，旧 D15 校验器仍按“普通占位条目”解释四个已审查切片：圣玛丽岛使用了切片专用 LOD/Unity 资源路径，码头与小艇泊位只有一个真实运行时锚点，四条许可证记录还缺少 `attribution_required` 布尔值。R49 只做证据绑定的兼容修复：

- D15-A01 仅允许带 D16-C08 审查证据的 `island_saint_marys` 使用切片 LOD 与资源路径；
- D15-A04 仅允许同类 reviewed slice override 使用一个运行时锚点，普通建筑仍要求至少三个；
- 四个 D16-C08 注册条目补齐 `license.attribution_required: false`；
- 测试中明确验证例外必须绑定审查证据，不能扩张为任意 `integrated` 放行。

### 43.2 证据

- [R49 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R49.json)
- [R49 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R49.log)

验证结果：跨阶段定向测试 `40/40 PASS`；D25—D31 选定回归 `93/93 PASS`；全量 Python 回归 `1026/1026 PASS`；D31 静态交互审计 `PASS`；AgentCore `dotnet build` 为 `0 warnings / 0 errors`；`git diff --check` 通过。

### 43.3 门禁边界

R49 只修复代码/数据契约回归，不升级任何正式视觉或运行时门禁。`D29-G06`、`D30-G06`、`D31-S05`、`D31-G06` 仍保持 `PARTIAL`；物理 OS 输入、Player 负向/禁用/重复点击、运行时异常注入、最终画质和性能 1% low 继续后置。

## 44. R50 R03 建筑重复标牌清理（2026-09-03）

### 44.1 发现与修复

R29 固定镜头复核发现，四栋程序化建筑同时生成前后两块同名标牌；在码头斜视角下，后立面标牌会从建筑侧面露出，形成 `MARKET`、`WAREHOUSE`、`TAVERN`、`SHIPYARD` 的重复文字，放大了“UI/场景堆叠”的观感问题。R50 仅删除 `BackSignBoard` 与 `BackSign`，保留后立面门、窗、棚架和建筑几何；不修改交互锚点、碰撞、业务 owner、存档或天气逻辑。

### 44.2 证据

- 生成器：[D19R03HarborPresentationBuilder.cs](../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- Unity 重建：[R50 Unity visual rebuild](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R50.log)
- Unity 审计：[R50 Unity visual audit](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R50.log)
- 回归日志：[R50 visual/interaction regression](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R50.log)
- Editor 结果：[D19 R03 Editor audit](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- 源码回归：[D19 R03 builder tests](../../tools/tests/test_d19_r03_visual_builder.py)

验证结果：生成器成功重建并写入 `D19_R03_HarborPresentation.prefab`；Editor 审计为 `307 Renderer / 22004 mesh vertices / 1 Collider`；Prefab 中 `BackSign` 标记为 `0`；D28—D31 相关回归 `47/47 PASS`（其中 D19 R03 builder 测试 5/5）；Unity batchmode 重建与审计退出码均为 0，未见新增 `error CS`。随后 R50 使用新构建的独立 Windows x64 Player 重新采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，分辨率为 `1920×1080`，截图为真实 backbuffer 输出。

### 44.3 门禁边界

R50 是表现层可读性修复，状态为 `PASS_WITHIN_SCOPE`，不等于最终画质通过。Player 复验已经完成并确认重复标牌已清理，但画面仍有低模/程序化建筑、平坦地表、杂乱竖向几何、粗糙环境道具和高密度 HUD；因此 `D25-S06/G07`、`D26-S05/G06`、`D27-S05/G06`、`D28-S05/G06`、`D29-S05/G06`、`D30-S05/G06`、`D31-S05/G06` 维持既有状态。物理 OS 输入、D30 实机异常恢复、最终画质和性能 1% low 仍未闭环。

R50 Player 证据：`game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R50.json`、`game/tools/results/D31/D31-P04/D31_P04_R50_REVALIDATION_RESULT.json`、`game/tools/results/D31/D31-P04/visual_evidence_R50/`、`game/tools/results/D31/D31-P04/D31_P04_BUILD_PLAYER_R50.log`、`game/tools/results/D31/D31-P04/D31_P04_VISUAL_PLAYER_R50.log`。

## 45. R51 固定机位棕榈遮挡收敛（2026-09-03）

### 45.1 发现与修复

R50 的真实 Player 画面显示，固定市场机位几乎贴着东侧棕榈树，树干在画面左右形成大型竖向遮挡。R51 只调整 R03 表现生成器中的棕榈表现布局：将靠近固定机位的棕榈移到画面边缘，并把表现树干半径从 `0.22` 收窄为 `0.14`。未修改玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。

### 45.2 证据

- 生成器：[D19R03HarborPresentationBuilder.cs](../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- 静态回归：[R51 visual/interaction regression](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R51.log)
- Unity 重建：[R51 Unity visual rebuild](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R51.log)
- Unity 审计：[R51 Unity visual audit](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R51.log)
- Player 证据：[R51 Player visual evidence](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R51.json)
- R51 复验结论：[D31_P04_R51_REVALIDATION_RESULT.json](../../game/tools/results/D31/D31-P04/D31_P04_R51_REVALIDATION_RESULT.json)
- 截图目录：[R51 visual evidence](../../game/tools/results/D31/D31-P04/visual_evidence_R51/)

验证结果：D19 R03 builder 与 D28—D31 相关回归 `48/48 PASS`；Unity 重建/审计完成，Editor 审计仍为 `307 Renderer / 22004 mesh vertices / 1 Collider`；真实独立 Windows x64 Player 在 `1920×1080` backbuffer 采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头。固定市场机位的近景树干遮挡明显减少。

### 45.3 门禁边界

R51 状态为 `PASS_WITHIN_SCOPE`，不等于最终画质通过。真实画面仍有低模/程序化建筑、平坦地表、少量边缘竖向几何、粗糙环境道具和高密度 HUD；D25—D31 的正式视觉、交互负向矩阵、D30 异常恢复和性能 1% low 门禁继续保持 `PARTIAL`。

## 46. R52 建筑灰泥材质误用修复（2026-09-03）

### 46.1 发现与修复

R51 Player 复核发现 `R03_Plaster` 仍沿用旧的泥地 BaseMap/NormalMap，导致市场和酒馆墙面看起来像岩土代理体。R52 在表现生成器的材质配置阶段对 `R03_Plaster` 清除旧 `BaseMap`、`MainTex` 和 `BumpMap`，保留 `Material(...)` 设置的暖色灰泥 `BaseColor`。未修改玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。

### 46.2 证据

- 生成器：[D19R03HarborPresentationBuilder.cs](../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- 静态回归：[R52 visual/interaction regression](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R52.log)
- Unity 重建：[R52 Unity visual rebuild](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R52.log)
- Unity 审计：[R52 Unity visual audit](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R52.log)
- Player 证据：[R52 Player visual evidence](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R52.json)
- R52 复验结论：[D31_P04_R52_REVALIDATION_RESULT.json](../../game/tools/results/D31/D31-P04/D31_P04_R52_REVALIDATION_RESULT.json)
- 截图目录：[R52 visual evidence](../../game/tools/results/D31/D31-P04/visual_evidence_R52/)

验证结果：D19 R03 builder 与 D28—D31 相关回归 `49/49 PASS`；Unity 重建/审计完成，Editor 审计为 `307 Renderer / 22004 mesh vertices / 1 Collider`；真实独立 Windows x64 Player 在 `1920×1080` backbuffer 采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，墙面不再使用泥地贴图。

### 46.3 门禁边界

R52 状态为 `PASS_WITHIN_SCOPE`，不等于最终画质通过。建筑轮廓、地形层次、岩石/道具细节、少量边缘几何和 HUD 分层仍未达到最终 Visual Candidate；D25—D31 正式视觉、交互负向矩阵、D30 异常恢复和性能 1% low 门禁继续保持 `PARTIAL`。

## 47. R53 岸线岩石轮廓从三角锥退化修复（2026-09-03）

### 47.1 目标

修复 R50—R52 之后仍明显可见的单一三角锥岩石代理体，保持本轮只触及 R03 表现生成器，不改变玩法、交互锚点、碰撞、业务 owner、存档或天气逻辑。该补充任务服务于 D31-P04 的视觉缺陷收敛，不把局部修复误报为整体画质通过。

### 47.2 实施与验证

- 修改 `game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs` 的 `MakeRockMesh`：使用 8 段、三环不规则轮廓，带顶部和封闭底部，替代六顶点三角锥。
- 静态回归：50 个 D19 R03 / D28—D31 相关测试全部通过。
- Unity 重建与审计完成：307 个 Renderer、24884 个网格顶点、1 个 Collider；无新增 `error CS`。
- 真实独立 Windows x64 Player 复验完成：14/14 固定镜头、3/3 Calm/Rain/Storm 天气镜头，真实 1920×1080 backbuffer PNG 全部落盘。
- 结果：岩石岸线轮廓更连续，但建筑与屋顶仍以低模/程序化盒体为主，地表/远景偏平，环境道具与 HUD 分层仍不足。

证据：

- 静态回归：[R53 visual/interaction regression](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R53.log)
- Unity 重建：[R53 Unity visual rebuild](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R53.log)
- Unity 审计：[R53 Unity visual audit](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R53.log)
- Player 证据：[R53 Player visual evidence](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R53.json)
- R53 复验结论：[D31_P04_R53_REVALIDATION_RESULT.json](../../game/tools/results/D31/D31-P04/D31_P04_R53_REVALIDATION_RESULT.json)
- 截图目录：[R53 visual evidence](../../game/tools/results/D31/D31-P04/visual_evidence_R53/)

### 47.3 门禁边界

R53 状态为 `PASS_WITHIN_SCOPE`，D31-P04、D31-S05 和 D31-G06 仍为 `PARTIAL`。本轮只证明局部岩石几何修复、代码回归、Editor 重建和 Player 镜头证据；最终 Visual Candidate 仍受低模/程序化建筑、地形层次、粗糙环境道具、高密度 HUD、物理输入矩阵、D30 实机异常恢复和性能 1% low 约束。

## 48. R54—R57 港口大气、建筑轮廓与招牌可读性收敛（2026-09-03）

### 48.1 目标与范围

R53 之后的短复核仍显示三个具体问题：天空盒地平线偏棕、四栋建筑的大体块仍像无细节代理盒、从建筑背面看会出现穿墙镜像招牌。R54—R57 继续采用小步、可回滚、表现层单一写者策略，只修改 D25 表现主题、R03 表现生成器和一个无业务状态的招牌可见性组件；不修改玩法规则、交互锚点、碰撞 owner、存档、天气事实或任何领域裁决系统。

### 48.2 实施内容

- R54：在 `D25VisualTheme.ApplyHarborAtmosphere` 中调校现有天空盒的 `_GroundColor`、`_SkyTint` 和 `_Exposure`，消除港口画面下半部的棕色 void 感；不替换天空盒资源，也不改变天气事实。
- R55：在 `D19R03HarborPresentationBuilder.CreateHouse` 中加入地基、山墙屋顶、屋脊、立面木构压条和侧窗板，打破大面积平面；Editor 审计由 R53 的 `307 Renderer / 24884 mesh vertices` 增至 `367 Renderer / 32828 mesh vertices`，碰撞仍为 `1`。
- R56：首次尝试将 TextMesh 旋转 180°，发现从出生镜头看招牌镜像；已撤回该错误旋转并保留字体放大。
- R57：新增 `D19R03LandmarkLabelVisibility`，只在摄像机位于招牌所属 -Z 立面前方时启用文字，背面时隐藏，避免透明 TextMesh 穿过建筑显示镜像；该组件不写入任何玩法/领域状态。

### 48.3 证据与结果

- 静态回归：[R57 visual/interaction regression](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R57.log)
- Unity 重建：[R57 Unity visual rebuild](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R57.log)
- Unity 审计：[R57 Unity visual audit](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R57.log)
- Editor 结果：[D19 R03 Editor audit](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- Player 构建：[R57 Player build](../../game/tools/results/D31/D31-P04/D31_P04_BUILD_PLAYER_R57.log)
- Player 运行日志：[R57 Player visual log](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_PLAYER_R57.log)
- Player 证据：[R57 Player visual evidence](../../game/tools/results/D31/D31-P04/D31_P04_VISUAL_EVIDENCE_PLAYER_R57.json)
- R57 复验结论：[D31_P04_R57_REVALIDATION_RESULT.json](../../game/tools/results/D31/D31-P04/D31_P04_R57_REVALIDATION_RESULT.json)
- 截图目录：[R57 visual evidence](../../game/tools/results/D31/D31-P04/visual_evidence_R57/)

验证结果：D25—D31 选定静态回归 `55/55 PASS`；Unity R03 重建/审计退出码为 0 且无新增 `error CS`；Prefab 空 `m_Name`/`m_EditorClassIdentifier` 字段为 0；真实独立 Windows x64 Player 在 `1920×1080` backbuffer 下采集 `14/14` 固定镜头和 `3/3` Calm/Rain/Storm 天气镜头，全部文件存在。出生镜头招牌可读，市场背面镜头不再显示穿墙镜像文字。

### 48.4 门禁边界

R54—R57 均为 `PASS_WITHIN_SCOPE`，不是最终视觉门禁通过。`D25-S06`、`D31-P04`、`D31-S05` 和 `D31-G06` 继续保持 `PARTIAL`：整体场景仍是低模/程序化风格，地形层次、环境道具和 HUD 仍需后续美术收敛；物理 OS 输入负向/禁用/重复矩阵、D30 实机异常恢复和性能 1% low 也未关闭。本轮为保护 Plus 滚动额度，没有重复长路线或性能采样。

## 49. R58 D29 仓库 owner 就绪边界修复（2026-09-03）

### 49.1 发现与修复

代码审计发现 D29 仓库按钮虽然显示在主 HUD 中，却没有像市场、酒馆、船坞和整备一样绑定 D3 owner readiness；当 D3 尚未完成启动时，鼠标或键盘 `I` 可能打开一个没有真实库存快照的空模态。R58 在 `D29InteractionHud` 中保存仓库按钮引用，将 `Button.interactable` 绑定到 D3 readiness，并在 `ToggleInfo` 的鼠标/键盘共用入口增加 owner 前置判断。关闭已经打开的仓库仍然可用，避免把错误修复变成无法退出的模态。

### 49.2 证据

- 源码：[D29InteractionHud.cs](../../game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs)
- 测试：[D29 interaction HUD tests](../../tools/tests/test_d29_interaction_hud.py)
- 逻辑回归：[R58 interaction logic regression](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R58.log)
- Unity 编译：[R58 Unity compile](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R58.log)
- D29-S05 结果：[D29_S05_RESULT.json](../../game/tools/results/D29/D29-S05/D29_S05_RESULT.json)
- D29-G06 结果：[D29_G06_RESULT.json](../../game/tools/results/D29/D29-G06/D29_G06_RESULT.json)

验证结果：D29 定向回归 `30/30 PASS`；D25—D31 选定回归 `56/56 PASS`；D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`；Unity Tundra 短编译成功，`error CS=0`，退出码为 0。R58 未修改领域事实、存档、网络、交互锚点或场景资产。

### 49.3 门禁边界

R58 为 `PASS_WITHIN_SCOPE`，只关闭仓库入口 owner readiness 与空模态代码缺口。`D29-S05`、`D29-G06`、`D31-S05` 和 `D31-G06` 仍保持 `PARTIAL`；真实 Player 的逐入口物理输入矩阵、D30 实机异常恢复、最终视觉质量和性能 1% low 仍需后续独立证据。

## 50. R59 D30 未知事件防重分类修复（2026-09-03）

### 50.1 发现与修复

代码复核发现 `D30VisualLoopCoordinator.NotifyInternal` 会把所有非空事件 id 交给阶段策略的防重参数。阶段策略原先只判断“id 相同且时间很近”，没有先确认事件是否属于已知路线事件，因此两个连续的未知事件会被第二次显示为“重复事件已忽略”，而不是明确的未知/未通过反馈。R59 在零 Unity 依赖的 `D30VisualStagePolicy` 中加入已知成功/失败事件分类：只有 `market_opened`、`tavern_opened`、`shipyard_opened`、`preparation_opened`、`boarded`、`departed`、`returned` 以及失败白名单事件可进入防重窗口；未知和空事件始终返回 `Unknown` 并保持当前阶段。没有移动任何业务裁决权，也没有修改存档、场景、交互锚点或网络代码。

### 50.2 证据

- 源码：[D30VisualStagePolicy.cs](../../game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs)
- EditMode 测试：[D31VisualStagePolicyTests.cs](../../game/Assets/Tests/EditMode/D31/D31VisualStagePolicyTests.cs)
- 静态交互审计：[R59 audit](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R59.json)
- 逻辑回归：[R59 regression](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R59.log)
- Unity 编译/测试摘要：[R59 Unity evidence](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R59.log)
- Unity 原始测试结果：[R59 EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R59.xml)

验证结果：D29/D30/D31 定向回归 `38/38 PASS`，D25—D31 选定回归 `56/56 PASS`；`d31_interaction_logic_audit.py` 为 `PASS / STATIC_LOGIC_ONLY`；Unity EditMode `24/24 PASS`；Tundra 编译成功、`error CS=0`、退出码为 0；`git diff --check` 通过。

### 50.3 门禁边界

R59 是代码层交互反馈分类修复，状态为 `PASS_WITHIN_SCOPE`。它不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`：真实 Player 物理 OS 输入的成功/失败/禁用/焦点/重复矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍需后置独立证据。

## 51. R60 D28 天气 owner 启动就绪边界修复（2026-09-03）

### 51.1 发现与修复

复核 D29 天气入口时发现，`FindFirstObjectByType` 可以在 D28 `Start` 执行前找到表现组件；此时 `TryCycleWeather()` 会返回成功，但随后 D28 `Start` 会创建降雨层并把天气初始化为 Calm，造成“入口刚成功、状态又被覆盖”的启动竞态。R60 在 `D28WaterWeatherPresentationRuntime` 中增加只读 `IsReady`，仅在 `BuildPrecipitation`、水面接线尝试和 Calm 初始应用完成后置为就绪；`TryCycleWeather()`、D29 天气按钮的 `interactable`、可用性签名和不可用提示统一使用该边界。没有修改天气事实、航行风险、存档或业务 owner。

### 51.2 证据

- D28 源码：[D28WaterWeatherPresentationRuntime.cs](../../game/Assets/Scripts/Gameplay/D28/D28WaterWeatherPresentationRuntime.cs)
- D29 源码：[D29InteractionHud.cs](../../game/Assets/Scripts/Gameplay/D29/D29InteractionHud.cs)
- 静态交互审计：[R60 audit](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R60.json)
- 逻辑回归：[R60 regression](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R60.log)
- Unity 编译/测试摘要：[R60 Unity evidence](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R60.log)
- Unity 原始测试结果：[R60 EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R60.xml)

验证结果：D29/D28 定向回归 `40/40 PASS`；D25—D31 选定回归 `58/58 PASS`；`d31_interaction_logic_audit.py` 为 `PASS / STATIC_LOGIC_ONLY`；Unity EditMode `24/24 PASS`；Tundra 编译成功、`error CS=0`、退出码为 0；`git diff --check` 通过。

### 51.3 门禁边界

R60 是启动时序和入口可用性修复，状态为 `PASS_WITHIN_SCOPE`。它不升级 `D28-S05`、`D28-G06`、`D29-G06`、`D31-S05` 或 `D31-G06`：真实 Player 物理输入矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍需后置独立证据。

## 52. R61 D35 材质审计基线与 R52 灰泥例外对齐（2026-09-03）

### 52.1 发现与修复

全量 Python 回归发现，`tools/d35_r03_surface_material_audit.py` 仍沿用 R35 的旧假设：12 个 R03 材质全部必须有纹理、Prefab 必须为 315 Renderer。R52 已有意清除 `R03_Plaster` 的泥地 BaseMap/NormalMap，恢复暖色 authored color；R55—R57 又增加了建筑轮廓与立面细节，使当前 Prefab 基线变为 367 Renderer / 32828 mesh vertices / 1 Collider。旧审计因此把正确的当前状态误报为失败。

R61 只修复审计契约，不把例外扩大到任意材质：

- 11 个材质继续要求本地批准底图和纹理中性 BaseColor；
- `R03_Plaster.mat` 是唯一允许无纹理的 authored-color 材质，并要求保留明确的灰泥颜色；
- 当前 Editor 审计基线更新为 367 Renderer、1 Collider、至少 32000 个网格顶点；
- 输出增加 `color_only_material_count`，避免以后把“无纹理”与“缺失材质”混为一谈。

### 52.2 证据

- 审计工具：[d35_r03_surface_material_audit.py](../../tools/d35_r03_surface_material_audit.py)
- 回归测试：[test_d35_r03_surface_material_audit.py](../../tools/tests/test_d35_r03_surface_material_audit.py)
- 当前审计：[R61 材质审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R61.json)
- 当前回归：[R61 材质回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_REGRESSION_R61.log)

验证结果：材质审计 `PASS / EDITOR_STATIC`；11/11 纹理材质、1/1 authored-color 灰泥例外、6/6 线性法线元数据通过；R03 Editor 基线为 367 Renderer / 1 Collider / 32828 mesh vertices；D35 与 D19 R03 定向测试 `11/11 PASS`；全量 Python 回归 `1035/1035 PASS`。

### 52.3 状态边界

R61 关闭了一个会阻断全量逻辑回归的过期审计规则，不修改运行时业务、交互、碰撞、存档或网络代码，也不升级 `D31-P04`、`D31-S05` 或 `D31-G06`。真实 Player 物理输入、D30 实机异常恢复、最终画质和性能 1% low 仍由后置门禁负责。

## 53. R62 R03 有机岛面轮廓与环岛水面（2026-09-03）

### 53.1 发现与修复

复核现有 Player 画面时，仍能从港口外围看出 R03 岛面由方形 26×26 网格直接铺设，且 `Water` 只覆盖南侧窄条；这正是“岛像方块、海面像一块背景板”的代码层根因。R62 保留原有网格密度、Renderer 数量和唯一碰撞体所有权，只改变表现几何：

- 用方形网格到圆盘/椭圆的确定性映射替换直接的 `x/z` 方格坐标；
- 用 3/5 次谐波的轻微边界扰动打破规则圆形，但保持可复现；
- 在同一个岛面 Mesh 中加入浅色岸线裙边，不新增 Renderer 或 Collider；
- 将现有命名为 `Water` 的表现网格扩展为 52×52 环岛水面，继续由 D28 水面 owner 读取；
- 不改变 D2/D6 行走地面、航行、交互锚点、存档、天气事实或任何业务 owner。

### 53.2 证据

- 表现生成器：[D19R03HarborPresentationBuilder.cs](../../game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs)
- 视觉构建日志：[R62 Unity 重建](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R62.log)
- R62 静态回归：[R62 视觉回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R62.log)
- Editor 审计：[R03 Editor 审计](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- 生成器测试：[D19 R03 builder tests](../../tools/tests/test_d19_r03_visual_builder.py)
- D31 结果：[D31-S05 result](../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

验证结果：Python 生成器/材质/交互定向测试 `16/16 PASS`；Unity Editor 构建退出码为 `0`；R03 审计为 `367 Renderer / 1 Collider / 33084 mesh vertices`；D35 材质审计继续 `PASS / EDITOR_STATIC`。本轮没有重复 Player 长路线、物理输入矩阵或性能采样。

### 53.3 状态边界

R62 是表现几何层修复，状态为 `PASS_WITHIN_SCOPE`。它只消除方形岛面和单侧水面这两个可定位的代码层缺陷，不把 Editor 结果升级为最终 Visual Candidate；真实 Player 画质、物理输入、D30 异常恢复和性能 1% low 仍由后置门禁负责。

## 54. R62 最终低成本代码层验收（2026-09-03）

### 54.1 验收结果

R61/R62 修改后的最后一次轻量验收已完成：

- D35 材质审计：`PASS / EDITOR_STATIC`；11 个纹理材质、1 个明确 authored-color 灰泥例外、6/6 法线元数据通过；当前 R03 Editor 基线为 `367 Renderer / 1 Collider / 33084 mesh vertices`；
- D31 交互逻辑审计：`PASS / STATIC_LOGIC_ONLY`；可见入口 `29` 个、键盘入口 `4` 个、错误 `0`；
- D19 R03 生成器、D35 材质审计、D31 交互审计定向单测：`16/16 PASS`；
- R62 结果 JSON、D31 汇合结果 JSON 和 R03 Editor 审计 JSON：`3/3 PASS`；
- `git diff --check`：未发现尾随空白错误。

### 54.2 交付边界

这条记录只确认代码、资源准入、Prefab 结构、Editor 构建和逻辑层交互没有被 R61/R62 破坏。它不新增或伪造运行时证据；`D25-S06`、`D26-S05`、`D27-S05`、`D28-S05`、`D29-S05`、`D30-S05`、`D31-S05` 及相应汇合门禁仍按状态看板保持 `PARTIAL`。后续在用户准备好时，再一次性执行 Player 物理输入矩阵、D30 异常恢复、最终画质和性能 1% low 验收。

### 54.3 证据

- [D31-S05 当前结果](../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)
- [D31-G06 当前结果](../../game/tools/results/D31/D31-G06/D31_G06_RESULT.json)
- [R62 视觉逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R62.log)
- [R61 材质审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R61.json)

## 55. R63 D29/D30 防重策略边界硬化（2026-09-03）

### 55.1 发现与修复

代码层复核发现，D29 一次性请求防重只规范化当前动作 ID，D30 阶段事件防重只规范化当前事件 ID；如果上一帧保存的是大小写或带空格的 ID，同一个请求可能绕过防重。另外，测试时钟回拨会被负 elapsed 错误识别为仍在防重窗口内。R63 在两个零 Unity 依赖的纯 C# 策略中统一规范化前后 ID，并要求时间值有限、guard 为正且 elapsed 非负后才认定为重复。

### 55.2 证据

- [D29 防重策略](../../game/Assets/Scripts/Gameplay/D29/Logic/D29OneShotPolicy.cs)
- [D30 阶段策略](../../game/Assets/Scripts/Gameplay/D30/Logic/D30VisualStagePolicy.cs)
- [D31 纯逻辑测试](../../game/Assets/Tests/EditMode/D31/D31VisualStagePolicyTests.cs)
- [R63 静态交互审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R63.json)
- [R63 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_REGRESSION_R63.log)
- [R63 Unity 编译/测试日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R63B.log)
- [R63 EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R63B.xml)

验证结果：D29/D31 Python 测试 `27/27 PASS`；Unity EditMode `28/28 PASS`、0 failures、0 skipped；Unity Tundra 编译成功、`error CS=0`；D31 静态交互审计保持 `PASS / STATIC_LOGIC_ONLY`。

### 55.3 状态边界

R63 是表现层防重策略的纯逻辑边界修复，状态为 `PASS_WITHIN_SCOPE`。它不升级 `D29-G06`、`D30-G06`、`D31-S05` 或 `D31-G06`；真实 Player 物理输入、D30 实机异常恢复、最终画质和性能 1% low 仍按后置门禁处理。

## 56. R64 R03 波面网格与环岛泡沫补强（2026-09-03）

### 56.1 发现与实现

R62 虽然已经把岛面改成有机椭圆轮廓并扩展了环岛水域，但水面本身仍是平的盒体，只有 `D28WaterMotion` 在运行时做整体上下移动；从固定镜头看仍容易读成一张背景板。R64 继续只修改 R03 表现生成器：

- 将 `Water` 平盒体替换为一个 `56×56`、`16×16` 网格的确定性低成本波面，最大静态起伏 `0.055`；仍只有一个水面 Renderer，不添加 Collider；
- 沿 `IslandPoint` 生成 28 段环岛泡沫短条，使泡沫与当前有机海岸轮廓使用同一坐标源，而不是只在南侧生成；
- 将 `R03_Teal` 设为不透明 URP Lit 表现，使用轻微金属度和较高平滑度，使波面几何能产生可读高光；不引入自定义 Shader；
- 保留 D28 的天气驱动和水面动态组件；不触碰 D2/D6 移动碰撞、D21 天气事实、航行、交互锚点、存档或任何业务 owner。

### 56.2 验证证据

- [R64 水体表现审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_WATER_SURFACE_AUDIT_R64.json)
- [R64 材质审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R64.json)
- [R64 视觉逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R64.log)
- [R64 Unity 表现重建](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R64.log)
- [R64 Unity Editor 审计](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R64.log)
- [最新 R03 Editor 审计](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [R03 生成器定向测试](../../tools/tests/test_d19_r03_visual_builder.py)

验证结果：定向 Python 测试 `21/21 PASS`；Unity R03 重建和审计退出码 `0`，日志包含 `Built and placed` 与 `Audit written`；`error CS=0`；当前 Editor 基线为 `382 Renderer / 1 Collider / 36564 mesh vertices`，其中水面保持单 Renderer、泡沫不新增 Collider。

### 56.3 状态边界

R64 状态为 `PASS_WITHIN_SCOPE / EDITOR_STATIC`，只证明水体表现的代码、Prefab 生成和 Editor 结构正确，不宣称最终画质通过。`D25-S06`、`D26-S05`、`D27-S05`、`D28-S05`、`D29-S05`、`D30-S05`、`D31-S05` 及相应汇合门禁继续按照真实证据保持既有状态；Player 物理输入、D30 实机异常恢复、最终画面质量和性能 1% low 仍由后置门禁负责。

## 57. R65 Kenney 海盗船候选接入与船体回退（2026-09-03）

### 57.1 发现与实现

R64 之后，R03 的船仍使用生成式盒体/楔体组合，和已经在 D18-A05 目录中登记并完成 Unity 导入验证的 Kenney Pirate Kit 船体候选不一致。R65 将 `ship-pirate-medium.fbx` 作为 R03 船的首选表现模型，并保留原生成式船体作为资源导入缺失时的显式回退：

- 通过 `AssetDatabase` 加载并嵌套候选 FBX，不复制或改写候选源文件；
- 根据候选渲染包围盒将模型归一到现有船体表现尺度，保持稳定 `ph_ship_fishing` 节点；
- 只替换表现几何，继续由 `D6_ShipHullCollision` 提供唯一船体碰撞；
- 候选源材质优先保留，缺失材质才使用 R03 船材质回退；
- 增加源候选和生成 Prefab 的 Editor 审计，记录渲染器、网格、材质、碰撞和候选嵌套状态。

### 57.2 证据

- [R65 船候选审计](../../game/tools/results/D31/D31-S05/D31_S05_SHIP_CANDIDATE_AUDIT_R65.json)
- [R65 视觉逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R65.log)
- [R65 Unity 重建](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R65.log)
- [R65 Unity 场景审计](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R65B.log)
- [最新 R03 Editor 审计](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [R03 船 Prefab](../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_ShipVisual.prefab)
- [D18-A05 船港道具 manifest](design/common/assets/packs/ships_port_props/kenney_pirate_kit_ships_port_props.manifest.json)

验证结果：候选源 `6 Renderer / 6 MeshFilter / 6 MaterialSlot / 0 MissingMaterial / 0 AnimationClip`；生成船 Prefab `6 Renderer / 1 Collider / 2204 mesh vertices`，候选嵌套实例存在；R03 重建和场景审计退出码 `0`，`error CS=0`；D19 R03 生成器 Python 定向测试 `11/11 PASS`。当前 R03 表现基线保持 `382 Renderer / 1 Collider / 36564 mesh vertices`。

### 57.3 状态边界

R65 状态为 `PASS_WITHIN_SCOPE / EDITOR_STATIC`。源资源仍为 `Candidate / ImportVerified`，没有修改 `LocalDevelopmentReady` 或 `ReleaseApproved`，也没有把候选船提升为发布资源。该修复不替代 Player 最终画质、物理 OS 输入、D30 实机异常恢复或性能 1% low 证据；`D31-S05` 与 `D31-G06` 继续按状态看板保持 `PARTIAL`。

## 58. R66 Kenney 港口建筑与道具候选补强（2026-09-03）

### 58.1 实现

在 R65 船体候选接入之后，R03 港口仍有大量生成式方块道具，视觉层缺少能表达港口身份的真实轮廓。R66 在同一表现生成器中加入六个 D18-A05 已登记且 `ImportVerified` 的 Kenney Pirate Kit 候选：

- `tower-complete-small.fbx`：后景塔楼地标；
- `structure-platform-dock-small.fbx`：主码头外侧延伸段；
- `boat-row-small.fbx`：靠泊小艇；
- `cannon.fbx`：船坞侧火炮；
- `crate-bottles.fbx`：仓储货箱；
- `flag-pirate.fbx`：港口旗帜。

候选以嵌套 Prefab 形式放入 `D19_R03_KenneyPiratePortDetails` 表现组，保留源 `colormap` 材质，只有缺失材质槽才使用明确回退材质；导入的 Collider 会在表现实例上移除，R03 继续只有一个 `D19_TerrainRoot` 路线 Collider，D2/D6 仍是移动、泊位、船体和交互的所有权来源。

### 58.2 证据

- [R66 候选源审计](../../game/tools/results/D31/D31-S05/D31_S05_PIRATE_PORT_CANDIDATE_AUDIT_R66.json)
- [R66 候选审计日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_PIRATE_PORT_CANDIDATE_AUDIT_R66.log)
- [R66 Unity 重建](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_REBUILD_R66.log)
- [R66 Unity 场景审计](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_VISUAL_AUDIT_R66.log)
- [R66 材质审计](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_MATERIAL_AUDIT_R66.json)
- [R66 视觉/逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_VISUAL_LOGIC_REGRESSION_R66.log)
- [最新 R03 Editor 审计](../../game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json)
- [R03 港口表现 Prefab](../../game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab)

结果：六个候选 `6/6 loaded`；源合计 `9 Renderer / 9 MeshFilter / 9 MaterialSlot / 0 MissingMaterial / 0 AnimationClip / 2554 mesh vertices`；R03 Editor 基线为 `391 Renderer / 1 Collider / 39118 mesh vertices`；Unity 重建/审计退出码 `0`、`error CS=0`；Python 定向回归 `24/24 PASS`；D35 材质审计 `PASS / EDITOR_STATIC`；D31 静态交互审计 `PASS / STATIC_LOGIC_ONLY`。

### 58.3 状态边界

R66 状态为 `PASS_WITHIN_SCOPE / EDITOR_STATIC`。它完成了港口表现层候选接入和静态契约同步，但不把资源提升到 `LocalDevelopmentReady` 或 `ReleaseApproved`，也不把 Editor 结构结果当作最终画质。`D25-S06`、`D26-S05`、`D27-S05`、`D28-S05`、`D29-S05`、`D30-S05`、`D31-S05` 与 `D31-G06` 的正式状态继续按看板保持 `PARTIAL`；Player 最终视觉审查、物理 OS 输入矩阵、D30 实机异常恢复和性能 1% low 仍留到后置验收。

## 59. R67 D31 交互布局纯逻辑收口（2026-09-03）

### 59.1 目标

将“面板是否会堆叠、按钮是否越界、仓库模态是否有独立命名”从依赖截图的主观检查，收敛为可重复的代码契约。该轮严格保持纯逻辑/编辑器范围，不重新启动完整 Player，也不把静态证据当作运行时通过。

### 59.2 实现

- 新增 `D29InteractionLayoutPolicy` 零 Unity 依赖策略，统一 1920×1080 参考分辨率、D25 标题/反馈/提示、D29 快捷面板、仓库模态和 8 个按钮的几何常量；
- 新增 `D31_HUD_LAYOUT_CONTRACT.json`，规定屏幕左上角坐标、界面矩形、按钮栈和模态互斥边界；
- 让 `D29InteractionHud` 消费同一策略常量，避免 UI 代码和设计文档各自维护一套尺寸；
- 新增 Python 静态审计和 Unity EditMode 测试，覆盖越界、相互重叠、按钮栈越界和仓库对象名/协调器别名区分；
- 没有改变交易、库存、存档、航行、输入所有权、正式场景资源或业务 owner。

### 59.3 证据与结果

- [HUD 布局契约](design/common/D31_HUD_LAYOUT_CONTRACT.json)
- [R67 HUD 布局审计](../../game/tools/results/D31/D31-S05/D31_S05_HUD_LAYOUT_AUDIT_R67.json)
- [R67 逻辑回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R67.log)
- [R67 Unity EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R67B.xml)
- [R67 D31-S05 结果](../../game/tools/results/D31/D31-S05/D31_S05_RESULT.json)

结果：布局审计 `PASS`（5 个界面矩形、8 个快捷按钮、0 越界、0 重叠）；Python 定向回归 `29/29 PASS`；Unity EditMode `31/31 PASS`；Unity 脚本编译 `error CS=0`；`git diff --check` `PASS`。

### 59.4 状态边界和下一步

R67 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。它只关闭 D25/D29 布局契约这一项代码层缺口，不升级 `D29-G06`、`D30-G06` 或 `D31-G06`。正式门禁仍需要后置的物理 OS 输入、完整 Player 成功/失败/禁用/重复点击、D30 实机异常恢复、最终画质和性能 1% low。下一逻辑目标是继续补齐 D31 交互的代码级状态/焦点/异常契约，保持不重复消耗长时间 Player 额度。

## 60. R68 D31 交互 owner 激活与船坞拒绝回退（2026-09-03）

### 60.1 发现

纯代码复核发现两个会直接表现为“按钮点击没有反应/面板堆住”的边界：

- D29 只启用 `InputSystemUIInputModule`，但没有显式激活并绑定当前 `EventSystem`，也没有刷新默认 Input System actions；场景切换或复用一个禁用 EventSystem 时，GraphicRaycaster 存在但事件可能无人接收；
- 船坞入口在调用 D4 owner 前先展示 `D4Canvas`，如果升级被拒绝或内容页没有建立，失败反馈之前可能留下空的临时页面。

### 60.2 实现

- `D29InteractionHud.EnsureEventSystem` 现在激活选中的 EventSystem、设置 `EventSystem.current`、打开导航事件、启用默认 Input System actions、关闭竞争模块并调用 `UpdateModules()`；
- `UpgradeShip` 在 `TryUpgradeHull()` 或 `HasVisibleContentPage` 失败时调用 `_d4.CloseContentPage()`，随后才发送 `shipyard_failed` 和主 HUD 失败反馈；
- 新增 D29 静态回归断言，锁定 EventSystem 生命周期和船坞拒绝恢复契约；没有改变 D4/D6 owner、领域数据、存档、正式场景或网络边界。

### 60.3 证据

- [R68 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R68.json)
- [R68 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R68.log)
- [R68 Unity 编译日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R68.log)
- [R68 Unity 测试日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R68B.log)
- [R68 EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R68B.xml)

验证结果：Python 定向回归 `31/31 PASS`；D31 HUD 布局审计 `PASS / STATIC_LOGIC_ONLY`；D31 交互静态审计 `PASS / STATIC_LOGIC_ONLY`；Unity EditMode `31/31 PASS`；Tundra 编译 `error CS=0`；`git diff --check` `PASS`。

### 60.4 状态边界和下一步

R68 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。它关闭两个交互入口的代码级恢复缺口，不升级 `D29-G06`、`D30-G06` 或 `D31-G06`。物理 OS 输入、完整 Player 成功/失败/禁用/焦点/重复点击、D30 实机异常恢复、最终画质和性能 1% low 仍必须由后置真实证据决定；按 Plus 滚动额度策略，本轮没有重复长时间 Player/性能运行。

## 61. R69 D31 owner 关闭异常的主 HUD 兜底（2026-09-03）

### 61.1 发现

继续做 D31 纯逻辑复核时发现，D29 的异常出口虽然会调用酒馆、船坞或港口整备 owner 的关闭回调，但如果这个回调本身再次抛异常，原实现只记录 `recovery failed`，没有继续恢复主 HUD。这样可能把临时页留在屏幕上，表现为“按钮有反馈但界面被空页遮挡”。

### 61.2 实现

- 在 `D29InteractionHud.ReportActionException` 的 owner 清理异常分支加入 `RuntimeHudCoordinator.RestorePrimaryHud()` 最后兜底；
- 兜底本身再次异常时只记录明确的 `recovery fallback failed`，不把 Unity UI 事件链重新抛崩；
- 新增 D29 源码级回归断言，锁定 owner 关闭失败仍必须尝试恢复主 HUD 的契约；
- 不改变 D4/D9/D10 owner 的业务裁决、领域数据、存档、船舶状态或网络边界。

### 61.3 验证

- D29 定向 Python 回归 `26/26 PASS`；
- `d31_interaction_logic_audit.py`：`PASS / STATIC_LOGIC_ONLY`，29 个可见入口、4 个键盘入口、错误 0；
- `d31_hud_layout_audit.py`：`PASS / STATIC_LOGIC_ONLY`，5 个界面、8 个快捷按钮、越界/重叠错误 0；
- Unity EditMode `Whalebone.Tests.D31`：`31/31 PASS`，失败 0、跳过 0；
- Unity Tundra 编译成功（0.20 秒），`error CS=0`；
- `git diff --check`：`PASS`。

证据：

- [R69 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R69.json)
- [R69 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R69.log)
- [R69 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R69.log)
- [R69 Unity 测试](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R69.log)
- [R69 EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R69.xml)

### 61.4 状态边界

R69 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，只关闭 owner 关闭回调二次异常的源码级缺口。`D29-G06`、`D30-G06`、`D31-G06` 仍保持 `PARTIAL`；物理 OS 输入、完整 Player 成功/失败/禁用/焦点/重复点击、D30 实机异常注入、最终画质和性能 1% low 仍不能由本轮证据替代。按 Plus 滚动额度策略，本轮没有启动 Player 或性能长测。

## 62. R70 天气与画质切换异常的失败阶段保持（2026-09-03）

### 62.1 发现

代码层复核发现，天气和画质切换虽然有 owner 未就绪反馈，但 owner 内部异常没有经过 D29 的统一异常出口。异常可能直接离开 Unity 输入回调，也没有向 D30 投影明确的失败事件。

### 62.2 实现

- `D29InteractionHud.CycleWeather` 和 `CycleQuality` 现在都通过 `try/catch` 保护；
- 异常统一投影为 `weather_failed` / `quality_failed`，显示“状态未改变”的失败反馈；
- `D30VisualStagePolicy` 将两类事件加入失败白名单，任何视觉阶段均保持不变；
- 新增 Python 源码回归和 Unity EditMode 失败矩阵覆盖；不改变天气事实、画质设置 owner 之外的业务数据或存档。

### 62.3 验证

- D29 定向 Python 回归 `27/27 PASS`；
- D25—D31 交互相关 Python 综合回归 `42/42 PASS`；
- `d31_interaction_logic_audit.py`：`PASS / STATIC_LOGIC_ONLY`；
- `d31_hud_layout_audit.py`：`PASS / STATIC_LOGIC_ONLY`；
- Unity EditMode `Whalebone.Tests.D31`：`33/33 PASS`，失败 0、跳过 0；
- Unity Tundra 编译成功（2.54 秒），`error CS=0`；
- `git diff --check`：`PASS`。

证据：

- [R70 交互逻辑审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R70.json)
- [R70 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R70.log)
- [R70 Unity 编译](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R70.log)
- [R70 Unity 测试](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R70.log)
- [R70 EditMode](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R70.xml)

### 62.4 状态边界

R70 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，关闭天气/画质异常的代码级未投影缺口，但不升级 `D29-G06`、`D30-G06` 或 `D31-G06`。物理 OS 输入、完整 Player 负向/重复/禁用矩阵、D30 实机异常注入、最终画质和性能 1% low 仍需要后置真实证据；本轮没有重复 Player/性能长测。

## 63. R71 D29 画质快捷入口矩阵补全（2026-09-03）

### 63.1 发现

`D29InteractionHud` 已实现 `Q -> CycleQuality -> D31VisualQualityRuntime.TryCycleQuality`，并会在主 HUD 状态栏显示成功或失败反馈，但交互矩阵的 `keyboard_only_controls` 仍只有 4 个 D4 入口，主 HUD 副标题也没有提示 `Q`。这会让“代码有入口、文档不可追踪”的情况继续存在。

### 63.2 修改

- 在 `D29_D30_INTERACTION_BUTTON_MATRIX.json` 增加唯一的 D29 `Q` 画质预览键盘入口；
- 在矩阵说明中明确 D29 画质预览是键盘专用入口，不新增一个拥挤的可见按钮；
- 主 HUD 提示从 `B/R/C/I` 更新为 `B/R/C/I/Q`；
- D31 静态交互审计与矩阵单测将键盘入口基线从 `4` 提升为 `5`，并显式检查 D29 `Q/CycleQuality`；
- 不改变画质 owner、天气/业务事实、存档、输入裁决或场景资产。

### 63.3 证据

- [D29/D30 交互矩阵](design/common/D29_D30_INTERACTION_BUTTON_MATRIX.json)
- [矩阵说明](design/common/D29_D30_INTERACTION_BUTTON_MATRIX.md)
- [D29 canonical 矩阵审计](../../game/tools/results/D29/D29-S05/D29_D30_INTERACTION_MATRIX_AUDIT.json)
- [R71 矩阵审计](../../game/tools/results/D31/D31-S05/D31_S05_MATRIX_AUDIT_R71.json)
- [R71 D31 交互审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R71.json)
- [R71 HUD 审计](../../game/tools/results/D31/D31-S05/D31_S05_HUD_LAYOUT_AUDIT_R71.json)
- [R71 逻辑回归](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R71.log)
- [R71 Unity EditMode 原始日志](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_TEST_R71B.log)
- [R71 Unity EditMode XML](../../game/tools/results/D31/D31-S05/D31_S05_EDITMODE_R71.xml)

验证结果：D29/D31/D28/D19 定向 Python `60/60 PASS`；D29 矩阵 `29/5` 审计 PASS；D31 交互静态审计 `29/5` PASS；HUD 布局 `5 surfaces / 8 visible buttons / 0 overlap / 0 out of bounds`；Unity EditMode `33/33 PASS`；Tundra 构建成功、`error CS=0`、退出码 0；`git diff --check` PASS。

### 63.4 状态边界

R71 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`。D29-S05、D29-G06、D30-G06、D31-S05 和 D31-G06 仍保持 `PARTIAL`：真实 Player 物理输入、完整负向/禁用/焦点/重复点击矩阵、D30 实机异常恢复、最终画面质量和性能 1% low 尚未证明。

## 64. R72 D27 港口角色与动物表现 Prefab 集成（2026-09-03）

### 64.1 目标

关闭“D20 角色/动物资源已经存在，但 R03 港口表现层没有实际放置内容”的编辑器层缺口，让港口不再只有空场景和孤立占位物。R72 只做只读表现集成，不把表现对象变成 NPC、生态、宠物或交互事实 owner。

### 64.2 实现

- 新增 `D27R03CharacterWildlifePresentationBuilder`，生成独立的 `D27_R03_CharacterWildlifePresentation.prefab`；
- 放置 12 个岗位 NPC，覆盖码头、渔业、船员、补网、交易、酒馆、船坞、守卫和居民；
- 放置 8 个陆生、鸟类和水生生物，以及 2 个宠物；
- NPC 绑定 `D20NpcReadOnlyPresentationAdapter`、岗位表现锚点和演示 Animator；
- 所有 NPC 非触发器碰撞体关闭，避免装饰性表现阻挡 D2/D6 移动；
- 将独立表现 Prefab 作为单一子 Prefab 挂入 `D19_R03_HarborPresentation.prefab`；
- 不修改 `InitialHarbor.unity`、Systems、AgentCore、D20/D21 事实 owner、交互裁决、位置事实或存档。

### 64.3 验证

- Unity 批处理构建退出码 `0`，Tundra 编译成功，`error CS=0`；
- 静态审计 `PASS / EDITOR_STATIC_ONLY`：NPC `12/12`、动物/水生生物 `8/8`、宠物 `2/2`、R03 挂接 `1/1`；
- D27/D28/D31 表现与交互定向 Python 测试 `12/12 PASS`；
- 构建结果：[D27_S05_R03_PRESENTATION_BUILD.json](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json)；
- 审计结果：[D27_S05_R03_PRESENTATION_AUDIT.json](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)；
- 构建日志：[D27_S05_R03_PRESENTATION_BUILD.log](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.log)；
- 审计脚本：[d27_r03_character_wildlife_audit.py](../../tools/d27_r03_character_wildlife_audit.py)；
- 审计测试：[test_d27_r03_character_wildlife_audit.py](../../tools/tests/test_d27_r03_character_wildlife_audit.py)。

### 64.4 状态边界与下一步

R72 是 `PASS_WITHIN_SCOPE / EDITOR_STATIC_ONLY`，不是 Player 画面通过。`D27-S05/G06`、`D29-S05/G06`、`D30-S05/G06` 和 `D31-S05/G06` 的正式状态不因本轮自动升级；岗位 NPC、动物、宠物和水生生物的逐项运行观察、最终低模画面质量、物理输入矩阵、D30 实机异常恢复和性能 1% low 仍由后置验收负责。额度受限时优先继续 D28—D31 的代码/Prefab/静态逻辑闭环，暂不重复长时间 Player/性能测试。

## 65. R73 D27 角色/动物表现预算登记（2026-09-03）

### 65.1 目标

让 R03 港口表现 Prefab 中新增的 22 个角色、动物和宠物包装器进入 D16 的统一表现预算登记，使 Renderer、Animator、Audio 和 LOD 预算能够按稳定 ID 追踪。该轮仍是编辑器/静态层工作，不把装饰性对象升级为 D20 业务事实或交互消费者。

### 65.2 实现

- `D27R03CharacterWildlifePresentationBuilder` 为 12 个 NPC、8 个动物/鸟类/水生生物和 2 个宠物挂载 `D16PresentationRegistration`；
- 生成唯一稳定 ID：`d27.presentation.npc.*`、`d27.presentation.wildlife.*`、`d27.presentation.pet.*`；
- 22 个登记均使用 `activeInteraction=false`，不新增交互裁决、事实写入、碰撞所有权或存档字段；
- Editor 构建报告增加 `registration_count`，用于防止新增表现对象脱离 D16 预算层；
- 没有修改 `InitialHarbor.unity`、Systems、AgentCore、D20/D21 owner 或运行时输入流程。

### 65.3 验证

- Unity 批处理构建退出码 `0`，Tundra 编译成功，`error CS=0`；
- Editor 统计：NPC `12`、动物/水生生物 `8`、宠物 `2`、D16 登记 `22`、Renderer `79`、Collider `22`；
- 静态审计：包装器 `12/8/2`、D16 登记 `22/22`、R03 挂接 `1/1`，结果 `PASS`；
- D27/D28/D31 定向 Python 单元测试 `12/12 PASS`；
- 证据：[R73 Unity 构建日志](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD_R73.log)、[R73 构建结果](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_BUILD.json)、[R73 静态审计](../../game/tools/results/D27/D27-S05/D27_S05_R03_PRESENTATION_AUDIT.json)。

### 65.4 状态边界与下一步

R73 为 `PASS_WITHIN_SCOPE / EDITOR_STATIC_ONLY`，`runtime_proof=false`。D27-S05/G06 和 D31-S05/G06 继续保持正式 `PARTIAL`：逐项 Windows Player 角色/动物/宠物观察、完整物理输入矩阵、D30 实机异常恢复、最终画面质量和性能 1% low 尚未由本轮证明。按 Plus 滚动额度策略，本轮只执行短时 Editor 构建、静态审计和单元回归，不重复长时间 Player/性能采样。

## 66. R74 D29 仓库模态关闭快捷键优先级（2026-09-04）

### 66.1 发现

仓库打开时必须由 `RuntimeHudCoordinator.PrimaryInputAllowed=false` 阻止 B/R/C/Q/P 等主 HUD 快捷键穿透；但原来的 D29 `I` 关闭处理位于这个门禁内部，导致仓库按钮文案中的 `I` 在模态打开后也被拦截，只能使用 `Escape` 关闭。

### 66.2 修改

- `D29InteractionHud.Update` 先判断仓库模态是否可见；仓库可见且按下 `I` 或 `Escape` 时立即调用 `CloseWarehouse()` 并结束本帧；
- 其他 D29 主 HUD 快捷键继续位于 `RuntimeHudCoordinator.PrimaryInputAllowed` 内，不能穿透仓库或其他临时页面；
- D31 静态审计新增“仓库关闭快捷键必须先于主输入门禁”的机器检查；
- 不修改 D3 库存事实、存档、业务 owner、交互裁决、正式场景或 Player 门禁。

### 66.3 验证

- D31 交互静态审计：`PASS / STATIC_LOGIC_ONLY`，29 个可见入口、5 个键盘入口、`warehouse_close_precedence=true`；
- Python 定向回归：`39/39 PASS`；
- Unity Tundra 编译退出码 `0`，`error CS=0`；Prefab 空 YAML 字段清理后的复核仍为退出码 `0`；
- 证据：[R74 交互审计](../../game/tools/results/D31/D31-S05/D31_S05_INTERACTION_LOGIC_AUDIT_R74.json)、[R74 回归日志](../../game/tools/results/D31/D31-S05/D31_S05_LOGIC_REGRESSION_R74.log)、[R74 Unity 编译复核](../../game/tools/results/D31/D31-S05/D31_S05_UNITY_COMPILE_R74B.log)。

### 66.4 状态边界

R74 为 `PASS_WITHIN_SCOPE / STATIC_LOGIC_ONLY`，只关闭仓库 I 快捷键被主输入门禁误拦截的代码层缺口，`runtime_proof=false`。D29-S05/G06、D30-S05/G06、D31-S05/G06 仍保持正式 `PARTIAL`；物理 OS 输入、完整 Player 负向/禁用/焦点/重复点击矩阵、D30 实机异常恢复、最终画质和性能 1% low 仍留待后置验收。

## 67. R75 VC-A16 首屏岸线与道路几何修复（2026-09-05）

### 67.1 根因

R03 场景重建时，旧版 `HideLegacyVisuals` 按名称遍历全部场景根节点，把当前 R03 自己的 `Beach` 渲染器也隐藏；同时旧的世界 Z 沙色阈值和矩形 ribbon 让岛面出现大块沙色卡片与断裂道路。泡沫又读取了岛面 `Height`，在山脊处被抬成悬空青色条。

### 67.2 修改

- 旧版名字清理遇到 `D19_R03_HarborPresentation` 直接跳过，R03 成为当前表现层而不被历史清理逻辑误伤；
- `SurfaceMaterial` 改用椭圆边缘半径生成两级沙色海岸环带，移除世界 Z 阈值和路线内意外沙色条；
- 新增 `MakePathMesh`，让沙滩、主路和仓库路沿点列并按 `Height` 贴地；
- 泡沫改为短小水线、固定水线高度并向外偏移；
- 固定镜头探针增加 `RERUN20/RERUN21` 输出目录，防止新证据覆盖旧证据；
- 更新 R03 builder 源码边界测试；不改变 D6/D19 碰撞所有权、玩法 ID、Systems 或正式结果门禁。

### 67.3 验证

- 定向 Python 回归：`65/65 PASS`；
- R03 Prefab/InitialHarbor 重建日志：`game/tools/results/VISUAL-CLEAN/VC-A09_R03_BUILD_R02.log`；
- D27 角色/动物表现层重建日志：`game/tools/results/VISUAL-CLEAN/VC-A09_D27_BUILD_R02.log`；
- Windows Player 重建日志：`game/tools/results/VISUAL-CLEAN/VC-A09_PLAYER_BUILD_R02.log`；
- 真实 Player 固定镜头：`game/tools/results/D19/D19-R03-A01/D19_R03_A02_FIXED_SHOT_PLAYER_RERUN21.json`，`1920×1080`、`14/14` backbuffer 截图，本次仅为 Calm；
- 场景诊断：`game/tools/results/VISUAL-CLEAN/VC-A09_SCENE_RENDERER_DIAGNOSTICS.json`，`Beach` 已启用，泡沫渲染器水线高度约 `0.02`，R03 可玩区域无近景 `Palm_*`；
- 完整任务证据：[VC-A16 结果](../../game/tools/results/VISUAL-CLEAN/VC-A16/VC_A16_RESULT.json)、[VC-A16 交接](design/common/handoffs/VISUAL-CLEAN/VC-A16_HANDOFF.md)。

### 67.4 状态边界

VC-A16 为 `PASS_WITHIN_SCOPE`，整体结果保留为 `PARTIAL`：低模最终美术质量、天气变体、真实逐入口负向/恢复矩阵和性能 1% low 尚未关闭。D29 的未就绪入口现为“变暗但仍可点击，由 owner 返回原因”，不应再按 R32 历史描述理解为“UI 禁用即业务权限”。
