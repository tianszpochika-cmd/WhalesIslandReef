# D20 角色、动物与动作表现标准

状态：`PASS`（D20-S01 标准冻结）  
任务：`D20-S01`  
日期：2026-08-14  
适用轨道：D20—D24 本地开发轨；不等于 `LocalDevelopmentReady`、`ReleaseApproved` 或 Steam Ready。

## 1. 目的、范围与不可变边界

本文件冻结玩家、核心船员、港口 NPC、陆生动物、水生生物、猫狗宠物和珍稀动物的比例、骨架、动作、表现层接口、LOD 和验收口径。D20-A01—A06 必须在本标准下建立独立工作场景和 Prefab；D20-S02 才能把通过工作场景验收的表现接入正式场景。

本任务只冻结标准和接口说明，没有修改正式模型、Animator、运行时代码、场景、Registry、存档、contracts 或资源状态。`ReleaseApproved` 仍只能由责任人类审核者确认，Agent 不得代签。

不可变边界：

- `CharacterSystem`/`D2CharacterController` 写玩家移动、接地、跳跃和碰撞事实；`D2ControlRouter` 写角色/船控制模式和上下船请求；`ShipSystem` 写航行/掌舵事实。
- `RelationSystem`/`CrewSystem`/`AnimalSystem`/`EcologySystem`/`PetSystem`/`TaskSystem` 分别写关系、船员状态、动物生态、宠物、任务和奖励事实。表现层只能读状态快照。
- `AppearanceSystem` 是外观组合唯一写者；`Agent` 只能提出表达或行动建议；UI 只能显示和产生输入意图。
- Animator、动画事件、材质、VFX、脚底校准和 LOD 不得直接改钱、库存、关系、任务、天气、生态、奖励、存档或实体 ID。
- 所有外部资源必须先满足 D18 `PrefabCandidateIntake` 受控准入；D20-S01 不把任何候选提升为 `LocalDevelopmentReady`。

## 2. 统一尺度、坐标、根节点与碰撞

### 2.1 冻结参数

| 项目 | 冻结值/规则 | 验证方法 |
|---|---|---|
| 世界单位 | `1 Unity unit = 1 m` | 导入报告测量 bounds；误差目标 ≤ `±1%` |
| 世界轴 | Y-up；D19 港口约定 `+Z=内陆`、`-Z=海/出航` | 工作场景轴向探针 |
| 角色前向 | Prefab 本地 `+Z` 为面部/行进前方，`+X` 为右侧 | T/A 姿态与 90° 转身探针 |
| 玩家视觉高度 | D17 基线 `1.80 m`；验收容差 `1.80 ± 0.03 m` | 从脚底接触面到头部 bounds 的实测 |
| 成人 NPC | 视觉高度 `1.65—1.95 m`；关键 NPC 必须逐体记录真实高度 | NPC 阵容联系表；不得用缩放掩盖比例错误 |
| 人体根节点 | 保留现有 `D2_PlayerAvatar`/稳定实体根；根节点 `localScale=(1,1,1)`，不挂 Renderer | Prefab 结构检查 |
| 视觉子树 | 根下固定 `Visual`；模型、骨骼、SkinnedMeshRenderer、LOD 只在其下 | Prefab hierarchy 检查 |
| 脚底原点 | 根节点原点投影到双脚中心的合法行走面；脚底 `Y=0`，允许校准误差 ≤ `0.02 m` | 射线/Collider ground probe |
| 物理胶囊 | 玩家参考高度 `1.80 m`、半径 `0.32 m`、中心 `(0,0.90,0)`；D17 现有运行时可按视觉 bounds 校准，但通过条件仍按世界值验收 | `CharacterController` 运行时日志 + EditMode 结构检查 |
| 步高/坡度 | 世界步高目标 ≤ `0.35 m`；坡度上限 `50°`；`skinWidth` 以现有运行时夹取范围 `0.015—0.06 m` 为准 | D17 平地、台阶、坡面、上下船回归 |
| 角色子 Collider | 禁止把视觉网格当唯一碰撞；玩家用根 `CharacterController`，视觉子树不得新增会改变移动事实的胶囊 | Collider 清单和运行时阻挡探针 |
| 相机目标 | 人物视角跟随实体根的 `CameraTarget`/目标偏移；默认注视高度 `1.10 m`（D17 当前 `CameraFollow` 目标偏移）；不跟随动画骨骼 | 相机遮挡、跳跃、上下船和读档回归 |
| 交互锚点 | `Head`、`Hand_L`、`Hand_R`、`Foot_L`、`Foot_R`、`Carry`、`Fish`、`Boarding`、`Disembark`、`Helm`；锚点是表现/定位接口，不替代系统交互距离 | 锚点存在性、朝向和迁移表检查 |

玩家根、碰撞体和相机锚点不得因更换模型而移动。`D2RuntimeBootstrap` 当前会将小尺寸占位视觉缩放转移到视觉子树，并从视觉 bounds 配置碰撞；正式模型必须使这一路径得到接近 `1.80 m` 的世界结果，而不是给物理根套任意缩放。

### 2.2 动物尺度与碰撞

动物不使用成人角色胶囊，也不强行套 Humanoid 比例。每个 `species_id` 必须记录 `height_or_shoulder_m`、`length_m`、`ground_offset_m` 或 `water_depth_m`、前向轴和碰撞类型。冻结参考范围如下，具体种必须以真实测量为准：

| 类别 | 参考尺寸 | 视觉/碰撞规则 |
|---|---:|---|
| 猫/小型宠物 | 肩高 `0.22—0.35 m` | Generic；低矮胶囊/盒碰撞，脚底接地 |
| 狗/中型宠物 | 肩高 `0.40—0.70 m` | Generic；跟随触发体与物理阻挡分离 |
| 小型陆生动物/岸鸟 | `0.10—0.45 m` | Generic；近距可碰撞，远距代理不保留物理碰撞 |
| 大型陆生动物 | 肩高 `0.80—1.70 m` | Generic；按物种限制碰撞半径和可达坡度 |
| 鱼群/近海生物 | 记录体长和海面/海床偏移 | Generic；水体层级和鱼群代理由生态表现适配器读取 |
| 海龟/大型水生生物 | 记录壳长/体长 | Generic；观察碰撞与航行阻挡分离，禁止动画事件发放奖励 |

动物的 `Idle/Forage/Travel/Alert/Flee/Rest/Injured/Observed` 等行为由确定性 `AnimalSystem/EcologySystem/PetSystem` 写入；模型和 Animator 只读取。

## 3. Rig、Avatar、骨骼和 Root Motion

### 3.1 Humanoid/Generic 选择

- 玩家、核心船员、成人 NPC 使用 Unity `Humanoid`，以导入时生成的 `Avatar` 为唯一重定向入口。必须提供 T-pose 或 A-pose 导入证据、Avatar 验证结果和骨骼映射报告。
- 鱼、鸟、猫狗、陆生动物和其他非人形生物使用 `Generic`；不为了复用人形动作强行生成人类 Avatar。
- 建筑、道具、船载装饰不属于本标准的角色 Rig；如需骨骼动画，使用 `Generic` 并在所属 D19/D21 标准中单独记录。

### 3.2 稳定骨骼命名

人形最低骨骼集使用下列逻辑名（大小写和左右方向必须稳定）：

`Root`、`Hips`、`Spine`、`Chest`、`Neck`、`Head`、`LeftShoulder`、`LeftUpperArm`、`LeftLowerArm`、`LeftHand`、`RightShoulder`、`RightUpperArm`、`RightLowerArm`、`RightHand`、`LeftUpperLeg`、`LeftLowerLeg`、`LeftFoot`、`LeftToes`、`RightUpperLeg`、`RightLowerLeg`、`RightFoot`、`RightToes`。

眼睛、手指、衣物和工具骨骼为可选扩展，不能替代最低集。当前 D2 占位代码按 `left/right + upperarm/upleg/lowerleg/foot` 进行兼容发现；D20-A01/A02 必须保留这些可发现别名，同时提交标准逻辑名，不能通过重命名删除 D17 兼容。

### 3.3 Root Motion 与动画状态

- 玩家、NPC、宠物、陆生动物默认 `Apply Root Motion = false`；位置、朝向、跳跃、上下船、跟随和水中移动由所属系统/控制器写入。
- 动画必须是 in-place 或在导入时明确剥离位移。Board/Disembark/Helm/Fish 使用已有世界锚点和状态机，不使用动画位移把实体“传送”到船上。
- 允许读取动画的视觉脚步/手部姿态，但禁止读取动画事件来发放奖励、推进任务、改变关系、改变库存或提交存档。
- 现有 D2 公共动作枚举顺序 `Idle=0, Walk=1, Run=2, Jump=3, Boarding=4, Disembarking=5` 必须保留。后续扩展只能追加值或使用字符串别名，禁止重排旧值；旧存档/测试仍能解析这六个动作。

## 4. 状态接口与 Animator 参数

### 4.1 只读表现快照

D20-A02/A04/A05/A06 的表现适配器只消费以下快照，不创建第二份权威数据：

| 字段 | 类型/范围 | 写者 | 表现用途 |
|---|---|---|---|
| `action_id` | 稳定字符串；旧动作映射到 `idle/walk/run/jump/boarding/disembarking` | D2/所属动作系统 | 选择状态机 |
| `grounded` | bool | `D2CharacterController`/对应动物移动系统 | Jump/Fall/Land 与脚底 |
| `speed01` | `0..1` | 移动系统快照 | Walk/Run 播放速率 |
| `vertical_speed` | m/s | 移动系统快照 | Jump/Fall/Land |
| `control_mode` | `Character/Ship/Locked` | `D2ControlRouter` | Board/Helm/输入锁反馈 |
| `interaction_kind` | 稳定枚举或空值 | 交互所属系统 | Interact/Talk/Fish/Carry |
| `health_band` | `healthy/warned/injured/critical` | 健康/战斗系统 | Hit/Recover/姿态；不改数值 |
| `fatigue_band` | `fresh/tired/exhausted` | 疲劳所属系统 | 呼吸、步幅、休息姿态 |
| `lod_band` | `near/mid/far/proxy` | 表现/加载层 | 渲染和 Animator 降级 |
| `paused` | bool | 暂停/输入层 | 保持画面、停止动作时间 |

快照字段不得被 UI、Agent、Animator Controller 或材质反向写回。具体系统可提供强类型 DTO，但跨端/存档字段仍遵循已有契约，不在本任务新增 contracts。

### 4.2 Animator 参数契约

D20 Animator Controller 后续统一采用以下参数名；A02 可使用现有控制器适配层，但不能静默改名：

| 参数 | 类型 | 语义 |
|---|---|---|
| `D20_ActionId` | int | 动作稳定 ID；旧 0—5 保持兼容，扩展只追加 |
| `D20_Speed01` | float | 规范化移动速度 |
| `D20_VerticalSpeed` | float | 垂直速度 |
| `D20_Grounded` | bool | 当前接地快照 |
| `D20_ControlMode` | int | Character/Ship/Locked |
| `D20_InteractionKind` | int | None/Interact/Talk/Carry/Fish/Helm |
| `D20_Injured` | bool | 只读健康带映射，不能改变健康 |
| `D20_LodBand` | int | 近、中、远、代理 |
| `D20_Paused` | bool | 暂停时禁止动作时间推进 |

一次性动作由系统先写入快照，再由适配器在一帧内消费；不得用重复按键或重复 Animation Event 造成重复交互。找不到参数、Clip 或 Avatar 时必须进入 neutral/程序化 fallback，并记录可理解的缺口。

## 5. 玩家基础动作冻结

下表同时是 D20-A02 的动作映射与 D20-S02 的验收清单。

| 动作 | 触发来源/事实写者 | 视觉职责 | 失败回退 |
|---|---|---|---|
| `Idle` | 无移动、合法接地；移动系统 | 呼吸、站姿、朝向保持 | 中立站姿；不得 T-pose |
| `Walk` | `D2CharacterController.IsMoving && !IsRunning` | 原地走步、脚步贴地、速度同步 | 程序化走步或 Idle；不改速度 |
| `Run` | `IsMoving && IsRunning`；Shift/手柄输入仅产生意图 | 更大步幅、身体前倾、脚步同步 | Walk，再回到真实移动速度 |
| `Jump` | `TryJump()` 成功；CharacterController 写垂直速度 | 离地起跳姿态 | 若请求失败保留 Idle/Walk；不强行抬根 |
| `Fall` | `!grounded && vertical_speed < 0` | 下落、手臂平衡、可读危险反馈 | Neutral 空中姿态；仍由物理处理 |
| `Land` | 接地从 false→true 的事实边沿 | 短暂屈膝/落地恢复 | Idle/Walk；不重复触发伤害 |
| `Interact` | 交互系统接受 `D16InputAction.Interact` | 手部/面对目标的短动作 | Idle + 不可用原因/提示 |
| `Talk` | 对话系统打开/关闭状态 | 面向 NPC、听说姿态 | Idle + 对话未打开反馈 |
| `Carry` | 任务/搬运系统已接受搬运状态 | 双手持物、负重姿态；道具来自稳定 ID | Walk/Idle，隐藏未绑定道具但不改库存 |
| `Fish` | 钓鱼系统进入可表现阶段 | 持竿、收线、观察水面 | Interact/Idle；不得由动画结算鱼获 |
| `Board` | `D2ControlRouter.TryToggleShip()` 的 EnterShip 路径 | 走向 `Boarding` 锚点并过渡 | 保留 Character 模式、显示距离/不可达原因 |
| `Helm` | ShipSystem 接受掌舵且 `control_mode=Ship` | 站在 `Helm` 锚点、扶舵/观察 | ShipDeck Idle；不直接改航向 |
| `Hit` | 战斗/伤害系统写入 `health_band` 或受击事件 | 短暂受击姿态，不能锁死输入 | Injured Idle/Recover；不由动画减血 |
| `Recover` | 受击短态结束/系统确认恢复可控 | 起身、重新稳定 | Idle/Walk；状态恢复由系统裁决 |

`Board` 对应当前代码的 `Boarding`，下船对应 `Disembarking`。正式动作可为独立 clip，但不得改变这两个旧语义或跳过 `D7ShipDeckController` 的泊位/甲板/安全点检查。

## 6. NPC、动物与宠物最低动作集

### 6.1 NPC/船员

| 层级 | 最低动作/表现 | Animator 与模拟要求 |
|---|---|---|
| L1 核心船员 | Idle、Walk、Run、Talk、Work、Carry、Repair、Cook、Fish、Helm、Lookout、Hit、Recover、Rest | 近景完整 Animator；状态仍由 Crew/Relation/Task 写入 |
| L2 关键 NPC | Idle、Walk、Talk、Gesture、Work、Rest、Alarm/Refuse、Injured | 共享 Humanoid Controller；繁忙/拒绝/不可达必须可见 |
| L3 普通居民 | Idle、Walk、Work、Talk、Avoid、Flee | 共享控制器或合批动画；不为每个居民运行 Agent |
| L4 远景群体 | 方向移动、停留、简单岗位循环 | 代理网格/群体动画；保留系统事实和位置快照 |

船员视觉可读取健康、疲劳、士气等带状状态，但不得写数值。社会关系、对白和拒绝路线仍由 D10 强逻辑系统裁决。

### 6.2 陆生动物、水生生物和宠物

- 陆生动物最低集：`Idle/Forage/Travel/Alert/Flee/Rest/Injured/Observed`；有攻击/防御事实时才提供 `Attack/Defend`。
- 鱼群/近海生物最低集：`Swim/Turn/Dive/Surface/Flee/Observed`；鱼群代理不能直接发放鱼获。
- 猫狗宠物最低集：`Idle/Walk/Run/Follow/SitOrRest/Interact/Play/Eat/Alert/Injured/Observed`；跟随、停留、上下船和传送恢复由 D11 Pet/Animal 系统写入。
- 珍稀动物最低集：`Idle/Travel/Alert/Flee/Observed`；发现/观察提示不能泄露未发现坐标，表现层不添加捕获或奖励分支。
- 行为状态必须可用固定种子复现；远景只能降低刷新频率、骨骼数和表现更新，不得跳过迁徙、观测、救助、捕鲸分支或存档事实。

## 7. LOD、Animator Culling、模拟等级与预算

### 7.1 距离带

遵循 D18 的统一距离：静态和角色默认 `0—12m LOD0`、`12—32m LOD1`、`32—80m LOD2`、`80m+ LOD3/代理/裁剪`；玩家和英雄角色可以把 LOD0 延至 `18m`，但不得超过本表预算。

| 对象 | Near | Mid | Far | 远距规则 |
|---|---|---|---|---|
| 玩家/核心船员 | 0—18m LOD0 + 完整 Animator | 18—32m LOD1 | 32—80m LOD2；80m+ proxy | 保留实体、碰撞/交互只由系统决定 |
| L2 关键 NPC | 0—12m LOD0 | 12—32m LOD1 | 32—80m LOD2/共享动作 | 远距保留岗位/位置事实 |
| L3/L4 居民 | 0—12m 共享动作 | 12—32m 低频动作 | 32m+ 群体代理 | 不运行逐帧 Agent |
| 宠物/陆生动物 | 0—12m 细节 | 12—32m 简化骨骼 | 32—80m 群体/代理 | 跟随目标事实不丢失 |
| 鱼群/大型水生生物 | 0—12m 近景 | 12—32m 群体简化 | 32m+ 方向代理 | 水深/生态事件仍由系统保存 |

Animator 默认使用 `BasedOnRenderers` 或等价的受控 culling；近景动作不应因摄像机短暂离屏而改变逻辑状态。对象池复用前必须清空材质、Animator 参数、动作一次性标志、字幕、InteractionAnchor 和宠物跟随目标。

### 7.2 D18 1080p 预算落地

D20 角色/动物必须服从 D18 的 Low/Medium/High 同屏目标：

| 指标 | Low / Medium / High | 测试场景 | 采集与超限处理 |
|---|---:|---|---|
| 1080p 表现实体 | `≤96 / 128 / 160` | InitialHarbor 市场、码头、近海 | 只读计数器；LOD/代理/裁剪，不删除实体 |
| 同屏 Agent/NPC | `≤8 / 12 / 16` | 市场含玩家和核心船员 | Animator/Renderer 计数；降低远景频率 |
| 同屏动物 | `≤12 / 24 / 32` | 海岸和近海 | Ecology 只读计数 + 录屏；用群体代理 |
| 玩家/核心船员 LOD0 三角形 | `8k—25k`/实例 | 出生、市场、登船 | 导入报告；超限需简化或有视觉收益说明 |
| 动物/宠物 LOD0 三角形 | `3k—15k`/实例 | 海岸、宠物互动 | 导入报告；超限退回 Candidate 或补预算 |
| CPU/GPU/FPS/GC | CPU p95 `≤12ms`、GPU p95 `≤15ms`、Medium 平均 `≥60FPS` 且 1% low `≥45FPS`、稳态 `0 B/frame` | 港口到码头、近海、天气各 60s | Unity Profiler/Frame Debugger；连续三轮失败标 BLOCKED |
| 工作集/纹理 | Medium `≤4.0GiB`/`≤1.5GiB` | InitialHarbor↔Nearshore 10 次 | Memory Profiler；卸载材质/音频，不删事实 |

低画质只允许减少骨骼、阴影、材质复杂度、动画采样和音频；角色位置、动作意图、交互结果、生态事件、关系事实和存档内容不可减少。

## 8. 玩家外观组合边界

外观是 `AppearanceLoadout` 的表现读取结果，所有槽位使用稳定资源 key。`D10AppearanceSystem` 是唯一写者；本标准不新增身份、关系或职业数据，也不把视觉标签当作裁决条件。

### 8.1 槽位和变体

| 槽位 | 允许内容 | 禁止内容 |
|---|---|---|
| `body_preset` | 成人体型/身高方向，保持 1.80m 物理基线 | 用体型改速度、伤害、关系或身份 |
| `presentation_profile` | 玩家性别表达/性别呈现方向；由已有身份选择结果驱动 | UI/服装直接改身份字段 |
| `skin_tone` | 经过历史/敏感内容审查的肤色材质 ID | 把肤色映射成阵营、职业或数值 |
| `hair_style` / `hair_color` | 发型、发色 ID；发型与帽子冲突可由表现层隐藏 | 以发型写文化、关系或任务条件 |
| `headwear` / `top` / `bottom` / `outerwear` / `footwear` | 1675 年可解释的服装组合 | 现代拉链、塑料扣、现代徽章、无来源制服 |
| `accessory` / `tool_visual` | 可见饰品、道具、职业工具 | 装饰直接改变库存、伤害、装备裁决 |
| `profession_variant` / `faction_variant` | 颜色、帽型、围裙、工具和轮廓的受控视觉变体 | 视觉直接裁决势力、法律、收入、关系 |

至少为玩家保留两个体型/性别呈现方向、六个发型槽位和八个时代合理服装组合的接口目标；实际可用数量由 D20-A01 的准入资源和审查证据决定，禁止复制同一 Prefab 凑数。变更外观不得重置 Avatar、角色状态、关系、装备事实或存档字段。

### 8.2 拼装规则

每个 PlayerVisual Prefab 必须能在不替换物理根的情况下挂载：`Visual`、`Rig/Avatar`、`Sockets`、`LOD0/LOD1/LOD2`、`CollisionProxy`（如有）、材质槽和回退标记。模块缺失时使用合法中性回退并记录 `missing_optional_slot`；缺失核心骨骼、脚底、材质或 Avatar 时不得进入本地运行时接入。

## 9. D20-A01—A06 唯一写者与拼装接口

| 任务 | 唯一写入目录 | 主要产物/接口 | 禁止写入 |
|---|---|---|---|
| D20-A01 玩家视觉 | `game/Assets/Prefabs/Characters/D20/PlayerVisuals/`、`game/Assets/Art/Models/Characters/D20/Player/` | PlayerVisual、外观槽位、Avatar、材质和尺寸报告；消费现有 Appearance/ResourceKey | 正式玩家生成器、Controller、Animator、InitialHarbor |
| D20-A02 玩家动作 | `game/Assets/Animations/D20/Player/`、`game/Assets/Scripts/Presentation/D20/PlayerAnimation/` | Animator/Override、动作适配器、参数兼容和 fallback | CharacterController、输入映射、正式 Player Prefab、场景 |
| D20-A03 港口 NPC 外观 | `game/Assets/Prefabs/Characters/D20/NPC/`、`game/Assets/Art/Models/Characters/D20/NPC/` | NPC Visual Prefab、岗位/身份外观组合、关键角色唯一性 | NPC 行为/生成器、对话/关系、InitialHarbor |
| D20-A04 NPC 表现 | `game/Assets/Scripts/Presentation/D20/NpcPresentation/`、`game/Assets/Animations/D20/NPC/`、专属演示 Prefab | 只读状态映射、岗位锚点、低距反馈和失败提示 | NPCSystem、AgentCore、工资/任务/关系、正式场景 |
| D20-A05 野生/水生表现 | `game/Assets/Prefabs/Animals/D20/Wildlife/`、`game/Assets/Art/Models/Animals/D20/`、`game/Assets/Animations/D20/Animals/` | 物种 Prefab、Generic 动作、群体/水深/LOD 适配 | Animal/Ecology 强逻辑、水体、InitialHarbor、Registry |
| D20-A06 宠物/珍稀表现 | `game/Assets/Prefabs/Animals/D20/PetsRare/`、`game/Assets/Scripts/Presentation/D20/PetRarePresentation/`、专属动画 | 猫狗/珍稀视觉、跟随/观察反馈、恢复和 fallback | Pet/Ecology/Reward/Relation 数值、正式场景、存档 Schema |

任务文档、测试和 `game/tools/results/D20/<TASK>/` 属于对应任务的交接范围；不允许两个任务共享同一可写资源目录。跨任务只通过稳定 `content_id`、`resource_key`、只读接口和交接报告传递事实。

### 9.1 Prefab 组装接口

所有 D20 角色/动物 Prefab 的根节点命名为 `D20_<domain>_<stable_id>`；最低结构如下：

```text
D20_<domain>_<stable_id>
├── Visual
│   └── Rig / Mesh / SkinnedMeshRenderer
├── Sockets
│   ├── Head, Hand_L, Hand_R, Foot_L, Foot_R
│   ├── Carry, Fish, Boarding, Disembark, Helm
│   └── CameraTarget (玩家/需要跟随镜头者)
├── LOD0
├── LOD1
├── LOD2
└── CollisionProxy (仅当所属任务有明确碰撞职责)
```

根节点不得引用临时场景对象、强逻辑单例或网络/LLM；Prefab 只声明稳定 ID、表现组件和资源引用。正式场景摆放与系统绑定由 D20-S02 唯一写者负责。

## 10. 测试矩阵

| 类别 | 最低用例 | 证据/通过门槛 |
|---|---|---|
| 结构/EditMode | 根/Visual/Rig/Sockets/LOD 命名、根 scale、脚底、forward、Avatar、骨骼最低集 | 0 缺失核心节点；1.80m 玩家误差 ≤0.03m |
| 材质/资产 | 许可证准入、材质槽、纹理、bounds、无粉色/丢引用 | 资源路径和证据可追溯；缺口为 BLOCKED/PARTIAL |
| Animator | 参数完整、旧动作 0—5、扩展动作可达、无 T-pose、Root Motion=false | 参数/状态机静态检查；缺动作有 fallback |
| 玩家物理 | 出生落地、平地 Walk/Run、真实 Jump/Fall/Land、坡面/台阶/越界恢复 | D17 基线不回退；脚底误差≤0.02m；无胶囊漂浮 |
| 玩家交互 | Interact/Talk/Carry/Fish/Board/Helm、暂停、动作中断、失败提示 | 逻辑系统写入事实；表现层无强逻辑写入 |
| 上下船/镜头 | 10 轮上船/下船、甲板/舵位、镜头切换/遮挡/读档 | 稳定 `Boarding/Disembark/Helm` 锚点；不传送、不丢控制 |
| NPC | 8+ 状态、4 个岗位、对话成功/忙碌/不可达/拒绝、近中远降级 | 只读快照；不改工资、库存、关系、任务 |
| 动物/宠物 | 物种动作、地面/水深、观察/受惊/逃离、跟随/停留/上下船/恢复 | 确定性复现；不改种群、奖励、好感或生态结果 |
| 性能 | 1080p Low/Medium/High 同屏实体、角色、动物、LOD、GC、帧时间 | 对齐 D18 表格；连续三轮超限则 BLOCKED |
| 保存恢复 | 外观 loadout、动作瞬态、场景卸载/重载、断点恢复 | 实体 ID/事实/事件游标不丢；表现重新从唯一写者重建 |
| 边界审计 | 搜索表现层对 Systems/Agent/存档/奖励的写入；搜索网络/LLM | 0 条越权写入；0 个网络/LLM 引用 |

D20-S01 只冻结矩阵，不宣称上述实现已经通过。A01—A06 必须各自留下真实测试命令、结果、失败项和截图/日志；D20-S02 才能做正式场景回归。

## 11. 玩家模式、开发调试模式与敏感内容

### 11.1 玩家模式

玩家镜头只能显示角色、服装、动作、反馈和必要提示。不得出现 Gizmo、碰撞体、LOD 标签、实体 ID、资源状态、调试水印、候选路径或未本地化诊断文本。关键交互失败要有可理解原因，不能只表现为死按钮。

### 11.2 开发调试模式

开发构建可显示 `DEBUG` 水印、实体/资源 ID、骨骼、脚底射线、碰撞代理、LOD/Animator/模拟等级、帧时间、GC 和同屏计数；调试层只读，不能改状态、触发奖励、重置关系、跳过任务或修改存档。调试模式截图不得充当玩家镜头通过证据。

### 11.3 历史与敏感边界

服装、肤色、身份表达、宗教/势力标志和动物表现必须读取已有 D14/D16 审查标签与授权状态。历史不确定项保持 `Pending`，不通过材质或动画伪造事实。敏感内容提示、低刺激替代和跳过由内容审查/可访问性数据驱动，不由角色 Animator 决定。

## 12. S02 集成顺序、回滚和 D18-S03 关系

推荐串行顺序：

1. D20-A01 冻结玩家视觉根、Avatar、外观槽位和尺寸证据。
2. D20-A02 接入动作参数/状态适配，先跑 D17 玩家移动、落地、跳跃和上下船回归。
3. D20-A03 建立关键 NPC/岗位外观组合，继续使用稳定 `npc_id`/`resource_key`。
4. D20-A04 只读接入日程、工作、对话、忙碌、拒绝、受伤和远距反馈。
5. D20-A05 接入陆生/鸟类/鱼群/水生表现，检查生态状态与水深/地面层级。
6. D20-A06 接入猫狗/珍稀观察表现，检查跟随、停留、上下船、恢复和敏感提示。
7. 每个任务完成独立 Work 场景验证后，由 D20-S02 唯一写者在受控集成分支串行更新正式角色生成引用和 InitialHarbor；每步保留前一步回滚点。
8. D20-S02 运行 D17/D19/D20 聚焦回归、固定镜头、性能和保存恢复，输出运行时消费者证据。
9. D18-S03 在正式 Prefab、工作场景、运行时消费者、碰撞、锚点、LOD、Animator、交互、保存、截图和回退证据齐全后，才审议哪些资源可晋级 `LocalDevelopmentReady`。

回滚点：

- A01—A06 阶段只回滚对应 Work 场景/Prefab/Presentation 目录，不触碰正式场景和稳定 ID。
- S02 集成前保存 `D20-S02-preintegration` 版本；失败时恢复角色生成引用、Prefab 绑定和场景实例，同时保留旧 `resource_key` 回退。
- 任何一项导致出生落地、镜头、上下船、读档、关系/动物事实变化时，立即回滚表现接入，不用表现层修复强逻辑。

## 13. 现状基线与验收结论

已核对的当前基线：

- `D2CharacterAnimationController` 当前公开动作仍为六项旧集合，并已保留 `Action`、`IsTransitioning`、`PlayTransition` 等接口；本标准要求扩展追加而非重排。
- 当前 `Resources/D6/D6CrewAnimator.controller` 仍提供 `D6_Idle/D6_Walk/D6_Run/D6_Jump/D6_Boarding/D6_Disembarking` 六个占位状态；D20-A02 必须以这六项为兼容基线，补齐新增动作和正式 Avatar/Clip 证据。
- `D2CharacterController` 当前有 `IsGrounded`、`IsRunning`、`IsMoving`、`SpeedNormalized`、`VerticalVelocity`、`TryJump`、`SetInputEnabled`、`SetCollisionEnabled`、`SnapToGround`；D17 物理参考为 `1.80m/0.32m/center Y 0.90m`，标准保留这些回归口径。
- `D2ControlRouter` 当前统一处理交互和 `TryToggleShip()`，上下船使用 `Boarding/Disembarking` 过渡；不得由新动画层绕过该入口。
- `CameraFollow` 当前人物视角默认目标偏移为 `Y=1.10m`，通过 `SetTargets` 在角色/船之间切换，并有碰撞回退；D20 不把相机目标绑到头骨或动画位移。
- D18 目标为 1080p、Low/Medium/High 实体 `96/128/160`、Agent/NPC `8/12/16`、动物 `12/24/32`，并规定 `0—12/12—32/32—80/80+` 距离带；本文件直接采用。

结论：**PASS（D20-S01 标准冻结）**。这只表示比例、Rig、动作接口、变体边界、LOD、写者、测试矩阵和集成顺序已冻结；不表示 D20-A01—A06 已完成，不表示任何资源已经 `LocalDevelopmentReady`，也不提前打开 D20-S02 的正式场景写权限。

相关入口：

- [D20 开发章节](../../AGENT_DEVELOPMENT_CHAPTER_12_D20.md)
- [D18 视觉标准](D18_VISUAL_ART_DIRECTION_AND_BUDGET.md)
- [D19 世界布局标准](D19_SAINT_MARGARET_WORLD_BUILD_SPEC.md)
- [D17 垂直切片冻结](D17_VERTICAL_SLICE_FREEZE.md)
- [D10 外观标准](D10_APPEARANCE_OUTFIT.md)
- [D11 宠物标准](D11_PETS_CATS_DOGS.md)
