# D17-A01/A03/A04 运行基线交接

日期：2026-08-12

## 本轮结果

使用当前工作区执行 Unity 6 D5 运行时烟测，日志为 [`d17_a01_d5_smoke_current_03.log`](../../../../../../tools/results/d17_a01_d5_smoke_current_03.log)。当前本机代码级基线通过：

- 角色具有 `CharacterController` 碰撞体，并在 `BeachGround` 落地；下船后在 `GangplankGround` 落地。
- 上船、切换船舶控制权、下船、恢复人物控制均成功。
- 人物视角、船上视角、港口总览可循环切换，含镜头遮挡检测和距离恢复。
- 程序姿态层、正式 Animator 状态引用和中立姿态基线均已执行。
- 烟测最终记录 `InitialHarbor verified`，覆盖角色、碰撞/落地、上下船、跳跃、镜头和 D7 控制/表现检查。

## 运行与证据边界

这只是当前 Windows 工作区的自动化基线，不是 D17-S02 正式通过。仍缺：

- 真人手动键鼠/手柄操作和多场景输入焦点复核；
- 目标设备、低端设备、目标分辨率和长时间稳定性；
- 连续 10 次“出生→走到沙滩/码头→上下船→操船→调视角→暂停/继续→保存读取→下船→重启恢复”故事验收；
- 最终授权角色/船舶/环境资源和动画资源质量验收；
- D16-A01—A06 的外部真实证据。

Unity 批处理日志保留许可证服务和图形能力探测警告；本轮没有隐藏这些日志，也没有出现 D5 Smoke 失败或异常。后续应先按 D16 门禁处理真实证据，再把这条基线升级为 D17-S01 后的正式验收夹具。

## 交接下一步

1. 保留当前 smoke 日志作为 D17-A01/A03/A04 的本机回归基线。
2. 在 D16-S01/S02 允许启动后，冻结 D17-S01 的验收故事、截图点和设备矩阵。
3. 将真实资源、手柄输入、暂停焦点和连续 10 次恢复链路接入正式 D17-S02；没有这些证据不得标记 D17 完成。

## 2026-08-12 增量：缺失船员资源时的低模人物降级

本轮新增 `D2FallbackHumanoidBuilder`，修复旧版运行时在正式船员资源不可用时把玩家显示成单一 Capsule 柱体的问题。降级人物包含 `HumanoidVisual`、头部、躯干、双臂、双腿、脚部和 `Left/RightUpperArm`、`UpperLeg`、`LowerLeg`、`Foot` 等动作控制节点；所有子物体碰撞体关闭，根节点由 `CharacterController` 统一负责物理。

证据：

- `tools/results/d2_fallback_humanoid_unit_smoke.log`：结构烟测 PASS，`visual=True`、17 个渲染器、17 个子碰撞体全部禁用、骨骼命名检查通过；
- `tools/results/d17_a02_fallback_humanoid_playmode.xml`：Unity 6 PlayMode **1/1 PASS**，真实运行时确认 `CharacterController`、8 个动作骨骼、程序姿态层和上船过渡；加入该用例后的 D16 PlayMode 全量 `tools/results/d17_a02_full_d16_playmode.xml` 为 **35/35 PASS**；
- `tools/results/d2_fallback_humanoid_d5_smoke.log`：当前 InitialHarbor 运行烟测 `InitialHarbor verified`，程序姿态层、动作切换、中立姿态、碰撞/落地、上下船、镜头和 D7 运行链通过；
- `tools/results/d2_fallback_humanoid_compile.log`：Unity 重新导入后脚本编译通过。

边界：当前场景优先复用已有正式船员模型，因此 D5 烟测验证的是正式模型路径；低模结构由独立烟测验证。低模仍不是最终美术资产，不改变 A05 的 2,264 个 placeholder，不代表 D17-S02 或 D16-S01/S02 通过。正式角色网格、绑定动画、脚底视觉、真实手动/手柄和目标设备验收仍需后续完成。

## 2026-08-12 当前全量回归补充

当前 Unity D3D11 全量结果 `tools/results/d16_full_current_goal_20260812.xml` 为 **54/54 PASS**，其中 `D17A02FallbackHumanoidBindsRuntimeActionAndPhysics` 为 Passed。随后再次尝试单独运行该用例时，Unity Licensing Client 通道等待超时，未生成新的定向 XML；该次仅作为环境阻塞记录，不作为新的通过证据。已有全量 XML 仍保留有效性。

## 2026-08-12 D7 表现绑定竞态修复

### 根因

当前烟测发现 D7 协调器以高优先级先给 `D2_PlayerAvatar` 添加 `D7CharacterPresentationController`，而 D2 随后才添加 `CharacterController`。表现组件在 `Awake` 里把当时的缺失永久缓存为未就绪，导致 D5 烟测出现 `d7Presentation=False`，角色虽已存在但落地校准和 D7 质量状态没有完成。

### 修改

- `game/Assets/Scripts/Gameplay/D7/D7CharacterPresentationController.cs`：增加可重复的运行时绑定；若 `CharacterController` 后挂载，下一次 `LateUpdate` 会重新绑定并完成视觉校准。
- `game/Assets/Tests/PlayMode/D17FallbackHumanoidPlayModeTests.cs`：新增 `D17A01PresentationBindsAfterCharacterControllerIsAdded`，复现真实生命周期顺序并验证最终 `IsReady`、人物视觉判定。

### 当前证据

- `tools/results/d17_a01_presentation_lifecycle_current_goal_20260812.xml`：专项 **1/1 PASS**；日志确认 `CharacterController=True, humanVisual=True`。
- `tools/results/d17_a01_d5_smoke_after_presentation_fix_20260812.log`：D5 烟测记录 D7 表现绑定、视觉落地校准 `error=0.000`、`D7 Smoke Quality check`，并最终输出 `InitialHarbor verified`；无 `runtime objects not ready`。

### 边界

该修复关闭本机运行时生命周期竞态，不等于 D17 正式验收、最终美术、真机/目标设备或 D16 外部门禁通过；不修改 Systems、存档、contracts、AssetRegistry 或 Steam 状态。

### 全量回归补充

- tools/results/d16_full_after_d7_presentation_fix_20260812.xml：Unity 6 PlayMode 55/55 PASS，0 failed / 0 inconclusive / 0 skipped；
- 全量日志 tools/results/d16_full_after_d7_presentation_fix_20260812.log：包含新增 D17 生命周期回归用例，并多次确认 CharacterController=True、humanVisual=True、visualGroundError=0.000；
- 关键工程编译：AgentCore、Whalebone.Systems、Save.Tests、Assembly-CSharp、D16.PlayMode.Tests、D16A02Runner、D16A03Runner 均为 0 warning / 0 error。

## 2026-08-12 D17-A03 安全上下船增量

- D2ControlRouter 上船优先使用 D7_HelmPoint；下船前从船周围检测明确的沙滩、码头或跳板碰撞面，未找到安全地面时拒绝下船并保持 Ship 控制权。
- tools/results/d17_a03_boarding_safe_surface_current_20260812.xml：上船/航行/安全下船闭环 1/1 PASS；
- tools/results/d17_a03_safe_disembark_refusal_current_20260812.xml：无行走面拒绝下船 1/1 PASS；
- tools/results/d16_full_after_safe_disembark_20260812.xml：全量 PlayMode 56/56 PASS。

该增量只收口本机上下船失败恢复，不宣称目标设备、最终资源、手动操作或 D17-S02 通过。

## 2026-08-12 D17-A04 手柄镜头与模态输入锁

- CameraFollow 新增 Gamepad 右摇杆镜头控制，保留鼠标右键/中键旋转和滚轮缩放；镜头输入继续由 D7ControlCoordinator 与 D16InputRouter 的模态锁统一裁决。
- D17CameraInputPlayModeTests 的 D17A04InitialHarborGamepadRightStickRotatesCamera 专项：1/1 PASS；测试同时验证锁定时 yaw 不变、解锁后 yaw 与实际相机方向改变。
- tools/results/d17_a04_full_after_gamepad_camera_20260812.xml：Unity 6 PlayMode 全量 57/57 PASS。
- tools/results/d17_a04_d5_smoke_after_gamepad_camera_20260812.log：InitialHarbor verified；Camera、物理落地、D7 控制和上下船烟测未回归。

边界：这是合成输入设备和本机 Unity 运行时证据；真实手柄、目标设备/分辨率、人工菜单焦点和连续 10 次 D17-S02 仍未验收。
