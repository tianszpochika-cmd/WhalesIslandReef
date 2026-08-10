# D6.2 角色动画姿态基线

> 日期：2026-08-09
> 状态：技术验收通过

## 1. 用户问题

D6.1 解决了角色的重力、落地和碰撞，但角色仍可能出现腿部方向异常、动作切换后模型姿态被覆盖的问题。排查发现，D6 的占位船员 FBX 没有内置动作片段，项目生成的动作属于“局部骨骼动画”，必须保留模型导入时的原始骨骼旋转。

## 2. 根因

旧动作资源只对骨骼写入 `localEulerAnglesRaw.x`，且 Animator 状态启用了 Write Defaults。状态切换时，未设关键帧的 y/z 旋转和其他默认姿态可能被 Unity Animator 覆盖，造成模型看起来像腿朝上、身体姿态不连续或动作切换后残留倾斜。

## 3. 修复内容

- `D6AnimationAssetInstaller` 为动作骨骼同时生成 x/y/z 三轴曲线，y/z 保留 FBX 原始姿态；
- 视觉根节点在每个动作中都写入完整旋转基线，避免奔跑倾斜残留到待机或上下船；
- 关闭六个 Animator 状态的 Write Defaults，局部动作不再重置未参与动作的骨骼；
- `D2CharacterAnimationController` 首次进入待机后采集中立姿态下肢偏差；
- `D5RuntimeSmokeProbe` 增加姿态回归门禁：中立姿态下肢最大偏差必须不超过 8°。

## 4. 验收证据

隔离 Unity 6 副本重建资源成功：

- `D6CrewAnimator.controller` 与六个动作片段重新生成；
- 六个动作片段均包含 `localEulerAnglesRaw.x/y/z` 曲线；
- Animator 六个状态的 `m_WriteDefaultValues` 均为 `0`；
- Unity 运行时冒烟退出码 `0`；
- `[D2/Animation] 中立姿态基线已采样：下肢最大偏差=4.3°`；
- `[D5 Smoke] Pose check: neutralSampled=True, lowerBodyError=4.3°`；
- `[D5 Smoke] InitialHarbor verified` 通过。

## 5. 范围边界

本任务只保证占位角色的姿态基线和动作状态切换稳定，不等同于最终动捕质量。后续接入正式 Humanoid 动捕资源时，可替换 `D6` Animator Motion，D2 输入、上下船和物理接口无需改变。
