# D2.5 角色动作体验层范围

> 目标：在 D2 灰盒移动与航行闭环之上，补齐玩家第一人称感知到的基础动作反馈。
> 状态：已完成；不改变 D2 Systems 契约，不引入网络或 LLM。

## 1. 本阶段交付

- 待机：轻微呼吸/身体反馈，避免角色完全静止。
- 走路：WASD/方向键移动时驱动步态摆动。
- 奔跑：按住 Shift 提高移动速度并切换更快、更大幅度步态。
- 跳跃：Space 触发跳跃，加入重力、起跳和落地状态。
- 上下船：E 键或 HUD 按钮切换船舶控制时，播放上船/下船过渡姿态。
- 通过编辑器动作资源管线生成可复用的 Unity `AnimationClip` / `AnimatorController`；资源缺失时仍保留确定性的程序化动作降级。

## 2. 输入与状态

| 行为 | 输入 | 表现状态 |
|---|---|---|
| 待机 | 无移动输入 | `Idle` |
| 走路 | WASD/方向键 | `Walk` |
| 奔跑 | WASD/方向键 + Shift | `Run` |
| 跳跃 | Space | `Jump` |
| 上船 | E / HUD 按钮 | `Boarding` |
| 下船 | E / HUD 按钮 | `Disembarking` |

强逻辑仍由现有 D2 控制权和 `SailingSystem` 管理；动作控制器只读取玩家运动状态并负责视觉反馈。

## 3. 明确不纳入

- 战斗连招、受击、死亡、游泳、攀爬和复杂物理。
- 最终商店级动作捕捉、面部表情和口型同步。
- 第三方动画包或外部下载服务的自动安装。

## 4. 验收门

1. InitialHarbor 运行时存在角色动作控制器。
2. 角色模型移动时能区分待机、走路和奔跑状态。
3. Space 能触发跳跃并回到地面，不改变船舶强逻辑。
4. 上船/下船会触发对应过渡状态，且不破坏已有船舶控制。
5. Unity 编译、D2/D5 运行时冒烟和原有 EditMode 测试通过。
6. 文档与当日 memory 留痕。

## 5. 实施与验收证据（2026-08-09）

- `D2CharacterAnimationController` 已接入 `D2_PlayerAvatar`。由于 `ph_crew_male.fbx` 没有内置 AnimationClip，新增 `D6AnimationAssetInstaller` 生成 `D6CrewAnimator` 及 6 个动作资源；运行时优先使用 Animator，资源异常时降级到确定性的程序化动作层；运行时识别到 8 个可驱动骨骼。
- `D2CharacterController` 已补齐移动状态、Shift 奔跑、Space 跳跃、重力和落地；`D2ControlRouter` 已在上下船时触发 `Boarding` / `Disembarking` 过渡。
- D5 HUD 操作提示已同步：WASD 移动、Shift 奔跑、Space 跳跃、E 或按钮上下船。
- Unity 6 临时副本运行时冒烟通过：角色模型、正式 Animator、动作控制器、上下船父子关系、跳跃状态、D5 主 HUD、海图和 12 个岛标记全部通过；无 C# 异常、空引用或编译错误。
- `dotnet build game/AgentCore/AgentCore.csproj --no-restore -v:minimal` 与 `server npm run build` 通过；Unity 6 脚本编译通过；`git diff --check` 通过。

## 6. 后续升级边界

本阶段完成的是“可操作、可见反馈”的动作底座，不等同于最终动捕品质。后续替换为商店级 Humanoid 动捕资源时，只需替换 `D6CrewAnimator` 的状态 Motion，输入和 D2 航行逻辑无需重写。
