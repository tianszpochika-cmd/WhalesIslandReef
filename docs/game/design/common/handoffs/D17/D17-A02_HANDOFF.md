# D17-A02 异步表现准备交接

日期：2026-08-12

## 状态

**代码级准备已完成；正式验收未开始。** D17-A02 只作为 D16 收口期间允许的异步表现准备，不改变 D17-S01/S02 的启动门禁。

## 本轮交付

- `game/Assets/Scripts/Gameplay/D2/D2FallbackHumanoidBuilder.cs`：正式船员资源不可用时，生成带头部、躯干、双臂、双腿、脚部和明确动作节点的可识别低模人物；不再把玩家显示成单一 Capsule 柱体。
- `game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs`：沿用动作状态接口，提供待机、走、跑、跳和上下船的程序姿态降级层；正式 Animator 可用时仍优先接入。
- `game/Assets/Scripts/Gameplay/D2/D2RuntimeBootstrap.cs`：正式 `ph_crew_male` 优先，缺失时才创建降级人物；根节点由 `CharacterController` 负责碰撞、重力和落地，子表现碰撞体关闭。

## 验证

- `tools/results/d2_fallback_humanoid_unit_smoke.log`：结构烟测通过；17 个渲染器、17 个子碰撞体和骨骼命名检查通过。
- `tools/results/d17_a02_fallback_humanoid_playmode.xml`：定向 PlayMode **1/1 PASS**。
- `tools/results/d16_full_current_goal_20260812.xml`：全量 PlayMode **54/54 PASS**，其中 `D17A02FallbackHumanoidBindsRuntimeActionAndPhysics` 为 Passed。
- 本轮再次发起定向 Unity 批处理时，因 Licensing Client 通道超时未生成新的 XML；该尝试记为环境阻塞，不计入通过，也不覆盖已有有效证据。

## 数据所有权与边界

- 只修改 D2 表现/运行时接线；不修改 Systems、AgentCore、contracts、存档裁决或内容 Registry 状态。
- 不把 2,264 个 A05 placeholder 改成 final，不伪造来源/授权，不宣称最终美术或绑定动画完成。
- 当前 InitialHarbor 正常路径仍优先复用场景内 `ph_crew_male`；低模路径的视觉脚底、正式动画质量、手动/手柄操作和目标设备表现仍未验收。

## 2026-08-12 运行时表现绑定竞态复验

发现 D7 协调器可能先于 D2 物理组件挂载表现脚本；旧实现只在 Awake 读取一次 CharacterController，会把短暂缺失错误保留为未就绪。现已在 D7CharacterPresentationController 增加延迟绑定，并新增 D17 回归用例覆盖该挂载顺序。

- tools/results/d17_a01_presentation_lifecycle_current_goal_20260812.xml：定向 PlayMode 1/1 PASS。
- tools/results/d17_a01_d5_smoke_after_presentation_fix_20260812.log：D5 运行时烟测确认 CharacterController=True、humanVisual=True、grounded=True、视觉落地误差 0.000，并完成 InitialHarbor verified。
- 该修复只属于本机异步基线，不把低模人物、程序动作或本机烟测升级为最终资源/正式 D17 验收；D16 真实门禁与 D17-S01/S02 仍保持未通过状态。

全量回归补充：tools/results/d16_full_after_d7_presentation_fix_20260812.xml 为 Unity 6 PlayMode 55/55 PASS，0 failed / 0 inconclusive / 0 skipped；这只说明该接线修复没有破坏现有本机回归，不改变 D16/D17 的总体门禁状态。

## 2026-08-12 动作状态链回归补充

为避免“玩家能移动但动作没有反应”只靠人工观察，本轮新增
`game/Assets/Tests/PlayMode/D17ActionQualityPlayModeTests.cs`。测试从真实 `InitialHarbor`
运行时读取玩家动作状态，覆盖：

`Idle → Walk → Run → Jump → Boarding → Disembarking`

本机 Unity 6 证据：

- `tools/results/d17_a02_action_quality_current_20260812.xml`：专项 **1/1 PASS**；
- `tools/results/d17_full_after_action_quality_20260812.xml`：PlayMode 全量 **59/59 PASS**，0 failed / 0 inconclusive / 0 skipped；
- `game/Assets/Scripts/Gameplay/D2/D2CharacterAnimationController.cs`：在物理组件尚未挂载的生命周期间隙保持 `Idle`，不把临时绑定缺口误判为 `Jump`。

该专项确认程序姿态层、Animator 状态名、角色物理动作状态和上下船过渡已经在本机代码链路中一致；它不等于真人最终动画资源、动作捕捉质量、目标设备帧率或 D17-S02 通过。

## 下游

D16-A01—A06 全部真实门禁通过并完成 D16-S01/S02 后，才能进入 D17-S01 冻结；随后 D17-A02 才能与已授权最终角色资源串行接入 D17-S02。当前用户反馈中的腿部姿态、漂浮、真实碰撞和走到沙滩问题，仍需在真人垂直切片中用目标设备和连续故事复核。
