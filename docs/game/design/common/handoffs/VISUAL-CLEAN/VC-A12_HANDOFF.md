# VC-A12 角色稳定子任务交接

## 结论

VC-A12 在限定范围内完成。`D2CharacterAnimationController` 现在把 Animator、程序化姿态和 neutral 回退建模为互斥驱动策略：完整可信 Rig 由 Animator 独占，缺少可信骨骼或存在左右匹配冲突时稳定回退，程序化姿态只在完整且唯一的 8 骨骼集合上运行。

本批次没有真实 standalone Player 的连续移动视觉证据，因此本子任务标记为 `PARTIAL`；不升级整体 D31，现有 D31 结果仍为 `PARTIAL/R74`。

## 根因与处理

1. 原程序化 Pose 同时修改 `visualRoot.localPosition/localRotation` 与四肢骨骼，而 D6 Animator clip 也包含 `Human Armature` 的根曲线，存在同帧覆盖、竖向漂移和侧摆叠加风险。现已移除程序化 bob/lean，只把视觉根锁回导入基线；实体根的位置和朝向不由动作控制器写入，Animator 的 `applyRootMotion` 固定为 `false`。
2. 原启发式匹配可能把 `LeftShoulder` 当作上臂；低模的 `LeftUpperArmMesh` 等渲染节点又会与骨骼产生同分候选。现已按角色侧与骨骼角色排序，排除 Renderer/mesh/geometry/skin 节点，使用 `HashSet<Transform>` 防止重复绑定；同分候选直接判冲突，不猜测驱动。
3. 正式 Animator 只有在六个 D2 动作状态都存在、角色 Rig 完整且自身启用时接管；其他 Animator 会关闭。骨骼不完整或左右冲突时进入 neutral，Animator 状态不完整但 Rig 可信时才进入单一程序化层。

## 实际修改

- `game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs`
  - 新增互斥 `Neutral/Animator/Procedural` 驱动状态和观测属性。
  - 移除程序化根部位移/倾斜，锁定视觉根基线，不触碰实体根。
  - 强化 D6 状态检查、根运动关闭、其他 Animator 关闭和稳定降级。
  - 强化骨骼侧别名、角色优先级、冲突拒绝、唯一绑定和视觉节点过滤。
  - 保持 `Idle/Walk/Run/Jump/Boarding/Disembarking` 枚举与 `PlayTransition` 接口兼容。
- `game/Assets/Tests/EditMode/D2/D2CharacterAnimationControllerTests.cs`
  - 覆盖实体根/视觉根基线、Animator 根运动配置、左右别名误配、重复候选和 Renderer 伪骨骼。
- `tools/tests/test_d2_character_animation_controller.py`
  - 对根写入红线、驱动互斥、匹配冲突与动作接口做源码边界审计。
- `game/tools/results/VISUAL-CLEAN/VC-A12/VC_A12_RESULT.json`
- `.workbuddy/memory/2026-09-04.md`

## 验证证据

- `python -m unittest tools/tests/test_d2_character_animation_controller.py`：4/4 PASS。
- Unity 6 `6000.0.80f1` EditMode：`game/tools/results/VISUAL-CLEAN/VC-A12/VC_A12_EDITMODE_R6.xml`，5/5 PASS；对应日志 `VC_A12_EDITMODE_R6.log` 显示 Tundra build success、无 `error CS`。日志同时记录冲突样例为 7 个唯一绑定、`冲突=True`、程序化能力关闭；完整样例为 8 个唯一绑定、Animator 独占。
- 既有 D17 低模 PlayMode：`game/tools/results/VISUAL-CLEAN/VC-A12/VC_A12_D17_FALLBACK_PLAYMODE_R3.xml`，1/1 PASS；运行日志确认低模绑定 8 骨骼、Animator 独占、neutral 基线采样。
- 既有 InitialHarbor 动作状态 PlayMode：`game/tools/results/VISUAL-CLEAN/VC-A12/VC_A12_D17_ACTION_QUALITY_PLAYMODE_R2.xml`，1/1 PASS；日志确认场景玩家绑定成功、W 输入进入 `Walk`，并完成 Jump/Boarding/Disembarking 接口回归。

## 未验证与下一步

- 尚未在真实 standalone Player 中采集 Walk/Run 左右转、停步的连续帧；因此不能声称已经用视觉证据证明“无横向蛇形摆动”。
- 尚未对真实 Player 做最终脚底接触和画面质量人工验收，也没有变更 InitialHarbor 场景、港口生成器、Prefab、D29/UI、D2RuntimeBootstrap 或 Systems/AgentCore。
- 下一步在独立 Player 中记录实体根世界 position/rotation、visualRoot 局部 position/rotation 和脚底接触点，再由人工视觉检查确认。若出现残余侧摆，只继续在本控制器的动作接管/回退策略内处理。

## D31 状态

未修改 `game/tools/results/D31/D31-S05/D31_S05_RESULT.json`，也未升级 D31；以该文件当前的 `PARTIAL/R74` 为准。
