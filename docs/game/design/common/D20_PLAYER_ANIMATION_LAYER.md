# D20-A02-R01 玩家动画层

任务：D20-A02-R01  
日期：2026-08-15  
结论：**PASS**

## 1. 门禁和边界

A01 只读接口已经真实发布 `rig_interface_ready=true`，并指向一个有效的 `Humanoid + Avatar` 工作 Prefab。本任务只建立 D20 动画工作层：Animator Controller、项目自有动作 Clip、只读状态映射和表现适配器。没有修改 A01 Prefab、D17 CharacterController、D2 输入/船控、InitialHarbor、Systems、Registry、存档、contracts、正式玩家生成入口或资源状态。

历史 D20-A02 的 BLOCKED 预检仍保留在 `game/tools/results/D20/D20-A02/D20_A02_PREFLIGHT_RESULT.json`，历史设计说明已复制到 `docs/game/design/common/D20_PLAYER_ANIMATION_LAYER_HISTORICAL_BLOCKED.md`；本 R01 结论建立在新 Avatar 证据上，不覆盖旧失败事实。

## 2. 工作层资产

| 资产 | 路径 | 说明 |
|---|---|---|
| Animator Controller | `game/Assets/Animations/D20/Player/D20_PlayerAnimation.controller` | 15 个稳定状态，Root Motion 关闭 |
| Animation Clips | `game/Assets/Animations/D20/Player/Clips/D20_Player_*.anim` | 15 个项目自有确定性曲线 Clip；不冒充外部动作包 |
| 只读适配器 | `game/Assets/Scripts/Presentation/D20/PlayerAnimation/D20PlayerAnimationPresentationAdapter.cs` | 只读快照、状态、参数、暂停和 LOD |
| 运行时程序集 | `game/Assets/Scripts/Presentation/D20/PlayerAnimation/WhalesIslandReef.D20.PlayerAnimation.Runtime.asmdef` | 纯表现层，无网络/LLM/强逻辑依赖 |

D18 的合法动作候选只证明了源文件/片段导入，未证明 Avatar/Animator 消费，因此本轮没有把这些 Generic 动作片段直接重定向到玩家。15 个 Clip 是项目自有骨架曲线工作资产，覆盖可测试状态并作为缺少正式动作资源时的确定性 fallback。

## 3. 状态和只读输入

| 状态 | 触发事实 | 回退 |
|---|---|---|
| Idle | 接地且速度 ≤ 0.05 | 中立站姿 |
| Walk / Run | 接地速度 0.05—0.65 / ≥ 0.65 | Idle / Walk |
| Jump / Fall | 未接地，垂直速度 ≥ 0.1 / < 0.1 | Idle/Walk，不移动根 |
| Land | 接地边沿 `false → true` | Idle/Walk |
| Interact / Talk | 已接受的交互类型或对话表现意图 | Idle + 系统反馈 |
| Carry / Fish | 搬运或钓鱼事实/意图已被所属系统接受 | Walk/Interact |
| Board / Disembark | `D2ControlRouter` 已裁决上/下船过渡 | Idle/不可达反馈 |
| Helm | `control_mode=Ship` 且船舵事实可表现 | ShipDeck Idle |
| Hit / Recover | 伤害/恢复表现快照 | Injured/Idle |

`D20PlayerAnimationSnapshot` 只含 `grounded`、`speed01`、`verticalSpeed`、`controlMode`、`interactionKind`、`actionIntent`、伤害/恢复、暂停和 LOD。适配器只写 Animator 参数、状态和 culling；不写 `Transform.position`、库存、金钱、航行、任务、关系、奖励或存档。Root Motion 固定为 false，D17 CharacterController 仍是唯一物理位移写者。

Controller 参数为：`Speed01`、`VerticalSpeed`、`Grounded`、`InteractionActive`、`ActionHash`、`Paused`。没有使用 Animation Event 结算事实；一次性状态由系统快照边沿驱动。

## 4. 暂停、LOD 和缺动作策略

- `paused=true` 时只将 Animator speed 置零，不冻结或修改 gameplay 根。
- LOD 0/1 使用 `CullUpdateTransforms`，LOD 2 使用 `CullCompletely`，LOD 3 禁用 Animator；所有档位都不改变系统事实。
- Controller 缺状态或工作环境没有 Animator 时，适配器保留请求状态并标记 `LastApplyUsedFallback=true`；有 Animator 但没有目标状态时回到 Idle。这样既不会永久锁在 T-Pose，也不会通过表现层伪造动作事实。
- 工作层预览使用临时 Animator Host，不保存到 A01 Prefab；不等同正式 Player 或 LocalDevelopmentReady。

## 5. 真实验证

- Unity 6000.0.80f1 重新编译：`game/tools/results/D20/D20-A02-R01/D20_A02_R01_COMPILE_FINAL.log`，batchmode exit 0。
- Controller/Clip 构建审计：`D20_A02_R01_CONTROLLER_AUDIT.json`，15/15 状态、15/15 Clip、每个 Clip 5 条曲线、6 个参数、Root Motion=false。
- EditMode：`D20_A02_R01_EDITMODE_TESTS_FINAL3.xml`，5 total / 5 passed / 0 failed / 0 skipped。
- PlayMode：`D20_A02_R01_PLAYMODE_TESTS_FINAL.xml`，1 total / 1 passed / 0 failed / 0 skipped；10 个只读动作快照均未移动物理根。
- D17 回归：`D20_A02_R01_D17_PLAYMODE.xml`，7 total / 7 passed / 0 failed / 0 skipped，覆盖平地移动、跳跃、落地、上下船和动作输入回归。
- 工作层截图：`game/tools/results/D20/D20-A02-R01/previews/`，Idle/Walk/Board/Fish 4 张，512×768；这是工作层 Camera.Render 预览，不是正式 Player 验收。
- 早期失败结果未删除：`D20_A02_R01_EDITMODE_TESTS.xml`、`D20_A02_R01_PLAYMODE_TESTS.xml`、`D20_A02_R01_PREVIEWS.log` 与预览 V2—V3 日志均保留。
- `git diff --check` 在交付时执行并通过；现有行尾转换提示不计为 whitespace error。

## 6. 后续门禁

本任务 PASS 仅表示 D20 动作工作层和 D17 回归可复核。A01 仍因第二身体/发型/时代服装不足为 PARTIAL；A02 没有正式场景消费者，`LocalDevelopmentReady=0`、`ReleaseApproved=0` 不变。D18-S03 必须在真实消费者、保存/恢复、交互、碰撞、截图和回退证据齐全后决定资源晋级；D20-S02-R01 仍必须等待 D19-R02-S02 PASS，且只有正式集成任务才能连接玩家/NPC/动物到 InitialHarbor。
