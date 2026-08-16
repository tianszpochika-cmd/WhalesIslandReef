# D20-S01 交接报告：角色、动物与动作表现标准冻结

## 1. 任务结论

任务编号：`D20-S01`  
日期：2026-08-14  
结论：**PASS（标准冻结）**

本结论只代表标准、接口、写者边界、测试矩阵和集成顺序已经写入并可复核。它不代表 D20-A01—A06 已完成，不代表任何候选资源为 `LocalDevelopmentReady`，也不打开 D20-S02 的正式场景写权限。

## 2. 实际修改文件

- `C:\Users\Administrator\Desktop\game\first01\docs\game\design\common\D20_CHARACTER_RIG_ANIMATION_STANDARD.md`
- `C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-S01\D20_S01_MACHINE_RESULT.json`
- `C:\Users\Administrator\Desktop\game\first01\docs\game\design\common\handoffs\D20\D20-S01_HANDOFF.md`
- `C:\Users\Administrator\Desktop\game\first01\.workbuddy\memory\2026-08-14.md`

未修改正式模型、Animator、Prefab、场景、运行时代码、Systems、AgentCore、Registry、存档、contracts、server、web 或资源状态。

## 3. 冻结的关键参数

| 项目 | 冻结值 |
|---|---|
| 单位/轴向 | `1u=1m`，Y-up，人形本地前向 `+Z`；D19 世界 `+Z` 内陆、`-Z` 海 |
| 玩家视觉高度 | `1.80m ±0.03m`，沿用 D17 基线 |
| 玩家胶囊参考 | 高 `1.80m`、半径 `0.32m`、中心 Y `0.90m`、坡度 `50°`、世界步高 ≤`0.35m` |
| 脚底/根 | 根原点位于脚底合法面，根 scale 为 `(1,1,1)`；模型位于 `Visual` 子树 |
| 相机 | 人物目标偏移 Y `1.10m`；不绑定头骨、不使用动画位移传送 |
| LOD | `0—12m/12—32m/32—80m/80m+`；玩家/英雄 LOD0 可到 `18m` |
| 1080p 同屏 | 表现实体 Low/Medium/High=`96/128/160`；NPC=`8/12/16`；动物=`12/24/32` |
| Root Motion | 默认关闭；移动、接地、跳跃、上下船、掌舵由所属系统/控制器写入 |
| 旧动作 ID | `Idle=0, Walk=1, Run=2, Jump=3, Boarding=4, Disembarking=5`，只允许追加不允许重排 |

## 4. D20-A01—A06 写者和接口

- A01：`game/Assets/Prefabs/Characters/D20/PlayerVisuals/`、`game/Assets/Art/Models/Characters/D20/Player/`；玩家 Visual/Avatar/外观槽位。
- A02：`game/Assets/Animations/D20/Player/`、`game/Assets/Scripts/Presentation/D20/PlayerAnimation/`；动作 Controller/Override/只读适配。
- A03：`game/Assets/Prefabs/Characters/D20/NPC/`、`game/Assets/Art/Models/Characters/D20/NPC/`；NPC 外观和岗位组合。
- A04：`game/Assets/Scripts/Presentation/D20/NpcPresentation/`、`game/Assets/Animations/D20/NPC/`；NPC 日程/工作/对话的只读表现。
- A05：`game/Assets/Prefabs/Animals/D20/Wildlife/`、`game/Assets/Art/Models/Animals/D20/`、`game/Assets/Animations/D20/Animals/`；野生/水生表现。
- A06：`game/Assets/Prefabs/Animals/D20/PetsRare/`、`game/Assets/Scripts/Presentation/D20/PetRarePresentation/`；猫狗/珍稀表现。

所有 Prefab 统一使用稳定 `content_id/resource_key`，并按 `Visual`、`Rig`、`Sockets`、`LOD0/1/2`、可选 `CollisionProxy` 拼装。正式场景/正式生成入口只有 D20-S02 的唯一写者可以修改。

## 5. D17 兼容要求

已保留并明确以下回归不可下降：

- D2 `CharacterController` 的 `IsGrounded`、`IsRunning`、`IsMoving`、`SpeedNormalized`、`VerticalVelocity`、`TryJump`、`SnapToGround` 和输入锁语义。
- D2 `D2CharacterAnimationController` 的六个既有动作和 `PlayTransition` 上下船过渡。
- D2 `D2ControlRouter.TryToggleShip()` 作为唯一上/下船入口；不使用动画位移跳过甲板/泊位/安全点检查。
- D17 的玩家视觉归一化、脚底接地、1.80m 高度、舵位/甲板锚点、镜头碰撞、暂停和读档恢复。

## 6. 测试与验证

已完成的本任务静态验证：

- 标准文档包含单位、比例、根节点、脚底、胶囊、相机、Humanoid/Generic、Avatar、骨骼、Root Motion、动作触发/视觉职责/回退、NPC/动物最低动作、LOD、模拟等级、变体、A01—A06 写者、测试矩阵、S02 顺序和回滚点：**PASS**。
- 当前控制器/动画/镜头接口只读核对：**PASS**，旧动作 ID 保留策略已写入。
- `git diff --check`：**PASS**。
- Unity 编译、正式 Prefab 结构、PlayMode、1080p 设备性能：本任务未运行，必须由 D20-A01—A06/D20-S02 真实执行，不能从本报告推断通过。

机器结果：

`C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-S01\D20_S01_MACHINE_RESULT.json`

## 7. 后续执行注意事项

1. A01 先锁玩家物理根和 Avatar；不能用新 Prefab 替换 `D2_PlayerAvatar` 根。
2. A02 扩展动作只能追加 ID/别名；无动作必须回退到 neutral/程序化层，不能 T-pose 或卡死。
3. A03/A04 不能用外观颜色直接裁决阵营、关系、收入、任务或法律身份。
4. A05/A06 不能从 Animator/VFX 直接改生态、捕获、奖励、好感或宠物所有权。
5. 所有远景降级只降低表现/采样频率；事实、存档、事件游标和稳定 ID 必须保留。
6. D20-S02 集成前保留 `D20-S02-preintegration` 回滚点；D18-S03 负责真实运行时消费者和 `LocalDevelopmentReady` 晋级。

## 8. 未完成事项与门禁

- D20-A01—A06 尚未由本任务实现，不能标记为完成。
- 正式角色/动物 Prefab、Animator、碰撞、锚点、运行时消费者和截图证据待后续任务。
- D17 现有正式回归需要在 A02 和 S02 实际执行；本次未以静态接口代替运行时证据。
- D18 外部资源仍受准入目录和 D18-S03 晋级门控制；本任务没有改变 `LocalDevelopmentReady=0` 或 `ReleaseApproved=0` 的真实状态。

