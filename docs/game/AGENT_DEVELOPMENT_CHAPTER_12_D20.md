# 第 12 章开发任务提示词：D20 玩家、NPC、动物与动作表现

> 本章目标：用正式可用模型和动作替换柱状/占位角色，让港口人物和动物有一致比例、可读行为和性能降级。`D20-S01` 已通过，不重跑；历史 A03—A06 的隔离 Prefab/Presentation 成果保留。当前硬缺口是玩家只有 Generic/NoAvatar 候选，导致 `D20-A01=PARTIAL`、`D20-A02=BLOCKED`，且 `D19-R01-S02=PARTIAL` 阻断正式集成。当前唯一有效顺序是 `D18-S02-R02 → D20-A01-R01 → D20-A02-R01 →（等待 D19-R02-S02 PASS）→ D20-S02-R01`。只有 S02-R01 可以修改正式角色生成入口和 InitialHarbor 中的角色引用。

## D20-S01：冻结角色比例、骨架、动作与变体标准

<details>
<summary>展开/复制：D20-S01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D20 角色技术总控 Agent。请直接完成 D20-S01。

任务编号：D20-S01（同步冻结任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D18-S02-v2 PASS；建议在 D19-S01 后执行，可与 D19 的独立工作场景模块并行；D18-S03 留给正式集成后的统一晋级验收。
并发规则：本任务完成前不启动 D20-A01—A06；你只冻结标准，不改正式角色 Prefab/场景。

开始前读取：AGENTS/CONTEXT/规范、D18 视觉/资源验收、character/animals 全套设计、D10 外观/关系/NPC 行为、D11 动物/宠物、D17 人物物理与动作报告、当前角色控制器、Animator、碰撞和生成代码。

执行内容：
- 冻结玩家/NPC 成人尺度、动物尺度、角色根节点与视觉子节点关系、单位缩放、前向轴、脚底原点、胶囊体和相机锚点；
- 定义 Humanoid/Generic 选择、Avatar、骨骼命名、Root Motion 策略、动作参数和状态接口；
- 定义玩家基础动作：Idle、Walk、Run、Jump、Fall、Land、Interact、Talk、Carry、Fish、Board、Helm、Hit/Recover；
- 定义 NPC/动物最低动作集、LOD、Animator Culling、远距模拟和同屏预算；
- 定义玩家性别/体型预设、肤色、发型、服装、职业/势力变体的组合边界，不改变角色身份/关系数据所有权；
- 列出 D20-A01—A06 唯一写入目录、Prefab 拼装接口、测试矩阵和 S02 集成步骤；
- 明确 D17 现有 1.8m 视觉高度、落地/上下船/镜头回归不能回退。

只允许修改 D20 标准文档、接口数据说明、交接和记忆日志；禁止修改正式模型、Animator、运行时代码、场景、Registry、存档和 contracts；不提交或推送 Git。

验收：比例/根节点/碰撞/动作参数/变体/LOD/写者边界/测试矩阵明确；每个动作有触发来源、视觉职责、失败回退；D17 兼容要求明确；git diff --check 通过。

结果必须保存到：
- 主成果：docs/game/design/common/D20_CHARACTER_RIG_ANIMATION_STANDARD.md、game/tools/results/D20/D20-S01/（验证结果）；
- 交接报告：docs/game/design/common/handoffs/D20/D20-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告给出冻结参数和 PASS/PARTIAL/BLOCKED，不用聊天回复替代文件。
```
</details>

## 历史 A01—A06 的依赖记录（保留，不作为当前执行顺序）

`D20-S01` 的历史冻结结果已经通过。A02 需要 A01 已验证的骨架接口，A04 需要 A03 的 NPC 外观接口，A06 需要 A05 的动物接口和猫狗资源。A03/A04/A05/A06 已留有可读隔离成果与接口；A01 的 interface 明确 `rig_interface_ready=false`，A02 因此被正确 BLOCKED。不要重跑历史任务来覆盖这些事实。

## 当前 R01 玩家修复与正式集成顺序（先读，再复制具体任务提示词）

```text
等待 D18-S02-R02 PASS，且 D18_R02_PLAYER_CONSUMPTION_GATE.json 明确存在 Humanoid + valid Avatar 的合法玩家候选
  ↓
D20-A01-R01（玩家 Humanoid/Avatar、1.80m、Sockets、外观工作 Prefab 和回退）
  ↓（A01-R01 interface 的 rig_interface_ready=true）
D20-A02-R01（玩家 Animator/动作表现；不改 D2 物理与正式场景）
  ↓
等待 D19-R02-S02 PASS，且历史 A03—A06 的 P0 interface/tests 以及 A01-R01/A02-R01 的 P0 结果均可读
  ↓
D20-S02-R01（唯一正式玩家/NPC/动物集成与真实 Player 回归）
```

这三项在同一台电脑上都需要 Unity/Player 实测，不能同时启动。每项 Agent 必须先通知用户“需要独占电脑进行 Unity/Player 实测”，收到允许后才操作；没有真正 Unity/Player 证据时保持 PARTIAL/BLOCKED。上游可以因非 P0 外观数量不足而 PARTIAL，但 D20-A01-R01 的 `rig_interface_ready=true` 与 D20-A02-R01 的 P0 动作通过是 D20-S02-R01 的硬门禁。

## D20-A01：玩家正式模型、性别/体型、发型与服装预设

<details>
<summary>展开/复制：D20-A01 完整提示词</summary>

```text
你是 WhalesIslandReef 的玩家角色美术实现 Agent。请直接完成 D20-A01。

任务编号：D20-A01（异步角色 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS；D18-S02-R01 PASS；最新 `d18_prefab_candidate_intake_catalog.json` 中至少有可用于 `player_visual_core` 的 `intake_eligible` 条目。女性/中性人形若仍未准入，不得伪称两套外观已完成，应如实 PARTIAL。
单一写者：game/Assets/Prefabs/Characters/D20/PlayerVisuals/、game/Assets/Art/Models/Characters/D20/Player/、D20 玩家材质/测试/文档。禁止改正式玩家生成器、控制器、Animator 和 InitialHarbor。

开始前读取：D20 标准、D18 PrefabCandidateIntake 准入目录中的角色资源、D10 外观/服装实现与资源 key、当前 ph_crew_male/player Prefab、D17 物理根节点和角色预设数据。

执行内容：
- 制作至少 2 个可切换性别/体型方向的玩家视觉基体或等价模块化组合，保持同一物理根节点和 gameplay 数据；
- 提供至少 6 个发型/头部变体、8 套时代合理的初始服装组合或模块化变体、受控肤色/发色材质；资源不足时按真实数交付并明确缺口，不复制同一 Prefab 凑数；
- 对齐 1.8m 基准、脚底、手部、头部、相机和交互锚点，修复缺材质、错误阴影和异常 bounds；
- 建立 PlayerVisual Prefab，不直接挂移动/经济/存档强逻辑；通过现有 Appearance/ResourceKey 接口选择，不新建第二套角色数据；
- 支持服装/发型切换时保留 Avatar/材质和不重置角色状态；
- 建立 Prefab 完整性、尺度、骨架、材质槽、资源 key 和缺失资源回退测试，输出正面/背面/动作 T-Pose 检查截图。
- 写 `game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json`，至少含 `rig_interface_ready`、基础 Prefab 路径、骨架/Avatar 信息、可用资源 key、稳定 fallback、已完成与缺失变体；A02 只能读取该文件，不能回写本任务目录。

硬约束：工作场景/Prefab 制作阶段只能使用准入目录中的 `intake_eligible` 外部资源；不自动 LocalDevelopmentReady/ReleaseApproved；不改变角色性别/特质/关系/库存裁决，不改 Save Schema、CharacterSystem、正式生成入口、Animator、场景/UI；不覆盖 D17 物理根；正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译和相关测试；每个组合无粉色材质/丢骨骼/悬空，尺度误差符合 D20 标准；记录实际变体数、资源 manifest ID、渲染器/材质数和性能；git diff --check。

结果必须保存到：
- 主成果：上述模型/Prefab/材质/测试、docs/game/design/common/D20_PLAYER_VISUAL_PRESETS.md、game/tools/results/D20/D20-A01/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出真实变体、缺口、兼容性、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-A02：玩家移动、交互、钓鱼、登船与掌舵动作层

<details>
<summary>展开/复制：D20-A02 完整提示词</summary>

```text
你是 WhalesIslandReef 的角色动画与动作适配 Agent。请直接完成 D20-A02。

任务编号：D20-A02（异步动画任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS、D18-S02-R01 PASS，且 `game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json` 存在并明确 `rig_interface_ready=true`。D20-A01 可以因非 P0 外观数量不足而 PARTIAL；若基础骨架/Prefab/回退未就绪，本任务必须 BLOCKED。只读 A01 的骨架/Prefab/interface，不得修改其文件。
单一写者：game/Assets/Animations/D20/Player/、game/Assets/Scripts/Presentation/D20/PlayerAnimation/、相关测试/文档。禁止改 CharacterController、正式玩家 Prefab 和 InitialHarbor。

开始前读取：D20 标准、D18 合法动作资源、D17 动作/落地/上下船/镜头代码和回归、当前 Animator 参数与输入状态、D9/D11 交互和钓鱼表现接口。

执行内容：
- 建立可复用 Animator Controller/Override 方案，至少覆盖 Idle/Walk/Run、Jump/Fall/Land、Interact/Talk、Carry、Fish、Board/Disembark、Helm；有合法动作时增加 Hit/Recover；
- 动画层只读取现有移动速度、地面状态、交互状态和动作意图，不自己修改位置、金钱、库存、航行或任务；
- 处理转身、过渡、脚滑、空中误判、动作中断、暂停和读档后的状态恢复；
- Root Motion 默认遵循 D20-S01，不得与 CharacterController 双重位移；
- 对缺动作提供明确 fallback，不允许人物卡 T-Pose 或动作永久锁死；
- 建立骨架重定向、参数完整性、状态可达、十轮走跑跳/交互/登船动作回归和截图/短序列帧证据。

硬约束：Presentation 代码不能写强逻辑；不改 Systems、输入映射、物理根、船控、场景、正式 Prefab、Registry/存档；外部资源只能使用准入目录中的 `intake_eligible` 条目；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译、Animator/EditMode/PlayMode 聚焦测试；D17 平地 Walk/Run、真实 Jump、上下船、落地回归不得下降；无新增 T-Pose、漂移或状态锁死；git diff --check。

结果必须保存到：
- 主成果：上述动画/Controller/Presentation 适配/测试、docs/game/design/common/D20_PLAYER_ANIMATION_LAYER.md、game/tools/results/D20/D20-A02/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出动作映射、缺失 fallback、测试计数和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-A03：港口 NPC 阵容、服装与外观变化

<details>
<summary>展开/复制：D20-A03 完整提示词</summary>

```text
你是 WhalesIslandReef 的 NPC 视觉阵容 Agent。请直接完成 D20-A03。

任务编号：D20-A03（异步 NPC Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS；D18-S02-R01 PASS；最新 `d18_prefab_candidate_intake_catalog.json` 中至少有可用于 `npc_visual_core` 的 `intake_eligible` 条目。
单一写者：game/Assets/Prefabs/Characters/D20/NPC/、game/Assets/Art/Models/Characters/D20/NPC/、对应材质/测试/文档。禁止改 NPC 行为逻辑、生成器和 InitialHarbor。

开始前读取：D20 标准、D18 合法角色资源、D10 NPC/身份/外观成果、factions/religion/character 时代设计、圣玛格丽特关键 NPC/岗位列表和当前 NPC content_id。

执行内容：
- 为船员、商人、码头工、渔民、酒馆人员、船坞工、卫兵/官员、普通居民制作代表 Prefab/外观组合；
- 样板港至少提供 12 个可辨识 NPC 视觉实例或模块化组合，其中关键角色不能只靠头顶文字区分；
- 通过肤色、发型、服装色、帽子/工具和轮廓形成受控变化，符合 1675 年和角色身份，不用现代制服；
- 每个 Prefab 对齐 D20 骨架/尺度/碰撞建议和现有 npc_id/resource key，不复制角色逻辑；
- 提供近/中/远 LOD 或 Animator Culling 建议，缺失正式动作时有可见 fallback；
- 建立资源 key、Prefab 完整性、材质、骨架、尺度、关键角色唯一性测试和阵容联系表。
- 写 `game/tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json`，至少含 `npc_visual_interface_ready`、基础 Prefab/资源 key、骨架兼容性、可读岗位集合、远距 fallback 与真实缺口；A04 只能读取该文件，不能回写本任务目录。

硬约束：不把服装外观当作阵营/关系/阶级强逻辑裁决；不改 NPCSystem/Agent/LLM、对话、存档、Registry、生成器、场景；敏感历史身份表达遵守已有审查标签；外部资源只能使用准入目录中的 `intake_eligible` 条目；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；12+ 样板 NPC 联系表，零粉色材质/丢骨架/明显现代物件；记录实际复用和独立模型数；git diff --check。

结果必须保存到：
- 主成果：上述 NPC 模型/Prefab/材质/测试、docs/game/design/common/D20_NPC_VISUAL_ROSTER.md、game/tools/results/D20/D20-A03/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

交接报告列出 npc_id、外观、复用关系、时代/审查问题、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-A04：NPC 日程、对话、工作与情绪的可视化表现

<details>
<summary>展开/复制：D20-A04 完整提示词</summary>

```text
你是 WhalesIslandReef 的 NPC 行为表现 Agent。请直接完成 D20-A04。

任务编号：D20-A04（异步 Presentation 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS、D18-S02-R01 PASS，且 `game/tools/results/D20/D20-A03/D20_A03_NPC_VISUAL_INTERFACE.json` 存在并明确 `npc_visual_interface_ready=true`。D20-A03 可以因非核心角色变体不足而 PARTIAL；若基础 NPC Prefab/资源 key/回退未就绪，本任务必须 BLOCKED。只读 A03 Prefab/interface，不得修改其文件。
单一写者：game/Assets/Scripts/Presentation/D20/NpcPresentation/、game/Assets/Animations/D20/NPC/、专属演示 Prefab/测试/文档。禁止修改 NPCSystem/AgentCore/NPC 数据所有者和 InitialHarbor。

开始前读取：D10 NPC 行为状态/岗位/关系/Agent 边界、D20 标准、当前 NPC 导航/对话/日程事件、D16 可访问性和 D19 港口锚点规范。

执行内容：
- 建立只读表现适配器，将现有 Idle/Work/Walk/Talk/Rest/Eat/Alarm/Refuse/Injured 等状态映射为 Animator、姿态、头顶/轮廓提示或环境动作；
- 建立市场、码头、船坞、酒馆等岗位的视觉锚点接口，NPC 到点后表现工作但不由表现层发工资、改库存或推进任务；
- 对话开始/结束、忙碌、不可达、拒绝、受伤等状态必须给玩家明确反馈，不能按钮无反应；
- 为远距/低画质提供降级：停用细动画但保留逻辑状态和位置事实；
- 处理寻路失败、动作缺失、暂停、场景卸载/重载和读档恢复；
- 建立状态映射、只读边界、失败回退和代表日程 PlayMode 测试。

硬约束：Systems/AgentCore 是裁决者；Presentation 只能读，不写 NPC 情绪、关系、工资、库存、任务或时间；不加入网络/LLM；不改 NPC Prefab、场景、Registry/存档/契约；不提交或推送 Git。

验证：Unity 编译和测试；至少覆盖 8 个状态、4 个岗位、对话成功/忙碌/不可达、低画质降级和读档恢复；证明表现层无强逻辑写入；git diff --check。

结果必须保存到：
- 主成果：上述 Presentation/动画映射/测试、docs/game/design/common/D20_NPC_BEHAVIOR_PRESENTATION.md、game/tools/results/D20/D20-A04/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出状态映射、失败反馈、只读证据、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-A05：陆生动物、鸟类与水生生物正式表现

<details>
<summary>展开/复制：D20-A05 完整提示词</summary>

```text
你是 WhalesIslandReef 的动物与水生生物表现 Agent。请直接完成 D20-A05。

任务编号：D20-A05（异步动物 Prefab 任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS；D18-S02-R01 PASS；最新 `d18_prefab_candidate_intake_catalog.json` 中至少有可用于 `wildlife_core` 的 `intake_eligible` 条目。鸟类尚未准入时，不得把鸟类目标写成完成，应如实保留缺口。
单一写者：game/Assets/Prefabs/Animals/D20/Wildlife/、game/Assets/Art/Models/Animals/D20/、game/Assets/Animations/D20/Animals/、Presentation 适配/测试/文档。禁止改 Animal/Ecology 强逻辑和正式场景。

开始前读取：D18 合法动物资源、D20 标准、animals 全套设计、D11 生态/珍稀动物/水生内容、D15 动物目录、当前 AnimalSystem/鱼群/生成状态接口。

执行内容：
- 为样板环境制作至少 8 类代表 Prefab：海鸟、岸鸟或小型鸟、陆生小动物、家畜/野生代表、浅水鱼群、近海鱼、海龟或同级生物、远景大型水生生物；实际合法资源不足则按真实数 PARTIAL；
- 映射 Idle/移动/进食/受惊/逃离/游泳/潜水等已有动画，缺失时提供不破坏逻辑的 fallback；
- 建立群体显示、LOD、Animator Culling、阴影和远距替代，防止所有鱼/动物都运行完整 Animator；
- 表现适配器只读取 species_id、行为状态、位置和生态结果，不修改种群、捕捞、稀有度或刷新；
- 设置合理尺寸、碰撞/触发边界、海面/海床层级，避免鱼漂在空中或动物沉入地面；
- 建立 Prefab/动画/LOD/状态映射和 1080p 同屏性能测试，输出预览截图。
- 写 `game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json`，至少含 `wildlife_interface_ready`、基础 Prefab/资源 key、物种类别、状态映射、LOD/远距 fallback、鸟类/其它缺口与宠物接口不能假设的范围；A06 只能读取该文件，不能回写本任务目录。

硬约束：不改变 EcologySystem/AnimalSystem 裁决，不新建第二套动物目录，不让 VFX/动画直接发奖励；外部资源只能使用准入目录中的 `intake_eligible` 条目；不改 InitialHarbor/水体/Registry/存档；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译/测试；8 类代表生物或真实缺口、各状态可见、地面/水体层级正确、LOD 生效、无缺材质/丢骨架；记录同屏预算；git diff --check。

结果必须保存到：
- 主成果：上述模型/Prefab/动画/Presentation/测试、docs/game/design/common/D20_WILDLIFE_AQUATIC_PRESENTATION.md、game/tools/results/D20/D20-A05/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出 species_id、模型来源、动作、LOD、性能、缺口和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-A06：猫狗宠物、珍稀动物与交互反馈

<details>
<summary>展开/复制：D20-A06 完整提示词</summary>

```text
你是 WhalesIslandReef 的宠物与珍稀动物表现 Agent。请直接完成 D20-A06。

任务编号：D20-A06（异步宠物/交互表现任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 PASS、D18-S02-R01 PASS，且 `game/tools/results/D20/D20-A05/D20_A05_WILDLIFE_INTERFACE.json` 存在并明确 `wildlife_interface_ready=true`；最新 intake catalog 中猫和狗各至少有一个可用于 `pet_core` 的 `intake_eligible` 条目。只读 A05 interface，不得修改其文件；任一猫狗资源缺失时必须如实 BLOCKED/PARTIAL，不能用改名模型替代。
单一写者：game/Assets/Prefabs/Animals/D20/PetsRare/、game/Assets/Scripts/Presentation/D20/PetRarePresentation/、专属动画/测试/文档。禁止改正式场景和宠物/生态强逻辑。

开始前读取：D11 宠物/珍稀动物成果、animals/gameplay 设计、D18 合法资源、D20 标准、当前领养/好感/发现/奖励事件接口和 D16 敏感内容状态。

执行内容：
- 制作猫、狗至少各一个正式可用 Prefab，支持 Idle/Walk/Run/Sit或Rest/Interact 的真实可用动作或明确 fallback；
- 制作至少 2 类珍稀动物代表视觉，用于发现/观察而非自动捕获；
- 建立只读表现适配器：跟随、停留、互动、受惊、不可用、发现成功/失败、图鉴提示；不直接改好感、所有权、奖励或生态；
- 宠物不得堵住门、梯、舵位和登船点；处理跟随丢失、传送恢复、上下船、读档和远距降级；
- 珍稀动物提示不得泄露未发现坐标，摄影/观察反馈要清晰；
- 建立猫狗跟随/互动/上下船恢复、珍稀发现成功/失败、只读边界和 Prefab 完整性测试。

硬约束：不改变 Pet/Ecology/Reward/Relationship 系统数值；不新增捕猎敏感玩法；不改场景、Registry/存档 Schema、UI 主框架；外部资源只能使用准入目录中的 `intake_eligible` 条目；不得提前标记 LocalDevelopmentReady，正式晋级等待 D18-S03；不提交或推送 Git。

验证：Unity 编译和测试；猫狗/珍稀动物预览截图，上下船/路径/读档代表流程，零必现卡死；git diff --check。

结果必须保存到：
- 主成果：上述 Prefab/Presentation/动画/测试、docs/game/design/common/D20_PETS_RARE_ANIMALS_PRESENTATION.md、game/tools/results/D20/D20-A06/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

报告列出真实动作、交互流程、失败恢复、测试和 PASS/PARTIAL/BLOCKED。
```
</details>

## D20-S02：角色、NPC、动物串行集成与回归验收

<details>
<summary>展开/复制：D20-S02 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D20 角色表现集成与验收 Agent。请直接完成 D20-S02。

任务编号：D20-S02（同步集成任务）
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D20-S01 已通过；D20-A01—A06 全部有可读成果；A01/A03/A05 的 interface JSON 都明确核心接口 ready，A02/A04/A06 的 P0 测试和回退已通过；`D19-R01-S02 PASS` 是硬门禁，不是“建议等待”。任一 P0 资源、接口、路线或场景验收未通过时，不启动正式集成。
并发规则：你是正式玩家/NPC/动物生成引用、共享 Animator 连接和 InitialHarbor 角色集成的唯一写者。

开始前读取：全部 D20 主成果/交接/测试、D17 人物物理回归、D19 正式场景验收、D18 PrefabCandidateIntake 准入目录、当前玩家/NPC/动物生成和 ResourceKey 解析代码。

执行内容：
1. 将 PlayerVisual、动作层、NPC Prefab/表现、动物/宠物 Prefab 通过现有稳定 resource key/适配器串行接入，不新建第二套角色或动物数据；
2. 保持物理根、CharacterController、出生/落地、移动、跳跃、上下船、镜头和暂停行为；
3. 在正式港口放置/生成 12+ 可辨识 NPC、代表动物和猫狗/珍稀样例入口，遵守现有生成/生态事实；
4. 验证外观切换、动作、对话、工作、受伤/拒绝反馈、宠物跟随、动物地面/水体层级、LOD 和读档恢复；
5. 替换可安全移除的柱状/占位视觉，保留测试兼容回退，不删除仍被旧档引用的资源 key；
6. 运行 D17/D19/D20 聚焦与全量核心 PlayMode 回归，输出固定角色截图矩阵和性能数据。

硬约束：不改 Character/NPC/Animal/Ecology/Relationship/Economy 强逻辑，不改存档 Schema/contracts/server；表现层只读系统状态；只接入 D18-S02-v2 准入目录和项目自有已审计表现资产，不标 LocalDevelopmentReady/ReleaseApproved；为 D18-S03 保留运行时消费者证据；不提交或推送 Git。

通过门槛：
- 玩家不再是柱状/明显占位模型，Idle/Walk/Run/Jump/Interact/Board/Helm 无 T-Pose 或永久锁死；
- D17 落地、移动、跳跃、上下船、相机回归全部通过；
- 12+ NPC 可辨识且行为反馈可见，猫狗和代表动物不穿地/漂浮/堵关键路线；
- 外观/动作/生成/读档不改变逻辑 ID 与存档事实；
- 1080p 同屏性能有记录，LOD/Culling 可验证；Unity 无新增错误，测试与截图齐全。

结果必须保存到：
- 主成果：正式角色/动物集成 Prefab、适配器/测试、docs/game/design/common/D20_CHARACTER_CREATURE_ACCEPTANCE.md、game/tools/results/D20/D20-S02/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-S02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED；报告必须列出测试计数、截图、性能、资源 ID、兼容回退和未解决问题。
```
</details>

## 当前 R01 修复任务：真实玩家 Humanoid、动作层与正式港口集成

> 以下任务不改写历史 D20-A01/A02/S02 的 PARTIAL/BLOCKED 记录，而是用新编号建立新的可复核结论。A01-R01 和 A02-R01 只制作/验证工作 Prefab 与表现层；S02-R01 才能消费历史 A03—A06 的隔离成果并改正式生成入口与 InitialHarbor。

## D20-A01-R01：玩家 Humanoid/Avatar、外观工作 Prefab 与稳定回退修复

<details>
<summary>展开/复制：D20-A01-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的玩家角色技术美术修复 Agent。请直接完成 D20-A01-R01：用 D18-S02-R02 已验证的合法人形资源建立真正可用的 Unity Humanoid/Avatar 玩家视觉工作 Prefab，并发布可供 A02-R01 只读消费的 rig interface。不要改正式玩家生成器、D2 物理、Animator、InitialHarbor 或存档。

任务编号：D20-A01-R01（异步、玩家视觉工作 Prefab 修复）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D20-S01 PASS、D18-S02-R02 PASS；docs/game/design/common/assets/D18_R02_PLAYER_CONSUMPTION_GATE.json 存在且明确至少一个合法玩家候选为 Humanoid + valid Avatar。若该门禁不成立，本任务必须 BLOCKED，只交付只读预检，绝不能再次用 Generic/NoAvatar 候选把 rig_interface_ready 写成 true。
单一写者：game/Assets/Prefabs/Characters/D20/PlayerVisuals/R01/、game/Assets/Art/Models/Characters/D20/Player/R01/、game/Assets/Art/Materials/D20/Player/R01/、game/Assets/Tests/EditMode/D20PlayerR01/、game/Assets/Tests/PlayMode/D20PlayerR01/、game/tools/results/D20/D20-A01-R01/、D20 专属文档/交接/记忆日志。不得修改历史 D20-A01 目录、D2/D17 控制器、正式玩家生成器、Animator 目录、InitialHarbor、D10 AppearanceSystem、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务必须实际在 Unity 中检查 Importer、Avatar、Prefab hierarchy、尺度和工作 Player。开始前先通知用户：“本任务需要独占电脑进行 Unity/Player 实测，请保存其他工作并停止使用键盘鼠标；收到允许接管后再开始。”没有获得独占许可时，只能做静态预检，不能写 `rig_interface_ready=true`。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D20-S01 与 D20 角色标准第 2/3/8/9/10 节、D18-S02-R02 report/gate/catalog、D18-A04-R02-F-R01 packet、D18-A04-R03-P packet、历史 D20-A01 interface/缺口、D10 AppearanceLoadout 和资源 key、D17/D2 物理根/相机/锚点规则。先记录旧 Generic/NoAvatar 的事实和现有 Player root，不得破坏旧 fallback。

执行内容：
1. 只从 D18 R02 gate 明确允许的 content_id/文件建立 R01 工作 PlayerVisual。Unity Importer 必须实际显示 Animation Type=Humanoid，Avatar 实际存在且 valid，T/A Pose、骨骼映射和 root motion 设置有截图/机器记录；Generic/NoAvatar 资源可以作为稳定回退来源，但绝不可成为 R01 的核心 rig；
2. 按 D20 标准建立仅表现用途的结构：Visual、Rig/Avatar、Sockets（Head、Hand_L、Hand_R、Foot_L、Foot_R、Carry、Fish、Boarding、Disembark、Helm、CameraTarget）、LOD0/LOD1/LOD2、材质槽与回退标记。物理根仍由现有 D2_PlayerAvatar/D2CharacterController 拥有，根 scale=(1,1,1)，不得把 Renderer/碰撞逻辑塞到物理根；
3. 在真实 Unity 中测量脚底、+Z 前向、1.80m±0.03m 高度、bounds、材质、阴影、Avatar 和 Socket 朝向。建立兼容 D10 AppearanceLoadout 的只读视觉映射；不新建性别/特质/关系/库存数据路径，也不让换装重置角色状态；
4. 从 R02 外观候选中接入经验证的独立发型/服装模块；目标仍是 2 个性别/体型表达方向、6 个发型槽位和 8 套时代合理组合。不得把同一网格改色计作独立几何；若非 P0 变体资源不足，保留准确数量和缺口，不得阻断已验证核心 rig；
5. 建立稳定 fallback：当可选模块缺失或 Avatar 运行时不可用时回到既有 D17 程序化视觉/合法中性视觉，保留 D2 控制、Appearance 历史和存档事实。不得在 fallback 内生成第二个玩家实体；
6. 新增真实 Unity EditMode/PlayMode/工作场景验证：Importer/Avatar、Prefab hierarchy、Sockets、尺寸、材质、切换模块、无 T-pose/悬空/粉色材质和 D17 物理根兼容。生成正背面、T/A Pose、动作预览截图以及机器 JSON；运行 git diff --check；
7. 写 game/tools/results/D20/D20-A01-R01/D20_A01_R01_PLAYER_RIG_INTERFACE.json。它至少包含 rig_interface_ready、content_id/manifest/path、Animation Type、Avatar valid 证据、Prefab/Socket/LOD/尺寸、外观真实数量、稳定 fallback、测试结果和 A02-R01 可读边界。

通过门槛：
- rig_interface_ready=true 仅在核心人形真实为 Humanoid + valid Avatar、Prefab/尺寸/Sockets/回退测试全部通过时允许；
- 玩家工作视觉不破坏 D17 物理根、相机、出生/落地、控制与存档；
- 实际资源/模块数量、缺口和时代审查如实记录；候选/准入不被写成 LocalDevelopmentReady/ReleaseApproved；
- Unity 编译/测试、截图、机器结果和 git diff --check 真实存在。

结果必须保存到：
- 主成果：上述 R01 PlayerVisual/材质/测试、docs/game/design/common/D20_R01_PLAYER_RIG_AND_APPEARANCE.md、game/tools/results/D20/D20-A01-R01/D20_A01_R01_PLAYER_RIG_INTERFACE.json；
- 机器结果：game/tools/results/D20/D20-A01-R01/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A01-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须写明电脑独占是否实际完成、rig_interface_ready 真值、Humanoid+valid Avatar 证据、真实外观数量、fallback、测试计数和结果路径。
```
</details>

## D20-A02-R01：玩家 Animator、交互/登船动作层与 D17 回归修复

<details>
<summary>展开/复制：D20-A02-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的玩家动画表现修复 Agent。请直接完成 D20-A02-R01：只读取 D20-A01-R01 的已验证 Humanoid rig interface，建立真正的 Animator/动作表现工作层，并证明它不接管 D2/D17 的物理、输入、船舶或存档逻辑。不要改正式玩家 Prefab、InitialHarbor 或 Systems。

任务编号：D20-A02-R01（异步、玩家动作表现修复）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：D20-S01 PASS、D18-S02-R02 PASS，且 game/tools/results/D20/D20-A01-R01/D20_A01_R01_PLAYER_RIG_INTERFACE.json 存在并明确 rig_interface_ready=true。若任一条件不成立，必须 BLOCKED；只能读取 A01-R01 文件，不得回写其目录。
单一写者：game/Assets/Animations/D20/Player/R01/、game/Assets/Scripts/Presentation/D20/PlayerAnimation/R01/、game/Assets/Tests/EditMode/D20PlayerAnimationR01/、game/Assets/Tests/PlayMode/D20PlayerAnimationR01/、game/tools/results/D20/D20-A02-R01/、D20 专属文档/交接/记忆日志。不得修改 D20-A01-R01 目录、D2/D17 控制器/输入、正式生成器、InitialHarbor、D10/D11/D3/D7、Systems、Registry、存档、contracts/server/web 或 Git 历史。

电脑独占规则：本任务需要 Unity Animator/PlayMode 和真实 Player 动作验证。开始前通知用户需要独占电脑并等待允许；获得许可后不得与其它 Unity/Player Agent 并发。没有真实 Unity/Player 证据时，不能把 P0 动作层写为通过。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D20-S01 标准第 3—5/10 节、A01-R01 rig interface、D17 动作/落地/上下船/相机回归、D2CharacterAnimationController 的旧动作枚举与公开接口、D2ControlRouter、D9/D11 交互意图和现有 D6 Animator。先冻结旧枚举顺序，禁止重排 Idle=0、Walk=1、Run=2、Jump=3、Boarding=4、Disembarking=5。

执行内容：
1. 用 A01-R01 的 valid Avatar 创建 Animator Controller/Override 和只读 Presentation adapter，至少覆盖 Idle、Walk、Run、Jump、Fall、Land、Interact、Talk、Carry、Fish、Board、Disembark、Helm，并在合法 clip 存在时加入 Hit/Recover。Root Motion 默认关闭，动作层绝不直接移动玩家；
2. 适配器只读取现有速度、接地、垂直速度、控制模式、交互意图、动作意图、暂停和 LOD 状态；参数/状态名称、一次性触发消费和 fallback 必须可测试。它不得写位置、转向、钱、库存、航行、关系、任务、生态或存档；
3. 对缺 clip、动作中断、暂停、读档、Avatar 失效、目标锚点缺失、登船/下船失败建立可见但不锁死的 fallback。不得出现 T-pose、永久 transition、动画位移导致传送或与 CharacterController 双重位移；
4. 在独立工作场景和真实 Player 中完成十轮 Walk/Run、Jump/Fall/Land、Interact/Talk、Board/Disembark、Helm、暂停/恢复和读档后状态恢复。真实 Player 必须使用 D2/D17 公开输入/状态，不能仅手动设置 Animator 参数；
5. 运行 Unity 编译、Animator/EditMode、聚焦 PlayMode、D17 关键回归并保存 XML、短帧/截图、动作日志和 git diff --check。输出 D20_A02_R01_PLAYER_ANIMATION_INTERFACE.json，列出动作映射、参数、clip/Avatar、fallback、测试计数和 S02-R01 只读消费边界。

通过门槛：
- 所有 P0 动作状态可从真实 D2/D17 状态到达，旧动作枚举和物理所有权不变；
- 无 T-pose、脚滑/双重位移、永久锁死或上下船/读档回退；
- D17 平地、跳跃、落地、上下船、相机和暂停回归通过；
- Unity/Player 实测、测试 XML、截图/日志与结果 JSON 完整，未提前集成正式场景。

结果必须保存到：
- 主成果：上述 R01 Animator/Presentation/测试、docs/game/design/common/D20_R01_PLAYER_ANIMATION_LAYER.md、game/tools/results/D20/D20-A02-R01/D20_A02_R01_PLAYER_ANIMATION_INTERFACE.json；
- 机器结果：game/tools/results/D20/D20-A02-R01/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-A02-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须列出电脑独占、真实动作成功/回退次数、D17 回归计数、未完成动作、P0 结论和结果路径。
```
</details>

## D20-S02-R01：正式玩家/NPC/动物集成与真实 Player 回归验收

<details>
<summary>展开/复制：D20-S02-R01 完整提示词</summary>

```text
你是 WhalesIslandReef 的 D20 R01 正式角色表现集成与验收 Agent。请直接完成 D20-S02-R01。这是 D20 当前唯一可修改正式玩家/NPC/动物生成引用、共享 Animator 连接和 InitialHarbor 角色表现的任务；它必须在 D19-R02-S02 PASS 后，以真实 Player 验证玩家、NPC、动物、宠物、保存和性能。不能用历史隔离测试冒充正式集成。

任务编号：D20-S02-R01（同步、唯一正式角色/动物集成与验收任务）
项目根目录：C:/Users/Administrator/Desktop/game/first01
前置任务：
- D20-S01 PASS；
- D18-S02-R02 PASS，且当前 catalog/gate 的核心人形证据仍成立；
- D20-A01-R01 rig_interface_ready=true，D20-A02-R01 的 P0 动作 interface/test 通过；
- 历史 D20-A03/A04/A05/A06 的主成果、interface、测试和交接均可读；若某项 P0 interface 不可读或测试失败，本任务必须 BLOCKED；
- D19-R02-S02 PASS 是硬门禁；不得用 D19-R01-S02 PARTIAL 或历史截图替代。
并发规则：开始前确认 D19-R02-A04/R02-S02 以及其它场景写者已停止。你是 InitialHarbor 角色表现、正式生成/解析桥接、共享 Animator 连接和 D20 R01 最终验收文件的唯一写者；不得与 D21/D22 的正式集成任务并行。

电脑独占规则：本任务需要启动 Unity、真实 Main Menu/Player、保存重载和 1080p 性能/截图采集。开始前先明确告诉用户需要独占电脑并等待允许；获得允许后其它 Agent/用户不得操作 Unity、Player、鼠标键盘或同一项目。没有真实 Player 独占和重载证据时严禁 PASS。

开始前必须读取：AGENTS.md、CONTEXT、DEVELOPMENT_STANDARDS、D18-S02-R02 catalog/gate、D19-R02-S02 report/route/performance/rollback、D20-S01、历史 D20-A01—A06 的全部 interface/交接/测试、D20-A01-R01/A02-R01 的 interface/测试、D17/D2 玩家生成和回归、D10 Appearance、D11 Animal/Pet、当前正式 NPC/动物生成和 ResourceKey 解析。先保存正式场景/生成引用的 SHA 与可回滚点，列出已有稳定 entity_id/content_id/resource_key。

执行内容：
1. 通过现有稳定 resource_key/公开表现适配器把 A01-R01 玩家视觉/动作、历史 A03/A04 NPC 外观/行为表现、A05 野生/水生表现和 A06 宠物/珍稀表现串行接入正式生成入口。不得新建第二套角色/NPC/动物数据、生态刷新、外观数据、保存字段或经济/关系裁决；
2. 保持 D2_PlayerAvatar 物理根、CharacterController、出生/落地、移动、跳跃、相机、暂停、登船/下船/舵位。玩家视觉不能再是柱状/程序化主视觉；D17 fallback 仅在资源不可用时出现并有日志；
3. 在正式 InitialHarbor 通过已有生成/锚点放置或生成 12+ 可辨识 NPC、代表陆生/鸟/水生动物，以及猫狗/珍稀观察入口。保留 NPC/Animal/Pet 系统事实；动物/宠物不得穿地、漂浮、堵门/梯/舵位/登船点，远距使用已有 LOD/Culling/Proxy；
4. 验证外观切换、全套 P0 动作、NPC 对话/忙碌/拒绝反馈、岗位表现、动物地面/水体层级、宠物跟随/停留/上下船恢复、暂停/读档。表现层只能读系统快照，动画/VFX 不能发奖励或写关系/库存/任务/生态/存档；
5. 在真实 Main Menu → InitialHarbor Player 中执行完整路线、至少十轮关键玩家动作/上下船、代表 NPC/动物/宠物互动、保存退出重启继续读档；采集 1920×1080 玩家/NPC/动物固定截图矩阵、Low/Medium/High 同屏性能、LOD/Animator/Renderer 统计和错误日志；
6. 运行 D17/D19/D20 相关 EditMode/PlayMode 及必要全量核心回归。不得删除/跳过用例或用隔离副本掩盖错误；为 D18-S03 保留真实运行时消费者、Prefab、场景、碰撞、Socket、LOD、Animator、材质、截图、性能、保存/恢复证据，但不得自行晋级 LocalDevelopmentReady。

通过门槛：
- D19-R02-S02 的真实港口门禁保持 PASS，正式集成未破坏其路线、HUD、性能或保存证据；
- 玩家主视觉为真实 Humanoid/Avatar，Idle/Walk/Run/Jump/Interact/Board/Helm 无 T-pose/永久锁死，D17 回归通过；
- 12+ NPC 可辨识且可见工作/对话反馈；代表动物和猫狗不穿地/漂浮/堵关键路，LOD/Culling 有真实证据；
- 外观/动作/生成/保存不改变逻辑 ID、资源所有权、存档事实或强逻辑；
- 1920×1080 Player 截图、三档性能、Unity 测试 XML、回滚信息和失败清单齐全；资源状态仍不越过 D18-S03。

结果必须保存到：
- 主成果：正式角色/动物集成 Prefab/适配器/最小生成桥接/测试、docs/game/design/common/D20_CHARACTER_CREATURE_ACCEPTANCE_R01.md；
- 机器结果：game/tools/results/D20/D20-S02-R01/；
- 交接报告：docs/game/design/common/handoffs/D20/D20-S02-R01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

结论只能 PASS/PARTIAL/BLOCKED。聊天回复必须写明电脑独占、D19 门禁真值、真实 Player 路线/保存重载、玩家 Avatar/动作、NPC/动物/宠物数量、测试计数、性能、回滚点、未解决问题和结果路径。
```
</details>
