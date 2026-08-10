# D6.4 相机遮挡保护

> 日期：2026-08-09  
> 状态：技术实现与运行时冒烟通过

## 1. 背景与目标

D6.3 增加了人物视角、船上视角和港口总览，但镜头在码头、船体或场景道具附近仍可能穿过障碍物。D6.4 只补齐相机表现层的遮挡保护，不改变人物、船舶或航行 Systems 的裁决逻辑。

目标是：

- 镜头从跟随目标向期望位置移动前，先检测中间是否有港口/船体/道具碰撞体；
- 发现遮挡时，把镜头安全距离收缩到障碍物前，并保留最小观察距离；
- 障碍物离开后平滑恢复原视角距离，避免镜头瞬移；
- 三种视角和右键/中键拖动、滚轮缩放继续共用同一套保护逻辑。

## 2. 实现

`CameraFollow` 增加：

- 以跟随目标的 pivot 为起点、期望镜头方向为终点的球体投射检测；
- `cameraCollisionRadius` 和 `cameraCollisionPadding`，为镜头保留安全间隙；
- 排除当前跟随目标及其子级碰撞体，避免人物/船体自身把镜头误判为障碍；
- `SmoothDamp` 平滑收缩/恢复碰撞距离；
- `IsObstructed`、`ResolvedDistance`、`CollisionCheckCount` 运行时诊断值。

D5 运行时冒烟新增相机保护门禁：要求碰撞检测实际执行，解析距离为有限值，并与三种视角循环一起通过。

## 3. 验收证据

- Unity 6 D5 运行时冒烟退出码：`0`；
- 相机日志：`modes=3, cycle=True, collisionChecks=4, obstructed=True, distance=6.01, restored=人物视角`；
- `InitialHarbor verified` 通过，动作质量、姿态、物理落地、上下船和既有 HUD 检查未回归；
- `dotnet build game/game.sln --no-restore -v:minimal`：0 警告、0 错误；
- AgentCore 独立构建：通过；
- `git diff --check`：通过。

## 4. 范围边界

当前是轻量通用的 SphereCast 遮挡保护，不是最终 Cinemachine 镜头系统，也不替代后续的室内镜头、复杂地形镜头策略或美术级镜头调参。Unity 批处理验证中出现的 Licensing/CDN 超时属于编辑器环境噪声；本次游戏运行时冒烟仍正常退出并通过全部门禁。
